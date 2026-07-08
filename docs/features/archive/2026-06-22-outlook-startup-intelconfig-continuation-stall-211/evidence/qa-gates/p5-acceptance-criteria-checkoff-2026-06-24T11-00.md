# AC15 Check-off Summary (#211 Phase 3.3)

Timestamp: 2026-06-24T11-00

## AC15 — Full add-in-startup-lifetime UI heartbeat with stage labels and bounded self-stop

Status: SATISFIED (toolchain/coverage portion); runtime validation is maintainer-gated (P4-T1/P4-T2).

| AC15 sub-requirement | Satisfying task(s) | Evidence |
|---|---|---|
| 250 ms `DispatcherTimer` UI heartbeat starts as the FIRST action in `ThisAddIn.Application_Startup` (before `SetUpBrightIdeasSettings()`, `SetUpDeedle()`, `new ApplicationGlobals(...)`, and the `IdleAsyncQueue.AddEntry` calls), on `UiThread.Dispatcher` | P2-T2, P2-T4 | `StartStartupLifetimeHeartbeat()` is the first statement in `Application_Startup`; constructs `DispatcherTimer` at 250 ms on `UiThread.Dispatcher` (ThisAddIn.cs) |
| Runs continuously across the whole add-in startup, independent of `LoadSequentialAsync` | P2-T2, P2-T4 | The timer is owned by `ThisAddIn` and started before any globals work; not scoped to `LoadSequentialAsync` |
| Each tick emits one `[startup-lifetime-heartbeat] stageLabel=<...> nominalMs=250.0 actualMs=<F1> gapMs=<F1>` line via `log4net` | P1-T1, P2-T2 | `EmitLifetimeHeartbeat(stageLabel, nominalMs, actualMs)` formats the exact line (F1 InvariantCulture, gap = actual - nominal); tick handler routes through `s => logger.Debug(s)` |
| Coarse `stageLabel` maintained and updated at lifecycle points (`PreGlobalsCtor`, `GlobalsCtor`, `AwaitingIdleQueue`, `Loading`, `PostLoad`); `Finished loading globals` flips to `PostLoad` | P1-T3, P2-T4 | `StartupStageLabels` constants; thin field writes at the five lifecycle points in `Application_Startup`; `_currentStartupStageLabel = PostLoad` and `_startupPostLoadReached = true` immediately after the `Finished loading globals` log line |
| Bounded self-stop: max cap (~180 s) OR sustained post-`PostLoad` responsiveness (gapMs below threshold for a sustained run) | P1-T2, P2-T2, P2-T3 | `StartupLifetimeStopDecider.ShouldStop(elapsed, gap, postLoadReached)`; tick handler calls `StopStartupLifetimeHeartbeat()` when it returns true; `Stop` is idempotent and releases the timer |
| Pure gap/stage/stop-condition logic lives in `StartupDiagnosticsProbe` (coverable, not `[ExcludeFromCodeCoverage]`), covered by deterministic MSTest (synthetic ticks; both stop branches; counter reset; pre-`PostLoad` guard; stage-label set); no live timer/Dispatcher/COM/filesystem/network; no temporary files | P1-T1, P1-T2, P1-T3, P3-T1, P3-T2, P3-T3, P3-T4 | `EmitLifetimeHeartbeat`, `StartupLifetimeStopDecider`, `StartupStageLabels` in StartupDiagnosticsProbe.cs (all 100% covered); 6 deterministic MSTests assert line shape, max-cap branch, sustained-responsiveness branch + counter reset, pre-`PostLoad` guard, the canonical stage-label set, and the decider's bounding-parameter exposure |
| `DispatcherTimer` construction/start/stop stays in `ThisAddIn` (lifecycle-exempt) as a thin seam; no MSTest constructs `ThisAddIn` or a live timer | P2-T1, P2-T2, P2-T3, P2-T6 | The live `DispatcherTimer`/`Stopwatch` live only in `[ExcludeFromCodeCoverage] ThisAddIn`; the two `ThisAddIn` references in tests are `typeof(ThisAddIn).Assembly.Location` only |
| Startup order/semantics unchanged; existing `[ui-heartbeat]`/`[gc-delta]`/`[continuation-resume]`/`[engine-init]` lines unchanged; `PreserveReferencesHandling.All` untouched | P2-T4, P2-T5 | Inserted statements are the heartbeat start and thin stage-label field writes only; the `IdleAsyncQueue` enqueue and load lambda are otherwise unchanged; no edit to the other instrumentation or to `PreserveReferencesHandling.All` |
| New code >= 90% coverage; no repo-wide regression; full C# toolchain passes in order | P5-T1..P5-T6 | New types 100% covered (StartupDiagnosticsProbe 112/112, StartupLifetimeStopDecider 54/54, StartupStageLabels 16/16); additive change, no repo-wide first-party regression; CSharpier/analyzers/nullable all exit 0; 140/140 tests pass |

## Runtime portion (maintainer-gated)

- P4-T1: maintainer capture instructions written (`coldstart-startup-lifetime-heartbeat-capture-instructions-2026-06-24T11-00.md`).
- P4-T2: evidence placeholder created (`runtime-capture-startup-lifetime-heartbeat-PLACEHOLDER.md`), marked PENDING MAINTAINER CAPTURE.
- The full ~2-minute cold-start capture is a runtime task not executed by the automated toolchain; it confirms, on a live cold start, exactly when (which stage) and for how long the STA is frozen across the entire add-in startup.

## AC source note

AC15 is introduced by this plan (the increment's acceptance criterion stated in the plan body
under "Acceptance criterion introduced by this plan"). Consistent with the Phase 3.2 precedent
(`p5-acceptance-criteria-checkoff-2026-06-23T22-30.md` for AC14), and per the no-phantom-criteria
rule (executors do not author new AC checkboxes in source files), AC15 is tracked here via the
plan's P5-T7 mapping rather than by adding a new checkbox to `spec.md`. The `spec.md`
`## Acceptance Criteria` section (AC1-AC10) is unchanged. AC10 (the latency FIX) remains GATED and
out of scope for this diagnosis-only increment.
