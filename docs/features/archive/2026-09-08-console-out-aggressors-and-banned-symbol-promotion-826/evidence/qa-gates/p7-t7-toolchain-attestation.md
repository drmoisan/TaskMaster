# Final QA loop closure attestation (issue #826, [P7-T7])

Timestamp: 2026-09-09T19-44

Command: this task performs no new measurement; it attests over the artifacts [P7-T1] through [P7-T5]
produced.

EXIT_CODE: 0

## The four steps, in order, with exit codes and artifacts

| # | Step | Command | Artifact | EXIT_CODE |
|---|---|---|---|---|
| 1 | Formatting | `dotnet tool run csharpier format .`, verified with `dotnet tool run csharpier check .` | `evidence/qa-gates/p7-t1-format.md` | 0 (format) and 0 (check) |
| 2 | Analyzer build | `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` | `evidence/qa-gates/p7-t2-analyzers.md` | 0 |
| 3 | Nullable build | `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true` | `evidence/qa-gates/p7-t3-nullable.md` | 0 |
| 4 | Testing, measured form | `dotnet-coverage collect --settings coverage.config ... -- vstest.console.exe <nine assemblies> ...` | `evidence/qa-gates/p7-t4-tests-coverage.md` | 0 |
| 4 | Testing, confirming CI-verbatim form | `vstest.console.exe <nine assemblies> /EnableCodeCoverage /InIsolation ...` | `evidence/qa-gates/p7-t5-tests-ci-verbatim.md` | 0 |

Every step cites an artifact from [P7-T1] through [P7-T5] and every one records exit code 0.

## Restart count

**0.** The phase was not restarted. No step failed and no step rewrote a file.

The zero-restart outcome is the expected one rather than a surprise: convention C4 requires every task
that creates or edits a `.cs` file to run `csharpier format` over exactly the paths it touched, before
that task's own acceptance gates. Every Phase 2 and Phase 5 task did so, so the Phase 7 step-1 pass found
the tree already clean.

## The final pass completed without any step failing or rewriting a file

The step-1 rewrite claim is not inferred from the exit code, which is 0 whether or not the formatter
changed anything. It rests on the SHA-256 comparison recorded in `evidence/qa-gates/p7-t1-format.md`:
1658 files were hashed before and after `csharpier format .`, the two arrays were index-aligned, and the
number of hash differences (`$rewritten`) was **0**. The paired read-only `csharpier check .` exited 0 and
printed `Checked 1624 files in 4766ms.`

## Non-vacuity of the two msbuild gates

Both used `/t:Rebuild`, never `/t:Build`, and both logs were read for the pair that makes the zero counts
meaningful:

| Log | `Skipping target "CoreCompile"` | `Task "Csc"` | ` 0 Error(s)` |
|---|---|---|---|
| `coverage/826-raw/p7-t2-analyzers.log` | 0 | 18 | 1 |
| `coverage/826-raw/p7-t3-nullable.log` | 0 | 18 | 1 |

A `Task "Csc"` count of 18, one per project in the solution, is what makes a zero
`Skipping target "CoreCompile"` count evidence of compilation rather than evidence of an empty log. Had a
warm `/t:Build` been used, MSBuild's up-to-date check would have returned exit 0 with `CoreCompile`
skipped on every project and neither gate could have failed.

## Test-count and coverage headline from the final pass

7192 of 7192 tests passed with 0 failed in both step-4 forms, and the two runs agree on every counter.
Root line coverage is 86.1329 percent, above the `>= 85%` floor.

Output Summary: all four toolchain steps ran in the order format, analyzer build, nullable build, test,
each exited 0, each is backed by a named artifact, the phase was restarted 0 times, and the final pass
completed without any step failing or rewriting a file. AC14 is satisfied.
