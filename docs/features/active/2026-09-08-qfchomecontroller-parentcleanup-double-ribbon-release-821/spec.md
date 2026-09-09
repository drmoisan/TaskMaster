# 2026-09-08-qfchomecontroller-parentcleanup-double-ribbon-release (Spec)

- **Issue:** #821 (consolidates #822, which is closed)
- **Parent (optional):** epic `review-residuals-2026-09-08`, child feature F821
- **Owner:** drmoisan
- **Last Updated:** 2026-09-09
- **Status:** Draft
- **Version:** 1.0
- **Work Mode:** full-bug — this file is the **sole** authoritative acceptance-criteria source. No
  `user-story.md` exists for this feature and none may be created; a second file carrying markdown
  checkboxes would split the acceptance criteria and break the check-off protocol.

> Path formatting in this document is deliberate. Every repository file this fix will modify appears
> at least once as a backticked repository-relative path; every file the fix will **not** modify is
> written as bare prose, including line citations. Do not "fix" this formatting — a downstream tool
> derives the change footprint by harvesting backticked path tokens, so adding backticks to an
> out-of-scope path widens the apparent blast radius and removing them from an in-scope path drops
> the file out of the footprint.

## Context

Two sites where a teardown guard applied by issue 810 was not carried to a sibling site. Both are
enumeration gaps in that fix rather than new regressions. The write set was widened by the
orchestrator from the two sites the issue names to four, because two additional sites carry the
character-for-character identical defect (see Scope & Non-Goals).

**Site A — `QuickFiler/Controllers/QfcHomeController.cs` line 405.** `Cleanup` invokes the
parent-cleanup callback as `ParentCleanup?.Invoke();` inside a `finally` (block spans lines 403-407),
without the read-into-local-then-clear idiom that issue 810 applied one level down. The
null-conditional operator guards against the delegate being null, not against it having already run,
so a repeated `Cleanup()` releases the ribbon twice.

**Site A' — `QuickFiler/Controllers/EfcHomeController.cs` line 349.** `Cleanup` invokes
`_parentCleanup.Invoke();` with no null-conditional at all. This is strictly worse than Site A: a
repeat call double-releases, and a null field throws a NullReferenceException that aborts teardown.

**Site B — `UtilitiesCS/Threading/ProgressViewer.cs` line 75.** `CancelButton_Click` calls
`_cancelSource!.Cancel();`. The null-forgiving operator suppresses the compiler's null check rather
than guarding the call, and the invariant its comment relies on is not enforced by the one path that
can enable the button without assigning a source.

**Site B' — `UtilitiesCS/Threading/ProgressPane.cs` line 57.** `_tokenSource!.Cancel();` under the
identical comment (lines 54-56), with the identical unconditional enabling path. `ProgressPane` is
the live production progress surface, so this instance is more reachable than Site B.

Environment:

- OS/version: Windows 11 Pro 10.0.26200, .NET Framework 4.8 VSTO add-in hosted by Outlook
- Language/runtime: C# 12 (`LangVersion` 12.0), targeting net48. Not applicable: Python.
- Command/flags used: `vstest.console.exe` over `QuickFiler.Test.dll` and `UtilitiesCS.Test.dll`
- Data source or fixture: no live Outlook host required for any of the four reproductions

Impact / Severity:

- [ ] Blocker
- [x] High
- [ ] Medium
- [ ] Low

High, with the severity resting on different grounds at each site. Sites A and A' are
resource-lifecycle defects whose own coverage is structurally blind to them — the same failure shape
that let issues 784, 787 and 788 persist. Sites B and B' are unhandled exceptions on a WinForms event
handler in a VSTO add-in, which surface an exception dialog to the Outlook user.

## Corrections to the issue text

Four claims in `issue.md` and in the previous draft of this spec are wrong or imprecise. Each was
independently re-derived against this worktree. The corrected value is authoritative everywhere in
this document; the issue text is not to be restated.

| # | Claim as filed | Corrected value | Evidence |
|---|---|---|---|
| C1 | The Site A defect is at line 403 | Line 403 is the `finally` keyword. The defective statement `ParentCleanup?.Invoke();` is at **line 405**; the block spans 403-407. | `QuickFiler/Controllers/QfcHomeController.cs` lines 403-407 |
| C2 | The Site B cause is the `CancelSource` property setter at line 60, "a second path to enabling the button" | **Inverted.** The setter at line 60 reads `ButtonCancel.Enabled = value != null;` and is the one path that already enforces the invariant. The unconditional enabling path is `SetCancellationTokenSource` at **line 67**, which sets `ButtonCancel.Enabled = true;` with no null check on its parameter. The fix targets line 67, not line 60. | `UtilitiesCS/Threading/ProgressViewer.cs` lines 54-68; research section 2.5, paths E2 and E3 |
| C3 | `ProgressViewer` is the "fourth sharer" of the cancellation token source | **Wrong as an unqualified claim.** Two distinct numbers, both derived below: the token source constructed at `QuickFiler/Controllers/QfcHomeController.cs` line 54 has **9 holders**, of which `ProgressViewer` is the third to receive the reference. The figure **4** is correct only for the narrower family "sites that call `Cancel()` on that shared instance", where `ProgressViewer` is the fourth of exactly four. The phrase "fourth sharer" conflates the two and must not be carried forward. | Enumeration 2 below; research sections 2.2, 2.3 and 7 (N-1, N-2) |
| C4 | The Site A double invocation is a live production defect | **Latent in production, not observed at runtime.** The RibbonController uses either the synchronous `Init()` path or `LaunchAsync` on a given instance, never both, and each captured `Cleanup` method group is single-shot under issue 810's idiom. The double invocation is presently reachable only from the existing test. | Research sections 1.2 and 1.3; TaskMaster/Ribbon/RibbonController.cs lines 104-107, 118-121, 139-142 |

C4 does not weaken the case for fixing Site A. The supplied production callback is
`ReleaseQuickFiler` (TaskMaster/Ribbon/RibbonController.cs lines 148-153), whose third statement
`SetHighConfidenceModeForLaunch(false)` mutates a persisted settings value. A stale second invocation
from a previous controller instance can therefore reset high-confidence mode after a subsequent
launch has set it to true. "At most once per controller" is the correct invariant precisely because
the callback is not in fact idempotent; relying on the callback happening to be harmless is what
makes the defect latent rather than absent.

## Repro & Evidence

**Site A.** Read `QuickFiler/Controllers/QfcHomeController.cs` lines 403-407:

```csharp
            finally
            {
                ParentCleanup?.Invoke();
                logger.Info("Home cleanup complete; ribbon release callback invoked.");
            }
```

Call `Cleanup()` twice on the same controller instance; the ribbon-release callback fires on both.
The existing test cannot observe this: in
`QuickFiler.Test/Controllers/QfcHomeControllerCleanupTests.cs`, the first `Cleanup()` is at line 98,
the `parentCleanup.Verify(x => x.Invoke(), Times.Once);` assertion is at line 115, and the second
`Cleanup()` is at line 120 — five lines after the assertion. Moq evaluates `Verify` eagerly, so the
assertion observes exactly one invocation and cannot fail regardless of what line 120 does.

**Site A'.** Read `QuickFiler/Controllers/EfcHomeController.cs` lines 342-350: five field nullings
followed by `_parentCleanup.Invoke();`. The field is declared at line 285, assigned at lines 64 and
100, and exposed at lines 286-290. `Cleanup` is handed out as a method group at lines 90 and 234 to
two distinct EfcFormController instances, each of which invokes its own captured copy once, so two
invocations of this method on one home controller are reachable. The existing test
`Cleanup_ClearsControllerFieldsAndInvokesParentCleanup` at line 158 of
`QuickFiler.Test/Controllers/EfcHomeControllerLifecycleTests.cs` calls `Cleanup()` once and asserts a
boolean flag, so it cannot observe a count.

**Site B.** Read `UtilitiesCS/Threading/ProgressViewer.cs` lines 70-77. `CancelButton_Click` calls
`_cancelSource!.Cancel();` and then `this.Close();`. Reach the handler either after another holder
has disposed the source (the reachable half, see the trace in Proposed Fix) or with the source null
(the latent half, via `SetCancellationTokenSource` at line 67).

**Site B'.** Read `UtilitiesCS/Threading/ProgressPane.cs` lines 46-59. `SetCancellationTokenSource`
(lines 46-50) assigns the field and sets `this.ButtonCancel.Enabled = true;` unconditionally at line
49; `CancelButton_Click` calls `_tokenSource!.Cancel();` at line 57 and then `this.Dispose();`. The
file carries `#nullable enable` at line 1, as does `UtilitiesCS/Threading/ProgressViewer.cs`.

Expected: the parent-cleanup callback fires at most once per controller; clicking Cancel either
cancels the operation or does nothing, and never raises an unhandled exception out of a WinForms
event handler.

Actual: the callback fires on every `Cleanup()` call at both Site A and Site A'; the cancel handlers
throw NullReferenceException when the field is null and ObjectDisposedException when the owner has
already disposed the source, with no catch anywhere between the click and the message loop.

Logs / Screenshots:

- [ ] Attached minimal logs or screenshot
- Snippet: none captured at any of the four sites. All four findings are from reading the call sites.
  Neither the double invocation nor the throw has been observed at runtime.

## Scope & Non-Goals

### Write Set

Production files (four):

- `QuickFiler/Controllers/QfcHomeController.cs` — Site A
- `QuickFiler/Controllers/EfcHomeController.cs` — Site A'
- `UtilitiesCS/Threading/ProgressViewer.cs` — Site B
- `UtilitiesCS/Threading/ProgressPane.cs` — Site B'

Test files (four):

- `QuickFiler.Test/Controllers/QfcHomeControllerCleanupTests.cs`
- `QuickFiler.Test/Controllers/EfcHomeControllerLifecycleTests.cs`
- `UtilitiesCS.Test/Threading/ProgressViewer_Tests.cs`
- `UtilitiesCS.Test/Threading/ProgressPane_Tests.cs`

No project file is in the write set. All eight files already carry a Compile Include entry, verified
by reading each project file: QuickFiler.csproj line 297 and line 330, UtilitiesCS.csproj line 945
and line 973, QuickFiler.Test.csproj line 129 and line 175, UtilitiesCS.Test.csproj line 504 and line
506. Each appears exactly once, so there is no duplicate-entry hazard and no fan-in conflict on a
project file.

### In scope

- Applying the read-into-local-then-clear idiom at Sites A and A'.
- Replacing the null-forgiving dereference at Sites B and B' with an explicit guard that the compiler
  can see, plus a handler boundary that cannot leak an exception to the Outlook UI thread.
- Closing the unconditional button-enabling path in `SetCancellationTokenSource` at both Site B and
  Site B'.
- Adding regression tests to the four test files listed above.

### Scope widening: why A' and B' are in scope

The research record filed `EfcHomeController.cs` (finding O-1) and `ProgressPane.cs` (finding O-2) as
out-of-scope and asked the plan to decide. The orchestrator decided to include both, and that
decision supersedes the research record on these two rows. The rationale:

1. This issue exists because a guard was applied at one site and not carried to its siblings. Fixing
   only the two sites the issue names would reproduce, inside the fix for an enumeration-gap defect,
   exactly the enumeration gap the issue exists to close.
2. `ProgressPane` is the live production progress surface. Shipping a fix for `ProgressViewer` alone
   would leave the more reachable of the two instances unfixed.
3. Verified preconditions: no sibling feature owns either file; neither corresponding test file is in
   feature 826's `Console.SetOut` population; every file in the write set already has a Compile
   Include entry, so no project file is edited and no fan-in conflict is possible.

### Out of scope / non-goals

Paths in this section are deliberately unbackticked; see the note at the top of this document.

Sibling-owned files, which must not be modified under this issue:
QuickFiler/Controllers/QfcItemController.FolderHandling.cs;
UtilitiesCS/OutlookObjects/Store/StoreWrapperController files;
QuickFiler/Viewers/BreadcrumbPopupOwnerRegistry.cs;
QuickFiler/Viewers/BreadcrumbDropDownHost.Open.cs;
UtilitiesCS/NewtonsoftHelpers/SDIL Reader; UtilitiesCS.Test/Properties/AssemblyInfo.cs (read-only
precondition check only); UtilitiesCS/OutlookObjects/Table/OlTableExtensions files;
UtilitiesCS/Threading/TimeOutTask.cs; UtilitiesCS/Extensions/DfDeedle.cs;
UtilitiesCS.Test/OutlookObjects/Folder/FolderPredictorTests.cs; .editorconfig; BannedSymbols.txt;
every Console.SetOut restore in a test class; and docs/features/epics, whose manifest is maintained
by the epic planner.

Preserved findings, out of scope here, to be promoted as follow-up issues rather than fixed:

- **O-3 — undisposed token sources.** UtilitiesCS/EmailIntelligence/SubjectMap/
  SubjectMapSco.Orchestration.cs line 228 and UtilitiesCS/Threading/ProgressPackage.cs line 25 each
  construct a CancellationTokenSource that no holder ever disposes. Resource leak, not this issue's
  defect class.
- **O-4 — the synchronous Init path silently loads nothing.** QfcHomeController.CreateCancellationToken
  (declared at QuickFiler/Controllers/QfcHomeController.cs lines 465-469) has no production caller:
  searching all C# files for `CreateCancellationToken` returns six hits — the declaration, one call
  from QuickFiler.Test/Controllers/QfcHomeControllerMetricsTests.cs line 124, and four
  EfcHomeController entries. Consequently the synchronous `Init()` path at lines 86-106 passes a
  **null** `_tokenSource` to QfcFormControllerLoader at line 102, and
  QfcFormController.Actions.cs lines 38, 75 and 131 then early-return on `_tokenSource is null`, so
  `LoadItems` and `LoadItemsAsync` silently do nothing on that path.
  RibbonController.LoadQuickFiler (TaskMaster/Ribbon/RibbonController.cs lines 97-110) is that path.
  Distinct defect, distinct blast radius; this is the most significant of the preserved findings and
  must be promoted, not dropped.
- **O-5 — dormant production code.** UtilitiesCS/Threading/ProgressTrackerAsync.cs has no
  construction site outside its own tests. Dead-code cleanup, not this issue.

Explicitly excluded behaviour changes:

- No holder other than the existing one may dispose a shared CancellationTokenSource. See the
  borrower constraint in Proposed Fix.
- The two `catch (System.Exception e)` blocks in `Cleanup` at
  `QuickFiler/Controllers/QfcHomeController.cs` lines 382-385 and 399-402 must not be widened to
  enclose the `finally`, and the ObjectDisposedException catch at
  QuickFiler/Controllers/QfcFormController.SetupDisposal.cs lines 229-232 must not be broadened. The
  live test `Cleanup_DatamodelCleanupThrows_StillInvokesParentCleanup` at line 44 of
  `QuickFiler.Test/Controllers/QfcHomeControllerCleanupTests.cs` pins the opposite behaviour: a
  throwing datamodel stage must still reach the callback. Restructuring the try/catch/finally to
  "be safe" would break it.

## Enumeration 1 — the cleanup-invoker set (Sites A and A')

Carried into this spec so the next reviewer inherits it rather than re-deriving it. Search scope: the
entire QuickFiler tree — Controllers, Viewers, Helper Classes, Interfaces and Legacy. Search
patterns: `\.Invoke\(\)`; delegate-typed field and property declarations; repository-declared
`delegate` types; `_parentCleanup|parentCleanup|ParentCleanup`; `public void Cleanup\(\)`.

| # | Site | Shape | Idiom applied? | Reachable more than once? | In write set? |
|---|---|---|---|---|---|
| S-A | `QuickFiler/Controllers/QfcHomeController.cs` line 405 | `ParentCleanup?.Invoke();` inside `finally` | **No — unguarded** | Latent (C4) | **Yes — Site A** |
| S-B | QfcFormController.SetupDisposal.cs lines 269-271 | read local, clear field, invoke local | Yes (comment cites Issue #810 AC4) | Yes | No |
| S-C | EfcFormController.cs lines 305-310 | read local, clear field, null-checked invoke | Yes | Yes | No |
| S-D | `QuickFiler/Controllers/EfcHomeController.cs` line 349 | `_parentCleanup.Invoke();` — not even null-conditional | **No — unguarded and NRE-prone** | Yes: method group handed out at lines 90 and 234 | **Yes — Site A'** |
| S-E | BreadcrumbMessengerHub.cs lines 483-485 | read local, clear field, invoke local | Yes | Yes | No |
| S-F | BreadcrumbItemViewerLifecycleCoordinator.cs lines 364-365 | `Interlocked.Exchange` then invoke | Yes (atomic variant) | Yes | No |
| S-G | BreadcrumbDropDownOpenCoordinator.cs line 215 | bare call on a readonly field | Guarded by a different, sound mechanism: a `_released` flag set under lock at lines 361-373 | Guarded | No |
| S-H | BreadcrumbWebViewSurfaceFactory.cs lines 148-158 | bare call on a readonly field, in try/catch-log | No clear-after-invoke, but the delegate is an idempotent event detach | Harmlessly | No |
| S-I | QuickFiler/Legacy/QuickFileController.cs line 664 | unguarded | No | n/a | **Not compiled** |
| S-J | QuickFiler/Legacy/QfcLauncher.cs line 62 | unguarded | No | n/a | **Not compiled** |

Legacy exclusion is verified, not assumed: searching QuickFiler/QuickFiler.csproj for `Legacy` and
for the four legacy file names returns zero hits, so S-I and S-J are dead source and are not live
defects.

**Derived count: beyond Site A there is exactly ONE additional unguarded compiled site — S-D.** Both
independent derivations (invocation-side regex over `\.Invoke\(\)`, and declaration-side enumeration
of every delegate-typed field traced to its invocation sites) produced the same single-member set.
The declaration-side pass additionally confirmed that no delegate field in QuickFiler is invoked with
bare-call syntax in a teardown role that the invocation-side regex would have missed. Full derivation
with inclusion and exclusion rules is at section 7, N-3 of the research record.

Also enumerated and found clean: QfcItemController.ViewerSetup.cs lines 414-450,
QfcCollectionController.cs lines 2128-2140 and EfcItemController.cs line 231 are `Cleanup` methods
that invoke no owner callback at all and are already field-nulling and idempotent.

The idiom to copy is at QfcFormController.SetupDisposal.cs lines 262-272. The only adaptation needed
at Site A is that the target is an auto-property (declared at
`QuickFiler/Controllers/QfcHomeController.cs` line 154 as `internal System.Action ParentCleanup { get; set; }`),
not a `_parentCleanup` field, so the plan must not assume a field of that name exists on that type.
Site A' does have a plain field and needs no adaptation.

## Enumeration 2 — the cancellation-token-source holder set (Sites B and B')

The source under discussion is constructed at `QuickFiler/Controllers/QfcHomeController.cs` line 54,
inside `LaunchAsync`. Two further production sources exist (SubjectMapSco.Orchestration.cs line 228
and ProgressPackage.cs line 25); neither is ever disposed, which is finding O-3.

| # | Holder | Receives the reference at | Stores | Calls `Cancel()` | Calls `Dispose()` | Nulls its own reference |
|---|---|---|---|---|---|---|
| H1 | `LaunchAsync` local `tokenSource` | `QuickFiler/Controllers/QfcHomeController.cs` line 54 (constructs it) | local | no | no | n/a (scope) |
| H2 | ProgressTracker._cancelSource | ProgressTracker.cs line 22, field at line 80 | yes | no | no | no |
| H3 | **`ProgressViewer._cancelSource`** | `UtilitiesCS/Threading/ProgressViewer.cs` line 59, field at line 53 | yes | **yes, line 75** | no | no |
| H4 | QfcHomeController._tokenSource | `QuickFiler/Controllers/QfcHomeController.cs` line 117, field at line 471 | yes | no | **yes, line 389** | **yes, line 390** |
| H5 | QfcDatamodel._tokenSource | QfcDatamodel.cs line 66 and line 315, field at line 165 | yes | yes, line 77 and QfcDatamodel.QueueProcessing.cs line 50 | no | no |
| H6 | QfcFormController._tokenSource | QfcFormController.cs line 39, field at line 187 | yes | via the parent property at QfcFormController.EventHandlers.cs line 133 | no | no |
| H7 | QfcCollectionController._tokenSource | QfcCollectionController.cs line 42, field at line 112 | yes | no | no | no |
| H8 | QfcItemController._tokenSource | QfcItemController.Initialization.cs line 386, field at QfcItemController.cs line 59 | yes | no | no | no |
| H9 | ConversationResolver._tokenSource | ConversationResolver.cs line 79, field at line 246 | yes | no | no | no |

**Derived holder count: 9.** Derived twice on different axes — a declaration-side pass (regex over
CancellationTokenSource-typed private fields, then confirming each declaring type lies on the path
from line 54) and a flow-side forward trace (following every argument-passing site out of line 54 and
reading each callee's parameter-storage statements). Both member sets are identical with no residue
on either side. Full derivation, including the exclusion of forwarded-but-not-stored parameters,
CancellationToken struct copies, and child ProgressTracker instances (whose constructor does not copy
the source), is at section 7, N-1 of the research record.

**Derived count of sites that call `Cancel()` on that instance: 4** — QfcDatamodel.cs line 77,
QfcDatamodel.QueueProcessing.cs line 50, QfcFormController.EventHandlers.cs line 133, and
`UtilitiesCS/Threading/ProgressViewer.cs` line 75. Derived twice: holder-anchored (per-holder field
identifier inspection) and operator-anchored (three separate regexes covering the `.`, `?.` and `!.`
dereference forms, run repository-wide). Both member sets are identical. The `!.` regex is what makes
the second pass exhaustive: a search covering only `?.` or only `.` would have missed
`UtilitiesCS/Threading/ProgressViewer.cs` line 75 entirely, which is the point of this issue. Full
derivation is at section 7, N-2 of the research record.

**The two numbers, stated so the label cannot drift again:** the token source constructed at
`QuickFiler/Controllers/QfcHomeController.cs` line 54 has **9 holders**, and `ProgressViewer` is the
**third** of those to receive the reference. Of those holders, **4 code sites call `Cancel()`**, and
`ProgressViewer` is the **fourth and last** of those four. The unqualified phrase "fourth sharer" is
wrong and must not appear in the delivered code, comments or documents.

**Only one holder disposes the source: H4, at `QuickFiler/Controllers/QfcHomeController.cs` line 389.**
Only one holder can call `Cancel()` after that disposal: H3. H5 is cut off because line 388 runs its
`Cleanup()` before the dispose and line 391 nulls the reference; H6 routes through a property whose
backing field line 390 nulls, so its null-conditional short-circuits; H2, H7, H8 and H9 never cancel.
H3 holds its own captured reference, never nulls it, and is driven by a user click rather than by
teardown ordering. In the ordinary flow the viewer closes first, but that is an ordering convention,
not an enforced invariant — which is exactly why a guard is required to make it structural.

### Paths that can enable the Cancel button

| # | Path | Location | Sets Enabled to | Can it enable while the source is null? |
|---|---|---|---|---|
| E1 | Constructor | `UtilitiesCS/Threading/ProgressViewer.cs` line 24 | false | No |
| E2 | `CancelSource` property setter | `UtilitiesCS/Threading/ProgressViewer.cs` line 60 | `value != null` | **No — this path is correct** |
| E3 | `SetCancellationTokenSource` | `UtilitiesCS/Threading/ProgressViewer.cs` line 67 | **true, unconditionally** | **Yes** — the parameter is declared non-nullable but never checked |
| E4 | Designer InitializeComponent | ProgressViewer.Designer.cs lines 56-65 | never assigns it; Button.Enabled defaults to true | Transiently, before line 24 runs. Not user-reachable, but it means enabled is the default state rather than an opted-in one |
| E5 | External code | none | n/a | Not applicable; the field is private |

`ProgressPane` has the same shape with one fewer path: it has no `CancelSource` property, so E2 does
not exist and E3 (`UtilitiesCS/Threading/ProgressPane.cs` line 49) is the only enabling path other
than the constructor's explicit `false` at line 22.

## Root Cause Analysis

Issue 810 applied the read-into-local-then-clear idiom at one level of the QuickFiler teardown chain
and did not carry it to the level above, nor to the Efc sibling of that level. Separately, the
"preserve the NRE-if-null behaviour" comment at Sites B and B' asserts an invariant that the type
does not enforce, and expresses it with an operator that removes the compiler's ability to warn about
it. All four sites are the same defect class from the same origin: a guard applied at one site and
not carried to its siblings, discovered by an enumeration that was never performed.

Consolidated 2026-09-08: what was filed as issue 822 (Site B) is closed into issue 821. Widened
2026-09-09 to include Sites A' and B' by orchestrator decision, on the rationale recorded in Scope.

Removing the `!` is not cosmetic and the guard is not optional. `UtilitiesCS/Threading/ProgressViewer.cs`
line 1 and `UtilitiesCS/Threading/ProgressPane.cs` line 1 are both `#nullable enable`, and both
fields are declared as explicitly nullable references. Writing the dereference without the `!` and
without a preceding null check yields CS8602, "Dereference of a possibly null reference".
UtilitiesCS.csproj carries no NoWarn, WarningsNotAsErrors or TreatWarningsAsErrors element, and
.editorconfig does not downgrade any CS86xx diagnostic — its catch-all analyzer severity setting at
line 27 applies to analyzer rules, not to compiler diagnostics. Toolchain step 3 therefore promotes
CS8602 to a build error. Any change that removes the `!` must supply a null check the compiler can
see.

## Proposed Fix

### The invariant this fix establishes

Stated in one sentence per site pair:

- **Sites A and A':** a home controller's parent-cleanup callback is invoked **at most once per
  controller instance**, unconditionally with respect to whether an earlier teardown stage threw,
  because the delegate is read into a local and the stored reference is cleared **before** the local
  is invoked.
- **Sites B and B':** a cancel gesture on a borrowed CancellationTokenSource either requests
  cancellation, or throws `InvalidOperationException` carrying a diagnosable message when no source
  has been supplied, or returns quietly when the owner has already disposed the source — and **no
  exception of any type escapes the WinForms event handler**, which closes or disposes its surface in
  all three cases.

### Trace of one accepted value through Site B

This is the path with no guard anywhere between the accept point and the message loop, which is what
makes the fix load-bearing rather than defensive.

1. **Accept point.** `SetCancellationTokenSource` at `UtilitiesCS/Threading/ProgressViewer.cs` lines
   64-68 assigns its parameter to the field and sets `ButtonCancel.Enabled = true;` at line 67. It
   does **not** validate that the parameter is non-null, and it does not consult the field afterwards.
   The button is now clickable in a state the comment at lines 72-74 asserts is impossible.
2. **Throw point.** The user clicks. `CancelButton_Click` at line 75 evaluates
   `_cancelSource!.Cancel();`. With a null field this raises NullReferenceException; with a field
   whose owner has already disposed the source at `QuickFiler/Controllers/QfcHomeController.cs` line
   389 it raises ObjectDisposedException, because on net48 `CancellationTokenSource.Cancel()` throws
   after `Dispose()`.
3. **Current absorption point: there is none.** No `try` encloses line 75, no caller of the handler
   exists in first-party code (the wiring is the Designer-generated event subscription at
   ProgressViewer.Designer.cs line 65), and the next frame is the WinForms message loop. The
   exception therefore surfaces as an unhandled-exception dialog inside the Outlook process. A
   second consequence: `this.Close()` at line 76 is skipped, so the dialog stays on screen holding a
   live token.
4. **Where the fix puts the catch.** Inside `CancelButton_Click` itself, which is the only frame that
   both knows the operation is a user gesture on a disposable surface and can still guarantee the
   surface closes. It logs through log4net — already the established logger throughout
   UtilitiesCS/Threading — and closes in a `finally`, so a wiring defect can neither strand the
   dialog open nor reach the user as a stack trace.

Neither half of this fix suffices alone. Guarding without the boundary leaves any future throw inside
the handler unhandled on the UI thread. Adding the boundary without the guard leaves the CS8602
suppression in place and converts a real wiring defect into a silently swallowed no-op, which is the
failure mode the "preserve NRE-if-null" comment was written to avoid.

### Site B design decision: does the "preserve the NRE-if-null" intent still stand?

Partly. The comment conflates two distinct failure modes and the fix must split them.

- **Null source while the button is enabled is a wiring defect.** The type was configured incorrectly
  by its host. Silently doing nothing would hide a real bug, so the fail-fast intent stands — and per
  the issue's own validation note it must be expressed as a **thrown exception carrying a message**,
  not as a suppressed compiler check. The correct type is `InvalidOperationException`: the object is
  in an invalid *state* for the operation, which is distinct from a bad argument, and rethrowing a
  bare NullReferenceException with a message would be worse than the current operator because it
  would look like a genuine null dereference in telemetry.
- **ObjectDisposedException from a disposed source is a lifecycle race, not a defect in this type.**
  The owner disposed the source because the tracked operation is over. There is nothing left to
  cancel and the correct response is to do nothing. Swallowing it here is not the silent error
  suppression the code-change policy forbids, because the condition is expected and a narrow catch
  documents why — the same reasoning already accepted in-repo at
  QfcFormController.SetupDisposal.cs lines 229-232.

**Borrower constraint (binding).** `ProgressViewer` and `ProgressPane` are **borrowers** of the token
source, not owners. Neither constructs one: searching both files and their Designer partials for
`new CancellationTokenSource` returns no matches. The source is always supplied from outside, and
`ProgressViewer.Dispose(bool)` disposes only its `components` container. For the QuickFiler session
source, a different holder disposes it at `QuickFiler/Controllers/QfcHomeController.cs` line 389. The
fix may guard and may swallow the disposed case; it **must not dispose the source** and **must not
rethrow out of the handler**. A borrower that disposed a source another holder still uses would
convert a benign cancel click into a repository-wide ObjectDisposedException generator; a borrower
that rethrows surfaces the dialog the Expected Behaviour forbids.

### Recommended shape (recommended, not mandated — the planner owns sequencing)

Split the throwing logic out of the event handler so the throw is directly observable and testable,
and keep the handler as the boundary that never lets anything escape. This mirrors the established
in-repo boundary pattern at QuickFiler/Controllers/QfcFormController.EventHandlers.cs lines 110-123.

```csharp
        // Requests cancellation on the borrowed source. This viewer does not own the source and
        // never disposes it. Throws InvalidOperationException when the Cancel button was enabled
        // without a source, because that is a host wiring defect rather than a user error. Returns
        // quietly when the owner has already disposed the source, because the operation this viewer
        // was tracking has already finished and there is nothing to cancel.
        internal void RequestCancel()
        {
            CancellationTokenSource source =
                _cancelSource
                ?? throw new InvalidOperationException(
                    "ProgressViewer cancellation was requested with no CancellationTokenSource. "
                        + "Assign CancelSource or call SetCancellationTokenSource before enabling ButtonCancel."
                );

            try
            {
                source.Cancel();
            }
            catch (ObjectDisposedException)
            {
                // The source's owner disposed it; the tracked operation is already over. Distinct
                // from the null case above, which is a wiring defect, not a lifecycle race.
                logger.Debug("Cancel requested after the token source was disposed; nothing to cancel.");
            }
        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            // Boundary: this is a WinForms handler in a VSTO add-in, so an escaping exception
            // surfaces to the Outlook user. RequestCancel carries the diagnosable message; this
            // frame logs it and still closes, so a wiring defect cannot strand the dialog open.
            try
            {
                RequestCancel();
            }
            catch (System.Exception ex)
            {
                logger.Error("ProgressViewer cancel request failed.", ex);
            }
            finally
            {
                this.Close();
            }
        }
```

Notes on the choices:

- The `?? throw` form gives the compiler a provably non-null local, satisfying CS8602 without the `!`
  and without any suppression.
- `internal` rather than `private` on the new member lets the tests assert the exception type and
  message directly, avoiding the TargetInvocationException wrapping that reflection imposes. The
  precondition is satisfied: UtilitiesCS/Properties/AssemblyInfo.cs line 19 already carries
  `InternalsVisibleTo("UtilitiesCS.Test")`. This was confirmed read-only; no assembly-info file is
  edited. The research record left this as an open question, and it is now resolved in favour of
  `internal`.
- Closing in a `finally` is a small behaviour improvement inside the fix's own boundary: today
  `this.Close()` is skipped when the cancel throws.
- log4net is already used throughout UtilitiesCS/Threading, so no new dependency is introduced.
- Also close the enabling hole: change line 67 to enable only when the supplied source is non-null,
  making E3 agree with E2 and making the comment's stated invariant true. This does not make the
  `InvalidOperationException` unreachable — a test reaches it by assigning a null through
  `CancelSource` and calling the new member directly, because it deliberately does not consult the
  button's enabled state.
- File size: `UtilitiesCS/Threading/ProgressViewer.cs` is 92 lines today and lands near 125, well
  under the 500-line ceiling.

### The same design applies to Site B'

`UtilitiesCS/Threading/ProgressPane.cs` takes the identical treatment, with three adaptations:

1. The field is named `_tokenSource`, not `_cancelSource`, and the exception message must name the
   pane and its own member (`SetCancellationTokenSource`) rather than `CancelSource`, which the pane
   does not have.
2. `ProgressPane` is a `UserControl`, not a `Form`. Its handler calls `this.Dispose()` at line 58
   rather than `this.Close()`, so the `finally` must call `this.Dispose()`. The existing test file
   documents at lines 24-26 that the handler disposes the pane and that tests exercising that path
   must not wrap the pane in a `using` block; that constraint carries over to the new tests.
3. The enabling hole is at line 49 inside `SetCancellationTokenSource` (lines 46-50), and the pane
   has no property-setter equivalent of E2, so line 49 is the only enabling path to fix.

The pane needs a log4net logger field; UtilitiesCS/Threading already establishes that pattern.

### Site A and A' implementation

At Site A, in the `finally` at `QuickFiler/Controllers/QfcHomeController.cs` lines 403-407: read
`ParentCleanup` into a local, set the property to null, then invoke the local through a
null-conditional. The invocation must stay inside the `finally` so a throwing earlier stage still
reaches the callback. At Site A', at `QuickFiler/Controllers/EfcHomeController.cs` line 349: the same
three statements against the `_parentCleanup` field, which additionally removes the
NullReferenceException exposure of the current bare `Invoke()`.

### Dependencies or blocked work

None. No file in the write set is owned by a sibling feature, no project file is touched, and no
other feature in the epic must land first.

### Rollback

Revert the commit. There is no feature flag, no configuration key and no persisted state involved.

## Assumptions, Constraints, Dependencies

- Assumptions: `InternalsVisibleTo("UtilitiesCS.Test")` remains present at
  UtilitiesCS/Properties/AssemblyInfo.cs line 19 (verified 2026-09-09). If it is removed, fall back
  to a private member plus reflection, matching the existing pattern at
  `UtilitiesCS.Test/Threading/ProgressViewer_Tests.cs` lines 144-155, rather than editing any
  assembly-info file.
- Constraints: target framework net48. `init` accessors, positional records and record structs fail
  CS0518 on this framework and must not be proposed; a nominal record with ordinary get/set does
  compile. MSTest, Moq and FluentAssertions only. No temporary files in tests, no live Outlook
  dependency, no `Thread.Sleep` or `Task.Delay`, and no sleep, retry or timing tolerance may be used
  to stabilise any test.
- Test-host constraints: both WinForms test classes require STA and an installed
  SynchronizationContext, because the constructors call
  `TaskScheduler.FromCurrentSynchronizationContext()`. `UtilitiesCS.Test/Threading/ProgressViewer_Tests.cs`
  and `UtilitiesCS.Test/Threading/ProgressPane_Tests.cs` are both already `[STATestClass]` and both
  already install and restore the context in a `finally`; the new tests reuse that pattern unchanged.
- The headless-instance helper in `UtilitiesCS.Test/Threading/ProgressViewer_Tests.cs` bypasses
  `InitializeComponent`, leaving the button field null. Any test that reads the button's enabled
  state must use the real constructor path.
- External dependencies: none added. log4net is already referenced by UtilitiesCS.

## Data / API / Config Impact

- User-facing changes: a cancel click that previously produced an unhandled-exception dialog now
  either cancels, or logs and closes the surface. No visible UI change in the normal path.
- Public API: one new `internal` member per progress surface. No public signature changes, no
  breaking changes to any caller.
- Data or migration: none.
- Logging: two new log statements per progress surface — a debug entry when a cancel arrives after
  disposal, and an error entry when the cancel request fails. Both use the existing log4net pattern.
  No message may include user content beyond the type and member names shown above.
- Configuration: no keys added, changed or removed.

## Test Strategy

All new tests are MSTest with Moq and FluentAssertions, deterministic, create no temporary files,
sleep nowhere and require no Outlook process. The disposed-source tests dispose a
CancellationTokenSource synchronously and assert on the next statement, so no timing is involved.

**Site A.** Add a dedicated test `Cleanup_CalledTwice_InvokesParentCleanupOnce` to
`QuickFiler.Test/Controllers/QfcHomeControllerCleanupTests.cs`, rather than relocating the existing
`Times.Once` assertion at line 115 below the second `Cleanup()` call at line 120. Both options
produce a test that fails before the fix and passes after, so the deciding factor is contamination:
relocating line 115 places the ribbon-release assertion between the second `Cleanup()` call and the
issue 810 AC3 datamodel assertion at lines 121-125, entangling two unrelated guarantees in one test
and making a future failure ambiguous about which one broke. A dedicated test leaves the AC3
assertion isolated and matches the established sibling naming at line 379 of
QuickFiler.Test/Controllers/QfcFormControllerCancelTeardownTests.cs. The existing assertion at line
115 stays where it is; the new test carries a `because` string, consistent with every other assertion
in that file except line 115.

**Site A'.** Add `Cleanup_CalledTwice_InvokesParentCleanupOnce` to
`QuickFiler.Test/Controllers/EfcHomeControllerLifecycleTests.cs`. The existing probe helper in that
file records parent cleanup as a boolean, which cannot distinguish one invocation from two; the new
test needs a counting seam. The existing test `Cleanup_ClearsControllerFieldsAndInvokesParentCleanup`
at line 158 must remain and must continue to pass.

**Sites B and B'.** Five tests per surface:

| Test | Asserts |
|---|---|
| `RequestCancel_WhenSourceIsNull_ThrowsInvalidOperationExceptionWithMessage` | throws InvalidOperationException; the message is non-empty and names the member that supplies the source |
| `RequestCancel_WhenSourceIsDisposed_DoesNotThrow` | no exception |
| Handler test, null source | nothing escapes the handler, and the surface is closed or disposed |
| Handler test, disposed source | nothing escapes the handler, and the surface is closed or disposed |
| `SetCancellationTokenSource_WithNull_DoesNotEnableButton` | the button's enabled state is false |

Handler test names follow each file's existing style: `CancelButton_Click_WhenSourceIsNull_DoesNotThrowOutOfTheHandler`
and `CancelButton_Click_WhenSourceIsDisposed_DoesNotThrowOutOfTheHandler` in
`UtilitiesCS.Test/Threading/ProgressViewer_Tests.cs`, and
`CancelButtonClick_WhenSourceIsNull_DoesNotThrowOutOfTheHandler` and
`CancelButtonClick_WhenSourceIsDisposed_DoesNotThrowOutOfTheHandler` in
`UtilitiesCS.Test/Threading/ProgressPane_Tests.cs`, matching the existing
`CancelButtonClick_WhenInvoked_CancelsTokenSource` at line 108 of that file.

**Fail-before evidence.** The two new cleanup tests must be run against the pre-fix production code
and observed to fail. Capture that run to
docs/features/active/2026-09-08-qfchomecontroller-parentcleanup-double-ribbon-release-821/evidence/regression-testing/,
per the repository evidence-location convention. A fix whose test passes both before and after has
demonstrated nothing.

**Integration scenarios to retest manually.** Open and close QuickFiler repeatedly, confirming ribbon
state and high-confidence mode remain correct across launches; cancel a long-running operation, then
cancel again after the source has been disposed.

**Coverage.** Note a policy divergence and how it is resolved: CLAUDE.md sets the repository line
floor at 80% with 90% for new modules, classes and methods, while .claude/rules/general-unit-test.md
and .claude/rules/quality-tiers.md state 85% line and 75% branch. CLAUDE.md is authority 1 in the
stated policy compliance order, so 80/90 governs here and the 85/75 figures are not used. No
merge-base coverage baseline has been captured in this feature folder, so the repository-wide figure
is a record-and-report obligation rather than a blocking gate; the blocking coverage conditions are
change-scoped.

## Acceptance Criteria

- [ ] **AC1 — Site A guard.** In `QuickFiler/Controllers/QfcHomeController.cs`, the `finally` block of
      `Cleanup` reads `ParentCleanup` into a local, assigns null to `ParentCleanup`, and then invokes
      the local, in that order; the file contains zero occurrences of the statement
      `ParentCleanup?.Invoke();`. The invocation remains inside the `finally`.
- [ ] **AC2 — Site A' guard.** In `QuickFiler/Controllers/EfcHomeController.cs`, `Cleanup` reads
      `_parentCleanup` into a local, assigns null to `_parentCleanup`, and then invokes the local
      through a null-conditional; the file contains zero occurrences of the statement
      `_parentCleanup.Invoke();`.
- [ ] **AC3 — Site A regression test, added not moved.** A test method named
      `Cleanup_CalledTwice_InvokesParentCleanupOnce` exists in
      `QuickFiler.Test/Controllers/QfcHomeControllerCleanupTests.cs`, calls `Cleanup()` twice on one
      controller instance with **both calls preceding** its single `Times.Once` verification of the
      parent-cleanup mock, and supplies a `because` string on that verification.
- [ ] **AC4 — the existing Site A test is repaired, not left blind.** In
      `QuickFiler.Test/Controllers/QfcHomeControllerCleanupTests.cs`, the method
      `Cleanup_DisposesTokenSourceAndDetachesWorkerCompleted` has its **second**
      `controller.Cleanup()` call relocated to sit immediately after the first, so that **both**
      `Cleanup()` calls precede every assertion in the method, and its `Times.Once` verification of
      the parent-cleanup mock is therefore evaluated after both calls. The verification's own text is
      unaltered — only the position of the second `Cleanup()` call changes. The issue 810 AC3
      datamodel verification and its `because` string remain present and unaltered, and still assert
      exactly one datamodel cleanup across both passes.

      Rationale, and why this does not contaminate the 810 assertion: issue #821 and the epic
      delegation both require the move explicitly, because the existing test is the artifact that was
      structurally unable to observe the defect and repairing it is part of this fix. Moving the call
      up does not weaken the datamodel verification, whose meaning is "exactly one datamodel cleanup
      across two passes" and which holds identically whether the second pass runs before or after it.
      The dedicated test required by AC3 is added **in addition**, so the suite carries one
      single-purpose fail-before artifact alongside the repaired multi-assertion test; AC3 and AC4
      are both required and neither substitutes for the other.
- [ ] **AC5 — Site A' regression test.** A test method named
      `Cleanup_CalledTwice_InvokesParentCleanupOnce` exists in
      `QuickFiler.Test/Controllers/EfcHomeControllerLifecycleTests.cs`, calls `Cleanup()` twice on one
      controller instance with both calls preceding the assertion, and asserts an invocation **count
      equal to 1**, not a boolean flag. The pre-existing test
      `Cleanup_ClearsControllerFieldsAndInvokesParentCleanup` remains present and passes.
- [ ] **AC6 — fail-before evidence for both cleanup sites.** A captured test run showing the two new
      tests from AC3 and AC5 **failing** against the pre-fix production code is committed under
      docs/features/active/2026-09-08-qfchomecontroller-parentcleanup-double-ribbon-release-821/evidence/regression-testing/,
      naming both failing test methods and their assertion messages. Evidence written to any other
      location does not satisfy this criterion.
- [ ] **AC7 — Site B null case throws typed and diagnosable.** Requesting cancellation on
      `UtilitiesCS/Threading/ProgressViewer.cs` while its token-source field is null throws
      InvalidOperationException whose message is non-empty and names the member a caller must use to
      supply a source. Discharged by `RequestCancel_WhenSourceIsNull_ThrowsInvalidOperationExceptionWithMessage`
      in `UtilitiesCS.Test/Threading/ProgressViewer_Tests.cs`, which asserts both the exception type
      and the message content.
- [ ] **AC8 — Site B disposed case returns quietly.** Requesting cancellation on
      `UtilitiesCS/Threading/ProgressViewer.cs` after the source's owner has disposed it raises no
      exception. Discharged by `RequestCancel_WhenSourceIsDisposed_DoesNotThrow` in
      `UtilitiesCS.Test/Threading/ProgressViewer_Tests.cs`.
- [ ] **AC9 — Site B handler containment.** Neither the null case nor the disposed case allows any
      exception to escape `CancelButton_Click` in `UtilitiesCS/Threading/ProgressViewer.cs`, and the
      form is closed in both cases. Discharged by
      `CancelButton_Click_WhenSourceIsNull_DoesNotThrowOutOfTheHandler` and
      `CancelButton_Click_WhenSourceIsDisposed_DoesNotThrowOutOfTheHandler` in
      `UtilitiesCS.Test/Threading/ProgressViewer_Tests.cs`.
- [ ] **AC10 — Site B enabling hole closed.** `SetCancellationTokenSource` in
      `UtilitiesCS/Threading/ProgressViewer.cs` no longer sets the Cancel button's enabled state to
      true unconditionally; handed a null source it leaves the button disabled. Discharged by
      `SetCancellationTokenSource_WithNull_DoesNotEnableButton` in
      `UtilitiesCS.Test/Threading/ProgressViewer_Tests.cs`. The `CancelSource` property setter's
      existing behaviour at line 60 is unchanged, and the existing test
      `CancelSource_WhenAssigned_EnablesButtonAndCancelsSameSourceOnClick` still passes.
- [ ] **AC11 — Site B' receives the same four changes.** `UtilitiesCS/Threading/ProgressPane.cs`
      carries the same guard, the same typed throw with a message naming its own supplying member,
      the same handler boundary (closing with `this.Dispose()` rather than a form close), and the
      same conditional enabling. Discharged by five tests in
      `UtilitiesCS.Test/Threading/ProgressPane_Tests.cs`:
      `RequestCancel_WhenSourceIsNull_ThrowsInvalidOperationExceptionWithMessage`,
      `RequestCancel_WhenSourceIsDisposed_DoesNotThrow`,
      `CancelButtonClick_WhenSourceIsNull_DoesNotThrowOutOfTheHandler`,
      `CancelButtonClick_WhenSourceIsDisposed_DoesNotThrowOutOfTheHandler`, and
      `SetCancellationTokenSource_WithNull_DoesNotEnableButton`. The existing test
      `CancelButtonClick_WhenInvoked_CancelsTokenSource` still passes.
- [ ] **AC12 — borrower constraint held.** Neither `UtilitiesCS/Threading/ProgressViewer.cs` nor
      `UtilitiesCS/Threading/ProgressPane.cs` calls Dispose on its cancellation token source field,
      and neither constructs a CancellationTokenSource. The single disposal site for the QuickFiler
      session source remains `QuickFiler/Controllers/QfcHomeController.cs` line 389, and neither
      progress surface rethrows out of its event handler.
- [ ] **AC13 — the suppression is gone and nothing was weakened to achieve it.** The four production
      files in the Write Set contain zero occurrences of the token `!.Cancel()` and zero occurrences
      of the directive `#pragma warning disable`. Neither .editorconfig nor BannedSymbols.txt appears
      in the diff of this change, and no analyzer or compiler diagnostic severity is lowered
      anywhere.
- [ ] **AC14 — the delivered implementation matches the stated invariant and trace.** The code
      satisfies both invariant sentences in Proposed Fix verbatim, and the four-step trace holds
      against the delivered code: the accept point validates its argument, the throw point is
      replaced by a guarded call, the previously absent absorption point now exists inside the
      handler, and the surface closes in all three outcomes.
- [ ] **AC15 — no consumer-side catch was widened.** The two `catch (System.Exception e)` blocks in
      `Cleanup` at `QuickFiler/Controllers/QfcHomeController.cs` remain two separate blocks that do
      not enclose the `finally`, the ObjectDisposedException catch in
      QuickFiler/Controllers/QfcFormController.SetupDisposal.cs is unchanged, and the existing test
      `Cleanup_DatamodelCleanupThrows_StillInvokesParentCleanup` in
      `QuickFiler.Test/Controllers/QfcHomeControllerCleanupTests.cs` still passes, proving a throwing
      teardown stage still reaches the callback.
- [ ] **AC16 — the enumerations survive and the wrong label does not.** Both enumeration tables in
      this spec are still present at delivery with their derived figures intact (9 holders, 4
      cancelling sites, 1 additional unguarded cleanup site beyond Site A), and the phrase
      "fourth sharer" appears nowhere in the delivered production code, test code, code comments or
      commit messages.
- [ ] **AC17 — full toolchain pass, in order, on the final state of the tree.** In this order:
      `dotnet tool run csharpier format .` then `dotnet tool run csharpier check .` reporting zero
      files needing formatting; `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
      with zero errors and no new warnings; `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`
      with zero errors; then `vstest.console.exe` over QuickFiler.Test.dll and UtilitiesCS.Test.dll
      with coverage enabled and zero failures. Both msbuild invocations must use the Rebuild target:
      MSBuild's incremental up-to-date check does not invalidate on a command-line property change,
      so a warm Build target returns success having skipped compilation, and the gate becomes
      vacuous. If any step fails or changes a file, restart from the first step.
- [ ] **AC18 — change-scoped coverage.** Every new or modified member in the four production files —
      the new cancel-request member on each progress surface, both modified
      `SetCancellationTokenSource` methods, both modified `CancelButton_Click` handlers, and both
      modified `Cleanup` methods — reaches at least 90 percent line coverage in the AC17 test run, and no
      line changed by this fix drops in coverage relative to the merge base. The repository-wide line
      figure against the testable denominator defined in CLAUDE.md section UT2 is recorded in the
      delivery notes and must not be lowered by this change; because no merge-base coverage baseline
      exists in this feature folder, the repository-wide figure is reported rather than gated.
- [ ] **AC19 — no project file is modified.** No file with a .csproj extension appears in the diff of
      this change. All eight files in the Write Set already carry a Compile Include entry, so none is
      required.
- [ ] **AC20 — no out-of-scope file is modified.** The set of files changed by this fix is exactly the
      eight files in the Write Set, plus this spec, plus the evidence artifact required by AC6. No
      file named in Out of scope / non-goals is modified, and no policy document under .claude/rules
      or .github/instructions, and not CLAUDE.md.
- [ ] **AC21 — preserved findings survive the merge.** Findings O-3, O-4 and O-5 are present in the
      Out of scope / non-goals section of this spec at merge, each with its file-and-line citation,
      and O-4 (the synchronous Init path passing a null token source, making item loading a silent
      no-op) is written out in enough detail to be promoted verbatim by a later reader without
      re-deriving it. Because this spec is committed to the feature branch, the findings reach main
      with the fix rather than disappearing with the working tree.

      Discharge boundary: this criterion is satisfied by the content of this spec alone and requires
      no GitHub API call. Filing O-4 as its own issue is deliberately **not** an acceptance criterion
      of this feature, for two independent reasons. First, `atomic-executor` has neither `gh` nor the
      promotion MCP tools in its surface, so an executor-facing criterion demanding an issue could
      never be discharged by the agent expected to discharge it. Second, the promotion lifecycle
      writes a record under `docs/features/potential/promoted/`, which AC20's footprint lock forbids
      this feature from adding — so discharging it would falsify AC20. The promotion obligation is
      therefore carried at the orchestration layer and reported to the epic, not embedded here.

## Risks & Mitigations

- **Risk: closing the surface in a `finally` changes behaviour on the exception path.** Today a
  throwing cancel leaves the dialog open; after the fix it closes. Mitigation: this is the intended
  correction and is stated in the invariant. It is confined to a path that currently ends in an
  unhandled exception, so no working behaviour is displaced.
- **Risk: the Site A' counting seam requires changing a shared test helper.** The probe helper is a
  private nested class inside `QuickFiler.Test/Controllers/EfcHomeControllerLifecycleTests.cs`, but
  the same helper name appears in other Efc test files. Mitigation: change only the nested class in
  the file in the Write Set, keep the existing boolean member so the existing assertion at line 165
  still compiles, and add a counter alongside it.
- **Risk: WinForms tests are host-sensitive.** Mitigation: both target test classes already run STA
  with an installed and restored SynchronizationContext, and the pane's handler disposes the pane, so
  the new tests must not wrap the pane in a `using` block. Both constraints are documented in the
  existing files and are reused, not invented.
- **Risk: broad catch at the handler boundary trips an analyzer.** Mitigation: the repository's
  .editorconfig catch-all analyzer severity is set to suggestion, so CA1031 cannot be promoted to an
  error. This is verified by AC17 rather than assumed.

## Rollout & Follow-up

- Release: no staged rollout, no feature flag. The change ships with the next add-in build.
- Post-fix monitoring: confirm no new error entries from the two new log statements appear in the
  add-in log during normal QuickFiler use, which would indicate a real wiring defect the guard is now
  surfacing rather than hiding.
- Follow-up work: promote O-4 as its own issue before merge (AC21), and O-3 and O-5 as lower-priority
  issues.
- Links: GitHub issue #821 (consolidating the closed #822); epic `review-residuals-2026-09-08`;
  research record at
  docs/features/active/2026-09-08-qfchomecontroller-parentcleanup-double-ribbon-release-821/research/enumeration-findings.2026-09-08T23-45.md.
