# Regression Test Fail-Before Evidence

Timestamp: 2026-03-25T00:00:00Z

## Tests Added

### P1-T1: QuickFileMetrics_WRITE_WhenGetCalendarReturnsNull_DoesNotThrow
- File: `QuickFiler.Test/Controllers/QfcHomeControllerTests.cs`
- Scenario: Mocks NameSpace.GetDefaultFolder().Folders with empty collection so Calendar.GetCalendar returns null. Calls QuickFileMetrics_WRITE and asserts no exception.
- Fail-before result: FAILED with System.NullReferenceException at QfcHomeController.cs line 419 (olEmailCalendar.Items.Add() where olEmailCalendar is null).

### P1-T2: GetMoveDiagnostics_WhenAppointmentIsNull_DoesNotThrow
- File: `QuickFiler.Test/Controllers/QfcCollectionControllerTests.cs`
- Scenario: Creates QfcCollectionController via FormatterServices.GetUninitializedObject, sets _itemGroupsToMove with one mock QfcItemGroup, calls GetMoveDiagnostics with null AppointmentItem ref.
- Fail-before result: FAILED with System.NullReferenceException at QfcCollectionController.cs line 2115 (olAppointment.Body where olAppointment is null).

### P1-T3: GetMoveDiagnostics_WhenAppointmentIsNull_ReturnsStringArray
- File: `QuickFiler.Test/Controllers/QfcCollectionControllerTests.cs`
- Scenario: Same setup as P1-T2, verifies result is not null.
- Fail-before result: FAILED with System.NullReferenceException at QfcCollectionController.cs line 2115.

## Command Used for Fail-Before Verification
Command: vstest.console.exe "QuickFiler.Test\bin\Debug\QuickFiler.Test.dll" /InIsolation /TestCaseFilter:"FullyQualifiedName~QfcCollectionControllerTests"
EXIT_CODE: 1 (2 failures)

Command: vstest.console.exe "QuickFiler.Test\bin\Debug\QuickFiler.Test.dll" /InIsolation /TestCaseFilter:"FullyQualifiedName~GetCalendarReturnsNull"
EXIT_CODE: Test Run Failed (1 failure)

## Summary
All three regression tests reproduce the bug as expected. Fail-before condition is confirmed.
Proceeding to fix implementation (P1-T3 and P1-T4 in the plan).
