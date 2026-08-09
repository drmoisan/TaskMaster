# P5-T3 — Authoritative Post-Format 500-Line Audit (AC-21)

Timestamp: 2026-08-08T21-34

Command:

```
pwsh -NoProfile -Command "Set-Location '<REPO>'; git diff --name-only f910ff2f21c67a03cf8eebcb340727d5415d8e08..HEAD | ForEach-Object { if (Test-Path $_) { '{0}={1}' -f $_, (Get-Content $_ | Measure-Object -Line).Lines } }"
```

EXIT_CODE: 0

Two measurements are reported per path. `Measure-Object -Line` (the plan's stated command) skips
empty strings and therefore counts **non-blank** lines; `@(Get-Content $_).Count` counts
**physical** lines. The 500-line cap is evaluated against the **physical** count, which is the
stricter and correct measure — see the P0-T11 measurement note.

The five files CSharpier rewrote at P5-T1 are not yet committed, so they are additionally measured
from the working tree; the counts below are the post-format working-tree counts in all cases.

## Output Summary

### Source and project files in the branch diff

| Path | `Measure-Object -Line` | **Physical** | Cap status |
|---|---|---|---|
| `TaskMaster/Ribbon/EngineToggleCatalog.cs` | 87 | **92** | OK |
| `TaskMaster/Ribbon/EngineToggleStateCoordinator.cs` | 360 | **389** | OK |
| `TaskMaster/Ribbon/EngineCommandCatalog.cs` | 96 | **101** | OK |
| `TaskMaster/Ribbon/RibbonController.EngineCommands.cs` | 155 | **164** | OK |
| `TaskMaster/Ribbon/RibbonViewer.EngineCommands.cs` | 286 | **328** | OK |
| `TaskMaster.Test/Ribbon/RibbonViewerEngineCallbackShapeTests.cs` | 334 | **365** | OK |
| `TaskMaster.Test/Ribbon/EngineToggleCatalogTests.cs` | 88 | **101** | OK |
| `TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.cs` | 392 | **459** | OK (largest `.cs`) |
| `TaskMaster.Test/Ribbon/EngineCommandCatalogTests.cs` | 115 | **128** | OK |
| `TaskMaster.Test/Ribbon/RibbonExplorerXmlTests.cs` | 293 | **323** | OK |
| `TaskMaster/Ribbon/RibbonExplorer.xml` | 545 | **545** | accepted pre-existing overage — see below |
| `TaskMaster/TaskMaster.csproj` | 581 | **584** | accepted pre-existing overage — see below |
| `TaskMaster.Test/TaskMaster.Test.csproj` | 384 | **384** | OK |

**Every `.cs` path in the diff is at or under 500 physical lines.** The largest is
`EngineToggleStateCoordinatorTests.cs` at 459, which is why no `Part2` split was needed
(section 4.3's conditional split did not trigger).

### Accepted pre-existing overages (non-`.cs`)

| Path | Merge-base | Post-change | Delta | Disposition |
|---|---|---|---|---|
| `TaskMaster/Ribbon/RibbonExplorer.xml` | 539 | 545 | +6 | **Accepted pre-existing overage.** AC-21 grants this file an explicit carve-out: it is a declarative embedded Office CustomUI resource, not production or test code. This change adds only the six `getEnabled` attributes, one per line, to existing `<button>` elements. Not remediated here. |
| `TaskMaster/TaskMaster.csproj` | 582 | 584 | +2 | **Accepted pre-existing overage.** A legacy non-SDK MSBuild project file, not production, test, or reusable-script code, so the `.claude/rules/general-code-change.md` cap does not classify it. It was already 82 lines over at the merge-base; this change adds only the two mandatory `<Compile Include>` entries of section 4.6. Splitting a VSTO `.csproj` is out of scope for this bug fix. |

Neither is a violation introduced by this change; both are recorded, not silently passed over.

### Markdown — exempt

Every path under `docs/features/` and `.claude/agent-memory/` is Markdown documentation or
evidence and is **EXEMPT** from the cap per `.claude/rules/general-code-change.md`. Counts are
recorded for completeness and are never treated as violations. The largest are
`spec.md` (712 physical), `research/2026-08-08T19-30-...-research.md` (530), and
`plan.2026-08-08T19-22.md` (387).

Binary outcome: **PASS** — no `.cs` file exceeds 500 lines, so no split is required and the phase
does not restart at P5-T1.

## Pass note

This is the second, uninterrupted Phase 5 pass. P5-T1 of this pass rewrote **0 of 10** files
(SHA-256 verified), so the tree measured here is byte-identical to the tree measured in the
aborted attempt 1 at `2026-08-08T21-21`; the counts above were re-measured and are unchanged.
