# 2026-03-14-outlook-objects-test-coverage-67 — Plan

- **Issue:** #67
- **Parent (optional):** none
- **Owner:** drmoisan
- **Last Updated:** 2026-03-14T12-13
- **Status:** Draft
- **Version:** 1.0
- **Work Mode:** full-feature
- **Budget:** large path (~45-52 new or expanded OutlookObjects test files, ~5 targeted production seam updates, explicit coverage/blocker evidence)
- **Branch Coordination Note:** Current working branch is `feature/outlook-objects-test-coverage-66` while the active feature/issue is `#67`; treat this as a coordination note only and do not rename branches as part of execution.

## Overview

Raise `UtilitiesCS\OutlookObjects` unit coverage from the researched ~27% baseline to at least 80% line coverage for every **testable** file by adding mirrored MSTest coverage under `UtilitiesCS.Test\OutlookObjects`, updating explicit `Compile Include` entries in `UtilitiesCS.Test.csproj`, and using Moq + FluentAssertions throughout. Execution is batched by testability so the low-risk wins land first, seam work stays local to hotspot files, and blocked branches are documented precisely instead of being hand-waved into the nearest rug.

## Required References

- [`.github/copilot-instructions.md`](../../../../.github/copilot-instructions.md)
- [`.github/instructions/general-code-change.instructions.md`](../../../../.github/instructions/general-code-change.instructions.md)
- [`.github/instructions/general-unit-test.instructions.md`](../../../../.github/instructions/general-unit-test.instructions.md)
- [`.github/instructions/csharp-code-change.instructions.md`](../../../../.github/instructions/csharp-code-change.instructions.md)
- [`.github/instructions/csharp-unit-test.instructions.md`](../../../../.github/instructions/csharp-unit-test.instructions.md)

**All work must comply with these policies; do not duplicate their content here.**

## Requirements Sources

- **Binding execution inputs:** `issue.md`, `spec.md`, `user-story.md`
- **Informational input:** `research.md`
- **Conflict rule:** If `issue.md`, `spec.md`, and `user-story.md` disagree, stop for plan revision before execution.
- **Mode resolution rule:** `issue.md` does not currently contain a `Work Mode` marker; execution must honor `full-feature` via fail-closed mode resolution and the caller directive for this planning cycle.

## Toolchain Commands (C#)

| Step | Command |
|------|---------|
| Restore | `dotnet restore TaskMaster.sln` |
| Format | `dotnet format TaskMaster.sln --verify-no-changes --no-restore` |
| Analyzers | `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` |
| Nullable | `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true` |
| Test | `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-MSTest.ps1 -SearchRoot . -Configuration Debug` |
| Test+Coverage | `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug` |

## Implementation Acceptance Standards

### Test-File Acceptance Standard (Phases 1–5 test tasks)

Every test-file task shares one binary outcome: the named mirrored test file is valid.

1. The test file exists at the exact mirrored path stated in the task and remains at or below the repo 500-line limit.
2. `UtilitiesCS.Test\UtilitiesCS.Test.csproj` contains the exact `Compile Include` line whose path matches the mirrored test file named in the task.
3. If a legacy flat OutlookObjects test file previously covered the same production file, the flat file is no longer the compiled authoritative location for that production file.
4. `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU"` exits with code 0 after the task’s changes.
5. The named MSTest class or classes pass with 0 failures in targeted or phase-level MSTest execution.

### Seam Acceptance Standard (Phases 4–5 production seam tasks)

Every seam task shares one binary outcome: exactly one hotspot file gains one narrow, test-enabling seam.

1. Only the named production file is modified for the seam unless the compiler requires a directly adjacent call-site/interface update in the same OutlookObjects folder.
2. The seam isolates one blocked dependency family only: dialog/UI, filesystem/temp-file, Outlook namespace/item resolution, row/table retry timing, or RCW release behavior.
3. The change preserves existing public behavior and does not introduce a broad adapter rewrite.
4. `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU"` exits with code 0 after the seam is introduced.

## Coverage Target Notes

- `UtilitiesCS.Test\UtilitiesCS.Test.csproj` uses explicit `Compile Include` entries; every new or relocated OutlookObjects test file must be added there or it will not build or run.
- `UtilitiesCS\OutlookObjects\Folder\FolderWrapper .cs` includes a literal space before `.cs`; all evidence, compile includes, and coverage references must use the exact path.
- Blocked-branch families already identified by research and policy are: temp-file branches, WinForms/dialog branches, live Outlook namespace/profile branches, and RCW lifetime branches (`Marshal.ReleaseComObject` / `Marshal.FinalReleaseComObject`).
- The Phase 0 target matrix must classify every OutlookObjects production file as one of: `coverage-target`, `excluded-non-executable`, or `blocked-candidate`.
- Expected immediate non-executable exclusions unless Phase 0 evidence proves otherwise:
  - `UtilitiesCS\OutlookObjects\Store\IStoreWrapperViewer.cs` — interface only
  - `UtilitiesCS\OutlookObjects\Store\StoreWrapperViewer.Designer.cs` — designer-generated WinForms code
  - `UtilitiesCS\OutlookObjects\Folder\MsgToMime\MAPIMethods.cs` — interop declaration file with no meaningful business logic branches
- Post-baseline correction rule: if later execution evidence proves that a Phase 0 `coverage-target` is actually non-live or non-compiled, the specific correction artifact named in this plan supersedes the original Phase 0 matrix row for all remaining implementation, audit, and QA tasks.
- Verified post-approval correction candidates already in scope:
  - `UtilitiesCS\OutlookObjects\Item\ItemComparer.cs` — commented-out stub only; no live `ItemComparer` type exists
  - `UtilitiesCS\OutlookObjects\MailResolution.cs` — legacy on-disk file with `MailResolution_ToRemove`; `UtilitiesCS\UtilitiesCS.csproj` compiles `UtilitiesCS\OutlookObjects\MailItem\MailResolution.cs` instead

## Implementation Plan (Atomic Tasks)

---

### Phase 0 — Context & Baseline Capture

- [x] [P0-T1] Read repo policy files in compliance order: (1) `.github/copilot-instructions.md`, (2) `general-code-change.instructions.md`, (3) `general-unit-test.instructions.md`, (4) `csharp-code-change.instructions.md`, (5) `csharp-unit-test.instructions.md`
  - Acceptance: Evidence artifact `evidence/baseline/phase0-instructions-read.md` exists with `Timestamp:`, `Policy Order:`, and an explicit list of the 5 files read

- [x] [P0-T2] Read the active feature inputs `issue.md`, `research.md`, `spec.md`, and `user-story.md`
  - Acceptance: Evidence artifact `evidence/baseline/phase0-feature-inputs.md` exists with `Timestamp:`, `Files Read:`, and a one-line status note for each feature input

- [x] [P0-T3] Record mode resolution, issue/branch coordination, and path hazards for this feature
  - Acceptance: Evidence artifact `evidence/baseline/phase0-context-resolution.md` exists with `Timestamp:`, `Resolved Work Mode: full-feature`, `Branch Coordination Note: feature/outlook-objects-test-coverage-66 vs issue #67`, and `Path Hazards:` including the exact `FolderWrapper .cs` filename

- [x] [P0-T4] Capture baseline restore state by running `dotnet restore TaskMaster.sln`
  - Acceptance: Evidence artifact `evidence/baseline/baseline-restore.{ISO-8601 yyyy-MM-ddTHH-mm}.md` exists with `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:`

- [x] [P0-T5] Capture baseline format state by running `dotnet format TaskMaster.sln --verify-no-changes --no-restore`
  - Acceptance: Evidence artifact `evidence/baseline/baseline-format.{ISO-8601 yyyy-MM-ddTHH-mm}.md` exists with `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:`

- [x] [P0-T6] Capture baseline analyzer build state by running `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
  - Acceptance: Evidence artifact `evidence/baseline/baseline-analyzers.{ISO-8601 yyyy-MM-ddTHH-mm}.md` exists with `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:`

- [x] [P0-T7] Capture baseline nullable build state by running `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true`
  - Acceptance: Evidence artifact `evidence/baseline/baseline-nullable.{ISO-8601 yyyy-MM-ddTHH-mm}.md` exists with `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:`

- [x] [P0-T8] Capture baseline test and coverage state by running `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug`
  - Acceptance: Evidence artifact `evidence/baseline/baseline-test-coverage.{ISO-8601 yyyy-MM-ddTHH-mm}.md` exists with `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:` including numeric pass/fail counts and the numeric OutlookObjects baseline coverage headline

- [x] [P0-T9] Record baseline per-file coverage for every `UtilitiesCS\OutlookObjects` production file from the emitted coverage report
  - Acceptance: Evidence artifact `evidence/baseline/baseline-outlookobjects-per-file-coverage.{ISO-8601 yyyy-MM-ddTHH-mm}.md` exists listing every OutlookObjects source file with its numeric line-rate percentage

- [x] [P0-T10] Create the OutlookObjects coverage target matrix for all 51 production files
  - Acceptance: Evidence artifact `evidence/baseline/baseline-outlookobjects-target-matrix.{ISO-8601 yyyy-MM-ddTHH-mm}.md` exists with one row per source file, exact path, classification (`coverage-target`, `excluded-non-executable`, or `blocked-candidate`), and a justification column for every non-target entry

### Phase 1 — Mirrored Low-Risk Logic, Coverage Corrections, and Comparers

- [x] [P1-T1] Create or relocate `UtilitiesCS.Test\OutlookObjects\Filter DASL\DASLFilterParserTests.cs` covering valid filters, malformed filters, empty input, null input, and nested condition parsing for `UtilitiesCS\OutlookObjects\Filter DASL\DASLFilterParser.cs`
  - Acceptance: Per test-file acceptance standard — file: `UtilitiesCS.Test\OutlookObjects\Filter DASL\DASLFilterParserTests.cs` | class: `DASLFilterParserTests`

- [x] [P1-T2] Create `UtilitiesCS.Test\OutlookObjects\Com\ComTypeTests.cs` covering enum/value conversion and guard behavior for `UtilitiesCS\OutlookObjects\Com\ComType.cs`
  - Acceptance: Per test-file acceptance standard — file: `UtilitiesCS.Test\OutlookObjects\Com\ComTypeTests.cs` | class: `ComTypeTests`

- [x] [P1-T3] Create `UtilitiesCS.Test\OutlookObjects\Calendar\CalendarTests.cs` covering deterministic calendar helper branches in `UtilitiesCS\OutlookObjects\Calendar\Calendar.cs`
  - Acceptance: Per test-file acceptance standard — file: `UtilitiesCS.Test\OutlookObjects\Calendar\CalendarTests.cs` | class: `CalendarTests`

- [x] [P1-T4] Create `UtilitiesCS.Test\OutlookObjects\MailResolutionTests.cs` covering the root `UtilitiesCS\OutlookObjects\MailResolution.cs`
  - Acceptance: Per test-file acceptance standard — file: `UtilitiesCS.Test\OutlookObjects\MailResolutionTests.cs` | class: `MailResolutionTests`
  - Note: Verified after execution that `UtilitiesCS\OutlookObjects\MailResolution.cs` is a non-compiled legacy file; `P1-T12` records the exclusion and `P2-T11` remains the authoritative live `MailResolution` coverage task.

- [x] [P1-T5] Create `UtilitiesCS.Test\OutlookObjects\Attachment\AttachmentHelperTests.cs` covering deterministic helper branches in `UtilitiesCS\OutlookObjects\Attachment\AttachmentHelper.cs`
  - Acceptance: Per test-file acceptance standard — file: `UtilitiesCS.Test\OutlookObjects\Attachment\AttachmentHelperTests.cs` | class: `AttachmentHelperTests`

- [x] [P1-T6] Create or relocate `UtilitiesCS.Test\OutlookObjects\Folder\FolderWrapperNameComparerTests.cs` covering equality, nulls, case handling, and special-character comparisons for `UtilitiesCS\OutlookObjects\Folder\FolderWrapperNameComparer.cs`
  - Acceptance: Per test-file acceptance standard — file: `UtilitiesCS.Test\OutlookObjects\Folder\FolderWrapperNameComparerTests.cs` | class: `FolderWrapperNameComparerTests`

- [x] [P1-T7] Create or relocate `UtilitiesCS.Test\OutlookObjects\Folder\FolderWrapperNameAndParentNameComparerTests.cs` covering same-name/different-parent and null-parent scenarios for `UtilitiesCS\OutlookObjects\Folder\FolderWrapperNameAndParentNameComparer.cs`
  - Acceptance: Per test-file acceptance standard — file: `UtilitiesCS.Test\OutlookObjects\Folder\FolderWrapperNameAndParentNameComparerTests.cs` | class: `FolderWrapperNameAndParentNameComparerTests`

- [x] [P1-T8] Create or relocate `UtilitiesCS.Test\OutlookObjects\Folder\FolderWrapperNameCountSizeComparerTests.cs` covering name/count/size ordering branches for `UtilitiesCS\OutlookObjects\Folder\FolderWrapperNameCountSizeComparer.cs`
  - Acceptance: Per test-file acceptance standard — file: `UtilitiesCS.Test\OutlookObjects\Folder\FolderWrapperNameCountSizeComparerTests.cs` | class: `FolderWrapperNameCountSizeComparerTests`

- [x] [P1-T9] Create or relocate `UtilitiesCS.Test\OutlookObjects\Folder\FolderWrapperNodeComparerTests.cs` covering same-node, different-node, null-node, and tree-depth comparisons for `UtilitiesCS\OutlookObjects\Folder\FolderWrapperNodeComparer.cs`
  - Acceptance: Per test-file acceptance standard — file: `UtilitiesCS.Test\OutlookObjects\Folder\FolderWrapperNodeComparerTests.cs` | class: `FolderWrapperNodeComparerTests`

- [x] [P1-T10] Create or relocate `UtilitiesCS.Test\OutlookObjects\Folder\FolderWrapperNodeContentsComparerTests.cs` covering content equality and mismatch cases for `UtilitiesCS\OutlookObjects\Folder\FolderWrapperNodeContentsComparer.cs`
  - Acceptance: Per test-file acceptance standard — file: `UtilitiesCS.Test\OutlookObjects\Folder\FolderWrapperNodeContentsComparerTests.cs` | class: `FolderWrapperNodeContentsComparerTests`

- [x] [P1-T11] Record the verified non-live exclusion for `UtilitiesCS\OutlookObjects\Item\ItemComparer.cs`
  - Acceptance: Evidence artifact `evidence/other/outlookobjects-itemcomparer-exclusion.{ISO-8601 yyyy-MM-ddTHH-mm}.md` exists with `Timestamp:`, `Exact File Path: UtilitiesCS\OutlookObjects\Item\ItemComparer.cs`, `Observed State: commented-out stub only`, `Search Result: no live class ItemComparer found`, and `Target Matrix Override: excluded-non-executable`

- [x] [P1-T12] Record the verified non-compiled exclusion for `UtilitiesCS\OutlookObjects\MailResolution.cs`
  - Acceptance: Evidence artifact `evidence/other/outlookobjects-root-mailresolution-exclusion.{ISO-8601 yyyy-MM-ddTHH-mm}.md` exists with `Timestamp:`, `Exact File Path: UtilitiesCS\OutlookObjects\MailResolution.cs`, `Observed Type Name: MailResolution_ToRemove`, `Compiled Include Evidence: OutlookObjects\MailItem\MailResolution.cs only`, and `Authoritative Follow-On Task: P2-T11`
  - Note: This correction task preserves the completed `P1-T4` checkmark while preventing the executor from treating the non-compiled root file as a live coverage target.

- [x] [P1-T13] Create or relocate `UtilitiesCS.Test\OutlookObjects\Item\OlItemSummaryTests.cs` covering constructor, property, `ToString`, and equality branches for `UtilitiesCS\OutlookObjects\Item\OlItemSummary.cs`
  - Acceptance: Per test-file acceptance standard — file: `UtilitiesCS.Test\OutlookObjects\Item\OlItemSummaryTests.cs` | class: `OlItemSummaryTests`

- [x] [P1-T14] Create or relocate `UtilitiesCS.Test\OutlookObjects\MailItem\ItemInfoTests.cs` covering constructor, property, equality, and serialization-safe branches for `UtilitiesCS\OutlookObjects\MailItem\ItemInfo.cs`
  - Acceptance: Per test-file acceptance standard — file: `UtilitiesCS.Test\OutlookObjects\MailItem\ItemInfoTests.cs` | class: `ItemInfoTests`

- [x] [P1-T15] Create `UtilitiesCS.Test\OutlookObjects\Recipient\RecipientInfoTests.cs` covering parsing/value-object branches in `UtilitiesCS\OutlookObjects\Recipient\RecipientInfo.cs`
  - Acceptance: Per test-file acceptance standard — file: `UtilitiesCS.Test\OutlookObjects\Recipient\RecipientInfoTests.cs` | class: `RecipientInfoTests`

- [x] [P1-T16] Create `UtilitiesCS.Test\OutlookObjects\Explorer\ExplorerActionsTests.cs` covering current-window selection, unreadable item handling, null application guards, and readable-item passthrough for `UtilitiesCS\OutlookObjects\Explorer\ExplorerActions.cs`
  - Acceptance: Per test-file acceptance standard — file: `UtilitiesCS.Test\OutlookObjects\Explorer\ExplorerActionsTests.cs` | class: `ExplorerActionsTests`

### Phase 2 — Existing-Pattern Wrappers and Small Object-Model Files

- [x] [P2-T1] Create or relocate `UtilitiesCS.Test\OutlookObjects\Attachment\AttachmentSerializableTests.cs` covering constructor, metadata, serialization-safe branches, and in-memory behavior for `UtilitiesCS\OutlookObjects\Attachment\AttachmentSerializable.cs`
  - Acceptance: Per test-file acceptance standard — file: `UtilitiesCS.Test\OutlookObjects\Attachment\AttachmentSerializableTests.cs` | class: `AttachmentSerializableTests`

- [x] [P2-T2] Expand `UtilitiesCS.Test\OutlookObjects\Recipient\RecipientStaticTests.cs` to cover HTML formatting, name extraction, null/empty guards, and mockable recipient resolution branches in `UtilitiesCS\OutlookObjects\Recipient\RecipientStatic.cs`
  - Acceptance: Per test-file acceptance standard — file: `UtilitiesCS.Test\OutlookObjects\Recipient\RecipientStaticTests.cs` | class: `RecipientStaticTests`

- [x] [P2-T3] Create `UtilitiesCS.Test\OutlookObjects\Store\StoreWrapperTests.cs` covering property projection, root folder selection, and isolated store metadata branches in `UtilitiesCS\OutlookObjects\Store\StoreWrapper.cs`
  - Acceptance: Per test-file acceptance standard — file: `UtilitiesCS.Test\OutlookObjects\Store\StoreWrapperTests.cs` | class: `StoreWrapperTests`

- [x] [P2-T4] Expand `UtilitiesCS.Test\OutlookObjects\Store\StoresWrapperTests.cs` to cover include/exclude filtering, duplicate handling, and safe initialization branches in `UtilitiesCS\OutlookObjects\Store\StoresWrapper.cs`
  - Acceptance: Per test-file acceptance standard — file: `UtilitiesCS.Test\OutlookObjects\Store\StoresWrapperTests.cs` | class: `StoresWrapperTests`

- [x] [P2-T5] Create `UtilitiesCS.Test\OutlookObjects\Store\StoreWrapperViewerTests.cs` covering event-forwarding and controller dispatch branches in `UtilitiesCS\OutlookObjects\Store\StoreWrapperViewer.cs`
  - Acceptance: Per test-file acceptance standard — file: `UtilitiesCS.Test\OutlookObjects\Store\StoreWrapperViewerTests.cs` | class: `StoreWrapperViewerTests`

- [x] [P2-T6] Create or relocate `UtilitiesCS.Test\OutlookObjects\Folder\FolderConverterTests.cs` covering sanitize, illegal-character, and root-resolution branches in `UtilitiesCS\OutlookObjects\Folder\FolderConverter.cs`
  - Acceptance: Per test-file acceptance standard — file: `UtilitiesCS.Test\OutlookObjects\Folder\FolderConverterTests.cs` | class: `FolderConverterTests`

- [x] [P2-T7] Create `UtilitiesCS.Test\OutlookObjects\Folder\FolderMinimalWrapperTests.cs` covering relative-path, restore, serialization, and null-guard branches in `UtilitiesCS\OutlookObjects\Folder\FolderMinimalWrapper.cs`
  - Acceptance: Per test-file acceptance standard — file: `UtilitiesCS.Test\OutlookObjects\Folder\FolderMinimalWrapperTests.cs` | class: `FolderMinimalWrapperTests`

- [x] [P2-T8] Create `UtilitiesCS.Test\OutlookObjects\Folder\FolderNavigatorTests.cs` covering folder-path normalization, lookup traversal, descendant flattening, and null-return branches in `UtilitiesCS\OutlookObjects\Folder\FolderNavigator.cs`
  - Acceptance: Per test-file acceptance standard — file: `UtilitiesCS.Test\OutlookObjects\Folder\FolderNavigatorTests.cs` | class: `FolderNavigatorTests`

- [x] [P2-T9] Create `UtilitiesCS.Test\OutlookObjects\MailItem\EmailDetailsTests.cs` covering DTO/value behavior in `UtilitiesCS\OutlookObjects\MailItem\EmailDetails.cs`
  - Acceptance: Per test-file acceptance standard — file: `UtilitiesCS.Test\OutlookObjects\MailItem\EmailDetailsTests.cs` | class: `EmailDetailsTests`

- [x] [P2-T10] Create `UtilitiesCS.Test\OutlookObjects\MailItem\EmailDetailsWrapperTests.cs` covering wrapper projection and null-safe access in `UtilitiesCS\OutlookObjects\MailItem\EmailDetailsWrapper.cs`
  - Acceptance: Per test-file acceptance standard — file: `UtilitiesCS.Test\OutlookObjects\MailItem\EmailDetailsWrapperTests.cs` | class: `EmailDetailsWrapperTests`

- [ ] [P2-T11] Create or relocate `UtilitiesCS.Test\OutlookObjects\MailItem\MailResolutionTests.cs` covering deterministic branches in the live compiled `UtilitiesCS\OutlookObjects\MailItem\MailResolution.cs`
  - Acceptance: Per test-file acceptance standard — file: `UtilitiesCS.Test\OutlookObjects\MailItem\MailResolutionTests.cs` | class: `MailItemMailResolutionTests`

- [ ] [P2-T12] Create `UtilitiesCS.Test\OutlookObjects\MailItem\MailItemExtensionsTests.cs` covering pure extension behavior in `UtilitiesCS\OutlookObjects\MailItem\MailItemExtensions.cs`
  - Acceptance: Per test-file acceptance standard — file: `UtilitiesCS.Test\OutlookObjects\MailItem\MailItemExtensionsTests.cs` | class: `MailItemExtensionsTests`

- [ ] [P2-T13] Create `UtilitiesCS.Test\OutlookObjects\MailItem\CaptureEmailAddressesModule2Tests.cs` covering deterministic address-extraction branches in `UtilitiesCS\OutlookObjects\MailItem\CaptureEmailAddressesModule2.cs`
  - Acceptance: Per test-file acceptance standard — file: `UtilitiesCS.Test\OutlookObjects\MailItem\CaptureEmailAddressesModule2Tests.cs` | class: `CaptureEmailAddressesModule2Tests`

- [ ] [P2-T14] Create `UtilitiesCS.Test\OutlookObjects\Category\CreateCategoryTests.cs` covering prefixing, duplicate prevention without UI invocation, add-success, and null/empty parameter branches that can be exercised safely in `UtilitiesCS\OutlookObjects\Category\CreateCategory.cs`
  - Acceptance: Per test-file acceptance standard — file: `UtilitiesCS.Test\OutlookObjects\Category\CreateCategoryTests.cs` | class: `CreateCategoryTests`

### Phase 3 — Reflection and Lazy-Wrapper Coverage

- [ ] [P3-T1] Create `UtilitiesCS.Test\OutlookObjects\Item\OutlookItemTests.cs` covering reflection-based property access, missing-member handling, method invocation, and exception branches in `UtilitiesCS\OutlookObjects\Item\OutlookItem.cs`
  - Acceptance: Per test-file acceptance standard — file: `UtilitiesCS.Test\OutlookObjects\Item\OutlookItemTests.cs` | class: `OutlookItemTests`

- [ ] [P3-T2] Create `UtilitiesCS.Test\OutlookObjects\Item\OutlookItemTryTests.cs` covering success/failure wrappers and null/exception behavior in `UtilitiesCS\OutlookObjects\Item\OutlookItemTry.cs`
  - Acceptance: Per test-file acceptance standard — file: `UtilitiesCS.Test\OutlookObjects\Item\OutlookItemTryTests.cs` | class: `OutlookItemTryTests`

- [ ] [P3-T3] Create `UtilitiesCS.Test\OutlookObjects\Item\OutlookItemTryGetTests.cs` covering try-get branches and default-value behavior in `UtilitiesCS\OutlookObjects\Item\OutlookItemTryGet.cs`
  - Acceptance: Per test-file acceptance standard — file: `UtilitiesCS.Test\OutlookObjects\Item\OutlookItemTryGetTests.cs` | class: `OutlookItemTryGetTests`

- [ ] [P3-T4] Create `UtilitiesCS.Test\OutlookObjects\Item\OutlookItemExtensionsTests.cs` covering extension methods that do not require live Outlook state in `UtilitiesCS\OutlookObjects\Item\OutlookItemExtensions.cs`
  - Acceptance: Per test-file acceptance standard — file: `UtilitiesCS.Test\OutlookObjects\Item\OutlookItemExtensionsTests.cs` | class: `OutlookItemExtensionsTests`

- [ ] [P3-T5] Create `UtilitiesCS.Test\OutlookObjects\Item\OutlookItemFlaggableTests.cs` covering flaggable wrapper state transitions and reflection branches in `UtilitiesCS\OutlookObjects\Item\OutlookItemFlaggable.cs`
  - Acceptance: Per test-file acceptance standard — file: `UtilitiesCS.Test\OutlookObjects\Item\OutlookItemFlaggableTests.cs` | class: `OutlookItemFlaggableTests`

- [ ] [P3-T6] Create `UtilitiesCS.Test\OutlookObjects\Item\OutlookItemFlaggableTryTests.cs` covering guarded flaggable access paths in `UtilitiesCS\OutlookObjects\Item\OutlookItemFlaggableTry.cs`
  - Acceptance: Per test-file acceptance standard — file: `UtilitiesCS.Test\OutlookObjects\Item\OutlookItemFlaggableTryTests.cs` | class: `OutlookItemFlaggableTryTests`

- [ ] [P3-T7] Create `UtilitiesCS.Test\OutlookObjects\Item\OlItemPseudoInterfaceTests.cs` covering pseudo-interface mapping behavior in `UtilitiesCS\OutlookObjects\Item\OlItemPseudoInterface.cs`
  - Acceptance: Per test-file acceptance standard — file: `UtilitiesCS.Test\OutlookObjects\Item\OlItemPseudoInterfaceTests.cs` | class: `OlItemPseudoInterfaceTests`

- [ ] [P3-T8] Create `UtilitiesCS.Test\OutlookObjects\AppointmentItem\MeetingItemHelperTests.cs` covering deterministic helper and override-enabled branches in `UtilitiesCS\OutlookObjects\AppointmentItem\MeetingItemHelper.cs`
  - Acceptance: Per test-file acceptance standard — file: `UtilitiesCS.Test\OutlookObjects\AppointmentItem\MeetingItemHelperTests.cs` | class: `MeetingItemHelperTests`

- [ ] [P3-T9] Create `UtilitiesCS.Test\OutlookObjects\Folder\FolderScorerTests.cs` covering additive scoring, top-N trimming, array ingestion, and query-shaping branches in `UtilitiesCS\OutlookObjects\Folder\FolderScorer.cs`
  - Acceptance: Per test-file acceptance standard — file: `UtilitiesCS.Test\OutlookObjects\Folder\FolderScorerTests.cs` | class: `FolderScorerTests`

- [ ] [P3-T10] Create `UtilitiesCS.Test\OutlookObjects\MailItem\MailItemHelperCoreTests.cs` covering static helpers, cache-free branches, compression, and null-guard behavior in `UtilitiesCS\OutlookObjects\MailItem\MailItemHelper.cs`
  - Acceptance: Per test-file acceptance standard — file: `UtilitiesCS.Test\OutlookObjects\MailItem\MailItemHelperCoreTests.cs` | class: `MailItemHelperCoreTests`

- [ ] [P3-T11] Create `UtilitiesCS.Test\OutlookObjects\MailItem\MailItemHelperProjectionTests.cs` covering projection, DTO conversion, and override-enabled lazy branches in `UtilitiesCS\OutlookObjects\MailItem\MailItemHelper.cs`
  - Acceptance: Per test-file acceptance standard — file: `UtilitiesCS.Test\OutlookObjects\MailItem\MailItemHelperProjectionTests.cs` | class: `MailItemHelperProjectionTests`

### Phase 4 — Seam-Enabled Fields, Conversation, and Table Hotspots

- [ ] [P4-T1] Extract a minimal property-accessor-independent seam in `UtilitiesCS\OutlookObjects\Fields\UserDefinedFields.cs`
  - Acceptance: Per seam acceptance standard — file: `UtilitiesCS\OutlookObjects\Fields\UserDefinedFields.cs`

- [ ] [P4-T2] Create `UtilitiesCS.Test\OutlookObjects\Fields\UserDefinedFieldsTests.cs` covering validation, default-value, lookup, and dictionary branches enabled by the `UserDefinedFields.cs` seam
  - Acceptance: Per test-file acceptance standard — file: `UtilitiesCS.Test\OutlookObjects\Fields\UserDefinedFieldsTests.cs` | class: `UserDefinedFieldsTests`

- [ ] [P4-T3] Create `UtilitiesCS.Test\OutlookObjects\Fields\MAPIFieldsTests.cs` covering deterministic schema/value branches in `UtilitiesCS\OutlookObjects\Fields\MAPIFields.cs`
  - Acceptance: Per test-file acceptance standard — file: `UtilitiesCS.Test\OutlookObjects\Fields\MAPIFieldsTests.cs` | class: `MAPIFieldsTests`

- [ ] [P4-T4] Extract a minimal namespace/item-resolution seam in `UtilitiesCS\OutlookObjects\Conversation\ConversationHelper.cs`
  - Acceptance: Per seam acceptance standard — file: `UtilitiesCS\OutlookObjects\Conversation\ConversationHelper.cs`

- [ ] [P4-T5] Create `UtilitiesCS.Test\OutlookObjects\Conversation\ConversationHelperTests.cs` covering transform, retry-safe, and mockable namespace-resolution branches enabled by the `ConversationHelper.cs` seam
  - Acceptance: Per test-file acceptance standard — file: `UtilitiesCS.Test\OutlookObjects\Conversation\ConversationHelperTests.cs` | class: `ConversationHelperTests`

- [ ] [P4-T6] Create `UtilitiesCS.Test\OutlookObjects\Table\OlToDoTableTests.cs` covering null returns, default-folder lookup, column configuration, and safe item-value update branches in `UtilitiesCS\OutlookObjects\Table\OlToDoTable.cs`
  - Acceptance: Per test-file acceptance standard — file: `UtilitiesCS.Test\OutlookObjects\Table\OlToDoTableTests.cs` | class: `OlToDoTableTests`

- [ ] [P4-T7] Extract a minimal row/timeout seam in `UtilitiesCS\OutlookObjects\Table\OlTableExtensions.cs`
  - Acceptance: Per seam acceptance standard — file: `UtilitiesCS\OutlookObjects\Table\OlTableExtensions.cs`

- [ ] [P4-T8] Create `UtilitiesCS.Test\OutlookObjects\Table\OlTableExtensionsTransformTests.cs` covering ETL and column-shaping branches enabled by the `OlTableExtensions.cs` seam
  - Acceptance: Per test-file acceptance standard — file: `UtilitiesCS.Test\OutlookObjects\Table\OlTableExtensionsTransformTests.cs` | class: `OlTableExtensionsTransformTests`

- [ ] [P4-T9] Create `UtilitiesCS.Test\OutlookObjects\Table\OlTableExtensionsRetryTests.cs` covering retry, timeout, and controlled failure branches enabled by the `OlTableExtensions.cs` seam
  - Acceptance: Per test-file acceptance standard — file: `UtilitiesCS.Test\OutlookObjects\Table\OlTableExtensionsRetryTests.cs` | class: `OlTableExtensionsRetryTests`

- [ ] [P4-T10] Create `UtilitiesCS.Test\OutlookObjects\Table\OlTableExtensionsConversionTests.cs` covering conversion, binary/object projection, and result-shaping branches enabled by the `OlTableExtensions.cs` seam
  - Acceptance: Per test-file acceptance standard — file: `UtilitiesCS.Test\OutlookObjects\Table\OlTableExtensionsConversionTests.cs` | class: `OlTableExtensionsConversionTests`

### Phase 5 — Folder/Store Hotspots, RCW Seams, and Blocked-Branch Closure

- [ ] [P5-T1] Extract a minimal dialog/picker seam in `UtilitiesCS\OutlookObjects\Store\StoreWrapperController.cs`
  - Acceptance: Per seam acceptance standard — file: `UtilitiesCS\OutlookObjects\Store\StoreWrapperController.cs`

- [ ] [P5-T2] Create `UtilitiesCS.Test\OutlookObjects\Store\StoreWrapperControllerTests.cs` covering controller state transitions and dialog-free branches enabled by the `StoreWrapperController.cs` seam
  - Acceptance: Per test-file acceptance standard — file: `UtilitiesCS.Test\OutlookObjects\Store\StoreWrapperControllerTests.cs` | class: `StoreWrapperControllerTests`

- [ ] [P5-T3] Extract a minimal enumeration/release seam in `UtilitiesCS\OutlookObjects\Folder\FolderWrapper .cs`
  - Acceptance: Per seam acceptance standard — file: `UtilitiesCS\OutlookObjects\Folder\FolderWrapper .cs`

- [ ] [P5-T4] Create `UtilitiesCS.Test\OutlookObjects\Folder\FolderWrapperStateTests.cs` covering lazy state, name/path loading, and guard branches enabled by the `FolderWrapper .cs` seam
  - Acceptance: Per test-file acceptance standard — file: `UtilitiesCS.Test\OutlookObjects\Folder\FolderWrapperStateTests.cs` | class: `FolderWrapperStateTests`

- [ ] [P5-T5] Create `UtilitiesCS.Test\OutlookObjects\Folder\FolderWrapperTraversalTests.cs` covering child enumeration, comparer-facing behavior, and release-adjacent traversal branches enabled by the `FolderWrapper .cs` seam
  - Acceptance: Per test-file acceptance standard — file: `UtilitiesCS.Test\OutlookObjects\Folder\FolderWrapperTraversalTests.cs` | class: `FolderWrapperTraversalTests`

- [ ] [P5-T6] Create `UtilitiesCS.Test\OutlookObjects\Folder\FolderTreeTests.cs` covering root detangling, flattening, compare operations, selection filtering, and notification wiring in `UtilitiesCS\OutlookObjects\Folder\FolderTree.cs`
  - Acceptance: Per test-file acceptance standard — file: `UtilitiesCS.Test\OutlookObjects\Folder\FolderTreeTests.cs` | class: `FolderTreeTests`

- [ ] [P5-T7] Extract a minimal dialog/filesystem seam in `UtilitiesCS\OutlookObjects\Folder\FolderPredictor.cs`
  - Acceptance: Per seam acceptance standard — file: `UtilitiesCS\OutlookObjects\Folder\FolderPredictor.cs`

- [ ] [P5-T8] Create `UtilitiesCS.Test\OutlookObjects\Folder\FolderPredictorTests.cs` covering deterministic prediction and selection branches enabled by the `FolderPredictor.cs` seam
  - Acceptance: Per test-file acceptance standard — file: `UtilitiesCS.Test\OutlookObjects\Folder\FolderPredictorTests.cs` | class: `FolderPredictorTests`

- [ ] [P5-T9] Record the final blocked-branch ledger for temp-file, dialog/UI, live-Outlook, and RCW-lifetime branches that remain intentionally uncovered
  - Acceptance: Evidence artifact `evidence/other/outlookobjects-blocked-branches.{ISO-8601 yyyy-MM-ddTHH-mm}.md` exists with `Timestamp:`, one entry per blocked file/branch family, `Blocking Policy or Runtime Constraint:`, `Exact File Path:`, `Exact Branch Type:`, and `Minimum Unblock Seam:`

- [ ] [P5-T10] Audit mirrored folder placement for all OutlookObjects test files touched by Phases 1–5
  - Acceptance: Evidence artifact `evidence/other/outlookobjects-mirrored-layout-audit.{ISO-8601 yyyy-MM-ddTHH-mm}.md` exists with `Timestamp:`, `SearchScope: UtilitiesCS.Test\OutlookObjects`, an explicit list of touched test files, and confirmation that no touched production file still relies on a compiled flat test path outside its mirrored subfolder

- [ ] [P5-T11] Audit `UtilitiesCS.Test\UtilitiesCS.Test.csproj` compile includes for all OutlookObjects test files touched by Phases 1–5
  - Acceptance: Evidence artifact `evidence/other/outlookobjects-csproj-compile-include-audit.{ISO-8601 yyyy-MM-ddTHH-mm}.md` exists with `Timestamp:`, one line per touched test file, and the exact matching `Compile Include` line text copied from `UtilitiesCS.Test.csproj`

- [ ] [P5-T12] Capture an intermediate OutlookObjects coverage gap report before final QA
  - Acceptance: Evidence artifact `evidence/other/outlookobjects-coverage-gap.{ISO-8601 yyyy-MM-ddTHH-mm}.md` exists with numeric per-file line rates for all `coverage-target` files after applying the `P1-T11` and `P1-T12` target-matrix overrides, identifies any still below 80%, and cross-references `outlookobjects-blocked-branches` for every remaining shortfall

### Phase 6 — Final QA Loop

Run the full C# toolchain in strict order. If any step fails or changes files, restart from `P6-T1` until a clean pass completes.

- [ ] [P6-T1] Run `dotnet restore TaskMaster.sln` and record the result
  - Acceptance: Evidence artifact `evidence/qa-gates/final-restore.{ISO-8601 yyyy-MM-ddTHH-mm}.md` exists with `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:`

- [ ] [P6-T2] Run `dotnet format TaskMaster.sln --verify-no-changes --no-restore` and record the result
  - Acceptance: Evidence artifact `evidence/qa-gates/final-format.{ISO-8601 yyyy-MM-ddTHH-mm}.md` exists with `Timestamp:`, `Command:`, `EXIT_CODE: 0`, and `Output Summary:`

- [ ] [P6-T3] Run `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` and record the result
  - Preconditions: `P6-T2` passed
  - Acceptance: Evidence artifact `evidence/qa-gates/final-analyzers.{ISO-8601 yyyy-MM-ddTHH-mm}.md` exists with `Timestamp:`, `Command:`, `EXIT_CODE: 0`, and `Output Summary:`

- [ ] [P6-T4] Run `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true` and record the result
  - Preconditions: `P6-T3` passed
  - Acceptance: Evidence artifact `evidence/qa-gates/final-nullable.{ISO-8601 yyyy-MM-ddTHH-mm}.md` exists with `Timestamp:`, `Command:`, `EXIT_CODE: 0`, and `Output Summary:`

- [ ] [P6-T5] Run `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug` and record the result
  - Preconditions: `P6-T4` passed
  - Acceptance: Evidence artifact `evidence/qa-gates/final-test-coverage.{ISO-8601 yyyy-MM-ddTHH-mm}.md` exists with `Timestamp:`, `Command:`, `EXIT_CODE: 0`, and `Output Summary:` including numeric pass/fail counts and the numeric OutlookObjects coverage headline

- [ ] [P6-T6] Verify final per-file OutlookObjects coverage against the Phase 0 target matrix
  - Preconditions: `P6-T5` passed with coverage data
  - Acceptance: Evidence artifact `evidence/qa-gates/final-outlookobjects-per-file-coverage.{ISO-8601 yyyy-MM-ddTHH-mm}.md` exists with one row per OutlookObjects source file, exact target classification after applying the `P1-T11` and `P1-T12` target-matrix overrides, numeric final line-rate percentage, every `coverage-target` file at or above 80%, and excluded files listed separately with their exclusion rationale

- [ ] [P6-T7] Record baseline-versus-final OutlookObjects coverage deltas
  - Preconditions: `P6-T6` passed
  - Acceptance: Evidence artifact `evidence/qa-gates/final-outlookobjects-coverage-delta.{ISO-8601 yyyy-MM-ddTHH-mm}.md` exists with numeric baseline and final line-rate values for the OutlookObjects module plus numeric per-file deltas for every `coverage-target` file

- [ ] [P6-T8] Restart the Phase 6 loop if any QA step failed, changed files, or produced a target file below 80%
  - Acceptance: `P6-T1` through `P6-T7` pass in one contiguous clean run with no file modifications after `P6-T2` and no `coverage-target` file below 80%

---

## Test Plan

Mirrored MSTest coverage under `UtilitiesCS.Test\OutlookObjects`, using Moq for Outlook COM doubles and FluentAssertions for assertions. No live Outlook, no WinForms rendering dependencies, and no temporary-file creation; blocked branches are recorded explicitly in evidence rather than silently omitted.

## Open Questions / Notes

- `StoreWrapperViewer.cs` is expected to be covered via direct event-forwarding tests without requiring WinForms rendering; `StoreWrapperViewer.Designer.cs` remains excluded as designer code.
- `AttachmentSerializable.cs`, `FolderConverter.cs`, `CreateCategory.cs`, `StoreWrapperController.cs`, `FolderPredictor.cs`, `ConversationHelper.cs`, `OlTableExtensions.cs`, and `FolderWrapper .cs` all have branch families that may still need explicit blocker entries even after seam work.
- The execution budget assumes several existing flat OutlookObjects tests will be relocated into mirrored subfolders rather than duplicated.
