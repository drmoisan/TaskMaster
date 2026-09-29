# 2026-09-28-tests-depend-on-uncontrolled-environment (Plan)

- **Issue:** #931 (consolidates #905 and #906)
- **Parent (optional):** none
- **Owner:** drmoisan
- **Last Updated:** 2026-09-28
- **Status:** Ready for preflight (planner internal review passed; executor validation-only preflight pending)
- **Version:** 1.0
- **Work Mode:** full-bug (persisted marker `- Work Mode: full-bug` at line 12 of the feature issue document; the feature spec is the sole acceptance-criteria source, AC1 through AC19; no user-story document exists or is required)
- **Requirements source:** `docs/features/active/2026-09-28-tests-depend-on-uncontrolled-environment-931/spec.md`, section `## Acceptance Criteria`
- **Branch:** bug/tests-depend-on-uncontrolled-environment-931
- **Base:** origin/main. Every diff in this plan is anchored to `MERGE-BASE`, the merge base of HEAD and origin/main computed by P0-T3 at execution time after a fetch, never to a bare local main ref and never to a SHA pinned by this document.
- **Execution session requirement:** the executor runs later, non-isolated, from the item worktree, with `pwsh` available. Every command-bearing task runs either a `git` invocation or one `pwsh -NoProfile -Command` process whose first statement is `Set-Location -LiteralPath "WORKTREE"`, where `WORKTREE` is the absolute worktree root the delegation prompt supplies (no trailing separator). P0-T4 probes that channel and stops the run with `CHANNEL UNAVAILABLE` if it is refused; the executor never edits hook or permission configuration to obtain a channel.
- **Pre-implementation gate requirement:** the hook at .claude/hooks/enforce-orchestration-preimplementation-gate.ps1 classifies every edit of a `.cs` or `.csproj` file and every non-exempt `git add` or `git commit` as implementation and denies them unless artifacts/orchestration/orchestrator-state.json is seeded and ready (fact 22). P0-T3 records that readiness read-only and stops with `PRE-IMPLEMENTATION GATE NOT SEEDED` when it is absent. Seeding the checkpoint is the orchestrator's responsibility; the executor never creates, edits, or works around it.
- **Task Count:** 66 (Phase 0: 13, Phase 1: 1, Phase 2: 10, Phase 3: 9, Phase 4: 33)

**Fail-closed evidence rule:** Include explicit baseline artifact tasks, final-QA artifact tasks, and coverage-comparison tasks for each in-scope language when policy requires coverage. If any required baseline artifact, QA artifact, or coverage-comparison artifact is missing, the audit verdict must be BLOCKED or INCOMPLETE, never PASS. One scoping carve-out: in a task that both commits and records that commit, the artifact's `EXIT_CODE:` field is scoped to the pre-commit observation the task names, and the commit's own exit code is reported under a separately named field or in the executor's final message, because an artifact cannot record the exit code of the commit that includes it.

**Evidence accounting rule:** Record the expected artifact path or location in each evidence-producing task. Do not mark evidence-backed work complete without the artifact.

---

## Binding Operator Constraints (received; encoded below)

- Temporary files are prohibited in tests and in plan helper steps inside the repository. The file-handle fix uses the test-owned `FileStream` over the running test host's own loaded assembly image through the existing internal `FileInfoWrapper(IFileInfo)` seam; no repository file and no temporary file is opened, created, written or deleted by any test. Every scratch output this plan writes lives under the git-ignored coverage directory (.gitignore line 144) and is never an acceptance input except by its presence flags.
- Tests stay in the parallel regime: no task adds `[DoNotParallelize]`, changes Workers or Scope, adds a retry attribute or loop, or adds `Thread.Sleep`, `Task.Delay`, a timeout or a wall-clock wait. P4-T11 gates the added lines of the diff for those tokens.
- Only the two AFFECTED sites the spec names are rewritten; the twenty UNAFFECTED sites are byte-identical to the merge base (P4-T11).
- The 500-line ceiling is gated on every `.cs` file in the Write Set after the repository-wide format (P4-T9).
- Each negative control is applied, built, observed failing, reverted with `git checkout` of the mutated file, proven reverted by an anchored `git diff --exit-code HEAD` span plus a pathspec-scoped porcelain span plus a SHA-256 equality, rebuilt, and observed passing, inside one artifact per control (Phase 3). The final diff contains no production change (P3-T9, P4-T11).
- Committed evidence is Markdown projections only. No `.trx`, `.xml` or `.coverage` file is added anywhere (P4-T12). No committed text carries an absolute host path, an account name or a host name; the placeholders `<repo-root>`, `<user-profile>`, `<user>` and `<host>` are used instead (P4-T12 sweeps before the final commit).
- Evidence paths are canonical: FEATURE/evidence/baseline/, FEATURE/evidence/regression-testing/ and FEATURE/evidence/qa-gates/ only. No task writes under artifacts/ except that the orchestrator-owned checkpoint under artifacts/orchestration/ is read by P0-T3.

## Execution Conventions

- `FEATURE` denotes `docs/features/active/2026-09-28-tests-depend-on-uncontrolled-environment-931`. `WORKTREE` denotes the absolute worktree root supplied by the delegation prompt, without a trailing separator. Both tokens are expanded by the executor; `WORKTREE` is never written into any artifact, commit message or console transcript that is copied into an artifact.
- **Evidence paths.** Every artifact is written under FEATURE/evidence/baseline/, FEATURE/evidence/regression-testing/ or FEATURE/evidence/qa-gates/. Nothing is written to artifacts/baseline/, artifacts/baselines/, artifacts/qa/, artifacts/qa-gates/, artifacts/evidence/, artifacts/coverage/, artifacts/regression-testing/ or artifacts/post-change/.
- **Artifact filenames.** The spec fixes ten projection names without a timestamp suffix: test-run-baseline.md and coverage-baseline.md under baseline/; mutation-owner-only-dispatcher-guard.md, mutation-null-owner-escape.md, mutation-inline-precondition.md, mutation-openread-sentinel.md, parallel-suite-quickfiler-test.md and parallel-suite-utilitiescs-test.md under regression-testing/; toolchain-pass.md and coverage-final.md under qa-gates/. The atomic-plan contract fixes phase0-instructions-read.md under baseline/. Every other artifact is named `<task-id>-<name>.<TS>.md` where `<TS>` is the executor's write time in `yyyy-MM-ddTHH-mm`, equal to the artifact's own `Timestamp:` field; later tasks locate it with the glob `<task-id>-<name>.*.md`, which must match exactly one file.
- **Artifact fields.** Every command-step artifact carries `Timestamp:`, `Command:`, `EXIT_CODE:` and `Output Summary:`. `ExpectedExitCode:` is written only where a task says so, at most once per artifact, and always equal to the observed value it explains. An artifact that records several commands names the one invocation its `EXIT_CODE:` row is scoped to and records the other exit codes as named `Output Summary:` lines. `Command:` records the plan's canonical form with every token except `WORKTREE` substituted.
- **Command channel.** Every `pwsh` payload is written as `pwsh -NoProfile -Command '<payload>'`: outer single quotes so the calling shell performs no interpolation, the payload's lines joined by `; `, its first statement `Set-Location -LiteralPath "WORKTREE"`. No payload contains a single-quote character; every string literal is double-quoted; a double quote inside a double-quoted literal is doubled; no double-quoted literal ends with a backslash immediately before its closing quote. A script that must run as its own process is started from inside a payload as `& pwsh -NoProfile -File (Join-Path (Get-Location).Path "scripts\vscode\<name>.ps1")`, that is by absolute script path resolved at run time; `pwsh -WorkingDirectory` is never used because it does not resolve the `-File` path. The child's exit code is read from `$LASTEXITCODE` after merging its error stream with `2>&1`; a PowerShell try/catch cannot observe an external process failure.
- **Working directory.** Every path inside a payload is repository-relative after the `Set-Location`; cmdlets take `-LiteralPath`; .NET static file APIs are never given a relative path because `Set-Location` does not update the .NET current directory.
- **File hashes.** Every SHA-256 this plan records or compares is the `Hash` property of `Get-FileHash -Algorithm SHA256 -LiteralPath <repository-relative path>`; the `Path` property is never recorded because it is absolute.
- **Tool resolution inside payloads.** vswhere.exe is `Join-Path ${env:ProgramFiles(x86)} "Microsoft Visual Studio\Installer\vswhere.exe"`; vstest.console.exe is `& $vswhere -latest -products * -find "Common7\IDE\Extensions\TestPlatform\vstest.console.exe" | Select-Object -First 1`; MSBuild.exe is `& $vswhere -latest -requires Microsoft.Component.MSBuild -find "MSBuild\**\Bin\MSBuild.exe" | Select-Object -First 1` (the shape scripts/vscode/Invoke-Restore.ps1 uses at its line 31). The `Command:` field records the CLAUDE.md-canonical `msbuild ...` and `vstest.console.exe ...` forms plus the note `resolved through vswhere`.
- **MSBuild node reuse.** Every msbuild invocation in this plan carries the additional switch /nodeReuse:false so the plan's own worker nodes exit when the build ends and hold no repository file open during a later test run (D-16). The switch changes no diagnostic, target or property; the `Command:` field records the CLAUDE.md-canonical command and the note `plus /nodeReuse:false`.
- **Console counters.** On an all-green run vstest.console.exe prints `Test Run Successful.`, `Total tests:`, `Passed:`, `Total time:` and no `Failed:` or `Skipped:` line; on a failing run it prints `Test Run Failed.`. Per-test outcomes and failure messages are read from the TRX the run writes (the ResultSummary/Counters attributes `total`, `executed`, `passed`, `failed`; the `UnitTestResult` attributes `testName` and `outcome`; the Output/ErrorInfo/Message element). TRX files are written under coverage\test-results\931\<task-id>\ (git-ignored by .gitignore line 144) and are never copied into FEATURE/evidence/. Every direct run passes an explicit results directory and a quoted trx logger with an explicit log file name, so no file name carries an account or machine name.
- **Runsettings for direct runs.** Every direct vstest.console.exe run in this plan uses the repository-root runsettings file TaskMaster.runsettings (Workers 0, Scope ClassLevel, lines 4 through 7) with the isolation switch, as spec AC16 requires; its SHA-256 is recorded once by P0-T4 as `RUNSETTINGS-HASH:` and re-read by every run as `RUNSETTINGS-HASH-NOW:`. That file also declares a Code Coverage data collector (lines 9 through 29); when the installed test platform activates it, the collector's own output lands under the run's results directory inside the git-ignored coverage tree, and the count of console lines mentioning `Code Coverage` is recorded as `COLLECTOR_LINES:` (an observation, not a gate). The coverage route (P0-T11, P4-T7) uses the CLI twin scripts/vscode/TaskMaster.cli.runsettings through the runner script, exactly as CLAUDE.md step 4 does.
- **Token census.** Every occurrence count in this plan is produced by `CMD-CENSUS` (Command Reference), which reads one file and counts case-sensitive, non-overlapping occurrences with `[regex]::Matches($content, [regex]::Escape($token)).Count`. Counts are occurrences, not lines. `Select-String` is never used for a count because it is case-insensitive by default and counts lines.
- **Refs.** P0-T3 records `BASE-SHA:` (HEAD before any task edits a file) and `MERGE-BASE:` (the output of `git merge-base HEAD origin/main` after `git fetch origin main`). P2-T10 records `FIX-HEAD:` and one `FIX-HASH-<n>:` per Write Set code file. Command spans write the bare tokens; the executor substitutes the recorded values, because no shell variable survives a task boundary.
- **Inherited paths (rule, not list).** Clause A: every path already changed relative to `MERGE-BASE` before the first task ran, captured mechanically by P0-T3 as `INHERITED-CLAUSE-A:` (the union of `git diff --name-only MERGE-BASE...HEAD` and `git status --porcelain --untracked-files=all`). Clause B: every path under the prefix .claude/agent-memory/, which is tracked and is written by agents while this plan executes. Footprint gates subtract the recorded Clause A set and the Clause B prefix, record the subtraction explicitly, and never subtract a Write Set path.
- **Git gates are pathspec-scoped.** Every `git diff` and `git status` gate carries an explicit pathspec so that agent-memory writes and sibling-worktree residue cannot fail it. No gate asserts an empty unscoped porcelain output. Every `git diff` carries a ref operand; every name-listing diff is paired with a porcelain span in the same task.
- **Commits.** This plan commits three times, each as `git add -- <pathspecs>` followed by a separate `git commit` invocation, one command segment per invocation, never chained. P0-T13 stages only the exempt tree docs/features/active/ and uses the issue #539 exemption form (a single `-m "<subject>"`, one pathspec operand after `--`, no `$`, backtick, `<` or `>` character anywhere on the line). P2-T10 and P4-T33 stage paths outside every exempt tree, so they run only in a session whose pre-implementation checkpoint P0-T3 recorded as ready; if the executing session's harness requires an attribution trailer on commits, it is supplied as a second `-m` paragraph on those two commits only. A PreToolUse refusal of any `git add`, `git commit`, `.cs` edit or `.csproj` edit is reported verbatim as `PRE-IMPLEMENTATION GATE BLOCKED` and stops the run; the executor does not modify hooks, checkpoints, permission configuration or another item's state.
- **Stall handling and the two recorded selections.** P0-T9 runs the four UtilitiesCS.Test shell-icon classes alone under the hang-dump blame collector and records exactly one `STALL-PROBE:` value: `CLEAR` (exit 0, `failed` 0, no Sequence document) or `REPRODUCES` (anything else). That value fixes two later substitutions: `UCS-FILTERARG` is empty under `CLEAR` and is the quoted exclusion filter of the four classes under `REPRODUCES` (the single filter spec AC16 permits, recorded verbatim in every artifact that uses it); `COVERAGE-ROUTE` is `RUNNER` under `CLEAR` (the CLAUDE.md step 4 runner script) and `DIRECT` under `REPRODUCES` (the runner's own inner collector invocation with the same four-class exclusion appended, because the runner hard-codes its filter at line 91 and offers no extension point). Every repository-wide and full-assembly run carries the hang-dump blame argument so a stalled test is named in a `Sequence_*.xml` document rather than left silent; a run that produces one is recorded as failed with that test name.
- **Negative-control mechanics.** Each control: apply the mutation with the Edit tool; run `CMD-CENSUS` on the mutated file; build with `CMD-BUILD`; run with `CMD-VSTEST` under the control's filter; record the failing run (`ExpectedExitCode: 1`); revert with `git checkout -- <file>`; prove the revert with `git diff --exit-code HEAD -- <file>` (exit 0), `git status --porcelain -- <file>` (empty) and the file's SHA-256 equal to its `FIX-HASH-<n>:`; rebuild; run the same filter again and record the passing run. The fix commit P2-T10 precedes every control so that `git checkout` restores exactly the committed fixed state.
- **Artifact hygiene.** Before any text is written into an artifact, an absolute path is replaced by `<repo-root>` (or `<user-profile>` when it lies under the user profile), the account name by `<user>` and the machine name by `<host>`. The `ASSEMBLY:` lines the direct coverage route prints are already worktree-relative (they begin with a backslash).

## Write Set

Code and project files (six; one line each, exactly the spec's Write Set):

- `QuickFiler.Test/Viewers/ItemViewerBreadcrumbThreadAffinityTests.cs` (modify: becomes partial; loses the three cross-thread tests, `ClearViewerDispatcher` and `RunOnDedicatedWorkerThread`)
- `QuickFiler.Test/Viewers/ItemViewerBreadcrumbThreadAffinityTests.Part2.cs` (create: partial continuation with the three cross-thread tests and `ClearViewerDispatcher`)
- `QuickFiler.Test/TestSupport/DedicatedWorkerThread.cs` (create: shared dedicated-thread helper)
- `QuickFiler.Test/Viewers/BreadcrumbPopupBoundaryCoverageTests.cs` (modify: rewrite `Dispatcher_OwnerOnlyWorker_ReportsWithoutRunningAction`)
- `QuickFiler.Test/QuickFiler.Test.csproj` (modify: two Compile Include entries)
- `UtilitiesCS.Test/HelperClasses/FileInfoWrapper_Tests.cs` (modify: rewrite four tests; remove `GetSolutionFile`)

Evidence directories (three; created by this run):

- `docs/features/active/2026-09-28-tests-depend-on-uncontrolled-environment-931/evidence/baseline/`
- `docs/features/active/2026-09-28-tests-depend-on-uncontrolled-environment-931/evidence/regression-testing/`
- `docs/features/active/2026-09-28-tests-depend-on-uncontrolled-environment-931/evidence/qa-gates/`

Feature documents whose checkbox state this run edits (admitted by spec AC15's feature-folder clause; no criterion text or plan text changes):

- `docs/features/active/2026-09-28-tests-depend-on-uncontrolled-environment-931/spec.md`
- `docs/features/active/2026-09-28-tests-depend-on-uncontrolled-environment-931/plan.2026-09-28T20-01.md`

## Outside the Write Set (plain prose; none of these paths is modified by the final diff)

Production files, never changed in the final diff: QuickFiler/Viewers/BreadcrumbUiDispatcher.cs (P3-T1 edits lines 276 to 277 temporarily; P3-T2 reverts it) and QuickFiler/Viewers/ItemViewer.Breadcrumb.cs (P3-T3 edits lines 435 to 438 temporarily; P3-T4 reverts it); every other file under QuickFiler/ and UtilitiesCS/, including UtilitiesCS/HelperClasses/FileSystem/FileInfoWrapper.cs whose internal seam at lines 21 to 24 is used as is.

The twenty UNAFFECTED Task.Run sites in QuickFiler.Test (spec Triage table): Helper Classes/EmailMoveMonitorTests.cs, Viewers/BreadcrumbCoordinatorLifecycleTests.cs, Viewers/BreadcrumbPopupControlDispatchTests.cs, Viewers/BreadcrumbPopupBoundaryCoverageTests.Part2.cs, Viewers/BreadcrumbSelectorToggleUiBoundaryTests.cs, Viewers/BreadcrumbSelectorOpenRetryTests.cs, Viewers/BreadcrumbUiThreadDispatchTests.cs, Controllers/QfcItemController.UiThreadDispatcherFixtureTests.cs; and the two pre-existing `[DoNotParallelize]` attributes in Helper Classes/EmailMoveMonitorTests.cs line 24 and Helper Classes/ViewerQueueStaticWrapperTests.cs line 11.

Configuration read but never written: TaskMaster.runsettings, scripts/vscode/TaskMaster.cli.runsettings, coverage.config, every script under scripts/vscode/, the .claude directory, config/blast-radius.json, config/orchestration-routing.json, artifacts/orchestration/orchestrator-state.json.

Local, git-ignored, never staged: everything under coverage/ (.gitignore line 144; coverage/.gitkeep is tracked so the directory exists), packages/, .dotnet-sdk/, every bin and obj directory.

## Verified Repository Facts

Each fact was re-derived against the assigned worktree on 2026-09-28 during plan authoring with the Read and Grep tools.

1. `QuickFiler.Test/Viewers/ItemViewerBreadcrumbThreadAffinityTests.cs` is 490 lines (the terminating newline is not a line) and declares `[TestClass] public sealed class ItemViewerBreadcrumbThreadAffinityTests` at lines 29 to 30 with seven `[TestMethod]` members at lines 38, 88, 128, 167, 218, 268, 318 (method declarations at 39, 89, 129, 168, 219, 269, 319). Its using block is lines 1 to 12 (`System`, `System.Collections.Generic`, `System.Drawing`, `System.Reflection`, `System.Threading`, `System.Threading.Tasks`, `System.Windows.Threading`, `FluentAssertions`, `Microsoft.VisualStudio.TestTools.UnitTesting`, `Moq`, `QuickFiler.Viewers`, `UtilitiesCS.OutlookObjects.Folder`); the class remarks are lines 21 to 28. It carries no nullable directive.
2. In that file: the two dedicated-thread tests span lines 199 to 251 and 253 to 304 (documentation through closing brace); the null-owner test spans 306 to 346 with its `Task.Run` at 332 and its blocking wait at 335 to 336, and its remark at 311 to 317; line 347 is blank; `InertOperations` is 348 to 355; line 356 is blank; `ClearViewerDispatcher` is 357 to 371; line 372 is blank; `RunOnDedicatedWorkerThread` is 373 to 403 (remark 377 to 384, body 385 to 403, `action();` at 392); line 404 is blank; `InertDropDownHost` 405 to 434; `DrainableSynchronizationContext` 436 to 461; `ViewerScope` 463 to 488. The precondition reason literal `the dedicated worker thread must not be the thread that constructed ` sits on one line at 234 and 283; the remark literal `distinct from every live thread by construction` sits on one line at 380.
3. Pre-edit occurrence counts in that file (positive controls; P0-T12 re-records them): `Task.Run(` 1; `.GetAwaiter()` 1; `RunOnDedicatedWorkerThread` 5; `ClearViewerDispatcher(` 2; `partial class` 0; `[TestMethod]` 7; `action();` 1; `new Thread(` 1; `IsBackground = true` 1; `thread.Join();` 1; `using System.Reflection;` 1; `dedicated worker thread must not be` 2; `distinct from every live thread by construction` 1; `unconditionally` 0; `owner.CheckAccess()` 0; `.BeNull(` 0; `DedicatedWorkerThread.Run(` 0; `using QuickFiler.Test.TestSupport;` 0; `Thread.Sleep`, `Task.Delay`, `[Timeout`, `DoNotParallelize` 0 each.
4. `QuickFiler.Test/Viewers/BreadcrumbPopupBoundaryCoverageTests.cs` is 361 lines, declares `public sealed partial class BreadcrumbPopupBoundaryCoverageTests` at line 17 (its continuation partial carries the same declaration at line 23 of its Part2 file and no `[TestClass]`), and holds `Dispatcher_OwnerOnlyWorker_ReportsWithoutRunningAction` at lines 52 to 62 with `Task.Run(` at 58 and the blocking wait at 59; `CreateOwnerOnlyDispatcher` is 124 to 134 and passes `Environment.CurrentManagedThreadId` as the owner at 133. Its using block is lines 1 to 12 with `using Moq;` at 11 and `using QuickFiler.Viewers;` at 12. Pre-edit counts: `Task.Run(` 1; `.GetAwaiter()` 4 (59, 297, 305, 308); `DedicatedWorkerThread.Run(` 0; `using QuickFiler.Test.TestSupport;` 0; `ownerThreadId` 0; `.NotBe(` 0; `.BeNull(` 0; `executions.Should().Be(0)` 1; `cannot marshal` 1; `dedicated worker thread must not be` 0; `partial class` 1; `Thread.Sleep`, `Task.Delay`, `[Timeout`, `DoNotParallelize` 0 each. The test name is unique in QuickFiler.Test.
5. `UtilitiesCS.Test/HelperClasses/FileInfoWrapper_Tests.cs` is 359 lines, declares `[TestClass] public class FileInfoWrapper_Tests` at 12 to 13 with the class opening brace at 14, eight `[TestMethod]` members (15, 25, 41, 55, 69, 83, 183, 297), the four rewritten tests at 25 to 39, 41 to 53, 55 to 67, 69 to 81, a blank line at 338, and `GetSolutionFile` at 339 to 357 (the `AppDomain` walk-up at 341, the `TaskMaster.sln` literal at 345, `File.Exists(` at 346); line 358 closes the class. It carries no nullable directive and already uses `using var` (line 62) and the rooted literal C:\Repo (line 87). Pre-edit counts: `GetSolutionFile` 5; `TaskMaster.sln` 1; `AppDomain` 1; `File.Exists(` 1; `File.Create`, `File.WriteAll`, `File.Delete`, `Path.GetTemp` 0 each; `FileMode.` 12; `FileMode.Open` 12; `FileAccess.` 10; `FileAccess.Read` 10; `FileShare.ReadWrite` 8; `Assembly.Location` 6; `FixturePath` 0; `using var sentinel = new FileStream(` 0; `BeSameAs(sentinel)` 0; `.Returns(decoy)` 0; `wrapper.OpenRead()` 2; `stream.CanRead.Should().BeTrue()` 1; `stream.Length.Should().BeGreaterThan(0)` 1; `[TestMethod]` 8; `Thread.Sleep`, `Task.Delay`, `[Timeout`, `DoNotParallelize` 0 each. The name `OpenRead_ShouldReturnReadableStreamForWrappedFile` is unique in UtilitiesCS.Test.
6. `QuickFiler.Test/QuickFiler.Test.csproj` uses explicit Compile Include entries: the affinity file at line 98, the boundary file and its Part2 at 105 to 106, the TestSupport entries at 227 to 228 (WinFormsPumpHost.cs, WinFormsPumpHostTests.cs). It defaults Platform to `AnyCPU` at line 12, sets OutputPath bin\Debug\ at 36, and references QuickFiler.csproj by ProjectReference at line 510 (UtilitiesCS.csproj at 514). UtilitiesCS.Test/UtilitiesCS.Test.csproj (outside the Write Set) registers FileInfoWrapper_Tests.cs at line 234, defaults Platform to `AnyCPU` at 11, OutputPath bin\Debug\ at 51, and references UtilitiesCS.csproj by ProjectReference at 963. QuickFiler.csproj and UtilitiesCS.csproj are libraries with AssemblyName `QuickFiler` and `UtilitiesCS` (lines 9 and 12; 12 and 15), so a project build of a test project rebuilds its production project when a production file changed, and the compiler echo for each carries the output path obj\Debug\<AssemblyName>.dll after its out switch. QuickFiler.csproj sets no TreatWarningsAsErrors property.
7. QuickFiler.Test/TestSupport/WinFormsPumpHost.cs declares `namespace QuickFiler.Test.TestSupport` at line 9; the new helper joins that namespace.
8. QuickFiler/Viewers/BreadcrumbUiDispatcher.cs: `Dispatch(Action)` is lines 71 to 151 and calls `IsCurrentBoundary()` at 78; the null-context rejection path reports `The owner-thread-only test dispatcher cannot marshal cross-thread UI work.` and returns `Task.CompletedTask` at 97 to 105; `IsCurrentBoundary()` is 255 to 278; the executing-callback branch returns true at 260; the captured-context branch is 269 to 272; the owner-thread-id branch is lines 276 to 277 (`return _ownerThreadId.HasValue` / `&& Environment.CurrentManagedThreadId == _ownerThreadId.Value;`). The file carries `#nullable enable`.
9. QuickFiler/Viewers/ItemViewer.Breadcrumb.cs: `ThrowIfOffUiBoundary` is lines 432 to 447; the null-owner escape is 435 to 438 (`if (owning == null)` / `{` / `return;` / `}`); the throw is 440 to 446. Its using block (lines 1 to 9) does not import `System.Threading`, so mutation M2 names `System.Threading.SynchronizationContext.Current` fully qualified. QuickFiler/Viewers/ItemViewer.cs captures `_uiDispatcher = Dispatcher.CurrentDispatcher;` at 27, declares `public SynchronizationContext UiSyncContext` at 59 and `public Dispatcher UiDispatcher` at 65 to 67; `Dispatcher.CheckAccess()` is thread-object identity.
10. UtilitiesCS/HelperClasses/FileSystem/FileInfoWrapper.cs: public constructor 14 to 19 wraps a `PhysicalFileInfoAdapter`; internal seam constructor 21 to 24; `OpenRead()` 153 to 156; `ToString()` 191 to 194; explicit `DirectoryInfoWrapper` cast 211 to 214. UtilitiesCS/Properties/AssemblyInfo.cs line 19 carries `InternalsVisibleTo("UtilitiesCS.Test")` (line 18 `DynamicProxyGenAssembly2`). UtilitiesCS/Interfaces/IHelperClasses/IFileInfo.cs line 26 declares `FileStream OpenRead();`. UtilitiesCS/HelperClasses/FileSystem/PhysicalFileInfoAdapter.cs: `Directory` at 104 wraps `FileInfo.Directory`, `DirectoryName` at 108, `OpenRead()` at 154 (unseamed), `ToString()` at 181; all are path computations or metadata reads for a non-existent rooted path.
11. TaskMaster.runsettings (30 lines): `Workers` 0 at line 5, `Scope` ClassLevel at line 6, a `Code Coverage` data collector at lines 9 to 29. scripts/vscode/TaskMaster.cli.runsettings (9 lines): the same Workers and Scope, no collector, no logger.
12. scripts/vscode/Invoke-MSTestWithCoverage.ps1 (439 lines): parameters `SearchRoot`, `Configuration`, `CoverageOutput` (default coverage\coverage.cobertura.xml), `NoExecute` only; no test-filter and no runsettings parameter. `Get-DotnetCoverageArgumentList` (41 to 95) hard-codes the LiveOutlook category test-case filter at 91 and passes the results directory and trx logger at 92 to 93; `ConvertTo-DerivedCoverageSettingsXml` 97 to 134 appends the test-assembly module exclusion; `Invoke-DotnetCoverageCollection` throws `MSTest with coverage failed with exit code <n>` at 261 to 263 on a non-zero collector exit, before any post-processing; the entry point resolves the repository root from its own location (305), the CLI runsettings from its directory (312), discovers `*.Test.dll` under bin\<Configuration>\ excluding obj, ref and .claude relative paths (330 to 337), fixes ResultsDirectory coverage\test-results and LogFileName mstest-coverage-run.trx (283 to 284), post-processes the Cobertura in place (382 to 384), asserts the 80 percent line and 75 percent branch floors (386 to 387), prints the one-line first-party summary (388), writes the JaCoCo package projection beside the Cobertura as coverage\coverage.cobertura.jacoco.xml (393 to 401), writes coverage\test-results\mstest-coverage-run.summary.txt (417 to 425), retains the raw document because it lies directly in the coverage directory (427 to 431), and is guarded for dot-sourcing at 437. Dot-sourcing it sets strict mode and `$ErrorActionPreference = 'Stop'` (271 to 272).
13. scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1 dot-sources the ClosureFilter, PackageRate, Threshold, FirstParty and Projection part files at lines 2 to 6 and declares `ConvertTo-KoverageCoberturaXml` at 407. The Projection part file declares `ConvertTo-JacocoPackageProjection` (14 to 81: one `package` element per source package with exactly two counters, LINE then BRANCH, attributes `missed` and `covered`), `Assert-JacocoProjectionReconciliation` (83 to 146) and `Test-RawCoverageDocumentRetained` (148 to 197). The FirstParty part file declares `Get-CoberturaFirstPartyCoverageReport` (123 to 162), whose output line has the shape `First-party coverage: lines <c>/<v> (<p>%), branches <c>/<v> (<p>%)`. The Threshold part file throws `Cobertura line coverage <p>% is below the required 80% threshold.` and `Cobertura branch coverage <p>% is below the required 75% threshold.`. scripts/vscode/Invoke-MSTest.TrxSummary.ps1 declares `Get-TrxRunSummary` (12 to 101) and `Format-TrxRunSummary` (103 to 150), whose five rendered lines begin `Test run outcome:`, `Total `, `Skipped `, `Figures reported verbatim by the test platform:` and `Failed tests:`.
14. scripts/vscode/Invoke-Restore.ps1 runs `msbuild <sln> /t:Restore /p:Configuration=Debug "/p:Platform=Any CPU" /p:RestorePackagesConfig=true /m` with parameters `SolutionPath`, `Configuration`, `Platform`. scripts/vscode/Install-RepoDotNetSdk.ps1 installs SDK 8.0.205 into .dotnet-sdk at the repository root (line 36), which global.json names in its `paths` array; dotnet-tools.json at the repository root pins csharpier 1.2.6 (commands `format` and `check`; a clean `check .` prints `Checked N files in Xms.` and `format .` prints `Formatted N files in Xms.`, N being the processed count in both).
15. .csharpierignore (18 lines) excludes every evidence tree, cobertura XML, coverage, coveragexml and trx documents, csproj, props and targets files, and every packages.config and app.config; CSharpier 1.2.6 processes C# source and XML files. .gitignore ignores every test-results directory (line 39), coverage and coveragexml documents (140 to 141), everything under the coverage directory except its gitkeep marker (144 to 145) and the packages directory (190); a trx document is not ignored by any pattern, which is why every TRX in this plan is written under coverage\.
16. The four UtilitiesCS.Test classes that stall a local vstest run on this workstation (observed 2026-09-04, excluded by the issue #781 and #900 plans) are `HelperClasses.ShellUtilities_Tests`, `HelperClasses.ShellUtilitiesStatic_Tests`, `HelperClasses.SysImageListHelperTests` and `EmailIntelligence.OSBrowser_Tests`; `DictionaryExtensions_Tests.TryAddValuesAsync_UpdatesExistingValue` is a known intermittent failure under parallel coverage runs (issue #780). Neither QuickFiler.Test nor UtilitiesCS.Test carries a `LiveOutlook` test category (grep over both trees: zero hits), so the two full-assembly parallel-suite runs need no category filter.
17. The #900 precedent run recorded exactly the #906 failure: its final coverage iteration 1 failed on `FileInfoWrapper_Tests.OpenRead_ShouldReturnReadableStreamForWrappedFile` with the solution file held by a resident MSBuild node-reuse worker left by the plan's own multi-process rebuilds (docs/features/active/breadcrumb-thread-affinity-tests-assume-taskrun-distinct-thread-900/evidence/qa-gates/p5-t5-mstest-coverage.2026-09-17T02-35.md, loop-context section).
18. The feature issue document line 12 is `- Work Mode: full-bug`. The spec's `## Acceptance Criteria` holds nineteen lines beginning `- [ ] AC1. ` through `- [ ] AC19. ` (lines 236 to 254), each on one line; the check-off edit changes only `- [ ] ACn.` to `- [x] ACn.`. No user-story document exists. The spec's header line 9 declares full-bug and names the spec the sole AC source.
19. .claude/hooks/validate-planner-output.ps1 requires every task's opening line to carry a slash- or backslash-bearing path token (line 95), the phase heading form with an em dash (238), sequential task ids per phase (299 to 302), a policy-read and a baseline task in Phase 0 (325 to 330) and QA vocabulary in the final phase (339).
20. .claude/rules/plan-acceptance-gates.md: G8 reports a `git diff` with no ref operand; G8b reports a name-listing diff with neither a `git add` nor a `git status --porcelain` companion in the same task; G7 reads a fixed six-entry write-mode register that does not include CSharpier, so the csharpier tasks carry their own before-and-after tree observation rather than relying on the exit code.
21. FluentAssertions failure phrases relied on by the negative controls: `Be(0)` on an integer prints `to be 0, but found 1`; `BeNull()` on a reference prints `to be <null>` followed by `but found` and the found object's type name; `BeFalse(reason)` and `NotBe(value, reason)` append `because <reason>`; `BeSameAs(expected)` prints `to refer to`. The phrase `to contain` was observed verbatim in the #900 P3-T1 artifact for `Contain`, which confirms the family's message shape on this FluentAssertions version.
22. .claude/hooks/enforce-orchestration-preimplementation-gate.ps1 reads its checkpoint from artifacts/orchestration/orchestrator-state.json (line 31) and its readiness predicate requires the string properties `issue-num` and `feature-folder` (235 to 236), `route_id` or, when absent, `path_selected` (237 to 239), and a truthy `lifecycle_ready` (242 to 243); an absent file or a missing property denies with the reason at 429. The helpers file's issue #539 exemption admits only `git add` and `git commit` segments whose every operand lies under docs/features/epics/, docs/features/parallel/, docs/features/active/, docs/features/potential/ or artifacts/orchestration/, models only `-m` and `--message` forms, requires at least one pathspec operand, and denies any line containing `$`, backtick, `>` or `<`.

## Decisions

- **D-1 Helper contract is identical to the removed private helper.** `DedicatedWorkerThread.Run(Action)` carries the body of `RunOnDedicatedWorkerThread` unchanged (no null-argument guard is added, because the spec requires the identical body and AC7 requires no assertion), the removed remark verbatim, and one added sentence stating that the helper asserts nothing itself. Every local name is pinned (`captured`, `thread`, `error`) so count gates cannot drift.
- **D-2 Part2 partial carries no `[TestClass]`.** The repository convention for continuation partials (fact 4) puts the attribute on the primary declaration only; the attribute applies to the whole type. The primary file keeps `[TestClass]` and gains `partial`.
- **D-3 Direct runs use the root runsettings file.** Spec AC16 names the root file; its Workers and Scope equal the CLI twin's. The collector it declares is recorded, not gated (Execution Conventions).
- **D-4 Coverage route is selected by a recorded observation.** `COVERAGE-ROUTE: RUNNER` runs scripts/vscode/Invoke-MSTestWithCoverage.ps1 verbatim (CLAUDE.md step 4). `COVERAGE-ROUTE: DIRECT` runs the runner's inner collector invocation directly, built from the same functions (`ConvertTo-DerivedCoverageSettingsXml`, the same assembly discovery filter, the CLI runsettings, the isolation switch, the LiveOutlook category filter) with the four-class exclusion appended, and post-processes with the runner's own helpers, so both routes yield the same three committed forms: the JaCoCo package projection, the one-line first-party summary and the TRX-derived summary. Under either route the raw Cobertura and the TRX stay under coverage\ and are never copied into the feature folder.
- **D-5 The fix is committed before the controls (P2-T10).** Each revert is then anchored: `git checkout -- <file>` restores exactly the committed fixed state, `git diff --exit-code HEAD -- <file>` exits 0 and the SHA-256 equals `FIX-HASH-<n>:`.
- **D-6 Predicted failing assertion per control.** M1 (P3-T1): `Dispatch` runs the action inline on the worker, `captured` is null, and `executions.Should().Be(0)` fails with a message containing `to be 0, but found 1`. M2 (P3-T3): the restored context-reference guard throws on the worker (its ambient context is null and the captured context is not), so `captured.Should().BeNull(...)` fails with a message containing `InvalidOperationException` and `but found`. M3 (P3-T5): `action()` runs on the caller before the thread starts, so each of the four in-thread preconditions fails with a message containing `dedicated worker thread must not be` and the thread is never started. M4 (P3-T7): the mock returns a second stream, so `stream.Should().BeSameAs(sentinel)` fails with a message containing `to refer to`. Any other failing assertion, or a passing control, is `MUTATION PREDICTION MISMATCH`: stop and report the message text; do not adjust the test.
- **D-7 Coverage comparison (AC18).** The gated quantities are per-package: for the packages named `UtilitiesCS` and `QuickFiler` in the two JaCoCo projections, the final LINE rate and BRANCH rate, each computed as covered divided by covered plus missed, must be no lower than the baseline rate. The repository-wide first-party totals are recorded with a comparability note only, because the collector's cross-assembly merge is order-sensitive. If a package rate reads lower, the final coverage command is re-run once, identically, as a second measurement (recorded as such); if it still reads lower, `AC18: NOT MET` is recorded and the run stops with the two projections quoted. No test is retried by this rule; only the measurement is repeated.
- **D-8 Analyzer and nullable gates.** Each rebuild must exit 0 (AC17), print zero `Skipping target "CoreCompile"` lines (the spec's non-vacuity clause), echo at least one compiler line for each of QuickFiler.Test.dll and UtilitiesCS.Test.dll, and carry no warning or error line naming any Write Set `.cs` file. Warning counts are recorded against the Phase 0 values as an observation.
- **D-9 Follow-ups are handed off, not filed, by the executor.** The executor has no MCP surface; P4-T13 writes the four potential-entry bodies from the spec's Rollout list and the orchestrator files them.
- **D-10 No production change and no UNAFFECTED-site change.** P4-T11 requires the anchored name-listing diff of QuickFiler.Test to list exactly the four QuickFiler.Test Write Set paths and the anchored diff of QuickFiler/, UtilitiesCS/, the two runsettings files and config/ to be empty.
- **D-11 AC15 footprint.** `THIS-ITEM-FOOTPRINT:` is the anchored diff plus porcelain union minus `INHERITED-CLAUSE-A:` minus the Clause B prefix, with both subtractions listed in the artifact; it must equal the six code files plus paths under FEATURE/.
- **D-12 Known #780 flake.** When a repository-wide coverage run's only failed test is `DictionaryExtensions_Tests.TryAddValuesAsync_UpdatesExistingValue`, the identical command is re-run once and both runs are recorded; any other failure stops the task.
- **D-13 Fail-before evidence.** No committed test can deterministically reproduce either defect without mutating process-global thread-pool state or spawning a handle-holding external process, both prohibited; P1-T1 writes the exception dossier and the four controls are the deterministic observed-failing evidence.
- **D-14 Rooted fixture literal.** The three metadata tests read the rooted literal C:\Repo\fixture.sln through the private constant `FixturePath`; the assertions mirror `file.Exists` and never read `Length`, so the outcome is identical whether or not the path exists on a machine.
- **D-15 Wrap-tolerant tokens.** CSharpier may break a `.Should()` chain across lines, so gates use `.BeNull(`, `.NotBe(`, `owner.CheckAccess()` and the reason-text literals rather than a whole chain; the reason literals are pinned to sit on one source line each.
- **D-16 Node reuse off.** Every msbuild invocation carries /nodeReuse:false (Execution Conventions) so that the node-reuse workers which produced the #906 failure in the #900 run (fact 17) do not exist when the parallel-suite runs execute. The switch is additive to the canonical commands and changes no diagnostic.
- **D-17 Baseline runs are recorded, not gated on exit 0.** P0-T10 and P0-T11 record failed-test sets so the final runs can report `NEWLY-FAILING:`; a non-empty baseline failed set other than the #780 flake is reported as `BASELINE NOT GREEN` and stops the run, because AC16 and AC17 cannot then be met by a test-only change.

## Command Reference

Each block is a payload in the sense of the command-channel convention: one complete statement per line (a braced block on one line is one statement), no line continuations, no single-quote characters, no comment lines. Uppercase tokens `WORKTREE`, `PATH`, `TASKID`, `TESTPROJECT`, `PRODUCTION`, `GATEARGS`, `ASSEMBLY`, `FILTERARG`, `NAMES`, `STAGE` and `RAW` are substituted by the executor as each task states. Names for the `NAMES` token: `NAMES-AFFINITY` is the seven quoted method names of fact 1 (`"InitializeBreadcrumbPipeline_ConstructedInsideDispatcherOperation_SucceedsUnderDifferentAmbientContext", "InitializeBreadcrumbPipeline_OwningThreadNullAmbientContext_DoesNotThrow", "InitializeBreadcrumbPipeline_OwningThreadDifferentPlainContext_DoesNotThrow", "ConfigureBreadcrumbDropDown_OwningThreadInsideDispatcherOperation_DoesNotThrow", "InitializeBreadcrumbPipeline_WorkerThread_ThrowsBoundaryDiagnostic", "ConfigureBreadcrumbDropDown_WorkerThread_ThrowsBoundaryDiagnostic", "InitializeBreadcrumbPipeline_NullOwningDispatcher_DoesNotThrow"`); `NAMES-QF` is `NAMES-AFFINITY` plus `"Dispatcher_OwnerOnlyWorker_ReportsWithoutRunningAction"`; `NAMES-FOUR` is the last three names of `NAMES-AFFINITY` plus `"Dispatcher_OwnerOnlyWorker_ReportsWithoutRunningAction"`; `NAMES-FIW` is the eight quoted method names of fact 5 (`"Constructor_WhenFileInfoIsNull_ThrowsArgumentNullException", "Properties_ShouldMirrorWrappedFileInfo", "ExplicitDirectoryCast_ShouldReturnWrappedContainingDirectory", "OpenRead_ShouldReturnReadableStreamForWrappedFile", "ToString_ShouldDelegateToWrappedFileInfo", "PropertyDelegates_ShouldMirrorMockedIFileInfo", "StreamAndCopyMethods_ShouldDelegateToWrappedIFileInfo", "AccessControlAndLifecycleMethods_ShouldDelegateToWrappedIFileInfo"`); `NAMES-NONE` is the empty list `@()` written as an empty `NAMES` substitution.

Filters (each is substituted for `FILTERARG` as one double-quoted argument beginning with the test-case-filter switch): `FILTER-FOUR` is `FullyQualifiedName~InitializeBreadcrumbPipeline_WorkerThread_ThrowsBoundaryDiagnostic|FullyQualifiedName~ConfigureBreadcrumbDropDown_WorkerThread_ThrowsBoundaryDiagnostic|FullyQualifiedName~InitializeBreadcrumbPipeline_NullOwningDispatcher_DoesNotThrow|FullyQualifiedName~Dispatcher_OwnerOnlyWorker_ReportsWithoutRunningAction` (expected total 4); `FILTER-DISPATCHER` is `FullyQualifiedName~Dispatcher_OwnerOnlyWorker_ReportsWithoutRunningAction` (total 1); `FILTER-NULLOWNER` is `FullyQualifiedName~InitializeBreadcrumbPipeline_NullOwningDispatcher_DoesNotThrow` (total 1); `FILTER-OPENREAD` is `FullyQualifiedName~OpenRead_ShouldReturnReadableStreamForWrappedFile` (total 1); `FILTER-AFFINITY-CLASS` is `FullyQualifiedName~ItemViewerBreadcrumbThreadAffinityTests` (total 7); `FILTER-FIW-CLASS` is `FullyQualifiedName~FileInfoWrapper_Tests` (total 8); `FILTER-STALL` is `FullyQualifiedName~HelperClasses.ShellUtilities_Tests|FullyQualifiedName~HelperClasses.ShellUtilitiesStatic_Tests|FullyQualifiedName~HelperClasses.SysImageListHelperTests|FullyQualifiedName~EmailIntelligence.OSBrowser_Tests`; `FILTER-UCS-EXCLUDE` is `FullyQualifiedName!~HelperClasses.ShellUtilities_Tests&FullyQualifiedName!~HelperClasses.ShellUtilitiesStatic_Tests&FullyQualifiedName!~HelperClasses.SysImageListHelperTests&FullyQualifiedName!~EmailIntelligence.OSBrowser_Tests`. `UCS-FILTERARG` (fixed by P0-T9) is empty under `STALL-PROBE: CLEAR` and is `FILTER-UCS-EXCLUDE` under `REPRODUCES`. A run whose `total` differs from the expected value is a failure of that task, not a pass: vstest.console.exe reports a zero-match filter without a non-zero exit.

Assemblies: `ASSEMBLY-QF` is QuickFiler.Test\bin\Debug\QuickFiler.Test.dll; `ASSEMBLY-UCS` is UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll.

**CMD-CENSUS** (token census over one file; `PATH` substituted; prints one `TOKEN <token> = <count>` line per token, `LINES = <n>` and `SHA256 = <hash>`):

    Set-Location -LiteralPath "WORKTREE"
    $path = "PATH"
    $content = Get-Content -LiteralPath $path -Raw -Encoding UTF8
    $tokens = @("Task.Run(", ".GetAwaiter()", "DedicatedWorkerThread.Run(", "RunOnDedicatedWorkerThread", "ClearViewerDispatcher(", "partial class", "[TestMethod]", "action();", "new Thread(", "IsBackground = true", "thread.Join();", "Join(", "Join()", "using System.Reflection;", "using QuickFiler.Test.TestSupport;", "namespace QuickFiler.Test.TestSupport", "internal static class DedicatedWorkerThread", "internal static Exception Run(Action action)", ".Should()", "dedicated worker thread must not be", "distinct from every live thread by construction", "unconditionally", "owner.CheckAccess()", ".NotBe(", "ownerThreadId", ".BeNull(", "executions.Should().Be(0)", "cannot marshal", "GetSolutionFile", "TaskMaster.sln", "AppDomain", "File.Exists(", "File.Create", "File.WriteAll", "File.Delete", "Path.GetTemp", "FileMode.", "FileMode.Open", "FileAccess.", "FileAccess.Read", "FileShare.ReadWrite", "Assembly.Location", "FixturePath", "using var sentinel = new FileStream(", "BeSameAs(sentinel)", ".Returns(decoy)", "wrapper.OpenRead()", "stream.CanRead.Should().BeTrue()", "stream.Length.Should().BeGreaterThan(0)", "return true;", "_ownerThreadId.HasValue", "System.Threading.SynchronizationContext.Current", "Include=""Viewers\ItemViewerBreadcrumbThreadAffinityTests.cs""", "Include=""Viewers\ItemViewerBreadcrumbThreadAffinityTests.Part2.cs""", "Include=""TestSupport\DedicatedWorkerThread.cs""", "Thread.Sleep", "Task.Delay", "[Timeout", "DoNotParallelize", "Retry")
    foreach ($t in $tokens) { Write-Output ("TOKEN " + $t + " = " + [regex]::Matches($content, [regex]::Escape($t)).Count) }
    Write-Output ("LINES = " + @(Get-Content -LiteralPath $path).Count)
    Write-Output ("SHA256 = " + (Get-FileHash -Algorithm SHA256 -LiteralPath $path).Hash)

The three project-file tokens are referred to in prose as INCLUDE-AFFINITY, INCLUDE-PART2 and INCLUDE-HELPER (the doubled quotes are PowerShell's escape for a quote inside a double-quoted literal).

**CMD-BUILD** (project build after a source edit; `TESTPROJECT` is `QuickFiler.Test` or `UtilitiesCS.Test`, `PRODUCTION` is `QuickFiler` or `UtilitiesCS`, `TASKID` is the lower-case task id):

    Set-Location -LiteralPath "WORKTREE"
    $vswhere = Join-Path ${env:ProgramFiles(x86)} "Microsoft Visual Studio\Installer\vswhere.exe"
    $msbuild = & $vswhere -latest -requires Microsoft.Component.MSBuild -find "MSBuild\**\Bin\MSBuild.exe" | Select-Object -First 1
    $dll = "TESTPROJECT\bin\Debug\TESTPROJECT.dll"
    $before = if (Test-Path -LiteralPath $dll) { (Get-Item -LiteralPath $dll).LastWriteTimeUtc } else { [datetime]::MinValue }
    New-Item -ItemType Directory -Path "coverage\logs" -Force | Out-Null
    $global:LASTEXITCODE = 0
    & $msbuild "TESTPROJECT\TESTPROJECT.csproj" /t:Build /m /nodeReuse:false /p:Configuration=Debug /p:Platform=AnyCPU "/flp:LogFile=coverage\logs\TASKID.msbuild.log;Verbosity=normal"
    Write-Output ("MSBUILD_EXIT_CODE: " + $LASTEXITCODE)
    $log = Get-Content -LiteralPath "coverage\logs\TASKID.msbuild.log" -Raw -Encoding UTF8
    Write-Output ("CSC_OUT_LINES: " + [regex]::Matches($log, [regex]::Escape("/out:obj\Debug\TESTPROJECT.dll")).Count)
    Write-Output ("PROD_CSC_OUT_LINES: " + [regex]::Matches($log, [regex]::Escape("/out:obj\Debug\PRODUCTION.dll")).Count)
    Write-Output ("ZERO_ERRORS_LINES: " + [regex]::Matches($log, [regex]::Escape(" 0 Error(s)")).Count)
    Write-Output ("DLL_ADVANCED: " + ((Get-Item -LiteralPath $dll).LastWriteTimeUtc -gt $before))

`ZERO_ERRORS_LINES` counts the literal with its leading space because `0 Error(s)` is a substring of `10 Error(s)`. `CMD-BUILD-QF` denotes this block with `QuickFiler.Test` and `QuickFiler`; `CMD-BUILD-UCS` denotes it with `UtilitiesCS.Test` and `UtilitiesCS`.

**CMD-REBUILD** (solution rebuild gate; `GATEARGS` is either `/p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` (analyzer gate) or the single TreatWarningsAsErrors property switch (nullable gate); `TASKID` substituted):

    Set-Location -LiteralPath "WORKTREE"
    $vswhere = Join-Path ${env:ProgramFiles(x86)} "Microsoft Visual Studio\Installer\vswhere.exe"
    $msbuild = & $vswhere -latest -requires Microsoft.Component.MSBuild -find "MSBuild\**\Bin\MSBuild.exe" | Select-Object -First 1
    New-Item -ItemType Directory -Path "coverage\logs" -Force | Out-Null
    $global:LASTEXITCODE = 0
    & $msbuild TaskMaster.sln /t:Rebuild /m /nodeReuse:false /p:Configuration=Debug "/p:Platform=Any CPU" GATEARGS "/flp:LogFile=coverage\logs\TASKID.msbuild.log;Verbosity=normal"
    Write-Output ("MSBUILD_EXIT_CODE: " + $LASTEXITCODE)
    $log = Get-Content -LiteralPath "coverage\logs\TASKID.msbuild.log" -Raw -Encoding UTF8
    Write-Output ("SKIP_CORECOMPILE_LINES: " + [regex]::Matches($log, [regex]::Escape("Skipping target ""CoreCompile""")).Count)
    Write-Output ("QF_TEST_CSC_OUT_LINES: " + [regex]::Matches($log, [regex]::Escape("/out:obj\Debug\QuickFiler.Test.dll")).Count)
    Write-Output ("UCS_TEST_CSC_OUT_LINES: " + [regex]::Matches($log, [regex]::Escape("/out:obj\Debug\UtilitiesCS.Test.dll")).Count)
    Write-Output ("ZERO_ERRORS_LINES: " + [regex]::Matches($log, [regex]::Escape(" 0 Error(s)")).Count)
    Write-Output ("WARNINGS: " + [regex]::Match($log, "(\d+) Warning\(s\)").Groups[1].Value)
    Write-Output ("ERRORS: " + [regex]::Match($log, "(\d+) Error\(s\)").Groups[1].Value)
    Write-Output ("WRITESET_DIAGNOSTIC_LINES: " + @(Get-Content -LiteralPath "coverage\logs\TASKID.msbuild.log" | Where-Object { ($_ -like "*ItemViewerBreadcrumbThreadAffinityTests*" -or $_ -like "*DedicatedWorkerThread.cs*" -or $_ -like "*BreadcrumbPopupBoundaryCoverageTests.cs*" -or $_ -like "*FileInfoWrapper_Tests.cs*") -and ($_ -like "*warning *" -or $_ -like "*error *") }).Count)

**CMD-VSTEST** (one assembly under the root runsettings with the isolation switch; `ASSEMBLY`, `FILTERARG` (may be empty), `NAMES`, `TASKID` substituted; the results directory is private to the task):

    Set-Location -LiteralPath "WORKTREE"
    $vswhere = Join-Path ${env:ProgramFiles(x86)} "Microsoft Visual Studio\Installer\vswhere.exe"
    $vstest = & $vswhere -latest -products * -find "Common7\IDE\Extensions\TestPlatform\vstest.console.exe" | Select-Object -First 1
    $assembly = (Resolve-Path -LiteralPath "ASSEMBLY").Path
    $settings = (Resolve-Path -LiteralPath "TaskMaster.runsettings").Path
    $results = Join-Path (Get-Location).Path "coverage\test-results\931\TASKID"
    if (Test-Path -LiteralPath $results) { Remove-Item -LiteralPath $results -Recurse -Force }
    New-Item -ItemType Directory -Path "coverage\logs" -Force | Out-Null
    Write-Output ("RUNSETTINGS-HASH-NOW: " + (Get-FileHash -Algorithm SHA256 -LiteralPath "TaskMaster.runsettings").Hash)
    $names = @(NAMES)
    $global:LASTEXITCODE = 0
    & $vstest $assembly "/Settings:$settings" /InIsolation FILTERARG "/ResultsDirectory:$results" "/Logger:trx;LogFileName=TASKID.trx" "/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None" 2>&1 | Tee-Object -FilePath "coverage\logs\TASKID.vstest.log"
    Write-Output ("VSTEST_EXIT_CODE: " + $LASTEXITCODE)
    $trxPath = Join-Path $results "TASKID.trx"
    Write-Output ("TRX_PRESENT: " + (Test-Path -LiteralPath $trxPath))
    Write-Output ("SEQUENCE_FILES: " + @(Get-ChildItem -LiteralPath $results -Recurse -Filter "Sequence_*.xml" -ErrorAction SilentlyContinue).Count)
    Write-Output ("COLLECTOR_LINES: " + @(Get-Content -LiteralPath "coverage\logs\TASKID.vstest.log" | Where-Object { $_ -like "*Code Coverage*" }).Count)
    if (-not (Test-Path -LiteralPath $trxPath)) { Write-Output "TRX ABSENT: the run aborted before writing its result document"; exit 3 }
    [xml]$trx = Get-Content -LiteralPath $trxPath -Raw -Encoding UTF8
    $ns = New-Object System.Xml.XmlNamespaceManager($trx.NameTable)
    $ns.AddNamespace("t", "http://microsoft.com/schemas/VisualStudio/TeamTest/2010")
    $counters = $trx.SelectSingleNode("//t:ResultSummary/t:Counters", $ns)
    Write-Output ("COUNTERS total=" + $counters.GetAttribute("total") + " executed=" + $counters.GetAttribute("executed") + " passed=" + $counters.GetAttribute("passed") + " failed=" + $counters.GetAttribute("failed"))
    $all = @($trx.SelectNodes("//t:UnitTestResult", $ns))
    Write-Output ("RESULT_COUNT: " + $all.Count)
    foreach ($r in $all) { if ($names -contains $r.GetAttribute("testName")) { Write-Output ("RESULT " + $r.GetAttribute("testName") + " = " + $r.GetAttribute("outcome")) } }
    foreach ($r in $all) { if ($r.GetAttribute("outcome") -eq "Failed") { Write-Output ("MESSAGE " + $r.GetAttribute("testName") + " :: " + $r.SelectSingleNode("t:Output/t:ErrorInfo/t:Message", $ns).InnerText) } }

The failed-result filter is applied in PowerShell rather than in XPath so no payload needs an embedded quote. The `Command:` field records `vstest.console.exe <assembly> "/Settings:TaskMaster.runsettings" /InIsolation <filter> "/ResultsDirectory:coverage\test-results\931\<task-id>" "/Logger:trx;LogFileName=<task-id>.trx" "/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None"` with the note `resolved through vswhere`.

**CMD-COVERAGE-RUNNER** (CLAUDE.md step 4 route; `STAGE` is `baseline` or `final`; used when `COVERAGE-ROUTE: RUNNER`):

    Set-Location -LiteralPath "WORKTREE"
    New-Item -ItemType Directory -Path "coverage\logs" -Force | Out-Null
    $script = Join-Path (Get-Location).Path "scripts\vscode\Invoke-MSTestWithCoverage.ps1"
    $global:LASTEXITCODE = 0
    & pwsh -NoProfile -File $script 2>&1 | Tee-Object -FilePath "coverage\logs\STAGE-931.runner.log"
    Write-Output ("RUNNER_EXIT_CODE: " + $LASTEXITCODE)
    $log = Get-Content -LiteralPath "coverage\logs\STAGE-931.runner.log" -Raw -Encoding UTF8
    Write-Output ("DISCOVERED_LINE: " + [regex]::Match($log, "Discovered \d+ test assemblies\.").Value)
    Write-Output ("FIRST_PARTY_LINE: " + [regex]::Match($log, "First-party coverage: [^\r\n]*").Value)
    Write-Output ("THRESHOLD_MESSAGE: " + [regex]::Match($log, "Cobertura (line|branch) coverage [^\r\n]*threshold\.").Value)
    Write-Output ("COLLECT_FAILURE_MESSAGE: " + [regex]::Match($log, "MSTest with coverage failed with exit code \d+").Value)
    Write-Output ("DOCUMENT_PRESENT: " + (Test-Path -LiteralPath "coverage\coverage.cobertura.xml"))
    Write-Output ("PROJECTION_PRESENT: " + (Test-Path -LiteralPath "coverage\coverage.cobertura.jacoco.xml"))
    Write-Output ("SUMMARY_PRESENT: " + (Test-Path -LiteralPath "coverage\test-results\mstest-coverage-run.summary.txt"))
    Write-Output ("TRX_PRESENT: " + (Test-Path -LiteralPath "coverage\test-results\mstest-coverage-run.trx"))
    if (Test-Path -LiteralPath "coverage\coverage.cobertura.xml") { Copy-Item -LiteralPath "coverage\coverage.cobertura.xml" -Destination "coverage\STAGE-931.cobertura.xml" -Force }
    if (Test-Path -LiteralPath "coverage\test-results\mstest-coverage-run.trx") { Copy-Item -LiteralPath "coverage\test-results\mstest-coverage-run.trx" -Destination "coverage\STAGE-931.trx" -Force }
    if (Test-Path -LiteralPath "coverage\coverage.cobertura.jacoco.xml") { Write-Output ("RUNNER_PROJECTION_SHA256: " + (Get-FileHash -Algorithm SHA256 -LiteralPath "coverage\coverage.cobertura.jacoco.xml").Hash) }

The runner's own log line naming the resolved vstest path and its `Coverage output:` line carry absolute paths and stay in the git-ignored log; only the named `_LINE`, `_MESSAGE` and `_PRESENT` values are transcribed. The `Command:` field records `pwsh -NoProfile -File scripts/vscode/Invoke-MSTestWithCoverage.ps1` (absolute script path resolved at run time).

**CMD-COVERAGE-DIRECT** (the runner's inner invocation issued directly with the four-class exclusion; used when `COVERAGE-ROUTE: DIRECT`; `STAGE` substituted):

    Set-Location -LiteralPath "WORKTREE"
    . (Join-Path (Get-Location).Path "scripts\vscode\Invoke-MSTestWithCoverage.ps1")
    $ErrorActionPreference = "Continue"
    $repo = (Get-Location).Path
    New-Item -ItemType Directory -Path "coverage\logs" -Force | Out-Null
    $canonical = Get-Content -LiteralPath "coverage.config" -Raw -Encoding UTF8
    $derived = ConvertTo-DerivedCoverageSettingsXml -CanonicalSettingsXml $canonical
    $effective = Join-Path $repo "coverage\effective-coverage-931.config"
    Set-Content -LiteralPath $effective -Value $derived -Encoding UTF8 -NoNewline
    $vswhere = Join-Path ${env:ProgramFiles(x86)} "Microsoft Visual Studio\Installer\vswhere.exe"
    $vstest = & $vswhere -latest -products * -find "Common7\IDE\Extensions\TestPlatform\vstest.console.exe" | Select-Object -First 1
    $rootLen = $repo.TrimEnd([char]92).Length
    $asm = @(Get-ChildItem -Path $repo -Recurse -Filter "*.Test.dll" | Where-Object { $_.FullName -like "*\bin\Debug\*" -and $_.FullName -notlike "*\obj\*" -and $_.FullName -notlike "*\ref\*" -and $_.FullName.Substring($rootLen) -notlike "\.claude\*" } | Select-Object -ExpandProperty FullName)
    $filter = "TestCategory!=LiveOutlook&FullyQualifiedName!~HelperClasses.ShellUtilities_Tests&FullyQualifiedName!~HelperClasses.ShellUtilitiesStatic_Tests&FullyQualifiedName!~HelperClasses.SysImageListHelperTests&FullyQualifiedName!~EmailIntelligence.OSBrowser_Tests"
    $output = Join-Path $repo "coverage\STAGE-931.cobertura.xml"
    $settings = Join-Path $repo "scripts\vscode\TaskMaster.cli.runsettings"
    $results = Join-Path $repo "coverage\test-results\931\STAGE"
    if (Test-Path -LiteralPath $results) { Remove-Item -LiteralPath $results -Recurse -Force }
    $global:LASTEXITCODE = 0
    & dotnet-coverage collect --output $output --output-format cobertura --settings $effective -- $vstest @asm "/Settings:$settings" /InIsolation "/TestCaseFilter:$filter" "/ResultsDirectory:$results" "/Logger:trx;LogFileName=STAGE-931.trx" "/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None" 2>&1 | Tee-Object -FilePath "coverage\logs\STAGE-931.collect.log"
    Write-Output ("COLLECT_EXIT_CODE: " + $LASTEXITCODE)
    Write-Output ("ASSEMBLY_COUNT: " + $asm.Count)
    $asm | ForEach-Object { Write-Output ("ASSEMBLY: " + $_.Substring($rootLen)) }
    Write-Output ("SEQUENCE_FILES: " + @(Get-ChildItem -LiteralPath $results -Recurse -Filter "Sequence_*.xml" -ErrorAction SilentlyContinue).Count)
    if (Test-Path -LiteralPath (Join-Path $results "STAGE-931.trx")) { Copy-Item -LiteralPath (Join-Path $results "STAGE-931.trx") -Destination "coverage\STAGE-931.trx" -Force }

The `ASSEMBLY:` lines print the path relative to the worktree root (the leading backslash is the first character). The discovery filter uses `-like` wildcards and `[char]92` rather than a regex ending in a backslash, so no payload line ends a literal with a backslash before its closing quote. The `Command:` field records `dotnet-coverage collect --output coverage\<stage>-931.cobertura.xml --output-format cobertura --settings coverage\effective-coverage-931.config -- vstest.console.exe <N test assemblies> "/Settings:scripts\vscode\TaskMaster.cli.runsettings" /InIsolation "/TestCaseFilter:<filter>" "/ResultsDirectory:coverage\test-results\931\<stage>" "/Logger:trx;LogFileName=<stage>-931.trx" "/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None"`.

**CMD-COVERAGE-POST** (post-process, project and summarise one stage's documents with the runner's own functions; `STAGE` substituted; `RAW` is `True` under the DIRECT route, where the document is raw collector output, and `False` under the RUNNER route, where the runner already post-processed it in place):

    Set-Location -LiteralPath "WORKTREE"
    . (Join-Path (Get-Location).Path "scripts\vscode\Invoke-MSTestWithCoverage.Helpers.ps1")
    . (Join-Path (Get-Location).Path "scripts\vscode\Invoke-MSTest.TrxSummary.ps1")
    $ErrorActionPreference = "Continue"
    $repo = (Get-Location).Path
    $doc = Get-Content -LiteralPath "coverage\STAGE-931.cobertura.xml" -Raw -Encoding UTF8
    if ("RAW" -eq "True") { $doc = ConvertTo-KoverageCoberturaXml -XmlContent $doc -RepoRoot $repo; Set-Content -LiteralPath "coverage\STAGE-931.cobertura.xml" -Value $doc -Encoding UTF8 -NoNewline }
    try { Assert-CoberturaLineCoverageThreshold -CoberturaXml $doc; Write-Output "LINE-FLOOR: MET" } catch { Write-Output ("LINE-FLOOR: NOT MET " + $_.Exception.Message) }
    try { Assert-CoberturaBranchCoverageThreshold -CoberturaXml $doc; Write-Output "BRANCH-FLOOR: MET" } catch { Write-Output ("BRANCH-FLOOR: NOT MET " + $_.Exception.Message) }
    Write-Output (Get-CoberturaFirstPartyCoverageReport -CoberturaXml $doc)
    [xml]$xml = $doc
    $projection = ConvertTo-JacocoPackageProjection -XmlDocument $xml
    Assert-JacocoProjectionReconciliation -XmlDocument $xml -ProjectionXml $projection
    Set-Content -LiteralPath "coverage\STAGE-931.jacoco.xml" -Value $projection -Encoding UTF8
    Write-Output "PROJECTION-BEGIN"
    Write-Output $projection
    Write-Output "PROJECTION-END"
    $summary = Get-TrxRunSummary -TrxContent (Get-Content -LiteralPath "coverage\STAGE-931.trx" -Raw -Encoding UTF8)
    Write-Output "SUMMARY-BEGIN"
    Write-Output (Format-TrxRunSummary -Summary $summary)
    Write-Output "SUMMARY-END"
    Write-Output ("FAILED-SET: " + (@($summary.FailedTestName) -join ", "))

The projection printed between `PROJECTION-BEGIN` and `PROJECTION-END` and the five summary lines between `SUMMARY-BEGIN` and `SUMMARY-END` are the two committed forms; they carry package names, counters and test names only.

**CMD-PACKAGE-COMPARE** (per-package comparison of the two JaCoCo projections for AC18):

    Set-Location -LiteralPath "WORKTREE"
    [xml]$b = Get-Content -LiteralPath "coverage\baseline-931.jacoco.xml" -Raw -Encoding UTF8
    [xml]$f = Get-Content -LiteralPath "coverage\final-931.jacoco.xml" -Raw -Encoding UTF8
    $q = [char]39
    foreach ($name in @("UtilitiesCS", "QuickFiler")) { foreach ($type in @("LINE", "BRANCH")) { $xp = "/report/package[@name=" + $q + $name + $q + "]/counter[@type=" + $q + $type + $q + "]"; $bc = $b.SelectSingleNode($xp); $fc = $f.SelectSingleNode($xp); if ($null -eq $bc -or $null -eq $fc) { Write-Output ("PACKAGE " + $name + " " + $type + " MISSING baseline=" + ($null -ne $bc) + " final=" + ($null -ne $fc)); continue }; $bCov = [int]$bc.GetAttribute("covered"); $bVal = $bCov + [int]$bc.GetAttribute("missed"); $fCov = [int]$fc.GetAttribute("covered"); $fVal = $fCov + [int]$fc.GetAttribute("missed"); $bRate = if ($bVal -gt 0) { [math]::Round($bCov / $bVal, 6) } else { 0 }; $fRate = if ($fVal -gt 0) { [math]::Round($fCov / $fVal, 6) } else { 0 }; Write-Output ("PACKAGE " + $name + " " + $type + " baseline=" + $bCov + "/" + $bVal + " rate=" + $bRate + " final=" + $fCov + "/" + $fVal + " rate=" + $fRate + " NOT-LOWER=" + ($fRate -ge $bRate)) } }

**CMD-SWEEP** (host-identifier and raw-document sweep over the feature folder; the tokens are derived at run time and never written into an artifact):

    Set-Location -LiteralPath "WORKTREE"
    $folder = "docs\features\active\2026-09-28-tests-depend-on-uncontrolled-environment-931"
    $all = @(Get-ChildItem -LiteralPath $folder -Recurse -File)
    $evidence = @(Get-ChildItem -LiteralPath (Join-Path $folder "evidence") -Recurse -File) + @(Get-Item -LiteralPath (Join-Path $folder "plan.2026-09-28T20-01.md"))
    $account = [regex]::Escape($env:USERNAME)
    $profileLeaf = [regex]::Escape((Split-Path -Leaf $env:USERPROFILE))
    $machine = [regex]::Escape($env:COMPUTERNAME)
    $root = [regex]::Escape((Get-Location).Path)
    Write-Output ("FILES: " + $all.Count)
    Write-Output ("ACCOUNT-TOKEN-MATCHES: " + @($evidence | Select-String -Pattern ("(?i)\b" + $account + "\b")).Count)
    Write-Output ("PROFILE-LEAF-MATCHES: " + @($evidence | Select-String -Pattern ("(?i)\b" + $profileLeaf + "\b")).Count)
    Write-Output ("MACHINE-TOKEN-MATCHES: " + @($evidence | Select-String -Pattern ("(?i)\b" + $machine + "\b")).Count)
    Write-Output ("WORKTREE-ROOT-MATCHES: " + @($all | Select-String -Pattern ("(?i)" + $root)).Count)
    Write-Output ("USERS-PATH-MATCHES: " + @($all | Select-String -Pattern "(?i)[a-z]:[\\/]users[\\/]").Count)
    Write-Output ("RAW-DOCUMENT-FILES: " + @($all | Where-Object { $_.Extension -in @(".trx", ".xml", ".coverage") }).Count)

The account, profile-leaf and machine sweeps run over the evidence tree and this plan (the committed evidence AC19 governs); the two path sweeps and the raw-document sweep run over the whole feature folder, because a drive-rooted user-profile path or a raw document is prohibited wherever it appears. The spec's `Owner:` field is a GitHub handle authored by the maintainer and is outside the evidence tree.

## Token Census Expectations

File aliases: `AFF` is the primary affinity file, `PART2` its continuation, `HELPER` the new helper, `BND` the boundary-coverage file, `FIW` the FileInfoWrapper test file, `CSPROJ` the QuickFiler.Test project file, `DISP` QuickFiler/Viewers/BreadcrumbUiDispatcher.cs, `IVB` QuickFiler/Viewers/ItemViewer.Breadcrumb.cs. `pre` is the pre-edit value P0-T12 records; `post` is the value after Phase 2 (P2-T7) that holds through the end of the run except where a control column says otherwise. Tokens not listed for a file are recorded and not gated.

| File | Token | pre | post | under control |
| --- | --- | --- | --- | --- |
| AFF | `LINES` | 490 | at most 500 (expected 293) | unchanged |
| AFF | `Task.Run(` | 1 | 0 | |
| AFF | `.GetAwaiter()` | 1 | 0 | |
| AFF | `RunOnDedicatedWorkerThread` | 5 | 0 | |
| AFF | `ClearViewerDispatcher(` | 2 | 0 | |
| AFF | `partial class` | 0 | 1 | |
| AFF | `[TestMethod]` | 7 | 4 | |
| AFF | `action();`, `new Thread(`, `IsBackground = true`, `thread.Join();` | 1 each | 0 each | |
| AFF | `using System.Reflection;` | 1 | 0 | |
| AFF | `dedicated worker thread must not be` | 2 | 0 | |
| AFF | `distinct from every live thread by construction` | 1 | 0 | |
| AFF | `DedicatedWorkerThread.Run(`, `using QuickFiler.Test.TestSupport;`, `Thread.Sleep`, `Task.Delay`, `[Timeout`, `DoNotParallelize` | 0 each | 0 each | |
| PART2 | `LINES` | (absent) | at most 500 (expected about 210) | |
| PART2 | `Task.Run(`, `.GetAwaiter()`, `RunOnDedicatedWorkerThread`, `action();`, `Thread.Sleep`, `Task.Delay`, `[Timeout`, `DoNotParallelize`, `Retry` | (absent) | 0 each | |
| PART2 | `DedicatedWorkerThread.Run(` | (absent) | 3 | |
| PART2 | `ClearViewerDispatcher(` | (absent) | 2 | |
| PART2 | `partial class`, `using QuickFiler.Test.TestSupport;`, `using System.Reflection;`, `owner.CheckAccess()`, `.BeNull(`, `unconditionally` | (absent) | 1 each | |
| PART2 | `[TestMethod]` | (absent) | 3 | |
| PART2 | `dedicated worker thread must not be` | (absent) | 3 | |
| HELPER | `LINES` | (absent) | at most 500 (expected about 50) | M3: one more |
| HELPER | `namespace QuickFiler.Test.TestSupport`, `internal static class DedicatedWorkerThread`, `internal static Exception Run(Action action)`, `new Thread(`, `IsBackground = true`, `thread.Join();`, `distinct from every live thread by construction` | (absent) | 1 each | |
| HELPER | `action();` | (absent) | 1 | M3 (P3-T5): 2; after P3-T6: 1 |
| HELPER | `Join(` equals `Join()` | (absent) | equal | |
| HELPER | `.Should()`, `Task.Run(`, `Thread.Sleep`, `Task.Delay`, `[Timeout`, `DoNotParallelize`, `Retry` | (absent) | 0 each | |
| BND | `LINES` | 361 | at most 500 (expected about 385) | |
| BND | `Task.Run(` | 1 | 0 | |
| BND | `.GetAwaiter()` | 4 | 3 | |
| BND | `DedicatedWorkerThread.Run(`, `using QuickFiler.Test.TestSupport;`, `.NotBe(`, `.BeNull(`, `dedicated worker thread must not be` | 0 each | 1 each | |
| BND | `ownerThreadId` | 0 | 2 | |
| BND | `executions.Should().Be(0)`, `cannot marshal`, `partial class` | 1 each | 1 each | |
| BND | `Thread.Sleep`, `Task.Delay`, `[Timeout`, `DoNotParallelize` | 0 each | 0 each | |
| FIW | `LINES` | 359 | at most 500 (expected about 362) | M4: five more |
| FIW | `GetSolutionFile` | 5 | 0 | |
| FIW | `TaskMaster.sln`, `AppDomain`, `File.Exists(` | 1 each | 0 each | |
| FIW | `File.Create`, `File.WriteAll`, `File.Delete`, `Path.GetTemp`, `Thread.Sleep`, `Task.Delay`, `[Timeout`, `DoNotParallelize` | 0 each | 0 each | |
| FIW | `FileMode.` equals `FileMode.Open` | 12 = 12 | 13 = 13 | |
| FIW | `FileAccess.` equals `FileAccess.Read` | 10 = 10 | 11 = 11 | |
| FIW | `FileShare.ReadWrite` | 8 | 9 | M4: 10 |
| FIW | `Assembly.Location` | 6 | 7 | M4 (P3-T7): 8; after P3-T8: 7 |
| FIW | `FixturePath` | 0 | 4 | |
| FIW | `using var sentinel = new FileStream(`, `BeSameAs(sentinel)` | 0 each | 1 each | |
| FIW | `.Returns(decoy)` | 0 | 0 | M4: 1; after P3-T8: 0 |
| FIW | `wrapper.OpenRead()` | 2 | 2 | |
| FIW | `stream.CanRead.Should().BeTrue()`, `stream.Length.Should().BeGreaterThan(0)` | 1 each | 1 each | |
| FIW | `[TestMethod]` | 8 | 8 | |
| CSPROJ | INCLUDE-AFFINITY | 1 | 1 | |
| CSPROJ | INCLUDE-PART2, INCLUDE-HELPER | 0 each | 1 each | |
| DISP | `return true;` | recorded as N1 | N1 | M1 (P3-T1): N1 plus 1; after P3-T2: N1 |
| DISP | `_ownerThreadId.HasValue` | recorded as N2 (at least 1) | N2 | M1: N2 minus 1; after P3-T2: N2 |
| IVB | `System.Threading.SynchronizationContext.Current` | 0 | 0 | M2 (P3-T3): 1; after P3-T4: 0 |

The remarks the executor writes must not contain the literals `Task.Run(`, `.GetAwaiter()` or `RunOnDedicatedWorkerThread`; they refer to those members as `Task.Run`, `GetResult()` and `DedicatedWorkerThread.Run`. The BND remark must not contain the phrase `cannot marshal`. No remark or comment contains the identifier `FixturePath`, the word `unconditionally` outside the one pinned sentence in PART2, or the phrase `dedicated worker thread must not be` outside the pinned reason literals.

## Target Source

Each block is shown with its outermost lines at column 0; the executor writes members at the file's member indentation and P2-T7's format pass normalizes whitespace. Whitespace is not a counted token.

### A. New file QuickFiler.Test/TestSupport/DedicatedWorkerThread.cs (entire content)

    using System;
    using System.Threading;

    namespace QuickFiler.Test.TestSupport
    {
        /// <summary>
        /// Runs a delegate on a dedicated, joined background thread for tests that must exercise a
        /// thread-identity guard from a thread that is provably not the calling thread.
        /// </summary>
        /// <remarks>
        /// Issue #900 and issue #931: a <c>Task.Run</c> work item is not guaranteed to run on a
        /// thread other than the caller's, so it cannot stand in for a different thread in a
        /// thread-identity test. A thread this method constructs is
        /// distinct from every live thread by construction. The untimed <c>Join()</c> is a
        /// completion wait on one bounded synchronous call, not a sleep or a wall-clock wait, and
        /// the waiting thread and the waited-for thread are never both thread-pool workers, so
        /// the wait cannot starve the pool under parallel execution. The helper asserts nothing
        /// itself: each test states its own distinctness precondition inside its delegate so that
        /// a failure names the guard under test rather than the helper.
        /// </remarks>
        internal static class DedicatedWorkerThread
        {
            /// <summary>
            /// Runs <paramref name="action"/> on a dedicated background thread, joins it, and
            /// returns the exception it threw, or <see langword="null"/> when it completed
            /// normally.
            /// </summary>
            internal static Exception Run(Action action)
            {
                Exception captured = null;
                var thread = new Thread(() =>
                {
                    try
                    {
                        action();
                    }
                    catch (Exception error)
                    {
                        captured = error;
                    }
                });
                thread.IsBackground = true;
                thread.Start();
                thread.Join();
                return captured;
            }
        }
    }

The remark line `/// distinct from every live thread by construction. The untimed <c>Join()</c> is a` is written exactly as shown so the census token sits on one line (D-15); the file carries no nullable directive, matching the file the helper came from.

### B. New file QuickFiler.Test/Viewers/ItemViewerBreadcrumbThreadAffinityTests.Part2.cs (entire content)

    using System;
    using System.Drawing;
    using System.Reflection;
    using System.Windows.Threading;
    using FluentAssertions;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using QuickFiler.Test.TestSupport;
    using QuickFiler.Viewers;
    using UtilitiesCS.OutlookObjects.Folder;

    namespace QuickFiler.Test.Viewers
    {
        /// <summary>
        /// Continuation partial of <see cref="ItemViewerBreadcrumbThreadAffinityTests"/> holding the
        /// three cross-thread cases, each of which runs its guarded call on a dedicated thread
        /// created by <see cref="DedicatedWorkerThread"/>. The owner-thread admission cases, the
        /// shared <c>InertOperations</c> factory and the nested helper types live in the primary
        /// partial so that each file stays under the 500-line limit (issue #931).
        /// </summary>
        public sealed partial class ItemViewerBreadcrumbThreadAffinityTests
        {
            <test 1: lines 199 to 251 of the pre-edit primary file, verbatim, except that the two
            remark occurrences of the removed helper name become DedicatedWorkerThread.Run and the
            call `RunOnDedicatedWorkerThread(() =>` becomes `DedicatedWorkerThread.Run(() =>`>

            <test 2: lines 253 to 304 of the pre-edit primary file, verbatim, with the same two
            substitutions>

            /// <summary>
            /// A viewer with no owning dispatcher stays inert, which is what keeps
            /// <c>FormatterServices.GetUninitializedObject</c>-built viewers in other test files from
            /// throwing. This is the only test covering the null-owner escape.
            /// </summary>
            /// <remarks>
            /// Issue #931: the guarded call is made from a dedicated thread created by
            /// <c>DedicatedWorkerThread.Run</c>, and the delegate asserts through the owner captured
            /// before the dispatcher is cleared that it is not on the owner thread. The call is
            /// therefore off the owner thread unconditionally, so the test discriminates against the
            /// pre-#781 context-reference guard: that guard would read the non-null captured context,
            /// find the worker's null ambient context different from it, and reject the call, whereas
            /// the null-owner escape admits it. Seeding first and repeating the same provider are
            /// still required: a first-time initialization under a null ambient context would throw
            /// at <c>BreadcrumbUiDispatcher.CaptureCurrent()</c> regardless of the guard, and only the
            /// already-initialized early return can witness the escape.
            /// </remarks>
            [TestMethod]
            public void InitializeBreadcrumbPipeline_NullOwningDispatcher_DoesNotThrow()
            {
                // Arrange
                using (var scope = new ViewerScope())
                {
                    BreadcrumbPopupUiOperations operations = InertOperations();
                    var provider = new Mock<IFolderHierarchyProvider>(MockBehavior.Strict);
                    scope.Viewer.InitializeBreadcrumbPipeline(provider.Object, operations);
                    object before = scope.Viewer.BreadcrumbCoordinator;
                    Dispatcher owner = scope.Viewer.UiDispatcher;
                    owner.Should().NotBeNull("the viewer must own a dispatcher before it is cleared");
                    ClearViewerDispatcher(scope.Viewer);

                    // Act
                    Exception captured = DedicatedWorkerThread.Run(() =>
                    {
                        bool isOwnerThread = owner.CheckAccess();
                        isOwnerThread
                            .Should()
                            .BeFalse(
                                "the dedicated worker thread must not be the owner thread, or the "
                                    + "null-owner escape would be witnessed on the owner thread and "
                                    + "the test would pass vacuously"
                            );
                        scope.Viewer.InitializeBreadcrumbPipeline(provider.Object, operations);
                    });

                    // Assert
                    captured
                        .Should()
                        .BeNull(
                            "a viewer with no owning dispatcher has no boundary to enforce and must "
                                + "stay inert"
                        );
                    scope.Viewer.BreadcrumbCoordinator.Should().BeSameAs(before);
                }
            }

            <ClearViewerDispatcher: lines 357 to 371 of the pre-edit primary file, verbatim>
        }
    }

The word `unconditionally` appears exactly once, in the remark above. The reason literal segment `"the dedicated worker thread must not be the owner thread, or the "` sits on one line.

### C. Edits to QuickFiler.Test/Viewers/ItemViewerBreadcrumbThreadAffinityTests.cs (pre-edit numbering; apply bottom-up)

1. Delete lines 357 through 404 (`ClearViewerDispatcher`, the blank line, `RunOnDedicatedWorkerThread` with its remark, and the following blank line), so that the blank line 356 is followed by the `InertDropDownHost` summary that was at 405.
2. Delete lines 199 through 347 (the three cross-thread tests and the blank line after the third), so that the blank line 198 is followed by the `InertOperations` summary that was at 348.
3. Replace line 30 `    public sealed class ItemViewerBreadcrumbThreadAffinityTests` with `    public sealed partial class ItemViewerBreadcrumbThreadAffinityTests`.
4. Insert one remark line after line 27 (`/// needs no message pump.`) reading `/// The three cross-thread cases and <c>ClearViewerDispatcher</c> live in the continuation` and, on the following line, `/// partial file (issue #931).` (two lines, matching the existing three-slash remark indentation).
5. Delete line 4 `using System.Reflection;` (the only member that used it moved to the Part2 file).

No other line changes. The resulting file holds the four owner-thread admission tests, `InertOperations`, `InertDropDownHost`, `DrainableSynchronizationContext` and `ViewerScope`.

### D. Edits to QuickFiler.Test/Viewers/BreadcrumbPopupBoundaryCoverageTests.cs (pre-edit numbering)

Replace lines 52 through 62 with:

        /// <summary>
        /// An owner-only dispatcher (null context) reached from a thread that is not its owner
        /// must report a marshalling failure and must not run the action.
        /// </summary>
        /// <remarks>
        /// Issue #931: the worker is a dedicated thread created by
        /// <c>DedicatedWorkerThread.Run</c>, never a <c>Task.Run</c> work item. A blocking wait on
        /// a pool work item queued from a pool thread can run the delegate inline on the owner
        /// thread, in which case the owner-thread-id branch of <c>IsCurrentBoundary()</c> admits
        /// the call, the action runs, and the test fails spuriously. The delegate asserts it is
        /// off the owner thread before it dispatches. The rejection path reports and returns a
        /// completed task synchronously, so no task wait is needed.
        /// </remarks>
        [TestMethod]
        public void Dispatcher_OwnerOnlyWorker_ReportsWithoutRunningAction()
        {
            var errors = new List<Exception>();
            int ownerThreadId = Environment.CurrentManagedThreadId;
            BreadcrumbUiDispatcher dispatcher = CreateOwnerOnlyDispatcher(errors.Add);
            int executions = 0;
            Exception captured = DedicatedWorkerThread.Run(() =>
            {
                Environment
                    .CurrentManagedThreadId.Should()
                    .NotBe(
                        ownerThreadId,
                        "the dedicated worker thread must not be the owner thread the dispatcher "
                            + "was built for, or the rejection path would never be reached"
                    );
                dispatcher.Dispatch(() => executions++);
            });
            captured.Should().BeNull();
            executions.Should().Be(0);
            errors.Should().ContainSingle().Which.Message.Should().Contain("cannot marshal");
        }

Then insert `using QuickFiler.Test.TestSupport;` as a new line between line 11 (`using Moq;`) and line 12 (`using QuickFiler.Viewers;`). The last two statements of the test are the two pre-existing assertions, unchanged. `System.Threading.Tasks` stays imported because other tests in the file use `Task`.

### E. Edits to UtilitiesCS.Test/HelperClasses/FileInfoWrapper_Tests.cs (pre-edit numbering; apply bottom-up)

1. Delete lines 338 through 357 (the blank line and `GetSolutionFile`), so that line 337 (`        }`) is followed by the class-closing brace that was at 358.
2. Replace lines 25 through 81 (the four tests) with:

        [TestMethod]
        public void Properties_ShouldMirrorWrappedFileInfo()
        {
            // Arrange
            var file = new FileInfo(FixturePath);
            var wrapper = new FileInfoWrapper(file);

            // Assert
            wrapper.Exists.Should().Be(file.Exists);
            wrapper.FullName.Should().Be(file.FullName);
            wrapper.Name.Should().Be(file.Name);
            wrapper.Extension.Should().Be(".sln");
            wrapper.DirectoryName.Should().Be(file.DirectoryName);
            wrapper.Directory.FullName.Should().Be(file.Directory.FullName);
        }

        [TestMethod]
        public void ExplicitDirectoryCast_ShouldReturnWrappedContainingDirectory()
        {
            // Arrange
            var wrapper = new FileInfoWrapper(new FileInfo(FixturePath));

            // Act
            var directoryWrapper = (DirectoryInfoWrapper)wrapper;

            // Assert
            directoryWrapper.FullName.Should().Be(wrapper.Directory.FullName);
            directoryWrapper.Name.Should().Be(wrapper.Directory.Name);
        }

        [TestMethod]
        public void OpenRead_ShouldReturnReadableStreamForWrappedFile()
        {
            // Arrange: the sentinel is a stream this test opens and owns over the running test
            // host's own loaded assembly image, read-only with FileShare.ReadWrite, so no other
            // process's handle can deny the open and no repository or temporary file is involved
            // (issue #931). IFileInfo.OpenRead() returns the concrete FileStream type, so a
            // MemoryStream cannot stand in for it through the seam.
            using var sentinel = new FileStream(
                typeof(FileInfoWrapper_Tests).Assembly.Location,
                FileMode.Open,
                FileAccess.Read,
                FileShare.ReadWrite
            );
            var fileInfo = new Mock<IFileInfo>(MockBehavior.Strict);
            fileInfo.Setup(x => x.OpenRead()).Returns(sentinel);
            var wrapper = new FileInfoWrapper(fileInfo.Object);

            // Act
            FileStream stream = wrapper.OpenRead();

            // Assert
            stream.Should().BeSameAs(sentinel);
            stream.CanRead.Should().BeTrue();
            stream.Length.Should().BeGreaterThan(0);
        }

        [TestMethod]
        public void ToString_ShouldDelegateToWrappedFileInfo()
        {
            // Arrange
            var file = new FileInfo(FixturePath);
            var wrapper = new FileInfoWrapper(file);

            // Act
            var result = wrapper.ToString();

            // Assert
            result.Should().Be(file.ToString());
        }

3. Insert after line 14 (the class opening brace) the constant and its comment, followed by one blank line:

        // Issue #931: a rooted literal that is not expected to exist and does not point into the
        // repository. The three metadata tests assert only path computations and Exists, so no
        // file handle is opened and the outcome does not depend on any other process.
        private const string FixturePath = @"C:\Repo\fixture.sln";

The comment names the constant nowhere, so the `FixturePath` census is exactly the declaration plus three uses. No other line changes.

### F. Project-file edits to QuickFiler.Test/QuickFiler.Test.csproj

Insert `    <Compile Include="Viewers\ItemViewerBreadcrumbThreadAffinityTests.Part2.cs" />` immediately after the entry for the primary affinity file (line 98), and `    <Compile Include="TestSupport\DedicatedWorkerThread.cs" />` immediately after the entry for TestSupport\WinFormsPumpHost.cs (line 227 pre-edit, 228 after the first insertion). Both use the project-relative backslash form of the neighbouring entries.

### G. Negative-control mutations (each applied by the Edit tool and reverted by `git checkout -- <file>`)

- M1 (P3-T1), QuickFiler/Viewers/BreadcrumbUiDispatcher.cs: replace lines 276 to 277 (`return _ownerThreadId.HasValue` and `&& Environment.CurrentManagedThreadId == _ownerThreadId.Value;`) with the single line `return true;`.
- M2 (P3-T3), QuickFiler/Viewers/ItemViewer.Breadcrumb.cs: replace line 437 (`return;`) with the four lines `if (!ReferenceEquals(System.Threading.SynchronizationContext.Current, UiSyncContext))`, `{`, `throw new InvalidOperationException("mutation 931: pre-781 context-reference guard");`, `}` followed by `return;` on a fifth line, all inside the `if (owning == null)` block at 435 to 438.
- M3 (P3-T5), QuickFiler.Test/TestSupport/DedicatedWorkerThread.cs: insert `action();` as the first statement of `Run`, immediately before `Exception captured = null;`.
- M4 (P3-T7), UtilitiesCS.Test/HelperClasses/FileInfoWrapper_Tests.cs: in `OpenRead_ShouldReturnReadableStreamForWrappedFile`, insert a second `using var decoy = new FileStream(` declaration with the same four arguments immediately after the `sentinel` declaration, and change `.Returns(sentinel)` to `.Returns(decoy)`.

## Planner Self-Review Record (initial authoring, 2026-09-28)

This section persists the planner's adversarial self-review so a reader can locate it without the conversation transcript. Every citation below was re-derived against the assigned worktree during this authoring pass; none is carried forward from the research record without a fresh read. Sibling regions re-checked: the whole of each of the three edited test files (all 490, 361 and 359 lines were read), lines 60 to 285 of BreadcrumbUiDispatcher.cs, lines 1 to 30 and 420 to 459 of ItemViewer.Breadcrumb.cs, lines 1 to 40 and 150 to 217 of FileInfoWrapper.cs, the whole runner script and its Projection, FirstParty, Threshold and TrxSummary part files.

Findings that changed the plan relative to the spec's text: (1) the runner script now writes the JaCoCo package projection and the TRX summary itself and asserts both floors before doing so (fact 12), so the coverage tasks name those outputs and carry a threshold branch; (2) the runner's hard-coded filter (fact 12, line 91) and the documented local stall (fact 16) make the coverage route a recorded selection (P0-T9), with the direct route built from the runner's own functions; (3) the #906 failure was produced in the #900 run by the plan's own node-reuse workers (fact 17), so every msbuild invocation here turns node reuse off (D-16); (4) ItemViewer.Breadcrumb.cs does not import `System.Threading` (fact 9), so mutation M2 uses the fully qualified name; (5) the FluentAssertions subject name in a failure message depends on caller identification, so every predicted message is gated on its outcome phrase, not on a subject name (fact 21). No acceptance-criterion text was amended.

SELF-REVIEW: RE-DERIVED THIS PASS
- `QuickFiler.Test/Viewers/ItemViewerBreadcrumbThreadAffinityTests.cs` | lines 1 to 490 read in full: `[TestMethod]` at 38, 88, 128, 167, 218, 268, 318; `Task.Run(` only at 332; `RunOnDedicatedWorkerThread` at 204, 228, 259, 277, 385; `ClearViewerDispatcher(` at 328 and 361; `action();` at 392; reason literal on one line at 234 and 283; `distinct from every live thread by construction` on one line at 380; blank lines at 198, 347, 356, 372, 404; no `partial`, no nullable directive.
- `QuickFiler.Test/Viewers/BreadcrumbPopupBoundaryCoverageTests.cs` | lines 1 to 140 and 340 to 361 read; `Task.Run(` at 58 only; `.GetAwaiter()` at 59, 297, 305, 308 (grep over the file); `partial class` at 17; `using Moq;` at 11; `CreateOwnerOnlyDispatcher` 124 to 134; no `.NotBe(`, `.BeNull(`, `ownerThreadId`, `Thread.Sleep`, `Task.Delay`, `[Timeout` (grep); total 361 lines.
- `UtilitiesCS.Test/HelperClasses/FileInfoWrapper_Tests.cs` | lines 1 to 359 read in full; `[TestMethod]` at 15, 25, 41, 55, 69, 83, 183, 297; `GetSolutionFile` at 29, 45, 59, 73, 339; `FileMode.` on 12 lines and `FileAccess.` on 10 lines (grep enumerated); `Assembly.Location` at 190, 196, 202, 208, 214, 220; `wrapper.OpenRead()` at 62 and 287; blank line 338; class close at 358.
- `QuickFiler.Test/QuickFiler.Test.csproj` | Compile Include for the affinity file at 98, boundary pair at 105 to 106, TestSupport pair at 227 to 228; Platform default at 12; OutputPath at 36; ProjectReference to QuickFiler at 510.
- UtilitiesCS.Test/UtilitiesCS.Test.csproj | FileInfoWrapper_Tests.cs at 234; Platform default at 11; OutputPath at 51; ProjectReference to UtilitiesCS at 963.
- QuickFiler/QuickFiler.csproj and UtilitiesCS/UtilitiesCS.csproj | OutputType Library and AssemblyName at 9 and 12, 12 and 15; no TreatWarningsAsErrors property in QuickFiler.csproj.
- QuickFiler/Viewers/BreadcrumbUiDispatcher.cs | `Dispatch` 71 to 151, `IsCurrentBoundary()` call at 78, null-context rejection 97 to 105, `IsCurrentBoundary` 255 to 278, `return true;` at 260, context branch 269 to 272, owner-id branch 276 to 277.
- QuickFiler/Viewers/ItemViewer.Breadcrumb.cs | usings 1 to 9 (no `System.Threading`); `ThrowIfOffUiBoundary` 432 to 447; null-owner escape 435 to 438; throw 440 to 446.
- QuickFiler/Viewers/ItemViewer.cs | `_uiDispatcher = Dispatcher.CurrentDispatcher;` at 27; `UiSyncContext` at 59; `UiDispatcher` at 65 to 67 (grep).
- UtilitiesCS/HelperClasses/FileSystem/FileInfoWrapper.cs | public constructor 14 to 19; seam 21 to 24; `OpenRead()` 153 to 156; `ToString()` 191 to 194; explicit cast 211 to 214.
- UtilitiesCS/HelperClasses/FileSystem/PhysicalFileInfoAdapter.cs | `Directory` 104, `DirectoryName` 108, `OpenRead()` 154, `ToString()` 181.
- UtilitiesCS/Interfaces/IHelperClasses/IFileInfo.cs | `FileStream OpenRead();` at 26.
- UtilitiesCS/Properties/AssemblyInfo.cs | `InternalsVisibleTo("UtilitiesCS.Test")` at 19.
- QuickFiler.Test/TestSupport/WinFormsPumpHost.cs | `namespace QuickFiler.Test.TestSupport` at 9.
- QuickFiler.Test/Viewers/BreadcrumbPopupBoundaryCoverageTests.Part2.cs | continuation partial declaration without `[TestClass]` at 23.
- TaskMaster.runsettings | 30 lines; Workers 0 at 5; Scope ClassLevel at 6; Code Coverage collector 9 to 29. scripts/vscode/TaskMaster.cli.runsettings | 9 lines; same Workers and Scope; no collector.
- scripts/vscode/Invoke-MSTestWithCoverage.ps1 | parameters 1 to 13; filter at 91; results directory and logger at 92 to 93; `ConvertTo-DerivedCoverageSettingsXml` 97 to 134; collector throw 261 to 263; strict mode and stop preference 271 to 272; fixed results names 283 to 284; repo root 305; runsettings 312; discovery 330 to 337; post-process 382 to 384; floors 386 to 387; first-party line 388; projection 393 to 401; summary 408 to 425; retention 427 to 431; entry guard 437.
- scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1 | part-file dot-sources 2 to 6; `ConvertTo-KoverageCoberturaXml` at 407. scripts/vscode/Invoke-MSTestWithCoverage.Projection.ps1 | `ConvertTo-JacocoPackageProjection` 14 to 81; `Assert-JacocoProjectionReconciliation` 83 to 146; `Test-RawCoverageDocumentRetained` 148 to 197. scripts/vscode/Invoke-MSTestWithCoverage.FirstParty.ps1 | `Format-CoberturaFirstPartyCoverageSummary` 95 to 121 (line shape at 117 to 120); `Get-CoberturaFirstPartyCoverageReport` 123 to 162. scripts/vscode/Invoke-MSTestWithCoverage.Threshold.ps1 | line floor message at 54; branch floor message at 124. scripts/vscode/Invoke-MSTest.TrxSummary.ps1 | `Get-TrxRunSummary` 12 to 101; `Format-TrxRunSummary` 103 to 150.
- scripts/vscode/Invoke-Restore.ps1 | parameters 1 to 10; vswhere shape at 31. scripts/vscode/Install-RepoDotNetSdk.ps1 | install directory at 36. global.json | SDK 8.0.205 and `.dotnet-sdk` path.
- .csharpierignore | 18 lines as listed in fact 15. .gitignore | lines 39, 140 to 141, 144 to 145, 190.
- .claude/hooks/validate-planner-output.ps1 | path regex 95; phase regex 238; task regex 239; sequential check 299 to 302; Phase 0 checks 325 to 330; final-phase check 339.
- .claude/hooks/enforce-orchestration-preimplementation-gate.ps1 | checkpoint path 31; readiness properties 235 to 243; deny reason 429.
- docs/features/active/2026-09-28-tests-depend-on-uncontrolled-environment-931/spec.md | header line 9 (full-bug, sole AC source); Write Set 222 to 232; AC lines 236 to 254, all unchecked; Rollout follow-ups 274 to 278.
- docs/features/active/2026-09-28-tests-depend-on-uncontrolled-environment-931/issue.md | line 12 `- Work Mode: full-bug`.
- docs/features/active/breadcrumb-thread-affinity-tests-assume-taskrun-distinct-thread-900/evidence/qa-gates/p5-t5-mstest-coverage.2026-09-17T02-35.md | loop-context section recording the iteration-1 `OpenRead_ShouldReturnReadableStreamForWrappedFile` failure caused by a node-reuse worker.
- This plan | every task line carries a path token; phase headings use the em dash; task ids are sequential per phase (13, 1, 10, 9, 33); the final phase title and tasks carry the QA vocabulary the hook requires.

PLANNER-INTERNAL-REVIEW: PASS
CITATION-TO-TREE: PASS
AC-TRACEABILITY: PASS
SCOPE-BOUNDARY: PASS
CITATION: QuickFiler.Test/Viewers/ItemViewerBreadcrumbThreadAffinityTests.cs | 490 lines; tests 199-251, 253-304, 306-346; `Task.Run(` 332; `ClearViewerDispatcher` 357-371; `RunOnDedicatedWorkerThread` 373-403
CITATION: QuickFiler.Test/Viewers/BreadcrumbPopupBoundaryCoverageTests.cs | 361 lines; test 52-62; `Task.Run(` 58; `CreateOwnerOnlyDispatcher` 124-134; usings 1-12
CITATION: UtilitiesCS.Test/HelperClasses/FileInfoWrapper_Tests.cs | 359 lines; tests 25-81; `GetSolutionFile` 339-357; sentinel pattern 189-224
CITATION: QuickFiler.Test/QuickFiler.Test.csproj | Compile Include 98, 105-106, 227-228; Platform 12; ProjectReference 510
CITATION: UtilitiesCS.Test/UtilitiesCS.Test.csproj | Compile Include 234; Platform 11; ProjectReference 963
CITATION: QuickFiler/Viewers/BreadcrumbUiDispatcher.cs | `Dispatch` 71-151; rejection 97-105; `IsCurrentBoundary` 255-278; owner-id branch 276-277
CITATION: QuickFiler/Viewers/ItemViewer.Breadcrumb.cs | usings 1-9; `ThrowIfOffUiBoundary` 432-447; escape 435-438
CITATION: QuickFiler/Viewers/ItemViewer.cs | `_uiDispatcher` 27; `UiSyncContext` 59; `UiDispatcher` 65-67
CITATION: UtilitiesCS/HelperClasses/FileSystem/FileInfoWrapper.cs | constructors 14-24; `OpenRead` 153-156; `ToString` 191-194; cast 211-214
CITATION: UtilitiesCS/HelperClasses/FileSystem/PhysicalFileInfoAdapter.cs | `Directory` 104; `OpenRead` 154; `ToString` 181
CITATION: UtilitiesCS/Interfaces/IHelperClasses/IFileInfo.cs | `FileStream OpenRead();` 26
CITATION: UtilitiesCS/Properties/AssemblyInfo.cs | InternalsVisibleTo 18-19
CITATION: QuickFiler.Test/TestSupport/WinFormsPumpHost.cs | namespace 9
CITATION: QuickFiler.Test/Viewers/BreadcrumbPopupBoundaryCoverageTests.Part2.cs | partial declaration 23
CITATION: TaskMaster.runsettings | Workers 5; Scope 6; collector 9-29
CITATION: scripts/vscode/TaskMaster.cli.runsettings | 9 lines
CITATION: scripts/vscode/Invoke-MSTestWithCoverage.ps1 | filter 91; derived settings 97-134; throw 261-263; discovery 330-337; floors 386-387; projection 393-401; summary 408-425; guard 437
CITATION: scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1 | dot-sources 2-6; `ConvertTo-KoverageCoberturaXml` 407
CITATION: scripts/vscode/Invoke-MSTestWithCoverage.Projection.ps1 | 14-81; 83-146; 148-197
CITATION: scripts/vscode/Invoke-MSTestWithCoverage.FirstParty.ps1 | 95-121; 123-162
CITATION: scripts/vscode/Invoke-MSTestWithCoverage.Threshold.ps1 | messages 54 and 124
CITATION: scripts/vscode/Invoke-MSTest.TrxSummary.ps1 | 12-101; 103-150
CITATION: scripts/vscode/Invoke-Restore.ps1 | parameters 1-10; vswhere 31
CITATION: scripts/vscode/Install-RepoDotNetSdk.ps1 | install directory 36
CITATION: .csharpierignore | 18 lines
CITATION: .gitignore | 39, 140-141, 144-145, 190
CITATION: .claude/hooks/validate-planner-output.ps1 | 95, 238-239, 299-302, 325-330, 339
CITATION: .claude/hooks/enforce-orchestration-preimplementation-gate.ps1 | 31, 235-243, 429
CITATION: docs/features/active/2026-09-28-tests-depend-on-uncontrolled-environment-931/spec.md | header 9; Write Set 222-232; AC 236-254; Rollout 274-278
CITATION: docs/features/active/2026-09-28-tests-depend-on-uncontrolled-environment-931/issue.md | line 12
CITATION: docs/features/active/breadcrumb-thread-affinity-tests-assume-taskrun-distinct-thread-900/evidence/qa-gates/p5-t5-mstest-coverage.2026-09-17T02-35.md | loop-context section
AC-INVENTORY: AC1, AC2, AC3, AC4, AC5, AC6, AC7, AC8, AC9, AC10, AC11, AC12, AC13, AC14, AC15, AC16, AC17, AC18, AC19
AC-MAPPING: AC1 | IMPLEMENTATION: P2-T5 (Target Source D) | TESTS: P3-T1 control and P4-T5 full run | EVIDENCE: p4-t9 census of BND, mutation-owner-only-dispatcher-guard.md, parallel-suite-quickfiler-test.md; check-off P4-T14
AC-MAPPING: AC2 | IMPLEMENTATION: P2-T3 (Target Source B null-owner test) | TESTS: P3-T3 control and P4-T5 | EVIDENCE: p4-t9 census of PART2, mutation-null-owner-escape.md, parallel-suite-quickfiler-test.md; check-off P4-T15
AC-MAPPING: AC3 | IMPLEMENTATION: D-10 (no edit outside the four QuickFiler.Test Write Set paths) | TESTS: P4-T11 anchored name-listing diff of QuickFiler.Test | EVIDENCE: p4-t11-scope-boundary; check-off P4-T16
AC-MAPPING: AC4 | IMPLEMENTATION: P2-T3 and P2-T4 (split into partials) | TESTS: P4-T5 seven affinity results | EVIDENCE: p4-t9 census of AFF and PART2, parallel-suite-quickfiler-test.md; check-off P4-T17
AC-MAPPING: AC5 | IMPLEMENTATION: P2-T1 through P2-T7 | TESTS: P4-T9 post-format line counts | EVIDENCE: p4-t9-post-format-census; check-off P4-T18
AC-MAPPING: AC6 | IMPLEMENTATION: P2-T2 (Target Source F) | TESTS: P4-T5 four named results | EVIDENCE: p4-t9 census of CSPROJ, parallel-suite-quickfiler-test.md; check-off P4-T19
AC-MAPPING: AC7 | IMPLEMENTATION: P2-T1 (Target Source A) | TESTS: P3-T5 control | EVIDENCE: p4-t9 census of HELPER, mutation-inline-precondition.md; check-off P4-T20
AC-MAPPING: AC8 | IMPLEMENTATION: P2-T6 (Target Source E) | TESTS: P4-T6 full run | EVIDENCE: p4-t9 census of FIW, parallel-suite-utilitiescs-test.md; check-off P4-T21
AC-MAPPING: AC9 | IMPLEMENTATION: P2-T6 (OpenRead test) | TESTS: P3-T7 control and P4-T6 | EVIDENCE: p4-t9 census of FIW, mutation-openread-sentinel.md; check-off P4-T22
AC-MAPPING: AC10 | IMPLEMENTATION: P2-T6 (three metadata tests) | TESTS: P4-T6 | EVIDENCE: p2-t6 added-line counts, parallel-suite-utilitiescs-test.md; check-off P4-T23
AC-MAPPING: AC11 | IMPLEMENTATION: P3-T1 and P3-T2 | TESTS: FILTER-DISPATCHER runs | EVIDENCE: mutation-owner-only-dispatcher-guard.md; check-off P4-T24
AC-MAPPING: AC12 | IMPLEMENTATION: P3-T3 and P3-T4 | TESTS: FILTER-NULLOWNER runs | EVIDENCE: mutation-null-owner-escape.md; check-off P4-T25
AC-MAPPING: AC13 | IMPLEMENTATION: P3-T5 and P3-T6 | TESTS: FILTER-FOUR runs | EVIDENCE: mutation-inline-precondition.md; check-off P4-T26
AC-MAPPING: AC14 | IMPLEMENTATION: P3-T7 and P3-T8 | TESTS: FILTER-OPENREAD runs | EVIDENCE: mutation-openread-sentinel.md; check-off P4-T27
AC-MAPPING: AC15 | IMPLEMENTATION: P3-T9 and D-11 | TESTS: P4-T11 footprint | EVIDENCE: p3-t9-post-control-clean-tree, p4-t11-scope-boundary; check-off P4-T28
AC-MAPPING: AC16 | IMPLEMENTATION: P4-T5 and P4-T6 under TaskMaster.runsettings | TESTS: full QuickFiler.Test and UtilitiesCS.Test runs | EVIDENCE: parallel-suite-quickfiler-test.md, parallel-suite-utilitiescs-test.md, p4-t11 added-line counts; check-off P4-T29
AC-MAPPING: AC17 | IMPLEMENTATION: P4-T1 through P4-T7 loop | TESTS: P4-T7 coverage route | EVIDENCE: toolchain-pass.md; check-off P4-T30
AC-MAPPING: AC18 | IMPLEMENTATION: P0-T11, P4-T7, P4-T8 | TESTS: CMD-PACKAGE-COMPARE | EVIDENCE: coverage-baseline.md, coverage-final.md; check-off P4-T31
AC-MAPPING: AC19 | IMPLEMENTATION: P4-T12 and the artifact-hygiene convention | TESTS: CMD-SWEEP | EVIDENCE: p4-t12-hygiene-sweep; check-off P4-T32
UNRESOLVED-GAPS: NONE
PREFLIGHT: VALIDATION REQUESTED (DIRECTIVE: PREFLIGHT VALIDATION ONLY through atomic-executor; the planner-side record above is not executor clearance)

---

### Phase 0 — Policy Reads, Preconditions and Toolchain Baseline

- [ ] [P0-T1] Read, in this exact order, the policy and instruction files CLAUDE.md, .claude/rules/general-code-change.md, .claude/rules/general-unit-test.md, .claude/rules/quality-tiers.md, .claude/rules/csharp.md, .claude/rules/tonality.md, .claude/rules/plan-acceptance-gates.md, .claude/skills/atomic-plan-contract/SKILL.md, .claude/skills/acceptance-criteria-tracking/SKILL.md, .claude/skills/evidence-and-timestamp-conventions/SKILL.md, then FEATURE/spec.md, FEATURE/issue.md and FEATURE/research/2026-09-28T20-15-tests-depend-on-uncontrolled-environment-research.md, and write FEATURE/evidence/baseline/phase0-instructions-read.md with `Timestamp:`, `Policy Order:` (the ordered list above) and one line per file giving its repository-relative path and line count. The filename carries no timestamp suffix because the atomic-plan contract names it exactly. Acceptance: the artifact exists at that exact path and lists all thirteen files, each with an integer line count.

- [ ] [P0-T2] Verify the full-bug preconditions read-only and record them in FEATURE/evidence/baseline/p0-t2-mode-preconditions.<TS>.md. Acceptance, all five required: FEATURE/issue.md contains the exact line `- Work Mode: full-bug`; FEATURE/spec.md contains a heading line whose text is exactly `## Acceptance Criteria`; the box-state-independent inventory regex `^- \[[ x]\] AC([1-9]|1[0-9])\. ` matches exactly 19 lines of FEATURE/spec.md and every one of them begins `- [ ] ` (none is checked before execution starts); FEATURE/user-story.md does not exist; the spec's `## Write Set` section lists the six code and project paths of this plan's Write Set and no other code path. Any failure is `MODE PRECONDITION FAILED`: stop and report; the executor does not edit FEATURE/spec.md to repair it.

- [ ] [P0-T3] Record the working context, the diff anchors, the inherited-path set and the pre-implementation gate readiness in FEATURE/evidence/baseline/p0-t3-worktree-context.<TS>.md. This task uses only `git` invocations and the Read tool. Run, in this order: `git rev-parse --abbrev-ref HEAD`; `git rev-parse HEAD` (recorded as `BASE-SHA:`); `git fetch origin main` (recorded as `FETCH-EXIT:`; a non-zero value is recorded and the task continues with the origin/main ref already present); `git merge-base HEAD origin/main` (recorded as `MERGE-BASE:`); `git diff --name-only MERGE-BASE...HEAD` and `git status --porcelain --untracked-files=all`, whose union of paths is recorded under `INHERITED-CLAUSE-A:` (the name-listing diff cannot see untracked paths, which is why the porcelain span is its companion). Record `TOPLEVEL CONTAINS FEATURE:` as `YES` or `NO` by testing that FEATURE/spec.md exists beneath the `git rev-parse --show-toplevel` value, and `TOPLEVEL LEAF:` as only that value's final path segment; do not record the value itself. Then read artifacts/orchestration/orchestrator-state.json with the Read tool (never `git add`, never edit) and record `CHECKPOINT-EXISTS:`, `CHECKPOINT-ISSUE-NUM:`, `CHECKPOINT-FEATURE-FOLDER:`, `CHECKPOINT-ROUTE:` (the `route_id` value, else `path_selected`, else `ABSENT`) and `CHECKPOINT-LIFECYCLE-READY:` (fact 22), and `PRE-IMPLEMENTATION GATE READY:` as `YES` only when the file exists, `CHECKPOINT-ISSUE-NUM:` is `931`, `CHECKPOINT-FEATURE-FOLDER:` begins docs/features/active/, `CHECKPOINT-ROUTE:` is not `ABSENT` and `CHECKPOINT-LIFECYCLE-READY:` is `true`; otherwise `NO`. Acceptance, all six required: the abbreviated branch name equals bug/tests-depend-on-uncontrolled-environment-931 (otherwise report `BRANCH MISMATCH` and stop; do not create or switch branches); `BASE-SHA:` and `MERGE-BASE:` are each a 40-character hexadecimal value; `INHERITED-CLAUSE-A:` is present and lists none of the six Write Set code paths (a listed one is `WRITE SET ALREADY DIRTY`: stop and report); `TOPLEVEL CONTAINS FEATURE: YES`; `PRE-IMPLEMENTATION GATE READY:` is recorded, and when it is `NO` the executor reports `PRE-IMPLEMENTATION GATE NOT SEEDED` with the five `CHECKPOINT-` values and stops at this task; the artifact contains no absolute filesystem path (any absolute value the checkpoint carries is recorded as `<repo-root>`).

- [ ] [P0-T4] Probe the command channel, then bootstrap and prove the C# toolchain, writing FEATURE/evidence/baseline/p0-t4-channel-and-toolchain.<TS>.md. Part 1: run `pwsh -NoProfile -Command 'Set-Location -LiteralPath "WORKTREE"; Write-Output ("PROBE-OK " + (Test-Path -LiteralPath "TaskMaster.sln"))'` and record `CHANNEL:` as `COMMAND` when the line `PROBE-OK True` is observed, else `UNAVAILABLE` with the refusal or error text verbatim (absolute paths and account names replaced before writing). When `CHANNEL: UNAVAILABLE`, report `CHANNEL UNAVAILABLE` and stop at this task; the executor does not modify hook or permission configuration to obtain a channel. Part 2 (only under `CHANNEL: COMMAND`): record `RUNSETTINGS-HASH:` (TaskMaster.runsettings), `CLI-RUNSETTINGS-HASH:` (scripts/vscode/TaskMaster.cli.runsettings), `PRE-EDIT-HASH-AFF:`, `PRE-EDIT-HASH-BND:`, `PRE-EDIT-HASH-FIW:` and `PRE-EDIT-HASH-CSPROJ:` (the four existing Write Set files) as `Get-FileHash -Algorithm SHA256 -LiteralPath` values, and `NEW-FILES-ABSENT:` as whether neither `QuickFiler.Test/Viewers/ItemViewerBreadcrumbThreadAffinityTests.Part2.cs` nor `QuickFiler.Test/TestSupport/DedicatedWorkerThread.cs` exists; then run, inside payloads, `& pwsh -NoProfile -File (Join-Path (Get-Location).Path "scripts\vscode\Install-RepoDotNetSdk.ps1")` (idempotent), test that the directory .dotnet-sdk\sdk\8.0.205 exists (`SDK-MARKER:`), `dotnet --version`, `dotnet tool restore`, `dotnet tool list --local` (the `csharpier` row must read `1.2.6`), resolve MSBuild.exe and vstest.console.exe through vswhere (recorded as `MSBUILD-RESOLVED:` and `VSTEST-RESOLVED:`, `YES` or `NO`, plus only the path segment after `Microsoft Visual Studio\`), and `dotnet-coverage --version` (if not found, run `dotnet tool install --global dotnet-coverage` and then `dotnet-coverage --version` again in a separate invocation). Acceptance, all nine required: `CHANNEL: COMMAND`; the six hashes are each 64-character hexadecimal values; `NEW-FILES-ABSENT: True`; `SDK-MARKER: YES`; `dotnet --version` and `dotnet tool restore` exit 0; the `csharpier` row shows `1.2.6`; `MSBUILD-RESOLVED: YES` and `VSTEST-RESOLVED: YES`; `dotnet-coverage --version` exits 0 with its version string recorded; the artifact contains no absolute filesystem path.

- [ ] [P0-T5] Restore NuGet packages by running, inside a payload, `& pwsh -NoProfile -File (Join-Path (Get-Location).Path "scripts\vscode\Invoke-Restore.ps1") 2>&1 | Tee-Object -FilePath "coverage\logs\p0-t5.restore.log"` followed by `Write-Output ("RESTORE_EXIT_CODE: " + $LASTEXITCODE)` and `Write-Output ("PACKAGE-DIR-COUNT: " + @(Get-ChildItem -LiteralPath packages -Directory).Count)`, and write FEATURE/evidence/baseline/p0-t5-nuget-restore.<TS>.md with `Timestamp:`, `Command:` (`pwsh -NoProfile -File scripts/vscode/Invoke-Restore.ps1`), `EXIT_CODE:` (the printed `RESTORE_EXIT_CODE:`) and an `Output Summary:` recording `PACKAGE-DIR-COUNT:`. A fresh agent worktree has no packages directory and no bin\Debug output, so every later build and test task depends on this step. Acceptance, both required: `EXIT_CODE: 0`; `PACKAGE-DIR-COUNT:` is at least 1.

- [ ] [P0-T6] Capture the baseline formatting state by running `dotnet tool run csharpier check .` inside a payload from the worktree root and write FEATURE/evidence/baseline/p0-t6-csharpier-check.<TS>.md with `Timestamp:`, `Command:`, `EXIT_CODE:` and an `Output Summary:` quoting the tool's final summary line verbatim (shape `Checked N files in Xms.`; the check command never prints a `Formatted` line) and recording `CHECKED-FILES:` as that N. Acceptance, both required: `EXIT_CODE: 0`; `CHECKED-FILES:` is a positive integer. A non-zero exit means pre-existing drift the repository owns, and the later repository-wide format would fold that repair into this branch: stop and report `FORMAT BASELINE NOT CLEAN` with the tool's file list; do not run `format` to repair it.

- [ ] [P0-T7] Capture the baseline analyzer state with `CMD-REBUILD` using the analyzer `GATEARGS` and `TASKID` `p0-t7` (`Command:` records `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`, resolved through vswhere, plus /nodeReuse:false) and write FEATURE/evidence/baseline/p0-t7-msbuild-analyzers.<TS>.md with `Timestamp:`, `Command:`, `EXIT_CODE:` (the printed `MSBUILD_EXIT_CODE:`, also recorded as `ANALYZE-BASELINE-EXIT:`), and an `Output Summary:` recording `SKIP_CORECOMPILE_LINES:`, `QF_TEST_CSC_OUT_LINES:`, `UCS_TEST_CSC_OUT_LINES:`, `ZERO_ERRORS_LINES:`, `WARNINGS:` (also `ANALYZE-BASELINE-WARNINGS:`), `ERRORS:` and `WRITESET_DIAGNOSTIC_LINES:`. Acceptance, all four required: `EXIT_CODE: 0`; `SKIP_CORECOMPILE_LINES: 0`; `QF_TEST_CSC_OUT_LINES:` and `UCS_TEST_CSC_OUT_LINES:` are each at least 1 (the two test projects were compiled, not skipped); `ERRORS: 0`. A non-zero exit or a non-zero error count is `ANALYZER BASELINE NOT CLEAN`: stop and report, because AC17 cannot then be met by a test-only change.

- [ ] [P0-T8] Capture the baseline nullable and type-check state with `CMD-REBUILD` using the nullable `GATEARGS` and `TASKID` `p0-t8` (`Command:` records `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`, resolved through vswhere, plus /nodeReuse:false; no Nullable property override, no incremental Build target) and write FEATURE/evidence/baseline/p0-t8-msbuild-nullable.<TS>.md with the P0-T7 field set (`NULLABLE-BASELINE-EXIT:`, `NULLABLE-BASELINE-WARNINGS:`) plus `QF-TEST-DLL-EXISTS:` and `UCS-TEST-DLL-EXISTS:` as whether QuickFiler.Test\bin\Debug\QuickFiler.Test.dll and UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll exist after the command. Acceptance, all five required: `EXIT_CODE: 0`; `SKIP_CORECOMPILE_LINES: 0`; both `_CSC_OUT_LINES:` values at least 1; `ERRORS: 0`; both `-DLL-EXISTS:` values `True` (the precondition for P0-T9 and P0-T10). A non-zero exit is `NULLABLE BASELINE NOT CLEAN`: stop and report.

- [ ] [P0-T9] Run the stall probe with `CMD-VSTEST` using `ASSEMBLY-UCS`, `FILTER-STALL`, `NAMES-NONE` and `TASKID` `p0-t9`, and write FEATURE/evidence/baseline/p0-t9-stall-probe.<TS>.md with `Timestamp:`, `Command:`, `EXIT_CODE:` (the printed `VSTEST_EXIT_CODE:`, or the payload exit code 3 when the TRX is absent), `ExpectedExitCode:` equal to the observed value when it is non-zero (presentational; this task gates nothing on the exit code), and an `Output Summary:` recording `RUNSETTINGS-HASH-NOW:`, `TRX_PRESENT:`, `SEQUENCE_FILES:`, `COLLECTOR_LINES:`, the `COUNTERS` line when present and every `MESSAGE` line. Then record exactly one `STALL-PROBE:` line: `CLEAR` when `EXIT_CODE: 0`, `failed` is 0 and `SEQUENCE_FILES: 0`; otherwise `REPRODUCES`. From it record exactly one `UCS-FILTERARG:` line (`NONE` under `CLEAR`; the quoted `FILTER-UCS-EXCLUDE` argument verbatim under `REPRODUCES`) and exactly one `COVERAGE-ROUTE:` line (`RUNNER` under `CLEAR`; `DIRECT` under `REPRODUCES`), with the sentence that the four excluded classes are a pre-existing local stall reproduced on main and are executed by CI (fact 16). Acceptance, all three required: `RUNSETTINGS-HASH-NOW:` equals `RUNSETTINGS-HASH:` from P0-T4; exactly one `STALL-PROBE:`, one `UCS-FILTERARG:` and one `COVERAGE-ROUTE:` line are present with the values the rule above derives; the probe took at most one invocation (it is never re-run). Both `STALL-PROBE:` values complete this task.

- [ ] [P0-T10] Capture the baseline parallel-suite runs of the two affected assemblies, before any edit, and write FEATURE/evidence/baseline/test-run-baseline.md (fixed name per the spec). Run `CMD-VSTEST` with `ASSEMBLY-QF`, empty `FILTERARG`, `NAMES-QF` and `TASKID` `p0-t10-qf`, then `CMD-VSTEST` with `ASSEMBLY-UCS`, `UCS-FILTERARG` (P0-T9), `NAMES-FIW` and `TASKID` `p0-t10-ucs`. The artifact carries `Timestamp:`, `Command:` (both commands, the runsettings file name, the isolation switch and the UtilitiesCS filter verbatim or `NONE`), `EXIT_CODE:` (scoped to the QuickFiler.Test run; the UtilitiesCS.Test exit code under `UCS-VSTEST-EXIT:`), `ExpectedExitCode:` equal to the QuickFiler.Test exit code when it is non-zero (presentational), and an `Output Summary:` with, per assembly, `RUNSETTINGS-HASH-NOW:`, the `COUNTERS` line, `RESULT_COUNT:`, `SEQUENCE_FILES:`, `COLLECTOR_LINES:`, every `RESULT` line and every `MESSAGE` line, plus `BASELINE-FAILED-QF:` and `BASELINE-FAILED-UCS:` listing every `Failed` test name or `NONE`. Acceptance, all five required: both `RUNSETTINGS-HASH-NOW:` values equal `RUNSETTINGS-HASH:`; both `COUNTERS` lines are present with `executed` at least 1; the seven `NAMES-AFFINITY` names and `Dispatcher_OwnerOnlyWorker_ReportsWithoutRunningAction` each appear in a `RESULT` line of the QuickFiler.Test run, and the eight `NAMES-FIW` names each appear in a `RESULT` line of the UtilitiesCS.Test run (this is the pre-edit population the final runs are compared against); both `SEQUENCE_FILES:` values are 0; both `BASELINE-FAILED-` lines are present. When either failed set is non-empty, this task records it (D-17): a set containing only tests named in the spec's Repro (the four FileInfoWrapper tests) or the #780 flake completes the task; any other name is `BASELINE NOT GREEN`, reported with the `MESSAGE` lines, and the run stops.

- [ ] [P0-T11] Capture the baseline repository-wide test and coverage run by the route P0-T9 fixed and write FEATURE/evidence/baseline/coverage-baseline.md (fixed name per the spec). Under `COVERAGE-ROUTE: RUNNER` run `CMD-COVERAGE-RUNNER` with `STAGE` `baseline`, then `CMD-COVERAGE-POST` with `STAGE` `baseline` and `RAW` `False`; under `DIRECT` run `CMD-COVERAGE-DIRECT` with `STAGE` `baseline`, then `CMD-COVERAGE-POST` with `STAGE` `baseline` and `RAW` `True`. The artifact carries `Timestamp:`, `Command:` (both payloads named, the route, and the filter the route applied), `EXIT_CODE:` (`RUNNER_EXIT_CODE:` or `COLLECT_EXIT_CODE:`), `ExpectedExitCode:` equal to the observed value when it is non-zero and the failed set is the #780 flake only (see below), and an `Output Summary:` recording `COVERAGE-ROUTE:`, `DISCOVERED_LINE:` or `ASSEMBLY_COUNT:` with every `ASSEMBLY:` line, `THRESHOLD_MESSAGE:` or `LINE-FLOOR:` and `BRANCH-FLOOR:`, the `First-party coverage:` line, the projection verbatim between `PROJECTION-BEGIN` and `PROJECTION-END`, the five summary lines verbatim between `SUMMARY-BEGIN` and `SUMMARY-END`, and `FAILED-SET:`. Branches, decided by the recorded values: (a) exit 0: complete. (b) non-zero exit whose log shows `COLLECT_FAILURE_MESSAGE:` (runner) or whose `FAILED-SET:` is non-empty (direct) and whose failed set is exactly `TryAddValuesAsync_UpdatesExistingValue`: re-run the identical route once (D-12), record both attempts, and complete on a green second run; otherwise `BASELINE NOT GREEN`: stop and report. (c) runner exit non-zero with a `THRESHOLD_MESSAGE:` and `DOCUMENT_PRESENT: True`: the document is already post-processed, so run `CMD-COVERAGE-POST` with `RAW` `False`, record `FLOOR: NOT MET` with the message, and complete (pre-existing merge-base state; this change adds no production line). Acceptance, all five required: the projection block contains a `package` element named `UtilitiesCS` and one named `QuickFiler`, each with a `LINE` and a `BRANCH` counter; the `First-party coverage:` line is present with four integer counts and two percentages; the summary block's first line begins `Test run outcome:`; `FAILED-SET:` is present; the artifact contains no absolute path (the runner's own path-bearing lines are not transcribed). coverage\baseline-931.jacoco.xml and coverage\baseline-931.cobertura.xml remain on disk, git-ignored, for P4-T8.

- [ ] [P0-T12] Record the pre-edit census by running `CMD-CENSUS` once for each of `QuickFiler.Test/Viewers/ItemViewerBreadcrumbThreadAffinityTests.cs`, `QuickFiler.Test/Viewers/BreadcrumbPopupBoundaryCoverageTests.cs`, `UtilitiesCS.Test/HelperClasses/FileInfoWrapper_Tests.cs`, `QuickFiler.Test/QuickFiler.Test.csproj`, QuickFiler/Viewers/BreadcrumbUiDispatcher.cs and QuickFiler/Viewers/ItemViewer.Breadcrumb.cs, and write FEATURE/evidence/baseline/p0-t12-pre-edit-census.<TS>.md transcribing every `TOKEN`, `LINES` and `SHA256` line per file, plus `N1:` (the DISP `return true;` count) and `N2:` (the DISP `_ownerThreadId.HasValue` count). These are the positive controls every later count gate is measured against. Acceptance, all four required: every value listed in the `pre` column of the Token Census Expectations table matches (in particular AFF `LINES = 490`, `Task.Run(` 1, `RunOnDedicatedWorkerThread` 5, `partial class` 0; BND `LINES = 361`, `Task.Run(` 1, `.GetAwaiter()` 4, `ownerThreadId` 0; FIW `LINES = 359`, `GetSolutionFile` 5, `TaskMaster.sln` 1, `FileMode.` 12, `FileAccess.` 10, `FixturePath` 0; CSPROJ INCLUDE-PART2 0 and INCLUDE-HELPER 0; IVB `System.Threading.SynchronizationContext.Current` 0); `N2:` is at least 1; the four `SHA256` values for the existing Write Set files equal the four `PRE-EDIT-HASH-` values of P0-T4; any mismatch is `TREE DIVERGED FROM PLAN` and is reported before any edit.

- [ ] [P0-T13] Commit the Phase 0 evidence and the feature documents in the exemption-eligible form. Run, as two separate invocations, `git add -- docs/features/active/2026-09-28-tests-depend-on-uncontrolled-environment-931` and then `git commit -m "docs(931): phase 0 baseline evidence for the uncontrolled-environment test fix" -- docs/features/active/2026-09-28-tests-depend-on-uncontrolled-environment-931` (a single `-m` string, one pathspec operand after `--`, one command segment, no `$`, backtick, `<` or `>` anywhere on the line; every staged path is under docs/features/active/), then `git status --porcelain -- QuickFiler QuickFiler.Test UtilitiesCS UtilitiesCS.Test` and `git rev-parse HEAD`. Write FEATURE/evidence/baseline/p0-t13-phase0-commit.<TS>.md recording `COMMIT-EXIT:`, `PHASE0-HEAD:` and the scoped porcelain output verbatim (`EXIT_CODE:` is scoped to the porcelain span). Acceptance, all three required: `COMMIT-EXIT: 0`; the scoped porcelain output is empty, proving no source edit preceded Phase 1; `PHASE0-HEAD:` differs from `BASE-SHA:`. A PreToolUse refusal is `PRE-IMPLEMENTATION GATE BLOCKED`: stop and report. The artifact this task writes is committed by P2-T10.

### Phase 1 — Fail-Before Evidence

- [ ] [P1-T1] Write the fail-before exception dossier FEATURE/evidence/regression-testing/fail-before-exception.<TS>.md, where `<TS>` equals its own `Timestamp:` field, with these sections: `Timestamp:`; `WhyFailingRunImpossible:` (one to three sentences: the distinct-thread defect is decided by whether the runtime inlines a pool work item onto the waiting thread, a branch no committed test can force without mutating process-global thread-pool state; the file-handle defect is decided by whether another process holds the solution file with a share mode excluding readers, which a committed test cannot arrange without starting an external process; both are prohibited by the unit-test policy); `## Alternative Proof` (the spec's Repro steps 1 to 3 with their line citations as re-recorded by P0-T12, fact 17's record of the observed #906 failure in the #900 run, and the sentence that P3-T1, P3-T3, P3-T5 and P3-T7 later demonstrate deterministically that each rewritten test fails against a deliberately broken guard, without stating those results as observations); `SearchScope:` (FEATURE/evidence/regression-testing/), `SearchPatterns:` (`fail-before-exception.*.md`), `SearchResult:` (this file); `## Output Summary`. Acceptance, all four required: the file exists with all named sections; its filename timestamp equals its `Timestamp:` field; `WhyFailingRunImpossible:` is non-empty; the text contains the literal `P3-T1` and the literal `inline` at least once each.

### Phase 2 — Fix

- [ ] [P2-T1] Create `QuickFiler.Test/TestSupport/DedicatedWorkerThread.cs` with exactly the Target Source A content, then run `CMD-CENSUS` on it and write FEATURE/evidence/regression-testing/p2-t1-helper-census.<TS>.md transcribing every `TOKEN`, `LINES` and `SHA256` line. Acceptance, all four required: `namespace QuickFiler.Test.TestSupport` 1, `internal static class DedicatedWorkerThread` 1, `internal static Exception Run(Action action)` 1, `new Thread(` 1, `IsBackground = true` 1, `thread.Join();` 1, `action();` 1, `distinct from every live thread by construction` 1; `Join(` equals `Join()`; `.Should()` 0, `Task.Run(` 0, `Thread.Sleep` 0, `Task.Delay` 0, `[Timeout` 0, `DoNotParallelize` 0, `Retry` 0; `LINES` at most 500. A PreToolUse refusal of the file creation is `PRE-IMPLEMENTATION GATE BLOCKED`: stop and report.

- [ ] [P2-T2] Register the two new files in `QuickFiler.Test/QuickFiler.Test.csproj` exactly as Target Source F states, then run `CMD-CENSUS` on the project file, `git diff --numstat HEAD -- QuickFiler.Test/QuickFiler.Test.csproj` and `git status --porcelain -- QuickFiler.Test/QuickFiler.Test.csproj`, and write FEATURE/evidence/regression-testing/p2-t2-csproj-census.<TS>.md transcribing the three project-file `TOKEN` lines, the numstat line and the porcelain line. Acceptance, all three required: INCLUDE-AFFINITY 1, INCLUDE-PART2 1, INCLUDE-HELPER 1; the numstat line reads 2 added and 0 deleted; the porcelain line begins ` M` for the project file.

- [ ] [P2-T3] Create `QuickFiler.Test/Viewers/ItemViewerBreadcrumbThreadAffinityTests.Part2.cs` with exactly the Target Source B content (the two verbatim test blocks copied from pre-edit lines 199 to 251 and 253 to 304 of the primary file with the two named substitutions each, the rewritten null-owner test, and `ClearViewerDispatcher` copied from pre-edit lines 357 to 371), then run `CMD-CENSUS` on it and write FEATURE/evidence/regression-testing/p2-t3-part2-census.<TS>.md transcribing every `TOKEN`, `LINES` and `SHA256` line. Acceptance, all four required: `DedicatedWorkerThread.Run(` 3, `ClearViewerDispatcher(` 2, `[TestMethod]` 3, `dedicated worker thread must not be` 3; `partial class` 1, `using QuickFiler.Test.TestSupport;` 1, `using System.Reflection;` 1, `owner.CheckAccess()` 1, `.BeNull(` 1, `unconditionally` 1; `Task.Run(` 0, `.GetAwaiter()` 0, `RunOnDedicatedWorkerThread` 0, `action();` 0, `Thread.Sleep` 0, `Task.Delay` 0, `[Timeout` 0, `DoNotParallelize` 0, `Retry` 0; `LINES` at most 500.

- [ ] [P2-T4] Apply the five Target Source C edits to `QuickFiler.Test/Viewers/ItemViewerBreadcrumbThreadAffinityTests.cs`, then run `CMD-CENSUS` on it, `git diff --numstat HEAD -- QuickFiler.Test/Viewers/ItemViewerBreadcrumbThreadAffinityTests.cs` and `git status --porcelain -- QuickFiler.Test/Viewers/ItemViewerBreadcrumbThreadAffinityTests.cs`, and write FEATURE/evidence/regression-testing/p2-t4-primary-census.<TS>.md transcribing every `TOKEN`, `LINES` and `SHA256` line, the numstat line and the porcelain line. Acceptance, all five required: `partial class` 1 and `[TestMethod]` 4; `Task.Run(` 0, `.GetAwaiter()` 0, `RunOnDedicatedWorkerThread` 0, `ClearViewerDispatcher(` 0, `action();` 0, `new Thread(` 0, `IsBackground = true` 0, `thread.Join();` 0, `using System.Reflection;` 0, `dedicated worker thread must not be` 0, `distinct from every live thread by construction` 0, `DedicatedWorkerThread.Run(` 0; `LINES` at most 500 and at least 280; the numstat line reads at most 4 added and at least 197 deleted (the edits add the `partial` declaration line and two remark lines and delete 199 lines); the porcelain line begins ` M`.

- [ ] [P2-T5] Apply the Target Source D edits to `QuickFiler.Test/Viewers/BreadcrumbPopupBoundaryCoverageTests.cs`, then run `CMD-CENSUS` on it and `git status --porcelain -- QuickFiler.Test/Viewers/BreadcrumbPopupBoundaryCoverageTests.cs`, and write FEATURE/evidence/regression-testing/p2-t5-boundary-census.<TS>.md transcribing every `TOKEN`, `LINES` and `SHA256` line and the porcelain line. Acceptance, all four required: `Task.Run(` 0 and `.GetAwaiter()` 3 (each one less than P0-T12); `DedicatedWorkerThread.Run(` 1, `using QuickFiler.Test.TestSupport;` 1, `.NotBe(` 1, `.BeNull(` 1, `dedicated worker thread must not be` 1, `ownerThreadId` 2; `executions.Should().Be(0)` 1, `cannot marshal` 1, `partial class` 1, `Thread.Sleep` 0, `Task.Delay` 0, `[Timeout` 0, `DoNotParallelize` 0; `LINES` at most 500.

- [ ] [P2-T6] Apply the three Target Source E edits to `UtilitiesCS.Test/HelperClasses/FileInfoWrapper_Tests.cs`, then run `CMD-CENSUS` on it, `git diff -U0 HEAD -- UtilitiesCS.Test/HelperClasses/FileInfoWrapper_Tests.cs` and `git status --porcelain -- UtilitiesCS.Test/HelperClasses/FileInfoWrapper_Tests.cs`, and write FEATURE/evidence/regression-testing/p2-t6-fileinfowrapper-census.<TS>.md transcribing every `TOKEN`, `LINES` and `SHA256` line, the porcelain line, and these counts over the diff's added lines (lines beginning with a single `+`): `ADDED-LENGTH:` (lines containing `.Length`), `ADDED-OPENREAD:` (lines containing `OpenRead()`), `ADDED-FIXTUREPATH:` (lines containing `FixturePath`), `ADDED-OPEN-CREATE-WRITE:` (lines containing `.Open(`, `.Create(` or `.OpenWrite(`). Acceptance, all six required: `GetSolutionFile` 0, `TaskMaster.sln` 0, `AppDomain` 0, `File.Exists(` 0, `File.Create` 0, `File.WriteAll` 0, `File.Delete` 0, `Path.GetTemp` 0; `FileMode.` equals `FileMode.Open` (13) and `FileAccess.` equals `FileAccess.Read` (11); `FileShare.ReadWrite` 9, `Assembly.Location` 7, `FixturePath` 4, `using var sentinel = new FileStream(` 1, `BeSameAs(sentinel)` 1, `.Returns(decoy)` 0, `wrapper.OpenRead()` 2, `stream.CanRead.Should().BeTrue()` 1, `stream.Length.Should().BeGreaterThan(0)` 1, `[TestMethod]` 8; `ADDED-LENGTH: 1`, `ADDED-OPENREAD: 2`, `ADDED-FIXTUREPATH: 4`, `ADDED-OPEN-CREATE-WRITE: 0` (the three metadata tests open no stream and call none of `Open`, `Create` or `OpenWrite`, AC10); `LINES` at most 500; the porcelain line begins ` M`.

- [ ] [P2-T7] Format and verify the five Write Set `.cs` files with the pinned CSharpier. Inside one payload: capture the SHA-256 of each of `QuickFiler.Test/Viewers/ItemViewerBreadcrumbThreadAffinityTests.cs`, `QuickFiler.Test/Viewers/ItemViewerBreadcrumbThreadAffinityTests.Part2.cs`, `QuickFiler.Test/TestSupport/DedicatedWorkerThread.cs`, `QuickFiler.Test/Viewers/BreadcrumbPopupBoundaryCoverageTests.cs` and `UtilitiesCS.Test/HelperClasses/FileInfoWrapper_Tests.cs`; run `dotnet tool run csharpier format` with those five paths as arguments; capture the five hashes again; run `dotnet tool run csharpier check` with the same five paths; then run `CMD-CENSUS` on each of the five. Write FEATURE/evidence/regression-testing/p2-t7-csharpier-scoped.<TS>.md with `Timestamp:`, `Command:` (all three), `EXIT_CODE:` (of the `check`), and an `Output Summary:` recording the ten hashes, `REWRITTEN:` as the number of files whose hash changed (the console line `Formatted 5 files in Xms.` is a processed-file count and is not this value), the `check` command's final summary line verbatim, and every `TOKEN` and `LINES` line per file. Acceptance, all three required: the `check` exits 0; every `post` value of the Token Census Expectations table holds for all five files (formatting re-wraps lines but every counted token is single-line by construction); every `LINES` value is at most 500. If any `LINES` exceeds 500, stop and report `FILE SIZE LIMIT EXCEEDED` with the file and value; do not shorten remarks to recover, because the remarks carry pinned census literals.

- [ ] [P2-T8] Build both test projects from the edited source with `CMD-BUILD-QF` (`TASKID` `p2-t8-qf`) and `CMD-BUILD-UCS` (`TASKID` `p2-t8-ucs`) and write FEATURE/evidence/regression-testing/p2-t8-build-after-fix.<TS>.md with `Timestamp:`, `Command:` (`msbuild QuickFiler.Test\QuickFiler.Test.csproj /t:Build /m /p:Configuration=Debug /p:Platform=AnyCPU` and the UtilitiesCS.Test twin, resolved through vswhere, plus /nodeReuse:false), `EXIT_CODE:` (scoped to the QuickFiler.Test build; the other under `UCS-MSBUILD-EXIT:`), and an `Output Summary:` recording, per build, `CSC_OUT_LINES:`, `PROD_CSC_OUT_LINES:`, `ZERO_ERRORS_LINES:` and `DLL_ADVANCED:`. Acceptance, all three required: both exit codes 0; both `CSC_OUT_LINES:` at least 1 and both `DLL_ADVANCED: True` (the edited files were compiled into the assemblies the next task loads); both `ZERO_ERRORS_LINES:` at least 1.

- [ ] [P2-T9] Run the rewritten tests before any control, as confirming runs (the measured runs are P4-T5 and P4-T6). Run `CMD-VSTEST` four times: `ASSEMBLY-QF` with `FILTER-FOUR`, `NAMES-FOUR`, `TASKID` `p2-t9-four`; `ASSEMBLY-UCS` with `FILTER-OPENREAD`, `NAMES-FIW`, `TASKID` `p2-t9-openread`; `ASSEMBLY-QF` with `FILTER-AFFINITY-CLASS`, `NAMES-AFFINITY`, `TASKID` `p2-t9-affinity`; `ASSEMBLY-UCS` with `FILTER-FIW-CLASS`, `NAMES-FIW`, `TASKID` `p2-t9-fiw`. Write FEATURE/evidence/regression-testing/p2-t9-pass-before-controls.<TS>.md with `Timestamp:`, `Command:` (all four), `EXIT_CODE:` (scoped to the first run; the other three under named lines) and an `Output Summary:` recording, per run, `RUNSETTINGS-HASH-NOW:`, the `COUNTERS` line, every `RESULT` line and every `MESSAGE` line. Acceptance, all five required: all four exit codes 0; `total` is 4, 1, 7 and 8 respectively with `failed` 0 in each (a different total means a stale assembly or an unregistered file and fails this task); every `RESULT` line reads `Passed`; the four `NAMES-FOUR` names, the seven `NAMES-AFFINITY` names and the eight `NAMES-FIW` names each appear in a `RESULT` line of the run that targets them; every `RUNSETTINGS-HASH-NOW:` equals `RUNSETTINGS-HASH:`. If any test fails, the fix is wrong: repair within the Target Source contract, re-run P2-T7 through P2-T9, and record each iteration; do not proceed to P2-T10 with a failing run.

- [ ] [P2-T10] Commit the fix and the Phase 1 and Phase 2 evidence. Run `git add -- QuickFiler.Test/Viewers/ItemViewerBreadcrumbThreadAffinityTests.cs QuickFiler.Test/Viewers/ItemViewerBreadcrumbThreadAffinityTests.Part2.cs QuickFiler.Test/TestSupport/DedicatedWorkerThread.cs QuickFiler.Test/Viewers/BreadcrumbPopupBoundaryCoverageTests.cs QuickFiler.Test/QuickFiler.Test.csproj UtilitiesCS.Test/HelperClasses/FileInfoWrapper_Tests.cs docs/features/active/2026-09-28-tests-depend-on-uncontrolled-environment-931`, then `git commit -m "test(931): remove scheduler and file-handle dependence from breadcrumb affinity and FileInfoWrapper tests"` (a second `-m` paragraph for the session's attribution trailer if its harness requires one), then `git diff --exit-code HEAD -- QuickFiler.Test UtilitiesCS.Test`, `git show --name-only --format= HEAD`, `git rev-parse HEAD`, and the SHA-256 of each of the six code files. Write FEATURE/evidence/regression-testing/p2-t10-fix-commit.<TS>.md recording `COMMIT-EXIT:`, `FIX-HEAD:`, `FIX-HASH-AFF:`, `FIX-HASH-PART2:`, `FIX-HASH-HELPER:`, `FIX-HASH-BND:`, `FIX-HASH-CSPROJ:`, `FIX-HASH-FIW:` (the anchors every Phase 3 revert compares against), the `git show` path list verbatim, and `EXIT_CODE:` scoped to the `git diff --exit-code` span. Acceptance, all four required: `COMMIT-EXIT: 0`; `EXIT_CODE: 0`, proving the committed state equals the working tree for both test projects; the `git show` list contains exactly the six code paths plus paths under docs/features/active/2026-09-28-tests-depend-on-uncontrolled-environment-931/ and nothing else; the five `.cs` hashes equal the post-format hashes P2-T7 recorded. This commit stages paths outside every exempt tree; a PreToolUse refusal is `PRE-IMPLEMENTATION GATE BLOCKED`: stop and report.

### Phase 3 — Negative Controls (each rewritten guard test observed failing against a broken guard)

- [ ] [P3-T1] [expect-fail] Control M1, owner-only dispatcher guard disabled: apply mutation M1 (Target Source G) to QuickFiler/Viewers/BreadcrumbUiDispatcher.cs, run `CMD-CENSUS` on that file, `CMD-BUILD-QF` with `TASKID` `p3-t1`, then `CMD-VSTEST` with `ASSEMBLY-QF`, `FILTER-DISPATCHER`, `NAMES-FOUR`, `TASKID` `p3-t1`. Write FEATURE/evidence/regression-testing/mutation-owner-only-dispatcher-guard.md (fixed name) with `Timestamp:`, `Command:` (all three), `EXIT_CODE:` (the `VSTEST_EXIT_CODE:` of this mutated run; the artifact states that its exit-code row is scoped to the mutated run), `ExpectedExitCode: 1`, and a section `## Mutated run` recording the mutated file (QuickFiler/Viewers/BreadcrumbUiDispatcher.cs) and hunk (lines 276 to 277 replaced by `return true;`), `TOKEN return true; = N1 plus 1` and `TOKEN _ownerThreadId.HasValue = N2 minus 1` (with the P0-T12 values named), `MSBUILD_EXIT_CODE:`, `PROD_CSC_OUT_LINES:`, the runsettings file name, the isolation switch, the filter verbatim, `RUNSETTINGS-HASH-NOW:`, the `COUNTERS` line, every `RESULT` line and every `MESSAGE` line verbatim. Predicted failure (D-6): the test `Failed` with a `MESSAGE` line containing `to be 0, but found 1`. Acceptance, all five required: the two census transitions hold; `MSBUILD_EXIT_CODE: 0` and `PROD_CSC_OUT_LINES:` at least 1 (the mutated production file was compiled); `total` 1, `failed` 1; the `MESSAGE` line contains `to be 0, but found 1`; `RUNSETTINGS-HASH-NOW:` equals `RUNSETTINGS-HASH:`. A passing run, or a `MESSAGE` naming a different assertion, is `MUTATION PREDICTION MISMATCH`: stop and report the text.

- [ ] [P3-T2] Revert control M1 and confirm: run `git checkout -- QuickFiler/Viewers/BreadcrumbUiDispatcher.cs`, then `git diff --exit-code HEAD -- QuickFiler/Viewers/BreadcrumbUiDispatcher.cs`, `git status --porcelain -- QuickFiler/Viewers/BreadcrumbUiDispatcher.cs`, `CMD-CENSUS` on that file, `CMD-BUILD-QF` with `TASKID` `p3-t2`, and `CMD-VSTEST` with `ASSEMBLY-QF`, `FILTER-DISPATCHER`, `NAMES-FOUR`, `TASKID` `p3-t2`. Append to FEATURE/evidence/regression-testing/mutation-owner-only-dispatcher-guard.md a section `## Revert and confirming run` recording `REVERT-DIFF-EXIT:` (the `git diff --exit-code` exit), the porcelain output verbatim (`EMPTY` when it printed nothing), `TOKEN return true; = N1`, `TOKEN _ownerThreadId.HasValue = N2`, `PROD_CSC_OUT_LINES:`, `CONFIRMING-RUN-EXIT:`, `RUNSETTINGS-HASH-NOW:`, the `COUNTERS` line and every `RESULT` line. Acceptance, all five required: `REVERT-DIFF-EXIT: 0`; the porcelain output is empty; both census values equal the P0-T12 values; `PROD_CSC_OUT_LINES:` at least 1 (the reverted production file was recompiled before the confirming run); `CONFIRMING-RUN-EXIT: 0` with `total` 1, `passed` 1 and the `RESULT` line `Passed`.

- [ ] [P3-T3] [expect-fail] Control M2, null-owner escape replaced by the pre-#781 context-reference throw: apply mutation M2 (Target Source G) to QuickFiler/Viewers/ItemViewer.Breadcrumb.cs, run `CMD-CENSUS` on that file, `CMD-BUILD-QF` with `TASKID` `p3-t3`, then `CMD-VSTEST` with `ASSEMBLY-QF`, `FILTER-NULLOWNER`, `NAMES-FOUR`, `TASKID` `p3-t3`. Write FEATURE/evidence/regression-testing/mutation-null-owner-escape.md (fixed name) with the same field set and `## Mutated run` content as P3-T1 (mutated file QuickFiler/Viewers/ItemViewer.Breadcrumb.cs, hunk lines 435 to 438, `TOKEN System.Threading.SynchronizationContext.Current = 1`), `EXIT_CODE:` scoped to the mutated run, `ExpectedExitCode: 1`. Predicted failure (D-6): the test `Failed` at the `captured` null assertion with a `MESSAGE` line containing `InvalidOperationException` and `but found`; this is the discrimination the old `Task.Run` shape had only when the work item was not inlined. Acceptance, all five required: the census value is 1; `MSBUILD_EXIT_CODE: 0` and `PROD_CSC_OUT_LINES:` at least 1; `total` 1, `failed` 1; the `MESSAGE` line contains both `InvalidOperationException` and `but found`; `RUNSETTINGS-HASH-NOW:` equals `RUNSETTINGS-HASH:`. A passing run, or a `MESSAGE` naming `NullReferenceException`, is `MUTATION PREDICTION MISMATCH`: stop and report.

- [ ] [P3-T4] Revert control M2 and confirm, with the P3-T2 mechanics applied to QuickFiler/Viewers/ItemViewer.Breadcrumb.cs (`git checkout`, anchored `git diff --exit-code HEAD`, scoped porcelain, `CMD-CENSUS`, `CMD-BUILD-QF` with `TASKID` `p3-t4`, `CMD-VSTEST` with `ASSEMBLY-QF`, `FILTER-NULLOWNER`, `NAMES-FOUR`, `TASKID` `p3-t4`), appending `## Revert and confirming run` to FEATURE/evidence/regression-testing/mutation-null-owner-escape.md. Acceptance, all five required: `REVERT-DIFF-EXIT: 0`; the porcelain output is empty; `TOKEN System.Threading.SynchronizationContext.Current = 0`; `PROD_CSC_OUT_LINES:` at least 1; `CONFIRMING-RUN-EXIT: 0` with `total` 1, `passed` 1 and the `RESULT` line `Passed`.

- [ ] [P3-T5] [expect-fail] Control M3, precondition run inline: apply mutation M3 (Target Source G) to `QuickFiler.Test/TestSupport/DedicatedWorkerThread.cs`, run `CMD-CENSUS` on it, `CMD-BUILD-QF` with `TASKID` `p3-t5`, then `CMD-VSTEST` with `ASSEMBLY-QF`, `FILTER-FOUR`, `NAMES-FOUR`, `TASKID` `p3-t5`. Write FEATURE/evidence/regression-testing/mutation-inline-precondition.md (fixed name) with the P3-T1 field set (mutated file `QuickFiler.Test/TestSupport/DedicatedWorkerThread.cs`, hunk: `action();` inserted before `Exception captured = null;`, `TOKEN action(); = 2`, `CSC_OUT_LINES:` instead of the production line count), `EXIT_CODE:` scoped to the mutated run, `ExpectedExitCode: 1`. Predicted failure (D-6): all four tests `Failed` at their in-thread distinctness precondition, each `MESSAGE` line containing `dedicated worker thread must not be`. Acceptance, all five required: `TOKEN action(); = 2`; `MSBUILD_EXIT_CODE: 0` and `CSC_OUT_LINES:` at least 1; `total` 4, `failed` 4; each of the four `MESSAGE` lines contains `dedicated worker thread must not be`; `RUNSETTINGS-HASH-NOW:` equals `RUNSETTINGS-HASH:`. Any passing test is `MUTATION PREDICTION MISMATCH`: stop and report.

- [ ] [P3-T6] Revert control M3 and confirm, with the P3-T2 mechanics applied to `QuickFiler.Test/TestSupport/DedicatedWorkerThread.cs` (`git checkout`, anchored `git diff --exit-code HEAD`, scoped porcelain, `CMD-CENSUS`, SHA-256 equality with `FIX-HASH-HELPER:`, `CMD-BUILD-QF` with `TASKID` `p3-t6`, `CMD-VSTEST` with `ASSEMBLY-QF`, `FILTER-FOUR`, `NAMES-FOUR`, `TASKID` `p3-t6`), appending `## Revert and confirming run` to FEATURE/evidence/regression-testing/mutation-inline-precondition.md. Acceptance, all five required: `REVERT-DIFF-EXIT: 0`; the porcelain output is empty; `TOKEN action(); = 1` and the SHA-256 equals `FIX-HASH-HELPER:`; `CSC_OUT_LINES:` at least 1 with `DLL_ADVANCED: True`; `CONFIRMING-RUN-EXIT: 0` with `total` 4, `passed` 4 and all four `RESULT` lines `Passed`.

- [ ] [P3-T7] [expect-fail] Control M4, OpenRead sentinel replaced: apply mutation M4 (Target Source G) to `UtilitiesCS.Test/HelperClasses/FileInfoWrapper_Tests.cs`, run `CMD-CENSUS` on it, `CMD-BUILD-UCS` with `TASKID` `p3-t7`, then `CMD-VSTEST` with `ASSEMBLY-UCS`, `FILTER-OPENREAD`, `NAMES-FIW`, `TASKID` `p3-t7`. Write FEATURE/evidence/regression-testing/mutation-openread-sentinel.md (fixed name) with the P3-T5 field set (mutated file `UtilitiesCS.Test/HelperClasses/FileInfoWrapper_Tests.cs`, hunk: the `decoy` declaration and the `.Returns(decoy)` change, `TOKEN .Returns(decoy) = 1`, `TOKEN Assembly.Location = 8`, `TOKEN BeSameAs(sentinel) = 1`), `EXIT_CODE:` scoped to the mutated run, `ExpectedExitCode: 1`. Predicted failure (D-6): the test `Failed` at the same-instance assertion with a `MESSAGE` line containing `to refer to`. Acceptance, all five required: the three census values hold; `MSBUILD_EXIT_CODE: 0` and `CSC_OUT_LINES:` at least 1; `total` 1, `failed` 1; the `MESSAGE` line contains `to refer to`; `RUNSETTINGS-HASH-NOW:` equals `RUNSETTINGS-HASH:`. A passing run is `MUTATION PREDICTION MISMATCH`: stop and report.

- [ ] [P3-T8] Revert control M4 and confirm, with the P3-T6 mechanics applied to `UtilitiesCS.Test/HelperClasses/FileInfoWrapper_Tests.cs` (`git checkout`, anchored `git diff --exit-code HEAD`, scoped porcelain, `CMD-CENSUS`, SHA-256 equality with `FIX-HASH-FIW:`, `CMD-BUILD-UCS` with `TASKID` `p3-t8`, `CMD-VSTEST` with `ASSEMBLY-UCS`, `FILTER-OPENREAD`, `NAMES-FIW`, `TASKID` `p3-t8`), appending `## Revert and confirming run` to FEATURE/evidence/regression-testing/mutation-openread-sentinel.md. Acceptance, all five required: `REVERT-DIFF-EXIT: 0`; the porcelain output is empty; `TOKEN .Returns(decoy) = 0`, `TOKEN Assembly.Location = 7` and the SHA-256 equals `FIX-HASH-FIW:`; `CSC_OUT_LINES:` at least 1 with `DLL_ADVANCED: True`; `CONFIRMING-RUN-EXIT: 0` with `total` 1, `passed` 1 and the `RESULT` line `Passed`.

- [ ] [P3-T9] Prove the tree is clean after the controls: run `git diff --exit-code HEAD -- QuickFiler QuickFiler.Test UtilitiesCS UtilitiesCS.Test`, `git status --porcelain -- QuickFiler QuickFiler.Test UtilitiesCS UtilitiesCS.Test`, and the SHA-256 of each of the six Write Set code files, and write FEATURE/evidence/regression-testing/p3-t9-post-control-clean-tree.<TS>.md with `Timestamp:`, `Command:`, `EXIT_CODE:` (the `git diff --exit-code` exit) and an `Output Summary:` recording the porcelain output verbatim (`EMPTY` when nothing printed) and the six hashes beside their `FIX-HASH-` anchors. Acceptance, all three required: `EXIT_CODE: 0`; the porcelain output is empty; all six hashes equal their P2-T10 anchors. This is the mechanical proof, for AC11 through AC15, that no temporary production edit and no control residue survives into Phase 4.

### Phase 4 — Parallel-Suite Runs, Toolchain Loop, Coverage Comparison, Acceptance Check-Off and Commit

The loop is P4-T1 through P4-T7 in order. If any of those tasks fails its acceptance, or P4-T1 rewrites a file, the executor repairs the Write Set file within the Target Source contract (or reports a stop condition where a task names one) and restarts from P4-T1; P4-T10 records the iteration count. No task in this phase edits any file outside the Write Set.

- [ ] [P4-T1] Run the repository-wide formatter `dotnet tool run csharpier format .` from the worktree root inside one payload that also captures, immediately before and immediately after the command, the SHA-256 of each of the five Write Set `.cs` files and the SHA-256 of the text printed by `git diff MERGE-BASE -- QuickFiler.Test UtilitiesCS.Test` (the anchored patch, because the formatter can rewrite a file the plan already changed without changing a name list), then runs `git status --porcelain -- QuickFiler QuickFiler.Test UtilitiesCS UtilitiesCS.Test`. Write FEATURE/evidence/qa-gates/p4-t1-csharpier-format.<TS>.md with `Timestamp:`, `Command:`, `EXIT_CODE:` and an `Output Summary:` recording the tool's `Formatted N files in Xms.` line verbatim (a processed-file count, not a rewrite count), the ten file hashes, `REWRITTEN:` (the number of the five whose hash changed), `FORMAT_CHANGED_TREE:` (whether the two patch hashes differ) and the porcelain output verbatim. Acceptance, all three required: `EXIT_CODE: 0`; `FORMAT_CHANGED_TREE:` and `REWRITTEN:` are recorded; the porcelain output lists only paths in the Write Set (any other path was clean at P0-T6 and cannot have been rewritten by a correct pinned formatter: `FORMAT SCOPE BREACH`, stop and report). When `REWRITTEN:` is greater than 0 the loop restarts from this task after `CMD-CENSUS` confirms the `post` token table still holds for the rewritten files; a second consecutive iteration with `REWRITTEN:` greater than 0 is `FORMAT NOT IDEMPOTENT`: stop and report.

- [ ] [P4-T2] Run `dotnet tool run csharpier check .` from the worktree root and write FEATURE/evidence/qa-gates/p4-t2-csharpier-check.<TS>.md with `Timestamp:`, `Command:`, `EXIT_CODE:` and an `Output Summary:` quoting the `Checked N files in Xms.` line verbatim and recording `CHECKED-FILES:` and `CHECKED-DELTA:` as that N minus the P0-T6 `CHECKED-FILES:` value. Acceptance, both required: `EXIT_CODE: 0`; `CHECKED-DELTA:` is exactly 2 (this plan adds two `.cs` files and removes none; the project file is excluded by .csharpierignore). A non-zero exit restarts the loop from P4-T1.

- [ ] [P4-T3] Run the analyzer gate with `CMD-REBUILD` using the analyzer `GATEARGS` and `TASKID` `p4-t3` and write FEATURE/evidence/qa-gates/p4-t3-msbuild-analyzers.<TS>.md with the P0-T7 field set plus `WARNINGS-DELTA:` (this run's `WARNINGS:` minus `ANALYZE-BASELINE-WARNINGS:`, an observation). Acceptance, all five required (D-8): `EXIT_CODE: 0`; `ERRORS: 0`; `SKIP_CORECOMPILE_LINES: 0`; `QF_TEST_CSC_OUT_LINES:` and `UCS_TEST_CSC_OUT_LINES:` each at least 1; `WRITESET_DIAGNOSTIC_LINES: 0`. A diagnostic naming a Write Set file is repaired in that file within the Target Source contract and the loop restarts from P4-T1; any other failure is reported.

- [ ] [P4-T4] Run the nullable gate with `CMD-REBUILD` using the nullable `GATEARGS` and `TASKID` `p4-t4` (no Nullable property override, no incremental Build target) and write FEATURE/evidence/qa-gates/p4-t4-msbuild-nullable.<TS>.md with the P0-T8 field set plus `WARNINGS-DELTA:` against `NULLABLE-BASELINE-WARNINGS:`. Acceptance, all five required: `EXIT_CODE: 0`; `ERRORS: 0`; `SKIP_CORECOMPILE_LINES: 0`; both `_CSC_OUT_LINES:` at least 1; `WRITESET_DIAGNOSTIC_LINES: 0`. Same restart rule as P4-T3.

- [ ] [P4-T5] Run the full QuickFiler.Test assembly in the parallel regime (the measured run for AC1, AC2, AC4, AC6 and the QuickFiler half of AC16) with `CMD-VSTEST` using `ASSEMBLY-QF`, empty `FILTERARG`, `NAMES-QF` and `TASKID` `p4-t5`, and write FEATURE/evidence/regression-testing/parallel-suite-quickfiler-test.md (fixed name) with `Timestamp:`, `Command:` (naming TaskMaster.runsettings, the isolation switch and `no test-case filter`), `EXIT_CODE:` and an `Output Summary:` recording `RUNSETTINGS-HASH-NOW:`, the `COUNTERS` line, `RESULT_COUNT:`, `SEQUENCE_FILES:`, `COLLECTOR_LINES:`, every `RESULT` line (the eight `NAMES-QF` names), every `MESSAGE` line, and `NEWLY-FAILING:` (names in this run's failed set that are not in `BASELINE-FAILED-QF:`, or `NONE`). Acceptance, all six required: `EXIT_CODE: 0`; `failed` is 0 and `executed` is at least the P0-T10 QuickFiler `executed` value; `SEQUENCE_FILES: 0`; the eight `RESULT` lines are present and each reads `Passed` (this is the proof that the Part2 file and the helper compiled and were discovered, AC6); `RUNSETTINGS-HASH-NOW:` equals `RUNSETTINGS-HASH:`; `NEWLY-FAILING: NONE`. A failing test in a Write Set file restarts the loop from P4-T1 after repair; any other failure is `AC16: NOT MET`, reported with the `MESSAGE` lines, and the run stops.

- [ ] [P4-T6] Run the full UtilitiesCS.Test assembly in the parallel regime (the measured run for AC8, AC9, AC10 and the UtilitiesCS half of AC16) with `CMD-VSTEST` using `ASSEMBLY-UCS`, `UCS-FILTERARG` (P0-T9), `NAMES-FIW` and `TASKID` `p4-t6`, and write FEATURE/evidence/regression-testing/parallel-suite-utilitiescs-test.md (fixed name) with the P4-T5 field set; `Command:` records the filter verbatim (or `no test-case filter`) and, when the filter is present, the four excluded class names and the sentence that the stall is pre-existing on main and covered by CI. Acceptance, all six required: `EXIT_CODE: 0`; `failed` 0 and `executed` at least 1; `SEQUENCE_FILES: 0`; the eight `NAMES-FIW` `RESULT` lines are present and each reads `Passed`; `RUNSETTINGS-HASH-NOW:` equals `RUNSETTINGS-HASH:`; `NEWLY-FAILING: NONE`. Same failure handling as P4-T5.

- [ ] [P4-T7] Run the final repository-wide test and coverage pass by the route P0-T9 fixed (`CMD-COVERAGE-RUNNER` or `CMD-COVERAGE-DIRECT` with `STAGE` `final`, then `CMD-COVERAGE-POST` with `STAGE` `final` and the route's `RAW` value) and write FEATURE/evidence/qa-gates/coverage-final.md (fixed name) with the P0-T11 field set, plus `NEWLY-FAILING:` (names in `FAILED-SET:` not in the P0-T11 `FAILED-SET:`, or `NONE`). Branches: exit 0 completes; a non-zero exit whose failed set is exactly the #780 flake permits one identical re-run (D-12), both recorded; a runner threshold message with `DOCUMENT_PRESENT: True` is handled as in P0-T11 branch (c) and recorded as `FLOOR: NOT MET`; any other failure in a Write Set file restarts the loop from P4-T1 after repair, and any other failure stops with `AC17: NOT MET`. Acceptance, all five required: the projection block contains the `UtilitiesCS` and `QuickFiler` package elements with `LINE` and `BRANCH` counters; the `First-party coverage:` line is present; the summary block's first line begins `Test run outcome:` and `FAILED-SET:` is empty on the completing run; `NEWLY-FAILING: NONE`; the artifact contains no absolute path. coverage\final-931.jacoco.xml remains on disk for P4-T8.

- [ ] [P4-T8] Compare the final coverage projection against the baseline (AC18) by running `CMD-PACKAGE-COMPARE` and append to FEATURE/evidence/qa-gates/coverage-final.md a section `## Comparison against coverage-baseline.md` carrying the four `PACKAGE` lines verbatim, `BASELINE-FIRST-PARTY:` and `FINAL-FIRST-PARTY:` (the two `First-party coverage:` lines), and exactly one `COMPARABILITY:` line: `A` when the two first-party `lines` denominators differ by at most 1 percent of the baseline denominator, else `B` with the one-sentence note that the repository-wide totals were measured over different merged denominators and are recorded, not gated (D-7). Acceptance, all three required: no `PACKAGE` line reads `MISSING`; all four `PACKAGE` lines read `NOT-LOWER=True`; exactly one `COMPARABILITY:` line is present. When a `PACKAGE` line reads `NOT-LOWER=False`, run P4-T7 once more as a second measurement (identical command, recorded as `MEASUREMENT: 2` in coverage-final.md) and re-run this comparison; a second `False` is `AC18: NOT MET`: stop and report with both projections quoted.

- [ ] [P4-T9] Audit file size and the token table after the repository-wide format by running `CMD-CENSUS` on each of the five Write Set `.cs` files and `QuickFiler.Test/QuickFiler.Test.csproj`, and write FEATURE/evidence/qa-gates/p4-t9-post-format-census.<TS>.md transcribing every `TOKEN`, `LINES` and `SHA256` line per file. Acceptance, all three required: every `LINES` value for the five `.cs` files is at most 500 (AC5; a larger value is `FILE SIZE LIMIT EXCEEDED`: stop and report rather than shortening remarks); every `post` value of the Token Census Expectations table holds, including `action();` 1 in HELPER, `.Returns(decoy)` 0 and `Assembly.Location` 7 in FIW, INCLUDE-PART2 1 and INCLUDE-HELPER 1 (no control residue); each `.cs` `SHA256` equals its `FIX-HASH-` anchor when P4-T1 recorded `REWRITTEN: 0`, and otherwise the artifact records the new value under `POST-FORMAT-HASH-<n>:` with the reason.

- [ ] [P4-T10] Close the toolchain loop and write FEATURE/evidence/qa-gates/toolchain-pass.md (fixed name) with `Timestamp:`, `Command:` (`reconciliation of P4-T1 through P4-T9`), `EXIT_CODE: 0` (scoped to the reconciliation), and an `Output Summary:` carrying one line per toolchain command of the final clean iteration in CLAUDE.md order, each naming the command, its exit code and its artifact: `dotnet tool run csharpier check .` (P4-T2), the analyzer rebuild (P4-T3) with its `SKIP_CORECOMPILE_LINES:` value, the nullable rebuild (P4-T4) with its `SKIP_CORECOMPILE_LINES:` value, and the MSTest-with-coverage route (P4-T7) with `COVERAGE-ROUTE:`; then the two parallel-suite runs (P4-T5, P4-T6); `ITERATIONS:` (the number of times the loop started from P4-T1); one `EXPECTATION-MET:` line per task P4-T1 through P4-T9; and `LOOP: CLEAN PASS`. Acceptance, all four required: the four toolchain lines each record exit 0; both `SKIP_CORECOMPILE_LINES:` values are 0; nine `EXPECTATION-MET:` lines each read `YES`; `LOOP: CLEAN PASS` is present. If any expectation is not met the loop restarts from P4-T1 and this task is re-run after it; nothing is written until every expectation holds.

- [ ] [P4-T11] Verify the footprint and scope boundary (AC3, AC15, AC16's diff clause) and write FEATURE/evidence/qa-gates/p4-t11-scope-boundary.<TS>.md. Run: `git diff --name-only MERGE-BASE` (working tree); `git status --porcelain --untracked-files=all`; `git diff --name-only MERGE-BASE -- QuickFiler.Test`; `git diff --name-only MERGE-BASE -- .claude`; `git diff --exit-code MERGE-BASE -- QuickFiler UtilitiesCS TaskMaster.runsettings scripts/vscode/TaskMaster.cli.runsettings config`; `git diff -U0 MERGE-BASE -- QuickFiler.Test UtilitiesCS.Test`; and the SHA-256 of TaskMaster.runsettings. Record the two full path lists verbatim; `INHERITED-AND-EXCLUDED:` (the paths removed by Clause A and Clause B, each listed with its clause); `THIS-ITEM-FOOTPRINT:` (the union of the first two lists minus those); `QUICKFILER-TEST-CHANGED:` (the third list); `CLAUDE-CHANGED:` (the fourth list); `PRODUCTION-AND-CONFIG-DIFF-EXIT:`; over the added lines (single leading `+`) of the `-U0` diff, `ADDED-DONOTPARALLELIZE:`, `ADDED-THREAD-SLEEP:`, `ADDED-TASK-DELAY:`, `ADDED-TIMEOUT:` (lines containing `[Timeout` or `Timeout=`), `ADDED-RETRY:` (lines containing `Retry`), `ADDED-WORKERS:` (lines containing `Workers`); and `RUNSETTINGS-HASH-NOW:`. Acceptance, all seven required: `THIS-ITEM-FOOTPRINT:` is exactly the six Write Set code paths plus paths under docs/features/active/2026-09-28-tests-depend-on-uncontrolled-environment-931/ and nothing else; `QUICKFILER-TEST-CHANGED:` is exactly the four QuickFiler.Test Write Set paths, so none of the eight files AC3 names and neither `[DoNotParallelize]` carrier changed; every path in `CLAUDE-CHANGED:` begins .claude/agent-memory/ (Clause B; agent memory writes are not this item's footprint); `PRODUCTION-AND-CONFIG-DIFF-EXIT: 0`; the six `ADDED-` counts are all 0; `RUNSETTINGS-HASH-NOW:` equals `RUNSETTINGS-HASH:`; the porcelain span is present as the name-listing diff's companion.

- [ ] [P4-T12] Sweep the feature folder for host identifiers and raw documents (AC19) by running `CMD-SWEEP` and write FEATURE/evidence/qa-gates/p4-t12-hygiene-sweep.<TS>.md with `Timestamp:`, `Command:`, `EXIT_CODE:` and an `Output Summary:` recording `FILES:`, `ACCOUNT-TOKEN-MATCHES:`, `PROFILE-LEAF-MATCHES:`, `MACHINE-TOKEN-MATCHES:`, `WORKTREE-ROOT-MATCHES:`, `USERS-PATH-MATCHES:` and `RAW-DOCUMENT-FILES:`; the tokens themselves are never written. Also run `git diff --name-only MERGE-BASE` with `git status --porcelain --untracked-files=all` and record `RAW-EXTENSION-PATHS:` as every listed path ending `.trx`, `.xml` or `.coverage` outside `INHERITED-CLAUSE-A:`, or `NONE`. Acceptance, all three required: the six match counts are 0; `RAW-DOCUMENT-FILES: 0`; `RAW-EXTENSION-PATHS: NONE`. A non-zero count is repaired by replacing the offending text with `<repo-root>`, `<user-profile>`, `<user>` or `<host>` in the artifact that carries it (never by deleting the artifact), or by removing a raw document from the feature folder, and the sweep is re-run until every count is 0.

- [ ] [P4-T13] Write the follow-up handoff record FEATURE/evidence/qa-gates/p4-t13-follow-up-handoff.<TS>.md for the orchestrator, who files the potential entries through the promotion lifecycle's MCP tool (D-9; the executor creates nothing under docs/features/potential/). The record carries `Timestamp:` and, for each of the four follow-ups in the spec's Rollout list, a `short_name`, a one-paragraph body with the spec's file and line citations, and the sentence `not fixed under #931`: (1) `physicalfilesystemadapters-tests-open-repository-solution-file` (the same `GetSolutionFile` pattern, the swallowed `IOException` catches and the real solution-file opens in UtilitiesCS.Test/HelperClasses/PhysicalFileSystemAdapters_Tests.cs); (2) `directoryinfowrapper-tests-enumerate-repository-solution-file` (UtilitiesCS.Test/HelperClasses/DirectoryInfoWrapper_Tests.cs lines 60, 79 and 381); (3) `breadcrumb-dispatchvalue-message-wording-broader-than-mechanism` (QuickFiler.Test/Viewers/BreadcrumbUiThreadDispatchTests.cs line 305, wording only); (4) `issue-900-handoff-misattributes-dispatchvalue-site-to-owner-thread-id-check` (documentation correction to the #900 follow-up handoff record). Acceptance, all three required: the record exists; it names the four `short_name` values exactly once each; it contains the literal `not fixed under #931` at least four times.

Check-off protocol for P4-T14 through P4-T32: each task changes exactly one line of `docs/features/active/2026-09-28-tests-depend-on-uncontrolled-environment-931/spec.md`, the line beginning `- [ ] ACn. `, to begin `- [x] ACn. `, preserving the criterion text; its acceptance is that exactly one line begins `- [x] ACn. `, no line begins `- [ ] ACn. `, and no other AC line changed in that task. When the named evidence does not hold, the box is left unchecked and `ACn: NOT MET` is recorded with the failing value in FEATURE/evidence/qa-gates/p4-t<k>-ac<n>-checkoff.<TS>.md; the task completes in either case.

- [ ] [P4-T14] Check off AC1 in FEATURE/spec.md. Evidence: P4-T9 census of BND (`Task.Run(` 0, `.GetAwaiter()` 3 down from 4, `DedicatedWorkerThread.Run(` 1, `ownerThreadId` 2, `.NotBe(` 1, `.BeNull(` 1, `executions.Should().Be(0)` 1, `cannot marshal` 1), P3-T1 (fails at the executions assertion when the guard is disabled) and P4-T5 (`Dispatcher_OwnerOnlyWorker_ReportsWithoutRunningAction` Passed).

- [ ] [P4-T15] Check off AC2 in FEATURE/spec.md. Evidence: P4-T9 census of PART2 (`Task.Run(` 0, `.GetAwaiter()` 0, `DedicatedWorkerThread.Run(` 3, `owner.CheckAccess()` 1, `.BeNull(` 1, `unconditionally` 1), P3-T3 (fails at the captured null assertion under the restored context-reference guard) and P4-T5 (`InitializeBreadcrumbPipeline_NullOwningDispatcher_DoesNotThrow` Passed).

- [ ] [P4-T16] Check off AC3 in FEATURE/spec.md. Evidence: P4-T11 (`QUICKFILER-TEST-CHANGED:` is exactly the four QuickFiler.Test Write Set paths; `ADDED-DONOTPARALLELIZE: 0`).

- [ ] [P4-T17] Check off AC4 in FEATURE/spec.md. Evidence: P4-T9 census of AFF (`partial class` 1, `[TestMethod]` 4, `RunOnDedicatedWorkerThread` 0, `ClearViewerDispatcher(` 0) and PART2 (`partial class` 1, `[TestMethod]` 3, `ClearViewerDispatcher(` 2, `RunOnDedicatedWorkerThread` 0), and P4-T5 (the seven `NAMES-AFFINITY` names present and Passed, the same seven P0-T10 recorded, so no test was renamed or removed).

- [ ] [P4-T18] Check off AC5 in FEATURE/spec.md. Evidence: P4-T9 (`LINES` at most 500 for each of the five `.cs` files, measured after the repository-wide format).

- [ ] [P4-T19] Check off AC6 in FEATURE/spec.md. Evidence: P4-T9 census of CSPROJ (INCLUDE-PART2 1, INCLUDE-HELPER 1) and P4-T5 (the four named tests present and Passed in the committed QuickFiler.Test projection).

- [ ] [P4-T20] Check off AC7 in FEATURE/spec.md. Evidence: P4-T9 census of HELPER (`namespace QuickFiler.Test.TestSupport` 1, `internal static class DedicatedWorkerThread` 1, `internal static Exception Run(Action action)` 1, `new Thread(` 1, `IsBackground = true` 1, `thread.Join();` 1, `Join(` equals `Join()`, `distinct from every live thread by construction` 1, `.Should()` 0, `Thread.Sleep` 0, `Task.Delay` 0, `[Timeout` 0, `Retry` 0), the `DedicatedWorkerThread.Run(` counts 3 (PART2) plus 1 (BND), and P3-T5 (all four tests fail together when the helper runs the delegate inline).

- [ ] [P4-T21] Check off AC8 in FEATURE/spec.md. Evidence: P4-T9 census of FIW (`GetSolutionFile` 0, `TaskMaster.sln` 0, `AppDomain` 0, `File.Create` 0, `File.WriteAll` 0, `File.Delete` 0, `Path.GetTemp` 0, `FileMode.` equal to `FileMode.Open`, `FileAccess.` equal to `FileAccess.Read`) and P4-T6 (the four named tests present and Passed).

- [ ] [P4-T22] Check off AC9 in FEATURE/spec.md. Evidence: P4-T9 census of FIW (`using var sentinel = new FileStream(` 1, `Assembly.Location` 7, `FileShare.ReadWrite` 9, `BeSameAs(sentinel)` 1, `stream.CanRead.Should().BeTrue()` 1, `stream.Length.Should().BeGreaterThan(0)` 1), P3-T7 (fails at the same-instance assertion under a second stream) and P4-T6 (`OpenRead_ShouldReturnReadableStreamForWrappedFile` Passed).

- [ ] [P4-T23] Check off AC10 in FEATURE/spec.md. Evidence: P2-T6 (`ADDED-FIXTUREPATH: 4`, `ADDED-LENGTH: 1`, `ADDED-OPENREAD: 2`, `ADDED-OPEN-CREATE-WRITE: 0`, so the three metadata tests use the rooted literal, open no stream and assert none of the excluded members) and P4-T6 (the three tests Passed).

- [ ] [P4-T24] Check off AC11 in FEATURE/spec.md. Evidence: FEATURE/evidence/regression-testing/mutation-owner-only-dispatcher-guard.md (P3-T1 mutated run failing on `to be 0, but found 1` with the mutated file and hunk, the filter, the runsettings name and the isolation switch recorded; P3-T2 revert with `REVERT-DIFF-EXIT: 0`, empty porcelain and the confirming pass).

- [ ] [P4-T25] Check off AC12 in FEATURE/spec.md. Evidence: FEATURE/evidence/regression-testing/mutation-null-owner-escape.md (P3-T3 and P3-T4, with the same recorded fields).

- [ ] [P4-T26] Check off AC13 in FEATURE/spec.md. Evidence: FEATURE/evidence/regression-testing/mutation-inline-precondition.md (P3-T5: all four `MESSAGE` lines carry the precondition reason; P3-T6 revert and confirming pass).

- [ ] [P4-T27] Check off AC14 in FEATURE/spec.md. Evidence: FEATURE/evidence/regression-testing/mutation-openread-sentinel.md (P3-T7 failing on `to refer to`; P3-T8 revert and confirming pass).

- [ ] [P4-T28] Check off AC15 in FEATURE/spec.md. Evidence: P4-T11 (`THIS-ITEM-FOOTPRINT:` equals the six Write Set code paths plus feature-folder documents; `PRODUCTION-AND-CONFIG-DIFF-EXIT: 0`; every `CLAUDE-CHANGED:` path is agent memory outside this item's footprint) and P3-T9 (no control residue).

- [ ] [P4-T29] Check off AC16 in FEATURE/spec.md. Evidence: FEATURE/evidence/regression-testing/parallel-suite-quickfiler-test.md and FEATURE/evidence/regression-testing/parallel-suite-utilitiescs-test.md (P4-T5 and P4-T6: exit 0, `failed` 0, the runsettings file, the isolation switch and the filter recorded verbatim, `RUNSETTINGS-HASH-NOW:` unchanged) and P4-T11 (the six `ADDED-` counts 0).

- [ ] [P4-T30] Check off AC17 in FEATURE/spec.md. Evidence: FEATURE/evidence/qa-gates/toolchain-pass.md (P4-T10: `LOOP: CLEAN PASS`, four toolchain commands at exit 0, both `SKIP_CORECOMPILE_LINES:` 0).

- [ ] [P4-T31] Check off AC18 in FEATURE/spec.md. Evidence: FEATURE/evidence/baseline/coverage-baseline.md and FEATURE/evidence/qa-gates/coverage-final.md (P0-T11, P4-T7 and P4-T8: both projections with the `First-party coverage:` line, four `PACKAGE` lines `NOT-LOWER=True`).

- [ ] [P4-T32] Check off AC19 in FEATURE/spec.md. Evidence: P4-T12 (six match counts 0, `RAW-DOCUMENT-FILES: 0`, `RAW-EXTENSION-PATHS: NONE`).

- [ ] [P4-T33] Commit the final state, write the closure record, and leave the source and feature trees clean. Steps, in order: (1) run `CMD-SWEEP` once more and stop with `HYGIENE SWEEP FAILED` if any count is non-zero; (2) run `git add -- QuickFiler.Test UtilitiesCS.Test docs/features/active/2026-09-28-tests-depend-on-uncontrolled-environment-931` then `git commit -m "docs(931): evidence, acceptance check-off and plan state for the uncontrolled-environment test fix"` (a second `-m` paragraph for the session's attribution trailer if its harness requires one), recording `COMMIT-1-EXIT:`; (3) write FEATURE/evidence/qa-gates/p4-t33-closure.<TS>.md with `Timestamp:`, `Command:` (the git spans of this task), `COMMIT-1-EXIT:`, `HEAD-AFTER-COMMIT:` (`git rev-parse HEAD`), the confirming footprint lists `git diff --name-only MERGE-BASE..HEAD -- QuickFiler QuickFiler.Test UtilitiesCS UtilitiesCS.Test` and `git diff --name-only origin/main...HEAD -- QuickFiler QuickFiler.Test UtilitiesCS UtilitiesCS.Test` (each must list exactly the six Write Set code paths; the step-2 `git add` span is their companion), the pointer to P4-T13, and the acceptance-criteria status summary in the exact form the acceptance-criteria-tracking skill requires (`Source:` FEATURE/spec.md, `Total AC items: 19`, `Checked off (delivered): <n>` counted from the `- [x] AC` lines on disk, `Remaining (unchecked): <19 minus n>`, `Items remaining:` listing any unchecked criterion text or `none`); (4) tick every remaining unchecked checkbox in `docs/features/active/2026-09-28-tests-depend-on-uncontrolled-environment-931/plan.2026-09-28T20-01.md`, including this task's own; (5) run `git add -- docs/features/active/2026-09-28-tests-depend-on-uncontrolled-environment-931` then `git commit --amend --no-edit`, recording `COMMIT-2-EXIT:` in the executor's final message (the artifact cannot record it); (6) run `git status --porcelain -- QuickFiler QuickFiler.Test UtilitiesCS UtilitiesCS.Test docs/features/active/2026-09-28-tests-depend-on-uncontrolled-environment-931` and `git rev-parse HEAD`; (7) append `EXIT_CODE:` (scoped to the step-6 porcelain span), `POST-AMEND-PORCELAIN:` (`EMPTY` or the lines verbatim) and `FINAL-HEAD:` to the closure record; (8) run `git add -- docs/features/active/2026-09-28-tests-depend-on-uncontrolled-environment-931` then `git commit --amend --no-edit` once more, and write nothing afterwards. Paths under .claude/agent-memory/ are outside every span above by pathspec and are not this task's residual. Acceptance, all five required: `COMMIT-1-EXIT: 0`; both confirming footprint lists are exactly the six Write Set code paths; the status summary reports `Total AC items: 19` and its `Checked off` count equals the number of `- [x] AC` lines in FEATURE/spec.md; `POST-AMEND-PORCELAIN: EMPTY`; the executor's final message reports the plan path, `FINAL-HEAD:`, `COMMIT-2-EXIT:`, the status summary and the P4-T13 follow-up pointer. A PreToolUse refusal at any git span is `PRE-IMPLEMENTATION GATE BLOCKED`: stop and report the refusal text verbatim; do not alter hooks, checkpoints, permission configuration or another item's files.

