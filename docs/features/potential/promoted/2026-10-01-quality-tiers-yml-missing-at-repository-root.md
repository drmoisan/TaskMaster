# quality-tiers-yml-missing-at-repository-root (Issue #967)

- Date captured: 2026-10-01
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/quality-tiers-yml-missing-at-repository-root/ (Issue #967)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #967
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/967
- Last Updated: 2026-10-02
## Summary

`.claude/rules/quality-tiers.md` states that `quality-tiers.yml` at the repository root maps every project to a tier, and that adding a project without a tier classification fails CI. The file does not exist on `main`, and no CI stage validates it. Feature reviews for #931 and #956 both flagged it as missing.

## Environment

- OS/version: n/a
- Python version: n/a
- Command/flags used: `git ls-tree origin/main -- quality-tiers.yml` (no result)
- Data source or fixture: repository tree

## Steps to Reproduce

1. Look for `quality-tiers.yml` at the repository root on `main`. It is absent.
2. Look for a `tier-classification` CI stage. There is none.

## Expected Behavior

Either the file exists, listing every project and its tier and validated in CI, or the rule is reconciled with this repository's actual policy.

## Actual Behavior

The rule references a file and a CI stage that do not exist.

## Logs / Screenshots

- [ ] Attached minimal logs or screenshot
- Snippet: #931 feature review ("`quality-tiers.yml` is absent at the repository root"); PR #965 follow-ups.

## Impact / Severity

- [ ] Blocker
- [ ] High
- [ ] Medium
- [x] Low

## Suspected Cause / Notes

`.claude/rules/**` is pushed down from drm-copilot. The #178 governance sync deliberately kept TaskMaster's own coverage and tier policy and excluded the reference repository's tier system, so this rule may itself be the leak. Related to #668 (the coverage-threshold discrepancy between CLAUDE.md and the rules). Decide between adding the file plus a CI stage here and fixing the rule upstream under #932.

## Proposed Fix / Validation Ideas

- [ ] Maintainer decision: adopt tiers in TaskMaster, or reject the rule here.
- [ ] If adopted: add `quality-tiers.yml` covering all `.sln` projects, plus a CI check, with a negative control for an unclassified project.
- [ ] If rejected: raise the upstream change under #932.

## Next Step

- [x] Promote to GitHub issue (bug-report template)
- [ ] Move to active fix folder / branch
