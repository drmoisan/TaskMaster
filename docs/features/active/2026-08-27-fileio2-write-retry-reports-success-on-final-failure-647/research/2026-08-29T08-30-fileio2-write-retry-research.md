# Research: FileIO2.WriteTextFileAsync reports success on final-attempt failure (Issue #647)

- **Issue:** #647
- **Branch:** `bug/fileio2-write-retry-reports-success-on-final-failure-647`
- **Date:** 2026-08-29T08-30
- **Scope:** research only; no production or test source file was modified.

## Source-access note

This session had no shell tool available, so `gh issue view 647` could not be executed. The issue
body was read from `docs/features/active/2026-08-27-fileio2-write-retry-reports-success-on-final-failure-647/issue.md`
and cross-checked against `docs/features/potential/promoted/2026-08-27-fileio2-write-retry-reports-success-on-final-failure.md`,
which the promotion tooling maps section-for-section into the GitHub bug template. The two are
identical in substance. Everything else below was verified by reading files in the working tree.

---

## 1. Recommended approach and blast radius (summary)

**Recommendation: change the return type to `Task<bool>` (option ii), restructure the loop so the
"stop retrying" flag no longer doubles as "the write succeeded", and pass the existing
`CancellationToken` to `Task.Delay`. Do NOT throw (option i).**

Rationale, in order of decisiveness:

1. **Throwing is unsafe at an existing call site.** `TaskMaster/AppGlobals/AppOlObjects.cs:302-308`
   assigns `async (items) => await FileIO2.WriteTextFileAsync(...)` to
   `TimedDiskWriter<string>.DiskWriter`, whose declared type is `Action<IEnumerable<T>>?`
   (`UtilitiesCS/ReusableTypeClasses/TimedActions/TimedDiskWriter.cs:79`). That makes the lambda
   **async void**. It is invoked from `OnTimedEvent`
   (`UtilitiesCS/ReusableTypeClasses/TimedActions/TimedDiskWriter.cs:213`), which runs on a
   `System.Timers.Timer` elapsed callback with no `SynchronizingObject`
   (`UtilitiesCS/ReusableTypeClasses/TimedActions/TimerWrapper.cs:42-45`) and therefore no
   `SynchronizationContext`. An exception escaping an async void body is re-raised on the thread
   pool, not returned to the timer, so `System.Timers.Timer`'s documented handler-exception
   suppression does not apply. No `legacyUnhandledExceptionPolicy` element exists anywhere in the
   repository (verified: zero matches across all `*.config`), so the .NET Framework default applies
   and the Outlook host process terminates. Option (i) converts a silent failed write into a
   process crash.
2. **`Task<bool>` and a result struct have identical blast radius.** The same five files change
   either way; only the type differs. Under "Simplicity first" (`.claude/rules/general-code-change.md`)
   and given the module is deprecation-marked, `bool` is the proportionate choice. The extra
   information a result type could carry (attempt count, causing exception) is already written to
   log4net and no caller has a differentiated response to it.
3. **A second defect must be fixed in the same change.** See section 4 (question D). Changing only
   the return type would still return `true` for a write that failed after the stream opened.

### Blast radius (files that must change)

| # | Path | Change |
|---|---|---|
| 1 | `UtilitiesCS/To Depricate/FileIO2.cs:50-89` | Signature `Task` -> `Task<bool>`; restructure loop; `Task.Delay(100, token)`; capture and log the causing `IOException`; add internal seam overload |
| 2 | `QuickFiler/Controllers/QfcHomeController.Metrics.cs:28-34` | Property type `Func<string,string[],string,CancellationToken,Task>` -> `...,Task<bool>>` |
| 3 | `QuickFiler/Controllers/QfcHomeController.Metrics.cs:179` | Capture the result and log on `false`; keep `CancellationToken.None` unchanged |
| 4 | `TaskMaster/AppGlobals/AppOlObjects.cs:302-308` | Capture the result and log on `false` (compiles unchanged, but would silently discard) |
| 5 | `QuickFiler.Test/Controllers/QfcHomeControllerMetricsTests.cs` lines 130, 335, 359, 382, 409, 438 | Six test-double lambdas must return `bool` |
| 6 | `UtilitiesCS.Test/HelperClasses/FileIO2_Tests.cs:29-47` | Replace the ~10-second locked-fixture test with seam-driven deterministic tests |

Files that must NOT change: `ToDoModel/Email Utilities/SortItemsToExistingFolder.cs`,
`QuickFiler/Legacy/QuickFileController.cs`, `UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.cs`,
`QuickFiler/Controllers/EfcHomeControllerDependencies.cs`,
`QuickFiler/Controllers/QfcHomeController.Metrics.cs:103`. All of these call the **synchronous**
`FileIO2.WriteTextFile`, which is a different method and out of scope.

---

## 2. Question A — Complete caller inventory

**Method under change:** `FileIO2.WriteTextFileAsync(string filename, string[] strOutput, string folderpath, CancellationToken token)`
declared at `UtilitiesCS/To Depricate/FileIO2.cs:50-55`.

### A.1 Verification method

Repository-wide `WriteTextFileAsync` search returned hits in `*.cs`, in `docs/**` markdown, in
`docs/features/**/evidence/**/*.cobertura.xml` coverage artifacts, and in `.claude/agent-memory/`.
A targeted search restricted to `*.{vb,xml,json,ps1,psm1,resx,config,md}` returned only documentation
and archived coverage XML — no build script, no manifest, no `.vb` file, and no reflection or
`nameof` reference. **The `.cs` inventory below is complete.**

### A.2 Complete `.cs` inventory (7 hits, 4 of consequence)

| Path:line | Kind | In scope |
|---|---|---|
| `UtilitiesCS/To Depricate/FileIO2.cs:50` | Declaration | Yes |
| `QuickFiler/Controllers/QfcHomeController.Metrics.cs:34` | Method-group assignment to a delegate-typed property | Yes |
| `QuickFiler/Controllers/QfcHomeController.Metrics.cs:23` | XML doc `<see cref="FileIO2.WriteTextFileAsync"/>` | Doc only |
| `TaskMaster/AppGlobals/AppOlObjects.cs:303` | Direct `await` inside an async-void lambda | Yes |
| `UtilitiesCS.Test/HelperClasses/FileIO2_Tests.cs:38` | Direct call from a test | Yes |
| `UtilitiesCS.Test/HelperClasses/FileIO2_Tests.cs:30` | Test method name | Rename candidate |
| `QuickFiler.Test/Controllers/QfcHomeControllerMetricsTests.cs:126` | Comment | Text update |

The list supplied in the delegation prompt is **correct and complete**. One item it did not name
explicitly is the XML-doc `cref` at `QuickFiler/Controllers/QfcHomeController.Metrics.cs:23`, which
is in the same file as the property and needs no separate action.

### A.3 Delegate / `Func<...>` type declarations pinned to the signature

Exactly **one** exists. Verified by a repository-wide multiline search for
`CancellationToken, Task>`: the only other matches are `TimeOutTask` extension-method parameters
(`UtilitiesCS/Threading/TimeOutTask.cs:463,476,698,721`), `StreamExtensions.cs:24`,
`BreadcrumbCoordinatorUpgradeLifetime.cs:196` and test locals, none of which reference `FileIO2`.

**`QuickFiler/Controllers/QfcHomeController.Metrics.cs:28-34`**, exact current text:

```csharp
internal Func<
    string,
    string[],
    string,
    CancellationToken,
    Task
> MetricsFileWriter { get; set; } = FileIO2.WriteTextFileAsync;
```

Required text per candidate return type:

- **`Task<bool>`** — replace `Task` on line 33 with `Task<bool>`.
- **result type `WriteOutcome`** — replace `Task` on line 33 with `Task<WriteOutcome>`, and add
  `using UtilitiesCS;` scope for the type (the file already has `using UtilitiesCS;` at line 8).
- **throw (no signature change)** — no text change.

`MetricsFileWriter` is `internal` on a partial class and is **not** declared on any interface. It is
reached from `QuickFiler.Test` through `[assembly: InternalsVisibleTo("QuickFiler.Test")]`
(`QuickFiler/Controllers/QfcHomeController.cs:15` and `QuickFiler/Properties/AssemblyInfo.cs:5`).
No other production assembly can see it.

### A.4 The `TimedDiskWriter` call site is NOT a pinned delegate type

`TaskMaster/AppGlobals/AppOlObjects.cs:302-308` assigns to
`TimedDiskWriter<string>.DiskWriter`, declared as `Action<IEnumerable<T>>?`
(`UtilitiesCS/ReusableTypeClasses/TimedActions/TimedDiskWriter.cs:79,84`). That type is **not**
shaped by `WriteTextFileAsync` — it takes only the item collection. The lambda adapts between the
two, so **no type declaration changes here for any candidate return type.**

Two consequences the plan must handle:

1. The lambda is **async void**, with the crash exposure described in section 1. This is the
   controlling argument against option (i).
2. With `Task<bool>` (or `Task<WriteOutcome>`), the expression-bodied form
   `async (items) => await FileIO2.WriteTextFileAsync(...)` still converts to
   `Action<IEnumerable<string>>`, because `await_expression` is a valid statement expression and
   the value is discarded. *(Inference from the C# lambda-to-void-delegate conversion rule; it is
   not verified by a build in this session and must be confirmed by the analyzer step.)* The
   consequence is that **the file compiles unchanged while silently discarding the new failure
   signal.** The plan must change it deliberately, e.g. to a block body that logs when the result
   is `false`, rather than leaving it to compile by accident.

### A.5 Callers of the synchronous `WriteTextFile` — explicitly out of scope

`FileIO2.WriteTextFile(string, string[], string)` is declared at
`UtilitiesCS/To Depricate/FileIO2.cs:36`. Its callers, none of which are affected:

- `ToDoModel/Email Utilities/SortItemsToExistingFolder.cs:230` and `:311`
- `QuickFiler/Legacy/QuickFileController.cs:1055`
- `UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.cs:1400`
- `QuickFiler/Controllers/QfcHomeController.Metrics.cs:103` (inside `QuickFileMetrics_WRITE`, a
  different method from `WriteMetricsAsync`)
- `QuickFiler/Controllers/EfcHomeControllerDependencies.cs:78` — method-group assignment to
  `internal Action<string, string[], string> MetricsLineWriter { get; }`
  (`QuickFiler/Controllers/EfcHomeControllerDependencies.cs:127`). This is the EFC precedent the
  QFC seam's XML doc cites; it is a *different* delegate type and is untouched.
- `UtilitiesCS.Test/HelperClasses/FileIO2_Tests.cs:24`

---

## 3. Question B — Return-shape options

### Target-framework constraint (corrected)

`UtilitiesCS.csproj:16` and `UtilitiesCS.Test.csproj:17` both declare
`<TargetFrameworkVersion>v4.8.1</TargetFrameworkVersion>`; `packages.config` entries carry
`targetFramework="net481"`. A repository-wide search confirms **no `IsExternalInit` polyfill type
exists** in any production `.cs` file (all matches are in documentation and agent memory).

The prompt states that `record` and `record struct` both fail CS0518. **That is accurate for
positional records and for any `{ get; init; }` accessor, but not for a nominal `record` with
get-only properties.** `UtilitiesCS/OutlookObjects/Store/StoreRehookResult.cs:59-98` is a
`public sealed record` with constructor-initialized `{ get; }` properties, is compiled
(`UtilitiesCS/UtilitiesCS.csproj:747`), and documents the reason in its own `<remarks>` at lines
53-58. The precise rule, recorded in `.claude/agent-memory/atomic-executor/project_record_struct_isexternalinit_netfx.md`,
is that the **`init` accessor** is what requires `IsExternalInit`. A result type may therefore be a
plain `readonly struct`, a plain class, **or** a nominal `record` with get-only properties — never
positional and never `init`.

### Option (i) — throw the last `IOException`

- **Source-compatible:** yes at compile time; every call site keeps compiling.
- **Blast radius:** zero declared type changes, but a behavioral change at all four call sites.
- **Verdict: reject.** `TaskMaster/AppGlobals/AppOlObjects.cs:302-308` is an async void lambda on a
  thread-pool timer callback with no `SynchronizationContext` and no `legacyUnhandledExceptionPolicy`,
  so a thrown `IOException` terminates the Outlook host process. Additionally,
  `QfcHomeController.WriteMetricsAsync` is awaited from a dispatcher continuation and currently
  cannot fault; making it fault is a wider behavioral change than the issue asks for. Third, the
  current `catch (IOException)` at `UtilitiesCS/To Depricate/FileIO2.cs:75` does not bind the
  exception at all, so "the last IOException" is not even retained today (see section 4.3).

### Option (ii) — `Task<bool>` (RECOMMENDED)

- **Source-compatible:** yes, at every existing site, but **misleadingly so**:
  - `QuickFiler/Controllers/QfcHomeController.Metrics.cs:34` — the method-group assignment
    `Func<..., Task> f = MethodReturningTaskOfBool` is legal, because C# method-group conversion
    permits return-type covariance through a reference conversion and `Task<bool>` derives from
    `Task`. *(Inference from the conversion rule; confirm at the analyzer build step.)* The
    property therefore keeps compiling **while discarding the value**, so the plan must explicitly
    change line 33 or the fix delivers nothing to this caller.
  - `QuickFiler/Controllers/QfcHomeController.Metrics.cs:179` — `await MetricsFileWriter(...)` as a
    statement discards a `Task<bool>` result with no compiler warning.
  - `TaskMaster/AppGlobals/AppOlObjects.cs:303` — see A.4.
  - `UtilitiesCS.Test/HelperClasses/FileIO2_Tests.cs:38` — wrapped in `Func<Task> act = () => ...`;
    `Func<Task>` accepts a `Task<bool>`-returning lambda by the same covariance rule, so this test
    also keeps compiling unchanged. It is being replaced regardless.
- **Blast radius once the property type is changed:** the six test-double lambdas in
  `QuickFiler.Test/Controllers/QfcHomeControllerMetricsTests.cs`. Five return `Task.CompletedTask`
  (lines 130-131, 338, 385, 412, 441) and must return `Task.FromResult(true)`; one is an `async`
  lambda with no return statement (lines 359-363) and must gain `return true;`.
- **Advantages:** smallest surface; matches a deprecation-marked module; every caller's only
  meaningful response is "log that the write did not happen".
- **Limitation:** `bool` at the call site is less self-describing than a named outcome. Mitigate by
  naming the local (`bool written = await ...`) and by an XML-doc `<returns>` on the method.

### Option (iii) — small result type

- **Shape that compiles on net481:** `public readonly struct` with an ordinary constructor and
  `{ get; }` auto-properties (repo precedents: `UtilitiesCS/EmailIntelligence/IntelligenceConfig.cs`
  `ResourceTimingRow`, `TaskMaster/AppGlobals/HookReadinessCoordinator.cs`), or a nominal
  `sealed record` following `UtilitiesCS/OutlookObjects/Store/StoreRehookResult.cs`.
- **Source-compatible:** identical to option (ii) — `Task<WriteOutcome>` converts to `Task` by the
  same reference conversion, with the same silent-discard hazard.
- **Blast radius:** **identical to option (ii)** — the same six files and the same six test-double
  lambdas, differing only in the returned expression.
- **Verdict: available but not recommended.** It adds a public type to a folder named
  `To Depricate` and carries no information any caller acts on. Reserve it for the case where
  review wants to distinguish "open failed after N retries" from "write failed mid-stream"
  (section 4) as separate outcomes rather than as separate log messages.

### Rejected alternatives (brief)

- **`out`/`ref` success flag:** not expressible on an `async` method.
- **Keep `Task`, expose a static `LastWriteFailed` flag:** process-global mutable state; would race
  under `[assembly: Parallelize(Workers = 0, Scope = ClassLevel)]`
  (`UtilitiesCS.Test/Properties/AssemblyInfo.cs:18-21`).
- **Delete `FileIO2.WriteTextFileAsync` and migrate callers to a supported writer** (the issue's own
  closing suggestion): correct long-term disposition, but no supported async text writer exists in
  the repository today, so this is a new-capability feature, not a bug fix. Recommend recording it
  as a follow-up rather than expanding #647.

---

## 4. Question C — Cancellation

### 4.1 The precise change

`UtilitiesCS/To Depricate/FileIO2.cs:80` currently reads:

```csharp
await Task.Delay(100);
```

The change is to `await Task.Delay(100, token);` (or the equivalent through the injected delay seam
recommended in section 6). Line 67 already calls `token.ThrowIfCancellationRequested()` at the top
of each attempt, so the cancellation contract of the method is already "throws
`OperationCanceledException`". `Task.Delay(TimeSpan, CancellationToken)` faults with
`TaskCanceledException`, which derives from `OperationCanceledException`, so the observable
exception type of the method does not change — only its latency on cancellation.

### 4.2 Effect at the `CancellationToken.None` call site — none

**Verified: no current in-repo call site passes a cancellable token.**

- `QuickFiler/Controllers/QfcHomeController.Metrics.cs:179` passes `CancellationToken.None`,
  deliberately, per the comment at lines 176-178.
- `TaskMaster/AppGlobals/AppOlObjects.cs:307` passes `default`, which is
  `CancellationToken.None`.
- `UtilitiesCS.Test/HelperClasses/FileIO2_Tests.cs:42` passes `CancellationToken.None`.

`CancellationToken.None` has `CanBeCanceled == false` and can never enter the cancelled state.
`Task.Delay(delay, token)` registers a cancellation callback only when the token is cancellable; for
a non-cancellable token it produces the same timer-backed task as the single-argument overload. It
therefore cannot complete early, cannot fault, and cannot change the ordering or duration of
anything observed by `QfcHomeController.WriteMetricsAsync`.

**Conclusion: passing the token to `Task.Delay` is a strict no-op for every existing call site.**
Constraint 1 from the delegation prompt — that `QfcHomeController.WriteMetricsAsync` must keep
passing `CancellationToken.None` and must not change observable semantics — is satisfied with no
special handling, and `CancellationToken.None` must remain at line 179 unchanged. The change only
enables a future caller that supplies a real token.

A corollary worth stating in the plan: because no caller supplies a cancellable token, the existing
`token.ThrowIfCancellationRequested()` at line 67 is also currently unreachable in production, and
the cancellation half of issue #647 is a latent-capability fix, not an observed-behavior fix. The
observed ten-second stall at the QFC call site is caused by the retry budget itself, not by the
absence of cancellation, and is only removable by a caller that passes a real token.

### 4.3 Related accuracy correction

The issue text states the final-failure path "logs the exception". It does not. The catch clause at
`UtilitiesCS/To Depricate/FileIO2.cs:75` is `catch (IOException)` with no exception variable, and
line 84 is `logger.Error($"Failed to write to {filepath} after {attempts} attempts.")` with no
exception argument. The causing exception is discarded and never reaches the log. The fix should
bind it (`catch (IOException ex)`) and pass it to the two-argument `logger.Error(object, Exception)`
overload. This is also why option (i) cannot simply "rethrow the last exception" without first
adding the binding.

---

## 5. Question D — The second defect at line 70. Verified: genuine, and it must be fixed

### 5.1 What the code does

```csharp
63  while (!success)
64  {
65      try
66      {
67          token.ThrowIfCancellationRequested();
68          using (var sw = new StreamWriter(filepath, true, System.Text.Encoding.UTF8))
69          {
70              success = true;
71              foreach (var output in strOutput)
72                  await sw.WriteLineAsync(output);
73          }
74      }
75      catch (IOException)
76      {
77          Interlocked.Increment(ref attempts);
78          if (attempts < 100)
79          {
80              await Task.Delay(100);
81          }
82          else
83          {
84              logger.Error($"Failed to write to {filepath} after {attempts} attempts.");
85              success = true;
86          }
87      }
88  }
```

`success` is assigned at line 70, **immediately after the `StreamWriter` constructor returns and
before any `WriteLineAsync` executes**. The retry loop therefore protects exactly two operations:
`token.ThrowIfCancellationRequested()` and the `StreamWriter` construction. Everything after the
stream opens is outside the retry budget.

### 5.2 Trace of a mid-write `IOException`

An `IOException` raised by `await sw.WriteLineAsync(output)` at line 72 — or by the implicit
`sw.Dispose()` at line 73, whose flush can also fail — propagates to the `catch` at line 75 with
`success` already `true`. The catch increments `attempts` to 1, takes the `attempts < 100` branch,
awaits a 100 ms delay, and falls out of the catch. The `while (!success)` condition at line 63 is
then **false**, so the loop exits and the method returns normally.

**Answer: yes.** An `IOException` thrown after the writer opened exits the loop reporting success,
after exactly one attempt and one pointless 100 ms delay, with no retry and no log entry at all
(line 84 is not reached). This is strictly worse than the exhaustion path, which at least logs.

### 5.3 Partial-write flushing

`StreamWriter` buffers; `WriteLineAsync` fills the buffer and flushes to the `FileStream` when it
fills. The `using` block disposes `sw` during exception unwinding, and `StreamWriter.Dispose`
flushes buffered characters. So **whatever was buffered before the failure is normally flushed to
disk**, and the file is opened in append mode (`append: true`, line 68). The observable outcome is
therefore a **partially appended file plus a `true`/normal return** — the worst of the two failure
modes, because the caller has no way to know the record is truncated.

### 5.4 Is fixing it required to make the issue's Expected Behavior true?

**Yes.** The issue's Expected Behavior is "Exhausting the retry budget is a failure and must be
reported as one." Changing only the return type would make the *exhaustion* path return `false`
while the *mid-write* path continues to return `true` for a write that did not complete. The stated
guarantee — that a normal, `true` return means the write happened — would still be false. The plan
must therefore address **two defects in one change**, not one.

This also matches the issue's own Root Cause Analysis ("the success flag appears to have been
intended as 'stop retrying' rather than 'the write succeeded', and the two meanings were
conflated"). Line 70 is the precise location where the conflation is observable.

### 5.5 Design consequence: append duplication on retry

Once line 70 is corrected so the flag means "the write succeeded", a mid-write failure would fall
into the retry branch — and because the file is opened in **append** mode, a retry after a partial
flush **duplicates the already-written lines**. That hazard does not exist today only because the
loop exits. The plan must choose deliberately:

- **(a) Retry mid-write failures.** Simplest control flow, but can produce duplicated lines in the
  metrics CSV on a contended file.
- **(b) Retry only failures raised while opening; treat a failure after the stream opened as
  terminal and return `false` immediately. (RECOMMENDED.)** No duplicated content, failure is still
  reported, and the flag keeps a single honest meaning. Concretely, keep a per-attempt local
  `bool opened` set immediately after the `using` header, and in the catch: if `opened`, log and
  `return false` without retrying; otherwise apply the retry budget.
- **(c) Buffer the payload and write once per attempt** via a single append call. Does not remove
  the duplication risk (the append itself can partially succeed) and changes the I/O shape.

Option (b) is minimal, is expressible inside the existing loop, and is the only one that both
reports failure and avoids introducing a new data-corruption mode.

### 5.6 Two further observations (report-only, not defects to fix in #647)

- `Interlocked.Increment(ref attempts)` at line 77 operates on a method-local captured by the async
  state machine, which is never touched concurrently. It is unnecessary but harmless. A `for` loop
  counter would be clearer.
- Retry granularity: `DirectoryNotFoundException` derives from `IOException`, so an absent folder
  currently consumes the full 100-attempt, ~10-second budget even though it can never succeed. This
  is exactly the behavior the comment at
  `QuickFiler.Test/Controllers/QfcHomeControllerMetricsTests.cs:126-127` documents. Narrowing the
  retryable set (excluding `DirectoryNotFoundException`) would remove that stall, but it is a
  behavior change beyond the issue's stated Expected Behavior and is not reachable in production at
  the QFC call site, which guards on `Globals.FS.SpecialFolders.TryGetValue("MyDocuments", ...)`
  before writing (`QuickFiler/Controllers/QfcHomeController.Metrics.cs:131-134`). Recommend
  recording it as a separate potential item rather than folding it into #647.
- Non-`IOException` failures are unhandled by design: `UnauthorizedAccessException` and
  `NotSupportedException` do not derive from `IOException` and propagate immediately. The existing
  test `WriteTextFile_WhenDevicePathIsUsed_ShouldThrowNotSupportedException`
  (`UtilitiesCS.Test/HelperClasses/FileIO2_Tests.cs:21-27`) documents that for the sync path. Do not
  widen the catch.

---

## 6. Question E — Test strategy under repository policy

### 6.1 What `WriteTextFileAsync_WhenTargetIsLocked_ShouldRetryAndExitWithoutThrowing` actually asserts

`UtilitiesCS.Test/HelperClasses/FileIO2_Tests.cs:29-47`. It:

1. resolves the fixture `UtilitiesCS.Test/TestData/FileIO2/sample.csv` (section 6.3),
2. opens it with `new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.None)`,
3. calls `FileIO2.WriteTextFileAsync(fileName, new[] { "delta" }, folderPath, CancellationToken.None)`,
4. asserts **only** `await act.Should().NotThrowAsync()`.

**Does it encode the buggy behavior?** The *name* does — "ShouldRetryAndExitWithoutThrowing" states
the defective contract as intentional, which the issue's Root Cause Analysis already flags. The
*assertion* does not conflict with the fix: returning `false` also does not throw, so the test would
**still pass unchanged after the fix**. That makes it a weak test that cannot detect the defect in
either direction. It must be replaced, not merely renamed.

**Runtime.** With the file exclusively locked, the `StreamWriter` constructor raises `IOException`
on every attempt. The loop performs 100 open attempts and 99 × 100 ms delays, so the test takes
**at least ~9.9 seconds** *(computed from the loop constants at `UtilitiesCS/To Depricate/FileIO2.cs:78-80`;
not measured in this session, which had no test-execution tool)*. That violates General Unit Test
Policy UT1 "Fast Execution" and is precisely the wall-clock wait that
`QuickFiler.Test/Controllers/QfcHomeControllerMetricsTests.cs:125-129` warns against.

**Additional hazard the plan should retire.** The fixture the test locks is the same
version-controlled `sample.csv` whose exact contents
`CsvReaders_WithFixtureAndMissingFiles_ShouldRespectHeaderOptions`
(`UtilitiesCS.Test/HelperClasses/FileIO2_Tests.cs:49-66`) asserts. `WriteTextFileAsync` opens in
append mode, so if the write ever succeeded it would append `delta` to the fixture and break the
sibling test permanently. The suite is safe today only because the write is guaranteed to fail.
`UtilitiesCS.Test` runs with `[assembly: Parallelize(Workers = 0, Scope = ExecutionScope.ClassLevel)]`
(`UtilitiesCS.Test/Properties/AssemblyInfo.cs:18-21`); both readers are in the same class, so they
are serialized relative to each other and there is no current cross-class race — but the ~10-second
exclusive lock on a shared source-tree file is fragile. The replacement should not touch the
filesystem at all.

**Verdict: replace.** Delete the locked-fixture test and its `FileStream` lock; cover the same and
more behavior through the seam (section 6.5).

### 6.2 The `QuickFiler.Test` comment at line ~126

`QuickFiler.Test/Controllers/QfcHomeControllerMetricsTests.cs:125-129`, verbatim:

```
// Replace the production file writer with a no-op. The default seam value is
// FileIO2.WriteTextFileAsync, which probes a real path and retries 100 times over ten
// seconds when the folder is absent; a unit test must not touch the filesystem or wait
// on wall-clock time. Tests that assert on the flush override this with a capturing
// delegate of their own.
```

**Accuracy: verified correct.** An absent folder raises `DirectoryNotFoundException`, which derives
from `IOException`, so it enters the retry loop and consumes the full budget.

**Are those tests affected?**

- **Behaviorally: no.** Every test in the class overrides `MetricsFileWriter` before acting
  (`BuildLooseMetricsController` at line 130, plus per-test overrides at 335, 359, 382, 409, 438), so
  `FileIO2.WriteTextFileAsync` never executes in `QuickFiler.Test`.
- **At compile time: yes, all six.** If `MetricsFileWriter`'s type becomes `Task<bool>`, five
  lambdas returning `Task.CompletedTask` (lines 130-131, 338, 385, 412, 441) and one `async` lambda
  with no return (359-363) all break and must be updated.
- **Text: the comment should be updated** to describe the post-fix contract (retries N times, then
  returns `false`), so it does not go stale.

### 6.3 How `FileIO2_Tests` obtains paths today (the pattern to follow)

`UtilitiesCS.Test/HelperClasses/FileIO2_Tests.cs:96-114`:

```csharp
private static string GetMissingFolder() =>
    Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "missing-fileio2-folder-for-tests");

private static (string FileName, string FolderPath) GetFixtureLocation()
{
    var fullPath = Path.GetFullPath(
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\TestData\FileIO2\sample.csv"));
    return (Path.GetFileName(fullPath), Path.GetDirectoryName(fullPath));
}
```

So: **no temporary files**. Read-only fixtures live in the source tree at
`UtilitiesCS.Test/TestData/FileIO2/` and are reached by walking two levels up from
`AppDomain.CurrentDomain.BaseDirectory` (i.e. `bin\Debug\..\..`). The fixture
`UtilitiesCS.Test/TestData/FileIO2/sample.csv` is **not** a `<Compile>` or `<None>` item in
`UtilitiesCS.Test/UtilitiesCS.Test.csproj` (verified: zero `TestData` matches) — it is read from the
source tree, not from the output directory, so no copy rule is required. Any negative-path folder is
a name that is guaranteed not to exist, never a created directory.

### 6.4 Is the retry-exhaustion path testable without a seam? No.

Verified: with the method as written, the only way to force repeated `IOException`s is to make the
real filesystem produce them, which requires either an exclusively locked real file (the current
~10-second test, which also risks mutating a shared fixture) or an absent directory (equally slow).
Neither can be made fast, and neither can observe the mid-write failure of section 5 at all, because
there is no way to make a real `StreamWriter` fail *after* opening without external interference.

**Minimum seam (recommended): a stateless internal overload, not static mutable properties.**

```csharp
// Public surface: unchanged parameters, new return type; production defaults supplied here.
public static Task<bool> WriteTextFileAsync(
    string filename, string[] strOutput, string folderpath, CancellationToken token) =>
    WriteTextFileAsync(filename, strOutput, folderpath, token, writerFactory: null, delay: null);

// Internal seam overload consumed only by UtilitiesCS.Test.
internal static async Task<bool> WriteTextFileAsync(
    string filename,
    string[] strOutput,
    string folderpath,
    CancellationToken token,
    Func<string, TextWriter>? writerFactory,
    Func<int, CancellationToken, Task>? delay)
```

Defaults inside the seam overload:
`writerFactory ?? (p => new StreamWriter(p, true, System.Text.Encoding.UTF8))` and
`delay ?? ((ms, t) => Task.Delay(ms, t))`.

Design notes:

- **`TextWriter`, not `StreamWriter`.** `StringWriter` is a `TextWriter` but not a `StreamWriter`,
  so typing the factory as `Func<string, TextWriter>` is what makes an in-memory success path
  testable. This deliberately differs from the closest repo precedent,
  `SmartSerializableBase.CreateStreamWriter` (`UtilitiesCS/ReusableTypeClasses/NewSmartSerializable/SmartSerializableBase.cs:444-449`),
  which is typed `Func<string, StreamWriter>` and therefore cannot accept a `StringWriter`.
  `TextWriter.WriteLineAsync(string)` exists, so line 72 needs no change beyond the variable's type.
- **Parameters, not static properties.** `UtilitiesCS.Test` runs under
  `[assembly: Parallelize(Workers = 0, Scope = ExecutionScope.ClassLevel)]`
  (`UtilitiesCS.Test/Properties/AssemblyInfo.cs:18-21`), and
  `.claude/agent-memory/atomic-executor/project_mstest_donotparallelize_overlaps_parallel_bucket.md`
  records the empirically verified fact (issue #292, 10 real CI failures) that a `[DoNotParallelize]`
  class does **not** run in a phase disjoint from the parallel bucket in this repository. A
  `static` mutable seam on `FileIO2` would therefore be a genuine cross-class race with no reliable
  mitigation. Passing the seam as a parameter removes shared state entirely and satisfies UT1
  Independence and Isolation with no `[TestCleanup]` restoration step.
- **Visibility:** `UtilitiesCS/Properties/AssemblyInfo.cs:19` already declares
  `[assembly: InternalsVisibleTo("UtilitiesCS.Test")]`. No new attribute is needed.
- **Scope:** this is two overloads and two nullable parameters in one already-deprecated file. It is
  not a refactor of the module.

**FakeTimeProvider considered and not recommended.** `Microsoft.Bcl.TimeProvider` 10.0.11 is
referenced by `UtilitiesCS` (`UtilitiesCS/packages.config:28`) and
`Microsoft.Extensions.TimeProvider.Testing` by `UtilitiesCS.Test`
(`UtilitiesCS.Test/packages.config:91`), with existing usage in
`UtilitiesCS.Test/Threading/ThreadMonitorTests.cs` and `TimeOutTask_AdditionalTests.cs`. But
`FakeTimeProvider.Delay` only completes when the clock is advanced from another thread, which makes
a 99-iteration retry loop a concurrency exercise rather than a deterministic assertion. The plain
delegate seam is strictly simpler and fully deterministic. If review prefers `TimeProvider`, it
should be injected instead of the `delay` delegate, not in addition.

### 6.5 Proposed tests (no code written; shapes only)

All in `UtilitiesCS.Test/HelperClasses/FileIO2_Tests.cs`, MSTest + FluentAssertions, no filesystem,
no wall-clock wait, no temporary files:

1. **Retry exhaustion reports failure.** Factory always throws `IOException`; delay seam is a
   counting no-op returning `Task.CompletedTask`. Assert result is `false`, factory invoked exactly
   100 times, delay invoked exactly 99 times. *(This is the regression test for defect 1; it fails
   on the pre-fix source because the pre-fix method returns `Task`, so it can only be written
   against the new signature — the plan should note that and, if a strictly pre-fix-failing test is
   required, add an interim assertion on the pre-fix behavior in the same task.)*
2. **Transient failure then success.** Factory throws for the first N calls then returns a
   `StringWriter`. Assert result is `true`, delay invoked N times, and the `StringWriter` content
   equals the lines each followed by `NewLine`.
3. **Mid-write failure is reported and not retried.** Factory returns a `TextWriter` whose
   `WriteLineAsync` throws `IOException`. Assert result is `false` and the delay seam was invoked
   **zero** times. *(Regression test for defect 2, section 5.)*
4. **Already-cancelled token throws before any open.** Assert
   `OperationCanceledException` and factory invoked zero times.
5. **The token reaches the delay.** Delay seam captures its `CancellationToken` argument; assert the
   captured token equals the one supplied. *(Regression test for the `Task.Delay(100)` ->
   `Task.Delay(100, token)` change; without it, that change is untested.)*
6. **Cancel during the retry window returns promptly.** Delay seam cancels a `CancellationTokenSource`
   and returns `Task.CompletedTask`; the next iteration's `ThrowIfCancellationRequested` must throw.
   Assert `OperationCanceledException` and a small bounded factory invocation count. Deterministic,
   zero wall clock.
7. **Delete** `WriteTextFileAsync_WhenTargetIsLocked_ShouldRetryAndExitWithoutThrowing` and its
   `FileStream` lock on the shared fixture.

Coverage note: the only line not reachable by the above is the production default lambda
`(ms, t) => Task.Delay(ms, t)` inside the public overload's forwarding call. That is a single
expression; accept it rather than adding a wall-clock test to cover it.

Existing tests in `QuickFiler.Test` need updating only for the delegate return type (section 6.2);
no new QuickFiler test is required by this issue, though a test asserting that `WriteMetricsAsync`
logs on a `false` result would be reasonable if the plan adds that logging.

---

## 7. Question F — Toolchain and gate implications

### 7.1 Nullable gate

`UtilitiesCS/To Depricate/FileIO2.cs:1` is `#nullable enable`, so every line added to it participates
in nullable flow analysis and its `CS86xx` diagnostics become errors under
`msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`.

Specific risks in the likely change:

- The two nullable seam parameters (`Func<string, TextWriter>? writerFactory`,
  `Func<int, CancellationToken, Task>? delay`) must be null-coalesced into non-nullable locals
  **once, before the loop**, not dereferenced conditionally inside it, or CS8602 will fire on the
  invocation.
- `catch (IOException ex)` followed by `logger.Error(message, ex)` is safe: log4net's reference
  assembly is unannotated.
- `.NET Framework 4.8.1` reference assemblies carry no nullable annotations, so BCL calls
  (`Path.Combine`, `new StreamWriter`, `Task.Delay`) are null-oblivious and cannot produce CS86xx.
  *(Inference, corroborated by the fact that
  `UtilitiesCS/To Depricate/FileIO2.cs:14-16` dereferences `MethodBase.GetCurrentMethod()` — which
  is `MethodBase?` on annotated targets — with no suppression and currently passes the gate.)*

**`TreatWarningsAsErrors` promotes ALL compiler warnings, not only `CS86xx`.** Additional watch
items for this change: `CS1998` (an `async` lambda with no `await` — a hazard if a seam default is
written as `async (ms, t) => ...`), `CS0162` (unreachable code after a restructured loop), and
`CS0168` (a bound but unused `ex`).

### 7.2 Analyzer gate

`.editorconfig:27` sets `dotnet_analyzer_diagnostic.severity = suggestion` as a global catch-all
(comment at lines 23-26 states this is deliberate, from issue #181, so new analyzer rules cannot be
promoted to errors under the nullable build). The only rule raised above `suggestion` is
`MSTEST0032` at `.editorconfig:29`, and it is `warning`, not `error`. **Consequently no CA/IDE/S/MA
diagnostic can fail the analyzer step**, including `CA1031`, `CA2007`, and `CA1806` (unused return
value). The analyzer step is low risk for this change; the nullable step is where failures will
appear.

### 7.3 Formatting

CSharpier is pinned to 1.2.6 by `dotnet-tools.json`. Run `dotnet tool run csharpier format .` first
in every toolchain pass. The multi-line `Func<...>` property at
`QuickFiler/Controllers/QfcHomeController.Metrics.cs:28-34` is CSharpier-formatted output;
hand-editing line 33 will very likely be reflowed, so format before building.

### 7.4 Coverage

`FileIO2.cs` is a compiled item (`UtilitiesCS/UtilitiesCS.csproj:1110`) and `coverage.config` excludes
only third-party module paths (Deedle, FSharp, Castle.Core, FluentAssertions, Moq, MSTest,
Microsoft.Testing). The `To Depricate` folder is **not** excluded, so all changed lines are in the
coverage denominator and the changed-lines-no-regression rule applies. The seam-driven tests in
section 6.5 should raise coverage of `WriteTextFileAsync` above its current level, since the retry
and mid-write branches are currently unexercised except through the ~10-second locked-file test.

### 7.5 Test assemblies to run

Touched assemblies and their test projects:

| Touched production file | Assembly | Test assembly |
|---|---|---|
| `UtilitiesCS/To Depricate/FileIO2.cs` | `UtilitiesCS` | `UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll` |
| `QuickFiler/Controllers/QfcHomeController.Metrics.cs` | `QuickFiler` | `QuickFiler.Test\bin\Debug\QuickFiler.Test.dll` |
| `TaskMaster/AppGlobals/AppOlObjects.cs` | `TaskMaster` | `TaskMaster.Test\bin\Debug\TaskMaster.Test.dll` |

`ToDoModel.Test` is not required: `ToDoModel` consumes only the synchronous `WriteTextFile`, which is
unchanged. Note however that `UtilitiesCS` grants `InternalsVisibleTo("ToDoModel.Test")`
(`UtilitiesCS/Properties/AssemblyInfo.cs:20`), so a `UtilitiesCS` rebuild does affect it — include it
in the final full pass.

CI (`.github/workflows/_mstest-coverage.yml:70,83`) discovers **every** `*.Test.dll` recursively and
runs `vstest.console.exe <all> /EnableCodeCoverage /InIsolation /Logger:trx /TestCaseFilter:"TestCategory!=LiveOutlook"`.
A local three-assembly run is therefore a subset of the gate; the final toolchain pass must run the
full set. When running locally in a worktree, the assembly list must exclude paths under `\.claude\`
and must pass `/InIsolation`, or assembly-load failures appear as sub-millisecond empty-message test
failures that are not real regressions.

---

## 8. Coordination risk

Issue **#646** (`docs/features/potential/promoted/2026-08-27-qfc-metrics-flush-writes-empty-session-file.md`)
proposes adding an empty-array guard immediately before
`await MetricsFileWriter(filename, lines, myDocuments, CancellationToken.None);` at
`QuickFiler/Controllers/QfcHomeController.Metrics.cs:179` — the same statement #647 must change to
capture the result. Both also touch `QuickFiler.Test/Controllers/QfcHomeControllerMetricsTests.cs`.
If both are in flight, expect a conflict at that statement and sequence them.

---

## 9. Verified vs inferred

**Verified by reading files in this working tree:** the full caller inventory and its completeness
across all file types (section 2); the exact text and declared type of every delegate involved;
`TimedDiskWriter.DiskWriter` being `Action<IEnumerable<T>>?` and the resulting async-void lambda;
`TimerWrapper` constructing a `System.Timers.Timer` with no `SynchronizingObject`; the absence of
`legacyUnhandledExceptionPolicy` in any `*.config`; the absence of an `IsExternalInit` polyfill and
the existence of a compiled nominal `record` at `StoreRehookResult.cs`; the exact assertion, fixture
mechanism and lock of the existing locked-file test; the `QuickFiler.Test` comment text; the
`[assembly: Parallelize(Workers = 0, Scope = ClassLevel)]` attribute; the `.editorconfig` analyzer
catch-all; `coverage.config` contents; the CI test-discovery command; `#nullable enable` on line 1
of `FileIO2.cs`; the three call sites all passing a non-cancellable token; and the line-by-line
control flow of the retry loop including the position of the `success = true` assignment and the
unbound `catch (IOException)`.

**Inference (stated as such above, and requiring confirmation at the build step):** that a
`Task<bool>`-returning method group converts to `Func<..., Task>` and that an
`await`-expression-bodied async lambda returning `Task<bool>` converts to `Action<T>` — both follow
from documented C# conversion rules but were not compiled in this session; the ~9.9-second runtime
of the existing locked-file test, which is computed from the loop constants rather than measured
(no test-execution tool was available); and that .NET Framework 4.8.1 reference assemblies are
null-oblivious, corroborated by the currently-passing dereference at `FileIO2.cs:14-16`.

**Not verified:** the current pass/fail state of any test, and the actual analyzer/nullable output of
a build, because this session had no shell tool.
