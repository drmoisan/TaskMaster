Timestamp: 2026-08-31T11:01:18-04:00
Command 1: pwsh -NoProfile -Command '. ".\scripts\vscode\Invoke-MSTestWithCoverage.Helpers.ps1"; $raw = Get-Content -LiteralPath ".\coverage\p7-t5-postchange.cobertura.xml" -Raw -Encoding UTF8; [xml]$d = ConvertTo-KoverageCoberturaXml -XmlContent $raw -RepoRoot (Get-Location).Path; foreach ($f in @("QuickFiler\Controllers\BreadcrumbBridgeRouter.Selection.cs","QuickFiler\Controllers\EfcDataModel.cs","QuickFiler\Controllers\EfcDataModel.FilingStem.cs")) { $u = @(); foreach ($c in $d.SelectNodes("//class")) { if ($c.GetAttribute("filename") -eq $f) { foreach ($l in $c.SelectNodes("./lines/line")) { if ([int]$l.GetAttribute("hits") -eq 0) { $u += [int]$l.GetAttribute("number") } } } }; $f + " uncovered=" + (($u | Sort-Object -Unique) -join ",") }'
Command 2: git diff -U0 0eda184ca0009bc79ac9b7146897270c17c095fa..HEAD -- QuickFiler/Controllers/BreadcrumbBridgeRouter.Selection.cs QuickFiler/Controllers/EfcDataModel.cs QuickFiler/Controllers/EfcDataModel.FilingStem.cs
EXIT_CODE: 0
Output Summary: No changed line is uncovered. The new helper has coverage rows on 13 lines, all with non-zero hits. Its `IsFullOutlookPath` conditional is recorded as branch=True with condition-coverage 100% (6/6), demonstrating both conditional outcomes.

Added lines from the anchored diff:
- QuickFiler/Controllers/BreadcrumbBridgeRouter.Selection.cs: 97-115.
- QuickFiler/Controllers/EfcDataModel.cs: 21, 337.
- QuickFiler/Controllers/EfcDataModel.FilingStem.cs: 1-29.

Coverage rows with zero hits:
- QuickFiler/Controllers/BreadcrumbBridgeRouter.Selection.cs: 29, 30, 205.
- QuickFiler/Controllers/EfcDataModel.cs: 181, 183-185, 189-195, 197-208, 210-221, 248-250, 253-254, 345-346, 362-368, 370-371, 386-392, 394-395, 406-419, 451-454, 457-458, 460, 463-466, 467, 469-475, 478, 480-481.
- QuickFiler/Controllers/EfcDataModel.FilingStem.cs: none.

Changed-line intersections:
- QuickFiler/Controllers/BreadcrumbBridgeRouter.Selection.cs: empty.
- QuickFiler/Controllers/EfcDataModel.cs: empty.
- QuickFiler/Controllers/EfcDataModel.FilingStem.cs: empty.

ToFilingStemOrVerbatim range:
- Re-derived declaration line: 11.
- Re-derived closing-brace line: 27.
- P4-T2 recorded range: 11-27.
- Comparison: unchanged after formatting.
- Coverage rows within the re-derived range: 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 26; each has hits=1.
- Lines with no coverage row: 11 (method signature), 24 (brace), 25 (blank line), 27 (brace).
- At least one range line carries a coverage row: yes.
- Branch coverage row: line 13 has branch="True" and condition-coverage="100% (6/6)". This is the `IsFullOutlookPath` conditional and records both outcomes.
