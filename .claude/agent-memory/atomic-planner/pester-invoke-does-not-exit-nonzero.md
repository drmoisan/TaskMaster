---
name: pester-invoke-does-not-exit-nonzero
description: Pester 5.6.1 defaults Run.Exit and Run.Throw to False, so a direct `Invoke-Pester -Configuration $c` exits 0 even when It blocks fail — never assert non-zero exit on that channel in an [expect-fail] task
metadata:
  type: feedback
---

Never write an unscoped "`EXIT_CODE:` non-zero" acceptance on a task that runs Pester through a direct `pwsh -Command '... Invoke-Pester -Configuration $c'` command. Pester **5.6.1** defaults `Run.Exit = False` and `Run.Throw = False`, so the process exits `0` even when `It` blocks fail. The red-state proof for such a task must be the **enumerated failing `It` names plus their verbatim failure messages**, not a process exit code. If a non-zero exit is genuinely wanted, the task text must require `$c.Run.Exit = $true` and require the executor to record that it set it.

**Why:** In the #512 plan, `[P1-T3]` was an `[expect-fail]` red-before-green gate whose acceptance opened with "`EXIT_CODE:` non-zero". A round-2 delta then added a second measurement channel ("run the MCP function for the record, then run the direct Pester command ... **Record both exit codes**"). Under the plan's fail-closed evidence rule, one of the two mandated exit codes is `0` by construction, which contradicted the task's own acceptance and would have stalled the plan's sole red-before-green proof. Preflight round 3 flagged it as the single blocking item.

**How to apply:**
- When a task measures the same run through two channels, **scope every exit-code clause to a named channel**. "the MCP run's `EXIT_CODE:` is recorded as returned and is expected to be non-zero" is satisfiable; a bare "`EXIT_CODE:` non-zero" is not.
- When a channel's exit code is known not to discriminate, say so **in the task**, and name what does discriminate. Silence lets an executor treat the `0` as a contradiction.
- This is the exit-code analogue of the detail-channel problem in [[project_512_toolchain_gate_fidelity_plan_seams]] seam 7: MCP PoshQC wrappers and direct Pester runs disagree on both *detail* and *exit semantics*, and a plan must pin which channel carries which claim. See also [[research-claims-as-acceptance-clauses]] — the Pester default here was measured by a preflight probe, not assumed.
