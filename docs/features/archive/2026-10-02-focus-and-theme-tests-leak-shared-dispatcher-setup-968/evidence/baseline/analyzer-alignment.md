# Analyzer-path alignment (issue #968, task P0-T7)

Timestamp: 2026-10-03T02-43
Command: pwsh -NoProfile -Command 'Set-Location -LiteralPath "WORKTREE"; [System.IO.Directory]::SetCurrentDirectory((Get-Location).Path); Write-Output ("WORKTREE-LEAF: " + (Split-Path -Leaf (Get-Location).Path)); $root = (Get-Location).Path; $rootLen = $root.TrimEnd([char]92).Length; $projs = @(Get-ChildItem -Path $root -Recurse -Filter "*.csproj" | Where-Object { $rel = $_.FullName.Substring($rootLen); $rel -notlike "\packages\*" -and $rel -notlike "\.claude\*" }); "PROJECTS=$($projs.Count)"; $missing = 0; $skew = 0; foreach ($p in $projs) { $dir = $p.DirectoryName; [xml]$x = Get-Content -LiteralPath $p.FullName -Raw; foreach ($a in @($x.SelectNodes("//*[local-name()=""Analyzer""]"))) { $inc = $a.GetAttribute("Include"); if (-not (Test-Path -LiteralPath (Join-Path $dir $inc))) { $missing++; "MISSING " + $p.FullName.Substring($rootLen) + " :: " + $inc } }; $pc = Join-Path $dir "packages.config"; if (Test-Path -LiteralPath $pc) { [xml]$c = Get-Content -LiteralPath $pc -Raw; foreach ($id in @("Meziantou.Analyzer", "Roslynator.Analyzers")) { $pin = @($c.SelectNodes("//package[@id=""$id""]") | ForEach-Object { $_.GetAttribute("version") }); $inc = @($x.SelectNodes("//*[local-name()=""Analyzer""]") | ForEach-Object { $_.GetAttribute("Include") } | Where-Object { $_.Contains("\$id.") }); foreach ($i in $inc) { if ($pin.Count -eq 0 -or -not $i.Contains("\$id." + $pin[0] + "\")) { $skew++; "SKEW " + $p.FullName.Substring($rootLen) + " :: " + $i } } } } }; "ANALYZER_MISSING=$missing"; "VERSION_SKEW=$skew"'
Canonical command: analyzer include-path and version-pin alignment over every first-party *.csproj outside packages\ and .claude\
EXIT_CODE: 0
Output Summary:
- WORKTREE-LEAF: agent-a291a7fbabf9d0229
- PROJECTS=18
- ANALYZER_MISSING=0
- VERSION_SKEW=0
- No MISSING or SKEW lines were printed.
