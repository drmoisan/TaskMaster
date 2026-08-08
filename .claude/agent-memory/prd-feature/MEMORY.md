- [push-down command pattern](project_push_down_pattern.md) — 10-file change map for adding a new push-down command; reference impl is pushDownCodexAndAgentsCustomizations
- [Promotion scaffold metadata defects](project_promotion_scaffold_metadata_defects.md) — fix Status path, Last Updated date, line-wrap-shredded AC checkboxes, and DoD checkbox inflation before filling docs
- [Test disposition: grep for old-overload pins](feedback_test_disposition_overload_pins.md) — grep test project for Setup/Verify of retired overloads before marking any test file "unchanged"; loose mocks fail at run time
- [AC gates: verify satisfiability + fresh reads](feedback_ac_gates_verify_satisfiability.md) — check baseline evidence before encoding repo-wide coverage floors as blocking AC; re-read spec from disk before reporting tallies
- [Interface files are zero-denominator for coverage](reference_interface_files_zero_coverage_denominator.md) — reusable 3-proof argument (no body / net48 no DIM / no Cobertura class element) + why shape tests are rejected

## Additional entries

- [QuickFiler per-file coverage baseline](reference_quickfiler_perfile_coverage_baseline.md) — grep the #424 Cobertura artifact for indicative per-file rates before scoping an epic #136 child; most files are already above 80%
- [Ratified exemption boundaries](reference_ratified_exemption_boundaries.md) — check docs/features/archive/ for a maintainer-decision artifact before planning any [ExcludeFromCodeCoverage] removal; never promise N -> 0

## Additional entries

- [ExcludeFromCodeCoverage lambda propagation](reference_exclude_from_code_coverage_lambda_propagation.md) — method-level leaks nested lambdas into the denominator, class-level does not; a partial-class attribute exempts the whole type
