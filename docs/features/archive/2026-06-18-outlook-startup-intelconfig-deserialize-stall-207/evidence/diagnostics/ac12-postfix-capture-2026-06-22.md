# AC12 post-fix runtime capture (2026-06-22)

Source: live Outlook startup of the add-in built from branch
`bug/outlook-startup-intelconfig-deserialize-stall-207` at commit `6adb73ed`, run under the Visual
Studio debugger (VS Output window; `Microsoft.VisualStudio.DesignTools.WpfTap.dll` present).

## The readiness-gate fix worked for its target (Events / Hook)

`Hook complete | elapsedMs=25; toDoItemsMs=3.32; remindersMs=18.46; inboxSubscribeMs=3.69`

Events phase = `0:00.01`; `OlReminders` 18 ms; `Ol.Inboxes` 3.7 ms; `ProcessNewInboxItemsAsync`
elapsedMs=1. The 113 s `OlReminders` / 53.9 s `Ol.Inboxes` STA block on the hookup path is GONE —
the coordinator deferred the hookups until Outlook was ready, then they completed in milliseconds.
The COMException-drops-subscription failure mode is also gone. The fix is validated for the
Events/Hook startup path.

## The UI lockup persists because the dominant cost is a DIFFERENT mechanism

Phase table:

```
|  0:00.13  LoadBasic    |
|  1:54.99  IntelConfig  |   <- ~115 s
|  0:00.05  OlObjects    |
|  0:01.43  ToDo         |
|  0:00.53  AutoFile     |
|  0:04.85  Engines      |
|  0:00.01  Events       |
|  2:02.02  TOTAL        |
```

The `[IntelConfig timing]` block shows `ReadConfigurationAsync` itself ran fast (read 3.92 ms; People
deserialize 125 ms; emitted at 16:41:55, ~6 s after startup). Yet the IntelConfig PHASE measured
~115 s. Timeline: ReadConfigurationAsync completes 16:41:55 → STA unavailable ~16:41:57–16:43:57 →
Events resumes 16:43:57. The ~115 s is the `Task.Run` continuation in `LoadSequentialAsync` unable to
resume on the STA, NOT TaskMaster deserialization.

## This stall predates and is independent of the fix

The IntelConfig-phase continuation stall was present in pre-fix captures: ~60 s (increment-2 Run B,
06-19) and ~115 s / ~60 s (increment-3, 06-21). This post-fix run (~115 s) is the same pre-existing
residual; the readiness-gate fix neither caused nor worsened it. The dominant startup cost simply
"migrated" off the now-fixed Hook path and is fully exposed as the IntelConfig-phase stall.

## Attribution is still OPEN (two confounds in this capture)

During the ~115 s STA-unavailable window the log shows BOTH:
- the Teams add-in throwing many first-chance exceptions (`InvalidOperationException`,
  `JsonReaderException` ×8, `COMException`, `LoadTimeReportingException`), and
- TaskMaster's own assemblies loading (Swordfish.NET.General, ToDoModel, the WPF stack
  PresentationFramework/PresentationCore/System.Xaml, TaskVisualization).

Two confounds prevent attribution from this single capture:
1. **Debugger overhead.** This run is under the VS debugger: symbol loading, unoptimized JIT,
   `WpfTap` WPF-init interception, and per-exception first-chance handling for Teams' many exceptions
   can massively inflate STA-occupied time versus a non-debugger run.
2. **Shared STA.** Outlook add-ins share the main STA; a long synchronous call by Teams (or Outlook
   itself) during its load would block TaskMaster's continuation — external to TaskMaster.

TaskMaster cannot be confirmed as the cause from this capture, and it cannot be excluded either (its
own WPF/TaskVisualization assemblies load in the window). Per the maintainer's scope rule (in scope
iff this add-in causes it), attribution must be settled before classifying.

## Recommended attribution captures (cheap, decisive)

1. **No-debugger run** — launch Outlook normally (not under the debugger) and read the timing via
   DebugView. If the IntelConfig-phase stall largely disappears, it was debugger overhead.
2. **Teams-disabled run** — disable the Teams Meeting add-in and relaunch. If the stall disappears,
   it is external (Teams) and out of scope; if it persists, TaskMaster's own load path is implicated
   (in scope) and the fix would extend to deferring/pre-jitting the heavy WPF/TaskVisualization load
   off the critical startup continuation.

## AC12 status

NOT MET. The end-to-end objective (startup completes without a prolonged STA block) is not satisfied:
the UI was locked ~2 minutes. The specific hookup-path block (AC1–AC6 target) is resolved, but a
distinct, pre-existing IntelConfig-phase continuation stall now dominates and must be attributed
before further corrective work. Per the scope-change rule, this stall is a separate root cause from
the readiness-gate fix and should be handled as a new investigation, not folded into this change.
