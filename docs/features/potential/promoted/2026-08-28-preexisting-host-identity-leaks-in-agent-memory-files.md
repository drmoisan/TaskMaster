# preexisting-host-identity-leaks-in-agent-memory-files (Issue #685)

- Date captured: 2026-08-28
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/preexisting-host-identity-leaks-in-agent-memory-files/ (Issue #685)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #685
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/685
- Last Updated: 2026-08-28
## Summary

Several `.claude/agent-memory/**` files already committed to `main` (from unrelated, prior features/issues) contain a real account name and/or machine name in plain text, violating this repo's no-absolute-host-paths policy (`.claude/agent-memory/_shared_no_absolute_host_paths.md`).

## Environment

- OS/version: Windows, agent-memory files under `.claude/agent-memory/**`
- Command/flags used: `git grep -lia "<account>\|<machine>" -- .claude/agent-memory/`
- Data source or fixture: N/A

## Steps to Reproduce

1. Run a case-insensitive content grep for the real account name and machine name across `.claude/agent-memory/**` on `main`.
2. Observe multiple hits in files unrelated to the current branch's work.

## Expected Behavior

No committed artifact, including agent-memory files, should contain a real account or machine name — per the repo's own stated policy.

## Actual Behavior

At least 7 pre-existing agent-memory files on `main` currently contain the real account name and/or machine name in plain text:
- `.claude/agent-memory/atomic-executor/project_bash_heredoc_collapses_doubled_backslashes.md` (introduced by a feature-review commit, unrelated to #680)
- `.claude/agent-memory/epic-orchestrator/feedback_measure_whole_volume_before_blaming_worktrees.md`
- `.claude/agent-memory/feature-review/project_464-review-residuals.md`
- `.claude/agent-memory/feature-review/project_488-review-residuals.md`
- `.claude/agent-memory/orchestrator/angle-bracket-redaction-breaks-trx-xml.md`
- `.claude/agent-memory/orchestrator/bash-tool-collapses-double-backslash-in-sed.md`
- `.claude/agent-memory/orchestrator/collect-pr-context-lands-in-main-checkout.md`

All seven pre-date the branch that discovered them (issue #680) and were introduced by unrelated prior commits (issues #464, #488, #445, and various epic fan-in/orchestrator commits), so fixing them is out of scope for that branch's PR.

## Logs / Screenshots

- [ ] Attached minimal logs or screenshot
- Snippet: N/A — see the file list above; each file's `git log` shows the introducing commit predates and is unrelated to issue #680.

## Impact / Severity

- [ ] Blocker
- [ ] High
- [x] Low

## Suspected Cause / Notes

Discovered incidentally while remediating a host-identity leak specific to issue #680's own branch (a multi-pass evidence-sanitization remediation cycle that repeatedly found the same leak class recurring in newly-generated review/test artifacts). A repo-wide sweep at that time surfaced these unrelated, pre-existing hits. Likely cause: earlier sessions writing agent-memory notes did not consistently apply the `<repo-root>`/`<user>`/`<host>` placeholder convention documented in `.claude/agent-memory/_shared_no_absolute_host_paths.md`.

## Proposed Fix / Validation Ideas

- [ ] Unit coverage areas: N/A (documentation/memory-file content fix, not code)
- [ ] Integration scenario to retest: `git grep -lia "<account>\|<machine>" -- .claude/agent-memory/` returns zero hits repo-wide after the fix
- [ ] Manual verification notes: sanitize each listed file with the same `<repo-root>`/`<user>`/`<host>` placeholder substitution already used elsewhere in this repo's evidence hygiene fixes; consider a one-time repo-wide sweep of all of `.claude/agent-memory/**` rather than only the seven files identified here, since this list was found incidentally and may not be exhaustive.

## Next Step

- [ ] Promote to GitHub issue (bug-report template)
- [ ] Move to active fix folder / branch
