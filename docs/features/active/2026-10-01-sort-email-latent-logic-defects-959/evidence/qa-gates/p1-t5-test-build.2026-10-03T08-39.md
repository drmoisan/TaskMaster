# P1-T5 Test Project Build Against the Unmodified Production Tree

Timestamp: 2026-10-03T08-39
Command: CMD-HASH on PATHS-A; then msbuild UtilitiesCS.Test\UtilitiesCS.Test.csproj /t:Build /m /p:Configuration=Debug /p:Platform=AnyCPU (CMD-BUILD-TEST, TASKID p1-t5; resolved through vswhere; plus /nodeReuse:false; plus a normal-verbosity file logger coverage\logs\p1-t5.msbuild.log; the console stream was discarded and every figure is read from the file logger)
EXIT_CODE: 0 (scoped to the CMD-BUILD-TEST invocation; the printed MSBUILD_EXIT_CODE)
Output Summary: production file A is unchanged; the test project with the two new test files built with zero errors and the output assembly advanced.

- A-HASH: A619C1A7C1B98F50B39DB066CA8C4F081410AFB2CB587AA9894C919F02D8305B (equals PRE-EDIT-HASH-A)
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

Acceptance check: A-HASH equals PRE-EDIT-HASH-A; MSBUILD_EXIT_CODE 0 and ERROR_LINES 0; CSC_OUT_LINES 2 and ZERO_ERRORS_LINES 1 (each at least 1); DLL_ADVANCED True. All four hold.
