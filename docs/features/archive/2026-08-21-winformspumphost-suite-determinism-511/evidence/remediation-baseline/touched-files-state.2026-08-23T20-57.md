# Remediation Baseline — Touched-Files State

Timestamp: 2026-08-23T18-59

Command:
```powershell
$files = @(
  "QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part2.cs",
  "QuickFiler.Test/Controllers/QfcItemController.ViewerSetupTests.cs",
  "QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part3.cs")
foreach ($f in $files) {
  $lc = (Get-Content -LiteralPath $f).Count
  $hc = (Select-String -LiteralPath $f -SimpleMatch "viewer.Handle").Count
  Write-Output ("{0} lines={1} viewerHandle={2}" -f $f, $lc, $hc)
}
```

EXIT_CODE: 0

Output Summary:

| File | `Get-Content` line count | Expected | `viewer.Handle` matches |
| --- | --- | --- | --- |
| `QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part2.cs` | 416 | 416 | 1 |
| `QuickFiler.Test/Controllers/QfcItemController.ViewerSetupTests.cs` | 470 | 470 | 1 |
| `QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part3.cs` | 398 | 398 | 0 (not asserted; this file carries no defensive read) |

All five asserted counts match the plan's expected values exactly. No drift is recorded.

Raw command output:

```
QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part2.cs lines=416 viewerHandle=1
QuickFiler.Test/Controllers/QfcItemController.ViewerSetupTests.cs lines=470 viewerHandle=1
QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part3.cs lines=398 viewerHandle=0
```
