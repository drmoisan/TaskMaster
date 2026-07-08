# CalcMaxSeedLength After Fix — Finding A test 2 (Cycle 5, Issue #181)

Timestamp: 2026-06-08T21-53

Command: `vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /Tests:CalcMaxSeedLength_WhenInitialized_ShouldSubtractComponentLengths`
(VS18 vstest.console.exe; MSYS_NO_PATHCONV=1.)

EXIT_CODE: 0

Output Summary:
- Total tests: 1. Passed: 1. Failed: 0.
- `CalcMaxSeedLength_WhenInitialized_ShouldSubtractComponentLengths` PASSED. With the uncorrupted `FolderPath` (`C:\output`, length 9 instead of the corrupted `C:\` length 3), `CalcMaxSeedLength()` now returns `MAX_PATH - "C:\output".Length - ".json".Length - "_bk".Length == 239`, matching the expectation. Previously returned 245 (the 6-char inflation caused by the shortened corrupted FolderPath).
