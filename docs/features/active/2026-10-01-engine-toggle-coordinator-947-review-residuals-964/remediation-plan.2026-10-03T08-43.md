# 2026-10-01-engine-toggle-coordinator-947-review-residuals (Remediation Plan, cycle 1)

- **Issue:** #964
- **Parent (optional):** none
- **Owner:** drmoisan
- **Work Mode:** minor-audit (`docs/features/active/2026-10-01-engine-toggle-coordinator-947-review-residuals-964/issue.md` line 12 reads `- Work Mode: minor-audit`)
- **Last Updated:** 2026-10-03T08-43
- **Status:** Authored, awaiting preflight
- **Version:** 1.0 (initial authoring of remediation cycle 1)
- **Task counts (mechanical):** Phase 0 has 9 tasks (P0-T1 to P0-T9), Phase 1 has 8 (P1-T1 to P1-T8), Phase 2 has 16 (P2-T1 to P2-T16); 33 in total.
- **Plan path continuity:** this file is updated in place for every revision round. No timestamped sibling plan file is created for this cycle. The executed plan `plan.2026-10-02T05-20.md` is read-only context and is not edited.
- **Execution topology:** The executor for this plan must be dispatched without worktree isolation: the Bash-tool isolation guard refuses every `pwsh` invocation in an isolated agent, and every toolchain step in this plan is a pwsh payload.

**Fail-closed evidence rule:** every command-bearing task writes one evidence artifact carrying `Timestamp:`, `Command:`, `EXIT_CODE:` and `Output Summary:`. A task whose artifact is missing or incomplete stays unchecked, and the plan outcome is BLOCKED or INCOMPLETE, never PASS. Baseline, final-QC and coverage-comparison artifacts are mandatory; a missing one makes the audit verdict BLOCKED. A task that uses only the Edit, Read or Grep tools writes the same four fields, with `Command:` naming the tools and the patterns used and `EXIT_CODE: 0` when every acceptance clause of the task holds.

**Evidence accounting rule:** the artifact path is named in each task. Do not mark an evidence-bearing task complete without the artifact on disk at that exact path.

**Evidence location:** every artifact lives under `docs/features/active/2026-10-01-engine-toggle-coordinator-947-review-residuals-964/evidence/` in the canonical sub-kinds `remediation-baseline/`, `regression-testing/`, `qa-gates/` and `other/`. EVIDENCE_LOCATION_OVERRIDE_REJECTED: none supplied. In task text the token FEATURE abbreviates `docs/features/active/2026-10-01-engine-toggle-coordinator-947-review-residuals-964`, and the token CYCLE-BASE abbreviates the commit `6b8e935c177128d2f455f7bcd2fedc7deff6e30f`.

## Requirement sources

- Primary input: `docs/features/active/2026-10-01-engine-toggle-coordinator-947-review-residuals-964/remediation-inputs.2026-10-03T08-43.md`: finding R-1 (from CR-1), finding R-2 (from CR-4), and its `## Constraints` section, which binds this plan.
- Supporting context (read only): `docs/features/active/2026-10-01-engine-toggle-coordinator-947-review-residuals-964/code-review.2026-10-03T08-50.md` (CR-1, CR-4, O-1), `docs/features/active/2026-10-01-engine-toggle-coordinator-947-review-residuals-964/issue.md` section `## Acceptance Criteria` (AC1 to AC8, all eight already checked `- [x]`; this plan edits none of them and does not weaken any), the executed plan `plan.2026-10-02T05-20.md` (command definitions reused below) and `evidence/qa-gates/coverage-final.md` (coverage figures and the class-node read-out method).
- Cycle base: commit `6b8e935c177128d2f455f7bcd2fedc7deff6e30f` (CYCLE-BASE), the commit that opened remediation cycle 1 (last entry of the worktree reflog). Every `git diff` in this plan that measures the cycle uses that literal as its ref operand. The prior-cycle anchor `94287369908cc920b21b0e3256314f988ad7d2f5` (BASE-SHA of the executed plan) is used only as the ref of one negative control in P2-T8.
- No `spec.md`, `user-story.md` or `research.md` exists in the feature folder or is required; P0-T2 fails closed if one appears.

## AC identity table

| ID | Subject | Proved by |
|---|---|---|
| R-1 | Null and empty engine key on the refusal path is tested; the Messages partial class node reaches branch-rate 1 | `HandleToggleClickAsync_WithNullOrEmptyKeyAndNullEngines_NotifiesOnceWithNullTokenAndInvokesNothing` (two data rows) passing at P1-T6; class-node read-out at P2-T5 |
| R-2 | The both-sinks-throw test asserts `Engines.VerifyNoOtherCalls()` and an empty `Invalidations` | P1-T1 token counts; `HandleToggleClickAsync_WhenNotifyAndLogSinksThrowWithNullEngines_DoesNotThrow` passing at P1-T6; P1-T8 numstat shows zero deleted lines |
| CONSTRAINTS | Test-only change set, SinkGuard partial at most 500 lines, MSTest, Moq and FluentAssertions, no temp files and no sleeps, evidence under FEATURE/evidence only, no acceptance criterion edited | P1-T4 token gates, P1-T8 and P2-T8 footprint gates, P2-T9 line counts |
| AC8 | Full CLAUDE.md C# toolchain in one clean pass with coverage not lower | Phase 2 loop P2-T1 to P2-T7 |

## Verified tree facts (re-derived in this worktree for version 1.0; Phase 0 re-checks each one)

1. The worktree `.git` file names the gitdir of this worktree, whose `HEAD` is `ref: refs/heads/bug/engine-toggle-coordinator-947-review-residuals-964`; the last reflog entry of that gitdir moves HEAD to `6b8e935c177128d2f455f7bcd2fedc7deff6e30f` (`docs(964): open remediation cycle 1 for related review findings CR-1 and CR-4`).
2. `TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.SinkGuard.cs` is 169 lines. Usings at 1 to 5 (`System`, `System.Threading.Tasks`, `FluentAssertions`, `Microsoft.VisualStudio.TestTools.UnitTesting`, `Moq`); namespace `TaskMaster.Test.Ribbon` at 7; `public partial class EngineToggleStateCoordinatorTests` at 20; `#region Issue #964 — a throwing notification sink on the refusal path` at 22; three tests in it: `HandleToggleClickAsync_WhenNotifySinkThrowsWithNullEngines_DoesNotThrow` (31), `HandleToggleClickAsync_WhenNotifySinkThrowsWithNullEngines_LogsSinkExceptionOnceAndInvokesNothing` (52) and `HandleToggleClickAsync_WhenNotifyAndLogSinksThrowWithNullEngines_DoesNotThrow` (88, body 89 to 112); the matching `#endregion` at 114; blank 115; `#region Issue #964 — the issue #948 record placement under the shared guard` at 116; `GetPressed_WhenLogSinkThrowsOnFaultedPrime_SameFaultKindIsReportedAgain` at 126; that region's `#endregion` at 167; class and namespace close at 168 and 169. The sibling test at 52 to 79 ends with `harness.Engines.VerifyNoOtherCalls();` (77) and `harness.Invalidations.Should().BeEmpty("a refused click changes no state to display");` (78). The both-sinks test ends with the `Errors[0].Exception.Should().BeSameAs(` chain at 105 to 111, whose last two lines are `"the log sink receives the notification failure unchanged"` (110, 20 spaces) and `);` (111, 16 spaces), then `}` at 112.
3. `TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.cs` is 481 lines (O-1 of the code review; it is not edited by this plan). Namespace `TaskMaster.Test.Ribbon` at 9, `[TestClass]` at 22, `SpamEngine` constant at 25. Existing data-driven precedent at 101 to 107: `[DataTestMethod]`, `[DataRow(null)]`, `[DataRow("")]`, `[DataRow("   ")]` on a `string engineName` parameter. `private sealed class Harness` at 403; its members used by this plan: `internal Mock<IAppItemEngines> Engines { get; } =` (427, strict mock), `Coordinator` (430), `EnginesAvailable` (436, default true), `OnNotify` (456), `Invalidations` (458), `Notifications` (460), `Errors` (462). The notification sink records into `Notifications` and then invokes `OnNotify?.Invoke(message);` (414 to 418). `HandleToggleClickAsync_WithNullEngines_NotifiesOnceAndInvokesNothing` (355 to 372) already covers the non-null key with unavailable engines.
4. `TaskMaster/Ribbon/EngineToggleStateCoordinator.Messages.cs` is 86 lines. `private const string NullEngineNameToken = "(null)";` at 12; `RenderEngineName` at 17 to 20 with `return string.IsNullOrEmpty(engineName) ? NullEngineNameToken : engineName;` at 19 (the code review cites this arm as line 20; the file read in this pass places the ternary at line 19 and the closing brace at 20); `BuildUnavailableMessage` at 25 to 33 passes `RenderEngineName(engineName)` at 31 and renders `"The engine '{0}' is not available yet, ..."` with `CultureInfo.CurrentCulture`. The refusal path of `HandleToggleClickAsync` calls `BuildUnavailableMessage(engineName)`, so a null or empty key yields the text `The engine '(null)' is not available yet, ...`. This file is production code and is not edited by this plan.
5. `TaskMaster.Test/TaskMaster.Test.csproj` already registers `Ribbon\EngineToggleStateCoordinatorTests.SinkGuard.cs` (code review: line 364); no csproj edit is needed because the new test lives in an existing partial.
6. `TaskMaster.Test/packages.config`: `MSTest.TestAdapter` and `MSTest.TestFramework` 4.4.1 (43, 44), `FluentAssertions` 8.11.0 (7). Fixture census at CYCLE-BASE: 44 attribute lines of `[TestMethod]`, `[DataTestMethod]` and `[DataRow(` in the seven partials (40, 1 and 3), so 43 executed cases (the first-cycle pass-after run observed 43; 40 `[TestMethod]` plus 3 data rows, a data-driven method reporting one counter per row and no parent counter).
7. `evidence/qa-gates/coverage-final.md` (first-cycle final run): `COORD-FILE` rows for the main, Prime and Messages files read `nodes=1` with covered=valid of 89, 72 and 42 (line 89 to 91); `COORD-LINES covered=203 valid=203` (93); `COORD-BRANCHES covered=43 valid=44` (94); the summary reads `Total 7388, executed 7388, passed 7388, failed 0.` (37); the `First-party coverage:` line reads `lines 56629/65881 (85.96%), branches 13683/17082 (80.10%)` (13). The raw document `coverage/final-964.cobertura.xml` is still on disk (git-ignored): its Messages class node at line 230924 reads `<class line-rate="1" branch-rate="0.5" complexity="2" name="TaskMaster.EngineToggleStateCoordinator" filename="TaskMaster\Ribbon\EngineToggleStateCoordinator.Messages.cs">`, and the node's single method `RenderEngineName` reads `branch-rate="0.5"`. So the type aggregate 43/44 is 42/42 in the main and Prime files plus 1/2 in the Messages file, and the arm that closes the gap is the one at fact 4. A passing new test adds no production line or branch, so the expected final figures are `COORD-LINES covered=203 valid=203` and `COORD-BRANCHES covered=44 valid=44`.
8. `evidence/qa-gates/csharpier-check-final.md`: `Checked 1640 files in 5276ms.` and exit 0 on the first-cycle final tree. `.csharpierignore` excludes `**/evidence/**` (the feature folder is never formatted).
9. `scripts/vscode/Invoke-MSTestWithCoverage.ps1` and its helpers are unchanged since the executed plan: the runner prints `First-party coverage:` on success, writes the JaCoCo projection and the trx summary, retains the post-processed Cobertura document at `coverage\coverage.cobertura.xml`, and throws `MSTest with coverage failed with exit code N` on a non-zero collector exit before post-processing (executed plan, fact 8). The helper `Get-CoberturaClassLineSummary -ClassNode` returns `CoveredLines`, `TotalLines`, `CoveredBranches` and `TotalBranches` (the executed plan's `CMD-COVERAGE-POST` summed these into the observed `COORD-BRANCHES covered=43 valid=44`).
10. Hook behaviour that constrains payload wording (delegation prompt): `enforce-promotion-mcp-only.ps1` blocks a pwsh payload that contains the case-insensitive substrings `gh`, `issue` and `new` together. No payload in this plan contains the substring `issue`; `CMD-REBUILD`, `CMD-COVERAGE-POST` and `CMD-VSTEST` contain `new` (through `New-Object` and `New-Item`) and no `issue`, so the combination is absent. Where a payload cannot avoid one of the tokens the plan keeps the command and a hook denial is reported verbatim and stops that task (D-6).

## Design decisions (do not redesign)

- **D-1 Test-only remediation.** The only code file this cycle changes is `TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.SinkGuard.cs`. No production file, no csproj, no other test file and no `issue.md` line changes. The primary fixture stays at 481 lines (O-1).
- **D-2 R-1 test shape.** One data-driven test, `[DataTestMethod]` with `[DataRow(null)]` and `[DataRow("")]` on a `string engineName` parameter (the same attribute spelling the primary fixture uses at fact 3), in a new region of the SinkGuard partial. It reuses the `Harness` with `EnginesAvailable = false`, calls `HandleToggleClickAsync(engineName)`, and asserts: no throw, exactly one notification whose text contains `(null)`, no error logged, `Engines.VerifyNoOtherCalls()` and no invalidation. It uses the same Arrange-Act-Assert layout, because-reason style and `Func<Task>` plus `NotThrowAsync` form as the existing refusal-path tests.
- **D-3 R-2 edit shape.** The both-sinks-throw test gains the same two statements its sibling carries (fact 2), appended after its last assertion: `harness.Engines.VerifyNoOtherCalls();` and `harness.Invalidations.Should().BeEmpty("a refused click changes no state to display");`. No existing line changes, so the numstat of the whole cycle shows zero deleted lines in the file (P1-T8).
- **D-4 No fail-before run.** Both changes add assertions or coverage over behaviour that is already correct, so a failing run against unmodified production code is structurally impossible. The auditable substitute is a fail-before exception dossier (P1-T7) whose alternative proof is the false-before and true-after pair: the new test name has 0 hits before the edit (P0-T3) and the Messages class node reads branch-rate 0.5 before and 1 after (P0-T4, P2-T5).
- **D-5 Coverage route.** Step 4 runs `scripts/vscode/Invoke-MSTestWithCoverage.ps1` verbatim through `CMD-COVERAGE-RUNNER` with the stage label fixed to `remediation`, so the raw documents land at `coverage\remediation-964.cobertura.xml` and `coverage\remediation-964.trx` and do not overwrite `coverage\final-964.cobertura.xml`, which P2-T5 uses as the false-before control. The test step passes only when the runner exits 0 with `FAILED-FQN-COUNT: 0`: the first-cycle final run had no failing test (7388 of 7388), the change adds only passing tests, so a failure is a new defect and stops the plan. `CMD-COVERAGE-POST` here omits the raw-document conversion branch (a completed runner has already post-processed the document in place) and omits the per-method loop (no production method changes in this cycle); it adds per-file branch totals and the per-class `line-rate` and `branch-rate` attributes, which are the quantities R-1 targets. Every added row is cross-checked in P2-T5 by a Grep read of the same class node in the raw document, a format already observed at fact 7.
- **D-6 Edit route and hooks.** Every `.cs` change is made with the Edit tool, never through a shell write, so the repository hooks see it. A hook denial of any Edit, Write or shell command is reported verbatim and stops that task; the executor does not retry with a rephrased command or another tool. Edit anchors are chosen to be unique in the file; if the Edit tool reports a missing or non-unique match, the executor re-reads the region and repeats with the text actually present, which is not a hook denial.
- **D-7 Commit and push (final tasks).** The plan ends with staging by explicit path (never `git add -A`, because a queued sibling's untracked files or ambient `.claude/agent-memory/` files would be swept in), a commit and a push of the branch. `.claude/agent-memory/` paths are ambient state of other sessions, are never staged, and are admitted by every footprint gate. The commit-record artifact is written before staging so that it is part of the commit; the commit hash and push output are post-commit observations reported in the executor's return and are not written into a file that would reopen the tree.
- **D-8 Restart semantics.** If P2-T1 rewrites the SinkGuard partial (its hash changes) the loop restarts once from P2-T1 and the artifacts record `PASS-2:` sections; a second rewrite is `FORMAT NOT STABLE`: stop. Any other failure of P2-T1 to P2-T5, or any file change by a step after P2-T1, stops the plan with its artifact and returns to the orchestrator as a further remediation round, which re-enters at P2-T1 after the fix (restart from step 1).

## Write Set (every file this plan creates or modifies)

Code files (the only path outside the feature folder this plan may change):

- `TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.SinkGuard.cs` (modify: one appended pair of assertions, one new region)

Feature documents:

- `docs/features/active/2026-10-01-engine-toggle-coordinator-947-review-residuals-964/remediation-plan.2026-10-03T08-43.md` (task check-off edits only)

Evidence files (fixed names; the write time is the `Timestamp:` field), all under FEATURE/evidence/:

- `remediation-baseline/`: `phase0-instructions-read.md`, `scope-and-anchor.md`, `sinkguard-partial-baseline.md`, `coverage-baseline.md`, `bootstrap-probe.md`, `csharpier-check-baseline.md`, `msbuild-analyzer-baseline.md`, `msbuild-nullable-baseline.md`, `coordinator-tests-baseline.md` (nine)
- `regression-testing/`: `cycle1-r2-edit.md`, `cycle1-r1-edit.md`, `cycle1-format.md`, `cycle1-token-gates.md`, `cycle1-build.md`, `cycle1-fixture-run.md`, `fail-before-exception.<yyyy-MM-ddTHH-mm>.md`, `cycle1-sinkguard-diff.md` (eight; the dossier name carries the host-clock time of its write)
- `qa-gates/`: `cycle1-csharpier-format.md`, `cycle1-csharpier-check.md`, `cycle1-msbuild-analyzer.md`, `cycle1-msbuild-nullable.md`, `cycle1-coverage.md`, `cycle1-coverage-comparison.md`, `cycle1-toolchain-pass.md`, `cycle1-footprint.md`, `cycle1-line-counts.md`, `cycle1-evidence-hygiene.md` (ten)
- `other/`: `cycle1-finding-closure.md`, `cycle1-reduced-audit-handoff.md`, `cycle1-commit-record.md` (three)
- Preparation-phase records (committed by the coordinator before execution; not written, modified or deleted by any task of this plan): any file `other/preflight-clearance.<yyyy-MM-ddTHH-mm>.md`.

Files this plan must not touch: every other file under `TaskMaster/` and `TaskMaster.Test/` (in particular `TaskMaster/Ribbon/EngineToggleStateCoordinator.cs`, `.Prime.cs`, `.Messages.cs`, both csproj files and the partials `EngineToggleStateCoordinatorTests.cs`, `.Race.cs`, `.PrimeFaultOrdering.cs`, `.PrimeRegistration.cs`, `.ThrowingSink.cs`, `.RepeatFaultSuppression.cs`), `docs/features/active/2026-10-01-engine-toggle-coordinator-947-review-residuals-964/issue.md`, the executed plan, `docs/features/potential/`, every `packages.config`, every file under `scripts/`, `.claude/rules/`, `config/` and `artifacts/`, `TaskMaster.runsettings`, `coverage.config`, `.editorconfig`, `.gitignore` and `.csharpierignore`. No raw trx, raw Cobertura document or msbuild log is copied into the feature folder under any name; raw documents stay under the git-ignored `coverage/` directory.

## Delivered source (the executor writes these texts; CSharpier output wins on layout, and every gate reads wrap-tolerant tokens)

Indentation rule: every block below is shown with four leading spaces of Markdown indent on each line; remove exactly those four spaces on every line when writing. The line numbers cited are those of the SinkGuard partial at CYCLE-BASE (fact 2).

**E1 — R-2, `HandleToggleClickAsync_WhenNotifyAndLogSinksThrowWithNullEngines_DoesNotThrow` (Edit tool).** Replace the three-line text `OLD-E1` with `NEW-E1`. `OLD-E1` occurs exactly once in the file: the same `BeSameAs` tail inside the sibling test is followed by the `harness.Errors[0].Message` line, not by a closing brace.

OLD-E1:

                        "the log sink receives the notification failure unchanged"
                    );
            }

NEW-E1:

                        "the log sink receives the notification failure unchanged"
                    );
                harness.Engines.VerifyNoOtherCalls();
                harness.Invalidations.Should().BeEmpty("a refused click changes no state to display");
            }

**E2 — R-1, new region (Edit tool).** Replace the single line `OLD-E2` (it occurs exactly once; the `#endregion` line of the same region begins `#endregion`, which does not contain the text `#region`) with `NEW-E2`, which is the new region, one blank line and the original line.

OLD-E2:

            #region Issue #964 — the issue #948 record placement under the shared guard

NEW-E2:

            #region Issue #964 — the refusal path with a null or empty engine key

            /// <summary>
            /// Refusal path for an unusable engine key: with the engines unavailable, a null or empty
            /// key is rendered as the <c>(null)</c> token in the one notification, the click does not
            /// throw, nothing is logged, no engine member is invoked and no control is invalidated.
            /// Exercises the null-or-empty arm of the engine-name renderer through the notification
            /// message builder.
            /// </summary>
            [DataTestMethod]
            [DataRow(null)]
            [DataRow("")]
            public async Task HandleToggleClickAsync_WithNullOrEmptyKeyAndNullEngines_NotifiesOnceWithNullTokenAndInvokesNothing(
                string engineName
            )
            {
                // Arrange: the pre-SetGlobals window, with sinks that record and do not throw.
                var harness = new Harness { EnginesAvailable = false };

                // Act
                Func<Task> act = () => harness.Coordinator.HandleToggleClickAsync(engineName);

                // Assert
                await act.Should()
                    .NotThrowAsync("a refused click with an unusable key must degrade quietly");
                harness
                    .Notifications.Should()
                    .ContainSingle("exactly one notice per refused toggle click");
                harness
                    .Notifications[0].Should()
                    .Contain("(null)", "an unusable key is rendered as the null-engine-name token");
                harness.Errors.Should().BeEmpty("a refused click is not a fault");
                harness.Engines.VerifyNoOtherCalls();
                harness.Invalidations.Should().BeEmpty("a refused click changes no state to display");
            }

            #endregion Issue #964 — the refusal path with a null or empty engine key

            #region Issue #964 — the issue #948 record placement under the shared guard

Expected size after both edits and the format: 169 plus 2 plus 38, about 209 lines (an observation; the gates are the bounds in P1-T4 and P2-T9).

## Name lists

- `R1-NAME`: `HandleToggleClickAsync_WithNullOrEmptyKeyAndNullEngines_NotifiesOnceWithNullTokenAndInvokesNothing` (a data-driven method that reports two results, one per row).
- `SINKGUARD-NAMES` (existing, all four): `HandleToggleClickAsync_WhenNotifySinkThrowsWithNullEngines_DoesNotThrow`, `HandleToggleClickAsync_WhenNotifySinkThrowsWithNullEngines_LogsSinkExceptionOnceAndInvokesNothing`, `HandleToggleClickAsync_WhenNotifyAndLogSinksThrowWithNullEngines_DoesNotThrow` (this one is `R2-NAME`), `GetPressed_WhenLogSinkThrowsOnFaultedPrime_SameFaultKindIsReportedAgain`.
- `INVARIANT-NAMES` (existing, eleven): `GetPressed_WhenPrimeStarts_RegistersPrimeHandleBeforeActivationReadRuns`, `GetPressed_AfterPrimeFaultsSynchronously_LaterReadStartsNewPrime`, `GetPressed_AfterPrimeIsCanceledSynchronously_LaterReadStartsNewPrime`, `GetPressed_WhenPrimeFaults_PrimeHandleStaysRegisteredUntilFaultIsLogged`, `GetPressed_WhenLogSinkThrowsOnFaultedPrime_LaterReadStartsNewPrime`, `GetPressed_WhenLogSinkThrowsOnCanceledPrime_LaterReadStartsNewPrime`, `GetPressed_WhenLogSinkThrows_FirstPrimeCompletesAndMarkerIsCleared`, `HandleToggleClickAsync_WhenLogSinkThrowsOnToggleFault_DoesNotThrowAndAttemptsReport`, `GetPressed_WhenPrimeFaultsRepeatedly_LogsFirstFaultOnly`, `GetPressed_WhenPrimeIsCanceledRepeatedly_LogsFirstCancellationOnly`, `HandleToggleClickAsync_WithNullEngines_NotifiesOnceAndInvokesNothing`.
- `NAMES-CYCLE` is `R1-NAME` followed by `SINKGUARD-NAMES` and `INVARIANT-NAMES`, written as a PowerShell list of double-quoted strings when substituted into `CMD-VSTEST`.
- Expected fixture totals: `BASELINE-TOTAL: 43` before the edits and `FIXTURE-TOTAL: 45` after them (43 plus the two data rows).

## Execution conventions

- **Paths.** `WORKTREE` denotes the absolute item worktree path from the delegation prompt; it is substituted into each payload's first line and is never written into an artifact. Every artifact records repository-relative paths; absolute paths in transcribed tool output are replaced by `REDACTED-PATH`.
- **Payload channel.** Each indented payload block is executed as one `pwsh -NoProfile -Command '<payload>'` Bash invocation (the only permitted shell form besides `git`): outer single quotes, inner double quotes only; a double quote needed inside a payload string is built from `[char]34` and an apostrophe from `[char]39`; no payload carries a backslash-escaped double quote, a literal apostrophe or a backtick. No payload combines git with the substrings add, commit or remove, and no payload contains all of gh, pr and create, because the hook command scanners match those by case-insensitive containment. No payload contains the substring `issue` (fact 10). The `Command:` field of an artifact records the canonical command the payload runs, not the payload text.
- **Git commands outside payloads** run as `git -C WORKTREE <arguments>` (one command per Bash invocation, no chaining); the `Command:` field records them without `-C`.
- **Toolchain commands.** Every `dotnet`, `msbuild` and `vstest.console.exe` command named in a task runs inside one `pwsh -NoProfile -Command` payload that begins with PRELUDE, because the first token of a permitted Bash command must be `git`, `pwsh` or `poetry`. A task that names only the tool command (for example `dotnet tool run csharpier check .`) means that command wrapped as `pwsh -NoProfile -Command 'Set-Location -LiteralPath "WORKTREE"; [Environment]::CurrentDirectory = (Get-Location).Path; <command>; "EXIT: $LASTEXITCODE"'`.
- **Grep tool patterns.** Every pattern this plan gives to the Grep tool is a regular expression with its metacharacters escaped as written: `\(`, `\)`, `\[`, `\]`, `\.`, `\?`, and a literal backslash as `\x5C`. A pattern written without any metacharacter is a plain phrase. The Grep tool counts matching lines. A Grep over a git-ignored file names that file as the path (an explicit file path is read; a directory path is filtered by `.gitignore`).
- **PRELUDE** (the first two lines of every payload):

        Set-Location -LiteralPath "WORKTREE"
        [Environment]::CurrentDirectory = (Get-Location).Path

- **TOOLS** (the lines of every build and test payload after PRELUDE):

        $vswhere = Join-Path ${env:ProgramFiles(x86)} "Microsoft Visual Studio\Installer\vswhere.exe"
        $msbuild = & $vswhere -latest -products * -find "MSBuild\**\Bin\MSBuild.exe" | Select-Object -First 1
        $vstest = & $vswhere -latest -products * -find "Common7\IDE\Extensions\TestPlatform\vstest.console.exe" | Select-Object -First 1
        $sln = Join-Path (Get-Location).Path "TaskMaster.sln"
        New-Item -ItemType Directory -Path "coverage\logs" -Force | Out-Null

- **Exit codes.** `EXIT_CODE:` records the printed exit value of the principal command. A deliberately or admissibly failing run carries `ExpectedExitCode:` equal to the observed non-zero value in its own artifact; no task of this plan expects a non-zero exit.
- **Long runs.** `CMD-COVERAGE-RUNNER` is started as a background process with output redirected to `coverage\logs\remediation-964.payload.log`; completion is detected by the final line `PAYLOAD-COMPLETE`. Before every vstest or coverage run the executor runs `pwsh -NoProfile -Command '"STRAY_TEST_PROCESSES: " + @(Get-Process -ErrorAction SilentlyContinue | Where-Object { $_.ProcessName -like "vstest*" -or $_.ProcessName -like "testhost*" -or $_.ProcessName -like "dotnet-coverage*" }).Count'` and proceeds only on `STRAY_TEST_PROCESSES: 0`. A coverage run still in progress after 120 minutes is `COVERAGE RUN STALLED`: stop and report; it is never re-run with a different route.
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
    Write-Output ("WRITESET_DIAGNOSTIC_LINES: " + @($lines | Where-Object { ($_.Contains("EngineToggleStateCoordinatorTests.SinkGuard")) -and ($_ -match "(error|warning) [A-Z]+\d+") }).Count)
    Write-Output ("COORDINATOR_DIAGNOSTIC_LINES: " + @($lines | Where-Object { ($_.Contains("EngineToggleStateCoordinator")) -and ($_ -match "(error|warning) [A-Z]+\d+") }).Count)
    Write-Output ("TEST_DLL_EXISTS: " + (Test-Path -LiteralPath "TaskMaster.Test\bin\Debug\TaskMaster.Test.dll"))

`ERRORS:` is read from the summary line, so `0 Error(s)` is never matched inside a larger count. The two `CSC_OUT_` counts are the observation that the compiler ran for the two projects. `WRITESET_DIAGNOSTIC_LINES` counts every error or warning line naming the one Write Set source file; `COORDINATOR_DIAGNOSTIC_LINES` counts every error or warning line naming any coordinator production or fixture file (the executed plan's `WRITESET_DIAGNOSTIC_LINES` definition), so a diagnostic in a file this cycle does not touch is still visible.

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

**CMD-VSTEST** (coordinator fixture run; `TASKID` and `NAMES` substituted; `Command:` records `vstest.console.exe TaskMaster.Test\bin\Debug\TaskMaster.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation "/TestCaseFilter:FullyQualifiedName~TaskMaster.Test.Ribbon.EngineToggleStateCoordinatorTests" "/ResultsDirectory:coverage\test-results\964\TASKID" "/Logger:trx;LogFileName=TASKID.trx" "/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None"` resolved through vswhere; a run whose filter matches zero tests is a failure). The `RESULT` line is the executed plan's line adapted for data-driven methods: it counts every result whose `testName` equals the method name or begins with the method name followed by ` (`, which is how a data row is named:

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
    foreach ($n in $names) { $rows = @($all | Where-Object { $_.GetAttribute("testName") -eq $n -or $_.GetAttribute("testName").StartsWith($n + " (") }); Write-Output ("RESULT " + $n + " rows=" + $rows.Count + " passed=" + @($rows | Where-Object { $_.GetAttribute("outcome") -eq "Passed" }).Count) }
    foreach ($r in $all) { if ($r.GetAttribute("outcome") -eq "Failed") { Write-Output ("FAILED " + $r.GetAttribute("testName")); $msg = $r.SelectSingleNode("t:Output/t:ErrorInfo/t:Message", $ns); Write-Output ("MESSAGE " + $r.GetAttribute("testName") + " :: " + $(if ($msg) { $msg.InnerText.Replace([string][char]13, " ").Replace([string][char]10, " / ") } else { "(no message)" })) } }

The artifact transcribes the `COUNTERS`, `RESULT`, `FAILED` and `MESSAGE` lines; the trx stays under the ignored coverage directory.

**CMD-COVERAGE-RUNNER** (CLAUDE.md step 4, stage label `remediation`; `Command:` records `pwsh -NoProfile -File scripts\vscode\Invoke-MSTestWithCoverage.ps1` invoked by absolute script path from the worktree directory):

    PRELUDE
    New-Item -ItemType Directory -Path "coverage\logs" -Force | Out-Null
    foreach ($f in @("coverage\coverage.cobertura.xml", "coverage\coverage.cobertura.jacoco.xml", "coverage\test-results\mstest-coverage-run.trx", "coverage\test-results\mstest-coverage-run.summary.txt", "coverage\remediation-964.cobertura.xml", "coverage\remediation-964.trx")) { if (Test-Path -LiteralPath $f) { Remove-Item -LiteralPath $f -Force } }
    $script = Join-Path (Get-Location).Path "scripts\vscode\Invoke-MSTestWithCoverage.ps1"
    $global:LASTEXITCODE = 0
    & pwsh -NoProfile -File $script 2>&1 | Tee-Object -FilePath "coverage\logs\remediation-964.runner.log" | Out-Null
    Write-Output ("RUNNER_EXIT_CODE: " + $LASTEXITCODE)
    $log = Get-Content -LiteralPath "coverage\logs\remediation-964.runner.log" -Raw -Encoding UTF8
    Write-Output ("DISCOVERED_LINE: " + [regex]::Match($log, "Discovered \d+ test assemblies\.").Value)
    Write-Output ("FIRST_PARTY_LINE: " + [regex]::Match($log, "First-party coverage: [^\r\n]*").Value)
    Write-Output ("THRESHOLD_MESSAGE: " + [regex]::Match($log, "Cobertura (line|branch) coverage [^\r\n]*threshold[^\r\n]*").Value)
    Write-Output ("COLLECT_FAILURE_MESSAGE: " + [regex]::Match($log, "MSTest with coverage failed with exit code \d+").Value)
    Write-Output ("DOCUMENT_PRESENT: " + (Test-Path -LiteralPath "coverage\coverage.cobertura.xml"))
    Write-Output ("TRX_PRESENT: " + (Test-Path -LiteralPath "coverage\test-results\mstest-coverage-run.trx"))
    Write-Output ("SUMMARY_FILE_PRESENT: " + (Test-Path -LiteralPath "coverage\test-results\mstest-coverage-run.summary.txt"))
    if (Test-Path -LiteralPath "coverage\coverage.cobertura.xml") { Copy-Item -LiteralPath "coverage\coverage.cobertura.xml" -Destination "coverage\remediation-964.cobertura.xml" -Force }
    if (Test-Path -LiteralPath "coverage\test-results\mstest-coverage-run.trx") { Copy-Item -LiteralPath "coverage\test-results\mstest-coverage-run.trx" -Destination "coverage\remediation-964.trx" -Force }
    Write-Output "PAYLOAD-COMPLETE"

The stale-output removal makes every `_PRESENT` value an observation of this run. Lines of the runner log that carry absolute paths stay in the ignored log; only the named values are transcribed.

**CMD-COVERAGE-POST** (run only after a runner exit of 0, so the document is already post-processed; adapted from the executed plan as described in D-5: no raw-conversion line, no per-method loop, and per-file branch totals plus per-class rate attributes added):

    PRELUDE
    . (Join-Path (Get-Location).Path "scripts\vscode\Invoke-MSTestWithCoverage.Helpers.ps1")
    . (Join-Path (Get-Location).Path "scripts\vscode\Invoke-MSTest.TrxSummary.ps1")
    $ErrorActionPreference = "Continue"
    $summary = Get-TrxRunSummary -TrxContent (Get-Content -LiteralPath "coverage\remediation-964.trx" -Raw -Encoding UTF8)
    Write-Output "SUMMARY-BEGIN"
    Write-Output (Format-TrxRunSummary -Summary $summary)
    Write-Output "SUMMARY-END"
    Write-Output ("FAILED-SET: " + (@($summary.FailedTestName) -join ", "))
    [xml]$trxXml = Get-Content -LiteralPath "coverage\remediation-964.trx" -Raw -Encoding UTF8
    $tns = New-Object System.Xml.XmlNamespaceManager($trxXml.NameTable)
    $tns.AddNamespace("t", "http://microsoft.com/schemas/VisualStudio/TeamTest/2010")
    $defs = @{}
    foreach ($u in @($trxXml.SelectNodes("//t:TestDefinitions/t:UnitTest", $tns))) { $tm = $u.SelectSingleNode("t:TestMethod", $tns); $defs[$u.GetAttribute("id")] = $tm.GetAttribute("className").Split([char]44)[0].Trim() + "." + $tm.GetAttribute("name") }
    Write-Output ("TEST-DEFINITIONS: " + $defs.Count)
    $fqn = @(@($trxXml.SelectNodes("//t:Results/t:UnitTestResult", $tns)) | Where-Object { $_.GetAttribute("outcome") -eq "Failed" } | ForEach-Object { $id = $_.GetAttribute("testId"); if ($defs.ContainsKey($id)) { $defs[$id] } else { "UNRESOLVED:" + $id } } | Sort-Object -Unique -CaseSensitive)
    Write-Output ("FAILED-FQN-COUNT: " + $fqn.Count)
    foreach ($n in $fqn) { Write-Output ("FAILED-FQN " + $n) }
    $doc = Get-Content -LiteralPath "coverage\remediation-964.cobertura.xml" -Raw -Encoding UTF8
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
        $fc = 0; $fv = 0; $fcb = 0; $fvb = 0
        foreach ($c in $nodes) { $s = Get-CoberturaClassLineSummary -ClassNode $c; $fc += $s.CoveredLines; $fv += $s.TotalLines; $fcb += $s.CoveredBranches; $fvb += $s.TotalBranches; Write-Output ("CLASS-NODE " + $norm + " line-rate=" + $c.GetAttribute("line-rate") + " branch-rate=" + $c.GetAttribute("branch-rate")) }
        $nodesTotal += $nodes.Count; $cv += $fc; $vl += $fv; $cb += $fcb; $vb += $fvb
        Write-Output ("COORD-FILE " + $norm + " nodes=" + $nodes.Count + " covered=" + $fc + " valid=" + $fv + " branches-covered=" + $fcb + " branches-valid=" + $fvb)
    }
    Write-Output ("COORD-CLASS-NODES: " + $nodesTotal)
    Write-Output ("COORD-LINES covered=" + $cv + " valid=" + $vl)
    Write-Output ("COORD-BRANCHES covered=" + $cb + " valid=" + $vb)
    Write-Output ("COORD-LINE-RATE: " + $(if ($vl -gt 0) { [math]::Round(100.0 * $cv / $vl, 2) } else { "NA" }))
    Write-Output ("COORD-BRANCH-RATE: " + $(if ($vb -gt 0) { [math]::Round(100.0 * $cb / $vb, 2) } else { "NA" }))

The summary block and the projection are the two CLAUDE.md committed forms; every other line is a figure, not a document. `FAILED-SET:` (short method names, executed plan fact 14) is an observation only; the failing-test gate reads `TEST-DEFINITIONS:`, `FAILED-FQN-COUNT:` and the `FAILED-FQN` rows, which are fully qualified names.

**CMD-LINECOUNT** (line counts of every fixture partial present, enumerated from the directory; adapted from the executed plan to the test partials, the only files this cycle may change):

    PRELUDE
    $files = @(Get-ChildItem -LiteralPath "TaskMaster.Test\Ribbon" -File -Filter "EngineToggleStateCoordinatorTests*.cs" | Sort-Object Name | ForEach-Object { Join-Path "TaskMaster.Test\Ribbon" $_.Name })
    foreach ($p in $files) { Write-Output ("LINES " + $p + " = " + @(Get-Content -LiteralPath $p -Encoding UTF8).Count) }
    Write-Output ("TEST-PARTIALS: " + $files.Count)

**CMD-HASH-SINKGUARD** (SHA-256 of the one Write Set code file; a hash only):

    PRELUDE
    $p = "TaskMaster.Test\Ribbon\EngineToggleStateCoordinatorTests.SinkGuard.cs"
    if (Test-Path -LiteralPath $p) { Write-Output ("HASH " + $p + " = " + (Get-FileHash -Algorithm SHA256 -LiteralPath $p).Hash) } else { Write-Output ("HASH " + $p + " = ABSENT") }

**CMD-HYGIENE** (host-identifier sweep over every Markdown file of the feature folder; the host tokens are derived at run time and never written into the artifact):

    PRELUDE
    $acct = Split-Path -Leaf $env:USERPROFILE; $machine = $env:COMPUTERNAME
    $files = @(Get-ChildItem -LiteralPath "docs\features\active\2026-10-01-engine-toggle-coordinator-947-review-residuals-964" -Recurse -File -Filter "*.md")
    $a = 0; $m = 0; $d = 0
    foreach ($f in $files) { $c = Get-Content -LiteralPath $f.FullName -Raw -Encoding UTF8; $fa = ([regex]::Matches($c, [regex]::Escape($acct), "IgnoreCase")).Count; $fm = ([regex]::Matches($c, [regex]::Escape($machine), "IgnoreCase")).Count; $n = $c.Replace([string][char]92, "/"); $fd = ([regex]::Matches($n, "[a-z]:/+users/+[a-z0-9_.~-]", "IgnoreCase")).Count; $a += $fa; $m += $fm; $d += $fd; if (($fa + $fm + $fd) -gt 0) { Write-Output ("HIT-FILE " + $f.Directory.Name + "/" + $f.Name + " ACCOUNT=" + $fa + " MACHINE=" + $fm + " DRIVE_USERS=" + $fd) } }
    $raw = @(Get-ChildItem -LiteralPath "docs\features\active\2026-10-01-engine-toggle-coordinator-947-review-residuals-964" -Recurse -File | Where-Object { $_.Name -like "*.trx" -or $_.Name -like "*cobertura*" -or $_.Name -like "*.coverage" -or $_.Name -like "*.coveragexml" -or $_.Name -like "*.log" }).Count
    Write-Output ("FILES_SCANNED=" + $files.Count + " ACCOUNT_HITS=" + $a + " MACHINE_HITS=" + $m + " DRIVE_USERS_HITS=" + $d + " RAW_DOCUMENTS=" + $raw)

## Token and size sets (quoted verbatim; each is the instruction the delivered source above fulfils)

- `TOKENS-R2` (Grep counts over `TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.SinkGuard.cs`; false-before value, then required true-after value): `harness\.Engines\.VerifyNoOtherCalls\(\);` 1 then 3 (the sibling test, the both-sinks test after E1, and the new R-1 test after E2, which carries the same statement; 2 after E1 alone); `harness\.Invalidations\.Should\(\)\.BeEmpty\("a refused click changes no state to display"\);` 1 then 3 (same three tests; 2 after E1 alone); `Engines\.VerifyNoOtherCalls` 1 then 3.
- `TOKENS-R1` (same file; false-before value, then required true-after value): `\[DataTestMethod\]` 0 then 1; `\[DataRow\(null\)\]` 0 then 1; `\[DataRow\(""\)\]` 0 then 1; `HandleToggleClickAsync_WithNullOrEmptyKeyAndNullEngines_NotifiesOnceWithNullTokenAndInvokesNothing` 0 then 1; `\[TestMethod\]` 4 then 4; `#region ` 2 then 3; `#endregion ` 2 then 3.
- `TOKENS-FORBIDDEN` (same file, required 0 before and after): `Thread\.Sleep`, `Task\.Delay`, `DoNotParallelize`, `GetTempPath`, `File\.`, `DateTime\.Now`, `DateTime\.UtcNow`. Positive control that the file was read: `TaskCompletionSource` at least 2.
- `SIZE-BOUNDS` (Grep count of pattern `^` over the SinkGuard partial): 169 before; after the edits and the format at least 195 and at most 500.

### Phase 0 — Policy Reads, Anchor and Baseline Capture

- [x] [P0-T1] Read the policy documents in the mandatory order — CLAUDE.md, then .claude/rules/general-code-change.md, then .claude/rules/general-unit-test.md, then .claude/rules/csharp.md — plus .claude/rules/tonality.md and .claude/rules/plan-acceptance-gates.md, and record FEATURE/evidence/remediation-baseline/phase0-instructions-read.md.
  - Acceptance: the artifact carries `Timestamp:`, a `Policy Order:` line naming CLAUDE.md, general-code-change.md, general-unit-test.md and csharp.md in that order, and a `Files read:` list naming all six repository-relative paths, one per line. No policy document is modified.
- [x] [P0-T2] Read FEATURE/remediation-inputs.2026-10-03T08-43.md in full, the `## Acceptance Criteria` section of FEATURE/issue.md and this plan, and record the requirements anchor and the Write Set in FEATURE/evidence/remediation-baseline/scope-and-anchor.md.
  - Command: `git -C WORKTREE status --porcelain --untracked-files=all` (recorded verbatim as `INHERITED-PORCELAIN:`); `git -C WORKTREE merge-base 6b8e935c177128d2f455f7bcd2fedc7deff6e30f HEAD`; `git -C WORKTREE diff --exit-code --stat 6b8e935c177128d2f455f7bcd2fedc7deff6e30f -- TaskMaster TaskMaster.Test`; the Grep tool over `FEATURE/issue.md` with patterns `^- Work Mode: minor-audit`, `^## Acceptance Criteria`, `^- \[x\] AC[1-8] ` and `^- \[ \] AC`; the Glob tool over FEATURE for `spec.md`, `user-story.md` and `research*.md`.
  - Acceptance, all required: the merge-base output equals `6b8e935c177128d2f455f7bcd2fedc7deff6e30f` (otherwise `CYCLE BASE NOT ANCESTOR`: stop; HEAD itself is not pinned, because a phase-boundary commit by the orchestrator legitimately advances it); the diff exits 0 and prints nothing (`ANCHOR-CODE-DIFF-EXIT=0`; otherwise `CODE DIFFERS FROM CYCLE BASE`: stop, because every later numstat gate assumes an unchanged code tree at the start); the issue Grep counts are 1, 1, 8 and 0 (the eight acceptance criteria stay checked and this plan edits none; any other count is `ACCEPTANCE SECTION CHANGED`: stop); the Glob result for the three names is `none` (a hit is `UNEXPECTED REQUIREMENTS DOCUMENT`: stop, per the minor-audit fail-closed rule); `INHERITED-PORCELAIN:` lists every entry, each of which must lie under FEATURE or under `.claude/agent-memory/` (any entry under `TaskMaster/` or `TaskMaster.Test/`, or any other path, is `UNEXPECTED INHERITED CHANGE`: stop); the artifact states the single Write Set code path verbatim, the constraint list of the remediation inputs verbatim, and the file `FEATURE/remediation-plan.2026-10-03T08-43.md` as this plan.
- [x] [P0-T3] Capture the baseline of the SinkGuard partial and its fixture harness in FEATURE/evidence/remediation-baseline/sinkguard-partial-baseline.md.
  - Command: the Grep tool with pattern `^` (count) over `TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.SinkGuard.cs`; the Grep tool counts over that file for each pattern of `TOKENS-R2`, `TOKENS-R1` and `TOKENS-FORBIDDEN`; the Grep tool with `-o` and pattern `public async Task \w+` over that file (the current test names); the Grep tool counts of `HandleToggleClickAsync_WithNullOrEmptyKeyAndNullEngines_NotifiesOnceWithNullTokenAndInvokesNothing` over `TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests*.cs`; the Grep tool counts over `TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.cs` for `internal Mock<IAppItemEngines> Engines`, `internal List<string> Notifications`, `internal List<LoggedError> Errors`, `internal List<string> Invalidations`, `internal Action<string> OnNotify`, `\[DataRow\(null\)\]` and the pattern `^` (count); the Grep tool count of `Ribbon\x5CEngineToggleStateCoordinatorTests\.SinkGuard\.cs"` over `TaskMaster.Test/TaskMaster.Test.csproj`.
  - Acceptance, all required: the `^` count of the SinkGuard partial is 169, recorded as `SINKGUARD-LINES-BEFORE: 169`; every `TOKENS-R2`, `TOKENS-R1` and `TOKENS-FORBIDDEN` count equals its false-before value (`TOKENS-FORBIDDEN` 0 each, `TaskCompletionSource` at least 2); the `-o` listing names exactly the four `SINKGUARD-NAMES`, recorded as `SINKGUARD-NAMES-BEFORE:`; the `R1-NAME` count over the seven partials is 0 in every file; each harness count is 1, the primary fixture `^` count is 481 and the SinkGuard csproj registration count is 1 (recorded as `TEST-CSPROJ-REGISTRATION: 1`, which is why no csproj edit is planned). Any mismatch is `SINKGUARD ANCHOR MOVED`: stop.
- [x] [P0-T4] Record the coverage baseline of the coordinator files from the committed first-cycle final evidence in FEATURE/evidence/remediation-baseline/coverage-baseline.md.
  - Command: the Grep tool with `-n` over `FEATURE/evidence/qa-gates/coverage-final.md` for the patterns `^COORD-BRANCHES covered=43 valid=44`, `^COORD-LINES covered=203 valid=203`, `^COORD-FILE TaskMaster/Ribbon/EngineToggleStateCoordinator\.Messages\.cs nodes=1 covered=42 valid=42`, `^- First-party coverage: lines ` and `^Total 7388, executed 7388, passed 7388, failed 0\.` (no pattern ends in a line anchor, because a CRLF line ending defeats `$` in the Grep tool); the Grep tool counts over `coverage/final-964.cobertura.xml` (an explicit file path) for the patterns `<class line-rate="1" branch-rate="0\.5" [^>]*filename="TaskMaster\x5CRibbon\x5CEngineToggleStateCoordinator\.Messages\.cs">` and `<class line-rate="1" branch-rate="1(\.0+)?" [^>]*filename="TaskMaster\x5CRibbon\x5CEngineToggleStateCoordinator\.Messages\.cs">`.
  - Acceptance, all required: each of the five committed-evidence patterns has exactly one matching line, and the recorded values are `BASELINE-COORD-BRANCHES: 43/44` (97.73 percent), `BASELINE-COORD-LINES: 203/203`, `BASELINE-MESSAGES-LINES: 42/42`, `BASELINE-FIRST-PARTY:` (the `First-party coverage:` line as written in the artifact) and `BASELINE-TEST-TOTAL: 7388`; the raw-document read gives 1 for the `0\.5` pattern and 0 for the `1(\.0+)?` pattern, recorded as `BASELINE-MESSAGES-BRANCH-RATE: 0.5` (the class-node read-out and the false-before half of the P2-T5 gate). If the raw document is absent the artifact records `RAW-BASELINE-ABSENT` and `BASELINE-MESSAGES-BRANCH-RATE: not read locally, 0.5 as read by CR-1 of the code review`, and the plan continues, because the committed 43/44 is itself the numeric false-before value; any other value of a committed-evidence pattern is `COVERAGE BASELINE MOVED`: stop.
- [x] [P0-T5] Probe that the toolchain bootstrap of the worktree is in place and record FEATURE/evidence/remediation-baseline/bootstrap-probe.md.
  - Command: `pwsh -NoProfile -Command 'Set-Location -LiteralPath "WORKTREE"; [Environment]::CurrentDirectory = (Get-Location).Path; "SDK_MARKER=$(Test-Path -LiteralPath ".dotnet-sdk\sdk\8.0.205")"; dotnet --version; dotnet tool list --local; "PACKAGE_DIRS=$(@(Get-ChildItem -LiteralPath packages -Directory -ErrorAction SilentlyContinue).Count)"; "DOTNET_COVERAGE_RESOLVED=$($null -ne (Get-Command dotnet-coverage -ErrorAction SilentlyContinue))"'`
  - Acceptance, all required: `SDK_MARKER=True`; `dotnet --version` prints a version string rather than the global.json error message; the local tool list contains a row whose Package Id is `csharpier` and whose Version is `1.2.6` (only the Package Id and Version columns are transcribed, the Manifest column carries an absolute path); `PACKAGE_DIRS=` at least 1; `DOTNET_COVERAGE_RESOLVED=True`; `EXIT_CODE: 0`. Any other value is `BOOTSTRAP MISSING`: stop and report, because the first-cycle executor provisioned this worktree and a missing item means the tree changed underneath the plan.
- [x] [P0-T6] Capture the read-only formatter baseline over the worktree (`.csharpierignore` applies) with `dotnet tool run csharpier check .` and record FEATURE/evidence/remediation-baseline/csharpier-check-baseline.md.
  - Command: `pwsh -NoProfile -Command 'Set-Location -LiteralPath "WORKTREE"; [Environment]::CurrentDirectory = (Get-Location).Path; dotnet tool run csharpier check .; "CSHARPIER_EXIT_CODE: $LASTEXITCODE"'`
  - Acceptance: `EXIT_CODE:` is the printed `CSHARPIER_EXIT_CODE:` value and is 0, the `Checked N files` line is recorded (the first-cycle value was `Checked 1640 files`; the figure is an observation), and every path CSharpier reports as unformatted is listed (none expected). A non-zero value is `FORMAT BASELINE NOT CLEAN`: stop, because the Phase 2 repository-wide format would then rewrite files outside the Write Set.
- [x] [P0-T7] Capture the analyzer baseline of TaskMaster.sln with `CMD-REBUILD` (`GATEARGS` `/p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`, `TASKID` p0-t7) and record FEATURE/evidence/remediation-baseline/msbuild-analyzer-baseline.md (`Command:` `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`).
  - Acceptance, all required: `EXIT_CODE: 0`; `ERRORS: 0`; both `CSC_OUT_` counts at least 1; `WRITESET_DIAGNOSTIC_LINES: 0`; `COORDINATOR_DIAGNOSTIC_LINES: 0`; `WARNINGS:` recorded as `ANALYZER-BASELINE-WARNINGS:`; `TEST_DLL_EXISTS: True`. A non-zero exit is `ANALYZER BASELINE NOT CLEAN`: stop.
- [x] [P0-T8] Capture the nullable baseline of TaskMaster.sln with `CMD-REBUILD` (`GATEARGS` `/p:TreatWarningsAsErrors=true`, `TASKID` p0-t8) and record FEATURE/evidence/remediation-baseline/msbuild-nullable-baseline.md (`Command:` `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`).
  - Acceptance, all required: `EXIT_CODE: 0`; `ERRORS: 0`; both `CSC_OUT_` counts at least 1; `WRITESET_DIAGNOSTIC_LINES: 0`; `COORDINATOR_DIAGNOSTIC_LINES: 0`; `WARNINGS:` recorded as `NULLABLE-BASELINE-WARNINGS:`; `TEST_DLL_EXISTS: True`. A non-zero exit is `NULLABLE BASELINE NOT CLEAN`: stop.
- [x] [P0-T9] Capture the pre-change coordinator fixture run over TaskMaster.Test\bin\Debug\TaskMaster.Test.dll (assembly freshly rebuilt by P0-T8 from the unchanged tree) with `CMD-VSTEST` (`TASKID` p0-t9, `NAMES` `NAMES-CYCLE`) and record FEATURE/evidence/remediation-baseline/coordinator-tests-baseline.md.
  - Acceptance, all required: `EXIT_CODE: 0` (the `VSTEST_EXIT_CODE:` value); `TRX_PRESENT: True`; `SEQUENCE_FILES: 0`; `COUNTERS` with `total=43`, `passed=43` and `failed=0`, recorded as `BASELINE-TOTAL: 43`; every `SINKGUARD-NAMES` and `INVARIANT-NAMES` entry has a `RESULT <name> rows=1 passed=1` line; the `R1-NAME` line reads `rows=0 passed=0` (false-before: the test does not exist yet); no `FAILED` line. Anything else is `EXISTING FIXTURE NOT GREEN AT CYCLE BASE`: stop.

### Phase 1 — Constrained Implementation: R-2 Assertions, R-1 Refusal-Path Test, Then Targeted Verification

Phase 1 is the constrained small-path implementation: test-only edits to one partial, then a build and a targeted run of the coordinator fixture. Ordering: R-2 first (one appended pair of statements), then R-1 (one new region), then format, token gates, build and fixture run, then the fail-before exception dossier and the diff-shape gate.

- [x] [P1-T1] Apply delivered source E1 (R-2: the two appended assertions of `HandleToggleClickAsync_WhenNotifyAndLogSinksThrowWithNullEngines_DoesNotThrow`) to `TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.SinkGuard.cs` with the Edit tool, and record FEATURE/evidence/regression-testing/cycle1-r2-edit.md.
  - Acceptance, all required: the Grep tool counts over the file match the `TOKENS-R2` first-edit values `harness\.Engines\.VerifyNoOtherCalls\(\);` 2 and `harness\.Invalidations\.Should\(\)\.BeEmpty\("a refused click changes no state to display"\);` 2; the Read tool over the file shows both new lines inside the body of `HandleToggleClickAsync_WhenNotifyAndLogSinksThrowWithNullEngines_DoesNotThrow`, after its `BeSameAs` chain and before that method's closing brace, and not inside any other method (recorded as `R2-PLACEMENT:` with the two line numbers read). A count of 1 for either pattern is `R-2 EDIT NOT APPLIED`; a count of 3 or more is `R-2 EDIT APPLIED TWICE`: correct with the Edit tool and re-check once, then stop.
- [x] [P1-T2] Apply delivered source E2 (R-1: the new region with the data-driven refusal-path test) to `TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.SinkGuard.cs` with the Edit tool, and record FEATURE/evidence/regression-testing/cycle1-r1-edit.md.
  - Acceptance, all required: the Grep tool counts over the file read `\[DataTestMethod\]` 1, `\[DataRow\(null\)\]` 1, `\[DataRow\(""\)\]` 1, `HandleToggleClickAsync_WithNullOrEmptyKeyAndNullEngines_NotifiesOnceWithNullTokenAndInvokesNothing` 1, `#region Issue #964 — the refusal path with a null or empty engine key` 1, `#endregion Issue #964 — the refusal path with a null or empty engine key` 1 and `harness\.Invalidations\.Should\(\)\.BeEmpty\("a refused click changes no state to display"\);` 3; the Read tool shows the new region between the `#endregion` of the first region and the `#region` of the issue-948 region, with the two `DataRow` lines directly above the method and the original issue-948 `#region` line still present once. Any other count is `R-1 EDIT MISAPPLIED`: correct with the Edit tool and re-check once, then stop.
- [x] [P1-T3] Format the SinkGuard partial with `dotnet tool run csharpier format TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.SinkGuard.cs`, then verify it with `dotnet tool run csharpier check TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.SinkGuard.cs` (both wrapped per the Toolchain commands convention), and record FEATURE/evidence/regression-testing/cycle1-format.md.
  - Acceptance, all required: the format exits 0 (its `Formatted N files` line is a processed count, not an assertion); the success-case observation is the check run, which exits 0, prints a line beginning `Checked 1 file` and lists no path. Both outputs are recorded under `FORMAT:`. A check run that lists the path is `FORMAT NOT CLEAN`: re-run the format and the check once, then stop.
- [x] [P1-T4] Verify the token, forbidden-token and size sets of the formatted SinkGuard partial (CONSTRAINTS, R-1, R-2) with the Grep tool and record FEATURE/evidence/regression-testing/cycle1-token-gates.md.
  - Command: the Grep tool counts over `TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.SinkGuard.cs` for every pattern of `TOKENS-R2`, `TOKENS-R1` and `TOKENS-FORBIDDEN`, and for the pattern `^`.
  - Acceptance, all required: every `TOKENS-R2` and `TOKENS-R1` count equals its required true-after value (a formatter line break cannot change a count, because every pattern is a single-line token or a method name); every `TOKENS-FORBIDDEN` count is 0 and `TaskCompletionSource` is at least 2 (positive control that the file was read); the `^` count satisfies `SIZE-BOUNDS` (at least 195 and at most 500), recorded as `SINKGUARD-LINES-AFTER:`. Any mismatch is `TOKEN GATE FAILED`: stop.
- [x] [P1-T5] Build TaskMaster.sln with `CMD-BUILD` (`TASKID` p1-t5) so the fixture run observes the edited test assembly, and record FEATURE/evidence/regression-testing/cycle1-build.md.
  - Acceptance, all required: `MSBUILD_EXIT_CODE: 0`; `ERRORS: 0`; `TEST_DLL_ADVANCED: True` (the test assembly was rebuilt after the edit; `False` means the run would observe a stale assembly and is `STALE TEST ASSEMBLY`: stop); `CSC_OUT_TASKMASTER_TEST:` at least 1.
- [x] [P1-T6] Run the coordinator fixture over TaskMaster.Test\bin\Debug\TaskMaster.Test.dll with `CMD-VSTEST` (`TASKID` p1-t6, `NAMES` `NAMES-CYCLE`) and record FEATURE/evidence/regression-testing/cycle1-fixture-run.md.
  - Acceptance, all required: `EXIT_CODE: 0` (the `VSTEST_EXIT_CODE:` value); `TRX_PRESENT: True`; `SEQUENCE_FILES: 0`; `COUNTERS` with `total=45`, `passed=45` and `failed=0`, recorded as `FIXTURE-TOTAL: 45` (the `BASELINE-TOTAL: 43` of P0-T9 plus the two data rows, so a run that did not discover the new rows reads 43 and fails this clause); the `R1-NAME` line reads `rows=` at least 2 with `passed=` equal to `rows=` (both data rows ran and passed); every `SINKGUARD-NAMES` entry, including `R2-NAME`, and every `INVARIANT-NAMES` entry reads `rows=1 passed=1`; no `FAILED` line. Anything else is `CYCLE FIXTURE NOT GREEN`: stop and report the `FAILED` and `MESSAGE` lines with absolute paths transcribed as `REDACTED-PATH`.
- [x] [P1-T7] Record the fail-before exception dossier for the cycle in FEATURE/evidence/regression-testing/fail-before-exception.<yyyy-MM-ddTHH-mm>.md, the timestamp being the host-clock time of the write.
  - Acceptance, all required: the artifact carries `Timestamp:`, `Command:` (the sources it cites), `EXIT_CODE: 0`, `Output Summary:`, a line `WhyFailingRunImpossible:` stating in one to three sentences that R-1 adds coverage over correct behaviour and R-2 adds assertions over correct behaviour, so no failing run against the unmodified production code exists, and an alternative-proof section citing, by artifact path and value, `R1-NAME` count 0 before the edit (`remediation-baseline/sinkguard-partial-baseline.md`), `BASELINE-MESSAGES-BRANCH-RATE: 0.5` and `BASELINE-COORD-BRANCHES: 43/44` (`remediation-baseline/coverage-baseline.md`), the `TOKENS-R2` and `TOKENS-R1` before and after counts (`remediation-baseline/sinkguard-partial-baseline.md` and `regression-testing/cycle1-token-gates.md`) and `FIXTURE-TOTAL: 45` against `BASELINE-TOTAL: 43` (`regression-testing/cycle1-fixture-run.md`). The final half of the proof (class-node branch-rate 1) is appended by P2-T6.
- [x] [P1-T8] Verify the diff shape of the cycle against CYCLE-BASE and record FEATURE/evidence/regression-testing/cycle1-sinkguard-diff.md.
  - Command: `git -C WORKTREE diff --numstat 6b8e935c177128d2f455f7bcd2fedc7deff6e30f -- TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.SinkGuard.cs`; `git -C WORKTREE diff --name-only 6b8e935c177128d2f455f7bcd2fedc7deff6e30f -- TaskMaster TaskMaster.Test`; `git -C WORKTREE status --porcelain --untracked-files=all -- TaskMaster TaskMaster.Test`.
  - Acceptance, all required: the numstat row reports 0 deleted lines and between 35 and 50 added lines for the SinkGuard partial (expected 40: 2 for E1 plus 38 for E2; a deleted-line count above 0 means an existing line was rewritten and is `EXISTING LINE CHANGED`: stop); the name-only listing under `TaskMaster` and `TaskMaster.Test` consists of exactly one line, `TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.SinkGuard.cs` (any other line is `FOOTPRINT EXCEEDS WRITE SET`: stop); the porcelain listing under those two paths is either empty (the edit was committed by an orchestrator phase-boundary commit, in which case the diff listing still carries the file) or exactly one entry, ` M TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.SinkGuard.cs`; the union of the two listings contains the SinkGuard path (positive control).

### Phase 2 — Final QA Toolchain Loop, Coverage Delta, Scope, Closure and Delivery

No code file is edited in Phase 2. The loop follows D-8: a SinkGuard rewrite by P2-T1 restarts the loop once from P2-T1 (recorded as `PASS-2:` sections in the same artifacts); any other failure of P2-T1 to P2-T5 stops the plan with its artifact, and the fix returns to the orchestrator as a further remediation round that re-enters at P2-T1.

- [x] [P2-T1] Apply repository-wide formatting from the worktree root (TaskMaster.sln tree, `.csharpierignore` applies) with `dotnet tool run csharpier format .` and record FEATURE/evidence/qa-gates/cycle1-csharpier-format.md.
  - Command: `CMD-HASH-SINKGUARD` and `git -C WORKTREE status --porcelain --untracked-files=all` before the format; `pwsh -NoProfile -Command 'Set-Location -LiteralPath "WORKTREE"; [Environment]::CurrentDirectory = (Get-Location).Path; dotnet tool run csharpier format .; "CSHARPIER_EXIT_CODE: $LASTEXITCODE"'`; then `CMD-HASH-SINKGUARD` and the same porcelain command again.
  - Acceptance, all required: `EXIT_CODE: 0`; the `Formatted N files` line is recorded as a processed count, not an assertion; the before-and-after tree observation holds: the `HASH` value after the format equals the value before it and the two porcelain listings are identical. In pass 1 a differing hash restarts the loop once from this task as stated above, and pass 2 compares against the hash recorded after the pass-1 format; a differing hash in pass 2 is `FORMAT NOT STABLE`: stop. A porcelain entry that changed and lies outside FEATURE, `.claude/agent-memory/` and the SinkGuard path is `FORMAT TOUCHED OUT-OF-SCOPE FILE`: stop.
- [x] [P2-T2] Verify formatting read-only from the worktree root (TaskMaster.sln tree) with `dotnet tool run csharpier check .` and record FEATURE/evidence/qa-gates/cycle1-csharpier-check.md.
  - Command: `pwsh -NoProfile -Command 'Set-Location -LiteralPath "WORKTREE"; [Environment]::CurrentDirectory = (Get-Location).Path; dotnet tool run csharpier check .; "CSHARPIER_EXIT_CODE: $LASTEXITCODE"'`
  - Acceptance: `EXIT_CODE: 0`, the `Checked N files` line is recorded (the success-case line is `Checked N files in <ms>ms.` and names no path), and no path is reported as unformatted.
- [x] [P2-T3] Run the analyzer gate on TaskMaster.sln with `CMD-REBUILD` (`GATEARGS` `/p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`, `TASKID` p2-t3) and record FEATURE/evidence/qa-gates/cycle1-msbuild-analyzer.md (`Command:` `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`).
  - Acceptance, all required: `EXIT_CODE: 0`; `ERRORS: 0`; both `CSC_OUT_` counts at least 1; `WRITESET_DIAGNOSTIC_LINES: 0`; `COORDINATOR_DIAGNOSTIC_LINES: 0`; `WARNINGS:` recorded beside `ANALYZER-BASELINE-WARNINGS:` as an observation.
- [x] [P2-T4] Run the type-check gate on TaskMaster.sln with `CMD-REBUILD` (`GATEARGS` `/p:TreatWarningsAsErrors=true`, `TASKID` p2-t4) and record FEATURE/evidence/qa-gates/cycle1-msbuild-nullable.md (`Command:` `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`).
  - Acceptance, all required: `EXIT_CODE: 0`; `ERRORS: 0`; both `CSC_OUT_` counts at least 1; `WRITESET_DIAGNOSTIC_LINES: 0`; `COORDINATOR_DIAGNOSTIC_LINES: 0`; `WARNINGS:` recorded beside `NULLABLE-BASELINE-WARNINGS:` as an observation.
- [x] [P2-T5] Run the test-and-coverage gate with scripts/vscode/Invoke-MSTestWithCoverage.ps1 through `CMD-COVERAGE-RUNNER` and then, only when the runner exit code is 0, `CMD-COVERAGE-POST`, and record FEATURE/evidence/qa-gates/cycle1-coverage.md.
  - Artifact: `Timestamp:`; `Command:` `pwsh -NoProfile -File scripts\vscode\Invoke-MSTestWithCoverage.ps1`; `EXIT_CODE:` the `RUNNER_EXIT_CODE:`; `Output Summary:` (at most 20 lines) carrying the exit code, `LINE-FLOOR:`, `BRANCH-FLOOR:`, the `First-party coverage:` line, the `ROOT` line, `COORD-LINES`, `COORD-BRANCHES`, `COORD-LINE-RATE:`, `COORD-BRANCH-RATE:`, the three `CLASS-NODE` rows, `FINAL-FAILED-FQN-COUNT:` and `FINAL-TEST-TOTAL:`; then `Details:` with `DISCOVERED_LINE:`, `THRESHOLD_MESSAGE:`, `COLLECT_FAILURE_MESSAGE:`, `DOCUMENT_PRESENT:`, `TRX_PRESENT:`, `SUMMARY_FILE_PRESENT:`, `TEST-DEFINITIONS:`, the projection verbatim between `PROJECTION-BEGIN` and `PROJECTION-END`, the summary verbatim between `SUMMARY-BEGIN` and `SUMMARY-END`, every `COORD-FILE` row, `COORD-CLASS-NODES:` and the Grep read-outs named below. `coverage\remediation-964.cobertura.xml` and `coverage\remediation-964.trx` stay on disk, git-ignored; no raw document is committed (CLAUDE.md Committed Test Evidence Format).
  - Command (class-node read-out, after the post step): the Grep tool counts over `coverage/remediation-964.cobertura.xml` and over `coverage/final-964.cobertura.xml` (explicit file paths) for the patterns `<class line-rate="1" branch-rate="1(\.0+)?" [^>]*filename="TaskMaster\x5CRibbon\x5CEngineToggleStateCoordinator\.Messages\.cs">` (`NODE-RATE-ONE`) and `<class line-rate="1" branch-rate="0\.5" [^>]*filename="TaskMaster\x5CRibbon\x5CEngineToggleStateCoordinator\.Messages\.cs">` (`NODE-RATE-HALF`).
  - Acceptance, all required: `RUNNER_EXIT_CODE` is 0 with an empty `COLLECT_FAILURE_MESSAGE:` and an empty `THRESHOLD_MESSAGE:` (a non-zero exit is `TEST STEP FAILED`: stop without a re-run, D-5); `DOCUMENT_PRESENT: True` and `TRX_PRESENT: True`; `LINE-FLOOR: MET` and `BRANCH-FLOOR: MET`; `FAILED-FQN-COUNT: 0` with `TEST-DEFINITIONS:` at least 1 and no `FAILED-FQN` row; the summary first line begins `Test run outcome:` and its second line reads `Total 7390, executed 7390, passed 7390, failed 0.` (the first-cycle total 7388 plus the two data rows; a total of 7388 means the new rows were not discovered and is `NEW TEST NOT DISCOVERED`: stop); the projection contains the `TaskMaster` package; every `COORD-FILE` row reads `nodes=1` and `COORD-CLASS-NODES: 3` (otherwise `PARTIAL CLASS ATTRIBUTION UNSUPPORTED`: stop); `COORD-LINES covered=203 valid=203`; `COORD-BRANCHES covered=44 valid=44` (R-1 closes the one uncovered branch of the first-cycle 43/44, and no production branch was added or removed); the Messages `COORD-FILE` row reads `branches-covered=2 branches-valid=2`; the Messages `CLASS-NODE` row reads `branch-rate=1` and the main and Prime `CLASS-NODE` rows read `branch-rate=1` (target 1 for every coordinator class node); the Grep read-out over `coverage/remediation-964.cobertura.xml` gives `NODE-RATE-ONE` 1 and `NODE-RATE-HALF` 0, and over `coverage/final-964.cobertura.xml` gives `NODE-RATE-ONE` 0 and `NODE-RATE-HALF` 1 (the false-before control: the same patterns report the other value on the pre-change document, so the check can fail); no absolute path in the artifact.
- [x] [P2-T6] Compare baseline and post-change coverage for the coordinator files under TaskMaster/Ribbon and record FEATURE/evidence/qa-gates/cycle1-coverage-comparison.md (sources: FEATURE/evidence/remediation-baseline/coverage-baseline.md and FEATURE/evidence/qa-gates/cycle1-coverage.md); then append the final half of the fail-before proof to the dossier of P1-T7.
  - Acceptance, all required: the artifact carries `Timestamp:`, `Command:` (the two source artifacts read), `EXIT_CODE: 0` and an `Output Summary:` with `BASELINE-FIRST-PARTY:` and `FINAL-FIRST-PARTY:` (repository line and branch percentages as printed; observations, because the repository-wide rate is not deterministic run to run; the gate on them is the floors of P2-T5), `BASELINE-COORD-LINES: 203/203` and `FINAL-COORD-LINES: 203/203`, `BASELINE-COORD-BRANCHES: 43/44 (97.73)` and `FINAL-COORD-BRANCHES: 44/44 (100)`, `BASELINE-MESSAGES-BRANCH-RATE: 0.5` and `FINAL-MESSAGES-BRANCH-RATE: 1`, and `NEW-CODE-COVERAGE: not applicable, no production line or branch was added` (the changed lines are test lines, excluded from the coverage denominator). Clauses, each recorded `MET` or `NOT MET` with its two values: final coordinator line count equals the baseline (no line removed from coverage); final covered branches equal final valid branches and exceed the baseline covered branches by exactly 1; the final Messages class-node branch-rate is 1; both final floors met. Any `NOT MET` stops. The dossier of P1-T7 gains an appended `CLASS-NODE-PROOF:` section naming the baseline value 0.5 and the final value 1.
- [x] [P2-T7] Record the single clean toolchain pass of TaskMaster.sln (P2-T1 to P2-T5) in FEATURE/evidence/qa-gates/cycle1-toolchain-pass.md.
  - Acceptance: the artifact carries `Timestamp:`, `Command:` listing the four CLAUDE.md commands verbatim in order (`dotnet tool run csharpier format .` with `dotnet tool run csharpier check .`; the analyzer `/t:Rebuild`; the `TreatWarningsAsErrors` `/t:Rebuild`; `Invoke-MSTestWithCoverage.ps1`), `EXIT_CODE: 0` and an `Output Summary:` naming each step's artifact and result, the pass number (1, or 2 after the admitted restart) and the test-step outcome (`TEST-STEP: PASS`, runner exit 0 and zero failed tests).
- [x] [P2-T8] Verify the change footprint of the cycle against CYCLE-BASE and the Write Set of FEATURE/remediation-plan.2026-10-03T08-43.md, and record FEATURE/evidence/qa-gates/cycle1-footprint.md.
  - Command: `git -C WORKTREE diff --name-only 6b8e935c177128d2f455f7bcd2fedc7deff6e30f`; `git -C WORKTREE status --porcelain --untracked-files=all`; `git -C WORKTREE diff --name-only 6b8e935c177128d2f455f7bcd2fedc7deff6e30f -- TaskMaster TaskMaster.Test`; negative control `git -C WORKTREE diff --name-only 94287369908cc920b21b0e3256314f988ad7d2f5 -- TaskMaster TaskMaster.Test`.
  - Acceptance, all required: every path in the first listing and every porcelain entry is `TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.SinkGuard.cs`, lies under `docs/features/active/2026-10-01-engine-toggle-coordinator-947-review-residuals-964/`, or lies under `.claude/agent-memory/` (ambient, never staged); the path `docs/features/active/2026-10-01-engine-toggle-coordinator-947-review-residuals-964/issue.md` appears in neither listing (no acceptance criterion was edited); the third listing consists of exactly one line, the SinkGuard path (no production file, csproj or other test file changed this cycle); positive control: the union of the first listing and the porcelain listing contains the SinkGuard path and `docs/features/active/2026-10-01-engine-toggle-coordinator-947-review-residuals-964/remediation-plan.2026-10-03T08-43.md` (a file recorded by an orchestrator phase-boundary commit appears in the diff listing and need not appear in the porcelain listing). Negative control, required: the control listing is non-empty and contains the line `TaskMaster/Ribbon/EngineToggleStateCoordinator.cs`, a path the first-cycle work changed and this cycle must not, which shows that the path-set rule above reports `FOOTPRINT EXCEEDS WRITE SET` when an out-of-scope code path differs from its anchor (otherwise `FOOTPRINT CONTROL INERT`: stop). Any other path in the first listing or porcelain is `FOOTPRINT EXCEEDS WRITE SET`: stop. Both listings and the control are recorded.
- [x] [P2-T9] Measure every fixture partial under TaskMaster.Test/Ribbon with `CMD-LINECOUNT` and record FEATURE/evidence/qa-gates/cycle1-line-counts.md.
  - Acceptance, all required: `TEST-PARTIALS: 7`; the `LINES` value of every partial is at most 500; the SinkGuard value is at least 195 (it grew from 169) and is recorded as `SINKGUARD-LINES-FINAL:` equal to `SINKGUARD-LINES-AFTER:` of P1-T4; the primary fixture `LINES` value is 481 (unchanged; any other value is `PRIMARY FIXTURE CHANGED`: stop). A value above 500 is `FILE SIZE CEILING EXCEEDED`: stop.
- [x] [P2-T10] Record the closure of findings R-1 and R-2 in FEATURE/evidence/other/cycle1-finding-closure.md, without editing issue.md.
  - Acceptance: the artifact carries `Timestamp:`, `Command:` (the artifacts read), `EXIT_CODE: 0` and an `Output Summary:` with one line per finding. `R-1: MET` only when P1-T2 shows the `TOKENS-R1` counts, P1-T6 shows `R1-NAME` with `rows=` at least 2 and `passed=` equal to it, P2-T5 shows the Messages `CLASS-NODE` `branch-rate=1` with `COORD-BRANCHES covered=44 valid=44`, and P2-T8 shows the SinkGuard partial as the only changed code file; `R-2: MET` only when P1-T1 shows both `TOKENS-R2` counts at their first-edit values and the placement inside the both-sinks test, P1-T6 shows `R2-NAME` `rows=1 passed=1`, and P1-T8 shows 0 deleted lines; otherwise the finding is recorded `NOT MET` with the failing values and the plan outcome is INCOMPLETE. The artifact also states that AC1 to AC8 of issue.md are unchanged and still checked (P0-T2 and P2-T8).
- [x] [P2-T11] Hand off for the reduced (minor) audit and record FEATURE/evidence/other/cycle1-reduced-audit-handoff.md.
  - Acceptance: the artifact carries `Timestamp:`, the finding source (`docs/features/active/2026-10-01-engine-toggle-coordinator-947-review-residuals-964/code-review.2026-10-03T08-50.md`, findings CR-1 and CR-4) and the AC source (`docs/features/active/2026-10-01-engine-toggle-coordinator-947-review-residuals-964/issue.md`, unchanged), the `R-1` and `R-2` statuses copied from cycle1-finding-closure.md, the evidence paths of every artifact written by P0-T1 to P2-T10 and of this artifact itself, then, under a heading WRITTEN AFTER THIS RECORD, the two fixed paths docs/features/active/2026-10-01-engine-toggle-coordinator-947-review-residuals-964/evidence/other/cycle1-commit-record.md (written by P2-T12) and docs/features/active/2026-10-01-engine-toggle-coordinator-947-review-residuals-964/evidence/qa-gates/cycle1-evidence-hygiene.md (written by P2-T13), with no result claimed for either, the reduced artifact checks for the auditor (the fixture run, the coverage comparison with the class-node read-out, the footprint gate and the hygiene gate), and the statement that CR-2, CR-3 and observations O-1 to O-5 are out of scope for this cycle.
- [x] [P2-T12] Record the pre-commit state in FEATURE/evidence/other/cycle1-commit-record.md before anything is staged.
  - Command: `git -C WORKTREE status --porcelain --untracked-files=all`; `git -C WORKTREE rev-parse --abbrev-ref HEAD`; `git -C WORKTREE rev-parse HEAD`.
  - Acceptance, all required: the artifact carries `Timestamp:`, `Command:`, `EXIT_CODE: 0` and `Output Summary:`; `BRANCH:` equals `bug/engine-toggle-coordinator-947-review-residuals-964`; `PRE-COMMIT-HEAD:` is the printed 40-hex-digit value; the porcelain listing is recorded and every entry is the SinkGuard path, lies under FEATURE or lies under `.claude/agent-memory/`; the artifact states the staging rule (explicit paths only: the SinkGuard path and the FEATURE folder), the commit subject `test(964): cover the null or empty engine key refusal path and symmetric sink-guard assertions` and the statement that the commit hash and the push output are reported in the executor return and not written into the repository (D-7). The hash is a pre-commit observation, so the artifact does not claim any commit-time value.
- [x] [P2-T13] Run `CMD-HYGIENE` over every Markdown file under docs/features/active/2026-10-01-engine-toggle-coordinator-947-review-residuals-964 after every artifact except the hygiene record itself exists, and record FEATURE/evidence/qa-gates/cycle1-evidence-hygiene.md.
  - Acceptance: `FILES_SCANNED=` at least 70 (derivation: the 40 Markdown files present when this plan was authored, namely issue.md, the executed plan, the policy audit, the code review, the feature audit, the remediation inputs and 34 evidence files, plus this plan, 41; plus the 29 evidence files of this plan other than cycle1-evidence-hygiene.md, which this task writes after the sweep: 9 remediation-baseline, 8 regression-testing, 9 qa-gates and 3 other; 41 + 29 = 70), `ACCOUNT_HITS=0`, `MACHINE_HITS=0`, `DRIVE_USERS_HITS=0` and `RAW_DOCUMENTS=0`. A non-zero host count is attributed by the `HIT-FILE` rows, each naming one offending file as its parent directory name, a slash and its file name; it is repaired by replacing each occurrence with REDACTED-PATH in every file a `HIT-FILE` row names and re-running this task, except that a `HIT-FILE` row naming `other/` followed by the file name of a `preflight-clearance.*.md` record is not repaired by this plan: it is `PREPARATION RECORD HOST HIT`: stop and report. A non-zero `RAW_DOCUMENTS` is repaired by deleting that copy from the feature folder (the original stays under `coverage/`).
- [x] [P2-T14] Stage the cycle by explicit path and verify the staged set.
  - Command: `git -C WORKTREE add -- TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.SinkGuard.cs docs/features/active/2026-10-01-engine-toggle-coordinator-947-review-residuals-964`; `git -C WORKTREE diff --cached --name-only`; `git -C WORKTREE status --porcelain --untracked-files=all`.
  - Acceptance, all required: every line of the cached listing is `TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.SinkGuard.cs` or lies under `docs/features/active/2026-10-01-engine-toggle-coordinator-947-review-residuals-964/` (the same path-set rule whose negative control P2-T8 shows can fail); the cached listing contains the SinkGuard path, `.../evidence/qa-gates/cycle1-evidence-hygiene.md` and `.../evidence/other/cycle1-commit-record.md` (positive controls that the sweep record and the commit record are part of the commit); no `.claude/agent-memory/` path and no path under `TaskMaster/` is staged; the porcelain listing after staging shows no `??` entry under FEATURE and no ` M` entry for the SinkGuard path (everything of the cycle is staged). The hook that gates a commit of a `.cs` file needs the orchestrator checkpoint; a denial is reported verbatim and stops this task. `git add -A` and `git add .` are never used.
- [x] [P2-T15] Commit the staged cycle with one `git -C WORKTREE commit -m "test(964): cover the null or empty engine key refusal path and symmetric sink-guard assertions"` command (a single `-m`; any attribution trailer lines the executing session requires are appended as further `-m` paragraphs; no heredoc, redirection or substitution), then verify the result.
  - Command: the commit; `git -C WORKTREE log -1 --format=%s`; `git -C WORKTREE show --name-only --format= HEAD`; `git -C WORKTREE status --porcelain --untracked-files=all`.
  - Acceptance, all required: the commit exits 0; the subject printed by `log -1` begins `test(964): cover the null or empty engine key refusal path`; the `show --name-only` listing equals the cached listing of P2-T14 (the same path-set rule holds); the porcelain listing after the commit contains no entry other than `.claude/agent-memory/` paths and, at most, the plan file `docs/features/active/2026-10-01-engine-toggle-coordinator-947-review-residuals-964/remediation-plan.2026-10-03T08-43.md`, modified only by the check-offs of P2-T14, this task and P2-T16, which are made after the staging snapshot (any other entry is `UNCOMMITTED CYCLE FILE`: stop). The printed commit hash is reported in the executor return. A hook denial is reported verbatim and stops this task.
- [x] [P2-T16] Push the branch with `git -C WORKTREE push origin bug/engine-toggle-coordinator-947-review-residuals-964` and verify that the remote ref equals the committed head.
  - Command: the push; `git -C WORKTREE rev-parse HEAD`; `git -C WORKTREE rev-parse refs/remotes/origin/bug/engine-toggle-coordinator-947-review-residuals-964`; `git -C WORKTREE status --porcelain --untracked-files=all`; `git -C WORKTREE diff --numstat HEAD -- docs/features/active/2026-10-01-engine-toggle-coordinator-947-review-residuals-964/remediation-plan.2026-10-03T08-43.md`.
  - Acceptance, all required: the push exits 0 (a rejected or non-fast-forward push is `PUSH REJECTED`: stop and report, with no force option); the two `rev-parse` outputs are equal (the remote-tracking ref was updated by the push to the head that holds the cycle commit, so the equality can fail if the push did nothing); the porcelain listing contains no entry other than `.claude/agent-memory/` paths and, at most, the plan file of this plan, whose numstat row against HEAD then reports an equal number of inserted and deleted lines of at most 3, an empty output counting as 0 and 0 (the check-offs of P2-T14 to P2-T16, left to the orchestrator's next phase-boundary commit). The push output and the two hashes are reported in the executor return and are not written to the repository (D-7).
