# P1-T3 — Rebuild with the deliberately failing regression test present

Timestamp: 2026-09-13T02-50

Canonical CLAUDE.md command text: `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`

Command: the same single-segment pwsh wrapper, rebuild target and log-counting shape P0-T17 used, with the file log written to the scratch logs directory as `p1-analyzers.log`. Console rendering of the MSBuild output was suppressed; every gate token is computed from the file log.

EXIT_CODE: 0

MSBUILD_EXIT=0 ERROR_CS_LINES=0 ZERO_ERROR_SUMMARY=1 CORECOMPILE_SKIPPED=0

Output Summary: the rebuild succeeded with the new test file compiled into the UtilitiesCS test assembly. All four acceptance clauses hold. This build is what makes the Phase 1 fail-before run meaningful: these projects are not SDK-style, so the Compile item P1-T2 added is what causes the file to be compiled at all, and without it the fail-before run would exercise an assembly that does not contain the test and would report the test as not found rather than as failed. Zero lines carry the case-sensitive literal ` error CS`, so the new test file introduces no compile error and the later `Failed` outcome cannot be a compile failure masquerading as evidence of the defect. No project skipped its `CoreCompile` target. Outlook held zero processes before the command ran.
