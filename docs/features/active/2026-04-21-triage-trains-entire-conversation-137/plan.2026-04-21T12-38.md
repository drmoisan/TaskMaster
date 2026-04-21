# 2026-04-21-triage-trains-entire-conversation (Plan)

- **Issue:** #137
- **Parent (optional):** none
- **Owner:** drmoisan
- **Last Updated:** 2026-04-21T12-38
- **Status:** Active
- **Version:** 1.0
- **Work Mode:** minor-audit

## Overview

`Triage_OlLogic.TrainSelectionAsync` iterates `ActiveExplorer().Selection`, which in Outlook conversation view returns the entire conversation thread rather than only the explicitly selected email. All conversation items are trained and receive the "Triage" UDF — not just the one the user clicked. `TotalEmailCount` increments by the conversation size rather than by the number of explicitly selected items.

The fix follows the bugfix workflow: write a failing regression test first, implement the minimal targeted change, then verify with the full C# toolchain until clean.

**Requirements source:** `docs/features/active/2026-04-21-triage-trains-entire-conversation-137/issue.md` (sole authority — `spec.md` and `user-story.md` do not exist and must not be created).

**Evidence location:** `docs/features/active/2026-04-21-triage-trains-entire-conversation-137/evidence/`

**Fail-closed evidence rule:** Any Phase 0 baseline artifact, Phase 1 expect-fail or fix-verify artifact, or Phase 2 final-QC artifact that is absent or has incomplete required fields causes the delivery audit to return BLOCKED or INCOMPLETE, never PASS.

---

### Phase 0 — Baseline Capture

- [x] [P0-T1] Read policy files in the required order and save `evidence/phase0-instructions-read.md`
  - Policy reading order: `.github/copilot-instructions.md` → `.github/instructions/general-code-change.instructions.md` → `.github/instructions/general-unit-test.instructions.md` → `.github/instructions/csharp-code-change.instructions.md` → `.github/instructions/csharp-unit-test.instructions.md`
  - Artifact: `docs/features/active/2026-04-21-triage-trains-entire-conversation-137/evidence/phase0-instructions-read.md`
  - Required fields: `Timestamp:`, `Policy Order:`, explicit list of each file read with per-file read confirmation.
  - Acceptance: artifact exists at the stated path with all required fields populated.

- [x] [P0-T2] Record current branch name and HEAD commit SHA; save `evidence/phase0-branch-baseline.md`
  - Command: `git branch --show-current && git rev-parse HEAD`
  - Artifact: `docs/features/active/2026-04-21-triage-trains-entire-conversation-137/evidence/phase0-branch-baseline.md`
  - Required fields: `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` (branch name and commit SHA).
  - Acceptance: artifact exists and `EXIT_CODE: 0`.

- [x] [P0-T3] Run baseline CSharpier format check and save `evidence/phase0-format-baseline.md`
  - Command: `dotnet tool run csharpier format .`
  - Artifact: `docs/features/active/2026-04-21-triage-trains-entire-conversation-137/evidence/phase0-format-baseline.md`
  - Required fields: `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` (number of files checked, any files flagged for reformatting).
  - Acceptance: artifact exists with all required fields.

- [x] [P0-T4] Run baseline lint/analyzer build and save `evidence/phase0-lint-baseline.md`
  - Command: `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-VSBuild.ps1 -SolutionPath TaskMaster.sln -Configuration Debug -Platform 'Any CPU' -EnableNETAnalyzers -EnforceCodeStyleInBuild`
  - Artifact: `docs/features/active/2026-04-21-triage-trains-entire-conversation-137/evidence/phase0-lint-baseline.md`
  - Required fields: `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` (build result, error count, warning count).
  - Acceptance: artifact exists with all required fields.

- [x] [P0-T5] Run baseline nullable/type-check build and save `evidence/phase0-nullable-baseline.md`
  - Command: `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-VSBuild.ps1 -SolutionPath TaskMaster.sln -Configuration Debug -Platform 'Any CPU' -EnableNullable -TreatWarningsAsErrors`
  - Artifact: `docs/features/active/2026-04-21-triage-trains-entire-conversation-137/evidence/phase0-nullable-baseline.md`
  - Required fields: `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` (build result, nullable warning count).
  - Acceptance: artifact exists with all required fields.

- [x] [P0-T6] Run baseline test suite with coverage and save `evidence/phase0-test-baseline.md`
  - Command: `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug`
  - Artifact: `docs/features/active/2026-04-21-triage-trains-entire-conversation-137/evidence/phase0-test-baseline.md`
  - Required fields: `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` (total tests, passed, failed, line coverage % as a numeric value).
  - Acceptance: artifact exists with a numeric line coverage % in `Output Summary:`.

---

### Phase 1 — Bugfix Work

- [x] [P1-T1] [expect-fail] Add (or update existing) regression test `TrainSelectionAsync_WhenSelectionContainsTwoMailItems_TrainsOnlyFirstItem_TotalEmailCountIncrementsOnce` to `UtilitiesCS.Test/EmailIntelligence/ClassifierGroups/Triage/Triage_OlLogicTests.cs`
  - Scenario (AC1): Arrange a mock `Selection` via `mockSelection.As<IEnumerable>().Setup(s => s.GetEnumerator()).Returns(new List<object> { mockMailItem1.Object, mockMailItem2.Object }.GetEnumerator())` where both `mockMailItem1` and `mockMailItem2` are `new Mock<MailItem>(MockBehavior.Loose)` each configured with a `Mock<Attachments>` returning an empty enumerator; record `emailCountBefore = _triage.ClassifierGroup.TotalEmailCount`; call `await _triageOlLogic.TrainSelectionAsync("A", CancellationToken.None)`; assert `_triage.ClassifierGroup.TotalEmailCount.Should().Be(emailCountBefore + 1)` — the fix processes only the first item in the selection, simulating a conversation-view click that must not train the entire thread.
  - If tests with the old name `TrainSelectionAsync_WhenSelectionContainsTwoMailItems_TotalEmailCountIncrementsByExactlyTwo` or `TrainSelectionAsync_WhenSelectionContainsTwoMailItems_MatchEmailCountForLabelIncrementsByTwo` already exist in the file (added during a previous aborted attempt), remove them entirely and replace with the correct tests below.
  - Test must follow AAA structure with an intent comment explaining that two items are in the mock Selection (simulating conversation view), but only the first must be trained.
  - Exact test command: `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot UtilitiesCS.Test -Configuration Debug`
  - Acceptance: test method exists in the file and the assembly compiles; running the exact test command above exits with non-zero exit code because `_triage.ClassifierGroup.TotalEmailCount` is `emailCountBefore + 2` (current code trains both items) but the assertion expects `emailCountBefore + 1`; evidence artifact `docs/features/active/2026-04-21-triage-trains-entire-conversation-137/evidence/p1t1-expect-fail.md` exists with `Timestamp:`, `Command:`, `EXIT_CODE:` (non-zero or failure-noted), `Output Summary:` (failure assertion excerpt naming this test).

- [x] [P1-T2] [expect-fail] Add (or update existing) regression test `TrainSelectionAsync_WhenSelectionContainsTwoMailItems_TrainsOnlyFirstItem_MatchEmailCountIncrementsOnce` to `UtilitiesCS.Test/EmailIntelligence/ClassifierGroups/Triage/Triage_OlLogicTests.cs`
  - Scenario (AC2): Same two-item mock setup as P1-T1; record `matchCountBefore` as `_triage.ClassifierGroup.Classifiers.TryGetValue("A", out var cb) ? cb.MatchEmailCount : 0`; call `await _triageOlLogic.TrainSelectionAsync("A", CancellationToken.None)`; assert `_triage.ClassifierGroup.Classifiers["A"].MatchEmailCount.Should().Be(matchCountBefore + 1)` — the fix processes only the first item, so MatchEmailCount increments by exactly 1, not 2.
  - Test must follow AAA structure with an intent comment explaining that two items are in the mock Selection, but only the first must contribute to the matched-label count.
  - Exact test command: `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot UtilitiesCS.Test -Configuration Debug`
  - Acceptance: test method exists in the file and the assembly compiles; running the exact test command above exits with non-zero because the current code increments MatchEmailCount by 2 but the assertion expects 1; evidence artifact `docs/features/active/2026-04-21-triage-trains-entire-conversation-137/evidence/p1t2-expect-fail.md` exists with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` (failure assertion excerpt naming this test).

- [x] [P1-T3] Run the focused `UtilitiesCS.Test` suite to confirm that both P1-T1 and P1-T2 tests report as FAILED with the pre-fix production code; save `evidence/p1t3-regression-confirmed.md`
  - Command: `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot UtilitiesCS.Test -Configuration Debug`
  - Artifact: `docs/features/active/2026-04-21-triage-trains-entire-conversation-137/evidence/p1t3-regression-confirmed.md`
  - Required fields: `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` (list of failed tests — must include both test names from P1-T1 and P1-T2; total pass count and fail count).
  - Acceptance: artifact exists; `Output Summary:` explicitly names both `TrainSelectionAsync_WhenSelectionContainsTwoMailItems_TrainsOnlyFirstItem_TotalEmailCountIncrementsOnce` and `TrainSelectionAsync_WhenSelectionContainsTwoMailItems_TrainsOnlyFirstItem_MatchEmailCountIncrementsOnce` as failed, confirming the regression reproduces before any fix is applied.

- [x] [P1-T4] Implement the minimal fix in `UtilitiesCS/EmailIntelligence/ClassifierGroups/Triage/Triage_OlLogic.cs` `TrainSelectionAsync`: add `.Take(1)` after `.Cast<MailItem>()` in the LINQ pipeline so that only the first item in the selection is trained per invocation, regardless of how many items Outlook's conversation view populates into the Selection.
  - The fix is a single `.Take(1)` insertion between `.Cast<MailItem>()` and `.ToAsyncEnumerable()`. Do not change any other part of the method or any other file.
  - Rationale comment to add on the `.Take(1)` line: `// Outlook conversation view may expand Selection to include the entire thread; process only the focused item.`
  - Restrict changes to `TrainSelectionAsync` in `Triage_OlLogic.cs` only; do not modify `Triage.cs`, `UnTrainSelectionAsync`, or any other file.
  - Acceptance: the solution builds without new errors; `git diff --name-only` shows only `Triage_OlLogic.cs` modified among production files; no other production files are changed.

- [x] [P1-T5] Run the focused `UtilitiesCS.Test` suite to confirm that P1-T1 and P1-T2 now pass AND that existing test `TrainSelectionAsync_WhenSelectionContainsMailItem_TrainsClassifierWithExpectedLabel` still passes (AC3); save `evidence/p1t5-fix-verified.md`
  - Command: `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot UtilitiesCS.Test -Configuration Debug`
  - Artifact: `docs/features/active/2026-04-21-triage-trains-entire-conversation-137/evidence/p1t5-fix-verified.md`
  - Required fields: `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` (all three target test names listed as PASSED: `TrainSelectionAsync_WhenSelectionContainsTwoMailItems_TrainsOnlyFirstItem_TotalEmailCountIncrementsOnce`, `TrainSelectionAsync_WhenSelectionContainsTwoMailItems_TrainsOnlyFirstItem_MatchEmailCountIncrementsOnce`, `TrainSelectionAsync_WhenSelectionContainsMailItem_TrainsClassifierWithExpectedLabel`; total fail count == 0).
  - Acceptance: artifact exists; `EXIT_CODE: 0`; `Output Summary:` explicitly names all three tests as passed with fail count 0.

- [x] [P1-T6] Run CSharpier format on modified files and apply any changes it makes
  - Command: `dotnet tool run csharpier format .`
  - Acceptance: command exits with code 0; `git diff --name-only` after the run shows no additional unintended files changed.

- [x] [P1-T7] Run lint/analyzer build to confirm no new analyzer warnings or errors are introduced by the fix
  - Command: `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-VSBuild.ps1 -SolutionPath TaskMaster.sln -Configuration Debug -Platform 'Any CPU' -EnableNETAnalyzers -EnforceCodeStyleInBuild`
  - Acceptance: command exits with code 0; error count is 0; warning count does not exceed the baseline count recorded in `evidence/phase0-lint-baseline.md`.

- [x] [P1-T8] Run nullable/type-check build to confirm no new nullable warnings are introduced by the fix
  - Command: `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-VSBuild.ps1 -SolutionPath TaskMaster.sln -Configuration Debug -Platform 'Any CPU' -EnableNullable -TreatWarningsAsErrors`
  - Acceptance: command exits with code 0.

---

### Phase 2 — Final QC Loop

- [x] [P2-T1] Run CSharpier format as the first step of the final QC pass; save `evidence/p2t1-final-format.md`
  - Command: `dotnet tool run csharpier format .`
  - Artifact: `docs/features/active/2026-04-21-triage-trains-entire-conversation-137/evidence/p2t1-final-format.md`
  - Required fields: `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` (number of files checked; confirmation that zero files were reformatted in this final pass).
  - Acceptance: artifact exists; `EXIT_CODE: 0`; `Output Summary:` confirms zero files were reformatted.

- [x] [P2-T2] Run lint/analyzer build as the second step of the final QC pass; save `evidence/p2t2-final-lint.md`
  - Command: `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-VSBuild.ps1 -SolutionPath TaskMaster.sln -Configuration Debug -Platform 'Any CPU' -EnableNETAnalyzers -EnforceCodeStyleInBuild`
  - Artifact: `docs/features/active/2026-04-21-triage-trains-entire-conversation-137/evidence/p2t2-final-lint.md`
  - Required fields: `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` (build result, error count == 0, warning count).
  - Acceptance: artifact exists; `EXIT_CODE: 0`.

- [x] [P2-T3] Run nullable/type-check build as the third step of the final QC pass; save `evidence/p2t3-final-nullable.md`
  - Command: `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-VSBuild.ps1 -SolutionPath TaskMaster.sln -Configuration Debug -Platform 'Any CPU' -EnableNullable -TreatWarningsAsErrors`
  - Artifact: `docs/features/active/2026-04-21-triage-trains-entire-conversation-137/evidence/p2t3-final-nullable.md`
  - Required fields: `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` (build result, nullable warning count == 0).
  - Acceptance: artifact exists; `EXIT_CODE: 0`.

- [x] [P2-T4] Run the full test suite with coverage as the fourth step of the final QC pass; save `evidence/p2t4-final-test.md`
  - Command: `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug`
  - Artifact: `docs/features/active/2026-04-21-triage-trains-entire-conversation-137/evidence/p2t4-final-test.md`
  - Required fields: `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` (total tests, passed, failed == 0, line coverage % as a numeric value).
  - Acceptance: artifact exists; `EXIT_CODE: 0`; `Output Summary:` contains numeric line coverage %; failed == 0.

- [x] [P2-T5] Save coverage comparison artifact `evidence/p2t5-coverage-comparison.md` showing baseline vs. post-fix coverage
  - Artifact: `docs/features/active/2026-04-21-triage-trains-entire-conversation-137/evidence/p2t5-coverage-comparison.md`
  - Required fields: `Timestamp:`, `Baseline Coverage:` (numeric % from `evidence/phase0-test-baseline.md` Output Summary), `Post-fix Coverage:` (numeric % from `evidence/p2t4-final-test.md` Output Summary), `Delta:` (post-fix minus baseline, expressed as ± percentage points), `Coverage Threshold Met:` (yes/no — overall line coverage >= 80%; new/changed code in `UtilitiesCS` >= 90%).
  - Acceptance: artifact exists with all fields populated with numeric values; `Coverage Threshold Met: yes`.
