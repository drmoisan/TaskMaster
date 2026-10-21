# Post-change per-class coverage extract (Issue #824, task P5-T10)

Timestamp: 2026-09-09T16-10

Command: `pwsh -NoProfile -Command 'Set-Location "<worktree-root>"; [xml]$d = Get-Content coverage/post-change.cobertura.xml -Raw; Write-Output ("DOC_LINE_RATE=" + $d.SelectSingleNode("/coverage").GetAttribute("line-rate")); foreach ($n in @("SDILReader.ILGlobals","SDILReader.MethodBodyReader")) { $c = @($d.SelectNodes("//class") | Where-Object { $_.GetAttribute("name") -eq $n }); Write-Output ($n + " COUNT=" + $c.Count); foreach ($e in $c) { Write-Output ($n + " FILE=" + $e.GetAttribute("filename") + " LINE=" + $e.GetAttribute("line-rate") + " BRANCH=" + $e.GetAttribute("branch-rate")) } }'`

This is the P0-T13 command run unchanged against `coverage/post-change.cobertura.xml`.

EXIT_CODE: 0

Output Summary:

Document-level `/coverage/@line-rate` = 0.856241 (85.6241 %).

| Class | Elements found | Source file | `line-rate` | `branch-rate` |
|---|---|---|---|---|
| `SDILReader.ILGlobals` | 1 | `UtilitiesCS\NewtonsoftHelpers\SDIL Reader\ILGlobals.cs` | 0.95 | 0.875 |
| `SDILReader.MethodBodyReader` | 1 | `UtilitiesCS\NewtonsoftHelpers\SDIL Reader\MethodBodyReader.cs` | 0.9732620320855615 | 0.8947368421052632 |

Each name reported an element count of exactly 1, matching the baseline extract, so the mechanical
disambiguation rule — when a count exceeds 1, select the element whose `filename` attribute names the
owned production file and record every element found — did not need to be applied. Both `filename`
attributes name the owned production file for their class.

The comparison against the baseline values, the changed-line analysis, and the threshold reporting
are performed by P5-T11.

No raw Cobertura document is copied into the feature folder. Per plan D7 and D22 the coverage
document of record for AC12 on the post-change side is this markdown extract, whose values were read
directly from `coverage/post-change.cobertura.xml` by the command above.
