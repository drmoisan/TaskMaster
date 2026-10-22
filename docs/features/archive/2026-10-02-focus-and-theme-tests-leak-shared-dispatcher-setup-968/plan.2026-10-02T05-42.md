# 2026-10-02-focus-and-theme-tests-leak-shared-dispatcher-setup (Plan)

- **Issue:** #968 (issue #972 folded in by the Coordinator Scope Amendment in issue.md; the pull request closes both)
- **Parent (optional):** none
- **Owner:** drmoisan
- **Work Mode:** full-bug (acceptance criteria come from `docs/features/active/2026-10-02-focus-and-theme-tests-leak-shared-dispatcher-setup-968/spec.md` only; no user story exists for this item and none is to be authored)
- **Last Updated:** 2026-10-03 (round-5 deltas applied; this planning session has no shell clock, so no minute stamp is composed)
- **Status:** Ready for preflight (revision round 5: the two round-5 deltas, 1a and 1b, applied verbatim, plus one knock-on edit to 1a raised in the delta-application pass: `RESTART-CORRECTED:` is the union over every D-13 restart in the run)
- **Version:** 1.5
- **Plan path continuity:** this file is updated in place for every preflight revision round. No timestamped sibling plan file is created for this cycle.

**Fail-closed evidence rule:** every command-bearing task writes one evidence artifact carrying `Timestamp:`, `Command:`, `EXIT_CODE:` and `Output Summary:`. A task whose artifact is missing or incomplete stays unchecked, and the plan outcome is BLOCKED or INCOMPLETE, never PASS.

**Evidence accounting rule:** the artifact path is named in the task text. Do not mark an evidence-bearing task complete without the artifact on disk at that exact path.

**Evidence location:** every artifact lives under `docs/features/active/2026-10-02-focus-and-theme-tests-leak-shared-dispatcher-setup-968/evidence/` in the canonical sub-kinds `baseline/`, `regression-testing/`, `qa-gates/` and `other/`. EVIDENCE_LOCATION_OVERRIDE_REJECTED: none supplied; no artifacts-tree evidence path appears in this plan. In task text the token FEATURE abbreviates `docs/features/active/2026-10-02-focus-and-theme-tests-leak-shared-dispatcher-setup-968`; the Write Set spells every path in full.

## Revision record

- **Round 1 (reviewer report `FEATURE/evidence/other/preflight-round1-report.2026-10-02T08-40.md`).** Defect 1: the `PWSH CHANNEL REFUSED` stop rule is appended to the payload-channel convention, with the cwd note. Defect 2: option B is superseded; fact 11, D-9, P0-T3, P6-T9, P8-T9 and P8-T46 are re-derived against the current branch, and every commit is pathspec-limited. Defect 3: the F-FIELDS comment no longer contains the `lock (FieldLock)` literal. Defects 4 and 5: N1 test 4 is the two-transaction variant and the nesting gate is per pin and per transaction, with every knock-on count updated (PC tokens, PRIMARY 16, CROSS 32, CONTROL 28, `INVOCATIONS-CLASSIFIED: 13 of 13 nested`, the N1 count line, the CMD-CENSUS note). Defect 6: the indentation statement names N1 and T1 as carrying one extra Markdown indent. Defect 7: WORKTREE is written with forward slashes in every `git -C` argument. Defect 8: `installed nothing carries` is in the FIX token list and in the AC11 check-off. Defect 9: the P3-T9 wording names the TS list. Defect 10: CMD-COVERAGE-POST prints every non-passed message, P8-T5 carries the `LEAK-DEPENDENT TEST EXPOSED` stop, and Risks carries the matching bullet.
- **Spec amendment 1.2 (issue #972 fold and the #968 liveness comment).** Phases 4 and 5 are new; the former Phases 4, 5 and 6 are now Phases 6, 7 and 8. AC25 to AC32 and amended AC20 are covered; the Write Set, every count and every footprint gate are re-derived.
- **Round 2 (reviewer report `FEATURE/evidence/other/preflight-round2-report.2026-10-02T23-56.md`).** Defect 1: F-SCOPE is forty-three lines, so the fixture is 375 lines after Phase 2 (F-SCOPE prose, P2-T6, P3-T9). Defects 2 and 3: the four fold span baselines print the END anchor line minus one, and the T1-LIVE `(await pending)` baseline is 1 (span anchors, P0-T13). Defect 4: the QDM `ForEachAwaitWithCancellationAsync` baseline is 2 (fact 14, P0-T13). Defect 5: the post-change `FakeTimeProvider` counts include the `ArmingFakeTimeProvider` substring (L prose, P5-T3, M prose, P5-T4); re-deriving them showed that the `using Microsoft.Extensions.Time.Testing;` directive does not contain the token either, so the baselines are LIV 1 and DMT 5 (facts 17 and 20, P0-T13, P4-T6) and the DMT post-change value is 6, not the 7 the report derived. Defect 6: the QfcDatamodel.cs hunk count is recorded and the numstat row `1	129` is gated (P4-T9; P6-T2 names numstat among the values it restates). Defect 7: the L-T1 and M-T doc comments read `a scheduler yield`. Defect 8: D-10 covers evidence-file Write and pwsh payload refusals, including `PREIMPLEMENTATION_GATE_BLOCKED`. The L1 prose and the fact 17 `await` count were tightened as sibling consistency edits.
- **Round 3 (reviewer report `FEATURE/evidence/other/preflight-round3-report.2026-10-03T01-01.md`).** Defect 1: the legacy-caller primary strategy returns 25 lines, 17 of them in `QfcDatamodel.cs` (fact 15, P4-T1, the self-review summary). Defect 2: P4-T1 classifies the two method-group assignments at `QfcDatamodel.cs` 40 and 52 as `METHOD-GROUP-ONE-ARG-OVERLOAD`. Defect 3: P6-T2 requires each token, span, hunk and numstat value as last recorded for its file, so the interim P4-T6, P4-T7 and P4-T9 values that P5-T5 supersedes are no longer demanded after formatting, and the project-file numstat row is restated as `3	0`. Defect 4: fact 22 reads 334 spec lines and 4 lines for `acquired and released inside a held` (lines 10, 105, 266, 282), and P0-T2 restates the 4. Advisory A1: the payload-channel convention states that payloads are never merged into one call, because the hook layer scans a pwsh payload as raw text. The task-description line of P6-T2 (the enumeration of commands to re-run) was read for the same defect and left unchanged: it names the commands, not the values, and re-running every listed command is satisfiable.
- **Round 4 (reviewer report `FEATURE/evidence/other/preflight-round4-report.2026-10-03T01-25.md`).** Defect 1: P4-T1 classifies the `#region` and `#endregion` lines of the empty `Linked List Locking` region (`QfcDatamodel.cs` 469 and 472) as `REGION-DIRECTIVE`, so every one of the 25 PRIMARY, 3 LOG and 2 CROSS lines that CMD-LEGACY-CALLERS prints has a category. Defect 2: D-13 states that on the Phase 8 restart to P6-T1 the P6-T9 commit is already in HEAD, so P6-T2 runs its HEAD-anchored git commands with BASE as the ref operand and its porcelain expectation becomes membership of the fourteen Write Set code paths with status ` M` (`P6-RESTART-PORCELAIN:`); the same rule is named for a Phase 6 restart that follows that commit, and the P6-T2 acceptance line carries a pointer to D-13. Defect 3: the P6-T2 exemption covers a printed `SPAN:` range and the recorded-not-gated QfcDatamodel.cs `HUNK_COUNT:` as well as a LINES value, so no value an earlier task records without gating is demanded after formatting. The optional advisory delta (naming the P1-T3 PROJ token in the P6-T2 per-file pointer) was declined by the orchestrator: the value holds regardless and the round stays narrow. Sweep: P8-T45 is the only other HEAD-anchored diff, and it runs after the check-offs and before the P8-T46 commit, on no restart path; every other `??` expectation (P1-T3, P2-T6, P3-T9, P4-T9, P5-T5) belongs to a pre-commit task and is re-run after the commit only through P6-T2, which the D-13 rule now covers; no other acceptance line restates a recorded-not-gated value as gated.
- **Round 5 (reviewer report `FEATURE/evidence/other/preflight-round5-report.2026-10-03T01-53.md`).** Defect 1: the P6-T2 rewrite exemption read only the current P6-T1 pass, so on a second or later pass (a Phase 6 restart, a Phase 8 restart to P6-T1, or both) a file the first pass re-laid, or the file edited by the correction that triggered the restart, kept a LINES value or `SPAN:` range that differed from its Phase 1 to 5 record without being named in `REWRITTEN:`, and the plan gave no recovery route. On a D-13 restart P6-T1 now also records `PRIOR-PASS-REWRITTEN:` (the union of the `REWRITTEN:` paths of every earlier P6-T1 pass in this run, or `NONE`) and `RESTART-CORRECTED:` (the union of the Write Set paths edited by every correction that triggered a D-13 restart in this run; the union wording is a knock-on edit raised in the delta-application pass, because a label naming only the latest correction leaves a file edited by an earlier correction, and not re-laid by the formatter, named by none of the three labels on a third P6-T1 pass), and the P6-T2 exemption admits a file named by any of the three labels. The exemption still covers only a LINES value, a printed `SPAN:` range and the recorded-not-gated QfcDatamodel.cs `HUNK_COUNT:`; every token value, the at-most-500 and at-most-400 LINES bounds and the two gated `HUNK_COUNT: 2` values stay gated. Sibling read, no change: P6-T3's `REWRITTEN:` condition means the current pass, because it gates whether the production build is fresh; P8-T1 and P8-T7 use the distinct labels `REWRITTEN-WRITESET:` and `REWRITTEN-OTHER:`; P8-T8 compares against the final P6-T2 values; D-13 defines the restart paths the two new labels refer to and is unchanged; P6-T9, P7-T3 and P8-T9 are BASE-anchored.

## Caller instructions applied with a recorded adjustment

1. **Absolute worktree path in `Command:` rows.** The repository hygiene guard (`scripts/hygiene/Test-RepositoryHygiene.Rules.ps1`, function `Get-UserProfilePathPattern`, line 21) rejects any committed line matching a drive-letter user-profile path, and both this plan and every evidence artifact are committed. Every `Command:` row therefore records the payload with the literal token `WORKTREE` in place of the absolute path, and every payload prints `WORKTREE-LEAF:` followed by the leaf name of its working directory. The acceptance condition is that `WORKTREE-LEAF: agent-a291a7fbabf9d0229` is recorded; a payload that ran in another tree prints a different leaf and fails that condition.
2. **Quote character of the pwsh channel.** The caller's example uses double quotes around the command string. Payloads contain `$`, so they are passed in outer single quotes (`pwsh -NoProfile -Command '...'`) and use double quotes only inside; the Bash channel cannot carry a single quote inside a single-quoted argument, so no payload and no asserted token contains an apostrophe.
3. **D1 placement.** D1 (the fixture's own doc comments) is applied in Phase 2 together with the fixture fix, because it edits the same file and the same regions; D2 to D6 remain in Phase 3.
4. **Spec amendments.** Amendment 1.1 (planner) and amendment 1.2 (orchestrator) are the text on disk; this plan does not edit spec.md other than the check-off edits. P0-T2 verifies the amended text is the text on disk.
5. **Fail-before exception name.** The dossier of P5-T1 is named `fail-before-exception.<timestamp>.md` with the timestamp read from the host clock when it is written, as the evidence conventions prefer; it is the only evidence file in this plan whose name is not fixed, and P8-T41 locates it by the pattern `fail-before-exception.*.md` (exactly one match required).

## Requirement sources

- Acceptance criteria: `docs/features/active/2026-10-02-focus-and-theme-tests-leak-shared-dispatcher-setup-968/spec.md`, section `## Acceptance Criteria`: thirty-two checkbox lines `- [ ] AC1:` through `- [ ] AC32:`, each on one line. The check-off edit changes only `- [ ] ACn:` to `- [x] ACn:`.
- Design records: `docs/features/active/2026-10-02-focus-and-theme-tests-leak-shared-dispatcher-setup-968/research/2026-10-02T05-50-dispatcher-pin-call-sites-research.md` (sections 1, 2.1, 3, 4, 5 and 6) and `docs/features/active/2026-10-02-focus-and-theme-tests-leak-shared-dispatcher-setup-968/research/2026-10-02T22-20-qfc-datamodel-972-fold-research.md` (sections 1 to 8); both read-only.
- Issue metadata: `docs/features/active/2026-10-02-focus-and-theme-tests-leak-shared-dispatcher-setup-968/issue.md` carries `- Work Mode: full-bug` at line 12 and the binding `## Coordinator Scope Amendment (2026-10-02T22-15, binding)` at lines 65 to 77. It is not an acceptance-criteria source.
- Structural reference: `docs/features/active/2026-09-30-quickfiler-tests-depend-on-wall-clock-timing-950/plan.2026-10-01T07-11.md` and its evidence folder (bootstrap, command-macro, coverage-route and evidence conventions); every citation below was re-derived against this worktree, not carried from that plan.

## Write Set (every file this plan creates or modifies)

Code files (the only paths outside the feature folder this plan may change; spec "Files/modules to change" including the amendment 1.2 folded scope):

- `QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixture.cs` (modified: pin counter, ownership flag, counted dispose, D1 docs)
- `QuickFiler.Test/Controllers/QfcItemController.FocusAndThemeTests.cs` (modified: two dead calls deleted, D5, D6)
- `QuickFiler.Test/Controllers/QfcItemController.TestSupport.cs` (modified: D2 wrapper doc, D5 shared-helper doc)
- `QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixtureTests.cs` (modified: D3 doc rewrite, R4 pin removal, D4 try/finally, which delivers #972 item 5)
- `QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherPinCountTests.cs` (new: regression test and three specification tests)
- `QuickFiler.Test/QuickFiler.Test.csproj` (modified: three new `Compile Include` items)
- `QuickFiler.Test/TestSupport/SynchronousBackgroundWorker.cs` (new: the shared synchronous worker and its starter; #972 item 1)
- `QuickFiler.Test/TestSupport/ArmingFakeTimeProvider.cs` (new: the armed-timer signal; #968 liveness comment)
- `QuickFiler.Test/Controllers/QfcDatamodelLivenessTests.cs` (modified: nested helper removed, caller-owned workers, `StartHeldOpenLoader` signature, test 1 rewrite, context scope helper)
- `QuickFiler.Test/Controllers/QfcDatamodelTeardownTests.cs` (modified: nested helper removed, starter retargeted)
- `QuickFiler.Test/Controllers/QfcInitEmailQueueZeroBatchTests.cs` (modified: nested helper removed, three `using` blocks, doc rewording)
- `QuickFiler.Test/Controllers/QfcDatamodelTests.cs` (modified: sibling liveness test rewrite, two `using` blocks)
- `QuickFiler/Controllers/QfcDatamodel.cs` (production; modified: four caller-free members, their commented-out references and the empty region removed; one `nameof` retargeted; #972 item 3)
- `QuickFiler/Controllers/QfcDatamodel.QueueProcessing.cs` (production; modified: `_remainingLoadActive` doc comment rewritten and the stale `TryUnhookOrReplace` line range dropped; comment-only; #972 item 2)

Fourteen code paths: two production, twelve under `QuickFiler.Test/`. No other project file changes.

Feature documents (the feature folder is committed by the branch's existing docs commits; this plan commits only its own additions and check-offs):

- `docs/features/active/2026-10-02-focus-and-theme-tests-leak-shared-dispatcher-setup-968/spec.md` (acceptance-criteria check-off edits only)
- `docs/features/active/2026-10-02-focus-and-theme-tests-leak-shared-dispatcher-setup-968/plan.2026-10-02T05-42.md` (task check-off edits only)

Evidence files, all new, all fixed names except the dossier:

- `FEATURE/evidence/baseline/`: `phase0-instructions-read.md`, `scope-and-anchor.md`, `bootstrap-sdk.md`, `bootstrap-tool-restore.md`, `bootstrap-nuget-restore.md`, `analyzer-alignment.md`, `bootstrap-dotnet-coverage.md`, `csharpier-check-baseline.md`, `msbuild-analyzer-baseline.md`, `msbuild-nullable-baseline.md`, `census-baseline.md`, `fold-census-baseline.md`, `concurrent-set-baseline.md`, `datamodel-set-baseline.md`, `stall-probe.md`, `coverage-summary.md`, `coverage-jacoco-projection.md`, `toolchain-baseline.md`, `phase0-commit.md`
- `FEATURE/evidence/regression-testing/`: `pin-count-file-census.md`, `fail-before-build.md`, `fail-before-pin-count.md`, `specification-tests-before-fix.md`, `pass-after-build.md`, `pass-after-pin-count.md`, `fold-build.md`, `datamodel-set-after-consolidation.md`, `fail-before-exception.<timestamp>.md`, `liveness-build.md`, `liveness-pass-after.md`, `liveness-sensitivity-check.md`, `liveness-revert-build.md`, `liveness-pass-after-revert.md`, `implementation-build.md`, `pin-count-class-pass-after.md`, `fixture-class-pass-after.md`, `focus-and-theme-class-pass-after.md`, `concurrent-set-test-summary.md`, `datamodel-set-test-summary.md`
- `FEATURE/evidence/qa-gates/`: `fixture-change-census.md`, `test-edit-census.md`, `qfc-datamodel-legacy-callers.md`, `fold-edit-census.md`, `liveness-edit-census.md`, `queue-processing-comment-census.md`, `scoped-format.md`, `post-format-census.md`, `implementation-commit.md`, `call-site-census.md`, `prohibited-constructs-grep.md`, `csharpier-format-final.md`, `csharpier-check-final.md`, `msbuild-analyzer-final.md`, `msbuild-nullable-final.md`, `coverage-summary.md`, `coverage-jacoco-projection.md`, `coverage-comparison.md`, `toolchain-final.md`, `file-line-counts.md`, `footprint-scope.md`, `evidence-hygiene.md`, `final-commit.md`
- `FEATURE/evidence/other/`: `ac-status-summary.md` (the planner review records `planner-review.2026-10-02T22-44.md` and the round-1 records already exist in this folder and are not written by the executor)

Files this plan must not touch, stated so the executor fails closed rather than infers: every file under QuickFiler/ other than the two production Write Set paths, every file under UtilitiesCS/, UtilitiesCS.Test/ and every other project, QuickFiler.Test/Helper Classes/EmailMoveMonitorTests.cs, every other file under QuickFiler.Test/ not listed above (including QfcDatamodelRethrowTests.cs, QfcQueuePurePathsTests.cs and QfcHomeControllerRunAsyncHighConfidenceTests.Part3.cs, which read `_remainingLoadActive` by reflection and are unaffected), QuickFiler/Controllers/QfcDatamodel.FrameBuilding.cs, QuickFiler/Interfaces/IQfcDatamodel.cs, TaskMaster.runsettings, scripts/vscode/TaskMaster.cli.runsettings, every file under scripts/, every file under .github/, every file under .claude/ (uncommitted .claude/agent-memory/ files are session memory and are never staged by this plan), every file under docs/features/potential/, and both research documents' content. The one temporary edit this plan makes (P5-T8, the sensitivity check) is reverted inside the same task and is never committed. No raw test-result document (trx), raw coverage document (cobertura, coverage, coveragexml) or msbuild log is copied into the feature folder under any name; raw documents stay under the repository coverage directory, which .gitignore line 150 ignores.

## AC identity table

Each ID names one checkbox in the spec's `## Acceptance Criteria` section, in document order; the label `ACn:` is part of the line text.

| ID | Opening words of the criterion | Evidence read by its check-off task |
|---|---|---|
| AC1 | Counted pin, non-last release | `regression-testing/pass-after-pin-count.md`, `qa-gates/coverage-summary.md` |
| AC2 | Counted pin, last release reverts only the fixture's own seeding | same two artifacts |
| AC3 | A foreign transaction value is never nulled by pin release | same two plus `regression-testing/fixture-class-pass-after.md`, `qa-gates/post-format-census.md` |
| AC4 | Ownership flag is cleared on the last release | `regression-testing/pass-after-pin-count.md`, `qa-gates/coverage-summary.md` |
| AC5 | Fail-before and pass-after evidence isolates the fixture change | `regression-testing/fail-before-pin-count.md`, `regression-testing/pass-after-pin-count.md`, `regression-testing/pin-count-file-census.md` |
| AC6 | The pin-count test class labels its tests | `qa-gates/post-format-census.md`, `regression-testing/specification-tests-before-fix.md` |
| AC7 | The dead theme-test calls are removed | `qa-gates/post-format-census.md`, `regression-testing/focus-and-theme-class-pass-after.md` |
| AC8 | Gated-caller census invariant holds | `qa-gates/call-site-census.md` |
| AC9 | The pin counter and install-ownership flag are private statics | `qa-gates/post-format-census.md` |
| AC10 | All existing fixture tests pass unchanged in behaviour | `regression-testing/fixture-class-pass-after.md`, `qa-gates/post-format-census.md` |
| AC11 | Fixture documentation describes the counted pin | `qa-gates/post-format-census.md` |
| AC12 | Wrapper documentation describes the counted pin | `qa-gates/post-format-census.md` |
| AC13 | The second-caller transaction test's doc no longer asserts the obsolete invariant | `qa-gates/post-format-census.md` |
| AC14 | The second-caller transaction test releases its gate on any throw (D4; closes #972 item 5) | `qa-gates/post-format-census.md`, `regression-testing/fixture-class-pass-after.md` |
| AC15 | The duplicated viewer helper is removed | `qa-gates/post-format-census.md`, `regression-testing/focus-and-theme-class-pass-after.md` |
| AC16 | The theme-test arrange comment is corrected | `qa-gates/post-format-census.md` |
| AC17 | `EnsureSynchronizationContext` is unchanged | `qa-gates/post-format-census.md` |
| AC18 | File-size limit | `qa-gates/file-line-counts.md` |
| AC19 | No prohibited constructs | `qa-gates/prohibited-constructs-grep.md` |
| AC20 | No production code change outside the folded scope (amended) | `qa-gates/footprint-scope.md` |
| AC21 | The new test file is built and discovered | `qa-gates/post-format-census.md`, `qa-gates/coverage-summary.md` |
| AC22 | Full toolchain pass | `qa-gates/toolchain-final.md` |
| AC23 | Coverage not reduced | `qa-gates/coverage-comparison.md` |
| AC24 | Parallel run of the three classes together passes | `regression-testing/concurrent-set-test-summary.md` |
| AC25 | One shared synchronous worker helper (#972 item 1) | `qa-gates/post-format-census.md`, `regression-testing/datamodel-set-test-summary.md` |
| AC26 | The producer-liveness comment matches post-fix behaviour (#972 item 2) | `qa-gates/post-format-census.md`, `qa-gates/queue-processing-comment-census.md` |
| AC27 | Caller-free legacy members are removed (#972 item 3) | `qa-gates/qfc-datamodel-legacy-callers.md`, `qa-gates/post-format-census.md`, `qa-gates/msbuild-analyzer-final.md`, `qa-gates/msbuild-nullable-final.md` |
| AC28 | The datamodel file sits well under the size limit | `qa-gates/file-line-counts.md`, `qa-gates/qfc-datamodel-legacy-callers.md` |
| AC29 | Removed production lines carry no coverage loss | `qa-gates/post-format-census.md`, `qa-gates/qfc-datamodel-legacy-callers.md`, `qa-gates/coverage-comparison.md` |
| AC30 | Test-owned workers are disposed (#972 item 4) | `qa-gates/post-format-census.md`, `regression-testing/datamodel-set-test-summary.md` |
| AC31 | The dequeue-liveness tests use explicit completion signals | `qa-gates/post-format-census.md`, `regression-testing/fail-before-exception.<timestamp>.md`, `regression-testing/liveness-sensitivity-check.md`, `regression-testing/liveness-pass-after-revert.md` |
| AC32 | The four datamodel test classes pass together | `regression-testing/datamodel-set-test-summary.md` |

## Verified tree facts (re-derived against this worktree while authoring; every count was re-read in this revision pass)

Line totals are content-line counts (the Grep tool's count of lines matching `^`, which equals `git grep -c ""`); every one of the ten existing Write Set `.cs` files ends every line with a carriage return (CRLF; the `\r$` count equals the line count for each), and `.gitattributes` line 4 sets `* text=auto`.

1. `QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixture.cs` is 342 lines. Class doc 12 to 31 (`/// </para>` at 30, `/// </summary>` at 31; design note on `EnsureDispatcher` never taking the gate at 25 to 30). Statics 34 to 38 (`_parkedDispatcher` at 38), counters 44 to 46. `Exchange` 77 to 85; `CompareExchange` declared at 92 with its `lock (FieldLock)` at 94. `EnsureDispatcher` doc 116 to 121 (line 120: `returned scope is optional: a discarded scope leaks exactly as the pre-fix helper did.`); declaration 122; comment 124 to 125; `Dispatcher parked = GetParkedDispatcher();` 126; blank 127; body 128 to 137 (`lock (FieldLock)` 128, `if (DispatcherField.GetValue(null) == null)` 130, `return new EnsureScope(parked);` 133, `return new EnsureScope(null);` 137); method close 138. `TransactionGateAcquireTimeoutMs` at 146. Parked thread name at 231. `EnsureScope` doc 243 to 248 (line 245: `static still holds the exact instance this scope installed. A scope that installed nothing`, line 246: `carries <c>null</c> and is a no-op, which is what keeps a discarded scope from clobbering a`); class 249 to 274 with `UiThreadDispatcherFixture.CompareExchange(_installed, null);` at 271. `UiThreadDispatcherTransaction` declared at 284, its `CompareExchange` call at 336. Counts: `lock (FieldLock)` 4 (66, 79, 94, 128); `CompareExchange(` 3 (92, 271, 336); `return new EnsureScope(` 2; `leaks exactly` 1; `installed nothing carries` 0 as a single-line token (the phrase wraps across 245 and 246, so that AC11 grep is satisfied vacuously before the change; the gate therefore also uses the single-line tokens `leaks exactly` and `A scope that installed nothing`, each 1 before and 0 after, and records `installed nothing carries` at 0 before and 0 after with the vacuity noted); `_pinCount` 0; `_fixtureInstalledParked` 0; `pins for the process lifetime` 0; `install-ownership flag` 0; bare `EnsureDispatcher` 5 lines (26, 27, 122, 195, 244).
2. `QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixtureTests.cs` is 470 lines. `private const int GateTimeoutMs = 60000;` at 33; `[Timeout(GateTimeoutMs)]` 8 occurrences; `[TestMethod]` 8. R1 declared 44 (transaction 50 to 52, `Install(liveA)` 56, pin 59 to 60 with `QfcItemControllerTestSupport.EnsureUiThreadDispatcher();` on 60, `ensureScope.Dispose();` 62, `transaction.Dispose();` 81 and 91, `ShutdownDispatcher(liveA)` 96). R2 declared 107 (transaction 110 to 112, `Install(null)` 116, pin 119, dispose 121, `transaction.Dispose();` 137 and 147). R3 declared 157 (transaction 160 to 162, `Install(null)` 165, pin 166, disposes 169 and 171, `transaction.Dispose();` 188). R4 doc 192 to 209 with the `<para>` 196 to 208 (line 205: `/// Invariant for future editors: no other class may dispose an ensure scope holding the`, line 206: `/// parked dispatcher (W2), and UiThread.Initialize (W5) must not latch during this test;`); attributes 210 and 211; declared 212; `// Arrange` 214; `liveA` 215; `try` 216; `{` 217; transaction A 218 to 220; `using (` 221; `IDisposable baseline = QfcItemControllerTestSupport.EnsureUiThreadDispatcher()` 222; `)` 223; `{` 224; `Dispatcher original = UiThreadDispatcherFixture.Current;` 225; `transactionA.Install(liveA);` 226 (unique in the file); `using (var secondCallerStarted = new ManualResetEventSlim(false))` 228; waiter 232 to 247 with its `finally` at 243 and `transactionB.Dispose();` 245; `secondCallerStarted.Wait();` 250; `transactionA.Dispose();` 251 (unique); assertions 255 to 268 with `issue #230 lost update` at 267 (unique in the file); `}` 269 (closes the `secondCallerStarted` using), `}` 270 (closes the `baseline` using), `}` 271 (closes the `try`), `finally` 272, `{` 273, `QfcItemControllerTestSupport.ShutdownDispatcher(liveA);` 274, `}` 275, method close 276. R5 declared 285. `EnsureUiThreadDispatcher()` 4 lines (60, 119, 166, 222); bare `EnsureDispatcher` 10 lines (44, 60, 70, 107, 119, 128, 157, 166, 198, 222); `no other class may dispose` 1; `(W5) must not latch` 1; `.BeginTransactionAsync()` 12 lines in the file. No `Issue #968` text.
3. `QuickFiler.Test/Controllers/QfcItemController.FocusAndThemeTests.cs` is 497 lines, class `QfcItemController_FocusAndThemeTests` at 27, `[TestMethod]` 17. Private `BuildExecutingViewer` 99 to 115 with blank lines at 98 and 116; `/// <summary>` of `EnableHandlelessThemeInvoke` at 117. Comment block 181 to 186 (line 182 ends `BuildExecutingViewer() executes the`). `var viewer = BuildExecutingViewer();` at 193, 213, 235, 254, 314, 331 and 367 (seven lines, all with twelve leading spaces). `SetThemeDark_FromNormal_SelectsDarkNormalTheme` 448 to 462: comment 450 to 451, `QfcItemControllerTestSupport.EnsureUiThreadDispatcher();` 452, `var controller = new FocusController();` 453. `SetThemeLight_FromNormal_SelectsLightNormalTheme` 465 to 478: `// Arrange` 467, the ensure call 468, `var controller = new FocusController();` 469. Counts: `EnsureUiThreadDispatcher` 2; `BuildExecutingViewer` 9 (99, 182, and the seven callers); `private static Mock<IItemViewer> BuildExecutingViewer` 1; `QfcItemControllerTestSupport.BuildExecutingViewer()` 0.
4. `QuickFiler.Test/Controllers/QfcItemController.TestSupport.cs` is 440 lines. `EnsureSynchronizationContext` doc 85 to 89, method 90 to 96. `BuildColorTheme` remarks 161 to 168 and method 169 to 181 (injects a `Mock<IUiDispatcher>` whose `InvokeAsync` returns `Task.CompletedTask` at 175 to 179). `EnsureUiThreadDispatcher` doc 216 to 237 (`Becomes moot` at 226, `leaks exactly` at 233, `still delegate to a callee` at 219), declaration 238, body `UiThreadDispatcherFixture.EnsureDispatcher();` 239. `BuildExecutingViewer` doc 282 to 288 (line 287: `/// <c>QfcItemController.FocusAndThemeTests.cs</c>, which is not reachable from another test file.`), method 289 to 305. `StartRunningDispatcher` 251 to 271; `ShutdownDispatcher` 277 to 280. Bare `EnsureDispatcher` 2 lines (238, 239). The only other caller of the shared helper is `QuickFiler.Test/Controllers/QfcItemController.MailActionsTests.cs` line 203.
5. Repository-wide call sites (Grep over `*.cs`, pattern `EnsureUiThreadDispatcher|EnsureDispatcher`): 20 lines in 5 files, exactly as research section 2.1 records (fixture 5, test support 2, fixture tests 10, focus-and-theme 2, `QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part2.cs` line 124, a comment). Lines matching `EnsureUiThreadDispatcher\(\)|EnsureDispatcher\(\)`: 9 (fixture 122, test support 238 and 239, fixture tests 60, 119, 166, 222, focus-and-theme 452, 468). `BeginTransactionAsync\(` matches 23 lines in 6 files (the census positive control). `[DoNotParallelize]` occurs in QuickFiler.Test only at `Helper Classes/EmailMoveMonitorTests.cs` 24 and `Helper Classes/ViewerQueueStaticWrapperTests.cs` 11, both outside the Write Set.
6. Theme path (read-only context for D6): `QuickFiler/Controllers/QfcItemController.FocusAndTheme.cs` 274 to 286 (`SetThemeDark` calls `_themes["DarkNormal"].SetQfcTheme(async)` then sets `_activeTheme`); `UtilitiesCS/HelperClasses/ThemeHelpers/Theme.cs` 427 to 445 (`SetQfcTheme(bool async)`: the async branch is `_uiDispatcher.InvokeAsync(() => SetQfcTheme());` at 431; the former static read is the commented line 441); `UtilitiesCS/Threading/UiThread.cs` 266 to 285 (the `Dispatcher` getter throws `InvalidOperationException` when `_dispatcher` is null; private backing field at 285). No statement on the theme path reads `UiThread.Dispatcher`.
7. `QuickFiler.Test/SetupAssemblyInitializer.cs` 14 to 25: `[AssemblyInitialize]` installs an assembly resolver and WinForms rendering defaults and does not write `UiThread._dispatcher`, so a class run alone starts from a null baseline.
8. `QuickFiler.Test/QuickFiler.Test.csproj`: `<TargetFrameworkVersion>v4.8.1</TargetFrameworkVersion>` at 17, `OutputPath` `bin\Debug\` at 35, no `LangVersion` element; `Compile Include` items for the Write Set files at 155 (QfcDatamodelTests), 157 (QfcDatamodelLivenessTests), 161 (QfcInitEmailQueueZeroBatchTests), 183 (QfcDatamodelTeardownTests), 200 (TestSupport), 201 (fixture), 203 (fixture tests), 212 (focus-and-theme), 227 (`TestSupport\WinFormsPumpHost.cs`), 228 (`TestSupport\DedicatedWorkerThread.cs`), 229 (`TestSupport\WinFormsPumpHostTests.cs`), each with four leading spaces; no item for the pin-count file, for `TestSupport\SynchronousBackgroundWorker.cs` or for `TestSupport\ArmingFakeTimeProvider.cs`. The existing shared helpers in `QuickFiler.Test/TestSupport/` are `internal` in namespace `QuickFiler.Test.TestSupport` (`DedicatedWorkerThread.cs` lines 4 and 21); nine files already carry `using QuickFiler.Test.TestSupport;`, placed in alphabetical order among the other `using` lines (for example `Controllers/QfcItemController.SeamFactoryTests.cs` line 13, between `using QuickFiler.Interfaces;` and `using TaskVisualization;`). No file under `QuickFiler.Test/TestSupport/` or among the four datamodel test files carries a `#nullable` directive.
9. `scripts/vscode/TaskMaster.cli.runsettings` lines 4 to 7 set Workers 0 and Scope ClassLevel. `scripts/vscode/Invoke-MSTestWithCoverage.ps1` (461 lines): `Get-DotnetCoverageArgumentList` appends `/InIsolation`, `/TestCaseFilter:TestCategory!=LiveOutlook`, the results directory and the trx logger at 89 to 93 with no extension point; `Invoke-DotnetCoverageCollection` throws `MSTest with coverage failed with exit code` at 262 after the collector exits non-zero, before post-processing; defaults `coverage\test-results` and `mstest-coverage-run.trx` at 297 to 298; discovery 348 to 355 filters `bin\Debug` and a `.claude` segment; post-processing 399 to 402; `First-party coverage:` printed at 410; projection 415 to 423; trx summary 430 to 447; entry guard 459 to 461 so dot-sourcing is safe. Helper functions: `Get-TrxRunSummary` and `Format-TrxRunSummary` in `scripts/vscode/Invoke-MSTest.TrxSummary.ps1` (12, 103); `ConvertTo-KoverageCoberturaXml` in `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1` (407); `Get-CoberturaFirstPartyCoverageReport` in `scripts/vscode/Invoke-MSTestWithCoverage.FirstParty.ps1` (123; the line format `First-party coverage: lines a/b (p%), branches c/d (q%)` at 117 to 120); `Assert-CoberturaLineCoverageThreshold` and `Assert-CoberturaBranchCoverageThreshold` in `scripts/vscode/Invoke-MSTestWithCoverage.Threshold.ps1` (3, 58); `ConvertTo-JacocoPackageProjection` and `Assert-JacocoProjectionReconciliation` in `scripts/vscode/Invoke-MSTestWithCoverage.Projection.ps1` (14, 83). The Helpers file dot-sources its part files, so dot-sourcing it alone resolves the FirstParty, Threshold and Projection functions, as the #950 run's CMD-COVERAGE-POST showed.
10. `.gitignore`: `*.coverage` 140, `*.coveragexml` 141, `*.trx` 146, `coverage/*` 150, `!coverage/.gitkeep` 151, `[Bb]in/` 26. `.csharpierignore` excludes `**/evidence/**` (4) and `*.csproj` (12). `global.json` pins SDK 8.0.205 under `.dotnet-sdk` with `latestFeature` roll-forward (lines 2 to 9); `dotnet-tools.json` at the repository root pins csharpier 1.2.6 (line 6). `scripts/vscode/Install-RepoDotNetSdk.ps1` defaults `-Version` to `8.0.205` (line 3); `scripts/vscode/Invoke-Restore.ps1` takes `SolutionPath`, `Configuration` and `Platform` (lines 1 to 10). `coverage.config` exists at the repository root and carries no `QfcDatamodel` entry.
11. Branch `bug/focus-and-theme-tests-leak-shared-dispatcher-setup-968` (the worktree's `.git` metadata reads `ref: refs/heads/bug/focus-and-theme-tests-leak-shared-dispatcher-setup-968`, and that ref resolves to `87cca65ede6a900cb5c4e5cce93ff4409ed08730` at authoring); BASE `94287369908cc920b21b0e3256314f988ad7d2f5` (origin/main at the branch cut, supplied by the caller and confirmed by both research headers). The branch now carries documentation-only commits above BASE (the coordinator reports `53d975270`, `d096f1250`, `4c6de5e84` and `87cca65ed`, and one further docs-only commit carrying this revised plan will precede execution), and `git diff --name-status 94287369 HEAD` lists only paths under FEATURE plus the two promoted records `docs/features/potential/promoted/2026-10-02-focus-and-theme-tests-leak-shared-dispatcher-setup.md` and `docs/features/potential/promoted/2026-10-02-qfc-datamodel-950-review-residuals.md`; both records exist on the tree (Glob). The expectation is therefore stated by set membership, not by commit list: every path committed above BASE before P0-T3 runs must be under FEATURE or be one of those two records. HEAD is recorded as an observation in P0-T3, never as an expectation. This planning session has no shell, so the `.dotnet-sdk`, `packages` and `bin` trees were not probed; every bootstrap task is guarded and gated on its post-task marker.
12. Host constraint carried from #950 on this workstation (its `evidence/baseline/stall-probe.md`): the four UtilitiesCS.Test shell-icon classes did not stall but one test failed with `Win32 handle that was passed to Icon is not valid`, so the runner route (which throws before post-processing on any failure) could not produce figures and the DIRECT route was used. Whether that reproduces today is unknown, so P0-T16 measures it and the result selects the coverage route (D-6).
13. The #950 run observed the FluentAssertions `BeSameAs` failure shape on this fixture: `Expected observedByB to refer to <null> because ..., but found System.Windows.Threading.Dispatcher { ... Name = "UiThreadDispatcherFixture.ParkedDispatcher" ... }`. The pin-count regression inverts the operands, so its expected message is `Expected afterFirstRelease to refer to System.Windows.Threading.Dispatcher { ... Name = "UiThreadDispatcherFixture.ParkedDispatcher" ... } because a holder that did not take the last pin must not lose the dispatcher, but found <null>.`; the gate reads `to refer to`, `ParkedDispatcher`, the because text and `but found <null>`, never the subject name (caller identification can fail and then prints `object`). The same `BeSameAs` shape over two `Task` operands is what the P5-T8 sensitivity check reads (`to refer to` plus the because fragment).
14. `QuickFiler/Controllers/QfcDatamodel.cs` is 495 lines; `[ExcludeFromCodeCoverage]` at 25 on the class declaration at 26 (a type-level attribute on one partial declaration applies to the whole type, so it covers `QfcDatamodel.QueueProcessing.cs` and `QfcDatamodel.FrameBuilding.cs` as well). `logger` 28 to 30; constructors 34 to 54 assign `RemainingEmailLoader = LoadRemainingEmailsToQueueAsync;` at 40 and 52 (method-group conversion to `Func<CancellationToken, Task<bool>>`, which binds the one-argument overload only); `Cleanup` 77 to 103 with the commented-out field nulls `//_blockingQueue = null;`, `//_priorityQueue = null;`, `//_queues = null;` at 99 to 101; `#region Private Variables` 107, blank 108, the duplicate `private static readonly log4net.ILog log = log4net.LogManager.GetLogger(` 109 to 111, `_globals` 112; `RemainingEmailLoader` doc 128 to 141 with the cref `LoadRemainingEmailsToQueueAsync(CancellationToken)` at 130; `WorkerStarter` 144 to 152; `SetupWorker` 188 to 195 with the commented subscription `//worker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(Worker_RunWorkerCompleted);` at 194; `Worker_DoWork` 197 to 241 with the commented calls `//e.Result = await LoadRemainingEmailsToQueueAsync(bw, _token);` 209 and `//e.Result = LoadRemainingEmailsToQueue(bw, _token);` 210, `_remainingLoadTask = loaderTask;` 218, `_remainingLoadActive = false;` 227; blank 242; comment 243 to 245; `Worker_RunWorkerCompleted` 246 to 265; blank 266; `#endregion BackgroundWorker` 267; `InitEmailQueue` 271 to 315 (`_remainingLoadActive = true;` 284 and 311, `WorkerStarter(worker);` 285 and 312); `InitEmailQueueAsync` 317 to 333; the one-argument `LoadRemainingEmailsToQueueAsync(CancellationToken cancel)` 335 to 376 with `//logger.Debug($"{nameof(LoadRemainingEmailsToQueue)} Task cancelled");` at 363 and the live `$"{nameof(LoadRemainingEmailsToQueue)} Error. \n {e.Message}\n{e.StackTrace}"` at 369; blank 377; synchronous `private bool LoadRemainingEmailsToQueue(BackgroundWorker bw, CancellationToken token)` 378 to 416 (its own `nameof` uses at 404 and 410); blank 417; two-argument `private async Task<bool> LoadRemainingEmailsToQueueAsync(` 418 to 465 (`#pragma warning disable CS0618` 436, `#pragma warning restore CS0618` 457, the file's only pragmas; commented `nameof(LoadRemainingEmailsToQueueAsync)` 462); blank 466; `#endregion Email Queue Initial Setup` 467; blank 468; `#region Linked List Locking` 469; blanks 470 to 471; `#endregion Linked List Locking` 472; blank 473; `#region Event Handlers` 474; `Application_NewMailEx` 476 to 491. Seven `#region` and seven `#endregion` lines. Counts: `Worker_RunWorkerCompleted` 2 (194, 246); `nameof(LoadRemainingEmailsToQueue)` 4 (363, 369, 404, 410); `nameof(LoadRemainingEmailsToQueueAsync)` 1 (462); `LoadRemainingEmailsToQueueAsync(` 4 (130, 209, 335, 418); `LoadRemainingEmailsToQueue(BackgroundWorker bw` 1; `log4net.ILog log =` 1; `log4net.ILog logger =` 1; `Linked List Locking` 2; `#pragma` 2; `[ExcludeFromCodeCoverage]` 1; `//e.Result =` 2; `//_blockingQueue = null;` 1; `//worker.RunWorkerCompleted` 1; `ForEachAwaitWithCancellationAsync` 2 (the comment at 431 and the call at 440); `: IQfcDatamodel` 1.
15. Zero-caller proof for the four members (addendum section 3.2 and its `## Numeric Derivation Evidence`, re-run in this pass). Grep `Worker_RunWorkerCompleted|LoadRemainingEmailsToQueue\b|LoadRemainingEmailsToQueueAsync|Linked List Locking` over `*.cs`: 25 lines in 5 files (17 in `QfcDatamodel.cs`: 40, 52, 130, 194, 209, 210, 246, 335, 363, 369, 378, 404, 410, 418, 462, 469, 472); outside `QfcDatamodel.cs` the hits are `QuickFiler/Controllers/QfcHomeController.cs` 92, 132, 344, 379 (the `QfcHomeController` method of the same name), `QuickFiler.Test/Controllers/QfcHomeControllerRunAsyncTests.cs` 325 and 376 (a test method name, and a `GetMethod("Worker_RunWorkerCompleted", ...)` invoked on `_controller`, a `QfcHomeController`, at 373 to 380), and doc prose at `QuickFiler.Test/Controllers/QfcDatamodelLivenessTests.cs` 104 and `QuickFiler.Test/Controllers/QfcInitEmailQueueZeroBatchTests.cs` 28 (both name the one-argument loader). Grep `\blog\b` over `QuickFiler/Controllers/QfcDatamodel*.cs`: 3 lines (the declaration at `QfcDatamodel.cs` 109, prose at `QfcDatamodel.QueueProcessing.cs` 71 and 90). String-literal and reflection sweep `"Worker_RunWorkerCompleted"|"LoadRemainingEmailsToQueue|"log"|nameof\(log\)|GetField\("log` over `*.cs`: 2 lines (`QfcHomeControllerRunAsyncTests.cs` 376, classified above, and the cref at `QfcDatamodel.cs` 130). `QuickFiler/Interfaces/IQfcDatamodel.cs` declares `DequeueNextItemGroupAsync` (103, 117), `DequeueNextItemGroupWithOutcomeAsync` (131), `DequeueNextItemGroup` (138), `UndoMove` (139), `MovedItems` (140), `InitEmailQueue` (141), `InitEmailQueueAsync` (142), `Complete` (148), `QuiesceLoaderAsync` (164) and `Cleanup` (166), none of the four. `QuickFiler/Controllers/QfcDatamodel.FrameBuilding.cs` references none of the four. `InternalsVisibleTo` grants under `QuickFiler/`: `QuickFiler.Test` (Properties/AssemblyInfo.cs 5, Controllers/QfcHomeController.cs 15) and `DynamicProxyGenAssembly2` (Legacy/IAcceleratorCallbacks.cs 5, Controllers/QfcHighConfidencePreFilter.cs 11); all four members are `private`. Primary and cross-check member sets are both exactly {`log`, `Worker_RunWorkerCompleted`, `LoadRemainingEmailsToQueue`, the two-argument `LoadRemainingEmailsToQueueAsync`}, count 4 and 4. P4-T1 re-runs both strategies at execution time.
16. `QuickFiler/Controllers/QfcDatamodel.QueueProcessing.cs` is 413 lines. `_remainingLoadActive` doc 15 to 23 (line 17 contains `each <c>RunWorkerAsync()</c> call`, line 22 contains `written on the worker thread and read by dequeue callers`), declaration 24; `_remainingLoadTask` doc 37 to 42, declaration 43; `QuiesceLoaderAsync` 48 to 66 with the comment `// Snapshot before the check: the field is written on the worker thread, so reading it` at 52; `TryUnhookOrReplace` declared at 146; the doc of `DequeueWithHighConfidenceGateWithOutcomeAsync` 280 to 291 with line 285 reading `/// <c>UnhookItem</c> throw path <see cref="TryUnhookOrReplace"/> (:31-66) removes the failed`; the gate construction 299 to 309 with `() => _remainingLoadActive,` at 305 (the `sourceActive` lambda; the preceding `null,` at 304 and the following `firstBatchDeadline,` at 306) and `await gate.DequeueAsync(quantity, timeOut, _token);` at 311; `TryUnhookOrReplace(ref nodes, i);` 364; `WaitForQueue` 404 to 411 with `while (_remainingLoadActive && (_masterQueue?.Count < quantity))` at 406. Counts: `RunWorkerAsync` 1; `written on the worker thread and read` 1; `written on the worker thread` 2; `(:31-66)` 1; `TryUnhookOrReplace` 3; `WorkerStarter` 0; `RemainingEmailLoader` 1 (line 18); `share no other fence` 0; `honest producer-liveness signal` 1; `() => _remainingLoadActive,` 1; `() => false,` 0. Repository-wide `_remainingLoadActive|_remainingLoadTask` over `*.cs`: 20 lines in 7 files (production: `QfcDatamodel.cs` 218, 227, 284, 311; `QueueProcessing.cs` 24, 43, 54, 305, 406; tests by reflection only: `QfcDatamodelLivenessTests.cs` 169, `QfcDatamodelTests.cs` 114, 126, 266, 279, `QfcDatamodelTeardownTests.cs` 121, 151, 204, 233, `QfcQueuePurePathsTests.cs` 246, `QfcHomeControllerRunAsyncHighConfidenceTests.Part3.cs` 116). `QuickFiler/Controllers/QfcStreamingDequeueConfidenceGate.cs` `DequeueAsync` 190 to 301: `alreadyWaitedForEmptySource` 215; empty-take branch 244 to 257 (`sourceCanStillProduce = _sourceActive?.Invoke() == true;` 246, the exhaustion return 247 to 250, `alreadyWaitedForEmptySource = true;` 252, `await _timeProvider.Delay(TimeSpan.FromMilliseconds(timeOut), token).ConfigureAwait(false);` 253 to 255). The three datamodel awaits on the dequeue path carry no `ConfigureAwait(false)` (QueueProcessing 311 and its two callers), so they capture the caller's ambient context.
17. `QuickFiler.Test/Controllers/QfcDatamodelLivenessTests.cs` is 312 lines, `[TestMethod]` 4. Usings 1 to 14 (`using Microsoft.Extensions.Time.Testing;` 9, `using Moq;` 12, `using UtilitiesCS;` 13); class doc 18 to 24 (about the reflection helpers; unchanged); blank 46; nested worker doc 47 to 52, class 53 to 56, blank 57, starter doc 58, starter 59 to 60, blank 61; `DrainableSynchronizationContext` doc 62 to 67 and class 68 to 87 (`Drain()` 78 to 86 asserts the creating thread at 80); `CreateHighConfidenceGlobals` 89 to 98; test 1 doc 100 to 108, attribute 109, declaration 110, body 111 to 164 (`var fake = new FakeTimeProvider();` 114, `var worker = new SynchronousBackgroundWorker();` 127, `model.WorkerStarter = StartSynchronously;` 128, `model.InitEmailQueue(0, worker);` 132, `pending` 138, `fake.Advance` 139, 141 and 156, `await Task.Yield();` 140, 142 and 157, `for (int i = 0; i < 20 && !pending.IsCompleted; i++)` 154, close 164); blank 165; `ReadLivenessFlag` 166 to 172; `StartHeldOpenLoader` doc 174 to 182, declaration 183 to 186, body 187 to 209 (`var worker = new SynchronousBackgroundWorker();` 201, `model.WorkerStarter = StartSynchronously;` 202, `model.InitEmailQueue(0, worker);` 203); test 2 211 to 234 (caller at 221); test 3 236 to 270 (caller at 249 inside `try` 247 to 265); test 4 272 to 310 (caller at 285 inside `try` 283 to 301). `FakeTimeProvider` occurs only at 114 (the `using Microsoft.Extensions.Time.Testing;` directive at 9 does not contain the type name); `SynchronousBackgroundWorker` 4 lines (53, 60, 127, 201); `StartSynchronously` 3 lines (59, 128, 202); `StartHeldOpenLoader` 4 lines (183, 221, 249, 285); `Task.Yield` 3; `fake.Advance` 3; `[Timeout` 0; `using (` 0; `await` 5 lines inside test 1 (124, 140, 142, 157, 163; the whole-file substring count is 10, the other five being doc or string text at 50, 102, 178, 263 and the test-4 lambda at 288).
18. `QuickFiler.Test/Controllers/QfcDatamodelTeardownTests.cs` is 244 lines, `[TestMethod]` 5. `using Moq;` 12; blank 58; nested worker doc 59 to 65, class 66 to 69, blank 70, starter doc 71, starter 72 to 73, blank 74; `/// <summary>` of the first test 75; `using (var worker = new BackgroundWorker { WorkerSupportsCancellation = true })` 180; `using (var worker = new SynchronousBackgroundWorker())` 220; `model.WorkerStarter = StartSynchronously;` 222. `SynchronousBackgroundWorker` 3 lines (66, 73, 220); `StartSynchronously` 2 lines (72, 222); `Duplicated per file` 1 (63).
19. `QuickFiler.Test/Controllers/QfcInitEmailQueueZeroBatchTests.cs` is 232 lines, `[TestMethod]` 3. `using Moq;` 12; remarks 23 to 35 with line 33 `/// the test thread through the nested <c>SynchronousBackgroundWorker</c>, so no test starts a`; `CreateInertRemainingEmailLoader` doc 93 to 100 with line 97 `/// Assigning this delegate before starting a real <see cref="BackgroundWorker"/> is what makes` and 98 `/// it safe to call <see cref="QfcDatamodel.InitEmailQueue(int, BackgroundWorker)"/> with a real` and 99 `/// worker in a unit test.`; blank 113; nested worker doc 114 to 119, class 120 to 123, blank 124, starter doc 125, starter 126 to 127, blank 128; test 1 136 to 154 (`model.WorkerStarter = StartSynchronously;` 143, `IList<MailItem> result = null;` 144, `System.Action act = () =>` 147, `result = model.InitEmailQueue(0, new SynchronousBackgroundWorker());` 148, asserts 151 to 153); test 2 165 to 183 (`model.WorkerStarter = StartSynchronously;` 172, `var worker = new SynchronousBackgroundWorker();` 173, `model.InitEmailQueue(0, worker);` 176, asserts 179 to 182); test 3 195 to 230 (`model.WorkerStarter = StartSynchronously;` 202, `var result = model.InitEmailQueue(2, new SynchronousBackgroundWorker());` 221, asserts 224 to 229). `SynchronousBackgroundWorker` 6 lines (33, 120, 127, 148, 173, 221); `StartSynchronously` 4 lines (126, 143, 172, 202); `Duplicated per file` 1 (117); `using (` 0.
20. `QuickFiler.Test/Controllers/QfcDatamodelTests.cs` is 371 lines, `[TestMethod]` 9. `using Microsoft.Extensions.Time.Testing;` 9, `using Moq;` 12; sibling test `DequeueNextItemGroupAsync_HighConfidenceMode_WaitsWhileSourceWorkerActive` attribute 95, declaration 96, body 97 to 131 (no doc comment; `var fake = new FakeTimeProvider();` 99, `var worker = new BackgroundWorker();` 108, `SetPrivateField(model, "_remainingLoadActive", true);` 114, `pending` 116, `fake.Advance` 118 and 127, `await Task.Yield();` 119, the because text `the datamodel source-active signal must keep polling while the worker can still add candidates` 123, `SetPrivateField(model, "_remainingLoadActive", false);` 126, `IList<MailItem> result = await pending;` 128, `result.Should().BeEmpty();` 130); the next test's attribute at 133; `CreateUninitializedDatamodel` 201 to 202 and `SetPrivateField` 204 to 211 (declared after first use; legal); `WaitForQueue` test 253 to 283 (`var worker = new BackgroundWorker();` 261, `SetPrivateField(model, "_worker", worker);` 262, `await task;` 281, `task.IsCompleted.Should().BeTrue();` 282, close 283). `FakeTimeProvider` at 99, 216, 224, 249, 258 (five lines; the `using Microsoft.Extensions.Time.Testing;` directive at 9 does not contain the type name); `new BackgroundWorker()` 2 lines (108, 261); `Task.Yield` 1; `using (` 0.
21. Helper precedents: `UtilitiesCS.Test/TestHelpers/ArmingBarrierTimeProvider.cs` (55 lines; forwarding decorator with `Armed` 26, `ReArm()` 28, the `CreateTimer` override 39 to 49 that forwards then `_armed.TrySetResult(true)`, and `NewSignal()` 52 to 53 using `TaskCreationOptions.RunContinuationsAsynchronously`); `QuickFiler.Test/Controllers/QfcFormControllerSeamTests.cs` 357 to 367 (`private sealed class CountingTimeProvider : FakeTimeProvider` overriding `public override ITimer CreateTimer(TimerCallback cb, object s, TimeSpan due, TimeSpan p)`, which proves the override compiles in this project and that `TimeProvider.Delay` reaches `CreateTimer` here). Repository-wide `ArmingFakeTimeProvider|NoSynchronizationContext` over `*.cs`: 0 lines (both names are free).
22. Spec tokens at authoring (`docs/features/active/.../spec.md`, 334 lines): `- [ ] AC` 32 lines (275 to 306); `- [x] AC` 0; `- [ ] AC32:` 1; `Amendment 1.2` 1 (line 9); `Amendment 1.1` 1 (line 10); `acquired and released inside a held` 4 (10, 105, 266, 282); `the removal of its baseline pin` 1 (284); `inherited committed set` 2 (10, 294). Issue.md line 12 reads `- Work Mode: full-bug`; its lines 65 to 77 carry the Coordinator Scope Amendment.

## Design decisions (do not redesign)

- **D-1 Fixture fix (spec decision 1, research Approach A).** `UiThreadDispatcherFixture` gains two private statics without initializers, `_pinCount` and `_fixtureInstalledParked`, read and written only while `FieldLock` is held (Delivered Source F-FIELDS). `EnsureDispatcher` increments the count and, when the field is null, writes the parked dispatcher and sets the flag; it always returns `new EnsureScope(parked)` (F-ENSURE-BODY). `EnsureScope` keeps the parked reference; its idempotent `Dispose` decrements under `FieldLock` and, in the same critical section, writes null and clears the flag only when the count reaches zero, the flag is set and the field still references the parked instance (F-SCOPE). The nested class reaches `FieldLock`, `DispatcherField` and the two statics directly, so the re-locking `CompareExchange` helper is not called from the scope. D1 docs: F-CLASSDOC, F-ENSURE-DOC and the F-SCOPE doc.
- **D-2 R4 restructure: option (a), the baseline pin is removed.** The `using (IDisposable baseline = ...)` block that #950 added to `Transaction_SecondCallerCannotInstallUntilTheFirstRestores` is deleted and replaced by a `try/finally` over `transactionA` (Delivered Source R-BODY); the body lines keep their indentation because the `try` block replaces the `using` block one-for-one. Rationale: the pin fenced the gate-free writers of the two theme tests, which Phase 3 deletes; after the deletion the census (P7-T1, P7-T2) proves every remaining pin is acquired and released inside a held transaction, so while `transactionA` holds the gate no other class can write the field and every other transaction restores before it releases, which is all R4's two assertions need; the pin as placed was released after `transactionA.Dispose()` and after `transactionB` completed, so it outlived its gate hold, and it installed `liveA` over a pinned parked value, the only shape in the repository reaching the flag-true-but-field-changed branch. Option (b), releasing the pin before `transactionA.Dispose()`, was rejected because it keeps that shape and leaves the parked dispatcher installed with the flag set after every R4 run. R4's two assertions and their `because` texts are unchanged (gated by R4SPAN tokens). The `try/finally` is D4 and delivers #972 item 5 (spec decision 5, AC14).
- **D-3 Fail-before evidence.** The new test file is compiled against the unmodified fixture (Phase 1) and test 1 is run alone by fully qualified name, tagged `[expect-fail]` (P1-T5); tests 2 to 4 are run on the same unmodified fixture and must pass (P1-T6), which is the evidence for their "passes before and after" labels. The fixture fix alone is then applied (Phase 2) and the four tests are run again (P2-T8). The porcelain spans recorded at P1-T3 and P2-T8 show that the only difference between the two runs is the fixture file.
- **D-4 Temporary state and byte-level checks.** The only temporary edit in this plan is the P5-T8 sensitivity edit, which is reverted inside P5-T8 and proved reverted by an anchored `--exit-code` diff before any later task runs; every other edit is a delivered edit. Build gates use `/t:Build` for the scoped test runs (CMD-BUILD, gated on the test assembly timestamp advancing) and `/t:Rebuild` for the baseline and final analyzer and nullable gates (CMD-REBUILD, gated on zero `Skipping target "CoreCompile"` lines and at least one `Csc` output line per Write Set project).
- **D-5 Hang handling.** Every direct vstest run carries `/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None`; a `Sequence_*.xml` file in the results directory, or a `Timeout` or `Aborted` outcome, is a stop.
- **D-6 Coverage route is selected by a recorded observation.** P0-T16 runs the four shell-icon classes alone and records `STALL-PROBE: CLEAR` or `REPRODUCES`. `COVERAGE-ROUTE: RUNNER` (CLEAR) runs `scripts/vscode/Invoke-MSTestWithCoverage.ps1` verbatim (CLAUDE.md step 4). `COVERAGE-ROUTE: DIRECT` (REPRODUCES) issues the runner's own inner collector invocation with the four-class exclusion appended and post-processes with the runner's own helpers, because the runner hard-codes its filter (fact 9). Both routes yield the same committed forms: the `First-party coverage:` line, the JaCoCo package projection text and the trx-derived summary, transcribed into Markdown. Under DIRECT, AC22 cannot be met as worded (the runner was not run) and its check-off records `AC22: NOT MET (ENVIRONMENTAL: COVERAGE-ROUTE DIRECT)` for the orchestrator's decision, as the #950 run did.
- **D-7 Coverage obligations.** Every changed test file is in a test assembly, which the runner's derived settings exclude from instrumentation (`.*\.Test\.dll$`), and both changed production files belong to the `[ExcludeFromCodeCoverage]` type `QfcDatamodel` (fact 14), which the collector omits from the report altogether (an excluded type is absent, not reported at 0 percent), so no changed line has a coverage figure: `CHANGED-CODE-COVERAGE: NOT MEASURED (TEST ASSEMBLY EXCLUDED; QFCDATAMODEL EXCLUDED BY ATTRIBUTE)` is recorded at both stages, and no per-file or per-class coverage gate is authored on `QfcDatamodel` (spec decision 7). The first-party line and the root counters are recorded at both stages; the repository-wide rate is compared in two branches (denominators within 1 percent: tolerance 0.5 percentage points; otherwise recorded, not gated), because that rate is not reproducible across runs of an identical tree. AC23 is worded without a tolerance, so its check-off is MET only when both printed first-party percentages are not lower than baseline, otherwise NOT MET with the figures and `COVERAGE-VARIANCE` recorded for the orchestrator. The 80 percent line and 75 percent branch floors are applied by the runner or by its threshold functions under DIRECT.
- **D-8 Baseline-relative test outcomes.** The baseline full run (P0-T17) may contain pre-existing failures; they are recorded as `BASELINE-FAILED-SET:` and do not stop Phase 0. The final run passes only when its failed set contains no name absent from the baseline failed set (`NEW-FAILURES: NONE`) and all sixteen target tests (NAMES-TARGETS) are `Passed`. AC22 additionally requires the runner route and exit 0.
- **D-9 Anchor.** Every diff gate uses `94287369908cc920b21b0e3256314f988ad7d2f5` as its ref operand (`BASE` in prose). P0-T3 verifies it is an ancestor of HEAD and equals `git merge-base origin/main HEAD`; a mismatch is `BASE-SHA MISMATCH` (ancestor check fails) or `BASE AHEAD OF BRANCH` (merge-base differs): stop and report, because `refs/remotes/origin/main` is shared by every worktree and a fetch elsewhere can move it. Paths changed between BASE and HEAD at P0-T3 form the inherited set `INHERITED-COMMITTED:`; each must be under FEATURE or be one of the two promoted records `docs/features/potential/promoted/2026-10-02-focus-and-theme-tests-leak-shared-dispatcher-setup.md` and `docs/features/potential/promoted/2026-10-02-qfc-datamodel-950-review-residuals.md` (fact 11), otherwise `INHERITED SET OUT OF SCOPE`: stop. Every footprint gate (P6-T9, P8-T9, P8-T46) excludes exactly the `INHERITED-COMMITTED:` paths, so it is consistent with AC20 as amended. Late in the run P8-T9 re-reads `git rev-parse origin/main`; a value other than BASE is recorded as `BASE REF MOVED` and does not stop the run, because every gate names BASE explicitly.
- **D-10 Commits.** Three commits: P0-T19 (FEATURE only), P6-T9 (the fourteen Write Set code files plus FEATURE, staged after the P6-T1 scoped format so the committed text is formatter-stable), P8-T46 (FEATURE). Each is a `git -C WORKTREE add -- <pathspecs>` invocation followed by a separate `git -C WORKTREE commit -m "<message>" -- <the same pathspecs>` invocation, one command per call, never chained, never `git add -A`, and always pathspec-limited on the commit as well as the add, so a path staged by anything else can never be swept into a commit. No commit message contains an angle bracket, a dollar sign or a backtick. A PreToolUse refusal of any `git add`, `git commit`, `.cs` edit, `.csproj` edit, spec edit, evidence-file Write or pwsh payload (including a refusal whose text begins `PREIMPLEMENTATION_GATE_BLOCKED`) is recorded verbatim as `PRE-IMPLEMENTATION GATE BLOCKED` and stops the run; the executor does not retry with a rephrased edit or another tool and does not modify hooks, checkpoints or permission configuration.
- **D-11 Git gates are pathspec-scoped and anchored.** Every `git diff` names BASE or HEAD as its ref operand; every name-listing diff is paired with a porcelain span in the same task; no gate asserts an unscoped empty porcelain. Uncommitted `.claude/agent-memory/` paths and this plan file may appear in porcelain output and are admitted by every scope gate; their count is deliberately unrecorded.
- **D-12 Check-offs follow the loop.** Every check-off task sits in Phase 8 after the final toolchain pass and reads an artifact that survived it. Each flips exactly one checkbox and, when its evidence does not hold, completes with the box unchecked and records `ACn: NOT MET` with the reason in `FEATURE/evidence/other/ac-status-summary.md`.
- **D-13 Restart rules.** Phase 6: if P6-T4, P6-T5, P6-T6, P6-T7 or P6-T8 shows a target test not `Passed`, the executor corrects the Write Set file at fault (within the delivered design; no prohibited construct) and restarts at P6-T1, recording `P6-RESTART: n` in the restarted artifacts (when a P6-T9 commit is already in HEAD because a Phase 8 restart preceded this one, the P6-T2 ref-operand and porcelain rule stated below applies to this restart as well). Phase 8: `ITERATION` starts at 1. If P8-T1 rewrites any Write Set file, the executor commits exactly the rewritten Write Set files (`style(968): apply csharpier output`, pathspec-limited), increments ITERATION and restarts at P8-T1. If P8-T2, P8-T3, P8-T4 or P8-T5 fails because of a Write Set file, the executor corrects it, restarts at P6-T1 (scoped format, census, build, pass-after runs, commit), re-runs Phase 7 in full, increments ITERATION and resumes at P8-T1. On that restart the P6-T9 commit is already in HEAD, so P6-T2 runs each of its HEAD-anchored git commands with 94287369908cc920b21b0e3256314f988ad7d2f5 as the ref operand instead (the numstat rows it gates and the FIELDLOCK-ENCLOSURE diff are read from those), and its porcelain expectation becomes: every porcelain line under QuickFiler/ or QuickFiler.Test/ names one of the fourteen Write Set code paths with status ` M`, recorded as `P6-RESTART-PORCELAIN:`. A rewrite of a file outside the Write Set, or a failure not attributable to the Write Set beyond the D-8 baseline rule and the P8-T5 `LEAK-DEPENDENT TEST EXPOSED` rule, is a stop with the failing artifact. P8-T7 records the final iteration only.
- **D-14 Line endings of new files.** The ten existing Write Set `.cs` files are CRLF in the worktree (fact header). P1-T1, P4-T2 and P5-T2 normalise each new file to CRLF with CMD-EOL after writing it, so `csharpier check .` and the committed blob (normalised to LF by `text=auto`) behave as for the siblings; the gate is `BARE_LF: 0`.
- **D-15 One shared synchronous worker (#972 item 1; addendum section 1.4).** New file `QuickFiler.Test/TestSupport/SynchronousBackgroundWorker.cs` declares `internal sealed class SynchronousBackgroundWorker : BackgroundWorker` in namespace `QuickFiler.Test.TestSupport` with `internal void RaiseDoWork()` and `internal static void StartSynchronously(BackgroundWorker worker)` (Delivered Source W1). It adds no fields, handles or subscriptions and does not override `Dispose(bool)`; disposal stays with the constructing test (D-17). The three nested classes and three private starters are deleted; each consumer adds `using QuickFiler.Test.TestSupport;` in alphabetical position (after `using Moq;`) and assigns `model.WorkerStarter = SynchronousBackgroundWorker.StartSynchronously;`. Doc rewording F5 (the ZeroBatch remarks line 33) and F6 (ZeroBatch lines 97 to 99) are applied; the Liveness and Teardown class headers describe the reflection helpers, not the worker, and are unchanged (addendum section 6.3).
- **D-16 Dead-code removal with a prior zero-caller proof (#972 item 3; spec decision 7).** P4-T1 re-runs the two search strategies of fact 15 against the pre-change tree with CMD-LEGACY-CALLERS, classifies every hit, compares the member sets and records the proof before any removal. P4-T8 then removes the four members, the commented-out references at pre-edit lines 99 to 101, 194, 209 to 210 and 363, and the empty region, and retargets the `nameof` at pre-edit line 369 to `LoadRemainingEmailsToQueueAsync` (F1 to F3). This is unreachable dead code: it has no behaviour to regress, so it takes no failing test; the compile proof is the two rebuilds (P8-T3, P8-T4), which fail on any surviving reference. Expected physical line count after the edit: 495 minus 128 = 367 (fact 14 and the removal breakdown under Delivered Source P1); the gate is at most 400 (AC28). `[ExcludeFromCodeCoverage]` is unchanged (AC29).
- **D-17 Caller-owned worker disposal (#972 item 4; addendum section 4).** Every `SynchronousBackgroundWorker` and every test-created `BackgroundWorker` in `QfcDatamodelLivenessTests.cs`, `QfcInitEmailQueueZeroBatchTests.cs` and `QfcDatamodelTests.cs` is constructed in the header of a `using (...) { }` block owned by the test method (never `using var`: `QuickFiler.Test.csproj` sets no `LangVersion`, and the block form is what `QfcDatamodelTeardownTests.cs` 180 and 220 already use). `StartHeldOpenLoader` takes the worker as its first parameter and no longer constructs one; its three callers own the worker. The datamodel never disposes `_worker` (addendum section 4.1), so test-side disposal cannot double-dispose, and `Component.Dispose()` is idempotent.
- **D-18 Deterministic liveness tests (#968 comment; addendum section 5.4, with the context scope scrutinised).** New file `QuickFiler.Test/TestSupport/ArmingFakeTimeProvider.cs` declares `internal sealed class ArmingFakeTimeProvider : FakeTimeProvider` with `internal Task Armed`, `internal void ReArm()` and a `CreateTimer` override that calls the base first and then `TrySetResult(true)` on a signal created with `TaskCreationOptions.RunContinuationsAsynchronously` (Delivered Source W2; precedents in fact 21). Both dequeue-liveness tests take the shape: arm-check (`clock.Armed.IsCompleted` is true before the dequeue call returns, because the gate reaches its first `Delay` synchronously), `ReArm()`, one `Advance(200)`, `Task first = await Task.WhenAny(clock.Armed, pending)`, `first.Should().BeSameAs(clock.Armed, ...)`, `pending.IsCompleted` false, then the flag transition, one more `Advance(200)` and `await pending` as the completion signal. The re-arm proof is context-independent because the gate's own await is `ConfigureAwait(false)` (fact 16): whether its continuation runs inline inside `Advance` or on the pool, `Armed` completes once the gate re-arms, and if the gate returned instead, `pending` wins the race and the assertion fails crisply rather than hanging. In the Liveness test the loader's completion must clear the flag before the final advance, so the datamodel calls and the `loaderRelease.SetResult(true)` are made inside a `NoSynchronizationContext()` scope: a nested `IDisposable` that captures `SynchronizationContext.Current`, sets it to null, and restores the captured value on dispose. The scope is sound because (i) `SetSynchronizationContext` is a per-thread setting, (ii) neither scope body contains an `await`, so the restore runs on the same thread that took the scope and before any continuation of the test method can move it to another thread (this is gated: the T1-LIVE span contains exactly two `using (NoSynchronizationContext())` lines and exactly three `await` lines, none of them inside a scope body), and (iii) an await registered under a null context with the default scheduler runs its continuation inline on the completing thread, which is what makes `ReadLivenessFlag(model)` read false before the final advance; if that TPL rule were ever violated the flag checkpoint fails crisply, which is the behaviour the addendum requires. The sibling test in `QfcDatamodelTests` writes the flag by reflection, so it needs no scope; its final `await pending` depends only on the ambient context being serviced, which is the same pre-existing dependence every other awaiting test in that file has. No `Thread.Sleep`, `Task.Delay`, retry, bounded loop, `Task.Yield`, timeout attribute (none exists on these tests, so none is added), `[DoNotParallelize]` or Workers change is introduced.
- **D-19 Fail-before exception and sensitivity check (spec decision 7; addendum section 5.5).** A deterministic failing run of the old test shape is structurally impossible: its failure requires the thread pool to delay a queued continuation past a bounded retry, which no test input can force, and the production behaviour is correct before and after. P5-T1 therefore writes the dossier `FEATURE/evidence/regression-testing/fail-before-exception.<timestamp>.md` with `WhyFailingRunImpossible:` and an alternative-proof section BEFORE the rewrite. P5-T8 then performs the labelled sensitivity check: the `sourceActive` lambda at `QfcDatamodel.QueueProcessing.cs` 305 is edited in the working copy from `() => _remainingLoadActive,` to `() => false,`, the tree is built, each rewritten test is run by fully qualified name and must fail on its re-arm assertion (`to refer to` plus its because fragment; not `Timeout`, not `Aborted`), the edit is reverted with the Edit tool, and the revert is proved by `git -C WORKTREE diff --exit-code 94287369908cc920b21b0e3256314f988ad7d2f5 -- QuickFiler/Controllers/QfcDatamodel.QueueProcessing.cs` exiting 0 (the file is at BASE content until P5-T11 edits its comments, which is why the comment edits come after the check). P5-T9 rebuilds the reverted tree and P5-T10 re-runs the pair green before any later gate. The sensitivity edit is never committed: P5-T8 is the only task that touches that line, and the P6-T9 commit is preceded by the P6-T2 census, which reads `() => _remainingLoadActive,` 1 and `() => false,` 0.
- **D-20 Comment-only production edit and the QuiesceLoaderAsync decision (#972 item 2).** The `_remainingLoadActive` doc (QueueProcessing 15 to 23) is replaced by Delivered Source Q1 and the `(:31-66)` range is dropped from line 285 (Q2); the declaration and every statement are unchanged, so the file stays 413 lines. The `QuiesceLoaderAsync` comment at line 52 (`the field is written on the worker thread`) is about `_remainingLoadTask`, which `Worker_DoWork` writes at `QfcDatamodel.cs` 218 on the thread that runs the handler; in production that is the `BackgroundWorker`'s pool thread (the production `WorkerStarter` calls `RunWorkerAsync()`), and `QuiesceLoaderAsync` races a live loader only in production. The comment is therefore accurate post-#950 and is left unchanged; P5-T12 records `QUIESCE-COMMENT-DECISION: UNCHANGED` with this reason. The token `written on the worker thread` consequently reads 1 after the change (line 52) while the AC26 token `written on the worker thread and read` reads 0.

## Risks (recorded; no mitigation is in scope)

- **AC22 under the DIRECT route.** If P0-T16 records `REPRODUCES`, the runner is not run and AC22 ends `NOT MET (ENVIRONMENTAL: COVERAGE-ROUTE DIRECT)`; the orchestrator decides, as for #950 AC17.
- **AC23 has no tolerance.** The repository-wide first-party rate varied by 0.01 percentage points between two #950 runs of an identical instrumented tree; a downward variation of that size fails AC23 as worded although no instrumented line changed. D-7 records the figures and the executor does not weaken the criterion.
- **CSharpier layout.** The P6-T1 scoped format may re-lay the delivered C# (chained assertions, the multi-line `if` in F-SCOPE, the `using (` headers, the long `because` strings). Its output wins. Every gate token below is a string literal, a single-line statement or a doc-comment line, none of which CSharpier splits or joins, so no gate depends on the layout.
- **Tests that relied on the leaked parked dispatcher.** The two deleted theme-test calls left the parked dispatcher installed for the rest of a run, and every later transaction restored it. Production QuickFiler code reads `UiThread.Dispatcher` in many places, so a test that depended on that leak now throws `The UI dispatcher has not been captured`. P8-T5 captures every non-passed message, records such a name as `LEAK-DEPENDENT TEST EXPOSED:` and stops for re-planning under the related-defect directive; it is neither a D-13 restart nor `NEW FAILURE OUTSIDE SCOPE`.
- **Timer mechanics of the rewritten liveness tests.** D-18 relies on `FakeTimeProvider.Advance` invoking due callbacks synchronously and on `TimeProvider.Delay` reaching the overridable `CreateTimer` (fact 21 precedents; addendum section 5.2 web-verified). If either assumption failed, P5-T7 would fail or hang under the 4-minute blame bound, and P5-T8 shows each test fails crisply rather than hangs when the liveness signal is wrong.

## Delivered source (the executor writes these texts; CSharpier output wins on any layout difference)

Every block below except N1, T1, W1 and W2 is shown at its in-file indentation (four, eight, twelve, sixteen or twenty leading spaces) and is written exactly as shown. N1, T1, W1 and W2 carry one extra four-space Markdown indent on every line, which the executor removes: the `using` and `namespace` lines of N1, W1 and W2 start in column 1, and T1 and the T2 lines start with four spaces. Prose-quoted gate tokens are each confined to one physical line of the delivered text. Every edit to an existing file is an in-place edit of the named lines (pre-edit numbering, facts 1 to 4 and 14 to 20); no existing file is rewritten whole.

**F-FIELDS — `QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixture.cs`, inserted after line 38** (`private static Dispatcher _parkedDispatcher = null;`): one blank line, then four lines:

        // Issue #968: the count of live ensure scopes and whether the fixture itself seeded the parked
        // dispatcher into a null field. Both are read and written only while FieldLock is held.
        private static int _pinCount;
        private static bool _fixtureInstalledParked;

**F-CLASSDOC — the same file, inserted after line 30** (`/// </para>`, the last line of the design-note paragraph) and before line 31 (`/// </summary>`), thirteen lines:

    /// <para>
    /// Issue #968: ensure pins are reference counted. A pin counter and an install-ownership flag
    /// live under <c>FieldLock</c>. The first pin on a <c>null</c> field seeds the parked dispatcher
    /// and sets the flag; the last release writes <c>null</c> back only when the flag is set and the
    /// field still holds the parked instance, then clears the flag. A discarded scope therefore
    /// pins for the process lifetime and leaves the parked dispatcher installed, so every caller
    /// disposes its scope, and every pin is acquired and released while its caller holds a
    /// transaction, which keeps the count at zero whenever a transaction is acquired. Residual: a
    /// transaction that installs over a pinned parked value and restores it after the last pin
    /// released leaves the parked value installed with zero pins and the flag set; the next pin
    /// cycle reverts it. No test in this assembly installs over a pinned value, so the residual is
    /// documented rather than exercised.
    /// </para>

Gate tokens quoted from F-CLASSDOC: install-ownership flag; pins for the process lifetime.

**F-ENSURE-DOC — the same file, replaces lines 116 to 121** (the `EnsureDispatcher` summary), nine lines replacing six:

        /// <summary>
        /// Takes one counted pin on the shared static (issue #968). The first pin on a <c>null</c>
        /// field seeds the parked dispatcher and records that the fixture owns the seeding; a pin
        /// taken while the field is non-null installs nothing. Disposing the returned scope releases
        /// the pin, and the field reverts to <c>null</c> only on the last release, only when the
        /// fixture owns the seeding, and only when the field still holds the parked instance. Never
        /// acquires <c>TransactionGate</c> and never blocks on anything a caller must release. A
        /// discarded scope pins for the process lifetime, so every caller disposes its scope.
        /// </summary>

**F-ENSURE-BODY — the same file, replaces lines 128 to 137** (from `lock (FieldLock)` through `return new EnsureScope(null);`; the comment 124 to 125, the `parked` local 126, the blank 127 and the method close 138 are unchanged), eleven lines replacing ten:

            lock (FieldLock)
            {
                _pinCount++;
                if (DispatcherField.GetValue(null) == null)
                {
                    DispatcherField.SetValue(null, parked);
                    _fixtureInstalledParked = true;
                }
            }

            return new EnsureScope(parked);

**F-SCOPE — the same file, replaces lines 243 to 274** (the `EnsureScope` documentation and class), forty-three lines replacing thirty-two:

        /// <summary>
        /// The scope returned by <see cref="EnsureDispatcher"/>: one counted pin. Disposal is
        /// idempotent and performs the decrement and the conditional revert inline in one
        /// <c>FieldLock</c> critical section, so no other pin can interleave between them. The revert
        /// writes <c>null</c> only when this release brings the count to zero, the fixture itself
        /// seeded the parked dispatcher, and the field still holds that instance; a value some other
        /// owner installed in the meantime is left in place.
        /// </summary>
        private sealed class EnsureScope : IDisposable
        {
            private readonly Dispatcher _parked;
            private bool _disposed = false;

            internal EnsureScope(Dispatcher parked)
            {
                _parked = parked;
                _disposed = false;
            }

            public void Dispose()
            {
                if (_disposed)
                {
                    return;
                }

                _disposed = true;

                lock (FieldLock)
                {
                    _pinCount--;
                    if (
                        _pinCount == 0
                        && _fixtureInstalledParked
                        && ReferenceEquals(DispatcherField.GetValue(null), _parked)
                    )
                    {
                        DispatcherField.SetValue(null, null);
                        _fixtureInstalledParked = false;
                    }
                }
            }
        }

After F-FIELDS to F-SCOPE the fixture is 375 lines before formatting; `_pinCount` occurs on exactly four lines (declaration, `_pinCount++;`, `_pinCount--;`, `_pinCount == 0`), `_fixtureInstalledParked` on exactly four (declaration, `_fixtureInstalledParked = true;`, `&& _fixtureInstalledParked`, `_fixtureInstalledParked = false;`), `lock (FieldLock)` on five (66, 79, 94, ENSURE, SCOPE; the F-FIELDS comment deliberately does not contain that literal), `CompareExchange(` on two (the helper's declaration and the transaction's call), `return new EnsureScope(` on one, and neither identifier appears in any comment.

**N1 — `QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherPinCountTests.cs`, new file, whole content:**

    using System;
    using System.Threading.Tasks;
    using System.Windows.Threading;
    using FluentAssertions;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    namespace QuickFiler.Controllers.Tests
    {
        /// <summary>
        /// Issue #968 tests for the reference-counted ensure pin of
        /// <see cref="UiThreadDispatcherFixture"/>. The regression lives at the fixture level because
        /// the theme tests the issue was filed against never read the shared static: their path
        /// dispatches through the theme's injected <c>IUiDispatcher</c> mock, so no value in the static
        /// can make a theme test fail, while the defect (the installing pin's release writing
        /// <c>null</c> while another pin is still live) is observable here on one thread with no
        /// concurrency.
        /// <para>
        /// Every test acquires a transaction first, so the pin count is zero and the baseline is known
        /// for the whole test, and carries the 60-second MSTest timeout of the sibling fixture test
        /// file. No sleep, delay, wall-clock wait, mock or temporary file is used; Moq is therefore
        /// not imported.
        /// </para>
        /// </summary>
        [TestClass]
        public class QfcItemController_UiThreadDispatcherPinCountTests
        {
            private const int GateTimeoutMs = 60000;

            /// <summary>
            /// Regression test: fails before the fix. With two pins held on a null baseline, releasing
            /// the first pin must leave the parked dispatcher in place. Before counting, the first pin
            /// was the installer and its release wrote <c>null</c> while the second pin was still live.
            /// </summary>
            [TestMethod]
            [Timeout(GateTimeoutMs)]
            public async Task EnsureDispatcher_TwoPinsHeld_ReleasingTheFirstKeepsTheDispatcherUntilTheLastRelease()
            {
                // Arrange
                UiThreadDispatcherTransaction transaction = await UiThreadDispatcherFixture
                    .BeginTransactionAsync()
                    .ConfigureAwait(false);
                try
                {
                    transaction.Install(null);
                    IDisposable pinA = QfcItemControllerTestSupport.EnsureUiThreadDispatcher();
                    IDisposable pinB = QfcItemControllerTestSupport.EnsureUiThreadDispatcher();
                    Dispatcher afterBothPins = UiThreadDispatcherFixture.Current;

                    // Act
                    pinA.Dispose();
                    Dispatcher afterFirstRelease = UiThreadDispatcherFixture.Current;
                    pinB.Dispose();
                    Dispatcher afterLastRelease = UiThreadDispatcherFixture.Current;

                    // Assert
                    afterBothPins
                        .Should()
                        .NotBeNull(
                            because: "the first pin seeds the parked dispatcher into a null field"
                        );
                    afterFirstRelease
                        .Should()
                        .BeSameAs(
                            afterBothPins,
                            because: "a holder that did not take the last pin must not lose the dispatcher"
                        );
                    afterLastRelease
                        .Should()
                        .BeNull(because: "the last release reverts the fixture's own seeding");
                }
                finally
                {
                    transaction.Dispose();
                }
            }

            /// <summary>
            /// Specification test: passes before and after the fix. Releasing the pins in the reverse
            /// order must produce the same outcome, so the count rather than the identity of the
            /// installing scope decides when the field reverts.
            /// </summary>
            [TestMethod]
            [Timeout(GateTimeoutMs)]
            public async Task EnsureDispatcher_TwoPinsHeld_ReleaseOrderDoesNotChangeTheOutcome()
            {
                // Arrange
                UiThreadDispatcherTransaction transaction = await UiThreadDispatcherFixture
                    .BeginTransactionAsync()
                    .ConfigureAwait(false);
                try
                {
                    transaction.Install(null);
                    IDisposable pinA = QfcItemControllerTestSupport.EnsureUiThreadDispatcher();
                    IDisposable pinB = QfcItemControllerTestSupport.EnsureUiThreadDispatcher();
                    Dispatcher afterBothPins = UiThreadDispatcherFixture.Current;

                    // Act
                    pinB.Dispose();
                    Dispatcher afterFirstRelease = UiThreadDispatcherFixture.Current;
                    pinA.Dispose();
                    Dispatcher afterLastRelease = UiThreadDispatcherFixture.Current;

                    // Assert
                    afterBothPins
                        .Should()
                        .NotBeNull(
                            because: "the first pin seeds the parked dispatcher into a null field"
                        );
                    afterFirstRelease
                        .Should()
                        .BeSameAs(
                            afterBothPins,
                            because: "a holder that did not take the last pin must not lose the dispatcher"
                        );
                    afterLastRelease
                        .Should()
                        .BeNull(because: "the last release reverts the fixture's own seeding");
                }
                finally
                {
                    transaction.Dispose();
                }
            }

            /// <summary>
            /// Specification test: passes before and after the fix; extends the existing fixture test
            /// EnsureDispatcher_WhileATransactionHoldsALiveDispatcher_DoesNotReplaceIt. While a
            /// transaction holds a live dispatcher, two pins install nothing, and releasing both must
            /// leave the live dispatcher in place: the count reaching zero writes nothing because the
            /// fixture did not seed the field. The live dispatcher is shut down in a finally block.
            /// </summary>
            [TestMethod]
            [Timeout(GateTimeoutMs)]
            public async Task EnsureDispatcher_UnderATransactionHoldingALiveDispatcher_ReleasingAllPinsLeavesTheLiveDispatcher()
            {
                // Arrange
                Dispatcher live = QfcItemControllerTestSupport.StartRunningDispatcher();
                try
                {
                    UiThreadDispatcherTransaction transaction = await UiThreadDispatcherFixture
                        .BeginTransactionAsync()
                        .ConfigureAwait(false);
                    try
                    {
                        transaction.Install(live);
                        IDisposable pinA = QfcItemControllerTestSupport.EnsureUiThreadDispatcher();
                        IDisposable pinB = QfcItemControllerTestSupport.EnsureUiThreadDispatcher();

                        // Act
                        pinA.Dispose();
                        pinB.Dispose();
                        Dispatcher afterAllReleased = UiThreadDispatcherFixture.Current;

                        // Assert
                        afterAllReleased
                            .Should()
                            .BeSameAs(
                                live,
                                because: "pins that installed nothing must not write over the transaction value "
                                    + "when the count reaches zero"
                            );
                    }
                    finally
                    {
                        transaction.Dispose();
                    }
                }
                finally
                {
                    QfcItemControllerTestSupport.ShutdownDispatcher(live);
                }
            }

            /// <summary>
            /// Specification test: passes before and after the fix. After a full two-pin cycle inside the
            /// same transaction, a fresh single pin on the null baseline must still seed the parked
            /// dispatcher and its release must still restore null. A second transaction then installs
            /// that parked instance as its own value, and a pin taken and released under it must leave
            /// the value in place: had the earlier cycle's last release left the install-ownership flag
            /// set, this release would revert a value the fixture did not seed.
            /// </summary>
            [TestMethod]
            [Timeout(GateTimeoutMs)]
            public async Task EnsureDispatcher_AfterAFullPinCycle_AFreshSinglePinStillInstallsAndRestores()
            {
                // Arrange
                Dispatcher parked;
                UiThreadDispatcherTransaction transaction = await UiThreadDispatcherFixture
                    .BeginTransactionAsync()
                    .ConfigureAwait(false);
                try
                {
                    transaction.Install(null);
                    IDisposable pinA = QfcItemControllerTestSupport.EnsureUiThreadDispatcher();
                    IDisposable pinB = QfcItemControllerTestSupport.EnsureUiThreadDispatcher();
                    parked = UiThreadDispatcherFixture.Current;
                    pinA.Dispose();
                    pinB.Dispose();

                    // Act
                    IDisposable freshPin = QfcItemControllerTestSupport.EnsureUiThreadDispatcher();
                    Dispatcher afterFreshPin = UiThreadDispatcherFixture.Current;
                    freshPin.Dispose();
                    Dispatcher afterFreshRelease = UiThreadDispatcherFixture.Current;

                    // Assert
                    afterFreshPin
                        .Should()
                        .NotBeNull(
                            because: "a pin on a null field seeds the parked dispatcher whatever earlier cycles did"
                        );
                    afterFreshRelease
                        .Should()
                        .BeNull(
                            because: "the fresh pin is the only live pin, so its release reverts the seeding"
                        );
                }
                finally
                {
                    transaction.Dispose();
                }

                // Act (second transaction): the parked instance is now a transaction value, not a seeding
                UiThreadDispatcherTransaction foreignTransaction = await UiThreadDispatcherFixture
                    .BeginTransactionAsync()
                    .ConfigureAwait(false);
                try
                {
                    foreignTransaction.Install(parked);
                    IDisposable foreignPin = QfcItemControllerTestSupport.EnsureUiThreadDispatcher();
                    foreignPin.Dispose();
                    Dispatcher afterForeignRelease = UiThreadDispatcherFixture.Current;

                    // Assert
                    afterForeignRelease
                        .Should()
                        .BeSameAs(
                            parked,
                            because: "the last release cleared the install-ownership flag, so a pin that seeded nothing leaves a transaction value in place"
                        );
                }
                finally
                {
                    foreignTransaction.Dispose();
                }
            }
        }
    }

N1 counts (each token on one physical line): `EnsureUiThreadDispatcher()` 10 (tests 1, 2 and 3 two each, test 4 four); bare `EnsureDispatcher` 15 lines (the 10 invocations, the 4 method names, the test 3 doc reference); `Regression test: fails before the fix` 1; `Specification test: passes before and after the fix` 3; `never read the shared static` 1; `[TestClass]` 1; `[TestMethod]` 4; `[Timeout(GateTimeoutMs)]` 4; `private const int GateTimeoutMs = 60000;` 1; `transaction.Install(null);` 3; `transaction.Install(live);` 1; `transaction.Dispose();` 4 (case-sensitive: `foreignTransaction.Dispose();` does not match because of the capital T); `QfcItemControllerTestSupport.ShutdownDispatcher(live);` 1; `a holder that did not take the last pin must not lose the dispatcher` 2; `the last release reverts the fixture` 2; `using Moq;` 0; `Thread.Sleep` 0; `Task.Delay` 0; `public class QfcItemController_UiThreadDispatcherPinCountTests` 1; `foreignTransaction.Install(parked);` 1; `.BeginTransactionAsync()` 5. Within each transaction, its `.Install(` line precedes every pin acquired under it, and every pin is disposed before that transaction's first `Dispose();`.

**T1 — `QuickFiler.Test/QuickFiler.Test.csproj`, one line inserted after line 203** (`    <Compile Include="Controllers\QfcItemController.UiThreadDispatcherFixtureTests.cs" />`), four leading spaces after the Markdown indent is removed:

        <Compile Include="Controllers\QfcItemController.UiThreadDispatcherPinCountTests.cs" />

**T2 — the same file, one line inserted after the line `    <Compile Include="TestSupport\DedicatedWorkerThread.cs" />`** (pre-edit line 228; line 229 once T1 is applied) by P4-T3, and a second line inserted immediately after that new line by P5-T2; four leading spaces each:

        <Compile Include="TestSupport\SynchronousBackgroundWorker.cs" />
        <Compile Include="TestSupport\ArmingFakeTimeProvider.cs" />

After T1 and both T2 lines the project file carries three more `<Compile Include=` lines than at BASE, and `TestSupport\WinFormsPumpHostTests.cs` immediately follows `TestSupport\ArmingFakeTimeProvider.cs`.

**A1 — `QuickFiler.Test/Controllers/QfcItemController.FocusAndThemeTests.cs`, delete lines 99 to 116** (the private `BuildExecutingViewer` method and the blank line that follows it; line 98 stays blank and line 117 `/// <summary>` becomes line 99). Eighteen lines removed.

**A2 — the same file, two edits, applied after A1 (so by content, not by pre-edit line number):** (i) every line whose trimmed text is `var viewer = BuildExecutingViewer();` (seven lines) becomes, at the same twelve-space indentation, `var viewer = QfcItemControllerTestSupport.BuildExecutingViewer();`; (ii) the six-line comment block beginning `// Cycle-3 P9-T5/P9-T6 (members #33/#35, de-exempted); cycle-4 remediation R1: the entire body` and ending `// state machine) and assert the resulting state transitions, not merely the Invoke marshal.` is replaced by these seven lines (eight-space indent):

        // Cycle-3 P9-T5/P9-T6 (members #33/#35, de-exempted); cycle-4 remediation R1: the entire body
        // runs inside a single _itemViewer.Invoke(...) delegate. The shared
        // QfcItemControllerTestSupport.BuildExecutingViewer() executes the delegate synchronously
        // (issue #968 removed this file's private copy) and EnableHandlelessThemeInvoke() populates
        // the terminal _themes[_activeTheme].SetQfcTheme(async: false) call's dependencies with
        // handle-less doubles, so these tests exercise the full method body (the _activeUI/_activeTheme
        // state machine) and assert the resulting state transitions, not merely the Invoke marshal.

**A3 — the same file, `SetThemeDark_FromNormal_SelectsDarkNormalTheme`:** the two comment lines beginning `// Arrange — async:true queues the theme application on the dispatcher without executing it,` and `// so no handle-less control is touched; the observable effect is the active-theme switch.` and the following line `QfcItemControllerTestSupport.EnsureUiThreadDispatcher();` (three lines, pre-A1 450 to 452) are replaced by these five lines (twelve-space indent):

            // Arrange — async:true queues the theme application through the theme's injected
            // IUiDispatcher mock (see BuildColorTheme), which absorbs the delegate without running it,
            // so no handle-less control is touched and the shared UiThread static is irrelevant to this
            // path (issue #968 deleted the former ensure call); the observable effect is the
            // active-theme switch.

**A4 — the same file, `SetThemeLight_FromNormal_SelectsLightNormalTheme`:** the line `// Arrange` and the following line `QfcItemControllerTestSupport.EnsureUiThreadDispatcher();` (pre-A1 467 to 468) are replaced by these two lines (twelve-space indent):

            // Arrange — same injected-mock arrangement as SetThemeDark_FromNormal_SelectsDarkNormalTheme:
            // the theme's IUiDispatcher mock absorbs the queued application (issue #968).

After A1 to A4 the file is 482 lines before formatting (497 minus 18 plus 1 plus 2). Gate tokens quoted from A2 to A4: QfcItemControllerTestSupport.BuildExecutingViewer(); absorbs the delegate without running it; shared UiThread static is irrelevant; absorbs the queued application.

**S1 — `QuickFiler.Test/Controllers/QfcItemController.TestSupport.cs`, replaces lines 216 to 237** (the `EnsureUiThreadDispatcher` documentation; the declaration 238 and body 239 are unchanged), twenty-four lines replacing twenty-two:

        /// <summary>
        /// Takes one reference-counted pin on the shared static <c>UiThread.Dispatcher</c> through
        /// <see cref="UiThreadDispatcherFixture.EnsureDispatcher"/> (issue #968): the first pin on a
        /// <c>null</c> field seeds a dedicated dispatcher hosted on a parked background thread that is
        /// never pumped, later pins install nothing, and the field reverts to <c>null</c> only when
        /// the last live pin releases and the fixture itself seeded it. The remaining legitimate
        /// callers are the fixture tests <c>QfcItemController_UiThreadDispatcherFixtureTests</c> and
        /// <c>QfcItemController_UiThreadDispatcherPinCountTests</c>, each of which acquires and
        /// releases its pin while holding a <see cref="UiThreadDispatcherTransaction"/>.
        /// <para>
        /// A dedicated (non-<c>CurrentDispatcher</c>) instance is used deliberately for test
        /// isolation: fire-and-forget <c>BeginInvoke</c>/<c>InvokeAsync</c> operations posted to the
        /// parked dispatcher are enqueued and never execute, so they cannot leak onto the test
        /// thread's own dispatcher and be run (and fault on a handle-less control) by an unrelated
        /// later test that pumps <c>Dispatcher.CurrentDispatcher</c>.
        /// </para>
        /// <para>
        /// Dispose the returned scope inside the same transaction that was held when it was taken: a
        /// discarded scope pins for the process lifetime, and a pin released outside its caller's
        /// transaction is released while another class may hold the gate. The implementation lives
        /// in <see cref="UiThreadDispatcherFixture"/>, the single owner of every mutation of that
        /// static made from this assembly's owned files.
        /// </para>
        /// </summary>

Gate tokens quoted from S1: remaining legitimate; QfcItemController_UiThreadDispatcherPinCountTests.

**S2 — the same file, replaces lines 282 to 288** (the `BuildExecutingViewer` documentation; applied after S1, at which point these lines are 284 to 290), seven lines replacing seven:

        /// <summary>
        /// Issue #480 shared arrange helper. Builds a viewer mock whose <c>Invoke</c> and
        /// <c>BeginInvoke</c> execute the supplied delegate synchronously, so a dispatch made through
        /// either path produces a countable call on whatever collaborator the delegate targets. Since
        /// issue #968 this is the single implementation; QfcItemController.FocusAndThemeTests.cs calls
        /// it instead of carrying a private copy.
        /// </summary>

After S1 and S2 the file is 442 lines before formatting.

**R-DOC — `QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixtureTests.cs`, replaces lines 196 to 208** (the R4 `<para>` ... `</para>`), thirteen lines replacing thirteen:

        /// <para>
        /// Issue #950 traced the earlier intermittent failure to a race on the shared static, not to
        /// a timing defect: a gate-free ensure call in another class could seed the parked dispatcher
        /// between the baseline read and the install, or between the restore and the second caller's
        /// read, and the test pinned a non-null baseline inside the gate to fence it. Issue #968
        /// removed that pin: the fixture now counts pins, so only the last release can revert the
        /// fixture's own seeding, and every remaining ensure call is acquired and released while its
        /// caller holds a transaction, so no class can write the field while transaction A holds the
        /// gate and every other transaction restores before it releases (W3/W4). Invariant for future
        /// editors: a pin must stay nested inside its caller's transaction, and UiThread.Initialize
        /// (W5) must not latch during this test; either would change the value the second caller
        /// observes.
        /// </para>

Gate tokens quoted from R-DOC: removed that pin: the fixture now counts pins; (W5) must not latch.

**R-BODY — the same file, R4 body, two edits against the pre-edit numbering (R-DOC replaces thirteen lines with thirteen, so the numbering below holds whether R-DOC is applied first or second; the plan applies R-DOC first):**

1. Replace lines 221 to 224 (the four-line `using (` header `using (` / `IDisposable baseline = QfcItemControllerTestSupport.EnsureUiThreadDispatcher()` / `)` / `{`) with these two lines (sixteen-space indent):

                try
                {

2. Replace line 270 (the sixteen-space `}` that closed the `baseline` using block, the line immediately after the `}` closing the `secondCallerStarted` using block and immediately before the twelve-space `}` closing the outer `try`) with these five lines:

                }
                finally
                {
                    transactionA.Dispose();
                }

Lines 225 to 269 (the `original` read, `transactionA.Install(liveA);`, the waiter, the explicit `transactionA.Dispose();` at 251, both assertions) are unchanged in text and indentation, because the `try` block body sits at the same twenty-space indentation the `using` block body had. The file is 472 lines before formatting (470 minus 4 plus 2 minus 1 plus 5). Within R4 after the edit: `EnsureUiThreadDispatcher()` 0, `using (` 1, `transactionA.Dispose();` 2, `finally` 3 (the waiter's, the new one, the `ShutdownDispatcher` one), `.BeSameAs(` 1, `.NotBeSameAs(` 1, `issue #230 lost update` 1.

**W1 — `QuickFiler.Test/TestSupport/SynchronousBackgroundWorker.cs`, new file, whole content (the four-space Markdown indent is removed):**

    using System.ComponentModel;

    namespace QuickFiler.Test.TestSupport
    {
        /// <summary>
        /// Test-side worker whose <see cref="RaiseDoWork"/> raises <c>DoWork</c> synchronously on the
        /// calling thread through the protected <c>OnDoWork</c>, so a privately subscribed handler such
        /// as <c>QfcDatamodel.Worker_DoWork</c> runs to its first incomplete await before
        /// <c>InitEmailQueue</c> returns, and no worker a test starts outlives that test. Issue #950
        /// introduced this shape to replace bounded waits on a thread-pool worker; issue #968 (folding
        /// issue #972) consolidated the three per-file copies here. The class adds no fields, handles
        /// or subscriptions, so it does not override <c>Dispose(bool)</c>; disposal stays with the test
        /// that constructs the worker, in a using block.
        /// </summary>
        internal sealed class SynchronousBackgroundWorker : BackgroundWorker
        {
            /// <summary>Raises <c>DoWork</c> on the calling thread.</summary>
            internal void RaiseDoWork() => OnDoWork(new DoWorkEventArgs(null));

            /// <summary>
            /// The synchronous starter assigned to <c>QfcDatamodel.WorkerStarter</c>. The worker handed
            /// to it must be a <see cref="SynchronousBackgroundWorker"/>.
            /// </summary>
            internal static void StartSynchronously(BackgroundWorker worker) =>
                ((SynchronousBackgroundWorker)worker).RaiseDoWork();
        }
    }

W1 counts: `class SynchronousBackgroundWorker` 1; `internal sealed class SynchronousBackgroundWorker : BackgroundWorker` 1; `internal static void StartSynchronously(BackgroundWorker worker)` 1; `Dispose` 1 (the doc line only); about 27 lines (the exact `LINES:` value is recorded by CMD-EOL and gated at most 500 and at least 20).

**W2 — `QuickFiler.Test/TestSupport/ArmingFakeTimeProvider.cs`, new file, whole content (the four-space Markdown indent is removed):**

    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Time.Testing;

    namespace QuickFiler.Test.TestSupport
    {
        /// <summary>
        /// A <see cref="FakeTimeProvider"/> that completes a signal after every <see cref="CreateTimer"/>
        /// call, so a test can prove that a production loop armed its next wait instead of returning,
        /// without a clock advance followed by a yield or a bounded retry.
        /// </summary>
        /// <remarks>
        /// Issue #968. <see cref="Armed"/> completes once the first timer after construction, or after
        /// the last <see cref="ReArm"/>, has been created; <c>TrySetResult</c> is used because a loop
        /// can arm one more timer than a test drives. Signals run their continuations asynchronously
        /// so a test never resumes inside the production <c>CreateTimer</c> call. Consecutive
        /// <c>Advance</c> calls without awaiting <see cref="Armed"/> in between are prohibited: a
        /// deadline the loop has not yet created is not advanced past, and the test would then wait on
        /// a timer that never fires. Modelled on UtilitiesCS.Test ArmingBarrierTimeProvider, as a
        /// subclass rather than a forwarding decorator because this project already subclasses
        /// <see cref="FakeTimeProvider"/>.
        /// </remarks>
        internal sealed class ArmingFakeTimeProvider : FakeTimeProvider
        {
            private volatile TaskCompletionSource<bool> _armed = NewSignal();

            /// <summary>Completes after the next <see cref="CreateTimer"/> call since the last re-arm.</summary>
            internal Task Armed => _armed.Task;

            /// <summary>Replaces the signal so the next <see cref="CreateTimer"/> call completes a fresh task.</summary>
            internal void ReArm() => _armed = NewSignal();

            public override ITimer CreateTimer(
                TimerCallback callback,
                object state,
                TimeSpan dueTime,
                TimeSpan period
            )
            {
                ITimer timer = base.CreateTimer(callback, state, dueTime, period);
                _armed.TrySetResult(true);
                return timer;
            }

            private static TaskCompletionSource<bool> NewSignal() =>
                new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        }
    }

W2 counts: `internal sealed class ArmingFakeTimeProvider : FakeTimeProvider` 1; `internal Task Armed` 1; `internal void ReArm()` 1; `public override ITimer CreateTimer(` 1; `base.CreateTimer(` 1; `RunContinuationsAsynchronously` 1; `_armed.TrySetResult(true);` 1; about 49 lines (the exact `LINES:` value is recorded by CMD-EOL and gated at most 500 and at least 30).

**L1 — `QuickFiler.Test/Controllers/QfcDatamodelLivenessTests.cs`, usings, in two halves:** L1a (P4-T6) inserts `using QuickFiler.Test.TestSupport;` immediately after `using Moq;` (pre-edit line 12). L1b (P5-T3, together with L3) deletes `using Microsoft.Extensions.Time.Testing;` (pre-edit line 9), because after L3 the file no longer names the `FakeTimeProvider` type (its two `ArmingFakeTimeProvider` lines resolve through `QuickFiler.Test.TestSupport`); deleting it earlier would break the Phase 4 build while test 1 still constructs one. Net zero lines; L1a is applied first, so every later L edit is located by content.

**L2 — the same file, delete pre-edit lines 47 to 61** (the nested worker doc, class, blank, starter doc, starter and the following blank; located by content as the block from the `/// <summary>` whose next line begins `/// Test-side worker whose` through the blank line after `((SynchronousBackgroundWorker)worker).RaiseDoWork();`). Fifteen lines removed; the `/// <summary>` of `DrainableSynchronizationContext` now follows the blank line after `SetPrivateField`.

**L3 — the same file, replace test 1 whole** (pre-edit lines 100 to 164: from the `/// <summary>` whose next line begins `/// Issue #424 regression test for the latent producer-liveness defect.` through the method's closing `}` that precedes the blank line before `/// <summary>Reads the issue #424 producer-liveness flag by reflection.</summary>`) with L-T1 (eight-space indent on the doc and attribute lines):

        /// <summary>
        /// Issue #424 regression test for the latent producer-liveness defect. <c>Worker_DoWork</c> is
        /// <c>async void</c>, so it returns at its first yielding await and
        /// <see cref="BackgroundWorker.IsBusy"/> goes false while
        /// <c>LoadRemainingEmailsToQueueAsync</c> is still producing. The dequeue gate's
        /// <c>sourceActive</c> signal consumed that dishonest value, so an empty queue was mistaken
        /// for an exhausted one and the gate returned an early partial batch. The datamodel-owned
        /// <c>volatile bool</c> flag makes the signal truthful.
        /// <para>
        /// Issue #968: every step waits on an explicit signal instead of a clock advance followed by
        /// a scheduler yield. <see cref="ArmingFakeTimeProvider.Armed"/> proves the gate armed its
        /// next wait; the dequeue task itself is the completion signal; the production awaits are
        /// registered with no synchronization context installed, so the loader's completion clears
        /// the flag inline and is read back before the final advance. No retry loop remains.
        /// </para>
        /// </summary>
        [TestMethod]
        public async Task DequeueNextItemGroupAsync_WhileLoaderStillProducing_KeepsPollingAfterWorkerIdle()
        {
            // Arrange
            var model = CreateUninitializedDatamodel();
            var clock = new ArmingFakeTimeProvider();
            model.TimeProvider = clock;
            SetPrivateField(model, "_globals", CreateHighConfidenceGlobals());
            SetPrivateField(model, "_masterQueue", new LockingLinkedList<MailItem>());

            var loaderEntered = new TaskCompletionSource<bool>();
            var loaderRelease = new TaskCompletionSource<bool>();
            model.RemainingEmailLoader = async _ =>
            {
                loaderEntered.TrySetResult(true);
                return await loaderRelease.Task;
            };

            using (var worker = new SynchronousBackgroundWorker())
            {
                model.WorkerStarter = SynchronousBackgroundWorker.StartSynchronously;
                Task<IList<MailItem>> pending;
                using (NoSynchronizationContext())
                {
                    // The issue #244 zero-batch short-circuit is COM-free and starts the worker
                    // through the issue #950 seam, which raises DoWork on this thread.
                    model.InitEmailQueue(0, worker);
                    loaderEntered
                        .Task.IsCompleted.Should()
                        .BeTrue("the synchronous starter must reach the injected RemainingEmailLoader");
                    pending = model.DequeueNextItemGroupAsync(1, 200);
                }

                clock
                    .Armed.IsCompleted.Should()
                    .BeTrue("the gate arms its first empty-queue wait before the dequeue call returns");
                clock.ReArm();

                // Act — the first wait expires while the loader is still producing.
                clock.Advance(TimeSpan.FromMilliseconds(200));
                Task first = await Task.WhenAny(clock.Armed, pending);

                // Assert
                first
                    .Should()
                    .BeSameAs(
                        clock.Armed,
                        "the loader is still producing, so the gate must arm a second wait rather than "
                            + "treat an empty queue as an exhausted source and return an early partial batch"
                    );
                pending.IsCompleted.Should().BeFalse("the gate re-armed instead of returning");

                // Cleanup — complete the loader; with no captured context its continuations run
                // inline and clear the flag before this call returns.
                using (NoSynchronizationContext())
                {
                    loaderRelease.SetResult(true);
                }

                ReadLivenessFlag(model)
                    .Should()
                    .BeFalse("the loader's completion must clear the flag before the next poll");
                clock.Advance(TimeSpan.FromMilliseconds(200));
                (await pending)
                    .Should()
                    .BeEmpty("once the loader completes, the gate exits on genuine exhaustion");
            }
        }

L-T1 is about 84 lines replacing 65. Gate tokens quoted from L-T1 (each on one line): `new ArmingFakeTimeProvider()`; `using (var worker = new SynchronousBackgroundWorker())`; `SynchronousBackgroundWorker.StartSynchronously`; `using (NoSynchronizationContext())` (2 lines); `clock.ReArm();`; `await Task.WhenAny(clock.Armed, pending)`; `the gate must arm a second wait`; `the gate re-armed instead of returning`; `must clear the flag before the next poll`; `(await pending)`; `await` on exactly three lines (the lambda's `return await loaderRelease.Task;`, the `WhenAny` line, the `(await pending)` line), none inside a `NoSynchronizationContext` block; `Task.Yield` 0; `fake.Advance` 0; `for (int i` 0.

**L4 — the same file, insert the context scope after `ReadLivenessFlag`** (after the method's closing `}` that follows `return (bool)field.GetValue(model);`): one blank line, then L-SCOPE (eight-space indent):

        /// <summary>
        /// Issue #968. Clears <see cref="SynchronizationContext.Current"/> on the calling thread for the
        /// lifetime of the returned scope and restores the previous value on dispose, so every
        /// production await registered inside the scope captures no context and its continuation runs
        /// inline on the completing thread. The scope body must contain no <c>await</c>: the restore
        /// has to run on the same thread that took the scope.
        /// </summary>
        private static IDisposable NoSynchronizationContext() => new SynchronizationContextScope();

        private sealed class SynchronizationContextScope : IDisposable
        {
            private readonly SynchronizationContext _previous = SynchronizationContext.Current;

            internal SynchronizationContextScope() =>
                SynchronizationContext.SetSynchronizationContext(null);

            public void Dispose() => SynchronizationContext.SetSynchronizationContext(_previous);
        }

L-SCOPE is about 19 lines including the leading blank. The field initializer runs before the constructor body, so `_previous` captures the context the scope then clears.

**L5 — the same file, `StartHeldOpenLoader`:** replace its doc and declaration (pre-edit lines 174 to 186, from the `/// <summary>` whose next line begins `/// Starts the worker with a` through the line `)` that closes the parameter list) with L-HELD-HEAD, and delete the two body lines `var worker = new SynchronousBackgroundWorker();` and `model.WorkerStarter = StartSynchronously;` (pre-edit 201 to 202), inserting in their place the single line `model.WorkerStarter = SynchronousBackgroundWorker.StartSynchronously;` (twelve-space indent). L-HELD-HEAD (eight-space indent), about fifteen lines replacing thirteen:

        /// <summary>
        /// Starts <paramref name="worker"/> with a <c>RemainingEmailLoader</c> held open by
        /// <paramref name="release"/>. The issue #950 synchronous starter raises <c>DoWork</c> on
        /// this thread, so by the time <c>InitEmailQueue</c> returns the async void
        /// <c>Worker_DoWork</c> has entered the loader and returned at its first incomplete await.
        /// <paramref name="release"/> runs its continuations asynchronously, so a test that has
        /// installed <c>DrainableSynchronizationContext</c> observes the resumed loader only
        /// through <c>Drain</c>, never inline inside <c>SetResult</c>. The caller owns and disposes
        /// the worker (issue #968, folding issue #972 item 4).
        /// </summary>
        private static QfcDatamodel StartHeldOpenLoader(
            SynchronousBackgroundWorker worker,
            Func<TaskCompletionSource<bool>, Task<bool>> loaderBody,
            out TaskCompletionSource<bool> release
        )

After L5 the helper body no longer contains `new SynchronousBackgroundWorker()`.

**L6 — the same file, test 2 body:** replace the body of `RemainingLoadActive_AcrossAsyncVoidFirstAwait_StaysTrueWhileLoaderProduces` (pre-edit lines 219 to 234, the `{` through the closing `}`) with L-T2 (eight-space indent on the braces):

        {
            using (var worker = new SynchronousBackgroundWorker())
            {
                // Arrange / Act
                QfcDatamodel model = StartHeldOpenLoader(
                    worker,
                    signal => signal.Task,
                    out TaskCompletionSource<bool> release
                );

                // Assert
                ReadLivenessFlag(model)
                    .Should()
                    .BeTrue(
                        "the producer is still live even though the async void handler already returned"
                    );

                release.SetResult(true);
            }
        }

**L7 — the same file, tests 3 and 4** (`RemainingLoadActive_AfterLoaderCompletes_BecomesFalse` and `RemainingLoadActive_WhenLoaderThrows_IsStillClearedByFinally`), the same mechanical edit in each: inside the `try` block, insert the two lines `using (var worker = new SynchronousBackgroundWorker())` and `{` (sixteen-space indent) immediately before the line `QfcDatamodel model = StartHeldOpenLoader(`; insert the line `worker,` (twenty-space indent) immediately after that `StartHeldOpenLoader(` line; insert a closing `}` (sixteen-space indent) immediately before the twelve-space `}` that closes the `try`; re-indent the enclosed lines by four spaces (CSharpier normalises indentation in P6-T1, so the gate is on tokens, not on indentation). Each test gains three lines.

After L1 to L7 the file is about 346 lines before formatting (L2 minus 15; L3 about plus 19; L4 about plus 19; L5 about plus 1 net; L6 plus 4; L7 plus 6); the exact value is recorded by CMD-LINECOUNT and gated at most 500 (fold line counts are observations, because CSharpier may re-wrap the long `because` strings). Post-change counts: `class SynchronousBackgroundWorker` 0; `StartSynchronously` 2 lines, both `SynchronousBackgroundWorker.StartSynchronously` (L-T1 and the helper body); `new SynchronousBackgroundWorker()` 4 lines, each inside `using (var worker = new SynchronousBackgroundWorker())`; `StartHeldOpenLoader(` 4 lines; `worker,` 4 lines (the three callers and the `StartHeldOpenLoader` parameter line `SynchronousBackgroundWorker worker,`); `Task.Yield` 0; `fake.Advance` 0; `FakeTimeProvider` 2 (`new ArmingFakeTimeProvider()` and the L-T1 doc cref `ArmingFakeTimeProvider.Armed`; the count is an ordinal substring match); `using QuickFiler.Test.TestSupport;` 1; `NoSynchronizationContext` 3 lines (declaration plus two uses); `Duplicated per file` 0.

**TD1 — `QuickFiler.Test/Controllers/QfcDatamodelTeardownTests.cs`:** insert `using QuickFiler.Test.TestSupport;` immediately after `using Moq;` (pre-edit line 12); delete pre-edit lines 59 to 74 (the nested worker doc, class, blank, starter doc, starter and the following blank; located by content from the `/// <summary>` whose next line begins `/// Test-side worker whose` through the blank line after `((SynchronousBackgroundWorker)worker).RaiseDoWork();`); replace the line `model.WorkerStarter = StartSynchronously;` (pre-edit 222) with `model.WorkerStarter = SynchronousBackgroundWorker.StartSynchronously;` at the same sixteen-space indent. The file is 244 plus 1 minus 16 = 229 lines before formatting. Post-change counts: `class SynchronousBackgroundWorker` 0; `StartSynchronously` 1 (`SynchronousBackgroundWorker.StartSynchronously`); `using (var worker = new SynchronousBackgroundWorker())` 1 (unchanged, line 220 pre-edit); `Duplicated per file` 0; `using QuickFiler.Test.TestSupport;` 1. The class header (lines 18 to 27) is unchanged: it documents the reflection helpers, not the worker.

**Z1 — `QuickFiler.Test/Controllers/QfcInitEmailQueueZeroBatchTests.cs`, usings and docs:** insert `using QuickFiler.Test.TestSupport;` immediately after `using Moq;` (pre-edit line 12). Replace pre-edit lines 32 to 34 (the three remarks lines beginning `/// assigns the <c>WorkerStarter</c> seam`, `/// the test thread through the nested`, `/// thread-pool worker and none outlives the test.`) with these three lines (four-space indent):

    /// assigns the <c>WorkerStarter</c> seam a starter that raises <c>DoWork</c> synchronously on
    /// the test thread through the shared <c>SynchronousBackgroundWorker</c> test-support helper
    /// (issue #968 consolidated the per-file copies), so no started worker outlives its test.

Replace pre-edit lines 97 to 99 (`/// Assigning this delegate before starting a real <see cref="BackgroundWorker"/> is what makes`, `/// it safe to call <see cref="QfcDatamodel.InitEmailQueue(int, BackgroundWorker)"/> with a real`, `/// worker in a unit test.`) with these three lines (eight-space indent):

        /// Assigning this delegate before the synchronous test worker is started is what makes it
        /// safe to call <see cref="QfcDatamodel.InitEmailQueue(int, BackgroundWorker)"/> in a unit
        /// test (issue #950: the worker raises DoWork on the test thread).

**Z2 — the same file, delete pre-edit lines 114 to 128** (the nested worker doc, class, blank, starter doc, starter and the following blank; located by content as for TD1). Fifteen lines removed.

**Z3 — the same file, the three tests (located by method name; every `model.WorkerStarter = StartSynchronously;` becomes `model.WorkerStarter = SynchronousBackgroundWorker.StartSynchronously;`):**

1. `InitEmailQueue_ZeroBatchSize_ReturnsEmptyListWithoutThrowing`: replace the lines from `// Act` through `result.Should().BeEmpty();` (pre-edit 146 to 153) with Z-T1 (twelve-space indent on the `using`):

            using (var worker = new SynchronousBackgroundWorker())
            {
                // Act
                System.Action act = () => result = model.InitEmailQueue(0, worker);

                // Assert
                act.Should().NotThrow();
                result.Should().NotBeNull();
                result.Should().BeEmpty();
            }

2. `InitEmailQueue_ZeroBatchSize_StillStartsBackgroundWorker`: replace the lines from `var worker = new SynchronousBackgroundWorker();` through the `.BeTrue("the injected RemainingEmailLoader must be invoked by the started worker");` line (pre-edit 173 to 182) with Z-T2:

            using (var worker = new SynchronousBackgroundWorker())
            {
                // Act
                model.InitEmailQueue(0, worker);

                // Assert
                worker.WorkerSupportsCancellation.Should().BeTrue();
                loaderInvokedTcs
                    .Task.IsCompleted.Should()
                    .BeTrue("the injected RemainingEmailLoader must be invoked by the started worker");
            }

3. `InitEmailQueue_PositiveBatchSize_RetainsExistingProjectionAndFrameDrop`: replace the lines from `// Act` through `frame.RowCount.Should().Be(0);` (pre-edit 220 to 229) with Z-T3:

            using (var worker = new SynchronousBackgroundWorker())
            {
                // Act
                var result = model.InitEmailQueue(2, worker);

                // Assert
                result.Should().HaveCount(2);
                result.Should().BeEquivalentTo(mailItemsByEntryId.Values);

                var frameField = typeof(QfcDatamodel).GetField("_frame", NonPublicInstance);
                var frame = (Frame<int, string>)frameField.GetValue(model);
                frame.RowCount.Should().Be(0);
            }

After Z1 to Z3 the file is about 225 lines before formatting (recorded, gated at most 500). Post-change counts: `class SynchronousBackgroundWorker` 0; `StartSynchronously` 3 lines, each `SynchronousBackgroundWorker.StartSynchronously`; `new SynchronousBackgroundWorker()` 3 lines, each `using (var worker = new SynchronousBackgroundWorker())`; `InitEmailQueue(0, new` 0; `InitEmailQueue(2, new` 0; `Duplicated per file` 0; `through the nested` 0; `starting a real` 0; `using QuickFiler.Test.TestSupport;` 1; `[TestMethod]` 3.

**M1 — `QuickFiler.Test/Controllers/QfcDatamodelTests.cs`, usings:** insert `using QuickFiler.Test.TestSupport;` immediately after `using Moq;` (pre-edit line 12). `using Microsoft.Extensions.Time.Testing;` stays (two other tests construct `FakeTimeProvider`).

**M2 — the same file, replace the sibling test whole** (pre-edit lines 95 to 131: from the `[TestMethod]` immediately above `public async Task DequeueNextItemGroupAsync_HighConfidenceMode_WaitsWhileSourceWorkerActive()` through its closing `}`) with M-T (eight-space indent on the doc and attribute lines):

        /// <summary>
        /// Issue #424: the high-confidence dequeue keeps polling while the datamodel-owned liveness
        /// flag is true. Issue #968: the re-arm is proved through
        /// <see cref="ArmingFakeTimeProvider.Armed"/> instead of a clock advance followed by
        /// a scheduler yield, and the dequeue task itself is the completion signal.
        /// </summary>
        [TestMethod]
        public async Task DequeueNextItemGroupAsync_HighConfidenceMode_WaitsWhileSourceWorkerActive()
        {
            // Arrange
            var model = CreateUninitializedDatamodel();
            var clock = new ArmingFakeTimeProvider();
            model.TimeProvider = clock;

            var settings = new Mock<IAppQuickFilerSettings>(MockBehavior.Strict);
            settings.SetupGet(x => x.HighConfidenceModeEnabled).Returns(true);
            settings.SetupGet(x => x.HighConfidenceThreshold).Returns(0.90);
            var globals = new Mock<IApplicationGlobals>(MockBehavior.Strict);
            globals.SetupGet(x => x.QfSettings).Returns(settings.Object);

            using (var worker = new BackgroundWorker())
            {
                SetPrivateField(model, "_globals", globals.Object);
                SetPrivateField(model, "_worker", worker);
                SetPrivateField(model, "_masterQueue", new LockingLinkedList<MailItem>());
                // Issue #424: the source-active signal is the datamodel-owned liveness flag, not
                // BackgroundWorker.isRunning, which is dishonest for an async void DoWork handler.
                SetPrivateField(model, "_remainingLoadActive", true);

                Task<IList<MailItem>> pending = model.DequeueNextItemGroupAsync(1, 200);
                clock
                    .Armed.IsCompleted.Should()
                    .BeTrue("the gate arms its first empty-queue wait before the dequeue call returns");
                clock.ReArm();

                // Act — the first wait expires while the source is still active.
                clock.Advance(TimeSpan.FromMilliseconds(200));
                Task first = await Task.WhenAny(clock.Armed, pending);

                // Assert
                first
                    .Should()
                    .BeSameAs(
                        clock.Armed,
                        "the datamodel source-active signal must keep polling while the worker can still add candidates"
                    );
                pending.IsCompleted.Should().BeFalse("the gate re-armed instead of returning");

                SetPrivateField(model, "_remainingLoadActive", false);
                clock.Advance(TimeSpan.FromMilliseconds(200));
                IList<MailItem> result = await pending;

                result.Should().BeEmpty();
            }
        }

**M3 — the same file, `WaitForQueue_WhenWorkerBusyAndQueueShort_AwaitsInjectedTwoHundredMsDelay`:** replace the line `var worker = new BackgroundWorker();` (pre-edit 261) with the two lines `using (var worker = new BackgroundWorker())` and `{` (twelve-space indent), insert a closing `}` (twelve-space indent) immediately after `task.IsCompleted.Should().BeTrue();` (pre-edit 282), and re-indent the enclosed lines by four spaces.

After M1 to M3 the file is about 393 lines before formatting (recorded, gated at most 500). Post-change counts: `new BackgroundWorker()` 2 lines, each `using (var worker = new BackgroundWorker())`; `new ArmingFakeTimeProvider()` 1; `clock.ReArm();` 1; `await Task.WhenAny(clock.Armed, pending)` 1; `must keep polling while the worker can still add candidates` 1; `the gate re-armed instead of returning` 1; `Task.Yield` 0; `await Task.Yield();` 0; `fake.Advance` 2 (the two untouched tests at pre-edit 241 and 280); `FakeTimeProvider` 6 (the four untouched lines at pre-edit 216, 224, 249 and 258 plus `new ArmingFakeTimeProvider()` and the M-T doc cref `ArmingFakeTimeProvider.Armed`; the count is an ordinal substring match, and the `using Microsoft.Extensions.Time.Testing;` directive does not contain it); `using QuickFiler.Test.TestSupport;` 1; `[TestMethod]` 9.

**P1 — `QuickFiler/Controllers/QfcDatamodel.cs`, deletions and one retarget (pre-edit numbering of fact 14; apply from the bottom of the file upward so earlier numbers stay valid, or locate each block by its quoted first and last lines):**

1. Delete lines 469 to 473 (`#region Linked List Locking`, two blanks, `#endregion Linked List Locking`, the following blank). Five lines.
2. Delete lines 417 to 465 (the blank before `private async Task<bool> LoadRemainingEmailsToQueueAsync(` with the `BackgroundWorker bw,` parameter, through that method's closing `}`; the block carries both `#pragma` lines). Forty-nine lines.
3. Delete lines 377 to 416 (the blank before `private bool LoadRemainingEmailsToQueue(BackgroundWorker bw, CancellationToken token)` through that method's closing `}`). Forty lines.
4. Replace line 369 `$"{nameof(LoadRemainingEmailsToQueue)} Error. \n {e.Message}\n{e.StackTrace}"` with `$"{nameof(LoadRemainingEmailsToQueueAsync)} Error. \n {e.Message}\n{e.StackTrace}"` (same twenty-four-space indent). Delete line 363 (`//logger.Debug($"{nameof(LoadRemainingEmailsToQueue)} Task cancelled");`). One line.
5. Delete lines 242 to 265 (the blank before `// This event handler demonstrates how to interpret`, the three comment lines, and `private void Worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)` through its closing `}`). Twenty-four lines.
6. Delete lines 209 to 210 (`//e.Result = await LoadRemainingEmailsToQueueAsync(bw, _token);`, `//e.Result = LoadRemainingEmailsToQueue(bw, _token);`). Two lines.
7. Delete line 194 (`//worker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(Worker_RunWorkerCompleted);`). One line.
8. Delete lines 109 to 111 (`private static readonly log4net.ILog log = log4net.LogManager.GetLogger(`, its argument line, `);`). Three lines.
9. Delete lines 99 to 101 (`//_blockingQueue = null;`, `//_priorityQueue = null;`, `//_queues = null;`; fields that no longer exist, addendum F2). Three lines.

Total removed 128; the file is 367 lines before formatting (495 minus 128), gated at most 400. No `using` directive becomes unused through this edit (addendum section 3.3: `Enumerable`, `Task`, `MessageBox`, `BackgroundWorker` and `log4net` remain referenced). Post-change counts (tokens never contain a double quote, because CMD-TOKEN-COUNT passes them as double-quoted PowerShell strings): `Worker_RunWorkerCompleted` 0; `nameof(LoadRemainingEmailsToQueue)` 0; `nameof(LoadRemainingEmailsToQueueAsync)} Error.` 1 (0 before); `nameof(LoadRemainingEmailsToQueueAsync)} Task cancelled` 0 (1 before); `LoadRemainingEmailsToQueueAsync(` 2 (the cref at pre-edit 130 and the one-argument declaration); `LoadRemainingEmailsToQueue(BackgroundWorker bw` 0; `log4net.ILog log =` 0; `log4net.ILog logger =` 1; `Linked List Locking` 0; `#pragma` 0; `#region` 6; `#endregion` 6; `[ExcludeFromCodeCoverage]` 1; `//e.Result =` 0; `//_blockingQueue = null;` 0; `//worker.RunWorkerCompleted` 0; `ForEachAwaitWithCancellationAsync` 0; `: IQfcDatamodel` 1; `RemainingEmailLoader = LoadRemainingEmailsToQueueAsync;` 2; `WorkerStarter(worker);` 2; `_remainingLoadActive = ` 3.

**Q1 — `QuickFiler/Controllers/QfcDatamodel.QueueProcessing.cs`, replaces lines 15 to 23** (the `_remainingLoadActive` summary; the declaration at 24 is unchanged), nine lines replacing nine:

        /// <summary>
        /// Issue #424 producer-liveness signal, read by the dequeue gate's <c>sourceActive</c>
        /// delegate and by <see cref="WaitForQueue"/>. <c>InitEmailQueue</c> sets it just before
        /// handing the worker to <see cref="WorkerStarter"/>, and <c>Worker_DoWork</c> clears it in
        /// a <c>finally</c> when the awaited <see cref="RemainingEmailLoader"/> task completes,
        /// because <c>BackgroundWorker.IsBusy</c> already reads idle at that handler's first
        /// incomplete await (issue #950 made the start synchronous in tests, so no particular thread
        /// owns either write). Volatile: the writers and the readers share no other fence.
        /// </summary>

**Q2 — the same file, line 285:** `        /// <c>UnhookItem</c> throw path <see cref="TryUnhookOrReplace"/> (:31-66) removes the failed` becomes `        /// <c>UnhookItem</c> throw path <see cref="TryUnhookOrReplace"/> removes the failed` (the ` (:31-66)` is removed; nothing else on the line changes).

The `QuiesceLoaderAsync` comment at line 52 is unchanged (D-20). After Q1 and Q2 the file is 413 lines. Post-change counts: `RunWorkerAsync` 0; `written on the worker thread and read` 0; `written on the worker thread` 1; `(:31-66)` 0; `TryUnhookOrReplace` 3; `WorkerStarter` 1; `share no other fence` 1; `honest producer-liveness signal` 0; `() => _remainingLoadActive,` 1; `() => false,` 0; `private volatile bool _remainingLoadActive;` 1.

## Execution conventions

- **Tokens.** `WORKTREE` denotes the absolute path of the item worktree supplied in the delegation prompt; the executor substitutes it into every payload and every `git -C` argument at run time and writes the token, never the path, into artifacts. In every `git -C` argument WORKTREE is written with forward slashes, because the Bash channel removes unquoted backslashes; inside a pwsh payload's double-quoted `Set-Location` argument the backslash form is used. `FEATURE` abbreviates the feature folder path. `BASE` denotes `94287369908cc920b21b0e3256314f988ad7d2f5`, which is written in full in every command. `QCT.` abbreviates the namespace prefix `QuickFiler.Controllers.Tests.` in this document only and is expanded in full in every executed filter. No artifact, and no line of this plan, carries an absolute host path, an account name or a machine name.
- **Payload channel.** Each indented payload in the Command Reference runs as one Bash tool call of the form `pwsh -NoProfile -Command '<payload>'`, newlines included, with the substitutions applied. Payloads are never merged into one call: the hook layer scans a pwsh payload as raw text, so a merged payload can combine words that each payload alone does not. Payloads use double quotes only, so the outer single quotes never conflict. Git commands run as single Bash calls of the form `git -C WORKTREE <arguments>`, never chained. No `cd`, no `&&`, `;` or `|` between Bash commands. File edits use the Edit and Write tools with absolute paths. If the Bash tool refuses a pwsh invocation (a refusal whose text begins "This agent is isolated in the worktree"), the executor records PWSH CHANNEL REFUSED with the verbatim refusal in that task's artifact and stops; no rephrased or alternative invocation is attempted. The plan has no non-pwsh fallback, so the orchestrator must dispatch the executor without worktree isolation. The executor is dispatched non-isolated, which means its Bash and pwsh process cwd is the coordinator session tree, not WORKTREE: every command therefore keeps targeting WORKTREE explicitly, through the PREFIX `Set-Location` plus `[System.IO.Directory]::SetCurrentDirectory` in every payload and through `git -C WORKTREE` in every git call, and `WORKTREE-LEAF: agent-a291a7fbabf9d0229` is the per-payload proof that it did.
- **Command rows.** The `Command:` field of a payload artifact records the full payload as executed, with `WORKTREE` in place of the path, followed on the next line by the canonical command it implements. Every payload artifact records `WORKTREE-LEAF: agent-a291a7fbabf9d0229`; any other value means the payload ran in the wrong tree, and the task fails.
- **Exit codes.** `EXIT_CODE:` records the payload's principal exit value as named in each Command Reference entry. Deliberately failing runs carry `ExpectedExitCode:` equal to the deterministic value the task states. A recorded observation whose exit value is not gated carries `ExpectedExitCode:` equal to the observed value when non-zero and says so; the field is omitted when the observed value is 0. The first `Command:`, `EXIT_CODE:` and `ExpectedExitCode:` rows of an artifact form its machine-read record, so a multi-command artifact places its gated command first.
- **Transcription.** Any absolute path inside a transcribed line is replaced by `REDACTED-PATH`. Trx and coverage documents are never copied into FEATURE.
- **Long-running payloads.** `CMD-COVERAGE-RUNNER` and `CMD-COVERAGE-DIRECT` are started with the Bash tool's `run_in_background` option and no shell redirection; the payload's own output, captured by the tool, is the record, and completion is the background-task notification together with a final output line `PAYLOAD-COMPLETE`. A run still in progress after 120 minutes is `COVERAGE RUN STALLED`: stop and report. Before any re-invocation after a timed-out or interrupted call, the executor runs `pwsh -NoProfile -Command '$leaf = "agent-a291a7fbabf9d0229"; $runner = "Invoke-MSTest" + "WithCoverage"; "STRAY_TEST_PROCESSES: " + @(Get-CimInstance Win32_Process -ErrorAction SilentlyContinue | Where-Object { $_.ProcessId -ne $PID -and $null -ne $_.CommandLine -and $_.CommandLine.Contains($leaf) -and ($_.Name -like "vstest*" -or $_.Name -like "testhost*" -or $_.Name -like "dotnet-coverage*" -or ($_.Name -like "pwsh*" -and $_.CommandLine.Contains($runner))) }).Count'` and proceeds only at `STRAY_TEST_PROCESSES: 0`. Both coverage payloads end with the unconditional line `Write-Output "PAYLOAD-COMPLETE"`; a run that ends without that line is `COVERAGE RUN ABORTED`.
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

**File tokens.** `FIX` is `QuickFiler.Test\Controllers\QfcItemController.UiThreadDispatcherFixture.cs`; `FAT` is `QuickFiler.Test\Controllers\QfcItemController.FocusAndThemeTests.cs`; `TS` is `QuickFiler.Test\Controllers\QfcItemController.TestSupport.cs`; `FT` is `QuickFiler.Test\Controllers\QfcItemController.UiThreadDispatcherFixtureTests.cs`; `PC` is `QuickFiler.Test\Controllers\QfcItemController.UiThreadDispatcherPinCountTests.cs`; `PROJ` is `QuickFiler.Test\QuickFiler.Test.csproj`; `SBW` is `QuickFiler.Test\TestSupport\SynchronousBackgroundWorker.cs`; `AFTP` is `QuickFiler.Test\TestSupport\ArmingFakeTimeProvider.cs`; `LIV` is `QuickFiler.Test\Controllers\QfcDatamodelLivenessTests.cs`; `TD` is `QuickFiler.Test\Controllers\QfcDatamodelTeardownTests.cs`; `ZB` is `QuickFiler.Test\Controllers\QfcInitEmailQueueZeroBatchTests.cs`; `DMT` is `QuickFiler.Test\Controllers\QfcDatamodelTests.cs`; `QDM` is `QuickFiler\Controllers\QfcDatamodel.cs`; `QQP` is `QuickFiler\Controllers\QfcDatamodel.QueueProcessing.cs`. **CS4** is `"FIX", "FAT", "TS", "FT"` (each expanded); **CS5** is CS4 plus `"PC"`; **FOLD6** is `"LIV", "TD", "ZB", "DMT", "QDM", "QQP"` (the six existing fold files); **FOLD8** is FOLD6 plus `"SBW", "AFTP"`; **CS13** is CS5 plus FOLD8 (every Write Set `.cs` file). **CODE14-GIT** is the fourteen Write Set code paths with forward slashes, space-separated, for git pathspecs: `QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixture.cs QuickFiler.Test/Controllers/QfcItemController.FocusAndThemeTests.cs QuickFiler.Test/Controllers/QfcItemController.TestSupport.cs QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixtureTests.cs QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherPinCountTests.cs QuickFiler.Test/QuickFiler.Test.csproj QuickFiler.Test/TestSupport/SynchronousBackgroundWorker.cs QuickFiler.Test/TestSupport/ArmingFakeTimeProvider.cs QuickFiler.Test/Controllers/QfcDatamodelLivenessTests.cs QuickFiler.Test/Controllers/QfcDatamodelTeardownTests.cs QuickFiler.Test/Controllers/QfcInitEmailQueueZeroBatchTests.cs QuickFiler.Test/Controllers/QfcDatamodelTests.cs QuickFiler/Controllers/QfcDatamodel.cs QuickFiler/Controllers/QfcDatamodel.QueueProcessing.cs`. **FEATURE-GIT** is `docs/features/active/2026-10-02-focus-and-theme-tests-leak-shared-dispatcher-setup-968`.

**CMD-REBUILD** (`GATEARGS` is either `/p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` or `/p:TreatWarningsAsErrors=true`; `TASKID` substituted; `EXIT_CODE:` is `MSBUILD_EXIT_CODE:`; canonical command `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" GATEARGS`, resolved through vswhere against this worktree's TaskMaster.sln, plus /nodeReuse:false and a normal-verbosity file logger under the ignored coverage directory):

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
    $files = @("QfcItemController.UiThreadDispatcherFixture.cs(", "QfcItemController.FocusAndThemeTests.cs(", "QfcItemController.TestSupport.cs(", "QfcItemController.UiThreadDispatcherFixtureTests.cs(", "QfcItemController.UiThreadDispatcherPinCountTests.cs(", "QuickFiler.Test.csproj(", "SynchronousBackgroundWorker.cs(", "ArmingFakeTimeProvider.cs(", "QfcDatamodelLivenessTests.cs(", "QfcDatamodelTeardownTests.cs(", "QfcInitEmailQueueZeroBatchTests.cs(", "QfcDatamodelTests.cs(", "QfcDatamodel.cs(", "QfcDatamodel.QueueProcessing.cs(")
    $diag = @($lines | Where-Object { $l = $_; (@($files | Where-Object { $l.Contains($_) }).Count -gt 0) -and ($l -match "(error|warning) [A-Z]+[0-9]+") })
    Write-Output ("WRITESET_DIAGNOSTIC_LINES: " + $diag.Count)
    Write-Output ("WRITESET_DIAGNOSTIC_CODES: " + ((@($diag | ForEach-Object { [regex]::Match($_, "(error|warning) ([A-Z]+[0-9]+)").Groups[2].Value }) | Sort-Object -Unique) -join ","))
    Write-Output ("TEST_DLL_EXISTS: " + (Test-Path -LiteralPath "QuickFiler.Test\bin\Debug\QuickFiler.Test.dll"))
    Write-Output ("UCS_TEST_DLL_EXISTS: " + (Test-Path -LiteralPath "UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll"))

`ERRORS:` is read from the build summary line, so `0 Error(s)` is not mistaken for a substring of a larger count. Under /t:Rebuild `SKIP_CORECOMPILE_LINES` is 0 by construction; the two `CSC_OUT_` counts show the compiler ran for the test project and its production dependency (MSBuild echoes each csc command line at normal verbosity; the #950 run observed 2 for each).

**CMD-BUILD** (incremental build so a scoped test run observes a fresh assembly; `TASKID` substituted; `EXIT_CODE:` is `MSBUILD_EXIT_CODE:`; canonical command `msbuild TaskMaster.sln /t:Build /m /p:Configuration=Debug "/p:Platform=Any CPU"`):

    PREFIX
    TOOLS
    $log = "coverage\logs\TASKID.msbuild.log"
    if (Test-Path -LiteralPath $log) { Remove-Item -LiteralPath $log -Force }
    $before = (Get-Item -LiteralPath "QuickFiler.Test\bin\Debug\QuickFiler.Test.dll" -ErrorAction SilentlyContinue).LastWriteTimeUtc
    $beforeProd = (Get-Item -LiteralPath "QuickFiler\bin\Debug\QuickFiler.dll" -ErrorAction SilentlyContinue).LastWriteTimeUtc
    $global:LASTEXITCODE = 0
    & $msbuild TaskMaster.sln /t:Build /m /nodeReuse:false /p:Configuration=Debug "/p:Platform=Any CPU" "/flp:LogFile=$log;Verbosity=normal" | Out-Null
    Write-Output ("MSBUILD_EXIT_CODE: " + $LASTEXITCODE)
    $lines = Get-Content -LiteralPath $log -Encoding UTF8
    Write-Output ("ERRORS: " + (($lines | Select-String -Pattern "^\s*(\d+) Error\(s\)" | Select-Object -Last 1).Matches[0].Groups[1].Value))
    $after = (Get-Item -LiteralPath "QuickFiler.Test\bin\Debug\QuickFiler.Test.dll").LastWriteTimeUtc
    $afterProd = (Get-Item -LiteralPath "QuickFiler\bin\Debug\QuickFiler.dll").LastWriteTimeUtc
    Write-Output ("TEST_DLL_ADVANCED: " + ($null -eq $before -or $after -gt $before))
    Write-Output ("PROD_DLL_ADVANCED: " + ($null -eq $beforeProd -or $afterProd -gt $beforeProd))
    Write-Output ("CSC_OUT_QUICKFILER: " + @($lines | Where-Object { $_.Contains("/out:obj\Debug\QuickFiler.dll") }).Count)
    Write-Output ("CSC_OUT_QUICKFILER_TEST: " + @($lines | Where-Object { $_.Contains("/out:obj\Debug\QuickFiler.Test.dll") }).Count)

`PROD_DLL_ADVANCED:` and `CSC_OUT_QUICKFILER:` are gated only by the tasks that change a production file (P4-T10, P5-T8, P5-T9, P6-T3); elsewhere they are recorded.

**CMD-VSTEST** (`FILTER`, `TASKID` and `NAMES` substituted; an empty `NAMES` prints every result; `EXIT_CODE:` is `VSTEST_EXIT_CODE:`, or 3 when the trx is absent; canonical command `vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation "/TestCaseFilter:FILTER" "/ResultsDirectory:coverage\test-results\968\TASKID" "/Logger:trx;LogFileName=TASKID.trx" "/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None"`, resolved through vswhere; `ASSEMBLY` is `QuickFiler.Test\bin\Debug\QuickFiler.Test.dll` except for the stall probe, which uses `UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll`):

    PREFIX
    TOOLS
    $results = "coverage\test-results\968\TASKID"
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

Filters and name lists (every `QCT.` expanded to `QuickFiler.Controllers.Tests.` when executed):

- `NAMES-PC`: `"EnsureDispatcher_TwoPinsHeld_ReleasingTheFirstKeepsTheDispatcherUntilTheLastRelease", "EnsureDispatcher_TwoPinsHeld_ReleaseOrderDoesNotChangeTheOutcome", "EnsureDispatcher_UnderATransactionHoldingALiveDispatcher_ReleasingAllPinsLeavesTheLiveDispatcher", "EnsureDispatcher_AfterAFullPinCycle_AFreshSinglePinStillInstallsAndRestores"`.
- `NAMES-FT`: `"EnsureDispatcher_WhileATransactionHoldsALiveDispatcher_DoesNotReplaceIt", "EnsureDispatcher_WhenTheFieldIsNull_InstallsAndRestoresOnDispose", "EnsureDispatcher_ScopeDisposedTwice_IsIdempotent", "Transaction_SecondCallerCannotInstallUntilTheFirstRestores", "Transaction_DisposedTwice_DoesNotOverReleaseTheGate", "Install_CalledTwiceOnTheSameTransaction_ThrowsInvalidOperationException", "TransactionGate_WhileThisTestHoldsATransaction_HasExactlyOneUnreleasedAcquisition", "BeginTransactionAsync_ZeroBoundWhileThisTestHoldsThePermit_ThrowsTimeoutExceptionAndReleasesNothing"`.
- `NAMES-THEME`: `"SetThemeDark_FromNormal_SelectsDarkNormalTheme", "SetThemeLight_FromNormal_SelectsLightNormalTheme"`.
- `NAMES-LIVENESS`: `"DequeueNextItemGroupAsync_WhileLoaderStillProducing_KeepsPollingAfterWorkerIdle", "DequeueNextItemGroupAsync_HighConfidenceMode_WaitsWhileSourceWorkerActive"`.
- `NAMES-TARGETS`: NAMES-PC, NAMES-FT, NAMES-THEME and NAMES-LIVENESS together (sixteen names).
- `FILTER-PC-T1`: `FullyQualifiedName=QCT.QfcItemController_UiThreadDispatcherPinCountTests.EnsureDispatcher_TwoPinsHeld_ReleasingTheFirstKeepsTheDispatcherUntilTheLastRelease`.
- `FILTER-PC-T234`: the `FullyQualifiedName=` expressions for the other three NAMES-PC methods, joined with `|` (vstest rejects `OR`).
- `FILTER-PC-CLASS`: `FullyQualifiedName~QCT.QfcItemController_UiThreadDispatcherPinCountTests.`
- `FILTER-FT-CLASS`: `FullyQualifiedName~QCT.QfcItemController_UiThreadDispatcherFixtureTests.`
- `FILTER-FAT-CLASS`: `FullyQualifiedName~QCT.QfcItemController_FocusAndThemeTests.`
- `FILTER-CONCURRENT`: the three class filters joined with `|` (29 tests: 8, 4 and 17).
- `FILTER-BASELINE-CONCURRENT`: `FILTER-FT-CLASS` and `FILTER-FAT-CLASS` joined with `|` (25 tests; the pin-count class does not exist at Phase 0).
- `FILTER-DATAMODEL`: `FullyQualifiedName~QCT.QfcDatamodelLivenessTests.|FullyQualifiedName~QCT.QfcDatamodelTeardownTests.|FullyQualifiedName~QCT.QfcInitEmailQueueZeroBatchTests.|FullyQualifiedName~QCT.QfcDatamodelTests.` (21 tests: 4, 5, 3 and 9; the trailing dot keeps `QfcDatamodelTests.` from matching the other classes, and no other class name in QuickFiler.Test starts with these four prefixes followed by a dot).
- `FILTER-LIVENESS-PAIR`: `FullyQualifiedName=QCT.QfcDatamodelLivenessTests.DequeueNextItemGroupAsync_WhileLoaderStillProducing_KeepsPollingAfterWorkerIdle|FullyQualifiedName=QCT.QfcDatamodelTests.DequeueNextItemGroupAsync_HighConfidenceMode_WaitsWhileSourceWorkerActive`.
- `FILTER-STALL`: `FullyQualifiedName~HelperClasses.ShellUtilities_Tests|FullyQualifiedName~HelperClasses.ShellUtilitiesStatic_Tests|FullyQualifiedName~HelperClasses.SysImageListHelperTests|FullyQualifiedName~EmailIntelligence.OSBrowser_Tests`.

**CMD-COVERAGE-RUNNER** (CLAUDE.md step 4 route; `STAGE` is `baseline` or `final`; `EXIT_CODE:` is `RUNNER_EXIT_CODE:`; canonical command `pwsh -NoProfile -File scripts\vscode\Invoke-MSTestWithCoverage.ps1` run from the worktree root):

    PREFIX
    New-Item -ItemType Directory -Path "coverage\logs" -Force | Out-Null
    foreach ($f in @("coverage\coverage.cobertura.xml", "coverage\coverage.cobertura.jacoco.xml", "coverage\test-results\mstest-coverage-run.trx", "coverage\test-results\mstest-coverage-run.summary.txt", "coverage\STAGE-968.cobertura.xml", "coverage\STAGE-968.trx")) { if (Test-Path -LiteralPath $f) { Remove-Item -LiteralPath $f -Force } }
    $script = Join-Path (Get-Location).Path "scripts\vscode\Invoke-MSTestWithCoverage.ps1"
    $global:LASTEXITCODE = 0
    & pwsh -NoProfile -File $script 2>&1 | Tee-Object -FilePath "coverage\logs\STAGE-968.runner.log" | Out-Null
    Write-Output ("RUNNER_EXIT_CODE: " + $LASTEXITCODE)
    $log = Get-Content -LiteralPath "coverage\logs\STAGE-968.runner.log" -Raw -Encoding UTF8
    Write-Output ("DISCOVERED_LINE: " + [regex]::Match($log, "Discovered \d+ test assemblies\.").Value)
    Write-Output ("FIRST_PARTY_LINE: " + [regex]::Match($log, "First-party coverage: [^\r\n]*").Value)
    Write-Output ("THRESHOLD_MESSAGE: " + [regex]::Match($log, "Cobertura (line|branch) coverage [^\r\n]*threshold\.").Value)
    Write-Output ("COLLECT_FAILURE_MESSAGE: " + [regex]::Match($log, "MSTest with coverage failed with exit code \d+").Value)
    Write-Output ("DOCUMENT_PRESENT: " + (Test-Path -LiteralPath "coverage\coverage.cobertura.xml"))
    Write-Output ("TRX_PRESENT: " + (Test-Path -LiteralPath "coverage\test-results\mstest-coverage-run.trx"))
    if (Test-Path -LiteralPath "coverage\coverage.cobertura.xml") { Copy-Item -LiteralPath "coverage\coverage.cobertura.xml" -Destination "coverage\STAGE-968.cobertura.xml" -Force }
    if (Test-Path -LiteralPath "coverage\test-results\mstest-coverage-run.trx") { Copy-Item -LiteralPath "coverage\test-results\mstest-coverage-run.trx" -Destination "coverage\STAGE-968.trx" -Force }
    Write-Output "PAYLOAD-COMPLETE"

**CMD-COVERAGE-DIRECT** (the runner's inner invocation issued directly with the four-class exclusion; `STAGE` substituted; `EXIT_CODE:` is `COLLECT_EXIT_CODE:`; canonical command `dotnet-coverage collect --output coverage\STAGE-968.cobertura.xml --output-format cobertura --settings coverage\effective-coverage-968.config -- vstest.console.exe <discovered test assemblies> /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation "/TestCaseFilter:<filter>" "/ResultsDirectory:coverage\test-results\968\STAGE" "/Logger:trx;LogFileName=STAGE-968.trx" "/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None"`):

    PREFIX
    . (Join-Path (Get-Location).Path "scripts\vscode\Invoke-MSTestWithCoverage.ps1")
    $ErrorActionPreference = "Continue"
    $repo = (Get-Location).Path
    New-Item -ItemType Directory -Path "coverage\logs" -Force | Out-Null
    foreach ($f in @("coverage\STAGE-968.cobertura.xml", "coverage\STAGE-968.trx")) { if (Test-Path -LiteralPath $f) { Remove-Item -LiteralPath $f -Force } }
    $canonical = Get-Content -LiteralPath "coverage.config" -Raw -Encoding UTF8
    $derived = ConvertTo-DerivedCoverageSettingsXml -CanonicalSettingsXml $canonical
    $effective = Join-Path $repo "coverage\effective-coverage-968.config"
    Set-Content -LiteralPath $effective -Value $derived -Encoding UTF8 -NoNewline
    $vswhere = Join-Path ${env:ProgramFiles(x86)} "Microsoft Visual Studio\Installer\vswhere.exe"
    $vstest = & $vswhere -latest -products * -find "Common7\IDE\Extensions\TestPlatform\vstest.console.exe" | Select-Object -First 1
    $rootLen = $repo.TrimEnd([char]92).Length
    $asm = @(Get-ChildItem -Path $repo -Recurse -Filter "*.Test.dll" | Where-Object { $_.FullName -like "*\bin\Debug\*" -and $_.FullName -notlike "*\obj\*" -and $_.FullName -notlike "*\ref\*" -and $_.FullName.Substring($rootLen) -notlike "\.claude\*" } | Select-Object -ExpandProperty FullName)
    $filter = "TestCategory!=LiveOutlook&FullyQualifiedName!~HelperClasses.ShellUtilities_Tests&FullyQualifiedName!~HelperClasses.ShellUtilitiesStatic_Tests&FullyQualifiedName!~HelperClasses.SysImageListHelperTests&FullyQualifiedName!~EmailIntelligence.OSBrowser_Tests"
    $output = Join-Path $repo "coverage\STAGE-968.cobertura.xml"
    $settings = Join-Path $repo "scripts\vscode\TaskMaster.cli.runsettings"
    $results = Join-Path $repo "coverage\test-results\968\STAGE"
    if (Test-Path -LiteralPath $results) { Remove-Item -LiteralPath $results -Recurse -Force }
    $global:LASTEXITCODE = 0
    & dotnet-coverage collect --output $output --output-format cobertura --settings $effective -- $vstest @asm "/Settings:$settings" /InIsolation "/TestCaseFilter:$filter" "/ResultsDirectory:$results" "/Logger:trx;LogFileName=STAGE-968.trx" "/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None" 2>&1 | Tee-Object -FilePath "coverage\logs\STAGE-968.collect.log" | Out-Null
    Write-Output ("COLLECT_EXIT_CODE: " + $LASTEXITCODE)
    Write-Output ("ASSEMBLY_COUNT: " + $asm.Count)
    $asm | ForEach-Object { Write-Output ("ASSEMBLY: " + $_.Substring($rootLen)) }
    Write-Output ("SEQUENCE_FILES: " + @(Get-ChildItem -LiteralPath $results -Recurse -Filter "Sequence_*.xml" -ErrorAction SilentlyContinue).Count)
    if (Test-Path -LiteralPath (Join-Path $results "STAGE-968.trx")) { Copy-Item -LiteralPath (Join-Path $results "STAGE-968.trx") -Destination "coverage\STAGE-968.trx" -Force }
    Write-Output ("TRX_PRESENT: " + (Test-Path -LiteralPath "coverage\STAGE-968.trx"))
    Write-Output ("DOCUMENT_PRESENT: " + (Test-Path -LiteralPath $output))
    Write-Output "PAYLOAD-COMPLETE"

**CMD-COVERAGE-POST** (summarise the trx, post-process if raw, apply the floors, print the committed forms and the figures; `STAGE` substituted; `RAW` is `True` under DIRECT and under a RUNNER run whose `COLLECT_FAILURE_MESSAGE:` is non-empty, otherwise `False`, because a completed runner run has already post-processed the document in place; `EXIT_CODE:` is the payload's own exit status, 0 when every line printed):

    PREFIX
    . (Join-Path (Get-Location).Path "scripts\vscode\Invoke-MSTestWithCoverage.Helpers.ps1")
    . (Join-Path (Get-Location).Path "scripts\vscode\Invoke-MSTest.TrxSummary.ps1")
    $ErrorActionPreference = "Continue"
    $repo = (Get-Location).Path
    $trxText = Get-Content -LiteralPath "coverage\STAGE-968.trx" -Raw -Encoding UTF8
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
    foreach ($r in @($trx.SelectNodes("//t:UnitTestResult", $ns))) { $o = $r.GetAttribute("outcome"); if ($o -ne "Passed" -and $o -ne "NotExecuted") { $m = $r.SelectSingleNode("t:Output/t:ErrorInfo/t:Message", $ns); Write-Output ("MESSAGE " + $r.GetAttribute("testName") + " :: " + $(if ($m) { $m.InnerText -replace "\s+", " " } else { "(no message)" })) } }
    $doc = Get-Content -LiteralPath "coverage\STAGE-968.cobertura.xml" -Raw -Encoding UTF8
    if ("RAW" -eq "True") { $doc = ConvertTo-KoverageCoberturaXml -XmlContent $doc -RepoRoot $repo; Set-Content -LiteralPath "coverage\STAGE-968.cobertura.xml" -Value $doc -Encoding UTF8 -NoNewline }
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
    Write-Output ("TEST_ASSEMBLY_PACKAGES: " + @($xml.SelectNodes("//package") | Where-Object { $_.GetAttribute("name") -like "*.Test" }).Count)
    Write-Output ("QFCDATAMODEL_CLASS_ENTRIES: " + @($xml.SelectNodes("//class") | Where-Object { $_.GetAttribute("name") -like "QuickFiler.Controllers.QfcDatamodel*" }).Count)

The projection and the summary block are the two committed forms (CLAUDE.md "Committed Test Evidence Format"); the remaining lines are figures. `TEST_ASSEMBLY_PACKAGES:` is the observation behind D-7: 0 means no test assembly was instrumented, so no changed test line of this plan has a coverage figure. `QFCDATAMODEL_CLASS_ENTRIES:` is recorded, not gated (D-7): it shows whether the `[ExcludeFromCodeCoverage]` type appears in the report at all. The `MESSAGE` loop prints every non-passed, executed test's message (round-1 defect 10), so P8-T5 can read the `The UI dispatcher has not been captured` text.

**CMD-HASH** (`FILES` substituted; hashes only):

    PREFIX
    foreach ($p in @(FILES)) { Write-Output ("HASH " + $p + " = " + (Get-FileHash -Algorithm SHA256 -LiteralPath $p).Hash) }

**CMD-LINECOUNT** (`FILES` substituted):

    PREFIX
    foreach ($p in @(FILES)) { Write-Output ("LINES " + $p + " = " + @(Get-Content -LiteralPath $p -Encoding UTF8).Count) }

**CMD-TOKEN-COUNT** (`FILE` and the `TOKENS` list substituted; ordinal, case-sensitive substring counts per physical line, so a token wrapped across two lines reads 0; no token contains a double quote):

    PREFIX
    $src = Get-Content -LiteralPath "FILE" -Encoding UTF8
    foreach ($t in @(TOKENS)) { Write-Output ("TOKEN [" + $t + "] = " + @($src | Where-Object { $_.Contains($t) }).Count) }

**CMD-SPAN-TOKEN-COUNT** (`FILE`, `START`, `END` and `TOKENS` substituted; the span runs from the first line containing START up to, not including, the next line containing END, or to the end of the file when END is the literal `EOF`; exit 4 when an anchor is missing):

    PREFIX
    $src = Get-Content -LiteralPath "FILE" -Encoding UTF8
    $s = -1; for ($i = 0; $i -lt $src.Count; $i++) { if ($src[$i].Contains("START")) { $s = $i; break } }
    $e = -1; if ($s -ge 0) { if ("END" -eq "EOF") { $e = $src.Count } else { for ($i = $s + 1; $i -lt $src.Count; $i++) { if ($src[$i].Contains("END")) { $e = $i; break } } } }
    Write-Output ("SPAN: " + ($s + 1) + "-" + $e)
    if ($s -lt 0 -or $e -lt 0) { exit 4 }
    $span = $src[$s..($e - 1)]
    foreach ($t in @(TOKENS)) { Write-Output ("SPAN-TOKEN [" + $t + "] = " + @($span | Where-Object { $_.Contains($t) }).Count) }

Span anchors used by this plan (file, START, END):

- `R4SPAN`: FT, `public async Task Transaction_SecondCallerCannotInstallUntilTheFirstRestores()`, `public async Task Transaction_DisposedTwice_DoesNotOverReleaseTheGate()`. Baseline `SPAN: 212-284`.
- `R4HEAD`: FT, the R4SPAN START, `Dispatcher original = UiThreadDispatcherFixture.Current;` (the first occurrence after the declaration). Baseline `SPAN: 212-224`.
- `R4TAIL`: FT, `issue #230 lost update`, `QfcItemControllerTestSupport.ShutdownDispatcher(liveA);` (the first occurrence after the START). Baseline `SPAN: 267-273`.
- `R1SPAN`: FT, `public async Task EnsureDispatcher_WhileATransactionHoldsALiveDispatcher_DoesNotReplaceIt()`, `public async Task EnsureDispatcher_WhenTheFieldIsNull_InstallsAndRestoresOnDispose()`.
- `R2SPAN`: FT, the R1SPAN END, `public async Task EnsureDispatcher_ScopeDisposedTwice_IsIdempotent()`.
- `R3SPAN`: FT, the R2SPAN END, the R4SPAN START.
- `ENSURE`: FIX, `internal static IDisposable EnsureDispatcher()`, `internal const int TransactionGateAcquireTimeoutMs = 120000;`. Baseline `SPAN: 122-145`.
- `SCOPE`: FIX, `private sealed class EnsureScope : IDisposable`, `internal sealed class UiThreadDispatcherTransaction : IDisposable`. Baseline `SPAN: 249-283`.
- `T1SPAN`: PC, `public async Task EnsureDispatcher_TwoPinsHeld_ReleasingTheFirstKeepsTheDispatcherUntilTheLastRelease()`, `public async Task EnsureDispatcher_TwoPinsHeld_ReleaseOrderDoesNotChangeTheOutcome()`.
- `T2SPAN`: PC, the T1SPAN END, `public async Task EnsureDispatcher_UnderATransactionHoldingALiveDispatcher_ReleasingAllPinsLeavesTheLiveDispatcher()`.
- `T3SPAN`: PC, the T2SPAN END, `public async Task EnsureDispatcher_AfterAFullPinCycle_AFreshSinglePinStillInstallsAndRestores()`.
- `T4SPAN`: PC, the T3SPAN END, `EOF`.
- `T1-LIVE`: LIV, `public async Task DequeueNextItemGroupAsync_WhileLoaderStillProducing_KeepsPollingAfterWorkerIdle()`, `/// <summary>Reads the issue #424 producer-liveness flag by reflection.</summary>`. Baseline `SPAN: 110-165`.
- `HELD`: LIV, `private static QfcDatamodel StartHeldOpenLoader(`, `public void RemainingLoadActive_AcrossAsyncVoidFirstAwait_StaysTrueWhileLoaderProduces()`. Baseline `SPAN: 183-217`.
- `T-SIB`: DMT, `public async Task DequeueNextItemGroupAsync_HighConfidenceMode_WaitsWhileSourceWorkerActive()`, `public async Task TryQueueRemainingMailItemAsync_HighConfidenceEnabled_AddsBelowThresholdCandidate()`. Baseline `SPAN: 96-133`.
- `GATE-LAMBDA`: QQP, `var gate = new QfcStreamingDequeueConfidenceGate(`, `QfcGateBatch batch = await gate.DequeueAsync(quantity, timeOut, _token);`. Baseline `SPAN: 299-310`.

**CMD-PIN-NESTING** (`FILE`, `START`, `END` substituted; same span rule as CMD-SPAN-TOKEN-COUNT; prints, in file order, every span line that carries one of the six nesting tokens, so the acquisition order can be read from the line numbers):

    PREFIX
    $src = Get-Content -LiteralPath "FILE" -Encoding UTF8
    $s = -1; for ($i = 0; $i -lt $src.Count; $i++) { if ($src[$i].Contains("START")) { $s = $i; break } }
    $e = -1; if ($s -ge 0) { if ("END" -eq "EOF") { $e = $src.Count } else { for ($i = $s + 1; $i -lt $src.Count; $i++) { if ($src[$i].Contains("END")) { $e = $i; break } } } }
    Write-Output ("SPAN: " + ($s + 1) + "-" + $e)
    if ($s -lt 0 -or $e -lt 0) { exit 4 }
    for ($i = $s; $i -lt $e; $i++) { $t = $src[$i]; foreach ($k in @("BeginTransactionAsync()", "EnsureUiThreadDispatcher()", "Dispose()", ".Install(", "finally", "ShutdownDispatcher(")) { if ($t.Contains($k)) { Write-Output ("NEST " + ($i + 1) + " [" + $k + "] " + $t.Trim()); break } } }

**CMD-CENSUS** (the two independent search strategies of research section 2.1 over every `*.cs` file under the worktree outside `packages`, `.claude`, `obj` and `bin`):

    PREFIX
    $root = (Get-Location).Path
    $rootLen = $root.TrimEnd([char]92).Length
    $files = @(Get-ChildItem -Path $root -Recurse -Filter "*.cs" | Where-Object { $rel = $_.FullName.Substring($rootLen); $rel -notlike "\packages\*" -and $rel -notlike "\.claude\*" -and $rel -notlike "*\obj\*" -and $rel -notlike "*\bin\*" })
    Write-Output ("CS_FILES: " + $files.Count)
    $primary = @($files | Select-String -Pattern "EnsureUiThreadDispatcher\(\)|EnsureDispatcher\(\)" -CaseSensitive)
    Write-Output ("PRIMARY_LINES: " + $primary.Count)
    foreach ($g in @($primary | Group-Object Path)) { Write-Output ("PRIMARY-FILE " + $g.Name.Substring($rootLen) + " = " + $g.Count) }
    foreach ($m in $primary) { Write-Output ("PRIMARY " + $m.Path.Substring($rootLen) + ":" + $m.LineNumber + " :: " + $m.Line.Trim()) }
    $cross = @($files | Select-String -Pattern "EnsureUiThreadDispatcher|EnsureDispatcher" -CaseSensitive)
    Write-Output ("CROSS_LINES: " + $cross.Count)
    foreach ($g in @($cross | Group-Object Path)) { Write-Output ("CROSS-FILE " + $g.Name.Substring($rootLen) + " = " + $g.Count) }
    foreach ($m in $cross) { Write-Output ("CROSS " + $m.Path.Substring($rootLen) + ":" + $m.LineNumber + " :: " + $m.Line.Trim()) }
    Write-Output ("CONTROL_LINES: " + @($files | Select-String -Pattern "BeginTransactionAsync\(" -CaseSensitive).Count)

`CONTROL_LINES:` is the positive control (fact 5: 23 before the change, 28 after N1 adds five).

**CMD-LEGACY-CALLERS** (the zero-caller proof for the four `QfcDatamodel` members, two independent strategies over the same file set as CMD-CENSUS plus an extension-unfiltered sweep; run before any removal):

    PREFIX
    $root = (Get-Location).Path
    $rootLen = $root.TrimEnd([char]92).Length
    $files = @(Get-ChildItem -Path $root -Recurse -Filter "*.cs" | Where-Object { $rel = $_.FullName.Substring($rootLen); $rel -notlike "\packages\*" -and $rel -notlike "\.claude\*" -and $rel -notlike "*\obj\*" -and $rel -notlike "*\bin\*" })
    Write-Output ("CS_FILES: " + $files.Count)
    $primary = @($files | Select-String -Pattern "Worker_RunWorkerCompleted|LoadRemainingEmailsToQueue\b|LoadRemainingEmailsToQueueAsync|Linked List Locking" -CaseSensitive)
    Write-Output ("PRIMARY_LINES: " + $primary.Count)
    foreach ($m in $primary) { Write-Output ("PRIMARY " + $m.Path.Substring($rootLen) + ":" + $m.LineNumber + " :: " + $m.Line.Trim()) }
    $logField = @(Get-ChildItem -Path (Join-Path $root "QuickFiler\Controllers") -Filter "QfcDatamodel*.cs" | Select-String -Pattern "\blog\b" -CaseSensitive)
    Write-Output ("LOG_LINES: " + $logField.Count)
    foreach ($m in $logField) { Write-Output ("LOG " + $m.Path.Substring($rootLen) + ":" + $m.LineNumber + " :: " + $m.Line.Trim()) }
    $q = [char]34
    $cross = @($files | Select-String -Pattern ($q + "Worker_RunWorkerCompleted" + $q + "|" + $q + "LoadRemainingEmailsToQueue|" + $q + "log" + $q + "|nameof\(log\)|GetField\(" + $q + "log") -CaseSensitive)
    Write-Output ("CROSS_LINES: " + $cross.Count)
    foreach ($m in $cross) { Write-Output ("CROSS " + $m.Path.Substring($rootLen) + ":" + $m.LineNumber + " :: " + $m.Line.Trim()) }
    $all = @(Get-ChildItem -Path $root -Recurse -File | Where-Object { $rel = $_.FullName.Substring($rootLen); $rel -notlike "\packages\*" -and $rel -notlike "\.claude\*" -and $rel -notlike "*\obj\*" -and $rel -notlike "*\bin\*" -and $rel -notlike "\.git\*" -and $rel -notlike "\coverage\*" -and $rel -notlike "\.dotnet-sdk\*" })
    $sweep = @($all | Select-String -Pattern "Worker_RunWorkerCompleted|LoadRemainingEmailsToQueue" -CaseSensitive -ErrorAction SilentlyContinue)
    Write-Output ("SWEEP_FILES: " + @($sweep | Group-Object Path).Count)
    foreach ($g in @($sweep | Group-Object Path)) { if ($g.Name.EndsWith(".cs")) { Write-Output ("SWEEP-CS " + $g.Name.Substring($rootLen) + " = " + $g.Count) } }
    $iface = @(Get-ChildItem -LiteralPath (Join-Path $root "QuickFiler\Interfaces\IQfcDatamodel.cs") | Select-String -Pattern "Worker_RunWorkerCompleted|LoadRemainingEmailsToQueue|\blog\b" -CaseSensitive)
    Write-Output ("INTERFACE_LINES: " + $iface.Count)
    $ivt = @(Get-ChildItem -Path (Join-Path $root "QuickFiler") -Recurse -Filter "*.cs" | Select-String -Pattern "InternalsVisibleTo" -CaseSensitive)
    foreach ($m in $ivt) { Write-Output ("IVT " + $m.Path.Substring($rootLen) + ":" + $m.LineNumber + " :: " + $m.Line.Trim()) }
    Write-Output ("QFCDATAMODEL_LINES: " + @(Get-Content -LiteralPath "QuickFiler\Controllers\QfcDatamodel.cs" -Encoding UTF8).Count)

**CMD-ADDED-SCAN** (added lines of the anchored diff over the fourteen Write Set code files; run after the P6-T9 commit so the new files are tracked):

    PREFIX
    $diff = @(git diff 94287369908cc920b21b0e3256314f988ad7d2f5 -- CODE14-GIT)
    Write-Output ("GIT_DIFF_EXIT_CODE: " + $LASTEXITCODE)
    $added = @($diff | Where-Object { $_.StartsWith("+") -and -not $_.StartsWith("+++") })
    Write-Output ("ADDED_LINES: " + $added.Count)
    foreach ($t in @("Thread.Sleep", "Task.Delay", "DoNotParallelize", "Retry(", "Path.GetTempFileName", "Path.GetTempPath", "Workers", "Timeout(", "[Timeout(GateTimeoutMs)]", "GateTimeoutMs = ", "await Task.Yield();", "for (int i", "using var ", "_pinCount", "ArmingFakeTimeProvider")) { Write-Output ("ADDED-TOKEN [" + $t + "] = " + @($added | Where-Object { $_.Contains($t) }).Count) }
    foreach ($l in @($added | Where-Object { $_.Contains("GateTimeoutMs = ") })) { Write-Output ("ADDED-LINE " + $l.Substring(1).Trim()) }

`_pinCount` and `ArmingFakeTimeProvider` are the positive controls: both are added by this plan, so a scan that cannot see added lines reports 0 for them and the gate fails.

**CMD-HUNKS** (`FILE-GIT` substituted with one forward-slash path; prints the hunk headers of the anchored diff so a region can be shown untouched):

    PREFIX
    $d = @(git diff 94287369908cc920b21b0e3256314f988ad7d2f5 -- "FILE-GIT")
    Write-Output ("GIT_DIFF_EXIT_CODE: " + $LASTEXITCODE)
    foreach ($l in $d) { if ($l.StartsWith("@@")) { Write-Output ("HUNK " + $l) } }
    Write-Output ("HUNK_COUNT: " + @($d | Where-Object { $_.StartsWith("@@") }).Count)

**CMD-EOL** (`FILE` substituted with PC, SBW or AFTP; normalises the new file to CRLF after the Write tool creates it, then reports the line-ending census):

    PREFIX
    $p = "FILE"
    $t = [System.IO.File]::ReadAllText($p)
    $crlf = [string][char]13 + [string][char]10
    $n = [regex]::Replace($t, "\r?\n", $crlf)
    if (-not $n.EndsWith($crlf)) { $n = $n + $crlf }
    [System.IO.File]::WriteAllText($p, $n, (New-Object System.Text.UTF8Encoding($false)))
    $b = [System.IO.File]::ReadAllText($p)
    Write-Output ("BARE_LF: " + [regex]::Matches($b, "(?<!\r)\n").Count)
    Write-Output ("CRLF_COUNT: " + [regex]::Matches($b, "\r\n").Count)
    Write-Output ("LINES: " + @(Get-Content -LiteralPath $p -Encoding UTF8).Count)

### Phase 0 — Policy Reads, Anchor, Bootstrap and Baseline Capture

- [x] [P0-T1] Read the policy documents in the mandatory order — CLAUDE.md, then .claude/rules/general-code-change.md, then .claude/rules/general-unit-test.md, then .claude/rules/csharp.md — plus .claude/rules/plan-acceptance-gates.md, .claude/rules/tonality.md, .claude/skills/evidence-and-timestamp-conventions/SKILL.md and .claude/skills/acceptance-criteria-tracking/SKILL.md, and record the read in FEATURE/evidence/baseline/phase0-instructions-read.md.
  - Acceptance: the artifact carries `Timestamp:`, a `Policy Order:` line naming the four mandatory documents in that order, and one line per document read. No policy document is modified.
- [x] [P0-T2] Read FEATURE/spec.md, FEATURE/issue.md (including its Coordinator Scope Amendment) and both research records (FEATURE/research/2026-10-02T05-50-dispatcher-pin-call-sites-research.md and FEATURE/research/2026-10-02T22-20-qfc-datamodel-972-fold-research.md) in full and record the Write Set, the prohibited paths, the acceptance-criteria inventory and the amendment literals in FEATURE/evidence/baseline/scope-and-anchor.md (this task creates the file; P0-T3 appends to it).
  - Commands: `CMD-TOKEN-COUNT` on `docs\features\active\2026-10-02-focus-and-theme-tests-leak-shared-dispatcher-setup-968\spec.md` with TOKENS `"- [ ] AC", "- [x] AC", "- [ ] AC32:", "Amendment 1.2", "Amendment 1.1", "acquired and released inside a held", "the removal of its baseline pin", "inherited committed set"`.
  - Acceptance: the artifact lists the fourteen code paths of the Write Set verbatim; names the prohibited paths from the Write Set section; records that issue.md line 12 reads `- Work Mode: full-bug` and that its `## Coordinator Scope Amendment (2026-10-02T22-15, binding)` heading exists; records the spec tokens as `- [ ] AC` 32, `- [x] AC` 0, `- [ ] AC32:` 1, and each of the five literals at least 1 (fact 22: 1, 1, 4, 1, 2; a value of 0 for any of them is `SPEC AMENDMENT MISSING`: stop, because the plan is written against the amended spec); and records that both promoted records `docs/features/potential/promoted/2026-10-02-focus-and-theme-tests-leak-shared-dispatcher-setup.md` and `docs/features/potential/promoted/2026-10-02-qfc-datamodel-950-review-residuals.md` exist on the tree.
- [x] [P0-T3] Record the anchor and the pre-change tree state by appending to FEATURE/evidence/baseline/scope-and-anchor.md.
  - Commands, each a separate Bash call: `git -C WORKTREE rev-parse HEAD`; `git -C WORKTREE rev-parse --abbrev-ref HEAD`; `git -C WORKTREE rev-parse origin/main`; `git -C WORKTREE merge-base --is-ancestor 94287369908cc920b21b0e3256314f988ad7d2f5 HEAD`; `git -C WORKTREE merge-base origin/main HEAD`; `git -C WORKTREE diff --name-status 94287369908cc920b21b0e3256314f988ad7d2f5 HEAD`; `git -C WORKTREE diff --exit-code 94287369908cc920b21b0e3256314f988ad7d2f5 HEAD -- QuickFiler QuickFiler.Test UtilitiesCS/Threading/UiThread.cs UtilitiesCS/HelperClasses/ThemeHelpers/Theme.cs scripts/vscode TaskMaster.runsettings coverage.config .csharpierignore .gitignore global.json dotnet-tools.json`; `git -C WORKTREE status --porcelain --untracked-files=all`.
  - Acceptance, all required: `HEAD-SHA:` records the first output as an observation; `BRANCH:` reads `bug/focus-and-theme-tests-leak-shared-dispatcher-setup-968`; `ORIGIN-MAIN-SHA:` records the third output as an observation (the P8-T9 `BASE REF MOVED` comparison basis); the ancestor check exits 0 (otherwise `BASE-SHA MISMATCH`: record and stop); the merge-base command prints exactly `94287369908cc920b21b0e3256314f988ad7d2f5` (otherwise `BASE AHEAD OF BRANCH`: record both values and stop; the operator brings the branch up to origin/main outside the plan and restarts at P0-T1); `INHERITED-COMMITTED:` lists every name-status line (expected non-empty: fact 11), and every listed path is under FEATURE or is exactly `docs/features/potential/promoted/2026-10-02-focus-and-theme-tests-leak-shared-dispatcher-setup.md` or `docs/features/potential/promoted/2026-10-02-qfc-datamodel-950-review-residuals.md` (otherwise `INHERITED SET OUT OF SCOPE`: stop); the scoped `--exit-code` diff exits 0, recorded as `CODE-TREE-AT-BASE: UNCHANGED` (otherwise `CITED TREE ADVANCED`: stop, because every line citation in this plan is against BASE); `PRE-EXISTING-WORKTREE-PATHS:` lists every porcelain line verbatim or `NONE`, and no line names a path under QuickFiler/ or QuickFiler.Test/ (otherwise `CODE TREE DIRTY AT ANCHOR`: stop). The porcelain output is not asserted empty: modified `.claude/agent-memory/` files and this plan's check-off edits are expected.
- [x] [P0-T4] Provision the repository .NET SDK with scripts/vscode/Install-RepoDotNetSdk.ps1 (guarded) and record it in FEATURE/evidence/baseline/bootstrap-sdk.md.
  - Command: `pwsh -NoProfile -Command 'PREFIX; if (-not (Test-Path -LiteralPath ".dotnet-sdk\sdk\8.0.205")) { & (Join-Path (Get-Location).Path "scripts\vscode\Install-RepoDotNetSdk.ps1") }; "SDK_MARKER=$(Test-Path -LiteralPath ".dotnet-sdk\sdk\8.0.205")"; dotnet --version; "DOTNET_EXIT=$LASTEXITCODE"'` (PREFIX expanded to its three lines, joined with semicolons).
  - Acceptance: `WORKTREE-LEAF: agent-a291a7fbabf9d0229`, `SDK_MARKER=True`, `DOTNET_EXIT=0`, and the version line is a version string rather than the global.json `errorMessage` text. Any installer line naming a resolved path is transcribed with `REDACTED-PATH`.
- [x] [P0-T5] Restore the manifest tools with `dotnet tool restore` at the worktree root (manifest dotnet-tools.json) and record it in FEATURE/evidence/baseline/bootstrap-tool-restore.md.
  - Command: `pwsh -NoProfile -Command 'PREFIX; dotnet tool restore; "RESTORE_EXIT=$LASTEXITCODE"; dotnet tool list --local; dotnet tool run csharpier check --help | Out-Null; "CHECK_HELP_EXIT=$LASTEXITCODE"'`.
  - Acceptance: `WORKTREE-LEAF: agent-a291a7fbabf9d0229`, `RESTORE_EXIT=0`, a local tool row with Package Id `csharpier` and Version `1.2.6`, and `CHECK_HELP_EXIT=0`. Only the Package Id and Version columns are transcribed (the Manifest column carries an absolute path).
- [x] [P0-T6] Restore NuGet packages with scripts/vscode/Invoke-Restore.ps1 and record it in FEATURE/evidence/baseline/bootstrap-nuget-restore.md.
  - Command: `pwsh -NoProfile -Command 'PREFIX; $env:MSBUILDDISABLENODEREUSE = "1"; & (Join-Path (Get-Location).Path "scripts\vscode\Invoke-Restore.ps1"); "RESTORE_EXIT=$LASTEXITCODE"; "PACKAGE_DIRS=$(@(Get-ChildItem -LiteralPath packages -Directory -ErrorAction SilentlyContinue).Count)"'`.
  - Acceptance: `WORKTREE-LEAF: agent-a291a7fbabf9d0229`, `RESTORE_EXIT=0` and `PACKAGE_DIRS=` at least 1.
- [x] [P0-T7] Verify analyzer-path alignment across every first-party project file (every `*.csproj` outside `packages\` and `.claude\`) and record it in FEATURE/evidence/baseline/analyzer-alignment.md.
  - Command: `pwsh -NoProfile -Command 'PREFIX; $root = (Get-Location).Path; $rootLen = $root.TrimEnd([char]92).Length; $projs = @(Get-ChildItem -Path $root -Recurse -Filter "*.csproj" | Where-Object { $rel = $_.FullName.Substring($rootLen); $rel -notlike "\packages\*" -and $rel -notlike "\.claude\*" }); "PROJECTS=$($projs.Count)"; $missing = 0; $skew = 0; foreach ($p in $projs) { $dir = $p.DirectoryName; [xml]$x = Get-Content -LiteralPath $p.FullName -Raw; foreach ($a in @($x.SelectNodes("//*[local-name()=""Analyzer""]"))) { $inc = $a.GetAttribute("Include"); if (-not (Test-Path -LiteralPath (Join-Path $dir $inc))) { $missing++; "MISSING " + $p.FullName.Substring($rootLen) + " :: " + $inc } }; $pc = Join-Path $dir "packages.config"; if (Test-Path -LiteralPath $pc) { [xml]$c = Get-Content -LiteralPath $pc -Raw; foreach ($id in @("Meziantou.Analyzer", "Roslynator.Analyzers")) { $pin = @($c.SelectNodes("//package[@id=""$id""]") | ForEach-Object { $_.GetAttribute("version") }); $inc = @($x.SelectNodes("//*[local-name()=""Analyzer""]") | ForEach-Object { $_.GetAttribute("Include") } | Where-Object { $_.Contains("\$id.") }); foreach ($i in $inc) { if ($pin.Count -eq 0 -or -not $i.Contains("\$id." + $pin[0] + "\")) { $skew++; "SKEW " + $p.FullName.Substring($rootLen) + " :: " + $i } } } } }; "ANALYZER_MISSING=$missing"; "VERSION_SKEW=$skew"'`.
  - Acceptance: `WORKTREE-LEAF: agent-a291a7fbabf9d0229`, `PROJECTS=` at least 1, `ANALYZER_MISSING=0` and `VERSION_SKEW=0`. A non-zero value is `ANALYZER PATH SKEW`: record every `MISSING` and `SKEW` line and stop; it is an environment defect, not a plan defect, and no version number is asserted here because pins move.
- [x] [P0-T8] Provision the dotnet-coverage global tool (guarded) and record it in FEATURE/evidence/baseline/bootstrap-dotnet-coverage.md.
  - Command: `pwsh -NoProfile -Command 'if (-not (Get-Command dotnet-coverage -ErrorAction SilentlyContinue)) { dotnet tool install --global dotnet-coverage }; "DOTNET_COVERAGE_RESOLVED=$($null -ne (Get-Command dotnet-coverage -ErrorAction SilentlyContinue))"; dotnet-coverage --version'` (no PREFIX: the command touches no repository path; `WORKTREE-LEAF:` is recorded as `not applicable`).
  - Acceptance: `DOTNET_COVERAGE_RESOLVED=True`, a version line is printed, `EXIT_CODE: 0`.
- [x] [P0-T9] Capture the read-only formatter baseline with `dotnet tool run csharpier check .` and record it in FEATURE/evidence/baseline/csharpier-check-baseline.md.
  - Command: `pwsh -NoProfile -Command 'PREFIX; dotnet tool run csharpier check .; "CSHARPIER_EXIT_CODE: $LASTEXITCODE"'`.
  - Acceptance: `WORKTREE-LEAF: agent-a291a7fbabf9d0229`; `EXIT_CODE:` is the printed `CSHARPIER_EXIT_CODE:` value; the success-case line beginning `Checked ` and ending `ms.` (observed in the #950 baseline as `Checked 1637 files in 5251ms.`) is transcribed; `EXIT_CODE: 0` is the gate. A non-zero value lists every reported path and is `FORMAT BASELINE NOT CLEAN`: stop, because the CLAUDE.md format step would then rewrite files outside the Write Set and the decision belongs to the orchestrator.
- [x] [P0-T10] Capture the analyzer baseline with `CMD-REBUILD` (analyzer GATEARGS, `TASKID` p0-t10) against WORKTREE/TaskMaster.sln and record it in FEATURE/evidence/baseline/msbuild-analyzer-baseline.md.
  - Acceptance, all required: `WORKTREE-LEAF: agent-a291a7fbabf9d0229`; `EXIT_CODE: 0`; `ERRORS: 0`; `SKIP_CORECOMPILE_LINES: 0`; `CSC_OUT_QUICKFILER:` and `CSC_OUT_QUICKFILER_TEST:` each at least 1; `TEST_DLL_EXISTS: True`; `UCS_TEST_DLL_EXISTS: True`; `WARNINGS:` recorded as `ANALYZER-BASELINE-WARNINGS:`; `WRITESET_DIAGNOSTIC_LINES:` and `WRITESET_DIAGNOSTIC_CODES:` recorded as `ANALYZER-BASELINE-WRITESET-LINES:` and `ANALYZER-BASELINE-WRITESET-CODES:` (the comparison basis for P8-T3). A non-zero exit is `ANALYZER BASELINE NOT CLEAN`: stop.
- [x] [P0-T11] Capture the nullable baseline with `CMD-REBUILD` (GATEARGS `/p:TreatWarningsAsErrors=true`, no Nullable property override, `TASKID` p0-t11) against WORKTREE/TaskMaster.sln and record it in FEATURE/evidence/baseline/msbuild-nullable-baseline.md.
  - Acceptance, all required: `WORKTREE-LEAF: agent-a291a7fbabf9d0229`; `EXIT_CODE: 0`; `ERRORS: 0`; `SKIP_CORECOMPILE_LINES: 0`; both `CSC_OUT_` counts at least 1; both `_DLL_EXISTS:` values `True`. A non-zero exit is `NULLABLE BASELINE NOT CLEAN`: stop.
- [x] [P0-T12] Record the pre-change census of the four existing #968 Write Set code files in FEATURE/evidence/baseline/census-baseline.md, using `CMD-LINECOUNT` and `CMD-HASH` on CS4, `CMD-CENSUS`, `CMD-TOKEN-COUNT` once per CS4 file and `CMD-SPAN-TOKEN-COUNT` for `R4SPAN`, `R4HEAD`, `R4TAIL`, `ENSURE` and `SCOPE`.
  - Token lists: FIX `"_pinCount", "_fixtureInstalledParked", "lock (FieldLock)", "CompareExchange(", "return new EnsureScope(", "leaks exactly", "A scope that installed nothing", "pins for the process lifetime", "install-ownership flag", "installed nothing carries"`; FAT `"EnsureUiThreadDispatcher", "private static Mock<IItemViewer> BuildExecutingViewer", "QfcItemControllerTestSupport.BuildExecutingViewer()", "BuildExecutingViewer", "absorbs the delegate without running it", "shared UiThread static is irrelevant", "absorbs the queued application", "[TestMethod]"`; TS `"Becomes moot", "leaks exactly", "still delegate to a callee", "not reachable from another test file", "remaining legitimate", "QfcItemController_UiThreadDispatcherPinCountTests", "internal static void EnsureSynchronizationContext()", "UiThreadDispatcherFixture.EnsureDispatcher();"`; FT `"no other class may dispose", "removed that pin: the fixture now counts pins", "(W5) must not latch", "[Timeout(GateTimeoutMs)]", "private const int GateTimeoutMs = 60000;", "EnsureUiThreadDispatcher()", "issue #230 lost update", "the waiter cannot observe the pre-restore value", "[TestMethod]"`; `R4SPAN` `"EnsureUiThreadDispatcher()", "using (", "transactionA.Dispose();", "finally", ".BeSameAs(", ".NotBeSameAs(", "issue #230 lost update"`; `R4HEAD` `"EnsureUiThreadDispatcher()", "using (", "try"`; `R4TAIL` `"}", "finally", "transactionA.Dispose();"`; `ENSURE` `"_pinCount++", "_fixtureInstalledParked = true;", "lock (FieldLock)", "return new EnsureScope("`; `SCOPE` `"CompareExchange(", "lock (FieldLock)", "_pinCount--", "_fixtureInstalledParked = false;", "DispatcherField.SetValue(null, null);"`.
  - Acceptance (each value is derived in facts 1 to 5 and is a falsifiable pre-change observation): `WORKTREE-LEAF: agent-a291a7fbabf9d0229`; LINES 342, 497, 440, 470 in CS4 order; four HASH values recorded as `BASE-HASH:` lines; CMD-CENSUS `PRIMARY_LINES: 9` with PRIMARY-FILE fixture 1, test support 2, fixture tests 4, focus-and-theme 2, `CROSS_LINES: 20` with CROSS-FILE fixture 5, test support 2, InitializationTests.Part2 1, fixture tests 10, focus-and-theme 2, and `CONTROL_LINES: 23`; FIX tokens 0, 0, 4, 3, 2, 1, 1, 0, 0, 0 (the last, `installed nothing carries`, is 0 because the phrase wraps across lines 245 and 246: a vacuous baseline, recorded as such); FAT tokens 2, 1, 0, 9, 0, 0, 0, 17; TS tokens 1, 1, 1, 1, 0, 0, 1, 1; FT tokens 1, 0, 1, 8, 1, 4, 1, 1, 8; `R4SPAN` 1, 2, 1, 2, 1, 1, 1 with `SPAN: 212-284`; `R4HEAD` 1, 1, 1 with `SPAN: 212-224`; `R4TAIL` 3, 1, 0 with `SPAN: 267-273`; `ENSURE` 0, 0, 1, 2 with `SPAN: 122-145`; `SCOPE` 1, 0, 0, 0, 0 with `SPAN: 249-283`. Any differing value is `CENSUS MISMATCH`: record and stop, because a later gate is defined against these values. The non-zero counts (the two focus-and-theme ensure calls, the private helper, the four stale doc tokens, the R4 pin) are the positive controls for the zero gates of P6-T2 and P7-T1.
- [x] [P0-T13] Record the pre-change census of the six existing folded-scope files and the project file in FEATURE/evidence/baseline/fold-census-baseline.md, using `CMD-LINECOUNT` and `CMD-HASH` on FOLD6, `CMD-TOKEN-COUNT` once per FOLD6 file and on PROJ, and `CMD-SPAN-TOKEN-COUNT` for `T1-LIVE`, `HELD`, `T-SIB` and `GATE-LAMBDA`.
  - Token lists: LIV `"class SynchronousBackgroundWorker", "StartSynchronously", "SynchronousBackgroundWorker.StartSynchronously", "new SynchronousBackgroundWorker()", "using (var worker = new SynchronousBackgroundWorker())", "StartHeldOpenLoader(", "Task.Yield", "fake.Advance", "FakeTimeProvider", "using QuickFiler.Test.TestSupport;", "NoSynchronizationContext", "Duplicated per file", "new ArmingFakeTimeProvider()", "[TestMethod]"`; TD `"class SynchronousBackgroundWorker", "StartSynchronously", "SynchronousBackgroundWorker.StartSynchronously", "using (var worker = new SynchronousBackgroundWorker())", "Duplicated per file", "using QuickFiler.Test.TestSupport;", "[TestMethod]"`; ZB `"class SynchronousBackgroundWorker", "StartSynchronously", "SynchronousBackgroundWorker.StartSynchronously", "new SynchronousBackgroundWorker()", "using (var worker = new SynchronousBackgroundWorker())", "InitEmailQueue(0, new", "InitEmailQueue(2, new", "Duplicated per file", "through the nested", "starting a real", "using QuickFiler.Test.TestSupport;", "[TestMethod]"`; DMT `"new BackgroundWorker()", "using (var worker = new BackgroundWorker())", "new ArmingFakeTimeProvider()", "clock.ReArm();", "await Task.WhenAny(clock.Armed, pending)", "must keep polling while the worker can still add candidates", "the gate re-armed instead of returning", "await Task.Yield();", "fake.Advance", "FakeTimeProvider", "using QuickFiler.Test.TestSupport;", "[TestMethod]"`; QDM `"Worker_RunWorkerCompleted", "nameof(LoadRemainingEmailsToQueue)", "nameof(LoadRemainingEmailsToQueueAsync)} Error.", "nameof(LoadRemainingEmailsToQueueAsync)} Task cancelled", "LoadRemainingEmailsToQueueAsync(", "LoadRemainingEmailsToQueue(BackgroundWorker bw", "log4net.ILog log =", "log4net.ILog logger =", "Linked List Locking", "#pragma", "#region", "#endregion", "[ExcludeFromCodeCoverage]", "//e.Result =", "//_blockingQueue = null;", "//worker.RunWorkerCompleted", "ForEachAwaitWithCancellationAsync", ": IQfcDatamodel", "RemainingEmailLoader = LoadRemainingEmailsToQueueAsync;", "WorkerStarter(worker);", "_remainingLoadActive = "`; QQP `"RunWorkerAsync", "written on the worker thread and read", "written on the worker thread", "(:31-66)", "TryUnhookOrReplace", "WorkerStarter", "share no other fence", "honest producer-liveness signal", "() => _remainingLoadActive,", "() => false,", "private volatile bool _remainingLoadActive;"`; PROJ `"<Compile Include=", "Controllers\QfcItemController.UiThreadDispatcherPinCountTests.cs", "TestSupport\SynchronousBackgroundWorker.cs", "TestSupport\ArmingFakeTimeProvider.cs", "TestSupport\DedicatedWorkerThread.cs", "TestSupport\WinFormsPumpHostTests.cs"`; `T1-LIVE` `"await", "using (NoSynchronizationContext())", "Task.Yield", "fake.Advance", "for (int i", "clock.ReArm();", "(await pending)"`; `HELD` `"new SynchronousBackgroundWorker()", "SynchronousBackgroundWorker worker,", "SynchronousBackgroundWorker.StartSynchronously"`; `T-SIB` `"await Task.Yield();", "clock.ReArm();", "await Task.WhenAny(clock.Armed, pending)", "using (var worker = new BackgroundWorker())", "IList<MailItem> result = await pending;"`; `GATE-LAMBDA` `"() => _remainingLoadActive,", "() => false,"`.
  - Acceptance (facts 14 and 16 to 20; falsifiable pre-change observations): `WORKTREE-LEAF: agent-a291a7fbabf9d0229`; LINES 312, 244, 232, 371, 495, 413 in FOLD6 order; six HASH values recorded as `BASE-HASH:` lines; LIV tokens 1, 3, 0, 2, 0, 4, 3, 3, 1, 0, 0, 0, 0, 4 (the ninth, `FakeTimeProvider`, is line 114 only: the `using Microsoft.Extensions.Time.Testing;` directive does not contain the token); TD tokens 1, 2, 0, 1, 1, 0, 5; ZB tokens 1, 4, 0, 3, 0, 1, 1, 1, 1, 1, 0, 3; DMT tokens 2, 0, 0, 0, 0, 1, 0, 1, 4, 5, 0, 9 (the tenth, `FakeTimeProvider`, is lines 99, 216, 224, 249 and 258; the directive at line 9 does not contain the token); QDM tokens 2, 4, 0, 1, 4, 1, 1, 1, 2, 2, 7, 7, 1, 2, 1, 1, 2, 1, 2, 2, 3; QQP tokens 1, 1, 2, 1, 3, 0, 0, 1, 1, 0, 1; PROJ tokens recorded as `PROJ-COMPILE-ITEMS-BASE:` for the first and 0, 0, 0, 1, 1 for the rest; `T1-LIVE` 5, 0, 3, 3, 1, 0, 1 with `SPAN: 110-165` (the last value is LIV line 163, `(await pending).Should().BeEmpty();`); `HELD` 1, 0, 0 with `SPAN: 183-217`; `T-SIB` 1, 0, 0, 0, 1 with `SPAN: 96-133`; `GATE-LAMBDA` 1, 0 with `SPAN: 299-310` (each printed end is the END anchor line minus one, as for R4SPAN). Any differing value is `FOLD CENSUS MISMATCH`: record and stop. The non-zero counts (three nested classes, the old-shape `Task.Yield` and retry loop, the four legacy members, the stale comment tokens) are the positive controls for the zero gates of P4-T9, P5-T5, P5-T12 and P6-T2.
- [x] [P0-T14] Capture the pre-change concurrent run of QfcItemController_UiThreadDispatcherFixtureTests and QfcItemController_FocusAndThemeTests from QuickFiler.Test/bin/Debug/QuickFiler.Test.dll with `CMD-VSTEST` (`FILTER-BASELINE-CONCURRENT`, `TASKID` p0-t14, empty `NAMES`) and record it in FEATURE/evidence/baseline/concurrent-set-baseline.md.
  - Acceptance: `WORKTREE-LEAF: agent-a291a7fbabf9d0229`; `TRX_PRESENT: True`; `SEQUENCE_FILES: 0`; `RESULT_COUNT: 25`; the `COUNTERS` line recorded as `BASELINE-CONCURRENT-COUNTERS:`; every `RESULT` line transcribed; `BASELINE-CONCURRENT-FAILED:` lists every non-Passed name with its `MESSAGE` line, or `NONE` (the comparison basis for P6-T7). Outcomes are observations and are not gated; `ExpectedExitCode:` carries the observed value when non-zero. A `Timeout` or `Aborted` outcome or a Sequence file is `BASELINE HANG`: stop.
- [x] [P0-T15] Capture the pre-change run of the four datamodel test classes from QuickFiler.Test/bin/Debug/QuickFiler.Test.dll with `CMD-VSTEST` (`FILTER-DATAMODEL`, `TASKID` p0-t15, empty `NAMES`) and record it in FEATURE/evidence/baseline/datamodel-set-baseline.md.
  - Acceptance: `WORKTREE-LEAF: agent-a291a7fbabf9d0229`; `TRX_PRESENT: True`; `SEQUENCE_FILES: 0`; `RESULT_COUNT: 21`; the `COUNTERS` line recorded as `BASELINE-DATAMODEL-COUNTERS:`; every `RESULT` line transcribed; `BASELINE-DATAMODEL-FAILED:` lists every non-Passed name with its `MESSAGE` line, or `NONE` (the comparison basis for P4-T11 and P6-T8). Outcomes are observations and are not gated; `ExpectedExitCode:` carries the observed value when non-zero. A `Timeout` or `Aborted` outcome or a Sequence file is `BASELINE HANG`: stop, and the two NAMES-LIVENESS tests are the first suspects (addendum section 5.3).
- [x] [P0-T16] Run the stall probe from UtilitiesCS.Test/bin/Debug/UtilitiesCS.Test.dll with `CMD-VSTEST` (`ASSEMBLY` the UtilitiesCS.Test assembly, `FILTER-STALL`, `TASKID` p0-t16, empty `NAMES`) and record FEATURE/evidence/baseline/stall-probe.md.
  - Acceptance: the artifact records `WORKTREE-LEAF:`, `EXIT_CODE:`, `ExpectedExitCode:` equal to the observed value when non-zero (presentational), `TRX_PRESENT:`, `SEQUENCE_FILES:`, the `COUNTERS` line when present and every `MESSAGE` line; then exactly one `STALL-PROBE:` line — `CLEAR` when `EXIT_CODE: 0`, `failed=0` and `SEQUENCE_FILES: 0`, otherwise `REPRODUCES` — and exactly one `COVERAGE-ROUTE:` line — `RUNNER` under CLEAR, `DIRECT` under REPRODUCES. The probe runs once and is never re-run. Both values complete this task.
- [x] [P0-T17] Capture the baseline repository-wide test-and-coverage run by the route P0-T16 fixed and record FEATURE/evidence/baseline/coverage-summary.md and FEATURE/evidence/baseline/coverage-jacoco-projection.md: under RUNNER run `CMD-COVERAGE-RUNNER` with `STAGE` baseline, under DIRECT run `CMD-COVERAGE-DIRECT` with `STAGE` baseline; then, unless branch (d) applies, run `CMD-COVERAGE-POST` with `STAGE` baseline and `RAW` per its rule.
  - Artifacts: coverage-summary.md carries `Timestamp:`, `Command:` (both payloads, with the route's canonical command), `EXIT_CODE:` (`RUNNER_EXIT_CODE:` or `COLLECT_EXIT_CODE:`), `ExpectedExitCode:` equal to the observed value when non-zero (a baseline observation), and an `Output Summary:` recording `WORKTREE-LEAF:`, `COVERAGE-ROUTE:`, `RAW:`, `DISCOVERED_LINE:` or `ASSEMBLY_COUNT:` with every `ASSEMBLY:` line, `TRX_PRESENT:`, `SEQUENCE_FILES:` (DIRECT), `THRESHOLD_MESSAGE:` and `COLLECT_FAILURE_MESSAGE:` (RUNNER), `LINE-FLOOR:`, `BRANCH-FLOOR:`, the `First-party coverage:` line (the numeric baseline headline: lines covered over valid with percentage, branches likewise), the `ROOT` line, the five summary lines verbatim between `SUMMARY-BEGIN` and `SUMMARY-END`, `FAILED-SET:` recorded as `BASELINE-FAILED-SET:`, every `MESSAGE` line (names redacted of paths), the `RESULT` lines (twelve at baseline: the pin-count tests do not exist yet), `TEST_ASSEMBLY_PACKAGES:`, `QFCDATAMODEL_CLASS_ENTRIES:` (recorded) and `CHANGED-CODE-COVERAGE: NOT MEASURED (TEST ASSEMBLY EXCLUDED; QFCDATAMODEL EXCLUDED BY ATTRIBUTE)`; coverage-jacoco-projection.md carries `Timestamp:`, a `Source:` line naming coverage-summary.md, and the projection verbatim between `PROJECTION-BEGIN` and `PROJECTION-END`.
  - Branches, checked in order: (d) `TRX_PRESENT: False`, `SEQUENCE_FILES:` greater than 0, or a non-zero exit with an empty `FAILED-SET:` and no floor message, is `COVERAGE RUN ABORTED`: stop, report the last lines of the log with paths redacted, do not re-run. (c) `TEST_ASSEMBLY_PACKAGES:` other than 0 is `TEST ASSEMBLY INSTRUMENTED`: stop, because D-7 rests on the derived exclusion. (b) a non-zero exit with a non-empty `FAILED-SET:` or a floor `NOT MET` is recorded as `BASELINE-STATE: PRE-EXISTING FAILURES` (with `BASELINE-FLOOR:` naming any floor not met) and completes this task (D-8). (a) exit 0 with both floors met is `BASELINE-STATE: GREEN` and completes this task.
- [x] [P0-T18] Write the baseline toolchain index FEATURE/evidence/baseline/toolchain-baseline.md from the P0-T9, P0-T10, P0-T11 and P0-T17 artifacts.
  - Acceptance: `Timestamp:`; one row per step in order — csharpier check, analyzer rebuild, TreatWarningsAsErrors rebuild, coverage run (route) — each with its canonical command, `EXIT_CODE:` copied from the step artifact, and the step artifact's path; `BASELINE-STATE:` copied from P0-T17; the `First-party coverage:` line copied from P0-T17. This file is an index over the per-step artifacts, not a substitute for them.
- [x] [P0-T19] Commit the Phase 0 evidence (FEATURE only) and record it in FEATURE/evidence/baseline/phase0-commit.md.
  - Commands, separate Bash calls: `git -C WORKTREE add -- docs/features/active/2026-10-02-focus-and-theme-tests-leak-shared-dispatcher-setup-968`; `git -C WORKTREE commit -m "docs(968): record phase 0 baseline evidence" -- docs/features/active/2026-10-02-focus-and-theme-tests-leak-shared-dispatcher-setup-968`; `git -C WORKTREE rev-parse HEAD`; `git -C WORKTREE show --name-status --format= HEAD`; `git -C WORKTREE status --porcelain --untracked-files=all`.
  - Acceptance: both git writes exit 0; `PHASE0-COMMIT:` records the new HEAD as an observation; `PHASE0-COMMIT-PATHS:` lists the `show` output and every path is under FEATURE (the pathspec-limited commit cannot carry anything else; any other path is `COMMIT SWEPT FOREIGN PATH`: stop); no porcelain line names a path under FEATURE other than this plan file (whose check-off mark is written after the commit) and FEATURE/evidence/baseline/phase0-commit.md (written after the commit), and no porcelain line names a path under QuickFiler/ or QuickFiler.Test/. This artifact itself is committed in P6-T9.

### Phase 1 — Regression Test First (fail-before on the unmodified fixture)

- [x] [P1-T1] Create QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherPinCountTests.cs with the whole content of Delivered Source N1 (Write tool, absolute path, the Markdown indent removed), then normalise its line endings with `CMD-EOL` (`FILE` PC).
  - Acceptance (CMD-EOL output recorded by P1-T3): `WORKTREE-LEAF: agent-a291a7fbabf9d0229`; `BARE_LF: 0`; `CRLF_COUNT:` equals `LINES:`; `LINES:` at most 500 and at least 200.
- [x] [P1-T2] Insert Delivered Source T1 into QuickFiler.Test/QuickFiler.Test.csproj immediately after line 203 (`<Compile Include="Controllers\QfcItemController.UiThreadDispatcherFixtureTests.cs" />`), as an in-place Edit.
  - Acceptance (recorded by P1-T3): exactly one line of the project file contains `Controllers\QfcItemController.UiThreadDispatcherPinCountTests.cs`, it begins with four spaces and `<Compile Include=`, and the line before it is the fixture-tests item.
- [x] [P1-T3] Record the new-file census in FEATURE/evidence/regression-testing/pin-count-file-census.md with the P1-T1 CMD-EOL output, `CMD-TOKEN-COUNT` on PC (TOKENS `"EnsureUiThreadDispatcher()", "Regression test: fails before the fix", "Specification test: passes before and after the fix", "never read the shared static", "[TestClass]", "[TestMethod]", "[Timeout(GateTimeoutMs)]", "private const int GateTimeoutMs = 60000;", "transaction.Install(null);", "transaction.Install(live);", "transaction.Dispose();", "QfcItemControllerTestSupport.ShutdownDispatcher(live);", "a holder that did not take the last pin must not lose the dispatcher", "the last release reverts the fixture", "using Moq;", "Thread.Sleep", "Task.Delay", "public class QfcItemController_UiThreadDispatcherPinCountTests", "foreignTransaction.Install(parked);"`), `CMD-TOKEN-COUNT` on PROJ (TOKENS `"Controllers\QfcItemController.UiThreadDispatcherPinCountTests.cs", "Controllers\QfcItemController.UiThreadDispatcherFixtureTests.cs"`), `git -C WORKTREE diff --numstat HEAD -- QuickFiler.Test/QuickFiler.Test.csproj` paired with `git -C WORKTREE status --porcelain -- QuickFiler QuickFiler.Test`.
  - Acceptance: PC tokens 10, 1, 3, 1, 1, 4, 4, 1, 3, 1, 4, 1, 2, 2, 0, 0, 0, 1, 1; PROJ tokens 1 and 1; numstat for the project file reads `1	0`; the porcelain span lists exactly ` M QuickFiler.Test/QuickFiler.Test.csproj` and `?? QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherPinCountTests.cs` (recorded as `PHASE1-PORCELAIN:`, the comparison basis for P2-T8). Any other value: correct the edit and re-run this task.
- [x] [P1-T4] Build the tree with the new test file compiled against the unmodified fixture using `CMD-BUILD` (`TASKID` p1-t4) against WORKTREE/TaskMaster.sln and record it in FEATURE/evidence/regression-testing/fail-before-build.md.
  - Acceptance: `WORKTREE-LEAF: agent-a291a7fbabf9d0229`, `EXIT_CODE: 0`, `ERRORS: 0`, `TEST_DLL_ADVANCED: True`, `CSC_OUT_QUICKFILER_TEST:` at least 1. A compile error in the new file is a defect in N1: correct it within the delivered design, re-run P1-T3 and this task.
- [x] [P1-T5] [expect-fail] Run the regression test alone against the unmodified fixture from QuickFiler.Test/bin/Debug/QuickFiler.Test.dll with `CMD-VSTEST` (`FILTER-PC-T1`, `TASKID` p1-t5, `NAMES` `"EnsureDispatcher_TwoPinsHeld_ReleasingTheFirstKeepsTheDispatcherUntilTheLastRelease"`) and record FEATURE/evidence/regression-testing/fail-before-pin-count.md.
  - Acceptance: `WORKTREE-LEAF: agent-a291a7fbabf9d0229`; `EXIT_CODE: 1`; `ExpectedExitCode: 1`; `TRX_PRESENT: True`; `SEQUENCE_FILES: 0`; `RESULT_COUNT: 1`; the `RESULT` line reads `Failed` with its duration recorded; the `MESSAGE` line contains `to refer to`, `ParkedDispatcher`, `a holder that did not take the last pin must not lose the dispatcher` and `but found <null>` (fact 13: the first-release assertion failed because the first pin's release nulled the field). The artifact states that the fixture is at BASE content (P0-T12 `BASE-HASH:` for FIX is re-derived with `CMD-HASH` on `"FIX"` in this task and must match), that the test ran alone so the baseline was null (fact 7), and that this run is the fail-before half of AC5. A `Passed` result is `REGRESSION DID NOT FAIL`: stop and report, because the root-cause claim rests on it. A `Failed` result whose `MESSAGE` lacks `to refer to` or the because text (for example an exception, or the `NotBeNull` because text `the first pin seeds the parked dispatcher into a null field`) is `FAIL-BEFORE WRONG REASON`: stop and report, because the test is then defective rather than the fixture.
- [x] [P1-T6] Run the three specification tests against the unmodified fixture from QuickFiler.Test/bin/Debug/QuickFiler.Test.dll with `CMD-VSTEST` (`FILTER-PC-T234`, `TASKID` p1-t6, `NAMES` the last three names of `NAMES-PC`) and record FEATURE/evidence/regression-testing/specification-tests-before-fix.md.
  - Acceptance: `WORKTREE-LEAF: agent-a291a7fbabf9d0229`; `EXIT_CODE: 0`; `TRX_PRESENT: True`; `SEQUENCE_FILES: 0`; `RESULT_COUNT: 3`; three `RESULT` lines, one per name, each `Passed` with duration recorded (the "passes before and after the fix" half of the labels AC6 requires; test 4's second transaction installs the captured parked instance and its pin installs nothing, so it passes on the unmodified fixture too). Any `Failed` is `SPECIFICATION TEST FAILS BEFORE FIX`: stop and report, because the spec's design trace for that test is then wrong.

### Phase 2 — Fixture Fix (reference-counted pin and D1 documentation)

- [x] [P2-T1] Insert Delivered Source F-FIELDS into QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixture.cs after line 38 (`private static Dispatcher _parkedDispatcher = null;`), as an in-place Edit.
  - Acceptance (recorded by P2-T6): one line contains `private static int _pinCount;` and one contains `private static bool _fixtureInstalledParked;`, both inside the class's static field block above the issue #743 counters; the inserted comment contains `only while FieldLock is held` and does not contain `lock (FieldLock)`.
- [x] [P2-T2] Insert Delivered Source F-CLASSDOC into QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixture.cs after pre-edit line 30 (`/// </para>` closing the design-note paragraph, now shifted by P2-T1 only if P2-T1 is applied first; apply this task by content, locating the `/// </para>` line that immediately precedes the class `/// </summary>`).
  - Acceptance (recorded by P2-T6): `install-ownership flag` 1 and `pins for the process lifetime` at least 1 in the file; the class doc ends with the new paragraph followed by `/// </summary>`.
- [x] [P2-T3] Replace the `EnsureDispatcher` summary (pre-edit lines 116 to 121, located by content: from the `/// <summary>` immediately above `internal static IDisposable EnsureDispatcher()` to the `/// </summary>` immediately above it) in QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixture.cs with Delivered Source F-ENSURE-DOC.
  - Acceptance (recorded by P2-T6): `leaks exactly` 0; `pins for the process lifetime` 2 in the file.
- [x] [P2-T4] Replace the `EnsureDispatcher` body (pre-edit lines 128 to 137, located by content: from the `lock (FieldLock)` line after `Dispatcher parked = GetParkedDispatcher();` through `return new EnsureScope(null);`) in QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixture.cs with Delivered Source F-ENSURE-BODY.
  - Acceptance (recorded by P2-T6): within `ENSURE`, `_pinCount++` 1, `_fixtureInstalledParked = true;` 1, `lock (FieldLock)` 1, `return new EnsureScope(` 1.
- [x] [P2-T5] Replace the `EnsureScope` documentation and class (pre-edit lines 243 to 274, located by content: from the `/// <summary>` immediately above `private sealed class EnsureScope : IDisposable` through the class's closing brace, the `}` immediately before the fixture class's closing `}`) in QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixture.cs with Delivered Source F-SCOPE.
  - Acceptance (recorded by P2-T6): within `SCOPE`, `CompareExchange(` 0, `lock (FieldLock)` 1, `_pinCount--` 1, `_fixtureInstalledParked = false;` 1, `DispatcherField.SetValue(null, null);` 1; `A scope that installed nothing` 0 in the file.
- [x] [P2-T6] Record the fixture-change census in FEATURE/evidence/qa-gates/fixture-change-census.md with `CMD-LINECOUNT` on `"FIX"`, `CMD-TOKEN-COUNT` on FIX (the P0-T12 FIX token list), `CMD-SPAN-TOKEN-COUNT` on `ENSURE` and `SCOPE` (the P0-T12 token lists), `git -C WORKTREE diff --numstat HEAD -- QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixture.cs`, `git -C WORKTREE diff HEAD -- QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixture.cs`, paired with `git -C WORKTREE status --porcelain -- QuickFiler QuickFiler.Test`.
  - Acceptance: `WORKTREE-LEAF: agent-a291a7fbabf9d0229`; FIX LINES 375; FIX tokens 4, 4, 5, 2, 1, 0, 0, 2, 1, 0 (in the P0-T12 order: `_pinCount`, `_fixtureInstalledParked`, `lock (FieldLock)`, `CompareExchange(`, `return new EnsureScope(`, `leaks exactly`, `A scope that installed nothing`, `pins for the process lifetime`, `install-ownership flag`, `installed nothing carries`); `ENSURE` 1, 1, 1, 1; `SCOPE` 0, 1, 1, 1, 1; the porcelain span lists exactly ` M QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixture.cs`, ` M QuickFiler.Test/QuickFiler.Test.csproj` and `?? QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherPinCountTests.cs`; and a `FIELDLOCK-ENCLOSURE:` section that, reading the transcribed diff, names the two `lock (FieldLock)` blocks (in `EnsureDispatcher` and in `EnsureScope.Dispose`) and states that the three non-declaration occurrences of each new field lie inside them and that no `CompareExchange` call appears in the scope class (the AC9 reading). Any other count: correct the edit and re-run this task.
- [x] [P2-T7] Build the fixed fixture with `CMD-BUILD` (`TASKID` p2-t7) against WORKTREE/TaskMaster.sln and record it in FEATURE/evidence/regression-testing/pass-after-build.md.
  - Acceptance: `WORKTREE-LEAF: agent-a291a7fbabf9d0229`, `EXIT_CODE: 0`, `ERRORS: 0`, `TEST_DLL_ADVANCED: True`, `CSC_OUT_QUICKFILER_TEST:` at least 1.
- [x] [P2-T8] Run the four pin-count tests against the fixed fixture from QuickFiler.Test/bin/Debug/QuickFiler.Test.dll with `CMD-VSTEST` (`FILTER-PC-CLASS`, `TASKID` p2-t8, `NAMES-PC`) and record FEATURE/evidence/regression-testing/pass-after-pin-count.md, together with `git -C WORKTREE status --porcelain -- QuickFiler QuickFiler.Test`.
  - Acceptance: `WORKTREE-LEAF: agent-a291a7fbabf9d0229`; `EXIT_CODE: 0`; `TRX_PRESENT: True`; `SEQUENCE_FILES: 0`; `RESULT_COUNT: 4`; four `RESULT` lines, one per NAMES-PC name, each `Passed` with duration recorded; the porcelain span (`PHASE2-PORCELAIN:`) equals `PHASE1-PORCELAIN:` from P1-T3 plus exactly one extra line ` M QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixture.cs`, recorded as the AC5 statement that the only difference between the P1-T5 run and this run is the fixture file. A `Failed` regression test is `FIX DID NOT TAKE`: correct the fixture within D-1 and re-run from P2-T6.

### Phase 3 — Theme-Test Deletions, R4 Restructure and D2 to D6

- [x] [P3-T1] Apply Delivered Source A1 to QuickFiler.Test/Controllers/QfcItemController.FocusAndThemeTests.cs: delete lines 99 to 116 (the private `BuildExecutingViewer` method and the blank line after it), located by content as the block from `private static Mock<IItemViewer> BuildExecutingViewer()` through its closing `}` plus the following blank line.
  - Acceptance (recorded by P3-T9): `private static Mock<IItemViewer> BuildExecutingViewer` 0; the line after the `BuildFocusController` method's closing `}` and one blank line is the `/// <summary>` of `EnableHandlelessThemeInvoke`.
- [x] [P3-T2] Apply Delivered Source A2 to QuickFiler.Test/Controllers/QfcItemController.FocusAndThemeTests.cs: replace all seven `var viewer = BuildExecutingViewer();` lines with `var viewer = QfcItemControllerTestSupport.BuildExecutingViewer();` and replace the six-line cycle-3 comment block with the seven-line A2 block.
  - Acceptance (recorded by P3-T9): `QfcItemControllerTestSupport.BuildExecutingViewer()` 8 and `BuildExecutingViewer` 8 (every remaining mention is prefixed).
- [x] [P3-T3] Apply Delivered Source A3 to `SetThemeDark_FromNormal_SelectsDarkNormalTheme` in QuickFiler.Test/Controllers/QfcItemController.FocusAndThemeTests.cs: replace the two arrange comment lines and the `QfcItemControllerTestSupport.EnsureUiThreadDispatcher();` line with the five A3 comment lines.
  - Acceptance (recorded by P3-T9): `absorbs the delegate without running it` 1 and `shared UiThread static is irrelevant` 1; the first statement of the test is `var controller = new FocusController();`.
- [x] [P3-T4] Apply Delivered Source A4 to `SetThemeLight_FromNormal_SelectsLightNormalTheme` in QuickFiler.Test/Controllers/QfcItemController.FocusAndThemeTests.cs: replace the `// Arrange` line and the `QfcItemControllerTestSupport.EnsureUiThreadDispatcher();` line with the two A4 comment lines.
  - Acceptance (recorded by P3-T9): `absorbs the queued application` 1; `EnsureUiThreadDispatcher` 0 in the file.
- [x] [P3-T5] Replace the `EnsureUiThreadDispatcher` documentation (lines 216 to 237, located by content: from the `/// <summary>` whose next line begins `/// Ensures the static <c>UiThread.Dispatcher</c>` to the `/// </summary>` immediately above `internal static IDisposable EnsureUiThreadDispatcher() =>`) in QuickFiler.Test/Controllers/QfcItemController.TestSupport.cs with Delivered Source S1.
  - Acceptance (recorded by P3-T9): `Becomes moot` 0, `leaks exactly` 0, `still delegate to a callee` 0, `remaining legitimate` 1, `QfcItemController_UiThreadDispatcherPinCountTests` 1; the declaration and body lines are unchanged (`UiThreadDispatcherFixture.EnsureDispatcher();` 1).
- [x] [P3-T6] Replace the `BuildExecutingViewer` documentation (pre-edit lines 282 to 288, located by content: from the `/// <summary>` whose next line begins `/// Issue #480 shared arrange helper.` to the `/// </summary>` immediately above `internal static Mock<IItemViewer> BuildExecutingViewer()`) in QuickFiler.Test/Controllers/QfcItemController.TestSupport.cs with Delivered Source S2.
  - Acceptance (recorded by P3-T9): `not reachable from another test file` 0; `Issue #480 shared arrange helper` 1.
- [x] [P3-T7] Replace the R4 `<para>` paragraph (lines 196 to 208, located by content: from the `/// <para>` whose next line begins `/// Issue #950: the earlier intermittent failure` to the following `/// </para>`) in QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixtureTests.cs with Delivered Source R-DOC.
  - Acceptance (recorded by P3-T9): `no other class may dispose` 0, `removed that pin: the fixture now counts pins` 1, `(W5) must not latch` 1.
- [x] [P3-T8] Apply Delivered Source R-BODY to `Transaction_SecondCallerCannotInstallUntilTheFirstRestores` in QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixtureTests.cs: replace the four-line `using (` header (lines 221 to 224) with `try` and `{`, and replace the sixteen-space `}` at line 270 with the five-line `}` / `finally` / `{` / `transactionA.Dispose();` / `}` block; lines 225 to 269 are unchanged.
  - Acceptance (recorded by P3-T9): `R4SPAN` `EnsureUiThreadDispatcher()` 0, `using (` 1, `transactionA.Dispose();` 2, `finally` 3, `.BeSameAs(` 1, `.NotBeSameAs(` 1, `issue #230 lost update` 1; `R4HEAD` 0, 0, 2; `R4TAIL` `}` 4, `finally` 2, `transactionA.Dispose();` 1; `[Timeout(GateTimeoutMs)]` 8 and `private const int GateTimeoutMs = 60000;` 1 unchanged.
- [x] [P3-T9] Record the test-edit census in FEATURE/evidence/qa-gates/test-edit-census.md with `CMD-LINECOUNT` on CS5, `CMD-TOKEN-COUNT` on FAT, TS and FT (the P0-T12 lists, the TS list extended with `"Issue #480 shared arrange helper"`), `CMD-SPAN-TOKEN-COUNT` on `R4SPAN`, `R4HEAD` and `R4TAIL` (the P0-T12 lists), `CMD-HUNKS` on `QuickFiler.Test/Controllers/QfcItemController.TestSupport.cs` and on `QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixtureTests.cs`, and `git -C WORKTREE diff --numstat HEAD -- QuickFiler.Test` paired with `git -C WORKTREE status --porcelain -- QuickFiler QuickFiler.Test`.
  - Acceptance: `WORKTREE-LEAF: agent-a291a7fbabf9d0229`; LINES 375, 482, 442, 472 for FIX, FAT, TS, FT and at most 500 for PC; FAT tokens 0, 0, 8, 8, 1, 1, 1, 17; TS tokens 0, 0, 0, 0, 1, 1, 1, 1 then `Issue #480 shared arrange helper` 1; FT tokens 0, 1, 1, 8, 1, 3, 1, 1, 8; `R4SPAN` 0, 1, 2, 3, 1, 1, 1; `R4HEAD` 0, 0, 2; `R4TAIL` 4, 2, 1; TestSupport `HUNK_COUNT: 2` with every `HUNK` old-range start at or above 200 (the `EnsureSynchronizationContext` region 85 to 96 is untouched, AC17); fixture-tests hunks each with old-range start at or above 190 and old-range start plus old-range length at or below 285 (every hunk lies in R4's doc and body, AC10); the porcelain span lists exactly the five `.cs` #968 Write Set paths (four ` M`, one `??`) and ` M QuickFiler.Test/QuickFiler.Test.csproj`. Any other value: correct the edit and re-run this task.

### Phase 4 — Folded Scope A: Shared Worker, Caller-Owned Disposal and Dead-Code Removal (#972 items 1, 3 and 4)

- [x] [P4-T1] Record the zero-caller proof for the four legacy `QfcDatamodel` members against the pre-change tree in FEATURE/evidence/qa-gates/qfc-datamodel-legacy-callers.md with `CMD-LEGACY-CALLERS`, before any Phase 4 edit.
  - Acceptance: `WORKTREE-LEAF: agent-a291a7fbabf9d0229`; `QFCDATAMODEL_LINES: 495` (recorded as `QFCDATAMODEL-LINES-BEFORE:`, the AC28 before figure); `PRIMARY_LINES: 25`; `LOG_LINES: 3`; `CROSS_LINES: 2`; `INTERFACE_LINES: 0`; `SWEEP-CS` lines name exactly the five `.cs` files of fact 15 (`QuickFiler/Controllers/QfcHomeController.cs`, `QuickFiler/Controllers/QfcDatamodel.cs`, `QuickFiler.Test/Controllers/QfcInitEmailQueueZeroBatchTests.cs`, `QuickFiler.Test/Controllers/QfcHomeControllerRunAsyncTests.cs`, `QuickFiler.Test/Controllers/QfcDatamodelLivenessTests.cs`); every `IVT` line is one of the four grants of fact 15. The artifact then classifies every `PRIMARY`, `LOG` and `CROSS` line as `DECLARATION` (inside `QfcDatamodel.cs`), `COMMENTED-OUT`, `DOC-PROSE`, `CREF-ONE-ARG-OVERLOAD` (`QfcDatamodel.cs` 130), `METHOD-GROUP-ONE-ARG-OVERLOAD` (`QfcDatamodel.cs` 40 and 52, the assignment that binds the surviving one-argument overload; fact 14), `REGION-DIRECTIVE` (`QfcDatamodel.cs` 469 and 472, the `#region` and `#endregion` lines of the empty `Linked List Locking` region that P1 removes), `NAMEOF-RETARGETED` (`QfcDatamodel.cs` 369), `SELF-REFERENCE` (a hit inside a removed member's own body), `OTHER-TYPE-SAME-NAME` (`QfcHomeController.cs` 92, 132, 344, 379 and `QfcHomeControllerRunAsyncTests.cs` 325, 376, whose reflective `GetMethod` is invoked on `_controller`, a `QfcHomeController`) or `INVOCATION`; records `INVOCATIONS: 0`; writes the `## Numeric Derivation Evidence` block of the addendum (Complete Family, Exhaustive Search Scope, Inclusion Rules, Exclusion Rules, Primary Search Strategy, Primary Member Set, Primary Count 4, Cross-check Search Strategy, Cross-check Member Set, Cross-check Count 4, Member-set Comparison identical); and states that this is unreachable dead code with no behaviour to regress, so no failing test precedes its removal and the compile proof is the two rebuilds. Any `INVOCATION` classification, or a count other than stated, is `LEGACY MEMBER HAS A CALLER`: stop and report.
- [x] [P4-T2] Create QuickFiler.Test/TestSupport/SynchronousBackgroundWorker.cs with the whole content of Delivered Source W1 (Write tool, absolute path, the Markdown indent removed), then normalise its line endings with `CMD-EOL` (`FILE` SBW).
  - Acceptance (recorded by P4-T9): `WORKTREE-LEAF: agent-a291a7fbabf9d0229`; `BARE_LF: 0`; `CRLF_COUNT:` equals `LINES:`; `LINES:` at most 500 and at least 20.
- [x] [P4-T3] Insert the first T2 line (`<Compile Include="TestSupport\SynchronousBackgroundWorker.cs" />`, four leading spaces) into QuickFiler.Test/QuickFiler.Test.csproj immediately after the line containing `TestSupport\DedicatedWorkerThread.cs`, as an in-place Edit.
  - Acceptance (recorded by P4-T9): exactly one line of the project file contains `TestSupport\SynchronousBackgroundWorker.cs`; the line before it contains `TestSupport\DedicatedWorkerThread.cs` and the line after it contains `TestSupport\WinFormsPumpHostTests.cs`.
- [x] [P4-T4] Apply Delivered Source TD1 to QuickFiler.Test/Controllers/QfcDatamodelTeardownTests.cs (the `using`, the deletion of the nested worker and starter, the starter retarget).
  - Acceptance (recorded by P4-T9): TD tokens `class SynchronousBackgroundWorker` 0, `StartSynchronously` 1, `SynchronousBackgroundWorker.StartSynchronously` 1, `using (var worker = new SynchronousBackgroundWorker())` 1, `Duplicated per file` 0, `using QuickFiler.Test.TestSupport;` 1, `[TestMethod]` 5; LINES 229.
- [x] [P4-T5] Apply Delivered Source Z1, Z2 and Z3 to QuickFiler.Test/Controllers/QfcInitEmailQueueZeroBatchTests.cs (the `using`, the two doc rewordings, the deletion of the nested worker and starter, the three `using` blocks and starter retargets).
  - Acceptance (recorded by P4-T9): ZB tokens `class SynchronousBackgroundWorker` 0, `StartSynchronously` 3, `SynchronousBackgroundWorker.StartSynchronously` 3, `new SynchronousBackgroundWorker()` 3, `using (var worker = new SynchronousBackgroundWorker())` 3, `InitEmailQueue(0, new` 0, `InitEmailQueue(2, new` 0, `Duplicated per file` 0, `through the nested` 0, `starting a real` 0, `using QuickFiler.Test.TestSupport;` 1, `[TestMethod]` 3; LINES at most 500.
- [x] [P4-T6] Apply Delivered Source L1a, L2, L5, L6 and L7 to QuickFiler.Test/Controllers/QfcDatamodelLivenessTests.cs (the added `using`, the deletion of the nested worker and starter, the `StartHeldOpenLoader` signature and body, the three callers' `using` blocks), and in test 1, as an interim edit that L3 replaces in P5-T3, change the line `model.WorkerStarter = StartSynchronously;` to `model.WorkerStarter = SynchronousBackgroundWorker.StartSynchronously;` so the file compiles before the test-1 rewrite.
  - Acceptance (recorded by P4-T9): LIV tokens `class SynchronousBackgroundWorker` 0, `StartSynchronously` 2, `SynchronousBackgroundWorker.StartSynchronously` 2, `new SynchronousBackgroundWorker()` 4, `using (var worker = new SynchronousBackgroundWorker())` 3 (the three callers; test 1 still carries its pre-rewrite `var worker = new SynchronousBackgroundWorker();`), `StartHeldOpenLoader(` 4, `Task.Yield` 3 and `fake.Advance` 3 and `FakeTimeProvider` 1 (all unchanged until P5-T3), `using QuickFiler.Test.TestSupport;` 1, `Duplicated per file` 0, `NoSynchronizationContext` 0, `new ArmingFakeTimeProvider()` 0, `[TestMethod]` 4; `HELD` 0, 1, 1.
- [x] [P4-T7] Apply Delivered Source M1 and M3 to QuickFiler.Test/Controllers/QfcDatamodelTests.cs (the `using`, and the `using` block around the `WaitForQueue` test's worker).
  - Acceptance (recorded by P4-T9): DMT tokens `new BackgroundWorker()` 2, `using (var worker = new BackgroundWorker())` 1 (the sibling test's worker is wrapped by M2 in P5-T4), `using QuickFiler.Test.TestSupport;` 1, `[TestMethod]` 9.
- [x] [P4-T8] Apply Delivered Source P1 to QuickFiler/Controllers/QfcDatamodel.cs (the nine deletions and the one `nameof` retarget, located by the quoted lines).
  - Acceptance (recorded by P4-T9): QDM tokens 0, 0, 1, 0, 2, 0, 0, 1, 0, 0, 6, 6, 1, 0, 0, 0, 0, 1, 2, 2, 3 in the P0-T13 order; LINES 367 and at most 400.
- [x] [P4-T9] Record the fold-edit census in FEATURE/evidence/qa-gates/fold-edit-census.md with the P4-T2 CMD-EOL output, `CMD-LINECOUNT` on FOLD6 plus `"SBW"`, `CMD-TOKEN-COUNT` on LIV, TD, ZB, DMT, QDM and PROJ (the P0-T13 lists) and on SBW (TOKENS `"class SynchronousBackgroundWorker", "internal sealed class SynchronousBackgroundWorker : BackgroundWorker", "internal static void StartSynchronously(BackgroundWorker worker)", "Dispose", "namespace QuickFiler.Test.TestSupport"`), `CMD-SPAN-TOKEN-COUNT` on `HELD`, `CMD-HUNKS` on `QuickFiler/Controllers/QfcDatamodel.cs`, and `git -C WORKTREE diff --numstat HEAD -- QuickFiler QuickFiler.Test` paired with `git -C WORKTREE status --porcelain -- QuickFiler QuickFiler.Test`.
  - Acceptance: `WORKTREE-LEAF: agent-a291a7fbabf9d0229`; the P4-T2 to P4-T8 token and LINES conditions all hold; SBW tokens 1, 1, 1, 1, 1; PROJ tokens `PROJ-COMPILE-ITEMS-BASE:` plus 2, 1, 1, 0, 1, 1; `HUNK_COUNT:` for QfcDatamodel.cs is recorded, not gated (git merges edits separated by at most six unchanged lines, so the nine P1 edits yield fewer hunks); the `--numstat` row for `QuickFiler/Controllers/QfcDatamodel.cs` reads `1	129` (128 removed lines plus the replaced line at old 369, whose replacement is the only added line); the porcelain span lists exactly the five #968 `.cs` paths, ` M QuickFiler.Test/QuickFiler.Test.csproj`, `?? QuickFiler.Test/TestSupport/SynchronousBackgroundWorker.cs`, ` M QuickFiler.Test/Controllers/QfcDatamodelLivenessTests.cs`, ` M QuickFiler.Test/Controllers/QfcDatamodelTeardownTests.cs`, ` M QuickFiler.Test/Controllers/QfcInitEmailQueueZeroBatchTests.cs`, ` M QuickFiler.Test/Controllers/QfcDatamodelTests.cs` and ` M QuickFiler/Controllers/QfcDatamodel.cs` (twelve lines; `QfcDatamodel.QueueProcessing.cs` is untouched until P5-T11). Any other value: correct the edit and re-run this task.
- [x] [P4-T10] Build the tree after the Phase 4 edits with `CMD-BUILD` (`TASKID` p4-t10) against WORKTREE/TaskMaster.sln and record it in FEATURE/evidence/regression-testing/fold-build.md.
  - Acceptance: `WORKTREE-LEAF: agent-a291a7fbabf9d0229`, `EXIT_CODE: 0`, `ERRORS: 0`, `TEST_DLL_ADVANCED: True`, `PROD_DLL_ADVANCED: True`, `CSC_OUT_QUICKFILER:` and `CSC_OUT_QUICKFILER_TEST:` each at least 1. This build is the first compile proof that no surviving code referenced a removed member (a CS0103 or CS0117 naming one of the four is `LEGACY MEMBER HAS A CALLER`: stop). A compile error in a fold test file is corrected within the delivered design, then P4-T9 and this task are re-run.
- [x] [P4-T11] Run the four datamodel classes after the consolidation from QuickFiler.Test/bin/Debug/QuickFiler.Test.dll with `CMD-VSTEST` (`FILTER-DATAMODEL`, `TASKID` p4-t11, empty `NAMES`) and record FEATURE/evidence/regression-testing/datamodel-set-after-consolidation.md.
  - Acceptance: `WORKTREE-LEAF: agent-a291a7fbabf9d0229`; `TRX_PRESENT: True`; `SEQUENCE_FILES: 0`; `RESULT_COUNT: 21`; every `RESULT` line transcribed; `EXIT_CODE: 0` with `passed=21 failed=0`, or every non-Passed name present in P0-T15 `BASELINE-DATAMODEL-FAILED:` (recorded as pre-existing with `ExpectedExitCode:` equal to the observed value). A new failure is `CONSOLIDATION BROKE A DATAMODEL TEST`: correct the fold edit at fault within the delivered design and re-run from P4-T9.

### Phase 5 — Folded Scope B: Deterministic Liveness Tests (#968 comment) and the Producer-Liveness Comment (#972 item 2)

- [x] [P5-T1] Write the fail-before exception dossier FEATURE/evidence/regression-testing/fail-before-exception.<timestamp>.md (timestamp from `Get-Date -Format yyyy-MM-ddTHH-mm` at write time) BEFORE any Phase 5 edit, recording with `CMD-SPAN-TOKEN-COUNT` on `T1-LIVE` and `T-SIB` (the P0-T13 token lists) that the old shapes are still on disk.
  - Acceptance: the artifact carries `Timestamp:`, `Command:` (the two span payloads), `EXIT_CODE: 0`, `Output Summary:` with `T1-LIVE` `Task.Yield` 3, `fake.Advance` 3, `for (int i` 1 and `T-SIB` `await Task.Yield();` 1 (the constructs whose removal AC31 requires), a `WhyFailingRunImpossible:` line stating that the old tests fail only when the thread pool delays a queued continuation past the bounded retry or past the second advance, which no test input can force, and that the production behaviour under test is correct before and after (addendum section 5.5), an `## Alternative proof` section naming (i) the mechanism reading of addendum section 5.3 with the web-verified timer facts of 5.2, and (ii) the labelled sensitivity check of P5-T8 as evidence of the new tests' sensitivity rather than a fail-before of the old ones, and a `SearchScope:` / `SearchPatterns:` / `SearchResult:` triple recording that `FEATURE/evidence/regression-testing/` holds no failing-run artifact for these two tests (`SearchPatterns: liveness-*.md, fail-before-*.md`). Exactly one file matching `fail-before-exception.*.md` exists in that folder after this task.
- [x] [P5-T2] Create QuickFiler.Test/TestSupport/ArmingFakeTimeProvider.cs with the whole content of Delivered Source W2 (Write tool, absolute path, the Markdown indent removed), normalise its line endings with `CMD-EOL` (`FILE` AFTP), and insert the second T2 line (`<Compile Include="TestSupport\ArmingFakeTimeProvider.cs" />`, four leading spaces) into QuickFiler.Test/QuickFiler.Test.csproj immediately after the line containing `TestSupport\SynchronousBackgroundWorker.cs`.
  - Acceptance (recorded by P5-T5): `WORKTREE-LEAF: agent-a291a7fbabf9d0229`; `BARE_LF: 0`; `CRLF_COUNT:` equals `LINES:`; `LINES:` at most 500 and at least 30; exactly one project-file line contains `TestSupport\ArmingFakeTimeProvider.cs`, the line before it contains `TestSupport\SynchronousBackgroundWorker.cs` and the line after it contains `TestSupport\WinFormsPumpHostTests.cs`.
- [x] [P5-T3] Apply Delivered Source L1b, L3 and L4 to QuickFiler.Test/Controllers/QfcDatamodelLivenessTests.cs (remove the `Microsoft.Extensions.Time.Testing` using, replace test 1 whole with L-T1, insert L-SCOPE after `ReadLivenessFlag`).
  - Acceptance (recorded by P5-T5): `T1-LIVE` tokens `await` 3, `using (NoSynchronizationContext())` 2, `Task.Yield` 0, `fake.Advance` 0, `for (int i` 0, `clock.ReArm();` 1, `(await pending)` 1; LIV tokens `Task.Yield` 0, `fake.Advance` 0, `FakeTimeProvider` 2 (the two `ArmingFakeTimeProvider` lines of L-T1), `NoSynchronizationContext` 3, `new ArmingFakeTimeProvider()` 1, `new SynchronousBackgroundWorker()` 4, `using (var worker = new SynchronousBackgroundWorker())` 4, `StartSynchronously` 2, `[TestMethod]` 4; and, reading the T1-LIVE span, each `await` line lies outside both `using (NoSynchronizationContext())` blocks (recorded as `SCOPE-BODIES-AWAIT-FREE: YES`).
- [x] [P5-T4] Apply Delivered Source M2 to QuickFiler.Test/Controllers/QfcDatamodelTests.cs (replace the sibling test whole with M-T).
  - Acceptance (recorded by P5-T5): `T-SIB` tokens `await Task.Yield();` 0, `clock.ReArm();` 1, `await Task.WhenAny(clock.Armed, pending)` 1, `using (var worker = new BackgroundWorker())` 1, `IList<MailItem> result = await pending;` 1; DMT tokens `new BackgroundWorker()` 2, `using (var worker = new BackgroundWorker())` 2, `new ArmingFakeTimeProvider()` 1, `must keep polling while the worker can still add candidates` 1, `the gate re-armed instead of returning` 1, `await Task.Yield();` 0, `fake.Advance` 2, `FakeTimeProvider` 6, `[TestMethod]` 9.
- [x] [P5-T5] Record the liveness-edit census in FEATURE/evidence/qa-gates/liveness-edit-census.md with the P5-T2 CMD-EOL output, `CMD-LINECOUNT` on `"LIV", "DMT", "AFTP"`, `CMD-TOKEN-COUNT` on LIV, DMT and PROJ (the P0-T13 lists) and on AFTP (TOKENS `"internal sealed class ArmingFakeTimeProvider : FakeTimeProvider", "internal Task Armed", "internal void ReArm()", "public override ITimer CreateTimer(", "base.CreateTimer(", "RunContinuationsAsynchronously", "_armed.TrySetResult(true);", "namespace QuickFiler.Test.TestSupport"`), `CMD-SPAN-TOKEN-COUNT` on `T1-LIVE`, `HELD` and `T-SIB`, and `git -C WORKTREE diff --numstat HEAD -- QuickFiler QuickFiler.Test` paired with `git -C WORKTREE status --porcelain -- QuickFiler QuickFiler.Test`.
  - Acceptance: `WORKTREE-LEAF: agent-a291a7fbabf9d0229`; the P5-T2, P5-T3 and P5-T4 conditions all hold; AFTP tokens 1, 1, 1, 1, 1, 1, 1, 1; PROJ tokens `PROJ-COMPILE-ITEMS-BASE:` plus 3, 1, 1, 1, 1, 1; every LINES value at most 500; the porcelain span equals the P4-T9 span plus `?? QuickFiler.Test/TestSupport/ArmingFakeTimeProvider.cs` (thirteen lines). Any other value: correct the edit and re-run this task.
- [x] [P5-T6] Build the tree with the rewritten tests using `CMD-BUILD` (`TASKID` p5-t6) against WORKTREE/TaskMaster.sln and record it in FEATURE/evidence/regression-testing/liveness-build.md.
  - Acceptance: `WORKTREE-LEAF: agent-a291a7fbabf9d0229`, `EXIT_CODE: 0`, `ERRORS: 0`, `TEST_DLL_ADVANCED: True`, `CSC_OUT_QUICKFILER_TEST:` at least 1. A compile error in W2, L-T1, L-SCOPE or M-T is corrected within the delivered design, then P5-T5 and this task are re-run.
- [x] [P5-T7] Run the two rewritten tests by fully qualified name from QuickFiler.Test/bin/Debug/QuickFiler.Test.dll with `CMD-VSTEST` (`FILTER-LIVENESS-PAIR`, `TASKID` p5-t7, `NAMES-LIVENESS`) and record FEATURE/evidence/regression-testing/liveness-pass-after.md.
  - Acceptance: `WORKTREE-LEAF: agent-a291a7fbabf9d0229`; `EXIT_CODE: 0`; `TRX_PRESENT: True`; `SEQUENCE_FILES: 0`; `RESULT_COUNT: 2`; two `RESULT` lines, one per NAMES-LIVENESS name, each `Passed` with duration recorded. A `Failed` result whose `MESSAGE` contains `must clear the flag before the next poll` means the inline-continuation assumption of D-18 did not hold on this host: stop and report with the message (do not add a wait or a loop). Any other failure or a `Timeout` outcome is `LIVENESS REWRITE DEFECT`: correct within D-18 and re-run from P5-T5.
- [x] [P5-T8] [expect-fail] Run the labelled sensitivity check and record FEATURE/evidence/regression-testing/liveness-sensitivity-check.md: (1) Edit QuickFiler/Controllers/QfcDatamodel.QueueProcessing.cs, replacing the line `                () => _remainingLoadActive,` (sixteen-space indent, inside the `new QfcStreamingDequeueConfidenceGate(` argument list) with `                () => false,`; (2) `CMD-SPAN-TOKEN-COUNT` on `GATE-LAMBDA`; (3) `CMD-BUILD` (`TASKID` p5-t8); (4) `CMD-VSTEST` (`FILTER-LIVENESS-PAIR`, `TASKID` p5-t8, `NAMES-LIVENESS`); (5) Edit the same line back to `                () => _remainingLoadActive,`; (6) `CMD-SPAN-TOKEN-COUNT` on `GATE-LAMBDA` again; (7) `git -C WORKTREE diff --exit-code 94287369908cc920b21b0e3256314f988ad7d2f5 -- QuickFiler/Controllers/QfcDatamodel.QueueProcessing.cs`; (8) `git -C WORKTREE status --porcelain -- QuickFiler/Controllers/QfcDatamodel.QueueProcessing.cs`.
  - Acceptance: the artifact's first `Command:` row is the step (4) payload with `EXIT_CODE: 1` and `ExpectedExitCode: 1`; `WORKTREE-LEAF: agent-a291a7fbabf9d0229` in every payload; step (2) `GATE-LAMBDA` 0, 1 (edit applied); step (3) `EXIT_CODE: 0`, `ERRORS: 0`, `PROD_DLL_ADVANCED: True`, `TEST_DLL_ADVANCED: True`; step (4) `TRX_PRESENT: True`, `SEQUENCE_FILES: 0`, `RESULT_COUNT: 2`, both `RESULT` lines `Failed` (not `Timeout`, not `Aborted`), the Liveness `MESSAGE` containing `to refer to` and `the gate must arm a second wait`, the sibling `MESSAGE` containing `to refer to` and `must keep polling while the worker can still add candidates` (each failed on its re-arm assertion, so the new shape is sensitive to a dishonest liveness signal and fails crisply rather than hanging); step (6) `GATE-LAMBDA` 1, 0 (edit reverted); step (7) exits 0 (the file is byte-identical to BASE; recorded as `SENSITIVITY-EDIT-REVERTED: YES`); step (8) prints nothing for that path. The artifact is headed `## Labelled sensitivity check (not a fail-before of the old tests)` and states that the temporary edit was never staged or committed. A `Passed` result in step (4) is `REWRITTEN TEST NOT SENSITIVE`: revert first (steps 5 to 8), then stop and report. If step (7) exits non-zero after the revert, correct the file to BASE content with the Edit tool and repeat steps (6) to (8) before anything else; no later task starts with that diff non-empty.
- [x] [P5-T9] Rebuild the reverted production assembly with `CMD-BUILD` (`TASKID` p5-t9) against WORKTREE/TaskMaster.sln and record it in FEATURE/evidence/regression-testing/liveness-revert-build.md.
  - Acceptance: `WORKTREE-LEAF: agent-a291a7fbabf9d0229`, `EXIT_CODE: 0`, `ERRORS: 0`, `PROD_DLL_ADVANCED: True` (the sensitivity binary is replaced), `TEST_DLL_ADVANCED: True`, `CSC_OUT_QUICKFILER:` at least 1.
- [x] [P5-T10] Re-run the two rewritten tests on the reverted tree from QuickFiler.Test/bin/Debug/QuickFiler.Test.dll with `CMD-VSTEST` (`FILTER-LIVENESS-PAIR`, `TASKID` p5-t10, `NAMES-LIVENESS`) and record FEATURE/evidence/regression-testing/liveness-pass-after-revert.md.
  - Acceptance: `WORKTREE-LEAF: agent-a291a7fbabf9d0229`; `EXIT_CODE: 0`; `TRX_PRESENT: True`; `SEQUENCE_FILES: 0`; `RESULT_COUNT: 2`; both `RESULT` lines `Passed` with durations recorded (the confirming run that the P5-T7 outcome is reproduced after the sensitivity cycle; the P5-T7 run is the measured one).
- [x] [P5-T11] Apply Delivered Source Q1 and Q2 to QuickFiler/Controllers/QfcDatamodel.QueueProcessing.cs (the `_remainingLoadActive` doc replacement and the removal of ` (:31-66)` from the `TryUnhookOrReplace` citation). The `QuiesceLoaderAsync` comment at line 52 is left unchanged (D-20).
  - Acceptance (recorded by P5-T12): QQP tokens 0, 0, 1, 0, 3, 1, 1, 0, 1, 0, 1 in the P0-T13 order; LINES 413.
- [x] [P5-T12] Record the comment-edit census in FEATURE/evidence/qa-gates/queue-processing-comment-census.md with `CMD-LINECOUNT` on `"QQP"`, `CMD-TOKEN-COUNT` on QQP (the P0-T13 list), `CMD-HUNKS` on `QuickFiler/Controllers/QfcDatamodel.QueueProcessing.cs`, `git -C WORKTREE diff 94287369908cc920b21b0e3256314f988ad7d2f5 -- QuickFiler/Controllers/QfcDatamodel.QueueProcessing.cs` paired with `git -C WORKTREE status --porcelain -- QuickFiler`.
  - Acceptance: `WORKTREE-LEAF: agent-a291a7fbabf9d0229`; the P5-T11 conditions hold; `HUNK_COUNT: 2` with one hunk whose old range starts at or above 10 and ends at or below 30 and one whose old range starts at or above 280 and ends at or below 292; every changed line in the transcribed diff begins with `///` after its indentation (comment-only: the AC26 and AC20 reading); the porcelain span lists exactly ` M QuickFiler/Controllers/QfcDatamodel.cs` and ` M QuickFiler/Controllers/QfcDatamodel.QueueProcessing.cs`; and the artifact records `QUIESCE-COMMENT-DECISION: UNCHANGED` with the D-20 reason (the `_remainingLoadTask` write at `QfcDatamodel.cs` 218 happens on the thread that runs `Worker_DoWork`, which in production is still the `BackgroundWorker` thread, and the comment's snapshot rationale holds for any cross-thread writer).

### Phase 6 — Scoped Format, Pass-After Runs and Implementation Commit

- [x] [P6-T1] Format the thirteen Write Set `.cs` files (CS13) with a scoped CSharpier pass and record the before-and-after hashes in FEATURE/evidence/qa-gates/scoped-format.md.
  - Command: `CMD-HASH` on CS13; then `pwsh -NoProfile -Command 'PREFIX; dotnet tool run csharpier format QuickFiler.Test\Controllers\QfcItemController.UiThreadDispatcherFixture.cs QuickFiler.Test\Controllers\QfcItemController.FocusAndThemeTests.cs QuickFiler.Test\Controllers\QfcItemController.TestSupport.cs QuickFiler.Test\Controllers\QfcItemController.UiThreadDispatcherFixtureTests.cs QuickFiler.Test\Controllers\QfcItemController.UiThreadDispatcherPinCountTests.cs QuickFiler.Test\Controllers\QfcDatamodelLivenessTests.cs QuickFiler.Test\Controllers\QfcDatamodelTeardownTests.cs QuickFiler.Test\Controllers\QfcInitEmailQueueZeroBatchTests.cs QuickFiler.Test\Controllers\QfcDatamodelTests.cs QuickFiler\Controllers\QfcDatamodel.cs QuickFiler\Controllers\QfcDatamodel.QueueProcessing.cs QuickFiler.Test\TestSupport\SynchronousBackgroundWorker.cs QuickFiler.Test\TestSupport\ArmingFakeTimeProvider.cs; "CSHARPIER_EXIT_CODE: $LASTEXITCODE"'`; then `CMD-HASH` on CS13 again.
  - Acceptance: `WORKTREE-LEAF: agent-a291a7fbabf9d0229`; `CSHARPIER_EXIT_CODE: 0`; the line beginning `Formatted ` is transcribed and labelled as a processed-file count, not a rewrite count; `REWRITTEN:` lists every CS13 path whose hash differs between the two captures, or `NONE` (the rewrite observation is the hash difference, not the console line). Either value completes this task: the pass exists so the committed text is formatter-stable. On a D-13 restart the restarted artifact also records `PRIOR-PASS-REWRITTEN:` (the union of the `REWRITTEN:` paths of every earlier P6-T1 pass in this run, or `NONE`) and `RESTART-CORRECTED:` (the union of the Write Set paths edited by every correction that triggered a D-13 restart in this run).
- [x] [P6-T2] Record the post-format census in FEATURE/evidence/qa-gates/post-format-census.md by re-running every P2-T6, P3-T9, P1-T3, P4-T9, P5-T5 and P5-T12 command (CMD-LINECOUNT on CS13, every CMD-TOKEN-COUNT list including PC, SBW, AFTP and PROJ, the ENSURE, SCOPE, R4SPAN, R4HEAD, R4TAIL, T1-LIVE, HELD, T-SIB and GATE-LAMBDA spans, CMD-HUNKS on TestSupport, the fixture tests, QfcDatamodel.cs and QfcDatamodel.QueueProcessing.cs, numstat paired with the porcelain span), plus `CMD-PIN-NESTING` on `T3SPAN`.
  - Acceptance: every token, span, hunk and numstat value holds after formatting as last recorded for its file (P1-T3 for PC; P2-T6 for FIX; P3-T9 for FAT, TS and FT; P4-T9 for TD, ZB, QDM, SBW and the QfcDatamodel.cs numstat row `1	129`; P5-T5 for LIV, DMT, AFTP and PROJ, superseding the interim P4-T6, P4-T7 and P4-T9 values for those files; P5-T12 for QQP), the project-file numstat row reads `3	0` in place of the P1-T3 value, and the porcelain span below replaces every earlier porcelain expectation (a LINES value, a printed `SPAN:` range and the recorded-not-gated QfcDatamodel.cs `HUNK_COUNT:` may differ from their pre-format values only if `REWRITTEN:`, `PRIOR-PASS-REWRITTEN:` or `RESTART-CORRECTED:` named the file; every CS13 LINES value is at most 500 and the QfcDatamodel.cs value is at most 400); `GATE-LAMBDA` 1, 0 (the sensitivity edit is not present); `T3SPAN` NEST output ends with four lines whose tokens are, in order, `finally`, `Dispose()` (the `transaction.Dispose();` line), `finally`, `ShutdownDispatcher(` (the AC3 reading that the live dispatcher is shut down in a finally block); the FIELDLOCK-ENCLOSURE reading of P2-T6 is restated against the formatted diff; the porcelain span lists exactly the fourteen Write Set code paths (eleven ` M`, three `??`); on a D-13 restart that follows the P6-T9 commit, the ref operand of every HEAD-anchored git command in this task and the porcelain expectation are the ones D-13 states. This artifact is the evidence for AC6, AC7, AC9, AC11, AC12, AC13, AC15, AC16, AC17, AC25, AC26, AC30 and the census half of AC3, AC10, AC14, AC21, AC27, AC29 and AC31.
- [x] [P6-T3] Build the formatted tree with `CMD-BUILD` (`TASKID` p6-t3) against WORKTREE/TaskMaster.sln and record it in FEATURE/evidence/regression-testing/implementation-build.md.
  - Acceptance: `WORKTREE-LEAF: agent-a291a7fbabf9d0229`, `EXIT_CODE: 0`, `ERRORS: 0`, `TEST_DLL_ADVANCED: True`, `CSC_OUT_QUICKFILER_TEST:` at least 1, and `PROD_DLL_ADVANCED: True` with `CSC_OUT_QUICKFILER:` at least 1 when `REWRITTEN:` named a production file (otherwise both recorded).
- [x] [P6-T4] Run the pin-count class alone from QuickFiler.Test/bin/Debug/QuickFiler.Test.dll with `CMD-VSTEST` (`FILTER-PC-CLASS`, `TASKID` p6-t4, `NAMES-PC`) and record FEATURE/evidence/regression-testing/pin-count-class-pass-after.md.
  - Acceptance: `WORKTREE-LEAF: agent-a291a7fbabf9d0229`; `EXIT_CODE: 0`; `TRX_PRESENT: True`; `SEQUENCE_FILES: 0`; `RESULT_COUNT: 4`; four `RESULT` lines, each `Passed`, durations recorded. Any other outcome invokes the D-13 Phase 6 restart rule.
- [x] [P6-T5] Run the fixture test class alone from QuickFiler.Test/bin/Debug/QuickFiler.Test.dll with `CMD-VSTEST` (`FILTER-FT-CLASS`, `TASKID` p6-t5, `NAMES-FT`) and record FEATURE/evidence/regression-testing/fixture-class-pass-after.md.
  - Acceptance: `WORKTREE-LEAF: agent-a291a7fbabf9d0229`; `EXIT_CODE: 0`; `TRX_PRESENT: True`; `SEQUENCE_FILES: 0`; `RESULT_COUNT: 8`; eight `RESULT` lines, one per NAMES-FT name, each `Passed`, durations recorded (R1 to R6 and the #743 and #882 tests pass with their assertions unchanged, AC10; R4 passes without its pin and with the `try/finally`, AC14). Any other outcome invokes the D-13 Phase 6 restart rule.
- [x] [P6-T6] Run the focus-and-theme class alone from QuickFiler.Test/bin/Debug/QuickFiler.Test.dll with `CMD-VSTEST` (`FILTER-FAT-CLASS`, `TASKID` p6-t6, `NAMES-THEME`) and record FEATURE/evidence/regression-testing/focus-and-theme-class-pass-after.md.
  - Acceptance: `WORKTREE-LEAF: agent-a291a7fbabf9d0229`; `EXIT_CODE: 0`; `TRX_PRESENT: True`; `SEQUENCE_FILES: 0`; `RESULT_COUNT: 17`; the two NAMES-THEME `RESULT` lines each `Passed` with durations recorded; `COUNTERS` shows `passed=17 failed=0` (every test in the class passes after the helper switch, AC15, and the theme tests pass without the deleted calls, AC7). Any other outcome invokes the D-13 Phase 6 restart rule.
- [x] [P6-T7] Run the three #968 classes together in one invocation from QuickFiler.Test/bin/Debug/QuickFiler.Test.dll under the CLI runsettings with `CMD-VSTEST` (`FILTER-CONCURRENT`, `TASKID` p6-t7, empty `NAMES`) and record FEATURE/evidence/regression-testing/concurrent-set-test-summary.md.
  - Acceptance: `WORKTREE-LEAF: agent-a291a7fbabf9d0229`; `EXIT_CODE: 0`; `TRX_PRESENT: True`; `SEQUENCE_FILES: 0`; `RESULT_COUNT: 29`; the `COUNTERS` line shows `total=29 executed=29 passed=29 failed=0`; every `RESULT` line transcribed and `Passed`; `CONCURRENT-NOT-PASSED: NONE`. The artifact states that this is a supporting observation under Workers 0 and ClassLevel scope (MSTest cannot be made to interleave classes on demand), not the regression gate. Any non-Passed outcome is compared with P0-T14 `BASELINE-CONCURRENT-FAILED:`: a name present there is recorded as pre-existing and completes the task with `ExpectedExitCode:` equal to the observed value; a new failure invokes the D-13 Phase 6 restart rule.
- [x] [P6-T8] Run the four datamodel classes together in one invocation from QuickFiler.Test/bin/Debug/QuickFiler.Test.dll under the CLI runsettings with `CMD-VSTEST` (`FILTER-DATAMODEL`, `TASKID` p6-t8, empty `NAMES`) and record FEATURE/evidence/regression-testing/datamodel-set-test-summary.md.
  - Acceptance: `WORKTREE-LEAF: agent-a291a7fbabf9d0229`; `EXIT_CODE: 0`; `TRX_PRESENT: True`; `SEQUENCE_FILES: 0`; `RESULT_COUNT: 21`; the `COUNTERS` line shows `total=21 executed=21 passed=21 failed=0`; every `RESULT` line transcribed and `Passed`, including both NAMES-LIVENESS names; `DATAMODEL-NOT-PASSED: NONE` (AC32; the pass-after half of AC25, AC30 and AC31). Any non-Passed outcome is compared with P0-T15 `BASELINE-DATAMODEL-FAILED:`: a name present there is recorded as pre-existing and completes the task with `ExpectedExitCode:` equal to the observed value; a new failure invokes the D-13 Phase 6 restart rule.
- [x] [P6-T9] Commit the implementation (the fourteen Write Set code files and FEATURE) and record FEATURE/evidence/qa-gates/implementation-commit.md.
  - Commands, separate Bash calls: `git -C WORKTREE add -- CODE14-GIT FEATURE-GIT` (both tokens expanded; one `git add` with fifteen pathspecs); `git -C WORKTREE commit -m "fix(968): reference-count the UiThreadDispatcherFixture ensure pin, remove the dead theme-test calls and fold the #972 datamodel residuals" -- CODE14-GIT FEATURE-GIT` (the same fifteen pathspecs); `git -C WORKTREE rev-parse HEAD`; `git -C WORKTREE diff --name-status 94287369908cc920b21b0e3256314f988ad7d2f5 HEAD`; `git -C WORKTREE status --porcelain --untracked-files=all`.
  - Acceptance: both git writes exit 0; `IMPLEMENTATION-COMMIT:` records the new HEAD as an observation; the name-status diff lists the eleven modified Write Set code paths with status `M`, the three new files (`QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherPinCountTests.cs`, `QuickFiler.Test/TestSupport/SynchronousBackgroundWorker.cs`, `QuickFiler.Test/TestSupport/ArmingFakeTimeProvider.cs`) with status `A`, FEATURE paths, and otherwise only paths from P0-T3 `INHERITED-COMMITTED:`; no porcelain line names a path under QuickFiler/ or QuickFiler.Test/. This artifact is committed in P8-T46.

### Phase 7 — Call-Site Census and Prohibited-Construct Gate

- [x] [P7-T1] Record the post-change call-site census (two independent strategies and the member-set comparison) in FEATURE/evidence/qa-gates/call-site-census.md with `CMD-CENSUS`.
  - Acceptance: `WORKTREE-LEAF: agent-a291a7fbabf9d0229`; `CS_FILES:` recorded; `PRIMARY_LINES: 16` with PRIMARY-FILE fixture 1, test support 2, fixture tests 3, pin-count tests 10 and no focus-and-theme entry; `CROSS_LINES: 32` with CROSS-FILE fixture 5, test support 3, InitializationTests.Part2 1, fixture tests 8, pin-count tests 15 and no focus-and-theme entry; `CONTROL_LINES: 28`; every `PRIMARY` and `CROSS` line transcribed. The artifact then classifies every PRIMARY line as one of `DECLARATION` (fixture `internal static IDisposable EnsureDispatcher()`, test support `internal static IDisposable EnsureUiThreadDispatcher() =>`), `FORWARDER` (test support `UiThreadDispatcherFixture.EnsureDispatcher();`) or `INVOCATION` (the thirteen test-side lines), and every CROSS line not in the PRIMARY set as `DOC`, `COMMENT` or `TEST-NAME` by reading its transcribed text, and records `MEMBER-SET-COMPARISON: AGREE` when the CROSS set contains every PRIMARY line and every CROSS-only line is non-executable (sixteen such lines: fixture 4, test support 1, Part2 1, fixture tests 5, pin-count tests 5). Any CROSS-only line that is an invocation (for example a call written across two lines) is `CENSUS MISMATCH`: stop and report. A `PRIMARY_LINES` or `CROSS_LINES` value other than stated is likewise `CENSUS MISMATCH`.
- [x] [P7-T2] Append the nesting classification to FEATURE/evidence/qa-gates/call-site-census.md with `CMD-PIN-NESTING` on `R1SPAN`, `R2SPAN`, `R3SPAN`, `R4SPAN`, `T1SPAN`, `T2SPAN`, `T3SPAN` and `T4SPAN`.
  - Acceptance: for each of R1SPAN, R2SPAN, R3SPAN, T1SPAN, T2SPAN, T3SPAN and T4SPAN the NEST output shows, for every transaction variable in the span (one per span; two in T4SPAN, transaction then foreignTransaction), by ascending line number: that transaction's BeginTransactionAsync() line, then its single .Install( line, then the pins taken under it, where each pin's EnsureUiThreadDispatcher() line precedes that pin's first Dispose() line (ensureScope, pinA, pinB, freshPin, foreignPin), and every such pin Dispose() line precedes that transaction's first Dispose() line; in T4SPAN the foreignTransaction BeginTransactionAsync() line follows the transaction.Dispose(); line; R4SPAN shows no EnsureUiThreadDispatcher() line and two transactionA.Dispose(); lines, the second inside a finally; the artifact records per method NESTED: YES and INSTALL-BETWEEN-PIN-ACQUIRE-AND-RELEASE: NONE, and records INVOCATIONS-CLASSIFIED: 13 of 13 nested (three in the fixture tests, ten in the pin-count tests). Any other ordering is NESTING VIOLATION: stop and report.
- [x] [P7-T3] Record the prohibited-construct gate in FEATURE/evidence/qa-gates/prohibited-constructs-grep.md with `CMD-ADDED-SCAN`, `git -C WORKTREE diff --exit-code 94287369908cc920b21b0e3256314f988ad7d2f5 HEAD -- scripts/vscode/TaskMaster.cli.runsettings TaskMaster.runsettings`, `git -C WORKTREE status --porcelain -- scripts/vscode/TaskMaster.cli.runsettings TaskMaster.runsettings`, and `CMD-TOKEN-COUNT` on FT (TOKENS `"[Timeout(GateTimeoutMs)]", "private const int GateTimeoutMs = 60000;"`).
  - Acceptance: `WORKTREE-LEAF: agent-a291a7fbabf9d0229`; `GIT_DIFF_EXIT_CODE: 0`; `ADDED_LINES:` greater than 0; `ADDED-TOKEN` counts 0 for `Thread.Sleep`, `Task.Delay`, `DoNotParallelize`, `Retry(`, `Path.GetTempFileName`, `Path.GetTempPath`, `Workers`, `await Task.Yield();`, `for (int i` and `using var `; `Timeout(` 4 and `[Timeout(GateTimeoutMs)]` 4 (every added timeout attribute is the sibling file's constant convention in the pin-count class; the datamodel tests gain none); `GateTimeoutMs = ` 1 with the single `ADDED-LINE` reading `private const int GateTimeoutMs = 60000;` (no timeout increase: the value equals the sibling constant); `_pinCount` and `ArmingFakeTimeProvider` each at least 1 (positive controls); the runsettings diff exits 0 and the porcelain span prints nothing; the FT counts read 8 and 1, equal to P0-T12. Any non-zero prohibited count is `PROHIBITED CONSTRUCT ADDED`: stop and report.

### Phase 8 — Final QA Loop, Coverage Comparison, Static Gates, Check-offs and Final Commit

- [x] [P8-T1] Run the CLAUDE.md formatting step `dotnet tool run csharpier format .` at the worktree root (WORKTREE/.) with a tree observation before and after, and record FEATURE/evidence/qa-gates/csharpier-format-final.md.
  - Commands: `git -C WORKTREE status --porcelain --untracked-files=all`; `CMD-HASH` on CS13; `pwsh -NoProfile -Command 'PREFIX; dotnet tool run csharpier format .; "CSHARPIER_EXIT_CODE: $LASTEXITCODE"'`; `CMD-HASH` on CS13; `git -C WORKTREE status --porcelain --untracked-files=all`.
  - Acceptance: `ITERATION:` recorded; `WORKTREE-LEAF: agent-a291a7fbabf9d0229`; `CSHARPIER_EXIT_CODE: 0`; the `Formatted ` line transcribed and labelled as a processed-file count; `REWRITTEN-WRITESET:` lists every CS13 path whose hash changed, or `NONE`; `REWRITTEN-OTHER:` lists every porcelain path present after and absent before, or `NONE`. Clean pass: both `NONE`. A non-empty `REWRITTEN-WRITESET:` with `REWRITTEN-OTHER: NONE` invokes the D-13 format restart; a non-empty `REWRITTEN-OTHER:` is `FORMAT TOUCHED OUT-OF-SCOPE FILE`: stop.
- [x] [P8-T2] Run the read-only formatter gate `dotnet tool run csharpier check .` at the worktree root (WORKTREE/.) and record FEATURE/evidence/qa-gates/csharpier-check-final.md.
  - Command: `pwsh -NoProfile -Command 'PREFIX; dotnet tool run csharpier check .; "CSHARPIER_EXIT_CODE: $LASTEXITCODE"'`.
  - Acceptance: `ITERATION:` recorded; `WORKTREE-LEAF: agent-a291a7fbabf9d0229`; `EXIT_CODE: 0`; the success-case line beginning `Checked ` and ending `ms.` transcribed verbatim.
- [x] [P8-T3] Run the analyzer rebuild with `CMD-REBUILD` (analyzer GATEARGS, `TASKID` p8-t3) against WORKTREE/TaskMaster.sln and record FEATURE/evidence/qa-gates/msbuild-analyzer-final.md.
  - Acceptance: `ITERATION:` recorded; `WORKTREE-LEAF: agent-a291a7fbabf9d0229`; `EXIT_CODE: 0`; `ERRORS: 0`; `SKIP_CORECOMPILE_LINES: 0`; both `CSC_OUT_` counts at least 1; `WRITESET_DIAGNOSTIC_LINES:` not greater than `ANALYZER-BASELINE-WRITESET-LINES:` and every code in `WRITESET_DIAGNOSTIC_CODES:` present in `ANALYZER-BASELINE-WRITESET-CODES:` (no new analyzer diagnostic in a Write Set file); `WARNINGS:` recorded beside `ANALYZER-BASELINE-WARNINGS:`. This rebuild is the second compile proof for AC27.
- [x] [P8-T4] Run the type-check rebuild with `CMD-REBUILD` (GATEARGS `/p:TreatWarningsAsErrors=true`, no Nullable override, `TASKID` p8-t4) against WORKTREE/TaskMaster.sln and record FEATURE/evidence/qa-gates/msbuild-nullable-final.md.
  - Acceptance: `ITERATION:` recorded; `WORKTREE-LEAF: agent-a291a7fbabf9d0229`; `EXIT_CODE: 0`; `ERRORS: 0`; `SKIP_CORECOMPILE_LINES: 0`; both `CSC_OUT_` counts at least 1; both `_DLL_EXISTS:` values `True`.
- [x] [P8-T5] Run the final repository-wide test-and-coverage run by the P0-T16 route (`CMD-COVERAGE-RUNNER` or `CMD-COVERAGE-DIRECT` with `STAGE` final, then `CMD-COVERAGE-POST` with `STAGE` final) and record FEATURE/evidence/qa-gates/coverage-summary.md and FEATURE/evidence/qa-gates/coverage-jacoco-projection.md.
  - Artifacts: the same fields as P0-T17, with `ITERATION:`, `FAILED-SET:` recorded as `FINAL-FAILED-SET:`, every `MESSAGE` line, and sixteen `RESULT` lines (NAMES-TARGETS).
  - Acceptance, all required: the route equals P0-T16's; `TRX_PRESENT: True`; `SEQUENCE_FILES:` 0 under DIRECT; `TEST_ASSEMBLY_PACKAGES: 0`; `QFCDATAMODEL_CLASS_ENTRIES:` recorded beside the P0-T17 value; the sixteen `RESULT` lines all `Passed` (the four pin-count tests appear as passed in the coverage route's test-result summary, AC21; the two rewritten liveness tests pass in the full parallel run); `NEW-FAILURES:` (names in `FINAL-FAILED-SET:` absent from `BASELINE-FAILED-SET:`) is `NONE`. Any `NEW-FAILURES:` name whose `MESSAGE` contains `The UI dispatcher has not been captured` is recorded as `LEAK-DEPENDENT TEST EXPOSED:` followed by the name, and stops the run for re-planning under the related-defect directive; it is neither a D-13 restart nor `NEW FAILURE OUTSIDE SCOPE`. `FIGURES-COMPARED:` restates the `Total`, `executed`, `error`, `timeout`, `aborted` and `notExecuted` figures from the P0-T17 summary block and from this run's summary block, and this run holds `Total` equal to the P0-T17 value plus 4 (this plan adds four test methods and removes none; the fold rewrites two and adds none), `executed` not less than the P0-T17 value plus 4, and each of `error`, `timeout`, `aborted` and `notExecuted` not greater than its P0-T17 value; `LINE-FLOOR:` and `BRANCH-FLOOR:` each `MET`, or `NOT MET` only where P0-T17 recorded the same floor as not met; the `First-party coverage:` line is recorded as the numeric post-change headline. `RUNNER-GREEN: YES` is recorded when the route is RUNNER and `RUNNER_EXIT_CODE: 0`, otherwise `RUNNER-GREEN: NO` with the reason (`COVERAGE-ROUTE DIRECT` or `PRE-EXISTING FAILURES`). A failure of any target test or a new failure attributable to the Write Set invokes the D-13 failure restart; any other new failure, and any `FIGURES-COMPARED:` breach, is `NEW FAILURE OUTSIDE SCOPE`: stop and report, without re-running.
- [x] [P8-T6] Compare baseline and post-change coverage and test outcomes and record FEATURE/evidence/qa-gates/coverage-comparison.md from FEATURE/evidence/baseline/coverage-summary.md and FEATURE/evidence/qa-gates/coverage-summary.md.
  - Acceptance: the artifact records the baseline and post-change `First-party coverage:` lines (lines and branches, numeric) and the two `ROOT` lines; `FIRST-PARTY-LINE-DELTA:` and `FIRST-PARTY-BRANCH-DELTA:` as post-change percentage minus baseline percentage (two decimals, signed); `AC23-STATUS: MET` when both deltas are at least 0.00, otherwise `AC23-STATUS: NOT MET (COVERAGE-VARIANCE)` with both deltas; the repository-wide comparison in exactly one named branch: `BRANCH A` when the two `lines-valid` figures differ by at most 1 percent of the baseline figure (the post-change line rate must not be lower than baseline by more than 0.5 percentage points), otherwise `BRANCH B` (recorded and not gated, with one sentence stating the denominators are not comparable); `CHANGED-CODE-COVERAGE: NOT MEASURED (TEST ASSEMBLY EXCLUDED; QFCDATAMODEL EXCLUDED BY ATTRIBUTE)` with `TEST_ASSEMBLY_PACKAGES: 0` at both stages and the two `QFCDATAMODEL_CLASS_ENTRIES:` values as its reason (the AC29 reading: the removed lines were in no measured denominator); and `BASELINE-FAILED-SET:`, `FINAL-FAILED-SET:` and `NEW-FAILURES: NONE` restated. A Branch A breach is `COVERAGE REGRESSION`: stop; an `AC23-STATUS: NOT MET` with Branch A satisfied is recorded and does not stop the run (D-7).
- [x] [P8-T7] Record the final toolchain pass in FEATURE/evidence/qa-gates/toolchain-final.md from the P8-T1 to P8-T5 artifacts of the final iteration.
  - Acceptance: one row per step, in order — csharpier format (`REWRITTEN-WRITESET: NONE`, `REWRITTEN-OTHER: NONE`), csharpier check (exit 0), analyzer rebuild (exit 0, `SKIP_CORECOMPILE_LINES: 0`), TreatWarningsAsErrors rebuild (exit 0, `SKIP_CORECOMPILE_LINES: 0`), coverage run (route, exit code, `RUNNER-GREEN:`) — each with its exact command, `EXIT_CODE:` and the same `ITERATION:` value; `SINGLE-PASS: YES` when all five rows come from one iteration with no restart after P8-T1; `AC22-STATUS: MET` only when `SINGLE-PASS: YES` and `RUNNER-GREEN: YES`, otherwise `AC22-STATUS: NOT MET` with the reason.
- [x] [P8-T8] Record the file-size gate in FEATURE/evidence/qa-gates/file-line-counts.md with `CMD-LINECOUNT` on CS13 and `CMD-TOKEN-COUNT` on PROJ (TOKENS `"<Compile Include="`).
  - Acceptance: `WORKTREE-LEAF: agent-a291a7fbabf9d0229`; thirteen `LINES` values, each at most 500, each equal to the P6-T2 value for the same file (the P8-T1 format rewrote nothing); the `QuickFiler\Controllers\QfcDatamodel.cs` value at most 400, recorded beside `QFCDATAMODEL-LINES-BEFORE: 495` from P4-T1 (AC28); the project file's `<Compile Include=` count recorded and equal to `PROJ-COMPILE-ITEMS-BASE:` plus 3 (the project file is not a C# source file and the 500-line limit does not apply to it).
- [x] [P8-T9] Record the footprint gate in FEATURE/evidence/qa-gates/footprint-scope.md with `git -C WORKTREE diff --name-status 94287369908cc920b21b0e3256314f988ad7d2f5 HEAD` paired with `git -C WORKTREE status --porcelain --untracked-files=all`, and `git -C WORKTREE rev-parse origin/main`.
  - Acceptance: `THIS-ITEM-FOOTPRINT:` (name-status paths outside FEATURE, excluding every path P0-T3 listed in `INHERITED-COMMITTED:`) is exactly the fourteen Write Set code paths: eleven with status `M` (`QuickFiler/Controllers/QfcDatamodel.cs`, `QuickFiler/Controllers/QfcDatamodel.QueueProcessing.cs`, `QuickFiler.Test/QuickFiler.Test.csproj`, `QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixture.cs`, `QuickFiler.Test/Controllers/QfcItemController.FocusAndThemeTests.cs`, `QuickFiler.Test/Controllers/QfcItemController.TestSupport.cs`, `QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixtureTests.cs`, `QuickFiler.Test/Controllers/QfcDatamodelLivenessTests.cs`, `QuickFiler.Test/Controllers/QfcDatamodelTeardownTests.cs`, `QuickFiler.Test/Controllers/QfcInitEmailQueueZeroBatchTests.cs`, `QuickFiler.Test/Controllers/QfcDatamodelTests.cs`) and three with status `A` (`QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherPinCountTests.cs`, `QuickFiler.Test/TestSupport/SynchronousBackgroundWorker.cs`, `QuickFiler.Test/TestSupport/ArmingFakeTimeProvider.cs`); the only paths under `QuickFiler/` are the two production paths AC20 names, and every other footprint path is under `QuickFiler.Test/`; the excluded inherited paths are recorded as `INHERITED-AND-EXCLUDED:` and each is under FEATURE or is one of the two promoted records; no porcelain line names a path under QuickFiler/, QuickFiler.Test/ or scripts/; `ORIGIN-MAIN-NOW:` records the rev-parse output and `BASE-REF-MOVED:` reads `NO` when it equals `94287369908cc920b21b0e3256314f988ad7d2f5` and otherwise `YES` (recorded, not a stop: every gate names BASE explicitly).
- [x] [P8-T10] Record the evidence hygiene gate in FEATURE/evidence/qa-gates/evidence-hygiene.md by scanning every file under FEATURE/evidence/.
  - Command: `pwsh -NoProfile -Command 'PREFIX; $b = [char]92; $files = @(Get-ChildItem -LiteralPath "docs\features\active\2026-10-02-focus-and-theme-tests-leak-shared-dispatcher-setup-968\evidence" -Recurse -File); "EVIDENCE_FILES=$($files.Count)"; "RAW_DOCUMENTS=$(@($files | Where-Object { $_.Extension -in @(".trx", ".xml", ".coverage", ".coveragexml", ".log") }).Count)"; $pattern = "[a-z]:[" + $b + $b + "/]+users[" + $b + $b + "/]+[a-z0-9_.~-]"; "PROFILE_PATH_LINES=$(@($files | Select-String -Pattern $pattern).Count)"'`.
  - Acceptance: `WORKTREE-LEAF: agent-a291a7fbabf9d0229`; `EVIDENCE_FILES=` at least 1; `RAW_DOCUMENTS=0`; `PROFILE_PATH_LINES=0` (the pattern is the repository hygiene rule's, built from `[char]92` so the Bash channel cannot collapse its backslashes). A non-zero value names the offending files; the executor redacts them and re-runs this task.

- [x] [P8-T11] Check off AC1 in FEATURE/spec.md when `EnsureDispatcher_TwoPinsHeld_ReleasingTheFirstKeepsTheDispatcherUntilTheLastRelease` is `Passed` in FEATURE/evidence/regression-testing/pass-after-pin-count.md and FEATURE/evidence/qa-gates/coverage-summary.md.
  - Acceptance: only `- [ ] AC1:` changes to `- [x] AC1:`; otherwise the box stays unchecked and `AC1: NOT MET` with the reason is recorded for P8-T43.
- [x] [P8-T12] Check off AC2 in FEATURE/spec.md when `EnsureDispatcher_TwoPinsHeld_ReleasingTheFirstKeepsTheDispatcherUntilTheLastRelease` and `EnsureDispatcher_TwoPinsHeld_ReleaseOrderDoesNotChangeTheOutcome` are both `Passed` in FEATURE/evidence/regression-testing/pass-after-pin-count.md and FEATURE/evidence/qa-gates/coverage-summary.md.
  - Acceptance: only `- [ ] AC2:` changes; otherwise unchecked with `AC2: NOT MET`.
- [x] [P8-T13] Check off AC3 in FEATURE/spec.md when `EnsureDispatcher_UnderATransactionHoldingALiveDispatcher_ReleasingAllPinsLeavesTheLiveDispatcher` is `Passed` in FEATURE/evidence/regression-testing/pass-after-pin-count.md and FEATURE/evidence/qa-gates/coverage-summary.md, `EnsureDispatcher_WhileATransactionHoldsALiveDispatcher_DoesNotReplaceIt` is `Passed` in FEATURE/evidence/regression-testing/fixture-class-pass-after.md, and FEATURE/evidence/qa-gates/post-format-census.md shows the T3SPAN NEST tail `finally`, `Dispose()`, `finally`, `ShutdownDispatcher(` and the PC token `QfcItemControllerTestSupport.ShutdownDispatcher(live);` 1.
  - Acceptance: only `- [ ] AC3:` changes; otherwise unchecked with `AC3: NOT MET`.
- [x] [P8-T14] Check off AC4 in FEATURE/spec.md when `EnsureDispatcher_AfterAFullPinCycle_AFreshSinglePinStillInstallsAndRestores` is `Passed` in FEATURE/evidence/regression-testing/pass-after-pin-count.md and FEATURE/evidence/qa-gates/coverage-summary.md.
  - Acceptance: only `- [ ] AC4:` changes; otherwise unchecked with `AC4: NOT MET`.
- [x] [P8-T15] Check off AC5 in FEATURE/spec.md when FEATURE/evidence/regression-testing/fail-before-pin-count.md records `Failed` with a `MESSAGE` containing `to refer to`, the because text and `but found <null>`, FEATURE/evidence/regression-testing/pass-after-pin-count.md records the same test `Passed`, and the `PHASE2-PORCELAIN:` span differs from `PHASE1-PORCELAIN:` (FEATURE/evidence/regression-testing/pin-count-file-census.md) by exactly the fixture file.
  - Acceptance: only `- [ ] AC5:` changes; otherwise unchecked with `AC5: NOT MET`.
- [x] [P8-T16] Check off AC6 in FEATURE/spec.md when FEATURE/evidence/qa-gates/post-format-census.md shows the PC tokens `Regression test: fails before the fix` 1, `Specification test: passes before and after the fix` 3 and `never read the shared static` 1, and FEATURE/evidence/regression-testing/specification-tests-before-fix.md records the three specification tests `Passed` before the fix.
  - Acceptance: only `- [ ] AC6:` changes; otherwise unchecked with `AC6: NOT MET`.
- [x] [P8-T17] Check off AC7 in FEATURE/spec.md when FEATURE/evidence/qa-gates/post-format-census.md shows the FAT token `EnsureUiThreadDispatcher` 0 and FEATURE/evidence/regression-testing/focus-and-theme-class-pass-after.md records `SetThemeDark_FromNormal_SelectsDarkNormalTheme` and `SetThemeLight_FromNormal_SelectsLightNormalTheme` both `Passed`.
  - Acceptance: only `- [ ] AC7:` changes; otherwise unchecked with `AC7: NOT MET`.
- [x] [P8-T18] Check off AC8 in FEATURE/spec.md when FEATURE/evidence/qa-gates/call-site-census.md records `MEMBER-SET-COMPARISON: AGREE`, `INVOCATIONS-CLASSIFIED: 13 of 13 nested` and `INSTALL-BETWEEN-PIN-ACQUIRE-AND-RELEASE: NONE` for every pin-bearing method.
  - Acceptance: only `- [ ] AC8:` changes; otherwise unchecked with `AC8: NOT MET`.
- [x] [P8-T19] Check off AC9 in FEATURE/spec.md when FEATURE/evidence/qa-gates/post-format-census.md shows FIX tokens `_pinCount` 4, `_fixtureInstalledParked` 4 and `lock (FieldLock)` 5, SCOPE `CompareExchange(` 0 and `lock (FieldLock)` 1, and its `FIELDLOCK-ENCLOSURE:` reading states both fields are private statics whose every use lies inside a `lock (FieldLock)` block.
  - Acceptance: only `- [ ] AC9:` changes; otherwise unchecked with `AC9: NOT MET`.
- [x] [P8-T20] Check off AC10 in FEATURE/spec.md when FEATURE/evidence/regression-testing/fixture-class-pass-after.md records all eight NAMES-FT tests `Passed` and FEATURE/evidence/qa-gates/post-format-census.md shows every fixture-tests hunk inside the R4 doc-and-body range and the R4SPAN tokens `.BeSameAs(` 1, `.NotBeSameAs(` 1, `issue #230 lost update` 1, with the FT token `the waiter cannot observe the pre-restore value` 1.
  - Acceptance: only `- [ ] AC10:` changes; otherwise unchecked with `AC10: NOT MET`.
- [x] [P8-T21] Check off AC11 in FEATURE/spec.md when FEATURE/evidence/qa-gates/post-format-census.md shows FIX tokens `leaks exactly` 0, `installed nothing carries` 0, `A scope that installed nothing` 0, `pins for the process lifetime` 2 and `install-ownership flag` 1.
  - Acceptance: only `- [ ] AC11:` changes; otherwise unchecked with `AC11: NOT MET`.
- [x] [P8-T22] Check off AC12 in FEATURE/spec.md when FEATURE/evidence/qa-gates/post-format-census.md shows TS tokens `Becomes moot` 0, `leaks exactly` 0, `still delegate to a callee` 0, `remaining legitimate` 1 and `QfcItemController_UiThreadDispatcherPinCountTests` 1.
  - Acceptance: only `- [ ] AC12:` changes; otherwise unchecked with `AC12: NOT MET`.
- [x] [P8-T23] Check off AC13 in FEATURE/spec.md when FEATURE/evidence/qa-gates/post-format-census.md shows FT tokens `no other class may dispose` 0, `removed that pin: the fixture now counts pins` 1 and `(W5) must not latch` 1.
  - Acceptance: only `- [ ] AC13:` changes; otherwise unchecked with `AC13: NOT MET`.
- [x] [P8-T24] Check off AC14 in FEATURE/spec.md when FEATURE/evidence/qa-gates/post-format-census.md shows R4SPAN `transactionA.Dispose();` 2 and R4TAIL `finally` 2 with `transactionA.Dispose();` 1, and FEATURE/evidence/regression-testing/fixture-class-pass-after.md records `Transaction_SecondCallerCannotInstallUntilTheFirstRestores` and `Transaction_DisposedTwice_DoesNotOverReleaseTheGate` both `Passed` (this check-off also records `CLOSES-972-ITEM-5: YES`).
  - Acceptance: only `- [ ] AC14:` changes; otherwise unchecked with `AC14: NOT MET`.
- [x] [P8-T25] Check off AC15 in FEATURE/spec.md when FEATURE/evidence/qa-gates/post-format-census.md shows FAT tokens `private static Mock<IItemViewer> BuildExecutingViewer` 0, `QfcItemControllerTestSupport.BuildExecutingViewer()` 8 and `BuildExecutingViewer` 8, the TS token `not reachable from another test file` 0, and FEATURE/evidence/regression-testing/focus-and-theme-class-pass-after.md records `passed=17 failed=0`.
  - Acceptance: only `- [ ] AC15:` changes; otherwise unchecked with `AC15: NOT MET`.
- [x] [P8-T26] Check off AC16 in FEATURE/spec.md when FEATURE/evidence/qa-gates/post-format-census.md shows FAT tokens `absorbs the delegate without running it` 1, `shared UiThread static is irrelevant` 1 and `absorbs the queued application` 1.
  - Acceptance: only `- [ ] AC16:` changes; otherwise unchecked with `AC16: NOT MET`.
- [x] [P8-T27] Check off AC17 in FEATURE/spec.md when FEATURE/evidence/qa-gates/post-format-census.md shows the TestSupport `HUNK_COUNT: 2` with every hunk old-range start at or above 200 and the TS token `internal static void EnsureSynchronizationContext()` 1.
  - Acceptance: only `- [ ] AC17:` changes; otherwise unchecked with `AC17: NOT MET`.
- [x] [P8-T28] Check off AC18 in FEATURE/spec.md when FEATURE/evidence/qa-gates/file-line-counts.md shows every CS13 `LINES` value at most 500.
  - Acceptance: only `- [ ] AC18:` changes; otherwise unchecked with `AC18: NOT MET`.
- [x] [P8-T29] Check off AC19 in FEATURE/spec.md when FEATURE/evidence/qa-gates/prohibited-constructs-grep.md holds every P7-T3 condition.
  - Acceptance: only `- [ ] AC19:` changes; otherwise unchecked with `AC19: NOT MET`.
- [x] [P8-T30] Check off AC20 in FEATURE/spec.md when FEATURE/evidence/qa-gates/footprint-scope.md shows `THIS-ITEM-FOOTPRINT:` equal to the fourteen listed paths, with exactly `QuickFiler/Controllers/QfcDatamodel.cs` and `QuickFiler/Controllers/QfcDatamodel.QueueProcessing.cs` under `QuickFiler/` and every other path under `QuickFiler.Test/`, and every other name-status path under FEATURE or in `INHERITED-AND-EXCLUDED:`.
  - Acceptance: only `- [ ] AC20:` changes; otherwise unchecked with `AC20: NOT MET`.
- [x] [P8-T31] Check off AC21 in FEATURE/spec.md when FEATURE/evidence/qa-gates/post-format-census.md shows the PROJ token `Controllers\QfcItemController.UiThreadDispatcherPinCountTests.cs` 1 and FEATURE/evidence/qa-gates/coverage-summary.md records all four NAMES-PC tests `Passed`.
  - Acceptance: only `- [ ] AC21:` changes; otherwise unchecked with `AC21: NOT MET`.
- [x] [P8-T32] Check off AC22 in FEATURE/spec.md when FEATURE/evidence/qa-gates/toolchain-final.md reads `AC22-STATUS: MET`.
  - Acceptance: only `- [ ] AC22:` changes; otherwise the box stays unchecked and the recorded reason (`COVERAGE-ROUTE DIRECT`, `PRE-EXISTING FAILURES` or a restart after P8-T1) is carried to P8-T43 as `AC22: NOT MET`.
- [x] [P8-T33] Check off AC23 in FEATURE/spec.md when FEATURE/evidence/qa-gates/coverage-comparison.md reads `AC23-STATUS: MET`.
  - Acceptance: only `- [ ] AC23:` changes; otherwise the box stays unchecked and `AC23: NOT MET (COVERAGE-VARIANCE)` with both deltas is carried to P8-T43.
- [x] [P8-T34] Check off AC24 in FEATURE/spec.md when FEATURE/evidence/regression-testing/concurrent-set-test-summary.md records `RESULT_COUNT: 29` and `CONCURRENT-NOT-PASSED: NONE`.
  - Acceptance: only `- [ ] AC24:` changes; otherwise unchecked with `AC24: NOT MET`.
- [x] [P8-T35] Check off AC25 in FEATURE/spec.md when FEATURE/evidence/qa-gates/post-format-census.md shows SBW tokens `class SynchronousBackgroundWorker` 1, `internal sealed class SynchronousBackgroundWorker : BackgroundWorker` 1 and `internal static void StartSynchronously(BackgroundWorker worker)` 1, LIV, TD and ZB tokens `class SynchronousBackgroundWorker` 0 and `Duplicated per file` 0 with ZB `through the nested` 0, every `StartSynchronously` line in LIV, TD and ZB equal to its `SynchronousBackgroundWorker.StartSynchronously` count (2 and 2, 1 and 1, 3 and 3), the PROJ token `TestSupport\SynchronousBackgroundWorker.cs` 1, and FEATURE/evidence/regression-testing/datamodel-set-test-summary.md records `passed=21 failed=0` (a repository-wide `class SynchronousBackgroundWorker` count of exactly 1 follows from the three consumer counts of 0 plus the SBW count of 1, because P0-T13 and P6-T2 enumerate every file that carried the identifier).
  - Acceptance: only `- [ ] AC25:` changes; otherwise unchecked with `AC25: NOT MET`.
- [x] [P8-T36] Check off AC26 in FEATURE/spec.md when FEATURE/evidence/qa-gates/post-format-census.md shows QQP tokens `RunWorkerAsync` 0, `written on the worker thread and read` 0, `(:31-66)` 0, `WorkerStarter` 1 and `share no other fence` 1, and FEATURE/evidence/qa-gates/queue-processing-comment-census.md records `QUIESCE-COMMENT-DECISION: UNCHANGED` with its reason and `HUNK_COUNT: 2` with comment-only changed lines.
  - Acceptance: only `- [ ] AC26:` changes; otherwise unchecked with `AC26: NOT MET`.
- [x] [P8-T37] Check off AC27 in FEATURE/spec.md when FEATURE/evidence/qa-gates/qfc-datamodel-legacy-callers.md records `INVOCATIONS: 0`, `Primary Count: 4`, `Cross-check Count: 4` and an identical member-set comparison, FEATURE/evidence/qa-gates/post-format-census.md shows QDM tokens `Worker_RunWorkerCompleted` 0, `nameof(LoadRemainingEmailsToQueue)` 0, `nameof(LoadRemainingEmailsToQueueAsync)} Error.` 1, `LoadRemainingEmailsToQueue(BackgroundWorker bw` 0, `log4net.ILog log =` 0, `Linked List Locking` 0, `//e.Result =` 0, `//worker.RunWorkerCompleted` 0, `ForEachAwaitWithCancellationAsync` 0 and `: IQfcDatamodel` 1, and FEATURE/evidence/qa-gates/msbuild-analyzer-final.md and FEATURE/evidence/qa-gates/msbuild-nullable-final.md both record `EXIT_CODE: 0` (every `IQfcDatamodel` member is still implemented, because the type still declares `: IQfcDatamodel` and the solution compiles).
  - Acceptance: only `- [ ] AC27:` changes; otherwise unchecked with `AC27: NOT MET`.
- [x] [P8-T38] Check off AC28 in FEATURE/spec.md when FEATURE/evidence/qa-gates/file-line-counts.md shows the `QuickFiler\Controllers\QfcDatamodel.cs` `LINES` value at most 400 beside `QFCDATAMODEL-LINES-BEFORE: 495`.
  - Acceptance: only `- [ ] AC28:` changes; otherwise unchecked with `AC28: NOT MET`.
- [x] [P8-T39] Check off AC29 in FEATURE/spec.md when FEATURE/evidence/qa-gates/post-format-census.md shows the QDM token `[ExcludeFromCodeCoverage]` 1, FEATURE/evidence/qa-gates/qfc-datamodel-legacy-callers.md records that no test references any of the four members (every test-file hit classified `DOC-PROSE` or `OTHER-TYPE-SAME-NAME`), and FEATURE/evidence/qa-gates/coverage-comparison.md reads `AC23-STATUS: MET`.
  - Acceptance: only `- [ ] AC29:` changes; otherwise unchecked with `AC29: NOT MET` (an `AC23-STATUS: NOT MET` carries `COVERAGE-VARIANCE` here too).
- [x] [P8-T40] Check off AC30 in FEATURE/spec.md when FEATURE/evidence/qa-gates/post-format-census.md shows LIV `new SynchronousBackgroundWorker()` 4 and `using (var worker = new SynchronousBackgroundWorker())` 4, ZB `new SynchronousBackgroundWorker()` 3 and `using (var worker = new SynchronousBackgroundWorker())` 3 with `InitEmailQueue(0, new` 0 and `InitEmailQueue(2, new` 0, DMT `new BackgroundWorker()` 2 and `using (var worker = new BackgroundWorker())` 2, `HELD` `new SynchronousBackgroundWorker()` 0 and `SynchronousBackgroundWorker worker,` 1, and FEATURE/evidence/regression-testing/datamodel-set-test-summary.md records `passed=21 failed=0`.
  - Acceptance: only `- [ ] AC30:` changes; otherwise unchecked with `AC30: NOT MET`.
- [x] [P8-T41] Check off AC31 in FEATURE/spec.md when FEATURE/evidence/qa-gates/post-format-census.md shows `T1-LIVE` `Task.Yield` 0, `fake.Advance` 0, `for (int i` 0, `clock.ReArm();` 1 and `(await pending)` 1, `T-SIB` `await Task.Yield();` 0, `clock.ReArm();` 1, `await Task.WhenAny(clock.Armed, pending)` 1 and `IList<MailItem> result = await pending;` 1, AFTP tokens all 1 and the PROJ token `TestSupport\ArmingFakeTimeProvider.cs` 1; exactly one file matches `fail-before-exception.*.md` under FEATURE/evidence/regression-testing/ and carries `WhyFailingRunImpossible:`; FEATURE/evidence/regression-testing/liveness-sensitivity-check.md records both tests `Failed` on their re-arm assertions with `SENSITIVITY-EDIT-REVERTED: YES`; and FEATURE/evidence/regression-testing/liveness-pass-after-revert.md and FEATURE/evidence/regression-testing/datamodel-set-test-summary.md record both NAMES-LIVENESS tests `Passed`.
  - Acceptance: only `- [ ] AC31:` changes; otherwise unchecked with `AC31: NOT MET`.
- [x] [P8-T42] Check off AC32 in FEATURE/spec.md when FEATURE/evidence/regression-testing/datamodel-set-test-summary.md records `RESULT_COUNT: 21` and `DATAMODEL-NOT-PASSED: NONE`.
  - Acceptance: only `- [ ] AC32:` changes; otherwise unchecked with `AC32: NOT MET`.
- [x] [P8-T43] Write the acceptance-criteria status summary FEATURE/evidence/other/ac-status-summary.md.
  - Acceptance: the artifact carries `Timestamp:` and the acceptance-criteria-tracking status block (Source: the spec path; Total AC items: 32; Checked off; Remaining; Items remaining with each `ACn: NOT MET` reason), with the counts read from FEATURE/spec.md after P8-T42 (lines beginning `- [x] AC` and `- [ ] AC`, summing to 32).
- [x] [P8-T44] Re-run the P8-T10 hygiene command over FEATURE/evidence/ after P8-T43 and append the result to FEATURE/evidence/qa-gates/evidence-hygiene.md as a `## Re-run after check-offs` section.
  - Acceptance: `RAW_DOCUMENTS=0` and `PROFILE_PATH_LINES=0` for the evidence tree as it will be committed.
- [x] [P8-T45] Verify that FEATURE/spec.md changed only in its checkbox lines by recording `git -C WORKTREE diff --numstat HEAD -- docs/features/active/2026-10-02-focus-and-theme-tests-leak-shared-dispatcher-setup-968/spec.md` and `git -C WORKTREE diff HEAD -- docs/features/active/2026-10-02-focus-and-theme-tests-leak-shared-dispatcher-setup-968/spec.md`, paired with `git -C WORKTREE status --porcelain -- docs/features/active/2026-10-02-focus-and-theme-tests-leak-shared-dispatcher-setup-968/spec.md`, in a `## Spec check-off diff` section appended to FEATURE/evidence/other/ac-status-summary.md.
  - Acceptance: added and deleted line counts are equal and equal the checked-off count; every deleted line begins `- [ ] AC` and every added line begins `- [x] AC` with identical remaining text.
- [x] [P8-T46] Commit the remaining feature evidence and check-offs (FEATURE only) and record FEATURE/evidence/qa-gates/final-commit.md.
  - Commands, separate Bash calls: `git -C WORKTREE add -- docs/features/active/2026-10-02-focus-and-theme-tests-leak-shared-dispatcher-setup-968`; `git -C WORKTREE commit -m "docs(968): record final QA evidence and acceptance check-offs" -- docs/features/active/2026-10-02-focus-and-theme-tests-leak-shared-dispatcher-setup-968`; `git -C WORKTREE rev-parse HEAD`; `git -C WORKTREE diff --name-status 94287369908cc920b21b0e3256314f988ad7d2f5 HEAD`; `git -C WORKTREE status --porcelain --untracked-files=all`.
  - Acceptance: both git writes exit 0; `FINAL-COMMIT:` records HEAD as an observation; the name-status paths outside FEATURE are exactly the P8-T9 footprint plus `INHERITED-AND-EXCLUDED:`; the porcelain output names no path under FEATURE other than this plan file (whose final check-off mark follows the commit) and FEATURE/evidence/qa-gates/final-commit.md (written after the commit), and no path under QuickFiler/, QuickFiler.Test/ or scripts/. The final-commit artifact and this plan's check-off marks are committed by the orchestrator with the plan file. No PR is opened and no merge is performed by this plan; the PR body the orchestrator authors carries `Closes #968` and `Closes #972` (spec "Dependencies").

## Planner self-review and internal review record

The round-1 to round-4 forms of the two records below are repeated verbatim in `FEATURE/evidence/other/planner-review.2026-10-02T22-44.md` (the round-4 form in its `## Round-4 delta application (2026-10-03T01-25 deltas)` section; the round-1, round-2 and round-3 records precede that section in the same file). The round-5 form is carried in this plan only: the round-5 revision instruction confined edits to the plan file, and the round-5 re-derivation is returned to the orchestrator in the planner's round-5 message.

SELF-REVIEW: RE-DERIVED THIS PASS

Citations re-derived in this pass (file and line, test or identifier): see items 1 to 17 and the sibling-region re-checks in `FEATURE/evidence/other/planner-review.2026-10-02T22-44.md`; in summary — the ten existing Write Set `.cs` files (line totals 342, 470, 497, 440, 312, 244, 232, 371, 495, 413 and CRLF on every line), the project file items 155, 157, 161, 183, 200, 201, 203, 212, 227 to 229, the repository-wide censuses (20/9/23 for the ensure calls; 25, 3 and 2 lines for the legacy-member proof; 13 lines in 3 files for `SynchronousBackgroundWorker`; 20 lines in 7 files for the liveness flag; 0 for `ArmingFakeTimeProvider|NoSynchronizationContext`), the gate's `DequeueAsync` 190 to 301, the two helper precedents, the worktree HEAD metadata, the two promoted records, the spec's 32 acceptance lines and amendment literals, issue.md 12 and 65 to 77, and the round-1 report's ten deltas; and, in the round-2 pass, the eight round-2 deltas — the 43-line F-SCOPE block and the 375 total; the fold span END anchors at LIV 166 and 218, DMT 134 and QQP 311 and LIV 163 `(await pending)`; QDM 431 and 440 `ForEachAwaitWithCancellationAsync`; the `FakeTimeProvider` substring at LIV 114 and DMT 99, 216, 224, 249 and 258, with the `using Microsoft.Extensions.Time.Testing;` directive at line 9 of each file not containing it; the P1 removal arithmetic (128 plus the replaced line 369 as numstat `1	129`) and the plan's own tab-separated numstat convention at P1-T3; the L-T1 and M-T doc lines; D-10 — together with the plan-wide occurrence sweep of every changed numeral and token; and, in the round-3 pass, the four round-3 deltas and advisory A1 — the legacy-member primary pattern over `QuickFiler/Controllers/QfcDatamodel.cs` (17 lines: 40, 52, 130, 194, 209, 210, 246, 335, 363, 369, 378, 404, 410, 418, 462, 469, 472) and over `*.cs` repository-wide (25 lines in 5 files: the 17 plus `QfcHomeController.cs` 4, `QfcHomeControllerRunAsyncTests.cs` 2, `QfcDatamodelLivenessTests.cs` 1, `QfcInitEmailQueueZeroBatchTests.cs` 1); the method-group assignments `RemainingEmailLoader = LoadRemainingEmailsToQueueAsync;` at `QfcDatamodel.cs` 40 and 52; the `QfcDatamodel.cs` lines adjacent to every P1 deletion (98 and 102, 193 and 195, 208 and 211, 362 and 364, and the `logger.Error(` argument at 368 to 370), which show that no deletion leaves a double blank line and that the retargeted line 369 is already a lone argument, so the `1	129` numstat row P6-T2 asserts after formatting does not depend on a CSharpier rewrite; `FEATURE/spec.md` line count 334 (Grep `^`) and the `acquired and released inside a held` lines 10, 105, 266 and 282; the plan's own lines 7 to 9, 22, 131, 138, 1050, 1395, 1500, 1554 to 1555, 1684 and 1688 (pre-edit numbering) and the sibling occurrences the sweep found and left unchanged (130 and 1710 are `QfcDatamodel.cs` line numbers, 143 and 1394 are prose and a token list, 563 and 639 are line arithmetic, 1449 and 1516 are interim values correct at their own tasks); and, in the round-4 pass, the three round-4 deltas — `QfcDatamodel.cs` 464 to 475 (`#endregion Email Queue Initial Setup` 467, `#region Linked List Locking` 469, blanks 470 and 471, `#endregion Linked List Locking` 472, `#region Event Handlers` 474); the Grep reproduction of the three CMD-LEGACY-CALLERS strategies (PRIMARY 25 lines in 5 files, LOG 3, CROSS 2) with every one of the 30 lines classified against the revised P4-T1 list (469 and 472 `REGION-DIRECTIVE`; no further category needed); the BASE anchoring of CMD-HUNKS and CMD-ADDED-SCAN, which leaves P6-T2 as the only task that re-runs a HEAD-anchored diff or a `??` expectation after the P6-T9 commit (P8-T45 runs after the check-offs and before P8-T46, on no restart path); the plan's own lines 7 to 9, 23, 155, 1501, 1556, 1685, 1689 and 1741 (pre-edit numbering) and the sibling occurrences the sweep found and left unchanged (1449, 1470, 1495, 1516 and 1533 are pre-commit census tasks; 149, 1252, 1429, 1431, 1435, 1517 and 1598 are the plan's other recorded-not-gated values, none restated as gated by P8-T6, P8-T8, P8-T20, P8-T27 or P8-T36); and, in the round-5 pass, the two round-5 deltas — the single occurrence of each old text (Grep: the P6-T1 sentence at line 1555 and the P6-T2 clause at line 1557, pre-edit numbering); the absence of `PRIOR-PASS-REWRITTEN` and `RESTART-CORRECTED` from the plan before the edit; the plan's own lines 7 to 9, 24, 1555, 1557, 1686 and 1690 (pre-edit numbering); every other `REWRITTEN` occurrence read and left unchanged (1541 is the P5-T8 `REWRITTEN TEST NOT SENSITIVE` stop label; 1559 is the P6-T3 current-pass condition on the production build; 1587 and 1601 are the P8-T1 and P8-T7 `REWRITTEN-WRITESET:` and `REWRITTEN-OTHER:` labels); and every `restart` occurrence read and left unchanged (156 is D-13, which already defines both restart paths the new labels refer to; 170, 1400, 1561 to 1569, 1587, 1597, 1601 and 1653 name a restart rule without reading the exemption); and, in the delta-application knock-on pass of the same round, the `RESTART-CORRECTED:` union wording — the four `RESTART-CORRECTED` occurrences (25, 1556, 1558 and 1691; no line was added, so the numbering is unchanged by the edit) with the label definition at 25 and 1556 changed to the union of every D-13 correction in this run and the P6-T2 consumer at 1558 and the D-13 text at 157 (156 in the pre-round-5 numbering the previous clause uses) read and left unchanged, because 1558 admits a file by label membership without restating how the label is derived and 157 bounds neither the number of Phase 6 restarts (`P6-RESTART: n`) nor the number of Phase 8 restarts to P6-T1 (`ITERATION`), so a run with two restarts is a state the plan already allows.

PLANNER-INTERNAL-REVIEW: PASS
CITATION-TO-TREE: PASS
AC-TRACEABILITY: PASS
SCOPE-BOUNDARY: PASS
CITATION: QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixture.cs | lines 12-31, 34-46, 92-104, 116-138, 146, 231, 243-274, 284-341
CITATION: QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixtureTests.cs | lines 33, 44-98, 107-149, 157-190, 192-276, 285
CITATION: QuickFiler.Test/Controllers/QfcItemController.FocusAndThemeTests.cs | lines 27, 98-117, 181-186, 193, 213, 235, 254, 314, 331, 367, 447-478
CITATION: QuickFiler.Test/Controllers/QfcItemController.TestSupport.cs | lines 85-96, 161-181, 216-239, 251-280, 282-305
CITATION: QuickFiler.Test/QuickFiler.Test.csproj | lines 17, 35, 155, 157, 161, 183, 196-215, 226-230
CITATION: QuickFiler.Test/SetupAssemblyInitializer.cs | lines 14-25
CITATION: QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part2.cs | line 124
CITATION: QuickFiler.Test/Controllers/QfcItemController.MailActionsTests.cs | line 203
CITATION: QuickFiler.Test/Controllers/QfcItemController.SeamFactoryTests.cs | line 13
CITATION: QuickFiler.Test/TestSupport/DedicatedWorkerThread.cs | lines 4, 21
CITATION: QuickFiler.Test/Controllers/QfcDatamodelLivenessTests.cs | lines 9, 12-13, 18-24, 47-61, 62-87, 100-164, 166-172, 174-209, 211-234, 236-270, 272-310
CITATION: QuickFiler.Test/Controllers/QfcDatamodelTeardownTests.cs | lines 12, 18-27, 59-74, 180, 220-222
CITATION: QuickFiler.Test/Controllers/QfcInitEmailQueueZeroBatchTests.cs | lines 12, 23-35, 93-100, 114-128, 136-154, 165-183, 195-230
CITATION: QuickFiler.Test/Controllers/QfcDatamodelTests.cs | lines 9, 12, 95-131, 201-211, 253-283
CITATION: QuickFiler.Test/Controllers/QfcFormControllerSeamTests.cs | lines 357-367
CITATION: QuickFiler.Test/Controllers/QfcHomeControllerRunAsyncTests.cs | lines 325, 370-380
CITATION: QuickFiler/Controllers/QfcDatamodel.cs | lines 25-26, 34-54, 77-103, 107-112, 128-152, 188-195, 197-241, 242-267, 271-315, 335-376, 377-416, 417-465, 467-474, 476-491
CITATION: QuickFiler/Controllers/QfcDatamodel.QueueProcessing.cs | lines 15-24, 37-43, 48-66, 146, 280-291, 299-311, 364, 404-411
CITATION: QuickFiler/Controllers/QfcDatamodel.FrameBuilding.cs | line 11
CITATION: QuickFiler/Controllers/QfcStreamingDequeueConfidenceGate.cs | lines 190-301
CITATION: QuickFiler/Controllers/QfcHomeController.cs | lines 92, 132, 344, 379
CITATION: QuickFiler/Interfaces/IQfcDatamodel.cs | lines 103, 117, 131, 138-148, 164, 166
CITATION: QuickFiler/Properties/AssemblyInfo.cs | line 5
CITATION: QuickFiler/Legacy/IAcceleratorCallbacks.cs | line 5
CITATION: QuickFiler/Controllers/QfcHighConfidencePreFilter.cs | line 11
CITATION: QuickFiler/Controllers/QfcItemController.FocusAndTheme.cs | lines 274-286
CITATION: UtilitiesCS/HelperClasses/ThemeHelpers/Theme.cs | lines 427-445
CITATION: UtilitiesCS/Threading/UiThread.cs | lines 266-285
CITATION: UtilitiesCS.Test/TestHelpers/ArmingBarrierTimeProvider.cs | lines 19-54
CITATION: scripts/vscode/Invoke-MSTestWithCoverage.ps1 | lines 89-93, 97-134, 262, 297-298, 348-355, 399-423, 430-453, 459-461
CITATION: scripts/vscode/Invoke-MSTestWithCoverage.FirstParty.ps1 | lines 117-123
CITATION: scripts/vscode/TaskMaster.cli.runsettings | lines 4-7
CITATION: scripts/hygiene/Test-RepositoryHygiene.Rules.ps1 | line 21
CITATION: scripts/vscode/Install-RepoDotNetSdk.ps1 | line 3
CITATION: scripts/vscode/Invoke-Restore.ps1 | lines 1-10
CITATION: .gitignore | lines 26, 140, 141, 146, 150, 151
CITATION: .gitattributes | line 4
CITATION: .csharpierignore | lines 4, 12
CITATION: global.json | lines 2-9
CITATION: dotnet-tools.json | line 6
CITATION: docs/features/active/2026-10-02-focus-and-theme-tests-leak-shared-dispatcher-setup-968/spec.md | lines 6-13, 56-79, 94-108, 138-171, 173-186, 237-270, 274-306, 308-318
CITATION: docs/features/active/2026-10-02-focus-and-theme-tests-leak-shared-dispatcher-setup-968/issue.md | lines 12, 65-77
CITATION: docs/features/active/2026-10-02-focus-and-theme-tests-leak-shared-dispatcher-setup-968/research/2026-10-02T05-50-dispatcher-pin-call-sites-research.md | sections 1.1, 2.1, 2.2, 3, 4, 5, 6, 7
CITATION: docs/features/active/2026-10-02-focus-and-theme-tests-leak-shared-dispatcher-setup-968/research/2026-10-02T22-20-qfc-datamodel-972-fold-research.md | sections 1 to 8 and Numeric Derivation Evidence
CITATION: docs/features/active/2026-10-02-focus-and-theme-tests-leak-shared-dispatcher-setup-968/evidence/other/preflight-round1-report.2026-10-02T08-40.md | defects 1 to 10
CITATION: docs/features/active/2026-10-02-focus-and-theme-tests-leak-shared-dispatcher-setup-968/evidence/other/preflight-round2-report.2026-10-02T23-56.md | defects 1 to 8
CITATION: docs/features/active/2026-10-02-focus-and-theme-tests-leak-shared-dispatcher-setup-968/evidence/other/preflight-round3-report.2026-10-03T01-01.md | defects 1 to 4 and advisory A1
CITATION: docs/features/active/2026-10-02-focus-and-theme-tests-leak-shared-dispatcher-setup-968/evidence/other/preflight-round4-report.2026-10-03T01-25.md | defects 1 to 3 (the advisory delta declined)
CITATION: docs/features/active/2026-10-02-focus-and-theme-tests-leak-shared-dispatcher-setup-968/evidence/other/preflight-round5-report.2026-10-03T01-53.md | defect 1 (deltas 1a and 1b) and the advisory
CITATION: docs/features/potential/promoted/2026-10-02-focus-and-theme-tests-leak-shared-dispatcher-setup.md | exists (Glob)
CITATION: docs/features/potential/promoted/2026-10-02-qfc-datamodel-950-review-residuals.md | exists (Glob)
AC-INVENTORY: AC1, AC2, AC3, AC4, AC5, AC6, AC7, AC8, AC9, AC10, AC11, AC12, AC13, AC14, AC15, AC16, AC17, AC18, AC19, AC20, AC21, AC22, AC23, AC24, AC25, AC26, AC27, AC28, AC29, AC30, AC31, AC32
AC-MAPPING: AC1 | IMPLEMENTATION: P1-T1, P2-T1 to P2-T5 | TESTS: P1-T5, P2-T8, P8-T5 | EVIDENCE: FEATURE/evidence/regression-testing/pass-after-pin-count.md
AC-MAPPING: AC2 | IMPLEMENTATION: P1-T1, P2-T4, P2-T5 | TESTS: P2-T8, P8-T5 | EVIDENCE: FEATURE/evidence/regression-testing/pass-after-pin-count.md
AC-MAPPING: AC3 | IMPLEMENTATION: P1-T1, P2-T5 | TESTS: P1-T6, P2-T8, P6-T5, P6-T2 | EVIDENCE: FEATURE/evidence/regression-testing/pass-after-pin-count.md
AC-MAPPING: AC4 | IMPLEMENTATION: P1-T1, P2-T5 | TESTS: P1-T6, P2-T8, P8-T5 | EVIDENCE: FEATURE/evidence/regression-testing/pass-after-pin-count.md
AC-MAPPING: AC5 | IMPLEMENTATION: P1-T1, P1-T2, P2-T1 to P2-T5 | TESTS: P1-T5, P2-T8 | EVIDENCE: FEATURE/evidence/regression-testing/fail-before-pin-count.md
AC-MAPPING: AC6 | IMPLEMENTATION: P1-T1 | TESTS: P1-T6, P6-T2 | EVIDENCE: FEATURE/evidence/qa-gates/post-format-census.md
AC-MAPPING: AC7 | IMPLEMENTATION: P3-T3, P3-T4 | TESTS: P6-T6, P6-T2 | EVIDENCE: FEATURE/evidence/regression-testing/focus-and-theme-class-pass-after.md
AC-MAPPING: AC8 | IMPLEMENTATION: P3-T3, P3-T4, P3-T8 | TESTS: P7-T1, P7-T2 | EVIDENCE: FEATURE/evidence/qa-gates/call-site-census.md
AC-MAPPING: AC9 | IMPLEMENTATION: P2-T1, P2-T4, P2-T5 | TESTS: P2-T6, P6-T2 | EVIDENCE: FEATURE/evidence/qa-gates/post-format-census.md
AC-MAPPING: AC10 | IMPLEMENTATION: P3-T7, P3-T8 | TESTS: P6-T5, P6-T2 | EVIDENCE: FEATURE/evidence/regression-testing/fixture-class-pass-after.md
AC-MAPPING: AC11 | IMPLEMENTATION: P2-T2, P2-T3, P2-T5 | TESTS: P6-T2 | EVIDENCE: FEATURE/evidence/qa-gates/post-format-census.md
AC-MAPPING: AC12 | IMPLEMENTATION: P3-T5 | TESTS: P6-T2 | EVIDENCE: FEATURE/evidence/qa-gates/post-format-census.md
AC-MAPPING: AC13 | IMPLEMENTATION: P3-T7 | TESTS: P6-T2 | EVIDENCE: FEATURE/evidence/qa-gates/post-format-census.md
AC-MAPPING: AC14 | IMPLEMENTATION: P3-T8 | TESTS: P6-T5, P6-T2 | EVIDENCE: FEATURE/evidence/regression-testing/fixture-class-pass-after.md
AC-MAPPING: AC15 | IMPLEMENTATION: P3-T1, P3-T2, P3-T6 | TESTS: P6-T6, P6-T2 | EVIDENCE: FEATURE/evidence/qa-gates/post-format-census.md
AC-MAPPING: AC16 | IMPLEMENTATION: P3-T3, P3-T4 | TESTS: P6-T2 | EVIDENCE: FEATURE/evidence/qa-gates/post-format-census.md
AC-MAPPING: AC17 | IMPLEMENTATION: P3-T5, P3-T6 | TESTS: P3-T9, P6-T2 | EVIDENCE: FEATURE/evidence/qa-gates/post-format-census.md
AC-MAPPING: AC18 | IMPLEMENTATION: P1-T1 to P5-T11, P6-T1 | TESTS: P8-T8 | EVIDENCE: FEATURE/evidence/qa-gates/file-line-counts.md
AC-MAPPING: AC19 | IMPLEMENTATION: P1-T1 to P5-T11 | TESTS: P7-T3 | EVIDENCE: FEATURE/evidence/qa-gates/prohibited-constructs-grep.md
AC-MAPPING: AC20 | IMPLEMENTATION: P6-T9 | TESTS: P8-T9 | EVIDENCE: FEATURE/evidence/qa-gates/footprint-scope.md
AC-MAPPING: AC21 | IMPLEMENTATION: P1-T2 | TESTS: P1-T4, P8-T5 | EVIDENCE: FEATURE/evidence/qa-gates/coverage-summary.md
AC-MAPPING: AC22 | IMPLEMENTATION: P8-T1 to P8-T5 | TESTS: P8-T5 | EVIDENCE: FEATURE/evidence/qa-gates/toolchain-final.md
AC-MAPPING: AC23 | IMPLEMENTATION: P0-T17, P8-T5 | TESTS: P8-T6 | EVIDENCE: FEATURE/evidence/qa-gates/coverage-comparison.md
AC-MAPPING: AC24 | IMPLEMENTATION: P1-T1 to P3-T8 | TESTS: P6-T7 | EVIDENCE: FEATURE/evidence/regression-testing/concurrent-set-test-summary.md
AC-MAPPING: AC25 | IMPLEMENTATION: P4-T2 to P4-T6 | TESTS: P4-T11, P6-T8, P6-T2 | EVIDENCE: FEATURE/evidence/qa-gates/post-format-census.md
AC-MAPPING: AC26 | IMPLEMENTATION: P5-T11 | TESTS: P5-T12, P6-T2 | EVIDENCE: FEATURE/evidence/qa-gates/queue-processing-comment-census.md
AC-MAPPING: AC27 | IMPLEMENTATION: P4-T1, P4-T8 | TESTS: P4-T10, P8-T3, P8-T4, P6-T2 | EVIDENCE: FEATURE/evidence/qa-gates/qfc-datamodel-legacy-callers.md
AC-MAPPING: AC28 | IMPLEMENTATION: P4-T8 | TESTS: P8-T8 | EVIDENCE: FEATURE/evidence/qa-gates/file-line-counts.md
AC-MAPPING: AC29 | IMPLEMENTATION: P4-T8 | TESTS: P6-T2, P8-T6 | EVIDENCE: FEATURE/evidence/qa-gates/coverage-comparison.md
AC-MAPPING: AC30 | IMPLEMENTATION: P4-T5, P4-T6, P4-T7, P5-T3, P5-T4 | TESTS: P6-T8, P6-T2 | EVIDENCE: FEATURE/evidence/qa-gates/post-format-census.md
AC-MAPPING: AC31 | IMPLEMENTATION: P5-T2, P5-T3, P5-T4 | TESTS: P5-T1, P5-T7, P5-T8, P5-T10, P6-T2 | EVIDENCE: FEATURE/evidence/regression-testing/liveness-sensitivity-check.md
AC-MAPPING: AC32 | IMPLEMENTATION: P4-T2 to P5-T11 | TESTS: P6-T8 | EVIDENCE: FEATURE/evidence/regression-testing/datamodel-set-test-summary.md
UNRESOLVED-GAPS: NONE

DIRECTIVE: PREFLIGHT VALIDATION ONLY
Executor preflight for this revision has not yet run; the signal below is the planner's request line for the confirming round, not a self-approval and not a discovered defect.
PREFLIGHT: REVISIONS REQUIRED

