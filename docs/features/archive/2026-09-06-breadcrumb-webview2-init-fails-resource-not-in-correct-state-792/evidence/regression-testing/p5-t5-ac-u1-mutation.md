# [P5-T5] AC-U1 non-vacuity: two mutations of the bounded breadcrumb retry (attempt limit; per-attempt reporting)

- Issue: #792
- Timestamp: 2026-09-17T20-39
- Command: for each mutation (A then B), from `coverage/plan792-helper.ps1` with the item worktree as the working directory: CMD-OUTLOOK (`OUTLOOK-CLOSED: true` before each build), mutate `QuickFiler/Controllers/EfcFormController.Breadcrumb.cs`, CMD-BUILD-PLAIN (`msbuild TaskMaster.sln /t:Build /m /p:Configuration=Debug "/p:Platform=Any CPU"`), CMD-VSTEST, CMD-SCOPED-RUN with `<FILTER>` = `FullyQualifiedName~EfcFormControllerIssue792Tests.InitializeBreadcrumbHostAsync_RetriesUpToTheAttemptLimitThenReportsOnce|FullyQualifiedName~EfcFormControllerIssue792Tests.InitializeBreadcrumbHostAsync_SucceedsOnALaterAttempt_ReportsNothing` and `<task>` = `p5-t5-a` (A) / `p5-t5-b` (B); restore with `git checkout -- QuickFiler/Controllers/EfcFormController.Breadcrumb.cs`, CMD-BUILD-PLAIN, re-run the same filter with `<task>` = `p5-t5-a-restored` / `p5-t5-b-restored`. Consoles in the gitignored `coverage/p5-t5-a-mutation-build.log`, `coverage/p5-t5-a-scoped.log`, `coverage/p5-t5-a-restore-build.log`, `coverage/p5-t5-a-restored-scoped.log` and the same four with `p5-t5-b`; TRX under the gitignored `coverage/test-results/p5-t5-a/`, `coverage/test-results/p5-t5-a-restored/`, `coverage/test-results/p5-t5-b/`, `coverage/test-results/p5-t5-b-restored/`.
- EXIT_CODE: 0
- Output Summary: both mutations discriminated on their pre-predicted assertions. Mutation A (limit 3 to 1): `Total tests: 2`, `Failed: 2` (exit 1), `RetriesUpToTheAttemptLimitThenReportsOnce` on `Invocations to be 3 ... but found 1`, `SucceedsOnALaterAttempt_ReportsNothing` on `Invocations to be 2 ... but found 1`; the `:244` notification assertion is unreachable under A. Mutation B (per-attempt report in the general catch, limit 3): `Total tests: 2`, `Failed: 2` (exit 1), `SucceedsOnALaterAttempt_ReportsNothing` on `captured ... to be empty ... {"boom"}` at `:244` with its two earlier assertions passing, and the sibling on `to contain a single item` (four captured, expected). Both mutated builds printed `0 Error(s)`. After each restoration `Test Run Successful.`, `Total tests: 2`, `Passed: 2` (exit 0); SHA-256 equal before mutation and after each restoration, no BOM at HEAD and none introduced, `git diff --numstat HEAD -- QuickFiler/Controllers/EfcFormController.Breadcrumb.cs` prints nothing.

## Pre-state (re-derived before mutation A)

- `git diff --numstat HEAD -- QuickFiler/Controllers/EfcFormController.Breadcrumb.cs` prints nothing (scoped porcelain over `*.cs`, `*.csproj`, `*.sln`, `packages.config` empty at HEAD `c2a55b235`), so no `PRE-STATE: restored` step was needed.
- Line citations re-derived on disk: `internal const int BreadcrumbInitializationAttemptLimit = 3;` is line 29; `catch (System.Exception ex)` opens at line 84 and closes at line 92; `lastFailure = ex;` is line 86; the final `TryReportBoundaryFault(...)` call spans lines 97-100. `TryReportBoundaryFault` is `private void` in the same partial class at `QuickFiler/Controllers/EfcFormController.cs:150`, and its default sink (`:137-141`) invokes `UserFaultNotifier` with the message, which the tests capture through `CaptureUserFaults` (`EfcFormControllerIssue792Tests.cs:54-57`).
- Test assertion order re-derived in `QuickFiler.Test/Controllers/EfcFormControllerIssue792Tests.cs`: `InitializeBreadcrumbHostAsync_RetriesUpToTheAttemptLimitThenReportsOnce` asserts `NotThrowAsync` (`:204`), then `Invocations.Should().Be(3, ...)` (`:205-210`), then `captured.Should().ContainSingle(...)` (`:211-218`). `InitializeBreadcrumbHostAsync_SucceedsOnALaterAttempt_ReportsNothing` asserts `NotThrowAsync` (`:240`), then `Invocations.Should().Be(2, ...)` (`:241-243`), then `captured.Should().BeEmpty(...)` (`:244`). Its scripted initializer (`:231`) throws `InvalidOperationException("boom")` on the first call and succeeds on the second.
- SHA256-BEFORE-MUTATION: `9002A324317539335AA10A33360678662D7C8869B167CBC932DA2A9E19D2F84C` (`Get-FileHash -Algorithm SHA256`)
- BOM-BEFORE-MUTATION: False (first three bytes are not `EF BB BF`)

## Mutation A: attempt limit 3 to 1

MUTATION-A: in `QuickFiler/Controllers/EfcFormController.Breadcrumb.cs` line 29, `internal const int BreadcrumbInitializationAttemptLimit = 3;` becomes `internal const int BreadcrumbInitializationAttemptLimit = 1;`.

PREDICTED-FAILING-ASSERTION-A (written before the mutated run):

- `InitializeBreadcrumbHostAsync_RetriesUpToTheAttemptLimitThenReportsOnce` fails on `initializer.Invocations.Should().Be(3, ...)` (`:205-210`): the loop makes one call, so the `OBSERVED:` line contains `Invocations to be 3` and `but found 1`.
- `InitializeBreadcrumbHostAsync_SucceedsOnALaterAttempt_ReportsNothing` fails on `initializer.Invocations.Should().Be(2, ...)` (`:241-243`): the loop makes one call (which fails) and never reaches the scripted success, so the `OBSERVED:` line contains `Invocations to be 2` and `but found 1`.
- `Total tests: 2`, `Failed: 2`, `Test Run Failed.`, exit 1.

Under mutation A the notification assertion `captured.Should().BeEmpty(...)` at `QuickFiler.Test/Controllers/EfcFormControllerIssue792Tests.cs:244` is downstream of the `Be(2)` assertion at `:241-243`, which fails first and ends the test, so mutation A does not prove that assertion. Mutation B exists to prove it.

Mutated build A (20:40): exit 0, `0 Warning(s)`, exact line `0 Error(s)`; 26 `CoreCompile:` lines (unanchored), 10 `csc.exe` lines (1 producing `QuickFiler.dll`, 1 producing `QuickFiler.Test.dll`); production DLL rewritten 20:28:02 to 20:40:33, test DLL 20:28:04 to 20:40:35. Needle matched once before and zero after the edit; replacement present once; 178 lines before and after; `git diff --numstat HEAD` read `1 1`; the hunk was the single line 29 change. `BOM-AFTER-MUTATION-A: False`.

OBSERVED-A (first `Error Message` line of each failed test, verbatim; run id `p5-t5-a`):

- `InitializeBreadcrumbHostAsync_RetriesUpToTheAttemptLimitThenReportsOnce`: `Expected initializer.Invocations to be 3 because the host initializer must be attempted exactly the limit of three times, but found 1.`
- `InitializeBreadcrumbHostAsync_SucceedsOnALaterAttempt_ReportsNothing`: `Expected initializer.Invocations to be 2 because the loop must stop on the first successful attempt, but found 1.`

Observed run A: `Failed InitializeBreadcrumbHostAsync_RetriesUpToTheAttemptLimitThenReportsOnce [164 ms]`; `Failed InitializeBreadcrumbHostAsync_SucceedsOnALaterAttempt_ReportsNothing [2 ms]`; `Test Run Failed.`; `Total tests: 2`; `Failed: 2`; exit 1.

PREDICTION-MATCHES-OBSERVATION-A: true (both tests, on the predicted invocation-count assertions).

Restore A: `git checkout -- QuickFiler/Controllers/EfcFormController.Breadcrumb.cs` exit 0; `SHA256-AFTER-RESTORE-A: 9002A324317539335AA10A33360678662D7C8869B167CBC932DA2A9E19D2F84C` (equal to `SHA256-BEFORE-MUTATION`); `BOM-AFTER-RESTORE-A: False`; needle count 1, replacement count 0; `RESTORED: git diff --numstat HEAD -- QuickFiler/Controllers/EfcFormController.Breadcrumb.cs` prints nothing; `git diff --no-index --numstat` snapshot vs restored file exit 0. Restored build: exit 0, `0 Warning(s)`, `0 Error(s)`; 26 `CoreCompile:`, 10 `csc.exe` (1 + 1); production DLL 20:40:33 to 20:40:52, test DLL 20:40:35 to 20:40:54. Restored run (`p5-t5-a-restored`): `Passed InitializeBreadcrumbHostAsync_RetriesUpToTheAttemptLimitThenReportsOnce [75 ms]`; `Passed InitializeBreadcrumbHostAsync_SucceedsOnALaterAttempt_ReportsNothing [4 ms]`; `Test Run Successful.`; `Total tests: 2`; `Passed: 2`; `Failed: 0 (omitted category)`; exit 0. Scoped porcelain after restoration: prints nothing.

## Mutation B: per-attempt report inside the general catch (limit restored to 3)

MUTATION-B: in `QuickFiler/Controllers/EfcFormController.Breadcrumb.cs`, inside the `catch (System.Exception ex)` block of `InitializeBreadcrumbHostAsync` (`:84-92`), the single statement `TryReportBoundaryFault(ex.Message, ex);` is inserted on its own line immediately after `lastFailure = ex;` (`:86`), so every failed attempt reports through the boundary sink instead of only the final failure at `:97-100`. Applied only after mutation A was restored byte-identically (limit is 3).

PREDICTED-FAILING-ASSERTION-B (written before the mutated run):

- `InitializeBreadcrumbHostAsync_SucceedsOnALaterAttempt_ReportsNothing` fails on `captured.Should().BeEmpty("a recovered initialization must not be reported")` (`:244`): the first attempt throws `boom` and now reports `boom` through the default sink into `captured`; the second attempt succeeds and returns before the final report, so `Invocations` is 2 and `:240` and `:241-243` pass. FluentAssertions outside an assertion scope raises on the first failing assertion, so an `OBSERVED:` line naming `captured` proves the two earlier assertions passed. The `OBSERVED:` line contains `to be empty` and `boom`.
- `InitializeBreadcrumbHostAsync_RetriesUpToTheAttemptLimitThenReportsOnce` fails on `captured.Should().ContainSingle(...)` (`:211-218`): three per-attempt reports plus the final `after 3 attempts` report give four captured notifications; its `Invocations.Should().Be(3)` passed. The `OBSERVED:` line contains `to contain a single item`. Recorded as expected, not as the load-bearing proof.
- `Total tests: 2`, `Failed: 2`, `Test Run Failed.`, exit 1.
- The mutated build prints the exact `0 Error(s)` line (the inserted call targets a private member of the same partial class, `EfcFormController.cs:150`); a compile error is a HALT, not a prediction miss.

Mutated build B (20:41): exit 0, `0 Warning(s)`, exact line `0 Error(s)`; 27 `CoreCompile:` lines (unanchored), 10 `csc.exe` lines (1 producing `QuickFiler.dll`, 1 producing `QuickFiler.Test.dll`); production DLL rewritten 20:40:52 to 20:41:26, test DLL 20:40:54 to 20:41:29. The needle `lastFailure = ex;` (plus CRLF) matched once before the edit and the inserted line was present once after; 178 lines before, 179 after; `git diff --numstat HEAD` read `1 0`; the hunk was the single added line `TryReportBoundaryFault(ex.Message, ex);` at line 87 (after `lastFailure = ex;` at line 86). `BOM-AFTER-MUTATION-B: False`.

OBSERVED-B (first `Error Message` line of each failed test, verbatim; run id `p5-t5-b`):

- `InitializeBreadcrumbHostAsync_SucceedsOnALaterAttempt_ReportsNothing`: `Expected captured to be empty because a recovered initialization must not be reported, but found at least one item {"boom"}.`
- `InitializeBreadcrumbHostAsync_RetriesUpToTheAttemptLimitThenReportsOnce`: `Expected captured to contain a single item because the exhausted limit must be reported to the user exactly once, but found {"boom", "boom", "boom", "Breadcrumb WebView2 initialization failed after 3 attempts: boom"}.`

Observed run B: `Failed InitializeBreadcrumbHostAsync_RetriesUpToTheAttemptLimitThenReportsOnce [169 ms]`; `Failed InitializeBreadcrumbHostAsync_SucceedsOnALaterAttempt_ReportsNothing [4 ms]`; `Test Run Failed.`; `Total tests: 2`; `Failed: 2`; exit 1.

PREDICTION-MATCHES-OBSERVATION-B: true. The load-bearing test failed on `captured` at `:244` (`to be empty`, `boom`), so `:240` and `:241-243` passed and the recovered-initialization-is-not-reported property is now proven non-vacuous by a mutation. The sibling test failed on `to contain a single item` with four captured notifications (three per-attempt `boom` plus the final `after 3 attempts` report), its `Invocations` assertion of 3 having passed; this is recorded as expected and is not the load-bearing proof.

## Restoration proof

- SHA256-BEFORE-MUTATION: `9002A324317539335AA10A33360678662D7C8869B167CBC932DA2A9E19D2F84C` (recorded before mutation A; re-read identical before mutation B; equal to the gitignored snapshots `coverage/p5-t5-a-snapshot.cs` and `coverage/p5-t5-b-snapshot.cs`)
- SHA256-AFTER-RESTORE-A: `9002A324317539335AA10A33360678662D7C8869B167CBC932DA2A9E19D2F84C`
- SHA256-AFTER-RESTORE-B: `9002A324317539335AA10A33360678662D7C8869B167CBC932DA2A9E19D2F84C`
- BOM-BEFORE-MUTATION: False; BOM-AFTER-MUTATION-A: False; BOM-AFTER-RESTORE-A: False; BOM-AFTER-MUTATION-B: False; BOM-AFTER-RESTORE-B: False (all equal; BOM state preserved through both mutations)
- RESTORED-IDENTICAL: true after A and after B; needle counts after each restore 1, replacement counts 0
- RESTORED: `git diff --numstat HEAD -- QuickFiler/Controllers/EfcFormController.Breadcrumb.cs` prints nothing after A and after B; `git diff --no-index --numstat` snapshot vs restored file exit 0 both times

Restore B: `git checkout -- QuickFiler/Controllers/EfcFormController.Breadcrumb.cs` exit 0. Restored build: exit 0, `0 Warning(s)`, `0 Error(s)`; 25 `CoreCompile:`, 10 `csc.exe` (1 + 1); production DLL 20:41:26 to 20:41:46, test DLL 20:41:29 to 20:41:49. Restored run (`p5-t5-b-restored`): `Passed InitializeBreadcrumbHostAsync_RetriesUpToTheAttemptLimitThenReportsOnce [62 ms]`; `Passed InitializeBreadcrumbHostAsync_SucceedsOnALaterAttempt_ReportsNothing [3 ms]`; `Test Run Successful.`; `Total tests: 2`; `Passed: 2`; `Failed: 0 (omitted category)`; exit 0. Scoped porcelain (`git status --porcelain -- '*.cs' '*.csproj' '*.sln' 'packages.config'`) after restoration: prints nothing.

## Prior halted run (2026-09-17T20-27; satisfies no clause of the rewritten task)

The prior run applied mutation A alone under the earlier task text, which predicted the second test would fail on the notification assertion. It observed `Expected initializer.Invocations to be 2 because the loop must stop on the first successful attempt, but found 1.` for `InitializeBreadcrumbHostAsync_SucceedsOnALaterAttempt_ReportsNothing` and `Expected initializer.Invocations to be 3 because the host initializer must be attempted exactly the limit of three times, but found 1.` for `InitializeBreadcrumbHostAsync_RetriesUpToTheAttemptLimitThenReportsOnce` (`Total tests: 2`, `Failed: 2`, exit 1), halted because the second test failed on a different assertion than predicted, and restored the file byte-identically (`SHA256 9002A324317539335AA10A33360678662D7C8869B167CBC932DA2A9E19D2F84C` before and after, no BOM, `git diff --numstat HEAD` printing nothing; restored run `Total tests: 2`, `Passed: 2`, exit 0). No test, assertion or prediction was edited.
