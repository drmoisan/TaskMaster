# Phase 0 — Policy Instruction Read Record

Timestamp: 2026-09-09T16-32

Policy Order: the order defined by .claude/skills/policy-compliance-order/SKILL.md, namely
CLAUDE.md first, then .claude/rules/general-code-change.md, then
.claude/rules/general-unit-test.md, then the language- or domain-specific rules for the files in
scope, then the tonality rule and the plan-acceptance-gate rule that governs this plan's acceptance
conditions.

Files read, in that order:

- CLAUDE.md
- .claude/rules/general-code-change.md
- .claude/rules/general-unit-test.md
- .claude/rules/quality-tiers.md
- .claude/rules/csharp.md
- .claude/rules/tonality.md
- .claude/rules/plan-acceptance-gates.md

## Notes

Hard constraints carried forward into execution. No policy document is modified by this plan. The
C# toolchain order is csharpier format, csharpier check, the MSBuild analyzer gate, the MSBuild
nullable gate, then vstest; any failure or any formatter rewrite restarts the loop from its first
step. No coverage threshold, analyzer severity or policy requirement is lowered to make a gate pass,
and no production file is added to a coverage exclusion list.

Determinism obligations. No Thread.Sleep, Task.Delay, DateTime.Now, Stopwatch or timing tolerance
appears in any test this plan writes or edits. Deadline behaviour is driven through FakeTimeProvider
or ArmingBarrierTimeProvider.

Tone. Professional, factual and neutral wording is used in every artifact, source comment and commit
message this plan produces.
