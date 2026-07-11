# WI-2 — ProjectReference Removal Verification (P3-T11)

- **Timestamp:** 2026-07-11T13-20
- **Feature:** swordfish-interface-project-teardown (#308), F5

## Nine first-party ProjectReference removals (P3-T2..P3-T10)

Each of the nine first-party csprojs had its `..\UtilitiesSwordfish\UtilitiesSwordfish.NET.General.csproj`
`<ProjectReference>` block removed (4-line block: Include + Project GUID + Name + close). Per-file
match count after removal (grep `UtilitiesSwordfish.NET.General.csproj`):

| csproj | matches |
|---|---|
| UtilitiesCS/UtilitiesCS.csproj (P3-T2) | 0 |
| UtilitiesCS.Test/UtilitiesCS.Test.csproj (P3-T3) | 0 |
| TaskMaster/TaskMaster.csproj (P3-T4) | 0 |
| TaskMaster.Test/TaskMaster.Test.csproj (P3-T5) | 0 |
| QuickFiler/QuickFiler.csproj (P3-T6) | 0 |
| ToDoModel/ToDoModel.csproj (P3-T7) | 0 |
| Tags/Tags.csproj (P3-T8) | 0 |
| TaskVisualization/TaskVisualization.csproj (P3-T9) | 0 |
| TaskVisualization.Test/TaskVisualization.Test.csproj (P3-T10) | 0 |

XML integrity confirmed (spot-checked UtilitiesCS.csproj: the preceding SVGControl ProjectReference
and the closing `</ItemGroup>` are intact; no orphaned `<Project>`/`<Name>` lines).

## Repo-wide verification

- **Command:** `git grep -n "UtilitiesSwordfish.NET.General.csproj" -- "*.csproj"`
- **EXIT_CODE:** 0 (one match — the vendored project's own internal reference)
- **Output Summary:** All NINE first-party references are removed (AC-7 scope). The sole remaining
  match is `UtilitiesSwordfish.Test/UtilitiesSwordfish.NET.Test.csproj:132`, the vendored test
  project's internal reference to the vendored General project. That csproj is NOT one of the nine
  first-party references; it is deleted wholesale with the `UtilitiesSwordfish.Test/` folder in Phase 4
  (P4-T4). True repo-wide zero over `*.csproj` is achieved after Phase 4 and re-verified at P5-T6.

## Verdict

WI-2 complete. All nine first-party `UtilitiesSwordfish.NET.General.csproj` ProjectReferences removed.
Delivers AC-7 (with the P3-T1 stale-reference evidence for Tags / TaskVisualization / TaskVisualization.Test).
