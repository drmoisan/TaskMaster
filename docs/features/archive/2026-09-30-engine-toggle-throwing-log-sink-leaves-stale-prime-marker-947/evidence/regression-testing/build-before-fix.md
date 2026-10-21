# Regression Testing: Build Before the Fix (P1-T5)

Timestamp: 2026-10-01T17-50
Task: P1-T5
Command: msbuild TaskMaster.sln /t:Build /m /p:Configuration=Debug "/p:Platform=Any CPU" (resolved through vswhere, plus /nodeReuse:false and a normal-verbosity file logger under the ignored coverage directory)
EXIT_CODE: 0

Output Summary:
- MSBUILD_EXIT_CODE: 0
- ERRORS: 0
- TEST_DLL_ADVANCED: True
- CSC_OUT_TASKMASTER_TEST: 2 (at least 1; the compiler ran for TaskMaster.Test with the new partial and its compile entry)
- CSC_OUT_TASKMASTER: 0 (recorded; not gated at this task; the production file is unchanged, so its project was up to date)
- The new partial compiles against the unchanged production file, so the P1-T6 failures are assertion failures rather than compile errors.
- Result: P1-T5 acceptance holds; no NEW PARTIAL DOES NOT COMPILE.

The msbuild log stays under the git-ignored coverage directory and is not copied into the feature folder.
