# P4-T1 — Guarded-Site Count Audit (AC-11)

Timestamp: 2026-08-08T21-09

Command:

```
pwsh -NoProfile -Command "Set-Location '<REPO>'; Select-String -Path 'TaskMaster\Ribbon\RibbonViewer.EngineCommands.cs' -Pattern 'Engines\.' | ForEach-Object { '{0}: {1}' -f $_.LineNumber, $_.Line.Trim() }"
```

EXIT_CODE: 0

(`$LASTEXITCODE` was unset — `Select-String` is a cmdlet, not an external process. The `pwsh`
process exited 0.)

## Output Summary

Every remaining `Engines.` occurrence in the post-change file, quoted verbatim with its
classification:

| Line | Occurrence | Classification |
|---|---|---|
| 146 | `(SpamBayes)Controller.Engines.InboxEngines[SpamBayes.GroupName].Engine` | Inside the `Func<Task>` lambda of `TestSpam_Click`'s `RunEngineCommandAsync("TestSpam", ...)` — the **pre-existing** #503 gated site. |
| 194 | `() => Controller.Engines.ShowDiskDialog(SpamBayes.GroupName, false)` | Inside the lambda of `SpamSaveNetwork_Click`'s `RunEngineCommandAsync("SpamSaveNetwork", ...)`. |
| 200 | `() => Controller.Engines.ShowDiskDialog(SpamBayes.GroupName, true)` | Inside the lambda of `SpamSaveLocal_Click`'s `RunEngineCommandAsync("SpamSaveLocal", ...)`. |
| 213 | `Controller.Engines.ShowSaveInfo(SpamBayes.GroupName);` | Inside the statement-bodied lambda of `GetSaveLocation_Click`'s `RunEngineCommandAsync("GetSaveState", ...)`. |
| 300 | `() => Controller.Engines.ShowDiskDialog("Triage", false)` | Inside the lambda of `TriageSaveNetwork_Click`'s `RunEngineCommandAsync("TriageSaveNetwork", ...)`. |
| 306 | `() => Controller.Engines.ShowDiskDialog("Triage", true)` | Inside the lambda of `TriageSaveLocal_Click`'s `RunEngineCommandAsync("TriageSaveLocal", ...)`. |
| 319 | `Controller.Engines.ShowSaveInfo("Triage");` | Inside the statement-bodied lambda of `TriageGetSaveLocation_Click`'s `RunEngineCommandAsync("TriageGetSaveState", ...)`. |

The four toggle/`getPressed` sites carry **no** `Engines.` occurrence at all: they now route
through `_controller?.IsEngineToggleActive(...)` and `Controller.HandleEngineToggleClickAsync(...)`,
which reach the engines behind `EngineToggleStateCoordinator`'s null-tolerant accessor.

## Counts

- **Newly guarded sites: 10** — the 4 toggle/`getPressed` sites (rerouted through the coordinator,
  so their dereference no longer appears in this file) plus the 6 command sites (dereference
  deferred into a `RunEngineCommandAsync` lambda).
- **Pre-existing gated site: 1** — `TestSpam_Click` (line 146).
- **Unguarded production dereferences: 0.**

Total `Engines.` occurrences fell from 11 (pre-change) to 7, because the four toggle-site
dereferences moved out of the viewer entirely.

## `TestSpam_Click` is functionally unchanged

`git diff <MERGE_BASE>..HEAD -- TaskMaster/Ribbon/RibbonViewer.EngineCommands.cs | grep -c 'TestSpam'`
returns **0**: no diff hunk touches `TestSpam_Click`, so its text is byte-identical to the
merge-base version.

Binary outcome: PASS.
