# parallel-run-merge-gate-misparses-pr-number (Issue #691)

- Date captured: 2026-08-29
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/parallel-run-merge-gate-misparses-pr-number/ (Issue #691)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #691
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/691
- Last Updated: 2026-08-29
## Summary

The parallel-orchestrator's PR-merge gate extracts the PR number by scanning the entire shell command text for the first standalone digit run, instead of parsing it from the `gh pr merge` argument specifically. A command prefixed with `cd` into a worktree path that itself contains a standalone 4-digit number (e.g. a year-prefixed worktree directory) gets its PR number misread from the path, and the merge is denied.

## Environment

- OS/version: Windows, PowerShell
- Command/flags used: `cd C:\Users\DanMoisan\repos\TaskMaster-wt\2026-08-29T00-11 && gh pr merge --merge 688`
- Data source or fixture: parallel-orchestrator final report for run `bugs-635-440`, section "Corrections to things I told you earlier"

## Steps to Reproduce

1. Work from a worktree whose path contains a standalone digit run (e.g. `...\TaskMaster-wt\2026-08-29T00-11`).
2. Run a PR-merge command prefixed with a `cd` into that path, e.g. `cd C:\Users\DanMoisan\repos\TaskMaster-wt\2026-08-29T00-11 && gh pr merge --merge 688`.
3. Observe the merge gate parses the PR number as `2026` (the first standalone digit run in the full command text) rather than `688`.
4. The gate denies the command because PR `2026` is not the expected/pinned PR for the run.

## Expected Behavior

The gate should parse the PR number from the `gh pr merge` invocation specifically (e.g. the argument following `--merge`, or the PR number/URL argument), regardless of what other digit runs appear earlier in the command line.

## Actual Behavior

A bare `gh pr merge --merge 688` (no `cd` prefix) is accepted; the same command prefixed with `cd` into a path containing a standalone digit run is denied because the gate parsed the wrong number.

## Logs / Screenshots

- [ ] Attached minimal logs or screenshot
- Snippet: Reported verbatim by the parallel-orchestrator: "The merge gate has a real defect. It parses the PR number by scanning the entire command text for the first standalone digit run, so my `cd .../2026-08-29T00-11 && gh pr merge --merge 688` was read as PR `2026` and denied. The checkpoint was correct throughout. Bare commands work."

## Impact / Severity

- [ ] Blocker
- [ ] High
- [x] Medium
- [ ] Low

## Suspected Cause / Notes

Likely a regex or substring scan in the merge-gate hook (`.claude/hooks/**` or equivalent enforcement script) that matches any standalone digit sequence in the full command text rather than anchoring to the `gh pr merge` call and its argument. A workaround (issuing the bare `gh pr merge` command without a leading `cd`) exists, so this did not block the `bugs-635-440` run, but it will recur for any worktree path containing a standalone digit run (e.g. year-prefixed worktree directories, which this repo's worktree naming convention produces routinely).

## Proposed Fix / Validation Ideas

- [ ] Locate the merge-gate hook implementation and anchor its PR-number extraction to the `gh pr merge` argument (e.g. `--merge <N>`, `--squash <N>`, or a trailing PR number/URL) instead of a whole-command digit scan
- [ ] Add a regression case using a worktree path with a standalone digit run (e.g. a year-prefixed directory) prefixed via `cd &&` to the merge command
- [ ] Verify no equivalent whole-command digit scan exists elsewhere in the orchestration hook surface

## Next Step

- [ ] Promote to GitHub issue (bug-report template)
- [ ] Move to active fix folder / branch
