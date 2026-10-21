# P2-T5 Test Project Build in the L4 Seam State

Timestamp: 2026-10-03T08-46
Command: msbuild UtilitiesCS.Test\UtilitiesCS.Test.csproj /t:Build /m /p:Configuration=Debug /p:Platform=AnyCPU (CMD-BUILD-TEST, TASKID p2-t5; resolved through vswhere; plus /nodeReuse:false; plus a normal-verbosity file logger; console stream discarded, figures read from the file logger)
EXIT_CODE: 0 (the printed MSBUILD_EXIT_CODE)
Output Summary: the test project, now including the L4 tests that call the four-parameter seam, built with zero errors; this closes the compile-red span P2-T1 to P2-T5.

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

Acceptance check: MSBUILD_EXIT_CODE 0 and ERROR_LINES 0; CSC_OUT_LINES 2 (at least 1); DLL_ADVANCED True. All three hold.
