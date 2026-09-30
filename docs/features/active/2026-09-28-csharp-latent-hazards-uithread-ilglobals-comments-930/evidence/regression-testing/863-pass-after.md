# #863 pass-after ([P1-T12])

Timestamp: 2026-09-29T09-16
Command: CMD-TEST-SCOPED with STAGE = 863-green and FILTER = FullyQualifiedName~UtilitiesCS.Test.NewtonsoftHelpers.SDILReader.ILGlobals_Tests: pwsh -NoProfile -Command '$vswhere = Join-Path ${env:ProgramFiles(x86)} "Microsoft Visual Studio\Installer\vswhere.exe"; $vstest = & $vswhere -latest -products * -find "Common7\IDE\Extensions\TestPlatform\vstest.console.exe" | Select-Object -First 1; New-Item -ItemType Directory -Force -Path coverage/test-results/930-863-green | Out-Null; & $vstest UtilitiesCS.Test/bin/Debug/UtilitiesCS.Test.dll /Settings:scripts/vscode/TaskMaster.cli.runsettings /InIsolation "/logger:console;verbosity=normal" "/Logger:trx;LogFileName=930-863-green.trx" /ResultsDirectory:coverage/test-results/930-863-green "/TestCaseFilter:FullyQualifiedName~UtilitiesCS.Test.NewtonsoftHelpers.SDILReader.ILGlobals_Tests" 2>&1 | Tee-Object -FilePath coverage/930-863-green-tests.log; "VSTEST_EXIT=$LASTEXITCODE"'
Command: CMD-TRX-SUMMARY with STAGE = 863-green
EXIT_CODE: 0
Output Summary:
- VSTEST_EXIT=0; console: Test Run Successful. Total tests: 15. Passed: 15.
- Test run outcome: Completed
- Total 15, executed 15, passed 15, failed 0.
- Failed tests: none
- RESULT Passed PublicStaticFields_AreAllInitOnly
- RESULT Passed PublicStaticFields_AreExactlyTheTwoOpCodeTables
- Cache_IsInitialized is absent from every RESULT line (15 RESULT lines, all Passed: MultiByteOpCodes_IsPublishedWithFullLength, ProcessSpecialTypes_Int32_ReturnsInt, ProcessSpecialTypes_Int_ReturnsInt, MultiByteOpCodes_FieldIsInitOnly, SingleByteOpCodes_IsPublishedWithFullLength, ProcessSpecialTypes_SystemDotstring_ReturnsString, ProcessSpecialTypes_SystemInt32_ReturnsInt, OpCodeTables_ContainEveryOpCodeDeclaredOnOpCodes, SingleByteOpCodes_FieldIsInitOnly, PublicStaticFields_AreExactlyTheTwoOpCodeTables, ProcessSpecialTypes_UnknownType_ReturnsSameString, ProcessSpecialTypes_StringAlone_ReturnsString, LoadOpCodes_DoesNotRepublishPublishedTables, ProcessSpecialTypes_SystemString_ReturnsString, PublicStaticFields_AreAllInitOnly).
- SEQUENCE_FILES=0 (informational; the scoped runs attach no blame collector)
