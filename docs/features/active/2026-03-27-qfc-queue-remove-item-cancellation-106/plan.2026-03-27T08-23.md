# 2026-03-27-qfc-queue-remove-item-cancellation (Plan)

- **Issue:** #106
- **Parent (optional):** none
- **Owner:** drmoisan
- **Branch:** `bug/qfc-queue-remove-item-cancellation-106`
- **Last Updated:** 2026-03-27T08-23
- **Status:** Active
- **Version:** 1.0
- **Work Mode:** minor-audit
- **Requirements Source:** `docs/features/active/2026-03-27-qfc-queue-remove-item-cancellation-106/issue.md`

## Overview

`QfcQueue.RemoveItem` propagates an unhandled `OperationCanceledException` when the
instance-level `_token` is already cancelled at the time the `EmailMoveMonitor` callback fires.
`JobsToFinish` calls `token.ThrowIfCancellationRequested()` in its polling loop; when
`_jobsRunning > 0` and the token is cancelled, the exception bubbles out unhandled. The fix
catches `OperationCanceledException` in `RemoveItem` and returns gracefully (logging at debug
level), since cleanup is moot when the token is cancelled. A regression MSTest is added in the
new file `QuickFiler.Test/Controllers/QfcQueueTests.cs`. The secondary concern
(`ConversationResolver.LoadConversationInfoAsync` assignment order, bug #103) is confirmed
already fixed in the current codebase — no code change is required for it.

---

### Phase 0 — Baseline Capture

- [x] [P0-T1] Read the 5 mandatory policy files in the required order and write evidence artifact `docs/features/active/2026-03-27-qfc-queue-remove-item-cancellation-106/phase0-instructions-read.md`.
  - Files to read in order:
    1. `.github/copilot-instructions.md`
    2. `.github/instructions/general-code-change.instructions.md`
    3. `.github/instructions/general-unit-test.instructions.md`
    4. `.github/instructions/csharp-code-change.instructions.md`
    5. `.github/instructions/csharp-unit-test.instructions.md`
  - Acceptance: `docs/features/active/2026-03-27-qfc-queue-remove-item-cancellation-106/phase0-instructions-read.md` exists and contains all of: `Timestamp:` (ISO-8601), `Policy Order:` (numbered list of all 5 files), and a `Status: All files read` line.

- [x] [P0-T2] Run baseline CSharpier format and write artifact `docs/features/active/2026-03-27-qfc-queue-remove-item-cancellation-106/baseline-build-format.md`.
  - Command: `dotnet tool run csharpier format .`
  - Acceptance: Artifact exists at the path above and contains all of: `Timestamp:` (ISO-8601), `Command: dotnet tool run csharpier format .`, `EXIT_CODE: 0`, `Output Summary:` (pass result — no files reformatted or "files were formatted" status).

- [x] [P0-T3] Run baseline analyzer build and write artifact `docs/features/active/2026-03-27-qfc-queue-remove-item-cancellation-106/baseline-build-analyzer.md`.
  - Command: `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-VSBuild.ps1 -SolutionPath TaskMaster.sln -Configuration Debug -Platform 'Any CPU' -EnableNETAnalyzers -EnforceCodeStyleInBuild`
  - Acceptance: Artifact exists at the path above and contains all of: `Timestamp:` (ISO-8601), `Command:` (full command above), `EXIT_CODE: 0`, `Output Summary:` (build outcome with warning and error counts, e.g. "Build succeeded. 0 Error(s), N Warning(s)").

- [x] [P0-T4] Run baseline nullable/type-safe build and write artifact `docs/features/active/2026-03-27-qfc-queue-remove-item-cancellation-106/baseline-build-nullable.md`.
  - Command: `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-VSBuild.ps1 -SolutionPath TaskMaster.sln -Configuration Debug -Platform 'Any CPU' -EnableNullable -TreatWarningsAsErrors`
  - Acceptance: Artifact exists at the path above and contains all of: `Timestamp:` (ISO-8601), `Command:` (full command above), `EXIT_CODE: 0`, `Output Summary:` (build outcome with warning and error counts).

- [x] [P0-T5] Run baseline MSTest with coverage and write artifact `docs/features/active/2026-03-27-qfc-queue-remove-item-cancellation-106/baseline-test.md`.
  - Command: `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug`
  - Acceptance: Artifact exists at the path above and contains all of: `Timestamp:` (ISO-8601), `Command:` (full command above), `EXIT_CODE: 0`, `Output Summary:` with numeric coverage headline (e.g., overall line coverage percentage and total pass/fail/skipped test counts for the QuickFiler.Test assembly).

---

### Phase 1 — Implementation

- [x] [P1-T1] Read `docs/features/active/2026-03-27-qfc-queue-remove-item-cancellation-106/issue.md` in full as the sole requirements source; confirm the two-item scope: (a) catch `OperationCanceledException` in `QfcQueue.RemoveItem` when `_token` is cancelled and return gracefully, (b) verify `ConversationResolver.LoadConversationInfoAsync` assignment order.
  - Acceptance: Task is complete when issue.md has been read; executor documents confirmation in a brief inline note (no artifact required for this task).

- [x] [P1-T2] Inspect `QuickFiler/Helper Classes/ConversationResolver.cs` method `LoadConversationInfoAsync` and verify that `ConversationInfo = pair` is assigned before `UpdateUI(pair.Expanded)` is called; record the finding as a note on this task. Make no code change regardless of finding.
  - Acceptance: Executor records one of these exact inline verdicts: `CONFIRMED: ConversationInfo = pair is assigned before UpdateUI is invoked (bug #103 fix present — no action required)` OR `NOT CONFIRMED: assignment order is incorrect — escalate before proceeding`.
  - Pre-verified finding (for executor reference): `ConversationInfo = pair` is assigned at the explicit assignment statement immediately before the `if (UpdateUI is not null)` block in `LoadConversationInfoAsync`. The code comment at that line reads "Assign ConversationInfo before calling UpdateUI so that any subsequent read of ConversationInfo.Expanded returns the cached value rather than re-entering the synchronous LoadConversationInfo()…". Bug #103 fix is confirmed present; no change required.

- [x] [P1-T3] Create new file `QuickFiler.Test/Controllers/QfcQueueTests.cs` containing MSTest `[TestClass]` `QfcQueueTests` with `[TestMethod]` `RemoveItem_WhenTokenPreCancelled_DoesNotThrow`. The test must: (1) cancel a `CancellationTokenSource` before constructing `QfcQueue`; (2) construct `QfcQueue` with the cancelled token, passing `null!` for `homeController` and a `Mock<IApplicationGlobals>().Object` for `appGlobals` (the pre-cancelled path exits before accessing either dependency); (3) set `_jobsRunning` to 1 via `System.Reflection.FieldInfo` so the `JobsToFinish` polling loop executes and reaches `ThrowIfCancellationRequested`; (4) call `await queue.RemoveItem(new Mock<Microsoft.Office.Interop.Outlook.MailItem>().Object)` inside a FluentAssertions `Awaiting(...).Should().NotThrowAsync<OperationCanceledException>()` assertion. The assertion is expected to FAIL before the production fix is applied (test is a TDD Red step).
  - Preconditions: Baseline captured in Phase 0; issue.md read in P1-T1.
  - Acceptance: File `QuickFiler.Test/Controllers/QfcQueueTests.cs` exists on disk with class `QfcQueueTests` and method `RemoveItem_WhenTokenPreCancelled_DoesNotThrow` compilable against the project's existing references.

- [x] [P1-T4] Register `QfcQueueTests.cs` in `QuickFiler.Test/QuickFiler.Test.csproj` by inserting `<Compile Include="Controllers\QfcQueueTests.cs" />` into the existing `<ItemGroup>` block that contains the other `Controllers\*.cs` compile entries (after the `QfcItemControllerTests.cs` entry).
  - Acceptance: `QuickFiler.Test.csproj` contains the line `<Compile Include="Controllers\QfcQueueTests.cs" />` in the Controllers ItemGroup; the solution builds (any build invocation returns EXIT_CODE 0 for the `QuickFiler.Test` project).

- [x] [P1-T5] [expect-fail] Run the full MSTest suite to confirm `RemoveItem_WhenTokenPreCancelled_DoesNotThrow` fails before the production fix is applied; save evidence artifact to `docs/features/active/2026-03-27-qfc-queue-remove-item-cancellation-106/expect-fail-p1t5.md`.
  - Command: `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug`
  - Acceptance: `docs/features/active/2026-03-27-qfc-queue-remove-item-cancellation-106/expect-fail-p1t5.md` exists and contains all of: `Timestamp:` (ISO-8601), `Command: pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug`, `EXIT_CODE: 1` (non-zero), `Failure:` field containing an excerpt attributable to `RemoveItem_WhenTokenPreCancelled_DoesNotThrow` (e.g., the test name and "OperationCanceledException was thrown" or the FluentAssertions failure message).

- [x] [P1-T6] Apply the minimal fix to `QfcQueue.RemoveItem` in `QuickFiler/Controllers/QfcQueue.cs`: wrap the `await JobsToFinish(100, _token)` call in a `try/catch (OperationCanceledException) when (_token.IsCancellationRequested)` block that logs at debug level ("RemoveItem exiting early: instance token is already cancelled") and returns. Do not change any other logic in the method or in `JobsToFinish`.
  - Acceptance: `QuickFiler/Controllers/QfcQueue.cs` diff shows only: a `try { ... }` around the existing `await JobsToFinish(100, _token)` statement at line 170, a `catch (OperationCanceledException) when (_token.IsCancellationRequested)` block with a `logger.Debug(...)` call and `return;`, and no other changes. The solution builds with EXIT_CODE 0.

- [x] [P1-T7] Run the full MSTest suite to confirm `RemoveItem_WhenTokenPreCancelled_DoesNotThrow` now passes after the fix; confirm no other tests regress.
  - Command: `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug`
  - Acceptance: Command exits with EXIT_CODE 0; test output shows `RemoveItem_WhenTokenPreCancelled_DoesNotThrow` as PASSED; total failed test count is 0 for the `QuickFiler.Test` assembly.

- [x] [P1-T8] Delegate implementation tasks P1-T3 through P1-T7 to `csharp-typed-engineer` with the following handoff inputs: (a) requirements source `docs/features/active/2026-03-27-qfc-queue-remove-item-cancellation-106/issue.md`, (b) production file to modify `QuickFiler/Controllers/QfcQueue.cs` (line 170), (c) new test file `QuickFiler.Test/Controllers/QfcQueueTests.cs`, (d) .csproj registration task P1-T4, (e) fix description from P1-T6, (f) expect-fail evidence artifact spec from P1-T5.
  - Acceptance: `csharp-typed-engineer` completes P1-T3 through P1-T7 and all per-task acceptance criteria above are satisfied; executor confirms handoff is finalized by verifying P1-T7 exit code 0.

---

### Phase 2 — Final QC Loop

- [x] [P2-T1] Run CSharpier format and write artifact `docs/features/active/2026-03-27-qfc-queue-remove-item-cancellation-106/final-qc-format.md`.
  - Command: `dotnet tool run csharpier format .`
  - Acceptance: Artifact exists at the path above and contains all of: `Timestamp:` (ISO-8601), `Command: dotnet tool run csharpier format .`, `EXIT_CODE: 0`, `Output Summary:` (no files reformatted or files-formatted count). If any files are reformatted, the toolchain loop restarts from this step.

- [x] [P2-T2] Run analyzer build and write artifact `docs/features/active/2026-03-27-qfc-queue-remove-item-cancellation-106/final-qc-analyzer.md`.
  - Command: `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-VSBuild.ps1 -SolutionPath TaskMaster.sln -Configuration Debug -Platform 'Any CPU' -EnableNETAnalyzers -EnforceCodeStyleInBuild`
  - Acceptance: Artifact exists at the path above and contains all of: `Timestamp:` (ISO-8601), `Command:` (full command above), `EXIT_CODE: 0`, `Output Summary:` (build outcome with warning and error counts). If EXIT_CODE is non-zero, fix all diagnostics and restart the toolchain loop from P2-T1.

- [x] [P2-T3] Run nullable/type-safe build and write artifact `docs/features/active/2026-03-27-qfc-queue-remove-item-cancellation-106/final-qc-nullable.md`.
  - Command: `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-VSBuild.ps1 -SolutionPath TaskMaster.sln -Configuration Debug -Platform 'Any CPU' -EnableNullable -TreatWarningsAsErrors`
  - Acceptance: Artifact exists at the path above and contains all of: `Timestamp:` (ISO-8601), `Command:` (full command above), `EXIT_CODE: 0`, `Output Summary:` (build outcome with warning and error counts). If EXIT_CODE is non-zero, fix all diagnostics and restart the toolchain loop from P2-T1.

- [x] [P2-T4] Run MSTest with coverage and write artifact `docs/features/active/2026-03-27-qfc-queue-remove-item-cancellation-106/final-qc-test.md`.
  - Command: `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug`
  - Acceptance: Artifact exists at the path above and contains all of: `Timestamp:` (ISO-8601), `Command:` (full command above), `EXIT_CODE: 0`, `Output Summary:` with numeric coverage headline (overall line coverage percentage and total pass/fail/skipped counts). If any test fails, fix the regression and restart the toolchain loop from P2-T1.

- [x] [P2-T5] Compare baseline coverage (from `baseline-test.md`) against post-change coverage (from `final-qc-test.md`) and report the delta; verify coverage thresholds.
  - Acceptance: Executor records all three values inline: (a) `Baseline coverage: N%` (from `baseline-test.md` `Output Summary:`), (b) `Post-change coverage: M%` (from `final-qc-test.md` `Output Summary:`), (c) `Delta: +/- X%`. Pass condition: overall coverage did not decrease from baseline (M >= N); new test code in `QfcQueueTests.cs` and changed production code in `QfcQueue.cs` (the catch block) are covered by `RemoveItem_WhenTokenPreCancelled_DoesNotThrow`. If coverage decreased, escalate before closing the QC loop.

- [x] [P2-T6] Delegate reduced small-path audit to `feature_code_review_agent` with the following inputs: feature folder `docs/features/active/2026-03-27-qfc-queue-remove-item-cancellation-106/`, production diff limited to `QfcQueue.cs`, test diff limited to `QfcQueueTests.cs`, all Phase 0 and Phase 2 QC artifacts, and the expect-fail evidence artifact `expect-fail-p1t5.md`.
  - Acceptance: `feature_code_review_agent` produces a reduced audit report in the feature folder (e.g., `audit-report.md`); executor confirms report exists and contains no blocking findings. If blocking findings are reported, they must be remediated before the plan is closed.
