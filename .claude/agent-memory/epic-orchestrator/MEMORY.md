# Epic-Orchestrator Memory Index

- [Inline child lifecycle prohibited](feedback_inline_child_lifecycle_prohibited.md) — must delegate every child via Agent(orchestrator) and final PR via Agent(pr-author); on genuine spawn failure record delegation_failures[] verbatim and stop blocked, never run inline
- [orchestrator/pr-author runtime availability is a moving target](project_orchestrator_subagent_not_registered.md) — 'orchestrator' absent 06:08Z but PRESENT/launchable 11:33Z on 2026-07-10; 'pr-author' still absent (use pr-author SKILL inline). Always verify the spawn per run
