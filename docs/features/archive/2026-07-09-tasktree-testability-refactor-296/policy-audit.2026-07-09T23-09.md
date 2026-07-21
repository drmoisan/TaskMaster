# Policy Compliance Audit — tasktree-testability-refactor (#296)

- Timestamp: 2026-07-09T23-09
- Reviewer: feature-reviewer
- Branch under review: `feature/tasktree-testability-refactor-296` @ `b320336ac3ee3c26dbabc2aaa382b5ccde1a7cb7`
- Base branch: `epic/winforms-testability-refactor-integration`
- Merge-base: `3f04d50f6544f084323e5d7a9a563facb9d579df`
- Work Mode (issue.md): `full-feature` → AC sources are `spec.md` (+ `issue.md`); `user-story.md` intentionally absent (refactor child, per spec.md lines 10-14).
- Scope: full branch diff vs merge-base. C# is the only language with changed source files (plus Markdown docs and legacy csproj/config).

## Rejected Scope Narrowing

None. The caller prompt requested the full spec-vs-policy audit of the whole branch diff; no attempt to narrow to a plan/task/phase or to skip a language was present. The plan file was not treated as a scope limiter.

## Evidence Location Compliance

Branch diff scanned for files written under `artifacts/baselines/`, `artifacts/qa/`, `artifacts/evidence/`, `artifacts/coverage/`. None found. All feature evidence is under the canonical `docs/features/active/2026-07-09-tasktree-testability-refactor-296/evidence/<kind>/` tree (baseline/, qa-gates/, issue-updates/). No evidence-location violation.

## Language Coverage Verdicts (mandatory, per changed language)

| Language | Changed files? | Coverage verdict | Basis |
|---|---|---|---|
| C# | Yes (TaskTree/*.cs, TaskTree.Test/*.cs) | PARTIAL / see note | Evidence documents report TaskTree.dll line 94.04%; canonical Cobertura artifact `artifacts/csharp/coverage.xml` is NOT committed to the branch, so the figure could not be independently recomputed. Line-floor (>=80%) is supported by evidence; branch-coverage (>=75%, general-unit-test.md) is not reported anywhere and is UNVERIFIED. The reported figure is additionally computed WITH the E4/E5/E6 exclusions that this audit finds Blocking (see §Coverage Exemption Adjudication); removing them lowers the measured base until replacement tests are added. |
| PowerShell | No | N/A | Zero `.ps1` changed in the branch diff. |
| Python | No | N/A | Zero `.py` changed. |
| TypeScript | No | N/A | Zero `.ts`/`.tsx` changed. |

Coverage artifact note: `artifacts/csharp/coverage.xml` referenced by `evidence/qa-gates/final-coverage.md` is gitignored and absent from the checked-out branch. Independent recomputation was therefore not possible in this worktree. The child→integration PR runs zero required CI checks, so no CI coverage run backstops this locally.

## Toolchain Compliance (C# — CLAUDE.md CUT3 order)

| Stage | Command (per evidence) | Result | Evidence |
|---|---|---|---|
| 1. Format | `csharpier check .` | EXIT 0, 1326 files clean (one restart after AssemblyInfo line-endings fix) | evidence/qa-gates/final-format.md |
| 2. Analyzers | `msbuild ... /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` | EXIT 0, 0 error / 0 warning | evidence/qa-gates/final-analyzers.md |
| 3. Nullable | `msbuild ... /p:Nullable=enable /p:TreatWarningsAsErrors=true` | EXIT 0, 0 error / 0 warning | evidence/qa-gates/final-nullable.md |
| 4. Test | `dotnet-coverage collect ... vstest.console.exe TaskTree.Test.dll` | EXIT 0, 37 passed / 0 failed | evidence/qa-gates/final-coverage.md |

Toolchain order and green single-pass are documented. Not independently re-run in this review (msbuild/vstest not on the review PATH); accepted on evidence with the coverage-artifact caveat above.

## Policy Checklist

| Policy | Verdict | Evidence |
|---|---|---|
| General Code Change §4 / general-code-change.md — no file > 500 lines (production AND test) | PASS | Independent count of every `TaskTree/*.cs` and `TaskTree.Test/*.cs` (awk NR): max production 312 (`TaskTreeForm.Designer.cs`), max test 447 (`TaskTreeControllerTests.cs`). No file over 500. See §File-Size below. |
| Separation of concerns (§1.4) — pure logic vs COM/WinForms/IO | PARTIAL | Move/tree logic correctly isolated in `TaskTreeController.MoveLogic.cs` behind `ITreeVisual`; facade in `TaskTreeForm`. However E4/E5/E6 leave testable decision-logic entangled inside exempt COM/live-control methods (see adjudication). |
| C# Code Change / C#1 tooling | PASS | csharpier + msbuild analyzer + nullable gates all EXIT 0. |
| C# Unit Test / CUT1-CUT2 — MSTest + Moq + FluentAssertions | PASS | Both test files use `[TestClass]`/`[TestMethod]`, `Mock<>`, and FluentAssertions. No xUnit/NUnit. |
| general-unit-test UT4 — no temp files, no external deps, mocks for boundaries | PASS | Tests mock `ITaskTreeForm`/`ITreeVisual`/`IApplicationGlobals`/Outlook interop; no temp files; `DropArgs` builds event args via reflection (in-memory). |
| general-unit-test — determinism, no Thread.Sleep/Task.Delay/DoEvents | PASS | Grep of test files: no `Thread.Sleep`, `Task.Delay`, `DoEvents`, `Show()`, `ShowDialog()`. Async tests await deterministically. |
| Maintainer-ratified STA policy (dedicated `*.StaTests.cs`, seams-first, no pumps, no Form-derived in tests, disposed) | PASS | No `[STATestClass]`/`[STATestMethod]`, no `*.StaTests.cs`, no `Form`/`Control` constructed in tests. Spec assessed STA and did not exercise it (spec.md lines 315-327); test suite constructs zero live controls. Honored. |
| general-unit-test Coverage Exclusion Policy — no exemption on testable seam | FAIL | E4, E5, E6 exempt testable seams (see adjudication). Blocking. |
| Coverage line floor >= 80% (CLAUDE.md UT2) | PASS (evidence) | 94.04% per final-coverage.md (measured with the Blocking exclusions; see caveat). |
| Coverage branch floor >= 75% (general-unit-test.md) | UNVERIFIED | Not reported in any evidence file; artifact absent. |
| New-file coverage >= 90% | PASS (evidence) | Controller 95.65%, MoveLogic 93.29% per coverage-delta.md (with the Blocking exclusions). |
| Solution wiring (non-globbing csproj → explicit `<Compile Include>`; sln entry + both platform configs) | PASS | See §Solution Wiring. |

## File-Size Independent Count (awk NR)

Production `TaskTree/`: ITaskTreeForm.cs 79; TaskTreeController.cs 206; TaskTreeController.MoveLogic.cs 295; TaskTreeForm.cs 194; TaskTreeForm.Designer.cs 312; TreeListViewVisual.cs 45; properties/AssemblyInfo.cs 40; My Project/Application.Designer.cs 11; My Project/AssemblyInfo.cs 35.
Test `TaskTree.Test/`: TaskTreeControllerTests.cs 447; TaskTreeControllerMoveLogicTests.cs 414; Properties/AssemblyInfo.cs 20; TaskTree.Test.csproj 310.

Baseline: Baseline / Post-change / Disposition — `TaskTreeController.cs` 546 (baseline, over limit) → 206 + 295 (post-change split) → Disposition PASS. All other files unchanged or new and under 500. No 500-line violation exists; the #293 hidden-over-500 test-file risk was independently checked and is NOT present here (largest test file is 447).

## Solution Wiring (AC6)

- `TaskTree.Test.csproj` is a legacy non-SDK (non-globbing) csproj. It carries explicit `<Compile Include>` entries for all three `.cs` files: `Properties\AssemblyInfo.cs`, `TaskTreeControllerTests.cs`, `TaskTreeControllerMoveLogicTests.cs`. No orphaned source file. PASS.
- `ProjectGuid` `{7C4E2B1A-3F9D-4A6E-8B2C-1D5E9F0A7C36}` is unique (does not reuse Tags.Test). `ProjectTypeGuids` includes the unit-test type GUID. `TargetFrameworkVersion v4.8.1`, `TestProjectType UnitTest`.
- `ProjectReference`s: `..\TaskTree\TaskTree.csproj`, `..\ToDoModel\ToDoModel.csproj`, `..\UtilitiesCS\UtilitiesCS.csproj` — all present with matching GUIDs.
- `BannedSymbols.txt` `<AdditionalFiles>` present.
- `TaskMaster.sln`: project entry at line 42; `ProjectConfigurationPlatforms` entries for the new GUID cover Debug|Any CPU + Release|Any CPU (ActiveCfg + Build.0) plus x64/x86 fallbacks. PASS.

## Coverage Exemption Adjudication (E4 / E5 / E6)

Full evidence-first determinations are in the accompanying feature-audit and code-review artifacts. Summary verdicts:

- E1 `TaskTreeForm` — permitted (Form-derived, ratified WinForms category). Not a finding.
- E2 `TreeListViewVisual` — permitted (minimal 2-line adapter over non-virtual ObjectListView control; bottom of the seam hierarchy). Not a finding.
- E3 `FormatRow` wrapper — permitted (decision extracted to covered `ResolveRowStyle`; residual wrapper reads non-constructible `FormatRowEventArgs`). Not a finding. This is the correct extract-decision/exempt-thin-wrapper pattern.
- E4 `ActivateOlItem(dynamic item)` — **FAIL / BLOCKING.** Exempts a testable seam. The Explorer is reachable via the mockable `IApplicationGlobals` seam; the only obstacle is the self-inflicted `dynamic` parameter. spec.md line 351 planned to cover it by mocking `Explorer.IsItemSelectableInView`.
- E5 `ActivateOlItemAsync` — **FAIL / BLOCKING.** Same as E4 (async form).
- E6 `HandleModelDropped` — **FAIL / BLOCKING.** Exempts host-neutral drop-routing that spec.md line 340 planned to cover with `ITreeVisual` mocks; only the terminal `e.RefreshObjects()` + adapter construction is irreducible.

Collateral: because E4/E5 freeze the `dynamic` design, the NON-exempt caller branches `TreeLvActivateItem`/`TreeLvActivateItemAsync` valid-type paths (the `ActivateOlItem(objItem)` / `await ActivateOlItemAsync(objItem)` calls) are left uncovered, accounting for uncovered lines in the otherwise 95.65% controller.

## Overall Policy Verdict

FAIL (3 Blocking coverage-exemption findings: E4, E5, E6). All other audited policies PASS or PARTIAL. Remediation inputs produced.
