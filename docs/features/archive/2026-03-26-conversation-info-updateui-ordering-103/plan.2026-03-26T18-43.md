# 2026-03-26-conversation-info-updateui-ordering (Plan)

- **Issue:** #103
- **Parent (optional):** none
- **Owner:** drmoisan
- **Last Updated:** 2026-03-26T18-43
- **Status:** In Progress
- **Version:** 1.0
- **Work Mode:** minor-audit

Requirements source: `docs/features/active/2026-03-26-conversation-info-updateui-ordering-103/issue.md`

## Root Cause Summary

`LoadConversationInfoAsync()` in `ConversationResolver` reads `ConversationInfo.Expanded` (via `UpdateUI(ConversationInfo.Expanded)`) **before** setting `ConversationInfo = pair`. This accesses the lazy property getter, which invokes `LoadConversationInfo()` synchronously. When `Count.Expanded == 0` (e.g. when a mail item is in `Junk E-mail` and all rows are filtered out of `Df.Expanded`), `LoadConversationInfo()` hits its guard clause and throws `InvalidOperationException`.

Fix: In `LoadConversationInfoAsync()`, assign `ConversationInfo = pair` **before** calling `UpdateUI`, and pass `pair.Expanded` directly to `UpdateUI` to eliminate the unnecessary property re-read.

---

### Phase 0 — Policy Read + Baseline Capture

- [x] [P0-T1] Read mandatory policy files in policy-compliance order and save a policy-read evidence artifact.
  - Files to read (in order):
    1. `CLAUDE.md`
    2. `.github/instructions/general-code-change.instructions.md`
    3. `.github/instructions/general-unit-test.instructions.md`
    4. `.github/instructions/csharp-code-change.instructions.md`
    5. `.github/instructions/csharp-unit-test.instructions.md`
  - Acceptance: File `docs/features/active/2026-03-26-conversation-info-updateui-ordering-103/evidence/baseline/phase0-instructions-read.md` exists and contains:
    - `Timestamp: <ISO-8601>`
    - `Policy Order:` listing all five policy files read in order
    - Explicit list of filenames read

- [x] [P0-T2] Run the formatter to establish a format baseline and save the artifact.
  - Command: `dotnet tool run csharpier format .`
  - Acceptance: File `docs/features/active/2026-03-26-conversation-info-updateui-ordering-103/evidence/baseline/baseline-format.md` exists and contains:
    - `Timestamp: <ISO-8601>`
    - `Command: dotnet tool run csharpier format .`
    - `EXIT_CODE: 0`
    - `Output Summary:` confirming no files were changed

- [x] [P0-T3] Run the lint/analyzer build to establish a lint baseline and save the artifact.
  - Command: `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-VSBuild.ps1 -SolutionPath TaskMaster.sln -Configuration Debug -Platform "Any CPU" -EnableNETAnalyzers -EnforceCodeStyleInBuild`
  - Acceptance: File `docs/features/active/2026-03-26-conversation-info-updateui-ordering-103/evidence/baseline/baseline-lint.md` exists and contains:
    - `Timestamp: <ISO-8601>`
    - `Command: <exact command above>`
    - `EXIT_CODE: 0`
    - `Output Summary:` confirming build succeeded with 0 errors

- [x] [P0-T4] Run the nullable/type-check build to establish a nullable baseline and save the artifact.
  - Command: `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-VSBuild.ps1 -SolutionPath TaskMaster.sln -Configuration Debug -Platform "Any CPU" -EnableNullable -TreatWarningsAsErrors`
  - Acceptance: File `docs/features/active/2026-03-26-conversation-info-updateui-ordering-103/evidence/baseline/baseline-nullable.md` exists and contains:
    - `Timestamp: <ISO-8601>`
    - `Command: <exact command above>`
    - `EXIT_CODE: 0`
    - `Output Summary:` confirming build succeeded with 0 errors

- [x] [P0-T5] Run the targeted regression test filter to establish a test baseline (expect 0 matching tests before implementation) and save the artifact.
  - Command: `& "C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\CommonExtensions\Microsoft\TestWindow\vstest.console.exe" "QuickFiler.Test\bin\Debug\QuickFiler.Test.dll" /InIsolation /TestCaseFilter:"FullyQualifiedName~UpdateUIBeforeSet OR FullyQualifiedName~LoadConversationInfoAsync"`
  - Acceptance: File `docs/features/active/2026-03-26-conversation-info-updateui-ordering-103/evidence/baseline/baseline-test-filter.md` exists and contains:
    - `Timestamp: <ISO-8601>`
    - `Command: <exact command above>`
    - `EXIT_CODE: <recorded integer>`
    - `Output Summary:` noting that `UpdateUIBeforeSet` / `LoadConversationInfoAsync` tests do not yet exist at baseline (0 tests found is the expected baseline state)

- [x] [P0-T6] Run the full QuickFiler.Test suite with coverage enabled to establish a numeric coverage baseline and save the artifact.
  - Command: `& "C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\CommonExtensions\Microsoft\TestWindow\vstest.console.exe" "QuickFiler.Test\bin\Debug\QuickFiler.Test.dll" /InIsolation /EnableCodeCoverage`
  - Acceptance: File `docs/features/active/2026-03-26-conversation-info-updateui-ordering-103/evidence/baseline/baseline-coverage.md` exists and contains:
    - `Timestamp: <ISO-8601>`
    - `Command: <exact command above>`
    - `EXIT_CODE: 0`
    - `Output Summary:` including the numeric QuickFiler line-coverage percentage reported

---

### Phase 1 — Regression Tests + Implementation Fix

- [x] [P1-T1] [expect-fail] Add regression test `LoadConversationInfoAsync_WhenCountExpandedIsZero_CallsUpdateUIWithPairExpandedNotProperty` to `QuickFiler.Test/Helper Classes/ConversationResolverTests.cs`. Confirm it fails before the fix.
  - Precondition: Phase 0 all tasks complete; `ConversationResolverTests.cs` exists.
  - Test scenario: This is a unit test for the async ordering bug. Because `LoadConversationInfoAsync` is internally driven by `Df` and `Count` state, test the observable symptom: accessing `ConversationInfo.Expanded` when `Count.Expanded == 0` throws `InvalidOperationException`. The regression test must verify that **after the fix** calling `UpdateUI` with the constructed pair does NOT re-trigger `LoadConversationInfo()` via the lazy getter. Since the async path is hard to unit-test synchronously, add a synchronous regression covering: when `Count == (0,0)` and `ConversationInfo` is NOT yet set, directly verifying the final `LoadConversationInfo()` guard behavior still throws (confirming we haven't silenced the exception) and that a pre-assigned `ConversationInfo` value is readable without re-triggering load.
  - Note: The async path test can use `Task.Run` wrapping if needed, but if COM mocking is infeasible for the full async flow, test the minimum observable contract: reading `ConversationInfo` before it is set with Count.Expanded==0 throws; reading it after it is set does NOT throw.
  - Acceptance: Test added with `[TestMethod]` attribute; before-fix behavior documented in a fail-before evidence file or via the description in the test.

- [x] [P1-T2] Fix `LoadConversationInfoAsync()` in `QuickFiler/Helper Classes/ConversationResolver.cs`: move `ConversationInfo = pair` **before** the `if (UpdateUI is not null)` block, and change `UpdateUI(ConversationInfo.Expanded)` to `UpdateUI(pair.Expanded)`.
  - Precondition: P1-T1 regression test added.

- [x] [P1-T3] Fix `LoadConversationInfo()` sync path (AC-2): replace the `InvalidOperationException` throw with a single-item fallback when `Count.Expanded <= 0`. Return `new Pair<List<MailItemHelper>>(sameFolder: fallbackList, expanded: fallbackList)` where `fallbackList = [MailHelper]`. Log at `logger.Error` before returning.
  - Acceptance: `LoadConversationInfo()` no longer throws when `Count.Expanded <= 0`; a `logger.Error` call precedes the return.

- [x] [P1-T4] Add/update regression tests (AC-4): update the three existing tests that asserted `InvalidOperationException` throws to assert the new fallback behavior. Rename each to reflect the changed semantics:
  - `LoadConversationInfo_WhenCountExpandedIsZero_ReturnsSingleItemFallbackContainingMailHelper`
  - `ConversationInfoGetter_WhenCountExpandedIsZero_ReturnsSingleItemFallback`
  - `ConversationInfo_WhenNotSetAndCountIsZero_ReturnsFallbackWithoutThrowing`
  - Acceptance: All 8 ConversationResolver tests pass; full suite 82/82 passes.
  - Current code (simplified):
    ```csharp
    // ...build pair...
    if (UpdateUI is not null)
    {
        token.ThrowIfCancellationRequested();
        await UiThread.Dispatcher.InvokeAsync(() => UpdateUI(ConversationInfo.Expanded)); // <-- BUG: reads property before assignment
    }
    // ...
    ConversationInfo = pair;   // <-- too late
    return pair;
    ```
  - Fixed code:
    ```csharp
    // ...build pair...
    ConversationInfo = pair;   // assign first
    if (UpdateUI is not null)
    {
        token.ThrowIfCancellationRequested();
        await UiThread.Dispatcher.InvokeAsync(() => UpdateUI(pair.Expanded)); // use local var, not property
    }
    return pair;
    ```
  - Acceptance: In `LoadConversationInfoAsync()`, `ConversationInfo = pair` appears before any access to `ConversationInfo`; `UpdateUI(pair.Expanded)` uses the local variable; no other behavioral changes.

---

### Phase 2 — Final QC Loop

- [x] [P2-T1] Run `dotnet tool run csharpier format .` and confirm no files changed.
  - Acceptance: File `docs/features/active/2026-03-26-conversation-info-updateui-ordering-103/evidence/qa-gates/qc-format.md` exists and contains:
    - `Timestamp: <ISO-8601>`
    - `Command: dotnet tool run csharpier format .`
    - `EXIT_CODE: 0`
    - `Output Summary:` confirming 0 files changed

- [x] [P2-T2] Run the lint/analyzer build and confirm 0 errors.
  - Command: `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-VSBuild.ps1 -SolutionPath TaskMaster.sln -Configuration Debug -Platform "Any CPU" -EnableNETAnalyzers -EnforceCodeStyleInBuild`
  - Acceptance: File `docs/features/active/2026-03-26-conversation-info-updateui-ordering-103/evidence/qa-gates/qc-lint.md` exists and contains:
    - `Timestamp: <ISO-8601>`
    - `Command: <exact command above>`
    - `EXIT_CODE: 0`
    - `Output Summary:` build succeeded with 0 errors, 0 new warnings relative to baseline

- [x] [P2-T3] Run the nullable/type-check build and confirm 0 errors.
  - Command: `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-VSBuild.ps1 -SolutionPath TaskMaster.sln -Configuration Debug -Platform "Any CPU" -EnableNullable -TreatWarningsAsErrors`
  - Acceptance: File `docs/features/active/2026-03-26-conversation-info-updateui-ordering-103/evidence/qa-gates/qc-nullable.md` exists and contains:
    - `Timestamp: <ISO-8601>`
    - `Command: <exact command above>`
    - `EXIT_CODE: 0`
    - `Output Summary:` build succeeded with 0 errors

- [x] [P2-T4] Run the targeted regression test filter and confirm regression tests pass.
  - Command: `& "C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\CommonExtensions\Microsoft\TestWindow\vstest.console.exe" "QuickFiler.Test\bin\Debug\QuickFiler.Test.dll" /InIsolation /TestCaseFilter:"FullyQualifiedName~ConversationResolver"`
  - Acceptance: File `docs/features/active/2026-03-26-conversation-info-updateui-ordering-103/evidence/qa-gates/qc-regression-tests.md` exists and contains:
    - `Timestamp: <ISO-8601>`
    - `Command: <exact command above>`
    - `EXIT_CODE: 0`
    - `Output Summary:` all `ConversationResolver` tests pass (including newly added test)

- [x] [P2-T5] Run the full QuickFiler.Test suite with coverage enabled and confirm no regressions.
  - Command: `& "C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\CommonExtensions\Microsoft\TestWindow\vstest.console.exe" "QuickFiler.Test\bin\Debug\QuickFiler.Test.dll" /InIsolation /EnableCodeCoverage`
  - Acceptance: File `docs/features/active/2026-03-26-conversation-info-updateui-ordering-103/evidence/qa-gates/qc-coverage.md` exists and contains:
    - `Timestamp: <ISO-8601>`
    - `Command: <exact command above>`
    - `EXIT_CODE: 0`
    - `Output Summary:` all previously passing tests still pass; post-change coverage >= baseline from P0-T6

- [x] [P2-T6] Check off satisfied acceptance criteria in `docs/features/active/2026-03-26-conversation-info-updateui-ordering-103/issue.md`.
  - Acceptance: Each AC item in `issue.md` that is satisfied by the work is checked off (`[x]`); a final AC-status summary is written to this plan.
