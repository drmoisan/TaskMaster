# outlook-startup-sta-thread-com-blocking-stall (Issue #207)

> Folder/branch slug `outlook-startup-intelconfig-deserialize-stall` is historical. Runtime
> evidence (2026-06-19) refuted the IntelConfig-deserialize hypothesis; the issue is reframed to
> the STA-thread COM-blocking diagnosis below. The issue number (#207) and branch are unchanged to
> preserve lifecycle linkage.

- Date captured: 2026-06-18 (reframed 2026-06-19)
- Author: Dan Moisan
- Status: Active — diagnostic instrumentation increment 2 in progress

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #207
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/207
- Last Updated: 2026-06-19
- Work Mode: minor-audit

## Summary

The Outlook VSTO add-in triggers a `ContextSwitchDeadlock` Managed Debugging Assistant during startup. Two runtime captures show the dominant cost is a variable synchronous COM/RPC block on the STA/UI thread (thread 1), not a fixed computation. The blocking site **migrates between runs**: the IntelConfig phase dominated Run 1 (~112 s), while `AppEvents.Hook()` dominated Run 2 (~113 s). Per-resource instrumentation added in increment 1 proved `IntelligenceConfig` deserialization itself is **not** the root cause (~133 ms total across three 560–702 byte payloads). The corrected diagnosis is a blocking COM/RPC call on the STA thread whose latency is dominated by Outlook/Exchange responsiveness.

## Environment

- OS/version: Windows; Outlook desktop (`outlook.exe` host)
- Runtime: .NET Framework Outlook VSTO add-in (TaskMaster), STA `VSTA_Main` thread (thread 1)
- Command/flags used: Add-in startup; `[Startup timing]` phase table and `[IntelConfig timing]` per-resource table both emit on the console/Debug output path
- Data source or fixture: Live `IntelligenceResources` configuration set; live Outlook/Exchange profile

## Steps to Reproduce

1. Launch Outlook with the TaskMaster add-in loaded.
2. Allow `ApplicationGlobals.LoadAsync(false)` to run the sequential startup phases and the deferred Events `LoadAsync`.
3. Observe the `[Startup timing]` phase table, the `[IntelConfig timing]` per-resource table, and the debugger's `ContextSwitchDeadlock` MDA.

## Expected Behavior

Add-in startup completes well within the 60 s COM-apartment threshold, the STA thread keeps pumping messages, Outlook remains responsive, and no `ContextSwitchDeadlock` MDA is raised.

## Actual Behavior

The `ContextSwitchDeadlock` MDA is raised (the CLR cannot transition the COM context for 60 s because the STA thread is blocked). Two captures show the dominant cost migrating between phases:

Run 1 (2026-06-18):

```
| Duration  Action       |
|  0:00.13  LoadBasic    |
|  1:52.31  IntelConfig  |   <- ~112 s
|  0:00.02  OlObjects    |
|  0:00.56  ToDo         |
|  0:00.36  AutoFile     |
|  0:03.66  Engines      |
|  0:12.24  Events       |
|  2:09.31  TOTAL        |
```

Run 2 (2026-06-19):

```
| Duration  Action       |
|  0:00.10  LoadBasic    |
|  0:00.22  IntelConfig  |
|  0:00.00  OlObjects    |
|  0:00.28  ToDo         |
|  0:00.42  AutoFile     |
|  0:02.21  Engines      |
|  2:10.79  Events       |   <- ~131 s
|  2:14.05  TOTAL        |
```

Run 2 Events breakdown (debug log): `AppEvents.Hook()` blocked 113.3 s (`elapsedMs=113334`); `ProcessNewInboxItemsAsync` took 17.4 s. `Hook()` performs only three synchronous COM property accesses, so the 113 s is a blocking COM/RPC call on the STA thread.

Run 2 `[IntelConfig timing]` per-resource breakdown (increment-1 instrumentation):

```
| Duration  SizeBytes  ResourceKey    |
|   131.23        678  People         |
|     1.21        702  StoresWrapper  |
|     0.45        560  RecentFolders  |
```

Deserialization totals ~133 ms. IntelConfig deserialization is exonerated as the root cause.

## Logs / Screenshots

- [x] Attached minimal logs or screenshot
- Snippet: both `[Startup timing]` tables, the `[IntelConfig timing]` table, the `Hook complete ... elapsedMs=113334` line, and the MDA message. Full capture retained under `evidence/diagnostics/`.

## Impact / Severity

- [ ] Blocker
- [x] High
- [ ] Medium
- [ ] Low

Startup unresponsiveness exceeding two minutes and a COM-apartment MDA on every affected profile.

## Suspected Cause / Notes

The dominant cost is a synchronous COM/RPC call on the STA/UI thread (thread 1) whose latency is dominated by Outlook/Exchange responsiveness rather than TaskMaster CPU work. Evidence:

- `IntelligenceConfig` deserialization is ~133 ms (increment-1 per-resource table); not the cause.
- In Run 2 the cost is entirely in `AppEvents.Hook()` (`TaskMaster/AppGlobals/AppEvents.cs:163-181`), which only reads `Globals.Ol.ToDoFolder.Items`, `Globals.Ol.OlReminders`, and subscribes to each inbox's `Items` — three COM accesses, no loop, yet 113 s.
- In Run 1 the cost is in the IntelConfig phase. The currently-instrumented `DeserializeLoaderAsync` is fast, so the Run-1 cost is in the **unmeasured** `IntelligenceConfig.GetSerializedConfigurations()` read (`IntelligenceConfig.cs:85`) or elsewhere in the IntelConfig load path, not in deserialize.

Two incidental defects observed in the same capture were split into separate issues (not in this scope):
- log4net cannot create its `logs\` directory; every log write throws `DirectoryNotFoundException` + `LockStateException` → Issue #208.
- Tesseract OCR engine fails to initialize per image item → Issue #209.

Files to inspect:
- `TaskMaster/AppGlobals/AppEvents.cs` (`Hook`)
- `UtilitiesCS/EmailIntelligence/IntelligenceConfig.cs` (`GetSerializedConfigurations`, `ReadConfigurationAsync`)
- `TaskMaster/AppGlobals/ApplicationGlobals.cs` (`LoadIntelConfigAsync`)

## Proposed Fix / Validation Ideas

- [x] Increment 1 (delivered, committed `2bc71fd5`): per-resource `DeserializeLoaderAsync` timing in `ReadConfigurationAsync`. Result: deserialize exonerated (~133 ms).
- [x] Increment 2 (delivered, committed `f5f0042b`): per-COM-operation timing in `AppEvents.Hook()` and `GetSerializedConfigurations()` read timing. Result: the blocking call is `Globals.Ol.OlReminders` (`remindersMs=113642`, 113.6 s, Run A); read and deserialize exonerated. See `evidence/diagnostics/startup-timing-increment2-2026-06-19.md`.
- [x] Increment 3 (delivered, committed `cfbbd636`): `OlReminders` first-access latency probe. Result: **Possibility 1 confirmed** (113 s early; 32 ms at 30 s delay; 18 ms at 120 s delay). Deferring `OlReminders` alone does NOT fix the stall — the block migrates to `Ol.Inboxes` (53.9 s), and early `Ol.Inboxes` access can THROW a COMException (0xDAC40111 / 0x8E640111) that fails the startup action and loses the inbox subscription. See `evidence/diagnostics/startup-timing-increment3-2026-06-21.md`.
- [ ] Corrective fix (large-path refactor; design directive captured 2026-06-21): minimize STA reliance across the startup hookup path. (a) Gate ALL readiness-dependent COM hookups (`ToDoFolder.Items`, `OlReminders`, `Ol.Inboxes`) on a real Outlook store-readiness check; (b) the STA must always pump — poll a cheap, non-throwing readiness signal (no synchronous block, no fixed delay); (c) break long-running startup work into short STA calls; (d) offload non-COM compute (tokenization, classification, deserialization) to worker threads and release the STA while workers run — COM touches stay on the STA (apartment-bound) but become short and pumped, with primitives extracted on the STA and results marshaled back; (e) treat a not-ready `Ol.Inboxes` COMException as a retry condition so the subscription is never lost. Residual (the ~115 s IntelConfig-phase `Task.Run` continuation stall) is **unattributed**: the window contains TaskMaster's own assembly loads (Swordfish, ToDoModel, TaskVisualization, the WPF stack) interleaved with Teams add-in COM exceptions. Scope rule: in scope if this add-in causes it (e.g., loading heavy WPF assemblies or doing STA work on the continuation path), out of scope if it is genuinely external (Outlook/Teams). Attribution is a research question and must be resolved before classifying. Acceptance criteria for this refactor will be authored from the research findings and design spec.

## Acceptance Criteria

Increment 1 (per-resource deserialize timing) is delivered. The acceptance criteria below govern **increment 2** — additional diagnostic instrumentation to localize the STA-thread blocking call before the corrective fix is scoped.

- [x] AC1: `AppEvents.Hook()` records the elapsed time of each of its three COM operations individually (`ToDoFolder.Items` read, `OlReminders` read, and the per-inbox `Items` subscription) using `System.Diagnostics.Stopwatch`, emitted via the existing `log4net` logger as a single consolidated readable block consistent in style with the existing `[Startup timing]` output.
- [x] AC2: `IntelligenceConfig.ReadConfigurationAsync` records the elapsed time of the `GetSerializedConfigurations()` serialized-payload read separately from the per-resource `DeserializeLoaderAsync` timing, so the read-versus-deserialize split is visible in the emitted block.
- [x] AC3: Instrumentation is behavior-preserving: `Hook()`'s subscriptions and the returned `Config` dictionary contents/semantics are unchanged relative to the pre-change implementation.
- [x] AC4: Deterministic MSTest coverage (Moq + FluentAssertions) verifies the new `IntelligenceConfig` read-versus-deserialize timing seam for a known fixture set, with no live COM, no network/filesystem dependency, and no temporary files. Explicit exception: `AppEvents.Hook()` is a COM-host-bound Outlook Interop method in the documented coverage-exempt set; its instrumentation is logging-only and is verified by inspection rather than a unit test, consistent with the COM/VSTO coverage exemption in `CLAUDE.md`.
- [x] AC5: No banned API is introduced; timing uses `Stopwatch` rather than `DateTime.Now`/`DateTime.UtcNow`.
- [x] AC6: The full C# toolchain passes in order (CSharpier → .NET analyzers → nullable/`TreatWarningsAsErrors` → MSTest with coverage). New and changed lines meet the repository coverage policy and introduce no repository-wide coverage regression.

## Acceptance Criteria — Increment 3 (OlReminders latency probe)

Increments 1 and 2 are delivered. The acceptance criteria below govern **increment 3** — a controlled probe that measures `OlReminders` first-access latency as a function of when the access occurs, to discriminate Possibility 1 (relocatable readiness wait) from Possibility 2 (intrinsic first-access build) before the corrective fix is scoped.

- [x] I3-AC1: A user-scoped setting `RemindersProbeDelaySeconds` (integer, default `0`) is introduced following the existing `StartupTimingEnabled` settings pattern. At the default value `0`, `AppEvents.Hook()` behaves exactly as the pre-increment-3 implementation (synchronous `OlReminders` access); this is behavior-preserving by default.
- [x] I3-AC2: When `RemindersProbeDelaySeconds > 0`, the first `Globals.Ol.OlReminders` access is deferred by that many seconds using a message-pumping mechanism that does not block the STA (a `System.Windows.Threading.DispatcherTimer` or the existing idle infrastructure — never `Thread.Sleep`/`Task.Delay`), then performed exactly once. The access latency (`System.Diagnostics.Stopwatch`) and the elapsed-since-startup at the access point are logged via the existing `log4net` logger in a single readable line.
- [x] I3-AC3: The deferred path is state-equivalent to the synchronous path: after the probe access, `OlReminders` holds the same value and the reminders subscription/behavior matches the synchronous path; the `ToDoFolder.Items` and inbox subscriptions are unchanged.
- [x] I3-AC4: A deterministic MSTest (Moq + FluentAssertions) covers the pure decision/scheduling logic — whether to defer and the resolved delay `TimeSpan` from the setting value — with no live COM, no live timer, no network/filesystem, and no temporary files. The COM access and `DispatcherTimer` wiring in `AppEvents.Hook()` are COM/VSTO-exempt (scheduling/logging only) and verified by inspection per the `CLAUDE.md` exemption.
- [x] I3-AC5: No banned API is introduced (`DateTime.Now`/`DateTime.UtcNow`, `Random.Shared`, `Thread.Sleep`, `Task.Delay`); timing uses `Stopwatch` and the delay uses a `DispatcherTimer`.
- [x] I3-AC6: The full C# toolchain passes in order (CSharpier → .NET analyzers → nullable/`TreatWarningsAsErrors` → MSTest with coverage). New/changed testable lines meet the repository coverage policy with no repository-wide regression, and all touched files remain ≤ 500 lines (extract a small helper if needed).

## Next Step

- [x] Promote to GitHub issue (bug-report template)
- [x] Move to active fix folder / branch
- [x] Increment 1 instrumentation delivered (deserialize timing)
- [x] Increment 2 instrumentation: `Hook()` and `GetSerializedConfigurations()` timing
- [x] Increment 3 instrumentation: `OlReminders` first-access latency probe (Possibility 1 vs 2)
- [ ] Capture latency-vs-delay curve; scope corrective fix from the result
