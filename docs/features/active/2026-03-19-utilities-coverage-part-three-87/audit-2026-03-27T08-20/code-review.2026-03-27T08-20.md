# Code Review — utilities-coverage-part-three-87 (2026-03-27T08-20)

- **Feature folder:** `docs/features/active/2026-03-19-utilities-coverage-part-three-87/`
- **Feature folder selection rule:** Used the requested issue `#87` feature folder because it matches the authoritative `v2` scoping docs and refreshed PR-context artifacts.
- **Base branch:** `development`
- **Comparison source:** `artifacts/pr_context.summary.txt` and `artifacts/pr_context.appendix.txt`

## Executive summary

This clean branch contains substantial `UtilitiesCS.Test` expansion and a healthy review-time C# toolchain run, but it is still not ready for PR review against `development`. The main feature contract remains unsatisfied because the refreshed Cobertura report places the `UtilitiesCS` package at `69.79%` line coverage instead of the required `>= 80%`. The refreshed diff is also not fully isolated to issue `#87`, because it still includes `VBFunctions.Test` changes, issue `#96` feature-folder content, and stale historical audit artifacts.

**Top 3 risks**

1. The branch still fails the feature's main coverage acceptance criterion.
2. The diff scope is not fully limited to issue `#87`, which complicates safe PR review.
3. Historical and unrelated documentation content increases review noise and risks accidental promotion of stale evidence.

**PR readiness:** **No-Go**.

## Findings

| Severity | File | Location | Finding | Recommendation | Rationale | Evidence |
|---|---|---|---|---|---|---|
| Blocker | `coverage/coverage.cobertura.xml` | `<package ... name="UtilitiesCS">` | The refreshed `UtilitiesCS` package line-rate is `0.6978814543390189` (`69.79%`), below the required `>= 80%` threshold. | Continue targeted coverage work on the remaining low-coverage production files and rerun coverage verification. | The feature's primary acceptance criterion is coverage completion, not partial improvement. | `coverage/coverage.cobertura.xml`; `v2/spec.md`; `v2/user-story.md` |
| Blocker | `artifacts/pr_context.appendix.txt` | `Raw summary evidence` / `Representative changed files by scope` | The branch still contains non-`#87` content relative to `development`, including `VBFunctions.Test/**`, issue `#96` docs, and stale audit artifacts. | Rebase or cherry-pick the intended `#87` work onto a cleaner branch, or explicitly remove residual unrelated files before PR review. | A feature review should not approve an unnecessarily mixed branch. | `artifacts/pr_context.appendix.txt` |
| Major | `docs/features/active/2026-03-19-utilities-coverage-part-three-87/v2/plan.2026-03-22T21-00.md` | Phase 90 completion area | The current plan already documents that Phase 90 coverage verification came in below threshold, which confirms the unresolved state. | Keep the plan and status documents aligned with the unresolved coverage result until a new passing run exists. | Prevents reviewers from reading historical execution as successful delivery. | `v2/plan.2026-03-22T21-00.md` |
| Minor | `docs/features/active/2026-03-19-utilities-coverage-part-three-87/audit-2026-03-26T09-40/**` | historical audit bundle | The branch still carries stale audit output from the earlier mixed-branch review. | Remove or clearly segregate stale audit artifacts from active review output before PR review. | Stale audit content is confusing and can be mistaken for current review evidence. | `artifacts/pr_context.appendix.txt` |

## Typed Python audit

**N/A** — no Python files are in scope for this feature review.

## Test quality audit

### Strengths

- The branch adds or updates a large number of MSTest files under `UtilitiesCS.Test`.
- The review-time QA loop passed in repository order: format, analyzer build, nullable/type-safety build, and coverage-enabled MSTest.
- The test run is large and healthy: `3415` total tests with `3413` passed and `2` skipped.

### Remaining gaps

- The feature did not target toolchain health alone; it targeted complete coverage uplift to the documented threshold.
- The refreshed coverage data still shows materially under-covered files, including examples such as:
  - `UtilitiesCS\Dialogs\InputBox.cs`
  - `UtilitiesCS\Dialogs\MyBox.cs`
  - `UtilitiesCS\Dialogs\NotImplementedDialog.cs`
  - `UtilitiesCS\Dialogs\YesNoToAll.cs`
  - `UtilitiesCS\EmailIntelligence\EmailParsingSorting\SortEmail.cs`
  - `UtilitiesCS\Extensions\AsyncSerialization.cs`
  - `UtilitiesCS\EmailIntelligence\People\PeopleScoDictionaryNew.cs`
  - `UtilitiesCS\EmailIntelligence\ClassifierGroups\ManagerAsyncLazy.cs`
  - `UtilitiesCS\HelperClasses\ToolTips\TipsController.cs`
- Because the branch still carries unrelated diff content, it is harder to audit the true blast radius of the intended `#87` work.

## Security / correctness checks

- **Secrets:** No secrets were observed in the refreshed review scope or generated artifacts.
- **Unsafe subprocess usage:** None introduced by the reviewed feature scope.
- **Input validation / correctness:** The expanded tests improve confidence in dialogs, threading helpers, collections, wrappers, and classifier helpers, but the incomplete coverage result means important code paths remain unverified.

## Research log

No external research was needed. This review relied on repository policies, refreshed PR-context artifacts, the live coverage run, and the on-disk feature docs.

## Review conclusion

**No-Go.**

The branch demonstrates real progress and is technically healthy in the QA loop, but it does not yet satisfy the defining issue `#87` acceptance criterion and is not fully isolated relative to `development`. The next review should occur only after coverage is raised to the required threshold and unrelated diff content is removed.
