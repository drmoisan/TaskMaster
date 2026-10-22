Timestamp: 2026-08-25T14-18
Command: vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /TestCaseFilter:"FullyQualifiedName~Issue609_FolderPredictor" /InIsolation
ExpectedExitCode: 1
EXIT_CODE: 1
Output Summary: The single Issue609_FolderPredictor test failed as expected. FolderArray contained the full in-root value `\\mailbox@example.com\Archive\Clients\North` instead of projecting it to `Clients\North`. The required VSTest executable directory was added to the process PATH before invoking the recorded command; no source input or production code changed before this run.
