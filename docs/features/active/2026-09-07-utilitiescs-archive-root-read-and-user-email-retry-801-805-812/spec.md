# utilitiescs-archive-root-read-and-user-email-retry-801-805 (Spec)

- **Issue:** #812
- **Parent (optional):** none
- **Owner:** drmoisan
- **Last Updated:** 2026-09-07T23-55
- **Status:** Approved
- **Version:** 0.1

Evidence base for every claim in this document:
`research/2026-09-07T23-45-utilitiescs-archive-root-and-user-email-retry-812-research.md`
(cited below by section ID: N1, N2, A1-A5, B1-B6, C1-C4). Every line number in this spec was
re-derived by reading the files in this worktree; where `issue.md` cites a different line, the
discrepancy is called out.

## Context
Consolidates two small `UtilitiesCS` defects filed as #801 and #805 so they ship as one change. (#801) `FolderPredictor.AddRecents` and `AddRecentRows` read the archive root through the `AppOlObjects.ArchiveRootPath` property, whose guard throws `InvalidOperationException` when the root cannot be resolved; with zero suggestions and a non-empty recents list they become the first reader on a path that previously never touched the property, so an unresolvable archive root now surfaces as a throw where the hierarchy provider degrades gracefully. (#805) The User Email retry added by #797 is documented as bounded to once per dialog open, but `PopulateWithCurrent` runs on every store re-selection and a failed retry leaves the address null, so a persistently failing Exchange lookup re-runs the blocking COM chain on the UI thread per selection change.

Environment:
- OS/version: Windows 11 Pro 10.0.26200
- Runtime: .NET Framework 4.8 VSTO add-in; `main` at `04a54e68`
- Command/flags used: QuickFiler with an unresolvable archive root and a non-empty recents list (#801); Settings -> Folder Settings, cycling the Display Name store selection on a mailbox whose Exchange lookup fails (#805)
- Data source or fixture: live mailbox; the 2026-09-06 log shows `COMException: The operation failed.` from `Session.CurrentUser`

Impact / Severity:
- [ ] Blocker
- [ ] High
- [x] Medium
- [ ] Low

#801 is a new throw on a previously non-throwing path; #805 is unbounded repetition of a synchronous COM call known to block the Outlook UI thread, in exactly the failure case the user is diagnosing.


## Repro & Evidence
Steps to Reproduce:
1. (#801) Make `ArchiveRootPath` unresolvable (no folder literally named `Archive` under the default store root, or a cross-store mismatch so `ArchiveRootPathGuard` throws). Launch QuickFiler on an item with no classifier suggestions and at least one recent folder. Observe `InvalidOperationException` propagating out of the recents projection instead of the entries rendering as stored.
2. (#805) On a profile where User Email shows "Email address unavailable: ...", open Folder Settings and change the store selection several times. Observe `GetSmtpAddressFromStore` running its full COM chain on every re-selection.

Expected:
- An unresolvable archive root degrades the recents projection to identity (entries rendered as stored), matching `OutlookFolderHierarchyProvider` and `EfcDataModel.TryGetArchiveRoot`, with one logged warning.
- The User Email retry runs at most once per dialog open per controller instance, and the comments and #797 specification prose state the bound the code enforces.

Actual:
- `FolderPredictor.cs` `AddRecents` / `AddRecentRows` read `_globals.Ol.ArchiveRootPath` unconditionally; the guard's exception propagates.
- `StoreWrapperController.Display.cs` gates the retry on `Current.UserEmailAddress is null`; `RefreshUserEmailAddress` assigns null back on failure; `PopulateWithCurrent` is called from `DisplayName_SelectedValueChanged` on every selection change. Comments in `StoreWrapperController.Display.cs`, `StoreWrapper.cs`, `StoreWrapperController_Tests.Display.cs`, and the #797 `spec.md` all claim once-per-open.

Line-number corrections to `issue.md` (re-derived; N2, and confirmed by reading each file):

| `issue.md` claim | Actual position in this worktree |
|---|---|
| `Display.cs` lines 41-51 (comment block) | comment at `StoreWrapperController.Display.cs:35-41`; retry gate at `:42-45` |
| `StoreWrapper.cs` 214-219 (comment) | `StoreWrapper.cs:194-199`; the assignment is at `:200` |
| #797 `spec.md` lines 491-493 | the sentence begins at `:492`; the surrounding claim runs `:490-493` |
| (not cited at all) | `UtilitiesCS.Test/OutlookObjects/Store/StoreWrapperController_Tests.Display.cs:90-92` |


## Scope & Non-Goals

### Write Set (the complete set of files this change may modify or create)

Production:
- `UtilitiesCS/OutlookObjects/Folder/FolderPredictor.ArchiveRoot.cs` (new partial part: logger + guarded accessor + the relocated `ProjectSuggestionPath` helper)
- `UtilitiesCS/OutlookObjects/Folder/FolderPredictor.cs` (replace the three display-projection read expressions with accessor calls; hoist the suggestion read; net line count must not increase. Dated amendment, 2026-09-08, per the #812 plan D9: the private helper `ProjectSuggestionPath` is relocated verbatim out of this file into the new part file, with a second `archiveRoot` parameter added and no behavioural change. The relocation is required rather than cosmetic, because without it the two hoisted accessor calls would raise the file from 1002 to 1004 lines and AC6 requires the post-change count to be no greater than 1002.)
- `UtilitiesCS/OutlookObjects/Store/StoreWrapperController.cs` (per-instance latch field + XML doc)
- `UtilitiesCS/OutlookObjects/Store/StoreWrapperController.Display.cs` (latch in the retry gate; comment correction)
- `UtilitiesCS/OutlookObjects/Store/StoreWrapper.cs` (comment correction only)

Test:
- `UtilitiesCS.Test/OutlookObjects/Folder/FolderPredictorArchiveRootDegradationTests.cs` (new)
- `UtilitiesCS.Test/OutlookObjects/Folder/ArchiveStemProjectionTests.cs` (add the null-root identity case; A5 gap)
- `UtilitiesCS.Test/OutlookObjects/Store/StoreWrapperController_Tests.Display.cs` (latch tests + comment correction)

Build configuration (dated amendment, 2026-09-08, per the #812 plan D14):
- `UtilitiesCS/UtilitiesCS.csproj` (add the `<Compile Include>` item for the new production part file)
- `UtilitiesCS.Test/UtilitiesCS.Test.csproj` (add the `<Compile Include>` item for the new test file)

Both project files are legacy `packages.config`-style projects carrying explicit `<Compile Include>`
items rather than globbing, so a new `.cs` file that is not added to its project does not compile.
Editing them is a mechanical consequence of the project format, not a scope expansion.

Write Set scoping note (dated amendment, 2026-09-08, per the #812 plan D14): the AC6 500-line audit
is executed over `*.cs` paths only. `.claude/rules/general-code-change.md` scopes the cap to
production code, test code, and reusable script files; a `.csproj` is build configuration and sits
outside that scope, and `.csharpierignore` likewise keeps `*.csproj` out of the formatter.

Documentation:
- `docs/features/active/2026-09-06-folder-settings-never-persist-and-user-email-error-loading-797/spec.md` (three prose passages)
- this feature folder's own artifacts (`spec.md`, plan, evidence)

Any backticked path elsewhere in this document that is not in the list above is a citation, not a
write target.

### In scope

1. **Defect A — three display-projection reads of `ArchiveRootPath` in `FolderPredictor`.** Decision 1.
   `FolderPredictor.cs` holds eight reads of `_globals.Ol.ArchiveRootPath` (N1: `:305`, `:376`,
   `:687`, `:752`, `:795`, `:857`, `:876`, `:914`; assertion admitted at count 8 by two independent
   query strategies with identical member sets). Exactly three are display projections that terminate
   in `ArchiveStemProjection.ToDisplayStem`:
   - `AddRecents(ref List<string>)` — read at `:795`, projection at `:797`
   - `ProjectSuggestionPath(string)` — read and projection at `:857`
   - `AddRecentRows(List<FolderRow>)` — read at `:876`, projection at `:879`

   These three are in scope.

2. **Defect B — the User Email retry bound.** A per-controller-instance latch in
   `StoreWrapperController` plus the prose correction. Decisions 4, 5, 6.

### Why the scope is three reads and not the two the issue names

`issue.md:16` and `:56` name only `AddRecents` and `AddRecentRows`. That is insufficient, and the
call graph (A2) is decisive:

| Entry point | Suggestion projection | Recents projection |
|---|---|---|
| `FolderArray` getter (`FolderPredictor.cs:217-232`) | `AddSuggestions(ref _folderList)` at `:225`, gated on `Suggestions.Count > 0` at `:224` | `AddRecents(ref _folderList)` at `:227` |
| `FolderRowArray` getter (`:244-259`) | `AddSuggestionRows(rows)` at `:251`, gated on `Suggestions.Count > 0` at `:249` | `AddRecentRows(rows)` at `:255` |
| `FindFolder(...)` (`:293-343`) | `AddSuggestions` at `:337`, unconditional | `AddRecents` at `:340` |
| `FindFolderRows(...)` (`:364-413`) | `AddSuggestionRows` at `:407`, unconditional | `AddRecentRows` at `:410` |

`AddSuggestions` (`:812-816`) and `AddSuggestionRows` (`:840-851`) reach the property through
`ProjectSuggestionPath` at `:857`. So whenever the suggestion set is non-empty the
`InvalidOperationException` escapes at `:857` **before** the recents projection is entered, and
guarding only `:795` and `:876` would leave the defect observable in the common case. The issue's
"zero classifier suggestions" repro is the one case that misses `:857` — internally consistent, but
narrower than the defect.

### Out of scope / non-goals

1. **The five functional reads stay as they are: `:305`, `:376`, `:687`, `:752`, `:914`.** Decision 1.
   `:305` and `:376` seed `emailSearchRoots`; `:687` and `:752` seed the folder-creation ancestor
   `olAncestor`; `:914` seeds the `LoopFolders` prefix-stripping ancestor. Substituting a null root at
   any of them changes which folders are searched or where a folder is created. That is a behaviour
   change, not graceful degradation, so all five keep their current throwing behaviour. If they need
   attention they belong in a separate issue.
2. **`QuickFiler/Controllers/QfcItemController.FolderHandling.cs:233` is not fixed here.** It reads
   `_globals.Ol?.ArchiveRootPath ?? string.Empty` inside `AssignFolderComboBox` (`:191`), six lines
   after consuming `FolderRowArray` at `:221`, in the same call frame and inside no `try`. The `?.`
   guards a null `Ol`, not a throwing property (A2). Consequence the reader must accept: after this
   change AC1's outcome is verifiable at the `FolderPredictor` unit level, **not** end to end in
   QuickFiler, until that site is also guarded. File as a follow-up defect.
3. **The historical #797 artifacts are NOT retroactively rewritten.** Decision 6. Four passages across
   three files: `plan.2026-09-06T22-00.md:663` and `:832`,
   `research/research-folder-settings-persistence.md:321`, and
   `evidence/issue-updates/issue-797.2026-09-06T22-00.md:46` (N2 sites 7-10). A completed plan, a
   dated research note, and an evidence record are dated statements of what was believed when they
   were written; amending them destroys the audit trail that made this defect discoverable. Only the
   living `spec.md` and the three code/test comments are corrected.
4. **`code-review.2026-09-07T22-40.md` and `feature-audit.2026-09-07T22-40.md` need no edit.** They
   already state the true bound (N2).
5. **No non-blocking Outlook COM read is delivered.** #797 `spec.md:208-214` (Non-Goals item 8)
   records the verified finding that no seam in `UtilitiesCS` makes an Outlook COM property read
   non-blocking: the only timeout primitive dispatches to ThreadPool (MTA) threads and an interop
   object is STA-bound, so the STA still blocks. That remains a filed follow-up.
6. **The other #797 advisory findings are not folded in:** CR-2 (`SerializeNow` file I/O and unbounded
   write-lock wait on the UI thread), CR-3 (single-shot guard re-armed early), CR-5 (AC5 double
   persistence). CR-2 was flagged in `issue.md:58` as foldable "if scope allows"; it is not folded in,
   because it is a distinct UI-thread-blocking defect on a different member and widening scope would
   put a third independent change into one bug branch.
7. **No new public API.** No signature on a public member changes except the addition of an archive-root
   parameter to the private `ProjectSuggestionPath`.


## Root Cause Analysis

### Defect A

`AppOlObjects.ResolveValidatedArchiveRootPath` (`TaskMaster/AppGlobals/AppOlObjects.ArchiveRoot.cs:40-72`)
delegates to `ArchiveRootPathGuard.RequireResolvedArchiveRoot` and normalizes a transient
`COMException` into `InvalidOperationException` (`:54-65`, with the contract stated in the XML doc at
`:35-39`) so that the getter's documented contract admits exactly one exception type. Consumers that
are not written to handle it therefore see an `InvalidOperationException` on an unresolvable or
cross-store archive root.

Two consumers already absorb it. `FolderPredictor` does not. Before #799 the recents path never read
the property; the shared projection introduced by #799 made three display surfaces first readers of a
throwing property on paths that previously could not throw.

`ArchiveStemProjection.ToDisplayStem` already degrades to identity on a null root:
`UtilitiesCS/OutlookObjects/Folder/ArchiveStemProjection.cs:45-48` returns `folderPath` unchanged, and
the parameter documentation at `:32-35` states that null, empty, and whitespace-only roots disable the
projection. No change to `ArchiveStemProjection` is needed (A5).

### Defect B

`StoreWrapper.RefreshUserEmailAddress` (`UtilitiesCS/OutlookObjects/Store/StoreWrapper.cs:192-202`)
assigns the lookup result — including `null` on total failure — back to `UserEmailAddress` at `:200`.
`GetSmtpAddressFromStore` (`:204-286`) has one terminal failure return, `return null;` at `:285`. The
controller gate `Current is not null && Current.UserEmailAddress is null`
(`StoreWrapperController.Display.cs:42-45`) therefore evaluates true again on the next
`PopulateWithCurrent`, which `DisplayName_SelectedValueChanged` invokes on every store re-selection.
The chain is synchronous (`RefreshUserEmailAddress` and `GetSmtpAddressFromStore` are non-`async`,
contain no `await` and no thread dispatch) and runs on the Outlook UI thread, because
`PopulateWithCurrent` marshals to it at `Display.cs:19-23` (B1). `issue.md:43` records a
`ThreadMonitor` observation inside `_ExchangeUser.get_PrimarySmtpAddress()` — the read at
`StoreWrapper.cs:237` — at 17:35:21 in `debug_2026-09-06.log`. The harm is N blocking COM chains for N
re-selections where the documentation promised one.


## Proposed Fix

### Design summary (what changes where)

**Defect A.** One shared guarded accessor on `FolderPredictor`, used by all three display-projection
call sites. It reads `ArchiveRootPath`, catches the `InvalidOperationException` the guard raises, logs
one warning, and returns `null`. `ToDisplayStem` then returns each entry unchanged, which is the
degraded outcome AC1 requires.

**Defect B.** A per-instance `bool` latch on `StoreWrapperController`, tested and set inside the
existing retry gate in `PopulateWithCurrent`. No reset anywhere. Plus the prose correction at four
sites.

### Decision A1 — precedent to follow: `EfcDataModel.TryGetArchiveRoot`

Two graceful-degradation precedents exist (A1). The chosen template is
**`EfcDataModel.TryGetArchiveRoot`, `QuickFiler/Controllers/EfcDataModel.cs:271-297`** (the `try` at
`:282`, the `catch (InvalidOperationException ex)` at `:287`, `logger.Warn(message, ex)` at `:290-294`,
`return false` at `:295`; XML doc `:271-277`). Three reasons, each verifiable:

1. **Structural fit.** `FolderPredictor` reads through the same `<globals>.Ol.ArchiveRootPath` chain
   that `EfcDataModel` does. The alternative precedent,
   `UtilitiesCS/OutlookObjects/Folder/OutlookFolderHierarchyProvider.cs:137-164`, reads through an
   injected `Func<string>? ArchiveRootAccessor` (`:94`, ctor parameter `:82`); adopting it would mean
   threading a delegate through three `FolderPredictor` constructors (`:26`, `:36`, `:43`) plus
   `AppAutoFileObjects.FolderPredictorLoad.cs` and the QuickFiler factory delegates — far larger than
   the defect warrants.
2. **Exception-type fidelity.** The provider catches bare `Exception` (`:156`). The throw to absorb is
   precisely `InvalidOperationException`, because `AppOlObjects.ArchiveRoot.cs:54-65` normalizes
   `COMException` into that single type. A bare catch would additionally swallow genuine programming
   errors on a display path, which General Code Change Policy §3 does not support.
3. **Log level.** `EfcDataModel` uses `logger.Warn`; the provider uses `logger.Debug` (`:157`). AC1
   requires a warning.

### Decision A2 — the accessor and its logger live in a NEW partial part file

`FolderPredictor.cs` is **1002 lines**, already twice the 500-line cap in
`.claude/rules/general-code-change.md`, and a `Grep` for `logger|log4net` over it returns zero matches,
so a logger must be introduced. The established remedy in this exact class is documented verbatim at
`UtilitiesCS/OutlookObjects/Folder/FolderPredictor.IFolderSearchHandler.cs:4-9`, which records that a
second partial-class part was created specifically "so `FolderPredictor.cs` itself (already 823 lines,
over the 500-line cap before this cycle) is not touched beyond the one-word `partial` edit".

The same move applies. Create `FolderPredictor.ArchiveRoot.cs` holding the repo-standard log4net
declaration (the form used at `StoreWrapperController.cs:73-75` and
`OutlookFolderHierarchyProvider.cs:45-47`) and the guarded accessor. Confine edits inside
`FolderPredictor.cs` to replacing read expressions, so its line count does not increase.

Test placement follows the same constraint: `UtilitiesCS.Test/OutlookObjects/Folder/FolderPredictorTests.cs`
is **1066 lines** and must not receive new tests; new Defect A tests go in a new file.
`UtilitiesCS.Test/OutlookObjects/Store/StoreWrapperController_Tests.Launch.cs` is 480 lines and must
not receive the Defect B tests; `StoreWrapperController_Tests.Display.cs` (252 lines) does.

### Boundaries and invariants to preserve

1. **Only `InvalidOperationException` is caught.** No bare `catch`. Any other failure, including a COM
   failure that escaped normalization, still propagates — the same contract
   `EfcDataModel.cs:275-276` states in prose.
2. **At most one property read, and therefore at most one warning, per projection-helper invocation** —
   not one per recent entry and not one per suggestion. `AddRecents` already hoists the read to `:795`,
   outside the `Select` at `:796-798`, and `AddRecentRows` to `:876`, outside the `foreach` at
   `:877-881`. That structure must be preserved.

   **Refinement of the invariant at the suggestion surface.** `ProjectSuggestionPath` (`:853-858`) is
   invoked *per element* — as a method group inside `Suggestions.ToArray(5).Select(...)` at `:815`, and
   inside the `foreach` at `:845-850`. Reading through the accessor there as-is would log up to five
   warnings per projection. To hold the invariant, `AddSuggestions` and `AddSuggestionRows` must hoist
   one accessor call above their loop and pass the resolved root into `ProjectSuggestionPath` as a
   second parameter, exactly mirroring the structure `AddRecents` already uses. `:815` becomes a lambda
   rather than a method group. This is a net-neutral line change in `FolderPredictor.cs` and adds no
   public surface (`ProjectSuggestionPath` is private).
3. **The null-`_globals` tolerance at `:857` must survive.** The navigation-only constructor
   (`:26-34`) sets `_globals = null!`, which is why `:857` reads `_globals?.Ol.ArchiveRootPath` rather
   than `_globals.Ol...`. The accessor must return `null` when `_globals` is null rather than throwing
   `NullReferenceException`. The recents sites are unaffected: `:791` and `:865` already dereference
   `_globals.AF` first.
4. **Text parity between `FolderArray` and `FolderRowArray` is preserved.** The contract is documented
   at `FolderPredictor.cs:234-242`. Guarding one surface and not its mirror would break it, which is a
   second reason all three display reads move together.
5. **`ArchiveStemProjection` is not modified.** Identity on a null root is already its documented and
   implemented contract (`:32-35`, `:45-48`).

### Decision B1 — direction (i), the per-instance latch, PLUS the prose correction

Both directions were open (B6). The decision is the hybrid, on this reasoning: #797 `spec.md:619-624`
accepted the reintroduction of a synchronous UI-thread COM read **on the stated basis** that it was
bounded to once per open ("Mitigation: retry at most once per dialog open and only when the address is
null, which bounds the added latency to the same single lookup the startup path already performs. The
residual risk is accepted because AC6 as written requires the retry."). Because the shipped code does
not enforce that bound, what shipped is not the risk that was accepted. Restoring the bound restores
the accepted risk profile. Correcting only the prose would ratify an unbounded blocking COM chain on
the Outlook UI thread in exactly the failure case the user is diagnosing.

Accepted behaviour change: after a failed retry, re-selecting the same store no longer re-attempts
within one dialog session. A user who wants another attempt closes and reopens the dialog. The label
text is unchanged either way, because `BuildUserEmailUnavailableText()` (`Display.cs:77-86`) renders
from the already-captured `LastSmtpLookupError`, which persists on the `StoreWrapper` after the first
failure.

### Decision B2 — the latch needs NO reset, and nothing touches `Launch`

`issue.md:66` proposes a flag "set on the first `RefreshUserEmailAddress` retry and reset in `Launch`",
with a test that "a new `Launch` permits one more". **The premise is false and this spec corrects it.**

`StoreWrapperController` has exactly one production construction site:
`TaskMaster/Ribbon/RibbonController.cs:259-263`, inside `FolderStoresSettings()`, which constructs a
fresh controller at `:261` and calls `Launch()` at `:262`. A repo-wide `Grep` for
`new StoreWrapperController` over `*.cs` returns this one production site and 24 test sites; both
ribbon entry points forward to this method (B2). A fresh instance per dialog open means a plain
per-instance `bool` field already **is** "at most once per dialog open", with no reset anywhere.

This matters for verifiability. `StoreWrapperController.Launch()` carries `[ExcludeFromCodeCoverage]`
at `StoreWrapperController.cs:115` and calls `Viewer.ShowDialog()` at `:135`. An exempt member emits no
`<method>` element in the Cobertura document at all, so a reset placed inside `Launch` would be neither
executable nor observable under unit test — the "proven to work but never proven to be wired" failure
mode the #797 review already flagged once. Placing no reset keeps every line of the change inside
`PopulateWithCurrent`, a member with eight existing tests and a working viewer double whose
`InvokeRequired` returns `false` (`StoreWrapperController_Tests.cs:160-178`, the `Setup` at `:168`).

The upstream AC2 wording "a new `Launch` permits one more" is restated as **"a new controller instance
permits one more."**

Residual risk and its mitigation: if a future caller reuses one controller across dialog opens, the
bound silently becomes once-per-lifetime. Mitigated by an XML doc comment on the latch field naming
`RibbonController.FolderStoresSettings` as the sole construction site and stating the dependency.

The latch field is `internal` or `private`; `UtilitiesCS/Properties/AssemblyInfo.cs:19` grants
`InternalsVisibleTo("UtilitiesCS.Test")`, so an `internal` field is directly assertable without
reflection (C4), following the precedent recorded at `StoreWrapperController.Display.cs:161-164`.

### Decision B3 — which prose is corrected

Four sites, all of which must state the bound the code enforces after this change ("at most once per
controller instance, that is once per dialog open because a fresh controller is constructed per open"):

1. `UtilitiesCS/OutlookObjects/Store/StoreWrapperController.Display.cs:35-41` — the `why: issue #797
   AC6` comment above the retry gate.
2. `UtilitiesCS/OutlookObjects/Store/StoreWrapper.cs:194-199` — the comment inside
   `RefreshUserEmailAddress`. Its wording, "The settings dialog calls this at most once per open",
   asserts a property of the *caller*; it must be restated as what this member guarantees (nothing —
   it re-runs the lookup on every call) plus what the caller now enforces.
3. `UtilitiesCS.Test/OutlookObjects/Store/StoreWrapperController_Tests.Display.cs:90-92` — the Arrange
   comment in `PopulateWithCurrent_WhenUserEmailIsAlreadyPopulated_DoesNotRetryLookup`. Missed entirely
   by `issue.md`; it is the third code comment the #797 review counted.
4. `docs/features/active/2026-09-06-folder-settings-never-persist-and-user-email-error-loading-797/spec.md`
   — three passages: Non-Goals item 8 at `:212-213`, the AC6 detail at `:490-493`, and Risks item 1 at
   `:619-624`. The AC6 detail passage is additionally **self-contradictory** as written: it asserts in
   one sentence that `PopulateWithCurrent` "runs both when the dialog opens and on every store
   re-selection" and in the next that "the retry is attempted at most once per dialog open". The
   correction must reconcile all three passages to one statement.

Each #797 `spec.md` edit is marked as a dated correction referencing issue #812, so the amendment is
visible as an amendment rather than presented as the original text.

### Files/modules to change

See the Write Set above. Line-count headroom (B3), all verified in this worktree:

| File | Lines now | Cap risk |
|---|---|---|
| `UtilitiesCS/OutlookObjects/Folder/FolderPredictor.cs` | 1002 | Already 2x over cap; net line count must not increase |
| `UtilitiesCS/OutlookObjects/Folder/FolderPredictor.ArchiveRoot.cs` | new | None |
| `UtilitiesCS/OutlookObjects/Store/StoreWrapperController.cs` | 388 | Low (a field + XML doc, ~8 lines) |
| `UtilitiesCS/OutlookObjects/Store/StoreWrapperController.Display.cs` | 173 | None |
| `UtilitiesCS/OutlookObjects/Store/StoreWrapper.cs` | 302 | None (comment rewrite only) |
| `UtilitiesCS.Test/OutlookObjects/Store/StoreWrapperController_Tests.Display.cs` | 252 | Low |
| `UtilitiesCS.Test/OutlookObjects/Folder/ArchiveStemProjectionTests.cs` | 176 | None |

### Error handling and logging updates

- One `logger.Warn(message, ex)` inside the single `catch (InvalidOperationException)` in the new
  accessor. The message must name the rule only and must not include the archive root path or any
  mailbox address, following the redaction precedent at `EfcDataModel.cs:267-269` and `:290-294` and
  the reason recorded at `AppOlObjects.ArchiveRoot.cs:35-39` (#602).
- No new user-facing message. Defect A's user-visible outcome is that the display renders stored paths
  instead of throwing.
- No logging change for Defect B.

### Rollback / feature-flag considerations

None. Both changes are small and local; rollback is a revert of the commit. No configuration key, no
flag, no migration.

## Assumptions, Constraints, Dependencies

- **Assumption (load-bearing):** `StoreWrapperController` continues to be constructed once per dialog
  open. Verified today at `RibbonController.cs:261`; documented in-code by Decision B2's XML comment so
  a future change to that site is visibly coupled to this bound.
- **Assumption:** the eight `ArchiveRootPath` reads enumerated in N1 are complete for
  `FolderPredictor`. Established by two independent search strategies with identical member sets.
- **Constraint:** `#nullable enable` is present at line 1 of `FolderPredictor.cs`,
  `ArchiveStemProjection.cs`, `StoreWrapperController.cs`, `StoreWrapperController.Display.cs`, and
  `StoreWrapper.cs`, so CS86xx diagnostics in all of them are promoted to errors by the nullable gate.
  The new partial part file must carry the pragma as well.
- **Constraint:** 500-line file cap; two touched files are already over it (see Decision A2).
- **Dependency:** none outside the repository. No new NuGet package. Moq, MSTest, and FluentAssertions
  are already referenced by `UtilitiesCS.Test`.
- **Test seam availability:** `IOlObjects.ArchiveRootPath` is an interface property
  (`UtilitiesCS/Interfaces/IGlobals/IOlObjects.cs:15`, reached via `IApplicationGlobals.Ol` at
  `IApplicationGlobals.cs:11`), so the throwing case is arranged by swapping `.Returns(...)` for
  `.Throws(new InvalidOperationException(...))` on the existing `SetupGet` in the fixtures at
  `FolderPredictorRecentsProjectionTests.cs:118-151`. No new production seam is required (A3).

## Data / API / Config Impact

- **User-facing changes:** (a) QuickFiler renders recents and suggestions as stored instead of throwing
  when the archive root is unresolvable; (b) the Folder Settings User Email retry no longer re-runs on
  repeated store re-selection within one dialog session.
- **Data or migration:** none. No serialized shape changes; `UserEmailAddress` keeps its `[JsonIgnore]`
  at `StoreWrapper.cs:173`.
- **Logging/telemetry:** one new warning, described above. No existing log line is removed or renamed.
- **Compatibility:** no public API break. The only signature change is on the private
  `ProjectSuggestionPath`.

## Test Strategy

The authoritative acceptance criteria are the single list under `## Acceptance Criteria` below. This
section states how they are exercised; it does not restate them.

### Defect A

- New file `UtilitiesCS.Test/OutlookObjects/Folder/FolderPredictorArchiveRootDegradationTests.cs`.
  Fixture derived from `FolderPredictorRecentsProjectionTests.cs:118-151` with
  `olObjects.SetupGet(x => x.ArchiveRootPath).Throws(new InvalidOperationException(...))`.
- Cases: recents-only (`Suggestions.Count == 0`, the issue's repro), suggestions-only, and both
  populated, across `FolderArray` and `FolderRowArray`. Each asserts `NotThrow()` and entry-for-entry
  identity with the stored strings.
- One case asserts the read/warn bound by `Verify` on the mocked getter.
- Text-parity case: the `FolderArray` strings and the `FolderRowArray` `Text` values match under a
  throwing root, pinning the contract at `FolderPredictor.cs:234-242`.
- One case in `ArchiveStemProjectionTests.cs` passes a literally `null` `archiveRoot`, closing the A5
  gap (the suite covers `string.Empty` at `:101` and whitespace at `:114`, but not `null`), so the
  degradation's dependency on `ArchiveStemProjection.cs:45-48` is pinned rather than implied.
- **The warning itself is not asserted by a test.** `logger.Warn` cannot be observed without mutating
  the process-global log4net repository, which General Unit Test Policy UT4 forbids; the only in-repo
  alternative is the injected-sink pattern at `OutlookFolderHierarchyProvider.cs:96-101`, which
  Decision A1 rejected along with the rest of that precedent. The warning is therefore verified by the
  read-count assertion (one accessor call per projection-helper invocation, hence one warning at most)
  plus code review of the single `logger.Warn` in the single `catch`.

### Defect B

- Tests added to `StoreWrapperController_Tests.Display.cs`, reusing `CreateControllerWithViewer()`
  (`StoreWrapperController_Tests.cs:160-178`) and `CreateDisplayFailingSmtpRootFolder(...)`
  (`StoreWrapperController_Tests.Display.cs:47-63`), which throws `COMException` from
  `PrimarySmtpAddress` and returns a non-at-sign `Address` so the whole chain yields null.
- Counting mechanism: `Mock.Verify` on the mocked `ExchangeUser.PrimarySmtpAddress` getter with
  `Times.Once()` / `Times.Never()` / `Times.Exactly(2)`. There is no invocation counter on
  `StoreWrapper`; the existing tests infer "no retry ran" indirectly from an unchanged value (B4), and
  the new tests replace that inference with direct verification.
- The three existing #797 AC6 tests (`Display.cs:66`, `:88`, `:111`) must remain green with no change
  other than the comment correction at `:90-92`.

### Determinism

Every test is pure Moq over interfaces and mocked Outlook interop: no live Outlook process, no
filesystem, no temporary file, no timer, no wall-clock read. `[DoNotParallelize]` already sits on
`StoreWrapperController_Tests` (`:14`) because several tests swap the process-global
`MyBox.DialogInvoker`; the new tests inherit it.

### Toolchain commands (exact, from `CLAUDE.md`; run in this order, restart from step 1 on any failure or any file rewrite)

1. `dotnet tool run csharpier format .` then `dotnet tool run csharpier check .`
   (`dotnet tool restore` once per worktree first; always via `dotnet tool run` so the
   manifest-pinned 1.2.6 is used).
2. `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
3. `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`
4. `vstest.console.exe <test-assembly-paths> /EnableCodeCoverage`

**`/t:Rebuild`, never `/t:Build`, for steps 2 and 3.** MSBuild's incremental up-to-date check compares
timestamps and does not invalidate on a command-line `/p:` change, so a warm `/t:Build` returns exit 0
with `CoreCompile` skipped on every project and runs no analyzers — the gate cannot fail. CI uses
`/t:Build` only because a runner checkout is always cold (`CLAUDE.md:201-202`, `:209-213`).

**Do not add `/p:Nullable=enable` to step 3.** No project carries a `<Nullable>` element and there is
no `Directory.Build.props`, so the property conscripts every file that never adopted the pragma; it
produced 195 errors in `UtilitiesCS.csproj` on 2026-08-10 against zero without it. Enforcement here is
per-file opt-in via `#nullable enable`.

### Test assemblies (C2)

- Primary: `UtilitiesCS.Test/bin/Debug/UtilitiesCS.Test.dll` — contains every test named above.
- Regression only: `QuickFiler.Test/bin/Debug/QuickFiler.Test.dll` (holds
  `Controllers/EfcDataModelArchiveRootTests.cs` and the `QfcItemController` folder-handling tests) and
  `TaskMaster.Test/bin/Debug/TaskMaster.Test.dll` (holds
  `AppGlobals/AppOlObjectsArchiveRootValidationTests.cs`, which pins the throwing behaviour this fix
  absorbs).

Assemblies must be named explicitly. Never discover by directory scan, or `.claude/worktrees/**` copies
are collected. Use `/InIsolation`; `vstest.console.exe` is not on `PATH` and is resolved through
`vswhere`.

### Known local test hazards — not regressions

1. **Four shell-icon test classes stall vstest on this machine** (P/Invoke `SHGetFileInfo`):
   `UtilitiesCS.Test.HelperClasses.ShellUtilities_Tests`,
   `UtilitiesCS.Test.HelperClasses.ShellUtilitiesStatic_Tests`,
   `UtilitiesCS.Test.HelperClasses.SysImageListHelperTests`, and
   `UtilitiesCS.Test.EmailIntelligence.OSBrowser_Tests`. The documented, reusable filter is recorded at
   `docs/features/active/2026-09-06-folder-settings-never-persist-and-user-email-error-loading-797/evidence/baseline/phase0-vstest.2026-09-06T22-00.md:29-38`:

   ```text
   TestCategory!=LiveOutlook&FullyQualifiedName!~HelperClasses.ShellUtilities_Tests&FullyQualifiedName!~HelperClasses.ShellUtilitiesStatic_Tests&FullyQualifiedName!~HelperClasses.SysImageListHelperTests&FullyQualifiedName!~EmailIntelligence.OSBrowser_Tests
   ```

   Precedence rule from the same evidence file (`:72-75`): `&` binds tighter than `|`, so
   `TestCategory!=LiveOutlook` must be repeated on **every** disjunct of an `|`-joined expression and a
   conjunctive exclusion binds only to the disjunct it appears in.
2. **`UtilitiesCS.Test.Extensions.DfDeedle_COM_Tests` carries a timing flake tracked as issue 803**,
   specifically `GetEmailDataInViewAsync_SeparatesTableSnapshotFromDataFrameTransform`. Protocol
   carried forward from #797 (`phase0-vstest.2026-09-06T22-00.md:50-54`): do **not** filter it out.
   Treat a failure as a known flake, re-run the scoped invocation once, record both attempts, and name
   issue 803.
3. `coverage/plan797-helpers.ps1` does not exist in this worktree (`coverage/` is git-ignored). The
   filter expression transfers; the runner script does not. TRX output carries `runUser` and
   `computerName` and must not be committed.
4. **`UtilitiesCS.Test.Extensions.DictionaryExtensions_Tests` carries a wall-clock timing flake
   tracked as issue 780**, specifically `TryAddValuesAsync_UpdatesExistingValue`. Dated amendment,
   2026-09-08, per the #812 plan D18. The mechanism is a fixed deadline in a production helper this
   change does not touch: `UtilitiesCS/Extensions/DictionaryExtensions.cs:177` calls
   `linkedTS.CancelAfter(500)` and the next line awaits work on that linked token, so under the
   class-level parallelism of a full-suite run the 500 ms elapses and the await throws
   `TaskCanceledException`. It passes in about 2 ms in an isolated scoped run. Do **not** filter it
   out. Treat a failure as a known flake, re-run the scoped invocation
   `FullyQualifiedName~DictionaryExtensions_Tests&TestCategory!=LiveOutlook` once, record both
   attempts, and name issue 780.

### Coverage targets

`CLAUDE.md` UT2: repository-wide line coverage `>= 80%` on the testable denominator; any new module,
class, or method `>= 90%`; changed lines must not lose coverage. The new accessor and the latch gate
are both fully reachable from unit tests, so no exemption is claimed for either.

## Acceptance Criteria

- [ ] **AC1 — Archive-root degradation across all three display-projection surfaces.** With
  `IOlObjects.ArchiveRootPath` arranged to throw `InvalidOperationException`, reading `FolderArray` and
  `FolderRowArray` completes without throwing and returns every suggestion and recent entry byte-identical
  to the stored string, in all three population states (recents only, suggestions only, both). Verified
  by the new test class `FolderPredictorArchiveRootDegradationTests` in
  `UtilitiesCS.Test/OutlookObjects/Folder/FolderPredictorArchiveRootDegradationTests.cs`, which must
  contain at least one `[TestMethod]` per population state per surface, each asserting `NotThrow()` and
  entry identity; plus a text-parity method asserting the `FolderArray` strings equal the
  `FolderRowArray` `Text` values under the throwing root; plus a new `[TestMethod]` in
  `ArchiveStemProjectionTests` passing a literally `null` `archiveRoot` and asserting the input is
  returned unchanged.

- [ ] **AC2 — One read and one warning per projection-helper invocation, and only
  `InvalidOperationException` absorbed.** With a throwing `ArchiveRootPath` arranged over 5 suggestions
  and 3 recents, a single `FolderArray` access invokes the `ArchiveRootPath` getter exactly twice
  (`Mock.Verify(..., Times.Exactly(2))` — once for `AddSuggestions`, once for `AddRecents`), and a
  single `FolderRowArray` access likewise exactly twice; a recents-only access invokes it exactly once.
  Separately, the new partial part file `UtilitiesCS/OutlookObjects/Folder/FolderPredictor.ArchiveRoot.cs`
  contains exactly one `catch` clause, whose exception type is `InvalidOperationException`, and exactly
  one `logger.Warn` call, whose message text contains no archive-root path and no mailbox address. A
  test arranging `ArchiveRootPath` to throw a `COMException` asserts that it propagates out of
  `FolderArray` rather than being absorbed.

- [ ] **AC3 — The five functional reads are not degraded.** After the change,
  `UtilitiesCS/OutlookObjects/Folder/FolderPredictor.cs` still contains the expression
  `_globals.Ol.ArchiveRootPath` at exactly five sites — the two `emailSearchRoots` seeds in
  `FindFolder` and `FindFolderRows`, the two `olAncestor` seeds in `CreateFolder` and
  `CreateFolderAsync`, and the `olAncestor` seed in `LoopFolders` — and none of those five is wrapped
  in a `try`/`catch` or routed through the new accessor. A test arranging a throwing `ArchiveRootPath`
  and invoking `FindFolder` with a null `emailSearchRoots` asserts that `InvalidOperationException` is
  still thrown, pinning the non-degradation. The existing `QuickFiler.Test` and `TaskMaster.Test`
  suites remain green with no edits.

- [ ] **AC4 — The User Email retry is bounded to one attempt per controller instance.** Using
  `CreateControllerWithViewer()` and `CreateDisplayFailingSmtpRootFolder(...)`, new `[TestMethod]`s in
  `StoreWrapperController_Tests` (file `StoreWrapperController_Tests.Display.cs`) assert: two
  consecutive `PopulateWithCurrent()` calls on one controller invoke the mocked
  `ExchangeUser.PrimarySmtpAddress` getter `Times.Once()`; a second controller constructed over the
  same failing store invokes it once more (`Times.Exactly(2)` cumulative, or `Times.Once()` on a fresh
  mock); and a store whose `UserEmailAddress` is already populated invokes it `Times.Never()`. No
  production line of this change is added to `StoreWrapperController.Launch()`, which remains
  `[ExcludeFromCodeCoverage]` and unmodified. The three existing #797 AC6 tests at
  `StoreWrapperController_Tests.Display.cs:66`, `:88`, and `:111` remain green with no assertion
  changed.

- [ ] **AC5 — Prose at four sites states the bound the code enforces.** Each of the following comments
  asserts, in its own words, that the retry is attempted at most once per `StoreWrapperController`
  instance and therefore once per dialog open because `RibbonController.FolderStoresSettings`
  constructs a fresh controller per open, and none of them asserts an unqualified "at most once per
  dialog open" as a property of a member that does not enforce it: the retry-gate comment in
  `UtilitiesCS/OutlookObjects/Store/StoreWrapperController.Display.cs`; the `RefreshUserEmailAddress`
  comment in `UtilitiesCS/OutlookObjects/Store/StoreWrapper.cs`, which must additionally state that
  this member itself re-runs the lookup on every call; the Arrange comment in
  `PopulateWithCurrent_WhenUserEmailIsAlreadyPopulated_DoesNotRetryLookup` in
  `UtilitiesCS.Test/OutlookObjects/Store/StoreWrapperController_Tests.Display.cs`; and the three
  passages (Non-Goals item 8, the AC6 detail, Risks item 1) of the #797
  `docs/features/active/2026-09-06-folder-settings-never-persist-and-user-email-error-loading-797/spec.md`,
  whose AC6-detail self-contradiction ("runs on every store re-selection" alongside "at most once per
  dialog open") must be resolved and whose edits must be marked as dated corrections referencing #812.
  Judged by the code-review audit against this statement. The five historical #797 artifacts named in
  Non-Goals items 3 and 4 — `plan.2026-09-06T22-00.md`,
  `research/research-folder-settings-persistence.md`,
  `evidence/issue-updates/issue-797.2026-09-06T22-00.md`, `code-review.2026-09-07T22-40.md`, and
  `feature-audit.2026-09-07T22-40.md` — are unmodified, verifiable by their absence from the branch
  diff.

- [ ] **AC6 — File-size and placement constraints hold.** The branch diff shows a new file
  `UtilitiesCS/OutlookObjects/Folder/FolderPredictor.ArchiveRoot.cs` declaring
  `public partial class FolderPredictor`; `UtilitiesCS/OutlookObjects/Folder/FolderPredictor.cs` has a
  line count no greater than its pre-change 1002; no file created or modified by this change other than
  `FolderPredictor.cs` exceeds 500 lines; and neither
  `UtilitiesCS.Test/OutlookObjects/Folder/FolderPredictorTests.cs` (1066 lines) nor
  `UtilitiesCS.Test/OutlookObjects/Store/StoreWrapperController_Tests.Launch.cs` (480 lines) appears in
  the diff.

- [ ] **AC7 — Full toolchain pass with non-vacuity and coverage.** In one uninterrupted pass in the
  order above: `dotnet tool run csharpier format .` followed by `dotnet tool run csharpier check .`
  reporting zero files needing formatting (the read-only check is the gate, not the formatter's exit
  code); both `msbuild ... /t:Rebuild ...` invocations reporting `0 Error(s)` and `0 Warning(s)`, with
  the captured log containing zero occurrences of `Skipping target "CoreCompile"` so the gates are
  demonstrably non-vacuous; and `vstest.console.exe` over `UtilitiesCS.Test.dll`, `QuickFiler.Test.dll`,
  and `TaskMaster.Test.dll` with `/EnableCodeCoverage`, `/InIsolation`, and the documented hazard filter,
  reporting zero failures other than a carve-out member handled under the issue-803 or issue-780
  protocol. Coverage of the new accessor and the latch gate is `>= 90%`; repository line coverage on the
  testable denominator is `>= 80%`. Evidence written under
  `docs/features/active/2026-09-07-utilitiescs-archive-root-read-and-user-email-retry-801-805-812/evidence/`.

## Risks & Mitigations

1. **The latch bound depends on one production construction site.** If a future caller reuses a
   controller across dialog opens, the bound silently becomes once-per-lifetime. Mitigation: an XML doc
   comment on the latch field naming `RibbonController.FolderStoresSettings` as the sole construction
   site and stating the dependency (Decision B2).
2. **A user can no longer force a retry by re-selecting the store.** Accepted, deliberately: the
   alternative is the unbounded blocking UI-thread COM chain that #797 never accepted. Mitigation:
   closing and reopening the dialog gives a fresh attempt, and the failure text already carries the
   captured reason.
3. **Defect A's user-visible outcome is not fully restored by this change alone.**
   `QfcItemController.FolderHandling.cs:233` still throws on the same call frame (Non-Goals item 2), so
   AC1 is verified at the `FolderPredictor` unit level rather than end to end in QuickFiler.
   Mitigation: file the follow-up defect before closing #812 and record the limitation in the PR body.
4. **`FolderPredictor.cs` remains twice over the 500-line cap.** This change does not worsen it (AC6)
   but does not fix it. Mitigation: none in scope; the partial-part remedy at
   `FolderPredictor.IFolderSearchHandler.cs:4-9` is the established containment.
5. **The logged warning is not directly asserted by a test.** Mitigation: AC2 pins the read count, which
   bounds the warning count, and pins the single `catch`/single `logger.Warn` shape by review. UT4
   forbids the process-global log4net mutation that direct assertion would require.
6. **Local test-run noise could be misread as regression.** Mitigation: the two hazards and their
   handling are recorded in Test Strategy above and must be quoted in the evidence artifact.

## Rollout & Follow-up

- **Release:** ships with the next add-in build. No configuration, migration, or operator action.
- **Manual verification:** (a) QuickFiler on a profile with no folder named `Archive` under the default
  store root — recents and suggestions render as stored paths, and one archive-root warning appears per
  projection in `TaskMaster\bin\Debug\logs\debug_<date>.log`; (b) Folder Settings on a mailbox whose
  Exchange lookup fails — exactly one `GetSmtpAddressFromStore` `[Startup timing]` chain per dialog
  open regardless of how many times the Display Name selection is cycled.
- **Follow-ups to file before closing #812:** the `QfcItemController.FolderHandling.cs:233` guard; the
  #797 CR-2 `SerializeNow` UI-thread file I/O and unbounded write-lock wait; the non-blocking Outlook
  COM read recorded as #797 Non-Goals item 8.
- **Close on merge with a pointer to this issue:** #801, #805.
- **Links:** issue https://github.com/drmoisan/TaskMaster/issues/812; supersedes #801 and #805; related
  #797 feature folder
  `docs/features/active/2026-09-06-folder-settings-never-persist-and-user-email-error-loading-797/`;
  research artifact `research/2026-09-07T23-45-utilitiescs-archive-root-and-user-email-retry-812-research.md`.
