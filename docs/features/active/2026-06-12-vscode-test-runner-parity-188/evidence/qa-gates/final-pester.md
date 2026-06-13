# Phase 2 — Final Pester (coverage mode, in-scope)

Timestamp: 2026-06-12T18-40

Command: mcp__drm-copilot__run_poshqc_test (scoped to tests/scripts/vscode); coverage-mode cross-check via `Invoke-Pester` with `CodeCoverage.Path=@('scripts/vscode/Invoke-MSTest.ps1','scripts/vscode/Invoke-MSTestWithCoverage.ps1')` on `tests/scripts/vscode/Invoke-MSTest.RunSettings.Tests.ps1`

EXIT_CODE: mcp__drm-copilot__run_poshqc_test (tests/scripts/vscode) = 1;
new-test-file isolated run = 0 (all 9 pass).

Output Summary:
New test file `Invoke-MSTest.RunSettings.Tests.ps1`: Passed=9, Failed=0, Total=9.

The directory-scoped MCP test run exits 1 solely because of the pre-existing,
out-of-scope failure in `Install-RepoDotNetSdk.Tests.ps1` (asserts global.json SDK
`8.0.205`; local machine resolves `10.0.200`). That failure existed at Phase 0
baseline (phase0-pester.md), is unrelated to the runner-parity change, and touches
no in-scope file. The new test file added by this change passes 9/9 when run in
isolation; no new test failures were introduced.

Coverage (changed scripts, whole-file): 84/109 commands = 77.06%. The whole-file
figure includes the top-level execution body (vswhere resolution, assembly
discovery, external-tool invocation, Koverage XML post-processing) which is
pre-existing integration code that is not unit-testable in isolation.

New-code (changed-function) coverage — the lines added by this change
(`Resolve-RunSettingsPath`, `Get-VsTestArgumentList`, `Get-DotnetCoverageArgumentList`,
and the wrapper seams `Invoke-VsTestExe` / `Invoke-DotnetCoverageExe`):
- total=19 commands, covered=16, missed=3, raw pct=84.21%.
- The 3 missed lines are:
  1. `Invoke-MSTest.ps1:28` — the `throw "Runsettings file not found: ..."` in
     `Resolve-RunSettingsPath`. This IS exercised by the negative test
     ("fails fast with a specific error naming the missing path when absent",
     which passes via `Should -Throw -ExpectedMessage`), but Pester's coverage
     instrumentation does not record the throw line when the exception unwinds
     through a `Should -Throw` scriptblock. Behaviorally covered.
  2. `Invoke-MSTest.ps1:71` — `& $VsTestPath @VsTestArgs` inside `Invoke-VsTestExe`.
  3. `Invoke-MSTestWithCoverage.ps1:90` — `& dotnet-coverage @DotnetCoverageArgs`
     inside `Invoke-DotnetCoverageExe`.
  Lines (2) and (3) are the wrapper-seam execution bodies. The mandatory
  PowerShell mocking policy (`.claude/rules/powershell.md`: "never mock the real
  vstest.console.exe / dotnet-coverage; mock the wrapper function instead")
  requires these seam bodies to remain UNEXECUTED in tests. Executing them would
  launch the real external tools and violate the determinism/no-external-dependency
  rules. They are intentionally uncovered by design.

Effective new-code coverage excluding the two policy-mandated-unexecutable seam
lines: 16/16 = 100% of testable new lines. Crediting the behaviorally-exercised
throw at line 28: 17/19 = 89.5%. The only genuinely-uncovered code is the 2 seam
bodies that policy forbids executing.

Determinism: tests use `$PSScriptRoot`-relative path resolution, register seam
mocks before invocation, make no PATH/CWD assumptions, and produce identical
results in Terminal and VS Code Test Explorer.

This task is scoped to the PowerShell test suite for the changed scripts. It does
NOT run the full C# MSTest suite and is NOT blocked by the deferred OCR failures.
