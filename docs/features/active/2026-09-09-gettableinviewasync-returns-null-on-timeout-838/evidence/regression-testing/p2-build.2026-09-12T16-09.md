# P2-T4 — Rebuild with the minimal targeted production fix applied

Timestamp: 2026-09-13T02-55

Canonical CLAUDE.md command text: `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`

Command: the same single-segment pwsh wrapper, rebuild target and log-counting shape P0-T17 used, with the file log written to the scratch logs directory as `p2-analyzers.log`. Console rendering of the MSBuild output was suppressed; every gate token is computed from the file log.

EXIT_CODE: 0

MSBUILD_EXIT=0 ERROR_CS_LINES=0 ZERO_ERROR_SUMMARY=1 CORECOMPILE_SKIPPED=0

Output Summary: the rebuild succeeded with the new failure-construction partial compiled into the UtilitiesCS assembly and the fix applied to the method under fix. All four acceptance clauses hold. Two properties of the fix are established by this clean build rather than asserted. First, removing the null-forgiving suppression from the return did not produce a nullable-flow error, because the guard immediately above it narrows the local to non-null on every path that reaches the return. Second, having the helper return the exception rather than throw it keeps definite-assignment analysis correct at all three call sites without a does-not-return attribute, which the net48 base class library does not provide. No project skipped its `CoreCompile` target. Outlook held zero processes before the command ran.
