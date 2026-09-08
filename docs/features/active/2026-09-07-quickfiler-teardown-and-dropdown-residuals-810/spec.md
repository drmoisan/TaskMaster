# quickfiler-teardown-and-dropdown-residuals (Spec)

- **Issue:** #810
- **Parent (optional):** none
- **Owner:** drmoisan
- **Last Updated:** 2026-09-07T22-40
- **Status:** Draft
- **Version:** 0.2
- **Work Mode:** `full-bug` — `spec.md` is the sole authoritative acceptance-criteria source. `user-story.md` is intentionally absent.

## Context

- **Summary of the bug and its impact.** Issue #810 consolidates the QuickFiler review residuals
  previously filed as #793 (from the #791 Cancel-teardown review) and #808 (from the #796 folder
  drop-down review). Both closed NOT_PLANNED at 2026-09-08T00:06Z so #810 is the single live tracker;
  their CLOSED state is not evidence that the work shipped. The lead defect is that the #796 AC2
  self-inflicted-deactivation guard inside `QfcFormController.ParkFocusAndCancelSelectors` also gates
  the #791 Cancel teardown caller, where the guard's predicate has no meaning. On that path the
  teardown's synchronous, ordered selector-cancel stage is skipped whenever a breadcrumb popup is
  open. Six further residuals sit in the same controllers and viewers.
- **Repro/playbook entries.** `issue.md` in this folder, "Steps to Reproduce" (four numbered
  reproductions). Source findings:
  `docs/features/active/2026-09-06-quickfiler-high-confidence-cancel-teardown-and-deadline-defects-791/code-review.2026-09-06T15-31.md`
  (N1, N2) and
  `docs/features/active/2026-09-06-quickfiler-folder-dropdown-closes-on-open-and-click-does-not-select-796/code-review.2026-09-07T17-05.md`
  (CR-1, CR-2, CR-3, CR-4, CR-7).
- **Observed environment(s).** Windows 11 Pro 10.0.26200; .NET Framework 4.8 VSTO add-in; `main` at
  `04a54e68` (PR #807 merge commit). Reproduced by QuickFiler ribbon launch followed by Cancel with a
  breadcrumb popup open.
- **Customer impact and severity.** Medium. All items are latent on the current tree. CR-3 is rated
  Major/latent by its reviewer because it touches the issue-677 keyboard-lock class; the popup is
  still closed later in teardown by `ItemViewer.ResetBreadcrumb` through a posted, fire-and-forget
  reset, so the observed effect today is a weakened ordering guarantee rather than a lost
  responsibility. N1 is unreachable only because `RibbonController` never calls
  `QfcHomeController.Cleanup()` directly. N2 requires a throwing viewer dispose.
- **First observed date and version(s) impacted.** Recorded 2026-09-06 (N1, N2) and 2026-09-07
  (CR-1 through CR-7) during the reviews of PR branches for issues #791 and #796. Both changes are
  merged to `main`, so the current `main` carries all seven residuals.

## Repro & Evidence

- **Steps to reproduce.**
  1. **AC1 / CR-3.** Open the folder drop-down on any item, then trigger Cancel. The `park-focus`
     teardown stage at `QuickFiler/Controllers/QfcFormController.EventHandlers.cs:144` calls
     `ParkFocusAndCancelSelectors`; the guard at
     `QuickFiler/Controllers/QfcFormController.Deactivate.cs:118` treats the open popup as a
     self-inflicted deactivation and returns before the per-item cancel loop.
  2. **AC3 / N1.** `QfcHomeController.Cleanup()` disposes `_tokenSource` at
     `QuickFiler/Controllers/QfcHomeController.cs:389` without nulling it.
     `QfcDatamodel.Cleanup()` and `QfcDatamodel.QuiesceLoaderAsync()` then call `Cancel()` on the same
     shared source, which raises `ObjectDisposedException` on .NET Framework 4.8.
  3. **AC4 / N2.** `QfcFormController.Cleanup()` invokes `_parentCleanup?.Invoke()` at
     `QuickFiler/Controllers/QfcFormController.SetupDisposal.cs:259` as its last statement with no
     `finally`. A throw from `_formViewer?.Dispose();` at `:251` skips the ribbon-release callback,
     and the ribbon buttons stay inert for the rest of the Outlook session.
  4. **AC5 / CR-2.** Open the popup, commit a selection (which sets `IsCommitPending`), then cause the
     next open to throw before `ShowPopup`. `RestoreAfterOpenFailure`
     (`QuickFiler/Viewers/BreadcrumbDropDownHost.cs:455-467`) calls `FinishClose(Uncommitted)`, the
     stale latch suppresses the cancel, and a selector session is left open with no popup.
- **Expected vs actual behavior.** Expected: the self-inflicted predicate applies only to the
  `Form.Deactivate` caller; the Cancel teardown's `park-focus` stage cancels every open selector
  synchronously; disposing the shared token source cannot make a later `Cancel()` throw; the
  ribbon-release callback runs under `finally` exactly once regardless of which stage threw; the
  commit-pending latch lives for exactly one popup lifetime on every path; comments match gate
  placement; no dead accessor remains; the AC2 producer has an automated test. Actual: as in the four
  reproductions above.
- **Logs/screenshots/error snippets.** None. All seven items are static review findings. The AC6
  close-ordering observation dated 2026-09-07T12-19 in the #796 feature folder `evidence/other/` is
  the closest runtime record.
- **Frequency / determinism.** Deterministic given the stated preconditions, but latent: each item
  requires a specific precondition (an open popup at Cancel; a second `Cleanup()` pass; a throwing
  viewer dispose; an open that fails after a commit) that the normal chain does not produce today.

## Scope & Non-Goals

### In scope

AC1 through AC8 as listed in the Acceptance Criteria section, delivered on one branch as seven code
changes plus the toolchain pass. The write set is enumerated under
"Files/modules to change".

### Out of scope / non-goals

- **Files owned by a concurrent run on issue 809. These MUST NOT be edited on this branch.**
  - `QuickFiler.Test/Controllers/QfcHomeControllerRunAsyncTests.cs`
  - `TaskMaster/ThisAddIn.cs`
  - `UtilitiesCS/Threading/UiThread.cs`

  None of the three is required. AC3's tests belong in
  `QuickFiler.Test/Controllers/QfcHomeControllerCleanupTests.cs`, which already owns
  `Cleanup_DisposesTokenSourceAndDetachesWorkerCompleted`; `QfcHomeControllerRunAsyncTests.cs`
  exercises `RunAsync`, not `Cleanup`, and its two `new QfcFormViewer()` occurrences are commented
  out, so the AC3 field change cannot affect it at compile time. The ribbon-release chain this issue
  touches terminates at `TaskMaster/Ribbon/RibbonController.ReleaseQuickFiler`, which is read for
  context only. `UiThread.Dispatcher` is used by `QfcFormController.EventHandlers.cs` and
  `QfcHomeController.cs` but only outside the changed statements.

- **`QuickFiler.Test/Controllers/QfcFormControllerDeactivateTests.cs` must stay byte-unmodified.**
  This file is the issue-677 / #796 regression fence. Its tests remaining green **while the file is
  unmodified** is itself the AC2 evidence; editing the file, even cosmetically, destroys that
  evidence. The two load-bearing cases are
  `FormDeactivated_SelfInflictedByOwnPopup_DoesNotCancelAnySelector` and
  `FormDeactivated_CancelsSelectorOnEveryItemController`.

- **Explicitly excluded systems and work.** The token-source ownership redesign (replacing the shared
  `CancellationTokenSource` handoff with `CancellationToken` parameters across `QfcDatamodel`,
  `QfcFormController`, `QfcCollectionController`, `QfcItemController` and `ProgressTracker`);
  `UtilitiesCS/Threading/ProgressViewer.cs`; `QuickFiler/Controllers/QfcDatamodel*.cs`; the
  `QfcDatamodel` coverage-exclusion question; any reordering of the #791 teardown stages; any change to
  the `"park-focus"` stage-name literal. See "Out of scope / report-only" for the report-only list.

## Root Cause Analysis

- **Confirmed root causes, one per AC.**
  1. **AC1.** `ParkFocusAndCancelSelectors` has exactly two callers and they ask different questions.
     `FormViewer_Deactivated` (`Deactivate.cs:27`) is handling a deactivation, so "was this
     deactivation caused by our own popup?" is meaningful. `RunTeardownStage("park-focus",
     ParkFocusAndCancelSelectors)` (`EventHandlers.cs:144`) has no deactivation event at all, so the
     predicate degenerates to "is any breadcrumb popup open?" — precisely the case the stage exists to
     handle. The guard is written once, at `Deactivate.cs:118`, and therefore applies to both.
  2. **AC3.** `QfcHomeController.Cleanup()` disposes `_tokenSource` at `:389` but nulls neither
     `_tokenSource` nor `_datamodel`, while nulling five sibling fields immediately below. A disposed
     source therefore stays reachable by `QfcDatamodel.Cleanup()`'s `_tokenSource?.Cancel()`.
  3. **AC4.** `_parentCleanup?.Invoke()` is the last statement of a straight-line method with three
     preceding statements that can throw (the COM-backed event-remove, `UnregisterFormEventHandlers()`,
     and `_formViewer?.Dispose()`). `_parentCleanup = null;` at `:260` is a real latch for a successful
     pass, but it does not run when an earlier statement throws, and it does not run when the callback
     itself throws — which `RunTeardownStage`'s catch makes reachable.
  4. **AC5.** The `IsCommitPending` latch has exactly one clear site, `ShowPopup`, which runs only
     when a native show actually begins. An open that fails before that point routes to
     `RestoreAfterOpenFailure` → `FinishClose(Uncommitted)` while the latch still carries the previous
     popup lifetime's committed state.
  5. **AC6.** The comment at `BreadcrumbDropDownHost.cs:450` states that the cancel step above always
     runs; since #796 that step is gated on the latch. `SearchOwnsDropDownDismissal` was added as an
     observation seam and never acquired a reader.
  6. **AC7.** The AC2 producer, `QfcFormViewer.IsDeactivationSelfInflictedByOwnPopup`, lives inside a
     class-level `[ExcludeFromCodeCoverage]` `Form`-derived type, so it is neither covered nor
     measurable.
- **Signals/evidence.** The two code-review artifacts named in Context, plus the research artifact
  `research/research.2026-09-07T22-10.md`, whose citations were spot-checked by the orchestrator.
- **Affected components/modules.** `QuickFiler/Controllers/` (`QfcFormController` partials,
  `QfcHomeController`, `QfcItemController.EventHandlers`), `QuickFiler/Viewers/`
  (`BreadcrumbDropDownHost` partials, `QfcFormViewer`), and the two legacy project files.

## Proposed Fix

### Design summary (what changes where)

Seven code changes on one branch:

1. **AC1 + AC2** — narrow the self-inflicted guard to the deactivation caller by making the condition
   caller-supplied instead of ambient.
2. **AC3** — null `_tokenSource` immediately after disposing it, and null `_datamodel` alongside its
   five sibling fields, so no later `Cancel()` can reach a disposed source.
3. **AC4** — wrap the body of `QfcFormController.Cleanup()` in `try` and invoke the ribbon-release
   callback from a `finally` that reads-and-clears the field before invoking.
4. **AC5** — clear the commit-pending latch inside `FinishClose`, as an operation in its `CompleteAll`
   list.
5. **AC6 (host half)** — replace the stale `FinishClose` comment with the corrected two-clause text
   plus a clause describing the new clear.
6. **AC6 (controller half)** — delete the dead `SearchOwnsDropDownDismissal` accessor and its XML doc.
7. **AC7** — extract the popup-owner registry out of the `[ExcludeFromCodeCoverage]` viewer into a
   host-neutral type so the AC2 derivation becomes directly unit-testable.

#### AC1 — the guard-narrowing design and its compiler constraint

Make the guard's applicability an explicit, **required** parameter:

```
internal void ParkFocusAndCancelSelectors(bool honourSelfInflictedGuard)
```

- `Deactivate.cs:27` becomes `ParkFocusAndCancelSelectors(honourSelfInflictedGuard: true)`.
- `EventHandlers.cs:144` becomes
  `RunTeardownStage("park-focus", () => ParkFocusAndCancelSelectors(honourSelfInflictedGuard: false));`
- The guard at `Deactivate.cs:118` gains one conjunct:
  `if (honourSelfInflictedGuard && _formViewer?.IsDeactivationSelfInflictedByOwnPopup == true)`.

**The parameter must be required, and the teardown call site must become an explicit lambda.**
`EventHandlers.cs:144` currently passes `ParkFocusAndCancelSelectors` as a **method group** converted
to `System.Action`. C# method-group conversion does not apply optional-argument defaults, so a
defaulted parameter would break that conversion with **CS0123**. This is a verified compiler
constraint, not a stylistic preference, and it is the single most likely first-attempt error.

The `"park-focus"` stage-name literal must not change: it is the marker
`QfcFormControllerCancelTeardownTests.MarkerParkFocus` compares against.

Rejected alternatives, recorded so they are not re-proposed: two entry points over a shared private
core (three members where one suffices, and three `<see cref>` references to re-point); a
caller-supplied enum (a new internal type to express a two-state decision with two total call sites).

#### AC4 — the `finally` shape

```
finally
{
    System.Action parentCleanup = _parentCleanup;
    _parentCleanup = null;
    parentCleanup?.Invoke();
}
```

Reading the field into a local and clearing it **before** invoking is what makes "exactly once"
unconditional. A plain `finally { _parentCleanup?.Invoke(); _parentCleanup = null; }` closes the
earlier-throw hole but not the callback-throws hole, because `:260` would still be skipped and a later
`Cleanup()` would invoke the callback a second time.

#### AC5 — where the latch is cleared, and why not in `RestoreAfterOpenFailure`

The clear belongs **inside `FinishClose`**, as the final operation in its `CompleteAll(params
Action[])` list. `FinishClose` is the single completion point of all three close paths, so clearing
there makes "the latch does not survive any close" a structural property rather than a per-call-site
obligation. Clearing only in `RestoreAfterOpenFailure` would fix the one path known to be broken today
and restate the invariant as a rule two other call sites must independently honour.

The clear must be an element of the `CompleteAll` operation list, **not** a statement after the
`CompleteAll` call: `CompleteAll` rethrows the first failing operation, so a trailing statement would
not run when an earlier operation throws.

### Design decision left open for the planner: file layout under the 500-line ceiling

`QuickFiler/Viewers/BreadcrumbDropDownHost.cs` and `QuickFiler/Controllers/QfcHomeController.cs` are
each exactly 496 lines. The production-code ceiling in `.claude/rules/general-code-change.md` is 500
lines, so each has four lines of headroom. Two designs are admissible and the planner must choose by
measurement, not by assumption:

- **Design A (in-place).** Add the latch clear and the corrected comment inside `FinishClose` where it
  stands. Admissible **only if** `BreadcrumbDropDownHost.cs` finishes at 500 lines or fewer **after**
  `dotnet tool run csharpier format .` has run.
- **Design B (relocate).** Move `FinishClose` (`:432-453`) and `RestoreAfterOpenFailure` (`:455-467`)
  into `QuickFiler/Viewers/BreadcrumbDropDownHost.Open.cs`, which already declares the
  `IsCommitPending` latch and owns its other clear site (`ShowPopup`). This co-locates the entire latch
  lifetime — declare, clear-on-show, read-and-clear-on-close — in one file. Relocation is safe: no test
  resolves either method by file path, and the one file-anchored assertion in this area names
  `BreadcrumbDropDownHost.Diagnostics.cs` for a different method and asserts against a
  reflection lookup on the type, which is file-agnostic.

**Decision rule.** The implementer measures the post-CSharpier line count of
`QuickFiler/Viewers/BreadcrumbDropDownHost.cs` and adopts **Design B if and only if Design A would
exceed 500 lines**. Apply the same measured rule to `QuickFiler/Controllers/QfcHomeController.cs`: its
AC3 change adds two statements, so the expected result is 498 lines and no relocation, but the
measurement is still required and must be recorded. Prefer the simplest design that satisfies the
ceiling, per the "Simplicity first" priority in `.claude/rules/general-code-change.md`. If a rationale
comment is wanted in `QfcHomeController.Cleanup()`, put it on the existing XML doc summary rather than
adding comment lines inside the method body.

The research artifact states that relocation is "required, not optional". That conclusion rests on a
line count taken by reading the file, not on a post-CSharpier measurement, and CSharpier had not been
run. This spec therefore treats relocation as conditional on the measurement above.

### Boundaries and invariants to preserve

#### The issue-677 keyboard-lock contract (leads; strictest constraint in this change set)

Two things must be true the moment activation leaves the QuickFiler form:

1. **No WebView2 child window may keep the shared Outlook UI thread's Win32 keyboard focus.** Outlook
   and QuickFiler share one UI thread, one input queue and one focus window. Once a WebView2 holds
   thread-wide focus the runtime does not reliably release it on click-back
   (MicrosoftEdge/WebView2Feedback #951, open upstream), so keystrokes typed into Outlook are consumed
   by the browser surface until QuickFiler closes.
2. **No breadcrumb `ToolStripDropDown` may stay open**, or WinForms modal menu mode keeps redirecting
   thread keyboard messages to the popup after the user has left.

**Enforcement sites.**

| Enforcement | Location |
|---|---|
| Deactivation routing | `QuickFiler/Interfaces/IQfcFormViewer.cs` (`FormDeactivated`) → `QuickFiler/Controllers/QfcFormController.Deactivate.cs:26-27`; subscribed at `QfcFormController.SetupDisposal.cs:175`, unsubscribed at `:204`. |
| Clause 1 — focus parking | `QfcFormController.Deactivate.cs:100-103` → `IQfcFormViewer.ParkFocusOffWebView2()` → `QuickFiler/Viewers/QfcFormViewer.cs:207`, gated on `IsWebView2Focused`. |
| Clause 2 — selector cancel | `QfcFormController.Deactivate.cs:123-147` → `IQfcItemController.CancelBreadcrumbSelector()` → `QfcItemController.FolderHandling.cs:161` → `ItemViewer.FolderSearch.cs:43` → `BreadcrumbCoordinator.CancelSelector()`. |
| Clause 1 — no re-steal after departure | `BreadcrumbDropDownHost.MayTakeFocus`, evaluated at execution time inside `FocusPending()` and `FocusAnchorIfPermitted()`; supplied by `ItemViewer.Breadcrumb.cs` as `MayRestoreBreadcrumbFocus`, true only when `Form.ActiveForm` is this form. |
| Test fences | `QuickFiler.Test/Controllers/QfcFormControllerDeactivateTests.cs` and `QuickFiler.Test/Viewers/BreadcrumbDropDownHostTests.Part3.cs`. |

**Polarity invariant (must not be inverted).** `QuickFiler/Interfaces/IQfcFormViewer.cs`, around
`:79-86`, documents that `false` from `IsDeactivationSelfInflictedByOwnPopup` means a **GENUINE**
deactivation. A viewer that reports nothing therefore keeps the issue-677 contract exactly. The
polarity is documented as load-bearing precisely because Moq's default `bool` return is `false`, which
makes an inversion silent in tests.

**The AC1 change preserves the contract by construction.** The extra condition is consumed only at
`Deactivate.cs:118`, and the deactivation caller supplies the value that keeps the guard active. For
the `Form.Deactivate` path, the guard's condition, its polarity, its placement **below** the
focus-parking step, and Moq's default-`false` behavior are unchanged. The falsifiable check is stated
in AC2: `QfcFormControllerDeactivateTests.cs` stays green **unmodified**.

**Prohibited changes.** Each of the following weakens the contract and is out of scope. If any is
found to be necessary, report it and stop rather than making it:

- Deleting the guard at `Deactivate.cs:118`, or making its condition unconditionally false. This
  restores the #796 defect: the popup a gesture just opened is cancelled by the deactivation that
  opening caused.
- Inverting `IsDeactivationSelfInflictedByOwnPopup`'s polarity, or changing the
  "reports nothing → `false`" default at `QfcFormViewer.cs:245-246`.
- Moving the guard **above** the focus-parking step at `Deactivate.cs:100-103`. #796 recorded this
  decision explicitly as `AC2-PARK-FOCUS-SUPPRESSED: NO`, on measured evidence that parking did not
  run on two of the three defective gestures.
- Touching `MayTakeFocus`, `FocusPending()`, `FocusAnchorIfPermitted()`, or the `ItemViewer` predicate
  wiring at `ItemViewer.Breadcrumb.cs`.
- Removing `ParkFocusOffWebView2()` from either path, or removing the `park-focus` stage from
  `ActionCancelAsync`.
- Reordering any `ActionCancelAsync` teardown stage, or changing the `"park-focus"` stage literal.

#### Other invariants

- **#791 teardown ordering.** `unregister-handlers` precedes `groups-cleanup`, and `quiesce-await`
  precedes `groups-cleanup`. Both are separately pinned by
  `QuickFiler.Test/Controllers/QfcFormControllerCancelTeardownTests.cs`.
- **No synchronous wait in `SetupDisposal.cs`.**
  `QfcFormControllerCleanupTests.Cleanup_SourceContainsNoSynchronousWait` reads the whole text of
  `QuickFiler/Controllers/QfcFormController.SetupDisposal.cs` and asserts it contains none of
  `.Wait(`, `.Result`, `Thread.Sleep`, `Task.Delay` after whitespace normalization. The AC4
  restructure introduces none of the four.
- **Nullable pragma parity.** `BreadcrumbDropDownHost.cs` and `BreadcrumbDropDownHost.Open.cs` both
  carry `#nullable enable`, so `/p:TreatWarningsAsErrors=true` promotes their `CS86xx` diagnostics to
  errors. The four controller files and `QfcFormViewer.cs` do not carry the pragma. The new
  `BreadcrumbPopupOwnerRegistry.cs` must carry `#nullable enable` to match the files it sits beside.
- **`_searchOwnedDismissal` stays live.** Deleting the accessor cannot regress the backing field to
  CS0649 or CS0169: the field is written at five sites and read at one, all of which remain.

### Dependencies or blocked work

- A **Phase 0 baseline Cobertura run** is a precondition for any changed-line coverage claim. No
  coverage artifact exists on this branch (see "Coverage impact").
- No external dependency, service, or release gates this work.

### Implementation strategy (what changes, not sequencing)

#### Files/modules to change

**Production**

| Path | AC | Change |
|---|---|---|
| `QuickFiler/Controllers/QfcFormController.Deactivate.cs` | AC1, AC2 | Required `honourSelfInflictedGuard` parameter at `:91`; conjunct at `:118`; extend the `Issue #796 (AC2)` comment block at `:111-117` with one sentence stating the predicate is meaningful only for a deactivation and is therefore supplied by the caller; update the call site at `:27`. |
| `QuickFiler/Controllers/QfcFormController.EventHandlers.cs` | AC1 | `:144` becomes a lambda passing `honourSelfInflictedGuard: false`. Stage literal `"park-focus"` unchanged. |
| `QuickFiler/Controllers/QfcHomeController.cs` | AC3 | `_tokenSource = null;` immediately after `_tokenSource?.Dispose();` at `:389`; `_datamodel = null;` alongside the sibling nullings at `:390-394`. **496 lines — measure against the ceiling.** |
| `QuickFiler/Controllers/QfcFormController.SetupDisposal.cs` | AC4 | `try` around `:215-258`; read-and-clear `finally` replacing `:259-260`. |
| `QuickFiler/Viewers/BreadcrumbDropDownHost.cs` | AC5, AC6 | Latch clear and corrected comment; under Design B, `FinishClose` (`:432-453`) and `RestoreAfterOpenFailure` (`:455-467`) move out. **496 lines — the layout decision applies here.** |
| `QuickFiler/Viewers/BreadcrumbDropDownHost.Open.cs` | AC5, AC6 | Update the latch lifetime doc at `:102-107` to record that `FinishClose` also clears it. Under Design B, receive both relocated methods. |
| `QuickFiler/Controllers/QfcItemController.EventHandlers.cs` | AC6 | Delete `:205-209` — the XML doc block and the dead `SearchOwnsDropDownDismissal` accessor. |
| `QuickFiler/Viewers/QfcFormViewer.cs` | AC7 | Replace `:212-213`, `:226-234`, `:245-246` with one registry field and two forwarding members. |
| `QuickFiler/Viewers/BreadcrumbPopupOwnerRegistry.cs` | AC7 | **New file.** `#nullable enable`, namespace `QuickFiler.Viewers`, no `[ExcludeFromCodeCoverage]`. |
| `QuickFiler/QuickFiler.csproj` | AC7 | Add `<Compile Include="Viewers\BreadcrumbPopupOwnerRegistry.cs" />` near `:415-419`. |

**Test**

| Path | AC | Change |
|---|---|---|
| `QuickFiler.Test/Controllers/QfcFormControllerCancelTeardownTests.cs` | AC1 | One added test, `ActionCancelAsync_SelfInflictedByOwnPopup_StillCancelsEverySelector`. Currently 393 lines. |
| `QuickFiler.Test/Controllers/QfcHomeControllerCleanupTests.cs` | AC3 | Extend `Cleanup_DisposesTokenSourceAndDetachesWorkerCompleted` with a second `Cleanup()` asserting no throw; add a test asserting `TokenSource` is null after cleanup. Currently 118 lines. |
| `QuickFiler.Test/Controllers/QfcFormControllerCleanupTests.cs` | AC4 | One added test, `Cleanup_ViewerDisposeThrows_StillInvokesParentCleanupOnce`. Currently 399 lines. |
| `QuickFiler.Test/Viewers/BreadcrumbDropDownCloseOrderingTests.cs` | AC5 | One added test using the existing `CloseOrderingHostHarness`; one added latch-cleared assertion appended to `NativeCloseWhileCommitPending_DoesNotCancelSelection`. Currently 290 lines. |
| `QuickFiler.Test/Viewers/BreadcrumbPopupOwnerRegistryTests.cs` | AC7 | **New file**, six cases. |
| `QuickFiler.Test/QuickFiler.Test.csproj` | AC7 | Add `<Compile Include="Viewers\BreadcrumbPopupOwnerRegistryTests.cs" />`. |

**Deliberately NOT changed:** `QuickFiler.Test/Controllers/QfcFormControllerDeactivateTests.cs`;
`QuickFiler.Test/Controllers/QfcHomeControllerRunAsyncTests.cs`; `TaskMaster/ThisAddIn.cs`;
`UtilitiesCS/Threading/UiThread.cs`; `UtilitiesCS/Threading/ProgressViewer.cs`;
`QuickFiler/Controllers/QfcDatamodel*.cs`.

**Legacy project-file constraint.** Both `QuickFiler/QuickFiler.csproj` and
`QuickFiler.Test/QuickFiler.Test.csproj` are non-SDK projects with explicit `<Compile Include>` items.
A new file that is not registered compiles to nothing and its tests silently do not run. Add each
`<Compile Include>` entry in the same task as the file it registers.

#### Functions/classes/CLI commands impacted

`QfcFormController.ParkFocusAndCancelSelectors` (signature change),
`QfcFormController.FormViewer_Deactivated`, `QfcFormController.ActionCancelAsync` (the `park-focus`
stage body only), `QfcFormController.Cleanup`, `QfcHomeController.Cleanup`,
`BreadcrumbDropDownHost.FinishClose`, `BreadcrumbDropDownHost.RestoreAfterOpenFailure` (under Design B
only), `QfcItemController.SearchOwnsDropDownDismissal` (deleted),
`QfcFormViewer.SetBreadcrumbPopupOwner` and `QfcFormViewer.IsDeactivationSelfInflictedByOwnPopup`
(both reduced to forwarding members), and the new `BreadcrumbPopupOwnerRegistry`. No CLI command is
affected.

#### Data flow and validation changes

- The self-inflicted predicate becomes caller-supplied rather than ambient. Its evaluation, polarity,
  and position within `ParkFocusAndCancelSelectors` are otherwise unchanged.
- `IsCommitPending` gains a second clear site. Its read site is unchanged and continues to be
  evaluated before the clear within the same `FinishClose` call, so paths 1 and 2 behave identically.
- The popup-owner registry's validation moves with it: the existing null-ignore guard for a null
  `itemViewer` or a null `popupIsOpen` must be preserved, and re-registering the same key must
  continue to **replace** rather than append — that is the documented reason the store is a keyed
  dictionary.

#### Error handling and logging updates

- AC4 changes which thread of control reaches the ribbon-release callback but adds no new log line. A
  throw from the callback now propagates out of `Cleanup()` into `RunTeardownStage`'s ERROR log with
  the `controller-cleanup` stage name, rather than being lost.
- No log message text, level, or stage-name literal changes.
- The unconditional "ribbon release callback invoked" log lines at
  `QfcFormController.EventHandlers.cs:171` and `QfcHomeController.cs:404` remain misleading in the
  throwing case. That is report-only item 7; do not change them here.

#### Rollback/feature-flag considerations (if applicable)

Not applicable. All seven changes are small, self-contained source edits with no feature flag, no
configuration switch, and no persisted state. Rollback is a branch revert.

### Technical specifications (interfaces/contracts)

#### Inputs/outputs and formats

- `internal void ParkFocusAndCancelSelectors(bool honourSelfInflictedGuard)` — `true` from the
  `Form.Deactivate` caller, `false` from the Cancel teardown stage. Required parameter; no default.
- `BreadcrumbPopupOwnerRegistry` — `Register(Control itemViewer, Func<bool> popupIsOpen)` (null
  arguments ignored; same key replaces) and a boolean `AnyOpen` carrying the
  `Values.Any(popupIsOpen => popupIsOpen())` derivation. No `Unregister` is added on this branch.
- No serialized format, wire format, or file format changes.

#### Required configuration keys and defaults

None. No configuration key is added, removed, or re-defaulted.

#### Backward-compatibility expectations

- `ParkFocusAndCancelSelectors` is `internal`. `QuickFiler.Test` is the only external consumer via
  `[assembly: InternalsVisibleTo("QuickFiler.Test")]`, and no test invokes the method directly, so the
  signature change breaks no caller outside the two in-repo sites.
- `IQfcFormViewer` and `IFilerHomeController` public surfaces are unchanged.
- `SearchOwnsDropDownDismissal` is `internal` with zero readers anywhere, so its deletion is not a
  breaking change.

#### Performance constraints (latency/throughput/memory)

No measurable change is expected. AC1 restores work that the teardown was designed to do, so the
Cancel path may do slightly more synchronous work when a popup is open; this is the intended #791
ordering and is bounded by the item count. AC3 and AC4 add field writes and a `try`/`finally`. AC7
replaces a dictionary field with an object holding the same dictionary.

## Assumptions, Constraints, Dependencies

- **Assumptions.** The working tree matches the line coordinates cited here, which were re-derived by
  reading each file. `QuickFiler.Test` has internals visibility into `QuickFiler`. Outlook is not
  running during builds and tests.
- **Constraints.**
  - 500-line production-code ceiling (`.claude/rules/general-code-change.md`), binding on
    `BreadcrumbDropDownHost.cs` and `QfcHomeController.cs` at 496 lines each.
  - `QfcFormControllerCleanupTests.cs` at 399 lines and `QfcFormControllerCancelTeardownTests.cs` at
    393 lines each have room for one added test; a second addition to either would need a
    partial-class split, for which `QfcStreamingDequeueConfidenceGateTests` (four `Part` files) is the
    established pattern.
  - Legacy non-SDK project files require explicit `<Compile Include>` entries.
  - Do not mark `_tokenSource` `volatile`; prior work in this repository recorded a CS0420 build break
    from a field of this shape, and no memory barrier is required here.
- **External dependencies.** None. MSTest, Moq, FluentAssertions and
  `Microsoft.Extensions.Time.Testing` are already referenced.

## Data / API / Config Impact

- **User-facing or API changes.** None. No public API, ribbon control, dialog, or user-visible string
  changes. The only behavioral difference a user could observe is that a breadcrumb popup open at
  Cancel is now closed synchronously in the ordered `park-focus` stage instead of later through the
  posted `ResetBreadcrumb` chain.
- **Data or migration considerations.** None. No persisted state, settings key, or schema is touched.
- **Logging/telemetry updates.** None; see "Error handling and logging updates".
- **Compatibility notes.** No CLI flag, config schema, or version marker changes.

## Test Strategy

### Order of work — CLAUDE.md Bugfix Workflow (mandatory)

For every AC that can carry one, write the failing regression test **first**, confirm it fails against
the unmodified production code, then make the minimal targeted fix, then confirm it passes. Do not
write the fix first and the test afterwards. AC2 and AC6 are the two criteria that cannot carry a
new failing test, for the reasons given in the AC table.

### Regression tests to add or update

| AC | Test | Home | Fails before the fix because |
|---|---|---|---|
| AC1 | `ActionCancelAsync_SelfInflictedByOwnPopup_StillCancelsEverySelector` — a near-clone of the existing `ActionCancelAsync_ParksFocusAndCancelsBreadcrumbSelectors` with one added Arrange line, `_mockFormViewer.SetupGet(x => x.IsDeactivationSelfInflictedByOwnPopup).Returns(true);` | `QuickFiler.Test/Controllers/QfcFormControllerCancelTeardownTests.cs` | The guard returns before the per-item loop, so the cancel count is 0. |
| AC2 | None added. `QfcFormControllerDeactivateTests.cs` stays byte-unmodified. | — | Not applicable — this is a preservation criterion, evidenced by an unmodified file staying green. |
| AC3 | Extend `Cleanup_DisposesTokenSourceAndDetachesWorkerCompleted` with a second `Cleanup()` asserting no throw; add a test asserting `TokenSource` is null after cleanup. | `QuickFiler.Test/Controllers/QfcHomeControllerCleanupTests.cs` | The second pass re-enters `_datamodel.Cleanup()` → `Cancel()` on a disposed source. |
| AC4 | `Cleanup_ViewerDisposeThrows_StillInvokesParentCleanupOnce` — `_mockFormViewer.Setup(x => x.Dispose()).Throws(...)`; assert the parent callback ran exactly once and that a second `Cleanup()` does not run it again. | `QuickFiler.Test/Controllers/QfcFormControllerCleanupTests.cs` | `_parentCleanup?.Invoke()` at `:259` is skipped by the throw at `:251`. |
| AC5 | `RestoreAfterOpenFailure_WithStaleCommitPending_StillCancelsAndClearsLatch`, using the existing `CloseOrderingHostHarness`; plus a latch-cleared assertion appended to `NativeCloseWhileCommitPending_DoesNotCancelSelection`. | `QuickFiler.Test/Viewers/BreadcrumbDropDownCloseOrderingTests.cs` | The stale latch suppresses `_cancelSelection`, so the cancel count is 0. |
| AC6 | None. Deletion and a comment correction; the analyzer and nullable rebuilds are the evidence. | — | Not applicable. |
| AC7 | New `QuickFiler.Test/Viewers/BreadcrumbPopupOwnerRegistryTests.cs`, six cases. | New file | The type does not exist. |

### Unit tests (MSTest) for the fixed behavior and boundaries

Framework and libraries are fixed by CLAUDE.md: **MSTest**, **Moq**, **FluentAssertions**. Available
seams, all already established in this test project:

- **Controller layer.** `Mock<IQfcFormViewer>`, `Mock<IQfcHomeController>`, `Mock<IQfcDatamodel>`,
  `Mock<IQfcCollectionController>`, `Mock<IQfcItemController>`, `Mock<IQfcKeyboardHandler>`, plus
  private-field injection by reflection. `new Control.ControlCollection(new Control())` with an empty
  exclusion list satisfies the guard at the top of `Register`/`UnregisterFormEventHandlers` **without
  creating a window handle** — this is the established headless idiom.
- **Host layer.** The `BreadcrumbDropDownHost` constructor overload taking a `LegacySurfaceFactory`
  plus explicit `focusPending` / `focusAnchor` / `cancelSelection` / `showPopup` delegates, driven
  under an `InlineSynchronizationContext`.
- **Determinism.** `FakeTimeProvider`, injected `Action<string>` log sinks, `TaskCompletionSource`, and
  the `CapturingSynchronizationContext` pattern for observing `async void` escapes. Per
  `.claude/rules/general-unit-test.md`, `Thread.Sleep`, `Task.Delay`, real wall-clock waits, and
  temporary files are prohibited in tests.

### Edge cases and negative scenarios

AC7's six cases are the negative and boundary matrix for the registry: no registration → `false` (the
GENUINE case, and the load-bearing polarity); one registration returning `false` → `false`; one
returning `true` → `true`; two registrations with one `true` → `true`; re-registering the same key
replaces rather than appends; a null `itemViewer` or a null `popupIsOpen` is ignored. AC3's second
`Cleanup()` pass and AC4's throwing-dispose case are the negative scenarios for the teardown path.
AC5's stale-latch case is the negative scenario for the close path.

### Error handling and logging verification

AC4's test must assert the callback ran exactly once **and** that a second `Cleanup()` does not run it
again, which is the observable form of the read-and-clear `finally`. No log-message assertion is added;
no log text changes.

### Coverage impact and targets for changed lines/modules

- The repository line-coverage floor is **85%** and the branch-coverage floor is **75%**, per
  `.claude/rules/general-unit-test.md` and `.claude/rules/quality-tiers.md`. Both apply uniformly
  across tiers T1–T4.
- **Changed lines must not regress coverage.** A Phase 0 baseline Cobertura run is required before any
  changed-line coverage claim: **no coverage artifact exists on this branch for issue #810.**
  `SearchScope:` the whole worktree plus
  `docs/features/active/2026-09-07-quickfiler-teardown-and-dropdown-residuals-810/**` and
  `docs/features/active/2026-09-06-quickfiler-folder-dropdown-closes-on-open-and-click-does-not-select-796/evidence/**/*.xml`.
  `SearchPatterns:` `**/coverage*.xml`, plus a full enumeration of both feature folders.
  `SearchResult:` none applicable — the 810 folder had no `evidence/` directory, and the 796 folder's
  coverage evidence is markdown-only. Absence of the baseline must produce a BLOCKED or INCOMPLETE
  verdict rather than an unsupported claim.
- **`QuickFiler/Viewers/BreadcrumbPopupOwnerRegistry.cs` is a new module and must carry no
  `[ExcludeFromCodeCoverage]` attribute.** Its entire purpose is to make the AC2 producer measurable;
  excluding it would defeat the criterion. As a new module it targets the higher new-code bar and must
  be fully exercised by its six cases.
- **Expect a recurring per-file disposition.** `QfcFormController.EventHandlers.cs` (58.12%) and
  `QfcHomeController.cs` (76.36%) were both below the per-file floor at the #791 head and are changed
  again here. Record the FAIL-but-non-blocking disposition under the ratified CLAUDE.md UT2 exemption
  class (c) in the policy audit rather than discovering it at review time. Those two figures come from
  the #791 review artifact and are secondary evidence, not a baseline for this branch.
- `QfcFormViewer.cs` and `ItemViewer.cs` carry class-level `[ExcludeFromCodeCoverage]` and therefore
  emit no `<class>` element at all in Cobertura; absence of a method element is the exemption signal,
  not a zero.

### Toolchain commands to run (format → lint → type-check → test)

Run in this exact order. If any step fails or auto-fixes any file, restart from step 1.

1. `dotnet tool run csharpier format .` (verify with `dotnet tool run csharpier check .`; always via
   `dotnet tool run`, never a global install)
2. `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
3. `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`
4. `vstest.console.exe <test-assembly-paths> /EnableCodeCoverage`

Binding constraints on these commands:

- Use `/t:Rebuild`, **never** `/t:Build`. MSBuild's up-to-date check does not invalidate on a
  command-line `/p:` change, so a warm `/t:Build` returns exit 0 with `CoreCompile` skipped on every
  project and runs no analyzers.
- **Never add `/p:Nullable=enable`.** Nullable enforcement in this repository is per-file opt-in via
  the `#nullable enable` pragma; the solution-wide property conscripts files that never adopted it and
  the gate can never pass.
- **Do not launch Outlook.** No step in this workflow requires a live Outlook process.
- **An MSB3061 file-lock warning is to be reported, not resolved by terminating a process.** If the
  build output is locked, report the condition and stop; do not kill Outlook or any other process.

### Manual validation steps (if required)

One live-Outlook runbook step, to be executed by the maintainer rather than by an agent, to close the
R2 risk: open QuickFiler, open a breadcrumb popup, click Cancel, and confirm that Outlook keyboard
input works immediately afterwards.

## Acceptance Criteria

Acceptance criteria are AC1 through AC8, worded exactly as in `issue.md`. `spec.md` is the
authoritative source for this `full-bug` work mode; the list in `issue.md` mirrors it.

| AC | Criterion | Production change that satisfies it | Test(s) that pin it | Evidence artifact kind | Automatable end to end? |
|---|---|---|---|---|---|
| AC1 | The AC2 self-inflicted-deactivation guard is scoped to the `Form.Deactivate` caller only, so the Cancel teardown's `park-focus` stage cancels every open selector synchronously. | Required `honourSelfInflictedGuard` parameter on `ParkFocusAndCancelSelectors` (`QfcFormController.Deactivate.cs:91`), conjunct at `:118`, `true` at `:27`, lambda passing `false` at `QfcFormController.EventHandlers.cs:144`. | `ActionCancelAsync_SelfInflictedByOwnPopup_StillCancelsEverySelector` (`QfcFormControllerCancelTeardownTests.cs`) | `evidence/regression-testing/` (fail-before, pass-after) | Yes, end to end at the controller seam. |
| AC2 | The issue-677 keyboard-lock contract is preserved for a genuine deactivation; no change weakens it. | No production change. Preservation is a property of the AC1 design: the extra condition is consumed only at `Deactivate.cs:118` and the deactivation caller supplies `true`. | `QuickFiler.Test/Controllers/QfcFormControllerDeactivateTests.cs`, **byte-unmodified and green**, notably `FormDeactivated_SelfInflictedByOwnPopup_DoesNotCancelAnySelector` and `FormDeactivated_CancelsSelectorOnEveryItemController`. | `evidence/regression-testing/` (run record) plus `evidence/other/` recording the file's unchanged status in the branch diff | Partially. The suite runs automatically, but the "file unmodified" half of the evidence is a diff check, not a test assertion. |
| AC3 | `QfcHomeController` no longer leaves a disposed shared `CancellationTokenSource` reachable by later `Cancel()` callers. | `_tokenSource = null;` immediately after `_tokenSource?.Dispose();` (`QfcHomeController.cs:389`); `_datamodel = null;` alongside the sibling nullings at `:390-394`. | Extended `Cleanup_DisposesTokenSourceAndDetachesWorkerCompleted` plus a `TokenSource`-is-null test (`QfcHomeControllerCleanupTests.cs`) | `evidence/regression-testing/` | Yes, end to end. |
| AC4 | `QfcFormController.Cleanup()` invokes the ribbon-release callback under `finally`, exactly once, regardless of which earlier stage threw. | `try` around `SetupDisposal.cs:215-258`; read-and-clear `finally` replacing `:259-260`. | `Cleanup_ViewerDisposeThrows_StillInvokesParentCleanupOnce` (`QfcFormControllerCleanupTests.cs`) | `evidence/regression-testing/` | Yes, end to end at the controller seam with a throwing `Mock<IQfcFormViewer>.Dispose()`. |
| AC5 | The commit-pending latch is cleared on consumption in `BreadcrumbDropDownHost.RestoreAfterOpenFailure`, so it lives for exactly one popup lifetime on every path. | Clear `IsCommitPending` as the final operation in `FinishClose`'s `CompleteAll` list — the single completion point through which `RestoreAfterOpenFailure` and both other close paths pass. | `RestoreAfterOpenFailure_WithStaleCommitPending_StillCancelsAndClearsLatch` plus a latch-cleared assertion on `NativeCloseWhileCommitPending_DoesNotCancelSelection` (`BreadcrumbDropDownCloseOrderingTests.cs`) | `evidence/regression-testing/` | Yes, end to end via the existing `CloseOrderingHostHarness` under an `InlineSynchronizationContext`. |
| AC6 | The stale `FinishClose` comment is corrected and the dead `SearchOwnsDropDownDismissal` accessor is removed. | Replace the comment at `BreadcrumbDropDownHost.cs:450` with the corrected two-clause text plus a clause describing the new clear; delete `QfcItemController.EventHandlers.cs:205-209`. | No new test. The analyzer build and the nullable build are the evidence that the deletion compiles and produces no CS0649/CS0169 on the retained backing field. | `evidence/qa-gates/` (analyzer and nullable build records) | No. Comment text and deletion are verified by build and diff review, not by an executable assertion. |
| AC7 | The AC2 self-inflicted-deactivation producer has automated test coverage. | Extract `QuickFiler/Viewers/BreadcrumbPopupOwnerRegistry.cs` (new, `#nullable enable`, no `[ExcludeFromCodeCoverage]`); reduce `QfcFormViewer.cs:212-246` to one field and two forwarding members; register both new files in the two `.csproj` files. | New `QuickFiler.Test/Viewers/BreadcrumbPopupOwnerRegistryTests.cs`, six cases (no registration; one `false`; one `true`; two with one `true`; same-key replace; null arguments ignored). | `evidence/regression-testing/` plus `evidence/qa-gates/` (coverage delta showing the new module measured) | **Seam only.** The derivation becomes testable only after the registry is extracted, and the wiring hop at `ItemViewer.Breadcrumb.cs:216` (`FindForm() as QfcFormViewer`) remains untestable without a real form hierarchy. AC7 covers the derivation, not the wiring; the residual is stated rather than implied. |
| AC8 | Full C# toolchain pass completed in order (CSharpier, msbuild analyzers, msbuild nullable, vstest with coverage) with no regression. | None — a process criterion over the whole change set. | The whole `QuickFiler.Test` suite, run under `vstest.console.exe /EnableCodeCoverage`. | `evidence/qa-gates/` (one artifact per gate, each carrying `Timestamp`, `Command`, `EXIT_CODE`) | Yes, end to end, subject to the Outlook-not-running and MSB3061 constraints in Test Strategy. |

Checkbox form, for the acceptance-criteria-tracking protocol:

- [x] AC1: The AC2 self-inflicted-deactivation guard is scoped to the `Form.Deactivate` caller only, so the Cancel teardown's `park-focus` stage cancels every open selector synchronously.
- [x] AC2: The issue-677 keyboard-lock contract is preserved for a genuine deactivation; no change weakens it.
- [x] AC3: `QfcHomeController` no longer leaves a disposed shared `CancellationTokenSource` reachable by later `Cancel()` callers.
- [x] AC4: `QfcFormController.Cleanup()` invokes the ribbon-release callback under `finally`, exactly once, regardless of which earlier stage threw.
- [x] AC5: The commit-pending latch is cleared on consumption in `BreadcrumbDropDownHost.RestoreAfterOpenFailure`, so it lives for exactly one popup lifetime on every path.
- [x] AC6: The stale `FinishClose` comment is corrected and the dead `SearchOwnsDropDownDismissal` accessor is removed.
- [x] AC7: The AC2 self-inflicted-deactivation producer has automated test coverage.
- [x] AC8: Full C# toolchain pass completed in order (CSharpier, msbuild analyzers, msbuild nullable, vstest with coverage) with no regression.

### Numeric assertions and their basis

The following counts are asserted in this spec and are licensed by the `## Numeric Derivation
Evidence` section of `research/research.2026-09-07T22-10.md`, each derived twice by independent search
strategies with independently enumerated and explicitly compared member sets:

- `ParkFocusAndCancelSelectors` has **exactly two** callers (family 1: identifier sweep vs. structural
  teardown-stage-plus-event-subscription sweep with a reflective-lookup check; both sets
  `{EventHandlers.cs:144, Deactivate.cs:27}`).
- `SearchOwnsDropDownDismissal` has **zero** non-declaration source references (family 2: exact
  identifier sweep vs. case-insensitive prefix sweep plus a reflective-lookup sweep; both sets empty).
- `IsCommitPending` has **four** production touch points (family 3).
- `FinishClose` has **three** call sites (family 4).

Counts deliberately **not** asserted, because no derivation record supports them: the number of tests
in `QfcFormControllerDeactivateTests.cs`. That file is referred to here by its role — the issue-677 /
#796 regression fence — and by named test methods, not by a test count.

The 496-line figures for `BreadcrumbDropDownHost.cs` and `QfcHomeController.cs` are stated as
pre-change measurements independently verified by the orchestrator, and the layout decision rule
requires the implementer to re-measure after CSharpier rather than relying on them.

## Out of scope / report-only

These items are to be **recorded for follow-up promotion through the issue lifecycle, not fixed on
this branch.** Prose in a feature folder does not survive merge; each item needs a real issue.

1. **`UtilitiesCS/Threading/ProgressViewer.cs:75` — un-null-guarded `Cancel()` on the shared source.**
   `_cancelSource!.Cancel()` runs from the progress dialog's Cancel button on the same
   `CancellationTokenSource` created at `QfcHomeController.cs:54`. It is the only `Cancel()` on that
   instance that is neither null-guarded nor ordered by teardown, and it is a fourth sharer the #791
   review did not enumerate. AC3's nulling does not affect it, because it holds its own captured
   reference assigned at construction. **The file must not be changed on this branch.**
2. **Token-source ownership redesign.** Replacing the shared `CancellationTokenSource` handoff with
   `CancellationToken` parameters across `QfcDatamodel`, `QfcFormController`,
   `QfcCollectionController`, `QfcItemController` and `ProgressTracker` is the structural fix for the
   whole family. Multi-file API change.
3. **`QfcDatamodel` coverage exclusion.** The class-level `[ExcludeFromCodeCoverage]` at
   `QfcDatamodel.cs:25` removes `QuiesceLoaderAsync`, `TryQueueRemainingMailItemAsync` and the whole
   queue-processing partial from the coverage denominator. The #791 review already recommended
   extracting the host-neutral logic. The direct conflict between `.claude/rules/general-unit-test.md`'s
   Coverage Exclusion Policy and CLAUDE.md UT2's ratified exemption pre-exists this branch.
4. **CR-6 — reflection by private field name in the #796 AC4 re-pin.**
   `QfcItemController.SearchDismissalTests.cs:85` sets `_searchOwnedDismissal` by string literal.
   Driving the real `TextBoxSearch_TextChanged` path instead would remove the coupling. Not required
   by AC6, which asks for removal of the accessor.
5. **CR-5 — `_breadcrumbPopupOwners` entries are never removed.** Growth is bounded by the item-viewer
   pool and a stale entry cannot wrongly report `true`, because a disposed host leaves `OpenState`
   false. If the AC7 extraction lands, the new registry is the natural home for an eventual
   `Unregister`; do not add one speculatively here.
6. **`ItemViewer.Breadcrumb.cs:216` registration hop remains untested.** `FindForm() as QfcFormViewer`
   needs a real form hierarchy.
7. **#791 N12 — misleading unconditional log lines.** `QfcFormController.EventHandlers.cs:171` and
   `QfcHomeController.cs:404` both log "ribbon release callback invoked" unconditionally, including
   when the callback did not run. AC4 makes the first true more often but not always, because a
   `RunTeardownStage` catch can still swallow a throw.
8. **The reverse-ordering limit of the commit-pending latch.** A native uncommitted-reason close
   arriving **before** the commit's `Close(ExplicitCommit)` would still cancel. AC5 does not change
   that and must not be read as doing so.
9. **`QuickFiler.Test/QuickFiler.Test.csproj` exceeds 500 lines.** Pre-existing. The ceiling rule
   enumerates "production code, test code, or reusable script file", and an MSBuild project file is
   none of these. AC7 adds one `<Compile Include>` line. Recorded so it is not read as a new violation.

## Risks & Mitigations

- **R1 — Weakening the issue-677 keyboard-lock contract.** The highest-consequence risk in this change
  set. **Mitigation:** the prohibited-change list under "Boundaries and invariants to preserve", plus
  the falsifiable check that `QfcFormControllerDeactivateTests.cs` stays green byte-unmodified.
- **R2 — Un-suppressing the cancel loop on the Cancel path re-enters the focus machinery.** After AC1,
  the `park-focus` stage will, with a popup open, call `CancelBreadcrumbSelector()` per item; that
  chain reaches `host.Close(Uncommitted)` → `CompleteClose` → `FinishClose` →
  `FocusAnchorIfPermitted`, and `MayRestoreBreadcrumbFocus` returns `true` while QuickFiler is still
  the active form — which it is during a Cancel button click. In principle the anchor WebView2 could be
  re-focused after `ParkFocusOffWebView2()` ran earlier in the same stage. Three facts bound the risk
  and none closes it from source alone: the cancel is asynchronous and most likely lands after the
  `hide-form` stage, by which point `Form.ActiveForm` is no longer this form; `_formViewer?.Dispose()`
  disposes the WebView2 children a few stages later, which is the mechanism by which closing QuickFiler
  recovers focus today; and the current behavior is not better, since the popup is closed anyway, later,
  through `ResetBreadcrumb`, which reaches the same `FinishClose`. **Mitigation:** do not reorder the
  #791 stages to accommodate this — the order is pinned by two tests and was the point of #791. Verify
  by the live-Outlook runbook step in Test Strategy.
- **R3 — File-size ceilings.** Two files sit four lines under the 500-line ceiling. **Mitigation:** the
  measured Design A / Design B rule above, applied after CSharpier.
- **R4 — Legacy project files.** A new file without a `<Compile Include>` entry compiles to nothing and
  its tests silently do not run. **Mitigation:** add each entry in the same task as the file, and treat
  a test count that does not increase as a failure signal.
- **R5 — Recurring per-file coverage disposition.** `QfcFormController.EventHandlers.cs` and
  `QfcHomeController.cs` are expected to remain below the per-file floor. **Mitigation:** record the
  disposition in the policy audit up front under the ratified CLAUDE.md UT2 exemption class (c).
- **R6 — No coverage baseline exists on this branch.** **Mitigation:** Phase 0 baseline Cobertura run;
  absence must produce BLOCKED or INCOMPLETE, not an unsupported claim.
- **R7 — Concurrent run on issue 809.** Three files are owned elsewhere. **Mitigation:** they are named
  as explicit non-goals in Scope & Non-Goals, and none is required by any AC.
- **Rollback.** Branch revert. No migration, flag, or persisted state to unwind.

## Rollout & Follow-up

- **Release/rollout steps.** Standard branch → PR → merge to `main`. No staged rollout, no flag, no
  configuration change. Build artifacts ship with the next add-in build.
- **Post-fix monitoring or clean-up tasks.**
  1. Execute the live-Outlook runbook step for R2.
  2. Promote each of the nine report-only items in "Out of scope / report-only" through the issue
     lifecycle so none is lost at merge.
  3. Record the R5 coverage disposition in the policy audit rather than at review time.
- **Links.**
  - Issue: https://github.com/drmoisan/TaskMaster/issues/810
  - Predecessors: #793 and #808, both closed NOT_PLANNED at 2026-09-08T00:06Z so #810 is the single
    live tracker. Their CLOSED state is not evidence the work shipped.
  - Research: `docs/features/active/2026-09-07-quickfiler-teardown-and-dropdown-residuals-810/research/research.2026-09-07T22-10.md`
  - Source finding artifacts:
    `docs/features/active/2026-09-06-quickfiler-high-confidence-cancel-teardown-and-deadline-defects-791/code-review.2026-09-06T15-31.md`
    (N1, N2) and
    `docs/features/active/2026-09-06-quickfiler-folder-dropdown-closes-on-open-and-click-does-not-select-796/code-review.2026-09-07T17-05.md`
    (CR-1 through CR-7).
  - Related: issue #677 (keyboard-lock contract), issue #791 (ordered Cancel teardown), issue #796
    (folder drop-down open/select).
