# Issue #979 File-Size Remediation Plan

Canonical issue number: 979

## Objective

Resolve PA-979-2 / CR-979-2 by extracting only the issue #979 test additions
from the three reviewed aggregate test files. Preserve every existing test and
the delivered Triage behavior. Do not modify production code, policy files, or
unrelated baseline tests.

## Scope and constraints

- Requirements and remediation source: `docs/features/active/2026-10-06-build-triage-classifier-979/issue.md` and `docs/features/active/2026-10-06-build-triage-classifier-979/2026-10-06T23-01-remediation/remediation-inputs.2026-10-06T23-01.md`.
- Required full-feature documents: `spec.md` and `user-story.md` in the same feature folder.
- Merge-base reference: `c76e830c18976221b5730f84b8d88aebbfc4f04b`.
- Every command evidence artifact must contain `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:` and must use the host-local `yyyy-MM-ddTHH-mm` timestamp.
- The one-time issue #979 exception applies only to coverage requirements. Capture coverage when the C# test command produces it, but do not treat numeric coverage thresholds as blockers. Formatting, analyzers, compiler/nullability, tests, diff hygiene, legacy project inclusion, and the 500-line limit remain mandatory.
- If a formatting, analyzer, compiler, or test gate changes files or fails, correct the in-scope cause and restart the Phase 3 loop at P3-T1.
- Do not create temporary files in unit tests, call Outlook, use network services, or weaken/drop assertions.

### Phase 0 — Policy, Context, and C# Baseline Capture

- [x] [P0-T1] Read `AGENTS.md` in policy order (standing instructions, cross-language code-change policy, cross-language unit-test policy, then C# requirements), `.agents/skills/csharp/SKILL.md`, `issue.md`, `spec.md`, `user-story.md`, `remediation-inputs.2026-10-06T23-01.md`, the three `2026-10-06T23-00-audit` artifacts, `artifacts/pr_context.summary.txt`, `artifacts/pr_context.appendix.txt`, `plan.2026-10-06T19-29.md`, and `2026-10-06T21-49-remediation/remediation-plan.2026-10-06T21-49.md`. Write `docs/features/active/2026-10-06-build-triage-classifier-979/evidence/remediation-baseline/p0-t1-instructions-read.<timestamp>.md` with `Timestamp:`, `Policy Order:`, and the complete read-file list. Acceptance: the artifact records PA-979-2, the issue-979 coverage-only exception, the full-feature document set, and the 500-line rule before source changes begin.

- [x] [P0-T2] Run `git diff --numstat c76e830c18976221b5730f84b8d88aebbfc4f04b..HEAD -- TaskMaster.Test/Ribbon/RibbonExplorerXmlTests.cs UtilitiesCS.Test/EmailIntelligence/ClassifierGroups/ClassifierGroups_Tests.cs UtilitiesCS.Test/EmailIntelligence/EmailDataMiner_Tests.cs` and count physical lines in those three files. Write `docs/features/active/2026-10-06-build-triage-classifier-979/evidence/remediation-baseline/p0-t2-file-size-baseline.<timestamp>.md`. Acceptance: the artifact records the three baseline/current values 496/531, 1732/1976, and 609/611 plus the issue-specific diff additions +35, +244, and +2.

- [x] [P0-T3] Run `dotnet tool restore`, then run `dotnet tool run csharpier check .` from the repository root. Write separate artifacts `docs/features/active/2026-10-06-build-triage-classifier-979/evidence/remediation-baseline/p0-t3-tool-restore.<timestamp>.md` and `docs/features/active/2026-10-06-build-triage-classifier-979/evidence/remediation-baseline/p0-t3-format-check.<timestamp>.md`. Acceptance: both artifacts contain the required command schema and the format artifact records whether the baseline requires formatting.

- [x] [P0-T4] Run `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`. Write `docs/features/active/2026-10-06-build-triage-classifier-979/evidence/remediation-baseline/p0-t4-analyzers.<timestamp>.md`. Acceptance: the artifact records analyzer and code-style warning/error totals for comparison with the final rebuild.

- [x] [P0-T5] Run `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true` without `/p:Nullable=enable`. Write `docs/features/active/2026-10-06-build-triage-classifier-979/evidence/remediation-baseline/p0-t5-nullable.<timestamp>.md`. Acceptance: the artifact records compiler/nullable warning/error totals and proves the command did not force project-wide nullable opt-in.

- [x] [P0-T6] Resolve `vstest.console.exe` with `& "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe" -latest -products * -find "Common7\IDE\Extensions\TestPlatform\vstest.console.exe" | Select-Object -First 1`, then run the resolved executable against `UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll` with `/EnableCodeCoverage /InIsolation /Logger:trx /ResultsDirectory:TestResults\issue-979-file-size-baseline-utilities`. Write `docs/features/active/2026-10-06-build-triage-classifier-979/evidence/remediation-baseline/p0-t6-utilities-vstest.<timestamp>.md`. Acceptance: the artifact records test pass/fail/skip totals and any available numeric coverage without applying coverage thresholds.

- [x] [P0-T7] Run the P0-T6 resolved VSTest executable against `TaskMaster.Test\bin\Debug\TaskMaster.Test.dll` with `/EnableCodeCoverage /InIsolation /TestCaseFilter:"TestCategory!=LiveOutlook" /Logger:trx /ResultsDirectory:TestResults\issue-979-file-size-baseline-taskmaster`. Write `docs/features/active/2026-10-06-build-triage-classifier-979/evidence/remediation-baseline/p0-t7-taskmaster-vstest.<timestamp>.md`. Acceptance: the artifact records test pass/fail/skip totals, preserves the standard-QC `LiveOutlook` exclusion, and records any available numeric coverage without applying coverage thresholds.

### Phase 1 — Extract Issue-Specific Tests

- [x] [P1-T1] Move `RibbonExplorerXml_BuildTriageClassifierIsInSettingsFolderClassifierMenu` from `TaskMaster.Test/Ribbon/RibbonExplorerXmlTests.cs` to new partial companion `TaskMaster.Test/Ribbon/RibbonExplorerXmlTests.FolderClassifier.cs`, retaining the `RibbonExplorerXmlTests` class name so it can use the existing private resource loader; add the new file to `TaskMaster.Test/TaskMaster.Test.csproj`. Acceptance: the original file contains none of the issue #979 menu-test addition, the new partial companion is below 500 physical lines, the test name and XML assertions are unchanged, and the project explicitly compiles the new file.

- [x] [P1-T2] Move the complete `TriageClassifierRebuild_Tests` class, including all six test methods and `ConfigurableManagerAsyncLazy`, from `UtilitiesCS.Test/EmailIntelligence/ClassifierGroups/ClassifierGroups_Tests.cs` to new `UtilitiesCS.Test/EmailIntelligence/ClassifierGroups/TriageClassifierRebuild_Tests.cs`, then add the new file to `UtilitiesCS.Test/UtilitiesCS.Test.csproj`. Acceptance: `ClassifierGroups_Tests.cs` has no issue #979 class content relative to the merge base, the new file is below 500 physical lines, all six test names and assertions are preserved, and the legacy project explicitly compiles the new file.

- [x] [P1-T3] Remove the two issue #979 Triage assertions from `UtilitiesCS.Test/EmailIntelligence/EmailDataMiner_Tests.cs` and create `UtilitiesCS.Test/EmailIntelligence/EmailDataMinerTriageMapping_Tests.cs` with the equivalent explicit `EmailDataMiner.ToMinedMail` Triage mapping verification; add the new file to `UtilitiesCS.Test/UtilitiesCS.Test.csproj`. Acceptance: `EmailDataMiner_Tests.cs` contains no issue #979 additions relative to the merge base, the new file is below 500 physical lines, and its deterministic MSTest/Moq/FluentAssertions behavior preserves the A/B/C/null mapping assertion without filesystem, network, or Outlook access.

- [x] [P1-T4] Run `git diff --numstat c76e830c18976221b5730f84b8d88aebbfc4f04b..HEAD -- TaskMaster.Test/Ribbon/RibbonExplorerXmlTests.cs UtilitiesCS.Test/EmailIntelligence/ClassifierGroups/ClassifierGroups_Tests.cs UtilitiesCS.Test/EmailIntelligence/EmailDataMiner_Tests.cs` and count physical lines in each remediated aggregate file and each new focused test file. Write `docs/features/active/2026-10-06-build-triage-classifier-979/evidence/regression-testing/p1-t4-test-extraction-shape.<timestamp>.md`. Acceptance: the three remediated aggregate files show no remaining issue #979 additions, `RibbonExplorerXmlTests.cs` is at or below 500 lines, and every newly created focused test file is below 500 lines.

### Phase 2 — Focused Functional Verification

- [x] [P2-T1] Run `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true` without `/p:Nullable=enable` after P1-T1 through P1-T3. Write `docs/features/active/2026-10-06-build-triage-classifier-979/evidence/regression-testing/p2-t1-post-extraction-build.<timestamp>.md`. Acceptance: the rebuild succeeds and produces current `UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll` and `TaskMaster.Test\bin\Debug\TaskMaster.Test.dll` assemblies containing all three extracted test files and their explicit legacy-project compile entries.

- [x] [P2-T2] Use the P0-T6 VSTest executable to run `UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /InIsolation /TestCaseFilter:"FullyQualifiedName~TriageClassifierRebuild_Tests"`. Write `docs/features/active/2026-10-06-build-triage-classifier-979/evidence/regression-testing/p2-t2-triage-rebuild-extraction.<timestamp>.md`. Acceptance: all six extracted rebuild tests are discovered and pass with unchanged classifier filtering, state, persistence, and manager-replacement assertions.

- [x] [P2-T3] Use the P0-T6 VSTest executable to run `TaskMaster.Test\bin\Debug\TaskMaster.Test.dll /InIsolation /TestCaseFilter:"FullyQualifiedName~RibbonExplorerXml_BuildTriageClassifierIsInSettingsFolderClassifierMenu"`. Write `docs/features/active/2026-10-06-build-triage-classifier-979/evidence/regression-testing/p2-t3-ribbon-menu-extraction.<timestamp>.md`. Acceptance: the extracted menu test is discovered and passes with the exact `Build Triage Classifier` hierarchy and callback assertions.

- [x] [P2-T4] Use the P0-T6 VSTest executable to run `UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /InIsolation /TestCaseFilter:"FullyQualifiedName~EmailDataMinerTriageMapping_Tests"`. Write `docs/features/active/2026-10-06-build-triage-classifier-979/evidence/regression-testing/p2-t4-mined-mail-triage-extraction.<timestamp>.md`. Acceptance: the focused mapping test is discovered and passes for A, B, C, and null Triage values.

### Phase 3 — Full C# Final QA Loop

- [x] [P3-T1] Run `dotnet tool run csharpier format .` and write `docs/features/active/2026-10-06-build-triage-classifier-979/evidence/qa-gates/p3-t1-format.<timestamp>.md`. Acceptance: the artifact records whether formatting changed files; if it did, restart Phase 3 at P3-T1 after the formatter completes.

- [x] [P3-T2] Run `dotnet tool run csharpier check .` and write `docs/features/active/2026-10-06-build-triage-classifier-979/evidence/qa-gates/p3-t2-format-check.<timestamp>.md`. Acceptance: CSharpier reports no files requiring formatting.

- [x] [P3-T3] Run `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` and write `docs/features/active/2026-10-06-build-triage-classifier-979/evidence/qa-gates/p3-t3-analyzers.<timestamp>.md`. Acceptance: the rebuild passes with no analyzer or code-style diagnostic regression from P0-T4.

- [x] [P3-T4] Run `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true` without `/p:Nullable=enable`, and write `docs/features/active/2026-10-06-build-triage-classifier-979/evidence/qa-gates/p3-t4-nullable.<timestamp>.md`. Acceptance: the rebuild passes with no compiler or nullable diagnostic regression from P0-T5.

- [x] [P3-T5] Use the P0-T6 VSTest executable to run `UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /EnableCodeCoverage /InIsolation /Logger:trx /ResultsDirectory:TestResults\issue-979-file-size-final-utilities`; then write `docs/features/active/2026-10-06-build-triage-classifier-979/evidence/qa-gates/p3-t5-utilities-vstest.<timestamp>.md`. Acceptance: the UtilitiesCS standard-QC suite passes, the artifact records test totals and available numeric coverage, and the issue-979 coverage-only exception is stated without waiving any functional failure.

- [x] [P3-T6] Use the P0-T6 VSTest executable to run `TaskMaster.Test\bin\Debug\TaskMaster.Test.dll /EnableCodeCoverage /InIsolation /TestCaseFilter:"TestCategory!=LiveOutlook" /Logger:trx /ResultsDirectory:TestResults\issue-979-file-size-final-taskmaster`; then write `docs/features/active/2026-10-06-build-triage-classifier-979/evidence/qa-gates/p3-t6-taskmaster-vstest.<timestamp>.md`. Acceptance: the TaskMaster standard-QC suite passes, `LiveOutlook` remains excluded by the existing contract, and the artifact records test totals and available numeric coverage under the coverage-only exception.

- [x] [P3-T7] Run `git diff --check c76e830c18976221b5730f84b8d88aebbfc4f04b..HEAD`, repeat the P1-T4 numstat and physical-line-count comparison, and write `docs/features/active/2026-10-06-build-triage-classifier-979/evidence/qa-gates/p3-t7-file-size-and-diff-hygiene.<timestamp>.md`. Acceptance: diff hygiene exits 0, each new file is below 500 lines, `RibbonExplorerXmlTests.cs` is at or below 500 lines, and the two preexisting oversized UtilitiesCS files have no issue #979 additions relative to the merge base.

### Phase 4 — Acceptance and Re-Review Loop

- [x] [P4-T1] Compare the five checked criteria in `docs/features/active/2026-10-06-build-triage-classifier-979/issue.md` with P2-T2 through P2-T4 and P3-T3 through P3-T7. Write `docs/features/active/2026-10-06-build-triage-classifier-979/evidence/other/p4-t1-acceptance-and-remediation-summary.<timestamp>.md`. Acceptance: the artifact maps every criterion to passing functional and non-coverage evidence, records the coverage-only exception separately, and leaves every already-verified issue checkbox unchanged.

- [ ] [P4-T2] Invoke the repository feature-review workflow autonomously for the full committed range `c76e830c18976221b5730f84b8d88aebbfc4f04b..HEAD`, supplying the completed remediation diff, all P0 through P4 evidence artifacts, `artifacts/pr_context.summary.txt`, `artifacts/pr_context.appendix.txt`, and the three `2026-10-06T23-00-audit` artifacts. Acceptance: the workflow writes validated policy-audit, code-review, and feature-audit artifacts; records a PR-readiness verdict; verifies CR-979-2/PA-979-2 is resolved without scope expansion; and automatically creates new remediation inputs and a plan if any autonomous non-coverage finding remains. No human validation or manual submission step is permitted.
