# onedrive-writer-timeout-test-determinism (Plan)

- **Issue:** #253
- **Issue URL:** https://github.com/drmoisan/TaskMaster/issues/253
- **Parent (optional):** none
- **Owner:** drmoisan
- **Last Updated:** 2026-07-07T14-05
- **Status:** Draft
- **Version:** 1.0
- **Work Mode:** minor-audit
- **Requirements Source:** `docs/features/active/2026-07-07-onedrive-writer-timeout-test-determinism-253/issue.md` (`## Acceptance Criteria` AC1-AC5)

**Fail-closed evidence rule:** This plan includes explicit baseline artifact tasks, final-QA artifact tasks, and a coverage no-regression task for the single in-scope language (C#). If any required baseline, QA, or coverage-comparison artifact is missing, or its required fields (`Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`) are incomplete, the audit verdict must be BLOCKED or INCOMPLETE, never PASS.

**Evidence accounting rule:** Each evidence-producing task names its exact artifact path under `docs/features/active/2026-07-07-onedrive-writer-timeout-test-determinism-253/evidence/<kind>/`. Do not mark an evidence-backed task complete without the artifact on disk.

## Requirements Boundary

This minor-audit plan uses only `docs/features/active/2026-07-07-onedrive-writer-timeout-test-determinism-253/issue.md` as the requirements source. Acceptance criteria are limited to the checkbox items (AC1-AC5) under that file's explicit `## Acceptance Criteria` section (confirmed present). `spec.md` and `user-story.md` are not required for minor-audit mode; if either is unexpectedly present in the feature folder, that is a fail-closed condition and must be reported, not silently ignored.

Adopted fix (research Section 3, Option (a) — the only fix approved for this issue): introduce the smallest injectable delegate seam on `OneDriveDownloader` around the nondeterministic timeout boundary inside `TryGetFileStreamWriter`, defaulting to the exact current production call, and rewrite the flaky test to inject a deterministic (no `Task.Run`, no `CancellationTokenSource`) runner. Expected touched files are limited to:

- `UtilitiesCS/OneDriveHelpers/OneDriveDownloader.cs` (sole production file)
- `UtilitiesCS.Test/OneDriveHelpers/OneDriveDownloader_Tests.cs` (test-only seam setter + rewritten tests)
- `docs/features/active/2026-07-07-onedrive-writer-timeout-test-determinism-253/issue.md` (AC checkbox status updates only)

`UtilitiesCS/Threading/TimeOutTask.cs` is explicitly OUT OF SCOPE. The separate exception-type mismatch defect in the `Func<T1,TResult>` synchronous overload of `RunWithTimeout` (research Section 2.1 / Section 3 Option (c)) is not fixed under issue #253; it is recorded as a follow-up item for a new issue in Phase 2.

**Bugfix-workflow nuance (intermittent thread-pool/timer race):** The reported failure is a race between a real `CancellationTokenSource(5000)` timer and thread-pool dispatch of the writer-factory delegate (research Section 1.3); it manifests only under thread-pool starvation (Visual Studio parallel test host) and is not reliably reproducible as a deterministic fail-then-pass transition. Per `evidence-and-timestamp-conventions`, Phase 0 captures the failure mode via a fail-before exception dossier rather than a deterministic failing run, and the fix itself (Phase 1) removes the race by construction rather than by masking it with retries/timing changes (prohibited by `.claude/rules/csharp.md`).

All evidence must be written under `docs/features/active/2026-07-07-onedrive-writer-timeout-test-determinism-253/evidence/<kind>/`.

## Confirmed Facts (from source inspection and research, recorded for the Phase 0 investigation task)

- `OneDriveDownloader.TryGetFileStreamWriter` (`UtilitiesCS/OneDriveHelpers/OneDriveDownloader.cs:82-103`) is `public virtual async Task<Stream>` and wraps `await GetFileStreamWriter.RunWithTimeout(destinationPath, cancel, timeoutMs, 3, false);` inside `try { } catch (Exception) { return null; }`.
- `GetFileStreamWriter` (`OneDriveDownloader.cs:105-118`) is an existing `public virtual Func<string, Stream>` property with a private backing field and a `protected set`, defaulting to a real `new FileStream(...)` call. This is the class's existing style for injectable delegate seams (mirrored by `ClientGetAsync`, `OneDriveDownloader.cs:33-38`).
- `TimeOutTask.RunWithTimeout<T1, TResult>` (`UtilitiesCS/Threading/TimeOutTask.cs:164-174`, extension method `this Func<T1, TResult> function, T1 arg1, CancellationToken token, int milliseconds, int maxAttempts, bool strict`) is the exact overload invoked; its private implementation (lines 176-229) drives a real `CancellationTokenSource` and `Task.Run`, which is the nondeterministic boundary.
- `TestableOneDriveDownloader` (`UtilitiesCS.Test/OneDriveHelpers/OneDriveDownloader_Tests.cs:14-27`) is an `internal class` deriving from `OneDriveDownloader` with `SetClientGetAsync` and `SetFileStreamWriter` test-only setters that assign the protected-settable virtual properties. `SetWriterTimeoutRunner` must follow the identical pattern.
- The affected test `TryGetFileStreamWriter_WhenWriterReturnsMemoryStream_ReturnsStream` (`OneDriveDownloader_Tests.cs:227-237`) sets `GetFileStreamWriter` to `_ => new MemoryStream()` and asserts the returned stream `Should().NotBeNull()` and `CanWrite.Should().BeTrue()`; this assertion only holds today if the queued `Task.Run` delegate executes before the real timer cancels it (research Section 1.4).
- The sibling test `TryGetFileStreamWriter_WhenWriterThrows_ReturnsNull` (`OneDriveDownloader_Tests.cs:250-258`) already passes deterministically under both race outcomes because both converge on `null` (research Section 1.4); it may optionally also inject the new deterministic runner for consistency, without changing its assertions.
- `UtilitiesCS/Threading/TimeOutTask.cs` and every `TimeOutTask_*` test file are unmodified by this fix; no regression risk is introduced there.

---

### Phase 0 — Policy and Baseline Evidence

- [x] [P0-T1] Record policy-read evidence for issue #253 before implementation begins.
  - Files read (in order): `CLAUDE.md`, `.claude/rules/general-code-change.md`, `.claude/rules/general-unit-test.md`, `.claude/rules/csharp.md`, `.claude/skills/atomic-plan-contract/SKILL.md`, `.claude/skills/evidence-and-timestamp-conventions/SKILL.md`, `.claude/skills/acceptance-criteria-tracking/SKILL.md`, `docs/features/active/2026-07-07-onedrive-writer-timeout-test-determinism-253/issue.md`
  - Evidence: `docs/features/active/2026-07-07-onedrive-writer-timeout-test-determinism-253/evidence/baseline/phase0-instructions-read.md`
  - Acceptance: Evidence file exists and contains `Timestamp:`, `Policy Order:`, and the explicit list of files read above, in order.

- [x] [P0-T2] Verify the minor-audit requirements boundary for issue #253.
  - Files: `docs/features/active/2026-07-07-onedrive-writer-timeout-test-determinism-253/issue.md` (and confirm the presence/absence of `spec.md`, `user-story.md` in the same folder)
  - Evidence: `docs/features/active/2026-07-07-onedrive-writer-timeout-test-determinism-253/evidence/baseline/minor-audit-scope.2026-07-07T14-05.md`
  - Acceptance: Evidence confirms `issue.md` contains `- Work Mode: minor-audit`, contains an explicit `## Acceptance Criteria` section listing AC1-AC5, treats only that section as the AC source, and records whether `spec.md`/`user-story.md` are present or absent in the feature folder (fail-closed if unexpectedly present).

- [x] [P0-T3] Record investigation evidence confirming the production call chain and existing seam pattern needed to design the fix, citing the research artifact.
  - Files: `UtilitiesCS/OneDriveHelpers/OneDriveDownloader.cs`, `UtilitiesCS/Threading/TimeOutTask.cs`, `UtilitiesCS.Test/OneDriveHelpers/OneDriveDownloader_Tests.cs`, `docs/features/active/2026-07-07-onedrive-writer-timeout-test-determinism-253/research/2026-07-07T13-00-onedrive-writer-timeout-research.md`
  - Evidence: `docs/features/active/2026-07-07-onedrive-writer-timeout-test-determinism-253/evidence/baseline/investigation-notes.2026-07-07T14-05.md`
  - Acceptance: Evidence records, with file:line citations: (a) `TryGetFileStreamWriter`'s current call to `GetFileStreamWriter.RunWithTimeout(...)` (`OneDriveDownloader.cs:88-102`); (b) the existing `GetFileStreamWriter`/`ClientGetAsync` virtual-delegate-property pattern (backing field + getter + protected setter) to be mirrored by the new seam; (c) the `RunWithTimeout<T1, TResult>` extension signature (`TimeOutTask.cs:164-174`) that the new seam's default implementation must call unchanged; (d) confirmation that `TimeOutTask.cs` is out of scope and will not be modified.

- [x] [P0-T4] Run the baseline C# formatting command.
  - Files: `UtilitiesCS/OneDriveHelpers/OneDriveDownloader.cs`, `UtilitiesCS.Test/OneDriveHelpers/OneDriveDownloader_Tests.cs`
  - Command: `dotnet tool run csharpier .`
  - Evidence: `docs/features/active/2026-07-07-onedrive-writer-timeout-test-determinism-253/evidence/baseline/csharpier-baseline.2026-07-07T14-05.md`
  - Acceptance: Evidence contains `Timestamp:`, `Command: dotnet tool run csharpier .`, `EXIT_CODE:`, and `Output Summary:` stating whether any files were changed.

- [x] [P0-T5] Run the baseline C# analyzer build command.
  - Files: `TaskMaster.sln`, `UtilitiesCS/OneDriveHelpers/OneDriveDownloader.cs`
  - Command: `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
  - Evidence: `docs/features/active/2026-07-07-onedrive-writer-timeout-test-determinism-253/evidence/baseline/csharp-analyzers-baseline.2026-07-07T14-05.md`
  - Acceptance: Evidence contains `Timestamp:`, the exact `Command:`, `EXIT_CODE:`, and `Output Summary:` with the warning/error count or primary diagnostic.

- [x] [P0-T6] Run the baseline C# nullable build command.
  - Files: `TaskMaster.sln`, `UtilitiesCS/OneDriveHelpers/OneDriveDownloader.cs`
  - Command: `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true`
  - Evidence: `docs/features/active/2026-07-07-onedrive-writer-timeout-test-determinism-253/evidence/baseline/csharp-nullable-baseline.2026-07-07T14-05.md`
  - Acceptance: Evidence contains `Timestamp:`, the exact `Command:`, `EXIT_CODE:`, and `Output Summary:` with the warning/error count or primary diagnostic.

- [x] [P0-T7] Capture the flaky failure mode as a fail-before exception dossier (a deterministic fail-before run is structurally impossible for this thread-pool/timer race).
  - Files: `docs/features/active/2026-07-07-onedrive-writer-timeout-test-determinism-253/issue.md` (Logs/Screenshots and Suspected Cause sections)
  - Evidence: `docs/features/active/2026-07-07-onedrive-writer-timeout-test-determinism-253/evidence/regression-testing/fail-before-exception.2026-07-07T14-05.md`
  - Acceptance: Evidence contains `Timestamp:`, `WhyFailingRunImpossible:` (the failure requires thread-pool starvation under the Visual Studio parallel test host and is intermittent — it cannot be deterministically forced without violating the no-timing-hack policy), and an alternative-proof section quoting the observed `~18s` duration and `Expected stream not to be <null>` failure from `issue.md`.

- [x] [P0-T8] Run the baseline MSTest coverage command for the full `UtilitiesCS.Test` suite, including the targeted `OneDriveDownloader_Tests` and `TimeOutTask_*` classes.
  - Files: `UtilitiesCS.Test/bin/Debug/UtilitiesCS.Test.dll`
  - Command: `vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /EnableCodeCoverage`
  - Evidence: `docs/features/active/2026-07-07-onedrive-writer-timeout-test-determinism-253/evidence/baseline/csharp-vstest-coverage-baseline.2026-07-07T14-05.md`
  - Acceptance: Evidence contains `Timestamp:`, the exact `Command:`, `EXIT_CODE:`, and `Output Summary:` with total tests, pass/fail counts, the numeric baseline coverage headline percentage, and the observed pass/fail/duration of `TryGetFileStreamWriter_WhenWriterReturnsMemoryStream_ReturnsStream` in this run (pass or fail; a single run does not by itself prove or disprove flakiness).

---

### Phase 1 — Constrained Implementation (Injectable Delegate Seam + Deterministic Test Rewrite)

- [x] [P1-T1] Add the `WriterTimeoutRunner` virtual delegate property to `OneDriveDownloader`, mirroring the existing `GetFileStreamWriter`/`ClientGetAsync` pattern.
  - Files: `UtilitiesCS/OneDriveHelpers/OneDriveDownloader.cs`
  - Precondition: Phase 0 complete.
  - Change: Add a backing field `protected Func<Func<string, Stream>, string, CancellationToken, int, Task<Stream>> _writerTimeoutRunner = (factory, destinationPath, cancel, timeoutMs) => factory.RunWithTimeout(destinationPath, cancel, timeoutMs, 3, false);` and a property `public virtual Func<Func<string, Stream>, string, CancellationToken, int, Task<Stream>> WriterTimeoutRunner { get => _writerTimeoutRunner; protected set => _writerTimeoutRunner = value; }`, placed adjacent to the existing `GetFileStreamWriter` property.
  - Acceptance: `OneDriveDownloader.cs` compiles with the new property; the default delegate body is byte-for-byte equivalent to the current call `GetFileStreamWriter.RunWithTimeout(destinationPath, cancel, timeoutMs, 3, false)` (same arguments, same order, same literal `3` and `false`). No other property or method in the file is changed by this task. Satisfies the seam-introduction half of AC2.

- [x] [P1-T2] Route `TryGetFileStreamWriter` through the new `WriterTimeoutRunner` seam instead of calling `RunWithTimeout` directly.
  - Files: `UtilitiesCS/OneDriveHelpers/OneDriveDownloader.cs`
  - Precondition: P1-T1 complete.
  - Change: Inside the existing `try` block of `TryGetFileStreamWriter` (`OneDriveDownloader.cs:88-97`), replace `var stream = await GetFileStreamWriter.RunWithTimeout(destinationPath, cancel, timeoutMs, 3, false);` with `var stream = await WriterTimeoutRunner(GetFileStreamWriter, destinationPath, cancel, timeoutMs);`. The surrounding `try`/`catch (Exception) { return null; }` block, method signature, and `return stream;` line are unchanged.
  - Acceptance: `TryGetFileStreamWriter`'s only change is the single call-site substitution described above; the method remains `public virtual async Task<Stream>` with identical parameters; `catch (Exception)` still returns `null`. With `WriterTimeoutRunner` left at its default (P1-T1), production behavior is identical to pre-change behavior for every existing caller. Satisfies AC2.

- [x] [P1-T3] Add `SetWriterTimeoutRunner` to `TestableOneDriveDownloader`, mirroring `SetClientGetAsync`/`SetFileStreamWriter`.
  - Files: `UtilitiesCS.Test/OneDriveHelpers/OneDriveDownloader_Tests.cs`
  - Precondition: P1-T2 complete.
  - Change: Add `public void SetWriterTimeoutRunner(Func<Func<string, Stream>, string, CancellationToken, int, Task<Stream>> func) { WriterTimeoutRunner = func; }` to the `TestableOneDriveDownloader` class (`OneDriveDownloader_Tests.cs:14-27`), directly beneath `SetFileStreamWriter`.
  - Acceptance: `TestableOneDriveDownloader` exposes `SetWriterTimeoutRunner` with the exact signature above; no other member of `TestableOneDriveDownloader` or `TestableOneDriveDownloaderFull` is changed.

- [x] [P1-T4] Rewrite `TryGetFileStreamWriter_WhenWriterReturnsMemoryStream_ReturnsStream` to inject a synchronous, deterministic `WriterTimeoutRunner` (no `Task.Run`, no `CancellationTokenSource`).
  - Files: `UtilitiesCS.Test/OneDriveHelpers/OneDriveDownloader_Tests.cs`
  - Precondition: P1-T3 complete.
  - Change: In the test method (`OneDriveDownloader_Tests.cs:227-237`), after `downloader.SetFileStreamWriter(_ => new MemoryStream());`, add `downloader.SetWriterTimeoutRunner((factory, path, token, ms) => Task.FromResult(factory(path)));`. Keep the existing `using var stream = await downloader.TryGetFileStreamWriter("ignored", 5000, default);`, `stream.Should().NotBeNull();`, and `stream.CanWrite.Should().BeTrue();` lines unchanged.
  - Acceptance: The rewritten test contains no reference to `Task.Run` or `CancellationTokenSource`, still calls the real `TryGetFileStreamWriter` method body and the real `GetFileStreamWriter` factory substitution, and its outcome no longer depends on thread-pool scheduling or a real timer. Satisfies AC1.

- [x] [P1-T5] Inject the deterministic `WriterTimeoutRunner` into `TryGetFileStreamWriter_WhenWriterThrows_ReturnsNull` for consistency, preserving its existing assertion.
  - Files: `UtilitiesCS.Test/OneDriveHelpers/OneDriveDownloader_Tests.cs`
  - Precondition: P1-T4 complete.
  - Change: In the test method (`OneDriveDownloader_Tests.cs:250-258`), after `downloader.SetFileStreamWriter(_ => throw new InvalidOperationException("boom"));`, add `downloader.SetWriterTimeoutRunner((factory, path, token, ms) => Task.FromResult(factory(path)));`. Keep `var stream = await downloader.TryGetFileStreamWriter("ignored", 5000, default);` and `stream.Should().BeNull();` unchanged.
  - Acceptance: The test still asserts `stream.Should().BeNull();` and now runs the writer factory synchronously through the injected runner, with no dependency on `RunWithTimeout`'s real timer/thread-pool path. Satisfies the deterministic half of AC3 (combined with P1-T4, both writer-returns-stream and writer-throws are covered deterministically).

- [x] [P1-T6] Record implementation-scope evidence confirming only the two approved files were changed.
  - Files: `UtilitiesCS/OneDriveHelpers/OneDriveDownloader.cs`, `UtilitiesCS.Test/OneDriveHelpers/OneDriveDownloader_Tests.cs`
  - Evidence: `docs/features/active/2026-07-07-onedrive-writer-timeout-test-determinism-253/evidence/regression-testing/implementation-scope.2026-07-07T14-05.md`
  - Acceptance: Evidence lists every changed file (via `git diff --stat`) and confirms the only production file changed is `UtilitiesCS/OneDriveHelpers/OneDriveDownloader.cs`, the only test file changed is `UtilitiesCS.Test/OneDriveHelpers/OneDriveDownloader_Tests.cs`, and `UtilitiesCS/Threading/TimeOutTask.cs` and every `TimeOutTask_*` test file are unmodified.

- [x] [P1-T7] Run the targeted `OneDriveDownloader_Tests` class with coverage and confirm all tests pass, including the rewritten test with a sub-second duration.
  - Files: `UtilitiesCS.Test/bin/Debug/UtilitiesCS.Test.dll`, `UtilitiesCS.Test/OneDriveHelpers/OneDriveDownloader_Tests.cs`
  - Command: `vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /TestCaseFilter:"FullyQualifiedName~OneDriveDownloader_Tests" /EnableCodeCoverage`
  - Evidence: `docs/features/active/2026-07-07-onedrive-writer-timeout-test-determinism-253/evidence/regression-testing/targeted-vstest-coverage.2026-07-07T14-05.md`
  - Acceptance: Evidence contains `Timestamp:`, the exact `Command:`, `EXIT_CODE: 0`, and `Output Summary:` confirming every test in `OneDriveDownloader_Tests` passes, with `TryGetFileStreamWriter_WhenWriterReturnsMemoryStream_ReturnsStream` and `TryGetFileStreamWriter_WhenWriterThrows_ReturnsNull` each completing in well under one second (no multi-second duration). Satisfies AC1, AC3 (verification half), and the single-run portion of AC4.

- [x] [P1-T8] Repeat the targeted `OneDriveDownloader_Tests` run at least 9 additional consecutive times to demonstrate deterministic, non-flaky pass behavior.
  - Files: `UtilitiesCS.Test/bin/Debug/UtilitiesCS.Test.dll`
  - Command (per run): `vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /TestCaseFilter:"FullyQualifiedName~OneDriveDownloader_Tests"`
  - Evidence: `docs/features/active/2026-07-07-onedrive-writer-timeout-test-determinism-253/evidence/regression-testing/determinism-repeated-runs.2026-07-07T14-05.md`
  - Acceptance: Evidence records `Timestamp:`, `Command:`, and a per-run `EXIT_CODE:` for at least 10 total consecutive runs (including P1-T7), with every run passing and no run exceeding one second for `TryGetFileStreamWriter_WhenWriterReturnsMemoryStream_ReturnsStream`. Satisfies AC1 and AC4 (VS Code / CLI-runner portion).

- [x] [P1-T9] Record a follow-up-issue note for the out-of-scope `TimeOutTask.cs` exception-type mismatch defect (research Section 2.1).
  - Files: `docs/features/active/2026-07-07-onedrive-writer-timeout-test-determinism-253/research/2026-07-07T13-00-onedrive-writer-timeout-research.md` (Section 2.1, Section 3 Option (c))
  - Evidence: `docs/features/active/2026-07-07-onedrive-writer-timeout-test-determinism-253/evidence/other/follow-up-issue-note.2026-07-07T14-05.md`
  - Acceptance: Evidence states that `TimeOutTask.RunWithTimeout<T1, TResult>` (`TimeOutTask.cs:199`) catches `TimeoutException` instead of `TaskCanceledException`, unlike every sibling overload, cites research Sections 2.1 and 3 (Option (c)) as the source, explicitly states this defect is NOT fixed under issue #253 and is out of scope per the Bugfix Workflow's minimal-fix principle, and records that a new GitHub issue should be filed to track it (issue number not yet assigned — record as a pending follow-up, not a fabricated issue number).

---

### Phase 2 — Final C# QA Loop

- [x] [P2-T1] Run the final C# formatting command.
  - Files: `UtilitiesCS/OneDriveHelpers/OneDriveDownloader.cs`, `UtilitiesCS.Test/OneDriveHelpers/OneDriveDownloader_Tests.cs`
  - Command: `dotnet tool run csharpier .`
  - Evidence: `docs/features/active/2026-07-07-onedrive-writer-timeout-test-determinism-253/evidence/qa-gates/csharpier-final.2026-07-07T14-05.md`
  - Acceptance: Evidence contains `Timestamp:`, `Command: dotnet tool run csharpier .`, `EXIT_CODE:`, and `Output Summary:`; if this command changes files, restart Phase 2 from P2-T1 after preserving the evidence.

- [x] [P2-T2] Run the final C# analyzer build command.
  - Files: `TaskMaster.sln`, `UtilitiesCS/OneDriveHelpers/OneDriveDownloader.cs`, `UtilitiesCS.Test/OneDriveHelpers/OneDriveDownloader_Tests.cs`
  - Command: `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
  - Evidence: `docs/features/active/2026-07-07-onedrive-writer-timeout-test-determinism-253/evidence/qa-gates/csharp-analyzers-final.2026-07-07T14-05.md`
  - Acceptance: Evidence contains `Timestamp:`, the exact `Command:`, `EXIT_CODE: 0`, and `Output Summary:`; if this command fails, fix the issue and restart Phase 2 from P2-T1.

- [x] [P2-T3] Run the final C# nullable build command.
  - Files: `TaskMaster.sln`, `UtilitiesCS/OneDriveHelpers/OneDriveDownloader.cs`, `UtilitiesCS.Test/OneDriveHelpers/OneDriveDownloader_Tests.cs`
  - Command: `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true`
  - Evidence: `docs/features/active/2026-07-07-onedrive-writer-timeout-test-determinism-253/evidence/qa-gates/csharp-nullable-final.2026-07-07T14-05.md`
  - Acceptance: Evidence contains `Timestamp:`, the exact `Command:`, `EXIT_CODE: 0`, and `Output Summary:`; if this command fails, fix the issue and restart Phase 2 from P2-T1.

- [x] [P2-T4] Run the final full-suite MSTest coverage command for `UtilitiesCS.Test`.
  - Files: `UtilitiesCS.Test/bin/Debug/UtilitiesCS.Test.dll`
  - Command: `vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /EnableCodeCoverage`
  - Evidence: `docs/features/active/2026-07-07-onedrive-writer-timeout-test-determinism-253/evidence/qa-gates/csharp-vstest-coverage-final.2026-07-07T14-05.md`
  - Acceptance: Evidence contains `Timestamp:`, the exact `Command:`, `EXIT_CODE: 0`, and `Output Summary:` with total tests, pass/fail counts, and the numeric post-change coverage headline percentage; if this command fails, fix the issue and restart Phase 2 from P2-T1.

- [x] [P2-T5] Verify no `TimeOutTask_*` test regressed and no other `OneDriveDownloader_*` test regressed.
  - Files: `docs/features/active/2026-07-07-onedrive-writer-timeout-test-determinism-253/evidence/baseline/csharp-vstest-coverage-baseline.2026-07-07T14-05.md`, `docs/features/active/2026-07-07-onedrive-writer-timeout-test-determinism-253/evidence/qa-gates/csharp-vstest-coverage-final.2026-07-07T14-05.md`
  - Evidence: `docs/features/active/2026-07-07-onedrive-writer-timeout-test-determinism-253/evidence/qa-gates/regression-check.2026-07-07T14-05.md`
  - Acceptance: Evidence compares the baseline (P0-T8) and final (P2-T4) full-suite results by test name/class, confirms every `TimeOutTask_*` test (across `TimeOutTask_Tests.cs`, `TimeOutTask_AdditionalTests.cs`, `TimeOutTask_OverloadCoverageTests.cs`, `TimeOutTask_InternalCoverageTests.cs`) and every test in `OneDriveDownloader_Tests` passes in both runs, and confirms the total pass count did not decrease. Satisfies the no-regression portion of AC5.

- [x] [P2-T6] Record C# coverage comparison evidence for issue #253.
  - Files: `docs/features/active/2026-07-07-onedrive-writer-timeout-test-determinism-253/evidence/baseline/csharp-vstest-coverage-baseline.2026-07-07T14-05.md`, `docs/features/active/2026-07-07-onedrive-writer-timeout-test-determinism-253/evidence/regression-testing/targeted-vstest-coverage.2026-07-07T14-05.md`, `docs/features/active/2026-07-07-onedrive-writer-timeout-test-determinism-253/evidence/qa-gates/csharp-vstest-coverage-final.2026-07-07T14-05.md`
  - Evidence: `docs/features/active/2026-07-07-onedrive-writer-timeout-test-determinism-253/evidence/qa-gates/csharp-coverage-comparison.2026-07-07T14-05.md`
  - Acceptance: Evidence records baseline coverage, targeted-test coverage, and post-change coverage numeric values, and confirms no repository-wide coverage regression and no regression on the changed lines in `OneDriveDownloader.cs` (the new `WriterTimeoutRunner` property and the modified line in `TryGetFileStreamWriter` are exercised by every existing call to `TryGetFileStreamWriter`, both production-default and test-injected paths). Satisfies the coverage portion of AC5.

- [x] [P2-T7] Update issue #253 acceptance-criteria status after verified completion.
  - Files: `docs/features/active/2026-07-07-onedrive-writer-timeout-test-determinism-253/issue.md`
  - Evidence: `docs/features/active/2026-07-07-onedrive-writer-timeout-test-determinism-253/evidence/issue-updates/ac-status.2026-07-07T14-05.md`
  - Acceptance: Only verified acceptance criteria (AC1-AC5) under `## Acceptance Criteria` in `issue.md` are changed from `[ ]` to `[x]`, each backed by the corresponding evidence artifact from Phase 1/Phase 2 named above. Unchanged text is preserved. Evidence records total AC items, checked items, remaining items (if any), and the verification evidence used for each checked item, per `acceptance-criteria-tracking`.

- [x] [P2-T8] Record final minor-audit readiness evidence for issue #253.
  - Files: `docs/features/active/2026-07-07-onedrive-writer-timeout-test-determinism-253/plan.2026-07-07T12-13.md`, `docs/features/active/2026-07-07-onedrive-writer-timeout-test-determinism-253/issue.md`, `docs/features/active/2026-07-07-onedrive-writer-timeout-test-determinism-253/evidence/baseline/phase0-instructions-read.md`, `docs/features/active/2026-07-07-onedrive-writer-timeout-test-determinism-253/evidence/regression-testing/implementation-scope.2026-07-07T14-05.md`, `docs/features/active/2026-07-07-onedrive-writer-timeout-test-determinism-253/evidence/qa-gates/csharp-coverage-comparison.2026-07-07T14-05.md`
  - Evidence: `docs/features/active/2026-07-07-onedrive-writer-timeout-test-determinism-253/evidence/qa-gates/minor-audit-readiness.2026-07-07T14-05.md`
  - Acceptance: Evidence confirms Phase 0 artifacts exist, Phase 1 scope/regression-test/follow-up-issue-note evidence exists, Phase 2 C# QA artifacts exist, every command-bearing task has an executed numeric `EXIT_CODE`, and AC1-AC5 are checked off in `issue.md`.
