Timestamp: 2026-07-04T13-15
Command: PowerShell XML parser over docs\features\active\2026-07-04-coverage-gaps-test-seams-236\evidence\baseline\baseline-coverage.cobertura.xml
EXIT_CODE: 0
Output Summary: Baseline Cobertura target coverage values were parsed from exact class entries. Repository line coverage is 44.60%. Target baseline coverage: EfcViewerQueue 0.00%, ItemViewerQueue 0.00%, QfcThemeHelper 0.00%, EfcHomeController 15.87%, TlpCellStates 62.20%.

Parser Method:
```powershell
$xml = [xml](Get-Content -LiteralPath 'docs\features\active\2026-07-04-coverage-gaps-test-seams-236\evidence\baseline\baseline-coverage.cobertura.xml')
$xml.SelectNodes('//class') | Where-Object { $_.name -like "*<target>*" -or $_.filename -like "*<target>*" }
```

Baseline Coverage Rows:
| Target | Cobertura Entry | File | CoverageSource | Line Rate | Percent |
| --- | --- | --- | --- | ---: | ---: |
| Repository | coverage root | baseline-coverage.cobertura.xml | root | 0.44603 | 44.60% |
| EfcViewerQueue | QuickFiler.EfcViewerQueue | QuickFiler\Helper Classes\EfcViewerQueue.cs | class | 0 | 0.00% |
| ItemViewerQueue | QuickFiler.ItemViewerQueue | QuickFiler\Helper Classes\ItemViewerQueue.cs | class | 0 | 0.00% |
| QfcThemeHelper | QuickFiler.QfcThemeHelper | QuickFiler\Helper Classes\QfcThemeHelper.cs | class | 0 | 0.00% |
| EfcHomeController | QuickFiler.EfcHomeController | QuickFiler\Controllers\EfcHomeController.cs | class | 0.15873 | 15.87% |
| TlpCellStates | QuickFiler.TlpCellStates | QuickFiler\Helper Classes\TlpCellSnapShot.cs | class | 0.622047 | 62.20% |

Remediation Status:
- REMEDIATION_REQUIRED: none for baseline target parsing.
- File-plus-changed-line fallback was not required because each target had an exact class entry.
