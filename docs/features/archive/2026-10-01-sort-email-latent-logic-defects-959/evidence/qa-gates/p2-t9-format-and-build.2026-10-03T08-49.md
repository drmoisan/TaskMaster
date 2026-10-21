# P2-T9 Format U and TST1 and Build the Test Project

Timestamp: 2026-10-03T08-49
Command: CMD-SCOPED-FORMAT (PATHS-U, PATHS-TST1; TASKID p2-t9): dotnet tool run csharpier format then dotnet tool run csharpier check over the two files; then msbuild UtilitiesCS.Test\UtilitiesCS.Test.csproj /t:Build /m /p:Configuration=Debug /p:Platform=AnyCPU (CMD-BUILD-TEST, TASKID p2-t9; resolved through vswhere; plus /nodeReuse:false; plus a normal-verbosity file logger; console stream discarded, figures read from the file logger)
EXIT_CODE: 0 (scoped to the CMD-BUILD-TEST invocation; the printed MSBUILD_EXIT_CODE)
Output Summary: the formatter rewrote U (line endings of the Write-tool output); TST1 was already formatted; the read-only check passed; the test project built with zero errors (no CS0103 for SanitizeArray, so no caller survives).

Format:
- BEFORE UtilitiesCS\EmailIntelligence\EmailParsingSorting\SortEmail.UndoAndMoveLog.cs = FFBF947FE5E2175C13BE3E879CA46358A153D0B1225BC5FCDBCB1CAF58575724
- BEFORE UtilitiesCS.Test\EmailIntelligence\SortEmail_Tests.cs = 8F34C44EA2C8F44382B1290B1AFA4733B180FDF031CC6234AF202A5205590D2A
- Formatter summary (observation): Formatted 2 files in 2056ms.
- FORMAT_EXIT_CODE: 0
- AFTER UtilitiesCS\EmailIntelligence\EmailParsingSorting\SortEmail.UndoAndMoveLog.cs = 228B900DD4A7FAE71DE1A08F081AC72DC37265C32B853D9487637860287562B4
- AFTER UtilitiesCS.Test\EmailIntelligence\SortEmail_Tests.cs = 8F34C44EA2C8F44382B1290B1AFA4733B180FDF031CC6234AF202A5205590D2A
- Check summary: Checked 2 files in 792ms.
- CHECK_EXIT_CODE: 0

Build:
- MSBUILD_EXIT_CODE: 0
- CSC_OUT_LINES: 2
- ZERO_ERRORS_LINES: 1
- ERROR_LINES: 0
- ERROR_LINES_TEST_FILES: 0
- ERROR_LINES_OTHER_FILES: 0
- MISSING_SAVEATTACHMENTASYNC_6: 0
- MISSING_SAVECASEASYNC_6: 0
- MISSING_SAVEATTACHMENT_4: 0
- MISSING_REDIRECTSAVEFOLDER: 0
- ERROR_CODES: (none)
- DLL_ADVANCED: True

Acceptance check: FORMAT_EXIT_CODE 0 and CHECK_EXIT_CODE 0; MSBUILD_EXIT_CODE 0 and ERROR_LINES 0; DLL_ADVANCED True. All three hold.
