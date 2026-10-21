# Phase 0 — Policy Read Evidence ([P0-T1])

Timestamp: 2026-09-08T09-08
Command: Read tool (seven policy files, worktree-rooted absolute paths)
EXIT_CODE: 0
Output Summary: All seven policy files were read in the required order from the issue-810 worktree. No file was missing and no read failed.

Policy Order: CLAUDE.md, .claude/rules/general-code-change.md, .claude/rules/general-unit-test.md, .claude/rules/csharp.md, .claude/rules/quality-tiers.md, .claude/rules/tonality.md, .claude/rules/plan-acceptance-gates.md

Files read:

- `CLAUDE.md`
- `.claude/rules/general-code-change.md`
- `.claude/rules/general-unit-test.md`
- `.claude/rules/csharp.md`
- `.claude/rules/quality-tiers.md`
- `.claude/rules/tonality.md`
- `.claude/rules/plan-acceptance-gates.md`

## Notes carried forward into execution

- Toolchain order is format, analyzers, nullable, tests. Restart from format whenever a step fails or changes a file (CLAUDE.md section 8, `.claude/rules/csharp.md` toolchain section).
- `/t:Rebuild` is required for both msbuild gates; `/t:Build` can skip `CoreCompile` and exit 0 without running analyzers.
- `/p:Nullable=enable` must not be added to the nullable gate.
- 500-line ceiling applies to production code, test code, and reusable script files (`.claude/rules/general-code-change.md`, File Size Limit).
- Two coverage floor families are in force and conflict: `>= 80%` line in CLAUDE.md UT2, and `>= 85%` line plus `>= 75%` branch in `.claude/rules/general-unit-test.md` and `.claude/rules/quality-tiers.md`. Both are recorded separately at [P7-T8] per the plan; neither is silently preferred.
- New modules target `>= 90%` coverage (CLAUDE.md UT2, `.claude/rules/csharp.md`).
- MSTest, Moq, FluentAssertions are the required C# test stack (CUT1, CUT2).
- Tonality policy applies to every artifact this plan writes.
