# Parallel Kickoff: bugs-2026-09-28

Planned by parallel-planner on 2026-09-29T03:58:00Z. All items are prepared: promoted, active folders created,
research complete, spec and user-story written, atomic plans approved, preflight ALL CLEAR, blast
radii declared and V1/V2-clear. Planning state:
artifacts/orchestration/parallel-planner-state.json (run branch: parallel/bugs-2026-09-28-plan).

Base: origin/main at 177b6d78e1b2408e5aedbd794cef3aad6b7fb372. Mode open, max_concurrency 6. Two
cohorts at generation 0: cohort 0 holds 882, 927, 928, 930 and 931; cohort 1 holds 929. The single
conflict edge is 927:929 (path_overlap on .github/workflows/README.md), so under the per-edge barrier
929 starts after 927 is merged and the other five items start together. Minor-audit items (928, 929,
930) record issue.md as their requirements source in place of a research artifact.

## Invocation Prompt

Run `/parallel-run bugs-2026-09-28` to execute this run, or paste the prompt below.

Use the parallel-orchestrator subagent to execute the prepared run whose manifest is
docs/features/parallel/bugs-2026-09-28/parallel.md on the plan-home branch parallel/bugs-2026-09-28-plan. Each item
resumes at atomic execution from its committed plan-path on its own pushed feature branch rather
than re-planning, and each item opens its own pull request against main.

## Item Summary

| issue_num | feature_folder | cohort | complexity | branch | plan-path |
| --- | --- | --- | --- | --- | --- |
| 882 | docs/features/active/2026-09-13-quickfiler-transactiongate-permit-leak-unexcluded-882 | 0 | C3 | bug/quickfiler-transactiongate-permit-leak-unexcluded-882 | docs/features/active/2026-09-13-quickfiler-transactiongate-permit-leak-unexcluded-882/plan.2026-09-13T18-24.md |
| 927 | docs/features/active/2026-09-28-evidence-and-identity-hygiene-sweep-927 | 0 | C3 | bug/evidence-and-identity-hygiene-sweep-927 | docs/features/active/2026-09-28-evidence-and-identity-hygiene-sweep-927/plan.2026-09-28T19-44.md |
| 928 | docs/features/active/2026-09-28-coverage-runner-scoped-threshold-and-format-928 | 0 | C3 | bug/coverage-runner-scoped-threshold-and-format-928 | docs/features/active/2026-09-28-coverage-runner-scoped-threshold-and-format-928/plan.2026-09-28T19-45.md |
| 929 | docs/features/active/2026-09-28-package-manifest-consistency-residuals-929 | 1 | C3 | bug/package-manifest-consistency-residuals-929 | docs/features/active/2026-09-28-package-manifest-consistency-residuals-929/plan.2026-09-28T20-01.md |
| 930 | docs/features/active/2026-09-28-csharp-latent-hazards-uithread-ilglobals-comments-930 | 0 | C3 | bug/csharp-latent-hazards-uithread-ilglobals-comments-930 | docs/features/active/2026-09-28-csharp-latent-hazards-uithread-ilglobals-comments-930/plan.2026-09-28T20-01.md |
| 931 | docs/features/active/2026-09-28-tests-depend-on-uncontrolled-environment-931 | 0 | C3 | bug/tests-depend-on-uncontrolled-environment-931 | docs/features/active/2026-09-28-tests-depend-on-uncontrolled-environment-931/plan.2026-09-28T20-01.md |

## Integrity

planning_commit: b62139f302f0b9901ec473ebe28afc335945659a

| plan-path | plan-hash |
| --- | --- |
| docs/features/active/2026-09-13-quickfiler-transactiongate-permit-leak-unexcluded-882/plan.2026-09-13T18-24.md | 05e0d64de1061aade459bf5f52d72786db157ce5 |
| docs/features/active/2026-09-28-evidence-and-identity-hygiene-sweep-927/plan.2026-09-28T19-44.md | 6b170debae0faca6e7fd06f0605f412c1d2b9091 |
| docs/features/active/2026-09-28-coverage-runner-scoped-threshold-and-format-928/plan.2026-09-28T19-45.md | 90475a9a52068b85a72a0d0d41c76af751ff34a6 |
| docs/features/active/2026-09-28-package-manifest-consistency-residuals-929/plan.2026-09-28T20-01.md | eb64f50fa569c191b4281c8fc7ca3e15597ecdb6 |
| docs/features/active/2026-09-28-csharp-latent-hazards-uithread-ilglobals-comments-930/plan.2026-09-28T20-01.md | fd05e84bd72dcf63b3952fc93ed15abd4c6de472 |
| docs/features/active/2026-09-28-tests-depend-on-uncontrolled-environment-931/plan.2026-09-28T20-01.md | e6ea4850a529590e220c6050023db18b12888202 |

## Execution Notes

Item branch heads at planning time: 882 f082187eaf0975ff48ec1662848da23e95884f62, 927
cfbb2bd6113745d1ecc12f28a2488a7223e362cf, 928 89633e91f5770c699e96d0f7b5bf531df150f1d2, 929
14177b8cf1fd0352187060e29c77e8095aa4bdde, 930 ac819907f479ee18026993054e714dc2e056142f, 931
3c2fa88fdf31b11aee283c308c6e0c4df4521a34. Plan hashes above are git blob hashes read from those refs.

Worktree isolation refuses pwsh, and every plan's Phase 0 needs pwsh, dotnet and msbuild. Run item
execution children without worktree isolation; an isolated child stops at its channel probe. Each
execution delegation prompt needs the prose line "Canonical issue number for this feature is N." and
a branch: label, or the model-routing hook denies it with TARGET_WORKTREE_NOT_DERIVABLE.

927 adds a repository hygiene CI guard; the other five items' evidence was planned to satisfy it.
Making the new hygiene check required in the branch ruleset is a manual maintainer step after a green
run. 928 must leave the unscoped coverage gate intact. 929 records issue 911 AC18 to AC20 as a
maintainer follow-up, not a merge gate. 931 uses no temporary files.

A pre-existing analyzer version skew (Meziantou.Analyzer 3.0.235 in project files against a newer
packages.config pin) breaks a cold restore; 882 and 929 plan a back-fill into the ignored packages
folder. 930 removes the public members ILGlobals.Cache and ILGlobals.modules.

For 927 the relaunched child's plan is authoritative. A separately cleared revision from the first
child is pinned on local branch prep/927-round6-all-clear (bac3519df) and is superseded.
