# Remediation Plan (Cycle 4): FilePathHelper deserialize-safe (#177 / F6 / AC25)

**Cycle:** 4
**Plan timestamp:** 2026-06-16T10-26
**Work Mode:** full-bug (defect remediation; bugfix workflow applies)
**Feature folder:** `docs/features/active/2026-06-08-hierarchical-lcppn-folder-prediction-177`
**Spec source:** `docs/features/active/2026-06-08-hierarchical-lcppn-folder-prediction-177/2026-06-16T10-26-remediation/remediation-inputs.2026-06-16T10-26.md`
**Research source:** `artifacts/research/2026-06-16-filepathhelper-deserialization-nre-research.md`
**Acceptance criterion:** AC25 (user-story.md lines 145-153)

## Scope (single finding — do not exceed)

One finding only: F6 / AC25. Root-cause a `NullReferenceException` raised when Json.NET
default-constructor + property-set deserialization populates a `FilePathHelper`'s `FileStemSeed`
before `_fileExtension` / `_fileStemSuffix` are set, causing the property-change handler to invoke
`AdjustForMaxPath()` and dereference null backing fields.

Files in scope (only these two may be edited):
- Production: `UtilitiesCS/HelperClasses/FileSystem/FilePathHelper.cs`
- Test: `UtilitiesCS.Test/HelperClasses/FilePathHelper_Tests.cs`

## Invariants (must hold; verified by tasks below)

- **INV-1 (retain workaround).** The cycle-3 `DoNotSerializeContractResolver("Config")` exclusion in
  `LcppnFolderPredictorStore.BuildSettings()` is RETAINED. It is not reverted, weakened, or edited.
  AC23 remains satisfied with the exclusion in place.
- **INV-2 (containment).** Zero edits to any file other than the two in-scope files. Specifically no
  edits to `LcppnFolderPredictor*`, the spam/triage/category/actionable subsystems,
  `ManagerAsyncLazy`, or the cycle-3 enablement/persistence files. AC1-AC24 must not regress.
- **INV-3 (contract preservation).** No serialized-document shape change; no public-API change to
  `FilePathHelper`. The guard is the only behavioral change: an early `return false` when
  `_fileExtension`, `_fileStemSuffix`, or `_fileStemSeed` is null, placed immediately after the
  existing `StemInitialized()` check (after current line 295).
- **INV-4 (file-size cap).** `FilePathHelper.cs` must not exceed 500 lines after the guard is added.
  Its current size is captured at Phase 0; if it is already at or over cap as a pre-existing overage,
  the guard adds only the minimal lines and the overage is recorded but not widened. The test file
  must remain <= 500 lines.
- **INV-5 (bugfix red-before-green).** The failing regression test is authored and confirmed red
  BEFORE the production guard is applied, then confirmed green after.

## Evidence locations (canonical, non-overridable)

All evidence is written under
`docs/features/active/2026-06-08-hierarchical-lcppn-folder-prediction-177/evidence/<kind>/`.
`<kind>` is one of `baseline`, `regression-testing`, `qa-gates`, `other`. Writing to any
`artifacts/...` evidence path is a policy violation and is rejected.

---

### Phase 0 — Baseline capture and policy reads

- [x] [P0-T1] Read policy files in required order and record the read evidence. Files to read, in
  order: `CLAUDE.md`; `.claude/rules/general-code-change.md`; `.claude/rules/general-unit-test.md`;
  `.claude/rules/csharp.md`; the spec
  `docs/features/active/2026-06-08-hierarchical-lcppn-folder-prediction-177/2026-06-16T10-26-remediation/remediation-inputs.2026-06-16T10-26.md`;
  the research `artifacts/research/2026-06-16-filepathhelper-deserialization-nre-research.md`.
  **Acceptance:** evidence file
  `docs/features/active/2026-06-08-hierarchical-lcppn-folder-prediction-177/evidence/baseline/phase0-instructions-read.2026-06-16T10-26.md`
  exists with `Timestamp:`, `Policy Order:`, and the explicit list of files read.

- [x] [P0-T2] Capture current line counts of the two in-scope files. Record line count of
  `UtilitiesCS/HelperClasses/FileSystem/FilePathHelper.cs` and of
  `UtilitiesCS.Test/HelperClasses/FilePathHelper_Tests.cs`, and explicitly state whether
  `FilePathHelper.cs` is already at/over the 500-line cap (pre-existing overage flag for INV-4).
  **Acceptance:** evidence file
  `docs/features/active/2026-06-08-hierarchical-lcppn-folder-prediction-177/evidence/baseline/phase0-linecounts.2026-06-16T10-26.md`
  records both counts, the over-cap determination for `FilePathHelper.cs`, and confirms the test file
  is under 500.

- [x] [P0-T3] Confirm the test file is already registered for compilation. Verify
  `UtilitiesCS.Test/UtilitiesCS.Test.csproj` contains a `<Compile Include="HelperClasses\FilePathHelper_Tests.cs" />`
  entry (it exists today at csproj line 221), so no csproj edit is required for the existing test file.
  **Acceptance:** evidence file
  `docs/features/active/2026-06-08-hierarchical-lcppn-folder-prediction-177/evidence/baseline/phase0-csproj-registration.2026-06-16T10-26.md`
  records the matched `<Compile Include>` line and the conclusion "no csproj edit required".

- [x] [P0-T4] Capture baseline formatting state. Run `dotnet tool run csharpier --check .` (or
  `csharpier --check .`) at repo root.
  **Acceptance:** evidence file
  `docs/features/active/2026-06-08-hierarchical-lcppn-folder-prediction-177/evidence/baseline/phase0-csharpier.2026-06-16T10-26.md`
  contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`.

- [x] [P0-T5] Capture baseline analyzer build state. Run
  `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`.
  **Acceptance:** evidence file
  `docs/features/active/2026-06-08-hierarchical-lcppn-folder-prediction-177/evidence/baseline/phase0-analyzers.2026-06-16T10-26.md`
  contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`.

- [x] [P0-T6] Capture baseline nullable/type-check build state. Run
  `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true`.
  **Acceptance:** evidence file
  `docs/features/active/2026-06-08-hierarchical-lcppn-folder-prediction-177/evidence/baseline/phase0-nullable.2026-06-16T10-26.md`
  contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`.

- [x] [P0-T7] Capture baseline test + coverage state for the in-scope test assembly. Run
  `vstest.console.exe <UtilitiesCS.Test assembly path> /EnableCodeCoverage` and record the existing
  `FilePathHelper_Tests` pass count plus the numeric repository/assembly coverage headline as the
  baseline for the changed-line coverage delta check in the final phase.
  **Acceptance:** evidence file
  `docs/features/active/2026-06-08-hierarchical-lcppn-folder-prediction-177/evidence/baseline/phase0-tests-coverage.2026-06-16T10-26.md`
  contains `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:` with numeric baseline
  coverage percent.

- [x] [P0-T8] Record AC23 baseline state. Confirm and document that the cycle-3
  `DoNotSerializeContractResolver("Config")` exclusion is present in
  `LcppnFolderPredictorStore.BuildSettings()` and that the AC23-related tests currently pass (from the
  P0-T7 run), establishing the pre-change AC23-green baseline for INV-1 re-verification.
  **Acceptance:** evidence file
  `docs/features/active/2026-06-08-hierarchical-lcppn-folder-prediction-177/evidence/baseline/phase0-ac23-baseline.2026-06-16T10-26.md`
  records the located `DoNotSerializeContractResolver("Config")` call site and the AC23 test pass
  status before any change.

---

### Phase 1 — Failing regression test (red before green)

- [ ] [P1-T1] Add the throw-reproduction regression test, expected to FAIL before the fix. In
  `UtilitiesCS.Test/HelperClasses/FilePathHelper_Tests.cs` add MSTest method
  `DeserializeFromSeedJson_WhenFileStemSeedSetBeforeExtension_DoesNotThrow`: arrange a JSON document
  for a `FilePathHelper` with `FileStemSeed` listed first (before `FileStemSuffix`, `FileExtension`,
  `FolderPath`); act `JsonConvert.DeserializeObject<FilePathHelper>(json)` with default settings; assert
  via FluentAssertions that the call does not throw and the deserialized `FileStemSeed` matches the
  source. No temp files, no real filesystem, no Moq. **[expect-fail]**
  **Acceptance:** the new test method exists in the file; the test is exercised and confirmed RED
  (throws `JsonSerializationException` wrapping `NullReferenceException`) against the unmodified
  production code; evidence file
  `docs/features/active/2026-06-08-hierarchical-lcppn-folder-prediction-177/evidence/regression-testing/p1-failing-test.2026-06-16T10-26.md`
  records `Timestamp:`, `Command:`, `EXIT_CODE:` (non-zero), and `Output Summary:` showing the failing
  assertion/exception.

- [ ] [P1-T2] Add the round-trip regression test. In the same file add MSTest method
  `DeserializeFromSeedJson_RoundTrip_PreservesAllStemFields`: arrange a `FilePathHelper` created via
  `FilePathHelper.FromSeed("report", ".json", "_bk", @"C:\data")` (confirm the exact `FromSeed`
  signature in the production file before authoring); act serialize via `JsonConvert.SerializeObject`
  then deserialize via `JsonConvert.DeserializeObject<FilePathHelper>`; assert via FluentAssertions
  that `FileStemSeed`, `FileStemSuffix`, `FileExtension`, and `FolderPath` all equal the source.
  No temp files, no real filesystem, no Moq.
  **Acceptance:** the new test method exists; it compiles; evidence file
  `docs/features/active/2026-06-08-hierarchical-lcppn-folder-prediction-177/evidence/regression-testing/p1-roundtrip-test-added.2026-06-16T10-26.md`
  records the method name and that the test file still compiles and remains <= 500 lines (INV-4).

- [ ] [P1-T3] Verify test-file containment and size after additions. Confirm the only edited test
  file is `UtilitiesCS.Test/HelperClasses/FilePathHelper_Tests.cs`, it remains <= 500 lines, and the
  existing `<Compile Include>` registration still covers it (no csproj edit).
  **Acceptance:** evidence file
  `docs/features/active/2026-06-08-hierarchical-lcppn-folder-prediction-177/evidence/regression-testing/p1-test-containment.2026-06-16T10-26.md`
  records the post-edit line count (<= 500) and confirms no other file changed.

---

### Phase 2 — Minimal targeted production fix (green)

- [ ] [P2-T1] Apply the null-guard in the instance `AdjustForMaxPath()`. In
  `UtilitiesCS/HelperClasses/FileSystem/FilePathHelper.cs`, immediately after the existing
  `if (!StemInitialized()) return false;` check (current lines 294-295) and before the
  `maxSeedLength` arithmetic (current line 297-298), add a guard that returns `false` when any of
  `_fileExtension`, `_fileStemSuffix`, or `_fileStemSeed` is null, with a short `// why` comment
  noting it guards partial initialization during Json.NET deserialization. Use the backing fields
  (not the public getters). No other change to the method body; no public-API change; no serialized-
  shape change (INV-3).
  **Acceptance:** the guard is present at the specified location using backing-field null checks; the
  method signature and return type are unchanged; evidence file
  `docs/features/active/2026-06-08-hierarchical-lcppn-folder-prediction-177/evidence/other/p2-guard-applied.2026-06-16T10-26.md`
  records the exact added lines and confirms they are the only production change.

- [ ] [P2-T2] Confirm the previously-red regression test now passes. Re-run
  `DeserializeFromSeedJson_WhenFileStemSeedSetBeforeExtension_DoesNotThrow` and
  `DeserializeFromSeedJson_RoundTrip_PreservesAllStemFields` against the patched production code.
  **Acceptance:** both tests pass (green); evidence file
  `docs/features/active/2026-06-08-hierarchical-lcppn-folder-prediction-177/evidence/regression-testing/p2-tests-green.2026-06-16T10-26.md`
  records `Timestamp:`, `Command:`, `EXIT_CODE: 0`, and `Output Summary:` showing both tests passing
  (red-before-green demonstrated when paired with P1-T1).

- [ ] [P2-T3] Verify production containment and file-size cap. Confirm the only production file edited
  is `FilePathHelper.cs`, no other production file changed, and the post-edit line count of
  `FilePathHelper.cs` is recorded against the Phase 0 baseline and the 500-line cap (INV-4); if it was
  a pre-existing overage, confirm the guard did not widen it beyond the minimal lines.
  **Acceptance:** evidence file
  `docs/features/active/2026-06-08-hierarchical-lcppn-folder-prediction-177/evidence/other/p2-production-containment.2026-06-16T10-26.md`
  records the post-edit line count, the cap determination, and that no other production file changed.

- [ ] [P2-T4] Re-verify INV-1 (retain workaround) and INV-2 (containment) by inspection. Confirm
  `LcppnFolderPredictorStore.BuildSettings()` still contains the
  `DoNotSerializeContractResolver("Config")` exclusion unchanged, and that `git status` shows only the
  two in-scope files modified (no edits to `LcppnFolderPredictor*`, spam/triage/category/actionable
  subsystems, `ManagerAsyncLazy`, or cycle-3 enablement/persistence files).
  **Acceptance:** evidence file
  `docs/features/active/2026-06-08-hierarchical-lcppn-folder-prediction-177/evidence/other/p2-invariants-recheck.2026-06-16T10-26.md`
  records the unchanged `Config` exclusion and a `git status` listing showing exactly two changed files.

---

### Phase 3 — Final QA loop and AC verification

- [ ] [P3-T1] Run formatting and record result. Run `dotnet tool run csharpier .` (or `csharpier .`)
  at repo root. If it changes files, restart the toolchain loop from this task.
  **Acceptance:** evidence file
  `docs/features/active/2026-06-08-hierarchical-lcppn-folder-prediction-177/evidence/qa-gates/p3-csharpier.2026-06-16T10-26.md`
  contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` (clean, no files changed in the
  passing pass).

- [ ] [P3-T2] Run analyzers and record result. Run
  `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`.
  If it fails or changes files, fix and restart from P3-T1.
  **Acceptance:** evidence file
  `docs/features/active/2026-06-08-hierarchical-lcppn-folder-prediction-177/evidence/qa-gates/p3-analyzers.2026-06-16T10-26.md`
  contains `Timestamp:`, `Command:`, `EXIT_CODE: 0`, `Output Summary:`.

- [ ] [P3-T3] Run nullable/type-check build and record result. Run
  `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true`.
  If it fails or changes files, fix and restart from P3-T1.
  **Acceptance:** evidence file
  `docs/features/active/2026-06-08-hierarchical-lcppn-folder-prediction-177/evidence/qa-gates/p3-nullable.2026-06-16T10-26.md`
  contains `Timestamp:`, `Command:`, `EXIT_CODE: 0`, `Output Summary:`.

- [ ] [P3-T4] Run tests with coverage and record result. Run
  `vstest.console.exe <UtilitiesCS.Test assembly path> /EnableCodeCoverage`. Confirm all
  `FilePathHelper_Tests` (existing + the two new tests) pass and capture the numeric post-change
  coverage headline. If it fails or changes files, fix and restart from P3-T1.
  **Acceptance:** evidence file
  `docs/features/active/2026-06-08-hierarchical-lcppn-folder-prediction-177/evidence/qa-gates/p3-tests-coverage.2026-06-16T10-26.md`
  contains `Timestamp:`, `Command:`, `EXIT_CODE: 0`, and `Output Summary:` with numeric post-change
  coverage percent and the `FilePathHelper_Tests` pass count.

- [ ] [P3-T5] Verify changed-line coverage policy. Compare the Phase 0 baseline coverage (P0-T7) with
  the post-change coverage (P3-T4); confirm repository-wide line coverage remains >= 80%, the changed
  production lines (the new guard) are covered by the new tests, and coverage on changed lines did not
  regress.
  **Acceptance:** evidence file
  `docs/features/active/2026-06-08-hierarchical-lcppn-folder-prediction-177/evidence/qa-gates/p3-coverage-delta.2026-06-16T10-26.md`
  reports baseline coverage, post-change coverage, and changed/new-code coverage, with an explicit
  pass/fail against the >= 80% floor and the no-regression rule.

- [ ] [P3-T6] Verify AC25 satisfied. Confirm the deserialize round-trip no longer throws (P2-T2 / P3-T4
  green), the guard is contract-preserving with no serialized-shape or public-API change (P2-T1,
  P2-T3), and the regression test demonstrated red-before-green (P1-T1 + P2-T2).
  **Acceptance:** evidence file
  `docs/features/active/2026-06-08-hierarchical-lcppn-folder-prediction-177/evidence/qa-gates/p3-ac25-verification.2026-06-16T10-26.md`
  maps each AC25 clause to its supporting evidence artifact and marks AC25 satisfied.

- [ ] [P3-T7] Verify AC23 retained and AC1-AC24 not regressed. Confirm the
  `DoNotSerializeContractResolver("Config")` exclusion is still present and AC23 tests pass in the
  P3-T4 run, and that the full test run shows no regressions across AC1-AC24-related tests
  (containment from P2-T4 supports zero behavioral impact outside the two in-scope files).
  **Acceptance:** evidence file
  `docs/features/active/2026-06-08-hierarchical-lcppn-folder-prediction-177/evidence/qa-gates/p3-ac1-24-noregress.2026-06-16T10-26.md`
  records the retained `Config` exclusion, AC23 test pass status, and a no-regression statement for
  AC1-AC24 backed by the P3-T4 full test results.

- [ ] [P3-T8] Record final toolchain-green and clean-worktree summary. Confirm the full C# toolchain
  (csharpier -> analyzers -> nullable/TWAE -> vstest+coverage) passed in a single final pass with no
  step changing files, and that only the two in-scope files were modified by this cycle.
  **Acceptance:** evidence file
  `docs/features/active/2026-06-08-hierarchical-lcppn-folder-prediction-177/evidence/qa-gates/p3-final-summary.2026-06-16T10-26.md`
  records the single-pass green result for all four steps and the two-file change set, completing the
  cycle-4 exit condition (`blocking_count == 0` inputs: AC25 satisfied, AC1-AC24 retained, containment
  held, toolchain green).
