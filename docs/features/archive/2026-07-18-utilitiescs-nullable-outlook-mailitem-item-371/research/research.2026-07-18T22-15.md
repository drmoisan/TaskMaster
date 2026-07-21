# Research — utilitiescs-nullable-outlook-mailitem-item (#371)

- Timestamp: 2026-07-18T22-15
- Scope verified: all `.cs` files under `UtilitiesCS/OutlookObjects/{MailItem,Item,Conversation,Attachment,Table}/` (30 files total: MailItem 12, Item 9, Conversation 2, Attachment 2, Table 5). Every file listed below was read in full.
- Upstream specs read in full: `docs/features/active/2026-07-18-utilitiescs-nullable-extensions-363/spec.md` (#363), `docs/features/active/2026-07-18-utilitiescs-nullable-helperclasses-364/spec.md` (#364).
- Sibling feature docs read: `spec.md`, `issue.md`, `plan.2026-07-18T22-05.md` (all template placeholders, not yet filled in) for this feature (#371).

## 1. File inventory and one-line role

### MailItem/ (12 files)

| File | Role | `#nullable` state |
|---|---|---|
| `MailItemHelper.cs` | Partial class core: ctors, lazy-field wiring (`InitLazyFields`/`InitializeSafeDefaults`), `INotifyPropertyChanged`. | Oblivious (no pragma) |
| `MailItemHelper.Html.cs` | Partial: HTML/plain-text compression, dark-mode toggling, `GetHtml`. **Contains an internal `#nullable enable` / `#nullable disable` region** wrapping only the `_emailHeader` field/property (lines 107–144); rest of file oblivious. | Partially/inconsistently enabled (verify-and-convert) |
| `MailItemHelper.Loading.cs` | Partial: async materialization (`FromDfAsync`, `FromMailItemAsync`, `ResolveMail`, `LoadPriorityForce`, `LoadRecipients*`). | Oblivious |
| `MailItemHelper.Properties.cs` | Partial: all public lazy-backed properties (`Actionable`, `Body`, `Sender`, `FolderInfo`, `AttachmentsInfo`, etc.). | Oblivious |
| `MailItemHelper.Serialization.cs` | Partial: `ToSerializableObject`/`FromSerializableObject`, `IEquatable<IItemInfo>` implementation, recipient-equivalence helpers. | Oblivious |
| `CaptureEmailAddressesModule2.cs` | Dead code — entire class body is commented out. | Oblivious, trivial |
| `CidImageResolver.cs` | Host-neutral, pure `cid:` image-reference rewriter for HTML bodies. No COM dependency (explicit XML-doc claim, verified — no `Microsoft.Office.Interop.Outlook` using or type reference). | Oblivious |
| `EmailDetails.cs` | Static extension methods building legacy string-array "Details" projections from `MailItem`/`MailItemHelper`; `GetActionTaken`, `GetTriage`. | Oblivious |
| `EmailDetailsWrapper.cs` | Thin injectable wrapper (`IEmailDetailsWrapper`) delegating to `EmailDetails` extension methods — the seam that makes `EmailDetails` testable without a live `MailItem`. | Oblivious |
| `ItemInfo.cs` | `[Serializable]` POCO implementing `IItemInfo`; namespace `UtilitiesCS.EmailIntelligence`. | Oblivious |
| `MailItemExtensions.cs` | `ToMIME`, `TryMoveAsync` extensions on `Outlook.MailItem`. | Oblivious |
| `MailResolution.cs` | `IsMailUnReadable`, `TryResolveMailItem` — tiny, pure classification helpers. | Oblivious |

### Item/ (9 files)

| File | Role | `#nullable` state |
|---|---|---|
| `ItemComparer.cs` | Entire file body is commented out (`//using ... //namespace ...`); no live code. | Oblivious, trivial |
| `OlItemPseudoInterface.cs` | `SetCategories`/`GetCategories`/`NoConflicts` extensions dispatching on `object item is MailItem/TaskItem/AppointmentItem/MeetingItem`. | Oblivious |
| `OlItemSummary.cs` | `[Obsolete]` static summary-extraction helpers over multiple Outlook item types. | Oblivious |
| `OutlookItem.cs` | Late-bound reflection wrapper (`GetPropertyValue<T>`/`SetPropertyValue<T>`/`CallMethod`) around a raw Outlook item `object`. **503 lines — exceeds the repo 500-line limit.** | Oblivious |
| `OutlookItemExtensions.cs` | `Try`/`TryGet` factory extensions plus internal `TryGetPropertyValue`/`TrySetPropertyValue`/`TryCallMethod` reflection helpers shared by `OutlookItem`. | Oblivious |
| `OutlookItemFlaggable.cs` | `OutlookItem` subclass implementing `IOutlookItemFlaggable` (task-flag semantics: `Complete`, `DueDate`, `FlagAsTask`, `TaskStartDate`, `TaskSubject`, `TotalWork`). | Oblivious |
| `OutlookItemFlaggableTry.cs` | `OutlookItemTry` subclass wrapping `IOutlookItemFlaggable` in try/catch-swallowing accessors. | Oblivious |
| `OutlookItemTry.cs` | Try/catch-swallowing decorator implementing `IOutlookItem` over another `IOutlookItem`; internal `TryGet<T>`/`TrySet<T>`/`TryCall<T>` generic helpers returning `default(T)` on failure. | Oblivious |
| `OutlookItemTryGet.cs` | `TryGet`-style (`bool Foo(out T result)`) wrapper over `OutlookItem`; internal `TryGet<T>(Func<T>, out T)` helpers. | Oblivious |

### Conversation/ (2 files — one partial-class group)

| File | Role | `#nullable` state |
|---|---|---|
| `ConversationHelper.cs` | `public static partial class ConvHelper` — `GetMailItemList`, `ConversationCt`, `GetConversationDf`/`GetConversationDfAsync` (multiple overloads with retry/timeout), `FilterConversation`. | Oblivious |
| `ConversationHelper.Formatting.cs` | Same `partial class ConvHelper` — `GetInfoDf`/`GetInfoTable`, `GetDataFrame`/`GetDataFrameAsync`, `GetConversationTable`, column-header formatting (`PadOrTrunc`, `JoinFixedWidth`), `GetConversation`/`ResolveType`. | Oblivious |

Note: the file names say "ConversationHelper" but the class itself is `ConvHelper` — the partial-group identity is by class name (`ConvHelper`), not file name.

### Attachment/ (2 files)

| File | Role | `#nullable` state |
|---|---|---|
| `AttachmentHelper.cs` | Non-partial class wrapping `Attachment` with save/delete path computation (`FilePathHelper`-backed), filename sanitization, `CheckParameters` (two overloads — 3-arg and 4-arg; only the 4-arg overload is called from `Init`, the 3-arg overload appears unreferenced within this file). | Oblivious |
| `AttachmentSerializable.cs` | `[Serializable]` `IAttachment` implementation; lazy byte-fetching (`GetBytes`, `TryFromAccessor`, `TryFromSaveAsLoad`, `TryFromContentIdAccessor`). | Oblivious |

### Table/ (5 files — one partial-class group)

| File | Role | `#nullable` state |
|---|---|---|
| `OlTableExtensions.cs` | `public static partial class OlTableExtensions` — `RemoveColumns`/`AddColumns`, `GetColumnDictionary`, `ExtractData2`, timing helpers. | Oblivious |
| `OlTableExtensions.Etl.cs` | Same partial class — `ETL`/`EtlAsync`/`EtlAsyncOld`, `EtlByRow*` family, `CastToRowArray`, `GetBinFields`/`GetObjectFields`. 468 lines. | Oblivious |
| `OlTableExtensions.RowTransforms.cs` | Same partial class — `WriteValuesToData`, `ToObjectRow`, `ConvertBinColumnsToString`/`ConvertObjectColumnsToString`. | Oblivious |
| `OlTableExtensions.TableAccess.cs` | Same partial class — `GetTableInView(Async)`, `Store`/`MAPIFolder`/`Conversation` `GetTable(Async)`/`TryGetTable(Async)` overload families, `GetRows`, `GetColumnHeaders`, `EnumerateTable`. 427 lines. | Oblivious |
| `OlToDoTable.cs` | Non-partial static class — `GetToDoTable`, `EnsureToDoIdExists`/`EnsureFolderField`/`EnsureItemValues` (uses `dynamic item = itemObj;`). | Oblivious |

**Total verify-only files: 0.** Every file in scope is currently oblivious except `MailItemHelper.Html.cs`, which has a *partial*, non-standard `#nullable enable`/`#nullable disable` region around one field/property only — this is not the epic's file-level pragma convention and must be normalized (converted to a whole-file `#nullable enable` and the interior `#nullable disable` removed) as part of remediating that file, not treated as already-compliant.

## 2. Partial-class groups requiring joint opt-in

Three partial-class groups were confirmed by reading every file and grepping for `partial class`:

1. **`MailItemHelper`** (`public partial class MailItemHelper : INotifyPropertyChanged, IItemInfo`) — 5 files: `MailItemHelper.cs`, `MailItemHelper.Html.cs`, `MailItemHelper.Loading.cs`, `MailItemHelper.Properties.cs`, `MailItemHelper.Serialization.cs`. Field/member split:
   - Backing fields for all lazy properties (`_actionable`, `_body`, `_sender`, `_folderInfo`, `_attachmentsInfo`, `_html`, etc.) are declared in `.Properties.cs` but *initialized* in `MailItemHelper.cs` (`InitLazyFields`/`InitializeSafeDefaults`) and *read/written* from `.Html.cs`, `.Loading.cs`, and `.Serialization.cs`. Nullable field-state analysis (definite assignment) spans all 5 files; splitting the pragma across only some of them would produce inconsistent CS8618 "uninitialized non-nullable field" diagnostics in whichever files are opted in first.
   - `_emailHeader` (declared and used only in `.Html.cs`) is the only field with an existing interior `?` annotation; it must be reconciled with the group's full-file annotation pass.

2. **`ConvHelper`** (`public static partial class ConvHelper`) — 2 files: `ConversationHelper.cs`, `ConversationHelper.Formatting.cs`. No instance fields (static class), but `ConversationHelper.Formatting.cs`'s `GetDataFrameAsync`/`GetConversationTable` call `LogConversationTiming`, which is a `private static` method declared in `ConversationHelper.cs`; cross-file member resolution means both files need consistent nullable parameter/return annotations for the shared logging helper's `string details = null` parameter.

3. **`OlTableExtensions`** (`public static partial class OlTableExtensions`) — 4 files: `OlTableExtensions.cs`, `OlTableExtensions.Etl.cs`, `OlTableExtensions.RowTransforms.cs`, `OlTableExtensions.TableAccess.cs`. Static class, no shared instance fields, but heavy cross-file private-method calls: `EtlByRow` (Etl.cs) calls `WriteValuesToData`/`ToObjectRow`/`ConvertBinColumnsToString`/`ConvertObjectColumnsToString` (RowTransforms.cs); `TableAccess.cs` and `Etl.cs` both call the shared `logger`/`LogTableTiming` declared in `OlTableExtensions.cs`. All four must be annotated together to keep the internal call graph's null-flow assumptions consistent.

No other multi-file partial-class groups exist in this cluster. `AttachmentHelper.cs`/`AttachmentSerializable.cs` are separate, non-partial classes despite being in the same directory (do not treat as a group).

## 3. Risk and COM-boundedness classification

**COM-bound (directly depend on `Microsoft.Office.Interop.Outlook` types without an injectable seam) — coverage-exempt per repo policy, annotate for null-safety only:**

All 30 files except `CidImageResolver.cs` reference `Microsoft.Office.Interop.Outlook` types directly (verified by grep restricted to the 5 in-scope directories: 28 of 30 files match a literal `using`/type reference; the remaining 2 non-matching files are `CidImageResolver.cs` — genuinely COM-free — and `MailItemHelper.Html.cs`, which has no direct `Microsoft.Office.Interop.Outlook` import but shares the `MailItemHelper` partial class's `_item` field of type `MailItem`, so it inherits the group's COM-boundedness). Within this set:

- **High-risk / high-annotation-density** (large surface, many nullable decisions, no seam): `MailItemHelper.*` (5-file group), `OutlookItem.cs`, `OutlookItemExtensions.cs`, `OutlookItemFlaggable.cs`, `OutlookItemTry.cs`, `OutlookItemTryGet.cs`, `EmailDetails.cs`, `OlTableExtensions.*` (4-file group), `ConvHelper` (2-file group), `AttachmentHelper.cs`, `AttachmentSerializable.cs`.
- **Lower-risk COM-bound** (small surface, few members): `MailResolution.cs`, `MailItemExtensions.cs`, `OlItemPseudoInterface.cs`, `OlItemSummary.cs` (also `[Obsolete]`), `OlToDoTable.cs`, `OutlookItemFlaggableTry.cs`, `ItemInfo.cs` (POCO, no COM calls itself but implements the COM-adjacent `IItemInfo` contract).
- **Trivial / effectively inert**: `CaptureEmailAddressesModule2.cs` and `ItemComparer.cs` are fully commented out — remediation is a no-op pragma addition (zero live diagnostics possible).

**Not COM-bound (host-neutral, pure logic):**
- `CidImageResolver.cs` — confirmed no `Microsoft.Office.Interop.Outlook` reference; operates purely on `string html`, `IReadOnlyCollection<IAttachment>`, `string virtualHost`. This file is a genuine unit-test target (has a dedicated `CidImageResolverTests.cs`) and should be held to the same >=90%-new/no-regression bar as any non-exempt file, not treated as COM-exempt.

**Injectable seams already present:**
- `EmailDetailsWrapper.cs` / `IEmailDetailsWrapper` is the existing seam over the static `EmailDetails` extension methods — this pattern (thin injectable wrapper delegating to static COM-touching extensions) is the repo's established seam style and should be preserved as-is (no new seam needed, no seam removed).
- `OutlookItemTry` / `OutlookItemTryGet` / `OutlookItemFlaggableTry` are themselves try/catch-swallowing decorators over `IOutlookItem`/`IOutlookItemFlaggable` — these interfaces (in `UtilitiesCS/Interfaces/IOutlookObjects/`) are out of scope and oblivious, mirroring the HelperClasses precedent where FileSystem wrapper implementations are in-scope but the interfaces they implement are not.

## 4. Cross-module public-contract surface

Grepped for `MailItemHelper|OutlookItemFlaggable|ConvHelper|OlTableExtensions|AttachmentHelper|OutlookItemTry|ItemInfo` across the full repo. Confirmed external (non-`UtilitiesCS`) consumers:

- **QuickFiler** (`QuickFiler/Controllers/QfcItemController*.cs`, `QfcCollectionController.cs`, `QfcHighConfidencePreFilter.cs`, `FilerQueue.cs`, `EfcItemController.cs`, `EfcHomeController.*.cs`, `EfcDataModel.cs`, `QuickFiler/Helper Classes/ConversationResolver*.cs`, `IConversationResolver.cs`, `IQfcItemController.cs`) — the heaviest external consumer of `MailItemHelper`/`ConvHelper`.
- **TaskVisualization** (`TaskController.cs`, `FlagChangeGroup.cs`, `AutoCreateProject.cs`, `AutoAssignContext.cs`, `AutoAssignPeople.cs`).
- **TaskMaster** (`Ribbon/TryFunctionalityInConstruction.cs`, `AppGlobals/EngineInitTimingProbe.cs`, `AppGlobals/AppItemEngines.cs`, `AppGlobals/AppEvents.cs`).
- **Tags** (`TagLauncher.cs`).
- **ToDoModel** (`Data Model/ToDo/ToDoItem.cs`).

This confirms AC5/AC6's premise: annotations on `MailItemHelper`, `OutlookItemFlaggable*`, `OlTableExtensions`, `ConvHelper`, and `AttachmentHelper` public members are genuine cross-project contracts, not internal-only decisions. Nullable choices on these types' public surface (e.g., `MailItemHelper.Sender`, `.FolderInfo`, `.AttachmentsInfo`, `.Globals`) will be visible to QuickFiler/TaskVisualization/TaskMaster/Tags/ToDoModel compilation even though those consuming files remain nullable-oblivious themselves (an oblivious caller silently accepts either `T` or `T?`; no cross-blocking occurs per the epic's per-file pragma architecture, but the annotation must still describe true behavior since a future nullable-enabled caller would rely on it).

## 5. Upstream #363/#364 contract consumption

**Confirmed direct consumption of #364 (HelperClasses) contracts:**
- `MailItemHelper.Loading.cs` calls `Initializer.GetOrLoad(ref _item, () => (MailItem)olNs.GetItemFromID(EntryId, StoreId), strict, _entryId, _storeId)` in `ResolveMail`. The #364 spec explicitly flags `Initializer.GetOrLoad`'s `ref T`/`default(T)` contract as a "deliberate contract choice" batch-8 (last, highest-risk) decision. This cluster's `ResolveMail`/`FromDfAsync`/`FromMailItemAsync` call chain is a direct downstream consumer of that decision: if `GetOrLoad` returns `T?` unconstrained, `ResolveMail`'s return type (`MailItem`) and its callers' null-checks must be reconciled.
- `AttachmentHelper.cs` constructs and holds `FilePathHelper` instances (`_filePathHelperSave`, `_filePathHelperSaveAlt`) and reads/writes their `FilePath`/`FolderPath` properties directly. The #364 spec's "crux of the file" split (`FilePath`/`FolderPath`/`FileName` default to `""`; `FileStemSeed`/`FileStemSuffix`/`FileStem`/`FileExtension` nullable) is consumed as-is here — `AttachmentHelper.FilePathSave`/`FolderPathSave` forward directly to `FilePathHelperSave.FilePath`/`.FolderPath`, so once #364 lands, these forwarding properties inherit the non-nullable `""`-default contract with no further change needed, but they must not add their own conflicting nullable annotation.
- `ConversationHelper.cs`/`OlTableExtensions.*` call `df.PrettyText()` (Debug.WriteLine(df.PrettyText())) from `PrettyPrint` — the #364 spec's explicitly-named batch-8 (last, highest-contract-sensitivity) file. This is confirmed consumption of the **final** #364 batch, meaning this Wave-1 child cannot be fully verified as CS86xx-clean until all eight #364 batches (not just the early ones) are merged upstream, if `PrettyText()`'s return type or nullability changes as part of that remediation.

**Confirmed direct consumption of #363 (Extensions) contracts:**
- Lazy-field infrastructure: `MailItemHelper.cs`/`.Loading.cs`/`.Properties.cs`/`.Serialization.cs` use `.ToLazy()`, `.ToLazyValue()`, `.ToLazyTry()` extensively (`UtilitiesCS.Extensions.Lazy` namespace) — these correspond to `LazyExtension.cs`, named in the #363 spec's Batch B (string/serialization/image-stream utilities).
- `ConversationColumnSchemas.ForEach(schema => table.Columns.Add(schema))` in `ConversationHelper.Formatting.cs` consumes `IEnumerableExtensions.ForEach`, named in #363's Batch C (core generic collection contracts, "careful review; must precede Batch E").
- `array.ToStringArray()` and `.SliceRow(i)` in `OlTableExtensions.TableAccess.cs.EnumerateTable`, and `jagged.To2D()` in `OlTableExtensions.Etl.cs` (`EtlByRow`/`EtlByRowAsync`), consume `ArrayExtensions.ToStringArray`/`SliceRow`/`To2D` — the exact same Batch C members the #363 spec names as consumed by `DfMLNet`/`DfDeedle` ("Batch C must precede Batch E because `DfMLNet`/`DfDeedle` consume `CastNullSafe`, `ToStringArray`, `SliceColumn`, and `To2D` from Batch C"). This is a previously-undocumented second consumer of Batch C beyond the Extensions feature's own Batch E: `OlTableExtensions` in this cluster has the identical ordering dependency on #363 Batch C landing first.

**Null-flow correction pressure this creates:** none of the above requires new runtime guards in this cluster's files — the upstream contracts (once landed) are read-only consumption points (extension-method call sites), so the correction is limited to accepting whatever `?`/`T?` the upstream signature declares and propagating it through this cluster's own signatures where the value flows to a public member (e.g., if `ToStringArray()` becomes `string?[,]`, `EnumerateTable`'s local `stringArray` variable and `SliceRow` call need a compatible nullable local type, not a new guard).

## 6. Proposed batch grouping (leaf-first, scope not fine-grained sequencing)

- **Batch A — trivial / dead-code confirm-clean:** `CaptureEmailAddressesModule2.cs`, `ItemComparer.cs`. Zero live diagnostics possible; pragma-only, no annotation decisions.
- **Batch B — pure/host-neutral leaf:** `CidImageResolver.cs`. Not COM-bound; no upstream dependency; independently verifiable and already has dedicated test coverage (`CidImageResolverTests.cs`).
- **Batch C — small COM-bound leaves (no partial-class entanglement, no upstream #363/#364 dependency identified):** `MailResolution.cs`, `MailItemExtensions.cs`, `OlItemPseudoInterface.cs`, `OlItemSummary.cs`, `OlToDoTable.cs`.
- **Batch D — OutlookItem reflection-wrapper family (must be reviewed together for consistent `TryGet<T>`/`default(T)`/`out T` unconstrained-generic annotation choices; internal call graph is tightly coupled):** `OutlookItem.cs`, `OutlookItemExtensions.cs`, `OutlookItemFlaggable.cs`, `OutlookItemTry.cs`, `OutlookItemTryGet.cs`, `OutlookItemFlaggableTry.cs`. Ordering constraint: `OutlookItem.cs` and `OutlookItemExtensions.cs` (the base wrapper and its reflection helpers) should be annotated before the two `*Try`/`*FlaggableTry` decorator classes that wrap them, since the decorators' `TryGet<T>() => default(T)` contract must match the base class's already-decided nullable contract for the same members.
- **Batch E — Attachment cluster:** `AttachmentSerializable.cs` before `AttachmentHelper.cs` (the former is the plain data/IAttachment implementation `AttachmentHelper` wraps and constructs; annotating the leaf first prevents re-touching `AttachmentHelper` when `AttachmentSerializable`'s nullable byte/string properties are decided). Depends on #364 (`FilePathHelper` contract) being landed first (see Section 5).
- **Batch F — ItemInfo / EmailDetails (MailItem's non-partial-group files, moderate cross-references to `MailItemHelper`):** `ItemInfo.cs`, `EmailDetails.cs`, `EmailDetailsWrapper.cs`. `EmailDetailsWrapper.cs` should follow `EmailDetails.cs` (thin delegator; its own signatures mirror whatever `EmailDetails` decides).
- **Batch G — `MailItemHelper` partial-class group (highest-contract-sensitivity in this cluster; must stay intact as one batch per Section 2):** `MailItemHelper.cs`, `MailItemHelper.Properties.cs`, `MailItemHelper.Html.cs`, `MailItemHelper.Loading.cs`, `MailItemHelper.Serialization.cs`. Depends on: Batch F (`ItemInfo.cs`, `EmailDetails.cs` — `MailItemHelper.Serialization.cs` constructs `ItemInfo` and calls `EmailDetails`-adjacent members); #364 `Initializer.GetOrLoad` contract (Section 5); #363 `LazyExtension` contract (Section 5). Ordering constraint: land after Batches D/E/F so `MailItemHelper`'s dependencies on `OutlookItem`-family patterns and `AttachmentHelper`/`ItemInfo` are already-decided rather than re-touched.
- **Batch H — `ConvHelper` partial-class group:** `ConversationHelper.cs`, `ConversationHelper.Formatting.cs`. Depends on #363 Batch C (`IEnumerableExtensions.ForEach`) and #364 Batch 8 (`PrettyPrint.PrettyText`) per Section 5; also calls `MailItem`/`Conversation` members shared with `MailItemHelper`'s domain, so scheduling after Batch G avoids re-deciding overlapping Outlook-type null contracts twice.
- **Batch I — `OlTableExtensions` partial-class group (largest, most cross-file-coupled group in this cluster):** `OlTableExtensions.cs`, `OlTableExtensions.RowTransforms.cs`, `OlTableExtensions.Etl.cs`, `OlTableExtensions.TableAccess.cs`. Depends on #363 Batch C (`ToStringArray`/`To2D`, Section 5) and on `ConvHelper` (Batch H) because `OlTableExtensions.cs`/`.TableAccess.cs` both have `using static UtilitiesCS.ConvHelper;` and call `ConvHelper`'s `Justify`/formatting members. Land last among the four partial groups.

Overall ordering constraint summary: A/B/C (trivial+leaf) → D (OutlookItem family) → E (Attachment, needs #364) → F (ItemInfo/EmailDetails) → G (MailItemHelper, needs D/E/F + #363/#364) → H (ConvHelper, needs #363/#364 + overlaps G) → I (OlTableExtensions, needs #363 + H via `using static ConvHelper`).

## Rejected alternatives

- **Remediate by directory instead of dependency graph** (MailItem batch, then Item batch, then Conversation, Attachment, Table in listed order) was considered but rejected: it would force `OlTableExtensions` (which has a `using static UtilitiesCS.ConvHelper` compile-time dependency) to be annotated before `ConvHelper`, and would split `MailItemHelper`'s dependents (`ItemInfo`/`EmailDetails` in the same directory) from `AttachmentHelper` (different directory) despite `MailItemHelper.Serialization.cs` referencing both. The dependency-graph-ordered batching above (Section 6) avoids re-touching already-annotated files.
- **Single all-30-files batch** was considered (matches the epic's per-file-pragma independence claim that each file is separately mergeable) but rejected for planning purposes: while technically each file could be opted in independently, the partial-class groups (Section 2) and the confirmed private cross-file call graphs within `OlTableExtensions`/`ConvHelper` make single-file review impractical; batching by the dependency order above keeps each reviewable unit small while respecting the required joint-opt-in groups.

## 7. Pre-existing conditions flagged (not fixed)

- **`OutlookItem.cs` is 503 lines — exceeds the repo 500-line file-size limit.** Verified by direct line count (`cat -n` offset read confirms line 503 is the file's final `}`). This is a pre-existing condition; annotation-only work adds a pragma line and `?`/`!` annotations, which will push the file further over 500, not under it. Per the epic's constraints, do not split the file (that would be a refactor); flag for a future issue.
- **`dynamic item = itemObj;` in `OlToDoTable.EnsureItemValues`** — `dynamic` member access (`item.PropertyAccessor`, `item.EntryID`, `item.Save()`) is invisible to nullable-flow analysis; the compiler cannot verify null-safety through a `dynamic` call site. This is a net481/reflection-era hazard the annotation-only work cannot resolve (converting `dynamic` to a typed access pattern would be a behavior-risk refactor, out of scope). Flag only.
- **Unconstrained-generic `default(T)` returns are pervasive in the `OutlookItem`/`OutlookItemTry`/`OutlookItemTryGet`/`OutlookItemExtensions` family** (`internal static T TryGet<T>(Func<T> getter)` etc., in at least 4 files) — mirrors the exact pattern the #363 spec calls out for `IEnumerableExtensions`/`ArrayExtensions` ("deliberate contract choices," not mechanical). Each `TryGet<T>`/`TryCall<T>` site needs an explicit `T?` (unconstrained) return-type decision; this is annotation work, not a defect, but is flagged here as a concentration of "deliberate contract choice" risk analogous to the upstream Initializer decision.
- **Sibling-property inconsistency in `MailItemHelper.Properties.cs`:** most lazy-backed properties guard their getter with a `??` fallback (`_body?.Value ?? string.Empty`, `_olRecipients?.Value ?? Array.Empty<Recipient>()`), but four properties do not — `Globals` (`get => _globals?.Value;`), `FolderInfo` (`get => _folderInfo?.Value;`), `Sender` (`get => _sender?.Value;`), and `AttachmentsInfo` (`get => _attachmentsInfo?.Value;`). These four can genuinely return null under an enabled nullable context and are the concrete candidates for `?` on the public contract (`IApplicationGlobals?`, `IFolderWrapper?`, `IRecipientInfo?`, `IAttachment[]?`) rather than a new `??` guard (adding a guard would be a behavior change per the "prefer annotation over new runtime guards" constraint carried from #363/#364). Flagged as the concrete decision point for Batch G, not a defect to fix differently.
- **`AttachmentHelper.CheckParameters` 3-arg overload appears unreferenced within this file** (only the 4-arg overload is called from `Init`). Not a nullable-remediation concern per se, but worth noting: if the 3-arg overload is genuinely dead, annotating it is still required (any in-scope file member must reach zero CS86xx), but no behavior verification via existing tests should be expected for that specific overload beyond what already exists.
- **`MailItemHelper.Html.cs`'s existing interior `#nullable enable`/`#nullable disable` region** (Section 1) is itself a pre-existing inconsistency with the epic's file-level-pragma convention; converting it to a whole-file pragma is in-scope remediation work (not a flag-only item), but is called out here because it is the one file in this cluster that is not starting from a fully-oblivious baseline.

## 8. Existing test surface

Verified via `Glob` against `UtilitiesCS.Test/OutlookObjects/{MailItem,Item,Conversation,Attachment,Table}/*.cs` — every production file in scope has at least one dedicated test file:

- **MailItem:** `CaptureEmailAddressesModule2Tests.cs`, `CidImageResolverTests.cs`, `EmailDetailsTests.cs`, `EmailDetailsWrapperTests.cs`, `ItemInfoTests.cs`, `MailItemExtensionsTests.cs`, `MailItemHelperCoreTests.cs`, `MailItemHelperProjectionTests.cs`, `MailItemHelper_ExtendedTests.cs`, `MailResolutionTests.cs`, plus `UtilitiesCS.Test/EmailIntelligence/EmailParsingSorting/MailItemHelperTests.cs` and `UtilitiesCS.Test/OutlookObjects/ItemInfo_Tests.cs`/`AttachmentSerializable_Tests.cs` (legacy-named duplicates alongside the newer `*/OutlookObjects/{Dir}/*Tests.cs` layout — both sets exist and both must stay green).
- **Item:** `OlItemPseudoInterfaceTests.cs`/`OlItemPseudoInterface_Tests.cs`, `OlItemSummaryTests.cs`, `OutlookItemExtensionsTests.cs`/`OutlookItemExtensions_Tests.cs`, `OutlookItemFlaggableTests.cs`/`OutlookItemFlaggable_Tests.cs`, `OutlookItemFlaggableTryTests.cs`/`OutlookItemFlaggableTry_Tests.cs`, `OutlookItemTests.cs`/`OutlookItem_Tests.cs`, `OutlookItemTryGetTests.cs`/`OutlookItemTryGet_Tests.cs`, `OutlookItemTryTests.cs`/`OutlookItemTry_Tests.cs`. (No test file for `ItemComparer.cs`, consistent with it being fully commented-out dead code — no test gap.)
- **Conversation:** `ConversationHelperAsyncTests.cs`, `ConversationHelperTests.cs`, `ConversationHelper_ExtendedTests.cs`.
- **Attachment:** `AttachmentHelperTests.cs`, `AttachmentSerializableTests.cs` (plus legacy `AttachmentSerializable_Tests.cs` at the `OutlookObjects/` root).
- **Table:** `OlTableExtensionsConversionTests.cs`, `OlTableExtensionsRetryTests.cs`, `OlTableExtensionsTransformTests.cs`, `OlTableExtensions_Tests.cs`, `OlToDoTableTests.cs`/`OlToDoTable_Tests.cs`.

**Coverage-exemption guidance for this test surface:** per repo policy and this feature's AC6, the COM-bound files identified in Section 3 are coverage-exempt for *new* test-writing pressure — annotation-only edits (`?`, `!`, generic constraints) must not trigger new test obligations on COM-bound members that lack an injectable seam (e.g., `OutlookItem.GetPropertyValue<T>`'s late-bound `InvokeMember` path, `AttachmentSerializable.GetBytes`'s `File.ReadAllBytes`/`SaveAsFile` COM calls). The existing test files above already exercise these classes through the established seams (`EmailDetailsWrapper`/`IEmailDetailsWrapper`, `OutlookItemTry`/`OutlookItemTryGet` as testable decorators, mocked `Attachment`/`MailItem` COM interfaces) — remediation must keep all of them green with no new behavior, and must not force new tests around the non-seamed COM call sites themselves. `CidImageResolver.cs` (Section 3, not COM-bound) is the one file in this cluster where normal (non-exempt) coverage expectations apply.
