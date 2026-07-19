# utilitiescs-nullable-residuals — Atomic Implementation Plan

- **Issue:** #375
- **Parent:** Epic `utilitiescs-nullable-remediation` (Wave 1)
- **Owner:** drmoisan
- **Last Updated:** 2026-07-18T23-13
- **Work Mode:** full-feature
- **Requirements sources:** `spec.md` (AC1–AC8, Maintainer Decisions, Implementation Strategy), `user-story.md`, `research/2026-07-18T21-30-residuals-nullable-research.md`, epic `docs/features/epics/utilitiescs-nullable-remediation/epic.md`.

## Scope Summary

Annotation-only, per-file `#nullable enable` remediation of the 44 residual CS86xx-risk files. Effective compiled hand-written opt-in set is **37 files** (44 − 6 `*.Designer.cs` never-opted-in − 1 dead uncompiled duplicate `PeopleScoDictionaryNewBackup.cs`). No behavior change, no refactor, no project/solution `<Nullable>` element.

Verification is the **pragma-only** rebuild `msbuild TaskMaster.sln /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU" /p:TreatWarningsAsErrors=true` (NO `/p:Nullable=enable`). net481 constraints: no post-condition attributes (`[NotNullWhen]` etc.), no `record`/`record struct`/`init`. Prefer annotation plus justified `!` over new runtime guards; preserve existing guards exactly. Keep annotations consistent with upstream #363/#364/#369 signatures: `TimeOutTask.RunWithTimeout` returns non-null `Task<TResult>`; `StreamExtensions.TryCopyToAsyncWithTimeout` returns `Task<bool>`; `IsNullOrEmpty(this string?)` is non-refining on net481.

All evidence artifacts are written under `docs/features/active/utilitiescs-nullable-residuals/evidence/<kind>/`. `<timestamp>` in artifact filenames is the ISO-8601 `yyyy-MM-ddTHH-mm` execution timestamp.

Test-assembly resolution rule: the `UtilitiesCS.Test` assembly resolves at execution time to `UtilitiesCS.Test/bin/Debug/<TFM>/UtilitiesCS.Test.dll` (TFM produced by the Debug build). The canonical coverage runner `scripts/vscode/Invoke-MSTestWithCoverage.ps1` auto-discovers `*.Test.dll` under `bin\Debug\` and drives `vstest.console.exe` with coverage; use it (pointing `-CoverageOutput` at the canonical evidence path) or invoke `vstest.console.exe <resolved dll> /EnableCodeCoverage` directly against the resolved path.

## Implementation Plan (Atomic Tasks)

### Phase 0 — Baseline Capture and Policy Read

- [x] [P0-T1] Read the repository policies in order (1) `CLAUDE.md`, (2) `.claude/rules/general-code-change.md`, (3) `.claude/rules/general-unit-test.md`, (4) `.claude/rules/csharp.md`, then the epic manifest `docs/features/epics/utilitiescs-nullable-remediation/epic.md`; write evidence to `docs/features/active/utilitiescs-nullable-residuals/evidence/other/phase0-instructions-read.md`.
  - Acceptance: artifact exists and contains `Timestamp:`, `Policy Order:`, and an explicit list of the exact files read (the five files above).
- [x] [P0-T2] Run the pragma-only baseline build `msbuild TaskMaster.sln /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU" /p:TreatWarningsAsErrors=true` before any edit and record the result to `docs/features/active/utilitiescs-nullable-residuals/evidence/baseline/baseline-pragma-build.<timestamp>.md`.
  - Acceptance: artifact contains `Timestamp:`, `Command:` (exact command above), `EXIT_CODE:`, and `Output Summary:` stating whether the integration branch currently builds clean under the pragma-only gate. A clean pre-edit build is expected because a residual file's debt is only surfaced once its own `#nullable enable` pragma is added; this is the same command used for per-batch and final verification.
- [x] [P0-T3] Capture the baseline `UtilitiesCS.Test` run with coverage using `scripts/vscode/Invoke-MSTestWithCoverage.ps1` (or `vstest.console.exe <resolved UtilitiesCS.Test.dll> /EnableCodeCoverage`) and record the result to `docs/features/active/utilitiescs-nullable-residuals/evidence/baseline/baseline-tests-coverage.<timestamp>.md`.
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:` with numeric passed/failed test counts and the numeric coverage headline (line-rate and branch-rate from the emitted Cobertura root `<coverage>` element). This establishes the pre-edit reference so no pre-existing failure is attributed to this child (AC6).

### Phase 1 — Batch 0 Verify-Only Pragma Opt-In

- [x] [P1-T1] Add `#nullable enable` to `UtilitiesCS/EmailIntelligence/IntelligenceFilters.cs` and annotate to zero CS86xx.
  - Acceptance: file carries `#nullable enable`; the pragma-only rebuild reports zero CS86xx for this file (expected verify-only clean).
- [x] [P1-T2] Add `#nullable enable` to `UtilitiesCS/EmailIntelligence/Evaluation/EvaluationResult.cs` and annotate to zero CS86xx.
  - Acceptance: file carries `#nullable enable`; pragma-only rebuild reports zero CS86xx for this file.
- [x] [P1-T3] Add `#nullable enable` to `UtilitiesCS/OutlookObjects/Fields/MAPIFields.cs` and annotate to zero CS86xx.
  - Acceptance: file carries `#nullable enable`; pragma-only rebuild reports zero CS86xx for this file.
- [x] [P1-T4] Add `#nullable enable` to the EmailIntelligence-root `UtilitiesCS/EmailIntelligence/FolderConverter.cs` (NOT `UtilitiesCS/OutlookObjects/Folder/FolderConverter.cs`, which is out of scope for this child) and annotate to zero CS86xx.
  - Acceptance: only the EmailIntelligence-root file carries `#nullable enable`; pragma-only rebuild reports zero CS86xx for this file.
- [x] [P1-T5] Add `#nullable enable` to `UtilitiesCS/EmailIntelligence/OlFolderTools/FilterOlFolders/IFilterOlFoldersViewer.cs` and annotate to zero CS86xx.
  - Acceptance: file carries `#nullable enable`; pragma-only rebuild reports zero CS86xx for this file.
- [x] [P1-T6] Add `#nullable enable` to `UtilitiesCS/EmailIntelligence/OlFolderTools/FolderRemap/IFolderRemapViewer.cs` and annotate to zero CS86xx.
  - Acceptance: file carries `#nullable enable`; pragma-only rebuild reports zero CS86xx for this file.
- [x] [P1-T7] Add `#nullable enable` to `UtilitiesCS/EmailIntelligence/OlFolderTools/OlFolderHelper/SmithWaterman.cs` (376 lines; under 500) and annotate to zero CS86xx.
  - Acceptance: file carries `#nullable enable`; pragma-only rebuild reports zero CS86xx for this file (expected clean or 0–2 minor).
- [x] [P1-T8] Add `#nullable enable` to `UtilitiesCS/EmailIntelligence/OlFolderTools/FilterOlFolders/OSFolder.cs` and annotate to zero CS86xx.
  - Acceptance: file carries `#nullable enable`; pragma-only rebuild reports zero CS86xx for this file.
- [x] [P1-T9] Add `#nullable enable` to `UtilitiesCS/OutlookObjects/Filter DASL/DASLFilterParser.cs` and annotate to zero CS86xx; if the consumed `ReusableTypeClasses.TreeNode<string>.Value` is non-null (undeclared #366 edge, oblivious), no annotation is required, otherwise change the affected return to `string?` only.
  - Acceptance: file carries `#nullable enable`; pragma-only rebuild reports zero CS86xx for this file; no new runtime guard added.
- [x] [P1-T10] Run the pragma-only rebuild `msbuild TaskMaster.sln /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU" /p:TreatWarningsAsErrors=true` and record Batch 0 verification to `docs/features/active/utilitiescs-nullable-residuals/evidence/other/batch0-pragma-verify.<timestamp>.md`.
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` confirming zero CS86xx across the nine Batch 0 opted-in files.

### Phase 2 — Batch 1 Small Static COM Helpers

- [x] [P2-T1] Add `#nullable enable` to `UtilitiesCS/OutlookObjects/Calendar/Calendar.cs`; change `FindCalendar` local `Folder foundCalendar = null` to `Folder? foundCalendar = null` and return type to `Folder?`.
  - Acceptance: file carries `#nullable enable`; pragma-only rebuild reports zero CS86xx; no new runtime guard.
- [x] [P2-T2] Add `#nullable enable` to `UtilitiesCS/OutlookObjects/Category/CreateCategory.cs`; change `Category objCategory = null` to `Category? objCategory = null` and the return to `Category?`.
  - Acceptance: file carries `#nullable enable`; pragma-only rebuild reports zero CS86xx; no new runtime guard.
- [x] [P2-T3] Add `#nullable enable` to `UtilitiesCS/OutlookObjects/Com/ComType.cs`; change `GetTypeName` return to `string?` (already `return null`).
  - Acceptance: file carries `#nullable enable`; pragma-only rebuild reports zero CS86xx; public contract change is additive nullability only.
- [x] [P2-T4] Add `#nullable enable` to `UtilitiesCS/OutlookObjects/Explorer/ExplorerActions.cs`; change `GetCurrentItem` and `Readable` returns to `object?`.
  - Acceptance: file carries `#nullable enable`; pragma-only rebuild reports zero CS86xx; no new runtime guard.
- [x] [P2-T5] Add `#nullable enable` to `UtilitiesCS/OutlookObjects/MailResolution.cs` (root file; class `MailResolution_ToRemove`); change `MailItem OlMail = null` to `MailItem? OlMail = null` and the return to `MailItem?`. Do NOT annotate or edit `UtilitiesCS/OutlookObjects/MailItem/MailResolution.cs` (belongs to #371).
  - Acceptance: only the root `MailResolution.cs` carries `#nullable enable`; pragma-only rebuild reports zero CS86xx; the `_ToRemove` type is remediated in place (its deletion-candidate status remains a maintainer flag in `spec.md`, not resolved here).
- [x] [P2-T6] Run the pragma-only rebuild and record Batch 1 verification to `docs/features/active/utilitiescs-nullable-residuals/evidence/other/batch1-pragma-verify.<timestamp>.md`.
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` confirming zero CS86xx across the five Batch 1 files.

### Phase 3 — Batch 2 Outlook Readiness Gate Pair

- [x] [P3-T1] Add `#nullable enable` to `UtilitiesCS/OutlookObjects/IOutlookReadinessGate.cs` and change the interface member to `IsReady(Store? store)` to match the documented "a null store returns false" contract.
  - Acceptance: file carries `#nullable enable`; pragma-only rebuild reports zero CS86xx; signature change is additive nullability only.
- [x] [P3-T2] Add `#nullable enable` to `UtilitiesCS/OutlookObjects/OutlookReadinessGate.cs` and co-annotate `IsReady(Store? store)` to match the interface; preserve the existing `store?.` guard and the `_app ?? throw` non-null invariant unchanged.
  - Acceptance: file carries `#nullable enable`; pragma-only rebuild reports zero CS86xx; no new runtime guard; interface and implementation signatures agree.
- [x] [P3-T3] Run the pragma-only rebuild and record Batch 2 verification to `docs/features/active/utilitiescs-nullable-residuals/evidence/other/batch2-pragma-verify.<timestamp>.md`.
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` confirming zero CS86xx across the interface+impl pair.

### Phase 4 — Batch 3 Recipient Cluster

- [x] [P4-T1] Add `#nullable enable` to `UtilitiesCS/OutlookObjects/Recipient/RecipientInfo.cs`; resolve the parameterless-ctor CS8618 on `_name`/`_address`/`_html` using the #371 ItemInfo/EmailDetails field-nullability pattern (`string?` fields/props, since `Equals`/`GetHashCode` use `?? ""`).
  - Acceptance: file carries `#nullable enable`; pragma-only rebuild reports zero CS86xx; annotation matches the #371 pattern; no behavior change.
- [x] [P4-T2] Add `#nullable enable` to `UtilitiesCS/OutlookObjects/Recipient/RecipientStatic.cs` (773 lines, pre-existing 500-line breach — FLAG, do NOT split); annotate `GetGlobalAddressList` to `AddressList?`, `ExtractNameFromAddress` to `(string?, string?, string?)`, the `AddressEntry` overload of `ToResolvedRecipient` to `Recipient?`, `SegmentStopWatch? sw = null`, and `string? address = null`; treat `IsNullOrEmpty` as non-refining on net481 and use justified `!` where a value is guaranteed non-null.
  - Acceptance: file carries `#nullable enable`; pragma-only rebuild reports zero CS86xx; no split performed; the 500-line breach is recorded as pre-existing in `spec.md` and not worsened in a status-changing way (AC8); no new runtime guard.
- [x] [P4-T3] Run the pragma-only rebuild and record Batch 3 verification to `docs/features/active/utilitiescs-nullable-residuals/evidence/other/batch3-pragma-verify.<timestamp>.md`.
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` confirming zero CS86xx across the Recipient pair.

### Phase 5 — Batch 4 OneDrive Helpers

- [x] [P5-T1] Add `#nullable enable` to `UtilitiesCS/OneDriveHelpers/AngleSharpParsedEmailBody.cs`; annotate `Html` to `string?`, `Links`/`FilteredLinks` to `IEnumerable<(string,string)>?`, `FilterLinksByDomain` return to the corresponding `...?`, and set the setter-assigned `_parser` to `= null!` (or annotate); preserve the `Links ??=` guard.
  - Acceptance: file carries `#nullable enable`; pragma-only rebuild reports zero CS86xx; no new runtime guard.
- [x] [P5-T2] Add `#nullable enable` to `UtilitiesCS/OneDriveHelpers/OneDriveDownloader.cs`; change `TryGetUrlStreamAsync` and `TryGetFileStreamWriter` returns to `Task<Stream?>` (callers already null-check); set `_client`/`_clientGetAsync` to `= null!` (behavior-preserving); add NO null handling around `response.IsSuccessStatusCode` or the returned stream because `TimeOutTask.RunWithTimeout` returns non-null `Task<TResult>` (#369) and `StreamExtensions.TryCopyToAsyncWithTimeout` returns `Task<bool>` (#363); preserve the existing `?.Dispose()`.
  - Acceptance: file carries `#nullable enable`; pragma-only rebuild reports zero CS86xx; annotations are consistent with the pinned #363/#369 return types (AC5); no new runtime guard.
- [x] [P5-T3] Run the pragma-only rebuild and record Batch 4 verification to `docs/features/active/utilitiescs-nullable-residuals/evidence/other/batch4-pragma-verify.<timestamp>.md`.
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` confirming zero CS86xx across the two OneDrive files.

### Phase 6 — Batch 5 EmailIntelligence Data Types

- [x] [P6-T1] Add `#nullable enable` to `UtilitiesCS/EmailIntelligence/FilterEntry.cs`; resolve the 2-arg-ctor CS8618 on `_description` with `private string _description = null!;` (behavior-preserving — keeps the current null) or `string?`; do NOT set `_description = ""` (that would change the runtime value).
  - Acceptance: file carries `#nullable enable`; pragma-only rebuild reports zero CS86xx; runtime value unchanged.
- [x] [P6-T2] Add `#nullable enable` to `UtilitiesCS/EmailIntelligence/IntelligenceConfig.cs`; set `Config = null!` (or `?`), `LastResourceTimingBreakdown` to `string?`, and the filtered KVP value to `null!`; leave `ResourceTimingRow` as the existing plain `readonly struct` (no `record struct`/`init`, avoids CS0518 on net481).
  - Acceptance: file carries `#nullable enable`; pragma-only rebuild reports zero CS86xx; no `record`/`init` introduced.
- [x] [P6-T3] Add `#nullable enable` to `UtilitiesCS/EmailIntelligence/Evaluation/FolderPredictorEvaluator.cs`; change `PredictTop` return to `string?`; because `string.IsNullOrEmpty` does NOT refine null-state on net481, apply justified `!` at the guaranteed-non-null sites (`trueLeaf!` for `leaves.Add`/`Increment`, `example!` for `example.Tokens`) rather than any new guard.
  - Acceptance: file carries `#nullable enable`; pragma-only rebuild reports zero CS86xx; no new runtime guard; `!` used only at guaranteed-non-null sites.
- [x] [P6-T4] Add `#nullable enable` to `UtilitiesCS/EmailIntelligence/People/PeopleScoDictionaryNew.cs`; set `Globals`/`_prefix` to `= null!` (behavior-preserving, preserves the existing deref) and change `AddMissingEntry`/`RefineValidateCategory` returns to `string?`.
  - Acceptance: file carries `#nullable enable`; pragma-only rebuild reports zero CS86xx; no new runtime guard; existing `[ExcludeFromCodeCoverage]` members unchanged.
- [x] [P6-T5] Run the pragma-only rebuild and record Batch 5 verification to `docs/features/active/utilitiescs-nullable-residuals/evidence/other/batch5-pragma-verify.<timestamp>.md`.
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` confirming zero CS86xx across the four Batch 5 files.

### Phase 7 — Batch 6 OlFolderTools FilterOlFolders

- [x] [P7-T1] Add `#nullable enable` to `UtilitiesCS/EmailIntelligence/OlFolderTools/FilterOlFolders/FolderInfoViewer.cs` (hand-written partial half) and annotate only its own declared fields (`?`/`= null!`); do not annotate Designer-declared controls.
  - Acceptance: file carries `#nullable enable`; pragma-only rebuild reports zero CS86xx; no Designer-declared control annotated.
- [x] [P7-T2] Add `#nullable enable` to `UtilitiesCS/EmailIntelligence/OlFolderTools/FilterOlFolders/OSBrowser.cs` (hand-written partial half); annotate own fields (`?`/`= null!`) consistent with the #364 `HelperClasses.FileSystem` contracts it consumes.
  - Acceptance: file carries `#nullable enable`; pragma-only rebuild reports zero CS86xx; no Designer-declared control annotated.
- [x] [P7-T3] Add `#nullable enable` to `UtilitiesCS/EmailIntelligence/OlFolderTools/FilterOlFolders/FilterOlFoldersViewer.cs` (hand-written partial half); annotate `_controller` to `FilterOlFoldersController?` with `_controller!` at the post-`SetController` invariant site, or `= null!`.
  - Acceptance: file carries `#nullable enable`; pragma-only rebuild reports zero CS86xx; no Designer-declared control annotated.
- [x] [P7-T4] Add `#nullable enable` to `UtilitiesCS/EmailIntelligence/OlFolderTools/FilterOlFolders/FilterOlFoldersController.cs` (343 lines, under 500); annotate `_folderTreeView` to `FolderTreeCompatibilityView?` (already null-checked) and `PutCheckedState` to `?` or `= null!`.
  - Acceptance: file carries `#nullable enable`; pragma-only rebuild reports zero CS86xx; existing `if(_folderTreeView==null)`/`?.Dispose()` guards preserved.
- [x] [P7-T5] Verify the four FilterOlFolders `*.Designer.cs` files (`FilterOlFoldersViewer.Designer.cs`, `FolderInfoViewer.Designer.cs`, `OSBrowser.Designer.cs`, `OSFolder.Designer.cs`) receive NO `#nullable enable` pragma and remain oblivious.
  - Acceptance: `git diff` shows no pragma added to any `*.Designer.cs` under `FilterOlFolders/`; these files are unmodified (AC3).
- [x] [P7-T6] Run the pragma-only rebuild and record Batch 6 verification to `docs/features/active/utilitiescs-nullable-residuals/evidence/other/batch6-pragma-verify.<timestamp>.md`.
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` confirming zero CS86xx across the four hand-written FilterOlFolders files while the Designer halves stay oblivious and non-cross-blocking.

### Phase 8 — Batch 7 OlFolderTools FolderRemap

- [ ] [P8-T1] Add `#nullable enable` to `UtilitiesCS/EmailIntelligence/OlFolderTools/FolderRemap/FolderSelector.cs`; set `OlFolderRemap? _selection = null` and change `Selection`/`SelectFolder` to `OlFolderRemap?` (matches controller `is null` checks).
  - Acceptance: file carries `#nullable enable`; pragma-only rebuild reports zero CS86xx; no Designer-declared control annotated.
- [ ] [P8-T2] Add `#nullable enable` to `UtilitiesCS/EmailIntelligence/OlFolderTools/FolderRemap/FolderRemapViewer.cs` (hand-written partial half) and annotate own fields (`?`/`= null!`).
  - Acceptance: file carries `#nullable enable`; pragma-only rebuild reports zero CS86xx; no Designer-declared control annotated.
- [ ] [P8-T3] Add `#nullable enable` to `UtilitiesCS/EmailIntelligence/OlFolderTools/FolderRemap/FolderRemapTree.cs` (264 lines); set `_roots = null!`, annotate `PropertyChanged` events `?`, `_mappedTo` to `OlFolderRemap?`, and the nested `OlFolderRemap` ctor-unset fields to `= null!`; keep `_batchNotifier` initializer.
  - Acceptance: file carries `#nullable enable`; pragma-only rebuild reports zero CS86xx; no new runtime guard.
- [ ] [P8-T4] Add `#nullable enable` to `UtilitiesCS/EmailIntelligence/OlFolderTools/FolderRemap/FolderRemapController.cs` (283 lines); set `_mappings2 = null!` (set via setter in ctor), annotate `PropertyChanged` `?`, and align `SelectFolder` consumption with `OlFolderRemap?`.
  - Acceptance: file carries `#nullable enable`; pragma-only rebuild reports zero CS86xx; existing `is null` checks preserved.
- [ ] [P8-T5] Verify the two FolderRemap `*.Designer.cs` files (`FolderRemapViewer.Designer.cs`, `FolderSelector.Designer.cs`) receive NO `#nullable enable` pragma and remain oblivious.
  - Acceptance: `git diff` shows no pragma added to any `*.Designer.cs` under `FolderRemap/`; these files are unmodified (AC3).
- [ ] [P8-T6] Run the pragma-only rebuild and record Batch 7 verification to `docs/features/active/utilitiescs-nullable-residuals/evidence/other/batch7-pragma-verify.<timestamp>.md`.
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` confirming zero CS86xx across the four hand-written FolderRemap files while the Designer halves stay oblivious.

### Phase 9 — Batch 8 Large COM Helpers (500-Line FLAG Batch)

- [ ] [P9-T1] Add `#nullable enable` to `UtilitiesCS/OutlookObjects/Fields/UserDefinedFields.cs` (722 lines, pre-existing 500-line breach — FLAG, do NOT split); annotate `SafeGetPropertyAccessorValue`/`TryGetProperty`/`GetUdfValue` returns to `object?`, `GetUdfString` to `string?`, `GetUdfValue<T>` to `T?`, and `UserProperty? objProperty`; COM member chains need no `!` (oblivious on net481).
  - Acceptance: file carries `#nullable enable`; pragma-only rebuild reports zero CS86xx; no split; breach recorded pre-existing in `spec.md`, not worsened in a status-changing way (AC8); no new runtime guard.
- [ ] [P9-T2] Add `#nullable enable` to `UtilitiesCS/OutlookObjects/AppointmentItem/MeetingItemHelper.cs` (847 lines, pre-existing 500-line breach — FLAG, do NOT split); annotate `Lazy<...>`-backed getters returning non-null `string`/`T` to `string?`/`T?` (or justified `!`), `PropertyChanged` event `?`, and `_item`/`Sw`/`Lazy<...>` ctor-unset fields; retain the existing inline `#nullable enable/disable` island around `_emailHeader` if still required for zero CS86xx.
  - Acceptance: file carries `#nullable enable`; pragma-only rebuild reports zero CS86xx; no split; breach recorded pre-existing in `spec.md`, not worsened in a status-changing way (AC8); no new runtime guard.
- [ ] [P9-T3] Run the pragma-only rebuild and record Batch 8 verification to `docs/features/active/utilitiescs-nullable-residuals/evidence/other/batch8-pragma-verify.<timestamp>.md`.
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` confirming zero CS86xx across the two large COM helpers.

### Phase 10 — To Depricate Batch

- [ ] [P10-T1] Add `#nullable enable` to `UtilitiesCS/To Depricate/FileIO2.cs`; change `CSV_ReadTxtF` and `CsvRead` returns to `string[]?` and preserve the current NRE behavior with `array1D!` at the two `SplitArrayTo2D`/`.Select` call sites (annotation-only, behavior-preserving). This file is deprecation-marked; remediate annotation-only under this child (deletion is a maintainer decision recorded in `spec.md`, not performed here).
  - Acceptance: file carries `#nullable enable`; pragma-only rebuild reports zero CS86xx; no behavior change.
- [ ] [P10-T2] Add `#nullable enable` to `UtilitiesCS/To Depricate/StringManipulation.cs` (22 lines, expected clean); remediate annotation-only. This file is deprecation-marked; deletion is a maintainer decision recorded in `spec.md`, not performed here.
  - Acceptance: file carries `#nullable enable`; pragma-only rebuild reports zero CS86xx.
- [ ] [P10-T3] Run the pragma-only rebuild and record To Depricate verification to `docs/features/active/utilitiescs-nullable-residuals/evidence/other/to-depricate-pragma-verify.<timestamp>.md`.
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` confirming zero CS86xx across the two To Depricate files.

### Phase 11 — Examples and Dead-Duplicate Handling

- [ ] [P11-T1] Add `#nullable enable` to `UtilitiesCS/Examples/MSDemoConv.cs` and remediate annotation-only (default maintainer-decision behavior): use `Outlook.Folder?` locals for the `... as Outlook.Folder` casts and apply justified `!` at the demo's own `folder.Store`/`.Name` derefs; no behavior change. The exclude/delete alternatives remain a maintainer decision recorded in `spec.md`.
  - Acceptance: file carries `#nullable enable`; pragma-only rebuild reports zero CS86xx (resolving the CS8600/CS8602 sites); no new runtime guard.
- [ ] [P11-T2] Confirm `UtilitiesCS/EmailIntelligence/People/PeopleScoDictionaryNewBackup.cs` receives NO `#nullable enable` pragma (dead, uncompiled duplicate — a pragma would be a no-op that cannot emit CS86xx) and is not in the `UtilitiesCS.csproj` `<Compile Include>` set.
  - Acceptance: `git diff` shows the backup file unmodified; a grep of `UtilitiesCS/UtilitiesCS.csproj` confirms only the live `PeopleScoDictionaryNew.cs` is compiled; effective compiled hand-written opt-in set is 37 files (the exclude/delete decision remains a maintainer flag in `spec.md`).
- [ ] [P11-T3] Run the pragma-only rebuild and record Examples/dead-duplicate verification to `docs/features/active/utilitiescs-nullable-residuals/evidence/other/examples-pragma-verify.<timestamp>.md`.
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` confirming zero CS86xx for `MSDemoConv.cs` and that the backup file emitted no diagnostics (not compiled).

### Phase 12 — Final QC and Acceptance-Criteria Mapping

- [ ] [P12-T1] Run `dotnet tool run csharpier .` (or `csharpier .`) and record the result to `docs/features/active/utilitiescs-nullable-residuals/evidence/qa-gates/qc-format-csharpier.<timestamp>.md`.
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`. If csharpier changes any file, restart the loop from P12-T1 after the change is verified.
- [ ] [P12-T2] Run the analyzer build `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` and record the result to `docs/features/active/utilitiescs-nullable-residuals/evidence/qa-gates/qc-analyzers.<timestamp>.md`.
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` with zero analyzer/code-style errors. If this step changes files, restart from P12-T1.
- [ ] [P12-T3] Run the pragma-only nullable/type-check gate `msbuild TaskMaster.sln /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU" /p:TreatWarningsAsErrors=true` (NO `/p:Nullable=enable`) and record the result to `docs/features/active/utilitiescs-nullable-residuals/evidence/qa-gates/qc-nullable-pragma-gate.<timestamp>.md`.
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` confirming zero CS86xx across all 37 opted-in files with `TreatWarningsAsErrors=true` (AC1).
- [ ] [P12-T4] Run `UtilitiesCS.Test` with coverage via `scripts/vscode/Invoke-MSTestWithCoverage.ps1` (or `vstest.console.exe <resolved UtilitiesCS.Test.dll> /EnableCodeCoverage`) and record the result to `docs/features/active/utilitiescs-nullable-residuals/evidence/qa-gates/qc-tests-coverage.<timestamp>.md`.
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` with numeric passed/failed counts and numeric post-change coverage (line-rate/branch-rate). If this step changes files, restart from P12-T1.
- [ ] [P12-T5] Compare baseline vs post-change coverage and record the delta to `docs/features/active/utilitiescs-nullable-residuals/evidence/qa-gates/qc-coverage-delta.<timestamp>.md`, reporting baseline coverage (from P0-T3), post-change coverage (from P12-T4), and changed-line coverage.
  - Acceptance: artifact contains `Timestamp:`, `Command:` (or method), `EXIT_CODE:`, `Output Summary:` showing no test regressions and no coverage regression on changed lines (AC6). Annotation-only edits are expected to be coverage-neutral because no new executable lines are introduced (`?`/`= null!`/`!` add no runtime branches). The no-regression-on-changed-lines requirement is uniform across CLAUDE.md and `.claude/rules/general-unit-test.md`; the 80/90 vs 85/75 threshold-source difference is a pre-existing repository conflict flagged to the maintainer at the epic level and is not resolved by this child.
- [ ] [P12-T6] Verify no in-scope file exceeds 500 lines as a result of edits and record the line-count check to `docs/features/active/utilitiescs-nullable-residuals/evidence/qa-gates/qc-line-count.<timestamp>.md`.
  - Acceptance: artifact lists post-edit line counts for the 37 opted-in files; only the three pre-existing breaches (`MeetingItemHelper.cs`, `RecipientStatic.cs`, `UserDefinedFields.cs`) exceed 500 and are flagged not split; no other file newly crosses 500 (AC8).
- [ ] [P12-T7] Verify `UtilitiesCS/UtilitiesCS.csproj` (and the solution) has no `<Nullable>` element introduced and no `/p:Nullable=enable` used in any verification command; record to `docs/features/active/utilitiescs-nullable-residuals/evidence/qa-gates/qc-no-project-nullable.<timestamp>.md`.
  - Acceptance: artifact shows a grep of `UtilitiesCS.csproj` with no `<Nullable>` element and confirms the pragma-only command was used (AC2).
- [ ] [P12-T8] Verify the 6 `*.Designer.cs` files under `OlFolderTools` (`FilterOlFoldersViewer.Designer.cs`, `FolderInfoViewer.Designer.cs`, `OSBrowser.Designer.cs`, `OSFolder.Designer.cs`, `FolderRemapViewer.Designer.cs`, `FolderSelector.Designer.cs`) carry no pragma and are unmodified; record to `docs/features/active/utilitiescs-nullable-residuals/evidence/qa-gates/qc-designer-oblivious.<timestamp>.md`.
  - Acceptance: artifact confirms all six Designer files are pragma-free and unmodified and were not cross-blocked (AC3).
- [ ] [P12-T9] Verify the six Maintainer Decisions and Flags are recorded in `spec.md` (dead-duplicate exclusion, `MSDemoConv.cs` decision, deprecation-marked `To Depricate/*`, `MailResolution_ToRemove`, the undeclared `ReusableTypeClasses` #366 edge, and the three 500-line breaches) and record to `docs/features/active/utilitiescs-nullable-residuals/evidence/qa-gates/qc-maintainer-flags.<timestamp>.md`.
  - Acceptance: artifact cites the `spec.md` "Maintainer Decisions and Flags" section location for each of the six items; none is silently resolved by the code changes (AC7).
- [ ] [P12-T10] Produce the acceptance-criteria mapping artifact `docs/features/active/utilitiescs-nullable-residuals/evidence/qa-gates/ac-mapping.<timestamp>.md` mapping AC1–AC8 to the evidence that satisfies each.
  - Acceptance: artifact maps AC1→P12-T3; AC2→P12-T7; AC3→P7-T5/P8-T5/P12-T8; AC4→per-batch "no new runtime guard" tasks and P12-T3; AC5→P5-T2 and P12-T3; AC6→P0-T3/P12-T4/P12-T5; AC7→P12-T9; AC8→P4-T2/P9-T1/P9-T2/P12-T6; each mapping names the concrete artifact path.

## Toolchain and Loop Rules

- Per-batch verification during Phases 1–11 is the compile-time pragma-only rebuild (`/t:Rebuild ... /p:TreatWarningsAsErrors=true`, no `/p:Nullable=enable`); it confirms each batch's opted-in files reach zero CS86xx without cross-blocking non-opted-in files.
- Final QC (Phase 12) runs the full C# toolchain in CLAUDE.md order: (1) csharpier, (2) analyzer build, (3) pragma-only nullable/type-check gate, (4) `vstest.console.exe` with coverage. If any final-QC step fails or changes files, restart the loop from P12-T1.
- Every command-bearing task records `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`; test steps record numeric coverage. No planned command task may be marked `SKIPPED`.

## Open Questions / Notes

- The pragma-only verification command is a deliberate, documented deviation from the stock `.claude/rules/csharp.md` type-check command for this child only; it must NOT be resolved by editing `.claude/rules/*` (epic-flagged, deferred to the Wave-2 CI capstone).
- The undeclared `ReusableTypeClasses` (#366) edge is harmless for ordering (Wave 0 precedes Wave 1) and is flagged for the epic-planner, not resolved here.
