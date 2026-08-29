---
name: model-routing-feature-review-is-always-fable
description: CORRECTED — the fable overlay applies ONLY under fable_policy preferred; under fable_policy available the C3 table model opus is the conformant choice for feature-review, atomic-planner, prd-feature and task-researcher.
metadata:
  type: reference
---

**This memory previously said feature-review is always fable and that no band makes an opus
review conformant. That is wrong as a general claim.** It was true of a run whose
`fable_policy` was `preferred`; it does not generalize.

`config/orchestration-routing.json` defines two things:

- `model_policy.complexity_to_model`: `C1 haiku`, `C2 sonnet`, `C3 opus`, `C4 fable`.
- `model_policy.preferred_overlay`, whose own description reads "Applied only under fable_policy
  preferred. Changes only the C3 cell to fable, and only for the listed agents." The listed
  agents are `atomic-planner`, `prd-feature`, `feature-review` and `task-researcher`.

So under `fable_policy: available` there is no overlay, and `feature-review` at C3 resolves to
`opus`. `.claude/lib/model-routing/ModelRouting.psm1` confirms it: `Resolve-DelegationModel
-Agent feature-review -Band C3 -FablePolicy available` returns `table_model=opus,
clamped_from=null, model=opus`. `atomic-executor` and `pr-author` stay opus at C3 under every
policy.

**Why:** on 2026-08-29 a parallel run carried `model_budget.fable_policy: available`. Following
the old memory would have meant either delegating at a non-conformant model or writing a
disclosure for a deviation that did not exist.

**How to apply:** read `fable_policy` off the delegation prompt first, then resolve with the
module rather than from memory. The module is present at
`.claude/lib/model-routing/ModelRouting.psm1` even where `scripts/dev_tools/` is absent; its
band parameter is `-Band`. Also useful: `Get-ComplexityFloor -SignalsPresent` recognises only
four floor signals — `classifier_or_model_logic`, `auth_or_token_handling`,
`concurrency_or_ordering`, `cross_module_contract_change` — each contributing C3, and anything
else contributes nothing and leaves the floor at C1.

Related: [[model-routing-scripts-absent-on-epic-integration-base]],
[[model-routing-hook-reads-canonical-path-only]]
