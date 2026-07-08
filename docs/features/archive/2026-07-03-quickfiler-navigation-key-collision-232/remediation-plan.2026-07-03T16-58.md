# Remediation Plan — Issue #232 (QuickFiler Navigation-Key Collision Fix)

**Plan timestamp:** 2026-07-03T16-58
**Cycle:** 1
**Base branch:** `main` (merge-base `00507b595297c3e6970634a1855f1144c987dbdf`)
**Head under remediation:** `90e75ec1`
**Canonical issue number:** 232 (all artifact content, file paths, and cross-references use this number)
**Feature folder:** `docs/features/active/2026-07-03-quickfiler-navigation-key-collision-232`

## Inputs

- Primary: `docs/features/active/2026-07-03-quickfiler-navigation-key-collision-232/remediation-inputs.2026-07-03T16-58.md`
- Supporting: `docs/features/active/2026-07-03-quickfiler-navigation-key-collision-232/remediation-inputs.2026-07-03T16-51.md`
- Audits (2026-07-03T16-51): `policy-audit.2026-07-03T16-51.md`, `code-review.2026-07-03T16-51.md`, `feature-audit.2026-07-03T16-51.md`

## Scope (exactly this, no more)

1. BLOCKING — Regenerate, persist, and re-verify a machine-readable C# Cobertura coverage artifact (AC10 PARTIAL -> PASS). Evidence-verifiability gap only; no change to the Part A crash fix (`QfcCollectionController.cs`) or Part B logic is required by this finding.
2. MINOR (bundled) — Correct the `logger.Debug(...)` caller-context string in `QuickFiler/Controllers/QfcDatamodel.cs` so it names the emitting method `ScoreRemainingQueueMailItemAsync` (retaining the `(master-queue admission)` descriptor). One-line string change, no control-flow effect.

## Out of scope

- No change to the Part A fix logic or the register/unregister/guard behavior.
- No change to high-confidence filtering / dequeue behavior (tracked separately as feature #233).
- The pre-existing `QfcCollectionController.cs` >500-line overage and the `QfcCollectionControllerTests.cs` at-cap condition are not remediation items for this cycle.

## Ordering rationale

The minor source correction (Phase 1) is applied first so the authoritative coverage run in the Final QA Loop (Phase 2) reflects the final source state. Persistence and verification (Phase 3) consume the artifact produced by that final toolchain run.

## Evidence-location invariant

All evidence resolves under `docs/features/active/2026-07-03-quickfiler-navigation-key-collision-232/evidence/<kind>/`. The only permitted non-feature evidence path is the single canonical `artifacts/csharp/coverage.xml` copy. Any non-canonical evidence path supplied downstream is rejected and replaced with the canonical feature `evidence/` path.

---

### Phase 0 — Baseline and Policy Capture

- [x] [P0-T1] Read policy files in the required order (`CLAUDE.md`, `.claude/rules/general-code-change.md`, `.claude/rules/general-unit-test.md`, `.claude/rules/csharp.md`) and record the read in `docs/features/active/2026-07-03-quickfiler-navigation-key-collision-232/evidence/remediation-baseline/phase0-instructions-read.2026-07-03T16-58.md`. AC: artifact exists and contains `Timestamp:`, `Policy Order:`, and the explicit list of files read.
- [x] [P0-T2] Capture the CSharpier formatting baseline by running `dotnet tool run csharpier --check .` and record the result in `docs/features/active/2026-07-03-quickfiler-navigation-key-collision-232/evidence/remediation-baseline/csharpier-baseline.2026-07-03T16-58.md`. AC: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, and an `Output Summary:` stating whether any file would be reformatted.
- [x] [P0-T3] Capture the analyzer build baseline by running `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` and record the result in `docs/features/active/2026-07-03-quickfiler-navigation-key-collision-232/evidence/remediation-baseline/msbuild-analyzers-baseline.2026-07-03T16-58.md`. AC: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, and an `Output Summary:` with warning/error counts.
- [x] [P0-T4] Capture the nullable/TreatWarningsAsErrors build baseline by running `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true` and record the result in `docs/features/active/2026-07-03-quickfiler-navigation-key-collision-232/evidence/remediation-baseline/msbuild-nullable-baseline.2026-07-03T16-58.md`. AC: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, and an `Output Summary:` with warning/error counts.
- [x] [P0-T5] Capture the coverage baseline by running vstest over `UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll QuickFiler.Test\bin\Debug\QuickFiler.Test.dll TaskMaster.Test\bin\Debug\TaskMaster.Test.dll` with `/InIsolation` and the ratified Cobertura runsettings (first-party + Swordfish module set, `[ExcludeFromCodeCoverage]`/`GeneratedCode` attribute excludes) referenced in `evidence/qa-gates/vstest-final.md`, and record the result in `docs/features/active/2026-07-03-quickfiler-navigation-key-collision-232/evidence/remediation-baseline/vstest-coverage-baseline.2026-07-03T16-58.md`. AC: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, and an `Output Summary:` recording numeric repo-wide line-rate (expected ~76.57%) and `QfcHighConfidencePreFilter.cs` mapped-class line-rate.

### Phase 1 — Logging Caller-Context String Correction

- [x] [P1-T1] In `QuickFiler/Controllers/QfcDatamodel.cs`, edit the `logger.Debug(...)` call inside `ScoreRemainingQueueMailItemAsync` (currently at line 326) so the caller-context string reads `Probability debug [QfcDatamodel.ScoreRemainingQueueMailItemAsync (master-queue admission)] ` instead of naming `LoadRemainingEmailsToQueueAsync`, retaining the `(master-queue admission)` descriptor and changing no control flow. AC: `QfcDatamodel.cs` contains the string `[QfcDatamodel.ScoreRemainingQueueMailItemAsync (master-queue admission)]` and no longer contains `[QfcDatamodel.LoadRemainingEmailsToQueueAsync (master-queue admission)]`.
- [x] [P1-T2] Record the before/after string and enclosing-method confirmation in `docs/features/active/2026-07-03-quickfiler-navigation-key-collision-232/evidence/other/qfcdatamodel-string-correction.2026-07-03T16-58.md`. AC: artifact contains `Timestamp:`, the exact prior string, the exact corrected string, and confirmation that the emitting method is `ScoreRemainingQueueMailItemAsync`.

### Phase 2 — Final QA Loop and Coverage Regeneration

Run the full C# toolchain in this exact order. If any step fails or changes files, fix and restart the loop from P2-T1. The vstest step produces the authoritative Cobertura `coverage.xml` reflecting the Phase 1-corrected source.

- [x] [P2-T1] Run `dotnet tool run csharpier .` and record the result in `docs/features/active/2026-07-03-quickfiler-navigation-key-collision-232/evidence/qa-gates/csharpier-final.2026-07-03T16-58.md`. AC: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE: 0`, and an `Output Summary:` confirming no files remained unformatted on the final pass; if any file changed, the loop restarts from P2-T1.
- [x] [P2-T2] Run `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` and record the result in `docs/features/active/2026-07-03-quickfiler-navigation-key-collision-232/evidence/qa-gates/msbuild-analyzers-final.2026-07-03T16-58.md`. AC: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE: 0`, and an `Output Summary:` with zero analyzer errors; on failure fix and restart from P2-T1.
- [x] [P2-T3] Run `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true` and record the result in `docs/features/active/2026-07-03-quickfiler-navigation-key-collision-232/evidence/qa-gates/msbuild-nullable-final.2026-07-03T16-58.md`. AC: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE: 0`, and an `Output Summary:` with zero nullable/type warnings-as-errors; on failure fix and restart from P2-T1.
- [x] [P2-T4] Run vstest over `UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll QuickFiler.Test\bin\Debug\QuickFiler.Test.dll TaskMaster.Test\bin\Debug\TaskMaster.Test.dll` with `/InIsolation` and the ratified Cobertura runsettings (identical first-party + Swordfish module set and `[ExcludeFromCodeCoverage]`/`GeneratedCode` attribute excludes as referenced in `evidence/qa-gates/vstest-final.md`), directing the Cobertura `coverage.xml` output to a run-results directory, and record the result in `docs/features/active/2026-07-03-quickfiler-navigation-key-collision-232/evidence/qa-gates/vstest-final.2026-07-03T16-58.md`. AC: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, and an `Output Summary:` with total/passed/failed test counts, numeric repo-wide line-rate, `QfcHighConfidencePreFilter.cs` mapped-class line-rates, and the absolute path of the generated `coverage.xml`; failure count does not exceed the Phase 0 baseline failure count. If this step changed any file the loop restarts from P2-T1.

### Phase 3 — Coverage Artifact Persistence, Verification, and Evidence Update

- [x] [P3-T1] Copy the authoritative Cobertura `coverage.xml` produced by P2-T4 to the canonical path `artifacts/csharp/coverage.xml`. AC: `artifacts/csharp/coverage.xml` exists and is byte-identical to the P2-T4 run output.
- [x] [P3-T2] Copy the same authoritative `coverage.xml` to the committable feature evidence path `docs/features/active/2026-07-03-quickfiler-navigation-key-collision-232/evidence/coverage/2026-07-03T16-58/coverage.xml`. AC: the feature evidence `coverage.xml` exists and is byte-identical to `artifacts/csharp/coverage.xml`.
- [x] [P3-T3] Confirm the persisted `coverage.xml` reflects the final source state by verifying it was generated from the P2-T4 run that executed after the Phase 1 correction, and record the confirmation in `docs/features/active/2026-07-03-quickfiler-navigation-key-collision-232/evidence/qa-gates/coverage-artifact-provenance.2026-07-03T16-58.md`. AC: artifact contains `Timestamp:`, the source run identifier/results path, the head commit under remediation, and a statement that no source change occurred between P2-T4 and persistence.
- [x] [P3-T4] From the persisted `docs/features/active/2026-07-03-quickfiler-navigation-key-collision-232/evidence/coverage/2026-07-03T16-58/coverage.xml`, verify every `<class>` element whose filename maps to `QfcHighConfidencePreFilter.cs` reports `line-rate="1"` (changed-line coverage >= 90% target), and record the enumerated class list and line-rates in `docs/features/active/2026-07-03-quickfiler-navigation-key-collision-232/evidence/qa-gates/coverage-verification.2026-07-03T16-58.md`. AC: artifact lists each mapped `<class>` name with `line-rate="1"` and states the changed-line-coverage determination PASS.
- [x] [P3-T5] From the same persisted `coverage.xml`, verify the repo-wide line-rate shows no regression versus the recorded ~76.57% baseline (exemption-governed testable denominator), and append the repo-wide line-rate, lines-covered, lines-valid, and delta to `docs/features/active/2026-07-03-quickfiler-navigation-key-collision-232/evidence/qa-gates/coverage-verification.2026-07-03T16-58.md`. AC: artifact records the artifact-derived repo-wide figures and a no-regression determination PASS.
- [x] [P3-T6] Update `docs/features/active/2026-07-03-quickfiler-navigation-key-collision-232/evidence/qa-gates/coverage-delta.md` to cite the persisted XML path (`evidence/coverage/2026-07-03T16-58/coverage.xml` and canonical `artifacts/csharp/coverage.xml`) and the artifact-derived figures from P3-T4 and P3-T5. AC: `coverage-delta.md` references the persisted XML paths, contains the artifact-derived repo-wide and `QfcHighConfidencePreFilter.cs` figures, and records AC10 as PASS.
