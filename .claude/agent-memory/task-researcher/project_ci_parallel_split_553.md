---
name: ci-parallel-split-553
description: "Issue #553 CI split research (2026-08-14): recommended 4 independent windows jobs w/ tailored setup, no artifact sharing; ruleset 18572843 single-PUT context swap is fail-closed either side; reusable-workflow check names are '<caller job> / <callee job>' and must be captured from a live run"
metadata:
  type: project
---

Issue #553 research (2026-08-14), artifact at
`docs/features/active/2026-08-14-ci-parallel-job-split-553/research/2026-08-14T13-30-ci-parallel-job-split-research.md`.

Key conclusions the implementation session will need:

- **Recommended topology:** four independent reusable-workflow jobs (`_format-check`, `_build-analyzers`,
  `_build-nullable`, `_mstest-coverage`), zero `needs:` edges, MSTest job does its own plain `/t:Build`.
  Build-once + upload/download artifact loses on critical-path arithmetic even at zero transfer cost.
  Est. wall clock ~277s (tailored setup) vs 444s baseline; billed windows seconds ~763 vs ~444.
- **Do not merge the two compiles:** adding `TreatWarningsAsErrors` to the analyzer build promotes analyzer
  warnings to errors (unratified change) or forces carve-outs that weaken the nullable gate. `/t:Rebuild`
  vs `/t:Build` is moot on a fresh runner but keep `/t:Rebuild` + its in-file comment.
- **Ruleset swap (id 18572843):** required contexts are exactly `actionlint` + `Format, build, analyze, and test`,
  `strict: true`. Both PUT-before-merge and merge-before-PUT are fail-closed (unreported required contexts
  block, never bypass); the only under-gating hazard is a PUT whose contexts set omits a gate. PUT is
  full-replace — round-trip writable fields only (name, target, enforcement, bypass_actors, conditions, rules).
- **Check-run names for called workflows are `<caller job name> / <callee job name>`** — capture exact strings
  via `gh api repos/drmoisan/TaskMaster/commits/<sha>/check-runs` from the split PR's green run BEFORE the PUT.
  This naming detail is the most likely migration failure.
- **Concurrency:** keep the `concurrency` block in caller `ci.yml` only; callee-level workflow concurrency
  under `workflow_call` is not clearly documented — avoid it.
- **Gaps recorded:** `.github/workflows/README.md` does not exist (AC requires creating it);
  `docs/features/potential/promoted/2026-08-14-ci-parallel-job-split.md` absent (content lives in the feature
  folder's issue.md); `TaskMaster.sln` has 18 projects (9 test), not 19 as delegation prose claimed;
  orchestrate SKILL's nesting cap of 4 is stale vs current GitHub docs (10 levels) but repo uses one level.
- CSharpier 1.2.6 pinned in root-level `dotnet-tools.json`; the format gate needs no `nuget restore` and
  should drop the packages cache. actions/cache: saves skipped on exact-key hit, caches immutable, lost
  concurrent save is a warning not a failure.

**Why:** The implementation, ruleset mutation, and post-split evidence capture happen in later sessions;
these are the non-obvious facts that were expensive to establish.

**How to apply:** When planning/reviewing the #553 implementation or its ruleset migration, start from the
research artifact and verify the tailored-setup assumption (msbuild jobs without setup-dotnet) in the first
green run before trusting the ~277s estimate.
