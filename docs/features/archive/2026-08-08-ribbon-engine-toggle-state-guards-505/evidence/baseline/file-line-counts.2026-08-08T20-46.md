# P0-T11 — Branch-Head Line Counts (ADVISORY)

> **ADVISORY.** The authoritative 500-line audit is **P5-T3**, measured after the final format
> pass. This artifact records the pre-change state only.

Timestamp: 2026-08-08T20-46

Command (as written in the plan):

```
pwsh -NoProfile -Command "Set-Location '<REPO>'; '<path list>' | ForEach-Object { '{0}={1}' -f $_, (Get-Content $_ | Measure-Object -Line).Lines }"
```

EXIT_CODE: 0

(`$LASTEXITCODE` was unset — the command invokes only cmdlets. The `pwsh` process exited 0.)

## Output Summary

| Path | `Measure-Object -Line` | True physical lines | Group |
|---|---|---|---|
| `TaskMaster\Ribbon\EngineCommandCatalog.cs` | 83 | **88** | scope lock (modified) |
| `TaskMaster\Ribbon\RibbonController.EngineCommands.cs` | 97 | **103** | scope lock (modified) |
| `TaskMaster\Ribbon\RibbonViewer.EngineCommands.cs` | 168 | **207** | scope lock (modified) |
| `TaskMaster\Ribbon\RibbonExplorer.xml` | 539 | **539** | scope lock (modified) — see note |
| `TaskMaster.Test\Ribbon\EngineCommandCatalogTests.cs` | 103 | **116** | scope lock (modified) |
| `TaskMaster.Test\Ribbon\RibbonExplorerXmlTests.cs` | 288 | **318** | scope lock (modified) |
| `TaskMaster\Ribbon\RibbonController.Intelligence.cs` | 360 | **412** | protected, zero-line diff |
| `TaskMaster\AppGlobals\AppItemEngines.cs` | 263 | **286** | protected, zero-line diff |
| `UtilitiesCS\Interfaces\IGlobals\IAppItemEngines.cs` | 16 | **18** | protected, zero-line diff |
| `TaskMaster\ThisAddIn.cs` | 271 | **307** | protected, zero-line diff |
| `TaskMaster\Ribbon\RibbonViewer.cs` | 299 | **388** | protected, zero-line diff |

### `RibbonExplorer.xml` — accepted pre-existing overage

`TaskMaster\Ribbon\RibbonExplorer.xml` is **539 lines**, above the 500-line cap. This is a
**pre-existing accepted overage** carried forward from #503: the file is a declarative embedded UI
resource (the Office CustomUI document), not production or test code, and AC-21 grants it an
explicit carve-out. This delivery adds only six `getEnabled` attributes to existing `<button>`
elements, each on its own line, so the overage grows marginally and is not remediated here.

### Measurement note (load-bearing for P5-T3)

`Get-Content <file> | Measure-Object -Line` **skips empty strings**, so it reports the count of
*non-blank* lines, not physical lines. The gap is material — 168 versus 207 for
`RibbonViewer.EngineCommands.cs` — and it under-reports, which is the unsafe direction for a
size cap (a file could pass the cap while physically exceeding it). The true physical counts above
were obtained with `@(Get-Content $_).Count` and match the branch-head values the plan tabulates
in section 4.2 exactly (88, 103, 207, 116, 318), confirming the plan's figures are physical-line
figures.

P5-T3 executes its stated command verbatim and additionally records the physical count for each
path; the 500-line binary outcome is evaluated against the **physical** count, which is the
stricter and correct measure.

Binary outcome: PASS (advisory record complete).
