Timestamp: 2026-08-04T19-15
Command: Read repository policy and execution skills
EXIT_CODE: 0
Output Summary: Required policy and execution instructions were read before implementation.

## Policy Order

1. `AGENTS.md` standing instructions and generated policy sections.
2. `AGENTS.md` cross-language code-change policy.
3. `AGENTS.md` cross-language unit-test policy.
4. `AGENTS.md` C# code-change and C# unit-test policy.
5. `.agents/skills/csharp/SKILL.md`.
6. `.agents/skills/atomic-plan-contract/SKILL.md`.
7. `.agents/skills/acceptance-criteria-tracking/SKILL.md`.
8. `.agents/skills/evidence-and-timestamp-conventions/SKILL.md`.

## Constraints Applied

- Execute the approved plan in task order and check off each task only after verification.
- Add deterministic MSTest coverage before production repair; do not use live Outlook, network access, temporary files, sleeps, timers, retries, or weakened assertions.
- Preserve Outlook STA ownership for live composition, hierarchy traversal, notification subscriptions, and post-yield continuation; do not add a `Task.Yield` fallback or a worker-local dispatcher.
- Keep `WpfDispatcherYield` strict.
- Keep all touched C# files under 500 lines.
- Run CSharpier, analyzer build, nullable build, and MSTest coverage in that order; restart the final pass if an earlier step changes files or fails.
- Store evidence only under this feature's canonical `evidence/` directories and include timestamp, command, exit code, and output summary.
