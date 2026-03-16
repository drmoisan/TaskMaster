<!-- markdownlint-disable-file -->

# Task Research Notes: outlook-objects-test-coverage

## Research Executed

### File Analysis

- `docs/features/active/2026-03-14-outlook-objects-test-coverage-67/issue.md`
  - Authoritative requirements source for Issue #67; requires per-file line coverage >=80% for `UtilitiesCS/OutlookObjects`, MSTest + Moq + FluentAssertions, mirrored test folders under `UtilitiesCS.Test/OutlookObjects`, and explicit `<Compile Include=...>` updates in `UtilitiesCS.Test.csproj`.
- `.github/prompts/research-issue.prompt.md`
  - Requires a single research artifact in `artifacts/research/`, comparison of at least two viable approaches with one final recommendation, and implementation guidance grounded in repo evidence plus external references.
- `.github/copilot-instructions.md`
  - Confirms repo-level C# testing conventions: MSTest framework, Moq for mocking, FluentAssertions for assertions.
- `.github/instructions/general-code-change.instructions.md`
  - Confirms bugfix/test workflow expectations, no-temp-file rule for tests, and full toolchain loop requirement after implementation.
- `.github/instructions/general-unit-test.instructions.md`
  - Confirms deterministic, isolated test expectations and repo-wide coverage policy.
- `.github/instructions/csharp-code-change.instructions.md`
  - Confirms required C# validation commands: `dotnet format`, analyzer build, nullable/warnings-as-errors build.
- `.github/instructions/csharp-unit-test.instructions.md`
  - Confirms concrete C# test toolchain selection: MSTest + Moq + FluentAssertions + `vstest.console.exe`.
- `scripts/vscode/Invoke-MSTest.ps1`
  - Confirms the workspace resolves `vstest.console.exe` via `vswhere` and currently runs discovered `*.Test.dll` assemblies with `/InIsolation`; coverage-specific orchestration will need to append coverage/reporting switches explicitly.
- `UtilitiesCS.Test/UtilitiesCS.Test.csproj`
  - Uses explicit `<Compile Include=...>` entries; current OutlookObjects tests are hand-listed and mostly live flat under `OutlookObjects\`, with only `Recipient\` and `Store\` subfolders mirrored so far.
- `UtilitiesCS/OutlookObjects/*`
  - Actual codebase contains 51 C# files under 15 subdirectories plus one root file in this checkout, not ~52. Largest clusters by verified line count are `Folder` (2,110), `Item` (1,257), `MailItem` (1,170), `Table` (964), `Store` (860), `Fields` (692), and `Conversation` (404).
- `UtilitiesCS.Test/OutlookObjects/*`
  - Current OutlookObjects test surface is 12 files total: 10 flat files, 1 `Recipient/RecipientStaticTests.cs`, and 1 `Store/StoresWrapperTests.cs`; the folder structure is not yet mirrored to production.
- `UtilitiesCS/OutlookObjects/Store/StoresWrapper.cs`
  - Good seam example: pure filter logic (`StoreIsIncluded`, `ShouldIncludeStore`) can be driven by mocked `Outlook.Store`; initialization path still depends on `Globals.Ol.NamespaceMAPI.Stores`.
- `UtilitiesCS/OutlookObjects/Store/StoreWrapper.cs`
  - Mixed wrapper: simple property projection around `Outlook.Store`, but `Init` and `GetSmtpAddressFromStore` still call live COM members.
- `UtilitiesCS/OutlookObjects/Store/StoreWrapperController.cs`
  - High-risk WinForms/controller file with `Viewer.ShowDialog`, `PickFolder`, `FolderBrowserDialog`, and `MyBox.ShowDialog`; only logic already behind `IStoreWrapperViewer` / `SelectFolder` seams is unit-test friendly today.
- `UtilitiesCS/OutlookObjects/Recipient/RecipientStatic.cs`
  - Large static helper with both pure parsing (`ConvertRecipientToHtml`, `ExtractNameFromAddress`) and deeply nested Outlook resolution (`AddressEntry`, `PropertyAccessor`, `NameSpace.CreateRecipient`).
- `UtilitiesCS/OutlookObjects/Folder/FolderWrapper .cs`
  - Critical high-risk wrapper with lazy evaluation, COM enumeration, `Marshal.ReleaseComObject`, async item comparison, and a filename containing a literal space before `.cs` (`FolderWrapper .cs`), which is a repo-specific path hazard.
- `UtilitiesCS/OutlookObjects/Folder/FolderMinimalWrapper.cs`
  - Promising seam-ready folder serializer with `internal virtual ToRelativePath` and `RestoreFromRelativePath`; still depends on live `Outlook.Folder` graphs.
- `UtilitiesCS/OutlookObjects/Folder/FolderConverter.cs`
  - Mixed pure/UI file: `SanitizeFilename`, illegal-character checks, and root resolution are unit-testable; `AskUserForAlternatives` and `MyBox/InputBox` branches are not isolated.
- `UtilitiesCS/OutlookObjects/Folder/FolderPredictor.cs`
  - Large orchestration-heavy class spanning Outlook navigation, regex search, suggestion/recent aggregation, WinForms dialogs, and filesystem creation via `Directory.CreateDirectory`; direct unit coverage requires targeted seams.
- `UtilitiesCS/OutlookObjects/MailItem/MailItemHelper.cs`
  - Large lazy-loading cache with many `virtual` members and some static helpers (`CompressPlainText`), but core initialization still captures live `MailItem`, `Folder`, attachments, recipients, `GetItemFromID`, and HTML/property accessor state.
- `UtilitiesCS/OutlookObjects/MailItem/ItemInfo.cs`
  - Pure serializable DTO/comparer implementation; existing tests show it is already seam-friendly and can serve as a style baseline.
- `UtilitiesCS/OutlookObjects/Attachment/AttachmentSerializable.cs`
  - Mostly seam-friendly DTO with internal helpers and one `internal virtual ParseFileName`, but `GetBytes` / `TryFromSaveAsLoad` hit local filesystem and temp-file paths that conflict with current unit-test policy.
- `UtilitiesCS/OutlookObjects/Item/OutlookItem.cs`
  - Reflection-based wrapper that closely matches Microsoft’s published helper pattern; virtual helper methods make it suitable for focused unit tests over reflection behavior and exception handling.
- `UtilitiesCS/OutlookObjects/Table/OlTableExtensions.cs`
  - Large static extension cluster that mixes pure ETL transforms with COM table access, retry loops, timeouts, and async helpers; this is one of the highest-value/highest-risk coverage targets.
- `UtilitiesCS/OutlookObjects/Conversation/ConversationHelper.cs`
  - Actually declared as `ConvHelper` under `namespace UtilitiesCS`; mixes `DataFrame`, Outlook `Conversation`, `GetItemFromID`, timeout/retry logic, and UI error dialogs.

### Code Search Results

- `UtilitiesCS/OutlookObjects/**/*.cs`
  - 51 production C# files were found; actual folder list is `AppointmentItem`, `Attachment`, `Calendar`, `Category`, `Com`, `Conversation`, `Explorer`, `Fields`, `Filter DASL`, `Folder`, `Folder\MsgToMime`, `Item`, `MailItem`, `Recipient`, `Store`, `Table`, plus root `MailResolution.cs`.
- `UtilitiesCS.Test/OutlookObjects/**/*.cs`
  - 12 current test files were found; only `Recipient` and `Store` have mirrored subfolders, while most files remain flat in `UtilitiesCS.Test/OutlookObjects`.
- `Marshal\.|ReleaseComObject|FinalReleaseComObject|CreateItem|new Outlook\.|ApplicationClass|Namespace|MAPIFolder|MailItem`
  - COM-heavy hotspots cluster in `Folder/FolderWrapper .cs`, `Conversation/ConversationHelper.cs`, `Recipient/RecipientStatic.cs`, `MailItem/MailItemHelper.cs`, `Table/OlTableExtensions.cs`, and `Store/*`.
- `interface |internal virtual|virtual |\[OnDeserialized\]|IApplicationGlobals|IStoreWrapperViewer|IFolderWrapper|IAttachment`
  - Existing seam surface already includes `IApplicationGlobals`, `IStoreWrapperViewer`, `IFolderWrapper`, `IAttachment`, `JsonConstructor`/`OnDeserialized` restore points, and many `virtual` / `internal virtual` methods in `MailItemHelper`, `MeetingItemHelper`, `FolderWrapper`, `FolderMinimalWrapper`, `AttachmentSerializable`, and `OutlookItem`.
- `MessageBox|ShowDialog|InputBox|FolderBrowserDialog|PickFolder|Directory\.CreateDirectory|File\.ReadAllBytes|File\.Delete|SaveAsFile|ReleaseComObject|Globals\.|GetNamespace\("MAPI"\)|GetItemFromID|PropertyAccessor`
  - Verified high-risk files with UI/filesystem/global side effects are `FolderPredictor.cs`, `FolderConverter.cs`, `StoreWrapperController.cs`, `AttachmentSerializable.cs`, `FolderWrapper .cs`, `Conversation/ConversationHelper.cs`, `Table/OlTableExtensions.cs`, and `MailItem/MailItemHelper.cs`.
- `OutlookObjects\\.*\.cs` in `UtilitiesCS.Test/UtilitiesCS.Test.csproj`
  - Every existing OutlookObjects test file is explicitly listed in the project file; every new test file will require a matching `<Compile Include=...>` entry or it will not build.
- `Obsolete|Ignore\(|InternalsVisibleTo|JsonConstructor|JsonIgnore`
  - Several legacy or compatibility surfaces remain in scope: `OlItemSummary` is `[Obsolete]` but already has tests; `UserDefinedFields` contains many obsolete overloads; serialization-focused wrappers use `JsonIgnore` / `JsonConstructor` heavily and should be validated accordingly.

### External Research

- #githubRepo:"devlooped/moq Quickstart mock interfaces virtual members strict recursive mocks"
  - Moq Quickstart (updated Jul 4, 2024) confirms the repo’s current test style is valid: use `Mock<T>`, `Setup`, `SetupGet`, `SetupProperty`, `SetupAllProperties`, `MockBehavior.Strict`, `DefaultValue.Mock`, `MockRepository`, and verification helpers. The guidance explicitly targets interfaces and overridable members, which aligns with the repo’s existing success mocking `MailItem`, `AppointmentItem`, `Store`, and `Folder` via generated proxies.
- #fetch:https://github.com/devlooped/moq/wiki/Quickstart
  - The most relevant Moq guidance for this feature is: prefer strict mocks when interaction matters; use property setup/tracking for mutable Outlook proxies; use recursive/default mocks for deep property graphs; use `Mock.Get(instance)` to continue configuring nested mocks; and remember protected/non-virtual members need alternate patterns.
- #fetch:https://learn.microsoft.com/en-us/office/client-developer/outlook/pia/how-to-create-a-helper-class-to-access-common-outlook-item-members
  - Microsoft documents the exact reflection-wrapper pattern implemented in `UtilitiesCS/OutlookObjects/Item/OutlookItem.cs`: Outlook often returns generic `Object` values, so a helper wrapper that centralizes common properties/methods behind reflection is a supported pattern rather than a repo-local invention.
- #fetch:https://learn.microsoft.com/en-us/office/vba/api/outlook.table
  - Microsoft documents `Outlook.Table` as a read-only, lightweight rowset whose rows reflect folder/search data, with `EntryID` used to rehydrate writable items via `NameSpace.GetItemFromID`. This matches the repo’s `ConversationHelper`/`OlTableExtensions` ETL and indicates that tests should focus on column transforms, retry logic, and result shaping rather than pretending `Table` is a fully mutable object graph.
- #fetch:https://learn.microsoft.com/en-us/office/vba/api/outlook.folder.gettable
  - `Folder.GetTable` returns all items when no filter is provided, accepts Jet or DASL filters, and uses a default column set that callers may modify via `Columns.Add/Remove/RemoveAll`. That directly supports the repo’s `DASLFilterParser`, `OlTableExtensions.RemoveColumns/AddColumns`, and retry-heavy table acquisition flows.
- #fetch:https://learn.microsoft.com/en-us/office/vba/api/outlook.namespace.getitemfromid
  - `NameSpace.GetItemFromID` returns a generic Outlook `Object` and usually needs both item EntryID and StoreID; this matches `MailItemHelper.FromDf*` and `ConversationHelper.GetItemAsync/GetMailItemList` and reinforces the need for small wrappers/factories around item resolution rather than end-to-end Outlook integration tests.
- #fetch:https://learn.microsoft.com/en-us/dotnet/api/system.runtime.interopservices.marshal.releasecomobject
  - Microsoft warns that `ReleaseComObject` explicitly manipulates RCW lifetime and can break callers if used incorrectly; this makes `FolderWrapper` and other manual release loops higher-risk and argues for narrow, behavior-level tests instead of over-asserting exact release timing.
- #fetch:https://learn.microsoft.com/en-us/dotnet/api/system.runtime.interopservices.marshal.finalreleasecomobject
  - `FinalReleaseComObject` releases all RCW references and makes the object unusable afterward; together with `ReleaseComObject` guidance, this supports treating COM-release branches as boundary behavior to smoke-test only when a seam exists, not as the primary target for broad mock-heavy unit coverage.

### Project Conventions

- Standards referenced: `.github/copilot-instructions.md`, `.github/instructions/general-code-change.instructions.md`, `.github/instructions/general-unit-test.instructions.md`, `.github/instructions/csharp-code-change.instructions.md`, `.github/instructions/csharp-unit-test.instructions.md`, `scripts/vscode/Invoke-MSTest.ps1`
- Instructions followed: research-only scratch-space rule, single-artifact rule, exact Task Researcher template, issue.md-as-authoritative-source rule, C# large-path/orchestration context, explicit compile-include convention for `UtilitiesCS.Test.csproj`

## Key Discoveries

### Project Structure

`UtilitiesCS/OutlookObjects` is not a single pattern-heavy module; it is a mixed bag of:

- pure/static helper logic (`DASLFilterParser`, comparer classes, string/regex helpers, portions of `FolderConverter`, DTO equality/hash code in `ItemInfo`),
- reflection wrappers (`OutlookItem`, `OutlookItemTry*`, flaggable wrappers),
- lazy serializable wrappers (`AttachmentSerializable`, `FolderMinimalWrapper`, `FolderWrapper`, `StoreWrapper`, `MailItemHelper`, `MeetingItemHelper`), and
- orchestration/UI/COM-heavy classes (`FolderPredictor`, `StoreWrapperController`, `ConversationHelper`, `OlTableExtensions`, `UserDefinedFields`).

Verified current size profile:

- Production: 51 files, ~9,248 lines.
- Tests: 12 files, ~2,561 lines.
- Largest production directories by line count: `Folder` 2,110; `Item` 1,257; `MailItem` 1,170; `Table` 964; `Store` 860; `Fields` 692; `Conversation` 404.

Important repo-specific wrinkles:

- Test folders do **not** yet mirror production folders, so substantial path normalization and project-file maintenance remain.
- Namespaces do not consistently match folders. Example: `AttachmentSerializable.cs` lives under `OutlookObjects/Attachment` but uses `namespace UtilitiesCS.EmailIntelligence.EmailParsing`; `UserDefinedFields.cs` lives under `OutlookObjects/Fields` but uses `namespace UtilitiesCS.OutlookExtensions`; `ConversationHelper.cs` declares `ConvHelper` in `namespace UtilitiesCS`.
- `FolderWrapper .cs` includes a literal space before `.cs`, which is easy to miss when writing compile includes, coverage filters, or search globs.

### Implementation Patterns

Verified seam patterns already available in the repo:

1. **Direct Moq against Outlook interop proxy types**
   - Existing tests successfully use `new Mock<MailItem>()`, `new Mock<AppointmentItem>()`, `new Mock<Folder>()`, and `new Mock<Store>()`.
   - This works well for property-driven methods and light branching.

2. **Interface-backed serialization boundaries**
   - `IApplicationGlobals`, `IAttachment`, `IFolderWrapper`, `IItemInfo`, and `IStoreWrapperViewer` already exist.
   - They provide natural boundaries for tests and selective seam injection.

3. **Internal/virtual helper methods for targeted subclassing**
   - `AttachmentSerializable.ParseFileName`, `FolderMinimalWrapper.ToRelativePath/RestoreFromRelativePath`, `FolderWrapper.LoadName/LoadRelativePath`, `OutlookItem.TryGetPropertyInfo/GetPropertyValueIfExists/SetPropertyValue/CallMethod`, and many `MailItemHelper` / `MeetingItemHelper` members are virtual.
   - These are ideal for test-specific subclasses where direct COM setup is brittle.

4. **Serialization restore hooks**
   - `StoresWrapper.RewireOlObjectsAsync`, `FolderWrapper` JSON constructor, and `JsonIgnore`/`JsonProperty` patterns indicate the module expects partially serialized state to be rehydrated later. Those branches can be tested with object state and fake wrappers before hitting live Outlook.

5. **Global singleton coupling**
   - Many classes still reach through `Globals.Ol.*`, `Globals.AF.*`, `MyBox`, `InputBox`, `MessageBox`, and filesystem APIs directly. These are the main blockers for broad per-file coverage without seam work.

### Complete Examples

```csharp
// Representative repo pattern for OutlookObjects tests.
// Source: UtilitiesCS.Test/OutlookObjects/Store/StoresWrapperTests.cs
var mockStore = new Mock<Microsoft.Office.Interop.Outlook.Store>();
mockStore.Setup(s => s.ExchangeStoreType)
    .Returns(OlExchangeStoreType.olPrimaryExchangeMailbox);
mockStore.Setup(s => s.DisplayName).Returns("Main Store");
mockStore.Setup(s => s.FilePath).Returns(@"C:\Data\main.pst");

var result = StoresWrapper.StoreIsIncluded(
    mockStore.Object,
    excludedStoreNameContains: new List<string> { "Archive" },
    excludedStoreFilePathContains: new List<string> { "Temp" },
    gwsoFilePathContains: new List<string>(),
    excludePublicFolderStores: true,
    excludeGwsoStores: false);

result.Should().BeTrue();
```

### API and Schema Documentation

- `OutlookItem` reflection wrapper is consistent with Microsoft’s published helper-class guidance for common Outlook item members.
- `Outlook.Table` is a read-only rowset; test focus should be on:
  - column mutation behavior (`RemoveColumns`, `AddColumns`),
  - ETL/data-shaping methods (`GetColumnDictionary`, `WriteValuesToData`, `ToObjectRow`, binary/object conversions),
  - retry/timeout branches around `GetTableAsync`, and
  - `EntryID`/`StoreID` rehydration logic through `NameSpace.GetItemFromID`.
- `ReleaseComObject` / `FinalReleaseComObject` are lifetime-boundary APIs with documented failure risks if misused; tests should avoid over-specifying RCW counts or assuming safe reuse after release.
- `UserDefinedFields` relies on `PropertyAccessor`, `UserProperties`, and `OlUserPropertyType` mappings. Its pure validation/default-value logic (`ValidPropertyArgs`, `GetUdfValue`, lookup dictionaries) is testable independently from live item mutation.

### Configuration Examples

```xml
<!-- Existing repo convention from UtilitiesCS.Test/UtilitiesCS.Test.csproj -->
<Compile Include="OutlookObjects\Store\StoresWrapperTests.cs" />
<Compile Include="OutlookObjects\Recipient\RecipientStaticTests.cs" />

<!-- Future mirrored additions should follow the same explicit include pattern -->
<Compile Include="OutlookObjects\Folder\FolderMinimalWrapperTests.cs" />
<Compile Include="OutlookObjects\MailItem\MailItemHelperTests.cs" />
<Compile Include="OutlookObjects\Table\OlTableExtensionsTests.cs" />
```

### Technical Requirements

- **Recommended batching strategy by subdirectory/type**
  1. **Pure/seam-ready files first**: `Filter DASL`, simple DTOs/comparers, `MailResolution.cs`, `Calendar.cs`, `Com/ComType.cs`, `ItemInfo.cs`, `RecipientInfo.cs`, `OutlookItem.cs` and `OutlookItemTry*` helper methods.
  2. **Existing-pattern expansion**: `Store/StoresWrapper.cs`, `Store/StoreWrapper.cs`, `Recipient/RecipientStatic.cs`, `Folder/FolderMinimalWrapper.cs`, `Folder` comparer files, `Attachment/AttachmentSerializable.cs` safe branches only.
  3. **Reflection/lazy wrappers**: `Item/OutlookItemFlaggable*`, `Item/OlItemPseudoInterface.cs`, `MailItem/EmailDetails*.cs`, `MailItem/MailResolution.cs`, `AppointmentItem/MeetingItemHelper.cs`, `MailItem/MailItemHelper.cs` (starting with static/helper methods and DTO conversion paths).
  4. **High-risk seam-needed files**: `Folder/FolderWrapper .cs`, `Folder/FolderPredictor.cs`, `Fields/UserDefinedFields.cs`, `Conversation/ConversationHelper.cs`, `Table/OlTableExtensions.cs`, `Store/StoreWrapperController.cs`, `Category/CreateCategory.cs`, `Explorer/ExplorerActions.cs`.

- **Likely production files and corresponding test groups**
  - `Attachment/AttachmentSerializable.cs` -> `UtilitiesCS.Test/OutlookObjects/Attachment/AttachmentSerializableTests.cs`
  - `Store/StoresWrapper.cs`, `Store/StoreWrapper.cs`, `Store/StoreWrapperController.cs` -> `UtilitiesCS.Test/OutlookObjects/Store/*Tests.cs`
  - `Recipient/RecipientStatic.cs`, `Recipient/RecipientInfo.cs` -> `UtilitiesCS.Test/OutlookObjects/Recipient/*Tests.cs`
  - `Folder/FolderMinimalWrapper.cs`, `Folder/FolderConverter.cs`, `Folder/FolderWrapper .cs`, folder comparers -> `UtilitiesCS.Test/OutlookObjects/Folder/*Tests.cs`
  - `Item/OutlookItem.cs`, `OutlookItemTry.cs`, `OutlookItemTryGet.cs`, `OutlookItemFlaggable*.cs`, `OlItemPseudoInterface.cs` -> `UtilitiesCS.Test/OutlookObjects/Item/*Tests.cs`
  - `MailItem/MailItemHelper.cs`, `EmailDetails.cs`, `EmailDetailsWrapper.cs`, `MailItemExtensions.cs`, `MailResolution.cs` -> `UtilitiesCS.Test/OutlookObjects/MailItem/*Tests.cs`
  - `AppointmentItem/MeetingItemHelper.cs` -> `UtilitiesCS.Test/OutlookObjects/AppointmentItem/MeetingItemHelperTests.cs`
  - `Fields/UserDefinedFields.cs`, `Fields/MAPIFields.cs` -> `UtilitiesCS.Test/OutlookObjects/Fields/*Tests.cs`
  - `Conversation/ConversationHelper.cs` -> `UtilitiesCS.Test/OutlookObjects/Conversation/ConversationHelperTests.cs`
  - `Table/OlTableExtensions.cs`, `Table/OlToDoTable.cs` -> `UtilitiesCS.Test/OutlookObjects/Table/*Tests.cs`

- **Proposed helper/factory/fake/mock patterns**
  - Introduce a shared `OutlookObjects/TestDoubles/OutlookMockFactory` (or equivalent per-folder static helper) for common mock graphs: `MailItem + Parent Folder`, `Recipient + AddressEntry + ExchangeUser`, `Store + RootFolder + Inbox`, `Table + Columns + Row`, `NameSpace.GetItemFromID` resolver.
  - Use `MockRepository(MockBehavior.Strict)` for interaction-heavy classes and `DefaultValue.Mock` only when nested Outlook property graphs would otherwise dominate arrange code.
  - Prefer test-specific subclasses for `internal virtual` seams (`AttachmentSerializable`, `FolderMinimalWrapper`, `FolderWrapper`, `OutlookItem`) over broad production refactors.
  - For lazy wrappers (`MailItemHelper`, `MeetingItemHelper`), test static/pure helpers first and use state-primed instances or subclass overrides to avoid forcing every test through live `MailItem` resolution.
  - For `Table` and `Conversation` ETL flows, add fakes around rows/column dictionaries and isolate the pure transform methods before attempting COM-facing async retry branches.

- **High-risk files that likely require seams/refactors before useful unit tests**
  - `UtilitiesCS/OutlookObjects/Folder/FolderPredictor.cs` — mixes Outlook graph traversal, regex search, WinForms prompts, and filesystem creation.
  - `UtilitiesCS/OutlookObjects/Store/StoreWrapperController.cs` — controller logic is testable, but current UI/file-dialog methods need injectable dialog/picker seams for broad coverage.
  - `UtilitiesCS/OutlookObjects/Conversation/ConversationHelper.cs` — heavy Outlook/DataFrame/timeout/UI coupling.
  - `UtilitiesCS/OutlookObjects/Table/OlTableExtensions.cs` — very large file with both pure transforms and COM retry logic; requires a split mindset or helper extraction.
  - `UtilitiesCS/OutlookObjects/Fields/UserDefinedFields.cs` — very broad API surface over live `PropertyAccessor`/`UserProperties`; prioritize pure dictionary/default-value/validation branches first.
  - `UtilitiesCS/OutlookObjects/Folder/FolderWrapper .cs` — COM enumeration plus explicit `Marshal.ReleaseComObject` in loops.
  - `UtilitiesCS/OutlookObjects/Attachment/AttachmentSerializable.cs` temp-file branches — blocked by current no-temp-file test policy unless seams are added.

- **Verification plan aligned with repo C# toolchain**
  1. `dotnet restore TaskMaster.sln`
  2. `dotnet format TaskMaster.sln --verify-no-changes --no-restore` (or repo-approved `dotnet format TaskMaster.sln` during active editing, then verify-clean)
  3. `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
  4. `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true`
  5. `vstest.console.exe <discovered *.Test.dll assemblies> /EnableCodeCoverage /InIsolation /Logger:trx`
  6. Re-run the loop from formatting if any step changes files or fails.
  7. Use coverage output to verify per-file line coverage for `UtilitiesCS/OutlookObjects`; do not rely on solution-wide aggregate coverage alone.

**Mandatory unachievable objective callout**:
- **Direct unit coverage of the temp-file branches in `AttachmentSerializable.GetBytes` / `TryFromSaveAsLoad` and the dialog/file-picker branches in `StoreWrapperController` / `FolderPredictor` is not achievable under the current repo test policy without first adding seams/refactors (or obtaining an explicit exception to the no-temp-file rule).**

## Recommended Approach

Adopt a **seam-first, batch-by-testability** strategy: expand coverage aggressively for pure logic, DTOs, comparers, reflection wrappers, and already seam-friendly Outlook wrappers **before** touching the UI/file-dialog/RCW-heavy branches; then add the smallest possible production seams only where a file cannot realistically reach 80% otherwise.

This is the best fit for the repo because it:

- aligns with the existing successful test style already in `StoresWrapperTests`, `OlItemSummary_Tests`, `ItemInfo_Tests`, and the folder comparer tests,
- minimizes disruptive production changes across a 51-file Outlook/COM surface area,
- preserves the current .NET Framework 4.8.1-era design instead of introducing a sweeping adapter rewrite, and
- allows orchestration to show real coverage gains early on the largest volume of low-risk files.

Recommended design boundaries for execution:

- **Phase A — low-risk coverage harvest**
  - Add mirrored test folders and compile includes.
  - Cover pure/static/helper files first.
  - Normalize current flat tests into mirrored paths only when doing so does not create churn beyond the active batch.

- **Phase B — wrapper expansion with existing seams**
  - Add tests for `StoreWrapper`, `FolderMinimalWrapper`, `OutlookItem`, `MailResolution`, `RecipientInfo`, and safe `MailItemHelper` / `MeetingItemHelper` helpers.
  - Use Moq directly against Outlook proxy types plus test subclasses for virtual seams.

- **Phase C — targeted seam insertion for blocked hotspots**
  - Introduce only narrow abstractions where needed, e.g. dialog/picker/file-system wrappers for `StoreWrapperController` and `FolderPredictor`, or small extraction helpers in `OlTableExtensions` / `UserDefinedFields` for pure transforms.
  - Keep seam additions local to the file under test instead of creating a repo-wide Outlook abstraction layer.

- **Phase D — high-risk coverage with explicit stop/go checkpoints**
  - Tackle `Conversation`, `Table`, `Fields`, and `FolderWrapper` only after validating that the earlier batches improve coverage materially and that seam additions are still staying within orchestration budget.

Brief rejected alternatives:

- **Full adapter-layer rewrite over all Outlook COM types** was rejected because it would balloon scope across many production files before delivering coverage, which conflicts with the repo’s simplicity-first policy and the already-proven viability of narrower seams.
- **Outlook-backed integration testing for the whole module** was rejected because the repo’s unit-test policy forbids external Outlook dependencies and requires deterministic, isolated tests.

## Implementation Guidance

- **Objectives**: Raise per-file coverage in `UtilitiesCS/OutlookObjects` to >=80% where the file is unit-testable; keep tests deterministic and Outlook-free; mirror production folder structure under `UtilitiesCS.Test/OutlookObjects`; preserve explicit compile-include maintenance.
- **Key Tasks**:
  - Create mirrored test subfolders and add each new file to `UtilitiesCS.Test.csproj`.
  - Start with the pure/seam-ready batches to establish helper factories and coverage momentum.
  - Reuse shared Outlook mock builders for `Store`, `Folder`, `MailItem`, `Recipient`, `NameSpace`, and `Table` scenarios.
  - Add targeted production seams only for blocked branches in `StoreWrapperController`, `FolderPredictor`, `OlTableExtensions`, `UserDefinedFields`, `ConversationHelper`, and `AttachmentSerializable`.
  - Capture per-file coverage after each batch, not just at the end.
  - Treat the current branch/issue mismatch (`feature/outlook-objects-test-coverage-66` vs promoted Issue #67) as a coordination note for orchestration artifacts only; no branch operation is required for this research.
- **Dependencies**: Existing repo dependencies are sufficient: MSTest, Moq, FluentAssertions, Newtonsoft.Json, Microsoft.Office.Interop.Outlook, and existing repo helper abstractions/interfaces. No new package is justified by the evidence gathered.
- **Success Criteria**:
  - New tests compile because every file has a matching `<Compile Include=...>` entry.
  - New tests follow MSTest + Moq + FluentAssertions conventions already present in the repo.
  - Coverage reports show material per-file gains and identify the remaining blocked files explicitly.
  - High-risk files only receive minimal seam/refactor changes justified by otherwise-unreachable coverage requirements.
  - Final validation passes the repo’s format -> analyzer build -> nullable build -> `vstest.console.exe` coverage run sequence.