# PowerShell Coverage Regeneration — Canonical Machine-Verifiable Artifact (Issue #283, R3)

Timestamp: 2026-07-08T18-52
Command: `Invoke-Pester -Configuration <cfg>` (Pester 5.6.1; `New-PesterConfiguration` with `CodeCoverage.Enabled=$true`, `CodeCoverage.Path=@('scripts/vscode/Invoke-MSTest.ps1','scripts/vscode/Invoke-MSTestWithCoverage.ps1')`, `CodeCoverage.OutputFormat='JaCoCo'`, `CodeCoverage.OutputPath='artifacts/pester/powershell-coverage.xml'`, `Run.Path='tests/scripts/vscode/Invoke-MSTest.RunSettings.Tests.ps1'`)
EXIT_CODE: 0

Output Summary:
- Pester version: 5.6.1.
- Tests: TOTAL 11, PASSED 11, FAILED 0, SKIPPED 0.
- Code coverage of the two in-scope QC scripts: 77.06% (CommandsAnalyzed 109, CommandsExecuted 84) — matches the P0-T9 / remediation baseline (77.06%, 109/84) exactly. No regression.
- Canonical machine-readable JaCoCo XML written to `artifacts/pester/powershell-coverage.xml` (permitted non-evidence coverage-output path; `artifacts/coverage/` is forbidden, `artifacts/pester/` is not).
- Missed (uncovered) commands reported by Pester are the host-bound top-level script-body lines (documented in `powershell-coverage-exemption.md`):
  - `Invoke-MSTest.ps1`: lines 31, 74 (`Invoke-VsTestExe` body `& $VsTestPath @VsTestArgs`), 92, 99, 104, 116, 128, 129, 130.
  - `Invoke-MSTestWithCoverage.ps1`: lines 94 (`Invoke-DotnetCoverageExe` body `& dotnet-coverage @DotnetCoverageArgs`), 114, 121, 126, 130, 142, 148, 171, 172, 173, 181-186 (Cobertura post-processing block).
  - These are fail-fast throw guards, the executable-invocation wrapper bodies, `Get-ChildItem`-based test-assembly discovery, and the Cobertura post-processing block — real external-executable / filesystem-discovery lines that cannot be unit-tested deterministically per the no-external-dependency rule. The pure logic (arg builders, runsettings resolver, wrapper seams) is fully covered.
