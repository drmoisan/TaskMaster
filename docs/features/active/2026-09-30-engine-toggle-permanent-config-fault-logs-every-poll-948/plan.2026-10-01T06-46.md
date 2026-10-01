# 2026-09-30-engine-toggle-permanent-config-fault-logs-every-poll (Plan)

INCOMPLETE: stopped for quota

- **Issue:** #948
- **Parent (optional):** none
- **Owner:** drmoisan
- **Work Mode:** full-bug (acceptance criteria come from `docs/features/active/2026-09-30-engine-toggle-permanent-config-fault-logs-every-poll-948/spec.md` only; no user story exists for this item and none is to be authored)
- **Last Updated:** 2026-10-01T09-40
- **Status:** INCOMPLETE draft (stopped for quota before the task phases were authored; not ready for preflight)
- **Version:** 0.2
- **Revision record:** version 0.2, authoring pass interrupted by a quota stop. The spec was amended by the planner in this pass (header advanced to 1.1; AC-N and the Test Strategy toolchain step four sentence admit the coverage runner's own inner collector invocation, with the four known local shell-icon test classes excluded, when the baseline stall probe reproduces the local shell-icon failure; decision D-9 below). The verified tree facts, design decisions and delivered source below are complete; the command reference and the phase task lists are not yet written.
- **Plan path continuity:** this file is updated in place for every revision round. No timestamped sibling plan file is created for this cycle.

**Fail-closed evidence rule:** every command-bearing task writes one evidence artifact carrying `Timestamp:`, `Command:`, `EXIT_CODE:` and `Output Summary:`. A task whose artifact is missing or incomplete stays unchecked, and the plan outcome is BLOCKED or INCOMPLETE, never PASS.

**Evidence accounting rule:** the artifact path is named in the task text. Do not mark an evidence-bearing task complete without the artifact on disk at that exact path.

**Evidence location:** every artifact lives under `docs/features/active/2026-09-30-engine-toggle-permanent-config-fault-logs-every-poll-948/evidence/` in the canonical sub-kinds `baseline/`, `regression-testing/`, `qa-gates/` and `other/`. EVIDENCE_LOCATION_OVERRIDE_REJECTED: none supplied. In task text the token FEATURE abbreviates `docs/features/active/2026-09-30-engine-toggle-permanent-config-fault-logs-every-poll-948`.

## Requirement sources

- Acceptance criteria: `docs/features/active/2026-09-30-engine-toggle-permanent-config-fault-logs-every-poll-948/spec.md`, section `## Acceptance Criteria`: sixteen checkbox lines `- [ ] AC-A.` through `- [ ] AC-P.`, each on one physical line (lines 242 to 257 when this plan was authored; check-off tasks locate them by their `AC-X.` prefix, never by line number). The check-off edit changes only `- [ ] AC-X.` to `- [x] AC-X.`.
- Design record: `docs/features/active/2026-09-30-engine-toggle-permanent-config-fault-logs-every-poll-948/research/2026-10-01T07-20-engine-toggle-permanent-config-fault-logs-every-poll-research.md` (sections 1.3, 1.5, 2.1, 3, 4, 5, 7 and 8 govern this plan).
- Issue metadata: `docs/features/active/2026-09-30-engine-toggle-permanent-config-fault-logs-every-poll-948/issue.md` carries `- Work Mode: full-bug` at line 12 and no acceptance-criteria section. It is not an acceptance-criteria source.
- Predecessor: issue #944 is merged (the current production file carries the registration-before-start shape at lines 279 to 287 and the `TaskCompletionSource` marker); the executed #944 plan at `docs/features/active/2026-09-30-engine-toggle-prime-marker-registration-races-removal-944/plan.2026-09-30T07-20.md` is the template for the command reference and evidence conventions this plan reuses.

## Write Set (every file this plan creates or modifies)

Code files (the only paths outside the feature folder this plan may change):

- `TaskMaster/Ribbon/EngineToggleStateCoordinator.cs` (modify: edits E1 to E4)
- `TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.RepeatFaultSuppression.cs` (create)
- `TaskMaster.Test/TaskMaster.Test.csproj` (modify: one compile entry)

Feature documents:

- `docs/features/active/2026-09-30-engine-toggle-permanent-config-fault-logs-every-poll-948/spec.md` (AC-N, the Test Strategy step four sentence and the header were amended by the planner in this authoring pass; the executor makes check-off edits only)
- `docs/features/active/2026-09-30-engine-toggle-permanent-config-fault-logs-every-poll-948/plan.2026-10-01T06-46.md` (task check-off edits only)

Evidence files (fixed names; the write time is the `Timestamp:` field), all under FEATURE/evidence/:

- `baseline/phase0-instructions-read.md`, `baseline/scope-and-anchor.md`, `baseline/anchor-merge-base.md`, `baseline/anchor-production-shape.md`, `baseline/anchor-test-side.md`, `baseline/bootstrap-sdk.md`, `baseline/bootstrap-tool-restore.md`, `baseline/bootstrap-nuget-restore.md`, `baseline/bootstrap-dotnet-coverage.md`, `baseline/csharpier-check-baseline.md`, `baseline/msbuild-analyzer-baseline.md`, `baseline/msbuild-nullable-baseline.md`, `baseline/stall-probe.md`, `baseline/coordinator-tests-baseline.md`, `baseline/coverage-baseline.md`, `baseline/file-line-counts-baseline.md`, `baseline/phase0-commit.md`
- `regression-testing/repeat-fault-suppression-partial-tokens.md`, `regression-testing/build-before-fix.md`, `regression-testing/repeat-fault-suppression-fail-before.md`, `regression-testing/build-after-fix.md`, `regression-testing/repeat-fault-suppression-pass-after.md`
- `qa-gates/csproj-registration.md`, `qa-gates/production-edit-scope.md`, `qa-gates/implementation-commit.md`, `qa-gates/csharpier-format.md`, `qa-gates/file-line-counts.md`, `qa-gates/csharpier-check-final.md`, `qa-gates/msbuild-analyzer-final.md`, `qa-gates/msbuild-nullable-final.md`, `qa-gates/coverage-projection.md`, `qa-gates/toolchain-final-pass.md`, `qa-gates/determinism-tokens.md`, `qa-gates/footprint-scope.md`, `qa-gates/evidence-hygiene.md`
- `other/ac-status-summary.md`, `other/reduced-audit-handoff.md`

Files this plan must not touch: TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.cs (470 lines; the primary fixture with the private Harness, LoggedError, SpamEngine and SpamToggleControlId), TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.Race.cs (277 lines; its stale remark at lines 195 to 202 is a follow-up per the spec), TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.PrimeFaultOrdering.cs (77 lines), TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.PrimeRegistration.cs (175 lines), TaskMaster/Ribbon/RibbonController.EngineCommands.cs, TaskMaster/Ribbon/EngineTogglePressedStateCache.cs, TaskMaster/TaskMaster.csproj, every packages.config, TaskMaster.runsettings, scripts/vscode/TaskMaster.cli.runsettings, every file under scripts/, .claude/ (including .claude/agent-memory/, which is never staged by this plan), config/ and artifacts/. Inside the production file, `GetPressed`, `StartPrimeIfNeeded`, `StartObservedPrime`, `ApplyPrimeAsync`, `HandleToggleClickAsync`, `ExecuteToggleAsync`, the constructor, the `_primeTasks` declaration and the `GetPrimeTask` body are not edited. No raw test-result document (trx), raw coverage document (cobertura, coverage, coveragexml) or msbuild log is copied into the feature folder under any name; raw documents stay under the repository coverage directory, which .gitignore line 150 ignores (line 151 re-includes only its .gitkeep; lines 146 and 147 ignore trx and cobertura names repository-wide).

## AC identity table

| ID | Opening words of the criterion |
|---|---|
| AC-A | Repeated faulted polls report once while every poll still re-primes |
| AC-B | Repeated canceled primes report once |
| AC-C | A later successful prime recovers |
| AC-D | A different failure kind for the same key is reported once more |
| AC-E | Suppression is per key |
| AC-F | Toggle-path faults are still reported on every click |
| AC-G | The first prime-failure message states that repeats are not logged again |
| AC-H | Fail-before is demonstrated |
| AC-I | Pass-after is demonstrated |
| AC-J | All existing coordinator tests pass unchanged (four partials byte-identical to the merge base) |
| AC-K | The invariants are preserved (four named tests; TryRemove last in CompletePrime) |
| AC-L | The throwing-sink behaviour is unchanged (no try/catch/finally in CompletePrime; record immediately after the sink call) |
| AC-M | Scope is held (footprint against the merge base) |
| AC-N | The full C# toolchain passes in one final pass (amended: direct collector route admitted under a reproduced stall probe) |
| AC-O | Coverage: changed lines on both guard branches; coordinator file at least ninety percent; repository figures recorded |
| AC-P | File-size ceiling (production file and regression partial under five hundred lines; primary fixture unchanged in length) |

## Verified tree facts (re-derived in this worktree at authoring; every one is re-checked by Phase 0)

1. `TaskMaster/Ribbon/EngineToggleStateCoordinator.cs` is 442 content lines with no nullable directive; usings `System` (1) and `System.Collections.Concurrent` (2). `_primeTasks` summary 72 to 77, declaration 78 to 81 ending `>(StringComparer.Ordinal);` at 81. `GetPrimeTask` returns element 243 to 249 with the sentence `receives <see cref="Task.CompletedTask"/> can rely on the fault having been reported.` at 248 and the #942 token `cleared only after that report has returned` at 247; signature at 250. `StartPrimeIfNeeded` 264 to 289 (lock 272, `ContainsKey` 274, registration comment 279 to 282, marker 283 to 287). `StartObservedPrime` 303 to 327 (try 314, finally 318). `ApplyPrimeAsync` 334 to 349. `CompletePrime` summary 351 to 356, remarks 357 to 365, signature `private void CompletePrime(Task completed, string engineName)` at 366, body 367 to 382: ran-to-completion return 368 to 371, `failure` 373 to 375, comment `// Report-then-clear is load-bearing:` 377 to 379, `_logError(BuildPrimeFailedMessage(engineName), failure);` 380, `_primeTasks.TryRemove(engineName, out _);` 381, closing brace 382. `BuildPrimeFailedMessage` 417 to 428 with the two string lines at 424 and 425. The word `catch` occurs on lines 155, 165, 182, 203, 295, 296 and 331; only line 182 (`catch (Exception ex)`) is a code line, every other occurrence is inside a `///` comment. The word `try` occurs as a keyword at 178 and 314 and inside a string literal at 400 (`Please try again`), so a file-wide keyword count must exclude comment lines and must not be applied to `try` outside the `CompletePrime` span. `finally` occurs at 300 (comment) and 318 (keyword). `lock (` occurs once (272).
2. `TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.cs` is 470 content lines: `[TestClass]` at 22 on `public partial class EngineToggleStateCoordinatorTests` (23); `private const string SpamEngine = "Spam";` at 25 and `private const string SpamToggleControlId = "SpamBayesEnabledToggle";` at 26; fifteen `[TestMethod]` and one `[DataTestMethod]` (101) with three `[DataRow(` (102 to 104); `private sealed class Harness` at 403 with the strict mock `new Mock<IAppItemEngines>(MockBehavior.Strict)` at 424, `Engines` 423, `Coordinator` 426, `OnLogError` 445, `Invalidations` 447, `Notifications` 449, `Errors` 451; `LoggedError` 457 to 468 with `Message` and `Exception`. No `Triage` constant exists in any partial.
3. The Race partial (277 lines) has six `[TestMethod]`; the PrimeFaultOrdering partial (77 lines) one; the PrimeRegistration partial (175 lines) three. Census: `[TestMethod]` 25 across the four partials, `[DataTestMethod]` 1, `[DataRow(` 3, so the fixture executes 28 cases (the #944 pass-after run observed `total=28`). The three re-prime cleanups (primary fixture 236 to 242, Race 234 to 245 and 271 to 272) take their single-error assertion before the re-prime, so suppression of the second report breaks no existing assertion.
4. `TaskMaster.Test/TaskMaster.Test.csproj` carries `<Compile Include="Ribbon\EngineToggleStateCoordinatorTests.cs" />` at 352, the Race entry at 359, the PrimeFaultOrdering entry at 360, the PrimeRegistration entry at 361 and the EngineTogglePressedStateCacheTests entry at 362; `<ProjectReference Include="..\TaskMaster\TaskMaster.csproj">` at 373. UtilitiesCS.Test/UtilitiesCS.Test.csproj also references TaskMaster.csproj (line 959); no other test project does. Explicit compile items: an unlisted file is not compiled. `.csharpierignore` line 12 excludes project files from the formatter.
5. `TaskMaster/Ribbon/EngineToggleCatalog.cs` maps `"Spam"` to `SpamBayesEnabledToggle` (51) and `"Triage"` to `TriageEnabledToggle` (52); these are the only mapped keys.
6. Packages: FluentAssertions 8.11.0, Moq 4.21.0, MSTest.TestFramework 4.4.1 (TaskMaster.Test/packages.config 7, 41, 44). `LangVersion` is `preview` (TaskMaster/TaskMaster.csproj 31); `ValueTuple` is in mscorlib on net481.
7. scripts/vscode/Invoke-MSTestWithCoverage.ps1: parameters `SearchRoot`, `Configuration`, `CoverageOutput`, `NoExecute` (1 to 12), no test filter; inner arguments hard-code `/TestCaseFilter:TestCategory!=LiveOutlook` (91), the results directory (92) and the trx name (93); `ConvertTo-DerivedCoverageSettingsXml` at 97; assembly discovery excludes paths matching `(^|\\)\.claude\\` relative to the search root (353), so a worktree under .claude/worktrees is discoverable; `Discovered N test assemblies.` printed at 374. Helpers.ps1: `Get-CoberturaClassLineSummary` at 160 taking `-ClassNode` and returning `LineMap` (entries carry `Hits`, `Branch`, `Covered`, `Total`), `TotalLines`, `CoveredLines`, `TotalBranches`, `CoveredBranches` (251 to 257); `Merge-CoberturaClassesByFilename` 260; `ConvertTo-KoverageCoberturaXml` 407. Threshold.ps1: `Assert-CoberturaLineCoverageThreshold` 3, `Assert-CoberturaBranchCoverageThreshold` 58. FirstParty.ps1: `Get-CoberturaFirstPartyCoverageReport` 123. Projection.ps1: `ConvertTo-JacocoPackageProjection` 14, `Assert-JacocoProjectionReconciliation` 83. TrxSummary.ps1: `Get-TrxRunSummary` 12, `Format-TrxRunSummary` 103.
8. scripts/vscode/TaskMaster.cli.runsettings sets `Workers` 0 and `Scope` ClassLevel (5 to 6). global.json, dotnet-tools.json, coverage.config, coverage/.gitkeep, BannedSymbols.txt, scripts/vscode/Install-RepoDotNetSdk.ps1, scripts/vscode/Invoke-Restore.ps1 and scripts/hygiene/Test-RepositoryHygiene.Rules.ps1 exist.
9. `.editorconfig` line 27 sets `dotnet_analyzer_diagnostic.severity = suggestion`, so analyzer rules do not promote to errors under the nullable gate; the production file carries no `#nullable enable`.
10. Observed success-case outputs on this machine (#944 evidence, 2026-09-30): `dotnet tool run csharpier format .` prints `Formatted 1627 files in <ms>ms.` (files processed, not files changed); `dotnet tool run csharpier check .` prints `Checked 1627 files in <ms>ms.` and names a path only for an unformatted file; `dotnet tool restore` prints `Tool 'csharpier' (version '1.2.6') was restored.`; `dotnet --version` prints `8.0.205`; the stall probe exited 1 with one shell-icon failure (`STALL-PROBE: REPRODUCES`, `COVERAGE-ROUTE: DIRECT`); the direct collector route ran 9 test assemblies, 7327 tests, with `First-party coverage: lines 56098/65750 (85.32%), branches 13597/17054 (79.73%)` and `COORD-LINES covered=157 valid=157`, `COORD-BRANCHES covered=37 valid=38`, `METHOD CompletePrime span=366-382 elements=10 covered=10 uncovered=0 rate=100`; the fail-before message of a FluentAssertions boolean assertion read `Expected handleCompletedDuringRead to be False because ..., but found True.` (the `{reason}` and `but found` template this plan relies on for the numeric `Be` assertion).
11. `.claude/hooks/validate-planner-output.ps1`: phase heading regex at 238 (`### Phase N — <Title>` with an em dash); each task opening line needs a separator-bearing path (95); the final phase text must carry QA vocabulary (339); the bounded `PLANNER-INTERNAL-REVIEW` record is validated against the agent's chat output (113 to 228) and must end at exactly one `PREFLIGHT:` signal (109, 121).
12. `docs/features/potential/promoted/2026-09-30-engine-toggle-permanent-config-fault-logs-every-poll.md` exists (`Status: Promoted` at 5, `Issue: #948` at 9) and is tracked on the branch (the delegation reported a clean worktree). The branch was cut from origin/main `9b3eea58447c264eae6f95a4bfee3bfcec7fb17f`; Phase 0 derives the diff base at execution time and never uses this literal as a ref operand.

## Design decisions (do not redesign)

- **D-1 Fix shape (spec Proposed Fix, research 2.1 and 4.2).** New field `_reportedPrimeFaults`, a `ConcurrentDictionary<(string EngineName, Type FaultType), byte>` declared after the `_primeTasks` declaration. In `CompletePrime` the unconditional report becomes a guarded block: `var reportKey = (EngineName: engineName, FaultType: failure.GetType());` then `if (!_reportedPrimeFaults.ContainsKey(reportKey)) { _logError(BuildPrimeFailedMessage(engineName), failure); _reportedPrimeFaults[reportKey] = 0; }` followed by the unchanged `_primeTasks.TryRemove(engineName, out _);` as the last statement. The record uses the indexer (no discarded `TryAdd` result) and sits immediately after the sink call, so a throwing sink leaves the pair unrecorded. The tuple literal names its elements explicitly so the inferred type equals the field's key type exactly. No try, catch, finally or lock is added anywhere.
- **D-2 Message.** `BuildPrimeFailedMessage` appends `Further failures of this kind for this engine are not logged again.` as a third concatenated string segment; the leading text is unchanged so every existing `Contain(SpamEngine)` assertion still holds.
- **D-3 Documentation.** The `CompletePrime` summary gains the suppression clause; its remarks gain a second paragraph stating the rule, why the record follows the sink, and the two constraints the sibling throwing-sink fix (issue 947) must respect (record after the sink returns; never inside a finally). The report-then-clear comment gains `(if any)` after `report` and a closing clause naming the suppressed case. The `GetPrimeTask` returns element gains `, or deliberately suppressed as a repeat of a failure kind already reported for that key.` The `logError` constructor parameter documentation is not edited.
- **D-4 New partial.** `TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.RepeatFaultSuppression.cs` follows the Race partial's shape: no `[TestClass]`, usings System, System.IO (IOException for test D), System.Threading, System.Threading.Tasks, FluentAssertions, Microsoft.VisualStudio.TestTools.UnitTesting and Moq; one `private const string TriageEngine = "Triage";`; seven `[TestMethod]` methods A to G named exactly as the spec's Test Strategy; one private static helper `PollAsync(Harness harness, string engineName, int polls)` returning the last read's answer, whose loop bound is a caller constant (never a condition on coordinator state). No new Harness member, type or mock. Each test constructs its own `Harness`.
- **D-5 Fail-before is a real run and every one of the seven tests fails before the fix, by program order.** Against the unchanged production file: A makes five reports (its first assertion, the numeric `Errors.Count.Should().Be(1, ...)`, fails with `but found 5` in its message); B five reports (`ContainSingle` fails); C two reports before the success (`ContainSingle` fails); D four reports (`HaveCount(2)` fails); E three reports, two Spam and one Triage (`HaveCount(2)` fails); F three reports, two prime and one toggle (`HaveCount(2)` fails); G's message lacks the sentence (`Contain("not logged again")` fails). Test A asserts the count through the numeric assertion rather than `ContainSingle` so that the fail-before message carries the observed count (`Expected ... to be 1 because a repeated fault of one kind for one engine is reported once, but found 5.`); the reason fragment `a repeated fault of one kind for one engine is reported once` occurs nowhere else in the fixture, so a compile error, an assembly-load failure or a timeout cannot produce it. The fail-before gate requires all seven `Failed`, the A message fragment, and `FAIL-BEFORE-ERROR-COUNT: 5` parsed from `but found (\d+)`; any other value is `FAIL-BEFORE COUNT UNEXPECTED`: stop for re-planning.
- **D-6 Self-anchor.** Phase 0 runs `git fetch origin` and records `MERGE-BASE:` as the output of `git merge-base origin/main HEAD`. Wherever the literal MERGE-BASE appears in a command of this plan, the executor substitutes that recorded 40-character value. The cited code files, project file, run settings and scripts/vscode are compared between MERGE-BASE and origin/main; any difference is `UPSTREAM CITED FILES CHANGED`: record the paths and stop. No merge of origin/main is performed by this plan. `INHERITED-COMMITTED:` (`git diff --name-status MERGE-BASE HEAD` at Phase 0, before any edit) may contain only feature-folder paths; anything else is `INHERITED SET EXCEEDS AC-M SCOPE`: stop for the orchestrator.
- **D-7 Coverage route is selected by a recorded observation.** The stall probe runs the four UtilitiesCS.Test shell-icon classes alone; `STALL-PROBE: CLEAR` (exit 0, failed 0, no hang dump) selects `COVERAGE-ROUTE: RUNNER` (scripts/vscode/Invoke-MSTestWithCoverage.ps1 verbatim); `REPRODUCES` selects `DIRECT` (the runner's inner collector invocation with those four classes excluded and the vstest hang-blame switch appended, post-processed with the runner's own helpers and floor functions). Both routes yield the same committed forms: the `First-party coverage:` line, the JaCoCo package projection, the trx-derived summary, and the per-method coordinator figures.
- **D-8 Per-method and guard-branch figures.** From the post-processed Cobertura document, the single `class` element whose `filename` ends with `TaskMaster/Ribbon/EngineToggleStateCoordinator.cs` is reduced with `Get-CoberturaClassLineSummary`. Method spans run from the first source line matching eight spaces, `private`, an optional `static`, a return type and the method name followed by an opening parenthesis, through the first following line that is exactly eight spaces and a closing brace; rates are 100 times covered elements over elements, rounded to two decimals, for `CompletePrime` and `BuildPrimeFailedMessage`. The guard line is the source line containing `if (!_reportedPrimeFaults.ContainsKey(reportKey))`; its `LineMap` entry must report `Branch` True with `Covered` equal to `Total` and `Total` at least 2 (both outcomes exercised: test A's first cycle takes the branch, its later cycles skip it), and the record line `_reportedPrimeFaults[reportKey] = 0;` must have hits at least 1. The repository-wide figures are compared under the #944 comparability rule: `COMPARABLE` when the two root lines-valid figures differ by at most one percent of the baseline (then the final root line rate must be at least the baseline rate minus 0.005), otherwise `INCOMPARABLE` (recorded, not gated).
- **D-9 Spec amendment made by the planner in this pass.** AC-N and the Test Strategy step four sentence now admit the DIRECT route when the baseline stall probe reproduces the known local shell-icon failure or stall; the spec header advanced to version 1.1. Reason: the #944 run on this machine observed one shell-icon test failing under the same filter the runner applies, so the unamended criterion (runner only) was unsatisfiable here. No other criterion text changed.
- **D-10 Commits.** Three commits, each `git add -- <pathspecs>` then a separate `git commit`, never chained, never `git add -A`: Phase 0 (feature folder only, exempt form `git commit -m "<message>" -- <feature folder>`), Phase 2 (the three code files and the feature folder, staged only after a scoped CSharpier format of the two C# files), Phase 3 (feature folder, exempt form). No `-m` value contains an angle bracket, a dollar sign or a backtick. `.claude/agent-memory/**` is never staged. No `git update-index` is used. A PreToolUse refusal is reported verbatim as `PRE-IMPLEMENTATION GATE BLOCKED` and stops the run.
- **D-11 Git gates.** Every `git diff` carries MERGE-BASE as its ref operand (two-dot, working tree against the base) or `--cached`; every name-listing diff is paired with a `git status --porcelain` span in the same task; porcelain gates after a commit assert scope only (no entry under TaskMaster/ or TaskMaster.Test/), never membership or count, and admit this plan file.
- **D-12 Fail-closed check-offs.** Every check-off task sits in Phase 3 after the toolchain loop, flips exactly one checkbox, and completes with the box unchecked when its evidence does not hold, appending `AC-X: MET` or `AC-X: NOT MET` with the failing values to FEATURE/evidence/other/ac-status-summary.md.
- **D-13 Payload conventions.** Every pwsh payload begins `Set-Location -LiteralPath "WORKTREE"` followed by `[Environment]::CurrentDirectory = (Get-Location).Path`, so cmdlets and .NET APIs resolve relative paths identically; payloads are passed as `pwsh -NoProfile -Command '<payload>'` with outer single quotes and inner double quotes only; MSBuild and vstest.console.exe are resolved through vswhere inside each payload; WORKTREE is never written into an artifact.

## Delivered source (the executor writes these texts; CSharpier output wins on any layout difference, and the token gates are re-run on the formatted text)

**Production file `TaskMaster/Ribbon/EngineToggleStateCoordinator.cs`, edit E1 — new field.** Insert, after the line `>(StringComparer.Ordinal);` that closes the `_primeTasks` declaration, one blank line and then:

        /// <summary>
        /// Prime failures already reported, keyed by engine and base-exception type. A present
        /// key means a later failure of the same kind for the same engine is not reported again.
        /// Never cleared: a key that gains a cached value never primes again, so no entry can
        /// become stale (issue #948).
        /// </summary>
        private readonly ConcurrentDictionary<
            (string EngineName, Type FaultType),
            byte
        > _reportedPrimeFaults = new ConcurrentDictionary<(string, Type), byte>();

**Edit E2 — `CompletePrime` documentation and body.** Replace every line from the `/// <summary>` line directly above `Observes the outcome of a prime.` through the closing brace of `CompletePrime` with:

        /// <summary>
        /// Observes the outcome of a prime. On any outcome other than ran-to-completion the cache
        /// is left unset — so the key still reports unchecked — the failure is reported through
        /// <c>logError</c> unless a failure of the same base-exception type has already been
        /// reported for this engine, and only then is the in-flight marker cleared so a later read
        /// may re-prime.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The status is tested rather than the exception. A CANCELED task carries a null
        /// <see cref="Task.Exception"/>, so a handler keyed on the exception returned early for a
        /// cancellation: nothing was logged, the cache stayed unset, and the in-flight marker stayed
        /// registered, which blocked any re-prime for the rest of the session. When there is no
        /// exception to unwrap a <see cref="TaskCanceledException"/> is synthesized so the sink
        /// always receives one. The faulted path is unchanged and still reports the unwrapped base
        /// exception.
        /// </para>
        /// <para>
        /// Repeat suppression (issue #948): a prime that keeps failing against a cached fault
        /// would otherwise be reported once per cache-miss poll without bound, so each pair of
        /// engine key and base-exception type is reported once per coordinator lifetime and then
        /// recorded in <see cref="_reportedPrimeFaults"/>. The pair is recorded only after the
        /// sink has returned, so a sink that throws leaves the report owed rather than
        /// suppressed. A later change that guarantees the marker is cleared when the sink throws
        /// must keep the record after the sink returns and outside any finally block; moving it
        /// earlier would suppress the report for the whole session. Re-priming is unchanged, so
        /// recovery stays automatic.
        /// </para>
        /// </remarks>
        private void CompletePrime(Task completed, string engineName)
        {
            if (completed.Status == TaskStatus.RanToCompletion)
            {
                return;
            }

            var failure =
                (Exception)completed.Exception?.GetBaseException()
                ?? new TaskCanceledException(completed);

            // Report-then-clear is load-bearing: the marker stays registered until the report
            // (if any) has returned, so a caller that observes the marker absent — including one
            // that fetched the prime handle after the fault — is guaranteed the fault has already
            // been reported, or deliberately suppressed as a repeat of a kind already reported.
            var reportKey = (EngineName: engineName, FaultType: failure.GetType());
            if (!_reportedPrimeFaults.ContainsKey(reportKey))
            {
                _logError(BuildPrimeFailedMessage(engineName), failure);
                _reportedPrimeFaults[reportKey] = 0;
            }

            _primeTasks.TryRemove(engineName, out _);
        }

**Edit E3 — `BuildPrimeFailedMessage`.** Replace the two string lines (`"Reading the activation state for engine '{0}' failed; its toggle continues to "` and `+ "report unchecked.",`) with:

                "Reading the activation state for engine '{0}' failed; its toggle continues to "
                    + "report unchecked. Further failures of this kind for this engine are not "
                    + "logged again.",

**Edit E4 — `GetPrimeTask` returns element.** Replace the line `/// receives <see cref="Task.CompletedTask"/> can rely on the fault having been reported.` with:

        /// receives <see cref="Task.CompletedTask"/> can rely on the fault having been reported, or
        /// deliberately suppressed as a repeat of a failure kind already reported for that key.

Documented tokens quoted here in prose so the presence gates are exonerated: Prime failures already reported, keyed by engine and base-exception type; unless a failure of the same base-exception type has already been; Repeat suppression (issue #948); after the sink returns and outside any finally block; (if any) has returned; deliberately suppressed as a repeat of a failure kind already reported for that key; Further failures of this kind for this engine are not; logged again. Expected post-change size: 442 plus about 31 lines, under 500.

**New partial `TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.RepeatFaultSuppression.cs` (whole text; the four leading spaces of Markdown indent are removed on every line when the file is written).**

    using System;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    namespace TaskMaster.Test.Ribbon
    {
        /// <summary>
        /// Regression tests for issue #948: a prime failure is reported through the error-log sink
        /// once per engine key and base-exception type, every failed prime still clears its marker so
        /// the next cache-miss read re-primes, and recovery stays automatic. A fifth partial of the
        /// coordinator fixture, so the private <c>Harness</c> and <c>LoggedError</c> types and the
        /// fixture constants are reused without adding any harness member.
        /// </summary>
        /// <remarks>
        /// Every activation read returns an already completed task (faulted, canceled or successful),
        /// which models the cached <c>AsyncLazy</c> fault exactly, and every poll awaits the
        /// coordinator's own prime handle, so each outcome is decided by program order. No test
        /// sleeps, polls a condition, reads the wall clock, touches the filesystem or starts a
        /// message pump.
        /// </remarks>
        public partial class EngineToggleStateCoordinatorTests
        {
            private const string TriageEngine = "Triage";

            #region Issue #948 — repeat prime-failure reports are suppressed per engine and fault kind

            /// <summary>
            /// Regression for issue #948 and the test that carries the fail-before obligation. Five
            /// cache-miss polls against one already faulted activation read re-prime five times and
            /// report once. The count is asserted first, through the numeric assertion, so that the
            /// fail-before message carries the observed number of reports: before the fix every poll
            /// reports and the message reads "but found 5".
            /// </summary>
            [TestMethod]
            public async Task GetPressed_WhenPrimeFaultsRepeatedly_LogsFirstFaultOnly()
            {
                // Arrange
                var harness = new Harness();
                var failure = new InvalidOperationException("configuration load failed");
                var faulted = Task.FromException<bool>(failure);
                harness.Engines.Setup(x => x.EngineActiveAsync(SpamEngine)).Returns(faulted);

                // Act
                var pressed = await PollAsync(harness, SpamEngine, 5);

                // Assert
                harness
                    .Errors.Count.Should()
                    .Be(1, "a repeated fault of one kind for one engine is reported once");
                harness
                    .Errors[0]
                    .Exception.Should()
                    .BeSameAs(failure, "the first report carries the injected fault");
                harness.Errors[0].Message.Should().Contain(SpamEngine);
                harness.Engines.Verify(
                    x => x.EngineActiveAsync(SpamEngine),
                    Times.Exactly(5),
                    "suppressing the report must not suppress the re-prime"
                );
                pressed.Should().BeFalse("a failed prime leaves the toggle reporting unchecked");
                harness.Invalidations.Should().BeEmpty("a failed prime changed no state to display");
            }

            /// <summary>
            /// A canceled prime synthesizes a fresh cancellation exception on every cycle; the
            /// synthesized exceptions share one type, so repeated cancellations are one failure kind
            /// and are reported once.
            /// </summary>
            [TestMethod]
            public async Task GetPressed_WhenPrimeIsCanceledRepeatedly_LogsFirstCancellationOnly()
            {
                // Arrange
                var harness = new Harness();
                var canceled = Task.FromCanceled<bool>(new CancellationToken(true));
                harness.Engines.Setup(x => x.EngineActiveAsync(SpamEngine)).Returns(canceled);

                // Act
                var pressed = await PollAsync(harness, SpamEngine, 5);

                // Assert
                harness.Errors.Should().ContainSingle("repeated cancellations are one failure kind");
                harness
                    .Errors[0]
                    .Exception.Should()
                    .BeAssignableTo<OperationCanceledException>(
                        "a canceled task carries no exception to unwrap, so one is synthesized"
                    );
                harness.Engines.Verify(
                    x => x.EngineActiveAsync(SpamEngine),
                    Times.Exactly(5),
                    "every canceled prime clears its marker, so every poll re-primes"
                );
                pressed.Should().BeFalse("a canceled prime stored no value");
            }

            /// <summary>
            /// Recovery. Two faults of one kind, the second a suppressed repeat, are followed by a
            /// successful read: the value is cached, the control is invalidated exactly once, and no
            /// further prime runs because the key is now a cache hit.
            /// </summary>
            [TestMethod]
            public async Task GetPressed_AfterSuppressedFaults_LaterSuccessfulPrimeCachesStateAndInvalidatesOnce()
            {
                // Arrange
                var harness = new Harness();
                var failure = new InvalidOperationException("configuration load failed");
                var faulted = Task.FromException<bool>(failure);
                harness
                    .Engines.SetupSequence(x => x.EngineActiveAsync(SpamEngine))
                    .Returns(faulted)
                    .Returns(faulted)
                    .Returns(Task.FromResult(true));

                // Act
                await PollAsync(harness, SpamEngine, 3);

                // Assert
                harness.Errors.Should().ContainSingle("the second identical fault is a suppressed repeat");
                harness
                    .Coordinator.GetPressed(SpamEngine)
                    .Should()
                    .BeTrue("the successful prime cached the real state, so the read is a cache hit");
                harness
                    .Invalidations.Should()
                    .Equal(
                        new[] { SpamToggleControlId },
                        "only the successful prime changed state to display"
                    );
                harness.Engines.Verify(
                    x => x.EngineActiveAsync(SpamEngine),
                    Times.Exactly(3),
                    "each faulted poll re-primed and the cached key primes no more"
                );
            }

            /// <summary>
            /// The suppression key includes the base-exception type, so a fault of a new kind for a key
            /// that already has a reported fault is reported once more. This is the path a transient
            /// startup fault followed by a permanent configuration fault takes.
            /// </summary>
            [TestMethod]
            public async Task GetPressed_WhenFailureKindChanges_LogsNewKindOnce()
            {
                // Arrange
                var harness = new Harness();
                var firstKind = new InvalidOperationException("configuration load failed");
                var secondKind = new IOException("configuration file unreadable");
                var firstFaulted = Task.FromException<bool>(firstKind);
                var secondFaulted = Task.FromException<bool>(secondKind);
                harness
                    .Engines.SetupSequence(x => x.EngineActiveAsync(SpamEngine))
                    .Returns(firstFaulted)
                    .Returns(firstFaulted)
                    .Returns(secondFaulted)
                    .Returns(secondFaulted);

                // Act
                await PollAsync(harness, SpamEngine, 4);

                // Assert
                harness.Errors.Should().HaveCount(2, "each distinct failure kind is reported once");
                harness.Errors[0].Exception.Should().BeSameAs(firstKind, "the first kind is reported");
                harness.Errors[1].Exception.Should().BeSameAs(secondKind, "a new kind is not a repeat");
                harness.Engines.Verify(
                    x => x.EngineActiveAsync(SpamEngine),
                    Times.Exactly(4),
                    "every faulted poll re-primed"
                );
            }

            /// <summary>
            /// Suppression is per engine key: a suppressed Spam fault does not suppress the first
            /// Triage fault. The two mapped keys are the only keys a prime can carry.
            /// </summary>
            [TestMethod]
            public async Task GetPressed_WhenSpamFaultIsSuppressed_FirstTriageFaultIsStillLogged()
            {
                // Arrange
                var harness = new Harness();
                var spamFailure = new InvalidOperationException("spam configuration load failed");
                var triageFailure = new InvalidOperationException("triage configuration load failed");
                harness
                    .Engines.Setup(x => x.EngineActiveAsync(SpamEngine))
                    .Returns(Task.FromException<bool>(spamFailure));
                harness
                    .Engines.Setup(x => x.EngineActiveAsync(TriageEngine))
                    .Returns(Task.FromException<bool>(triageFailure));

                // Act
                await PollAsync(harness, SpamEngine, 2);
                await PollAsync(harness, TriageEngine, 1);

                // Assert
                harness.Errors.Should().HaveCount(2, "suppression is keyed by engine as well as by kind");
                harness.Errors[0].Message.Should().Contain(SpamEngine);
                harness.Errors[0].Exception.Should().BeSameAs(spamFailure);
                harness.Errors[1].Message.Should().Contain(TriageEngine);
                harness.Errors[1].Exception.Should().BeSameAs(triageFailure);
            }

            /// <summary>
            /// The suppression applies to prime failures only. A toggle fault after a suppressed prime
            /// fault is still reported, because the click boundary reports every click.
            /// </summary>
            [TestMethod]
            public async Task HandleToggleClickAsync_AfterSuppressedPrimeFault_StillLogsToggleFault()
            {
                // Arrange
                var harness = new Harness();
                var primeFailure = new InvalidOperationException("configuration load failed");
                var toggleFailure = new InvalidOperationException("toggle failed");
                harness
                    .Engines.Setup(x => x.EngineActiveAsync(SpamEngine))
                    .Returns(Task.FromException<bool>(primeFailure));
                harness.Engines.Setup(x => x.ToggleEngineAsync(SpamEngine)).ThrowsAsync(toggleFailure);

                // Act
                await PollAsync(harness, SpamEngine, 2);
                await harness.Coordinator.HandleToggleClickAsync(SpamEngine);

                // Assert
                harness.Errors.Should().HaveCount(2, "one suppressed prime repeat, every toggle fault");
                harness.Errors[0].Exception.Should().BeSameAs(primeFailure);
                harness.Errors[1].Exception.Should().BeSameAs(toggleFailure, "the click boundary reports");
                harness.Invalidations.Should().BeEmpty("neither path changed state to display");
                harness.Notifications.Should().BeEmpty("a fault is logged, not surfaced as a notice");
            }

            /// <summary>
            /// The first prime-failure entry tells the reader that repeats are suppressed, so a log
            /// that shows one entry is not read as a fault that cleared.
            /// </summary>
            [TestMethod]
            public async Task GetPressed_WhenPrimeFaults_FirstReportStatesRepeatsAreNotLoggedAgain()
            {
                // Arrange
                var harness = new Harness();
                var failure = new InvalidOperationException("configuration load failed");
                harness
                    .Engines.Setup(x => x.EngineActiveAsync(SpamEngine))
                    .Returns(Task.FromException<bool>(failure));

                // Act
                await PollAsync(harness, SpamEngine, 1);

                // Assert
                harness.Errors.Should().ContainSingle("one faulted poll produces one report");
                harness
                    .Errors[0]
                    .Message.Should()
                    .Contain("not logged again", "the entry must state that repeats are suppressed");
                harness
                    .Errors[0]
                    .Message.Should()
                    .EndWith("Further failures of this kind for this engine are not logged again.");
            }

            #endregion Issue #948 — repeat prime-failure reports are suppressed per engine and fault kind

            /// <summary>
            /// Runs a fixed number of cache-miss polls, each the synchronous read followed by awaiting
            /// the prime it started, and returns the answer of the last read. The count is a constant
            /// chosen by the caller, never a condition on coordinator state, so the loop cannot spin.
            /// </summary>
            private static async Task<bool> PollAsync(Harness harness, string engineName, int polls)
            {
                var pressed = false;
                for (var poll = 0; poll < polls; poll++)
                {
                    pressed = harness.Coordinator.GetPressed(engineName);
                    await harness.Coordinator.GetPrimeTask(engineName);
                }

                return pressed;
            }
        }
    }

Test-side tokens quoted here in prose so the presence gates are exonerated: a repeated fault of one kind for one engine is reported once; suppressing the report must not suppress the re-prime; repeated cancellations are one failure kind; the second identical fault is a suppressed repeat; each distinct failure kind is reported once; suppression is keyed by engine as well as by kind; one suppressed prime repeat, every toggle fault; the entry must state that repeats are suppressed; Further failures of this kind for this engine are not logged again.

**Project file `TaskMaster.Test/TaskMaster.Test.csproj`.** One line inserted immediately after the line carrying `EngineToggleStateCoordinatorTests.PrimeRegistration.cs` (line 361 at authoring; re-derived by Phase 0): `<Compile Include="Ribbon\EngineToggleStateCoordinatorTests.RepeatFaultSuppression.cs" />` with the same four-space indentation as its neighbours.

## Phase structure (to be authored; stopped for quota)

The intended structure, carried over from the executed #944 plan with the following substitutions: results directories under `coverage\test-results\948\`; the test-name list `NAMES-948` = the seven new tests plus `GetPressed_WhenPrimeFaults_PrimeHandleStaysRegisteredUntilFaultIsLogged`, `GetPressed_OnCacheMissWithEnginesAvailable_StartsExactlyOnePrime`, `GetPressed_WhenPrimeStarts_RegistersPrimeHandleBeforeActivationReadRuns`, `GetPressed_WhenPrimeIsCanceled_LogsErrorAndClearsPrimeMarker`, `HandleToggleClickAsync_WhenToggleFaults_LogsErrorDoesNotThrowDoesNotInvalidate`, `GetPressed_WhenPrimeFaults_LogsErrorAndStillReturnsFalse` and `ExecuteToggleAsync_WhenToggleFaults_PropagatesUnchanged`; the per-method coverage rows for `CompletePrime` and `BuildPrimeFailedMessage` with the guard-line branch row of D-8; whitespace-stripped token counts (text with every whitespace run removed) for the field declaration (`ConcurrentDictionary<(stringEngineName,TypeFaultType),byte>_reportedPrimeFaults` and `>_reportedPrimeFaults=newConcurrentDictionary<(string,Type),byte>();` each 1) and per-line counts for every other token; the AC-L gate as a code-anchored search (lines whose trimmed text does not start with `//`, regex `\bcatch\b`, expected exactly 1 file-wide with that line containing `catch (Exception ex)`; `\btry\b`, `\bcatch\b`, `\bfinally\b` and `\block\b` each 0 on the code lines of the `CompletePrime` span; the span's last statement line equal to `_primeTasks.TryRemove(engineName, out _);`; the record line equal to the `_logError` line plus one), verified at Phase 0 to return the same single click-boundary hit on the unchanged file.

- Phase 0 — policy reads; spec, issue and research read with the sixteen-criterion count; self-anchor (`git fetch origin`, MERGE-BASE, upstream comparison of cited files, inherited set, porcelain); production and test-side anchor shape; SDK, tool restore, NuGet restore, dotnet-coverage bootstrap (guarded); csharpier check baseline; analyzer and nullable `/t:Rebuild` baselines with the CoreCompile non-vacuity counts; stall probe; coordinator-class baseline run (28 cases expected; total recorded as `BASELINE-TOTAL:`); coverage baseline by the selected route; line counts and hashes; Phase 0 docs commit.
- Phase 1 — create the partial and run its token gates; register the compile entry; incremental build; `[expect-fail]` coordinator run recorded in `evidence/regression-testing/repeat-fault-suppression-fail-before.md` with all seven new tests `Failed`, the A message fragment and `FAIL-BEFORE-ERROR-COUNT: 5`, total equal to `BASELINE-TOTAL:` plus 7, exit code non-zero with `ExpectedExitCode:` equal to the observed value.
- Phase 2 — edits E1 to E4; build; pass-after run recorded in `evidence/regression-testing/repeat-fault-suppression-pass-after.md` (35 of 35 passed); population comparison; production edit scope and shape gates; protected-file diffs against MERGE-BASE; scoped CSharpier format of the two C# files, token re-check, implementation commit.
- Phase 3 — Final QA Toolchain Loop, Coverage Delta, Footprint and Acceptance: `dotnet tool run csharpier format .` with hash and scoped-porcelain observation; post-format gate re-run; line counts (AC-P); `dotnet tool run csharpier check .`; analyzer and nullable `/t:Rebuild` gates; coordinator fixture on the rebuilt assembly; coverage run by the selected route into `evidence/qa-gates/coverage-projection.md`; `evidence/qa-gates/toolchain-final-pass.md`; coverage comparison and changed-line and guard-branch rows; determinism tokens over the anchored diff of TaskMaster.Test/Ribbon (the single bounded `for (` of the helper admitted by count); raw-document and footprint checks (anchored `git diff --name-status MERGE-BASE HEAD` paired with porcelain; AC-J byte identity by `git diff --exit-code MERGE-BASE -- ` over the four existing partials); hygiene sweep; sixteen check-off tasks AC-A to AC-P; status summary; reduced-audit handoff; final docs commit.

## Planner Adversarial Self-Review

SELF-REVIEW: BLOCKED

The authoring pass was stopped for quota before the command reference, the phase task lists and the bounded internal review record were written. The citations in the Verified tree facts section were re-derived in this pass against the assigned worktree (the production file, the four test partials, the project file, EngineToggleCatalog.cs, packages.config, the coverage scripts, the runsettings, .gitignore, .csharpierignore, .editorconfig, the hook, the spec, the issue, the research record and the promotion record), but the plan is not complete and must not be handed to preflight in this state.
