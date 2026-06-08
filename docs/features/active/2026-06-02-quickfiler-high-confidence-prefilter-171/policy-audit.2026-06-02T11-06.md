# Policy Audit — quickfiler-high-confidence-prefilter (Issue #171)

- Component: QuickFiler high-confidence pre-UI scoring/filter pass
- Date: 2026-06-02T11-06
- Reviewer: feature-reviewer agent
- Review type: RE-AUDIT following remediation (supersedes `policy-audit.2026-06-02T10-36.md`)
- Work Mode: full-bug (from `issue.md`)
- Base branch (resolved): `development` @ `5e944344041b10becb98c56d358176fc9e7b8ee9`
- Head: `bug/quickfiler-high-confidence-prefilter-171` @ `9ddaa32e750be3ef29c9103cb8b7852b8ea6a9e7`
- Diff range: `5e944344041b10becb98c56d358176fc9e7b8ee9..9ddaa32e750be3ef29c9103cb8b7852b8ea6a9e7`
- AC source (full-bug): `spec.md` Definition of Done; the workflow input also names `user-story.md` AC1-AC8. Both were evaluated.

## Executive Summary

This is a re-audit of Issue #171 after remediation of the single blocking finding from the prior round (R1: the mandatory canonical C# coverage artifact was absent). The remediation generated the canonical Cobertura coverage report at `artifacts/csharp/coverage.xml`, recorded machine-readable coverage verification under `evidence/coverage/`, and restored `TaskMaster/TaskMaster.csproj` to its base-branch form.

The reviewer independently parsed the now-present canonical coverage artifact and confirms:
- New file `QfcHighConfidencePreFilter.cs`: 100.00% line coverage (54/54 instrumented lines) — meets the >= 90% new-file gate.
- `FolderScorer.cs`: 90.98% (726/798) — the reused scoring helper remains well covered.
- Modified controllers remain at their pre-existing COM/WinForms-bound levels with no changed-line regression (`QfcHomeController` 53.54%, `QfcFormController` 39.19%, `QfcItemController` 7.54%, `QfcItemGroup` 81.82%, `QfcCollectionController` 3.53%).
- The application module actually exercised by testable logic, `UtilitiesCS`, is at 87.45% (>= 80% floor).

The blocking coverage-artifact gap is resolved. R2 (coverage verification) and R3 (csproj revert) are resolved. No blocking findings remain.

Overall verdict: PASS. See Section 10.

## Rejected Scope Narrowing

No caller instruction attempted to narrow scope to a plan subset, a file subset, or to mark any language out of scope. The supplied input explicitly directed a full branch-diff audit ("Determine scope yourself per the workflow's scope invariant; do not narrow scope"), consistent with the workflow scope invariant. None rejected.

## Evidence Location Compliance

All Issue #171 implementation and remediation evidence is written under the canonical `<FEATURE>/evidence/<kind>/` subtree:
- `evidence/baseline/` (policy-read, inputs-read, csharpier, analyzers, nullable, tests, file-line-counts, csproj-diff-before, remediation baselines)
- `evidence/coverage/` (baseline, final, comparison, prefilter, and the remediation `remediation-*` coverage/parse/convert/module/changed-line files)
- `evidence/qa/` (qa-final, file-size-check, csproj-diff-after, remediation-qa-final, remediation-reaudit-confirmation, remediation-csharpier-after)
- `evidence/regression/`

The canonical machine-readable C# coverage artifact resides at `artifacts/csharp/coverage.xml`. This is the designated language coverage-artifact path per the workflow Coverage Artifact table; it is not an evidence-location violation (evidence-location rules govern `<FEATURE>/evidence/<kind>/` artifacts the reviewer or executor produces, while coverage artifacts have a dedicated per-language canonical path).

Branch-diff scan for prohibited non-canonical evidence paths (`artifacts/baselines/`, `artifacts/qa/`, `artifacts/evidence/`, `artifacts/coverage/`): none of these paths appear among the files added or modified in the range. Verified via `git diff --name-only <range> | grep -E "artifacts/(baselines|qa|evidence|coverage)/"` which returned no matches.

Note: `validate_evidence_locations.py` is not present in this repository; the equivalent scan was performed against `git diff --name-only` output for the resolved range. The repository hook `.claude/hooks/enforce-evidence-locations.ps1` enforces this at write time.

Verdict: PASS.

## 1. General Unit Test Policy Compliance

- Independence / isolation: PASS. New tests use per-test Moq mocks and reference-identity keys; no shared mutable state across tests.
- Determinism: PASS for Issue #171 tests. The new tests mock the scoring seam (`IFolderScoringService`) and do not touch COM, network, filesystem, or clocks. The pre-existing process-wide-state contention in `OlTableExtensions_Tests` (redirecting `Console.Out`) was mitigated in this branch with `[DoNotParallelize]`.
- No temp files / external deps: PASS. No temporary files; no live Outlook COM in unit tests.
- Scenario completeness for the new filter: PASS. Positive (above threshold), negative (below threshold, zero-score), boundary (exactly at cutoff), null/empty input, all-below, order preservation, and cancellation are covered (`QfcHighConfidencePreFilterTests`).
- AAA + intent documentation: PASS. Tests follow Arrange/Act/Assert and carry XML-doc/comment intent.

Verdict: PASS.

## 2. General Code Change Policy Compliance

- Simplicity / separation of concerns: PASS. New scoring/filter logic placed in a dedicated file rather than inflating the oversized controllers; pure filter logic separated from the COM-bound `FolderScoringService` adapter.
- Reusability / DRY: PASS. Scoring reuses the existing `FolderPredictor`/`FolderScorer` path through the seam rather than duplicating the Bayesian body. (One near-duplication of the carrier `LoadControlsAndHandlers_01Async` overload body is noted in the code review as a low maintainability item, not a policy violation.)
- Error handling: PASS. `FilterAsync` and `FolderScoringService.ScoreAsync` honor cancellation via `ThrowIfCancellationRequested`; `QfcPreScoredItem` coerces a null folder to `string.Empty` to keep its non-null contract.
- File size limit (500 lines): PARTIAL (pre-existing, not introduced here). The four touched controllers were already over 500 lines before this branch and remain so (`QfcCollectionController.cs` 2297, `QfcItemController.cs` 2498, `QfcFormController.cs` 1142, `QfcHomeController.cs` 759). The bulk of new logic (182 lines) lives in the new compliant file `QfcHighConfidencePreFilter.cs`. The violation is pre-existing and not materially worsened; spec and remediation scope explicitly forbid refactoring these controllers.
- csproj reformat (prior R3): RESOLVED. `git diff development -- TaskMaster/TaskMaster.csproj` shows no content diff; the project file matches the base-branch form and the trailing newline is restored.

Verdict: PASS (with the pre-existing file-size note carried forward, not introduced here).

## 3. Language-Specific Code Change Policy Compliance (C#)

- Strong contracts / explicit APIs: PASS. New public/internal members carry XML docs; `QfcPreScoredItem` is an immutable `readonly struct` (positional record struct avoided due to .NET Framework 4.8 lacking `IsExternalInit`; documented).
- Null-safety: PASS. Guard clauses on null/empty inputs; non-null folder contract enforced.
- Composition / focused types: PASS. Interface seam (`IFolderScoringService`) plus a narrow injectable delegate (`HighConfidencePreFilterLoader`) follow the repo DI-seam preference order.
- Async / resource safety: PASS. Scoring runs off the UI thread (`Task.Run` in `RunAsync`); `Task.WhenAll` parallelizes per-item scoring; UI construction stays on the UI thread.
- `[ExcludeFromCodeCoverage]` on `FolderScoringService`: intact and not relaxed (verified at `QfcHighConfidencePreFilter.cs` line 157). The adapter is COM-bound and not unit-testable without live Outlook; the exclusion narrows the measured surface to the testable filter logic, a legitimate boundary exclusion.
- Formatting (CSharpier): PASS for touched `.cs` files per `evidence/qa/qa-final-171.2026-06-02T10-26.md` and `evidence/qa/remediation-csharpier-after-171.2026-06-02T10-36.txt`. The prior `TaskMaster.csproj` churn is reverted.

Verdict: PASS.

## 4. Language-Specific Unit Test Policy Compliance (C#)

- Framework: PASS. MSTest (`[TestClass]`/`[TestMethod]`).
- Mocking: PASS. Moq, including a strict mock for the scoring seam.
- Assertions: PASS. FluentAssertions used throughout the new tests.
- New module coverage target (>= 90%): PASS. Verified from the canonical artifact: `QfcHighConfidencePreFilter.cs` = 100.00% (54/54 instrumented lines). The reused `FolderScorer.cs` is at 90.98%.

Verdict: PASS.

## 5. Test Coverage Detail

Languages with changed files in the branch diff: **C# only**. The diff contains `.cs` (15), `.csproj` (3), `.xml` (1, `RibbonExplorer.xml`), `.md` (26 docs/evidence), and `.txt` (18 evidence). No `.ts/.tsx`, `.py`, `.ps1/.psm1` files changed.

| Language | Changed files | Canonical artifact | Artifact present | Coverage verdict |
|---|---|---|---|---|
| C# (csharp / .NET) | yes (9 prod incl. RibbonExplorer.xml-adjacent, 6 test, 3 csproj) | `artifacts/csharp/coverage.xml` | YES | **PASS** |
| TypeScript | none | `coverage/lcov.info` | n/a | N/A (zero changed files) |
| Python | none | `artifacts/python/lcov.info` | n/a | N/A (zero changed files) |
| PowerShell | none | `artifacts/pester/powershell-coverage.xml` | n/a | N/A (zero changed files) |

C# coverage verdict: **PASS**. The mandatory canonical C# coverage artifact `artifacts/csharp/coverage.xml` now exists, is Cobertura XML, parses, and carries per-line `hits` counters. The reviewer parsed it directly (model: verify existing artifact, no re-run).

Reviewer-parsed per-file line coverage from the canonical artifact (distinct instrumented lines across class nodes):

| File | Covered/Total | Line coverage | Gate |
|---|---|---|---|
| `QfcHighConfidencePreFilter.cs` (new) | 54/54 | 100.00% | new-file >= 90% — PASS |
| `FolderScorer.cs` (reused scorer) | 726/798 | 90.98% | >= 80% — PASS |
| `QfcHomeController.cs` (modified) | 484/904 | 53.54% | no changed-line regression — PASS (note A) |
| `QfcFormController.cs` (modified) | 602/1536 | 39.19% | no changed-line regression — PASS (note A) |
| `QfcItemGroup.cs` (modified) | 18/22 | 81.82% | no regression (rose from 53.85%) — PASS |
| `QfcItemController.cs` (modified) | 246/3261 | 7.54% | no changed-line regression — PASS (note A) |
| `QfcCollectionController.cs` (modified) | 110/3118 | 3.53% | no changed-line regression — PASS (note A) |

C# repo-wide coverage (whole-artifact Cobertura `line-rate`): 57.99% (lines-covered 96055 / lines-valid 165651).

Application-module coverage exercised by the two in-scope test assemblies (`QuickFiler.Test`, `UtilitiesCS.Test`):
- `UtilitiesCS`: 87.45% (>= 80% floor — PASS).
- `QuickFiler`: 25.31% (improved +1.20 vs baseline 24.11%; below floor — pre-existing COM/WinForms condition, see note A).

Note A — modified controllers and repo-wide floor (pre-existing condition, no regression):
The modified QuickFiler controllers are oversized, COM/WinForms-bound files whose UI and Outlook-COM paths are not unit-testable without live Outlook, which repo policy prohibits. These files were at ~3-7%-to-53% at baseline and remain at the same levels. The reviewer independently confirmed, against the canonical artifact and the baseline, that:
- No changed line that was covered at baseline became uncovered.
- Every uncovered changed line is a pre-existing COM/WinForms boundary (live `MailItem` interaction, WinForms form/control display, or the production live-scoring seam default lambda).
- The new testable selection logic (`PopulateAndSelectFolder`), the `PredeterminedFolder` carry property, and the `RunAsync` high-confidence branch are covered by the new tests.
The sub-80% C# repo-wide aggregate is a documented pre-existing condition driven by out-of-scope modules (`TaskMaster` 6.60%, `ToDoModel` 0.00%, `Tags` 0.00%) and third-party/vendored assemblies (FSharp.Core, Deedle, log4net, System.Linq.Async, System.Interactive), none of which are introduced or changed by Issue #171. Issue #171 raised QuickFiler module coverage (+1.20) and lowered no module.

Per the General Unit Test Policy, the governing change-scope gates are: >= 90% for new modules (met: 100%), and no coverage reduction on changed lines (met: no changed-line regression). Both are satisfied. The absolute floor on the pre-existing COM/WinForms controllers and the whole-repo aggregate is a documented pre-existing condition, not a regression introduced by this branch.

Evidence: reviewer parse of `artifacts/csharp/coverage.xml`; `evidence/coverage/remediation-module-coverage-171.2026-06-02T10-36.md`; `evidence/coverage/remediation-changed-line-verification-171.2026-06-02T10-36.md`; `evidence/coverage/coverage-comparison-171.2026-06-02T10-26.md`; `evidence/qa/remediation-reaudit-confirmation-171.2026-06-02T10-36.md`.

Verdict: PASS (canonical C# coverage artifact present; new-file and changed-line gates met; pre-existing-condition justification documented and independently verified for the COM/WinForms floor).

## 6. Test Execution Metrics

From `evidence/qa/qa-final-171.2026-06-02T10-26.md` and the remediation QA evidence (vstest over `QuickFiler.Test.dll` + `UtilitiesCS.Test.dll`, `/EnableCodeCoverage`):
- Total ~3943, all 18 Issue #171 tests pass.
- Remaining failures are the pre-existing timing-flaky `UtilitiesCS.Test` timer/serialization tests that also failed at baseline and pass in isolation; not Issue #171 regressions.
- New-test additions are consistent with the reported passed-count increase from baseline.

Note: these metrics are taken from feature-folder evidence (review model is evidence verification, not re-execution). The pre-existing flaky failures are unrelated to Issue #171 logic.

Verdict: PASS for Issue #171 tests (the residual flaky failures are pre-existing and unrelated; no Issue #171 regression).

## 7. Code Quality Checks

- CSharpier (touched files): PASS per qa-final and remediation csharpier-after evidence.
- CSharpier (`TaskMaster.csproj`): RESOLVED. The prior reformat churn is reverted; `git diff development -- TaskMaster/TaskMaster.csproj` shows no content diff and the trailing newline is restored.
- Analyzers (msbuild): PASS — 0 errors; pre-existing warnings only, none from Issue #171 files (per evidence).
- Nullable (msbuild): PASS for Issue #171 files — 0 nullable errors in QuickFiler/UtilitiesCS/test projects; pre-existing errors confined to vendored projects, equal to baseline (non-regression).

Verdict: PASS.

## 8. Gaps and Exceptions

1. Canonical C# coverage artifact `artifacts/csharp/coverage.xml` — RESOLVED (present, parses; reviewer-verified). (Prior round R1, BLOCKING.)
2. Changed-line / repo-wide coverage — RESOLVED. No changed-line regression; new file 100%; UtilitiesCS application module 87.45%; QuickFiler module sub-floor is a documented pre-existing COM/WinForms condition. (Prior round R2.)
3. `TaskMaster.csproj` reformat — RESOLVED. File restored to base-branch form; trailing newline present. (Prior round R3.)
4. Pre-existing oversized controller files (>500 lines) — carried forward (pre-existing; out of scope per spec and remediation constraints; not materially worsened).
5. Carrier `LoadControlsAndHandlers_01Async` overload near-duplicates its sibling body (~85 lines) — low maintainability item recorded in the code review; not a policy violation.

## 9. Summary of Changes

C# production (8 + 1 ribbon XML): `QfcHighConfidencePreFilter.cs` (new, +182), `QfcCollectionController.cs` (+97/-... glue), `QfcItemController.cs` (+73 glue), `QfcFormController.cs` (+60), `QfcHomeController.cs` (+37 glue), `QfcItemGroup.cs` (+6), `IQfcCollectionController.cs` (+5), `IQfcFormController.cs` (+2), `RibbonExplorer.xml` (+12/-... edit-box relocation).

C# tests (6 + 1): `QfcHighConfidencePreFilterTests.cs` (new, +334), `QfcHomeControllerTests.cs` (+272), `RibbonExplorerXmlTests.cs` (new, +97), `QfcItemControllerTests.cs` (+62), `QfcFormControllerTests.cs` (+42), `QfcCollectionControllerTests.cs` (+38), `OlTableExtensions_Tests.cs` (+4, `[DoNotParallelize]`).

Build/config (3 csproj): `QuickFiler.csproj` (+1 compile include), `QuickFiler.Test.csproj` (+1), `TaskMaster.Test.csproj` (+1). `TaskMaster.csproj` no longer differs from base (R3 revert).

Docs/evidence/agent-memory: feature scoping docs, prior-round audit artifacts, the `evidence/` subtree (including remediation coverage evidence), and `.claude/agent-memory/` entries.

The canonical coverage artifact `artifacts/csharp/coverage.xml` is generated build output and is not part of the source change set in the conventional sense; it is the verification artifact for the C# coverage gate.

## 10. Compliance Verdict

**PASS.**

Blocking findings: 0

The single prior-round blocking finding (absent canonical C# coverage artifact) is resolved; the artifact exists, parses, and the reviewer independently verified the new-file (100%) and no-changed-line-regression gates from it. R2 and R3 are resolved. The sub-80% C# repo-wide aggregate and the low coverage on the modified COM/WinForms controllers are documented pre-existing conditions with no regression introduced by Issue #171, consistent with the General Unit Test Policy change-scope gates (>= 90% new module, no reduction on changed lines).

Acceptance criteria AC1-AC8 are implemented and supported by code, tests, and now-verifiable coverage (see `feature-audit.2026-06-02T11-06.md`). Recommendation: go for PR.

## Appendix A: Test Inventory

New / changed Issue #171 tests (verified by reading the diff and test sources):
- `QfcHighConfidencePreFilterTests`: `FilterAsync_WithSingleAboveThresholdItem_ReturnsThatItem`, `FilterAsync_ExcludesItemsBelowCutoff`, `FilterAsync_ExcludesZeroScoreNoSuggestion`, `FilterAsync_RetainsItemExactlyAtCutoff`, `FilterAsync_SurvivorsCarryPredeterminedTopFolder`, `FilterAsync_NullItems_ReturnsEmpty`, `FilterAsync_EmptyItems_ReturnsEmpty`, `FilterAsync_AllBelowThreshold_ReturnsEmpty`, `FilterAsync_HonorsCancellation` (plus mixed-batch order preservation).
- `QfcHomeControllerTests`: `HighConfidencePreFilterLoader_CanBeOverridden_ForTesting`, `RunAsync_HighConfidenceEnabled_InvokesPreFilterBeforeCarrierLoad`, `RunAsync_HighConfidence_PreFilterPrecedesUiConstruction`, `RunAsync_HighConfidenceDisabled_DoesNotPreFilterUsesPlainOverload`, `RunAsync_HighConfidenceDisabled_UsesPlainOverloadOnly`.
- `QfcFormControllerTests`: `LoadItemsAsync_PreScored_DoesNotInvokePostUiRemoval`.
- `QfcCollectionControllerTests`: `CarrierLoad_SetsPredeterminedFolderOnItemGroup`.
- `QfcItemControllerTests`: `AssignFolderComboBox_WithPredeterminedFolder_SelectsThatFolderNotIndexOne`, `AssignFolderComboBox_WithoutPredeterminedFolder_SelectsIndexOne`.
- `RibbonExplorerXmlTests` (new): ribbon XML structure tests covering the relocated `HighConfidenceThreshold` edit box.

## Appendix B: Toolchain Commands Reference

Commands recorded in feature/remediation evidence (review is evidence verification; the reviewer additionally parsed the canonical coverage artifact directly):
1. Format: `dotnet tool run csharpier check <touched .cs>` (CSharpier 1.2.6)
2. Analyzers: `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
3. Nullable: `msbuild TaskMaster.sln /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true`
4. Test + coverage: `vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /EnableCodeCoverage`
5. Coverage verification (reviewer): parsed canonical `artifacts/csharp/coverage.xml` (Cobertura) for repo-wide `line-rate` and per-file `hits` counters.
