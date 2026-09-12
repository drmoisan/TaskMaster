# F4 Per-File Research — `TlpCellSnapShot.cs`

Timestamp: 2026-08-07T22-40

Feature: `quickfiler-helper-classes-coverage` (issue #434), child F4 of epic
`quickfiler-per-file-coverage` (issue #136), wave 1, complexity band C3.

Scope of this artifact: exactly one production file, per the #136 one-file-at-a-time mandate. This
file declares **three** public types, all of which are in this artifact's denominator.

---

## 1. File facts

| Fact | Value | Evidence |
| --- | --- | --- |
| Path | `QuickFiler/Helper Classes/TlpCellSnapShot.cs` | — |
| Line count | 223 (last content line is `}` at line 223) | Full Read; EOF after 223 |
| Compiled | Yes | `QuickFiler/QuickFiler.csproj:353` — `<Compile Include="Helper Classes\TlpCellSnapShot.cs" />` |
| `[ExcludeFromCodeCoverage]` | **Absent** — confirmed | Repo grep for `ExcludeFromCodeCoverage` across `QuickFiler/Helper Classes/` returned **no matches** |
| Namespace | `QuickFiler` | `TlpCellSnapShot.cs:10` |
| Types declared | `public class TlpCellStates : Dictionary<string, TlpCellSnapShotList>` (`:12`); `public class TlpCellSnapShotList : List<TlpCellSnapShot>` (`:64`); `public class TlpCellSnapShot` (`:78`) | — |
| Internals visible to tests | Yes (not required — all three types are `public`) | `QuickFiler/Properties/AssemblyInfo.cs:5` |

Numeric baseline line coverage is captured at execution time with F1's per-file coverage harness
(epic `Shared Design` §6) and recorded under `<FEATURE>/evidence/qa-gates/`.

---

## 2. Member inventory (the coverage denominator)

### Type A — `TlpCellStates : Dictionary<string, TlpCellSnapShotList>` (lines 12–62)

| # | Member | Signature | Line span | Decision points |
| --- | --- | --- | --- | --- |
| A1 | default ctor | `public TlpCellStates() : base()` | 14–15 | 0 |
| A2 | typed-collection ctor | `public TlpCellStates(IEnumerable<KeyValuePair<string, TlpCellSnapShotList>> collection)` | 17–27 | **2** — `if (collection is null)` @19; `foreach` @25 |
| A3 | raw-collection ctor | `public TlpCellStates(IEnumerable<KeyValuePair<string, List<TlpCellSnapShot>>> collection)` | 29–39 | **2** — `if (collection is null)` @31; `foreach` @37 |
| A4 | `TryAddState` | `public bool TryAddState(string stateName)` | 41–50 | **1** — `if (this.ContainsKey(stateName))` @43 |
| A5 | `TryAddState` (overload) | `public bool TryAddState(string stateName, List<TlpCellSnapShot> snapShots)` | 52–61 | **1** — `if (this.ContainsKey(stateName))` @54 |

### Type B — `TlpCellSnapShotList : List<TlpCellSnapShot>` (lines 64–76)

| # | Member | Signature | Line span | Decision points |
| --- | --- | --- | --- | --- |
| B1 | default ctor | `public TlpCellSnapShotList() : base()` | 66–67 | 0 |
| B2 | collection ctor | `public TlpCellSnapShotList(IEnumerable<TlpCellSnapShot> collection) : base(collection)` | 69–70 | 0 (null validation is inherited from `List<T>`'s base constructor) |
| B3 | `ApplyState` | `public void ApplyState(IContainerControlLocal root)` | 72–75 | **1** — `List<T>.ForEach` iteration @73 |
| B3-L1 | closure `s => s.ApplyState(root)` | 73 | 1 line | 0 |

### Type C — `TlpCellSnapShot` (lines 78–222)

| # | Member | Signature | Line span | Decision points |
| --- | --- | --- | --- | --- |
| C1 | default ctor | `public TlpCellSnapShot()` | 80 | 0 |
| C2 | snapshot ctor | `public TlpCellSnapShot(TableLayoutPanel tlp, Control control)` | 82–85 | 0 (delegates to C3) |
| C3 | `SnapCell` | `public void SnapCell(TableLayoutPanel tlp, Control control)` | 87–110 | **4** — `for` @96; `for` @101; `if (ControlName.StartsWith("LblAc") && control is Label)` @106 (a short-circuiting `&&`, so **two** conditions) |
| C4 | `TlpName` | `public string TlpName { get; set; }` (expression-bodied over `_tlpName`) | 112–117 | 0 |
| C5 | `ControlName` | over `_controlName` | 119–124 | 0 |
| C6 | `AcceleratorText` | over `_acceleratorText` | 126–131 | 0 |
| C7 | `Cell` | `public TableLayoutPanelCellPosition Cell` over `_cell` | 133–138 | 0 |
| C8 | `Row` | `get => _cell.Row; set => _cell.Row = value;` | 139–143 | 0 |
| C9 | `Column` | `get => _cell.Column; set => _cell.Column = value;` | 144–148 | 0 |
| C10 | `RowStyles` | `public List<RowStyle>` over `_rowStyles` | 150–155 | 0 |
| C11 | `RowSpan` | `public int` over `_rowSpan` | 157–162 | 0 |
| C12 | `ColumnSpan` | `public int` over `_columnSpan` | 164–169 | 0 |
| C13 | `ColumnStyles` | `public List<ColumnStyle>` over `_columnStyles` | 171–176 | 0 |
| C14 | `Enabled` | `public bool` over `_enabled` | 178–183 | 0 |
| C15 | `Visible` | `public bool` over `_visible` | 185–190 | 0 |
| C16 | `ApplyState` | `public void ApplyState(IContainerControlLocal root)` | 192–221 | **6** — `for` @195; `for` @199; `if (!ControlName.IsNullOrEmpty())` @203; `if (control.Parent != tlp)` @208; `if (ControlName.StartsWith("LblAc") && control is Label)` @216 (two conditions) |

**Total executable surface: 3 types, 24 members + 1 lambda; 17 decision points.**

### Verified facts

- **C8/C9 mutate a struct field in place.** `Row`/`Column` write `_cell.Row` / `_cell.Column`
  directly on the `TableLayoutPanelCellPosition` **field** (not through the `Cell` property), so the
  mutation persists. `TlpCellStatesTests.cs:165-174` already pins this.
- **`RowStyle.Clone()` / `ColumnStyle.Clone()` are repo extension methods**, not framework members:
  `UtilitiesCS/Extensions/WinFormsExtensions.cs:310-317` and `:319-326`. Each returns
  `new RowStyle(sourceStyle.SizeType, sourceStyle.Height)` / `new ColumnStyle(SizeType, Width)` and
  **throws `ArgumentNullException` on null**. C16 therefore de-aliases stored styles on restore.
- **`string.IsNullOrEmpty` used at line 203 is the repo extension**
  `UtilitiesCS/Extensions/StringExtensions.cs:15` — null-safe, so a null `ControlName` takes the
  false branch rather than throwing.
- **C16 has two unguarded dereferences.** Line 194 `root.Controls.Find(TlpName, true).FirstOrDefault() as TableLayoutPanel`
  yields `null` when the name does not resolve, and line 196 then dereferences it. Line 205 does the
  same for the control, dereferenced at 206. Both are real `NullReferenceException` paths.
- **`ApplyState(IContainerControlLocal root)` is already an interface seam.** The class-level comment
  at `QuickFiler.Test/Helper Classes/TlpCellSnapShotTests.cs:11-19` records that a prior
  de-exemption cycle ("Cycle-5 (R2, de-exempted)") changed the parameter from a concrete `Control` to
  `IContainerControlLocal` (`UtilitiesCS/Interfaces/IWinForm/IContainerControl.cs:7`) precisely so
  tests could drive it from a `Mock<IItemViewer>` whose `Controls` getter returns a bare host's
  `Control.ControlCollection`. **Do not re-invent this seam.**

---

## 3. Existing test inventory

**Two test files**, both registered in `QuickFiler.Test/QuickFiler.Test.csproj`:

- `QuickFiler.Test/Helper Classes/TlpCellStatesTests.cs` — csproj line **162**
- `QuickFiler.Test/Helper Classes/TlpCellSnapShotTests.cs` — csproj line **163**

| Test method (file:line) | Production member(s) exercised |
| --- | --- |
| `TlpCellStatesTests.EmptyConstructor_CreatesEmptyStateDictionary` (`:15`) | **A1** |
| `TlpCellStatesTests.TypedCollectionConstructor_PreservesSnapshotListsByKey` (`:23`) | **A2** success path + `foreach` (25-26) |
| `TlpCellStatesTests.RawCollectionConstructor_ConvertsListsToTlpCellSnapShotLists` (`:41`) | **A3** success path (37-38) + **B2** |
| `TlpCellStatesTests.CollectionConstructors_WithEmptyInputs_CreateEmptyStateDictionary` (`:57`) | **A2**, **A3** zero-iteration loop path |
| `TlpCellStatesTests.TypedCollectionConstructor_WithDuplicateKeys_ThrowsArgumentException` (`:71`) | **A2** line 26 `Dictionary.Add` duplicate-key throw |
| `TlpCellStatesTests.TryAddState_WithoutSnapshots_AddsOnlyMissingState` (`:91`) | **A4** — both branches of guard @43 + **B1** |
| `TlpCellStatesTests.TryAddState_WithSnapshots_AddsConvertedListOnlyForMissingState` (`:104`) | **A5** — both branches of guard @54 + **B2** |
| `TlpCellStatesTests.TypedCollectionConstructor_WithNullInput_ThrowsArgumentNullException` (`:121`) | **A2** guard 19-22 |
| `TlpCellStatesTests.RawCollectionConstructor_WithNullInput_ThrowsArgumentNullException` (`:129`) | **A3** guard 31-34 |
| `TlpCellStatesTests.SnapshotConstructor_CapturesControlCellState` (`:141`) | **C2**, **C3** full body incl. **true** branch of @106; getters **C4**, **C5**, **C6**, **C8**, **C9** |
| `TlpCellStatesTests.RowAndColumnAccessors_UpdateCellPosition` (`:165`) | **C1**, **C8**/**C9** setters, **C7** getter |
| `TlpCellStatesTests.ApplyState_WhenControlHasDifferentParent_ReparentsAndRestoresCell` (`:177`) | **C16** full body: loops @195/@199 (1 iteration each), **true** @203, **true** @208, **true** @216; setters **C4**, **C5**, **C6**, **C7**, **C10**–**C15** |
| `TlpCellSnapShotTests.ApplyState_OnInstance_RestoresSnapshottedEnabledVisibleAndAcceleratorText` (`:24`) | **C1**, **C3**, **C16** with **false** branch of @208 |
| `TlpCellSnapShotTests.ApplyState_OnList_AppliesEveryEntry` (`:66`) | **B2**, **B3** + **B3-L1**; **C3** with **false** branch of @106 (control named `LblOne`); **C16** with **false** branch of @216 |

Cross-child test files that *consume* these types (not part of F4's test surface, listed for
completeness): `QuickFiler.Test/Controllers/QfcItemController.NavigationTests.cs:313,325,326,366,378,379`;
`QuickFiler.Test/Controllers/QfcFormControllerSeamTests.cs:306,307,314,325,332,333,348`;
`QuickFiler.Test/Controllers/QfcCollectionControllerDarkModeTests.cs:58`.

---

## 4. Per-member coverage gap

| Member | Status | Missed detail |
| --- | --- | --- |
| A1 | **covered** | `TlpCellStatesTests.cs:15` |
| A2 | **covered** | Both guard branches, populated and empty loop (`:23`, `:57`, `:71`, `:121`) |
| A3 | **covered** | Both guard branches, populated and empty loop (`:41`, `:57`, `:129`) |
| A4 | **covered** | Both branches (`:91`) |
| A5 | **covered** | Both branches (`:104`) |
| B1 | **covered** | `TlpCellStatesTests.cs:91` (via `TryAddState`) |
| B2 | **partially covered** (branches missed: base-constructor null path) | Success path at `:41`, `:104`, `TlpCellSnapShotTests.cs:105`. `new TlpCellSnapShotList(null)` → `List<T>` base throws `ArgumentNullException` — never exercised. |
| B3 + B3-L1 | **partially covered** (branches missed: zero-element list) | Two-element list at `TlpCellSnapShotTests.cs:113`. Empty-list path never exercised. |
| C1 | **covered** | `:46`, `:167` |
| C2 | **covered** | `TlpCellStatesTests.cs:155` |
| C3 `SnapCell` | **partially covered** (branches missed: 3) | (a) `@106` with `StartsWith("LblAc") == true` **and** `control is Label == false` — never exercised; (b) `for` @96 with `RowSpan > 1` (multi-iteration) — never exercised, every existing test uses span 1; (c) `for` @101 with `ColumnSpan > 1` — never exercised. Error paths for a null `tlp`/`control` (lines 89-91) and for a control that is not a child of the panel (`GetCellPosition` returns `(-1,-1)` → `RowStyles[-1]`) are also unexercised. |
| C4–C15 (12 properties) | **covered** | All getters are read by C16 (lines 194-218) and by the assertions at `TlpCellStatesTests.cs:157-161`, `:211-217`; all setters are written by the object initializer at `TlpCellStatesTests.cs:193-205`. |
| C16 `ApplyState` | **partially covered** (branches missed: 5) | (a) `@203` **false** branch (null or empty `ControlName`) — never exercised; (b) `@216` with `StartsWith("LblAc") == true` **and** `control is Label == false` — never exercised; (c) `for` @195 with `RowSpan > 1`; (d) `for` @199 with `ColumnSpan > 1`; (e) the two `NullReferenceException` paths at 194-196 (unresolved `TlpName`) and 205-206 (unresolved `ControlName`). Also unexercised: a null `root`. |

**Summary: line coverage for this file is already substantial; the residual gap is almost entirely
branch and error-path coverage, concentrated in `C3.SnapCell` and `C16.ApplyState`.** The only
member whose *lines* are meaningfully at risk is none — every member body is entered by at least one
existing test.

---

## 5. Testability classification per member

| Member | Classification | WinForms API touched |
| --- | --- | --- |
| A1–A5 | **pure-testable-now** | None. Pure `Dictionary<string, …>` manipulation. |
| B1, B2 | **pure-testable-now** | None. Pure `List<T>`. |
| B3, B3-L1 | **pure-testable-now** | None directly; forwards to C16. |
| C1, C2 | **pure-testable-now** | None (C2 forwards to C3). |
| C3 `SnapCell` | **pure-testable-now** (with in-memory, never-shown controls) | `TableLayoutPanel.Name` (89), `Control.Name` (90), **`TableLayoutPanel.GetCellPosition(Control)`** (91), **`TableLayoutPanel.GetRowSpan(Control)`** (93), `TableLayoutPanel.RowStyles[i]` (97), **`TableLayoutPanel.GetColumnSpan(Control)`** (99), `TableLayoutPanel.ColumnStyles[i]` (102), `Control.Enabled` (104), `Control.Visible` (105), `Label.Text` (108). All are property/extender reads that create **no window handle**. |
| C4–C15 | **pure-testable-now** | None. `TableLayoutPanelCellPosition` is a plain struct; `RowStyle`/`ColumnStyle` are plain classes. |
| C16 `ApplyState` | **pure-testable-now — the seam already exists** | Via `IContainerControlLocal.Controls` → `Control.ControlCollection.Find(name, searchAllChildren: true)` (194, 205); `TableLayoutPanel.RowStyles[i]` (196), `ColumnStyles[i]` (200), `Control.Enabled` (206), `Control.Visible` (207), **`Control.Parent`** (208, 210), **`TableLayoutPanel.SetCellPosition`** (212), **`SetRowSpan`** (213), **`SetColumnSpan`** (214), `Label.Text` (218). All operate on in-memory, never-shown controls. |

**Precedent that this works with no live form, no popup, and no UI thread:** both existing test
files already call every one of the APIs listed above —
`TlpCellSnapShotTests.cs:29-57` and `TlpCellStatesTests.cs:143-218` — using a bare `new Control()`
host plus `Mock<IItemViewer>` whose `Controls` getter returns the host's `ControlCollection`
(`TlpCellSnapShotTests.cs:53-54`, `TlpCellStatesTests.cs:206-207`).

---

## 6. Seam proposal

**Recommendation: introduce NO new seam. Make no production change to this file.**

The one seam this file needed has already been introduced and ratified in a prior de-exemption
cycle: **`ApplyState(IContainerControlLocal root)`** (C16, B3), documented at
`QuickFiler.Test/Helper Classes/TlpCellSnapShotTests.cs:11-19`. The remaining gap is entirely
additional test cases against the existing surface (§4), not a reachability problem.

Options evaluated against the epic §2 hierarchy, and why each is rejected:

1. **Interface seam (rank 1) — `ITlpCellHost` replacing `TableLayoutPanel` in `SnapCell(tlp, control)`.**
   **REJECTED.** `SnapCell` is invoked via the `TlpCellSnapShot(TableLayoutPanel, Control)`
   constructor from `QuickFiler/Viewers/QfcFormViewer.cs:201-253` — **twelve** construction sites in
   a **sibling-owned (F15)** file. Changing the parameter type would require editing
   `QuickFiler/Viewers/QfcFormViewer.cs:194-253`, owned by F15. That is a merge conflict.
   **Alternative that does not require it:** none is needed — the concrete
   `TableLayoutPanel`/`Control` parameters are already directly constructible in a unit test (§5),
   so the seam buys no reachability.

2. **Injectable delegate seam (rank 2) — an optional
   `Func<TableLayoutPanel, Control, TableLayoutPanelCellPosition> cellPositionReader = null`
   parameter on `SnapCell`, defaulting to `(t, c) => t.GetCellPosition(c)`.**
   **REJECTED on value, not on conflict.** An optional parameter with a production default *would*
   keep all twelve F15 call sites compiling unchanged, so it satisfies the additive-only constraint.
   But it yields zero coverage that the direct in-memory test does not already yield, and it adds a
   parameter to a public API for test convenience only — contrary to CLAUDE.md § "Simplicity first".

3. **Adapter seam (rank 3) — wrapping the `TableLayoutPanel` extender calls.** **REJECTED.** Same
   objection as (2), with more indirection.

4. **Pure-extraction of the span arithmetic** — an `internal static IEnumerable<int> SpanIndices(int start, int span)`
   helper replacing the two `for` loops in C3 and the two in C16. **AVAILABLE AND ADDITIVE**
   (new private/internal member; no signature change; no call-site change anywhere). **Still not
   recommended for this child:** the loops are two lines each and are fully coverable by supplying a
   snapshot with `RowSpan`/`ColumnSpan` greater than 1 (test cases 10, 11, 22, 23 in §9). Extracting
   them would relocate coverage rather than create it, and would add lines to a file whose members
   are consumed by five sibling children. Record as an optional follow-up.

**Conflict statement for the recommendation: requires no sibling-owned file change** (no production
change at all).

---

## 7. Cross-child conflict analysis

**This is the highest cross-child coupling of any file in the F4 theme/layout cluster.** F4 owns
only the 13 files under `QuickFiler/Helper Classes/` plus `QuickFiler/Interfaces/IEmailMoveMonitor.cs`.
Every file below belongs to a sibling running in parallel.

### Every file outside F4 that references these three types (repo-wide `*.cs` grep)

| File and line | Reference | Owning child |
| --- | --- | --- |
| `QuickFiler/Viewers/QfcFormViewer.cs:187` | `public TlpCellStates CaptureTlpCellStates()` — return type | **F15** `quickfiler-form-viewers-bayesian-coverage` |
| `QuickFiler/Viewers/QfcFormViewer.cs:194` | `new TlpCellStates(...)` | **F15** |
| `QuickFiler/Viewers/QfcFormViewer.cs:195, 197, 199, 227, 229` | `List<KeyValuePair<string, List<TlpCellSnapShot>>>` construction | **F15** |
| `QuickFiler/Viewers/QfcFormViewer.cs:201, 205, 209, 213, 217, 221, 231, 235, 239, 243, 247, 251` | **twelve** `new TlpCellSnapShot(tlp, control)` calls | **F15** |
| `QuickFiler/Interfaces/IQfcFormViewer.cs:32` | `TlpCellStates CaptureTlpCellStates();` — interface member | **F6** `quickfiler-qfc-form-explorer-controller-coverage` |
| `QuickFiler/Controllers/QfcFormController.cs:88` | `private TlpCellStates _states;` | **F6** |
| `QuickFiler/Controllers/QfcFormController.SetupDisposal.cs:37` | `_states = _formViewer.CaptureTlpCellStates();` | **F6** |
| `QuickFiler/Controllers/QfcQueue.cs:320` | `private TlpCellStates _tlpStates;` | **F2** `quickfiler-queue-admission-coverage` |
| `QuickFiler/Controllers/QfcQueue.cs:321` | `public TlpCellStates TlpStates` | **F2** |
| `QuickFiler/Controllers/IQfcQueue.cs:16` | `TlpCellStates TlpStates { get; set; }` | **F2** |
| `QuickFiler/Controllers/IQfcQueue1.cs:16` | `TlpCellStates TlpStates { get; set; }` | **F2** |
| `QuickFiler/Controllers/QfcItemController.cs:60` | `private TlpCellStates _tlpStates;` | **F10** `quickfiler-item-controller-coverage` |
| `QuickFiler/Controllers/QfcItemController.Initialization.cs:37, 94, 119, 147, 354, 412, 445` | **seven** `TlpCellStates tlpStates` parameters | **F10** |
| `QuickFiler/Controllers/QfcItemController.Navigation.cs:171, 189` | comments referencing `TlpCellSnapShot`-bound overloads (no compile dependency) | **F10** |
| `QuickFiler/Controllers/QfcCollectionController.cs:38, 77, 590, 612, 638, 659` | **six** `TlpCellStates` parameters/fields | **F11** `quickfiler-collection-controller-coverage` |

**Total: 38 sibling-owned production references across five siblings — F2, F6, F10, F11, F15.**

**Verdict: requires no sibling-owned file change.** The recommendation in §6 is tests-only, so every
one of the 38 references keeps compiling byte-identically.

**Hard constraint derived from the above, binding on the atomic plan:** any change to this file must
be **strictly additive and signature-preserving**. Specifically, the plan must not (a) change any
parameter or return type of `TlpCellStates(…)`, `TlpCellSnapShotList(…)`, `TlpCellSnapShot(…)`,
`SnapCell`, or either `ApplyState`; (b) rename any of the three types; (c) split the three types into
separate files (see §8); or (d) change any of C4–C15's property types. Had seam option (1) in §6 been
adopted, it would have required editing `QuickFiler/Viewers/QfcFormViewer.cs:201-251` (owned by
**F15**); the alternative that avoids that edit is precisely the recommendation — keep the concrete
`TableLayoutPanel`/`Control` signature and drive it with in-memory controls.

### Test-side cross-child note

The existing F4 tests use `Mock<IItemViewer>` (`TlpCellSnapShotTests.cs:53`,
`TlpCellStatesTests.cs:206`). `IItemViewer` is `QuickFiler/Viewers/IItemViewer.cs`, owned by **F14**.
This is a **test-only** consumption of an existing public interface — no F14 production file is
edited. If F14 changes `IItemViewer.Controls` mid-wave the result is a compile break in F4's tests,
handled by the child's R1–R5 remediation loop, not a merge conflict. New tests should keep using
`Mock<IItemViewer>` for consistency with the existing files rather than introducing a second
`IContainerControlLocal` stub.

### Shared-file risk

| Shared file | Required edit | Risk |
| --- | --- | --- |
| `QuickFiler.Test/QuickFiler.Test.csproj` | **NONE** | Both destination test files are already registered — `TlpCellStatesTests.cs` at line **162** and `TlpCellSnapShotTests.cs` at line **163**. All 27 new cases in §9 land in these two existing files. **This file therefore contributes zero shared-file conflict risk.** |
| `QuickFiler/QuickFiler.csproj` | **NONE** | No production change. |

---

## 8. 500-line compliance

- **Production file: 223 of 500. Headroom 277 lines. Compliant; no production change proposed, so it
  stays at 223.** No partial split is required.
- **A cohesion split is deliberately deferred, not overlooked.** The file declares three types, which
  is in tension with CLAUDE.md § "Module & File Structure" item 1 ("Keep modules cohesive — a
  module/file should have a clear purpose. Avoid dumping unrelated classes/functions into the same
  file"). Splitting into `TlpCellStates.cs`, `TlpCellSnapShotList.cs`, and `TlpCellSnapShot.cs`
  would:
  - require **three** new `<Compile Include>` lines in `QuickFiler/QuickFiler.csproj` (the
    `Helper Classes\` block, lines 342-354) — a shared-file conflict against F2, F9, F10, and F11,
    all of which add production files to the same `<ItemGroup>` during the same wave; **and**
  - **redraw the per-file coverage denominator mid-epic.** F1's ledger and harness key on file
    paths; replacing one measured file with three would invalidate F1's classification entry for
    `TlpCellSnapShot.cs` and require a ledger amendment that F4 has no authority to make (epic
    `Shared Design` §1: F1 owns exemption and classification).

  **Recommendation: do not split during this epic.** Record it as a follow-up issue against the
  cohesion rule, to be scheduled after the capstone F16 closes.
- **Test files:** `TlpCellStatesTests.cs` is 247 lines and `TlpCellSnapShotTests.cs` is 122 lines
  (both of 500). The 27 new cases in §9 are apportioned 6 to the former (→ ~330 lines) and 21 to the
  latter (→ ~430 lines). Both stay under 500 with margin. No test-file split needed.
- Cross-check of the other three F4 theme/layout files (each has its own artifact):
  `EfcThemeHelper.cs` 499/500 (1 line of headroom — the cluster's binding constraint);
  `QfcThemeHelper.cs` 375/500; `QfcThemeControlSet.cs` 110/500.

---

## 9. Recommended test cases (enumerated individually)

Destinations are **existing, already-registered** files; no csproj edit is required.

### Type A — `TlpCellStates` → `QuickFiler.Test/Helper Classes/TlpCellStatesTests.cs`

| # | `[TestMethod]` name | Arrange / Act / Assert | Category |
| --- | --- | --- | --- |
| 1 | `TryAddState_WithNullStateName_ThrowsArgumentNullException` | Arrange empty `TlpCellStates`; Act `TryAddState(null)`; Assert `Throw<ArgumentNullException>()` — `Dictionary.ContainsKey(null)` throws at line 43. | invalid-input |
| 2 | `TryAddState_WithSnapshotsAndNullStateName_ThrowsArgumentNullException` | as #1 for the two-argument overload (line 54). | invalid-input |
| 3 | `TryAddState_WithNullSnapshotList_ThrowsArgumentNullException` | Act `TryAddState("expanded", null)`; Assert `Throw<ArgumentNullException>()` — `new TlpCellSnapShotList(null)` at line 58 delegates to `List<T>`'s base ctor. Also covers **B2**'s null path. | invalid-input |
| 4 | `TryAddState_WithEmptySnapshotList_AddsAnEmptyState` | Act `TryAddState("expanded", new List<TlpCellSnapShot>())`; Assert returns `true` and `states["expanded"].Should().BeEmpty()`. | boundary |
| 5 | `TryAddState_WithEmptyStringStateName_AddsTheState` | Act `TryAddState(string.Empty)`; Assert returns `true` and the key exists — empty string is a valid dictionary key. | boundary |
| 6 | `RawCollectionConstructor_WithANullValueList_ThrowsArgumentNullException` | Arrange one `KeyValuePair<string, List<TlpCellSnapShot>>("raw", null)`; Act construct; Assert `Throw<ArgumentNullException>()` — line 38 wraps a null list. | error-handling |

### Type B — `TlpCellSnapShotList` → `QuickFiler.Test/Helper Classes/TlpCellSnapShotTests.cs`

| # | `[TestMethod]` name | Arrange / Act / Assert | Category |
| --- | --- | --- | --- |
| 7 | `ListCollectionConstructor_WithNullCollection_ThrowsArgumentNullException` | Act `new TlpCellSnapShotList(null)`; Assert `Throw<ArgumentNullException>()` — **B2** null path. | invalid-input |
| 8 | `ApplyState_OnAnEmptyList_DoesNotThrowAndTouchesNoControl` | Arrange empty `TlpCellSnapShotList` and a `Mock<IItemViewer>`; Act `ApplyState(mock.Object)`; Assert no throw and `mock.Verify(v => v.Controls, Times.Never)` — **B3** zero-iteration path. | boundary |
| 9 | `ApplyState_OnList_AppliesEntriesInDeclarationOrder` | Arrange two snapshots targeting the **same** control with different `Enabled` values; Act `ApplyState`; Assert the final state equals the **last** entry's — pins the `ForEach` ordering invariant of **B3**/**B3-L1**. | positive |

### Type C — `SnapCell` → `QuickFiler.Test/Helper Classes/TlpCellSnapShotTests.cs`

| # | `[TestMethod]` name | Arrange / Act / Assert | Category |
| --- | --- | --- | --- |
| 10 | `SnapCell_WithMultiRowSpan_CapturesEveryRowStyleInTheSpan` | Arrange a 3-row `TableLayoutPanel` with distinct `RowStyle` heights, a label at row 0 with `SetRowSpan(label, 2)`; Act `SnapCell`; Assert `RowStyles.Should().HaveCount(2)` and the two heights match rows 0 and 1 — **covers the multi-iteration path of the `for` at line 96.** | boundary |
| 11 | `SnapCell_WithMultiColumnSpan_CapturesEveryColumnStyleInTheSpan` | as #10 for columns with `SetColumnSpan(label, 2)`; Assert `ColumnStyles.Should().HaveCount(2)` — **`for` at line 101.** | boundary |
| 12 | `SnapCell_WhenControlNameStartsWithLblAcButTheControlIsNotALabel_LeavesAcceleratorTextNull` | Arrange a `Button { Name = "LblAcOpen", Text = "Open" }` in the panel; Act `SnapCell`; Assert `AcceleratorText.Should().BeNull()` — **covers the second condition of the `&&` at line 106 evaluating false.** | boundary |
| 13 | `SnapCell_WhenControlNameDoesNotStartWithLblAc_LeavesAcceleratorTextNull` | Arrange a `Label { Name = "TxtSubject", Text = "x" }`; Act; Assert `AcceleratorText.Should().BeNull()` — first condition false (already reached incidentally at `TlpCellSnapShotTests.cs:79-98`; this makes it explicit and independent). | boundary |
| 14 | `SnapCell_WithNullTableLayoutPanel_ThrowsNullReferenceException` | Act `new TlpCellSnapShot().SnapCell(null, new Label())`; Assert `Throw<NullReferenceException>()` — pins the documented absence of a guard at line 89. | error-handling |
| 15 | `SnapCell_WithNullControl_ThrowsNullReferenceException` | Act `SnapCell(tlp, null)`; Assert `Throw<NullReferenceException>()` — line 90. | error-handling |
| 16 | `SnapCell_WhenTheControlIsNotAChildOfThePanel_ThrowsArgumentOutOfRangeException` | Arrange a `TableLayoutPanel` and a `Label` that is **not** added to it (so `GetCellPosition` returns `(-1, -1)`); Act `SnapCell`; Assert `Throw<ArgumentOutOfRangeException>()` — the `for` at line 96 immediately indexes `RowStyles[-1]`. | error-handling |
| 17 | `SnapCell_CalledTwiceOnTheSameInstance_OverwritesThePreviousSnapshot` | Arrange one snapshot instance; Act `SnapCell(tlpA, labelA)` then `SnapCell(tlpB, labelB)`; Assert every property reflects the **second** call, and `RowStyles`/`ColumnStyles` are new list instances — pins the state-transition contract of a reused instance (UT2 § "State transitions for stateful components"). | positive |

### Type C — `ApplyState` → `QuickFiler.Test/Helper Classes/TlpCellSnapShotTests.cs`

| # | `[TestMethod]` name | Arrange / Act / Assert | Category |
| --- | --- | --- | --- |
| 18 | `ApplyState_WithEmptyControlName_RestoresStylesButPerformsNoControlMutation` | Arrange a snapshot with `ControlName = string.Empty`, valid `TlpName`, `RowStyles`/`ColumnStyles`; Act `ApplyState`; Assert row/column styles were restored and the label in the panel was **not** re-parented or re-enabled — **covers the false branch of line 203.** | boundary |
| 19 | `ApplyState_WithNullControlName_RestoresStylesButPerformsNoControlMutation` | as #18 with `ControlName = null` — proves the null-safe `IsNullOrEmpty` extension (`StringExtensions.cs:15`) rather than a throw. | boundary |
| 20 | `ApplyState_WhenTheTlpNameDoesNotResolve_ThrowsNullReferenceException` | Arrange a host containing no panel of that name; Act `ApplyState`; Assert `Throw<NullReferenceException>()` — the `as TableLayoutPanel` at 194 yields null and 196 dereferences it. | error-handling |
| 21 | `ApplyState_WhenTheControlNameDoesNotResolve_ThrowsNullReferenceException` | Arrange a resolvable panel but no control of that name; Act; Assert `Throw<NullReferenceException>()` — line 205 → 206. | error-handling |
| 22 | `ApplyState_WithMultiRowSpan_RestoresEveryRowStyleInTheSpan` | Arrange a snapshot with `RowSpan = 2` and two distinct `RowStyle`s; Act; Assert both `tlp.RowStyles[row]` and `[row+1]` match — **multi-iteration path of the `for` at line 195.** | boundary |
| 23 | `ApplyState_WithMultiColumnSpan_RestoresEveryColumnStyleInTheSpan` | as #22 for columns — **`for` at line 199.** | boundary |
| 24 | `ApplyState_WhenControlNameStartsWithLblAcButTheControlIsNotALabel_DoesNotAssignAcceleratorText` | Arrange a `Button { Name = "LblAcOpen", Text = "Live" }` and a snapshot with `AcceleratorText = "Snapshot"`; Act; Assert `button.Text.Should().Be("Live")` — **second condition of the `&&` at line 216 evaluating false.** | boundary |
| 25 | `ApplyState_WithNullRoot_ThrowsNullReferenceException` | Act `snapshot.ApplyState(null)`; Assert `Throw<NullReferenceException>()` — line 194. | invalid-input |
| 26 | `ApplyState_RestoresRowStyleSizeTypeAndValue` | Arrange a snapshot holding `new RowStyle(SizeType.Absolute, 33)` against a panel whose row style is `Percent, 50`; Act; Assert `tlp.RowStyles[i].SizeType == SizeType.Absolute` **and** `.Height == 33` — pins the `RowStyle.Clone()` extension contract (`WinFormsExtensions.cs:310-317`). | positive |
| 27 | `ApplyState_DoesNotAliasTheStoredStyleInstances` | Act `ApplyState`, then mutate `tlp.RowStyles[i].Height`; Assert the snapshot's own `RowStyles[0].Height` is unchanged — proves `Clone()` de-aliases and that a second `ApplyState` would restore the same values. | positive |

**Total: 27 enumerated test cases.** Category spread: 5 positive, 5 invalid-input, 11 boundary,
6 error-handling — all four categories present.

---

## 10. STA determination

**STA is NOT required for any member of this file. No `*.StaTests.cs` file should be created.**

Per-member justification, with the seam hierarchy explicitly accounted for:

- **A1–A5, B1, B2** — no WinForms surface at all. Not applicable.
- **B3, C16 `ApplyState`** — the **rank-1 interface seam is already in place**:
  `IContainerControlLocal` (`UtilitiesCS/Interfaces/IWinForm/IContainerControl.cs:7`). The hierarchy
  therefore terminates at rank 1 and STA is never reached. The residual WinForms calls
  (`Control.ControlCollection.Find`, `TableLayoutPanel.SetCellPosition`/`SetRowSpan`/`SetColumnSpan`,
  `RowStyles[i]`/`ColumnStyles[i]`, `Control.Parent`, `Control.Enabled`, `Control.Visible`,
  `Label.Text`) act on caller-supplied in-memory controls, create no window handle, show no form,
  and post no window message. **Proven:** `TlpCellSnapShotTests.cs:24-63` and
  `TlpCellStatesTests.cs:177-218` already exercise every one of these calls in plain `[TestClass]`
  files carrying no STA attribute and covered by no apartment-scoped runsettings.
- **C3 `SnapCell`** — takes concrete `TableLayoutPanel` and `Control` parameters. A seam was
  **considered and rejected** (§6 option 1) because it would require editing twelve F15-owned call
  sites; a delegate seam was **considered and rejected** (§6 option 2) because it adds no
  reachability. The hierarchy was therefore not "exhausted" in the sense that would justify STA —
  it was **satisfied without a seam**, because both parameter types are directly constructible in a
  unit test. `TlpCellStatesTests.cs:141-162` already does exactly this. STA is not required.
- **C1, C2, C4–C15** — plain data members; not applicable.

Tests must construct no `Form`, show no popup, and take no dependency on the UI thread. Nothing in
this file's test set approaches those boundaries.

---

## 11. Determinism

| Concern | Finding | Requirement on tests |
| --- | --- | --- |
| Wall-clock time | **None.** No `DateTime`, `DateTimeOffset`, `TimeProvider`, `Stopwatch`, or timer in the file. | No clock seam needed; `FakeTimeProvider` not applicable. |
| Randomness | **None.** | No seeded RNG needed. |
| Ambient state — `SystemColors` / `Color` | **None.** This file contains no colour value. |
| Ambient state — **`Control.Visible` is parent-dependent** | `Control.Visible` (read at line 105, written at 207) returns the **effective** visibility, which is false when any ancestor is invisible. With the established `new Control()` host pattern the host has no parent and defaults to visible, so a child label reports `true`; `TlpCellSnapShotTests.cs:60-62` relies on and confirms this. | New tests must assert the **round trip** (snapshot → mutate → restore → equals the snapshotted value) rather than an absolute expected `Visible`, so the assertion does not silently couple to the ancestor-visibility rule. Applies to cases 9, 17, 18, 19, 24. |
| Ambient state — **`Control.ControlCollection.Find(name, searchAllChildren: true)`** | Deterministic given a fixed control tree, but returns matches in tree order and `FirstOrDefault()` takes the first. | Tests that add two controls with the same `Name` would be order-dependent; **no proposed case does this**. Keep control names unique within each test's host. |
| Ambient state — DPI / font scaling | `RowStyle.Height` and `ColumnStyle.Width` with `SizeType.Absolute` are stored verbatim and are not scaled by `SnapCell`/`ApplyState`; no layout pass is triggered because no control is shown. | Assert `SizeType` **and** the numeric value, as case 26 does. Do not assert computed `Control.Width`/`Height`, which would be layout- and DPI-dependent. |
| COM | **None.** No Outlook interop type appears in this file. |
| `Thread.Sleep` / `Task.Delay` / real waits | None in the file; **prohibited** in tests (`.claude/rules/general-unit-test.md` § Determinism Infrastructure; repo-root `BannedSymbols.txt`). | — |
| Temporary files, external services | None. | Prohibited by UT4. |
| Cross-test shared state | `ApplyState` mutates the supplied control tree (parent, cell position, spans, enabled, visible, text). | Every test must build its own host/panel/control graph in `Arrange`. No `[ClassInitialize]` control instances. The existing files already follow this. |

---

## 12. Projected coverage

- **Every member of all three types is already entered by at least one existing test** (§4), so the
  file's current line coverage is high and the residual deficit is concentrated in branch outcomes
  and error paths.
- The 27 cases in §9 close: **B2**'s null path; **B3**'s zero-iteration path; **C3**'s two
  multi-iteration loops, its `&&` second-condition-false outcome, and three error paths; **C16**'s
  `@203` false branch, `@216` second-condition-false outcome, two multi-iteration loops, two
  `NullReferenceException` paths, and its null-`root` path; plus the `Clone()` de-aliasing contract.
- After the proposed set, **no branch outcome and no line in the file remains unexercised.**
- **Projected line coverage: ~100% of executable lines. Projected branch coverage: ~100%** across the
  17 decision points.
- **Clears the 80% floor decisively.** The argument is structural: all 24 members plus the single
  lambda are reachable from plain constructor/method calls on in-memory objects, with the one
  host-facing entry point (`ApplyState`) already behind an `IContainerControlLocal` interface seam.
- **This file does not require an exemption.** It should be classified `testable` in F1's ledger
  (`docs/features/epics/quickfiler-per-file-coverage/coverage-ledger.md`), which remains the
  authority on that classification. It cannot qualify for the CLAUDE.md § UT2 exemption: it is not
  form-derived, not Designer-generated, and depends on no `Application`/`MailItem`/`Store`/`MAPIFolder`.
  Its WinForms dependency is on plain container/layout types that a unit test constructs directly —
  precisely the "testable seam within an otherwise-COM-bound assembly" that § UT2 explicitly holds to
  the 80% floor.
- Numeric before/after per-file figures are produced by **F1's harness** (Cobertura output of
  `Invoke-MSTestWithCoverage.ps1`) at execution time and committed under
  `<FEATURE>/evidence/qa-gates/`.
