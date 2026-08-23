# Out-of-Scope Defect Promotions (AC29) — Issue #503 (P7-T28)

Timestamp: 2026-08-08T15-05

## Promotion receipts

Each defect catalogued in research section 9, plus the pre-existing test flake discovered during baseline capture, was promoted to its own GitHub issue **upstream of this plan**:

| Research section 9 defect | Issue | Local promoted entry |
|---|---|---|
| Five orphan `onAction` callbacks in `RibbonExplorer.xml` (`_Clicked` vs `_Click` suffix mismatch on `BtnMigrateIDs_Click`, `MoveEntireConversation_Clicked`, `SaveAttachments_Clicked`, `SaveEmailCopy_Clicked`, `SavePictures_Clicked`) | **#504** | `docs/features/potential/promoted/2026-08-08-ribbon-dead-callback-names.md` |
| Invalid `getPressed` callback signatures (`async Task<bool>` instead of the documented `bool`) on `SpamBayesEnabled_GetPressed` and `TriageEnabled_GetPressed` | **#505** | `docs/features/potential/promoted/2026-08-08-ribbon-async-getpressed-signature.md` |
| Fire-and-forget `ToggleEngineAsync` in `SpamBayesEnabled_Click` / `TriageEnabled_Click` (task and any exception discarded in a `void` method) | **#506** | `docs/features/potential/promoted/2026-08-08-ribbon-toggle-engine-fire-and-forget.md` |
| `RibbonController.Engines` is not null-safe on `Globals` (`RibbonController.Intelligence.cs:204`), unlike the sibling `SB`/`Triage` properties | **#507** | `docs/features/potential/promoted/2026-08-08-ribbon-controller-engines-null-unsafe.md` |
| Pre-existing order-dependent flake `UtilitiesCS.Test.OutlookObjects.Folder.WpfDispatcherYieldTests.YieldAsync_WithoutDispatcher_RemainsStrict` | **#508** | `docs/features/potential/promoted/2026-08-08-wpf-dispatcher-yield-test-order-dependent.md` |

## Explicit statements required by P7-T28

1. **These were promoted upstream of this plan.** All five promotions were performed before execution of this plan began; the five `docs/features/potential/promoted/` entries were already present in the working tree at the P0-T4 git-state capture and were committed by P0-T13.

2. **No promotion lifecycle call was made inside #503.** This plan does not invoke the promotion lifecycle. AC29 is satisfied by recording the five issue numbers above, exactly as plan section 6 specifies.

3. **No fix for any of them was made inside #503.** The section 4 scope lock permits no edit to any file implicated by #504, #505, #506, or #507, and section 3 rule 7 plus decision D7 explicitly forbid fixing #508. Verification:
   - The five orphan `onAction` attributes in `RibbonExplorer.xml` are untouched: the branch diff of that file is `23 3`, and all 23 added lines are the eight `getEnabled="EngineCommand_GetEnabled"` attributes plus the reformatting of the three `TriageSet*` buttons to one-attribute-per-line (see `<FEATURE>\evidence\qa-gates\zero-line-diff-postformat.2026-08-08T14-59.md`).
   - `SpamBayesEnabled_GetPressed` and `TriageEnabled_GetPressed` were relocated verbatim by P3-T4 and are byte-identical to their merge-base text; their signatures are unchanged (`<FEATURE>\evidence\qa-gates\office-surface-audit.2026-08-08T14-12.md` establishes that all 26 relocated members are moves, not modifications).
   - `SpamBayesEnabled_Click` and `TriageEnabled_Click` were likewise relocated verbatim; the fire-and-forget `ToggleEngineAsync` call is unchanged.
   - `RibbonController.Intelligence.cs` does not appear in the branch diff at all, so `RibbonController.Engines` is unchanged. This change deliberately does **not** use it as the readiness accessor, building `new EngineReadinessGate(() => Globals?.Engines!)` instead, precisely because of #507.
   - `UtilitiesCS.Test/` does not appear in the branch diff (`git diff --name-only <MERGE_BASE>..HEAD -- UtilitiesCS.Test/` returns 0 paths), so #508 was not touched.

## Additional defects discovered during execution (recorded for the orchestrator, not fixed, not promoted here)

Two further out-of-scope conditions were observed while executing this plan. Neither is fixed by this change, and neither is promoted by this plan; they are recorded so the orchestrator can route them:

| Condition | Evidence | Why not fixed |
|---|---|---|
| `CS2002` duplicate `<Compile Include>` for `OutlookObjects\Folder\PercentageFormatterTests.cs` in `UtilitiesCS.Test\UtilitiesCS.Test.csproj`. Present at the merge-base (`grep -c` returns 2); surfaced only when `UtilitiesCS.Test` was forced to recompile. | `<FEATURE>\evidence\other\phase2-build.2026-08-08T13-30.md`, `<FEATURE>\evidence\qa-gates\msbuild-analyzers.2026-08-08T14-35.md` | `UtilitiesCS.Test.csproj` is outside the section 4 scope lock. |
| Large pre-existing nullable debt in `TaskMaster.csproj`: 220 `CS86xx` errors under a forced `/t:Rebuild /p:Nullable=enable /p:TreatWarningsAsErrors=true`, concentrated in `AppOlObjects.cs` (58), `AppAutoFileObjects.cs` (52), `AppToDoObjects.cs` (48), `AppOlObjects.FolderTreeService.cs` (48), `AppStagingFilenames.cs` (40), `ApplicationGlobals.cs` (40), `AppItemEngines.cs` (18) and others. The plan-specified `/t:Build` gate does not surface it because MSBuild's up-to-date check skips `CoreCompile` when only `/p:` values change. | `<FEATURE>\evidence\qa-gates\msbuild-nullable.2026-08-08T14-49.md` | Every implicated file is outside the section 4 scope lock, and two of them (`AppItemEngines.cs`, `ApplicationGlobals.cs`) are AC15-protected zero-line-diff paths that must not be edited. |

A third, purely cosmetic observation: `spec.md` ends with two stray lines, `</content>` and `</invoke>`, left by the authoring tool. This is a documentation artifact with no effect on any gate; it is recorded rather than removed, since `spec.md` content outside the acceptance-criteria checkboxes is not this plan's to edit.
