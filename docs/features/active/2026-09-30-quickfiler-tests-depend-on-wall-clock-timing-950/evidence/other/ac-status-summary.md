# Acceptance-criteria status summary (P6-T29)

Timestamp: 2026-10-02T01-26

### Acceptance Criteria Status
- Source: docs/features/active/2026-09-30-quickfiler-tests-depend-on-wall-clock-timing-950/spec.md (Work Mode full-bug; spec.md is the only acceptance-criteria source)
- Total AC items: 17
- Checked off (delivered): 16 (lines beginning `- [x] AC`, counted after P6-T28)
- Remaining (unchecked): 1 (lines beginning `- [ ] AC`)
- Items remaining:
  - AC17: NOT MET (ENVIRONMENTAL: COVERAGE-ROUTE DIRECT). The P0-T15 stall probe recorded STALL-PROBE: REPRODUCES because UtilitiesCS.Test ShellUtilitiesStatic_Tests.GetFileIcon_WithUseFileType_ShouldReturnIconsForDirectoryAndFileExtension failed on this workstation ("Win32 handle that was passed to Icon is not valid or is the wrong type"). Under D-6 the test step therefore ran the runner's inner dotnet-coverage collector with the four shell-icon classes excluded, plus the runner's own post-processing, rather than scripts\vscode\Invoke-MSTestWithCoverage.ps1 verbatim. Every other step of the final toolchain passed in one iteration (SINGLE-PASS: YES), and the DIRECT run passed 7361 of 7361 tests with both coverage floors met (FEATURE/evidence/qa-gates/toolchain-final-pass.md). The coordinator rules on AC17.

## Per-criterion evidence
| AC | Status | Evidence |
|---|---|---|
| AC1 | MET | qa-gates/post-format-census.md (property 1, constructor default 2, documentation tokens 1 and 1) |
| AC2 | MET | qa-gates/post-format-census.md (INITQ RunWorkerAsync 0, WorkerStarter(worker); 2) |
| AC3 | MET | qa-gates/post-format-census.md (QfcDatamodel.cs 495 lines) |
| AC4 | MET | qa-gates/wall-clock-tokens.md (all counts 0, TIMESPAN-UNCLASSIFIED: 0) |
| AC5 | MET | qa-gates/prohibited-constructs.md (prohibited added tokens 0, runsettings unchanged, [Timeout] 8 and constant 1) |
| AC6 | MET | regression-testing/targets-pass-after.md and qa-gates/coverage-post-change.md (Passed); census StartSynchronously 2 |
| AC7 | MET | regression-testing/targets-pass-after.md and qa-gates/coverage-post-change.md (Passed) |
| AC8 | MET | same two artifacts (Passed); census pump.Drain(); 2 |
| AC9 | MET | same two artifacts (Passed) |
| AC10 | MET | same two artifacts (Passed); teardown WaitForState 0 and .Wait( 0 |
| AC11 | MET | same two artifacts (Passed); zero-batch .Wait( 0 |
| AC12 | MET | same two artifacts (both tests Passed); zero-batch StartSynchronously 3 |
| AC13 | MET | targets-pass-after.md, concurrent-classes-pass-after.md and coverage-post-change.md (Passed); R4 spans 1,1,2,1,1 / 0,0 / 1,1 / 3 |
| AC14 | MET | qa-gates/post-format-census.md (flake-watch 0, Append an observation 0, Issue #950: 1, three R-DOC tokens 1 each) |
| AC15 | MET | other/negative-controls-summary.md (nine AC15 rows, two Drain-dependency rows) |
| AC16 | MET | other/ambient-synchronization-context.md (AMBIENT-SYNCHRONIZATION-CONTEXT: null) |
| AC17 | NOT MET | qa-gates/toolchain-final-pass.md (AC17-STATUS: NOT MET, COVERAGE-ROUTE DIRECT) |

## Spec check-off diff

Timestamp: 2026-10-02T01-27
Task: P6-T31
Command: git -C WORKTREE diff --numstat HEAD -- docs/features/active/2026-09-30-quickfiler-tests-depend-on-wall-clock-timing-950/spec.md; git -C WORKTREE diff HEAD -- docs/features/active/2026-09-30-quickfiler-tests-depend-on-wall-clock-timing-950/spec.md; git -C WORKTREE status --porcelain -- docs/features/active/2026-09-30-quickfiler-tests-depend-on-wall-clock-timing-950/spec.md (separate calls)
EXIT_CODE: 0

Output Summary:
numstat: 16	16	docs/features/active/2026-09-30-quickfiler-tests-depend-on-wall-clock-timing-950/spec.md (CRLF normalisation warning only)
Diff: one hunk at spec.md lines 263 to 284. The sixteen deleted lines are `- [ ] AC1:` through `- [ ] AC16:` and the sixteen added lines are `- [x] AC1:` through `- [x] AC16:`, each with the remaining criterion text identical. `- [ ] AC17:` is unchanged context.
porcelain:  M docs/features/active/2026-09-30-quickfiler-tests-depend-on-wall-clock-timing-950/spec.md

Added and deleted counts are equal (16) and equal the checked-off count (16). spec.md changed only in its checkbox lines.
