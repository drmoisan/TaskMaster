# P5 popup-surface ownership reconciliation

Timestamp: 2026-07-22T04:55:26.1148688Z

Command: `$files=@('QuickFiler/Viewers/BreadcrumbDropDownHost.cs','QuickFiler/Viewers/ItemViewer.Breadcrumb.cs','QuickFiler/Viewers/BreadcrumbPopupUiOperations.cs','QuickFiler.Test/Viewers/BreadcrumbSelectorToggleUiBoundaryTests.cs','QuickFiler.Test/Viewers/BreadcrumbPopupControlDispatchTests.cs','QuickFiler.Test/Viewers/BreadcrumbSelectorOpenRetryTests.cs'); 'UTC='+(Get-Date -AsUTC -Format 'yyyy-MM-ddTHH:mm:ss.fffffffZ'); foreach($file in $files){'{0}|LINES={1}|SHA256={2}|STATUS={3}' -f $file,(Get-Content -LiteralPath $file).Count,(Get-FileHash -Algorithm SHA256 -LiteralPath $file).Hash,((git status --short -- $file) -join ' ')}; 'TEST_INCLUDE_TOGGLE='+(Select-String -Path 'QuickFiler.Test/QuickFiler.Test.csproj' -SimpleMatch 'Viewers\BreadcrumbSelectorToggleUiBoundaryTests.cs').Count; 'TEST_INCLUDE_POPUP='+(Select-String -Path 'QuickFiler.Test/QuickFiler.Test.csproj' -SimpleMatch 'Viewers\BreadcrumbPopupControlDispatchTests.cs').Count; 'TEST_INCLUDE_RETRY='+(Select-String -Path 'QuickFiler.Test/QuickFiler.Test.csproj' -SimpleMatch 'Viewers\BreadcrumbSelectorOpenRetryTests.cs').Count; 'POPUP_OPS_INCLUDE='+(Select-String -Path 'QuickFiler/QuickFiler.csproj' -SimpleMatch 'Viewers\BreadcrumbPopupUiOperations.cs').Count; 'EXCLUDED=QuickFiler/Viewers/BreadcrumbDropDownOpenLifetime.cs'; git diff --check -- @files; 'EXIT_CODE='+$LASTEXITCODE`

EXIT_CODE: 0

Output Summary: This is a read-only current-state ownership receipt. It did not start an editing worker and did not modify a production or test file. The historical composition handoff edited four production files in one worker and was therefore noncompliant with the current three-production/three-test cap; this receipt preserves those edits without claiming that historical handoff was compliant. `BreadcrumbDropDownOpenLifetime.cs` is explicitly excluded and is reconciled separately by P5-T19.

| Owned path | Lines | SHA-256 | Future bounded edit owner |
|---|---:|---|---|
| `QuickFiler/Viewers/BreadcrumbDropDownHost.cs` | 467 | `C510C2B869275298FF61BE346B7553F864F87B0E77A86C91DC060ED139C404A9` | P5-T36 through P5-T42 |
| `QuickFiler/Viewers/ItemViewer.Breadcrumb.cs` | 496 | `FB6137B5A5C9513953C2CE09495C046F8951905DB7E38561452C64E6E21ED9AB` | P5-T56 through P5-T62 |
| `QuickFiler/Viewers/BreadcrumbPopupUiOperations.cs` | 501 | `329E4FF0ED3985BFB06BD6F827FDF8BEF601D08708A61E9E07AA8303561B12DE` | P5-T29 through P5-T35 |
| `QuickFiler.Test/Viewers/BreadcrumbSelectorToggleUiBoundaryTests.cs` | 493 | `F445DC19960167E48D53B6FF53C2E996966FC70074D864E5207B05736C0D4A19` | P5-T22 through P5-T28, P5-T36 through P5-T42, and P5-T56 through P5-T62 |
| `QuickFiler.Test/Viewers/BreadcrumbPopupControlDispatchTests.cs` | 513 | `114FC797A04D9BA27BEE0F7343568338167CB9248806C3B2805315CB7653D3EC` | P5-T22 through P5-T35 and P5-T56 through P5-T62 |
| `QuickFiler.Test/Viewers/BreadcrumbSelectorOpenRetryTests.cs` | 503 | `148F8AF11D9EC73CE767E89E4F0FD44D6C6A475C5F6335F22C0F46F51FE84CE6` | P5-T22 through P5-T42 and P5-T56 through P5-T62 |

Each of the three test files and `BreadcrumbPopupUiOperations.cs` has exactly one adjacent legacy-project `Compile` entry. Scoped `git diff --check` returned zero; the displayed LF/CRLF notice is not a whitespace error. No production or test file is authorized for editing by this receipt.
