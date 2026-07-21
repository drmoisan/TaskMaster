# Research Findings — TaskTree Testability Refactor (#296)

- Feature: `2026-07-09-tasktree-testability-refactor-296` (child of epic winforms-testability-refactor #295)
- Research timestamp (UTC): 2026-07-09T21:15:00Z
- Scope: research only. No production or test code modified.
- Authoritative issue number for all artifacts/paths/cross-references: **296**.

## Summary

`TaskTree/TaskTreeController.cs` (546 lines) exceeds the 500-line limit and depends on
the concrete `TaskTreeForm` WinForms type. The `TaskTree` project has no test project.
This research produces an implementation-ready design that:

1. Introduces `ITaskTreeForm : UtilitiesCS.Interfaces.IWinForm.IForm` and retargets the
   controller to it, using **intent-named facade members** (not exposed control objects)
   to surface the `TreeListView` operations the controller currently performs directly.
2. Splits the controller into three production files (controller wiring + host-neutral
   move/tree logic + debug CSV writer), each < 500 lines, separating host-neutral logic
   from COM/WinForms interaction.
3. Replaces the two `MessageBox.Show` call sites and the CSV file-writing helpers with
   DI seams (injectable delegate seams) that default to safe production behavior.
4. Creates a new `TaskTree.Test` MSTest + Moq + FluentAssertions project mirroring
   `Tags.Test`, wired into `TaskMaster.sln`, auto-discovered by CI, bringing the
   `TaskTree` project to >= 80% line coverage with no live forms and no popups.

The single external caller (`TaskMaster/Ribbon/RibbonController.cs::LoadTaskTree`) needs
no signature change because the concrete `TaskTreeForm` still implements `ITaskTreeForm`;
the only change is the controller field/parameter type.

---

## A. Current-State Inventory

### A.1 Members the controller accesses on the form / its controls

The controller field is `private TaskTreeForm _viewer`. All accesses (exact):

| Access site (method) | Member on `_viewer` | Type | Operation |
|---|---|---|---|
| ctor | `SetController(this)` | method | back-reference wiring |
| InitializeTreeListView | `TreeLv.CanExpandGetter` (set) | `BrightIdeasSoftware.TreeListView` | delegate set |
| InitializeTreeListView | `TreeLv.ChildrenGetter` (set) | TreeListView | delegate set |
| InitializeTreeListView | `TreeLv.ParentGetter` (set) | TreeListView | delegate set |
| InitializeTreeListView | `TreeLv.ModelFilter` (set) | `IModelFilter` | set `new ModelFilter(...)` |
| InitializeTreeListView | `TreeLv.Roots` (set) | TreeListView | set to `_dataModel.Roots` |
| InitializeTreeListView | `TreeLv.Sort(OlvToDoID, SortOrder.Ascending)` | method | sort |
| InitializeTreeListView | `OlvToDoID` (read) | `OLVColumn` | passed to Sort |
| InitializeTreeListView | `TreeLv.DropSink` (get, cast to `SimpleDropSink`) | property | configure drop sink flags |
| InitializeTreeListView | `SplitContainer1` (read) | `SplitContainer` | passed to ControlResizer |
| InitializeTreeListView | `SplitContainer1.Panel2` (read) | `SplitterPanel` | passed to ControlResizer |
| InitializeTreeListView | `_viewer` itself (read) | `TaskTreeForm` (Control) | passed to `_rs.FindAllControls` / `ResizeAllControls` |
| HandleModelDropped | `TreeLv.ModelFilter` (set), `TreeLv.Sort()` | TreeListView | refilter + sort after drop |
| ToggleExpandCollapseAll | `TreeLv.CollapseAll()` / `TreeLv.ExpandAll()` | method | expand/collapse |
| ResizeForm | `TreeLv.AutoScaleColumnsToContainer()` | method | column autoscale |
| ResizeForm | `_viewer` (read) | Control | passed to `_rs.ResizeAllControls` |
| RebuildTreeVisual | `TreeLv.Roots` (set), `TreeLv.RebuildAll(false)` | TreeListView | rebuild |
| ToggleHideComplete | `TreeLv.ModelFilter` (set null / set filter) | TreeListView | toggle filter |
| GetSelectedTreeNode | `TreeLv.GetItem(TreeLv.SelectedIndex).RowObject` | method + property | read selection |

The drop event handlers (`MoveObjectsToRoots/Sibling/Children`) also receive
`TreeListView` instances **from the BrightIdeasSoftware event args** (`e.ListView`,
`e.SourceListView` cast to `TreeListView`) and call `AddObject` / `RemoveObject` on them.
These `TreeListView` parameters are not read off `_viewer`; they arrive via the event.

Form public surface consumed today: `SetController(TaskTreeController)`, `TreeLv`,
`OlvToDoID`, `SplitContainer1`. `UiSyncContext` / `UiScheduler` are exposed by the form
but **not** referenced by the controller (only used internally by the form's
`TLV_ItemActivate`). The form's private event handlers all forward to controller methods.

### A.2 COM / Outlook-Interop types the controller touches

| Site | COM expression | Interop type |
|---|---|---|
| ActivateOlItem / ActivateOlItemAsync | `_globals.Ol.App.ActiveExplorer()` | `Outlook.Application` → `Outlook.Explorer` |
| ActivateOlItem(Async) | `activeExplorer.IsItemSelectableInView(item)`, `.ClearSelection()`, `.AddToSelection(item)`, `.Activate()` | `Outlook.Explorer` methods |
| ActivateOlItem(Async) | `item.Display()` (dynamic) | late-bound COM |
| TreeLvActivateItem(Async) | `node.Value.OlItem.InnerObject` | `IItem`/COM inner object |
| IsValidType | `item is Outlook.MailItem`, `item is Outlook.TaskItem` | `Outlook.MailItem`, `Outlook.TaskItem` (both interfaces — mockable with Moq) |
| MoveObjectsToSibling | `_globals.TD.IDList.GetNextToDoID(...)`, `(IDList)_globals.TD.IDList` | `IIDList` (pure string logic, no COM) |
| MoveObjectsToChildren / MoveObjectsToSibling | `_dataModel.AddChild(...)`, `ReNumberChildrenIDs(...)` | `ToDoModel` data model (no COM) |

Verified dependency types (grepped): `IApplicationGlobals.Ol` is `IOlObjects`
(`UtilitiesCS/Interfaces/IGlobals/IApplicationGlobals.cs:11`); `IOlObjects.App` is
`Outlook.Application` (`IOlObjects.cs:13`); `IApplicationGlobals.TD` is `IToDoObjects`
(`IApplicationGlobals.cs:12`); `IToDoObjects.IDList` is `IIDList`
(`IToDoObjects.cs:21`). `Outlook.Application`, `Outlook.Explorer`, `Outlook.MailItem`,
`Outlook.TaskItem` are all Interop **interfaces**, so Moq can create mocks for them
without a live Outlook process.

### A.3 Data-model / non-UI logic (host-neutral candidates)

Pure or near-pure (no WinForms/COM once the `TreeListView` and `MessageBox` accesses are
seam-abstracted): `HandleModelCanDrop` (operates on `ModelDropEventArgs`), the three
`MoveObjects*` methods (data-model mutation + tree `AddObject/RemoveObject`),
`FindChildByID` (fully pure recursion), `IsValidType` (type check), and the
`LoopTreeToWrite` traversal. `WriteTreeToDisk`/`AppendLineToCSV` are file-I/O debug
helpers.

### A.4 Toolchain / project constraints

- `TaskTree.csproj`: non-SDK, `packages.config`, `TargetFrameworkVersion v4.8.1`,
  `LangVersion latest`, `OutputType Library`, ProjectGuid
  `{8F7F59E6-18A7-0CF3-0E1D-4478954B612A}`. References `ObjectListView`,
  `Microsoft.Office.Interop.Outlook`, `System.Windows.Forms`; project-refs `ToDoModel`
  and `UtilitiesCS`. Same five-analyzer stack + `BannedSymbols.txt` as the rest of repo.
- net48 constraints from memory (`reference_net48_no_init_record_struct`): no
  `IsExternalInit` — do not use `init`/`record`/`record struct`; use plain classes or
  `readonly struct`.
- `BannedSymbols.txt` bans `Thread.Sleep`, `Task.Delay`, `DateTime.Now/UtcNow`,
  `Random.Shared` (currently `suggestion` severity, but tests must avoid them anyway per
  determinism policy).

---

## B. `ITaskTreeForm` Design

### B.1 Recommendation: intent-named facade members (not exposed controls)

**Recommended:** surface the operations the controller needs as **intent-named facade
members on `ITaskTreeForm`**, not as exposed `TreeListView`/`OLVColumn`/`SplitContainer`
properties.

Rationale:
- `BrightIdeasSoftware.TreeListView` is a concrete third-party WinForms `Control`.
  Moq cannot mock a concrete non-virtual control, and constructing a real `TreeListView`
  creates a live control that needs a window handle for several operations
  (`GetItem`, `RebuildAll`, `AutoScaleColumnsToContainer`). Exposing `TreeLv` on the
  interface (the `IStoreWrapperViewer` style, which exposes concrete `Label`/`Button`
  controls) would force tests to instantiate real controls — a direct violation of the
  epic's "no live forms" rule and of the unit-test policy.
- Intent-named members let a `Mock<ITaskTreeForm>` satisfy every controller access with a
  pure verifiable call. The mapping from intent to `TreeLv` lives entirely inside the
  concrete `TaskTreeForm` (host-bound, exempt-eligible), keeping the controller and its
  tests control-free.
- This matches the epic's "Seams over UI-thread execution" and "COM/logic separation"
  principles more closely than the older `IStoreWrapperViewer` control-exposure shape.

The `IStoreWrapperViewer` pattern (exposing `Label`/`Button`/`ComboBox`) is noted as an
in-repo precedent but is **rejected** for this feature because those controls are simple
`get/set` data holders, whereas `TreeListView` here is driven through behavior-rich method
calls that are not mock-friendly as a concrete control.

### B.2 Proposed `ITaskTreeForm` members

`ITaskTreeForm : UtilitiesCS.Interfaces.IWinForm.IForm`. Facade members (all intent-named,
no WinForms control types leak except delegate/`IModelFilter` shapes already domain-facing):

Controller wiring:
- `void SetController(TaskTreeController controller);` (already on the form)

Tree configuration / lifecycle (replaces direct `TreeLv` access):
- `void InitializeTreeView(IEnumerable<TreeNode<ToDoItem>> roots, Predicate<object> incompleteFilter);`
  — encapsulates setting `CanExpandGetter/ChildrenGetter/ParentGetter`, `ModelFilter`,
  `Roots`, initial `Sort(OlvToDoID, Ascending)`, and drop-sink flag configuration
  (the `SimpleDropSink` setup). These are pure designer-wired defaults with no branching,
  so folding them behind one facade member keeps the controller logic-free of controls.
- `void SetModelFilter(Predicate<object> filter);` — `filter == null` clears the filter;
  supports `ToggleHideComplete` and the post-drop refilter.
- `void SortTree();` — parameterless re-sort (post-drop).
- `void ExpandAllNodes();` / `void CollapseAllNodes();` — for `ToggleExpandCollapseAll`.
- `void RebuildTree(IEnumerable<TreeNode<ToDoItem>> roots);` — sets roots + `RebuildAll(false)`.
- `void AutoSizeTreeColumns();` — `AutoScaleColumnsToContainer()`.
- `TreeNode<ToDoItem> GetSelectedNode();` — wraps
  `TreeLv.GetItem(TreeLv.SelectedIndex).RowObject as TreeNode<ToDoItem>` including the
  existing try/catch-returns-null behavior.

Resize wiring (host-bound; see D/G — kept in the form, exempt-eligible):
- `void ResizeControls();` — wraps `ControlResizer.ResizeAllControls(this)` plus the
  `FindAllControls`/`SetResizeDimensions` initialization currently in the controller.

Notes on `IForm` overlap:
- `IForm` already declares `Load`, `Activated`, `Close()`, `Show(...)`, etc. The
  controller does not consume those, so there is no member collision. `SetController` and
  all tree facade members are additive and unique to `ITaskTreeForm`.
- `IForm` derives from `IContainerControl, IScrollableControl`; the concrete `Form`
  already satisfies these. No new implementation burden beyond the facade methods, all of
  which the concrete form implements as thin delegations to `TreeLv`/`ControlResizer`.
- `TreeNode<ToDoItem>` and `IModelFilter`/`Predicate<object>` are domain/OLV types the
  controller already uses; surfacing them on the interface introduces no new control
  coupling. `Predicate<object>` is preferred over `IModelFilter` on the interface so the
  controller never constructs `ModelFilter` (a BrightIdeasSoftware type); the concrete
  form wraps the predicate in `new ModelFilter(predicate)`.

The `MoveObjects*` methods take `TreeListView` parameters sourced from the drop event
args, not from the interface. See C.3 for how those are handled so the move logic becomes
testable.

---

## C. Seam Design (dialogs, file I/O, UI-thread, tree-control event params)

Per `.claude/rules/csharp.md` seam preference order (interface > injectable delegate >
adapter). Each seam has a safe production default so existing behavior is unchanged.

### C.1 MessageBox seam (injectable delegate)

Two call sites: `MoveObjectsToSibling` and `MoveObjectsToChildren` each call
`MessageBox.Show("Error ... out of sync at roots")` on a data-model/UI desync. These are
the only dialog calls in the controller.

- Seam: `private readonly Action<string> _showMessage;`
- Constructor default: `_showMessage = showMessage ?? (m => MessageBox.Show(m));`
- Signature added to controller ctor: `Action<string> showMessage = null` (optional, last
  parameter, so the production caller is unchanged).
- Tests inject a recording `Action<string>` and assert the message fires on the desync
  branch (negative/edge case) and does **not** fire on the happy path.

Interface seam (`IMessageBoxService`) is heavier than needed for a single `void(string)`
call path; the injectable delegate is the smallest sufficient seam and keeps the
production default deterministic.

### C.2 CSV writer seam (injectable delegate) for debug helpers

`WriteTreeToDisk` / `AppendLineToCSV` write to disk via `StreamWriter`/`File.AppendText`.
Unit-test policy prohibits temp files. Options:

- Recommended: extract the traversal (`LoopTreeToWrite`) to operate against an injected
  `Action<string>` line-sink (default writes to the CSV file). Then `LoopTreeToWrite` is a
  pure, fully testable traversal (assert the emitted lines for a known tree), and only the
  thin `WriteTreeToDisk` file-open wrapper remains host-bound.
- Signature: `internal void LoopTreeToWrite(IReadOnlyList<TreeNode<ToDoItem>> nodes, Action<string> writeLine, string linePrefix)`.
- `WriteTreeToDisk(string filepath)` keeps its `StreamWriter` open/close and passes
  `sw.WriteLine` (or a closure) as the sink; its remaining 2–3 I/O lines are exempt-eligible.

These debug helpers are `public` today but have no in-repo callers (grep: referenced only
within the controller). They may alternatively be deleted as dead code; that is a scope
decision for the planner. If retained, the seam above makes the traversal testable.

### C.3 Drop-event `TreeListView` parameters (adapter via narrow interface — or logic split)

`HandleModelDropped` dispatches to `MoveObjects*`, passing `e.ListView`/`e.SourceListView`
(concrete `TreeListView`) as `targetTree`/`sourceTree`; the move methods call
`AddObject`/`RemoveObject`/`ReferenceEquals` on them. To make the move logic testable:

- Recommended: define a narrow interface `ITreeVisual` with the exact members the move
  methods use — `void AddObject(object model); void RemoveObject(object model);` — and
  change the `MoveObjects*` signatures to accept `ITreeVisual` rather than `TreeListView`.
  A tiny adapter `TreeListViewVisual : ITreeVisual` (in the form/host-bound file) wraps a
  `TreeListView`. `ReferenceEquals(sourceTree, targetTree)` becomes reference comparison of
  the adapters (or the wrapped controls); tests pass two distinct/identical mock
  `ITreeVisual` instances.
- Rationale: this is the adapter seam (level 3) because `TreeListView.AddObject`/
  `RemoveObject` are third-party control methods that cannot be mocked directly. The
  interface is minimal (two methods actually used).
- Consequence: the three `MoveObjects*` methods plus `HandleModelDropped`'s post-drop
  refilter/sort become unit-testable against `Mock<ITreeVisual>` + `Mock<ITaskTreeForm>` +
  a real `TreeOfToDoItems`/`IDList`, asserting both data-model state
  (`_dataModel.Roots`, parent/child links, renumbered `ToDoID`s) and the visual calls
  (`AddObject`/`RemoveObject` verified via Moq).

### C.4 UI-thread / async

`ActivateOlItemAsync` uses `Task.Run` for COM activation. The form's `TLV_ItemActivate`
handles `SynchronizationContext`. No `Thread.Sleep`/`Task.Delay` exists in the controller.
For tests, `ActivateOlItem` (sync) is exercised against mocked `Outlook.Explorer`; the
async variant's COM branch can be covered by mocking the explorer and asserting
`ClearSelection`/`AddToSelection`/`Activate` or `Display()` are invoked. `Task.Run`
wrapping is preserved in production; tests await the returned task deterministically (no
timing dependence — the mock returns synchronously).

---

## D. File Decomposition (each < 500 lines)

Split `TaskTreeController.cs` (546) into three partial-class files plus the new interface.
Keeping one `partial class TaskTreeController` preserves the public type and private-field
sharing while dividing responsibilities. Approximate line counts include headers/usings.

| File | Responsibility | Approx lines | Host coupling |
|---|---|---|---|
| `TaskTree/ITaskTreeForm.cs` | New interface (B.2); `ITreeVisual` may live here or in its own file | ~45 | interface only |
| `TaskTree/TaskTreeController.cs` | ctor + fields + `InitializeTreeListView` wiring + expand/collapse/resize/rebuild/filter toggles + `GetSelectedTreeNode` + Outlook activation (COM) | ~250 | `ITaskTreeForm` facade + COM (mockable) |
| `TaskTree/TaskTreeController.MoveLogic.cs` | `HandleModelCanDrop`, `HandleModelDropped`, `MoveObjectsToRoots/Sibling/Children`, `FindChildByID`, `IsValidType` (host-neutral move/data logic against `ITreeVisual` + data model) | ~230 | none (pure + `ITreeVisual`) |
| `TaskTree/TaskTreeController.Debug.cs` | `WriteTreeToDisk`, `LoopTreeToWrite`, `AppendLineToCSV` (or delete if dead) | ~40 | thin file I/O only |

`TaskTreeForm.cs` (108) gains the `ITaskTreeForm` implementation (facade methods +
`ITreeVisual` adapter wiring); it stays well under 500. `TaskTreeForm.Designer.cs` (311)
is designer-generated and unchanged. All resulting files are < 500 lines.

Alternative considered (separate non-partial helper class holding pure move logic): adds a
new public type and requires passing `_dataModel`/`_globals`/seams explicitly. Rejected in
favor of partial files because the move logic reads several private fields
(`_dataModel`, `_globals`, `_filterCompleted`) and partials keep that access without
widening the public surface.

---

## E. Callers and Required Changes

Repo-wide grep for `TaskTreeController` / `TaskTreeForm` — the only production caller is:

- `TaskMaster/Ribbon/RibbonController.cs::LoadTaskTree` (lines ~88–95):
  ```
  var taskTreeViewer = new TaskTreeForm();
  var dataModel = new TreeOfToDoItems([]);
  dataModel.LoadTree(TreeOfToDoItems.LoadOptions.vbLoadInView, Globals);
  var taskTreeController = new TaskTreeController(Globals, taskTreeViewer, dataModel);
  taskTreeViewer.Show();
  ```
  Required change: **none to this call site** if the controller ctor keeps a
  `TaskTreeForm`-compatible parameter. The controller parameter type changes from
  `TaskTreeForm Viewer` to `ITaskTreeForm Viewer`; because `TaskTreeForm` implements
  `ITaskTreeForm`, `new TaskTreeForm()` still binds. The optional `Action<string> showMessage`
  seam parameter defaults to null, so the existing 3-argument construction is unchanged.
  `_viewer.SetController(this)` remains valid (member is on the interface).

Other matches are documentation/spec/epic files and `.bak`/`.vbproj.bak` files (not
compiled — `TaskTree.csproj` only compiles `TaskTreeController.cs`, `TaskTreeForm.cs`,
`TaskTreeForm.Designer.cs`, `Properties/AssemblyInfo.cs`). No other production caller
exists. The controller's own file and the form file are updated as part of the refactor.

---

## F. New `TaskTree.Test` Project

Mirror `Tags.Test` (verified structure). Concrete requirements:

### F.1 csproj contents (`TaskTree.Test/TaskTree.Test.csproj`)

- **New unique ProjectGuid** (generate one; must not collide with existing GUIDs). Do not
  reuse `Tags.Test`'s `{486C1CAE-5C32-406E-963F-79F654EC9B07}`.
- `ProjectTypeGuids`: `{3AC096D0-A1C2-E12C-1390-A8335801FDAB};{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}`
  (test + C# project type), exactly as `Tags.Test`.
- `TargetFrameworkVersion v4.8.1`; `OutputType Library`; `AssemblyName TaskTree.Test`;
  `RootNamespace TaskTree.Test`; `TestProjectType UnitTest`.
- **Top-of-file `Import` props** for the Microsoft.Testing.Platform + MSTest.TestAdapter
  chain (copy the four `<Import ... .props>` lines and the bottom `<Import ... .targets>`
  lines and the `EnsureNuGetPackageBuildImports` target verbatim from `Tags.Test`,
  adjusting nothing but keeping the `..\packages\...` relative paths — `TaskTree.Test` sits
  as a sibling of `TaskTree`, one level under repo root, same as `Tags.Test`).
- **Package references / hint paths** (mirror `Tags.Test.csproj` and its `packages.config`):
  - MSTest: `MSTest.TestFramework 4.2.2` (+ `.Extensions`), `MSTest.TestAdapter 4.2.2`,
    `MSTest.Analyzers 4.2.2` (as `<Analyzer>` items), `Microsoft.Testing.Platform 2.2.2`
    stack, `Microsoft.TestPlatform.ObjectModel 18.5.1`, `Microsoft.TestPlatform.AdapterUtilities 18.5.1`.
  - Moq: `Moq 4.20.72` (`..\packages\Moq.4.20.72\lib\net462\Moq.dll`) + `Castle.Core 5.2.1`.
  - FluentAssertions: `8.9.0` (`..\packages\FluentAssertions.8.9.0\lib\net47\FluentAssertions.dll`).
  - `Microsoft.Office.Interop.Outlook 15.0.0.0` (`EmbedInteropTypes False`) — required to
    mock `Explorer`/`MailItem`/`TaskItem` and construct `IsValidType` inputs.
  - `System.Windows.Forms`, `System.Drawing`, `System.Core`, `System.Data`, etc. (BCL refs
    as in `Tags.Test`). `ObjectListView` reference from
    `..\packages\ObjectListView.Official.2.9.1\lib\net20\ObjectListView.dll` — needed if
    tests reference `TreeListView`/`ModelDropEventArgs`/`OLVColumn` types (they should
    avoid constructing controls, but the `ITreeVisual` adapter and event-arg types come
    from this assembly; add the reference to compile).
  - The full transitive Microsoft.Extensions / OpenTelemetry / Identity reference list in
    `Tags.Test.csproj` is present because `Tags` pulls those in transitively; `TaskTree`
    depends on `ToDoModel` + `UtilitiesCS`, so the same transitive closure is required.
    Copy the reference block from `Tags.Test.csproj` wholesale and keep the matching
    `packages.config` so restore succeeds. Prune only if a reference fails to resolve.
- **ProjectReferences**: `..\TaskTree\TaskTree.csproj`
  (`{8F7F59E6-18A7-0CF3-0E1D-4478954B612A}`), `..\UtilitiesCS\UtilitiesCS.csproj`
  (`{91b5f9bb-aa29-4dda-9e26-d3dad73ec7ca}`), and `..\ToDoModel\ToDoModel.csproj`
  (`{241d7156-b046-4b65-b0ac-1cdff6d90c6b}`) — TaskTree tests use `TreeNode<ToDoItem>` /
  `TreeOfToDoItems` / `IDList` from ToDoModel and `IApplicationGlobals` from UtilitiesCS.
- **Compile items**: `Properties\AssemblyInfo.cs` plus the new test `.cs` files.
- **Analyzer `<ItemGroup>`**: copy the five-analyzer stack + `MSTest.Analyzers` +
  `<AdditionalFiles Include="$(MSBuildThisFileDirectory)..\BannedSymbols.txt" />` exactly
  as in `Tags.Test.csproj`.
- **`app.config`**: copy `Tags.Test/app.config` verbatim (the binding-redirect set is
  required so the Extensions/Identity/Testing assemblies load at runtime under vstest).

### F.2 AssemblyInfo (`TaskTree.Test/Properties/AssemblyInfo.cs`)

Mirror `Tags.Test/Properties/AssemblyInfo.cs`: titles/product `TaskTree.Test`,
`[assembly: ComVisible(false)]`, `[assembly: Guid("<same-lowercase-as-new-ProjectGuid>")]`,
`AssemblyVersion 1.0.0.0`.

### F.3 Solution wiring (`TaskMaster.sln`)

- Add a `Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "TaskTree.Test",
  "TaskTree.Test\TaskTree.Test.csproj", "{<NEW-GUID>}"` / `EndProject` entry (same shape as
  the existing `Tags.Test` entry at sln line 37).
- Add the four `GlobalSection(ProjectConfigurationPlatforms)` lines for the new GUID
  (Debug|Any CPU + Release|Any CPU, `ActiveCfg` + `Build.0`), matching how `Tags.Test`'s
  GUID is configured. Confirm the platform token used by the other test projects
  (`Any CPU` vs `AnyCPU`) and match it exactly to avoid an unbuilt configuration.

### F.4 Coverage / CI discovery

- CI (`.github/workflows/ci.yml`, "Run MSTest suite with coverage") discovers test
  assemblies by `Get-ChildItem -Recurse -Filter '*.Test.dll'` filtered to
  `\bin\$BUILD_CONFIGURATION\` (not `obj`/`ref`), then runs
  `vstest.console.exe <assemblies> /EnableCodeCoverage /InIsolation /Logger:trx
  /TestCaseFilter:"TestCategory!=LiveOutlook"`. Because the assembly is named
  `TaskTree.Test.dll` and builds into `TaskTree.Test\bin\<config>\`, it is **auto-discovered**
  with no CI edit required.
- Do **not** mark TaskTree unit tests `[TestCategory("LiveOutlook")]` (that filter excludes
  them). All new tests must be non-LiveOutlook.
- Coverage exclusion configs (`coverage.config`, `TaskMaster.runsettings`) exclude only
  third-party module paths (Deedle/FSharp/Castle/FluentAssertions/Moq/MSTest/Microsoft.Testing).
  `TaskTree.dll` is first-party and will be measured. No config change needed for the
  production assembly to appear in the coverage denominator.
- Per-project 80% is measured against `TaskTree.dll`'s instrumented lines. The
  `TaskTree.Test.dll` assembly itself is test code and is not the coverage target.

---

## G. Per-Behavior Test Plan

All tests: MSTest `[TestClass]/[TestMethod]`, Moq, FluentAssertions, AAA structure, no
real `Form`/`Control`, no popups, no `Thread.Sleep`/`Task.Delay`, no temp files,
deterministic. Dependencies mocked: `ITaskTreeForm`, `ITreeVisual`, `IApplicationGlobals`
(→ `IOlObjects.App` → `Outlook.Explorer`; `IToDoObjects.IDList` → **real** `IDList`),
`Action<string>` message seam, `Action<string>` line-sink. Real domain objects:
`TreeOfToDoItems`, `TreeNode<ToDoItem>`, `ToDoItem`, `IDList` (all COM-free per grep).

| Behavior | Test approach | Positive / Negative / Edge |
|---|---|---|
| ctor wiring | Construct with mocked `ITaskTreeForm`; assert `SetController(controller)` invoked (Moq Verify) | Positive: SetController called. Edge: message seam null → default assigned (no throw). |
| `InitializeTreeListView` | Mock `ITaskTreeForm.InitializeTreeView`/`ResizeControls`; verify called with `_dataModel.Roots` and a non-null incomplete filter predicate | Positive: facade invoked. Edge: empty roots. (The `ControlResizer` calls move behind `ResizeControls`.) |
| `HandleModelCanDrop` | Build `ModelDropEventArgs` (BrightIdeasSoftware type; set properties) with source/target `TreeNode<ToDoItem>`; assert `e.Effect`/`e.InfoMessage`/`e.Handled` | Positive: reorder above-item → Move. Negative: drop on self → None + "Cannot drop on self". Edge: background with all-roots → "already roots"; drop on descendant → paradox message. |
| `HandleModelDropped` dispatch | Mock `ITreeVisual` source/target via event args; real data model; verify correct `MoveObjects*` effect + post-drop `SetModelFilter`/`SortTree` on `ITaskTreeForm` | Positive: each `DropTargetLocation` routes correctly. Edge: `default` returns early (no refilter). |
| `MoveObjectsToRoots` | Real `TreeOfToDoItems`; `ITreeVisual` mocks; same vs different tree | Positive: same-tree child promoted (parent.RemoveChild + `AddObject`). Cross-tree: `RemoveObject`/`AddObject` + parent nulled. Edge: node already root (no-op branch). |
| `MoveObjectsToSibling` | Real data model + real `IDList`; assert `_dataModel.Roots` order and renumbered `ToDoID`s via `GetNextToDoID`; message seam recorded | Positive: insert as sibling under parent (`ReNumberChildrenIDs`). Positive: insert among roots (ID reseed). Negative: root not in `_dataModel.Roots` → message seam fires. Edge: `siblingOffset` 0 vs 1. |
| `MoveObjectsToChildren` | Real data model; `ITreeVisual` mock; verify `AddChild` result and `RemoveObject` for former roots | Positive: root moved to child (`RemoveObject` + `Roots.Remove` + `AddChild`). Negative: desync (not in Roots) → message seam fires. Edge: non-root child reparent. |
| `ToggleExpandCollapseAll` | Mock `ITaskTreeForm.ExpandAllNodes/CollapseAllNodes`; call twice | Positive: first call expands, flips `_expanded`; second collapses. State transition covered. |
| `ToggleHideComplete` | Mock `SetModelFilter`; call twice | Positive: first clears filter (`_filterCompleted` false); second sets incomplete filter. State transition. |
| `RebuildTreeVisual` | Verify `RebuildTree(_dataModel.Roots)` | Positive. Edge: empty roots. |
| `ResizeForm` | Verify `ResizeControls` + `AutoSizeTreeColumns` | Positive. |
| `GetSelectedTreeNode` | Mock `ITaskTreeForm.GetSelectedNode` returning node or null | Positive: returns node. Negative: null (no selection). |
| `IsValidType` | Pass `Mock<Outlook.MailItem>.Object`, `Mock<Outlook.TaskItem>.Object`, and an arbitrary object | Positive: MailItem/TaskItem → true. Negative: other → false. |
| `TreeLvActivateItem` / `Async` | Mock `GetSelectedNode` → node with `OlItem.InnerObject`; mock `IApplicationGlobals.Ol.App.ActiveExplorer()` → `Mock<Explorer>`; assert `ClearSelection`/`AddToSelection` or `Display`/message per validity | Positive: selectable → clear+add. Positive: not selectable → `Display()`. Negative: unsupported type → message seam. Edge: null selection → no-op. |
| `ActivateOlItem(Async)` | Mock `Explorer.IsItemSelectableInView` true/false | Positive: selectable path; else `Display`. Edge: null item → no-op. Async awaited deterministically (mock returns sync). |
| `FindChildByID` | Real nested `TreeNode` tree | Positive: found at depth. Negative: not found → null. Edge: null/empty ID matching (`?? ""`). |
| `LoopTreeToWrite` | Inject recording `Action<string>` line-sink; known tree | Positive: emits expected prefixed lines. Edge: null nodes → no emission. (No file I/O in the test.) |

### G.1 Irreducible UI/COM lines (candidate `[ExcludeFromCodeCoverage]`, minimized)

The following remain host-bound and cannot be covered without a live control/window;
these live in `TaskTreeForm.cs` (form) or the thinnest controller wrappers and are the
**only** exemption candidates, each requiring maintainer ratification per policy:

- The concrete `TaskTreeForm` facade implementations (thin delegations to `TreeLv.*` and
  `ControlResizer.*`) and the `TreeListViewVisual : ITreeVisual` adapter. `TaskTreeForm`
  is a `Form`-derived class and already falls under the ratified WinForms exemption
  category (form-derived + Designer code) in the coverage policy.
- `WriteTreeToDisk`'s `StreamWriter` open/`File.AppendText` lines (2–3 lines) if the debug
  helpers are retained; the traversal itself (`LoopTreeToWrite`) is **not** exempt and is
  tested via the line-sink seam. Preferred: delete the debug helpers (no callers) to
  remove the exemption entirely.

Testable seams (the `ITaskTreeForm` facade consumers in the controller, the `ITreeVisual`
move logic, the message seam, the line-sink) are **never** exempt and must meet the floor.
Coverage strategy note: the bulk of `TaskTreeController` becomes testable because every
control access is behind `ITaskTreeForm`/`ITreeVisual`, every dialog behind the message
seam, and every COM call behind mockable Interop interfaces. Achieving >= 80% line coverage
on `TaskTree.dll` is feasible because, after extraction, the only unavoidably-uncovered
lines are the form's designer/host wiring (already exemption-eligible) — the controller
partial files are fully reachable from mocks.

---

## Automation Feasibility

This refactor is code-only. It requires no third-party UI portal step, no external
service, no human-in-the-loop approval, and no live Outlook/WinForms process at
implementation or test time. Every new behavior is exercised through Moq mocks
(`ITaskTreeForm`, `ITreeVisual`, `IApplicationGlobals`/`Explorer`/`MailItem`/`TaskItem`),
injectable delegate seams (message box, CSV line-sink), and real COM-free domain objects
(`TreeOfToDoItems`, `TreeNode<ToDoItem>`, `IDList`). The work is fully automatable via the
standard C# toolchain in order: `csharpier .` → analyzers build
(`/p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`) → nullable type-check build
(`/p:Nullable=enable /p:TreatWarningsAsErrors=true`) → `vstest.console.exe TaskTree.Test.dll
/EnableCodeCoverage`. No manual or portal step is required. The only human gate is the
policy-mandated maintainer ratification of any residual `[ExcludeFromCodeCoverage]`
attributes (Section G.1), which is a review approval, not an implementation-time external
dependency.

---

## Rejected Alternatives (brief)

- **Expose concrete controls on `ITaskTreeForm`** (the `IStoreWrapperViewer` style):
  rejected because `TreeListView` is a behavior-rich third-party control that cannot be
  Moq-mocked and forces live-control instantiation in tests. Intent-named facade members
  chosen instead (Section B.1).
- **Extract move logic into a separate public helper class** rather than partial files:
  rejected because the logic reads several private controller fields; partial classes
  keep field access without widening the public surface (Section D).
- **Full `IMessageBoxService` interface seam** for two dialog calls: rejected as heavier
  than needed; a `void(string)` injectable delegate is the smallest sufficient seam
  (Section C.1).
