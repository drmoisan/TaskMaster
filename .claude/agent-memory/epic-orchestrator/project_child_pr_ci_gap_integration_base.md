---
name: child-pr-ci-gap-integration-base
description: TaskMaster's ci.yml triggers pull_request only on [main, development], so epic child PRs based on the integration branch get zero CI checks; use workflow_dispatch on the integration branch as the integrated-tree gate
metadata:
  type: project
---

In TaskMaster, `.github/workflows/ci.yml` declares `pull_request: branches: [main, development]`.
An epic child PR whose base is `epic/<slug>-integration` matches no workflow trigger, so
`gh pr view --json statusCheckRollup` returns empty and `mergeStateStatus` is `CLEAN`. The child's
S9 merge-on-green step has nothing to parse.

**Why:** This silently converts merge-on-green into merge-on-nothing for every child in an epic.
A child that reports "CI green" against an integration base has not been gated by CI at all — it
has at best run local CI-equivalent commands. The failure would otherwise surface only at the final
integration-to-`main` PR, after every child has already merged, which is the most expensive place
to discover it. Compare [[project_cross_child_annotation_fanin_debt]]: a green per-child gate is
not a green integrated-tree gate, and here the per-child gate does not even exist.

**How to apply:** Verify the trigger list yourself early in any epic run rather than trusting a
child's CI claim — read `.github/workflows/ci.yml` from the integration ref. Then: (1) tell each
child in its kickoff prompt that no CI will run and that local CI-equivalent verification is
mandatory and must be recorded as feature evidence; (2) `ci.yml` also declares
`workflow_dispatch`, so run `gh workflow run ci.yml --ref epic/<slug>-integration` at each wave
boundary to gate the *integrated* tree; (3) rely on the final `main`-based integration PR for the
authoritative full-CI gate. Do not "fix" this by adding the integration branch to the workflow's
trigger list mid-epic — a `.github/workflows/**` diff is itself Blocking under the
`modified-workflow-needs-green-run` policy rule and is outside every child's scope.

Re-verified unchanged on 2026-08-22 against `origin/epic/quickfiler-suite-determinism-foundation-integration`:
the trigger list is still `[main, development]` and `workflow_dispatch` is still declared.

Re-verified again on 2026-08-26 on `epic/quickfiler-bug-family-integration`. Two refinements
learned by doing it:

- **Dispatch against the CHILD BRANCH HEAD, not just the integration branch.** When a child has
  already merged the integration branch into itself (which the stale-base guard forces anyway),
  its head *is* the post-merge tree, so `gh workflow run ci.yml --ref <child-branch>` gates
  exactly what the PR will land. That upgrades a child's S9 from "compensating local run" to a
  genuine green remote run on the exact PR head SHA. Feature 498 merged on that basis
  (run 33005439742, success on head 4c54d418, PR #626).
- Record it in the child checkpoint's `ci_gate` with `workflow_run_id`, `workflow_run_url` and
  `head_sha`, and state plainly in `conclusion_basis` that it came from a dispatch rather than
  from a `pull_request` event. Contrast 446, whose `ci_gate.conclusion: success` rested only on a
  local toolchain re-run — both merged, but only one was actually gated by CI.
- A green dispatch run is **not** a completeness signal. Feature 468's dispatch was also green
  while a third of its atomic plan had never been executed; CI proves the tree builds and the
  tests that exist pass, nothing more. See [[feedback_premise_falsified_child_halt]].
