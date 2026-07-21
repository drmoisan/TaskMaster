# Batch F3a Tests With Coverage (P4-T4)

Timestamp: 2026-07-19T12-25

Command: `pwsh scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot UtilitiesCS.Test -CoverageOutput docs/features/active/2026-07-18-utilitiescs-nullable-outlook-folder-store-365/evidence/regression-testing/batch-f3a-coverage.cobertura.xml`

EXIT_CODE: 0

Output Summary: Test Run Successful. Total tests: 4511, Passed: 4511, Failed: 0. No regression in
FolderConverterTests, FolderConverter_Tests, FolderNavigatorTests, FolderMinimalWrapperTests,
FolderWrapperCoverageExpansionTests, FolderWrapperStateTests, FolderWrapperTraversalTests (AC3).
Coverage: line-rate="0.652979" branch-rate="0.613466" . No regression vs baseline.

Flakiness note: the full-suite dotnet-coverage run aborted early (exit -1, partial pass) twice before a clean
4511/4511 pass on the third invocation — the known infra nondeterminism, unrelated to the annotation changes.
