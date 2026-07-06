# qfc-high-confidence-empty-batch-crash (Plan)

- **Issue:** #244
- **Parent (optional):** none
- **Owner:** drmoisan
- **Last Updated:** 2026-07-06T15-45
- **Status:** Draft
- **Version:** 1.1
- **Work Mode:** minor-audit

Requirements source: `docs/features/active/2026-07-06-qfc-high-confidence-empty-batch-crash-244/issue.md` (`## Acceptance Criteria`, AC1-AC5). No `spec.md`/`user-story.md` required for this minor-audit bugfix.

Diagnosis artifact: `docs/research/2026-07-06-quickfiler-entryid-column-index-diagnosis.md`.

## Revision Note (v1.1)

The v1.0 regression tests in `QuickFiler.Test/Controllers/QfcInitEmailQueueZeroBatchTests.cs` were confirmed defective and are revised in this version:

1. **Live BackgroundWorker triggering real production UX/COM (blocking).** All three v1.0 tests call `model.InitEmailQueue(..., new BackgroundWorker())`, which starts the real `QfcDatamodel.Worker_DoWork` on a threadpool thread. `Worker_DoWork` runs `LoadRemainingEmailsToQueueAsync(_token)`, which calls `MessageBox.Show("Email Frame is empty")` when `_frame.RowCount == 0` and accesses Outlook COM via `_olApp.GetNamespace("MAPI")`. `InitEmailQueue_PositiveBatchSize_RetainsExistingProjectionAndFrameDrop` drains `_frame` to 0 rows before the worker starts, so it reliably pops the "Email Frame is empty" modal dialog during the test run — the maintainer reported these pop-ups. Unit tests must never trigger real UX or COM (`.claude/rules/general-unit-test.md` UT4, `.claude/rules/csharp.md` Deterministic Test Rules).
2. **`InitEmailQueue_ZeroBatchSize_StillStartsBackgroundWorker` asserted a racing `worker.IsBusy == true` (flaky).** `Worker_DoWork` is `async void`, so `BackgroundWorker` reports the operation complete at its first `await`; the synchronous post-call `IsBusy` check races the worker thread and is not reliably green in every run/filter context.

Fix for the test approach: introduce the smallest DI seam per `.claude/rules/csharp.md` "DI Seams" (injectable delegate seam, seam-preference tier 2) so the worker's loading body is swappable in tests. See "Root Cause Summary" below for the seam design and the "Test Isolation Requirement" it imposes. The production `batchSize <= 0` guard itself (v1.0 fix) is correct and is unchanged by this revision.

---

## Root Cause Summary

`QfcHomeController.RunAsync` sets `initializationBatchSize = 0` in High Confidence mode (`QuickFiler/Controllers/QfcHomeController.cs:281`) and passes it to `QfcDatamodel.InitEmailQueueAsync` -> `InitEmailQueue`. `InitEmailQueue` (`QuickFiler/Controllers/QfcDatamodel.cs:211`) has no `batchSize == 0` guard: it evaluates `_frame.GetRowsAt(new int[0])` (line 217), producing a Deedle frame with an empty column index, then `firstIteration.GetRowsAs<IEmailSortInfo>()` (line 225) throws `"The interface member 'EntryId' does not exist in the column index."`. `_frame` itself is well-formed; the defect is the unconditional slice-and-project block for a zero-size batch.

Fix: add a `batchSize <= 0` short-circuit at the top of `InitEmailQueue`, before the clamp/slice, that still calls `SetupWorker(worker)` and `worker.RunWorkerAsync()` and returns a new empty `List<MailItem>`, and does not touch `_frame.GetRowsAt`/`GetRowsAs<IEmailSortInfo>()`. The `batchSize > 0` branch is left byte-for-byte unchanged. Single production file: `QuickFiler/Controllers/QfcDatamodel.cs`. `QfcHomeController.cs` is not modified (the `batchSize == 0` intent in High-Confidence mode is by design, per issue constraints).

`QfcDatamodel` carries a class-level `[ExcludeFromCodeCoverage]` attribute (`QfcDatamodel.cs:24`), so this change does not add measured lines to the C# coverage denominator; the coverage-no-regression gate (AC5) still applies to the repository totals in Phase 3.

### Worker-body seam and test-isolation requirement (v1.1)

`SetupWorker(worker)` wires `worker.DoWork += Worker_DoWork`, and `Worker_DoWork` (`QfcDatamodel.cs:154`) directly calls `await LoadRemainingEmailsToQueueAsync(_token)` (`QfcDatamodel.cs:168`). `LoadRemainingEmailsToQueueAsync(CancellationToken)` (`QfcDatamodel.cs:269`) calls `MessageBox.Show("Email Frame is empty")` when `_frame.RowCount == 0` and reads Outlook COM via `_olApp.GetNamespace("MAPI")` otherwise. Because `InitEmailQueue` (both the `batchSize <= 0` branch and the `batchSize > 0` branch) always calls `SetupWorker(worker); worker.RunWorkerAsync();`, any test that constructs a real `BackgroundWorker` and calls `InitEmailQueue` inevitably starts this real worker body on a threadpool thread — this is what produced the maintainer-reported pop-ups.

The smallest seam that removes this coupling without changing production behavior is an injectable delegate seam (`.claude/rules/csharp.md` DI Seams, tier 2 — a full interface seam is unnecessary for a single call path): add `internal Func<CancellationToken, Task<bool>> RemainingEmailLoader { get; set; }` to `QfcDatamodel`, defaulted at property-initializer time to the existing instance method `LoadRemainingEmailsToQueueAsync` (method-group conversion resolves unambiguously to the single-argument `LoadRemainingEmailsToQueueAsync(CancellationToken)` overload, since the second overload takes `(BackgroundWorker, CancellationToken)` and does not match the `Func<CancellationToken, Task<bool>>` shape), and change `Worker_DoWork` to invoke `await RemainingEmailLoader(_token)` instead of calling `LoadRemainingEmailsToQueueAsync(_token)` directly. Test instances built via `FormatterServices.GetUninitializedObject(typeof(QfcDatamodel))` (the existing test-fixture pattern) bypass all field/property initializers, so `RemainingEmailLoader` is `null` on such instances until a test assigns it explicitly — this is why every test that starts the worker must assign an inert delegate before calling `InitEmailQueue`. Instances built through the real constructors/`LoadAsync` (production path) run the property initializer normally, so production default behavior — the real loader — is unchanged.

**Test-isolation requirement:** every test in `QuickFiler.Test/Controllers/QfcInitEmailQueueZeroBatchTests.cs` that calls `InitEmailQueue` with a real `BackgroundWorker` MUST first assign `model.RemainingEmailLoader` to an inert, recording delegate (e.g. one that sets a `TaskCompletionSource<bool>` and returns `Task.FromResult(true)`) via the `internal` seam (accessible from `QuickFiler.Test` through the existing `InternalsVisibleTo("QuickFiler.Test")`). No test may allow a real `MessageBox.Show` call or a live COM call (`_olApp.GetNamespace(...)`) on any thread, per this repository's UT4 external-dependency rule and the DI Seams policy. This requirement applies independently of the `batchSize <= 0` guard fix, so the seam must exist and be exercised correctly in both the pre-fix (red) and post-fix (green) states described in Phase 1.

---

### Phase 0 — Policy Read + Baseline Capture

- [x] [P0-T1] Read the mandatory policy files in policy-compliance order and save a policy-read evidence artifact.
  - Files to read (in order):
    1. `CLAUDE.md`
    2. `.claude/rules/general-code-change.md`
    3. `.claude/rules/general-unit-test.md`
    4. `.claude/rules/csharp.md`
  - Acceptance: File `docs/features/active/2026-07-06-qfc-high-confidence-empty-batch-crash-244/evidence/baseline/phase0-instructions-read.md` exists and contains `Timestamp: <ISO-8601>`, a `Policy Order:` list naming all four files in order, and an explicit list of filenames read.

- [x] [P0-T2] Run CSharpier to establish a format baseline and save the artifact.
  - Command: `dotnet tool run csharpier .`
  - Acceptance: File `docs/features/active/2026-07-06-qfc-high-confidence-empty-batch-crash-244/evidence/baseline/baseline-format.md` exists and contains `Timestamp:`, `Command: dotnet tool run csharpier .`, `EXIT_CODE: 0`, and `Output Summary:` confirming no files were reformatted (or, if files were reformatted, that the command was re-run to a clean pass and the final run recorded).

- [x] [P0-T3] Run the analyzer/lint build to establish a lint baseline and save the artifact.
  - Command: `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-VSBuild.ps1 -SolutionPath TaskMaster.sln -Configuration Debug -Platform "Any CPU" -EnableNETAnalyzers -EnforceCodeStyleInBuild`
  - Acceptance: File `docs/features/active/2026-07-06-qfc-high-confidence-empty-batch-crash-244/evidence/baseline/baseline-lint.md` exists and contains `Timestamp:`, `Command:`, `EXIT_CODE: 0`, and `Output Summary:` confirming the build succeeded with 0 analyzer errors.

- [x] [P0-T4] Run the nullable/type-check build to establish a nullable baseline and save the artifact.
  - Command: `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-VSBuild.ps1 -SolutionPath TaskMaster.sln -Configuration Debug -Platform "Any CPU" -EnableNullable -TreatWarningsAsErrors`
  - Acceptance: File `docs/features/active/2026-07-06-qfc-high-confidence-empty-batch-crash-244/evidence/baseline/baseline-nullable.md` exists and contains `Timestamp:`, `Command:`, `EXIT_CODE: 0`, and `Output Summary:` confirming the build succeeded with 0 errors.

- [x] [P0-T5] Run a targeted vstest filter for the not-yet-created regression test names to confirm the baseline test-existence state, and save the artifact.
  - Command: `& "C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\CommonExtensions\Microsoft\TestWindow\vstest.console.exe" "QuickFiler.Test\bin\Debug\QuickFiler.Test.dll" /InIsolation /TestCaseFilter:"FullyQualifiedName~InitEmailQueue_ZeroBatchSize OR FullyQualifiedName~InitEmailQueue_PositiveBatchSize"`
  - Acceptance: File `docs/features/active/2026-07-06-qfc-high-confidence-empty-batch-crash-244/evidence/baseline/baseline-test-filter.md` exists and contains `Timestamp:`, `Command:`, `EXIT_CODE: <recorded integer>`, and `Output Summary:` noting 0 matching tests found (the regression tests do not exist yet at baseline).

- [x] [P0-T6] Run the full `QuickFiler.Test` suite with coverage enabled to establish a numeric coverage baseline and save the artifact.
  - Command: `& "C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\CommonExtensions\Microsoft\TestWindow\vstest.console.exe" "QuickFiler.Test\bin\Debug\QuickFiler.Test.dll" /InIsolation /EnableCodeCoverage`
  - Acceptance: File `docs/features/active/2026-07-06-qfc-high-confidence-empty-batch-crash-244/evidence/baseline/baseline-coverage.md` exists and contains `Timestamp:`, `Command:`, `EXIT_CODE: 0`, and `Output Summary:` including the total test pass count and the numeric `QuickFiler.Test`-scope line-coverage percentage reported by vstest.

---

### Phase 1 — Regression Tests (red) + Minimal Fix + Worker-Body Seam

- [x] [P1-T1] Wire the new regression test file into the legacy `packages.config`-based test project so it participates in the build.
  - File: `QuickFiler.Test\QuickFiler.Test.csproj`
  - Action: Add `<Compile Include="Controllers\QfcInitEmailQueueZeroBatchTests.cs" />` to the existing `<ItemGroup>` that lists `Controllers\QfcDatamodelTests.cs` (matches the project's explicit-`<Compile Include>` convention; there is no glob-based compile).
  - Acceptance: The `<Compile Include="Controllers\QfcInitEmailQueueZeroBatchTests.cs" />` line exists in `QuickFiler.Test.csproj`.

- [x] [P1-T2] Add the `RemainingEmailLoader` injectable-delegate seam to `QfcDatamodel.cs` so `Worker_DoWork` no longer hard-calls the real loader, with no change to production default behavior.
  - File: `QuickFiler/Controllers/QfcDatamodel.cs`
  - Action: In the `#region Private Variables` block (near the existing `TimeProvider` seam property, `QfcDatamodel.cs:109`), add `internal Func<CancellationToken, Task<bool>> RemainingEmailLoader { get; set; } = LoadRemainingEmailsToQueueAsync;` (method-group conversion resolves to the single-argument `LoadRemainingEmailsToQueueAsync(CancellationToken)` overload at `QfcDatamodel.cs:269`, not the two-argument overload at `QfcDatamodel.cs:383`, because only the single-argument overload matches the `Func<CancellationToken, Task<bool>>` shape). In `Worker_DoWork` (`QfcDatamodel.cs:154`), change `e.Result = await LoadRemainingEmailsToQueueAsync(_token);` to `e.Result = await RemainingEmailLoader(_token);`. Do not change `SetupWorker`, the `batchSize <= 0` guard, or any other line in `InitEmailQueue`.
  - Rationale (why this task precedes test-writing): the test file added in P1-T3/P1-T4/P1-T5 must compile against the `RemainingEmailLoader` property, so the seam must exist before those test methods are authored, independent of when the `batchSize <= 0` guard itself is applied (P1-T7).
  - Acceptance: `QfcDatamodel.cs` declares `internal Func<CancellationToken, Task<bool>> RemainingEmailLoader { get; set; } = LoadRemainingEmailsToQueueAsync;`; `Worker_DoWork` invokes `RemainingEmailLoader(_token)` instead of calling `LoadRemainingEmailsToQueueAsync(_token)` directly; the solution builds with 0 new errors; `QfcHomeController.cs` is not modified.

- [x] [P1-T3] [expect-fail] Create `QuickFiler.Test\Controllers\QfcInitEmailQueueZeroBatchTests.cs` with a `[TestClass]` `QfcInitEmailQueueZeroBatchTests`, a private `Frame<int, string>` fixture builder (`Frame.FromRecords` of 2 anonymous records with members `EntryId`, `MessageClass`, `SentOn`, `ConversationId`, `Triage`, `StoreId`, matching `IEmailSortInfo`), a private `CreateUninitializedDatamodel()`/`SetPrivateField(object, string, object)` pair (mirroring the pattern already used in `QfcDatamodelTests.cs` lines 229-239: `FormatterServices.GetUninitializedObject(typeof(QfcDatamodel))` plus reflection field assignment), and a private helper `CreateInertRemainingEmailLoader(out TaskCompletionSource<bool> invoked)` that returns a `Func<CancellationToken, Task<bool>>` which calls `invoked.TrySetResult(true)` and returns `Task.FromResult(true)` without touching `MessageBox` or `_olApp`. Add one test method `InitEmailQueue_ZeroBatchSize_ReturnsEmptyListWithoutThrowing` that builds a model via `CreateUninitializedDatamodel()`, sets `_frame` to the 2-row fixture via `SetPrivateField`, assigns `model.RemainingEmailLoader = CreateInertRemainingEmailLoader(out _)` (through the `internal` seam, visible via `InternalsVisibleTo("QuickFiler.Test")`), calls `model.InitEmailQueue(0, new BackgroundWorker())`, and asserts (FluentAssertions) the call does not throw and the returned list is not null and is empty. Run it and confirm it fails today with the Deedle `"The interface member 'EntryId' does not exist in the column index."` exception (the `batchSize <= 0` guard does not exist yet; the seam assignment is present but never reached because the exception occurs before `SetupWorker`/`RunWorkerAsync`, so no live UX/COM occurs even in this red state).
  - Acceptance: Evidence artifact `docs/features/active/2026-07-06-qfc-high-confidence-empty-batch-crash-244/evidence/regression-testing/fail-before-InitEmailQueue-zero-batch.2026-07-06T15-45.md` exists and contains `Timestamp:`, `Command: <targeted vstest filter command from P1-T6>`, `EXIT_CODE: <non-zero>`, `Output Summary:` quoting the pre-fix exception message, confirming the test is red, and confirming no `MessageBox.Show` pop-up or COM call occurred during the run.

- [x] [P1-T4] [expect-fail] Add a second test method `InitEmailQueue_ZeroBatchSize_StillStartsBackgroundWorker` to `QuickFiler.Test\Controllers\QfcInitEmailQueueZeroBatchTests.cs` that builds a model via `CreateUninitializedDatamodel()`, sets `_frame` to the same 2-row fixture, assigns `model.RemainingEmailLoader = CreateInertRemainingEmailLoader(out var loaderInvokedTcs)`, constructs a real `new BackgroundWorker()`, calls `model.InitEmailQueue(0, worker)`, and asserts (FluentAssertions) `worker.WorkerSupportsCancellation.Should().BeTrue()` (a synchronous side effect of `SetupWorker`) AND `loaderInvokedTcs.Task.Wait(TimeSpan.FromSeconds(5)).Should().BeTrue("the injected RemainingEmailLoader must be invoked by the started worker")` (a bounded, deterministic wait on the injected delegate's completion signal, not a fixed sleep). Do NOT assert `worker.IsBusy` (removed: `Worker_DoWork` is `async void`, so `BackgroundWorker` reports the operation complete at its first `await`, making a synchronous post-call `IsBusy` check a race). Do NOT use `Thread.Sleep`/`Task.Delay`/fixed sleeps anywhere in this test. Run it and confirm it fails today (the pre-guard code throws before reaching `SetupWorker`/`RunWorkerAsync`, so neither assertion is reached).
  - Acceptance: Evidence artifact `docs/features/active/2026-07-06-qfc-high-confidence-empty-batch-crash-244/evidence/regression-testing/fail-before-InitEmailQueue-worker-start.2026-07-06T15-45.md` exists and contains `Timestamp:`, `Command:`, `EXIT_CODE: <non-zero>`, and `Output Summary:` confirming the test is red today and that the test contains no `worker.IsBusy` assertion and no `Thread.Sleep`/`Task.Delay` call.

- [x] [P1-T5] Add a non-expect-fail characterization test `InitEmailQueue_PositiveBatchSize_RetainsExistingProjectionAndFrameDrop` to `QuickFiler.Test\Controllers\QfcInitEmailQueueZeroBatchTests.cs` that builds a model via `CreateUninitializedDatamodel()`, sets `_frame` to a 2-row fixture and `_olApp` to a `Mock<Outlook.Application>` whose `GetNamespace("MAPI")` returns a `Mock<NameSpace>` whose `GetItemFromID(It.IsAny<string>(), It.IsAny<object>())` returns a distinct `Mock<MailItem>().Object` per row (mirrors the existing `Mock<Application>`/`GetNamespace("MAPI")` pattern in `QuickFiler.Test\Helper Classes\ConversationResolverTests.cs:484-504`), assigns `model.RemainingEmailLoader = CreateInertRemainingEmailLoader(out _)` before the call (so the drained-frame worker started by `InitEmailQueue`'s trailing `SetupWorker`/`RunWorkerAsync` cannot reach the real `LoadRemainingEmailsToQueueAsync` and pop the "Email Frame is empty" `MessageBox.Show` dialog against the now-empty `_frame`), calls `model.InitEmailQueue(2, new BackgroundWorker())`, and asserts the returned list has exactly 2 items matching the mocked `MailItem` instances and that the private `_frame` field (read via reflection) now has `RowCount == 0`. This test must pass both before and after the fix, proving the `batchSize > 0` path is unchanged, and must never trigger a `MessageBox.Show` pop-up in either state.
  - Acceptance: Test method exists, assigns the inert `RemainingEmailLoader` before calling `InitEmailQueue`, and, when run in isolation today (pre-fix), passes with no pop-up dialog (it does not exercise the `batchSize == 0` branch, and the guard's absence does not affect this branch).

- [x] [P1-T6] Run the targeted vstest filter for all three new test methods and confirm the expected mixed red/green baseline before the fix, with no pop-up dialogs.
  - Command: `& "C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\CommonExtensions\Microsoft\TestWindow\vstest.console.exe" "QuickFiler.Test\bin\Debug\QuickFiler.Test.dll" /InIsolation /TestCaseFilter:"FullyQualifiedName~InitEmailQueue_ZeroBatchSize OR FullyQualifiedName~InitEmailQueue_PositiveBatchSize"`
  - Acceptance: File `docs/features/active/2026-07-06-qfc-high-confidence-empty-batch-crash-244/evidence/regression-testing/pre-fix-test-run.2026-07-06T15-45.md` exists and contains `Timestamp:`, `Command:`, `EXIT_CODE: <non-zero>`, and `Output Summary:` recording `InitEmailQueue_ZeroBatchSize_ReturnsEmptyListWithoutThrowing` = Failed, `InitEmailQueue_ZeroBatchSize_StillStartsBackgroundWorker` = Failed, `InitEmailQueue_PositiveBatchSize_RetainsExistingProjectionAndFrameDrop` = Passed (1 passed, 2 failed), and explicitly confirming no `MessageBox.Show` pop-up occurred during the run.

- [x] [P1-T7] Apply the minimal fix to `QuickFiler/Controllers/QfcDatamodel.cs`: add a `batchSize <= 0` short-circuit at the top of `InitEmailQueue` (immediately after `_worker = worker;`, before the existing `batchSize = batchSize < _frame.RowCount ...` clamp line) that calls `SetupWorker(worker); worker.RunWorkerAsync();` and returns `new List<MailItem>()`, leaving the existing `batchSize > 0` code path (clamp, `GetRowsAt`, frame drop, `GetRowsAs<IEmailSortInfo>()`, `GetItemFromID` projection, `SetupWorker`/`RunWorkerAsync`, return) textually unchanged below the guard.
  - Acceptance: `InitEmailQueue` contains an `if (batchSize <= 0) { ... return new List<MailItem>(); }` block before the clamp line; no line in the pre-existing `batchSize > 0` body is modified; `QfcHomeController.cs` is not modified; this task is independent of and unaffected by the P1-T2 seam (already verified true in the current repository state — the guard exists at `QfcDatamodel.cs:219-224`).

---

### Phase 2 — Verification (green)

- [x] [P2-T1] Re-run the targeted vstest filter for the three regression tests AND the full `QuickFiler.Test` suite, and confirm all three regression tests pass in BOTH runs with no pop-up dialogs and no live COM calls.
  - Narrow-filter command: `& "C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\CommonExtensions\Microsoft\TestWindow\vstest.console.exe" "QuickFiler.Test\bin\Debug\QuickFiler.Test.dll" /InIsolation /TestCaseFilter:"FullyQualifiedName~InitEmailQueue_ZeroBatchSize OR FullyQualifiedName~InitEmailQueue_PositiveBatchSize"`
  - Full-suite command: `& "C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\CommonExtensions\Microsoft\TestWindow\vstest.console.exe" "QuickFiler.Test\bin\Debug\QuickFiler.Test.dll" /InIsolation /EnableCodeCoverage`
  - Acceptance: File `docs/features/active/2026-07-06-qfc-high-confidence-empty-batch-crash-244/evidence/regression-testing/post-fix-test-run.2026-07-06T15-45.md` exists and contains `Timestamp:`, both `Command:` lines, `EXIT_CODE: 0` for each run, and `Output Summary:` recording all three tests Passed (3 passed, 0 failed) in the narrow-filter run AND passed in the full-suite run, and explicitly confirming no `MessageBox.Show` pop-up and no live `_olApp`/COM call occurred in either run. This satisfies AC1, AC2, AC3, and AC4 with no context-dependent caveat (the v1.0 narrow-filter/`IsBusy` caveat recorded against AC2/AC4 in `issue.md` no longer applies once this task is complete and is removed by P3-T5).

---

### Phase 3 — Final QA Loop

- [x] [P3-T1] Run CSharpier and confirm no files change.
  - Command: `dotnet tool run csharpier .`
  - Acceptance: File `docs/features/active/2026-07-06-qfc-high-confidence-empty-batch-crash-244/evidence/qa-gates/qc-format.md` exists and contains `Timestamp:`, `Command:`, `EXIT_CODE: 0`, and `Output Summary:` confirming 0 files changed. If files changed, this task is not complete until re-run to a clean pass and the final clean run is the one recorded.

- [x] [P3-T2] Run the analyzer/lint build and confirm 0 errors.
  - Command: `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-VSBuild.ps1 -SolutionPath TaskMaster.sln -Configuration Debug -Platform "Any CPU" -EnableNETAnalyzers -EnforceCodeStyleInBuild`
  - Acceptance: File `docs/features/active/2026-07-06-qfc-high-confidence-empty-batch-crash-244/evidence/qa-gates/qc-lint.md` exists and contains `Timestamp:`, `Command:`, `EXIT_CODE: 0`, and `Output Summary:` confirming the build succeeded with 0 errors and 0 new warnings relative to the P0-T3 baseline.

- [x] [P3-T3] Run the nullable/type-check build and confirm 0 errors.
  - Command: `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-VSBuild.ps1 -SolutionPath TaskMaster.sln -Configuration Debug -Platform "Any CPU" -EnableNullable -TreatWarningsAsErrors`
  - Acceptance: File `docs/features/active/2026-07-06-qfc-high-confidence-empty-batch-crash-244/evidence/qa-gates/qc-nullable.md` exists and contains `Timestamp:`, `Command:`, `EXIT_CODE: 0`, and `Output Summary:` confirming the build succeeded with 0 errors relative to the P0-T4 baseline.

- [x] [P3-T4] Run the full `QuickFiler.Test` suite with coverage enabled and confirm no regressions and a numeric coverage comparison against baseline.
  - Command: `& "C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\CommonExtensions\Microsoft\TestWindow\vstest.console.exe" "QuickFiler.Test\bin\Debug\QuickFiler.Test.dll" /InIsolation /EnableCodeCoverage`
  - Acceptance: File `docs/features/active/2026-07-06-qfc-high-confidence-empty-batch-crash-244/evidence/qa-gates/qc-coverage.md` exists and contains `Timestamp:`, `Command:`, `EXIT_CODE: 0`, and `Output Summary:` recording: total pass count (including the 3 regression tests, all previously-passing tests still passing), the baseline coverage percentage from P0-T6, the post-change coverage percentage, a delta showing coverage is not lower than baseline, and confirmation that no `MessageBox.Show` pop-up occurred during the run (satisfies AC5's changed-line-coverage-no-regression requirement for the `QuickFiler.Test` scope; `QfcDatamodel` is excluded from measurement by its existing class-level `[ExcludeFromCodeCoverage]` attribute, so neither the guard's lines nor the `RemainingEmailLoader` seam's lines are part of the measured denominator).

- [x] [P3-T5] Check off satisfied acceptance criteria in `docs/features/active/2026-07-06-qfc-high-confidence-empty-batch-crash-244/issue.md` and record a final AC status summary, removing the v1.0 narrow-filter/`IsBusy`-race caveats from AC2 and AC4 since the seam-based fix (P1-T2 through P2-T1) makes all three regression tests deterministically green in every run context.
  - Acceptance: AC1-AC5 in the `## Acceptance Criteria` section of `issue.md` are each marked `[x]` with the satisfying v1.1 evidence artifact path noted inline or in an adjacent summary, and any caveat text referencing a narrow-filter `IsBusy` race or a context-dependent AC2/AC4 result is removed or explicitly superseded; a mirrored copy of the update is written to `docs/features/active/2026-07-06-qfc-high-confidence-empty-batch-crash-244/evidence/issue-updates/issue-244.2026-07-06T15-45.md` per the issue-update mirroring convention (`Timestamp:`, exact text, `PostedAs:`).

---

## Acceptance Criteria Mapping

- **AC1** (`InitEmailQueue(0, worker)` returns empty, non-null list without throwing): P1-T3, P1-T7, P2-T1.
- **AC2** (zero-batch call still sets up and starts the background worker): P1-T2, P1-T4, P1-T7, P2-T1.
- **AC3** (`batchSize > 0` retains existing projection/frame-drop behavior): P1-T5, P1-T7, P2-T1.
- **AC4** (deterministic, Outlook-free regression test reproduces failure red-before-fix and passes green-after-fix, with no live UX/COM in either state): P1-T2, P1-T3, P1-T4, P1-T5, P1-T6, P2-T1.
- **AC5** (full C# toolchain passes; changed-line coverage does not regress): P3-T1, P3-T2, P3-T3, P3-T4, P3-T5.

---

DIRECTIVE: PREFLIGHT VALIDATION ONLY
