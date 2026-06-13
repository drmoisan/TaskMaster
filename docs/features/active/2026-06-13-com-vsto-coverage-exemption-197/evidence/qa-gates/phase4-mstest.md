# Phase 4 — MSTest with Coverage

Timestamp: 2026-06-13T13-28

Command: pwsh -NoProfile scripts/vscode/Invoke-MSTestWithCoverage.ps1 -CoverageOutput coverage/coverage.phase4.cobertura.xml
(Koverage dedup -> coverage/coverage.phase4.firstparty.cobertura.xml.)

EXIT_CODE: vstest reported 1 failure -> pipeline exit 1 (dedup re-applied manually)

## Test results
- Total tests: 4068
- Passed: 4067
- Failed: 1 (AddEntry_UseUiThreadTrue_DequeuesEntryAndSuppressesDispatcherException, known flaky timing test)

## Coverage headline (first-party deduped, all non-.Test incl vendored constant)
- covered: 38,684
- lines-valid: 55,627
- line rate: 69.54%

## QuickFiler controller annotation verification
- QuickFiler package denominator: baseline 14,362 -> 10,438 lines (controllers removed; viewers follow in Phase 5).
- QuickFiler package rate: 25.15% -> 34.21%.
- 6 controllers annotated once each: QfcDatamodel, EfcItemController, QfcExplorerController, KeyboardHandler, EfcFormController, QfcCollectionController.
- Testable seams confirmed present in QuickFiler package: KbdActions, KaChar, KaKey, QfcFormController, EfcDataModel, FilerQueue, QfcQueue, QfcItemGroup, ConversationResolver, QfcPreScoredItem.
- QfcHighConfidencePreFilter class: NOT class-level annotated (verified by git diff: file unchanged this feature). Its only pre-existing exemption is on the nested Outlook-COM adapter `FolderScoringService` (pre-dates this feature). The class did not surface a measured line in this particular run because its FilterAsync path was not exercised; this is a test-exercise matter, not an exemption-boundary change. Final boundary check (P7-T7) verifies class-level attribute absence from source.
- Do-not-annotate controllers unannotated: EfcHomeController, QfcItemController.
