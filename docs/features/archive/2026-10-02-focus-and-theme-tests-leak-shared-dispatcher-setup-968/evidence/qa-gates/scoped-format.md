# Scoped format (issue #968, task P6-T1)

Timestamp: 2026-10-03T03-16
Command: pwsh -NoProfile -Command 'Set-Location -LiteralPath "WORKTREE"; [System.IO.Directory]::SetCurrentDirectory((Get-Location).Path); Write-Output ("WORKTREE-LEAF: " + (Split-Path -Leaf (Get-Location).Path)); dotnet tool run csharpier format QuickFiler.Test\Controllers\QfcItemController.UiThreadDispatcherFixture.cs QuickFiler.Test\Controllers\QfcItemController.FocusAndThemeTests.cs QuickFiler.Test\Controllers\QfcItemController.TestSupport.cs QuickFiler.Test\Controllers\QfcItemController.UiThreadDispatcherFixtureTests.cs QuickFiler.Test\Controllers\QfcItemController.UiThreadDispatcherPinCountTests.cs QuickFiler.Test\Controllers\QfcDatamodelLivenessTests.cs QuickFiler.Test\Controllers\QfcDatamodelTeardownTests.cs QuickFiler.Test\Controllers\QfcInitEmailQueueZeroBatchTests.cs QuickFiler.Test\Controllers\QfcDatamodelTests.cs QuickFiler\Controllers\QfcDatamodel.cs QuickFiler\Controllers\QfcDatamodel.QueueProcessing.cs QuickFiler.Test\TestSupport\SynchronousBackgroundWorker.cs QuickFiler.Test\TestSupport\ArmingFakeTimeProvider.cs; "CSHARPIER_EXIT_CODE: $LASTEXITCODE"'
Canonical command: dotnet tool run csharpier format <the thirteen CS13 paths>, preceded and followed by CMD-HASH on CS13
EXIT_CODE: 0
Output Summary:
- WORKTREE-LEAF: agent-a291a7fbabf9d0229 (all three payloads)
- CSHARPIER_EXIT_CODE: 0
- `Formatted 13 files in 9724ms.` (a processed-file count, not a rewrite count)
- REWRITTEN: QuickFiler.Test\Controllers\QfcDatamodelLivenessTests.cs, QuickFiler.Test\Controllers\QfcInitEmailQueueZeroBatchTests.cs, QuickFiler.Test\Controllers\QfcDatamodelTests.cs (the rewrite observation is the hash difference)

Hashes before (CMD-HASH, exit 0) and after (CMD-HASH, exit 0):

| File | Before | After |
|---|---|---|
| QuickFiler.Test\Controllers\QfcItemController.UiThreadDispatcherFixture.cs | E22EAB9E3DBE8CBE79B9AD941FB8F9CF4679FCD2DA49287DF41EB06C90715A30 | E22EAB9E3DBE8CBE79B9AD941FB8F9CF4679FCD2DA49287DF41EB06C90715A30 |
| QuickFiler.Test\Controllers\QfcItemController.FocusAndThemeTests.cs | C8EBE51AD2810AEDD5B425100B12D99202D1EA8655A69D3F84004306BF6A053C | C8EBE51AD2810AEDD5B425100B12D99202D1EA8655A69D3F84004306BF6A053C |
| QuickFiler.Test\Controllers\QfcItemController.TestSupport.cs | 4EE3776F0C43AFB359B5B06734950E082822519E88B347B9F453D6B2814703FD | 4EE3776F0C43AFB359B5B06734950E082822519E88B347B9F453D6B2814703FD |
| QuickFiler.Test\Controllers\QfcItemController.UiThreadDispatcherFixtureTests.cs | 15B6334032EB4D8BD8DFC4AEACD943FE82857711584D1ECFB7CEA28A08ADB54C | 15B6334032EB4D8BD8DFC4AEACD943FE82857711584D1ECFB7CEA28A08ADB54C |
| QuickFiler.Test\Controllers\QfcItemController.UiThreadDispatcherPinCountTests.cs | 0CE75AF833D376769D92875AAE467AE92107E02ADD8B7BA58FAC9A37190F7C32 | 0CE75AF833D376769D92875AAE467AE92107E02ADD8B7BA58FAC9A37190F7C32 |
| QuickFiler.Test\Controllers\QfcDatamodelLivenessTests.cs | 310E0DE66BF2FB90E53A28C3AA20CF7B5105D5E514898F7681D027686E3BF24A | 705AFA3C0383066D4469B5F3E229946A9DA7B57C06CE7BDC8892947D7593EE34 |
| QuickFiler.Test\Controllers\QfcDatamodelTeardownTests.cs | 1AD5B22646DE2AB6311D7CF864B1B52DB088839C47AB2615A79F0BCF4F942523 | 1AD5B22646DE2AB6311D7CF864B1B52DB088839C47AB2615A79F0BCF4F942523 |
| QuickFiler.Test\Controllers\QfcInitEmailQueueZeroBatchTests.cs | F62E707199464578315FF1BE6E1CA79F6E460DC115C6A75875288CE70CF0C181 | 17500EA75C8E7B442F77E853901A7B40E290F414E5016851079B159A9292DA6D |
| QuickFiler.Test\Controllers\QfcDatamodelTests.cs | 9DF350D6C90593D3D66A7E3556AC6E069CEC5C0A13B093A4492643A355D8DE8A | 5FF13DCBD54022AC4C298A7FA32151D3A5BA39AED7CB42AB06C894C40733FE6A |
| QuickFiler\Controllers\QfcDatamodel.cs | 7D9E620EF27B587E9A69667F13514E1A40144A5887CFAF5D7E0C08D0E1CA089D | 7D9E620EF27B587E9A69667F13514E1A40144A5887CFAF5D7E0C08D0E1CA089D |
| QuickFiler\Controllers\QfcDatamodel.QueueProcessing.cs | 577285E41E88EF7A64C15CFBFF69E2C3264BB3D92B51384F5B42DECF1CA3C732 | 577285E41E88EF7A64C15CFBFF69E2C3264BB3D92B51384F5B42DECF1CA3C732 |
| QuickFiler.Test\TestSupport\SynchronousBackgroundWorker.cs | 2AD708EA88DD1FEE83A30E72910F0D2E4A97A33B9B4D1BA4085ADA5D0CA4D65A | 2AD708EA88DD1FEE83A30E72910F0D2E4A97A33B9B4D1BA4085ADA5D0CA4D65A |
| QuickFiler.Test\TestSupport\ArmingFakeTimeProvider.cs | 6B45C5018B062F17C4C08B37F1938C9E64C78DD260F2F87FFA19881CBEA4E9E9 | 6B45C5018B062F17C4C08B37F1938C9E64C78DD260F2F87FFA19881CBEA4E9E9 |

This is the first P6-T1 pass of the run (no D-13 restart), so PRIOR-PASS-REWRITTEN and RESTART-CORRECTED are not applicable.
