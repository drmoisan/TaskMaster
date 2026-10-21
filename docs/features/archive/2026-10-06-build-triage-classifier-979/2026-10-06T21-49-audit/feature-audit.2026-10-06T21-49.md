# Feature Audit — Issue #979

Timestamp: 2026-10-06T21-49
Requirements sources: `spec.md`, `user-story.md`

## Scope and Baseline

- Base branch: `main`.
- Merge base: `c76e830c18976221b5730f84b8d88aebbfc4f04b`.
- Reviewed head: `ca8b98d6a69cfbb38439571c2105cda3994ea8f0`.
- Primary context: refreshed `artifacts/pr_context.summary.txt`.
- Secondary context: refreshed `artifacts/pr_context.appendix.txt` and `git diff --check main...HEAD`.
- Requirements sources: `spec.md` and `user-story.md`, because issue #979 is `full-feature` work.

## Acceptance Criteria Inventory

The authoritative source files contain six checkbox criteria each. The review evaluates both files independently because the work mode is `full-feature`.

## Acceptance Criteria Evaluation

| Criterion | Status | Evidence |
| --- | --- | --- |
| Mined mail preserves nullable Triage values | PASS | Constructor, deep-copy, and JSON tests passed; implementation copies `IItemInfo.Triage`. |
| Only valid A/B/C labels train the classifier | PASS | Rebuild filter at `Triage.MinedMailRebuild.cs:49`; invalid-label tests passed. |
| Existing aggregate and token-base initialization is used | PASS | Focused rebuild tests passed and feature-method coverage is 93.55 percent or greater. |
| Persistence and manager replacement remain integrated | PASS | Focused persistence/replacement tests passed. |
| Ribbon command invokes the rebuild | FAIL | The command is visible and callback wiring passes, but it returns without rebuilding when the Triage engine is disabled and absent from `InboxEngines`. |
| Unit tests cover required behavior | PARTIAL | Required model and ribbon tests pass, but no test covers the disabled-engine execution path. |

### Acceptance Criteria Status

- Source: `docs/features/active/2026-10-06-build-triage-classifier-979/spec.md`; `docs/features/active/2026-10-06-build-triage-classifier-979/user-story.md`
- Total AC items: 12
- Checked off (delivered): 10
- Remaining (unchecked): 2
- Items remaining: the ribbon command invocation criterion and the unit-test coverage criterion require the remediation in `CR-979-1`.

## Coverage Exception Assessment

The user authorized a one-time issue-979 exception for aggregate repository coverage. The exception applies because final aggregate coverage is 65.2006 percent, below 80 percent but above the 65.1433 percent baseline, while feature-method coverage meets the stated 90 percent target. It does not apply to the failed ribbon behavior.

## Summary

**Overall Feature Readiness:** FAIL

- **PASS:** 8 criteria across the two source files.
- **PARTIAL:** 2 criteria across the two source files.
- **FAIL:** 2 criteria across the two source files.
- **UNVERIFIED:** 0 criteria.

The aggregate coverage exception is accepted for this issue. CR-979-1 prevents PR readiness.

## Acceptance Criteria Check-off

The source files already mark all criteria checked. This reviewer did not alter them because the ribbon-command and unit-test criteria evaluate as FAIL/PARTIAL. The later remediation execution must reconcile checkbox state against its completed evidence.

## Recommendation

No-go for PR creation until CR-979-1 is remediated, reviewed, and verified. The coverage exception removes the aggregate-coverage blocker only.
