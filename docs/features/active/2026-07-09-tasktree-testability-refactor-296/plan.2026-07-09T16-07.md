# TaskTree Testability Refactor (#296) — Atomic Implementation Plan

- Issue: #296 (child of epic winforms-testability-refactor #295)
- Feature folder: `docs/features/active/2026-07-09-tasktree-testability-refactor-296/`
- Plan path: `docs/features/active/2026-07-09-tasktree-testability-refactor-296/plan.2026-07-09T16-07.md`
- Work Mode: full-feature (per `issue.md` metadata: `- Work Mode: full-feature`)
- Integration target (execution-time only): epic integration branch `epic/winforms-testability-refactor-integration`
- Authoritative issue number for all artifacts/paths/cross-references: **296**

All file paths in this plan are repo-root-relative. Execution occurs in a different git
worktree branched from the epic integration branch; absolute paths are intentionally not used.

## Objective

Restructure the `TaskTree` project so controller logic is unit-testable without a live UI,
split the 546-line `TaskTreeController.cs` so every production file is <= 500 lines, create a
new `TaskTree.Test` MSTest project mirroring `Tags.Test`, and bring `TaskTree.dll` to
>= 80% line coverage (new files >= 90%) while preserving all observable behavior and keeping
the single caller `TaskMaster/Ribbon/RibbonController.cs::LoadTaskTree` compiling unchanged.

## Evidence Locations (canonical, non-overridable)

Per `.claude/skills/evidence-and-timestamp-conventions/SKILL.md` (single source of truth),
all schema-bearing evidence artifacts are written under the canonical scheme:

- Baseline evidence: `docs/features/active/2026-07-09-tasktree-testability-refactor-296/evidence/baseline/`
- QA-gate evidence: `docs/features/active/2026-07-09-tasktree-testability-refactor-296/evidence/qa-gates/`
- Regression evidence: `docs/features/active/2026-07-09-tasktree-testability-refactor-296/evidence/regression-testing/`
- Issue-update mirrors: `docs/features/active/2026-07-09-tasktree-testability-refactor-296/evidence/issue-updates/`

Coverage reconciliation with the delegation prompt: the delegation named
`artifacts/csharp/` for "coverage evidence". `artifacts/csharp/` is NOT one of the FORBIDDEN
`artifacts/` evidence sub-paths (`artifacts/baselines|baseline|qa|qa-gates|evidence|coverage|regression-testing|post-change`),
so it is used ONLY as the raw review-gate consumable location for the coverage XML file
(`artifacts/csharp/coverage.xml`), which the feature-review coverage gate expects. The
schema-bearing coverage SUMMARY evidence (with `Timestamp:` / `Command:` / `EXIT_CODE:` /
`Output Summary:` and numeric coverage headline values) is written to the canonical
`evidence/baseline/` (baseline) and `evidence/qa-gates/` (post-change) folders. No canonical
evidence artifact is written to a forbidden path.

`EVIDENCE_LOCATION_OVERRIDE_REJECTED: none required` — the delegation's `artifacts/csharp/`
is used only for the raw coverage XML review consumable; all schema-bearing evidence resolves
to `<FEATURE>/evidence/<kind>/`.

## Debug-Helper Disposition Decision (planner-selected)

The `public` debug helpers `WriteTreeToDisk`, `LoopTreeToWrite`, and `AppendLineToCSV` in
`TaskTree/TaskTreeController.cs` have no in-repo callers (research grep-verified; re-verified
in Phase 0). Per the spec's Non-Goals "Flagged decision for the planner", the planner selects
the **preferred DELETE option**: these three helpers and the `ToDoTree` field (used only by
them) are removed. This removes the only file-I/O exemption candidate from the project and
eliminates the need for the CSV line-sink seam. The retention-with-line-sink fallback is NOT
used. If Phase 0 discovers an unexpected external caller, execution stops and the plan is
revised to the retention-with-line-sink fallback before proceeding.

## Scope-Lock (files created / modified)

Files CREATED:
- `TaskTree/ITaskTreeForm.cs` — new `ITaskTreeForm : UtilitiesCS.Interfaces.IWinForm.IForm`
  facade interface + narrow `ITreeVisual` interface (`AddObject`/`RemoveObject`).
- `TaskTree/TreeListViewVisual.cs` — host adapter `TreeListViewVisual : ITreeVisual` wrapping
  `BrightIdeasSoftware.TreeListView` (exemption site E2).
- `TaskTree/TaskTreeController.MoveLogic.cs` — `partial class TaskTreeController` host-neutral
  move/tree/data logic.
- `TaskTree.Test/TaskTree.Test.csproj`, `TaskTree.Test/packages.config`,
  `TaskTree.Test/app.config`, `TaskTree.Test/Properties/AssemblyInfo.cs`,
  `TaskTree.Test/TaskTreeControllerTests.cs`, `TaskTree.Test/TaskTreeControllerMoveLogicTests.cs`.

Files MODIFIED:
- `TaskTree/TaskTreeController.cs` — retargeted to `ITaskTreeForm`; message seam; facade
  calls; `ITreeVisual`-typed move dispatch; debug helpers deleted; made `partial`.
- `TaskTree/TaskTreeForm.cs` — implements `ITaskTreeForm` (facade delegations); class-level
  `[ExcludeFromCodeCoverage]` (exemption site E1).
- `TaskTree/TaskTree.csproj` — add `<Compile Include>` for `ITaskTreeForm.cs`,
  `TreeListViewVisual.cs`, `TaskTreeController.MoveLogic.cs` (legacy explicit-include project:
  new `.cs` files DO NOT compile unless wired).
- `TaskMaster.sln` — add `TaskTree.Test` `Project(...)`/`EndProject` entry + the
  `GlobalSection(ProjectConfigurationPlatforms)` block for the new GUID (mirror the `Tags.Test`
  entry at sln line 37 and its config block at sln lines 216-227).

## `[ExcludeFromCodeCoverage]` Exemption Register (maintainer ratification required)

Exemptions are limited to irreducible form-derived / host-adapter wiring under the ratified
COM/VSTO/WinForms coverage exemption (CLAUDE.md General Unit Test Policy §UT2, category (b)
WinForms form-derived + Designer code). Each site is individually justified. Testable seams are
NEVER exempt.

**Maintainer-ratified STA-refinement application (epic manifest authority).** Per
`docs/features/epics/winforms-testability-refactor/epic.md` Shared Design Pattern item 4,
"Maintainer-ratified refinement (2026-07-09, last-resort STA controls)", each exemption site
below was re-assessed against the STA option (in-memory, never-shown WinForms controls MAY be
constructed on an STA thread strictly as a last resort where no seam isolates the logic, subject
to conditions (a) seams first + documented infeasibility, (b) dedicated `*.StaTests.cs`
`[STATestClass]`/`[STATestMethod]` files, (c) no `Show()`/`ShowDialog()`, no message-pump
reliance, controls disposed per test, no popups, and (d) `Form`-derived types remain prohibited
even unshown). Assessment outcomes for this feature:

- **E1** — remains exempt; STA is not attempted. `TaskTreeForm` is `Form`-derived, which
  refinement condition (d) prohibits in tests even when unshown.
- **E2** — assessed for STA coverage of `AddObject`/`RemoveObject` and RETAINED as exempt. The
  STA test mechanism itself IS available (MSTest 4.2.2 provides `[STATestClass]`/
  `[STATestMethod]`, introduced in MSTest 3.6), so the blocker is not tooling; it is the
  ObjectListView 2.9.1 control contract documented in the E2 row below (a virtual-mode
  `TreeListView` cannot execute these members deterministically on an unshown, handle-less
  control without reintroducing the live-control/message-pump reliance condition (c) prohibits).
- **E3** — unaffected. Its obstacle is type constructibility
  (`FormatRowEventArgs.Model` get-only, `FormatRowEventArgs.Item` internal setter), not the
  live-control prohibition; constructing a control on STA does not change constructibility.

No exemption is removed by this refinement; the resulting register state is E1/E2/E3 all
retained. The MSTest STA mechanics available for any future STA test in this project are:
`[STATestClass]`/`[STATestMethod]` (per-class/per-method apartment scoping, preferred, available
in the pinned MSTest 4.2.2); the runsettings alternative is
`<RunConfiguration><ExecutionThreadApartmentState>STA</ExecutionThreadApartmentState></RunConfiguration>`,
whose tradeoff is that it forces STA on the entire `TaskTree.Test` run rather than the single
class that needs it, unnecessarily broadening the apartment scope.

| # | Site (file + type) | Attribute placement | Justification |
|---|---|---|---|
| E1 | `TaskTree/TaskTreeForm.cs` — `partial class TaskTreeForm : Form, ITaskTreeForm` | class-level `[ExcludeFromCodeCoverage]` on the `TaskTreeForm.cs` partial declaration | Form-derived WinForms class (category b). All facade members are thin delegations to `TreeLv`/`ControlResizer` that require a live `TreeListView`/window handle; the private event handlers forward to the controller. The class-level attribute on this partial declaration also covers the `TaskTree/TaskTreeForm.Designer.cs` designer-generated partial (category b). |
| E2 | `TaskTree/TreeListViewVisual.cs` — `class TreeListViewVisual : ITreeVisual` | class-level `[ExcludeFromCodeCoverage]` | Host adapter over `BrightIdeasSoftware.TreeListView` (ObjectListView 2.9.1.1072); `AddObject`/`RemoveObject` are pure two-line delegations to the wrapped control with no branching. **STA-refinement assessment (epic manifest Shared Design Pattern item 4, maintainer-ratified 2026-07-09) — RETAINED on API grounds, not tooling grounds:** The STA mechanism exists (MSTest 4.2.2 provides `[STATestClass]`/`[STATestMethod]`), so tooling is not the blocker. The obstacle is the ObjectListView 2.9.1 control contract: `TreeListView` is a Win32 virtual-mode list view (`VirtualMode = true`) backed by a `Tree`/`IVirtualListDataSource`, and its members are non-virtual (unmockable). `AddObject`/`RemoveObject` route through `AddObjects`/`RemoveObjects` → tree-model mutation + `UpdateVirtualListSize()` + redraw, which synchronize with the native `SysListView32` and are only well-defined once the native handle exists. On an unshown, handle-less control the 2.9.1 public contract does not guarantee that `AddObject`/`RemoveObject` deterministically reflect in `Roots`/`Objects` enumeration. Forcing handle creation (`.Handle`/`CreateControl()`) to make the mutation observable instantiates a real native window and drives message-based virtual-list synchronization — reintroducing exactly the live-control/message-pump reliance refinement condition (c) prohibits. The refinement's enumerated last-resort controls (`TableLayoutPanel`, `Label`, `Panel`, `CheckBox`) expose managed state readable without a handle; a virtual `TreeListView` does not share that property for list mutation. Condition (a) is satisfied trivially because the adapter already IS the thinnest possible seam boundary (its body is exactly the concrete-control delegation), so an STA test would exercise ObjectListView's own behavior, not adapter logic, and its marginal coverage (two delegating lines) does not justify handle-dependent nondeterminism. Retained under the ratified WinForms exemption (category b/c analog). |
| E3 | `TaskTree/TaskTreeController.cs` — residual `internal void FormatRow(object, FormatRowEventArgs)` event-handler wrapper (post-extraction) | method-level `[ExcludeFromCodeCoverage]` on the `FormatRow` wrapper only | Narrowly scoped. The strikeout DECISION is extracted (P6-T1 production change) into the host-neutral `internal static FontStyle ResolveRowStyle(FontStyle, bool)`, which is directly unit-tested on both `Complete` branches and is NOT exempt. The residual `FormatRow` wrapper contains only event-arg marshalling: it reads `e.Model` (get-only) and assigns `e.Item.Font`, and `FormatRowEventArgs`/`OLVListItem` are not constructible from `TaskTree.Test` in ObjectListView 2.9.1 (`.Model` get-only, `.Item` internal setter) and require a live `TreeListView` row item (category b/c analog). Exemption covers only those few unavoidable wrapper lines; the >= 90% file target is achievable with the wrapper exempted and `ResolveRowStyle` covered. STA-refinement assessment (epic manifest Shared Design Pattern item 4): the STA refinement was assessed and does not alter this obstacle — the blocker is type constructibility (`FormatRowEventArgs.Model` get-only, `FormatRowEventArgs.Item` internal setter), not the live-control prohibition, and constructing a `TreeListView`/row item on STA does not make `FormatRowEventArgs`/`OLVListItem` constructible from `TaskTree.Test`; E3 is unchanged. |

NOT exempt (must meet coverage floor): `TaskTree/TaskTreeController.cs` (except the E3 residual
`FormatRow` event-handler wrapper method-level exemption above) — this explicitly INCLUDES the
extracted `internal static FontStyle ResolveRowStyle(FontStyle, bool)` helper, whose both `Complete`
branches are covered by the P6-T1 `ResolveRowStyle` test; `TaskTree/TaskTreeController.MoveLogic.cs`,
the `ITaskTreeForm`/`ITreeVisual` consumers in the controller, and the `_showMessage` message seam.

## Banned-API Register

`BannedSymbols.txt` bans `DateTime.Now`, `DateTime.UtcNow`, `Random.Shared`, `Thread.Sleep`,
`Task.Delay`. Phase 0 scans every production file this plan touches. The current controller uses
`Task.Run` (NOT banned) and no banned symbol. Expected finding: zero banned symbols in touched
production files. Any banned symbol found in a touched file is remediated within the task that
touches that file. Test code must not use `Thread.Sleep`/`Task.Delay`/wall-clock waits; async
tests await deterministically against synchronously-returning mocks.

---

### Phase 0 — Baseline Capture and Precondition Verification

- [ ] [P0-T1] Read policy files in the required order and record them in
  `docs/features/active/2026-07-09-tasktree-testability-refactor-296/evidence/baseline/phase0-instructions-read.md`
  with `Timestamp:`, `Policy Order:`, and the explicit list of files read: `CLAUDE.md`,
  `.claude/rules/general-code-change.md`, `.claude/rules/general-unit-test.md`,
  `.claude/rules/csharp.md`, `.claude/skills/atomic-plan-contract/SKILL.md`,
  `.claude/skills/evidence-and-timestamp-conventions/SKILL.md`. Binary outcome: artifact exists
  with all three required fields populated.
- [ ] [P0-T2] Run `csharpier .` in check mode and capture baseline formatting state to
  `docs/features/active/2026-07-09-tasktree-testability-refactor-296/evidence/baseline/baseline-format.md`
  with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`. Binary outcome: artifact exists
  with all four fields.
- [ ] [P0-T3] Run `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
  and capture baseline analyzer state to
  `docs/features/active/2026-07-09-tasktree-testability-refactor-296/evidence/baseline/baseline-analyzers.md`
  with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`. Binary outcome: artifact exists
  with all four fields.
- [ ] [P0-T4] Run `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true`
  and capture baseline nullable/type-check state to
  `docs/features/active/2026-07-09-tasktree-testability-refactor-296/evidence/baseline/baseline-nullable.md`
  with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`. Binary outcome: artifact exists
  with all four fields.
- [ ] [P0-T5] Run the existing MSTest suite with coverage
  (`vstest.console.exe <existing *.Test.dll set> /EnableCodeCoverage`), copy the raw coverage
  XML to `artifacts/csharp/coverage.xml`, and record the baseline `TaskTree.dll` line-coverage
  headline (expected 0% — no test project exists yet) to
  `docs/features/active/2026-07-09-tasktree-testability-refactor-296/evidence/baseline/baseline-coverage.md`
  with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` (must state the numeric
  `TaskTree.dll` baseline line-% value). Binary outcome: artifact records the numeric baseline.
- [ ] [P0-T6] Verify structural preconditions and record them in
  `docs/features/active/2026-07-09-tasktree-testability-refactor-296/evidence/baseline/baseline-preconditions.md`:
  `TaskTree/TaskTreeController.cs` is 546 lines; `TaskTree/TaskTreeForm.cs` is 108 lines;
  `TaskTree/TaskTreeForm.Designer.cs` is 311 lines; `UtilitiesCS/Interfaces/IWinForm/IForm.cs`
  declares interface `IForm : IContainerControl, IScrollableControl`; `TaskTree/TaskTree.csproj`
  is a legacy non-SDK `packages.config` project with explicit `<Compile Include>` items (no
  glob); `Tags.Test/Tags.Test.csproj` + `Tags.Test/packages.config` + `Tags.Test/app.config` +
  `Tags.Test/Properties/AssemblyInfo.cs` all exist (mirror source); NO `TaskTree.Test/` folder
  exists; `TaskMaster.sln` contains the `Tags.Test` entry (line 37) and its config block
  (lines 216-227). Binary outcome: all listed facts confirmed and recorded.
- [ ] [P0-T7] Record the current caller baseline for `TaskMaster/Ribbon/RibbonController.cs::LoadTaskTree`
  (the exact 3-argument construction `new TaskTreeController(Globals, taskTreeViewer, dataModel)`
  and `taskTreeViewer.Show()` at lines ~88-95) verbatim to
  `docs/features/active/2026-07-09-tasktree-testability-refactor-296/evidence/baseline/baseline-caller.md`
  so the post-refactor no-edit invariant can be diffed. Binary outcome: verbatim call site recorded.
- [ ] [P0-T8] Grep-verify no external caller invokes the `TaskTreeController` debug helpers
  `WriteTreeToDisk`, `LoopTreeToWrite`, or `AppendLineToCSV`, or accesses the controller `ToDoTree`
  field. Scope the check to invocations that resolve to a `TaskTreeController` instance: search the
  repo for `.WriteTreeToDisk(`, `.LoopTreeToWrite(`, `.AppendLineToCSV(`, and `.ToDoTree`
  member-access against a `TaskTreeController` reference, and confirm the only declarations and
  self-calls are inside `TaskTree/TaskTreeController.cs`. Do NOT use a bare-name grep: it would
  false-positive on `ToDoModel/Data Model/Tree/TreeOfToDoItems.cs`, which declares and internally
  calls its OWN independent, same-named `LoopTreeToWrite` (line 452) and `AppendLineToCSV`
  (line 471) members — invoked from `WriteTreeToCSVDebug`/`LoopTreeToWrite` at lines 449, 462, 466.
  Explicitly annotate those `TreeOfToDoItems.cs` members in the evidence as unrelated to the
  controller and EXPECTED non-callers that MUST NOT trip the zero-callers gate. Also scan every
  production file this plan touches (`TaskTree/TaskTreeController.cs`, `TaskTree/TaskTreeForm.cs`)
  for banned symbols (`DateTime.Now`, `DateTime.UtcNow`, `Random.Shared`, `Thread.Sleep`,
  `Task.Delay`). Record results and the confirmed DELETE disposition in
  `docs/features/active/2026-07-09-tasktree-testability-refactor-296/evidence/baseline/baseline-deadcode-and-bannedapi.md`.
  Binary outcome: zero external `TaskTreeController` debug-helper callers (the `TreeOfToDoItems.cs`
  same-named members are not controller callers and do not count) AND zero banned symbols recorded;
  if a genuine controller caller or a banned symbol is found, stop and revise the plan
  (retention-with-line-sink fallback / banned-API remediation).

### Phase 1 — Interface and Adapter Contracts

- [ ] [P1-T1] Create `TaskTree/ITaskTreeForm.cs` declaring
  `public interface ITaskTreeForm : UtilitiesCS.Interfaces.IWinForm.IForm` with the intent-named
  facade members `void SetController(TaskTreeController controller)`,
  `void InitializeTreeView(IEnumerable<TreeNode<ToDoItem>> roots, Predicate<object> incompleteFilter)`,
  `void SetModelFilter(Predicate<object> filter)`, `void SortTree()`, `void ExpandAllNodes()`,
  `void CollapseAllNodes()`, `void RebuildTree(IEnumerable<TreeNode<ToDoItem>> roots)`,
  `void AutoSizeTreeColumns()`, `TreeNode<ToDoItem> GetSelectedNode()`, `void ResizeControls()`;
  plus `public interface ITreeVisual { void AddObject(object model); void RemoveObject(object model); }`.
  No `TreeListView`/`OLVColumn`/`SplitContainer`/`ModelFilter` type is exposed. Add a matching
  `<Compile Include="ITaskTreeForm.cs" />` to `TaskTree/TaskTree.csproj`. Verification: run the
  full C# toolchain in order (`csharpier .` -> analyzers build -> nullable build ->
  `vstest.console.exe <existing test dlls> /EnableCodeCoverage`); all steps green. Binary
  outcome: file exists, wired into csproj, solution builds green.
- [ ] [P1-T2] Create `TaskTree/TreeListViewVisual.cs` declaring
  `[ExcludeFromCodeCoverage] class TreeListViewVisual : ITreeVisual` (exemption E2) that wraps a
  `BrightIdeasSoftware.TreeListView` and delegates `AddObject(object)`/`RemoveObject(object)` to
  the wrapped control, exposing the wrapped control (or supporting reference comparison) so
  same-tree vs cross-tree checks are preserved. Add a matching
  `<Compile Include="TreeListViewVisual.cs" />` to `TaskTree/TaskTree.csproj`. Verification: run
  the full C# toolchain in order; all steps green. Binary outcome: file exists, wired into
  csproj, solution builds green.

### Phase 2 — TaskTreeForm Facade Implementation

- [ ] [P2-T1] Modify `TaskTree/TaskTreeForm.cs` so `TaskTreeForm` implements `ITaskTreeForm`
  (`public partial class TaskTreeForm : Form, ITaskTreeForm`), adding thin facade delegations:
  `InitializeTreeView` (sets `TreeLv.CanExpandGetter/ChildrenGetter/ParentGetter`, wraps the
  incomplete predicate in `new ModelFilter(predicate)`, sets `TreeLv.Roots`, initial
  `TreeLv.Sort(OlvToDoID, SortOrder.Ascending)`, and the `SimpleDropSink` flag configuration);
  `SetModelFilter` (null clears; non-null wraps in `new ModelFilter`); `SortTree` (`TreeLv.Sort()`);
  `ExpandAllNodes`/`CollapseAllNodes`; `RebuildTree` (set roots + `RebuildAll(false)`);
  `AutoSizeTreeColumns` (`TreeLv.AutoScaleColumnsToContainer()`); `GetSelectedNode` (wraps
  `TreeLv.GetItem(TreeLv.SelectedIndex).RowObject as TreeNode<ToDoItem>` including the existing
  try/catch-returns-null); and `ResizeControls` (owns the `ControlResizer` `FindAllControls` +
  `SetResizeDimensions(SplitContainer1 ...)` + `ResizeAllControls(this)` wiring moved out of the
  controller). Apply class-level `[ExcludeFromCodeCoverage]` (exemption E1) on the
  `TaskTreeForm.cs` partial declaration. `SetController(TaskTreeController)` remains. The
  controller is NOT yet retargeted, so the solution still builds with the concrete form.
  Verification: run the full C# toolchain in order; all steps green. Binary outcome:
  `TaskTreeForm` implements every `ITaskTreeForm` member and the solution builds green.

### Phase 3 — Controller Refactor and Partial Split

- [ ] [P3-T1] Refactor `TaskTree/TaskTreeController.cs` to depend only on `ITaskTreeForm`/`ITreeVisual`:
  change the `_viewer` field and the `Viewer` constructor parameter type from `TaskTreeForm` to
  `ITaskTreeForm`; add a trailing optional constructor parameter `Action<string> showMessage = null`
  with `private readonly Action<string> _showMessage = showMessage ?? (m => MessageBox.Show(m));`;
  replace EVERY direct control access (`_viewer.TreeLv.*`, `_viewer.OlvToDoID`,
  `_viewer.SplitContainer1`, and the controller-owned `_rs`/`_rscol` `ControlResizer` wiring) with
  the intent-named `ITaskTreeForm` facade calls (`InitializeTreeView`, `SetModelFilter`,
  `SortTree`, `ExpandAllNodes`/`CollapseAllNodes`, `RebuildTree`, `AutoSizeTreeColumns`,
  `GetSelectedNode`, `ResizeControls`); replace ALL FOUR `MessageBox.Show(...)` call sites with
  `_showMessage(...)` — the two desync sites in the move methods (`MoveObjectsToSibling` ~line 306
  and `MoveObjectsToChildren` ~line 366) AND the two "unsupported type" sites in `TreeLvActivateItem`
  (~line 434) and `TreeLvActivateItemAsync` (~line 451) — so no raw `MessageBox.Show` popup can fire
  from the controller and the activation "unsupported type -> message seam" assertions in P6-T1 are
  satisfiable without triggering a live popup; retarget the `MoveObjectsToRoots/Sibling/Children`
  parameters from `TreeListView` to `ITreeVisual`; and in `HandleModelDropped` wrap the
  drop-event controls in `TreeListViewVisual`, using a single adapter instance when
  `e.ListView`/`e.SourceListView` are reference-equal so the same-tree `ReferenceEquals` check is
  preserved. Verification: run the full C# toolchain in order; all steps green AND (a) a grep for
  `TreeLv` / `OlvToDoID` / `SplitContainer1` in `TaskTree/TaskTreeController*.cs` each returns zero
  matches; (b) a grep for `MessageBox.Show` in `TaskTree/TaskTreeController*.cs` returns EXACTLY ONE
  match, and that single match is the `_showMessage` default lambda in the constructor field
  initializer (`private readonly Action<string> _showMessage = showMessage ?? (m => MessageBox.Show(m));`);
  (c) none of the four former call sites (`MoveObjectsToSibling`, `MoveObjectsToChildren`,
  `TreeLvActivateItem`, `TreeLvActivateItemAsync`) retains a direct `MessageBox.Show` (each now calls
  `_showMessage(...)`); AND the constructor parameter type is `ITaskTreeForm`. Binary outcome:
  controller builds green against the interface seams with no direct control references, and the only
  `MessageBox.Show` token in the controller is the `_showMessage` seam default.
- [ ] [P3-T2] Delete the dead debug helpers `WriteTreeToDisk`, `LoopTreeToWrite`, and
  `AppendLineToCSV` and the `ToDoTree` field from `TaskTree/TaskTreeController.cs`, and remove any
  now-unused `using` directives (e.g. `System.IO`) surfaced by analyzers. Verification: run the
  full C# toolchain in order; all steps green AND grep for
  `WriteTreeToDisk|LoopTreeToWrite|AppendLineToCSV|ToDoTree` in `TaskTree/` returns zero matches.
  Binary outcome: helpers removed and solution builds green.
- [ ] [P3-T3] Move `HandleModelCanDrop`, `HandleModelDropped`, `MoveObjectsToRoots`,
  `MoveObjectsToSibling`, `MoveObjectsToChildren`, `FindChildByID`, and `IsValidType` into a new
  `TaskTree/TaskTreeController.MoveLogic.cs` declaring `public partial class TaskTreeController`,
  and change `TaskTree/TaskTreeController.cs` to `public partial class TaskTreeController`. Add a
  matching `<Compile Include="TaskTreeController.MoveLogic.cs" />` to `TaskTree/TaskTree.csproj`.
  Verification: run the full C# toolchain in order; all steps green AND both
  `TaskTree/TaskTreeController.cs` and `TaskTree/TaskTreeController.MoveLogic.cs` are each
  <= 500 lines. Binary outcome: partial split complete, wired into csproj, both files <= 500
  lines, solution builds green.

### Phase 4 — Caller and Wiring Verification

- [ ] [P4-T1] Verify `TaskMaster/Ribbon/RibbonController.cs::LoadTaskTree` compiles with NO
  call-site edit: `git diff` on `TaskMaster/Ribbon/RibbonController.cs` shows zero changes, and
  the solution builds green (the 3-argument `new TaskTreeController(Globals, taskTreeViewer, dataModel)`
  binds because `TaskTreeForm` implements `ITaskTreeForm` and the `showMessage` parameter is
  optional). Record the diff-empty result to
  `docs/features/active/2026-07-09-tasktree-testability-refactor-296/evidence/qa-gates/caller-unchanged.md`
  with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`. Binary outcome: caller file
  unchanged and solution builds green.
- [ ] [P4-T2] Verify `TaskTree/TaskTree.csproj` contains `<Compile Include>` items for
  `ITaskTreeForm.cs`, `TreeListViewVisual.cs`, and `TaskTreeController.MoveLogic.cs`, and that all
  production files in `TaskTree/` (`TaskTreeController.cs`, `TaskTreeController.MoveLogic.cs`,
  `ITaskTreeForm.cs`, `TreeListViewVisual.cs`, `TaskTreeForm.cs`, `TaskTreeForm.Designer.cs`) are
  each <= 500 lines. Record to
  `docs/features/active/2026-07-09-tasktree-testability-refactor-296/evidence/qa-gates/wiring-and-filesize.md`.
  Binary outcome: all new files wired AND every production file <= 500 lines.

### Phase 5 — Create TaskTree.Test Project

- [ ] [P5-T1] Create `TaskTree.Test/TaskTree.Test.csproj` by mirroring
  `Tags.Test/Tags.Test.csproj` exactly, changing ONLY: a newly generated unique
  `<ProjectGuid>` (candidate `{7C4E2B1A-3F9D-4A6E-8B2C-1D5E9F0A7C36}` — the executor MUST grep
  `TaskMaster.sln` and every `*.csproj` to confirm no collision and regenerate if it collides);
  `<RootNamespace>`/`<AssemblyName>` to `TaskTree.Test`; the `<ProjectReference>` set to
  `..\TaskTree\TaskTree.csproj` (`{8F7F59E6-18A7-0CF3-0E1D-4478954B612A}`),
  `..\UtilitiesCS\UtilitiesCS.csproj` (`{91b5f9bb-aa29-4dda-9e26-d3dad73ec7ca}`),
  `..\ToDoModel\ToDoModel.csproj` (`{241d7156-b046-4b65-b0ac-1cdff6d90c6b}`) in place of the
  `Tags` reference; and adding a `<Reference Include="ObjectListView, ...">` with
  `<HintPath>..\packages\ObjectListView.Official.2.9.1\lib\net20\ObjectListView.dll</HintPath>`
  (needed for `ModelDropEventArgs`/`TreeListView`/`OLVColumn` types). Preserve verbatim: the
  `TargetFrameworkVersion v4.8.1`, `OutputType Library`, `ProjectTypeGuids`
  `{3AC096D0-A1C2-E12C-1390-A8335801FDAB};{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}`,
  `TestProjectType UnitTest`, the top-of-file Testing.Platform `<Import ... .props>` lines, the
  bottom `<Import ... .targets>` lines, the `EnsureNuGetPackageBuildImports` target, the
  `Microsoft.Office.Interop.Outlook 15.0.0.0` (`EmbedInteropTypes False`) reference, the full
  transitive Microsoft.Extensions / OpenTelemetry / Identity / BCL reference closure, the
  five-analyzer stack + `MSTest.Analyzers`, and
  `<AdditionalFiles Include="$(MSBuildThisFileDirectory)..\BannedSymbols.txt" />`. `<Compile Include>`
  initially lists `Properties\AssemblyInfo.cs` only. Binary outcome: csproj exists mirroring
  `Tags.Test` with the four adjustments and a verified-unique GUID.
- [ ] [P5-T2] Create `TaskTree.Test/packages.config` verbatim from `Tags.Test/packages.config`
  (identical package/version/targetFramework `net481` set, including the `developmentDependency`
  analyzer entries). Binary outcome: file exists byte-equivalent to the `Tags.Test` package set.
- [ ] [P5-T3] Create `TaskTree.Test/app.config` verbatim from `Tags.Test/app.config` (binding
  redirects required for Extensions/Identity/Testing assemblies under vstest). Binary outcome:
  file exists identical to `Tags.Test/app.config`.
- [ ] [P5-T4] Create `TaskTree.Test/Properties/AssemblyInfo.cs` mirroring
  `Tags.Test/Properties/AssemblyInfo.cs` with titles/product `TaskTree.Test`,
  `[assembly: ComVisible(false)]`, `[assembly: Guid("<new-ProjectGuid-lowercased>")]`, and
  `AssemblyVersion 1.0.0.0`. Binary outcome: file exists with the new GUID matching P5-T1.
- [ ] [P5-T5] Add `TaskTree.Test` to `TaskMaster.sln`: insert a
  `Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "TaskTree.Test", "TaskTree.Test\TaskTree.Test.csproj", "{<NEW-GUID>}"`
  / `EndProject` entry (same shape as the `Tags.Test` entry at line 37) and a full
  `GlobalSection(ProjectConfigurationPlatforms)` block for the new GUID mirroring the `Tags.Test`
  GUID block at lines 216-227 (Debug|Any CPU, Debug|x64, Debug|x86, Release|Any CPU, Release|x64,
  Release|x86, each `ActiveCfg` + `Build.0`, using the exact `Any CPU` token). Binary outcome:
  sln contains the new project entry and its complete config block.
- [ ] [P5-T6] Restore and build the empty `TaskTree.Test` project and confirm discovery: run the
  full C# toolchain in order (`csharpier .` -> analyzers build -> nullable build ->
  `vstest.console.exe TaskTree.Test\bin\Debug\TaskTree.Test.dll /EnableCodeCoverage`); the
  assembly builds into `TaskTree.Test\bin\Debug\` and is enumerated by vstest. Record to
  `docs/features/active/2026-07-09-tasktree-testability-refactor-296/evidence/qa-gates/tasktree-test-scaffold-build.md`
  with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`. Binary outcome: project builds
  green and the test assembly is discoverable.

### Phase 6 — Unit Tests to Coverage Floor

- [ ] [P6-T1] Create `TaskTree.Test/TaskTreeControllerTests.cs` (MSTest `[TestClass]`/`[TestMethod]`,
  Moq, FluentAssertions, AAA) covering: ctor wiring (Verify `SetController`; null message seam ->
  default assigned, no throw), `InitializeTreeListView` (Verify `InitializeTreeView` with
  `_dataModel.Roots` + non-null predicate and `ResizeControls`), `ToggleExpandCollapseAll`
  (state transition across two calls), `ToggleHideComplete` (Verify `SetModelFilter` transition),
  `RebuildTreeVisual` (Verify `RebuildTree`), `ResizeForm` (Verify `ResizeControls` +
  `AutoSizeTreeColumns`), `GetSelectedTreeNode` (node / null via mocked `GetSelectedNode`),
  the strikeout decision via the extracted host-neutral helper `ResolveRowStyle` (see the production
  change clause below): cover BOTH `Complete` branches of
  `internal static FontStyle ResolveRowStyle(FontStyle baseStyle, bool complete)` by asserting
  `ResolveRowStyle(FontStyle.Regular, true)` includes `FontStyle.Strikeout` and
  `ResolveRowStyle(FontStyle.Strikeout, false)` excludes it, with NO
  `FormatRowEventArgs`/`OLVListItem`/`OLVListItem.Font` construction — those types are not
  constructible from `TaskTree.Test` in ObjectListView 2.9.1 (`FormatRowEventArgs.Model` is get-only
  and `FormatRowEventArgs.Item` has an internal setter), and the residual `FormatRow` event-handler
  wrapper is covered by exemption E3 below,
  `IsValidType` (`Mock<MailItem>`/`Mock<TaskItem>` -> true, other -> false), and the Outlook
  activation paths (`ActivateOlItem`/`Async`, `TreeLvActivateItem`/`Async` against mocked
  `IApplicationGlobals.Ol.App.ActiveExplorer()` -> `Mock<Explorer>`; assert
  `ClearSelection`/`AddToSelection` vs `Display`; unsupported type -> message seam; null -> no-op;
  async awaited deterministically against synchronous mocks). No real `Form`/`Control`, no popups,
  no `Thread.Sleep`/`Task.Delay`, no temp files, no `[TestCategory("LiveOutlook")]`. Production change
  (same file, NO new file, NO csproj change): in `TaskTree/TaskTreeController.cs`, extract the
  strikeout decision from `FormatRow` into
  `internal static FontStyle ResolveRowStyle(FontStyle baseStyle, bool complete) => complete ? (baseStyle | FontStyle.Strikeout) : (baseStyle & ~FontStyle.Strikeout);`
  and rewrite the `FormatRow` body to call it
  (`e.Item.Font = new Font(e.Item.Font, ResolveRowStyle(e.Item.Font.Style, todo.Complete));`); the
  edit stays within the already-wired `TaskTreeController.cs`, adds no file, and keeps that file
  <= 500 lines. Add a matching
  `<Compile Include="TaskTreeControllerTests.cs" />` to `TaskTree.Test/TaskTree.Test.csproj`. Keep
  the file <= 500 lines. Verification: run the full C# toolchain in order; all steps green and the
  new tests pass. Binary outcome: `ResolveRowStyle` extracted in `TaskTreeController.cs`, test file
  exists, wired into csproj, all its tests pass.
- [ ] [P6-T2] Create `TaskTree.Test/TaskTreeControllerMoveLogicTests.cs` (MSTest, Moq,
  FluentAssertions, AAA) covering: `HandleModelCanDrop` (reorder -> Move; drop-on-self -> None;
  all-roots background; drop-on-descendant paradox message), `HandleModelDropped` dispatch (each
  `DropTargetLocation` routes + post-drop `SetModelFilter`/`SortTree`; `default` early return),
  `MoveObjectsToRoots` (same-tree promote; cross-tree `RemoveObject`/`AddObject`; already-root),
  `MoveObjectsToSibling` (sibling insert + `ReNumberChildrenIDs`; root insert reseed via real
  `IDList`/`GetNextToDoID`; root-not-in-Roots -> message seam; offset 0 vs 1),
  `MoveObjectsToChildren` (`AddChild` + `RemoveObject`; desync -> message seam; non-root reparent),
  and `FindChildByID` (found at depth; not found -> null; null/empty ID). Use real
  `TreeOfToDoItems`/`TreeNode<ToDoItem>`/`ToDoItem`/`IDList`, `Mock<ITreeVisual>` source/target
  (identical instance for same-tree, distinct for cross-tree), and a recording `Action<string>`
  message seam asserting fire on desync and no-fire on happy path. No real controls, no popups, no
  banned APIs, no temp files, no `[TestCategory("LiveOutlook")]`. Add a matching
  `<Compile Include="TaskTreeControllerMoveLogicTests.cs" />` to `TaskTree.Test/TaskTree.Test.csproj`.
  Keep the file <= 500 lines. Verification: run the full C# toolchain in order; all steps green and
  the new tests pass. Binary outcome: file exists, wired into csproj, all its tests pass.
- [ ] [P6-T3] Run `vstest.console.exe TaskTree.Test\bin\Debug\TaskTree.Test.dll /EnableCodeCoverage`,
  copy the raw coverage XML to `artifacts/csharp/coverage.xml`, and confirm `TaskTree.dll` line
  coverage is >= 80% with the new files (`TaskTreeController.cs`, `TaskTreeController.MoveLogic.cs`)
  >= 90%. If below threshold, add targeted `[TestMethod]`s to the existing test files (no csproj
  wiring change beyond already-listed files) and re-run until met. Record numeric per-file and
  `TaskTree.dll` totals to
  `docs/features/active/2026-07-09-tasktree-testability-refactor-296/evidence/qa-gates/coverage-interim.md`
  with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` (numeric headline values).
  Binary outcome: `TaskTree.dll` >= 80% line and new files >= 90% line, recorded numerically.

### Phase 7 — Final QA Loop and Coverage Verification

- [ ] [P7-T1] Run `csharpier .` and record the result to
  `docs/features/active/2026-07-09-tasktree-testability-refactor-296/evidence/qa-gates/final-format.md`
  with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`. If files change, restart the QA
  loop from this task. Binary outcome: formatting clean with no changes.
- [ ] [P7-T2] Run `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
  (lint/analyzers) and record to
  `docs/features/active/2026-07-09-tasktree-testability-refactor-296/evidence/qa-gates/final-analyzers.md`
  with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`. If a prior QA step changed
  files, restart from P7-T1. Binary outcome: zero analyzer errors.
- [ ] [P7-T3] Run `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true`
  (type-check/nullable) and record to
  `docs/features/active/2026-07-09-tasktree-testability-refactor-296/evidence/qa-gates/final-nullable.md`
  with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`. Binary outcome: build green with
  no nullable warnings-as-errors.
- [ ] [P7-T4] Run `vstest.console.exe TaskTree.Test\bin\Debug\TaskTree.Test.dll /EnableCodeCoverage`
  (test), copy the raw coverage XML to `artifacts/csharp/coverage.xml`, and record the post-change
  coverage to
  `docs/features/active/2026-07-09-tasktree-testability-refactor-296/evidence/qa-gates/final-coverage.md`
  with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` including numeric `TaskTree.dll`
  line-% and per-new-file line-% values. Binary outcome: all tests pass and coverage numbers
  recorded.
- [ ] [P7-T5] Verify the coverage thresholds and no-regression: baseline `TaskTree.dll`
  (from P0-T5) vs post-change (from P7-T4) shows `TaskTree.dll` >= 80% line, new files
  (`TaskTreeController.cs`, `TaskTreeController.MoveLogic.cs`) >= 90% line, and no regression on
  changed lines. Record the delta to
  `docs/features/active/2026-07-09-tasktree-testability-refactor-296/evidence/qa-gates/coverage-delta.md`
  with baseline %, post-change %, and new-file %. Binary outcome: thresholds met and delta
  recorded; if unmet, outcome is remediation-required (not PASS).
- [ ] [P7-T6] Verify every production file in `TaskTree/` is <= 500 lines
  (`TaskTreeController.cs`, `TaskTreeController.MoveLogic.cs`, `ITaskTreeForm.cs`,
  `TreeListViewVisual.cs`, `TaskTreeForm.cs`, `TaskTreeForm.Designer.cs`) and record the line
  counts to
  `docs/features/active/2026-07-09-tasktree-testability-refactor-296/evidence/qa-gates/final-filesize.md`.
  Binary outcome: all production files <= 500 lines.
- [ ] [P7-T7] Verify `TaskMaster/Ribbon/RibbonController.cs` remains unchanged (`git diff` empty)
  after the full refactor and record to
  `docs/features/active/2026-07-09-tasktree-testability-refactor-296/evidence/qa-gates/final-caller-unchanged.md`
  with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`. Binary outcome: caller file
  unchanged.
- [ ] [P7-T8] Reconcile the `issue.md` Acceptance Criteria and `spec.md` Definition of Done
  against collected evidence, mirror the status to
  `docs/features/active/2026-07-09-tasktree-testability-refactor-296/evidence/issue-updates/issue-296.<timestamp>.md`,
  and confirm the `[ExcludeFromCodeCoverage]` Exemption Register (E1, E2, E3) is flagged for
  maintainer ratification. Binary outcome: every AC/DoD item mapped to an evidence artifact and
  the exemption register surfaced for ratification.

---

## Preflight Self-Validation

Structural self-check performed (the `mcp__drm-copilot__validate_orchestration_artifacts` MCP
tool is not available in this TaskMaster checkout per prior planner findings; the authoritative
gate here is the planner-output SubagentStop hook):

- Canonical phase headings `### Phase N — <Title>` (em-dash), Phases 0-7.
- Sequential `[P#-T#]` task IDs within each phase.
- Phase 0 includes a policy-read task (P0-T1) and baseline command tasks (P0-T2..P0-T5).
- Every task names an explicit repo-root-relative file/path token.
- Final phase (Phase 7) runs the full QA loop: format -> analyzers -> nullable -> vstest coverage,
  with restart-on-change behavior and numeric coverage evidence.
- All schema-bearing evidence paths resolve to `<FEATURE>/evidence/{baseline,qa-gates,regression-testing,issue-updates}/`;
  no forbidden `artifacts/` evidence path is used; `artifacts/csharp/coverage.xml` is the raw
  review-gate consumable only.

plan-path: docs/features/active/2026-07-09-tasktree-testability-refactor-296/plan.2026-07-09T16-07.md

PREFLIGHT: ALL CLEAR
