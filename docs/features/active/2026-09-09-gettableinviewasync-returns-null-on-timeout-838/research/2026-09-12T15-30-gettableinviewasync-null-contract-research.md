# GetTableInViewAsync null-on-timeout contract (Issue #838) — Research

- Issue: #838
- Feature folder: `docs/features/active/2026-09-09-gettableinviewasync-returns-null-on-timeout-838`
- Date: 2026-09-12
- Scope: read-only analysis. No production source, configuration, or test file was modified.
- Verification basis: every line citation below was re-derived by reading the file in the current
  worktree during this session. No line number was taken on trust from the delegation prompt.

---

## 1. Current state

### 1.1 The declaration and the two cited lines

`UtilitiesCS/OutlookObjects/Table/OlTableExtensions.TableAccess.cs` is 452 lines long (verified by
line count). The method under investigation:

- `:36` — `public static async Task<Outlook.Table> GetTableInViewAsync(` — non-nullable return type,
  in a file carrying `#nullable enable` at `:1`.
- `:74-81` — the call `table = await TimeOutTask.RunWithTimeout(view.GetTable, token, timeoutMs, 1,
  false, resolvedTimeoutSourceFactory);`. The positional arguments are `maxAttempts: 1` at `:78` and
  `strict: false` at `:79`.
- `:138` — `return table!;`, preceded by the acknowledging comment at `:136-137`.

Both cited line numbers in the delegation prompt are confirmed against the current tree.

Parameter list (`:36-43`), which is load-bearing for the reflective test bindings:

```
this Explorer activeExplorer,
CancellationToken token,
int counter,
int timeoutMs = 2000,
Func<int, CancellationTokenSource>? timeoutSourceFactory = null,
TimeProvider? timeProvider = null
```

### 1.2 Adjacent, already-landed precedent

`UtilitiesCS/Extensions/DfDeedle.QfcColumns.cs` contains `AddQfcColumnsAsync`, the step that runs
immediately after `GetTableInViewAsync` on the same call chain. Its failure contract is:

- `:146-149` — outer-token cancellation returns quietly.
- `:151-155` — exhausted retry throws `TimeoutException` with a message naming the folder and the
  total budget: `"The column add step timed out after 9000 ms for folder '{folderName}'."`

This is the closest in-repo precedent for the remedy and it is already pinned by a test
(`UtilitiesCS.Test/Extensions/DfDeedleQfcColumnTimeoutTests.cs:235`,
`await FluentActions.Awaiting(() => call).Should().ThrowAsync<TimeoutException>();`).

---

## Research question 1 — control flow that produces a null return

### 1a. Which `RunWithTimeout` overload binds

The delegate argument is `view.GetTable`, where `view` is `Outlook.TableView` and
`Outlook.TableView.GetTable()` is parameterless and returns `Outlook.Table`. The method group
therefore converts to `Func<Outlook.Table>`.

The call at `TableAccess.cs:74-81` is a static invocation (`TimeOutTask.RunWithTimeout(...)`) with
six positional arguments. The binding overload is:

- `UtilitiesCS/Threading/TimeOutTask.cs:21-38` —
  `public static async Task<TResult> RunWithTimeout<TResult>(this Func<TResult> function,
  CancellationToken token, int milliseconds, int maxAttempts, bool strict,
  Func<int, CancellationTokenSource>? timeoutSourceFactory = null)`

The only other six-parameter public candidate is the `<T1, TResult>` form at `TimeOutTask.cs:165-184`
(`this Func<T1, TResult> function, T1 arg1, CancellationToken token, ...`). It cannot bind: it would
have to take `token` as `arg1` and then `timeoutMs` (an `int`) as the `CancellationToken` parameter.
`TResult` therefore resolves to `Outlook.Table`, and the whole `<T1, TResult>` family — including the
different `catch (System.Exception e) when (e is TaskCanceledException || e is TimeoutException)`
filter at `TimeOutTask.cs:217` — is not on this path. That filter exists only on the `<T1, TResult>`
overload; the bound overload's catch list is narrower.

### 1b. How the private recursion counts attempts

The public overload at `:21` forwards to the private overload at `TimeOutTask.cs:40-95` with
`attempt: 0` (`:35`). The private body:

- `:50` — `token.ThrowIfCancellationRequested();` (outside the `try`).
- `:52-54` — `using var timeoutSource = (timeoutSourceFactory ?? (ms => new CancellationTokenSource(ms)))(milliseconds);`
  The factory is invoked **outside the `try`**, so anything it throws escapes `RunWithTimeout`
  unmodified. The `using` declaration disposes the source at the end of each attempt.
- `:55-58` — links the caller token and the timeout token.
- `:60` — `TResult result = default(TResult)!;`
- `:63` — `result = await Task.Run(() => function(), combinedToken.Token);`
- `:65-84` — `catch (TaskCanceledException)`: first `token.ThrowIfCancellationRequested()` (`:67`),
  then `if (attempt < maxAttempts)` recurse with `attempt + 1` (`:69-79`), `else` log
  `"Task timed out after {attempt} attempts."` (`:82`) and fall through.
- `:85-92` — `catch (System.Exception e) { logger.Error(e); if (strict) { throw; } }`.
- `:94` — `return result!;`

The comparison is `attempt < maxAttempts` with a zero-based `attempt`. With `maxAttempts: 1` the
reachable attempt indices are `0` and `1`: at `attempt == 0`, `0 < 1` is true and the method recurses;
at `attempt == 1`, `1 < 1` is false, so the warning is logged and `default(TResult)` — `null` for a
reference type — is returned. The naming is misleading: `maxAttempts: 1` yields **two** attempts,
not one.

### 1c. Does `strict: false` suppress the throw

Yes, for the generic catch at `:85-92` only. With `strict == false` the `throw;` at `:90` is skipped,
so every exception other than `TaskCanceledException` — including a `TimeoutException` raised by the
delegate itself — is logged and absorbed, and the method returns `default`. The
`TaskCanceledException` catch at `:65` has no `strict` check at all: it always absorbs after the
retry budget is spent.

### 1d. How many delegate invocations actually occur

`Task.Run(() => function(), combinedToken.Token)` is **scheduled twice** for `maxAttempts: 1` (once
at `attempt == 0`, once at `attempt == 1`). The number of times the delegate **body** executes is
between 0 and 2:

- **0 executions on the null-returning timeout path.** `Task.Run`'s `CancellationToken` argument
  suppresses *scheduling* only; it cannot interrupt a delegate already running on the thread pool.
  This is stated in the repository at `DfDeedle.QfcColumns.cs:114-118` and demonstrated by
  `OlTableExtensions_Tests.cs:1244-1253`. Therefore the only way both awaits raise
  `TaskCanceledException` is for both work items to be cancelled *before* being dequeued — that is,
  thread-pool starvation or an already-cancelled timeout source. In that case `view.GetTable` is
  never called at all, and `RunWithTimeout` still returns `null`.
- **1 or 2 executions** when the delegate itself throws `TaskCanceledException`, or when it throws
  any other exception (1 execution, absorbed at `:85-92`).

Consequence for the defect: a genuinely slow `GetTable` does **not** produce the null. What produces
the null is the work item failing to start inside the deadline. Total wall-clock cost before the null
is returned is up to `2 × timeoutMs` per `GetTableInViewAsync` frame.

### 1e. Reachability of the two `catch` clauses in `GetTableInViewAsync`

Under this binding with `strict: false`, `RunWithTimeout<TResult>` can throw only:

- `OperationCanceledException` from `TimeOutTask.cs:50` or `:67`. `CancellationToken.
  ThrowIfCancellationRequested()` throws `OperationCanceledException`, which is the **base** of
  `TaskCanceledException`, not an instance of it. It therefore does **not** match
  `catch (TaskCanceledException)` at `TableAccess.cs:88`, and it escapes `GetTableInViewAsync`
  entirely. This is exactly what `OlTableExtensions_Tests.cs:1296-1325` asserts.
- Whatever the `timeoutSourceFactory` throws at `TimeOutTask.cs:52-54`, or whatever
  `CreateLinkedTokenSource` throws at `:55-58`, both outside the `try`.

In **production**, the resolved factory is
`(timeProvider ?? TimeProvider.System).CreateCancellationTokenSource(...)`
(`TableAccess.cs:63-70`), which does not throw `TaskCanceledException` or `TimeoutException`.
Therefore in production **both catch clauses at `TableAccess.cs:88` and `:113` are unreachable, and
neither retry recursion at `:99` or `:122` ever runs.** The only production route to a null return is
the absorbed default. This matches the issue text and the epic note at
`docs/features/epics/review-residuals-2026-09-08/epic.md:272-278`.

In **tests**, both catch clauses are reachable through the factory seam, and two existing test classes
already exploit it (`OlTableExtensionsTimeoutDiagnosticsTests.cs:14-19` documents the mechanism).

---

## Research question 2 — every path that hands back null

Four, confirmed. The prompt's count of four is correct; the reachability of each differs sharply.

| # | Site | Mechanism | Production reachability |
|---|---|---|---|
| 1 | `TableAccess.cs:74-81` assigns null, flows to `:138` `return table!;` | `RunWithTimeout` exhausts `attempt 0` and `attempt 1` and returns `default(Outlook.Table)` without throwing | **Reachable.** The defect. |
| 2 | `TableAccess.cs:92` — `table = null;` inside `catch (TaskCanceledException)` when `token.IsCancellationRequested` (`:90`) | requires a `TaskCanceledException` to escape `RunWithTimeout` while the outer token is cancelled | **Unreachable in production** (see 1e). Reachable in test via a factory that cancels the outer source and then throws `TaskCanceledException`. |
| 3 | `TableAccess.cs:109` — `table = null;` in the `else` of `if (counter < 2)` (`:97`) inside `catch (TaskCanceledException)` | requires an escaping `TaskCanceledException` and `counter >= 2` | **Unreachable in production.** Reachable in test via the factory seam with `counter: 2`. |
| 4 | `TableAccess.cs:132` — `table = null;` in the `else` of `if (counter < 2)` (`:117`) inside `catch (TimeoutException)` (`:113`) | requires an escaping `TimeoutException` and `counter >= 2` | **Unreachable in production.** Reachable in test via the factory seam with `counter: 2`. |

All four converge on `:138`, where `table!` erases the null from nullable analysis. A fifth, distinct
exit exists and is **not** a null path: `OperationCanceledException` propagating out of the method
from `TimeOutTask.cs:50`/`:67`. It is already the correct behaviour and is pinned by a test.

---

## Research question 3 — every call site

### 3.1 Direct C# invocation expressions (3)

| File:line | Context | Result dereferenced? | Null guard? |
|---|---|---|---|
| `UtilitiesCS/Extensions/DfDeedle.cs:148` | `Outlook.Table table = await activeExplorer.GetTableInViewAsync(token, 0, timeProvider: timeProvider);` inside `GetEmailDataInViewAsync` | **Yes.** `table` is passed to `AddQfcColumnsAsync(table, ...)` at `:172` and dereferenced at `table.EtlAsync(...)` at `:176`. The local is declared as non-nullable `Outlook.Table` in a `#nullable enable` file. | **None.** The adjacent guard at `:186-193` covers `tableSnapshot.data`, a different value produced by `EtlAsync`. |
| `UtilitiesCS/OutlookObjects/Table/OlTableExtensions.TableAccess.cs:99` | internal recursion inside `catch (TaskCanceledException)` | assigned to the local `Outlook.Table? table`; not dereferenced at the assignment | n/a — the local is nullable; the null reaches `:138` |
| `UtilitiesCS/OutlookObjects/Table/OlTableExtensions.TableAccess.cs:122` | internal recursion inside `catch (TimeoutException)` | same | same |

### 3.2 Reflective binding sites (6)

Every test binds through `typeof(OlTableExtensions).GetMethod("GetTableInViewAsync", BindingFlags.
Static | BindingFlags.Public | BindingFlags.NonPublic, binder: null, types: parameterTypes,
modifiers: null)` and then `method.Invoke(null, args)`. Reflection is required, not preferred: the
return type `Task<Outlook.Table>` involves an embedded interop type, so a direct `await` from the test
assembly is rejected with CS1769 (documented at `GetTableInViewAsyncClockTests.cs:74-80` and
`OlTableExtensionsTimeoutDiagnosticsTests.cs:64-69`).

| File:line | Enclosing member | Result dereferenced? | Null guard? |
|---|---|---|---|
| `UtilitiesCS.Test/OutlookObjects/Table/OlTableExtensions_Tests.cs:1215` | `GetTableInViewAsync_NullTableView_ThrowsInvalidOperationException` (`:1206`) | no — the call is expected to throw | n/a |
| `UtilitiesCS.Test/OutlookObjects/Table/OlTableExtensions_Tests.cs:1273` | `GetTableInViewAsync_SlowSynchronousGetTable_ReturnsTableWithoutSyntheticRetry` (`:1237`) | asserted, not dereferenced: `result.Should().BeSameAs(mockTable.Object)` (`:1291`) | none needed; result is non-null |
| `UtilitiesCS.Test/OutlookObjects/Table/OlTableExtensions_Tests.cs:1306` | `GetTableInViewAsync_CanceledToken_PropagatesOperationCanceledException` (`:1296`) | no — expected to throw `OperationCanceledException` (`:1324`) | n/a |
| `UtilitiesCS.Test/OutlookObjects/Table/OlTableExtensions_Tests.cs:1637` | `GetTableInViewAsync_ImmediateSuccess_CallsGetTableOnceAndReturnsSnapshot` (`:1620`) | asserted `BeSameAs` (`:1655`) | none needed |
| `UtilitiesCS.Test/OutlookObjects/Table/OlTableExtensionsTimeoutDiagnosticsTests.cs:76` | private helper `InvokeGetTableInViewAsync` (`:70`), called from `:145` and `:202` | boxed result returned; both callers assert `BeSameAs` (`:170-175`, `:227-232`) | none needed |
| `UtilitiesCS.Test/OutlookObjects/Table/GetTableInViewAsyncClockTests.cs:87` | private helper `InvokeGetTableInViewAsync` (`:81`), called from `:136`, `:182`, `:238`, `:272` | boxed result; `:136` discards it, the other three assert `BeSameAs` | none needed |

### 3.3 Indirect helper invocations (not counted as members)

These six lines call a local helper, not the target method, and are listed for completeness only:
`OlTableExtensionsTimeoutDiagnosticsTests.cs:145`, `:202`; `GetTableInViewAsyncClockTests.cs:136`,
`:182`, `:238`, `:272`.

### 3.4 Non-invocation textual occurrences (excluded)

`TableAccess.cs:36` (the declaration), `:47`, `:84` (log-message string literals containing the name),
`:96`, `:115` (`nameof(GetTableInViewAsync)`), `:136` (comment);
`DfDeedle.cs:147` (commented-out `logger.Debug`); XML-doc `<see cref=...>` at
`OlTableExtensionsTimeoutDiagnosticsTests.cs:14` and `GetTableInViewAsyncClockTests.cs:16`; prose
comments at `OlTableExtensions_Tests.cs:18-19`, `OlTableExtensionsTimeoutDiagnosticsTests.cs:18`,
`:49`, `:65`, `:124`, `GetTableInViewAsyncClockTests.cs:52`, `:75`, `:109`,
`DfDeedleEtlTimeoutTests.cs:174`; the class name `GetTableInViewAsyncClockTests` and its file name;
and the build-file reference `UtilitiesCS.Test/UtilitiesCS.Test.csproj:549`.

---

## Numeric Derivation Evidence

- Complete Family: GetTableInViewAsync
- Exhaustive Search Scope: The entire repository source tree, every .cs file under every project in the worktree, production and test alike, with no project, directory, or file-name restriction applied.
- Inclusion Rules: A member is a source location that binds GetTableInViewAsync for execution, namely (a) a direct C# invocation expression whose invoked member is GetTableInViewAsync, or (b) a reflective MethodInfo lookup of the literal member name GetTableInViewAsync that is followed by Invoke in the same member body. Both production and test locations count; the method's own recursive invocations count.
- Exclusion Rules: Excluded are the method declaration itself; nameof(GetTableInViewAsync) expressions; string literals that merely contain the name inside a longer log message; commented-out code; XML documentation see cref references; prose comments; type and file names that embed the token; MSBuild Compile Include entries; and calls to a local test helper whose own body performs the binding, since the binding line is already counted.
- Primary Search Strategy or Query Expression: Content search across every .cs file for the bare token GetTableInViewAsync using the Grep tool with glob *.cs and output_mode content, then manual classification of each of the returned lines against the inclusion and exclusion rules above.
- Primary Member Set: UtilitiesCS/Extensions/DfDeedle.cs:148, UtilitiesCS/OutlookObjects/Table/OlTableExtensions.TableAccess.cs:99, UtilitiesCS/OutlookObjects/Table/OlTableExtensions.TableAccess.cs:122, UtilitiesCS.Test/OutlookObjects/Table/OlTableExtensions_Tests.cs:1215, UtilitiesCS.Test/OutlookObjects/Table/OlTableExtensions_Tests.cs:1273, UtilitiesCS.Test/OutlookObjects/Table/OlTableExtensions_Tests.cs:1306, UtilitiesCS.Test/OutlookObjects/Table/OlTableExtensions_Tests.cs:1637, UtilitiesCS.Test/OutlookObjects/Table/OlTableExtensionsTimeoutDiagnosticsTests.cs:76, UtilitiesCS.Test/OutlookObjects/Table/GetTableInViewAsyncClockTests.cs:87
- Primary Count: 9
- Cross-check Search Strategy or Query Expression: Two independent syntax-shaped regular expressions run separately and then unioned, so that classification is performed by the query rather than by the reader: the invocation-syntax expression \.GetTableInViewAsync\( which can only match a member-access call, and the reflection-argument expression "GetTableInViewAsync" which requires the token to be a complete quoted string and therefore matches only a GetMethod name argument. Each expression was run over glob *.cs and its results were enumerated before the two result lists were combined.
- Cross-check Member Set: UtilitiesCS/OutlookObjects/Table/OlTableExtensions.TableAccess.cs:99, UtilitiesCS/OutlookObjects/Table/OlTableExtensions.TableAccess.cs:122, UtilitiesCS/Extensions/DfDeedle.cs:148, UtilitiesCS.Test/OutlookObjects/Table/OlTableExtensions_Tests.cs:1215, UtilitiesCS.Test/OutlookObjects/Table/OlTableExtensions_Tests.cs:1273, UtilitiesCS.Test/OutlookObjects/Table/OlTableExtensions_Tests.cs:1306, UtilitiesCS.Test/OutlookObjects/Table/OlTableExtensions_Tests.cs:1637, UtilitiesCS.Test/OutlookObjects/Table/OlTableExtensionsTimeoutDiagnosticsTests.cs:76, UtilitiesCS.Test/OutlookObjects/Table/GetTableInViewAsyncClockTests.cs:87
- Cross-check Count: 9
- Member-set Comparison: After trimming and lowercasing, the primary and cross-check member sets are equal; both contain the same nine path:line members, differing only in listing order, and both counts are 9.

---

## Research question 4 — recommended remedy

**Recommendation: throw. Keep the return type `Task<Outlook.Table>` and the parameter list
byte-identical. Convert all four null paths into exceptions, and delete the `!` at `:138`.**

### 4a. Is `DfDeedle.cs:186-193` a precedent worth matching?

Located and read. It is:

```
// EtlAsync swallows its TimeoutException and returns a null data array through a
// nullable tuple element, so this is the first point the failure can be named.
if (tableSnapshot.data is null)
{
    throw new InvalidOperationException(
        $"The table snapshot for folder '{folderName}' was not produced: the table ETL "
            + "timed out or was cancelled before returning any rows, so the email data "
            + "frame cannot be built for this folder."
    );
}
```

Judgement: it is a precedent for *naming the failure*, but it is the wrong precedent to copy for this
fix, for two reasons.

1. **It is a caller-side compensation, not a producer-side contract.** Its own comment concedes that
   `EtlAsync` swallows the exception and that the caller is "the first point the failure can be
   named". Issue #838 is the opportunity to fix the producer, which removes the need for a
   compensating guard rather than adding a second one.
2. **`InvalidOperationException` is the wrong type here.** The condition is a deadline expiry, not an
   object in an invalid state.

The precedent that should be matched is the sibling step on the same call chain,
`DfDeedle.QfcColumns.cs:146-155`: quiet return on outer-token cancellation, `TimeoutException` with a
named context on exhausted retry. That method is production code on `main`, is pinned by
`DfDeedleQfcColumnTimeoutTests.cs:235`, and sits one statement away from the fix site
(`DfDeedle.cs:172` calls it immediately after `:148`). Matching it makes the two adjacent steps of
the same pipeline report the same failure mode in the same way.

### 4b. Cost of a sentinel at each enumerated call site

A sentinel means either `Task<Outlook.Table?>` or a result struct.

- `DfDeedle.cs:148` — the only production consumer. It would need a new `if (table is null) throw ...`
  block, which is a verbatim duplicate of the `:186-193` block for a different value. Net effect:
  the null travels one frame further and then produces the same exception the producer could have
  thrown directly. It also widens the declared local from `Outlook.Table` to `Outlook.Table?`, which
  then propagates maybe-null flow state into `:172` and `:176` and would need a `!` or a guard at both
  — that is, the suppression at `TableAccess.cs:138` would be replaced by two suppressions in
  `DfDeedle.cs`. This is a net loss.
- `TableAccess.cs:99` and `:122` — free. The local `table` at `:50` is already `Outlook.Table?`.
- The six reflective binding sites — `Type.GetMethod(name, flags, binder, types, modifiers)` matches
  on **parameter** types only, so a change limited to the return type's *nullable annotation* would
  not break binding (`Task<Outlook.Table?>` is the same runtime type as `Task<Outlook.Table>`; the
  annotation is metadata). A **result struct**, however, changes the runtime return type and would
  break `task.GetType().GetProperty("Result").GetValue(task)` plus every
  `result.Should().BeSameAs(mockTable.Object)` assertion — seven assertion sites across three files.
- `net48` constraint: a result struct cannot use `record struct` or `init` accessors (CS0518, no
  `IsExternalInit`), so it would have to be a plain `readonly struct` with a constructor, adding a new
  `.cs` file and a `Compile Include` edit to `UtilitiesCS.csproj`.

Signature churn under the recommended remedy: **zero**. The nine call sites need **zero** edits.

### 4c. Which exception type, and should cancellation differ?

Yes, they must differ, and the repository already distinguishes them.

| Condition | Type | Rationale |
|---|---|---|
| Outer `token` cancelled | `OperationCanceledException` (as today, including its `TaskCanceledException` subtype) | Caller-initiated shutdown is not a failure. It is already the observed behaviour via `TimeOutTask.cs:50`/`:67` and is pinned by `OlTableExtensions_Tests.cs:1324`. `QfcDatamodel.FrameBuilding.cs:96-101` catches `TaskCanceledException`, restores offline mode, and returns null quietly — the intended quiet path. |
| Acquisition deadline exhausted | `TimeoutException` | Matches `AddQfcColumnsAsync` (`DfDeedle.QfcColumns.cs:152`), `OlTableExtensions.cs:132` and `:136`, and the `TimeoutAfter` helper's own fault type (`TimeOutTask.cs:856`, `:868`, `:925`, `:937`). `TimeoutException` is **not** a `TaskCanceledException`, so it falls to `QfcDatamodel.FrameBuilding.cs:102-108`, which restores offline mode, logs with the method name, and rethrows. That is the desired loud path. |

Blast radius of the new throw, traced by reading (not executed): today a null `table` reaches
`DfDeedle.cs:172`, which passes it into `AddQfcColumnsAsync`, whose work delegate at
`DfDeedle.QfcColumns.cs:119` casts and dereferences it. The resulting `NullReferenceException` is
neither `TaskCanceledException` nor `TimeoutException`, so it escapes that method's catch list and
surfaces at `QfcDatamodel.FrameBuilding.cs:102` with no folder, no attempt count, and a misleading
type. After the fix the same boundary receives a `TimeoutException` naming the deadline and attempt.
The exception **shape** at that boundary is unchanged (log-then-rethrow), so no caller-side control
flow changes.

### 4d. Rejected alternatives (brief)

- **Nullable return / sentinel.** Rejected: it moves one suppression at `TableAccess.cs:138` into two
  suppressions or two guards in `DfDeedle.cs`, duplicates the `:186-193` guard, and — in the result-
  struct form — breaks seven test assertions and requires a new file plus a `.csproj` edit, all to
  serve a single production consumer that has no meaningful null-handling behaviour available to it.
- **Change `RunWithTimeout`'s `strict` argument to `true` at `TableAccess.cs:79`.** Rejected: `strict`
  only affects the generic catch at `TimeOutTask.cs:85-92`; the `TaskCanceledException` retry-
  exhaustion path at `:80-83` returns `default` regardless of `strict`, so the defect would survive.
  It would also change behaviour for unrelated delegate exceptions.
- **Change `TimeOutTask.RunWithTimeout` to throw on exhaustion.** Rejected for this issue: that public
  helper has many other callers across the solution and the change would be an uncontained behaviour
  change. It is a legitimate separate follow-up.

---

## Research question 5 — what each null path becomes

Proposed target state for `OlTableExtensions.TableAccess.cs`. The wording below is a design
specification, not final source.

| Path | Current | Becomes |
|---|---|---|
| 1. Absorbed default (`:74-81` → `:138`) | `return table!;` | Insert before the return: `if (table is null) { token.ThrowIfCancellationRequested(); throw AcquisitionTimeout(counter, timeoutMs); }` then `return table;` with the `!` deleted. The `ThrowIfCancellationRequested` is ordered first so a cancelled outer token is reported as cancellation rather than as a timeout; it is inexpensive and makes the guard correct independently of `RunWithTimeout`'s internals. |
| 2. `:92` — cancellation branch of `catch (TaskCanceledException)` | `table = null;` | `throw;` — rethrow the caught `TaskCanceledException`. Because `TaskCanceledException : OperationCanceledException`, this preserves the contract asserted at `OlTableExtensions_Tests.cs:1324` and keeps the quiet path at `QfcDatamodel.FrameBuilding.cs:96`. Requires no catch-variable binding. |
| 3. `:109` — `counter >= 2` branch of `catch (TaskCanceledException ex)` | `table = null;` | `throw AcquisitionTimeout(counter, timeoutMs, ex);`. Requires binding the catch variable at `:88`. |
| 4. `:132` — `counter >= 2` branch of `catch (TimeoutException ex)` | `table = null;` | `throw AcquisitionTimeout(counter, timeoutMs, ex);`. Requires binding the catch variable at `:113`. |

Outer-token-cancellation path (not one of the four, but explicitly in scope): **unchanged.** The
`OperationCanceledException` raised at `TimeOutTask.cs:50`/`:67` must continue to propagate
uncaught out of `GetTableInViewAsync`. Do not add a `catch (OperationCanceledException)`.

Supporting element: a private factory method rather than three inline `throw new` statements.

```
private static TimeoutException AcquisitionTimeout(
    int counter, int timeoutMs, System.Exception? inner = null) => ...
```

Returning the exception (rather than a `void` method that throws) keeps definite-assignment and
nullable flow analysis correct without a `DoesNotReturnAttribute`, which is not available in the
`net48` BCL. Using one factory for three sites is also a size control: the file is at **452 of 500**
lines, leaving a 48-line budget. Three inline `throw new TimeoutException($"...")` statements with
CSharpier's wrapping would consume most of it. If the budget is still exceeded after formatting, move
the factory into a new partial file `OlTableExtensions.TableAccess.Failures.cs`, which requires a new
`Compile Include` entry adjacent to `UtilitiesCS/UtilitiesCS.csproj:1068`.

Also update, in the same change:
- the comment at `:136-137`, which states the null-on-timeout condition is "pre-existing latent" — it
  will no longer be true;
- the XML documentation of the method, which currently has none, to state the failure contract
  (`TimeoutException` on exhausted deadline, `OperationCanceledException` on caller cancellation,
  `InvalidOperationException` when the current view is not a `TableView`, per `:52-58`);
- the stale explanatory comment at `GetTableInViewAsyncClockTests.cs:166-167`, which asserts in prose
  that advancing past `timeoutMs` "would cancel the acquisition and RunWithTimeout would return
  default, making the returned table null". No assertion depends on it; only the prose is wrong.

---

## Research question 6 — existing test inventory and required assertion changes

Files under `UtilitiesCS.Test/OutlookObjects/Table/` that exercise `GetTableInViewAsync`:

| Test method | File:line | What it drives | Behaviour under the remedy |
|---|---|---|---|
| `GetTableInViewAsync_NullTableView_ThrowsInvalidOperationException` | `OlTableExtensions_Tests.cs:1206` | `CurrentView` is not a `TableView`; throws at `TableAccess.cs:54` before the `try` | **Unchanged.** |
| `GetTableInViewAsync_SlowSynchronousGetTable_ReturnsTableWithoutSyntheticRetry` | `OlTableExtensions_Tests.cs:1237` | factory returns a source the delegate cancels mid-flight; `Task.Run` still completes `RanToCompletion` | **Unchanged** (`:1291-1292`: `BeSameAs`, `callCount == 1`). |
| `GetTableInViewAsync_CanceledToken_PropagatesOperationCanceledException` | `OlTableExtensions_Tests.cs:1296` | pre-cancelled outer token; `TimeOutTask.cs:50` throws | **Unchanged** (`:1324`). This test is the regression guard for path 2 and must keep passing verbatim. |
| `GetTableInViewAsync_ImmediateSuccess_CallsGetTableOnceAndReturnsSnapshot` | `OlTableExtensions_Tests.cs:1620` | happy path with a `FakeTimeProvider` | **Unchanged.** |
| `GetTableInViewAsync_TimeoutSourceThrowsTimeout_EntersTimeoutCatchAndRetriesOnce` | `OlTableExtensionsTimeoutDiagnosticsTests.cs:129` | factory throws `TimeoutException` on call 1 only; `counter: 0`, so `:117` retries and the retry succeeds | **Unchanged** — the `counter >= 2` branch at `:132` is not entered. |
| `GetTableInViewAsync_TimeoutSourceThrowsTaskCanceled_EntersCancelCatchElseAndRetriesOnce` | `OlTableExtensionsTimeoutDiagnosticsTests.cs:185` | same, with `TaskCanceledException`; `:97` retries, succeeds | **Unchanged.** |
| `GetTableInViewAsync_TimeoutRetry_UsesCallerTimeoutMsNotLiteral2000` | `GetTableInViewAsyncClockTests.cs:112` | factory throws once, records `ms` values; retry succeeds | **Unchanged.** |
| `GetTableInViewAsync_InjectedClock_ArmsAcquisitionDeadlineOnInjectedProvider` | `GetTableInViewAsyncClockTests.cs:170` | barrier proves a timer is armed on the injected provider; gate released, clock never advanced | **Unchanged assertions**; the prose comment at `:166-167` becomes wrong and must be corrected. |
| `GetTableInViewAsync_ExplicitFactorySupplied_TakesPrecedenceOverTimeProvider` | `GetTableInViewAsyncClockTests.cs:223` | factory precedence | **Unchanged.** |
| `GetTableInViewAsync_NoTimeProviderSupplied_UsesSystemClockAndCompletes` | `GetTableInViewAsyncClockTests.cs:264` | default system clock | **Unchanged.** |

Indirect, outside that folder:

| `GetEmailDataInViewAsync_EtlDeadlineExpires_ThrowsInvalidOperationNamingFolder` | `UtilitiesCS.Test/Extensions/DfDeedleEtlTimeoutTests.cs:144` | drives `GetTableInViewAsync` through `DfDeedle.cs:148`; releases `gateAcquire` after the acquisition timer arms, and advances only the third timer | **Unchanged** — the acquisition completes normally. |
| `GetEmailDataInViewAsync_ClockNeverAdvances_ReturnsOneRowFrame` | `UtilitiesCS.Test/Extensions/DfDeedleEtlTimeoutTests.cs:212` | green path | **Unchanged.** |

**Result: no existing test assertion requires a change.** That is expected, because no existing test
reaches any of the four null paths — which is precisely why the defect survived. The only mandatory
edits are two stale prose comments (`TableAccess.cs:136-137`,
`GetTableInViewAsyncClockTests.cs:166-167`).

### Signature change and reflective bindings

The recommended remedy **does not change the signature** — neither the parameter list nor the return
type. Every reflective binding therefore continues to resolve. For completeness, the sites that would
have to be updated if a future change did alter the parameter list are:

- `GetTableInViewAsyncClockTests.cs:55-64` — the shared `SignatureTypes` property.
- `OlTableExtensionsTimeoutDiagnosticsTests.cs:53-62` — the shared `SignatureTypes` property.
- `OlTableExtensions_Tests.cs:1216-1224`, `:1274-1282`, `:1307-1315`, `:1638-1646` — four inline
  six-element `Type[]` arrays (this file predates the shared-property refactor).
- The corresponding argument arrays, since `MethodInfo.Invoke` does not apply C# optional-parameter
  defaults without `Type.Missing` plus `BindingFlags.OptionalParamBinding`. This constraint is
  documented in-repo at `DfDeedle_COM_Tests.cs:500-503`; a mismatched arity throws
  `TargetParameterCountException`, not a compile error, so it fails only at run time.

---

## Research question 7 — is a new test file required?

**Yes.**

- `UtilitiesCS.Test/OutlookObjects/Table/OlTableExtensions_Tests.cs` is **1822 lines**, already far
  past the 500-line ceiling in `.claude/rules/general-code-change.md`, and the file itself declares
  the constraint at `:1614` ("this file is at its line ceiling"). Nothing may be added to it.
- `GetTableInViewAsyncClockTests.cs` is **289** lines and `OlTableExtensionsTimeoutDiagnosticsTests.cs`
  is **235** lines. Either could absorb roughly 150 lines, but the four failure-contract tests plus
  their factory helpers and XML documentation are estimated at 180-220 formatted lines, which would
  put `GetTableInViewAsyncClockTests.cs` within about 20 lines of the ceiling. A dedicated file is the
  safer choice and keeps the deadline-mechanics class cohesive.

Proposed file: `UtilitiesCS.Test/OutlookObjects/Table/GetTableInViewAsyncFailureContractTests.cs`.

Owning project: `UtilitiesCS.Test/UtilitiesCS.Test.csproj`. These projects are not SDK-style, so the
file must be added to the `Compile` item list. The new entry belongs among these existing lines
(quoted verbatim from `UtilitiesCS.Test/UtilitiesCS.Test.csproj:547-551`):

```
    <Compile Include="OutlookObjects\Table\OlTableExtensions_Tests.cs" />
    <Compile Include="OutlookObjects\Table\OlTableExtensionsEtlClockTests.cs" />
    <Compile Include="OutlookObjects\Table\GetTableInViewAsyncClockTests.cs" />
    <Compile Include="OutlookObjects\Table\OlTableExtensionsTimeoutDiagnosticsTests.cs" />
    <Compile Include="OutlookObjects\Table\OlToDoTable_Tests.cs" />
```

A second, smaller `Compile Include` group for the same folder exists at `:402-405`; either group is
valid, but placing the new entry immediately after `:549` keeps the `GetTableInViewAsync*` files
adjacent.

No new production file is required, provided the file-size budget at `TableAccess.cs` (452/500) holds
after formatting. If it does not, add `OutlookObjects\Table\OlTableExtensions.TableAccess.Failures.cs`
to `UtilitiesCS/UtilitiesCS.csproj` next to `:1068`.

---

## Research question 8 — test conventions, with concrete in-repo examples

Required by `CLAUDE.md` (C# Unit Test Policy) and `.claude/rules/general-unit-test.md`. Every
technique the new tests need already has an example in the table test files:

| Convention | Concrete existing example |
|---|---|
| MSTest attributes | `[TestClass]` at `GetTableInViewAsyncClockTests.cs:21`; `[TestMethod]` at `:111` |
| Moq for Outlook COM interfaces | `BuildExplorer` at `GetTableInViewAsyncClockTests.cs:31-49`, which mocks `Outlook.Table`, `Outlook.TableView` and `Outlook.Explorer` and wires `CurrentView` |
| `MockBehavior.Strict` where the interaction set is closed | `OlTableExtensions_Tests.cs:1622-1624` |
| FluentAssertions for value assertions | `result.Should().BeSameAs(mockTable.Object, "...")` at `GetTableInViewAsyncClockTests.cs:202-207` |
| FluentAssertions for async exception assertions | `await act.Should().ThrowAsync<OperationCanceledException>();` at `OlTableExtensions_Tests.cs:1324`; `await FluentActions.Awaiting(() => call).Should().ThrowAsync<InvalidOperationException>().WithMessage("*Inbox*");` at `DfDeedleEtlTimeoutTests.cs:192-196` — the latter is the exact shape for asserting the new `TimeoutException` message content |
| `FakeTimeProvider` (`Microsoft.Extensions.Time.Testing`) | `using` at `GetTableInViewAsyncClockTests.cs:7`; instantiated at `OlTableExtensions_Tests.cs:1652` and `GetTableInViewAsyncClockTests.cs:72` |
| Deterministic timer-arming barrier | `UtilitiesCS.Test/TestHelpers/ArmingBarrierTimeProvider.cs`, used at `GetTableInViewAsyncClockTests.cs:71-72` and `DfDeedleEtlTimeoutTests.cs:152` |
| `timeoutSourceFactory` seam instead of wall-clock waits | `ThrowOnFirstCallFactory` at `OlTableExtensionsTimeoutDiagnosticsTests.cs:101-120` |
| Banned-symbol avoidance (`BannedSymbols.txt` lines 10-11 ban both timed `CancellationTokenSource` constructors; RS0030 is held at `suggestion` in `.editorconfig:555`, so this is a convention, not a build gate) | the deliberate note at `OlTableExtensionsTimeoutDiagnosticsTests.cs:96-99`: "The parameterless constructor is used deliberately so this file adds no new banned-symbol call site" |
| No temporary files, no external dependencies | none of the listed files touches the filesystem; `OlTableExtensionsTimeoutDiagnosticsTests.cs:18-19` states this explicitly |
| Release every gate in a `finally` so an orphaned `Task.Run` body cannot outlive the test | `GetTableInViewAsyncClockTests.cs:209-214`; `DfDeedleEtlTimeoutTests.cs:198-204` |
| AAA structure with explicit `// Arrange` / `// Act` / `// Assert` markers | `GetTableInViewAsyncClockTests.cs:114`, `:135`, `:146` |

Coverage obligation: `CLAUDE.md` sets the repository floor at `>= 80%` and new or changed code at
`>= 90%`. (`.claude/rules/general-unit-test.md` states 85/75; the divergence between the two documents
is pre-existing and is not resolved by this issue. Use `CLAUDE.md`, which the policy-compliance order
places first.) The four new tests cover all four converted paths plus the preserved cancellation path,
so every changed line in `TableAccess.cs` is exercised.

Note on project isolation: `[DoNotParallelize]` is applied at `GetTableInViewAsyncClockTests.cs:23`
because that class drives a real `Task.Run` gate. The proposed new tests do **not** need it: the
pre-cancelled-source technique in question 9 never dispatches a work item and never blocks a gate.

---

## Research question 9 — testing the absorbed-default path

### The constraint

The absorbed-default path requires `Task.Run(() => function(), combinedToken.Token)` at
`TimeOutTask.cs:63` to be cancelled **before the work item is dequeued**, on both attempt 0 and
attempt 1. Three facts make the obvious approaches non-deterministic or unusable:

1. Advancing a `FakeTimeProvider` past `timeoutMs` while the call is in flight does not reliably
   pre-empt the work item: if the thread pool has already dequeued it, the delegate runs to completion
   and the task reports `RanToCompletion` regardless of the token. This is the mechanism documented at
   `OlTableExtensions_Tests.cs:1244-1253` and at `DfDeedle.QfcColumns.cs:114-118`. The outcome would
   depend on host scheduling, which violates the determinism requirement.
2. `CancellationTokenSource.CancelAfter` is banned (`BannedSymbols.txt:8-9`) and, per the comment at
   `TableAccess.cs:32-35`, does not terminate the original delay timer on pre-.NET 8 runtimes.
3. Both timed `CancellationTokenSource` constructors are banned (`BannedSymbols.txt:10-11`), so a
   `new CancellationTokenSource(1)` shortcut is not available.

### The deterministic technique

Supply a `timeoutSourceFactory` that returns a **fresh, already-cancelled** source on every
invocation:

```
Func<int, CancellationTokenSource> preCancelledFactory = _ =>
{
    var source = new CancellationTokenSource();   // parameterless: not a banned symbol
    source.Cancel();
    return source;
};
```

Why it works, step by step, with citations:

- `TimeOutTask.cs:50` — the outer token is `CancellationToken.None`, so this check passes.
- `:52-54` — the factory supplies an already-cancelled source.
- `:55-58` — the linked source is created already cancelled.
- `:63` — `Task.Run` with a cancelled token returns a `Canceled` task **without ever invoking the
  delegate**; awaiting it throws `TaskCanceledException`.
- `:67` — the outer token is not cancelled, so no `OperationCanceledException`.
- `:69` — `0 < 1`, so it recurses with `attempt: 1`; the factory is invoked a second time.
- `:69` at `attempt == 1` — `1 < 1` is false, so `:82` logs and `:94` returns `default`, i.e. `null`.
- `TableAccess.cs:88` and `:113` do not fire, so `table` stays null and reaches `:138`.

No wall-clock wait, no timer advance, no gate, no thread-pool dependency.

**A fresh source per invocation is mandatory.** `TimeOutTask.cs:52` holds the source in a `using`
declaration and disposes it at the end of each attempt, so returning the same instance would make the
retry read `.Token` on a disposed source and throw `ObjectDisposedException` — a different path
entirely. This hazard is already documented at `OlTableExtensionsTimeoutDiagnosticsTests.cs:94-99`.

### Proposed new tests (four)

All in `GetTableInViewAsyncFailureContractTests.cs`. Test code is not written here.

1. `GetTableInViewAsync_RunWithTimeoutExhaustsRetries_ThrowsTimeoutException` — the regression test
   for the defect and for path 1. Arrange the pre-cancelled factory and a counting `GetTable`;
   assert `ThrowAsync<TimeoutException>()`, and additionally assert the factory was invoked exactly
   twice while `GetTable` was invoked zero times. Those two counts are the empirical proof that the
   two attempts were internal to `RunWithTimeout` and that `GetTableInViewAsync`'s own recursion at
   `:99`/`:122` never ran — the precise claim in the issue. Before the fix this test fails by
   returning null; after the fix it passes.
2. `GetTableInViewAsync_TimeoutSourceThrowsTaskCanceledAfterCancellingOuterToken_PropagatesCancellation`
   — path 2. Factory cancels the test-owned outer `CancellationTokenSource` and then throws
   `TaskCanceledException`; assert `ThrowAsync<OperationCanceledException>()`. The ordering matters:
   `TimeOutTask.cs:50` runs before the factory at `:52`, so the token must still be uncancelled when
   the method is entered and cancelled by the factory body.
3. `GetTableInViewAsync_CounterAtRetryCeilingWithTaskCanceled_ThrowsTimeoutException` — path 3.
   `counter: 2`, factory always throws `TaskCanceledException`; assert `ThrowAsync<TimeoutException>()`
   and `.WithMessage("*...*")` naming the attempt or timeout value.
4. `GetTableInViewAsync_CounterAtRetryCeilingWithTimeout_ThrowsTimeoutExceptionPreservingInner` —
   path 4. `counter: 2`, factory always throws `TimeoutException`; assert
   `ThrowAsync<TimeoutException>()` and that `InnerException` is the injected instance, which
   distinguishes the new wrapper from a bare rethrow.

All four invoke through the same reflective helper shape as
`GetTableInViewAsyncClockTests.cs:81-102`, since the CS1769 embedded-interop constraint applies
equally.

---

## Summary of required changes

**Production (1 file):** `UtilitiesCS/OutlookObjects/Table/OlTableExtensions.TableAccess.cs`
— convert `:92`, `:109`, `:132` and the absorbed-default path into exceptions; delete the `!` at
`:138`; bind the catch variables at `:88` and `:113`; add a private `AcquisitionTimeout` factory; add
XML documentation; correct the comment at `:136-137`. Watch the 452/500 line budget.

**Production (0 other files):** `UtilitiesCS/Extensions/DfDeedle.cs:148` and
`QuickFiler/Controllers/QfcDatamodel.FrameBuilding.cs:80-109` need no change; the existing
log-and-rethrow boundary already handles the new `TimeoutException` correctly.

**Test (1 new file + 1 comment fix):** new
`UtilitiesCS.Test/OutlookObjects/Table/GetTableInViewAsyncFailureContractTests.cs` plus a
`Compile Include` entry in `UtilitiesCS.Test/UtilitiesCS.Test.csproj` near `:549`; correct the stale
prose comment at `GetTableInViewAsyncClockTests.cs:166-167`. No existing assertion changes.

## Open items and unknowns

- The exact final line count of `TableAccess.cs` after CSharpier formatting is unknown until the
  change is written. If it exceeds 500 the failure factory must move to a new partial file with a
  `Compile Include` edit at `UtilitiesCS/UtilitiesCS.csproj:1068`.
- The `NullReferenceException` chain described in 4c was traced by reading
  `DfDeedle.cs:172` → `DfDeedle.QfcColumns.cs:119` → `:112`. It was not executed, so the precise
  throw site inside the work delegate is inferred, not verified.
- Whether `TimeOutTask.RunWithTimeout` itself should stop returning `default` on exhaustion is a
  larger question affecting every caller of that helper. It is out of scope for #838 and is a
  candidate follow-up.
