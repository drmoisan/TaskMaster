# Epic-Orchestrator Memory Index

- [Inline child lifecycle prohibited](feedback_inline_child_lifecycle_prohibited.md) — must delegate every child via Agent(orchestrator) and final PR via Agent(pr-author); on genuine spawn failure record delegation_failures[] verbatim and stop blocked, never run inline
- [orchestrator/pr-author runtime availability is a moving target](project_orchestrator_subagent_not_registered.md) — 'orchestrator' absent 06:08Z but PRESENT/launchable 11:33Z on 2026-07-10; 'pr-author' still absent (use pr-author SKILL inline). Always verify the spawn per run
- [Hung wave child = no clean autonomous recovery](feedback_hung_child_recovery_blocked_by_removal_gate.md) — orphaned/stalled prior-session child can't be re-attached, removal gate blocks worktree reset, feature branch collides on re-delegate; halt and report, never falsify merge_status or kill sibling processes
- [Live child at pr-author is NOT necessarily hung](feedback_live_child_at_pr_author_not_hung.md) — re-derive REMOTE branch/PR truth before declaring a stall; push rejection = a child just landed work; child may merge minutes later (324/PR#333 case)
