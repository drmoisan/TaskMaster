# Popup UI-boundary composition CSharpier gate

Timestamp: 2026-07-22T04:25:06.7447637Z

Command: `$files=@('QuickFiler/Viewers/BreadcrumbDropDownHost.cs','QuickFiler/Viewers/ItemViewer.Breadcrumb.cs','QuickFiler/Viewers/BreadcrumbPopupUiOperations.cs','QuickFiler/Viewers/BreadcrumbDropDownOpenLifetime.cs','QuickFiler.Test/Viewers/BreadcrumbSelectorToggleUiBoundaryTests.cs','QuickFiler.Test/Viewers/BreadcrumbPopupControlDispatchTests.cs','QuickFiler.Test/Viewers/BreadcrumbSelectorOpenRetryTests.cs'); $before=@{}; foreach($f in $files){$before[$f]=(Get-FileHash -Algorithm SHA256 -LiteralPath $f).Hash}; csharpier format @files; $exit=$LASTEXITCODE; 'EXIT_CODE=' + $exit; foreach($f in $files){$after=(Get-FileHash -Algorithm SHA256 -LiteralPath $f).Hash; '{0}|CHANGED={1}|LINES={2}|BEFORE={3}|AFTER={4}' -f $f,($before[$f] -ne $after),(Get-Content -LiteralPath $f).Count,$before[$f],$after}; exit $exit`

EXIT_CODE: 0

Output Summary: CSharpier formatted all seven composition-batch files in 1870 ms. Every before/after SHA-256 pair was identical, so the gate was stable and did not restart the batch. All files remain below 500 lines with structural headroom.

| File | Lines | Stable SHA-256 |
|---|---:|---|
| `BreadcrumbDropDownHost.cs` | 467 | `7E2D3E43C409147D1F42AD96AED3349C5040C6A1D8E1EB9BAE9795ADC160DAF7` |
| `ItemViewer.Breadcrumb.cs` | 496 | `FB6137B5A5C9513953C2CE09495C046F8951905DB7E38561452C64E6E21ED9AB` |
| `BreadcrumbPopupUiOperations.cs` | 488 | `835615DA6AEF0CF89F22D059D4BBE9E5E3ECEE0E6E27EB4A41626FF9C8EE316D` |
| `BreadcrumbDropDownOpenLifetime.cs` | 411 | `204AE06C5F689B6BD3C75C4224624438AA2FC2F5D7B7A043261DADFAD0A8A00C` |
| `BreadcrumbSelectorToggleUiBoundaryTests.cs` | 493 | `6B76A012D2FC60CAC063F035F2D45CEBB69E0AB562BB3A495B8B3CE3FB63ED45` |
| `BreadcrumbPopupControlDispatchTests.cs` | 495 | `1DF205C80A944452F319B78D231FC532C164622F4BF40A6F4E0C9174AEBBC9B5` |
| `BreadcrumbSelectorOpenRetryTests.cs` | 498 | `C344CCC73BE6DB7AEEDD85A6C37B260CFC6B2102BF371E3052B98BABA36264E6` |
