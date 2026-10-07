# Fail-Before Exception Dossier (P1-T2)

Timestamp: 2026-09-30T07-28

WhyFailingRunImpossible: The defect is decided by whether another process holds TaskMaster.sln with a share mode that excludes readers, or by whether the repository layout places a solution file above the test assembly. A committed test can arrange neither without starting an external process or relocating the assembly, and the unit-test policy prohibits both, so no committed test can deterministically reproduce the failure before the fix.

## Alternative Proof

- Static reproduction, as re-recorded by P0-T12 (FEATURE/evidence/baseline/p0-t12-pre-edit-census.2026-09-30T07-22.md): UtilitiesCS.Test/HelperClasses/PhysicalFileSystemAdapters_Tests.cs walks up from the test assembly to the directory holding TaskMaster.sln in `GetRepositoryRoot()` (lines 349 to 371) and returns the solution file itself from `GetSolutionFile()` (lines 373 to 377); the pre-edit tests then perform real mutating calls on those two entries (fact 2: the `Attributes` and six timestamp setters, `Create()`, `Create(security)`, `CreateSubdirectory` and `SetAccessControl` on the repository root; the six timestamp setters, `IsReadOnly` and `SetAccessControl` on TaskMaster.sln; and `CopyTo` and `Replace` naming the solution file as destination). UtilitiesCS.Test/HelperClasses/DirectoryInfoWrapper_Tests.cs walks up the same way in its `GetRepositoryRoot()` (lines 375 to 391) and asserts the presence of TaskMaster.sln in its enumeration assertions at lines 60 and 79. Each of these sites opens or modifies the solution file, so a concurrent holder of that file with an excluding share mode makes the test fail with an I/O exception unrelated to the unit under test.
- Recorded failure of the same defect class: the #906 failure observed during the #900 run, cited through the #931 dossier docs/features/active/2026-09-28-tests-depend-on-uncontrolled-environment-931/evidence/regression-testing/fail-before-exception.2026-09-29T09-07.md.
- Deterministic observed-failing evidence: P1-T9 through P1-T30 later demonstrate deterministically that each rewritten test fails against a deliberately broken wrapper or adapter and passes once the break is reverted. This dossier states that plan; it records no result of those tasks.

SearchScope: FEATURE/evidence/regression-testing/
SearchPatterns: `fail-before-exception.*.md`
SearchResult: this file (fail-before-exception.2026-09-30T07-28.md)

## Output Summary

Fail-before run structurally impossible without an external process holding the solution file or a relocated assembly; alternative proof recorded (static reproduction from the P0-T12 pre-edit census, the #906 failure through the #931 dossier, and the eleven negative controls P1-T9 through P1-T30 as the deterministic observed-failing evidence).
