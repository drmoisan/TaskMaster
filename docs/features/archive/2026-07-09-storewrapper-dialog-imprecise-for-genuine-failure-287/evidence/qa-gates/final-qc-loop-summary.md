Timestamp: 2026-09-01T05-20
Command: pwsh -NoProfile -Command 'git status --porcelain' (compared with the P3-T1 porcelain listing)
EXIT_CODE: 0
Output Summary: One iteration of the Phase 3 loop was required; no restart.

Gate artifacts and exit codes (single pass):
- evidence/qa-gates/final-csharpier-format.md — EXIT_CODE 0 ("Formatted 1565 files in 4606ms.")
- evidence/qa-gates/final-csharpier-check.md — EXIT_CODE 0 ("Checked 1565 files in 4414ms.")
- evidence/qa-gates/final-analyzer-build.md — EXIT_CODE 0 (Build succeeded, 5 Warning(s), 0 Error(s))
- evidence/qa-gates/final-nullable-build.md — EXIT_CODE 0 (Build succeeded, 5 Warning(s), 0 Error(s))
- evidence/qa-gates/final-coverage-test-run.md — EXIT_CODE 0 (6912/6912 passed, failed set empty)

P3-T1 porcelain listing (captured immediately after the format pass, before any further edit):
 M UtilitiesCS.Test/OutlookObjects/Store/DisabledStoresControllerTests.cs
 M UtilitiesCS.Test/OutlookObjects/Store/StoreWrapperController_Tests.Launch.cs
 M UtilitiesCS/OutlookObjects/Store/DisabledStoresController.cs
 M UtilitiesCS/OutlookObjects/Store/StoreLaunchReadinessEvaluator.cs
 M UtilitiesCS/OutlookObjects/Store/StoreWrapperController.cs
 M docs/features/active/2026-07-09-storewrapper-dialog-imprecise-for-genuine-failure-287/plan.2026-08-31T20-56.md
?? docs/features/active/2026-07-09-storewrapper-dialog-imprecise-for-genuine-failure-287/evidence/baseline/
?? docs/features/active/2026-07-09-storewrapper-dialog-imprecise-for-genuine-failure-287/evidence/other/build-after-copy-methods.md
?? docs/features/active/2026-07-09-storewrapper-dialog-imprecise-for-genuine-failure-287/evidence/other/build-after-rewiring.md
?? docs/features/active/2026-07-09-storewrapper-dialog-imprecise-for-genuine-failure-287/evidence/regression-testing/

Current porcelain listing (captured after P3-T5):
 M UtilitiesCS.Test/OutlookObjects/Store/DisabledStoresControllerTests.cs
 M UtilitiesCS.Test/OutlookObjects/Store/StoreWrapperController_Tests.Launch.cs
 M UtilitiesCS/OutlookObjects/Store/DisabledStoresController.cs
 M UtilitiesCS/OutlookObjects/Store/StoreLaunchReadinessEvaluator.cs
 M UtilitiesCS/OutlookObjects/Store/StoreWrapperController.cs
 M docs/features/active/2026-07-09-storewrapper-dialog-imprecise-for-genuine-failure-287/plan.2026-08-31T20-56.md
?? docs/features/active/2026-07-09-storewrapper-dialog-imprecise-for-genuine-failure-287/evidence/baseline/
?? docs/features/active/2026-07-09-storewrapper-dialog-imprecise-for-genuine-failure-287/evidence/other/build-after-copy-methods.md
?? docs/features/active/2026-07-09-storewrapper-dialog-imprecise-for-genuine-failure-287/evidence/other/build-after-rewiring.md
?? docs/features/active/2026-07-09-storewrapper-dialog-imprecise-for-genuine-failure-287/evidence/qa-gates/
?? docs/features/active/2026-07-09-storewrapper-dialog-imprecise-for-genuine-failure-287/evidence/regression-testing/

The two listings agree on every path outside the feature folder: the same five tracked source `M` paths and the same tracked `plan.md` M path. The only difference is the new `evidence/qa-gates/` directory, which P3-T1 through P3-T5 wrote by design after the P3-T1 porcelain capture was taken. No tracked source file moved after the P3-T1 formatting pass.
