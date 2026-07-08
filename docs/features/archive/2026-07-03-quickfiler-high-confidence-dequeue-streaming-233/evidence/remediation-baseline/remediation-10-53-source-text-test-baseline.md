Timestamp: 2026-07-04T11-07-04:00
Command: Select-String -Path QuickFiler.Test\Controllers\*.cs -Pattern 'File\.ReadAllText|ReadControllerSource|AppDomain\.CurrentDomain\.BaseDirectory'
EXIT_CODE: 0
Output Summary:
- Matches were found in controller unit-test files.
- Planned remediation scope includes QuickFiler.Test/Controllers/QfcDatamodelTests.cs and QuickFiler.Test/Controllers/QfcQueuePurePathsTests.cs.
- Additional matches also exist in QfcFormControllerSeamTests.cs, QfcHighConfidencePreFilterTests.cs, and QfcItemController.FolderHandlingTests.cs; those files are not named for edits by P1-T1 or P1-T2 and will be recorded in the later check-only evidence disposition.

Matches:
- QuickFiler.Test/Controllers/QfcDatamodelTests.cs: ReadControllerSource helper, AppDomain.CurrentDomain.BaseDirectory, File.ReadAllText, and source reads for QfcDatamodel.cs and QfcDatamodel.QueueProcessing.cs.
- QuickFiler.Test/Controllers/QfcFormControllerSeamTests.cs: ReadControllerSource helper and File.ReadAllText for QfcFormController.Actions.cs.
- QuickFiler.Test/Controllers/QfcHighConfidencePreFilterTests.cs: ReadControllerSource helper, AppDomain.CurrentDomain.BaseDirectory, and File.ReadAllText for QfcHighConfidencePreFilter.cs.
- QuickFiler.Test/Controllers/QfcItemController.FolderHandlingTests.cs: ReadControllerSource helper, AppDomain.CurrentDomain.BaseDirectory, and File.ReadAllText for QfcItemController.FolderHandling.cs.
- QuickFiler.Test/Controllers/QfcQueuePurePathsTests.cs: ReadControllerSource helper, AppDomain.CurrentDomain.BaseDirectory, and File.ReadAllText.
