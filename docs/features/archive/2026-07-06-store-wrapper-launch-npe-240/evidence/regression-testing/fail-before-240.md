# Fail-Before Evidence (Issue #240, Phase 1 Red)

Timestamp: 2026-07-06T07-25

Command: `vstest.console.exe UtilitiesCS.Test/bin/Debug/UtilitiesCS.Test.dll /TestCaseFilter:"FullyQualifiedName~Launch_WhenStoresWrapperIsNull_ShowsUserMessageAndDoesNotThrowOrOpenViewer|FullyQualifiedName~Launch_WhenStoresListIsNull_ShowsUserMessageAndDoesNotThrowOrOpenViewer" /InIsolation`

EXIT_CODE: 1

Output Summary: Test Run Failed. Total tests: 2, Failed: 2, Passed: 0. Both regression tests fail against the pre-fix production code, reproducing the issue #240 crash path:

- `Launch_WhenStoresWrapperIsNull_ShowsUserMessageAndDoesNotThrowOrOpenViewer` fails with an unhandled `System.NullReferenceException` at `StoreWrapperController.cs:line 52` (`Model.Stores.Select(...)` on a null `Model`), matching the exact stack trace reported in `issue.md`.
- `Launch_WhenStoresListIsNull_ShowsUserMessageAndDoesNotThrowOrOpenViewer` fails with an unhandled `System.ArgumentNullException` ("Value cannot be null. Parameter name: source") at the same line 52, because `Enumerable.Select` on a null `IEnumerable<T>` (`Model.Stores` is null while `Model` itself is non-null) throws `ArgumentNullException` rather than `NullReferenceException`. This is a more precise characterization than the plan text's blanket "both fail with NullReferenceException": both scenarios reproduce an unhandled, uncaught exception from the same unguarded `Launch()` code path, satisfying AC2's "does not throw" requirement equally; only the concrete CLR exception type differs for the null-list case.

Both failures confirm the pre-fix defect and establish the fail-before baseline required by AC3.
