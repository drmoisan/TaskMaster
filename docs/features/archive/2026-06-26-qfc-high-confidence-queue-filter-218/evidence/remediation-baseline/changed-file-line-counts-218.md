Timestamp: 2026-06-26T21-09
Command: $files = @('QuickFiler/Controllers/QfcDatamodel.cs','QuickFiler/Controllers/QfcHomeController.cs','QuickFiler.Test/Controllers/QfcDatamodelTests.cs','QuickFiler.Test/Controllers/QfcHomeControllerTests.cs','QuickFiler.Test/QuickFiler.Test.csproj'); $result = foreach ($file in $files) { $count = (Get-Content -LiteralPath $file).Count; [pscustomobject]@{ File = $file; Lines = $count; Result = if ($count -le 500) { 'PASS' } else { 'FAIL' } } }; $result | Format-Table -AutoSize; if ($result.Result -contains 'FAIL') { exit 1 }
EXIT_CODE: 1
Output Summary:
- PASS/FAIL against 500-line limit: FAIL.
- QuickFiler/Controllers/QfcDatamodel.cs: 843 lines, FAIL.
- QuickFiler/Controllers/QfcHomeController.cs: 739 lines, FAIL.
- QuickFiler.Test/Controllers/QfcDatamodelTests.cs: 168 lines, PASS.
- QuickFiler.Test/Controllers/QfcHomeControllerTests.cs: 1475 lines, FAIL.
- QuickFiler.Test/QuickFiler.Test.csproj: 354 lines, PASS.
