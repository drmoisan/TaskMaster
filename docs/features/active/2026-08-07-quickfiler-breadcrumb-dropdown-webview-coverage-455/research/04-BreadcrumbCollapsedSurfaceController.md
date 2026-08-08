# Per-File Research — `QuickFiler/Viewers/BreadcrumbCollapsedSurfaceController.cs`

- Epic: #136 `quickfiler-per-file-coverage`, child F13, feature issue #455
- Production file: `QuickFiler/Viewers/BreadcrumbCollapsedSurfaceController.cs` (308 lines, 192 lines of headroom)
- csproj entry: `QuickFiler/QuickFiler.csproj:404`
- Research date: 2026-08-07
- Builds on: `research/00-cross-cutting-context.md` (shared context; not repeated here)

---

## 0. Headline and acceptance bar

**This file already passes both gates. The acceptance bar is retain-or-improve, not gap closure.**

Recomputed from `docs/features/active/2026-08-06-quickfiler-high-confidence-queue-init-stall-424/evidence/qa-gates/coverage-final.cobertura.xml`
(class element at XML line 13281; denominator taken from `<line>` child count per epic Directive B,
because the `<class>` `line-rate`/`branch-rate` attributes are inflated by open issue **#441**):

| Metric | Value | Floor | Margin |
| --- | --- | --- | --- |
| Line | **193/195 = 98.97%** | 80% | +18.97 |
| Branch | **36/42 = 85.71%** | 75% | +10.71 |

The `<class>` attributes read `line-rate="0.994302" branch-rate="0.858974"`; my recomputation agrees to
within 0.5 and 0.2 points respectively, so the delegating brief's table is confirmed. This file has
the **lowest branch coverage of the four in this batch and of all eight instrumented F13 files**.

Uncovered lines: **exactly two — 198 and 199**. Uncovered branch outcomes: **exactly six**, at
lines 197 (1), 230 (1), and 243 (4).

---

## 1. Structural map

Single type: `internal sealed class BreadcrumbCollapsedSurfaceController : IDisposable`, lines 11-307.
`internal` is reachable from tests via `QuickFiler/Properties/AssemblyInfo.cs:5`
(`[assembly: InternalsVisibleTo("QuickFiler.Test")]`).

### 1.1 Fields and state model

| Line | Field | Role |
| --- | --- | --- |
| 13-15 | `static readonly log4net.ILog log` | error sink for disposal failures only |
| 17 | `readonly object _sync` | the single monitor guarding all mutable state |
| 18 | `TaskCompletionSource<bool> _generationCancellation` | current generation's cancellation signal; `RunContinuationsAsynchronously` |
| 19 | `IWebViewMessenger? _pendingMessenger` | candidate awaiting exact-navigation success |
| 20 | `Task? _pendingReadiness` | the exact readiness task for `_pendingMessenger` |
| 21 | `IDisposable? _pendingReadinessLifetime` | optional owning `BreadcrumbNavigationReadiness` |
| 22 | `Task<bool>? _pendingAttachment` | the in-flight attachment result, returned on idempotent re-attach |
| 23 | `IWebViewMessenger? _readyMessenger` | the single published messenger |
| 24 | `long _generation` | monotonically increasing generation counter |
| 25 | `bool _disposed` | terminal flag |

**State model.** Three states per generation: *idle* (`_pendingMessenger == null && _readyMessenger == null`),
*pending* (`_pendingMessenger != null`), *ready* (`_readyMessenger != null`). Transitions:

- idle → pending: `AttachCore` lines 156-159.
- pending → ready: `CompleteAttachmentAsync` lines 201-203 (only when `IsCurrent` holds).
- pending → idle (rejected): `RejectPending` line 228, or `Reset`/`Dispose`/`AttachCore` via `ClearPending`.
- ready → idle: `Reset` line 71, `Dispose` line 95, `AttachCore` line 150.
- any → terminal: `Dispose` line 91.

**Generation invariant (the load-bearing one).** `InvalidateGeneration` (250-255) is the *only* writer
of both `_generation` and `_generationCancellation`, and it writes both under the caller's `lock (_sync)`.
Therefore `_generation` and `_generationCancellation` **always move together**. This single fact
determines which of the six uncovered branch outcomes are reachable (see §2.3).

### 1.2 Members with line ranges

| Lines | Member | Visibility |
| --- | --- | --- |
| 28-37 | `ReadyMessenger` (get) | internal property, lock-guarded read |
| 40-43 | `AttachAsync(IWebViewMessenger, Task)` | internal; delegates to `AttachCore(..., readinessLifetime: null)` |
| 48-58 | `AttachAsync(IWebViewMessenger, BreadcrumbNavigationReadiness)` | internal; null-guards, then `AttachCore(messenger, readiness.Completion, readiness)` |
| 61-77 | `Reset()` | internal |
| 80-101 | `Dispose()` | public, `IDisposable` |
| 103-173 | `AttachCore(IWebViewMessenger, Task, IDisposable?)` | private |
| 175-219 | `CompleteAttachmentAsync(...)` | private `async Task`, started fire-and-forget at 164 |
| 221-239 | `RejectPending(long, IWebViewMessenger)` | private |
| 241-248 | `IsCurrent(long, Task, IWebViewMessenger)` | private, **must be called under `_sync`** (it reads `_disposed`, `_generation`, `_generationCancellation`, `_pendingMessenger` without taking the lock itself) |
| 250-255 | `InvalidateGeneration()` | private, caller holds `_sync` |
| 257-263 | `ClearPending()` | private, caller holds `_sync` |
| 265-271 | `ThrowIfDisposed()` | private, caller holds `_sync` |
| 273-283 | `ObserveLateFailureAsync(Task)` | private static `async Task`, fire-and-forget at 184 |
| 285-299 | `SafeDispose(IDisposable?)` | private static |
| 301-306 | `NewCompletionSource()` | private static |

### 1.3 Constructor dependencies and seams

**There is no constructor.** The type uses the implicit parameterless constructor; every construction
site is `new BreadcrumbCollapsedSurfaceController()` with no arguments
(`QuickFiler/Viewers/ItemViewer.Breadcrumb.cs:266`;
`QuickFiler.Test/Viewers/BreadcrumbCollapsedSurfaceReadinessTests.cs:395-396`,
`BreadcrumbMessengerHubTests.cs:225`, `BreadcrumbMessengerHubCoverageTests.cs:152,206,274,308,337`,
`BreadcrumbItemViewerLifecycleCoordinatorTests.cs:233`).

Existing seams, named precisely:

1. **`IWebViewMessenger`** (`QuickFiler/Viewers/IWebViewMessenger.cs`) — the only injected collaborator
   type. Every messenger the controller handles arrives as an `AttachAsync` argument, so a test fake
   is trivially substitutable. **This is the seam that makes §8's recommended tests possible.**
2. **`Task readiness`** — the 2-arg `AttachAsync` overload (40-43) takes a bare `Task`. A test-owned
   `TaskCompletionSource<bool>` gives full control over completion timing and outcome, with no
   `Thread.Sleep` and no timer.
3. **`BreadcrumbNavigationReadiness`** — the 3-arg path (48-58); a concrete `internal sealed` class
   in `QuickFiler/Viewers/BreadcrumbWebViewSurfaceFactory.cs:19`, directly constructible from tests
   (`new BreadcrumbNavigationReadiness(surfaceName, detachHandlers)`).
4. **Disposal reentrancy** — `SafeDispose(replacedReadyMessenger as IDisposable)` at line 163 invokes
   caller-supplied code (`IWebViewMessenger.Dispose`) *outside* the lock but *before*
   `CompleteAttachmentAsync` starts. This is an **existing, already-exercisable behavioural seam**
   and it is the key to closing four of the six uncovered branch outcomes without touching production
   code. See §2.4.

**No clock, no `TimeProvider`, no timer, no `CancellationToken`.** The type is entirely
completion-driven. Determinism is achievable purely by controlling task completion order.

---

## 2. Branch inventory

### 2.1 Complete conditional inventory

| file:line | Construct | Cobertura `condition-coverage` | Status |
| --- | --- | --- | --- |
| `:53` | `if (readiness == null)` throw guard | `100% (2/2)` | covered |
| `:87` | `if (_disposed) return;` in `Dispose` | `100% (2/2)` | covered (double-Dispose test at `BreadcrumbCollapsedSurfaceReadinessTests.cs:125-126`) |
| `:109` | `if (messenger == null)` guard | `100% (2/2)` | covered |
| `:113` | `if (readiness == null)` guard | `100% (2/2)` | covered |
| `:126` | `if (ReferenceEquals(_readyMessenger, messenger))` | `100% (2/2)` | covered |
| `:130` | `if (ReferenceEquals(_pendingMessenger, messenger))` | `100% (2/2)` | covered |
| `:132` | `if (ReferenceEquals(_pendingReadiness, readiness))` | `100% (2/2)` | covered |
| `:140` | `if (ReferenceEquals(_pendingReadiness, readiness))` (cross-messenger) | `100% (2/2)` | covered |
| `:189` | `if (!ReferenceEquals(completed, readiness))` — WhenAny arm select | `100% (2/2)` | covered |
| **`:197`** | `if (!IsCurrent(generation, cancellation, messenger))` | **`50% (1/2)`** | **UNCOVERED (1)** — the "not current" jump to 198 is never taken |
| `:206` | `catch (Exception)` (readiness fault/cancel) | not a Cobertura branch | covered (lines 206-207 hits=1) |
| `:212` | `if (!published)` in `finally` | `100% (2/2)` | covered |
| `:226` | `if (generation == _generation && ReferenceEquals(_pendingMessenger, messenger))` | `100% (4/4)` | covered |
| **`:230-232`** | `disposeMessenger = !Ref(_pendingMessenger, m) && !Ref(_readyMessenger, m)` | **`50% (1/2)`** | **UNCOVERED (1)** — the short-circuit taken when `_pendingMessenger == messenger` |
| `:235` | `if (disposeMessenger)` | `100% (2/2)` | covered |
| **`:243-247`** | five-operand `&&` chain in `IsCurrent`, four short-circuit jumps | **`50% (4/8)`** | **UNCOVERED (4)** — every false-jump |
| `:267` | `if (_disposed)` in `ThrowIfDisposed` | `100% (2/2)` | covered |
| `:279` | `catch (Exception)` in `ObserveLateFailureAsync` | not a Cobertura branch | covered |
| `:287` | `if (disposable == null)` in `SafeDispose` | `100% (2/2)` | covered |
| `:295` | `catch (Exception exception)` in `SafeDispose` | not a Cobertura branch | covered (lines 295-298 hits=1) |

No `switch`, no ternary, no `??`, no `?.`, no pattern match, no loop in this file.

### 2.2 The six uncovered outcomes, individually

`IsCurrent` (243-247) is one expression with five operands and therefore four instrumented jumps:

```
243   return !_disposed                                                   // c0
244       && generation == _generation                                     // c1
245       && ReferenceEquals(cancellation, _generationCancellation.Task)   // c2
246       && !cancellation.IsCompleted                                     // c3
247       && ReferenceEquals(_pendingMessenger, messenger);                // (result, no jump)
```

| # | file:line | Uncovered outcome | Verdict |
| --- | --- | --- | --- |
| U1 | `:197` | the `true` arm of `!IsCurrent(...)`, i.e. lines 198-199 | **Closable with existing seams** (see §2.4) |
| U2 | `:243` c0 | `_disposed == true` at publish time | **Closable with existing seams** |
| U3 | `:244` c1 | `generation != _generation` at publish time | **Closable with existing seams** |
| U4 | `:245` c2 | `cancellation` is not the current generation's task while `generation == _generation` | **GENUINELY UNREACHABLE** |
| U5 | `:246` c3 | `cancellation.IsCompleted` while c0-c2 all hold | **GENUINELY UNREACHABLE** |
| U6 | `:230` | `_pendingMessenger == messenger` inside `RejectPending` after the generation check failed | **Closable with existing seams** |

**Why U4 and U5 are unreachable (proof, not assertion).** `InvalidateGeneration` (250-255) is the sole
writer of `_generation` (252) and `_generationCancellation` (253-254), and both writes occur inside the
caller's `lock (_sync)` (`Reset`:65, `Dispose`:85, `AttachCore`:123). `IsCurrent` is only called from
inside `lock (_sync)` at `:195`. Consequently:

- If c1 holds (`generation == _generation`), no `InvalidateGeneration` has run since capture, so
  `_generationCancellation` is still the object whose `.Task` was captured at `:154` — c2 is
  necessarily true. c2's false outcome is unreachable.
- If c1 and c2 hold, the captured task can only have been completed by `TrySetResult(true)` at `:253`,
  which is inseparable from the `_generation++` at `:252`. So `cancellation.IsCompleted` is
  necessarily false — c3's false outcome is unreachable.

U4 and U5 are therefore **defence-in-depth redundancy over an invariant the code already enforces**.
They are not a test gap, they are dead branches. No seam of any kind makes them reachable. The
residual ceiling for this file is **40/42 = 95.24% branch**, and any plan that targets 100% branch
here is targeting an impossible number.

**Nested-lambda instrumentation defect (the sibling-established `[ExcludeFromCodeCoverage]`/lambda
issue): NOT APPLICABLE to this file.** The file carries no `ExcludeFromCodeCoverage` attribute at any
level (grep: zero occurrences), and its only lambda-free async state machines
(`CompleteAttachmentAsync`, `ObserveLateFailureAsync`) are instrumented normally — every one of their
lines except 198-199 is hit.

### 2.3 Why the three closable outcomes were missed by the existing tests

`BreadcrumbCollapsedSurfaceReadinessTests.cs:87-137` already tests `Reset` and `Dispose` against a
pending navigation. Those tests do **not** reach `:197`, and the reason is structural rather than
accidental:

`Reset()`/`Dispose()` complete `_generationCancellation` at `:253`. `CompleteAttachmentAsync` is parked
at `await Task.WhenAny(readiness, cancellation)` (`:188`). The cancellation arm therefore wins, `:189`
evaluates true, and the method **returns at `:191` without ever evaluating `IsCurrent`**. Every
existing invalidation test exits through `:190-191`. The success-then-stale interleaving that reaches
`:197` requires readiness to win `WhenAny` *and* the generation to change before the lock at `:195` —
a window the existing tests cannot open, because in them the invalidation is what releases the await.

The same applies to `BreadcrumbCollapsedSurfaceReadinessTests.cs:140-181`
(`LaterNavigation_InvalidatesEarlierGenerationAndPublishesOnlyCurrentMessenger`): the second
`AttachAsync` invalidates via `AttachCore:149`, releasing the stale attachment through the cancellation
arm as well.

### 2.4 The mechanism that closes U1, U2, U3 and U6 — **no production change required**

`AttachCore` calls `SafeDispose(replacedReadyMessenger as IDisposable)` at `:163`, **after** the lock is
released (`:160`) and **before** `CompleteAttachmentAsync` is started (`:164`). The disposed object is a
test-supplied `IWebViewMessenger`. A fake whose `Dispose()` re-enters the controller therefore mutates
the generation *between* the capture at `:153-154` and the start of the attachment task — exactly the
window §2.3 says is otherwise unreachable.

Combine that with an **already-completed** `readiness` task and the whole of `CompleteAttachmentAsync`
runs synchronously on the test thread:

```
1.  m0 = fake messenger;  controller.AttachAsync(m0, Task.CompletedTask)
      -> AttachCore publishes m0 synchronously (WhenAny fast path), _readyMessenger = m0, gen = G1
2.  arm m0.Dispose() to re-enter the controller exactly once
3.  m1 = fake messenger;  attach = controller.AttachAsync(m1, Task.CompletedTask)
      :148  replacedReadyMessenger = m0
      :149  InvalidateGeneration -> G2, C2
      :153-154 generation = G2, cancellation = C2 captured
      :163  SafeDispose(m0) -> reentrant call runs here  <-- the window
      :164  CompleteAttachmentAsync(m1, CompletedTask, null, G2, C2, ...) runs inline
      :188  WhenAny(readiness[0], C2[1]) -> readiness (argument-order first-completed-wins)
      :197  IsCurrent(G2, C2, m1) -> false  ==> U1, and lines 198-199
```

- Reentrant call = `controller.Reset()` → c1 (`G2 != G3`) is the failing operand → **closes U3 + U1**.
- Reentrant call = `controller.Dispose()` → c0 (`_disposed`) is the failing operand → **closes U2**.
- Reentrant call = `controller.Reset()` followed by `controller.AttachAsync(m1, otherPendingTask)`
  → at `RejectPending(G2, m1)`: `:226` fails on the generation, and `:230` then evaluates
  `!ReferenceEquals(_pendingMessenger /* == m1 */, m1)` as **false** → **closes U6**, and asserts the
  correct behaviour that the re-pended messenger is *not* disposed.

**Determinism caveat, stated honestly.** Step `:188` relies on `Task.WhenAny` resolving to its first
argument when both arguments are already complete. That is the behaviour of the framework's
`CommonCWAnyLogic` completion-action ordering (actions are attached in argument order, and an
already-completed task fires its action inline during attachment, so index 0 wins), and it is stable
across .NET Framework and .NET. It is nonetheless an implementation detail. Mitigations, in order:

1. Assert only observable outcomes (`(await attach) == false`, `ReadyMessenger == null`,
   `m1.DisposeCount`), which hold on **either** `WhenAny` arm, so the test can never be flaky.
2. Verify the branch was actually taken by re-measuring per-file branch coverage after the change; if
   `:197` is still `50% (1/2)`, the tie-break did not go our way.
3. Only if (2) fails, fall back to an injectable-delegate seam (§5.3). Do **not** introduce the seam
   pre-emptively.

---

## 3. Concurrency, ordering, and time

| file:line | Primitive | Notes |
| --- | --- | --- |
| `:17` | `readonly object _sync` | one monitor, all state |
| `:32`, `:65`, `:85`, `:123`, `:195`, `:224` | `lock (_sync)` | six critical sections |
| `:18`, `:301-306` | `TaskCompletionSource<bool>` with `RunContinuationsAsynchronously` | generation cancellation; the flag guarantees continuations are queued, never inlined under the lock |
| `:118`, `:155`, `:181`, `:217` | `TaskCompletionSource<bool> completion` | the per-attachment result, also `RunContinuationsAsynchronously` |
| `:164` | `_ = CompleteAttachmentAsync(...)` | **fire-and-forget async**, not `async void`; the returned task is discarded but every path is wrapped in try/catch/finally |
| `:184` | `_ = ObserveLateFailureAsync(readiness)` | second fire-and-forget, purely to observe late faults so they do not surface as `UnobservedTaskException` |
| `:188` | `await Task.WhenAny(readiness, cancellation).ConfigureAwait(false)` | the only race point in the file |
| `:194`, `:277` | `await readiness.ConfigureAwait(false)` | |
| `:24`, `:252` | `long _generation`, `_generation++` | **not** `Interlocked`/`Volatile`; safe only because every read and write is inside `lock (_sync)`. Verified: reads at `:153`, `:226`, `:244`; writes at `:252`. All lock-guarded. |

- **No `CancellationToken` anywhere.** Cancellation is modelled as a completed `Task`.
- **No `SemaphoreSlim`, no timer, no wall-clock read, no timeout, no `Thread.Sleep`/`Task.Delay`.**
- **No injected clock or `TimeProvider` seam exists, and none is needed** — nothing in this file reads
  time.
- **Thread affinity: none.** The type is not UI-thread bound; it never touches a `Control` or a
  `SynchronizationContext`. It is safe to exercise entirely from an MSTest worker thread.

**Deterministic mechanism required by each currently-untested path:** a test-owned
`TaskCompletionSource<bool>` (or `Task.CompletedTask`) for readiness, plus the reentrant-`Dispose`
fake described in §2.4. No sleeps, no polling, no `Task.Delay`, no real time. Note that
`.claude/rules/general-unit-test.md` bans `Thread.Sleep`/`Task.Delay`/wall-clock waits in tests; the
proposed approach uses none.

**Lock-ordering note.** This file takes only `_sync` and never calls outward while holding it — with
one exception worth recording: `:197` calls `IsCurrent` under the lock, which is pure state reading,
and `:253` calls `TrySetResult` under the lock, whose continuations are guaranteed asynchronous by
`RunContinuationsAsynchronously` at `:304`. That guarantee is what keeps this file clear of the
lock-held-across-outward-call defect recorded for `BreadcrumbDropDownOpenCoordinator.cs:95` in
`00-cross-cutting-context.md` §9 L2. **If anyone ever removes `RunContinuationsAsynchronously` from
`NewCompletionSource` (`:303-305`), this file acquires that defect.** Worth a code comment, but that
is a change this child should not make.

---

## 4. Error paths

| file:line | Construct | Reachable from a unit test today? |
| --- | --- | --- |
| `:53-56` | `throw new ArgumentNullException(nameof(readiness))` | Yes — covered |
| `:67`, `:125` | `ThrowIfDisposed()` → `:269` `throw new ObjectDisposedException` | Yes — covered |
| `:109-112` | `throw new ArgumentNullException(nameof(messenger))` | Yes — covered |
| `:113-116` | `throw new ArgumentNullException(nameof(readiness))` | Yes — covered |
| `:136-138` | `throw new InvalidOperationException("The collapsed messenger already has a pending navigation.")` | Yes — covered |
| `:142-144` | `throw new InvalidOperationException("The pending navigation already belongs to another collapsed messenger.")` | Yes — covered |
| `:134` | `return _pendingAttachment!` — null-forgiving deref | Yes — covered. Safe by construction: `_pendingMessenger` and `_pendingAttachment` are written together at `:156`/`:159` and cleared together at `:259`/`:262`, both under `_sync`. |
| `:206-209` | `catch (Exception) { }` — swallows readiness fault/cancellation | Yes — covered. **Not a bare `catch {}`**: it has an explanatory comment (`:208`) and the outcome is expressed as `published == false`. Acceptable under `.claude/rules/general-code-change.md`. |
| `:279-282` | `catch (Exception) { }` in `ObserveLateFailureAsync` | Yes — covered; commented at `:281`. |
| `:291-298` | `try { disposable.Dispose(); } catch (Exception exception) { log.Error(...) }` | Yes — covered. Logs and swallows; correct for a cleanup path, and it uses the project log4net pattern. |

**No bare `catch {}` (no comment, no logging, no rethrow) exists in this file.** The two
comment-only catches at `:206` and `:279` are deliberate and documented. This file is *not* affected
by the `BreadcrumbPopupUiOperations.cs:349` / `BreadcrumbDropDownOpenLifetime.cs:197` bare-catch
finding recorded by siblings.

**No seam is needed for any error path.** Every guard, every throw, and every catch is already
exercised.

---

## 5. Requirements mapping and candidate approaches

### 5.1 Approach A (RECOMMENDED) — reentrant-disposal fake, tests only

Add one new test file containing three tests that use an `IWebViewMessenger` fake whose `Dispose()`
re-enters the controller once (§2.4). Closes U1, U2, U3, U6 and lines 198-199. No production edit,
no new production file, no csproj production entry, no new ledger row.

Projected result: **195/195 = 100% line, 40/42 = 95.24% branch** (U4/U5 remain, provably unreachable).

Aligns with repo conventions: MSTest + Moq + FluentAssertions, no live form, no popup, no temp file,
no sleep, `internal` visibility already granted.

### 5.2 Approach B (REJECTED) — inject a `Func<Task, Task, Task<Task>>` "when-any" seam

Add a constructor parameter defaulting to `Task.WhenAny` so a test fake can mutate the controller
between the await and the lock. Deterministic without relying on `WhenAny` tie-break ordering.

Rejected because: it adds a production constructor to a type that currently has none, changing five
construction sites including production `ItemViewer.Breadcrumb.cs:266` (F14-owned — see §6); it is a
purely test-motivated API; and Approach A achieves the same coverage with zero production risk. Keep
it documented as the fallback if the §2.4 determinism caveat materialises.

### 5.3 Approach C (REJECTED) — delete the unreachable c2/c3 operands

Simplifying `IsCurrent` to three operands would take branch coverage to 100%. Rejected: the epic's
NFR is no behaviour change, the operands are harmless defensive redundancy, and removing them would
weaken the code against a future refactor that decouples `_generation` from `_generationCancellation`.
Record the analysis in the ledger instead.

### 5.4 Required file changes if Approach A is taken

| File | Change |
| --- | --- |
| `QuickFiler.Test/Viewers/BreadcrumbCollapsedSurfaceGenerationRaceTests.cs` | NEW test file (see §8) |
| `QuickFiler.Test/QuickFiler.Test.csproj` | one new `<Compile Include>` line in the breadcrumb block (lines 60-89), CRLF preserved |
| `QuickFiler/Viewers/BreadcrumbCollapsedSurfaceController.cs` | **no change** |
| `QuickFiler/QuickFiler.csproj` | **no change** |
| epic coverage ledger | update the existing row's measured figures; no new row (no new production file) |

---

## 6. Coupling to sibling-owned files

| Direction | Their file:line | Coupling | Can we mock through an existing interface? |
| --- | --- | --- | --- |
| they → us | `QuickFiler/Viewers/BreadcrumbMessengerHub.cs:280` (`private readonly BreadcrumbCollapsedSurfaceController _controller;`), `:290` (ctor parameter), `:298`, `:315-318`, `:359` — **F12** | The hub owns a controller instance and drives `AttachAsync(messenger, BreadcrumbNavigationReadiness)`. Our type is a **concrete dependency** of F12's hub; there is no `ICollapsedSurfaceController` interface. | N/A for us — we do not need to mock the hub. **Constraint: every `internal` signature on this type is frozen.** Changing the `AttachAsync` overload set or adding a required constructor parameter breaks `BreadcrumbMessengerHub.cs` at compile time. This is the single strongest argument for Approach A over Approach B. |
| they → us | `QuickFiler/Viewers/BreadcrumbItemViewerLifecycleCoordinator.cs:80,89` — **F12** | Passes `Func<Tuple<IWebViewMessenger, BreadcrumbNavigationReadiness>>` candidate factories that ultimately feed our `AttachAsync`. Indirect. | Yes — everything crosses via `IWebViewMessenger`. |
| they → us | `QuickFiler/Viewers/ItemViewer.Breadcrumb.cs:266` (`new BreadcrumbCollapsedSurfaceController()`) — **F14** | The only production construction site. | Not applicable; but it pins the parameterless constructor. |
| we → them | none | This file references **no** sibling-owned type. Its entire dependency surface is `IWebViewMessenger` (F13-owned interface) and `BreadcrumbNavigationReadiness` (F13-owned, declared in our own `BreadcrumbWebViewSurfaceFactory.cs:19`). | — |

`BreadcrumbPopupLifecycleOperations` (`BreadcrumbItemViewerLifecycleCoordinator.cs:355`) and
`BreadcrumbNavigationSubscription` (`:337`) are **not referenced by this file**. F12's likely split of
that 481-line file therefore cannot conflict with our change set.

**Net: this file has zero outbound sibling coupling and four inbound references. Do not change any
signature.**

---

## 7. Existing test inventory

| Test file | Lines | Headroom to 500 | What it asserts about this file |
| --- | --- | --- | --- |
| `QuickFiler.Test/Viewers/BreadcrumbCollapsedSurfaceReadinessTests.cs` | 487 | **13** | The primary fixture. `:20` deferred publication until exact success; `:62` exact-navigation failure leaves no ready messenger; `:87` `Reset` during pending cancels, detaches once, rejects late success, disposes the surface once; `:113` same for double `Dispose`; `:140` a later navigation invalidates the earlier generation and publishes only the current messenger; `:184` viewer attachment caches and replays current state exactly once; `:230` failure/reset/reuse/disposal leave no stale attachment. Harness at `:393-399` owns `Controller` and disposes it. |
| `QuickFiler.Test/Viewers/BreadcrumbMessengerHubCoverageTests.cs` | 478 | 22 | `:152,206,274,308,337` construct the controller as the hub's collaborator; asserts hub-side stale/orphan readiness handling. F12-primary. |
| `QuickFiler.Test/Viewers/BreadcrumbMessengerHubTests.cs` | 414 | 86 | `:225` constructs the controller; `:229,248` candidate tuples. F12-primary. |
| `QuickFiler.Test/Viewers/BreadcrumbItemViewerLifecycleCoordinatorTests.cs` | 327 | 173 | `:233` constructs the controller inside the coordinator harness. F12-primary. |

**There is no `BreadcrumbCollapsedSurfaceControllerTests.cs`.** The de-facto owner fixture is
`BreadcrumbCollapsedSurfaceReadinessTests.cs`, and at 487 lines it has **13 lines of headroom** — not
enough for even one new test method with an arrange block. Per the sibling artifact's §2.3, thirteen
F13-relevant test files sit within 25 lines of the 500-line limit and
`BreadcrumbDropDownIntegrationTests.cs` is at exactly 500. **A new file is mandatory.**

---

## 8. Recommended test-case list

Target file: **`QuickFiler.Test/Viewers/BreadcrumbCollapsedSurfaceGenerationRaceTests.cs`** (new).

Rationale for a new name rather than `BreadcrumbCollapsedSurfaceReadinessTests.Part2.cs`: the three
cases share one distinctive harness (a reentrant-disposal messenger fake) and one theme (the
publish-window generation race), so a named file documents intent better than a `.Part2` continuation.
Either name is acceptable; `.Part2.cs` is the established convention
(`BreadcrumbDropDownOpenCoordinatorTests.Part2.cs`, `BreadcrumbPopupBoundaryCoverageTests.Part2.cs`)
if the planner prefers uniformity.

Shared arrange (one `private sealed class ReentrantDisposeMessenger : IWebViewMessenger, IDisposable`
fake with an `Action? OnFirstDispose` hook and a `DisposeCount`; construct with plain `new`, no Moq
needed because the fake needs behaviour, not verification).

| # | Test name | Closes | Arrange / Act / Assert sketch | One atomic task? |
| --- | --- | --- | --- | --- |
| T1 | `PublishWindow_ResetDuringReplacedMessengerDisposal_RejectsStaleGeneration` | U1, U3, lines 198-199 | Publish `m0` via `AttachAsync(m0, Task.CompletedTask)`; arm `m0.OnFirstDispose = () => controller.Reset()`; act `attach = AttachAsync(m1, Task.CompletedTask)`; assert `(await attach).Should().BeFalse()`, `controller.ReadyMessenger.Should().BeNull()`, `m1.DisposeCount.Should().Be(1)`. | Yes |
| T2 | `PublishWindow_DisposeDuringReplacedMessengerDisposal_RejectsStaleGeneration` | U2 | As T1 but `m0.OnFirstDispose = () => controller.Dispose()`; assert the attachment resolves `false`, `ReadyMessenger` is null, `m1` disposed exactly once, and a later `AttachAsync` throws `ObjectDisposedException`. | Yes |
| T3 | `RejectPending_MessengerRePendedUnderNewGeneration_IsNotDisposed` | U6 | As T1 but `m0.OnFirstDispose = () => { controller.Reset(); controller.AttachAsync(m1, pendingTcs.Task); }`; assert the *first* attachment resolves `false` **and** `m1.DisposeCount.Should().Be(0)` — the re-pended messenger must survive the stale rejection. | Yes |

Each test is deterministic (all readiness tasks are `Task.CompletedTask` or a test-owned TCS that is
never completed), uses no live form, no popup, no temp file, no sleep, and no external service. Each
is independently runnable and does not share mutable static state, which matters because
`scripts/vscode/TaskMaster.cli.runsettings` enables `Scope=ClassLevel` parallelism.

**Explicitly NOT recommended:**

- No test for U4 (`:245` c2) or U5 (`:246` c3) — proven unreachable in §2.2. Record them in the ledger
  as unreachable defensive operands with the proof, and set this file's branch expectation at
  95.24%, not 100%.
- No shape-assertion or reflection-existence tests. The epic prohibits them
  (`epic.md:521-522`) and this file needs none.
- No additional guard-clause tests: all eleven throw paths are already covered.

If the orchestrator's change budget is tight, **T1 is the highest-value single task** (it closes three
of the six outcomes and both uncovered lines) and T2/T3 can be deferred without breaching any gate.

---

## 9. csproj impact

- **`QuickFiler/QuickFiler.csproj`: no change.** No new production file. Existing entry stays at
  `:404` inside the contiguous F13 block (`:396-411`).
- **`QuickFiler.Test/QuickFiler.Test.csproj`: one new line.** Insert
  `    <Compile Include="Viewers\BreadcrumbCollapsedSurfaceGenerationRaceTests.cs" />`
  adjacent to `:76` (`Viewers\BreadcrumbCollapsedSurfaceReadinessTests.cs`), inside the breadcrumb
  block at `:60-89`.
- **CRLF must be preserved.** Both projects are non-SDK with explicit compile lists and CRLF line
  endings (`QuickFiler.csproj`: 593 of 593 lines CRLF-terminated). Use the `Edit` tool or
  `perl -0777` with explicit `\r\n`. A git-bash `sed -i` strips CRLF and produces a whole-file diff
  that is guaranteed to conflict at epic fan-in (`epic.md:610-612`).
- **Coverage ledger:** update the existing `testable` row's measured figures. **No new row** — no new
  production file, so the `>= 90%` new-file rule does not engage.

---

## 10. Latent defects

**None found in this file that is not already recorded by a sibling.** Specifically:

- No bare `catch {}` (§4). The `BreadcrumbPopupUiOperations.cs:349` /
  `BreadcrumbDropDownOpenLifetime.cs:197` finding does not extend here.
- No lock held across an outward call. The `BreadcrumbDropDownOpenCoordinator.cs:95` finding does not
  extend here — but see the fragility note below.
- The null-forgiving `_pendingAttachment!` at `:134` is **safe by construction** and is *not* an
  instance of the `BreadcrumbDropDownOpenLifetime.cs:229-230` null-forgiving-deref finding: the field
  is written and cleared in lockstep with `_pendingMessenger` under the same lock.
- The nested-lambda `[ExcludeFromCodeCoverage]` instrumentation defect does not apply (no attribute
  in this file).

Two observations recorded for completeness, both **below the bar for issue promotion**:

| ID | file:line | Observation | Why not promoted |
| --- | --- | --- | --- |
| O1 | `:245-246` | `IsCurrent`'s c2 and c3 operands are permanently unreachable given that `InvalidateGeneration` (`:250-255`) is the sole, atomic writer of both `_generation` and `_generationCancellation`. They are permanently-uncoverable defensive redundancy. | Not a defect — correct-but-redundant code. It *is* a required input to the coverage ledger (this file's branch ceiling is 95.24%, not 100%) and must be recorded there. |
| O2 | `:303-305` | The `RunContinuationsAsynchronously` flag on `NewCompletionSource` is load-bearing for lock safety: `TrySetResult(true)` at `:253` runs inside `lock (_sync)` in `Reset`/`Dispose`/`AttachCore`. Removing the flag would let arbitrary continuations execute under the controller's monitor, creating exactly the lock-ordering hazard recorded as L2 for `BreadcrumbDropDownOpenCoordinator.cs:95`. | Currently correct; there is no defect to fix. The risk is a *future* edit. The cheapest mitigation is a one-line explanatory comment, which is a behaviour-neutral change this child could make — but it is optional and outside the coverage mandate. |

---

## 11. Deviations from the delegation brief

| Brief statement | Finding |
| --- | --- |
| "`BreadcrumbCollapsedSurfaceController.cs` ~99.0-99.4% line, ~85.7-85.9% branch" | **Confirmed.** Recomputed 193/195 = 98.97% line, 36/42 = 85.71% branch by `<line>`-child-count and `condition-coverage` summation. |
| "the weakest branch coverage in the entire child" | **Confirmed** against the other seven instrumented F13 files (next lowest: `BreadcrumbPopupUiOperations.cs` at 86.88%). |
| "pinpoint the ~6 uncovered conditions" | **Exactly six**, itemised in §2.2 as U1-U6 with file:line. |
| Implied premise that residual branches need new seams | **Partly refuted.** Four of the six are closable with *existing* seams via reentrant disposal (§2.4); two are provably unreachable and closable by nothing. **Zero production changes are required.** |
| "state whether an injected clock/`TimeProvider` seam exists" | Neither exists **and neither is needed** — the file reads no clock and has no timer (§3). |

---

*No commands were executed in this session; all findings are derived from the working-tree files and
the committed Cobertura report cited in §0, with exact paths and line numbers given throughout.*
