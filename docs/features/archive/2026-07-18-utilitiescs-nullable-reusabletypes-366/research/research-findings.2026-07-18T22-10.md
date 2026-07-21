# Research: utilitiescs-nullable-reusabletypes (Issue #366) — Wave-0

- Date: 2026-07-18T22-10
- Feature: `docs/features/active/2026-07-18-utilitiescs-nullable-reusabletypes-366/`
- Epic: `utilitiescs-nullable-remediation` (Wave 0, complexity C3, cross-module contract change)
- Scope: per-file `#nullable enable` remediation of `UtilitiesCS/ReusableTypeClasses/` (recursive, including `TimedActions/` and `NewSmartSerializable/`).
- Method: static reading only (no build permitted in this environment). All null-risk assessments are static inference from source; the atomic executor confirms exact diagnostics under the pragma-driven gate.
- Sibling reference (format + shared language facts): `docs/features/active/2026-07-18-utilitiescs-nullable-extensions-363/research/research-findings.2026-07-18T21-45.md`.

---

## 1. Project / language / target-framework facts

Evidence: `UtilitiesCS/UtilitiesCS.csproj`, root `.editorconfig`.

- `<TargetFrameworkVersion>v4.8.1</TargetFrameworkVersion>` (net481). Non-SDK, `packages.config` legacy VSTO/.NET-Framework project (csproj line 16).
- `<LangVersion>12.0</LangVersion>` (csproj line 10) — C# 12. All nullable *syntax* is available: `?` annotations, null-forgiving `!`, `where T : notnull`, unconstrained `T?`, `is null`/`is not null` flow.
- No `<Nullable>` element anywhere in the csproj (grep for `Nullable` returns no matches). AC2 requires it stays absent; enforcement is per-file pragma only.
- No directory-scoped `.editorconfig` exists under `UtilitiesCS/` (glob `UtilitiesCS/**/.editorconfig` → none). The single repo-root `.editorconfig` has a `[*.cs]` section that sets analyzer/style severities but does NOT set any `nullable`/`Nullable`/`CS86xx` diagnostic level and does NOT enable a nullable context. Relevant: root `.editorconfig` line 27 `dotnet_analyzer_diagnostic.severity = suggestion` and the comment at lines 24-25 stating all new analyzer diagnostics default to `suggestion` so they cannot be promoted to errors under the nullable `TreatWarningsAsErrors` build. CS86xx compiler diagnostics are NOT analyzer diagnostics and are unaffected by that catch-all; they still become errors under `TreatWarningsAsErrors` inside a `#nullable enable` file.
- ZERO of the 54 `.cs` files under `ReusableTypeClasses/` currently carry `#nullable enable` (grep `#nullable` over the tree → no matches). This is a greenfield remediation for the whole cluster.
- The net481 nullable-attribute caveat established by the sibling child applies unchanged here: the `System.Diagnostics.CodeAnalysis` post-condition attributes (`[NotNullWhen]`, `[MaybeNullWhen]`, `[NotNullIfNotNull]`, `[MemberNotNull]`, etc.) are not polyfilled in-repo and MUST NOT be used. Reach zero CS86xx with plain `?`, `where T : notnull`, unconstrained `T?`, guard clauses, and justified `!`. The `[CallerMemberName]` attribute IS available and is already used across this cluster (e.g. `NewSmartSerializableConfig.Notify`, csproj line-referenced files).

---

## 2. File inventory (54 `.cs` files)

Line counts from ripgrep line-start count. Difficulty is a static estimate of null-state surface (trivial = few/no CS86xx, mostly interface/EventArgs/pure value; moderate = several fields/params/generics; complex = serialization round-trips, events, generic base constraints, nullable node graphs, file IO seams). `.resx` (ConfigViewer.resx, 192) is not a `.cs` and is out of scope.

### AsyncLazy/
| File | Lines | Difficulty |
|---|--:|---|
| AsyncLazy.cs | 159 | moderate |

### Concurrent/Observable/Bag/
| File | Lines | Difficulty |
|---|--:|---|
| BagChangedEventArgs.cs | 23 | trivial |
| ConcurrentObservableBag.cs | 252 | moderate |
| ISimpleActionBagObserver.cs | 7 | trivial (interface; likely zero CS86xx) |
| SimpleActionBagObserver.cs | 19 | trivial |

### Concurrent/Observable/Collection/
| File | Lines | Difficulty |
|---|--:|---|
| ConcurrentObservableCollection.cs | 169 | moderate |
| ConcurrentObservableCollection.Serialization.cs | 405 | complex (partial; file IO + seams) |
| IConcurrentObservableCollectionSeams.cs | 63 | trivial/moderate (interface + seam) |

### Concurrent/Observable/Dictionary/
| File | Lines | Difficulty |
|---|--:|---|
| ConcurrentObservableDictionary.cs | 375 | complex (CS8714 notnull; events; `default(TValue)`) |
| DictionaryChangedEventArgs.cs | 30 | trivial |
| SimpleActionDictionaryObserver.cs | 21 | trivial |

### LazyTry/
| File | Lines | Difficulty |
|---|--:|---|
| LazyTry.cs | 62 | moderate (Try/out generic) |

### Locking/
| File | Lines | Difficulty |
|---|--:|---|
| ILockingLinkedList.cs | 36 | trivial (interface) |
| LockingLinkedList.cs | 456 | complex (nullable node graph + locking) |
| LockingLinkedListNode.cs | 124 | moderate (Next/Prev nullable) |

### Locking/Observable/LinkedList/
| File | Lines | Difficulty |
|---|--:|---|
| ILockingLinkedListObserver.cs | 9 | trivial (interface) |
| LockingObservableLinkedList.cs | 522 | complex |
| LockingObservableLinkedListChangedEventArgs.cs | 26 | trivial |
| LockingObservableLinkedListNode.cs | 126 | moderate |
| SimpleActionLockingLinkedListObserver.cs | 27 | trivial |

### Matrices/
| File | Lines | Difficulty |
|---|--:|---|
| DataConverter2d.cs | 56 | moderate (`object[,]` casts) |
| DenMatrix.cs | 184 | moderate |
| JaggedMatrix.cs | 188 | moderate |
| Matrix.cs | 157 | moderate |

### NewSmartSerializable/
| File | Lines | Difficulty |
|---|--:|---|
| Config/ConfigController.cs | 154 | moderate (WinForms-coupled controller; testable seam — IN SCOPE) |
| Config/ConfigGroupBox.cs | 42 | EXEMPT — WinForms `GroupBox`-derived control |
| Config/ConfigViewer.cs | 147 | EXEMPT — `Form`-derived |
| Config/ConfigViewer.Designer.cs | 3734 | EXEMPT — Designer-generated |
| Config/NewSmartSerializableConfig.cs | 278 | moderate (events + Lazy fields; IN SCOPE) |
| SmartSerializable.cs | 596 | complex (>500; do not split) |
| SmartSerializableBase.cs | 534 | complex (>500; do not split) |
| SmartSerializableLoader.cs | 205 | complex |
| SmartSerializableNonTyped.cs | 104 | moderate |
| SmartSerializableStatic.cs | 107 | moderate |

### Observable/
| File | Lines | Difficulty |
|---|--:|---|
| ObservableCollectionBatchUpdate.cs | 31 | trivial |
| ObservableDictionary.cs | 834 | complex (largest file; >500; do not split) |
| ObserverHelper.cs | 43 | trivial/moderate |

### Other/
| File | Lines | Difficulty |
|---|--:|---|
| AbstractCloneable.cs | 43 | trivial/moderate (generic clone) |
| AsyncQueue.cs | 40 | trivial (BufferBlock<T>; likely near-zero CS86xx) |
| StackGeek.cs | 195 | moderate |
| StackObjectCS.cs | 196 | moderate |
| TreeNodeOfT.cs | 339 | moderate (`Parent` nullable root; `Value` T) |

### Serializable/
| File | Lines | Difficulty |
|---|--:|---|
| Concurrent/ScBag.cs | 325 | complex (serialization) |
| SerializableList.cs | 575 | complex (>500; do not split) |

### SerializableNew/
| File | Lines | Difficulty |
|---|--:|---|
| Concurrent/Observable/ScoDictionaryNew.cs | 281 | complex (CS8714; highest-contract; serialization) |
| Concurrent/Observable/ScoDictionaryStatic.cs | 49 | moderate |
| Concurrent/Observable/SloLinkedList.cs | 168 | moderate/complex (serialization; NotImplemented stubs) |
| Concurrent/Observable/SloStack.cs | 260 | complex |
| Concurrent/ScDictionary.cs | 449 | complex (CS8714) |

### TimedActions/
| File | Lines | Difficulty |
|---|--:|---|
| TimedAsyncTask.cs | 123 | moderate |
| TimedBatchAction.cs | 102 | moderate |
| TimedDiskWriter.cs | 363 | complex (timer + disk IO seams) |
| TimedQueueOfActions.cs | 369 | complex |
| TimerWrapper.cs | 185 | moderate |

Six files exceed the 500-line general limit (`ObservableDictionary` 834, `SmartSerializable` 596, `SerializableList` 575, `SmartSerializableBase` 534, `LockingObservableLinkedList` 522, plus the Designer file which is exempt). All are pre-existing. This epic is annotation-only and MUST NOT split any file (that would be a refactor, out of scope). Flag for a separate future issue; do not fix here.

---

## 3. Scope decision (opt-in vs. exempt)

Recommendation: opt in every production file under `ReusableTypeClasses/` that emits CS86xx **except** the WinForms-host-derived and Designer-generated files. Net: 51 files in scope, 3 exempt.

### Exempt (do NOT add `#nullable enable`)

Evidence: csproj lines 890-898 declare `ConfigGroupBox.cs` with `<SubType>Component</SubType>`, `ConfigViewer.cs` with `<SubType>Form</SubType>`, and `ConfigViewer.Designer.cs` with `<DependentUpon>ConfigViewer.cs</DependentUpon>`; a sibling `ConfigViewer.resx` (csproj line 1167) confirms designer/form provenance.

1. `Config/ConfigViewer.Designer.cs` (3734 lines) — Designer-generated. CLAUDE.md General Unit Test Policy exemption (b) covers "WinForms form-derived classes and Designer-generated code." Adding a pragma and remediating machine-generated code risks being overwritten on the next designer round-trip and delivers no downstream contract value. Do not opt in.
2. `Config/ConfigViewer.cs` (`public partial class ConfigViewer : Form`) — Form-derived. Its members are UI event handlers (`ButtonSave_Click`, `GroupBox_Enter`, etc.) that cannot be unit-tested without a live message pump and are not consumed as reusable cross-module contracts. Do not opt in.
3. `Config/ConfigGroupBox.cs` (`internal class ConfigGroupBox : GroupBox`) — WinForms control-derived. Same posture as (2).

Under AC6, leaving these three null-oblivious does not cross-block any opted-in file: null-oblivious types are treated as "unknown null-state" by consumers, never as errors.

### In scope (opt in)

- `Config/ConfigController.cs` — IN SCOPE. It is a plain controller class, NOT `Form`/control-derived, and carries an injectable test seam (`internal Action<ConfigViewer> ShowViewer = viewer => viewer.Show();`, lines 74) with dedicated tests (`ConfigController_Tests.cs`). This matches the policy carve-back: "Testable seams within otherwise-COM/WinForms-bound assemblies are explicitly NOT exempt." It does dereference `Viewer` and constructs a `ConfigViewer`, so it will emit CS86xx and needs the pragma.
- `Config/NewSmartSerializableConfig.cs` — IN SCOPE. Pure data/config type implementing `ISmartSerializableConfig` with `PropertyChanged` and Newtonsoft round-tripping; has `NewSmartSerializableConfig_Tests.cs`.
- All remaining 49 collection/serialization/matrix/timed/locking types.

Interface-only files (`ISimpleActionBagObserver`, `ILockingLinkedList`, `ILockingLinkedListObserver`, and likely `IConcurrentObservableCollectionSeams`) contain no method bodies and will emit no CS86xx; per AC1 the pragma is only required on files that emit CS86xx. Add the pragma to these for cluster consistency but expect zero remediation work; they are effectively verify-only.

---

## 4. Cross-module consumer mapping (contract-risk)

Evidence: repo-wide grep for the key type names (2038 hits across 210 files). Consumers OUTSIDE `UtilitiesCS.Test` and outside the defining files:

- **`ScoDictionaryNew<TKey,TValue>` and the `SmartSerializable<T>` family** — the highest-contract types. Production consumers: `UtilitiesCS/EmailIntelligence/People/PeopleScoDictionaryNewBackup.cs`, `UtilitiesCS/EmailIntelligence/SubjectMap/{SubjectMapSco,SubjectMapEncoder,CommonWords}.cs`, `UtilitiesCS/OutlookObjects/Store/StoresWrapper.cs`. Cross-project consumers: `QuickFiler/Controllers/{QfcDatamodel,QfcFormController,QfcCollectionController}.cs` and `QuickFiler/Interfaces/{IQfcDatamodel,IQfcCollectionController}.cs`; `TaskMaster.Test/AppGlobals/AppToDoObjects*`. Annotations on `ScoDictionaryNew` and `SmartSerializable` propagate to People/SubjectMap/QuickFiler/AppToDoObjects. **Flag as the top contract-risk cluster.**
- **`ConcurrentObservableDictionary<TKey,TValue>` / `ConcurrentObservableCollection<T>`** — base types for `ScoDictionaryNew` and other collections; broad transitive reach. The `where TKey : notnull` question (section 6) surfaces here and propagates to every derived type.
- **`SloLinkedList<T>` / `SloStack<T>` / `ScDictionary` / `ScBag<T>` / `SerializableList<T>`** — serializable wrappers consumed by TaskVisualization (`TaskVisualization.Test/{ManageFiltersController,AutoCreateProject,AutoAssignPeople,AutoAssignContext}Tests.cs`) and RibbonController (`TaskMaster.Test/Ribbon/RibbonControllerTests.cs`).
- **`TreeNode<T>`** — consumed by `UtilitiesCS/Extensions/*` (its `using UtilitiesCS.Extensions`) and folder-hierarchy code; `Parent` nullability is the key contract (nullable root).
- **`TimedDiskWriter`** — consumed via `UtilitiesCS.Test/HelperClasses/TimedDiskWriterTests.cs`; disk-writer seam type.
- **`AsyncQueue<T>`** — low reach; test-only external consumer located.

Highest-contract-risk ranking for careful, consistent review: (1) `SmartSerializable`/`SmartSerializableBase` + `NewSmartSerializableConfig`, (2) `ScoDictionaryNew` + `ConcurrentObservableDictionary`, (3) `SloLinkedList`/`SloStack`/`ScDictionary`/`ScBag`/`SerializableList`, (4) `TreeNode<T>`.

---

## 5. Null-state pattern catalog (expected CS86xx-class diagnostics)

Recurring patterns observed statically; each drives a specific annotation:

1. **Uninitialized non-nullable auto-property / field (CS8618).** `public string Name { get; set; }` in `ScoDictionaryNew.cs:174` and `SloLinkedList.cs:112`; `TreeNode<T>._parent` (`TreeNodeOfT.cs:22-27`, `Parent` is null at the root — `Depth` checks `Parent is null` at line 56). Fix: annotate to `string?` / `TreeNode<T>?` where the value is genuinely optional (accurate contract), or initialize in ctor where non-null is invariant.
2. **Uninitialized non-nullable event (CS8618).** `public event PropertyChangedEventHandler PropertyChanged;` recurs in `NewSmartSerializableConfig.cs:274`, `ScoDictionaryNew.cs:190`, `SloLinkedList.cs:130`; `public event EventHandler<DictionaryChangedEventArgs<TKey,TValue>> CollectionChanged;` in `ConcurrentObservableDictionary.cs:19`. Fix: `PropertyChangedEventHandler?` / `EventHandler<...>?`. Existing `?.Invoke(...)` call sites (e.g. `NewSmartSerializableConfig.cs:271`) already assume nullability, so this is annotation-only.
3. **Event-handler delegate parameter mismatch (CS8622).** `private void Config_PropertyChanged(object sender, PropertyChangedEventArgs e)` (`ScoDictionaryNew.cs:178`, `SloLinkedList.cs:118`) assigned to delegates whose sender is `object?`. WinForms handlers in `ConfigViewer.cs` show the same `(object sender, EventArgs e)` shape. Fix: `object? sender` (matches the framework delegate).
4. **`null` literal passed to non-nullable delegate/param (CS8625).** `CreateEmpty<T>(response, disk, settings, null)` in `SmartSerializableBase.cs:103` passes `null` for `Func<T> altLoader`; the callee null-checks it (`altLoader is null`, line 80). Fix: `Func<T>? altLoader`. Same `altLoader` pattern in `ScoDictionaryNew` / `SloLinkedList` interface-explicit deserialize overloads.
5. **`default(T)` / `default(TValue)` for unconstrained generic, then returned or stored (CS8603/CS8604).** `ConcurrentObservableDictionary.OnCollectionChanged` uses `var newValue = default(TValue); var oldValue = default(TValue);` (lines 59-60); `IListExtensions.Find`-backed `ConcurrentObservableCollection.Find` returns `default` (documented "or `default`", `ConcurrentObservableCollection.cs:48-49`). Fix: `TValue?` locals / `T?` return where the default can be null.
6. **Nullable-forgiving after `?.` in fire-and-forget async (CS8602).** `await Controller?.SaveAsync();` in `ConfigViewer.cs:62,67` (exempt file) is the pattern; the in-scope analogue is any `x?.Member` whose result is then dereferenced. Prefer guard/annotation over `!` unless the invariant is documented.
7. **`MethodBase.GetCurrentMethod().DeclaringType` (CS8602).** `GetCurrentMethod()` returns `MethodBase?` and `.DeclaringType` returns `Type?`; the logger initializer pattern `log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType)` recurs in `SmartSerializableBase.cs:20-22`, `ScoDictionaryNew.cs:25-27`, `SloLinkedList.cs:17-19`. Fix: `GetCurrentMethod()!.DeclaringType` with a justifying comment (the current method always exists here), matching a null-forgiving-only site.
8. **`as` cast producing nullable, then used (CS8600/CS8602).** `SpecialFolderComboBox.SelectedItem as string` in `ConfigGroupBox.cs:35` (exempt) is representative; in-scope analogues appear in matrix `object[,]` conversions (`DataConverter2d.cs`).
9. **Fields assigned in a called init method, not inline (CS8618).** `NewSmartSerializableConfig` ctor calls `ResetLazy()` (line 16) which assigns `_jsonSettings` / `_netJsonSettings` / `_localJsonSettings` (lines 66-68); the compiler does not follow definite assignment into the method, so all three protected `Lazy<...>` fields (lines 92, 104, 116) emit CS8618. Fix: `= null!` initializer with a comment, or annotate the fields, since they are non-null after construction. Same shape in `SloLinkedList.ism` (`protected SmartSerializable<...> ism;` at line 44, assigned in every ctor).

---

## 6. Serialization / generics gotchas

### 6.1 CS8714 `notnull`-constraint mismatch on generic dictionary bases (highest-risk decision)

Evidence: `ConcurrentObservableDictionary<TKey, TValue> : ConcurrentDictionary<TKey, TValue>` (`ConcurrentObservableDictionary.cs:15-16`) declares `TKey` unconstrained. Under `#nullable enable`, the annotated BCL `ConcurrentDictionary<TKey,TValue>` requires `where TKey : notnull`. An unconstrained `TKey` is "not known to be non-null," so the base-list type argument emits **CS8714** ("Nullability of type argument 'TKey' doesn't match 'notnull' constraint"). This propagates to `ScoDictionaryNew<TKey,TValue>` (derives from `ConcurrentObservableDictionary`, `ScoDictionaryNew.cs:22`), `ScoDictionaryStatic`, and `ScDictionary` (grep confirms `ScDictionary.cs` wraps the concurrent dictionary family).

Important nuance: **CS8714 is an 87xx diagnostic, not CS86xx.** AC1 targets CS86xx, but the verification gate is `/t:Rebuild /p:TreatWarningsAsErrors=true`, under which CS8714 in a nullable-enabled file becomes an error and blocks the build. The plan must therefore resolve it even though it is outside the literal CS86xx set.

Options, all annotation-only:
- **Recommended: add `where TKey : notnull`** to `ConcurrentObservableDictionary`, `ScoDictionaryNew`, `ScoDictionaryStatic`, and `ScDictionary`. This is the accurate contract (`ConcurrentDictionary` rejects null keys at runtime with `ArgumentNullException` regardless), is IL-metadata-only (no runtime behavior change, satisfying AC3), and gives downstream consumers the honest key contract (AC5). Existing callers instantiate with non-null reference or value keys and are unaffected; a caller would only warn if it explicitly used a nullable-reference key, of which there are none today.
- Rejected: `#pragma warning disable CS8714`. Suppresses rather than fixes; leaves an inaccurate contract; discouraged by policy.

Because adding a generic constraint touches the public generic-parameter list of shared reusable types, **flag this as the single highest-risk decision in the epic and require maintainer ratification before the planner commits it.** It is the one place where "accurate annotation" and "no API redesign" are in tension. (`ConcurrentBag<T>`-based types — `ConcurrentObservableBag`, `ScBag` — take `T` with no `notnull` requirement and are not affected.)

### 6.2 `NewtonsoftHelpers` interaction (do not remediate)

`ScoDictionaryNew.cs:15` `using UtilitiesCS.NewtonsoftHelpers.Sco;` and `GetSettingsJson` (lines 196-210) construct `AppGlobalsConverter`, `FilePathHelperConverter`, and `ScoDictionaryConverter<T,TKey,TValue>` from the `NewtonsoftHelpers` namespace. `NewtonsoftHelpers` is the SEPARATE sibling child (#9004) and is OUT OF SCOPE here. Interaction to note: while `NewtonsoftHelpers` remains null-oblivious, converter references from this cluster see them as unknown-null-state (no CS86xx forced across the boundary). Do not annotate or touch any `NewtonsoftHelpers` file; only annotate the local usage sites.

### 6.3 Serialization round-trip null fields

Types deserialized via Newtonsoft (`SmartSerializable`, `SmartSerializableBase`, `ScoDictionaryNew`, `SloLinkedList`, `SloStack`, `ScDictionary`, `ScBag`, `SerializableList`, `NewSmartSerializableConfig`) have fields that Newtonsoft populates by reflection after construction. The compiler cannot see that, so backing fields/props (`Config`, `ism`, `Name`, the `Lazy<JsonSerializerSettings>` trio) emit CS8618. Because the values are legitimately non-null after a successful round-trip, prefer `= null!` on the field (with a `// set by deserialization` comment) or a nullable annotation where the property is genuinely optional. Do NOT convert any of these to `record`/`init`/`record struct` — those fail CS0518 on net481 (no `IsExternalInit`), per the sibling child's do-not-touch trap.

### 6.4 `NotImplementedException` interface stubs

`SloLinkedList` implements several `ISmartSerializable<...>` members as `throw new NotImplementedException();` (lines 84, 93, 102, 107) with `Func<...> altLoader` parameters. Annotate the parameters to match the interface's nullable contract (`Func<SloLinkedList<T>>? altLoader` if the interface annotates it optional) but do not implement the bodies (out of scope).

---

## 7. Recommended remediation batching (feeds atomic-planner)

Ordered base/leaf-first so shared bases are annotated before dependents, minimizing re-touch. Intra-cluster dependency chain (from reads): `SmartSerializableBase` -> `SmartSerializable` -> {`SmartSerializableStatic`, `SmartSerializableNonTyped`, `SmartSerializableLoader`}; `SmartSerializable` is the `ism` used by all serializable wrappers; `ConcurrentObservableDictionary`/`ConcurrentObservableCollection` are bases of `ScoDictionaryNew`; `LockingLinkedList(Node)` -> `LockingObservableLinkedList(Node)` -> `SloLinkedList`; `NewSmartSerializableConfig` is the `Config` used by the serializable family.

- **Phase 1 — trivial leaves: EventArgs, observers, interfaces, batch/helper (13):** `BagChangedEventArgs`, `ISimpleActionBagObserver`, `SimpleActionBagObserver`, `DictionaryChangedEventArgs`, `SimpleActionDictionaryObserver`, `ILockingLinkedList`, `ILockingLinkedListObserver`, `LockingObservableLinkedListChangedEventArgs`, `SimpleActionLockingLinkedListObserver`, `ObservableCollectionBatchUpdate`, `ObserverHelper`, `AbstractCloneable`, `IConcurrentObservableCollectionSeams`. Establishes the pragma + csharpier + gate loop at near-zero risk.
- **Phase 2 — standalone value/util types (7):** `AsyncQueue`, `AsyncLazy`, `LazyTry`, `StackGeek`, `StackObjectCS`, `TreeNodeOfT`, `DataConverter2d`.
- **Phase 3 — matrices (3):** `DenMatrix`, `JaggedMatrix`, `Matrix`.
- **Phase 4 — timed actions (5):** `TimerWrapper`, `TimedAsyncTask`, `TimedBatchAction`, `TimedQueueOfActions`, `TimedDiskWriter`.
- **Phase 5 — locking core (4):** `LockingLinkedListNode`, `LockingLinkedList`, `LockingObservableLinkedListNode`, `LockingObservableLinkedList`.
- **Phase 6 — concurrent-observable bases + bag (CS8714 decision here) (5):** `ConcurrentObservableBag`, `ConcurrentObservableCollection`, `ConcurrentObservableCollection.Serialization`, `ConcurrentObservableDictionary`, `ObservableDictionary`. The `where TKey : notnull` decision (section 6.1) is ratified in this phase before any dependent consumes it. Keep the `ConcurrentObservableCollection` partial pair together.
- **Phase 7 — SmartSerializable family + config controller (base-first) (7):** `NewSmartSerializableConfig`, `SmartSerializableBase`, `SmartSerializable`, `SmartSerializableStatic`, `SmartSerializableNonTyped`, `SmartSerializableLoader`, `ConfigController`. Highest cross-module contract scrutiny.
- **Phase 8 — serializable wrappers (depend on Phases 6-7) (7):** `SerializableList`, `ScBag`, `ScoDictionaryStatic`, `ScoDictionaryNew`, `SloLinkedList`, `SloStack`, `ScDictionary`.
- **Exempt (not a phase):** `ConfigViewer.cs`, `ConfigViewer.Designer.cs`, `ConfigGroupBox.cs`.

Total in scope: 51 files across 8 phases (13+7+3+5+4+5+7+7). Phase sizes intentionally vary by cohesion; the two contract-critical phases (6 and 7) are grouped for consistent review of the `notnull` and serialization-round-trip decisions.

---

## 8. Test posture (AC3/AC4 no-regression)

Evidence: `UtilitiesCS.Test/` glob. Test coverage across this cluster is near-complete — nearly every in-scope type has a dedicated MSTest file:

- Serialization: `SmartSerializable_Tests`, `SmartSerializableBase_Tests`, `SmartSerializableStatic_Tests`, `SmartSerializableNonTyped_Tests`, `NewSmartSerializableConfig_Tests`, `SerializableList_Tests` + `SerializableListCoverageTests`, `ScBag_Tests`.
- SerializableNew: `ScoDictionaryNew_Tests`, `ScoDictionaryNewTests`, `ScoDictionaryNew_OnDiskCompatibility_Tests`, `ScDictionary_Tests`, `SloLinkedList_Tests`, `SloStack_Tests` + `SloStackUndoContract_Tests`.
- Concurrent/Observable: `ConcurrentObservableDictionaryTests` (+ legacy `ConcurrentObservableDictionaryTest`), `ConcurrentObservableCollection_Tests`, `ConcurrentObservableCollectionSerialization_Tests`, `ConcurrentObservableCollectionLockRecursionTests`.
- Locking: `LockingLinkedList_Tests`, `LockingLinkedListNode_Tests`, `LockingObservableLinkedList_Tests`, `LockingObservableLinkedListNode_Tests`, `SimpleActionLockingLinkedListObserver_Tests`.
- Matrices: `Matrix_Tests`, `DenMatrix_Tests`, `JaggedMatrix_Tests`, `DataConverter2d_Tests`.
- Other/Timed/Lazy/Config: `TreeNodeOfT_Tests`, `AsyncQueue_Tests`, `StackGeek_Tests`, `StackObjectCS_Tests`, `AbstractCloneable_Tests`, `AsyncLazy_Tests`, `LazyTry_Tests`, `ObserverHelper_Tests`, `TimedDiskWriterTests`, `TimedAsyncTask_Tests`, `TimedBatchAction_Tests`, `TimedQueueOfActions_Tests`, `TimerWrapper_Tests` (+ `TestHelpers/ManualFireTimerWrapper`), `ConfigController_Tests` (two locations), `ConfigGroupBox_Tests`, `ConfigViewer_Tests`.

Test-coverage gaps to note (no dedicated file located; verify during execution): the standalone `ObservableDictionary.cs` (834 lines — largest; no `ObservableDictionary_Tests` located, only `ConcurrentObservableDictionary*` and the EmailIntelligence `ObservableDictionary` usages), `ObservableCollectionBatchUpdate.cs`, the `ConcurrentObservableBag`/`SimpleActionBagObserver`/`BagChangedEventArgs` bag family, `SmartSerializableLoader.cs` (likely exercised transitively by `SmartSerializable_Tests`), and the interface/EventArgs leaves (no executable behavior — acceptable at 0% executable coverage per the general-unit-test type-only clarification).

Implication for AC4 (no coverage regression on changed lines): because the edits are annotation-only (pragma, `?`, `!`, constraint additions), they change signatures/metadata but not executable-line counts. The only changed-line-coverage risk is any NEW runtime guard clause added to satisfy flow analysis. Prefer annotation and justified `!` over inserting new `if (x is null) throw` statements, which would add uncovered executable lines (AC4 pressure) and could constitute behavior change (AC3). Note that `ConfigViewer_Tests` / `ConfigGroupBox_Tests` exist even though those files are exempt from the pragma — leaving them null-oblivious does not affect their tests.

### Toolchain / verification (per CLAUDE.md order, matching the sibling child)

1. `csharpier .` before each build (pragma + `?` insertions reformat).
2. Nullable gate: `msbuild TaskMaster.sln /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU" /p:TreatWarningsAsErrors=true` — the pragma-driven gate (matches PR #361's `/t:Rebuild`). Under `TreatWarningsAsErrors`, CS86xx AND CS8714 in a pragma-enabled file become errors while non-opted files stay silent.
3. Do NOT pass `/p:Nullable=enable` for this child's verification — that forces nullable project-wide and surfaces the full pre-existing repo debt, drowning this child's signal. The global-flag-vs-per-file-pragma mismatch is the rules-vs-convention tension the epic defers to the Wave-2 capstone; out of scope here.
4. Analyzer/codestyle step runs as usual; new analyzer severities remain `suggestion` (root `.editorconfig` line 27) so they cannot break the nullable gate.
5. Tests: `vstest.console.exe <UtilitiesCS.Test assembly> /EnableCodeCoverage`.

---

## 9. Rejected alternatives (brief)

- **Project-level `<Nullable>enable`** — rejected by confirmed architecture (AC2); would make no child independently mergeable.
- **Adding a `System.Diagnostics.CodeAnalysis` nullable-attribute polyfill** — rejected: unnecessary (zero CS86xx is reachable without it, proven by the sibling child) and adds new production surface (scope creep). Use `out TValue?`, plain `?`, and `where T : notnull`.
- **Suppressing CS8714 with `#pragma warning disable`** — rejected in favor of the accurate `where TKey : notnull` constraint (section 6.1), pending maintainer ratification.
- **Opting the WinForms Form/Designer/control files into the pragma** — rejected per the COM/VSTO/WinForms posture (section 3); no downstream contract value and high churn/overwrite risk.
- **One large batch or 51 single-file batches** — rejected: the first is unreviewable for a C3 contract change; the second is excessive churn. Cohesive base-first phases balance reviewability against contract-propagation ordering.
