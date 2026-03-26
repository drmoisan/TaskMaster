# Code Review — utilities-coverage-part-three-87 (2026-03-26T09-40)

- **Feature folder:** `docs/features/active/2026-03-19-utilities-coverage-part-three-87/`
- **Feature folder selection rule:** Used the requested active feature folder for issue `#87`; it matches the refreshed PR-context scoping docs even though the branch diff contains unrelated work.
- **Base branch:** `development`
- **Comparison source:** refreshed `artifacts/pr_context.summary.txt` and `artifacts/pr_context.appendix.txt`

## Executive summary

This branch contains substantial and mostly well-evidenced `UtilitiesCS` test expansion work, but it is not PR-ready relative to `development`. The final QA artifacts are green at the toolchain level, yet the feature’s main acceptance criterion still fails: `UtilitiesCS` line coverage is only `69.81%`, not the required `>= 80%`. In parallel, the refreshed branch diff is far broader than the `#87` scope, pulling in unrelated `.codex`, `.github`, `QuickFiler`, `TaskMaster`, and other feature-folder changes.

**Top 3 risks**

1. The branch cannot be reviewed or merged cleanly because the diff against `development` spans unrelated work.
2. The primary coverage commitment in the feature docs is still unmet, so the branch would merge incomplete feature scope.
3. The checked `P90-T6` plan task conflicts with its own follow-up note, which may mislead future reviewers about actual completion state.

**PR readiness:** **No-Go** — remediation required.

## Findings

| Severity | File | Location | Finding | Recommendation | Rationale | Evidence |
|---|---|---|---|---|---|---|
| Blocker | `artifacts/pr_context.appendix.txt` | `Diff shortstat`, `Files by top-level path`, `Changed files (representative unrelated scope evidence)` | The branch diff relative to `development` is not isolated to issue `#87`; it contains unrelated `.codex`, `.github`, `QuickFiler`, `TaskMaster`, and additional feature-folder changes. | Rebase or cherry-pick the `#87` coverage work onto a clean branch from `development`, then refresh PR context and rerun review. | A feature review cannot safely approve a branch when the comparison baseline includes unrelated work. | `artifacts/pr_context.appendix.txt` |
| Blocker | `docs/features/active/2026-03-19-utilities-coverage-part-three-87/evidence/qa-gates/final-qc-test-coverage.md` | `Output Summary` | The feature’s primary requirement remains unmet: `UtilitiesCS line coverage: 69.81%`. | Continue coverage work on the remaining non-skip sub-80 files until the measured coverage reaches `>= 80%`, then rerun coverage verification. | The feature docs explicitly require every compiled `UtilitiesCS` file to meet the threshold or be documented as a skip candidate. | `final-qc-test-coverage.md`; `v1/evidence/qa-gates/final-coverage-verification.md`; `user-story.md`; `spec.md` |
| Major | `docs/features/active/2026-03-19-utilities-coverage-part-three-87/plan.2026-03-22T21-00.md` | `P90-T6` | `P90-T6` is checked even though its follow-up note states the measured coverage was below threshold. | Leave the item documented as historically executed if needed, but keep the feature status and review verdict explicit that acceptance is still incomplete until coverage is actually fixed. | A checked task that documents its own failure can confuse future reviewers and automation. | `plan.2026-03-22T21-00.md` |
| Minor | `docs/features/active/2026-03-19-utilities-coverage-part-three-87/spec.md` | front matter / Definition of Done | The spec correctly remains `In Progress`, but the combination of checked DoD items and the unresolved top-line coverage target makes completion state easy to misread. | Keep `Status: In Progress` until AC1 passes, and update completion-oriented docs only after the next successful coverage run. | This is documentation hygiene, not a runtime defect. | `spec.md`; `user-story.md`; `feature-audit.2026-03-26T09-40.md` |

## Typed Python audit

**N/A** — no Python files are in scope for this feature review.

## Test quality audit

### Strengths

- The branch adds and extends a large volume of MSTest coverage in `UtilitiesCS.Test`, matching repository test conventions.
- Final QA artifacts show the toolchain loop completes cleanly: formatter, analyzer build, nullable build, and coverage-enabled MSTest all exit successfully.
- Coverage has materially improved over the baseline, and repository-wide regression did not occur.

### Remaining gaps

- The feature goal was not incremental improvement alone; it was completion to the `>= 80%` threshold. The current evidence still lists many non-skip files below `80%`.
- The remaining uncovered set includes nontrivial production files such as `SortEmail.cs`, `SubjectMapEncoder.cs`, `DfDeedle.cs`, `QfcTipsDetails.cs`, `BayesianPerformanceMeasurement.cs`, `SubjectMapSco.cs`, `IntelligenceConfig.cs`, `ClassifierGroupUtilities.cs`, and several serialization/container types.
- Because the branch diff includes unrelated work, it is difficult to reason about the true blast radius of the `#87` change set.

## Security / correctness checks

- **Secrets:** No secrets were found in the reviewed `#87` feature docs or QA artifacts.
- **Unsafe subprocess usage:** No new subprocess patterns were required for the `#87` implementation itself.
- **Input and boundary behavior:** The expanded tests appear to improve behavior coverage across dialogs, COM wrappers, collections, serialization, and threading helpers.
- **Correctness blocker:** The unresolved coverage gate means the feature is still incomplete by its own documented contract.

## Research log

None required. This review used repository-local policies, refreshed PR-context artifacts, and on-disk QA evidence.

## Review conclusion

**No-Go.**

The branch shows real testing progress, but the current comparison against `development` is too broad and the feature’s headline acceptance criterion still fails. The next review should occur only after the `#87` work is isolated on a clean branch and coverage evidence demonstrates `UtilitiesCS >= 80%` with no remaining non-skip below-threshold files.