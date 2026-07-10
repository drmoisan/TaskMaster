# tasktree-testability-refactor - Refactor Spec

- **Issue:** #296
- **Parent (optional):** winforms-testability-refactor (#295)
- **Owner:** drmoisan
- **Last Updated:** 2026-07-09
- **Status:** Ready for Planning
- **Version:** 1.0

> `user-story.md` is intentionally NOT applicable for this refactor child. Per the epic
> manifest (`docs/features/epics/winforms-testability-refactor/epic.md`, "Design-Phase
> Deliverables"), refactor children are enabler work with no end-user narrative, so no
> user story is authored and the file is intentionally absent. Acceptance-criteria
> tracking for this feature uses `issue.md` and this `spec.md` only.

## Intent & Outcomes

This is a testability refactor of the `TaskTree` project. It preserves all observable
behavior while restructuring the code so the controller logic is unit-testable without a
live UI, and it brings the `TaskTree` project to at least 80% line coverage.

Target outcomes:

- **`ITaskTreeForm` facade.** Introduce an `ITaskTreeForm` interface deriving from
  `UtilitiesCS.Interfaces.IWinForm.IForm`. The interface exposes intent-named facade
  members (for example `InitializeTreeView`, `SetModelFilter`, `SortTree`,
  `ExpandAllNodes`/`CollapseAllNodes`, `RebuildTree`, `AutoSizeTreeColumns`,
  `GetSelectedNode`, `ResizeControls`, and the existing `SetController`) that surface the
  operations the controller currently performs directly against the concrete form and its
  controls. The concrete `TreeListView` control (`TreeLv`) is deliberately NOT exposed on
  the interface; every control access is expressed as an intent-named facade call, and the
  mapping from intent to `TreeLv`/`ControlResizer` lives entirely inside the concrete
  `TaskTreeForm`. `TaskTreeForm` implements `ITaskTreeForm`; `TaskTreeController` depends on
  `ITaskTreeForm` rather than the concrete form.
- **COM/logic separation.** Host-neutral tree and move/business logic is extracted into a
  separate file from COM/WinForms interaction, minimizing methods that mix COM calls with
  pure logic.
- **File-size compliance.** All resulting production files in `TaskTree` are <= 500 lines.
- **New test project.** A NEW `TaskTree.Test` project (MSTest + Moq + FluentAssertions),
  mirroring the existing `Tags.Test` project, is created and wired into `TaskMaster.sln`.
- **Coverage floor.** The `TaskTree` project reaches >= 80% line coverage, with new classes
  targeting >= 90%, without instantiating real Windows Forms objects.

## Invariants (must not change)

The following behaviors, contracts, and external surfaces must remain identical:

- **Observable tree UI behavior.** All end-user-visible behavior of the task tree
  (expand/collapse, sort order, hide-complete filtering, rebuild, selection activation,
  column autosize) is unchanged. This is a refactor with no behavior or UX change.
- **Drag/drop move behavior.** The existing behavior of drag/drop moves — move-to-roots,
  move-to-sibling (including sibling renumbering via `GetNextToDoID`/`ReNumberChildrenIDs`),
  and move-to-children — is preserved exactly, including the desync error dialog on
  out-of-sync roots.
- **Single caller compiles unchanged.** The single external production caller,
  `TaskMaster/Ribbon/RibbonController.cs::LoadTaskTree`, must compile with no call-site
  edit. Per the research, the controller constructor keeps a `TaskTreeForm`-compatible
  parameter (the parameter type changes from the concrete `TaskTreeForm` to the
  `ITaskTreeForm` interface, which `TaskTreeForm` implements), and the new
  `Action<string> showMessage` seam parameter is optional (defaults to `null`), so the
  existing three-argument `new TaskTreeController(Globals, taskTreeViewer, dataModel)`
  construction and `taskTreeViewer.Show()` remain valid without modification.
- **No new production dependencies.** No new NuGet packages or third-party references are
  added to the `TaskTree` production project. Seams use only injectable delegates and
  narrow first-party interfaces.
- **Performance characteristics.** No latency, throughput, or memory regression; the
  refactor changes structure, not runtime work.
- **Compatibility guarantees.** No CLI flags, config schemas, or public data formats change.

## Scope (structural changes)

### Facade and interface

- Add `ITaskTreeForm : UtilitiesCS.Interfaces.IWinForm.IForm` with intent-named facade
  members. Summary member list (per research Section B.2): `SetController`,
  `InitializeTreeView(IEnumerable<TreeNode<ToDoItem>> roots, Predicate<object> incompleteFilter)`,
  `SetModelFilter(Predicate<object> filter)` (`null` clears the filter), `SortTree()`,
  `ExpandAllNodes()`, `CollapseAllNodes()`, `RebuildTree(IEnumerable<TreeNode<ToDoItem>> roots)`,
  `AutoSizeTreeColumns()`, `GetSelectedNode()` (returns `TreeNode<ToDoItem>` or `null`),
  and `ResizeControls()`. `Predicate<object>` is used on the interface (not
  `IModelFilter`) so the controller never constructs the BrightIdeasSoftware `ModelFilter`
  type; the concrete form wraps the predicate. `TreeLv`/`OLVColumn`/`SplitContainer` are
  NOT exposed.
- `TaskTreeForm` implements `ITaskTreeForm` as thin delegations to `TreeLv` and
  `ControlResizer`. The controller's field/parameter type changes from `TaskTreeForm` to
  `ITaskTreeForm`.

### `ITreeVisual` adapter (drop-event control parameters)

- Define a narrow interface `ITreeVisual` with the exact two members the move methods use:
  `void AddObject(object model);` and `void RemoveObject(object model);`. Change the
  `MoveObjects*` signatures to accept `ITreeVisual` in place of the concrete `TreeListView`
  sourced from BrightIdeasSoftware drop-event args. A tiny host-bound adapter
  `TreeListViewVisual : ITreeVisual` wraps a `TreeListView`. Reference comparison of
  source/target (formerly `ReferenceEquals` on the controls) is performed against the
  adapters/wrapped controls.

### Injectable delegate seams

- **MessageBox seam.** Replace the two `MessageBox.Show` desync call sites in
  `MoveObjectsToSibling` and `MoveObjectsToChildren` with a
  `private readonly Action<string> _showMessage` seam. Constructor default:
  `_showMessage = showMessage ?? (m => MessageBox.Show(m));`. The ctor gains an optional,
  last-position `Action<string> showMessage = null` parameter.
- **CSV line-sink seam.** For the debug traversal, extract `LoopTreeToWrite` to write
  through an injected `Action<string>` line-sink (default writes to the CSV file), making
  the traversal a pure, testable transform. See Non-Goals for the preferred deletion of the
  debug helpers.

### File decomposition (partial-class split; each < 500 lines)

Split `TaskTreeController.cs` (546 lines) into partial-class files, preserving the public
type and private-field sharing (per research Section D):

| File | Responsibility | Approx lines | Host coupling |
|---|---|---|---|
| `TaskTree/ITaskTreeForm.cs` | New `ITaskTreeForm` interface; `ITreeVisual` may live here or in its own file | ~45 | interface only |
| `TaskTree/TaskTreeController.cs` | ctor + fields + `InitializeTreeListView` wiring + expand/collapse/resize/rebuild/filter toggles + `GetSelectedTreeNode` + Outlook activation (COM behind mockable Interop interfaces) | ~250 | `ITaskTreeForm` facade + COM (mockable) |
| `TaskTree/TaskTreeController.MoveLogic.cs` | `HandleModelCanDrop`, `HandleModelDropped`, `MoveObjectsToRoots/Sibling/Children`, `FindChildByID`, `IsValidType` (host-neutral move/data logic against `ITreeVisual` + data model) | ~230 | none (pure + `ITreeVisual`) |
| `TaskTree/TaskTreeController.Debug.cs` | `WriteTreeToDisk`, `LoopTreeToWrite`, `AppendLineToCSV` (or delete if dead — see Non-Goals) | ~40 | thin file I/O only |

`TaskTreeForm.cs` (108 lines) gains the `ITaskTreeForm` facade implementation and the
`ITreeVisual` adapter wiring; it stays well under 500. `TaskTreeForm.Designer.cs` (311
lines, designer-generated) is unchanged.

### New `TaskTree.Test` project blueprint

Mirror `Tags.Test` exactly (per research Section F):

- **Legacy non-SDK csproj**, `TargetFrameworkVersion v4.8.1` (net481), `OutputType Library`,
  `AssemblyName`/`RootNamespace` `TaskTree.Test`, `TestProjectType UnitTest`.
- **New unique ProjectGuid** (generate; must not collide with existing GUIDs — do not reuse
  `Tags.Test`'s GUID). `ProjectTypeGuids`
  `{3AC096D0-A1C2-E12C-1390-A8335801FDAB};{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}`.
- **`packages.config`** mirroring `Tags.Test`.
- **Package set:** MSTest 4.2.2 (`MSTest.TestFramework` + `.Extensions`,
  `MSTest.TestAdapter`, `MSTest.Analyzers`), Moq 4.20.72 (+ `Castle.Core 5.2.1`),
  FluentAssertions 8.9.0, the Microsoft.Testing.Platform chain
  (`Microsoft.Testing.Platform 2.2.2` stack, `Microsoft.TestPlatform.ObjectModel 18.5.1`,
  `Microsoft.TestPlatform.AdapterUtilities 18.5.1`), `Microsoft.Office.Interop.Outlook
  15.0.0.0` (`EmbedInteropTypes False`), `ObjectListView.Official.2.9.1`, and the BCL /
  transitive Microsoft.Extensions / OpenTelemetry / Identity reference closure copied from
  `Tags.Test.csproj` (with matching `packages.config`).
- **Testing.Platform Import chain:** copy the top-of-file `<Import ... .props>` lines, the
  bottom `<Import ... .targets>` lines, and the `EnsureNuGetPackageBuildImports` target
  verbatim from `Tags.Test`, keeping the `..\packages\...` relative paths (`TaskTree.Test`
  is a sibling of `TaskTree`, one level under repo root).
- **Five-analyzer stack + `MSTest.Analyzers`** and
  `<AdditionalFiles Include="$(MSBuildThisFileDirectory)..\BannedSymbols.txt" />` copied
  exactly.
- **`app.config`** copied verbatim from `Tags.Test/app.config` (binding redirects required
  so Extensions/Identity/Testing assemblies load under vstest).
- **`Properties/AssemblyInfo.cs`** mirroring `Tags.Test`, with the new ProjectGuid used for
  `[assembly: Guid(...)]`, `[assembly: ComVisible(false)]`, `AssemblyVersion 1.0.0.0`.
- **ProjectReferences:** `..\TaskTree\TaskTree.csproj`
  (`{8F7F59E6-18A7-0CF3-0E1D-4478954B612A}`), `..\UtilitiesCS\UtilitiesCS.csproj`
  (`{91b5f9bb-aa29-4dda-9e26-d3dad73ec7ca}`), `..\ToDoModel\ToDoModel.csproj`
  (`{241d7156-b046-4b65-b0ac-1cdff6d90c6b}`).
- **`TaskMaster.sln` wiring:** add the `Project(...)`/`EndProject` entry (same shape as the
  existing `Tags.Test` entry) and the four `GlobalSection(ProjectConfigurationPlatforms)`
  lines for the new GUID — both platform configs (Debug|Any CPU + Release|Any CPU,
  `ActiveCfg` + `Build.0`) — matching the exact platform token (`Any CPU` vs `AnyCPU`) used
  by the other test projects.

## Non-Goals

- **No behavior or UX change** to the forms themselves. Observable behavior is preserved.
- **No migration off WinForms/VSTO** (that is the separate No-COM architecture effort).
- **No new production dependencies.**
- **No scope beyond `TaskTree`.** Sibling epic children (#293, #297, #298) touch disjoint
  projects and are out of scope here.

### Flagged decision for the planner: dead debug helpers

The `public` debug helpers `WriteTreeToDisk`, `LoopTreeToWrite`, and `AppendLineToCSV` have
no in-repo callers (grep-verified: referenced only within the controller). The spec's
preferred disposition is to **DELETE** these dead helpers, which removes the only file-I/O
exemption candidate from the project entirely. The **fallback**, if the planner or
maintainer elects to retain them, is retention-with-seam: extract `LoopTreeToWrite` behind
the `Action<string>` line-sink so the traversal is fully testable, leaving only the thin
`WriteTreeToDisk` file-open wrapper (2–3 lines) as an exemption candidate. The planner
selects one of these two dispositions.

## Dependencies / Touchpoints

- **`UtilitiesCS.Interfaces.IWinForm.IForm`** — `ITaskTreeForm` derives from it. `IForm`
  already declares `Load`/`Activated`/`Close`/`Show`; the controller consumes none of
  those, so there is no member collision. All facade members are additive.
- **`TaskMaster.sln`** — the new `TaskTree.Test` project is added to the solution and both
  platform configurations.
- **CI (`.github/workflows/ci.yml`)** — the MSTest suite step auto-discovers test
  assemblies by recursive `*.Test.dll` under `\bin\<config>\`. `TaskTree.Test.dll` is
  auto-discovered with **no workflow edit required**.
- **`LiveOutlook` filter** — CI runs with `/TestCaseFilter:"TestCategory!=LiveOutlook"`.
  New `TaskTree.Test` tests must NOT carry `[TestCategory("LiveOutlook")]`, or they will be
  excluded from the run.
- **Coverage configs** (`coverage.config`, `TaskMaster.runsettings`) exclude only
  third-party module paths; `TaskTree.dll` is first-party and measured with no config
  change.
- **Single production caller:** `TaskMaster/Ribbon/RibbonController.cs::LoadTaskTree`
  (compiles unchanged per Invariants).
- **Epic wave:** wave 0, `depends_on: []` — no dependency on sibling children; runs in
  parallel with #293 and #297.
- **Required coordination:** none beyond maintainer ratification of any residual
  `[ExcludeFromCodeCoverage]` attribute (review approval, not an implementation-time
  external dependency).

## Risks & Mitigations

- **Legacy non-SDK csproj creation risk.** Hand-authoring a legacy `packages.config`-style
  test csproj is error-prone (Import chain, hint paths, binding redirects, transitive
  reference closure). **Mitigation:** mirror `Tags.Test` exactly — copy the Import chain,
  analyzer stack, `app.config`, and reference/`packages.config` block wholesale; change
  only assembly identity (name, GUID) and project references. Prune a reference only if it
  fails to resolve.
- **COM touchpoints.** The controller touches Outlook Interop
  (`Outlook.Application`/`Explorer`/`MailItem`/`TaskItem`). **Mitigation:** these are all
  Interop **interfaces** and are Moq-mockable without a live Outlook process, so COM paths
  are unit-testable.
- **Third-party control not mockable.** `BrightIdeasSoftware.TreeListView` is a concrete
  non-virtual control that cannot be mocked and needs a window handle. **Mitigation:** the
  intent-named `ITaskTreeForm` facade and the `ITreeVisual` adapter keep all control access
  behind mockable seams; tests never construct a live control.
- **Public contract change.** The controller constructor parameter type changes.
  **Mitigation:** the change is source-compatible for the single caller (interface
  implemented by the concrete form; new seam parameter optional), so no call-site edit is
  required.
- **Coverage-exemption creep.** **Mitigation:** `[ExcludeFromCodeCoverage]` is restricted
  to irreducible form-derived facade/adapter wiring under the ratified WinForms exemption,
  each individually justified; testable seams are never exempt.

## Technical Specifications

### Files created

- `TaskTree/ITaskTreeForm.cs` (new interface; may also hold `ITreeVisual`).
- `TaskTree/TaskTreeController.MoveLogic.cs` (partial; host-neutral move/tree logic).
- `TaskTree/TaskTreeController.Debug.cs` (partial; debug helpers — only if retained per the
  Non-Goals decision).
- `TaskTree.Test/TaskTree.Test.csproj`, `TaskTree.Test/packages.config`,
  `TaskTree.Test/app.config`, `TaskTree.Test/Properties/AssemblyInfo.cs`, and the new test
  `.cs` files.

### Files changed

- `TaskTree/TaskTreeController.cs` — retargeted to `ITaskTreeForm`; message seam and
  `ITreeVisual`-typed move signatures; reduced to ~250 lines.
- `TaskTree/TaskTreeForm.cs` — implements `ITaskTreeForm` (facade delegations) and hosts the
  `TreeListViewVisual : ITreeVisual` adapter.
- `TaskMaster.sln` — new project entry + both platform configurations.

### Public contracts affected

- New public interface `ITaskTreeForm : UtilitiesCS.Interfaces.IWinForm.IForm`.
- New interface `ITreeVisual` (two methods: `AddObject`, `RemoveObject`).
- `TaskTreeController` constructor: parameter type `TaskTreeForm` → `ITaskTreeForm`; new
  optional trailing `Action<string> showMessage = null` parameter.
- `MoveObjects*` method signatures: `TreeListView` parameters → `ITreeVisual`.
- Behavior of all affected members is unchanged.

### Data flow / validation

- No data-format or validation changes. The `Predicate<object>` filter and
  `TreeNode<ToDoItem>` roots flow unchanged through the facade; the concrete form wraps the
  predicate in `new ModelFilter(predicate)`.

### Logging / telemetry

- No logging or telemetry changes. The desync `MessageBox.Show` path is preserved via the
  message seam (default behavior identical).

### Migration / backfill

- None.

### Coverage exemption policy

- `[ExcludeFromCodeCoverage]` is applied ONLY to irreducible form-derived facade/adapter
  wiring that cannot be covered without a live control/window — specifically the concrete
  `TaskTreeForm` facade implementations and the `TreeListViewVisual : ITreeVisual` adapter,
  which fall under the ratified WinForms exemption category (form-derived + Designer code).
  Each exemption is individually justified and requires maintainer ratification.
- Testable seams — the `ITaskTreeForm` facade consumers in the controller, the `ITreeVisual`
  move logic, the message seam, and the line-sink traversal — are NEVER exempt and must
  meet the coverage floor.
- **Maintainer-ratified last-resort STA-controls refinement.** Per the epic manifest
  (`docs/features/epics/winforms-testability-refactor/epic.md`, Shared Design Pattern item 4,
  "Maintainer-ratified refinement (2026-07-09, last-resort STA controls)"), in-memory,
  never-shown WinForms controls MAY be constructed in unit tests on an STA thread strictly as a
  last resort where no seam can isolate the logic, subject to conditions: (a) seams remain the
  required first approach and each STA test documents why no seam is feasible; (b) all STA-bound
  tests live in dedicated `*.StaTests.cs` files marked `[STATestClass]`/`[STATestMethod]` (or an
  equivalent runsettings apartment scope); (c) never `Show()`/`ShowDialog()`, no message-pump
  reliance, controls disposed per test, popups prohibited; (d) `Form`-derived types remain
  prohibited even when unshown. This refinement was assessed against every exemption site in
  this feature. Resulting register state (see the plan's `[ExcludeFromCodeCoverage]` Exemption
  Register): **E1 `TaskTreeForm`** retained (Form-derived — condition (d) prohibits STA);
  **E2 `TreeListViewVisual`** retained after STA assessment — the STA mechanism is available in
  the pinned MSTest 4.2.2 (`[STATestClass]`/`[STATestMethod]`), but ObjectListView 2.9.1's
  virtual-mode `TreeListView` cannot execute `AddObject`/`RemoveObject` deterministically on an
  unshown, handle-less control without reintroducing the message-pump/live-control reliance
  condition (c) prohibits, and the adapter body is a pure two-line delegation that would test
  the third-party control rather than adapter logic; **E3 `FormatRow` wrapper** retained —
  unaffected because its obstacle is type constructibility, not the live-control prohibition.
  No exemption is removed by the refinement.
- net481 constraint: no `init`/`record`/`record struct` (no `IsExternalInit` polyfill on
  net48); use plain classes or `readonly struct`.

## Test Strategy

All tests use MSTest (`[TestClass]`/`[TestMethod]`), Moq, and FluentAssertions, in
Arrange–Act–Assert structure. No real `Form`/`Control` is constructed, no popups are shown,
no `Thread.Sleep`/`Task.Delay` is used, and no temporary files are created.

**Last-resort STA-controls refinement (assessed; not exercised in this feature).** The
maintainer-ratified refinement (epic manifest Shared Design Pattern item 4, 2026-07-09) permits
constructing in-memory, never-shown WinForms controls on an STA thread as a last resort where no
seam isolates the logic, using dedicated `*.StaTests.cs` files with `[STATestClass]`/
`[STATestMethod]` (available in the pinned MSTest 4.2.2) and subject to conditions (a)-(d) in the
Coverage exemption policy section. That refinement was assessed against this feature's exemption
sites and did NOT change the register: E1 (Form-derived, condition (d)), E2 (ObjectListView
2.9.1 virtual-mode `TreeListView` — no deterministic unshown/handle-less `AddObject`/
`RemoveObject` without message-pump/live-control reliance prohibited by condition (c)), and E3
(type-constructibility obstacle, unchanged by STA) are all retained. Consequently no
`*.StaTests.cs` file and no `[STATestClass]`/`[STATestMethod]` test is introduced by this
feature, and the "no real `Form`/`Control` constructed" property above holds for the whole test
suite. Dependencies
mocked: `ITaskTreeForm`, `ITreeVisual`, `IApplicationGlobals` (→ `IOlObjects.App` →
`Outlook.Explorer`; `Outlook.MailItem`/`TaskItem`), and the two `Action<string>` seams
(message, line-sink). Real COM-free domain objects are used directly: `TreeOfToDoItems`,
`TreeNode<ToDoItem>`, `ToDoItem`, and `IDList`.

### Per-behavior test mapping (from research Section G)

| Behavior | Approach | Positive / Negative / Edge |
|---|---|---|
| ctor wiring | mocked `ITaskTreeForm`; Verify `SetController` | Positive: SetController called. Edge: null message seam → default assigned, no throw. |
| `InitializeTreeListView` | Verify `InitializeTreeView`/`ResizeControls` called with `_dataModel.Roots` + non-null filter | Positive: facade invoked. Edge: empty roots. |
| `HandleModelCanDrop` | build `ModelDropEventArgs`; assert `Effect`/`InfoMessage`/`Handled` | Positive: reorder → Move. Negative: drop on self → None. Edge: all-roots background; drop on descendant. |
| `HandleModelDropped` dispatch | mock `ITreeVisual` src/target; verify `MoveObjects*` + post-drop `SetModelFilter`/`SortTree` | Positive: each `DropTargetLocation` routes. Edge: default → early return. |
| `MoveObjectsToRoots` | real `TreeOfToDoItems`; `ITreeVisual` mocks | Positive: same-tree promote; cross-tree `RemoveObject`/`AddObject`. Edge: already root. |
| `MoveObjectsToSibling` | real data model + real `IDList`; message seam recorded | Positive: sibling insert + renumber; root insert reseed. Negative: root not in Roots → seam fires. Edge: offset 0 vs 1. |
| `MoveObjectsToChildren` | real data model; `ITreeVisual` mock | Positive: `AddChild` + `RemoveObject`. Negative: desync → seam fires. Edge: non-root reparent. |
| `ToggleExpandCollapseAll` | Verify `ExpandAllNodes`/`CollapseAllNodes` | Positive: state transition across two calls. |
| `ToggleHideComplete` | Verify `SetModelFilter` | Positive: state transition across two calls. |
| `RebuildTreeVisual` | Verify `RebuildTree(_dataModel.Roots)` | Positive. Edge: empty roots. |
| `ResizeForm` | Verify `ResizeControls` + `AutoSizeTreeColumns` | Positive. |
| `GetSelectedTreeNode` | mock `GetSelectedNode` → node/null | Positive: node. Negative: null. |
| `IsValidType` | `Mock<MailItem>`/`Mock<TaskItem>`/other | Positive: MailItem/TaskItem → true. Negative: other → false. |
| `TreeLvActivateItem`/`Async` | mock `GetSelectedNode` + `Explorer` | Positive: selectable → clear+add; not selectable → `Display`. Negative: unsupported → seam. Edge: null → no-op. |
| `ActivateOlItem(Async)` | mock `Explorer.IsItemSelectableInView` | Positive: selectable / else `Display`. Edge: null item. Async awaited deterministically. |
| `FindChildByID` | real nested tree | Positive: found at depth. Negative: not found → null. Edge: null/empty ID. |
| `LoopTreeToWrite` (if retained) | recording `Action<string>` line-sink | Positive: expected prefixed lines. Edge: null nodes → no emission. No file I/O. |

### Invariant validation

- Move behaviors assert both data-model state (`_dataModel.Roots`, parent/child links,
  renumbered `ToDoID`s) and the visual calls (`AddObject`/`RemoveObject` via Moq Verify),
  confirming observable behavior is unchanged.

### Error handling and logging verification

- The desync branches assert the message seam fires (negative/edge) and does not fire on
  the happy path, verifying the preserved dialog behavior without showing a popup.

### Coverage impact and targets

- `TaskTree` project (`TaskTree.dll`): >= 80% line coverage.
- New classes/files: >= 90% line coverage.
- No coverage regression on changed lines.

### Toolchain commands (run in order)

1. `csharpier .` (or `dotnet tool run csharpier .`)
2. `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
3. `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true`
4. `vstest.console.exe TaskTree.Test\bin\<config>\TaskTree.Test.dll /EnableCodeCoverage`

If any step fails or auto-fixes files, restart from step 1.

### Manual validation

- None required. The refactor is code-only and fully automatable (no portal step, no live
  Outlook/WinForms process). The only human gate is maintainer ratification of any residual
  `[ExcludeFromCodeCoverage]` attribute.

## Definition of Done

- [ ] Structure matches this spec; legacy paths retired or redirected
- [ ] Invariants validated with tests or comparisons
- [ ] Imports/tooling/entry points updated
- [ ] Edge cases and error handling verified
- [ ] Tests, linting, and type checks clean
- [ ] Docs updated (initiative/README/tasks as needed)
- [ ] Toolchain pass completed (format → lint → type-check → test)
- [ ] `ITaskTreeForm` exists, derives from `IForm`, and `TaskTreeForm` implements it
- [ ] `TaskTreeController` depends on `ITaskTreeForm`, not the concrete form
- [ ] Host-neutral logic separated from COM/WinForms interaction
- [ ] No production file in `TaskTree` exceeds 500 lines
- [ ] `TaskTree.Test` project exists, follows the repo MSTest pattern, and is in the solution
- [ ] No unit test constructs a live form/window or triggers a popup
- [ ] `TaskTree` project reaches >= 80% line coverage
- [ ] Full C# toolchain (csharpier → analyzers → nullable → MSTest) passes with no regression

## Seeded Test Conditions (from potential)
- [ ] Tree/business-logic units covered with pure inputs.
- [ ] Dialog-driven or UI-bound paths covered via seams (no popups).
- [ ] Event handler logic covered via a mocked `ITaskTreeForm`.
