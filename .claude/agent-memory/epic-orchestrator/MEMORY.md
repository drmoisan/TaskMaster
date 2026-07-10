# Epic-Orchestrator Memory Index

- [Inline child lifecycle prohibited](feedback_inline_child_lifecycle_prohibited.md) — must delegate every child via Agent(orchestrator) and final PR via Agent(pr-author); on genuine spawn failure record delegation_failures[] verbatim and stop blocked, never run inline
- [orchestrator/pr-author subagents not registered](project_orchestrator_subagent_not_registered.md) — granted in frontmatter but absent from runtime launchable Agent set (verified 2026-07-10); spawn returns "Agent type 'orchestrator' not found" — verify before relying, it may change
