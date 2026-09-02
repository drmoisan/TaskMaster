# Phase 0 — Policy instructions read (P0-T1)

Timestamp: 2026-09-01T10-27
Task: [P0-T1]
Command: `pwsh -NoProfile -File <scratchpad>/p0t1-linecounts.ps1` (line counts via `(Get-Content -LiteralPath <path>).Count`)
EXIT_CODE: 0

Policy Order: the reading order defined by `.claude/skills/policy-compliance-order/SKILL.md` and by the
`## Policy Compliance Order` section of `CLAUDE.md`: standing instructions first, then the cross-language
code-change policy, then the cross-language unit-test policy, then the tier and tone rules, then the
language-specific rule file for the files in scope (C#).

## Files read, in order

| # | File | Line count |
|---|---|---|
| 1 | `CLAUDE.md` | 447 |
| 2 | `.claude/rules/general-code-change.md` | 80 |
| 3 | `.claude/rules/general-unit-test.md` | 105 |
| 4 | `.claude/rules/quality-tiers.md` | 51 |
| 5 | `.claude/rules/tonality.md` | 80 |
| 6 | `.claude/rules/csharp.md` | 96 |

Six files read. `.claude/rules/csharp.md` exists in this worktree and was read; the plan made that read
conditional on its existence.

Output Summary: All six policy files were read in the stated order. Line counts are numeric and were
measured with `(Get-Content -LiteralPath <path>).Count` rather than `Measure-Object -Line`. Constraints
carried forward into execution: CSharpier is invoked only through `dotnet tool run`; MSBuild uses
`/t:Rebuild` and never `/t:Build`; `/p:Nullable=enable` is never added; tests use MSTest with Moq and
FluentAssertions; `Thread.Sleep`, `Task.Delay`, and wall-clock waits are banned in test code; no
temporary files are created by tests; no file may exceed 500 lines; and no file under `.claude/rules/`,
`.claude/skills/`, or `CLAUDE.md` may be edited by this change.
