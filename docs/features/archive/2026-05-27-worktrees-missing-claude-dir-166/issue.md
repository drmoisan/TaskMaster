# worktrees-missing-claude-dir (Issue #166)

- Date captured: 2026-05-27
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/worktrees-missing-claude-dir/ (Issue #166)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #166
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/166
- Last Updated: 2026-05-27
- Work Mode: minor-audit

## Summary

The `.claude/` agentic environment directory is excluded by `.gitignore`, so it is not tracked by git. Git worktrees created for background tasks only check out tracked files, so those worktrees lack the agents, hooks, rules, skills, and project settings required for the agentic toolchain to function.

## Environment

- OS/version: Windows 11 Pro 10.0.26200
- Python version: N/A (repository tooling defect, not language-specific)
- Command/flags used: `git worktree add <path>`
- Data source or fixture: repository `.gitignore` and `.claude/` directory

## Steps to Reproduce

1. Confirm `.gitignore` contains a `.claude` ignore entry (currently the final line).
2. Create a new git worktree from the repository (the mechanism used to run background tasks).
3. Inspect the new worktree for the `.claude/` directory.

## Expected Behavior

The new worktree contains the `.claude/` agentic environment (agents, hooks, rules, skills, project `settings.json`) so background tasks run with the required tooling.

## Actual Behavior

The `.claude/` directory is absent from the worktree because it is untracked and ignored. Background tasks run without the required agentic tooling.

## Logs / Screenshots

- [x] Attached minimal logs or screenshot
- Snippet:
  - `git check-ignore .claude` returns `.claude` (directory is ignored).
  - `git ls-files .claude` returns no output (nothing tracked).

## Impact / Severity

- [ ] Blocker
- [ ] High
- [x] Medium
- [ ] Low

## Suspected Cause / Notes

`.gitignore` ignores `.claude`, so the directory is never committed and therefore never materializes in git worktrees. Related prior work: Issue #149 (`push_down_claude_dir`) established that when bundling `.claude/`, `settings.local.json` and `agent-memory/` must be excluded. The fix should preserve those exclusions.

## Proposed Fix / Validation Ideas

- [ ] Unit coverage areas: not applicable; `.gitignore` behavior is verified with `git check-ignore`, not the MSTest suite (an external git process is prohibited in unit tests).
- [ ] Integration scenario to retest: create a fresh worktree and confirm `.claude/` tooling is present while `settings.local.json` and `agent-memory/` remain excluded.
- [ ] Manual verification notes: `git check-ignore .claude/skills` returns nothing (tracked); `git check-ignore .claude/settings.local.json` and `git check-ignore .claude/agent-memory` still report ignored.

## Next Step

- [x] Promote to GitHub issue (bug-report template)
- [x] Move to active fix folder / branch

## Resolution (implemented 2026-05-27)

Implemented fix: edited the repository-root `.gitignore` only. Removed the bare
final-line `.claude` entry and replaced it with targeted ignores so the `.claude/`
agentic environment (`agents/`, `hooks/`, `rules/`, `skills/`, `settings.json`) is
tracked and materializes in git worktrees:

```gitignore
# .claude/ agentic environment is tracked so it materializes in git worktrees (Issue #166).
# Keep per-developer and per-agent state out of version control (Issue #149).
.claude/settings.local.json
.claude/agent-memory/
```

Issue #149 invariant preserved: `.claude/settings.local.json` (per-developer local
settings) and `.claude/agent-memory/` (per-agent learned state) remain git-ignored.
This was verified pre- and post-fix.

Documented exceptions (not skipped requirements):
- No MSTest/unit regression test applies. `.gitignore` behavior is a git-process
  property; the General Unit Test Policy (UT4) and C# Unit Test Policy prohibit
  external processes and temporary files in unit tests. The regression is adapted to
  deterministic `git check-ignore` command verification.
- C# toolchain N/A. No `*.cs`, `*.csproj`, `*.props`, or `*.targets` files changed,
  so CSharpier, msbuild analyzer/nullable builds, and vstest coverage are not
  applicable. Repository-wide C# coverage is unaffected.

Validation evidence (under `docs/features/active/2026-05-27-worktrees-missing-claude-dir-166/evidence/`):
- Phase 2 pre-fix defect proof: `regression/166-pre-fix-check-ignore.txt`
  (tooling subtrees and the Issue #149 paths were all ignored before the fix).
- Phase 4 post-fix tooling allowed: `qa/166-post-fix-check-ignore-allowed.txt`
  (tooling subtrees no longer ignored; exit code 1, no output).
- Phase 4 post-fix invariant preserved: `qa/166-post-fix-check-ignore-still-ignored.txt`
  (`settings.local.json` and `agent-memory/` still ignored; exit code 0).
- Phase 4 dry-run staging: `qa/166-git-add-dryrun.txt`
  (tooling staged; the two excluded items absent).
- Phase 4 toolchain determination: `qa/166-toolchain-summary.txt`.

Note: Committing the now-tracked `.claude/` content is performed later by the
orchestrator pre-review `git add -A` step, not by this `.gitignore` change.