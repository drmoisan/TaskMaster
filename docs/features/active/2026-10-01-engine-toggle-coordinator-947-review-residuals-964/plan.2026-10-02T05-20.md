# 2026-10-01-engine-toggle-coordinator-947-review-residuals (Plan)

- **Issue:** #964
- **Parent (optional):** none
- **Owner:** drmoisan
- **Work Mode:** minor-audit (`docs/features/active/2026-10-01-engine-toggle-coordinator-947-review-residuals-964/issue.md` line 12 reads `- Work Mode: minor-audit`)
- **Last Updated:** 2026-10-02T07-40
- **Status:** Draft, revision round 3 applied (preflight deltas: forward-slash `CLEARANCE-PATH:`, per-file `HIT-FILE` attribution in CMD-HYGIENE, preparation-phase record counted in the P2-T9 and P2-T20 lower bounds), awaiting executor preflight
- **Version:** 1.3 (revision round 3: P0-T2 records `CLEARANCE-PATH:` in repository-relative forward-slash form and P2-T19 compares the Glob result after the same conversion; CMD-HYGIENE prints one `HIT-FILE` row per file with a non-zero host count, and P2-T9 and P2-T20 repair or stop by those rows; the P2-T9 lower bound becomes 33 and the P2-T20 lower bound becomes 36, each now counting the preparation-phase record, superseding the version 1.2 wording that scanned it outside the bounds). Version 1.2 (revision round 2: the `## Write Set` names the preparation-phase record `other/preflight-clearance.<yyyy-MM-ddTHH-mm>.md`; P0-T2 records `CLEARANCE-PATH:` and `CLEARANCE-BLOB:`; P2-T19 admits exactly that one record and requires its working-tree hash to equal `CLEARANCE-BLOB:`; P2-T9 and P2-T20 state that the record is scanned outside their lower bounds and is never repaired by this plan). Version 1.1 (revision round 1: the pre-existing-failure comparison uses fully qualified test names (D1); P1-T5, P1-T11 and P1-T21 name and create their evidence artifacts (D2); a final host-path sweep P2-T20 runs after the last artifact is written (D3)). Version 1.0 was the initial authoring; the scaffold that occupied this path was replaced in full.
- **Plan path continuity:** this file is updated in place for every revision round. No timestamped sibling plan file is created for this cycle.
- **Execution topology:** The executor for this plan must be dispatched without worktree isolation: the Bash-tool isolation guard refuses every `pwsh` invocation in an isolated agent, and every toolchain step in this plan is a pwsh payload.

**Fail-closed evidence rule:** every command-bearing task writes one evidence artifact carrying `Timestamp:`, `Command:`, `EXIT_CODE:` and `Output Summary:`. A task whose artifact is missing or incomplete stays unchecked, and the plan outcome is BLOCKED or INCOMPLETE, never PASS. Baseline, final-QC and coverage-comparison artifacts are mandatory; a missing one makes the audit verdict BLOCKED.

**Evidence accounting rule:** the artifact path is named in each task. Do not mark an evidence-bearing task complete without the artifact on disk at that exact path.

**Evidence location:** every artifact lives under `docs/features/active/2026-10-01-engine-toggle-coordinator-947-review-residuals-964/evidence/` in the canonical sub-kinds `baseline/`, `regression-testing/`, `qa-gates/` and `other/`. EVIDENCE_LOCATION_OVERRIDE_REJECTED: none supplied. In task text the token FEATURE abbreviates `docs/features/active/2026-10-01-engine-toggle-coordinator-947-review-residuals-964`.

## Requirement sources

- Sole requirements source: `docs/features/active/2026-10-01-engine-toggle-coordinator-947-review-residuals-964/issue.md`, section `## Acceptance Criteria` (line 23), eight checkbox lines `- [ ] AC1` through `- [ ] AC8` (lines 27 to 34 when this plan was authored; check-off tasks locate each line by its `- [ ] ACn (` prefix, never by line number). Only that section is an acceptance-criteria source. No `spec.md`, `user-story.md` or `research.md` exists in the feature folder or is required; P0-T2 fails closed if one appears.
- Predecessors (merged into the base): issues #942, #944, #947 and #948, whose invariants AC4 preserves.
- Base: origin/main at `94287369908cc920b21b0e3256314f988ad7d2f5` (BASE-SHA). The branch `bug/engine-toggle-coordinator-947-review-residuals-964` was cut from it. Every `git diff` and `git show` in this plan uses that literal as its ref operand.

## AC identity table

| ID | Subject | Proved by |
|---|---|---|
| AC1 | Refusal-path notification guard, regression-first | `HandleToggleClickAsync_WhenNotifySinkThrowsWithNullEngines_DoesNotThrow` failing at P1-T14, passing at P1-T24 |
| AC2 | Notification failure reported once, nothing invoked; double throw contained | `HandleToggleClickAsync_WhenNotifySinkThrowsWithNullEngines_LogsSinkExceptionOnceAndInvokesNothing`, `HandleToggleClickAsync_WhenNotifyAndLogSinksThrowWithNullEngines_DoesNotThrow` |
| AC3 | One shared sink guard | P1-T22 stripped census; the four #947 tests in `.ThrowingSink.cs` passing unchanged |
| AC4 | Ordering invariants preserved across the split | P1-T8 and P1-T24 fixture runs; P1-T25 unchanged-partials gate; `GetPressed_WhenLogSinkThrowsOnFaultedPrime_SameFaultKindIsReportedAgain` |
| AC5 | `GetPrimeTask` documentation accuracy | P1-T22 phrase census |
| AC6 | File-size split | P1-T26 line counts and csproj registration; P1-T7 census; builds at P1-T8, P1-T23, P2-T3 and P2-T4 |
| AC7 | Comment drift in touched files | P1-T22 phrase census |
| AC8 | Toolchain and coverage | Phase 2 loop, P2-T6 comparison |

## Verified tree facts (re-derived in this worktree for version 1.0; Phase 0 re-checks each one)

1. `.git` of the worktree names the branch `bug/engine-toggle-coordinator-947-review-residuals-964`; that branch ref and `refs/remotes/origin/main` both resolve to BASE-SHA, so HEAD equals the base when this plan was authored.
2. `TaskMaster/Ribbon/EngineToggleStateCoordinator.cs` is 496 lines (CRLF in the working tree). Usings at 1 to 6: `System`, `System.Collections.Concurrent`, `System.Globalization`, `System.Threading`, `System.Threading.Tasks`, `UtilitiesCS`. Line 45 `internal sealed class EngineToggleStateCoordinator`; 46 `{`; 47 to 51 the `NullEngineNameToken` summary and constant; 52 blank; 53 `_enginesAccessor`; constructor parameter docs `enginesAccessor` 95 to 99, `notifyUnavailable` 104 to 107, `logError` 108 to 111; `GetPressed` returns 134 to 138; `HandleToggleClickAsync` summary 162 to 165 (`The other two are sink guards` at 164), remarks 171 to 181, signature 182, refusal call `_notifyUnavailable(BuildUnavailableMessage(engineName));` at 186, click-boundary sink guard 194 to 204; `ExecuteToggleAsync` closing brace 255; 256 blank; `GetPrimeTask` summary 257 to 260 (`The in-flight — or most recently completed — prime` at 258), returns 262 to 269 (`The prime task, or` at 263), signature 270; marker registration 303 to 306; `StartObservedPrime` remarks 314 to 324 (`The three` at 315), `marker.SetResult(true)` in `finally` at 340 to 343; `CompletePrime` summary 373 to 378, remarks 379 to 404, signature 405, guarded report 421 to 432 with `_reportedPrimeFaults[reportKey] = 0;` at 426 directly after the sink call inside the `try`, `TryRemove` 434, closing brace 435; 436 blank; `RenderEngineName` summary 437, signature 440; `BuildUnmappedKeyMessage` closing brace 494; 495 `    }`; 496 `}`. Code `catch` lines: 194, 200, 428.
3. `TaskMaster/Ribbon/RibbonCommandBoundary.cs` `ReportFailure` 80 to 94 (presentation failure forwarded to `SafeLog`) and `SafeLog` 103 to 113 (log failure discarded): the pattern AC2 adopts.
4. `TaskMaster/Ribbon/RibbonController.EngineCommands.cs` constructs the coordinator at 67 to 77 and forwards at 103 to 106; its returns text (98 to 102, "The returned task never faults") becomes true on every path after the fix; it is not edited.
5. `TaskMaster/TaskMaster.csproj` (legacy, explicit compile items, CRLF) carries `<Compile Include="Ribbon\EngineToggleStateCoordinator.cs" />` at 466 followed by `Ribbon\RibbonController.cs` at 467. `TaskMaster.Test/TaskMaster.Test.csproj` carries the six fixture entries at 352 and 359 to 363 (`.RepeatFaultSuppression.cs` at 363, followed by `EngineTogglePressedStateCacheTests.cs` at 364). An unlisted file is not compiled.
6. Fixture partials under `TaskMaster.Test/Ribbon/`: `EngineToggleStateCoordinatorTests.cs` 470 lines (`[TestClass]` 22, `private sealed class Harness` 403, notify sink `message => Notifications.Add(message),` at 414, `OnLogError` property 445, `Notifications` 449, `Errors` 451, `LoggedError` 457); `.Race.cs` 277 (stale remark 196 to 201: "logs a second error", false since #948 suppresses the repeat); `.PrimeFaultOrdering.cs` 77; `.PrimeRegistration.cs` 175; `.ThrowingSink.cs` 215; `.RepeatFaultSuppression.cs` 290. Census: 36 `[TestMethod]`, 1 `[DataTestMethod]`, 3 `[DataRow(`, so 39 executed cases (the #948 pass-after run observed `COUNTERS total=39`). The harness has no notification hook.
7. `UtilitiesCS/Interfaces/IGlobals/IAppItemEngines.cs` declares `IAppItemEngines` in namespace `UtilitiesCS` (line 5). `TaskMaster/TaskMaster.csproj` sets `LangVersion` `preview` (31) and generates no documentation file, so `cref` targets are not compiled into diagnostics. `.editorconfig` raises only `MSTEST0032` to warning (29); analyzer rules are suggestions.
8. `scripts/vscode/Invoke-MSTestWithCoverage.ps1`: parameters `SearchRoot`, `Configuration`, `CoverageOutput` (default `coverage\coverage.cobertura.xml`, 9); the inner vstest arguments hard-code `/TestCaseFilter:TestCategory!=LiveOutlook` (91), the results directory and the trx name (92, 93; fixed values `coverage\test-results` and `mstest-coverage-run.trx` at 297 and 298); a non-zero collector exit throws `MSTest with coverage failed with exit code N` (262) before post-processing, so the raw document and the trx stay on disk and no summary or projection is written; assembly discovery excludes `(^|\\)\.claude\\` relative to the search root (353), so the runner discovers this worktree's own test assemblies; on success it prints the `First-party coverage:` line (410), writes the JaCoCo projection beside the output (415 to 419), writes `coverage\test-results\mstest-coverage-run.summary.txt` (440 to 447) and retains the raw document because it sits in the repository coverage directory (449 to 453, `Test-RawCoverageDocumentRetained`); the entry guard at 459 makes the file safe to dot-source.
9. `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1` defines `Get-CoberturaClassLineSummary -ClassNode` (160) and `Merge-CoberturaClassesByFilename` (260, groups class elements by `filename`), applied by `ConvertTo-KoverageCoberturaXml` (407, 442); after post-processing each source file is one class node (the #948 final run read `COORD-CLASS-NODES: 1` for the then single-file coordinator). `Invoke-MSTestWithCoverage.FirstParty.ps1` `Get-CoberturaFirstPartyCoverageReport` (123) renders `First-party coverage: lines a/b (x%), branches c/d (y%)` (117 to 120). `Invoke-MSTest.TrxSummary.ps1` `Format-TrxRunSummary` (103) renders five lines beginning `Test run outcome:`.
10. `scripts/vscode/Install-RepoDotNetSdk.ps1` installs SDK 8.0.205 to `.dotnet-sdk` with marker `.dotnet-sdk\sdk\8.0.205` (3, 56); `scripts/vscode/Invoke-Restore.ps1` exists; `global.json` pins 8.0.205 with paths `.dotnet-sdk` and the host. Neither `.dotnet-sdk\sdk` nor `packages\` exists in this worktree, so bootstrap is required. `.gitignore` ignores `*.trx` (146), `*cobertura*.xml` (147), `coverage/*` (150, `.gitkeep` re-included at 151), `**/[Pp]ackages/*` (197) and `.dotnet*/` (357). `.csharpierignore` excludes `**/evidence/**` (4), raw coverage and trx names (5 to 8), project files (12 to 14), `packages.config` (16) and `app.config` (18).
11. Observed outputs from the executed #947 and #948 runs on this machine (their evidence folders, 2026-10-01): the stall probe over the four UtilitiesCS.Test shell-icon classes exited 1 with one failed test (`GetFileIcon_WithUseFileType_ShouldReturnIconsForDirectoryAndFileExtension`, Win32 icon-handle `ArgumentException`), not a hang; the coordinator fixture fail-before message of a FluentAssertions `NotThrowAsync` assertion read `Did not expect any exception because <reason>, but found System.InvalidOperationException: sink failed / at ...`; an exception escaping a test method read `Test method <fully qualified name> threw exception: / <type>: <message>`; `dotnet tool run csharpier check .` printed `Checked N files in <ms>ms.` and named only unformatted paths; `First-party coverage: lines 56204/65855 (85.35%), branches 13618/17078 (79.74%)` and `COORD-LINES covered=177 valid=177` at the #948 final.
12. The four UtilitiesCS.Test shell-icon classes are `UtilitiesCS.Test.HelperClasses.ShellUtilities_Tests`, `UtilitiesCS.Test.HelperClasses.ShellUtilitiesStatic_Tests`, `UtilitiesCS.Test.HelperClasses.SysImageListHelperTests` and `UtilitiesCS.Test.EmailIntelligence.OSBrowser_Tests`; CI runs them unfiltered.
13. `docs/features/potential/promoted/2026-10-01-engine-toggle-coordinator-947-review-residuals.md` is the promotion record for #964; this plan never edits it.
14. (re-derived in version 1.1) `scripts/vscode/Invoke-MSTest.TrxSummary.ps1` `Get-TrxRunSummary` builds `FailedTestName` from the `testName` attribute of each failed result node (74 to 85), which is the short method name, so two failed tests of the same method name in different classes are indistinguishable in `FAILED-SET:`. `CMD-COVERAGE-POST` therefore resolves each failed result's `testId` against `TestDefinitions/UnitTest/TestMethod` (`className` up to its first comma, a dot, then `name`) and prints `FAILED-FQN` rows; every pre-existing-failure comparison in this plan uses those rows. The runner's trx name and directory are the fixed values at `Invoke-MSTestWithCoverage.ps1` 297 and 298, copied by `CMD-COVERAGE-RUNNER` to `coverage\STAGE-964.trx`. Every fixture partial declares `namespace TaskMaster.Test.Ribbon` and `public partial class EngineToggleStateCoordinatorTests` (primary file 9 and 23 with `[TestClass]` at 22; ThrowingSink 7 and 19), and delivered source T2 uses the same namespace and class, so every fixture test's fully qualified name begins `TaskMaster.Test.Ribbon.EngineToggleStateCoordinatorTests.`.

## Design decisions (do not redesign)

- **D-1 Split first, behaviour-preserving (AC6).** `EngineToggleStateCoordinator` becomes `internal sealed partial class EngineToggleStateCoordinator` across three files: `EngineToggleStateCoordinator.cs` (class documentation, fields, constructor, `GetPressed`, `HandleToggleClickAsync`, `ExecuteToggleAsync`, and after the fix `TryInvokeSink`), `EngineToggleStateCoordinator.Prime.cs` (`GetPrimeTask`, `StartPrimeIfNeeded`, `StartObservedPrime`, `ApplyPrimeAsync`, `CompletePrime`) and `EngineToggleStateCoordinator.Messages.cs` (`NullEngineNameToken`, `RenderEngineName` and the `Build*Message` helpers). All fields stay in the main file (`_pressedState` is shared by the toggle and prime paths). The split is a pure move: base lines are copied verbatim, only the class keyword gains `partial`, and each file carries exactly the usings its members need. It lands in P1-T2 to P1-T8, before the regression tests are written, and P1-T8 proves the unchanged fixture still passes against it, so the fail-before run at P1-T14 compiles against the split but unfixed coordinator. The split is verified by a multiset census of non-blank trimmed lines against BASE-SHA (P1-T7) whose only admitted differences are the declaration keyword, the two new namespace and class scaffolds and the redistributed usings.
- **D-2 One guard helper (AC3).** `private static bool TryInvokeSink(Action sinkCall, out Exception sinkFailure)` in the main file holds the only sink `catch` in the type: it returns `true` when the sink returned normally and `false` with the exception when it threw, and never rethrows. All four sink invocations route through it: the refusal-path `notifyUnavailable`, the refusal-path `logError` that reports a notification failure, the click-boundary `logError` and the prime-fault `logError` in `CompletePrime`. Afterwards the coordinator source carries exactly two `catch` clauses: the click boundary (which observes an engine fault) and the guard.
- **D-3 #948 invariant inside the guard (AC4).** In `CompletePrime` the record `_reportedPrimeFaults[reportKey] = 0;` is the only statement of the branch taken when `TryInvokeSink` returns `true`, so it executes only when the sink returned normally and directly after it; a throwing sink leaves the fault kind unrecorded and the report owed. It is never placed before the sink call, in an `else` branch, or after the branch.
- **D-4 Refusal path (AC1, AC2).** When the engines accessor yields null the notification goes through the guard; when the guard returns `false`, the captured exception is reported once through the guarded `logError` with the new message `BuildNotifyFailedMessage`; the method returns without invoking any engine member or invalidating any control, and never throws. This follows `RibbonCommandBoundary.ReportFailure`/`SafeLog` (fact 3).
- **D-5 Documentation (AC5, AC7).** `GetPrimeTask` describes the registration marker; the `HandleToggleClickAsync` summary and remarks, the `StartObservedPrime` remarks and the `CompletePrime` summary and remarks match the two-`catch` structure; the constructor `enginesAccessor`, `notifyUnavailable` and `logError` parameter docs state the guarded behaviour and the accessor's non-throwing precondition; the `GetPressed` returns sentence states the same precondition.
- **D-6 Tests (AC1, AC2, AC4).** A new partial `TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.SinkGuard.cs` holds four tests whose names are fixed here (NEW-NAMES-964). Three carry the fail-before obligation; the fourth (`GetPressed_WhenLogSinkThrowsOnFaultedPrime_SameFaultKindIsReportedAgain`) closes a coverage gap found in planning: no existing test asserts that a throwing sink leaves the fault kind unrecorded, which is exactly the record placement this change rewrites. It passes before and after the fix and fails if the record moves ahead of the sink call or out of the success branch. The primary fixture's `Harness` gains one member, `OnNotify`, invoked after the notification is recorded (mirroring `OnLogError`), so a throwing hook both records the attempt and models a throwing sink; no assertion in any existing partial changes.
- **D-7 Folded-in related defects (maintainer directive 2026-10-02).** (a) the `.Race.cs` remark at 196 to 201 claims the re-prime "logs a second error", which #948 made false; the remark is reworded, no code changes. (b) the `GetPressed` returns sentence and the `logError` parameter doc (D-5). (c) the missing throwing-sink-leaves-report-owed test (D-6). No completely unrelated defect was found.
- **D-8 Coverage route and AC8 test-step rule.** Step 4 runs `scripts/vscode/Invoke-MSTestWithCoverage.ps1` verbatim (by absolute path from the worktree cwd, fact 8). The runner's exit code is recorded. AC8's own qualifier ("no new failing test relative to baseline") defines the test-step pass condition: the step passes when the runner exits 0, or when it exits non-zero with `MSTest with coverage failed with exit code` and the trx-derived set of fully qualified failed names (`FAILED-FQN` rows of `CMD-COVERAGE-POST`, each the `TestMethod` `className` up to its first comma, a dot, and the `TestMethod` `name`) is non-empty, `TEST-DEFINITIONS:` is at least 1, no value begins with `UNRESOLVED:`, every value appears verbatim as a `FAILED-FQN` row of `FEATURE/evidence/baseline/coverage-baseline.md`, and no value begins with `TaskMaster.Test.Ribbon.EngineToggleStateCoordinatorTests.`. When the runner exits non-zero it writes no summary or projection (fact 8), so `CMD-COVERAGE-POST` post-processes the raw document with the runner's own helpers (`RAW True`); when it exits 0, the document is already post-processed (`RAW False`). Both floors (80% line, 75% branch, applied by the runner's threshold functions) must be met in every case. A run that hangs is bounded by wall clock and stops the plan; no alternative route is taken.
- **D-9 Coordinator coverage figure (AC8).** The coordinator figure is the sum, over every class node whose normalised `filename` ends with one of the three coordinator source paths, of `Get-CoberturaClassLineSummary` covered and valid lines; the rate is 100 times covered over valid, rounded to two decimals. Baseline has one file; final has three. Final rate must be at least the baseline rate. Per-method rows for `HandleToggleClickAsync`, `CompletePrime`, `TryInvokeSink` and `BuildNotifyFailedMessage` are read from the node of the file that contains the method: the two new methods must reach at least 90.00 (CLAUDE.md new-code target) and the two changed methods must have no more uncovered elements than at baseline. At final, each of the three files must have at least one class node; otherwise `PARTIAL CLASS ATTRIBUTION UNSUPPORTED`: stop and report, because the per-file figures would then be unmeasurable.
- **D-10 No commits.** This plan creates no commit and stages nothing. Every footprint gate compares the working tree against BASE-SHA with `git diff --name-only BASE-SHA` paired with `git status --porcelain --untracked-files=all` in the same task. `.claude/agent-memory/` paths are ambient state of other sessions, are never staged, and are admitted by the footprint gate. Committing is the orchestrator's step after the reduced audit.
- **D-11 Edit route.** Every `.cs` and `.csproj` change is made with the Edit or Write tool, never through a shell write, so the repository hooks see it. A hook denial of any Edit or Write is reported verbatim and stops that step; the executor does not retry with a rephrased edit or another tool.

## Write Set (every file this plan creates or modifies)

Code files (the only paths outside the feature folder this plan may change):

- `TaskMaster/Ribbon/EngineToggleStateCoordinator.cs` (modify: split, then fix)
- `TaskMaster/Ribbon/EngineToggleStateCoordinator.Prime.cs` (create)
- `TaskMaster/Ribbon/EngineToggleStateCoordinator.Messages.cs` (create)
- `TaskMaster/TaskMaster.csproj` (modify: two compile items)
- `TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.cs` (modify: the `OnNotify` harness member)
- `TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.SinkGuard.cs` (create)
- `TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.Race.cs` (modify: remark only, D-7a)
- `TaskMaster.Test/TaskMaster.Test.csproj` (modify: one compile item)

Feature documents:

- `docs/features/active/2026-10-01-engine-toggle-coordinator-947-review-residuals-964/issue.md` (check-off edits `- [ ] ACn` to `- [x] ACn` only)
- `docs/features/active/2026-10-01-engine-toggle-coordinator-947-review-residuals-964/plan.2026-10-02T05-20.md` (task check-off edits only)

Evidence files (fixed names; the write time is the `Timestamp:` field), all under FEATURE/evidence/:

- `baseline/`: `phase0-instructions-read.md`, `scope-and-anchor.md`, `anchor-production.md`, `anchor-test-side.md`, `bootstrap-sdk.md`, `bootstrap-tool-restore.md`, `bootstrap-nuget-restore.md`, `bootstrap-dotnet-coverage.md`, `csharpier-check-baseline.md`, `msbuild-analyzer-baseline.md`, `msbuild-nullable-baseline.md`, `coordinator-tests-baseline.md`, `coverage-baseline.md` (thirteen)
- `regression-testing/`: `split-census.md`, `split-fixture-green.md`, `sink-guard-partial-tokens.md`, `refusal-path-fail-before.md`, `refusal-path-pass-after.md` (five)
- `qa-gates/`: `production-edit-scope.md`, `test-partials-unchanged.md`, `file-line-counts.md`, `csharpier-format.md`, `csharpier-check-final.md`, `msbuild-analyzer-final.md`, `msbuild-nullable-final.md`, `coverage-final.md`, `coverage-comparison.md`, `toolchain-final-pass.md`, `footprint-scope.md`, `evidence-hygiene.md` (twelve)
- `other/`: `implementation-handoff.md`, `ac-status-summary.md`, `reduced-audit-handoff.md` (three)
- Preparation-phase record (committed by the preparation run before execution; not written, modified or deleted by any task of this plan): exactly one file `other/preflight-clearance.<yyyy-MM-ddTHH-mm>.md`.

Files this plan must not touch: every other file under `TaskMaster/` and `TaskMaster.Test/` (in particular `TaskMaster/Ribbon/RibbonController.EngineCommands.cs`, `TaskMaster/Ribbon/RibbonCommandBoundary.cs`, `TaskMaster/Ribbon/EngineTogglePressedStateCache.cs` and the partials `.PrimeFaultOrdering.cs`, `.PrimeRegistration.cs`, `.ThrowingSink.cs`, `.RepeatFaultSuppression.cs`), every `packages.config`, every file under `scripts/`, `.claude/`, `config/` and `artifacts/`, `docs/features/potential/`, `TaskMaster.runsettings`, `coverage.config`, `.editorconfig`, `.gitignore` and `.csharpierignore`. No raw trx, raw Cobertura document or msbuild log is copied into the feature folder under any name; raw documents stay under the git-ignored `coverage/` directory.

## Delivered source (the executor writes these texts; CSharpier output wins on layout, and every gate reads whitespace-normalised text)

Indentation rule: every block below is shown with four leading spaces of Markdown indent on each line; remove exactly those four spaces on every line when writing. Base line numbers refer to `TaskMaster/Ribbon/EngineToggleStateCoordinator.cs` at BASE-SHA (fact 2), which P0-T4 proves equal to the working file before Phase 1.

**S1 — `TaskMaster/Ribbon/EngineToggleStateCoordinator.Messages.cs` (create).** Content: the header block, then base lines 47 to 51 verbatim, one blank line, base lines 437 to 494 verbatim, then the footer block.

Header:

    using System;
    using System.Globalization;

    namespace TaskMaster
    {
        internal sealed partial class EngineToggleStateCoordinator
        {

Footer:

        }
    }

**S2 — `TaskMaster/Ribbon/EngineToggleStateCoordinator.Prime.cs` (create).** Content: the header block, then base lines 257 to 435 verbatim, then the S1 footer block.

Header:

    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using UtilitiesCS;

    namespace TaskMaster
    {
        internal sealed partial class EngineToggleStateCoordinator
        {

**S3 — `TaskMaster/Ribbon/EngineToggleStateCoordinator.cs` (modify).** Remove base lines 3 (`using System.Globalization;`) and 4 (`using System.Threading;`); replace line 45 with `    internal sealed partial class EngineToggleStateCoordinator`; remove base lines 47 to 52 (the constant block and the blank line after it); remove base lines 256 to 494 (the blank line after `ExecuteToggleAsync`, the moved prime members, the blank line 436 and the moved message members). The result is base lines 1, 2, 5 to 44, the new line 45, 46, 53 to 255, 495 and 496: 249 lines.

**F1 — constructor parameter docs (main file).** Replace the `enginesAccessor` element (base 95 to 99), the `notifyUnavailable` element (base 104 to 107) and the `logError` element (base 108 to 111) with, respectively:

            /// <param name="enginesAccessor">
            /// Supplies the current engines container. Must not be null, and must not throw: its
            /// result is read outside any guard by <see cref="GetPressed"/> and by the refusal check
            /// of <see cref="HandleToggleClickAsync"/>, so an exception it raised would escape both.
            /// It is expected to return null before the ribbon controller has been given its globals,
            /// which this type treats as "state unknown" rather than as an error.
            /// </param>

            /// <param name="notifyUnavailable">
            /// Receives exactly one message per toggle click refused because the engines are not
            /// available. Presentation is the sink's concern. Must not be null. The call is guarded
            /// (issue #964): an exception it throws is reported once through
            /// <paramref name="logError"/> and is not rethrown.
            /// </param>

            /// <param name="logError">
            /// Receives an observed prime fault, toggle fault or notification failure as a message
            /// plus the exception. Must not be null. The call is guarded: an exception it throws is
            /// discarded, because no further reporting channel remains.
            /// </param>

**F2 — `GetPressed` returns (main file).** Replace base 134 to 138 with:

            /// <returns>
            /// The cached activation state, or <see langword="false"/> when the key is null,
            /// whitespace, unmapped, or has never been primed. This method performs a dictionary read
            /// only: it never awaits, never blocks, and never throws while the engines accessor
            /// honours its non-throwing precondition.
            /// </returns>

**F3 — `HandleToggleClickAsync` (main file).** Replace the summary (base 162 to 165) with SUMMARY-HTC, the remarks (base 171 to 181) with REMARKS-HTC, and the method body (base 183 to 205, the opening brace through the closing brace) with BODY-HTC.

SUMMARY-HTC:

            /// <summary>
            /// The toggle-click boundary: the only <c>catch</c> clause in this type that observes an
            /// engine fault. Every sink call on this path goes through <see cref="TryInvokeSink"/>,
            /// which holds the only other <c>catch</c> clause.
            /// </summary>

REMARKS-HTC:

            /// <remarks>
            /// When the engines are not available the click is refused with exactly one
            /// <c>notifyUnavailable</c> message and no engine member is invoked. That notification is
            /// guarded (issue #964): if the sink throws, its exception is reported once through
            /// <c>logError</c>. Otherwise <see cref="ExecuteToggleAsync"/> runs inside a single
            /// boundary <c>try</c>/<c>catch</c>: a fault is reported through <c>logError</c>, is not
            /// rethrown, and does not invalidate. Every <c>logError</c> call is itself guarded
            /// (issue #947): it is the last reporting channel, so a failure inside it has nowhere
            /// else to go and is discarded deliberately, following
            /// <c>RibbonCommandBoundary.SafeLog</c>. This method therefore never throws on either
            /// path, even when both sinks throw, provided the engines accessor honours its
            /// non-throwing precondition, because its caller is an <c>async void</c> Office handler
            /// whose faults would otherwise become unobserved.
            /// </remarks>

BODY-HTC:

            {
                if (_enginesAccessor() is null)
                {
                    if (
                        !TryInvokeSink(
                            () => _notifyUnavailable(BuildUnavailableMessage(engineName)),
                            out var notifyFailure
                        )
                    )
                    {
                        _ = TryInvokeSink(
                            () => _logError(BuildNotifyFailedMessage(engineName), notifyFailure),
                            out _
                        );
                    }

                    return;
                }

                try
                {
                    await ExecuteToggleAsync(engineName).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    _ = TryInvokeSink(() => _logError(BuildToggleFailedMessage(engineName), ex), out _);
                }
            }

**F4 — `TryInvokeSink` (main file, new).** Insert directly after the closing brace of `ExecuteToggleAsync` (the last method of the main file), preceded by one blank line:

            /// <summary>
            /// Invokes one injected sink and contains any exception it throws (issues #947 and #964).
            /// This holds the only <c>catch</c> clause in this type that intercepts a sink failure;
            /// every <c>notifyUnavailable</c> and <c>logError</c> call goes through it.
            /// </summary>
            /// <param name="sinkCall">The sink invocation, with its arguments already bound.</param>
            /// <param name="sinkFailure">
            /// The exception the sink threw, or <see langword="null"/> when the sink returned
            /// normally.
            /// </param>
            /// <returns>
            /// <see langword="true"/> when the sink returned normally; <see langword="false"/> when
            /// it threw. The exception is never rethrown.
            /// </returns>
            /// <remarks>
            /// The caller decides what happens to a contained failure, following
            /// <c>RibbonCommandBoundary.ReportFailure</c>: the refusal path of
            /// <see cref="HandleToggleClickAsync"/> forwards a notification failure to the log sink,
            /// and every log-sink caller discards a log failure because no further channel remains.
            /// <see cref="CompletePrime"/> records a reported fault kind only when this method
            /// returns <see langword="true"/>, so a sink that throws leaves the report owed
            /// (issue #948).
            /// </remarks>
            private static bool TryInvokeSink(Action sinkCall, out Exception sinkFailure)
            {
                try
                {
                    sinkCall();
                    sinkFailure = null;
                    return true;
                }
                catch (Exception ex)
                {
                    sinkFailure = ex;
                    return false;
                }
            }

**F5 — `GetPrimeTask` documentation (Prime file).** Replace the lines from the `/// <summary>` directly above the line containing `The in-flight` through the `/// </returns>` directly above `internal Task GetPrimeTask(string engineName)` with:

            /// <summary>
            /// The registration marker for an engine key, exposed so tests can await the outcome of
            /// its prime deterministically instead of polling or sleeping. The marker is not the
            /// prime task itself: it is registered before the prime starts and is completed only after
            /// the prime outcome has been observed and, on a fault or cancellation, reported.
            /// </summary>
            /// <param name="engineName">The engine key; ordinal, case-sensitive.</param>
            /// <returns>
            /// The registered marker, or <see cref="Task.CompletedTask"/> when no marker is registered
            /// for the key. The marker never faults or cancels: a prime fault is observed by
            /// <see cref="CompletePrime"/> and reported through <c>logError</c>, and the marker is
            /// completed in a <c>finally</c> after that observation. For a key whose prime did not
            /// run to completion, the marker is cleared only after that report has returned or
            /// thrown, so a caller that receives <see cref="Task.CompletedTask"/> can rely on the
            /// report having been attempted or deliberately suppressed as a repeat of a kind already
            /// reported.
            /// </returns>

**F6 — `StartObservedPrime` remarks (Prime file).** Replace the nine remark text lines from the line containing `The observer is a continuation rather than a` through the line containing `of the sink, the discarded continuation has no remaining throw source of its own.` with:

            /// The observer is a continuation rather than a <c>catch</c> clause. The two
            /// <c>catch</c> clauses in this type are the click boundary in
            /// <see cref="HandleToggleClickAsync"/> and the single sink guard in
            /// <see cref="TryInvokeSink"/>. Reading <see cref="Task.Exception"/> inside
            /// <see cref="CompletePrime"/> marks the fault observed, so no unobserved task remains.
            /// The continuation task itself is discarded; the value a test awaits is the marker,
            /// which the continuation completes only through <c>SetResult</c> in a <c>finally</c>
            /// after <see cref="CompletePrime"/> exits, so it never faults or cancels. Because
            /// <see cref="CompletePrime"/> routes its sink call through <see cref="TryInvokeSink"/>,
            /// the discarded continuation has no remaining throw source of its own.

**F7 — `CompletePrime` (Prime file).** Three replacements inside the `CompletePrime` documentation and body.

F7a: replace the summary text line containing `sink failure is contained here, and only then is the marker cleared for a later re-prime.` with the two lines:

            /// sink failure is contained by <see cref="TryInvokeSink"/>, and only then is the marker
            /// cleared for a later re-prime.

F7b: replace the second remarks paragraph text (the seven lines from the line containing `The sink call is guarded (issue #947). The sink is the last reporting channel of this` through the line containing `completes rather than faulting.`) with the first block below, and the third paragraph text (the four lines from the line containing `Repeat suppression (issue #948)` through the line containing `that record before the sink, or into a catch or finally arm, suppresses it for the session.`) with the second block:

            /// The sink call is guarded (issue #947) through <see cref="TryInvokeSink"/>, the guard
            /// this type uses at every sink call site (issue #964). The sink is the last reporting
            /// channel of this type, so a failure inside it has nowhere else to go; letting it escape
            /// skipped the clear below, which left a stale marker that blocked every later re-prime,
            /// and faulted the discarded continuation unobserved. With the sink contained, the
            /// continuation in <see cref="StartObservedPrime"/> has no remaining throw source of its
            /// own, so it completes rather than faulting.

            /// Repeat suppression (issue #948): each pair of engine key and base-exception type is
            /// reported once, then recorded in <see cref="_reportedPrimeFaults"/> by the only
            /// statement of the branch taken when <see cref="TryInvokeSink"/> reports that the sink
            /// returned normally, so a sink that throws leaves the report owed. Moving that record
            /// before the sink call, or out of that branch, suppresses it for the session.

F7c: replace the guarded report (from the line `if (!_reportedPrimeFaults.ContainsKey(reportKey))` through its closing brace, base 421 to 432) with:

                if (!_reportedPrimeFaults.ContainsKey(reportKey))
                {
                    if (
                        TryInvokeSink(
                            () => _logError(BuildPrimeFailedMessage(engineName), failure),
                            out _
                        )
                    )
                    {
                        _reportedPrimeFaults[reportKey] = 0;
                    }
                }

The `// Report-then-clear is load-bearing:` comment and the final `_primeTasks.TryRemove(engineName, out _);` are unchanged.

**F8 — `BuildNotifyFailedMessage` (Messages file, new).** Insert directly after the closing brace of `BuildToggleFailedMessage`, preceded by one blank line:

            /// <summary>
            /// The message logged when the notification for a refused toggle click throws.
            /// </summary>
            private static string BuildNotifyFailedMessage(string engineName)
            {
                return string.Format(
                    CultureInfo.CurrentCulture,
                    "Notifying that the engine '{0}' is not available failed, so the refused toggle "
                        + "click was not surfaced to the user.",
                    RenderEngineName(engineName)
                );
            }

**T1 — `Harness` notification hook (`TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.cs`).** Replace the line `message => Notifications.Add(message),` (base 414) with the first block, and insert the second block (the property, then one blank line) directly after the line `internal Action<string, Exception> OnLogError { get; set; }` and the blank line that follows it:

                        message =>
                        {
                            Notifications.Add(message);
                            OnNotify?.Invoke(message);
                        },

                /// <summary>
                /// An optional extra observer invoked from inside the notification sink, immediately
                /// after the message has been appended to <see cref="Notifications"/>, so a throwing
                /// hook both records the attempt and models a throwing notification sink.
                /// </summary>
                internal Action<string> OnNotify { get; set; }

**T2 — `TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.SinkGuard.cs` (create).**

    using System;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    namespace TaskMaster.Test.Ribbon
    {
        /// <summary>
        /// Regression tests for issue #964: on the refusal path of <c>HandleToggleClickAsync</c>,
        /// taken when the engines accessor yields null, a <c>notifyUnavailable</c> sink that throws
        /// must not escape into the <c>async void</c> Office handler, and its exception must be
        /// reported once through <c>logError</c>; plus a guard for the issue #948 record placement
        /// now that every sink call goes through one shared guard. A further partial of the
        /// coordinator fixture, so the private <c>Harness</c> and <c>LoggedError</c> types and the
        /// fixture constants are reused. The harness invokes <c>OnNotify</c> and <c>OnLogError</c>
        /// after it has recorded the call, so a throwing hook both records the attempt and models a
        /// throwing sink. No test sleeps, polls, reads the clock or touches the filesystem.
        /// </summary>
        public partial class EngineToggleStateCoordinatorTests
        {
            #region Issue #964 — a throwing notification sink on the refusal path

            /// <summary>
            /// Regression for issue #964 and the test that carries the fail-before obligation.
            /// Invariant: with the engines unavailable, a throwing notification sink does not escape
            /// the click handler. Without the fix the exception escapes the unguarded notification
            /// call, so the awaited call faults with it.
            /// </summary>
            [TestMethod]
            public async Task HandleToggleClickAsync_WhenNotifySinkThrowsWithNullEngines_DoesNotThrow()
            {
                // Arrange: the pre-SetGlobals window, with a notification sink that throws.
                var harness = new Harness { EnginesAvailable = false };
                harness.OnNotify = _ => throw new InvalidOperationException("notify sink failed");

                // Act
                Func<Task> act = () => harness.Coordinator.HandleToggleClickAsync(SpamEngine);

                // Assert
                await act.Should()
                    .NotThrowAsync("a throwing notification sink must not escape the refusal path");
            }

            /// <summary>
            /// Regression for issue #964, the reporting guarantee: the notification is attempted
            /// once, its exception reaches the log sink once and unchanged, and the refused click
            /// still touches no engine member and invalidates no control. Without the fix the
            /// exception escapes, so the test method throws before any assertion runs.
            /// </summary>
            [TestMethod]
            public async Task HandleToggleClickAsync_WhenNotifySinkThrowsWithNullEngines_LogsSinkExceptionOnceAndInvokesNothing()
            {
                // Arrange
                var harness = new Harness { EnginesAvailable = false };
                var notifyFailure = new InvalidOperationException("notify sink failed");
                harness.OnNotify = _ => throw notifyFailure;

                // Act
                await harness.Coordinator.HandleToggleClickAsync(SpamEngine);

                // Assert
                harness
                    .Notifications.Should()
                    .ContainSingle("the notification sink is attempted exactly once");
                harness
                    .Errors.Should()
                    .ContainSingle("a notification failure is reported once through the log sink");
                harness
                    .Errors[0]
                    .Exception.Should()
                    .BeSameAs(notifyFailure, "the log sink receives the notification failure unchanged");
                harness.Errors[0].Message.Should().Contain(SpamEngine);
                harness.Engines.VerifyNoOtherCalls();
                harness.Invalidations.Should().BeEmpty("a refused click changes no state to display");
            }

            /// <summary>
            /// Regression for issue #964, both sinks failing: when the log sink also throws while it
            /// reports the notification failure, the click handler still completes without throwing,
            /// because no further reporting channel remains. Without the fix the notification
            /// exception escapes first.
            /// </summary>
            [TestMethod]
            public async Task HandleToggleClickAsync_WhenNotifyAndLogSinksThrowWithNullEngines_DoesNotThrow()
            {
                // Arrange
                var harness = new Harness { EnginesAvailable = false };
                var notifyFailure = new InvalidOperationException("notify sink failed");
                harness.OnNotify = _ => throw notifyFailure;
                harness.OnLogError = (_, _) => throw new InvalidOperationException("log sink failed");

                // Act
                Func<Task> act = () => harness.Coordinator.HandleToggleClickAsync(SpamEngine);

                // Assert
                await act.Should().NotThrowAsync("the refusal path contains a failure of both sinks");
                harness.Notifications.Should().ContainSingle("the notification is attempted once");
                harness
                    .Errors.Should()
                    .ContainSingle("the log sink is attempted once before it throws");
                harness
                    .Errors[0]
                    .Exception.Should()
                    .BeSameAs(notifyFailure, "the log sink receives the notification failure unchanged");
            }

            #endregion Issue #964 — a throwing notification sink on the refusal path

            #region Issue #964 — the issue #948 record placement under the shared guard

            /// <summary>
            /// Guard for the issue #948 invariant now that the prime-fault sink call goes through the
            /// shared guard: a log sink that throws while a faulted prime is reported leaves that
            /// failure kind unrecorded, so the next fault of the same kind is reported again. Passes
            /// before and after the issue #964 change; it fails if the record moves ahead of the sink
            /// call or out of the branch taken when the sink returned normally.
            /// </summary>
            [TestMethod]
            public async Task GetPressed_WhenLogSinkThrowsOnFaultedPrime_SameFaultKindIsReportedAgain()
            {
                // Arrange: two faulted primes of one kind; the sink throws on the first report only.
                var harness = new Harness();
                var firstProbe = new TaskCompletionSource<bool>();
                var secondProbe = new TaskCompletionSource<bool>();
                harness
                    .Engines.SetupSequence(x => x.EngineActiveAsync(SpamEngine))
                    .Returns(firstProbe.Task)
                    .Returns(secondProbe.Task);
                var reports = 0;
                harness.OnLogError = (_, _) =>
                {
                    reports++;
                    if (reports == 1)
                    {
                        throw new InvalidOperationException("log sink failed");
                    }
                };
                harness.Coordinator.GetPressed(SpamEngine);
                var firstPrime = harness.Coordinator.GetPrimeTask(SpamEngine);

                // Act
                firstProbe.SetException(new InvalidOperationException("configuration load failed"));
                await firstPrime;
                harness.Coordinator.GetPressed(SpamEngine);
                var secondPrime = harness.Coordinator.GetPrimeTask(SpamEngine);
                secondProbe.SetException(new InvalidOperationException("configuration load failed"));
                await secondPrime;

                // Assert
                secondPrime.Should().NotBeSameAs(firstPrime, "the later read registered a new prime");
                harness
                    .Errors.Should()
                    .HaveCount(
                        2,
                        "a report the throwing sink did not accept is still owed, so the repeat is reported"
                    );
                harness.Errors[1].Message.Should().Contain(SpamEngine);
            }

            #endregion Issue #964 — the issue #948 record placement under the shared guard
        }
    }

**T3 — `.Race.cs` remark (related defect D-7a).** Replace the six remark text lines from the line containing `Assertion order is load-bearing. The harness engines mock is strict and this test` through the line containing `deterministic.` with:

            /// Assertion order is load-bearing. The harness engines mock is strict and this test
            /// supplies one setup, so the re-prime triggered by the second read re-enters that same
            /// canceled task. Since issue #948 that second cancellation is a repeat of a kind already
            /// reported and is not logged, but the single-error assertion is still made before the
            /// re-prime so the test does not depend on the suppression rule. The marker-cleared
            /// conclusion is drawn from prime-handle identity, which is deterministic.

**C1 — `TaskMaster/TaskMaster.csproj`.** Insert directly after the line `<Compile Include="Ribbon\EngineToggleStateCoordinator.cs" />` the two lines below; after the four Markdown-indent spaces are removed each line keeps four leading spaces, matching its neighbours:

        <Compile Include="Ribbon\EngineToggleStateCoordinator.Messages.cs" />
        <Compile Include="Ribbon\EngineToggleStateCoordinator.Prime.cs" />

**C2 — `TaskMaster.Test/TaskMaster.Test.csproj`.** Insert directly after the line `<Compile Include="Ribbon\EngineToggleStateCoordinatorTests.RepeatFaultSuppression.cs" />` the line below (four leading spaces after the Markdown indent is removed):

        <Compile Include="Ribbon\EngineToggleStateCoordinatorTests.SinkGuard.cs" />

## Name lists

- `NEW-NAMES-964`: `HandleToggleClickAsync_WhenNotifySinkThrowsWithNullEngines_DoesNotThrow`, `HandleToggleClickAsync_WhenNotifySinkThrowsWithNullEngines_LogsSinkExceptionOnceAndInvokesNothing`, `HandleToggleClickAsync_WhenNotifyAndLogSinksThrowWithNullEngines_DoesNotThrow`, `GetPressed_WhenLogSinkThrowsOnFaultedPrime_SameFaultKindIsReportedAgain`. The first three are `FAIL-BEFORE-NAMES`; the fourth is `GUARD-NAME`.
- `INVARIANT-NAMES` (existing tests AC4 names): `GetPressed_WhenPrimeStarts_RegistersPrimeHandleBeforeActivationReadRuns`, `GetPressed_AfterPrimeFaultsSynchronously_LaterReadStartsNewPrime`, `GetPressed_AfterPrimeIsCanceledSynchronously_LaterReadStartsNewPrime`, `GetPressed_WhenPrimeFaults_PrimeHandleStaysRegisteredUntilFaultIsLogged`, `GetPressed_WhenLogSinkThrowsOnFaultedPrime_LaterReadStartsNewPrime`, `GetPressed_WhenLogSinkThrowsOnCanceledPrime_LaterReadStartsNewPrime`, `GetPressed_WhenLogSinkThrows_FirstPrimeCompletesAndMarkerIsCleared`, `HandleToggleClickAsync_WhenLogSinkThrowsOnToggleFault_DoesNotThrowAndAttemptsReport`, `GetPressed_WhenPrimeFaultsRepeatedly_LogsFirstFaultOnly`, `GetPressed_WhenPrimeIsCanceledRepeatedly_LogsFirstCancellationOnly`, `HandleToggleClickAsync_WithNullEngines_NotifiesOnceAndInvokesNothing`.
- `NAMES-ALL` is `NEW-NAMES-964` followed by `INVARIANT-NAMES`, written as a PowerShell list of double-quoted strings when substituted into `CMD-VSTEST`.

## Execution conventions

- **Paths.** `WORKTREE` denotes the absolute item worktree path from the delegation prompt; it is substituted into each payload's first line and is never written into an artifact. Every artifact records repository-relative paths; absolute paths in transcribed tool output are replaced by `REDACTED-PATH`.
- **Payload channel.** Each indented payload block is executed as one `pwsh -NoProfile -Command '<payload>'` Bash invocation (the only permitted shell form besides `git`): outer single quotes, inner double quotes only; a double quote needed inside a payload string is built from `[char]34` and an apostrophe from `[char]39`; no payload carries a backslash-escaped double quote, a literal apostrophe or a backtick. No payload combines git with the substrings add, commit or remove, and no payload contains all of gh, pr and create, because the hook command scanners match those by case-insensitive containment. The `Command:` field of an artifact records the canonical command the payload runs, not the payload text.
- **Git commands outside payloads** run as `git -C WORKTREE <arguments>`; the `Command:` field records them without `-C`.
- **Toolchain commands.** Every `dotnet`, `msbuild` and `vstest.console.exe` command named in a task runs inside one `pwsh -NoProfile -Command` payload that begins with PRELUDE, because the first token of a permitted Bash command must be `git`, `pwsh` or `poetry`. A task that names only the tool command (for example `dotnet tool run csharpier check` over three paths) means that command wrapped as `pwsh -NoProfile -Command 'Set-Location -LiteralPath "WORKTREE"; [Environment]::CurrentDirectory = (Get-Location).Path; <command>; "EXIT: $LASTEXITCODE"'`.
- **Grep tool patterns.** Every pattern this plan gives to the Grep tool is a regular expression with its metacharacters escaped as written: `\(`, `\)`, `\[`, `\]`, `\.`, `\?`, and a literal backslash as `\x5C`. A pattern written without any metacharacter is a plain phrase. The Grep tool counts matching lines.
- **PRELUDE** (the first two lines of every payload):

        Set-Location -LiteralPath "WORKTREE"
        [Environment]::CurrentDirectory = (Get-Location).Path

- **TOOLS** (the lines of every build and test payload after PRELUDE):

        $vswhere = Join-Path ${env:ProgramFiles(x86)} "Microsoft Visual Studio\Installer\vswhere.exe"
        $msbuild = & $vswhere -latest -products * -find "MSBuild\**\Bin\MSBuild.exe" | Select-Object -First 1
        $vstest = & $vswhere -latest -products * -find "Common7\IDE\Extensions\TestPlatform\vstest.console.exe" | Select-Object -First 1
        $sln = Join-Path (Get-Location).Path "TaskMaster.sln"
        New-Item -ItemType Directory -Path "coverage\logs" -Force | Out-Null

- **Exit codes.** `EXIT_CODE:` records the printed exit value of the principal command. A deliberately or admissibly failing run carries `ExpectedExitCode:` equal to the observed non-zero value in its own artifact.
- **Long runs.** `CMD-COVERAGE-RUNNER` is started as a background process with output redirected to `coverage\logs\STAGE-964.payload.log`; completion is detected by the final line `PAYLOAD-COMPLETE`. Before every vstest or coverage run the executor runs `pwsh -NoProfile -Command '"STRAY_TEST_PROCESSES: " + @(Get-Process -ErrorAction SilentlyContinue | Where-Object { $_.ProcessName -like "vstest*" -or $_.ProcessName -like "testhost*" -or $_.ProcessName -like "dotnet-coverage*" }).Count'` and proceeds only on `STRAY_TEST_PROCESSES: 0`. A coverage run still in progress after 120 minutes is `COVERAGE RUN STALLED`: stop and report; it is never re-run with a different route.
- **Stop rule.** A named stop condition halts the plan; the executor reports the artifact and the failing values and does not work around it.

## Command reference

**CMD-REBUILD** (`GATEARGS` is `/p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` or `/p:TreatWarningsAsErrors=true`; `TASKID` substituted; `Command:` records `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" GATEARGS` resolved through vswhere with the worktree solution by absolute path; never `/t:Build`, never `/p:Nullable=enable`):

    PRELUDE
    TOOLS
    $log = "coverage\logs\TASKID.msbuild.log"
    if (Test-Path -LiteralPath $log) { Remove-Item -LiteralPath $log -Force }
    $global:LASTEXITCODE = 0
    & $msbuild $sln /t:Rebuild /m /nodeReuse:false /p:Configuration=Debug "/p:Platform=Any CPU" GATEARGS "/flp:LogFile=$log;Verbosity=normal" | Out-Null
    Write-Output ("MSBUILD_EXIT_CODE: " + $LASTEXITCODE)
    $lines = Get-Content -LiteralPath $log -Encoding UTF8
    Write-Output ("ERRORS: " + (($lines | Select-String -Pattern "^\s*(\d+) Error\(s\)" | Select-Object -Last 1).Matches[0].Groups[1].Value))
    Write-Output ("WARNINGS: " + (($lines | Select-String -Pattern "^\s*(\d+) Warning\(s\)" | Select-Object -Last 1).Matches[0].Groups[1].Value))
    Write-Output ("CSC_OUT_TASKMASTER: " + @($lines | Where-Object { $_.Contains("/out:obj\Debug\TaskMaster.dll") }).Count)
    Write-Output ("CSC_OUT_TASKMASTER_TEST: " + @($lines | Where-Object { $_.Contains("/out:obj\Debug\TaskMaster.Test.dll") }).Count)
    Write-Output ("WRITESET_DIAGNOSTIC_LINES: " + @($lines | Where-Object { ($_.Contains("EngineToggleStateCoordinator")) -and ($_ -match "(error|warning) [A-Z]+\d+") }).Count)
    Write-Output ("TEST_DLL_EXISTS: " + (Test-Path -LiteralPath "TaskMaster.Test\bin\Debug\TaskMaster.Test.dll"))

`ERRORS:` is read from the summary line, so `0 Error(s)` is never matched inside a larger count. The two `CSC_OUT_` counts are the observation that the compiler ran for the two Write Set projects. `WRITESET_DIAGNOSTIC_LINES` counts every error or warning line naming any Write Set source file, because every one contains `EngineToggleStateCoordinator`.

**CMD-BUILD** (incremental build so a scoped test run observes a fresh assembly; `TASKID` substituted; `Command:` records `msbuild TaskMaster.sln /t:Build /m /p:Configuration=Debug "/p:Platform=Any CPU"`; never used as a toolchain gate):

    PRELUDE
    TOOLS
    $log = "coverage\logs\TASKID.msbuild.log"
    if (Test-Path -LiteralPath $log) { Remove-Item -LiteralPath $log -Force }
    $before = (Get-Item -LiteralPath "TaskMaster.Test\bin\Debug\TaskMaster.Test.dll" -ErrorAction SilentlyContinue).LastWriteTimeUtc
    $global:LASTEXITCODE = 0
    & $msbuild $sln /t:Build /m /nodeReuse:false /p:Configuration=Debug "/p:Platform=Any CPU" "/flp:LogFile=$log;Verbosity=normal" | Out-Null
    Write-Output ("MSBUILD_EXIT_CODE: " + $LASTEXITCODE)
    $lines = Get-Content -LiteralPath $log -Encoding UTF8
    Write-Output ("ERRORS: " + (($lines | Select-String -Pattern "^\s*(\d+) Error\(s\)" | Select-Object -Last 1).Matches[0].Groups[1].Value))
    $after = (Get-Item -LiteralPath "TaskMaster.Test\bin\Debug\TaskMaster.Test.dll").LastWriteTimeUtc
    Write-Output ("TEST_DLL_ADVANCED: " + ($null -eq $before -or $after -gt $before))
    Write-Output ("CSC_OUT_TASKMASTER: " + @($lines | Where-Object { $_.Contains("/out:obj\Debug\TaskMaster.dll") }).Count)
    Write-Output ("CSC_OUT_TASKMASTER_TEST: " + @($lines | Where-Object { $_.Contains("/out:obj\Debug\TaskMaster.Test.dll") }).Count)

**CMD-VSTEST** (coordinator fixture run; `TASKID` and `NAMES` substituted; `Command:` records `vstest.console.exe TaskMaster.Test\bin\Debug\TaskMaster.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation "/TestCaseFilter:FullyQualifiedName~TaskMaster.Test.Ribbon.EngineToggleStateCoordinatorTests" "/ResultsDirectory:coverage\test-results\964\TASKID" "/Logger:trx;LogFileName=TASKID.trx" "/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None"` resolved through vswhere; a run whose filter matches zero tests is a failure):

    PRELUDE
    TOOLS
    $results = "coverage\test-results\964\TASKID"
    if (Test-Path -LiteralPath $results) { Remove-Item -LiteralPath $results -Recurse -Force }
    $names = @(NAMES)
    $global:LASTEXITCODE = 0
    & $vstest "TaskMaster.Test\bin\Debug\TaskMaster.Test.dll" /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation "/TestCaseFilter:FullyQualifiedName~TaskMaster.Test.Ribbon.EngineToggleStateCoordinatorTests" "/ResultsDirectory:$results" "/Logger:trx;LogFileName=TASKID.trx" "/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None" 2>&1 | Tee-Object -FilePath "coverage\logs\TASKID.vstest.log" | Out-Null
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
    foreach ($r in $all) { if ($names -contains $r.GetAttribute("testName")) { Write-Output ("RESULT " + $r.GetAttribute("testName") + " = " + $r.GetAttribute("outcome")) } }
    foreach ($r in $all) { if ($r.GetAttribute("outcome") -eq "Failed") { Write-Output ("FAILED " + $r.GetAttribute("testName")); $msg = $r.SelectSingleNode("t:Output/t:ErrorInfo/t:Message", $ns); Write-Output ("MESSAGE " + $r.GetAttribute("testName") + " :: " + $(if ($msg) { $msg.InnerText.Replace([string][char]13, " ").Replace([string][char]10, " / ") } else { "(no message)" })) } }

The artifact transcribes the `COUNTERS`, `RESULT`, `FAILED` and `MESSAGE` lines; the trx stays under the ignored coverage directory.

**CMD-COVERAGE-RUNNER** (CLAUDE.md step 4; `STAGE` is `baseline` or `final`; `Command:` records `pwsh -NoProfile -File scripts\vscode\Invoke-MSTestWithCoverage.ps1` invoked by absolute script path from the worktree directory):

    PRELUDE
    New-Item -ItemType Directory -Path "coverage\logs" -Force | Out-Null
    foreach ($f in @("coverage\coverage.cobertura.xml", "coverage\coverage.cobertura.jacoco.xml", "coverage\test-results\mstest-coverage-run.trx", "coverage\test-results\mstest-coverage-run.summary.txt", "coverage\STAGE-964.cobertura.xml", "coverage\STAGE-964.trx")) { if (Test-Path -LiteralPath $f) { Remove-Item -LiteralPath $f -Force } }
    $script = Join-Path (Get-Location).Path "scripts\vscode\Invoke-MSTestWithCoverage.ps1"
    $global:LASTEXITCODE = 0
    & pwsh -NoProfile -File $script 2>&1 | Tee-Object -FilePath "coverage\logs\STAGE-964.runner.log" | Out-Null
    Write-Output ("RUNNER_EXIT_CODE: " + $LASTEXITCODE)
    $log = Get-Content -LiteralPath "coverage\logs\STAGE-964.runner.log" -Raw -Encoding UTF8
    Write-Output ("DISCOVERED_LINE: " + [regex]::Match($log, "Discovered \d+ test assemblies\.").Value)
    Write-Output ("FIRST_PARTY_LINE: " + [regex]::Match($log, "First-party coverage: [^\r\n]*").Value)
    Write-Output ("THRESHOLD_MESSAGE: " + [regex]::Match($log, "Cobertura (line|branch) coverage [^\r\n]*threshold[^\r\n]*").Value)
    Write-Output ("COLLECT_FAILURE_MESSAGE: " + [regex]::Match($log, "MSTest with coverage failed with exit code \d+").Value)
    Write-Output ("DOCUMENT_PRESENT: " + (Test-Path -LiteralPath "coverage\coverage.cobertura.xml"))
    Write-Output ("TRX_PRESENT: " + (Test-Path -LiteralPath "coverage\test-results\mstest-coverage-run.trx"))
    Write-Output ("SUMMARY_FILE_PRESENT: " + (Test-Path -LiteralPath "coverage\test-results\mstest-coverage-run.summary.txt"))
    if (Test-Path -LiteralPath "coverage\coverage.cobertura.xml") { Copy-Item -LiteralPath "coverage\coverage.cobertura.xml" -Destination "coverage\STAGE-964.cobertura.xml" -Force }
    if (Test-Path -LiteralPath "coverage\test-results\mstest-coverage-run.trx") { Copy-Item -LiteralPath "coverage\test-results\mstest-coverage-run.trx" -Destination "coverage\STAGE-964.trx" -Force }
    Write-Output "PAYLOAD-COMPLETE"

The stale-output removal makes every `_PRESENT` value an observation of this run. Lines of the runner log that carry absolute paths stay in the ignored log; only the named values are transcribed.

**CMD-COVERAGE-POST** (`STAGE` substituted; `RAW` is `True` when the runner printed a non-empty `COLLECT_FAILURE_MESSAGE:`, otherwise `False`, because a completed runner run has already post-processed the document in place):

    PRELUDE
    . (Join-Path (Get-Location).Path "scripts\vscode\Invoke-MSTestWithCoverage.Helpers.ps1")
    . (Join-Path (Get-Location).Path "scripts\vscode\Invoke-MSTest.TrxSummary.ps1")
    $ErrorActionPreference = "Continue"
    $repo = (Get-Location).Path
    $summary = Get-TrxRunSummary -TrxContent (Get-Content -LiteralPath "coverage\STAGE-964.trx" -Raw -Encoding UTF8)
    Write-Output "SUMMARY-BEGIN"
    Write-Output (Format-TrxRunSummary -Summary $summary)
    Write-Output "SUMMARY-END"
    Write-Output ("FAILED-SET: " + (@($summary.FailedTestName) -join ", "))
    [xml]$trxXml = Get-Content -LiteralPath "coverage\STAGE-964.trx" -Raw -Encoding UTF8
    $tns = New-Object System.Xml.XmlNamespaceManager($trxXml.NameTable)
    $tns.AddNamespace("t", "http://microsoft.com/schemas/VisualStudio/TeamTest/2010")
    $defs = @{}
    foreach ($u in @($trxXml.SelectNodes("//t:TestDefinitions/t:UnitTest", $tns))) { $tm = $u.SelectSingleNode("t:TestMethod", $tns); $defs[$u.GetAttribute("id")] = $tm.GetAttribute("className").Split([char]44)[0].Trim() + "." + $tm.GetAttribute("name") }
    Write-Output ("TEST-DEFINITIONS: " + $defs.Count)
    $fqn = @(@($trxXml.SelectNodes("//t:Results/t:UnitTestResult", $tns)) | Where-Object { $_.GetAttribute("outcome") -eq "Failed" } | ForEach-Object { $id = $_.GetAttribute("testId"); if ($defs.ContainsKey($id)) { $defs[$id] } else { "UNRESOLVED:" + $id } } | Sort-Object -Unique -CaseSensitive)
    Write-Output ("FAILED-FQN-COUNT: " + $fqn.Count)
    foreach ($n in $fqn) { Write-Output ("FAILED-FQN " + $n) }
    $doc = Get-Content -LiteralPath "coverage\STAGE-964.cobertura.xml" -Raw -Encoding UTF8
    if ("RAW" -eq "True") { $doc = ConvertTo-KoverageCoberturaXml -XmlContent $doc -RepoRoot $repo; Set-Content -LiteralPath "coverage\STAGE-964.cobertura.xml" -Value $doc -Encoding UTF8 -NoNewline }
    try { Assert-CoberturaLineCoverageThreshold -CoberturaXml $doc; Write-Output "LINE-FLOOR: MET" } catch { Write-Output ("LINE-FLOOR: NOT MET " + $_.Exception.Message) }
    try { Assert-CoberturaBranchCoverageThreshold -CoberturaXml $doc; Write-Output "BRANCH-FLOOR: MET" } catch { Write-Output ("BRANCH-FLOOR: NOT MET " + $_.Exception.Message) }
    Write-Output (Get-CoberturaFirstPartyCoverageReport -CoberturaXml $doc)
    [xml]$xml = $doc
    $root = $xml.SelectSingleNode("/coverage")
    Write-Output ("ROOT line-rate=" + $root.GetAttribute("line-rate") + " branch-rate=" + $root.GetAttribute("branch-rate") + " lines-covered=" + $root.GetAttribute("lines-covered") + " lines-valid=" + $root.GetAttribute("lines-valid"))
    $projection = ConvertTo-JacocoPackageProjection -XmlDocument $xml
    Assert-JacocoProjectionReconciliation -XmlDocument $xml -ProjectionXml $projection
    Write-Output "PROJECTION-BEGIN"
    Write-Output $projection
    Write-Output "PROJECTION-END"
    $srcFiles = @("TaskMaster\Ribbon\EngineToggleStateCoordinator.cs", "TaskMaster\Ribbon\EngineToggleStateCoordinator.Prime.cs", "TaskMaster\Ribbon\EngineToggleStateCoordinator.Messages.cs")
    $nodesTotal = 0; $cv = 0; $vl = 0; $cb = 0; $vb = 0
    foreach ($f in $srcFiles) {
        $norm = $f.Replace([string][char]92, "/")
        $nodes = @($xml.SelectNodes("//class[@filename]") | Where-Object { $_.GetAttribute("filename").Replace([string][char]92, "/").EndsWith($norm) })
        $fc = 0; $fv = 0
        foreach ($c in $nodes) { $s = Get-CoberturaClassLineSummary -ClassNode $c; $fc += $s.CoveredLines; $fv += $s.TotalLines; $cb += $s.CoveredBranches; $vb += $s.TotalBranches }
        $nodesTotal += $nodes.Count; $cv += $fc; $vl += $fv
        Write-Output ("COORD-FILE " + $norm + " nodes=" + $nodes.Count + " covered=" + $fc + " valid=" + $fv)
    }
    Write-Output ("COORD-CLASS-NODES: " + $nodesTotal)
    Write-Output ("COORD-LINES covered=" + $cv + " valid=" + $vl)
    Write-Output ("COORD-BRANCHES covered=" + $cb + " valid=" + $vb)
    Write-Output ("COORD-LINE-RATE: " + $(if ($vl -gt 0) { [math]::Round(100.0 * $cv / $vl, 2) } else { "NA" }))
    foreach ($m in @("HandleToggleClickAsync", "CompletePrime", "TryInvokeSink", "BuildNotifyFailedMessage")) {
        $found = $false
        foreach ($f in $srcFiles) {
            if ($found -or -not (Test-Path -LiteralPath $f)) { continue }
            $src = @(Get-Content -LiteralPath $f -Encoding UTF8)
            $start = 0; for ($i = 0; $i -lt $src.Count; $i++) { if ($src[$i] -match ("^\s{8}(private|internal) (static )?(async )?\w+ " + $m + "\(")) { $start = $i + 1; break } }
            if ($start -eq 0) { continue }
            $end = 0; for ($i = $start; $i -lt $src.Count; $i++) { if ($src[$i].TrimEnd() -eq "        }") { $end = $i + 1; break } }
            $norm = $f.Replace([string][char]92, "/")
            $nodes = @($xml.SelectNodes("//class[@filename]") | Where-Object { $_.GetAttribute("filename").Replace([string][char]92, "/").EndsWith($norm) })
            $map = @{}
            foreach ($c in $nodes) { $s = Get-CoberturaClassLineSummary -ClassNode $c; foreach ($k in $s.LineMap.Keys) { if (-not $map.ContainsKey($k) -or $s.LineMap[$k].Hits -gt $map[$k]) { $map[$k] = $s.LineMap[$k].Hits } } }
            $inSpan = @($map.Keys | Where-Object { $_ -ge $start -and $_ -le $end } | Sort-Object)
            $cov = @($inSpan | Where-Object { $map[$_] -ge 1 }).Count
            $rate = if ($inSpan.Count -gt 0) { [math]::Round(100.0 * $cov / $inSpan.Count, 2) } else { "NA" }
            Write-Output ("METHOD " + $m + " file=" + $norm + " span=" + $start + "-" + $end + " nodes=" + $nodes.Count + " elements=" + $inSpan.Count + " covered=" + $cov + " uncovered=" + ($inSpan.Count - $cov) + " rate=" + $rate)
            foreach ($n in $inSpan) { Write-Output ("METHOD-LINE " + $m + " " + $n + " hits=" + $map[$n]) }
            $found = $true
        }
        if (-not $found) { Write-Output ("METHOD " + $m + " ABSENT") }
    }

At baseline only the main file exists, so `TryInvokeSink` and `BuildNotifyFailedMessage` read `ABSENT` and the Prime and Messages `COORD-FILE` rows read `nodes=0`; those are the expected baseline rows. The summary block and the projection are the two CLAUDE.md committed forms; every other line is a figure, not a document. `FAILED-SET:` (short method names from `Get-TrxRunSummary`, fact 14) is recorded as an observation only; every pre-existing-failure comparison reads the `TEST-DEFINITIONS:`, `FAILED-FQN-COUNT:` and `FAILED-FQN` rows, which are fully qualified names (a `TestMethod` `className` up to its first comma, a dot, and its `name`).

**CMD-LINECOUNT** (line counts of every coordinator source file and every fixture partial present, enumerated from the directories):

    PRELUDE
    $files = @(Get-ChildItem -LiteralPath "TaskMaster\Ribbon" -File -Filter "EngineToggleStateCoordinator*.cs" | Sort-Object Name | ForEach-Object { Join-Path "TaskMaster\Ribbon" $_.Name }) + @(Get-ChildItem -LiteralPath "TaskMaster.Test\Ribbon" -File -Filter "EngineToggleStateCoordinatorTests*.cs" | Sort-Object Name | ForEach-Object { Join-Path "TaskMaster.Test\Ribbon" $_.Name })
    foreach ($p in $files) { Write-Output ("LINES " + $p + " = " + @(Get-Content -LiteralPath $p -Encoding UTF8).Count) }
    Write-Output ("PRODUCTION-FILES: " + @($files | Where-Object { $_.StartsWith("TaskMaster\Ribbon") }).Count)
    Write-Output ("TEST-PARTIALS: " + @($files | Where-Object { $_.StartsWith("TaskMaster.Test\Ribbon") }).Count)

**CMD-STRIPPED-COUNT** (`TOKENS` substituted; counts over the concatenated text of the coordinator source files present with every whitespace run removed, so a formatter line break cannot change a count):

    PRELUDE
    $all = ""
    foreach ($f in @("TaskMaster\Ribbon\EngineToggleStateCoordinator.cs", "TaskMaster\Ribbon\EngineToggleStateCoordinator.Prime.cs", "TaskMaster\Ribbon\EngineToggleStateCoordinator.Messages.cs")) { if (Test-Path -LiteralPath $f) { $all += (Get-Content -LiteralPath $f -Raw -Encoding UTF8) } }
    $text = [regex]::Replace($all, "\s+", "")
    foreach ($t in @(TOKENS)) { Write-Output ("STRIPPED [" + $t + "] = " + ([regex]::Matches($text, [regex]::Escape($t))).Count) }

**CMD-PHRASE-COUNT** (`PHRASES` substituted; each line of the coordinator source files present is trimmed and stripped of a leading `//` or `///` marker, the lines are joined with single spaces and every whitespace run collapses to one space, so a phrase that a comment wraps across lines still counts once):

    PRELUDE
    $parts = New-Object System.Collections.Generic.List[string]
    foreach ($f in @("TaskMaster\Ribbon\EngineToggleStateCoordinator.cs", "TaskMaster\Ribbon\EngineToggleStateCoordinator.Prime.cs", "TaskMaster\Ribbon\EngineToggleStateCoordinator.Messages.cs")) { if (Test-Path -LiteralPath $f) { foreach ($l in @(Get-Content -LiteralPath $f -Encoding UTF8)) { $parts.Add([regex]::Replace($l.Trim(), "^/{2,3}\s?", "")) } } }
    $text = [regex]::Replace(($parts -join " "), "\s+", " ")
    foreach ($t in @(PHRASES)) { Write-Output ("PHRASE [" + $t + "] = " + ([regex]::Matches($text, [regex]::Escape($t))).Count) }

**CMD-SPLIT-CENSUS** (ordinal multiset of non-blank trimmed lines: BASE-SHA file against the three split files):

    PRELUDE
    [Console]::OutputEncoding = [System.Text.Encoding]::UTF8
    $base = @(git show "94287369908cc920b21b0e3256314f988ad7d2f5:TaskMaster/Ribbon/EngineToggleStateCoordinator.cs")
    $work = @()
    foreach ($p in @("TaskMaster\Ribbon\EngineToggleStateCoordinator.cs", "TaskMaster\Ribbon\EngineToggleStateCoordinator.Prime.cs", "TaskMaster\Ribbon\EngineToggleStateCoordinator.Messages.cs")) { $work += @(Get-Content -LiteralPath $p -Encoding UTF8) }
    function Get-Bag([string[]]$lines) { $bag = [System.Collections.Generic.Dictionary[string,int]]::new([System.StringComparer]::Ordinal); foreach ($l in $lines) { $t = $l.TrimStart([char]0xFEFF).Trim(); if ($t.Length -gt 0) { if ($bag.ContainsKey($t)) { $bag[$t] = $bag[$t] + 1 } else { $bag[$t] = 1 } } }; return ,$bag }
    $b = Get-Bag $base; $w = Get-Bag $work
    $keys = @(@($b.Keys) + @($w.Keys) | Sort-Object -Unique -CaseSensitive)
    foreach ($k in $keys) { $nb = 0; $nw = 0; if ($b.ContainsKey($k)) { $nb = $b[$k] }; if ($w.ContainsKey($k)) { $nw = $w[$k] }; if ($nb -gt $nw) { Write-Output ("MISSING x" + ($nb - $nw) + " :: " + $k) }; if ($nw -gt $nb) { Write-Output ("EXTRA x" + ($nw - $nb) + " :: " + $k) } }
    Write-Output ("BASE-LINES: " + $base.Count + " WORK-LINES: " + $work.Count)

**CMD-PROTECTED-SPANS** (hashes the span of every signature in `SIGNATURES` in the BASE-SHA file and in whichever split file holds it; a span runs from the first line containing the signature through the first following line that is exactly eight spaces and a closing brace; two field declarations are hashed between explicit start and end tokens):

    PRELUDE
    [Console]::OutputEncoding = [System.Text.Encoding]::UTF8
    $left = @(git show "94287369908cc920b21b0e3256314f988ad7d2f5:TaskMaster/Ribbon/EngineToggleStateCoordinator.cs")
    if ($left.Count -gt 0) { $left[0] = $left[0].TrimStart([char]0xFEFF) }
    $work = @()
    foreach ($p in @("TaskMaster\Ribbon\EngineToggleStateCoordinator.cs", "TaskMaster\Ribbon\EngineToggleStateCoordinator.Prime.cs", "TaskMaster\Ribbon\EngineToggleStateCoordinator.Messages.cs")) { $work += @(Get-Content -LiteralPath $p -Encoding UTF8) }
    function Get-SpanHash([string[]]$lines, [string]$st, [string]$et) { $a = -1; for ($i = 0; $i -lt $lines.Count; $i++) { if ($lines[$i].Contains($st)) { $a = $i; break } }; if ($a -lt 0) { return "ABSENT" }; $b = -1; for ($i = $a + 1; $i -lt $lines.Count; $i++) { if (($et -eq "CLOSE" -and $lines[$i].TrimEnd() -eq "        }") -or ($et -ne "CLOSE" -and $lines[$i].Contains($et))) { $b = $i; break } }; if ($b -lt 0) { return "UNTERMINATED" }; $text = ((@($lines[$a..$b]) | ForEach-Object { $_.TrimEnd([char]13).TrimEnd() }) -join ([string][char]10)); return [BitConverter]::ToString([System.Security.Cryptography.SHA256]::HashData([Text.Encoding]::UTF8.GetBytes($text))) }
    foreach ($sig in @(SIGNATURES)) { $l = Get-SpanHash $left $sig "CLOSE"; $r = Get-SpanHash $work $sig "CLOSE"; Write-Output ("SPAN-HASH [" + $sig + "] equal=" + ($l -eq $r -and $l -ne "ABSENT" -and $l -ne "UNTERMINATED")) }
    foreach ($pair in @(@("_primeTasks = new ConcurrentDictionary<", ">(StringComparer.Ordinal);"), @("(string EngineName, Type FaultType),", "_reportedPrimeFaults = new ConcurrentDictionary<(string, Type), byte>();"))) { $l = Get-SpanHash $left $pair[0] $pair[1]; $r = Get-SpanHash $work $pair[0] $pair[1]; Write-Output ("SPAN-HASH [" + $pair[0] + "] equal=" + ($l -eq $r -and $l -ne "ABSENT" -and $l -ne "UNTERMINATED")) }

`SIGNATURES-PROTECTED`: `"internal EngineToggleStateCoordinator(", "internal bool GetPressed(string engineName)", "internal async Task ExecuteToggleAsync(string engineName)", "internal Task GetPrimeTask(string engineName)", "private void StartPrimeIfNeeded(string engineName, string controlId)", "private void StartObservedPrime(", "private async Task ApplyPrimeAsync(", "private static string RenderEngineName(string engineName)", "private static string BuildUnavailableMessage(string engineName)", "private static string BuildToggleFailedMessage(string engineName)", "private static string BuildPrimeFailedMessage(string engineName)", "private static string BuildUnmappedKeyMessage(string engineName)"`. A method span excludes the documentation above its signature, so documentation-only edits (F1, F2, F5, F6) leave these spans equal; the span of `StartPrimeIfNeeded` carries the #944 registration-before-start lines, and the span of `StartObservedPrime` carries the `finally` that completes the marker.

**CMD-TEST-DIFF** (AC4 shape of the test-side changes against BASE-SHA):

    PRELUDE
    [Console]::OutputEncoding = [System.Text.Encoding]::UTF8
    foreach ($f in @("TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.cs", "TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.Race.cs")) {
        $d = @(git diff -U0 94287369908cc920b21b0e3256314f988ad7d2f5 -- $f)
        $minus = @($d | Where-Object { $_.StartsWith("-") -and -not $_.StartsWith("--- ") })
        $plus = @($d | Where-Object { $_.StartsWith("+") -and -not $_.StartsWith("+++ ") })
        Write-Output ("DIFF " + $f + " minus=" + $minus.Count + " plus=" + $plus.Count)
        foreach ($m in $minus) { Write-Output ("MINUS " + $f + " :: " + $m.Substring(1).Trim()) }
        Write-Output ("NON-DOC-CHANGES " + $f + " = " + @(@($minus + $plus) | Where-Object { -not $_.Substring(1).Trim().StartsWith("///") }).Count)
    }
    foreach ($f in @("TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.PrimeFaultOrdering.cs", "TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.PrimeRegistration.cs", "TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.ThrowingSink.cs", "TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.RepeatFaultSuppression.cs")) { git diff --quiet 94287369908cc920b21b0e3256314f988ad7d2f5 -- $f; Write-Output ("UNCHANGED " + $f + " exit=" + $LASTEXITCODE) }

**CMD-HASH** (SHA-256 of the six Write Set C# files; hashes only):

    PRELUDE
    foreach ($p in @("TaskMaster\Ribbon\EngineToggleStateCoordinator.cs", "TaskMaster\Ribbon\EngineToggleStateCoordinator.Prime.cs", "TaskMaster\Ribbon\EngineToggleStateCoordinator.Messages.cs", "TaskMaster.Test\Ribbon\EngineToggleStateCoordinatorTests.cs", "TaskMaster.Test\Ribbon\EngineToggleStateCoordinatorTests.SinkGuard.cs", "TaskMaster.Test\Ribbon\EngineToggleStateCoordinatorTests.Race.cs")) { if (Test-Path -LiteralPath $p) { Write-Output ("HASH " + $p + " = " + (Get-FileHash -Algorithm SHA256 -LiteralPath $p).Hash) } else { Write-Output ("HASH " + $p + " = ABSENT") } }

**CMD-HYGIENE** (host-identifier sweep over every Markdown file of the feature folder; the host tokens are derived at run time and never written into the artifact):

    PRELUDE
    $acct = Split-Path -Leaf $env:USERPROFILE; $machine = $env:COMPUTERNAME
    $files = @(Get-ChildItem -LiteralPath "docs\features\active\2026-10-01-engine-toggle-coordinator-947-review-residuals-964" -Recurse -File -Filter "*.md")
    $a = 0; $m = 0; $d = 0
    foreach ($f in $files) { $c = Get-Content -LiteralPath $f.FullName -Raw -Encoding UTF8; $fa = ([regex]::Matches($c, [regex]::Escape($acct), "IgnoreCase")).Count; $fm = ([regex]::Matches($c, [regex]::Escape($machine), "IgnoreCase")).Count; $n = $c.Replace([string][char]92, "/"); $fd = ([regex]::Matches($n, "[a-z]:/+users/+[a-z0-9_.~-]", "IgnoreCase")).Count; $a += $fa; $m += $fm; $d += $fd; if (($fa + $fm + $fd) -gt 0) { Write-Output ("HIT-FILE " + $f.Directory.Name + "/" + $f.Name + " ACCOUNT=" + $fa + " MACHINE=" + $fm + " DRIVE_USERS=" + $fd) } }
    $raw = @(Get-ChildItem -LiteralPath "docs\features\active\2026-10-01-engine-toggle-coordinator-947-review-residuals-964" -Recurse -File | Where-Object { $_.Name -like "*.trx" -or $_.Name -like "*cobertura*" -or $_.Name -like "*.coverage" -or $_.Name -like "*.coveragexml" -or $_.Name -like "*.log" }).Count
    Write-Output ("FILES_SCANNED=" + $files.Count + " ACCOUNT_HITS=" + $a + " MACHINE_HITS=" + $m + " DRIVE_USERS_HITS=" + $d + " RAW_DOCUMENTS=" + $raw)

## Token and phrase sets (quoted verbatim; each is the instruction the delivered source above fulfils)

- `TOKENS-STRUCT` (CMD-STRIPPED-COUNT; base value, then required final value): `"catch("` 3 then 2; `"catch(Exceptionex)"` 1 then 2; `"catch(Exception)"` 2 then 0; `"TryInvokeSink("` 0 then 5; `"_logError("` 2 then 3; `"_notifyUnavailable("` 1 then 1; `"TryInvokeSink(()=>_notifyUnavailable(BuildUnavailableMessage(engineName)),outvarnotifyFailure)"` 0 then 1; `"TryInvokeSink(()=>_logError(BuildNotifyFailedMessage(engineName),notifyFailure),out_)"` 0 then 1; `"TryInvokeSink(()=>_logError(BuildToggleFailedMessage(engineName),ex),out_)"` 0 then 1; `"failure),out_)){_reportedPrimeFaults[reportKey]=0;}"` 0 then 1; `"_reportedPrimeFaults[reportKey]=0;"` 1 then 1; `"privatestaticboolTryInvokeSink(ActionsinkCall,outExceptionsinkFailure)"` 0 then 1; `"privatestaticstringBuildNotifyFailedMessage(stringengineName)"` 0 then 1; `"internalsealedclassEngineToggleStateCoordinator"` 1 then 0; `"internalsealedpartialclassEngineToggleStateCoordinator"` 0 then 3; `"_primeTasks.TryRemove(engineName,out_);"` 1 then 1.
- `PHRASES-DOC` (CMD-PHRASE-COUNT; base value, then required final value): `"The in-flight"` 1 then 0; `"most recently completed"` 1 then 0; `"The prime task, or"` 1 then 0; `"The registration marker for an engine key"` 0 then 1; `"The marker is not the prime task itself"` 0 then 1; `"The registered marker, or"` 0 then 1; `"The other two are sink guards"` 1 then 0; `"which holds the only other <c>catch</c> clause"` 0 then 1; `"the only <c>catch</c> clause in this type that observes an engine fault"` 1 then 1; `"The three"` 1 then 0; `"all sit in"` 1 then 0; `"The two <c>catch</c> clauses in this type are the click boundary"` 0 then 1; `"also contains a failure of the sink"` 1 then 0; `"routes its sink call through"` 0 then 1; `"a sink failure is contained here"` 1 then 0; `"a sink failure is contained by"` 0 then 1; `"by the statement directly after the sink call"` 1 then 0; `"by the only statement of the branch taken when"` 0 then 1; `"never throws, even when the sink throws"` 1 then 0; `"never throws on either path, even when both sinks throw"` 0 then 1; `"honours its non-throwing precondition"` 0 then 2; `"and must not throw"` 0 then 1; `"The call is guarded (issue #964)"` 0 then 1; `"Receives an observed prime fault, toggle fault or notification failure"` 0 then 1.

### Phase 0 — Policy Reads, Anchor and Baseline Capture

- [ ] [P0-T1] Read the policy documents in the mandatory order — CLAUDE.md, then .claude/rules/general-code-change.md, then .claude/rules/general-unit-test.md, then .claude/rules/csharp.md — plus .claude/rules/tonality.md and .claude/rules/plan-acceptance-gates.md, and record FEATURE/evidence/baseline/phase0-instructions-read.md.
  - Acceptance: the artifact carries `Timestamp:`, a `Policy Order:` line naming CLAUDE.md, general-code-change.md, general-unit-test.md and csharp.md in that order, and a `Files read:` list naming all six repository-relative paths, one per line. No policy document is modified.
- [ ] [P0-T2] Read `docs/features/active/2026-10-01-engine-toggle-coordinator-947-review-residuals-964/issue.md` in full and this plan, and record the requirements anchor and the Write Set in FEATURE/evidence/baseline/scope-and-anchor.md.
  - Command: `git -C WORKTREE status --porcelain --untracked-files=all` (recorded verbatim as `INHERITED-PORCELAIN:`); the Glob tool over `docs/features/active/2026-10-01-engine-toggle-coordinator-947-review-residuals-964` for `spec.md`, `user-story.md` and `research*.md`; the Glob tool over `docs/features/active/2026-10-01-engine-toggle-coordinator-947-review-residuals-964/evidence/other` for `preflight-clearance.*.md` (recorded as `CLEARANCE-PATH:` in repository-relative form with forward-slash separators, `docs/features/active/2026-10-01-engine-toggle-coordinator-947-review-residuals-964/evidence/other/preflight-clearance.<yyyy-MM-ddTHH-mm>.md`; the Glob tool can return a backslash-separated or absolute path, which is converted to that form before it is recorded, because `git rev-parse HEAD:<path>` does not resolve a backslash-separated tree path); `git -C WORKTREE rev-parse HEAD:CLEARANCE-PATH` with the recorded path substituted (recorded as `CLEARANCE-BLOB:`).
  - Acceptance, all required: the artifact records that issue.md line 12 reads `- Work Mode: minor-audit`; that a heading line exactly `## Acceptance Criteria` exists; that the section holds exactly 8 lines beginning `- [ ] AC` and 0 beginning `- [x] AC` (counted with the Grep tool, patterns `^- \[ \] AC[1-8] ` and `^- \[x\] AC`); that the Glob result for the three names is `none` (a hit is `UNEXPECTED REQUIREMENTS DOCUMENT`: stop, per the minor-audit fail-closed rule); the eight code paths of the Write Set verbatim; and `INHERITED-PORCELAIN:` with every entry, each of which must lie under the feature folder, equal `docs/features/potential/promoted/2026-10-01-engine-toggle-coordinator-947-review-residuals.md`, or lie under `.claude/agent-memory/` (any other entry is `UNEXPECTED INHERITED CHANGE`: stop); and the preparation-phase record of the Write Set: the Glob result for `preflight-clearance.*.md` is exactly one path, recorded as `CLEARANCE-PATH:`, and `CLEARANCE-BLOB:` is the 40-hex-digit blob that `rev-parse` prints for it (zero or several paths, or a `rev-parse` failure because the record is not committed at HEAD, is `PREPARATION RECORD MISSING`: stop).
- [ ] [P0-T3] Verify the base anchor of the branch for `TaskMaster/` and `TaskMaster.Test/` against BASE-SHA and record FEATURE/evidence/baseline/anchor-production.md (this task creates the file; P0-T4 appends to it).
  - Command: `git -C WORKTREE rev-parse HEAD`; `git -C WORKTREE rev-parse origin/main`; `git -C WORKTREE merge-base 94287369908cc920b21b0e3256314f988ad7d2f5 HEAD`; `git -C WORKTREE diff --exit-code --stat 94287369908cc920b21b0e3256314f988ad7d2f5 -- TaskMaster TaskMaster.Test`.
  - Acceptance, all required: the merge-base output equals `94287369908cc920b21b0e3256314f988ad7d2f5` (otherwise `BASE NOT ANCESTOR`: stop); the diff exits 0 and prints nothing (`ANCHOR-CODE-DIFF-EXIT=0`; otherwise `CODE DIFFERS FROM BASE`: stop); `HEAD:` and `ORIGIN-MAIN:` are recorded as observations, and when `ORIGIN-MAIN:` differs from BASE-SHA the artifact records `ORIGIN-MAIN MOVED` without stopping, because every anchor in this plan is the BASE-SHA literal.
- [ ] [P0-T4] Verify the split anchors and the false-before token and phrase values of `TaskMaster/Ribbon/EngineToggleStateCoordinator.cs`, and append them to FEATURE/evidence/baseline/anchor-production.md.
  - Command: the Read tool over `TaskMaster/Ribbon/EngineToggleStateCoordinator.cs` lines 1 to 10, 43 to 55, 253 to 272, 403 to 407, 433 to 442 and 492 to 496; `CMD-STRIPPED-COUNT` with `TOKENS-STRUCT`; `CMD-PHRASE-COUNT` with `PHRASES-DOC`; the Grep tool count of pattern `^` over the file.
  - Acceptance, all required: the Grep count is 496; the lines read match fact 2 exactly at 3 (`using System.Globalization;`), 4 (`using System.Threading;`), 45, 46, 47 (`/// <summary>`), 51, 52 (blank), 53, 255 (`        }`), 256 (blank), 257 (`/// <summary>`), 270, 405, 435 (`        }`), 436 (blank), 437 (`/// <summary>`), 440, 494 (`        }`), 495 (`    }`) and 496 (`}`) (any mismatch is `SPLIT ANCHOR MOVED`: stop); every `STRIPPED` value equals its base value in `TOKENS-STRUCT` and every `PHRASE` value equals its base value in `PHRASES-DOC` (any other value is `BASELINE TOKEN MISMATCH`: stop). The recorded values are the false-before half of the P1-T22 gate.
- [ ] [P0-T5] Re-derive the test-side anchors for TaskMaster.Test/TaskMaster.Test.csproj, TaskMaster/TaskMaster.csproj and the fixture partials under TaskMaster.Test/Ribbon, and record FEATURE/evidence/baseline/anchor-test-side.md.
  - Command: `CMD-LINECOUNT`; the Grep tool with `-n` over `TaskMaster.Test/TaskMaster.Test.csproj` for `EngineToggleStateCoordinatorTests` and over `TaskMaster/TaskMaster.csproj` for `EngineToggleStateCoordinator`; the Grep tool count over `TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests*.cs` for `\[TestMethod\]`, `\[DataTestMethod\]`, `\[DataRow\(`, `\[TestClass\]` and `OnNotify`; the Grep tool over `TaskMaster.Test` for each `NEW-NAMES-964` name.
  - Acceptance, all required: `PRODUCTION-FILES: 1` and `TEST-PARTIALS: 6`, with `LINES` 496 for the production file and 470, 77, 175, 277, 290 and 215 for the primary, PrimeFaultOrdering, PrimeRegistration, Race, RepeatFaultSuppression and ThrowingSink partials; the test csproj lists six fixture entries (352, 359 to 363) and the production csproj one coordinator entry (466); `[TestMethod]` totals 36, `[DataTestMethod]` 1, `[DataRow(` 3 and `[TestClass]` 1, recorded as `EXPECTED-CASES: 39`; `OnNotify` count 0; every `NEW-NAMES-964` name has 0 hits; no file named `EngineToggleStateCoordinatorTests.SinkGuard.cs`, `EngineToggleStateCoordinator.Prime.cs` or `EngineToggleStateCoordinator.Messages.cs` exists. Any mismatch is `TEST ANCHOR MOVED`: stop.
- [ ] [P0-T6] Provision the repository .NET SDK with scripts/vscode/Install-RepoDotNetSdk.ps1 (guarded) and record FEATURE/evidence/baseline/bootstrap-sdk.md.
  - Command: `pwsh -NoProfile -Command 'Set-Location -LiteralPath "WORKTREE"; [Environment]::CurrentDirectory = (Get-Location).Path; if (-not (Test-Path -LiteralPath ".dotnet-sdk\sdk\8.0.205")) { & (Join-Path (Get-Location).Path "scripts\vscode\Install-RepoDotNetSdk.ps1") }; "SDK_MARKER=$(Test-Path -LiteralPath ".dotnet-sdk\sdk\8.0.205")"; dotnet --version'`
  - Acceptance: `SDK_MARKER=True`, `dotnet --version` prints a version string rather than the global.json error message, `EXIT_CODE: 0`. Installer lines carrying an absolute path are transcribed with REDACTED-PATH.
- [ ] [P0-T7] Restore the manifest tools from dotnet-tools.json with `dotnet tool restore` and record FEATURE/evidence/baseline/bootstrap-tool-restore.md.
  - Command: `pwsh -NoProfile -Command 'Set-Location -LiteralPath "WORKTREE"; [Environment]::CurrentDirectory = (Get-Location).Path; dotnet tool restore; "RESTORE_EXIT=$LASTEXITCODE"; dotnet tool list --local'`
  - Acceptance: `RESTORE_EXIT=0` and the local tool list contains a row whose Package Id is `csharpier` and whose Version is `1.2.6`. Only the Package Id and Version columns are transcribed (the Manifest column carries an absolute path).
- [ ] [P0-T8] Restore NuGet packages with scripts/vscode/Invoke-Restore.ps1 and record FEATURE/evidence/baseline/bootstrap-nuget-restore.md.
  - Command: `pwsh -NoProfile -Command 'Set-Location -LiteralPath "WORKTREE"; [Environment]::CurrentDirectory = (Get-Location).Path; $env:MSBUILDDISABLENODEREUSE = "1"; & (Join-Path (Get-Location).Path "scripts\vscode\Invoke-Restore.ps1"); "RESTORE_EXIT=$LASTEXITCODE"; "PACKAGE_DIRS=$(@(Get-ChildItem -LiteralPath packages -Directory -ErrorAction SilentlyContinue).Count)"; foreach ($proj in @("TaskMaster\TaskMaster.csproj", "TaskMaster.Test\TaskMaster.Test.csproj")) { $dir = Split-Path -Parent $proj; [xml]$x = Get-Content -LiteralPath $proj -Raw; $missing = @($x.SelectNodes("//*[local-name()=""Analyzer""]") | Where-Object { -not (Test-Path -LiteralPath (Join-Path $dir $_.GetAttribute("Include"))) }).Count; "ANALYZER_MISSING $proj = $missing" }'`
  - Acceptance: `RESTORE_EXIT=0`, `PACKAGE_DIRS=` at least 1, and both `ANALYZER_MISSING` values 0 (non-zero is `ANALYZER PATH SKEW`: stop).
- [ ] [P0-T9] Provision the dotnet-coverage global tool (guarded) for scripts/vscode/Invoke-MSTestWithCoverage.ps1 and record FEATURE/evidence/baseline/bootstrap-dotnet-coverage.md.
  - Command: `pwsh -NoProfile -Command 'Set-Location -LiteralPath "WORKTREE"; [Environment]::CurrentDirectory = (Get-Location).Path; if (-not (Get-Command dotnet-coverage -ErrorAction SilentlyContinue)) { dotnet tool install --global dotnet-coverage }; "DOTNET_COVERAGE_RESOLVED=$($null -ne (Get-Command dotnet-coverage -ErrorAction SilentlyContinue))"; dotnet-coverage --version'`
  - Acceptance: `DOTNET_COVERAGE_RESOLVED=True`, a version line is printed, `EXIT_CODE: 0`.
- [ ] [P0-T10] Capture the read-only formatter baseline over the worktree (`.csharpierignore` applies) with `dotnet tool run csharpier check .` and record FEATURE/evidence/baseline/csharpier-check-baseline.md.
  - Command: `pwsh -NoProfile -Command 'Set-Location -LiteralPath "WORKTREE"; [Environment]::CurrentDirectory = (Get-Location).Path; dotnet tool run csharpier check .; "CSHARPIER_EXIT_CODE: $LASTEXITCODE"'`
  - Acceptance: `EXIT_CODE:` is the printed `CSHARPIER_EXIT_CODE:` value, the `Checked N files` line is recorded, and every path CSharpier reports as unformatted is listed. `EXIT_CODE: 0` is required; a non-zero value is `FORMAT BASELINE NOT CLEAN`: stop, because the Phase 2 repository-wide format would then rewrite files outside the Write Set.
- [ ] [P0-T11] Capture the analyzer baseline of TaskMaster.sln with `CMD-REBUILD` (`GATEARGS` `/p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`, `TASKID` p0-t11) and record FEATURE/evidence/baseline/msbuild-analyzer-baseline.md (`Command:` `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`).
  - Acceptance, all required: `EXIT_CODE: 0`; `ERRORS: 0`; both `CSC_OUT_` counts at least 1; `WRITESET_DIAGNOSTIC_LINES: 0`; `WARNINGS:` recorded as `ANALYZER-BASELINE-WARNINGS:`; `TEST_DLL_EXISTS: True`. Non-zero exit is `ANALYZER BASELINE NOT CLEAN`: stop.
- [ ] [P0-T12] Capture the nullable baseline of TaskMaster.sln with `CMD-REBUILD` (`GATEARGS` `/p:TreatWarningsAsErrors=true`, `TASKID` p0-t12) and record FEATURE/evidence/baseline/msbuild-nullable-baseline.md (`Command:` `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`).
  - Acceptance, all required: `EXIT_CODE: 0`; `ERRORS: 0`; both `CSC_OUT_` counts at least 1; `WRITESET_DIAGNOSTIC_LINES: 0`; `WARNINGS:` recorded as `NULLABLE-BASELINE-WARNINGS:`; `TEST_DLL_EXISTS: True`. Non-zero exit is `NULLABLE BASELINE NOT CLEAN`: stop.
- [ ] [P0-T13] Capture the pre-change coordinator fixture run over TaskMaster.Test\bin\Debug\TaskMaster.Test.dll with `CMD-VSTEST` (`TASKID` p0-t13, `NAMES` `NAMES-ALL`) and record FEATURE/evidence/baseline/coordinator-tests-baseline.md.
  - Acceptance, all required: `EXIT_CODE: 0`; `TRX_PRESENT: True`; `SEQUENCE_FILES: 0`; `COUNTERS` with `total` equal to `EXPECTED-CASES` (39) and `failed=0`, recorded as `BASELINE-TOTAL: 39`; every `INVARIANT-NAMES` entry has a `RESULT ... = Passed` line; no `RESULT` line names a `NEW-NAMES-964` entry; no `FAILED` line. Anything else is `EXISTING FIXTURE NOT GREEN AT BASE`: stop.
- [ ] [P0-T14] Capture the baseline repository-wide test-and-coverage run with scripts/vscode/Invoke-MSTestWithCoverage.ps1 through `CMD-COVERAGE-RUNNER` (`STAGE` baseline) and then `CMD-COVERAGE-POST` (`STAGE` baseline, `RAW` per its rule), and record FEATURE/evidence/baseline/coverage-baseline.md.
  - Artifact: `Timestamp:`; `Command:` `pwsh -NoProfile -File scripts\vscode\Invoke-MSTestWithCoverage.ps1`; `EXIT_CODE:` the `RUNNER_EXIT_CODE:`; `ExpectedExitCode:` equal to it when non-zero; `Output Summary:` (at most 20 lines) carrying the exit code, `RAW:`, `LINE-FLOOR:`, `BRANCH-FLOOR:`, the `First-party coverage:` line (repository line and branch headline), the `ROOT` line, `COORD-LINES`, `COORD-LINE-RATE:` (the coordinator class line coverage baseline, recorded also as `BASELINE-COORD-LINE-RATE:`) and `BASELINE-FAILED-FQN-COUNT:` (the `FAILED-FQN-COUNT:` value) in `Output Summary:` and every `FAILED-FQN` row, with `TEST-DEFINITIONS:`, under `Details:`; then `Details:` with `DISCOVERED_LINE:`, `THRESHOLD_MESSAGE:`, `COLLECT_FAILURE_MESSAGE:`, `DOCUMENT_PRESENT:`, `TRX_PRESENT:`, `SUMMARY_FILE_PRESENT:`, the projection verbatim between `PROJECTION-BEGIN` and `PROJECTION-END`, the summary verbatim between `SUMMARY-BEGIN` and `SUMMARY-END`, every `COORD-FILE` row, `COORD-CLASS-NODES:`, `COORD-BRANCHES`, and every `METHOD` and `METHOD-LINE` row.
  - Branches, checked in order: (d) `DOCUMENT_PRESENT: False` or `TRX_PRESENT: False` is `COVERAGE RUN ABORTED`: stop without `CMD-COVERAGE-POST`. (c) a non-empty `THRESHOLD_MESSAGE:`, `LINE-FLOOR: NOT MET` or `BRANCH-FLOOR: NOT MET` is `COVERAGE FLOOR BASELINE NOT MET`: record and stop. (b) a non-zero exit with a non-empty `COLLECT_FAILURE_MESSAGE:`, `TEST-DEFINITIONS:` at least 1, `FAILED-FQN-COUNT:` at least 1, no `FAILED-FQN` value beginning `UNRESOLVED:` and none beginning `TaskMaster.Test.Ribbon.EngineToggleStateCoordinatorTests.` completes this task with those `FAILED-FQN` rows as the baseline failed set (the pre-existing local failures, for example the shell-icon test of fact 11; CI runs them). (a) exit 0 with both floors met completes it with `BASELINE-FAILED-FQN-COUNT: 0`. Any other outcome, including a non-zero exit with `FAILED-FQN-COUNT: 0` or with a `FAILED-FQN` value beginning `UNRESOLVED:`, is `COVERAGE RUN ABORTED`: stop.
  - Acceptance, all required: branch (a) or (b); `COORD-CLASS-NODES: 1` with the main-file `COORD-FILE` row `nodes=1` and the Prime and Messages rows `nodes=0`; `COORD-LINES` valid at least 1; `METHOD HandleToggleClickAsync` and `METHOD CompletePrime` rows present with `elements=` at least 1, recorded as `BASELINE-METHOD-HTC-UNCOVERED:` and `BASELINE-METHOD-CP-UNCOVERED:`; `METHOD TryInvokeSink ABSENT` and `METHOD BuildNotifyFailedMessage ABSENT`; the projection contains a `package` named `TaskMaster` with `LINE` and `BRANCH` counters; the summary first line begins `Test run outcome:`; no absolute path in the artifact. `coverage\baseline-964.cobertura.xml` and `coverage\baseline-964.trx` stay on disk, git-ignored.

### Phase 1 — Constrained Implementation: Behaviour-Preserving Split, Regression Tests First, Then the Fix

Phase 1 is the constrained small-path implementation, executed task by task by the delegated executor. Ordering: the split (P1-T2 to P1-T8) lands and is proven behaviour-preserving before any test is written; the regression tests (P1-T9 to P1-T14) are recorded failing against the split but unfixed coordinator; the fix (P1-T15 to P1-T24) then turns them green.

- [ ] [P1-T1] Record the implementation handoff for the Write Set of docs/features/active/2026-10-01-engine-toggle-coordinator-947-review-residuals-964/plan.2026-10-02T05-20.md in FEATURE/evidence/other/implementation-handoff.md.
  - Acceptance: the artifact carries `Timestamp:`, names the delegated executor role (atomic-executor, small-path implementation), lists the eight Write Set code paths verbatim, states the implementation-completion criteria (P1-T24 pass-after gate, P1-T22 census gate, P1-T25 unchanged-partials gate and P1-T26 size gate all met), and records the Edit-route rule of D-11.
- [ ] [P1-T2] Create `TaskMaster/Ribbon/EngineToggleStateCoordinator.Messages.cs` with the Write tool as delivered source S1, transcribing base lines 47 to 51 and 437 to 494 of `TaskMaster/Ribbon/EngineToggleStateCoordinator.cs` verbatim (read with the Read tool).
  - Acceptance: the file exists; its first two lines are `using System;` and `using System.Globalization;`; the Grep tool counts `internal sealed partial class EngineToggleStateCoordinator` 1, `private const string NullEngineNameToken` 1 and `private static string BuildUnmappedKeyMessage` 1 in it. The transcription itself is proved by P1-T7.
- [ ] [P1-T3] Create `TaskMaster/Ribbon/EngineToggleStateCoordinator.Prime.cs` with the Write tool as delivered source S2, transcribing base lines 257 to 435 of `TaskMaster/Ribbon/EngineToggleStateCoordinator.cs` verbatim.
  - Acceptance: the file exists; its usings are exactly `System`, `System.Threading`, `System.Threading.Tasks` and `UtilitiesCS`; the Grep tool counts `internal Task GetPrimeTask` 1 and `private void CompletePrime` 1 in it.
- [ ] [P1-T4] Reduce `TaskMaster/Ribbon/EngineToggleStateCoordinator.cs` with the Edit or Write tool to the 249-line main partial of delivered source S3.
  - Acceptance: the Grep tool counts 0 in the file for each of `using System\.Globalization;`, `using System\.Threading;`, `NullEngineNameToken = `, `internal Task GetPrimeTask` and `private static string RenderEngineName`, and 1 for each of `internal sealed partial class EngineToggleStateCoordinator` and `internal async Task ExecuteToggleAsync`; the Grep count of pattern `^` over the file is 249.
- [ ] [P1-T5] Register the two new files in `TaskMaster/TaskMaster.csproj` with the Edit tool as delivered source C1.
  - Acceptance: the Grep tool counts exactly 1 line for each of `Ribbon\x5CEngineToggleStateCoordinator\.cs"`, `Ribbon\x5CEngineToggleStateCoordinator\.Messages\.cs"` and `Ribbon\x5CEngineToggleStateCoordinator\.Prime\.cs"` in the file, the two new lines directly follow the existing entry (Read tool), and the numstat row of `git -C WORKTREE diff --numstat 94287369908cc920b21b0e3256314f988ad7d2f5 -- TaskMaster/TaskMaster.csproj` reports 2 inserted and 0 deleted lines. The Grep counts, the Read observation and the numstat row are recorded under `CSPROJ-REGISTRATION:` in FEATURE/evidence/regression-testing/split-census.md (this task creates that file, with `Timestamp:`, `Command:`, `EXIT_CODE:` and `Output Summary:`).
- [ ] [P1-T6] Format the three coordinator files under TaskMaster/Ribbon with `dotnet tool run csharpier format TaskMaster/Ribbon/EngineToggleStateCoordinator.cs TaskMaster/Ribbon/EngineToggleStateCoordinator.Prime.cs TaskMaster/Ribbon/EngineToggleStateCoordinator.Messages.cs`, then verify with `dotnet tool run csharpier check TaskMaster/Ribbon/EngineToggleStateCoordinator.cs TaskMaster/Ribbon/EngineToggleStateCoordinator.Prime.cs TaskMaster/Ribbon/EngineToggleStateCoordinator.Messages.cs` (both wrapped per the Toolchain commands convention).
  - Acceptance: the format exits 0 (its `Formatted N files` line is a processed count, not an assertion); the success-case observation is the check run, which exits 0 and prints `Checked 3 files` with no path listed. Both outputs are recorded in FEATURE/evidence/regression-testing/split-census.md under `FORMAT:` (appended to the file P1-T5 created).
- [ ] [P1-T7] Prove the split is a pure move with `CMD-SPLIT-CENSUS` over the three files under TaskMaster/Ribbon and append the output to FEATURE/evidence/regression-testing/split-census.md.
  - Acceptance: the output consists of exactly these eight difference lines and the `BASE-LINES:` line: `MISSING x1 :: internal sealed class EngineToggleStateCoordinator`, `EXTRA x3 :: internal sealed partial class EngineToggleStateCoordinator`, `EXTRA x2 :: namespace TaskMaster`, `EXTRA x4 :: {`, `EXTRA x4 :: }`, `EXTRA x2 :: using System;`, `EXTRA x1 :: using System.Threading.Tasks;`, `EXTRA x1 :: using UtilitiesCS;`. Any other `MISSING` or `EXTRA` line is `SPLIT NOT A PURE MOVE`: correct the transcription with the Edit tool against the base lines and re-run P1-T6 and this task; after two failed corrections stop. `BASE-LINES: 496` is required.
- [ ] [P1-T8] Build TaskMaster.sln with `CMD-BUILD` (`TASKID` p1-t8) and run the unchanged fixture against the split coordinator with `CMD-VSTEST` (`TASKID` p1-t8, `NAMES` `NAMES-ALL`), recording FEATURE/evidence/regression-testing/split-fixture-green.md.
  - Acceptance, all required: `MSBUILD_EXIT_CODE: 0`, `ERRORS: 0`, `CSC_OUT_TASKMASTER:` at least 1 (the split files compiled); `VSTEST_EXIT_CODE: 0`, `SEQUENCE_FILES: 0`, `COUNTERS` total equal to `BASELINE-TOTAL` (39) with `failed=0`; every `INVARIANT-NAMES` entry `Passed`. Anything else is `SPLIT CHANGED BEHAVIOUR`: stop.
- [ ] [P1-T9] Add the `OnNotify` harness member to `TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.cs` with the Edit tool as delivered source T1.
  - Acceptance: the Grep tool counts `internal Action<string> OnNotify` 1, `OnNotify\?\.Invoke\(message\);` 1 and `Notifications\.Add\(message\),` 0 in the file, and `Notifications\.Add\(message\);` 1.
- [ ] [P1-T10] Create `TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.SinkGuard.cs` with the Write tool as delivered source T2.
  - Acceptance: the file exists and the Grep tool counts `\[TestMethod\]` 4 and `\[TestClass\]` 0 in it, and each `NEW-NAMES-964` name exactly once.
- [ ] [P1-T11] Register the new partial in `TaskMaster.Test/TaskMaster.Test.csproj` with the Edit tool as delivered source C2.
  - Acceptance: the Grep tool counts exactly 1 line for `Ribbon\x5CEngineToggleStateCoordinatorTests\.SinkGuard\.cs"` in the file, directly after the RepeatFaultSuppression entry (Read tool), and the numstat row of `git -C WORKTREE diff --numstat 94287369908cc920b21b0e3256314f988ad7d2f5 -- TaskMaster.Test/TaskMaster.Test.csproj` reports 1 inserted and 0 deleted lines. The Grep count, the Read observation and the numstat row are recorded under `TEST-CSPROJ-REGISTRATION:` in FEATURE/evidence/regression-testing/sink-guard-partial-tokens.md (this task creates that file, with `Timestamp:`, `Command:`, `EXIT_CODE:` and `Output Summary:`).
- [ ] [P1-T12] Reword the stale remark in `TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.Race.cs` with the Edit tool as delivered source T3 (related defect D-7a).
  - Acceptance: the Grep tool counts `logs a second error` 0 and `Since issue #948 that second cancellation is a repeat of a kind already` 1 in the file; P1-T25 proves no code line changed.
- [ ] [P1-T13] Format the three touched test files under TaskMaster.Test/Ribbon with `dotnet tool run csharpier format TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.cs TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.SinkGuard.cs TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.Race.cs`, verify with `dotnet tool run csharpier check` over the same three paths (both wrapped per the Toolchain commands convention), and append to FEATURE/evidence/regression-testing/sink-guard-partial-tokens.md.
  - Acceptance, all required: the check run exits 0 and prints `Checked 3 files` with no path listed (the success-case observation of the write-mode format); in the SinkGuard partial the Grep tool counts 0 for each of `Thread\.Sleep`, `Task\.Delay`, `DoNotParallelize`, `GetTempPath`, `File\.`, `DateTime\.Now` and `DateTime\.UtcNow`, and at least 2 for `TaskCompletionSource` (positive control that the file was read); the reason fragments `a throwing notification sink must not escape the refusal path`, `the refusal path contains a failure of both sinks` and `notify sink failed` each occur whole on one physical line (Grep count at least 1 each).
- [ ] [P1-T14] [expect-fail] Build TaskMaster.sln with `CMD-BUILD` (`TASKID` p1-t14) and run the fixture with the new tests against the split but unfixed coordinator with `CMD-VSTEST` (`TASKID` p1-t14, `NAMES` `NAMES-ALL`), recording FEATURE/evidence/regression-testing/refusal-path-fail-before.md with `ExpectedExitCode:` equal to the observed non-zero exit.
  - Acceptance, all required: `MSBUILD_EXIT_CODE: 0`, `ERRORS: 0`, `TEST_DLL_ADVANCED: True`, `CSC_OUT_TASKMASTER_TEST:` at least 1; `CMD-STRIPPED-COUNT` with `TOKENS-STRUCT`, run immediately before the test run, prints the base value for every token except the two split tokens (`"internalsealedclassEngineToggleStateCoordinator"` 0 and `"internalsealedpartialclassEngineToggleStateCoordinator"` 3), proving the fix is absent from the coordinator under test; `SEQUENCE_FILES: 0`; `COUNTERS` total equal to `BASELINE-TOTAL` plus 4 (43) with `failed=3`; the `FAILED` lines name exactly the three `FAIL-BEFORE-NAMES`; `RESULT GetPressed_WhenLogSinkThrowsOnFaultedPrime_SameFaultKindIsReportedAgain = Passed` and every `INVARIANT-NAMES` entry `Passed`. Reason gate: the first name's `MESSAGE` contains both `a throwing notification sink must not escape the refusal path` and `notify sink failed`; the second's contains `threw exception` and `notify sink failed`; the third's contains `the refusal path contains a failure of both sinks` and `notify sink failed`. A `FAIL-BEFORE-NAMES` entry that passes is `FAIL-BEFORE NOT REPRODUCED`: stop; one whose message lacks its two fragments is `FAIL-BEFORE WRONG REASON`: stop; the `GUARD-NAME` failing is `INVARIANT GUARD RED AT BASE`: stop; any other failed test is `UNEXPECTED FAILURE`: stop. Absolute paths in messages are transcribed as REDACTED-PATH.
- [ ] [P1-T15] Apply delivered source F1 (constructor parameter docs) and F2 (`GetPressed` returns) to `TaskMaster/Ribbon/EngineToggleStateCoordinator.cs` with the Edit tool.
  - Acceptance: the Grep tool counts `and must not throw: its` 1, `an exception it throws is reported once through` 1, `Receives an observed prime fault, toggle fault or notification failure as a message` 1 and `honours its non-throwing precondition` 1 in the file.
- [ ] [P1-T16] Apply delivered source F3 (`HandleToggleClickAsync` summary, remarks and body) to `TaskMaster/Ribbon/EngineToggleStateCoordinator.cs` with the Edit tool.
  - Acceptance: the Grep tool counts `_notifyUnavailable\(BuildUnavailableMessage\(engineName\)\);` 0 and `The other two are sink guards` 0 in the file, and `out var notifyFailure` 1.
- [ ] [P1-T17] Insert delivered source F4 (`TryInvokeSink`) into `TaskMaster/Ribbon/EngineToggleStateCoordinator.cs` with the Edit tool, directly after the closing brace of `ExecuteToggleAsync`.
  - Acceptance: the Grep tool counts `private static bool TryInvokeSink\(Action sinkCall, out Exception sinkFailure\)` 1 in the file.
- [ ] [P1-T18] Apply delivered source F5 (`GetPrimeTask` documentation) to `TaskMaster/Ribbon/EngineToggleStateCoordinator.Prime.cs` with the Edit tool.
  - Acceptance: the Grep tool counts `The in-flight` 0 and `The prime task, or` 0 in the file, and `The marker is not the` at least 1.
- [ ] [P1-T19] Apply delivered source F6 (`StartObservedPrime` remarks) to `TaskMaster/Ribbon/EngineToggleStateCoordinator.Prime.cs` with the Edit tool.
  - Acceptance: the Grep tool counts `The three` 0 and `all sit in` 0 in the file.
- [ ] [P1-T20] Apply delivered source F7a, F7b and F7c (`CompletePrime` summary, remarks and guarded report) to `TaskMaster/Ribbon/EngineToggleStateCoordinator.Prime.cs` with the Edit tool. The record `_reportedPrimeFaults[reportKey] = 0;` must be the only statement of the branch taken when `TryInvokeSink` returns `true`, directly after the guarded sink call; it is never placed before the call, in an `else` branch or after the branch (D-3, the #948 invariant that a throwing sink leaves the report owed).
  - Acceptance: the Grep tool counts `catch \(Exception\)` 0, `sink failure is contained here` 0 and `_reportedPrimeFaults\[reportKey\] = 0;` 1 in the file.
- [ ] [P1-T21] Insert delivered source F8 (`BuildNotifyFailedMessage`) into `TaskMaster/Ribbon/EngineToggleStateCoordinator.Messages.cs` with the Edit tool, directly after the closing brace of `BuildToggleFailedMessage`, then format the three coordinator files under TaskMaster/Ribbon with the P1-T6 format command and verify them with the P1-T6 check command (both wrapped per the Toolchain commands convention).
  - Acceptance: the Grep tool counts `private static string BuildNotifyFailedMessage\(string engineName\)` 1 in the Messages file; the check run exits 0 and prints `Checked 3 files` with no path listed (the success-case observation of the write-mode format). Both outputs and the Grep count are recorded under `FORMAT:` in FEATURE/evidence/qa-gates/production-edit-scope.md (this task creates that file, with `Timestamp:`, `Command:`, `EXIT_CODE:` and `Output Summary:`).
- [ ] [P1-T22] Verify the edit scope of the three files under TaskMaster/Ribbon (AC3, AC5, AC7) with `CMD-STRIPPED-COUNT` (`TOKENS-STRUCT`), `CMD-PHRASE-COUNT` (`PHRASES-DOC`) and `CMD-PROTECTED-SPANS` (`SIGNATURES-PROTECTED`), and append to FEATURE/evidence/qa-gates/production-edit-scope.md.
  - Acceptance, all required: every `STRIPPED` value equals its required final value in `TOKENS-STRUCT`; every `PHRASE` value equals its required final value in `PHRASES-DOC`; every `SPAN-HASH` line reads `equal=True` (twelve signatures and two field declarations). Together with the P0-T4 base values this shows each token and phrase moving from its false-before to its true-after value. The artifact also records, from reading the formatted files with the Read tool, the line ranges of the two remaining `catch` clauses (one in `HandleToggleClickAsync`, one in `TryInvokeSink`) as `CATCH-SITES:`. Any mismatch is `EDIT SCOPE MISMATCH`: correct the edit against the delivered source with the Edit tool and re-run P1-T21 and this task; after two failed corrections stop.
- [ ] [P1-T23] Build TaskMaster.sln with `CMD-BUILD` (`TASKID` p1-t23) and record the build lines at the top of FEATURE/evidence/regression-testing/refusal-path-pass-after.md (this task creates the file).
  - Acceptance: `MSBUILD_EXIT_CODE: 0`, `ERRORS: 0`, `CSC_OUT_TASKMASTER:` at least 1 and `TEST_DLL_ADVANCED: True`.
- [ ] [P1-T24] Run the fixture against the fixed coordinator with `CMD-VSTEST` over TaskMaster.Test\bin\Debug\TaskMaster.Test.dll (`TASKID` p1-t24, `NAMES` `NAMES-ALL`) and append the result to FEATURE/evidence/regression-testing/refusal-path-pass-after.md.
  - Acceptance, all required: `VSTEST_EXIT_CODE: 0`; `SEQUENCE_FILES: 0`; `COUNTERS` total equal to `BASELINE-TOTAL` plus 4 (43), `passed` equal to `total`, `failed=0`; every `NEW-NAMES-964` and every `INVARIANT-NAMES` entry has a `RESULT ... = Passed` line; no `FAILED` line. Anything else is `PASS-AFTER NOT MET`: stop and report.
- [ ] [P1-T25] Verify that no existing test assertion in TaskMaster.Test/Ribbon was weakened or removed (AC4) with `CMD-TEST-DIFF`, and record FEATURE/evidence/qa-gates/test-partials-unchanged.md.
  - Acceptance, all required: the four `UNCHANGED` lines read `exit=0` (PrimeFaultOrdering, PrimeRegistration, ThrowingSink and RepeatFaultSuppression byte-equal to BASE-SHA); for `EngineToggleStateCoordinatorTests.Race.cs` `NON-DOC-CHANGES` is 0; for `EngineToggleStateCoordinatorTests.cs` the `DIFF` line shows `minus=1` and the single `MINUS` line reads `message => Notifications.Add(message),`. Any other result is `EXISTING TEST CHANGED`: stop.
- [ ] [P1-T26] Measure every coordinator source file under TaskMaster/Ribbon and every fixture partial under TaskMaster.Test/Ribbon (AC6) with `CMD-LINECOUNT` and `CMD-HASH`, check the csproj registrations, and record FEATURE/evidence/qa-gates/file-line-counts.md.
  - Command: `CMD-LINECOUNT`; `CMD-HASH`; the Grep tool counts of `Ribbon\x5CEngineToggleStateCoordinator\.` over `TaskMaster/TaskMaster.csproj` and of `Ribbon\x5CEngineToggleStateCoordinatorTests` over `TaskMaster.Test/TaskMaster.Test.csproj`.
  - Acceptance, all required: `PRODUCTION-FILES: 3` and every production `LINES` value at most 450 (expected about 307, 196 and 86 for the main, Prime and Messages files; observations only); `TEST-PARTIALS: 7` and every test `LINES` value at most 500 (primary fixture expected 481, Race 277, SinkGuard about 175); the production csproj has 3 coordinator compile entries and the test csproj 7 fixture entries, each file named by a `LINES` row registered exactly once. The `HASH` values are recorded as `PHASE1-HASHES:` for P2-T1. Any production file over 450 or test file over 500 is `FILE SIZE CEILING EXCEEDED`: stop.

### Phase 2 — Final QA Toolchain Loop, Coverage Delta, Scope and Acceptance

No code file is edited in Phase 2. If P2-T1 rewrites a Write Set file, the loop restarts once from P2-T1 (recorded as `PASS-2:` sections in the same artifacts); any other failure of P2-T1 to P2-T5 stops the plan with its artifact, and the fix returns to the orchestrator as a remediation round.

- [ ] [P2-T1] Apply repository-wide formatting from the worktree root (TaskMaster.sln tree, `.csharpierignore` applies) with `dotnet tool run csharpier format .` and record FEATURE/evidence/qa-gates/csharpier-format.md.
  - Command: `CMD-HASH` and `git -C WORKTREE status --porcelain --untracked-files=all` before the format; `pwsh -NoProfile -Command 'Set-Location -LiteralPath "WORKTREE"; [Environment]::CurrentDirectory = (Get-Location).Path; dotnet tool run csharpier format .; "CSHARPIER_EXIT_CODE: $LASTEXITCODE"'`; then `CMD-HASH` and the same porcelain command again.
  - Acceptance, all required: `EXIT_CODE: 0`; the `Formatted N files` line is recorded as a processed count, not an assertion; the before-and-after tree observation holds: every `HASH` value after the format equals the value before it (in pass 1 the before values must also equal `PHASE1-HASHES:`) and the two porcelain listings are identical. In pass 1 a differing Write Set hash restarts the loop once from this task as stated above, and pass 2 compares against the hashes recorded after the pass-1 format; a differing hash in pass 2 is `FORMAT NOT STABLE`: stop. A changed porcelain entry outside the Write Set is `FORMAT TOUCHED OUT-OF-SCOPE FILE`: stop.
- [ ] [P2-T2] Verify formatting read-only from the worktree root (TaskMaster.sln tree) with `dotnet tool run csharpier check .` and record FEATURE/evidence/qa-gates/csharpier-check-final.md.
  - Command: `pwsh -NoProfile -Command 'Set-Location -LiteralPath "WORKTREE"; [Environment]::CurrentDirectory = (Get-Location).Path; dotnet tool run csharpier check .; "CSHARPIER_EXIT_CODE: $LASTEXITCODE"'`
  - Acceptance: `EXIT_CODE: 0`, the `Checked N files` line is recorded, and no path is reported as unformatted.
- [ ] [P2-T3] Run the analyzer gate on TaskMaster.sln with `CMD-REBUILD` (`GATEARGS` `/p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`, `TASKID` p2-t3) and record FEATURE/evidence/qa-gates/msbuild-analyzer-final.md (`Command:` `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`).
  - Acceptance, all required: `EXIT_CODE: 0`; `ERRORS: 0`; both `CSC_OUT_` counts at least 1; `WRITESET_DIAGNOSTIC_LINES: 0`; `WARNINGS:` recorded beside `ANALYZER-BASELINE-WARNINGS:` as an observation.
- [ ] [P2-T4] Run the type-check gate on TaskMaster.sln with `CMD-REBUILD` (`GATEARGS` `/p:TreatWarningsAsErrors=true`, `TASKID` p2-t4) and record FEATURE/evidence/qa-gates/msbuild-nullable-final.md (`Command:` `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`).
  - Acceptance, all required: `EXIT_CODE: 0`; `ERRORS: 0`; both `CSC_OUT_` counts at least 1; `WRITESET_DIAGNOSTIC_LINES: 0`; `WARNINGS:` recorded beside `NULLABLE-BASELINE-WARNINGS:` as an observation.
- [ ] [P2-T5] Run the test-and-coverage gate with scripts/vscode/Invoke-MSTestWithCoverage.ps1 through `CMD-COVERAGE-RUNNER` (`STAGE` final) and then `CMD-COVERAGE-POST` (`STAGE` final, `RAW` per its rule), and record FEATURE/evidence/qa-gates/coverage-final.md with the same artifact layout as P0-T14 (numeric post-change figures in `Output Summary:`: the `First-party coverage:` line, the `ROOT` line, `COORD-LINES` and `COORD-LINE-RATE:`; `FINAL-FAILED-FQN-COUNT:` and every `FAILED-FQN` row).
  - Acceptance, all required: `DOCUMENT_PRESENT: True` and `TRX_PRESENT: True`; `LINE-FLOOR: MET`, `BRANCH-FLOOR: MET` and an empty `THRESHOLD_MESSAGE:`; the test-step rule of D-8 holds: `EXIT_CODE: 0`, or a non-zero exit with a non-empty `COLLECT_FAILURE_MESSAGE:`, `TEST-DEFINITIONS:` at least 1, `FAILED-FQN-COUNT:` at least 1, no `FAILED-FQN` value beginning `UNRESOLVED:` or `TaskMaster.Test.Ribbon.EngineToggleStateCoordinatorTests.`, and every `FAILED-FQN` value present verbatim among the `FAILED-FQN` rows of `FEATURE/evidence/baseline/coverage-baseline.md` (then `ExpectedExitCode:` equals the observed value and the artifact records `TEST-STEP: PASS (PRE-EXISTING FAILURES ONLY)`); every `COORD-FILE` row reads `nodes=` at least 1 (otherwise `PARTIAL CLASS ATTRIBUTION UNSUPPORTED`: stop); the summary first line begins `Test run outcome:`; the projection contains the `TaskMaster` package; no absolute path in the artifact. A `FAILED-FQN` value absent from the baseline rows is `NEW FAILING TEST`: stop without a re-run. `coverage\final-964.cobertura.xml` and `coverage\final-964.trx` stay on disk, git-ignored.
- [ ] [P2-T6] Compare baseline and post-change coverage for the coordinator files under TaskMaster/Ribbon and record FEATURE/evidence/qa-gates/coverage-comparison.md (sources: FEATURE/evidence/baseline/coverage-baseline.md and FEATURE/evidence/qa-gates/coverage-final.md).
  - Acceptance, all required: the artifact carries `Timestamp:`, `Command:` (the two source artifacts read), `EXIT_CODE: 0` and an `Output Summary:` with `BASELINE-FIRST-PARTY:` and `FINAL-FIRST-PARTY:` (repository line and branch percentages), `BASELINE-COORD-LINES:`, `FINAL-COORD-LINES:`, `BASELINE-COORD-LINE-RATE:`, `FINAL-COORD-LINE-RATE:`, the four final `METHOD` rows and `NEW-CODE-COVERAGE:` (the `TryInvokeSink` and `BuildNotifyFailedMessage` rates). Clauses: `FINAL-COORD-LINE-RATE` at least `BASELINE-COORD-LINE-RATE` (AC8; otherwise `COORDINATOR COVERAGE LOWERED`: stop); `METHOD TryInvokeSink` and `METHOD BuildNotifyFailedMessage` rate at least 90.00; `METHOD HandleToggleClickAsync` uncovered at most `BASELINE-METHOD-HTC-UNCOVERED:` and `METHOD CompletePrime` uncovered at most `BASELINE-METHOD-CP-UNCOVERED:` (changed lines not reduced); both final floors met. Each clause is recorded `MET` or `NOT MET` with its two values; any `NOT MET` stops.
- [ ] [P2-T7] Record the single clean toolchain pass of TaskMaster.sln (P2-T1 to P2-T5) in FEATURE/evidence/qa-gates/toolchain-final-pass.md.
  - Acceptance: the artifact carries `Timestamp:`, `Command:` listing the four CLAUDE.md commands verbatim in order (`dotnet tool run csharpier format .` with `dotnet tool run csharpier check .`; the analyzer `/t:Rebuild`; the `TreatWarningsAsErrors` `/t:Rebuild`; `Invoke-MSTestWithCoverage.ps1`), `EXIT_CODE: 0` and an `Output Summary:` naming each step's artifact and result, the pass number (1, or 2 after the admitted restart), and the D-8 test-step outcome.
- [ ] [P2-T8] Verify the change footprint of the worktree against BASE-SHA and the Write Set of docs/features/active/2026-10-01-engine-toggle-coordinator-947-review-residuals-964/plan.2026-10-02T05-20.md, and record FEATURE/evidence/qa-gates/footprint-scope.md.
  - Command: `git -C WORKTREE diff --name-only 94287369908cc920b21b0e3256314f988ad7d2f5`; `git -C WORKTREE status --porcelain --untracked-files=all`.
  - Acceptance, all required: every path in the diff listing and every porcelain entry is one of the eight Write Set code paths, lies under the feature folder, equals the promotion record path, lies under `.claude/agent-memory/` (ambient, never staged), or appears in `INHERITED-PORCELAIN:` of P0-T2; positive control: the union of the two listings contains all eight Write Set code paths (the three created files appear as `??` entries in the porcelain listing, the five modified files in both). Any other path is `FOOTPRINT EXCEEDS WRITE SET`: stop.
- [ ] [P2-T9] Run `CMD-HYGIENE` over every Markdown file under docs/features/active/2026-10-01-engine-toggle-coordinator-947-review-residuals-964 and record FEATURE/evidence/qa-gates/evidence-hygiene.md.
  - Acceptance: `FILES_SCANNED=` at least 33 (derivation: issue.md and this plan, 2; the 13 baseline, 5 regression-testing and 11 qa-gates evidence files of the Write Set other than evidence-hygiene.md, which this task writes after the sweep, 29; implementation-handoff.md, 1; the preparation-phase record, whose presence P0-T2 established, 1; 2 + 29 + 1 + 1 = 33), `ACCOUNT_HITS=0`, `MACHINE_HITS=0`, `DRIVE_USERS_HITS=0` and `RAW_DOCUMENTS=0`. A non-zero host count is attributed by the `HIT-FILE` rows, each naming one offending file as its parent directory name, a slash and its file name; it is repaired by replacing each occurrence with REDACTED-PATH in every file a `HIT-FILE` row names and re-running this task, except that a `HIT-FILE` row naming `other/` followed by the file name of `CLEARANCE-PATH:` is not repaired by this plan: it is `PREPARATION RECORD HOST HIT`: stop and report; a non-zero `RAW_DOCUMENTS` is repaired by deleting that copy from the feature folder (the original stays under `coverage/`).
- [ ] [P2-T10] Check off AC1 in `docs/features/active/2026-10-01-engine-toggle-coordinator-947-review-residuals-964/issue.md` per acceptance-criteria-tracking and append `AC1: MET` or `AC1: NOT MET` with its evidence to FEATURE/evidence/other/ac-status-summary.md (this task creates the file).
  - Acceptance: AC1 is checked (`- [ ] AC1 (` becomes `- [x] AC1 (`, no other text changed) only when P1-T14 shows `HandleToggleClickAsync_WhenNotifySinkThrowsWithNullEngines_DoesNotThrow` Failed with its two fragments, P1-T24 shows it Passed, and no P2-T5 `FAILED-FQN` row equals `TaskMaster.Test.Ribbon.EngineToggleStateCoordinatorTests.HandleToggleClickAsync_WhenNotifySinkThrowsWithNullEngines_DoesNotThrow`; otherwise it stays unchecked with the failing values recorded.
- [ ] [P2-T11] Check off AC2 in `docs/features/active/2026-10-01-engine-toggle-coordinator-947-review-residuals-964/issue.md` and append the result to FEATURE/evidence/other/ac-status-summary.md.
  - Acceptance: AC2 is checked only when P1-T24 shows `HandleToggleClickAsync_WhenNotifySinkThrowsWithNullEngines_LogsSinkExceptionOnceAndInvokesNothing` and `HandleToggleClickAsync_WhenNotifyAndLogSinksThrowWithNullEngines_DoesNotThrow` Passed and P1-T14 shows both Failed for their recorded reasons; otherwise unchecked.
- [ ] [P2-T12] Check off AC3 in `docs/features/active/2026-10-01-engine-toggle-coordinator-947-review-residuals-964/issue.md` and append the result to FEATURE/evidence/other/ac-status-summary.md.
  - Acceptance: AC3 is checked only when P1-T22 shows `"catch("` 2, `"catch(Exception)"` 0, `"TryInvokeSink("` 5 and each of the four call-site tokens 1, `CATCH-SITES:` names `HandleToggleClickAsync` and `TryInvokeSink` only, and P1-T25 shows the ThrowingSink partial `exit=0` with its four tests Passed at P1-T24; otherwise unchecked.
- [ ] [P2-T13] Check off AC4 in `docs/features/active/2026-10-01-engine-toggle-coordinator-947-review-residuals-964/issue.md` and append the result to FEATURE/evidence/other/ac-status-summary.md.
  - Acceptance: AC4 is checked only when P1-T8 and P1-T24 show every `INVARIANT-NAMES` entry Passed with `failed=0`, P1-T24 shows `GetPressed_WhenLogSinkThrowsOnFaultedPrime_SameFaultKindIsReportedAgain` Passed, P1-T25 met every clause, and P1-T22 shows every `SPAN-HASH` `equal=True` and `"failure),out_)){_reportedPrimeFaults[reportKey]=0;}"` 1; otherwise unchecked.
- [ ] [P2-T14] Check off AC5 in `docs/features/active/2026-10-01-engine-toggle-coordinator-947-review-residuals-964/issue.md` and append the result to FEATURE/evidence/other/ac-status-summary.md.
  - Acceptance: AC5 is checked only when P1-T22 shows `"The in-flight"`, `"most recently completed"` and `"The prime task, or"` 0 and `"The registration marker for an engine key"`, `"The marker is not the prime task itself"` and `"The registered marker, or"` 1, and a Read of the `GetPrimeTask` summary and returns in `TaskMaster/Ribbon/EngineToggleStateCoordinator.Prime.cs` confirms the text of delivered source F5; otherwise unchecked.
- [ ] [P2-T15] Check off AC6 in `docs/features/active/2026-10-01-engine-toggle-coordinator-947-review-residuals-964/issue.md` and append the result to FEATURE/evidence/other/ac-status-summary.md.
  - Acceptance: AC6 is checked only when P1-T26 met every clause, P1-T7 met its census, and P2-T3 and P2-T4 show `CSC_OUT_TASKMASTER:` at least 1 with `ERRORS: 0`; otherwise unchecked.
- [ ] [P2-T16] Check off AC7 in `docs/features/active/2026-10-01-engine-toggle-coordinator-947-review-residuals-964/issue.md` and append the result to FEATURE/evidence/other/ac-status-summary.md.
  - Acceptance: AC7 is checked only when every remaining `PHRASES-DOC` entry of P1-T22 holds its final value and a Read of the `HandleToggleClickAsync` summary and remarks, the `StartObservedPrime` remarks, the `CompletePrime` summary and remarks and the three constructor parameter docs confirms the delivered texts F1, F3, F6 and F7 against the code the comments describe; otherwise unchecked.
- [ ] [P2-T17] Check off AC8 in `docs/features/active/2026-10-01-engine-toggle-coordinator-947-review-residuals-964/issue.md` and append the result to FEATURE/evidence/other/ac-status-summary.md, followed by the AC status summary block of acceptance-criteria-tracking (source, total 8, checked, remaining, items remaining).
  - Acceptance: AC8 is checked only when P2-T7 records one clean pass of all four steps, P2-T5 met the D-8 test-step rule and both floors, and P2-T6 shows `FINAL-COORD-LINE-RATE` at least `BASELINE-COORD-LINE-RATE`; the AC8 line in the summary states the runner exit code and, when it is non-zero, the pre-existing `FAILED-FQN` values. Otherwise unchecked.
- [ ] [P2-T18] Hand off for the reduced (minor) audit and record FEATURE/evidence/other/reduced-audit-handoff.md.
  - Acceptance: the artifact carries `Timestamp:`, the AC source (`docs/features/active/2026-10-01-engine-toggle-coordinator-947-review-residuals-964/issue.md`), the AC status counts copied from ac-status-summary.md, the evidence paths of every Phase 0, Phase 1 and Phase 2 artifact, the D-7 list of folded-in related defects, the statement that no commit was created (D-10), and the reduced artifact checks for the auditor: the fail-before artifact, the pass-after artifact, the coverage comparison, the footprint gate and the hygiene gate (P2-T9 and its P2-T20 final sweep).
- [ ] [P2-T19] Confirm the `## Write Set` section of docs/features/active/2026-10-01-engine-toggle-coordinator-947-review-residuals-964/plan.2026-10-02T05-20.md enumerates every file the execution created or modified, and append the confirmation to FEATURE/evidence/other/reduced-audit-handoff.md under `WRITE-SET-CONFIRMATION:`.
  - Acceptance: every code path of P2-T8's union listing is named in the `## Write Set` code list and every one of the eight code entries appears in that union; every evidence file named in the `## Write Set` evidence list exists on disk (Glob tool over FEATURE/evidence); the Glob tool over FEATURE/evidence/other for `preflight-clearance.*.md` returns exactly one path which, converted to repository-relative forward-slash form, equals `CLEARANCE-PATH:` of P0-T2, and `git -C WORKTREE hash-object CLEARANCE-PATH` (a working-tree hash, so the check holds whether or not anything has been committed since P0-T2) prints the `CLEARANCE-BLOB:` value of P0-T2, recorded as `CLEARANCE-BLOB-FINAL:`; and no other file exists under FEATURE/evidence, so any file named neither in the evidence list nor as the preparation-phase record is a discrepancy. A discrepancy is recorded and leaves this task unchecked.
- [ ] [P2-T20] Re-run `CMD-HYGIENE` over every Markdown file under docs/features/active/2026-10-01-engine-toggle-coordinator-947-review-residuals-964 after the AC check-offs and the audit handoff, and append the output under `FINAL-SWEEP:` to FEATURE/evidence/qa-gates/evidence-hygiene.md.
  - Acceptance: `FILES_SCANNED=` at least 36 (derivation: issue.md and this plan, 2, plus the 33 evidence files of the Write Set — 13 baseline, 5 regression-testing, 12 qa-gates and 3 other — and the preparation-phase record, all of which exist once P2-T19 has passed; 2 + 33 + 1 = 36), `ACCOUNT_HITS=0`, `MACHINE_HITS=0`, `DRIVE_USERS_HITS=0` and `RAW_DOCUMENTS=0`. A non-zero host count is attributed by the `HIT-FILE` rows, each naming one offending file as its parent directory name, a slash and its file name; it is repaired by replacing each occurrence with REDACTED-PATH in every file a `HIT-FILE` row names and re-running this task, except that a `HIT-FILE` row naming `other/` followed by the file name of `CLEARANCE-PATH:` is not repaired by this plan: it is `PREPARATION RECORD HOST HIT`: stop and report, because P2-T19 requires that record unchanged; a non-zero `RAW_DOCUMENTS` is repaired by deleting that copy from the feature folder. This task creates no new evidence file, so the P2-T19 evidence-set confirmation still holds.
