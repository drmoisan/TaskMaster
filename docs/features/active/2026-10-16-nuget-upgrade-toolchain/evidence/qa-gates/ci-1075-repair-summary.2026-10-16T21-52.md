Timestamp: 2026-10-16T21-52
Command: CI 1075 repair validation in ../TaskMaster-wt/2026-10-16T16-48-toolchain
EXIT_CODE: 0

# CI 1075 Repair QA Summary

The tracked `TaskMaster/TaskMaster.csproj` `PublishUrl` is `publish\\`. The ignored `TaskMaster/TaskMaster.csproj.user` retains the personal publish override. The corrected project was copied to `../TaskMaster-wt/2026-10-16T16-48-toolchain/TaskMaster/TaskMaster.csproj`; both files were byte-identical during validation.

Project SHA256 during local toolchain validation:

`CD27ECE7FDF6967DD25954B67C19FD99444F9FAC097BDF3AB785D6DEEDCA0CB2`

Validation commands and observed exit codes:

| Command | EXIT_CODE | Result |
| --- | ---: | --- |
| `dotnet tool run csharpier format .` | 0 | Formatted 1,650 files |
| `dotnet tool run csharpier check .` | 0 | Checked 1,650 files |
| `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true /nologo /verbosity:minimal` | 0 | All solution projects rebuilt; no diagnostics |
| `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true /nologo /verbosity:minimal` | 0 | All solution projects rebuilt; no diagnostics |
| `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug` | 0 | 7,450 passed, 0 failed |

Coverage output: `../TaskMaster-wt/2026-10-16T16-48-toolchain/coverage/coverage.cobertura.xml`.

Coverage result: 56,890/66,143 lines (86.01%) and 13,768/17,173 branches (80.17%).

Input checks passed: `TaskMaster/TaskMaster.csproj` parsed as XML; `evidence/qa-gates/validation-inputs.2026-10-16T20-53.json` and `evidence/qa-gates/final-coverage-summary.2026-10-16T20-55.json` parsed as JSON; all recorded validation paths are relative.

The original hygiene failure is recorded in `evidence/baseline/ci-1075-hygiene-failure.2026-10-16T21-46.md`. Final hygiene rerun is owned by the root session after staging.

Final project SHA256 after restoring original line endings: `1253062BE8D7049F0BB88A259F0F1756A2D6399854B1AC1B07D2FC329751AA65`.

The final review restored the committed project's original line-ending bytes while preserving the identical XML and relative PublishUrl. The isolated validation copy matches the final file byte-for-byte. Remote CI will verify this final commit.

Final staged hygiene: `pwsh -NoProfile -File scripts/hygiene/Test-RepositoryHygiene.ps1` returned `HYGIENE Findings=0` and EXIT_CODE 0. Final staged diff whitespace check passed. The corrective project diff contains one property change.
