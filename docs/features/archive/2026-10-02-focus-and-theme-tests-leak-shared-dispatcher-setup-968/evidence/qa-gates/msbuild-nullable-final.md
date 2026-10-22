# Final TreatWarningsAsErrors rebuild (issue #968, task P8-T4)

Timestamp: 2026-10-03T03-29
Command: pwsh -NoProfile -Command '<CMD-REBUILD payload>' with GATEARGS `/p:TreatWarningsAsErrors=true` and TASKID p8-t4; the payload is identical, line for line, to the one transcribed in full in FEATURE/evidence/baseline/msbuild-analyzer-baseline.md except that `/p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` is replaced by `/p:TreatWarningsAsErrors=true` and the log is `coverage\logs\p8-t4.msbuild.log`
Canonical command: msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true (no Nullable override; resolved through vswhere against WORKTREE/TaskMaster.sln, plus /nodeReuse:false and a normal-verbosity file logger under the ignored coverage directory)
EXIT_CODE: 0
Output Summary:
- ITERATION: 1
- WORKTREE-LEAF: agent-a291a7fbabf9d0229
- MSBUILD_EXIT_CODE: 0
- ERRORS: 0
- WARNINGS: 0
- SKIP_CORECOMPILE_LINES: 0
- CSC_OUT_QUICKFILER: 2
- CSC_OUT_QUICKFILER_TEST: 2
- WRITESET_DIAGNOSTIC_LINES: 0
- WRITESET_DIAGNOSTIC_CODES: (empty)
- TEST_DLL_EXISTS: True
- UCS_TEST_DLL_EXISTS: True
