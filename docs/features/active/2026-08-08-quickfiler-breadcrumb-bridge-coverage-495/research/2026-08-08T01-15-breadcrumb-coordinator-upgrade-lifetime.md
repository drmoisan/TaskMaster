# Research: `QuickFiler/Viewers/BreadcrumbCoordinatorUpgradeLifetime.cs` (F12 / issue #495)

- Timestamp: 2026-08-08T01-15
- Epic: `docs/features/epics/quickfiler-per-file-coverage/epic.md` (#136), child F12
- Child issue: #495
- Branch: `feature/quickfiler-breadcrumb-bridge-coverage-r2` (based on `epic/quickfiler-per-file-coverage-integration`)
- Scope: ONE production file, per the #136 one-research-artifact-per-file mandate.
- Companion artifact: `2026-08-08T01-15-breadcrumb-bridge-coordinator.md` (this artifact resolves its
  forward references **H3** and **LD-A**).

---

## 1. Current State — verified

### 1.1 File shape

`QuickFiler/Viewers/BreadcrumbCoordinatorUpgradeLifetime.cs` is **309 physical lines** (last line `}`
at `BreadcrumbCoordinatorUpgradeLifetime.cs:309`). Against the 500-line ceiling in
`.claude/rules/general-code-change.md` § File Size Limit that is **191 lines of headroom** — by far
the most comfortable margin of any F12 file (the sibling coordinator has 13).

- **Two** type declarations, both in namespace `QuickFiler.Viewers`:
  - `internal sealed class BreadcrumbUpgradeLease` (`:9`) — **not `partial`**.
  - `internal sealed class BreadcrumbCoordinatorUpgradeLifetime : IDisposable` (`:35`) — **not
    `partial`**. The sibling artifact's statement that this type is `internal sealed` at `:35` is
    **confirmed exactly**.
- **No `[ExcludeFromCodeCoverage]` anywhere in the file** (verified by targeted grep — zero matches).
- `#nullable enable` at `:1`. Only three `using` directives (`System`, `System.Threading`,
  `System.Threading.Tasks`) — no WinForms, no Outlook Interop, no WebView2, no log4net.
- Registered in the compile set at `QuickFiler/QuickFiler.csproj:394`.
- `QuickFiler/Properties/AssemblyInfo.cs:5` carries `[assembly: InternalsVisibleTo("QuickFiler.Test")]`,
  so **every `internal` member of both types is directly callable from `QuickFiler.Test`**. It already
  is — `QuickFiler.Test/Viewers/BreadcrumbCoordinatorUpgradeLifetimeTests.cs:19-33`.

**`Dispose()` at `:221` is the only `public` member in the entire file** (an explicit `IDisposable`
implementation). Everything else is `internal` or `private`. No production edit is therefore
constrained by any public-API compatibility concern.

### 1.2 API surface — exact signatures with line anchors

`BreadcrumbUpgradeLease` (`:9`):

| Member | Line | Signature |
| --- | --- | --- |
| ctor | `:13` | `internal BreadcrumbUpgradeLease(long generation, CancellationTokenSource source)` |
| `Generation` | `:19` | `internal long Generation { get; }` |
| `Token` | `:20` | `internal CancellationToken Token => _source.Token;` |
| `CancellationStarted` | `:21` | `internal bool CancellationStarted { get; set; }` |
| `Cancelled` | `:22` | `internal bool Cancelled { get; set; }` |
| `Settled` | `:23` | `internal bool Settled { get; set; }` |
| `SourceDisposed` | `:24` | `internal bool SourceDisposed { get; set; }` |
| `Cancel` | `:26` | `internal void Cancel() => _source.Cancel();` |
| `DisposeSource` | `:28` | `internal void DisposeSource() => _source.Dispose();` |

`BreadcrumbCoordinatorUpgradeLifetime` (`:35`):

| Member | Line | Signature |
| --- | --- | --- |
| ctor | `:43` | `internal BreadcrumbCoordinatorUpgradeLifetime(Action<Exception> report)` |
| `BeginPopulation` | `:48-50` | `internal BreadcrumbUpgradeLease BeginPopulation(CancellationToken cancellationToken = default(CancellationToken))` |
| `Invalidate` | `:72` | `internal bool Invalidate()` |
| `Abandon` | `:89` | `internal void Abandon(BreadcrumbUpgradeLease lease)` |
| `IsCurrent` | `:103` | `internal bool IsCurrent(BreadcrumbUpgradeLease lease)` |
| `RunSynchronous` | `:111` | `internal void RunSynchronous(BreadcrumbUpgradeLease lease, Action operation)` |
| `Guard` | `:124` | `internal Action Guard(BreadcrumbUpgradeLease? lease, Action action)` |
| `TryRunCurrent` | `:133` | `internal bool TryRunCurrent(BreadcrumbUpgradeLease lease, Action action)` |
| `RunAsync` | `:150-153` | `internal async Task RunAsync(BreadcrumbUpgradeLease lease, Func<CancellationToken, Task> operation)` |
| `RunAsync<T>` | `:179-183` | `internal async Task RunAsync<T>(BreadcrumbUpgradeLease lease, Func<CancellationToken, Task<T>> operation, Func<T, Task> publishCurrent)` |
| `TryDispose` | `:203` | `internal bool TryDispose()` |
| `Dispose` | `:221` | `public void Dispose()` |
| `IsGenerationCurrent` | `:229` | `private bool IsGenerationCurrent(BreadcrumbUpgradeLease lease)` |
| `IsGenerationCurrentCore` | `:237` | `private bool IsGenerationCurrentCore(BreadcrumbUpgradeLease lease)` (expression-bodied) |
| `Complete` | `:240` | `private void Complete(BreadcrumbUpgradeLease lease)` |
| `CancelLease` | `:258` | `private void CancelLease(BreadcrumbUpgradeLease? lease)` |
| `DisposeLease` | `:297` | `private void DisposeLease(BreadcrumbUpgradeLease lease)` |

> **Correction to the brief.** The delegation brief enumerated `Guard`, `TryRunCurrent`, `IsCurrent`,
> `BeginPopulation`, `RunSynchronous`, `Invalidate`, `TryDispose` and the lease type. It omitted
> **`Abandon` (`:89`), both `RunAsync` overloads (`:150`, `:179`), and `Dispose()` (`:221`)**.
> `Abandon` and `RunAsync<T>` are load-bearing for the gap analysis below.

### 1.3 Collaborators and their owning child

| Symbol | Declared at | Owner |
| --- | --- | --- |
| `BreadcrumbBridgeCoordinator` — **the sole production consumer** | `QuickFiler/Viewers/BreadcrumbBridgeCoordinator.cs:25` | **F12** |
| `BreadcrumbUiDispatcher.Report(Exception)` — supplies the `_report` sink | declared `QuickFiler/Viewers/BreadcrumbUiDispatcher.cs:238`; type at `:12`; wired as a method group at `BreadcrumbBridgeCoordinator.cs:56` | **F13 (#455)** |
| `BreadcrumbUiDispatcher.Dispatch(Action)` — executes the `Guard` result | `QuickFiler/Viewers/BreadcrumbUiDispatcher.cs:71` (inline branch at `:84`) | **F13 (#455)** |
| `BreadcrumbMessengerHub.PostJson` — reached transitively from `TryRunCurrent`'s action | `QuickFiler/Viewers/BreadcrumbMessengerHub.cs:15`, method at `:119` | **F12** |
| `IWebViewMessenger` | `QuickFiler/Viewers/IWebViewMessenger.cs:13` | **F13 (#455)** |
| `CancellationTokenSource` / `CancellationToken` | BCL `System.Threading` | n/a |

**Complete caller census across `QuickFiler/`.** A repository-wide grep for
`BreadcrumbCoordinatorUpgradeLifetime|BreadcrumbUpgradeLease` returns production hits in exactly
**one** file besides the declaration: `BreadcrumbBridgeCoordinator.cs`.

| Call site | Member invoked |
| --- | --- |
| `BreadcrumbBridgeCoordinator.cs:31` | field declaration |
| `BreadcrumbBridgeCoordinator.cs:56` | `new BreadcrumbCoordinatorUpgradeLifetime(_dispatcher.Report)` |
| `BreadcrumbBridgeCoordinator.cs:95` | `BeginPopulation(cancellationToken)` — the **only** call site passing a token |
| `BreadcrumbBridgeCoordinator.cs:104`, `:134` | `BeginPopulation()` |
| `BreadcrumbBridgeCoordinator.cs:105`, `:135` | `RunSynchronous(lease, …)` |
| `BreadcrumbBridgeCoordinator.cs:124`, `:141` | `RunAsync<T>(…)` / `RunAsync(…)` |
| `BreadcrumbBridgeCoordinator.cs:152` | `Invalidate()` |
| `BreadcrumbBridgeCoordinator.cs:165` | `TryDispose()` |
| `BreadcrumbBridgeCoordinator.cs:262` | `IsCurrent(lease)` |
| `BreadcrumbBridgeCoordinator.cs:267` | `Guard(lease, action)` — the **only** `Guard` call site |

Notably **`BreadcrumbItemViewerLifecycleCoordinator.cs` does not use this type at all**, so the
largest branch gap in F12 (that file, at 66.4%) is entirely independent of this one.
`Abandon` (`:89`) and `TryRunCurrent` (`:133`) have **no external caller anywhere in production** —
they are reached only from inside this file (`:115` and `:130` for `TryRunCurrent`, `:119` for
`Abandon`). Both remain directly callable from `QuickFiler.Test` through the IVT grant.

### 1.4 Concurrency and determinism inventory

Verified by direct read of all 309 lines plus a targeted grep.

**Present:**

| Construct | Lines |
| --- | --- |
| Single `private readonly object _sync = new object();` | `:38` |
| `lock (_sync)` acquisitions (10) | `:57`, `:75`, `:91`, `:105`, `:139`, `:206`, `:231`, `:243`, `:264`, `:282` |
| Disposal flag `private bool _disposed` (guarded by `_sync`) | `:41`, read at `:59`, `:77`, `:208`, `:238`, written at `:212` |
| Generation counter `private long _generation` (guarded by `_sync`) | `:40`, incremented at `:65`, `:81`, `:95`, `:213` |
| Re-entrancy / double-entry latches on the lease | `CancellationStarted` `:266-270`; `Cancelled`/`SourceDisposed` rendezvous `:246-250` and `:285-289` |
| `CancellationToken` / `CancellationTokenSource` | `:11`, `:20`, `:26`, `:28`, `:49`, `:52-54`, `:107`, `:141`, `:165`, `:167`, `:187`, `:189`, `:193` |
| Linked token source | `:53` |
| `async Task` methods (2) | `:150`, `:179` |
| `await` with `.ConfigureAwait(false)` (3 of 3) | `:166`, `:188`, `:192` |
| Exception filters (`catch … when`) | `:169`, `:196` |
| `IDisposable` / idempotent `TryDispose` | `:203-219`, `:221-227` |

**Absent (grep for `DateTime|Stopwatch|Timer|Task.Delay|Thread.Sleep|TimeProvider|Interlocked|volatile|SynchronizationContext` returns ZERO matches):**
no clock, no timer, no `Interlocked`, no `volatile`, no `SynchronizationContext`, no fire-and-forget
discard, no thread creation.

#### Determinism finding — the brief's "injected clock and fake timers" instruction is REFUTED for this file

`docs/features/active/2026-08-08-quickfiler-breadcrumb-bridge-coverage-495/spec.md:69-70` and `:112`
require "an injected clock and fake timers". There is **no time dependency of any kind in this
file** to control. Determinism here is not even scheduler control — it is **direct synchronous API
driving**: `BeginPopulation`, `Invalidate`, `Abandon`, `IsCurrent`, `Guard`, `TryRunCurrent`,
`RunSynchronous`, `TryDispose` and `Dispose` are all fully synchronous, and the two `async` methods
are driven by caller-supplied, already-completed `Task` values in the existing suite.

This independently reproduces sibling F13's ratified ruling at
`docs/features/active/2026-08-07-quickfiler-breadcrumb-dropdown-webview-coverage-455/spec.md:381-390`
("Determinism here is **scheduler** control, not clock control. Any plan task that introduces an
injected clock or a fake-timer facility is out of scope and must be rejected") and the companion F12
artifact's identical refutation for `BreadcrumbBridgeCoordinator.cs`. **Three files in this cluster
have now refuted the same instruction from independent evidence.** The spec phrasing must be struck
and recorded as a documented deviation.

#### Lock-ordering contract, stated precisely

1. **One lock, no nesting of distinct locks inside this file.** `_sync` is the only lock. No lock in
   this file is ever acquired while another lock in this file is held.
2. **No `Monitor` is held across an `await`.** Every `await` (`:166`, `:188`, `:192`) is in
   `RunAsync`/`RunAsync<T>`, neither of which takes `_sync`. `IsCurrent` (`:105-108`) and
   `IsGenerationCurrent` (`:231-234`) take the lock and return without awaiting. **Verified — there
   is no orphaned-lock or thread-affinity hazard.**
3. **The file's own stated convention is: call out to non-owned code OUTSIDE the lock.** It is
   followed in five places:
   - `CancelLease(superseded)` is invoked *after* the lock block in `BeginPopulation` (`:68`),
     `Invalidate` (`:85`), `Abandon` (`:99`) and `TryDispose` (`:217`).
   - `lease.Cancel()` (`:274`) — which synchronously runs arbitrary user cancellation callbacks —
     runs outside the lock, after the `CancellationStarted` latch is set under it (`:264-271`).
   - `DisposeLease(lease)` runs outside the lock at `:254` and `:293`.
   - `_report(exception)` runs outside the lock at `:278` and `:305`.
4. **`TryRunCurrent` is the single, deliberate exception**: `action()` executes at `:145` **inside**
   `lock (_sync)` (`:139`). This is the entire basis of LD-A below, and its significance is that the
   file otherwise demonstrates the author knew to avoid exactly this.
5. **Cross-object ordering**: the only outward lock edge is `lifetime._sync` -> `hub._sync`, via
   `:145` -> `BreadcrumbBridgeCoordinator.cs:271` -> `BreadcrumbMessengerHub.cs:126`.
   **Checked for inversion and none exists**: `BreadcrumbMessengerHub.OnSurfaceMessageReceived`
   (`BreadcrumbMessengerHub.cs:157-173`) snapshots the handler under `hub._sync` at `:170` and
   invokes it **outside** the lock at `:172`, so no `hub._sync` -> `lifetime._sync` edge is created
   on the inbound path. **No deadlock is demonstrable on the current code.**
6. **Re-entrant self-acquisition of `_sync` is routine, not hypothetical** — see LD-A.

---

## 2. Measured Baseline — independently recomputed

Source: `docs/features/active/2026-08-06-quickfiler-high-confidence-queue-init-stall-424/evidence/qa-gates/coverage-final.cobertura.xml`.

Exactly **one** `<class>` element carries `filename="QuickFiler\Viewers\BreadcrumbCoordinatorUpgradeLifetime.cs"`
(XML line **7510**, closing at **7849**); its class-level `<lines>` block runs XML lines
**7541-7848**. No cross-class union is required — but see the trap in §2.2, which is the opposite
trap to the one the epic warned about.

Recomputed from the class-level `<line>` nodes only — never `class.iter('line')`, never
`.//lines/line`, never the emitted `line-rate` / `branch-rate` attributes (#441):

| Metric | Value |
| --- | --- |
| Coverable lines (class-level `<line>` count) | **204** |
| Lines with `hits="0"` | **2** (`:267`, `:268`) |
| Line coverage | 202 / 204 = **99.02%** |
| Branching lines | 25 |
| Branch outcomes valid (sum of `condition-coverage` denominators) | **54** |
| Branch outcomes taken | **50** |
| Branch coverage | 50 / 54 = **92.59%** |

**The brief's row (204 lines / 99.0% / 92.6%) is CONFIRMED exactly.** This is the first F12 file
whose brief row survives recomputation unchanged. Both floors (>= 80% line, >= 75% branch, epic
"Coverage-Target Reconciliation") are cleared today, so the bar is **retain-or-improve on both
axes**, plus closure of the four reachable outcomes identified in §3.

The emitted attributes read `line-rate="0.990566"` and `branch-rate="0.910714"`. The line rate
happens to agree to three decimals (`202/204 = 0.990196` vs `0.990566` — a 105/106 ratio from the
double-counted method block); the **branch rate does not** (`0.910714` = 51/56 vs the true 50/54).
Compute; do not read.

### 2.1 The 99.0% is exactly two lines, and they are one gap

```
264:            lock (_sync)
265:            {
266:                if (lease.CancellationStarted)   <- condition-coverage="50% (1/2)"
267:                {                                 <- hits="0"
268:                    return;                       <- hits="0"
269:                }
270:                lease.CancellationStarted = true;
271:            }
```

Both zero-hit lines are the early-return body of the `CancellationStarted` latch in `CancelLease`
(`:258-295`). The untaken **true** side of `:266` and the two zero-hit lines are therefore a
**single** gap (H4 below), not three independent ones. Closing H4 takes line coverage to 100.00%.

### 2.2 A measurement trap specific to this file — the `<class name>` is not this file's principal type

The single `<class>` element for this file is named **`QuickFiler.Viewers.BreadcrumbUpgradeLease`**,
not `QuickFiler.Viewers.BreadcrumbCoordinatorUpgradeLifetime`. Two consequences, both verified:

1. **A harness keyed on `<class name>` would report `BreadcrumbCoordinatorUpgradeLifetime` as absent
   from the report** and, under the epic's own "absent is not covered" warning, could misclassify a
   99%-covered file as unmeasured. This is a concrete positive control for the epic's binding
   directive to key on `filename`, not on class name (epic § "Directives for F1's Ledger and
   Harness", requirement 1).
2. **This file is a fresh instance of #478.** The class-level `<lines>` block spans the whole file
   (`:13` through `:307`, both types plus the lifted closure for the `:130` lambda), i.e. the
   per-filename union is correct — but the `<methods>` subtree under that same element contains
   **only the four `BreadcrumbUpgradeLease` methods** (`.ctor`, `get_Token`, `Cancel`,
   `DisposeSource`, XML lines 7512-7539). There is no `<method>` entry for `BeginPopulation`,
   `Guard`, `TryRunCurrent`, `RunAsync` or any other lifetime member. A harness reading the method
   subtree would see a 5-line file. This is exactly the "correct class-level union blended with a
   primary-only method subtree" defect F11 filed as #478, reproduced on a second file.

No new issue is warranted — #441 and #478 already cover both mechanisms. Recording it here gives F16
a second verification specimen.

### 2.3 Complete branch-point census — all 25 branching lines

**Fully covered (21 lines, 46 of 46 outcomes):**

| Line | Construct |
| --- | --- |
| `:45` (2/2) | `_report = report ?? throw new ArgumentNullException(nameof(report));` |
| `:59` (2/2) | `if (_disposed)` in `BeginPopulation` |
| `:77` (2/2) | `if (_disposed)` in `Invalidate` |
| `:93` (2/2) | `if (ReferenceEquals(_current, lease))` in `Abandon` |
| `:107` (2/2) | `IsGenerationCurrentCore(lease) && !lease.Token.IsCancellationRequested` |
| `:126` (2/2) | `if (action == null)` in `Guard` |
| `:135` (2/2) | `if (action == null)` in `TryRunCurrent` |
| `:141` (**4/4**) | `if (!IsGenerationCurrentCore(lease) \|\| lease.Token.IsCancellationRequested)` |
| `:155` (2/2) | `if (lease == null)` in `RunAsync` |
| `:159` (2/2) | `if (operation == null)` in `RunAsync` |
| `:190` (2/2) | `if (IsCurrent(lease))` in `RunAsync<T>` |
| `:208` (2/2) | `if (_disposed)` in `TryDispose` |
| `:223` (2/2) | `if (TryDispose())` in `Dispose` |
| `:238` (**4/4**) | `!_disposed && ReferenceEquals(_current, lease) && lease.Generation == _generation` |
| `:246` (2/2) | `dispose = lease.Cancelled && !lease.SourceDisposed;` in `Complete` |
| `:247` (2/2) | `if (dispose)` in `Complete` (inside lock) |
| `:252` (2/2) | `if (dispose)` in `Complete` (outside lock) |
| `:260` (2/2) | `if (lease == null)` in `CancelLease` |
| `:285` (2/2) | `dispose = lease.Settled && !lease.SourceDisposed;` in `CancelLease` |
| `:286` (2/2) | `if (dispose)` in `CancelLease` (inside lock) |
| `:291` (2/2) | `if (dispose)` in `CancelLease` (outside lock) |

**Partial (4 lines, 4 untaken outcomes):**

| Line | Construct | Condition index | Untaken side |
| --- | --- | --- | --- |
| `:16` | `_source = source ?? throw new ArgumentNullException(nameof(source));` | 0 (50%) | the `throw` |
| `:52` | `cancellationToken.CanBeCanceled ? CreateLinkedTokenSource(…) : new CancellationTokenSource()` | 0 (50%) | the **`CanBeCanceled == true`** arm (`:53`) |
| `:130` | `return lease == null ? action : new Action(() => TryRunCurrent(lease, action));` | 0 (50%) | the **`lease == null`** arm (return `action` unwrapped) |
| `:266` | `if (lease.CancellationStarted)` | 0 (50%) | the **`true`** side (`:267-268`, both `hits="0"`) |

**How each untaken side was determined.**

- `:16`, `:130`, `:266` — determined from `hits` directly. For `:266` the two zero-hit lines `:267`
  and `:268` are the guarded body, so the `true` side is unambiguously the unobserved one. For
  `:130`, `BreadcrumbBridgeCoordinator.cs:262` reports `condition-coverage="75% (3/4)"` with its
  condition 0 (`lease != null`) at 50% and only its true side observed — so `Guard` is only ever
  entered with a non-null lease, and the `lease == null` arm is the untaken one.
- `:52` — **`hits` cannot disambiguate this one**, and stating otherwise would be wrong: the report
  gives `:53` and `:54` **both** `hits="1"`, which is a sequence-point artifact of a ternary
  initializer spanning three physical lines. The untaken side was instead established by an
  **exhaustive call-site census**, which is stronger evidence than the ambiguous hit map:
  - The only production call site passing a token is `BreadcrumbBridgeCoordinator.cs:95`
    (`SetSuggestionsAsync`), and a repository-wide grep shows `BreadcrumbBridgeCoordinator.SetSuggestionsAsync`
    has **no production caller at all** — only tests.
  - Every one of the six test call sites passes `CancellationToken.None`:
    `BreadcrumbBridgeCoordinatorTests.cs:137`, `BreadcrumbCoordinatorLifecycleTests.cs:289` and
    `:293`, `BreadcrumbSelectorToggleUiBoundaryTests.cs:54`, `BreadcrumbUiThreadDispatchTests.cs:42`,
    `BreadcrumbSubfolderActivationTests.cs:351`.
  - Every direct `BeginPopulation` call in the suite is parameterless:
    `BreadcrumbCoordinatorUpgradeLifetimeTests.cs:23`, `:43`, `:62`, `:63`, and
    `BreadcrumbCoordinatorLifecycleTests.cs:378`.
  - A grep of `QuickFiler.Test/Viewers/` for `new CancellationTokenSource` returns **zero** matches.

  `CancellationToken.None.CanBeCanceled` and `default(CancellationToken).CanBeCanceled` are both
  `false`, so the observed arm is `:54` and the untaken arm is `:53`.

**Line-number drift: none.** Every line cited by the brief, by the epic and by the companion
artifact re-anchors exactly on the current working-tree file. No re-anchoring is required.

**Exception filters are invisible to this metric.** `catch (OperationCanceledException) when (…)` at
`:169` and `:196` produce no Cobertura branch entry (both lines report `branch="False"`), so the
filter-false path carries no measurable outcome either way. Noted so a planner does not chase a
phantom gap there.

---

## 3. Gap Inventory — four gaps, four atomic test tasks

Gap identifiers use the **H** prefix the companion artifact assigned to this file, with **H3**
reserved for the `:130` `Guard(null, …)` gap it forward-referenced.

### H1 — `BreadcrumbUpgradeLease` null-source guard (`:16`) — 1 outcome

```
16:            _source = source ?? throw new ArgumentNullException(nameof(source));
```

**Why untaken today.** `BreadcrumbCoordinatorUpgradeLifetimeTests.ArgumentGuards_NullInputsThrowArgumentNullException`
(`:16-34`) is the test that almost reaches it: it exercises the **lifetime's** null-report guard
(`:45`, already 2/2) and four other null guards, but never the lease's own constructor.
`Disposal_RepeatedLifetimeDisposeIsSafeAndLeaseDisposeFailureIsReported` (`:73-91`) **does** construct
a lease directly — `new BreadcrumbUpgradeLease(1, new ThrowingCancellationTokenSource(sentinel))` at
`:78-81` — proving the direct-construction path is already open; it simply never passes null.

**Reachability: fully reachable, no production change.** The constructor is `internal` and the IVT
grant is in place; the precedent invocation is already committed.

**Arrange / Act.**
```
Action construct = () => new BreadcrumbUpgradeLease(1, null);
```

**Assert.** `construct.Should().Throw<ArgumentNullException>().WithParameterName("source");`
plus a positive control on a well-formed lease: `new BreadcrumbUpgradeLease(7, cts).Generation == 7`
and its `Token` equals `cts.Token`.

**Contract pinned.** The lease's construction-time invariant: a lease *always* owns a real
`CancellationTokenSource`, which is what makes `Token` (`:20`), `Cancel` (`:26`) and `DisposeSource`
(`:28`) unconditionally safe — none of the three has a null check, and the constructor guard is the
only thing standing between them and a `NullReferenceException`. Asserting the parameter name
distinguishes a real guard from an incidental throw.

### H2 — linked cancellation source when the caller supplies a cancellable token (`:52`) — 1 outcome

```
52:            CancellationTokenSource source = cancellationToken.CanBeCanceled
53:                ? CancellationTokenSource.CreateLinkedTokenSource(cancellationToken)
54:                : new CancellationTokenSource();
```

**Why untaken today.** See the census in §2.3: six test call sites reach `BeginPopulation(token)`
through `SetSuggestionsAsync`, and every one passes `CancellationToken.None`. The closest test is
`BreadcrumbCoordinatorLifecycleTests.AsyncPopulation_SupersededCompletionDoesNotPublishAgain`
(`:273-305`), which does capture and assert on a `CancellationToken` (`:278`, `:295`) — but that is
the *lease's* token observed inside the provider, not a caller-owned token flowing in. It stops
short at `:289`/`:293` with `CancellationToken.None`.

**Reachability: fully reachable, no production change.** `BeginPopulation` is `internal`, already
invoked five times from tests.

**Arrange / Act.** Two lifetimes so the linked and unlinked arms are compared without one
superseding the other:
```
var reported = new List<Exception>();
var linkedLifetime   = new BreadcrumbCoordinatorUpgradeLifetime(reported.Add);
var unlinkedLifetime = new BreadcrumbCoordinatorUpgradeLifetime(reported.Add);
using var cts = new CancellationTokenSource();
BreadcrumbUpgradeLease linked   = linkedLifetime.BeginPopulation(cts.Token);
BreadcrumbUpgradeLease unlinked = unlinkedLifetime.BeginPopulation();
bool ran = false;

cts.Cancel();
```

**Assert.**
- `linked.Token.IsCancellationRequested.Should().BeTrue();`
- `unlinked.Token.IsCancellationRequested.Should().BeFalse();`
- `linkedLifetime.IsCurrent(linked).Should().BeFalse();` — because `:107` ANDs
  `!lease.Token.IsCancellationRequested`.
- `linkedLifetime.TryRunCurrent(linked, () => ran = true).Should().BeFalse();` and
  `ran.Should().BeFalse();`
- `linked.Cancelled.Should().BeFalse();` — the lifetime never observed the cancellation, so its own
  cancel/settle bookkeeping is untouched.
- `reported.Should().BeEmpty();`

**Contract pinned.** Caller-token linkage: an *externally* cancelled token makes the lease
non-current **without** superseding it and **without** running any lifetime-owned cancellation
bookkeeping. That is the only observable difference between the two ternary arms, and it is the
behavior `SetSuggestionsAsync`'s `CancellationToken` parameter exists to provide — currently
unverified anywhere in the suite.

### H3 — `Guard(null, action)` returns the action unwrapped (`:130`) — 1 outcome

**This resolves the companion artifact's forward reference.**

```
130:            return lease == null ? action : new Action(() => TryRunCurrent(lease, action));
```

**Verification of the companion artifact's claim — CONFIRMED.** Its G3 test invokes
`BreadcrumbBridgeCoordinator.PostRenderAndSelectorAsync` reflectively with
`new object[] { "render", null, null }`. Traced end to end on the current code:

1. `BreadcrumbBridgeCoordinator.cs:262` — `if (lease != null && !_upgradeLifetime.IsCurrent(lease))`
   short-circuits false on condition 0, so `IsCurrent` is never called and the early return at `:264`
   is skipped. (This is the outcome its G3 exists to close.)
2. `BreadcrumbBridgeCoordinator.cs:266-267` — `_dispatcher.Dispatch(_upgradeLifetime.Guard(lease, …))`
   with `lease == null`.
3. `BreadcrumbCoordinatorUpgradeLifetime.cs:126` — `action` is non-null, so no throw.
4. `BreadcrumbCoordinatorUpgradeLifetime.cs:130` — `lease == null` is **true**, so the untaken arm is
   taken and `action` is returned unwrapped.

**The claim is correct: G3 does close `:130`.** Its recommendation that this file nonetheless take a
**direct** `Guard(null, …)` test is endorsed, for two reasons that are stronger than isolation
hygiene alone:

- G3's closure of `:130` is a **side effect of a reflective call on a different type**. If F12's plan
  reorders, re-scopes or drops T3, this file silently loses a branch outcome with no signal in its own
  test file.
- A direct test can assert the *behavioral* asymmetry, which the reflective test cannot observe:
  an unleased guard runs **unconditionally, even after the lifetime is disposed**, whereas a leased
  guard does not.

**Reachability: fully reachable, no production change.** `Guard` is `internal` and already called
directly from `BreadcrumbCoordinatorUpgradeLifetimeTests.cs:26`.

**Arrange / Act / Assert (direct test shape).**
```
// Arrange
var reported = new List<Exception>();
var unleased = new BreadcrumbCoordinatorUpgradeLifetime(reported.Add);
var leasedLifetime = new BreadcrumbCoordinatorUpgradeLifetime(reported.Add);
int runs = 0;
Action action = () => runs++;

// Act — unleased arm
Action passthrough = unleased.Guard(null, action);
BreadcrumbUpgradeLease lease = leasedLifetime.BeginPopulation();
Action guarded = leasedLifetime.Guard(lease, action);
unleased.TryDispose();
leasedLifetime.TryDispose();
passthrough();
guarded();

// Assert
passthrough.Should().BeSameAs(action);      // no wrapper allocated
guarded.Should().NotBeSameAs(action);       // wrapper allocated
runs.Should().Be(1);                        // only the unleased one ran
reported.Should().BeEmpty();
```

**Contract pinned.** `Guard` is a *conditional* wrapper only when a lease is supplied. With no lease
there is no generation to check, so the action is a pass-through that survives disposal of the
lifetime; with a lease, disposal suppresses it. `BeSameAs` is not a shape assertion here — reference
identity *is* the mechanism by which the unconditional semantics are delivered, and the paired
`runs.Should().Be(1)` makes the behavioral consequence explicit.

### H4 — idempotent lease abandonment: the `CancellationStarted` latch (`:266`, `:267`, `:268`) — 1 branch outcome + 2 lines

```
264:            lock (_sync)
265:            {
266:                if (lease.CancellationStarted)
267:                {
268:                    return;
269:                }
270:                lease.CancellationStarted = true;
271:            }
```

**Why untaken today.** `CancelLease` has four call sites — `BeginPopulation:68`, `Invalidate:85`,
`Abandon:99`, `TryDispose:217` — and each nulls `_current` under the lock first, so a given lease is
handed to `CancelLease` at most once through those paths. Two existing tests come close and both stop
short for identifiable reasons:

- `RunAsync_SupersededCancellationIsSwallowedAndSettled` (`BreadcrumbCoordinatorUpgradeLifetimeTests.cs:58-70`)
  calls `BeginPopulation()` twice, so the first lease is superseded and cancelled — but **exactly
  once**.
- `Disposal_RepeatedLifetimeDisposeIsSafeAndLeaseDisposeFailureIsReported` (`:72-91`) calls
  `lifetime.Dispose()` twice, which looks like a double-cancel but is not: the second `TryDispose()`
  returns at `:209-210` on the `_disposed` guard **before** reaching `CancelLease` at `:217`. That
  guard is what makes this the "almost" case.

**Reachability: fully reachable through the internal API, no production change and no reflection.**
The one route that re-enters `CancelLease` with the same lease is calling `Abandon` twice:

1. `Abandon#1` (`:89-101`) — `ReferenceEquals(_current, lease)` true -> `_generation++`,
   `_current = null` (`:95-96`); `CancelLease` latches `CancellationStarted`, calls `lease.Cancel()`,
   sets `Cancelled = true`, and finds `Settled == false` so does not dispose; `Complete` (`:100`)
   then sets `Settled = true`, finds `Cancelled && !SourceDisposed` true, sets `SourceDisposed` and
   **disposes the source** at `:254`.
2. `Abandon#2` — `_current` is now null so `:93` is false; `CancelLease` enters with
   `CancellationStarted == true` and **returns at `:267-268`**; `Complete` finds
   `Cancelled && !SourceDisposed` false, so nothing is disposed twice.

**Why this ordering is the discriminating one.** An `Invalidate()`-then-`Abandon(lease)` sequence
also reaches `:267-268`, but it is a weaker test: at that point the source has not yet been disposed,
so removing the latch would change nothing observable (`CancellationTokenSource.Cancel()` on an
already-cancelled, undisposed source is a silent no-op). In the **double-`Abandon`** ordering the
source *is* already disposed when `CancelLease` re-enters, so without the latch `lease.Cancel()`
(`:274`) would throw `ObjectDisposedException`, which `:276-279` would funnel into `_report`. The
assertion therefore fails if the latch is removed — which is what makes it a contract test rather
than a coverage artefact.

**Arrange / Act / Assert.**
```
// Arrange
var reported = new List<Exception>();
var lifetime = new BreadcrumbCoordinatorUpgradeLifetime(reported.Add);
var counting = new CountingCancellationTokenSource();          // override Dispose(bool), call base
var lease = new BreadcrumbUpgradeLease(1, counting);
SetCurrentLease(lifetime, lease);                              // existing helper, :93-105

// Act
lifetime.Abandon(lease);
lifetime.Abandon(lease);

// Assert
lease.CancellationStarted.Should().BeTrue();
lease.Cancelled.Should().BeTrue();
lease.Settled.Should().BeTrue();
lease.SourceDisposed.Should().BeTrue();
counting.DisposeCount.Should().Be(1);
reported.Should().BeEmpty("the CancellationStarted latch must stop a second Cancel() on a source that is already disposed");
```

`CountingCancellationTokenSource` is a three-line sibling of the already-committed
`ThrowingCancellationTokenSource` (`BreadcrumbCoordinatorUpgradeLifetimeTests.cs:107-120`), which
proves `Dispose(bool)` is override-able on this target framework. `Cancel()` is **not** virtual on
`CancellationTokenSource`, which is why the dispose counter plus the empty error sink — not a cancel
counter — is the right instrumentation.

**Contract pinned.** Abandonment is idempotent: a second abandonment neither re-cancels a disposed
source, nor double-disposes it, nor surfaces an error to the coordinator's reporting sink. This is
the only latch in the file with no other defence, and its removal is currently invisible to the
suite.

### 3.1 Projected result

| Axis | Before | After (projected) | Floor |
| --- | --- | --- | --- |
| Line | 202/204 = 99.02% | **204/204 = 100.00%** | >= 80% |
| Branch | 50/54 = 92.59% | **54/54 = 100.00%** | >= 75% |

All four untaken outcomes are reachable from `QuickFiler.Test` with no production change, no
reflection beyond the helper already committed at `BreadcrumbCoordinatorUpgradeLifetimeTests.cs:93-105`,
and no scheduler, clock or thread control. **Zero documented deviations are required for
reachability.** No line and no branch outcome in this file is unreachable.

---

## 4. Production Edit Verdict

**No production edit to `BreadcrumbCoordinatorUpgradeLifetime.cs` is required or recommended.**

All four gaps close through the existing `internal` surface plus the `AssemblyInfo.cs:5` IVT grant.
Consequences:

- **The 191-line headroom (309/500) is not consumed.** No new seam, no new adapter type, no new
  member, no partial split.
- **The #457 measurement trap does not apply.** No `[ExcludeFromCodeCoverage]` is introduced at
  either level, so no lifted-lambda leak needs reasoning about. Recorded for completeness: had a
  thin-forwarder adapter been required, it would have to be a **type-level**-exempt adapter that is
  `sealed` and **not `partial`** (epic § "Measurement Trap", § "fourth exemption ground" condition 4).
  This file already satisfies the structural half of that rule — both its types are `sealed` and
  neither is `partial`, so no attribute could propagate across partials here.
- **No `QuickFiler/QuickFiler.csproj` edit** is required, because no production file is created.
  The "Mid-Wave File Creation" ledger-row obligation and the >= 90% new-file target therefore do not
  engage for this file.

### 4.1 Resolution of the companion artifact's cascade claim — REFINED, not simply confirmed

The companion artifact rejected an alternative that would have made
`BreadcrumbBridgeCoordinator.PostRenderAndSelectorAsync`'s `lease` parameter required, giving as one
of three reasons that it "cascades into `BreadcrumbCoordinatorUpgradeLifetime.Guard`'s own null arm,
making that dead too and forcing a second production edit in a second file."

Verified from this side, and the claim is **half right**:

- **Confirmed:** `BreadcrumbBridgeCoordinator.cs:267` is the *only* call site of `Guard` in the entire
  repository. Making its `lease` argument non-nullable would leave `Guard`'s `lease == null` arm with
  zero production callers — genuinely dead production code. Under the epic's own precedent for dead
  code (the § "Epic Ruling: delete the dead region in `QfcExplorerController.cs`" ruling, which
  states that deleting unreachable code is the cleanest available refactor and that exempting it is
  the pattern this epic rejects), that would create an obligation to delete the arm — a second
  production edit in a second file, exactly as claimed.
- **Refined:** the cascade is **not** a coverage obligation. `Guard` is `internal` and directly
  callable from `QuickFiler.Test` (`BreadcrumbCoordinatorUpgradeLifetimeTests.cs:26` already calls
  it), so H3's direct test closes `:130` regardless of what the coordinator passes. The alternative
  would not have stranded this file's branch coverage.

Net: the companion artifact's **rejection of the alternative stands**, primarily on the epic's
zero-production-change NFR and on the absence of any coverage benefit — the cascade is a real but
secondary consideration, and this artifact records it at its true weight so a later planner does not
over-rely on it.

---

## 5. Retain-or-Improve Risk Analysis

At 99.02% line coverage this file has almost nothing to give back: 202 of its 204 coverable lines are
load-bearing on some existing test. The risk profile is unusual for F12 because coverage arrives
through **two** channels of very different sizes.

### 5.1 The full existing test surface

**Direct references — 2 test files, 14 occurrences** (a grep of the whole of `QuickFiler.Test/` for
`BreadcrumbCoordinatorUpgradeLifetime|BreadcrumbUpgradeLease|_upgradeLifetime`; the third hit is the
csproj, not a test):

| File | Occurrences | Nature |
| --- | --- | --- |
| `QuickFiler.Test/Viewers/BreadcrumbCoordinatorUpgradeLifetimeTests.cs` | 11 | direct, 4 `[TestMethod]` |
| `QuickFiler.Test/Viewers/BreadcrumbCoordinatorLifecycleTests.cs` | 3 | **reflective**, `:370-379` |

> **Correction.** The companion artifact found 15 referencing test files where its brief claimed 4.
> The inverse holds here: for **this** type the direct surface is only **2** files. The number that
> matters for retain-or-improve is the **indirect** surface below, and a planner who greps only for
> this type's name will badly underestimate its exposure.

**Indirect references — 15 test files, 56 occurrences** of `BreadcrumbBridgeCoordinator`, every one
of which constructs a coordinator and therefore constructs and drives a
`BreadcrumbCoordinatorUpgradeLifetime` at `BreadcrumbBridgeCoordinator.cs:56`:

`BreadcrumbCoordinatorLifecycleTests.cs` (13), `BreadcrumbSelectorCoordinatorTests.cs` (8),
`BreadcrumbBridgeCoordinatorTests.cs` (7), `BreadcrumbUiThreadDispatchTests.cs` (6),
`BreadcrumbItemViewerLifecycleCoordinatorTests.cs` (4),
`BreadcrumbBridgeCoordinatorProbabilityTests.cs` (3),
`BreadcrumbDuplicateIdentityIntegrationTests.cs` (3), `BreadcrumbDropDownReadinessTests.cs` (2),
`BreadcrumbSelectorToggleUiBoundaryTests.cs` (2), `BreadcrumbDropDownHostTests.cs` (1),
`BreadcrumbDropDownLifecycleTests.cs` (1), `BreadcrumbMessengerHubTests.cs` (1),
`BreadcrumbPopupPlacementTests.cs` (1), `BreadcrumbSubfolderActivationTests.cs` (1),
`FolderBreadcrumbAssetContractTests.cs` (1).

Four of those fifteen (`BreadcrumbDropDownHostTests`, `BreadcrumbDropDownLifecycleTests`,
`BreadcrumbDropDownReadinessTests`, `BreadcrumbPopupPlacementTests`) primarily target **F13-owned**
production files, and `BreadcrumbItemViewerLifecycleCoordinatorTests` constructs an **F14-owned**
`ItemViewer`. This file's 99% is therefore partly a by-product of sibling-adjacent tests.

### 5.2 R1 (highest) — four exception-handling regions rest on two tests in one file

The following regions are reached by **no** coordinator-driven test, because coordinator actions do
not throw and its `CancellationTokenSource`s do not fail:

| Region | Lines | Sole driver |
| --- | --- | --- |
| `RunSynchronous` catch -> `Abandon` -> rethrow | `:117-120` (4) | `BreadcrumbCoordinatorUpgradeLifetimeTests.RunSynchronous_FailureAbandonsLinkedLeaseAndReportsCancellationFailure` (`:36-56`) |
| `Abandon` body | `:90-101` (12) | same |
| `CancelLease` cancel-failure catch -> `_report` | `:276-279` (4) | same, via `lease.Token.Register(() => throw sentinel)` at `:45` |
| `DisposeLease` catch -> `_report` | `:303-306` (4) | `Disposal_RepeatedLifetimeDisposeIsSafeAndLeaseDisposeFailureIsReported` (`:72-91`), via `ThrowingCancellationTokenSource` |

**If `BreadcrumbCoordinatorUpgradeLifetimeTests.cs` were replaced rather than extended, roughly 24
lines drop to zero hits and this file falls from 99.02% to about 87.3% line coverage** — still above
the 80% floor, but a clear regression against the retain-or-improve bar and against issue #136 AC8.
This is the decisive argument for **extending** that file rather than superseding it (§6).

### 5.3 R2 — two disposal-guard regions rest on a single test in an F12-adjacent file

`BreadcrumbCoordinatorLifecycleTests.DisposedCoordinator_RejectsPopulationAndClearRemainsSafe`
(`:247-271`) is the only test that drives a coordinator **after** disposal. It alone covers:

- `BeginPopulation`'s disposed guard — `:59` (both sides), `:60`, `:61`, `:62` (`source.Dispose()`
  then `throw new ObjectDisposedException`), reached via `populateAfterDisposal` at `:259-260`;
- `Invalidate`'s disposed guard — `:77` (both sides), `:78`, `:79`, reached via `clearAfterDisposal`
  at `:261`.

Losing it costs ~7 lines and **4 branch outcomes** (2 fully covered branch lines drop to 1/2 each),
taking branch coverage from a projected 100% to about 92.6%. Any F12 plan task that restructures
`BreadcrumbCoordinatorLifecycleTests.cs` must preserve this test.

### 5.4 R3 — reflective couplings into this file's **private state**, in both directions

- **Into this file's private fields.** `BreadcrumbCoordinatorUpgradeLifetimeTests.SetCurrentLease`
  (`:93-105`) reflectively writes `"_current"` and `"_generation"` with
  `BindingFlags.Instance | BindingFlags.NonPublic`. **Renaming either field, or replacing them with
  an `Interlocked`/`volatile` representation, breaks that test at runtime, not at compile time**, and
  with it the `DisposeLease` catch coverage in R1. This is a harder coupling than the companion
  artifact's R4, which reaches only a private *field of the coordinator*.
- **Through the coordinator into this type's API.** `BreadcrumbCoordinatorLifecycleTests.cs:370-379`
  reads the coordinator's private `"_upgradeLifetime"` field, casts it to
  `BreadcrumbCoordinatorUpgradeLifetime`, then calls `BeginPopulation()` (`:378`) and `Invalidate()`
  (`:379`). The **field name** is a runtime coupling; the two **method names** are compile-time
  couplings once the cast succeeds. Confirmed present on this branch.

Practical rule for the plan: **rename nothing private in this file**, and add no task that
reorganises `_current`/`_generation`.

### 5.5 R4 — the lease's mutable flags are a de-facto public test contract

`CancellationStarted`, `Cancelled`, `Settled` and `SourceDisposed` (`:21-24`) are `internal bool` with
public-within-assembly setters and are asserted directly at
`BreadcrumbCoordinatorUpgradeLifetimeTests.cs:51-52`, `:67-69` and `:89`. H4 adds four more
assertions on them. They are effectively frozen for the duration of this epic. Note they carry no
`volatile` and no `Interlocked`: their consistency depends entirely on every mutation occurring under
`lifetime._sync` (`:245-250`, `:266-270`, `:284-289`), which is true today.

### 5.6 R5 — the `_report` sink is F13-owned

`_report` (`:37`) is bound at `BreadcrumbBridgeCoordinator.cs:56` to
`BreadcrumbUiDispatcher.Report` (`BreadcrumbUiDispatcher.cs:238`), an **F13-owned** file. Every
assertion in R1 about the error sink depends on `Report` swallowing rather than rethrowing. F13's
spec commits to **no public or internal signature changes** to its 15 files
(`.../455/spec.md:49-50`); that commitment is this file's protection and should be cited in F12's
plan. Note also that `BreadcrumbUiDispatcher.Dispatch` catches every exception from the dispatched
action at `:86-89` and routes it to the same sink — so a `Guard`-wrapped action that throws is
observable only through `Report`, never as a propagating exception.

---

## 6. Test-File Plan

### 6.1 Existing test file — verified

| File | Lines | `[TestMethod]` | Headroom vs 500 |
| --- | --- | --- | --- |
| `QuickFiler.Test/Viewers/BreadcrumbCoordinatorUpgradeLifetimeTests.cs` | **122** | **4** | **378** |

**The companion artifact's figures are confirmed exactly.** The class is declared
`public class BreadcrumbCoordinatorUpgradeLifetimeTests` at `:14` — **not `sealed`, not `partial`**.
It is already registered at `QuickFiler.Test/QuickFiler.Test.csproj:63`.

### 6.2 Recommendation — **extend** the existing file; do not create a new one

Add four `[TestMethod]`s to `BreadcrumbCoordinatorUpgradeLifetimeTests.cs`:

| Task | Test method | Closes |
| --- | --- | --- |
| H1 | `LeaseConstructor_NullSource_ThrowsForTheSourceParameter` | `:16` |
| H2 | `BeginPopulation_CancellableCallerToken_LinksAndDeactivatesTheLeaseWithoutSuperseding` | `:52` |
| H3 | `Guard_WithoutLease_ReturnsTheActionUnwrappedAndRunsAfterDisposal` | `:130` |
| H4 | `Abandon_CalledTwice_IsIdempotentAndReportsNothing` | `:266`, `:267`, `:268` |

Plus one `private sealed class CountingCancellationTokenSource : CancellationTokenSource` helper
alongside the existing `ThrowingCancellationTokenSource` (`:107-120`).

Estimated addition: **90-110 lines**, giving a final file of roughly **212-232 lines** — comfortably
inside the 500-line limit with ~270 lines still spare.

Justification for extending rather than adding a standalone class:

1. **It removes this file from the shared-csproj conflict surface entirely.** The epic identifies
   `QuickFiler.Test/QuickFiler.Test.csproj` as the *larger* shared-file surface (§ "Cross-Child
   Constraints" 1b) because every child must edit it. Extending an already-registered file means
   **zero csproj edits for this production file**, which is a real fan-in benefit — no other F12 file
   can make that claim.
2. **It directly mitigates R1.** The 24 lines of exception-path coverage that only this file
   supplies stay in the same class as the new tests, so a future edit that touches one is forced to
   see the other.
3. **The helpers the new tests need are already there**: the `SetCurrentLease` reflection helper
   (`:93-105`) is required verbatim by H4, and the `ThrowingCancellationTokenSource` pattern
   (`:107-120`) is the template for H4's counting variant. Re-creating either in a second file would
   be duplication.
4. **All four tests target this type directly.** None needs a coordinator, a messenger, a dispatcher,
   a provider mock or a `SynchronizationContext`, so there is no fixture reason to separate them.
5. **378 lines of headroom** make a split unnecessary. Contrast the companion artifact's situation,
   where `BreadcrumbBridgeCoordinatorTests.cs` had only 12 lines spare and a new class was forced.

**No `<Compile Include=…>` edit is required for this file.** If a reviewer nonetheless prefers a
standalone class, the repo has `.Part2.cs` precedent at `QuickFiler.Test/QuickFiler.Test.csproj:82`
and `:85`, and the entry would be
`    <Compile Include="Viewers\BreadcrumbCoordinatorUpgradeLifetimeTests.Part2.cs" />` inserted
immediately after `:63`, **preserving CRLF via the Edit tool — never `sed -i`** (epic
§ "Cross-Child Constraints" 1b). That path also requires adding `partial` to the class declaration at
`BreadcrumbCoordinatorUpgradeLifetimeTests.cs:14`, which is a second reason to prefer extension.

### 6.3 Projected post-change figures

| Axis | Before | After | Floor | Verdict |
| --- | --- | --- | --- | --- |
| Line | 99.02% (202/204) | **100.00%** (204/204) | >= 80% | improved |
| Branch | 92.59% (50/54) | **100.00%** (54/54) | >= 75% | improved |
| Test file size | 122 | ~212-232 | <= 500 | compliant |
| Production file size | 309 | 309 (unchanged) | <= 500 | compliant |

### 6.4 Determinism contract for every new test

- **Framework:** MSTest `[TestClass]`/`[TestMethod]`, **Moq** where a delegate spy is wanted (the
  existing file uses `Mock<Action<Exception>>` at `:40-41`), **FluentAssertions** for every
  assertion, explicit **Arrange / Act / Assert** section comments.
- **Deterministic vehicles already present and green in this exact file** — no new infrastructure is
  needed:
  1. **Direct synchronous API driving.** Every member H1-H4 touches is synchronous; there is no
     scheduler to control. This is the primary vehicle.
  2. `ThrowingCancellationTokenSource` (`:107-120`) — deterministic failure injection through the
     overridable `Dispose(bool)`. H4's `CountingCancellationTokenSource` is its direct analogue.
  3. `lease.Token.Register(() => throw sentinel)` (`:45`) — deterministic failure injection through
     `Cancel()`, which runs callbacks synchronously on the calling thread.
  4. `SetCurrentLease` reflection helper (`:93-105`) — places lifetime state without racing.
  5. `List<Exception>` report sink (`:39-41`, `:77`) — a synchronous, order-preserving capture of
     everything routed to `_report`.
  6. `Task.FromCanceled(token)` (`:65`) — a pre-completed cancelled task; no scheduler, no timing.
- **Explicitly not needed and not to be introduced:** `SynchronizationContext`,
  `BreadcrumbUiDispatcher`, any pump or drain harness, `[STATestClass]`/`[STATestMethod]`, any live
  or shown form, any WinForms control.
- **Prohibited and must be absent:** `Thread.Sleep`, `Task.Delay`, any wall-clock wait, any real-time
  polling, `DateTime.Now`/`UtcNow`, `Stopwatch`, `Timer`, injected clocks, `TimeProvider`,
  `FakeTimeProvider`, temporary files, any filesystem write, external services or processes, network
  access, live or shown forms, popups.
- **Concurrency testing posture — explicit.** Testing a lock-based currency primitive deterministically
  means **driving state transitions in a controlled order on a single thread**, never racing threads.
  No new test may spawn a thread, use `Task.Run`, or attempt to reproduce LD-A's re-entrancy window.
  H1-H4 are all single-threaded by construction. The one `Task.Run` in the neighbouring suite
  (`BreadcrumbCoordinatorLifecycleTests.cs:350`) is an existing pattern in an F12-adjacent file and is
  not a precedent this file should adopt.
- **Resource hygiene:** H2's `CancellationTokenSource` must be in a `using`. H1's and H3's lifetimes
  should be disposed (or `TryDispose`d, as H3 already does as part of its Act).

---

## 7. Latent Defects — verified, assessed, NOT fixed

All candidates were cross-checked against the currently-open issue list retrieved from GitHub
(`#495`, `#491`, `#488`, `#476`, `#475`, `#462`, `#458`, `#456`, `#455`, `#440`, `#438`, `#431`).
**None of the open issues covers the lifetime lock or the lease rendezvous.** The orchestrator, not
this agent, performs any promotion.

### LD-A — `action()` executes while `BreadcrumbCoordinatorUpgradeLifetime._sync` is held

**Resolution of the companion artifact's forward reference: I CONCUR, and REFINE in two ways that
strengthen it.**

**Severity: Low-Medium. Recommend promoting to a GitHub issue.** No duplicate exists.

Verified from this file's side, line by line:

1. `BreadcrumbCoordinatorUpgradeLifetime.cs:130` — `Guard` wraps the caller's action in
   `new Action(() => TryRunCurrent(lease, action))`. **Confirmed.**
2. `BreadcrumbCoordinatorUpgradeLifetime.cs:139-147` — `TryRunCurrent` takes `lock (_sync)` at `:139`,
   evaluates currency at `:141`, and calls `action()` at **`:145`, inside the lock**, returning `true`
   at `:146` still inside it. **Confirmed.**
3. `BreadcrumbBridgeCoordinator.cs:266-275` — the guarded action calls `_messenger.PostJson(renderJson)`
   at `:271`. **Confirmed.**
4. `BreadcrumbMessengerHub.cs:119-136` — in production `_messenger` is the concrete hub, whose
   `PostJson` takes its **own** `lock (_sync)` at `:126` and, still holding it, calls `PostToSurface`
   at `:133`. **Confirmed.**

**Concurring on both stated consequences:**

- **Nested two-lock acquisition, `lifetime._sync` -> `hub._sync`.** Checked for inversion:
  `BreadcrumbMessengerHub.OnSurfaceMessageReceived` (`:157-173`) snapshots the handler under the lock
  at `:170` and invokes it **outside** the lock at `:172`, so the inbound path creates no reverse
  edge. **No deadlock is demonstrable on the current code.** Concur exactly.
- **The lock does not deliver the atomicity it appears to.** `Monitor` is re-entrant, so an STA COM
  call from `PostToSurface` that pumps messages and re-enters managed code on the same thread would
  let a re-entrant `BeginPopulation` / `Invalidate` / `TryDispose` acquire `lifetime._sync`
  successfully and mutate `_current` **between** the currency check at `:141` and the completion of
  `action()` at `:145` — the exact invariant `TryRunCurrent` exists to enforce. Concur.

**Refinement 1 — the exposure is wider than the `Guard` path.** `TryRunCurrent` has a second caller:
`RunSynchronous` (`:115`). Through it, the whole of `BreadcrumbBridgeCoordinator.SetSuggestions`'s
body (`:107-113`) runs under `lifetime._sync` — router fallback scoring (`:109`), selector-state read
(`:110`), the render/selector post (`:111`) **and the kick-off of the asynchronous upgrade** (`:112`).
`AddItems` (`:137-145`) is the same shape. The lock therefore spans considerably more third-party and
async-initiating work than the `Guard`-only framing suggests, including the synchronous prefix of
`RunAsync<T>` up to its first suspension point.

**Refinement 2 — re-entrant self-acquisition is already routine on the happy path, not merely a COM
hypothetical.** Traced on the current code, `SetSuggestions` produces **three nested acquisitions of
`_sync` on one thread**, with no COM involvement at all:

```
BreadcrumbBridgeCoordinator.cs:105  RunSynchronous
  -> BreadcrumbCoordinatorUpgradeLifetime.cs:139   lock(_sync)          [acquisition 1]
     -> :145 action()
        -> BreadcrumbBridgeCoordinator.cs:111  PostRenderAndSelectorAsync
           -> BreadcrumbBridgeCoordinator.cs:262  _upgradeLifetime.IsCurrent(lease)
              -> BreadcrumbCoordinatorUpgradeLifetime.cs:105  lock(_sync)  [acquisition 2, re-entrant]
           -> BreadcrumbBridgeCoordinator.cs:266  _dispatcher.Dispatch(Guard(lease, …))
              -> BreadcrumbUiDispatcher.cs:84   action() executed INLINE on the owner boundary
                 -> BreadcrumbCoordinatorUpgradeLifetime.cs:139  lock(_sync) [acquisition 3, re-entrant]
```

That the design already relies on re-entrancy for its ordinary path is what makes the hazard real
rather than theoretical: there is no "the lock is never re-entered" defence available.

**Refinement 3 — the file contradicts itself.** As documented in §1.4, this file is otherwise
scrupulous about calling out to non-owned code **outside** the lock: `lease.Cancel()` at `:274`,
`DisposeLease` at `:254` and `:293`, and `_report` at `:278` and `:305` are all deliberately placed
after the lock block, with a latch (`:266-270`) or a flag (`:246-250`, `:285-289`) used to make the
outside-the-lock call safe. `:145` is the sole departure from a convention the same file establishes
five times.

**Why out of scope.** The fix is to move `action()` outside the lock and re-check currency after it —
a change to concurrency semantics, squarely outside the epic's no-behavior-change NFR, and one that
would alter the observable ordering guarantees the coordinator's existing tests encode. Per the
epic's § "Latent Defect Promotion" it must become a GitHub issue rather than prose in a feature
folder. Also recorded as **LD-1** in the companion artifact; **one issue should cover both**, since
it is one defect observed from two files.

### LD-B — `RunSynchronous` discards `TryRunCurrent`'s currency signal (the mechanism behind the companion artifact's LD-3)

**Severity: Low. Recommend promoting, bundled with LD-A if the orchestrator prefers one issue.**

The companion artifact's LD-3 observes that `BreadcrumbBridgeCoordinator.cs:112` assigns
`SuggestionsUpgrade` **inside** the lambda passed to `RunSynchronous`, so a non-current lease leaves
the caller believing an upgrade is in flight when none was started. **Assessed from this file's side,
the observation is correct and the mechanism is here, not there:**

- `TryRunCurrent` **does** report the failure — it returns `false` at `:143` when
  `!IsGenerationCurrentCore(lease) || lease.Token.IsCancellationRequested`.
- `RunSynchronous` **throws that signal away**: `:115` reads `TryRunCurrent(lease, operation);` as a
  statement, discarding the `bool`. It then returns `void` (`:111`), so the caller has no way to
  distinguish "the operation ran" from "the operation was silently skipped".

So the silent no-op is not an oversight at the coordinator's call site; it is structurally guaranteed
by `RunSynchronous`'s signature. The window is narrow — nothing spans `BeginPopulation`
(`BreadcrumbBridgeCoordinator.cs:104`) and `RunSynchronous` (`:105`) atomically, so on a single UI
thread it requires re-entrancy or genuine concurrency, i.e. it is reachable only through the same
mechanism as LD-A. No test reproduces it.

The cheapest mitigation would be for `RunSynchronous` to return `bool` and for the coordinator to
observe it — an additive signature change on an `internal` member with one caller. It is still a
production change with observable consequences and is out of scope here. `AddItems`
(`BreadcrumbBridgeCoordinator.cs:131-147`) has the identical structure and exposes no handle at all,
its dispatch task being discarded at `:141`.

### LD-C — `RunAsync<T>` has no argument guards, unlike its two-argument sibling

**Severity: Low (API inconsistency). Recommend recording; promotion optional and low priority.**

`RunAsync(lease, operation)` (`:150-177`) validates both arguments — `:155-158` for `lease`,
`:159-162` for `operation` — and both guards are covered (2/2 at `:155` and `:159`).
`RunAsync<T>(lease, operation, publishCurrent)` (`:179-201`) validates **none** of its three
arguments; a null `lease` produces a `NullReferenceException` at `:187`, and a null `publishCurrent`
one at `:192`. `RunSynchronous` (`:111`) likewise does not null-check `lease` before handing it to
`TryRunCurrent`, which dereferences it at `:238`.

There is no coverage consequence — an absent guard contributes no branch — so this is recorded for
completeness rather than as a gap. Adding guards would be a production change that *adds* branch
points and would need its own tests; it is out of scope under the no-behavior-change NFR. It is worth
noting because the existing test
`ArgumentGuards_NullInputsThrowArgumentNullException` (`:16-34`) asserts the two-argument overload's
guards and could be misread as covering the generic one.

### LD-D — `_sync` is acquired inside an exception filter

**Severity: Low (informational). Does NOT warrant a GitHub issue.**

`catch (OperationCanceledException) when (!IsGenerationCurrent(lease))` at `:169` and `:196` calls
`IsGenerationCurrent` (`:229-235`), which takes `lock (_sync)` at `:231`. Exception filters run in
the first pass of two-pass exception handling, on the throwing thread, **before** the stack unwinds,
so a filter that blocks on a contended lock stalls the exception in its filter phase.

Assessment: the risk is genuinely small. `_sync` is never held across an `await` (§1.4 item 2) and is
held only for short, allocation-free critical sections; and when the throwing thread is the same one
that holds the lock, `Monitor` re-entrancy makes the acquisition free. No hang is demonstrable on the
current code. Recorded so that a future change which lengthens a `_sync` critical section — including
any fix for LD-A — evaluates this interaction rather than discovering it.

---

## 8. Corrections to the Brief

### Disproved

1. **"Use an injected clock and fake timers" is wrong for this file and must be struck.**
   `docs/features/active/2026-08-08-quickfiler-breadcrumb-bridge-coverage-495/spec.md:69-70` and its
   seeded condition at `:112` require it. A grep of this file for
   `DateTime|Stopwatch|Timer|Task.Delay|Thread.Sleep|TimeProvider` returns **zero matches**, as does a
   grep for `Interlocked|volatile|SynchronizationContext`. Determinism here is not even scheduler
   control — it is direct synchronous API driving. This independently reproduces F13's ratified ruling
   at `.../455/spec.md:381-390` and the companion artifact's identical refutation. **Three F12/F13
   files have now refuted the same instruction.** Record as a documented deviation.
2. **The brief's API enumeration is incomplete.** It named `Guard`, `TryRunCurrent`, `IsCurrent`,
   `BeginPopulation`, `RunSynchronous`, `Invalidate`, `TryDispose` and `BreadcrumbUpgradeLease`, and
   omitted **`Abandon` (`:89`)**, **`RunAsync` (`:150`)**, **`RunAsync<T>` (`:179`)** and
   **`Dispose()` (`:221`)**. `Abandon` is the sole route to gap H4 and `RunAsync<T>` carries LD-C;
   a planner working from the brief's list would not find either.
3. **"99.0% of 204 implies roughly two uncovered lines" understates the coupling.** It is exactly two
   (`:267`, `:268`) — but they are the body of the *same* construct whose branch at `:266` is the
   fourth partial branch. Line gap and branch gap are one gap (H4), not two work items, and the
   file's real deficit is **four** outcomes across four constructs, not "two lines plus four
   branches".
4. **The "union multiple `<class>` elements by filename" expectation does not apply here, and the
   opposite trap does.** This file emits exactly **one** `<class>` element and its `name` attribute is
   `QuickFiler.Viewers.BreadcrumbUpgradeLease` — **not** the file's principal type. A harness keyed on
   class name would report `BreadcrumbCoordinatorUpgradeLifetime` as absent. The file is also a second
   specimen of **#478**: the class-level `<lines>` block correctly spans `:13`-`:307` while the
   `<methods>` subtree contains only the four `BreadcrumbUpgradeLease` methods. See §2.2.
5. **The `:52` untaken side cannot be read off `hits`.** The brief instructs determining the untaken
   side "from `hits` evidence — not by inference". For `:52` that is not possible: `:53` and `:54`
   both report `hits="1"` while the line is 1/2, a sequence-point artifact of a three-line ternary
   initializer. The untaken side (`:53`, the `CanBeCanceled == true` arm) was established instead by
   an exhaustive call-site census across production and all of `QuickFiler.Test/` — see §2.3.
6. **The companion artifact's cascade claim is half right.** Making
   `PostRenderAndSelectorAsync`'s `lease` required would make `Guard`'s null arm dead **production**
   code, creating a deletion obligation under the epic's own `QfcExplorerController` dead-code ruling
   — but it would **not** strand this file's coverage, because `Guard` is `internal` and directly
   testable. See §4.1. The rejection of that alternative still stands on the epic's zero-production-
   change NFR.
7. **The direct-reference test surface is 2 files, not comparable to the sibling's 15.** A planner
   who greps only for `BreadcrumbCoordinatorUpgradeLifetime` will see 2 files and 14 occurrences and
   badly underestimate exposure; the number that governs retain-or-improve is the **15-file, 56-
   occurrence indirect surface** that constructs a `BreadcrumbBridgeCoordinator`. See §5.1.

### Confirmed

8. **The coverage row is exact.** 204 coverable lines, 99.0% line (202/204 = 99.02%), 92.6% branch
   (50/54 = 92.59%). Recomputed from the class-level `<line>` nodes only. **This is the first F12
   file whose brief row survives recomputation unchanged.**
9. **`internal sealed` at `:35`** — confirmed, and additionally: not `partial`, `IDisposable`, and
   accompanied by a second `internal sealed` type `BreadcrumbUpgradeLease` at `:9` that the epic
   manifest does not mention.
10. **309 physical lines**, matching both the brief and the epic manifest's F12 assignment table;
    191 lines of headroom against the 500-line ceiling.
11. **No `[ExcludeFromCodeCoverage]` anywhere in the file**, consistent with `spec.md:32`'s statement
    that none of F12's five files carries one.
12. **`QuickFiler/Properties/AssemblyInfo.cs:5` grants `InternalsVisibleTo("QuickFiler.Test")`**, so
    every `internal` member is directly reachable from tests. Already exercised.
13. **`QuickFiler.Test/Viewers/BreadcrumbCoordinatorUpgradeLifetimeTests.cs` is 122 lines with 4
    `[TestMethod]`s and 378 lines of headroom**, exactly as the companion artifact reported. It is
    registered at `QuickFiler.Test/QuickFiler.Test.csproj:63`.
14. **`BreadcrumbCoordinatorLifecycleTests.cs:370-379` reaches this type through the coordinator's
    private `_upgradeLifetime` field**, exactly as documented — and this artifact adds a second,
    harder coupling at `BreadcrumbCoordinatorUpgradeLifetimeTests.cs:93-105`, which reflectively
    writes this file's own private `_current` and `_generation` fields.
15. **The companion artifact's G3 claim is correct**: its reflective `PostRenderAndSelectorAsync`
    invocation with a null lease does reach and close `BreadcrumbCoordinatorUpgradeLifetime.cs:130`.
    Traced through four steps in §3 (H3). Its recommendation of an additional direct test is endorsed
    and its shape is given.
16. **The companion artifact's LD-1 is correct in every verified particular**, including the absence
    of a lock-order inversion on the hub's inbound path. This artifact concurs and adds three
    refinements (§7, LD-A).
17. **Line-number drift: none.** Every anchor cited by the brief, the spec, the epic manifest and the
    companion artifact re-anchors exactly on the current working-tree file.
18. **`QuickFiler.Test/QuickFiler.Test.csproj` is a non-SDK project with explicit `<Compile Include>`
    entries and no globbing**, as the epic states — but no edit to it is required by this file,
    because the target test file is already registered.
