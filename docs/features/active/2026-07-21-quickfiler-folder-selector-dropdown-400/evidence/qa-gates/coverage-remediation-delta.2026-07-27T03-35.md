# Coverage remediation delta — remediation required

Timestamp: 2026-07-27T03-35
Command: Parsed P9-T4 `coverage-final-remediation.2026-07-27T03-32.cobertura.xml` and P0 remediation baseline `coverage-remediation-baseline.2026-07-21T22-13.cobertura.xml`.
EXIT_CODE: 1
Output Summary: Repository line coverage increased from 84.15% at the P0 baseline to 84.47% in P9-T4 (91,846/108,736 lines), satisfying the repository 80% threshold. Measurable current selector class coverage is 92.62% (226/244) for `BreadcrumbPopupUiOperations` and 95.91% (211/220) for `BreadcrumbDropDownOpenCoordinator`. However, the current full-repository Cobertura XML has no `BreadcrumbDropDownOpenLifetime` class entry. P9-T6 requires every applicable measurable new/changed selector type/member/state machine, explicitly including `BreadcrumbDropDownOpenLifetime`, to have current numeric coverage. Its required value is unavailable; this is remediation required and P9-T6 remains unchecked.

## Required revision delta

Modify the P9-T6 continuation to add a deterministic, full-P9-T4-visible coverage path for `BreadcrumbDropDownOpenLifetime`, or revise the plan with a validator-backed reason that the type is non-measurable. The correction must retain full-repository coverage mode, the canonical `coverage.config` hash, and the >=90% requirement for every applicable measurable selector member/state machine before P9-T6 can be checked.
