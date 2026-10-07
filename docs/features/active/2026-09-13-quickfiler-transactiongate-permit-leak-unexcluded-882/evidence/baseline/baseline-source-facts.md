# Baseline Source Facts (P0-T15)

Timestamp: 2026-09-29T09-05
Command: pwsh -NoProfile -Command 'Set-Location "<repo-root>"; $fx = "QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixture.cs"; $ft = "QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixtureTests.cs"; $h = @(Get-FileHash -Algorithm SHA256 -LiteralPath $fx, $ft | ForEach-Object { $_.Hash }); "HASH-FX=" + $h[0]; "HASH-FT=" + $h[1]; "LINES-FX=" + @(Get-Content -LiteralPath $fx).Count; "LINES-FT=" + @(Get-Content -LiteralPath $ft).Count; foreach ($l in @("TransactionGate.WaitAsync()", "TimeoutException", "120000", "using System.Globalization;", "if (!acquired)", "throw new TimeoutException(", "BeginTransactionAsync(TimeSpan bound)", "TRANSACTIONGATE_ACQUIRE_TIMEOUT", "Interlocked.Increment(ref _transactionAcquisitions);", "Interlocked.Increment(ref _contendedAcquisitions);")) { "FX-COUNT " + $l + " = " + @(Select-String -LiteralPath $fx -SimpleMatch -CaseSensitive -Pattern $l).Count }; foreach ($l in @("TimeSpan.Zero", "ThrowAsync<TimeoutException>", "TRANSACTIONGATE_ACQUIRE_TIMEOUT", "NotThrow<SemaphoreFullException>", "[TestMethod]", "DoNotParallelize")) { "FT-COUNT " + $l + " = " + @(Select-String -LiteralPath $ft -SimpleMatch -CaseSensitive -Pattern $l).Count }; "TOKEN-UNDER-QUICKFILER-TEST=" + @(Get-ChildItem -Recurse -File -Path QuickFiler.Test -Include *.cs | Select-String -SimpleMatch -Pattern "TRANSACTIONGATE_ACQUIRE_TIMEOUT").Count'
EXIT_CODE: 0
Output Summary:
- HASH-FX=FB1661247941A0CECF3A76AF996EEC6BCFB4B7114C6462E132AFF76F22E39097
- HASH-FT=EB9436CC8918A6983D44214430468BD19751AF4B74A961B37E537F6F46B65C4C
- LINES-FX=304
- LINES-FT=396
- FX-COUNT TransactionGate.WaitAsync() = 1
- FX-COUNT TimeoutException = 0
- FX-COUNT 120000 = 0
- FX-COUNT using System.Globalization; = 0
- FX-COUNT if (!acquired) = 0
- FX-COUNT throw new TimeoutException( = 0
- FX-COUNT BeginTransactionAsync(TimeSpan bound) = 0
- FX-COUNT TRANSACTIONGATE_ACQUIRE_TIMEOUT = 0
- FX-COUNT Interlocked.Increment(ref _transactionAcquisitions); = 1
- FX-COUNT Interlocked.Increment(ref _contendedAcquisitions); = 1
- FT-COUNT TimeSpan.Zero = 0
- FT-COUNT ThrowAsync<TimeoutException> = 0
- FT-COUNT TRANSACTIONGATE_ACQUIRE_TIMEOUT = 0
- FT-COUNT NotThrow<SemaphoreFullException> = 0
- FT-COUNT [TestMethod] = 7
- FT-COUNT DoNotParallelize = 0
- TOKEN-UNDER-QUICKFILER-TEST=0
- Verdict: every value equals the plan's re-derived expectation (LINES 304/396; fixture counts 1,0,0,0,0,0,0,0,1,1; test-file counts 0,0,0,0,7,0; token count 0). BASELINE SOURCE FACTS DIFFER did not fire.
