# Remediation Plan — Store Disable Service (F1, Issue #261) — Cycle 1

- Timestamp: 2026-07-07T23-46
- Work mode: full-feature (AC source: `spec.md` §9, AC1-AC15; this cycle targets AC15)
- Remediation inputs (authoritative finding source): `docs/features/active/2026-07-07-store-disable-service-261/remediation-inputs.2026-07-07T23-46.md`
- Scope: this is remediation of review findings on an already-implemented feature, not new
  feature work. No production source file (`*.cs` under `UtilitiesCS/`) is touched. All edits are
  to test files (`UtilitiesCS.Test/OutlookObjects/Store/StoresWrapperTests.cs`,
  `UtilitiesCS.Test/OutlookObjects/Store/StoreDisableServiceTests.cs`) plus one new test file and
  one `.csproj` wiring edit.
- Findings remediated: R1 (Blocking — 500-line file-size violation) and N1 (Non-blocking —
  unawaited async throw assertions), per the remediation-inputs document above.
- Evidence root for this cycle: `docs/features/active/2026-07-07-store-disable-service-261/evidence/`
  (canonical `<FEATURE>/evidence/<kind>/` scheme only; no `artifacts/` evidence paths are used
  anywhere in this plan).

## Planned File Split (R1)

- `UtilitiesCS.Test/OutlookObjects/Store/StoresWrapperTests.cs` — currently 688 lines. Remove the
  6 `InclusionFilters_*` test methods (current lines 272-385, 114 lines), the F1 disabled-store
  comment + 5 test methods (current lines 386-509, 124 lines), and the now-orphaned private helper
  `AssertInclusionDecision` (current lines 511-543, 33 lines; it has no remaining caller once the
  6 `InclusionFilters_*` tests move). Total removed: 271 lines. Projected resulting size: **417
  lines**. `CreateGlobalsWithStores`, `CreateStore`, and `CreateRootFolderWithPrimarySmtpAddress`
  remain in this file unchanged because the file's other retained tests
  (`CreateAsync_WhenInputsValid_ReturnsInitializedStoresWrapper`,
  `Init_WhenStoresMatchFilters_ProjectsOnlyIncludedStores`, both `RewireOlObjectsAsync_*` tests,
  `RewireAfterDeserializeAsync_PublicEntryHitsRealMethodBody`) still call them.
- `UtilitiesCS.Test/OutlookObjects/Store/StoresWrapperDisableTests.cs` — new file. Contains: the 6
  moved `InclusionFilters_*` test methods, the 5 moved F1 disabled-store test methods (with their
  preceding comment), the moved (not duplicated) `AssertInclusionDecision` helper (its only two
  caller groups both moved here), and **duplicated** (not moved) private copies of
  `CreateGlobalsWithStores`, `CreateStore`, and `CreateRootFolderWithPrimarySmtpAddress` because
  the moved tests call them and the originals must stay in `StoresWrapperTests.cs` for its
  retained tests. Projected size: usings/namespace/class boilerplate (~17 lines) + 238 lines of
  moved test methods + 106 lines of duplicated/moved helpers ≈ **361 lines**.
- Both projected sizes (417 and 361) are comfortably under the 500-line cap. Exact post-edit line
  counts are captured and verified by P1-T6/P1-T7 and re-verified after formatting by P2-T7; the
  projections above are not treated as the recorded evidence.
- `UtilitiesCS.Test/UtilitiesCS.Test.csproj` (legacy packages.config project, no glob) gets one new
  `<Compile Include="OutlookObjects\Store\StoresWrapperDisableTests.cs" />` item next to the
  existing `StoresWrapperTests.cs` item.

## N1 Fix (Non-blocking, folded into this pass)

- `UtilitiesCS.Test/OutlookObjects/Store/StoreDisableServiceTests.cs`: two test methods
  (`Writes_ThrowArgumentException_ForSentinelIdentity` at lines ~211-230,
  `Writes_ThrowInvalidOperation_WhenModelIsNull` at lines ~247-264) call
  `.Should().ThrowAsync<...>()` on the `ReenableAsync` guard path without `await`, so that
  assertion never executes. Both methods change from `public void` to `public async Task` and gain
  an `await` on the `ReenableAsync` assertion line.

---

### Phase 0 — Policy Reads and Remediation Baseline

- [x] [P0-T1] Read `C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-a957d835cc071fcf9\CLAUDE.md`
      in full (this worktree's copy, not any other worktree path). Acceptance: file read
      confirmed and its C# coverage exemption clause (UT2, 80% testable-denominator floor / 90%
      new-code) and 500-line file limit (§4.1) are recorded verbatim in
      `docs/features/active/2026-07-07-store-disable-service-261/evidence/remediation-baseline/phase0-instructions-read.md`.
- [x] [P0-T2] Read `C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-a957d835cc071fcf9\.claude\rules\general-code-change.md`
      in full. Acceptance: file read confirmed and appended to the same
      `phase0-instructions-read.md` artifact with `Timestamp:` and `Policy Order:` fields.
- [x] [P0-T3] Read `C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-a957d835cc071fcf9\.claude\rules\general-unit-test.md`
      in full. Acceptance: file read confirmed and appended to the same artifact; note in the
      artifact that this remediation follows CLAUDE.md's explicit COM/VSTO coverage-exemption
      thresholds (already the established baseline for this feature per `spec.md` AC15 delivery
      annotations), not the generic 85%/75% figures in this file, per the Policy Compliance Order
      precedence (CLAUDE.md first).
- [x] [P0-T4] Read `C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-a957d835cc071fcf9\.claude\rules\csharp.md`
      in full. Acceptance: file read confirmed and appended to the same artifact, completing the
      explicit list of files read required by the Phase 0 contract.
- [x] [P0-T5] Capture the current line count of
      `UtilitiesCS.Test\OutlookObjects\Store\StoresWrapperTests.cs` by running
      `(Get-Content 'UtilitiesCS.Test\OutlookObjects\Store\StoresWrapperTests.cs' | Measure-Object -Line).Lines`
      from the worktree root. Acceptance: exit code 0 and the numeric line count (expected 688) is
      recorded with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` in
      `docs/features/active/2026-07-07-store-disable-service-261/evidence/remediation-baseline/wc-stores-wrapper-tests-before.md`.
- [x] [P0-T6] Capture the current line count of
      `UtilitiesCS.Test\OutlookObjects\Store\StoreDisableServiceTests.cs` by running
      `(Get-Content 'UtilitiesCS.Test\OutlookObjects\Store\StoreDisableServiceTests.cs' | Measure-Object -Line).Lines`
      from the worktree root. Acceptance: exit code 0 and the numeric line count recorded with
      `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` in
      `docs/features/active/2026-07-07-store-disable-service-261/evidence/remediation-baseline/wc-store-disable-service-tests-before.md`.
- [x] [P0-T7] Confirm the two unawaited N1 locations exist as described by running
      `Select-String -Path 'UtilitiesCS.Test\OutlookObjects\Store\StoreDisableServiceTests.cs' -Pattern 'ThrowAsync<'`
      from the worktree root. Acceptance: exit code 0, output shows exactly 4 matches (2 awaited
      `DisableSessionOnly`/`DisableForFutureSessions` calls use `Throw<...>` not `ThrowAsync<...>`
      so are unaffected; the 2 `ReenableAsync` `.ThrowAsync<...>()` calls at
      `Writes_ThrowArgumentException_ForSentinelIdentity` and
      `Writes_ThrowInvalidOperation_WhenModelIsNull` are the only `ThrowAsync<` occurrences and are
      confirmed not preceded by `await` on their own statement), recorded with `Timestamp:`,
      `Command:`, `EXIT_CODE:`, `Output Summary:` in
      `docs/features/active/2026-07-07-store-disable-service-261/evidence/remediation-baseline/n1-location-confirmation.md`.
- [x] [P0-T8] Capture the current repo-wide test count and coverage baseline by running
      `pwsh ./scripts/vscode/Invoke-MSTestWithCoverage.ps1 -CoverageOutput coverage\remediation-cycle1-baseline.cobertura.xml`
      from the worktree root (the repo's canonical numeric-coverage path, wrapping
      `vstest.console.exe` with `dotnet-coverage collect`). Acceptance: exit code 0, total
      tests/passed/failed counts recorded (expected 5032 passing per remediation-inputs), and the
      numeric repo-wide line-coverage percentage recorded with `Timestamp:`, `Command:`,
      `EXIT_CODE:`, `Output Summary:` in
      `docs/features/active/2026-07-07-store-disable-service-261/evidence/remediation-baseline/test-coverage-baseline-cycle1.md`.

### Phase 1 — R1 File Split and N1 Await Fix

- [x] [P1-T1] Add `<Compile Include="OutlookObjects\Store\StoresWrapperDisableTests.cs" />`
      immediately after the existing `<Compile Include="OutlookObjects\Store\StoresWrapperTests.cs" />`
      item in `UtilitiesCS.Test\UtilitiesCS.Test.csproj`. Acceptance: the new `<Compile Include>`
      item is present in the file at that location (legacy packages.config project — no glob — so
      the new file will not compile without this item).
- [x] [P1-T2] Create `UtilitiesCS.Test\OutlookObjects\Store\StoresWrapperDisableTests.cs` containing
      only: the `using` directives `System`, `System.Collections`, `System.Collections.Generic`,
      `System.Linq`, `FluentAssertions`, `Microsoft.Office.Interop.Outlook`,
      `Microsoft.VisualStudio.TestTools.UnitTesting`, `Moq`, `UtilitiesCS`,
      `UtilitiesCS.OutlookObjects.Store`, and the `OutlookFolder`/`OutlookStore` aliases
      (`using OutlookFolder = Microsoft.Office.Interop.Outlook.Folder;` /
      `using OutlookStore = Microsoft.Office.Interop.Outlook.Store;`), the `namespace
      UtilitiesCS.Test.OutlookObjects.Store` block, and an empty
      `[TestClass] public class StoresWrapperDisableTests { }` body. Acceptance: the file exists at
      that path with exactly this skeleton content (no test methods yet).
- [x] [P1-T3] Into the class body of `StoresWrapperDisableTests.cs` created in P1-T2, paste verbatim
      (byte-identical bodies, no assertion changes) the four private static helper methods currently
      defined in `StoresWrapperTests.cs`: `CreateGlobalsWithStores`, `CreateStore`,
      `CreateRootFolderWithPrimarySmtpAddress`, and `AssertInclusionDecision`. Acceptance: all four
      methods exist in `StoresWrapperDisableTests.cs` with signatures and bodies identical to their
      current source in `StoresWrapperTests.cs`.
- [x] [P1-T4] Append verbatim (unchanged assertions) into `StoresWrapperDisableTests.cs` the 6
      `[TestMethod]` blocks currently at `StoresWrapperTests.cs` lines 272-385:
      `InclusionFilters_ExcludePublicFoldersWhenConfigured`,
      `InclusionFilters_ExcludeMatchingDisplayNames_IgnoringCase` (including its two `[DataRow]`
      attributes), `InclusionFilters_ExcludeMatchingGwsoPaths_IgnoringCase`,
      `InclusionFilters_ExcludeMatchingFilePaths_IgnoringWhitespaceEntries`,
      `InclusionFilters_WhenFilePathAccessThrows_TreatsPathAsUnavailable`,
      `InclusionFilters_WhenNoExclusionMatches_ReturnsTrue`. Acceptance: all 6 methods exist in
      `StoresWrapperDisableTests.cs` with bodies identical to the pre-move source.
- [x] [P1-T5] Append verbatim (unchanged assertions) into `StoresWrapperDisableTests.cs`, after the
      tests added in P1-T4, the comment
      `// --- Disabled-store filter integration + persistence (P7-T4, issue #261) ---` and the 5
      `[TestMethod]` blocks currently at `StoresWrapperTests.cs` lines 386-509:
      `ShouldIncludeStore_ExcludesSessionDisabledStore_KeepsNonDisabled`,
      `ShouldIncludeStore_ExcludesFutureDisabledStore_KeepsNonDisabled`,
      `StoreIsIncluded_WhenIsDisabledTrue_ReturnsFalse`,
      `Init_ExcludesSessionAndFutureDisabledStores_ViaInstrumentedPath`,
      `Serialization_RoundTrip_PreservesDisabledListAndOmitsSessionSet`. Acceptance: the comment and
      all 5 methods exist in `StoresWrapperDisableTests.cs` with bodies identical to the pre-move
      source.
- [x] [P1-T6] In `StoresWrapperTests.cs`, delete the 6 `InclusionFilters_*` test methods (former
      lines 272-385), the disabled-store comment + 5 test methods (former lines 386-509), and the
      now-orphaned `AssertInclusionDecision` private helper (former lines 511-543). Do not delete
      `CreateGlobalsWithStores`, `CreateStore`, or `CreateRootFolderWithPrimarySmtpAddress`.
      Acceptance: `StoresWrapperTests.cs` no longer contains any of these 12 members, and every
      remaining test method's body is textually unchanged from before this task.
- [x] [P1-T7] Verify the post-split line count of `StoresWrapperTests.cs` by running
      `(Get-Content 'UtilitiesCS.Test\OutlookObjects\Store\StoresWrapperTests.cs' | Measure-Object -Line).Lines`
      from the worktree root. Acceptance: exit code 0 and the reported count is <= 500 (projected
      417), recorded with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` in
      `docs/features/active/2026-07-07-store-disable-service-261/evidence/remediation-baseline/wc-stores-wrapper-tests-after-split.md`.
- [x] [P1-T8] Verify the line count of `StoresWrapperDisableTests.cs` by running
      `(Get-Content 'UtilitiesCS.Test\OutlookObjects\Store\StoresWrapperDisableTests.cs' | Measure-Object -Line).Lines`
      from the worktree root. Acceptance: exit code 0 and the reported count is <= 500 (projected
      361), recorded with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` in
      `docs/features/active/2026-07-07-store-disable-service-261/evidence/remediation-baseline/wc-stores-wrapper-disable-tests-after-split.md`.
- [x] [P1-T9] In `UtilitiesCS.Test\OutlookObjects\Store\StoreDisableServiceTests.cs`, change
      `public void Writes_ThrowArgumentException_ForSentinelIdentity()` to
      `public async Task Writes_ThrowArgumentException_ForSentinelIdentity()` and add `await`
      immediately before
      `service.Invoking(s => s.ReenableAsync(sentinel)).Should().ThrowAsync<ArgumentException>();`.
      Acceptance: the method signature is `public async Task
      Writes_ThrowArgumentException_ForSentinelIdentity()` and the `ReenableAsync` assertion
      statement begins with `await`; the two preceding synchronous `Throw<ArgumentException>()`
      assertions in the same method are unchanged.
- [x] [P1-T10] In the same file, change
      `public void Writes_ThrowInvalidOperation_WhenModelIsNull()` to
      `public async Task Writes_ThrowInvalidOperation_WhenModelIsNull()` and add `await`
      immediately before
      `service.Invoking(s => s.ReenableAsync(StoreIdentity.Resolve(StoreName))).Should().ThrowAsync<InvalidOperationException>();`.
      Acceptance: the method signature is `public async Task
      Writes_ThrowInvalidOperation_WhenModelIsNull()` and the `ReenableAsync` assertion statement
      begins with `await`; the two preceding synchronous `Throw<InvalidOperationException>()`
      assertions in the same method are unchanged.

### Phase 2 — Final QA Loop

Run the full C# toolchain in this exact order; if any step fails or changes files, restart the
loop from P2-T1.

- [x] [P2-T1] Run `dotnet tool run csharpier .` from the worktree root. Acceptance: exit code 0 and
      no files reported as reformatted (if files are reformatted, restart the loop from this task);
      recorded with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` in
      `docs/features/active/2026-07-07-store-disable-service-261/evidence/qa-gates/qa-01-format-cycle1.md`.
- [x] [P2-T2] Run
      `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
      from the worktree root. Acceptance: exit code 0 with zero analyzer errors/warnings on the
      touched files, recorded with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` in
      `docs/features/active/2026-07-07-store-disable-service-261/evidence/qa-gates/qa-02-analyzers-cycle1.md`.
- [x] [P2-T3] Run
      `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true`
      from the worktree root. Acceptance: exit code 0 with zero nullable warnings on the touched
      files, recorded with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` in
      `docs/features/active/2026-07-07-store-disable-service-261/evidence/qa-gates/qa-03-nullable-cycle1.md`.
- [x] [P2-T4] Run
      `vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll TaskMaster.Test\bin\Debug\TaskMaster.Test.dll /EnableCodeCoverage`
      from the worktree root. Acceptance: exit code 0, all tests pass, no test count decrease in
      the two named assemblies, recorded with `Timestamp:`, `Command:`, `EXIT_CODE:`,
      `Output Summary:` in
      `docs/features/active/2026-07-07-store-disable-service-261/evidence/qa-gates/qa-04-mstest-cycle1.md`.
- [x] [P2-T5] Run
      `pwsh ./scripts/vscode/Invoke-MSTestWithCoverage.ps1 -CoverageOutput coverage\remediation-cycle1-post-change.cobertura.xml`
      from the worktree root to obtain the numeric repo-wide coverage figure (the plain
      `vstest /EnableCodeCoverage` run in P2-T4 emits a binary `.coverage` file that is not
      offline-convertible to a percentage in this environment, per the feature's existing baseline
      evidence). Acceptance: exit code 0, total tests/passed/failed counts recorded (expected 5032
      passing, unchanged from P0-T8), and the numeric repo-wide line-coverage percentage recorded
      with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` in
      `docs/features/active/2026-07-07-store-disable-service-261/evidence/qa-gates/qa-05-coverage-post-change-cycle1.md`.
- [x] [P2-T6] Compare the P0-T8 baseline coverage/test-count figures against the P2-T5 post-change
      figures and record a delta/threshold verification in
      `docs/features/active/2026-07-07-store-disable-service-261/evidence/qa-gates/qa-06-coverage-delta-cycle1.md`
      with explicit `Baseline coverage:`, `Post-change coverage:`, `Test count baseline:`,
      `Test count post-change:` fields. Acceptance: post-change test count equals the baseline
      count (5032 passing, 0 failed) and post-change coverage shows no regression versus baseline
      (this remediation moves and reformats test code only; it adds no production code, so the
      AC15 new-code >= 90% obligation carries no new denominator and is satisfied by inspection,
      recorded as such in this artifact).
- [x] [P2-T7] Re-verify file sizes after formatting by running
      `(Get-Content 'UtilitiesCS.Test\OutlookObjects\Store\StoresWrapperTests.cs' | Measure-Object -Line).Lines`
      and
      `(Get-Content 'UtilitiesCS.Test\OutlookObjects\Store\StoresWrapperDisableTests.cs' | Measure-Object -Line).Lines`
      from the worktree root. Acceptance: both exit codes 0 and both reported counts are <= 500,
      recorded with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` in
      `docs/features/active/2026-07-07-store-disable-service-261/evidence/qa-gates/qa-07-file-size-final-cycle1.md`.
- [x] [P2-T8] Confirm the N1 fix is exercised by inspecting the P2-T4 MSTest run output for
      `Writes_ThrowArgumentException_ForSentinelIdentity` and
      `Writes_ThrowInvalidOperation_WhenModelIsNull`, verifying both report as passed individual
      test results (not silently skipped as fire-and-forget). Acceptance: both test names appear
      as passed in the P2-T4 test result output, recorded in
      `docs/features/active/2026-07-07-store-disable-service-261/evidence/qa-gates/qa-08-n1-verification-cycle1.md`.
- [x] [P2-T9] Write the AC15 re-confirmation artifact
      `docs/features/active/2026-07-07-store-disable-service-261/evidence/qa-gates/ac15-reconfirmation-cycle1.md`
      summarizing: full toolchain green (P2-T1..P2-T4), both split files <= 500 lines (P2-T7), no
      test-count or coverage regression (P2-T6), and R1 + N1 both resolved. Acceptance: the
      artifact exists with `Timestamp:` and explicit references to the P2-T1 through P2-T8
      evidence files as support for the AC15 PASS determination.
