# Code Review: hierarchical-lcppn-folder-prediction (#177) — Cycle 3 Exit Reaudit

- Review date: 2026-06-16
- Exit timestamp: 2026-06-16T02-06
- Branch: `TaskMaster-wt-2026-06-08-12-06`
- Scope: full branch diff vs merge-base `c12aaf1c`; cycle-3 delta `0b589c83..HEAD` (`cc769a05`, `c7ef085a`, `f4159154`). C#-only source.

## Executive Summary

The cycle-3 production-migration code is well-structured and adheres to the repository's C# code-change and unit-test policies. The design isolates the host settings dependency in the TaskMaster layer (`AppAutoFileObjects.FolderPredictorLoad.cs`) and passes only a resolved `bool` across the `IAppAutoFileObjects` boundary, so `UtilitiesCS` does not reference `TaskMaster.Properties.Settings` — no layering violation. Persistence is centralized in a single focused static class (`LcppnFolderPredictorStore`) that keeps the serialize and deserialize paths in agreement via one file-name constant and one shared `JsonSerializerSettings` factory. The serialization-correctness fix (excluding the runtime-only `Config`/`Disk` from the document) is implemented cleanly through `DoNotSerializeContractResolver("Config")` and is backed by an explicit round-trip test that asserts the document omits `"Disk"` while content survives.

Error handling is appropriate to the contract: `BuildConfig` fails fast on null/empty AppData, while the startup load path is deliberately fail-soft (catches at the defined startup boundary and re-surfaces through log4net) as required by AC23/AC22. Async wiring is correct (`LoadFolderPredictorAsync` awaited from both load paths). Naming is descriptive, XML docs explain the non-obvious rationale (layering boundary, fail-soft, `Config` exclusion), and all new files are well under the 500-line cap. Tests are deterministic, in-memory, AAA-structured, and use Moq + FluentAssertions with per-test settings save/restore.

No blocking findings. Two low-severity observations are recorded below; neither blocks merge. The full C# toolchain (csharpier, analyzers, nullable/TWAE, MSTest) is green in a single final pass per the recorded evidence.

Overall verdict: **PASS.** Blocking findings: 0.

## Findings Table

| Severity | File | Location | Finding | Recommendation | Rationale | Evidence |
|----------|------|----------|---------|----------------|-----------|----------|
| Low | TaskMaster/AppGlobals/AppAutoFileObjects.FolderPredictorLoad.cs | LoadFolderPredictorAsync catch block (lines 89-99) | Broad `catch (Exception e)` at the startup load boundary | Keep as-is; optionally narrow to the expected IO/serialization exception types if the surface is known | Broad catch is justified here: AC23 requires fail-soft startup so a corrupt/unreadable predictor file never breaks the add-in. It is at a defined boundary and re-surfaces via `_folderPredictorLogger.Error(...)` rather than swallowing, consistent with the General Code Change Policy boundary-catch carve-out. Not a defect. | AppAutoFileObjects.FolderPredictorLoad.cs:89-99; AC23 contract in remediation-inputs.2026-06-16T01-04.md |
| Low | UtilitiesCS/EmailIntelligence/ClassifierGroups/OlFolder/OlFolderClassifierGroup.cs | Serialize block inside BuildClassifiersAsync (lines 303-309) | The new `predictor.Serialize()` call sits inside the Outlook-COM-bound `BuildClassifiersAsync`, so it is not exercised by a unit test (no live Outlook seam) | Accept under the CLAUDE.md COM/VSTO testable-denominator exemption; the serialize settings and round-trip behavior are independently covered by `LcppnFolderPredictorStore_Tests` | The uncoverable lines are limited to the COM-bound build orchestration; the persistence correctness (config/round-trip) is fully covered at 100% via the store tests, so the risk is the build-path glue only. | coverage-delta.2026-06-16T01-04.md (OlFolderClassifierGroup 65.38% -> 72.73%); LcppnFolderPredictorStore_Tests.cs round-trip |
| Info | TaskMaster/AppGlobals/AppAutoFileObjects.cs | Class declaration + LoadParallel/LoadSequential (line 31, +2 wiring) | `AppAutoFileObjects.cs` is 849 lines (pre-existing over-cap, was 847) | No action this cycle; the load body correctly lives in the new partial file so the over-cap file grew only by the `partial` keyword and two await wiring lines | Splitting the pre-existing over-cap file is out of scope (separate refactor); the cycle minimized its growth by placing all new logic in `AppAutoFileObjects.FolderPredictorLoad.cs` (102 lines). | final-filesize.2026-06-16T01-04.md; awk NR line counts |

## Design and Policy Assessment

- Separation of concerns: PASS. Host settings access confined to TaskMaster; pure store/config logic in UtilitiesCS; load orchestration in a dedicated partial.
- Simplicity: PASS. Single bool boundary, one lazy config resolution, one store constant. No new abstraction framework or new NuGet dependency.
- Reusability: PASS. `BuildConfig`/`BuildSettings` shared by serialize and deserialize; file name centralized.
- Extensibility/testability seams: PASS. `FolderPredictorConfig` setter and `internal FolderPredictorDeserializer` delegate provide injectable seams; explicit config overrides the persisted default.
- Null-safety: PASS. Nullable gate green; guard clauses (`ThrowIfNullOrEmpty`, `is not null`, `TryGetValue`).
- Logging: PASS. log4net at appropriate levels (Warn for absent file/unresolved AppData; Error for genuine read/parse failure).
- File-size cap: PASS for all new/changed files except the pre-existing over-cap `AppAutoFileObjects.cs` (Info above); over-cap callers `FolderScorer.cs` (608) and `SortEmail.cs` (1406) are unchanged this cycle.
- Containment: PASS. Zero diff in spam/triage/category/multiclass; `ManagerAsyncLazy` typing unchanged; flat rebuild retained.

## Test Quality Assessment

- Framework/libraries: MSTest + Moq + FluentAssertions throughout; no xUnit/NUnit introduced. PASS.
- Determinism/isolation: in-memory seams, no Outlook COM, no network, no temporary files; `Settings.Default.UseLcppnPredictor` saved/restored per test. PASS.
- Structure/diagnostics: explicit AAA blocks; FluentAssertions with rationale strings. PASS.
- Scenario completeness: positive (selection, load success, round-trip), negative (null/empty AppData throws, OFF short-circuit), edge (ON-but-no-holder, missing file, unresolved AppData), error (IOException caught). PASS.

## Toolchain Verification (from recorded evidence)

| Step | Command | Result | Status |
|------|---------|--------|--------|
| Format | `csharpier check .` | Clean, 1080 files, exit 0 | PASS |
| Lint | `msbuild ... /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` | 0 errors, exit 0 | PASS |
| Type-check | `msbuild ... /p:Nullable=enable /p:TreatWarningsAsErrors=true` | 0 warnings/errors, exit 0 | PASS |
| Test | `vstest.console.exe ... /InIsolation /Settings:TaskMaster.runsettings` | 4019 pass / 0 fail, exit 0 | PASS |

## Verdict

**PASS — ready for merge.** No blocking findings. Two low-severity observations (justified broad catch at the startup boundary; COM-bound serialize-glue uncoverable under the documented exemption) and one informational note (pre-existing over-cap `AppAutoFileObjects.cs`, minimized growth). Blocking findings contributed by this artifact: 0.
