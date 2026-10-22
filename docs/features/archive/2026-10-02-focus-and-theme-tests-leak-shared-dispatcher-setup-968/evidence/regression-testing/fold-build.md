# Fold build (issue #968, task P4-T10)

Timestamp: 2026-10-03T03-08
Command: pwsh -NoProfile -Command '<CMD-BUILD payload>' with TASKID p4-t10; the payload is identical, line for line, to the CMD-BUILD payload transcribed in full in FEATURE/evidence/regression-testing/fail-before-build.md except that the log is `coverage\logs\p4-t10.msbuild.log`
Canonical command: msbuild TaskMaster.sln /t:Build /m /p:Configuration=Debug "/p:Platform=Any CPU" (resolved through vswhere against WORKTREE/TaskMaster.sln)
EXIT_CODE: 0
Output Summary:
- WORKTREE-LEAF: agent-a291a7fbabf9d0229
- MSBUILD_EXIT_CODE: 0
- ERRORS: 0
- TEST_DLL_ADVANCED: True
- PROD_DLL_ADVANCED: True
- CSC_OUT_QUICKFILER: 2
- CSC_OUT_QUICKFILER_TEST: 2
- First compile proof that no surviving code referenced a removed QfcDatamodel member: no CS0103 or CS0117 (ERRORS 0).
