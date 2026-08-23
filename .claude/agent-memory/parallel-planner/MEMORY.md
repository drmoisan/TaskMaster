# Parallel Planner Memory Index

- [Bug corpus is 71% QuickFiler](project_bug_corpus_is_quickfiler_concentrated.md) — measure the module histogram before fanning out; "all bugs" is 51 cohorts for 72 items, so max_concurrency is inert
- [Parallel surface status + spurious-contention defects](project_parallel_surface_partial_port.md) — #545 fixed, but 3 config/extractor defects make large runs effectively serial (83.3% density, mean cohort width 1.45)
- [TaskMaster lacks extensions/ and scripts/dev_tools/](reference_taskmaster_lacks_python_toolchain.md) — drm-copilot MCP bugs aren't fixable here; use the PS/bash ports, never `poetry run python -m scripts.dev_tools`
- [Blast-radius extractor mechanics](reference_blast_radius_extractor_mechanics.md) — Get-PlanPaths only sees backtick-delimited paths; bare prose paths silently fail OPEN
- [Parallel surface cannot express ordering](project_parallel_cannot_express_ordering.md) — depends_on/wave are prohibited; lane-sequential or flight-ordered work belongs to /epic-plan
- [drm-copilot is the governance upstream](reference_drm_copilot_upstream.md) — check it when a rule/config/library a skill references is missing locally
