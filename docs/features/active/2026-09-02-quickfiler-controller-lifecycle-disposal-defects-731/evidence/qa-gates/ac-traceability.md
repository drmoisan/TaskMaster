# Acceptance-criteria traceability — issue #731

Timestamp: 2026-09-03T15-18

Task: [P6-T1]
Issue: #731

## Scope

Work mode is `full-bug`, so the sole acceptance-criteria source is `docs/features/active/2026-09-02-quickfiler-controller-lifecycle-disposal-defects-731/spec.md`, section `## Acceptance Criteria`. That section holds 19 checkboxes at spec.md lines 217 through 235. AC identifiers are assigned by reading the section top to bottom: AC1 is the checkbox at line 217 and AC19 is the checkbox at line 235.

`user-story.md` does not exist and is not required in `full-bug` mode. `issue.md` is not an AC source in this mode.

All artifact paths below are relative to the feature folder; `EVIDENCE` abbreviates `docs/features/active/2026-09-02-quickfiler-controller-lifecycle-disposal-defects-731/evidence`.

## Traceability table

Nineteen rows, one per AC identifier from AC1 to AC19, with no identifier missing and none duplicated.

| AC | spec.md line | Plan tasks | Evidence artifact(s) |
|---|---|---|---|
| AC1 | 217 | [P1-T1], [P1-T2], [P1-T3] | No artifact named by the phase mapping. Verified in source: the per-owner comment sits immediately above the initializer at `QuickFiler/Controllers/QfcCollectionController.cs:84`, `QuickFiler/Controllers/QfcDatamodel.cs:104` and `QuickFiler/Controllers/QfcQueue.cs:41`, each citing the `BeforeItemMove` `FirstOrDefault` dispatch, the instance-scoped `UnhookAll`, and `issue #731 finding 1, issue #620`. |
| AC2 | 218 | [P1-T5] | `EVIDENCE/regression-testing/finding1-topology-pin-pass.md` |
| AC3 | 219 | [P5-T5], [P5-T9] | `EVIDENCE/qa-gates/mstest-coverage.md`, `EVIDENCE/qa-gates/scope-boundary.md` |
| AC4 | 220 | [P1-T4] | No artifact named by the phase mapping. Verified in source: the class comment at `QuickFiler/Helper Classes/EmailMoveMonitor.cs:17-21` describes the class's actual behaviour and a case-insensitive search of that file for `malfunction`, `disabled`, `does not work`, `broken` and `non-functional` returns 0 matches. |
| AC5 | 221 | [P2-T4] | `EVIDENCE/regression-testing/finding2-cleanup-pass-after.md` |
| AC6 | 222 | [P2-T4] | `EVIDENCE/regression-testing/finding2-cleanup-pass-after.md` — the `Cleanup_SourceContainsNoSynchronousWait` and `Cleanup_WithParkedConsumer_ReturnsWithoutWaiting` results |
| AC7 | 223 | [P2-T1], [P2-T3], [P2-T5] | `EVIDENCE/regression-testing/finding2-cleanup-fail-before.md`, `EVIDENCE/regression-testing/finding2-cleanup-pass-after.md` |
| AC8 | 224 | [P2-T6], [P5-T10] | `EVIDENCE/regression-testing/finding2-cleanup-pass-after.md` (`## Frozen file check`), `EVIDENCE/qa-gates/file-size-audit.md` |
| AC9 | 225 | [P3-T3] | No artifact named by the phase mapping. Verified in source: the sole constructor at `QuickFiler/Controllers/QfcRemainingQueueAdmission.cs:14-24` declares only `addToQueue`, `hookItem` and `removeFromQueue`, and the type declares only the three matching fields at `:10-12`. |
| AC10 | 226 | [P3-T4], [P3-T5], [P5-T4] | `EVIDENCE/qa-gates/msbuild-nullable.md` |
| AC11 | 227 | [P3-T1] | `EVIDENCE/regression-testing/finding3-admission-pin-pass-after.md` |
| AC12 | 228 | [P4-T5] | No artifact named by the phase mapping. Verified in source at `QuickFiler/Controllers/QfcCollectionController.cs`: declaration `:911`, `Interlocked.Increment` `:915`, sole read through `Volatile.Read` `:993`, `Interlocked.Decrement` `:1010`. |
| AC13 | 229 | [P4-T2] | `EVIDENCE/regression-testing/finding4-volatile-pass-after.md` |
| AC14 | 230 | [P4-T6] | `EVIDENCE/regression-testing/finding4-volatile-pass-after.md` — the `RemoveSpecificControlGroupAsync_ThrowAtFirstStatement_RestoresReentrancyCounter` and `RemoveSpecificControlGroupAsync_ThrowLaterInBody_RestoresReentrancyCounter` results |
| AC15 | 231 | [P5-T8] | `EVIDENCE/qa-gates/setupdisposal-coverage.md` |
| AC16 | 232 | [P1-T6], [P2-T2], [P4-T3], [P5-T5] | `EVIDENCE/qa-gates/mstest-coverage.md` |
| AC17 | 233 | [P5-T1], [P5-T2], [P5-T3], [P5-T4], [P5-T5] | `EVIDENCE/qa-gates/csharpier-format.md`, `EVIDENCE/qa-gates/csharpier-check.md`, `EVIDENCE/qa-gates/msbuild-analyzers.md`, `EVIDENCE/qa-gates/msbuild-nullable.md`, `EVIDENCE/qa-gates/mstest-coverage.md` |
| AC18 | 234 | [P5-T5] compared against [P0-T9] | `EVIDENCE/qa-gates/mstest-coverage.md`, `EVIDENCE/baseline/mstest-coverage.md` |
| AC19 | 235 | [P5-T9], [P5-T10] | `EVIDENCE/qa-gates/scope-boundary.md`, `EVIDENCE/qa-gates/file-size-audit.md` |

Row count: 19. Identifiers present: AC1, AC2, AC3, AC4, AC5, AC6, AC7, AC8, AC9, AC10, AC11, AC12, AC13, AC14, AC15, AC16, AC17, AC18, AC19 — 19 distinct values, none missing and none duplicated.

## AC17 conditional resolution

[P6-T18] makes the AC17 check-off conditional on the DEGRADED-RUN STATE MODEL, because AC17 states an absolute bar while the model admits rows in which the final pass is not clean. The condition is resolved here against the recorded artifacts rather than assumed.

| Conjunct | Recorded value | Source artifact | Met |
|---|---|---|---|
| Axis F resolved to row F-CLEAN, so [P5-T1] ran the repository-wide `csharpier format .` | `Axis F row taken: F-CLEAN`, selected by the `EXIT_CODE: 0` [P0-T6] recorded | `EVIDENCE/qa-gates/csharpier-format.md` | yes |
| [P5-T1] recorded `EXIT_CODE: 0` | `EXIT_CODE: 0` | `EVIDENCE/qa-gates/csharpier-format.md` | yes |
| [P5-T2] recorded `EXIT_CODE: 0` | `EXIT_CODE: 0` | `EVIDENCE/qa-gates/csharpier-check.md` | yes |
| [P5-T3] recorded `EXIT_CODE: 0` | `EXIT_CODE: 0` | `EVIDENCE/qa-gates/msbuild-analyzers.md` | yes |
| [P5-T4] recorded `EXIT_CODE: 0` | `EXIT_CODE: 0` | `EVIDENCE/qa-gates/msbuild-nullable.md` | yes |
| Axis C resolved to row C1, with [P5-T5] recording `EXIT_CODE: 0` and a failed count of 0 | `Axis C row: C1`, `EXIT_CODE: 0`, failed count 0 | `EVIDENCE/qa-gates/mstest-coverage.md` | yes |

All six conjuncts hold, so the check-off branch of [P6-T18] applies and AC17 is checked off. The leave-unchecked branch was not taken, so no note recording an unmet absolute bar is appended.

The `Absolute floor result:` value [P5-T5] records is not a conjunct of this check-off, per [P6-T18]. It is recorded as `PASS` in any case.

## Deviations recorded at check-off time

Two check-off tasks require a recorded note. Both are reproduced here.

**AC15 ([P6-T16]) — evidence kind.** spec.md line 231 names an `evidence/coverage` directory. That spelling is not in the canonical scheme defined by `.claude/skills/evidence-and-timestamp-conventions/SKILL.md`, which recognises only `baseline/`, `regression-testing/`, `qa-gates/`, `issue-updates/`, `other/` and `remediation-baseline/`. The finding-5 re-measurement is produced by the mandatory final QA gate, so its canonical kind is `qa-gates/`, and the artifact lives at `EVIDENCE/qa-gates/setupdisposal-coverage.md`. This substitution is recorded in the plan preamble as `EVIDENCE_LOCATION_OVERRIDE_REJECTED` and is non-overridable.

**AC16 ([P6-T17]) — file count.** spec.md line 232 says "Both new test files", anticipating two. Three files were registered with `<Compile Include>` entries, at `QuickFiler.Test/QuickFiler.Test.csproj:146` (`Controllers\QfcMoveMonitorTopologyTests.cs`), `:152` (`Controllers\QfcFormControllerCleanupTests.cs`) and `:140` (`Controllers\QfcCollectionControllerDefects468Tests.Volatile.cs`). The third is the deviation disclosed at the head of the plan: the issue-#731 finding-4 structural proxy could not be added to `QuickFiler.Test/Controllers/QfcCollectionControllerDefects468Tests.cs` because that file sits two lines below the 500-line ceiling, so it was added as a partial-class continuation of the same type in a separate file. All three are confirmed present in the built test assembly by appearing in the [P5-T5] run.

**AC18 ([P6-T19]) — public API surface.** `QfcRemainingQueueAdmission` is declared `internal sealed` at `QuickFiler/Controllers/QfcRemainingQueueAdmission.cs:8`, and its constructor is `internal` at `:14`, so the constructor signature change made by [P3-T3] is not a public API break.

## Check-off outcome

All 19 criteria are checked off in spec.md by [P6-T2] through [P6-T20]. No criterion took a leave-unchecked branch.
