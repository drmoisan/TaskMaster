# Code Review: filesystem-wrapper-tests-open-repository-solution-file (Issue #940)

- Branch: `bug/filesystem-wrapper-tests-open-repository-solution-file-940` at `bd71b8160e281054657280af8a7c54eeffe5c563`
- Base: `origin/main` at `66afa6372fd82fc1ffd7c81f85a1ad65eebc5817` (merged at `40e587ce2`)
- Files reviewed in full: `UtilitiesCS.Test/HelperClasses/PhysicalFileSystemAdapters_Tests.cs` (446 lines), `UtilitiesCS.Test/HelperClasses/DirectoryInfoWrapper_Tests.cs` (394 lines); production context read in full: `UtilitiesCS/HelperClasses/FileSystem/PhysicalDirectoryInfoAdapter.cs`, `PhysicalFileInfoAdapter.cs`, `DirectoryInfoWrapper.cs` (unchanged)
- Review timestamp label: `2026-09-30T12-00`
- Blocking findings: **0**. Non-blocking findings: **7** (CR-1 to CR-7). Follow-ups recommended: 3 (F-1 to F-3).

## Executive Summary

The change is a test-only rewrite that removes the repository-root walk, the `TaskMaster.sln` dependency and the `IOException` swallowing from two MSTest classes, and replaces them with three owned fixtures (the loaded test-assembly image, its directory, and its parent) plus prefixed missing paths under the output directory. The rewritten tests assert delegation by thrown exception type, by wrapper type, by reference identity against sentinel streams routed through the adapter's existing internal delegate seam, and by Moq strict-mock setups. The code follows MSTest, Moq and FluentAssertions, creates no temporary file, introduces no serialization, sleep, retry or timeout, and keeps both files under the 500-line limit. Eleven negative controls demonstrate that each rewritten test fails when the delegation it verifies is broken.

No blocking defect was found. The seven non-blocking findings concern a real (idempotent) ACL write admitted by the amended acceptance criteria, coarse-grained test methods, missing AAA markers in untouched tests, a small duplication in the sentinel-stream setup, a rooted-literal probe outside the repository, a parent-directory assumption in the enumeration test, and the post-merge format check being delegated to CI.

## Findings Table

| Severity | File | Location | Finding | Recommendation | Rationale | Evidence |
| --- | --- | --- | --- | --- | --- | --- |
| Non-blocking (Minor) | `UtilitiesCS.Test/HelperClasses/PhysicalFileSystemAdapters_Tests.cs` | lines 58 and 333 (`adapter.SetAccessControl(security)`) | CR-1: `SetAccessControl` with the security object just read back is a real DACL write (identical content) on the test output directory and on the loaded assembly image. It is idempotent in content, but it is the one call in the suite that writes security metadata to an entry other test classes are concurrently reading (Workers 0, class-level parallelism), and it requires WRITE_DAC on the output tree. | Accept for this item (AC3 and AC4, as amended on 2026-09-30, admit `SetAccessControl` with an unmodified security object as a no-op by construction). File follow-up F-1: route `SetAccessControl` through the existing internal delegate seam of `PhysicalFileInfoAdapter` and a matching seam on `PhysicalDirectoryInfoAdapter`, so that the suite issues no ACL write at all. | UT4 "must not rely on mutable global state"; a DACL rewrite is a write even when the value is unchanged. Four full-suite runs and two scoped parallel runs passed, so no observed effect. | `p2-t14-ac3-mutating-call-inventory.2026-09-30T11-31.md` classes `owned output directory` and `existing owned entry`; issue.md AC3/AC4 text; `test-run-final.md`. |
| Non-blocking (Minor) | `UtilitiesCS.Test/HelperClasses/PhysicalFileSystemAdapters_Tests.cs` | `PhysicalDirectoryInfoAdapter_AccessorsAndNoOpCreation_MirrorOwnedDirectory` (lines 31 to 81), `PhysicalDirectoryInfoAdapter_Enumeration_WrapsOwnedDirectoryEntries` (84 to 187), `PhysicalFileInfoAdapter_PropertiesStreamsAndAccessors_MirrorFileInfo` (253 to 366) | CR-2: each method asserts 20 to 30 members of one type. A failure stops at the first failed assertion, so one run reports one broken member. | Keep for this item (the pre-existing files used the same shape, and splitting would exceed the 500-line limit without a second file). Consider splitting per member family in a follow-up if these tests start failing in practice. | UT1 "Isolation": failures should clearly identify the faulty unit. FluentAssertions names the member expression in the message (verified in the C1, C5 and C8 control messages), which mitigates. | Control records C1, C5, C8. |
| Non-blocking (Minor) | `UtilitiesCS.Test/HelperClasses/DirectoryInfoWrapper_Tests.cs` | `Properties_ShouldMirrorWrappedDirectoryInfo` (lines 39 to 52, no `// Act`); `PropertyDelegates_ShouldMirrorMockedIDirectoryInfo` (116 to 203), `EnumerationAndArrayMethods_ShouldDelegateToWrappedIDirectoryInfo` (206 to 348), `LifecycleAndAccessControlMethods_ShouldDelegateToWrappedIDirectoryInfo` (351 to 392) with no AAA markers | CR-3: AAA structure is present in substance but unmarked in four tests; the three Moq tests are untouched by this diff. | Leave unchanged (outside the rewrite; a marker-only edit after the negative controls would violate the plan's `WRITE SET CHANGED AFTER CONTROLS` rule). Add markers opportunistically when these tests are next touched. | UT3 Arrange-Act-Assert. | Diff spans in `p1-t4-diw-census.2026-09-30T07-31.md` (edits at pre-edit lines 30, 43 to 61, 64 to 80, 86, 374 to 391, and the fixture insert). |
| Non-blocking (Minor) | `UtilitiesCS.Test/HelperClasses/PhysicalFileSystemAdapters_Tests.cs` | lines 304 to 321 | CR-4: three separate read-only `FileStream` opens of the same image serve as three sentinels that are only compared by reference. One shared sentinel would suffice for the identity assertions of `Open(mode)`, `Open(mode, access)` and `OpenWrite()`. | Optional simplification in a follow-up; no behavior impact. | Design principle "simplicity first". | File read. |
| Non-blocking (Info) | `UtilitiesCS.Test/HelperClasses/DirectoryInfoWrapper_Tests.cs` | line 21 (`RootedFixturePath = @"C:\Repo\fixture"`), tests at 39 and 102 | CR-5: `wrapper.Exists.Should().Be(directory.Exists)` performs a read-only existence probe of a rooted path outside the repository. Both sides read the same `DirectoryInfo`, so the assertion holds whether or not the path exists on a machine; no disk write occurs. | None. Documented in the fixture comment (lines 16 to 20). | Determinism: outcome independent of the machine. | File read; control C8 and C11 messages print the literal, confirming it is a fixture constant, not a host path. |
| Non-blocking (Info) | `UtilitiesCS.Test/HelperClasses/PhysicalFileSystemAdapters_Tests.cs` | lines 25, 35, 91; `ContainSingle()` assertions over `parentAdapter` at 144 to 160 and 182 to 186 | CR-6: the enumeration test assumes the output directory's parent (`bin`) exists and contains exactly one directory named like the output directory (`Debug`). True for the Debug/AnyCPU build the plan and CI run; a shadow-copied or drive-root assembly location would break the assumption. | None for this item; note for future test authors. | Determinism under alternative runners. | File read; `evidence/qa-gates/p2-t10-scope-boundary` confirms runsettings unchanged. |
| Non-blocking (Evidence) | toolchain evidence | `evidence/qa-gates/toolchain-pass.md` line 12 vs `p2-t7-*` records | CR-7: the format check (`dotnet tool run csharpier check .`) was last run at P2-T2 on the pre-merge tree; the analyzer, nullable and coverage steps were re-run post-merge. The two changed files are hash-identical to the checked state, and the merged files come from origin/main. | Orchestrator: confirm the CI `_format-check.yml` job on the PR head before merge. | CLAUDE.md toolchain "in this exact order"; the coordinator ruling scoped the post-merge re-run. | `p2-t2-csharpier-check.2026-09-30T08-08.md`; `p2-t10-scope-boundary` hash table; plan Revision Log entry A. |

## Detailed Review

### Fixture design

- `OwnedAssemblyFile => new FileInfo(typeof(...).Assembly.Location)`, `OwnedAssemblyDirectory => OwnedAssemblyFile.Directory`, `MissingOwnedPath(suffix) => Path.Combine(OwnedAssemblyDirectory.FullName, "__940_missing_" + suffix)`. Static expression-bodied properties construct a fresh `FileInfo`/`DirectoryInfo` per access, so no `FileSystemInfo` cache is shared between tests. Verified by reading lines 22 to 28 (PFS) and 23 to 26 (DIW).
- Every missing-path test asserts `adapter.Exists.Should().BeFalse()` before acting, and every destination path (`copyTarget`, `moveTarget`, `replaceTarget`, `backupTarget`, `directoryMoveTarget`) is asserted absent with `File.Exists`/`Directory.Exists` (lines 232 to 233, 413 to 417). The BCL members then fail on the missing source before touching the destination (`CopyTo`, `MoveTo`, `Replace` throw `FileNotFoundException`; `DirectoryInfo.Delete`/`MoveTo` throw `DirectoryNotFoundException`; `FileInfo.Delete` on a missing file is a documented no-op). No path under the output directory is created.

### Delegation assertions

- Physical adapters: getters are mirrored against the same wrapped BCL object, so a broken delegation (C1: `Exists` inverted; C8: `Name` returning `FullName`) is detected while a correct one is a tautology by design. Enumeration overloads assert both membership of the owned entry and the wrapper type (`FileInfoWrapper`, `DirectoryInfoWrapper`) so the wrapping step is covered (C2, C10).
- Write-mode members of `PhysicalFileInfoAdapter` (`AppendText`, `Open(mode)`, `Open(mode, access)`, `OpenWrite`) are exercised through the internal five-argument constructor with sentinel streams and asserted `BeSameAs`, so no write or exclusive handle is requested on the image. Verified against `PhysicalFileInfoAdapter.cs` lines 34 to 48 and 118, 146 to 149, 158.
- `DirectoryInfoWrapper` delegation to `IDirectoryInfo` is asserted with `MockBehavior.Strict` mocks and `BeSameAs` on returned children (pre-existing tests); the physical route through `PhysicalDirectoryInfoAdapter` is covered by the two retargeted enumeration tests.

### Policy checks performed on the two files

| Check | Result |
| --- | --- |
| Repository root walk / `TaskMaster.sln` / `AppDomain` | absent (Grep and read) |
| `catch` of any type | absent |
| Temporary file creation | absent; no `Path.GetTemp`, `File.Create`, `File.WriteAll`, `Directory.CreateDirectory` |
| `DoNotParallelize`, `[Timeout`, `Thread.Sleep`, `Task.Delay`, retry | absent |
| Runsettings, `coverage.config`, project file | unchanged (P2-T10 `OUT-OF-SET-DIFF-EXIT: 0`, `RUNSETTINGS-HASH-NOW` equal) |
| MSTest / Moq / FluentAssertions | used as required; no MSTest `Assert` |
| File length | 446 and 394 lines (limit 500) |
| Nullable directives, suppressions, `ExcludeFromCodeCoverage` | none added |
| Absolute host paths in artifacts | none in the feature folder (executor sweep 0 matches; reviewer Grep) |

### Evidence fidelity checks

- Commit and collector clocks agree with the artifact labels: reflog epochs 1790767471 (Phase 0 commit, 07:24:31 local) / 1790768099 (fix commit, 07:34:59) / 1790781435 (plan revision, 11:17:15) / 1790782454 (final head, 11:34:14) and Cobertura roots 1790767237 (baseline, 07:20:37) / 1790781715 (final3, 11:21:55) bracket the labels `07-24`, `07-35`, `11-19`, `11-33`, `07-22`, `11-22` in order.
- The two Cobertura documents carry different root counters (56084 vs 56092 covered lines) and different class-node rates for the three carried files, so they are two runs, not one copy.
- Each of the eleven control records carries a mutated-run `MESSAGE` containing the plan's predicted phrase, a `PROD_CSC_OUT_LINES: 2` proof that the mutated production file was recompiled, a revert with `REVERT-DIFF-EXIT: 0`, the production file's SHA-256 equal to its anchor, and a confirming pass. All eleven read as the plan's Control Table predicts.

## Follow-ups (owed to the orchestrator; not filed by this item)

- F-1: seam `SetAccessControl` (both adapters) so that no ACL write remains in the unit-test suite (CR-1).
- F-2: `PhysicalFileInfoAdapter` constructor null-guard branches (6 of 12 branches uncovered, pre-existing, 50% file-level branch coverage) - add the null-argument tests for the public and internal constructors.
- F-3: `UtilitiesCS.Test/EmailIntelligence/SortEmail_Tests.cs` `TrySaveAttachmentAsync_WhenSaveSucceeds_ReturnsTrueAndCallsSaveAsFile` calls `Directory.CreateDirectory` on the repository root through production code (AC7 classification section 4, `same defect class`); promote to an issue per the repository's potential-to-issue lifecycle.

## Verdict

PASS. Blocking findings: 0. Non-blocking findings: 7. The change may proceed to the PR gates listed in `policy-audit.2026-09-30T12-00.md` section 10.
