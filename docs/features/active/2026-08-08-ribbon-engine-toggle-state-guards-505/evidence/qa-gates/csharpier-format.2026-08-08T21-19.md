> **SUPERSEDED — attempt 1 of Phase 5.** This pass was aborted at P5-T6 by an environmental
> failure in `QuickFiler.Test` (see
> `<FEATURE>\evidence\other\phase5-attempt1-aborted.2026-08-08T21-30.md`), and the phase was
> restarted at P5-T1. The authoritative Phase 5 evidence is the second, uninterrupted pass at
> timestamps `2026-08-08T21-3x`. This artifact is retained as an audit trail only.
# P5-T1 — CSharpier Format (scope-locked paths only)

Timestamp: 2026-08-08T21-19

Command:

```
pwsh -NoProfile -Command "Set-Location '<REPO>'; & '<CSHARPIER>' format TaskMaster\Ribbon\EngineToggleCatalog.cs TaskMaster\Ribbon\EngineToggleStateCoordinator.cs TaskMaster\Ribbon\EngineCommandCatalog.cs TaskMaster\Ribbon\RibbonController.EngineCommands.cs TaskMaster\Ribbon\RibbonViewer.EngineCommands.cs TaskMaster.Test\Ribbon\RibbonViewerEngineCallbackShapeTests.cs TaskMaster.Test\Ribbon\EngineToggleCatalogTests.cs TaskMaster.Test\Ribbon\EngineToggleStateCoordinatorTests.cs TaskMaster.Test\Ribbon\EngineCommandCatalogTests.cs TaskMaster.Test\Ribbon\RibbonExplorerXmlTests.cs"
```

Executed through a scratchpad `.ps1` so the ten-element path list survives the shell wrapper
intact. No `EngineToggleStateCoordinatorTests.Part2.cs` exists — no split was required — so the
path list is the base ten of section 4.5.

EXIT_CODE: 0

## Output Summary

```
Formatted 10 files in 2282ms.
```

Ten files were passed to `format`; **five** were rewritten on disk, identified by
`git status --porcelain` immediately after the run (the working tree was clean of `.cs`
modifications beforehand, because P3-T9 had just committed everything):

```
 M TaskMaster/Ribbon/EngineToggleCatalog.cs
 M TaskMaster/Ribbon/EngineToggleStateCoordinator.cs
 M TaskMaster.Test/Ribbon/EngineToggleCatalogTests.cs
 M TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.cs
 M TaskMaster.Test/Ribbon/RibbonViewerEngineCallbackShapeTests.cs
```

The remaining five scope-locked files (`EngineCommandCatalog.cs`,
`RibbonController.EngineCommands.cs`, `RibbonViewer.EngineCommands.cs`,
`EngineCommandCatalogTests.cs`, `RibbonExplorerXmlTests.cs`) were already
CSharpier-clean and were left byte-identical.

The other porcelain entries at that moment were this delivery's own evidence artifacts and the
plan checklist under `docs/features/`, which the phase's loop semantics explicitly exclude from the
"intervening change" rule.

## Compliance notes

- `csharpier format` was invoked with the explicit scope-locked path list and **never repo-wide**
  (plan rule 5): a repo-wide `format .` would rewrite files that were already unformatted at the
  merge-base and break the AC-13 zero-line-diff requirements.
- `csharpier pipe-files` was **not** used (plan rule 4): it writes to stdout only and yields a
  false "stable" result.
- Because this is step 1 of the Phase 5 loop, its own rewrites do not trigger a restart; the loop
  proceeds forward to P5-T2. The five rewritten files are rebuilt and retested by P5-T4, P5-T5,
  and P5-T6 within this same pass.

Binary outcome: PASS.
