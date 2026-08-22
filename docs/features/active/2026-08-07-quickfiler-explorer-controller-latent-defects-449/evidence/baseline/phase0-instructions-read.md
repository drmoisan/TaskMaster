# Phase 0 — Policy Instructions Read (Issue #449)

Timestamp: 2026-08-22T09-16

Policy Order: The order mandated by `.claude/skills/policy-compliance-order/SKILL.md` is
1. `CLAUDE.md` (standing instructions, all sections including the embedded General Code Change Policy, General Unit Test Policy, C# Code Change Policy, and C# Unit Test Policy)
2. `.claude/rules/general-code-change.md` (cross-language code change policy)
3. `.claude/rules/general-unit-test.md` (cross-language unit test policy)
4. Language-specific rules for the files in scope — for C#, `.claude/rules/csharp.md`

Command: `grep -c '' CLAUDE.md .claude/rules/general-code-change.md .claude/rules/general-unit-test.md .claude/skills/policy-compliance-order/SKILL.md` and `grep -c '' .claude/rules/csharp.md`
EXIT_CODE: 0

## Files read end to end, in policy order

| Order | Path | Line count | Task |
| --- | --- | --- | --- |
| 1 | `CLAUDE.md` | 447 | [P0-T1] |
| 2 | `.claude/rules/general-code-change.md` | 80 | [P0-T2] |
| 3 | `.claude/rules/general-unit-test.md` | 105 | [P0-T3] |
| 4 | `.claude/rules/csharp.md` | 96 | [P0-T4] |

Supporting reference read for the order itself: `.claude/skills/policy-compliance-order/SKILL.md` (40 lines).

## [P0-T4] resolution

`.claude/rules/csharp.md` is PRESENT (96 lines) and was read in full. The planner note that the file
was present at plan-authoring time is confirmed. No absence statement is required.

Directory listing that established presence:

```
architecture-boundaries.md   general-code-change.md    powershell.md
benchmark-baselines.md       general-unit-test.md      python.md
ci-workflows.md              mermaid.md                python-suppressions.md
csharp.md                    orchestrator-state.md     quality-tiers.md
                             parallel-orchestration.md self-explanatory-code-commenting.md
                             plan-acceptance-gates.md  shell.md
                                                       tonality.md
                                                       typescript.md
                                                       typescript-suppressions.md
```

## Output Summary

All four mandatory policy documents were read end to end in the mandated order. No policy document
was edited; `.claude/**` is read-only for this child. The controlling constraints carried forward
into execution are: CSharpier via `dotnet tool run` only; `/t:Rebuild` never `/t:Build`; no
`/p:Nullable=enable`; MSTest + Moq + FluentAssertions only; no `Thread.Sleep`, `Task.Delay`,
`DateTime.Now`, `Random.Shared`, temporary file, or real wall-clock wait in tests; 500-line file cap
for non-Markdown files.
