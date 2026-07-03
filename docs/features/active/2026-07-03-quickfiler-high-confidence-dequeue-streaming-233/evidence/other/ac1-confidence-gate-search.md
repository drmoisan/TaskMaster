Timestamp: 2026-07-03T17:44:54.5907363-04:00
Command: Select-String -Path (Get-ChildItem -Recurse -File -Include '*.cs' | Select-Object -ExpandProperty FullName) -Pattern 'HighConfidenceThreshold|RemoveBelowThreshold|ApplyHighConfidenceFilter|Math.Round\(.*threshold \* 1000|TopFolderScore'
EXIT_CODE: 0
Output Summary:
- The repo-wide search found one live issue #233 dequeue-layer threshold gate:
  - `QuickFiler/Controllers/QfcStreamingDequeueConfidenceGate.cs`: computes `_cutoff = (long)Math.Round(threshold * 1000, 0)` and accepts dequeued candidates with `score >= _cutoff`.
- The live high-confidence dequeue path reaches that gate through:
  - `QuickFiler/Controllers/QfcDatamodel.QueueProcessing.cs`: passes `_globals.QfSettings.HighConfidenceThreshold` into `QfcStreamingDequeueConfidenceGate` when `HighConfidenceModeEnabled == true`.
- The search also found legacy/dormant issue #171 members that are not the live #233 enforcement gate:
  - `QuickFiler/Controllers/QfcCollectionController.cs`: `RemoveBelowThresholdAsync(double threshold)` remains as the legacy post-display removal helper.
  - `QuickFiler/Controllers/QfcFormController.Actions.cs`: `ApplyHighConfidenceFilterAsync(IQfcCollectionController groups)` remains as a dormant helper and is not invoked by the live `LoadItemsAsync(IList<MailItem>, ProgressTracker)` path.
  - `QuickFiler/Controllers/QfcHighConfidencePreFilter.cs`: remains available as pre-scored filtering code, but the issue #233 live first-page and later-page path uses the datamodel dequeue layer.
- The search found settings, ribbon, and interface accessor/documentation matches that do not perform live candidate filtering:
  - `TaskMaster/AppGlobals/AppQuickFilerSettings.cs`
  - `TaskMaster/Properties/Settings.Designer.cs`
  - `TaskMaster/Ribbon/RibbonController.Intelligence.cs`
  - `TaskMaster/Ribbon/RibbonViewer.cs`
  - `UtilitiesCS/Interfaces/IGlobals/IAppQuickFilerSettings.cs`
  - `QuickFiler/Interfaces/IQfcCollectionController.cs`
  - `QuickFiler/Interfaces/IQfcItemController.cs`
  - `QuickFiler/Controllers/QfcItemController.cs`
- Test matches were excluded from the live gate count.
- Classification result: PASS. There is exactly one live issue #233 confidence threshold gate, and it is located in the dequeue layer.
