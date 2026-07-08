# Code Review: coverage-increments-1-3-testable-seams (#199)

**Review Date:** 2026-06-14
**Reviewer:** feature-reviewer agent
**Feature Folder:** `docs/features/active/2026-06-14-coverage-increments-1-3-testable-seams-199`
**Feature Folder Selection Rule:** Suffix `-199` matches the canonical issue number and the branch's primary scoping doc (spec.md).
**Base Branch:** `origin/main` @ `d436a06f`
**Head Branch:** `refactor/coverage-increments-1-3-199` @ `f7287905`
**Review Type:** Initial review

---

## Executive Summary

This branch is a test-only refactor that adds 99 MSTest unit tests (MSTest + Moq + FluentAssertions) across `ToDoModel.Test`, `QuickFiler.Test`, and `TaskMaster.Test`, targeting the genuinely-testable seams that #197 preserved as measured. The reviewer verified via `git diff d436a06f..f7287905` that no production source file changed; the only non-test changes are additive `<Compile Include>` lines in the three test `.csproj` files (a mechanical requirement of these legacy non-SDK projects). The implementation quality is high: every test follows Arrange-Act-Assert with descriptive names and FluentAssertions reason strings, all paths are deterministic (synchronously-completing async delegates, no temp files, no live Outlook/WinForms), and mutable global `Settings.Default` is snapshotted/restored per test.

**What changed:**
- 11 new C# test files (81-246 lines each) covering: `ToDoLoader.SetAndSave<T>` (4 overloads), `IDList.GetNextToDoID`, `BaseChanger` remaining branches, `ProjectEntry` (dialog-free branches), `KaChar`/`KaCharAsync`/`KaKey`/`KaKeyAsync`/`KaStringAsync`, `KbdActions<>` registry, `FilerQueue`, `QfcQueue` pure paths, `AppStagingFilenames`, and remaining `AppQuickFilerSettings` properties.
- 3 additive test-csproj `<Compile Include>` registrations.
- Per-assembly production line-rate increased: ToDoModel 10.82%->24.65%, QuickFiler 25.20%->30.76%, TaskMaster 25.78%->44.13% (reviewer-parsed `final-fullsuite.cobertura.xml`).
- Two documented Flag-and-Stop coverage gaps where covering the path would require a prohibited production seam or filesystem mutation.

**Top 3 risks:**
1. The `Settings.Default` snapshot/restore pattern touches process-global mutable state; correctness depends on every such test class restoring in `[TestCleanup]`. Verified present in all three affected classes, but it is a shared-state coupling that future tests must respect.
2. Two targeted seams (ProjectEntry dialog branches; MatchBestSpecialFolder) remain uncovered. This is an accepted, documented limitation, but it leaves real production branches untested at the unit level.
3. Repo-wide C# coverage remains below the 80% floor (pre-existing 197-COV-001 exception), so the metric gate is satisfied only via the accepted exception, not by reaching the threshold.

**PR readiness recommendation:** **Go** — Test-only, zero production change, full toolchain green per evidence, new-code coverage 100% on reachable paths; the residual gaps and below-floor rate are accepted and documented.

---

## Findings Table

| Severity | File | Location | Finding | Recommendation | Rationale | Evidence |
|---|---|---|---|---|---|---|
| Info | `ToDoModel.Test/Data Model/Project/ProjectEntryTests.cs` | SetProjectId/CompareTo tie-break | Malformed-id, change-confirmation, and length tie-break branches are intentionally not covered (route through static `MyBox.ShowDialog`/WinForms). | None required; gap is documented and Flag-and-Stopped per spec. | Covering would need a WinForms dialog or a new `InternalsVisibleTo`/seam — both prohibited. | `evidence/other/projectentry-malformed-gap.2026-06-14T08-22.md`; per-method 0.5/0.727 line-rate in `evidence/qa-gates/inc1-coverage-delta` |
| Info | `TaskMaster.Test/AppGlobals/` | MatchBestSpecialFolder | `AppFileSystemFolderPaths.MatchBestSpecialFolder` is entirely uncovered; all accessible constructors call `LoadFolders()` which writes to the filesystem. | None required; gap is documented and Flag-and-Stopped per spec. | Constructing the type performs `Directory.CreateDirectory` (prohibited in unit tests); the LoadFolders-free ctor is `private`. | `evidence/other/matchbestspecialfolder-gap.2026-06-14T08-22.md` |
| Minor | `ToDoModel.Test/.../IDListGetNextToDoIDTests.cs`, `TaskMaster.Test/AppGlobals/AppStagingFilenamesTests.cs`, `AppQuickFilerSettingsRemainingPropertiesTests.cs` | TestInitialize/TestCleanup | Tests mutate the process-global `Settings.Default` singleton, mitigated by snapshot/restore. | Keep the snapshot/restore guard on any future tests in these classes; consider a future shared base/fixture to enforce it. | Shared mutable global state is a determinism risk if a future test omits restore; current code restores correctly. | Inspected `[TestInitialize]`/`[TestCleanup]` in all three files |
| Info | `*/`*.Test.csproj | `<Compile Include>` additions | The plan Hard-Constraints list `*.csproj` among files not to edit; the three test csproj got additive Compile-item lines. | None; this is mechanically required by legacy non-SDK projects and matches the pre-existing pattern. | The intent of the constraint is no production/config/pipeline change; test-file registration is not that. | `evidence/qa-gates/final-invariant-check.2026-06-14T08-22.md` |

No Blocker or Major findings.

---

## Implementation Audit

### C# implementation audit

#### What changed well

- Seam reachability is reasoned explicitly in class-level XML doc comments before any test is written (e.g., ToDoLoader's two-delegate Outlook-free constructor, KaKey using the `Keys` enum to avoid a WinForms message loop, QfcQueue using a mocked `IApplicationGlobals` and a null home controller).
- Scenario coverage is methodical: positive, negative, edge, and error variants per target. Arithmetic boundaries (`IDList` base-36 rollover, `BaseChanger` even-length padding, zero/single-digit) and queue/registry state transitions are all exercised.
- Async paths are deterministic: every `Func<…, Task>` test uses `Task.CompletedTask` (no `Task.Delay`/`Sleep`), directly addressing the flaky-timing pattern tracked in #191/#176.
- Where a branch cannot be reached without a prohibited production change, the test restricts itself to reachable branches and records the gap, rather than introducing a silent seam or a temp-file/WinForms dependency.

#### Type safety and API notes

- No production API changed. `internal` members are reached through pre-existing `InternalsVisibleTo` declarations (`ToDoModel.Test`, `TaskMaster.Test`); no visibility was widened in production.
- Nullable build is clean (`/p:Nullable=enable /p:TreatWarningsAsErrors=true` EXIT_CODE 0). Null-delegate and null-argument paths are asserted as either stored-not-rejected (KaChar/KaKey value objects) or throwing (SetAndSave guard), matching the actual production contract verified against source.

#### Error handling and logging

- Tests assert specific exception types (`ArgumentNullException`, `ArgumentException`, `ArgumentOutOfRangeException`, `InvalidOperationException`) rather than broad catches, consistent with the fail-fast policy. No logging surface is involved in the targeted pure seams.

---

## Test Quality Audit

The reviewed evidence consists of the executor's per-increment and final QA-gate artifacts plus the new test source. The reviewer independently parsed the post-feature Cobertura artifact to confirm the recorded coverage increases rather than relying solely on the executor's prose.

### Reviewed test and QA artifacts

- `evidence/qa-gates/final-mstest-coverage.2026-06-14T08-22.md` — confirms vstest.console.exe over the three test DLLs returned EXIT_CODE 0 (99 tests pass).
- `evidence/qa-gates/final-coverage-comparison.2026-06-14T08-22.md` — records per-assembly pre/post line-rates and the net-increase argument; reviewer-verified against the Cobertura XML.
- `evidence/qa-gates/{inc1,inc2,inc3}-coverage-delta.2026-06-14T08-22.md` — per-method line-rate analysis showing 100% on reachable targeted methods.
- `evidence/qa-gates/final-invariant-check.2026-06-14T08-22.md` — confirms zero production change and zero `[ExcludeFromCodeCoverage]` delta.
- `evidence/other/{projectentry-malformed-gap,matchbestspecialfolder-gap}.2026-06-14T08-22.md` — document the two Flag-and-Stop gaps with file/line reasoning.
- `artifacts/csharp/final-fullsuite.cobertura.xml` — reviewer-parsed: root line-rate 0.1907 (full assembly set incl. exempt code); target packages ToDoModel 0.2465, QuickFiler 0.3076, TaskMaster 0.4413.

### Quality assessment prompts

- **Determinism:** No randomness, clock, network, filesystem, or sleep. Async delegates complete synchronously. Global `Settings.Default` snapshotted/restored. Diff scan for prohibited patterns returned only affirming comments.
- **Isolation:** Each test targets one method/branch; private factory helpers keep arrangement local.
- **Speed:** All in-memory; suite ran to EXIT_CODE 0 (no per-test timing separately recorded, but no slow constructs present).
- **Diagnostics:** FluentAssertions `because` strings on essentially every assertion give actionable failure messages.

---

## Security / Correctness Checks

| Check | Status | Evidence |
|---|---|---|
| No secrets in code | ✅ PASS | Test data is literal filenames/keys; no credentials. |
| No unsafe subprocess or command construction | ✅ PASS | No process invocation in tests. |
| Input validation at boundaries | ✅ PASS | Tests assert the production guards (null/empty/invalid-base/invalid-char). |
| Error handling remains explicit | ✅ PASS | Specific exception-type assertions; no broad catches. |
| Configuration / path handling is safe | ✅ PASS | No real filesystem path is read or written; `Settings.Default` restored. |

---

## Research Log

No external research was required. All findings are grounded in the branch diff, the new test source, the feature-folder evidence artifacts, and a direct parse of the post-feature Cobertura coverage XML.

---

## Verdict

The change is ready for normal PR flow. It is a disciplined, test-only contribution that increases measured coverage on the post-#197 testable denominator without touching production code, configuration, or the coverage pipeline. Test quality is high and consistent across all 11 files. The two uncovered targeted seams and the below-floor repo-wide rate are accepted, documented limitations consistent with the feature's spec (Flag-and-Stop rule, Non-Goals) and the merged #197 exemption (197-COV-001); they are not blockers. This conclusion is consistent with the Findings Table (no Blocker/Major) and the Go recommendation above.
