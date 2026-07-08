# Baseline — Site Reconfirmation (P0-T3)

Timestamp: 2026-06-28T19-02
Command: grep -n -E "DateTime\.Now|Task\.Delay" in the four target files

Eight active (non-commented) banned-API sites confirmed. No line drift from spec.md.

| # | File | Line | Exact source text | Kind |
|---|------|------|-------------------|------|
| 1 | QuickFiler/Controllers/QfcDatamodel.FrameBuilding.cs | 43 | `await Task.Delay(5);` | Task.Delay |
| 2 | QuickFiler/Controllers/QfcDatamodel.QueueProcessing.cs | 142 | `await Task.Delay(200);` | Task.Delay |
| 3 | QuickFiler/Controllers/QfcHomeController.cs | 75 | `$"{DateTime.Now.ToString("mm:ss.fff")} "` | DateTime.Now |
| 4 | QuickFiler/Controllers/QfcHomeController.Metrics.cs | 20 | `var now = DateTime.Now;` | DateTime.Now |
| 5 | QuickFiler/Controllers/QfcHomeController.Metrics.cs | 100 | `curDateText = DateTime.Now.ToString("MM/dd/yyyy");` | DateTime.Now |
| 6 | QuickFiler/Controllers/QfcHomeController.Metrics.cs | 102 | `curTimeText = DateTime.Now.ToString("hh:mm");` | DateTime.Now |
| 7 | QuickFiler/Controllers/QfcHomeController.Metrics.cs | 114 | `OlEndTime = DateTime.Now;` | DateTime.Now |
| 8 | QuickFiler/Controllers/QfcHomeController.Metrics.cs | 214 | `await Task.Delay(20);` | Task.Delay |

Line-drift assessment: NONE. All eight sites match spec.md line numbers exactly.

Commented-out references (out of scope; not to be modified):
- FrameBuilding.cs lines 54, 61, 76, 79 (commented logger.Debug DateTime.Now)
- QfcHomeController.cs lines 43, 262, 276, 281, 287 (commented logger.Debug DateTime.Now)
- Metrics.cs lines 21, 22 (commented curDateText/curTimeText DateTime.Now)

Binary outcome: artifact lists eight confirmed active sites.
