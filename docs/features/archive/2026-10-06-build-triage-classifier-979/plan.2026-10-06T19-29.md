# Build Triage Classifier — Execution Plan

- **Issue:** #979
- **Work mode:** full-feature
- **Requirements:** `issue.md`, `spec.md`, `user-story.md`, and `research/2026-10-06T19-34-build-triage-classifier-research.md`
- **Evidence root:** `docs/features/active/2026-10-06-build-triage-classifier-979/evidence/`

Every command evidence artifact must contain `Timestamp:`, `Command:`,
`EXIT_CODE:`, and `Output Summary:`. Use the host-local `yyyy-MM-ddTHH-mm`
timestamp. If any formatter, analyzer, nullable, or test gate changes files or
fails, correct the issue and restart the final loop at P5-T1. An expected-fail
task is evidence only and does not authorize progression until its pass-after
test succeeds.

### Phase 0 — Policy and Baseline Capture

- [x] [P0-T1] Read `AGENTS.md` (standing instructions, general code-change policy, general unit-test policy), `.agents/skills/csharp/SKILL.md`, `issue.md`, `spec.md`, `user-story.md`, and `research/2026-10-06T19-34-build-triage-classifier-research.md` in that order. Write `docs/features/active/2026-10-06-build-triage-classifier-979/evidence/baseline/phase0-instructions-read.<timestamp>.md`. Acceptance: the artifact names the policy order and every file read.

- [x] [P0-T2] Run `dotnet tool restore` and `dotnet tool run csharpier check .` from the repository root. Write separate artifacts `evidence/baseline/p0-t2-tool-restore.<timestamp>.md` and `evidence/baseline/p0-t2-format.<timestamp>.md`. Acceptance: each artifact records its command result and the formatting artifact lists every unformatted file, if any.

- [x] [P0-T3] Run `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` and write `evidence/baseline/p0-t3-analyzers.<timestamp>.md`. Acceptance: the artifact records the exact warning/error counts and diagnostic identities.

- [x] [P0-T4] Run `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true` without `Nullable=enable`, and write `evidence/baseline/p0-t4-nullable.<timestamp>.md`. Acceptance: the command and warning/error counts are recorded and the command contains no `Nullable=enable` property.

- [x] [P0-T5] Resolve `vstest.console.exe` with `& "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe" -latest -products * -find "Common7\IDE\Extensions\TestPlatform\vstest.console.exe" | Select-Object -First 1`; run the resolved executable against `UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll` and `TaskMaster.Test\bin\Debug\TaskMaster.Test.dll` with `/EnableCodeCoverage /InIsolation /Logger:trx /ResultsDirectory:TestResults\issue-979-baseline`; then locate the produced `TestResults\issue-979-baseline\**\*.coverage` file and run `dotnet tool run dotnet-coverage merge <resolved-coverage-file> --output docs/features/active/2026-10-06-build-triage-classifier-979/evidence/baseline/p0-t5-baseline.cobertura.xml --output-format cobertura`. Parse the Cobertura XML automatically to record the numeric aggregate line-coverage percentage and per-changed/new-method coverage for the production methods changed by this plan. Write `evidence/baseline/p0-t5-vstest-coverage.<timestamp>.md`. Acceptance: the artifact includes pass/fail/skip counts, baseline failure identities, the numeric aggregate line-coverage percentage, and changed/new-method coverage; it compares the aggregate result against the 80 percent repository threshold and each changed/new method against the 90 percent threshold. Unavailable coverage is `REMEDIATION_REQUIRED`.

### Phase 1 — Preserve Nullable Triage on Mined Mail

- [x] [P1-T1] Add deterministic MSTest cases in `UtilitiesCS.Test/EmailIntelligence/EmailParsingSorting/MinedMailInfoTests.cs` that verify `MinedMailInfo(IItemInfo)` preserves A, B, C, and null Triage values, `DeepCopy` preserves each value, and JSON round-trip preserves null. Acceptance: tests use Moq and FluentAssertions without filesystem or Outlook dependencies.

- [x] [P1-T2] [expect-fail] Run the new cases using the P0-T5 VSTest executable with `UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /InIsolation /TestCaseFilter:"FullyQualifiedName~MinedMailInfoTests"`; write `evidence/regression-testing/p1-t2-mined-triage-fail-before.<timestamp>.md`. Acceptance: failure proves the absent field or copy path; an unexpected pass requires `evidence/regression-testing/p1-t2-fail-before-exception.<timestamp>.md` explaining the verified existing behavior.

- [x] [P1-T3] Update `UtilitiesCS/EmailIntelligence/EmailParsingSorting/MinedMailInfo.cs` to add nullable `Triage`, assign it from `IItemInfo.Triage` in the constructor, and preserve it in `DeepCopy`. Acceptance: A/B/C/null values are unchanged and no null label is converted to an A/B/C class.

- [x] [P1-T4] Format `MinedMailInfo.cs` and `MinedMailInfoTests.cs` using `dotnet tool run csharpier format UtilitiesCS/EmailIntelligence/EmailParsingSorting/MinedMailInfo.cs UtilitiesCS.Test/EmailIntelligence/EmailParsingSorting/MinedMailInfoTests.cs`, then rerun P1-T2's filter and write `evidence/regression-testing/p1-t4-mined-triage-pass-after.<timestamp>.md`. Acceptance: all filtered tests pass.

### Phase 2 — Rebuild from Staged Mined Mail

- [x] [P2-T1] Add deterministic tests in `UtilitiesCS.Test/EmailIntelligence/ClassifierGroups/ClassifierGroups_Tests.cs` for rebuilding from synthetic `MinedMailInfo`: valid A/B/C records create corresponding classifiers and initialize aggregate email counts/shared token base; null, empty, lower-case, and invalid labels add no trained data and leave aggregate email counts and shared-token state unchanged. Acceptance: assertions directly inspect `BayesianClassifierGroup` state with no external service, filesystem, or Outlook dependency, while retaining the established A/B/C structure.

- [x] [P2-T2] [expect-fail] Run the focused classifier tests with the P0-T5 VSTest executable and `UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /InIsolation /TestCaseFilter:"FullyQualifiedName~Triage"`; write `evidence/regression-testing/p2-t2-rebuild-fail-before.<timestamp>.md`. Acceptance: the result proves the missing API or reconstruction behavior; an unexpected pass requires the exception dossier pattern from P1-T2.

- [x] [P2-T3] Update `UtilitiesCS/EmailIntelligence/ClassifierGroups/Triage/Triage.cs` with an asynchronous collection-based rebuild operation over `IEnumerable<MinedMailInfo>` or `MinedMailInfo[]` for deterministic tests, a thin wrapper that loads staged records through the existing staging path, and a narrow injectable seam for persistence and manager replacement. Strictly filter labels to `A`, `B`, and `C` before aggregate counts, shared-token initialization, per-class reconstruction, persistence, or manager replacement; reuse existing `CreateTriageClassifiersAsync` and `BayesianClassifierGroup.RebuildClassifier` initialization. Acceptance: invalid/null records contribute neither classifier data, class counts, nor shared tokens; valid records initialize all three classes, aggregate state, one persistence request, and active manager replacement without changing existing training or classification behavior.

- [x] [P2-T4] Extend `ClassifierGroups_Tests.cs` to verify through the Phase 2 seam exactly one serialize request, manager replacement, and no persistence or manager mutation when no valid training data is present. Format `Triage.cs` and `ClassifierGroups_Tests.cs`, rerun P2-T2's filter, and write `evidence/regression-testing/p2-t4-rebuild-pass-after.<timestamp>.md`. Acceptance: all focused tests pass and map filtering, reconstruction, persistence, manager replacement, and the no-valid-training-data mutation guard to test names.

### Phase 3 — Ribbon Command

- [x] [P3-T1] Add failing ribbon tests in `TaskMaster.Test/Ribbon/RibbonExplorerXmlTests.cs` and `TaskMaster.Test/Ribbon/RibbonViewerEngineCallbackShapeTests.cs` requiring the label `Build Triage Classifier` in the existing `TaskMaster -> Settings -> Folder Classifier` hierarchy, a matching XML `onAction`, a public `void (Microsoft.Office.Core.IRibbonControl)` callback on `RibbonViewer`, viewer-to-controller dispatch, and an awaited rebuild invocation through the Phase 2 seam. Acceptance: tests parse the embedded XML, reflect production types, and invoke the dispatch path without launching Outlook, creating UI handles, or using Outlook/UI handles.

- [x] [P3-T2] [expect-fail] Run the new tests with the P0-T5 VSTest executable and `TaskMaster.Test\bin\Debug\TaskMaster.Test.dll /InIsolation /TestCaseFilter:"FullyQualifiedName~RibbonExplorerXmlTests|FullyQualifiedName~RibbonViewerEngineCallbackShapeTests"`; write `evidence/regression-testing/p3-t2-ribbon-fail-before.<timestamp>.md`. Acceptance: the artifact proves the requested button or callback is absent; unexpected success uses the P1-T2 exception dossier pattern.

- [x] [P3-T3] Update `TaskMaster/Ribbon/RibbonExplorer.xml`, `TaskMaster/Ribbon/RibbonViewer.EngineCommands.cs`, and `TaskMaster/Ribbon/RibbonController.Intelligence.cs` to add `Build Triage Classifier` at `TaskMaster -> Settings -> Folder Classifier`, bind it to a public viewer callback, and await the Phase 2 MinedMailInfo rebuild operation in the controller. Acceptance: the control is schema-legal, the callback has the exact Office signature, and the controller reaches the rebuild operation.

- [x] [P3-T4] Format the four changed C# ribbon/test files with CSharpier, rerun P3-T2's filter, and write `evidence/regression-testing/p3-t4-ribbon-pass-after.<timestamp>.md`. Acceptance: focused ribbon tests pass and identify the XML placement and callback path.

### Phase 4 — Acceptance-Criteria Verification

- [x] [P4-T1] Run focused UtilitiesCS tests with the P0-T5 VSTest executable and `UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /InIsolation /TestCaseFilter:"FullyQualifiedName~MinedMailInfoTests|FullyQualifiedName~Triage"`; write `evidence/regression-testing/p4-t1-focused-utilities.<timestamp>.md`. Acceptance: all selected tests pass and map field preservation, filtering, aggregate/token initialization, persistence, and manager replacement to test names.

- [x] [P4-T2] Run focused TaskMaster ribbon tests with the P0-T5 VSTest executable and `TaskMaster.Test\bin\Debug\TaskMaster.Test.dll /InIsolation /TestCaseFilter:"FullyQualifiedName~RibbonExplorerXmlTests|FullyQualifiedName~RibbonViewerEngineCallbackShapeTests"`; write `evidence/regression-testing/p4-t2-focused-ribbon.<timestamp>.md`. Acceptance: all selected tests pass and map command placement and callback routing to test names.

- [x] [P4-T3] Check off verified acceptance criteria in `docs/features/active/2026-10-06-build-triage-classifier-979/issue.md`, `spec.md`, and `user-story.md`, and write `evidence/other/p4-t3-acceptance-criteria.<timestamp>.md`. Acceptance: each checked item cites a passing evidence artifact; every unverified item remains unchecked and is `REMEDIATION_REQUIRED`.

### Phase 5 — Full C# QA Loop

- [x] [P5-T1] Run `dotnet tool run csharpier format .` and write `evidence/qa-gates/p5-t1-format.<timestamp>.md`. Acceptance: exit code is 0; any changed file restarts Phase 5 at this task.

- [x] [P5-T2] Run `dotnet tool run csharpier check .` and write `evidence/qa-gates/p5-t2-format-check.<timestamp>.md`. Acceptance: exit code is 0 and zero files require formatting.

- [x] [P5-T3] Run `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` and write `evidence/qa-gates/p5-t3-analyzers.<timestamp>.md`. Acceptance: exit code is 0 with no diagnostics added from P0-T3; otherwise correct in scope and restart P5-T1.

- [x] [P5-T4] Run `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true` and write `evidence/qa-gates/p5-t4-nullable.<timestamp>.md`. Acceptance: exit code is 0, `Nullable=enable` is absent, and no compiler/nullable diagnostic is added from P0-T4; otherwise correct in scope and restart P5-T1.

- [x] [P5-T5] Run the P0-T5 VSTest executable against `UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll` and `TaskMaster.Test\bin\Debug\TaskMaster.Test.dll` with `/EnableCodeCoverage /InIsolation /Logger:trx /ResultsDirectory:TestResults\issue-979-final`; then locate the produced `TestResults\issue-979-final\**\*.coverage` file and run `dotnet tool run dotnet-coverage merge <resolved-coverage-file> --output docs/features/active/2026-10-06-build-triage-classifier-979/evidence/qa-gates/p5-t5-final.cobertura.xml --output-format cobertura`. Parse the Cobertura XML automatically to record the numeric aggregate line-coverage percentage and per-changed/new-method coverage, and compare them to P0-T5. Write `evidence/qa-gates/p5-t5-vstest-coverage.<timestamp>.md`. Acceptance: exit code is 0, no new failure exists from P0-T5, the numeric aggregate coverage line is recorded and meets the 80 percent repository threshold, every changed/new method meets the 90 percent threshold, and no applicable measure regresses from P0-T5; missing or regressed coverage is `REMEDIATION_REQUIRED` and restarts P5-T1. Verification: `p5-t5-vstest-coverage.2026-10-06T20-42.md` records 5,491 passing tests, non-regressing aggregate coverage of 65.2006 percent, and passing feature-method coverage. The user authorized a one-time exception to the aggregate 80 percent threshold for issue #979 only; see `evidence/other/coverage-exception.2026-10-06T21-37.md`.

- [x] [P5-T6] Compare the Phase 0 baselines with P5-T2 through P5-T5 and write `evidence/qa-gates/p5-t6-qa-summary.<timestamp>.md`. Acceptance: the summary reports formatting, analyzer, nullable, test, and numeric coverage baseline/post values and states that the final toolchain pass completed without errors. Verification: `p5-t6-qa-summary.2026-10-06T20-42.md` records a clean full C# toolchain pass. The aggregate coverage exception is documented in `evidence/other/coverage-exception.2026-10-06T21-37.md`.
