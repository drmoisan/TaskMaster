Timestamp: 2026-03-27T13:01:15-04:00
Command: pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-MSTest.ps1 -SearchRoot QuickFiler.Test -Configuration Debug
EXIT_CODE: 1
Output Summary:
- The approved focused MSTest command returned a non-zero exit code in the pre-fix state.
- The repository script failed before dispatching tests for a single discovered assembly with: `The property 'Count' cannot be found on this object. Verify that the property exists.`
- To capture the actual regression named in this plan task, the compiled `QuickFiler.Test.dll` assembly was executed immediately afterward with Visual Studio Test Platform resolved via `vswhere.exe`.
- Failing test: `QuickFiler.Controllers.Tests.KbdActionsTests.Add_WhenSourceAndStoredKeysAreDistinct_DoesNotTreatSubstringAsDuplicate`.
- Failure signal: `Did not expect any exception ... but found System.ArgumentException: Cannot add key because it already exists. Key 1 SourceId Collection` from `QuickFiler.Controllers.KbdActions.Add`.
- This reproduces the substring-based storage collision before the production fix.
