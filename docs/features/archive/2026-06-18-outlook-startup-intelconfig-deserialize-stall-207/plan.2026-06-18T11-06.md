# outlook-startup-intelconfig-deserialize-stall (Plan)

- **Issue:** #207
- **Parent (optional):** none
- **Owner:** drmoisan
- **Last Updated:** 2026-06-18T11-06
- **Status:** Draft
- **Version:** 0.2
- **Work Mode:** minor-audit (small path)
- **Requirements source:** `docs/features/active/2026-06-18-outlook-startup-intelconfig-deserialize-stall-207/issue.md` → `## Acceptance Criteria` (AC1–AC6, sole minor-audit AC source)

**Scope lock:** Exactly two files may change — production `UtilitiesCS/EmailIntelligence/IntelligenceConfig.cs` and test `UtilitiesCS.Test/EmailIntelligence/IntelligenceConfig_Tests.cs`. Do NOT modify `ApplicationGlobals.cs`, `AppEvents.cs`, `SmartSerializableLoader`, or any other file. If the instrumentation cannot be done within these two files (for example, the 500-line file cap is exceeded), STOP and report rather than widening scope. A small cohesive private helper inside `IntelligenceConfig.cs` (or a new sibling helper file only if the cap forces it) is the only permitted structural variation, and any new file must be reported before creation.

**Deliverable intent:** Diagnostic instrumentation only. This plan adds per-resource timing inside `IntelligenceConfig.ReadConfigurationAsync` plus its unit test. It does NOT change deserialization behavior and does NOT implement the corrective threading/loading fix (deferred to a follow-up issue after the per-resource breakdown is captured).

**Fail-closed evidence rule:** Phase 0 baseline artifacts and Phase 2 final-QC artifacts are mandatory. If any required baseline artifact, final-QC artifact, or coverage-comparison artifact is missing or has incomplete fields, the audit verdict must be BLOCKED or INCOMPLETE, never PASS.

**Evidence accounting rule:** Each evidence-producing task names its exact canonical artifact path. Do not mark an evidence-backed task complete without the artifact on disk containing `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:`.

**Canonical evidence root:** `docs/features/active/2026-06-18-outlook-startup-intelconfig-deserialize-stall-207/evidence/`. All evidence MUST be written under `evidence/baseline/`, `evidence/qa-gates/`, or `evidence/regression-testing/`. Writing to `artifacts/baselines/`, `artifacts/qa/`, `artifacts/coverage/`, or any other non-canonical location is a policy violation.

**C# toolchain order (every loop):** (1) CSharpier → (2) .NET analyzers (`EnableNETAnalyzers`/`EnforceCodeStyleInBuild`) → (3) nullable/`TreatWarningsAsErrors` → (4) MSTest with coverage. Restart from step 1 if any step fails or changes files.

---

### Phase 0 — Baseline Capture

- [x] [P0-T1] Read policy files in required order (`CLAUDE.md`, `.claude/rules/general-code-change.md`, `.claude/rules/general-unit-test.md`, `.claude/rules/csharp.md`) and record the read in `evidence/baseline/phase0-instructions-read.md` with fields `Timestamp:`, `Policy Order:`, and the explicit list of files read.
- [x] [P0-T2] Record branch and commit baseline (current branch name and HEAD short SHA) in `evidence/baseline/branch-commit-2026-06-18T11-06.md` with fields `Timestamp:`, `Command:` (`git rev-parse --abbrev-ref HEAD` and `git rev-parse --short HEAD`), `EXIT_CODE:`, and `Output Summary:` naming branch + SHA.
- [x] [P0-T3] Run CSharpier in check mode (`dotnet tool run csharpier --check .`) and write `evidence/baseline/csharpier-2026-06-18T11-06.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` (formatted/unformatted file count).
- [x] [P0-T4] Run the analyzer build (`msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`) and write `evidence/baseline/analyzers-2026-06-18T11-06.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` (build pass/fail, warning/error counts).
- [x] [P0-T5] Run the nullable/type-check build (`msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true`) and write `evidence/baseline/nullable-2026-06-18T11-06.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` (build pass/fail, nullable warning count).
- [x] [P0-T6] Run the UtilitiesCS.Test MSTest suite with coverage (`vstest.console.exe <UtilitiesCS.Test assembly path> /EnableCodeCoverage`) and write `evidence/baseline/mstest-coverage-2026-06-18T11-06.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:` recording numeric values: passed/failed test counts, repository-wide line coverage percent, and `IntelligenceConfig.cs` (targeted module) coverage percent.

### Phase 1 — Constrained Small-Path Implementation

- [x] [P1-T1] In `UtilitiesCS/EmailIntelligence/IntelligenceConfig.cs`, add a private nested readonly record/struct (for example `ResourceTimingRow` with `ResourceKey`, `SizeBytes`, `ElapsedMs`) to hold one per-resource measurement. Single binary outcome: the type compiles and is referenced only within `IntelligenceConfig`. (Supports AC1.)
- [x] [P1-T2] In `ReadConfigurationAsync`, wrap each per-entry `DeserializeLoaderAsync(kvp.Value)` call with a `System.Diagnostics.Stopwatch` (`Stopwatch.StartNew()` before the await, `sw.Stop()` after) and capture `kvp.Key`, the serialized payload size (`kvp.Value?.Length ?? 0` characters or UTF-8 byte length — choose byte length via `System.Text.Encoding.UTF8.GetByteCount`), and `sw.Elapsed.TotalMilliseconds` into a `ResourceTimingRow` added to a local list. Single binary outcome: every enumerated entry contributes exactly one timing row. No `DateTime.Now`/`DateTime.UtcNow`/`Random.Shared`/`Thread.Sleep`/`Task.Delay` is introduced. (Supports AC1, AC5.)
- [x] [P1-T3] Add a private helper method `FormatResourceTimingBreakdown(IReadOnlyList<ResourceTimingRow> rows)` that builds a jagged `string[][]` (columns: `Duration` ms, `SizeBytes`, `ResourceKey`) and renders it with `UtilitiesCS.PrettyPrinters.ToFormattedText(string[][], headers, justifications)`, mirroring `StartupTimingRecorder.FormatTable`. Single binary outcome: the helper returns a formatted table string for a given row list and is pure (no logging, no I/O). (Supports AC2.)
- [x] [P1-T4] After the per-resource loop completes in `ReadConfigurationAsync`, emit the breakdown exactly once via the existing `logger` as a single consolidated block (for example `logger.Info($"[IntelConfig timing]\n{FormatResourceTimingBreakdown(rows)}")`). Single binary outcome: one emission call after the loop; no per-iteration logging of the table. (Supports AC2.)
- [x] [P1-T5] Verify and preserve behavior: confirm the returned `ConcurrentDictionary<string, SmartSerializableLoader>` is built from the same enumeration, the same null-loader filtering, the same converter-attachment branches, and the same `PropertyChanged` subscription as before instrumentation; timing capture must not alter key set, values, or ordering semantics. Single binary outcome: the only additions are stopwatch measurement, row accumulation, and one post-loop log emission; deserialization control flow is otherwise identical. (Supports AC3.)
- [x] [P1-T6] Confirm `UtilitiesCS/EmailIntelligence/IntelligenceConfig.cs` remains under the 500-line file cap after edits. If the edit would exceed 500 lines, STOP and report; only then extract the timing record + formatter into a small cohesive sibling helper file under `UtilitiesCS/EmailIntelligence/` and report the new file before creating it. Single binary outcome: file is ≤ 500 lines OR a reported, approved helper extraction has occurred. (Supports file-size constraint.)
- [x] [P1-T7] In `UtilitiesCS.Test/EmailIntelligence/IntelligenceConfig_Tests.cs`, add a deterministic MSTest method (MSTest attributes, Moq for `IApplicationGlobals`, FluentAssertions) that uses the existing `TestableIntelligenceConfig` seam (overriding `GetSerializedConfigurations` and `DeserializeLoaderAsync`) to feed a known fixture set of serialized entries, invokes `ReadConfigurationAsync`/`InitAsync`, and asserts the per-resource breakdown is produced for each fixture entry (assert against the captured table text or an exposed `protected internal`/test-visible breakdown surface). No live COM, no network, no filesystem, no temporary files. Single binary outcome: the test exists and asserts a breakdown row per fixture entry. (Supports AC4.)
- [x] [P1-T8] Add or extend a deterministic MSTest assertion pinning behavior preservation (AC3): given a known fixture set, the returned `Config` key set and ordering semantics match the pre-instrumentation expectation (for example keys equal the non-null fixture keys in enumeration order). Single binary outcome: one test asserts the `Config` dictionary contents are unchanged by instrumentation. (Supports AC3, AC4.)

### Phase 2 — Final QC Loop

- [x] [P2-T1] Run CSharpier format (`dotnet tool run csharpier .`) and write `evidence/qa-gates/csharpier-2026-06-18T11-06.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` (files formatted). If any file changed, restart the loop from this task.
- [x] [P2-T2] Run the analyzer build (`msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`) and write `evidence/qa-gates/analyzers-2026-06-18T11-06.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` (build pass/fail, warning/error counts; confirm no new warning-severity diagnostics and no banned-API RS0030 violation).
- [x] [P2-T3] Run the nullable/type-check build (`msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true`) and write `evidence/qa-gates/nullable-2026-06-18T11-06.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` (build pass/fail, nullable warnings).
- [x] [P2-T4] Run the UtilitiesCS.Test MSTest suite with coverage (`vstest.console.exe <UtilitiesCS.Test assembly path> /EnableCodeCoverage`) and write `evidence/qa-gates/mstest-coverage-2026-06-18T11-06.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:` recording numeric post-change values: passed/failed counts, repository-wide line coverage percent, and `IntelligenceConfig.cs` coverage percent.
- [x] [P2-T5] Write a coverage delta/threshold verification artifact `evidence/qa-gates/coverage-delta-2026-06-18T11-06.md` comparing baseline (P0-T6) vs post-change (P2-T4): record `baseline repo-wide %`, `post-change repo-wide %`, `IntelligenceConfig.cs new/changed-line coverage %`, and an explicit PASS/FAIL against the policy thresholds (repo-wide ≥ 80%, no repo-wide regression, new/changed lines ≥ 90%). If required coverage numbers are unavailable, mark outcome remediation-required, not PASS.
- [x] [P2-T6] Verify each acceptance criterion explicitly and record the result in `evidence/qa-gates/ac-verification-2026-06-18T11-06.md`: AC1 (per-resource key + payload size + Stopwatch-measured elapsed recorded for each entry), AC2 (single consolidated log4net block in `[Startup timing]`-consistent style), AC3 (returned `Config` contents/ordering unchanged — cite the pinning test), AC4 (deterministic MSTest with Moq + FluentAssertions over the `protected internal virtual` seams, no COM/network/filesystem/temp files), AC5 (no banned API; `Stopwatch` used — cite analyzer artifact), AC6 (full toolchain green in order; coverage policy met — cite P2-T1..T5). Each AC must be marked PASS with a cited artifact path or the verdict is BLOCKED.

---

## Acceptance Criteria Traceability

| AC | Description | Verifying tasks |
|---|---|---|
| AC1 | Per-resource key, payload size, Stopwatch elapsed recorded for each entry | P1-T1, P1-T2, P2-T6 |
| AC2 | Single consolidated log4net breakdown block, `[Startup timing]`-style | P1-T3, P1-T4, P2-T6 |
| AC3 | Behavior-preserving: `Config` contents/ordering unchanged | P1-T5, P1-T8, P2-T6 |
| AC4 | Deterministic MSTest (Moq + FluentAssertions) over existing seams, no COM/network/FS/temp | P1-T7, P2-T4, P2-T6 |
| AC5 | No banned API; `Stopwatch` only | P1-T2, P2-T2, P2-T6 |
| AC6 | Full toolchain green in order; coverage policy met, no regression | P2-T1, P2-T2, P2-T3, P2-T4, P2-T5, P2-T6 |
