---
name: pr-creation-readiness-gate-and-receipt-mechanics
description: The PR-creation gate requires local_execution_overrides to be empty with no documented drain procedure, and the pr-author receipt must be newer than the context summary — plain cp breaks that
metadata:
  type: project
---

Opening an item's pull request can be blocked by two independent mechanisms that both look like
defects and are not.

**Why:** Both were hit on run bugs-638-644-647 on 2026-08-30, after the item was fully delivered
(58 of 58 plan tasks, zero-Blocking GO review, 1254 of 1254 tests). Neither is documented in any
skill or rule, so each costs a diagnosis cycle.

**How to apply:**

- **`local_execution_overrides` must be an EMPTY LIST before a pull request can be created.**
  `Get-OrchestratorStatePrCreationReadinessError` in
  `.claude/lib/orchestrator-state/OrchestratorState.psm1` enforces it, alongside: steps 5 through 8
  not `pending`/`blocked`/`blocked_remediation_loop_limit`, and `blocked_reason` absent, null, or
  the literal `none`. The same field is re-checked at COMPLETION as rule C6.10 via
  `$script:COMPLETION_EMPTY_LIST_KEYS` in `OrchestratorStateRoutingContract.psm1`, paired with
  `delegation_bypasses`.
- **There is no drain or adjudication procedure for that field anywhere under `.claude/`.** It
  appears only in those two library modules — never in a skill, never in a rule. Its only function
  is to block PR creation for a run that deviated from its approved plan, so clearing it is not
  bookkeeping. **Escalate to the user; never clear it on another agent's say-so**, including an
  orchestrator that "ratified" the override. A child that refuses this is behaving correctly.
  Two legitimate resolutions exist: amend the plan so the deviations stop being deviations and
  re-preflight, or clear it as an exception the USER explicitly authorizes. Preserve the records
  (the child may already have staged `local_execution_overrides_archived`) and record the
  authorizing party.
- **`local_execution_overrides_archived` is an OBJECT, not an array** — shape `{note, entries}`.
  An `Array.isArray` probe returns false and looks like the archive is missing; read `.entries`.
- **Run the readiness check locally before theorising.** Dot-source the module and call
  `Get-OrchestratorStatePrCreationReadinessError` against the checkpoint. PowerShell needs
  `-ExecutionPolicy Bypass`. It returns every failing conjunct at once, which distinguishes the
  substantive blocker from a child's own halt bookkeeping (`step8_status: blocked`,
  `blocked_reason: validator_failed`) that clears on any resume.
- **The hooks read `artifacts/orchestration/orchestrator-state.json` relative to the HOOK PROCESS
  CWD.** A child that ran in an isolated worktree wrote its final record there, while the session
  root may still hold a stale record from an earlier child. Copy the child's final checkpoint to the
  session root before running `gh pr create` from the session root, and verify `issue-num` matches
  the item first.
- **`PR_AUTHOR_RECEIPT_STALE` is usually self-inflicted by `cp`.** The hook requires
  `artifacts/pr_body_<N>.receipt.json`'s `created_at` to be strictly newer than the LAST-WRITE TIME
  of `artifacts/pr_context.summary.txt`. A plain `cp` stamps the summary with the current time and
  inverts a relationship that was correct at the source. Use **`cp -p`** to preserve mtimes: that
  restores the genuine provenance rather than fabricating an ordering, and the body's SHA-256 still
  matches the receipt because the bytes are untouched. Do not edit the body to "fix" anything —
  the receipt binds its exact SHA, so any edit invalidates it and forces a re-author.

- **The PR-creation preflight wants a SIX-key delegation receipt, and it is a different set from the
  completion gate's.** The members are `step`, `agent_name`, `agent_id`, `skill_source`,
  `result_signal`, `artifact_paths`. The intuitive `{agent, phase, model, status}` shape fails with
  **one error per missing key per receipt**, so a run with several receipts produces a wall of
  errors that reads like a broken checkpoint rather than one wrong shape. The completion gate asks
  instead for `evidence`; satisfying one gate's shape does not satisfy the other's.
- **The pre-implementation gate keys on a TOP-LEVEL boolean `lifecycle_ready`.** A nested
  `lifecycle_readiness: {ready_for_execution: true}` does not satisfy `Test-OrchestrationReady`,
  which also wants a non-empty `issue-num`, a `feature-folder` beginning with the literal
  `docs/features/active/`, and `route_id` (falling back to `path_selected`). The deny message names
  the CONCEPT rather than the key, which is why the wrong shape is easy to author and hard to
  diagnose from the error alone.

See [[parallel-run-execution-playbook]] and [[issue-merge-and-removal-commands-bare]].
