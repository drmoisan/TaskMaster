# Per-File Research — `QuickFiler/Controllers/EfcHomeController.Timing.cs`

- **Feature:** `2026-08-07-quickfiler-efc-home-controller-coverage-437` (issue #437)
- **Epic:** #136 `quickfiler-per-file-coverage`, child F8
- **Production file:** `C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-aea998f94efaa2eb4\QuickFiler\Controllers\EfcHomeController.Timing.cs` (43 lines)
- **Research date:** 2026-08-07
- **Method:** static read of the production file, of its sole caller
  (`EfcHomeController.HandleSelectionChangedAsync`), and of all seven existing
  `EfcHomeController*` test files. No build and no test run was performed (research-only mandate).

---

## 0. Constraints restated

1. `EfcHomeControllerDependencies` and `EfcHomeControllerDependencyFactories` are the injection-seam
   contract for the **whole** EFC controller family, including `EfcFormController` and
   `EfcItemController`, which belong to **sibling child F9**. This child must not propose or apply
   edits to F9's files. Any dependency-contract change must be **additive** so that F9 needs no
   edit; where that is impossible the gap is flagged as a **cross-child contract note**.
2. No change to `coverage.config` or to any shared build property file.
3. Tests: MSTest, Moq, FluentAssertions, deterministic, isolated, no temporary files, no external
   services, no live WinForms forms, no popups, no live Outlook store.
4. Seam hierarchy: **interface seam > injectable delegate > adapter**.
5. `Thread.Sleep`, `Task.Delay`, and real wall-clock waits are **prohibited** in tests.
6. No production file may exceed 500 lines.
7. Upstream dependency **F1 `quickfiler-coverage-ledger`** delivers the per-file coverage harness
   (the sole per-file coverage evidence mechanism for this epic) and the ratified exemption ledger at
   `docs/features/epics/quickfiler-per-file-coverage/coverage-ledger.md` (the future authority for
   whether any file is `ratified-exempt`). F1's outputs **do not exist on disk yet** and were not
   read or executed. Every coverage figure below is a **static estimate**.
8. `EfcHomeController.Timing.cs` carries **no** `[ExcludeFromCodeCoverage]` attribute, so it is
   already in the coverage denominator. Nothing here proposes adding one.

**Evidence location:** all coverage evidence for this child goes to
`docs/features/active/2026-08-07-quickfiler-efc-home-controller-coverage-437/evidence/qa-gates/`
per `.claude/skills/evidence-and-timestamp-conventions/SKILL.md`.

---

## 1. What this file actually is — the name is misleading

Despite the `.Timing.cs` suffix, **this file performs no timing measurement and reads no clock.**
It is a diagnostic-logging helper partial containing four `private static` methods that build and
emit one structured log line describing the *environment* in which a first-selection phase ran.

**Answer to the required time-source question: there is no time source in this file.** There is no
`DateTime.Now`, no `DateTime.UtcNow`, no `Stopwatch`, no `Environment.TickCount`, and no
`TimeProvider` read anywhere in `EfcHomeController.Timing.cs`. **No injected-clock seam is therefore
required for this file**, and proposing one would add an unused abstraction. The determinism hazards
that do exist here are two *ambient statics*, analysed in §3.

For completeness, the wall-clock and elapsed-time reads in the rest of the F8 file set are:

| Reader | File | Line | Already seamed? |
| --- | --- | --- | --- |
| `Stopwatch.StartNew()` and `selectionStopwatch.ElapsedMilliseconds` | `EfcHomeController.cs` | L176, L192 | **No** — but the value is only interpolated into a log string that no test asserts, so it creates no test-determinism problem today |
| `_stopWatch.Elapsed.Seconds` | `EfcHomeController.Metrics.cs` | L23 | No — see that file's own artifact |
| `DateTime.Now` | `EfcHomeControllerDependencies.cs` | L77 (`MetricsNowFactory` default) | **Yes** — injectable delegate, already substituted by `EfcHomeControllerMetricsTests.CreateController` |

The `Stopwatch` at `EfcHomeController.cs` L176/L192 is the only unseamed elapsed-time read reachable
from this file's caller. It is **not** a blocker: the sole consumer is the interpolated
`elapsedMs=` fragment in the details string, and the recommended assertions (§4) never assert an
exact elapsed value. Introducing a clock seam solely to make that fragment assertable would be
speculative abstraction and is rejected in §6.

---

## 2. Member-by-member inventory

The file has one region: four `private static` helpers on `public partial class EfcHomeController`.
Note that `logger` (used at L38) is declared in `EfcHomeController.cs` L20-22, so this partial has a
cross-partial dependency on that static field.

| Lines | Member | Executable statements | Status | Covering test (class.method) |
| --- | --- | --- | --- | --- |
| L9-12 | `private static string DescribeSynchronizationContext(SynchronizationContext syncContext)` | L11 (single `return` with `?.`/`??`) | **line COVERED**, **branch PARTIAL** | reached transitively by `EfcHomeControllerTests.BuildFirstSelectionTimingContext_WhenEventsUnavailable_ReportsUnknownOverlapState` and by `...LogFirstSelectionTiming_AcceptsUnprefixedPhaseWithoutThrowing`. Which arm executes depends on the ambient `SynchronizationContext.Current` in the MSTest worker — **non-deterministic**, see §3.1 |
| L14-17 | `private static string DescribeStartupOverlapState(IApplicationGlobals globals)` | L16 (single `return` ternary) | **line COVERED**, **branch PARTIAL** | `EfcHomeControllerTests.BuildFirstSelectionTimingContext_WhenEventsUnavailable_ReportsUnknownOverlapState` asserts `startupOverlapState=unknown`; its `FakeApplicationGlobals.Events => null`. The **`"correlated"` arm (globals with non-null `Events`) is UNCOVERED** |
| L19-25 | `private static string BuildFirstSelectionTimingContext(IApplicationGlobals globals, int selectedItemCount)` | L24 (single interpolated `return`) | **COVERED** | `EfcHomeControllerTests.BuildFirstSelectionTimingContext_WhenEventsUnavailable_ReportsUnknownOverlapState` (invoked by reflection; asserts `selectedItemCount=2`, `startupOverlapState=unknown`, and `threadId=`) |
| L27-41 | `private static void LogFirstSelectionTiming(string phase, IApplicationGlobals globals, int selectedItemCount, string details = null)` | L34 (`detailSegment`), L35-37 (`phaseLabel`), L38-40 (`logger.Debug`) = 3 statements | **lines COVERED**, **branches PARTIAL** | `EfcHomeControllerTests.LogFirstSelectionTiming_AcceptsUnprefixedPhaseWithoutThrowing` (invoked by reflection; unprefixed phase + non-empty details, asserts only "does not throw"). Also traversed twice per call of `EfcHomeController.HandleSelectionChangedAsync` (L179, L188) with an **already-prefixed** phase and non-empty details, so both ternary arms of `phaseLabel` are exercised across the suite. The **`details` null/whitespace arm producing `string.Empty` is UNCOVERED** |

### 2.1 Static line-coverage estimate

Executable statements: **6** (L11, L16, L24, L34, L35-37, L38-40). All six are reached by existing
tests. **Estimated current line coverage: 100%.**

This file therefore **already clears the 80% per-file floor** and needs no new production seam to do
so. The estimate is manual and static; the authoritative figure is F1's per-file harness output,
captured to `<FEATURE>/evidence/qa-gates/`.

**Planning consequence:** F8's genuine work on this file is *not* line coverage. It is (a) closing
three uncovered conditional arms, (b) removing the non-determinism in §3.1 so the file's coverage is
reproducible run to run, and (c) replacing two brittle reflection-based "does not throw" assertions
with real behavioural assertions. A plan phase that adds tests here purely to raise a line
percentage would be redundant work.

---

## 3. Determinism analysis (the real risk in this file)

### 3.1 `SynchronizationContext.Current` — ambient, non-deterministic

`BuildFirstSelectionTimingContext` (L24) reads the ambient `SynchronizationContext.Current` and
passes it to `DescribeSynchronizationContext`. Under `vstest.console.exe` the current context in a
worker thread may be `null` or may be a framework-supplied context depending on the async
continuation the test happens to be on. That means:

- which arm of `syncContext?.GetType().FullName ?? "null"` executes is **not reproducible**;
- any test asserting the exact `syncContext=` substring would be **flaky**.

The existing test correctly avoids this by asserting only `Contains("threadId=")` and the two
deterministic fields. Preserve that discipline.

### 3.2 `Thread.CurrentThread.ManagedThreadId` — ambient, non-deterministic value

L24 interpolates the managed thread id. The value varies per run and per test-parallelism setting.
Assert `Contains("threadId=")` only — never an exact id. The existing test already does this; it is
the correct precedent.

### 3.3 No prohibited waits

Nothing in this file sleeps, delays, or waits. No test proposed below introduces one. The prohibition
on `Thread.Sleep`, `Task.Delay`, and real wall-clock waits is satisfied trivially.

---

## 4. Genuine remaining gaps and the test scenario required for each

### T1 — `DescribeStartupOverlapState` "correlated" arm is never executed (UNCOVERED branch)

- **Scenario:** positive (the correlated-startup case).
- **How, deterministically:** call the helper with an `IApplicationGlobals` whose `Events` is
  non-null. `Mock<IApplicationGlobals>(MockBehavior.Loose)` with
  `SetupGet(g => g.Events).Returns(Mock.Of<TaskMaster.IAppEvents>())` is sufficient — no COM, no
  Outlook, no form. Assert the result is `"correlated"`.
- **Also cover the two boundary arms explicitly:** `globals == null` → `"unknown"` (the `?.` arm,
  distinct from the `Events == null` arm and currently only reached through the `Events == null`
  path), and `globals.Events == null` → `"unknown"`.
- **New seam required:** none, given the visibility change in T4.

### T2 — `DescribeSynchronizationContext` both arms, deterministically

- **Scenario:** positive plus boundary.
- **How:** invoke the helper **directly with an explicit argument** rather than relying on the
  ambient `SynchronizationContext.Current`: pass `null` and assert `"null"`; pass
  `new SynchronizationContext()` and assert the result equals
  `typeof(SynchronizationContext).FullName`. Because the argument is supplied by the test, both arms
  are reached deterministically and §3.1's flakiness does not apply.
- **New seam required:** none, given the visibility change in T4.

### T3 — `LogFirstSelectionTiming` `details` null/whitespace arm is never executed (UNCOVERED branch)

- **Scenario:** invalid/absent input.
- **How:** invoke with `details: null` and again with `details: "   "`, and assert the emitted
  message contains **no** ` | ` separator after the context block. Today the test can only assert
  "does not throw" because the method returns `void` and writes to the static `logger` — which is
  why T5 is recommended.
- **Also assert the already-prefixed phase arm** (`phase` starting with `[First-selection timing]`
  must not be double-prefixed). That arm is executed today only as a side effect of
  `HandleSelectionChangedAsync`; there is no direct assertion that double-prefixing is avoided,
  which is exactly the kind of formatting regression this test should pin.
- **New seam required:** none for reaching the arm; T5 is required to *assert* on the output.

### T4 — Replace reflection with an `internal` visibility widening (recommended, additive)

- **Finding:** all four members are `private static`, so
  `EfcHomeControllerTests.BuildFirstSelectionTimingContext_...` and
  `...LogFirstSelectionTiming_AcceptsUnprefixedPhaseWithoutThrowing` reach them through
  `Type.GetMethod(..., BindingFlags.NonPublic | BindingFlags.Static)` and `Invoke`. Reflection-based
  tests are brittle (a rename silently degrades them to a `Should().NotBeNull()` failure rather than
  a compile error) and force `object[]`-boxed arguments.
- **Recommendation:** widen `DescribeSynchronizationContext`, `DescribeStartupOverlapState`,
  `BuildFirstSelectionTimingContext`, and `LogFirstSelectionTiming` from `private static` to
  `internal static`. `QuickFiler\Properties\AssemblyInfo.cs` already declares
  `[assembly: InternalsVisibleTo("QuickFiler.Test")]`, so the test project gains direct, compile-checked
  access with **zero** runtime behavior change and **zero** public-API change.
- **Why this and not a delegate seam:** per the seam hierarchy, the cheapest correct mechanism wins.
  These are pure functions of their arguments; they need no substitution, only visibility. A delegate
  or interface seam here would be over-engineering.
- **Cross-child impact:** none. These are members of `EfcHomeController`, not of
  `EfcHomeControllerDependencies`, so F9 is untouched.

### T5 — Extract a pure message builder so the log output can be asserted (recommended, additive)

- **Finding:** `LogFirstSelectionTiming` composes a message and immediately hands it to the static
  log4net `logger`. Because the composition and the emission are fused, the only assertion available
  today is "does not throw" — which is why the `details` and `phaseLabel` arms have no behavioural
  coverage.
- **Recommendation (preferred, simplest):** extract the composition into a pure function on the same
  partial:
  `internal static string BuildFirstSelectionTimingMessage(string phase, IApplicationGlobals globals, int selectedItemCount, string details)`
  containing the existing `detailSegment` / `phaseLabel` / interpolation logic, and reduce
  `LogFirstSelectionTiming` to `logger.Debug(BuildFirstSelectionTimingMessage(...));`. Tests then
  assert on the returned string with full determinism (subject to §3.1/§3.2: assert `Contains`, not
  equality, for the ambient fields). No mutable static, no test-ordering coupling, no log-appender
  fixture.
- **Rejected alternative — an injectable static log sink** (`internal static Action<string> TimingLogSink { get; set; } = message => logger.Debug(message);`):
  it would make emission observable, but it introduces mutable global state requiring a
  `[TestCleanup]` reset in every consuming class, repeating the isolation hazard already present in
  `EfcHomeControllerDependencyFactories.cs`'s eleven `Production*` statics. The pure-builder
  extraction achieves the same assertability with no shared mutable state. Rejected on isolation
  grounds.
- **Rejected alternative — a log4net memory appender fixture:** couples tests to logging
  configuration, is process-global, and is not isolated across parallel test classes. Rejected.
- **Cross-child impact:** none — new member on `EfcHomeController`, not on the dependency bundle.

---

## 5. Seam inventory and file-size risk

### 5.1 Existing seams reaching this file

**None.** This file has no injected dependency of any kind. It consumes:

- its `IApplicationGlobals` parameter (supplied by the caller; substitutable with `Mock<IApplicationGlobals>`
  or the hand-written `FakeApplicationGlobals` already present in three test files);
- the ambient statics `Thread.CurrentThread` and `SynchronizationContext.Current` (§3);
- the static `logger` field declared in `EfcHomeController.cs`.

### 5.2 New seams proposed

| Proposal | Kind | Additive? | F9 impact |
| --- | --- | --- | --- |
| T4 — widen four helpers to `internal static` | visibility only, not a seam | Yes | None |
| T5 — extract `BuildFirstSelectionTimingMessage` pure function | pure-function extraction, not a seam | Yes | None |

No interface seam, delegate seam, or adapter is required. **No injected clock is required**, because
the file reads no clock (§1).

### 5.3 File-size risk

`EfcHomeController.Timing.cs` is at **43 of 500** lines — 457 lines of headroom.

| Change | Net lines |
| --- | --- |
| T4 visibility widening | 0 |
| T5 pure-builder extraction | approximately +10 to +14 |

Projected size ~55 lines. **No 500-line risk and no partial split required.** This file is also the
natural destination for any future first-selection diagnostic logic that would otherwise push
`EfcHomeController.cs` (441/500) toward its ceiling — see that file's artifact §5.

---

## 6. COM / Outlook-Interop and WinForms exposure

**None.** `EfcHomeController.Timing.cs` imports only `System`, `System.Threading`, and `UtilitiesCS`.
`Microsoft.Office.Interop.Outlook` is not imported. Neither `Application`, `MailItem`, `Store`, nor
`MAPIFolder` appears anywhere in the file. `System.Windows.Forms` is not imported and no form,
control, dispatcher, or popup is touched.

The only external contact is `globals?.Events` — a property read on the `IApplicationGlobals`
**interface**, which is fully mockable and triggers no COM call.

Consequently the CLAUDE.md § UT2 COM/VSTO/WinForms exemption **does not apply to this file under any
reading**, and it must meet the >= 80% floor. It already does (§2.1). F1's ledger should classify it
`testable` with no line-level irreducible remainder.

---

## 7. Rejected alternatives

- **Add an injected clock (`Func<DateTime>` or `TimeProvider`) to this file.** Rejected: the file
  reads no clock. Adding one would be an unused abstraction contradicting the "simplicity first"
  design principle. The one unseamed elapsed-time read in the family
  (`Stopwatch` at `EfcHomeController.cs` L176/L192) lives in a different file, is only interpolated
  into a log string, and is not asserted by any recommended test.
- **Seam the `Stopwatch` in `HandleSelectionChangedAsync` so `elapsedMs=` becomes assertable.**
  Rejected for F8 scope: it would add an injectable timing seam to `EfcHomeController.cs` purely to
  assert a diagnostic substring that has no behavioural contract. If a future change gives
  `elapsedMs` a behavioural role (for example, a threshold-triggered warning), revisit it then and
  place the seam in this partial.
- **Injectable static log sink** — see T5; rejected on test-isolation grounds.
- **log4net memory-appender test fixture** — see T5; rejected as process-global and not isolated.
- **Leave the four helpers `private` and keep using reflection.** Rejected: reflection tests are
  brittle and cannot assert the `details`/`phaseLabel` arms meaningfully. `InternalsVisibleTo` is
  already configured, so the cost of the fix is zero.

---

## 8. Cross-child contract notes

**None required for this file.** Both recommendations (T4, T5) are confined to
`QuickFiler/Controllers/EfcHomeController.Timing.cs` plus new tests under
`QuickFiler.Test/Controllers/`. Neither `EfcHomeControllerDependencies` nor
`EfcHomeControllerDependencyFactories` is modified, so the injection contract consumed by F9's
`EfcFormController` and `EfcItemController` is untouched and **no F9 file needs to be read or
edited**.

---

## 9. Do not duplicate — scenarios already covered

- `BuildFirstSelectionTimingContext` reporting `selectedItemCount=<n>`, `startupOverlapState=unknown`
  (globals with null `Events`), and the presence of a `threadId=` field —
  `EfcHomeControllerTests.BuildFirstSelectionTimingContext_WhenEventsUnavailable_ReportsUnknownOverlapState`.
  Only the **`correlated`** arm (T1) is open.
- `LogFirstSelectionTiming` accepting an **unprefixed** phase with **non-empty** details without
  throwing — `EfcHomeControllerTests.LogFirstSelectionTiming_AcceptsUnprefixedPhaseWithoutThrowing`.
  Only the **null/whitespace details** arm and the **already-prefixed phase** assertion (T3) are open.
- Transitive execution of all four helpers through `HandleSelectionChangedAsync` —
  `EfcHomeControllerSeamTests.HandleSelectionChangedAsync_SnapshotsSelectionBeforeAsyncDataLoad`,
  `EfcHomeControllerSeamTests.CreateAsync_WithExplicitMail_UsesSelectionAndInitializationFactories`,
  `EfcHomeControllerSeamTests.LoadFinderAsync_WithEmptySelection_InitializesFindShellAndDummyDataModel`,
  `EfcHomeControllerLifecycleTests.CreateAsync_PublicWrapper_UsesInjectedDefaultDependencies`, and
  `EfcHomeControllerLifecycleTests.LoadFinderAsync_PublicWrapper_UsesInjectedDefaultDependencies`.
  Do not add another end-to-end test whose only purpose is to reach these lines — they are already
  reached.
- The selection-snapshot behaviour that `HandleSelectionChangedAsync` wraps —
  `EfcHomeControllerTests.CaptureSelectionSnapshot_ReturnsIndependentCopyBeforeBackgroundModelLoad`
  and `EfcHomeControllerSeamTests.HandleSelectionChangedAsync_SnapshotsSelectionBeforeAsyncDataLoad`
  (that logic lives in `EfcHomeController.cs`, not here).

---

## 10. Testing strategy summary (no test code written here)

- **Placement:** a new `QuickFiler.Test/Controllers/EfcHomeControllerTimingTests.cs`, mirroring the
  production tree per `.claude/rules/general-unit-test.md` § Test File Location. Migrate the two
  existing reflection-based timing tests out of `EfcHomeControllerTests.cs` into it once T4 lands,
  so all timing coverage is in one place and no reflection remains.
- **Shape:** pure-function tests over `BuildFirstSelectionTimingMessage`,
  `BuildFirstSelectionTimingContext`, `DescribeStartupOverlapState`, and
  `DescribeSynchronizationContext`. Arrange-Act-Assert, FluentAssertions, `Mock<IApplicationGlobals>`
  for the `Events`-present case.
- **Determinism rules for this file specifically:** assert `Contains("threadId=")` and never an exact
  thread id; supply the `SynchronizationContext` explicitly rather than depending on
  `SynchronizationContext.Current`; never assert an exact `elapsedMs` value. No `Thread.Sleep`, no
  `Task.Delay`, no wall-clock wait — none is needed because the file reads no clock.
- **Isolation:** no mutable global state is introduced by any recommendation, so no `[TestCleanup]`
  reset is required for this file's tests (unlike the `EfcHomeController.cs` static-factory tests and
  the `EfcHomeControllerDependencyFactories.cs` `Production*` statics).
- **Coverage evidence:** run F1's per-file harness after F1 merges to the integration branch and
  commit the numeric per-file result for this file to `<FEATURE>/evidence/qa-gates/`. Aggregate
  assembly coverage does not satisfy issue #136.
- **Toolchain:** `csharpier .` → analyzer msbuild → nullable msbuild →
  `vstest.console.exe ... /EnableCodeCoverage`, restarting from step 1 on any failure or auto-fix.
