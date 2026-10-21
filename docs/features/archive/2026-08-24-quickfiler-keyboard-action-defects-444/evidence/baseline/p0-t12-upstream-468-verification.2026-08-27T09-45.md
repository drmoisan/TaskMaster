# [P0-T12] Upstream #468 halt gate — `WireUpKeyboardHandler` zero-hit verification

Timestamp: 2026-08-27T09-45
EXIT_CODE: 0

## Command (verbatim, run from `WS` under `pwsh -NoProfile`)

```powershell
$files = @(Get-ChildItem -Path . -Recurse -Filter *.cs |
  Where-Object { $_.FullName -notlike '*\obj\*' -and $_.FullName -notlike '*\bin\*' } |
  ForEach-Object { Resolve-Path -Relative $_.FullName } |
  Where-Object { $_ -notlike '*\.claude\*' })
$hits = @(Select-String -SimpleMatch -Pattern 'WireUpKeyboardHandler' -Path $files)
$hits.Count
```

The candidate set is reduced to paths relative to `WS` before the `.claude` filter is applied, so a
nested agent worktree holding a pre-#468 copy cannot produce a false halt. The search is run with
`-Path $files` rather than by piping the collection: `Select-String` searches piped strings as
content, which would return `0` unconditionally and make this gate vacuous.

## Observed

| Measure | Value |
| --- | --- |
| `$files.Count` | 1580 |
| `$files` contains `.\QuickFiler\Controllers\QfcCollectionController.cs` | True |
| Recorded hit count | **0** |
| Hit paths | none |

## Acceptance evaluation

- `$files` is non-empty (1580 members). PASS.
- `$files` includes `.\QuickFiler\Controllers\QfcCollectionController.cs`. PASS.
- Recorded hit count is exactly `0`. PASS.

`BLOCKED: upstream #468 has not landed on this branch` is **not** written: the gate passed.

Upstream #468 task `[P1-T2]` deleted `WireUpKeyboardHandler`, the dead method containing the duplicate
`("Collection", Keys.Down)` registration, and thereby resolved that duplicate as a side effect. No
source file was edited by this task. The deleted block was **not** recreated in order to remove it.

Output Summary: 1580 non-build `.cs` files scanned relative to `WS` with `.claude`-segment paths
excluded; zero hits for `WireUpKeyboardHandler`; upstream #468 confirmed landed; no halt.
