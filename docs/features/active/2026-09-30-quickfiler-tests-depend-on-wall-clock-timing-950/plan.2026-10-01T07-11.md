# 2026-09-30-quickfiler-tests-depend-on-wall-clock-timing (Plan)

- **Issue:** #950
- **Parent (optional):** none
- **Owner:** drmoisan
- **Work Mode:** full-bug (acceptance criteria come from `docs/features/active/2026-09-30-quickfiler-tests-depend-on-wall-clock-timing-950/spec.md` only; no user story exists for this item and none is to be authored)
- **Last Updated:** 2026-10-01T07-11
- **Status:** Ready for preflight (revision round 1 applied)
- **Version:** 1.1
- **Revision R1:** applies executor preflight round 1 defects 1, 4, 5+6, 7 and 8 and the orchestrator's spec change committed at e3827fb2c (the R4 baseline pin is taken inside the gate, which supersedes preflight defects 2 and 3). Section "Revision R1 change log" below lists every change.
- **Plan path continuity:** this file is updated in place for every preflight revision round. No timestamped sibling plan file is created for this cycle.

**Fail-closed evidence rule:** every command-bearing task writes one evidence artifact carrying `Timestamp:`, `Command:`, `EXIT_CODE:` and `Output Summary:`. A task whose artifact is missing or incomplete stays unchecked, and the plan outcome is BLOCKED or INCOMPLETE, never PASS.

**Evidence accounting rule:** the artifact path is named in the task text. Do not mark an evidence-bearing task complete without the artifact on disk at that exact path.

**Evidence location:** every artifact lives under `docs/features/active/2026-09-30-quickfiler-tests-depend-on-wall-clock-timing-950/evidence/` in the canonical sub-kinds `baseline/`, `regression-testing/`, `qa-gates/` and `other/`. EVIDENCE_LOCATION_OVERRIDE_REJECTED: none supplied; no artifacts-tree evidence path appears in this plan. In task text the token FEATURE abbreviates `docs/features/active/2026-09-30-quickfiler-tests-depend-on-wall-clock-timing-950`; the Write Set spells every path in full.

## Caller instructions applied with a recorded adjustment

Three delegation instructions are applied in an adjusted form. Each adjustment keeps the instruction's purpose and is stated here so a reviewer does not read it as an omission.

1. **Absolute worktree path in `Command:` rows.** The caller asked that each artifact's `Command:` row record the full prefixed payload. The prefix carries the absolute worktree path, which contains the account's profile directory. The repository hygiene guard (`scripts/hygiene/Test-RepositoryHygiene.Rules.ps1`, function `Get-UserProfilePathPattern`, line 21) rejects any committed line matching a drive-letter user-profile path, and both this plan and every evidence artifact are committed. Every `Command:` row therefore records the full payload with the literal token `WORKTREE` in place of the absolute path, and every payload prints `WORKTREE-LEAF:` followed by the leaf name of its working directory. The acceptance condition for the prefix is that the recorded payload begins with the PREFIX lines of the Command Reference and that `WORKTREE-LEAF: agent-a7805823735145ca4` is recorded; a payload that ran in the session checkout prints a different leaf and fails that condition.
2. **Quote character of the prefix.** The caller's prefix uses single quotes around the path. Payloads containing `$` are passed in outer single quotes (`pwsh -NoProfile -Command '...'`), and the Bash channel cannot carry a single quote inside a single-quoted argument. The prefix is therefore written `Set-Location -LiteralPath "WORKTREE"`; PowerShell treats a double-quoted literal path that contains no `$` and no backtick identically to the single-quoted form.
3. **Location of negative-control records.** The caller places negative-control runs under `evidence/regression-testing/`; spec AC15 requires the outcomes as a Markdown summary under `evidence/other/`. Both are produced: one run artifact per control batch under `regression-testing/`, and the AC15 summary under `other/`.

## Revision R1 change log

- Spec change (R4 pin inside the gate): Delivered Source R-BODY re-authored (four lines inserted after pre-edit line 214, pre-edit lines 215 to 258 re-indented, one closing line inserted after pre-edit line 258); R-DOC re-authored (cause, inside-the-gate rationale with W3/W4, W2/W5 invariant; `Issue #950:` once); the R-BODY gate token changed to a form that holds whichever way CSharpier lays out the `using` header; three new span anchors (`R4PRE`, `R4HEAD`, `R4TAIL`) prove the placement; fact 5, P0-T12, P3-T12, P3-T13, P3-T15, P5-T8, P5-T9 and P6-T24 updated; new section "Risks".
- Defect 1: L3 replaced with the executor's text (release source created with `TaskCreationOptions.RunContinuationsAsynchronously`), extended to also replace the stale test 2 summary (defect 8); D-2 appended; P3-T3 acceptance appended; census token list and rows updated; new negative-control batch C (P5-T18 to P5-T23: delete `pump.Drain();` in tests 3 and 4) proves the two tests depend on `Drain()`; former P5-T18 and P5-T19 are now P5-T24 and P5-T25.
- Defect 4: P6-T10 acceptance replaced.
- Defects 5 and 6: the "Long-running payloads" convention replaced; both coverage payloads end with `PAYLOAD-COMPLETE` (verified in their text).
- Defect 7: P3-T1 and P3-T6 acceptance replaced.
- Defect 8: liveness test 2 summary rewritten inside L3; zero-batch class remarks rewritten by new task P3-T14 (Delivered Source Z-R); the census formerly numbered P3-T14 is now P3-T15, and every reference to it is renumbered.

## Revision R2 change log (orchestrator re-anchor at execution start, standing authority)

- Cause: the execution run merged origin/main into the branch as its first action (merge commit `b8fcc0f8a`). After that merge, `git merge-base origin/main HEAD` prints `34c2ed88cbb009f2f231453db87bc64d45a9bd51`, so the P0-T3 checks against the original anchor would stop with `BASE-SHA MISMATCH`, `INHERITED SET OUT OF SCOPE` and `CITED TREE ADVANCED` although no cited file changed.
- Change: every occurrence of the original anchor in a command, in D-9 and in the Execution conventions `BASE` token (thirteen occurrences) is replaced by `34c2ed88cbb009f2f231453db87bc64d45a9bd51`. Fact 11 keeps the original cut point as history in its abbreviated form. No task, acceptance criterion, Delivered Source block or line citation changes.
- Citation validity: `git diff --stat 9b3eea584 34c2ed88c` over QuickFiler/Controllers, QuickFiler.Test/Controllers, QuickFiler.Test/SetupAssemblyInitializer.cs, both QuickFiler project files, QuickFiler.Test/packages.config, UtilitiesCS/Threading, scripts, TaskMaster.runsettings, .csharpierignore, global.json and dotnet-tools.json prints nothing, so every line citation made against the original anchor holds at the new one. The only .gitignore change on main is one line appended at 258, below the cited lines 146, 150 and 151.
- Negative control: the same P0-T3 commands run against the original anchor fail (merge-base differs; the name-status list contains main's paths outside FEATURE), so the re-anchored checks still discriminate. Against the new anchor, the name-status list before execution is exactly the five FEATURE files plus the promoted record.

## Requirement sources

- Acceptance criteria: `docs/features/active/2026-09-30-quickfiler-tests-depend-on-wall-clock-timing-950/spec.md`, section `## Acceptance Criteria`, lines 266 to 282: seventeen checkbox lines `- [ ] AC1:` through `- [ ] AC17:`, each on one line. The check-off edit changes only `- [ ] ACn:` to `- [x] ACn:`.
- Design record: `docs/features/active/2026-09-30-quickfiler-tests-depend-on-wall-clock-timing-950/research/2026-10-01T00-00-wall-clock-waits-research.md` (sections 2.3, 2.5, 3.3, 3.4 and 3.5 govern this plan; read-only).
- Issue metadata: `docs/features/active/2026-09-30-quickfiler-tests-depend-on-wall-clock-timing-950/issue.md` carries `- Work Mode: full-bug` at line 12 and no acceptance-criteria section. It is not an acceptance-criteria source for this cycle.

## Write Set (every file this plan creates or modifies)

Code files (the only paths outside the feature folder this plan may change; spec "Files/modules to change"):

- `QuickFiler/Controllers/QfcDatamodel.cs`
- `QuickFiler.Test/Controllers/QfcDatamodelLivenessTests.cs`
- `QuickFiler.Test/Controllers/QfcDatamodelTeardownTests.cs`
- `QuickFiler.Test/Controllers/QfcInitEmailQueueZeroBatchTests.cs`
- `QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixtureTests.cs`

No project file changes: every edited file is already listed in its project file (fact 9), and no new source file is created. The helpers are private nested types in each test file that needs them (spec helper placement decision), so the spec's file list needs no amendment.

Feature documents:

- `docs/features/active/2026-09-30-quickfiler-tests-depend-on-wall-clock-timing-950/spec.md` (acceptance-criteria check-off edits only)
- `docs/features/active/2026-09-30-quickfiler-tests-depend-on-wall-clock-timing-950/plan.2026-10-01T07-11.md` (task check-off edits only)

Evidence files, all new. Every name is fixed except the fail-before exception dossier, which carries the conventional timestamped name; the write time is the `Timestamp:` field (yyyy-MM-ddTHH-mm):

- `FEATURE/evidence/baseline/`: `phase0-instructions-read.md`, `scope-and-anchor.md`, `bootstrap-sdk.md`, `bootstrap-tool-restore.md`, `bootstrap-nuget-restore.md`, `analyzer-alignment.md`, `bootstrap-dotnet-coverage.md`, `csharpier-check-baseline.md`, `msbuild-analyzer-baseline.md`, `msbuild-nullable-baseline.md`, `census-baseline.md`, `targets-baseline.md`, `concurrent-classes-baseline.md`, `stall-probe.md`, `coverage-baseline.md`, `phase0-commit.md`
- `FEATURE/evidence/regression-testing/`: `phase1-temporary-edits.md`, `phase1-build.md`, `r4-fail-before.md`, `phase1-revert.md`, `fail-before-exception.<timestamp>.md` (exactly one), `pass-after-build.md`, `targets-pass-after.md`, `concurrent-classes-pass-after.md`, `controls-a-edits.md`, `controls-a-build.md`, `negative-controls-batch-a.md`, `controls-a-revert.md`, `controls-b-edits.md`, `controls-b-build.md`, `negative-controls-batch-b.md`, `controls-b-revert.md`, `controls-c-edits.md`, `controls-c-build.md`, `negative-controls-batch-c.md`, `controls-c-revert.md`
- `FEATURE/evidence/qa-gates/`: `production-seam-census.md`, `test-rewrite-census.md`, `scoped-format.md`, `post-format-census.md`, `implementation-commit.md`, `csharpier-format.md`, `csharpier-check-final.md`, `msbuild-analyzer-final.md`, `msbuild-nullable-final.md`, `coverage-post-change.md`, `coverage-comparison.md`, `toolchain-final-pass.md`, `wall-clock-tokens.md`, `prohibited-constructs.md`, `footprint-scope.md`, `evidence-hygiene.md`, `final-commit.md`
- `FEATURE/evidence/other/`: `ambient-synchronization-context.md`, `negative-controls-summary.md`, `ac-status-summary.md`

Files this plan must not touch, stated so the executor fails closed rather than infers: QuickFiler.Test/QuickFiler.Test.csproj, QuickFiler/QuickFiler.csproj, QuickFiler.Test/Controllers/QfcItemController.FocusAndThemeTests.cs, QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixture.cs, QuickFiler.Test/Controllers/QfcItemController.TestSupport.cs, UtilitiesCS/Threading/UiThread.cs, TaskMaster.runsettings, scripts/vscode/TaskMaster.cli.runsettings, every file under scripts/, every file under .github/, every file under .claude/ (uncommitted .claude/agent-memory/ files are session memory and are never staged by this plan), every file under docs/features/potential/, and the research document. No raw test-result document (trx), raw coverage document (cobertura, coverage, coveragexml) or msbuild log is copied into the feature folder under any name; raw documents stay under the repository coverage directory, which .gitignore line 150 ignores.

## AC identity table

Each ID names one checkbox in the spec's `## Acceptance Criteria` section, in document order.

| ID | Spec line | Opening words of the criterion | Evidence read by its check-off task |
|---|---|---|---|
| AC1 | 266 | QfcDatamodel declares an internal WorkerStarter property | `qa-gates/post-format-census.md` |
| AC2 | 267 | InitEmailQueue contains no direct RunWorkerAsync call | `qa-gates/post-format-census.md` |
| AC3 | 268 | QuickFiler/Controllers/QfcDatamodel.cs stays at or below five hundred total lines | `qa-gates/post-format-census.md` |
| AC4 | 269 | No SpinWait.SpinUntil call, no Task.Wait call, and no WaitForState helper | `qa-gates/wall-clock-tokens.md` |
| AC5 | 270 | No Thread.Sleep, Task.Delay, [DoNotParallelize], retry construct, or [Timeout] value change | `qa-gates/prohibited-constructs.md` |
| AC6 | 271 | DequeueNextItemGroupAsync_WhileLoaderStillProducing_KeepsPollingAfterWorkerIdle passes | `regression-testing/targets-pass-after.md`, `qa-gates/coverage-post-change.md`, `qa-gates/post-format-census.md` |
| AC7 | 272 | RemainingLoadActive_AcrossAsyncVoidFirstAwait_StaysTrueWhileLoaderProduces passes | same three artifacts |
| AC8 | 273 | RemainingLoadActive_AfterLoaderCompletes_BecomesFalse passes | same three artifacts |
| AC9 | 274 | RemainingLoadActive_WhenLoaderThrows_IsStillClearedByFinally passes | same three artifacts |
| AC10 | 275 | Worker_DoWork_CapturesRemainingLoadTask passes | same three artifacts |
| AC11 | 276 | InitEmailQueue_ZeroBatchSize_StillStartsBackgroundWorker passes | same three artifacts |
| AC12 | 277 | the two other zero-batch-file tests both assign a synchronous WorkerStarter and pass | same three artifacts |
| AC13 | 278 | Transaction_SecondCallerCannotInstallUntilTheFirstRestores opens a using over EnsureUiThreadDispatcher after transaction A acquires the gate | same three artifacts plus `regression-testing/concurrent-classes-pass-after.md` |
| AC14 | 279 | The R4 doc comment no longer carries the flake-watch instruction | `qa-gates/post-format-census.md` |
| AC15 | 280 | Each test named in AC6 through AC13 has a recorded negative control | `other/negative-controls-summary.md` |
| AC16 | 281 | The observed ambient SynchronizationContext.Current value is recorded once | `other/ambient-synchronization-context.md` |
| AC17 | 282 | The full C# toolchain passes in a single pass in order | `qa-gates/toolchain-final-pass.md` |

## Verified tree facts (re-derived against this worktree while authoring)

Line totals below are content-line counts (the count a line-oriented reader returns for a file ending in a newline).

1. `QuickFiler/Controllers/QfcDatamodel.cs` is 483 lines. `[ExcludeFromCodeCoverage]` at 25 on `public partial class QfcDatamodel` at 26. The private constructor 34 to 41 assigns `RemainingEmailLoader = LoadRemainingEmailsToQueueAsync;` at 40; the public constructor 43 to 52 assigns it at 51; that literal occurs exactly twice. `RemainingEmailLoader` property documentation 126 to 139, declaration `internal Func<CancellationToken, Task<bool>> RemainingEmailLoader { get; set; }` at 140, blank 141, `#endregion Private Variables` at 142. `SetupWorker` 176 to 183 subscribes `Worker_DoWork` at 181. `Worker_DoWork` 185 to 229 (`async void`): loader call 205, capture 206, `e.Result = await loaderTask;` 207, `finally` 209 to 216 clears `_remainingLoadActive` at 215, `catch` 225 to 228. `InitEmailQueue` 259 to 303: zero-batch branch 267 to 275 with `_remainingLoadActive = true;` 272 and `worker.RunWorkerAsync();` 273; positive-batch `_remainingLoadActive = true;` 299 and `worker.RunWorkerAsync();` 300. `InitEmailQueueAsync` begins at 305. The file has exactly two lines whose trimmed text is `worker.RunWorkerAsync();` and no occurrence of `WorkerStarter`. Usings include System, System.ComponentModel (5) and Microsoft.Office.Interop.Outlook (14); the Outlook namespace declares a non-generic `Action`, which does not collide with the generic `Action<BackgroundWorker>` because C# name lookup matches type arity.
2. `QuickFiler.Test/Controllers/QfcDatamodelLivenessTests.cs` is 255 lines, namespace `QuickFiler.Controllers.Tests`, class `QfcDatamodelLivenessTests` at 26. `WaitForState` documentation and body 47 to 57 with `SpinWait.SpinUntil(condition, TimeSpan.FromSeconds(5))` at 56. Test 1 `DequeueNextItemGroupAsync_WhileLoaderStillProducing_KeepsPollingAfterWorkerIdle` 79 to 138: `new BackgroundWorker()` 97, `InitEmailQueue` 100, `.Task.Wait(TimeSpan.FromSeconds(5))` 103, `WaitForState` 106 to 110, `fake.Advance` 113, 115 and 130. `ReadLivenessFlag` 140 to 146. `StartHeldOpenLoader` documentation 148 to 152, method 153 to 181 (`new BackgroundWorker()` 169, `InitEmailQueue` 170, wait 173, `WaitForState` 176 to 179). Blank line 182. Test 2 183 to 205: summary 183 to 187 (stale after the rewrite: line 185 says `BackgroundWorker.IsBusy` "has already gone false"), `release.SetResult(true);` 204. Test 3 207 to 227 (`release.SetResult(true);` 220, `WaitForState` 223 to 226). Test 4 229 to 253 (`release.SetResult(true);` 246, `WaitForState` 249 to 252). Class closes at 254. Counts: `SpinWait` 1, `.Wait(` 2, `WaitForState` 5, `new BackgroundWorker()` 2, `IsBusy` 6 (73, 107, 108, 151, 177, 185; line 73 is test 1's issue #424 description of production behavior and stays). No occurrence of `TaskCreationOptions.RunContinuationsAsynchronously`, `pump.Drain();` or `SynchronizationContext.SetSynchronizationContext(previous);`.
3. `QuickFiler.Test/Controllers/QfcDatamodelTeardownTests.cs` is 235 lines. `WaitForState` documentation and expression body 59 to 67 (`SpinWait.SpinUntil` at 67). `Cleanup_CalledTwice_DoesNotThrow` uses `new BackgroundWorker { WorkerSupportsCancellation = true }` at 174 and starts no worker. `Worker_DoWork_CapturesRemainingLoadTask` 196 to 233: the `using (var worker = new BackgroundWorker())` block 214 to 232 with `InitEmailQueue` 218, `.Task.Wait(TimeSpan.FromSeconds(5))` 220, `WaitForState` 225 to 229, `loaderRelease.TrySetResult(true);` 231. `QuiesceLoaderAsync(TimeSpan.FromSeconds(5))` at 118 and 149, `fake.Advance(TimeSpan.FromSeconds(6));` at 154. Counts: `SpinWait` 1, `.Wait(` 1, `WaitForState` 2, `new BackgroundWorker()` 1.
4. `QuickFiler.Test/Controllers/QfcInitEmailQueueZeroBatchTests.cs` is 212 lines. Class remarks 23 to 31 (`/// <remarks>` 23, `/// </remarks>` 31); line 24 says every test "starts a real" `BackgroundWorker`, which is stale after the rewrite (the token `starts a real` occurs once, at 24). `IsBusy` occurs twice (139, 141, both inside the Z1 documentation that Z1 replaces). `CreateInertRemainingEmailLoader` 89 to 108 (completes its source and returns `Task.FromResult(true)`). Z0 `InitEmailQueue_ZeroBatchSize_ReturnsEmptyListWithoutThrowing` 117 to 133 (loader assignment 123, `new BackgroundWorker()` in the act lambda 127). Z1 `InitEmailQueue_ZeroBatchSize_StillStartsBackgroundWorker` documentation 135 to 145, method 146 to 164 (`new BackgroundWorker()` 153, `.Task.Wait(TimeSpan.FromSeconds(5))` 161). Z2 `InitEmailQueue_PositiveBatchSize_RetainsExistingProjectionAndFrameDrop` 176 to 210 (loader assignment 182, `new BackgroundWorker()` 201). Counts: `SpinWait` 0, `.Wait(` 1, `WaitForState` 0, `new BackgroundWorker()` 3.
5. `QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixtureTests.cs` is 458 lines. `private const int GateTimeoutMs = 60000;` at 33; `[Timeout(GateTimeoutMs)]` occurs 8 times. R4 documentation 192 to 203 with the flake-watch `<para>` 196 to 202 (`flake-watch` at 199, `Append an observation` at 200). R4 `Transaction_SecondCallerCannotInstallUntilTheFirstRestores` attributes `[TestMethod]` 204 and `[Timeout(GateTimeoutMs)]` 205, declared at 206; body 207 to 264: `{` 207, `// Arrange` 208, `Dispatcher liveA = ...` 209, `try` 210, `{` 211, transaction A acquisition 212 to 214 (`UiThreadDispatcherTransaction transactionA = await UiThreadDispatcherFixture` 212, `.BeginTransactionAsync()` 213, `.ConfigureAwait(false);` 214, all at sixteen spaces except the two chained lines at twenty), `Dispatcher original = UiThreadDispatcherFixture.Current;` 215, `transactionA.Install(liveA);` 216 (the only occurrence in the file), blank 217, `using (var secondCallerStarted = new ManualResetEventSlim(false))` 218 (the only `using (` between 206 and 273), blank lines 221, 237 and 242, transaction B acquisition 225 to 227, `BeSameAs(original)` 244 to 250, `NotBeSameAs(liveA)` 251 to 257 with `+ "issue #230 lost update"` at 256 (the only occurrence of `issue #230 lost update` in the file), `}` 258 (closes the `secondCallerStarted` using, sixteen spaces), `}` 259 (closes the `try`, twelve spaces), `finally` 260, `{` 261, `QfcItemControllerTestSupport.ShutdownDispatcher(liveA);` 262, `}` 263, method close 264. Between 256 and 261, exactly two lines contain `}` (258 and 259). R5 `Transaction_DisposedTwice_DoesNotOverReleaseTheGate` is declared at 273. The literal `Dispatcher original = UiThreadDispatcherFixture.Current;` also occurs at 55 and 115, so it is not a unique anchor; the first occurrence after line 206 is 215. The first `.BeginTransactionAsync()` after 206 is 213. Precedent for an ensure scope taken while a transaction holds the gate: R2 and R3 call `IDisposable ensureScope = QfcItemControllerTestSupport.EnsureUiThreadDispatcher();` at 119 and 166, each after their own `BeginTransactionAsync` (111, 161). The file has no `Issue #950` text and no `W3` or `W4` text.
6. `QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixture.cs`: `EnsureDispatcher` 122 to 138 seeds the parked dispatcher only into a null field and never takes the transaction gate (design note 26 to 30); the parked thread is named `UiThreadDispatcherFixture.ParkedDispatcher` (231). `QfcItemController.TestSupport.cs` 238 to 239: `EnsureUiThreadDispatcher()` wraps it. `QfcItemController.FocusAndThemeTests.cs` declares class `QfcItemController_FocusAndThemeTests` (27) and discards the scope at 452 and 468. None is modified.
7. `QuickFiler.Test/SetupAssemblyInitializer.cs` `[AssemblyInitialize]` (14 to 25) installs an assembly resolver and WinForms rendering defaults and does not write `UiThread._dispatcher`, so R4 run alone by fully qualified name starts with a null baseline.
8. `scripts/vscode/TaskMaster.cli.runsettings` lines 4 to 7 set Workers 0 and Scope ClassLevel. `scripts/vscode/Invoke-MSTestWithCoverage.ps1` (461 lines): `Get-DotnetCoverageArgumentList` appends `/InIsolation`, `/TestCaseFilter:TestCategory!=LiveOutlook`, the results directory and the trx logger at 89 to 93 with no extension point; `Invoke-DotnetCoverageCollection` throws `MSTest with coverage failed with exit code` at 262 after the collector exits non-zero, before post-processing; defaults `coverage\test-results` and `mstest-coverage-run.trx` at 297 to 298; discovery 348 to 355 filters `bin\Debug` and a `.claude` segment relative to the search root; post-processing 399 to 402; threshold gate 406 to 409; `First-party coverage:` line printed at 410; JaCoCo projection 415 to 423; trx summary 430 to 447; raw document retained when written to the repository coverage directory 449 to 453; entry guard 459 to 461, so dot-sourcing is safe. `Invoke-MSTest.TrxSummary.ps1`: `Get-TrxRunSummary` 12 to 101, `Format-TrxRunSummary` 103 to 150 (its last line is `Failed tests: ` followed by names or `none`). `Invoke-MSTestWithCoverage.FirstParty.ps1` 117 to 120 renders `First-party coverage: lines a/b (p%), branches c/d (q%)`.
9. `QuickFiler.Test/QuickFiler.Test.csproj` lists the four test files explicitly (157, 161, 183, 203), `OutputPath` `bin\Debug\` (35), analyzer items 530 to 561 including `Meziantou.Analyzer.3.0.290` (554) and `Roslynator.Analyzers.5.0.0` (555 to 558); `QuickFiler.Test/packages.config` pins Meziantou.Analyzer 3.0.290 (11) and Roslynator.Analyzers 5.0.0 (52). `QuickFiler/QuickFiler.csproj` lists `Controllers\QfcDatamodel.cs` at 325, `OutputPath` `bin\Debug\` (24).
10. `.gitignore`: `*.trx` at 146, `coverage/*` at 150, `!coverage/.gitkeep` at 151. `.csharpierignore` excludes `**/evidence/**` (4) and `*.csproj` (12); there is no `.csharpierrc`, so the default print width of 100 applies. `global.json` pins SDK 8.0.205 under `.dotnet-sdk` with `latestFeature` roll-forward (3 to 8); `dotnet-tools.json` pins csharpier 1.2.6 (6). `scripts/vscode/Install-RepoDotNetSdk.ps1` defaults to version 8.0.205; `scripts/vscode/Invoke-Restore.ps1` takes SolutionPath, Configuration and Platform.
11. Branch `bug/quickfiler-tests-depend-on-wall-clock-timing-950`, originally cut at `9b3eea584` (worktree reflog line 1; the original anchor, superseded by Revision R2); at the original authoring, HEAD was `8bbd48f68f9631247ccc3c58fa232ff333d4fe56` after four documentation commits; for revision R1 the delegation states HEAD `e3827fb2c`, the orchestrator's spec-only commit (spec.md lines 72, 131 and AC13 at 278 re-read in this pass; the acceptance-criteria lines are still 266 to 282). HEAD is recorded as an observation in P0-T3, never as an expectation. `docs/features/potential/promoted/2026-09-30-quickfiler-tests-depend-on-wall-clock-timing.md` exists on the tree. The `.dotnet-sdk`, `packages` and `bin` trees are absent (orchestrator environment fact 1), so every bootstrap task is guarded and gated on its post-task marker.
12. Host constraint carried from sibling items in this parallel run: four UtilitiesCS.Test shell-icon classes (`HelperClasses.ShellUtilities_Tests`, `HelperClasses.ShellUtilitiesStatic_Tests`, `HelperClasses.SysImageListHelperTests`, `EmailIntelligence.OSBrowser_Tests`) have stalled vstest on this workstation, and CI executes them. Whether the stall reproduces today is unknown, so P0-T15 measures it and the result selects the coverage route (D-6).

## Design decisions (do not redesign)

- **D-1 Production seam (research option D1, spec Proposed Fix).** `QfcDatamodel` gains `internal Action<BackgroundWorker> WorkerStarter { get; set; }` with the XML documentation in Delivered Source S1, inserted after line 140. Both constructors assign `WorkerStarter = worker => worker.RunWorkerAsync();` immediately after their `RemainingEmailLoader` assignment (S2). Both start sites in `InitEmailQueue` become `WorkerStarter(worker);` (S3). Nothing else in the file changes. Net growth is 12 lines (483 to 495); the diff against the anchor is 14 added and 2 deleted lines.
- **D-2 Test helpers are private nested types per file.** `SynchronousBackgroundWorker : BackgroundWorker` exposes `RaiseDoWork()`, which calls the protected `OnDoWork(new DoWorkEventArgs(null))`, and the static method `StartSynchronously(BackgroundWorker worker)` casts and raises. Both live in each of the three datamodel test files. `DrainableSynchronizationContext` lives only in the liveness file (only tests 3 and 4 observe a continuation after release). No file is added and no project file changes. `StartHeldOpenLoader` creates its release source with `TaskCreationOptions.RunContinuationsAsynchronously`, so the loader's continuation is posted to the installed context and runs only inside `Drain()`. With a default source the continuation runs inline inside `SetResult` and `Drain()` runs nothing (measured during preflight). Negative-control batch C (P5-T18 to P5-T23) deletes `pump.Drain();` from tests 3 and 4 and requires both to fail, so a regression to inline continuations is observable.
- **D-3 Fail-before evidence.** Defect B: P1-T6 runs the deterministic R4 repro from the spec's Test Strategy table on the unmodified test (one discarded `QfcItemControllerTestSupport.EnsureUiThreadDispatcher();` inserted immediately before `transactionA.Install(liveA);`, test run alone by fully qualified name), tagged `[expect-fail]`; P4-T6 pairs it with the pass-after results. Defect A: the failure needs the thread pool to delay the worker body past a five-second bound, which no policy-compliant test can force, so P1-T8 writes a fail-before exception dossier and P5-T24 appends the post-fix deterministic controls to it.
- **D-4 AC16 observation.** A temporary probe test (Delivered Source P-PROBE) is added to the unmodified liveness file in Phase 1, run alone under the repository runsettings, and reverted. The probe always fails by construction, so its exit code is deterministic (1) and its failure message carries the observed value after `AMBIENT-SYNC-CONTEXT=`. The probe is never committed.
- **D-5 Temporary edits and byte-level reverts.** Phase 1 edits are applied to the tree as committed at P0-T17; Phase 5 edits are applied to the tree as committed at P4-T7. Each batch is reverted with `git restore --source=HEAD --worktree` and verified by `git diff --exit-code HEAD -- <file>` exiting 0 and `git status --porcelain -- <file>` printing nothing. A control that hangs instead of failing is a defect in the rewrite (spec Test Strategy): every direct vstest run carries the hang-dump blame switch, and a `Sequence_*.xml` file or a Timeout or Aborted outcome is a stop.
- **D-6 Coverage route is selected by a recorded observation.** P0-T15 runs the four shell-icon classes alone and records `STALL-PROBE: CLEAR` or `REPRODUCES`. `COVERAGE-ROUTE: RUNNER` (CLEAR) runs `scripts/vscode/Invoke-MSTestWithCoverage.ps1` verbatim (CLAUDE.md step 4). `COVERAGE-ROUTE: DIRECT` (REPRODUCES) issues the runner's own inner collector invocation with the four-class exclusion appended and post-processes with the runner's own helpers, because the runner hard-codes its filter (fact 8). Both routes yield the same committed forms: the `First-party coverage:` line, the JaCoCo package projection text and the trx-derived summary, transcribed into Markdown. Under DIRECT, AC17 cannot be met as worded (the runner was not run) and its check-off records `AC17: NOT MET (ENVIRONMENTAL: COVERAGE-ROUTE DIRECT)`.
- **D-7 Coverage obligations.** `QfcDatamodel` is type-level `[ExcludeFromCodeCoverage]` (fact 1), so its file has no class element in the Cobertura document and no per-file coverage judgment is available in either direction; P0-T16 and P6-T5 record `QFCDATAMODEL-CLASS-NODES:` and the gate is that it is 0 at both stages. Test assemblies are excluded from instrumentation by the runner's derived settings. The first-party line and the root counters are recorded at both stages; the merged repository-wide rate is compared in two branches (denominators within 1 percent: tolerance 0.5 percentage points; otherwise recorded, not gated), because that rate is not reproducible across runs of an identical tree. The 80 percent line and 75 percent branch floors are applied by the runner (or by the runner's own threshold functions under DIRECT).
- **D-8 Baseline-relative test outcomes.** The baseline full run (P0-T16) may contain pre-existing failures; they are recorded as `BASELINE-FAILED-SET:` and do not stop Phase 0. The final run passes only when its failed set contains no name absent from the baseline failed set (`NEW-FAILURES: NONE`) and all nine target tests are `Passed`. AC17 additionally requires the runner to exit 0; a final run whose only failures are baseline failures completes P6-T5 but records `AC17: NOT MET (PRE-EXISTING FAILURES)` at P6-T28, and the orchestrator decides.
- **D-9 Anchor.** Every diff gate uses `34c2ed88cbb009f2f231453db87bc64d45a9bd51` as its ref operand (`BASE` in prose). P0-T3 verifies it is an ancestor of HEAD and equals `git merge-base origin/main HEAD`; a mismatch is `BASE-SHA MISMATCH`: stop and report (the orchestrator re-anchors). Paths changed between BASE and HEAD at P0-T3 form the inherited set `INHERITED-COMMITTED:`; each must be under FEATURE or be exactly `docs/features/potential/promoted/2026-09-30-quickfiler-tests-depend-on-wall-clock-timing.md`, otherwise `INHERITED SET OUT OF SCOPE`: stop.
- **D-10 Commits.** Three commits: P0-T17 (FEATURE only), P4-T7 (the five code files plus FEATURE, staged after the P4-T1 scoped format so the committed text is formatter-stable), P6-T32 (FEATURE). Each is a `git -C WORKTREE add -- <pathspecs>` invocation followed by a separate `git -C WORKTREE commit -m "<message>"` invocation, one command per call, never chained, never `git add -A`. No commit message contains an angle bracket, a dollar sign or a backtick. A PreToolUse refusal of any `git add`, `git commit`, `.cs` edit or spec edit is recorded verbatim as `PRE-IMPLEMENTATION GATE BLOCKED` and stops the run; the executor does not modify hooks, checkpoints or permission configuration.
- **D-11 Git gates are pathspec-scoped and anchored.** Every `git diff` names BASE or HEAD as its ref operand; every name-listing diff is paired with a porcelain span in the same task; no gate asserts an unscoped empty porcelain. Uncommitted `.claude/agent-memory/` paths and this plan file may appear in porcelain output and are admitted by every scope gate.
- **D-12 Check-offs follow the loop.** Every check-off task sits in Phase 6 after the final toolchain pass and reads an artifact that survived it. Each flips exactly one checkbox and, when its evidence does not hold, completes with the box unchecked and records `ACn: NOT MET` with the reason in `FEATURE/evidence/other/ac-status-summary.md`.
- **D-13 Restart rules.** Phase 4: if P4-T4 or P4-T5 shows a target test not `Passed`, the executor corrects the Write Set file at fault (within the delivered design; no prohibited construct) and restarts at P4-T1, recording `P4-RESTART: n` in the restarted artifacts. Phase 6: `ITERATION` starts at 1. If P6-T1 rewrites any Write Set file, the executor commits exactly the rewritten Write Set files (`style(950): apply csharpier output`), increments ITERATION and restarts at P6-T1. If P6-T2, P6-T3, P6-T4 or P6-T5 fails because of a Write Set file, the executor corrects it, restarts at P4-T1 (scoped format, build, pass-after runs, commit), re-runs Phase 5 in full, increments ITERATION and resumes at P6-T1. A rewrite of a file outside the Write Set, or a failure not attributable to the Write Set beyond the D-8 baseline rule, is a stop with the failing artifact. P6-T7 records the final iteration only.

## Risks (recorded; no mitigation is in scope)

- **R4 pin-scope disposal can null the field under a concurrent theme test.** When R4's `baseline` scope disposes at the end of R4 it can reset `UiThread._dispatcher` to null (it does so whenever the pin seeded the parked dispatcher, which is the case whenever the field was null at the pin). A concurrently running `QfcItemController_FocusAndThemeTests` theme test (`SetThemeDark_FromNormal_SelectsDarkNormalTheme` or `SetThemeLight_FromNormal_SelectsLightNormalTheme`) whose discarded `EnsureUiThreadDispatcher()` ran during R4 installed nothing (the field was non-null), so after R4's disposal it can throw from `UiThread.Dispatcher`. This exposure pre-exists through the R2 and R3 scope disposals and through transaction restores, which reset the field the same way; it is out of scope for this issue. If it is observed in P4-T5 or P6-T5 (either theme test not `Passed` and absent from the corresponding baseline failed set, P0-T14 `BASELINE-CONCURRENT-FAILED:` or P0-T16 `BASELINE-FAILED-SET:`), the run stops and reports it with the failing artifact and the test's `MESSAGE` line as `THEME TEST NULL-DISPATCHER EXPOSURE OBSERVED`. This stop takes precedence over the D-13 restart rule; the Write Set is not expanded and FocusAndThemeTests.cs is not edited.
- **CSharpier layout of the re-indented R4 body.** The four-space re-indent pushes some R4 lines past the default width of 100 (for example the transaction B acquisition, pre-edit 225 to 227). The P4-T1 scoped format may reflow them; its output wins. No gate depends on that layout: every R4 gate token is confined to a line whose text CSharpier does not split (see R-BODY), and the R4 file's line count is recorded, not fixed.

## Delivered source (the executor writes these texts; CSharpier output wins on any layout difference)

Every block below except R-INJECT is shown at its in-file indentation (four, eight, twelve, sixteen or twenty leading spaces), so it is written exactly as shown; R-INJECT states its indentation with it. Prose-quoted tokens used by later gates are each confined to one physical line of the delivered text.

**S1 — `QuickFiler/Controllers/QfcDatamodel.cs`, inserted after line 140** (one blank line, then nine lines):

        /// <summary>
        /// Injectable worker-start seam for <see cref="InitEmailQueue(int, BackgroundWorker)"/>
        /// (issue #950). Both instance constructors assign a starter that calls
        /// <see cref="BackgroundWorker.RunWorkerAsync()"/>, so production behavior is unchanged;
        /// tests assign a starter that raises DoWork synchronously on the calling thread. The
        /// property stays null on instances built by GetUninitializedObject, so InitEmailQueue
        /// fails fast with a NullReferenceException there instead of starting a thread.
        /// </summary>
        internal Action<BackgroundWorker> WorkerStarter { get; set; }

Gate tokens quoted from S1: Injectable worker-start seam; null on instances built by GetUninitializedObject; internal Action<BackgroundWorker> WorkerStarter { get; set; }.

**S2 — the same file, one line inserted after line 40 and one after line 51** (twelve-space indent):

            WorkerStarter = worker => worker.RunWorkerAsync();

**S3 — the same file, line 273 and line 300** (each line's whole text `worker.RunWorkerAsync();` is replaced, keeping that line's indentation):

                WorkerStarter(worker);

**L1 — `QuickFiler.Test/Controllers/QfcDatamodelLivenessTests.cs`, replaces lines 47 to 57** (the `WaitForState` documentation and method):

        /// <summary>
        /// Test-side worker whose <see cref="RaiseDoWork"/> raises <c>DoWork</c> synchronously on
        /// the calling thread through the protected <c>OnDoWork</c>, so the privately subscribed
        /// <c>Worker_DoWork</c> runs to its first incomplete await before <c>InitEmailQueue</c>
        /// returns. Issue #950: this replaces the bounded waits on a thread-pool worker.
        /// </summary>
        private sealed class SynchronousBackgroundWorker : BackgroundWorker
        {
            public void RaiseDoWork() => OnDoWork(new DoWorkEventArgs(null));
        }

        /// <summary>The synchronous starter assigned to <c>QfcDatamodel.WorkerStarter</c>.</summary>
        private static void StartSynchronously(BackgroundWorker worker) =>
            ((SynchronousBackgroundWorker)worker).RaiseDoWork();

        /// <summary>
        /// Queues posted continuations and runs them only on an explicit <see cref="Drain"/> call,
        /// on the creating thread. Drain runs only work already queued, plus work that work queues,
        /// and never blocks. Installed around <c>InitEmailQueue</c> by the tests that observe the
        /// loader's continuation, and restored in a <c>finally</c>.
        /// </summary>
        private sealed class DrainableSynchronizationContext : SynchronizationContext
        {
            private readonly Queue<Tuple<SendOrPostCallback, object>> _callbacks =
                new Queue<Tuple<SendOrPostCallback, object>>();
            private readonly int _creatorThreadId = Environment.CurrentManagedThreadId;

            public override void Post(SendOrPostCallback d, object state) =>
                _callbacks.Enqueue(Tuple.Create(d, state));

            /// <summary>Runs every queued callback, including work queued while draining.</summary>
            internal void Drain()
            {
                Environment.CurrentManagedThreadId.Should().Be(_creatorThreadId);
                while (_callbacks.Count > 0)
                {
                    Tuple<SendOrPostCallback, object> callback = _callbacks.Dequeue();
                    callback.Item1(callback.Item2);
                }
            }
        }

**L2 — the same file, replaces lines 97 to 110 of test 1** (from `var worker = new BackgroundWorker();` through the closing `);` of the `WaitForState` call; lines 111 onward are unchanged):

            var worker = new SynchronousBackgroundWorker();
            model.WorkerStarter = StartSynchronously;

            // Act — the issue #244 zero-batch short-circuit is COM-free and starts the worker
            // through the issue #950 seam, which raises DoWork on this thread.
            model.InitEmailQueue(0, worker);

            loaderEntered
                .Task.IsCompleted.Should()
                .BeTrue("the synchronous starter must reach the injected RemainingEmailLoader");

**L3 — the same file, replaces lines 148 to 187** (`StartHeldOpenLoader` documentation and method 148 to 181, blank line 182, and the stale test 2 summary 183 to 187; test 2's `[TestMethod]` at 188 and everything after it up to test 3 is unchanged). The block ends with test 2's rewritten summary:

        /// <summary>
        /// Starts the worker with a <c>RemainingEmailLoader</c> held open by
        /// <paramref name="release"/>. The issue #950 synchronous starter raises <c>DoWork</c> on
        /// this thread, so by the time <c>InitEmailQueue</c> returns the async void
        /// <c>Worker_DoWork</c> has entered the loader and returned at its first incomplete await.
        /// <paramref name="release"/> runs its continuations asynchronously, so a test that has
        /// installed <c>DrainableSynchronizationContext</c> observes the resumed loader only
        /// through <c>Drain</c>, never inline inside <c>SetResult</c>.
        /// </summary>
        private static QfcDatamodel StartHeldOpenLoader(
            Func<TaskCompletionSource<bool>, Task<bool>> loaderBody,
            out TaskCompletionSource<bool> release
        )
        {
            var model = CreateUninitializedDatamodel();
            var entered = new TaskCompletionSource<bool>();
            var localRelease = new TaskCompletionSource<bool>(
                TaskCreationOptions.RunContinuationsAsynchronously
            );
            release = localRelease;

            model.RemainingEmailLoader = _ =>
            {
                entered.TrySetResult(true);
                return loaderBody(localRelease);
            };

            var worker = new SynchronousBackgroundWorker();
            model.WorkerStarter = StartSynchronously;
            model.InitEmailQueue(0, worker);

            entered
                .Task.IsCompleted.Should()
                .BeTrue("the synchronous starter must reach the injected loader before returning");
            return model;
        }

        /// <summary>
        /// AC 7: the flag stays true across the <c>async void</c> first-await boundary while the
        /// loader is still producing. Issue #950: the synchronous starter has already run
        /// <c>Worker_DoWork</c> to that boundary when <c>InitEmailQueue</c> returns, so the flag
        /// is read with no wait.
        /// </summary>

The block above is forty-three lines replacing forty (net +3). No line of it contains `IsBusy`, `.Wait(`, `WaitForState` or `pump.Drain();`. Gate tokens quoted from L3: TaskCreationOptions.RunContinuationsAsynchronously; the synchronous starter must reach the injected loader before returning.

**L4 — the same file, replaces lines 207 to 227** (test 3 documentation and method):

        /// <summary>
        /// AC 7: the flag becomes false only after the loader completes — never before. Issue
        /// #950: the continuation that clears the flag is drained from a test-owned context.
        /// </summary>
        [TestMethod]
        public void RemainingLoadActive_AfterLoaderCompletes_BecomesFalse()
        {
            // Arrange
            SynchronizationContext previous = SynchronizationContext.Current;
            var pump = new DrainableSynchronizationContext();
            SynchronizationContext.SetSynchronizationContext(pump);
            try
            {
                QfcDatamodel model = StartHeldOpenLoader(
                    signal => signal.Task,
                    out TaskCompletionSource<bool> release
                );
                ReadLivenessFlag(model).Should().BeTrue("the loader has not completed yet");

                // Act
                release.SetResult(true);
                pump.Drain();

                // Assert
                ReadLivenessFlag(model)
                    .Should()
                    .BeFalse(
                        "the finally around the awaited loader must clear the flag once it completes"
                    );
            }
            finally
            {
                SynchronizationContext.SetSynchronizationContext(previous);
            }
        }

**L5 — the same file, replaces lines 229 to 253** (test 4 documentation and method):

        /// <summary>
        /// AC 7: the <c>finally</c> clears the flag even when the loader throws. Issue #950: the
        /// faulted loader's continuation is drained from a test-owned context.
        /// </summary>
        [TestMethod]
        public void RemainingLoadActive_WhenLoaderThrows_IsStillClearedByFinally()
        {
            // Arrange
            SynchronizationContext previous = SynchronizationContext.Current;
            var pump = new DrainableSynchronizationContext();
            SynchronizationContext.SetSynchronizationContext(pump);
            try
            {
                QfcDatamodel model = StartHeldOpenLoader(
                    async signal =>
                    {
                        await signal.Task;
                        throw new InvalidOperationException("loader failed");
                    },
                    out TaskCompletionSource<bool> release
                );
                ReadLivenessFlag(model).Should().BeTrue("the loader has not failed yet");

                // Act
                release.SetResult(true);
                pump.Drain();

                // Assert
                ReadLivenessFlag(model)
                    .Should()
                    .BeFalse(
                        "the finally must clear the flag on the throwing path too, or the gate would poll forever"
                    );
            }
            finally
            {
                SynchronizationContext.SetSynchronizationContext(previous);
            }
        }

**T1 — `QuickFiler.Test/Controllers/QfcDatamodelTeardownTests.cs`, replaces lines 59 to 67** (the `WaitForState` documentation and method):

        /// <summary>
        /// Test-side worker whose <see cref="RaiseDoWork"/> raises <c>DoWork</c> synchronously on
        /// the calling thread through the protected <c>OnDoWork</c>, so the privately subscribed
        /// <c>Worker_DoWork</c> runs to its first incomplete await before <c>InitEmailQueue</c>
        /// returns (issue #950). Duplicated per file, following the convention documented on
        /// <c>QfcDatamodelLivenessTests</c>.
        /// </summary>
        private sealed class SynchronousBackgroundWorker : BackgroundWorker
        {
            public void RaiseDoWork() => OnDoWork(new DoWorkEventArgs(null));
        }

        /// <summary>The synchronous starter assigned to <c>QfcDatamodel.WorkerStarter</c>.</summary>
        private static void StartSynchronously(BackgroundWorker worker) =>
            ((SynchronousBackgroundWorker)worker).RaiseDoWork();

**T2 — the same file, replaces lines 214 to 232** (the `using` block of `Worker_DoWork_CapturesRemainingLoadTask`):

            using (var worker = new SynchronousBackgroundWorker())
            {
                model.WorkerStarter = StartSynchronously;

                // Act — the issue #244 zero-batch short-circuit is COM-free and starts the worker
                // through the issue #950 seam, which raises DoWork on this thread, so
                // Worker_DoWork has captured the loader task before InitEmailQueue returns.
                model.InitEmailQueue(0, worker);

                // Assert
                loaderEntered
                    .Task.IsCompleted.Should()
                    .BeTrue("the synchronous starter must reach the injected RemainingEmailLoader");
                GetPrivateField(model, "_remainingLoadTask")
                    .Should()
                    .NotBeNull(
                        "the loader task must be captured before it is awaited, so the Cancel path has "
                            + "a handle to quiesce"
                    );

                loaderRelease.TrySetResult(true);
            }

**Z-H — `QuickFiler.Test/Controllers/QfcInitEmailQueueZeroBatchTests.cs`, inserted after line 108** (one blank line, then the block):

        /// <summary>
        /// Test-side worker whose <see cref="RaiseDoWork"/> raises <c>DoWork</c> synchronously on
        /// the calling thread through the protected <c>OnDoWork</c>, so no worker started by
        /// <c>InitEmailQueue</c> outlives the test (issue #950). Duplicated per file, following
        /// the convention documented on <c>QfcDatamodelLivenessTests</c>.
        /// </summary>
        private sealed class SynchronousBackgroundWorker : BackgroundWorker
        {
            public void RaiseDoWork() => OnDoWork(new DoWorkEventArgs(null));
        }

        /// <summary>The synchronous starter assigned to <c>QfcDatamodel.WorkerStarter</c>.</summary>
        private static void StartSynchronously(BackgroundWorker worker) =>
            ((SynchronousBackgroundWorker)worker).RaiseDoWork();

**Z0 — the same file, test `InitEmailQueue_ZeroBatchSize_ReturnsEmptyListWithoutThrowing`:** insert `            model.WorkerStarter = StartSynchronously;` immediately after the line `            model.RemainingEmailLoader = CreateInertRemainingEmailLoader(out _);` of this test (pre-edit line 123), and in the act lambda (pre-edit line 127) replace `new BackgroundWorker()` with `new SynchronousBackgroundWorker()`.

**Z1 — the same file, replaces the documentation and method of `InitEmailQueue_ZeroBatchSize_StillStartsBackgroundWorker`** (pre-edit lines 135 to 164):

        /// <summary>
        /// Issue #244 AC2: a zero batch size must still set up and start the background worker so
        /// remaining emails continue to load into the master queue. <see cref="BackgroundWorker.WorkerSupportsCancellation"/>
        /// (set synchronously by <see cref="QfcDatamodel.SetupWorker"/>) proves the worker was set up.
        /// Issue #950: the worker is started through the <c>WorkerStarter</c> seam with a starter
        /// that raises <c>DoWork</c> on this thread, and the inert loader completes its
        /// <see cref="TaskCompletionSource{TResult}"/> synchronously, so the loader-invoked signal
        /// is read without any wait as soon as <c>InitEmailQueue</c> returns.
        /// </summary>
        [TestMethod]
        public void InitEmailQueue_ZeroBatchSize_StillStartsBackgroundWorker()
        {
            // Arrange
            var model = CreateUninitializedDatamodel();
            SetPrivateField(model, "_frame", CreateTwoRowEmailFrame());
            model.RemainingEmailLoader = CreateInertRemainingEmailLoader(out var loaderInvokedTcs);
            model.WorkerStarter = StartSynchronously;
            var worker = new SynchronousBackgroundWorker();

            // Act
            model.InitEmailQueue(0, worker);

            // Assert
            worker.WorkerSupportsCancellation.Should().BeTrue();
            loaderInvokedTcs
                .Task.IsCompleted.Should()
                .BeTrue("the injected RemainingEmailLoader must be invoked by the started worker");
        }

**Z2 — the same file, test `InitEmailQueue_PositiveBatchSize_RetainsExistingProjectionAndFrameDrop`:** insert `            model.WorkerStarter = StartSynchronously;` immediately after this test's line `            model.RemainingEmailLoader = CreateInertRemainingEmailLoader(out _);` (pre-edit line 182), and on the act line (pre-edit line 201) replace `new BackgroundWorker()` with `new SynchronousBackgroundWorker()`.

**Z-R — the same file, replaces the class remarks, lines 23 to 31** (four-space indent; thirteen lines replacing nine, net +4; applied by P3-T14 after every other zero-batch edit, and lines 23 to 31 lie above every other zero-batch edit site, so their numbering is the pre-edit numbering):

    /// <remarks>
    /// v1.1 revision (issue #244): every test below assigns an inert, recording
    /// <see cref="QfcDatamodel.RemainingEmailLoader"/> delegate via the internal seam BEFORE
    /// calling <see cref="QfcDatamodel.InitEmailQueue(int, BackgroundWorker)"/>. Without this,
    /// the started worker's <c>Worker_DoWork</c> reaches the real
    /// <c>LoadRemainingEmailsToQueueAsync</c>, which pops a live
    /// <see cref="System.Windows.Forms.MessageBox"/> dialog and touches Outlook COM
    /// (<c>_olApp.GetNamespace("MAPI")</c>) — this is the maintainer-reported defect in the v1.0
    /// revision of these tests, and this file must never reproduce it. Issue #950: every test also
    /// assigns the <c>WorkerStarter</c> seam a starter that raises <c>DoWork</c> synchronously on
    /// the test thread through the nested <c>SynchronousBackgroundWorker</c>, so no test starts a
    /// thread-pool worker and none outlives the test.
    /// </remarks>

No line of Z-R contains `starts a real`, `IsBusy`, `new BackgroundWorker()`, `new SynchronousBackgroundWorker()` or `WorkerStarter = StartSynchronously;`. Gate token quoted from Z-R: so no test starts a.

**R-DOC — `QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixtureTests.cs`, replaces lines 196 to 202** (the R4 `<para>`; thirteen lines replacing seven, net +6). P3-T13 applies it after P3-T12, and R-BODY's first change is at line 214, below this region, so these are still the pre-edit line numbers when P3-T13 runs:

        /// <para>
        /// Issue #950: the earlier intermittent failure was a race on the shared static, not a
        /// timing defect. The gate-free fixture method EnsureDispatcher seeds the parked dispatcher
        /// whenever the field is null, so a concurrently running class that calls it could write
        /// between the baseline read and the install, or between the restore and the second
        /// caller's read. The test therefore pins a non-null baseline with an ensure scope that it
        /// opens only after transaction A has acquired the gate and holds through both assertions.
        /// Taking the pin inside the gate means that a gated transaction from another class (W3/W4)
        /// cannot restore a null previous value between the pin and this test's acquisition.
        /// Invariant for future editors: no other class may dispose an ensure scope holding the
        /// parked dispatcher (W2), and UiThread.Initialize (W5) must not latch during this test;
        /// either would change the value the second caller observes.
        /// </para>

`Issue #950:` occurs once in R-DOC and nowhere else in the file after the change (R-BODY adds no comment). Gate tokens quoted from R-DOC, each confined to one line: Issue #950: the earlier intermittent failure was a race on the shared static; The gate-free fixture method EnsureDispatcher seeds the parked dispatcher; cannot restore a null previous value between the pin; (W2), and UiThread.Initialize (W5) must not latch.

**R-BODY — the same file, R4 body (applied by P3-T12, before R-DOC, against the pre-edit numbering).** Three changes, nothing else:

1. Insert these four lines immediately after pre-edit line 214 (`.ConfigureAwait(false);`, the line that completes transaction A's acquisition) and before pre-edit line 215 (`Dispatcher original = UiThreadDispatcherFixture.Current;`). The `using (`, `)` and `{` lines are at sixteen spaces and the declaration at twenty:

                using (
                    IDisposable baseline = QfcItemControllerTestSupport.EnsureUiThreadDispatcher()
                )
                {

2. Re-indent pre-edit lines 215 to 258 inclusive (forty-four lines: from the `original` read through the `}` that closes the `secondCallerStarted` using block) by four additional leading spaces, with no other character changed; the blank lines among them (pre-edit 217, 221, 237 and 242) stay empty.
3. Insert one line consisting of sixteen spaces and `}` immediately after pre-edit line 258, closing the `baseline` using block before the `try` block's closing brace (pre-edit 259).

Pre-edit lines 207 to 214 (including `// Arrange` at 208 and the transaction A acquisition) and 259 to 264 (the `try` close, the `finally` block and the method close) are unchanged. Net growth five lines (458 plus 5 plus R-DOC's 6 is 469 before formatting). The two assertions (pre-edit 244 to 257) are retained apart from indentation, so B's read (pre-edit 230) and both assertions run under the pin, and the pin is taken after transaction A holds the gate.

The `using` header is shown split because the one-line form `using (IDisposable baseline = QfcItemControllerTestSupport.EnsureUiThreadDispatcher())` at sixteen spaces is 102 characters, over the default width of 100; the split form is the expected CSharpier layout, and the declaration line is 98 characters. If CSharpier lays the header out differently, its output wins: the gate token below is the declaration text, which is a substring of the header in the one-line form and the whole declaration line in the split form, so it holds in either layout. Gate token quoted from R-BODY: IDisposable baseline = QfcItemControllerTestSupport.EnsureUiThreadDispatcher().

**P-PROBE — temporary (Phase 1 only, never committed). The unmodified liveness file, inserted after line 253** (the closing brace of `RemainingLoadActive_WhenLoaderThrows_IsStillClearedByFinally`), one blank line then:

        [TestMethod]
        public void Issue950_AmbientSynchronizationContextProbe()
        {
            SynchronizationContext observed = SynchronizationContext.Current;
            string observedName = observed == null ? "null" : observed.GetType().FullName;
            string report =
                "AMBIENT-SYNC-CONTEXT="
                + observedName
                + " THREAD-POOL="
                + Thread.CurrentThread.IsThreadPoolThread
                + " APARTMENT="
                + Thread.CurrentThread.GetApartmentState();
            report.Should().Be("PROBE-ALWAYS-FAILS", report);
        }

**R-INJECT — temporary (Phase 1 on the unmodified file, Phase 5 on the fixed file; never committed).** One line inserted immediately before the unique line whose trimmed text is `transactionA.Install(liveA);`, at that line's indentation (sixteen spaces on the unmodified file; twenty on the pinned file, where the inserted line sits inside the `baseline` using block, after the `original` read):

    QfcItemControllerTestSupport.EnsureUiThreadDispatcher();

## Execution conventions

- **Tokens.** `WORKTREE` denotes the absolute path of the item worktree supplied in the delegation prompt; the executor substitutes it into every payload and every `git -C` argument at run time and writes the token, never the path, into artifacts. `FEATURE` abbreviates the feature folder path. `BASE` denotes `34c2ed88cbb009f2f231453db87bc64d45a9bd51`, which is written in full in every command. No artifact, and no line of this plan, carries an absolute host path, an account name or a machine name.
- **Payload channel.** Each indented payload in the Command Reference runs as one Bash tool call of the form `pwsh -NoProfile -Command '<payload>'`, newlines included, with the substitutions applied. Payloads use double quotes only, so the outer single quotes never conflict. Git commands run as single Bash calls of the form `git -C WORKTREE <arguments>`, never chained. No `cd`, no `&&`, `;` or `|` between Bash commands.
- **Command rows.** The `Command:` field of a payload artifact records the full payload as executed, with `WORKTREE` in place of the path, followed on the next line by the canonical command it implements (for example `msbuild TaskMaster.sln /t:Rebuild ...`). Every payload artifact records `WORKTREE-LEAF: agent-a7805823735145ca4`; any other value means the payload ran in the wrong tree, and the task fails.
- **Exit codes.** `EXIT_CODE:` records the payload's principal exit value as named in each Command Reference entry. Deliberately failing runs carry `ExpectedExitCode:` equal to the deterministic value the task states. A recorded observation whose exit value is not gated carries `ExpectedExitCode:` equal to the observed value and says so.
- **Transcription.** Any absolute path inside a transcribed line is replaced by `REDACTED-PATH`. Trx and coverage documents are never copied into FEATURE.
- **Long-running payloads.** `CMD-COVERAGE-RUNNER` and `CMD-COVERAGE-DIRECT` are started with the Bash tool's `run_in_background` option and no shell redirection (a Bash `>` target resolves against the session checkout, not WORKTREE); the payload's own output, captured by the tool, is the record, and completion is the background-task notification together with a final output line `PAYLOAD-COMPLETE`. A run still in progress after 120 minutes is `COVERAGE RUN STALLED`: stop and report. Before any re-invocation after a timed-out or interrupted call, the executor runs `pwsh -NoProfile -Command '$leaf = "agent-a7805823735145ca4"; $runner = "Invoke-MSTest" + "WithCoverage"; "STRAY_TEST_PROCESSES: " + @(Get-CimInstance Win32_Process -ErrorAction SilentlyContinue | Where-Object { $_.ProcessId -ne $PID -and $null -ne $_.CommandLine -and $_.CommandLine.Contains($leaf) -and ($_.Name -like "vstest*" -or $_.Name -like "testhost*" -or $_.Name -like "dotnet-coverage*" -or ($_.Name -like "pwsh*" -and $_.CommandLine.Contains($runner))) }).Count'` and proceeds only at `STRAY_TEST_PROCESSES: 0`; the count covers the collector, the console, any test host whose command line carries the leaf, and a surviving pwsh runner (which respawns test hosts), each scoped to this worktree's leaf so test runs in sibling worktrees cannot hold the gate open; the runner token is assembled from two literals and the checking process excludes itself by PID, so the check never counts its own command line (preflight round 2 observed 0 with this form on an idle machine, and 1 with the joined literal and no PID exclusion), and two collections from this worktree never run at once. Both payloads, as written in the Command Reference, end with the unconditional line `Write-Output "PAYLOAD-COMPLETE"`, which runs on success and on a non-zero collector exit alike (a native command's exit code does not stop the payload); a run that ends without that line is incomplete and is treated as `COVERAGE RUN ABORTED`.
- **No wall-clock construct anywhere.** No payload sleeps; no test edit adds a sleep, delay, retry, timeout change, parallelism attribute or temporary file.

## Command reference

**PREFIX** (the first three lines of every payload):

    Set-Location -LiteralPath "WORKTREE"
    [System.IO.Directory]::SetCurrentDirectory((Get-Location).Path)
    Write-Output ("WORKTREE-LEAF: " + (Split-Path -Leaf (Get-Location).Path))

**TOOLS** (the next lines of every build and test payload):

    $vswhere = Join-Path ${env:ProgramFiles(x86)} "Microsoft Visual Studio\Installer\vswhere.exe"
    $msbuild = & $vswhere -latest -products * -find "MSBuild\**\Bin\MSBuild.exe" | Select-Object -First 1
    $vstest = & $vswhere -latest -products * -find "Common7\IDE\Extensions\TestPlatform\vstest.console.exe" | Select-Object -First 1
    New-Item -ItemType Directory -Path "coverage\logs" -Force | Out-Null

**CODE5** (the five Write Set code paths, in this order): `"QuickFiler\Controllers\QfcDatamodel.cs", "QuickFiler.Test\Controllers\QfcDatamodelLivenessTests.cs", "QuickFiler.Test\Controllers\QfcDatamodelTeardownTests.cs", "QuickFiler.Test\Controllers\QfcInitEmailQueueZeroBatchTests.cs", "QuickFiler.Test\Controllers\QfcItemController.UiThreadDispatcherFixtureTests.cs"`. **THREE** is the second, third and fourth entries. **CODE5-GIT** is the same five paths with forward slashes, space-separated, for git pathspecs.

**CMD-REBUILD** (`GATEARGS` is either `/p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` or `/p:TreatWarningsAsErrors=true`; `TASKID` substituted; `EXIT_CODE:` is `MSBUILD_EXIT_CODE:`; canonical command `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" GATEARGS`, resolved through vswhere, plus /nodeReuse:false and a normal-verbosity file logger under the ignored coverage directory):

    PREFIX
    TOOLS
    $log = "coverage\logs\TASKID.msbuild.log"
    if (Test-Path -LiteralPath $log) { Remove-Item -LiteralPath $log -Force }
    $global:LASTEXITCODE = 0
    & $msbuild TaskMaster.sln /t:Rebuild /m /nodeReuse:false /p:Configuration=Debug "/p:Platform=Any CPU" GATEARGS "/flp:LogFile=$log;Verbosity=normal" | Out-Null
    Write-Output ("MSBUILD_EXIT_CODE: " + $LASTEXITCODE)
    $lines = Get-Content -LiteralPath $log -Encoding UTF8
    Write-Output ("ERRORS: " + (($lines | Select-String -Pattern "^\s*(\d+) Error\(s\)" | Select-Object -Last 1).Matches[0].Groups[1].Value))
    Write-Output ("WARNINGS: " + (($lines | Select-String -Pattern "^\s*(\d+) Warning\(s\)" | Select-Object -Last 1).Matches[0].Groups[1].Value))
    Write-Output ("SKIP_CORECOMPILE_LINES: " + @($lines | Where-Object { $_.Contains("Skipping target ""CoreCompile""") }).Count)
    Write-Output ("CSC_OUT_QUICKFILER: " + @($lines | Where-Object { $_.Contains("/out:obj\Debug\QuickFiler.dll") }).Count)
    Write-Output ("CSC_OUT_QUICKFILER_TEST: " + @($lines | Where-Object { $_.Contains("/out:obj\Debug\QuickFiler.Test.dll") }).Count)
    $files = @("QfcDatamodel.cs(", "QfcDatamodelLivenessTests.cs(", "QfcDatamodelTeardownTests.cs(", "QfcInitEmailQueueZeroBatchTests.cs(", "QfcItemController.UiThreadDispatcherFixtureTests.cs(")
    $diag = @($lines | Where-Object { $l = $_; (@($files | Where-Object { $l.Contains($_) }).Count -gt 0) -and ($l -match "(error|warning) [A-Z]+[0-9]+") })
    Write-Output ("WRITESET_DIAGNOSTIC_LINES: " + $diag.Count)
    Write-Output ("WRITESET_DIAGNOSTIC_CODES: " + ((@($diag | ForEach-Object { [regex]::Match($_, "(error|warning) ([A-Z]+[0-9]+)").Groups[2].Value }) | Sort-Object -Unique) -join ","))
    Write-Output ("TEST_DLL_EXISTS: " + (Test-Path -LiteralPath "QuickFiler.Test\bin\Debug\QuickFiler.Test.dll"))
    Write-Output ("UCS_TEST_DLL_EXISTS: " + (Test-Path -LiteralPath "UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll"))

`ERRORS:` is read from the build summary line, so `0 Error(s)` is not mistaken for a substring of a larger count. Under /t:Rebuild `SKIP_CORECOMPILE_LINES` is 0 by construction; the two `CSC_OUT_` counts show the compiler ran for both Write Set projects (MSBuild echoes each csc command line at normal verbosity).

**CMD-BUILD** (incremental build so a scoped test run observes a fresh assembly; `TASKID` substituted; `EXIT_CODE:` is `MSBUILD_EXIT_CODE:`; canonical command `msbuild TaskMaster.sln /t:Build /m /p:Configuration=Debug "/p:Platform=Any CPU"`):

    PREFIX
    TOOLS
    $log = "coverage\logs\TASKID.msbuild.log"
    if (Test-Path -LiteralPath $log) { Remove-Item -LiteralPath $log -Force }
    $before = (Get-Item -LiteralPath "QuickFiler.Test\bin\Debug\QuickFiler.Test.dll" -ErrorAction SilentlyContinue).LastWriteTimeUtc
    $global:LASTEXITCODE = 0
    & $msbuild TaskMaster.sln /t:Build /m /nodeReuse:false /p:Configuration=Debug "/p:Platform=Any CPU" "/flp:LogFile=$log;Verbosity=normal" | Out-Null
    Write-Output ("MSBUILD_EXIT_CODE: " + $LASTEXITCODE)
    $lines = Get-Content -LiteralPath $log -Encoding UTF8
    Write-Output ("ERRORS: " + (($lines | Select-String -Pattern "^\s*(\d+) Error\(s\)" | Select-Object -Last 1).Matches[0].Groups[1].Value))
    $after = (Get-Item -LiteralPath "QuickFiler.Test\bin\Debug\QuickFiler.Test.dll").LastWriteTimeUtc
    Write-Output ("TEST_DLL_ADVANCED: " + ($null -eq $before -or $after -gt $before))
    Write-Output ("CSC_OUT_QUICKFILER_TEST: " + @($lines | Where-Object { $_.Contains("/out:obj\Debug\QuickFiler.Test.dll") }).Count)

**CMD-VSTEST** (`ASSEMBLY`, `FILTER`, `TASKID` and `NAMES` substituted; an empty `NAMES` prints every result; `EXIT_CODE:` is `VSTEST_EXIT_CODE:`, or 3 when the trx is absent; canonical command `vstest.console.exe ASSEMBLY /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation "/TestCaseFilter:FILTER" "/ResultsDirectory:coverage\test-results\950\TASKID" "/Logger:trx;LogFileName=TASKID.trx" "/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None"`, resolved through vswhere):

    PREFIX
    TOOLS
    $results = "coverage\test-results\950\TASKID"
    if (Test-Path -LiteralPath $results) { Remove-Item -LiteralPath $results -Recurse -Force }
    $names = @(NAMES)
    $global:LASTEXITCODE = 0
    & $vstest "ASSEMBLY" /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation "/TestCaseFilter:FILTER" "/ResultsDirectory:$results" "/Logger:trx;LogFileName=TASKID.trx" "/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None" 2>&1 | Tee-Object -FilePath "coverage\logs\TASKID.vstest.log" | Out-Null
    Write-Output ("VSTEST_EXIT_CODE: " + $LASTEXITCODE)
    $trxPath = Join-Path $results "TASKID.trx"
    Write-Output ("TRX_PRESENT: " + (Test-Path -LiteralPath $trxPath))
    Write-Output ("SEQUENCE_FILES: " + @(Get-ChildItem -LiteralPath $results -Recurse -Filter "Sequence_*.xml" -ErrorAction SilentlyContinue).Count)
    if (-not (Test-Path -LiteralPath $trxPath)) { exit 3 }
    [xml]$trx = Get-Content -LiteralPath $trxPath -Raw -Encoding UTF8
    $ns = New-Object System.Xml.XmlNamespaceManager($trx.NameTable)
    $ns.AddNamespace("t", "http://microsoft.com/schemas/VisualStudio/TeamTest/2010")
    $counters = $trx.SelectSingleNode("//t:ResultSummary/t:Counters", $ns)
    Write-Output ("COUNTERS total=" + $counters.GetAttribute("total") + " executed=" + $counters.GetAttribute("executed") + " passed=" + $counters.GetAttribute("passed") + " failed=" + $counters.GetAttribute("failed"))
    $all = @($trx.SelectNodes("//t:UnitTestResult", $ns))
    Write-Output ("RESULT_COUNT: " + $all.Count)
    foreach ($r in $all) { if ($names.Count -eq 0 -or $names -contains $r.GetAttribute("testName")) { Write-Output ("RESULT " + $r.GetAttribute("testName") + " = " + $r.GetAttribute("outcome") + " duration=" + $r.GetAttribute("duration")) } }
    foreach ($r in $all) { if ($r.GetAttribute("outcome") -ne "Passed") { $msg = $r.SelectSingleNode("t:Output/t:ErrorInfo/t:Message", $ns); Write-Output ("MESSAGE " + $r.GetAttribute("testName") + " :: " + $(if ($msg) { $msg.InnerText -replace "\s+", " " } else { "(no message)" })) } }

The trx stays under the ignored coverage directory; the artifact transcribes the `COUNTERS`, `RESULT_COUNT:`, `RESULT` and `MESSAGE` lines.

Substitutions: `ASSEMBLY-QF` is `QuickFiler.Test\bin\Debug\QuickFiler.Test.dll`; `ASSEMBLY-UCS` is `UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll`. The fully qualified prefix `QuickFiler.Controllers.Tests.` is written `QCT.` below for brevity and is expanded in full in every executed filter.

- `FILTER-TARGETS`: the nine expressions `FullyQualifiedName=QCT.QfcDatamodelLivenessTests.DequeueNextItemGroupAsync_WhileLoaderStillProducing_KeepsPollingAfterWorkerIdle`, `...=QCT.QfcDatamodelLivenessTests.RemainingLoadActive_AcrossAsyncVoidFirstAwait_StaysTrueWhileLoaderProduces`, `...=QCT.QfcDatamodelLivenessTests.RemainingLoadActive_AfterLoaderCompletes_BecomesFalse`, `...=QCT.QfcDatamodelLivenessTests.RemainingLoadActive_WhenLoaderThrows_IsStillClearedByFinally`, `...=QCT.QfcDatamodelTeardownTests.Worker_DoWork_CapturesRemainingLoadTask`, `...=QCT.QfcInitEmailQueueZeroBatchTests.InitEmailQueue_ZeroBatchSize_StillStartsBackgroundWorker`, `...=QCT.QfcInitEmailQueueZeroBatchTests.InitEmailQueue_ZeroBatchSize_ReturnsEmptyListWithoutThrowing`, `...=QCT.QfcInitEmailQueueZeroBatchTests.InitEmailQueue_PositiveBatchSize_RetainsExistingProjectionAndFrameDrop`, `...=QCT.QfcItemController_UiThreadDispatcherFixtureTests.Transaction_SecondCallerCannotInstallUntilTheFirstRestores`, joined with `|` (vstest rejects `OR`).
- `NAMES-TARGETS`: the nine method names above, each double-quoted, comma-separated.
- `FILTER-CONCURRENT`: `FullyQualifiedName~QCT.QfcDatamodelLivenessTests.|FullyQualifiedName~QCT.QfcDatamodelTeardownTests.|FullyQualifiedName~QCT.QfcInitEmailQueueZeroBatchTests.|FullyQualifiedName~QCT.QfcItemController_UiThreadDispatcherFixtureTests.|FullyQualifiedName~QCT.QfcItemController_FocusAndThemeTests.` (all four target classes with the concurrent-writer class in one invocation under ClassLevel parallelism).
- `FILTER-PROBE`: `FullyQualifiedName=QCT.QfcDatamodelLivenessTests.Issue950_AmbientSynchronizationContextProbe`.
- `FILTER-R4`: `FullyQualifiedName=QCT.QfcItemController_UiThreadDispatcherFixtureTests.Transaction_SecondCallerCannotInstallUntilTheFirstRestores`.
- `FILTER-CONTROLS-A`: the `FILTER-TARGETS` expressions for test 1, test 3, test 4, the teardown test, the three zero-batch tests and R4 (eight expressions; test 2 excluded).
- `FILTER-CONTROLS-B`: `FullyQualifiedName=QCT.QfcDatamodelLivenessTests.RemainingLoadActive_AcrossAsyncVoidFirstAwait_StaysTrueWhileLoaderProduces`.
- `FILTER-CONTROLS-C`: `FullyQualifiedName=QCT.QfcDatamodelLivenessTests.RemainingLoadActive_AfterLoaderCompletes_BecomesFalse|FullyQualifiedName=QCT.QfcDatamodelLivenessTests.RemainingLoadActive_WhenLoaderThrows_IsStillClearedByFinally` (tests 3 and 4).
- `FILTER-STALL`: `FullyQualifiedName~HelperClasses.ShellUtilities_Tests|FullyQualifiedName~HelperClasses.ShellUtilitiesStatic_Tests|FullyQualifiedName~HelperClasses.SysImageListHelperTests|FullyQualifiedName~EmailIntelligence.OSBrowser_Tests`.

**CMD-COVERAGE-RUNNER** (CLAUDE.md step 4 route; `STAGE` is `baseline` or `final`; `EXIT_CODE:` is `RUNNER_EXIT_CODE:`; canonical command `pwsh -NoProfile -File scripts\vscode\Invoke-MSTestWithCoverage.ps1`):

    PREFIX
    New-Item -ItemType Directory -Path "coverage\logs" -Force | Out-Null
    foreach ($f in @("coverage\coverage.cobertura.xml", "coverage\coverage.cobertura.jacoco.xml", "coverage\test-results\mstest-coverage-run.trx", "coverage\test-results\mstest-coverage-run.summary.txt", "coverage\STAGE-950.cobertura.xml", "coverage\STAGE-950.trx")) { if (Test-Path -LiteralPath $f) { Remove-Item -LiteralPath $f -Force } }
    $script = Join-Path (Get-Location).Path "scripts\vscode\Invoke-MSTestWithCoverage.ps1"
    $global:LASTEXITCODE = 0
    & pwsh -NoProfile -File $script 2>&1 | Tee-Object -FilePath "coverage\logs\STAGE-950.runner.log" | Out-Null
    Write-Output ("RUNNER_EXIT_CODE: " + $LASTEXITCODE)
    $log = Get-Content -LiteralPath "coverage\logs\STAGE-950.runner.log" -Raw -Encoding UTF8
    Write-Output ("DISCOVERED_LINE: " + [regex]::Match($log, "Discovered \d+ test assemblies\.").Value)
    Write-Output ("FIRST_PARTY_LINE: " + [regex]::Match($log, "First-party coverage: [^\r\n]*").Value)
    Write-Output ("THRESHOLD_MESSAGE: " + [regex]::Match($log, "Cobertura (line|branch) coverage [^\r\n]*threshold\.").Value)
    Write-Output ("COLLECT_FAILURE_MESSAGE: " + [regex]::Match($log, "MSTest with coverage failed with exit code \d+").Value)
    Write-Output ("DOCUMENT_PRESENT: " + (Test-Path -LiteralPath "coverage\coverage.cobertura.xml"))
    Write-Output ("TRX_PRESENT: " + (Test-Path -LiteralPath "coverage\test-results\mstest-coverage-run.trx"))
    if (Test-Path -LiteralPath "coverage\coverage.cobertura.xml") { Copy-Item -LiteralPath "coverage\coverage.cobertura.xml" -Destination "coverage\STAGE-950.cobertura.xml" -Force }
    if (Test-Path -LiteralPath "coverage\test-results\mstest-coverage-run.trx") { Copy-Item -LiteralPath "coverage\test-results\mstest-coverage-run.trx" -Destination "coverage\STAGE-950.trx" -Force }
    Write-Output "PAYLOAD-COMPLETE"

The stale-output removal makes every `_PRESENT` value an observation of this run. Runner lines naming resolved paths stay in the ignored log.

**CMD-COVERAGE-DIRECT** (the runner's inner invocation issued directly with the four-class exclusion; `STAGE` substituted; `EXIT_CODE:` is `COLLECT_EXIT_CODE:`; canonical command `dotnet-coverage collect --output coverage\STAGE-950.cobertura.xml --output-format cobertura --settings coverage\effective-coverage-950.config -- vstest.console.exe <discovered test assemblies> /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation "/TestCaseFilter:<filter>" "/ResultsDirectory:coverage\test-results\950\STAGE" "/Logger:trx;LogFileName=STAGE-950.trx" "/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None"`):

    PREFIX
    . (Join-Path (Get-Location).Path "scripts\vscode\Invoke-MSTestWithCoverage.ps1")
    $ErrorActionPreference = "Continue"
    $repo = (Get-Location).Path
    New-Item -ItemType Directory -Path "coverage\logs" -Force | Out-Null
    foreach ($f in @("coverage\STAGE-950.cobertura.xml", "coverage\STAGE-950.trx")) { if (Test-Path -LiteralPath $f) { Remove-Item -LiteralPath $f -Force } }
    $canonical = Get-Content -LiteralPath "coverage.config" -Raw -Encoding UTF8
    $derived = ConvertTo-DerivedCoverageSettingsXml -CanonicalSettingsXml $canonical
    $effective = Join-Path $repo "coverage\effective-coverage-950.config"
    Set-Content -LiteralPath $effective -Value $derived -Encoding UTF8 -NoNewline
    $vswhere = Join-Path ${env:ProgramFiles(x86)} "Microsoft Visual Studio\Installer\vswhere.exe"
    $vstest = & $vswhere -latest -products * -find "Common7\IDE\Extensions\TestPlatform\vstest.console.exe" | Select-Object -First 1
    $rootLen = $repo.TrimEnd([char]92).Length
    $asm = @(Get-ChildItem -Path $repo -Recurse -Filter "*.Test.dll" | Where-Object { $_.FullName -like "*\bin\Debug\*" -and $_.FullName -notlike "*\obj\*" -and $_.FullName -notlike "*\ref\*" -and $_.FullName.Substring($rootLen) -notlike "\.claude\*" } | Select-Object -ExpandProperty FullName)
    $filter = "TestCategory!=LiveOutlook&FullyQualifiedName!~HelperClasses.ShellUtilities_Tests&FullyQualifiedName!~HelperClasses.ShellUtilitiesStatic_Tests&FullyQualifiedName!~HelperClasses.SysImageListHelperTests&FullyQualifiedName!~EmailIntelligence.OSBrowser_Tests"
    $output = Join-Path $repo "coverage\STAGE-950.cobertura.xml"
    $settings = Join-Path $repo "scripts\vscode\TaskMaster.cli.runsettings"
    $results = Join-Path $repo "coverage\test-results\950\STAGE"
    if (Test-Path -LiteralPath $results) { Remove-Item -LiteralPath $results -Recurse -Force }
    $global:LASTEXITCODE = 0
    & dotnet-coverage collect --output $output --output-format cobertura --settings $effective -- $vstest @asm "/Settings:$settings" /InIsolation "/TestCaseFilter:$filter" "/ResultsDirectory:$results" "/Logger:trx;LogFileName=STAGE-950.trx" "/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None" 2>&1 | Tee-Object -FilePath "coverage\logs\STAGE-950.collect.log" | Out-Null
    Write-Output ("COLLECT_EXIT_CODE: " + $LASTEXITCODE)
    Write-Output ("ASSEMBLY_COUNT: " + $asm.Count)
    $asm | ForEach-Object { Write-Output ("ASSEMBLY: " + $_.Substring($rootLen)) }
    Write-Output ("SEQUENCE_FILES: " + @(Get-ChildItem -LiteralPath $results -Recurse -Filter "Sequence_*.xml" -ErrorAction SilentlyContinue).Count)
    if (Test-Path -LiteralPath (Join-Path $results "STAGE-950.trx")) { Copy-Item -LiteralPath (Join-Path $results "STAGE-950.trx") -Destination "coverage\STAGE-950.trx" -Force }
    Write-Output ("TRX_PRESENT: " + (Test-Path -LiteralPath "coverage\STAGE-950.trx"))
    Write-Output ("DOCUMENT_PRESENT: " + (Test-Path -LiteralPath $output))
    Write-Output "PAYLOAD-COMPLETE"

**CMD-COVERAGE-POST** (summarise the trx, post-process if raw, apply the floors, print the committed forms and the figures; `STAGE` substituted; `RAW` is `True` under DIRECT and under a RUNNER run whose `COLLECT_FAILURE_MESSAGE:` is non-empty, otherwise `False`, because a completed runner run has already post-processed the document in place; `EXIT_CODE:` is the payload's own exit status, 0 when every line printed):

    PREFIX
    . (Join-Path (Get-Location).Path "scripts\vscode\Invoke-MSTestWithCoverage.Helpers.ps1")
    . (Join-Path (Get-Location).Path "scripts\vscode\Invoke-MSTest.TrxSummary.ps1")
    $ErrorActionPreference = "Continue"
    $repo = (Get-Location).Path
    $trxText = Get-Content -LiteralPath "coverage\STAGE-950.trx" -Raw -Encoding UTF8
    $summary = Get-TrxRunSummary -TrxContent $trxText
    Write-Output "SUMMARY-BEGIN"
    Write-Output (Format-TrxRunSummary -Summary $summary)
    Write-Output "SUMMARY-END"
    Write-Output ("FAILED-SET: " + ((@($summary.FailedTestName) | Sort-Object -Unique) -join ", "))
    [xml]$trx = $trxText
    $ns = New-Object System.Xml.XmlNamespaceManager($trx.NameTable)
    $ns.AddNamespace("t", "http://microsoft.com/schemas/VisualStudio/TeamTest/2010")
    $names = @(NAMES-TARGETS)
    foreach ($r in @($trx.SelectNodes("//t:UnitTestResult", $ns))) { if ($names -contains $r.GetAttribute("testName")) { Write-Output ("RESULT " + $r.GetAttribute("testName") + " = " + $r.GetAttribute("outcome")) } }
    $doc = Get-Content -LiteralPath "coverage\STAGE-950.cobertura.xml" -Raw -Encoding UTF8
    if ("RAW" -eq "True") { $doc = ConvertTo-KoverageCoberturaXml -XmlContent $doc -RepoRoot $repo; Set-Content -LiteralPath "coverage\STAGE-950.cobertura.xml" -Value $doc -Encoding UTF8 -NoNewline }
    try { Assert-CoberturaLineCoverageThreshold -CoberturaXml $doc; Write-Output "LINE-FLOOR: MET" } catch { Write-Output ("LINE-FLOOR: NOT MET " + $_.Exception.Message) }
    try { Assert-CoberturaBranchCoverageThreshold -CoberturaXml $doc; Write-Output "BRANCH-FLOOR: MET" } catch { Write-Output ("BRANCH-FLOOR: NOT MET " + $_.Exception.Message) }
    Write-Output (Get-CoberturaFirstPartyCoverageReport -CoberturaXml $doc)
    [xml]$xml = $doc
    $root = $xml.SelectSingleNode("/coverage")
    Write-Output ("ROOT line-rate=" + $root.GetAttribute("line-rate") + " branch-rate=" + $root.GetAttribute("branch-rate") + " lines-covered=" + $root.GetAttribute("lines-covered") + " lines-valid=" + $root.GetAttribute("lines-valid") + " branches-covered=" + $root.GetAttribute("branches-covered") + " branches-valid=" + $root.GetAttribute("branches-valid"))
    $projection = ConvertTo-JacocoPackageProjection -XmlDocument $xml
    Assert-JacocoProjectionReconciliation -XmlDocument $xml -ProjectionXml $projection
    Write-Output "PROJECTION-BEGIN"
    Write-Output $projection
    Write-Output "PROJECTION-END"
    $qd = @($xml.SelectNodes("//class[@filename]") | Where-Object { $_.GetAttribute("filename").Replace([string][char]92, "/") -eq "QuickFiler/Controllers/QfcDatamodel.cs" })
    Write-Output ("QFCDATAMODEL-CLASS-NODES: " + $qd.Count)

The projection and the summary block are the two committed forms (CLAUDE.md "Committed Test Evidence Format"); the remaining lines are figures. The filename comparison is an exact equality on the repository-relative path, so the separately instrumented `QuickFiler/Interfaces/IQfcDatamodel.cs` cannot be counted.

**CMD-HASH** (SHA-256 of CODE5; hashes only):

    PREFIX
    foreach ($p in @(CODE5)) { Write-Output ("HASH " + $p + " = " + (Get-FileHash -Algorithm SHA256 -LiteralPath $p).Hash) }

**CMD-LINECOUNT** (content line counts of CODE5):

    PREFIX
    foreach ($p in @(CODE5)) { Write-Output ("LINES " + $p + " = " + @(Get-Content -LiteralPath $p -Encoding UTF8).Count) }

**CMD-TOKEN-COUNT** (`FILE` and the `TOKENS` list substituted; ordinal, case-sensitive substring counts per physical line, so a token wrapped across two lines reads 0):

    PREFIX
    $src = Get-Content -LiteralPath "FILE" -Encoding UTF8
    foreach ($t in @(TOKENS)) { Write-Output ("TOKEN [" + $t + "] = " + @($src | Where-Object { $_.Contains($t) }).Count) }
    Write-Output ("TRIMMED-EQUAL [worker.RunWorkerAsync();] = " + @($src | Where-Object { $_.Trim() -eq "worker.RunWorkerAsync();" }).Count)

**CMD-SPAN-TOKEN-COUNT** (`FILE`, `START`, `END` and `TOKENS` substituted; the span runs from the first line containing START up to, not including, the next line containing END; exit 4 when either anchor is missing):

    PREFIX
    $src = Get-Content -LiteralPath "FILE" -Encoding UTF8
    $s = -1; for ($i = 0; $i -lt $src.Count; $i++) { if ($src[$i].Contains("START")) { $s = $i; break } }
    $e = -1; if ($s -ge 0) { for ($i = $s + 1; $i -lt $src.Count; $i++) { if ($src[$i].Contains("END")) { $e = $i; break } } }
    Write-Output ("SPAN: " + ($s + 1) + "-" + $e)
    if ($s -lt 0 -or $e -lt 0) { exit 4 }
    $span = $src[$s..($e - 1)]
    foreach ($t in @(TOKENS)) { Write-Output ("SPAN-TOKEN [" + $t + "] = " + @($span | Where-Object { $_.Contains($t) }).Count) }

Span anchors used by this plan: `INITQ` is START `public IList<MailItem> InitEmailQueue(` and END `public async Task<IList<MailItem>> InitEmailQueueAsync(` in `QuickFiler\Controllers\QfcDatamodel.cs`; `R4SPAN` is START `public async Task Transaction_SecondCallerCannotInstallUntilTheFirstRestores()` and END `public async Task Transaction_DisposedTwice_DoesNotOverReleaseTheGate()` in `QuickFiler.Test\Controllers\QfcItemController.UiThreadDispatcherFixtureTests.cs`. Three further anchors in the same file locate the R4 pin (fact 5): `R4PRE` is START the R4SPAN START and END `.BeginTransactionAsync()` (the span runs from the R4 declaration up to, not including, the `.BeginTransactionAsync()` line of transaction A's acquisition; it includes that statement's first line, `UiThreadDispatcherTransaction transactionA = await UiThreadDispatcherFixture`, so a pin placed before the acquisition statement is counted in it); `R4HEAD` is START the R4SPAN START and END `Dispatcher original = UiThreadDispatcherFixture.Current;` (from the R4 declaration up to, not including, the `original` read); `R4TAIL` is START `issue #230 lost update` and END `QfcItemControllerTestSupport.ShutdownDispatcher(liveA);` (from R4's last assertion argument up to, not including, the `finally` body). A pin token that is 0 in `R4PRE` and 1 in `R4HEAD` lies after transaction A's acquisition begins and before the `original` read; one more line containing `}` in `R4TAIL` than at BASE places the pin's closing brace after the last assertion and before `finally`.

**CMD-TIMESPAN** (classifies every line of THREE that contains `TimeSpan.`):

    PREFIX
    foreach ($p in @(THREE)) { $src = Get-Content -LiteralPath $p -Encoding UTF8; for ($i = 0; $i -lt $src.Count; $i++) { if ($src[$i].Contains("TimeSpan.")) { $ok = $src[$i].Contains("QuiesceLoaderAsync(") -or $src[$i].Contains("fake.Advance("); Write-Output ("TIMESPAN " + $p + ":" + ($i + 1) + " " + $(if ($ok) { "CLASSIFIED" } else { "UNCLASSIFIED" }) + " :: " + $src[$i].Trim()) } } }
    $n = 0; foreach ($p in @(THREE)) { $n += @(Get-Content -LiteralPath $p -Encoding UTF8 | Where-Object { $_.Contains("TimeSpan.") -and -not ($_.Contains("QuiesceLoaderAsync(") -or $_.Contains("fake.Advance(")) }).Count }
    Write-Output ("TIMESPAN-UNCLASSIFIED: " + $n)

**CMD-ADDED-SCAN** (added lines of the anchored diff over the five code files):

    PREFIX
    $diff = @(git diff 34c2ed88cbb009f2f231453db87bc64d45a9bd51 -- CODE5-GIT)
    Write-Output ("GIT_DIFF_EXIT_CODE: " + $LASTEXITCODE)
    $added = @($diff | Where-Object { $_.StartsWith("+") -and -not $_.StartsWith("+++") })
    Write-Output ("ADDED_LINES: " + $added.Count)
    foreach ($t in @("Thread.Sleep", "Task.Delay", "DoNotParallelize", "Retry(", "Timeout(", "WorkerStarter")) { Write-Output ("ADDED-TOKEN [" + $t + "] = " + @($added | Where-Object { $_.Contains($t) }).Count) }

`WorkerStarter` is the positive control: it is added by this plan, so a scan that cannot see added lines reports 0 for it and the gate fails.

### Phase 0 — Policy Reads, Anchor, Bootstrap and Baseline Capture

- [x] [P0-T1] Read the policy documents in the mandatory order — CLAUDE.md, then .claude/rules/general-code-change.md, then .claude/rules/general-unit-test.md, then .claude/rules/csharp.md — plus .claude/rules/plan-acceptance-gates.md, .claude/rules/tonality.md, .claude/skills/evidence-and-timestamp-conventions/SKILL.md and .claude/skills/acceptance-criteria-tracking/SKILL.md, and record the read in FEATURE/evidence/baseline/phase0-instructions-read.md.
  - Acceptance: the artifact carries `Timestamp:`, a `Policy Order:` line naming the four mandatory documents in that order, and one line per document read. No policy document is modified.
- [x] [P0-T2] Read FEATURE/spec.md, FEATURE/issue.md and FEATURE/research/2026-10-01T00-00-wall-clock-waits-research.md in full and record the Write Set, the prohibited paths and the acceptance-criteria inventory in FEATURE/evidence/baseline/scope-and-anchor.md (this task creates the file; P0-T3 appends to it).
  - Acceptance: the artifact lists the five code paths of the Write Set verbatim; names the prohibited paths from the Write Set section; records that issue.md line 12 reads `- Work Mode: full-bug`; and records, counted from the file, that spec.md holds exactly 17 lines beginning `- [ ] AC` and 0 lines beginning `- [x] AC`.
- [x] [P0-T3] Record the anchor and the pre-change tree state by appending to FEATURE/evidence/baseline/scope-and-anchor.md.
  - Commands, each a separate Bash call: `git -C WORKTREE rev-parse HEAD`; `git -C WORKTREE rev-parse --abbrev-ref HEAD`; `git -C WORKTREE merge-base --is-ancestor 34c2ed88cbb009f2f231453db87bc64d45a9bd51 HEAD`; `git -C WORKTREE merge-base origin/main HEAD`; `git -C WORKTREE diff --name-status 34c2ed88cbb009f2f231453db87bc64d45a9bd51 HEAD`; `git -C WORKTREE diff --exit-code 34c2ed88cbb009f2f231453db87bc64d45a9bd51 HEAD -- QuickFiler QuickFiler.Test UtilitiesCS/Threading/UiThread.cs scripts/vscode TaskMaster.runsettings`; `git -C WORKTREE status --porcelain --untracked-files=all`.
  - Acceptance, all required: `HEAD-SHA:` records the first output as an observation; `BRANCH:` reads `bug/quickfiler-tests-depend-on-wall-clock-timing-950`; the ancestor check exits 0 and the merge-base command prints exactly `34c2ed88cbb009f2f231453db87bc64d45a9bd51` (otherwise `BASE-SHA MISMATCH`: record both values and stop); `INHERITED-COMMITTED:` lists every name-status line or `NONE`, and every listed path is under FEATURE or is exactly `docs/features/potential/promoted/2026-09-30-quickfiler-tests-depend-on-wall-clock-timing.md` (otherwise `INHERITED SET OUT OF SCOPE`: stop); the scoped `--exit-code` diff exits 0, recorded as `CODE-TREE-AT-BASE: UNCHANGED` (otherwise `CITED TREE ADVANCED`: stop, because every line citation in this plan is against BASE); `PRE-EXISTING-WORKTREE-PATHS:` lists every porcelain line verbatim or `NONE`, and no line names a path under QuickFiler/ or QuickFiler.Test/ (otherwise `CODE TREE DIRTY AT ANCHOR`: stop). The porcelain output is not asserted empty.
- [x] [P0-T4] Provision the repository .NET SDK with scripts/vscode/Install-RepoDotNetSdk.ps1 (guarded) and record it in FEATURE/evidence/baseline/bootstrap-sdk.md.
  - Command: `pwsh -NoProfile -Command 'PREFIX; if (-not (Test-Path -LiteralPath ".dotnet-sdk\sdk\8.0.205")) { & (Join-Path (Get-Location).Path "scripts\vscode\Install-RepoDotNetSdk.ps1") }; "SDK_MARKER=$(Test-Path -LiteralPath ".dotnet-sdk\sdk\8.0.205")"; dotnet --version; "DOTNET_EXIT=$LASTEXITCODE"'` (PREFIX expanded to its three lines, joined with semicolons).
  - Acceptance: `WORKTREE-LEAF: agent-a7805823735145ca4`, `SDK_MARKER=True`, `DOTNET_EXIT=0`, and the version line is a version string rather than the global.json `errorMessage` text. The installer's filesystem marker is the gate; version equality is not asserted because global.json rolls forward within the feature band.
- [x] [P0-T5] Restore the manifest tools with `dotnet tool restore` at the worktree root (manifest dotnet-tools.json) and record it in FEATURE/evidence/baseline/bootstrap-tool-restore.md.
  - Command: `pwsh -NoProfile -Command 'PREFIX; dotnet tool restore; "RESTORE_EXIT=$LASTEXITCODE"; dotnet tool list --local; dotnet tool run csharpier check --help | Out-Null; "CHECK_HELP_EXIT=$LASTEXITCODE"'`.
  - Acceptance: `WORKTREE-LEAF: agent-a7805823735145ca4`, `RESTORE_EXIT=0`, a local tool row with Package Id `csharpier` and Version `1.2.6`, and `CHECK_HELP_EXIT=0`. Only the Package Id and Version columns are transcribed (the Manifest column carries an absolute path).
- [x] [P0-T6] Restore NuGet packages with scripts/vscode/Invoke-Restore.ps1 and record it in FEATURE/evidence/baseline/bootstrap-nuget-restore.md.
  - Command: `pwsh -NoProfile -Command 'PREFIX; $env:MSBUILDDISABLENODEREUSE = "1"; & (Join-Path (Get-Location).Path "scripts\vscode\Invoke-Restore.ps1"); "RESTORE_EXIT=$LASTEXITCODE"; "PACKAGE_DIRS=$(@(Get-ChildItem -LiteralPath packages -Directory -ErrorAction SilentlyContinue).Count)"'`.
  - Acceptance: `WORKTREE-LEAF: agent-a7805823735145ca4`, `RESTORE_EXIT=0` and `PACKAGE_DIRS=` at least 1.
- [x] [P0-T7] Verify analyzer-path alignment across every first-party project file (every `*.csproj` outside `packages\` and `.claude\`) and record it in FEATURE/evidence/baseline/analyzer-alignment.md.
  - Command: `pwsh -NoProfile -Command 'PREFIX; $root = (Get-Location).Path; $rootLen = $root.TrimEnd([char]92).Length; $projs = @(Get-ChildItem -Path $root -Recurse -Filter "*.csproj" | Where-Object { $rel = $_.FullName.Substring($rootLen); $rel -notlike "\packages\*" -and $rel -notlike "\.claude\*" }); "PROJECTS=$($projs.Count)"; $missing = 0; $skew = 0; foreach ($p in $projs) { $dir = $p.DirectoryName; [xml]$x = Get-Content -LiteralPath $p.FullName -Raw; foreach ($a in @($x.SelectNodes("//*[local-name()=""Analyzer""]"))) { $inc = $a.GetAttribute("Include"); if (-not (Test-Path -LiteralPath (Join-Path $dir $inc))) { $missing++; "MISSING " + $p.FullName.Substring($rootLen) + " :: " + $inc } }; $pc = Join-Path $dir "packages.config"; if (Test-Path -LiteralPath $pc) { [xml]$c = Get-Content -LiteralPath $pc -Raw; foreach ($id in @("Meziantou.Analyzer", "Roslynator.Analyzers")) { $pin = @($c.SelectNodes("//package[@id=""$id""]") | ForEach-Object { $_.GetAttribute("version") }); $inc = @($x.SelectNodes("//*[local-name()=""Analyzer""]") | ForEach-Object { $_.GetAttribute("Include") } | Where-Object { $_.Contains("\$id.") }); foreach ($i in $inc) { if ($pin.Count -eq 0 -or -not $i.Contains("\$id." + $pin[0] + "\")) { $skew++; "SKEW " + $p.FullName.Substring($rootLen) + " :: " + $i } } } } }; "ANALYZER_MISSING=$missing"; "VERSION_SKEW=$skew"'`.
  - Acceptance: `WORKTREE-LEAF: agent-a7805823735145ca4`, `PROJECTS=` at least 1, `ANALYZER_MISSING=0` and `VERSION_SKEW=0`. A non-zero value is `ANALYZER PATH SKEW`: record every `MISSING` and `SKEW` line and stop; it is an environment defect, not a plan defect, and no version number is asserted here because pins move.
- [x] [P0-T8] Provision the dotnet-coverage global tool (guarded) and record it in FEATURE/evidence/baseline/bootstrap-dotnet-coverage.md.
  - Command: `pwsh -NoProfile -Command 'if (-not (Get-Command dotnet-coverage -ErrorAction SilentlyContinue)) { dotnet tool install --global dotnet-coverage }; "DOTNET_COVERAGE_RESOLVED=$($null -ne (Get-Command dotnet-coverage -ErrorAction SilentlyContinue))"; dotnet-coverage --version'` (no PREFIX: the command touches no repository path; `WORKTREE-LEAF:` is recorded as `not applicable`).
  - Acceptance: `DOTNET_COVERAGE_RESOLVED=True`, a version line is printed, `EXIT_CODE: 0`.
- [x] [P0-T9] Capture the read-only formatter baseline with `dotnet tool run csharpier check .` and record it in FEATURE/evidence/baseline/csharpier-check-baseline.md.
  - Command: `pwsh -NoProfile -Command 'PREFIX; dotnet tool run csharpier check .; "CSHARPIER_EXIT_CODE: $LASTEXITCODE"'`.
  - Acceptance: `WORKTREE-LEAF: agent-a7805823735145ca4`; `EXIT_CODE:` is the printed `CSHARPIER_EXIT_CODE:` value; the success-case line beginning `Checked ` and ending `ms.` is transcribed; `EXIT_CODE: 0` is the gate. A non-zero value lists every reported path and is `FORMAT BASELINE NOT CLEAN`: stop, because the CLAUDE.md format step (`csharpier format .`) would then rewrite files outside the Write Set and the decision belongs to the orchestrator.
- [x] [P0-T10] Capture the analyzer baseline with `CMD-REBUILD` (analyzer GATEARGS, `TASKID` p0-t10) and record it in FEATURE/evidence/baseline/msbuild-analyzer-baseline.md.
  - Acceptance, all required: `WORKTREE-LEAF: agent-a7805823735145ca4`; `EXIT_CODE: 0`; `ERRORS: 0`; `SKIP_CORECOMPILE_LINES: 0`; `CSC_OUT_QUICKFILER:` and `CSC_OUT_QUICKFILER_TEST:` each at least 1; `TEST_DLL_EXISTS: True`; `UCS_TEST_DLL_EXISTS: True`; `WARNINGS:` recorded as `ANALYZER-BASELINE-WARNINGS:`; `WRITESET_DIAGNOSTIC_LINES:` and `WRITESET_DIAGNOSTIC_CODES:` recorded as `ANALYZER-BASELINE-WRITESET-LINES:` and `ANALYZER-BASELINE-WRITESET-CODES:` (the comparison basis for P6-T3). A non-zero exit is `ANALYZER BASELINE NOT CLEAN`: stop.
- [x] [P0-T11] Capture the nullable baseline with `CMD-REBUILD` (nullable GATEARGS `/p:TreatWarningsAsErrors=true`, no Nullable property override, `TASKID` p0-t11) and record it in FEATURE/evidence/baseline/msbuild-nullable-baseline.md.
  - Acceptance, all required: `WORKTREE-LEAF: agent-a7805823735145ca4`; `EXIT_CODE: 0`; `ERRORS: 0`; `SKIP_CORECOMPILE_LINES: 0`; both `CSC_OUT_` counts at least 1; both `_DLL_EXISTS:` values `True`. A non-zero exit is `NULLABLE BASELINE NOT CLEAN`: stop.
- [x] [P0-T12] Record the pre-change census of the five code files in FEATURE/evidence/baseline/census-baseline.md, using `CMD-LINECOUNT`, `CMD-HASH`, `CMD-TIMESPAN`, `CMD-TOKEN-COUNT` once per code file and `CMD-SPAN-TOKEN-COUNT` for `INITQ`, `R4SPAN`, `R4PRE`, `R4HEAD` and `R4TAIL`.
  - Token lists: for each of THREE, `"SpinWait", ".Wait(", "WaitForState", "new BackgroundWorker()", "new SynchronousBackgroundWorker()", "WorkerStarter = StartSynchronously;", "IsBusy", "starts a real"`; for QfcDatamodel.cs, `"WorkerStarter", "RemainingEmailLoader = LoadRemainingEmailsToQueueAsync;"`; for the R4 file, `"flake-watch", "Append an observation", "Issue #950:", "[Timeout(GateTimeoutMs)]", "private const int GateTimeoutMs = 60000;"`; for `INITQ`, `"RunWorkerAsync", "WorkerStarter(worker);"`; for `R4SPAN`, `"IDisposable baseline = QfcItemControllerTestSupport.EnsureUiThreadDispatcher()", "EnsureUiThreadDispatcher()", "using (", ".BeSameAs(", ".NotBeSameAs("`; for `R4PRE` and `R4HEAD`, `"EnsureUiThreadDispatcher()", "using ("`; for `R4TAIL`, `"}"`.
  - Acceptance (each value is derived in facts 1 to 5 and is a falsifiable pre-change observation): `WORKTREE-LEAF: agent-a7805823735145ca4`; LINES 483, 255, 235, 212, 458 in CODE5 order; five HASH values recorded as `BASE-HASH:` lines; liveness tokens 1, 2, 5, 2, 0, 0, 6, 0; teardown tokens 1, 1, 2, 1, 0, 0, 0, 0; zero-batch tokens 0, 1, 0, 3, 0, 0, 2, 1; QfcDatamodel.cs `WorkerStarter` 0, the loader-assignment literal 2, `TRIMMED-EQUAL [worker.RunWorkerAsync();] = 2`; R4 file tokens 1, 1, 0, 8, 1; `INITQ` `RunWorkerAsync` 2 and `WorkerStarter(worker);` 0; `R4SPAN` tokens 0, 0, 1, 1, 1 with `SPAN: 206-272`; `R4PRE` 0 and 0 with `SPAN: 206-212`; `R4HEAD` 0 and 0 with `SPAN: 206-214`; `R4TAIL` 2 with `SPAN: 256-261`; `TIMESPAN-UNCLASSIFIED: 6` (liveness 56, 103, 173; teardown 67, 220; zero-batch 161). Any differing value is `CENSUS MISMATCH`: record and stop, because a later gate is defined against these values. The non-zero wall-clock counts are the positive control for the zero gates of P6-T8.
- [x] [P0-T13] Capture the pre-change run of the nine target tests with `CMD-VSTEST` (`ASSEMBLY-QF`, `FILTER-TARGETS`, `TASKID` p0-t13, `NAMES-TARGETS`) and record it in FEATURE/evidence/baseline/targets-baseline.md.
  - Acceptance: `WORKTREE-LEAF: agent-a7805823735145ca4`; `TRX_PRESENT: True`; `SEQUENCE_FILES: 0`; `RESULT_COUNT: 9` with one `RESULT` line per target name and its outcome; `BASELINE-TARGETS-NOT-PASSED:` lists every non-Passed name with its `MESSAGE` line, or `NONE`. Outcomes are observations of the pre-fix, load-dependent tests and are not gated; `ExpectedExitCode:` carries the observed value when non-zero.
- [x] [P0-T14] Capture the pre-change concurrent run of the four target classes with QfcItemController_FocusAndThemeTests using `CMD-VSTEST` (`ASSEMBLY-QF`, `FILTER-CONCURRENT`, `TASKID` p0-t14, empty `NAMES`) and record it in FEATURE/evidence/baseline/concurrent-classes-baseline.md.
  - Acceptance: `WORKTREE-LEAF: agent-a7805823735145ca4`; `TRX_PRESENT: True`; `SEQUENCE_FILES: 0`; the `COUNTERS` line recorded as `BASELINE-CONCURRENT-COUNTERS:`; every `RESULT` line transcribed; `BASELINE-CONCURRENT-FAILED:` lists every non-Passed name or `NONE` (the comparison basis for P4-T5). `ExpectedExitCode:` carries the observed value when non-zero; nothing here is gated beyond trx presence and no hang.
- [x] [P0-T15] Run the stall probe with `CMD-VSTEST` (`ASSEMBLY-UCS`, `FILTER-STALL`, `TASKID` p0-t15, empty `NAMES`) and record FEATURE/evidence/baseline/stall-probe.md.
  - Acceptance: the artifact records `WORKTREE-LEAF:`, `EXIT_CODE:`, `ExpectedExitCode:` equal to the observed value when non-zero (presentational), `TRX_PRESENT:`, `SEQUENCE_FILES:`, the `COUNTERS` line when present and every `MESSAGE` line; then exactly one `STALL-PROBE:` line — `CLEAR` when `EXIT_CODE: 0`, `failed=0` and `SEQUENCE_FILES: 0`, otherwise `REPRODUCES` — and exactly one `COVERAGE-ROUTE:` line — `RUNNER` under CLEAR, `DIRECT` under REPRODUCES. The probe runs once and is never re-run. Both values complete this task.
- [x] [P0-T16] Capture the baseline repository-wide test-and-coverage run by the route P0-T15 fixed and record FEATURE/evidence/baseline/coverage-baseline.md: under RUNNER run `CMD-COVERAGE-RUNNER` with `STAGE` baseline, under DIRECT run `CMD-COVERAGE-DIRECT` with `STAGE` baseline; then, unless branch (d) applies, run `CMD-COVERAGE-POST` with `STAGE` baseline and `RAW` per its rule.
  - Artifact: `Timestamp:`, `Command:` (both payloads, with the route's canonical command), `EXIT_CODE:` (`RUNNER_EXIT_CODE:` or `COLLECT_EXIT_CODE:`), `ExpectedExitCode:` equal to the observed value when non-zero (a baseline observation), and an `Output Summary:` recording `WORKTREE-LEAF:`, `COVERAGE-ROUTE:`, `RAW:`, `DISCOVERED_LINE:` or `ASSEMBLY_COUNT:` with every `ASSEMBLY:` line, `TRX_PRESENT:`, `SEQUENCE_FILES:` (DIRECT), `THRESHOLD_MESSAGE:` and `COLLECT_FAILURE_MESSAGE:` (RUNNER), `LINE-FLOOR:`, `BRANCH-FLOOR:`, the `First-party coverage:` line (the numeric baseline headline: lines covered over valid with percentage, branches likewise), the `ROOT` line, the projection verbatim between `PROJECTION-BEGIN` and `PROJECTION-END`, the five summary lines verbatim between `SUMMARY-BEGIN` and `SUMMARY-END`, `FAILED-SET:` recorded as `BASELINE-FAILED-SET:`, the nine `RESULT` lines and `QFCDATAMODEL-CLASS-NODES:`.
  - Branches, checked in order: (d) `TRX_PRESENT: False`, `SEQUENCE_FILES:` greater than 0, or a non-zero exit with an empty `FAILED-SET:` and no floor message, is `COVERAGE RUN ABORTED`: stop, report the last lines of the log with paths redacted, do not re-run. (c) `QFCDATAMODEL-CLASS-NODES:` other than 0 is `QFCDATAMODEL INSTRUMENTED`: stop, because D-7 rests on fact 1. (b) a non-zero exit with a non-empty `FAILED-SET:` or a floor `NOT MET` is recorded as `BASELINE-STATE: PRE-EXISTING FAILURES` (with `BASELINE-FLOOR:` naming any floor not met) and completes this task (D-8). (a) exit 0 with both floors met is `BASELINE-STATE: GREEN` and completes this task.
- [x] [P0-T17] Commit the Phase 0 evidence (FEATURE only) and record it in FEATURE/evidence/baseline/phase0-commit.md.
  - Commands, separate Bash calls: `git -C WORKTREE add -- docs/features/active/2026-09-30-quickfiler-tests-depend-on-wall-clock-timing-950`; `git -C WORKTREE commit -m "docs(950): record phase 0 baseline evidence"`; `git -C WORKTREE rev-parse HEAD`; `git -C WORKTREE status --porcelain --untracked-files=all`.
  - Acceptance: both git commands exit 0; `PHASE0-COMMIT:` records the new HEAD as an observation; no porcelain line names a path under FEATURE other than this plan file (whose check-off mark is written after the commit) and FEATURE/evidence/baseline/phase0-commit.md (written after the commit), and no porcelain line names a path under QuickFiler/ or QuickFiler.Test/. This artifact itself is committed in P4-T7.

### Phase 1 — Execution-Time Observation and Fail-Before Evidence (temporary edits on the unmodified tree)

- [x] [P1-T1] Insert Delivered Source P-PROBE into QuickFiler.Test/Controllers/QfcDatamodelLivenessTests.cs after line 253 (temporary; reverted by P1-T7).
  - Acceptance (recorded by P1-T3): the file contains exactly one line containing `public void Issue950_AmbientSynchronizationContextProbe()` and exactly one line containing `PROBE-ALWAYS-FAILS`; no other line changed.
- [x] [P1-T2] Insert Delivered Source R-INJECT into QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixtureTests.cs immediately before line 216 `transactionA.Install(liveA);` (temporary; reverted by P1-T7).
  - Acceptance (recorded by P1-T3): `R4SPAN` `EnsureUiThreadDispatcher()` count is 1 and the inserted line sits between the line containing `Dispatcher original = UiThreadDispatcherFixture.Current;` (pre-edit 215) and the line containing `transactionA.Install(liveA);`; no other line changed.
- [x] [P1-T3] Record the temporary-edit census in FEATURE/evidence/regression-testing/phase1-temporary-edits.md using `CMD-TOKEN-COUNT` on the liveness file (TOKENS `"public void Issue950_AmbientSynchronizationContextProbe()", "PROBE-ALWAYS-FAILS"`), `CMD-SPAN-TOKEN-COUNT` on `R4SPAN` (TOKENS `"EnsureUiThreadDispatcher()"`) and `git -C WORKTREE diff --numstat HEAD -- QuickFiler.Test/Controllers/QfcDatamodelLivenessTests.cs QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixtureTests.cs` paired with `git -C WORKTREE status --porcelain -- QuickFiler QuickFiler.Test`.
  - Acceptance: both liveness tokens 1; the R4 span token 1; numstat `15	0` for the liveness file and `1	0` for the R4 file; the porcelain span lists exactly those two paths with status ` M`.
- [x] [P1-T4] Build the temporarily edited tree with `CMD-BUILD` (`TASKID` p1-t4) and record it in FEATURE/evidence/regression-testing/phase1-build.md.
  - Acceptance: `WORKTREE-LEAF: agent-a7805823735145ca4`, `EXIT_CODE: 0`, `ERRORS: 0`, `TEST_DLL_ADVANCED: True`, `CSC_OUT_QUICKFILER_TEST:` at least 1.
- [x] [P1-T5] [expect-fail] Run the AC16 probe alone with `CMD-VSTEST` (`ASSEMBLY-QF`, `FILTER-PROBE`, `TASKID` p1-t5, `NAMES` `"Issue950_AmbientSynchronizationContextProbe"`) under the repository runsettings and record FEATURE/evidence/other/ambient-synchronization-context.md.
  - Acceptance: `WORKTREE-LEAF: agent-a7805823735145ca4`; `EXIT_CODE: 1`; `ExpectedExitCode: 1` (the probe fails by construction, D-4); `TRX_PRESENT: True`; `SEQUENCE_FILES: 0`; `RESULT_COUNT: 1`; the `RESULT` line reads `Failed`; the `MESSAGE` line contains `AMBIENT-SYNC-CONTEXT=`; and the artifact carries exactly one line `AMBIENT-SYNCHRONIZATION-CONTEXT:` followed by the text the message carries after `AMBIENT-SYNC-CONTEXT=` up to the next space, plus `THREAD-POOL:` and `APARTMENT:` lines copied the same way. A `RESULT` of `Passed`, or a message without `AMBIENT-SYNC-CONTEXT=`, means the probe did not run as written: stop. The recorded value is an observation (spec Assumptions); no value is required, because tests 3 and 4 install their own context.
- [x] [P1-T6] [expect-fail] Run the deterministic R4 fail-before repro alone with `CMD-VSTEST` (`ASSEMBLY-QF`, `FILTER-R4`, `TASKID` p1-t6, `NAMES` `"Transaction_SecondCallerCannotInstallUntilTheFirstRestores"`) and record FEATURE/evidence/regression-testing/r4-fail-before.md.
  - Acceptance: `WORKTREE-LEAF: agent-a7805823735145ca4`; `EXIT_CODE: 1`; `ExpectedExitCode: 1`; `TRX_PRESENT: True`; `SEQUENCE_FILES: 0`; `RESULT_COUNT: 1`; the `RESULT` line reads `Failed` with its duration recorded; the `MESSAGE` line contains `to refer to` and `ParkedDispatcher` (the CI failure signature of spec Repro and Evidence: the second caller observed the parked dispatcher where the null baseline was expected). The artifact states the edit applied (R-INJECT on the unmodified file), that the test ran alone so the baseline was null (fact 7), and that this run is both the Defect B fail-before evidence and the R4 negative control of the spec Test Strategy table. A `Passed` result is `R4 REPRO DID NOT FAIL`: stop and report, because the root-cause claim rests on it.
- [x] [P1-T7] Revert both temporary edits to HEAD and verify the revert byte-for-byte, recording FEATURE/evidence/regression-testing/phase1-revert.md.
  - Commands, separate Bash calls: `git -C WORKTREE restore --source=HEAD --worktree -- QuickFiler.Test/Controllers/QfcDatamodelLivenessTests.cs QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixtureTests.cs`; `git -C WORKTREE diff --exit-code HEAD -- QuickFiler.Test/Controllers/QfcDatamodelLivenessTests.cs QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixtureTests.cs`; `git -C WORKTREE status --porcelain -- QuickFiler QuickFiler.Test`; then `CMD-HASH`.
  - Acceptance: the restore exits 0; the anchored diff exits 0 and prints nothing; the porcelain span prints nothing; the five HASH values equal the P0-T12 `BASE-HASH:` values.
- [x] [P1-T8] Author the Defect A fail-before exception dossier FEATURE/evidence/regression-testing/fail-before-exception.<timestamp>.md (timestamp yyyy-MM-ddTHH-mm at authoring).
  - Required content: `Timestamp:`; `Defect: A (wall-clock waits on a thread-pool worker)`; `WhyFailingRunImpossible:` stating in one to three sentences that the Defect A failure occurs only when the thread pool delays the BackgroundWorker body past a five-second bound, and that no deterministic, policy-compliant test can force that delay (sleeps, delays and wall-clock waits are prohibited, and starving the thread pool changes process-wide state that other parallel classes share); and an `## Alternative proof` section citing (1) the P0-T12 census (`SpinWait` 1 and 1, `.Wait(` 2, 1 and 1, `WaitForState` 5 and 2, `TIMESPAN-UNCLASSIFIED: 6`), (2) the two direct start sites QfcDatamodel.cs lines 273 and 300 with `WorkerStarter` absent (P0-T12), (3) the recorded failure history from spec Context (two failures at the #944 P3-T8 gate, one in the #929 local run), and (4) a forward statement that P5-T24 appends the post-fix deterministic negative controls.
  - Acceptance: exactly one file matching `fail-before-exception.*.md` exists under FEATURE/evidence/regression-testing/, and it carries `Timestamp:`, `WhyFailingRunImpossible:` and the `## Alternative proof` heading with items (1) to (4).

### Phase 2 — Production Seam in QfcDatamodel.cs

- [x] [P2-T1] Insert Delivered Source S1 (the `WorkerStarter` property and its XML documentation) into QuickFiler/Controllers/QfcDatamodel.cs after line 140. This task, P2-T2 and P2-T3 apply S1 to S3 as in-place edits that preserve the file's leading UTF-8 byte-order mark (gated by `QFCDATAMODEL-BOM: True` in P2-T4); the file is never rewritten whole.
  - Acceptance (recorded by P2-T4): the file contains exactly one line containing `internal Action<BackgroundWorker> WorkerStarter { get; set; }`, one containing `Injectable worker-start seam` and one containing `null on instances built by GetUninitializedObject`; the property sits before `#endregion Private Variables`.
- [x] [P2-T2] Insert Delivered Source S2 into both constructors of QuickFiler/Controllers/QfcDatamodel.cs, each immediately after that constructor's `RemainingEmailLoader = LoadRemainingEmailsToQueueAsync;` line (pre-edit 40 and 51).
  - Acceptance (recorded by P2-T4): exactly two lines contain `WorkerStarter = worker => worker.RunWorkerAsync();`, each directly below one of the two loader-assignment lines.
- [x] [P2-T3] Replace both start sites in `InitEmailQueue` of QuickFiler/Controllers/QfcDatamodel.cs (pre-edit lines 273 and 300) with Delivered Source S3, keeping each line's indentation.
  - Acceptance (recorded by P2-T4): `TRIMMED-EQUAL [worker.RunWorkerAsync();] = 0`; within `INITQ`, `RunWorkerAsync` 0 and `WorkerStarter(worker);` 2.
- [x] [P2-T4] Record the production seam census in FEATURE/evidence/qa-gates/production-seam-census.md with `CMD-TOKEN-COUNT` on QuickFiler\Controllers\QfcDatamodel.cs (TOKENS `"internal Action<BackgroundWorker> WorkerStarter { get; set; }", "WorkerStarter = worker => worker.RunWorkerAsync();", "Injectable worker-start seam", "null on instances built by GetUninitializedObject", "RemainingEmailLoader = LoadRemainingEmailsToQueueAsync;"`), `CMD-SPAN-TOKEN-COUNT` on `INITQ` (TOKENS `"RunWorkerAsync", "WorkerStarter(worker);"`) and `CMD-LINECOUNT`, and `pwsh -NoProfile -Command 'PREFIX; $b = [System.IO.File]::ReadAllBytes((Join-Path (Get-Location).Path "QuickFiler\Controllers\QfcDatamodel.cs")); "QFCDATAMODEL-BOM: " + ($b.Length -ge 3 -and $b[0] -eq 239 -and $b[1] -eq 187 -and $b[2] -eq 191)'`.
  - Acceptance: tokens 1, 2, 1, 1, 2; `TRIMMED-EQUAL [worker.RunWorkerAsync();] = 0`; `INITQ` 0 and 2; QfcDatamodel.cs LINES 495; `QFCDATAMODEL-BOM: True` (the file carries a UTF-8 byte-order mark at BASE; S1 to S3 are applied as in-place edits that keep it, because an edit that drops it rewrites line 1 and P4-T2's numstat then reads `15	3`). Any other value: correct the edit and re-run this task.

### Phase 3 — Test Rewrites

- [ ] [P3-T1] Replace lines 47 to 57 of QuickFiler.Test/Controllers/QfcDatamodelLivenessTests.cs (the `WaitForState` documentation and method) with Delivered Source L1.
  - Acceptance: (recorded by P3-T15) one line contains `private sealed class SynchronousBackgroundWorker : BackgroundWorker` and one contains `private sealed class DrainableSynchronizationContext : SynchronizationContext`.
- [ ] [P3-T2] Replace the arrange-act-wait block of test 1 in QuickFiler.Test/Controllers/QfcDatamodelLivenessTests.cs (pre-edit lines 97 to 110) with Delivered Source L2; lines from `Task<IList<MailItem>> pending = ...` onward are unchanged.
  - Acceptance (recorded by P3-T15): the test contains `new SynchronousBackgroundWorker()`, `model.WorkerStarter = StartSynchronously;` and the reason literal `the synchronous starter must reach the injected RemainingEmailLoader`, and no `.Wait(` or `WaitForState`.
- [ ] [P3-T3] Replace `StartHeldOpenLoader` and its documentation, the following blank line and the summary of test 2 `RemainingLoadActive_AcrossAsyncVoidFirstAwait_StaysTrueWhileLoaderProduces` in QuickFiler.Test/Controllers/QfcDatamodelLivenessTests.cs (pre-edit lines 148 to 187) with Delivered Source L3.
  - Acceptance (recorded by P3-T15): the method contains `new SynchronousBackgroundWorker()`, `model.WorkerStarter = StartSynchronously;` and the reason literal `the synchronous starter must reach the injected loader before returning`, and the method creates `localRelease` with `TaskCreationOptions.RunContinuationsAsynchronously`; test 2's summary no longer mentions `IsBusy` (the liveness `IsBusy` count is 1, the issue #424 line of test 1's documentation).
- [ ] [P3-T4] Replace test 3 `RemainingLoadActive_AfterLoaderCompletes_BecomesFalse` and its documentation in QuickFiler.Test/Controllers/QfcDatamodelLivenessTests.cs (pre-edit lines 207 to 227) with Delivered Source L4.
  - Acceptance (recorded by P3-T15): the test installs `DrainableSynchronizationContext`, calls `pump.Drain();` after `release.SetResult(true);`, restores the previous context in a `finally`, and keeps the reason literal `the finally around the awaited loader must clear the flag once it completes`.
- [ ] [P3-T5] Replace test 4 `RemainingLoadActive_WhenLoaderThrows_IsStillClearedByFinally` and its documentation in QuickFiler.Test/Controllers/QfcDatamodelLivenessTests.cs (pre-edit lines 229 to 253) with Delivered Source L5.
  - Acceptance (recorded by P3-T15): the test installs `DrainableSynchronizationContext`, calls `pump.Drain();` after `release.SetResult(true);`, restores the previous context in a `finally`, and keeps the reason literal `the finally must clear the flag on the throwing path too`.
- [ ] [P3-T6] Replace lines 59 to 67 of QuickFiler.Test/Controllers/QfcDatamodelTeardownTests.cs (the `WaitForState` documentation and method) with Delivered Source T1.
  - Acceptance: (recorded by P3-T15) one line contains `private sealed class SynchronousBackgroundWorker : BackgroundWorker`.
- [ ] [P3-T7] Replace the `using` block of `Worker_DoWork_CapturesRemainingLoadTask` in QuickFiler.Test/Controllers/QfcDatamodelTeardownTests.cs (pre-edit lines 214 to 232) with Delivered Source T2.
  - Acceptance (recorded by P3-T15): the teardown file has `SpinWait` 0, `.Wait(` 0, `WaitForState` 0, `new SynchronousBackgroundWorker()` 1 and `WorkerStarter = StartSynchronously;` 1; the four other tests in the file are unchanged (their `QuiesceLoaderAsync(TimeSpan.FromSeconds(5))` and `fake.Advance(TimeSpan.FromSeconds(6))` arguments stay).
- [ ] [P3-T8] Insert Delivered Source Z-H into QuickFiler.Test/Controllers/QfcInitEmailQueueZeroBatchTests.cs after line 108.
  - Acceptance (recorded by P3-T15): one line contains `private sealed class SynchronousBackgroundWorker : BackgroundWorker` and one contains `private static void StartSynchronously(BackgroundWorker worker) =>`.
- [ ] [P3-T9] Apply Delivered Source Z0 to `InitEmailQueue_ZeroBatchSize_ReturnsEmptyListWithoutThrowing` in QuickFiler.Test/Controllers/QfcInitEmailQueueZeroBatchTests.cs.
  - Acceptance (recorded by P3-T15): the test assigns `model.WorkerStarter = StartSynchronously;` and its act lambda constructs `new SynchronousBackgroundWorker()`.
- [ ] [P3-T10] Replace the documentation and method of `InitEmailQueue_ZeroBatchSize_StillStartsBackgroundWorker` in QuickFiler.Test/Controllers/QfcInitEmailQueueZeroBatchTests.cs with Delivered Source Z1.
  - Acceptance (recorded by P3-T15): the test asserts the loader-invoked signal with the reason literal `the injected RemainingEmailLoader must be invoked by the started worker`; the documentation no longer mentions a bounded timeout.
- [ ] [P3-T11] Apply Delivered Source Z2 to `InitEmailQueue_PositiveBatchSize_RetainsExistingProjectionAndFrameDrop` in QuickFiler.Test/Controllers/QfcInitEmailQueueZeroBatchTests.cs.
  - Acceptance (recorded by P3-T15): the zero-batch file has `.Wait(` 0, `new BackgroundWorker()` 0, `new SynchronousBackgroundWorker()` 3 and `WorkerStarter = StartSynchronously;` 3.
- [ ] [P3-T12] Apply Delivered Source R-BODY (the baseline pin taken inside the gate: four lines inserted after pre-edit line 214, pre-edit lines 215 to 258 re-indented by four spaces, one closing line inserted after pre-edit line 258) to `Transaction_SecondCallerCannotInstallUntilTheFirstRestores` in QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixtureTests.cs. This task runs before P3-T13, so the pre-edit line numbers are current.
  - Acceptance (recorded by P3-T15): within `R4SPAN`, `IDisposable baseline = QfcItemControllerTestSupport.EnsureUiThreadDispatcher()` 1, `EnsureUiThreadDispatcher()` 1, `using (` 2, `.BeSameAs(` 1 and `.NotBeSameAs(` 1; within `R4PRE`, `EnsureUiThreadDispatcher()` 0 and `using (` 0; within `R4HEAD`, `EnsureUiThreadDispatcher()` 1 and `using (` 1; within `R4TAIL`, `}` 3; the `[Timeout(GateTimeoutMs)]` attribute and `private const int GateTimeoutMs = 60000;` are unchanged.
- [ ] [P3-T13] Replace the R4 documentation paragraph (pre-edit lines 196 to 202, unchanged by P3-T12) of QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixtureTests.cs with Delivered Source R-DOC.
  - Acceptance (recorded by P3-T15): `flake-watch` 0, `Append an observation` 0, `Issue #950:` 1, and one line each containing `The gate-free fixture method EnsureDispatcher seeds the parked dispatcher`, `cannot restore a null previous value between the pin` and `(W2), and UiThread.Initialize (W5) must not latch`.
- [ ] [P3-T14] Replace the class remarks (pre-edit lines 23 to 31) of QuickFiler.Test/Controllers/QfcInitEmailQueueZeroBatchTests.cs with Delivered Source Z-R.
  - Acceptance (recorded by P3-T15): the zero-batch file has `starts a real` 0 and one line containing `so no test starts a`.
- [ ] [P3-T15] Record the test-rewrite census in FEATURE/evidence/qa-gates/test-rewrite-census.md with `CMD-TOKEN-COUNT` on each of THREE (TOKENS `"SpinWait", ".Wait(", "WaitForState", "new BackgroundWorker()", "new SynchronousBackgroundWorker()", "WorkerStarter = StartSynchronously;", "private sealed class SynchronousBackgroundWorker : BackgroundWorker", "private sealed class DrainableSynchronizationContext : SynchronizationContext", "pump.Drain();", "SynchronizationContext.SetSynchronizationContext(previous);", "TaskCreationOptions.RunContinuationsAsynchronously", "IsBusy", "starts a real", "so no test starts a"`), `CMD-TOKEN-COUNT` on the R4 file (the P0-T12 R4 tokens plus `"The gate-free fixture method EnsureDispatcher seeds the parked dispatcher", "cannot restore a null previous value between the pin", "(W2), and UiThread.Initialize (W5) must not latch"`), `CMD-SPAN-TOKEN-COUNT` on `R4SPAN`, `R4PRE`, `R4HEAD` and `R4TAIL` (the P0-T12 token list of each) and `CMD-TIMESPAN`, plus a second `CMD-TOKEN-COUNT` on each of THREE (TOKENS `"the synchronous starter must reach the injected RemainingEmailLoader", "the synchronous starter must reach the injected loader before returning", "the finally around the awaited loader must clear the flag once it completes", "the finally must clear the flag on the throwing path too", "private static void StartSynchronously(BackgroundWorker worker)", "the injected RemainingEmailLoader must be invoked by the started worker", "bounded timeout"`).
  - Acceptance (fourteen THREE tokens in the order listed): liveness 0, 0, 0, 0, 2, 2, 1, 1, 2, 2, 1, 1, 0, 0; teardown 0, 0, 0, 0, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0; zero-batch 0, 0, 0, 0, 3, 3, 1, 0, 0, 0, 0, 0, 0, 1; reason-literal rows (seven tokens in the order listed): liveness 1, 1, 1, 1, 1, 0, 0; teardown 1, 0, 0, 0, 1, 0, 0; zero-batch 0, 0, 0, 0, 1, 1, 0; R4 file `flake-watch` 0, `Append an observation` 0, `Issue #950:` 1, `[Timeout(GateTimeoutMs)]` 8, the constant 1, the three R-DOC tokens 1 each; `R4SPAN` 1, 1, 2, 1, 1; `R4PRE` 0, 0; `R4HEAD` 1, 1; `R4TAIL` 3; `TIMESPAN-UNCLASSIFIED: 0`, with every `TIMESPAN` line `CLASSIFIED`. Any other value: correct the edit and re-run this task. (The first eleven values of each THREE row are the executor's round 1 rows; the last three columns are added by revision R1 for the stale-comment edits.)

### Phase 4 — Scoped Format, Pass-After Runs and Implementation Commit

- [ ] [P4-T1] Format the five Write Set code files with a scoped CSharpier pass and record the before-and-after hashes in FEATURE/evidence/qa-gates/scoped-format.md.
  - Command: `CMD-HASH`; then `pwsh -NoProfile -Command 'PREFIX; dotnet tool run csharpier format QuickFiler\Controllers\QfcDatamodel.cs QuickFiler.Test\Controllers\QfcDatamodelLivenessTests.cs QuickFiler.Test\Controllers\QfcDatamodelTeardownTests.cs QuickFiler.Test\Controllers\QfcInitEmailQueueZeroBatchTests.cs QuickFiler.Test\Controllers\QfcItemController.UiThreadDispatcherFixtureTests.cs; "CSHARPIER_EXIT_CODE: $LASTEXITCODE"'`; then `CMD-HASH` again.
  - Acceptance: `WORKTREE-LEAF: agent-a7805823735145ca4`; `CSHARPIER_EXIT_CODE: 0`; the line beginning `Formatted ` is transcribed and labelled as a processed-file count, not a rewrite count; `REWRITTEN:` lists every CODE5 path whose hash differs between the two captures, or `NONE` (the rewrite observation is the hash difference, not the console line). Either value completes this task: the pass exists so the committed text is formatter-stable.
- [ ] [P4-T2] Record the post-format census in FEATURE/evidence/qa-gates/post-format-census.md by re-running the P2-T4 commands and the P3-T15 commands, plus `git -C WORKTREE diff --numstat 34c2ed88cbb009f2f231453db87bc64d45a9bd51 -- QuickFiler/Controllers/QfcDatamodel.cs` and `git -C WORKTREE diff 34c2ed88cbb009f2f231453db87bc64d45a9bd51 -- QuickFiler/Controllers/QfcDatamodel.cs`, paired with `git -C WORKTREE status --porcelain -- QuickFiler QuickFiler.Test`.
  - Acceptance: every P2-T4 and P3-T15 value holds after formatting; CMD-LINECOUNT reports at most 500 for each CODE5 file (expected 495 for QfcDatamodel.cs; the others are recorded); numstat for QfcDatamodel.cs reads `14	2`, and the two deleted lines in the anchored diff both have the trimmed text `worker.RunWorkerAsync();`; the porcelain span lists exactly the five CODE5 paths with status ` M`. This artifact is the evidence for AC1, AC2, AC3 and AC14 and the census half of AC6 to AC13.
- [ ] [P4-T3] Build the fixed tree with `CMD-BUILD` (`TASKID` p4-t3) and record it in FEATURE/evidence/regression-testing/pass-after-build.md.
  - Acceptance: `WORKTREE-LEAF: agent-a7805823735145ca4`, `EXIT_CODE: 0`, `ERRORS: 0`, `TEST_DLL_ADVANCED: True`, `CSC_OUT_QUICKFILER_TEST:` at least 1.
- [ ] [P4-T4] Run the nine target tests with `CMD-VSTEST` (`ASSEMBLY-QF`, `FILTER-TARGETS`, `TASKID` p4-t4, `NAMES-TARGETS`) and record FEATURE/evidence/regression-testing/targets-pass-after.md.
  - Acceptance: `WORKTREE-LEAF: agent-a7805823735145ca4`; `EXIT_CODE: 0`; `TRX_PRESENT: True`; `SEQUENCE_FILES: 0`; `RESULT_COUNT: 9`; nine `RESULT` lines, one per target name, each `Passed`, with durations recorded. Any target not `Passed` invokes the D-13 Phase 4 restart rule.
- [ ] [P4-T5] Run the four target classes concurrently with QfcItemController_FocusAndThemeTests using `CMD-VSTEST` (`ASSEMBLY-QF`, `FILTER-CONCURRENT`, `TASKID` p4-t5, empty `NAMES`) and record FEATURE/evidence/regression-testing/concurrent-classes-pass-after.md.
  - Acceptance: `WORKTREE-LEAF: agent-a7805823735145ca4`; `TRX_PRESENT: True`; `SEQUENCE_FILES: 0`; every nine-target `RESULT` line `Passed`; `CONCURRENT-NOT-PASSED:` lists every non-Passed name or `NONE`, and every listed name is in P0-T14 `BASELINE-CONCURRENT-FAILED:` (`CONCURRENT-NEW-FAILURES: NONE`). `EXIT_CODE: 0` when the list is `NONE`; otherwise `ExpectedExitCode:` equals the observed value and the artifact names the baseline entries that account for it. A new failure of either FocusAndThemeTests theme test is `THEME TEST NULL-DISPATCHER EXPOSURE OBSERVED` (section "Risks"): stop and report; any other new failure invokes the D-13 Phase 4 restart rule.
- [ ] [P4-T6] Confirm the R4 pass-after condition from FEATURE/evidence/regression-testing/concurrent-classes-pass-after.md and FEATURE/evidence/regression-testing/targets-pass-after.md by appending a `R4-PASS-AFTER:` section to FEATURE/evidence/regression-testing/r4-fail-before.md.
  - Acceptance: the section quotes the R4 `RESULT` line from both pass-after artifacts (`Passed` in each) beside the P1-T6 `Failed` line, completing the fail-before and pass-after pair for Defect B.
- [ ] [P4-T7] Commit the implementation (the five CODE5 files and FEATURE) and record FEATURE/evidence/qa-gates/implementation-commit.md.
  - Commands, separate Bash calls: `git -C WORKTREE add -- QuickFiler/Controllers/QfcDatamodel.cs QuickFiler.Test/Controllers/QfcDatamodelLivenessTests.cs QuickFiler.Test/Controllers/QfcDatamodelTeardownTests.cs QuickFiler.Test/Controllers/QfcInitEmailQueueZeroBatchTests.cs QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixtureTests.cs docs/features/active/2026-09-30-quickfiler-tests-depend-on-wall-clock-timing-950`; `git -C WORKTREE commit -m "fix(950): start the QfcDatamodel worker through a seam and pin the R4 baseline"`; `git -C WORKTREE rev-parse HEAD`; `git -C WORKTREE diff --name-status 34c2ed88cbb009f2f231453db87bc64d45a9bd51 HEAD`; `git -C WORKTREE status --porcelain --untracked-files=all`.
  - Acceptance: both git writes exit 0; `IMPLEMENTATION-COMMIT:` records the new HEAD as an observation; the name-status diff lists the five CODE5 paths with status `M`, FEATURE paths, and at most the inherited promotion record, nothing else; no porcelain line names a path under QuickFiler/ or QuickFiler.Test/. This artifact is committed in P6-T32.

### Phase 5 — Negative Controls (temporary edits against the implementation commit)

- [ ] [P5-T1] Control edit A1 in QuickFiler.Test/Controllers/QfcDatamodelLivenessTests.cs: replace the first occurrence in file order of `model.WorkerStarter = StartSynchronously;` (test 1) with `model.WorkerStarter = _ => { };`.
  - Acceptance (recorded by P5-T9): the liveness file has `WorkerStarter = StartSynchronously;` 1 and `WorkerStarter = _ => { };` 1, the latter inside test 1.
- [ ] [P5-T2] Control edit A2 in QuickFiler.Test/Controllers/QfcDatamodelLivenessTests.cs: delete the line `release.SetResult(true);` inside `RemainingLoadActive_AfterLoaderCompletes_BecomesFalse` (test 3), so the loader is never released before `pump.Drain();`.
  - Acceptance (recorded by P5-T9): test 3 no longer contains `release.SetResult(true);` and still contains `pump.Drain();`.
- [ ] [P5-T3] Control edit A3 in QuickFiler.Test/Controllers/QfcDatamodelLivenessTests.cs: delete the line `release.SetResult(true);` inside `RemainingLoadActive_WhenLoaderThrows_IsStillClearedByFinally` (test 4).
  - Acceptance (recorded by P5-T9): the liveness file has `release.SetResult(true);` 1 (test 2 only) and `pump.Drain();` 2.
- [ ] [P5-T4] Control edit A4 in QuickFiler.Test/Controllers/QfcDatamodelTeardownTests.cs: replace `model.WorkerStarter = StartSynchronously;` with `model.WorkerStarter = _ => { };`.
  - Acceptance (recorded by P5-T9): the teardown file has `WorkerStarter = StartSynchronously;` 0 and `WorkerStarter = _ => { };` 1.
- [ ] [P5-T5] Control edit A5 in QuickFiler.Test/Controllers/QfcInitEmailQueueZeroBatchTests.cs: inside `InitEmailQueue_ZeroBatchSize_StillStartsBackgroundWorker`, replace `model.WorkerStarter = StartSynchronously;` with `model.WorkerStarter = _ => { };`.
  - Acceptance (recorded by P5-T9): `WorkerStarter = _ => { };` 1 in the zero-batch file, inside that test.
- [ ] [P5-T6] Control edit A6 in QuickFiler.Test/Controllers/QfcInitEmailQueueZeroBatchTests.cs: inside `InitEmailQueue_ZeroBatchSize_ReturnsEmptyListWithoutThrowing`, delete the line `model.WorkerStarter = StartSynchronously;`.
  - Acceptance (recorded by P5-T9): that test contains no `WorkerStarter` assignment.
- [ ] [P5-T7] Control edit A7 in QuickFiler.Test/Controllers/QfcInitEmailQueueZeroBatchTests.cs: inside `InitEmailQueue_PositiveBatchSize_RetainsExistingProjectionAndFrameDrop`, delete the line `model.WorkerStarter = StartSynchronously;`.
  - Acceptance (recorded by P5-T9): the zero-batch file has `WorkerStarter = StartSynchronously;` 0 and `WorkerStarter = _ => { };` 1.
- [ ] [P5-T8] Control edit A8 in QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixtureTests.cs: insert Delivered Source R-INJECT immediately before `transactionA.Install(liveA);` in the pinned R4 body (inside the `baseline` using block, after the `original` read, at twenty spaces).
  - Acceptance (recorded by P5-T9): within `R4SPAN`, `EnsureUiThreadDispatcher()` 2 and `IDisposable baseline = QfcItemControllerTestSupport.EnsureUiThreadDispatcher()` 1.
- [ ] [P5-T9] Record the batch A edit census in FEATURE/evidence/regression-testing/controls-a-edits.md with `CMD-TOKEN-COUNT` on THREE (TOKENS `"WorkerStarter = StartSynchronously;", "WorkerStarter = _ => { };", "release.SetResult(true);", "pump.Drain();"`), `CMD-SPAN-TOKEN-COUNT` on `R4SPAN` (TOKENS `"EnsureUiThreadDispatcher()", "IDisposable baseline = QfcItemControllerTestSupport.EnsureUiThreadDispatcher()"`), and `git -C WORKTREE diff --numstat HEAD -- QuickFiler.Test` paired with `git -C WORKTREE status --porcelain -- QuickFiler QuickFiler.Test`.
  - Acceptance: liveness 1, 1, 1, 2; teardown 0, 1, 0, 0; zero-batch 0, 1, 0, 0; `R4SPAN` 2 and 1; numstat lists exactly the four test files (liveness `1	3`, teardown `1	1`, zero-batch `1	3`, R4 `1	0`); porcelain lists exactly those four paths with ` M`. The artifact names each edit A1 to A8 with its test.
- [ ] [P5-T10] Build the batch A tree with `CMD-BUILD` (`TASKID` p5-t10) and record FEATURE/evidence/regression-testing/controls-a-build.md.
  - Acceptance: `WORKTREE-LEAF: agent-a7805823735145ca4`, `EXIT_CODE: 0`, `ERRORS: 0`, `TEST_DLL_ADVANCED: True`.
- [ ] [P5-T11] [expect-fail] Run batch A with `CMD-VSTEST` (`ASSEMBLY-QF`, `FILTER-CONTROLS-A`, `TASKID` p5-t11, `NAMES-TARGETS`) and record FEATURE/evidence/regression-testing/negative-controls-batch-a.md.
  - Acceptance: `WORKTREE-LEAF: agent-a7805823735145ca4`; `EXIT_CODE: 1`; `ExpectedExitCode: 1`; `TRX_PRESENT: True`; `SEQUENCE_FILES: 0`; `RESULT_COUNT: 8`; and per test, with duration recorded: test 1 `Failed` with a message containing `the synchronous starter must reach the injected RemainingEmailLoader`; test 3 `Failed` with `the finally around the awaited loader must clear the flag once it completes`; test 4 `Failed` with `the finally must clear the flag on the throwing path too`; teardown `Failed` with `the synchronous starter must reach the injected RemainingEmailLoader`; Z1 `Failed` with `the injected RemainingEmailLoader must be invoked by the started worker`; Z0 `Failed` with `NullReferenceException`; Z2 `Failed` with `NullReferenceException`; R4 `Passed` (the pin neutralises the injected gate-free writer). Any `Timeout`, `Aborted` or `NotExecuted` outcome, any Sequence file, or any control that passes is `CONTROL DEFECT`: stop and report, because a control that does not fail immediately is a defect in the rewrite (spec Test Strategy).
- [ ] [P5-T12] Revert batch A to HEAD and verify byte-for-byte, recording FEATURE/evidence/regression-testing/controls-a-revert.md.
  - Commands, separate Bash calls: `git -C WORKTREE restore --source=HEAD --worktree -- QuickFiler.Test/Controllers/QfcDatamodelLivenessTests.cs QuickFiler.Test/Controllers/QfcDatamodelTeardownTests.cs QuickFiler.Test/Controllers/QfcInitEmailQueueZeroBatchTests.cs QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixtureTests.cs`; `git -C WORKTREE diff --exit-code HEAD -- QuickFiler QuickFiler.Test`; `git -C WORKTREE status --porcelain -- QuickFiler QuickFiler.Test`.
  - Acceptance: the restore exits 0; the anchored diff exits 0 and prints nothing; the porcelain span prints nothing.
- [ ] [P5-T13] Control edit B1 in QuickFiler.Test/Controllers/QfcDatamodelLivenessTests.cs: inside `StartHeldOpenLoader`, replace `model.WorkerStarter = StartSynchronously;` with `model.WorkerStarter = _ => { };`.
  - Acceptance (recorded by P5-T14): the liveness file has `WorkerStarter = StartSynchronously;` 1 (test 1) and `WorkerStarter = _ => { };` 1 (inside `StartHeldOpenLoader`).
- [ ] [P5-T14] Record the batch B edit census in FEATURE/evidence/regression-testing/controls-b-edits.md with `CMD-TOKEN-COUNT` on the liveness file (TOKENS `"WorkerStarter = StartSynchronously;", "WorkerStarter = _ => { };"`) and `git -C WORKTREE diff --numstat HEAD -- QuickFiler.Test` paired with `git -C WORKTREE status --porcelain -- QuickFiler QuickFiler.Test`.
  - Acceptance: 1 and 1; numstat lists only the liveness file with `1	1`; porcelain lists only that path with ` M`.
- [ ] [P5-T15] Build the batch B tree with `CMD-BUILD` (`TASKID` p5-t15) and record FEATURE/evidence/regression-testing/controls-b-build.md.
  - Acceptance: `WORKTREE-LEAF: agent-a7805823735145ca4`, `EXIT_CODE: 0`, `ERRORS: 0`, `TEST_DLL_ADVANCED: True`.
- [ ] [P5-T16] [expect-fail] Run batch B with `CMD-VSTEST` (`ASSEMBLY-QF`, `FILTER-CONTROLS-B`, `TASKID` p5-t16, `NAMES` `"RemainingLoadActive_AcrossAsyncVoidFirstAwait_StaysTrueWhileLoaderProduces"`) and record FEATURE/evidence/regression-testing/negative-controls-batch-b.md.
  - Acceptance: `WORKTREE-LEAF: agent-a7805823735145ca4`; `EXIT_CODE: 1`; `ExpectedExitCode: 1`; `TRX_PRESENT: True`; `SEQUENCE_FILES: 0`; `RESULT_COUNT: 1`; the `RESULT` line `Failed` with duration recorded; the `MESSAGE` line contains `the synchronous starter must reach the injected loader before returning`. Any other outcome is `CONTROL DEFECT`: stop.
- [ ] [P5-T17] Revert batch B to HEAD and verify byte-for-byte, recording FEATURE/evidence/regression-testing/controls-b-revert.md.
  - Commands, separate Bash calls: `git -C WORKTREE restore --source=HEAD --worktree -- QuickFiler.Test/Controllers/QfcDatamodelLivenessTests.cs`; `git -C WORKTREE diff --exit-code HEAD -- QuickFiler QuickFiler.Test`; `git -C WORKTREE status --porcelain -- QuickFiler QuickFiler.Test`.
  - Acceptance: the restore exits 0; the anchored diff exits 0 and prints nothing; the porcelain span prints nothing.
- [ ] [P5-T18] Control edit C1 in QuickFiler.Test/Controllers/QfcDatamodelLivenessTests.cs: delete the line `pump.Drain();` inside `RemainingLoadActive_AfterLoaderCompletes_BecomesFalse` (test 3), keeping `release.SetResult(true);`, so the released loader's continuation stays queued in the installed context.
  - Acceptance (recorded by P5-T20): test 3 still contains `release.SetResult(true);` and no longer contains `pump.Drain();`.
- [ ] [P5-T19] Control edit C2 in QuickFiler.Test/Controllers/QfcDatamodelLivenessTests.cs: delete the line `pump.Drain();` inside `RemainingLoadActive_WhenLoaderThrows_IsStillClearedByFinally` (test 4), keeping `release.SetResult(true);`.
  - Acceptance (recorded by P5-T20): the liveness file has `pump.Drain();` 0 and `release.SetResult(true);` 3.
- [ ] [P5-T20] Record the batch C edit census in FEATURE/evidence/regression-testing/controls-c-edits.md with `CMD-TOKEN-COUNT` on the liveness file (TOKENS `"pump.Drain();", "release.SetResult(true);", "TaskCreationOptions.RunContinuationsAsynchronously"`) and `git -C WORKTREE diff --numstat HEAD -- QuickFiler.Test` paired with `git -C WORKTREE status --porcelain -- QuickFiler QuickFiler.Test`.
  - Acceptance: 0, 3 and 1; numstat lists only the liveness file with `0	2`; porcelain lists only that path with ` M`. The artifact names C1 and C2 with their tests.
- [ ] [P5-T21] Build the batch C tree with `CMD-BUILD` (`TASKID` p5-t21) and record FEATURE/evidence/regression-testing/controls-c-build.md.
  - Acceptance: `WORKTREE-LEAF: agent-a7805823735145ca4`, `EXIT_CODE: 0`, `ERRORS: 0`, `TEST_DLL_ADVANCED: True`.
- [ ] [P5-T22] [expect-fail] Run batch C with `CMD-VSTEST` (`ASSEMBLY-QF`, `FILTER-CONTROLS-C`, `TASKID` p5-t22, `NAMES` `"RemainingLoadActive_AfterLoaderCompletes_BecomesFalse", "RemainingLoadActive_WhenLoaderThrows_IsStillClearedByFinally"`) and record FEATURE/evidence/regression-testing/negative-controls-batch-c.md.
  - Acceptance: `WORKTREE-LEAF: agent-a7805823735145ca4`; `EXIT_CODE: 1`; `ExpectedExitCode: 1`; `TRX_PRESENT: True`; `SEQUENCE_FILES: 0`; `RESULT_COUNT: 2`; with duration recorded, test 3 `Failed` with a message containing `the finally around the awaited loader must clear the flag once it completes` and test 4 `Failed` with a message containing `the finally must clear the flag on the throwing path too`. This shows that, with the loader released, the flag clears only through `Drain()`: the continuation is posted to the installed context rather than run inline inside `SetResult` (D-2). Either test `Passed` is `DRAIN NOT LOAD-BEARING`, and any `Timeout`, `Aborted` or `NotExecuted` outcome or any Sequence file is `CONTROL DEFECT`: stop and report in both cases.
- [ ] [P5-T23] Revert batch C to HEAD and verify byte-for-byte, recording FEATURE/evidence/regression-testing/controls-c-revert.md.
  - Commands, separate Bash calls: `git -C WORKTREE restore --source=HEAD --worktree -- QuickFiler.Test/Controllers/QfcDatamodelLivenessTests.cs`; `git -C WORKTREE diff --exit-code HEAD -- QuickFiler QuickFiler.Test`; `git -C WORKTREE status --porcelain -- QuickFiler QuickFiler.Test`.
  - Acceptance: the restore exits 0; the anchored diff exits 0 and prints nothing; the porcelain span prints nothing.
- [ ] [P5-T24] Append a `## Post-fix deterministic controls` section to the FEATURE/evidence/regression-testing/fail-before-exception.*.md dossier citing FEATURE/evidence/regression-testing/negative-controls-batch-a.md, FEATURE/evidence/regression-testing/negative-controls-batch-b.md and FEATURE/evidence/regression-testing/negative-controls-batch-c.md.
  - Acceptance: the section names the ten Defect A controls (A1 to A7 from batch A, B1 from batch B, C1 and C2 from batch C) with their outcome `Failed` and duration copied from the three batch artifacts, and states that each rewritten test fails at once when its signal is withheld, which the pre-fix tests could show only by waiting out a five-second bound.
- [ ] [P5-T25] Write the AC15 negative-control summary FEATURE/evidence/other/negative-controls-summary.md.
  - Acceptance: a Markdown table with exactly nine rows, one per test named in AC6 to AC13 in that order, with columns Test, Mechanism (the spec Test Strategy row), Edit applied (A1 to A8, B1, or R-INJECT on the unmodified file), Observed outcome, Failure message fragment, Duration and Source artifact. The R4 row cites P1-T6 (`Failed`, pre-fix shape with the injected writer) and P5-T11 (`Passed`, pinned shape with the same injected writer), matching the spec's two-part R4 mechanism. A second table headed `Drain-dependency controls` has exactly two rows, tests 3 and 4, with the same columns, Edit applied C1 and C2, and values copied from P5-T22; it supplements, and does not replace, the Test Strategy mechanism in the first table. Every row's values are copied from the cited artifacts; the artifact carries `Timestamp:`.

### Phase 6 — Final QA Loop, Static Gates, Check-offs and Final Commit

- [ ] [P6-T1] Run the CLAUDE.md formatting step `dotnet tool run csharpier format .` at the worktree root with a tree observation before and after, and record FEATURE/evidence/qa-gates/csharpier-format.md.
  - Commands: `git -C WORKTREE status --porcelain --untracked-files=all`; `CMD-HASH`; `pwsh -NoProfile -Command 'PREFIX; dotnet tool run csharpier format .; "CSHARPIER_EXIT_CODE: $LASTEXITCODE"'`; `CMD-HASH`; `git -C WORKTREE status --porcelain --untracked-files=all`.
  - Acceptance: `ITERATION:` recorded; `WORKTREE-LEAF: agent-a7805823735145ca4`; `CSHARPIER_EXIT_CODE: 0`; the `Formatted ` line transcribed and labelled as a processed-file count; `REWRITTEN-WRITESET:` lists every CODE5 path whose hash changed, or `NONE`; `REWRITTEN-OTHER:` lists every porcelain path present after and absent before, or `NONE`. Clean pass: both `NONE`. A non-empty `REWRITTEN-WRITESET:` with `REWRITTEN-OTHER: NONE` invokes the D-13 format restart; a non-empty `REWRITTEN-OTHER:` is `FORMAT TOUCHED OUT-OF-SCOPE FILE`: stop.
- [ ] [P6-T2] Run the read-only formatter gate `dotnet tool run csharpier check .` and record FEATURE/evidence/qa-gates/csharpier-check-final.md.
  - Command: `pwsh -NoProfile -Command 'PREFIX; dotnet tool run csharpier check .; "CSHARPIER_EXIT_CODE: $LASTEXITCODE"'`.
  - Acceptance: `ITERATION:` recorded; `WORKTREE-LEAF: agent-a7805823735145ca4`; `EXIT_CODE: 0`; the success-case line beginning `Checked ` and ending `ms.` transcribed verbatim.
- [ ] [P6-T3] Run the analyzer rebuild with `CMD-REBUILD` (analyzer GATEARGS, `TASKID` p6-t3) and record FEATURE/evidence/qa-gates/msbuild-analyzer-final.md.
  - Acceptance: `ITERATION:` recorded; `WORKTREE-LEAF: agent-a7805823735145ca4`; `EXIT_CODE: 0`; `ERRORS: 0`; `SKIP_CORECOMPILE_LINES: 0`; both `CSC_OUT_` counts at least 1; `WRITESET_DIAGNOSTIC_LINES:` not greater than `ANALYZER-BASELINE-WRITESET-LINES:` and every code in `WRITESET_DIAGNOSTIC_CODES:` present in `ANALYZER-BASELINE-WRITESET-CODES:` (no new analyzer diagnostic in a Write Set file); `WARNINGS:` recorded beside `ANALYZER-BASELINE-WARNINGS:`.
- [ ] [P6-T4] Run the type-check rebuild with `CMD-REBUILD` (GATEARGS `/p:TreatWarningsAsErrors=true`, no Nullable override, `TASKID` p6-t4) and record FEATURE/evidence/qa-gates/msbuild-nullable-final.md.
  - Acceptance: `ITERATION:` recorded; `WORKTREE-LEAF: agent-a7805823735145ca4`; `EXIT_CODE: 0`; `ERRORS: 0`; `SKIP_CORECOMPILE_LINES: 0`; both `CSC_OUT_` counts at least 1; both `_DLL_EXISTS:` values `True`.
- [ ] [P6-T5] Run the final repository-wide test-and-coverage run by the P0-T15 route (`CMD-COVERAGE-RUNNER` or `CMD-COVERAGE-DIRECT` with `STAGE` final, then `CMD-COVERAGE-POST` with `STAGE` final) and record FEATURE/evidence/qa-gates/coverage-post-change.md.
  - Artifact: the same fields as P0-T16, with `ITERATION:` and `FAILED-SET:` recorded as `FINAL-FAILED-SET:`.
  - Acceptance, all required: the route equals P0-T15's; `TRX_PRESENT: True`; `SEQUENCE_FILES:` 0 under DIRECT; `QFCDATAMODEL-CLASS-NODES: 0`; the nine `RESULT` lines all `Passed`; `NEW-FAILURES:` (names in `FINAL-FAILED-SET:` absent from `BASELINE-FAILED-SET:`) is `NONE`; `FIGURES-COMPARED:` restates the `Total`, `executed`, `error`, `timeout`, `aborted` and `notExecuted` figures from the P0-T16 summary block and from this run's summary block, and this run holds `Total` equal to the P0-T16 value (this plan adds and removes no test method), `executed` not less than the P0-T16 value, and each of `error`, `timeout`, `aborted` and `notExecuted` not greater than its P0-T16 value, because the failed set lists only results whose outcome is `Failed` and cannot show an aborted or unexecuted test; `LINE-FLOOR:` and `BRANCH-FLOOR:` each `MET`, or `NOT MET` only where P0-T16 recorded the same floor as not met; the `First-party coverage:` line is recorded as the numeric post-change headline. `RUNNER-GREEN: YES` is recorded when the route is RUNNER and `RUNNER_EXIT_CODE: 0`, otherwise `RUNNER-GREEN: NO` with the reason (`COVERAGE-ROUTE DIRECT` or `PRE-EXISTING FAILURES`). A failure of any target test or a new failure attributable to the Write Set invokes the D-13 failure restart; a new failure of either FocusAndThemeTests theme test is `THEME TEST NULL-DISPATCHER EXPOSURE OBSERVED` (section "Risks"); any other new failure, and any `FIGURES-COMPARED:` breach, is `NEW FAILURE OUTSIDE SCOPE`; both are a stop and report, without re-running.
- [ ] [P6-T6] Compare baseline and post-change coverage and test outcomes and record FEATURE/evidence/qa-gates/coverage-comparison.md from FEATURE/evidence/baseline/coverage-baseline.md and FEATURE/evidence/qa-gates/coverage-post-change.md.
  - Acceptance: the artifact records the baseline and post-change `First-party coverage:` lines (lines and branches, numeric), the two `ROOT` lines, and the repository-wide comparison in exactly one named branch: `BRANCH A` when the two `lines-valid` figures differ by at most 1 percent of the baseline figure (the post-change line rate must not be lower than baseline by more than 0.5 percentage points), otherwise `BRANCH B` (recorded and not gated, with one sentence stating the denominators are not comparable). New and changed code: `CHANGED-PRODUCTION-COVERAGE: NOT MEASURED` with the reason `[ExcludeFromCodeCoverage]` at QuickFiler/Controllers/QfcDatamodel.cs line 25 and `QFCDATAMODEL-CLASS-NODES: 0` at both stages; the changed test files are outside the instrumented set. The artifact also restates `BASELINE-FAILED-SET:`, `FINAL-FAILED-SET:` and `NEW-FAILURES: NONE`. A Branch A breach is `COVERAGE REGRESSION`: stop.
- [ ] [P6-T7] Record the final toolchain pass in FEATURE/evidence/qa-gates/toolchain-final-pass.md from the P6-T1 to P6-T5 artifacts of the final iteration.
  - Acceptance: one row per step, in order — csharpier format (`REWRITTEN-WRITESET: NONE`, `REWRITTEN-OTHER: NONE`), csharpier check (exit 0), analyzer rebuild (exit 0), TreatWarningsAsErrors rebuild (exit 0), coverage run (route, exit code, `RUNNER-GREEN:`) — each with its exact command, `EXIT_CODE:` and the same `ITERATION:` value; `SINGLE-PASS: YES` when all five rows come from one iteration with no restart after P6-T1; `AC17-STATUS: MET` only when `SINGLE-PASS: YES` and `RUNNER-GREEN: YES`, otherwise `AC17-STATUS: NOT MET` with the reason.
- [ ] [P6-T8] Record the AC4 wall-clock census in FEATURE/evidence/qa-gates/wall-clock-tokens.md with `CMD-TOKEN-COUNT` on each of THREE (TOKENS `"SpinWait", ".Wait(", "WaitForState", "Task.Wait("`) and `CMD-TIMESPAN`.
  - Acceptance: every count 0 in all three files; `TIMESPAN-UNCLASSIFIED: 0` with every `TIMESPAN` line `CLASSIFIED` (the surviving lines are the teardown file's two `QuiesceLoaderAsync(` production arguments and one `fake.Advance(` and the liveness file's three `fake.Advance(` lines); the artifact cites the non-zero P0-T12 counts as the positive control.
- [ ] [P6-T9] Record the AC5 prohibited-construct gate in FEATURE/evidence/qa-gates/prohibited-constructs.md with `CMD-ADDED-SCAN`, `git -C WORKTREE diff --exit-code 34c2ed88cbb009f2f231453db87bc64d45a9bd51 HEAD -- scripts/vscode/TaskMaster.cli.runsettings TaskMaster.runsettings`, `git -C WORKTREE status --porcelain -- scripts/vscode/TaskMaster.cli.runsettings TaskMaster.runsettings`, and `CMD-TOKEN-COUNT` on the R4 file (TOKENS `"[Timeout(GateTimeoutMs)]", "private const int GateTimeoutMs = 60000;"`).
  - Acceptance: `WORKTREE-LEAF: agent-a7805823735145ca4`; `GIT_DIFF_EXIT_CODE: 0`; `ADDED_LINES:` greater than 0; `ADDED-TOKEN` counts 0 for `Thread.Sleep`, `Task.Delay`, `DoNotParallelize`, `Retry(` and `Timeout(`, and at least 1 for `WorkerStarter` (positive control); the runsettings diff exits 0 and the porcelain span prints nothing; the R4 file reads 8 and 1, equal to P0-T12.
- [ ] [P6-T10] Record the footprint gate in FEATURE/evidence/qa-gates/footprint-scope.md with `git -C WORKTREE diff --name-status 34c2ed88cbb009f2f231453db87bc64d45a9bd51 HEAD` paired with `git -C WORKTREE status --porcelain --untracked-files=all`.
  - Acceptance: `THIS-ITEM-FOOTPRINT:` (name-status paths outside FEATURE, excluding any path P0-T3 listed in `INHERITED-COMMITTED:`) is exactly the five CODE5 paths with status `M`; the excluded inherited paths are recorded as `INHERITED-AND-EXCLUDED:` (at most `docs/features/potential/promoted/2026-09-30-quickfiler-tests-depend-on-wall-clock-timing.md`, status `A`); no path ends in `.csproj`; no status `A` outside FEATURE other than an `INHERITED-AND-EXCLUDED:` path; no porcelain line names a path under QuickFiler/, QuickFiler.Test/ or scripts/.
- [ ] [P6-T11] Record the evidence hygiene gate in FEATURE/evidence/qa-gates/evidence-hygiene.md by scanning every file under FEATURE/evidence/.
  - Command: `pwsh -NoProfile -Command 'PREFIX; $b = [char]92; $files = @(Get-ChildItem -LiteralPath "docs\features\active\2026-09-30-quickfiler-tests-depend-on-wall-clock-timing-950\evidence" -Recurse -File); "EVIDENCE_FILES=$($files.Count)"; "RAW_DOCUMENTS=$(@($files | Where-Object { $_.Extension -in @(".trx", ".xml", ".coverage", ".coveragexml", ".log") }).Count)"; $pattern = "[a-z]:[" + $b + $b + "/]+users[" + $b + $b + "/]+[a-z0-9_.~-]"; "PROFILE_PATH_LINES=$(@($files | Select-String -Pattern $pattern).Count)"'`.
  - Acceptance: `WORKTREE-LEAF: agent-a7805823735145ca4`; `EVIDENCE_FILES=` at least 1; `RAW_DOCUMENTS=0`; `PROFILE_PATH_LINES=0` (the pattern is the repository hygiene rule's, built from `[char]92` so the Bash channel cannot collapse its backslashes). A non-zero value names the offending files; the executor redacts them and re-runs this task.
- [ ] [P6-T12] Check off AC1 in FEATURE/spec.md when FEATURE/evidence/qa-gates/post-format-census.md shows the property line 1, the constructor default 2 and both documentation tokens 1.
  - Acceptance: only `- [ ] AC1:` changes to `- [x] AC1:`; otherwise the box stays unchecked and `AC1: NOT MET` with the reason is recorded for P6-T29.
- [ ] [P6-T13] Check off AC2 in FEATURE/spec.md when FEATURE/evidence/qa-gates/post-format-census.md shows `INITQ` `RunWorkerAsync` 0 and `WorkerStarter(worker);` 2.
  - Acceptance: only `- [ ] AC2:` changes; otherwise unchecked with `AC2: NOT MET`.
- [ ] [P6-T14] Check off AC3 in FEATURE/spec.md when FEATURE/evidence/qa-gates/post-format-census.md shows QfcDatamodel.cs LINES at most 500.
  - Acceptance: only `- [ ] AC3:` changes; otherwise unchecked with `AC3: NOT MET`.
- [ ] [P6-T15] Check off AC4 in FEATURE/spec.md when FEATURE/evidence/qa-gates/wall-clock-tokens.md holds every P6-T8 condition.
  - Acceptance: only `- [ ] AC4:` changes; otherwise unchecked with `AC4: NOT MET`.
- [ ] [P6-T16] Check off AC5 in FEATURE/spec.md when FEATURE/evidence/qa-gates/prohibited-constructs.md holds every P6-T9 condition.
  - Acceptance: only `- [ ] AC5:` changes; otherwise unchecked with `AC5: NOT MET`.
- [ ] [P6-T17] Check off AC6 in FEATURE/spec.md when `DequeueNextItemGroupAsync_WhileLoaderStillProducing_KeepsPollingAfterWorkerIdle` is `Passed` in FEATURE/evidence/regression-testing/targets-pass-after.md and FEATURE/evidence/qa-gates/coverage-post-change.md, and post-format-census.md shows the liveness `WorkerStarter = StartSynchronously;` count 2.
  - Acceptance: only `- [ ] AC6:` changes; otherwise unchecked with `AC6: NOT MET`.
- [ ] [P6-T18] Check off AC7 in FEATURE/spec.md when `RemainingLoadActive_AcrossAsyncVoidFirstAwait_StaysTrueWhileLoaderProduces` is `Passed` in FEATURE/evidence/regression-testing/targets-pass-after.md and FEATURE/evidence/qa-gates/coverage-post-change.md.
  - Acceptance: only `- [ ] AC7:` changes; otherwise unchecked with `AC7: NOT MET`.
- [ ] [P6-T19] Check off AC8 in FEATURE/spec.md when `RemainingLoadActive_AfterLoaderCompletes_BecomesFalse` is `Passed` in FEATURE/evidence/regression-testing/targets-pass-after.md and FEATURE/evidence/qa-gates/coverage-post-change.md, and post-format-census.md shows `pump.Drain();` 2 in the liveness file.
  - Acceptance: only `- [ ] AC8:` changes; otherwise unchecked with `AC8: NOT MET`.
- [ ] [P6-T20] Check off AC9 in FEATURE/spec.md when `RemainingLoadActive_WhenLoaderThrows_IsStillClearedByFinally` is `Passed` in FEATURE/evidence/regression-testing/targets-pass-after.md and FEATURE/evidence/qa-gates/coverage-post-change.md.
  - Acceptance: only `- [ ] AC9:` changes; otherwise unchecked with `AC9: NOT MET`.
- [ ] [P6-T21] Check off AC10 in FEATURE/spec.md when `Worker_DoWork_CapturesRemainingLoadTask` is `Passed` in FEATURE/evidence/regression-testing/targets-pass-after.md and FEATURE/evidence/qa-gates/coverage-post-change.md, and post-format-census.md shows the teardown `WaitForState` 0 and `.Wait(` 0.
  - Acceptance: only `- [ ] AC10:` changes; otherwise unchecked with `AC10: NOT MET`.
- [ ] [P6-T22] Check off AC11 in FEATURE/spec.md when `InitEmailQueue_ZeroBatchSize_StillStartsBackgroundWorker` is `Passed` in FEATURE/evidence/regression-testing/targets-pass-after.md and FEATURE/evidence/qa-gates/coverage-post-change.md, and post-format-census.md shows the zero-batch `.Wait(` 0.
  - Acceptance: only `- [ ] AC11:` changes; otherwise unchecked with `AC11: NOT MET`.
- [ ] [P6-T23] Check off AC12 in FEATURE/spec.md when `InitEmailQueue_ZeroBatchSize_ReturnsEmptyListWithoutThrowing` and `InitEmailQueue_PositiveBatchSize_RetainsExistingProjectionAndFrameDrop` are both `Passed` in FEATURE/evidence/regression-testing/targets-pass-after.md and FEATURE/evidence/qa-gates/coverage-post-change.md, and post-format-census.md shows the zero-batch `WorkerStarter = StartSynchronously;` 3.
  - Acceptance: only `- [ ] AC12:` changes; otherwise unchecked with `AC12: NOT MET`.
- [ ] [P6-T24] Check off AC13 in FEATURE/spec.md when `Transaction_SecondCallerCannotInstallUntilTheFirstRestores` is `Passed` in FEATURE/evidence/regression-testing/targets-pass-after.md, FEATURE/evidence/regression-testing/concurrent-classes-pass-after.md and FEATURE/evidence/qa-gates/coverage-post-change.md, and post-format-census.md shows `R4SPAN` 1, 1, 2, 1, 1, `R4PRE` 0, 0, `R4HEAD` 1, 1 and `R4TAIL` 3 (the pin opens after transaction A's acquisition and before the `original` read, closes after both assertions, and both assertions remain).
  - Acceptance: only `- [ ] AC13:` changes; otherwise unchecked with `AC13: NOT MET`.
- [ ] [P6-T25] Check off AC14 in FEATURE/spec.md when FEATURE/evidence/qa-gates/post-format-census.md shows `flake-watch` 0, `Append an observation` 0, `Issue #950:` 1 and the three R-DOC tokens 1 each.
  - Acceptance: only `- [ ] AC14:` changes; otherwise unchecked with `AC14: NOT MET`.
- [ ] [P6-T26] Check off AC15 in FEATURE/spec.md when FEATURE/evidence/other/negative-controls-summary.md holds nine rows in its AC15 table and two rows in its Drain-dependency table whose outcomes match P5-T25's acceptance.
  - Acceptance: only `- [ ] AC15:` changes; otherwise unchecked with `AC15: NOT MET`.
- [ ] [P6-T27] Check off AC16 in FEATURE/spec.md when FEATURE/evidence/other/ambient-synchronization-context.md carries exactly one `AMBIENT-SYNCHRONIZATION-CONTEXT:` line.
  - Acceptance: only `- [ ] AC16:` changes; otherwise unchecked with `AC16: NOT MET`.
- [ ] [P6-T28] Check off AC17 in FEATURE/spec.md when FEATURE/evidence/qa-gates/toolchain-final-pass.md reads `AC17-STATUS: MET`.
  - Acceptance: only `- [ ] AC17:` changes; otherwise the box stays unchecked and the recorded reason (`COVERAGE-ROUTE DIRECT`, `PRE-EXISTING FAILURES` or a restart after P6-T1) is carried to P6-T29 as `AC17: NOT MET`.
- [ ] [P6-T29] Write the acceptance-criteria status summary FEATURE/evidence/other/ac-status-summary.md.
  - Acceptance: the artifact carries `Timestamp:` and the acceptance-criteria-tracking status block (Source: the spec path; Total AC items: 17; Checked off; Remaining; Items remaining with each `ACn: NOT MET` reason), with the counts read from FEATURE/spec.md after P6-T28 (lines beginning `- [x] AC` and `- [ ] AC`, summing to 17).
- [ ] [P6-T30] Re-run the P6-T11 hygiene command over FEATURE/evidence/ after P6-T29 and append the result to FEATURE/evidence/qa-gates/evidence-hygiene.md as a `## Re-run after check-offs` section.
  - Acceptance: `RAW_DOCUMENTS=0` and `PROFILE_PATH_LINES=0` for the evidence tree as it will be committed.
- [ ] [P6-T31] Verify that FEATURE/spec.md changed only in its checkbox lines by recording `git -C WORKTREE diff --numstat HEAD -- docs/features/active/2026-09-30-quickfiler-tests-depend-on-wall-clock-timing-950/spec.md` and `git -C WORKTREE diff HEAD -- docs/features/active/2026-09-30-quickfiler-tests-depend-on-wall-clock-timing-950/spec.md`, paired with `git -C WORKTREE status --porcelain -- docs/features/active/2026-09-30-quickfiler-tests-depend-on-wall-clock-timing-950/spec.md`, in a `## Spec check-off diff` section appended to FEATURE/evidence/other/ac-status-summary.md.
  - Acceptance: added and deleted line counts are equal and equal the checked-off count; every deleted line begins `- [ ] AC` and every added line begins `- [x] AC` with identical remaining text.
- [ ] [P6-T32] Commit the remaining feature evidence and check-offs (FEATURE only) and record FEATURE/evidence/qa-gates/final-commit.md.
  - Commands, separate Bash calls: `git -C WORKTREE add -- docs/features/active/2026-09-30-quickfiler-tests-depend-on-wall-clock-timing-950`; `git -C WORKTREE commit -m "docs(950): record final QA evidence and acceptance check-offs"`; `git -C WORKTREE rev-parse HEAD`; `git -C WORKTREE diff --name-status 34c2ed88cbb009f2f231453db87bc64d45a9bd51 HEAD`; `git -C WORKTREE status --porcelain --untracked-files=all`.
  - Acceptance: both git writes exit 0; `FINAL-COMMIT:` records HEAD as an observation; the name-status paths outside FEATURE are exactly the P6-T10 footprint; the porcelain output names no path under FEATURE other than this plan file (whose final check-off mark follows the commit) and FEATURE/evidence/qa-gates/final-commit.md (written after the commit), and no path under QuickFiler/, QuickFiler.Test/ or scripts/. The final-commit artifact and this plan's check-off marks are committed by the orchestrator with the plan file.

## Planner self-review and internal review record

SELF-REVIEW: RE-DERIVED THIS PASS

Revision R1 pass (this pass): every citation the R1 edits touched, and the sibling lines around them, was re-read directly from the item worktree files (HEAD `e3827fb2c` as stated by the delegation; this planning session has no shell, so HEAD itself was not re-queried and P0-T3 records it):

- R1-a. QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixtureTests.cs — lines 185 to 279 read in full: para 196 to 202; attributes 204 and 205; declaration 206; 207 to 214 (acquisition 212 to 214); `original` read 215; `Install` 216; blank 217, 221, 237, 242; `using (` 218 (the only one in 206 to 272); transaction B 225 to 227 and read 230; assertions 244 to 257; `issue #230 lost update` at 256 (unique in the file); braces 258 and 259; `finally` 260 to 263; method close 264; R5 at 273. File-wide search: `.BeginTransactionAsync()` at 51, 111, 161, 213, 226 and later; `Dispatcher original = ` at 55, 115, 215; `EnsureUiThreadDispatcher` at 60, 119, 166; `ShutdownDispatcher(liveA)` at 96, 262, 310, 351; no `Issue #950`, no `W3`/`W4`; 11 lines carry `GateTimeoutMs`, `flake-watch` or `Append an observation` (9, 1 and 1). Test names: R1 44, R2 107, R3 157, R4 206, R5 273. Derived: `R4SPAN` 206-272, `R4PRE` 206-212, `R4HEAD` 206-214, `R4TAIL` 256-261 (two `}` lines).
- R1-b. QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixture.cs — `EnsureDispatcher` 116 to 138 (seeds only a null field, line 130; never takes the gate); design note 25 to 30. QuickFiler.Test/Controllers/QfcItemController.TestSupport.cs 238 to 239 (`EnsureUiThreadDispatcher` returns `IDisposable`).
- R1-c. QuickFiler.Test/Controllers/QfcDatamodelLivenessTests.cs — read in full (255 lines): L1 region 47 to 57; test 1 70 to 138 (IsBusy 73, 107, 108); `StartHeldOpenLoader` 148 to 181 (IsBusy 151, 177); blank 182; test 2 summary 183 to 187 (IsBusy 185), `[TestMethod]` 188; test 3 207 to 227; test 4 229 to 253. Post-edit census rows re-derived from the final prescribed source text (L1 to L5 including the extended L3): 0, 0, 0, 0, 2, 2, 1, 1, 2, 2, 1, 1, 0, 0; batch C census 0, 3, 1.
- R1-d. QuickFiler.Test/Controllers/QfcInitEmailQueueZeroBatchTests.cs — read in full (212 lines): remarks 23 to 31 (`starts a real` at 24 only); inert loader 89 to 108; Z0 110 to 133; Z1 135 to 164 (IsBusy 139, 141); Z2 166 to 210. Post-edit row 0, 0, 0, 0, 3, 3, 1, 0, 0, 0, 0, 0, 0, 1.
- R1-e. QuickFiler.Test/Controllers/QfcDatamodelTeardownTests.cs — token search for every census token: `WaitForState` 66 and 225, `SpinWait` 67, `new BackgroundWorker()` 214, `.Wait(` 220; no `IsBusy`, `starts a real`, `RunContinuationsAsynchronously`, `SetSynchronizationContext` or `Drain`. Post-edit row 0, 0, 0, 0, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0.
- R1-f. docs/features/active/2026-09-30-quickfiler-tests-depend-on-wall-clock-timing-950/spec.md — lines 60 to 284 read: scope item 72 (pin inside the gate, W3/W4 rationale), design summary 131, invariants 139, Test Strategy table 236 to 246, acceptance lines 266 to 282 (AC13 at 278).
- R1-g. This plan — the Command Reference payloads `CMD-COVERAGE-RUNNER` and `CMD-COVERAGE-DIRECT` each end with `Write-Output "PAYLOAD-COMPLETE"`; no remaining reference to `STAGE-950.result.log`; every former `P3-T14`, `P5-T18` and `P5-T19` reference renumbered.

Original authoring pass (HEAD 8bbd48f68f9631247ccc3c58fa232ff333d4fe56; items not touched by R1 are carried from it):

1. QuickFiler/Controllers/QfcDatamodel.cs — 483 lines; `[ExcludeFromCodeCoverage]` 25; loader assignments 40 and 51 (exactly two); `RemainingEmailLoader` declaration 140 followed by blank 141 and `#endregion Private Variables` 142; `Worker_DoWork` 185 to 229 with 205, 206, 207 and finally 209 to 216; `InitEmailQueue` 259 to 303 with start sites 273 and 300; `InitEmailQueueAsync` at 305; `WorkerStarter` absent.
2. QuickFiler.Test/Controllers/QfcDatamodelLivenessTests.cs — 255 lines; `WaitForState` 47 to 57 (SpinWait at 56); test 1 97 to 110 block, waits 103 and 106; `StartHeldOpenLoader` 148 to 181; `release.SetResult(true);` 204, 220, 246; test 3 207 to 227; test 4 229 to 253; tokens `SpinWait` 1, `.Wait(` 2, `WaitForState` 5, `new BackgroundWorker()` 2.
3. QuickFiler.Test/Controllers/QfcDatamodelTeardownTests.cs — 235 lines; `WaitForState` 59 to 67; `using` block 214 to 232 with waits 220 and 225; `QuiesceLoaderAsync(TimeSpan.FromSeconds(5))` 118 and 149; `fake.Advance(TimeSpan.FromSeconds(6))` 154; `new BackgroundWorker { ... }` at 174 does not match the `new BackgroundWorker()` token.
4. QuickFiler.Test/Controllers/QfcInitEmailQueueZeroBatchTests.cs — 212 lines; inert loader 97 to 108; Z0 117 to 133 (123, 127); Z1 doc 135 to 145 and method 146 to 164 (153, 161); Z2 176 to 210 (182, 201).
5. QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixtureTests.cs — 458 lines; `GateTimeoutMs` constant 33; `[Timeout(GateTimeoutMs)]` 8 occurrences; R4 doc 192 to 203 with flake-watch para 196 to 202; R4 declared 206; 208 to 216 arrange block; `transactionA.Install(liveA);` unique at 216; `Dispatcher original = ...` also at 55 and 115; assertions 244 to 257; R5 declared 273.
6. QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixture.cs — `EnsureDispatcher` 122 to 138; design note 26 to 30; parked thread name 231.
7. QuickFiler.Test/Controllers/QfcItemController.TestSupport.cs 238 to 239; QuickFiler.Test/Controllers/QfcItemController.FocusAndThemeTests.cs class at 27, discards at 452 and 468; QuickFiler.Test/SetupAssemblyInitializer.cs 14 to 25 (no dispatcher write).
8. scripts/vscode/Invoke-MSTestWithCoverage.ps1 — 89 to 93, 262, 297 to 298, 348 to 355, 399 to 402, 406 to 410, 415 to 423, 430 to 453, 459 to 461; scripts/vscode/Invoke-MSTest.TrxSummary.ps1 12 to 150; scripts/vscode/Invoke-MSTestWithCoverage.FirstParty.ps1 117 to 120; scripts/vscode/TaskMaster.cli.runsettings 4 to 7; scripts/vscode/Install-RepoDotNetSdk.ps1 default version; scripts/vscode/Invoke-Restore.ps1 parameters.
9. QuickFiler.Test/QuickFiler.Test.csproj 35, 157, 161, 183, 203, 554 to 558; QuickFiler.Test/packages.config 11 and 52; QuickFiler/QuickFiler.csproj 24 and 325.
10. .gitignore 146, 150, 151; .csharpierignore 4 and 12; global.json 3 to 8; dotnet-tools.json 6; scripts/hygiene/Test-RepositoryHygiene.Rules.ps1 line 21.
11. spec.md acceptance lines 266 to 282 (17 lines `- [ ] AC`); issue.md line 12; worktree reflog for the BASE cut and HEAD.

Sibling-region re-checks: the three `release.SetResult(true);` lines in the liveness file (control edits A2 and A3 target the second and third only); the three `Dispatcher original = ...` lines in the R4 file (R-INJECT anchors on the unique `transactionA.Install(liveA);`); the teardown file's `Cleanup_CalledTwice_DoesNotThrow` worker at 174 (not a start site, not edited); the teardown `TimeSpan` lines that must survive (118, 149, 154); `QuickFiler/Interfaces/IQfcDatamodel.cs` (instrumented separately, excluded by the exact filename equality in CMD-COVERAGE-POST); the R4 `[Timeout]` attribute line, which R-BODY does not re-indent, so the added-line scan for `Timeout(` reads 0. Revision R1 sibling re-checks: R1, R2 and R3 in the R4 file also call the ensure wrapper (60, 119, 166) but lie outside every R4 span; the R4 acquisition lines 212 to 214 stay outside the re-indented range; the `secondCallerStarted.Wait();` line (239) is re-indented and so appears as an added line, which the added-line scan tolerates because it does not scan `.Wait(`; the liveness test 1 documentation (73) keeps its one `IsBusy` (production behavior, issue #424); test 2's `release.SetResult(true);` (204) is untouched by batch C; the zero-batch inert-loader documentation (93, "starting a real") does not contain `starts a real` and is left unchanged.

PLANNER-INTERNAL-REVIEW: PASS
CITATION-TO-TREE: PASS
AC-TRACEABILITY: PASS
SCOPE-BOUNDARY: PASS
CITATION: QuickFiler/Controllers/QfcDatamodel.cs | lines 25, 40, 51, 140, 185-229, 259-303 (start sites 273 and 300)
CITATION: QuickFiler.Test/Controllers/QfcDatamodelLivenessTests.cs | lines 47-57, 73, 97-110, 148-187, 207-227, 229-253
CITATION: QuickFiler.Test/Controllers/QfcDatamodelTeardownTests.cs | lines 59-67, 214-232
CITATION: QuickFiler.Test/Controllers/QfcInitEmailQueueZeroBatchTests.cs | lines 23-31, 97-108, 117-133, 135-164, 176-210
CITATION: QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixtureTests.cs | lines 33, 192-203, 206-264 (acquisition 212-214, re-indent range 215-258, tail 256-261), 216, 273
CITATION: QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixture.cs | lines 26-30, 116-138, 231
CITATION: QuickFiler.Test/Controllers/QfcItemController.TestSupport.cs | lines 238-239
CITATION: QuickFiler.Test/SetupAssemblyInitializer.cs | lines 14-25
CITATION: scripts/vscode/Invoke-MSTestWithCoverage.ps1 | lines 89-93, 262, 348-355, 410, 415-423, 449-453
CITATION: scripts/vscode/TaskMaster.cli.runsettings | lines 4-7
CITATION: scripts/hygiene/Test-RepositoryHygiene.Rules.ps1 | line 21
CITATION: docs/features/active/2026-09-30-quickfiler-tests-depend-on-wall-clock-timing-950/spec.md | lines 72, 131, 236-246, 266-282
AC-INVENTORY: AC1, AC2, AC3, AC4, AC5, AC6, AC7, AC8, AC9, AC10, AC11, AC12, AC13, AC14, AC15, AC16, AC17
AC-MAPPING: AC1 | IMPLEMENTATION: P2-T1, P2-T2 | TESTS: P2-T4, P4-T2 | EVIDENCE: FEATURE/evidence/qa-gates/post-format-census.md
AC-MAPPING: AC2 | IMPLEMENTATION: P2-T3 | TESTS: P2-T4, P4-T2 | EVIDENCE: FEATURE/evidence/qa-gates/post-format-census.md
AC-MAPPING: AC3 | IMPLEMENTATION: P2-T1, P2-T2, P2-T3, P4-T1 | TESTS: P4-T2 | EVIDENCE: FEATURE/evidence/qa-gates/post-format-census.md
AC-MAPPING: AC4 | IMPLEMENTATION: P3-T1 to P3-T11 | TESTS: P6-T8 | EVIDENCE: FEATURE/evidence/qa-gates/wall-clock-tokens.md
AC-MAPPING: AC5 | IMPLEMENTATION: P2-T1 to P3-T14 | TESTS: P6-T9 | EVIDENCE: FEATURE/evidence/qa-gates/prohibited-constructs.md
AC-MAPPING: AC6 | IMPLEMENTATION: P3-T1, P3-T2 | TESTS: P4-T4, P6-T5 | EVIDENCE: FEATURE/evidence/regression-testing/targets-pass-after.md
AC-MAPPING: AC7 | IMPLEMENTATION: P3-T1, P3-T3 | TESTS: P4-T4, P6-T5 | EVIDENCE: FEATURE/evidence/regression-testing/targets-pass-after.md
AC-MAPPING: AC8 | IMPLEMENTATION: P3-T1, P3-T3, P3-T4 | TESTS: P4-T4, P6-T5 | EVIDENCE: FEATURE/evidence/regression-testing/targets-pass-after.md
AC-MAPPING: AC9 | IMPLEMENTATION: P3-T1, P3-T3, P3-T5 | TESTS: P4-T4, P6-T5 | EVIDENCE: FEATURE/evidence/regression-testing/targets-pass-after.md
AC-MAPPING: AC10 | IMPLEMENTATION: P3-T6, P3-T7 | TESTS: P4-T4, P6-T5 | EVIDENCE: FEATURE/evidence/regression-testing/targets-pass-after.md
AC-MAPPING: AC11 | IMPLEMENTATION: P3-T8, P3-T10 | TESTS: P4-T4, P6-T5 | EVIDENCE: FEATURE/evidence/regression-testing/targets-pass-after.md
AC-MAPPING: AC12 | IMPLEMENTATION: P3-T8, P3-T9, P3-T11 | TESTS: P4-T4, P6-T5 | EVIDENCE: FEATURE/evidence/regression-testing/targets-pass-after.md
AC-MAPPING: AC13 | IMPLEMENTATION: P3-T12 | TESTS: P1-T6, P4-T4, P4-T5, P6-T5 | EVIDENCE: FEATURE/evidence/regression-testing/concurrent-classes-pass-after.md
AC-MAPPING: AC14 | IMPLEMENTATION: P3-T13 | TESTS: P4-T2 | EVIDENCE: FEATURE/evidence/qa-gates/post-format-census.md
AC-MAPPING: AC15 | IMPLEMENTATION: P5-T1 to P5-T23, P5-T25 | TESTS: P1-T6, P5-T11, P5-T16, P5-T22 | EVIDENCE: FEATURE/evidence/other/negative-controls-summary.md
AC-MAPPING: AC16 | IMPLEMENTATION: P1-T1 | TESTS: P1-T5 | EVIDENCE: FEATURE/evidence/other/ambient-synchronization-context.md
AC-MAPPING: AC17 | IMPLEMENTATION: P6-T1 to P6-T5 | TESTS: P6-T5 | EVIDENCE: FEATURE/evidence/qa-gates/toolchain-final-pass.md
UNRESOLVED-GAPS: NONE

DIRECTIVE: PREFLIGHT VALIDATION ONLY
Executor preflight round 1 returned REVISIONS REQUIRED; revision R1 applies every delta, and executor preflight round 2 is requested. The signal below is the planner's request line, not a self-approval and not a newly discovered defect.
PREFLIGHT: REVISIONS REQUIRED
