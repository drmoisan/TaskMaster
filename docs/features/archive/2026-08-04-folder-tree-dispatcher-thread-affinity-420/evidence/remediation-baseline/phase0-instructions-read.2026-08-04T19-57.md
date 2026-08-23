Timestamp: 2026-08-04T19:57:00-04:00
Command: Read repository and execution-policy files in required order.
EXIT_CODE: 0
Output Summary: Required policy and execution instructions were read before remediation. The repair must retain dispatcher-owned live traversal, deterministic tests, no fallback behavior, and mandatory coverage gates.

Policy Order:
1. `AGENTS.md` standing instructions and tone policy.
2. `AGENTS.md` general code-change policy.
3. `AGENTS.md` general unit-test policy.
4. `AGENTS.md` C# code-change and unit-test policy.
5. `.agents/skills/policy-compliance-order/SKILL.md`.
6. `.agents/skills/csharp/SKILL.md`.
7. `.agents/skills/atomic-plan-contract/SKILL.md`.
8. `.agents/skills/acceptance-criteria-tracking/SKILL.md`.
9. `.agents/skills/evidence-and-timestamp-conventions/SKILL.md`.

Non-negotiable constraints:
- Do not introduce `Task.Yield`, a worker-local dispatcher, or caller-selected live-traversal fallback.
- Preserve the captured STA boundary for live Outlook access and WPF dispatcher yielding.
- Use deterministic MSTest, Moq, and FluentAssertions tests without Outlook, network, temporary files, sleeps, timers, polling, retries, or manual validation.
- Run CSharpier, analyzer build, nullable build, and coverage-enabled MSTest in order; restart the pass after a modifying or failing step.
- Require repository line coverage of at least 80 percent, at least 90 percent coverage for every new method, class, or module, and no changed-line regression.
