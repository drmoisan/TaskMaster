# 2026-07-03-quickfiler-high-confidence-dequeue-streaming — Plan

- **Issue:** #233
- **Owner:** drmoisan
- **Last Updated:** 2026-07-03T17-53
- **Status:** Draft
- **Version:** 0.3
- **Work Mode:** full-feature

## Requirements Sources

- `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/issue.md`
- `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/spec.md`
- `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/user-story.md`
- `artifacts/research/2026-07-03T17-01-quickfiler-high-confidence-dequeue-streaming-233-research.md`

## Evidence Contract

All evidence produced by this plan must be written under `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/evidence/<kind>/`.

Required evidence artifact fields for command-bearing tasks:

- `Timestamp:`
- `Command:`
- `EXIT_CODE:`
- `Output Summary:`

Issue #232 prerequisite evidence from `docs/features/active/2026-07-03-quickfiler-navigation-key-collision-232/` must not be copied into the #233 feature folder. The executor must record new #233 evidence that describes the reconciliation result.

## Implementation Plan

### Phase 0 — Compliance and Baseline

- [x] [P0-T1] Read `AGENTS.md`, `.github/instructions/general-code-change.instructions.md`, `.github/instructions/general-unit-test.instructions.md`, `.github/instructions/csharp-code-change.instructions.md`, `.github/instructions/csharp-unit-test.instructions.md`, `.agents/skills/csharp/SKILL.md`, and `BannedSymbols.txt`; write `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/evidence/baseline/phase0-instructions-read.md` with `Timestamp:`, `Policy Order:`, and the exact file list read.
- [x] [P0-T2] Verify full-feature mode and AC1-AC12 by reading `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/issue.md`, `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/spec.md`, `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/user-story.md`, and `artifacts/research/2026-07-03T17-01-quickfiler-high-confidence-dequeue-streaming-233-research.md`; write `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/evidence/baseline/requirements-baseline.md` listing the AC source sections and confirming issue number `#233`.
- [x] [P0-T3] Capture repository and prerequisite drift state by running `git status --short --branch`, `git branch --contains 90e75ec1`, `git show --name-only --format='' 90e75ec1`, and `git merge-base --is-ancestor 90e75ec1 HEAD`; write `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/evidence/baseline/prerequisite-232-drift-baseline.md` with the commands, exit codes, and a file-scoped reconciliation list that includes only required #232 production/test changes and excludes #232 feature-folder evidence.
- [x] [P0-T4] Capture the CSharpier baseline by running `dotnet tool run csharpier .`; write `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/evidence/baseline/csharpier-baseline.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:`.
- [x] [P0-T5] Capture the analyzer baseline by running `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`; write `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/evidence/baseline/msbuild-analyzers-baseline.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:`.
- [x] [P0-T6] Capture the nullable baseline by running `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true`; write `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/evidence/baseline/msbuild-nullable-baseline.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:`.
- [x] [P0-T7] Capture the MSTest coverage baseline by running `vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /EnableCodeCoverage /ResultsDirectory:docs\features\active\2026-07-03-quickfiler-high-confidence-dequeue-streaming-233\evidence\baseline\vstest-results`; write `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/evidence/baseline/vstest-baseline.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, test counts, and numeric coverage values or a remediation-required statement if the coverage collector does not emit numeric coverage.
- [x] [P0-T8] Establish the coverage comparison baseline by parsing the Phase 0 VSTest coverage output or recording a blocked coverage-conversion finding; write `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/evidence/baseline/coverage-baseline.md` with repository coverage, touched-file coverage where available, and the exact parser or conversion command used.

### Phase 1 — Reconcile Issue 232 Navigation Prerequisite

- [x] [P1-T1] [expect-fail] Create or update automated regression tests in `QuickFiler.Test/Controllers/QfcCollectionControllerTests.cs` that fail before the #232 navigation reconciliation by proving `LoadControlsAndHandlers_01(TableLayoutPanel, List<QfcItemGroup>)` routes page swaps through the unregister/register path and `RemoveSpecificControlGroupAsync` does not double-register navigation; run the targeted navigation test command expecting failure and write schema-valid failing-run evidence to `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/evidence/regression-testing/issue-232-navigation.expect-fail.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:`.
- [x] [P1-T2] Port only the required #232 navigation production behavior from commit `90e75ec1` into `QuickFiler/Controllers/QfcCollectionController.cs`, specifically the `SwapItemGroups(itemGroups)` page-swap routing and double-registration guard; do not copy any files from `docs/features/active/2026-07-03-quickfiler-navigation-key-collision-232/`.
- [x] [P1-T3] Run the targeted navigation regression tests in `QuickFiler.Test/Controllers/QfcCollectionControllerTests.cs` with `vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /Tests:<navigation-test-names>`; write `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/evidence/regression-testing/issue-232-navigation.pass.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:`.

### Phase 2 — Reconcile Issue 232 Probability Logging Prerequisite

- [x] [P2-T1] [expect-fail] Create or update automated logging regression tests in `QuickFiler.Test/Controllers/QfcDatamodelTests.cs`, `QuickFiler.Test/Controllers/QfcHighConfidencePreFilterTests.cs`, and `QuickFiler.Test/Controllers/QfcItemController.FolderHandlingTests.cs` that fail before the #232 logging reconciliation by verifying item summary, score, and caller context are emitted at the three #232 scoring call sites; run the targeted logging test command expecting failure and write schema-valid failing-run evidence to `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/evidence/regression-testing/issue-232-logging.expect-fail.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:`.
- [x] [P2-T2] Port only the required #232 probability logging production behavior from commit `90e75ec1` into `QuickFiler/Controllers/QfcDatamodel.cs`, `QuickFiler/Controllers/QfcHighConfidencePreFilter.cs`, and `QuickFiler/Controllers/QfcItemController.FolderHandling.cs`; do not port #232 feature documents, #232 evidence, or unrelated memory files.
- [x] [P2-T3] Run the targeted logging regression tests in `QuickFiler.Test/Controllers/QfcDatamodelTests.cs`, `QuickFiler.Test/Controllers/QfcHighConfidencePreFilterTests.cs`, and `QuickFiler.Test/Controllers/QfcItemController.FolderHandlingTests.cs`; write `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/evidence/regression-testing/issue-232-logging.pass.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:`.

### Phase 3 — Build Dequeue-Time Streaming Gate

- [x] [P3-T1] [expect-fail] Create `QuickFiler.Test/Controllers/QfcStreamingDequeueConfidenceGateTests.cs` with MSTest/Moq/FluentAssertions coverage for AC2 dequeue-time score selection, AC3 scan-many-to-yield-few backfill, AC4 zero and partial source exhaustion, AC9 threshold inclusivity, cancellation propagation, and below-threshold discard; run `vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /Tests:QfcStreamingDequeueConfidenceGateTests` expecting failure and write schema-valid failing-run evidence to `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/evidence/regression-testing/streaming-gate.expect-fail.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:`.
- [x] [P3-T2] Implement `QuickFiler/Controllers/QfcStreamingDequeueConfidenceGate.cs` as the single testable dequeue-layer confidence gate that uses dequeue-time scoring, inclusive cutoff `score >= (long)Math.Round(threshold * 1000, 0)`, `CancellationToken.ThrowIfCancellationRequested`, `TimeProvider.Delay(..., token)` for waits, and an item summary/score/caller debug log line.
- [x] [P3-T3] Run `vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /Tests:QfcStreamingDequeueConfidenceGateTests`; write `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/evidence/regression-testing/streaming-gate.pass.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:`.

### Phase 4 — Move Live Gate Into Queue Dequeue

- [x] [P4-T1] Update automated tests in `QuickFiler.Test/Controllers/QfcDatamodelTests.cs`, `QuickFiler.Test/Controllers/QfcHomeControllerIterationTests.cs`, and `QuickFiler.Test/Controllers/QfcQueuePurePathsTests.cs` to assert AC2 dequeue-time scoring, AC3 backfill, AC4 source exhaustion, AC6 sparse qualifying full-page behavior, and AC7 disabled-mode parity at the datamodel/queue seam.
- [x] [P4-T2] Update `QuickFiler/Controllers/QfcRemainingQueueAdmission.cs` and `QuickFiler/Controllers/QfcDatamodel.cs` so remaining-mail admission no longer rejects candidates by high-confidence threshold before `_masterQueue` insertion, while preserving hook/remove behavior and null handling.
- [x] [P4-T3] Update `QuickFiler/Controllers/QfcDatamodel.QueueProcessing.cs` so `DequeueNextItemGroupAsync(int quantity, int timeOut)` uses `QfcStreamingDequeueConfidenceGate` when `HighConfidenceModeEnabled == true`, preserves the current direct `TryTakeFirst(quantity)` behavior when `HighConfidenceModeEnabled == false`, terminates when the source is exhausted, and preserves cancellation behavior.
- [x] [P4-T4] Run targeted queue/datamodel regression tests from `QuickFiler.Test/Controllers/QfcDatamodelTests.cs`, `QuickFiler.Test/Controllers/QfcHomeControllerIterationTests.cs`, and `QuickFiler.Test/Controllers/QfcQueuePurePathsTests.cs`; write `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/evidence/regression-testing/dequeue-integration.pass.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:`.

### Phase 5 — Remove Live Post-Display Confidence Removal

- [x] [P5-T1] Update automated tests in `QuickFiler.Test/Controllers/QfcHomeControllerIssue218Tests.cs`, `QuickFiler.Test/Controllers/QfcHomeControllerRunAsyncTests.cs`, and `QuickFiler.Test/Controllers/QfcFormControllerSeamTests.cs` to assert the first page uses the same dequeue-layer candidate path as later pages and that `ApplyHighConfidenceFilterAsync` / `RemoveBelowThresholdAsync` is not invoked for live confidence enforcement after items are surfaced.
- [x] [P5-T2] Update `QuickFiler/Controllers/QfcHomeController.cs` so the initial page is populated through the same dequeue-layer gate used by later pages when high-confidence mode is enabled, without loading an unfiltered fixed batch that is later trimmed.
- [x] [P5-T3] Update `QuickFiler/Controllers/QfcFormController.Actions.cs` and `QuickFiler/Interfaces/IQfcCollectionController.cs` so the live `LoadItemsAsync(IList<MailItem>, ProgressTracker)` path no longer applies post-display confidence threshold removal, and the interface documentation records that `RemoveBelowThresholdAsync(double threshold)` is not the live #233 enforcement gate.
- [x] [P5-T4] Run targeted first-page and no-post-display-removal tests from `QuickFiler.Test/Controllers/QfcHomeControllerIssue218Tests.cs`, `QuickFiler.Test/Controllers/QfcHomeControllerRunAsyncTests.cs`, and `QuickFiler.Test/Controllers/QfcFormControllerSeamTests.cs`; write `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/evidence/regression-testing/first-page-and-no-post-display-removal.pass.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:`.

### Phase 6 — Prove Single Gate and Pipeline Disposition

- [x] [P6-T1] Run a repo-wide confidence-gate search with `Select-String -Path (Get-ChildItem -Recurse -File -Include '*.cs' | Select-Object -ExpandProperty FullName) -Pattern 'HighConfidenceThreshold|RemoveBelowThreshold|ApplyHighConfidenceFilter|Math.Round\(.*threshold \* 1000|TopFolderScore'`; write `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/evidence/other/ac1-confidence-gate-search.md` showing exactly one live dequeue-layer threshold gate, excluding tests and the recorded dormant #171 disposition.
- [x] [P6-T2] Record the AC8 disposition for `QuickFiler/Controllers/QfcHighConfidencePreFilter.cs`, `QuickFiler/Controllers/QfcHomeController.cs`, and `QuickFiler/Controllers/QfcFormController.Actions.cs` in `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/evidence/other/ac8-dormant-171-disposition.md`, stating whether #171 remains dormant or is retired and why no third filtering pipeline exists.
- [x] [P6-T3] Run automated regression tests covering AC5 stable surfaced pages and AC12 ordinary non-high-confidence OK/Skip/pop-out, queue draining, `WaitForQueue` termination, and move-monitor hook/unhook behavior from `QuickFiler.Test/Controllers/QfcHomeControllerIterationTests.cs`, `QuickFiler.Test/Controllers/QfcItemController.NavigationTests.cs`, and `QuickFiler.Test/Controllers/QfcCollectionControllerTests.cs`; write `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/evidence/regression-testing/non-high-confidence-regression.pass.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:`.

### Phase 7 — Acceptance Criteria Tracking

- [x] [P7-T1] Update only satisfied AC checkboxes in `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/spec.md` by mapping AC1-AC12 to evidence files under `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/evidence/`; leave any AC unchecked if the supporting automated evidence is absent or remediation-required.
- [x] [P7-T2] Update only satisfied AC checkboxes in `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/user-story.md` by applying the same evidence mapping used for `spec.md`; leave any AC unchecked if the supporting automated evidence is absent or remediation-required.
- [x] [P7-T3] Write `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/evidence/issue-updates/issue-233.local-status.md` summarizing AC1-AC12 evidence status, prerequisite #232 reconciliation status, and any remediation-required state without posting to GitHub.

### Phase 8 — Final C# QA Loop

- [x] [P8-T1] Run CSharpier with `dotnet tool run csharpier .`; write `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/evidence/qa-gates/csharpier-final.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:`; if CSharpier changes files, continue only after restarting the Phase 8 loop at P8-T1.
- [x] [P8-T2] Run analyzer msbuild with `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`; write `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/evidence/qa-gates/msbuild-analyzers-final.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:`; if the command fails, fix the diagnostics and restart the Phase 8 loop at P8-T1.
- [x] [P8-T3] Run nullable msbuild with `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true`; write `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/evidence/qa-gates/msbuild-nullable-final.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:`; if the command fails, fix the diagnostics and restart the Phase 8 loop at P8-T1.
- [x] [P8-T4] Run VSTest with coverage using `vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /EnableCodeCoverage /ResultsDirectory:docs\features\active\2026-07-03-quickfiler-high-confidence-dequeue-streaming-233\evidence\qa-gates\vstest-results`; write `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/evidence/qa-gates/vstest-final.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, test counts, and numeric coverage values or a remediation-required statement if numeric coverage cannot be produced.
- [x] [P8-T5] Compare final coverage against `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/evidence/baseline/coverage-baseline.md` and the final VSTest coverage output; write `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/evidence/qa-gates/coverage-comparison.md` reporting baseline coverage, post-change coverage, changed/new-code coverage, repository coverage floor status, and PASS only when changed/new non-COM-bound code is at or above 90% and repository coverage does not regress below the applicable baseline/floor.

### Phase 9 — AC10 QA Tooling Remediation

- [x] [P9-T1] Run the installed CSharpier 1.2.6 formatter with `dotnet tool run csharpier -- format .`; write `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/evidence/qa-gates/csharpier-remediation-format.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:`; if this command changes any files, restart the Phase 9 loop at P9-T1 after recording the changed-file summary in this artifact.
- [x] [P9-T2] Run the installed CSharpier 1.2.6 check command with `dotnet tool run csharpier -- check .`; write `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/evidence/qa-gates/csharpier-remediation-check.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:`; if this command fails, run P9-T1 again and restart the Phase 9 loop at P9-T1.
- [x] [P9-T3] Run analyzer msbuild with `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`; write `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/evidence/qa-gates/msbuild-analyzers-remediation-final.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:`; if the command fails, fix only the reported diagnostics and restart the Phase 9 loop at P9-T1.
- [x] [P9-T4] Run nullable msbuild with `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true`; write `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/evidence/qa-gates/msbuild-nullable-remediation-final.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:`; if the command fails, fix only the reported diagnostics and restart the Phase 9 loop at P9-T1.
- [x] [P9-T5] Run VSTest with coverage using `vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /EnableCodeCoverage /ResultsDirectory:docs\features\active\2026-07-03-quickfiler-high-confidence-dequeue-streaming-233\evidence\qa-gates\vstest-remediation-results`; write `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/evidence/qa-gates/vstest-remediation-final.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, test counts, and the `.coverage` attachment path; if the command fails, fix only the reported test failures and restart the Phase 9 loop at P9-T1.
- [x] [P9-T6] Convert the VSTest `.coverage` attachment from P9-T5 by running `$coverageFile = Get-ChildItem -LiteralPath 'docs\features\active\2026-07-03-quickfiler-high-confidence-dequeue-streaming-233\evidence\qa-gates\vstest-remediation-results' -Recurse -Filter '*.coverage' | Sort-Object LastWriteTimeUtc -Descending | Select-Object -First 1; if ($null -eq $coverageFile) { throw 'No .coverage file found for issue #233 final QA.' }; dotnet-coverage merge $coverageFile.FullName -o 'docs\features\active\2026-07-03-quickfiler-high-confidence-dequeue-streaming-233\evidence\qa-gates\vstest-remediation-final.cobertura.xml' -f cobertura`; write `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/evidence/qa-gates/coverage-conversion-remediation-final.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`, input `.coverage` path, and output Cobertura path.
- [x] [P9-T7] Compare `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/evidence/qa-gates/vstest-remediation-final.cobertura.xml` against `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/evidence/baseline/coverage-baseline.md`; write `docs/features/active/2026-07-03-quickfiler-high-confidence-dequeue-streaming-233/evidence/qa-gates/coverage-comparison-remediation-final.md` with baseline repository coverage, post-change repository coverage, changed/new non-COM-bound coverage for `QuickFiler/Controllers/QfcStreamingDequeueConfidenceGate.cs`, and PASS only when repository coverage remains at or above the applicable baseline and floor and the changed/new non-COM-bound coverage is at or above 90%.

## Batch Limits

- Phase 1 touches at most one production file and one test file.
- Phase 2 touches at most three production files and three test files.
- Phase 3 touches at most one production file and one test file.
- Phase 4 touches at most three production files and three test files.
- Phase 5 touches at most three production files and three test files.
- Phase 6 is verification and evidence only unless automated tests expose a regression, in which case remediation must be planned as a separate batch.

## Automated Validation Summary

- AC1: P5 and P6 prove the single live dequeue-layer gate and absence of live post-display threshold removal.
- AC2: P3 and P4 prove dequeue-time score selection.
- AC3: P3 and P4 prove streaming backfill.
- AC4: P3 and P4 prove zero and partial source exhaustion.
- AC5: P5 and P6 prove surfaced pages are stable after later score changes.
- AC6: P4 proves sparse qualifying candidates fill pages up to source exhaustion.
- AC7: P4 proves disabled-mode parity.
- AC8: P6 records the dormant #171 disposition.
- AC9: P3 proves inclusive threshold semantics.
- AC10: P8 records the original final C# QA attempt, and P9 reruns the final C# QA loop with the installed CSharpier 1.2.6 `format`/`check` command shape and `dotnet-coverage` Cobertura conversion.
- AC11: P2 and P3 preserve #232 scoring logs and add dequeue-time score logging.
- AC12: P1 and P6 protect issue #232 navigation and ordinary non-high-confidence flow.
