# 2026-03-14-outlook-objects-test-coverage-67 — Executor-Ready Atomic Plan

- **Status:** Planned
- **Issue:** #67
- **Owner:** Dan Moisan
- **Target Plan File:** `C:\Users\DanMoisan\repos\TaskMaster-2026-03-14T11-01\docs\features\active\2026-03-14-outlook-objects-test-coverage-67\plan.2026-03-14T12-13.md`

## Overview

Complete the remaining `UtilitiesCS\OutlookObjects` coverage work by finishing the currently uncovered hotspot files, adding only narrow testability seams inside the named hotspot production files, and proving compliance with the full C# QA loop plus per-file coverage evidence. The plan is scoped to the active feature folder inputs and is fail-closed to `full-feature` mode because `issue.md` does not contain a valid `- Work Mode:` marker.

## Resolved Inputs

- **Name:** `2026-03-14-outlook-objects-test-coverage-67`
- **File:** `C:\Users\DanMoisan\repos\TaskMaster-2026-03-14T11-01\docs\features\active\2026-03-14-outlook-objects-test-coverage-67\plan.2026-03-14T12-13.md`
- **Spec:** `C:\Users\DanMoisan\repos\TaskMaster-2026-03-14T11-01\docs\features\active\2026-03-14-outlook-objects-test-coverage-67\spec.md`
- **User Story:** `C:\Users\DanMoisan\repos\TaskMaster-2026-03-14T11-01\docs\features\active\2026-03-14-outlook-objects-test-coverage-67\user-story.md`
- **Research:** `C:\Users\DanMoisan\repos\TaskMaster-2026-03-14T11-01\docs\features\active\2026-03-14-outlook-objects-test-coverage-67\research.md`
- **Work Mode:** `full-feature`
- **Fallback Reason:** `issue.md does not contain a valid '- Work Mode:' marker, so mode resolution fails closed to 'full-feature'.`

## Proposed Class and Module Structure

- Keep production logic in its existing folders under `UtilitiesCS\OutlookObjects\`.
- Keep all new tests in mirrored folders under `UtilitiesCS.Test\OutlookObjects\`.
- Do not introduce a repo-wide Outlook abstraction layer.
- Keep each new seam co-located with the hotspot production file that requires it.
- Keep each new mirrored test file focused on one hotspot slice or one seam-enabled behavior family.

### Production files expected to change

- `UtilitiesCS\OutlookObjects\MailItem\MailItemHelper.cs`
- `UtilitiesCS\OutlookObjects\Fields\UserDefinedFields.cs`
- `UtilitiesCS\OutlookObjects\Conversation\ConversationHelper.cs`
- `UtilitiesCS\OutlookObjects\Table\OlTableExtensions.cs`
- `UtilitiesCS\OutlookObjects\Store\StoreWrapperController.cs`
- `UtilitiesCS\OutlookObjects\Folder\FolderWrapper .cs`
- `UtilitiesCS\OutlookObjects\Folder\FolderPredictor.cs`

### Test files expected to change

- `UtilitiesCS.Test\OutlookObjects\MailItem\MailItemHelperCoreTests.cs`
- `UtilitiesCS.Test\OutlookObjects\MailItem\MailItemHelperProjectionTests.cs`
- `UtilitiesCS.Test\OutlookObjects\Fields\UserDefinedFieldsTests.cs`
- `UtilitiesCS.Test\OutlookObjects\Fields\MAPIFieldsTests.cs`
- `UtilitiesCS.Test\OutlookObjects\Conversation\ConversationHelperTests.cs`
- `UtilitiesCS.Test\OutlookObjects\Table\OlToDoTableTests.cs`
- `UtilitiesCS.Test\OutlookObjects\Table\OlTableExtensionsTransformTests.cs`
- `UtilitiesCS.Test\OutlookObjects\Table\OlTableExtensionsRetryTests.cs`
- `UtilitiesCS.Test\OutlookObjects\Table\OlTableExtensionsConversionTests.cs`
- `UtilitiesCS.Test\OutlookObjects\Store\StoreWrapperControllerTests.cs`
- `UtilitiesCS.Test\OutlookObjects\Folder\FolderWrapperStateTests.cs`
- `UtilitiesCS.Test\OutlookObjects\Folder\FolderWrapperTraversalTests.cs`
- `UtilitiesCS.Test\OutlookObjects\Folder\FolderTreeTests.cs`
- `UtilitiesCS.Test\OutlookObjects\Folder\FolderPredictorTests.cs`
- `UtilitiesCS.Test\UtilitiesCS.Test.csproj`

## Minimal DI Seams

- `UtilitiesCS\OutlookObjects\MailItem\MailItemHelper.cs`
  - Use existing `virtual` members first.
  - If one extra seam is still required, add exactly one `internal virtual` helper for Outlook-member projection.
- `UtilitiesCS\OutlookObjects\Fields\UserDefinedFields.cs`
  - Add exactly one narrow `internal` accessor seam for property-accessor lookup or value retrieval.
- `UtilitiesCS\OutlookObjects\Conversation\ConversationHelper.cs`
  - Add exactly one narrow `internal` resolver seam for `NameSpace.GetItemFromID`-style item rehydration.
- `UtilitiesCS\OutlookObjects\Table\OlTableExtensions.cs`
  - Add exactly one narrow `internal` seam for retry or timing execution over table rows.
- `UtilitiesCS\OutlookObjects\Store\StoreWrapperController.cs`
  - Add exactly one narrow `internal` dialog-selection seam for picker or UI result acquisition.
- `UtilitiesCS\OutlookObjects\Folder\FolderWrapper .cs`
  - Add exactly one narrow `internal` enumeration or release seam around child traversal and RCW-adjacent behavior.
- `UtilitiesCS\OutlookObjects\Folder\FolderPredictor.cs`
  - Add exactly one narrow `internal` seam for dialog and directory-creation side effects.

## MSTest Scenario-Level Strategy

- Use MSTest with one test method per scenario.
- Use Arrange–Act–Assert structure in every new or modified test.
- Enumerate scenarios across these behavior families only:
  - positive path,
  - null or missing input,
  - boundary or empty-data path,
  - controlled error path.
- Keep every test deterministic and independent.
- Keep every new or modified test file at or below the 500-line repo limit.

## Moq Mock Strategy

- Use `Moq` with `MockBehavior.Strict` when interaction order or call count matters.
- Use `SetupGet`, `SetupProperty`, `SetupSequence`, and explicit return values for Outlook COM proxies.
- Use `DefaultValue.Mock` only when a nested Outlook object graph would otherwise dominate arrange code.
- Mock only interfaces or virtual members.
- For non-virtual or RCW-lifetime-adjacent behavior, prefer test subclasses plus the planned narrow seams instead of deep COM mocking.
- Use `FluentAssertions` for new assertions unless MSTest `Assert` is required for a specific assertion shape.

## QA Commands

- `dotnet restore TaskMaster.sln`
- `msbuild TaskMaster.sln /t:Restore /p:RestorePackagesConfig=true /p:Configuration=Debug /p:Platform="Any CPU"`
- `dotnet format TaskMaster.sln --verify-no-changes --no-restore`
- `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
- `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true`
- `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug`

### Phase 0 — Context & Baseline Capture

- [x] [P0-T1] Read `.github/copilot-instructions.md`, `.github/instructions/general-code-change.instructions.md`, `.github/instructions/csharp-code-change.instructions.md`, `.github/instructions/general-unit-test.instructions.md`, and `.github/instructions/csharp-unit-test.instructions.md`
  - Acceptance:
    - `evidence/baseline/phase0-instructions-read.md` exists.
    - The artifact contains `Timestamp:`, `Policy Order:`, and the exact five file paths.

- [x] [P0-T2] Read `change-plan.md`, `issue.md`, `spec.md`, `user-story.md`, `research.md`, and `.github/prompts/generate-atomic-plan.prompt.md`
  - Acceptance:
    - `evidence/baseline/phase0-feature-inputs.md` exists.
    - The artifact contains `Timestamp:` and an explicit `Files Read:` list naming all six inputs.

- [x] [P0-T3] Record mode resolution and fallback reason for this plan
  - Acceptance:
    - `evidence/baseline/phase0-mode-resolution.md` exists.
    - The artifact contains `Resolved Work Mode: full-feature` and the exact fallback reason from the resolved-inputs section.

- [x] [P0-T4] Capture the compiled OutlookObjects production inventory from `UtilitiesCS\UtilitiesCS.csproj`
  - Acceptance:
    - `evidence/baseline/phase0-production-inventory.md` exists.
    - The artifact lists every compiled `OutlookObjects\*.cs` entry from `UtilitiesCS\UtilitiesCS.csproj`.
    - The artifact explicitly states that `UtilitiesCS\OutlookObjects\MailItem\MailResolution.cs` is compiled and `UtilitiesCS\OutlookObjects\MailResolution.cs` is not compiled.

- [x] [P0-T5] Capture the compiled OutlookObjects test inventory from `UtilitiesCS.Test\UtilitiesCS.Test.csproj`
  - Acceptance:
    - `evidence/baseline/phase0-test-inventory.md` exists.
    - The artifact lists every `OutlookObjects\*.cs` compile include currently present in `UtilitiesCS.Test\UtilitiesCS.Test.csproj`.

- [x] [P0-T6] Run `dotnet restore TaskMaster.sln`
  - Acceptance:
    - At least one file matching `evidence/baseline/baseline-dotnet-restore.*.md` exists.
    - The newest matching artifact contains `Timestamp:`, `Command: dotnet restore TaskMaster.sln`, `EXIT_CODE:`, and `Output Summary:`.

- [x] [P0-T7] Run `msbuild TaskMaster.sln /t:Restore /p:RestorePackagesConfig=true /p:Configuration=Debug /p:Platform="Any CPU"`
  - Acceptance:
    - At least one file matching `evidence/baseline/baseline-packagesconfig-restore.*.md` exists.
    - The newest matching artifact contains `Timestamp:`, the exact command, `EXIT_CODE:`, and `Output Summary:`.

- [x] [P0-T8] Run `dotnet format TaskMaster.sln --verify-no-changes --no-restore`
  - Acceptance:
    - At least one file matching `evidence/baseline/baseline-format.*.md` exists.
    - The newest matching artifact contains `Timestamp:`, the exact command, `EXIT_CODE:`, and `Output Summary:`.

- [x] [P0-T9] Run `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
  - Acceptance:
    - At least one file matching `evidence/baseline/baseline-analyzers.*.md` exists.
    - The newest matching artifact contains `Timestamp:`, the exact command, `EXIT_CODE:`, and `Output Summary:`.

- [x] [P0-T10] Run `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true`
  - Acceptance:
    - At least one file matching `evidence/baseline/baseline-nullable.*.md` exists.
    - The newest matching artifact contains `Timestamp:`, the exact command, `EXIT_CODE:`, and `Output Summary:`.

- [x] [P0-T11] Run `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug`
  - Acceptance:
    - At least one file matching `evidence/baseline/baseline-test-coverage.*.md` exists.
    - The newest matching artifact contains `Timestamp:`, the exact command, `EXIT_CODE:`, and `Output Summary:` with numeric pass count, numeric fail count, numeric skip count, numeric repository-wide coverage headline, numeric OutlookObjects coverage headline, and numeric new-or-changed-code coverage headline.

- [x] [P0-T12] Record the exact changed-code coverage scope method for this feature
  - Acceptance:
    - At least one file matching `evidence/baseline/baseline-changed-code-scope.*.md` exists.
    - The newest matching artifact contains `Timestamp:`, `Diff Base: merge-base between the feature branch and main`, `Changed File Source: final PR unified diff`, `Changed Line Method: unified diff hunk line numbers`, and `Coverage Mapping Method: changed lines intersected with Cobertura line data`.
    - The newest matching artifact names the feature-target production and test files that are expected to participate in the changed-code coverage calculation.

- [x] [P0-T13] Record per-file OutlookObjects baseline coverage from `coverage\coverage.cobertura.xml`
  - Acceptance:
    - At least one file matching `evidence/baseline/baseline-outlookobjects-per-file-coverage.*.md` exists.
    - The newest matching artifact includes one row per compiled OutlookObjects source file with exact relative path and numeric line-rate percentage.

- [x] [P0-T14] Build the OutlookObjects target matrix
  - Acceptance:
    - At least one file matching `evidence/baseline/baseline-outlookobjects-target-matrix.*.md` exists.
    - The newest matching artifact classifies every compiled OutlookObjects file as exactly one of `coverage-target` or `excluded-non-executable`.
    - The newest matching artifact explicitly excludes:
      - `UtilitiesCS\OutlookObjects\Item\ItemComparer.cs`
      - `UtilitiesCS\OutlookObjects\MailResolution.cs`
      - `UtilitiesCS\OutlookObjects\Store\IStoreWrapperViewer.cs`
      - `UtilitiesCS\OutlookObjects\Store\StoreWrapperViewer.Designer.cs`
      - `UtilitiesCS\OutlookObjects\Folder\MsgToMime\MAPIMethods.cs`

### Phase 1 — MailItem and Fields Completion

- [x] [P1-T1] Add test method `CompressPlainText_collapses_runs_of_whitespace` to `UtilitiesCS.Test\OutlookObjects\MailItem\MailItemHelperCoreTests.cs`
  - Acceptance:
    - `UtilitiesCS.Test\OutlookObjects\MailItem\MailItemHelperCoreTests.cs` contains `CompressPlainText_collapses_runs_of_whitespace`.
    - The test `UtilitiesCS.Test.OutlookObjects.MailItem.MailItemHelperCoreTests.CompressPlainText_collapses_runs_of_whitespace` passes.

- [x] [P1-T2] Add test method `CompressPlainText_returns_safe_value_for_null_or_empty_input` to `UtilitiesCS.Test\OutlookObjects\MailItem\MailItemHelperCoreTests.cs`
  - Acceptance:
    - `UtilitiesCS.Test\OutlookObjects\MailItem\MailItemHelperCoreTests.cs` contains `CompressPlainText_returns_safe_value_for_null_or_empty_input`.
    - The test `UtilitiesCS.Test.OutlookObjects.MailItem.MailItemHelperCoreTests.CompressPlainText_returns_safe_value_for_null_or_empty_input` passes.

- [x] [P1-T3] Create `UtilitiesCS.Test\OutlookObjects\MailItem\MailItemHelperProjectionTests.cs`
  - Acceptance:
    - The file exists at the exact path.
    - `UtilitiesCS.Test\UtilitiesCS.Test.csproj` contains `<Compile Include="OutlookObjects\MailItem\MailItemHelperProjectionTests.cs" />`.

- [x] [P1-T4] Add one projection seam to `UtilitiesCS\OutlookObjects\MailItem\MailItemHelper.cs`
  - Acceptance:
    - `UtilitiesCS\OutlookObjects\MailItem\MailItemHelper.cs` is the only production file changed by this task.
    - The added seam is `internal` or `internal virtual`.
    - `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU"` exits with code `0`.

- [x] [P1-T5] Add test method `Projection_returns_subject_and_entry_id_from_mock_mail_item` to `UtilitiesCS.Test\OutlookObjects\MailItem\MailItemHelperProjectionTests.cs`
  - Acceptance:
    - `UtilitiesCS.Test\OutlookObjects\MailItem\MailItemHelperProjectionTests.cs` contains `Projection_returns_subject_and_entry_id_from_mock_mail_item`.
    - The test `UtilitiesCS.Test.OutlookObjects.MailItem.MailItemHelperProjectionTests.Projection_returns_subject_and_entry_id_from_mock_mail_item` passes.

- [x] [P1-T6] Add test method `Projection_returns_safe_defaults_when_member_lookup_fails` to `UtilitiesCS.Test\OutlookObjects\MailItem\MailItemHelperProjectionTests.cs`
  - Acceptance:
    - `UtilitiesCS.Test\OutlookObjects\MailItem\MailItemHelperProjectionTests.cs` contains `Projection_returns_safe_defaults_when_member_lookup_fails`.
    - The test `UtilitiesCS.Test.OutlookObjects.MailItem.MailItemHelperProjectionTests.Projection_returns_safe_defaults_when_member_lookup_fails` passes.

- [x] [P1-T7] Create `UtilitiesCS.Test\OutlookObjects\Fields\UserDefinedFieldsTests.cs`
  - Acceptance:
    - The file exists at the exact path.
    - `UtilitiesCS.Test\UtilitiesCS.Test.csproj` contains `<Compile Include="OutlookObjects\Fields\UserDefinedFieldsTests.cs" />`.

- [x] [P1-T8] Add one property-accessor seam to `UtilitiesCS\OutlookObjects\Fields\UserDefinedFields.cs`
  - Acceptance:
    - `UtilitiesCS\OutlookObjects\Fields\UserDefinedFields.cs` is the only production file changed by this task.
    - The seam isolates property-accessor lookup or value access only.
    - `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU"` exits with code `0`.

- [x] [P1-T9] Add test method `ValidPropertyArgs_rejects_missing_required_inputs` to `UtilitiesCS.Test\OutlookObjects\Fields\UserDefinedFieldsTests.cs`
  - Acceptance:
    - `UtilitiesCS.Test\OutlookObjects\Fields\UserDefinedFieldsTests.cs` contains `ValidPropertyArgs_rejects_missing_required_inputs`.
    - The test `UtilitiesCS.Test.OutlookObjects.Fields.UserDefinedFieldsTests.ValidPropertyArgs_rejects_missing_required_inputs` passes.

- [x] [P1-T10] Add test method `GetUdfValue_returns_expected_lookup_value_for_known_field` to `UtilitiesCS.Test\OutlookObjects\Fields\UserDefinedFieldsTests.cs`
  - Acceptance:
    - `UtilitiesCS.Test\OutlookObjects\Fields\UserDefinedFieldsTests.cs` contains `GetUdfValue_returns_expected_lookup_value_for_known_field`.
    - The test `UtilitiesCS.Test.OutlookObjects.Fields.UserDefinedFieldsTests.GetUdfValue_returns_expected_lookup_value_for_known_field` passes.

- [x] [P1-T11] Create `UtilitiesCS.Test\OutlookObjects\Fields\MAPIFieldsTests.cs`
  - Acceptance:
    - The file exists at the exact path.
    - `UtilitiesCS.Test\UtilitiesCS.Test.csproj` contains `<Compile Include="OutlookObjects\Fields\MAPIFieldsTests.cs" />`.

- [x] [P1-T12] Add test method `Known_property_tag_returns_expected_constant` to `UtilitiesCS.Test\OutlookObjects\Fields\MAPIFieldsTests.cs`
  - Acceptance:
    - `UtilitiesCS.Test\OutlookObjects\Fields\MAPIFieldsTests.cs` contains `Known_property_tag_returns_expected_constant`.
    - The test `UtilitiesCS.Test.OutlookObjects.Fields.MAPIFieldsTests.Known_property_tag_returns_expected_constant` passes.

- [x] [P1-T13] Add test method `Unknown_property_tag_returns_safe_result` to `UtilitiesCS.Test\OutlookObjects\Fields\MAPIFieldsTests.cs`
  - Acceptance:
    - `UtilitiesCS.Test\OutlookObjects\Fields\MAPIFieldsTests.cs` contains `Unknown_property_tag_returns_safe_result`.
    - The test `UtilitiesCS.Test.OutlookObjects.Fields.MAPIFieldsTests.Unknown_property_tag_returns_safe_result` passes.

### Phase 2 — Conversation and Table Completion

- [x] [P2-T1] Create `UtilitiesCS.Test\OutlookObjects\Conversation\ConversationHelperTests.cs`
  - Acceptance:
    - The file exists at the exact path.
    - `UtilitiesCS.Test\UtilitiesCS.Test.csproj` contains `<Compile Include="OutlookObjects\Conversation\ConversationHelperTests.cs" />`.

- [x] [P2-T2] Add one namespace or item-resolution seam to `UtilitiesCS\OutlookObjects\Conversation\ConversationHelper.cs`
  - Acceptance:
    - `UtilitiesCS\OutlookObjects\Conversation\ConversationHelper.cs` is the only production file changed by this task.
    - The seam isolates `GetItemFromID`-style resolution only.
    - `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU"` exits with code `0`.

- [x] [P2-T3] Add test method `Transform_returns_expected_row_shape_for_resolved_conversation_items` to `UtilitiesCS.Test\OutlookObjects\Conversation\ConversationHelperTests.cs`
  - Acceptance:
    - `UtilitiesCS.Test\OutlookObjects\Conversation\ConversationHelperTests.cs` contains `Transform_returns_expected_row_shape_for_resolved_conversation_items`.
    - The test `UtilitiesCS.Test.OutlookObjects.Conversation.ConversationHelperTests.Transform_returns_expected_row_shape_for_resolved_conversation_items` passes.

- [x] [P2-T4] Add test method `Resolver_failure_returns_controlled_result_without_live_outlook` to `UtilitiesCS.Test\OutlookObjects\Conversation\ConversationHelperTests.cs`
  - Acceptance:
    - `UtilitiesCS.Test\OutlookObjects\Conversation\ConversationHelperTests.cs` contains `Resolver_failure_returns_controlled_result_without_live_outlook`.
    - The test `UtilitiesCS.Test.OutlookObjects.Conversation.ConversationHelperTests.Resolver_failure_returns_controlled_result_without_live_outlook` passes.

- [x] [P2-T5] Create `UtilitiesCS.Test\OutlookObjects\Table\OlToDoTableTests.cs`
  - Acceptance:
    - The file exists at the exact path.
    - `UtilitiesCS.Test\UtilitiesCS.Test.csproj` contains `<Compile Include="OutlookObjects\Table\OlToDoTableTests.cs" />`.

- [x] [P2-T6] Add test method `GetToDoTable_returns_null_or_safe_result_when_folder_is_missing` to `UtilitiesCS.Test\OutlookObjects\Table\OlToDoTableTests.cs`
  - Acceptance:
    - `UtilitiesCS.Test\OutlookObjects\Table\OlToDoTableTests.cs` contains `GetToDoTable_returns_null_or_safe_result_when_folder_is_missing`.
    - The test `UtilitiesCS.Test.OutlookObjects.Table.OlToDoTableTests.GetToDoTable_returns_null_or_safe_result_when_folder_is_missing` passes.

- [x] [P2-T7] Add test method `Column_configuration_applies_expected_defaults` to `UtilitiesCS.Test\OutlookObjects\Table\OlToDoTableTests.cs`
  - Acceptance:
    - `UtilitiesCS.Test\OutlookObjects\Table\OlToDoTableTests.cs` contains `Column_configuration_applies_expected_defaults`.
    - The test `UtilitiesCS.Test.OutlookObjects.Table.OlToDoTableTests.Column_configuration_applies_expected_defaults` passes.

- [x] [P2-T8] Create `UtilitiesCS.Test\OutlookObjects\Table\OlTableExtensionsTransformTests.cs`
  - Acceptance:
    - The file exists at the exact path.
    - `UtilitiesCS.Test\UtilitiesCS.Test.csproj` contains `<Compile Include="OutlookObjects\Table\OlTableExtensionsTransformTests.cs" />`.

- [x] [P2-T9] Create `UtilitiesCS.Test\OutlookObjects\Table\OlTableExtensionsRetryTests.cs`
  - Acceptance:
    - The file exists at the exact path.
    - `UtilitiesCS.Test\UtilitiesCS.Test.csproj` contains `<Compile Include="OutlookObjects\Table\OlTableExtensionsRetryTests.cs" />`.

- [x] [P2-T10] Create `UtilitiesCS.Test\OutlookObjects\Table\OlTableExtensionsConversionTests.cs`
  - Acceptance:
    - The file exists at the exact path.
    - `UtilitiesCS.Test\UtilitiesCS.Test.csproj` contains `<Compile Include="OutlookObjects\Table\OlTableExtensionsConversionTests.cs" />`.

- [x] [P2-T11] Add one row or retry seam to `UtilitiesCS\OutlookObjects\Table\OlTableExtensions.cs`
  - Acceptance:
    - `UtilitiesCS\OutlookObjects\Table\OlTableExtensions.cs` is the only production file changed by this task.
    - The seam isolates retry or timing execution only.
    - `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU"` exits with code `0`.

- [x] [P2-T12] Add test method `GetColumnDictionary_returns_expected_name_value_pairs` to `UtilitiesCS.Test\OutlookObjects\Table\OlTableExtensionsTransformTests.cs`
  - Acceptance:
    - `UtilitiesCS.Test\OutlookObjects\Table\OlTableExtensionsTransformTests.cs` contains `GetColumnDictionary_returns_expected_name_value_pairs`.
    - The test `UtilitiesCS.Test.OutlookObjects.Table.OlTableExtensionsTransformTests.GetColumnDictionary_returns_expected_name_value_pairs` passes.

- [x] [P2-T13] Add test method `Retry_stops_after_successful_table_call` to `UtilitiesCS.Test\OutlookObjects\Table\OlTableExtensionsRetryTests.cs`
  - Acceptance:
    - `UtilitiesCS.Test\OutlookObjects\Table\OlTableExtensionsRetryTests.cs` contains `Retry_stops_after_successful_table_call`.
    - The test `UtilitiesCS.Test.OutlookObjects.Table.OlTableExtensionsRetryTests.Retry_stops_after_successful_table_call` passes.

- [x] [P2-T14] Add test method `Retry_returns_controlled_failure_after_exhaustion` to `UtilitiesCS.Test\OutlookObjects\Table\OlTableExtensionsRetryTests.cs`
  - Acceptance:
    - `UtilitiesCS.Test\OutlookObjects\Table\OlTableExtensionsRetryTests.cs` contains `Retry_returns_controlled_failure_after_exhaustion`.
    - The test `UtilitiesCS.Test.OutlookObjects.Table.OlTableExtensionsRetryTests.Retry_returns_controlled_failure_after_exhaustion` passes.

- [x] [P2-T15] Add test method `ToObjectRow_projects_binary_and_scalar_values_correctly` to `UtilitiesCS.Test\OutlookObjects\Table\OlTableExtensionsConversionTests.cs`
  - Acceptance:
    - `UtilitiesCS.Test\OutlookObjects\Table\OlTableExtensionsConversionTests.cs` contains `ToObjectRow_projects_binary_and_scalar_values_correctly`.
    - The test `UtilitiesCS.Test.OutlookObjects.Table.OlTableExtensionsConversionTests.ToObjectRow_projects_binary_and_scalar_values_correctly` passes.

### Phase 3 — Store and Folder Hotspots

- [x] [P3-T1] Create `UtilitiesCS.Test\OutlookObjects\Store\StoreWrapperControllerTests.cs`
  - Acceptance:
    - The file exists at the exact path.
    - `UtilitiesCS.Test\UtilitiesCS.Test.csproj` contains `<Compile Include="OutlookObjects\Store\StoreWrapperControllerTests.cs" />`.

- [x] [P3-T2] Add one dialog-selection seam to `UtilitiesCS\OutlookObjects\Store\StoreWrapperController.cs`
  - Acceptance:
    - `UtilitiesCS\OutlookObjects\Store\StoreWrapperController.cs` is the only production file changed by this task.
    - The seam isolates picker or dialog result acquisition only.
    - `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU"` exits with code `0`.

- [x] [P3-T3] Add test method `Controller_applies_selected_folder_when_dialog_returns_success` to `UtilitiesCS.Test\OutlookObjects\Store\StoreWrapperControllerTests.cs`
  - Acceptance:
    - `UtilitiesCS.Test\OutlookObjects\Store\StoreWrapperControllerTests.cs` contains `Controller_applies_selected_folder_when_dialog_returns_success`.
    - The test `UtilitiesCS.Test.OutlookObjects.Store.StoreWrapperControllerTests.Controller_applies_selected_folder_when_dialog_returns_success` passes.

- [x] [P3-T4] Add test method `Controller_leaves_state_unchanged_when_dialog_is_cancelled` to `UtilitiesCS.Test\OutlookObjects\Store\StoreWrapperControllerTests.cs`
  - Acceptance:
    - `UtilitiesCS.Test\OutlookObjects\Store\StoreWrapperControllerTests.cs` contains `Controller_leaves_state_unchanged_when_dialog_is_cancelled`.
    - The test `UtilitiesCS.Test.OutlookObjects.Store.StoreWrapperControllerTests.Controller_leaves_state_unchanged_when_dialog_is_cancelled` passes.

- [x] [P3-T5] Create `UtilitiesCS.Test\OutlookObjects\Folder\FolderWrapperStateTests.cs`
  - Acceptance:
    - The file exists at the exact path.
    - `UtilitiesCS.Test\UtilitiesCS.Test.csproj` contains `<Compile Include="OutlookObjects\Folder\FolderWrapperStateTests.cs" />`.

- [x] [P3-T6] Create `UtilitiesCS.Test\OutlookObjects\Folder\FolderWrapperTraversalTests.cs`
  - Acceptance:
    - The file exists at the exact path.
    - `UtilitiesCS.Test\UtilitiesCS.Test.csproj` contains `<Compile Include="OutlookObjects\Folder\FolderWrapperTraversalTests.cs" />`.

- [x] [P3-T7] Add one enumeration or release seam to `UtilitiesCS\OutlookObjects\Folder\FolderWrapper .cs`
  - Acceptance:
    - `UtilitiesCS\OutlookObjects\Folder\FolderWrapper .cs` is the only production file changed by this task.
    - The seam isolates child enumeration or RCW-adjacent release behavior only.
    - `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU"` exits with code `0`.

- [x] [P3-T8] Add test method `Lazy_name_and_relative_path_load_once` to `UtilitiesCS.Test\OutlookObjects\Folder\FolderWrapperStateTests.cs`
  - Acceptance:
    - `UtilitiesCS.Test\OutlookObjects\Folder\FolderWrapperStateTests.cs` contains `Lazy_name_and_relative_path_load_once`.
    - The test `UtilitiesCS.Test.OutlookObjects.Folder.FolderWrapperStateTests.Lazy_name_and_relative_path_load_once` passes.

- [x] [P3-T9] Add test method `Traversal_returns_expected_children_without_live_com_release` to `UtilitiesCS.Test\OutlookObjects\Folder\FolderWrapperTraversalTests.cs`
  - Acceptance:
    - `UtilitiesCS.Test\OutlookObjects\Folder\FolderWrapperTraversalTests.cs` contains `Traversal_returns_expected_children_without_live_com_release`.
    - The test `UtilitiesCS.Test.OutlookObjects.Folder.FolderWrapperTraversalTests.Traversal_returns_expected_children_without_live_com_release` passes.

- [x] [P3-T10] Create `UtilitiesCS.Test\OutlookObjects\Folder\FolderTreeTests.cs`
  - Acceptance:
    - The file exists at the exact path.
    - `UtilitiesCS.Test\UtilitiesCS.Test.csproj` contains `<Compile Include="OutlookObjects\Folder\FolderTreeTests.cs" />`.

- [x] [P3-T11] Add test method `Flatten_returns_all_nodes_in_expected_order` to `UtilitiesCS.Test\OutlookObjects\Folder\FolderTreeTests.cs`
  - Acceptance:
    - `UtilitiesCS.Test\OutlookObjects\Folder\FolderTreeTests.cs` contains `Flatten_returns_all_nodes_in_expected_order`.
    - The test `UtilitiesCS.Test.OutlookObjects.Folder.FolderTreeTests.Flatten_returns_all_nodes_in_expected_order` passes.

- [x] [P3-T12] Add test method `Selection_filter_excludes_non_matching_nodes` to `UtilitiesCS.Test\OutlookObjects\Folder\FolderTreeTests.cs`
  - Acceptance:
    - `UtilitiesCS.Test\OutlookObjects\Folder\FolderTreeTests.cs` contains `Selection_filter_excludes_non_matching_nodes`.
    - The test `UtilitiesCS.Test.OutlookObjects.Folder.FolderTreeTests.Selection_filter_excludes_non_matching_nodes` passes.

- [x] [P3-T13] Create `UtilitiesCS.Test\OutlookObjects\Folder\FolderPredictorTests.cs`
  - Acceptance:
    - The file exists at the exact path.
    - `UtilitiesCS.Test\UtilitiesCS.Test.csproj` contains `<Compile Include="OutlookObjects\Folder\FolderPredictorTests.cs" />`.

- [x] [P3-T14] Add one dialog or filesystem seam to `UtilitiesCS\OutlookObjects\Folder\FolderPredictor.cs`
  - Acceptance:
    - `UtilitiesCS\OutlookObjects\Folder\FolderPredictor.cs` is the only production file changed by this task.
    - The seam isolates dialog or directory-creation side effects only.
    - `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU"` exits with code `0`.

- [x] [P3-T15] Add test method `Predictor_returns_highest_ranked_match_from_seed_data` to `UtilitiesCS.Test\OutlookObjects\Folder\FolderPredictorTests.cs`
  - Acceptance:
    - `UtilitiesCS.Test\OutlookObjects\Folder\FolderPredictorTests.cs` contains `Predictor_returns_highest_ranked_match_from_seed_data`.
    - The test `UtilitiesCS.Test.OutlookObjects.Folder.FolderPredictorTests.Predictor_returns_highest_ranked_match_from_seed_data` passes.

- [x] [P3-T16] Add test method `Predictor_returns_controlled_result_when_user_choice_is_cancelled` to `UtilitiesCS.Test\OutlookObjects\Folder\FolderPredictorTests.cs`
  - Acceptance:
    - `UtilitiesCS.Test\OutlookObjects\Folder\FolderPredictorTests.cs` contains `Predictor_returns_controlled_result_when_user_choice_is_cancelled`.
    - The test `UtilitiesCS.Test.OutlookObjects.Folder.FolderPredictorTests.Predictor_returns_controlled_result_when_user_choice_is_cancelled` passes.

### Phase 4 — Mirrored Layout, Compile Includes, and Coverage Evidence

- [x] [P4-T1] Reconcile `UtilitiesCS.Test\UtilitiesCS.Test.csproj` so every new OutlookObjects test file from Phases 1–3 is compiled exactly once
  - Acceptance:
    - `UtilitiesCS.Test\UtilitiesCS.Test.csproj` contains one `<Compile Include="...">` line for each new file from Phases 1–3.
    - No file from Phases 1–3 appears more than once in `UtilitiesCS.Test\UtilitiesCS.Test.csproj`.

- [x] [P4-T2] Record the mirrored-layout audit for OutlookObjects tests
  - Acceptance:
    - At least one file matching `evidence/other/outlookobjects-mirrored-layout-audit.*.md` exists.
    - The newest matching artifact lists every OutlookObjects test file added or expanded by this plan and its matching production file.

- [x] [P4-T3] Record the blocked-branch ledger for any remaining intentionally uncovered branch families
  - Acceptance:
    - At least one file matching `evidence/other/outlookobjects-blocked-branches.*.md` exists.
    - Each entry in the newest matching artifact includes `Exact File Path:`, `Exact Branch Type:`, and `Minimum Unblock Seam:`.

### Phase 5 — Final QA Loop

Run the full C# QA loop in strict order. If any task in this phase fails or changes files, restart from `P5-T1` and do not mark any later Phase 5 task complete until a fresh contiguous clean run is re-established.

- [x] [P5-T1] Run `dotnet restore TaskMaster.sln`
  - Acceptance:
    - At least one file matching `evidence/qa-gates/final-dotnet-restore.*.md` exists.
    - The newest matching artifact contains `Timestamp:`, `Command: dotnet restore TaskMaster.sln`, `EXIT_CODE:`, and `Output Summary:`.

- [x] [P5-T2] Run `msbuild TaskMaster.sln /t:Restore /p:RestorePackagesConfig=true /p:Configuration=Debug /p:Platform="Any CPU"`
  - Acceptance:
    - At least one file matching `evidence/qa-gates/final-packagesconfig-restore.*.md` exists.
    - The newest matching artifact contains `Timestamp:`, the exact command, `EXIT_CODE:`, and `Output Summary:`.

- [x] [P5-T3] Run `dotnet format TaskMaster.sln --verify-no-changes --no-restore`
  - Acceptance:
    - At least one file matching `evidence/qa-gates/final-format.*.md` exists.
    - The newest matching artifact contains `Timestamp:`, the exact command, `EXIT_CODE: 0`, and `Output Summary:`.

- [x] [P5-T4] Run `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
  - Acceptance:
    - At least one file matching `evidence/qa-gates/final-analyzers.*.md` exists.
    - The newest matching artifact contains `Timestamp:`, the exact command, `EXIT_CODE: 0`, and `Output Summary:`.

- [x] [P5-T5] Run `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true`
  - Acceptance:
    - At least one file matching `evidence/qa-gates/final-nullable.*.md` exists.
    - The newest matching artifact contains `Timestamp:`, the exact command, `EXIT_CODE: 0`, and `Output Summary:`.

- [x] [P5-T6] Run `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug`
  - Acceptance:
    - At least one file matching `evidence/qa-gates/final-test-coverage.*.md` exists.
    - The newest matching artifact contains `Timestamp:`, the exact command, `EXIT_CODE: 0`, and `Output Summary:` with numeric pass count, numeric fail count, numeric skip count, numeric repository-wide coverage headline, numeric OutlookObjects coverage headline, numeric new-or-changed-code coverage headline, and `Threshold Result: pass`.

- [x] [P5-T7] Record the post-implementation OutlookObjects coverage gap report from `coverage\coverage.cobertura.xml`
  - Acceptance:
    - At least one file matching `evidence/qa-gates/final-outlookobjects-coverage-gap.*.md` exists.
    - Every `coverage-target` file from the Phase 0 matrix appears in the newest matching artifact with a numeric line-rate.
    - Any remaining file below `80%` is cross-referenced to the newest blocked-branch ledger artifact.

- [x] [P5-T8] Record final per-file OutlookObjects coverage from `coverage\coverage.cobertura.xml`
  - Acceptance:
    - At least one file matching `evidence/qa-gates/final-outlookobjects-per-file-coverage.*.md` exists.
    - Every `coverage-target` file from the Phase 0 matrix appears in the newest matching artifact with a numeric final line-rate.
    - Every `coverage-target` file is at or above `80%`.

- [x] [P5-T9] Record the baseline-versus-final OutlookObjects coverage delta
  - Acceptance:
    - At least one file matching `evidence/qa-gates/final-outlookobjects-coverage-delta.*.md` exists.
    - The newest matching artifact includes numeric baseline and final repository-wide coverage values, numeric baseline and final OutlookObjects module coverage values, numeric baseline and final new-or-changed-code coverage values, and numeric per-file deltas for every `coverage-target` file.
    - The newest matching artifact contains `Coverage Policy Result: pass` and `Repository Coverage Regression: none`.

- [x] [P5-T10] Verify the final Phase 5 artifact set represents one contiguous clean QA run
  - Acceptance:
    - The newest matching artifacts for `final-dotnet-restore`, `final-packagesconfig-restore`, `final-format`, `final-analyzers`, `final-nullable`, `final-test-coverage`, `final-outlookobjects-coverage-gap`, `final-outlookobjects-per-file-coverage`, and `final-outlookobjects-coverage-delta` all refer to the same clean run.
    - No artifact in that final set records a non-zero final `EXIT_CODE`.
    - No post-`P5-T3` step in that final set reports file modifications.
    - The final delta artifact reports `Coverage Policy Result: pass` and `Repository Coverage Regression: none`.
    - No `coverage-target` file remains below `80%` in the final per-file coverage artifact.
