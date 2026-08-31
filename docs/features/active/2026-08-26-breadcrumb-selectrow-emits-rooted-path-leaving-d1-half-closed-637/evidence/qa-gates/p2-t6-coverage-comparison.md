# P2-T6 coverage comparison

Timestamp: 2026-08-31T17-16

Command: `Parse coverage/remediation-baseline.cobertura.xml and coverage/p7-t5-remediation.cobertura.xml line-rate values; inspect remediation diff paths`

EXIT_CODE: 0

Output Summary: Post-split repository line coverage is 85.3389%, which exceeds the 80% floor and is 0.0031 percentage points above the 85.3358% baseline. The remediation changes only test and project surfaces, so no changed production-code sequence points exist to calculate.

- Baseline repository line coverage: 85.3358%
- Post-change repository line coverage: 85.3389%
- Coverage delta: +0.0031 percentage points
- Repository coverage floor: 80.0000% — passed
- No-regression check: passed

Changed implementation paths:

- `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterIssue439Tests.cs`
- `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterIssue439Tests.Activation.cs`
- `QuickFiler.Test/QuickFiler.Test.csproj`

No production-code path is changed; therefore changed production-code sequence points are not applicable.
