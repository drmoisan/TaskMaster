# Banned API Search Evidence

Issue: #214
Timestamp: 2026-06-24T18:49-04:00

Command:

```powershell
$patterns = 'Application\.DoEvents|DateTime\.Now|DateTime\.UtcNow|Random\.Shared|Thread\.Sleep|Task\.Delay'; $tracked = git diff --name-only -- '*.cs'; $untracked = git ls-files --others --exclude-standard -- '*.cs'; $violations = New-Object System.Collections.Generic.List[string]; foreach ($path in $tracked) { $matches = git diff --unified=0 -- $path | Select-String -Pattern "^\+[^+].*($patterns)"; foreach ($match in $matches) { $violations.Add("TRACKED:${path}:$($match.Line)") } }; foreach ($path in $untracked) { $matches = Select-String -Path $path -Pattern $patterns; foreach ($match in $matches) { $violations.Add("UNTRACKED:${path}:$($match.LineNumber):$($match.Line.Trim())") } }; "Tracked C# files scanned: $($tracked.Count)"; "Untracked C# files scanned: $($untracked.Count)"; if ($violations.Count -eq 0) { 'No banned API usage found in issue #214 added C# lines.'; exit 0 } else { $violations; exit 1 }
```

EXIT_CODE: 0

Output Summary:

- Tracked C# files scanned: 16
- Untracked C# files scanned: 59
- No banned API usage found in issue #214 added C# lines.
- Initial attempt failed before scanning because PowerShell parsed `$path:` as an invalid variable reference; the corrected command above completed successfully.
