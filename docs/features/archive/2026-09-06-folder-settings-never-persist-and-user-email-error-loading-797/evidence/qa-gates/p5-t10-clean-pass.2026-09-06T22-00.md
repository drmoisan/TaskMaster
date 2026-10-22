# P5-T10 — Clean-pass Confirmation (Issue #797)

Timestamp: 2026-09-07T10-13

The six commands of the Phase 5 loop, P5-T1 through P5-T6, in order, with the exit code each produced
on the final pass.

1. `dotnet tool run csharpier format .` — EXIT_CODE: 0
2. `dotnet tool run csharpier check .` — EXIT_CODE: 0
3. `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true "/flp:Verbosity=detailed;LogFile=coverage/plan797-final-analyzers.log"` — EXIT_CODE: 0
4. `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true "/flp:Verbosity=detailed;LogFile=coverage/plan797-final-nullable.log"` — EXIT_CODE: 0
5. `pwsh -NoProfile -File coverage/plan797-helpers.ps1 -Mode Test -FilterName All -ResultsDirectory coverage/plan797-trx/p5` — EXIT_CODE: 0
6. `pwsh -NoProfile -File coverage/plan797-helpers.ps1 -Mode Coverage -FilterName All -OutputPath coverage/plan797-final/coverage.cobertura.xml -ResultsDirectory coverage/plan797-trx/p5-coverage` — EXIT_CODE: 0

Command 5 is the plan's `vstest.console.exe <test-assembly-paths> /EnableCodeCoverage` step in the
form rule R6 requires: explicit assembly paths rather than directory discovery, the `/InIsolation`
switch CI uses, and the shell-icon exclusions. Command 6 supplies the coverage collection.

## Loop restarts

Number of loop restarts performed: 1.

Reason for the restart: the first execution of P5-T1 rewrote files. The formatter is a write-mode
command, and under the Phase 5 loop rule a step that changes files restarts the loop at P5-T1. That
first run reformatted source that this change had edited but not yet formatted.

The restarted pass is the one recorded above. Its formatter run rewrote zero Write Set files, verified
by hashing all thirteen Write Set C# files immediately before and immediately after the run with
SHA-256 and comparing, which reported `FORMAT-REWRITTEN-COUNT=0`. The read-only check subcommand then
exited 0 over 1605 files with no unformatted file reported.

The final pass required no restart: no step failed against its declared expectation and no step
changed files. Every one of the six steps declared an expected exit code of 0 and produced 0.

Output Summary: P5-T1 through P5-T6 completed in a single uninterrupted pass with all six exit codes
at 0, after exactly one restart caused by the first formatter run rewriting files.
