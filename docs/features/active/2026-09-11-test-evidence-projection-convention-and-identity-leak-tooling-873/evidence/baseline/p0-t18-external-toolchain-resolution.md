# P0-T18 — External Toolchain Resolution

Timestamp: 2026-09-13T05-07
Task: [P0-T18]

Command: pwsh -NoProfile -Command 'Set-Location -LiteralPath <worktree>; $vswherePath = Join-Path ${env:ProgramFiles(x86)} "Microsoft Visual Studio\Installer\vswhere.exe"; Test-Path $vswherePath; & $vswherePath -latest -products * -find "Common7\IDE\Extensions\TestPlatform\vstest.console.exe" | Select-Object -First 1; Get-Command "dotnet-coverage" -ErrorAction SilentlyContinue'
EXIT_CODE: 0

Each of the three tools was resolved by the same mechanism the coverage entry point itself uses, read
from `scripts/vscode/Invoke-MSTestWithCoverage.ps1`:

- line 279 composes the vswhere path by joining the thirty-two-bit Program Files environment value
  with the Visual Studio Installer subdirectory and the executable name, and line 280 guards it with a
  path-existence test that throws at line 281 when absent;
- lines 284 through 287 invoke that vswhere with the argument array `-latest -products * -find
  Common7\IDE\Extensions\TestPlatform\vstest.console.exe` and take the first returned result, throwing
  at line 289 when empty;
- line 292 resolves the command named `dotnet-coverage` through `Get-Command` with a silent error
  action, throwing at line 293 when it does not resolve.

All three guards precede any work this delivery adds, and no other task in this plan verifies any of
them.

## The three labelled results

VSWHERE_RESOLVED: True vswhere.exe
VSTEST_CONSOLE_RESOLVED: True vstest.console.exe
DOTNET_COVERAGE_RESOLVED: True dotnet-coverage.exe

No full path is recorded. The resolved paths are absolute host paths, and this delivery's own rule text
prohibits carrying one into an artifact, so only the leaf name of each resolved file is recorded.

## Output Summary

All three recorded values are `True` and each line carries a leaf name, so this task does not halt. The
two Phase 6 end-to-end runs are known to be runnable in this worktree with respect to the three
external tools the coverage entry point guards on.

Recorded separately so the Phase 6 reader is not misled: tool resolution is not a statement about the
solution's build state. The P0-T8 and P0-T9 baselines are red for a pre-existing analyzer HintPath
skew, and the coverage entry point requires built test assemblies, so Phase 6 additionally depends on a
compilable solution, which this worktree does not currently have. That dependency is recorded here as
an observation and is outside this task's acceptance.

EXIT_CODE: 0
