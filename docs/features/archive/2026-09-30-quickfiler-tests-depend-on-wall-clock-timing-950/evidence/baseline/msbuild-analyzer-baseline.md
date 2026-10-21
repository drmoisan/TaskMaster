# Analyzer baseline (P0-T10)

Timestamp: 2026-10-02T00-49
Command: CMD-REBUILD with GATEARGS `/p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` and TASKID p0-t10, executed as one pwsh -NoProfile -Command payload: PREFIX (Set-Location -LiteralPath "WORKTREE"; SetCurrentDirectory; WORKTREE-LEAF echo), TOOLS (vswhere resolution of MSBuild.exe and vstest.console.exe; coverage\logs created), then the CMD-REBUILD body verbatim from the plan Command Reference with $log = "coverage\logs\p0-t10.msbuild.log".
Canonical command: msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true (resolved through vswhere, plus /nodeReuse:false and a normal-verbosity file logger under the ignored coverage directory)
EXIT_CODE: 0

Output Summary:
WORKTREE-LEAF: agent-a7805823735145ca4
MSBUILD_EXIT_CODE: 0
ERRORS: 0
WARNINGS: 0
SKIP_CORECOMPILE_LINES: 0
CSC_OUT_QUICKFILER: 2
CSC_OUT_QUICKFILER_TEST: 2
WRITESET_DIAGNOSTIC_LINES: 0
WRITESET_DIAGNOSTIC_CODES: (empty)
TEST_DLL_EXISTS: True
UCS_TEST_DLL_EXISTS: True

ANALYZER-BASELINE-WARNINGS: 0
ANALYZER-BASELINE-WRITESET-LINES: 0
ANALYZER-BASELINE-WRITESET-CODES: (empty)
