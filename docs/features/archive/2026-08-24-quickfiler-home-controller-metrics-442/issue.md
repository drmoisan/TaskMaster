# quickfiler-home-controller-metrics (Issue)

- Issue: #442
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/442
- Also Resolves: #443, #451
- Type: bug
- Work Mode: full-bug
- Owner: drmoisan
- Epic: quickfiler-bug-family (integration branch `epic/quickfiler-bug-family-integration`)
- Last Updated: 2026-08-24T09-45
- Status: Prepared (preparation mode; execution deferred to epic-orchestrator)

> Promotion note: all three issues below were promoted to GitHub before this run and were verified
> OPEN on 2026-08-24 via `gh issue view`. No new potential entry and no new issue were created by
> this run; `new_potential_bug_entry` and `potential_to_issue` were deliberately not invoked because
> `potential_to_issue` has no idempotent path and always creates a new issue, which would have
> duplicated every one of them.

> Acceptance-criteria source: work mode is `full-bug`, so `spec.md` is the single authoritative
> acceptance-criteria source for this feature. This file is the consolidated requirements record and
> deliberately carries no competing `## Acceptance Criteria` section.

## Summary

Three pre-existing, independently promoted bugs in the QuickFiler home-controller metrics paths are
fixed together because they share the same code, the same emitted CSV artifact, and the same
underlying invariant — when a metrics flush occurs relative to the controller lifecycle, and which
elapsed interval that flush records.

| Issue | Title | Severity | Controller |
|---|---|---|---|
| [#442](https://github.com/drmoisan/TaskMaster/issues/442) | Qfc home-controller metrics never flushed | Medium | `QfcHomeController` |
| [#443](https://github.com/drmoisan/TaskMaster/issues/443) | Qfc home-controller metrics duration misread | Medium | `QfcHomeController` |
| [#451](https://github.com/drmoisan/TaskMaster/issues/451) | Efc home-controller metrics inert duration | Low-Medium | `EfcHomeController` |

The three defects compound: #442 means no QuickFiler metrics row is ever written at all, which
currently MASKS #443's wrong duration value. #451 is the same class of defect in the sibling EFC
controller, where the stopwatch is never started so every duration is zero.

## Authoritative Requirement Documents

Each promoted potential document is richer than its GitHub issue body and carries file:line, the
offending code block, root cause, suggested fix, and severity. These are the authoritative
requirements for this feature:

- `docs/features/potential/promoted/2026-08-07-qfc-home-controller-metrics-never-flushed.md` (#442)
- `docs/features/potential/promoted/2026-08-07-qfc-home-controller-metrics-duration-misread.md` (#443)
- `docs/features/potential/promoted/2026-08-07-efc-home-controller-metrics-inert-duration.md` (#451)

## Defects In Scope

### #442 — QuickFiler session metrics are never flushed to disk

`QfcHomeController.WriteMetricsAsync` enqueues metrics lines into a `BlockingCollection<string>`
that no consumer ever drains.

- `QfcHomeController.Metrics.cs:226` — the guard
  `Interlocked.CompareExchange(ref _metricsConsumers, 0, 2) == 2` can never be true.
  `_metricsConsumers` is initialized to `0` (`QfcHomeController.cs:356`) and is only ever
  *decremented* (`QfcHomeController.Metrics.cs:228`, `QfcHomeController.cs:366`). No code path
  increments it.
- `QfcHomeController.Metrics.cs:229-230` — even if the guard passed, the
  `new System.Timers.Timer(2000)` is assigned to a **local**, has `TimedConsumerAsync` subscribed to
  `Elapsed`, and is never `Start()`ed, never enabled, and never disposed.
- `_metrics` never receives `CompleteAdding()`, so the `GetConsumingEnumerable()` drain at
  `QfcHomeController.cs:367` would block indefinitely if the consumer ever did run.
- `QfcHomeController.cs:358` — `_fileName` is `static` on an instance-scoped concern, is assigned at
  `Metrics.cs:153`, and is never read; `TimedConsumerAsync` uses `Globals.FS.Filenames.EmailSession`
  instead.

### #443 — Qfc duration is read from the wrong stopwatch and truncated

- **Wrong stopwatch.** `QfcHomeController.Metrics.cs:121` reads `StopWatch.Elapsed` (`_stopWatch`);
  the commented-out line 120 shows it previously read `_stopWatchMoved`. Production calls
  `SwapStopWatch()` *before* the metrics write on the end-of-database path, so `_stopWatch` is the
  freshly restarted stopwatch and the true interval sits unread in `_stopWatchMoved`. The sibling
  writer `QuickFileMetrics_WRITE` (`Metrics.cs:42`) reads `_stopWatchMoved` — the two writers
  disagree.
- **Seconds truncation.** `Metrics.cs:42` and `Metrics.cs:121` use `TimeSpan.Seconds` (the 0-59
  component) rather than `TotalSeconds`, so a 90-second interval is recorded as 30.
  `Metrics.cs:44` compounds this by deriving `startTime` from the full `Elapsed` while `duration`
  uses the truncated value, so the calendar appointment span and the CSV duration disagree.

### #451 — Efc duration is permanently zero, plus four adjacent defects

- **Defect 1 (primary).** `EfcHomeController._stopWatch` is constructed at
  `EfcHomeController.cs:76` and `EfcHomeController.cs:225` but **never started** anywhere in the
  `EfcHomeController` family, so `EfcHomeController.Metrics.cs:23` always evaluates to `0`. Contrast
  the sibling `QfcHomeController.cs:267-268`, which constructs *and* starts.
- **Defect 2.** `EfcHomeController.Metrics.cs:23` uses `.Seconds` instead of `.TotalSeconds`. This
  is latent behind Defect 1 today but becomes active the moment the stopwatch is started, so the two
  must be fixed together.
- **Defect 3.** `EfcHomeController.ExecuteMoves.cs:48-57` guards re-entrancy with a `volatile` field
  (`_isExecuting`, declared `EfcHomeController.cs:389`). `volatile` constrains memory ordering only;
  it does not make the read-then-write atomic, so two callers can both observe the "not executing"
  state and both proceed. `Interlocked.CompareExchange` is the correct primitive.
- **Defect 4.** `EfcHomeController.Metrics.cs:80-81` omits the CSV field separator between
  `ToRecipientsName` and `SenderName`, so the two values are emitted concatenated. This defect is
  currently **pinned by an existing assertion** expecting the concatenated form at
  `QuickFiler.Test/Controllers/EfcHomeControllerMetricsTests.cs:59`; the fix must update that
  assertion deliberately in the same change.
- **Defect 5.** `EfcHomeController.Metrics.cs:79` applies `QfcCollectionController.xComma(...)` to
  `Subject` only, while `ToRecipientsName`, `SenderName`, and the folder value are written
  unsanitized, so a comma in any of them corrupts the CSV row shape.
- **Defect 6.** `EfcHomeController.Metrics.cs:26-29` declares a public single-argument
  `QuickFileMetrics_WRITE(string filename)` overload whose entire body is
  `throw new NotImplementedException();`. It is public API surface that cannot be called
  successfully.

## Files This Feature Owns

Only these production files may be written:

- `QuickFiler/Controllers/QfcHomeController.cs` (487 lines — only ~13 lines of headroom under the
  500-line policy cap)
- `QuickFiler/Controllers/QfcHomeController.Metrics.cs`
- `QuickFiler/Controllers/EfcHomeController.cs`
- `QuickFiler/Controllers/EfcHomeController.Metrics.cs`
- `QuickFiler/Controllers/EfcHomeController.ExecuteMoves.cs`

Test files (both already carry `Compile Include` entries, so no project-file edit is expected):

- `QuickFiler.Test/Controllers/QfcHomeControllerMetricsTests.cs`
- `QuickFiler.Test/Controllers/EfcHomeControllerMetricsTests.cs`

## Files This Feature Must Not Write

These are owned by sibling epic children. Reading them is expected; writing them is forbidden. Any
fix that appears to require one must be recorded in `spec.md` as a cross-feature note and kept out
of the plan.

- `QuickFiler/Controllers/QfcHomeController.Iteration.cs` (feature 446)
- `QuickFiler/Controllers/QfcCollectionController.cs` (feature 468)
- `QuickFiler/Controllers/QfcFormController.EventHandlers.cs` (feature 446)
- `QuickFiler/Controllers/EfcFormController.cs` (feature 464)

## Constraints

- **Bugfix Workflow (CLAUDE.md).** Every defect needs a failing regression test first, then the
  minimal targeted fix, then local verification.
- **Deterministic clock.** `.claude/rules/general-unit-test.md` requires a controllable clock and
  prohibits real wall-clock waits, `Thread.Sleep`, and `Task.Delay` in tests. The injectable time
  seam must be used rather than wall-clock reads. `QfcHomeController.Metrics.cs:17` already exposes
  `internal TimeProvider TimeProvider { get; set; } = TimeProvider.System;`, and
  `EfcHomeController.Metrics.cs` already routes through `_dependencies.MetricsNowFactory()` and
  `_dependencies.MetricsLineWriter(...)`.
- **No temporary files in tests.** Metrics writing must be asserted through an injectable writer
  seam, never by touching disk.
- **C# toolchain and libraries.** MSTest, Moq, FluentAssertions; CSharpier, then msbuild analyzers,
  then msbuild nullable, then vstest, per CLAUDE.md.
- **Emitted-CSV behavior change.** Starting the EFC stopwatch, widening to `TotalSeconds`, and
  adding the missing separator all change the content of an emitted metrics file, so they require a
  deliberate backward-compatibility decision recorded in `spec.md`.
- **Test project file.** If a genuinely new test file is unavoidable, its `Compile Include` may be
  added only within the alphabetical neighbourhood of that file's own name in the item group at
  `QuickFiler.Test/QuickFiler.Test.csproj` lines 57-175, which is ordered alphabetically and shared
  with sibling children.

## Out of Scope for This Run

Preparation mode only. Atomic execution, PR authoring, and CI monitoring are performed later by
`epic-orchestrator`.
