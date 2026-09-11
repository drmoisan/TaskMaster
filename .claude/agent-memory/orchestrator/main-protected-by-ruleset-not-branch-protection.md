---
name: main-protected-by-ruleset-not-branch-protection
description: TaskMaster's main is protected by a repository RULESET, so the classic branch-protection endpoint returns 404 "Branch not protected" — a misleading negative; the ruleset carries the five required checks and the up-to-date-with-base requirement
metadata:
  type: reference
---

Querying classic branch protection on `main` returns a 404:

```
gh api repos/drmoisan/TaskMaster/branches/main/protection
-> {"message":"Branch not protected","status":"404"}
```

**That negative is misleading and must not be read as "main is unprotected."** Protection is configured as a
repository *ruleset*, which lives at a different endpoint:

```
gh api repos/drmoisan/TaskMaster/rulesets --jq '.[] | {id, name, target, enforcement}'
-> {"enforcement":"active","id":18572843,"name":"main","target":"branch"}
gh api repos/drmoisan/TaskMaster/rulesets/18572843 --jq '.rules[] | {type, parameters}'
```

**Why it matters:** the ruleset sets `strict_required_status_checks_policy: true`, which is the
"branch must be up to date with base before merging" requirement. If you conclude from the 404 that main is
unprotected, you skip the mandatory merge of `origin/main` and the PR cannot land. On #796 the merge was
genuinely required, and confirming it from the ruleset rather than assuming it is what justified re-anchoring
every footprint diff (see [[merging-main-invalidates-plan-base-anchor]]).

**The five required status checks**, exact context strings, verified 2026-09-07:

- `actionlint / actionlint`
- `format-check / Verify formatting`
- `build-analyzers / Build with analyzers and code style enforcement`
- `build-nullable / Build with nullable warnings treated as errors`
- `mstest-coverage / Run MSTest suite with coverage`

Other rules: `deletion` and `non_fast_forward` blocked; `allowed_merge_methods` is `["merge"]` only;
`required_approving_review_count` is 0 but `require_extra_approval_for_unattributed_changes` is true.

**How to apply.** `mstest-coverage` runs the FULL MSTest suite, not the assembly-scoped subset a
QuickFiler-only or UtilitiesCS-only plan gates on locally. It is therefore the one required check exposed to
repo-wide flakes such as the DfDeedle ETL race
(see [[project_flaky_dfdeedle_etl_250ms_timeout.md]]). A failure there on a single unrelated test is a flake
candidate, not a branch defect — discriminate by re-running the SAME head SHA and checking `run_attempt`
first (see [[ci-rerun-same-sha-discriminates-flake]]). Conversely, a green with `run_attempt: 1` is a genuine
first-attempt pass and needs no flake discrimination at all.
