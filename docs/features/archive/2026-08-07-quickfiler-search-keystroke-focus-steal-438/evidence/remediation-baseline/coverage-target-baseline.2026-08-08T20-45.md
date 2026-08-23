## [P0-T4] R1 Target Method Coverage Baseline (from Cycle-1 artifact)

- Timestamp: 2026-08-08T20-45
- Command: `pwsh -NoProfile -Command "[xml]$x = Get-Content docs/features/active/2026-08-07-quickfiler-search-keystroke-focus-steal-438/evidence/qa-gates/coverage-final.cobertura.xml; $m = $x.SelectNodes('//class[@filename=\"QuickFiler\Viewers\BreadcrumbItemViewerLifecycleCoordinator.Search.cs\"]/methods/method[@name=\"PresentSearchResults\"]'); if ($m.Count -ne 1) { Write-Error \"Expected exactly 1 matching node, found $($m.Count)\" ; exit 1 } ; $m | ForEach-Object { $_.'line-rate'; $_.'branch-rate' } ; exit $LASTEXITCODE"`
- EXIT_CODE: 0
- Output Summary: Node count = 1 (backslash-form XPath predicate on `filename` attribute matched exactly one `method[@name="PresentSearchResults"]` node). `line-rate` = `1` (5/5). `branch-rate` = `0.5` (2/4) — confirms R1's stated measurement before any remediation edit.

### Source artifact

`docs/features/active/2026-08-07-quickfiler-search-keystroke-focus-steal-438/evidence/qa-gates/coverage-final.cobertura.xml` (Cycle-1, read-only, unchanged tree per P0-T2 baseline).
