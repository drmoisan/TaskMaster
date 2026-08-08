# quickfiler-breadcrumb-dropdown-webview-coverage — Atomic Implementation Plan

- **Issue:** #455
- **Parent:** epic #136 `quickfiler-per-file-coverage`, child F13
- **Owner:** drmoisan
- **Last Updated:** 2026-08-07T22-36
- **Status:** Draft (preparation mode — authored now, executed later by `epic-orchestrator` in a different worktree)
- **Version:** 1.0
- **Work Mode:** `full-feature` (`spec.md` + `user-story.md` are the authoritative AC sources)
- **Integration branch:** `epic/quickfiler-per-file-coverage-integration`
- **Upstream dependency:** F1 (#432) `quickfiler-coverage-denominator-and-exemption-ledger`

## Path Conventions (read before executing any task)

- **All paths in this plan are repository-relative.** No absolute path appears anywhere in this
  document, and no task may introduce one. Every path resolves from the repository root of whatever
  worktree executes this plan.
- `<FEATURE>` expands to
  `docs/features/active/2026-08-07-quickfiler-breadcrumb-dropdown-webview-coverage-455`.
- `<EPIC>` expands to `docs/features/epics/quickfiler-per-file-coverage`.
- `<ts>` expands to the ISO-8601 timestamp `yyyy-MM-ddTHH-mm` at the moment the task runs.
- Evidence locations are non-overridable and are exactly:
  `<FEATURE>/evidence/baseline/`, `<FEATURE>/evidence/qa-gates/`,
  `<FEATURE>/evidence/regression-testing/`, `<FEATURE>/evidence/other/`.
  No `artifacts/` sub-path may be used for evidence.
- Every command-step evidence artifact records `Timestamp:`, `Command:`, `EXIT_CODE:`,
  `Output Summary:`. Every coverage-bearing step additionally records **numeric line AND branch
  percentages**.
- **Phase 1 line locators are pre-deletion coordinates.** Every `BreadcrumbPopupUiOperations.cs` line
  number cited in Phase 1 (`:58`, `:105-110`, `:380-410`, `:412`, `:415`, `:416`, `:438`, `:457-492`)
  is read against the file **as it stands before `[P1-T2]`**. `[P1-T2]` deletes `:380-410` and other
  spans, so every locator after a deleted span shifts upward. All Phase 1 acceptances are
  **content-based** (member name, attribute count, symbol reference) and remain satisfiable after the
  shift. An executor must not treat a shifted line number as a defect; locate by content, not by
  line.
- **Phases 2 and 3 each contain an expected non-compiling window, and no acceptance inside either
  window requires a build.** In Phase 2 the tree does not compile from `[P2-T2]` through `[P2-T7]`:
  `[P2-T2]` writes `CoreWebView2MessageChannel` against `WebView2Messenger.ExtractPayload`, which is
  not created until `[P2-T5]`, and `WebView2Messenger` does not bind a channel until `[P2-T4]` and
  `[P2-T7]`. The first Phase 2 task whose acceptance records a compiling tree is `[P2-T11]`. In
  Phase 3 the tree does not compile from `[P3-T2]` through `[P3-T8]`: `[P3-T2]` writes
  `WebView2ControlSurface` against `IBreadcrumbControlSurface` while `WebView2BreadcrumbHost` is
  still mid-relocation across `[P3-T4]`, `[P3-T6]`, and `[P3-T7]`, and does not consume the surface
  until `[P3-T8]`. The first Phase 3 task whose acceptance records a compiling tree is `[P3-T12]`.
  This mirrors the Phase 1 window declared inline at `[P1-T3]` and closed by `[P1-T7]`. An executor
  must not run a build inside either window and must not treat the expected intermediate
  non-compiling state as a defect.

## Required References

- `CLAUDE.md`
- `.claude/rules/general-code-change.md`
- `.claude/rules/general-unit-test.md`
- `.claude/rules/quality-tiers.md`
- `.claude/rules/tonality.md`
- `<FEATURE>/spec.md` (AC-1 .. AC-25), `<FEATURE>/user-story.md` (US-1 .. US-11),
  `<FEATURE>/issue.md` (D1 .. D13)
- `<FEATURE>/research/00-cross-cutting-context.md` .. `<FEATURE>/research/11-BreadcrumbPopupUiOperations.md`
- `<EPIC>/epic.md`

All work must comply with these policies; this plan does not restate their content.

## Upstream Dependency Handling — Halt Gate, Evaluated at Execution Time

`<EPIC>/coverage-ledger.md` and F1's per-file coverage harness **do not exist on the branch this
plan was authored against, and that is expected**. F1 (#432) is being prepared concurrently and
lands on the integration branch before F13 executes.

`[P0-T5]` is therefore an **execution-time** existence test, not a planning-time precondition. When
this plan runs, the executor tests for `<EPIC>/coverage-ledger.md` from the repository root; if the
file is absent at that moment, execution **halts at Phase 0**, no Phase 1 task runs, and the
executor reports `BLOCKED ON F1 (#432)`. Genuine absence at execution time is an epic-orchestrator
sequencing failure raised then, not a defect in this plan.

F1's per-file harness is a **soft** dependency. If the ledger exists but no harness script is
published, every per-file coverage figure in this plan is derived from the Cobertura produced by
`scripts/vscode/Invoke-MSTestWithCoverage.ps1`, applying the reading rules recorded in `[P0-T6]`.

## Measurement Rules (binding on every coverage task in this plan)

1. Key on the Cobertura `filename=` attribute. **Never** on `<class name=>`. Three F13 files prove
   the necessity: `BreadcrumbPopupPlacement.cs` reports as `…BreadcrumbPopupPlacementResult`,
   `BreadcrumbWebViewSurfaceFactory.cs` as `…BreadcrumbNavigationReadiness`, and
   `BreadcrumbDropDownOpenLifetime.cs` as `…BreadcrumbDropDownOpenLease`.
2. Sum **class-level** `<lines>` children only, deduplicated by line number with `max(hits)`.
   Never sum `<method>` blocks. Never read a `<class>` `line-rate` / `branch-rate` attribute — they
   are inflated by open issue **#441** (on `BreadcrumbPopupUiOperations.cs` the inflation is exactly
   +2.24 points line and +1.46 points branch).
3. Repository-wide figures are captured **before and after in the same session on the same branch**,
   over the full `*.Test.dll` set (`-SearchRoot '.'`), run from the executing worktree root.

## Irreducible Outcomes — No Task May Target These, and No File Below May Be Targeted at 100%

| File | Outcome | Consequence |
|---|---|---|
| `QuickFiler/Viewers/BreadcrumbCollapsedSurfaceController.cs:245-246` | `IsCurrent` c2/c3 operands; `InvalidateGeneration` is the sole atomic writer of both fields | branch ceiling **95.24%** |
| `QuickFiler/Viewers/BreadcrumbUiDispatcher.cs:276` | `_ownerThreadId.HasValue` unreachable across all 24 construction sites | branch ceiling **97.22%** |
| `QuickFiler/Viewers/BreadcrumbWebViewSurfaceFactory.cs:221-222` | Roslyn `catch { await …; throw; }` rewrite artifact | ceilings **99.29% line / 97.62% branch** |
| `QuickFiler/Viewers/BreadcrumbDropDownOpenLifetime.cs:359` (and `:260` second `&&` operand) | leave-target of a catch that always rethrows; short-circuit precedence | line ceiling **99.13%** |
| `QuickFiler/Viewers/BreadcrumbPopupUiOperations.cs:325` and half of `:324` | `await` inside `catch` | ceilings **99.57% line / 99.17% branch** |
| `QuickFiler/Viewers/BreadcrumbDropDownHost.cs:420` (`_disposed == true`) | lease invalidated before the queued lambda can run | branch ceiling **~97.9%** |
| `QuickFiler/Viewers/BreadcrumbDropDownOpenCoordinator.cs:241-242` | `CloseCore` released guard, unreachable through the public surface | branch ceiling **~98.9%** |

## Hard Constraints (binding on every task)

- **Determinism.** Manually-pumped fake `SynchronizationContext` with an explicit `Drain()`; pattern
  green at `QuickFiler.Test/Viewers/BreadcrumbPopupUiOperationsDirectAdapterTests.cs:274-300`.
  **No injected clock, no `TimeProvider`, no fake-timer facility** — there is no `DateTime`,
  `Stopwatch`, `Timer`, or `TimeProvider` anywhere in these files. No `Thread.Sleep`, no
  `Task.Delay`, no wall-clock wait. **No STA and no `*.StaTests.cs` file.** No live forms, no shown
  popups, no temporary files, no external services or processes.
- **Frozen signatures.** Every `public` / `internal` signature in the 15 in-scope files is frozen.
  Six sibling children compile against them.
- **Designer field.** `QuickFiler/Viewers/ItemViewer.Designer.cs` must remain byte-identical;
  `_l0vhBreadcrumb_WebView2` (`:6214`) must not be retyped. It is pinned by a live green reflection
  test at `QuickFiler.Test/Viewers/ItemViewerBreadcrumbDropDownContractTests.cs:18-29`.
- **Sibling boundaries — do not edit.** F12 owns `QuickFiler/Controllers/BreadcrumbBridgeRouter.cs`,
  `QuickFiler/Viewers/BreadcrumbBridgeCoordinator.cs`,
  `QuickFiler/Viewers/BreadcrumbCoordinatorUpgradeLifetime.cs`,
  `QuickFiler/Viewers/BreadcrumbItemViewerLifecycleCoordinator.cs`,
  `QuickFiler/Viewers/BreadcrumbMessengerHub.cs`. F14 owns `QuickFiler/Viewers/ItemViewer.Breadcrumb.cs`.
  `BreadcrumbPopupLifecycleOperations` and `BreadcrumbNavigationSubscription` are declared **inside**
  F12's `BreadcrumbItemViewerLifecycleCoordinator.cs` at `:355` and `:337`.
- **csproj mechanics.** `QuickFiler/QuickFiler.csproj` (121 `<Compile Include>` entries, F13 block at
  lines 396-411) and `QuickFiler.Test/QuickFiler.Test.csproj` (107 entries, breadcrumb block at lines
  58-91) are non-SDK explicit-include and **CRLF on every line**. Use the `Edit` tool, or `perl -0777`
  with explicit `\r\n`. A git-bash `sed -i` strips CRLF and guarantees a fan-in merge conflict.
- **Latent defects #457, #458, #462, #475, #476, #477 are already promoted and MUST NOT be fixed
  here.** Tests pin **current** behavior, not corrected behavior. This matters most for #462: the
  test covering `BreadcrumbDropDownOpenCoordinator`'s stale `_closePending` branch asserts today's
  behavior and cites #462 in its doc comment.
- **Interface-only files** receive no `[ExcludeFromCodeCoverage]`, are reported **N/A** (never 0%),
  and **no shape-assertion test may be written for them** to manufacture coverage.
- **`QuickFiler/Viewers/WebView2CoreInitializer.cs` retains its exemption** (contingent on the F1
  ruling read in `[P0-T6]`). No test may invoke `CreateEnvironmentAsync` or `EnsureCoreWebView2Async`.

## Decisions Record (rationale a reviewer would otherwise re-derive)

- **D-1.** `BreadcrumbPopupProductionSurface` is a **separate `internal static class`, not a
  `partial` of `BreadcrumbPopupUiOperations`.** An `[ExcludeFromCodeCoverage]` on one partial
  declaration applies to the whole type and would silently exempt all 234 currently-covered lines —
  Blocking under `epic.md:223`.
- **D-2.** `QuickFiler.Test/Viewers/WebViewTestDoubles.cs` is created in **Phase 2** (not Phase 3, as
  research artifact 08 §10 proposes) and extended in Phase 3, because the mandated phase order places
  `WebView2Messenger` before `WebView2BreadcrumbHost`. This removes a forward dependency; the file
  content is unchanged from what artifacts 08 and 09 specify.
- **D-3.** Research artifact 11 case **T8** (`BeginInitializationAsync_ProductionBinding_…`) is
  **not** authored. It was contingent on `BeginProductionInitialization` being reclassified
  `testable`; this plan relocates that member into the class-level-exempt
  `BreadcrumbPopupProductionSurface`, so the case would contribute nothing to any denominator. The
  finalizer risk of `FormatterServices.GetUninitializedObject(typeof(WebView2))` is therefore not
  taken on.
- **D-4.** Research artifact 06 optional case **O1**
  (`BeginNavigation_NavigateThrowsAfterConcurrentCancel_DoesNotDoubleDetach`) is **deferred**. It has
  a measured coverage delta of zero and `BreadcrumbWebViewSurfaceFactory.cs` is already at its
  structural ceiling. Recorded, not scheduled.
- **D-5.** Research artifact 05 optional case
  (`Report_SinkThrows_FallsBackToLogWithoutEscaping`) is **deferred** for the same reason:
  `BreadcrumbUiDispatcher.cs:251` already reports `hits="1"`, so the case adds zero coverage.
- **D-6.** No partial split is proposed for `BreadcrumbDropDownHost.cs` (480) or
  `BreadcrumbDropDownOpenLifetime.cs` (477). No production change is required for any recommended
  test case on either file, and a new `<Compile Include>` entry would add fan-in conflict surface for
  no benefit.

---

## Implementation Plan (Atomic Tasks)

### Phase 0 — Baseline Capture and Upstream Gate

- [ ] [P0-T1] Bootstrap the C# toolchain by running `pwsh -NoProfile -File scripts/vscode/Install-RepoDotNetSdk.ps1`, then `dotnet tool restore`, then `dotnet tool install --global dotnet-coverage` (or confirm it already resolves), from the repository root; acceptance: `<FEATURE>/evidence/baseline/toolchain-bootstrap.<ts>.md` records `EXIT_CODE: 0` for all three plus a resolving `dotnet tool run csharpier --version` and `dotnet-coverage --version`
- [ ] [P0-T2] Read `CLAUDE.md`, `.claude/rules/general-code-change.md`, `.claude/rules/general-unit-test.md`, `.claude/rules/quality-tiers.md`, and `.claude/rules/tonality.md` in that order; acceptance: `<FEATURE>/evidence/baseline/phase0-instructions-read.<ts>.md` records `Timestamp:`, `Policy Order:`, and the explicit list of files read
- [ ] [P0-T3] Read `<FEATURE>/spec.md`, `<FEATURE>/user-story.md`, and `<FEATURE>/issue.md` and transcribe the AC inventory (AC-1..AC-25, US-1..US-11, D1..D13) into `<FEATURE>/evidence/baseline/ac-inventory.<ts>.md`; acceptance: the artifact lists 25 spec ACs, 11 user-story ACs, and 13 deviations with no gaps
- [ ] [P0-T4] Read all twelve research artifacts `<FEATURE>/research/00-cross-cutting-context.md` through `<FEATURE>/research/11-BreadcrumbPopupUiOperations.md` and record the per-file recommended test-case identifiers into `<FEATURE>/evidence/baseline/research-testcase-index.<ts>.md`; acceptance: the artifact enumerates every case ID this plan schedules and flags any case ID in a research artifact that this plan does not schedule, with the Decisions-Record reference
- [ ] [P0-T5] **HALT GATE.** At execution time, verify from the repository root that `<EPIC>/coverage-ledger.md` exists; if it is absent, halt immediately, run no Phase 1 task, and report `BLOCKED ON F1 (#432)`; acceptance: `<FEATURE>/evidence/qa-gates/f1-ledger-halt-gate.<ts>.md` records the tested path, the boolean existence result, and either `GATE: PASS` or `GATE: HALT — BLOCKED ON F1 (#432)`
- [ ] [P0-T6] Read `<EPIC>/coverage-ledger.md` and transcribe verbatim (a) its three bucket definitions, (b) its classification rules for mid-wave file creation, (c) the F1 ruling on the proposed fourth exemption ground (d) per `spec.md` §4.5, and (d) the per-file harness command if one is published; acceptance: `<FEATURE>/evidence/qa-gates/f1-ledger-contract.<ts>.md` states either `GROUND_D: RATIFIED` or `GROUND_D: DECLINED`, and either `HARNESS: <command>` or `HARNESS: ABSENT — using Invoke-MSTestWithCoverage.ps1 fallback per Measurement Rules`
- [ ] [P0-T7] Record the executing branch, `git rev-parse HEAD`, `git merge-base HEAD origin/epic/quickfiler-per-file-coverage-integration`, and `git status --porcelain`; acceptance: `<FEATURE>/evidence/baseline/tree-state.<ts>.md` records all four with `EXIT_CODE: 0` and an empty porcelain output
- [ ] [P0-T8] Run `dotnet tool run csharpier check .` from the repository root; acceptance: `<FEATURE>/evidence/baseline/csharpier-check.<ts>.md` records `Command:`, `EXIT_CODE:`, and an `Output Summary:` naming any pre-existing unformatted file
- [ ] [P0-T9] Run `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`; acceptance: `<FEATURE>/evidence/baseline/msbuild-analyzers.<ts>.md` records `Command:`, `EXIT_CODE:`, and warning/error counts in `Output Summary:`
- [ ] [P0-T10] Run `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true`; acceptance: `<FEATURE>/evidence/baseline/msbuild-nullable.<ts>.md` records `Command:`, `EXIT_CODE:`, and the nullable-diagnostic count in `Output Summary:`
- [ ] [P0-T11] Run `pwsh -NoProfile -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot '.' -Configuration Debug -CoverageOutput '<FEATURE>/evidence/baseline/coverage-baseline.<ts>.cobertura.xml'` from the executing worktree root; acceptance: `<FEATURE>/evidence/baseline/coverage-baseline.<ts>.md` records `Command:`, `EXIT_CODE:`, total/passed/failed test counts, and the repository-wide **numeric line-rate and branch-rate** read from the emitted Cobertura root `<coverage>` element
- [ ] [P0-T12] Recompute per-file baseline line and branch rates for all 11 in-scope production files from `<FEATURE>/evidence/baseline/coverage-baseline.<ts>.cobertura.xml`, keying on `filename=` and summing deduplicated class-level `<line>` children with `max(hits)`; acceptance: `<FEATURE>/evidence/baseline/per-file-baseline.<ts>.md` reproduces the eight instrumented figures (99.42/91.49, 99.13/91.86, 98.25/92.05, 98.97/85.71, 100/97.22, 99.29/97.62, 100/100, 90.70/88.33) and records the three WebView2 files as `ABSENT (exempt, unmeasured)`, with an explicit note that no `<class>` `line-rate` attribute was used
- [ ] [P0-T13] Record a line-count listing for the 15 in-scope production/interface files and for every F13-relevant test file under `QuickFiler.Test/Viewers/` and `QuickFiler.Test/Controllers/`; acceptance: `<FEATURE>/evidence/baseline/line-counts.<ts>.md` records `BreadcrumbPopupUiOperations.cs` at 494, `BreadcrumbDropDownIntegrationTests.cs` at 500, `BreadcrumbDropDownHostTests.cs` at 499, and `BreadcrumbDropDownReadinessTests.cs` at 498
- [ ] [P0-T14] Verify and record that every line of `QuickFiler/QuickFiler.csproj` and `QuickFiler.Test/QuickFiler.Test.csproj` is CRLF-terminated, together with each file's total line count and `<Compile Include>` entry count; acceptance: `<FEATURE>/evidence/baseline/csproj-crlf.<ts>.md` records 593/593 CRLF lines and 121 entries for `QuickFiler.csproj` and full CRLF plus 107 entries for `QuickFiler.Test.csproj`

### Phase 1 — BreadcrumbPopupUiOperations

- [ ] [P1-T1] Create `QuickFiler/Viewers/BreadcrumbPopupProductionSurface.cs` as an `internal static class` carrying exactly one **type-level** `[ExcludeFromCodeCoverage]`, declared **NOT `partial`** and sharing no type identity with `BreadcrumbPopupUiOperations`, holding relocated copies of `ShowOwnedPopup`, `CreateProductionControl`, `BeginProductionInitialization`, `ReadProductionCore`, `BeginProductionNavigation`, `BindNavigation` (from `BindProductionNavigation`), plus a new `NavigationBindingFor(BreadcrumbUiDispatcher)` returning the delegate currently written inline as the lambda at `BreadcrumbPopupUiOperations.cs:58`, and add its `<Compile Include="Viewers\BreadcrumbPopupProductionSurface.cs" />` entry inside the F13 block of `QuickFiler/QuickFiler.csproj` using the `Edit` tool, preserving CRLF; acceptance: the file compiles, is <= 500 lines, and contains exactly one `ExcludeFromCodeCoverage` occurrence, on the type declaration
- [ ] [P1-T2] Delete the six relocated members (`:105-110`, `:380-381`, `:383-388`, `:390-392`, `:394-410`, `:457-492`) from `QuickFiler/Viewers/BreadcrumbPopupUiOperations.cs` and rewrite the production constructor at `:52-60` to bind the four relocated method groups plus `BreadcrumbPopupProductionSurface.NavigationBindingFor(dispatcher)` and the local `DisposeProductionSurface`; acceptance: no lambda remains in the production constructor, and the `Microsoft.Web.WebView2.WinForms` using directive is removed if it becomes unreferenced
- [ ] [P1-T3] Rebind `NavigateToDocument`'s default binder at `QuickFiler/Viewers/BreadcrumbPopupUiOperations.cs:438` to `BreadcrumbPopupProductionSurface.BindNavigation`; acceptance: the file contains no reference to the deleted `BindProductionNavigation` and the relocated target resolves against the `<Compile Include>` entry added by `[P1-T1]`; the build is not exercised here because `QuickFiler/Viewers/BreadcrumbDropDownHost.cs:74` still names the `[P1-T2]`-deleted `BreadcrumbPopupUiOperations.ShowOwnedPopup` until `[P1-T4]` — the compiling build is recorded by `[P1-T7]`
- [ ] [P1-T4] Rebind the single `ShowOwnedPopup` call site at `QuickFiler/Viewers/BreadcrumbDropDownHost.cs:74` from `BreadcrumbPopupUiOperations.ShowOwnedPopup` to `BreadcrumbPopupProductionSurface.ShowOwnedPopup`; acceptance: a repository grep for `BreadcrumbPopupUiOperations.ShowOwnedPopup` returns zero matches and `BreadcrumbDropDownHost.cs` remains <= 500 lines
- [ ] [P1-T5] Remove the `[ExcludeFromCodeCoverage]` attribute at `QuickFiler/Viewers/BreadcrumbPopupUiOperations.cs:412` on `DisposeProductionSurface`, leaving the member in the primary file with its signature `(Control?, IWebViewMessenger?)` unchanged; acceptance: the file's `ExcludeFromCodeCoverage` occurrence count is zero and `<FEATURE>/evidence/qa-gates/ac08-dispose-exemption-removal.<ts>.md` records the before/after attribute counts (AC-8, D4)
- [ ] [P1-T6] Verify the `<Compile Include="Viewers\BreadcrumbPopupProductionSurface.cs" />` entry added by `[P1-T1]` is the only `QuickFiler/QuickFiler.csproj` change in this phase; acceptance: the file's CRLF line count incremented by exactly one, the entry count is 122, and no property, reference, or existing entry ordering changed (AC-16)
- [ ] [P1-T7] Verify `QuickFiler/Viewers/BreadcrumbPopupUiOperations.cs` is **<= 420 lines** and `QuickFiler/Viewers/BreadcrumbPopupProductionSurface.cs` is <= 500 lines, and that `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU"` succeeds; acceptance: `<FEATURE>/evidence/qa-gates/ac05-line-count-and-build.<ts>.md` records both counts and `EXIT_CODE: 0` (AC-5, AC-15)
- [ ] [P1-T8] Append a `<EPIC>/coverage-ledger.md` row for `QuickFiler/Viewers/BreadcrumbPopupProductionSurface.cs` as `ratified-exempt` with the rationale "third-party WebView2 SDK forwarders and WinForms popup presentation with no seam beneath them; zero decision logic; every consumer already injects a delegate seam over it; relocation of six previously-ratified member exemptions minus one withdrawn", and update the `BreadcrumbPopupUiOperations.cs` row to `testable`; acceptance: both rows are present in the same change as the `<Compile Include>` entry added by `[P1-T1]` (AC-17)
- [ ] [P1-T9] Create `QuickFiler.Test/Viewers/BreadcrumbPopupUiOperationsGuardTests.cs` containing research case **T1** `Constructor_AnyNullDependency_ThrowsArgumentNullExceptionNamingIt` as a `[DataTestMethod]` with six `[DataRow]`s indexed 0-5 asserting `ArgumentNullException` with parameter names `dispatcher`, `create`, `initialize`, `readCore`, `navigate`, `dispose` (closes `:71`, `:72`, `:73`, `:75`, `:76`, `:77`), and add its `<Compile Include="Viewers\BreadcrumbPopupUiOperationsGuardTests.cs" />` entry to the breadcrumb block of `QuickFiler.Test/QuickFiler.Test.csproj` preserving CRLF; acceptance: the six rows pass and the csproj entry count increments by one
- [ ] [P1-T10] Add research case **T2** `NullOperationAndNullBinder_FaultWithArgumentNullException` to `QuickFiler.Test/Viewers/BreadcrumbPopupUiOperationsGuardTests.cs`, asserting (a) `ObserveInitializationAsync(null)` and `ObserveReadinessAsync(null)` fault the awaited task with `ArgumentNullException("operation")` (closes `:330`) and (b) `NavigateToDocumentCore(dispatcher, core, owner, () => {}, "Popup", null)` throws `ArgumentNullException("bindNavigation")` (closes `:453`), using `FormatterServices.GetUninitializedObject` tokens for `core`/`owner` that are never dereferenced; acceptance: the test passes
- [ ] [P1-T11] Add `PrimaryType_CarriesNoExcludeFromCodeCoverageAttribute` to `QuickFiler.Test/Viewers/BreadcrumbPopupUiOperationsGuardTests.cs` asserting by reflection that `typeof(BreadcrumbPopupUiOperations)` carries no `ExcludeFromCodeCoverageAttribute` and that `typeof(BreadcrumbPopupProductionSurface)` carries exactly one; acceptance: the test passes (AC-9)
- [ ] [P1-T12] Create `QuickFiler.Test/Viewers/BreadcrumbPopupInstallEdgeTests.cs` containing research case **T3** `CreateAndInstallSurfaceAsync_FactoryYieldsNullSurface_ThrowsDiagnosticAndCleansUp`, driving the factory `environment => Task.FromResult<Tuple<Control, IWebViewMessenger, Task>>(null)` and asserting `InvalidOperationException` with message `"Popup initialization did not provide a control, messenger, and readiness task."`, an empty `dropDown.Items`, and exactly one error-sink entry (closes `:259` condition 0), and add its `<Compile Include>` entry preserving CRLF; acceptance: the test passes
- [ ] [P1-T13] Add research case **T4** `CreateAndInstallSurfaceAsync_CancellationWithNonDisposableMessenger_DisposesControlOnly` to `QuickFiler.Test/Viewers/BreadcrumbPopupInstallEdgeTests.cs` with `cancellation = Task.CompletedTask`, readiness = a never-completed test-owned `TaskCompletionSource.Task`, and messenger = `new Mock<IWebViewMessenger>(MockBehavior.Strict).Object`, asserting a `null` return, the control disposed exactly once, and no error reported (closes `:274`); acceptance: the test passes
- [ ] [P1-T14] Create `QuickFiler.Test/Viewers/BreadcrumbPopupDisposalBoundaryTests.cs` containing research case **T5** `DisposeHostedSurfaceAsync_ReportFailureFalse_SuppressesTheReportButStillThrows`, using a host whose `Dispose()` throws and calling `DisposeHostedSurfaceAsync(dropDown, host, control, messenger, reportFailure: false)`, asserting the returned task faults with that exception and the dispatcher error sink is empty, contrasted against the `reportFailure: true` default (closes `:364`), and add its `<Compile Include>` entry preserving CRLF; acceptance: the test passes
- [ ] [P1-T15] Add research case **T6** `DisposeSurfaceAsync_ControlOnly_DisposesControlAndSkipsMessenger` to `QuickFiler.Test/Viewers/BreadcrumbPopupDisposalBoundaryTests.cs`, constructing operations through the production constructor so `_disposeSurface` binds `DisposeProductionSurface`, calling with `(control: trackingControl, messenger: null)` and asserting the control is disposed exactly once (closes the `:415` null half and the `:416` non-null half); acceptance: the test passes and depends on `[P1-T5]` having removed the attribute
- [ ] [P1-T16] Add research case **T7** `DisposeSurfaceAsync_NonDisposableMessengerWithoutControl_CompletesWithoutDisposal` to `QuickFiler.Test/Viewers/BreadcrumbPopupDisposalBoundaryTests.cs`, calling with `(control: null, messenger: strictMockMessenger)` and asserting the task completes with no exception and `MockBehavior.Strict` records no call (closes the `:415` non-`IDisposable` half and the `:416` null half); acceptance: the test passes
- [ ] [P1-T17] Verify each of the three new test files is <= 500 lines and run them with `$vstest = vswhere -latest -products * -find Common7\IDE\Extensions\TestPlatform\vstest.console.exe; & $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation /TestCaseFilter:"FullyQualifiedName~BreadcrumbPopupUiOperationsGuardTests|FullyQualifiedName~BreadcrumbPopupInstallEdgeTests|FullyQualifiedName~BreadcrumbPopupDisposalBoundaryTests"`; acceptance: `<FEATURE>/evidence/regression-testing/phase1-scoped-run.<ts>.md` records `Command:`, `EXIT_CODE: 0`, passed/failed counts, and the three line counts
- [ ] [P1-T18] Recompute per-file coverage for `QuickFiler/Viewers/BreadcrumbPopupUiOperations.cs` from a fresh Cobertura run under the Measurement Rules; acceptance: `<FEATURE>/evidence/qa-gates/ac05-popupuiops-coverage.<ts>.md` records **>= 99.0% line and >= 97.5% branch** (projected 99.57% / 99.17%), confirms source lines 58, 406, 409 and 471-490 no longer appear in that file's Cobertura `<lines>` block, and records that 4/4 conditions are covered on the two `DisposeProductionSurface` body lines that were source 415 and 416 before the `[P1-T2]` deletions (AC-5, AC-8, AC-9)
- [ ] [P1-T19] Record the irreducible residue for this file — line `325` and half of line `324`, both artifacts of `await` inside `catch` — with the Roslyn-rewrite proof; acceptance: `<FEATURE>/evidence/qa-gates/irreducible-popupuiops.<ts>.md` names both outcomes, states the ceilings 99.57% line / 99.17% branch, and confirms no task in this plan targets them (AC-24)

### Phase 2 — WebView2Messenger

- [ ] [P2-T1] Create `QuickFiler/Viewers/IWebViewMessageChannel.cs` declaring `internal interface IWebViewMessageChannel` with exactly `void Subscribe(Action<string> onPayload)`, `void Unsubscribe()`, and `void PostJson(string json)`, carrying **no** `[ExcludeFromCodeCoverage]`, and add its `<Compile Include="Viewers\IWebViewMessageChannel.cs" />` entry inside the F13 block of `QuickFiler/QuickFiler.csproj` using the `Edit` tool, preserving CRLF; acceptance: no WebView2 type appears in any member signature and the file compiles (AC-10, AC-11, AC-16)
- [ ] [P2-T2] Create `QuickFiler/Viewers/CoreWebView2MessageChannel.cs` as `internal sealed class CoreWebView2MessageChannel : IWebViewMessageChannel` with exactly one **class-level** `[ExcludeFromCodeCoverage]`, wrapping one `CoreWebView2`, holding the bridging `EventHandler<CoreWebView2WebMessageReceivedEventArgs>` field, and implementing the inbound bridge as `_bridge = (_, e) => onPayload(WebView2Messenger.ExtractPayload(e.TryGetWebMessageAsString, () => e.WebMessageAsJson));`, and add its `<Compile Include="Viewers\CoreWebView2MessageChannel.cs" />` entry inside the F13 block of `QuickFiler/QuickFiler.csproj` using the `Edit` tool, preserving CRLF; acceptance: the type contains zero branches and zero mutable state beyond the bridge field, and exactly the five SDK statements `WebMessageReceived +=`, `PostWebMessageAsJson`, `WebMessageReceived -=`, `TryGetWebMessageAsString()`, and `WebMessageAsJson` (AC-10, AC-16)
- [ ] [P2-T3] Remove the class-level `[ExcludeFromCodeCoverage]` at `QuickFiler/Viewers/WebView2Messenger.cs:20`; acceptance: `<FEATURE>/evidence/qa-gates/ac07-messenger-deexemption.<ts>.md` records the removal and that the file's `ExcludeFromCodeCoverage` occurrence count is zero at this point in the phase, noting that `[P2-T7]` later reintroduces exactly one method-level occurrence on `CreateProductionChannel` (AC-7)
- [ ] [P2-T4] Add `internal WebView2Messenger(BreadcrumbUiDispatcher dispatcher, IWebViewMessageChannel channel)` as a non-exempt seam constructor to `QuickFiler/Viewers/WebView2Messenger.cs` and rechain the existing `public WebView2Messenger(CoreWebView2)` and `internal WebView2Messenger(CoreWebView2, BreadcrumbUiDispatcher)` to it **with `coreWebView` evaluated before `dispatcher`**, so a both-null call still reports `"coreWebView"`; acceptance: both existing signatures are byte-compatible, the seam constructor guards `dispatcher` and `channel`, and the parameter names `"coreWebView"` and `"dispatcher"` are preserved verbatim
- [ ] [P2-T5] Extract `internal static string ExtractPayload(Func<string> tryGetString, Func<string> readJson)` into `QuickFiler/Viewers/WebView2Messenger.cs` as a pure, **non-exempt** member preserving both existing fallbacks — the `catch (ArgumentException)` fallback and the independent `?? readJson()` coalesce; acceptance: the member carries no coverage attribute and both fallbacks are independently reachable
- [ ] [P2-T6] Extract `internal void HandleInboundPayload(string payload)` into `QuickFiler/Viewers/WebView2Messenger.cs` as a **non-exempt** member carrying the outer disposal guard, the dispatch, the inner disposal guard, and the `MessageReceived?.Invoke` raise; acceptance: the member carries no coverage attribute and the guard ordering at the previous `:99` and `:106` is preserved
- [ ] [P2-T7] Add `[ExcludeFromCodeCoverage] private static IWebViewMessageChannel CreateProductionChannel(CoreWebView2 coreWebView) => new CoreWebView2MessageChannel(coreWebView ?? throw new ArgumentNullException(nameof(coreWebView)));` to `QuickFiler/Viewers/WebView2Messenger.cs`; acceptance: the guard fires before any adapter construction and the parameter name is `coreWebView`
- [ ] [P2-T8] Verify the two `<Compile Include>` entries added by `[P2-T1]` and `[P2-T2]` are the only `QuickFiler/QuickFiler.csproj` changes in this phase; acceptance: the entry count is 124, CRLF is intact on every line, and no unrelated entry moved (AC-16)
- [ ] [P2-T9] Append `<EPIC>/coverage-ledger.md` rows for `IWebViewMessageChannel.cs` as `interface-only / not-measured` (N/A, no attribute) and `CoreWebView2MessageChannel.cs` as `ratified-exempt` with a per-statement rationale naming the five SDK statements; acceptance: both rows land in the same change as the `<Compile Include>` entries added by `[P2-T1]` and `[P2-T2]` (AC-17)
- [ ] [P2-T10] Verify `QuickFiler/Viewers/WebView2Messenger.cs` is <= 500 lines and that no pre-existing `public` or `internal` member signature changed, using a before/after signature listing; acceptance: `<FEATURE>/evidence/qa-gates/ac13-messenger-signatures.<ts>.md` records the line count and an empty signature diff for pre-existing members (AC-13, AC-15)
- [ ] [P2-T11] Create `QuickFiler.Test/Viewers/WebViewTestDoubles.cs` (no `[TestClass]`) containing an instance-based `FakeWebViewMessageChannel` recording `Subscribe`/`Unsubscribe`/`PostJson` and exposing the captured `Action<string>` with an opt-in throw hook, a `QueuedSynchronizationContext` with an explicit `Drain()`, and a recording error sink, all with **no mutable static state**, and add its `<Compile Include>` entry preserving CRLF; acceptance: the file compiles, is <= 500 lines, and contains no `Thread.Sleep`, `Task.Delay`, or wall-clock wait
- [ ] [P2-T12] Create `QuickFiler.Test/Viewers/WebView2MessengerConstructionTests.cs` containing research case **X1** `PublicConstructor_NullCore_ThrowsArgumentNullExceptionNamedCoreWebView` asserting `.WithParameterName("coreWebView")`, and add its `<Compile Include>` entry preserving CRLF; acceptance: the test passes
- [ ] [P2-T13] Add research case **X2** `PublicConstructor_WithoutAmbientSynchronizationContext_ThrowsInvalidOperationException` to `QuickFiler.Test/Viewers/WebView2MessengerConstructionTests.cs`, asserting the message emitted by `BreadcrumbUiDispatcher.CaptureCurrent()`; acceptance: the test passes with MSTest's default null ambient context
- [ ] [P2-T14] Add research case **X3** `PublicConstructor_WithAmbientSynchronizationContext_CapturesThatBoundary` to `QuickFiler.Test/Viewers/WebView2MessengerConstructionTests.cs`, setting the ambient `SynchronizationContext` to the queued fake inside a `try` and restoring it in a `finally`, then asserting **on the recorded `Post`** that the fake context captured exactly one queued callback and that the messenger's dispatcher targets that fake — **without calling `Drain()`**, because the public constructor binds the production `CoreWebView2MessageChannel` and draining the queued subscribe would invoke `WebMessageReceived +=` on a `FormatterServices.GetUninitializedObject(typeof(CoreWebView2))` token; acceptance: the test passes, asserts the recorded post rather than executing it, and the ambient context is restored on every exit path (AC-14)
- [ ] [P2-T15] Add research case **X4** `InternalConstructor_BothArgumentsNull_ReportsCoreWebViewFirst` to `QuickFiler.Test/Viewers/WebView2MessengerConstructionTests.cs` as the exception-fidelity regression guard, asserting `ArgumentNullException` with `.WithParameterName("coreWebView")` — not `"dispatcher"`, not `"core"`, not the adapter's parameter name; acceptance: the test passes and `<FEATURE>/evidence/regression-testing/ac12-exception-fidelity.<ts>.md` records the assertion and its rationale (AC-12)
- [ ] [P2-T16] Add research case **X5** `InternalConstructor_NullDispatcher_ThrowsArgumentNullExceptionNamedDispatcher` to `QuickFiler.Test/Viewers/WebView2MessengerConstructionTests.cs`, using a `FormatterServices.GetUninitializedObject(typeof(CoreWebView2))` token that is never dereferenced; acceptance: the test passes
- [ ] [P2-T17] Add research case **X6** `SeamConstructor_NullChannel_ThrowsArgumentNullException` to `QuickFiler.Test/Viewers/WebView2MessengerConstructionTests.cs`; acceptance: the test passes and asserts the seam constructor's own parameter name
- [ ] [P2-T18] Add research case **X7** `Construction_AfterDrain_SubscribesExactlyOnce` to `QuickFiler.Test/Viewers/WebView2MessengerConstructionTests.cs` using the queued context plus an explicit `Drain()`; acceptance: `FakeWebViewMessageChannel.SubscribeCount == 1`
- [ ] [P2-T19] Add research case **X8** `Construction_SubscribesWithASinkThatRoutesToMessageReceived` to `QuickFiler.Test/Viewers/WebView2MessengerConstructionTests.cs`, invoking the captured `Action<string>` and asserting `MessageReceived` fires end to end; acceptance: the test passes
- [ ] [P2-T20] Add research case **X9** `DisposeBeforeDrain_NeitherSubscribesNorUnsubscribes` to `QuickFiler.Test/Viewers/WebView2MessengerConstructionTests.cs`, constructing with the queued context, calling `Dispose()` before draining, then draining once; acceptance: both `SubscribeCount` and `UnsubscribeCount` are zero
- [ ] [P2-T21] Add research case **X10** `Type_IsNotExcludedFromCodeCoverage` to `QuickFiler.Test/Viewers/WebView2MessengerConstructionTests.cs` asserting by reflection that `typeof(WebView2Messenger)` carries no `ExcludeFromCodeCoverageAttribute` and `typeof(CoreWebView2MessageChannel)` carries exactly one; acceptance: the test passes (AC-7, AC-10)
- [ ] [P2-T22] Create `QuickFiler.Test/Viewers/WebView2MessengerPostTests.cs` containing research case **P1** `PostJson_Null_ThrowsArgumentNullExceptionNamedJson`, and add its `<Compile Include>` entry preserving CRLF; acceptance: the test passes
- [ ] [P2-T23] Add research case **P2** `PostJson_NullAfterDispose_StillThrowsArgumentNullException` to `QuickFiler.Test/Viewers/WebView2MessengerPostTests.cs`, asserting `ArgumentNullException` with parameter `"json"` rather than `ObjectDisposedException`, pinning the null-guard-before-disposed-check ordering; acceptance: the test passes (AC-12)
- [ ] [P2-T24] Add research case **P3** `PostJson_AfterDispose_ThrowsObjectDisposedException` to `QuickFiler.Test/Viewers/WebView2MessengerPostTests.cs` with `.WithMessage("*WebView2Messenger*")`; acceptance: the test passes
- [ ] [P2-T25] Add research case **P4** `PostJson_HappyPath_ForwardsExactJsonOnce` to `QuickFiler.Test/Viewers/WebView2MessengerPostTests.cs`; acceptance: `FakeWebViewMessageChannel` records exactly one `PostJson` with the exact string
- [ ] [P2-T26] Add research case **P5** `PostJson_EmptyString_IsForwarded` to `QuickFiler.Test/Viewers/WebView2MessengerPostTests.cs`, pinning that the guard is `== null` and not `IsNullOrEmpty`; acceptance: the empty string reaches the channel
- [ ] [P2-T27] Add research case **P6** `PostJson_DisposedBetweenDispatchAndDrain_DoesNotReachChannel` to `QuickFiler.Test/Viewers/WebView2MessengerPostTests.cs`, enqueuing the post, disposing, then draining; acceptance: `PostJson` never reaches the channel and no exception escapes
- [ ] [P2-T28] Add research case **P7** `PostJson_ChannelThrows_ReportsToDispatcherSinkAndDoesNotEscape` to `QuickFiler.Test/Viewers/WebView2MessengerPostTests.cs`; acceptance: the recording error sink holds exactly one entry and no exception propagates to the caller
- [ ] [P2-T29] Add research case **P8** `PostJson_InlineDispatcher_ForwardsSynchronously` to `QuickFiler.Test/Viewers/WebView2MessengerPostTests.cs` using `BreadcrumbUiDispatcher.CreateForCurrentThreadTests()`; acceptance: the channel receives the payload without any `Drain()` call
- [ ] [P2-T30] Create `QuickFiler.Test/Viewers/WebView2MessengerInboundTests.cs` containing research case **N1** `ExtractPayload_StringAvailable_ReturnsIt`, and add its `<Compile Include>` entry preserving CRLF; acceptance: the test passes against the pure static with two `Func<string>` arguments
- [ ] [P2-T31] Add research case **N2** `ExtractPayload_ArgumentException_FallsBackToJson` to `QuickFiler.Test/Viewers/WebView2MessengerInboundTests.cs`; acceptance: the `catch (ArgumentException)` fallback returns the JSON value
- [ ] [P2-T32] Add research case **N3** `ExtractPayload_NullString_CoalescesToJson` to `QuickFiler.Test/Viewers/WebView2MessengerInboundTests.cs`, exercising the second independent fallback; acceptance: the test passes without entering the catch
- [ ] [P2-T33] Add research case **N4** `ExtractPayload_NonArgumentException_Propagates` to `QuickFiler.Test/Viewers/WebView2MessengerInboundTests.cs`; acceptance: a non-`ArgumentException` escapes unchanged
- [ ] [P2-T34] Add research case **N5** `InboundPayload_RaisesMessageReceivedWithSenderIdentity` to `QuickFiler.Test/Viewers/WebView2MessengerInboundTests.cs`; acceptance: `sender` is reference-equal to the messenger
- [ ] [P2-T35] Add research case **N6** `InboundPayload_NoSubscriber_DoesNotThrow` to `QuickFiler.Test/Viewers/WebView2MessengerInboundTests.cs`; acceptance: the null-conditional raise completes without exception
- [ ] [P2-T36] Add research case **N7** `InboundPayload_AfterDispose_IsNotDispatchedAtAll` to `QuickFiler.Test/Viewers/WebView2MessengerInboundTests.cs`, invoking the captured sink after `Dispose()`; acceptance: nothing is enqueued and `MessageReceived` is not raised
- [ ] [P2-T37] Add research case **N8** `InboundPayload_DisposedBetweenDispatchAndDrain_IsNotRaised` to `QuickFiler.Test/Viewers/WebView2MessengerInboundTests.cs`; acceptance: the dispatched body no-ops at the inner guard
- [ ] [P2-T38] Add research case **N9** `InboundPayload_HandlerThrows_ReportsToDispatcherSink` to `QuickFiler.Test/Viewers/WebView2MessengerInboundTests.cs`; acceptance: the recording sink holds exactly one entry and the exception does not escape
- [ ] [P2-T39] Add research case **N10** `InboundPayload_ReentrantPostFromHandler_ReachesChannel` to `QuickFiler.Test/Viewers/WebView2MessengerInboundTests.cs`; acceptance: the nested dispatch runs inline and the channel receives the post with no deadlock
- [ ] [P2-T40] Add research case **N11** `InboundPayload_TwoSubscribers_BothInvokedInOrder` to `QuickFiler.Test/Viewers/WebView2MessengerInboundTests.cs`; acceptance: both handlers observe the payload in subscription order
- [ ] [P2-T41] Create `QuickFiler.Test/Viewers/WebView2MessengerDisposalTests.cs` containing research case **D1** `Dispose_AfterSubscribe_UnsubscribesExactlyOnce`, and add its `<Compile Include>` entry preserving CRLF; acceptance: the test passes
- [ ] [P2-T42] Add research case **D2** `Dispose_CalledTwice_UnsubscribesOnce` to `QuickFiler.Test/Viewers/WebView2MessengerDisposalTests.cs`, exercising the `Interlocked` single-entry gate; acceptance: `UnsubscribeCount == 1`
- [ ] [P2-T43] Add research case **D3** `Dispose_ClearsMessageReceivedSubscribers` to `QuickFiler.Test/Viewers/WebView2MessengerDisposalTests.cs`; acceptance: a handler added before disposal is not invoked afterwards
- [ ] [P2-T44] Add research case **D4** `Dispose_WhenChannelUnsubscribeThrows_StillClearsStateAndDoesNotEscape` to `QuickFiler.Test/Viewers/WebView2MessengerDisposalTests.cs`; acceptance: the `finally` still clears subscription state and no exception reaches the caller
- [ ] [P2-T45] Add research case **D5** `Dispose_WhenNeverSubscribed_DoesNotCallUnsubscribe` to `QuickFiler.Test/Viewers/WebView2MessengerDisposalTests.cs`; acceptance: `UnsubscribeCount == 0`
- [ ] [P2-T46] Add research case **D6** `IsDisposalRequested_TransitionsFalseToTrue` to `QuickFiler.Test/Viewers/WebView2MessengerDisposalTests.cs`, observed through `ThrowIfDisposed` behavior; acceptance: the transition is observable and one-way
- [ ] [P2-T47] Add research case **D7** `Dispose_ThenPostJson_ThrowsObjectDisposedException` to `QuickFiler.Test/Viewers/WebView2MessengerDisposalTests.cs`; acceptance: the end-to-end state transition is asserted
- [ ] [P2-T48] Verify each of the five files created in this phase under `QuickFiler.Test/Viewers/` is <= 500 lines and contains no `Thread.Sleep`, `Task.Delay`, wall-clock wait, temporary file, shown form, popup, STA attribute, injected clock, or `TimeProvider`; acceptance: `<FEATURE>/evidence/qa-gates/phase2-test-file-audit.<ts>.md` records each line count and a zero-match scan result (AC-14, AC-15)
- [ ] [P2-T49] Run `$vstest = vswhere -latest -products * -find Common7\IDE\Extensions\TestPlatform\vstest.console.exe; & $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation /TestCaseFilter:"FullyQualifiedName~WebView2Messenger"`; acceptance: `<FEATURE>/evidence/regression-testing/phase2-scoped-run.<ts>.md` records `Command:`, `EXIT_CODE: 0`, and 36 or more passing tests with zero failures
- [ ] [P2-T50] Recompute per-file coverage for `QuickFiler/Viewers/WebView2Messenger.cs` from a fresh Cobertura run under the Measurement Rules; acceptance: `<FEATURE>/evidence/qa-gates/ac07-messenger-coverage.<ts>.md` records that the file now has a `filename=` entry and measures **>= 90% line and >= 80% branch** (AC-7, US-1, US-2)

### Phase 3 — WebView2BreadcrumbHost

- [ ] [P3-T1] Create `QuickFiler/Viewers/IBreadcrumbControlSurface.cs` declaring `internal interface IBreadcrumbControlSurface` with exactly `CoreWebView2? ReadCore()`, `void PostJson(CoreWebView2 core, string json)`, `void NavigateToString(string html)`, `void BindInitializationHandler(Action<bool, Exception?> onCompleted)`, `void BindMessageHandler(Action<string> onPayload)`, and `Task EnsureCoreAsync(IWebViewCoreInitializer initializer, CoreWebView2Environment environment)`, carrying **no** `[ExcludeFromCodeCoverage]`, and add its `<Compile Include="Viewers\IBreadcrumbControlSurface.cs" />` entry inside the F13 block of `QuickFiler/QuickFiler.csproj` using the `Edit` tool, preserving CRLF; acceptance: the file compiles and the two `Bind*` members are documented as idempotent (AC-10, AC-11, AC-16)
- [ ] [P3-T2] Create `QuickFiler/Viewers/WebView2ControlSurface.cs` as `internal sealed class WebView2ControlSurface : IBreadcrumbControlSurface` with exactly one **class-level** `[ExcludeFromCodeCoverage]`, holding the `WebView2` control and the two bridge `EventHandler` fields required for idempotent unhook/hook, and add its `<Compile Include="Viewers\WebView2ControlSurface.cs" />` entry inside the F13 block of `QuickFiler/QuickFiler.csproj` using the `Edit` tool, preserving CRLF; acceptance: every member is a single statement with zero branches, and the file is <= 500 lines (AC-10, AC-16)
- [ ] [P3-T3] Remove the class-level `[ExcludeFromCodeCoverage]` at `QuickFiler/Viewers/WebView2BreadcrumbHost.cs:29`; acceptance: `<FEATURE>/evidence/qa-gates/ac06-host-deexemption.<ts>.md` records the removal and the file's remaining attribute occurrences (AC-6)
- [ ] [P3-T4] Add a non-exempt `internal WebView2BreadcrumbHost(IWebViewCoreInitializer initializer, IBreadcrumbControlSurface surface, Func<string> resolveCacheFolder)` seam constructor to `QuickFiler/Viewers/WebView2BreadcrumbHost.cs` with three null guards, and rewrite the existing `public WebView2BreadcrumbHost(WebView2, IWebViewCoreInitializer)` as a **method-level** `[ExcludeFromCodeCoverage]` production-wiring constructor chaining to it while preserving its `control` and `initializer` parameter names and guard order; acceptance: the public signature is byte-compatible and both guards still report their original parameter names
- [ ] [P3-T5] Extract `internal static string ResolveProductionCacheFolder()` into `QuickFiler/Viewers/WebView2BreadcrumbHost.cs` as a **non-exempt** member using `Environment.GetFolderPath` plus `Path.Combine`; acceptance: the member creates no file or directory and carries no coverage attribute
- [ ] [P3-T6] Extract `internal void HandleInitializationCompleted(bool isSuccess, Exception? initializationException)` into `QuickFiler/Viewers/WebView2BreadcrumbHost.cs` as a **non-exempt** member replacing the body of `OnCoreInitializationCompleted`, with the SDK-arg unwrap relocated to `WebView2ControlSurface.BindInitializationHandler`; acceptance: the failure branch, the null-exception `?.Message` path, the message-handler bind, the `IsCoreInitialized` transition, and the `CoreInitialized` raise all live in the non-exempt member
- [ ] [P3-T7] Extract `internal void RaiseMessageReceived(string payload)` into `QuickFiler/Viewers/WebView2BreadcrumbHost.cs` as a **non-exempt** member replacing the body of `OnWebMessageReceived`, with the SDK-arg unwrap relocated to `WebView2ControlSurface.BindMessageHandler`; acceptance: the null-conditional raise and the sender identity are preserved
- [ ] [P3-T8] Route `NavigateToString`, `PostMessageJson`, and `InitializeAsync` in `QuickFiler/Viewers/WebView2BreadcrumbHost.cs` through `IBreadcrumbControlSurface` without changing their signatures, keeping `IsCoreInitialized`, `MessageReceived`, and `CoreInitialized` exactly as declared; acceptance: a before/after signature listing shows zero changes to any pre-existing `public` or `internal` member (AC-13)
- [ ] [P3-T9] Verify the two `<Compile Include>` entries added by `[P3-T1]` and `[P3-T2]` are the only `QuickFiler/QuickFiler.csproj` changes in this phase; acceptance: the entry count is 126, CRLF is intact on every line, and no unrelated entry moved (AC-16)
- [ ] [P3-T10] Append `<EPIC>/coverage-ledger.md` rows for `IBreadcrumbControlSurface.cs` as `interface-only / not-measured` (N/A, no attribute) and `WebView2ControlSurface.cs` as `ratified-exempt` with a per-operation rationale; acceptance: both rows land in the same change as the `<Compile Include>` entries added by `[P3-T1]` and `[P3-T2]` (AC-17)
- [ ] [P3-T11] Verify `QuickFiler/Viewers/WebView2BreadcrumbHost.cs` is <= 500 lines and that `QuickFiler/Viewers/ItemViewer.Designer.cs` is byte-identical to its pre-change state; acceptance: `<FEATURE>/evidence/qa-gates/ac13-host-signatures-and-designer.<ts>.md` records the line count and the Designer file's unchanged SHA-256 (AC-13, AC-15)
- [ ] [P3-T12] Extend `QuickFiler.Test/Viewers/WebViewTestDoubles.cs` with an instance-based `FakeBreadcrumbControlSurface` recording every invocation and exposing the captured `Action<bool, Exception?>` and `Action<string>`, with no mutable static state; acceptance: the file remains <= 500 lines, contains no `Thread.Sleep`, `Task.Delay`, or wall-clock wait, and the Phase 2 messenger test classes still compile against it unmodified; the host test classes do not exist until `[P3-T13]`, so their compilation against this double is recorded by `[P3-T13]`
- [ ] [P3-T13] Create `QuickFiler.Test/Viewers/WebView2BreadcrumbHostContractTests.cs` containing research case **C1** `Constructor_NullControl_ThrowsArgumentNullException` asserting `.WithParameterName("control")`, and add its `<Compile Include>` entry preserving CRLF; acceptance: the test passes
- [ ] [P3-T14] Add research case **C2** `Constructor_NullInitializer_ThrowsArgumentNullException` to `QuickFiler.Test/Viewers/WebView2BreadcrumbHostContractTests.cs` asserting `.WithParameterName("initializer")`; acceptance: the test passes
- [ ] [P3-T15] Add research case **C3** `SeamConstructor_NullInitializer_ThrowsArgumentNullException` to `QuickFiler.Test/Viewers/WebView2BreadcrumbHostContractTests.cs`; acceptance: the test passes
- [ ] [P3-T16] Add research case **C4** `SeamConstructor_NullSurface_ThrowsArgumentNullException` to `QuickFiler.Test/Viewers/WebView2BreadcrumbHostContractTests.cs`; acceptance: the test passes
- [ ] [P3-T17] Add research case **C5** `SeamConstructor_NullCacheFolderResolver_ThrowsArgumentNullException` to `QuickFiler.Test/Viewers/WebView2BreadcrumbHostContractTests.cs`; acceptance: the test passes
- [ ] [P3-T18] Add research case **C6** `Construction_BindsInitializationHandlerExactlyOnce` to `QuickFiler.Test/Viewers/WebView2BreadcrumbHostContractTests.cs`; acceptance: `FakeBreadcrumbControlSurface` records exactly one `BindInitializationHandler`
- [ ] [P3-T19] Add research case **C7** `NewInstance_ReportsCoreNotInitialized` to `QuickFiler.Test/Viewers/WebView2BreadcrumbHostContractTests.cs`; acceptance: `IsCoreInitialized` is `false`
- [ ] [P3-T20] Add research case **C8** `Type_IsNotExcludedFromCodeCoverage` to `QuickFiler.Test/Viewers/WebView2BreadcrumbHostContractTests.cs` asserting by reflection that `typeof(WebView2BreadcrumbHost)` carries no `ExcludeFromCodeCoverageAttribute` and `typeof(WebView2ControlSurface)` carries exactly one; acceptance: the test passes (AC-6, AC-10)
- [ ] [P3-T21] Add research case **C9** `ProductionCacheFolder_IsRootedAndEndsWithWindowsFormsWebView2` to `QuickFiler.Test/Viewers/WebView2BreadcrumbHostContractTests.cs`; acceptance: the assertion passes and the test creates no file or directory (AC-14)
- [ ] [P3-T22] Create `QuickFiler.Test/Viewers/WebView2BreadcrumbHostInitializationTests.cs` containing research case **I1** `InitializeAsync_NullContext_ThrowsArgumentNullException` asserting `.WithParameterName("uiSyncContext")`, and add its `<Compile Include>` entry preserving CRLF; acceptance: the test passes
- [ ] [P3-T23] Add research case **I2** `InitializeAsync_MarshalsToUiContextBeforeCreatingEnvironment` to `QuickFiler.Test/Viewers/WebView2BreadcrumbHostInitializationTests.cs`, asserting the fake context records a `Post` before the first `CreateEnvironmentAsync` invocation; acceptance: the ordering assertion passes with an explicit `Drain()` and no wall-clock wait
- [ ] [P3-T24] Add research case **I3** `InitializeAsync_PassesResolvedCacheFolderAndNonNullOptions` to `QuickFiler.Test/Viewers/WebView2BreadcrumbHostInitializationTests.cs` using `Mock<IWebViewCoreInitializer>`; acceptance: `CreateEnvironmentAsync(expectedFolder, It.IsNotNull<CoreWebView2EnvironmentOptions>())` is verified exactly once
- [ ] [P3-T25] Add research case **I4** `InitializeAsync_PassesCreatedEnvironmentToEnsureCore` to `QuickFiler.Test/Viewers/WebView2BreadcrumbHostInitializationTests.cs`; acceptance: the environment token identity flows from creation to `EnsureCoreAsync`
- [ ] [P3-T26] Add research case **I5** `InitializeAsync_EnvironmentCreationFaults_PropagatesAndSkipsEnsureCore` to `QuickFiler.Test/Viewers/WebView2BreadcrumbHostInitializationTests.cs`; acceptance: the returned task faults and `EnsureCoreAsync` is never invoked
- [ ] [P3-T27] Add research case **I6** `InitializeAsync_EnsureCoreFaults_Propagates` to `QuickFiler.Test/Viewers/WebView2BreadcrumbHostInitializationTests.cs`; acceptance: the fault surfaces to the caller unchanged
- [ ] [P3-T28] Add research case **I7** `InitializeAsync_InvokedTwice_CreatesTwoEnvironments` to `QuickFiler.Test/Viewers/WebView2BreadcrumbHostInitializationTests.cs`, documenting current re-initialization behavior without changing it; acceptance: the invocation count is two and the test's doc comment states it pins current behavior only (AC-21, AC-25)
- [ ] [P3-T29] Create `QuickFiler.Test/Viewers/WebView2BreadcrumbHostMessagingTests.cs` containing research case **M1** `PostMessageJson_BeforeCoreInitialized_DropsPayload`, and add its `<Compile Include>` entry preserving CRLF; acceptance: `ReadCore()` returns null, `PostJson` is never invoked, and no exception is thrown
- [ ] [P3-T30] Add research case **M2** `PostMessageJson_AfterCoreInitialized_ForwardsExactJson` to `QuickFiler.Test/Viewers/WebView2BreadcrumbHostMessagingTests.cs`; acceptance: the exact string is forwarded exactly once
- [ ] [P3-T31] Add research case **M3** `PostMessageJson_EmptyString_IsForwardedNotDropped` to `QuickFiler.Test/Viewers/WebView2BreadcrumbHostMessagingTests.cs`, pinning that the guard is on the core and not the payload; acceptance: the empty string reaches the surface
- [ ] [P3-T32] Add research case **M4** `NavigateToString_ForwardsExactHtml` to `QuickFiler.Test/Viewers/WebView2BreadcrumbHostMessagingTests.cs`; acceptance: the exact HTML string reaches the surface exactly once
- [ ] [P3-T33] Add research case **M5** `RaiseMessageReceived_WithSubscriber_RaisesWithExactPayloadAndSenderIdentity` to `QuickFiler.Test/Viewers/WebView2BreadcrumbHostMessagingTests.cs`; acceptance: `sender` is reference-equal to the host
- [ ] [P3-T34] Add research case **M6** `RaiseMessageReceived_WithNoSubscriber_DoesNotThrow` to `QuickFiler.Test/Viewers/WebView2BreadcrumbHostMessagingTests.cs`; acceptance: the null-conditional raise completes cleanly
- [ ] [P3-T35] Add research case **M7** `RaiseMessageReceived_WithTwoSubscribers_InvokesBoth` to `QuickFiler.Test/Viewers/WebView2BreadcrumbHostMessagingTests.cs`; acceptance: both subscribers observe the payload
- [ ] [P3-T36] Create `QuickFiler.Test/Viewers/WebView2BreadcrumbHostLifecycleTests.cs` containing research case **L1** `InitializationCompleted_Failure_DoesNotTransitionOrRaise`, and add its `<Compile Include>` entry preserving CRLF; acceptance: `IsCoreInitialized` stays false, `CoreInitialized` is not raised, and `BindMessageHandler` is not called
- [ ] [P3-T37] Add research case **L2** `InitializationCompleted_FailureWithNullException_DoesNotThrow` to `QuickFiler.Test/Viewers/WebView2BreadcrumbHostLifecycleTests.cs`; acceptance: the `?.Message` path completes without exception
- [ ] [P3-T38] Add research case **L3** `InitializationCompleted_Success_BindsMessageHandlerOnce` to `QuickFiler.Test/Viewers/WebView2BreadcrumbHostLifecycleTests.cs`; acceptance: `BindMessageHandler` is recorded exactly once
- [ ] [P3-T39] Add research case **L4** `InitializationCompleted_Success_SetsIsCoreInitialized` to `QuickFiler.Test/Viewers/WebView2BreadcrumbHostLifecycleTests.cs`; acceptance: the flag transitions to true
- [ ] [P3-T40] Add research case **L5** `InitializationCompleted_Success_RaisesCoreInitializedOnceWithHostAsSender` to `QuickFiler.Test/Viewers/WebView2BreadcrumbHostLifecycleTests.cs`; acceptance: exactly one raise with `sender` reference-equal to the host
- [ ] [P3-T41] Add research case **L6** `InitializationCompleted_SuccessTwice_RebindsAndRaisesTwice` to `QuickFiler.Test/Viewers/WebView2BreadcrumbHostLifecycleTests.cs`, documenting pooled-viewer re-initialization as it behaves today; acceptance: two binds and two raises, with a doc comment stating current behavior is pinned, not corrected (AC-21, AC-25)
- [ ] [P3-T42] Add research case **L7** `InitializationCompleted_SuccessThenFailure_LeavesIsCoreInitializedTrue` to `QuickFiler.Test/Viewers/WebView2BreadcrumbHostLifecycleTests.cs`; acceptance: the flag never reverts
- [ ] [P3-T43] Add research case **L8** `CoreInitializedHandler_PostingReentrantly_ObservesInitializedState` to `QuickFiler.Test/Viewers/WebView2BreadcrumbHostLifecycleTests.cs`, pinning that the state transition precedes the raise; acceptance: the re-entrant post succeeds
- [ ] [P3-T44] Add research case **L9** `InboundMessageBoundHandler_DeliversPayloadThroughMessageReceived` to `QuickFiler.Test/Viewers/WebView2BreadcrumbHostLifecycleTests.cs`, driving the fake surface's captured `Action<string>` end to end; acceptance: `MessageReceived` fires with the exact payload
- [ ] [P3-T45] Verify each of the four test files created in this phase and the modified `QuickFiler.Test/Viewers/WebViewTestDoubles.cs` is <= 500 lines and contains no `Thread.Sleep`, `Task.Delay`, wall-clock wait, temporary file, shown form, popup, STA attribute, injected clock, or `TimeProvider`; acceptance: `<FEATURE>/evidence/qa-gates/phase3-test-file-audit.<ts>.md` records each line count and a zero-match scan result (AC-14, AC-15)
- [ ] [P3-T46] Run `$vstest = vswhere -latest -products * -find Common7\IDE\Extensions\TestPlatform\vstest.console.exe; & $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation /TestCaseFilter:"FullyQualifiedName~WebView2BreadcrumbHost"`; acceptance: `<FEATURE>/evidence/regression-testing/phase3-scoped-run.<ts>.md` records `Command:`, `EXIT_CODE: 0`, and 32 or more passing tests with zero failures
- [ ] [P3-T47] Recompute per-file coverage for `QuickFiler/Viewers/WebView2BreadcrumbHost.cs` from a fresh Cobertura run under the Measurement Rules; acceptance: `<FEATURE>/evidence/qa-gates/ac06-host-coverage.<ts>.md` records that the file now has a `filename=` entry and measures **>= 90% line and >= 80% branch** (AC-6, US-1, US-2)

### Phase 4 — WebView2CoreInitializer

- [ ] [P4-T1] Transcribe the F1 ruling recorded in `<FEATURE>/evidence/qa-gates/f1-ledger-contract.<ts>.md` and state the resulting classification of `QuickFiler/Viewers/WebView2CoreInitializer.cs`; acceptance: `<FEATURE>/evidence/qa-gates/ac02-fourth-ground-ruling.<ts>.md` records either `GROUND_D: RATIFIED — WebView2CoreInitializer.cs = ratified-exempt, attribute retained` or `GROUND_D: DECLINED — WebView2CoreInitializer.cs = testable, attribute removed` (AC-2)
- [ ] [P4-T2] Move `QuickFiler.Test/Controllers/WebView2CoreInitializerTests.cs` to `QuickFiler.Test/Viewers/WebView2CoreInitializerTests.cs`, updating its namespace to `QuickFiler.Test.Viewers`; acceptance: the source path no longer exists and the destination path does; the build is not exercised here because `QuickFiler.Test/QuickFiler.Test.csproj` still references the old path until `[P4-T3]` (AC-18, D13)
- [ ] [P4-T3] Remove `<Compile Include="Controllers\WebView2CoreInitializerTests.cs" />` from line 150 of `QuickFiler.Test/QuickFiler.Test.csproj` and add `<Compile Include="Viewers\WebView2CoreInitializerTests.cs" />` inside the breadcrumb block (lines 58-91) using the `Edit` tool, preserving CRLF; acceptance: the net entry count is unchanged, CRLF is intact on every line, and no unrelated entry moved (AC-16, AC-18)
- [ ] [P4-T4] Apply the ruling from `[P4-T1]` to `QuickFiler/Viewers/WebView2CoreInitializer.cs`: if `GROUND_D: RATIFIED`, leave the class-level attribute at `:15` in place and change no line; if `GROUND_D: DECLINED`, remove the attribute and record the measured line rate together with the `CLAUDE.md` §UT4 temporary-file and external-process prohibition citations; acceptance: `<FEATURE>/evidence/qa-gates/ac02-coreinitializer-disposition.<ts>.md` records which branch was taken, the resulting file state, and the measured figure when the declined branch applies (AC-2)
- [ ] [P4-T5] Preserve research case **K1** `Construction_YieldsAnIWebViewCoreInitializer` verbatim in `QuickFiler.Test/Viewers/WebView2CoreInitializerTests.cs`; acceptance: the assertion text is unchanged from the pre-move file and the test passes
- [ ] [P4-T6] Add research case **K2** `Seam_DeclaresCreateEnvironmentAsyncWithExpectedSignature` to `QuickFiler.Test/Viewers/WebView2CoreInitializerTests.cs`, asserting by reflection that `IWebViewCoreInitializer.CreateEnvironmentAsync` returns `Task<CoreWebView2Environment>` with parameters `(string cacheFolder, CoreWebView2EnvironmentOptions options)`; acceptance: the test passes and invokes no member
- [ ] [P4-T7] Add research case **K3** `Seam_DeclaresEnsureCoreWebView2AsyncWithExpectedSignature` to `QuickFiler.Test/Viewers/WebView2CoreInitializerTests.cs`, asserting by reflection a `Task` return with parameters `(WebView2 control, CoreWebView2Environment environment)`; acceptance: the test passes and invokes no member
- [ ] [P4-T8] Add research case **K4** `Adapter_ImplementsEverySeamMember` to `QuickFiler.Test/Viewers/WebView2CoreInitializerTests.cs`, asserting every `IWebViewCoreInitializer` member has a matching public method on `WebView2CoreInitializer`; acceptance: the test passes and guards against an explicit-interface-implementation regression
- [ ] [P4-T9] Add research case **K5** `Adapter_CoverageAttributeMatchesLedgerClassification` to `QuickFiler.Test/Viewers/WebView2CoreInitializerTests.cs`, asserting the presence of `ExcludeFromCodeCoverageAttribute` on `typeof(WebView2CoreInitializer)` when `[P4-T1]` recorded `GROUND_D: RATIFIED`, or its absence when `[P4-T1]` recorded `GROUND_D: DECLINED`; acceptance: the assertion direction matches the recorded ruling, the ruling has already been applied to the source by `[P4-T4]`, and the test passes
- [ ] [P4-T10] Add research case **K6** `Adapter_IsSealed` to `QuickFiler.Test/Viewers/WebView2CoreInitializerTests.cs`, preventing a subclass from silently inheriting the exemption; acceptance: the test passes
- [ ] [P4-T11] Verify that no test in `QuickFiler.Test/Viewers/WebView2CoreInitializerTests.cs` invokes `CreateEnvironmentAsync` or `EnsureCoreWebView2Async`; acceptance: `<FEATURE>/evidence/qa-gates/ac18-coreinitializer-no-invocation.<ts>.md` records a zero-match grep for both member names outside reflection-signature assertions (AC-18)
- [ ] [P4-T12] Update the `<EPIC>/coverage-ledger.md` row for `QuickFiler/Viewers/WebView2CoreInitializer.cs` with its bucket per `[P4-T1]` and the rationale wording "executing either member is prohibited, not merely difficult: `CreateEnvironmentAsync` creates and populates a user-data folder on disk and requires the Evergreen WebView2 Runtime; `EnsureCoreWebView2Async` additionally needs a created window handle and starts a browser process", noting that the file's own "1:1 forwarding" doc comment is false and is tracked as issue #477; acceptance: the row is present and the #477 reference is recorded (AC-17)
- [ ] [P4-T13] Run `$vstest = vswhere -latest -products * -find Common7\IDE\Extensions\TestPlatform\vstest.console.exe; & $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation /TestCaseFilter:"FullyQualifiedName~WebView2CoreInitializerTests"`; acceptance: `<FEATURE>/evidence/regression-testing/phase4-scoped-run.<ts>.md` records `Command:`, `EXIT_CODE: 0`, and six passing tests with zero failures

### Phase 5 — BreadcrumbCollapsedSurfaceController

- [ ] [P5-T1] Create `QuickFiler.Test/Viewers/BreadcrumbCollapsedSurfaceGenerationRaceTests.cs` containing a `private sealed class ReentrantDisposeMessenger : IWebViewMessenger, IDisposable` fake with an `Action? OnFirstDispose` hook and a `DisposeCount`, plus research case **T1** `PublishWindow_ResetDuringReplacedMessengerDisposal_RejectsStaleGeneration` (publish `m0`, arm `m0.OnFirstDispose = () => controller.Reset()`, act `AttachAsync(m1, Task.CompletedTask)`, assert `false`, `ReadyMessenger` null, `m1.DisposeCount == 1`), and add its `<Compile Include>` entry preserving CRLF; acceptance: the test passes and closes uncovered lines 198-199
- [ ] [P5-T2] Add research case **T2** `PublishWindow_DisposeDuringReplacedMessengerDisposal_RejectsStaleGeneration` to `QuickFiler.Test/Viewers/BreadcrumbCollapsedSurfaceGenerationRaceTests.cs` with `m0.OnFirstDispose = () => controller.Dispose()`; acceptance: the attachment resolves `false`, `ReadyMessenger` is null, `m1` is disposed exactly once, and a later `AttachAsync` throws `ObjectDisposedException`
- [ ] [P5-T3] Add research case **T3** `RejectPending_MessengerRePendedUnderNewGeneration_IsNotDisposed` to `QuickFiler.Test/Viewers/BreadcrumbCollapsedSurfaceGenerationRaceTests.cs` with `m0.OnFirstDispose = () => { controller.Reset(); controller.AttachAsync(m1, pendingTcs.Task); }`; acceptance: the first attachment resolves `false` and `m1.DisposeCount == 0`
- [ ] [P5-T4] Verify `QuickFiler.Test/Viewers/BreadcrumbCollapsedSurfaceGenerationRaceTests.cs` is <= 500 lines, uses only `Task.CompletedTask` or never-completed test-owned `TaskCompletionSource` instances for readiness, and shares no mutable static state; acceptance: `<FEATURE>/evidence/qa-gates/phase5-test-file-audit.<ts>.md` records the line count and a zero-match determinism scan (AC-14, AC-15)
- [ ] [P5-T5] Run `$vstest = vswhere -latest -products * -find Common7\IDE\Extensions\TestPlatform\vstest.console.exe; & $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation /TestCaseFilter:"FullyQualifiedName~BreadcrumbCollapsedSurface"`; acceptance: `<FEATURE>/evidence/regression-testing/phase5-scoped-run.<ts>.md` records `Command:`, `EXIT_CODE: 0`, and zero failures across the new and pre-existing collapsed-surface tests
- [ ] [P5-T6] Recompute per-file coverage for `QuickFiler/Viewers/BreadcrumbCollapsedSurfaceController.cs` and record the irreducible operands at `:245-246`; acceptance: `<FEATURE>/evidence/qa-gates/collapsedsurface-coverage.<ts>.md` records line **>= 98.97%** and branch **>= 85.71%** (goal 95.24%), states the branch ceiling as 95.24%, and confirms no task targets `:245` or `:246` (AC-4, AC-24)

### Phase 6 — BreadcrumbDropDownHost

- [ ] [P6-T1] Create `QuickFiler.Test/Viewers/BreadcrumbDropDownHostReentrancyTests.cs` containing its own local harness plus research case **T1** `OnDropDownClosed_AfterDispose_ReturnsWithoutCancellingOrFocusing` (open, `Dispose()`, drain, then reflect-invoke `OnDropDownClosed`; assert `CancelCount`/`FocusAnchorCount` unchanged and no new dispatch posted), and add its `<Compile Include>` entry preserving CRLF; acceptance: the test passes
- [ ] [P6-T2] Add research case **T2** `CloseNative_ReentrantClosedEvent_IsSuppressedByProgrammaticCloseGuard` to `QuickFiler.Test/Viewers/BreadcrumbDropDownHostReentrancyTests.cs`, constructing via the 9-argument internal constructor with a `closePopup` that reflect-invokes `OnDropDownClosed`; acceptance: exactly one `cancelSelection` and one `focusAnchor` are recorded
- [ ] [P6-T3] Add research case **T3** `QueuedClosedCallback_DrainedDuringProgrammaticClose_PerformsNoSecondClose` to `QuickFiler.Test/Viewers/BreadcrumbDropDownHostReentrancyTests.cs`; acceptance: `IsOpen == false`, one `cancelSelection`, one `focusAnchor`
- [ ] [P6-T4] Add research case **T4** `Dispose_OrphanedPopupControlWithoutControlHost_DisposesTheControlExactlyOnce` to `QuickFiler.Test/Viewers/BreadcrumbDropDownHostReentrancyTests.cs`, setting `InstalledPopupControl` to a fresh undisposed `Panel` with `InstalledControlHost` left null; acceptance: the control's `Disposed` event fires exactly once, closing uncovered lines 334-335
- [ ] [P6-T5] Add research case **T5** `DisposeSurfaceAfterFailure_ControlHostMismatch_RetainsInstalledSurface` to `QuickFiler.Test/Viewers/BreadcrumbDropDownHostReentrancyTests.cs` with a tuple whose `Item1` differs; acceptance: the installed surface is unchanged and nothing is disposed
- [ ] [P6-T6] Add research case **T6** `DisposeSurfaceAfterFailure_PopupControlMismatch_RetainsInstalledSurface` to `QuickFiler.Test/Viewers/BreadcrumbDropDownHostReentrancyTests.cs` with a tuple whose `Item2` differs; acceptance: the installed surface is unchanged and nothing is disposed
- [ ] [P6-T7] Add research case **T7** `DisposeSurfaceAfterFailure_MessengerMismatch_RetainsInstalledSurface` to `QuickFiler.Test/Viewers/BreadcrumbDropDownHostReentrancyTests.cs` with a tuple whose `Item3` differs; acceptance: the installed surface is unchanged, closing uncovered line 377
- [ ] [P6-T8] Add research case **T8** `Reset_WhenCancelSelectionThrows_StillClearsSurfaceAndResetPending` to `QuickFiler.Test/Viewers/BreadcrumbDropDownHostReentrancyTests.cs` with a throwing `cancelSelection`; acceptance: the surface is disposed, `LastInitializationException` is null, and the failure reaches the error sink exactly once
- [ ] [P6-T9] Verify `QuickFiler.Test/Viewers/BreadcrumbDropDownHostReentrancyTests.cs` is <= 500 lines and that no production edit was made to `QuickFiler/Viewers/BreadcrumbDropDownHost.cs` in this phase beyond the `[P1-T4]` call-site rebind; acceptance: `<FEATURE>/evidence/qa-gates/phase6-test-file-audit.<ts>.md` records the test line count and the production file at <= 500 lines (AC-15)
- [ ] [P6-T10] Run `$vstest = vswhere -latest -products * -find Common7\IDE\Extensions\TestPlatform\vstest.console.exe; & $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation /TestCaseFilter:"FullyQualifiedName~BreadcrumbDropDownHost"`; acceptance: `<FEATURE>/evidence/regression-testing/phase6-scoped-run.<ts>.md` records `Command:`, `EXIT_CODE: 0`, and zero failures across the new and pre-existing host tests
- [ ] [P6-T11] Recompute per-file coverage for `QuickFiler/Viewers/BreadcrumbDropDownHost.cs` and record the irreducible outcome at `:420`; acceptance: `<FEATURE>/evidence/qa-gates/dropdownhost-coverage.<ts>.md` records line **>= 99.42%** and branch **>= 91.49%** (goal ~97.9%), states the branch ceiling as ~97.9%, and confirms no task targets `:420` (AC-4, AC-24)

### Phase 7 — BreadcrumbDropDownOpenLifetime

- [ ] [P7-T1] Create `QuickFiler.Test/Viewers/BreadcrumbDropDownOpenLifetimeCancellationTests.cs` carrying its own harness derived from the existing `LifecycleHarness` pattern plus research case **T1** `TryCancelPendingOpen_NullCloseOperation_ThrowsWithCloseOperationParamName` (closes `:75`), and add its `<Compile Include>` entry preserving CRLF; acceptance: the test passes and asserts `ArgumentNullException.ParamName == "closeOperation"`
- [ ] [P7-T2] Add research case **T2** `Schedule_LeaseInvalidatedBeforeDrain_DoesNotRunTheOperation` to `QuickFiler.Test/Viewers/BreadcrumbDropDownOpenLifetimeCancellationTests.cs` (closes `:123`); acceptance: the scheduled operation never runs and the error sink is empty
- [ ] [P7-T3] Add research case **T3** `OpenAsync_LeaseInvalidatedDuringPlacement_CompletesFalseWithoutShowing` to `QuickFiler.Test/Viewers/BreadcrumbDropDownOpenLifetimeCancellationTests.cs`, hooking the surface `Control.SizeChanged` to bump the generation (closes `:237`/`:238`); acceptance: the open task completes `false`, `ShowCount == 0`, `FocusPendingCount == 0`, `LastInitializationException` is null, and the error sink is empty
- [ ] [P7-T4] Add research case **T4** `OpenAsync_LeaseInvalidatedBetweenPlacementAndShow_StopsBeforeSettingOpenState` to `QuickFiler.Test/Viewers/BreadcrumbDropDownOpenLifetimeCancellationTests.cs`, single-stepping the pump with `DrainOne()` (closes the first `&&` operand at `:260`); acceptance: `IsOpen == false` and `ShowCount == 0`
- [ ] [P7-T5] Add research case **T5** `OpenAsync_FocusPendingInvalidatesLifecycle_CompletesFalseAndLeavesExceptionUnset` to `QuickFiler.Test/Viewers/BreadcrumbDropDownOpenLifetimeCancellationTests.cs` with a `focusPending` that calls `Host.Reset()` (closes `:295`); acceptance: the open task completes `false`, `LastInitializationException` is null, and the surface is disposed exactly once
- [ ] [P7-T6] Add research case **T6** `Close_WhileOpenPending_CloseOperationThrows_CompletesFalseWithoutFaulting` to `QuickFiler.Test/Viewers/BreadcrumbDropDownOpenLifetimeCancellationTests.cs` (closes `:197`); acceptance: the shared open task reaches `RanToCompletion` with `Result == false`, `IsFaulted == false`, and the thrown exception reaches the error sink exactly once
- [ ] [P7-T7] Verify `QuickFiler.Test/Viewers/BreadcrumbDropDownOpenLifetimeCancellationTests.cs` is <= 500 lines and that `QuickFiler/Viewers/BreadcrumbDropDownOpenLifetime.cs` received no production edit and no partial split; acceptance: `<FEATURE>/evidence/qa-gates/phase7-test-file-audit.<ts>.md` records the test line count and the production file unchanged at 477 lines (AC-15, AC-20)
- [ ] [P7-T8] Run `$vstest = vswhere -latest -products * -find Common7\IDE\Extensions\TestPlatform\vstest.console.exe; & $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation /TestCaseFilter:"FullyQualifiedName~BreadcrumbDropDownOpenLifetime|FullyQualifiedName~BreadcrumbDropDownLifecycle"`; acceptance: `<FEATURE>/evidence/regression-testing/phase7-scoped-run.<ts>.md` records `Command:`, `EXIT_CODE: 0`, and zero failures
- [ ] [P7-T9] Recompute per-file coverage for `QuickFiler/Viewers/BreadcrumbDropDownOpenLifetime.cs` and record the irreducible outcomes at `:359` and the second `&&` operand at `:260`; acceptance: `<FEATURE>/evidence/qa-gates/openlifetime-coverage.<ts>.md` records line **>= 99.13%** and branch **>= 91.86%** (goal ~97.7%), states the line ceiling as 99.13%, and confirms no task targets either outcome (AC-4, AC-24)

### Phase 8 — BreadcrumbDropDownOpenCoordinator

- [ ] [P8-T1] Extend the `ControlledHost` fake in `QuickFiler.Test/Viewers/BreadcrumbDropDownOpenCoordinatorTests.cs` (research case **T0**) with additive `ClaimCloseWithoutClearingOpen` and `ReturnNullOpenTask` properties; acceptance: the file remains <= 500 lines, all pre-existing tests in it still pass unmodified, and the two properties default to the current behavior
- [ ] [P8-T2] Add research case **T1** `CurrentOpenTask_BeforeAnyRequest_IsACompletedClosedTask` to `QuickFiler.Test/Viewers/BreadcrumbDropDownOpenCoordinatorTests.Part2.cs` (closes `:65`); acceptance: the task is completed with `Result == false` without any `RequestOpen`
- [ ] [P8-T3] Add research case **T2** `RequestOpen_WhileClosePendingAndHostStillOpen_ReturnsClosedWithoutRequesting` to `QuickFiler.Test/Viewers/BreadcrumbDropDownOpenCoordinatorTests.Part2.cs` (closes `:92`/`:93`), with a doc comment stating it pins **current** behavior and referencing issue **#462** as the promoted defect that must not be fixed on this branch; acceptance: the returned task is completed `false`, `Host.Requests` is empty, `_closePending` is not cleared, and the `#462` reference is present (AC-25)
- [ ] [P8-T4] Add research case **T3** `SetDroppedDown_QueuedBodyDrainedAfterRelease_PerformsNoWork` to `QuickFiler.Test/Viewers/BreadcrumbDropDownOpenCoordinatorTests.Part2.cs` (closes `:106`/`:107`); acceptance: `OpenSelectorCalls == 0`, `SelectorOpenReads == 0`, and the host is untouched
- [ ] [P8-T5] Create `QuickFiler.Test/Viewers/BreadcrumbDropDownOpenCoordinatorTests.Part3.cs` declared `public sealed partial class BreadcrumbDropDownOpenCoordinatorTests` in `namespace QuickFiler.Test.Viewers` **without a second `[TestClass]` attribute**, containing research case **T4** `RequestOpen_ResetBeforeBeginOpenDrains_NeverConsultsProvidersOrHost` (closes `:186`/`:187`), and add its `<Compile Include>` entry preserving CRLF; acceptance: `Host.Requests` is empty, the row-count and anchor providers are never invoked, and the assembly still exposes exactly one `BreadcrumbDropDownOpenCoordinatorTests` test class
- [ ] [P8-T6] Add research case **T5** `RequestOpen_HostReturnsNullOpenTask_ReportsContractViolationAndRollsBack` to `QuickFiler.Test/Viewers/BreadcrumbDropDownOpenCoordinatorTests.Part3.cs` (closes `:195`); acceptance: the request completes `false` and the error sink holds exactly one `InvalidOperationException` with message `"The breadcrumb popup host returned no open task."`
- [ ] [P8-T7] Add research case **T6** `RequestOpen_ResetWhileOpenPending_LateSuccessIsClosedWithExplicitCommit` to `QuickFiler.Test/Viewers/BreadcrumbDropDownOpenCoordinatorTests.Part3.cs` (closes `:210`); acceptance: the request completes `false`, `Host.CloseReasons` equals `[ExplicitCommit]`, and `CancelCount == 0`
- [ ] [P8-T8] Add research case **T7** `RequestOpen_ResetWhileOpenPending_LateFailureDoesNotCancelTheLiveSelection` to `QuickFiler.Test/Viewers/BreadcrumbDropDownOpenCoordinatorTests.Part3.cs` (closes `:225`); acceptance: the request completes `false`, `CancelCalls` is unchanged, and the failure reaches the error sink exactly once
- [ ] [P8-T9] Verify `QuickFiler.Test/Viewers/BreadcrumbDropDownOpenCoordinatorTests.cs`, `.Part2.cs`, and `.Part3.cs` are each <= 500 lines and that `QuickFiler/Viewers/BreadcrumbDropDownOpenCoordinator.cs` received no production edit; acceptance: `<FEATURE>/evidence/qa-gates/phase8-test-file-audit.<ts>.md` records the line counts of all four named files — `BreadcrumbDropDownOpenCoordinatorTests.cs`, `BreadcrumbDropDownOpenCoordinatorTests.Part2.cs`, `BreadcrumbDropDownOpenCoordinatorTests.Part3.cs`, and `QuickFiler/Viewers/BreadcrumbDropDownOpenCoordinator.cs` — each at <= 500 lines (AC-15, AC-20)
- [ ] [P8-T10] Run `$vstest = vswhere -latest -products * -find Common7\IDE\Extensions\TestPlatform\vstest.console.exe; & $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation /TestCaseFilter:"FullyQualifiedName~BreadcrumbDropDownOpenCoordinatorTests"`; acceptance: `<FEATURE>/evidence/regression-testing/phase8-scoped-run.<ts>.md` records `Command:`, `EXIT_CODE: 0`, and zero failures
- [ ] [P8-T11] Recompute per-file coverage for `QuickFiler/Viewers/BreadcrumbDropDownOpenCoordinator.cs` and record the irreducible outcome at `:241-242`; acceptance: `<FEATURE>/evidence/qa-gates/opencoordinator-coverage.<ts>.md` records line **>= 98.25%** and branch **>= 92.05%** (goal ~98.9%), states the branch ceiling as ~98.9%, and confirms the coordinator still carries no `[ExcludeFromCodeCoverage]` (AC-4, AC-24)

### Phase 9 — BreadcrumbUiDispatcher

- [ ] [P9-T1] Recompute per-file coverage for `QuickFiler/Viewers/BreadcrumbUiDispatcher.cs` under the Measurement Rules and confirm 187/187 lines and 35/36 conditions are retained; acceptance: `<FEATURE>/evidence/qa-gates/uidispatcher-coverage.<ts>.md` records **100% line and 97.22% branch** with the raw numerator/denominator pairs (AC-4)
- [ ] [P9-T2] Record in `<EPIC>/coverage-ledger.md` that this file's branch ceiling is 35/36 = **97.22%**, with the proof that `_ownerThreadId.HasValue` at `:276` is unreachable across all three constructor paths and all 24 construction sites; acceptance: the ledger row carries the ceiling and the proof text (AC-24)
- [ ] [P9-T3] Record the determination that **no new test is warranted** for this file, citing that every guard, catch, inline/post decision, and the report-exactly-once contract already have a named passing test, and that a further test would be a shape assertion prohibited by `epic.md:521-522`; acceptance: `<FEATURE>/evidence/qa-gates/uidispatcher-no-new-test.<ts>.md` records the determination and confirms no test file was added or modified for this file in this child

### Phase 10 — BreadcrumbWebViewSurfaceFactory

- [ ] [P10-T1] Recompute per-file coverage for `QuickFiler/Viewers/BreadcrumbWebViewSurfaceFactory.cs` under the Measurement Rules and confirm 139/140 lines and 41/42 conditions are retained; acceptance: `<FEATURE>/evidence/qa-gates/surfacefactory-coverage.<ts>.md` records **99.29% line and 97.62% branch** with the raw numerator/denominator pairs (AC-4)
- [ ] [P10-T2] Record in `<EPIC>/coverage-ledger.md` that this file's ceilings are 99.29% line and 97.62% branch, caused by the Roslyn `catch { await …; throw; }` rewrite artifact at `:221-222`, so the capstone does not treat 100% as achievable; acceptance: the ledger row carries both ceilings and the artifact explanation (AC-24)
- [ ] [P10-T3] Record the harness reading directives for F1 — key on Cobertura `filename=`, sum class-level `<line>` children deduplicated with `max(hits)`, never sum `<method>` blocks, never read a `<class>` `line-rate`/`branch-rate` attribute — citing the three F13 files whose `<class>` name differs from their filename; acceptance: `<FEATURE>/evidence/qa-gates/harness-directives.<ts>.md` names `BreadcrumbPopupPlacement.cs`, `BreadcrumbWebViewSurfaceFactory.cs`, and `BreadcrumbDropDownOpenLifetime.cs` with their reported `<class>` names, and the same text is mirrored into the ledger (AC-3, D11)

### Phase 11 — BreadcrumbPopupPlacement

- [ ] [P11-T1] Create `QuickFiler.Test/Viewers/BreadcrumbPopupPlacementBoundaryTests.cs` calling `BreadcrumbPopupPlacement.Calculate(...)` **directly** with no reflection and no reference to any F12-owned type, containing research case **P1** `Calculate_DesiredHeightExactlyEqualsBelowSpace_OpensBelowAtFullHeight` (anchor `(100,100,200,25)`, working area `(0,0,800,600)`, desired `300x475`; assert `OpensBelow == true` and `Bounds == (100,125,300,475)`), and add its `<Compile Include>` entry preserving CRLF; acceptance: the test passes and kills the `<=` to `<` mutant at `:52`
- [ ] [P11-T2] Add research case **P2** `Calculate_DesiredHeightExactlyEqualsAboveSpace_OpensAboveAtFullHeight` to `QuickFiler.Test/Viewers/BreadcrumbPopupPlacementBoundaryTests.cs` (anchor `(100,400,200,25)`, working area `(0,0,800,600)`, desired `300x400`); acceptance: `OpensBelow == false` and `Bounds == (100,0,300,400)`, killing the `<=` to `<` mutant at `:56`
- [ ] [P11-T3] Add research case **P3** `Calculate_NegativeWorkingAreaAndDesiredDimensions_ClampToZero` to `QuickFiler.Test/Viewers/BreadcrumbPopupPlacementBoundaryTests.cs` (working area `(10,20,-5,-7)`, desired `(-3,-9)`); acceptance: the result is a zero-size rectangle at the working-area origin with `OpensBelow == true`, killing all four `Math.Max(0, …)` mutants
- [ ] [P11-T4] Add research case **P4** `Calculate_AnchorBelowWorkingArea_ClampsAboveSpaceToWorkingHeight` to `QuickFiler.Test/Viewers/BreadcrumbPopupPlacementBoundaryTests.cs` (anchor `(100,900,200,25)`, working area `(0,0,800,300)`, desired `300x400`); acceptance: `OpensBelow == false`, `Bounds.Height == 300`, and `Bounds.Top >= workingArea.Top`, killing the `Math.Min` deletion mutant at `:47`
- [ ] [P11-T5] Add research case **P5** `Calculate_ZeroDesiredSize_ReturnsZeroSizeBelowAnchor` to `QuickFiler.Test/Viewers/BreadcrumbPopupPlacementBoundaryTests.cs` (anchor `(100,100,200,25)`, working area `(0,0,800,600)`, desired `(0,0)`); acceptance: `OpensBelow == true` and `Bounds == (100,125,0,0)`
- [ ] [P11-T6] Verify `QuickFiler.Test/Viewers/BreadcrumbPopupPlacementBoundaryTests.cs` is <= 500 lines, uses FluentAssertions exact-`Rectangle` equality, and references no F12-owned type; acceptance: `<FEATURE>/evidence/qa-gates/phase11-test-file-audit.<ts>.md` records the line count and a zero-match grep for `BreadcrumbBridgeCoordinator` (AC-15, AC-19)
- [ ] [P11-T7] Run `$vstest = vswhere -latest -products * -find Common7\IDE\Extensions\TestPlatform\vstest.console.exe; & $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation /TestCaseFilter:"FullyQualifiedName~BreadcrumbPopupPlacement"`; acceptance: `<FEATURE>/evidence/regression-testing/phase11-scoped-run.<ts>.md` records `Command:`, `EXIT_CODE: 0`, and zero failures across the new and pre-existing placement tests
- [ ] [P11-T8] Recompute per-file coverage for `QuickFiler/Viewers/BreadcrumbPopupPlacement.cs` under the Measurement Rules; acceptance: `<FEATURE>/evidence/qa-gates/popupplacement-coverage.<ts>.md` records **100% line and 100% branch** retained (48/48 lines, 12/12 conditions) (AC-4)

### Phase 12 — Interface-Only Classification and In-Scope Structural Corrections

- [ ] [P12-T1] Record `interface-only / not-measured` ledger rows in `<EPIC>/coverage-ledger.md` for `QuickFiler/Viewers/IBreadcrumbDropDownHost.cs`, `IBreadcrumbWebHost.cs`, `IWebViewCoreInitializer.cs`, `IWebViewMessenger.cs`, `IBreadcrumbControlSurface.cs`, and `IWebViewMessageChannel.cs`, each reported **N/A** rather than 0%, with the `IBreadcrumbDropDownHost.cs` rationale reading exactly "interface + enum declaration, no executable IL"; acceptance: six rows exist and none is reported as 0% or as a failure (AC-11, US-11)
- [ ] [P12-T2] Verify that none of the six interface-only files carries an `[ExcludeFromCodeCoverage]` attribute; acceptance: `<FEATURE>/evidence/qa-gates/ac11-interface-only-attributes.<ts>.md` records a zero-match grep across all six paths (AC-11)
- [ ] [P12-T3] Verify that no test added by this child asserts only the shape of an interface-only file for the purpose of manufacturing coverage; acceptance: `<FEATURE>/evidence/qa-gates/ac11-no-shape-assertion-tests.<ts>.md` enumerates every test added by this child that touches one of the six interfaces and states the behavioral or ledger-machine-check purpose of each (AC-11)
- [ ] [P12-T4] Rewrite `QuickFiler.Test/Viewers/BreadcrumbPopupPlacementTests.cs:138-155` to anchor its assembly-handle reflection on an F13-owned type instead of the F12-owned `BreadcrumbBridgeCoordinator`, leaving every existing assertion's meaning intact; acceptance: a repository grep of that file for `BreadcrumbBridgeCoordinator` and every other F12-owned type name returns zero matches, the file remains <= 500 lines, and all its tests pass unmodified in intent (AC-19, D13)
- [ ] [P12-T5] Verify that the three D13 in-scope non-coverage items landed: the `[ExcludeFromCodeCoverage]` at `BreadcrumbPopupUiOperations.cs:412` is removed, `WebView2CoreInitializerTests.cs` lives under `QuickFiler.Test/Viewers/`, and `BreadcrumbPopupPlacementTests.cs` is re-anchored; acceptance: `<FEATURE>/evidence/qa-gates/d13-structural-corrections.<ts>.md` records one PASS line per item with the verifying grep or path check (AC-8, AC-18, AC-19)
- [ ] [P12-T6] Author the irreducible-outcome record naming every unreachable outcome in `spec.md` D9 — `BreadcrumbCollapsedSurfaceController.cs:245-246`, `BreadcrumbUiDispatcher.cs:276`, `BreadcrumbWebViewSurfaceFactory.cs:221-222`, `BreadcrumbDropDownOpenLifetime.cs:359` and `:260`, `BreadcrumbPopupUiOperations.cs:324-325`, `BreadcrumbDropDownHost.cs:420`, `BreadcrumbDropDownOpenCoordinator.cs:241-242` — each with its proof and resulting ceiling; acceptance: `<FEATURE>/evidence/qa-gates/irreducible-outcomes.<ts>.md` contains all seven entries and is committed (AC-24, US-7)
- [ ] [P12-T7] Verify that no acceptance criterion and no task in this plan targets 100% on a file carrying a stated ceiling for that metric; acceptance: `<FEATURE>/evidence/qa-gates/ac24-no-100-percent-targets.<ts>.md` cross-references each ceilinged file against its phase verification task and records `NO 100% TARGET` for each (AC-24)
- [ ] [P12-T8] Verify by diff review that the branch contains no change altering the behavior described by promoted issues **#457, #458, #462, #475, #476, #477**; acceptance: `<FEATURE>/evidence/qa-gates/ac25-latent-defects-deferred.<ts>.md` records one line per issue naming the file and line it concerns and confirming the diff leaves that behavior unchanged (AC-25, US-8)
- [ ] [P12-T9] Verify AC-13 by producing a before/after `public` and `internal` signature listing for all 15 in-scope files, confirming `QuickFiler/Viewers/ItemViewer.Designer.cs` is byte-identical by SHA-256, and running `QuickFiler.Test/Viewers/ItemViewerBreadcrumbDropDownContractTests.cs` unmodified; acceptance: `<FEATURE>/evidence/qa-gates/ac13-frozen-contracts.<ts>.md` records an empty signature diff, matching SHA-256 values, and `EXIT_CODE: 0` with zero failures for that test class (AC-13)
- [ ] [P12-T10] Scan every test file added or modified by this child for `Thread.Sleep`, `Task.Delay`, wall-clock waits, temporary files, external services or processes, shown forms, popups, `ToolStripDropDown.Show`, STA attributes, `*.StaTests.cs` filenames, injected clocks, and `TimeProvider`, and confirm every ambient `SynchronizationContext` assignment is restored in a `finally`; acceptance: `<FEATURE>/evidence/qa-gates/ac14-determinism.<ts>.md` records a zero-match result for every prohibited pattern and enumerates each ambient-context test with its `finally` restore (AC-14, US-9)
- [ ] [P12-T11] Produce a line-count listing for every production and test file created or modified by this child; acceptance: `<FEATURE>/evidence/qa-gates/ac15-line-counts.<ts>.md` shows every listed file at <= 500 lines and `BreadcrumbPopupUiOperations.cs` at <= 420 (AC-15, AC-5)
- [ ] [P12-T12] Verify AC-16 csproj mechanics: five `<Compile Include="Viewers\…" />` entries added inside the F13 block of `QuickFiler/QuickFiler.csproj`, one entry per created test file plus the relocated `Viewers\WebView2CoreInitializerTests.cs` inside the breadcrumb block of `QuickFiler.Test/QuickFiler.Test.csproj`, the `Controllers\WebView2CoreInitializerTests.cs` entry removed, both files CRLF-terminated on every line, no property change, no reference change, and no reordering of unrelated entries; acceptance: `<FEATURE>/evidence/qa-gates/ac16-csproj-mechanics.<ts>.md` records the entry deltas, the CRLF line counts, and a diff hunk listing confined to the named blocks (AC-16)
- [ ] [P12-T13] Verify AC-17 by confirming that each of the five created production files — `BreadcrumbPopupProductionSurface.cs`, `IWebViewMessageChannel.cs`, `CoreWebView2MessageChannel.cs`, `IBreadcrumbControlSurface.cs`, `WebView2ControlSurface.cs` — has a `<EPIC>/coverage-ledger.md` row with its bucket and rationale, added in the same change as its `<Compile Include>` entry; acceptance: `<FEATURE>/evidence/qa-gates/ac17-ledger-rows.<ts>.md` records five rows and confirms none is classified `testable` without a measured >= 90% line figure (AC-17)
- [ ] [P12-T14] Run `git diff --name-only <merge-base>...HEAD` using the merge base recorded in `[P0-T7]` and confirm every path is within the 15 in-scope files, the 5 created production files, `QuickFiler/QuickFiler.csproj`, `QuickFiler.Test/QuickFiler.Test.csproj`, `QuickFiler.Test/Viewers/`, `<EPIC>/coverage-ledger.md`, or `<FEATURE>/`; acceptance: `<FEATURE>/evidence/qa-gates/ac20-scope-containment.<ts>.md` records the full path list with zero F12-owned and zero F14-owned paths (AC-20)
- [ ] [P12-T15] Verify AC-21 by confirming from the diff that the production change consists only of the attribute removals in AC-6/AC-7/AC-8, the member relocations in `spec.md` §6.1-§6.3, and additive non-exempt seam members, and that no pre-existing assertion in `QuickFiler.Test` was weakened, disabled, or deleted; acceptance: `<FEATURE>/evidence/qa-gates/ac21-no-behavior-change.<ts>.md` records the production-diff classification and a zero-count of removed or weakened pre-existing assertions (AC-21, US-8)

### Phase 13 — Final QC Loop

- [ ] [P13-T1] Run `dotnet tool run csharpier format .` from the repository root; acceptance: `<FEATURE>/evidence/qa-gates/final-csharpier-format.<ts>.md` records `Command:`, `EXIT_CODE: 0`, and the list of files reformatted (empty list expected on a clean pass)
- [ ] [P13-T2] Run `dotnet tool run csharpier check .` from the repository root; acceptance: `<FEATURE>/evidence/qa-gates/final-csharpier-check.<ts>.md` records `Command:`, `EXIT_CODE: 0`, and `Output Summary:` stating zero unformatted files
- [ ] [P13-T3] Run `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`; acceptance: `<FEATURE>/evidence/qa-gates/final-msbuild-analyzers.<ts>.md` records `Command:`, `EXIT_CODE: 0`, and zero errors with the warning count relative to the `[P0-T9]` baseline
- [ ] [P13-T4] Run `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true`; acceptance: `<FEATURE>/evidence/qa-gates/final-msbuild-nullable.<ts>.md` records `Command:`, `EXIT_CODE: 0` and zero nullable diagnostics introduced relative to the `[P0-T10]` baseline
- [ ] [P13-T5] Run `pwsh -NoProfile -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot '.' -Configuration Debug -CoverageOutput '<FEATURE>/evidence/qa-gates/coverage-final.<ts>.cobertura.xml'` from the executing worktree root; acceptance: `<FEATURE>/evidence/qa-gates/final-test-coverage.<ts>.md` records `Command:`, `EXIT_CODE: 0`, total/passed/failed counts with zero failures, and the repository-wide **numeric line-rate and branch-rate** from the Cobertura root element (AC-22)
- [ ] [P13-T6] Verify that no step in `[P13-T1]` through `[P13-T5]` failed or modified a tracked file; if any did, restart the loop from `[P13-T1]` and re-record every artifact for the new pass; acceptance: `<FEATURE>/evidence/qa-gates/final-toolchain-loop.<ts>.md` records the pass number, `git status --porcelain` output taken after `[P13-T5]`, and `LOOP: CLEAN PASS` (AC-22, US-10)
- [ ] [P13-T7] Produce the per-file coverage report for all 11 existing production files plus all 5 created production files from `<FEATURE>/evidence/qa-gates/coverage-final.<ts>.cobertura.xml`, keying on `filename=` and summing deduplicated class-level `<line>` children with `max(hits)`; acceptance: `<FEATURE>/evidence/qa-gates/ac03-per-file-report.<ts>.md` shows line and branch for each measured file, `N/A` for each `ratified-exempt` and `interface-only` file, and an explicit statement that no figure was taken from a `<class>` `line-rate` or `branch-rate` attribute (AC-3, US-1)
- [ ] [P13-T8] Verify retain-or-improve for the eight already-passing files against the `spec.md` §12 AC-4 baselines (99.42/91.49, 99.13/91.86, 98.25/92.05, 98.97/85.71, 100/97.22, 99.29/97.62, 100/100, 90.70/88.33); acceptance: `<FEATURE>/evidence/qa-gates/ac04-retain-or-improve.<ts>.md` shows every post-change figure greater than or equal to its baseline on both metrics with no regression (AC-4, US-3)
- [ ] [P13-T9] Compare the repository-wide line and branch rates from `[P0-T11]` and `[P13-T5]`, both captured in the same session on this branch over the full `*.Test.dll` set; acceptance: `<FEATURE>/evidence/qa-gates/ac23-repo-wide-delta.<ts>.md` records both numeric pairs, the delta, an explicit statement that no figure inherited from another feature folder was used, and `RESULT: RETAINED OR IMPROVED` (AC-23, US-10)
- [ ] [P13-T10] Check off every acceptance criterion in `<FEATURE>/spec.md` §12 (AC-1..AC-25), `<FEATURE>/user-story.md` (US-1..US-11), and `<FEATURE>/spec.md` §13 Definition of Done, citing the evidence artifact path for each; acceptance: `<FEATURE>/evidence/qa-gates/ac-status-summary.<ts>.md` maps all 25 + 11 + 5 items to an evidence path with no item marked PASS without a cited artifact
- [ ] [P13-T11] Commit every evidence artifact, the coverage ledger update, and all production and test changes; acceptance: `git status --porcelain` returns empty and `<FEATURE>/evidence/other/final-commit-state.<ts>.md` records the final commit SHA and the clean-tree confirmation

---

## Test Plan

- **Unit (new, MSTest + Moq + FluentAssertions, all under `QuickFiler.Test/Viewers/`):** 7 cases for
  `BreadcrumbPopupUiOperations` (T1-T7 plus the AC-9 reflection assertion), 36 cases for
  `WebView2Messenger` (X1-X10, P1-P8, N1-N11, D1-D7), 32 cases for `WebView2BreadcrumbHost`
  (C1-C9, I1-I7, M1-M7, L1-L9), 6 contract assertions for `WebView2CoreInitializer` (K1-K6),
  3 for `BreadcrumbCollapsedSurfaceController` (T1-T3), 8 for `BreadcrumbDropDownHost` (T1-T8),
  6 for `BreadcrumbDropDownOpenLifetime` (T1-T6), 7 plus one fake extension for
  `BreadcrumbDropDownOpenCoordinator` (T0-T7), 5 for `BreadcrumbPopupPlacement` (P1-P5).
- **Retain-and-verify only (no new test):** `BreadcrumbUiDispatcher` (Phase 9) and
  `BreadcrumbWebViewSurfaceFactory` (Phase 10).
- **Regression:** `[P2-T15]` (`InternalConstructor_BothArgumentsNull_ReportsCoreWebViewFirst`) and
  `[P2-T23]` (`PostJson_NullAfterDispose_StillThrowsArgumentNullException`) are exception-fidelity
  regression guards required by AC-12. Neither is `[expect-fail]`: both assert current behavior that
  the refactor must preserve.
- **Integration:** none added. The complete pre-existing `QuickFiler.Test` suite is the integration
  gate and runs in `[P13-T5]`.
- **Coverage evidence:**
  - Baseline: `<FEATURE>/evidence/baseline/coverage-baseline.<ts>.cobertura.xml` and
    `<FEATURE>/evidence/baseline/per-file-baseline.<ts>.md` (`[P0-T11]`, `[P0-T12]`).
  - Post-change: `<FEATURE>/evidence/qa-gates/coverage-final.<ts>.cobertura.xml` and
    `<FEATURE>/evidence/qa-gates/ac03-per-file-report.<ts>.md` (`[P13-T5]`, `[P13-T7]`).
  - Comparison: `<FEATURE>/evidence/qa-gates/ac04-retain-or-improve.<ts>.md` and
    `<FEATURE>/evidence/qa-gates/ac23-repo-wide-delta.<ts>.md` (`[P13-T8]`, `[P13-T9]`).
  - New-file threshold: `[P12-T13]` confirms no created production file is classified `testable`
    without a measured >= 90% line figure.

## Traceability

| AC | Satisfied by |
|---|---|
| AC-1 | `[P0-T5]` |
| AC-2 | `[P0-T6]`, `[P4-T1]`, `[P4-T4]` |
| AC-3 | `[P10-T3]`, `[P13-T7]` |
| AC-4 | `[P5-T6]`, `[P6-T11]`, `[P7-T9]`, `[P8-T11]`, `[P9-T1]`, `[P10-T1]`, `[P11-T8]`, `[P13-T8]` |
| AC-5 | `[P1-T7]`, `[P1-T18]`, `[P12-T11]` |
| AC-6 | `[P3-T3]`, `[P3-T20]`, `[P3-T47]` |
| AC-7 | `[P2-T3]`, `[P2-T21]`, `[P2-T50]` |
| AC-8 | `[P1-T5]`, `[P1-T15]`, `[P1-T16]`, `[P1-T18]`, `[P12-T5]` |
| AC-9 | `[P1-T1]`, `[P1-T11]`, `[P1-T18]` |
| AC-10 | `[P2-T1]`, `[P2-T2]`, `[P2-T21]`, `[P3-T1]`, `[P3-T2]`, `[P3-T20]` |
| AC-11 | `[P12-T1]`, `[P12-T2]`, `[P12-T3]` |
| AC-12 | `[P2-T15]`, `[P2-T23]` |
| AC-13 | `[P2-T10]`, `[P3-T8]`, `[P3-T11]`, `[P12-T9]` |
| AC-14 | `[P2-T48]`, `[P3-T45]`, `[P5-T4]`, `[P12-T10]` |
| AC-15 | `[P1-T7]`, `[P2-T48]`, `[P3-T45]`, `[P5-T4]`, `[P6-T9]`, `[P7-T7]`, `[P8-T9]`, `[P11-T6]`, `[P12-T11]` |
| AC-16 | `[P1-T1]`, `[P1-T6]`, `[P2-T1]`, `[P2-T2]`, `[P2-T8]`, `[P3-T1]`, `[P3-T2]`, `[P3-T9]`, `[P4-T3]`, `[P12-T12]` |
| AC-17 | `[P1-T8]`, `[P2-T9]`, `[P3-T10]`, `[P4-T12]`, `[P12-T13]` |
| AC-18 | `[P4-T2]`, `[P4-T3]`, `[P4-T11]`, `[P12-T5]` |
| AC-19 | `[P11-T6]`, `[P12-T4]`, `[P12-T5]` |
| AC-20 | `[P7-T7]`, `[P8-T9]`, `[P12-T14]` |
| AC-21 | `[P3-T28]`, `[P3-T41]`, `[P12-T15]` |
| AC-22 | `[P13-T1]`..`[P13-T6]` |
| AC-23 | `[P0-T11]`, `[P13-T5]`, `[P13-T9]` |
| AC-24 | `[P1-T19]`, `[P5-T6]`, `[P6-T11]`, `[P7-T9]`, `[P8-T11]`, `[P9-T2]`, `[P10-T2]`, `[P12-T6]`, `[P12-T7]` |
| AC-25 | `[P3-T28]`, `[P3-T41]`, `[P8-T3]`, `[P12-T8]` |
| US-1 | `[P2-T50]`, `[P3-T47]`, `[P13-T7]` |
| US-2 | `[P2-T50]`, `[P3-T47]` |
| US-3 | `[P13-T8]` |
| US-4 | `[P1-T8]`, `[P2-T9]`, `[P3-T10]`, `[P4-T12]` |
| US-5 | `[P1-T5]`, `[P1-T15]`, `[P1-T16]` |
| US-6 | `[P1-T18]` |
| US-7 | `[P12-T6]` |
| US-8 | `[P12-T8]`, `[P12-T15]` |
| US-9 | `[P12-T10]` |
| US-10 | `[P13-T6]`, `[P13-T9]` |
| US-11 | `[P12-T1]`, `[P12-T13]` |

## Open Questions / Notes

- **The F1 fourth-ground ruling is the only genuine open question**, and it is resolved at execution
  time by `[P0-T6]`. Both branches are planned: `[P4-T4]` applies the ruling to the source and
  `[P4-T9]` asserts it, in that order, so neither branch leaves a task asserting a source state that
  has not yet been applied. The disposition of `WebView2BreadcrumbHost.cs` and `WebView2Messenger.cs`
  is unaffected by either ruling — both are de-exempted in all cases.
- Fan-in conflicts on `QuickFiler/QuickFiler.csproj` and `QuickFiler.Test/QuickFiler.Test.csproj` are
  expected (F12's entries at `QuickFiler.csproj:393-395` and `:400` interleave with F13's) and are
  resolved **additively** — keep both sides.
- `BreadcrumbPopupLifecycleOperations` and `BreadcrumbNavigationSubscription` live inside F12's
  `BreadcrumbItemViewerLifecycleCoordinator.cs` at `:355` and `:337`. The Phase 1 split moves two of
  the three F13 call sites (`:401`, `:466`) into the exempt file, leaving only `:414` measured. If
  F12 splits its file for the 500-line rule, a pure file move is source-compatible with no edit here.
- Research artifact 00 §4 could not run `gh issue list` (no shell tool in that session). The
  orchestrator should re-run `gh issue list --state open --limit 200 --json number,title,labels` and
  grep for `breadcrumb|dropdown|drop-down|WebView2|popup|coverage` before execution begins, to
  confirm no promoted-but-inactive issue touches the F13 file set.
