# Parallel Orchestrator Memory Index

- [Parallel run execution playbook](project_parallel_run_execution_playbook.md) — kickoff artifact lives on the plan-home branch, no poetry, no status template; use a dedicated plan-home worktree
- [Pre-implementation gate scope](project_preimplementation_gate_scope.md) — parallel runs need no synthetic orchestrator-state.json; .json/.js writes blocked everywhere, .md allowed; git commit needs a pathspec operand
- [Free item branches by detaching](feedback_free_item_branches_by_detaching.md) — stale planner worktrees hold the item branches; detach HEAD, never `git worktree remove` (both removal gates fail closed)
- [Issue merge/removal commands bare](feedback_issue_merge_and_removal_commands_bare.md) — the merge gate reads the first digit run in the whole command, so a `cd` path with digits becomes the PR number; also unlock a finished child's worktree before removing
- [Defer the /parallel-add checkpoint write](feedback_defer_the_checkpoint_write_until_admission.md) — a `proposed` item is unrepresentable (invariants 9 + 13) and concurrent adds race on the one checkpoint file; write only after preparation returns
- [Verify delivery before preparing an admission](feedback_verify_delivery_before_preparing_an_admission.md) — grep main for `fix(<N>)` first; an OPEN issue is not proof of outstanding work, and preparation costs ~8 min to learn it
- [468-family shipped with issues left open](project_qfc_collection_468_family_shipped_issues_left_open.md) — #286/469/470/471/473/474 are delivered on main but still OPEN; adding any of them prepares a no-op
- [Derive counts exhaustively before approving](feedback_derive_counts_exhaustively_before_approving.md) — no count enters an approved AC from a single-pass grep; search the symbol family, cross-check independently, scope counting tools to the named section
- [Self-review before preflight round one](feedback_self_review_before_preflight_round_one.md) — mirror preflight's own checks internally first; a preflight round count above 1 is a process defect to investigate
- [Children share one orchestrator-state file](project_children_share_one_orchestrator_state_file.md) — a finishing child's late write lands in the next child's live checkpoint; verify identity fields by substring, stay read-only mid-run
- [Don't repair a concurrent add's partial item](feedback_do_not_repair_a_concurrent_adds_partial_item.md) — a stalled /parallel-add leaves an `admitted` item with no cohort; leave it, report it, keep re-reading before writes
- [Blast-radius PowerShell calling convention](reference_blast_radius_powershell_calling_convention.md) — absolute Import-Module, `-DateKind String`, read the `conflict` key; three traps that all present as a wrong verdict
