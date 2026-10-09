# dependabot-repair-workflow-comments-predate-redirect-sync (Issue #986)

- Date captured: 2026-10-09
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/dependabot-repair-workflow-comments-predate-redirect-sync/ (Issue #986)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #986
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/986
- Last Updated: 2026-10-09
## Summary

The explanatory comments in `.github/workflows/dependabot-repair.yml` (lines 87-105) predate the unconditional binding-redirect sync pass added by issue #985 (`scripts/dependencies/BindingRedirectSync.psm1`). They state that the binding-redirect class is not reachable from the workflow_run trigger and that the app.config reconciliation pass never runs, which is no longer true: the sync pass now rewrites stale transitive redirects on every run and reports through a separate `RedirectSync` field and `WrittenPath`. The behavior is correct; only the comments mislead a reader.

## Environment

- OS/version: GitHub Actions `windows-latest`
- Python version: n/a
- Command/flags used: `dependabot-repair.yml` "Repair package manifest consistency" step
- Data source or fixture: issue #985 code review (`code-review.2026-10-09T14-55.md`, non-blocking item)

## Steps to Reproduce

1. Read `.github/workflows/dependabot-repair.yml` lines 87-105 after issue #985 merges.
2. Compare with `Repair-PackageManifestConsistency.ps1`, which now invokes the redirect sync pass unconditionally.

## Expected Behavior

The workflow comments describe the current write classes: analyzer repairs (repair records), manifest normalisation, the `-CandidateUpgrade` reconciliation pass (still unreachable from this trigger), and the unconditional redirect sync pass (reported in `RedirectSync`, written files counted by `WrittenPath`, not counted in `beyond-known-weak`).

## Actual Behavior

The comments say the app.config reconciliation pass never runs from this trigger and do not mention the redirect sync pass.

## Logs / Screenshots

- [ ] Attached minimal logs or screenshot
- Snippet: `# The binding-redirect class is not reachable from the workflow_run trigger`

## Impact / Severity

- [ ] Blocker
- [ ] High
- [ ] Medium
- [x] Low

## Suspected Cause / Notes

Issue #985 deliberately excluded `.github/workflows/**` from its write set: the `modified-workflow-needs-green-run` rule cannot be satisfied for this workflow before merge, because it runs only on `dependabot/` branches and has no manual trigger.

## Proposed Fix / Validation Ideas

- [ ] Update the comment block at lines 87-105 to describe the redirect sync pass; comment-only change.
- [ ] actionlint passes; the green-run requirement is satisfied by the next Dependabot PR's repair run.
- [ ] Manual verification notes: none.

## Next Step

- [ ] Promote to GitHub issue (bug-report template)
- [ ] Move to active fix folder / branch
