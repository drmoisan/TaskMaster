# C# Coverage Artifact — Issue #169 Remediation (R2)

- **Timestamp (UTC):** 2026-06-01T17-35-23Z
- **Canonical artifact:** `artifacts/csharp/coverage.xml` (Cobertura)
- **Validity:** Parsed as XML successfully; `coverage` root present with `line-rate`,
  `packages/package`, `classes/class`, `methods/method` structure. Size ~30.6 MB.

## Commands run (exact)

Instrumented test run that produced the `.coverage`:

```
vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll QuickFiler.Test\bin\Debug\QuickFiler.Test.dll TaskMaster.Test\bin\Debug\TaskMaster.Test.dll /EnableCodeCoverage
```

vstest path: `C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe`

Source `.coverage` file:
`TestResults/850a8907-3a68-4ad5-a717-55c98775a764/DanMoisan_MEGALODON4_2026-06-01.13_43_34.coverage`

Conversion to canonical Cobertura XML:

```
dotnet-coverage merge TestResults\850a8907-3a68-4ad5-a717-55c98775a764\DanMoisan_MEGALODON4_2026-06-01.13_43_34.coverage -f cobertura -o artifacts\csharp\coverage.xml
```

dotnet-coverage version: 18.5.2 (v18.5.2.0 [win-x64 - .NET 10.0.8]). Cobertura format used
(not the `-f xml` fallback).

This artifact corresponds to the POST-REMEDIATION instrumented run (test count 3991,
including the two new R1 regression tests in `RibbonControllerTests`). The same `.coverage`
file backs the final-pass artifact (see P5-T2).

## P3-T2 — R1 decision-logic coverage (entry-point decision no longer at 0%)

From `artifacts/csharp/coverage.xml`, class `TaskMaster.RibbonController`:

| Member | Signature | line-rate | branch-rate | Lines |
|--------|-----------|-----------|-------------|-------|
| `SetHighConfidenceModeForLaunch` | `(bool)` | 1.0 (100%) | 1.0 | line 269, hits=1 |
| `IsHighConfidenceModeActive` | `()` | 1.0 (100%) | — | covered |
| `ToggleHighConfidenceMode` | `()` | 1.0 (100%) | — | covered |

`SetHighConfidenceModeForLaunch(bool)` — the new R1 decision method — is covered at
100% line-rate (>= 90% new-member target SATISFIED), exercised by the P1-T5 tests
`SetHighConfidenceModeForLaunch_True_EnablesMode` and
`StandardLaunchAfterHighConfidenceLaunch_DoesNotEnableMode`. The behaviorally-distinct
entry-point mode decision (`SetHighConfidenceModeForLaunch` + `IsHighConfidenceModeActive`)
is therefore no longer at 0%, closing the prior coverage gap that drove AC1 PARTIAL and
the policy C# coverage FAIL.

Note: the COM/VSTO launch-lifecycle methods (`LoadQuickFilerAsync`,
`LoadQuickFilerHighConfidenceAsync`, `ReleaseQuickFiler`) require Outlook COM and the live
`QfcHomeController` and remain outside unit-test scope (pre-existing VSTO shell condition);
`ReleaseQuickFiler` shows line-rate 0 for that reason. The unit-testable mode decision they
delegate to (`SetHighConfidenceModeForLaunch`) is fully covered.
