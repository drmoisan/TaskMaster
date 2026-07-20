# utilitiescs-nullable-residuals (#375) — Annotation-Only Nullable Research

- **Timestamp:** 2026-07-18T21-30
- **Issue:** #375 (epic child `utilitiescs-nullable-residuals`, Wave 1)
- **Epic:** `utilitiescs-nullable-remediation`
- **Scope:** EXACTLY the 44 residual files enumerated in the delegation prompt. Annotation and
  null-safety only; per-file `#nullable enable`; no project-level `<Nullable>`; net481 target;
  no post-condition attributes; no `record`/`init`; no file over 500 lines as a result of edits.
- **Verification command (pragma-only):**
  `msbuild TaskMaster.sln /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU" /p:TreatWarningsAsErrors=true`
  (NO `/p:Nullable=enable`).

This document records only findings verified by reading the actual source of all 44 files, the
epic manifest, the `dialogs-misc` (#374) spec, and the Wave-0 threading (#369) spec + plan.

---

## Executive summary (most load-bearing findings first)

1. **Three pre-existing 500-line breaches inside the 44** — `MeetingItemHelper.cs` (847),
   `RecipientStatic.cs` (773), `UserDefinedFields.cs` (722). All three already exceed 500 lines
   *before* any edit. Adding a pragma keeps them over (848/774/723). Splitting is a refactor and
   out of annotation-only scope. **Recommendation: FLAG as pre-existing, do NOT fix**, exactly as
   Wave-0 threading did for `TimeOutTask.cs` (975 lines). No other in-scope file exceeds 500 lines
   before or after annotation (next largest hand-written: `SmithWaterman.cs` 376, then
   `FilterOlFoldersController.cs` 343, then `IntelligenceConfig.cs` 311).

2. **`PeopleScoDictionaryNewBackup.cs` is a DEAD, UNCOMPILED duplicate — not a partial.** Both it
   and `PeopleScoDictionaryNew.cs` declare a **non-partial** `class PeopleScoDictionaryNew` in
   namespace `ToDoModel.Data_Model.People`. Two non-partial classes of one fully-qualified name
   cannot co-compile (CS0101). The csproj (old-style, explicit `<Compile Include>`) lists only
   `EmailIntelligence\People\PeopleScoDictionaryNew.cs` (line 689); `PeopleScoDictionaryNewBackup.cs`
   is NOT in the compile set. Adding a pragma to it has zero effect on the gate (it is not compiled
   and cannot emit CS86xx). **MAINTAINER DECISION required: exclude from opt-in or delete.** Do not
   spend annotation effort on it.

3. **No two-hand-written partial-class pairs exist among the 44** — so no combined-batch
   requirement applies. Every `partial class` in the set (the six OlFolderTools viewers/forms) is a
   Designer/hand-written pair: only the hand-written half is opted in; the six `*.Designer.cs`
   files stay oblivious (never a pragma), which does not cross-block the hand-written half because
   `#nullable enable` is lexical/per-file (same rule threading applied to its form partials).

4. **Sibling/undeclared clusters stay OBLIVIOUS at execution time, which sharply bounds the work.**
   The only annotated contracts the residuals consume are the three declared Wave-0 upstreams
   (Extensions #363, HelperClasses #364, Threading #369). All *other* cross-cluster types the 44
   files touch — `ReusableTypeClasses` (#366), `OutlookObjects/Folder`+`Store` (#365),
   `OutlookObjects/MailItem`+`Item` (#371), `EmailIntelligence/Bayesian`+`ClassifierGroups`+`Flags`
   (#372), `Dialogs` (#374), and external `ToDoModel`/`Tags`/`BrightIdeasSoftware`/`AngleSharp` — are
   nullable-oblivious when residuals executes (either not opted-in, or a different assembly). Under
   the pragma, dereferencing an oblivious-typed member produces **no** CS86xx. Consequently the real
   debt is dominated by each file's OWN members: **CS8618** (uninitialized non-null fields/
   auto-props/events), **CS8603/CS8625** (own `return null` / `= null` on unconstrained-`T` or
   reference returns), and **self-induced CS8602** (only after a field is annotated `T?`).

5. **Dependency counts confirmed against source: 7 Extensions / 4 HelperClasses / 1 Threading.**
   Details in §2, including one refinement to the manifest: `OneDriveDownloader.cs`
   `TryCopyToAsyncWithTimeout` resolves to **Extensions** (`StreamExtensions`), while
   `RunWithTimeout` resolves to **Threading** (`TimeOutTask`, namespace `UtilitiesCS`) — so
   OneDriveDownloader carries BOTH the Extensions and Threading edges.

6. **Undeclared dependency edge on ReusableTypeClasses (#366)** — five in-scope files consume
   `ReusableTypeClasses` types (`TreeNode<T>`, `SmartSerializableLoader`, `ScoDictionaryNew<,>`).
   #366 is Wave-0 (prepared) but is NOT in this child's `depends_on`. Harmless in ordering (Wave-0
   precedes Wave-1) but flagged for the epic-planner, mirroring the `dialogs-misc` threading-edge
   precedent (§6).

---

## 1. CS86xx pattern inventory per file (Q1)

Legend: `8618`=uninitialized non-null field/prop/event/auto-prop; `8603`=possible-null return;
`8625`=null literal to non-null; `8604`=possible-null argument; `8602`=deref of maybe-null (only
self-induced here). "clean" = expected zero CS86xx under the pragma (verify-only pragma).

### Loose trees
| File | Lines | Notable CS86xx | Fix shape |
|---|---|---|---|
| `Examples/MSDemoConv.cs` | 96 | 8600/8602: `selectedItem as Outlook.MailItem` -> `mailItem`; `mailItem.Parent as Outlook.Folder` -> unguarded `folder.Store` (l.26-27); repeated `... as Outlook.Folder` then `.Name` (l.60-61, 86-87). COM types oblivious, but the `as`-to-non-null local assignments still emit 8600, and the follow-on derefs 8602. | See MAINTAINER DECISION (§6). If annotated: `Outlook.Folder?` locals + justified `!` at the demo's own derefs. |
| `To Depricate/FileIO2.cs` | 227 | 8603: `CSV_ReadTxtF` (l.130) and `CsvRead` (l.152) `return null;` (`string[]`); cascading 8602/8604 at `CsvReadTo2D`/`CsvReadToJagged` which pass the now-`string[]?` to `SplitArrayTo2D`/`.Select`. | `string[]?` returns; preserve current NRE with `array1D!` at the two call sites (annotation-only, behavior-preserving). Deprecation-marked (§6). |
| `To Depricate/StringManipulation.cs` | 22 | none (single `Regex.Replace` on non-null param). | clean (verify-only pragma). Deprecation-marked (§6). |
| `OneDriveHelpers/OneDriveDownloader.cs` | 139 | 8603: `TryGetUrlStreamAsync` (l.56) and `TryGetFileStreamWriter` (l.101) `return null;` (`Task<Stream>`). 8618 candidate: `_client`/`_clientGetAsync` set through property setters in ctor (setter body not traced by the ctor null-analysis). | Return `Task<Stream?>` (callers already null-check l.67, 72). `_client = null!;`/`_clientGetAsync = null!;` (behavior-preserving) OR annotate the fields. `RunWithTimeout`/`TryCopyToAsyncWithTimeout` returns are non-null (see §3) so `response`/`stream`/`contentStream` need no null handling beyond existing guards. |
| `OneDriveHelpers/AngleSharpParsedEmailBody.cs` | 88 | 8618: `_html`/`_links`/`_filteredLinks` never set in ctor; `_parser` set via setter. 8603: `FilterLinksByDomain` (l.80) `return null;`. `Links ??=` (l.77) shows `_links` is genuinely nullable. | `Html`->`string?`, `Links`/`FilteredLinks`->`IEnumerable<(string,string)>?`, `FilterLinksByDomain`->`...?` return. `_parser = null!` or annotate. |

### OutlookObjects residual (root 3 + 10 leaf)
| File | Lines | Notable CS86xx | Fix shape |
|---|---|---|---|
| `IOutlookReadinessGate.cs` | 50 | interface only. Contract: `IsReady(Outlook.Store store)` doc'd "null store returns false". | Annotate param `Store? store` to match impl; else clean. Co-annotate with the impl. |
| `OutlookReadinessGate.cs` | 113 | near-clean. `_app` non-null (`?? throw`). `IsReady(Store store)` uses `store?.` -> annotate `Store? store` for contract consistency (no 8602 fires without it). `IsTransientError` has `if (e is null)` on non-null param (allowed; not CS86xx). | `Store? store` param; else clean. |
| `MailResolution.cs` (root; class `MailResolution_ToRemove`) | 29 | 8625/8600: `MailItem OlMail = null;` (l.17). 8603: `return OlMail;` (l.26). Class name suffix `_ToRemove` signals dead code. | `MailItem?` return + `MailItem? OlMail = null`. Flag `_ToRemove` as deletion candidate (§6). |
| `AppointmentItem/MeetingItemHelper.cs` | **847** | HIGH volume. 8618: `_item`, `Sw` auto-prop, `PropertyChanged` event, many `Lazy<...>` fields. 8603 pervasive: every `get => _x?.Value;` returning non-null `string`/`T` (l.281-506 style). Already contains an inline `#nullable enable/disable` island (l.699-736) around `_emailHeader`. | Largest single-file effort. `Lazy<...>?` fields already `?`-guarded getters -> return-type `string?`/`T?` OR justified `!`. `PropertyChanged` -> `?`. **500-line breach FLAG (§7).** |
| `Calendar/Calendar.cs` | 24 | 8625/8603: `Folder foundCalendar = null;` (l.12) -> `return foundCalendar;`. | `Folder?` return + `Folder? foundCalendar = null`. |
| `Category/CreateCategory.cs` | 90 | 8625/8603: `Category objCategory = null;` (l.15) -> `return objCategory;`. | `Category?` return + `Category? objCategory = null`. |
| `Com/ComType.cs` | 71 | 8603: `GetTypeName` `return null;` (l.21). | `string?` return. |
| `Explorer/ExplorerActions.cs` | 41 | 8603: `GetCurrentItem` `return null;` (l.28); `Readable` `return null;` (l.36). | `object?` returns. |
| `Fields/MAPIFields.cs` | 130 | clean — all static string/immutable-dict initializers assigned; `struct Schemas` has only static members (no instance fields -> no 8618). | verify-only pragma. |
| `Fields/UserDefinedFields.cs` | **722** | 8603 many: `SafeGetPropertyAccessorValue`(l.25), `TryGetProperty`(l.159), `GetUdfValue`(l.117), `GetUdfString` `value as string`(l.74), `GetUdfValue<T>` `default(T)`(l.81). 8625: `UserProperty objProperty = null`(l.217). | `object?`/`string?`/`T?` returns; `UserProperty? objProperty`. **500-line breach FLAG (§7).** |
| `Filter DASL/DASLFilterParser.cs` | 121 | Own code clean (`ParseExpression`/`CombineTree` always return a value). Consumes `ReusableTypeClasses.TreeNode<string>` (undeclared #366 edge, §6). `CombineTree` returns `node.Value` -> only fires 8603 if #366 annotates `TreeNode<T>.Value` as `T?` (likely non-null). | Likely clean under pragma if TreeNode.Value stays non-null; else `string?`. |
| `Recipient/RecipientInfo.cs` | 75 | 8618: `_name`/`_address`/`_html` unset in parameterless ctor `RecipientInfo(){}`. `Equals`/`GetHashCode` use `?? ""` -> fields genuinely nullable. | `string?` fields+props OR `= null!` (near-dup of #371's ItemInfo/EmailDetails pattern — keep consistent). Co-annotate with `RecipientStatic.cs`. |
| `Recipient/RecipientStatic.cs` | **773** | 8603: `GetGlobalAddressList` `return null;`(l.53). 8619/8625: `ExtractNameFromAddress` `return (null,null,null)`(l.512) -> `(string?,string?,string?)`. 8625: `string address = null`(l.129); `SegmentStopWatch sw = null` default(l.401). One `ToResolvedRecipient(AddressEntry)` returns `default`(l.378)->`Recipient?`; the `Recipient` overload returns non-null. `IsNullOrEmpty` is the Extensions #363 contract. | `AddressList?`/tuple-`?`/`Recipient?` returns; `SegmentStopWatch? sw = null`; `string? address = null`. **500-line breach FLAG (§7).** |

### EmailIntelligence residual (root 4 + Evaluation 2 + OlFolderTools 12 + People 2)
| File | Lines | Notable CS86xx | Fix shape |
|---|---|---|---|
| `FilterEntry.cs` | 83 | 8618: the 2-arg ctor (l.19-24) omits `_description` (exactly the `dialogs-misc`-sampled case). | `private string _description = null!;` (behavior-preserving: keeps the 2-arg ctor's current null) OR `string?`. Do NOT add `_description=""` (that changes runtime value = behavior change). `_flags` is `FlagClassNoItem` (#372 sibling, oblivious). |
| `FolderConverter.cs` (EmailIntel root) | 62 | clean — string helpers throw on null inputs; no `return null`. `relativePath[0].Equals(".")` odd but compiles. | verify-only pragma. Distinct from `OutlookObjects/Folder/FolderConverter.cs` (#365). |
| `IntelligenceConfig.cs` | 311 | 8618: `Config` auto-prop (set in `InitAsync`), `LastResourceTimingBreakdown` (doc'd null-until-run). 8625: `new KeyValuePair<...>(kvp.Key, null)`(l.121). Already uses a plain `readonly struct ResourceTimingRow` deliberately avoiding record-struct/CS0518 (net481 compliant). | `Config = null!` (or `?`), `string? LastResourceTimingBreakdown`, `null!` for the filtered KVP value. Consumes `ReusableTypeClasses` (#366) + `ToDoModel` (external) — oblivious. |
| `IntelligenceFilters.cs` | 11 | empty class body. | clean (verify-only pragma). |
| `Evaluation/EvaluationResult.cs` | 74 | clean — two immutable classes, all ctor-assigned reference params. | verify-only pragma. |
| `Evaluation/FolderPredictorEvaluator.cs` | 196 | 8603: `PredictTop` `return top.Length==0 ? null : ...`(l.181). net481 pattern: `string.IsNullOrEmpty(trueLeaf)` does NOT refine, so `leaves.Add(trueLeaf)`/`Increment(...,trueLeaf)` need `trueLeaf!`; `example.Tokens`(l.126) after the trueLeaf guard needs `example!`. NOTE: `MinedMailInfo`/`IFolderPredictor` are #372 (oblivious) so most of these only fire on the file's own `string?`/`!` decisions. | `string?` on `PredictTop`; justified `!` on `trueLeaf`/`example` (net481 non-refining BCL, §4). |
| `OlFolderTools/FilterOlFolders/FilterOlFoldersController.cs` | 343 | 8618: `_folderTreeView` (set in `SetFolderTreeView`), `PutCheckedState` (never assigned). `_folderTreeView` is genuinely null-checked (`if(_folderTreeView==null)`, `?.Dispose()`) -> annotate `FolderTreeCompatibilityView?`. Cross-cluster `TreeNode<FolderWrapper>`, `FolderTree*` (#365/#366) oblivious. | `_folderTreeView`->`?`; `PutCheckedState`->`?` or `= null!`. |
| `OlFolderTools/FilterOlFolders/FilterOlFoldersViewer.cs` | 127 | 8618: `_controller` (set in `SetController`). Designer-declared controls (`TlvNotFiltered`, etc.) live in the oblivious `.Designer.cs` -> no CS86xx. | `_controller`->`FilterOlFoldersController?` with `_controller!` in `SetupTree` (invariant: called only after `SetController`), OR `= null!`. |
| `OlFolderTools/FilterOlFolders/FolderInfoViewer.cs` | 66 | WinForms hand-partial; same shape (own field(s) 8618; Designer controls oblivious). | `?`/`= null!` on own fields. (Not exhaustively read; same pattern.) |
| `OlFolderTools/FilterOlFolders/IFilterOlFoldersViewer.cs` | 43 | interface (`TreeListView` external oblivious). | clean (verify-only pragma). |
| `OlFolderTools/FilterOlFolders/OSBrowser.cs` | 230 | WinForms hand-partial; consumes `HelperClasses.FileSystem` (#364 declared). Own-field 8618 + Designer-oblivious controls. | `?`/`= null!` on own fields; align to #364 FileSystem contracts. (Same pattern; read confirmed partial+FileSystem using.) |
| `OlFolderTools/FilterOlFolders/OSFolder.cs` | 20 | trivial hand-partial. | likely clean/verify-only. |
| `OlFolderTools/FolderRemap/FolderRemapController.cs` | 283 | 8618: `_mappings2` (set via `Mappings2` setter in ctor) -> `= null!`. `PropertyChanged` reachable via `FolderRemapTree`. Cross-cluster `TreeNode<OlFolderRemap>` (#366) + `BrightIdeasSoftware` oblivious. `MakeCheckedStatePutter` uses `FolderSelector.SelectFolder(...)` (in-scope) `is null` check. | `_mappings2 = null!`; align `SelectFolder` return with `OlFolderRemap?`. |
| `OlFolderTools/FolderRemap/FolderRemapTree.cs` | 264 | 8618: `_roots` (unset in `FolderRemapTree(){}`), `PropertyChanged` event; nested `OlFolderRemap`: `_olRoot/_olFolder/_name/_relativePath` unset in `OlFolderRemap(){}`, `_mappedTo` genuinely nullable, `PropertyChanged` event. `_batchNotifier` has initializer. | `_roots = null!`, events `?`, `_mappedTo`->`OlFolderRemap?`, others `= null!`. Consumes `TreeNode<T>`/`TimedBatchAction`/`ITimerWrapper` (#366) oblivious + `HelperClasses` (#364). |
| `OlFolderTools/FolderRemap/FolderRemapViewer.cs` | 95 | WinForms hand-partial; own-field 8618 + Designer-oblivious. | `?`/`= null!` on own fields. (Same pattern.) |
| `OlFolderTools/FolderRemap/FolderSelector.cs` | 56 | 8625: `OlFolderRemap _selection = null;`(l.54). 8603: `SelectFolder` returns `selector.Selection`(nullable)(l.30). | `OlFolderRemap? _selection = null`; `Selection`->`OlFolderRemap?`; `SelectFolder`->`OlFolderRemap?` (matches controller `is null` checks). |
| `OlFolderTools/FolderRemap/IFolderRemapViewer.cs` | 43 | interface. | clean (verify-only pragma). |
| `OlFolderTools/OlFolderHelper/SmithWaterman.cs` | 376 | near-clean. `object[,]`/`int[,]` element types non-null in nullable context (no 8602 on unboxing casts). `Matrix[x,y]?.ToString() ?? ""` already guarded. Reflection (`frame.GetMethod()`, `.Name`) is net481-oblivious. | likely clean or 0-2 minor; verify. |
| `People/PeopleScoDictionaryNew.cs` | 300 | 8618: `Globals` prop (set only in 1 ctor), `_prefix` (deref'd `_prefix.Value`). 8603: `AddMissingEntry` `return null;`(l.144), `RefineValidateCategory` (returns null on cancel). Consumes `ScoDictionaryNew` (#366), `MailItemHelper` (#371), `MyBox`/`InputBox` (#374) — all oblivious. | `Globals = null!`/`_prefix = null!` (behavior-preserving, preserves deref); `string?` returns. |
| `People/PeopleScoDictionaryNewBackup.cs` | 257 | **DEAD/uncompiled duplicate — not compiled, cannot emit CS86xx.** | MAINTAINER DECISION: exclude or delete (§2, §6). |

---

## 2. Dependency-using confirmation: 7 / 4 / 1 (Q2)

**`using UtilitiesCS.Extensions;` (or `.Extensions.Lazy`) — 7 files (manifest confirmed):**
1. `OneDriveHelpers/OneDriveDownloader.cs` (`.Extensions`)
2. `OneDriveHelpers/AngleSharpParsedEmailBody.cs` (`.Extensions.Lazy` — `.ToLazy()`)
3. `EmailIntelligence/IntelligenceConfig.cs`
4. `EmailIntelligence/People/PeopleScoDictionaryNew.cs`
5. `OutlookObjects/Recipient/RecipientStatic.cs`
6. `OutlookObjects/Fields/UserDefinedFields.cs`
7. `OutlookObjects/AppointmentItem/MeetingItemHelper.cs` (`.Extensions` + `.Extensions.Lazy`)

(6 use `using UtilitiesCS.Extensions;` exactly; the 7th, AngleSharp, uses the `Extensions.Lazy`
sub-namespace only. Both are the #363 cluster.) Extension methods actually called across these:
`IsNullOrEmpty` (the key `string?` contract), `ToLazy`/`ToLazyValue`/`ToLazyTry`,
`TryCopyToAsyncWithTimeout` (StreamExtensions), `GetRegexGroups`, `Tokenize`,
`ToConcurrentDictionaryAsync`/`ToAsyncEnumerable`, `FlattenArrayTree`/`IsArray`, `ToFormattedText`,
`Split(...,trim:true)`. None of these return a value whose null-state the residuals rely on beyond
`IsNullOrEmpty` (which, per §4, does NOT refine on net481).

**`using UtilitiesCS.HelperClasses;` (or `.HelperClasses.FileSystem`) — 4 files (confirmed):**
1. `EmailIntelligence/OlFolderTools/FolderRemap/FolderRemapTree.cs`
2. `EmailIntelligence/OlFolderTools/FilterOlFolders/OSBrowser.cs` (`.HelperClasses.FileSystem`)
3. `OutlookObjects/Recipient/RecipientStatic.cs`
4. `OutlookObjects/AppointmentItem/MeetingItemHelper.cs`

**Threading `TimeOutTask` API — 1 file (confirmed):** `OneDriveHelpers/OneDriveDownloader.cs`
(`ClientGetAsync.RunWithTimeout(...)`, `factory.RunWithTimeout(...)`). **Manifest refinement:**
`TryCopyToAsyncWithTimeout` does NOT resolve to Threading — it is `UtilitiesCS.Extensions.StreamExtensions`
(returns `Task<bool>`). `RunWithTimeout` is `TimeOutTask` (namespace `UtilitiesCS`, `Threading/TimeOutTask.cs`).
So OneDriveDownloader carries BOTH the Extensions and Threading edges; the Threading edge is via
`RunWithTimeout` only.

---

## 3. Upstream annotated contracts the annotations must match (Q2/Q4)

- **Threading `TimeOutTask.RunWithTimeout<...>` return type is NON-nullable `Task<TResult>`** and
  must stay so. The #369 plan (task P8-T2) pins: keep `Task<TResult>`, use `result = default!` /
  `return result!` internally; do NOT widen to `Task<TResult?>`. Consequence for
  `OneDriveDownloader`: `var response = await ClientGetAsync.RunWithTimeout(...)` and
  `await WriterTimeoutRunner(...) (= factory.RunWithTimeout(...))` are **non-null** — annotate no
  null handling around `response.IsSuccessStatusCode` or the returned stream beyond the file's own
  existing guards.
- **`StreamExtensions.TryCopyToAsyncWithTimeout` returns `Task<bool>`** (value type, non-null) — no
  nullability concern; existing `?.Dispose()` in `DownloadFileAsync` is unaffected.
- **Extensions `IsNullOrEmpty(this string?)`** — the annotated `string?` receiver contract; safe to
  call on nullable strings. It does NOT act as a `[NotNullWhen(false)]` refinement (net481, §4).
- **HelperClasses** members consumed (e.g. `SegmentStopWatch`, `TimedBatchAction`,
  `FileSystem` types) — treat their #364-annotated signatures as authoritative; the residual call
  sites here pass/store them without relying on a nullable-return refinement.

---

## 4. COM-interop deref & net481 constraints (Q4, Q8)

- **COM types (`Application`/`MailItem`/`Store`/`MAPIFolder`/`Folder`/`Recipient`/`AddressEntry`/
  `PropertyAccessor` etc.) are nullable-oblivious** (net481 reference assemblies carry no nullable
  metadata). Dereferencing them does NOT emit CS8602. Therefore the COM-heavy files
  (`OutlookReadinessGate`, `RecipientStatic`, `UserDefinedFields`, `MeetingItemHelper`, `Calendar`,
  `CreateCategory`, `MeetingItemHelper`, the OlFolderTools viewers) do NOT need `!` on COM member
  chains and do NOT need new runtime guards. Existing guards (e.g. `store?.GetDefaultFolder`,
  `_app.Session?.DefaultStore?...`) already handle the null paths and MUST be preserved as-is.
- **Where `!` is genuinely required it is the net481-oblivious-BCL pattern**, not COM: `string.IsNullOrEmpty`
  / `IsNullOrWhiteSpace` are NOT annotated `[NotNullWhen(false)]` on net481, so a value proven
  non-null by such a guard is still `maybe-null` to flow analysis. Resolve with a justified `!` at
  the guaranteed-non-null site (e.g. `FolderPredictorEvaluator` `trueLeaf!`), never a new guard.
  This mirrors the #369 `StoreLockupResponder`/`IsNullOrWhiteSpace` decision.
- **Post-condition attributes are forbidden and unnecessary here** — no `[NotNullWhen]`,
  `[MaybeNullWhen]`, `[NotNullIfNotNull]`, `[MaybeNull]`, `[AllowNull]`, `[DisallowNull]`,
  `[DoesNotReturn]`, `[MemberNotNull]`. Zero CS86xx is reachable with `?`, `= null!`, and justified
  `!` only. (`[ExcludeFromCodeCoverage]` is present in several files —
  `PeopleScoDictionaryNew`, `FolderSelector`, `SmithWaterman` uses none of the forbidden set — and
  is available on net481; it is not evidence the post-condition attributes are available.)
- **No `record`/`record struct`/`init` introduced.** `IntelligenceConfig.ResourceTimingRow` is
  already a plain `readonly struct` chosen specifically to avoid CS0518 (documented in-file). No
  new value types are added; no CS0518 risk.

---

## 5. Recommended batch grouping for the atomic plan (Q5)

Leaf-first, directory-cohesive; Designer files never opted in; no combined-batch requirement
(no two-hand-written partials). Cross-cluster consumers of #363/#364/#369 come after those upstreams
have merged (all Wave-0, guaranteed before Wave-1 execution). Suggested batches:

- **Batch 0 — verify-only / clean (pragma, expect zero CS86xx):** `IntelligenceFilters.cs`,
  `EvaluationResult.cs`, `MAPIFields.cs`, `FolderConverter.cs` (EmailIntel), `StringManipulation.cs`,
  `IFilterOlFoldersViewer.cs`, `IFolderRemapViewer.cs`, `SmithWaterman.cs` (verify), `OSFolder.cs`.
- **Batch 1 — small static COM helpers (8603/8625 return-nullability):** `Calendar.cs`,
  `CreateCategory.cs`, `ComType.cs`, `ExplorerActions.cs`, `MailResolution.cs` (root),
  `FolderConverter`(if not in B0).
- **Batch 2 — Outlook readiness pair (co-annotate interface+impl):** `IOutlookReadinessGate.cs`
  + `OutlookReadinessGate.cs` (`Store? store` on both).
- **Batch 3 — Recipient cluster (co-annotate):** `RecipientInfo.cs` + `RecipientStatic.cs`
  (773 lines — 500-line FLAG). Keep `RecipientInfo` field-nullability consistent with #371's
  ItemInfo/EmailDetails pattern.
- **Batch 4 — OneDrive (Extensions + Threading edges):** `AngleSharpParsedEmailBody.cs`,
  `OneDriveDownloader.cs` (consume #363 `TryCopyToAsyncWithTimeout`/`ToLazy` and #369
  `RunWithTimeout`; both merged first).
- **Batch 5 — EmailIntelligence data types:** `FilterEntry.cs`, `IntelligenceConfig.cs`,
  `FolderPredictorEvaluator.cs`, `PeopleScoDictionaryNew.cs`.
- **Batch 6 — OlFolderTools FilterOlFolders (hand-partials; Designer halves oblivious):**
  `FolderInfoViewer.cs`, `OSBrowser.cs`, `FilterOlFoldersViewer.cs`, `FilterOlFoldersController.cs`.
- **Batch 7 — OlFolderTools FolderRemap (hand-partials; Designer halves oblivious):**
  `FolderSelector.cs`, `FolderRemapViewer.cs`, `FolderRemapTree.cs`, `FolderRemapController.cs`.
- **Batch 8 — large COM helpers (500-line FLAG batch):** `UserDefinedFields.cs` (722),
  `MeetingItemHelper.cs` (847). Focused review; heavy `Lazy<...>?`/`8603` annotation on
  `MeetingItemHelper`.
- **To Depricate (own batch, pending §6 decision):** `FileIO2.cs`, `StringManipulation.cs`.
- **NOT in any batch (pending §6 decision):** `Examples/MSDemoConv.cs`,
  `People/PeopleScoDictionaryNewBackup.cs`.

Ordering note: place the three 500-line-breach files (`MeetingItemHelper`, `RecipientStatic`,
`UserDefinedFields`) last within their batches with the breach FLAG recorded, mirroring #369's
`TimeOutTask` handling.

---

## 6. Blocked / MAINTAINER-DECISION files (Q6)

1. **`Examples/MSDemoConv.cs`** — demo/sample code (namespace `UtilitiesCS.Examples`), compiled but
   not production surface. Genuine 8600/8602 (`as`-cast then unguarded COM deref). DECISION:
   remediate annotation-only, exclude via `[ExcludeFromCodeCoverage]`/pragma, or delete. Do not
   assume; surface for spec.md. Annotation-only remediation is feasible (`Outlook.Folder?` locals +
   justified `!`).
2. **`To Depricate/FileIO2.cs` and `To Depricate/StringManipulation.cs`** — real production helpers
   explicitly marked for future deprecation. Annotation-only is feasible (FileIO2 needs `string[]?`
   returns + `!`; StringManipulation is already clean). DECISION: remediate vs delete vs schedule
   deletion — remediating deprecation-marked code may be wasted effort.
3. **`People/PeopleScoDictionaryNewBackup.cs`** — DEAD, uncompiled duplicate (not in csproj Compile
   set; would be CS0101 if compiled alongside the live file). Cannot emit CS86xx; a pragma is a
   no-op. DECISION: exclude from opt-in or delete. Recommend excluding from the child's opt-in
   count (effective compiled hand-written opt-in set = 37, not 38).
4. **`OutlookObjects/MailResolution.cs` — class `MailResolution_ToRemove`** — the `_ToRemove` suffix
   signals a deletion candidate (like the `To Depricate` files). Annotation-only remediation is
   trivial (`MailItem?` return). Flag for maintainer alongside the deprecation set; default to
   remediate-in-place under this child unless told otherwise.
5. **Undeclared dependency edge on `ReusableTypeClasses` (#366)** — `DASLFilterParser.cs`,
   `IntelligenceConfig.cs`, `FolderRemapTree.cs`, `FilterOlFoldersController.cs`,
   `FolderRemapController.cs`, `PeopleScoDictionaryNew.cs` consume #366 types (`TreeNode<T>`,
   `SmartSerializableLoader`, `ScoDictionaryNew<,>`). #366 is Wave-0 (prepared) but is NOT in this
   child's `depends_on: [extensions, helperclasses, threading]`. Harmless in ordering, but flag for
   the epic-planner to add the edge (or confirm the consumed members are annotated null-neutral), in
   the same spirit as `dialogs-misc`'s flagged threading edge. Latent (harmless, sibling-oblivious)
   edges also exist on #365/#371/#372/#374 and external `ToDoModel`/`Tags`.

None of the 44 files is genuinely *blocked* from annotation-only remediation; the items above are
scope/ownership decisions, not technical blockers.

---

## 7. 500-line analysis (Q7)

Authoritative line counts (pre-edit). **Three files already exceed 500 lines** (pre-existing
breaches; a pragma adds one line):

| File | Pre-edit lines | With pragma | Status |
|---|---|---|---|
| `OutlookObjects/AppointmentItem/MeetingItemHelper.cs` | 847 | 848 | PRE-EXISTING breach — FLAG, do NOT split |
| `OutlookObjects/Recipient/RecipientStatic.cs` | 773 | 774 | PRE-EXISTING breach — FLAG, do NOT split |
| `OutlookObjects/Fields/UserDefinedFields.cs` | 722 | 723 | PRE-EXISTING breach — FLAG, do NOT split |

All other 41 in-scope files are comfortably under 500 both before and after annotation (largest
remaining: `SmithWaterman.cs` 376, `FilterOlFoldersController.cs` 343, `IntelligenceConfig.cs` 311,
`PeopleScoDictionaryNew.cs` 300, `FolderRemapController.cs` 283, `FolderRemapTree.cs` 264,
`AttachmentSerializable`-class files are out of scope). Annotation-only edits (`?`, `= null!`,
`!`) plus csharpier reflow will not push any of these 41 over 500. **Recommendation:** treat the
three breaches exactly as #369 treated `TimeOutTask.cs` — record the pre-existing violation as a
maintainer flag, defer any split to a separate refactor issue, and keep annotations IN-PLACE
(prefer `?`/`= null!`/`!` over new multi-line guard blocks) so no breach is *worsened* by this work.

---

## 8. net481 constraints hold (Q8)

- No post-condition attributes are proposed anywhere (§4). Zero CS86xx is reachable with `?`,
  `= null!`, justified `!`.
- No `record`/`record struct`/`init` introduced. The one existing struct in scope
  (`IntelligenceConfig.ResourceTimingRow`) is already a plain constructor-initialized
  `readonly struct` chosen to avoid CS0518; leave it as-is.
- Designer files (6) never receive a pragma; hand-written partial halves annotate only their own
  declared fields, never Designer-declared controls (which stay oblivious and do not cross-block).
- No project- or solution-level `<Nullable>` element; verify command stays pragma-only
  (`/t:Rebuild ... /p:TreatWarningsAsErrors=true`, no `/p:Nullable=enable`).

---

## Testing implications (per repo policy; no test code written)

- This is annotation-only with the "prefer `?`/`= null!`/justified `!` over new guards" rule, so
  no new executable lines should be introduced -> no changed-line coverage regression (AC pressure
  matches #369/#374). Where a `return null` type changes to `T?`, the executable line count is
  unchanged.
- Capture a clean baseline `vstest.console.exe` run (pass/fail + coverage) for `UtilitiesCS.Test`
  before edits, per the evidence-and-timestamp-conventions skill; diff after each batch. Existing
  MSTest/Moq/FluentAssertions suites for the touched areas (Recipient, ReadinessGate #207,
  IntelligenceConfig #207, FolderPredictorEvaluator, OlFolderTools controllers which already have
  injectable-viewer seams) must stay green and behavior-identical.
- The COM/VSTO-bound methods (`MeetingItemHelper`, `RecipientStatic`, `UserDefinedFields`,
  OlFolderTools viewers, `PeopleScoDictionaryNew` `[ExcludeFromCodeCoverage]` members) are covered
  by the repo's documented COM/VSTO exemption; annotation must not add executable guard lines that
  would create newly-uncovered branches.
- Verification is compile-time (`msbuild /t:Rebuild ... /p:TreatWarningsAsErrors=true`) per opted-in
  file reaching zero CS86xx; no live Outlook process is required for the nullable gate.
