# Phase 0 — Policy instructions read

Timestamp: 2026-09-09T12-26
Task: [P0-T1]

Policy Order: the order given by `.claude/skills/policy-compliance-order/SKILL.md` —
1. `CLAUDE.md` (standing instructions)
2. `.claude/rules/general-code-change.md` (cross-language code change policy)
3. `.claude/rules/general-unit-test.md` (cross-language unit test policy)
4. Language- or domain-specific rules for the files in scope. This change touches `*.cs` only, so
   `.claude/rules/csharp.md` applies. `.claude/rules/quality-tiers.md` and `.claude/rules/tonality.md`
   carry `paths: "**"` frontmatter and apply to every file, and are read in the positions the plan
   task lists them.

Command: `pwsh -NoProfile -Command "@('CLAUDE.md','.claude/rules/general-code-change.md','.claude/rules/general-unit-test.md','.claude/rules/quality-tiers.md','.claude/rules/tonality.md','.claude/rules/csharp.md') | ForEach-Object { if (Test-Path -LiteralPath $_) { '{0} {1}' -f $_, (Get-Content -LiteralPath $_).Count } else { '{0} NOT PRESENT' -f $_ } }"`
EXIT_CODE: 0

## Files read

| # | Path | Line count |
|---|---|---|
| 1 | `CLAUDE.md` | 447 |
| 2 | `.claude/rules/general-code-change.md` | 80 |
| 3 | `.claude/rules/general-unit-test.md` | 105 |
| 4 | `.claude/rules/quality-tiers.md` | 51 |
| 5 | `.claude/rules/tonality.md` | 80 |
| 6 | `.claude/rules/csharp.md` | 96 |

All six paths are present in this worktree. No path is recorded `NOT PRESENT`.

Output Summary: all six policy documents were read in full in the order above. Six of six present;
zero recorded `NOT PRESENT`. The binding constraints carried forward into execution are: the 500-line
per-file ceiling (`general-code-change.md` line 49); the four-step C# toolchain order
csharpier -> msbuild analyzers -> msbuild nullable -> vstest with a restart from step 1 on any failure
or auto-fix (`CLAUDE.md` lines 403-410); `/t:Rebuild` mandatory and `/p:Nullable=enable` prohibited
(`CLAUDE.md` lines 202 and 211); MSTest plus Moq plus FluentAssertions for tests
(`.claude/rules/csharp.md` lines 33-35); no temporary files in tests
(`general-unit-test.md` line 73); and the professional, non-hyperbolic tone required by
`tonality.md`. The coverage-threshold divergence between `CLAUDE.md` (80% repository, 90% new code)
and `.claude/rules/general-unit-test.md` / `.claude/rules/quality-tiers.md` (85% line, 75% branch) is
resolved by plan decision 5 in favour of `CLAUDE.md`; the change-scoped conditions are the blocking
ones under AC18.
