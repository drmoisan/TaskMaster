---
name: never-emit-fable-policy-disabled
description: Emitting "model_budget.fable_policy: disabled" in preparation prompts clamps every fable cell to opus and exhausted the Opus weekly limit mid-run; use the repository default or "preferred"
metadata:
  type: feedback
---

**Rule: never write `model_budget.fable_policy: disabled.` into a preparation-delegation prompt
unless you can state why this specific run must not use the fable tier.** Emit `preferred`, or emit
the repository's configured default, and say which you chose.

**Why.** Verified 2026-09-12 on the `bugs-2026-09-11` run. `config/orchestration-routing.json` maps

```text
C1 -> haiku    C2 -> sonnet    C3 -> opus    C4 -> fable
```

and its own `model_budget.fable_policy` is **`available`**. The model-budget contract in
`.claude/rules/orchestrator-state.md` says `disabled` "removes `fable` from the consideration set
and clamps `fable` cells to `opus`". So `disabled` does not mean "be conservative" — it means
**every C4 delegation moves from fable onto opus**, on top of every C3 delegation that was already
there.

I emitted `disabled` in all twelve prompts reasoning that it was the documented default and that
changing model routing unilaterally was the riskier move. Both halves were wrong: the skill's
`<disabled|available|preferred>` placeholder is not a statement of the repository's default, and
`disabled` is the *most* opus-intensive setting, not the most neutral one.

The two C4 items (792 breadcrumb WebView2, 743 ItemViewer seam) are the heaviest in the corpus and
ran their whole chain — researcher, prd-feature, planner, executor preflight — on opus. Four wave-2
children then died together on `HTTP 429 ... weekly limit ... model sent to the API: claude-opus-5`,
with a reset two days out. Wave 1 had already spent roughly 2.9M subagent tokens across six items.

**What `preferred` buys.** The `preferred_overlay` changes only the C3 cell and only for
`atomic-planner`, `prd-feature`, `feature-review`, `task-researcher`. So under `preferred`:

- C1 haiku, C2 sonnet — untouched by the outage either way
- C3 fable for those four agents, **opus only for `atomic-executor` preflight**
- C4 fable for everyone (base table, overlay does not touch C4)

That leaves one opus call site per C3 item instead of a whole chain per C3 and C4 item.

**How to apply.**

- Read `config/orchestration-routing.json` `model_budget.fable_policy` before authoring prompts and
  treat it as the default to carry, not the skill's placeholder list.
- Band inflation compounds this. Children correctly revised three of this run's estimates upward
  (C1->C2, C1->C3, C2->C3, C2->C3), because `compute_complexity_floor` forces C3 whenever
  `concurrency_or_ordering` or `cross_module_contract_change` is present. In a QuickFiler bug corpus
  those signals are common, so assume the realized band distribution is heavier than the intake
  estimate and pick the policy for the realized distribution.
- A rate-limit death is recoverable. The stranded worktree survives with its uncommitted research,
  spec and partial plan; a relaunched child can Read those absolute paths and reuse them. Point it
  at the exact paths and tell it to re-verify every citation rather than trust them.

See [[planner-git-commits-must-be-single-bare-segments]] for why you cannot commit the stranded
work yourself, and [[parallel-artifact-authoring-gotchas]] for the rest of the run's traps.
