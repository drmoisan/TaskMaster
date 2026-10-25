# Scope Fence (P6-T3)

Timestamp: 2026-10-22T16-29
Command: git diff --name-status <merge-base>; git status --porcelain --untracked-files=all
EXIT_CODE: 0
Output Summary: PASS. Every listed path is a spec Write Set file (including TaskMaster.Test/AppGlobals/SettingsSaveProbe.cs, Decision D-A), a path under the feature folder, or a pre-existing documentation path already listed in merge-base-scope.2026-10-22T16-06.md (.claude/agent-memory/** and docs/features/potential/promoted/**). ToDoModel.Test/ToDoModel.Test.csproj and TaskMaster.Test/Ribbon/RibbonControllerTests.Engines.cs are absent. No other .claude/ path and no file under UtilitiesCS/Extensions/ appears.

MERGE-BASE: recorded in `evidence/baseline/merge-base-scope.2026-10-22T16-06.md`.

## Code and project paths (tracked changes plus untracked)

| Path | Status | Classification |
| --- | --- | --- |
| `UtilitiesCS/HelperClasses/Settings/ISettingsStore.cs` | untracked (new) | Write Set |
| `UtilitiesCS/HelperClasses/Settings/ApplicationSettingsStore.cs` | untracked (new) | Write Set |
| `ToDoModel/Data Model/ID/IDList.cs` | M | Write Set |
| `TaskMaster/AppGlobals/AppQuickFilerSettings.cs` | M | Write Set |
| `TaskMaster/AppGlobals/AppStagingFilenames.cs` | M | Write Set |
| `TaskMaster/AppGlobals/AppAutoFileObjects.cs` | M | Write Set |
| `TaskMaster/AppGlobals/AppAutoFileObjects.ScalarSettings.cs` | untracked (new) | Write Set |
| `UtilitiesCS/UtilitiesCS.csproj` | M | Write Set |
| `TaskMaster/TaskMaster.csproj` | M | Write Set |
| `UtilitiesCS.Test/Extensions/DfDeedle_COM_Tests.cs` | M | Write Set |
| `UtilitiesCS.Test/Extensions/DfDeedle_COM_Tests.QfcColumnsAsync.cs` | untracked (new) | Write Set |
| `UtilitiesCS.Test/HelperClasses/Settings/ApplicationSettingsStoreTests.cs` | untracked (new) | Write Set |
| `UtilitiesCS.Test/UtilitiesCS.Test.csproj` | M | Write Set |
| `ToDoModel.Test/Data Model/ID/IDListGetNextToDoIDTests.cs` | M | Write Set |
| `TaskTree.Test/TaskTreeControllerMoveLogicTests.cs` | M | Write Set |
| `TaskTree.Test/TaskTreeControllerRouteDropTests.cs` | M | Write Set |
| `TaskMaster.Test/AppGlobals/InMemorySettingsStore.cs` | untracked (new) | Write Set |
| `TaskMaster.Test/AppGlobals/SettingsSaveProbe.cs` | untracked (new) | Write Set (Decision D-A) |
| `TaskMaster.Test/AppGlobals/AppQuickFilerSettingsTests.cs` | M | Write Set |
| `TaskMaster.Test/AppGlobals/AppQuickFilerSettingsRemainingPropertiesTests.cs` | M | Write Set |
| `TaskMaster.Test/AppGlobals/AppStagingFilenamesTests.cs` | M | Write Set |
| `TaskMaster.Test/AppGlobals/AppAutoFileObjectsCoverageExpansionTests.cs` | M | Write Set |
| `TaskMaster.Test/Ribbon/RibbonControllerTests.cs` | M | Write Set |
| `TaskMaster.Test/TaskMaster.Test.csproj` | M | Write Set |

## Documentation paths

| Path set | Classification |
| --- | --- |
| `docs/features/active/2026-10-22-parallel-test-failures-wall-clock-and-shared-user-config-1005/**` (issue, spec, user story, research, plan, preflight records, and this run's evidence) | under the feature folder |
| `.claude/agent-memory/atomic-planner/MEMORY.md`, `.claude/agent-memory/atomic-planner/project_1005_r0_settings_store_seam_and_probe_seams.md`, `.claude/agent-memory/atomic-planner/project_1005_r1_jacoco_projection_is_package_level_only.md`, `.claude/agent-memory/prd-feature/feedback_ac_gates_verify_satisfiability.md`, `.claude/agent-memory/task-researcher/MEMORY.md`, `.claude/agent-memory/task-researcher/project_settings_save_shared_user_config_1005.md` | pre-existing in `merge-base-scope.2026-10-22T16-06.md` (agent memory) |
| `docs/features/potential/promoted/2026-10-22-parallel-test-failures-wall-clock-and-shared-user-config.md`, `docs/features/potential/promoted/2026-10-22-tests-mutate-shared-settings-default-in-memory.md` | pre-existing in `merge-base-scope.2026-10-22T16-06.md` (promotion records) |

## Required absences

| Path | Present |
| --- | --- |
| `ToDoModel.Test/ToDoModel.Test.csproj` | no |
| `TaskMaster.Test/Ribbon/RibbonControllerTests.Engines.cs` | no |
| any `.claude/` path other than `.claude/agent-memory/**` | no |
| any file under `UtilitiesCS/Extensions/` | no |
