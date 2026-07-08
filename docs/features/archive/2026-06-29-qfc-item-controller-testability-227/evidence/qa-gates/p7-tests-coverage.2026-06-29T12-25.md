# Phase 7 — Tests + Coverage (P7-T12)
Timestamp: 2026-06-29T12-25
Command: vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /EnableCodeCoverage /InIsolation
EXIT_CODE: 0
Output Summary:
- Total tests: 224; Passed: 224; Failed: 0 (baseline 201 preserved + 23 new tests, all passing).
- QfcItemController production coverage (Cobertura sequence-point basis, excluding *Tests.cs files): 388 / 3293 = 11.78% (up from 7.54% baseline).
- Per-cluster (covered/total, %): Conversation 70/226 30.97; EventWiring 186/598 31.10; main(Properties/INotify) 54/170 31.76; FolderHandling 24/242 9.92; MailActions 24/250 9.60; Navigation 28/350 8.00; EventHandlers 0/248 0; FocusAndTheme 0/483 0; ViewerSetup 0/312 0; Initialization 2/414 0.48.
- The 0%/low clusters are predominantly COM/Outlook/WinForms-bound (UI event handlers, theme, control-tree setup, COM init) and are the [ExcludeFromCodeCoverage] targets for Phase 8; the testable seams (Conversation routing, EventWiring registration, KbdExecuteAsync, PackageItems, MarkItemForDeletion, Properties/INotifyPropertyChanged, PopulateAndSelectFolder) now carry direct unit coverage.
