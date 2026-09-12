# 2026-09-11-qfcqueue-enqueue-path-lacks-injectable-seams (Spec)

- **Issue:** #871
- **Parent (optional):** none
- **Owner:** drmoisan
- **Last Updated:** 2026-09-12T10-35
- **Status:** Draft
- **Version:** 0.2
- **Work Mode:** full-bug — this document is the sole authoritative acceptance-criteria source. The companion user story in this folder is narrative context only and carries no checkboxes.

> Formatting contract — do not "fix" the backtick formatting in this document. A downstream
> change-footprint harvester treats every whitespace-free backticked token as a claim that the change
> writes to that path, and it has no notion of negation. Only files this change creates or modifies
> are written as backticked repository-relative paths. Every file the change does **not** touch is
> named in plain prose, deliberately without backticks and, where possible, without slashes. The
> `## Write Set` section below is the authoritative footprint.

## Context

`QfcQueue.EnqueueAsync` and `QfcQueue.LoadControllersViewersAsync` drive a live WinForms
`TableLayoutPanel`, per-row `ItemViewer` construction, UI idle-call marshalling, and the
`EmailMoveMonitor` hook through private members with no injectable seam, so their control flow cannot
be unit-tested without a live Outlook window. Item #678 left its acceptance criterion AC20 unchecked
for this reason and issue #727 sub-finding 4 recorded it as a policy gap. The maintainer decision
recorded on #727 on 2026-09-11 is that the resolution is a seam, not a coverage exemption.

**Corrected baseline figures.** The promotion scaffold carried two stale numbers that research
re-derived against base commit 2405a829d and replaced:

- `QuickFiler/Controllers/QfcQueue.cs` is **507 lines**, not 439. It is therefore already 7 lines over
  the repository's 500-line hard ceiling *before* any seam is added. The file split described below is
  mandatory, not conditional.
- `QuickFiler/Controllers/QfcQueue.Enqueue.cs` is **200 lines** and is **15.3% covered — 13 of 85
  lines**, not 0%. The honest size of the uncovered work on that file is **72 lines**. The covered
  portion is the primary constructor, `ResolveCarriedHandler`, and the `ItemControllerFactory` default
  lambda, all of which item #678 introduced. The member-level claim of zero coverage for
  `EnqueueAsync` and `LoadControllersViewersAsync` remains correct; the file-level claim of zero does
  not.

Both figures were measured from the committed Cobertura evidence for item 825 (the file named
coverage-postchange.cobertura.xml under that feature folder's evidence qa-gates directory) and from a
tracked-file line count at the same commit. The file-level line rates recorded there are 0.503205 for
the base part and 0.152941 for the enqueue part.

Environment:

- OS/version: Windows 11 Pro 10.0.26200
- Runtime: C# / .NET Framework 4.8.1 VSTO add-in; MSTest, Moq, FluentAssertions
- Test assembly consumed by the runner: the QuickFiler test project's Debug output assembly
- Data source or fixture: the committed Cobertura evidence for item #678 (PR #724) and for item 825

Impact / Severity:

- [ ] Blocker
- [ ] High
- [x] Medium
- [ ] Low

Medium: no incorrect production behavior. Two members on the high-confidence display path carry zero
test coverage, and every future change to them is unverifiable by the unit suite.

## Repro & Evidence

Steps to Reproduce:

1. Search the QuickFiler test project for `EnqueueAsync`: zero references across the three existing
   QfcQueue test files under its Controllers folder.
2. Read `QuickFiler/Controllers/QfcQueue.Enqueue.cs` lines 71-138. `EnqueueAsync` calls
   `_moveMonitor.HookItem` on a field initialised inline to `new EmailMoveMonitor()`
   (`QuickFiler/Controllers/QfcQueue.cs` line 42), clones `_tlpTemplate` through `UiIdleCallAsync`,
   and awaits `LoadControllersViewersAsync` through `UiIdleAsyncCallAsync`.
3. Read `QuickFiler/Controllers/QfcQueue.Enqueue.cs` lines 154-198. `LoadControllersViewersAsync`
   calls `AddAsync` (`QuickFiler/Controllers/QfcQueue.cs` line 264), which constructs a viewer and
   adds it to the `TableLayoutPanel` on the UI idle path. Only the item-controller construction that
   follows is behind a seam (`ItemControllerFactory`, added by #678).
4. Attempt to write an MSTest that exercises either member: the accessor for the process-wide WPF
   dispatcher in the UtilitiesCS threading namespace throws `InvalidOperationException` when unset, so
   every one of the four UI idle call sites throws at the first read in a headless test host.

Expected:

Both members accept substitutes for every boundary they drive, following the injectable-seam guidance
already applied to `ItemControllerFactory`. Tests then cover the argument-guard branches, the
`_jobsRunning` increment and decrement, the `OperationCanceledException` and general-exception catch
paths, the `CollectionChanged` raise in `finally`, the `digits` computation, and the carrier
resolution per row, without a live WinForms viewer or Outlook process.

Actual:

Neither member is reachable from a unit test. The production defaults are constructed inline from
private fields, so no test can substitute them. Member-level coverage on both is 0%, and item #678's
AC20 remains unchecked.

Logs / Screenshots:

- [x] Cobertura evidence cited above stands in for logs; no screenshot is applicable to a coverage
      and testability defect.
- Snippet: item #678 review, AC20 recorded PARTIAL (PR #724); issue #727 sub-finding 4.

Facts confirmed by research that remove risk from the plan:

- `QfcQueue` is already constructible in a unit test today. All three existing QfcQueue test files
  build a real instance through the real primary constructor, passing a literal
  `(QfcHomeController)null` and a loose Moq `IApplicationGlobals`. No new construction affordance is
  needed.
- Constructing `QfcQueue` touches neither the WPF dispatcher nor Outlook COM: the move monitor's
  default marshalling delegate is lazy and its setup method only assigns a delegate field.
- The QuickFiler production assembly already carries an `InternalsVisibleTo` attribute for the
  QuickFiler test assembly, so `internal` seam members are visible to the tests.

## Scope & Non-Goals

- In scope:
  - The injectable seams `MoveMonitor`, `UiIdleDispatcher`, `ItemViewerFactory`, `ViewerRowPlacer`,
    `ItemGroupFactory`, and `BackgroundTlpFactory` on `QfcQueue`.
  - The new narrow internal interface `IUiIdleDispatcher` and its production adapter.
  - The mandatory split of `QuickFiler/Controllers/QfcQueue.cs` into
    `QuickFiler/Controllers/QfcQueue.Tlp.cs` and `QuickFiler/Controllers/QfcQueue.UiIdle.cs`, which is
    required by the 500-line ceiling independently of the seams.
  - The new MSTest suite, one partial test class split across
    `QuickFiler.Test/Controllers/QfcQueueEnqueueTests.cs` and
    `QuickFiler.Test/Controllers/QfcQueueEnqueueTests.Harness.cs`.
  - The `<Compile Include>` manifest entries in `QuickFiler/QuickFiler.csproj` and
    `QuickFiler.Test/QuickFiler.Test.csproj` that the new files require.
  - The feature documents and evidence directories listed in the Write Set.

- Out of scope / non-goals. The paths in this subsection are deliberately written in plain prose
  without backticks so that the change-footprint harvester does not read them as write claims; do not
  add backticks to them.
  - The unbalanced job-counter control flow that this research uncovered has been promoted separately
    as its own potential bug entry and must not be fixed here. Changing it would mix a behavioural fix
    into a testability change. The increment of the running-jobs counter sits outside the try block
    whose finally decrements it, so a throw from the hook loop or the template clone leaks the counter
    permanently; that is a real defect, it is not this item's defect, and no test written here may
    assert the leaking behaviour as correct.
  - Any edit to the UtilitiesCS threading types. The existing UI dispatcher interface, its WPF
    adapter, and the static UI thread holder in that namespace are all left exactly as they are.
  - Any edit to the three existing QuickFiler test files that address QfcQueue — the base test file,
    the pure-paths test file, and the coverage-expansion test file, all under the QuickFiler test
    project's Controllers folder. Their reflection-based access to the private move-monitor field is
    exactly what the seam design preserves.
  - Deleting the dead, entirely commented-out template-activation member. Relocating it during the
    split moves dead code rather than deleting it; deletion is a separate, larger decision.
  - Covering the reflection-based control clone extension in the UtilitiesCS extensions namespace, or
    the template-setter that calls it.
  - Any coverage exemption attribute or assembly-level exclusion. The maintainer decision on #727
    sub-finding 4 dated 2026-09-11 rules this out, and the repository's coverage-exclusion policy
    prohibits excluding a production file from measurement.

- Explicitly excluded systems, integrations, or datasets: Outlook COM, the live WPF dispatcher and
  message pump, the process-global static viewer queue and its test-only core swap, the filesystem,
  the network, and any temporary file. No test added by this item may touch any of them.

## Root Cause Analysis

Five boundaries on the enqueue path are constructed or reached inline, with no substitution point.

1. **Move monitor.** `QuickFiler/Controllers/QfcQueue.cs` lines 41-42 initialise `_moveMonitor` inline
   to `new EmailMoveMonitor()`. A load-bearing comment on line 41 records why the instance is
   per-owner and not shared (issue #731 finding 1, issue #620).

2. **UI idle marshalling.** `QuickFiler/Controllers/QfcQueue.cs` lines 472-505 hold three members —
   `UiIdleCallAsync(Action)`, `UiIdleCallAsync<T>(Func<T>)`, and `UiIdleAsyncCallAsync<T>(Func<Task<T>>)`
   — each of which reads the static WPF dispatcher directly at
   `DispatcherPriority.ContextIdle`. The accessor for that dispatcher throws when the host has not
   started, which is the hard blocker: it defeats all four internal call sites
   (`QuickFiler/Controllers/QfcQueue.cs` lines 197 and 275,
   `QuickFiler/Controllers/QfcQueue.Enqueue.cs` lines 97 and 105) regardless of any other seam.

3. **Viewer construction and row placement.** `QuickFiler/Controllers/QfcQueue.cs` lines 264-277
   (`AddAsync`) dequeues a real `ItemViewer` from a static queue and then sets five `Control`
   properties on it inside `AddViewerToTlp`.

4. **Background template clone.** `QuickFiler/Controllers/QfcQueue.Enqueue.cs` lines 97-99 clone
   `_tlpTemplate` through a reflection-driven copy of every public instance property of the control.
   Whether that succeeds headlessly is unverified and unverifiable without executing it, and the
   template setter that performs the same clone is itself measured at zero coverage. Everything after
   line 99 in `EnqueueAsync` is gated on it.

5. **Existing precedent.** `QuickFiler/Controllers/QfcQueue.Enqueue.cs` lines 25-56 already carry
   `ItemControllerFactory`, an `internal` auto-property `Func<>` seam whose non-null production
   default reproduces the previous construction expression argument for argument. That is the pattern
   to copy wherever the language permits it.

Two constraints discovered by research change the shape of the fix and are settled, not open:

- **A constructor-parameter seam for the move monitor is illegal.** `IEmailMoveMonitor` is `internal`
  and `QfcQueue` is `public`, so an `IEmailMoveMonitor` primary-constructor parameter is CS0051 and a
  `public` property of that type is CS0053. The seam must be an `internal` property layered over the
  retained `_moveMonitor` field. The field must be **retained**, not converted to an auto-property,
  because six existing test methods reflect on the backing field name `_moveMonitor` and assert it is
  non-null before setting it; an auto-property would rename the backing field and fail all six.
- **An instance field or auto-property initializer cannot reference `this`.** Any seam whose
  production default is an *instance* method of `QfcQueue` (`AddAsync`, `AddViewerToTlp`) therefore
  cannot use the initializer form that `ItemControllerFactory` uses. Those seams must use a lazy `??=`
  property getter, which does have `this` in scope and still yields a non-null default on first read.

Finally, the file-size defect: `QuickFiler/Controllers/QfcQueue.cs` at 507 lines already exceeds the
500-line ceiling stated in both CLAUDE.md and the general code-change rule file. Neither exception
clause applies. The split is a precondition of adding roughly 25 further lines of seam.

## Proposed Fix

### Design summary (what changes where)

**Invariant being established:** every boundary that `EnqueueAsync` and `LoadControllersViewersAsync`
reach — the move-monitor hook, the UI idle marshalling, the background template clone, the viewer
construction, and the row placement — becomes replaceable through an `internal` member on `QfcQueue`
whose production default reproduces the previous expression argument for argument, so that the
production call graph is byte-for-byte unchanged while a test can substitute any single boundary.

Trace of one accepted value through the changed path, to make the equivalence auditable: a caller
passes a non-null, non-empty `items` list to `EnqueueAsync`. Guards at
`QuickFiler/Controllers/QfcQueue.Enqueue.cs` lines 79-86 do not throw. Line 91's
`_moveMonitor.HookItem` becomes `MoveMonitor.HookItem` — in production the property returns the same
instance the field initializer created, so the same monitor receives the same item and the same
async-void lambda. Line 94 increments the running-jobs counter, unchanged. Lines 97-99 become
`await UiIdleCallAsync(() => BackgroundTlpFactory(_tlpTemplate))` — in production
`BackgroundTlpFactory` is `template => template.Clone(name: "BackgroundTableLayout")`, the identical
call with the identical named argument, still executed inside the identical `UiIdleCallAsync`
wrapper, which now forwards to an adapter holding the identical `DispatcherPriority.ContextIdle`
body. Line 177's `AddAsync(tlp, items[i - start], i)` becomes
`ItemGroupFactory(tlp, items[i - start], i)`, whose default is the `AddAsync` method group, so the
same instance method receives the same three arguments. Inside `AddAsync`,
`ItemViewerQueue.Dequeue(_token)` becomes `ItemViewerFactory(_token)`, whose default is the
`ItemViewerQueue.Dequeue` method group, and `AddViewerToTlp(tlp, viewer, indexNumber)` becomes
`ViewerRowPlacer(tlp, viewer, indexNumber)`, whose default is the `AddViewerToTlp` method group. The
value reaches `_queue.Add((tlp, itemGroups))` at line 116 having visited exactly the same production
methods with exactly the same arguments as before.

Seam table:

| # | Member | Declared on | Form | Production default |
|---|---|---|---|---|
| S1 | `internal IEmailMoveMonitor MoveMonitor { get; set; }` | `QuickFiler/Controllers/QfcQueue.cs` | property over the retained `_moveMonitor` field | the existing field initializer, unchanged |
| S2 | `internal IUiIdleDispatcher UiIdleDispatcher { get; set; }` | `QuickFiler/Controllers/QfcQueue.UiIdle.cs` | lazy `??=` property, interface seam | a new adapter instance holding the three current bodies verbatim |
| S3 | `internal Func<CancellationToken, ItemViewer> ItemViewerFactory { get; set; }` | `QuickFiler/Controllers/QfcQueue.Tlp.cs` | plain initializer | the `ItemViewerQueue.Dequeue` method group |
| S4 | `internal Action<TableLayoutPanel, ItemViewer, int> ViewerRowPlacer { get; set; }` | `QuickFiler/Controllers/QfcQueue.Tlp.cs` | lazy `??=` property | the `AddViewerToTlp` method group |
| S5 | `internal Func<TableLayoutPanel, MailItem, int, Task<QfcItemGroup>> ItemGroupFactory { get; set; }` | `QuickFiler/Controllers/QfcQueue.Tlp.cs` | lazy `??=` property | the `AddAsync` method group |
| S6 | `internal Func<TableLayoutPanel, TableLayoutPanel> BackgroundTlpFactory { get; set; }` | `QuickFiler/Controllers/QfcQueue.Tlp.cs` | plain initializer | `template => template.Clone(name: "BackgroundTableLayout")` |

S4 and S5 are both required around viewer construction. A single coarse seam in place of S4 would
make `LoadControllersViewersAsync` coverable but would leave the production default it displaces —
`AddAsync` itself — permanently uncovered, relocating the untestable region instead of closing it. S6
is required for the background `TableLayoutPanel` clone; without it, everything in `EnqueueAsync`
after line 99 stays gated on unproven reflection-driven behaviour and no deterministic test of the
success path, the two catch paths, the counter bookkeeping, or the collection-changed notification is
possible.

**Why a new interface rather than the existing UI dispatcher abstraction.** The UI dispatcher
interface already present in the UtilitiesCS threading namespace must not be reused. Its
`InvokeAsync<TResult>(Func<TResult>)` and `InvokeAsync<TResult>(Func<Task<TResult>>)` members take no
`DispatcherPriority`, and its production adapter forwards them at the WPF default `Normal`. Routing
`QfcQueue` through it would silently promote two call sites from `ContextIdle` to `Normal`, changing
when background page construction runs relative to input and rendering — a behaviour change, not a
refactor. Its async body also omits the `await Task.Yield()` that the current `QfcQueue` member
performs. Adding priority overloads to it is rejected: on this target framework there are no default
interface members, so all of its implementers — one production adapter plus nine hand-written test
doubles across three test assemblies — would have to be edited for the benefit of one caller. A
per-shape delegate seam is also rejected: two of the three shapes are generic methods, and no `Func<>`
type can hold "for any `T`, a map from `Func<T>` to `Task<T>`".

Instead, `QuickFiler/Interfaces/IUiIdleDispatcher.cs` introduces a narrow `internal` interface with
`InvokeIdleAsync(Action)`, `InvokeIdleAsync<T>(Func<T>)`, and `InvokeIdleAsync<T>(Func<Task<T>>)`. Its
production adapter holds the three current bodies verbatim, including `ContextIdle` on all three and
the `await await` plus `await Task.Yield()` in the third. The three existing `UiIdleCallAsync` and
`UiIdleAsyncCallAsync` members are **kept as one-line forwards** to the seam, so none of the four
internal call sites is edited.

**Placement of the adapter.** The adapter class is declared inside
`QuickFiler/Controllers/QfcQueue.UiIdle.cs` rather than in a file of its own. The only existing folder
that would otherwise suit it has a space in its name, and a path containing a space is silently
dropped by the downstream change-footprint tooling, which would remove the file from the run's
footprint. Co-locating it with the only member that constructs it keeps the footprint intact and
keeps the file well under the size ceiling.

### Boundaries and invariants to preserve

- **One move monitor per owner.** S1 never creates, shares, or caches an instance across owners; the
  default is still produced once per `QfcQueue` instance by the existing field initializer. The
  load-bearing comment at `QuickFiler/Controllers/QfcQueue.cs` line 41 moves with nothing and stays
  verbatim. A `Func<IEmailMoveMonitor>` factory form is rejected because it converts a fixed per-owner
  instance into a per-call one, which is the exact failure mode that comment exists to prevent.
- **The `_moveMonitor` field name.** Six existing test methods reflect on it. It is not renamed, not
  removed, and not converted to an auto-property.
- **`ContextIdle` priority on all three UI marshalling shapes**, and the `await Task.Yield()` in the
  async shape.
- **Headless construction safety.** S2's default is lazy, so the static WPF dispatcher is still never
  read at construction time and `new QfcQueue(...)` remains safe in a test host.
- **No `#nullable enable` on relocated code.** Nullable enforcement is per-file opt-in in this
  repository. The relocated members are moved verbatim; adding the pragma would conscript them into
  `CS86xx`-as-error under the type-check gate for no benefit and would make the move unreviewable.
  The brand-new interface file may opt in, following the existing UtilitiesCS threading files.
- **The obsolete-API pragma** around the async-enumerable projection at
  `QuickFiler/Controllers/QfcQueue.Enqueue.cs` lines 173 and 196 is preserved as-is.
- **Region structure.** Both relocated blocks are complete region/endregion pairs and move intact; no
  region is left unbalanced on either side of the split.
- **No relocated member references a primary-constructor parameter.** The three parameters are
  referenced only by field initializers on the base part, which do not move. Whether a
  primary-constructor parameter is in scope in another partial part is unknown and is deliberately
  never exercised by this split.

### Dependencies or blocked work

- None blocking. The seam plan is fully reachable with the construction pattern the three existing
  test files already use.
- The separately promoted job-counter defect depends on this item only in the sense that it was
  discovered here; it is independently fixable and is not sequenced behind this work.

### Implementation strategy (what changes, not sequencing)

#### Files/modules to change

Created:

- `QuickFiler/Controllers/QfcQueue.Tlp.cs` — receives the whole Tlp Manipulation region from
  `QuickFiler/Controllers/QfcQueue.cs` lines 230-453, plus seams S3, S4, S5, S6. Predicted size about
  270 lines.
- `QuickFiler/Controllers/QfcQueue.UiIdle.cs` — receives the whole Helper Methods region from
  `QuickFiler/Controllers/QfcQueue.cs` lines 472-505, converts the three members to one-line forwards,
  and adds seam S2 and the production adapter class. Predicted size about 120 lines.
- `QuickFiler/Interfaces/IUiIdleDispatcher.cs` — the new narrow internal interface. Predicted size
  about 40 lines.
- `QuickFiler.Test/Controllers/QfcQueueEnqueueTests.cs` — the test-class part of the new MSTest
  suite, carrying every `[TestMethod]`.
- `QuickFiler.Test/Controllers/QfcQueueEnqueueTests.Harness.cs` — the harness part of the same
  suite, carrying the shared arrangement. The suite is one partial test class split across these
  two files because the test cases plus the shared harness do not fit in one file under the
  500-line ceiling.

Modified:

- `QuickFiler/Controllers/QfcQueue.cs` — loses the two relocated regions, gains seam S1. Predicted
  size about 260 lines, down from 507.
- `QuickFiler/Controllers/QfcQueue.Enqueue.cs` — three call-site substitutions at lines 91, 97-99 and
  177. Predicted size under 210 lines.
- `QuickFiler/QuickFiler.csproj` — three new `<Compile Include>` items.
- `QuickFiler.Test/QuickFiler.Test.csproj` — two new `<Compile Include>` items.

Both projects are legacy non-SDK projects with no implicit source glob, so a missing manifest entry
does not present as a missing-file error; it presents as "the seam member does not exist".

#### Functions/classes/CLI commands impacted

`QfcQueue.EnqueueAsync`, `QfcQueue.LoadControllersViewersAsync`, `QfcQueue.AddAsync`,
`QfcQueue.UiIdleCallAsync` (both overloads), `QfcQueue.UiIdleAsyncCallAsync`, and the relocated
members `TlpTemplate`, `ActivateTlpTemplate`, `TlpStates`, `AddViewerToTlp`, `AdjustTlp`,
`ChangeIterationSize`, `RenumberGroups`, `GrowEntry`. No CLI command is affected. No public API member
is added, removed, or re-signed: every new member is `internal`.

#### Data flow and validation changes

None on the success path. The only new validation is a guard in each seam setter rejecting `null` with
`ArgumentNullException`, which cannot fire in production because production never assigns a seam.

#### Error handling and logging updates

None. The two catch blocks and the single `logger.Error` call in `EnqueueAsync` are unchanged in
position, in condition, and in message. The log4net logger is a real static; with no appender
configured in the test host the error call is a no-op, so the general-exception branch needs no test
infrastructure.

#### Rollback/feature-flag considerations (if applicable)

Not applicable. No feature flag is introduced. Rollback is a revert of the branch; because the seams
are additive and defaulted, no persisted state or configuration migrates.

### Technical specifications (interfaces/contracts)

#### Inputs/outputs and formats

`IUiIdleDispatcher` (internal, declared in `QuickFiler/Interfaces/IUiIdleDispatcher.cs`):

- `Task InvokeIdleAsync(Action action)` — marshals `action` at `ContextIdle`.
- `Task<T> InvokeIdleAsync<T>(Func<T> func)` — marshals `func` at `ContextIdle`, returns its result.
- `Task<T> InvokeIdleAsync<T>(Func<Task<T>> func)` — marshals `func` at `ContextIdle`, awaits the
  inner task, yields, returns the result.

Each seam contract: the getter returns a non-null value before any assignment; the setter throws
`ArgumentNullException` on `null`; assignment replaces the boundary for that instance only and never
mutates process-global state.

#### Required configuration keys and defaults

None. There is no configuration surface; the defaults are compiled in.

#### Backward-compatibility expectations

Fully backward compatible. All six seams and the new interface are `internal`. The public surface of
`QfcQueue` is unchanged: no public member is added, removed, retyped, or resigned. Callers outside the
QuickFiler assembly cannot observe the change.

#### Performance constraints (latency/throughput/memory)

No measurable change is expected or permitted. The added indirections are one property read per call
site (S1, S3, S4, S5, S6) and one interface dispatch per UI marshalling call (S2), against work items
that already cross a WPF dispatcher boundary. The dispatcher priority must remain `ContextIdle` on all
three shapes precisely because promoting it to `Normal` would be a scheduling change; that is
enforced by AC2 rather than by a benchmark.

## Assumptions, Constraints, Dependencies

- Assumptions (environment, data, access):
  - The QuickFiler production assembly's `InternalsVisibleTo` attribute for the QuickFiler test
    assembly remains in place, making `internal` seams assertable from tests.
  - The preview language-version setting on the QuickFiler project remains, so the `??=` operator, the
    primary constructor, and collection expressions continue to compile. No language-version change is
    requested.
  - The QuickFiler test project continues to target .NET Framework 4.8.1 and to be discovered by the
    runner from its Debug output directory.

- Constraints (budget, performance, compatibility):
  - 500-line hard ceiling per production and test file. This is the binding constraint that makes the
    split mandatory.
  - Coverage floors: **>= 80% repository-wide line coverage and >= 90% for new code**, per CLAUDE.md
    and its stated Policy Compliance Order, which lists CLAUDE.md first and does not name the
    repository rule files under the dot-claude rules directory. A competing, lower-precedence
    statement of 85% line and 75% branch appears
    in the general unit-test rule file and the quality-tiers rule file; this item uses 80/90 and notes
    the divergence here once, factually, without adopting the other figures silently.
  - No tier-classification criterion is written anywhere in this document, because the
    quality-tiers manifest the rule file refers to does not exist at the repository root and no CI
    stage validates one. Any plan step that says "confirm the QuickFiler tier" cannot be satisfied.
  - MSTest, Moq, and FluentAssertions only. No new test dependency.
  - No temporary files, no filesystem, no network, no Outlook process in any test.

- External dependencies (services, libraries, releases): none added.

## Data / API / Config Impact

- User-facing or API changes: none. All new members are `internal`; the add-in's behaviour on the
  success path is unchanged.
- Data or migration considerations: none. No persisted format, schema, or settings key is touched.
- Logging/telemetry updates (if any): none. Existing log call sites, levels, and messages are
  preserved exactly.
- Compatibility notes (CLI flags, config schemas, versioning): none.

## Test Strategy

Seeded from issue:

- [x] Unit coverage areas: `EnqueueAsync` guard branches, both catch paths, `finally` bookkeeping and
      `CollectionChanged`; `LoadControllersViewersAsync` digit computation, carrier resolution,
      per-row factory invocation and `InitializeAsync` await.
- [x] Integration scenario to retest: high-confidence mode with more than one page, confirming
      background pages still render (manual, live Outlook).
- [x] Manual verification notes: production defaults must reproduce the previous construction
      expressions exactly, argument for argument, as `ItemControllerFactory` did.

- Regression tests to add or update: all new tests land in the two parts of the new partial test
  class, `QuickFiler.Test/Controllers/QfcQueueEnqueueTests.cs` carrying every `[TestMethod]` and
  `QuickFiler.Test/Controllers/QfcQueueEnqueueTests.Harness.cs` carrying the shared harness. No
  existing test file is edited. The three
  existing QfcQueue test files in the QuickFiler test project's Controllers folder must continue to
  pass unmodified; their reflection on the private move-monitor field is the reason S1 keeps the field.

- Unit tests for the fixed behavior and boundaries, using MSTest attributes, Moq doubles, and
  FluentAssertions:
  - Shared arrange helpers: a `NewQueue()` helper mirroring the existing coverage-expansion file's
    construction pattern; a hand-written synchronous `IUiIdleDispatcher` fake that invokes inline and
    returns a completed task (hand-written rather than Moq because Moq handles generic methods whose
    return type depends on the type parameter awkwardly, and because the repository already
    hand-writes nine such fakes for the other dispatcher interface); a recording `ItemGroupFactory`
    capturing `(tlp, mailItem, index)` and returning `new QfcItemGroup(mailItem)` with `ItemViewer`
    left null; a recording `ItemControllerFactory` capturing all nine arguments and returning a
    `Mock<IQfcItemController>` whose `InitializeAsync()` returns `Task.CompletedTask`; a
    `BackgroundTlpFactory` returning a sentinel `TableLayoutPanel` identifiable by reference so no
    reflection clone runs.
  - Seam-default tests: one per seam, asserting the default is non-null on first read, mirroring how
    the existing pure-paths test file treats `ItemControllerFactory`. For S3 additionally assert the
    default's `Method.Name` and `Method.DeclaringType` identify `ItemViewerQueue.Dequeue` **without
    invoking it**, because invoking it would read the static WPF dispatcher.
  - Success path, both catch paths, counter bookkeeping, collection-changed notification with and
    without a subscriber, strict verification of the move-monitor hook once per item, both arms of the
    `digits` ternary, a non-zero `start` case pinning the `items[i - start]` index mapping, the
    nine-argument controller pass-through, and the per-row `InitializeAsync` await.
  - `AddAsync` exercised directly with S5 left at its default and S3/S4 substituted, so the production
    body of `AddAsync` is executed rather than displaced.

- Edge cases and negative scenarios (invalid inputs, missing data, boundary values): null `items`;
  empty `items`; a `preScored` list that is null, that is empty, and that contains no carrier for the
  item; `digits` at totals of 9, 10, and 11; a non-zero `start`; a `CollectionChanged` raise with no
  subscriber attached; a `null` assignment to each seam setter.

- Error handling and logging verification: `OperationCanceledException` and a general exception each
  raised from the substituted `ItemGroupFactory`; in both cases `EnqueueAsync` must not propagate, the
  queue count must stay 0, and the running-jobs counter must return to 0. The general-exception case
  additionally proves the `logger.Error` line executes without test infrastructure.

- Coverage impact and targets for changed lines/modules: new code >= 90%; repository-wide line
  coverage >= 80%; the measured file-level line rate for `QuickFiler/Controllers/QfcQueue.Enqueue.cs`
  must rise strictly above the recorded baseline of 0.152941 and the rate for
  `QuickFiler/Controllers/QfcQueue.cs` must not fall below its recorded baseline of 0.503205 when the
  post-split parts are considered together. A post-change Cobertura artifact must be committed under
  `docs/features/active/2026-09-11-qfcqueue-enqueue-path-lacks-injectable-seams-871/evidence/qa-gates/`
  and the pre-change baseline under
  `docs/features/active/2026-09-11-qfcqueue-enqueue-path-lacks-injectable-seams-871/evidence/baseline/`.

- Toolchain commands to run (format, then analyzers, then nullable type-check, then test), restarting
  from the first on any failure or auto-fix, exactly as CLAUDE.md section CUT3 specifies:

```
dotnet tool run csharpier format .
dotnet tool run csharpier check .
msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true
msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true
vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /EnableCodeCoverage /InIsolation
```

  Do not add a solution-wide nullable property to the type-check command and do not substitute an
  incremental build target for the rebuild target; CLAUDE.md section C#1 records why both
  substitutions silently disable the gate.

- Manual validation steps (if required): with the add-in loaded in a live Outlook session, run
  high-confidence mode over a selection large enough to produce more than one page and confirm that
  background pages still render and that row numbering is unchanged. This is the only check that
  covers the production template clone, which no unit test executes.

## Write Set

- `QuickFiler/Controllers/QfcQueue.cs`
- `QuickFiler/Controllers/QfcQueue.Enqueue.cs`
- `QuickFiler/Controllers/QfcQueue.Tlp.cs`
- `QuickFiler/Controllers/QfcQueue.UiIdle.cs`
- `QuickFiler/Interfaces/IUiIdleDispatcher.cs`
- `QuickFiler/QuickFiler.csproj`
- `QuickFiler.Test/Controllers/QfcQueueEnqueueTests.cs`
- `QuickFiler.Test/Controllers/QfcQueueEnqueueTests.Harness.cs`
- `QuickFiler.Test/QuickFiler.Test.csproj`
- `docs/features/active/2026-09-11-qfcqueue-enqueue-path-lacks-injectable-seams-871/issue.md`
- `docs/features/active/2026-09-11-qfcqueue-enqueue-path-lacks-injectable-seams-871/spec.md`
- `docs/features/active/2026-09-11-qfcqueue-enqueue-path-lacks-injectable-seams-871/user-story.md`
- `docs/features/active/2026-09-11-qfcqueue-enqueue-path-lacks-injectable-seams-871/plan.2026-09-12T10-25.md`
- `docs/features/active/2026-09-11-qfcqueue-enqueue-path-lacks-injectable-seams-871/research/2026-09-12T10-35-qfcqueue-enqueue-seams-research.md`
- `docs/features/active/2026-09-11-qfcqueue-enqueue-path-lacks-injectable-seams-871/evidence/baseline/`
- `docs/features/active/2026-09-11-qfcqueue-enqueue-path-lacks-injectable-seams-871/evidence/qa-gates/`
- `docs/features/active/2026-09-11-qfcqueue-enqueue-path-lacks-injectable-seams-871/evidence/regression-testing/`

## Acceptance Criteria

- [ ] AC1 — S1 move-monitor seam. `QuickFiler/Controllers/QfcQueue.cs` declares
      `internal IEmailMoveMonitor MoveMonitor { get; set; }` whose getter returns the existing
      `_moveMonitor` field and whose setter rejects `null` with `ArgumentNullException`. The explicit
      `_moveMonitor` field and its load-bearing per-owner comment survive verbatim; the field is not
      renamed and not converted to an auto-property. Verified by a named test asserting the getter is
      non-null on a freshly constructed queue before any assignment, and by the six existing test
      methods in the QuickFiler test project that reflect on the field name continuing to pass
      unmodified.

- [ ] AC2 — S2 UI idle dispatcher seam and interface. `QuickFiler/Interfaces/IUiIdleDispatcher.cs`
      declares an `internal` interface with exactly the members `InvokeIdleAsync(Action)`,
      `InvokeIdleAsync<T>(Func<T>)` and `InvokeIdleAsync<T>(Func<Task<T>>)`. Its production adapter,
      declared in `QuickFiler/Controllers/QfcQueue.UiIdle.cs`, reproduces the three current bodies
      argument for argument: `DispatcherPriority.ContextIdle` is passed in all three, and the
      `Func<Task<T>>` body retains the double await and the `await Task.Yield()`. A named test asserts
      that a substituted dispatcher receives each of the three shapes and that the queue's public
      behaviour is unchanged. Verified additionally by a reviewer-checkable diff showing that no
      occurrence of `DispatcherPriority.Normal` is introduced anywhere in the Write Set and that the
      three `UiIdleCallAsync` and `UiIdleAsyncCallAsync` members remain present as one-line
      forwards. Three of the four internal call sites — `QuickFiler/Controllers/QfcQueue.cs` line 197
      and `QuickFiler/Controllers/QfcQueue.Enqueue.cs` lines 97 and 105 — are unedited apart from the
      S6 substitution named in AC6 at lines 97-99. The fourth, at
      `QuickFiler/Controllers/QfcQueue.cs` line 275 before the split, relocates into
      `QuickFiler/Controllers/QfcQueue.Tlp.cs` and has its inner argument substituted by AC4; its
      enclosing `UiIdleCallAsync` wrapper is unchanged, which is the property this criterion gates.

- [ ] AC3 — S3 viewer-factory seam. `QuickFiler/Controllers/QfcQueue.Tlp.cs` declares
      `internal Func<CancellationToken, ItemViewer> ItemViewerFactory { get; set; }` defaulted to the
      `ItemViewerQueue.Dequeue` method group, and `AddAsync` calls `ItemViewerFactory(_token)` in
      place of the previous direct call. A named test asserts, without invoking the delegate, that the
      default's `Method.Name` is `Dequeue` and its `Method.DeclaringType` is `ItemViewerQueue`, and a
      second named test asserts the substituted factory receives the queue's own cancellation token.

- [ ] AC4 — S4 row-placer seam. `QuickFiler/Controllers/QfcQueue.Tlp.cs` declares
      `internal Action<TableLayoutPanel, ItemViewer, int> ViewerRowPlacer { get; set; }` as a lazy
      `??=` property defaulted to the `AddViewerToTlp` method group, because an instance-method default
      cannot appear in an initializer. `AddAsync` calls `ViewerRowPlacer(tlp, viewer, indexNumber)`
      inside the same `UiIdleCallAsync` wrapper as before. Named tests assert the default is non-null
      on first read and that a substituted placer receives exactly the tuple
      `(tlp, viewer, indexNumber)` that the previous direct call passed.

- [ ] AC5 — S5 item-group seam. `QuickFiler/Controllers/QfcQueue.Tlp.cs` declares
      `internal Func<TableLayoutPanel, MailItem, int, Task<QfcItemGroup>> ItemGroupFactory { get; set; }`
      as a lazy `??=` property defaulted to the `AddAsync` method group, and
      `QuickFiler/Controllers/QfcQueue.Enqueue.cs` line 177 routes through it. Named tests assert the
      default is non-null on first read and that a substituted factory is invoked once per item with
      the `items[i - start]` mapping preserved for a non-zero `start`.

- [ ] AC6 — S6 background-template seam. `QuickFiler/Controllers/QfcQueue.Tlp.cs` declares
      `internal Func<TableLayoutPanel, TableLayoutPanel> BackgroundTlpFactory { get; set; }` defaulted
      to `template => template.Clone(name: "BackgroundTableLayout")`, reproducing the previous
      expression including the named argument, and `QuickFiler/Controllers/QfcQueue.Enqueue.cs` lines
      97-99 become a call to it inside the unchanged `UiIdleCallAsync` wrapper. A named test asserts
      that the `TableLayoutPanel` the substituted factory returns is the same reference that reaches
      the dequeued queue entry, proving the value flows through unmodified.

- [ ] AC7 — Seam contracts. The setter of each of `MoveMonitor`, `UiIdleDispatcher`,
      `ItemViewerFactory`, `ViewerRowPlacer`, `ItemGroupFactory` and `BackgroundTlpFactory` rejects
      `null` with `ArgumentNullException`, and the getter of each returns a non-null value on a
      freshly constructed queue before any assignment. Verified by a named test per seam named in
      this criterion. A freshly constructed queue
      still performs no read of the static WPF dispatcher and no Outlook COM call, verified by a named
      test that constructs a queue in the headless test host and asserts no exception is thrown.

- [ ] AC8 — File-size ceiling. After the change, the measured physical line count of every production
      file in the Write Set is strictly under 500:
      `QuickFiler/Controllers/QfcQueue.cs` (507 before the change),
      `QuickFiler/Controllers/QfcQueue.Enqueue.cs` (200 before the change),
      `QuickFiler/Controllers/QfcQueue.Tlp.cs`, `QuickFiler/Controllers/QfcQueue.UiIdle.cs`, and
      `QuickFiler/Interfaces/IUiIdleDispatcher.cs`. The two new test files
      `QuickFiler.Test/Controllers/QfcQueueEnqueueTests.cs` and
      `QuickFiler.Test/Controllers/QfcQueueEnqueueTests.Harness.cs` are each also strictly under 500.
      The suite is split across two parts of one partial test class because 28 test cases plus the
      shared harness do not fit in one file under the ceiling, and a second part file is the form
      this project already uses for that purpose. Verified by a
      re-measured line count per file recorded in the evidence qa-gates directory; the predicted
      figures in this document are arithmetic estimates and are not acceptable as evidence.

- [ ] AC9 — Project manifests. `QuickFiler/QuickFiler.csproj` contains a `<Compile Include>` item for
      each of `QuickFiler/Controllers/QfcQueue.Tlp.cs`, `QuickFiler/Controllers/QfcQueue.UiIdle.cs`
      and `QuickFiler/Interfaces/IUiIdleDispatcher.cs`, and
      `QuickFiler.Test/QuickFiler.Test.csproj` contains one for
      `QuickFiler.Test/Controllers/QfcQueueEnqueueTests.cs`, and a second for
      `QuickFiler.Test/Controllers/QfcQueueEnqueueTests.Harness.cs`. Because these are legacy non-SDK projects
      with no implicit source glob, verification is positive rather than by absence: a captured
      analyzer-gate build log shows a non-vacuous compilation, and at least one test in the new suite
      references a type declared in each new production file, so a missing manifest entry fails the
      build rather than passing silently.

- [ ] AC10 — Guard branches reachable. Named tests prove `EnqueueAsync` throws
      `ArgumentNullException` for a null `items` argument and `ArgumentException` for an empty one,
      using FluentAssertions async throw assertions.

- [ ] AC11 — Success path reachable. A named test enqueues one page with all seams substituted and
      asserts the queue count becomes 1, that the dequeued tuple carries the `TableLayoutPanel` the
      substituted `BackgroundTlpFactory` produced, and that the item groups appear in input order.

- [ ] AC12 — Catch paths reachable. Two named tests make the substituted `ItemGroupFactory` throw
      `OperationCanceledException` and `InvalidOperationException` respectively; in both cases
      `EnqueueAsync` does not propagate, the queue count stays 0, and the running-jobs counter returns
      to 0.

- [ ] AC13 — Counter bookkeeping observable. A named test captures the running-jobs count from inside
      a seam callback to prove the increment took effect mid-flight, and asserts the count is 0 after
      each of the three outcomes in AC11 and AC12, proving the `finally` decrement.

- [ ] AC14 — Collection-changed notification reachable on both arms. A named test subscribes to
      `CollectionChanged` and asserts exactly one event whose `Action` is
      `NotifyCollectionChangedAction.Add`; a second named test runs the same flow with no subscriber
      attached and asserts no exception, covering the other arm of the null-conditional invocation at
      `QuickFiler/Controllers/QfcQueue.Enqueue.cs` line 133.

- [ ] AC15 — Move-monitor hook verified. A named test assigns a
      `Mock<IEmailMoveMonitor>(MockBehavior.Strict)` through S1 and verifies `HookItem` is called
      exactly once per item with the item and a non-null `Action<MailItem>`. The captured delegate is
      not invoked by the test, because it is an async-void lambda.

- [ ] AC16 — `LoadControllersViewersAsync` branches reachable. Named tests cover both arms of the
      `digits` ternary at `QuickFiler/Controllers/QfcQueue.Enqueue.cs` line 166 at totals of 9, 10 and
      11; the carrier-found and carrier-absent outcomes of `ResolveCarriedHandler`; the nine-argument
      pass-through into `ItemControllerFactory` with every argument captured and asserted; and one
      awaited `InitializeAsync` call per row verified with `Times.Once`.

- [ ] AC17 — `AddAsync` body executed, not displaced. A named test leaves `ItemGroupFactory` at its
      default and substitutes only S3 and S4, calls `AddAsync` directly, and asserts the returned
      `QfcItemGroup` carries the supplied `MailItem`, that `ItemViewerFactory` received the queue's
      cancellation token, and that `ViewerRowPlacer` received `(tlp, viewer, indexNumber)`. This
      criterion exists because a coarse seam alone would relocate the uncovered region into `AddAsync`
      rather than close it.

- [ ] AC18 — No behaviour change on the success path. A reviewer-checkable diff of
      `QuickFiler/Controllers/QfcQueue.cs`, `QuickFiler/Controllers/QfcQueue.Enqueue.cs`,
      `QuickFiler/Controllers/QfcQueue.Tlp.cs` and `QuickFiler/Controllers/QfcQueue.UiIdle.cs` shows
      that: every relocated member moved verbatim apart from the named seam substitutions; no
      `#nullable enable` directive was added to any relocated code; the obsolete-API pragma pair at
      `QuickFiler/Controllers/QfcQueue.Enqueue.cs` lines 173 and 196 is intact; every region opens and
      closes on the same side of the split; the two catch blocks, the single `logger.Error` call and
      its message are unchanged; and no public member of `QfcQueue` was added, removed, retyped or
      resigned. The full toolchain passes in a single pass in the order format, analyzers, nullable
      type-check, test, with the logs captured under the evidence qa-gates directory.

- [ ] AC19 — Coverage floors met and improvement measured. New code reaches >= 90% line coverage and
      repository-wide line coverage remains >= 80%, per CLAUDE.md. The measured file-level line rate
      for `QuickFiler/Controllers/QfcQueue.Enqueue.cs` is strictly greater than the recorded baseline
      of 0.152941, and the combined rate for `QuickFiler/Controllers/QfcQueue.cs`,
      `QuickFiler/Controllers/QfcQueue.Tlp.cs` and `QuickFiler/Controllers/QfcQueue.UiIdle.cs` is not
      below the recorded baseline of 0.503205 for the pre-split file. Verified by a pre-change
      Cobertura artifact committed under the evidence baseline directory and a post-change Cobertura
      artifact committed under the evidence qa-gates directory, both named in the completion report.
      The competing 85% line and 75% branch figures stated in the general unit-test rule file and the
      quality-tiers rule file are recorded here as a known divergence and are not the governing
      figures for this item.

- [ ] AC20 — Residual unreachable regions recorded explicitly. A document committed under
      `docs/features/active/2026-09-11-qfcqueue-enqueue-path-lacks-injectable-seams-871/evidence/regression-testing/`
      names every region on the enqueue path that remains uncovered after this change, states why, and
      cites the post-change Cobertura artifact for each. At minimum it must address: the reflection
      driven control clone in the UtilitiesCS extensions namespace and the template setter that calls
      it, which remain uncovered because seam S6 deliberately bypasses them; the dead, entirely
      commented-out template-activation member; and any statement in `AddAsync` or
      `QuickFiler/Controllers/QfcQueue.Enqueue.cs` still reported at zero hits. Silently leaving a
      region uncovered without an entry in that document fails this criterion.

- [ ] AC21 — Out-of-scope defect untouched and not codified. The running-jobs counter increment in
      `QuickFiler/Controllers/QfcQueue.Enqueue.cs` remains outside the `try` block whose `finally`
      decrements it, exactly as it is today; the diff shows no change to that control flow. No test in
      either `QuickFiler.Test/Controllers/QfcQueueEnqueueTests.cs` or
      `QuickFiler.Test/Controllers/QfcQueueEnqueueTests.Harness.cs` asserts the counter-leak behaviour as
      correct, and no test makes the substituted `BackgroundTlpFactory` or the move-monitor hook throw
      in a way that would require the leak to be treated as expected. The separately promoted
      potential-bug entry for this defect is linked from the Rollout & Follow-up section of this
      document.

- [ ] AC22 — Untouched files stay untouched. The final diff contains no change to any file outside
      the Write Set. In particular the three existing QfcQueue test files in the QuickFiler test
      project's Controllers folder, the UtilitiesCS threading types, and the UtilitiesCS extension
      that performs the control clone are unmodified, and the full QuickFiler test suite passes with
      those files unchanged.

## Risks & Mitigations

- **Risk: the split relocates a member that depends on primary-constructor scoping.** Whether a
  primary-constructor parameter is visible in a different partial part is unverified and there is no
  in-repo precedent. *Mitigation:* the split boundary is chosen so the question never arises — the
  three parameters are referenced only by field initializers on the base part, which do not move. AC18
  makes the verbatim-move property reviewable.

- **Risk: converting the move monitor to an auto-property silently breaks six existing test methods.**
  Reflection on `_moveMonitor` would return null and the helper's non-null assertion would fail.
  *Mitigation:* AC1 pins the field's retention; AC22 requires those six existing test methods to pass
  unmodified.

- **Risk: reusing the existing UI dispatcher abstraction silently promotes two call sites from
  `ContextIdle` to `Normal`.** *Mitigation:* a new narrow interface is introduced instead; AC2
  requires `ContextIdle` on all three shapes and forbids introducing `DispatcherPriority.Normal`
  anywhere in the Write Set.

- **Risk: a coarse seam appears to close the gap while only relocating it.** *Mitigation:* S4 and S5
  are separate seams and AC17 requires the production body of `AddAsync` to be executed with
  `ItemGroupFactory` left at its default.

- **Risk: a new file compiles nowhere because its manifest entry is missing**, presenting as "the seam
  member does not exist" rather than as a missing-file error. *Mitigation:* AC9 requires positive
  verification through a non-vacuous build plus at least one test referencing a type from each new
  production file.

- **Risk: the production template clone is never exercised by any automated test**, so a regression in
  it would not be caught. *Mitigation:* AC20 records it as an accepted residual and the manual
  validation step in the Test Strategy covers it in a live session. This is a deliberate,
  documented limit, not an omission.

- **Risk: a later editor "fixes" the backtick formatting and corrupts the change footprint.**
  *Mitigation:* the formatting contract blockquote at the top of this document.

- Rollbacks: revert the branch. No persisted state, configuration, or schema migrates, so no
  compensating action is required.

## Rollout & Follow-up

- Release/rollout steps: merge to the integration branch for the parallel run, then to the default
  branch through the normal pull-request gates. No staged rollout, no flag, no migration.
- Post-fix monitoring or clean-up tasks:
  - Re-measure the line count of every production file in the Write Set after the edit and record the
    figures as AC8 evidence; the predictions in this document are estimates only.
  - Check off item #678's acceptance criterion AC20 and close out issue #727 sub-finding 4 once the
    post-change Cobertura artifact demonstrates the coverage movement required by AC19.
  - Track the separately promoted job-counter defect to its own fix. That fix widens the try block to
    enclose the hook loop and the template clone, which is a behaviour change and must not be made
    here.
- Links: issue #871; prior art item #678 and pull request #724; policy gap issue #727 sub-finding 4;
  the move-monitor per-owner invariant issues #731 and #620; the dispatcher synchronization-context
  hazard issues #781 and #784, which this item neither introduces nor mitigates because every
  production default is preserved; the research artifact in this feature folder's research directory,
  which is authoritative over the issue document wherever the two disagree.
