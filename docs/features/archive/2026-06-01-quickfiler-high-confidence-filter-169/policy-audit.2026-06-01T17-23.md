# Policy Compliance Audit — quickfiler-high-confidence-filter (Issue #169)

- Generated: 2026-06-01T17-23 (UTC)
- Base branch (resolved): `development`
- Merge-base SHA: `3322bbee6a941eaa05e8388dd78ec3998e542d75`
- Head SHA: `32de29d7748492eb0ec62219f2fe20b3d279142e`
- Diff range: `3322bbee6a941eaa05e8388dd78ec3998e542d75..32de29d7748492eb0ec62219f2fe20b3d279142e`
- Work mode: full-feature (issue.md absent → fail-closed to full-feature, matching caller input)
- Scope: full branch diff vs. base (feature-vs-base audit)

## Scope Note (PR-context summary discrepancy)

The PR-context summary (`artifacts/pr_context.summary.txt`) reports "Core logic changes: 0 files"
and lists only docs/tooling under "Changed files overview". This is inaccurate relative to the
actual branch diff, which contains 19 changed C# source/test files plus one ribbon XML file. Per
the workflow scope invariant, this audit is performed against the full branch diff, not the
summary's understated view. The summary artifact is stale/misclassified; this does not narrow audit
scope.

Full diff (production + test, excluding docs/evidence):

| File | Type | +/- |
|---|---|---|
| `UtilitiesCS/OutlookObjects/Folder/FolderScorer.cs` | modified | +8 |
| `UtilitiesCS/Interfaces/IGlobals/IAppQuickFilerSettings.cs` | modified | +2 |
| `QuickFiler/Controllers/QfcCollectionController.cs` | modified | +39 |
| `QuickFiler/Controllers/QfcFormController.cs` | modified | +26 |
| `QuickFiler/Controllers/QfcItemController.cs` | modified | +6 |
| `QuickFiler/Interfaces/IQfcCollectionController.cs` | modified | +12 |
| `QuickFiler/Interfaces/IQfcItemController.cs` | modified | +7 |
| `TaskMaster/AppGlobals/AppQuickFilerSettings.cs` | modified | +20 |
| `TaskMaster/Properties/Settings.Designer.cs` | modified | +26/-1 |
| `TaskMaster/Properties/Settings.settings` | modified | +6 |
| `TaskMaster/Ribbon/RibbonController.cs` | modified | +60 |
| `TaskMaster/Ribbon/RibbonViewer.cs` | modified | +11 |
| `TaskMaster/Ribbon/RibbonExplorer.xml` | modified | +14 |
| `UtilitiesCS.Test/OutlookObjects/Folder/FolderScorerTests.cs` | modified (added tests) | +59 |
| `TaskMaster.Test/AppGlobals/AppQuickFilerSettingsTests.cs` | added | +88 |
| `TaskMaster.Test/Ribbon/RibbonControllerTests.cs` | added | +146 |
| `TaskMaster.Test/TaskMaster.Test.csproj` | modified | +2 |
| `QuickFiler.Test/Controllers/QfcCollectionControllerTests.cs` | modified (added tests) | +159 |
| `QuickFiler.Test/Controllers/QfcFormControllerTests.cs` | modified (added tests) | +72 |

## Rejected Scope Narrowing

None. The caller specified the full feature-vs-base scope and did not attempt to narrow scope to a
plan, task, phase, or subset of changed files. (The PR-context summary misclassification noted above
is treated as a stale artifact, not a caller narrowing instruction.)

## Changed Languages in Branch Diff

- C# (`.cs`): present (19 files). Coverage verdict required (PASS/FAIL).
- TypeScript / Python / PowerShell: zero changed files on branch → not applicable.

## Verdict Summary

| Policy area | Verdict | Evidence |
|---|---|---|
| C# formatting (CSharpier) | PASS | `csharpier check` on 15 changed `.cs` files: 0 require reformatting |
| C# analyzers (.NET) | PASS | independent `msbuild ... /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`: see Appendix B |
| C# nullable / type-check | PASS | independent `msbuild ... /p:Nullable=enable /p:TreatWarningsAsErrors=true`: see Appendix B |
| C# tests (MSTest) | PARTIAL | 24 issue-169 tests pass; suite has 11 pre-existing flaky failures under coverage instrumentation |
| C# coverage | FAIL | canonical artifact `artifacts/csharp/coverage.xml` absent; verification mandatory for changed language |
| Test framework conformance (MSTest/Moq/FluentAssertions) | PASS | all new tests use the mandated stack |
| General code-change design principles | PASS | seam-based DI, focused methods, guard clauses, XML docs |
| File size limit (<= 500 lines) | PARTIAL | pre-existing oversize files touched; not introduced by this work |
| Logging / error handling | PASS | guard clauses; no broad catch; no new ad-hoc console output |
| Evidence location compliance | PASS | all evidence under canonical `<FEATURE>/evidence/<kind>/` |
| Tone policy (docs authored by feature) | PASS | spec/user-story use neutral factual language |
| Behavioral correctness vs. spec/AC | FAIL | AC6 regression: high-confidence mode persists and leaks to the standard entry point (see Code Review F1) |

## Detailed Findings

### C# Formatting — PASS
Independent check-only run:
`dotnet tool run csharpier check <15 changed .cs files>` → "Checked 15 files"; no files require
reformatting. Consistent with the executor's recorded formatter pass.

### C# Analyzers — PASS
Independent solution build with analyzers enabled. See Appendix B for the exact command and result
(Build succeeded, 0 warnings, 0 errors).

### C# Nullable / Type-check — PASS
Independent solution build with nullable enabled and warnings-as-errors. See Appendix B (Build
succeeded, 0 warnings, 0 errors). The new `TopFolderScore` and `TopScore()` members use
null-conditional access and a `?? 0` fallback; `ApplyHighConfidenceFilterAsync` and
`RemoveBelowThresholdAsync` apply explicit null guards.

### C# Tests — PARTIAL
- The 24 tests added for issue #169 are present and target the new logic (FolderScorer.TopScore,
  AppQuickFilerSettings settings, RemoveBelowThresholdAsync selection, ApplyHighConfidenceFilterAsync
  conditional, RibbonController helpers). All pass per executor evidence and independent inspection
  of the test bodies.
- The executor evidence (`evidence/qa/final-toolchain.2026-06-01T17-12-39Z.md`) records 11 failing
  tests under coverage instrumentation, asserted to be pre-existing flaky timing/concurrency tests in
  `UtilitiesCS.Test` unrelated to #169. The baseline (`evidence/baselines/tests-coverage...txt`) shows
  the same flaky category (8 failures, passing on re-run) before any code change. This corroborates
  the "pre-existing flakiness" classification. The PARTIAL verdict reflects that the full suite is not
  green in a single instrumented pass; the issue-169 subset is green. No new failing tests were
  introduced by this work.

### C# Coverage — FAIL
- The canonical C# coverage artifact required by this workflow, `artifacts/csharp/coverage.xml`
  (JaCoCo/Cobertura XML consumed by `validate-feature-review-coverage.ps1`), is **absent**. The
  coverage-verification model for this workflow is mandatory for every language with changed files;
  an absent artifact for a changed language is a FAIL and a remediation trigger.
- The executor instead produced a narrative comparison at
  `evidence/coverage/comparison.2026-06-01T17-12-39Z.md` (canonical feature evidence location, which
  is correct for evidence storage) reporting:
  - UtilitiesCS.dll line coverage 85.45% (>= 80%, no regression vs. 85.39% baseline).
  - QuickFiler.dll 23.40% and TaskMaster.dll 25.16% — both well below the 80% repo-wide floor. These
    assemblies are dominated by VSTO/WinForms/COM UI code. The executor argues the controlling gate is
    the dominant library plus per-new-member coverage. That argument does not satisfy the literal
    repo-wide ">= 80% per language/assembly" requirement for QuickFiler.dll and TaskMaster.dll.
  - New pure/logic members reach 90–100%, except `QfcItemController.TopFolderScore` (0%, COM seam),
    `RibbonViewer` callbacks (0%, Office ribbon callbacks), and
    `RibbonController.LoadQuickFilerHighConfidenceAsync` (0%, live COM/WinForms launch).
- Verdict rationale: the canonical machine-checkable coverage artifact is missing, so coverage cannot
  be independently verified by the workflow's required mechanism. Separately, the narrative shows two
  production assemblies below the 80% floor and one new added member of substantive behavior
  (`LoadQuickFilerHighConfidenceAsync`, which carries the only behavioral difference of the feature's
  entry point and the defect in Code Review F1) at 0% coverage. Both are remediation triggers.

### File Size Limit — PARTIAL
`QfcItemController.cs` (~2437), `QfcCollectionController.cs` (~2207), `QfcFormController.cs` (~1080),
and `FolderScorer.cs` (607) exceed the 500-line limit. Per the diff and the executor note, these
files already exceeded the limit at the merge-base; this work added small members rather than causing
the oversize. The pre-existing condition is recorded for transparency and is not a new violation
introduced by issue #169. No remediation is required of this feature for the pre-existing condition,
but the files should not continue to grow; a future split should be tracked separately.

## Evidence Location Compliance

All evidence artifacts produced for this feature reside under the canonical
`docs/features/active/quickfiler-high-confidence-filter-169/evidence/<kind>/` tree:
- `evidence/baselines/` (csharpier, analyzer-build, nullable-build, policy-read, tests-coverage)
- `evidence/coverage/` (comparison)
- `evidence/qa/` (final-toolchain, ac-status)

No files were written to forbidden locations (`artifacts/baselines/`, `artifacts/qa/`,
`artifacts/coverage/`, `artifacts/evidence/`). The `validate_evidence_locations.py --root .` script
referenced by the agent contract is not present in this repository; the equivalent rule is enforced
by `.claude/hooks/enforce-evidence-locations.ps1`, whose forbidden-prefix set none of the feature's
evidence files match. Verdict: PASS.

Note: the workflow's coverage table designates `artifacts/csharp/coverage.xml` as the C# coverage
artifact. That path is on the hook allowlist (not forbidden) and is the canonical location for the
machine-readable coverage XML specifically; it is distinct from narrative evidence under
`<FEATURE>/evidence/coverage/`. Its absence is the basis for the C# coverage FAIL above.

## Appendix B — Command Reference

| Step | Command | Result |
|---|---|---|
| Format check | `dotnet tool run csharpier check <15 changed .cs files>` | PASS — 15 files checked, 0 need reformatting |
| Analyzer build | `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` | PASS — see build log (0 warnings, 0 errors) |
| Nullable build | `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true` | PASS — see build log (0 warnings, 0 errors) |
| Tests + coverage | `vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll QuickFiler.Test\bin\Debug\QuickFiler.Test.dll TaskMaster.Test\bin\Debug\TaskMaster.Test.dll /EnableCodeCoverage` | Not re-run by reviewer; executor evidence inspected. Canonical `artifacts/csharp/coverage.xml` absent. |

Note on environment: `msbuild` and `vstest.console.exe` are available on the Windows PATH via Visual
Studio 18. Reviewer ran CSharpier and the two msbuild gates independently (check-only). The
coverage-instrumented vstest run was not re-executed by the reviewer; coverage was assessed by
inspecting the executor's pre-existing evidence per the workflow's evidence-verification model, and
the canonical coverage XML artifact was confirmed absent.
