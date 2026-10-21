# QA CSharpier Format, Scoped (P4-T1)

Timestamp: 2026-09-29T09-13
Command: pwsh -NoProfile -Command 'Set-Location "<repo-root>"; "BEFORE:"; Get-FileHash -Algorithm SHA256 -LiteralPath QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixture.cs, QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixtureTests.cs | ForEach-Object { $_.Hash }; dotnet tool run csharpier format QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixture.cs QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixtureTests.cs; "FORMAT-EXIT=$LASTEXITCODE"; "AFTER:"; Get-FileHash -Algorithm SHA256 -LiteralPath QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixture.cs, QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixtureTests.cs | ForEach-Object { $_.Hash }; dotnet tool run csharpier check QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixture.cs QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixtureTests.cs; "CHECK-EXIT=$LASTEXITCODE"'
EXIT_CODE: 0
ITERATION: 1
Output Summary:
- BEFORE-HASH-FX=1BB53DBE378F6E6B69C42D9234061465CAD9CE79002E2AE9CF8004731449EFA6
- BEFORE-HASH-FT=40EE455C7D2ACF6ADA5D01B8880B37A7BCBAF320CEC9F83A8B497015EAE56F52
- FORMAT-EXIT=0 (console line `Formatted 2 files in 2309ms.` is a processed-file count, not the rewritten count)
- AFTER-HASH-FX=1BB53DBE378F6E6B69C42D9234061465CAD9CE79002E2AE9CF8004731449EFA6
- AFTER-HASH-FT=40EE455C7D2ACF6ADA5D01B8880B37A7BCBAF320CEC9F83A8B497015EAE56F52
- REWRITTEN-COUNT: 0 (both hashes identical before and after; equal to the P3-T1 AFTER hashes)
- CHECK-EXIT=0 with `Checked 2 files in 794ms.`
