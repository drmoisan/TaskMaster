# Research — issue #816: `IsCompleted` branch-2 residual and issue #809 AC5 apartment measurement

- Timestamp: 2026-09-12T14-05
- Issue: #816 (OPEN, work mode `full-bug`)
- Feature folder: `docs/features/active/2026-09-08-uithread-iscompleted-branch2-residual-and-ac5-apartment-measurement-816/`
- Base: `origin/main` at `2405a829d`
- Scope: research only. No production source, configuration, or project file was modified.

## Tooling limitation recorded up front

The `Bash` tool is disabled in this session and `pwsh` is refused under Agent worktree
isolation, so **`git log` and `git blame` could not be executed**. Every finding below is
derived from reading the current tree plus the committed evidence artifacts of issue #809,
which record the pre-change state, the fail-before run, and the pass-after run verbatim. Where
a claim would have required git history, that is stated explicitly rather than inferred.

---

## R1 — What "hardened" means, and what the residual is

### What issue #809 changed in `IsCompleted`

The pre-change predicate is quoted by #809's own fail-before artifact:
`docs/features/active/2026-09-07-uithread-init-contract-residuals-784-787-788-809/evidence/regression-testing/p2-t10-fail-before.md:67`
records that before the fix `UiThread.cs:100` was a single reference comparison of `_context`
against the ambient context, and that the only awaiter test that went red was
`IsCompleted_OnOwningUiThreadWithADispatcherContextCapturedInsideAnInvoke_ReturnsTrue`
("red because `UiThread.cs:100` compares contexts by reference, so a dispatcher context
evaluated against a different ambient returns false").

The post-change predicate occupies `UtilitiesCS/Threading/UiThread.cs:155-190` and has five
exits, in evaluation order:

| # | Lines | Exit | Introduced by |
|---|---|---|---|
| B1 | 160-163 | `true` when `ReferenceEquals(_context, ambient)` | pre-existing (the whole predicate) |
| B2 | 167-170 | `false` when `ambient is null` | #809 |
| B3 | 171-174 | `false` when `_uiThreadId == -1` or `_uiThreadId != Thread.CurrentThread.ManagedThreadId` | #809 |
| B4 | 176-179 | `true` when `ReferenceEquals(_context, _uiSyncContext)` — **"branch 2" in the issue text** | #809 |
| B5 | 184-188 | `_context is DispatcherSynchronizationContext && ReferenceEquals(Dispatcher.FromThread(Thread.CurrentThread), _dispatcher)` — the dispatcher branch | #809 |

The branch #809 *hardened*, in the sense of "gave a second, independent proof obligation", is
B5. `UiThread.cs:180-183` states the rule in-code: "A dispatcher context is UI-owned only when
this thread's dispatcher is the UI dispatcher." The AC3 defect test that drove the change
(`UtilitiesCS.Test/Threading/UiThread_Tests.cs:170-202`) exercises B5, and its negative twin
`IsCompleted_WhenTheDispatcherContextBelongsToADifferentThreadsDispatcher_ReturnsFalse`
(`UiThread_Tests.cs:204-239`) proves the `Dispatcher.FromThread` clause is load-bearing.

### The precise asymmetry between B4 (176-179) and B5 (184-188)

B5 requires **two** facts beyond the B3 thread-id guard:

1. a type test — `_context is DispatcherSynchronizationContext`;
2. a **live re-verification of thread identity by a second, independent mechanism** —
   `ReferenceEquals(System.Windows.Threading.Dispatcher.FromThread(Thread.CurrentThread), _dispatcher)`.

Fact 2 is the hardening. `Dispatcher.FromThread` is a per-thread runtime lookup; it does not
consult `_uiThreadId`. If the managed-thread-id comparison at B3 is a *false positive* — a
different thread that happens to carry a recycled managed id equal to `_uiThreadId` — then
`Dispatcher.FromThread(Thread.CurrentThread)` on that thread returns `null` or a different
dispatcher, and B5 fails closed.

B4 has **no** second mechanism. Its only guards are B2 (ambient non-null) and B3
(`_uiThreadId == Thread.CurrentThread.ManagedThreadId`). Once `_context` happens to be the
persistent `_uiSyncContext`, B4 reduces exactly to a bare owning-thread-identity predicate for
that one context — the predicate shape the #816 constraint forbids. That is the asymmetry.

### The reachable state in which B4 returns `true` with a different, non-null ambient

B4 is reached when all of: `ambient != null`; `ambient != _context`; `_uiThreadId != -1` and
equal to the current managed thread id; and `ReferenceEquals(_context, _uiSyncContext)`.

**Leg 1 — genuinely on the UI thread, inside a WPF dispatcher operation (demonstrably
reachable in-tree).** `UiThread.Initialize()` at `UiThread.cs:79` assigns `UiSyncContext` from
`IUiCaptureSource.UiSyncContext`, and the production implementation
`UtilitiesCS/Threading/SyncContextForm.cs:36` sets that to `SynchronizationContext.Current`
read on the UI thread — i.e. the thread's persistent `WindowsFormsSynchronizationContext`.
Inside a `Dispatcher.Invoke`/`BeginInvoke` callback on that same thread, WPF installs a
`DispatcherSynchronizationContext` as `SynchronizationContext.Current` for the duration of the
operation. That this happens is established in-tree, not assumed:
`UiThread_Tests.cs:184-196` captures `SynchronizationContext.Current` *inside*
`host.Dispatcher.Invoke(...)` and the test passes only if that captured value satisfies
`_context is DispatcherSynchronizationContext` at `UiThread.cs:184`
(`.../evidence/regression-testing/p3-t6-pass-after.md:51`, outcome `Passed`).
So a caller executing `await <viewer>.UiSyncContext` while inside a dispatcher operation on the
UI thread sees: ambient = throwaway `DispatcherSynchronizationContext`, `_context` = persistent
`WindowsFormsSynchronizationContext` = `_uiSyncContext`, thread id matches → **B4 returns
`true`**.

That `<viewer>.UiSyncContext` is the same object as `UiThread._uiSyncContext` on a live host
follows from both being `SynchronizationContext.Current` captured on the UI thread
(`QuickFiler/Viewers/ItemViewer.cs:26` and `SyncContextForm.cs:36`), and WinForms installing at
most one `WindowsFormsSynchronizationContext` per thread. This is a code-reading inference; it
was **not** executed, and no in-tree test asserts the two captures are reference-equal.

**Leg 2 — a foreign thread with a recycled managed id (the named rationale, not demonstrated).**
The CLR reuses managed thread ids after a thread dies. A continuation resumed after
`ConfigureAwait(false)` onto a pool thread whose id equals `_uiThreadId`, and which carries some
non-null ambient context, satisfies B2, B3 and B4 and completes inline **off the UI thread
entirely**. I could not construct an in-tree demonstration of id recycling, so this leg is
recorded as the documented rationale (see R2) rather than as a measured reachable state.

### Consequence of completing inline in that state

`IsCompleted == true` makes the compiler-generated await call `GetResult()`
(`UiThread.cs:195`, a no-op) on the current stack and run the continuation **synchronously**.
`OnCompleted` (`UiThread.cs:192-193`, `_context.Post(_postCallback, continuation)`) is never
invoked. Three consequences follow:

1. **The ambient context after the `await` is the foreign one, not `_context`.** For leg 1 the
   continuation runs under the throwaway `DispatcherSynchronizationContext`. The two
   `TaskScheduler.FromCurrentSynchronizationContext()` call sites that sit immediately after
   such an await — `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs:64` then `:67`, and
   `QuickFiler/Controllers/EfcItemController.cs:191` then `:201` — therefore capture a scheduler
   bound to a dispatcher-operation context that is torn down when the `Invoke` returns, instead
   of the persistent WinForms context. #809's own closure summary records the *inverse*
   direction of this same hazard for B5 at
   `.../evidence/other/p6-t13-closure-summary.md:37-38`; for B4 the direction is reversed.
2. **Ordering inverts.** Work already queued through `_context.Post` runs *after* the inline
   continuation instead of before it. #809 enumerated eleven production await sites at which
   this matters and recorded that **no existing test asserts ordering at any of them**
   (`p6-t13-closure-summary.md:29-38`). That residual is inherited by this issue.
3. **On leg 2 the continuation runs UI work on a non-UI thread with no marshalling at all.**

Note one honesty caveat that the plan should carry: on leg 1 the caller *is* on the UI thread,
so inline completion is arguably correct on thread-affinity grounds. The defect is that B4
cannot distinguish leg 1 from leg 2, and that inline completion silently changes the ambient
context the continuation observes. The hardening targets leg 2 and the context substitution; it
should not be framed as "B4 must return false inside a dispatcher operation", because B5
already returns `true` for a dispatcher context in that same state and the two branches would
then disagree in the opposite direction.

### Coverage status of B4 today

`.../evidence/qa-gates/p6-t1-uithread-file-coverage.md:14` records
`FINAL_UITHREAD_UNCOVERED_LINES: 38,39,40,177,178`, and `:78-81` attributes 177-178 to "the body
of the `ReferenceEquals(_context, _uiSyncContext)` clause of the new predicate, at line 176".
Those line numbers match the current tree exactly. `UiThreadStateScope.SetUiSyncContext`
(`UtilitiesCS.Test/TestHelpers/UiThreadStateScope.cs:148`) exists but has **zero callers**
anywhere in the repository — verified by a repo-wide grep returning only the declaration. B4 is
therefore both untested and, per #809's closure, knowingly left so.

---

## R2 — The correct hardening, and what must not be relaxed

### The constraint text, verified against the current tree

`QuickFiler/Viewers/BreadcrumbUiDispatcher.cs:255-278` is `IsCurrentBoundary()`. The lines the
issue cites are correct and have not moved:

```
263            // When a context was captured it is the authoritative boundary, so only an ambient
264            // reference match to that exact context proves the caller is on it. Bare owner-thread
265            // identity must never substitute here: a continuation resumed after
266            // ConfigureAwait(false) can be scheduled onto a recycled thread-pool thread whose
267            // managed thread ID equals the captured owner thread ID, which would run UI work inline
268            // and complete the returned task without any post ever crossing the captured context.
269            if (_context != null)
270            {
271                return ReferenceEquals(SynchronizationContext.Current, _context);
272            }
```

**Correction to the issue's framing.** `BreadcrumbUiDispatcher` is **not a call site** of
`UiThread.SynchronizationContextAwaiter`. A grep of that file for `await` of a context returns
nothing; it uses `_context.Post` directly (`:122`, `:206`) and its own `IsCurrentBoundary()`
(`:255`). It is a **precedent that records the rule**, not code a change to `IsCompleted` could
break. No candidate hardening of `IsCompleted` can affect it.

`QuickFiler.Test/TestSupport/WinFormsPumpHostTests.cs` — the cited range has shifted by a small
amount. The test is `AwaitingSyncContext_FromTheTestThread_ResumesOnThePumpThread`, declared at
`:181-183` and running to `:205`; the awaiting statement is `await host.SyncContext;` at `:190`
and the assertion is `:194-199`. A second consumer of the same awaiter is
`BothMarshalRoutes_WpfDispatcherAndSyncContext_ExecuteOnThePumpThread` at `:216-218`, whose
`await host.SyncContext;` is at `:245`. `#809`'s spec names both
(`.../784-787-788-809/spec.md:442`: "the two tests a bare-id predicate would break").

This **is** a real call site and it confirms the claim. `host.SyncContext` is the pump thread's
`WindowsFormsSynchronizationContext` (`QuickFiler.Test/TestSupport/WinFormsPumpHost.cs:72`,
asserted `BeOfType<WindowsFormsSynchronizationContext>` at `WinFormsPumpHostTests.cs:42-45`),
and the pump thread is asserted distinct from the MSTest thread at `:46-50`. Under a bare-id
predicate, if `UiThread._uiThreadId` had been populated with the MSTest thread's id by any
earlier test in the same process, the `await` at `:190` would complete inline on the MSTest
thread and the assertion at `:194-199` would fail. The in-repo regression guard for exactly this
shape is `SynchronizationContextAwaiter_Tests.IsCompleted_WithAForeignWindowsFormsContextWhileUiThreadIdMatches_ReturnsFalse`
(`UiThread_Tests.cs:241-275`), whose comment at `:244-247` says so explicitly.

### Candidate hardenings

**H1 (recommended) — add the dispatcher-identity re-verification to B4.** Change B4 from
`ReferenceEquals(_context, _uiSyncContext)` to `ReferenceEquals(_context, _uiSyncContext) &&
ReferenceEquals(System.Windows.Threading.Dispatcher.FromThread(Thread.CurrentThread), _dispatcher)`.
- Effect on `WinFormsPumpHostTests`: none. `host.SyncContext` is not `_uiSyncContext`, so B4 is
  not reached for that context under either the current or the hardened predicate.
- Effect on `BreadcrumbUiDispatcher`: none (not a call site).
- Effect on the seven existing `SynchronizationContextAwaiter_Tests` predicate cases: none. No
  existing case installs `_uiSyncContext`, so none currently reaches B4
  (`p6-t1-uithread-file-coverage.md:81`).
- Effect on leg 2: closes it. A recycled pool thread has no WPF dispatcher, so
  `Dispatcher.FromThread` returns `null` and the clause fails closed.
- Effect on leg 1: B4 still returns `true` on the genuine UI thread, so behaviour at the eleven
  production await sites is unchanged for the case that actually occurs today.

**H2 — hoist the dispatcher-identity check above both B4 and B5**, so B3 becomes
"thread id matches **and** this thread's dispatcher is the UI dispatcher". Structurally cleaner
and makes the symmetry explicit, but it changes the shape of the guard that three existing
tests were written against (`UiThread_Tests.cs:143-168`, `:204-239`, `:241-275`), so the diff
touches more lines with no additional behavioural gain over H1. Recorded and not recommended.

**Rejected alternatives.** Replacing the reference comparison at B1 or B4 with any
owning-thread-identity test (`_uiThreadId == Thread.CurrentThread.ManagedThreadId` alone, or
`Environment.CurrentManagedThreadId` alone) is prohibited by the constraint, is contradicted by
`BreadcrumbUiDispatcher.cs:263-272`, and would break `WinFormsPumpHostTests.cs:190` and `:245`
as described above. Removing B4 entirely was also considered and rejected: it would make
`await UiThread.UiSyncContext` from inside a dispatcher operation on the UI thread `Post` to
itself, which changes ordering at all eleven production await sites in a direction no test
covers.

### The two WebView2 setup sites named by the comment at `UiThread.cs:164-166`

- `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs:64` (`await _itemViewer.UiSyncContext;`)
  followed by `:67` (`TaskScheduler ui = TaskScheduler.FromCurrentSynchronizationContext();`).
- `QuickFiler/Controllers/EfcItemController.cs:191` (`await _itemViewer.UiSyncContext;`)
  followed by `:201` (`TaskScheduler ui = TaskScheduler.FromCurrentSynchronizationContext();`).

Both are corroborated by `.../evidence/other/p6-t13-closure-summary.md:37-38`. A third
`await _itemViewer.UiSyncContext;` exists at `QfcItemController.ViewerSetup.cs:287` but is not
followed by a scheduler capture.

---

## R3 — Apartment-state measurement (issue #809 AC5)

### What #809 AC5 actually requires (read this before planning)

`.../784-787-788-809/spec.md:464` (unchecked, `- [ ]`):

> AC5: An evidence artifact under this feature folder's `evidence/other/` directory records a
> measurement of whether `new SyncContextForm(); Show();` throws when executed on an MTA thread
> on the execution host, and the AC2 regression test is justified against that measured result
> rather than against the #782 narrative. The full-suite run records
> `UtilitiesCS.Test.Extensions.DictionaryExtensions_Tests.TryAddValuesAsync_UpdatesExistingValue`
> across at least three repetitions, and no single failure of that test is attributed to this
> delivery.

The deliverable is therefore **not** merely "read the ambient worker's apartment". It is:
run `new SyncContextForm(); Show();` on a thread whose apartment is *measured* as MTA, and
record whether it throws. Measuring the apartment is the precondition the `[P0-T15]` probe
omitted, which is why its conclusion was withdrawn
(`.../evidence/other/p0-t15-mta-synccontextform-measurement.md:60-94`).

### (a) `.runsettings` files and `ExecutionThreadApartmentState`

Nine `.runsettings` files exist in the tree. A repo-wide content grep for
`ExecutionThreadApartmentState` returns **zero hits in any `.runsettings` file**; every hit is
in Markdown under `docs/` or in `.claude/agent-memory/`. The claim in `issue.md:34` is
**confirmed**.

The four live (non-archived) files are:

| File | Relevant content |
|---|---|
| `UtilitiesCS.Test/test.runsettings` | `:6` is `<RunSettings />` — empty. `:2-5` is a comment stating global STA execution is intentionally disabled and STA is opt-in per class or method. |
| `TaskMaster.runsettings` | `:3-8` `<MSTest><Parallelize><Workers>0</Workers><Scope>ClassLevel</Scope>` plus a Code Coverage data collector at `:9-29`. |
| `scripts/vscode/TaskMaster.cli.runsettings` | `:3-8` the same `<MSTest><Parallelize>` block; no data collector. |
| `TaskVisualization.Test/coverage.runsettings`, `TaskTree.Test/coverage.tasktree.runsettings` | `<RunConfiguration>` present at `:3-5` in the first; neither sets an apartment. |

**Correction to the issue's reasoning, and it is load-bearing.** `issue.md:47-48` argues "because
no `.runsettings` sets `ExecutionThreadApartmentState` and the only assembly-level `Parallelize`
attribute is the one at `AssemblyInfo.cs:18`, the probe most likely ran STA". The second premise
is incomplete: `TaskMaster.runsettings:3-8` and `scripts/vscode/TaskMaster.cli.runsettings:3-8`
each carry a **runsettings-level** `<MSTest><Parallelize>` directive that applies to *every*
assembly in the run whenever `/Settings:` is passed. The repository's standard coverage command
does pass it — see
`docs/features/active/2026-09-08-console-out-aggressors-and-banned-symbol-promotion-826/evidence/qa-gates/p7-t4-tests-coverage.md:8`
(`/Settings:scripts\vscode\TaskMaster.cli.runsettings`) — while the `/EnableCodeCoverage`
variant at `.../p7-t5-tests-ci-verbatim.md:11` does not. So whether a given test lands on a
pooled worker or on the vstest main execution thread, and hence its apartment, **depends on the
invocation**. This is precisely why AC5 demands a runtime measurement and why no static
inference can settle it.

### (b) Is `AssemblyInfo.cs:18` the only assembly-level `Parallelize` attribute?

**Yes, as an attribute.** A repo-wide grep for `assembly:\s*Parallelize` over all files returns
exactly one `.cs` hit: `UtilitiesCS.Test/Properties/AssemblyInfo.cs:18`, spanning `:18-21`:

```
[assembly: Parallelize(
    Workers = 0,
    Scope = Microsoft.VisualStudio.TestTools.UnitTesting.ExecutionScope.ClassLevel
)]
```

Every other hit is Markdown under `docs/` or a memory file under `.claude/agent-memory/`. No
other test project (`QuickFiler.Test`, `TaskMaster.Test`, `TaskTree.Test`,
`TaskVisualization.Test`, `ToDoModel.Test`, …) carries one. Qualify the statement as "only
assembly-level *attribute*", per (a).

### (c) Existing infrastructure that creates STA threads or measures apartment state

`Thread.CurrentThread.GetApartmentState()` appears in exactly **two** places in all `*.cs` in
the repository:

- `UtilitiesCS/Threading/UiThread.cs:30` — the production AC1 precondition.
- `UtilitiesCS.Test/Threading/UiThreadInitContract_Tests.cs:244` — inside
  `[STATestMethod] Init_OnStaThread_DoesNotThrowAndPopulatesAllFourCaptureFields`, asserting
  `.Should().Be(ApartmentState.STA)`. Because the method is `[STATestMethod]`, this measures a
  *forced* STA thread, not the ambient worker. **No test anywhere measures the apartment of an
  ambient, unforced test thread.** That is the AC5 gap in one sentence.

`SetApartmentState` appears at 40+ sites. Every one of them hardcodes `ApartmentState.STA`
except one:

- **`UtilitiesCS.Test/Threading/UiThreadInitContract_Tests.cs:85-104`,
  `internal static class ApartmentThreadRunner.RunOnThread(ApartmentState apartment, Action action)`** —
  `:100` calls `thread.SetApartmentState(apartment)` with the caller-supplied value. This is the
  **only** helper in the repository that can host work on **both** an STA and an MTA thread. It
  starts a background thread, joins it, and returns the thrown `Exception` or `null`.
  It is `internal` in namespace `UtilitiesCS.Test.Threading`, so it is reachable from any class
  in `UtilitiesCS.Test` without a new seam. Its signature returns only an `Exception`, so a
  measurement must be captured into a closure variable rather than returned.

Read in full as requested:

- `UtilitiesCS.Test/TestHelpers/UiThreadStateScope.cs` (209 lines) — snapshots and restores the
  eleven `UiThread` statics plus `SyncContextFormFactory` (`:58-72`, `:79-91`, `:173-188`), and
  exposes `SetUiSyncContext` (`:148`), `SetUiThreadId` (`:155`) and `SetDispatcher` (`:161`).
  It creates **no** thread and measures **no** apartment. `:15-26` states it is deliberately not
  thread-safe and relies on `[DoNotParallelize]` on every consumer.
- `UtilitiesCS.Test/TestHelpers/UiThreadDispatcherScope.cs` (126 lines) — installs/restores
  `UiThread._dispatcher` only (`:75-90`, `:102-112`). No thread, no apartment.

Sibling STA-only hosts that could be copied but cannot host MTA as written:
`SharedStaDispatcherHost` (`UiThreadInitContract_Tests.cs:135-163`, `:149`), the two private
`StaDispatcherHost` copies in `UiThread_Tests.cs` (`:319-346` at `:333`; `:429-456` at `:443`),
and `QuickFiler.Test/TestSupport/WinFormsPumpHost.cs:58`.

`[DoNotParallelize]` is present on every `UiThread` consumer class:
`UiThread_Tests.cs:11` and `:372`, `UiThreadInitContract_Tests.cs:175` and `:305`.
`UiThreadInitRetryContract_Tests` additionally carries `[STATestClass]` at `:304`.

**Answer to "which helper can host a measurement on both STA and MTA": `ApartmentThreadRunner.RunOnThread`,
and only that one.**

### (d) How the measurement should be recorded, and what settles AC5

**Do not commit raw tool output.** Per the maintainer decision on issue #671 (2026-09-11,
supplied by the orchestrator), no `.trx` and no `.cobertura.xml` may be written into the
repository; numeric figures are transcribed into Markdown and the raw output is discarded. Note
for the record: `.claude/skills/evidence-and-timestamp-conventions/SKILL.md` does not itself
require committing raw output — it requires `Timestamp:`, `Command:`, `EXIT_CODE:` and, for
baseline artifacts, `Output Summary:` inside the Markdown — so the two conventions are
compatible and no exception is needed.

**Location.** `docs/features/active/2026-09-08-uithread-iscompleted-branch2-residual-and-ac5-apartment-measurement-816/evidence/other/`
is canonical for a measurement that is neither a gate nor a regression run. A second mirror
under the #809 folder is **not** needed; #809 AC5 names `evidence/other/` under *its* folder, so
if the orchestrator intends to check off #809's AC5 in place, the artifact (or a copy) must also
land at
`docs/features/active/2026-09-07-uithread-init-contract-residuals-784-787-788-809/evidence/other/`.
Flag this as a decision for the planner; it is not resolvable from the tree.

**The single observed value that settles AC5.** Two values are needed and only one of them is
the discriminator:

1. A **guard** value: the apartment read by
   `Thread.CurrentThread.GetApartmentState()` *on the probe thread, inside the probe delegate*.
   The artifact must record it verbatim. If it is not `MTA`, the probe measured nothing and the
   run is void.
2. The **settling** value: whether `new SyncContextForm(); Show();` **threw** on that measured-MTA
   thread. Record it as the outcome token #809's plan already fixed —
   `MTA_INITIALIZE_OUTCOME: COMPLETED` or `MTA_INITIALIZE_OUTCOME: THREW` (with exception type
   and message on the `THREW` branch), matching
   `.../evidence/other/p0-t15-mta-synccontextform-measurement.md:18` and its schema note at `:20-26`.

`THREW` confirms the #782 mechanism narrative on this host; `COMPLETED` refutes it on this host.
Either value settles AC5, because AC5 is a measurement obligation, not a gate — `p0-t15:16`
records that reasoning and it still applies, so the artifact should carry
`ExpectedExitCode:` equal to the observed exit code.

**Route.** #809's correction asserted that settling this "requires reverting
`UtilitiesCS/Threading/UiThread.cs` to its pre-fix state" (`p0-t15:92-94`). **That is not
required, and the planner should not do it.** `QuickFiler.Viewers.SyncContextForm` is
`public partial class SyncContextForm : Form` (`UtilitiesCS/Threading/SyncContextForm.cs:16`)
compiled into the `UtilitiesCS` assembly (`UtilitiesCS/UtilitiesCS.csproj:1100-1103` region), so
`UtilitiesCS.Test` can construct it directly and call `Show()` without touching `UiThread` at
all. The route is: inside `ApartmentThreadRunner.RunOnThread(ApartmentState.MTA, ...)`, read and
capture `Thread.CurrentThread.GetApartmentState()`, then `new SyncContextForm()` and `Show()`,
and let `RunOnThread` return the thrown exception or `null`.

Two constraints on that route, both verified:

- `UtilitiesCS.Test/NoLiveFormInTestAssemblyTests.cs:16-38` forbids **compiling** a
  `Form`-derived type into `UtilitiesCS.Test`. It reflects over type metadata only (`:19`,
  "nothing is instantiated") and scopes to `Assembly.GetExecutingAssembly()` (`:21`). Constructing
  a `Form` declared in `UtilitiesCS` does not violate it.
- The probe must not leave a live window or an un-shut-down message loop. `Show()` on a
  non-pumping MTA thread is the exact production shape at `UiThread.cs:75`; the probe should
  `Hide()`/`Dispose()` the form in a `finally`, mirroring `UiThread.cs:99`.

If the planner prefers not to construct a real `Form` in a unit test at all, the alternative is
to record the measurement as a **one-off probe artifact** rather than a committed test, and to
state in the artifact that the probe test was added and removed within the task. That trades
repeatability for hygiene; H1-style permanence is preferable and is recommended.

---

## R4 — Issue #782 latch status: two separate findings

### Finding 4A — production code: the retry-after-failure behaviour IS present

`UtilitiesCS/Threading/UiThread.cs:19-60`. The relevant structure:

- `:30-34` — the apartment precondition runs **first**, before any global is mutated
  (rationale in the comment at `:26-29`).
- `:51-59` — `lock (InitLock)` guards `if (_initialized) return; Initialize(); _initialized = true;`.
- `:47-50` — the in-code statement of the #782 contract: "The flag is set after `Initialize()`
  returns, not before it runs, so a failed first attempt leaves it false and a later call from
  an STA thread retries. The lock additionally serializes concurrent first attempts, which the
  previous `Interlocked.Exchange` latch never did."

`_initialized` is declared at `:67` and reset by `ResetForTesting()` at `:124`. The pre-fix
shape — an `Interlocked.Exchange` latch consumed *before* `Initialize()` ran — is recorded by
`.../evidence/regression-testing/p2-t10-fail-before.md:65` ("red because the latch at
`UiThread.cs:36` is consumed before `Initialize()` runs, so the retry is a silent no-op").
**Status: present. Not a residual.**

### Finding 4B — test coverage: a retry test exists, and its outcome IS apartment-dependent

The test is
`UtilitiesCS.Test/Threading/UiThreadInitContract_Tests.cs:309`,
`UiThreadInitRetryContract_Tests.Init_WhenFirstInitializeThrows_SecondInitWithWorkingFactorySucceedsAndPopulatesAllFourCaptureFields`,
body `:309-337`. It drives a throwing factory (`:315-316`), asserts the throw (`:318`), swaps in
a working factory (`:321-322`), and asserts the retry succeeds and captures all four values
(`:328-335`). It went red before the fix and green after
(`p2-t10-fail-before.md:105`, `:148`; `p3-t6-pass-after.md:37`, `:61`).

**The apartment dependency, and a latent weakness worth reporting.**

1. The class carries `[STATestClass]` (`:304`) and `[DoNotParallelize]` (`:305`). The class XML
   doc at `:299-302` states the reason: "The class is `[STATestClass]` because `Initialize()`
   must succeed here."
2. The test calls `UiThread.Init()` **directly on the test method's own thread** (`:317`, `:325`).
   If that thread is MTA, `UiThread.cs:31` throws before `Initialize()` runs, and the retry at
   `:328` (`retry.Should().NotThrow()`) fails. So the outcome **does** depend on the apartment of
   the thread it runs on.
3. **The test does not measure that apartment.** Unlike its sibling at `:244`, it contains no
   `Thread.CurrentThread.GetApartmentState()` assertion.
4. **Worse, the first assertion is apartment-blind.** `:318` is
   `failing.Should().Throw<InvalidOperationException>();` with **no** `.WithMessage`. The
   non-STA rejection at `UiThread.cs:33` throws the *same exception type* as the capture failure
   at `UiThreadInitContract_Tests.cs:68`. So under MTA, `:318` would pass for entirely the wrong
   reason and only `:328` would go red. The sibling test at `:352-354` does constrain the message
   (`.WithMessage(FakeUiCaptureSource.CaptureFailureMessage)`); this one does not. That asymmetry
   is a concrete, low-cost hardening the planner can bundle.
5. **Whether `[STATestClass]` guarantees STA for plain `[TestMethod]` members is not established
   from the tree.** MSTest.TestFramework 4.4.0 is pinned
   (`UtilitiesCS.Test/packages.config:151`). The in-tree record is actively contradictory on the
   mechanism: `p2-t10-fail-before.md:113` attributed the observed STA to the class sharing a
   serial bucket with an `[STATestClass]`, and `p6-t13-closure-summary.md:71` **withdraws** that
   explanation as "not established and should not be relied on". The load-bearing rule that
   survived both is `p6-t13-closure-summary.md:69`: "A test that needs a caller of a known
   apartment must create a dedicated thread and set the apartment explicitly, rather than relying
   on the ambient worker." By that rule, this test currently relies on the ambient/class-forced
   worker and is the one #782-related test that does not.

**Status: a retry test exists and passes; its apartment premise is unmeasured and its first
assertion cannot distinguish the two `InvalidOperationException` sources.** Recommended, and
cheap: add `Thread.CurrentThread.GetApartmentState().Should().Be(ApartmentState.STA);` as the
first Arrange line, and tighten `:318` to `.WithMessage(FakeUiCaptureSource.CaptureFailureMessage)`.
Both are in-file edits to an already-registered file.

---

## R5 — Test host hazard: the four stalling shell-icon classes

| # | Test class (fully qualified) | File | Declaration line |
|---|---|---|---|
| 1 | `UtilitiesCS.Test.HelperClasses.ShellUtilities_Tests` | `UtilitiesCS.Test/HelperClasses/ShellUtilities_Tests.cs` | `:10` (namespace `:7`) |
| 2 | `UtilitiesCS.Test.HelperClasses.ShellUtilitiesStatic_Tests` | `UtilitiesCS.Test/HelperClasses/ShellUtilitiesStatic_Tests.cs` | `:10` (namespace `:7`) |
| 3 | `UtilitiesCS.Test.HelperClasses.SysImageListHelperTests` | `UtilitiesCS.Test/HelperClasses/SysImageListHelperTests.cs` | `:12` (namespace `:9`) |
| 4 | `UtilitiesCS.Test.EmailIntelligence.OSBrowser_Tests` | `UtilitiesCS.Test/EmailIntelligence/OSBrowser_Tests.cs` | `:27` (namespace `:11`) |

`UtilitiesCS.Test/HelperClasses/ShellUtilitiesTests.cs:16` declares a *commented-out* class and
is not part of the set.

**Current status of the hazard, from the most recent probe.**
`docs/features/active/2026-09-06-quickfiler-crash-column-add-timeout-swallowed-keynotfound-798/evidence/baseline/shell-icon-stall-probe.md`
(timestamp `2026-09-07T00-55`) records at `:36-40` that **the stall no longer reproduces** — the
filtered set completes in ~2.2 s with no hang dump. It nonetheless records
`SHELL_ICON_EXCLUSION: REQUIRED` at `:72` for a different reason: two consecutive runs each
produced one non-deterministic failure out of 23, with the failing test moving between
`ShellUtilities_Tests` and `ShellUtilitiesStatic_Tests` and the diagnostic
`System.ArgumentException: Win32 handle that was passed to Icon is not valid or is the wrong type`
(`:49-53`). Exclude them, but describe the reason accurately in any artifact.

**Exact exclusion syntax used by this repository.** The canonical fragment, quoted verbatim
from `shell-icon-stall-probe.md:89`:

```
&FullyQualifiedName!~ShellUtilities&FullyQualifiedName!~SysImageListHelper&FullyQualifiedName!~OSBrowser
```

A full command line, quoted verbatim from
`docs/features/active/2026-09-07-utilitiescs-test-determinism-780-803-594-811/evidence/regression-testing/p8-t5-ac4-runs.md:5`,
which also shows the **blame hang timeout** form:

```
Command: <vstest> <nine assemblies> /EnableCodeCoverage /InIsolation "/Logger:trx;LogFileName=p8-t5-run<k>.trx" /ResultsDirectory:coverage/trx/p8-t5 /Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None "/TestCaseFilter:TestCategory!=LiveOutlook&FullyQualifiedName!~ShellUtilities&FullyQualifiedName!~SysImageListHelper&FullyQualifiedName!~OSBrowser"
```

The blame argument in isolation: `/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None`.
For a scoped run, prepend a positive selector inside the same quoted filter, as at
`.../798/evidence/regression-testing/p2-ac4-fail-before.md:35`:
`"TestCategory!=LiveOutlook&FullyQualifiedName~<ClassName>&FullyQualifiedName!~ShellUtilities&FullyQualifiedName!~SysImageListHelper&FullyQualifiedName!~OSBrowser"`.

**Two traps recorded in prior evidence.**
- `/Tests:` and `/TestCaseFilter:` are **mutually exclusive** in vstest
  (`.../784-787-788-809/evidence/other/p0-t15-mta-synccontextform-measurement.md:11`). Choose one.
- The `/Settings:` argument changes parallelization (see R3(a)), and therefore can change which
  thread a test lands on. A run whose purpose is an apartment measurement should record whether
  `/Settings:` was passed.

**Evidence form.** Record the counts, the per-test rows and the verbatim failure message in
Markdown under `.../816/evidence/`; write the `.trx` to a directory outside the repository (or a
gitignored `coverage/` path) and do not commit it, per the #671 decision. Point
`/ResultsDirectory:` accordingly.

---

## R6 — Where the predicate's tests live, and how a new file must be registered

### Existing tests for `SynchronizationContextAwaiter.IsCompleted`

All twelve live in one class:
`UtilitiesCS.Test/Threading/UiThread_Tests.cs`, `SynchronizationContextAwaiter_Tests`
(`[TestClass]` `:10`, `[DoNotParallelize]` `:11`, class `:12-347`). The eight `IsCompleted`
cases are at `:24`, `:38`, `:92`, `:116`, `:143`, `:170`, `:204`, `:241`, `:277`. Supporting
fixtures: private `TestSynchronizationContext` `:300-313` and private `StaDispatcherHost`
`:319-346`.

### File-size headroom (the reason a new file may be needed)

| File | Lines | Headroom to 500 |
|---|---|---|
| `UtilitiesCS/Threading/UiThread.cs` | 293 | 207 |
| `UtilitiesCS.Test/Threading/UiThread_Tests.cs` | 458 | 42 |
| `UtilitiesCS.Test/Threading/UiThreadInitContract_Tests.cs` | 460 | 40 |

Both test files can absorb a small number of tests but not a new fixture class. A B4 test needs
`UiThreadStateScope.SetUiSyncContext` plus an STA dispatcher host; the host already exists
privately in `UiThread_Tests.cs:319-346`, so the cheapest placement is **inside
`SynchronizationContextAwaiter_Tests`** — one test method of roughly 30 lines fits in the 42-line
headroom, two do not. If two or more are required, a new file is unavoidable.

### Exact `<Compile Include>` format and insertion point

`UtilitiesCS.Test/UtilitiesCS.Test.csproj` uses explicit items with a backslash-separated
relative path, self-closing, indented four spaces:

```
    <Compile Include="Threading\UiThread_Tests.cs" />
    <Compile Include="Threading\UiThreadInitContract_Tests.cs" />
```

Those are lines **511** and **512**. The `Threading\` block runs `:490-517`.

**The block is not alphabetically ordered.** Verified: `:490` `AppGlobalsConverterTests.cs`,
`:492` `ProgressPackage_Tests.cs`, `:497` `TaskPriority_Tests.cs`, `:500`
`TimeOutTaskCoverageTests.cs`, `:505` `ApplicationIdleTimer_Tests.cs`, `:509`
`AsyncMultiTasker_Tests.cs`, `:513` `WpfUiDispatcherTests.cs`, `:514` `CurrentStoreContextTests.cs`,
`:517` `StoreLockupResponderTests.cs`. There is no ordinal or alphabetical invariant to satisfy;
the governing convention is topical adjacency. A new `Threading\UiThread*.cs` entry therefore
goes **immediately after line 512 and before line 513**, giving:

```
511    <Compile Include="Threading\UiThread_Tests.cs" />
512    <Compile Include="Threading\UiThreadInitContract_Tests.cs" />
       <Compile Include="Threading\<NewFile>.cs" />          <-- insert here
513    <Compile Include="Threading\WpfUiDispatcherTests.cs" />
```

A new file under `TestHelpers\` would instead go adjacent to `:77-78`
(`TestHelpers\UiThreadDispatcherScope.cs`, `TestHelpers\UiThreadStateScope.cs`), in the
`ItemGroup` beginning at `:72`.

No production `.cs` file is expected to be added; `UtilitiesCS/Threading/UiThread.cs` is already
registered at `UtilitiesCS/UtilitiesCS.csproj:1114`.

---

## R7 — Known contention with the sibling `ProgressPackage` / `ProgressTrackerAsync` item

Reported for merge-risk assessment only. No coordination attempted.

| File | Line | Content | Sibling's likely action |
|---|---|---|---|
| `UtilitiesCS/UtilitiesCS.csproj` | 944 | `<Compile Include="Threading\ProgressPackage.cs" />` | Unchanged — editing a `.cs` needs no csproj change. |
| `UtilitiesCS/UtilitiesCS.csproj` | 971 | `<Compile Include="Threading\ProgressTrackerAsync.cs" />` | **Deleted.** |
| `UtilitiesCS.Test/UtilitiesCS.Test.csproj` | 492 | `<Compile Include="Threading\ProgressPackage_Tests.cs" />` | Unchanged unless the test file is also removed. |
| `UtilitiesCS.Test/UtilitiesCS.Test.csproj` | 496 | `<Compile Include="Threading\ProgressTrackerAsync_Tests.cs" />` | **Deleted** if the paired test file goes with the production file. |

**Merge-risk assessment for this issue.** Low, but not zero:

- In `UtilitiesCS/UtilitiesCS.csproj`, this issue touches nothing (`UiThread.cs` is already
  registered at `:1114`, 143 lines below `:971`). No overlap.
- In `UtilitiesCS.Test/UtilitiesCS.Test.csproj`, this issue's only possible edit is an insertion
  between `:512` and `:513`. The sibling's only edit is a deletion at `:496`. These are 16 lines
  apart in the same `ItemGroup` (`:490-517`). Git's default 3-line context means the hunks do
  not overlap and should auto-merge. A conflict would arise only if the sibling also reflows or
  re-sorts the block.
- Additional watch item, **not** on the sibling's declared list:
  `UtilitiesCS.Test/Threading/ProgressTrackerAsync_Tests.cs:222` calls
  `staThread.SetApartmentState(ApartmentState.STA)`. If that file is deleted, one STA-thread
  precedent disappears; it is not one this issue depends on
  (`ApartmentThreadRunner` is the dependency, and it lives in `UiThreadInitContract_Tests.cs`).

---

## Supporting Numeric Derivations

The two claim blocks below are the long-form derivations written during research. The normative,
machine-checked block is the `## Numeric Derivation Evidence` section at the end of this file.

### Claim N1 — `SynchronizationContextAwaiter.IsCompleted` has exactly five exits, of which two return `true` after the thread-id guard

- **Complete Family:** every `return` statement lexically inside the `get` accessor of
  `SynchronizationContextAwaiter.IsCompleted`.
- **Exhaustive Search Scope:** `UtilitiesCS/Threading/UiThread.cs:155-190`, read in full in this
  session. The accessor is not partial, has no local functions, and no `#if`; the type
  `SynchronizationContextAwaiter` is declared once (`:140`) and `IsCompleted` once (`:155`),
  confirmed by the whole-file read.
- **Inclusion Rules:** any `return` token whose enclosing block is the accessor body.
- **Exclusion Rules:** `return` tokens in `OnCompleted` (`:192-193`), `GetResult` (`:195`), or
  any other member; commented-out code.
- **Primary Search Strategy:** sequential full read of `UiThread.cs` lines 155-190 and manual
  enumeration of `return` tokens by line number.
- **Primary Member Set:** `{ :162 return true; :169 return false; :173 return false; :178 return true; :184-188 return <expression>; }`
- **Primary Count:** 5 exits; 2 of them (`:178`, and the `true` result of `:184-188`) can yield
  `true` after the `:171-174` thread-id guard.
- **Cross-check Search Strategy:** independent reconstruction from #809's coverage artifact,
  which enumerates the accessor's executable lines from the Cobertura document rather than from
  the source — `.../evidence/qa-gates/p6-t1-uithread-file-coverage.md:78-81`.
- **Cross-check Member Set:** that artifact names line `176` as the `if`, `177` as `{`, `178` as
  `return true;` for the `ReferenceEquals(_context, _uiSyncContext)` clause, and describes the
  alternative as "the dispatcher clause". It independently identifies the same two `true`-yielding
  clauses after the thread-id guard, and the `:81` sentence ("reached only when the caller stands
  on the owning UI thread with a non-null ambient context that is not the captured context")
  independently reconstructs the `:160-163`, `:167-170` and `:171-174` early exits.
- **Cross-check Count:** 5 exits; 2 `true`-yielding clauses after the guard.
- **Member-set Comparison:** the two member sets agree on every line number (`162`, `169`, `173`,
  `178`, `184-188`) and on the identification of `178` and the dispatcher clause as the two
  post-guard `true` results. No disagreement.

### Claim N2 — the shell-icon exclusion covers exactly four test classes in `UtilitiesCS.Test`

- **Complete Family:** every `[TestClass]`-bearing type in `UtilitiesCS.Test` matched by at least
  one of the three filter fragments `ShellUtilities`, `SysImageListHelper`, `OSBrowser` applied
  to its fully qualified name with vstest's `!~` (substring) operator.
- **Exhaustive Search Scope:** all `*.cs` under `UtilitiesCS.Test/`. Both the `HelperClasses/`
  and `EmailIntelligence/` directories are included; the family is not confined to one folder.
- **Inclusion Rules:** a non-commented `public class` declaration whose fully qualified name
  contains one of the three fragments.
- **Exclusion Rules:** production types in `UtilitiesCS/` (`ShellUtilities.cs:15`,
  `SysImageListHelper.cs:21`) are not test classes; the commented-out declaration at
  `ShellUtilitiesTests.cs:16` compiles to nothing.
- **Primary Search Strategy:** regex `public class (ShellUtilities\w*|SysImageListHelper\w*|OSBrowser\w*)`
  over `**/*.cs`, followed by a per-file namespace read to build the fully qualified name.
- **Primary Member Set:**
  `{ UtilitiesCS.Test.HelperClasses.ShellUtilities_Tests, UtilitiesCS.Test.HelperClasses.ShellUtilitiesStatic_Tests, UtilitiesCS.Test.HelperClasses.SysImageListHelperTests, UtilitiesCS.Test.EmailIntelligence.OSBrowser_Tests }`
- **Primary Count:** 4.
- **Cross-check Search Strategy:** independent derivation from the recorded probe run, which
  enumerated the set by *executing* the positive filter
  `FullyQualifiedName~ShellUtilities|FullyQualifiedName~SysImageListHelper|FullyQualifiedName~OSBrowser`
  and reporting the discovered totals — `.../798/evidence/baseline/shell-icon-stall-probe.md:13`,
  `:26-31`, `:36`, `:45`, `:63`, `:91-92`.
- **Cross-check Member Set:** the artifact names `ShellUtilitiesStatic_Tests` (`:45`) and
  `ShellUtilities_Tests` (`:63`) by fully qualified name, states at `:36` that the exclusion was
  created for "these four classes", and at `:91-92` states "The three name fragments cover both
  observed failing tests: the fragment `ShellUtilities` matches `ShellUtilities_Tests` and
  `ShellUtilitiesStatic_Tests` alike", implying the remaining two members are supplied by the
  `SysImageListHelper` and `OSBrowser` fragments.
- **Cross-check Count:** 4 (`:36`, "these four classes"), across 23 discovered test methods
  (`:26-31`).
- **Member-set Comparison:** the primary set's four members match the cross-check's cardinality
  of 4 and both of its explicitly named members. The two members the cross-check names only by
  fragment (`SysImageListHelperTests`, `OSBrowser_Tests`) are the unique `[TestClass]` matches
  for those fragments in the primary enumeration. No disagreement, and no fifth candidate
  survives the exclusion rules.

---

## Test strategy implications (no test code written)

1. **Keep the hardening and its test in one change.** `issue.md:79-80` requires it and the
   reasoning holds: H1 changes B4's truth conditions, so a test written before the change would
   assert a different contract.
2. **Fail-before is constructible for the B4 hardening.** A test that installs
   `_uiSyncContext` (via `UiThreadStateScope.SetUiSyncContext`, currently zero callers), installs
   `_uiThreadId` for a thread that is *not* the dispatcher owner, and asserts `IsCompleted == false`
   goes red under the current code (B4 returns `true`) and green under H1. Its positive twin —
   `_uiSyncContext` installed, evaluated on the real dispatcher-owning STA thread, asserting
   `true` — must pass both before and after, and is the case that closes the `177-178` coverage
   gap `p6-t1-uithread-file-coverage.md:81` recorded.
3. **AC5 has no fail-before and cannot have one.** It is a measurement obligation. Record a
   fail-before exception dossier at
   `.../816/evidence/regression-testing/fail-before-exception.<timestamp>.md` with
   `WhyFailingRunImpossible:` per
   `.claude/skills/evidence-and-timestamp-conventions/SKILL.md:133-143`.
4. **Determinism.** Every new test must set its apartment explicitly via
   `ApartmentThreadRunner.RunOnThread` or a dedicated `SetApartmentState` thread, per the
   surviving operational rule at `p6-t13-closure-summary.md:69`. No test may rely on the ambient
   worker's apartment.
5. **Serialization.** Any new class touching `UiThread` statics needs `[DoNotParallelize]`;
   `UiThreadStateScope.cs:15-26` and `UiThread.cs:116-121` both state that the scopes are not
   internally synchronized and depend on it.
6. **Scope discipline.** The eleven production await sites remain without ordering tests
   (`p6-t13-closure-summary.md:29-31`). H1 does not change behaviour on the leg those sites
   actually take today, but the residual should be restated in this issue's closure rather than
   silently inherited.

## Open questions the tree cannot settle

- Whether MSTest 4.4.0's `[STATestClass]` forces STA for plain `[TestMethod]` members, or whether
  the observed STA came from the vstest main execution thread. The in-tree record is explicitly
  contradictory (`p2-t10-fail-before.md:113` versus `p6-t13-closure-summary.md:71`). Settling it
  requires the runtime measurement described in R3(d), which is itself part of the deliverable.
- Whether `<viewer>.UiSyncContext` and `UiThread._uiSyncContext` are reference-equal on a live
  Outlook host. Code reading says yes; nothing in the repository asserts it.
- Whether #809's AC5 is to be checked off in #809's `spec.md` (requiring an artifact under the
  #809 folder) or restated as a new AC under #816. This is an orchestration decision.

---

## Numeric Derivation Evidence

This block is the normative, machine-checked derivation. It restates Claim N1 from the
Supporting Numeric Derivations section above in the exact label form the prd-feature gate parses.
The family is the set of exit points of the `IsCompleted` accessor, labelled in source order.

Complete Family: B1, B2, B3, B4, B5
Exhaustive Search Scope: the entire repository source tree, searched for every declaration of a SynchronizationContextAwaiter IsCompleted accessor before enumerating the exits of the one declaration that exists
Inclusion Rules: a return statement whose immediately enclosing accessor body is the IsCompleted get accessor of SynchronizationContextAwaiter
Exclusion Rules: return statements in OnCompleted, in GetResult, in the constructor, and in every other member or type in the tree
Primary Search Strategy or Query Expression: read UtilitiesCS/Threading/UiThread.cs lines 155 to 190 from top to bottom and label each exit in source order, yielding B1 then B2 then B3 then B4 then B5
Primary Member Set: B1, B2, B3, B4, B5
Primary Count: 5
Cross-check Search Strategy or Query Expression: reconstruct the exit list from the committed issue 809 coverage and fail-before artifacts, which quote the accessor line by line, then map each quoted line onto a label without rereading the accessor, recovering B5 and B4 and B3 and B2 and B1
Cross-check Member Set: B5, B4, B3, B2, B1
Cross-check Count: 5
Member-set Comparison: the primary and cross-check member sets are equal ignoring order and case, so the two independent enumerations agree on the cardinality of 5

Label key, for readers: B1 is the ambient-identity `true` at lines 160-163; B2 is the null-ambient
`false` at 167-170; B3 is the thread-id guard `false` at 171-174; B4 is the `_uiSyncContext`
`true` at 176-179, which is the branch this issue hardens; B5 is the dispatcher expression at
184-188.
