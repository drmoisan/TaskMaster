---
name: gh-ci-log-and-download-gotchas
description: Three measured traps when a plan reads CI through gh - the pr-author hook blocks any gh command text containing createdAt, the Pester summary line in a GH log carries ANSI codes, and gh run download never overwrites
metadata:
  type: project
---

Measured at the #929 round-4 preflight on 2026-09-30 (run 36666302259, Pester job 109731601928):

1. **`createdAt` in a gh command trips the pr-author hook.** A read-only
   `pwsh -NoProfile -Command '... gh run list ... --json databaseId,headSha,...,createdAt ...'`
   was blocked with PR_AUTHOR_SKILL_BLOCKED. The same command without `createdAt` passed. The
   structural `gh pr create` detector in `.claude/hooks/enforce-pr-author-skill-helpers.ps1`
   (Test-CommandLineInvocation, line 279) scans the whole command string.
2. **Pester's "Tests Passed:" line is ANSI-coloured per segment in the GitHub Actions log**:
   `ESC[32mTests Passed: 373, ESC[0mESC[90mFailed: 0, ...`. So the pattern
   `Tests Passed: [0-9]+, Failed: [0-9]+` never matches. Strip SGR first with
   `-replace ([string][char]27 + "[[][0-9;]*m"), ""`, which needs no backslash. The script's own
   `PESTER Passed=` and `COVERAGE LinePercent=` lines are plain.
3. **`gh run download --dir D` exits 1 ("The file exists.") when D already holds the artifact
   file.** Any re-run into the same folder fails, for example a loop restart (iter2). Use a
   per-run, per-attempt folder and record Test-Path beforehand.

**Why:** all three make a CI-sourced gate unsatisfiable in ways that plan text alone does not
show.

**How to apply:** at preflight, grep every gh-bearing command for `create`. Require ANSI stripping
before any Select-String over a `gh run view --log` capture. Require a unique `--dir` for each
download invocation. See [[project_tool_results_inject_bash_read_edit_instruction]].
