# Feature Audit - Issue #952

## Scope and Baseline

- Base: origin/main (34c2ed88cbb009f2f231453db87bc64d45a9bd51); branch head 1639eda78a39b6afc04e095713a5b7ea6c4e4db6
- Work mode: minor-audit; AC source is the `## Acceptance Criteria` section of `issue.md` only (no spec.md or user-story.md by design)
- Changed production-surface files: the installation-token runbook (1 line) and `.github/workflows/dependabot-repair.yml` (comment lines 13-17)

## Summary

Verdict: PASS with one PENDING item. AC1 to AC4 are verified PASS. AC5 is PENDING: the green CI run on the branch head can only exist after the pull request is opened; per the coordinator ruling this is not a defect and not blocking at this stage. Blocking findings: 0.

## Acceptance Criteria Inventory

1. AC1: runbook line 301 reads "Client ID location (steps 10-12)" in place of "App ID location".
2. AC2: the token `App ID location` appears nowhere in the runbook.
3. AC3: the workflow `# Credential:` paragraph is rewrapped so no comment line exceeds 100 characters, wording unchanged.
4. AC4: no non-comment YAML change (diff adds and removes comment lines only) and actionlint passes.
5. AC5: the modified-workflow-needs-green-run rule is satisfied for the changed workflow file (green CI run on the branch head recorded as evidence).

## Acceptance Criteria Evaluation

| AC | Verdict | Evidence |
|---|---|---|
| AC1 | PASS | `git diff origin/main` on the runbook shows line 301 changed from "App ID location" to "Client ID location (steps 10-12)". |
| AC2 | PASS | Grep for `App ID location` in the runbook returns 0 matches. |
| AC3 | PASS | Comment lines 13-17 are 5 lines, each at most 100 characters; a search for lines longer than 100 characters returns only non-comment lines (80, 106, 127, 132, 151). Word-for-word comparison of removed and added text shows unchanged wording. |
| AC4 | PASS | Diff read directly: 4 added, 3 removed, all beginning with `#`; executor evidence p2-t4 NONCOMMENT=0; actionlint 1.7.7 exit 0 with no output (p2-t2). |
| AC5 | PENDING | `dependabot-repair.yml` is `workflow_run`-triggered, filtered to `dependabot/` head branches, and has no `workflow_dispatch` trigger (p2-t8: 0 lines), so only a green CI run (including the `actionlint` job) on the branch head can satisfy the rule. That run will be produced by the pull request's own CI. Left unchecked. |

## Acceptance Criteria Check-off

- Already checked in issue.md and verified by this review: AC1, AC2, AC3, AC4.
- Newly checked by this review: none.
- AC5: left unchecked by design (PENDING).

### Acceptance Criteria Status

- Source: docs/features/active/2026-09-30-dependabot-repair-runbook-and-workflow-comment-wording-952/issue.md
- Total AC items: 5
- Checked off (delivered): 4
- Remaining (unchecked): 1
- Items remaining: AC5 (modified-workflow-needs-green-run; PENDING on the pull request CI run)

## Baseline Comparison

Before: runbook line 301 cited "App ID location" and the workflow comment contained a line of about 150 characters. After: both corrected; no other behavior changed.

## Follow-Ups (not filed)

- Confirm the CI run on the pull request head is green, including the `actionlint` job, and record it for AC5 before merge.
- Optional: align the secret name `DEPENDABOT_REPAIR_APP_ID` with Client ID terminology if the secret configuration is revisited.
