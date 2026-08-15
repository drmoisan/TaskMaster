# mstest-coverage-discovery-claude-worktree-exclusion (Issue #531)

- Date captured: 2026-08-10
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/mstest-coverage-discovery-claude-worktree-exclusion/ (Issue #531)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #531
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/531
- Last Updated: 2026-08-11
## Summary

`Invoke-MSTestWithCoverage.ps1` test-assembly discovery lacks a `\.claude\` exclusion, so running with `-SearchRoot .` descends into agent worktrees and picks up stale sibling-worktree assemblies.

## Environment

- OS/version: Windows 11 Pro 10.0.26200
- Python version: n/a (PowerShell)
- Command/flags used: `scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot .`
- Data source or fixture: main checkout `C:\Users\DanMoisan\repos\TaskMaster` containing `.claude\worktrees\agent-*\`

## Steps to Reproduce

1. Create at least one agent worktree under `.claude\worktrees\agent-*\` and build it so it contains `*.Test.dll` assemblies.
2. From the main checkout, run `scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot .`.
3. Inspect the discovered assembly list and the resulting coverage figure.

## Expected Behavior

Discovery should enumerate only the current checkout's test assemblies and ignore any path under `.claude\worktrees\`.

## Actual Behavior

`scripts/vscode/Invoke-MSTestWithCoverage.ps1:296-302` filters discovered `*.Test.dll` paths on `\bin\<Configuration>\`, `\obj\` and `\ref\` only. There is no `\.claude\` guard, so discovery descends into `.claude\worktrees\agent-*\**` and picks up stale sibling-worktree assemblies, producing bogus `AssemblyInitialize` signature failures and a coverage figure computed over the wrong assembly set.

## Logs / Screenshots

- [x] Attached minimal logs or snippet
- Snippet: bogus `AssemblyInitialize` signature failures originating from assemblies under `.claude\worktrees\agent-*\`.

## Impact / Severity

- [ ] Blocker
- [x] High
- [ ] Medium
- [ ] Low

Local coverage runs report a figure computed over the wrong assembly set, and spurious failures obscure real ones.

## Suspected Cause / Notes

The discovery filter predates the agent-worktree layout under `.claude\worktrees\`. Suggested fix: add `-and $_.FullName -notmatch '\\\.claude\\'` to the existing discovery filter. Note that the exclusion must be applied to the path relative to the search root, so that a legitimate run *inside* an agent worktree still discovers its own assemblies.

Deliberately out of scope for #441 / #478: that is a production behaviour change to a file those issues do not otherwise touch, and #441's AC-18 pins its diff to exactly two source files. Recorded as follow-up candidate 3 in `docs/features/active/2026-08-10-cobertura-coverage-arithmetic-441/spec.md` § Rollout & Follow-up.

## Proposed Fix / Validation Ideas

- [x] Unit coverage areas: a Pester test over the discovery filter asserting that a `.claude\worktrees\` path is excluded and an in-worktree relative path is retained.
- [x] Integration scenario to retest: run with `-SearchRoot .` from the main checkout with at least one populated agent worktree present.
- [x] Manual verification notes: confirm the discovered assembly count matches the checkout's own test projects.

## Next Step

- [x] Promote to GitHub issue (bug-report template)
- [ ] Move to active fix folder / branch
