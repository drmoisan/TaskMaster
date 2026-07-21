
# Research: `utilitiescs-nullable-outlook-folder-store` (Issue #365 / epic placeholder 9007)

- Scope: `UtilitiesCS/OutlookObjects/Folder/` (recursive, incl. `MsgToMime/`) and
  `UtilitiesCS/OutlookObjects/Store/`.
- Nature of work: null-annotation and null-safety remediation only, per the per-file
  `#nullable enable` opt-in architecture confirmed by the epic
  (`docs/features/epics/utilitiescs-nullable-remediation/epic.md`). No behavior changes, no
  refactors, no API redesign, no new features.
- All file counts, pragma states, and code excerpts below were obtained by reading every
  `.cs` file in both directories in this worktree (commit surface as of 2026-07-18) and by
  grepping for `#nullable enable`. No count is estimated.

## 1. Refined file inventory

### 1.1 Totals (verified, supersedes the epic's ~29-file estimate)

| Directory | Total `.cs` files | Already `#nullable enable` (verify-only) | Designer-exempt (recommend left oblivious) | Opt-in targets (need pragma) |
| --- | --- | --- | --- | --- |
| `Folder/` (incl. `MsgToMime/`) | 63 | 17 | 0 | 46 |
| `Store/` | 20 | 1 | 2 | 17 |
| **Cluster total** | **83** | **18** | **2** | **63** |

The epic manifest's `~29` estimate for `utilitiescs-nullable-outlook-folder-store`
(`docs/features/epics/utilitiescs-nullable-remediation/epic.md`, Wave 1 table) is stale: it
predates the breadcrumb work (#327/#349/#350/#351, which added `Breadcrumb*.cs`,
`FolderBreadcrumbBridgeRouter.cs`, `FolderBreadcrumbSegment.cs`, and the `IFolderHierarchyProvider`
family) and the folder-tree-percentage work (#324/#325, which added the `FolderTree*`,
`FolderSuggestionTree`/`FolderSuggestionNode`, `FolderNodeViewModel`, `FolderRow`/`FolderScore`,
`FolderHierarchyBuilder`, and `FolderTreeStateModel` families). The real remediation surface is 63
files needing a new pragma, not ~29. The epic manifest itself is out of scope to edit (per the
delegation instructions); this is a finding for the spec/plan authors, not an edit to
`epic.md`.

### 1.2 Hazard: literal trailing space in a filename

`UtilitiesCS/OutlookObjects/Folder/FolderWrapper .cs` (note the space before `.cs`) is
confirmed to exist via `Glob` and `Read`. It defines `public class FolderWrapper : INotifyPropertyChanged, IFolderWrapper` (531 lines). Handling hazards for the atomic plan:

- `csharpier` invoked as `csharpier .` operates recursively and should pick the file up by
  glob, but any file-scoped invocation (`csharpier UtilitiesCS/OutlookObjects/Folder/FolderWrapper .cs`)
  will be parsed as two shell arguments unless the whole path is quoted.
- `git add`/`git status` pathspecs must quote the path (`"UtilitiesCS/OutlookObjects/Folder/FolderWrapper .cs"`) or escape the space; an unquoted pathspec will not match.
- msbuild's `Compile Include` glob (`**/*.cs`) already includes it (it is currently compiling),
  so no `.csproj` change is needed — only tooling invocations that pass an explicit file path
  need quoting.
- Do not rename the file. Renaming is a workspace change beyond annotation-only scope and would
  break any existing `Compile Include`/exclude patterns or code referencing the class by file
  path in tooling. Flag the space as a pre-existing naming defect for a future issue; do not fix
  it here.

### 1.3 Full inventory table — `Folder/`

Legend for **Pragma**: `enabled` = already carries `#nullable enable` (verify-only); `none` = not yet opted in (this feature's remediation target).
Legend for **Class**: `com-exempt` (COM-bound/WinForms-Designer, no injectable seam, per CLAUDE.md COM/VSTO/WinForms exemption), `testable-domain` (host-neutral or COM-adjacent but already unit-tested via mocked/faked Interop interfaces or injected seams — NOT exempt, must not regress), `iface-enum-dto` (interface/enum/simple DTO, typically trivial).
Legend for **CS86xx pressure**: qualitative estimate from reading the code (no build was run to enumerate exact counts; see §4 for why).

| File | Pragma | Class | CS86xx pressure | Notes |
| --- | --- | --- | --- | --- |
| BreadcrumbBridgeMessages.cs | enabled | testable-domain | none | Verify-only. |
| BreadcrumbDocumentAssets.cs | enabled | iface-enum-dto | none | Verify-only; const strings only. |
| BreadcrumbHtmlRenderer.cs | enabled | testable-domain | none | Verify-only. |
| BreadcrumbMessageCodec.cs | enabled | testable-domain | none | Verify-only. |
| BreadcrumbMessages.cs | enabled | testable-domain | none | Verify-only. |
| BreadcrumbRenderProjection.cs | enabled | testable-domain | none | Verify-only. |
| BreadcrumbRow.cs | enabled | testable-domain | none | Verify-only. |
| BreadcrumbRowBuilder.cs | enabled | testable-domain | none | Verify-only. |
| BreadcrumbSegment.cs | enabled | iface-enum-dto | none | Verify-only. |
| BreadcrumbSelectionMap.cs | enabled | testable-domain | none | Verify-only. |
| BreadcrumbStateModel.cs | enabled | testable-domain | none | Verify-only. |
| DeadlineClock.cs | none | testable-domain | low | Small sealed class, no reference-type fields besides `Stopwatch`/`TimeSpan` (value types). Has `DeadlineClockTests.cs`. |
| FolderBreadcrumbBridgeRouter.cs | enabled | testable-domain | none | Verify-only. |
| FolderBreadcrumbSegment.cs | none | iface-enum-dto | low | Plain immutable DTO; ctor guards `key` non-null, `displayName`/`folderPath` fall back to `string.Empty`. Straightforward `?`/non-null split. |
| FolderConverter.cs | none | testable-domain | moderate | Static extension methods over `Folder`/`MAPIFolder`/`IApplicationGlobals`; several `Func<...>` static delegate properties (`AlternativeFolderPrompt`, `AlternativeFolderSelectionDialog`, `AlternativeFolderInputDialog`) with nullable-shaped tuple/dictionary generics; two `ToFsFolderpath` overloads return `null` from a `string`-typed method (return type must become `string?`). Has `FolderConverterTests.cs`/`FolderConverter_Tests.cs`. |
| FolderHierarchyBuilder.cs | none | testable-domain | moderate | `TreeNode<FolderNodeViewModel> currentNode = null;` / `string cumulative = null;` locals need `?`; consumes external `TreeNode<T>` (see §3, cross-cluster dependency). Has `FolderHierarchyBuilderTests.cs`. |
| FolderMinimalWrapper.cs | none | testable-domain | high | `Outlook.Folder OlRoot/OlFolder` non-null-annotated properties but constructed via an `internal FolderMinimalWrapper() { }` empty ctor (CS8618 risk); `Lazy<string> _lazyName/_lazyRelativePath` fields assigned only in `ResetLazy()`, not always called; `ToRelativePath()`/multiple logger paths return `null` from `string`-declared method. Has `FolderMinimalWrapperTests.cs`. |
| FolderNavigator.cs | none | com-exempt | low | Static helper directly walking `Outlook.Folder`/`Folders` with no injectable seam; `GetOutlookFolder` returns `null` from `Folder`-typed method (`Folder?`). Has `FolderNavigatorTests.cs` (likely via mocked Interop interfaces — Folder/Folders are COM interfaces, Moq-mockable). |
| FolderNodeViewModel.cs | none | testable-domain | low | Plain class; `char? Glyph` already nullable; string properties currently non-null (verify ctor never receives null in practice). Has `FolderNodeViewModelTests.cs`. |
| FolderPredictor.cs | none | testable-domain | high | 974 lines (pre-existing >500-line violation — do NOT split; annotation-only). Many `object`/`string` fields uninitialized in some ctor overloads (`_globals`, `_folderList`, `Suggestions`), several methods return `null` (`GetFolder` × 3 overloads, `CreateFolder`, `CreateFolderAsync`), optional parameters default to `null` (`List<string> emailSearchRoots = null`, `IEnumerable<(...)> exclusions = null`, `string defaultValue = null`). Extensive existing tests (`FolderPredictorTests.cs`, `FolderPredictorCoverageExpansionTests.cs`, `FolderPredictorSeam_Tests.cs`, `FolderPredictorSeam_DefaultOn_Tests.cs`). MUST be remediated in the same batch as `FolderPredictor.IFolderSearchHandler.cs` (partial class, see §2). |
| FolderPredictor.IFolderSearchHandler.cs | none | testable-domain | none | Empty partial-class declaration (`public partial class FolderPredictor : IFolderSearchHandler { }`) — no members, so this file alone emits no CS86xx once opted in; still must ship in the same commit/batch as `FolderPredictor.cs` because they are one partial type. |
| FolderProbabilityAdapter.cs | enabled | testable-domain | none | Verify-only. |
| FolderRow.cs | none | iface-enum-dto | low | `readonly struct`; `string Text` currently non-null, `FolderScore? Score` already nullable. |
| FolderScore.cs | none | iface-enum-dto | none/low | `readonly struct`; all members effectively non-null value/string. |
| FolderScorer.cs | none | testable-domain | high | 663 lines (pre-existing >500-line violation — do NOT split). Consumes `ScoDictionaryNew<string,long>` (cross-cluster, `ReusableTypeClasses`, oblivious today — see §3); several methods take `object folderObject`/`object foldersObject` and cast with `as` (`as string`, `as string[]`) producing nullable locals; `MailItem olMail` params dereferenced without upstream null-check in places; internal `struct FolderScoring` has array fields default-initialized. Has `FolderScorerTests.cs`, `FolderScorerCoverageExpansionTests.cs`, `FolderScorerRegressionTests.cs`. |
| FolderSuggestionNode.cs | enabled | testable-domain | none | Verify-only. |
| FolderSuggestionTree.cs | enabled | testable-domain | none | Verify-only. |
| FolderTree.cs | none | testable-domain | high | Directly constructs from `MAPIFolder` (COM interface, Moq-mockable — has `FolderTreeTests.cs`, so NOT COM-exempt despite the direct Interop dependency); `public event PropertyChangedEventHandler PropertyChanged;` (CS8618-adjacent nullable event field pattern); several tuple-returning `Compare`/`CompareMembers` methods; `_roots` list assigned in every ctor path (verify all paths). |
| FolderTreeCompatibilityView.cs | none | testable-domain | moderate | `CreateNode` returns `TreeNode<FolderWrapper>` but can return `null` (needs `TreeNode<FolderWrapper>?`); optional ctor param `selectionOverlay` defaults via `??`. Has `FolderTreeCompatibilityViewTests.cs`/`FolderTreeCompatibilityViewDisposalTests.cs`. |
| FolderTreeNodeKey.cs | none | iface-enum-dto | low | `IEquatable<FolderTreeNodeKey>`; `Equals(object obj)` param must become `object?`; `Equals(FolderTreeNodeKey other)` param becomes `FolderTreeNodeKey?`. Has `FolderTreeNodeKeyTests.cs`. |
| FolderTreeRefreshReason.cs | none | iface-enum-dto | none | Plain enum. |
| FolderTreeRequest.cs | none | iface-enum-dto | low | Ctor takes `IEnumerable<string> storeIds` and null-coalesces; needs `IEnumerable<string>? storeIds`. Has `FolderTreeRequestTests.cs`. |
| FolderTreeSelectionOverlay.cs | none | testable-domain | low | Same null-coalescing ctor pattern. Has `FolderTreeSelectionOverlayTests.cs`. |
| FolderTreeSnapshot.cs | none | testable-domain | moderate | Two ctor overloads, one with `FolderTreeRequest request = null`-shaped null-forwarding; `FindByPath`/`TryGetNode` return nullable nodes (`FolderTreeSnapshotNode?`, `out FolderTreeSnapshotNode? node`). Has `FolderTreeSnapshotTests.cs`. |
| FolderTreeSnapshotBuilder.cs | none | testable-domain | low | Optional `IDeadlineClock deadlineClock`/`IDispatcherYield dispatcherYield` ctor params (both nullable by design — `?? null` two-arg convenience ctor). Has `FolderTreeSnapshotBuilderTests.cs` + Cancellation/Yield variants. |
| FolderTreeSnapshotChangedEventArgs.cs | none | iface-enum-dto | low | Straightforward DTO with guard clauses already in place. |
| FolderTreeSnapshotNode.cs | none | testable-domain | moderate | `FolderTreeNodeKey parentKey` is nullable by design (root node has no parent) → `FolderTreeNodeKey?`; `EntryId`/`RelativePath`/`StaleReason` already null-coalesce to `string.Empty`. Has `FolderTreeSnapshotNodeTests.cs`. |
| FolderTreeSnapshotQueries.cs | none | testable-domain | moderate | Static query helpers; `GetArchiveRoot`/`FindByPath`-style `FirstOrDefault` returns feed into methods returning `FolderTreeSnapshotNode` (must become nullable); `GetAncestorChain` already null-safe. Has `FolderTreeSnapshotQueriesTests.cs`/`FolderTreeSnapshotQueriesAncestorChainTests.cs`. |
| FolderTreeStateModel.cs | none | testable-domain | moderate | `_highlighted` field of type `TreeNode<FolderNodeViewModel>` is nullable by design (`Highlighted => _highlighted` documented as null when nothing highlighted) → `TreeNode<FolderNodeViewModel>? _highlighted`. Consumes external `TreeNode<T>` (oblivious today, see §3). Has `FolderTreeStateModelTests.cs`. |
| FolderWrapper .cs (space in filename) | none | testable-domain | high | 531 lines (pre-existing >500-line violation — do NOT split; also see §1.2 filename hazard). `MAPIFolder`-typed properties (`OlFolder`/`OlRoot`, COM interface, Moq-mockable — extensive tests: `FolderWrapperCoverageExpansionTests.cs`, `FolderWrapperStateTests.cs`, `FolderWrapperTraversalTests.cs`); several `Lazy<T>` fields assigned only in `ResetLazy()` (CS8618-adjacent, not always invoked from the parameterless `protected FolderWrapper()` ctor used by the `[JsonConstructor]` overload path); `IApplicationGlobals Globals { get; set; }` nullable by design (checked with `is null` before use); `LoadItemHelpers`/`CompareItemsAsync` interplay with `AsyncLazy<IItemInfo[]>` (external type, see §3). |
| FolderWrapperNameAndParentNameComparer.cs | none | testable-domain | low | `IEqualityComparer<TreeNode<FolderWrapper>>`; `Equals(x, y)` params become `TreeNode<FolderWrapper>?`; already null-guards internally. Has `FolderWrapperNameAndParentNameComparerTests.cs` (×2 test files). |
| FolderWrapperNameComparer.cs | none | testable-domain | low | Same comparer-pattern pressure. Has `FolderWrapperNameComparerTests.cs` (×2). |
| FolderWrapperNameCountSizeComparer.cs | none | testable-domain | low | Same comparer-pattern pressure. Has `FolderWrapperNameCountSizeComparerTests.cs` (×2). |
| FolderWrapperNodeComparer.cs | none | testable-domain | low | Same comparer-pattern pressure; internally composes the other comparers. Has `FolderWrapperNodeComparerTests.cs` (×2). |
| FolderWrapperNodeContentsComparer.cs | none | testable-domain | low | Same comparer-pattern pressure. Has `FolderWrapperNodeContentsComparerTests.cs` (×2). |
| IDeadlineClock.cs | none | iface-enum-dto | none | Two bool/void members; no nullable surface. |
| IDispatcherYield.cs | none | iface-enum-dto | none | Single `Task YieldAsync(CancellationToken)` member. |
| IFolderHandleResolver.cs | none | iface-enum-dto | low | `object Resolve(...)`/`bool TryResolve(..., out object folder)` — `out object?` needed. |
| IFolderHierarchyProvider.cs | none | iface-enum-dto | low | `ResolveLeafKeyAsync` doc says returns `null` when unmatched → `Task<FolderTreeNodeKey?>`. |
| IFolderProbabilitySource.cs | enabled | iface-enum-dto | none | Verify-only. |
| IFolderSearchHandler.cs | none | iface-enum-dto | low | Optional params default `null` over `List<string>`/`IEnumerable<(...)>` tuple — needs `?` on each. Must match `FolderPredictor`'s implementation signature exactly (same batch consideration, see §2/§3). |
| IOutlookFolderHierarchyReader.cs | none | iface-enum-dto | none | Single async method, no nullable surface beyond param types already covered by `FolderTreeRequest`/`IDeadlineClock`/`IDispatcherYield`. |
| IOutlookFolderNotificationSink.cs | none | iface-enum-dto | none | Events + `Store`/`string` params, all effectively non-null by contract. |
| IOutlookFolderTreeService.cs | none | iface-enum-dto | none | No nullable surface. |
| MsgToMime/MAPIMethods.cs | none | com-exempt | none | `internal class MAPIMethods` — COM interop enum/interface declarations only (`ComImport`, `PreserveSig`), no executable logic, no coverage implication either way. |
| OutlookFolderHandleResolver.cs | none | com-exempt | low | Every member already `[ExcludeFromCodeCoverage]`; thin adapter directly wrapping `Outlook.NameSpace.GetFolderFromID`. `bool TryResolve(FolderTreeSnapshotNode node, out object folder)` needs `FolderTreeSnapshotNode?`/`out object?`. |
| OutlookFolderHierarchyProvider.cs | none | testable-domain | moderate | Depends only on `IOutlookFolderTreeService` (interface, no direct Interop type) — genuinely host-neutral, not COM-exempt. `ResolveLeafKeyAsync` returns `match?.Key` (already correctly nullable-shaped once the interface's return type is annotated). Has `OutlookFolderHierarchyProviderTests.cs`. |
| OutlookFolderHierarchyReader.cs | none | com-exempt (mixed) | moderate | Public/most-private members marked `[ExcludeFromCodeCoverage]` (direct `Outlook.NameSpace`/`Outlook.Store` construction paths), but the file also declares two testable internal interfaces (`IOutlookStoreAdapter`, `IOutlookFolderAdapter`) and non-excluded static helpers (`GetRelativePath`) exercised by `OutlookFolderHierarchyReaderTests.cs` via fakes. Root can return `null` (`GetRootFolder` → `IOutlookFolderAdapter?`). |
| OutlookFolderHierarchyRecord.cs | none | iface-enum-dto | low | Plain DTO with `RequireText` guards already in place; `ParentEntryId`/`RelativePath` already default to `string.Empty`. |
| OutlookFolderNotificationSink.cs | none | com-exempt (mixed) | moderate | 498 lines (near the 500-line limit; do not let annotation edits push it over — if it does, flag rather than split). Most public members `[ExcludeFromCodeCoverage]`, but `AddStoreSubscriptions`/`IsStoreHooked`/`RemoveStore` are explicitly NOT excluded and are exercised by `OutlookFolderNotificationSinkTests.cs` + `FakeOutlookFolderNotificationSink.cs`/`FolderTreeNotificationFakeTests.cs`. |
| OutlookFolderTreeService.cs | none | testable-domain | moderate | Depends only on `FolderTreeSnapshotBuilder`/`IOutlookFolderNotificationSink` (no direct Interop types) — host-neutral, not COM-exempt. Several nullable-flow fields (`_snapshot`, `_inFlightSnapshot`, `_scheduledRefresh`, `_pendingRefreshRequest`) start `null`. Extensive existing tests (`OutlookFolderTreeServiceConcurrencyTests.cs`, `...DisposalTests.cs`, `...InvalidationTests.cs`, `...ScopeTests.cs`, `...StateTests.cs`). |
| PercentageFormatter.cs | enabled | iface-enum-dto | none | Verify-only. |
| WpfDispatcherYield.cs | none | com-exempt | none | Already `[ExcludeFromCodeCoverage]`; thin WPF `Dispatcher.Yield` wrapper, no nullable surface. |

### 1.4 Full inventory table — `Store/`

| File | Pragma | Class | CS86xx pressure | Notes |
| --- | --- | --- | --- | --- |
| DisabledStoreRow.cs | none | iface-enum-dto | low | Plain mutable DTO (`StoreIdentity Identity`, `string DisplayName`, `string ScopeLabel`, `bool IsFutureSession`); auto-properties need init discipline or `= null!`/non-null default. |
| DisabledStoresController.cs | none | testable-domain | moderate | `internal IDisabledStoresViewer Viewer { get; set; }` and `internal List<DisabledStoreRow> Rows { get; set; } = new();` — `Viewer` nullable by design (unset until `Launch()`); `Launch()` itself is `[ExcludeFromCodeCoverage]`. Has `DisabledStoresControllerTests.cs`. |
| DisabledStoresViewer.cs | none | com-exempt | low | `Form`-derived WinForms shell (exempt per CLAUDE.md); `DataGridView Dgv` backing field. |
| DisabledStoresViewer.Designer.cs | none (recommend leave oblivious) | designer-exempt | n/a | Designer-generated code-behind; per the `#364` HelperClasses precedent (`DvgForm.Designer.cs` handling), the default recommendation is to leave Designer files non-opted-in since `#nullable` is lexical/per-file and will not cross-block the opted-in `DisabledStoresViewer.cs`. Flagged for the maintainer (see §5) rather than pragma-annotated by default. |
| IDisabledStoresViewer.cs | none | iface-enum-dto | none | `internal interface IDisabledStoresViewer : IForm` — no nullable-sensitive members beyond `BindRows(IList<DisabledStoreRow> rows)`. |
| IStoreWrapperViewer.cs | none | iface-enum-dto | none | WinForms control-typed properties (`Label`, `Button`, `ComboBox`, `CheckBox`) — all non-null by contract once the form is constructed. |
| StoreDisableService.cs | none | testable-domain | moderate | `StoreDisableService(IApplicationGlobals globals, IStoreRehookService rehook = null)` — `rehook` nullable-by-default param; `GetModelOrNull()` explicitly returns `StoresWrapper?`. Has `StoreDisableServiceTests.cs`. |
| StoreFilterAttribution.cs | none | testable-domain | moderate | Static pure decision function with many `string`/`IList<string>`/`IReadOnlyCollection<string>` parameters that are documented "may be null" (`storeId`, `displayName`, `filePath`, `excludedStoreIds`, etc.) — every one needs an explicit `?`. Deliberately NOT `[ExcludeFromCodeCoverage]` per its own doc comment; has `StoreFilterAttributionTests.cs`. |
| StoreIdentity.cs | none | testable-domain | low | `readonly struct`; `Resolve(string displayName, string filePathFallback = null)` needs `string?` on the optional param; `Resolve(Outlook.Store store)` overload directly touches `Outlook.Store` (COM interface) inside try/catch — Moq-mockable, has `StoreIdentityTests.cs`. |
| StoreLaunchReadinessEvaluator.cs | none | testable-domain | low | `internal static` evaluator; `globals?.Ol?.StoresWrapper` null-conditional chain already in place. |
| StoreLockupAttribution.cs | none | testable-domain | low | Single static `FormatLine` with an `identity` param documented nullable; deliberately not `[ExcludeFromCodeCoverage]`; has `StoreLockupAttributionTests.cs`. |
| StoreRehookResult.cs | enabled | testable-domain | none | Verify-only; already `#nullable enable` (mid-file, after `using System;`, not line 1 — cosmetically different from the rest of the cluster but functionally identical). `sealed record` with hand-written properties (no positional/`init` syntax), confirmed net481-safe. |
| StoreWrapper.cs | none | testable-domain | high | Many properties (`DisplayName`, `StoreId`, `InnerStore`, `Inbox`, `RootFolder`, `UserEmailAddress`, `GlobalAddressBook`) populated only inside `Init()`/`Restore()`, not the ctor (CS8618 risk); `GetSmtpAddressFromStore()` explicit `catch (COMException)` returning `null` from a `string`-declared method; directly depends on `Outlook.Store`/`Outlook.Folder`/`Outlook.AddressEntry` (COM interfaces, Moq-mockable — has `StoreWrapperTests.cs`, so NOT COM-exempt). Also consumes `UtilitiesCS.Threading.CurrentStoreContext` (cross-cluster dependency, see §3). |
| StoreWrapperController.cs | none | testable-domain | high | `internal readonly struct StoreLaunchReadiness` already has a documented `#pragma warning disable CS8625`/`restore` workaround for its own null sentinel (evidence the file is nullable-adjacent already); many `internal` viewer-bound fields (`ArchiveOutlook`, `ArchiveFS`, `JunkEmail`, `JunkPotential`) nullable by design; `Launch()`/`SelectFsFolder()` are `[ExcludeFromCodeCoverage]` (WinForms/dialog glue) but the bulk of the class (`AnyChanges`, `PopulateWithCurrent`, `SaveChanges`, `ApplyExcludeStoreSelection`, etc.) is host-neutral and heavily tested (`StoreWrapperControllerTests.cs`, `StoreWrapperController_Tests.cs`, `...ButtonAndPopulate.cs`, `...ExcludeStore.cs`, `...Launch.cs`). Note: the existing `#pragma warning disable CS8625` inside `StoreLaunchReadiness.NotReady` should be re-evaluated once the file opts in — the file-level pragma may make the disable/restore pair redundant or may need to move/stay depending on flow analysis; do not remove it without confirming a rebuild is still clean. |
| StoreWrapperInitClock.cs | none | testable-domain | none | Static `long` accumulator, `Interlocked`-guarded; no reference-type fields. |
| StoreWrapperInitProbe.cs | none | testable-domain | low | Ctor guards `emit` non-null already; `storeDisplayName` params documented nullable (`?? "<null>"` pattern already present) — needs `string? storeDisplayName`. |
| StoreWrapperViewer.cs | none | com-exempt | low | `Form`-derived WinForms shell (exempt); `StoreWrapperController Controller { get; set; }` nullable by design (guarded with `Controller?.` everywhere). |
| StoreWrapperViewer.Designer.cs | none (recommend leave oblivious) | designer-exempt | n/a | Designer-generated code-behind; same recommendation as `DisabledStoresViewer.Designer.cs` — leave non-opted-in by default, flag the epic-scope conflict if the maintainer wants full opt-in coverage. |
| StoresWrapper.cs | none | testable-domain | high | `partial class StoresWrapper : SmartSerializable<StoresWrapper>` (cross-cluster base type, `ReusableTypeClasses`, oblivious today — see §3); many `[JsonProperty]` list properties default-initialized (`= []`) but `Globals` (`IApplicationGlobals`) is not; `[OnDeserialized]` callback pattern; directly enumerates `Outlook.Store`/`Outlook.Stores` (COM interfaces, Moq-mockable — has `StoresWrapperTests.cs`, `StoresWrapperDisableTests.cs`, `StoresWrapperRehookTests.cs`, `StoresWrapperTests.StoreIdExclusion.cs`). MUST be remediated in the same batch as `StoresWrapper.Filtering.cs` (partial class, see §2). |
| StoresWrapper.Filtering.cs | none | testable-domain | moderate | Static `StoreIsIncluded` overload with several nullable-by-default parameters (`storeId = null`, `excludedStoreIds = null`) mirroring `StoreFilterAttribution.Decide`; directly touches `Outlook.Store` inside try/catch. Partial-class member of `StoresWrapper` — same batch as `StoresWrapper.cs`. |

## 2. Partial-class groups (MUST remediate together)

Verified via `grep "partial class"` across `UtilitiesCS/OutlookObjects/Folder` and
`UtilitiesCS/OutlookObjects/Store` (Table/Conversation/MailItem partials found in the same
grep belong to the sibling Wave-1 child `utilitiescs-nullable-outlook-mailitem-item` and are
out of scope here):

1. **`FolderPredictor.cs` + `FolderPredictor.IFolderSearchHandler.cs`** — one partial type
   (`public partial class FolderPredictor`). The second file's own doc comment states it exists
   specifically so `FolderPredictor.cs` (974 lines, already over the 500-line cap) is not touched
   beyond the one-word `partial` edit. Must ship in the same commit/batch; annotating one without
   the other risks inconsistent member-level null-state assumptions across the two parts (e.g. a
   field's nullability declared in one part must be visible to code in the other part).
2. **`StoresWrapper.cs` + `StoresWrapper.Filtering.cs`** — one partial type
   (`public partial class StoresWrapper : SmartSerializable<StoresWrapper>`). The `.Filtering.cs`
   file's own doc comment states it was relocated from `StoresWrapper.cs` specifically to keep
   the main file within the 500-line limit after the #328 StoreID-exclusion additions. Both share
   the `ShouldIncludeStore`/`StoreIsIncluded` decision logic pattern and must be annotated
   consistently (same nullable shape for `storeId`, `excludedStoreIds`, `displayName`, `filePath`
   parameters across both the instance and static overloads).
3. **`StoreWrapperViewer.cs` + `StoreWrapperViewer.Designer.cs`** — Designer/code-behind pair
   (`public partial class StoreWrapperViewer : Form, IStoreWrapperViewer` + generated
   `partial class StoreWrapperViewer`). Not a "must annotate together" pair in the CS86xx sense
   (Designer files are recommended to stay oblivious, see §1.4/§5), but they must be considered
   together when planning batches since a hand-edit to the Designer file is prohibited by
   convention.
4. **`DisabledStoresViewer.cs` + `DisabledStoresViewer.Designer.cs`** — same Designer/code-behind
   pattern as (3).

No `FolderTree*`-prefixed files share a partial-class relationship despite the similar naming;
each (`FolderTree`, `FolderTreeSnapshot`, `FolderTreeSnapshotBuilder`, `FolderTreeSnapshotNode`,
`FolderTreeSnapshotChangedEventArgs`, `FolderTreeSnapshotQueries`, `FolderTreeStateModel`,
`FolderTreeCompatibilityView`, `FolderTreeRequest`, `FolderTreeSelectionOverlay`,
`FolderTreeNodeKey`, `FolderTreeRefreshReason`) is a distinct, independently-declared class,
struct, or enum — confirmed by reading every file (none declares `partial`).

## 3. Cross-cluster dependency findings (important for sequencing)

The delegation brief asked specifically which upstream `#363`/`#364` members this cluster
consumes, and whether those upstream annotations are present on this worktree. Findings:

### 3.1 `#363` (`utilitiescs-nullable-extensions`) and `#364` (`utilitiescs-nullable-helperclasses`) are NOT yet applied in this worktree

Grepping `#nullable enable` in `UtilitiesCS/Extensions/StringExtensions.cs`,
`UtilitiesCS/Extensions/IEnumerableExtensions.cs`, `UtilitiesCS/HelperClasses/FileSystem/FilePathHelper.cs`,
`UtilitiesCS/HelperClasses/Logging/VerboseLogger.cs`, and `UtilitiesCS/HelperClasses/Tokenizer.cs`
returns **no matches** — none of these upstream files currently carry the pragma in this
worktree. Both `#363` and `#364` are `Status: Draft` specs, not yet executed. The delegation
brief's statement that "those upstream annotations are present on the integration branch this
feature is based on" describes the epic's intended sequencing (Wave 0 completes before Wave 1
executes), not the current state of this worktree. **This feature's actual execution should not
start until `#363` and `#364` land**, or its remediation will be annotating against oblivious
(non-nullable) upstream signatures — which does not produce incorrect CS86xx today (oblivious-in
means no flow-analysis signal, not a false negative that later breaks), but does mean any
annotation decisions made now about upstream call sites would need to be re-verified once the
real upstream annotations arrive, particularly for `FilePathHelper`'s null-by-design sentinel
split (`FilePath`/`FolderPath`/`FileName` non-null vs. `FileStemSeed`/`FileStemSuffix`/`FileStem`/
`FileExtension` nullable, per `#364`'s spec) and `Initializer.GetOrLoad`/`SetAndSave`.

### 3.2 Specific `#363`/`#364` members this cluster calls

Confirmed by reading the Folder/Store source and grepping definition sites:

| Member | Defined in | Consumed by (this cluster) | Relevance |
| --- | --- | --- | --- |
| `string.IsNullOrEmpty()`/`.IsNullOrEmpty()` extension | `UtilitiesCS/Extensions/StringExtensions.cs` (`#363`, Batch B) | `FolderConverter.cs`, `FolderPredictor.cs` (`folderName.IsNullOrEmpty()`, `result.IsNullOrEmpty()`, `olAncestor.IsNullOrEmpty()`) | Null-check helper; its own nullable annotation (`bool IsNullOrEmpty(this string? value)`) determines whether call sites narrow correctly. |
| `.ToLazy()`/`.ToLazyValue()` | `UtilitiesCS/Extensions/LazyExtension.cs` (`#363`, Batch B) | `FolderMinimalWrapper.cs`, `FolderWrapper .cs` (`_lazyName = value?.ToLazy()`, `_lazyItemCount = value.ToLazyValue()`, etc.) | Determines whether `Lazy<string>`/`Lazy<int>` wrapper fields are correctly nullable-typed. |
| `.ForEach(...)` | `UtilitiesCS/Extensions/IEnumerableExtensions.cs` (`#363`, Batch C) | `FolderTree.cs` (`_roots.ForEach(...)`), `FolderScorer.cs` (`folders.ForEach(...)`) | Generic extension over `IEnumerable<T>`; annotation affects whether the lambda parameter is nullable. |
| `.SentenceJoin()` | `UtilitiesCS/Extensions/IEnumerableExtensions.cs` (`#363`, Batch C) | `FolderConverter.cs`, `FolderPredictor.cs` (error-message formatting of illegal-character arrays) | Same generic-extension consideration; low-risk (string formatting only). |
| `AsTokenPattern()` | `UtilitiesCS/HelperClasses/Tokenizer.cs` (`#364`, Batch 2 — Logging cluster groups Tokenizer with TraceUtility per the spec's phase list, though Tokenizer.cs itself is listed under root pure/simple helpers in Batch 1) | `FolderScorer.cs` (`_wordChars.AsTokenPattern()`, `Tokenizer.GetRegex(...)`) | Regex-construction helper feeding `FolderScorer`'s `_tokenizerRegex` field. |
| `VerboseLogger<T>` | `UtilitiesCS/HelperClasses/Logging/VerboseLogger.cs` (`#364`, Batch 2) | `FolderScorer.cs` (`VerboseLogger<FolderScorer> _verboseLogger = new(); public VerboseLogger<FolderScorer> Vlog => _verboseLogger;`) | Generic logging helper; `LogObject`/`IsVerbose` signatures affect `FolderScorer`'s own nullable surface only indirectly (no null args currently observed). |
| `FilePathHelper` | `UtilitiesCS/HelperClasses/FileSystem/FilePathHelper.cs` (`#364`, Batch 8 — highest-contract-sensitivity, done last in `#364`'s own sequencing) | `StoreWrapper.cs` (`public FilePathHelper ArchiveFsRoot { get; set; } = new();`) | Directly consumes the null-by-design sentinel split (`FileStemSeed`/etc. nullable) `#364`'s spec calls out as "the crux of the file." `StoreWrapperController.GetRelativeFsPath()` reads `Current.ArchiveFsRoot.FolderPath` — must match `#364`'s eventual `FolderPath` non-null decision. |

### 3.3 Dependencies NOT covered by `#363`/`#364` at all — flag for spec authors

The epic manifest declares `depends_on: [utilitiescs-nullable-extensions, utilitiescs-nullable-helperclasses]`
for this feature (issue-placeholder 9007), but several types this cluster consumes live in
neither cluster:

- **`TreeNode<T>`** (`UtilitiesCS/ReusableTypeClasses/Other/TreeNodeOfT.cs`) — consumed by
  `FolderHierarchyBuilder.cs`, `FolderTreeStateModel.cs`, `FolderTree.cs`,
  `FolderTreeCompatibilityView.cs`, and all five `FolderWrapper*Comparer.cs` files. Belongs to
  the Wave-0 sibling `utilitiescs-nullable-reusabletypes` (epic placeholder 9003), not `#363`/`#364`.
  Confirmed **not yet** `#nullable enable` in this worktree.
- **`ScoDictionaryNew<TKey,TValue>`** and **`SmartSerializable<T>`**
  (`UtilitiesCS/ReusableTypeClasses/SerializableNew/...`, `UtilitiesCS/ReusableTypeClasses/NewSmartSerializable/...`)
  — consumed by `FolderScorer.cs` (`_folderNameScores`) and `StoresWrapper.cs`
  (`: SmartSerializable<StoresWrapper>`) respectively. Same Wave-0 sibling (9003), not yet
  annotated.
- **`ProgressTracker`** and **`CurrentStoreContext`** (`UtilitiesCS/Threading/`) — consumed by
  `FolderTree.cs` (`ProgressTracker progress`) and `StoreWrapper.cs`/`StoresWrapper.cs`
  (`CurrentStoreContext.Begin(...)`). Belongs to the Wave-0 sibling `utilitiescs-nullable-threading`
  (epic placeholder 9005), not `#363`/`#364`. Confirmed **not yet** `#nullable enable`.
- **`AsyncLazy<T>`** (`UtilitiesCS/ReusableTypeClasses/AsyncLazy/AsyncLazy.cs`) — consumed by
  `FolderWrapper .cs` (`ItemHelpers` property). Same Wave-0 sibling (9003).
- **`FilePathHelperConverter`** (`UtilitiesCS/NewtonsoftHelpers/FilePathHelperConverter.cs`) —
  consumed by `StoreWrapperController.cs` (`new FilePathHelperConverter(Globals.FS)`). Belongs to
  the Wave-0 sibling `utilitiescs-nullable-newtonsofthelpers` (epic placeholder 9004), not `#363`/`#364`.

**Why this does not block independent merge**: because enforcement is per-file pragma (not
project-level), none of these oblivious upstream types cross-block this cluster's own opted-in
files — the epic's own stated architecture ("Files that are not yet remediated remain
non-opted-in and are not cross-blocking") holds. The finding is a **documentation/precision** gap
in the epic manifest's `depends_on` list, not a hard execution blocker: this feature CAN be
opted in before `9003`/`9004`/`9005` complete, but annotation choices touching `TreeNode<T>`,
`ScoDictionaryNew<>`, `SmartSerializable<T>`, `ProgressTracker`, `CurrentStoreContext`, and
`AsyncLazy<T>` members will necessarily treat those external members as oblivious (no
compiler-enforced null contract) until those siblings land, which reduces this feature's own
CS86xx pressure at those call sites but also means the annotations chosen here cannot be
verified against a real upstream contract yet. Recommend the spec/plan authors add these four
folders as informational (non-blocking) dependency notes, and re-run the pragma-gate build after
`9003`/`9004`/`9005` land to confirm no new CS86xx appears at those call sites once the upstream
types stop being oblivious.

## 4. Proposed leaf-first batch ordering

Rationale mirrors the pattern already used by `#363`'s and `#364`'s specs: interfaces/enums/DTOs
first (near-zero pressure, unblock later batches), then host-neutral/testable domain types
(moderate pressure, already covered by existing tests), then COM-adjacent/WinForms-shell types
last (thin, mostly `[ExcludeFromCodeCoverage]`, but conventionally deferred so any COM-boundary
`!`-justification decisions are made once the underlying domain types are settled).

### Batch F0 — Folder interfaces, enums, and trivial DTOs (leaf-first; unblocks F1-F3)
`IDeadlineClock.cs`, `IDispatcherYield.cs`, `IFolderHandleResolver.cs`,
`IFolderHierarchyProvider.cs`, `IFolderSearchHandler.cs`, `IOutlookFolderHierarchyReader.cs`,
`IOutlookFolderNotificationSink.cs`, `IOutlookFolderTreeService.cs`,
`FolderTreeRefreshReason.cs`, `FolderRow.cs`, `FolderScore.cs`, `FolderBreadcrumbSegment.cs`,
`FolderTreeSnapshotChangedEventArgs.cs`, `OutlookFolderHierarchyRecord.cs`.
- Hard ordering constraint: `IFolderSearchHandler.cs` must be annotated with the exact same
  nullable parameter shape that `FolderPredictor.cs` (Batch F3) will use for its narrow seam
  implementation (`FolderArray`, `Suggestions`, `FolderRowArray`, `FindFolder(...)`); decide this
  interface's nullable shape before or in lockstep with `FolderPredictor.cs`, since the interface
  defines the contract `FolderPredictor` implements.

### Batch F1 — Folder value/key types and comparers (low pressure; depends only on F0 or nothing)
`FolderTreeNodeKey.cs`, `FolderTreeRequest.cs`, `FolderTreeSelectionOverlay.cs`,
`FolderNodeViewModel.cs`, `DeadlineClock.cs` (implements `IDeadlineClock` from F0),
`FolderWrapperNameComparer.cs`, `FolderWrapperNameCountSizeComparer.cs`,
`FolderWrapperNameAndParentNameComparer.cs`, `FolderWrapperNodeComparer.cs` (composes the
previous two — order within F1: `NameComparer`/`NameCountSizeComparer` before `NodeComparer`/
`NodeContentsComparer`, since the latter two instantiate the former internally),
`FolderWrapperNodeContentsComparer.cs`.

### Batch F2 — Folder tree snapshot family (moderate pressure; depends on F0/F1 key types)
`FolderTreeSnapshotNode.cs` (depends on `FolderTreeNodeKey` from F1),
`FolderTreeSnapshot.cs` (depends on `FolderTreeSnapshotNode`, `FolderTreeNodeKey`,
`FolderTreeRequest`), `FolderTreeSnapshotQueries.cs` (depends on `FolderTreeSnapshot`),
`FolderTreeSnapshotBuilder.cs` (depends on `IOutlookFolderHierarchyReader`, `IDeadlineClock`,
`IDispatcherYield` from F0), `FolderTreeCompatibilityView.cs` (depends on `FolderTreeSnapshot`,
`FolderTreeSelectionOverlay`, plus external `TreeNode<T>` and `FolderWrapper` — see ordering note
below), `FolderTreeStateModel.cs` (depends on external `TreeNode<T>`, `FolderNodeViewModel` from
F1), `FolderHierarchyBuilder.cs` (depends on `FolderRow`/`FolderScore` from F0, `FolderNodeViewModel`
from F1, external `TreeNode<T>`).
- Ordering note: `FolderTreeCompatibilityView.cs` and `FolderHierarchyBuilder.cs`/`FolderTreeStateModel.cs`
  consume the cross-cluster `TreeNode<T>` (§3.3); no hard ordering constraint against this
  cluster's own files, but annotate these last within F2 so the `TreeNode<T>`-touching call
  sites are reviewed together.

### Batch F3 — Folder domain/COM-adjacent testable classes (moderate–high pressure; largest batch)
`FolderConverter.cs`, `FolderNavigator.cs`, `FolderMinimalWrapper.cs`,
`FolderWrapper .cs` (space-in-filename hazard, §1.2), `FolderTree.cs` (depends on `FolderWrapper`
and the five comparers from F1, plus external `ProgressTracker`), `FolderScorer.cs` (depends on
external `ScoDictionaryNew<>`, `VerboseLogger<T>`, `Tokenizer` — §3.2/§3.3),
`FolderPredictor.cs` + `FolderPredictor.IFolderSearchHandler.cs` (single commit, §2; depends on
`FolderScorer.cs`, `IFolderSearchHandler.cs` from F0, `FolderRow.cs`/`FolderScore.cs` from F0),
`FolderProbabilityAdapter.cs`/`FolderSuggestionTree.cs`/`FolderSuggestionNode.cs`/
`IFolderProbabilitySource.cs`/`FolderBreadcrumbBridgeRouter.cs`/breadcrumb files/`PercentageFormatter.cs`
are already enabled — **verify-only**, run after F0-F2 land to confirm no regression from the
newly-opted-in neighbors' signature changes (e.g. `FolderProbabilityAdapter` consumes
`FolderSuggestionTree`/`FolderSuggestionNode`, both already enabled, so no new risk expected, but
a rebuild should confirm).
- Hard ordering constraint: `FolderScorer.cs` must precede `FolderPredictor.cs` (the latter holds
  a `FolderScorer Suggestions` field and calls its members directly).

### Batch F4 — Folder host-neutral facade/service layer (moderate pressure; depends on F2)
`OutlookFolderHierarchyProvider.cs` (depends on `IOutlookFolderTreeService` from F0,
`FolderTreeSnapshotQueries`/`FolderTreeNodeKey`/`FolderBreadcrumbSegment` from F0-F2),
`OutlookFolderTreeService.cs` (depends on `FolderTreeSnapshotBuilder` from F2,
`IOutlookFolderNotificationSink` from F0).

### Batch F5 — Folder COM-boundary adapters (low–moderate pressure; last, thin/mostly-exempt)
`OutlookFolderHandleResolver.cs`, `OutlookFolderHierarchyReader.cs`,
`OutlookFolderNotificationSink.cs` (watch the 498-line count, §1.3), `MsgToMime/MAPIMethods.cs`,
`WpfDispatcherYield.cs`.

### Batch S0 — Store interfaces and trivial DTOs (leaf-first; unblocks S1-S3)
`IDisabledStoresViewer.cs`, `IStoreWrapperViewer.cs`, `DisabledStoreRow.cs`.

### Batch S1 — Store value types and pure attribution helpers (low–moderate pressure)
`StoreIdentity.cs`, `StoreLaunchReadinessEvaluator.cs`, `StoreFilterAttribution.cs`,
`StoreLockupAttribution.cs`, `StoreWrapperInitClock.cs`, `StoreWrapperInitProbe.cs`.
- `StoreRehookResult.cs` is already enabled — verify-only, no batch placement needed, but
  re-verify after S1/S2 since `StoreDisableService.cs` and the rehook coordinator consume it.

### Batch S2 — Store domain classes (high pressure; largest Store batch)
`StoreWrapper.cs` (depends on external `CurrentStoreContext` — §3.3; depends on
`StoreWrapperInitClock`/`StoreWrapperInitProbe` from S1), `StoresWrapper.cs` +
`StoresWrapper.Filtering.cs` (single commit, §2; depends on `StoreIdentity`/`StoreFilterAttribution`
from S1, `StoreWrapper.cs`, external `SmartSerializable<T>`/`CurrentStoreContext` — §3.3),
`StoreDisableService.cs` (depends on `StoresWrapper.cs`, `StoreIdentity.cs`).

### Batch S3 — Store controllers (high pressure; depends on S2)
`StoreWrapperController.cs` (depends on `StoresWrapper.cs`, `StoreWrapper.cs`, `IStoreWrapperViewer.cs`
from S0, external `FilePathHelperConverter`/`FilePathHelper` — §3.2), `DisabledStoresController.cs`
(depends on `StoreDisableService.cs`, `IDisabledStoresViewer.cs` from S0, `DisabledStoreRow.cs`
from S0).

### Batch S4 — Store WinForms shells (low pressure; last, thin/mostly-exempt)
`StoreWrapperViewer.cs`, `DisabledStoresViewer.cs`.
- `StoreWrapperViewer.Designer.cs`/`DisabledStoresViewer.Designer.cs` are recommended to remain
  non-opted-in (§1.4/§5); no batch action needed unless the maintainer overrides the default.

### Cross-directory ordering
Folder and Store clusters have no direct compile-time dependency on each other **except**:
`OutlookFolderHierarchyReader.cs` (Folder, Batch F5) takes a `StoresWrapper storesWrapper`
constructor parameter and calls `store.ShouldInclude(_storesWrapper)` /
`storesWrapper.ShouldIncludeStore(_store)`. This means **Batch S2 (`StoresWrapper.cs` +
`StoresWrapper.Filtering.cs`) must land before Batch F5 (`OutlookFolderHierarchyReader.cs`)**.
Recommended overall sequence: F0 -> F1 -> F2 -> F3 -> F4 -> S0 -> S1 -> S2 -> F5 -> S3 -> S4 (F5
deferred until after S2 specifically for this one file; F4 does not depend on Store and can run
before or interleaved with S0/S1).

## 5. Verification commands

Per the epic's confirmed architecture and both upstream Wave-0 specs, verification for this
child MUST use the pragma-only nullable gate, NOT `/p:Nullable=enable` globally:

1. **Format**: `dotnet tool run csharpier .` (or `csharpier .` if installed globally). Remember
   the space-in-filename hazard (§1.2) if ever invoking csharpier on an explicit path rather than
   the recursive `.` form.
2. **Analyzer/codestyle build**:
   `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
3. **Nullable gate (pragma-only, per-file enforcement)**:
   `msbuild TaskMaster.sln /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU" /p:TreatWarningsAsErrors=true`
   — deliberately omits `/p:Nullable=enable`. Confirmed: `UtilitiesCS.csproj` has no `<Nullable>`
   element (grepped, no match), so this command relies entirely on each file's own pragma. Use
   `/t:Rebuild` (not `/t:Build`) per PR #361's fix to avoid a silently-skipped incremental build.
4. **Test**: `vstest.console.exe <UtilitiesCS.Test assembly path> /EnableCodeCoverage`.
   - Test project: `UtilitiesCS.Test/UtilitiesCS.Test.csproj`, `AssemblyName` =
     `UtilitiesCS.Test`, old-style (non-SDK) csproj targeting `TargetFrameworkVersion v4.8.1`
     with `OutputPath` = `bin\Debug\` (Debug config). Expected assembly path:
     `UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll` (no TFM-named subfolder, since this is an
     old-style `.csproj`, not SDK-style).
   - Existing test classes that cover this cluster and must show no regression (confirmed via
     `Glob` under `UtilitiesCS.Test/OutlookObjects/Folder/` and `UtilitiesCS.Test/OutlookObjects/Store/`,
     ~80 files total): all files listed as "Has `...Tests.cs`" in §1.3/§1.4, plus the
     `UtilitiesCS.Test/OutlookObjects/Folder/Fakes/` fakes (`FakeDeadlineClock.cs`,
     `FakeDispatcherYield.cs`, `FakeFolderHandleResolver.cs`, `FakeFolderHierarchyRecord.cs`,
     `FakeOutlookFolderHierarchyReader.cs`, `FakeOutlookFolderNotificationSink.cs`) that the
     COM-adjacent Folder classes' tests depend on.

Do NOT pass `/p:Nullable=enable` for this feature's verification, for the same documented reason
as `#363`/`#364`: it would force nullable project-wide and surface the full ~2131-diagnostic epic
debt as false failures unrelated to this child. This is the same rules-vs-convention conflict
flagged at the epic level (`.claude/rules/csharp.md` documents the global-flag form); it is not
resolved here and no `.claude/rules/*` file should be edited.

## 6. Risks and hazards specific to this cluster

1. **Space-in-filename** (`FolderWrapper .cs`) — see §1.2. Quote all tooling invocations that
   reference the path explicitly; do not rename.
2. **Pre-existing >500-line files** (do NOT split — annotation-only scope): `FolderPredictor.cs`
   (974 lines), `FolderScorer.cs` (663 lines), `FolderWrapper .cs` (531 lines). Flag all three as
   pre-existing policy exceptions in the feature docs, matching the precedent set by `#363`
   (`ArrayExtensions.cs`, 544 lines) and `#364` (`PrettyPrint.cs`, 677 lines).
3. **Near-limit file**: `OutlookFolderNotificationSink.cs` (498 lines). Annotation edits (adding
   `?`/`!`/pragma line) could push it just over 500; if so, flag rather than split, consistent
   with the `#364` spec's `FilePathHelper.cs` (494 lines) precedent.
4. **`= default`-field / struct patterns needing `= default!`**: `FolderScorer.FolderScoring`
   (`internal struct` with `string FolderPath; string FolderName; int[] FolderEncoding; int[] FolderWordLengths; int Score;` — all reference-type fields are implicitly default-initialized when the struct is default-constructed via `new FolderScoring { ... }` object-initializer syntax, which always sets every field explicitly in current call sites, so no `= default!` should be needed in practice, but verify each construction site sets every reference field). No `record`/`record struct`/`init` conversions are present or should be introduced anywhere in this cluster (confirmed: `FolderRow`, `FolderScore`, `StoreIdentity` are already plain `readonly struct`; `StoreRehookResult` is already a hand-written `sealed record` with constructor-set get-only properties, not positional/`init` syntax — net481-safe, matches the `ResourceTimingRow`/`FolderHierarchyNode` precedent cited in its own doc comment).
5. **COM event-handler null-flow**: `OutlookFolderNotificationSink.cs`'s nested
   `StoresNotificationSubscription.OnStoreAdd(Outlook.Store store)` and
   `OnBeforeStoreRemove(Outlook.Store store, ref bool cancel)` read `store?.StoreID` — already
   null-conditional, low risk. `FoldersNotificationSubscription`'s `OnFolderAdd`/`OnFolderChange`/
   `OnFolderRemove` handlers take Interop delegate signatures that are fixed by the COM event
   contract (cannot be changed) — annotate the local variable, not the delegate signature, if a
   mismatch arises.
6. **Global-flag-vs-per-file-pragma conflict**: `.claude/rules/csharp.md` documents forcing
   `/p:Nullable=enable` globally for the type-check stage, which conflicts with this feature's
   (and the whole epic's) per-file opt-in convention. This is the same conflict `#363` and `#364`
   flag at the epic level. **Out of scope to resolve here; do not edit `.claude/rules/*`.** Carry
   the same flag forward into this feature's spec.
7. **Designer-file opt-in conflict**: the epic lists `Store/DisabledStoresViewer.Designer.cs` and
   `Store/StoreWrapperViewer.Designer.cs` as in-scope files (they are `.cs` files under `Store/`),
   but the repo convention (and `#364`'s explicit `DvgForm.Designer.cs` precedent) is to leave
   Designer files non-opted-in by default since `#nullable` is lexical/per-file and the generated
   code produces no CS8618/CS8625 either way. Recommend the same default here: leave both
   Designer files oblivious, and flag the epic-scope-vs-convention conflict for the maintainer
   rather than pragma-annotating generated code. If the maintainer requires full opt-in, the
   fallback (per `#364`'s own fallback) is annotating only the generated `IContainer` field
   (`private System.ComponentModel.IContainer? components = null;`) without touching
   `InitializeComponent`.
8. **`FolderPredictor.IFolderSearchHandler.cs` triviality**: this file has zero members (`{ }`
   empty partial-class body) — it will never itself emit CS86xx, but it still needs the pragma
   added (for consistency and to avoid a future contributor assuming it was skipped) and must be
   committed together with `FolderPredictor.cs` (§2).
9. **Sequencing gap vs. epic `depends_on`**: this feature transitively consumes types from
   `ReusableTypeClasses` (`TreeNode<T>`, `ScoDictionaryNew<>`, `SmartSerializable<T>`,
   `AsyncLazy<T>` — epic placeholder 9003), `Threading` (`ProgressTracker`,
   `CurrentStoreContext` — epic placeholder 9005), and `NewtonsoftHelpers`
   (`FilePathHelperConverter` — epic placeholder 9004), none of which are declared in the epic
   manifest's `depends_on: [utilitiescs-nullable-extensions, utilitiescs-nullable-helperclasses]`
   for this feature. Per-file architecture means this is not a hard blocker, but the spec/plan
   authors should record it as an informational note and re-run the pragma gate after those
   siblings land (§3.3).

## 7. Citations

- Upstream contracts consumed: `docs/features/active/2026-07-18-utilitiescs-nullable-extensions-363/spec.md`
  (issue #363) and `docs/features/active/2026-07-18-utilitiescs-nullable-extensions-363/plan.2026-07-18T21-20.md`;
  `docs/features/active/2026-07-18-utilitiescs-nullable-helperclasses-364/spec.md` (issue #364) and
  `docs/features/active/2026-07-18-utilitiescs-nullable-helperclasses-364/plan.2026-07-18T21-21.md`.
- Epic manifest: `docs/features/epics/utilitiescs-nullable-remediation/epic.md` (Wave 1 table,
  `~29` file estimate for `utilitiescs-nullable-outlook-folder-store`, `depends_on` edges, and the
  rules-vs-convention conflict framing reused in §6 item 6).
- This feature's own scaffolds (currently template placeholders, not yet filled in):
  `docs/features/active/2026-07-18-utilitiescs-nullable-outlook-folder-store-365/spec.md`,
  `.../issue.md`, `.../plan.2026-07-18T22-03.md`, `.../user-story.md`.
