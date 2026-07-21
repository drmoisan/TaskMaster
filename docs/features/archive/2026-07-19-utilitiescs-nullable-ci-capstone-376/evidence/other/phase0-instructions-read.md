# Phase 0 — Policy Instructions Read

Timestamp: 2026-07-19T05-00

Policy Order:
1. `CLAUDE.md`
2. `.claude/rules/general-code-change.md`
3. `.claude/rules/general-unit-test.md`
4. `.claude/rules/csharp.md`
5. `.claude/rules/ci-workflows.md`
6. `.claude/rules/benchmark-baselines.md`

Files read (in the order above, in full):
- `C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-a990f1a3b96eb6fae\CLAUDE.md`
- `C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-a990f1a3b96eb6fae\.claude\rules\general-code-change.md`
- `C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-a990f1a3b96eb6fae\.claude\rules\general-unit-test.md`
- `C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-a990f1a3b96eb6fae\.claude\rules\csharp.md`
- `C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-a990f1a3b96eb6fae\.claude\rules\ci-workflows.md`
- `C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-a990f1a3b96eb6fae\.claude\rules\benchmark-baselines.md`

Also read for feature context (per delegation directive, not part of the six-file policy order above):
- `docs/features/active/2026-07-19-utilitiescs-nullable-ci-capstone-376/issue.md`
- `docs/features/active/2026-07-19-utilitiescs-nullable-ci-capstone-376/spec.md`
- `docs/features/active/2026-07-19-utilitiescs-nullable-ci-capstone-376/user-story.md`
- `docs/features/active/2026-07-19-utilitiescs-nullable-ci-capstone-376/research/2026-07-19T00-30-ci-capstone-research.md`
- `docs/features/active/2026-07-19-utilitiescs-nullable-ci-capstone-376/plan.2026-07-19T04-25.md`
- `docs/features/epics/utilitiescs-nullable-remediation/epic.md` (capstone scope addendum section)

Notes:
- `.claude/rules/csharp.md` line 16 and lines 81-83 document the toolchain type-check step as
  forcing `/p:Nullable=enable` globally, which is the exact rules-vs-convention conflict this
  plan's AC4/Phase 5 flags for the maintainer without editing the rule file.
- `.claude/rules/ci-workflows.md`'s deliberately-failing-nested-command requirement does not
  apply to the planned `.github/workflows/ci.yml` edit (Phase 3): the edit removes one
  `msbuild` flag and updates a comment; it does not add a deliberately-failing nested command,
  and the existing `if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }` line is preserved verbatim.
- `.claude/rules/benchmark-baselines.md` does not apply: this feature touches no
  `scripts/benchmarks/**` path and introduces no baseline artifact.
