# Final analyzer rebuild (issue #968, task P8-T3)

Timestamp: 2026-10-03T03-28
Command: pwsh -NoProfile -Command '<CMD-REBUILD payload>' with the analyzer GATEARGS (`/p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`) and TASKID p8-t3; the payload is identical, line for line, to the one transcribed in full in FEATURE/evidence/baseline/msbuild-analyzer-baseline.md except that the log is `coverage\logs\p8-t3.msbuild.log`
Canonical command: msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true (resolved through vswhere against WORKTREE/TaskMaster.sln, plus /nodeReuse:false and a normal-verbosity file logger under the ignored coverage directory)
EXIT_CODE: 0
Output Summary:
- ITERATION: 1
- WORKTREE-LEAF: agent-a291a7fbabf9d0229
- MSBUILD_EXIT_CODE: 0
- ERRORS: 0
- WARNINGS: 0 (ANALYZER-BASELINE-WARNINGS: 0)
- SKIP_CORECOMPILE_LINES: 0
- CSC_OUT_QUICKFILER: 2
- CSC_OUT_QUICKFILER_TEST: 2
- WRITESET_DIAGNOSTIC_LINES: 0 (not greater than ANALYZER-BASELINE-WRITESET-LINES: 0)
- WRITESET_DIAGNOSTIC_CODES: (empty; ANALYZER-BASELINE-WRITESET-CODES: empty) — no new analyzer diagnostic in a Write Set file
- TEST_DLL_EXISTS: True
- UCS_TEST_DLL_EXISTS: True
- This rebuild is the second compile proof for AC27 (no surviving reference to a removed QfcDatamodel member).
