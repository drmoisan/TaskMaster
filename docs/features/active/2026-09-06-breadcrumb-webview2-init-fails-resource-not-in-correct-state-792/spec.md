# 2026-09-06-breadcrumb-webview2-init-fails-resource-not-in-correct-state (Spec)

- **Issue:** #792
- **Parent (optional):** none
- **Owner:** drmoisan
- **Last Updated:** 2026-09-12
- **Status:** Draft
- **Version:** 1.0
- **Kind:** bug
- **Work Mode:** full-bug

## Context

The breadcrumb `CoreWebView2` initialization fails with HRESULT 0x8007139F ("The group or resource is not in the correct state to perform the requested operation"). The failure is logged by both `WebView2BreadcrumbHost` and `EfcFormController` and then swallowed, so the session continues with a breadcrumb host that never initialized.

The defect was originally recorded as intermittent (2 of 6 launches on 2026-09-06 morning). By the evening of the same day it reproduced on every Efc open: ten pop-out opens between 17:39:00 and 17:41:42, three between 19:04 and 19:06, and the ribbon Sort Email open at 19:56:36. The severity was raised from Medium to High on 2026-09-06 because both Efc entry points are unusable for folder selection in an affected session.

Two user-visible symptoms are reported, and research confirms both are this single failure:

1. Pop-out from a QuickFiler row to an Efc item shows an empty folder list. No suggestions, no banners, and typing a search string does nothing.
2. Ribbon -> Sort Email opens an Efc viewer whose "Matched Folders:" section has no entries. The label is a static WinForms label above the breadcrumb WebView2, which is why the label survives while the list is blank.

Environment:

- OS/version: Windows 11 Pro 10.0.26200
- Language/runtime: C# / .NET Framework 4.8 VSTO add-in (not Python)
- Command/flags used: QuickFiler launched from the ribbon (High Confidence button); add-in loaded from TaskMaster\bin\Debug built 2026-09-06 08:51 from 7c8ac9ae
- Data source or fixture: live Outlook Inbox view

Impact / Severity:

- [ ] Blocker
- [x] High
- [ ] Medium
- [ ] Low

High. The breadcrumb folder selector is the primary folder-selection surface on both Efc entry points, and it is unavailable for the whole session once the failure occurs. A half-initialized WebView2 was also raised as a candidate contributor to the sporadic Outlook keyboard lock tracked under #677; that link remains unconfirmed and is not claimed here.

## Repro & Evidence

Steps to reproduce (original, intermittent form):

1. Launch QuickFiler from the ribbon several times in one Outlook session.
2. Inspect TaskMaster\bin\Debug\logs\debug_&lt;date&gt;.log for "Breadcrumb CoreWebView2 initialization failed".
3. Observe that the failure occurs on some launches and not others.

Steps to reproduce (deterministic form, established 2026-09-06 evening and explained by the root cause below):

1. Open QuickFiler from the ribbon so that at least one item-body WebView2 is created in the Outlook process.
2. Pop out any row to an Efc item, or invoke ribbon -> Sort Email.
3. Observe an empty folder list under "Matched Folders:" and the paired error lines in the log.

Expected behavior:

WebView2 initialization either succeeds, or fails with a clear surfaced error and a defined fallback state that cannot retain keyboard focus. A failed initialization is retried or the host is disposed, not left half-constructed.

Actual behavior:

Two ERROR lines per occurrence, then normal operation continues with a permanently blank folder list. A `BreadcrumbUiDispatcher` dispatch failure followed in the same session on 2026-09-06 at 09:01:56.

Logs / Screenshots:

- [x] Attached minimal logs
- Snippet (debug_2026-09-06.log):

```
2026-09-06 08:55:22,227 [VSTA_Main] ERROR QuickFiler.Viewers.WebView2BreadcrumbHost - Breadcrumb CoreWebView2 initialization failed: ... (HRESULT: 0x8007139F)
2026-09-06 08:55:22,286 [VSTA_Main] ERROR QuickFiler.Controllers.EfcFormController - Breadcrumb WebView2 initialization failed: ... (HRESULT: 0x8007139F)
2026-09-06 09:01:56,237 [VSTA_Main] ERROR QuickFiler.Viewers.BreadcrumbUiDispatcher - Breadcrumb UI dispatch failed.
2026-09-06 19:56:37,940 [VSTA_Main] ERROR QuickFiler.Viewers.WebView2BreadcrumbHost - Breadcrumb CoreWebView2 initialization failed: The group or resource is not in the correct state to perform the requested operation. (Exception from HRESULT: 0x8007139F)
2026-09-06 19:56:38,004 [VSTA_Main] ERROR QuickFiler.Controllers.EfcFormController - Breadcrumb WebView2 initialization failed: The group or resource is not in the correct state to perform the requested operation. (Exception from HRESULT: 0x8007139F)
```

The 59-67 ms gap between the paired lines in every logged occurrence is consistent with one event-then-task-continuation sequence, not with two independent SDK attempts. This is verified in the research record: the two lines are one failure logged twice.

## Scope & Non-Goals

In scope:

- Converging all three production WebView2 environment creations on one shared owner of the user-data folder and the additional browser arguments, so the options conflict that produces 0x8007139F cannot recur.
- Bounded, deterministic retry of the breadcrumb host initialization, and a visible error state in the folder area on final failure.
- Guaranteeing that the router's pending document and the breadcrumb outbound queue are both drained or explicitly discarded, never left silently pending.
- Routing the breadcrumb initialization failure through the existing fault boundary so the user is notified rather than only the log.
- Carrying the already-initialized folder predictor and the loaded `MailItemHelper` from the QuickFiler item into the pop-out Efc view, and constructing the Efc viewer through the dispatcher rather than inline.
- The file-splitting and project-file edits that the 500-line ceiling and the non-SDK-style project format oblige for every file touched.

Out of scope / non-goals:

- A full split of `QuickFiler/Controllers/QfcCollectionController.cs` (2329 lines) or of `QuickFiler/Controllers/EfcItemController.cs` (1121 lines). Only the members this change edits move into new compliant partials; the parents' remaining over-ceiling size is pre-existing debt this change neither introduces nor resolves. See AC-U9.
- The archive-root read at the breadcrumb bind boundary. Research confirms the read is unguarded but already fail-soft and already user-surfaced through the existing boundary, and its behavior is pinned by an existing test. The QuickFiler twin of that read is tracked separately under issue #813.
- Any change to the UtilitiesCS folder-search-handler interface or to the folder predictor. See the carry typing decision below.
- Any claim about, or fix for, the #677 keyboard lock. The link between a half-initialized WebView2 and the keyboard lock is unconfirmed and is not addressed here.
- The two prior findings #458 (pooled-viewer handler retention) and #476 (unmarshalled SDK call and unsynchronized state). Research verified both are already fixed in the current tree and neither contributes to this defect.

Explicitly excluded systems, integrations, or datasets: no Outlook Interop surface, no settings schema, no persisted data, no network call.

## Root Cause Analysis

0x8007139F is `HRESULT_FROM_WIN32(ERROR_INVALID_STATE)`. Microsoft documents it for WebView2 environment creation as "Specified options do not match the options of the WebViews that are currently running in the shared browser process." The same rule appears in prose on the `options` parameter of the .NET `CoreWebView2Environment.CreateAsync` reference.

The add-in has exactly three production environment creations, all against the same user-data folder `%LocalAppData%\WindowsFormsWebView2`, but only two supply `--incognito `:

- `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs` at line 61 supplies `--incognito `
- `QuickFiler/Controllers/EfcItemController.cs` at lines 176 and 187-189 supplies `--incognito `
- `QuickFiler/Viewers/WebView2BreadcrumbHost.cs` at line 250 supplies no additional browser arguments

The breadcrumb host is the odd one out and is the only one that fails. The count of three and the identification of the single divergent site are derived twice by independent search strategies in the research record's Numeric Derivation Evidence section, which agree on both membership and count.

This explains the reported timeline without appeal to timing. The failure was originally intermittent because it depends on whether an `--incognito` WebView was already running in the Outlook process when the Efc breadcrumb initialized. It is now deterministic on both reported entry points because a pop-out always follows an open QuickFiler, and the logged 19:56 Sort Email open followed ten pop-outs in the same session.

The consequences described in the issue are real and follow from this cause:

- The failure branch of the initialization-completed handler returns before raising `CoreInitialized` and before publishing `IsCoreInitialized`, so neither of the two drains of the router's pending document can ever fire for the rest of the viewer's life. Every subsequent bind and theme change overwrites a stash nobody will read.
- The breadcrumb outbound queue has the same shape and the same single drain, so a failed session accumulates an unbounded serialized payload that is never released.
- The initialization task's failure is log-only.

Candidates from the original issue body that research refuted: initialization before the control's handle or parent is valid (reachable but not this HRESULT, and the identical pre-show ordering holds for the item-body WebView that does not fail); a second initialization against a control already mid-initialization (one construction site, one initialization call site, and a per-control owner registry); a disposed or pooled-and-reused control (pooling is real but would surface as a different, already-fixed defect shape); and the reading that two log lines imply two initialization paths.

## Proposed Fix

### Design summary (what changes where)

Give the process one owner of the WebView2 environment contract and make all three creations use it. A new file `QuickFiler/Viewers/WebView2EnvironmentContract.cs` holds the additional-browser-arguments constant, the user-data-folder resolution currently duplicated three times, and an options factory. `QuickFiler/Viewers/WebView2BreadcrumbHost.cs` is the behavior-changing site: its environment acquires `--incognito ` and stops conflicting. The two item-controller sites are re-pointed at the same owner, line-neutrally where possible.

On top of that root-cause remedy, four behavioral changes land:

- `QuickFiler/Controllers/EfcFormController.Breadcrumb.cs` (a new partial part) gains a bounded, deterministic retry of the host initialization, routes the final failure through the existing fault boundary, and notifies the router of the failure.
- `QuickFiler/Controllers/BreadcrumbBridgeRouter.cs` gains a failure-notification entry point next to the existing success notification. It renders a visible error banner into the folder area, clears the pending document, and explicitly drains or discards `QuickFiler/Controllers/BreadcrumbOutboundQueue.cs`.
- `QuickFiler/Controllers/QfcCollectionController.PopOut.cs` (a new partial part) carries the source item's initialized folder handler and loaded `MailItemHelper` into the pop-out, through `QuickFiler/Controllers/EfcHomeController.cs` and into `QuickFiler/Controllers/EfcDataModel.Carry.cs` (a new partial part).
- `QuickFiler/Helper Classes/EfcViewerQueue.cs` changes its production blocking scheduler from an inline invocation to a dispatcher invocation, matching the sibling item viewer queue.

### Boundaries and invariants to preserve

- One owner, one contract: after this change, no production code constructs WebView2 environment options other than through the shared contract. The three sites must resolve to the same user-data folder and the same additional browser arguments.
- The pending document and the outbound queue have exactly one terminal state each: delivered, or explicitly discarded with the user informed. Silent retention is the defect and must not survive in any branch.
- A later successful initialization must still navigate a stash produced before a failure. Adding the failure path must not remove the success path.
- The retry must be deterministic and must not use wall-clock waits. The repository determinism rule bans `Thread.Sleep` and `Task.Delay` in tests, so any delay is an injected delegate substituted with a no-op.
- The carry is adopted only when the carried handler is already initialized for that item under the same initialization sequence the construction branch would otherwise run, mirroring the #678 receiver contract. Otherwise the existing construction path runs unchanged.
- `OperationCanceledException` is classified as non-fault, matching the existing precedent on the bind boundary and the keyboard guard.
- The retry and the error surfacing do not go into the SDK initialization-completed handler. That branch is structurally unreachable from a unit test because the completed-event argument type has no public constructor, and it is already coverage-exempt for that documented reason.
- The item controllers' separate WebView initialization fault member is a deliberately distinct contract and must not be conflated with the form controller's boundary fault reporter.

### Dependencies or blocked work

- A prerequisite verification task confirms the breadcrumb document does not depend on persisted browsing storage before the options direction is finalized. See Risks.
- No dependency on any other in-flight item. Two siblings touch adjacent surface; see Known contention.

### Implementation strategy (what changes, not sequencing)

Settled scope decisions, recorded here so they are not relitigated:

1. **Options direction.** All three sites converge on `--incognito `. Two of three already use it and the item-body preview has always used it. A prerequisite verification task will confirm the breadcrumb document does not depend on persisted browsing storage before this is finalized.
2. **Error-state primitive for AC-U1.** The visible error state is a banner row composed from the existing breadcrumb banner-prefix convention and delivered through the existing router and renderer. Banner rows are already non-selectable. No new WinForms control is added to the viewer, because that would land in a Designer-owned file that is excluded from coverage.
3. **Carry typing for AC-U3.** The carried object is typed as the folder-search-handler interface. The Efc data model's concrete predictor property is not retyped and the UtilitiesCS interface is not widened. The carry is adopted only when the carried instance is the concrete predictor type, by pattern match, and otherwise the existing construction path runs unchanged. This keeps the change inside the QuickFiler project. UtilitiesCS is not modified by this change.
4. **File-size obligations.** The 500-line ceiling applies. `QuickFiler/Controllers/EfcFormController.cs` is 1321 lines and is not currently declared partial; it is split into six files, five of them new, each under 500 lines. `QuickFiler/Controllers/EfcDataModel.cs` is 499 lines, one under the ceiling, so the carry work is placed in a new partial. `QuickFiler/Controllers/QfcCollectionController.cs` is 2329 lines and `QuickFiler/Controllers/EfcItemController.cs` is 1121 lines; a full split of either is out of scope for a bug fix, so the edited members move into new compliant partials and the parent files' remaining over-ceiling size is recorded as pre-existing debt this change does not introduce and does not resolve.
5. **Non-SDK-style projects.** Adding or removing any .cs file requires editing the owning project file's Compile item list. `QuickFiler/QuickFiler.csproj` and `QuickFiler.Test/QuickFiler.Test.csproj` are both in the write set for that reason. Hand-written partial parts are listed as bare self-closing Compile elements with no metadata; DependentUpon is used only for Designer and resx pairings and must not be added.
6. **Evidence convention.** Per the maintainer decision on issue 671 of 2026-09-11, commit projections only. No .trx file and no .cobertura.xml file is written into the repository. Numeric coverage and test-result figures are recorded inside the Markdown evidence artifacts under the feature folder's evidence directory, and the raw tool output is discarded.
7. **AC-U5 is manual.** It is a human-executed live-Outlook verification with a runbook. It is not an automated gate and must not be described as one.
8. **Manual build gate.** Outlook must be closed, never killed, before any rebuild, or the build output stays locked. This is a human step in the runbook.

#### Files/modules to change

The authoritative list is the `## Write Set` section below. In summary:

- New shared contract: `QuickFiler/Viewers/WebView2EnvironmentContract.cs`.
- Environment creation sites: `QuickFiler/Viewers/WebView2BreadcrumbHost.cs`, `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs`, and `QuickFiler/Controllers/EfcItemController.cs` with its edited members moved into `QuickFiler/Controllers/EfcItemController.WebViewEnvironment.cs`.
- Router and queue: `QuickFiler/Controllers/BreadcrumbBridgeRouter.cs`, `QuickFiler/Controllers/BreadcrumbOutboundQueue.cs`.
- Form controller split into six files, five new: `QuickFiler/Controllers/EfcFormController.cs` (retained), `QuickFiler/Controllers/EfcFormController.Breadcrumb.cs`, `QuickFiler/Controllers/EfcFormController.SetupAndProperties.cs`, `QuickFiler/Controllers/EfcFormController.EventHandlers.cs`, `QuickFiler/Controllers/EfcFormController.Actions.cs`, `QuickFiler/Controllers/EfcFormController.Helpers.cs`.
- Pop-out carry: `QuickFiler/Controllers/QfcCollectionController.cs`, `QuickFiler/Controllers/QfcCollectionController.PopOut.cs`, `QuickFiler/Controllers/EfcHomeController.cs`, `QuickFiler/Controllers/EfcDataModel.cs`, `QuickFiler/Controllers/EfcDataModel.Carry.cs`, `QuickFiler/Controllers/QfcItemController.cs`. The item-controller interface is not modified; see the exclusion paragraph under the Write Set.
- Viewer construction: `QuickFiler/Helper Classes/EfcViewerQueue.cs`.
- Project files: `QuickFiler/QuickFiler.csproj`, `QuickFiler.Test/QuickFiler.Test.csproj`.
- Tests: nine .cs files listed in the write set (eight new, one existing file whose stale test name is corrected).

#### Functions/classes/CLI commands impacted

- `WebView2BreadcrumbHost.InitializeAsync` — environment options now come from the shared contract.
- `QfcItemController.InitializeWebViewAsync` and `EfcItemController.InitializeWebViewAsync` — re-pointed at the shared contract; the Efc one moves into a new partial.
- `EfcFormController.InitializeBreadcrumbHostAsync` — becomes internal, gains bounded retry, routes the final failure through `TryReportBoundaryFault`, and notifies the router.
- `EfcFormController.ConfigureBreadcrumbControl` — wires the new failure notification next to the existing `CoreInitialized` subscription.
- `EfcFormController.PopulateFolderCombobox` — unchanged in behavior; its existing test is strengthened.
- `BreadcrumbBridgeRouter.NotifyInitializationFailed` — new. Renders the error banner, clears the pending document, drains or discards the outbound queue.
- `BreadcrumbBridgeRouter.NotifyCoreInitialized` — unchanged behavior, retained for the later-success path.
- `QfcCollectionController.PopOutControlGroup` and `PopOutControlGroupAsync` — move to the new partial and gain an injectable home-controller factory seam plus the carry read.
- `QfcItemController` — gains an internal read-only folder-handler accessor over the existing private field. The item-controller interface is not widened: it has an implementer in the test project outside the write set (a private fake in the theme-helper tests) and a legacy implementer, so adding a member would break a file this change may not edit. The pop-out reads the accessor by pattern-matching the group's item controller to the concrete type. The interface already exposes the mail item helper, so that half of the carry needs no new accessor.
- `EfcHomeController` constructors — gain trailing optional carry parameters and deposit them on the data model before the form controller is constructed.
- `EfcDataModel.InitFolderHandlerAsync` — moves to the new carry partial and gains the adoption branch.
- `EfcViewerQueue.ProductionBlockingPriorityScheduler` and `ResetProductionCoreDefaultsForTesting` — the inline invocation becomes a dispatcher invocation.

#### Data flow and validation changes

Environment creation: all three sites now read one folder string and one options instance from the shared contract, so the shared browser process sees a single consistent option set.

Breadcrumb delivery on failure: initialization fails -> bounded retry exhausts -> the form controller calls the router's failure notification -> the router renders an error banner row and delivers it through the existing renderer -> the pending document is cleared -> the outbound queue is drained or discarded and its pending count returns to zero -> the form controller reports the fault through the existing boundary so the user is notified.

Pop-out carry: the source item controller's mail item helper is read through the existing interface member and its folder handler through the internal accessor on the concrete item controller (null when the group's controller is not the concrete type); both are passed to the home controller, deposited on the data model before the form controller is constructed, and adopted inside the folder-handler initialization only when no explicit folder list was supplied and the carried instance matches the concrete predictor type. The carry is released after adoption. This ordering is required because the form controller's initialization fires a fire-and-forget folder-combobox population that otherwise overwrites the deposited handler.

#### Error handling and logging updates

- The breadcrumb initialization failure is reported through the form controller's existing boundary fault reporter instead of a bare logger call. Its default sink logs and then surfaces a modeless notice to the user.
- `OperationCanceledException` remains a debug-level, non-fault classification.
- The stale comment on the initialization member, which states the queue is released when initialization fires, is corrected: on failure it is released by the failure notification.
- No new logging category is introduced.

#### Rollback/feature-flag considerations

No feature flag. The change is a source-level fix delivered in one branch; rollback is a revert of the branch. The root-cause remedy is a single-value convergence and can be reverted independently of the retry and carry work if a browsing-storage dependence is discovered after the fact.

### Technical specifications (interfaces/contracts)

#### Inputs/outputs and formats

- Shared contract: an internal constant string for the additional browser arguments, an internal static resolution of the user-data folder path, and an internal static options factory. No file is created by the resolution; the path is computed as a string.
- Router failure notification: takes the initialization exception and returns void. It must be safe to call when the host was never initialized.
- Carry parameters: trailing optional parameters typed as the folder-search-handler interface and the mail item helper, defaulting to null so every existing call site compiles unchanged.

#### Required configuration keys and defaults

None. No settings key is added, read, or changed.

#### Backward-compatibility expectations

All new constructor and factory parameters are trailing and optional. No public signature is removed or narrowed. No interface is widened: the folder-handler accessor is an internal get-only property on the concrete item controller, reached by pattern match, so the item-controller interface's out-of-write-set implementers compile unchanged.

#### Performance constraints

The retry is bounded by a fixed attempt count with no wall-clock delay by default, so the worst case adds a small constant number of failed environment-creation attempts to Efc open. No throughput or memory constraint applies beyond removing the unbounded outbound-queue growth, which this change fixes.

## Assumptions, Constraints, Dependencies

Assumptions:

- The documented WebView2 rule applies to this add-in's process model: a single shared browser process per user-data folder, pinned to the options of the first WebView that starts it.
- The breadcrumb document, which is generated locally and delivered by navigating to a string, does not depend on persisted browsing storage. This is not yet audited and is a prerequisite verification task, not an established fact.
- The maintainer accepts a banner row in the folder area as the visible error state for AC-U1. This is a settled scope decision.

Constraints:

- 500-line ceiling on every production, test, and reusable script file.
- MSTest, Moq, and FluentAssertions only. No xUnit, no NUnit.
- No temporary files in tests, no live Outlook in tests, no `Thread.Sleep` or `Task.Delay` in tests.
- Non-SDK-style project files with explicit Compile item lists in both the production and the test project.
- Toolchain order: CSharpier format, then the analyzer rebuild, then the nullable rebuild, then vstest. Any failure restarts the loop at format.

External dependencies:

- Microsoft.Web.WebView2 SDK, already referenced by the QuickFiler project. No version change.

## Data / API / Config Impact

- User-facing changes: the folder area now shows a visible error banner instead of a blank list when initialization finally fails, and the user receives the existing modeless fault notice. On the pop-out path the folder list is populated from the carried predictor, so it appears without a rebuild from scratch.
- Data or migration considerations: none. No persisted data, no schema, no settings key.
- Logging/telemetry updates: the breadcrumb initialization failure moves from a bare logger error to the boundary fault reporter, which still logs. One stale explanatory comment is corrected.
- Compatibility notes: no CLI flag, no config schema, no versioned contract. The added internal accessor on the concrete item controller and the added optional parameters are source-compatible with every in-repo caller.

## Test Strategy

Framework: MSTest with Moq and FluentAssertions, in the `QuickFiler.Test` project. That project lists every source file explicitly, so each new test file needs a Compile item edit.

Seam per criterion:

- **AC-U1 (retry and visible error state).** Seam: a Moq double of the WebView core-initializer interface whose environment-creation or ensure-core call returns a faulted task for the first N calls, combined with an injectable initialization delegate on the form controller. The retry must live on the awaited-task path, not in the SDK completed-event handler, because the completed-event argument type has no public constructor and that branch is structurally unreachable. The error state is asserted at the router and renderer level as a banner row, not at the WebView2 level. Files: `QuickFiler.Test/Viewers/WebView2BreadcrumbHostIssue792Tests.cs` and `QuickFiler.Test/Controllers/EfcFormControllerIssue792Tests.cs`.
- **AC-U2 (pending document never silently dropped).** Seam: a Moq double of the breadcrumb web host interface with a settable initialized flag, exactly as the existing router queue tests already do. Assert that a later successful initialization notification navigates a stash produced earlier, and that a failure notification produces an error document and leaves no stash. No new production seam is required. File: `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterIssue792Tests.cs`.
- **AC-U3 (pop-out carry and UI-thread construction).** Carry half seam: the new injectable home-controller factory on the pop-out partial, an uninitialized concrete item controller carrying a folder handler and a mail item helper set by reflection, plus a Moq double of the item-controller interface for the non-concrete-type case, and the carry parameter path into the data model's folder-handler initialization. Files: `QuickFiler.Test/Controllers/QfcCollectionControllerIssue792PopOutTests.cs` and `QuickFiler.Test/Controllers/EfcDataModelIssue792CarryTests.cs`. UI-thread half seam: substitution of the Efc viewer queue's production blocking priority scheduler, asserting the action was scheduled rather than run inline. Note explicitly: this half is verifiable only as a scheduler-delegate assertion, not as a real thread-affinity assertion. File: `QuickFiler.Test/Helper Classes/EfcViewerQueueIssue792Tests.cs`.
- **AC-U4 (boundary reporting).** Seam: substitution of the form controller's boundary error sink on a minimally constructed controller, plus the injectable initialization delegate required by AC-U1. Note: the `PopulateFolderCombobox` half of this criterion is already satisfied on main at line 1270 of the form controller. The work for that half is to strengthen the existing test in `QuickFiler.Test/Controllers/EfcFormControllerTests.cs`, which today asserts only that the method logs once and does not fault, so that it asserts the sink call and the criterion is pinned rather than incidentally true. The `InitializeBreadcrumbHostAsync` half is a real change. File: `QuickFiler.Test/Controllers/EfcFormControllerIssue792Tests.cs`.
- **AC-U5 (both entry points).** Not automatable. It is a human-executed live-Outlook verification with a runbook recorded in the user story. It is not an automated gate.
- **AC-U6 (one shared owner of the environment contract).** Seam: a Moq double of the WebView core-initializer interface verifying that the folder argument and the additional-browser-arguments property handed to environment creation equal the shared contract's values, for each site that goes through the seam; plus a structural parity test asserting the three sites resolve to one constant. The Efc item-controller site currently bypasses the seam by calling the SDK factory directly, so making it verifiable requires routing it through the seam. Files: `QuickFiler.Test/Viewers/WebView2EnvironmentContractTests.cs` and `QuickFiler.Test/Viewers/WebView2BreadcrumbHostIssue792Tests.cs`.
- **AC-U7 (outbound queue drained or discarded).** Seam: the outbound queue's existing public pending-count property, driven through the router's failure notification with a Moq host double. Assert the pending count is zero after the notification. File: `QuickFiler.Test/Controllers/BreadcrumbOutboundQueueIssue792Tests.cs`.
- **AC-U8 (500-line ceiling and Compile item parity).** Seam: a measured line count of every file in the write set, and a comparison of the set of .cs files added or removed against the Compile item edits in the two project files. Recorded as a Markdown evidence artifact, not as a runtime test.
- **AC-U9 (pre-existing debt recorded).** Verified by inspection of the change description; no test.

Regression tests to add or update:

- New: the eight new test files named in the write set.
- Updated: `QuickFiler.Test/Controllers/EfcFormControllerTests.cs`, to strengthen the folder-combobox fault test from "logs once and does not fault" to an assertion on the boundary sink.

Edge cases and negative scenarios:

- Initialization fails on every attempt; initialization fails then succeeds on a later attempt; initialization is canceled rather than failed.
- A document is stashed before the failure, and another after it.
- The outbound queue is non-empty at the moment of failure.
- The pop-out supplies no carry (null handler), supplies a carry of an unexpected runtime type, and supplies an explicit folder list alongside a carry. In all three the existing construction path must run unchanged.

Error handling and logging verification: assert the boundary sink is invoked exactly once per failure and that a canceled initialization does not invoke it.

Coverage impact and targets: every new production file targets at least 90 percent line coverage. No file in the write set may regress coverage on its changed lines. Repository floors continue to apply. Numeric figures are recorded inside the Markdown evidence artifacts under the feature folder's evidence directory; no .trx and no .cobertura.xml is committed.

Toolchain commands, in order, restarting at step 1 on any failure or auto-fix:

1. `dotnet tool run csharpier format .` (verify with `dotnet tool run csharpier check .`)
2. `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
3. `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`
4. `vstest.console.exe <test-assembly-paths> /EnableCodeCoverage`

Manual validation steps: see AC-U5 and the runbook in user-story.md. Outlook must be closed, never killed, before the rebuild, or the build output stays locked.

## Acceptance Criteria

- [ ] AC-U1: A failed `CoreWebView2` initialization is retried, and on final failure the Efc view shows a visible error state in the folder area instead of a blank list.
- [ ] AC-U2: `_pendingDocument` is never silently dropped: it is delivered when initialization later succeeds or an error is surfaced.
- [ ] AC-U3: The pop-out path carries the already-initialized folder predictor and loaded `MailItemHelper` from the QfcItem, following the #678 carry pattern, and constructs the `EfcViewer` on the UI thread.
- [ ] AC-U4: `PopulateFolderCombobox` and `InitializeBreadcrumbHostAsync` report failures through `TryReportBoundaryFault` to the user, not log-only.
- [ ] AC-U5: Manual verification on both entry points: pop-out from QuickFiler and ribbon Sort Email each show suggestion rows and respond to typed search.
- [ ] AC-U6: All three production WebView2 environment creations resolve their user-data folder and their additional browser arguments from one shared owner, and a test asserts the three agree.
- [ ] AC-U7: The breadcrumb outbound queue is not left to grow without bound after a failed initialization: a failure notification drains or discards it explicitly, and a test asserts its pending count is zero afterwards.
- [ ] AC-U8: No file created or modified by this change exceeds 500 lines, and every added or removed .cs file has a matching Compile item edit in its owning project file.
- [ ] AC-U9: The pre-existing over-ceiling size of the two files that are not fully split is recorded explicitly in the change description as pre-existing debt, with the line counts before and after.

## Write Set

`QuickFiler/Viewers/WebView2EnvironmentContract.cs`
`QuickFiler/Viewers/WebView2BreadcrumbHost.cs`
`QuickFiler/Controllers/QfcItemController.ViewerSetup.cs`
`QuickFiler/Controllers/EfcItemController.cs`
`QuickFiler/Controllers/EfcItemController.WebViewEnvironment.cs`
`QuickFiler/Controllers/BreadcrumbBridgeRouter.cs`
`QuickFiler/Controllers/BreadcrumbOutboundQueue.cs`
`QuickFiler/Controllers/EfcFormController.cs`
`QuickFiler/Controllers/EfcFormController.Breadcrumb.cs`
`QuickFiler/Controllers/EfcFormController.SetupAndProperties.cs`
`QuickFiler/Controllers/EfcFormController.EventHandlers.cs`
`QuickFiler/Controllers/EfcFormController.Actions.cs`
`QuickFiler/Controllers/EfcFormController.Helpers.cs`
`QuickFiler/Controllers/QfcCollectionController.cs`
`QuickFiler/Controllers/QfcCollectionController.PopOut.cs`
`QuickFiler/Controllers/EfcHomeController.cs`
`QuickFiler/Controllers/EfcDataModel.cs`
`QuickFiler/Controllers/EfcDataModel.Carry.cs`
`QuickFiler/Controllers/QfcItemController.cs`
`QuickFiler/Helper Classes/EfcViewerQueue.cs`
`QuickFiler/QuickFiler.csproj`
`QuickFiler.Test/Viewers/WebView2BreadcrumbHostIssue792Tests.cs`
`QuickFiler.Test/Viewers/WebView2EnvironmentContractTests.cs`
`QuickFiler.Test/Controllers/BreadcrumbBridgeRouterIssue792Tests.cs`
`QuickFiler.Test/Controllers/BreadcrumbOutboundQueueIssue792Tests.cs`
`QuickFiler.Test/Controllers/EfcFormControllerIssue792Tests.cs`
`QuickFiler.Test/Controllers/EfcFormControllerTests.cs`
`QuickFiler.Test/Controllers/QfcCollectionControllerIssue792PopOutTests.cs`
`QuickFiler.Test/Controllers/EfcDataModelIssue792CarryTests.cs`
`QuickFiler.Test/Helper Classes/EfcViewerQueueIssue792Tests.cs`
`QuickFiler.Test/QuickFiler.Test.csproj`

Two of the paths above contain a space in the directory name, under Helper Classes in the production project and in the test project. Those are the real tracked paths and are reproduced exactly.

The list above holds 31 paths. Files deliberately not modified, stated in prose because the extractor has no notion of polarity and would harvest an excluded path as though it were written. The QuickFiler item-controller interface in the Interfaces folder is not touched: it has two implementers outside the write set (a private fake in the test project's theme-helper tests and a legacy controller), so a new member would break a file this change may not edit; the pop-out carry reads an internal accessor on the concrete item controller by pattern match instead. The UtilitiesCS folder-search-handler interface and the folder predictor that implements it are not touched: the carry is typed as the existing interface and adopted by pattern match on the concrete predictor type, so neither the interface nor the predictor needs widening, and no change leaves the QuickFiler project. The breadcrumb UI dispatcher is not touched: research assessed its boundary check as self-consistent and a sibling item explicitly scoped it out, so it is edited only if a test proves a need, which is not anticipated. The breadcrumb row builder and the breadcrumb HTML renderer are not touched: the error state reuses the existing banner-prefix convention and the existing rendering path rather than adding a new primitive. The item viewer files owned by a sibling item are not touched: the overlap there is conceptual only. The breadcrumb HTML resource is not touched: the document content is unchanged and only its delivery on the failure path changes.

## Known contention

Two sibling items in the same parallel run touch adjacent surface. This section records the overlap; it does not coordinate. The scheduler serializes.

Sibling A adds invariant-culture date and time formatting and edits the QuickFiler collection controller at three format call sites. Those sites are disjoint from the pop-out members this item edits, so there is no line overlap. Both items nonetheless touch that collection-controller file and the QuickFiler project file, and this item adds Compile items to that project file while the sibling also edits it, so a project-file item-group merge is expected.

Sibling B adds a UI-marshalling seam scoped to the item viewer files, which this item does not write. The overlap is conceptual only: both items decide how UI-boundary ownership is proven. This item adopts the owner-thread-identity idiom that sibling ratified rather than inventing a third convention.

## Risks & Mitigations

Technical and operational risks, including the research record's open questions:

- **Whether the shared browser process is torn down when all viewers close is unverified.** The documented options rule is scoped to WebViews currently running in the shared browser process; whether closing every QuickFiler and Efc viewer releases that process is not established from the code. This affects only the AC-U5 manual repro recipe, specifically whether a clean Outlook session is needed to observe the pre-fix failure. It does not affect the fix. Mitigation: the runbook opens QuickFiler first so that an item-body WebView is running before the Efc open, which reproduces the conflict regardless of teardown behavior.
- **Whether the breadcrumb document depends on persisted browsing storage is unverified and must be verified before the options direction is finalized.** The document is generated locally and delivered by navigating to a string, so no cookie, cache or storage dependence is apparent, but the generated markup and script were not audited for local storage or similar. Mitigation: a prerequisite verification task audits the generated document before the convergence on `--incognito ` is committed. If a storage dependence is found, the alternative convergence direction (no additional arguments at all three sites) resolves the conflict equally well and the direction decision is revisited.
- **The claim that the pop-out continuation lands off the UI thread is unverified.** One await on the pop-out path was not traced to a terminal configure-await, so the off-thread continuation was not reproduced. The AC-U3 UI-thread clause is therefore justified as defence in depth and as parity with the sibling item viewer queue, which already schedules through the dispatcher, not as a proven reproduction. Mitigation: the change is a one-line scheduler substitution in a small file with an existing test seam, so its cost is low even if the failure mode never occurs in production.
- **The visible error state has no pre-existing error rendering primitive.** The banner row is the nearest fit and is a settled scope decision. Mitigation: banner rows are already non-selectable, so the error row cannot be chosen as a folder, and the alternative of a new WinForms control was rejected because it lands in a Designer-owned, coverage-excluded file.
- **Home-controller file headroom.** That file is 447 lines, leaving roughly 53 lines. The carry parameters plus their documentation could exceed the ceiling, forcing an additional partial and an additional Compile item. Mitigation: size the change before committing to the file list; the write set is adjusted if the measurement requires it, and AC-U8 makes the ceiling a blocking criterion rather than an afterthought.
- **The Efc item controller bypasses the mockable initializer seam.** Its environment creation calls the SDK factory directly, so AC-U6's assertion for that site requires routing it through the seam. Mitigation: the routing change is part of the write set for that file's new partial.
- **Retry could mask a genuine environment problem.** A bounded, small attempt count with no wall-clock delay keeps the worst case short, and the final failure is surfaced to the user rather than absorbed.
- **Project-file merge with the sibling item.** Both items edit the same explicit item list. Mitigation: additions are appended adjacent to their neighbours as bare self-closing elements with no metadata, which minimises the conflict surface; a merge on item-group ordering is expected and accepted.

Mitigations and rollback: the change carries no feature flag; rollback is a branch revert. The root-cause convergence is a single-value change and can be reverted independently of the retry, router and carry work.

## Rollout & Follow-up

Release/rollout steps:

1. Run the full toolchain in order and confirm all four steps pass in one pass.
2. Close Outlook, never kill it, then rebuild. Reopen Outlook and load the rebuilt add-in.
3. Execute the AC-U5 manual runbook in user-story.md against both entry points and record the observation.
4. Record the AC-U8 line-count and Compile-item parity measurements, and the AC-U9 pre-existing debt statement with before and after counts, in the change description and in the Markdown evidence artifacts under the feature folder's evidence directory.

Post-fix monitoring or clean-up tasks:

- Review the add-in debug log across several Outlook sessions for any remaining occurrence of HRESULT 0x8007139F.
- Track the remaining over-ceiling size of the collection controller and the Efc item controller as pre-existing debt for a separate split item.
- Re-assess whether the #677 keyboard lock persists once the half-initialized WebView2 state is eliminated. No claim is made here that it will.

Links:

- Issue: https://github.com/drmoisan/TaskMaster/issues/792
- Issue record: docs/features/active/2026-09-06-breadcrumb-webview2-init-fails-resource-not-in-correct-state-792/issue.md
- Research record: docs/features/active/2026-09-06-breadcrumb-webview2-init-fails-resource-not-in-correct-state-792/research/2026-09-12T10-30-breadcrumb-webview2-init-research.md
- User story: docs/features/active/2026-09-06-breadcrumb-webview2-init-fails-resource-not-in-correct-state-792/user-story.md
- Related: #678 (carry pattern), #677 (keyboard hook leak, unconfirmed link), #813 (QuickFiler twin of the archive-root read), #458 and #476 (verified already fixed, not contributing)
