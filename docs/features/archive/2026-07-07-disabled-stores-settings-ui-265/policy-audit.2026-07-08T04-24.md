# Policy Audit — F5 disabled-stores-settings-ui (Issue #265)

- Feature: `docs/features/active/2026-07-07-disabled-stores-settings-ui-265/`
- Branch: `feature/disabled-stores-settings-ui-265` @ HEAD `abe278ec`
- Diff range (epic child -> integration): `872eafb4..HEAD` (epic #260 integration tip that merged F1 #261, F2 #262, F3 #263)
- Work Mode: `full-feature` (AC sources: `spec.md` AC1-AC10 and `user-story.md`)
- Reviewer: feature-review agent
- Timestamp: 2026-07-08T04-24

## Executive Summary

F5 adds a testable `DisabledStoresController` + `IDisabledStoresViewer` seam, a pure `DisabledStoreRow`
view-model, a shared `StoreLaunchReadinessEvaluator`, a WinForms `DisabledStoresViewer`/Designer, and an
additive Settings-menu ribbon button. The change is a pure consumer of F1's `IStoreDisableService`
(`GetDisabledStores()` / `ReenableAsync(StoreIdentity)`). The full local C# toolchain passes in order
(CSharpier, .NET analyzers, nullable/`TreatWarningsAsErrors`, MSTest with coverage), 4230/4230 tests pass,
and new testable-code line coverage clears the 90% new-code target. No Blocking findings were identified.
Overall verdict: PASS.

## Scope Confirmation (Legitimate Base)

The audit scope is the full branch diff against the resolved base for this epic child branch. The base
`872eafb4` is the epic #260 integration tip from which this branch was cut; it is the authoritative base
per the epic-mode delivery model, not an illegitimate subset narrowing. This branch is an epic
child -> integration branch, so its child PR receives zero CI checks by design; the CI-green requirement is
vacuously satisfied and local 4-step toolchain evidence is the authority. No workflow files
(`.github/workflows/**`) are changed in this diff, so `modified-workflow-needs-green-run` does not apply.

### Rejected Scope Narrowing

None. No caller instruction attempted to narrow the audit to a plan/task/phase subset, to mark any
language as out of scope, or to skip a required check for a language with changed files. The epic-mode
base-branch and zero-CI framing are legitimate scope definitions and were applied as such.

## Changed-File Inventory by Language

| Language | Changed files (branch diff) | Coverage-gate applies |
|---|---|---|
| C# (`.cs`, `.csproj`) | 5 new production `.cs`, 1 modified production `.cs`, 3 modified ribbon `.cs`, 2 modified `.csproj`, 1 new test `.cs` | Yes |
| XML (`.xml`) | `RibbonExplorer.xml` (additive markup), `DisabledStoresViewer.resx` (new) | No executable-coverage language |
| Markdown (`.md`) | spec, user-story, plan, evidence artifacts | Exempt (docs) |
| TypeScript / Python / PowerShell | 0 changed files | Not applicable — zero changed files on branch |

## 1. Coverage Compliance

Coverage thresholds applied are the CLAUDE.md-authoritative C# thresholds for this legacy VSTO/WinForms
area: repository line coverage >= 80% on the testable denominator, and >= 90% for new code, with no
regression on changed lines (CLAUDE.md General Unit Test Policy UT2). The feature-review coverage hook
hard-codes an 85% line floor and reads `artifacts/csharp/coverage.xml`; the canonical per-feature C#
coverage artifact for this branch is Cobertura at `coverage/utilitiescs-postchange.cobertura.xml`. Per the
caller directive and CLAUDE.md precedence, the CLAUDE.md thresholds are authoritative here. The observed
testable-denominator figure (88.01%) clears both the CLAUDE.md 80% floor and the hook's 85% floor, so the
distinction is not outcome-determinative.

### 1.1 C# coverage verdict (verified from `coverage/utilitiescs-postchange.cobertura.xml`)

| Row | Baseline | Post-change | Change | Disposition | Evidence | New/changed-code coverage | Verdict |
|---|---|---|---|---|---|---|---|
| C# repository line coverage (first-party UtilitiesCS testable denominator) | 88.21% | 88.01% | -0.20 pp | Non-regression: the -0.20 pp is entirely 96 new WinForms-exempt lines added to the package denominator; no baseline-covered line became uncovered | qa-05-coverage-delta.md; qa-04-test-coverage.md | new-code line coverage 91.67% (controller) / 100% (evaluator) | PASS |
| C# new file `DisabledStoresController.cs` line coverage | new file | 91.67% (main class 97.67% + async state-machine 82.76%, file-aggregate 66/72 lines) | new | Above the 90% new-code target | cobertura class nodes (verified) | 91.67% | PASS |
| C# new file `StoreLaunchReadinessEvaluator.cs` line coverage | new file | 100% (13/13 lines) | new | Above the 90% new-code target | cobertura class node (verified) | 100% | PASS |
| C# new file `DisabledStoreRow.cs` line coverage | new file | pure auto-property POCO, no coverable sequence points; every property exercised/asserted by PopulateRows_ProjectsServiceEntriesIntoRows | new | Vacuously meets the 90% new-code target (zero uncovered lines) | qa-04-test-coverage.md | 100% effective (0 uncovered) | PASS |
| C# modified file `StoreWrapperController.cs` changed-line coverage | covered | one-line delegation body remains covered by 51/51 StoreWrapper regression suite | no regression | No regression on the single changed line | readiness-extraction-behavior-preserving.md | changed line covered | PASS |

Repo-wide C# line coverage is 88.01% on the testable denominator, above the 80% CLAUDE.md floor and the
85% hook floor. Branch coverage on the `ReenableAsync` async state-machine fragment is 50% (see the
non-blocking observation in §6); the fragment's uncovered branch is the `Viewer.InvokeRequired == true`
UI-thread marshaling path, which is WinForms-thread plumbing, and line coverage is the CLAUDE.md-
authoritative gate for this area. Overall C# coverage verdict: PASS.

Raw-artifact caveat: the raw Cobertura overall for the collection run is 72.34% because it counts
runtime-instrumented vendored/third-party assemblies in the denominator. The first-party testable
denominator (88.01%) is the correct figure per the repository's documented COM/VSTO/WinForms exemption
and vendored-assembly denominator model.

### 1.2 Coverage exemptions applied (COM/VSTO/WinForms, CLAUDE.md UT2)

| File | Kind | Exemption basis | Enforcement |
|---|---|---|---|
| `DisabledStoresViewer.cs` | WinForms form-derived | Form-derived class exemption (b) | 0% observed; excluded from new-code numerator |
| `DisabledStoresViewer.Designer.cs` | Designer-generated | Designer-generated exemption (b) | 0% observed; excluded from new-code numerator |
| `IDisabledStoresViewer.cs` | interface-only | interface-only, no executable lines | legitimately 0% executable |
| `DisabledStoresController.Launch()` | WinForms shell | `[ExcludeFromCodeCoverage]` attribute present | verified in source |
| `RibbonController.DisabledStoresSettings()` / `RibbonViewer.DisabledStoresSettings_Click` | VSTO ribbon dispatch | inherits class-level `[ExcludeFromCodeCoverage]` (verified on both classes) | verified in source |

## 2. Toolchain Compliance (local 4-step, run in order)

| Stage | Command | Result | Evidence | Verdict |
|---|---|---|---|---|
| 1. Format | `csharpier format .` then `csharpier check .` | EXIT 0; 1300 files checked, 0 require formatting (idempotent) | qa-01-format.md | PASS |
| 2. Analyzers | `msbuild TaskMaster.sln ... /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` | EXIT 0; 0 errors; 73 warnings (baseline 75; 0 new diagnostics from F5 files) | qa-02-analyzers.md | PASS |
| 3. Nullable | `msbuild TaskMaster.sln ... /p:Nullable=enable /p:TreatWarningsAsErrors=true` | EXIT 0; 0 errors, 0 warnings on touched files | qa-03-nullable.md | PASS |
| 4. Test + coverage | `vstest.console.exe UtilitiesCS.Test.dll /EnableCodeCoverage` (dotnet-coverage cobertura) | EXIT 0; 4230/4230 pass, 0 fail (baseline 4223 + 7 new) | qa-04-test-coverage.md; controller-tests-pass.md | PASS |

## 3. Design & Structure Policy (General + C#)

| Check | Finding | Verdict |
|---|---|---|
| File size <= 500 lines (all new/modified) | Max is `StoreWrapperController.cs` at 382 (reduced by extraction); all new files 26-291 lines | PASS |
| Every new `.cs` wired into its `.csproj` | 5 production `.cs` + evaluator + resx + Designer wired in `UtilitiesCS.csproj`; test wired in `UtilitiesCS.Test.csproj` (verified in diff) | PASS |
| Separation of concerns: decision logic separate from WinForms/I/O | All logic in controller behind `IDisabledStoresViewer`; the only `Dgv.DataSource` write is in `DisabledStoresViewer.BindRows` (WinForms-exempt) | PASS |
| net48 value-type constraint (no `record struct` / `init`) | `StoreIdentity` and `DisabledStoreEntry` are plain `readonly struct` with ordinary constructors and get-only properties; no new type uses `record struct` or `init` | PASS |
| Prefer `internal` for non-public API | `DisabledStoresController.Viewer` and `IDisabledStoresViewer` are `internal`; public surface is `Launch()` + the two public POCO/event members | PASS |
| Error handling: fail-fast, no silent broad catch | `ReenableAsync` catch logs via log4net and surfaces via `MyBox` (the established controller error boundary), then refreshes; does not swallow silently | PASS |
| Logging pattern | Uses log4net `logger.Error(...)` per repository pattern | PASS |
| XML docs on non-obvious public API | Controller, row, interface, evaluator carry XML docs including the CS0053 internal-Viewer rationale | PASS |

## 4. Scope / Contract Policy (F5-specific invariants)

| Invariant | Finding | Verdict |
|---|---|---|
| F5 calls `IStoreDisableService` only (`GetDisabledStores()`, `ReenableAsync(StoreIdentity)`) | Confirmed; `grep` for `Rehook`/`IStoreRehookService`/`Serialize`/`Persist`/`StoresWrapper.` in the controller returns none | PASS |
| No direct F3 rehook call | None present; reenable routes only through `Globals.StoreDisable.ReenableAsync` | PASS |
| No persistence performed by F5 itself | No serialize/persist calls in the controller | PASS |
| Row-index resolution against controller's own `Rows`, never a live grid | `Dgv_CellContentClick` resolves `Rows[e.RowIndex]` with header/column/range guards; no live-grid read | PASS |
| Behavior-preserving readiness extraction | `EvaluateLaunchReadiness()` becomes a one-line delegation to `StoreLaunchReadinessEvaluator.Evaluate`; 51/51 StoreWrapper tests pass unmodified | PASS |
| Existing single-store editor + Folder/Junk Folder Settings buttons unchanged | Confirmed unchanged (non-interference-confirmation.md; diff) | PASS |

## 5. Test Policy (General + C# Unit Test)

| Check | Finding | Verdict |
|---|---|---|
| MSTest + Moq + FluentAssertions | All three used; `[TestClass]`/`[TestMethod]` | PASS |
| No live Outlook, no live DataGridView, no temp files | Mocked `IStoreDisableService` + `IDisabledStoresViewer`; `DataGridViewCellEventArgs` constructed directly | PASS |
| Determinism (no sleeps/delays/real timers/wall-clock) | Async driven by completed/faulted `Task`; no banned timing APIs | PASS |
| Scenario completeness (positive/empty/negative/edge/error) | Populate, empty, click happy-path, header/non-button, out-of-range, success-refetch, failure-surfaced-and-refetch | PASS |
| Test file location mirrors source; `tests`-style test project | `UtilitiesCS.Test/OutlookObjects/Store/DisabledStoresControllerTests.cs` mirrors production path | PASS |

## 6. Non-Blocking Observations

1. Branch coverage on the `DisabledStoresController.<ReenableAsync>` async state-machine is 50% because the
   `Viewer.InvokeRequired == true` marshaling branch is not exercised (tests set `InvokeRequired = false`).
   Line coverage is the CLAUDE.md-authoritative gate for this legacy WinForms area and passes; the
   untested branch is UI-thread marshaling plumbing. Recommendation (non-blocking): add one test with a
   mocked `InvokeRequired = true` asserting `Viewer.Invoke` is used for the refresh.
2. `DisabledStoreRow` exposes public settable auto-properties. This is required for WinForms `BindingList`
   data binding and is consistent with the view-model role; not a defect.

## 7. Evidence Location Compliance

All feature evidence is written under the canonical `<FEATURE>/evidence/{baseline,regression-testing,qa-gates,other,issue-updates}/`
path. The branch diff contains no files under `artifacts/baselines/`, `artifacts/qa/`, `artifacts/evidence/`,
or `artifacts/coverage/`. No evidence-location violations. (The `coverage/*.cobertura.xml` collection outputs
are gitignored build artifacts and are not part of the committed branch diff.)

## Appendix A — Coverage Artifact Provenance

- C# coverage artifact consumed: `coverage/utilitiescs-postchange.cobertura.xml` (Cobertura; 25.8 MB), baseline `coverage/utilitiescs-baseline.cobertura.xml`.
- Per-class line-rates independently verified from the post-change Cobertura: `DisabledStoresController` main class 0.9767, `<ReenableAsync>` state-machine 0.8276, `StoreLaunchReadinessEvaluator` 1.0, both `DisabledStoresViewer` nodes 0.0 (exempt).
- The feature-review hook path `artifacts/csharp/coverage.xml` (JaCoCo) is not the artifact produced by this branch's per-feature Cobertura collection; the CLAUDE.md thresholds were applied against the Cobertura artifact as directed.

## Verdict

PASS. Zero Blocking findings. Two non-blocking observations recorded.
