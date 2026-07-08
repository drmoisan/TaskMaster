# Phase 0 — Policy and Baseline Instructions Read (Issue #269)

- Timestamp: 2026-07-08T09-20
- Task: [P0-T1]

## Policy Order

1. `CLAUDE.md`
2. `.claude/rules/general-code-change.md`
3. `.claude/rules/general-unit-test.md`
4. `.claude/rules/csharp.md`
5. `.claude/skills/atomic-plan-contract/SKILL.md`
6. `.claude/skills/evidence-and-timestamp-conventions/SKILL.md`
7. `.claude/skills/acceptance-criteria-tracking/SKILL.md`
8. `docs/features/active/2026-07-07-quickfiler-darkmode-unread-labels-stay-blue-269/issue.md`
9. `docs/features/active/2026-07-07-quickfiler-darkmode-toggle-stale-elements-254/research/mechanism-unread-labels-blue-254.md`

## Files Read (in order)

1. `CLAUDE.md` — root project instructions, policy compliance order, C# toolchain and unit test policy.
2. `.claude/rules/general-code-change.md` — cross-language code change policy, mandatory toolchain loop, 500-line file limit.
3. `.claude/rules/general-unit-test.md` — cross-language unit test policy, coverage thresholds, AAA structure, external-dependency prohibition.
4. `.claude/rules/csharp.md` — C#-specific toolchain commands (CSharpier, analyzers, nullable, MSTest+Moq+FluentAssertions), DI seam guidance, analyzer stack.
5. `.claude/skills/atomic-plan-contract/SKILL.md` — plan format, Phase 0 requirements, evidence path invariants, expect-fail task requirements, no-SKIPPED rule.
6. `.claude/skills/evidence-and-timestamp-conventions/SKILL.md` — canonical evidence locations under `<FEATURE>/evidence/<kind>/`, ISO-8601 timestamp format, artifact schema.
7. `.claude/skills/acceptance-criteria-tracking/SKILL.md` — AC source resolution for minor-audit mode, check-off protocol, AC status summary format.
8. `docs/features/active/2026-07-07-quickfiler-darkmode-unread-labels-stay-blue-269/issue.md` — issue #269 requirements, AC1-AC5, confirmed root cause summary.
9. `docs/features/active/2026-07-07-quickfiler-darkmode-toggle-stale-elements-254/research/mechanism-unread-labels-blue-254.md` — confirmed mechanism derivation for the defect.

## Notes

- Fresh worktree bootstrap was required before any toolchain command could run: `scripts/vscode/Install-RepoDotNetSdk.ps1` (installed repo-local .NET SDK 8.0.205) and `scripts/vscode/Invoke-Restore.ps1 -SolutionPath TaskMaster.sln -Configuration Debug -Platform "Any CPU"` (restored 169 NuGet packages). Both completed successfully (`Build succeeded. 0 Warning(s). 0 Error(s).`) prior to any Phase 0 command-bearing task.
