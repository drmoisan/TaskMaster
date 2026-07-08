Timestamp: 2026-06-23T10-55
Command: Maintainer-provided Outlook startup debug-output capture from Codex attachment pasted-text.txt.
EXIT_CODE: 0

# Runtime Capture: Debugger-Attached Outlook Startup

## Summary

This capture contains the Phase 1 `[continuation-resume]` probe output for issue #211.
It is useful runtime evidence that the probe emits the expected fields, but it does not
satisfy AC5 because the output appears to be debugger-attached rather than a
non-debugger DebugView or OutputDebugString capture.

Debugger-attached indicators in the capture:

- Visual Studio output format is present (`'outlook.exe' (CLR v4.0.30319: ...)` module-load lines).
- TaskMaster assemblies report `Symbols loaded`.
- `Microsoft.VisualStudio.DesignTools.WpfTap.dll` is loaded from a Visual Studio installation path.

## Continuation Probe Results

The captured continuation waits were:

| Prior Phase | waitMs | resumeThreadId | resumeSyncContext | staIsIdle | staCpuUsage | staGuiActivity |
| --- | ---: | ---: | --- | --- | ---: | ---: |
| IntelConfig | 16.8 | 1 | System.Windows.Threading.DispatcherSynchronizationContext | True | 0.012 | 0.0 |
| OlObjects | 3.6 | 1 | System.Windows.Threading.DispatcherSynchronizationContext | True | 4.152 | 0.0 |
| ToDo | 0.4 | 1 | System.Windows.Threading.DispatcherSynchronizationContext | True | 2.969 | 0.3 |
| AutoFile | 1.6 | 1 | System.Windows.Threading.DispatcherSynchronizationContext | False | 1.666 | 0.8 |

No `Engines` continuation line appears in the provided pasted text.

## Interpretation

The IntelConfig continuation wait in this capture is `16.8 ms`, which is below the
Phase 2 threshold of `> 5000 ms`. On its own, this capture does not justify the
Phase 2 `ConfigureAwait(false)` plus explicit UI-thread re-marshal change.

Because the capture appears debugger-attached, it does not close AC5. The next
required evidence remains a non-debugger cold-start capture that includes all
`[continuation-resume]` lines.

## Relevant Raw Lines

```text
2026-06-23 10:55:20,848 INFO UtilitiesCS.EmailIntelligence.IntelligenceConfig - [IntelConfig timing]
GetSerializedConfigurations read: durationMs=7.34; entries=3
|   116.85        678  People         |
|     1.22        702  StoresWrapper  |
|     0.81        560  RecentFolders  |

2026-06-23 10:57:12,138 DEBUG TaskMaster.ApplicationGlobals - [continuation-resume] priorPhase=IntelConfig waitMs=16.8 resumeThreadId=1 resumeSyncContext=System.Windows.Threading.DispatcherSynchronizationContext staIsIdle=True staCpuUsage=0.012 staGuiActivity=0.0
2026-06-23 10:57:12,146 DEBUG TaskMaster.ApplicationGlobals - [continuation-resume] priorPhase=OlObjects waitMs=3.6 resumeThreadId=1 resumeSyncContext=System.Windows.Threading.DispatcherSynchronizationContext staIsIdle=True staCpuUsage=4.152 staGuiActivity=0.0
2026-06-23 10:57:12,480 DEBUG TaskMaster.ApplicationGlobals - [continuation-resume] priorPhase=ToDo waitMs=0.4 resumeThreadId=1 resumeSyncContext=System.Windows.Threading.DispatcherSynchronizationContext staIsIdle=True staCpuUsage=2.969 staGuiActivity=0.3
2026-06-23 10:57:12,874 DEBUG TaskMaster.ApplicationGlobals - [continuation-resume] priorPhase=AutoFile waitMs=1.6 resumeThreadId=1 resumeSyncContext=System.Windows.Threading.DispatcherSynchronizationContext staIsIdle=False staCpuUsage=1.666 staGuiActivity=0.8
```

