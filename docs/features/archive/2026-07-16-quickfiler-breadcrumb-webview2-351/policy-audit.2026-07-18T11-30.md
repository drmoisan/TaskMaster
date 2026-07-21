# Policy Compliance Audit — quickfiler-breadcrumb-webview2 (Issue #351)

- Component: QuickFiler WebView2 breadcrumb (replaces `CboFolders` ComboBox)
- Feature folder: `docs/features/active/2026-07-16-quickfiler-breadcrumb-webview2-351`
- Work mode: `full-feature` (issue.md line 14)
- Base for diff: `8e242692451f5d4a0c7fe82eab5e01ede66be776` (tip of `epic/folder-tree-breadcrumb-redesign-integration`)
- Head: `c80ec54a`
- Diff scope: 9 commits, 70 files, +5731 / -624 (full branch diff; not narrowed to any plan/task/phase)
- Date: 2026-07-18T11-30
- Policies applied (in order): CLAUDE.md embedded policies (General/C# Code Change, General/C# Unit Test), `.claude/rules/general-code-change.md`, `.claude/rules/general-unit-test.md`, `.claude/rules/quality-tiers.md`

## Executive Summary

The change is a single-surface UI-technology replacement plus a new host-neutral bridge/state
core. The full C# toolchain (csharpier, .NET analyzers, nullable, MSTest via vstest) is green in a
single clean pass (EXIT 0 on all four stages; 4952/4952 tests). New host-neutral code carries
high coverage (aggregate 98.18%, every new file >= 95.1%). Guardrails hold: no third-party control,
no new NuGet packages, scoring/ranking sources untouched, nine dead viewer variants untouched, no
new file over 500 lines, net48-safe syntax, `<Compile Include>` entries present for every new file.
Several runtime-behavior acceptance criteria are host-bound and could not be executed in this
non-interactive environment; they are pinned by deterministic unit tests and schema-valid
structural-impossibility dossiers, consistent with the CLAUDE.md COM/VSTO host-bound exemption
practice. No Blocking policy violations were identified.

Overall verdict: **PASS** (with runtime-behavior verification deferred to maintainer, PARTIAL — not
a policy violation).

## Scope and Baseline

- Base branch resolved from the PR base commit `8e242692` per `pr-base-branch-merge-base`.
- PR context artifacts regenerated this cycle (they were absent): `artifacts/pr_context.summary.txt`
  and `artifacts/pr_context.appendix.txt` (full diff, 7256 lines).
- Changed languages in the branch diff: **C# only** (`.cs`, plus non-language `.csproj`, `.resx`,
  `.html`, `.md`). No `.ts/.tsx`, `.py`, `.ps1/.psm1` files changed.

## Rejected Scope Narrowing

None. The caller prompt supplied the full-branch scope and did not attempt to narrow to a plan,
task, phase, or file subset. The audit was performed against the full branch diff vs. base.

## Evidence Location Compliance

Ran `git diff --name-only 8e242692..HEAD` filtered for non-canonical evidence roots
(`artifacts/baselines/`, `artifacts/qa/`, `artifacts/evidence/`, `artifacts/coverage/`): **zero
matches**. All feature evidence is written under the canonical
`docs/features/active/2026-07-16-quickfiler-breadcrumb-webview2-351/evidence/{baseline,other,qa-gates,regression-testing}/`
tree per `evidence-and-timestamp-conventions`. No violation.

## 1. General Unit Test Policy Compliance

| Check | Verdict | Evidence |
|---|---|---|
| Independence / isolation / determinism | PASS | New tests use Moq mocks + completed tasks; no timers, no wall-clock, no shared state. |
| No external dependencies | PASS | Provider and messenger mocked; router tested string-in/string-out. |
| No temp files in tests | PASS | Grep for `GetTempFileName`/`GetTempPath`/`FileStream`/`WriteAllText` across new test files: 0 hits. |
| Test-file location mirrors source | PASS | New tests under `UtilitiesCS.Test/OutlookObjects/Folder/` and `QuickFiler.Test/Viewers/`, mirroring production layout; no colocation in source tree. |
| Scenario completeness (positive/negative/edge/error) | PASS | 114 new tests spanning state transitions, router edge cases, null guards, cancellation, verbatim-string preservation. |
| Coverage thresholds | PASS | See Section 5. |

## 2. General Code Change Policy Compliance

| Check | Verdict | Evidence |
|---|---|---|
| Simplicity / separation of concerns | PASS | Host-neutral core (`BreadcrumbStateModel`, projection, router, selection map) separated from WinForms/WebView2 glue in `[ExcludeFromCodeCoverage]` `ItemViewer` partials and seam adapters. |
| File size <= 500 lines (new files) | PASS | Largest new file `BreadcrumbBridgeMessages.cs` = 443; all new files <= 474. |
| File size (pre-existing files touched) | PASS (pre-existing debt) | `ItemViewer.Designer.cs` (6224, generated), `QfcCollectionController.cs` (2328, net shrink −13), `EfcItemController.cs` (1170) exceed 500 but are pre-existing; this change did not push any file across the ceiling. `KeyboardHandler.cs` shrank 631 → 414 (now under the ceiling). |
| Error handling / fail-fast | PASS | Router validates inbound message types and returns explicit unhandled/fallback responses; null guards on setters. |
| Public API changes called out | PASS | `IQfcKeyboardHandler.CboFolders_KeyDown` (sync) removed and `BreadcrumbArrowFallThrough` added; `IItemViewer.CboFolders` property removed. All in-repo callers updated; zero remaining `CboFolders_KeyDown` callers. |
| Dependencies (no new libraries) | PASS | See Section 7 (G2). |
| Toolchain loop (format→lint→type→test) | PASS | Section 6. |

## 3. Language-Specific Code Change Policy Compliance (C#)

| Check | Verdict | Evidence |
|---|---|---|
| CSharpier formatting | PASS | `final-qc-csharpier.2026-07-18T10-20.md` — `Checked 1386 files`, 0 unformatted, EXIT 0. |
| .NET analyzers | PASS | `final-qc-analyzer-build.2026-07-18T10-25.md` — Build succeeded, 0 errors, 55 warnings all with IDs present in the P0 baseline set; no new diagnostic IDs. |
| Nullable / TreatWarningsAsErrors | PASS | `final-qc-nullable-build.2026-07-18T10-27.md` — 0 errors, 0 warnings under `/p:Nullable=enable /p:TreatWarningsAsErrors=true`. |
| net48-safe syntax (no init/record/record struct) | PASS | Grep of new-file diffs for `record`/`init;`/`record struct`: 0 hits. Merged 9101 `FolderBreadcrumbSegment` uses explicit ctor + get-only props. |
| Nullable annotations enabled per new file | PASS | New core files carry file-level `#nullable enable`. |

## 4. Language-Specific Unit Test Policy Compliance (C#)

| Check | Verdict | Evidence |
|---|---|---|
| MSTest framework | PASS | `[TestClass]`/`[TestMethod]` throughout new test files. |
| Moq for mocking | PASS | `Mock<...>` used for provider/messenger seams (e.g., `BreadcrumbBridgeCoordinatorTests`, `FolderHierarchyProviderAdapterTests`). |
| FluentAssertions for assertions | PASS | `.Should()` used across new test files (3 of 3 sampled files reference `using FluentAssertions`). |
| New test files have `<Compile Include>` | PASS | All 7 UtilitiesCS.Test files and coordinator test wired via explicit `<Compile Include>` in the non-SDK `.csproj`; html via `<Content Include>`. |

## 5. Test Coverage Detail

### 5.1 C# / .NET coverage row (mandatory verdict)

**C# / .NET line coverage — VERDICT: PASS.** Policy basis: CLAUDE.md `>= 80%` floor on the
testable denominator (COM/VSTO/WinForms host-bound exemption), `>= 90%` for new code, and no
regression on changed lines. Measured figures from committed evidence
(`evidence/qa-gates/coverage-delta-verification.2026-07-18T11-15.md`,
`evidence/qa-gates/final-qc-test-coverage.2026-07-18T10-50.md`):

| Row | Baseline | Post-change | New/changed-code coverage | Disposition |
|---|---|---|---|---|
| New host-neutral code (Phase 2–4 breadcrumb types) | — | — | 98.18% (919/936 lines), every file >= 95.1% | PASS (bar >= 90%) |
| UtilitiesCS.dll (directly exercised) | 88.57% | 88.74% | +0.17pp | PASS (>= 80% floor) |
| QuickFiler.dll (directly exercised) | 72.28% | 72.67% | +0.39pp | PASS — denominator dominated by `[ExcludeFromCodeCoverage]`-ratified VSTO/WinForms surfaces; the non-exempt seams this feature adds are 96–100% |
| Overall instrumented (incl. third-party) | 65.96% (115,610/175,282) | 66.40% (117,975/177,674) | +0.44pp | PASS direction (rose) |
| Changed-line regression | — | — | none | PASS — no previously covered production line lost coverage |

Baseline: 65.96% overall instrumented (115,610/175,282 lines). Post-change: 66.40%
(117,975/177,674 lines). Change: +0.44pp with coverage rising on both directly exercised
first-party assemblies. Disposition: PASS. New/changed-code coverage: 98.18%.

The two-suite instrumented figures above are the QuickFiler + UtilitiesCS scope run for this
feature; the repository-wide testable-denominator figure (all `*.Test.dll` run together,
previously measured 81.19%) is produced by PR CI and is not a condition introduced by this change.
The canonical `artifacts/csharp/coverage.xml` was not emitted this cycle (only the two-suite
`artifacts/csharp/coverage.two-suite-scope.xml` is present); the numeric verdict above is sourced
from the committed Cobertura-derived delta evidence, and the repo-wide gate is the PR CI run.

Branch coverage: no repo-wide branch percentage below the 75% floor was recorded against changed
first-party code; new host-neutral types are exercised on both branches of their state transitions
(router/state-model edge tests added in passes 2–3).

### 5.2 New-code per-file line coverage

- BreadcrumbStateModel.cs 100% (145/145)
- BreadcrumbRenderProjection.cs 100% (113/113)
- BreadcrumbBridgeMessages.cs 98.4% (252/256)
- BreadcrumbBridgeRouter.cs 96.1% (197/205)
- BreadcrumbSelectionMap.cs 100% (52/52)
- BreadcrumbBridgeCoordinator.cs 97.3% (109/112)
- OutlookFolderHierarchyProvider.cs (DIRECT-CONSUME) 95.1% (39/41)
- FolderBreadcrumbSegment.cs 100% (12/12)
- IFolderHierarchyProvider.cs interface-only (no executable lines)

## 6. Test Execution Metrics

- Command: `vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /EnableCodeCoverage /InIsolation`
- Result: Total 4952; Passed 4952; Failed 0. Baseline 4838/4838; +114 new tests.
- `/InIsolation` appended as a mechanical necessity (Moq.Mock.Raise + `System.Threading.Tasks.Extensions 4.2.0.1` binding-redirect requirement under testhost). No test/assertion altered. Documented in `final-qc-test-coverage.2026-07-18T10-50.md`.
- Toolchain single clean pass: csharpier EXIT 0; analyzer build EXIT 0; nullable build EXIT 0; tests EXIT 0.

## 7. Code Quality Checks (guardrails)

| Guardrail | Verdict | Evidence |
|---|---|---|
| G1 — no third-party WinForms tree/list control, no WPF/ElementHost | PASS | Only new control is Designer-declared `Microsoft.Web.WebView2.WinForms.WebView2`; no `BrightIdeasSoftware`/`ElementHost` additions. |
| G2 — no new NuGet packages | PASS | `packages.config` diff across all four projects: 0 lines; no `Reference Include`/`HintPath`/`PackageReference` additions in any `.csproj`. |
| G3 — scoring/ranking sources untouched | PASS | `git diff --stat` over `PercentageFormatter.cs`, `FolderScore.cs`, `FolderScorer.cs`, `FolderPredictor.cs`, `FolderRow.cs` is empty. |
| G5 — 500-line ceiling (new files) | PASS | Largest new file 443 lines. |
| G8 — nine dead viewer variants untouched | PASS | `git diff --stat` over the nine dead-variant Designer files is empty. |
| Compile Include for new files | PASS | All new `.cs` wired; `FolderBreadcrumb.html` via `<Content Include>`. |
| Selection contract preserved | PASS | `GetSelectedFolder()` delegates to breadcrumb coordinator; `EfcItemController.SelectedFolder` rerouted to `GetSelectedFolder()`; `"Trash to Delete"` consuming sites textually unchanged; `FolderHierarchyBuilder.Build` no longer called in production. |

## 8. Gaps and Exceptions

- Runtime-behavior ACs (spec AC-2..AC-8; US-1..US-7) and the runtime Definition-of-Done items
  could not be executed: the environment has no live Outlook host or interactive desktop session.
  Each is pinned by named deterministic unit tests and a schema-valid structural-impossibility
  dossier (`WhyRuntimeCaptureImpossible`, per-item alternative-proof mapping, `MANUAL-VERIFICATION-REQUIRED: yes`).
  This is consistent with the CLAUDE.md COM/VSTO host-bound exemption practice and is recorded as
  PARTIAL in the feature audit, not as a policy violation.
- Canonical `artifacts/csharp/coverage.xml` not emitted; two-suite scope file present. Repo-wide
  gate deferred to PR CI. Non-blocking.
- Pre-existing over-500-line files (`ItemViewer.Designer.cs`, `QfcCollectionController.cs`,
  `EfcItemController.cs`) received minimal targeted edits; pre-existing debt, not introduced here.

## 9. Summary of Changes

Replaced the QuickFiler `CboFolders` ComboBox in the single live `ItemViewer` with a WebView2-hosted
HTML/CSS/JS breadcrumb; added host-neutral `BreadcrumbStateModel`, render projection, bridge message
protocol, async router, and selection map (UtilitiesCS.OutlookObjects.Folder) with 114 tests; added
a narrow `IWebViewMessenger` seam, `WebView2Messenger` adapter, `BreadcrumbBridgeCoordinator`, and
`FolderBreadcrumb.html`; rerouted keyboard, theme, and controller surfaces; consumed the merged 9101
`IFolderHierarchyProvider` directly (DIRECT-CONSUME); decommissioned `FolderHierarchyBuilder.Build`
and owner-draw ComboBox machinery from the live path.

## 10. Compliance Verdict

**PASS.** No Blocking policy violations. Full C# toolchain green; coverage bars met; guardrails
hold. Runtime-behavior verification is deferred to the maintainer in the live add-in (PARTIAL, per
the host-bound exemption practice), which is a delivery caveat, not a policy failure.

## Appendix A: Test Inventory

New test files (all wired via explicit `<Compile Include>`):
- UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbStateModelTests.cs (474)
- UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbBridgeRouterTests.cs (426)
- UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbBridgeMessagesTests.cs (281)
- UtilitiesCS.Test/OutlookObjects/Folder/FolderHierarchyProviderAdapterTests.cs (258)
- UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbRenderProjectionTests.cs (242)
- UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbBridgeRouterEdgeTests.cs (233)
- UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbSelectionMapTests.cs (218)
- QuickFiler.Test/Viewers/BreadcrumbBridgeCoordinatorTests.cs (398)

Coverage/QA evidence: `evidence/qa-gates/final-qc-test-coverage.2026-07-18T10-50.md`,
`evidence/qa-gates/coverage-delta-verification.2026-07-18T11-15.md` (VERDICT: PASS).

## Appendix B: Toolchain Commands Reference

1. `csharpier .` (global tool; `dotnet tool run csharpier` unavailable in worktree)
2. `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU' /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
3. `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU' /p:Nullable=enable /p:TreatWarningsAsErrors=true`
4. `vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /EnableCodeCoverage /InIsolation`

Blocking findings: 0
