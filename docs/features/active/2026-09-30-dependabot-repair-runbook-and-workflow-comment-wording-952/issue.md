# dependabot-repair-runbook-and-workflow-comment-wording (Issue #952)

- Date captured: 2026-09-30
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/dependabot-repair-runbook-and-workflow-comment-wording/ (Issue #952)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #952
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/952
- Last Updated: 2026-09-30
- Work Mode: minor-audit

## Summary

The #929 review left two cosmetic defects in the Dependabot repair documentation and workflow. The runbook's sources line still cites "App ID location" although the procedure now records the Client ID, and the header comment in `dependabot-repair.yml` is about 150 characters long.

## Environment

- OS/version: n/a
- Python version: n/a
- Command/flags used: n/a
- Data source or fixture: n/a

## Steps to Reproduce

1. Read `docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/runbooks/github-app-installation-token.runbook.md` line 301.
2. Read `.github/workflows/dependabot-repair.yml` line 14.

## Expected Behavior

Line 301 reads "Client ID location", matching steps 10 and 22 and the YAML sample. The workflow header comment is wrapped to the repository's line length.

## Actual Behavior

Line 301 reads "Private key generation and App ID location (steps 10-12)". The workflow header comment is about 150 characters long.

## Acceptance Criteria

- [x] Line 301 of `docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/runbooks/github-app-installation-token.runbook.md` reads "Client ID location (steps 10–12)" in place of "App ID location (steps 10–12)", matching steps 10 and 22 and the YAML sample.
- [x] The token `App ID location` appears nowhere in that runbook (a fixed-string search returns zero matches).
- [x] The header comment block of `.github/workflows/dependabot-repair.yml` (the `# Credential:` paragraph, originally lines 13 through 16) is rewrapped so that no comment line in the file exceeds 100 characters, the width the other comment lines in the file use; the comment wording is unchanged.
- [x] No non-comment YAML content changes: the diff of `.github/workflows/dependabot-repair.yml` against `origin/main` adds and removes comment lines only (every added and removed line begins with `#` after the diff marker), and `scripts/dev-tools/run-actionlint.ps1` or the CI `actionlint` job passes.
- [ ] The modified-workflow-needs-green-run rule (`.claude/skills/feature-review-workflow/SKILL.md`) is satisfied for the changed workflow file: a green run of the CI workflow against the branch head, including its `actionlint` job that lints `.github/workflows/dependabot-repair.yml`, is recorded as evidence. `dependabot-repair.yml` is `workflow_run`-triggered, filtered to `dependabot/` head branches, and defines no `workflow_dispatch` trigger, so no run of that workflow itself can occur against this branch head; that constraint and the comment-only diff are recorded as the disposition for the rule.

## Logs / Screenshots

- [ ] Attached minimal logs or screenshot
- Snippet: #929 code review (Minor) and policy audit NB-5 (`docs/features/active/2026-09-28-package-manifest-consistency-residuals-929/`).

## Impact / Severity

- [ ] Blocker
- [ ] High
- [ ] Medium
- [x] Low

## Suspected Cause / Notes

The citation text drifted when #929 corrected the instructions. The instructions themselves are correct.

## Proposed Fix / Validation Ideas

- [ ] Reword line 301 and wrap the workflow comment.
- [ ] Confirm that actionlint still passes.

## Next Step

- [x] Promote to GitHub issue (bug-report template)
- [ ] Move to active fix folder / branch
