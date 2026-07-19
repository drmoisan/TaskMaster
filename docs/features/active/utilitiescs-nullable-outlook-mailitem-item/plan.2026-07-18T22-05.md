# utilitiescs-nullable-outlook-mailitem-item — Atomic Implementation Plan

- **Issue:** #371
- **Parent:** Epic `utilitiescs-nullable-remediation` (child, Wave 1)
- **Owner:** drmoisan
- **Last Updated:** 2026-07-18T22-05
- **Status:** Draft
- **Version:** 1.0
- **Work Mode:** full-feature

## Requirements Sources (read all in Phase 0)

- `docs/features/active/utilitiescs-nullable-outlook-mailitem-item/spec.md` (Definition of Done + AC1–AC6 — AC source)
- `docs/features/active/utilitiescs-nullable-outlook-mailitem-item/user-story.md` (Acceptance Criteria — AC source)
- `docs/features/active/utilitiescs-nullable-outlook-mailitem-item/issue.md`
- `docs/features/active/utilitiescs-nullable-outlook-mailitem-item/research/research.2026-07-18T22-15.md`

Policy compliance is governed by `CLAUDE.md`, `.claude/rules/general-code-change.md`,
`.claude/rules/general-unit-test.md`, and `.claude/rules/csharp.md`. Do not duplicate their content
here; comply with them.

## Hard Constraints (encoded, non-negotiable)

- Per-file `#nullable enable` pragma on each remediated file under
  `UtilitiesCS/OutlookObjects/{MailItem,Item,Conversation,Attachment,Table}/`; bring each opted-in
  file to ZERO CS86xx under the pragma. `UtilitiesCS.csproj` keeps NO `<Nullable>` element.
- Annotation and null-safety ONLY: `?` annotations, `T?`/`out` unconstrained-generic decisions,
  null-flow corrections, and `!` only where justified. NO behavior change, NO refactor, NO API
  redesign, NO feature work. Existing null guards stay as-is; no new runtime guards are added
  solely to satisfy the gate.
- Target framework net481, C# 12: no `System.Diagnostics.CodeAnalysis` nullable post-condition
  attributes (`[NotNullWhen]`, `[MaybeNullWhen]`, `[NotNullIfNotNull]`, `[MaybeNull]`, `[AllowNull]`,
  `[DisallowNull]`, `[DoesNotReturn]`, `[MemberNotNull]`); no `init`/positional `record`/`record
  struct`.
- Three partial-class groups must be opted in together, each as one unit: `MailItemHelper` (5
  files), `ConvHelper` (2 files: `ConversationHelper.cs`, `ConversationHelper.Formatting.cs`),
  `OlTableExtensions` (4 files).
- COM/VSTO coverage exemption applies to all 30 in-scope files except `CidImageResolver.cs`:
  annotate COM-bound files for null-safety only; do NOT add new tests around COM-bound,
  non-seamed code paths. Preserve the `EmailDetailsWrapper`/`IEmailDetailsWrapper` and
  `OutlookItemTry`/`OutlookItemTryGet`/`OutlookItemFlaggableTry` seams exactly as-is.
- Pre-existing conditions are flagged, not fixed: `OutlookItem.cs` (503 lines, over the 500-line
  limit — do not split); `dynamic item` in `OlToDoTable.EnsureItemValues` (invisible to nullable
  analysis — do not convert); `CaptureEmailAddressesModule2.cs`/`ItemComparer.cs` (dead files,
  no-op pragma). `MailItemHelper.Html.cs`'s interior `#nullable enable`/`disable` region IS in-scope
  remediation work (normalize to a whole-file pragma), not a flag-only item.
- Batch order (dependency-graph, from research Section 6), each batch independently verified before
  the next begins: A (dead-code) → B (`CidImageResolver.cs`) → C (small COM-bound leaves) → D
  (`OutlookItem` family) → E (Attachment, needs #364 `FilePathHelper`) → F (`ItemInfo`/
  `EmailDetails`) → G (`MailItemHelper` group, needs D/E/F + #363 `LazyExtension` + #364
  `Initializer.GetOrLoad`) → H (`ConvHelper` group, needs #363 `IEnumerableExtensions.ForEach` +
  #364 `PrettyPrint.PrettyText`) → I (`OlTableExtensions` group, needs #363
  `ArrayExtensions.ToStringArray`/`To2D` + Batch H's `ConvHelper`).

## CRITICAL Toolchain Deviation (applies to every nullable/type-check task in this plan)

The nullable / type-check verification step MUST use the pragma-only build and MUST NOT add
`/p:Nullable=enable`:

`msbuild TaskMaster.sln /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU" /p:TreatWarningsAsErrors=true`

Rationale: adding `/p:Nullable=enable` turns nullable ON project-wide and surfaces the entire
epic's pre-existing debt across the solution as false failures unrelated to issue #371.
Enforcement for this child is per-file pragma only. This is a deliberate, documented deviation from
the stock `CLAUDE.md` / `.claude/rules/csharp.md` type-check command, for THIS child only. It MUST
NOT be resolved by editing `.claude/rules/*`. The remaining toolchain stages are standard:

- Format: `dotnet tool run csharpier .` (or `csharpier .`)
- Analyzers / codestyle: `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
- Type-check (nullable, pragma-only): the `/t:Rebuild ... /p:TreatWarningsAsErrors=true` command above (NO `/p:Nullable=enable`)
- Test + coverage: `vstest.console.exe <UtilitiesCS.Test assembly> /EnableCodeCoverage` (repo-canonical full-suite driver: `scripts/vscode/Invoke-MSTestWithCoverage.ps1`, which wraps `vstest.console.exe` with coverage and emits Cobertura XML)

## Evidence Path Scheme (non-overridable)

All evidence artifacts resolve under
`docs/features/active/utilitiescs-nullable-outlook-mailitem-item/evidence/<kind>/` with kinds
`baseline`, `regression-testing`, `qa-gates`, `other`. Timestamps use `yyyy-MM-ddTHH-mm`. No
`artifacts/...` evidence path is used. The delegation prompt supplied only canonical `evidence/`
kinds, so no `EVIDENCE_LOCATION_OVERRIDE_REJECTED` substitution is required.

---

### Phase 0 — Policy Reads and Baseline Capture

- [x] [P0-T1] Read the policy and requirements files in order and emit a policy-read evidence artifact to `docs/features/active/utilitiescs-nullable-outlook-mailitem-item/evidence/baseline/phase0-instructions-read.<yyyy-MM-ddTHH-mm>.md`
  - Read order: `CLAUDE.md`, `.claude/rules/general-code-change.md`, `.claude/rules/general-unit-test.md`, `.claude/rules/csharp.md`, then `docs/features/active/utilitiescs-nullable-outlook-mailitem-item/spec.md`, `user-story.md`, `issue.md`, and `research/research.2026-07-18T22-15.md`.
  - Acceptance: artifact contains `Timestamp:`, `Policy Order:`, and an explicit list of every file read.
- [x] [P0-T2] Run the CSharpier format check baseline and record it to `docs/features/active/utilitiescs-nullable-outlook-mailitem-item/evidence/baseline/csharpier-baseline.<yyyy-MM-ddTHH-mm>.md`
  - Command: `dotnet tool run csharpier --check .`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` (pass/fail and any unformatted-file count).
- [x] [P0-T3] Run the analyzer/codestyle build baseline and record it to `docs/features/active/utilitiescs-nullable-outlook-mailitem-item/evidence/baseline/analyzer-build-baseline.<yyyy-MM-ddTHH-mm>.md`
  - Command: `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` (build result and analyzer warning/error counts).
- [x] [P0-T4] Run the pragma-only nullable build baseline and record the pre-remediation CS86xx count for the 30 in-scope files to `docs/features/active/utilitiescs-nullable-outlook-mailitem-item/evidence/baseline/nullable-build-baseline.<yyyy-MM-ddTHH-mm>.md`
  - Command: `msbuild TaskMaster.sln /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU" /p:TreatWarningsAsErrors=true`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` recording the exact pre-remediation CS86xx count for `UtilitiesCS/OutlookObjects/{MailItem,Item,Conversation,Attachment,Table}/` files (accounting for `MailItemHelper.Html.cs`'s existing interior pragma region, which may already emit diagnostics under `TreatWarningsAsErrors`), and confirming NO `/p:Nullable=enable` was passed.
- [x] [P0-T5] Run the coverage baseline over the UtilitiesCS test assemblies, targeting the `UtilitiesCS.Test/OutlookObjects/` suite, and record numeric coverage to `docs/features/active/utilitiescs-nullable-outlook-mailitem-item/evidence/baseline/coverage-baseline.<yyyy-MM-ddTHH-mm>.md` with the Cobertura XML at `docs/features/active/utilitiescs-nullable-outlook-mailitem-item/evidence/baseline/coverage-baseline.<yyyy-MM-ddTHH-mm>.cobertura.xml`
  - Command: `pwsh scripts/vscode/Invoke-MSTestWithCoverage.ps1 -CoverageOutput docs/features/active/utilitiescs-nullable-outlook-mailitem-item/evidence/baseline/coverage-baseline.<yyyy-MM-ddTHH-mm>.cobertura.xml` (full-suite driver wrapping `vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /EnableCodeCoverage`).
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` with the NUMERIC baseline overall `line-rate`/`branch-rate` from the Cobertura root `<coverage>` element AND the targeted `UtilitiesCS/OutlookObjects/` line percentage if obtainable from per-package figures; passed/failed test counts recorded.

### Phase 1 — Batch A Dead-Code Confirm-Clean

- [x] [P1-T1] Add `#nullable enable` to `UtilitiesCS/OutlookObjects/MailItem/CaptureEmailAddressesModule2.cs` (entire body already commented out; no live-code changes possible)
  - Acceptance: file carries the pragma as its sole change; zero CS86xx (verified in P1-T4).
- [x] [P1-T2] Add `#nullable enable` to `UtilitiesCS/OutlookObjects/Item/ItemComparer.cs` (entire body already commented out; no live-code changes possible)
  - Acceptance: file carries the pragma as its sole change; zero CS86xx (verified in P1-T4).
- [x] [P1-T3] Run CSharpier over the Batch A files (`MailItem/CaptureEmailAddressesModule2.cs`, `Item/ItemComparer.cs`) with `dotnet tool run csharpier .` and confirm no residual formatting diff
  - Acceptance: `csharpier --check .` exits 0 for the touched files.
- [x] [P1-T4] Run the pragma-only nullable build and record Batch A verification to `docs/features/active/utilitiescs-nullable-outlook-mailitem-item/evidence/qa-gates/batch-a-nullable-build.<yyyy-MM-ddTHH-mm>.md`
  - Command: `msbuild TaskMaster.sln /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU" /p:TreatWarningsAsErrors=true`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` confirming zero CS86xx for the 2 opted-in Batch A files and NO new diagnostics elsewhere.
- [x] [P1-T5] Run the Batch A UtilitiesCS tests and record results to `docs/features/active/utilitiescs-nullable-outlook-mailitem-item/evidence/regression-testing/batch-a-tests.<yyyy-MM-ddTHH-mm>.md`
  - Command: `vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /EnableCodeCoverage /TestCaseFilter:"FullyQualifiedName~CaptureEmailAddressesModule2"`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` with passed/failed counts; `CaptureEmailAddressesModule2Tests.cs` green and behavior-identical (no test exists for `ItemComparer.cs`, consistent with it being dead code).

### Phase 2 — Batch B Pure Host-Neutral Leaf

- [x] [P2-T1] Add `#nullable enable` to `UtilitiesCS/OutlookObjects/MailItem/CidImageResolver.cs` and apply annotation-only null-safety edits to reach zero CS86xx
  - Acceptance: file carries the pragma; annotation-only; zero CS86xx (verified in P2-T3). This file is NOT COM-bound and is held to normal, non-exempt coverage expectations.
- [x] [P2-T2] Run CSharpier over `UtilitiesCS/OutlookObjects/MailItem/CidImageResolver.cs` with `dotnet tool run csharpier .` and confirm no residual formatting diff
  - Acceptance: `csharpier --check .` exits 0 for the touched file.
- [x] [P2-T3] Run the pragma-only nullable build and record Batch B verification to `docs/features/active/utilitiescs-nullable-outlook-mailitem-item/evidence/qa-gates/batch-b-nullable-build.<yyyy-MM-ddTHH-mm>.md`
  - Command: `msbuild TaskMaster.sln /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU" /p:TreatWarningsAsErrors=true`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` confirming zero CS86xx for `CidImageResolver.cs` and NO new diagnostics elsewhere.
- [x] [P2-T4] Run the `CidImageResolverTests.cs` suite with coverage (non-exempt file) and record results to `docs/features/active/utilitiescs-nullable-outlook-mailitem-item/evidence/regression-testing/batch-b-tests.<yyyy-MM-ddTHH-mm>.md`
  - Command: `vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /EnableCodeCoverage /TestCaseFilter:"FullyQualifiedName~CidImageResolver"`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` with passed/failed counts AND numeric coverage for `CidImageResolver.cs`; no coverage regression on changed lines.

### Phase 3 — Batch C Small COM-Bound Leaves

- [ ] [P3-T1] Add `#nullable enable` to `UtilitiesCS/OutlookObjects/MailItem/MailResolution.cs` and apply annotation-only null-safety edits to reach zero CS86xx
  - Acceptance: file carries the pragma; annotation-only; zero CS86xx (verified in P3-T7).
- [ ] [P3-T2] Add `#nullable enable` to `UtilitiesCS/OutlookObjects/MailItem/MailItemExtensions.cs` and apply annotation-only null-safety edits to reach zero CS86xx
  - Acceptance: file carries the pragma; annotation-only; zero CS86xx (verified in P3-T7).
- [ ] [P3-T3] Add `#nullable enable` to `UtilitiesCS/OutlookObjects/Item/OlItemPseudoInterface.cs` and apply annotation-only null-safety edits to reach zero CS86xx
  - Acceptance: file carries the pragma; annotation-only; zero CS86xx (verified in P3-T7).
- [ ] [P3-T4] Add `#nullable enable` to `UtilitiesCS/OutlookObjects/Item/OlItemSummary.cs` and apply annotation-only null-safety edits to reach zero CS86xx
  - Acceptance: file carries the pragma; annotation-only; zero CS86xx (verified in P3-T7).
- [ ] [P3-T5] Add `#nullable enable` to `UtilitiesCS/OutlookObjects/Table/OlToDoTable.cs` and apply annotation-only null-safety edits to reach zero CS86xx, leaving the `dynamic item = itemObj;` line in `EnsureItemValues` unconverted
  - Acceptance: file carries the pragma; annotation-only; the `dynamic item` line is byte-unchanged; zero CS86xx (verified in P3-T7).
- [ ] [P3-T6] Record the `dynamic item` nullable-flow-analysis hazard flag to `docs/features/active/utilitiescs-nullable-outlook-mailitem-item/evidence/other/maintainer-flags.<yyyy-MM-ddTHH-mm>.md`
  - Acceptance: artifact records that `dynamic item = itemObj;` in `OlToDoTable.EnsureItemValues` is invisible to nullable-flow analysis and was flagged, not fixed (converting to a typed access pattern would be a behavior-risk refactor, out of scope), with `Timestamp:`.
- [ ] [P3-T7] Run CSharpier over the Batch C files (`MailItem/MailResolution.cs`, `MailItem/MailItemExtensions.cs`, `Item/OlItemPseudoInterface.cs`, `Item/OlItemSummary.cs`, `Table/OlToDoTable.cs`) and confirm no residual formatting diff
  - Acceptance: `csharpier --check .` exits 0 for the touched files.
- [ ] [P3-T8] Run the pragma-only nullable build and record Batch C verification to `docs/features/active/utilitiescs-nullable-outlook-mailitem-item/evidence/qa-gates/batch-c-nullable-build.<yyyy-MM-ddTHH-mm>.md`
  - Command: `msbuild TaskMaster.sln /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU" /p:TreatWarningsAsErrors=true`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` confirming zero CS86xx for the 5 opted-in Batch C files and NO new diagnostics elsewhere.
- [ ] [P3-T9] Run the Batch C UtilitiesCS tests and record results to `docs/features/active/utilitiescs-nullable-outlook-mailitem-item/evidence/regression-testing/batch-c-tests.<yyyy-MM-ddTHH-mm>.md`
  - Command: `vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /EnableCodeCoverage /TestCaseFilter:"FullyQualifiedName~MailResolution|FullyQualifiedName~MailItemExtensions|FullyQualifiedName~OlItemPseudoInterface|FullyQualifiedName~OlItemSummary|FullyQualifiedName~OlToDoTable"`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` with passed/failed counts; `OlItemPseudoInterfaceTests.cs`/`OlItemPseudoInterface_Tests.cs`, `OlItemSummaryTests.cs`, `MailResolutionTests.cs`, `OlToDoTableTests.cs`/`OlToDoTable_Tests.cs` all green and behavior-identical.

### Phase 4 — Batch D OutlookItem Reflection-Wrapper Family

- [ ] [P4-T1] Add `#nullable enable` to `UtilitiesCS/OutlookObjects/Item/OutlookItem.cs` and make the deliberate `GetPropertyValue<T>`/`SetPropertyValue<T>`/`CallMethod` unconstrained-generic nullable-contract decisions to reach zero CS86xx, without splitting the file
  - Acceptance: file carries the pragma; annotation-only; file remains 503+ lines (not split); generic return-nullability decisions recorded as deliberate contracts; zero CS86xx (verified in P4-T9).
- [ ] [P4-T2] Record the `OutlookItem.cs` 500-line pre-existing file-size breach flag to `docs/features/active/utilitiescs-nullable-outlook-mailitem-item/evidence/other/maintainer-flags.<yyyy-MM-ddTHH-mm>.md`
  - Acceptance: artifact records that `OutlookItem.cs` is a pre-existing 500+-line breach, that annotation-only work pushes it further over 500 rather than under it, and that splitting is explicitly out of scope for this remediation (flagged for a future issue), with `Timestamp:`.
- [ ] [P4-T3] Add `#nullable enable` to `UtilitiesCS/OutlookObjects/Item/OutlookItemExtensions.cs` and settle the shared `TryGetPropertyValue`/`TrySetPropertyValue`/`TryCallMethod` reflection-helper nullable contracts consistently with `OutlookItem.cs`'s Batch D decisions to reach zero CS86xx
  - Acceptance: file carries the pragma; annotation-only; zero CS86xx (verified in P4-T9).
- [ ] [P4-T4] Add `#nullable enable` to `UtilitiesCS/OutlookObjects/Item/OutlookItemFlaggable.cs` and apply annotation-only null-safety edits consistent with the `OutlookItem.cs` base-class contract to reach zero CS86xx
  - Acceptance: file carries the pragma; annotation-only; zero CS86xx (verified in P4-T9).
- [ ] [P4-T5] Add `#nullable enable` to `UtilitiesCS/OutlookObjects/Item/OutlookItemTry.cs` and annotate the internal `TryGet<T>`/`TrySet<T>`/`TryCall<T>` generic helpers' `default(T)` returns consistently with the Batch D base-class decisions to reach zero CS86xx
  - Acceptance: file carries the pragma; annotation-only; the try/catch-swallowing decorator seam over `IOutlookItem` is preserved exactly; zero CS86xx (verified in P4-T9).
- [ ] [P4-T6] Add `#nullable enable` to `UtilitiesCS/OutlookObjects/Item/OutlookItemTryGet.cs` and annotate the internal `TryGet<T>(Func<T>, out T)` helpers to reach zero CS86xx
  - Acceptance: file carries the pragma; annotation-only; zero CS86xx (verified in P4-T9).
- [ ] [P4-T7] Add `#nullable enable` to `UtilitiesCS/OutlookObjects/Item/OutlookItemFlaggableTry.cs` and apply annotation-only null-safety edits consistent with the `OutlookItemTry.cs` decorator contract to reach zero CS86xx
  - Acceptance: file carries the pragma; annotation-only; the `IOutlookItemFlaggable` try/catch-swallowing decorator seam is preserved exactly; zero CS86xx (verified in P4-T9).
- [ ] [P4-T8] Run CSharpier over the Batch D files (`Item/OutlookItem.cs`, `Item/OutlookItemExtensions.cs`, `Item/OutlookItemFlaggable.cs`, `Item/OutlookItemTry.cs`, `Item/OutlookItemTryGet.cs`, `Item/OutlookItemFlaggableTry.cs`) and confirm no residual formatting diff
  - Acceptance: `csharpier --check .` exits 0 for the touched files.
- [ ] [P4-T9] Run the pragma-only nullable build and record Batch D verification to `docs/features/active/utilitiescs-nullable-outlook-mailitem-item/evidence/qa-gates/batch-d-nullable-build.<yyyy-MM-ddTHH-mm>.md`
  - Command: `msbuild TaskMaster.sln /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU" /p:TreatWarningsAsErrors=true`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` confirming zero CS86xx for the 6 opted-in Batch D files (consistent unconstrained-generic contract across the family) and NO new diagnostics elsewhere.
- [ ] [P4-T10] Run the Batch D UtilitiesCS tests and record results to `docs/features/active/utilitiescs-nullable-outlook-mailitem-item/evidence/regression-testing/batch-d-tests.<yyyy-MM-ddTHH-mm>.md`
  - Command: `vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /EnableCodeCoverage /TestCaseFilter:"FullyQualifiedName~OutlookItem"`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` with passed/failed counts; `OutlookItemTests.cs`/`OutlookItem_Tests.cs`, `OutlookItemExtensionsTests.cs`/`OutlookItemExtensions_Tests.cs`, `OutlookItemFlaggableTests.cs`/`OutlookItemFlaggable_Tests.cs`, `OutlookItemFlaggableTryTests.cs`/`OutlookItemFlaggableTry_Tests.cs`, `OutlookItemTryTests.cs`/`OutlookItemTry_Tests.cs`, `OutlookItemTryGetTests.cs`/`OutlookItemTryGet_Tests.cs` all green and behavior-identical; no new tests added around non-seamed COM-bound reflection paths.

### Phase 5 — Batch E Attachment Cluster

- [ ] [P5-T1] Verify the upstream #364 `FilePathHelper` contract (non-nullable `""`-default `FilePath`/`FolderPath`/`FileName` properties) has landed in `UtilitiesCS/HelperClasses/FileSystem/FilePathHelper.cs` before annotating this batch
  - Command: `grep -n "FilePath\|FolderPath\|FileName" UtilitiesCS/HelperClasses/FileSystem/FilePathHelper.cs`
  - Acceptance: grep output confirms the property declarations exist with the expected non-nullable, default-`""` shape; if the contract has not landed, this task is BLOCKED and must be re-run before proceeding to P5-T2/P5-T3.
- [ ] [P5-T2] Add `#nullable enable` to `UtilitiesCS/OutlookObjects/Attachment/AttachmentSerializable.cs` and annotate the lazy byte-fetching members (`GetBytes`, `TryFromAccessor`, `TryFromSaveAsLoad`, `TryFromContentIdAccessor`) to reach zero CS86xx
  - Acceptance: file carries the pragma; annotation-only; zero CS86xx (verified in P5-T5).
- [ ] [P5-T3] Add `#nullable enable` to `UtilitiesCS/OutlookObjects/Attachment/AttachmentHelper.cs` and forward `FilePathSave`/`FolderPathSave` to the `FilePathHelperSave.FilePath`/`.FolderPath` contract without adding a conflicting nullable annotation, to reach zero CS86xx
  - Acceptance: file carries the pragma; annotation-only; `FilePathSave`/`FolderPathSave` inherit the non-nullable `""`-default contract as-is; zero CS86xx (verified in P5-T5).
- [ ] [P5-T4] Run CSharpier over the Batch E files (`Attachment/AttachmentSerializable.cs`, `Attachment/AttachmentHelper.cs`) and confirm no residual formatting diff
  - Acceptance: `csharpier --check .` exits 0 for the touched files.
- [ ] [P5-T5] Run the pragma-only nullable build and record Batch E verification to `docs/features/active/utilitiescs-nullable-outlook-mailitem-item/evidence/qa-gates/batch-e-nullable-build.<yyyy-MM-ddTHH-mm>.md`
  - Command: `msbuild TaskMaster.sln /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU" /p:TreatWarningsAsErrors=true`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` confirming zero CS86xx for the 2 opted-in Batch E files and NO new diagnostics elsewhere.
- [ ] [P5-T6] Run the Batch E UtilitiesCS tests and record results to `docs/features/active/utilitiescs-nullable-outlook-mailitem-item/evidence/regression-testing/batch-e-tests.<yyyy-MM-ddTHH-mm>.md`
  - Command: `vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /EnableCodeCoverage /TestCaseFilter:"FullyQualifiedName~AttachmentSerializable|FullyQualifiedName~AttachmentHelper"`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` with passed/failed counts; `AttachmentHelperTests.cs`, `AttachmentSerializableTests.cs`, and legacy `AttachmentSerializable_Tests.cs` all green and behavior-identical.

### Phase 6 — Batch F ItemInfo and EmailDetails

- [ ] [P6-T1] Add `#nullable enable` to `UtilitiesCS/OutlookObjects/MailItem/ItemInfo.cs` and apply annotation-only null-safety edits to reach zero CS86xx
  - Acceptance: file carries the pragma; annotation-only; zero CS86xx (verified in P6-T5).
- [ ] [P6-T2] Add `#nullable enable` to `UtilitiesCS/OutlookObjects/MailItem/EmailDetails.cs` and apply annotation-only null-safety edits to reach zero CS86xx
  - Acceptance: file carries the pragma; annotation-only; zero CS86xx (verified in P6-T5).
- [ ] [P6-T3] Add `#nullable enable` to `UtilitiesCS/OutlookObjects/MailItem/EmailDetailsWrapper.cs` and mirror `EmailDetails.cs`'s Batch F nullable decisions in the thin `IEmailDetailsWrapper` delegator to reach zero CS86xx
  - Acceptance: file carries the pragma; annotation-only; the `IEmailDetailsWrapper` seam over the static `EmailDetails` extension methods is preserved exactly; zero CS86xx (verified in P6-T5).
- [ ] [P6-T4] Run CSharpier over the Batch F files (`MailItem/ItemInfo.cs`, `MailItem/EmailDetails.cs`, `MailItem/EmailDetailsWrapper.cs`) and confirm no residual formatting diff
  - Acceptance: `csharpier --check .` exits 0 for the touched files.
- [ ] [P6-T5] Run the pragma-only nullable build and record Batch F verification to `docs/features/active/utilitiescs-nullable-outlook-mailitem-item/evidence/qa-gates/batch-f-nullable-build.<yyyy-MM-ddTHH-mm>.md`
  - Command: `msbuild TaskMaster.sln /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU" /p:TreatWarningsAsErrors=true`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` confirming zero CS86xx for the 3 opted-in Batch F files and NO new diagnostics elsewhere.
- [ ] [P6-T6] Run the Batch F UtilitiesCS tests and record results to `docs/features/active/utilitiescs-nullable-outlook-mailitem-item/evidence/regression-testing/batch-f-tests.<yyyy-MM-ddTHH-mm>.md`
  - Command: `vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /EnableCodeCoverage /TestCaseFilter:"FullyQualifiedName~ItemInfo|FullyQualifiedName~EmailDetails"`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` with passed/failed counts; `ItemInfoTests.cs`, legacy `ItemInfo_Tests.cs`, `EmailDetailsTests.cs`, `EmailDetailsWrapperTests.cs` all green and behavior-identical.

### Phase 7 — Batch G MailItemHelper Partial-Class Group

- [ ] [P7-T1] Verify the upstream #363 `LazyExtension` contract (`.ToLazy()`/`.ToLazyValue()`/`.ToLazyTry()`) has landed in `UtilitiesCS/Extensions/LazyExtension.cs` before annotating this batch
  - Command: `grep -n "ToLazy\|ToLazyValue\|ToLazyTry" UtilitiesCS/Extensions/LazyExtension.cs`
  - Acceptance: grep output confirms all three extension methods exist; if not landed, this task is BLOCKED and must be re-run before proceeding to P7-T3..P7-T7.
- [ ] [P7-T2] Verify the upstream #364 `Initializer.GetOrLoad` contract has landed in `UtilitiesCS/HelperClasses/Initializer.cs` before annotating this batch
  - Command: `grep -n "GetOrLoad" UtilitiesCS/HelperClasses/Initializer.cs`
  - Acceptance: grep output confirms the method exists with its `ref T`/nullable-return contract; if not landed, this task is BLOCKED and must be re-run before proceeding to P7-T5.
- [ ] [P7-T3] Add `#nullable enable` to `UtilitiesCS/OutlookObjects/MailItem/MailItemHelper.cs` and annotate the `InitLazyFields`/`InitializeSafeDefaults` ctor wiring and `INotifyPropertyChanged` implementation to reach zero CS86xx
  - Acceptance: file carries the pragma; annotation-only; zero CS86xx (verified as one unit with the rest of the group in P7-T9).
- [ ] [P7-T4] Add `#nullable enable` to `UtilitiesCS/OutlookObjects/MailItem/MailItemHelper.Html.cs`, normalize the existing interior `#nullable enable`/`#nullable disable` region (lines 107–144) to the whole-file pragma by removing the interior `#nullable disable` directive, and reconcile `_emailHeader`'s existing `?` annotation with the file's full annotation pass to reach zero CS86xx
  - Acceptance: file carries a single whole-file pragma (no interior `#nullable disable` remains); `_emailHeader` annotation reconciled; annotation-only; zero CS86xx (verified as one unit in P7-T9).
- [ ] [P7-T5] Add `#nullable enable` to `UtilitiesCS/OutlookObjects/MailItem/MailItemHelper.Loading.cs` and consume the Batch G `Initializer.GetOrLoad` contract in `ResolveMail`'s return type and null-checks (no new runtime guards) to reach zero CS86xx
  - Acceptance: file carries the pragma; annotation-only; `ResolveMail`/`FromDfAsync`/`FromMailItemAsync` null-checks are compatible with the `GetOrLoad` contract; zero CS86xx (verified as one unit in P7-T9).
- [ ] [P7-T6] Add `#nullable enable` to `UtilitiesCS/OutlookObjects/MailItem/MailItemHelper.Properties.cs` and annotate `Sender` as `IRecipientInfo?`, `FolderInfo` as `IFolderWrapper?`, `AttachmentsInfo` as `IAttachment[]?`, and `Globals` as `IApplicationGlobals?` (the four lazy-backed properties without a `??` fallback) without adding new `??` guards, to reach zero CS86xx
  - Acceptance: file carries the pragma; annotation-only; the four properties carry the nullable public-contract annotation exactly as specified; no new `??` guard added; zero CS86xx (verified as one unit in P7-T9).
- [ ] [P7-T7] Add `#nullable enable` to `UtilitiesCS/OutlookObjects/MailItem/MailItemHelper.Serialization.cs` and annotate `ToSerializableObject`/`FromSerializableObject`, the `IEquatable<IItemInfo>` implementation, and recipient-equivalence helpers, consuming the Batch F `ItemInfo`/`EmailDetails` contracts, to reach zero CS86xx
  - Acceptance: file carries the pragma; annotation-only; zero CS86xx (verified as one unit in P7-T9).
- [ ] [P7-T8] Run CSharpier over the Batch G files (`MailItem/MailItemHelper.cs`, `MailItem/MailItemHelper.Html.cs`, `MailItem/MailItemHelper.Loading.cs`, `MailItem/MailItemHelper.Properties.cs`, `MailItem/MailItemHelper.Serialization.cs`) and confirm no residual formatting diff
  - Acceptance: `csharpier --check .` exits 0 for the touched files.
- [ ] [P7-T9] Run the pragma-only nullable build and record Batch G verification to `docs/features/active/utilitiescs-nullable-outlook-mailitem-item/evidence/qa-gates/batch-g-nullable-build.<yyyy-MM-ddTHH-mm>.md`
  - Command: `msbuild TaskMaster.sln /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU" /p:TreatWarningsAsErrors=true`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` confirming zero CS86xx across all 5 `MailItemHelper` partial-class files as one unit (no inconsistent CS8618/definite-assignment diagnostics between files) and NO new diagnostics elsewhere.
- [ ] [P7-T10] Run the Batch G UtilitiesCS tests and record results to `docs/features/active/utilitiescs-nullable-outlook-mailitem-item/evidence/regression-testing/batch-g-tests.<yyyy-MM-ddTHH-mm>.md`
  - Command: `vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /EnableCodeCoverage /TestCaseFilter:"FullyQualifiedName~MailItemHelper"`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` with passed/failed counts; `MailItemHelperCoreTests.cs`, `MailItemHelperProjectionTests.cs`, `MailItemHelper_ExtendedTests.cs`, and `UtilitiesCS.Test/EmailIntelligence/EmailParsingSorting/MailItemHelperTests.cs` (legacy-named duplicate) all green and behavior-identical.

### Phase 8 — Batch H ConvHelper Partial-Class Group

- [ ] [P8-T1] Verify the upstream #363 `IEnumerableExtensions.ForEach` contract has landed in `UtilitiesCS/Extensions/IEnumerableExtensions.cs` before annotating this batch
  - Command: `grep -n "ForEach" UtilitiesCS/Extensions/IEnumerableExtensions.cs`
  - Acceptance: grep output confirms the method exists; if not landed, this task is BLOCKED and must be re-run before proceeding to P8-T4.
- [ ] [P8-T2] Verify the upstream #364 `PrettyPrint.PrettyText` contract has landed in `UtilitiesCS/HelperClasses/PrettyPrint.cs` before annotating this batch
  - Command: `grep -n "PrettyText" UtilitiesCS/HelperClasses/PrettyPrint.cs`
  - Acceptance: grep output confirms the method exists; if not landed, this task is BLOCKED (this cluster cannot be fully verified CS86xx-clean until all eight #364 batches are merged upstream, per spec Constraints & Risks) and must be re-run before proceeding to P8-T3/P8-T4.
- [ ] [P8-T3] Add `#nullable enable` to `UtilitiesCS/OutlookObjects/Conversation/ConversationHelper.cs` and annotate `GetMailItemList`, `ConversationCt`, `GetConversationDf`/`GetConversationDfAsync` overloads, `FilterConversation`, and the shared `private static LogConversationTiming` helper's `string details = null` parameter to reach zero CS86xx
  - Acceptance: file carries the pragma; annotation-only; zero CS86xx (verified as one unit with `.Formatting.cs` in P8-T6).
- [ ] [P8-T4] Add `#nullable enable` to `UtilitiesCS/OutlookObjects/Conversation/ConversationHelper.Formatting.cs` and consume `LogConversationTiming`'s Batch H nullable-parameter contract and `IEnumerableExtensions.ForEach`/`PrettyPrint.PrettyText` consistently, annotating `GetInfoDf`/`GetInfoTable`, `GetDataFrame`/`GetDataFrameAsync`, `GetConversationTable`, and `PadOrTrunc`/`JoinFixedWidth`/`GetConversation`/`ResolveType`, to reach zero CS86xx
  - Acceptance: file carries the pragma; annotation-only; cross-file call to `LogConversationTiming` uses a consistent nullable parameter annotation; zero CS86xx (verified as one unit in P8-T6).
- [ ] [P8-T5] Run CSharpier over the Batch H files (`Conversation/ConversationHelper.cs`, `Conversation/ConversationHelper.Formatting.cs`) and confirm no residual formatting diff
  - Acceptance: `csharpier --check .` exits 0 for the touched files.
- [ ] [P8-T6] Run the pragma-only nullable build and record Batch H verification to `docs/features/active/utilitiescs-nullable-outlook-mailitem-item/evidence/qa-gates/batch-h-nullable-build.<yyyy-MM-ddTHH-mm>.md`
  - Command: `msbuild TaskMaster.sln /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU" /p:TreatWarningsAsErrors=true`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` confirming zero CS86xx across both `ConvHelper` partial-class files as one unit and NO new diagnostics elsewhere.
- [ ] [P8-T7] Run the Batch H UtilitiesCS tests and record results to `docs/features/active/utilitiescs-nullable-outlook-mailitem-item/evidence/regression-testing/batch-h-tests.<yyyy-MM-ddTHH-mm>.md`
  - Command: `vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /EnableCodeCoverage /TestCaseFilter:"FullyQualifiedName~ConversationHelper"`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` with passed/failed counts; `ConversationHelperAsyncTests.cs`, `ConversationHelperTests.cs`, `ConversationHelper_ExtendedTests.cs` all green and behavior-identical.

### Phase 9 — Batch I OlTableExtensions Partial-Class Group

- [ ] [P9-T1] Verify the upstream #363 `ArrayExtensions.ToStringArray`/`SliceRow`/`To2D` contract has landed in `UtilitiesCS/Extensions/ArrayExtensions.cs` before annotating this batch
  - Command: `grep -n "ToStringArray\|SliceRow\|To2D" UtilitiesCS/Extensions/ArrayExtensions.cs`
  - Acceptance: grep output confirms all three members exist; if not landed, this task is BLOCKED and must be re-run before proceeding to P9-T3/P9-T5.
- [ ] [P9-T2] Add `#nullable enable` to `UtilitiesCS/OutlookObjects/Table/OlTableExtensions.cs` and annotate `RemoveColumns`/`AddColumns`, `GetColumnDictionary`, `ExtractData2`, and the shared `logger`/`LogTableTiming` timing helpers to reach zero CS86xx
  - Acceptance: file carries the pragma; annotation-only; zero CS86xx (verified as one unit with the rest of the group in P9-T7).
- [ ] [P9-T3] Add `#nullable enable` to `UtilitiesCS/OutlookObjects/Table/OlTableExtensions.Etl.cs` and annotate `ETL`/`EtlAsync`/`EtlAsyncOld`, `EtlByRow`/`EtlByRowAsync` (consuming `ArrayExtensions.To2D`), `CastToRowArray`, and `GetBinFields`/`GetObjectFields`, calling `WriteValuesToData`/`ToObjectRow`/`ConvertBinColumnsToString`/`ConvertObjectColumnsToString` in `.RowTransforms.cs` with a consistent nullable contract, to reach zero CS86xx
  - Acceptance: file carries the pragma; annotation-only; cross-file calls to `.RowTransforms.cs` members use consistent nullable signatures; zero CS86xx (verified as one unit in P9-T7).
- [ ] [P9-T4] Add `#nullable enable` to `UtilitiesCS/OutlookObjects/Table/OlTableExtensions.RowTransforms.cs` and annotate `WriteValuesToData`, `ToObjectRow`, `ConvertBinColumnsToString`/`ConvertObjectColumnsToString` consistently with `.Etl.cs`'s Batch I call-site contract to reach zero CS86xx
  - Acceptance: file carries the pragma; annotation-only; zero CS86xx (verified as one unit in P9-T7).
- [ ] [P9-T5] Add `#nullable enable` to `UtilitiesCS/OutlookObjects/Table/OlTableExtensions.TableAccess.cs` and annotate `GetTableInView(Async)`, the `Store`/`MAPIFolder`/`Conversation` `GetTable(Async)`/`TryGetTable(Async)` overload families, `GetRows`, `GetColumnHeaders`, and `EnumerateTable` (consuming `ArrayExtensions.ToStringArray`/`SliceRow` and the Batch H `using static UtilitiesCS.ConvHelper;` formatting members) to reach zero CS86xx
  - Acceptance: file carries the pragma; annotation-only; `EnumerateTable`'s local variable types are compatible with the upstream `ToStringArray`/`SliceRow` nullable contract; zero CS86xx (verified as one unit in P9-T7).
- [ ] [P9-T6] Run CSharpier over the Batch I files (`Table/OlTableExtensions.cs`, `Table/OlTableExtensions.Etl.cs`, `Table/OlTableExtensions.RowTransforms.cs`, `Table/OlTableExtensions.TableAccess.cs`) and confirm no residual formatting diff
  - Acceptance: `csharpier --check .` exits 0 for the touched files.
- [ ] [P9-T7] Run the pragma-only nullable build and record Batch I verification to `docs/features/active/utilitiescs-nullable-outlook-mailitem-item/evidence/qa-gates/batch-i-nullable-build.<yyyy-MM-ddTHH-mm>.md`
  - Command: `msbuild TaskMaster.sln /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU" /p:TreatWarningsAsErrors=true`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` confirming zero CS86xx across all 4 `OlTableExtensions` partial-class files as one unit and NO new diagnostics elsewhere.
- [ ] [P9-T8] Run the Batch I UtilitiesCS tests and record results to `docs/features/active/utilitiescs-nullable-outlook-mailitem-item/evidence/regression-testing/batch-i-tests.<yyyy-MM-ddTHH-mm>.md`
  - Command: `vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /EnableCodeCoverage /TestCaseFilter:"FullyQualifiedName~OlTableExtensions"`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` with passed/failed counts; `OlTableExtensionsConversionTests.cs`, `OlTableExtensionsRetryTests.cs`, `OlTableExtensionsTransformTests.cs`, `OlTableExtensions_Tests.cs` all green and behavior-identical.

### Phase 10 — Final QC Loop, Coverage Delta, and Acceptance Verification

- [ ] [P10-T1] Run the CSharpier format gate over the repository and record it to `docs/features/active/utilitiescs-nullable-outlook-mailitem-item/evidence/qa-gates/final-csharpier.<yyyy-MM-ddTHH-mm>.md`
  - Command: `dotnet tool run csharpier --check .`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` with a clean (exit 0) result. If CSharpier changes files, restart the toolchain loop from this task.
- [ ] [P10-T2] Run the analyzer/codestyle build gate and record it to `docs/features/active/utilitiescs-nullable-outlook-mailitem-item/evidence/qa-gates/final-analyzer-build.<yyyy-MM-ddTHH-mm>.md`
  - Command: `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` with a clean build and no new analyzer diagnostics vs the P0-T3 baseline. If this step changes files, restart from P10-T1.
- [ ] [P10-T3] Run the pragma-only nullable/`TreatWarningsAsErrors` type-check gate and record it to `docs/features/active/utilitiescs-nullable-outlook-mailitem-item/evidence/qa-gates/final-nullable-build.<yyyy-MM-ddTHH-mm>.md`
  - Command: `msbuild TaskMaster.sln /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU" /p:TreatWarningsAsErrors=true` (NO `/p:Nullable=enable`)
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` confirming zero CS86xx across all 30 opted-in `UtilitiesCS/OutlookObjects/{MailItem,Item,Conversation,Attachment,Table}/` files and NO new diagnostics elsewhere; records that `/p:Nullable=enable` was NOT passed. If this step changes files, restart from P10-T1.
- [ ] [P10-T4] Run the coverage-enabled test gate over the UtilitiesCS test assemblies and record numeric post-change coverage to `docs/features/active/utilitiescs-nullable-outlook-mailitem-item/evidence/qa-gates/final-coverage.<yyyy-MM-ddTHH-mm>.md` with the Cobertura XML at `docs/features/active/utilitiescs-nullable-outlook-mailitem-item/evidence/qa-gates/final-coverage.<yyyy-MM-ddTHH-mm>.cobertura.xml`
  - Command: `pwsh scripts/vscode/Invoke-MSTestWithCoverage.ps1 -CoverageOutput docs/features/active/utilitiescs-nullable-outlook-mailitem-item/evidence/qa-gates/final-coverage.<yyyy-MM-ddTHH-mm>.cobertura.xml`
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` with NUMERIC post-change overall `line-rate`/`branch-rate` and the `UtilitiesCS/OutlookObjects/` targeted percentage if obtainable, plus passed/failed test counts (all UtilitiesCS tests green, including all legacy-named duplicate test files identified in research Section 8). If this step changes files, restart from P10-T1.
- [ ] [P10-T5] Verify `UtilitiesCS.csproj` introduces no project-level or solution-level `<Nullable>` element and record the check to `docs/features/active/utilitiescs-nullable-outlook-mailitem-item/evidence/qa-gates/csproj-no-nullable.<yyyy-MM-ddTHH-mm>.md`
  - Command: `grep -n "<Nullable>" UtilitiesCS/UtilitiesCS.csproj TaskMaster.sln`
  - Acceptance: artifact records `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` confirming no `<Nullable>` element exists (AC2 satisfied).
- [ ] [P10-T6] Compute the coverage delta and changed-line no-regression check and record it to `docs/features/active/utilitiescs-nullable-outlook-mailitem-item/evidence/qa-gates/coverage-delta.<yyyy-MM-ddTHH-mm>.md`
  - Inputs: baseline Cobertura from P0-T5 and post-change Cobertura from P10-T4.
  - Acceptance: artifact records baseline coverage (numeric), post-change coverage (numeric), and changed-line coverage, and confirms NO coverage regression on changed lines (AC4); if regression is detected the outcome is remediation-required, not PASS.
- [ ] [P10-T7] Map each acceptance-criteria checkbox in BOTH `spec.md` `## Definition of Done`/AC1–AC6 AND `user-story.md` `## Acceptance Criteria` (6 checkboxes) to its satisfying phase/task per the `acceptance-criteria-tracking` skill (full-feature mode: track each source file independently) and record it to `docs/features/active/utilitiescs-nullable-outlook-mailitem-item/evidence/qa-gates/ac-checkoff.<yyyy-MM-ddTHH-mm>.md`
  - Acceptance: artifact contains `Timestamp:` and, for `spec.md`, a row per DoD item and per AC1–AC6 — zero CS86xx per-file pragma (Phases 1–9 + P10-T3), no `<Nullable>` element (P10-T5), no behavior change / tests pass (Phases 1–9 test tasks + P10-T4), no coverage regression on changed lines (P10-T6), public-signature behavior-compatibility and upstream-contract consumption (P7-T1/T2/T6, P8-T1/T2, P9-T1), COM/VSTO coverage exemption respected with no forced new tests (Phases 3–9 acceptance notes) — each mapped to a satisfying task with its evidence path; AND, for `user-story.md` `## Acceptance Criteria`, a separate independent section with one row per each of the 6 Acceptance-Criteria checkboxes mapped to its satisfying task with its evidence path. Both source files must have their checkboxes updated to `[x]` as each item is verified.
