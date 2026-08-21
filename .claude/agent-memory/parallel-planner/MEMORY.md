# Parallel Planner Memory Index

- [Parallel surface status + spurious-contention defects](project_parallel_surface_partial_port.md) — #545 fixed, but 3 config/extractor defects make large runs effectively serial (83.3% density, mean cohort width 1.45)
- [Blast-radius extractor mechanics](reference_blast_radius_extractor_mechanics.md) — Get-PlanPaths only sees backtick-delimited paths; bare prose paths silently fail OPEN
- [Parallel surface cannot express ordering](project_parallel_cannot_express_ordering.md) — depends_on/wave are prohibited; lane-sequential or flight-ordered work belongs to /epic-plan
- [drm-copilot is the governance upstream](reference_drm_copilot_upstream.md) — check it when a rule/config/library a skill references is missing locally
