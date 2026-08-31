# 2026-08-27-fileio2-write-retry-reports-success-on-final-failure (Spec)

- **Issue:** #647
- **Parent (optional):** none
- **Owner:** drmoisan
- **Last Updated:** 2026-08-29
- **Status:** Implemented; all 21 acceptance criteria verified
- **Version:** 0.2
- **Work Mode:** full-bug

> **Authoritative acceptance-criteria source.** The work mode recorded in issue.md is `full-bug`,
> so this file is the sole acceptance-criteria source for issue #647. No user-story.md exists for
> this feature and none may be created; a second checkbox-bearing file would split the criteria and
> break the check-off protocol in the acceptance-criteria-tracking skill
> (.claude/skills/acceptance-criteria-tracking/SKILL.md).

## Context
`FileIO2.WriteTextFileAsync` in `UtilitiesCS/To Depricate/FileIO2.cs` retries on `IOException` up to
100 times with a 100 millisecond delay between attempts, roughly a ten-second bounded window. When
the final attempt still fails it logs and then sets its success flag to `true` and
returns, so the caller cannot distinguish a completed write from a write that never happened.

Two consequences:

1. **A persistently failed write is silent.** Any caller that awaits this method and treats normal
   return as success is wrong, and there is no return value or exception that would let it behave
   otherwise.
2. **The retry window is not cancellable.** The loop's delay does not observe a `CancellationToken`,
   so a caller that awaits the method while the target file is locked is stalled for the whole
   bounded window regardless of what its own token does.

The second consequence became reachable in a new place through issue #442. `QfcHomeController.WriteMetricsAsync`
now awaits this writer directly, and it deliberately passes `CancellationToken.None` so that a
session cancellation cannot destroy the metrics write. That choice is correct for its own purpose,
but it means a locked session-metrics file stalls the awaiting continuation for the full window with
no cancellation path.

`FileIO2.cs` was **not** modified by #442 and is outside that feature's owned files. This is recorded
as a pre-existing defect in a module already marked for deprecation, surfaced by that work rather
than caused by it. Feature-review raised it as finding CR-2 (Minor, pre-existing, non-blocking) and
explicitly recommended the promotion lifecycle rather than an in-scope fix.

Environment:
- OS/version: Windows 11, Outlook VSTO add-in host
- Language/runtime: C# on .NET Framework 4.8.1. Both affected projects declare
  `<TargetFrameworkVersion>v4.8.1</TargetFrameworkVersion>` (UtilitiesCS/UtilitiesCS.csproj line 16
  and UtilitiesCS.Test/UtilitiesCS.Test.csproj line 17), and their packages.config entries carry
  `targetFramework="net481"`. The earlier "Python version" line in the promotion template was a
  template artifact and does not apply to this repository.
- Command/flags used: `vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll`
- Data source or fixture: any file held open exclusively by another process while the write is attempted

Impact / Severity:
- [ ] Blocker
- [ ] High
- [x] Medium
- [ ] Low

Medium. Silent data loss on a genuinely contended file, and an uncancellable multi-second stall in
any `await` path that reaches it. Both are bounded and neither is reachable from unit tests, because
every current in-repo caller of consequence writes through an injectable seam that tests substitute.


## Repro & Evidence
Steps to Reproduce:
1. Open the intended target file exclusively in another process and keep the handle.
2. Call `FileIO2.WriteTextFileAsync` against that path.
3. Wait for the retry window to expire, then observe the method's return and the file's contents.

Expected:
Exhausting the retry budget is a failure and must be reported as one: either by throwing, or by
returning a result the caller can inspect. The retry delay should also observe a supplied
`CancellationToken` so a caller can abandon the attempt.

Actual:
The method logs a message and returns normally. The caller has no way to learn the write did not
happen, and the delay is uncancellable for the duration of the window.

Logs / Screenshots:
- [x] Attached minimal logs or snippet
- Snippet: the retry loop sits at `UtilitiesCS/To Depricate/FileIO2.cs` lines 50-89; the
  final-failure path logs a message and then assigns the success flag `true` before returning.

**Accuracy correction to the issue text.** The issue body states that the final-failure path "logs
the exception". It does not. The catch clause at `UtilitiesCS/To Depricate/FileIO2.cs` line 75 is
`catch (IOException)` with no exception variable, and line 84 is
`logger.Error($"Failed to write to {filepath} after {attempts} attempts.")`, which uses the
single-argument `ILog.Error(object)` overload. The causing exception is discarded and never reaches
the log. Binding and logging it is therefore part of this fix, not an existing behavior to preserve.

Research input: the authoritative technical analysis for this issue is the research findings file in
this feature folder's `research/` subdirectory, dated 2026-08-29T08-30. Every design decision below
is drawn from it. Its own "Verified vs inferred" section records that two C# conversion behaviors
(a `Task<bool>`-returning method group converting to `Func<..., Task>`, and an await-expression-bodied
async lambda converting to `Action<T>`) are inferred from the language rules rather than compiled,
and must be confirmed at the analyzer build step.


## Scope & Non-Goals

- In scope:
  - Change the return type of the asynchronous writer in `UtilitiesCS/To Depricate/FileIO2.cs` from
    `Task` to `Task<bool>` and restructure the retry loop so its success flag has exactly one
    meaning.
  - Fix the second defect in the same method: the success flag is currently assigned before the
    writes execute, so a mid-write failure exits the loop reporting success.
  - Bind the causing `IOException` and pass it to the logger.
  - Pass the existing `CancellationToken` to the retry delay.
  - Add an `internal` test-seam overload to the same file so the retry, mid-write and cancellation
    branches become deterministically testable without touching the filesystem.
  - Update all four call sites so the new failure signal is observed rather than discarded:
    `QuickFiler/Controllers/QfcHomeController.Metrics.cs` (the `MetricsFileWriter` property type and
    the `WriteMetricsAsync` flush statement), `TaskMaster/AppGlobals/AppOlObjects.cs` (the
    `TimedDiskWriter.DiskWriter` lambda), and `UtilitiesCS.Test/HelperClasses/FileIO2_Tests.cs`.
  - Update the six test-double lambdas and the seam comment in
    `QuickFiler.Test/Controllers/QfcHomeControllerMetricsTests.cs` to the new delegate shape.
  - Replace the locked-fixture test in `UtilitiesCS.Test/HelperClasses/FileIO2_Tests.cs` with
    seam-driven deterministic tests.

- Out of scope / non-goals (follow-up candidates, to be promoted separately):
  - **Narrowing the retryable exception set.** `DirectoryNotFoundException` derives from
    `IOException`, so an absent folder consumes the full 100-attempt window even though it can never
    succeed. Excluding it would remove that stall, but it is a behavior change beyond the issue's
    stated Expected Behavior, and the QuickFiler call site already guards on
    `Globals.FS.SpecialFolders.TryGetValue("MyDocuments", ...)` before writing, so the case is not
    reachable there.
  - **Deleting the method and migrating callers to a supported async writer.** This is the issue's
    own closing suggestion and the correct long-term disposition, but no supported async text writer
    exists in the repository today. Building one is a new capability, not a bug fix, and would
    expand #647 well past its stated scope.
  - **Removing the unnecessary `Interlocked.Increment` on a method-local.** The counter at line 77
    is a local captured by the async state machine and is never touched concurrently. The interlocked
    call is unnecessary but harmless; changing it is cosmetic and is deferred.

- Explicitly excluded systems, integrations, and files:
  - The synchronous `FileIO2.WriteTextFile(string, string[], string)` and every one of its callers.
    Those are a different method and must not change: ToDoModel/Email Utilities/SortItemsToExistingFolder.cs
    (lines 230 and 311), QuickFiler/Legacy/QuickFileController.cs line 1055,
    UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.cs line 1400,
    QuickFiler/Controllers/EfcHomeControllerDependencies.cs line 78 (a different delegate type,
    `Action<string, string[], string>`), and the synchronous call at line 103 of the QuickFiler
    metrics partial, which is inside `QuickFileMetrics_WRITE` rather than `WriteMetricsAsync`.
  - `TimedDiskWriter<T>` itself. Its `DiskWriter` property is declared `Action<IEnumerable<T>>?`,
    a shape that is not derived from the writer's signature, so no type declaration changes in
    UtilitiesCS/ReusableTypeClasses/TimedActions/TimedDiskWriter.cs or its timer wrapper.
  - No .csproj, .editorconfig, coverage.config, or AssemblyInfo.cs change is required. The
    `[assembly: InternalsVisibleTo("UtilitiesCS.Test")]` attribute the seam needs already exists in
    UtilitiesCS/Properties/AssemblyInfo.cs at line 19.

  > Formatting note: repository paths in this Non-Goals section are deliberately written as bare
  > prose rather than as Markdown code spans. Backticked paths in this document denote the intended
  > change footprint, so backticking an out-of-scope file would falsely widen it. Do not "fix" the
  > formatting here.

## Root Cause Analysis
The success flag appears to have been intended as "stop retrying" rather than "the write succeeded",
and the two meanings were conflated. The module lives under `UtilitiesCS/To Depricate/`, which
suggests the defect has survived because the file is slated for removal rather than repair.

Line 70 is the precise point where the conflation becomes observable. `success = true` is assigned
immediately after the `StreamWriter` constructor returns and before any `WriteLineAsync` executes, so
the retry loop protects exactly two operations: `token.ThrowIfCancellationRequested()` and the
`StreamWriter` construction. An `IOException` raised by a write, or by the flush inside the implicit
`Dispose` at the end of the `using` block, reaches the catch clause with `success` already `true`. The
catch increments `attempts` to 1, takes the `attempts < 100` branch, awaits one 100 ms delay, and
falls out; `while (!success)` is then false and the method returns normally. The exhaustion log at
line 84 is never reached, so that path produces no log entry at all. Because the file is opened in
append mode and `StreamWriter.Dispose` flushes buffered characters during unwinding, the observable
outcome is a partially appended file plus a normal return.

This is why the fix must address two defects rather than one. Changing only the return type would
make the exhaustion path return `false` while the mid-write path continued to return `true` for a
write that did not complete, leaving the issue's stated Expected Behavior — that a normal return
means the write happened — still false.

Raised as finding CR-2 in
docs/features/active/quickfiler-home-controller-metrics-442/code-review.2026-08-27T14-35.md.

Related: `UtilitiesCS.Test.HelperClasses.FileIO2_Tests` already contains
`WriteTextFileAsync_WhenTargetIsLocked_ShouldRetryAndExitWithoutThrowing`, whose name records the
current contract as intentional. Its only assertion is `NotThrowAsync`, which would still pass after
the fix, so the test cannot detect the defect in either direction and must be replaced rather than
renamed.


## Proposed Fix

### Design summary (what changes where):

`FileIO2.WriteTextFileAsync` returns `Task<bool>`: `true` means the write completed, `false` means it
did not. The retry loop is restructured so the flag that ends the loop is the same flag that reports
success, the causing `IOException` is bound and logged, and the retry delay receives the caller's
`CancellationToken`. An `internal` overload accepting a writer factory and a delay delegate makes the
retry, mid-write and cancellation branches testable without the filesystem. All four call sites are
edited explicitly so the new signal is observed.

Ratified decisions and their rationale:

1. **Return type becomes `Task<bool>`.** `true` means the write completed; `false` means it did not.
   The value is the smallest surface that makes the failure observable, and every current caller's
   only meaningful response to failure is to log that the write did not happen.

2. **Throwing was rejected.** `TaskMaster/AppGlobals/AppOlObjects.cs` (lines 302-308) assigns an
   `async` lambda to `TimedDiskWriter<string>.DiskWriter`, whose declared type is
   `Action<IEnumerable<T>>?`. That makes the lambda **async void**. It is invoked from the writer's
   timed-event handler, which runs on a `System.Timers.Timer` elapsed callback with no
   `SynchronizingObject` and therefore no `SynchronizationContext`. An exception escaping an async
   void body is re-raised on the thread pool rather than returned to the timer, so
   `System.Timers.Timer`'s documented handler-exception suppression does not apply, and no
   `legacyUnhandledExceptionPolicy` element exists anywhere in the repository (verified: zero matches
   across all .config files). The .NET Framework default therefore applies and the exception
   terminates the Outlook host process. Throwing would convert a silent failed write into a host
   crash, which is a strictly worse outcome than the defect being fixed.

3. **A dedicated result type was considered and rejected.** A `readonly struct` or nominal `record`
   outcome type is expressible on net481 (positional records and `init` accessors are not, for lack
   of an `IsExternalInit` polyfill, but get-only nominal shapes are). Its blast radius is identical
   to `Task<bool>` — the same files, the same six test-double lambdas, differing only in the returned
   expression — and no caller differentiates the extra information, which is already written to
   log4net. Under "Simplicity first" in the general code change policy
   (.claude/rules/general-code-change.md), and given the module
   is deprecation-marked, `bool` is the proportionate choice. Legibility at call sites is recovered
   by naming the local (`bool written = await ...`) and by an XML-doc `<returns>` clause.

4. **Two defects are in scope, not one.** See Root Cause Analysis. Fixing only the return type would
   leave a `true` return for a write that did not complete.

5. **Mid-write failures are terminal, not retried.** The file is opened in append mode, so retrying
   after a partial flush would duplicate already-written lines — a new data-corruption mode that does
   not exist today only because the loop currently exits. The implementation tracks whether the
   stream opened; a failure raised after it opened logs and returns `false` immediately without
   consuming the retry budget, while a failure raised while opening keeps the existing 100-attempt
   budget.

6. **The causing exception must be bound and logged.** See the accuracy correction in Repro &
   Evidence.

7. **`Task.Delay(100)` becomes `Task.Delay(100, token)`.** This is a strict no-op at all three
   existing call sites, because every one passes a non-cancellable token: `CancellationToken.None` at
   the QuickFiler metrics flush, `default` (which is `CancellationToken.None`) in the TaskMaster
   disk-writer lambda, and `CancellationToken.None` in the current test. `CancellationToken.None` has
   `CanBeCanceled == false`, so `Task.Delay(delay, token)` produces the same timer-backed task as the
   single-argument overload and cannot complete early or fault. The cancellation half of this issue
   is therefore a **latent-capability fix, not an observed-behavior fix**: it enables a future caller
   that supplies a real token. By the same reasoning the existing
   `token.ThrowIfCancellationRequested()` at line 67 is currently unreachable in production. The
   observed multi-second stall comes from the retry budget itself, not from the absence of
   cancellation, and is only removable by a caller that passes a real token.

8. **`QfcHomeController.WriteMetricsAsync` keeps passing `CancellationToken.None`.** That choice is
   deliberate and correct — the dispatcher continuation carrying the write is not awaited to
   completion, so a session cancellation must not destroy the metrics — and it must not change. That
   call site gains only the capture-and-log of a `false` result.

9. **A test seam is added** as an `internal` overload of the same method taking two additional
   nullable delegate parameters: a writer factory typed `Func<string, TextWriter>?` and a delay
   delegate `Func<int, CancellationToken, Task>?`. The factory is typed to return `TextWriter`, not
   `StreamWriter`, so a `StringWriter` fits and an in-memory success path becomes testable; this
   deliberately differs from the nearest repository precedent, `SmartSerializableBase.CreateStreamWriter`,
   which is typed to `StreamWriter` and therefore cannot accept a `StringWriter`. The seam is passed
   as **parameters, not static mutable state**, because `UtilitiesCS.Test` runs under
   `[assembly: Parallelize(Workers = 0, Scope = ExecutionScope.ClassLevel)]`; a static mutable seam
   would be a genuine cross-class race with no reliable mitigation, and parameters remove the shared
   state entirely with no `[TestCleanup]` restoration step. The existing
   `[assembly: InternalsVisibleTo("UtilitiesCS.Test")]` already covers the test assembly, so no new
   attribute is needed.

10. **Every call site changes deliberately.** This is the principal hazard of the chosen design.
    `Task<bool>` converts to `Task` by a reference conversion, so the method-group assignment to the
    `MetricsFileWriter` property, the `await` statement at the flush, the async-void
    `DiskWriter` lambda, and the `Func<Task> act = ...` wrapper in the existing test would **all keep
    compiling while silently discarding the new failure signal**. No compiler warning is produced,
    and `CA1806` (unused return value) cannot fail the build because .editorconfig sets
    `dotnet_analyzer_diagnostic.severity = suggestion` as a global catch-all. Each site must
    therefore be edited explicitly and verified by reading the diff; "it still compiles" is not
    evidence that the fix reached the caller.

### Boundaries and invariants to preserve:

- The public method's name, parameter names, parameter order and parameter types are unchanged. Only
  the return type changes.
- The observable exception contract is unchanged. The method already throws
  `OperationCanceledException` via `token.ThrowIfCancellationRequested()`;
  `Task.Delay(int, CancellationToken)` faults with `TaskCanceledException`, which derives from
  `OperationCanceledException`, so no new exception type is introduced.
- The catch clause is not widened. `UnauthorizedAccessException` and `NotSupportedException` do not
  derive from `IOException` and must continue to propagate immediately; the existing test
  `WriteTextFile_WhenDevicePathIsUsed_ShouldThrowNotSupportedException` documents that for the
  synchronous path.
- The retry constants are unchanged: 100 attempts, 100 ms between attempts, append mode, UTF-8.
- The QuickFiler metrics flush keeps `CancellationToken.None` and keeps its explanatory comment.
- Production behavior for a *successful* write is byte-for-byte unchanged.
- No new public type is added to the `To Depricate` folder.

### Dependencies or blocked work:

None blocking. Issue #646 touches the same statement and the same QuickFiler test file; see Risks &
Mitigations for the sequencing requirement.

### Implementation strategy (what changes, not sequencing):

#### Files/modules to change:

| # | Path | Change |
|---|---|---|
| 1 | `UtilitiesCS/To Depricate/FileIO2.cs` | `Task` -> `Task<bool>`; restructure the loop so the flag means "written"; treat post-open failures as terminal; bind and log the `IOException`; `Task.Delay(100, token)`; add the `internal` seam overload; add XML doc `<returns>`. |
| 2 | `QuickFiler/Controllers/QfcHomeController.Metrics.cs` | `MetricsFileWriter` property type `Func<string, string[], string, CancellationToken, Task>` -> `Func<string, string[], string, CancellationToken, Task<bool>>`; at the `WriteMetricsAsync` flush, capture the result into a named local and log when it is `false`, keeping `CancellationToken.None` and its comment. |
| 3 | `TaskMaster/AppGlobals/AppOlObjects.cs` | Convert the expression-bodied `DiskWriter` lambda into a block body that captures the result and logs when it is `false`. The file compiles unchanged, which is exactly why it must be edited deliberately. |
| 4 | `QuickFiler.Test/Controllers/QfcHomeControllerMetricsTests.cs` | Six test-double lambdas must return `bool`: five currently return `Task.CompletedTask` (near lines 130, 338, 385, 412, 441) and become `Task.FromResult(true)`; one is an `async` lambda with no return statement (near lines 359-363) and gains `return true;`. The seam comment near lines 125-129 is updated to describe the post-fix contract. |
| 5 | `UtilitiesCS.Test/HelperClasses/FileIO2_Tests.cs` | Delete `WriteTextFileAsync_WhenTargetIsLocked_ShouldRetryAndExitWithoutThrowing` and its `FileStream` lock; add the seam-driven deterministic tests listed under Test Strategy. |

Evidence produced by this work is written under
`docs/features/active/2026-08-27-fileio2-write-retry-reports-success-on-final-failure-647/evidence/`,
per the evidence-and-timestamp-conventions skill
(.claude/skills/evidence-and-timestamp-conventions/SKILL.md).

#### Functions/classes/CLI commands impacted:

- `UtilitiesCS.FileIO2.WriteTextFileAsync` — signature, control flow, logging, cancellation, plus a
  new `internal` overload.
- `QuickFiler.Controllers.QfcHomeController.MetricsFileWriter` — delegate type.
- `QuickFiler.Controllers.QfcHomeController.WriteMetricsAsync` — result capture and failure logging.
- The `TimedDiskWriter<string>.DiskWriter` lambda constructed in `TaskMaster/AppGlobals/AppOlObjects.cs`.
- `UtilitiesCS.Test.HelperClasses.FileIO2_Tests` — one test removed, six added.
- `QuickFiler.Test.Controllers.QfcHomeControllerMetricsTests` — six test-double lambdas.

No CLI command, no interface, and no public delegate type outside these is affected.
`MetricsFileWriter` is `internal` on a partial class and is not declared on any interface, so only
`QuickFiler.Test` can see it through `InternalsVisibleTo`.

#### Data flow and validation changes:

The data written is unchanged. The only new data on the wire is the returned `bool`, which flows from
the writer to each caller and, on `false`, into a log entry. No input validation is added or removed.
A mid-write failure now stops after the first attempt instead of performing one unnecessary 100 ms
delay, and no longer leaves the caller believing the write succeeded.

#### Error handling and logging updates:

- `catch (IOException)` becomes `catch (IOException ex)`.
- The exhaustion path logs through the two-argument `ILog.Error(object, Exception)` overload so the
  causing exception reaches the appender, then returns `false`.
- A **new** log entry covers the mid-write path, which is silent today. Its message must be
  distinguishable from the exhaustion message, since the two failures have different operational
  meanings: exhaustion implies contention, a post-open failure implies a partially appended file.
- The two callers each log at their own boundary when the result is `false`, using their existing
  logger, so a failed write is attributable to the caller as well as to the writer.
- Non-`IOException` failures continue to propagate unhandled.

#### Rollback/feature-flag considerations (if applicable):

None. No feature flag, no configuration switch, and no staged rollout. The change is a single commit
whose rollback is a revert; there is no persisted state or schema to migrate back.

### Technical specifications (interfaces/contracts):

Public surface after the change:

```csharp
public static Task<bool> WriteTextFileAsync(
    string filename,
    string[] strOutput,
    string folderpath,
    CancellationToken token);
```

Internal test seam in the same class:

```csharp
internal static Task<bool> WriteTextFileAsync(
    string filename,
    string[] strOutput,
    string folderpath,
    CancellationToken token,
    Func<string, TextWriter>? writerFactory,
    Func<int, CancellationToken, Task>? delay);
```

The public overload forwards with both delegates null. The seam overload substitutes production
defaults equivalent to `p => new StreamWriter(p, true, System.Text.Encoding.UTF8)` and
`(ms, t) => Task.Delay(ms, t)`. Because `UtilitiesCS/To Depricate/FileIO2.cs` carries `#nullable
enable` on line 1, both nullable parameters must be null-coalesced into non-nullable locals **once,
before the loop** rather than dereferenced conditionally inside it, or CS8602 will be promoted to a
build error under the nullable gate.

#### Inputs/outputs and formats:

- Inputs: unchanged — file name, lines to append, folder path, cancellation token.
- Output: `true` when every line was written and the writer was disposed without error; `false` when
  the retry budget was exhausted while opening, or when an `IOException` was raised after the writer
  opened. `OperationCanceledException` (including `TaskCanceledException`) on cancellation.
- File format on disk: unchanged — UTF-8, append, one line per array element.

#### Required configuration keys and defaults:

None. No configuration key, app setting, or environment variable is added, removed, or read.

#### Backward-compatibility expectations:

- **Source compatibility is preserved but must not be relied upon.** All four call sites keep
  compiling after the return type changes, by the same reference conversion described in decision 10.
  That is the hazard, not the guarantee. Every call site is edited explicitly.
- **Binary compatibility is not preserved.** Changing a return type is a binary-breaking change. All
  consumers are in-repo and rebuilt together, and `MetricsFileWriter` is `internal`, so there is no
  external consumer to consider.
- **Behavioral compatibility on the success path is exact.** A write that succeeds today writes the
  same bytes and takes the same path after the change.
- **Behavioral change on the mid-write failure path is intentional**: one fewer 100 ms delay, a new
  log entry, and a `false` return instead of a normal return.

#### Performance constraints (latency/throughput/memory):

- The retry budget is unchanged: at most 100 open attempts with 100 ms between them, so the
  worst-case latency of the open-failure path stays at roughly 9.9 seconds. This change does not
  shorten the observed stall; only a caller supplying a cancellable token can do that, and none does
  today (decision 7). Shortening the budget is out of scope.
- The mid-write failure path becomes strictly faster: it returns after the first attempt instead of
  performing one 100 ms delay before exiting.
- The success path is unchanged in latency, throughput and allocation. The two nullable seam
  parameters add two null checks per call and no allocation when they are null.
- Test execution time improves: deleting the locked-fixture test removes roughly 9.9 seconds of
  wall-clock wait from `UtilitiesCS.Test`, and every replacement test is synchronous in effect.

## Assumptions, Constraints, Dependencies
- Assumptions (environment, data, access):
  - The complete caller inventory is the seven `.cs` hits recorded in the research file, four of
    which are of consequence. A repository-wide search restricted to `*.{vb,xml,json,ps1,psm1,resx,config,md}`
    returned only documentation and archived coverage XML, so there is no build script, manifest,
    reflection, or `nameof` reference to this method.
  - .NET Framework 4.8.1 reference assemblies are null-oblivious, so BCL calls in the changed code
    cannot produce `CS86xx` diagnostics. This is corroborated by the currently-passing dereference of
    `MethodBase.GetCurrentMethod()` at lines 14-16 of the same file.
  - `UtilitiesCS.Test` can reach the `internal` seam through the existing `InternalsVisibleTo`.
- Constraints (budget, performance, compatibility):
  - Tests must not create temporary files or directories, and must not wait on wall-clock time
    (general unit test policy, .claude/rules/general-unit-test.md). This rules out any test that
    drives the real retry loop.
  - `#nullable enable` is on line 1 of the changed file, and `/p:TreatWarningsAsErrors=true` promotes
    **all** compiler warnings, not only `CS86xx`. Watch `CS1998` (an `async` lambda with no `await` —
    a hazard if a seam default is written as `async (ms, t) => ...`), `CS0162` (unreachable code after
    the loop restructure) and `CS0168` (a bound but unused `ex`).
  - CSharpier 1.2.6 owns the formatting of the multi-line `Func<...>` property in
    `QuickFiler/Controllers/QfcHomeController.Metrics.cs`; hand-editing that declaration will very
    likely be reflowed, so format before building.
  - The analyzer gate is low risk for this change: .editorconfig sets
    `dotnet_analyzer_diagnostic.severity = suggestion` as a deliberate global catch-all, and the only
    rule raised above it is `MSTEST0032` at `warning`. Failures, if any, will appear at the nullable
    step.
- External dependencies (services, libraries, releases):
  - None added. log4net, MSTest, Moq and FluentAssertions are already referenced.
  - `Microsoft.Extensions.TimeProvider.Testing` is available to `UtilitiesCS.Test`, but
    `FakeTimeProvider` is **not** used here: `FakeTimeProvider.Delay` completes only when the clock is
    advanced from another thread, which would turn a 99-iteration retry loop into a concurrency
    exercise rather than a deterministic assertion. The plain delegate seam is simpler and fully
    deterministic. If a later review prefers `TimeProvider`, it should be injected *instead of* the
    delay delegate, not in addition.

## Data / API / Config Impact
- User-facing or API changes: none visible to an end user. The only API change is the return type of
  a public static method in `UtilitiesCS`, plus one new `internal` overload. No ribbon, form, or
  command surface changes.
- Data or migration considerations: none. The CSV files written by these callers keep their existing
  format and location. No stored data is read, rewritten, or migrated. The mid-write fix reduces the
  chance of a silently truncated appended record but does not repair records already written.
- Logging/telemetry updates: one existing error log gains its causing exception; one new error log
  covers the previously silent mid-write path; two caller-side log entries are added for a `false`
  result. All use the existing log4net loggers and existing appender configuration. No new telemetry
  sink, category, or level.
- Compatibility notes (CLI flags, config schemas, versioning): no CLI flag, no config schema, no
  package version change. Binary compatibility of `UtilitiesCS` is broken by the return-type change;
  all consumers are in-repo and rebuilt in the same solution.

## Test Strategy

Framework: **MSTest** (`Microsoft.VisualStudio.TestTools.UnitTesting`), **Moq** for mocking where a
mock is needed, **FluentAssertions** for assertions, per the repository CLAUDE.md. The earlier
"pytest" line in the
promotion template was a template artifact; this is a C# project.

Seeded from the issue (retained for traceability):

- [x] Unit coverage areas: decide the contract first. Resolved — the contract is `Task<bool>`, not a
  throw; see decisions 1 and 2. `WriteTextFileAsync_WhenTargetIsLocked_ShouldRetryAndExitWithoutThrowing`
  is deleted rather than re-asserted, because its `NotThrowAsync` assertion passes both before and
  after the fix and it holds a ~10-second exclusive lock on a shared source-tree fixture.
- [x] Integration scenario to retest: every in-repo caller reviewed; see the file table above.
  `QfcHomeController.WriteMetricsAsync` keeps `CancellationToken.None` and gains failure logging.
- [x] Manual verification notes: banned-API check — no new test may use a real `Task.Delay`,
  `Thread.Sleep`, or any wall-clock wait, and none may create a file or directory. The delay seam
  makes every timing-dependent branch synchronous.

- Regression tests to add or update, all in `UtilitiesCS.Test/HelperClasses/FileIO2_Tests.cs`, all
  driving the `internal` seam overload, none touching the filesystem:
  1. **Retry exhaustion reports failure** (regression test for defect 1). Writer factory always
     throws `IOException`; delay seam is a counting no-op returning `Task.CompletedTask`. Assert the
     result is `false`, the factory was invoked exactly 100 times, and the delay was invoked exactly
     99 times. Reference name:
     `WriteTextFileAsync_WhenEveryOpenAttemptFails_ShouldReturnFalseAfterBudget`.
  2. **Transient failure then success.** Factory throws for the first N calls then returns a
     `StringWriter`. Assert the result is `true`, the delay was invoked N times, and the writer's
     content equals the supplied lines each followed by `Environment.NewLine`. Reference name:
     `WriteTextFileAsync_WhenTransientOpenFailureThenSucceeds_ShouldReturnTrueAndWriteAllLines`.
  3. **Mid-write failure is reported and not retried** (regression test for defect 2). Factory
     returns a `TextWriter` whose `WriteLineAsync` throws `IOException`. Assert the result is `false`,
     the delay seam was invoked **zero** times, and the factory was invoked exactly once. Reference
     name: `WriteTextFileAsync_WhenWriteFailsAfterOpen_ShouldReturnFalseWithoutRetrying`.
  4. **Already-cancelled token throws before any open.** Assert `OperationCanceledException` and a
     factory invocation count of zero. Reference name:
     `WriteTextFileAsync_WhenTokenAlreadyCancelled_ShouldThrowBeforeOpening`.
  5. **The token reaches the delay** (regression test for the `Task.Delay(100)` ->
     `Task.Delay(100, token)` change; without it that change is untested). The delay seam captures its
     `CancellationToken` argument; assert every captured token equals the token supplied to the
     method. Reference name: `WriteTextFileAsync_WhenRetrying_ShouldPassCallerTokenToDelay`.
  6. **Cancellation during the retry window returns promptly.** The delay seam cancels a
     `CancellationTokenSource` and returns `Task.CompletedTask`, so the next iteration's
     `ThrowIfCancellationRequested` throws. Assert `OperationCanceledException` and a small bounded
     factory invocation count. Deterministic, zero wall clock. Reference name:
     `WriteTextFileAsync_WhenCancelledDuringRetryWindow_ShouldThrowPromptly`.
  7. **Delete** `WriteTextFileAsync_WhenTargetIsLocked_ShouldRetryAndExitWithoutThrowing` together
     with its `FileStream` lock on the shared fixture.
- Unit tests for the fixed behavior and boundaries: the six tests above cover the success path, the
  open-failure retry boundary (99/100), the terminal mid-write path, and both cancellation entry
  points. In `QuickFiler.Test/Controllers/QfcHomeControllerMetricsTests.cs` the six existing
  test-double lambdas are updated to the new delegate shape; a test asserting that
  `WriteMetricsAsync` logs when the writer returns `false` is a reasonable addition once that logging
  exists.
- Edge cases and negative scenarios: empty `strOutput` (writer opened, zero lines written, `true`
  returned); a factory that throws a non-`IOException` (must propagate, not be caught); the
  boundary between attempt 99 and attempt 100.
- Error handling and logging verification: the mid-write and exhaustion paths are distinguished by
  their return value and delay-invocation count in tests, and by distinct log messages on inspection.
  Log assertions are not required; the log4net static logger is not injectable in this class and
  adding an injectable logger is out of scope.
- Coverage impact and targets for changed lines/modules: no merge-base coverage baseline has been
  captured for this feature yet, so no repository-wide figure is asserted as a blocking gate here.
  The blocking obligations are change-scoped: every changed line in
  `UtilitiesCS/To Depricate/FileIO2.cs` is exercised, `WriteTextFileAsync` reaches at least 90% line
  coverage as a changed method, and no changed line regresses. The repository-wide figure is captured
  before and after and recorded under the feature's `evidence/baseline/` and `evidence/qa-gates/` directories, and must not be
  lowered by this change; it is interpreted against the testable denominator defined in CLAUDE.md
  § UT2. `UtilitiesCS/To Depricate/FileIO2.cs` is a compiled item and coverage.config excludes only
  third-party module
  paths, so the `To Depricate` folder is in the denominator. One line is expected to remain
  uncovered — the production default delay lambda inside the public overload's forwarding call —
  and that is accepted rather than covered by a wall-clock test.
- Toolchain commands to run (format -> lint -> type-check -> test), restarting from the top on any
  failure or auto-fix:
  1. `dotnet tool run csharpier format .` then `dotnet tool run csharpier check .`
  2. `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
  3. `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`
  4. `vstest.console.exe <all *.Test.dll> /EnableCodeCoverage /InIsolation /Logger:trx /TestCaseFilter:"TestCategory!=LiveOutlook"`
     The three directly affected test assemblies are `UtilitiesCS.Test`, `QuickFiler.Test` and
     `TaskMaster.Test`; `ToDoModel.Test` must also run in the final pass because `UtilitiesCS` grants
     it `InternalsVisibleTo`. CI discovers every `*.Test.dll` recursively, so the final pass must be
     the full set, not the three-assembly subset. When running inside a worktree, exclude assembly
     paths under `\.claude\` and pass `/InIsolation`, or assembly-load failures appear as
     sub-millisecond empty-message test failures that are not real regressions.
- Manual validation steps: none required. Every behavior in scope is covered by a deterministic
  automated test. Reproducing the original defect by hand would require locking a real file and
  waiting roughly ten seconds, which the seam-driven tests replace.


## Acceptance Criteria
- [x] AC1 — In `UtilitiesCS/To Depricate/FileIO2.cs`, the public `WriteTextFileAsync` declares the return type `Task<bool>`, and its parameter names, order, and types (`string filename, string[] strOutput, string folderpath, CancellationToken token`) are unchanged.
- [x] AC2 — The public `WriteTextFileAsync` carries an XML documentation comment whose `<returns>` clause states that `true` means the write completed and `false` means it did not, and that the method does not throw on a failed write.
- [x] AC3 — A deterministic test in `UtilitiesCS.Test/HelperClasses/FileIO2_Tests.cs` drives the seam with a writer factory that always throws `IOException` and asserts the method returns `false`, the factory was invoked exactly 100 times, and the delay delegate was invoked exactly 99 times.
- [x] AC4 — A deterministic test in `UtilitiesCS.Test/HelperClasses/FileIO2_Tests.cs` drives the seam with a `TextWriter` whose `WriteLineAsync` throws `IOException` and asserts the method returns `false`, the delay delegate was invoked zero times, and the writer factory was invoked exactly once.
- [x] AC5 — A deterministic test in `UtilitiesCS.Test/HelperClasses/FileIO2_Tests.cs` asserts the success path: a factory that fails N times then returns a `StringWriter` yields `true`, N delay invocations, and `StringWriter` content equal to the supplied lines each followed by `Environment.NewLine`.
- [x] AC6 — In `UtilitiesCS/To Depricate/FileIO2.cs`, the value returned as `true` is assigned only after the write loop has completed and the writer has been disposed without error; no assignment establishing success occurs between the writer's creation and the completion of the writes.
- [x] AC7 — In `UtilitiesCS/To Depricate/FileIO2.cs`, the catch clause binds the exception (`catch (IOException ex)`), and both the retry-exhaustion log call and the mid-write-failure log call pass `ex` to the two-argument `logger.Error(object, Exception)` overload. The two log messages are textually distinct from each other.
- [x] AC8 — In `UtilitiesCS/To Depricate/FileIO2.cs`, the retry delay receives the caller's token; no call to a single-argument `Task.Delay` remains in `WriteTextFileAsync`.
- [x] AC9 — A deterministic test in `UtilitiesCS.Test/HelperClasses/FileIO2_Tests.cs` captures the `CancellationToken` argument passed to the injected delay delegate and asserts it equals the token supplied to `WriteTextFileAsync`.
- [x] AC10 — Deterministic tests in `UtilitiesCS.Test/HelperClasses/FileIO2_Tests.cs` cover both cancellation entry points: an already-cancelled token throws `OperationCanceledException` with zero writer-factory invocations, and cancellation signalled from inside the delay seam throws `OperationCanceledException` after a bounded factory invocation count.
- [x] AC11 — `UtilitiesCS/To Depricate/FileIO2.cs` contains an `internal static` overload of `WriteTextFileAsync` taking the four original parameters plus `Func<string, TextWriter>?` and `Func<int, CancellationToken, Task>?`; the public overload forwards to it with both delegates null; no new `static` mutable field or property is added to `FileIO2`; and no new `InternalsVisibleTo` attribute is added anywhere in the repository.
- [x] AC12 — Call site 1: in `QuickFiler/Controllers/QfcHomeController.Metrics.cs`, the `MetricsFileWriter` property is declared `Func<string, string[], string, CancellationToken, Task<bool>>`.
- [x] AC13 — Call site 2: in `QuickFiler/Controllers/QfcHomeController.Metrics.cs`, the `WriteMetricsAsync` flush assigns the awaited result to a named local and emits a log entry when that result is `false`. The statement is not left as a bare `await` that discards the value.
- [x] AC14 — At that same flush in `QuickFiler/Controllers/QfcHomeController.Metrics.cs`, the fourth argument is still `CancellationToken.None`, and the explanatory comment stating why the session token must not be used is retained.
- [x] AC15 — Call site 3: in `TaskMaster/AppGlobals/AppOlObjects.cs`, the `TimedDiskWriter` `DiskWriter` assignment is a block-bodied lambda that assigns the awaited result to a named local and logs when it is `false`. No exception is allowed to escape that async void lambda.
- [x] AC16 — Call site 4: `WriteTextFileAsync_WhenTargetIsLocked_ShouldRetryAndExitWithoutThrowing` is deleted from `UtilitiesCS.Test/HelperClasses/FileIO2_Tests.cs`, and no test in that file opens the UtilitiesCS.Test/TestData/FileIO2/sample.csv fixture with `FileShare.None` or calls the public `WriteTextFileAsync` overload against a real filesystem path.
- [x] AC17 — In `QuickFiler.Test/Controllers/QfcHomeControllerMetricsTests.cs`, all six `MetricsFileWriter` test doubles return a `bool`-bearing task (no remaining `Task.CompletedTask` assignment to `MetricsFileWriter`, and the `async` double contains an explicit `return`), and the seam comment preceding the default double describes the post-fix contract rather than the pre-fix one.
- [x] AC18 — No new or modified test creates a file or directory, uses a temporary path, calls `Thread.Sleep`, or calls a real `Task.Delay`; all timing-dependent branches are driven through the injected delay delegate. Verifiable by inspection of the two changed test files.
- [x] AC19 — The change footprint is exactly the five source files named in this spec plus this feature folder's documents and evidence. In particular, `FileIO2.WriteTextFile` (the synchronous overload) and every file that calls only it are unmodified, and no .csproj, .editorconfig, coverage.config, or AssemblyInfo.cs file is modified.
- [x] AC20 — Every changed line in `UtilitiesCS/To Depricate/FileIO2.cs` is exercised by the new tests, `WriteTextFileAsync` reaches at least 90% line coverage as a changed method, and no changed line regresses in coverage. The repository-wide line-coverage figure is captured before and after under this feature's `evidence/baseline/` and `evidence/qa-gates/` directories and is not lowered by this change; it is assessed against the testable denominator defined in CLAUDE.md § UT2, since no merge-base baseline was available when this spec was authored.
- [x] AC21 — A full toolchain pass completes in a single run with no failures and no auto-fixes, in order: `dotnet tool run csharpier format .` followed by a clean `dotnet tool run csharpier check .`; the analyzer msbuild command; the `TreatWarningsAsErrors` msbuild command; and `vstest.console.exe` over all discovered `*.Test.dll` assemblies with `/EnableCodeCoverage /InIsolation`, excluding paths under `\.claude\`. The commands run and their results are recorded under this feature's `evidence/qa-gates/` directory.

## Risks & Mitigations
- Technical or operational risks:
  1. **Silent discard of the new signal.** `Task<bool>` converts to `Task` by reference conversion, so
     all four call sites compile unchanged while discarding the result, no compiler warning is
     produced, and `CA1806` cannot fail the build because the analyzer catch-all in .editorconfig is
     `suggestion`. A change that compiles cleanly can deliver nothing.
     *Mitigation:* AC12 through AC16 require each site to be verified by reading the diff, not by
     observing a successful build.
  2. **Coordination conflict with issue #646.** #646 proposes adding an empty-array guard immediately
     before the same `await MetricsFileWriter(...)` statement in
     `QuickFiler/Controllers/QfcHomeController.Metrics.cs` that #647 must change to capture the
     result, and both issues also modify `QuickFiler.Test/Controllers/QfcHomeControllerMetricsTests.cs`.
     If both are in flight, expect a merge conflict at that statement and in that test file.
     *Mitigation:* sequence the two issues rather than running them in parallel; whichever lands
     second rebases onto the first and re-runs the QuickFiler test assembly. Confirm #646's branch
     state before starting implementation.
  3. **Append duplication if mid-write failures were retried.** The file is opened in append mode, so
     a retry after a partial flush would duplicate already-written lines.
     *Mitigation:* decision 5 — post-open failures are terminal. AC4 asserts zero delay invocations
     on that path, which is the observable proof that no retry occurred.
  4. **Nullable gate failures from the new seam parameters.** `#nullable enable` is on line 1 of the
     changed file and `TreatWarningsAsErrors` promotes all compiler warnings.
     *Mitigation:* null-coalesce both delegates into non-nullable locals once before the loop; watch
     `CS1998`, `CS0162`, and `CS0168` as described under Constraints.
  5. **The exhaustion regression test cannot fail against pre-fix source.** It can only be written
     against the new signature, so the bugfix-workflow expectation of a test that fails first is not
     literally satisfiable for defect 1 by that test alone.
     *Mitigation:* record this explicitly in the plan. The mid-write test (AC4) does express a
     behavior that is false pre-fix, and the pre-fix behavior of both defects is documented in Root
     Cause Analysis with exact line references.
  6. **Two conversion behaviors are inferred, not compiled.** The research file marks the method-group
     and async-lambda conversions as inference.
     *Mitigation:* both are confirmed or refuted at the first analyzer build; if either is wrong, the
     affected call site fails to compile, which is a louder and safer failure than the silent discard
     it would otherwise cause.
- Mitigations and rollbacks: the change is a single revertible commit with no persisted state, no
  configuration, and no migration. Reverting restores the prior signature and prior behavior exactly.

## Rollout & Follow-up

- Outcome note: all 21 acceptance criteria are verified and checked off above. The two defects carry
  different fail-before evidence, as anticipated by Risk 5.
  - The **mid-write** regression (AC4) carries a real failing pre-fix run at
    `docs/features/active/2026-08-27-fileio2-write-retry-reports-success-on-final-failure-647/evidence/regression-testing/p3-t2-midwrite-fail-before.md`.
    It was made possible by landing the test seam ahead of the loop restructure, so the same test
    could be driven against unfixed control flow. It observed a delay-invocation count of 1 where 0
    was required; the matching post-fix run observed 0.
  - The **retry-exhaustion** regression (AC3) carries the exception dossier at
    `docs/features/active/2026-08-27-fileio2-write-retry-reports-success-on-final-failure-647/evidence/regression-testing/fail-before-exception.2026-08-31T19-40.md`,
    because a test asserting a `false` return can only be written against the post-fix signature and
    that signature change is itself the fix. The dossier's alternative proof is a pre-fix
    characterization run showing the always-failing open path consuming its full 100-attempt budget
    and 99 delays before returning with no failure signal.
- Release/rollout steps: merge with the rest of the solution; the change ships with the next add-in
  build. No deployment step, no configuration change, and no user communication is required.
- Post-fix monitoring or clean-up tasks:
  - Watch the log for the two new `Error` entries. Their appearance indicates genuine file contention
    or a partially appended metrics record, which was previously invisible.
  - Promote the three deferred items in Scope & Non-Goals through the feature-promotion lifecycle
    into their own issues rather than leaving them recorded only here: narrowing the retryable
    exception set, replacing this method with a supported async writer as part of completing the
    `To Depricate` migration, and removing the unnecessary `Interlocked.Increment`.
  - Re-check the seam comment in `QuickFiler.Test/Controllers/QfcHomeControllerMetricsTests.cs` when
    #646 lands, so the two descriptions of the writer's contract stay consistent.
- Links:
  - Issue: https://github.com/drmoisan/TaskMaster/issues/647
  - Related issue: #646 (QuickFiler metrics flush writes an empty session file) — same statement,
    same test file; sequence, do not parallelize.
  - Origin: finding CR-2 in the issue #442 code review, referenced under Root Cause Analysis.
  - Research: the 2026-08-29T08-30 findings file in this feature folder's research subdirectory.
