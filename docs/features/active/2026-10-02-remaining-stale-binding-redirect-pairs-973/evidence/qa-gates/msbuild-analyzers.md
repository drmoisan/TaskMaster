# P4-T5 analyzer Rebuild gate (final C# pass, iteration 1)

Timestamp: 2026-10-06T18-25
Command: msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true (resolved through vswhere, plus /nodeReuse:false and the file logger "/flp:LogFile=coverage\logs\p4-t5.msbuild.log;Verbosity=normal"; CMD-REBUILD p4-t5 run from Set-Location -LiteralPath "<execution-worktree-root>")
EXIT_CODE: 0
Output Summary: Rebuild succeeded: MSBUILD_EXIT_CODE 0, ERRORS 0, WARNINGS 0 (not greater than BASELINE-WARNINGS 0, P0-T16), no CoreCompile skipped, CS0121/CS0433/MSB3277 0, USING_DIAG_LINES 0, 36 csc /out lines (at least 18), UtilitiesCS csc echoed 2 times and the Part H file named 2 times in the log. Every pass condition holds; D9 and the Part F repair branch were not taken.

## Printed lines

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
NEWFILE_CSC_LINES: 2

BASELINE-WARNINGS: 0 (P0-T16, evidence/baseline/msbuild-analyzers-baseline.2026-10-03T10-51.md)
WARNINGS-NOT-ABOVE-BASELINE: True
ALIAS-FAILURE: none
USING-FAILURE: none
DIRECTIVE-RESTORED: none
