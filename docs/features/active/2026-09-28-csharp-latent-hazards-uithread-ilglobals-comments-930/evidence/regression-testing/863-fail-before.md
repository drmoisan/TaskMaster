# #863 fail-before ([P1-T9], expect-fail)

Timestamp: 2026-09-29T09-14
Command: CMD-TEST-SCOPED with STAGE = 863-red and FILTER = FullyQualifiedName~UtilitiesCS.Test.NewtonsoftHelpers.SDILReader.ILGlobals_Tests: pwsh -NoProfile -Command '$vswhere = Join-Path ${env:ProgramFiles(x86)} "Microsoft Visual Studio\Installer\vswhere.exe"; $vstest = & $vswhere -latest -products * -find "Common7\IDE\Extensions\TestPlatform\vstest.console.exe" | Select-Object -First 1; New-Item -ItemType Directory -Force -Path coverage/test-results/930-863-red | Out-Null; & $vstest UtilitiesCS.Test/bin/Debug/UtilitiesCS.Test.dll /Settings:scripts/vscode/TaskMaster.cli.runsettings /InIsolation "/logger:console;verbosity=normal" "/Logger:trx;LogFileName=930-863-red.trx" /ResultsDirectory:coverage/test-results/930-863-red "/TestCaseFilter:FullyQualifiedName~UtilitiesCS.Test.NewtonsoftHelpers.SDILReader.ILGlobals_Tests" 2>&1 | Tee-Object -FilePath coverage/930-863-red-tests.log; "VSTEST_EXIT=$LASTEXITCODE"'
Command: CMD-TRX-SUMMARY with STAGE = 863-red
EXIT_CODE: 1
ExpectedExitCode: 1
Output Summary:
- VSTEST_EXIT=1
- Console failure messages: PublicStaticFields_AreAllInitOnly: "Expected fields to contain only items matching field.IsInitOnly ... but {System.Collections.Generic.Dictionary`2[System.Int32,System.Object] Cache, System.Reflection.Module[] modules} do(es) not match." PublicStaticFields_AreExactlyTheTwoOpCodeTables: "... but found extraneous items "Cache" (at index 2), "modules" (at index 3)".
- Test run outcome: Failed
- Total 15, executed 15, passed 13, failed 2.
- Failed tests: PublicStaticFields_AreExactlyTheTwoOpCodeTables, PublicStaticFields_AreAllInitOnly
- RESULT Passed (13): ProcessSpecialTypes_UnknownType_ReturnsSameString, OpCodeTables_ContainEveryOpCodeDeclaredOnOpCodes, LoadOpCodes_DoesNotRepublishPublishedTables, ProcessSpecialTypes_Int32_ReturnsInt, ProcessSpecialTypes_Int_ReturnsInt, ProcessSpecialTypes_SystemDotstring_ReturnsString, MultiByteOpCodes_IsPublishedWithFullLength, SingleByteOpCodes_IsPublishedWithFullLength, ProcessSpecialTypes_SystemInt32_ReturnsInt, ProcessSpecialTypes_StringAlone_ReturnsString, MultiByteOpCodes_FieldIsInitOnly, ProcessSpecialTypes_SystemString_ReturnsString, SingleByteOpCodes_FieldIsInitOnly
- RESULT Failed (2): PublicStaticFields_AreExactlyTheTwoOpCodeTables, PublicStaticFields_AreAllInitOnly
- SEQUENCE_FILES=0 (informational; the scoped runs attach no blame collector)

ProductionSourceState: `git diff --stat ac819907f479ee18026993054e714dc2e056142f -- "UtilitiesCS/NewtonsoftHelpers/SDIL Reader/ILGlobals.cs"` printed nothing; the production source is unmodified.
