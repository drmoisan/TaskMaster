# P5 open-lifetime ownership reconciliation

Timestamp: 2026-07-22T04:56:03.6452910Z

Command: `$files=@('QuickFiler/Viewers/BreadcrumbDropDownOpenLifetime.cs','QuickFiler.Test/Viewers/BreadcrumbSelectorToggleUiBoundaryTests.cs','QuickFiler.Test/Viewers/BreadcrumbPopupControlDispatchTests.cs','QuickFiler.Test/Viewers/BreadcrumbSelectorOpenRetryTests.cs'); 'UTC='+(Get-Date -AsUTC -Format 'yyyy-MM-ddTHH:mm:ss.fffffffZ'); foreach($file in $files){'{0}|LINES={1}|SHA256={2}|STATUS={3}' -f $file,(Get-Content -LiteralPath $file).Count,(Get-FileHash -Algorithm SHA256 -LiteralPath $file).Hash,((git status --short -- $file) -join ' ')}; 'LIFETIME_INCLUDE='+(Select-String -Path 'QuickFiler/QuickFiler.csproj' -SimpleMatch 'Viewers\BreadcrumbDropDownOpenLifetime.cs').Count; Select-String -Path 'QuickFiler/Viewers/BreadcrumbDropDownOpenLifetime.cs' -Pattern 'lock \(_sync\)|InvalidateCore|IsCurrent\(|PostAsync|RunAsync|CompleteOpenAsync|ObserveScheduledAsync' | ForEach-Object {'OBLIGATION_LINE={0}:{1}' -f $_.LineNumber,$_.Line.Trim()}; 'EXCLUDED=QuickFiler/Viewers/BreadcrumbDropDownHost.cs;QuickFiler/Viewers/ItemViewer.Breadcrumb.cs;QuickFiler/Viewers/BreadcrumbPopupUiOperations.cs'; git diff --check -- @files; 'EXIT_CODE='+$LASTEXITCODE`

EXIT_CODE: 0

Output Summary: This is a separate read-only current-state ownership receipt for the open-lifetime helper. It did not start an editing worker or modify production/tests. `BreadcrumbDropDownHost.cs`, `ItemViewer.Breadcrumb.cs`, and `BreadcrumbPopupUiOperations.cs` are explicitly excluded. The helper has exactly one adjacent `QuickFiler.csproj` include. This receipt preserves the historical work without claiming the earlier four-production-file handoff complied with the current batch cap.

| Owned path | Lines | SHA-256 | Future bounded edit owner |
|---|---:|---|---|
| `QuickFiler/Viewers/BreadcrumbDropDownOpenLifetime.cs` | 411 | `71FDE1A60A58E52626F69E965815F875DD5D1E78528160CE28E46CC282040CB2` | P5-T36 through P5-T55 |
| `QuickFiler.Test/Viewers/BreadcrumbSelectorToggleUiBoundaryTests.cs` | 493 | `F445DC19960167E48D53B6FF53C2E996966FC70074D864E5207B05736C0D4A19` | P5-T22 through P5-T28, P5-T36 through P5-T42, and P5-T56 through P5-T62 |
| `QuickFiler.Test/Viewers/BreadcrumbPopupControlDispatchTests.cs` | 513 | `114FC797A04D9BA27BEE0F7343568338167CB9248806C3B2805315CB7653D3EC` | P5-T22 through P5-T35 and P5-T56 through P5-T62 |
| `QuickFiler.Test/Viewers/BreadcrumbSelectorOpenRetryTests.cs` | 503 | `148F8AF11D9EC73CE767E89E4F0FD44D6C6A475C5F6335F22C0F46F51FE84CE6` | P5-T22 through P5-T42 and P5-T56 through P5-T62 |

Current obligations retained for P5-T20 reverification are: generation/cancellation invalidation remains lock-protected; canceled completion is performed outside the lock; open kickoff and lifecycle operations use the owning dispatcher; shared open completion checks the current lease; placement, show, focus, retention, publication, and rollback paths re-enter through `RunAsync` and apply current-generation checks; and scheduled lifecycle tasks are observed. P5-T36 through P5-T55 must correct the identified Dispose race and one-pass primary rollback before these obligations can be approved. Scoped `git diff --check` returned zero.
