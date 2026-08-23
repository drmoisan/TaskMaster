# wpf-dispatcher-yield-test-order-dependent (Plan)

- **Issue:** #508
- **Parent (optional):** none
- **Owner:** drmoisan
- **Last Updated:** 2026-08-08T15-23
- **Status:** Draft
- **Version:** 1.2 (revision pass 2 — git-gate scoping deltas 1-3 applied to P1-T15, P1-T16, P2-T13; delta 4 rebuild + bounded timeout appended to P0-T12; csharpier version and git-gate scoping notes added. No task IDs changed.)
- **Work Mode:** minor-audit (small path, 3-phase minimal-audit plan)
- **Branch:** `bug/wpf-dispatcher-yield-test-order-dependent-508`
- **Base Branch:** `main` / merge-base `003c5715055d7d1933db68a742531332756e30b2`

## Path Aliases

`<FEATURE>` = `docs/features/active/2026-08-08-wpf-dispatcher-yield-test-order-dependent-508`

All evidence artifacts resolve under `<FEATURE>/evidence/<kind>/` with `<kind>` in
{`baseline`, `regression-testing`, `qa-gates`, `issue-updates`, `other`}. Example literal path:
`docs/features/active/2026-08-08-wpf-dispatcher-yield-test-order-dependent-508/evidence/baseline/phase0-instructions-read.md`.
`artifacts/baseline*`, `artifacts/qa*`, `artifacts/coverage/`, and `artifacts/evidence/` are forbidden
for evidence output and must not be used by any task in this plan.

`<ts>` = ISO-8601 timestamp in `yyyy-MM-ddTHH-mm` form, captured at the moment the artifact is written.

## Requirements Source

`<FEATURE>/issue.md` is the sole requirements source for this `minor-audit` cycle. Its
`## Acceptance Criteria` section (AC1..AC9) is the only AC source. `spec.md`, `user-story.md`, and
`research.md` are absent by design; their absence is not a blocker. If any of those files is found in
the active folder, execution fails closed.

## Fail-closed Evidence Rules

- Include explicit baseline artifact tasks, final-QA artifact tasks, and coverage-comparison tasks.
  If any required baseline, QA, or coverage artifact is missing, the verdict is BLOCKED or
  INCOMPLETE, never PASS.
- Record the expected artifact path in every evidence-producing task. Do not mark evidence-backed
  work complete without the artifact on disk.
- Every command-step artifact must contain `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`.
- Phase 2 command tasks are unconditional. `EXIT_CODE: SKIPPED` is not a passing outcome.

## Design Decision — Seam Shape (binding for Phase 1)

Chosen: **injectable delegate seam** on `UtilitiesCS/OutlookObjects/Folder/WpfDispatcherYield.cs`
(`.claude/rules/csharp.md` "DI Seams", preference 2).

Target shape:

- Two readonly fields of type `Func<Dispatcher?>`: a current-thread dispatcher provider and a
  fallback dispatcher provider.
- A `public WpfDispatcherYield()` constructor that chains to the seam constructor. This explicit
  parameterless constructor is **mandatory**: adding any constructor removes the implicit one and
  would break `TaskMaster/AppGlobals/AppOlObjects.FolderTreeService.cs:365` and
  `UtilitiesCS.Test/OutlookObjects/Folder/OutlookFolderTreeServiceConcurrencyTests.cs:55`.
- An `internal WpfDispatcherYield(Func<Dispatcher?>? currentThreadDispatcherProvider, Func<Dispatcher?>? fallbackDispatcherProvider)`
  seam constructor. `internal` is sufficient because
  `UtilitiesCS/Properties/AssemblyInfo.cs:19` already declares
  `[assembly: InternalsVisibleTo("UtilitiesCS.Test")]`. This keeps the **public** API surface
  byte-identical, which is the strongest possible answer to AC4.
- Null arguments fall back to the exact current production expressions:
  `() => Dispatcher.FromThread(Thread.CurrentThread)` and `() => UtilitiesCS.UiThread.Dispatcher`.
- Resolution order (`thread-affinitized` then `process-global fallback`) stays inside `YieldAsync`,
  so the test still verifies the ordering rather than replacing it.

Alternatives considered and rejected:

1. **Interface seam** (`IDispatcherProvider` + a production implementation). Rejected: it adds a new
   public type plus an implementation class to `UtilitiesCS` for a single call path with one
   production call site, which conflicts with AC4 ("minimal") and with `.claude/rules/csharp.md`
   "Introduce the smallest seam that enables reliable unit testing". The delegate seam is explicitly
   sanctioned "for a single call path when a full interface is excessive".
2. **Owned dedicated thread only** (run the assertion on a thread the test creates). Rejected: it
   arranges only operand 1; `UiThread.Dispatcher` remains unarranged process-global state, so the
   test stays order-dependent. This matches the analysis already recorded in `<FEATURE>/issue.md`.
3. **Reflection mutation of `UiThread._dispatcher`** (precedent:
   `UtilitiesCS.Test/Threading/IdleAsyncQueue_Tests.cs:51-53`). Rejected as the fix: it mutates
   process-global state under `Parallelize(Workers = 0, Scope = ClassLevel)` and would require
   serialization, reintroducing the coupling this issue exists to remove.

## Design Decision — `[ExcludeFromCodeCoverage]`

Recommendation: **remove** `[ExcludeFromCodeCoverage]` from
`UtilitiesCS/OutlookObjects/Folder/WpfDispatcherYield.cs:13`, and remove the then-unused
`using System.Diagnostics.CodeAnalysis;` at line 3.

Rationale: `.claude/rules/general-unit-test.md` "Coverage Exclusion Policy" states no production file
may be excluded from coverage measurement, and the stated purpose of this work is testability. Once
the seam exists, the class is genuinely unit-testable.

Honest coverage expectation (must be measured, not assumed): the `await dispatcher.InvokeAsync(...)`
line **is** reachable, because the test can supply a `Dispatcher` obtained from a pumping STA thread
the test itself owns and shuts down (precedent:
`UtilitiesCS.Test/OutlookObjects/Folder/FolderTreeSnapshotBuilderYieldTests.cs:118-147`).

Two measurement facts are binding on Phase 2:

1. `YieldAsync` is `async`, so its body compiles into the nested state machine
   `WpfDispatcherYield/<YieldAsync>d__N` and appears as a **separate `<class>` element** in the
   Cobertura report. The changed-class coverage figure MUST be computed by aggregating the
   `UtilitiesCS.OutlookObjects.Folder.WpfDispatcherYield` element **and** every compiler-generated
   nested type it owns (`<YieldAsync>d__*` state machines and `<>c*` lambda display classes).
   Reading the named class element alone yields only the constructors and lambdas and understates
   the figure to roughly 83%, which would fail the gate for a measurement reason.
2. Exactly **one** line is expected to remain uncovered: the body of the default **fallback**
   provider lambda `() => UtilitiesCS.UiThread.Dispatcher`. It is evaluated only when the
   parameterless constructor is used and the thread-affinitized lookup returns null; the sole
   existing parameterless-ctor caller that reaches resolution
   (`OutlookFolderTreeServiceConcurrencyTests.GetSnapshotAsync_WorkerOriginatedColdBuild_UsesCapturedStaDispatcher`)
   runs on a thread that *has* a dispatcher, and arranging the null case through the parameterless
   ctor would reintroduce exactly the process-global ambient dependency this issue exists to remove.
   The default **thread-affinitized** lambda and the parameterless constructor are covered by that
   same existing test.

Branch coverage will be **less than** 100%, because the throwing path of the trailing post-yield
`cancellationToken.ThrowIfCancellationRequested()` is not deterministically arrangeable: reaching it
requires cancellation to land strictly between the `DispatcherOperation` completing and the guard
executing. Cancelling any earlier aborts the operation and throws out of the `await` instead, and a
timing hack to win that race is prohibited. Phase 2 records the measured figures.

## MSTest Discovery Caveat (applies to every test command task)

When globbing for `*.Test.dll`, **exclude any path that resolves outside the workspace root**. The
repository carries roughly 20 stale `.claude/worktrees/agent-*` worktrees whose old builds otherwise
get discovered and produce bogus `AssemblyInitialize` signature failures. The workspace root for this
execution is itself an agent worktree, so the exclusion must be expressed as a workspace-root prefix
test, not as a `\.claude\` substring test.

`scripts/vscode/Invoke-MSTestWithCoverage.ps1` is the repo-canonical coverage runner and **is used**
by this plan for the two coverage captures (baseline and final). Its discovery filter
(`scripts/vscode/Invoke-MSTestWithCoverage.ps1:296-302`) excludes `\obj\` and `\ref\` only, and it
resolves its search root from `$PSScriptRoot\..\..` (line 271), which in this execution is the agent
worktree `C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-ad7090ae544fd0fb0`. Because the
workspace root is itself located under `\.claude\worktrees\`, a naive "path contains `\.claude\`"
assertion is unsatisfiable and MUST NOT be used. The correct assertion is: every discovered assembly
path begins with the workspace-root prefix `C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-ad7090ae544fd0fb0\`,
and no discovered path contains a `\.claude\worktrees\` segment **after** that prefix (which would
indicate a stale sibling worktree build). The three AC7 repeat runs use `vstest.console.exe`
directly against an explicitly named assembly path, so discovery globbing is bypassed entirely.

## Scope Boundary

- In scope: `UtilitiesCS/OutlookObjects/Folder/WpfDispatcherYield.cs` and
  `UtilitiesCS.Test/OutlookObjects/Folder/WpfDispatcherYieldTests.cs`.
- Out of scope: `TaskMaster/Ribbon/**` (concurrent work on #503 and #507), `UtilitiesCS/Threading/UiThread.cs`,
  any other test file, and any `.csproj` change.

### Phase 0 — Baseline capture

- [x] [P0-T1] Read policy files in the `.claude/skills/policy-compliance-order/SKILL.md` order — `CLAUDE.md`, `.claude/rules/general-code-change.md`, `.claude/rules/general-unit-test.md`, `.claude/rules/csharp.md` — and write `<FEATURE>/evidence/baseline/phase0-instructions-read.md` containing `Timestamp:`, `Policy Order:`, and the explicit list of files read.
- [x] [P0-T2] Verify `<FEATURE>/issue.md` contains an explicit `## Acceptance Criteria` section with AC1..AC9 and that `<FEATURE>/spec.md`, `<FEATURE>/user-story.md`, and `<FEATURE>/research.md` are absent; record the check and the nine AC identifiers in `<FEATURE>/evidence/baseline/requirements-source.<ts>.md`. Fail closed if the AC section is missing or if any of the three files exists.
- [x] [P0-T3] Record repository tree state — `git rev-parse HEAD`, merge-base `003c5715055d7d1933db68a742531332756e30b2`, and `git status --porcelain` — in `<FEATURE>/evidence/baseline/repo-state.<ts>.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`. Gate on zero `.cs`/`.csproj` diff versus the merge-base and on `git status --porcelain -- '*.cs' '*.csproj' '*.sln'` returning empty. Do **not** gate on globally-clean porcelain: `.claude/agent-memory/**` is modified at branch head and the entire `<FEATURE>` folder plus every evidence artifact this plan writes are untracked by construction. Do not pin the recorded HEAD sha as a later expectation.
- [x] [P0-T4] Capture the verbatim pre-change contents of `UtilitiesCS/OutlookObjects/Folder/WpfDispatcherYield.cs` (44 lines), `UtilitiesCS.Test/OutlookObjects/Folder/WpfDispatcherYieldTests.cs` (39 lines), and the `Parallelize` attribute at `UtilitiesCS.Test/Properties/AssemblyInfo.cs:18-21` into `<FEATURE>/evidence/baseline/source-under-test.<ts>.md`.
- [x] [P0-T5] Confirm the four seam preconditions and record them in `<FEATURE>/evidence/baseline/seam-preconditions.<ts>.md`: `[assembly: InternalsVisibleTo("UtilitiesCS.Test")]` present at `UtilitiesCS/Properties/AssemblyInfo.cs:19`; `<LangVersion>Latest</LangVersion>` present at `UtilitiesCS.Test/UtilitiesCS.Test.csproj:18`; `#nullable enable` already in use by peer files in `UtilitiesCS.Test/OutlookObjects/Folder/`; and the two `new WpfDispatcherYield()` call sites at `TaskMaster/AppGlobals/AppOlObjects.FolderTreeService.cs:365` and `UtilitiesCS.Test/OutlookObjects/Folder/OutlookFolderTreeServiceConcurrencyTests.cs:55`.
- [x] [P0-T6] Run baseline formatter check `csharpier check .` from the workspace root, invoking the global tool at `C:\Users\DanMoisan\.dotnet\tools\csharpier.exe` (CSharpier 1.3.0). Do **not** use `dotnet tool run csharpier`: this checkout has no `.config/dotnet-tools.json` manifest (the manifest at repo root is `dotnet-tools.json`, which `dotnet tool run` does not read) and no repo-local `.dotnet-sdk`, so every `dotnet` SDK command fails with the `global.json` missing-SDK error. Write `<FEATURE>/evidence/baseline/csharpier.<ts>.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`.
- [x] [P0-T7] Restore NuGet packages for the solution: run `pwsh -File scripts/vscode/Invoke-Restore.ps1` from the workspace root (it resolves MSBuild via vswhere and runs `/t:Restore /p:RestorePackagesConfig=true`, so no .NET SDK is required). This checkout has no `packages/` directory and no build output; without restore the analyzer and nullable baselines are vacuous. Write `<FEATURE>/evidence/baseline/nuget-restore.<ts>.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` including the resulting existence of `packages/`. If the MSBuild restore path fails on the legacy packages.config projects, fall back to `nuget.exe restore TaskMaster.sln` using `C:\Users\DanMoisan\AppData\Local\Microsoft\WinGet\Packages\Microsoft.NuGet_Microsoft.Winget.Source_8wekyb3d8bbwe\nuget.exe` and record the fallback in the same artifact.
- [x] [P0-T8] Run baseline analyzer build `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` and write `<FEATURE>/evidence/baseline/msbuild-analyzers.<ts>.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` (warning and error counts).
- [x] [P0-T9] Run baseline nullable build `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true` and write `<FEATURE>/evidence/baseline/msbuild-nullable.<ts>.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`.
- [x] [P0-T10] Run the full-suite baseline coverage capture `pwsh -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug -CoverageOutput "<FEATURE>/evidence/baseline/coverage-baseline.cobertura.xml"`, assert that every discovered assembly path begins with the workspace-root prefix `C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-ad7090ae544fd0fb0\` and that no discovered path contains a `\.claude\worktrees\` segment after that prefix, and write `<FEATURE>/evidence/baseline/tests-coverage.<ts>.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, and an `Output Summary:` carrying numeric total/passed/failed counts plus the root `<coverage>` element `line-rate` and `branch-rate` values.
- [x] [P0-T11] Extract the pre-change per-class coverage figure for `UtilitiesCS.OutlookObjects.Folder.WpfDispatcherYield` from `<FEATURE>/evidence/baseline/coverage-baseline.cobertura.xml` and record it in `<FEATURE>/evidence/baseline/wpfdispatcheryield-coverage.<ts>.md`; record whichever state is actually observed. Note that `coverage.config` supplies a custom `<Configuration><CodeCoverage>` block with no `<Attributes>` element, which replaces the dotnet-coverage default attribute-exclude set; `[ExcludeFromCodeCoverage]` is therefore likely **not** honored and the class is likely to be present with a real rate. If the class is present, record its aggregated pre-change line rate (aggregating compiler-generated nested types per P2-T12's method) as the baseline comparand. If it is genuinely absent, state the absence explicitly rather than reporting it as zero.
- [x] [P0-T12] [expect-fail] Produce a deterministic, hang-free demonstration that `YieldAsync_WithoutDispatcher_RemainsStrict` at `UtilitiesCS.Test/OutlookObjects/Folder/WpfDispatcherYieldTests.cs:28-37` passes only by accident of ambient state, and record the failing run in `<FEATURE>/evidence/regression-testing/fail-before.<ts>.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`. Mechanism (binding — the pre-change class has no seam, so nothing can be injected into it): temporarily edit the existing `YieldAsync_WithoutDispatcher_RemainsStrict` method **in place** so that it (a) constructs a `StaDispatcherHost`-style owned STA thread that calls `Dispatcher.Run()` and is shut down with `BeginInvokeShutdown` + `Join`, per `UtilitiesCS.Test/OutlookObjects/Folder/FolderTreeSnapshotBuilderYieldTests.cs:118-147`, and (b) marshals the existing `new WpfDispatcherYield().YieldAsync(CancellationToken.None)` call **onto that owned pumping thread** via `host.Dispatcher.InvokeAsync`, keeping the `.Should().ThrowAsync<InvalidOperationException>()` assertion unchanged. Because `Dispatcher.FromThread(Thread.CurrentThread)` is then non-null on the executing thread, `YieldAsync` completes instead of throwing and the assertion fails with a bounded FluentAssertions "did not throw" failure. Edit the existing file only — do **not** add a new probe `.cs` file, because `UtilitiesCS.Test.csproj` is a legacy non-SDK project with explicit `<Compile Include>` items (`UtilitiesCS.Test.csproj:334`) and adding a file would require a csproj edit that P0-T14 and P1-T15 forbid. Run the probe in isolation: `<vstest> UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /InIsolation /Tests:YieldAsync_WithoutDispatcher_RemainsStrict`. Rebuild the test assembly after the in-place edit and before running the probe (`msbuild UtilitiesCS.Test\UtilitiesCS.Test.csproj /t:Build /p:Configuration=Debug /p:Platform="Any CPU"`), otherwise the run executes the stale pre-edit assembly and reports a false pass. Add a bounded `[Timeout(30000)]` to the probe method for the duration of the probe so a composition mistake surfaces as a timeout rather than a suite hang; the attribute is part of the temporary probe edit and is removed by the P0-T14 revert.
- [x] [P0-T13] Record the hang hazard, the mitigation applied in P0-T12, and the operand scope of the reproduction in `<FEATURE>/evidence/regression-testing/fail-before-method.<ts>.md`. Hang hazard: a dispatcher created by touching `Dispatcher.CurrentDispatcher` on a non-pumping pooled worker never completes `InvokeAsync(..., DispatcherPriority.Background, ...)`, so the probe must never await a yield against a non-pumping dispatcher; P0-T12 avoids this by running `Dispatcher.Run()` on the owned STA thread. Operand scope: the probe reproduces **operand 1** (`Dispatcher.FromThread(Thread.CurrentThread)`). Operand 2 (`UiThread.Dispatcher`), which the `## Notes` section infers is the dominant real-world contributor, is deliberately **not** probed, because arranging it without a seam would require either `UiThread.Init()` (shows a form) or reflection mutation of the process-global `UiThread._dispatcher`, both rejected in the `## Design Decision — Seam Shape` section. Both operands share one root cause — an unarranged `??` over ambient state — and the Phase 1 seam arranges both, so an operand-1 reproduction is sufficient fail-before evidence for AC6. If and only if P0-T12 could not produce a genuinely failing run, additionally write a schema-valid `<FEATURE>/evidence/regression-testing/fail-before-exception.<ts>.md` containing `WhyFailingRunImpossible:` and an alternative proof section, per `.claude/skills/evidence-and-timestamp-conventions/SKILL.md`.
- [x] [P0-T14] Confirm the temporary probe edit made for P0-T12 has been fully reverted: `git diff -- UtilitiesCS.Test/OutlookObjects/Folder/WpfDispatcherYieldTests.cs` is empty, `git status --porcelain -- '*.cs' '*.csproj' '*.sln'` is empty, and `UtilitiesCS.Test/UtilitiesCS.Test.csproj` is unmodified. Do not gate on globally-clean porcelain (see P0-T3). Record the confirmation in `<FEATURE>/evidence/baseline/probe-teardown.<ts>.md`.
- [x] [P0-T15] Verify every Phase 0 artifact exists on disk under `<FEATURE>/evidence/baseline/` or `<FEATURE>/evidence/regression-testing/` and that every **command-step** artifact (P0-T3, P0-T6, P0-T7-restore, P0-T8-analyzers, P0-T9-nullable, P0-T10-coverage, P0-T12-fail-before) carries `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`, and that `phase0-instructions-read.md` carries `Timestamp:`, `Policy Order:`, and the explicit list of files read; leave any Phase 0 checkbox unchecked whose artifact is absent or incomplete, and record the audit in `<FEATURE>/evidence/baseline/phase0-completeness.<ts>.md`.

### Phase 1 — Implementation (constrained small path)

- [x] [P1-T1] Hand off implementation to the C# implementation engineer with this plan, `<FEATURE>/issue.md`, and the two in-scope files `UtilitiesCS/OutlookObjects/Folder/WpfDispatcherYield.cs` and `UtilitiesCS.Test/OutlookObjects/Folder/WpfDispatcherYieldTests.cs`; record the handoff in `<FEATURE>/evidence/other/implementation-handoff.<ts>.md`.
- [x] [P1-T2] In `UtilitiesCS/OutlookObjects/Folder/WpfDispatcherYield.cs`, add an `internal` seam constructor taking `Func<Dispatcher?>? currentThreadDispatcherProvider` and `Func<Dispatcher?>? fallbackDispatcherProvider`, storing them in two `readonly` fields. Acceptance: the seam constructor is `internal`, not `public`, and the class `public` surface is otherwise unchanged.
- [x] [P1-T3] In `UtilitiesCS/OutlookObjects/Folder/WpfDispatcherYield.cs`, add an explicit `public WpfDispatcherYield()` constructor chaining to the seam constructor with both arguments null. Acceptance: `new WpfDispatcherYield()` still compiles unchanged at `TaskMaster/AppGlobals/AppOlObjects.FolderTreeService.cs:365` and `UtilitiesCS.Test/OutlookObjects/Folder/OutlookFolderTreeServiceConcurrencyTests.cs:55`, with zero call-site edits.
- [x] [P1-T4] In `UtilitiesCS/OutlookObjects/Folder/WpfDispatcherYield.cs`, default a null `currentThreadDispatcherProvider` to `() => Dispatcher.FromThread(Thread.CurrentThread)` and a null `fallbackDispatcherProvider` to `() => UtilitiesCS.UiThread.Dispatcher`. Acceptance: the fallback reads the `UiThread.Dispatcher` property only (a plain field read at `UtilitiesCS/Threading/UiThread.cs:135-140`); it must not touch `UiThread.UiSyncContext` or `UiThread.AutoScaleFactor`, both of which call `Init()` and would show a form.
- [x] [P1-T5] In `UtilitiesCS/OutlookObjects/Folder/WpfDispatcherYield.cs`, keep the resolution order inside `YieldAsync` as `currentThreadDispatcherProvider() ?? fallbackDispatcherProvider()`, and keep the existing `InvalidOperationException` message text byte-identical. Acceptance: the thread-affinitized provider is evaluated first and the fallback is evaluated only when the first returns null.
- [x] [P1-T6] In `UtilitiesCS/OutlookObjects/Folder/WpfDispatcherYield.cs`, change the resolved local from `Dispatcher` to `Dispatcher?` so the nullable flow analysis is correct under `/p:Nullable=enable /p:TreatWarningsAsErrors=true`. Acceptance: no new CS86xx diagnostic is introduced.
- [x] [P1-T7] In `UtilitiesCS/OutlookObjects/Folder/WpfDispatcherYield.cs`, remove `[ExcludeFromCodeCoverage]` from line 13 and remove the resulting unused `using System.Diagnostics.CodeAnalysis;` at line 3. Acceptance: the file has no `ExcludeFromCodeCoverage` token and no unused-using diagnostic.
- [x] [P1-T8] In `UtilitiesCS.Test/OutlookObjects/Folder/WpfDispatcherYieldTests.cs`, add `#nullable enable` as the first line so `Func<Dispatcher?>` annotations do not raise CS8632 under the nullable gate. Acceptance: the file compiles with no CS8632.
- [x] [P1-T9] In `UtilitiesCS.Test/OutlookObjects/Folder/WpfDispatcherYieldTests.cs`, add a private `StaDispatcherHost`-style nested helper that owns a pumping STA thread and shuts it down in `Dispose`, modelled on `UtilitiesCS.Test/OutlookObjects/Folder/FolderTreeSnapshotBuilderYieldTests.cs:118-147`. Acceptance: no form is shown, `UiThread.Init()` is never called, no COM object is touched, and the thread is joined on disposal.
- [x] [P1-T10] In `UtilitiesCS.Test/OutlookObjects/Folder/WpfDispatcherYieldTests.cs`, add the branch-1 test: the thread-affinitized provider returns the owned host `Dispatcher`, the fallback provider is a counting delegate. Acceptance (Arrange-Act-Assert, FluentAssertions): `YieldAsync` completes, the thread provider invocation count is 1, and the fallback invocation count is 0.
- [x] [P1-T11] In `UtilitiesCS.Test/OutlookObjects/Folder/WpfDispatcherYieldTests.cs`, add the branch-2 test: the thread-affinitized provider returns null and the fallback provider returns the owned host `Dispatcher`. Acceptance: `YieldAsync` completes and both provider invocation counts are 1, pinning the fallback ordering.
- [x] [P1-T12] In `UtilitiesCS.Test/OutlookObjects/Folder/WpfDispatcherYieldTests.cs`, rewrite `YieldAsync_WithoutDispatcher_RemainsStrict` so both providers return null and the assertion remains `ThrowAsync<InvalidOperationException>()`. Acceptance: the arrangement is explicit, the assertion is not weakened, and the outcome is independent of the executing thread and of whether `UiThread.Initialize()` ran earlier.
- [x] [P1-T13] In `UtilitiesCS.Test/OutlookObjects/Folder/WpfDispatcherYieldTests.cs`, strengthen `YieldAsync_CanceledToken_ThrowsBeforeDispatcherYield` to assert that neither provider delegate was invoked, proving the cancellation guard runs before dispatcher resolution. Acceptance: `OperationCanceledException` is still asserted and both invocation counts are 0.
- [x] [P1-T14] Verify `UtilitiesCS.Test/OutlookObjects/Folder/WpfDispatcherYieldTests.cs` uses MSTest attributes, FluentAssertions, and Moq only where a mock is warranted, creates no temporary files, and contains no `Thread.Sleep`, `Task.Delay`, retry loop, `[DoNotParallelize]`, or `[Ignore]`. Acceptance: a grep of the file for those tokens returns zero hits.
- [x] [P1-T15] Verify no `.cs`, `.csproj`, or `.sln` file was added or removed and that neither `UtilitiesCS/UtilitiesCS.csproj` nor `UtilitiesCS.Test/UtilitiesCS.Test.csproj` was modified. Acceptance: `git diff --name-only -- '*.cs' '*.csproj' '*.sln'` lists exactly `UtilitiesCS/OutlookObjects/Folder/WpfDispatcherYield.cs` and `UtilitiesCS.Test/OutlookObjects/Folder/WpfDispatcherYieldTests.cs`, and `git status --porcelain -- '*.cs' '*.csproj' '*.sln'` reports only those two files as modified with no added or deleted entry. Do **not** gate on unscoped `git diff --name-only`: `.claude/agent-memory/**` is tracked and already modified at branch head (three files at merge-base `003c5715`) and may be modified further during execution, so an unscoped "lists exactly" assertion is unsatisfiable — the same scoping rationale as P0-T3.
- [x] [P1-T16] Verify the scope boundary held: `git diff --name-only -- '*.cs' '*.csproj' '*.sln'` contains no path under `TaskMaster/Ribbon/`, no `UtilitiesCS/Threading/UiThread.cs`, and no other test file. Record that scoped file list in `<FEATURE>/evidence/other/scope-boundary.<ts>.md` together with the exact command, and state that the list is scoped to source paths because `.claude/agent-memory/**` is modified at branch head (see P0-T3).

### Phase 2 — Final QC loop, repeated-run proof, and reduced-audit handoff

- [x] [P2-T1] Toolchain step 1 (format): run `csharpier format .` from the workspace root via `C:\Users\DanMoisan\.dotnet\tools\csharpier.exe` (CSharpier 1.3.0 requires the `format` subcommand; bare `csharpier .` is not a valid invocation, and `dotnet tool run csharpier` is unavailable in this checkout). Write `<FEATURE>/evidence/qa-gates/csharpier-format.<ts>.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` including the count of reformatted files. If any file changed, restart the loop at P2-T1.
- [x] [P2-T2] Toolchain step 1 verification: run `csharpier check UtilitiesCS/OutlookObjects/Folder/WpfDispatcherYield.cs UtilitiesCS.Test/OutlookObjects/Folder/WpfDispatcherYieldTests.cs` via `C:\Users\DanMoisan\.dotnet\tools\csharpier.exe` and write `<FEATURE>/evidence/qa-gates/csharpier-check.<ts>.md`; require `EXIT_CODE: 0`. Do not substitute `pipe-files`, which writes to stdout and does not enforce.
- [x] [P2-T3] Toolchain step 2 (lint): run `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` and write `<FEATURE>/evidence/qa-gates/msbuild-analyzers.<ts>.md` with all four required fields. On failure, fix and restart at P2-T1.
- [x] [P2-T4] Toolchain step 3 (type-check): run `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true` and write `<FEATURE>/evidence/qa-gates/msbuild-nullable.<ts>.md` with all four required fields. On failure, fix and restart at P2-T1.
- [x] [P2-T5] Toolchain step 4 (test with coverage): run `pwsh -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug -CoverageOutput "<FEATURE>/evidence/qa-gates/coverage-postchange.cobertura.xml"`, assert that every discovered assembly path begins with the workspace-root prefix `C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-ad7090ae544fd0fb0\` and that no discovered path contains a `\.claude\worktrees\` segment after that prefix, and write `<FEATURE>/evidence/qa-gates/tests-coverage.<ts>.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, and an `Output Summary:` carrying numeric total/passed/failed counts and the root `line-rate` and `branch-rate`.
- [x] [P2-T6] Attest that P2-T1 through P2-T5 completed in that order within a single pass with no failure and no file rewritten, and record the pass ordinal and per-step exit codes in `<FEATURE>/evidence/qa-gates/toolchain-clean-pass.<ts>.md`.
- [x] [P2-T7] Repeat run 1 of 3: resolve `vstest.console.exe` via `C:\Program Files (x86)\Microsoft Visual Studio\Installer\vswhere.exe` (`vstest.console.exe` is not on PATH; the resolved location in this environment is `C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe`) and run `<vstest> UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation /TestCaseFilter:"TestCategory!=LiveOutlook"` (class-level parallelization, `Workers=0`), writing `<FEATURE>/evidence/qa-gates/repeat-run-1.<ts>.md` with all four required fields plus the per-test outcome of every `WpfDispatcherYieldTests` method. The assembly path is named explicitly and is workspace-root-relative, so no stale sibling-worktree assembly can be discovered.
- [x] [P2-T8] Repeat run 2 of 3: rerun the identical command from P2-T7 and write `<FEATURE>/evidence/qa-gates/repeat-run-2.<ts>.md` with all four required fields plus the per-test outcome of every `WpfDispatcherYieldTests` method.
- [x] [P2-T9] Repeat run 3 of 3: rerun the identical command from P2-T7 and write `<FEATURE>/evidence/qa-gates/repeat-run-3.<ts>.md` with all four required fields plus the per-test outcome of every `WpfDispatcherYieldTests` method.
- [x] [P2-T10] Compare the three artifacts `<FEATURE>/evidence/qa-gates/repeat-run-1.<ts>.md`, `repeat-run-2.<ts>.md`, and `repeat-run-3.<ts>.md` and record in `<FEATURE>/evidence/qa-gates/repeat-run-comparison.<ts>.md` that all four `WpfDispatcherYieldTests` methods passed in all three runs and that the assembly total/passed/failed counts are identical across runs. A single green run is insufficient; any divergence fails this task.
- [x] [P2-T11] Compute the repository-wide coverage delta from `<FEATURE>/evidence/baseline/coverage-baseline.cobertura.xml` versus `<FEATURE>/evidence/qa-gates/coverage-postchange.cobertura.xml` and record baseline `line-rate`, post-change `line-rate`, and the signed delta in `<FEATURE>/evidence/qa-gates/coverage-delta.<ts>.md`. Require a non-negative line-rate delta.
- [x] [P2-T12] Record the changed-code coverage for the two in-scope files in `<FEATURE>/evidence/qa-gates/coverage-changed-lines.<ts>.md`. Compute the changed-class figure by aggregating every `<class>` element in the post-change Cobertura report whose `filename` is `UtilitiesCS/OutlookObjects/Folder/WpfDispatcherYield.cs` — this is the `UtilitiesCS.OutlookObjects.Folder.WpfDispatcherYield` element **plus** its compiler-generated nested types (`<YieldAsync>d__*` async state machine, `<>c*` lambda display classes). Reading the named class element alone is invalid and will understate the figure to roughly 83%. Record: the aggregated line count, the aggregated covered-line count, the derived aggregated line rate, the aggregated branch rate, the uncovered lines by source line number, and an explicit statement of which branch remains uncovered and why. Require aggregated line coverage >= 90% for the changed class per `.claude/rules/csharp.md`; report the branch figure as measured rather than asserting 100%. The single expected uncovered line is the default fallback provider lambda body `() => UtilitiesCS.UiThread.Dispatcher` (see the `## Design Decision — [ExcludeFromCodeCoverage]` section); if any additional line is uncovered or the aggregated line rate is below 90%, record the shortfall and escalate rather than weakening the gate.
- [x] [P2-T13] Verify none of the prohibited fixes was used: grep the **scoped** diff `git diff -- UtilitiesCS/OutlookObjects/Folder/WpfDispatcherYield.cs UtilitiesCS.Test/OutlookObjects/Folder/WpfDispatcherYieldTests.cs` for `DoNotParallelize`, `Ignore]`, `Thread.Sleep`, `Task.Delay`, `Retry`, `GetField(`, and `BindingFlags`, and confirm the assertion in `UtilitiesCS.Test/OutlookObjects/Folder/WpfDispatcherYieldTests.cs` is still `ThrowAsync<InvalidOperationException>()`; record the exact command and the grep output in `<FEATURE>/evidence/qa-gates/prohibited-fix-audit.<ts>.md`. Any hit **within the scoped diff** fails this task. Do **not** grep an unscoped `git diff`: `.claude/agent-memory/atomic-planner/MEMORY.md` is modified at branch head and its text already contains the literal token `DoNotParallelize`, producing a false positive unrelated to the fix. Scoping loses no coverage of this check because P1-T15 independently proves the two in-scope files are the entire `.cs` diff.
- [x] [P2-T14] Verify no runtime behavior change: confirm the public surface of `UtilitiesCS/OutlookObjects/Folder/WpfDispatcherYield.cs` gained only the explicit parameterless constructor, that the seam constructor is `internal`, that the default delegates match the pre-change expressions captured in `<FEATURE>/evidence/baseline/source-under-test.<ts>.md`, and that no call site changed. Record in `<FEATURE>/evidence/qa-gates/no-behavior-change.<ts>.md`.
- [x] [P2-T15] Audit that every artifact produced by this plan resides under `<FEATURE>/evidence/<kind>/` and that no file was written to `artifacts/baseline`, `artifacts/qa`, `artifacts/coverage/`, or `artifacts/evidence/`; record the audit in `<FEATURE>/evidence/other/evidence-path-audit.<ts>.md`.
- [x] [P2-T16] Check off AC1 in `<FEATURE>/issue.md` citing `<FEATURE>/evidence/qa-gates/repeat-run-comparison.<ts>.md` as evidence, per `.claude/skills/acceptance-criteria-tracking/SKILL.md`.
- [x] [P2-T17] Check off AC2 in `<FEATURE>/issue.md` citing `<FEATURE>/evidence/qa-gates/prohibited-fix-audit.<ts>.md` as evidence.
- [x] [P2-T18] Check off AC3 in `<FEATURE>/issue.md` citing `<FEATURE>/evidence/qa-gates/coverage-changed-lines.<ts>.md` as evidence.
- [x] [P2-T19] Check off AC4 in `<FEATURE>/issue.md` citing `<FEATURE>/evidence/qa-gates/no-behavior-change.<ts>.md` as evidence.
- [x] [P2-T20] Check off AC5 in `<FEATURE>/issue.md` citing `<FEATURE>/evidence/qa-gates/prohibited-fix-audit.<ts>.md` as evidence.
- [x] [P2-T21] Check off AC6 in `<FEATURE>/issue.md` citing `<FEATURE>/evidence/regression-testing/fail-before.<ts>.md` (or the `fail-before-exception.<ts>.md` dossier) as evidence.
- [x] [P2-T22] Check off AC7 in `<FEATURE>/issue.md` citing `<FEATURE>/evidence/qa-gates/repeat-run-1.<ts>.md`, `repeat-run-2.<ts>.md`, and `repeat-run-3.<ts>.md` as evidence.
- [x] [P2-T23] Check off AC8 in `<FEATURE>/issue.md` citing `<FEATURE>/evidence/qa-gates/toolchain-clean-pass.<ts>.md` as evidence.
- [x] [P2-T24] Check off AC9 in `<FEATURE>/issue.md` citing `<FEATURE>/evidence/qa-gates/coverage-delta.<ts>.md` as evidence.
- [x] [P2-T25] Reconcile the acceptance criteria: confirm all nine boxes in the `## Acceptance Criteria` section of `<FEATURE>/issue.md` are `[x]` and that each cites an artifact that exists on disk; record the reconciliation in `<FEATURE>/evidence/issue-updates/ac-reconciliation.<ts>.md`.
- [x] [P2-T26] Check off the `baseline` item in the `## Evidence Checklist` section of `<FEATURE>/issue.md`, citing `<FEATURE>/evidence/baseline/phase0-completeness.<ts>.md`.
- [x] [P2-T27] Check off the `targeted verification` item in the `## Evidence Checklist` section of `<FEATURE>/issue.md`, citing `<FEATURE>/evidence/regression-testing/fail-before.<ts>.md`.
- [x] [P2-T28] Check off the `end-state` item in the `## Evidence Checklist` section of `<FEATURE>/issue.md`, citing `<FEATURE>/evidence/qa-gates/toolchain-clean-pass.<ts>.md`.
- [x] [P2-T29] Hand off to the small-path reduced `feature-review` audit with the reduced artifact set: `<FEATURE>/issue.md`, this plan, `<FEATURE>/evidence/baseline/`, `<FEATURE>/evidence/regression-testing/`, and `<FEATURE>/evidence/qa-gates/`; record the handoff and the reduced-audit scope in `<FEATURE>/evidence/other/reduced-audit-handoff.<ts>.md`.

## Reduced Audit Block (small path)

The post-implementation audit is the reduced `feature-review` pass. Required checks for this cycle:

- Requirements traceability limited to the `## Acceptance Criteria` section of `<FEATURE>/issue.md`;
  `spec.md` and `user-story.md` are not required and their absence is not a finding.
- Policy compliance for `.claude/rules/csharp.md` (DI seams, prohibited behaviors) and
  `.claude/rules/general-unit-test.md` (determinism, coverage exclusion policy).
- Evidence completeness against `<FEATURE>/evidence/baseline/`,
  `<FEATURE>/evidence/regression-testing/`, and `<FEATURE>/evidence/qa-gates/`.
- Coverage gate: repository line-rate non-regression plus changed-class line coverage >= 90%.
- Scope boundary: diff confined to the two in-scope files.

## Traceability

- AC1 -> P1-T12, P2-T7..P2-T10, P2-T16
- AC2 -> P1-T5, P1-T12, P2-T13, P2-T17
- AC3 -> P1-T10, P1-T11, P1-T12, P2-T12, P2-T18
- AC4 -> P1-T2, P1-T3, P1-T4, P1-T15, P2-T14, P2-T19
- AC5 -> P1-T14, P2-T13, P2-T20
- AC6 -> P0-T12, P0-T13, P2-T21
- AC7 -> P2-T7, P2-T8, P2-T9, P2-T10, P2-T22
- AC8 -> P2-T1..P2-T6, P2-T23
- AC9 -> P0-T10, P0-T11, P2-T11, P2-T12, P2-T24

## Notes

- The intermittent failure mode observed at baseline is `Failed`, not `Hang`, which indicates the
  accidentally-resolved dispatcher in those runs was pumping. That is consistent with operand 2
  (`UiThread.Dispatcher`, populated by `UiThread.Init()` which shows and pumps a `SyncContextForm`)
  being the dominant contributor. The fix arranges both operands, so it covers either path.
- File-size limit: `UtilitiesCS/OutlookObjects/Folder/WpfDispatcherYield.cs` is 44 lines and
  `UtilitiesCS.Test/OutlookObjects/Folder/WpfDispatcherYieldTests.cs` is 39 lines pre-change. Both
  remain far below the 500-line limit after the planned additions; no type split is required.
- `Thread.Sleep` and `Task.Delay` are listed in `BannedSymbols.txt`, but RS0030 is held at
  `severity = suggestion`, so the analyzer build will not fail on them. P2-T13 greps the diff
  directly rather than relying on the analyzer.
- Execution note (environment, not a plan defect): after the P0-T7 restore, watch for analyzer
  version skew between `<Analyzer Include>` HintPaths and the `packages.config` pins. That skew
  produces CS0006 on a fresh worktree and must be resolved as an environment issue.
- Execution note: `TaskMaster.Test` and `UtilitiesCS.Test` may fail to build if the Office Tools
  v4.0 VSTO runtime is absent (four CS0234 diagnostics in `ThisAddIn.Designer.cs`), which would
  deflate the repository-wide baseline line-rate. P0-T10 records whatever is measured and P2-T11
  compares like-for-like against that same baseline, so this does not invalidate the delta gate —
  but the executor must state the condition explicitly in the baseline `Output Summary:`.
- Execution note: the full-suite coverage runs (roughly 6293 tests) are long. If a run is killed,
  also kill the detached `pwsh` runner process, not just the testhosts, before retrying.
- Environment note (not a defect): `dotnet-tools.json` pins CSharpier 1.2.6 while the global
  executable this plan uses (`C:\Users\DanMoisan\.dotnet\tools\csharpier.exe`) is 1.3.0. P0-T6,
  P2-T1, and P2-T2 all invoke the same 1.3.0 binary, so the baseline and the gate are internally
  consistent, and no `.csproj` references `CSharpier.MsBuild`, so no version cross-check will fire.
  The reduced audit must not read this version difference as a defect.
- Git-gate scoping (binding on P0-T3, P0-T14, P1-T15, P1-T16, P2-T13): `.claude/agent-memory/**` is
  tracked and is already modified at branch head (three files versus merge-base `003c5715`), and
  agents write further memory during execution. Every diff/status/grep gate in this plan is
  therefore scoped with an explicit pathspec (`-- '*.cs' '*.csproj' '*.sln'`, or the two in-scope
  file paths). Unscoped `git diff`/`git status --porcelain` assertions are unsatisfiable here and
  must not be substituted.
