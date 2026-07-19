# utilitiescs-nullable-threading — Atomic Implementation Plan

- **Issue:** #369
- **Parent:** Epic `utilitiescs-nullable-remediation` (child, Wave 0)
- **Owner:** drmoisan
- **Last Updated:** 2026-07-18T22-04
- **Status:** Draft
- **Version:** 1.0
- **Work Mode:** full-feature

## Execution Mode Note (preparation only)

This planning run authors and preflight-clears this plan ONLY. Atomic EXECUTION is OUT OF SCOPE for
this run. Execution (running the commands, editing source, capturing evidence, and checking off
tasks) is performed later by `epic-orchestrator` / the atomic executor. No source file is edited and
no evidence artifact is produced during this planning run.

## Requirements Sources (read all in Phase 0)

- `docs/features/active/utilitiescs-nullable-threading/spec.md` (Definition of Done — AC source)
- `docs/features/active/utilitiescs-nullable-threading/user-story.md` (Acceptance Criteria — AC source)
- `docs/features/active/utilitiescs-nullable-threading/issue.md`
- `docs/features/active/utilitiescs-nullable-threading/research/research-findings.2026-07-18T22-45.md`
- `docs/features/epics/utilitiescs-nullable-remediation/epic.md`

Policy compliance is governed by `CLAUDE.md`, `.claude/rules/general-code-change.md`,
`.claude/rules/general-unit-test.md`, and `.claude/rules/csharp.md`. Do not duplicate their content
here; comply with them.

## Hard Constraints (encoded, non-negotiable)

- Per-file `#nullable enable` pragma on each remediated file under `UtilitiesCS/Threading/`; bring
  each opted-in file to ZERO CS86xx under the pragma. A file that emits no CS86xx receives the pragma
  for cluster consistency only.
- NO project-level or solution-level `<Nullable>` element. `UtilitiesCS.csproj` keeps none.
- Annotation and null-safety ONLY: `?` annotations, null guards, `!` only where justified (with a
  short `// why` comment), and null-flow corrections. NO behavior change, NO refactor, NO API
  redesign, NO feature work.
- NO change to locking, ordering, scheduling, single-shot guards, `Interlocked`, `volatile`, timer
  arm/re-arm, `SynchronizationContext` handling, `Dispatcher.Post`/`Send` sequencing, or
  store-lockup-watchdog concurrency semantics.
- Annotations on public members become cross-module contracts for Wave-1 dependents; keep public
  signatures behavior-compatible and annotate to reflect actual runtime null behavior. Annotate the
  cross-module-contract members deliberately and LAST within their batch.
- Do NOT introduce `System.Diagnostics.CodeAnalysis` post-condition attributes (`[NotNullWhen]`,
  `[MaybeNullWhen]`, `[NotNullIfNotNull]`) — unavailable/unpolyfilled on net481. Zero CS86xx is
  reachable with plain `?`, `= null!`, and justified `!`.
- Leave every `*.Designer.cs` non-opted-in (no pragma, no hand-edit) and the 4 `.resx` untouched.
- FLAG (do NOT fix) the pre-existing `TimeOutTask.cs` 500-line breach; FLAG any annotation-induced
  breach of `ApplicationIdleTimer.cs` / `AsyncMultiTasker.cs` past 500 lines rather than splitting.

## CRITICAL Toolchain Deviation (applies to every nullable/type-check task in this plan)

The nullable / type-check verification step MUST use the pragma-only build and MUST NOT add
`/p:Nullable=enable`:

`msbuild TaskMaster.sln /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU" /p:TreatWarningsAsErrors=true`

Rationale: adding `/p:Nullable=enable` turns nullable ON project-wide and surfaces the entire epic's
~2131 CS86xx diagnostics across ~234 files as false failures unrelated to issue #369. Enforcement for
this child is per-file pragma only. This is a deliberate, documented deviation from the stock
`CLAUDE.md` / `.claude/rules/csharp.md` type-check command, for THIS child only. It MUST NOT be
resolved by editing `.claude/rules/*`. The remaining toolchain stages are standard:

- Format: `dotnet tool run csharpier .` (or `csharpier .`)
- Analyzers / codestyle: `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
- Type-check (nullable, pragma-only): the `/t:Rebuild ... /p:TreatWarningsAsErrors=true` command above (NO `/p:Nullable=enable`)
- Test + coverage: `vstest.console.exe <UtilitiesCS test assemblies> /EnableCodeCoverage` (repo-canonical full-suite driver: `scripts/vscode/Invoke-MSTestWithCoverage.ps1`, which wraps `vstest.console.exe` with coverage and emits Cobertura XML)

## Evidence Path Scheme (non-overridable)

All evidence artifacts resolve under
`docs/features/active/utilitiescs-nullable-threading/evidence/<kind>/` with kinds `baseline`,
`regression-testing`, `qa-gates`, `other`. Timestamps use `yyyy-MM-ddTHH-mm`. No `artifacts/...`
evidence path is used. The delegation prompt supplied only canonical `evidence/` kinds, so no
`EVIDENCE_LOCATION_OVERRIDE_REJECTED` substitution is required.

## Batch Map (25 hand-written files, research §6 order)

- Batch 1 (Phase 1) — no-op / confirm-clean (5): `TaskPriority.cs`, `AsyncIdleQueue1.cs`, `ThreadSafeSingleShotGuard.cs`, `ThreadSafeFunctions.cs`, `ProgressMultiStepViewer.cs`.
- Batch 2 (Phase 2) — interfaces + dispatcher adapter (3, Contract): `IUiDispatcher.cs`, `WpfUiDispatcher.cs`, `IProgressViewer.cs`.
- Batch 3 (Phase 3) — ambient/value concurrency types (2, Contract): `CurrentStoreContext.cs`, `LockupStallDecider.cs`.
- Batch 4 (Phase 4) — idle scheduling + idle timer (3): `IdleActionQueue.cs`, `IdleAsyncQueue.cs`, `ApplicationIdleTimer.cs`.
- Batch 5 (Phase 5) — WinForms hand-partials (3): `ProgressPane.cs`, `ProgressViewer.cs`, `SyncContextForm.cs`.
- Batch 6 (Phase 6) — progress trackers (4, Contract): `ProgressPackage.cs`, `ProgressTracker.cs`, `ProgressTrackerAsync.cs`, `ProgressTrackerPane.cs`.
- Batch 7 (Phase 7) — dispatch + watchdog core (3, CRITICAL): `UiThread.cs`, `ThreadMonitor.cs`, `StoreLockupResponder.cs`.
- Batch 8 (Phase 8) — high-contract parallel + timeout (2, LAST): `AsyncMultiTasker.cs`, `TimeOutTask.cs`.

---

### Phase 0 — Policy Reads and Baseline Capture

- [ ] [P0-T1] Read the policy and requirements files in order and emit a policy-read evidence artifact to `docs/features/active/utilitiescs-nullable-threading/evidence/baseline/phase0-instructions-read.<yyyy-MM-ddTHH-mm>.md`
  - Read order: `CLAUDE.md`, `.claude/rules/general-code-change.md`, `.claude/rules/general-unit-test.md`, `.claude/rules/csharp.md`, then `docs/features/active/utilitiescs-nullable-threading/spec.md`, `user-story.md`, `issue.md`, and `research/research-findings.2026-07-18T22-45.md`.
  - Acceptance: artifact contains `Timestamp:`, `Policy Order:`, and an explicit list of every file read.
- [ ] [P0-T2] Run the CSharpier format-check baseline and record it to `docs/features/active/utilitiescs-nullable-threading/evidence/baseline/csharpier-baseline.<yyyy-MM-ddTHH-mm>.md`
  - Command: `dotnet tool run csharpier --check .`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` (pass/fail and any unformatted-file count).
- [ ] [P0-T3] Run the analyzer/codestyle build baseline and record it to `docs/features/active/utilitiescs-nullable-threading/evidence/baseline/analyzer-build-baseline.<yyyy-MM-ddTHH-mm>.md`
  - Command: `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` (build result and analyzer warning/error counts).
- [ ] [P0-T4] Run the PRAGMA-ONLY nullable build baseline and record it to `docs/features/active/utilitiescs-nullable-threading/evidence/baseline/nullable-build-baseline.<yyyy-MM-ddTHH-mm>.md`
  - Command: `msbuild TaskMaster.sln /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU" /p:TreatWarningsAsErrors=true` (NO `/p:Nullable=enable`)
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` recording the current CS86xx count attributable to `UtilitiesCS/Threading/` (expected near-zero because no Threading file yet carries a pragma and the project has no `<Nullable>` element; this documents the pre-opt-in state) and explicitly confirming NO `/p:Nullable=enable` was passed.
- [ ] [P0-T5] Run the coverage baseline over the UtilitiesCS test assemblies and record numeric coverage to `docs/features/active/utilitiescs-nullable-threading/evidence/baseline/coverage-baseline.<yyyy-MM-ddTHH-mm>.md` with the Cobertura XML at `docs/features/active/utilitiescs-nullable-threading/evidence/baseline/coverage-baseline.<yyyy-MM-ddTHH-mm>.cobertura.xml`
  - Command: `pwsh scripts/vscode/Invoke-MSTestWithCoverage.ps1 -CoverageOutput docs/features/active/utilitiescs-nullable-threading/evidence/baseline/coverage-baseline.<yyyy-MM-ddTHH-mm>.cobertura.xml` (full-suite driver wrapping `vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /EnableCodeCoverage`).
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` with a NUMERIC baseline overall `line-rate`/`branch-rate` from the Cobertura root `<coverage>` element AND the targeted `UtilitiesCS/Threading/` line percentage if obtainable from per-package figures; passed/failed test counts recorded.

### Phase 1 — Batch 1 No-Op / Confirm-Clean

- [ ] [P1-T1] Add `#nullable enable` to `UtilitiesCS/Threading/TaskPriority.cs` (entire body commented out; expected zero CS86xx — pragma for cluster consistency) so the file remains behavior-identical and emits zero CS86xx under the pragma
  - Acceptance: file carries the pragma; no behavior change; zero CS86xx for this file (verified in P1-T7).
- [ ] [P1-T2] Add `#nullable enable` to `UtilitiesCS/Threading/AsyncIdleQueue1.cs` (entire file commented-out dead reference copy; expected zero CS86xx) so it remains behavior-identical and emits zero CS86xx under the pragma
  - Acceptance: file carries the pragma; no behavior change; zero CS86xx (verified in P1-T7).
- [ ] [P1-T3] Add `#nullable enable` to `UtilitiesCS/Threading/ThreadSafeSingleShotGuard.cs` (only `int` fields + `Interlocked.Exchange`; single-shot guard semantics must NOT be touched) so it emits zero CS86xx under the pragma
  - Acceptance: file carries the pragma; `Interlocked` single-shot-guard logic byte-unchanged except for the pragma; zero CS86xx (verified in P1-T7).
- [ ] [P1-T4] Add `#nullable enable` to `UtilitiesCS/Threading/ThreadSafeFunctions.cs` (value-type `ref` math + non-null `Func` params + `Interlocked.CompareExchange`) and apply annotation-only edits to reach zero CS86xx
  - Acceptance: file carries the pragma; annotation-only; `Interlocked` math untouched; zero CS86xx (verified in P1-T7).
- [ ] [P1-T5] Add `#nullable enable` to `UtilitiesCS/Threading/ProgressMultiStepViewer.cs` (ctor-only hand partial; `ProgressMultiStepViewer.Designer.cs` left oblivious and untouched) to reach zero CS86xx
  - Acceptance: file carries the pragma; no own fields dereferenced changed beyond annotation; `ProgressMultiStepViewer.Designer.cs` byte-unchanged; zero CS86xx (verified in P1-T7).
- [ ] [P1-T6] Run CSharpier over the Batch 1 files (`Threading/TaskPriority.cs`, `Threading/AsyncIdleQueue1.cs`, `Threading/ThreadSafeSingleShotGuard.cs`, `Threading/ThreadSafeFunctions.cs`, `Threading/ProgressMultiStepViewer.cs`) with `dotnet tool run csharpier .` and confirm no residual formatting diff
  - Acceptance: `csharpier --check .` exits 0 for the touched files; no `*.Designer.cs` was formatted.
- [ ] [P1-T7] Run the pragma-only nullable build and record Batch 1 verification to `docs/features/active/utilitiescs-nullable-threading/evidence/qa-gates/batch1-nullable-build.<yyyy-MM-ddTHH-mm>.md`
  - Command: `msbuild TaskMaster.sln /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU" /p:TreatWarningsAsErrors=true`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` confirming zero CS86xx for the 5 opted-in Batch 1 files and NO new diagnostics elsewhere (build result matches the P0-T4 baseline).
- [ ] [P1-T8] Run the Batch 1 UtilitiesCS tests and record results to `docs/features/active/utilitiescs-nullable-threading/evidence/regression-testing/batch1-tests.<yyyy-MM-ddTHH-mm>.md`
  - Command: `vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /EnableCodeCoverage /TestCaseFilter:"FullyQualifiedName~TaskPriority|FullyQualifiedName~ThreadSafeSingleShotGuard|FullyQualifiedName~ThreadSafeFunctions"`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` with passed/failed counts; all Batch 1 tests green and behavior-identical (no assertions added, removed, or weakened).

### Phase 2 — Batch 2 Interfaces and Dispatcher Adapter

- [ ] [P2-T1] Add `#nullable enable` to `UtilitiesCS/Threading/IUiDispatcher.cs` (interface; no bodies — declares `Action`/`Func<TResult>`/`CancellationToken` params non-null; ~50 cross-module consumers) so the declared contract is nullable-correct with zero CS86xx
  - Acceptance: file carries the pragma; delegate/param nullability reflects actual contract; zero CS86xx (verified in P2-T5).
- [ ] [P2-T2] Add `#nullable enable` to `UtilitiesCS/Threading/WpfUiDispatcher.cs` (`IUiDispatcher` implementation; five one-line forwards to `UiThread.Dispatcher.Invoke(...)`; oblivious framework surface) to reach zero CS86xx
  - Acceptance: file carries the pragma; annotation-only; implementation stays signature-compatible with `IUiDispatcher`; zero CS86xx (verified in P2-T5).
- [ ] [P2-T3] Add `#nullable enable` to `UtilitiesCS/Threading/IProgressViewer.cs` (interface declaration; pragma fixes nullability of the declared `Bar`/`JobName`/`ButtonCancel`/`UiDispatcher`/`SetCancellationTokenSource` contract) to reach zero CS86xx
  - Acceptance: file carries the pragma; declared-contract nullability set deliberately; zero CS86xx (verified in P2-T5).
- [ ] [P2-T4] Run CSharpier over the Batch 2 files (`Threading/IUiDispatcher.cs`, `Threading/WpfUiDispatcher.cs`, `Threading/IProgressViewer.cs`) and confirm no residual formatting diff
  - Acceptance: `csharpier --check .` exits 0 for the touched files.
- [ ] [P2-T5] Run the pragma-only nullable build and record Batch 2 verification to `docs/features/active/utilitiescs-nullable-threading/evidence/qa-gates/batch2-nullable-build.<yyyy-MM-ddTHH-mm>.md`
  - Command: `msbuild TaskMaster.sln /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU" /p:TreatWarningsAsErrors=true`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` confirming zero CS86xx for the 3 opted-in Batch 2 files and NO new diagnostics elsewhere.
- [ ] [P2-T6] Run the Batch 2 UtilitiesCS and QuickFiler dispatcher tests and record results to `docs/features/active/utilitiescs-nullable-threading/evidence/regression-testing/batch2-tests.<yyyy-MM-ddTHH-mm>.md`
  - Command: `vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /EnableCodeCoverage /TestCaseFilter:"FullyQualifiedName~WpfUiDispatcher|FullyQualifiedName~UiDispatcher"`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`; `WpfUiDispatcherTests` (real STA thread) and any `IUiDispatcher` consumers green and behavior-identical.

### Phase 3 — Batch 3 Ambient and Value Concurrency Types

- [ ] [P3-T1] Add `#nullable enable` to `UtilitiesCS/Threading/CurrentStoreContext.cs` and annotate the `volatile string _current` field to `volatile string?` (matching the documented "null = no context" contract), `Current` return to `string?`, `Normalize` param/return to `string?`, `Begin` param to `string?`, and `Scope._previous` to `string?`, leaving the `volatile` keyword and single-writer/single-reader ordering untouched, to reach zero CS86xx
  - Acceptance: file carries the pragma; `volatile`/ordering byte-unchanged; identity nullability matches runtime behavior; zero CS86xx (verified in P3-T4).
- [ ] [P3-T2] Add `#nullable enable` to `UtilitiesCS/Threading/LockupStallDecider.cs` and annotate the `LockupAttribution` struct `StoreIdentity` ctor param and property from `string` to `string?` (genuinely null when no per-store scope is open — the settled contract the watchdog batch consumes); `LockupStallDecider` itself is value-typed and clean; reach zero CS86xx
  - Acceptance: file carries the pragma; annotation-only; the `StoreIdentity` → `string?` chain is settled before Batch 7; `IsStallConfirmed` boundary logic unchanged; zero CS86xx (verified in P3-T4).
- [ ] [P3-T3] Run CSharpier over the Batch 3 files (`Threading/CurrentStoreContext.cs`, `Threading/LockupStallDecider.cs`) and confirm no residual formatting diff
  - Acceptance: `csharpier --check .` exits 0 for the touched files.
- [ ] [P3-T4] Run the pragma-only nullable build and record Batch 3 verification to `docs/features/active/utilitiescs-nullable-threading/evidence/qa-gates/batch3-nullable-build.<yyyy-MM-ddTHH-mm>.md`
  - Command: `msbuild TaskMaster.sln /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU" /p:TreatWarningsAsErrors=true`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` confirming zero CS86xx for the 2 opted-in Batch 3 files and NO new diagnostics elsewhere.
- [ ] [P3-T5] Run the Batch 3 UtilitiesCS and cross-module contract tests and record results to `docs/features/active/utilitiescs-nullable-threading/evidence/regression-testing/batch3-tests.<yyyy-MM-ddTHH-mm>.md`
  - Command: `vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll TaskMaster.Test\bin\Debug\TaskMaster.Test.dll /EnableCodeCoverage /TestCaseFilter:"FullyQualifiedName~CurrentStoreContext|FullyQualifiedName~LockupStallDecider|FullyQualifiedName~StoreLockupAttribution|FullyQualifiedName~AppOlObjectsAttributionContext|FullyQualifiedName~StoresWrapperEnumerationScope"`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`; the `LockupAttribution`/`CurrentStoreContext` consumer tests green and behavior-identical.

### Phase 4 — Batch 4 Idle Scheduling and Idle Timer

- [ ] [P4-T1] Add `#nullable enable` to `UtilitiesCS/Threading/IdleActionQueue.cs` and annotate the lazily `??=`-initialized `_entries` field to `ConcurrentQueue<Action>?` and the `TryDequeue(out Action action)` out param (`out Action?` / `out var`), leaving the idle subscribe single-shot-guard reset and `Application.Idle` scheduling untouched, to reach zero CS86xx
  - Acceptance: file carries the pragma; field-nullability only; scheduling/subscribe-guard byte-unchanged; zero CS86xx (verified in P4-T6).
- [ ] [P4-T2] Add `#nullable enable` to `UtilitiesCS/Threading/IdleAsyncQueue.cs` (initialized `Entries` = new(), value-tuple `TryDequeue`; expected near-clean) and apply annotation-only edits to reach zero CS86xx
  - Acceptance: file carries the pragma; annotation-only; zero CS86xx (verified in P4-T6).
- [ ] [P4-T3] Add `#nullable enable` to `UtilitiesCS/Threading/ApplicationIdleTimer.cs` and annotate the singleton `instance` as `= null!` (set in static ctor; behavior-preserving), `_timer` as `= null!` (set in `StartTimer()`), `syncContext` as `SynchronizationContext?` (already null-checked), the `ApplicationIdle` event as `event ApplicationIdleEventHandler?`, and `FindTriggeringEventHandler` return as `Delegate?`, leaving `Heartbeat`/`ComputeCPUUsage`/`OnApplicationIdle` timing math and the `Interlocked` subscription counting untouched, to reach zero CS86xx; keep annotations IN-PLACE (no new multi-line guard blocks) to avoid crossing the 500-line limit
  - Acceptance: file carries the pragma; annotation-only; timing math and subscription counting byte-unchanged; zero CS86xx (verified in P4-T6); line count observed for the 500-line flag (P4-T7).
- [ ] [P4-T4] Run CSharpier over the Batch 4 files (`Threading/IdleActionQueue.cs`, `Threading/IdleAsyncQueue.cs`, `Threading/ApplicationIdleTimer.cs`) and confirm no residual formatting diff
  - Acceptance: `csharpier --check .` exits 0 for the touched files.
- [ ] [P4-T5] Run the pragma-only nullable build and record Batch 4 verification to `docs/features/active/utilitiescs-nullable-threading/evidence/qa-gates/batch4-nullable-build.<yyyy-MM-ddTHH-mm>.md`
  - Command: `msbuild TaskMaster.sln /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU" /p:TreatWarningsAsErrors=true`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` confirming zero CS86xx for the 3 opted-in Batch 4 files and NO new diagnostics elsewhere.
- [ ] [P4-T6] Run the Batch 4 UtilitiesCS tests and record results to `docs/features/active/utilitiescs-nullable-threading/evidence/regression-testing/batch4-tests.<yyyy-MM-ddTHH-mm>.md`
  - Command: `vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /EnableCodeCoverage /TestCaseFilter:"FullyQualifiedName~IdleActionQueue|FullyQualifiedName~IdleAsyncQueue|FullyQualifiedName~ApplicationIdleTimer"`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`; all Batch 4 tests green and behavior-identical.
- [ ] [P4-T7] Record the `ApplicationIdleTimer.cs` line-count observation to `docs/features/active/utilitiescs-nullable-threading/evidence/other/maintainer-flags.<yyyy-MM-ddTHH-mm>.md`
  - Acceptance: artifact records `Timestamp:` and the post-annotation line count of `ApplicationIdleTimer.cs` (481 pre-change; pragma yields 482); if csharpier reflow + annotations push it to 501+ it is FLAGGED as an annotation-induced breach for the maintainer (NOT split here).

### Phase 5 — Batch 5 WinForms Hand-Partials

- [ ] [P5-T1] Add `#nullable enable` to `UtilitiesCS/Threading/ProgressPane.cs` and annotate ONLY its own hand-declared fields — `_dispatcher` (`Dispatcher?`), `_tokenSource` (`CancellationTokenSource?`), `_context`/`_uiScheduler` as their actual null-state — using `_tokenSource!.Cancel()` in `CancelButton_Click` (invariant: button enabled only after `SetCancellationTokenSource`, preserving current NRE-if-null behavior); never annotate Designer-declared controls (`ButtonCancel`); reach zero CS86xx
  - Acceptance: file carries the pragma; only own hand-declared fields annotated; `ProgressPane.Designer.cs` byte-unchanged and non-opted-in; `UiDispatcher`/`UiSyncContext`/`UiScheduler` getters behavior-compatible; zero CS86xx (verified in P5-T5).
- [ ] [P5-T2] Add `#nullable enable` to `UtilitiesCS/Threading/ProgressViewer.cs` and annotate ONLY its own hand-declared fields — `_dispatcher` (`Dispatcher?`), `_cancelSource` (`CancellationTokenSource?`; public `CancelSource` consumed by `ProgressTracker.Initialize`), `_context`/`_uiScheduler`/`_uiThreadNumber` — using `_cancelSource!.Cancel()` in `CancelButton_Click`; never annotate Designer-declared controls (`Bar`, `JobName`, `ButtonCancel`); reach zero CS86xx
  - Acceptance: file carries the pragma; only own hand-declared fields annotated; `ProgressViewer.Designer.cs` byte-unchanged and non-opted-in; `CancelSource` contract behavior-compatible; zero CS86xx (verified in P5-T5).
- [ ] [P5-T3] Add `#nullable enable` to `UtilitiesCS/Threading/SyncContextForm.cs` and annotate its own auto-props `UiSyncContext` (`SynchronizationContext`) and `UiDispatcher` (`Dispatcher`) — set later in `CaptureUiVariables()`, not the ctor — as `?` or `{ get; private set; } = null!` to reflect actual init behavior; value-type members untouched; never annotate Designer-declared members; reach zero CS86xx
  - Acceptance: file carries the pragma; only own hand-declared auto-props annotated; `SyncContextForm.Designer.cs` byte-unchanged and non-opted-in; consumed-by-`UiThread.Initialize` contract behavior-compatible; zero CS86xx (verified in P5-T5).
- [ ] [P5-T4] Run CSharpier over the Batch 5 hand-written files (`Threading/ProgressPane.cs`, `Threading/ProgressViewer.cs`, `Threading/SyncContextForm.cs`) and confirm no residual formatting diff; do NOT run CSharpier against any `*.Designer.cs`
  - Acceptance: `csharpier --check .` exits 0 for the touched hand-written files; the three Batch 5 `*.Designer.cs` files unchanged.
- [ ] [P5-T5] Run the pragma-only nullable build and record Batch 5 verification to `docs/features/active/utilitiescs-nullable-threading/evidence/qa-gates/batch5-nullable-build.<yyyy-MM-ddTHH-mm>.md`
  - Command: `msbuild TaskMaster.sln /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU" /p:TreatWarningsAsErrors=true`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` confirming zero CS86xx for the 3 opted-in Batch 5 hand-written files, that the corresponding `*.Designer.cs` files produce no CS86xx (oblivious, do not cross-block the hand-written partials), and NO new diagnostics elsewhere.
- [ ] [P5-T6] Run the Batch 5 UtilitiesCS tests and record results to `docs/features/active/utilitiescs-nullable-threading/evidence/regression-testing/batch5-tests.<yyyy-MM-ddTHH-mm>.md`
  - Command: `vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /EnableCodeCoverage /TestCaseFilter:"FullyQualifiedName~ProgressPane|FullyQualifiedName~ProgressViewer|FullyQualifiedName~SyncContextForm"`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`; all Batch 5 tests green and behavior-identical.

### Phase 6 — Batch 6 Progress Trackers

- [ ] [P6-T1] Add `#nullable enable` to `UtilitiesCS/Threading/ProgressPackage.cs` and annotate all optional reference params (`CancellationTokenSource? cancelSource = null`, `ProgressTracker? progressTracker = null`, `ProgressTrackerPane? = null`, `SegmentStopWatch? stopWatch = null`, `Screen? screen = null`) and the mutually-exclusive `_progressTracker`/`_progressTrackerPane` fields as `?`, keeping defaults null and behavior identical, to reach zero CS86xx
  - Acceptance: file carries the pragma; annotation-only; the shared `IProgress<(int Value, string JobName)>` tuple contract stays consistent; `SpawnChild` `?.` behavior unchanged; zero CS86xx (verified in P6-T6).
- [ ] [P6-T2] Add `#nullable enable` to `UtilitiesCS/Threading/ProgressTracker.cs` and annotate `_jobName` (`string?`), the ctor/`Initialize`-set `_cancelSource`/`_screen`/`_uiDispatcher`/`_progressViewer` (`?` or `= null!` where an init-order invariant holds), and reflection results with justified `!`, leaving the `ParentProgress<T>` struct and the `Report`/`ReportAsync` close-on-100 logic unchanged, to reach zero CS86xx
  - Acceptance: file carries the pragma; annotation-only; report/close logic byte-unchanged; zero CS86xx (verified in P6-T6).
- [ ] [P6-T3] Add `#nullable enable` to `UtilitiesCS/Threading/ProgressTrackerAsync.cs` and annotate `_cancelSource`, `_screen` (`Screen?`), `_progressViewer` (set in `InitializeAsync` → `?` or `= null!`), `_jobName` (`string?`), and `_uiDispatcher` (`Dispatcher?`) to reach zero CS86xx
  - Acceptance: file carries the pragma; annotation-only; zero CS86xx (verified in P6-T6).
- [ ] [P6-T4] Add `#nullable enable` to `UtilitiesCS/Threading/ProgressTrackerPane.cs` and annotate `_progressViewer` (assigned inside a `UiThread.Dispatcher.Invoke(...)` ctor lambda; already null-checked in `SafeAction`) as `ProgressPane?` and `_jobName` as `string?`, preserving the `IAppAutoFileObjects.ProgressTracker`-returns-`ProgressTrackerPane` cross-module contract, to reach zero CS86xx
  - Acceptance: file carries the pragma; annotation-only; `SafeAction` null-branch unchanged; the `IAppAutoFileObjects.ProgressTracker` return contract behavior-compatible; zero CS86xx (verified in P6-T6).
- [ ] [P6-T5] Run CSharpier over the Batch 6 files (`Threading/ProgressPackage.cs`, `Threading/ProgressTracker.cs`, `Threading/ProgressTrackerAsync.cs`, `Threading/ProgressTrackerPane.cs`) and confirm no residual formatting diff
  - Acceptance: `csharpier --check .` exits 0 for the touched files.
- [ ] [P6-T6] Run the pragma-only nullable build and record Batch 6 verification to `docs/features/active/utilitiescs-nullable-threading/evidence/qa-gates/batch6-nullable-build.<yyyy-MM-ddTHH-mm>.md`
  - Command: `msbuild TaskMaster.sln /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU" /p:TreatWarningsAsErrors=true`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` confirming zero CS86xx for the 4 opted-in Batch 6 files and NO new diagnostics elsewhere.
- [ ] [P6-T7] Run the Batch 6 UtilitiesCS and QuickFiler tracker tests and record results to `docs/features/active/utilitiescs-nullable-threading/evidence/regression-testing/batch6-tests.<yyyy-MM-ddTHH-mm>.md`
  - Command: `vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /EnableCodeCoverage /TestCaseFilter:"FullyQualifiedName~ProgressPackage|FullyQualifiedName~ProgressTracker|FullyQualifiedName~ProgressTrackerAsync|FullyQualifiedName~ProgressTrackerPane"`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`; all Batch 6 tracker tests green and behavior-identical.

### Phase 7 — Batch 7 Dispatch and Watchdog Core

- [ ] [P7-T1] Add `#nullable enable` to `UtilitiesCS/Threading/UiThread.cs` and annotate the conditionally-set static fields — `_onLockupDetected` (`Action<LockupAttribution>?`), `_monitorTimeProvider` (`TimeProvider?`), `_syncContextForm` (`SyncContextForm?`), `_threadMonitor` (`ThreadMonitor?`), `_uiSyncContext`/`_dispatcher` (`= null!` or `?` per init-order in `Init`) — keeping the public `UiSyncContext`/`Dispatcher`/`UiThreadId`/`AutoScaleFactor` contract (50+ consumers) behavior-compatible and leaving the `SynchronizationContextAwaiter.Post` marshaling, the `_loaded` single-shot init guard, and the `ThreadMonitor` wiring order untouched, to reach zero CS86xx
  - Acceptance: file carries the pragma; annotation-only; public dispatch contract behavior-compatible; single-shot init guard and Post marshaling byte-unchanged; zero CS86xx (verified in P7-T5).
- [ ] [P7-T2] Add `#nullable enable` to `UtilitiesCS/Threading/ThreadMonitor.cs` and annotate the `thread` ctor param/field (`Thread?`), `_onLockupDetected` (`Action<LockupAttribution>?`), `_pollTimer` (`ITimer?`; already `_pollTimer?.Change`), the `TimeProvider timeProvider = null`/`onLockupDetected = null` defaults (`?`), the `GetStackTrace` return and local (`StackTrace?`), and use `dispatcher!.InvokeAsync(...)` in the `[ExcludeFromCodeCoverage]` ping path (behavior-preserving), leaving the polling loop, one-shot timer re-arm (`_pollTimer?.Change` in `finally`), `_lockupReported` once-per-episode latch, and `Thread.Suspend/Resume` diagnostic path untouched, to reach zero CS86xx (the `[ExcludeFromCodeCoverage]` methods must still compile clean under the pragma)
  - Acceptance: file carries the pragma; annotation-only; polling/timer-re-arm/latch byte-unchanged; `EvaluatePoll` consumes `CurrentStoreContext.Current` (`string?`) into `new LockupAttribution(..., string?)` cleanly; zero CS86xx (verified in P7-T5).
- [ ] [P7-T3] Add `#nullable enable` to `UtilitiesCS/Threading/StoreLockupResponder.cs` and annotate ONLY around the documented null-store-model guards — `StoreLockupNotifier? notify = null` and `Action<string>? logSink = null` ctor params, and `var displayName = attribution.StoreIdentity;` now `string?` — resolving any residual guaranteed-non-null call site with `displayName!` (net481 `IsNullOrWhiteSpace` does not refine null-state) rather than a new guard; do NOT add, remove, reorder, or alter the content of any null-branch (no-context / unresolved-sentinel / `<Stores-enumeration>` / already-disabled — issues #260/#264/#292); reach zero CS86xx
  - Acceptance: file carries the pragma; annotation-only; the four null-branches are byte-unchanged in order and content; the existing ctor `?? throw` guards unchanged; zero CS86xx (verified in P7-T5). If a residual diagnostic appears to require touching a branch, it is FLAGGED in P7-T7 rather than resolved.
- [ ] [P7-T4] Run CSharpier over the Batch 7 files (`Threading/UiThread.cs`, `Threading/ThreadMonitor.cs`, `Threading/StoreLockupResponder.cs`) and confirm no residual formatting diff
  - Acceptance: `csharpier --check .` exits 0 for the touched files.
- [ ] [P7-T5] Run the pragma-only nullable build and record Batch 7 verification to `docs/features/active/utilitiescs-nullable-threading/evidence/qa-gates/batch7-nullable-build.<yyyy-MM-ddTHH-mm>.md`
  - Command: `msbuild TaskMaster.sln /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU" /p:TreatWarningsAsErrors=true`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` confirming zero CS86xx for the 3 opted-in Batch 7 files and NO new diagnostics elsewhere.
- [ ] [P7-T6] Run the Batch 7 UtilitiesCS, QuickFiler, and TaskMaster contract tests and record results to `docs/features/active/utilitiescs-nullable-threading/evidence/regression-testing/batch7-tests.<yyyy-MM-ddTHH-mm>.md`
  - Command: `vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll QuickFiler.Test\bin\Debug\QuickFiler.Test.dll TaskMaster.Test\bin\Debug\TaskMaster.Test.dll /EnableCodeCoverage /TestCaseFilter:"FullyQualifiedName~UiThread|FullyQualifiedName~ThreadMonitor|FullyQualifiedName~StoreLockupResponder|FullyQualifiedName~WpfUiDispatcher"`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`; the watchdog/dispatch seams (`ThreadMonitor.EvaluatePoll`, `StoreLockupResponder` null-branches, `UiThread`) green and behavior-identical.
- [ ] [P7-T7] Record the watchdog null-branch preservation confirmation to `docs/features/active/utilitiescs-nullable-threading/evidence/other/maintainer-flags.<yyyy-MM-ddTHH-mm>.md`
  - Acceptance: artifact records `Timestamp:` and confirms the `StoreLockupResponder` no-context / unresolved-sentinel / `<Stores-enumeration>` / already-disabled branches are unchanged in order and content, that only `string?`/`!` annotations were applied around them, and that no diagnostic required touching a branch (or, if one did, that it was FLAGGED not resolved).

### Phase 8 — Batch 8 High-Contract Parallel and Timeout

- [ ] [P8-T1] Add `#nullable enable` to `UtilitiesCS/Threading/AsyncMultiTasker.cs` and annotate the `Func<TimeSpan, ITimerWrapper>? timerFactory = null` defaults (then `??=`), and in the second `AsyncMultiTaskChunker` overload resolve the null-assigned-then-deref `ITimerWrapper timer` (assigned inside `await Task.Run(...)`, dereferenced in `catch`/`finally`) with `timer!.StopTimer()` / `timer!.Dispose()` — PRESERVE the current NRE-if-unassigned behavior; do NOT switch to `timer?.` (that would swallow the failure and change behavior) — plus `!`/pattern-guard on the unconstrained `((IItemInfo)x).Sw.Durations` deref, leaving `Task.Run` fan-out and `Task.WhenAll` ordering untouched, to reach zero CS86xx; keep annotations IN-PLACE to avoid crossing 500 lines
  - Acceptance: file carries the pragma; annotation-only; `timer!` chosen over `timer?.` (behavior-preserving); `Task.Run`/`Task.WhenAll` ordering byte-unchanged; zero CS86xx (verified in P8-T5); line count observed for the 500-line flag (P8-T7).
- [ ] [P8-T2] Add `#nullable enable` to `UtilitiesCS/Threading/TimeOutTask.cs` and drive its ~15 `RunWithTimeout`/`TimeoutAfter` overloads to zero CS86xx WITHOUT widening the public return type — keep `Task<TResult>` and use `result = default!` / `return result!` for the unconstrained-`TResult` `default` paths (do NOT widen to `Task<TResult?>`); annotate `Func<int, CancellationTokenSource>? timeoutSourceFactory = null`, the `MarshalTaskResults` `castedSource = source as Task<TResult>` local (`Task<TResult>?`, already null-checked), and the timer-callback `state` casts with justified `!`; do NOT split the file
  - Acceptance: file carries the pragma; annotation-only; public `Task<TResult>` return type unchanged (no `Task<TResult?>` widening); no file split; zero CS86xx (verified in P8-T5). The 975-line breach is FLAGGED in P8-T7.
- [ ] [P8-T3] Run CSharpier over the Batch 8 files (`Threading/AsyncMultiTasker.cs`, `Threading/TimeOutTask.cs`) and confirm no residual formatting diff
  - Acceptance: `csharpier --check .` exits 0 for the touched files.
- [ ] [P8-T4] Run the pragma-only nullable build and record Batch 8 verification to `docs/features/active/utilitiescs-nullable-threading/evidence/qa-gates/batch8-nullable-build.<yyyy-MM-ddTHH-mm>.md`
  - Command: `msbuild TaskMaster.sln /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU" /p:TreatWarningsAsErrors=true`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` confirming zero CS86xx for the 2 opted-in Batch 8 files and NO new diagnostics elsewhere.
- [ ] [P8-T5] Run the Batch 8 UtilitiesCS and QuickFiler timeout/parallel tests and record results to `docs/features/active/utilitiescs-nullable-threading/evidence/regression-testing/batch8-tests.<yyyy-MM-ddTHH-mm>.md`
  - Command: `vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /EnableCodeCoverage /TestCaseFilter:"FullyQualifiedName~AsyncMultiTasker|FullyQualifiedName~TimeOutTask|FullyQualifiedName~QfcItemController"`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`; the multiple `TimeOutTask` coverage suites and `AsyncMultiTasker` tests green and behavior-identical.
- [ ] [P8-T6] Record the `AsyncMultiTasker` `timer!` and `TimeOutTask` return-type-stability decisions to `docs/features/active/utilitiescs-nullable-threading/evidence/other/maintainer-flags.<yyyy-MM-ddTHH-mm>.md`
  - Acceptance: artifact records `Timestamp:` and both behavior-preserving decisions for reviewer confirmation: (a) `AsyncMultiTasker` second-overload `timer!` (NOT `timer?.`) preserves current NRE-if-unassigned behavior; (b) `TimeOutTask` keeps public `Task<TResult>` with `result = default!` / `return result!` rather than the silently-widening `Task<TResult?>` (escalate any genuine desire to widen to the maintainer).
- [ ] [P8-T7] Record the file-size pre-existing/annotation-induced breach flags to `docs/features/active/utilitiescs-nullable-threading/evidence/other/maintainer-flags.<yyyy-MM-ddTHH-mm>.md`
  - Acceptance: artifact records `Timestamp:` and (a) `TimeOutTask.cs` (975 lines; 976 with the pragma) exceeds the 500-line limit as a PRE-EXISTING condition, FLAGGED not fixed (splitting the ~15 overloads is an out-of-scope refactor deferred to a separate issue); and (b) the post-annotation line counts of `AsyncMultiTasker.cs` (465 pre-change) and `ApplicationIdleTimer.cs` (481 pre-change, cross-referenced from P4-T7) with any crossing of 500 FLAGGED as an annotation-induced breach rather than resolved by splitting.

### Phase 9 — Final QC Loop, Coverage Delta, and Acceptance Verification

- [ ] [P9-T1] Run the CSharpier format gate over the repository and record it to `docs/features/active/utilitiescs-nullable-threading/evidence/qa-gates/final-csharpier.<yyyy-MM-ddTHH-mm>.md`
  - Command: `dotnet tool run csharpier --check .`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` with a clean (exit 0) result. If CSharpier changes files, restart the toolchain loop from this task. This command MUST run and be recorded (no SKIPPED path).
- [ ] [P9-T2] Run the analyzer/codestyle build gate and record it to `docs/features/active/utilitiescs-nullable-threading/evidence/qa-gates/final-analyzer-build.<yyyy-MM-ddTHH-mm>.md`
  - Command: `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` with a clean build and no new analyzer diagnostics vs the P0-T3 baseline. If this step changes files, restart from P9-T1. This command MUST run and be recorded (no SKIPPED path).
- [ ] [P9-T3] Run the PRAGMA-ONLY nullable/TreatWarningsAsErrors type-check gate and record it to `docs/features/active/utilitiescs-nullable-threading/evidence/qa-gates/final-nullable-build.<yyyy-MM-ddTHH-mm>.md`
  - Command: `msbuild TaskMaster.sln /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU" /p:TreatWarningsAsErrors=true` (NO `/p:Nullable=enable`)
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` confirming zero CS86xx across all 25 opted-in `Threading/` files and NO new diagnostics elsewhere; records that `/p:Nullable=enable` was NOT passed. If this step changes files, restart from P9-T1. This command MUST run and be recorded (no SKIPPED path).
- [ ] [P9-T4] Run the coverage-enabled test gate over the UtilitiesCS test assemblies and record numeric post-change coverage to `docs/features/active/utilitiescs-nullable-threading/evidence/qa-gates/final-coverage.<yyyy-MM-ddTHH-mm>.md` with the Cobertura XML at `docs/features/active/utilitiescs-nullable-threading/evidence/qa-gates/final-coverage.<yyyy-MM-ddTHH-mm>.cobertura.xml`
  - Command: `pwsh scripts/vscode/Invoke-MSTestWithCoverage.ps1 -CoverageOutput docs/features/active/utilitiescs-nullable-threading/evidence/qa-gates/final-coverage.<yyyy-MM-ddTHH-mm>.cobertura.xml`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` with NUMERIC post-change overall `line-rate`/`branch-rate` and the `UtilitiesCS/Threading/` targeted percentage if obtainable, plus passed/failed test counts (all UtilitiesCS tests green). If this step changes files, restart from P9-T1. This command MUST run and be recorded (no SKIPPED path).
- [ ] [P9-T5] Verify `UtilitiesCS.csproj` (and `TaskMaster.sln`) introduce no project-level or solution-level `<Nullable>` element and record the check to `docs/features/active/utilitiescs-nullable-threading/evidence/qa-gates/csproj-no-nullable.<yyyy-MM-ddTHH-mm>.md`
  - Command: grep `UtilitiesCS/UtilitiesCS.csproj` and `TaskMaster.sln` for `<Nullable>`.
  - Acceptance: artifact records `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` confirming no `<Nullable>` element exists (DoD item satisfied).
- [ ] [P9-T6] Compute the coverage delta and changed-line no-regression check and record it to `docs/features/active/utilitiescs-nullable-threading/evidence/qa-gates/coverage-delta.<yyyy-MM-ddTHH-mm>.md`
  - Inputs: baseline Cobertura from P0-T5 and post-change Cobertura from P9-T4.
  - Acceptance: artifact records baseline coverage (numeric), post-change coverage (numeric), and changed-line coverage, and confirms NO coverage regression on changed lines (the annotation-only edits prefer `?` / `= null!` / justified `!` over new guards, so no new uncovered executable line is introduced); if regression is detected the outcome is remediation-required, not PASS.
- [ ] [P9-T7] Map each acceptance-criteria checkbox in BOTH `spec.md` `## Definition of Done` (8 checkboxes) AND `user-story.md` `## Acceptance Criteria` (8 checkboxes) to its satisfying phase/task per the `acceptance-criteria-tracking` skill and record it to `docs/features/active/utilitiescs-nullable-threading/evidence/qa-gates/ac-checkoff.<yyyy-MM-ddTHH-mm>.md`
  - Acceptance: artifact contains `Timestamp:` and, for `spec.md` `## Definition of Done`, a row per DoD item — per-file pragma/zero-CS86xx (Phases 1-8 + P9-T3), no `<Nullable>` element (P9-T5), annotation/null-safety only with no concurrency-semantics change (Phases 1-8 + P9-T1/T2), tests pass / no changed-line coverage regression (P9-T4/T6), full toolchain final pass with pragma-only type-check (P9-T1..T4), `StoreLockupResponder` null-branch preserved (P7-T3/T7), Designer/`.resx` left oblivious and hand-partials own-field-only (P1-T5, P5-T1..T5), `TimeOutTask.cs` 500-line flag plus `ApplicationIdleTimer`/`AsyncMultiTasker` breach flags (P4-T7, P8-T7) — each mapped to a satisfying task with its evidence path; AND, for `user-story.md` `## Acceptance Criteria`, a separate independent section with one row per each of the 8 Acceptance-Criteria checkboxes mapped to its satisfying task with its evidence path. Both source files must have their checkboxes updated to `[x]` as each item is verified.

## Notes

- Atomic EXECUTION is out of scope for this planning run (preparation mode): this plan is authored
  and preflight-cleared only. Execution is performed later by `epic-orchestrator` / the atomic
  executor, which runs the commands, edits source, captures the evidence artifacts, and checks off
  the tasks.
- The rules-vs-convention conflict (`.claude/rules/csharp.md` documenting global `/p:Nullable=enable`
  vs the per-file opt-in) is FLAGGED at the epic level (Wave-2 capstone child); it is NOT resolved in
  this feature and no `.claude/rules/*` file is edited.
