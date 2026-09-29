# Scoped Format After Fix (P3-T1)

Timestamp: 2026-09-29T09-08
Command: pwsh -NoProfile -Command 'Set-Location "<repo-root>"; "BEFORE:"; Get-FileHash -Algorithm SHA256 -LiteralPath QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixture.cs, QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixtureTests.cs | ForEach-Object { $_.Hash }; dotnet tool run csharpier format QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixture.cs QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixtureTests.cs; "FORMAT-EXIT=$LASTEXITCODE"; "AFTER:"; Get-FileHash -Algorithm SHA256 -LiteralPath QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixture.cs, QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixtureTests.cs | ForEach-Object { $_.Hash }; dotnet tool run csharpier check QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixture.cs QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixtureTests.cs; "CHECK-EXIT=$LASTEXITCODE"'
EXIT_CODE: 0
Output Summary:
- BEFORE-HASH-FX=CB2E95C533F349CCC4792927B69558D238264CB6280FBF742842714000C4BB08
- BEFORE-HASH-FT=70F5A1B0A27CAB73CBAA42375D5A49DCDB64FE7BC2B6512BDA2177D7BA44D9E7
- FORMAT-EXIT=0 (console line `Formatted 2 files in 2283ms.` is a processed-file count, not the rewritten count)
- AFTER-HASH-FX=1BB53DBE378F6E6B69C42D9234061465CAD9CE79002E2AE9CF8004731449EFA6
- AFTER-HASH-FT=40EE455C7D2ACF6ADA5D01B8880B37A7BCBAF320CEC9F83A8B497015EAE56F52
- REWRITTEN-COUNT: 2
- Rewrites observed (git diff against HEAD): fixture file, the delegating return statement and the bounded signature were wrapped so that `TimeSpan bound` sits on its own line (the two rewraps D3 and the Delivered source anticipate); test file, the `probe` lambda body was moved onto its own line.
- CHECK-EXIT=0 with `Checked 2 files in 790ms.`
