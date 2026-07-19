# Batch 4 — Regression Tests (Issue #364)

- Timestamp: 2026-07-19T09-35
- Task: [P4-T10]
- Command: `vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /InIsolation /TestCaseFilter:"FullyQualifiedName~FileSystemInfoWrapper|FullyQualifiedName~DirectoryInfoWrapper|FullyQualifiedName~FileInfoWrapper|FullyQualifiedName~PhysicalFileSystemAdapters|FullyQualifiedName~MyFileSystemInfo"`
- EXIT_CODE: 0

## Invocation Note

`/EnableCodeCoverage` omitted for the targeted per-batch run (coverage captured at P0-T5 / P9-T4). `/InIsolation` required for the Moq assembly. Test assembly rebuilt before the run.

## Output Summary

- Test Run Successful.
- Total tests: 56; Passed: 56; Failed: 0. Total time ~2.24s.
- All Batch-4 tests green and behavior-identical. The `PhysicalFileSystemAdapters` tests are deterministic (no reintroduced shared-file flakiness); the injectable-delegate seam was preserved exactly.
