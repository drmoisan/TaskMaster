# Research: Narrow FileIO2.WriteTextFileAsync's retryable exception set (Issue #707)

- **Issue:** #707
- **Date:** 2026-09-02T09-15
- **Scope:** research only; no production or test source file was modified.

## 1. Current State Analysis

### 1.1 The method under change

`UtilitiesCS/To Depricate/FileIO2.cs` contains two overloads of `WriteTextFileAsync`:

- **Public overload** (`UtilitiesCS/To Depricate/FileIO2.cs:69-74`): `Task<bool> WriteTextFileAsync(string filename, string[] strOutput, string folderpath, CancellationToken token)`. Forwards to the internal seam overload with `writerFactory: null, delay: null`, which selects the production defaults.
- **Internal test-seam overload** (`UtilitiesCS/To Depricate/FileIO2.cs:83-150`): adds `Func<string, TextWriter>? writerFactory` and `Func<int, CancellationToken, Task>? delay` parameters. `UtilitiesCS/Properties/AssemblyInfo.cs` already declares `[assembly: InternalsVisibleTo("UtilitiesCS.Test")]`, so no new visibility attribute is required.

Both overloads were introduced by the #647 fix (`docs/features/active/2026-08-27-fileio2-write-retry-reports-success-on-final-failure-647/`), which is the sibling issue that explicitly deferred this narrowing (see 1.3). #647's own fix is already on this branch; the method today already returns `bool`, already binds and logs the causing exception, and already distinguishes "failure before open" (retryable) from "failure after open" (terminal, logged, `return false`) via the `opened` local.

### 1.2 The retry loop as it exists today

`UtilitiesCS/To Depricate/FileIO2.cs:100-149`:

```csharp
Func<string, TextWriter> createWriter =
    writerFactory ?? (p => new StreamWriter(p, true, System.Text.Encoding.UTF8));
Func<int, CancellationToken, Task> delayAsync = delay ?? ((ms, t) => Task.Delay(ms, t));

int attempts = 0;

while (true)
{
    bool opened = false;
    try
    {
        token.ThrowIfCancellationRequested();
        using (var sw = createWriter(filepath))
        {
            opened = true;
            foreach (var output in strOutput)
                await sw.WriteLineAsync(output);
        }
        return true;
    }
    catch (IOException ex)
    {
        if (opened)
        {
            logger.Error($"Write to {filepath} failed after the writer opened. ...", ex);
            return false;
        }

        Interlocked.Increment(ref attempts);
        if (attempts >= 100)
        {
            logger.Error($"Failed to write to {filepath} after {attempts} attempts.", ex);
            return false;
        }

        await delayAsync(100, token);
    }
}
```

The single `catch (IOException ex)` at line 126 is the only exception handler in the loop. It treats every `IOException`-hierarchy failure raised during `createWriter(filepath)` identically: increment `attempts`, and if the budget (100) is not exhausted, await `delayAsync(100, token)` and loop again. The `opened` flag distinguishes pre-open from post-open failures, but does not distinguish *why* the pre-open failure occurred. `DirectoryNotFoundException` — raised by `createWriter` on every attempt when `folderpath` does not exist — falls into the identical retry path as a transient sharing-violation `IOException`, consuming the full 100-attempt budget (99 calls to `delayAsync`) before returning `false`.

### 1.3 Repo precedent: this exact narrowing was deferred from #647

`docs/features/potential/promoted/2026-08-27-fileio2-write-retry-reports-success-on-final-failure.md` (the #647 potential doc) records under "Suspected Cause / Notes" and "Manual verification notes" (also cross-checked against `docs/features/active/2026-08-27-fileio2-write-retry-reports-success-on-final-failure-647/research/2026-08-29T08-30-fileio2-write-retry-research.md` section 5.6, "Two further observations (report-only, not defects to fix in #647)"):

> Retry granularity: `DirectoryNotFoundException` derives from `IOException`, so an absent folder currently consumes the full 100-attempt, ~10-second budget even though it can never succeed. ... Narrowing the retryable set (excluding `DirectoryNotFoundException`) would remove that stall, but it is a behavior change beyond the issue's stated Expected Behavior and is not reachable in production at the QFC call site ... Recommend recording it as a separate potential item rather than folding it into #647.

That recommendation was followed: it was captured as `docs/features/potential/promoted/2026-08-31-narrow-fileio2-retryable-exception-set.md`, then promoted to issue #707, which is the issue under research here. The #647 research also confirms `UnauthorizedAccessException` does not derive from `IOException` (section 5.6, "Non-`IOException` failures are unhandled by design") — independently confirmed against Microsoft Learn below.

The #647 active feature folder (`docs/features/active/2026-08-27-fileio2-write-retry-reports-success-on-final-failure-647/`) has not been archived as of this session, but the production source at `UtilitiesCS/To Depricate/FileIO2.cs` already carries the post-#647 shape (`Task<bool>`, bound `ex`, `opened` terminal-failure branch, internal seam overload) and the current test file already carries the post-#647 test suite (see 1.4). #707's scope is additive to that shape: it does not need to re-derive or re-implement any part of #647's fix.

### 1.4 Existing test seam and coverage (`UtilitiesCS.Test/HelperClasses/FileIO2_Tests.cs`)

The test file already exercises the internal seam with `writerFactory`/`delay` injectable delegates, matching the pattern the #647 research recommended (section 6.5) and that #707's own potential doc's "Unit coverage areas" item calls for. Existing tests, all deterministic, no filesystem, no wall-clock wait:

| Test (line) | Scenario | Assertions |
|---|---|---|
| `WriteTextFileAsync_WhenWriteFailsAfterOpen_ShouldReturnFalseWithoutRetrying` (36) | Mid-write `IOException` via a custom `TextWriter` | factory calls = 1, delay calls = 0, result = `false` |
| `WriteTextFileAsync_WhenEveryOpenAttemptFails_ShouldReturnFalseAfterBudget` (72) | Factory always throws plain `IOException` | factory calls = 100, delay calls = 99, result = `false` |
| `WriteTextFileAsync_WhenTransientOpenFailureThenSucceeds_ShouldReturnTrueAndWriteAllLines` (108) | Factory throws plain `IOException` for 3 calls then returns a `StringWriter` | delay calls = 3, result = `true`, content matches |
| `WriteTextFileAsync_WhenTokenAlreadyCancelled_ShouldThrowBeforeOpening` (152) | Token cancelled before first attempt | throws `OperationCanceledException`, factory calls = 0 |
| `WriteTextFileAsync_WhenCancelledDuringRetryWindow_ShouldThrowPromptly` (184) | Delay seam cancels the token | throws `OperationCanceledException`, factory calls = 1 |
| `WriteTextFileAsync_WhenRetrying_ShouldPassCallerTokenToDelay` (218) | Delay seam captures its token argument | captured tokens all equal the caller's token |

None of these tests throws `DirectoryNotFoundException` (or any other `IOException` subtype) from the writer factory today — every retryable-failure test uses `throw new IOException("Simulated ... failure.")` directly, and the mid-write failure test uses a custom `TextWriter` subclass (`ThrowingOnWriteTextWriter`, line 258-266) that throws `IOException` from `WriteLineAsync`. This is the gap #707 must fill: a new test asserting a `DirectoryNotFoundException`-throwing factory is invoked exactly once and the delay seam is invoked zero times.

`GetFixtureLocation()`/`GetMissingFolder()` (lines 315-333) remain the pattern for path resolution used by the unrelated CSV-read tests; the retry-loop tests do not use them and do not need to, since the seam never touches the real filesystem.

### 1.5 Production callers (blast radius)

Grep for `WriteTextFileAsync` across `*.cs` (excluding the declaration and every test file) returns exactly two production reference sites, both already consuming the post-#647 `Task<bool>` signature:

| Path:line | Context |
|---|---|
| `TaskMaster/AppGlobals/AppOlObjects.cs:315` | `bool movedMailsWritten = await FileIO2.WriteTextFileAsync(_globals.FS.Filenames.MovedMails, items.ToArray(), myDocuments, default);` inside `LoadEmailMoveWriter()`'s `writer.DiskWriter` lambda (`AppOlObjects.cs:306-330`). `myDocuments` is resolved via `_globals.FS.SpecialFolders.TryGetValue("MyDocuments", out var myDocuments)` at `AppOlObjects.cs:300`, inside an `if` whose body contains the whole lambda assignment — i.e., this caller also only reaches `WriteTextFileAsync` when `MyDocuments` was found. `movedMailsWritten` is checked; a `false` result is logged (`AppOlObjects.cs:321-326`) but the caller takes no other corrective action. |
| `QuickFiler/Controllers/QfcHomeController.Metrics.cs:34` | `internal Func<string, string[], string, CancellationToken, Task<bool>> MetricsFileWriter { get; set; } = FileIO2.WriteTextFileAsync;` — a method-group default for an injectable seam property, `internal` and reached only from `QuickFiler.Test` via `InternalsVisibleTo`. The call site consuming it is at `QuickFiler/Controllers/QfcHomeController.Metrics.cs:179` (out of view in this session but referenced by the issue text), guarded by `Globals.FS.SpecialFolders.TryGetValue("MyDocuments", out var myDocuments)` at `QfcHomeController.Metrics.cs:131-134`, which `return`s early (skipping the write entirely) if the key is absent. This file is explicitly **out of scope to modify** per the delegation prompt; it is cited here only as caller context. |

No other `.cs` file references `WriteTextFileAsync` outside `UtilitiesCS.Test/HelperClasses/FileIO2_Tests.cs` (6 call sites, all already covered in 1.4) and the XML-doc `<see cref>` comment at `QfcHomeController.Metrics.cs:23`. Both production callers pre-resolve `myDocuments` through a `TryGetValue("MyDocuments", ...)` guard before ever reaching `WriteTextFileAsync`, which is why the issue rates severity Low: the specific "folder does not exist" failure mode is not the primary way either caller's target directory could be absent (a resolved special folder path is very unlikely to vanish between resolution and write), but a stall is still possible for any other structural path defect the loop encounters, and the fix removes the class of defect rather than a single reproduction.

## 2. .NET Framework 4.8.1 `System.IO` exception hierarchy for `StreamWriter(string, bool, Encoding)`

The production default writer factory at `UtilitiesCS/To Depricate/FileIO2.cs:101` is `p => new StreamWriter(p, true, System.Text.Encoding.UTF8)`, which resolves to the `StreamWriter(String, Boolean, Encoding)` constructor overload. Its documented exceptions (Microsoft Learn, `system.io.streamwriter.-ctor`, `netframework-4.8.1` moniker, confirmed applicable to all listed monikers back through `netframework-1.1`):

| Exception | Condition | Derives from `IOException`? |
|---|---|---|
| `UnauthorizedAccessException` | Access is denied. | **No.** Confirmed via Microsoft Learn `system.unauthorizedaccessexception`: inheritance chain is `Object -> Exception -> SystemException -> UnauthorizedAccessException`. Does not pass through `IOException`. |
| `ArgumentException` | `path` is empty, or contains the name of a system device (`com1`, `com2`, ...). | No (`Object -> Exception -> SystemException -> ArgumentException`). |
| `ArgumentNullException` | `path` or `encoding` is `null`. | No (derives from `ArgumentException`). |
| `DirectoryNotFoundException` | The specified path is invalid (e.g., on an unmapped drive, or — the case this issue targets — the parent directory does not exist). | **Yes.** Confirmed via Microsoft Learn `system.io.directorynotfoundexception`: `Object -> Exception -> SystemException -> IOException -> DirectoryNotFoundException`. |
| `IOException` | `path` includes an incorrect or invalid syntax for file name, directory name, or volume label syntax. | Is itself the base type. This is also the type raised directly (not through a named subtype) for genuine transient sharing violations — e.g., another process holding the file open with `FileShare.None` — which is not separately enumerated in the constructor's documented exception list because it is a runtime condition of the underlying `FileStream` open, not a validation failure of the `path` string. The existing test suite already models this correctly: every "should retry" test throws a bare `new IOException("Simulated ... failure.")` (`FileIO2_Tests.cs:88`, `:129`, `:199`, `:237`), never a named subtype. |
| `PathTooLongException` | The specified path, file name, or both exceed the system-defined maximum length. | **Yes.** Confirmed via Microsoft Learn `system.io.pathtoolongexception`: `Object -> Exception -> SystemException -> IOException -> PathTooLongException`. |
| `SecurityException` | The caller does not have the required permission (legacy Code Access Security). | No (`System.Security.SecurityException` derives from `SystemException` directly, not through `IOException`). Not reachable under the .NET Framework CAS model used by this codebase (no partial-trust configuration in the repo), noted for completeness only. |

`FileNotFoundException` is **not** among the documented exceptions for the `(String, Boolean, Encoding)` write-mode constructor (confirmed via Microsoft Learn `system.io.filenotfoundexception`: it does derive from `IOException` — `Object -> Exception -> SystemException -> IOException -> FileNotFoundException` — but is raised by read-mode opens such as `File.OpenRead` or `FileMode.Open` against a missing file, not by a write/append-mode `StreamWriter` construction, which creates the file if it does not exist). It is therefore not a case this fix needs to handle: `createWriter` in this method's production configuration cannot raise it.

**Summary for the fix:** `DirectoryNotFoundException` and `PathTooLongException` are the only two exception types in the `StreamWriter(String, Boolean, Encoding)` constructor's documented exception set that both (a) derive from `IOException` and (b) represent a structural condition of `folderpath`/`filepath` that cannot be resolved by waiting and retrying. `UnauthorizedAccessException` is already outside the retry set (does not derive from `IOException`) and needs no new handling, confirming the potential doc's manual-verification note.

## 3. Candidate approaches

### Approach A — Catch only `DirectoryNotFoundException` as terminal (issue's literal scope)

Add one new `catch (DirectoryNotFoundException ex)` block, ordered before the existing `catch (IOException ex)` block (C# requires a more-derived exception type to be caught before its base type — catching the base type first would make the more-derived catch clause unreachable and is a compiler error, CS0160). The new block mirrors the existing `opened`-terminal-failure shape at lines 128-135: log and `return false` immediately, without touching `attempts` or calling `delayAsync`.

- **Advantages:** Matches the issue's Summary, Expected Behavior, and Suspected Cause / Notes exactly — the issue text names only `DirectoryNotFoundException`. Minimal diff (one new catch block plus its regression test). Does not touch #647's already-verified logic for the `opened`-terminal-failure or retry-exhaustion paths.
- **Limitation:** `PathTooLongException` is left in the general `IOException` retry path, so a path that exceeds the system-defined maximum length would still consume the full retry budget before failing. Per section 2, this is also structurally undecidable by retrying.
- **Alignment with repo conventions:** Directly matches `.claude/rules/general-code-change.md` "Simplicity first" and the Bugfix Workflow's "minimal, targeted fix" requirement — the issue and spec name one exception type, and expanding scope to a second, unreported type risks widening a bug fix past its stated Expected Behavior (the same reasoning #647's own research used to defer this narrowing out of #647 in the first place, section 1.3).

### Approach B — Catch `DirectoryNotFoundException` and `PathTooLongException` together as terminal

Same structural change as Approach A, but the new catch block's declared type is a shared abstraction — either two separate catch blocks (`catch (DirectoryNotFoundException ex)` then `catch (PathTooLongException ex)`, both before the general `catch (IOException ex)`, each duplicating the terminal-failure body) or, since both types have no other members in the catch clause, one is technically not mergeable in C# without an `is`-pattern discriminator (`catch (IOException ex) when (ex is DirectoryNotFoundException or PathTooLongException)`).

- **Advantages:** Closes both currently-known "cannot succeed by waiting" `IOException` subtypes documented for this exact constructor overload (section 2), not just the one the issue happened to reproduce.
- **Limitation:** Neither the issue text, the spec, nor the potential doc mentions `PathTooLongException`. Introducing it is a scope expansion beyond the issue's stated Expected Behavior ("A failure that cannot be resolved by waiting should not consume the retry budget... structural failures such as a missing directory") — the spec's own example is specific to the missing-directory case, and the issue's Proposed Fix / Validation Ideas and Test Strategy sections describe coverage only for `DirectoryNotFoundException`. `PathTooLongException` is also not reachable from either in-repo production caller: `AppOlObjects.cs:315` and `QfcHomeController.Metrics.cs`'s guarded call both build `filepath` from a resolved special-folder path plus a short, fixed filename (`_globals.FS.Filenames.MovedMails`), which cannot realistically approach the system path-length maximum.

### Recommendation: Approach A

Approach A is recommended. It satisfies the issue's stated Expected Behavior exactly, requires the smallest diff, and follows the same "narrow scope, defer additional narrowing" discipline that produced #707 out of #647 in the first place (section 1.3) — expanding #707's own scope to `PathTooLongException` without a corresponding issue/spec update would repeat the pattern #647's research explicitly avoided. If `PathTooLongException`'s retry-budget stall is judged worth fixing, it should be recorded as its own potential-doc entry (mirroring how this issue itself was recorded) rather than folded into #707's diff.

### Rejected alternatives (brief)

- **Catch-all `when` filter on the general `catch (IOException ex)` clause** (e.g., `when (!(ex is DirectoryNotFoundException))` on the retry branch, or restructuring into a single clause with an `is`-pattern switch): functionally equivalent to Approach A but less readable than a dedicated catch block, and harder to extend if a future issue adds another terminal type — the existing code already establishes the "one catch block per named exception-handling branch" pattern (the `opened`-flag branch inside the general catch, at lines 128-135). Not recommended; no advantage over a dedicated catch block and departs from the existing branch structure.
- **Widen the retry loop to inspect `HResult` instead of the exception's CLR type**: unnecessary indirection: the type hierarchy already draws exactly the line this issue needs (section 2), and the `HResult` values are not otherwise used anywhere in this file or its tests.

## 4. Behavior semantics

### 4.1 Success / failure conditions (unchanged, established by #647)

- **Success:** every line in `strOutput` is written and the writer is disposed without error (line 124, `return true`). Unaffected by this issue.
- **Failure after open (`opened == true`):** logged, `return false` immediately, no retry (lines 128-135). Unaffected by this issue.
- **Failure before open, retryable (`opened == false`, general `IOException`):** increments `attempts`; retries up to 100 total attempts with a 100 ms delay (via `delayAsync`) between attempts; on exhaustion, logs and `return false` (lines 137-147). Unaffected by this issue for the *general* `IOException` case (e.g., sharing violations).
- **Failure before open, terminal (new, `opened == false`, `DirectoryNotFoundException`):** must log and `return false` immediately, on the first occurrence, without incrementing `attempts` and without calling `delayAsync`. This is the new behavior #707 adds.

### 4.2 Ordering rule

C# catch-clause ordering requires the more-derived `DirectoryNotFoundException` catch block to appear textually before the less-derived `IOException` catch block in the same `try`. Placing it after would be a compile-time error (CS0160, "A previous catch clause already catches all exceptions of this or of a super type"), because the general `catch (IOException ex)` at line 126 would already match every `DirectoryNotFoundException` instance and make the later, more specific clause unreachable.

### 4.3 Edge cases

- **Cancellation still takes priority over both catch branches.** `token.ThrowIfCancellationRequested()` at line 114 runs before `createWriter` is invoked on each iteration, so a caller that cancels between attempts is unaffected by which catch branch a prior attempt took (existing tests `WriteTextFileAsync_WhenTokenAlreadyCancelled_ShouldThrowBeforeOpening` and `WriteTextFileAsync_WhenCancelledDuringRetryWindow_ShouldThrowPromptly` already cover this and require no change).
- **A `DirectoryNotFoundException` raised mid-write (after `opened = true`) is impossible under the current writer contract for the production `StreamWriter` factory**, because `DirectoryNotFoundException` is documented only against the constructor, not against `TextWriter.WriteLineAsync`. The new catch clause therefore only needs to be reachable in the pre-open state; no interaction with the `opened`-terminal-failure branch is required. (A test-seam `TextWriter` *could* synthesize a mid-write `DirectoryNotFoundException` for symmetry with the existing `ThrowingOnWriteTextWriter` pattern, but this would test an unreachable production condition; not recommended as a required test, though harmless if added.)
- **A first-attempt `DirectoryNotFoundException` must not call `delayAsync` at all** — this is the key observable difference from the general-`IOException` retry path and the assertion the regression test must make (delay-delegate invocation count of exactly 0, per the issue's own "Unit coverage areas" note).

## 5. Requirements mapping

### 5.1 Proposed code change

In `UtilitiesCS/To Depricate/FileIO2.cs`, insert a new catch block immediately before the existing `catch (IOException ex)` at line 126:

```csharp
catch (DirectoryNotFoundException ex)
{
    logger.Error(
        $"Failed to write to {filepath}: the target directory does not exist.",
        ex
    );
    return false;
}
catch (IOException ex)
{
    // ... existing body, unchanged ...
}
```

This is additive only: the existing `catch (IOException ex)` block, the `opened`-terminal-failure branch inside it, the retry-exhaustion branch, and the `delayAsync` call are all unchanged. No signature change, no new parameters, no change to either overload's declaration.

### 5.2 Files/modules to change

| # | Path | Change |
|---|---|---|
| 1 | `UtilitiesCS/To Depricate/FileIO2.cs` (insert before line 126) | Add `catch (DirectoryNotFoundException ex)` terminal-failure block |
| 2 | `UtilitiesCS.Test/HelperClasses/FileIO2_Tests.cs` | Add one new `[TestMethod]` regression test (see 6) |

No other file requires a change. Both production callers (`AppOlObjects.cs:315`, `QfcHomeController.Metrics.cs`) already consume `Task<bool>` and already handle a `false` result; the new catch path returns through the same `false` result they already handle, so no caller-side code changes are needed. `QfcHomeController.Metrics.cs` is explicitly out of scope to modify per the delegation prompt, and this design confirms no modification to it is required to satisfy the issue.

### 5.3 State model / transitions

No new state is introduced. The existing `attempts` counter and `opened` boolean are unchanged. The new catch block is a third terminal exit from the loop (alongside `return true` on success and the existing `return false` in the `opened`-branch), reached only when `opened == false` and the specific exception type is `DirectoryNotFoundException`.

## 6. Testing implications

Per repository policy (MSTest + FluentAssertions, no filesystem, no wall-clock wait, no temporary files — `.claude/rules/general-unit-test.md`, CUT1/CUT2), add one new test to `UtilitiesCS.Test/HelperClasses/FileIO2_Tests.cs`, following the exact seam pattern of the six existing tests (section 1.4):

- **Test name:** e.g. `WriteTextFileAsync_WhenDirectoryDoesNotExist_ShouldReturnFalseWithoutRetrying` (mirrors the naming of `WriteTextFileAsync_WhenWriteFailsAfterOpen_ShouldReturnFalseWithoutRetrying`).
- **Arrange:** `writerFactory` that increments a call counter and always `throw new DirectoryNotFoundException("Simulated missing directory.")`; `delay` that increments a separate call counter and returns `Task.CompletedTask`.
- **Act:** `await FileIO2.WriteTextFileAsync("irrelevant.csv", new[] { "alpha" }, "irrelevant-folder", cts.Token, writerFactory: ..., delay: ...)`.
- **Assert:** result is `false`; factory invocation count is exactly `1`; delay invocation count is exactly `0`. This is the precise assertion shape the issue's own "Unit coverage areas" note specifies ("assert a writer-factory invocation count of exactly 1 and a delay-delegate invocation count of exactly 0").

This test would fail against the pre-fix source (the writer factory would be invoked up to 100 times and the delay seam up to 99 times, since `DirectoryNotFoundException` currently falls into the general retry branch), and pass once the new catch block is added — satisfying the Bugfix Workflow's "create a failing regression test first" step.

No change is needed to the six existing tests: none of them exercises `DirectoryNotFoundException`, so none of their assertions are affected by adding a new, more specific catch clause ahead of the general one.

### Toolchain

Standard C# toolchain applies, in order: `dotnet tool run csharpier format .` (verify with `check .`), then the two `msbuild` rebuild passes (analyzers, then nullable-as-errors), then `vstest.console.exe` against `UtilitiesCS.Test`. `FileIO2.cs:1` carries `#nullable enable`, so the new catch block's `ex` local and the unchanged nullable-seam locals remain in nullable flow analysis; `catch (DirectoryNotFoundException ex)` followed by `logger.Error(message, ex)` uses the same two-argument `log4net.ILog.Error(object, Exception)` overload already used by the sibling `catch (IOException ex)` block, so no new nullable-annotation risk is introduced. `UtilitiesCS.csproj` compiles `FileIO2.cs` and `coverage.config` does not exclude the `To Depricate` folder, so the new catch block's lines are in the coverage denominator and must be exercised by the new test to avoid a changed-lines coverage regression.

## 7. Verified vs inferred

**Verified by reading files in this working tree:** the full current text and line numbers of both `WriteTextFileAsync` overloads and the retry loop; the complete existing test suite in `FileIO2_Tests.cs` and that none of its tests throws `DirectoryNotFoundException`; the two production caller sites and their `TryGetValue("MyDocuments", ...)` guards; the #647 potential doc and research file's explicit deferral of this narrowing; `InternalsVisibleTo("UtilitiesCS.Test")` already present.

**Verified via Microsoft Learn (`netframework-4.8.1` moniker unless noted):** `DirectoryNotFoundException : IOException`, `PathTooLongException : IOException`, `FileNotFoundException : IOException`, `UnauthorizedAccessException : SystemException` (not `IOException`); the documented exception set of the `StreamWriter(String, Boolean, Encoding)` constructor overload, including that `FileNotFoundException` is not among them.

**Not verified in this session (no shell/build tool available):** actual compiler/analyzer/test output of the proposed catch-block insertion; the exact pre-fix failing-test runtime of the proposed regression test.
