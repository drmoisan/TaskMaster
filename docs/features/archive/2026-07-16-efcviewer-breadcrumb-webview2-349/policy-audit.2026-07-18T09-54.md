# Policy Audit — efcviewer-breadcrumb-webview2 (#349)

- Timestamp: 2026-07-18T09-54
- Reviewer: feature-reviewer
- Branch: `feature/efcviewer-breadcrumb-webview2-349`
- Diff base (merge-base with `origin/epic/folder-tree-breadcrumb-redesign-integration`): `8e242692451f5d4a0c7fe82eab5e01ede66be776`
- HEAD: `be6f38d1f68e923a0ddb779b4dd0bd1c07fe3bd6` (single implementation commit)
- Work mode: `full-feature` (AC sources: `spec.md` and `user-story.md`)
- Scope: full branch diff `8e242692..HEAD`

## Scope and Baseline

Scope was derived from the authoritative sources named in the Scope Invariant: the resolved
merge-base above (recomputed with `git merge-base HEAD origin/epic/folder-tree-breadcrumb-redesign-integration`)
and the full branch diff. The PR context artifacts (`artifacts/pr_context.summary.txt`,
`artifacts/pr_context.appendix.txt`) are absent in this worktree; the `collect_pr_context` MCP
tool is not available in-session, so scope was taken directly from `git diff --numstat 8e242692..HEAD`,
which is authoritative per the Scope Invariant. No caller narrowing was attempted.

Changed files by category (branch diff):

- C# production: `UtilitiesCS/OutlookObjects/Folder/Breadcrumb*.cs` (7 new), `QuickFiler/Controllers/BreadcrumbBridgeRouter.cs`,
  `QuickFiler/Controllers/BreadcrumbOutboundQueue.cs`, `QuickFiler/Viewers/IBreadcrumbWebHost.cs`,
  `QuickFiler/Viewers/WebView2BreadcrumbHost.cs` (new), plus modified
  `QuickFiler/Controllers/EfcFormController.cs`, `QuickFiler/Viewers/EfcViewer.cs`,
  `QuickFiler/Viewers/EfcViewer.Designer.cs`, `QuickFiler/Viewers/EfcViewer3.Designer.cs`.
- C# test: 6 new breadcrumb test files (UtilitiesCS.Test, QuickFiler.Test) plus a compile-fix to
  `EfcHomeControllerExecuteMovesTests.cs`.
- Build wiring: 4 `.csproj` files (`<Compile Include>` entries only).
- Docs / evidence / agent-memory markdown (non-code).

Only C# has changed source files in the branch diff. TypeScript, Python, and PowerShell have zero
changed files on the branch; their coverage verdicts are therefore correctly recorded as N/A below
(these languages have no changed files — the N/A applies only to non-changed languages, not to any
language with a changed file).

## Rejected Scope Narrowing

None. No orchestrator or caller prompt attempted to narrow the audit scope to a plan/task/phase, to
a subset of changed files, or to mark any changed language as out of scope. The manual-verification
ground rule supplied by the orchestrator constrains only the *verdict severity* of five
runtime-observation acceptance criteria (PARTIAL vs FAIL), not the audit scope; it is applied in the
feature audit and does not narrow the policy or coverage scope.

## 1. Policy Compliance Verdicts

Applied policy order: `CLAUDE.md` -> `.claude/rules/general-code-change.md` ->
`.claude/rules/general-unit-test.md` -> C# policy sections (`CLAUDE.md` C#1-C#7, C# Unit Test Policy).

| # | Policy area | Verdict | Evidence |
|---|---|---|---|
| 1 | CSharpier formatting | PASS | `evidence/qa-gates/phase9-final-csharpier.md` EXIT 0 (format) + EXIT 0 (check clean, 0 remaining diffs). |
| 2 | .NET analyzers (`EnableNETAnalyzers`/`EnforceCodeStyleInBuild`) | PASS | `evidence/qa-gates/phase9-final-analyzers.md` EXIT 0, 0 errors / 0 warnings across UtilitiesCS, QuickFiler, and both test projects. |
| 3 | Nullable / `TreatWarningsAsErrors` | PASS | `evidence/qa-gates/phase9-final-nullable.md` EXIT 0, 0 warnings-as-errors; every new file carries `#nullable enable`. |
| 4 | Test framework MSTest + Moq + FluentAssertions | PASS | All 6 new test files reference `Microsoft.VisualStudio.TestTools.UnitTesting`, `Moq`, and `FluentAssertions` (spot-verified `BreadcrumbRowBuilderTests.cs`, `BreadcrumbBridgeRouterTests.cs`); router tested against `Mock<IFolderHierarchyProvider>` and `Mock<IBreadcrumbWebHost>`. |
| 5 | No temporary files in tests | PASS | Grep over all changed test files for `GetTempFileName`/`GetTempPath`/`Path.GetTemp`/`File.WriteAllText`/`File.Create`/`new FileStream`: zero hits. |
| 6 | Banned APIs (`DateTime.Now`, `DateTime.UtcNow`, `Random.Shared`, `Thread.Sleep`, `Task.Delay`) | PASS | Independent reviewer grep over all 23 new/touched `.cs` files: zero hits (corroborates `evidence/qa-gates/banned-api-scan.md`). Init waiting is event-driven on `CoreWebView2InitializationCompleted` + `BreadcrumbOutboundQueue`. |
| 7 | 500-line file cap (new files) | PASS | Largest new file `BreadcrumbBridgeRouter.cs` = 450 lines. The three files over 500 (`EfcViewer.Designer.cs` 4276, `EfcFormController.cs` 1086, `EfcViewer3.Designer.cs` 509) are pre-existing and each SHRANK in this diff (baseline 4308/1122/540 respectively); no new file crosses 500. |
| 8 | net48 constraints (no `init`/`record`/`record struct`) | PASS | New contract types are plain sealed classes with explicit constructors (`BreadcrumbInboundMessage`, `BreadcrumbRenderMessage`, etc.); `BreadcrumbSegment` is a plain type; no `init`/`record`/`record struct` introduced. |
| 9 | I/O isolation (pure core testable without Outlook) | PASS | Pure model/renderer/codec/builder live in UtilitiesCS with no WinForms/COM/WebView2 types; live-Outlook I/O is behind `IFolderHierarchyProvider`; WebView2 I/O is behind `IBreadcrumbWebHost`. Router holds no host types. |
| 10 | `[ExcludeFromCodeCoverage]` limited to COM/VSTO/WinForms exemption categories with in-code justification | PASS | Only 3 occurrences in the diff: `WebView2BreadcrumbHost` (1:1 SDK adapter, in-code justification citing the `WebView2CoreInitializer` precedent), `EfcViewer` (WinForms form), `EfcFormController` (wholly-exempt wiring). The previously-exempt inner `DictionaryProbabilitySource` was deleted. No new exemption on any pure/testable class. |
| 11 | No new testable logic added to `EfcFormController` | PASS | `EfcFormController.cs` net -36 lines; changes are constructor/event wiring to the router (`ConfigureBreadcrumbControl`, `BindBreadcrumbRowsAsync`, fire-and-forget init). All decision logic moved to the non-exempt router/model classes. |
| 12 | Dependency policy (no new packages) | PASS | `.csproj` diffs add only `<Compile Include>` entries; no `<PackageReference>`/`<Reference>`/`packages.config` change. WebView2 SDK and Newtonsoft.Json were already referenced. |
| 13 | Error handling / fail-fast | PASS | Codec fails fast with a logged `BreadcrumbMessageException` on malformed/unknown-type/missing-field JSON; provider I/O boundaries catch, log a specific error, and leave row state unchanged; no silent broad-catch without rethrow or documented boundary. |
| 14 | Logging pattern | PASS | All new host-bound/boundary classes use the repo log4net pattern (`log4net.LogManager.GetLogger`); no ad-hoc console output. Temporary repro instrumentation was authored for removal (see note below). |

### Note on temporary repro instrumentation (process checklist item)

`spec.md` records a temporary log4net `OnShown` diagnostic in `EfcViewer.cs` (P1-T1) to be removed in
P8-T3. The current `EfcViewer.cs` diff shows only the `BreadcrumbWebView` property addition; the
`OnShown` override is not present in the branch head, consistent with the removal step. This is a
process observation, not a policy violation.

## 2. Coverage Audit

Coverage was verified from pre-existing evidence artifacts (per the evidence-verification model);
coverage generation was NOT rerun. Sources: `evidence/qa-gates/phase9-final-tests-coverage.md`,
`evidence/qa-gates/phase9-coverage-delta.md`, and the canonical `artifacts/csharp/coverage.xml`.

### 2.1 C# / .NET coverage — verdict PASS

The C# / .NET coverage verdict is **PASS**. Basis (measured percentages from the evidence
artifacts and the canonical coverage artifact):

- New non-exempt module line coverage (all >= 90% floor for new modules): BreadcrumbRowBuilder 100%,
  BreadcrumbSegment 100%, BreadcrumbMessages types 100%, BreadcrumbRow 98.02%, BreadcrumbBridgeRouter 97.87%,
  BreadcrumbHtmlRenderer 96.90%, BreadcrumbOutboundQueue 95.83%, BreadcrumbMessageCodec 95.56%.
  Every new non-exempt module clears the 90% new-code line bar.
- No regression on the two touched packages: UtilitiesCS line 88.55% -> 88.65%, QuickFiler line
  72.32% -> 73.34% (both improved); UtilitiesCS branch 82.22% -> 82.36%, QuickFiler branch
  62.32% -> 64.30% (both improved). No changed non-exempt line lost coverage.
- Repository floor of 80% applies to the testable denominator per the CLAUDE.md COM/VSTO/WinForms
  exemption. UtilitiesCS line coverage 88.77% (36031/40588) clears both the 80% CLAUDE.md floor and
  the 85% general-unit-test line floor. The correctly-scoped instrumented repository-wide C#
  coverage (UtilitiesCS + QuickFiler, the only production assemblies exercised by the two-assembly
  vstest collection) is 86.42% line and 80.03% branch — both above the 85% / 75% general floors.

C# / .NET coverage verdict: **PASS** (see the comparison line in 2.1.1 and the row in Section 7).

### 2.1.1 Baseline vs post-change comparison (C# / .NET)

- Baseline (P0-T5): UtilitiesCS line 88.55%, QuickFiler line 72.32%; instrumented-scope repo-wide comparable.
- Post-change (P9-T4): UtilitiesCS line 88.65%, QuickFiler line 73.34%; instrumented-scope repo-wide 86.42% line / 80.03% branch.
- Change: UtilitiesCS +0.10 pp line, QuickFiler +1.02 pp line; +97 tests (4838 -> 4935), 0 failures.
- Disposition: PASS. No regression; new-module floors met; testable-denominator floor met.
- Evidence: `evidence/qa-gates/phase9-final-tests-coverage.md`, `evidence/qa-gates/phase9-coverage-delta.md`, `artifacts/csharp/coverage.xml`.
- New/changed-code coverage: >= 95.56% line on every new non-exempt module (lowest is BreadcrumbMessageCodec at 95.56%).

### 2.2 Canonical coverage artifact — scoping observation (procedural, non-blocking)

The canonical `artifacts/csharp/coverage.xml` (JaCoCo-format, converted from the P9-T4 Cobertura run)
aggregates seven packages. Two (UtilitiesCS, QuickFiler) were instrumented by the two-assembly
vstest collection named in `phase9-final-tests-coverage.md`. The other five (SVGControl, TaskMaster,
Tags, ToDoModel, TaskVisualization) read 0% or near-0% covered because their own test projects were
not part of this collection — they were loaded/counted but not exercised. Summing all seven packages
yields a mis-scoped repository-wide figure of 74.49% line / 68.06% branch, which does not reflect the
feature's actual measured coverage. The correctly-scoped instrumented figure is 86.42% line / 80.03%
branch.

Impact: the SubagentStop coverage hook (`validate-feature-review-coverage.ps1`) parses this artifact
as JaCoCo and sums all `//counter` nodes. If a `pr_context.summary.txt` enumerating C# files is later
generated and the hook is exercised, it would read the mis-scoped 74.49% / 68.06% and force a FAIL
verdict against a denominator that includes un-instrumented assemblies. This is the recurring
canonical-JaCoCo scoping defect. Recommended remediation (procedural, non-blocking): re-scope
`artifacts/csharp/coverage.xml` to the instrumented assemblies (UtilitiesCS, QuickFiler) before the
coverage hook is run with a generated PR-context summary. This does not change the coverage verdict,
which is PASS on the correctly-scoped and per-module evidence.

### 2.3 Other languages

- TypeScript: no changed files on the branch. No coverage obligation.
- Python: no changed files on the branch. No coverage obligation.
- PowerShell: no changed `.ps1`/`.psm1` files on the branch. No coverage obligation.

## 3. Evidence Location Compliance

- No branch-diff files were written under `artifacts/baselines/`, `artifacts/qa/`,
  `artifacts/evidence/`, or `artifacts/coverage/` (grep of the diff file list: none). All feature
  evidence is under the canonical `docs/features/active/2026-07-16-efcviewer-breadcrumb-webview2-349/evidence/{baseline,qa-gates,regression-testing,other}/` tree.
- `scripts/dev_tools/validate_evidence_locations.py` is not present in this repository, so the
  scripted scan could not be run; the manual diff scan above is the substitute and found no
  violations.
- `EVIDENCE_LOCATION_OVERRIDE_REJECTED`: none — no delegation prompt specified a non-canonical
  evidence path.

## 4. Summary

- Policy verdicts: 14/14 PASS.
- C# / .NET coverage verdict: PASS (per-module new-code floors met, no regression, testable-denominator floor met).
- One procedural, non-blocking observation: the canonical `artifacts/csharp/coverage.xml` is mis-scoped by inclusion of un-instrumented assemblies; recommend re-scoping before the coverage hook is exercised with a generated PR-context summary.
- Blocking findings: 0.
