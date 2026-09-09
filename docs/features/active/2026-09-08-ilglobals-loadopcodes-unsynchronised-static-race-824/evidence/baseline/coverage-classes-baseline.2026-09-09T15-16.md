# Baseline per-class coverage extract (Issue #824, task P0-T13)

Timestamp: 2026-09-09T15-16

Command: `pwsh -NoProfile -Command 'Set-Location "<worktree-root>"; [xml]$d = Get-Content coverage/baseline.cobertura.xml -Raw; Write-Output ("DOC_LINE_RATE=" + $d.SelectSingleNode("/coverage").GetAttribute("line-rate")); foreach ($n in @("SDILReader.ILGlobals","SDILReader.MethodBodyReader")) { $c = @($d.SelectNodes("//class") | Where-Object { $_.GetAttribute("name") -eq $n }); Write-Output ($n + " COUNT=" + $c.Count); foreach ($e in $c) { Write-Output ($n + " FILE=" + $e.GetAttribute("filename") + " LINE=" + $e.GetAttribute("line-rate") + " BRANCH=" + $e.GetAttribute("branch-rate")) } }'`

EXIT_CODE: 0

Output Summary:

Document-level `/coverage/@line-rate` = 0.856204 (85.6204 %).

| Class | Elements found | Source file | `line-rate` | `branch-rate` |
|---|---|---|---|---|
| `SDILReader.ILGlobals` | 1 | `UtilitiesCS\NewtonsoftHelpers\SDIL Reader\ILGlobals.cs` | 0.9459459459459459 | 0.875 |
| `SDILReader.MethodBodyReader` | 1 | `UtilitiesCS\NewtonsoftHelpers\SDIL Reader\MethodBodyReader.cs` | 0.9732620320855615 | 0.8947368421052632 |

Each name reported an element count of exactly 1, so the mechanical disambiguation rule this task
states — when a count exceeds 1, select the element whose `filename` attribute names the owned
production file and record every element found — did not need to be applied. Both `filename`
attributes name the owned production file for their class. The filename separators are backslashes,
which is how the processed Cobertura document spells them.

These four numeric values are the AC12 baseline. P5-T10 re-runs this command against
`coverage/post-change.cobertura.xml` and P5-T11 compares the two sets.

No raw Cobertura document is copied into the feature folder. Per plan D7 and D22, the coverage
document of record for AC12 is this markdown extract, whose values were read directly from
`coverage/baseline.cobertura.xml` by the command above.
