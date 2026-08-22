# winformspumphost-suite-determinism (Issues #511 and #571)

- Work Mode: full-bug
- Type: bug
- Primary Issue: #511
- Primary Issue URL: https://github.com/drmoisan/TaskMaster/issues/511
- Secondary Issue: #571
- Secondary Issue URL: https://github.com/drmoisan/TaskMaster/issues/571
- Epic: quickfiler-suite-determinism-foundation (child 1 of 4, wave 0)
- Integration Branch: epic/quickfiler-suite-determinism-foundation-integration
- Branch: bug/winformspumphost-suite-determinism-511
- Last Updated: 2026-08-21T18-20

> Provenance note. This file was authored by the orchestrator, not copied by
> `mcp__drm-copilot__new_active_feature_folder`. That tool scaffolded `spec.md` and the plan
> template but produced no `issue.md`, because the folder short-name
> (`winformspumphost-suite-determinism`) does not match either promoted source filename. The two
> promoted records named under "Requirements Sources" below remain in place and are the
> authoritative requirements source; this file is a consolidation, not a replacement.

> Acceptance-criteria authority. Work Mode is `full-bug`, so per the `acceptance-criteria-tracking`
> skill the authoritative acceptance-criteria source for this feature is `spec.md` only. The
> criteria are not duplicated here.

## Requirements Sources

Both promoted records are richer than the GitHub issue bodies and are authoritative:

- `docs/features/potential/promoted/2026-08-08-winformspumphost-tests-load-flaky-visible-window.md` (#511)
- `docs/features/potential/promoted/2026-08-15-qfc-item-controller-init-tests-flaky-window-handle.md` (#571)

Issue state was verified against durable GitHub state on 2026-08-21 with
`gh issue view <n> --json number,title,state,labels,url`. Both are `OPEN` and carry the `bug`
label. No promotion tool was invoked to create them; see the "Promotion" section.

## Summary

Two open defects describe one underlying condition in the QuickFiler test suite and are closed
together by this feature.

**#511 (High) — `WinFormsPumpHost` tests are load-flaky and display a visible window.**
Tests built on `QuickFiler.Test/TestSupport/WinFormsPumpHost.cs` start a real WinForms message pump
on a dedicated STA thread and construct real WinForms controls. They failed nondeterministically
under sustained high CPU load (approximately 96%), requiring six attempts to obtain one clean
full-suite baseline during issue #438 work on 2026-08-08, and a visible window appeared during an
otherwise headless run.

**#571 (Medium) — Two initialization tests fail intermittently on a missing window handle.**
`InitializeNineArgOverload_ThroughThePumpHost_SavesParametersAndDelegates` and
`InitializeBool_ThroughThePumpHost_CompletesAndInitializesState` fail intermittently in a
full-suite run with `InvalidOperationException: Invoke or BeginInvoke cannot be called on a control
until the window handle has been created`, but pass every time when the class runs in isolation.

## Root Cause (established, to be confirmed by research)

`WinFormsPumpHost.RunPumpThread` installs a `WindowsFormsSynchronizationContext` and then calls
`Application.Run(new ApplicationContext())` without ever adding a form or a control, so no window
handle is ever created on the pump thread. The pump harness in
`QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part2.cs` constructs a real
`QuickFiler.ItemViewer` (a `UserControl`) on that thread and never parents it or forces handle
creation. `QfcItemController.InvokeBeginInvoke` reaches `_itemViewer.Invoke(action)`, and
`Control.Invoke` throws unless the native window handle already exists. Whether it happens to exist
depends on ambient WinForms state that differs between a full-suite run and a single-class run.

## The Tension Between #511 and #571

#511 and #571 are in tension, not in dependency order. #511 proposes replacing the real message
pump with an injectable synchronization-context seam. Executed literally, that would delete or
reclassify the very tests #571 stabilizes, together with the pump-hosted coverage justifications
recorded in `QuickFiler/Controllers/QfcItemController.Initialization.cs` and
`QuickFiler/Controllers/QfcItemController.ViewerSetup.cs`.

`spec.md` must state the reconciliation decision explicitly rather than inherit an order from
either issue. It must also record the reading that deterministically establishing a window handle
on the pump thread before the act removes the race rather than masking it, and is therefore not a
prohibited timing hack under the "Prohibited Behaviors" section of `.claude/rules/csharp.md`.

## Constraints Binding This Feature

1. **No new project-file compile entry.** `QuickFiler.Test/QuickFiler.Test.csproj` is a legacy
   non-SDK project. Sibling child #491 owns its `Form1` region and sibling child #449 owns one
   appended `Controllers` entry. This feature must not touch the project file at all, so regression
   tests belong in files that already carry compile entries:
   `QuickFiler.Test/TestSupport/WinFormsPumpHostTests.cs` and
   `QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part3.cs`.
2. **Preserve the dispatcher serialization.**
   `QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part2.cs` defines a
   `UiThreadDispatcherGate` semaphore and a `SwapUiThreadDispatcher` helper that mutate the
   process-wide static `UtilitiesCS.UiThread._dispatcher` by reflection, serializing the pump tests
   across two test classes. Any change to the host or its harness must preserve that serialization
   or `QfcItemController.SeamFactoryTests` and `QfcItemController.InitializationTests` will deadlock
   against each other under class-level parallelization.
3. **The marshalling seam already exists.** `QfcItemController` holds `IItemViewer` rather than a
   concrete control; `Invoke`, `BeginInvoke`, and `InvokeRequired` are re-declared on that interface
   for mockability; and a second seam `UtilitiesCS.Threading.IUiDispatcher` is also held. Both are
   already exercised pump-free in
   `QuickFiler.Test/Controllers/QfcItemController.FocusAndThemeTests.cs`. Issue #230, which built
   `WinFormsPumpHost`, is closed. No new seam is to be planned on the assumption that none exists.
4. **No `.claude/**` edits.** That tree is push-down-owned; a sync overwrites it from an upstream
   bundle with no merge. Where an issue cites a rule file, the citation is the policy the fix is
   measured against, not an edit target.
5. **Re-derive every line number.** `file:line` citations in the promoted records and the epic
   manifest have drifted. The epic's "Known-Stale Potential-Document References" section is binding.
6. **`vstest` must carry `/InIsolation`**, and recursive `*.Test.dll` discovery must exclude
   `\.claude\` so stale agent-worktree builds are not loaded. Omitting `/InIsolation` produces
   roughly 1,695 phantom failures with empty messages, surfacing as a Moq
   `TypeInitializationException` via `System.Threading.Tasks.Extensions`.
7. **No Python toolchain exists.** There is no `scripts/dev_tools/` and no Poetry manifest, so any
   skill step naming `poetry run python -m scripts.dev_tools.*` is unrunnable by absence and must be
   reported as such rather than fabricated or silently skipped.
8. **Evidence paths are non-overridable**: `<FEATURE>/evidence/<kind>/` only.

## Toolchain

Run in this exact order; restart from the first step if any step fails or changes files.

1. `dotnet tool restore`
2. `dotnet tool run csharpier format .` (verify with `dotnet tool run csharpier check .`)
3. `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
4. `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`
5. `vstest.console.exe <assemblies> /EnableCodeCoverage /InIsolation /TestCaseFilter:"TestCategory!=LiveOutlook"`

Use `/t:Rebuild`, never `/t:Build`: a warm `/t:Build` skips `CoreCompile` on every project and runs
no analyzers, returning exit 0 without gating anything. Never add `/p:Nullable=enable`; no project
carries a `<Nullable>` element and there is no `Directory.Build.props`, so the property conscripts
files that never opted in and diverges from `.github/workflows/ci.yml`.

## Promotion

Issues #511 and #571 were already open and their promoted potential records already existed, so no
potential-entry or issue-promotion tool was invoked. `mcp__drm-copilot__potential_to_issue` has no
idempotent path and always creates a new issue; calling it would have filed duplicates. Only
`mcp__drm-copilot__new_active_feature_folder` was called. Receipts are recorded truthfully in
`artifacts/orchestration/orchestrator-state.winformspumphost-suite-determinism.json` under
`delegation_receipts.promotion`, with the potential-entry and issue receipts marked
`status: pre-existing`.
