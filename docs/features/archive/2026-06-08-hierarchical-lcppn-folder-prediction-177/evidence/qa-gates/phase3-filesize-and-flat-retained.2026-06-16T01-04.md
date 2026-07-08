# Phase 3 — Flat-Rebuild Retained + File-Size Cap (Cycle 3, #177)

Timestamp: 2026-06-16T01-04
Command: grep flat-rebuild lines; wc -l <touched/new files>; git diff HEAD -- AppAutoFileObjects.cs
EXIT_CODE: 0

Output Summary:
INV-3 flat rebuild retained — the always-on flat Manager["Folder"] rebuild + serialize is intact and
unconditional in BuildClassifiersAsync:
- OlFolderClassifierGroup.cs:294  classifierGroup.Serialize();
- OlFolderClassifierGroup.cs:296  Globals.AF.Manager["Folder"] = classifierGroup.ToAsyncLazy();
The LCPPN build block is additive and nested under the same Manager.Configuration "Folder" branch;
the flat write is not removed or made conditional.

File sizes (all <= 500):
- OlFolderClassifierGroup.cs: 345
- LcppnFolderPredictorStore.cs (new): 45
- AppAutoFileObjects.FolderPredictorLoad.cs (new): 90
- AppAutoFileObjects.cs: 849 (baseline 847)

AppAutoFileObjects.cs grew by exactly the permitted wiring + partial keyword (3 added lines vs HEAD):
+    public partial class AppAutoFileObjects : IAppAutoFileObjects   (partial keyword)
+                LoadFolderPredictorAsync(),                          (LoadParallelAsync tasks list)
+            await LoadFolderPredictorAsync();                        (LoadSequentialAsync await)
No other logic was added to AppAutoFileObjects.cs; the LoadFolderPredictorAsync body and the
UseLcppnPredictor accessor live in AppAutoFileObjects.FolderPredictorLoad.cs.
