# Code Review — Issue #638 (EFC unguarded archive-root read)

- **Branch:** `bug/efc-unguarded-archive-root-read-crashes-ui-thread-638`
- **Base:** `ecdb1c84ba8541ab67042985919cfed4df768c01`
- **Head:** `af1b36e2d93c6beeeb98bbe4998d752e1ebfd732`
- **Review date:** 2026-08-29T13-06
- **Scope:** full branch diff against the base — 3 source files and 35 feature-folder documents

## Verdict

**PASS. 0 blocking findings.** Seven non-blocking findings are recorded below: one Minor design
observation, one Minor test-robustness finding, one Minor coverage-value finding, and four Trivial
or informational items.

The fix is close to the smallest change that resolves the defect. It reuses two shapes that already
exist in this codebase — the `return false` degrade that the adjacent OneDrive guard uses, and the
injectable `Action` seam that `EfcHomeController.MoveFailureMessageAction` uses — rather than
inventing new ones. The catch is correctly narrowed by type, the guard ordering preserves both
load-bearing invariants, and the user-facing text is redacted.

## Findings

| ID | Severity | Blocking | Area | Summary |
|---|---|---|---|---|
| CR-1 | Minor | No | Test robustness | `MoveToFolderAsync_WhenArchiveRootResolves_StillReadsItOnce` depends on a `NullReferenceException` thrown by an unrelated collaborator as its stopping barrier |
| CR-2 | Minor | No | Test value | The two `InvalidOperationException` tests differ only by message text, and production dispatches by type, so the second exercises no additional production path |
| CR-3 | Minor | No | Layering | The diagnostic seam's default delegate binds `EfcDataModel` to `MessageBox`, deepening a pre-existing UI coupling in a data-model class |
| CR-4 | Trivial | No | Organization | `TryGetArchiveRoot` and `ArchiveRootUnavailableMessage` are declared inside `#region Public Properties` |
| CR-5 | Trivial | No | Duplication | `UserDiagnosticAction(ArchiveRootUnavailableMessage)` is written out at both `Open*` call sites |
| CR-6 | Informational | No | Test comment | The two rule-text constants are described as verbatim copies of another assembly's internals, implying a coupling that is not actually load-bearing |
| CR-7 | Informational | No | Residual defect | Five equivalent unguarded reads remain in `EfcFormController`, one of them on a path with no local catch |

### CR-1 — Success-path test uses an incidental crash as its barrier (Minor, non-blocking)

`QuickFiler.Test/Controllers/EfcDataModelArchiveRootTests.cs:172-186` arranges a resolving archive
root, then asserts:

```csharp
await act.Should().ThrowAsync<NullReferenceException>();
olObjects.VerifyGet(value => value.ArchiveRootPath, Times.Once());
```

The `VerifyGet` is the assertion that matters — it is what pins AC7's single-read invariant. The
`ThrowAsync<NullReferenceException>()` above it is not a property of the unit under test; it is the
`EmailFiler` collaborator dereferencing a `MailItemHelper` whose folder information is null, several
frames past the code this change touches. The fixture's XML doc is honest about this and explains
precisely why the arrangement is shaped that way, which is better than most tests of this kind. The
concern is durability rather than correctness: if `EmailFiler.SortAsync` later gains a null guard and
returns `false` instead of throwing, this test fails with a message about an expected
`NullReferenceException`, which points a future maintainer at the wrong subsystem entirely.

The test is also the only one that reaches line 339 (`OlAncestor = olAncestor,` on the move path), so
losing it would drop changed-line coverage from 93.10 to roughly 89.7 percent.

Suggested direction, for a follow-up rather than for this change: introduce a filer-construction seam
on `EfcDataModel` so the success path can terminate deliberately, and then assert only the
`VerifyGet`. That is a larger change than the Bugfix Workflow permits here.

### CR-2 — The second throw-condition test adds coverage without discriminating power (Minor, non-blocking)

`MoveToFolderAsync_WhenArchiveRootIsUnresolvable_ReturnsFalseInsteadOfThrowing` and
`MoveToFolderAsync_WhenArchiveRootIsCrossStoreUnresolvable_ReturnsFalseInsteadOfThrowing` are
identical except for the message string carried by the injected `InvalidOperationException`
(`UnresolvableRuleText` versus `CrossStoreRuleText`). `TryGetArchiveRoot` dispatches on exception
*type* and never inspects the message, so both tests drive exactly the same production statements. A
defect that broke one would break both; neither can fail while the other passes.

This is not a defect in the change. `spec.md` AC9 asks that both documented throw conditions be
exercised, and at the `IOlObjects.ArchiveRootPath` seam the only observable difference between the
two conditions *is* the message. A stronger test is not constructible without moving the assertion up
into `TaskMaster/AppGlobals/ArchiveRootPathGuard.cs`, where
`TaskMaster.Test/AppGlobals/AppOlObjectsArchiveRootValidationTests.cs` already pins both conditions
and which this change correctly leaves untouched. The finding is recorded so that the AC9 evidence is
understood for what it is: documentation that the guard is message-agnostic, not an independent
second proof.

### CR-3 — The seam default couples the data model to `MessageBox` (Minor, non-blocking)

`QuickFiler/Controllers/EfcDataModel.cs:154`:

```csharp
internal Action<string> UserDiagnosticAction { get; set; } = text => MessageBox.Show(text);
```

The General Code Change Policy's fourth design principle asks that pure logic be kept separate from
UI. A data-model class that owns a default `MessageBox.Show` is on the wrong side of that line.

Three facts make this an accepted trade-off rather than a defect, and they are worth recording so the
decision is not silently re-litigated:

1. It is not new coupling. `System.Windows.Forms` was already imported at
   `QuickFiler/Controllers/EfcDataModel.cs:10` and `MessageBox.Show` is already called at `:417`, in
   the pre-existing `MAPIFolder` overload of `MoveToFolderAsync`. The change matches existing style,
   as § 7.1 of the policy requires.
2. The alternative was evaluated and rejected on stronger grounds. `spec.md` § Backward-compatibility
   records that widening `OpenOlFolderAsync` and `OpenFsFolderAsync` to `Task<bool>` would leave all
   five production call sites discarding the value, converting one silent-swallow site into five.
3. The seam makes the coupling testable and overridable, which is exactly what the class previously
   lacked at `:417` — that call site is still a hard-coded modal dialog and remains uncoverable.

A future cleanup could hoist the reporting responsibility into `EfcHomeController`, which already owns
`MoveFailureMessageAction`, and let `EfcDataModel` stay UI-free. That belongs with follow-up issue
#697, which covers the reporting surface at the boundaries.

### CR-4 — Private members declared under a "Public Properties" region (Trivial, non-blocking)

`ArchiveRootUnavailableMessage` (`:264-268`) and `TryGetArchiveRoot` (`:270-296`) sit inside the
`#region Public Properties` block that closes at `:299`. One is a private constant and the other is a
private method, so neither belongs there. `UserDiagnosticAction` at `:154` is a property and is
`internal`, so its placement is defensible.

The file already carries a `#region Public Methods` and a `#region Constructors and Initializers`, so
the convention exists to follow. Moving the two members to a private-helpers region would cost three
lines and would not affect any gate. Suggested for a later touch of this file, not for this change.

### CR-5 — Diagnostic invocation duplicated across the two `Open*` call sites (Trivial, non-blocking)

```csharp
if (!TryGetArchiveRoot(out var olAncestor))
{
    UserDiagnosticAction(ArchiveRootUnavailableMessage);
    return;
}
```

appears verbatim at `:356-360` and `:380-384`. Two occurrences is at the low end of what the
"avoid copy-paste" rule targets, and keeping the invocation at the call site rather than inside
`TryGetArchiveRoot` is a defensible choice: it keeps the helper free of UI concerns, which is why
`MoveToFolderAsync` can use the same helper without raising a dialog. Recorded as an observation, not
a change request.

One asymmetry is worth noting for readability: `OpenOlFolderAsync` separates its two guards with a
blank line (`:354-355`) while `OpenFsFolderAsync` does not (`:379-380`). CSharpier does not normalize
this, so it is a hand-formatting inconsistency between two adjacent, otherwise identical methods.

### CR-6 — The copied rule-text constants imply a coupling that is not load-bearing (Informational)

`QuickFiler.Test/Controllers/EfcDataModelArchiveRootTests.cs:25-32` carries a comment stating that
`UnresolvableRuleText` and `CrossStoreRuleText` are "verbatim copies" of the constants in
`TaskMaster/AppGlobals/ArchiveRootPathGuard.cs`. That reads as a maintenance obligation, but no
assertion in the file compares against the real constants — the two strings are only payloads for
`new InvalidOperationException(...)`, and production never inspects them. If the real constants change
the copies will drift and every test will keep passing, correctly, because nothing depends on the
equality. Rewording the comment to say the strings are *representative of* the guard's messages rather
than copies of them would prevent a future maintainer from believing a sync obligation exists.

The redaction test, `ArchiveRootFailureDiagnostic_DoesNotContainTheArchivePathOrMailboxAddress`, does
not depend on these constants either: it asserts over the *seam's* captured message, which is
`ArchiveRootUnavailableMessage`, against the `mailbox@example.com` and archive-path literals declared
locally at `:34-36`. That is the right target — it verifies the change's own output rather than the
upstream guard's.

### CR-7 — Equivalent unguarded reads remain in `EfcFormController` (Informational, filed as #698)

Confirmed present at head and correctly excluded from this change's scope:
`QuickFiler/Controllers/EfcFormController.cs:529`, `:539`, `:836`, `:846` and `:987` all read
`_globals.Ol.ArchiveRootPath` without a guard. The `:836` and `:846` pair sits in `CreateFolderAsync`,
which has no local `try`/`catch`; that path is reached from the keyboard `'N'` binding and terminates
in `KeyboardHandler`'s log-only catch, so the same silent-swallow symptom this issue fixed in
`EfcDataModel` is still reachable there.

This is exactly non-goal (c) in `spec.md` § Scope & Non-Goals and is filed as issue #698 with all five
citations re-derived in `evidence/other/p8-t2-followup-issue-dossier.md`. It is recorded here only so a
reader of the diff does not mistake the remaining reads for an incomplete fix. The `EfcDataModel`
helper the dossier proposes those five sites adopt is the one this change introduces.

## What the change does well

- **The `try` placement problem is solved cleanly.** All three reads sit syntactically inside
  `EmailFilerConfig` object initializers, where a `try` cannot be written without restructuring. The
  `bool TryGetArchiveRoot(out string)` shape lifts the read out of the initializer, keeps the
  initializer a single assignment from a local, and reads naturally next to the existing
  `Globals.FS.SpecialFolders.TryGetValue(...)` guard directly above it.
- **Guard ordering is correct in all three methods and pinned from both sides.** The `MailInfo is
  null` check remains first in `MoveToFolderAsync` (`:311-314`); the OneDrive `SpecialFolders` read is
  second (`:321-325`); the archive-root guard is third (`:327-330`). Both `Open*` methods place the
  new guard after their OneDrive guard. The ordering is asserted from the production side by three
  `VerifyGet(..., Times.Never())` tests and from the untouched side by
  `EfcHomeControllerLifecycleTests.cs:217`'s `SpecialFoldersAccessCount.Should().Be(2)`, which still
  passes with that file unmodified. Reversing the order would have broken that assertion two ways at
  once — the count would drop to 0, and the probe's null `Ol` would raise a `NullReferenceException`
  the narrow catch cannot absorb.
- **The catch is narrow by type and the narrowness is pinned by a test.**
  `catch (InvalidOperationException ex)` at `:287`, with
  `MoveToFolderAsync_WhenArchiveRootThrowsComException_StillPropagates` asserting that a
  `COMException` from the same getter is not absorbed. That keeps follow-up issue #696 visible rather
  than silently swallowing it, and it satisfies the fail-fast rule.
- **No degradation to an empty root.** The failure branch returns before `new EmailFiler(config)` at
  `:343`, `:370` and `:394`, so no partially populated `EmailFilerConfig` is constructed and
  `OlAncestor` is never assigned an empty or synthesized value. That avoids reintroducing the #614
  store-root-leak failure mode.
- **Redaction is preserved in both channels.** `ArchiveRootUnavailableMessage` names the rule and
  withholds the path; `logger.Warn` passes a redacted string plus the exception object rather than
  interpolating the exception message into user-visible text.
- **The single-read performance invariant is honoured and pinned.** `Globals.Ol.ArchiveRootPath` is
  COM-backed on first resolution, so a double read would add a real round trip. The helper reads once
  and `VerifyGet(..., Times.Once())` pins it.
- **The fixture design is documented rather than merely working.** `TestableEfcDataModel`'s XML doc
  explains why the two-argument `ConversationResolver` constructor is required and states that the
  five-argument constructor would materialize its helper through the lazy factory and read the archive
  root a second time. That is the kind of comment the "why, not what" rule asks for, and it prevents a
  future maintainer from making the change that silently breaks AC7.
- **The new test file is registered and proven registered.** Both projects are legacy non-SDK and
  enumerate every source file. The `<Compile Include>` entry was added, and the evidence goes further
  than asserting the line exists: it confirms all 11 tests appear in the full-suite TRX, which is the
  only thing that distinguishes a registered file from a silently absent one.

## Diff hygiene

- No unused `using` directive was added; all 12 imports in the new test file are consumed
  (`System.Runtime.InteropServices` by the `COMException` test,
  `System.Collections.Concurrent` by the special-folder dictionaries,
  `QuickFiler.Helper_Classes` by `ConversationResolver`, and `UtilitiesCS` by `MailItemHelper`,
  `IApplicationGlobals`, `IOlObjects` and `IFileSystemFolderPaths`).
- No commented-out code, no `TODO`, no debug output added.
- No `#pragma warning disable`, `[SuppressMessage]` or `[ExcludeFromCodeCoverage]` added.
- No absolute filesystem path, account name or machine name appears in either source file or in any
  of the 35 feature-folder documents; verified by recursive case-insensitive grep.
- The `.csproj` edit is a single line inserted in alphabetical proximity to the two sibling
  `EfcDataModel` test registrations; no other project property was touched, and CSharpier is not
  applied to `*.csproj` (`.csharpierignore:12`), so no formatter churn was introduced there.
- Both changed source files are under the 500-line cap: 485 and 389. `EfcDataModel.cs` has 15 lines
  of headroom remaining, which is worth remembering before the next addition to that file.
