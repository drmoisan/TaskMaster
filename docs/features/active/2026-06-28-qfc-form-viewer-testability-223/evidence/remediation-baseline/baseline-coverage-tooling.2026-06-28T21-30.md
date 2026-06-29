# Baseline — Coverage Tooling Availability and Prior-Cycle Numeric Headlines (Remediation Cycle 1, Issue #223)

Timestamp: 2026-06-28T21-46
Command: Get-Command dotnet-coverage; vswhere.exe -latest -find Common7\IDE\Extensions\TestPlatform\vstest.console.exe
EXIT_CODE: 0

## Tool availability
- dotnet-coverage: PRESENT — `C:\Users\DanMoisan\.dotnet\tools\dotnet-coverage.exe`
- vstest.console.exe: PRESENT — `C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe` (resolved via vswhere)

## Prior-cycle numeric coverage headline baseline (carried from cycle 2026-06-28T20-52)
- QfcFormController changed-type (no-regression gate, AC5): post-change 363 / 700 = 51.86% (baseline 301 / 767 = 39.24%; delta +12.62 pp, NO REGRESSION).
- QfcFormKeyHandler new code (>= 90% floor, AC5): 2 / 2 = 100.0% (PASS).
- Disclaimed single-assembly process-wide figure (QuickFiler.Test only, instruments all loaded modules): post-change 12.86% (9800 / 76203). NOT the policy gate.
- Repo-wide first-party testable-denominator figure (>= 80% policy gate, AC5): UNMEASURED at cycle entry. This is the target of this remediation.

Output Summary:
Both coverage tools required by `scripts/vscode/Invoke-MSTestWithCoverage.ps1` (dotnet-coverage and vstest.console.exe via vswhere) are present, so PATH-LOCAL is feasible to attempt. The repo-wide first-party testable-denominator coverage figure is UNMEASURED; producing and measuring it from `artifacts/csharp/coverage.xml` is the objective of Phases 1-2.
