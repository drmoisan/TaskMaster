# Code Review — Issue #648 (`bug/wpfuidispatchertests-ungated-static-swap-648`)

- Timestamp: 2026-09-01T14-06
- Branch HEAD: `08868ba0ddc6036a49c3cdaf95b6993315b30aec`
- Base: `origin/main` at `c7b4f08f6d80296840f9a351042cb2113892e95f`
- Reviewed source change: `QuickFiler.Test/Controllers/WpfUiDispatcherTests.cs` (+50/-34), the only
  `.cs` path in the branch diff.
- Consumed unchanged: `QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixture.cs`
  (`UiThreadDispatcherFixture`, `UiThreadDispatcherTransaction`).

## Verdict

**Approve. Blocking findings: 0.** Three non-blocking findings, all recorded below with reachability.

The change does what the issue asked and slightly more: it removes the ungated reflection write, it
puts the swap under both locks introduced by #493, and it replaces an unconditional restore with the
transaction's conditional compare-then-write. It also incidentally fixes a smaller latent problem the
issue did not name — see "Improvement beyond the acceptance criteria" below.

## CR-1 — Executor deviation from P1-T2's directed expression: ACCEPT

Plan task P1-T2 directed a single expression:

```
await UiThreadDispatcherFixture.BeginTransactionAsync().ConfigureAwait(false)
```

The executor delivered two statements at `WpfUiDispatcherTests.cs:58-60`:

```csharp
                Task<UiThreadDispatcherTransaction> gate =
                    UiThreadDispatcherFixture.BeginTransactionAsync();
                UiThreadDispatcherTransaction transaction = await gate.ConfigureAwait(false);
```

This was judged, not assumed. Four questions were asked and answered independently.

### Was the deviation actually forced?

Yes. The reviewer reproduced the formatter behaviour with a scratch probe rather than accepting the
executor's account. A throwaway file containing the directed expression at the same 16-column indent,
formatted with the manifest-pinned CSharpier 1.2.6 (`dotnet-tools.json`, invoked through
`dotnet tool run`) under the repository's default 100-column print width — no `.csharpierrc` exists
and `.editorconfig` sets no `max_line_length` — is rewritten to:

```csharp
                UiThreadDispatcherTransaction transaction = await UiThreadDispatcherFixture
                    .BeginTransactionAsync()
                    .ConfigureAwait(false);
```

The full single-line statement is 138 columns, so it cannot survive the 100-column width, and
CSharpier resolves that by breaking the member chain rather than after the `=`. In the resulting
shape the qualified call `UiThreadDispatcherFixture.BeginTransactionAsync` spans two lines and matches
no single line, which makes P1-T6's first token assertion unsatisfiable for any executor who followed
P1-T2 literally. CSharpier reprints from the syntax tree and ignores input line breaks, so no
hand-written arrangement of the same expression can produce a different output. The plan was
internally contradictory; the executor's deviation is the correct resolution of that contradiction,
not a shortcut around a gate.

### Is the delivered form semantically equivalent?

Yes. `BeginTransactionAsync()` is invoked once and returns one `Task<UiThreadDispatcherTransaction>`;
binding that task to a local changes nothing about it. `ConfigureAwait(false)` is then applied to the
same task instance and awaited. `ConfigureAwait` is a pure accessor on `Task<T>` that returns a
`ConfiguredTaskAwaitable<T>` struct; it has no side effect and does not depend on whether its receiver
arrived from an invocation expression or from a local. Continuation behaviour, exception propagation,
and synchronization-context suppression are identical in both forms.

### Does the split introduce a new failure mode between `:59` and `:60`?

No. Three specific hazards were examined.

1. **An exception thrown after the task is started but before it is awaited.** No user code executes
   between the two statements. `BeginTransactionAsync` is declared `async Task<T>`, so by the C#
   async method contract every exception it raises — including one raised before its first `await` —
   is captured into the returned task rather than thrown synchronously at the call site. Assigning a
   reference to a local cannot throw, and `ConfigureAwait(false)` on a non-null task cannot throw.
   The only remaining interrupts are asynchronous ones (thread abort, out-of-memory, an MSTest
   `[Timeout]` abort), and each of those is equally possible at exactly the same point inside the
   single-expression form, because that form compiles to the same call-then-await sequence. The
   split is therefore failure-mode-neutral.

2. **A leaked `TransactionGate` permit.** `BeginTransactionAsync` acquires the semaphore permit
   *before* it constructs the `UiThreadDispatcherTransaction`
   (`QfcItemController.UiThreadDispatcherFixture.cs:122-126`), so there is a window in which the
   permit is held and no disposable owner exists. That window is a property of the two-phase design
   inherited from #493 and is byte-for-byte identical in both the directed and the delivered form.
   It is not created, widened, or narrowed by the split.

3. **Definite assignment of `transaction` relative to the inner `finally` at `:95`.** `transaction`
   is declared and assigned at `:60`, before the inner `try` opens at `:61`. The inner `finally` can
   only run if control entered the inner `try`, which requires `:60` to have completed normally.
   `transaction` is therefore definitely assigned and non-null at every point the inner `finally` can
   execute, and it cannot be observed in a partially-initialised state. The compiler agrees: P1-T3's
   project rebuild reports `0 Error(s)` with no CS0165, and the reviewer executed the resulting
   assembly directly.

Conversely, if the await at `:60` were to throw, the inner `try` is never entered and `Dispose` is
never called. In this fixture that path is unreachable: `TransactionGate.WaitAsync()` is called with
no cancellation token and no timeout, and the semaphore is a `static readonly` field that is never
disposed, so the only documented throwing condition (`ObjectDisposedException`) cannot arise.

### Is the deviation adequately documented?

Yes, at two levels. The code comment at `:56-57` states the mechanism —

```csharp
                // Split across two statements so the qualified call stays on one line: CSharpier
                // wraps the single-expression form into a three-line member chain at this indent.
```

— and `evidence/regression-testing/p1-t6-ac3-fixture-routing.md:51-83` records it explicitly as a
deviation from P1-T2's literal text, gives the column arithmetic, cites the sibling call site that
exhibits the chain break, states that the executor confirmed it empirically rather than by inference,
and asserts the semantic equivalence. That is the standard this repository expects for a directed-text
deviation. One small improvement would be for the code comment to name the constraint it serves
(P1-T6's single-line token assertion) rather than only the formatter behaviour, since a future reader
of the source alone cannot tell why one-line contiguity mattered. Trivial; not worth a change.

**CR-1 disposition: accepted. Not a defect. Reachability of any harm: none — no execution path
differs from the directed form.**

## CR-2 — Is the lock protocol actually honoured? YES

The point of #648 is participation in `FieldLock` and `TransactionGate`, not merely the removal of
reflection. Each obligation was checked against
`QfcItemController.UiThreadDispatcherFixture.cs` rather than against the test file's intent.

| Obligation | Mechanism | Verdict |
|---|---|---|
| Gate held for the entire install-to-restore span | `BeginTransactionAsync` awaits `TransactionGate.WaitAsync()` at `:124` and returns only after acquiring. The permit is released solely by `UiThreadDispatcherTransaction.Dispose` via `ReleaseTransactionGate` at `:275`. In the test the gate is acquired at `:59-60` and released at `:95`, spanning `Install` at `:63` and every assertion through `:91`. | HELD |
| Read-modify-write is atomic | `Install` at `:242-254` delegates to `Exchange` at `:55-63`, which performs `GetValue` then `SetValue` inside a single `lock (FieldLock)` with no await, no wait, and no thread creation inside the critical section. | ATOMIC |
| Restore is conditional, not unconditional | `Dispose` at `:261-276` calls `CompareExchange(_installedValue, _previous)` at `:272`. `CompareExchange` at `:70-82` takes `FieldLock`, evaluates `ReferenceEquals(DispatcherField.GetValue(null), expected)`, returns `false` without writing when the identity check fails, and writes only when it holds. This is exactly the compare-then-write the issue's Expected Behavior section demands, and it replaces the pre-change `field.SetValue(null, original)` that wrote unconditionally. | CONDITIONAL |
| Restore precedes gate release | Inside `Dispose`, `CompareExchange` at `:272` runs before `ReleaseTransactionGate` at `:275`, so a waiter released by this transaction can never observe the pre-restore value. | ORDERED |
| Restore precedes dispatcher shutdown | `transaction.Dispose()` is in the inner `finally` at `:95`; `ShutdownDispatcher(dispatcher)` is in the outer `finally` at `:100`. The static no longer references the test's dispatcher by the time `InvokeShutdown` is called. This preserves the pre-change relative order. | ORDERED |
| No second reflection owner introduced | The quoted literal `"_dispatcher"` now appears on exactly one tracked `*.cs` line beneath `QuickFiler.Test/` — `QfcItemController.UiThreadDispatcherFixture.cs:136` — verified by the reviewer with `git grep -n -F '"_dispatcher"' -- 'QuickFiler.Test/*.cs'`. | SINGLE OWNER |
| No deadlock introduced | Lock ordering is `TransactionGate` then `FieldLock`, never the reverse; `EnsureDispatcher` deliberately takes only `FieldLock`; no gate holder blocks on a resource another gate holder owns. `[Timeout(60000)]` bounds the failure. Reviewer ran the five dispatcher-touching classes together at `ClassLevel` scope with 24 workers: 47/47 passed in 1.45 s. | NO CYCLE |
| Double-install and double-dispose guarded | `Install` throws `InvalidOperationException` on a second call (`:244-249`); `Dispose` short-circuits on `_disposed` (`:263-266`), so no `SemaphoreFullException` can arise from the single call site at `:95`. | GUARDED |

**CR-2 disposition: the lock protocol is honoured in full. The fix is real, not cosmetic.**

## Improvement beyond the acceptance criteria (worth noting for the merge record)

The pre-change code read the previous value *outside any lock* and *before* the dispatcher thread was
started:

```csharp
            object original = field.GetValue(null);
            Dispatcher dispatcher = QfcItemControllerTestSupport.StartRunningDispatcher();
```

The delivered code captures the previous value inside `Exchange`, under `FieldLock`, at the moment of
the write. The captured value can therefore no longer be stale relative to the value being replaced.
No acceptance criterion asked for this; it falls out of using the fixture. `StartRunningDispatcher`
does not touch the static (`QfcItemController.TestSupport.cs:251-271`), so moving the capture after
it changes nothing else.

## Design and readability review (General Code Change Policy)

| Principle | Assessment |
|---|---|
| Simplicity first | The test body lost 12 lines of reflection plumbing and gained a nested `try`/`finally`. Net readability improves: the reader now sees a named transaction rather than `FieldInfo`/`BindingFlags` mechanics. |
| Reusability | The change consumes the existing shared fixture rather than duplicating it. This is the correct direction and is precisely what #493's residual R-1 asked for. |
| Separation of concerns | The static-mutation concern now lives entirely in the fixture; the test file contains only dispatch assertions and lifecycle scaffolding. |
| Error handling | Failures surface as exceptions from `Install`/`Dispose` or as assertion failures. No broad catch. The `[Timeout]` converts a hang into a failure. |
| Naming | `gate`, `transaction`, `GateTimeoutMs` are descriptive and consistent with the sibling file, which uses the same `GateTimeoutMs` constant name and value. |
| Comments explain why, not what | The `<para>` block at `:37-46` and the inline comment at `:56-57` both explain rationale. |
| Public API stability | No production type or signature changed. The only signature change is the test method's own, from `void` to `async Task`, which MSTest supports directly. |
| File size | 104 lines against a 500-line limit. |
| No new dependency | `using UtilitiesCS;` and `using System.Reflection;` were removed; nothing was added. The remaining `using` set is minimal for what the file now references. |

## Test-quality review (General + C# Unit Test Policy)

- Independence and isolation: improved. The test no longer races other classes for the static.
- Determinism: improved for the same reason. `signal.Wait()` at `:89` is an unbounded event wait
  satisfied by the delegate posted at `:84`; `invokeAsyncTask.GetAwaiter().GetResult()` at `:77` is a
  completion wait. Neither is a wall-clock delay. Both are pre-existing.
- Scenario coverage: the method asserts the positive flow for all three dispatch shapes (`Invoke`,
  `InvokeAsync`, `BeginInvoke`) against the dispatcher's own managed thread id. It asserts no negative
  or error path, which is unchanged from the pre-change file and is outside what AC-4 permits to
  change on this branch.
- `[Timeout(GateTimeoutMs)]` with `GateTimeoutMs = 60000` matches the sibling #493 regression class
  exactly, including the constant's name, value, and placement immediately after the class opening
  brace.
- Blocking calls now execute on a thread-pool continuation rather than the MSTest thread, because
  `ConfigureAwait(false)` suppresses context capture. This is safe here: the work being awaited is
  performed by a dedicated STA dispatcher thread, not by the thread pool, so there is no
  pool-starvation dependency. Observed behaviour confirms it — the reviewer's rerun completed the
  method in 40 ms.

## Non-blocking findings

### CR-F1 — Raw Cobertura blobs are still reachable in branch history (Minor; reachable with certainty)

Detailed in `policy-audit.2026-09-01T14-06.md` finding F-1. Two ~10.6 MB blobs entered history in
`8d933975` and were removed only in `08868ba0`. Squash-merging the pull request drops them without a
history rewrite; a merge commit makes the 21 MB permanent. This is a merge-method recommendation, not
a code change.

### CR-F2 — Evidence `Timestamp:` values are not observed clock readings (Minor; present on all 42 artifacts)

Detailed in `policy-audit.2026-09-01T14-06.md` finding F-2, including the self-contradicting artifact
that records `Timestamp: 2026-09-01T13-38` while quoting an MSBuild banner reading `1:24:24 PM` inside
its own output section. The gate outcomes themselves were cross-checked against recorded elapsed
times, the surviving coverage document's mtime and byte size, and an independent rerun, and they hold.
Only the ordering claims that rest on these stamps are unreliable.

### CR-F3 — Code comment at `:56-57` names the mechanism but not the constraint (Trivial; cosmetic)

The comment explains that CSharpier wraps the single-expression form, but not why one-line contiguity
was required (P1-T6's token assertion). A source-only reader cannot recover the reason. Fixing it
would require touching the file again for no behavioural gain; recorded, not requested.

## Reviewer verification actions

All of the following were executed by this reviewer, not read from evidence:

1. `git merge-base origin/main HEAD` — confirmed the supplied base rather than trusting it.
2. `git grep -n -F '"_dispatcher"' -- 'QuickFiler.Test/*.cs'` and `-- '*.cs'` — 1 and 4 matches.
3. `git grep -n -E 'GetField|SetValue|using System\.Reflection;' -- <the changed file>` — no match.
4. `awk 'END{print NR}'` on the changed file — 104 lines.
5. CSharpier 1.2.6 scratch probe reproducing the chain break on the directed expression.
6. `vstest.console.exe` from `Common7\IDE\Extensions\TestPlatform` against
   `QuickFiler.Test\bin\Debug\QuickFiler.Test.dll` with the repository runsettings, `/InIsolation`,
   and a `WpfUiDispatcherTests` filter — 2/2 passed, exit 0.
7. The same runner with a filter spanning all five dispatcher-touching classes — 47/47 passed in
   1.45 s under `ClassLevel` parallelism with 24 workers.
8. Arithmetic re-summation of every `<counter>` element in both JaCoCo projections against the
   Cobertura root counters the plan artifacts quote.
9. `sha256sum` comparison of `artifacts/csharp/coverage.xml` against the committed head projection.
10. `git ls-tree -l 8d933975` on both removed Cobertura paths to size the history residue.
11. `git log --format=%ad --date=iso-local` on individual evidence files versus their own `Timestamp:`
    fields.
12. `.github/workflows/_mstest-coverage.yml` inspection plus a repository-wide grep for `Settings:`
    across `.github/workflows/`, confirming the fail-before dossier's CI-dormancy claim.
