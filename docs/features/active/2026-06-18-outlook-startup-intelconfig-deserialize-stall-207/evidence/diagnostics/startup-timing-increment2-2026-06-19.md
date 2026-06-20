# Increment-2 startup instrumentation capture (2026-06-19)

Source: live Outlook startup of the add-in built from branch
`bug/outlook-startup-intelconfig-deserialize-stall-207` at commit `f5f0042b`. Captured from the
Visual Studio Output (Debug) window. Two cold starts (21:04 and 21:11) plus prior captures.

## What increment 2 added

- `AppEvents.Hook()` now reports per-COM-operation timing: `toDoItemsMs`, `remindersMs`,
  `inboxSubscribeMs`.
- `IntelligenceConfig.ReadConfigurationAsync` now reports the `GetSerializedConfigurations()` read
  duration separately from per-resource deserialize timing.

## Run A — 21:04:49 to 21:06:50

`[IntelConfig timing]`: `GetSerializedConfigurations read: durationMs=7.80; entries=3`; People
deserialize 122.44 ms, StoresWrapper 1.07 ms, RecentFolders 0.91 ms. IntelConfig work total ~133 ms.

`Hook complete | elapsedMs=113654; inboxSubscriptions=1; toDoItemsMs=5.00; remindersMs=113642.35; inboxSubscribeMs=5.59`

Phase table:

```
|  0:00.08  LoadBasic    |
|  0:00.20  IntelConfig  |
|  0:00.00  OlObjects    |
|  0:00.22  ToDo         |
|  0:00.32  AutoFile     |
|  0:02.59  Engines      |
|  1:53.69  Events       |
|  1:57.13  TOTAL        |
```

Localization: the Events cost is entirely `Globals.Ol.OlReminders` — `remindersMs=113642` (113.6 s).
`ToDoFolder.Items` (5 ms) and the inbox subscription (5.6 ms) are trivial. ProcessNewInboxItemsAsync
processed 0 items in 22 ms.

## Run B — 21:10:47 to 21:11:57

`[IntelConfig timing]`: `GetSerializedConfigurations read: durationMs=8.10; entries=3`; People
deserialize 123.64 ms. IntelConfig work total ~133 ms.

`Hook complete | elapsedMs=28; toDoItemsMs=3.31; remindersMs=20.16; inboxSubscribeMs=3.60`

Phase table:

```
|  0:00.10  LoadBasic    |
|  1:00.04  IntelConfig  |
|  0:00.03  OlObjects    |
|  0:00.98  ToDo         |
|  0:00.64  AutoFile     |
|  0:02.71  Engines      |
|  0:00.06  Events       |
|  1:04.59  TOTAL        |
```

Localization: in this run `OlReminders` was fast (20 ms) and Hook completed in 28 ms. The phase
recorder attributes 60.04 s to IntelConfig, yet `ReadConfigurationAsync` — the entire work of that
phase (`InitAsync` does only `await ReadConfigurationAsync()`) — measured read 8 ms + deserialize
133 ms = ~141 ms. The ~60 s is therefore NOT inside any instrumented IntelConfig computation. It is
a continuation/STA-contention delay: `LoadIntelConfigAsync` offloads to `Task.Run`, and the `await`
continuation back to the STA cannot resume until the STA thread is free. During that window the log
shows heavy assembly loading and Teams add-in COM exceptions on the STA.

## Confirmed conclusions

1. `Globals.Ol.OlReminders` is a confirmed blocking COM/RPC call on the STA/UI thread: 113.6 s when
   it stalls (Run A). It is the single worst offender in the Hook path.
2. `IntelligenceConfig` read (~8 ms) and deserialize (~133 ms) are exonerated in every run.
3. Inbox processing is exonerated (0 items, ~25 ms in both runs).
4. The dominant cost is a synchronous dependency on Outlook/Exchange readiness during the cold-start
   window. It lands on whichever STA operation is pending while Outlook is not yet responsive:
   `OlReminders` in Run A; the IntelConfig `Task.Run` continuation in Run B. The
   `StartupTimingRecorder` attributes STA-stall time to whichever phase's `await` is outstanding,
   so the phase table can mis-attribute pure STA-contention to a phase.

## Implication for the corrective fix

The highest-confidence, concrete defect is the synchronous `OlReminders` access in `Hook()` on the
STA during startup. The broader structural issue is that startup hookups and phase continuations run
on the STA while Outlook may be unready, so a fix targeting only `OlReminders` may reduce but not
fully eliminate the stall (Run B shows a second ~60 s contributor outside TaskMaster computation).
Post-fix validation must be a fresh runtime capture, not a unit test, because the defect is a
COM/STA timing condition not reproducible in MSTest.
