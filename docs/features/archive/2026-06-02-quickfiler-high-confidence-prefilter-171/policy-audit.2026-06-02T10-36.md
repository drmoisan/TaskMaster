# Policy Audit — quickfiler-high-confidence-prefilter (Issue #171)

- Component: QuickFiler high-confidence pre-UI scoring/filter pass
- Date: 2026-06-02T10-36
- Reviewer: feature-reviewer agent
- Work Mode: full-bug (from `issue.md`)
- Base branch (resolved): `development` @ `5e944344041b10becb98c56d358176fc9e7b8ee9`
- Head: `bug/quickfiler-high-confidence-prefilter-171` @ `ae7eb670ee7738640cab2b41bc7226255224f7ca`
- Diff range: `5e944344041b10becb98c56d358176fc9e7b8ee9..ae7eb670ee7738640cab2b41bc7226255224f7ca`
- AC source (full-bug): `spec.md` Definition of Done (the workflow input also names `user-story.md` AC1-AC8; both were evaluated)

## Executive Summary

The branch implements a pre-UI scoring/filter pass for QuickFiler high-confidence mode (Issue #171). The new logic is isolated in `QuickFiler/Controllers/QfcHighConfidencePreFilter.cs` and is exercised by focused MSTest + Moq + FluentAssertions tests. The design reuses the existing `FolderPredictor`/`FolderScorer` scoring path via a DI seam (`IFolderScoringService`) and threads a predetermined folder through the carrier-list load path. The implemented behavior aligns with the acceptance criteria as written.

Two findings prevent an unconditional PASS verdict:

1. The mandatory canonical C# coverage artifact `artifacts/csharp/coverage.xml` is absent. Coverage is documented only as human-readable text/markdown in the feature `evidence/coverage/` subtree, not in the machine-readable artifact path the workflow mandates. Per the workflow, an absent coverage artifact for a language with changed files is a FAIL and a remediation trigger.
2. The PR context summary supplied as input misclassified the change set ("Core logic changes: 0 files"; all C# changes labeled docs). The feature-review agent corrected the `Changed files overview` section of `artifacts/pr_context.summary.txt` to reflect the actual `git diff` for the resolved range. This did not change scope; it corrected stale evidence.

Overall verdict: PARTIAL (remediation required). See Section 10.

## Rejected Scope Narrowing

No caller instruction attempted to narrow scope to a plan subset, a file subset, or to mark any language out of scope. The supplied input explicitly directed a full branch-diff audit and "do not narrow scope," which is consistent with the workflow scope invariant. None rejected.

Note on stale evidence (not a narrowing): the input PR context summary's automated classification under-reported the C# change set. This is recorded as a stale-evidence correction (Section 2 / Section 9), not a scope narrowing.

## Evidence Location Compliance

All Issue #171 implementation evidence is written under the canonical `<FEATURE>/evidence/<kind>/` subtree:
- `evidence/baseline/` (policy-read, inputs-read, csharpier, analyzers, nullable, tests, file-line-counts)
- `evidence/coverage/` (baseline, final, comparison, prefilter)
- `evidence/qa/` (qa-final, file-size-check)
- `evidence/regression/`

Branch-diff scan for prohibited non-canonical evidence paths (`artifacts/baselines/`, `artifacts/qa/`, `artifacts/evidence/`, `artifacts/coverage/`): none of these paths appear among the files added or modified in the range. No evidence-location violation found in the change set.

Note: `validate_evidence_locations.py --root .` is not present in this repository; the equivalent scan was performed against the `git diff --name-status` output for the resolved range. The repository hook `.claude/hooks/enforce-evidence-locations.ps1` enforces this at write time.

Verdict: PASS.

## 1. General Unit Test Policy Compliance

- Independence / isolation: PASS. New tests use per-test Moq mocks and reference-identity keys; no shared mutable state across tests.
- Determinism: PASS for Issue #171 tests. The new tests mock the scoring seam (`IFolderScoringService`) and do not touch COM, network, filesystem, or clocks. One pre-existing process-wide-state flakiness (`OlTableExtensions_Tests` redirecting `Console.Out`) was mitigated in this branch by adding `[DoNotParallelize]` (a correct, in-scope-adjacent fix).
- No temp files / external deps: PASS. No temporary files; no live Outlook COM in unit tests.
- Scenario completeness for the new filter: PASS. Positive (above threshold), negative (below threshold, zero-score), boundary (exactly-at-cutoff), null/empty input, all-below, order preservation, and cancellation are covered (`QfcHighConfidencePreFilterTests`, 11 methods).
- AAA + intent documentation: PASS. Tests follow Arrange/Act/Assert and carry XML-doc/comment intent.

Verdict: PASS.

## 2. General Code Change Policy Compliance

- Simplicity / separation of concerns: PASS. New scoring/filter logic placed in a dedicated file rather than inflating the oversized controllers; pure filter logic separated from the COM-bound `FolderScoringService` adapter.
- Reusability / DRY: PASS. Scoring reuses the existing `FolderPredictor`/`FolderScorer` path through the seam rather than duplicating the Bayesian body.
- Error handling: PASS. `FilterAsync` and `FolderScoringService.ScoreAsync` honor cancellation via `ThrowIfCancellationRequested`; `QfcPreScoredItem` coerces a null folder to `string.Empty` to keep its non-null contract.
- File size limit (500 lines): PARTIAL (pre-existing). The four touched controllers were already over 500 lines before this branch (2206/2431/1082/724) and remain so (+91/+67/+60/+35). The additions are minimal glue and the bulk of new logic (182 lines) lives in the new compliant file. The limit violation is pre-existing and not materially worsened; not introduced by this change.
- Stale evidence correction: the input PR context summary mislabeled the change set; corrected in `artifacts/pr_context.summary.txt` (see Section 9). Source code and policy documents were not modified by the reviewer.

Verdict: PASS (with the pre-existing file-size note carried forward, not introduced here).

## 3. Language-Specific Code Change Policy Compliance (C#)

- Strong contracts / explicit APIs: PASS. New public/internal members carry XML docs; `QfcPreScoredItem` is an immutable `readonly struct` (positional record struct avoided due to .NET Framework 4.8 lacking `IsExternalInit`; documented).
- Null-safety: PASS. Guard clauses on null/empty inputs; non-null folder contract enforced.
- Composition / focused types: PASS. Interface seam (`IFolderScoringService`) plus a narrow injectable delegate (`HighConfidencePreFilterLoader`) follow the repo DI-seam preference order.
- Async / resource safety: PASS. Scoring runs off the UI thread (`Task.Run` in `RunAsync`); `Task.WhenAll` parallelizes per-item scoring; UI construction stays on the UI thread.
- `[ExcludeFromCodeCoverage]` on `FolderScoringService`: acceptable — the adapter is COM-bound and not unit-testable without live Outlook, and it is documented. This narrows the measured surface to the testable filter logic, which is a legitimate boundary exclusion.
- Formatting (CSharpier): PASS for touched `.cs` files per `evidence/qa/qa-final-171.2026-06-02T10-26.md` (CSharpier 1.2.6, "CLEAN" on touched files). A pre-existing CSharpier error exists in `TaskMaster.csproj`; note that `TaskMaster.csproj` IS in this diff (whitespace/formatting churn). See Section 7.

Verdict: PASS.

## 4. Language-Specific Unit Test Policy Compliance (C#)

- Framework: PASS. MSTest (`[TestClass]`/`[TestMethod]`).
- Mocking: PASS. Moq, including a strict mock for the scoring seam.
- Assertions: PASS. FluentAssertions used throughout the new tests.
- New module coverage target (>= 90%): the documented evidence reports `QfcHighConfidencePreFilter.cs` at 100% of its testable surface. However this is asserted only in feature-folder text evidence; the canonical machine-readable artifact required to verify it independently is absent (see Section 5). Verdict deferred to Section 5.

Verdict: PARTIAL (verification artifact absent; see Section 5).

## 5. Test Coverage Detail

Languages with changed files in the branch diff: **C# only** (`.cs` production and test files, plus `.csproj` and `RibbonExplorer.xml`). No `.ts/.tsx`, `.py`, `.ps1/.psm1` files changed.

| Language | Changed files | Canonical artifact | Artifact present | Coverage verdict |
|---|---|---|---|---|
| C# (csharp / .NET) | yes (9 prod, 6 test, 4 csproj) | `artifacts/csharp/coverage.xml` | **NO** | **FAIL** |
| TypeScript | none | `coverage/lcov.info` | n/a | N/A (zero changed files) |
| Python | none | `artifacts/python/lcov.info` | n/a | N/A (zero changed files) |
| PowerShell | none | `artifacts/pester/powershell-coverage.xml` | n/a | N/A (zero changed files) |

C# coverage verdict: **FAIL**. Reason: the mandatory canonical C# coverage artifact `artifacts/csharp/coverage.xml` is absent. Coverage verification is mandatory for every language with changed files and must be performed against the canonical artifact. The agent does not re-run coverage generation; it verifies from existing artifacts, and the required artifact does not exist.

Documented (non-canonical) coverage evidence for context (from `evidence/coverage/coverage-comparison-171.2026-06-02T10-26.md` and `coverage-final-171.2026-06-02T10-26.txt`), which cannot substitute for the canonical artifact but is recorded here:
- New file `QfcHighConfidencePreFilter.cs`: reported 100.00% (testable surface) — would meet the >= 90% new-file gate if verifiable from the canonical artifact.
- Modified files (range-line basis): `QfcHomeController.cs` 50.51% -> 52.22%; `QfcFormController.cs` 39.64% -> 39.73%; `QfcItemController.cs` 7.02% -> 7.29%; `QfcItemGroup.cs` 53.85% -> 84.62%; `QfcCollectionController.cs` 3.81% -> 3.65%.
- Reported per-module line coverage: `QuickFiler.dll` 24.32%; `UtilitiesCS.dll` 87.58%.

Repo-wide / per-module concern (independent of the missing-artifact FAIL): the reported `QuickFiler.dll` module line coverage (24.32%) is far below the 80% repo-wide floor. The coverage-comparison evidence argues this is a pre-existing condition (the touched controllers are COM/WinForms-bound and ~3-7% covered at baseline, unchanged by this branch). That argument is plausible for non-regression on changed lines, but the absolute module figure remains below the documented 80% per-language repo-wide floor. This is recorded as a contributing FAIL factor pending verification from the canonical artifact.

Verdict: FAIL (canonical C# coverage artifact absent; mandatory verification cannot be completed).

## 6. Test Execution Metrics

From `evidence/qa/qa-final-171.2026-06-02T10-26.md` (vstest over `QuickFiler.Test.dll` + `UtilitiesCS.Test.dll`, `/EnableCodeCoverage`):
- Total 3943, Passed 3935, Failed 8.
- All 18 Issue #171 tests pass.
- The 8 failures are pre-existing timing-flaky `UtilitiesCS.Test` timer/serialization tests that also failed at baseline and pass in isolation; not Issue #171 regressions.
- Passed count rose +19 from baseline (3916 -> 3935), consistent with the new tests.

Note: these metrics are taken from feature-folder evidence (timestamp 10-26). They were not independently re-executed by the reviewer (the review model is evidence verification, not re-execution). The reported 8 pre-existing failures should be confirmed against the canonical run output during remediation.

Verdict: PARTIAL (8 failing tests, asserted pre-existing/flaky but not independently re-verified by the reviewer).

## 7. Code Quality Checks

- CSharpier (touched files): PASS per qa-final evidence.
- CSharpier (`TaskMaster.csproj`): the branch reformats `TaskMaster.csproj` (large whitespace churn collapsing multi-line attributes onto single lines, and the file now ends with no trailing newline). The qa-final evidence states a pre-existing CSharpier error exists in `TaskMaster.csproj` and that it is "not in the Issue #171 change set" — but the diff shows `TaskMaster.csproj` IS modified in this range. This is a low-severity inconsistency: a project-file reformat that is unrelated to the feature behavior and conflicts with the C# policy guidance to avoid using formatters that rewrite `.csproj` files. Recorded as a code-review finding (low). Not a build blocker per the evidence (analyzers/nullable builds succeeded), but it should be reverted or justified.
- Analyzers (msbuild): PASS — 0 errors, 61 pre-existing warnings, zero from Issue #171 files (per evidence).
- Nullable (msbuild): PASS for Issue #171 files — 0 nullable errors in QuickFiler/UtilitiesCS/test projects; 84 pre-existing errors confined to vendored projects (`SVGControl`, `UtilitiesSwordfish.NET.General`), equal to baseline (non-regression).

Verdict: PARTIAL (unrelated `TaskMaster.csproj` reformat; analyzers/nullable pass).

## 8. Gaps and Exceptions

1. Canonical C# coverage artifact `artifacts/csharp/coverage.xml` absent — blocks mandatory coverage verification (FAIL).
2. `QuickFiler.dll` module line coverage (24.32%) below the 80% per-language repo-wide floor; argued pre-existing but unverified against the canonical artifact (contributing FAIL).
3. `TaskMaster.csproj` reformatted by a tool that rewrites project files, contrary to C# policy guidance; file ends without a trailing newline (low).
4. Pre-existing oversized controller files (>500 lines) not introduced here but carried forward (pre-existing).
5. 8 failing tests asserted pre-existing/flaky; not independently re-verified by the reviewer (PARTIAL).
6. Input PR context summary was stale/misclassified; corrected by the reviewer (resolved).

## 9. Summary of Changes

C# production (9): `QfcHighConfidencePreFilter.cs` (new, +182), `QfcCollectionController.cs` (+92/-1), `QfcItemController.cs` (+71/-2), `QfcFormController.cs` (+60), `QfcHomeController.cs` (+36/-1), `QfcItemGroup.cs` (+6), `IQfcCollectionController.cs` (+5), `IQfcFormController.cs` (+2), `RibbonExplorer.xml` (+6/-6, edit-box relocation).

C# tests (6 + 1): `QfcHighConfidencePreFilterTests.cs` (new, +334), `QfcHomeControllerTests.cs` (+272), `RibbonExplorerXmlTests.cs` (new, +97), `QfcItemControllerTests.cs` (+62), `QfcFormControllerTests.cs` (+42), `QfcCollectionControllerTests.cs` (+38), `OlTableExtensions_Tests.cs` (+4, `[DoNotParallelize]`).

Build/config (4): `TaskMaster.csproj` (+15/-60 reformat), `QuickFiler.csproj` (+1 compile include), `QuickFiler.Test.csproj` (+1), `TaskMaster.Test.csproj` (+1).

Docs/evidence/agent-memory (21): feature scoping docs, `evidence/` subtree, and `.claude/agent-memory/atomic-executor/` entries.

Reviewer action: corrected the inaccurate `Changed files overview` section of `artifacts/pr_context.summary.txt` to match the actual range diff. No source code or policy documents were modified by the reviewer.

## 10. Compliance Verdict

**PARTIAL — remediation required.**

Blocking findings: 1
- Mandatory canonical C# coverage artifact `artifacts/csharp/coverage.xml` absent (Section 5). This blocks completion of coverage verification for a language with changed files and triggers remediation per the workflow.

Contributing / non-blocking findings: `QuickFiler.dll` module coverage below floor (unverified against canonical artifact); `TaskMaster.csproj` reformat (low); pre-existing oversized controllers; 8 flaky pre-existing test failures (re-verify).

Acceptance criteria themselves (AC1-AC8) are implemented and supported by code and tests as written (see `feature-audit.2026-06-02T10-36.md`). The remediation requirement is driven by the coverage-artifact gap, not by missing feature behavior.

## Appendix A: Test Inventory

New / changed Issue #171 tests verified by reading the diff and test sources:
- `QfcHighConfidencePreFilterTests`: `FilterAsync_WithSingleAboveThresholdItem_ReturnsThatItem`, `FilterAsync_ExcludesItemsBelowCutoff`, `FilterAsync_ExcludesZeroScoreNoSuggestion`, `FilterAsync_RetainsItemExactlyAtCutoff`, `FilterAsync_SurvivorsCarryPredeterminedTopFolder`, `FilterAsync_NullItems_ReturnsEmpty`, `FilterAsync_EmptyItems_ReturnsEmpty`, `FilterAsync_AllBelowThreshold_ReturnsEmpty`, `FilterAsync_HonorsCancellation` (plus order-preservation coverage via the mixed-batch test).
- `QfcHomeControllerTests`: `HighConfidencePreFilterLoader_CanBeOverridden_ForTesting`, `RunAsync_HighConfidenceEnabled_InvokesPreFilterBeforeCarrierLoad`, `RunAsync_HighConfidence_PreFilterPrecedesUiConstruction`, `RunAsync_HighConfidenceDisabled_DoesNotPreFilterUsesPlainOverload`, `RunAsync_HighConfidenceDisabled_UsesPlainOverloadOnly`.
- `QfcFormControllerTests`: `LoadItemsAsync_PreScored_DoesNotInvokePostUiRemoval`.
- `QfcCollectionControllerTests`: `CarrierLoad_SetsPredeterminedFolderOnItemGroup`.
- `QfcItemControllerTests`: `AssignFolderComboBox_WithPredeterminedFolder_SelectsThatFolderNotIndexOne`, `AssignFolderComboBox_WithoutPredeterminedFolder_SelectsIndexOne`.
- `RibbonExplorerXmlTests` (new): ribbon XML structure tests covering the relocated `HighConfidenceThreshold` edit box.

## Appendix B: Toolchain Commands Reference

Commands recorded in feature evidence (not re-executed by the reviewer; review is evidence verification):
1. Format: `dotnet tool run csharpier check <touched .cs>` (CSharpier 1.2.6)
2. Analyzers: `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
3. Nullable: `msbuild TaskMaster.sln /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true`
4. Test + coverage: `vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /EnableCodeCoverage`

Coverage verification command the reviewer expected to inspect (artifact absent): canonical `artifacts/csharp/coverage.xml`.
