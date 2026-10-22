# QA Post-Format Size and Identity Audit (P4-T7)

Timestamp: 2026-09-29T09-17
Command: pwsh -NoProfile -Command 'Set-Location "<repo-root>"; $fx = "QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixture.cs"; $ft = "QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixtureTests.cs"; Get-FileHash -Algorithm SHA256 -LiteralPath $fx, $ft | ForEach-Object { $_.Hash }; "LINES-FX=" + @(Get-Content -LiteralPath $fx).Count; "LINES-FT=" + @(Get-Content -LiteralPath $ft).Count; foreach ($lit in @("Interlocked.Increment(ref _contendedAcquisitions);", "TransactionGate.WaitAsync(bound)", "throw new TimeoutException(", "Interlocked.Increment(ref _transactionAcquisitions);", "return new UiThreadDispatcherTransaction();")) { "LINE " + $lit + " = " + (Select-String -LiteralPath $fx -SimpleMatch -Pattern $lit | Select-Object -First 1).LineNumber }'
EXIT_CODE: 0
Output Summary:
- HASH-FX=1BB53DBE378F6E6B69C42D9234061465CAD9CE79002E2AE9CF8004731449EFA6 (equals the P4-T1 AFTER-HASH-FX)
- HASH-FT=40EE455C7D2ACF6ADA5D01B8880B37A7BCBAF320CEC9F83A8B497015EAE56F52 (equals the P4-T1 AFTER-HASH-FT)
- No edit to either C# file happened after the final format.
- LINES-FX: 342 (greater than 304, at most 500)
- LINES-FT: 458 (greater than 396, at most 500)
- LINE Interlocked.Increment(ref _contendedAcquisitions); = 175
- LINE TransactionGate.WaitAsync(bound) = 178
- LINE throw new TimeoutException( = 181
- LINE Interlocked.Increment(ref _transactionAcquisitions); = 188
- LINE return new UiThreadDispatcherTransaction(); = 189
- The five line numbers remain strictly increasing (175 < 178 < 181 < 188 < 189) and equal the P3-T4 values.
