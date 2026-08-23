# AC25 Post-Format 500-Line Audit — Issue #503 (P6-T3)

Timestamp: 2026-08-08T14-47

This is the **authoritative** file-size audit, measured after the P6-T1 CSharpier format pass. It supersedes the advisory P0-T11 counts.

Command:
```
pwsh -NoProfile -Command "Set-Location 'C:\Users\DanMoisan\repos\TaskMaster-wt\2026-08-08T11-55'; git diff --name-only 003c5715055d7d1933db68a742531332756e30b2..HEAD | ForEach-Object { if (Test-Path $_) { '{0}={1}' -f $_, (Get-Content $_ | Measure-Object -Line).Lines } }"
```

Corroborating command (physical line counts, the stricter measure — `Measure-Object -Line` does not count blank lines, as recorded in the P0-T11 measurement-method note):
```
git diff --name-only 003c5715055d7d1933db68a742531332756e30b2..HEAD | while read f; do [ -f "$f" ] && wc -l < "$f"; done
```

EXIT_CODE: 0

## Output Summary — source files in the branch diff

| Path | `Measure-Object -Line` | Physical lines (`wc -l`) | <= 500? |
|---|---|---|---|
| `TaskMaster/Ribbon/EngineCommandCatalog.cs` | 83 | **88** | Yes |
| `TaskMaster/Ribbon/EngineCommandRefreshPlanner.cs` | 56 | **58** | Yes |
| `TaskMaster/Ribbon/EngineGatedCommandRunner.cs` | 129 | **139** | Yes |
| `TaskMaster/Ribbon/EngineReadinessGate.cs` | 95 | **103** | Yes |
| `TaskMaster/Ribbon/RibbonController.EngineCommands.cs` | 94 | **100** | Yes |
| `TaskMaster/Ribbon/RibbonViewer.EngineCommands.cs` | 168 | **207** | Yes |
| `TaskMaster/Ribbon/RibbonViewer.cs` | 299 | **388** | Yes |
| `TaskMaster/ThisAddIn.cs` | 271 | **307** | Yes |
| `TaskMaster.Test/Ribbon/EngineCommandCatalogTests.cs` | 103 | **116** | Yes |
| `TaskMaster.Test/Ribbon/EngineCommandRefreshPlannerTests.cs` | 47 | **52** | Yes |
| `TaskMaster.Test/Ribbon/EngineGatedCommandRunnerTests.cs` | 306 | **346** | Yes |
| `TaskMaster.Test/Ribbon/EngineReadinessGateTests.cs` | 189 | **223** | Yes |
| `TaskMaster.Test/Ribbon/RibbonExplorerXmlTests.cs` | 279 | **309** | Yes |
| `TaskMaster/Ribbon/RibbonExplorer.xml` | 539 | **539** | No — accepted pre-existing exception, see below |
| `TaskMaster/TaskMaster.csproj` | 579 | **581** | No — accepted pre-existing exception, see below |
| `TaskMaster.Test/TaskMaster.Test.csproj` | 380 | **380** | Yes |

**Every `.cs` path is at or under 500 lines by both measures.** The largest is `TaskMaster.Test/Ribbon/EngineGatedCommandRunnerTests.cs` at 346 physical lines, a 154-line margin under the cap.

### `TaskMaster/Ribbon/RibbonViewer.cs` is below its merge-base count

Merge-base: **487** physical lines. Post-change: **388** physical lines, a reduction of 99. This is the P3-T1 / P3-T4 partial-class split (one line added for `partial`, 100 lines removed by relocating the `#region Spam Manager` and `#region Triage` blocks), which is exactly what AC25 requires and what created the headroom for the new callbacks.

### Accepted pre-existing exceptions (not violations)

| Path | Merge-base | Post-change | Rationale |
|---|---|---|---|
| `TaskMaster/Ribbon/RibbonExplorer.xml` | 519 | 539 | Declarative embedded UI resource, not production/test/script code. The overage **predates #503** (519 > 500 at the merge-base). The plan section 4.2 and spec Correction Log entry 5 record this as an accepted pre-existing exception rather than something remediated here; splitting the ribbon into multiple embedded resources is a separate, larger change to `RibbonViewer.GetCustomUI`. |
| `TaskMaster/TaskMaster.csproj` | 575 | 581 | Declarative MSBuild project file, not production/test/script code. The overage **predates #503** (575 > 500 at the merge-base). The change adds exactly the six mandated `<Compile Include>` entries; a legacy non-SDK `packages.config` project cannot use globs, so the entries are unavoidable. |

Both are declarative build/UI descriptors rather than code files, and both were already over 500 lines at the merge-base, so neither is an overage introduced by this change.

### Documentation and evidence paths in the diff

Every remaining path in the diff lies under `docs/features/` or `.claude/agent-memory/`. These are documentation and evidence, are expected diff entries, and are **not** source-scope violations. Markdown documentation files are explicitly exempt from the 500-line cap per the file-size exceptions in `.claude/rules/general-code-change.md`. Their counts are recorded for the audit trail:

- Largest Markdown: `docs/features/active/2026-08-08-ribbon-engine-readiness-guard-503/spec.md` at 509 physical lines — exempt as Markdown documentation, recorded not treated as a violation.
- `docs/features/active/2026-08-08-ribbon-engine-readiness-guard-503/research/2026-08-08T12-45-ribbon-engine-readiness-guard-research.md` at 446, `plan.2026-08-08T11-59.md` at 378 — both under 500 in any case.
- `docs/features/active/2026-08-08-ribbon-engine-readiness-guard-503/evidence/baseline/coverage-baseline.cobertura.xml` at 187,114 lines — a tool-generated Cobertura coverage evidence artifact, not production/test/script code, and required by AC24 as the baseline half of the coverage comparison.
- All `.claude/agent-memory/` files are Markdown, the largest at 86 lines.

## Scope statement

Every path that appears in the audit is either a member of the plan's section 4 scope lock, or lies under `docs/features/` or `.claude/agent-memory/`. No `.cs`, `.csproj`, `.xml`, or `.sln` path outside the section 4 scope lock appears.

Binary outcome: **PASS** — no `.cs` file exceeds 500 lines, so no restart at P6-T1 is triggered.
