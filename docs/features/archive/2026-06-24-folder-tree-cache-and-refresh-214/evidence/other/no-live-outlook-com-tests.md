# No Live Outlook COM Test Evidence

Issue: #214
Timestamp: 2026-06-24T18:50-04:00

Command:

```powershell
$patterns = 'new\s+(Microsoft\.Office\.Interop\.Outlook\.)?Application|new\s+Outlook\.Application|ApplicationClass|Activator\.CreateInstance|Marshal\.GetActiveObject|Interaction\.CreateObject|GetNamespace\("MAPI"\)|PickFolder\('; $tracked = git diff --name-only -- '*.cs' | Where-Object { $_ -match '(^|/|\\)[^/\\]*Test(s)?(/|\\)|\.Test(/|\\)|Test\.' }; $untracked = git ls-files --others --exclude-standard -- '*.cs' | Where-Object { $_ -match '(^|/|\\)[^/\\]*Test(s)?(/|\\)|\.Test(/|\\)|Test\.' }; $violations = New-Object System.Collections.Generic.List[string]; foreach ($path in $tracked) { $matches = git diff --unified=0 -- $path | Select-String -Pattern "^\+[^+].*($patterns)"; foreach ($match in $matches) { $violations.Add("TRACKED:${path}:$($match.Line)") } }; foreach ($path in $untracked) { $matches = Select-String -Path $path -Pattern $patterns; foreach ($match in $matches) { $violations.Add("UNTRACKED:${path}:$($match.LineNumber):$($match.Line.Trim())") } }; "Tracked test C# files scanned: $($tracked.Count)"; "Untracked test C# files scanned: $($untracked.Count)"; if ($violations.Count -eq 0) { 'No live Outlook COM construction found in issue #214 added test lines.'; exit 0 } else { $violations; exit 1 }
```

EXIT_CODE: 0

Output Summary:

- Tracked test C# files scanned: 7
- Untracked test C# files scanned: 31
- No live Outlook COM construction found in issue #214 added test lines.
