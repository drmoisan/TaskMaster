Timestamp: 2026-06-23T13-51
Command: Maintainer-provided non-debugger Outlook startup DebugView capture from Codex attachment pasted-text.txt.
EXIT_CODE: 0

# Runtime Capture: Non-Debugger Outlook Startup

## Summary

This capture satisfies the AC5 runtime evidence requirement for issue #211. It
contains `[continuation-resume]` probe lines from a non-debugger Outlook startup
capture and records the continuation wait after `IntelConfig`.

No Visual Studio debugger indicators from the earlier capture are present in the
provided text. The capture includes Outlook, Teams Meeting Add-in, Apple Outlook
Change Notifier, WebView2, and MAPI/address-book provider debug output.

## Continuation Probe Results

| Prior Phase | waitMs | resumeThreadId | resumeSyncContext | staIsIdle | staCpuUsage | staGuiActivity |
| --- | ---: | ---: | --- | --- | ---: | ---: |
| IntelConfig | 0.6 | 1 | System.Windows.Threading.DispatcherSynchronizationContext | True | 0.000 | 0.0 |
| OlObjects | 0.1 | 1 | System.Windows.Threading.DispatcherSynchronizationContext | True | 7.827 | 0.0 |
| ToDo | 0.0 | 1 | System.Windows.Threading.DispatcherSynchronizationContext | False | 1.167 | 3.2 |
| AutoFile | 1.1 | 1 | System.Windows.Threading.DispatcherSynchronizationContext | False | 1.430 | 22.2 |
| Engines | 0.1 | 1 | System.Windows.Threading.DispatcherSynchronizationContext | False | 0.029 | 1.1 |

## Startup Timing Result

The startup timing table in this capture shows the long startup cost is no
longer attributed to the IntelConfig continuation:

| Phase | Duration |
| --- | ---: |
| LoadBasic | 0:00.07 |
| IntelConfig | 0:05.00 |
| OlObjects | 0:00.00 |
| ToDo | 0:00.74 |
| AutoFile | 0:00.35 |
| Engines | 1:52.59 |
| Events | 0:00.00 |
| TOTAL | 1:58.79 |

## Interpretation

The `IntelConfig` continuation wait is `0.6 ms`, which is below the Phase 2
threshold of `> 5000 ms`. This does not support the issue #211 Phase 2
`ConfigureAwait(false)` plus explicit UI-thread re-marshal change for
`IntelConfig`.

The remaining long startup delay is in the `Engines` phase (`1:52.59`). That is
separate from the IntelConfig continuation-stall hypothesis and should be
tracked as a separate follow-up if it is still reproducible and user-impacting.

## Relevant Raw Lines

```text
2026-06-23 13:52:03,294 INFO UtilitiesCS.EmailIntelligence.IntelligenceConfig - [IntelConfig timing]
GetSerializedConfigurations read: durationMs=7.21; entries=3
|   103.78        678  People         |
|     0.31        702  StoresWrapper  |
|     0.14        560  RecentFolders  |

2026-06-23 13:52:08,145 DEBUG TaskMaster.ApplicationGlobals - [continuation-resume] priorPhase=IntelConfig waitMs=0.6 resumeThreadId=1 resumeSyncContext=System.Windows.Threading.DispatcherSynchronizationContext staIsIdle=True staCpuUsage=0.000 staGuiActivity=0.0
2026-06-23 13:52:08,147 DEBUG TaskMaster.ApplicationGlobals - [continuation-resume] priorPhase=OlObjects waitMs=0.1 resumeThreadId=1 resumeSyncContext=System.Windows.Threading.DispatcherSynchronizationContext staIsIdle=True staCpuUsage=7.827 staGuiActivity=0.0
2026-06-23 13:52:08,894 DEBUG TaskMaster.ApplicationGlobals - [continuation-resume] priorPhase=ToDo waitMs=0.0 resumeThreadId=1 resumeSyncContext=System.Windows.Threading.DispatcherSynchronizationContext staIsIdle=False staCpuUsage=1.167 staGuiActivity=3.2
2026-06-23 13:52:09,255 DEBUG TaskMaster.ApplicationGlobals - [continuation-resume] priorPhase=AutoFile waitMs=1.1 resumeThreadId=1 resumeSyncContext=System.Windows.Threading.DispatcherSynchronizationContext staIsIdle=False staCpuUsage=1.430 staGuiActivity=22.2
2026-06-23 13:54:01,847 DEBUG TaskMaster.ApplicationGlobals - [continuation-resume] priorPhase=Engines waitMs=0.1 resumeThreadId=1 resumeSyncContext=System.Windows.Threading.DispatcherSynchronizationContext staIsIdle=False staCpuUsage=0.029 staGuiActivity=1.1

2026-06-23 13:54:01,859 INFO TaskMaster.ApplicationGlobals - [Startup timing]
|  0:00.07  LoadBasic    |
|  0:05.00  IntelConfig  |
|  0:00.00  OlObjects    |
|  0:00.74  ToDo         |
|  0:00.35  AutoFile     |
|  1:52.59  Engines      |
|  0:00.00  Events       |
|  1:58.79  TOTAL        |
```

