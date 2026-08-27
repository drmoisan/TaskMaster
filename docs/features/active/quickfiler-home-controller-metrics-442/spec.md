# quickfiler-home-controller-metrics (Spec)

- **Issue:** #442
- **Also Resolves:** #443, #451
- **Parent (optional):** epic `quickfiler-bug-family` (integration branch `epic/quickfiler-bug-family-integration`)
- **Owner:** drmoisan
- **Work Mode:** `full-bug`
- **Last Updated:** 2026-08-24T10-30
- **Status:** Prepared (preparation mode; execution deferred to `epic-orchestrator`)
- **Version:** 1.0

> **Acceptance-criteria authority.** Work mode is `full-bug`, so per
> `.claude/skills/acceptance-criteria-tracking/SKILL.md` this file is the **single authoritative
> acceptance-criteria source** for this feature. `user-story.md` is deliberately absent; its status
> is NONE. `issue.md` is the consolidated requirements record and carries no competing
> `## Acceptance Criteria` section. Check-off happens in this file only.

> **Timestamp provenance.** No shell clock is reachable from this session. The `Last Updated`
> minute component is derived from the session date (2026-08-24) and is known to be at or after
> `10-00`, the timestamp of the research artifact this spec consumes. The minute is approximate,
> not clock-read.

---

## Context

Three independently promoted, pre-existing defects in the QuickFiler home-controller metrics paths
are fixed together because they share the same code, emit into the same CSV artifact, and turn on
the same invariant: **when a metrics flush occurs relative to the controller lifecycle, and which
elapsed interval that flush records.**

| Issue | Title | Severity | Controller |
|---|---|---|---|
| [#442](https://github.com/drmoisan/TaskMaster/issues/442) | Qfc home-controller metrics never flushed | Medium | `QfcHomeController` |
| [#443](https://github.com/drmoisan/TaskMaster/issues/443) | Qfc home-controller metrics duration misread | Medium | `QfcHomeController` |
| [#451](https://github.com/drmoisan/TaskMaster/issues/451) | Efc home-controller metrics inert duration | Low-Medium | `EfcHomeController` |

The defects compound. #442 means no QuickFiler session-metrics row is ever written at all, which
today **masks** #443's wrong duration value. #451 is the same class of defect in the sibling EFC
controller, where the stopwatch is constructed but never started, so every EFC duration is `0`.

- **Observed environment:** Windows 11 Pro 10.0.26200; C# / .NET Framework 4.8 (net481) VSTO
  add-in; QuickFiler and EmailFiler launched from the TaskMaster ribbon against a live Outlook
  mailbox.
- **Impact and severity:** No user-visible functional regression in filing. The impact is
  data-quality: the QuickFiler performance-metrics feature produces **no output at all**, and the
  EmailFiler rows it does produce carry a permanently zero duration and a malformed column shape.
  Any tuning decision based on this data is unsound. Frequency is 100% — every session, every row.
- **First observed:** 2026-08-07, during read-only research for coverage children #433 and #437
  under epic #136. All three defects are pre-existing; none was introduced by a recent change.
  Both defects were deliberately not fixed inside a coverage-only child, whose NFR forbids
  behavior change, and were promoted instead.

---

## Repro & Evidence

### #442 — QuickFiler metrics are never written

1. Launch QuickFiler from the TaskMaster ribbon and file at least one batch of messages so a
   metrics line is produced.
2. Complete the session so `QfcHomeController.WriteMetricsAsync` runs.
3. Inspect the configured session-metrics file (`Globals.FS.Filenames.EmailSession`, default
   `99999EmailSession.csv` per `TaskMaster/Properties/Settings.Designer.cs:436-454`).

- **Expected:** the session-metrics line is appended to the file.
- **Actual:** no line is written. The line is added to the in-memory `_metrics`
  `BlockingCollection<string>` and remains there until the process exits.
- **Determinism:** always. The failure is silent; there is no log entry and no exception.

### #443 — QuickFiler duration is wrong when it is written

1. Launch QuickFiler and file messages for a measurable interval longer than 60 seconds.
2. Complete the session on the end-of-database path so the metrics write runs.
3. Inspect the recorded duration.

- **Expected:** the recorded duration equals the elapsed time of the completed filing interval.
- **Actual (currently masked by #442):** on the end-of-database path the recorded duration is
  approximately 0 seconds regardless of the real interval; where any duration is recorded, a
  90-second interval is written as `30` because only the 0-59 second component is taken.
- **Determinism:** the end-of-database path is deterministically wrong. The `MoveAndIterate` path
  is non-deterministic (see Root Cause Analysis, RC-4).

### #451 — EmailFiler duration is permanently zero and the row is malformed

1. Use EmailFiler to move at least one message.
2. Inspect the same session-metrics file.

- **Expected:** a well-formed 12-field CSV row with a real elapsed duration.
- **Actual:** an 11-field row (the `ToRecipientsName` and `SenderName` values are emitted
  concatenated with no separator) whose duration column is always `0`.
- **Determinism:** always.

### Static evidence

All file:line evidence is recorded in
`docs/features/active/quickfiler-home-controller-metrics-442/research/quickfiler-home-controller-metrics.research.2026-08-24T10-00.md`,
which verified every claim in this section against source. The three promoted potential documents
under `docs/features/potential/promoted/` carry the original discovery evidence.

The single pinning assertion that documents defect #451/4 in the test suite today is
`QuickFiler.Test/Controllers/EfcHomeControllerMetricsTests.cs:56-60`, whose expected string
contains the substring `RecipientSender`.

---

## Scope & Non-Goals

### In scope

1. **#442** — replace the never-draining producer/consumer machinery in `QfcHomeController` with a
   direct awaited append through an injectable writer seam, so a QuickFiler session actually writes
   its metrics row.
2. **#443** — read the correct stopwatch (`_stopWatchMoved`) and the correct component
   (`TotalSeconds`) in `QfcHomeController.Metrics.cs`, and align the reconstructed calendar-span
   start time with the untruncated elapsed value.
3. **#451** — start the EFC stopwatch at both construction sites; widen the elapsed value from
   `int` to `double` and read `TotalSeconds`; add the missing CSV field separator; apply
   `xComma` sanitization to every interpolated free-text field; make the `TryBeginExecuteMoves`
   re-entrancy guard atomic; implement the interface-mandated single-argument
   `QuickFileMetrics_WRITE(string)` overload.
4. **Culture invariance** of the six numeric `##0` / `##0.00` format sites in the two owned metrics
   files. This is in scope because it is a direct consequence of the other fixes, not tidiness: #442
   is what causes the QFC CSV to be written for the first time, and #451's `TotalSeconds` widening
   is what gives the EFC duration a fractional part. Both changes *increase* the surface on which a
   comma decimal separator splits one field into two and corrupts every row. Shipping #442 and #443
   while leaving that in place would defeat the feature's stated purpose on any non-invariant
   machine. Every CSV-corrupting numeric site is in an owned file.
5. **Defensive null filtering** of the diagnostics array in `WriteMetricsAsync` before the write
   (see Cross-Feature Notes, CFN-2).
6. **Deliberate update of the tests that pin the defective behavior**, named in Test Strategy.

### Out of scope / non-goals

- **The `MoveAndIterate` stopwatch race (#443, partial).** The end-of-database path is fully
  fixable inside owned files; the `MoveAndIterate` path is not. It is knowingly left unfixed here
  and recorded as CFN-1. Rationale in RC-4 and Cross-Feature Notes.
- **The `"hh:mm"` 12-hour-without-designator defect.** All three sites are in owned files, so it is
  *possible*, but it is a **content** change to an existing column rather than a **shape** change:
  it does not corrupt row structure and does not block this feature's purpose. It would break three
  currently passing tests on their asserted literals, and none of the three issues lists it as an
  acceptance criterion (#443 mentions it only under "fix together or split as judged"). This
  feature already changes the duration columns' values and the EFC row's column count; a third
  simultaneous change to the timestamp column enlarges the reconciliation a downstream spreadsheet
  owner must perform in one step. Recorded as CFN-4 and promoted to its own issue.
- **The trailing-`null` array-sizing defect at `QfcCollectionController.cs:2284`.** Not an owned
  file. Mitigated defensively here; recorded as CFN-2.
- **The un-awaited dispatcher continuation at `QfcFormController.EventHandlers.cs:228-231`.** Not
  an owned file. Recorded as CFN-3.
- **Coverage uplift for these files.** Tracked separately under #433 and #437. This feature is
  behavior-only; it must not reduce coverage on the lines it changes.
- **Abstracting `Stopwatch` behind `TimeProvider`.** Not achievable inside owned files (see
  Assumptions, A-4).
- **Any change to the `HH:mm:ss` precision of the EFC `SentDate` column.** Not a defect; changing
  it would be an unforced column-content change.

### Explicitly excluded files

`QuickFiler/Controllers/QfcHomeController.Iteration.cs` (feature 446),
`QuickFiler/Controllers/QfcFormController.EventHandlers.cs` (feature 446),
`QuickFiler/Controllers/QfcCollectionController.cs` (feature 468),
`QuickFiler/Controllers/EfcFormController.cs` (feature 464),
`QuickFiler/QuickFiler.csproj`, `QuickFiler.Test/QuickFiler.Test.csproj`,
`QuickFiler/Interfaces/IFilerHomeController.cs`,
`QuickFiler/Controllers/IQfcHomeController.cs`,
`QuickFiler/Controllers/EfcHomeControllerDependencies.cs`.

Reading these is expected; writing them is forbidden.

---

## Root Cause Analysis

### RC-1 (#442) — the consumer can never start, and the queue can never drain

Confirmed, not hypothesized. Four independent failures each sufficient to prevent any write:

- `QfcHomeController.Metrics.cs:226` guards consumer scheduling with
  `Interlocked.CompareExchange(ref _metricsConsumers, 0, 2) == 2`. `_metricsConsumers` is
  initialized to `0` (`QfcHomeController.cs:356`) and is only ever **decremented**
  (`QfcHomeController.Metrics.cs:228`, `QfcHomeController.cs:366`). A repo-wide search returns
  exactly those four sites: **no increment exists**, so the guard can never be true.
- `QfcHomeController.Metrics.cs:229-230` constructs `new System.Timers.Timer(2000)` into a
  **local**, subscribes `TimedConsumerAsync` to `Elapsed`, and never calls `Start()`, never sets
  `Enabled`, and never disposes it. The local leaves scope at `:231`.
- `_metrics.CompleteAdding()` is called nowhere in the repository, so the
  `GetConsumingEnumerable()` drain at `QfcHomeController.cs:367` would block the consumer thread
  indefinitely even if the consumer did run.
- `_fileName` (`QfcHomeController.cs:358`) is `static` on an instance-scoped concern, has exactly
  one write (`Metrics.cs:153`) and zero reads. `_lockObject` (`QfcHomeController.cs:357`) is
  likewise `static` and unreferenced anywhere in the repository.

**Affected components:** `QuickFiler/Controllers/QfcHomeController.cs:353-386`,
`QuickFiler/Controllers/QfcHomeController.Metrics.cs:153-154, 190-232`.

**Scope note:** #442 is **QFC-only**. The EFC path already writes synchronously through
`_dependencies.MetricsLineWriter` (`EfcHomeController.Metrics.cs:51`).

### RC-2 (#443) — the wrong stopwatch is read

`SwapStopWatch()` (`QfcHomeController.Iteration.cs:79-84`) assigns `_stopWatchMoved = _stopWatch`
and then replaces `_stopWatch` with a freshly started zero. **Post-swap invariant:**
`_stopWatchMoved` holds the completed interval; `_stopWatch` reads approximately zero. At any
metrics write that happens after a swap, `_stopWatchMoved` is the correct read.

`QfcHomeController.Metrics.cs:121` reads `StopWatch.Elapsed` (that is, `_stopWatch`). The
commented-out `:120` shows it previously read `_stopWatchMoved`, so the two writers agreed before a
regression: the sibling `QuickFileMetrics_WRITE` still reads `_stopWatchMoved` (`:42`, `:44`).

### RC-3 (#443, #451) — `TimeSpan.Seconds` is the 0-59 component, not the interval

`QfcHomeController.Metrics.cs:42`, `QfcHomeController.Metrics.cs:121`, and
`EfcHomeController.Metrics.cs:23` all use `.Seconds`. A 90-second interval records as `30`.
`QfcHomeController.Metrics.cs:123` compounds this by reconstructing the calendar span from the
truncated integer (`new TimeSpan(0, 0, 0, (int)Duration)`) while `:44` does the equivalent
correctly from the full `Elapsed`, so the appointment span and the CSV duration disagree.

### RC-4 (#443) — the `MoveAndIterate` path races, and the race is not reachable from owned files

`QfcFormController.EventHandlers.cs:154-177` starts `BackGroundMoveAsync()` at `:157` **without
awaiting it** and awaits `LoadUiFromQueue()` at `:161`, which performs the swap at `:142`; the move
task is awaited only at `:175`. The swap and the metrics write are therefore concurrent and
unordered, and **neither field is deterministically correct**:

| Interleaving | `_stopWatchMoved` holds | `_stopWatch` holds |
|---|---|---|
| swap completes before the write | the current interval (correct) | approximately 0 |
| write completes before the swap | the previous group's interval (stale) | the current interval (correct) |

By contrast the end-of-database branch (`EventHandlers.cs:190-192`) executes strictly in order —
`CacheMoveObjects()` then `SwapStopWatch()` then `await BackGroundMoveAsync()` — with no
interleaving, so `_stopWatchMoved` is deterministically correct there.

Ordering the swap relative to the write requires editing `QfcFormController.EventHandlers.cs` or
`QfcHomeController.Iteration.cs`, both owned by feature 446. Three owned-file workarounds were
evaluated and all fail:

- *Snapshot in a property setter* — fixes which value is captured, not when the capture happens
  relative to the write. The race is unchanged, and it breaks four tests that set the field by
  reflection.
- *Have `WriteMetricsAsync` call `SwapStopWatch()` itself* — the write fires at `ContextIdle`,
  later than the true end of the interaction interval, and a swap that won the race would zero the
  interval for the next write. It converts one race into two.
- *Capture at `CacheMoveObjects()` time* — semantically correct, but the method is on
  `QfcCollectionController` (468) and both call sites are in `EventHandlers.cs` (446).

**Disposition: knowingly left unfixed here.** See CFN-1.

### RC-5 (#451) — the EFC stopwatch is never started

`_stopWatch` is constructed at `EfcHomeController.cs:76` and `EfcHomeController.cs:225` and
**`Start()` is called nowhere in the `EfcHomeController` family**, so
`EfcHomeController.Metrics.cs:23` always evaluates to `0`. The sibling pattern to mirror is
`QfcHomeController.cs:267-268` and `:315-316`, which construct and start.

### RC-6 (#451) — non-atomic re-entrancy guard

`EfcHomeController.ExecuteMoves.cs:48-57` reads then writes `_isExecuting`
(`private volatile bool`, `EfcHomeController.cs:389`). `volatile` constrains memory ordering only;
it does not make read-then-write atomic, so two callers can both observe "not executing" and both
proceed. `Interlocked.CompareExchange` is the correct primitive.

### RC-7 (#451) — malformed and unsanitized CSV row

`EfcHomeController.Metrics.cs:80-81` omits the field separator between `{itemInfo.ToRecipientsName}`
(end of `:80`) and `{itemInfo.SenderName}` (start of `:81`), producing an 11-field row where the
QFC writer produces 12. `:79` applies `QfcCollectionController.xComma(...)` to `Subject` only;
`ToRecipientsName`, `SenderName`, and `selectedFolder` are interpolated raw, so a comma in any of
them corrupts the row shape. The QFC writer sanitizes all four
(`QfcCollectionController.cs:2311-2322`).

### RC-8 (#451) — an interface member that cannot succeed

`EfcHomeController.Metrics.cs:26-29` declares `public void QuickFileMetrics_WRITE(string filename)`
whose entire body is `throw new NotImplementedException();`.
**`QuickFiler/Interfaces/IFilerHomeController.cs:41` declares
`void QuickFileMetrics_WRITE(string filename);`, so the member is mandated by the interface and the
interface file is not owned. #451's "either implement it or remove it" is settled: it must be
implemented. Removal is not an available option.**

### RC-9 (all) — culture-dependent numeric formatting

`ToString("##0")` and `ToString("##0.00")` with no `IFormatProvider` bind to
`CultureInfo.CurrentCulture`. On any culture using `,` as the decimal separator (de-DE, fr-FR,
es-ES, pt-BR and others), `durationMinutesText` renders as `2,00` and splits one CSV field into
two. Affected owned sites: `QfcHomeController.Metrics.cs:53, 56, 132, 135` and
`EfcHomeController.Metrics.cs:73, 74`. The remaining culture-sensitive sites are either date
formats built from `/` and `:` literals (culture-stable under a custom format string) or the
appointment body rather than the CSV.

---

## Proposed Fix

### Design summary (what changes where)

| # | Change | File | Defect |
|---|---|---|---|
| 1 | Add `internal Func<string, string[], string, CancellationToken, Task> MetricsFileWriter { get; set; } = FileIO2.WriteTextFileAsync;` | `QfcHomeController.Metrics.cs` | #442 |
| 2 | Replace `Metrics.cs:153-154` with a null/whitespace-filtered `await MetricsFileWriter(filename, lines, myDocuments, CancellationToken.None)` | `QfcHomeController.Metrics.cs` | #442, CFN-2 |
| 3 | Delete `NonBlockingProducer` (both overloads) and the unreachable consumer-scheduling block (`Metrics.cs:190-232`) | `QfcHomeController.Metrics.cs` | #442 |
| 4 | Delete `_metrics`, `_metricsConsumers`, `_lockObject`, `_fileName` (`:353-358`) and `TimedConsumerAsync` (`:362-386`) | `QfcHomeController.cs` | #442 |
| 5 | Remove the then-unused `using System.Collections.Concurrent;` (`:2`) and `using System.Timers;` (`:11`) after verifying no other member of the partial consumes them | `QfcHomeController.cs` | #442 |
| 6 | `:121` becomes `Duration = _stopWatchMoved.Elapsed.TotalSeconds;` | `QfcHomeController.Metrics.cs` | #443 |
| 7 | `:123` becomes `OlStartTime = OlEndTime.Subtract(_stopWatchMoved.Elapsed);` | `QfcHomeController.Metrics.cs` | #443 |
| 8 | `:42` becomes `double duration = _stopWatchMoved.Elapsed.TotalSeconds;` | `QfcHomeController.Metrics.cs` | #443 |
| 9 | `:53, :56, :132, :135` gain `CultureInfo.InvariantCulture` | `QfcHomeController.Metrics.cs` | RC-9 |
| 10 | `:76` and `:225` become `_stopWatch = Stopwatch.StartNew();` | `EfcHomeController.cs` | #451/1 |
| 11 | `:23` becomes `.TotalSeconds` | `EfcHomeController.Metrics.cs` | #451/2 |
| 12 | `:35` and `:57` widen `int elapsedSeconds` to `double elapsedSeconds` | `EfcHomeController.Metrics.cs` | #451/2 |
| 13 | Insert the missing `,` between `ToRecipientsName` and `SenderName` (`:80-81`) | `EfcHomeController.Metrics.cs` | #451/4 |
| 14 | Wrap `ToRecipientsName`, `SenderName`, and `selectedFolder` in `QfcCollectionController.xComma(...)` | `EfcHomeController.Metrics.cs` | #451/5 |
| 15 | `:73-74` gain `CultureInfo.InvariantCulture` | `EfcHomeController.Metrics.cs` | RC-9 |
| 16 | Implement `QuickFileMetrics_WRITE(string filename)` as guarded delegation to the 3-argument overload | `EfcHomeController.Metrics.cs` | #451/6 |
| 17 | Replace the `volatile` check-then-set with `Interlocked.CompareExchange`; `_isExecuting` becomes `private int`; `ResetExecuteMovesState` becomes `Interlocked.Exchange(ref _isExecuting, 0)` | `EfcHomeController.ExecuteMoves.cs`, `EfcHomeController.cs:389` | #451/3 |

`Stopwatch.StartNew()` is preferred over the two-statement construct-then-start pattern at the two
EFC sites: it is atomic, so no future edit can separate construction from start and re-introduce
exactly the defect being fixed, and it saves two lines per site. `System.Diagnostics` is already
imported at `EfcHomeController.cs:3`.

The single-argument overload (change 16) derives its two missing arguments from state the
controller already holds — `selectedFolder` from `_formController.SelectedFolder`, `moved` from the
owned `internal static SelectMoveMetricsItems(...)` (`ExecuteMoves.cs:111`) — mirroring
`ExecuteMovesCoreAsync` (`ExecuteMoves.cs:66-72`), and returns early when `_formController`,
`DataModel`, or `DataModel.Mail` is absent. This adds no seam and touches no forbidden file. The
rejected alternative — throwing `NotSupportedException` with a message — satisfies "no bare
`NotImplementedException`" literally but leaves an interface member that can never succeed.

### Boundaries and invariants to preserve

**Flush invariant (the central invariant of #442).**

> For every invocation of `WriteMetricsAsync(filename)` that passes the `MyDocuments` lookup at
> `QfcHomeController.Metrics.cs:114`, the complete set of diagnostic lines produced for that
> invocation must have been handed to the writer seam, and the writer's returned `Task` must have
> completed, **before the `Task` returned by `WriteMetricsAsync` completes**. After that method
> returns, no metrics data may remain in controller state, and no part of the flush may be
> scheduled onto a timer, a background consumer, or `Cleanup()`.

Why this instant, precisely: `WriteMetricsAsync` is invoked at
`QfcFormController.EventHandlers.cs:229` inside `BackGroundMoveAsync`, which runs before
`ActionCancelAsync` (`EventHandlers.cs:84`) invokes `Cleanup()`.
`QfcHomeController.Cleanup()` sets `Globals = null` at `QfcHomeController.cs:391`, after which
`Globals.FS.SpecialFolders` and `Globals.FS.Filenames.EmailSession` are both unreachable. Satisfying
the invariant above means the flush is complete while `Globals` is still live, and it removes any
need to add flush obligations to `Cleanup()` — which is `void` (mandated by
`IFilerHomeController.cs:17`) and therefore cannot await, and which runs after
`_parent.TokenSource.Cancel()` at `EventHandlers.cs:86`.

Corollary, and the reason the write must not use `this.Token`: because the dispatcher continuation
at `EventHandlers.cs:228-231` is not awaited to completion (CFN-3), a session cancellation can be
raised while the write is still in flight. The writer must therefore receive
`CancellationToken.None` / `default`, never `Token`. The existing precedent at
`QfcHomeController.cs:376` already passes `default` and is correct.

Other invariants to preserve:

- `_stopWatch` and `_stopWatchMoved` must remain **fields**, not properties.
  `SwapStopWatch_ExecutesCorrectly` (`QfcHomeControllerIterationTests.cs:435-458`),
  `StopWatch_PropertyWorksCorrectly` (`QfcHomeControllerPropertyTests.cs:232-252`), and three tests
  in `QfcHomeControllerMetricsTests.cs` set them by field reflection.
- No public signature on `WriteMetricsAsync(string)`, `QuickFileMetrics_WRITE(string)`, or
  `Stopwatch StopWatch { get; }` may change. The two widened members (`:35`, `:57`) are `internal`;
  `QuickFiler` grants `InternalsVisibleTo("QuickFiler.Test")` (`QfcHomeController.cs:18`), so the
  only external consumer is the test project. **No public API breaks.**
- The QFC emitted column order and its 12-field shape are unchanged. The EFC row moves from 11 to
  12 fields, converging on the QFC shape.

### Dependencies or blocked work

None blocking. The four cross-feature notes are advisory to sibling children and do not gate this
feature. Execution is sequenced by `epic-orchestrator` under
`epic/quickfiler-bug-family-integration`.

### Implementation strategy (what changes, not sequencing)

#### Files/modules to change

Production (owned, exhaustive):

- `QuickFiler/Controllers/QfcHomeController.cs`
- `QuickFiler/Controllers/QfcHomeController.Metrics.cs`
- `QuickFiler/Controllers/EfcHomeController.cs`
- `QuickFiler/Controllers/EfcHomeController.Metrics.cs`
- `QuickFiler/Controllers/EfcHomeController.ExecuteMoves.cs`

Tests (both already carry `Compile Include` entries at `QuickFiler.Test/QuickFiler.Test.csproj:110`
and `:133`, so no project-file edit is expected or permitted):

- `QuickFiler.Test/Controllers/QfcHomeControllerMetricsTests.cs`
- `QuickFiler.Test/Controllers/EfcHomeControllerMetricsTests.cs`

**No new `.cs` file may be created.** `QuickFiler.csproj` and `QuickFiler.Test.csproj` are legacy
non-SDK projects with explicit `<Compile Include>` entries and are not owned, so a new file cannot
be registered. All new production and test code lands in the seven files above.

#### Functions/classes/CLI commands impacted

`QfcHomeController.WriteMetricsAsync`, `QfcHomeController.QuickFileMetrics_WRITE`,
`QfcHomeController.NonBlockingProducer` (deleted), `QfcHomeController.TimedConsumerAsync`
(deleted), `EfcHomeController.QuickFileMetrics_WRITE` (all three overloads),
`EfcHomeController.BuildQuickFileMetricLines`, `EfcHomeController.TryBeginExecuteMoves`,
`EfcHomeController.ResetExecuteMovesState`, and the two EFC constructors/initializers at
`EfcHomeController.cs:76` and `:225`. No CLI surface exists.

#### Data flow and validation changes

`WriteMetricsAsync` currently ends `_fileName = filename; await NonBlockingProducer(strOutput, Token);`
with `strOutput` flowing into a queue nothing drains. After the change, `strOutput` is filtered for
`null`/whitespace entries and passed directly to `MetricsFileWriter`, whose default is
`FileIO2.WriteTextFileAsync`. The `MyDocuments` guard at `:114` is unchanged and remains the single
validation gate: when it fails, no write occurs and no exception is raised.

On the EFC side the elapsed value flows as a `double` parameter through
`QuickFileMetrics_WRITE(3-arg)` to `QuickFileMetrics_WRITE(4-arg)` to the pure static
`BuildQuickFileMetricLines`, which retains its existing `moved is null || moved.Count == 0` guard
and its existing `dataLines.Length == 0` guard.

#### Error handling and logging updates

No new logging is introduced. `FileIO2.WriteTextFileAsync` (`FileIO2.cs:50-89`) already retries
`IOException` internally; that behavior is inherited unchanged. No broad `catch` is added. The
guarded early return in the newly implemented `QuickFileMetrics_WRITE(string)` overload follows the
existing precedent at `EfcHomeController.Metrics.cs:18-21`: absent prerequisites produce a silent
no-op, not an exception, because the member is invoked from an interface contract with no return
channel.

#### Rollback/feature-flag considerations

No feature flag. The change is a behavior fix to a write-only diagnostic artifact with no in-repo
consumer (see Data / API / Config Impact), so rollback is a plain revert of the commit range.

### Technical specifications (interfaces/contracts)

#### Inputs/outputs and formats

New internal seam on `QfcHomeController`, declared in the owned `QfcHomeController.Metrics.cs` and
mirroring the existing EFC precedent at `EfcHomeControllerDependencies.cs:78`:

```
internal Func<string, string[], string, CancellationToken, Task> MetricsFileWriter { get; set; }
    = FileIO2.WriteTextFileAsync;
```

Parameters in order: filename, lines, folder root, cancellation token.

Widened EFC signatures (both `internal`, so no public break):

- `internal void QuickFileMetrics_WRITE(string filename, string selectedFolder, List<MailItemHelper> moved, double elapsedSeconds)`
- `internal static string[] BuildQuickFileMetricLines(DateTime currentDateTime, double elapsedSeconds, string selectedFolder, List<MailItemHelper> moved)`

EFC emitted row, after the fix (12 fields, matching the QFC shape):

```
<date MM/dd/yyyy>,<time hh:mm>,<xComma(Subject)>,SingleSorted,<durationText>,<durationMinutesText>,
<xComma(ToRecipientsName)>,<xComma(SenderName)>,Email,<xComma(selectedFolder)>,
<SentDate MM/dd/yyyy>,<SentDate HH:mm:ss>
```

#### Required configuration keys and defaults

None added. The metrics filename continues to come from `Globals.FS.Filenames.EmailSession`
(default `99999EmailSession.csv`, `TaskMaster/Properties/Settings.Designer.cs:436-454`).

#### Backward-compatibility expectations

**Decision: the emitted-CSV change is accepted without a compatibility shim, on evidence.**

A repository-wide search for `EmailSession` across `*.cs`, `*.py`, `*.ps1`, `*.ipynb`, `*.R`, and
`*.sql` returns exactly six files: three settings-plumbing declarations
(`TaskMaster/Properties/Settings.Designer.cs:436-454`,
`TaskMaster/AppGlobals/AppStagingFilenames.cs:85-93`,
`UtilitiesCS/Interfaces/IGlobals/IAppStagingFilenames.cs:10`) and three **writers**
(`QfcHomeController.cs:373`, `QfcFormController.EventHandlers.cs:229`,
`EfcHomeController.ExecuteMoves.cs:141`). **There is no parser, no reader, and no schema consumer
anywhere in the repository.** The artifact is write-only from the codebase's perspective.

This answers #451's stated concern about "potential downstream consumers" with evidence rather than
assumption. The residual risk is confined to a human-maintained spreadsheet outside the repository
whose EFC column count shifts from 11 to 12. That is stated explicitly in the PR body rather than
gated on.

Three content changes accompany the shape change and are equally deliberate: EFC durations become
non-zero, all durations become untruncated totals, and numeric fields render invariantly.

#### Performance constraints (latency/throughput/memory)

The append moves from a (never-running) timer thread onto the `ContextIdle` dispatcher
continuation. `FileIO2.WriteTextFileAsync` retries `IOException` up to 100 times with
`await Task.Delay(100)` (`FileIO2.cs:75-87`), so a locked file can keep that continuation pending
for up to approximately 10 seconds. The continuation is asynchronous, so the UI message pump is not
blocked; only the dispatcher operation remains pending. This is the accepted cost of the design and
is recorded in Risks & Mitigations as R-1.

Memory falls: a `BlockingCollection<string>` that accumulates for the process lifetime is removed.
No throughput requirement applies to a once-per-OK-click diagnostic append.

---

## Assumptions, Constraints, Dependencies

- **A-1 — the producer/consumer machinery is replaced, not repaired.** The queue exists to batch
  appends across a 2-second window, but the array reaching `WriteMetricsAsync` is already the
  complete batch for one OK-click (`GetMoveDiagnostics` returns one line per moved item) and
  `FileIO2.WriteTextFileAsync` opens the file once per call and writes all lines inside one
  `using`. The batching is therefore a second-order optimization over an already-batched call, for
  which the General Code Change Policy demands a demonstrated need; none is documented. Repairing
  it instead would add four lifecycle obligations (a paired increment, a started and disposed
  timer, a `CompleteAdding()` and a consumer join) to a synchronous `void Cleanup()` running under
  an already-cancelled token, and would require timer-driven tests to recover the determinism the
  seam design gets for free. Deleting it is also **net-negative on lines**: `QfcHomeController.cs`
  falls from 487 to approximately 454 and `QfcHomeController.Metrics.cs` from 234 to approximately
  204, which resolves rather than consumes the 500-line-cap headroom problem. Since the queue has
  never drained once in production, no behavior is regressed: the change moves from "zero writes"
  to "one append per OK-click", not from "batched writes" to "unbatched writes".
- **A-2 — no new `.cs` file, production or test.** Both `.csproj` files are legacy non-SDK projects
  with explicit `<Compile Include>` entries and neither is owned.
- **A-3 — no new EFC dependency seam.** `EfcHomeControllerDependencies.cs` is not owned, so #451
  work must use the existing `MetricsNowFactory` and `MetricsLineWriter` seams plus method
  parameters.
- **A-4 — no `Stopwatch` abstraction.** `System.Diagnostics.Stopwatch` reads
  `Stopwatch.GetTimestamp()` directly and cannot be moved by a `FakeTimeProvider`. A
  `TimeProvider.GetTimestamp()` / `GetElapsedTime()` interval abstraction is *available* on net481
  in this repository (demonstrated by compiling production code at
  `QfcStreamingDequeueConfidenceGate.cs:102,110` against `Microsoft.Bcl.TimeProvider` 10.0.11), but
  adopting it would require editing `SwapStopWatch()` (`Iteration.cs:79`, feature 446), two
  forbidden stopwatch-construction sites, and the public `Stopwatch StopWatch { get; }` member of
  `IFilerHomeController` (`IFilerHomeController.cs:27`). **Not achievable inside owned files;
  out of scope.** Determinism is instead obtained from the existing parameter seam on the EFC side
  and from reflection-injected stopwatches plus `Stopwatch.IsRunning` on the QFC side.
- **A-5 — net481 language constraints.** No `init` accessors, no `record`, no `record struct`, no
  `IsExternalInit`. Any new value type must be a plain `readonly struct` or a class with a
  constructor. The recommended change set introduces no new type, so this constraint is recorded
  but not binding.
- **A-6 — `TimeProvider` and `FakeTimeProvider` are already referenced.**
  `Microsoft.Bcl.TimeProvider` 10.0.11 at `QuickFiler.csproj:66-67` and
  `QuickFiler.Test.csproj:206-207`; `Microsoft.Extensions.TimeProvider.Testing` 10.9.0 at
  `QuickFiler.Test.csproj:255-256`. No `.csproj` edit is needed, which matters because neither is
  owned.
- **A-7 — determinism rules.** `.claude/rules/general-unit-test.md` prohibits `Thread.Sleep`,
  `Task.Delay`, real wall-clock waits, and `Date`/clock reads outside the clock interface in test
  code, and the repository prohibits temporary files in tests outright. Every assertion in this
  feature must be reachable without any of them.
- **A-8 — file-size cap.** 500 lines for production and test files
  (`.claude/rules/general-code-change.md`). `QfcHomeControllerMetricsTests.cs` is 421 lines with
  approximately 79 lines of headroom; new QFC test methods must reuse
  `BuildLooseMetricsController()` (`:259-310`) and stay short.
  `EfcHomeControllerMetricsTests.cs` is 244 lines and is comfortable.
- **A-9 — toolchain.** MSTest, Moq, FluentAssertions; CSharpier, then the msbuild analyzer pass,
  then the msbuild nullable pass, then vstest, in that order, per CLAUDE.md.
- **A-10 — unverified point carried from research.**
  `QuickFileMetrics_WRITE_WithNullList_SkipsBodyAndDoesNotThrow`
  (`QuickFiler.Test/Controllers/EfcHomeControllerTests.cs`, not an owned test file) is expected to
  be unaffected by the `int` to `double` widening because the guard at
  `EfcHomeController.Metrics.cs:18-21` returns before `:23`, but the file was not read in full
  during research. Confirm during implementation.
- **A-11 — unverified point carried from research.** Whether
  `using System.Collections.Concurrent;` (`QfcHomeController.cs:2`) has another consumer in the
  same partial was not exhaustively verified. The compiler is the authority; the analyzer pass will
  surface either an unused-using diagnostic or a missing-type error.

---

## Data / API / Config Impact

- **User-facing or API changes:** none. No public signature changes. Two `internal` signatures
  widen from `int` to `double`, visible only to `QuickFiler.Test` through `InternalsVisibleTo`.
- **Data:** the session-metrics CSV changes in three ways — QFC rows begin to be written at all;
  EFC rows gain a twelfth field; all duration values become real, untruncated, and
  culture-invariant. No migration is possible or required for an append-only diagnostic log; older
  rows remain in the file in their original shape.
- **Logging/telemetry:** the metrics CSV *is* the telemetry. No new log sink or level is added.
- **Compatibility notes:** no CLI flag, config schema, or version change. The settings key
  `EmailSession` and its default value are untouched.

---

## Test Strategy

All tests use MSTest, Moq, and FluentAssertions, live in the two existing owned test files, touch
no disk, create no temporary file, and use no `Thread.Sleep`, `Task.Delay`, or wall-clock wait.

### Regression tests to add (bugfix workflow: failing first)

| Defect | Test file | Assertion shape | Red state today |
|---|---|---|---|
| #442 flush | `QfcHomeControllerMetricsTests.cs` | inject `MetricsFileWriter`, capture invocations, assert exactly one with the expected filename, folder root, and lines | writer never invoked; capture list empty |
| #442 negative | `QfcHomeControllerMetricsTests.cs` | `SpecialFolders` without `MyDocuments` -> writer not invoked | passes today and after; guards the guard |
| #442 token | `QfcHomeControllerMetricsTests.cs` | cancel `TokenSource`, then assert the captured token is not cancelled | n/a — new behavior |
| CFN-2 null filter | `QfcHomeControllerMetricsTests.cs` | mocked `GetMoveDiagnostics` returns an array with a trailing `null`; assert the writer receives no `null`/whitespace entry | writer never invoked |
| #443 wrong stopwatch | `QfcHomeControllerMetricsTests.cs` | populate `_stopWatchMoved`, leave `_stopWatch` fresh; assert the `duration` argument reaching the mocked `GetMoveDiagnostics` is `> 0` via `It.Is<double>(d => d > 0)` | argument is `0` |
| #451 stopwatch started | `EfcHomeControllerMetricsTests.cs` | `controller.StopWatch.IsRunning.Should().BeTrue()`, per the `QfcHomeControllerRunAsyncTests.cs:303` precedent | `IsRunning` is `false` |
| #451 truncation | `EfcHomeControllerMetricsTests.cs` | `BuildQuickFileMetricLines` with `elapsedSeconds = 90` renders `90`, not `30` | renders the truncated value |
| #451 rounding pin | `EfcHomeControllerMetricsTests.cs` | `moved.Count > 1` case pinning the exact rendered `durationText` under real division | integer division |
| #451 separator | `EfcHomeControllerMetricsTests.cs` | rendered line splits into exactly 12 fields and contains `,Recipient,Sender,` | 11 fields; contains `RecipientSender` |
| #451 sanitization | `EfcHomeControllerMetricsTests.cs` | commas embedded in `ToRecipientsName`, `SenderName`, and `selectedFolder`; assert the line still splits into 12 fields | field count inflates |
| #451 re-entrancy | `EfcHomeControllerMetricsTests.cs` | `TryBeginExecuteMoves()` true, second call false, then true again after `ResetExecuteMovesState()` | passes today; pins the primitive change |
| #451 overload | `EfcHomeControllerMetricsTests.cs` | absent prerequisites -> returns without throwing; present prerequisites -> delegates to the 3-argument overload | throws `NotImplementedException` |
| RC-9 culture | both files | swap `CultureInfo.CurrentCulture` to `de-DE` inside a `try`/`finally` that restores it, assert the rendered numeric fields use `.` and the row splits into the expected field count | renders `2,00`; field count inflates |

The re-entrancy test is deliberately **sequential**, not concurrent: a genuinely concurrent
assertion on `Interlocked.CompareExchange` is not deterministic and must not be attempted.

The culture test is the only test that mutates ambient state. It is confined to a single test
method, restores `CultureInfo.CurrentCulture` in a `finally`, and is the only way to observe the
defect. It introduces no clock read, no wall-clock wait, and no file.

### Tests that will break — deliberate, planned updates

These are not incidental breakage. Each is a test that currently pins defective behavior, and each
must be updated in the same change with the reason recorded:

1. **`BuildQuickFileMetricLines_WithMovedMailItems_FormatsMetricLine`**
   (`EfcHomeControllerMetricsTests.cs:35`, expected string at `:56-60`). Its literal contains
   `RecipientSender`, the concatenation produced by the missing separator. After the fix the
   expected string becomes `...,2.00,Recipient,Sender,Email,...`. #451 names this update as a
   constraint.
2. **`QuickFileMetricsWriteFilenameOnly_PreservesNotImplementedContract`**
   (`EfcHomeControllerMetricsTests.cs:138-148`). Its name and its
   `act.Should().Throw<NotImplementedException>()` at `:147` explicitly pin the defective contract.
   It is replaced by the two overload tests in the table above.
3. **`WriteMetricsAsync_UsesInjectedClock_ForDateAndTimeStamps`**
   (`QfcHomeControllerMetricsTests.cs:328`). It sets **only** `_stopWatch` (`:332`) and leaves
   `_stopWatchMoved` null, so once `:121` reads `_stopWatchMoved` the test throws
   `NullReferenceException`. It must set `_stopWatchMoved` in the same change.
4. **`NonBlockingProducer_DelaySeam_HonorsInjectedTwentyMillisecondDelay`**
   (`QfcHomeControllerMetricsTests.cs:401`). Its body never calls `NonBlockingProducer`; it
   exercises `TimeProvider.Delay` directly, so it will still compile and pass after the deletion —
   but its name and XML doc become false, and after the change `TimeProvider.Delay` has no
   production call site at all (`Metrics.cs:222` sits inside the deleted method). Delete it, or
   rename it and rewrite the doc comment. Record the choice.

The `int` to `double` widening additionally changes rounding at `EfcHomeController.Metrics.cs:72`:
`duration /= moved.Count` becomes real division, so `120 / 7` yields `17.142857...` where it
previously yielded `17`. `ToString("##0")` rounds both to `17`, and the existing single-item
fixture (`120 / 1`) renders unchanged, but the rounding boundary shifts for multi-item moves. This
is pinned deliberately by the `#451 rounding pin` test above and must be stated in the change
description.

### Tests expected to remain green (named, so a break is investigated rather than absorbed)

`SwapStopWatch_ExecutesCorrectly` (`QfcHomeControllerIterationTests.cs:435-458`),
`StopWatch_PropertyWorksCorrectly` (`QfcHomeControllerPropertyTests.cs:232-252`),
`QuickFileMetrics_WRITE_WhenGetCalendarReturnsNull_DoesNotThrow`
(`QfcHomeControllerMetricsTests.cs:76`),
`GetMoveDiagnostics_NullAppointment_DoesNotThrow` (`:162`),
`QuickFileMetrics_WRITE_UsesInjectedClock_ForDataLine` (`:363`),
`QuickFileMetricsWrite_WithMyDocumentsFolder_InvokesInjectedWriter`
(`EfcHomeControllerMetricsTests.cs:64`), `QuickFileMetricsWrite_WithoutMyDocumentsFolder_DoesNotInvokeWriter`
(`:92`), `QuickFileMetricsWrite_WithNoMovedItems_DoesNotInvokeWriter` (`:117`),
`BuildQuickFileMetricLines_WithNullOrEmptyMovedItems_ReturnsNoLines` (`:20`),
`QuickFileMetrics_WRITE_WithEmptyList_SkipsBodyAndDoesNotThrow` and
`QuickFileMetrics_WRITE_WithNullList_SkipsBodyAndDoesNotThrow` (`EfcHomeControllerTests.cs`, see
A-10), and `QfcHomeControllerRunAsyncTests.cs:303`.

### Known coverage boundaries (recorded, not papered over)

- **QFC seconds-truncation is not asserted numerically.** A `Stopwatch` cannot be set to an
  arbitrary elapsed value without reflection into its internal tick field or a wall-clock wait, and
  the wait is prohibited. The truncation fix is asserted on the EFC side, where
  `BuildQuickFileMetricLines` takes the elapsed value as a plain `double` parameter and needs no
  stopwatch at all. On the QFC side only the behaviorally significant half — *which stopwatch was
  read* — is asserted.
- **`OlStartTime` is not asserted.** `UtilitiesCS.Calendar.GetCalendar` returns `null` in every
  unit fixture (the mocked `Folders.GetEnumerator()` returns an empty enumerator), so the
  appointment branch is skipped and the reconstructed start time is not observable. Change 7 is
  verified by source inspection and by the absence of the `(int)Duration` cast.
- **The EFC constructor-site stopwatch start (`EfcHomeController.cs:76`) may be unreachable from a
  fixture.** The existing helper builds an `EfcDataModel` via
  `FormatterServices.GetUninitializedObject` with `Mail = null`, so the
  `if (DataModel.Mail is not null)` guard at `EfcHomeController.cs:73` is false and line 76 never
  runs. Either supply a data model with a non-null `Mail` for that one test, or assert only the
  `InitAsync` site (`:225`) and verify `:76` by source inspection. Record whichever is chosen.

### Coverage impact and targets

Per CLAUDE.md § UT2, the members changed here — `BuildQuickFileMetricLines`,
`SelectMoveMetricsItems`, `TryBeginExecuteMoves`, `ResetExecuteMovesState`, and the metrics writers
behind their injectable seams — are **testable seams and are explicitly NOT exempt** from the
coverage floor, notwithstanding the COM/VSTO exemption that applies to Outlook-Interop-bound
members of the same classes. Changed lines must not lose coverage; new and changed members target
`>= 90%`. No merge-base baseline has been captured for this feature yet, so the repository-wide
figure is a **record-and-report** obligation against the testable denominator rather than a
blocking threshold (see AC-22).

### Toolchain commands (in order; restart from step 1 on any failure or auto-fix)

1. `dotnet tool run csharpier format .` (verify with `dotnet tool run csharpier check .`)
2. `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
3. `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`
4. `vstest.console.exe <test-assembly-paths> /EnableCodeCoverage`

### Manual validation (optional, not gating)

Run one QuickFiler session longer than 60 seconds ending on the end-of-database path, and one
EmailFiler move, then inspect the session-metrics CSV for a QFC row with a non-zero untruncated
duration and an EFC row with 12 fields. Not gating, because it requires a live Outlook profile.

---

## Acceptance Criteria

Each criterion is independently verifiable by the named test or the stated command. All `git grep`
commands are run from the repository root.

- [ ] **AC-1 (bugfix workflow, regression-test-first).** For each defect RC-1 through RC-9, a
      regression test was written and observed **failing** against the pre-fix source before the
      corresponding fix was made, and observed passing after. The red observation for each is
      recorded in `docs/features/active/quickfiler-home-controller-metrics-442/evidence/regression/`
      with the test name and the verbatim failure message. A defect with no recorded red
      observation does not satisfy this criterion.
- [ ] **AC-2 (#442 flush occurs).** `WriteMetricsAsync` invokes the injected `MetricsFileWriter`
      exactly once with the supplied filename, the `MyDocuments` folder root, and the diagnostic
      lines, asserted by a new test in `QuickFiler.Test/Controllers/QfcHomeControllerMetricsTests.cs`
      that captures the delegate's arguments. The test fails on the pre-fix source because the
      capture list is empty.
- [ ] **AC-3 (#442 flush-timing invariant).** The writer's returned `Task` completes before the
      `Task` returned by `WriteMetricsAsync` completes, asserted by a test whose injected delegate
      sets a flag that is `true` immediately after the `await`. Additionally,
      `git grep -nE "NonBlockingProducer|TimedConsumerAsync|_metricsConsumers|_lockObject|_fileName" QuickFiler/Controllers/`
      returns no match, proving no part of the flush was left on a timer, a background consumer, or
      residual controller state. (The same command matches multiple sites on the pre-fix source.)
- [ ] **AC-4 (#442 flush survives session cancellation).** The token passed to `MetricsFileWriter`
      is not the controller's `Token`: a test that cancels `TokenSource` before invoking
      `WriteMetricsAsync` asserts the captured `CancellationToken.IsCancellationRequested` is
      `false`. This preserves the `QfcHomeController.cs:376` precedent and keeps the flush
      unaffected by `ActionCancelAsync`'s cancel at `EventHandlers.cs:86`.
- [ ] **AC-5 (#442 no blank CSV line).** When `GetMoveDiagnostics` returns an array whose trailing
      element is `null`, the lines reaching `MetricsFileWriter` contain no `null` or whitespace-only
      entry, asserted by a dedicated test with a mocked `GetMoveDiagnostics` returning such an array.
- [ ] **AC-6 (#443 correct stopwatch).** With `_stopWatchMoved` populated to a non-zero interval and
      `_stopWatch` freshly constructed, the `duration` argument reaching the mocked
      `GetMoveDiagnostics` is greater than zero, asserted with
      `It.Is<double>(d => d > 0)`. The test fails on the pre-fix source, where the argument is `0`.
- [ ] **AC-7 (#443 and #451 no seconds truncation).**
      `git grep -n "Elapsed.Seconds" QuickFiler/Controllers/` returns no match, and
      `BuildQuickFileMetricLines` invoked with `elapsedSeconds = 90` and a single moved item renders
      a `durationText` of `90`, not `30`, asserted by a named test in
      `QuickFiler.Test/Controllers/EfcHomeControllerMetricsTests.cs`.
- [ ] **AC-8 (#443 calendar span agrees with the duration).**
      `QfcHomeController.Metrics.cs` reconstructs the appointment start as
      `OlEndTime.Subtract(_stopWatchMoved.Elapsed)` and contains no `(int)Duration` cast, verified by
      `git grep -n "OlEndTime.Subtract" QuickFiler/Controllers/QfcHomeController.Metrics.cs` showing
      the `_stopWatchMoved.Elapsed` form. This criterion is verified by inspection rather than by a
      test, because `UtilitiesCS.Calendar.GetCalendar` returns `null` in every unit fixture; the
      boundary is recorded in Test Strategy.
- [ ] **AC-9 (#451 EFC stopwatch is started).** `EfcHomeController.StopWatch.IsRunning` is `true`
      after construction, asserted by a named test in
      `QuickFiler.Test/Controllers/EfcHomeControllerMetricsTests.cs` following the
      `QfcHomeControllerRunAsyncTests.cs:303` precedent, and
      `git grep -n "Stopwatch.StartNew" QuickFiler/Controllers/EfcHomeController.cs` returns both
      construction sites (`:76` and `:225`). If the `:76` site proves unreachable from a fixture
      without a live Outlook `MailItem`, that limitation is recorded in
      `docs/features/active/quickfiler-home-controller-metrics-442/evidence/qa-gates/` naming the
      blocker, and the site remains covered by the `git grep` assertion.
- [ ] **AC-10 (#451 signature widening).** `EfcHomeController.Metrics.cs:35` and `:57` declare
      `double elapsedSeconds`, and `git grep -n "int elapsedSeconds" QuickFiler/` returns no match.
      The solution compiles clean under toolchain step 3.
- [ ] **AC-11 (#451 rounding pinned deliberately).** A named test invokes
      `BuildQuickFileMetricLines` with `moved.Count > 1` and asserts the exact rendered
      `durationText`, pinning the behavior change from integer to real division at
      `EfcHomeController.Metrics.cs:72`. The change is also stated explicitly in the PR body.
- [ ] **AC-12 (#451 CSV separator).** The line produced by `BuildQuickFileMetricLines` splits on
      `,` into exactly **12** fields and contains the substring `,Recipient,Sender,`, asserted by
      the updated `BuildQuickFileMetricLines_WithMovedMailItems_FormatsMetricLine`. The substring
      `RecipientSender` appears nowhere in
      `QuickFiler.Test/Controllers/EfcHomeControllerMetricsTests.cs`, verified by
      `git grep -n "RecipientSender" QuickFiler.Test/` returning no match.
- [ ] **AC-13 (#451 CSV sanitization).** With a comma embedded in each of `ToRecipientsName`,
      `SenderName`, and `selectedFolder`, the rendered line still splits into exactly 12 fields,
      asserted by a named test. `QfcCollectionController.xComma(...)` is applied to all four
      free-text fields in `EfcHomeController.Metrics.cs`.
- [ ] **AC-14 (#451 atomic re-entrancy).** `TryBeginExecuteMoves()` returns `true` on the first
      call, `false` on a second call before reset, and `true` again after
      `ResetExecuteMovesState()`, asserted by named sequential tests; `_isExecuting` is declared
      `private int` and
      `git grep -n "volatile" QuickFiler/Controllers/EfcHomeController.cs` returns no match. No
      concurrent assertion is attempted, as it would be non-deterministic.
- [ ] **AC-15 (#451 interface overload implemented).**
      `git grep -n "NotImplementedException" QuickFiler/Controllers/EfcHomeController.Metrics.cs`
      returns no match; `QuickFileMetrics_WRITE(string filename)` returns without throwing when
      `_formController`, `DataModel`, or `DataModel.Mail` is absent, and delegates to the
      3-argument overload when they are present, asserted by two named tests replacing
      `QuickFileMetricsWriteFilenameOnly_PreservesNotImplementedContract`. The interface member
      `IFilerHomeController.cs:41` is left unchanged.
- [ ] **AC-16 (culture invariance).** The six numeric format sites
      (`QfcHomeController.Metrics.cs:53, 56, 132, 135` and `EfcHomeController.Metrics.cs:73, 74`)
      pass `CultureInfo.InvariantCulture`, and a named test that temporarily sets
      `CultureInfo.CurrentCulture` to `de-DE` inside a `try`/`finally` asserts the rendered numeric
      fields use `.` as the decimal separator and that the row splits into the expected field count.
      The test restores the original culture in the `finally` block.
- [ ] **AC-17 (test determinism).**
      `git grep -nE "Thread\.Sleep|Task\.Delay|DateTime\.Now|Path\.GetTempPath|GetTempFileName" QuickFiler.Test/Controllers/QfcHomeControllerMetricsTests.cs QuickFiler.Test/Controllers/EfcHomeControllerMetricsTests.cs`
      returns no match. No test in either file touches the filesystem; every clock read goes through
      `FakeTimeProvider` or an injected factory.
- [ ] **AC-18 (deliberate test updates recorded).** All four tests named in Test Strategy
      ("Tests that will break") are updated, replaced, or deleted, and the disposition of each —
      including the choice made for
      `NonBlockingProducer_DelaySeam_HonorsInjectedTwentyMillisecondDelay` — is stated in the change
      description and in the PR body. No pinning assertion for a fixed defect survives.
- [ ] **AC-19 (ownership boundary).** `git diff --name-only <merge-base>..HEAD` lists only the five
      owned production files, the two owned test files, and files under
      `docs/features/active/quickfiler-home-controller-metrics-442/`. In particular
      `QfcHomeController.Iteration.cs`, `QfcFormController.EventHandlers.cs`,
      `QfcCollectionController.cs`, `EfcFormController.cs`,
      `IFilerHomeController.cs`, and `EfcHomeControllerDependencies.cs` are unmodified.
- [ ] **AC-20 (no project-file edit, no new source file).** The diff contains no change to any
      `*.csproj`, `*.props`, or `*.targets` file and adds no new `.cs` file. All new production and
      test code lands in the seven existing owned files.
- [ ] **AC-21 (file-size cap).** Every file touched by the change is under 500 lines. Specifically
      `QuickFiler/Controllers/QfcHomeController.cs` is at or below its pre-change 487 lines (target
      approximately 454) and `QuickFiler.Test/Controllers/QfcHomeControllerMetricsTests.cs` is under
      500.
- [ ] **AC-22 (coverage).** Toolchain step 4 runs with `/EnableCodeCoverage`; no line changed by
      this feature loses coverage relative to the merge-base baseline, and the members named in Test
      Strategy ("Coverage impact and targets") reach `>= 90%`. The repository-wide line-coverage
      figure against the testable denominator per CLAUDE.md § UT2 is **recorded** in
      `docs/features/active/quickfiler-home-controller-metrics-442/evidence/coverage/` together with
      the merge-base baseline, and the change does not lower it. The repository-wide figure is a
      record-and-report obligation, not a blocking threshold for this feature, because no merge-base
      baseline had been captured at spec time.
- [ ] **AC-23 (full toolchain pass).** The four commands in Test Strategy ran in order and the
      final pass completed with zero errors and no file modified by the formatter. The transcript
      (commands, exit codes, and the passed/failed test counts) is recorded in
      `docs/features/active/quickfiler-home-controller-metrics-442/evidence/qa-gates/`.
- [ ] **AC-24 (backward-compatibility decision stated).** The PR body states that the EFC metrics
      row moves from 11 to 12 fields, that EFC durations change from `0` to real values, that all
      durations become untruncated and culture-invariant, and that a repository-wide search found
      **no in-repo reader** of the session-metrics CSV — only the three writers and the three
      settings-plumbing declarations enumerated in Data / API / Config Impact.
- [ ] **AC-25 (cross-feature notes filed).** All four items in `## Cross-Feature Notes` are
      recorded in this file, none of them is fixed in this feature's diff, and CFN-4 (`"hh:mm"`) is
      promoted to its own GitHub issue via the promotion lifecycle with the issue number written
      back into CFN-4. CFN-1, CFN-2, and CFN-3 are communicated to the owning sibling children (446
      and 468) through the epic.

**Total acceptance criteria: 25.**

---

## Cross-Feature Notes

These defects are real, are evidenced, and are **excluded from this feature's scope** because
fixing them requires writing a file owned by a sibling epic child. None is fixed here.

### CFN-1 — `SwapStopWatch()` races the metrics write on the `MoveAndIterate` path (feature 446)

- **Location:** `QuickFiler/Controllers/QfcFormController.EventHandlers.cs:157` versus
  `:161` -> `:142`.
- **Defect:** `BackGroundMoveAsync()` is started at `:157` without being awaited until `:175`, and
  `LoadUiFromQueue()` performs the swap at `:142`. The swap and the metrics write are concurrent
  and unordered, so neither `_stopWatch` nor `_stopWatchMoved` is deterministically correct on that
  path (RC-4).
- **Recommendation to 446:** relocate `_parent.SwapStopWatch()` out of `LoadUiFromQueue()`
  (`:142`) to immediately after `_groups.CacheMoveObjects()` at `:156`, mirroring the
  end-of-database ordering at `:190-191`. That single relocation makes both branches identical and
  removes the race.
- **What this feature does instead, and why that is still an improvement.** AC-6 changes
  `Metrics.cs:121` to read `_stopWatchMoved`. This makes the end-of-database path deterministically
  correct. On the `MoveAndIterate` path the same race remains, but its two outcomes change from
  "correct interval **or zero**" to "current interval **or the previous batch's interval**" — both
  now real durations of the right order of magnitude. **The `MoveAndIterate` race is knowingly left
  unfixed here.** Three owned-file workarounds were evaluated and each fails (RC-4): a
  property-setter snapshot does not change *when* the capture happens and breaks four
  reflection-based tests; having `WriteMetricsAsync` call `SwapStopWatch()` itself converts one race
  into two; and capturing at `CacheMoveObjects()` time requires two forbidden files.

### CFN-2 — `GetMoveDiagnostics` returns an array one element longer than it fills (feature 468)

- **Location:** `QuickFiler/Controllers/QfcCollectionController.cs:2284` allocates
  `new string[_itemGroupsToMove.Count + 1]`; the loop at `:2286-2325` fills only indices
  `0..Count-1`, so the final element is always `null`.
- **Consequence:** `FileIO2.WriteTextFileAsync` calls `sw.WriteLineAsync(null)`
  (`FileIO2.cs:72`), appending a blank line to the CSV on every write. **This is invisible today
  only because nothing is ever written; fixing #442 makes it manifest.**
- **Recommendation to 468:** size the array `_itemGroupsToMove.Count`, not `+ 1`.
- **Owned-file mitigation applied here:** AC-5 filters `null` and whitespace-only entries in
  `WriteMetricsAsync` before handing the array to the writer seam. This is defensive and remains
  correct regardless of what feature 468 does.

### CFN-3 — the dispatcher continuation carrying the metrics write is not awaited (feature 446)

- **Location:** `QuickFiler/Controllers/QfcFormController.EventHandlers.cs:228-231`:
  `await UiThread.Dispatcher.InvokeAsync(async () => await WriteMetrics(...), DispatcherPriority.ContextIdle)`.
- **Defect:** `Dispatcher.InvokeAsync(Func<Task>, priority)` returns `DispatcherOperation<Task>`;
  awaiting that operation yields the inner `Task` **without awaiting it**, so the metrics write is
  effectively fire-and-forget past its first suspension point and its failures do not surface.
- **Recommendation to 446:** use `.Task.Unwrap()`, the pattern already present at
  `UtilitiesCS/Threading/WpfUiDispatcher.cs:61`, so failures propagate and the write completes
  before `ActionCancelAsync` cancels the token.
- **Mitigation applied here:** AC-4 passes `CancellationToken.None` to the writer, so a
  cancellation raised while the write is still in flight cannot abort it.

### CFN-4 — `"hh:mm"` renders 14:30 as `02:30` with no AM/PM designator (own issue)

- **Location:** `QfcHomeController.Metrics.cs:31`, `QfcHomeController.Metrics.cs:110`,
  `EfcHomeController.Metrics.cs:68`. All three are in **owned** files, so this is technically
  fixable here.
- **Deliberately out of scope** for the three reasons stated in Scope & Non-Goals: it is a content
  rather than a shape defect, it breaks three currently passing tests on their asserted literals
  (`QfcHomeControllerMetricsTests.cs:336-337`, `:371-372`,
  `EfcHomeControllerMetricsTests.cs:59`), and no issue lists it as an acceptance criterion.
- **Disposition:** `PROMOTION BLOCKED`. The promotion could not be completed from the executing
  session. A repository policy hook rejects direct `gh issue create` and requires the drm-copilot
  MCP promotion path (`new_potential_entry` -> `potential_to_issue` -> `new_active_feature_folder`);
  those MCP tools are not present in the executing agent's tool surface. `gh` itself is installed
  and authenticated, so this is a policy restriction rather than a missing tool. The exact issue
  title and body to be filed, and the required follow-up, are recorded in the blocker artifact
  `docs/features/active/quickfiler-home-controller-metrics-442/evidence/issue-updates/cfn4-promotion-blocked.2026-08-26T11-32.md`.
  AC-25 is left unchecked until the issue number is written here in place of `PROMOTION BLOCKED`.

---

## Risks & Mitigations

| ID | Risk | Likelihood | Mitigation |
|---|---|---|---|
| R-1 | The append moves onto the `ContextIdle` dispatcher continuation; `FileIO2.WriteTextFileAsync` retries `IOException` up to 100 times with a 100 ms delay, so a locked file can keep that operation pending for approximately 10 seconds | Low (requires a locked metrics file) | Accepted trade-off of the design (A-1). The continuation is asynchronous, so the UI message pump is not blocked; only the dispatcher operation remains pending. Tests never exercise the real writer — they inject the seam |
| R-2 | Deleting `NonBlockingProducer` orphans the `TimeProvider.Delay` seam, whose only remaining consumer would be a test | Certain, given the design | AC-18: delete or rename `NonBlockingProducer_DelaySeam_HonorsInjectedTwentyMillisecondDelay` and state the disposition |
| R-3 | Removing `using System.Collections.Concurrent;` or `using System.Timers;` breaks an unnoticed consumer elsewhere in the partial class | Low | The compiler is the authority; toolchain steps 2 and 3 surface either an unused-using diagnostic or a missing-type error (A-11) |
| R-4 | The `int` to `double` widening silently changes `##0` rounding for multi-item EFC moves | Medium | AC-11 pins the new rounding with an explicit multi-item test and requires it be stated in the change description |
| R-5 | `QfcHomeControllerMetricsTests.cs` (421 lines) crosses the 500-line cap as new tests are added | Medium | AC-21. Reuse `BuildLooseMetricsController()` and keep methods short. If it overflows, the only compliant remedy would move tests to the wrong file, so budget from the start |
| R-6 | The EFC constructor-site stopwatch start (`:76`) is unreachable with the existing fixture because `DataModel.Mail` is `null` | High | AC-9 permits either a non-null `Mail` fixture or a recorded exemption plus the `git grep` shape assertion |
| R-7 | An external, human-maintained spreadsheet consumes the CSV and breaks on the EFC column-count change | Unknown; no in-repo evidence either way | AC-24 requires the shape change be stated explicitly in the PR body. The repository-wide search establishing that no in-repo reader exists is the evidence for not gating on it |
| R-8 | Fixing #442 makes CFN-2's trailing-`null` blank line manifest in production output | Certain, absent mitigation | AC-5 filters null and whitespace entries in the owned file; the root cause is raised to feature 468 |
| R-9 | The culture test mutates `CultureInfo.CurrentCulture`, which is ambient state | Low | Confined to one test method with a `try`/`finally` restore. It is the only way to observe the defect and introduces no clock read, wait, or file |

---

## Rollout & Follow-up

### Release/rollout steps

1. Execute under `epic-orchestrator` on the epic child branch, sequenced per the research
   recommendation: EFC metrics fixes first (their seams already exist,
   `BuildQuickFileMetricLines` is a pure static, and no COM is involved — fastest red/green), then
   EFC re-entrancy, then the QFC stopwatch fixes, then the QFC flush redesign last, because it is
   the largest diff and it is the change that makes CFN-2 observable. Each step ends in a full green
   toolchain pass.
2. Author the PR body via the `pr-author` skill, including the column-shape statement required by
   AC-24 and the rounding statement required by AC-11.
3. Merge into `epic/quickfiler-bug-family-integration`; the epic's integration PR closes #442,
   #443, and #451.

### Post-fix monitoring and clean-up

- After the first real QuickFiler session on a build carrying this change, inspect the session
  metrics CSV and confirm one appended row per OK-click, no blank lines, a non-zero untruncated
  duration, and 12 fields on EFC rows.
- Confirm the follow-up issue for CFN-4 is open and its number is recorded in this file.
- Confirm CFN-1, CFN-2, and CFN-3 have reached features 446 and 468 through the epic before those
  children are closed.
- If the `EmailSession` CSV ever acquires an in-repo reader, revisit the backward-compatibility
  decision recorded in Data / API / Config Impact.

### Links

- Issues: [#442](https://github.com/drmoisan/TaskMaster/issues/442),
  [#443](https://github.com/drmoisan/TaskMaster/issues/443),
  [#451](https://github.com/drmoisan/TaskMaster/issues/451)
- Epic: `quickfiler-bug-family`, integration branch `epic/quickfiler-bug-family-integration`
- Requirements record: `docs/features/active/quickfiler-home-controller-metrics-442/issue.md`
- Research: `docs/features/active/quickfiler-home-controller-metrics-442/research/quickfiler-home-controller-metrics.research.2026-08-24T10-00.md`
- Promoted potentials: `docs/features/potential/promoted/2026-08-07-qfc-home-controller-metrics-never-flushed.md`,
  `docs/features/potential/promoted/2026-08-07-qfc-home-controller-metrics-duration-misread.md`,
  `docs/features/potential/promoted/2026-08-07-efc-home-controller-metrics-inert-duration.md`
- Sibling children referenced: feature 446, feature 464, feature 468
- Related coverage work: [#433](https://github.com/drmoisan/TaskMaster/issues/433),
  [#437](https://github.com/drmoisan/TaskMaster/issues/437)
