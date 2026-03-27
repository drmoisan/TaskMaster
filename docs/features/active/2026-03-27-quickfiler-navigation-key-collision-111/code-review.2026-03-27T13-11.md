# Code Review — quickfiler-navigation-key-collision-111 (2026-03-27T13-11)

- **Feature folder:** `docs/features/active/2026-03-27-quickfiler-navigation-key-collision-111/`
- **Feature folder selection rule:** Used the expected active feature folder because it exists, matches issue suffix `-111`, and is the only local folder aligned to the requested QuickFiler duplicate-key review.
- **Base branch:** `main`

## Executive summary

This branch is not ready for PR review as the requested small-path QuickFiler duplicate-key fix relative to `main`. The authoritative `main...HEAD` diff contains no changes to `QuickFiler/Controllers/KbdActions.cs`, `QuickFiler/Controllers/QfcCollectionController.cs`, `QuickFiler.Test/Controllers/KbdActionsTests.cs`, or the `2026-03-27-quickfiler-navigation-key-collision-111` feature folder. Instead, the branch range contains merge/content for issue `#106` (`QfcQueue`) plus archived-doc moves. Separately, the sole `minor-audit` requirements source `issue.md` remains an unfilled template, and the active plan has checked items whose backing evidence does not satisfy the task acceptance literally.

The current branch does pass the repository C# QA loop: formatter check, analyzer build, nullable build, and MSTest with coverage all succeeded. That result lowers general code-health risk, but it does not rescue the feature-specific review because the requested fix is absent from the branch diff against `main`.

**Top 3 risks**

1. The branch content does not match the intended feature scope, so any PR opened from this branch to `main` would review and merge the wrong change set.
2. `issue.md` is the authoritative `minor-audit` requirements source, yet it contains placeholders instead of acceptance criteria for the duplicate-key bug.
3. The active plan overstates evidence completeness, which makes the existing plan and evidence artifacts unreliable for merge readiness decisions.

**PR readiness:** **No-Go** — remediation required before PR review.

## Findings

| Severity | File | Location | Finding | Recommendation | Rationale | Evidence |
|---|---|---|---|---|---|---|
| Blocker | Branch diff vs `main` | `git diff --name-status main...HEAD` whole range | The branch does not contain the requested QuickFiler duplicate-key implementation relative to `main`. The scoped diff for `KbdActions.cs`, `QfcCollectionController.cs`, `KbdActionsTests.cs`, and the active feature folder is empty. | Rebase/reset the branch or cherry-pick the intended QuickFiler duplicate-key fix so the `main...HEAD` diff contains only the scoped QuickFiler files and the matching feature-folder artifacts. | A feature review cannot approve a fix that is not present in the branch being reviewed. | `git diff --name-status main...HEAD`; `git diff --name-status main...HEAD -- QuickFiler/Controllers/KbdActions.cs QuickFiler/Controllers/QfcCollectionController.cs QuickFiler.Test/Controllers/KbdActionsTests.cs`; `git diff --name-status main...HEAD -- docs/features/active/2026-03-27-quickfiler-navigation-key-collision-111/**` |
| Blocker | `docs/features/active/2026-03-27-quickfiler-navigation-key-collision-111/issue.md` | `## Summary`, `## Steps to Reproduce`, `## Expected Behavior`, `## Actual Behavior`, `## Proposed Fix / Validation Ideas` | In `minor-audit` mode, `issue.md` is the sole requirements source, but it is still template content rather than a concrete issue specification for the duplicate-key fix. | Fill `issue.md` with the actual bug statement, reproduction, expected behavior, actual behavior, and explicit acceptance-criteria checkboxes for issue `#111`. Keep `spec.md` and `user-story.md` absent. | Without authoritative requirements in `issue.md`, the audit must fail closed and cannot mark feature acceptance PASS. | Direct inspection of `issue.md`; work-mode marker `- Work Mode: minor-audit` |
| Major | `docs/features/active/2026-03-27-quickfiler-navigation-key-collision-111/plan.2026-03-27T12-45.md` | `P0-T3` checklist row and acceptance text | The plan marks `P0-T3` complete even though the linked baseline artifact records that the exact planned formatter command failed with exit code `1`. | Update the plan so checked tasks only reflect schema-valid passing evidence, or regenerate the baseline with the actual supported formatter command and sync the checklist afterward. | Evidence-backed planning is a non-negotiable review requirement in this repository. | `plan.2026-03-27T12-45.md`; `evidence/baseline/p0-t3-format.2026-03-27T12-52.md` |
| Major | `docs/features/active/2026-03-27-quickfiler-navigation-key-collision-111/evidence/regression-testing/p1-t2-kbdactions-distinct-keys.2026-03-27T13-01.md` | `Output Summary` | The fail-before artifact does reproduce the duplicate-key exception, but only after an ad hoc fallback command because the approved focused MSTest script failed before executing tests. | Capture deterministic fail-before evidence with a working focused test invocation and then update the plan/checklist to match that verified command path. | The literal task acceptance for `P1-T2` was not met, so the current evidence chain is insufficient for a closed PASS audit. | `p1-t2-kbdactions-distinct-keys.2026-03-27T13-01.md` |
| Minor | Repository QA loop | Review-time QA commands | General branch health is good: formatter check, analyzer build, nullable build, and MSTest with coverage all passed. | Preserve the clean QA state when reconstructing the intended `#111` diff. | The remediation path should focus on scope and evidence integrity rather than speculative code cleanup. | Review-time commands run at 2026-03-27T13-11 |

## Typed Python audit

**N/A** — no Python files are in the requested feature scope, and the `main...HEAD` diff for the requested QuickFiler bug does not include Python changes.

## Test quality audit

### Strengths

- The current branch passes the repository MSTest coverage run with `2877` total tests, `2875` passed, `0` failed, and `2` skipped.
- The evidence folder for issue `#111` includes both baseline and QA-gate artifacts, so the intended feature workflow was at least documented.

### Blocking gaps

- The branch diff relative to `main` contains no `KbdActions`-scope tests to review for this feature.
- The fail-before regression evidence for `P1-T2` depends on a manual fallback after the approved script path failed before dispatching any tests.
- Because `issue.md` lacks explicit acceptance criteria for the duplicate-key bug, test completeness cannot be assessed against an authoritative minor-audit checklist.

## Security / correctness checks

- **Secrets:** No secrets or credentials were observed in the reviewed feature folder artifacts.
- **Unsafe subprocess usage:** None introduced in the requested QuickFiler scope, because that scope is absent from the `main...HEAD` diff.
- **Input validation:** Unreviewable for the requested feature in branch diff terms because the intended production changes are not present relative to `main`.
- **Branch correctness:** The dominant correctness issue is branch composition: the branch tip currently represents unrelated `QfcQueue` and archival work rather than the requested duplicate-key fix.

## Research log

None required. The review used repository-local policies, feature-folder evidence, direct git history, and fresh QA execution.

## Review conclusion

**No-Go for PR readiness.**

The current branch should not be opened or merged as the QuickFiler duplicate-key fix against `main`. Remediation must first align the branch content to issue `#111`, populate the sole `minor-audit` requirements source with real acceptance criteria, and repair the plan/evidence chain so it is truthful and reviewable.