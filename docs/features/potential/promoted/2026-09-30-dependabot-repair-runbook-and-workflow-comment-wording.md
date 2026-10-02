# dependabot-repair-runbook-and-workflow-comment-wording (Issue #952)

- Date captured: 2026-09-30
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/dependabot-repair-runbook-and-workflow-comment-wording/ (Issue #952)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #952
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/952
- Last Updated: 2026-09-30
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
