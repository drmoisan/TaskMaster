Timestamp: 2026-08-25T14-48
Command: & 'C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe' UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /TestCaseFilter:"FullyQualifiedName~Issue609_FolderPredictor_ProjectsCaseVariantInRootFullSuggestionPath" /InIsolation
ExpectedExitCode: 1
EXIT_CODE: 1
Output Summary: The expected fail-before regression executed and failed. The assertion expected `Clients\North`, but FolderArray returned `\\MAILBOX@EXAMPLE.COM\archive\Clients\North` at suggestion index 1. The test runner's absolute path was required because `vstest.console.exe` is not on PATH.
