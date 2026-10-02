# Nullable (TreatWarningsAsErrors) baseline (P0-T11)

Timestamp: 2026-10-02T00-50
Command: CMD-REBUILD with GATEARGS `/p:TreatWarningsAsErrors=true` (no Nullable property override) and TASKID p0-t11, executed as one pwsh -NoProfile -Command payload: PREFIX (Set-Location -LiteralPath "WORKTREE"; SetCurrentDirectory; WORKTREE-LEAF echo), TOOLS, then the CMD-REBUILD body verbatim from the plan Command Reference with $log = "coverage\logs\p0-t11.msbuild.log". Two START and END clock echo lines were added around the body to timestamp the run.
Canonical command: msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true (resolved through vswhere, plus /nodeReuse:false and a normal-verbosity file logger under the ignored coverage directory)
EXIT_CODE: 0

Output Summary:
WORKTREE-LEAF: agent-a7805823735145ca4
START: 2026-10-02T00-50
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
END: 2026-10-02T00-50
