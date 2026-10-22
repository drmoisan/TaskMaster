# P1-T10 Format A and Build the Test Project

Timestamp: 2026-10-03T08-42
Command: CMD-SCOPED-FORMAT (PATHS-A, TASKID p1-t10): dotnet tool run csharpier format then dotnet tool run csharpier check over UtilitiesCS\EmailIntelligence\EmailParsingSorting\SortEmail.AttachmentSaving.cs; then msbuild UtilitiesCS.Test\UtilitiesCS.Test.csproj /t:Build /m /p:Configuration=Debug /p:Platform=AnyCPU (CMD-BUILD-TEST, TASKID p1-t10; resolved through vswhere; plus /nodeReuse:false; plus a normal-verbosity file logger; console stream discarded, figures read from the file logger)
EXIT_CODE: 0 (scoped to the CMD-BUILD-TEST invocation; the printed MSBUILD_EXIT_CODE)
Output Summary: A was already in CSharpier form (hash unchanged); the test project built with zero errors and the assembly advanced.

Format:
- BEFORE UtilitiesCS\EmailIntelligence\EmailParsingSorting\SortEmail.AttachmentSaving.cs = 2A145BF2BD43921CF6F9B624501523C0C5AFC9D155BB743DA2487315966BBC79
- Formatter summary (observation): Formatted 1 files in 1139ms.
- FORMAT_EXIT_CODE: 0
- AFTER UtilitiesCS\EmailIntelligence\EmailParsingSorting\SortEmail.AttachmentSaving.cs = 2A145BF2BD43921CF6F9B624501523C0C5AFC9D155BB743DA2487315966BBC79
- Check summary: Checked 1 files in 475ms.
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
