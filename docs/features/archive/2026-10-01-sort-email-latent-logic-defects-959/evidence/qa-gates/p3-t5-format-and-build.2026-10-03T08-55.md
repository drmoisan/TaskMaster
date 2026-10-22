# P3-T5 Format T and Build the Test Project

Timestamp: 2026-10-03T08-55
Command: CMD-SCOPED-FORMAT (PATHS-T; TASKID p3-t5): dotnet tool run csharpier format then dotnet tool run csharpier check over UtilitiesCS\EmailIntelligence\EmailParsingSorting\SortEmail.TrySaveAttachment.cs; then msbuild UtilitiesCS.Test\UtilitiesCS.Test.csproj /t:Build /m /p:Configuration=Debug /p:Platform=AnyCPU (CMD-BUILD-TEST, TASKID p3-t5; resolved through vswhere; plus /nodeReuse:false; plus a normal-verbosity file logger; console stream discarded, figures read from the file logger)
EXIT_CODE: 0 (scoped to the CMD-BUILD-TEST invocation; the printed MSBUILD_EXIT_CODE)
Output Summary: the formatter rewrote T (line endings of the Write-tool output); the read-only check passed; the test project built with zero errors.

Format:
- BEFORE UtilitiesCS\EmailIntelligence\EmailParsingSorting\SortEmail.TrySaveAttachment.cs = 63141294DEF46D2B34B4A8FB01A894C9DF2CF702C579EC1065CF3F0F35FBFECC
- Formatter summary (observation): Formatted 1 files in 1147ms.
- FORMAT_EXIT_CODE: 0
- AFTER UtilitiesCS\EmailIntelligence\EmailParsingSorting\SortEmail.TrySaveAttachment.cs = AA6C5D7347969E4729692C479EC81B8889FDAFA98B58C50DC21CDB8ADB135C12
- Check summary: Checked 1 files in 472ms.
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
