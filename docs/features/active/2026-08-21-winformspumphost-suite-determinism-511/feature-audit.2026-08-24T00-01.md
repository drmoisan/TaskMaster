# Feature Audit — winformspumphost-suite-determinism-511

- Timestamp: 2026-08-24T00-01 (UTC)
- Review cycle: re-audit after remediation cycle 1

## Scope and Baseline

- Base branch (resolved): `main`
- Merge base: `f85a36faebaaec29fe5233c9d9f69d223d80e4c5`
- Head: `b4d47adc369d021f6fb4eff092f419dc49e9a5e5` (branch `bug/winformspumphost-suite-determinism-511-exec`; verified equal to `git rev-parse HEAD`; working tree clean)
- Work mode: `full-bug` (persisted `- Work Mode: full-bug` marker in `issue.md`)
- Acceptance-criteria source: `docs/features/active/winformspumphost-suite-determinism-511/spec.md`, section `## Acceptance Criteria` — the sole authoritative source for this mode. `user-story.md` does not exist and is not required.
- Branch diff: 97 files — 3 C# test files under `QuickFiler.Test/Controllers/` (+124/-0), 68 files under the feature folder (spec re-scope, decision record, remediation inputs/plan, evidence tree), 26 files under `.claude/agent-memory/`.
- Context: issues #511 and #571 are CLOSED as NOT_PLANNED, superseded by #592 (open). #594 and #597 are open. The recorded decision (`decision-record.2026-08-23T20-40.md`) is that this branch does not claim to repair #511/#571; its delivered value is the fixture hardening, the two regression tests pinning the measured inherited handle state, and the mechanism finding.
- `origin/main` has advanced two commits past the merge base (PR #600, a 1,989-file documentation archive move). Verified non-intersecting with this branch's diff paths; the merge base remains a valid baseline for this audit.

## Acceptance Criteria Inventory

`spec.md` `## Acceptance Criteria` contains 14 checkbox criteria. At review time all 14 are
checked (`[x]`). Criteria 3 and 6 carry dated revision markers ("Revised 2026-08-23 per
remediation Finding F / Finding E"); the revisions were ratified through
`remediation-inputs.2026-08-23T20-57.md` and the maintainer decision record.

## Acceptance Criteria Evaluation

| # | Criterion (abbreviated) | Verdict | Evidence |
| --- | --- | --- | --- |
| 1 | `InitializeNineArgOverload_ThroughThePumpHost_SavesParametersAndDelegates` passes in 10/10 consecutive full nine-assembly runs, TRX evidence under `evidence/regression-testing/` | PASS | `named-tests-ten-runs.2026-08-21T18-10.md` (Passed 10/10, per-TRX). Note: the raw TRX were subsequently deleted at maintainer instruction after fidelity verification; the distilled record is the evidence of record (`raw-vstest-artifact-disposition.2026-08-23T21-40.md`). Wording drift recorded as CR-2, non-blocking. |
| 2 | `InitializeBool_ThroughThePumpHost_CompletesAndInitializesState` passes in the same 10/10 runs | PASS | Same records as AC 1. |
| 3 | Ten runs under induced CPU load with `/EnableCodeCoverage /InIsolation /TestCaseFilter:"TestCategory!=LiveOutlook"`; zero failures in `QuickFiler.Test`; run 5's single sibling `UtilitiesCS.Test` failure attributed to #594 (revised per Finding F) | PASS | `determinism-ten-runs.2026-08-21T18-10.md` (9/10 suite-green; run 5 failed=1, FQN matches the #594 flake); `load-generator-start/stop` records; `p4-t2-narrowing-rationale.2026-08-23T20-57.md`. The revised wording matches the measured record exactly; the narrowing follows the ratified owned-class precedent. |
| 4 | Empirical pre-fix baseline: per-run pass/fail of both named tests plus observed `IsHandleCreated`, across ten runs | PASS | `prefix-baseline.2026-08-21T18-10.md` — twenty-row measured table (10 class-filtered + 10 full-suite), every cell observed from TRX, `IsHandleCreated: true` on every run — the measurement that falsified the premise. |
| 5 | `BuildPumpHarness_ForcesTheViewerWindowHandleOnThePumpThread` exists in Part3.cs, asserts `IsHandleCreated` true, and passes | PASS | Present at `Part3.cs:301`; asserts `IsHandleCreated.Should().BeTrue()` and `InvokeRequired` false on the pump thread; Passed 10/10 (`regression-tests-ten-runs`) and in the final 6,459/6,459 run. |
| 6 | `BuildPumpHarness_DoesNotCreateTheWebViewChildHandles` exists, asserts the measured inherited state (both WebView2 children handle-created at construction via `ISupportInitialize.EndInit()`), and passes (revised per Finding E) | PASS | Present at `Part3.cs:356`; asserts both children `IsHandleCreated` true with mechanism-stating `because:` messages and a "Measured, not predicted" remarks block; Passed 10/10 and in the final run; measurement provenance in `webview-child-handle-measurement.2026-08-21T18-10.md`. |
| 7 | Zero diff hunks in `QfcItemController.Initialization.cs` and `QfcItemController.ViewerSetup.cs`; #230 de-exemption blocks and retained `[ExcludeFromCodeCoverage]` intact | PASS | Reviewer re-ran `git diff --numstat` on both files: zero output. `#230` literals present (8 occurrences in Initialization.cs; ViewerSetup.cs block at `:254`); `ExcludeFromCodeCoverage` retained (`ViewerSetup.cs:41`). |
| 8 | All 21 pump-host call sites pass in the final run (13 self-tests + 8 consumer tests) | PASS | `pumphost-selftests.2026-08-21T18-10.md` (13/13), `consumer-tests.2026-08-21T18-10.md` (8/8), corroborated by the final 0-failure suite run (`remediation-suite-run.2026-08-23T20-57.md`). |
| 9 | Diff lists exactly three code files, all under `QuickFiler.Test/`; no `QuickFiler/` file, no `*.csproj`, no `.claude/` path other than `.claude/agent-memory/` | PASS | Reviewer re-ran `git diff --name-only f85a36fa..b4d47adc`: code-file set is exactly the three named paths; prohibited counts all zero; `.claude/` paths are exclusively `agent-memory/`. Matches `scope-lock-after-comment-fix.2026-08-23T20-57.md`. |
| 10 | `QfcItemController_SeamFactoryTests` and `QfcItemController_InitializationTests` both pass in the same run; `UiThreadDispatcherGate` and `SwapUiThreadDispatcher` retain acquire-and-release structure | PASS | Final run 6,459/6,459 with 0 failures covers both classes; structure verified at `Part2.cs:51` (gate) and `:148` (swap helper; spec cites the pre-insertion line `:139` — drift noted as CR-5, structure intact). `gate-structure-part2` / `gate-serialization` records corroborate. |
| 11 | Every changed file under 500 lines (was 409 / 467 / 290) | PASS | Independently re-measured: 418 / 474 / 398. Matches `remediation-file-size-audit.2026-08-23T20-57.md`. |
| 12 | No `Thread.Sleep`, `Task.Delay`, `SpinWait`, retry loop, or raised timeout constant; `PumpTimeoutMs = 60000`, `TimeoutMs = 30000` unchanged | PASS | Reviewer added-line scan of the branch diff: only `[Timeout(PumpTimeoutMs)]` references to the existing constant; constants verified unchanged at 60000. `no-timing-hacks.2026-08-21T18-10.md` corroborates. |
| 13 | Five-step toolchain green in a single final pass; coverage captured under `evidence/qa-gates/`; `QuickFiler` line coverage >= pre-fix baseline | PASS | `remediation-clean-pass.2026-08-23T20-57.md` (single pass, 0 restarts); `remediation-coverage.2026-08-23T20-57.md` (85.59% repo line / 79.06% repo branch / 81.08% QuickFiler package); delta vs. baseline +0.15 pp package, +0.04 pp repo, no per-class regression (`remediation-coverage-delta`). |
| 14 | `## Rollout & Follow-up` records the visible-window half as out of scope with its re-attribution and names the filed follow-up issue | PASS | Spec `## Rollout & Follow-up` names #592 (superseding #511/#571), #594, and #597; the visible-window re-attribution to `UtilitiesCS.Test/Threading/ProgressViewer_Tests.cs` is recorded with the section stating no new issue remains to be filed. |

## Summary

- 14 of 14 acceptance criteria evaluate PASS. 0 FAIL, 0 PARTIAL, 0 UNVERIFIED.
- The two criteria revised this cycle (AC 3, AC 6) were revised through the ratified remediation
  process with dated markers, and the revised text matches the measured evidence exactly.
- Non-blocking residuals are recorded in `code-review.2026-08-24T00-01.md` (CR-1 stale Root Cause
  Analysis narrative; CR-2 AC-wording drift against the maintainer-instructed raw-TRX deletion;
  CR-5 stale line citation in AC 10).
- Blocking findings contributed by this feature audit: 0.
- The branch honours the recorded decision that it does not claim to repair #511 or #571: no
  closing keyword appears in any commit message in range, and the PR is to be opened against
  `main` without closing keywords for either issue.

## Acceptance Criteria Check-off

All 14 criteria were already checked (`[x]`) in `spec.md` by remediation cycle 1's executor
(tasks P4-T1 through P4-T6 of `remediation-plan.2026-08-23T20-57.md`), each with cited evidence
recorded in `evidence/other/ac-status-summary.2026-08-23T20-57.md`. This review verified each
check-off independently and confirms the checkbox state matches the PASS evaluations above. No
new check-offs were required and none were made; no criterion was un-checked.

### Acceptance Criteria Status
- Source: docs/features/active/winformspumphost-suite-determinism-511/spec.md
- Total AC items: 14
- Checked off (delivered): 14
- Remaining (unchecked): 0
- Items remaining: none
