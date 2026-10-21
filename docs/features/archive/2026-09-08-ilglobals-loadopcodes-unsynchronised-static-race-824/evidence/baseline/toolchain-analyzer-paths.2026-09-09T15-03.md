# Toolchain baseline — analyzer reference resolution (Issue #824, task P0-T5)

Timestamp: 2026-09-09T15-03

Command: `pwsh -NoProfile -Command 'Set-Location "<worktree-root>"; $m = 0; Get-ChildItem -Path . -Recurse -Filter *.csproj | Where-Object { $rel = [System.IO.Path]::GetRelativePath((Get-Location).Path, $_.FullName); ($rel -notmatch "^packages") -and ($rel -notmatch "^[.]dotnet") -and ($rel -notmatch "^[.]claude") } | ForEach-Object { $p = $_; [xml]$x = Get-Content $p.FullName -Raw; foreach ($a in $x.GetElementsByTagName("Analyzer")) { $inc = $a.GetAttribute("Include"); if ($inc) { if (-not (Test-Path (Join-Path $p.DirectoryName $inc))) { $m = $m + 1; Write-Output ("MISSINGPATH " + $p.Name + " " + $inc) } } } }; Write-Output ("MISSING=" + $m)'`

EXIT_CODE: 0

Output Summary:

```
MISSING=0
```

No `MISSINGPATH` line was emitted. Every `<Analyzer Include>` item in every first-party project file
resolves to an existing file on disk after the P0-T4 restore. A missing analyzer path is
`error CS0006` at compile time rather than a warning, so a non-zero count would have blocked Phase 0
until the named package folders were restored.
