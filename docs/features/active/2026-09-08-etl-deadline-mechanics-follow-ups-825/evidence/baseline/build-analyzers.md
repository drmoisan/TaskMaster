# Baseline — MSBuild Analyzer Gate

Timestamp: 2026-09-09T16-36

Command: & $msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true /flp:LogFile=docs/features/active/2026-09-08-etl-deadline-mechanics-follow-ups-825/evidence/baseline/build-analyzers.txt;Verbosity=detailed

EXIT_CODE: 0

WarningCount: 0
ErrorCount: 0

Output Summary: MSBuild was resolved through vswhere per D1 to
C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe. The solution
rebuild reported "Build succeeded", 0 Warning(s) and 0 Error(s) in 00:00:16.99. The sibling detailed
file log build-analyzers.txt exists and carries 59393 lines. /t:Rebuild is used rather than /t:Build
because MSBuild's up-to-date check does not invalidate on a command-line property change, so a warm
/t:Build would return exit 0 with CoreCompile skipped and would run no analyzers. Non-vacuity is
corroborated in this log by two lines carrying the token `/out:obj\Debug\UtilitiesCS.dll` and two
carrying `/out:obj\Debug\UtilitiesCS.Test.dll`, which are the csc.exe command lines MSBuild echoes
under each project's CoreCompile heading.

## D7 sanitisation

The sibling build-analyzers.txt was sanitised as the last action of this task, after every count
above was taken. The counted tokens carry no absolute path and are unaffected by the rewrite.

Deviation recorded. D7 names two rewrites: the worktree root to the literal `<repo-root>` and the
main checkout root to the literal `<main-checkout-root>`. Both were applied, in that order, because
the main checkout root C:\Users\DanMoisan\repos\TaskMaster is a proper prefix of the worktree root
C:\Users\DanMoisan\repos\TaskMaster-wt\rr0908-825 and rewriting the shorter string first would
corrupt the longer one. After those two rewrites, 21 lines still contained the token `C:\Users\`,
in two further leak classes D7 does not name: the MSBuildUserExtensionsPath property expanded from
the environment, and a _DeploymentUrl property reassignment naming a OneDrive folder. Both carry
the host account name. P9-T4's confirming check requires a zero count of `C:\Users\` in this log, so
a third rewrite was applied, mapping the user profile root C:\Users\DanMoisan to the literal
`<user-profile-root>`. It ran after the two D7 rewrites, so it touches only the residual
occurrences. The count of lines containing `C:\Users\` in this log is now 0.
