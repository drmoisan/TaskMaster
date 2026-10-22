# P3-T2 Format TST2 and Build the Test Project

Timestamp: 2026-10-03T08-52
Command: CMD-SCOPED-FORMAT (PATHS-TST2; TASKID p3-t2): dotnet tool run csharpier format then dotnet tool run csharpier check over UtilitiesCS.Test\EmailIntelligence\SortEmail_TrySaveAttachment_Tests.cs; then msbuild UtilitiesCS.Test\UtilitiesCS.Test.csproj /t:Build /m /p:Configuration=Debug /p:Platform=AnyCPU (CMD-BUILD-TEST, TASKID p3-t2; resolved through vswhere; plus /nodeReuse:false; plus a normal-verbosity file logger; console stream discarded, figures read from the file logger)
EXIT_CODE: 0 (scoped to the CMD-BUILD-TEST invocation; the printed MSBUILD_EXIT_CODE)
Output Summary: the formatter wrapped the new T12 lines (the diff against the merge base stays insert-only: 37 added, 0 deleted); the read-only check passed; T12 compiles against the unmodified five-argument overload and the build has zero errors.

Format:
- BEFORE UtilitiesCS.Test\EmailIntelligence\SortEmail_TrySaveAttachment_Tests.cs = B6EF9164D8413AA900311CA6118B9F305F32718C5F7F816FFB1CA849604E8ECC
- Formatter summary (observation): Formatted 1 files in 1194ms.
- FORMAT_EXIT_CODE: 0
- AFTER UtilitiesCS.Test\EmailIntelligence\SortEmail_TrySaveAttachment_Tests.cs = 0C2A1463F2810F281A33190059A71260B513FF178FF57BB12E67E3B53C9E5E8D
- Check summary: Checked 1 files in 492ms.
- CHECK_EXIT_CODE: 0
- Observation after the format: git diff --numstat 94287369908cc920b21b0e3256314f988ad7d2f5 -- UtilitiesCS.Test/EmailIntelligence/SortEmail_TrySaveAttachment_Tests.cs reads 37 0 (insert-only)

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
