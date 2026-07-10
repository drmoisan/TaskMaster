# Phase 0 — Policy Instructions Read (P0-T1)

Timestamp: 2026-07-09T16-30

Policy Order: CLAUDE.md -> .claude/rules/general-code-change.md -> .claude/rules/general-unit-test.md -> .claude/rules/csharp.md -> language/domain-specific skills

Files read (in required order):
- CLAUDE.md (project standing instructions; loaded via session context)
- .claude/rules/general-code-change.md (cross-language code change policy)
- .claude/rules/general-unit-test.md (cross-language unit test policy)
- .claude/rules/csharp.md (C#-specific toolchain and coding standards)
- .claude/skills/atomic-plan-contract/SKILL.md (atomic plan format / Phase 0 / final QA loop)
- .claude/skills/evidence-and-timestamp-conventions/SKILL.md (canonical evidence paths)
- .claude/skills/policy-compliance-order/SKILL.md (mandatory policy reading order)
- .claude/skills/acceptance-criteria-tracking/SKILL.md (AC check-off protocol)

Key operative constraints confirmed:
- C# toolchain order: csharpier -> analyzers build -> nullable/TreatWarningsAsErrors build -> vstest /EnableCodeCoverage; restart from step 1 on any change/failure.
- Tests: MSTest + Moq + FluentAssertions; AAA; no live Form/Control; no popups; no Thread.Sleep/Task.Delay; no temp files.
- Coverage: TaskTree.dll >= 80% line; new files >= 90%; no regression on changed lines.
- Banned symbols (BannedSymbols.txt): DateTime.Now, DateTime.UtcNow, Random.Shared, Thread.Sleep, Task.Delay.
- Evidence resolves to <FEATURE>/evidence/<kind>/ only; artifacts/csharp/coverage.xml is the raw review-gate consumable only.

Binary outcome: artifact exists with Timestamp, Policy Order, and explicit list of files read. PASS.
