# P0-T17 nullable Rebuild baseline (issue #973)

Timestamp: 2026-10-03T10-52
Command: msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true (resolved through vswhere, plus /nodeReuse:false and the file logger "/flp:LogFile=coverage\logs\p0-t17.msbuild.log;Verbosity=normal"; CMD-REBUILD p0-t17 payload run from <execution-worktree-root>)
EXIT_CODE: 0
Output Summary: TreatWarningsAsErrors Rebuild of the unchanged tree passed the CMD-REBUILD pass set: exit 0, 0 errors, 0 warnings, CoreCompile never skipped, no CS0121/CS0433/MSB3277, no using diagnostics, UtilitiesCS CoreCompile executed; NEWFILE_CSC_LINES 0 (negative control).

MSBUILD_EXIT_CODE: 0
SKIP_CORECOMPILE_LINES: 0
ZERO_ERRORS_LINES: 1
WARNINGS: 0
ERRORS: 0
CS0121_LINES: 0
CS0433_LINES: 0
MSB3277_LINES: 0
CSC_OUT_LINES: 36
USING_DIAG_LINES: 0
UTILITIESCS_CSC_LINES: 2
NEWFILE_CSC_LINES: 0
