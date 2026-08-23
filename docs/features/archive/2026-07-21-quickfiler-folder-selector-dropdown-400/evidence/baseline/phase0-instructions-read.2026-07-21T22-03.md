Timestamp: 2026-07-21T22-03Z
Command: `Get-Content AGENTS.md` for the standing instructions, cross-language code-change section, and cross-language unit-test section, followed by `Get-Content -Raw .agents/skills/csharp/SKILL.md`
EXIT_CODE: 0
Policy Order: 1. `AGENTS.md` standing instructions and canonical Copilot instructions; 2. `AGENTS.md` Agent Code Change Policy; 3. `AGENTS.md` General Unit Test Policy; 4. `.agents/skills/csharp/SKILL.md`
Files and sections read:
- `AGENTS.md`: repository setup, canonical instructions, tone policy, Agent Code Change Policy, and General Unit Test Policy
- `.agents/skills/csharp/SKILL.md`: C# toolchain, coding standards, testing standards, deterministic test rules, DI seams, and prohibited behaviors
Output Summary: Required policies were read in the mandated order. The remediation remains governed by failure-first tests, the 500-line limit, MSTest/Moq/FluentAssertions conventions, the exact CSharpier -> analyzer build -> nullable build -> coverage-enabled MSTest sequence, and zero-regression coverage gates.
