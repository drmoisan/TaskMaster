# Baseline — File Line Counts (P0-T5)

Timestamp: 2026-06-28T19-05
Command: wc -l <files>

| File | Lines |
|------|-------|
| QuickFiler/Controllers/QfcHomeController.cs | 454 |
| QuickFiler/Controllers/QfcHomeController.Metrics.cs | 226 |
| QuickFiler/Controllers/QfcDatamodel.cs | 432 |
| QuickFiler/Controllers/QfcDatamodel.FrameBuilding.cs | 154 |
| QuickFiler/Controllers/QfcDatamodel.QueueProcessing.cs | 146 |
| QuickFiler.Test/Controllers/QfcHomeControllerMetricsTests.cs | 241 |
| QuickFiler.Test/Controllers/QfcDatamodelTests.cs | 177 |

All files <= 500. QfcHomeController.cs at 454 has limited headroom; seam property is added to QfcHomeController.Metrics.cs (226) per plan P2-T2.
