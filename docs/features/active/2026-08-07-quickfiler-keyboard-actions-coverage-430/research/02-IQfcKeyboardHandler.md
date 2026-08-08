# Per-File Coverage Research — `QuickFiler/Interfaces/IQfcKeyboardHandler.cs`

Timestamp: 2026-08-07T21-55
Feature: `quickfiler-keyboard-actions-coverage` (issue #430, epic child F3, wave 1)
Epic: `quickfiler-per-file-coverage` (issue #136)
Branch: `feature/quickfiler-keyboard-actions-coverage`

---

## 1. File Under Research

| Attribute | Value |
| --- | --- |
| Path | `C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-aafcc2531072ca96b\QuickFiler\Interfaces\IQfcKeyboardHandler.cs` |
| Line count | 37 |
| Type | `public interface IQfcKeyboardHandler` (line 9), namespace `QuickFiler.Interfaces` |
| Compiled | Yes — `QuickFiler/QuickFiler.csproj:366` `<Compile Include="Interfaces\IQfcKeyboardHandler.cs" />` |
| `[ExcludeFromCodeCoverage]` | **Absent.** No attribute on the file or the type. |
| Existing tests | None targeting this file directly (correct — see §5). Referenced by 17 test files as a Moq target. |
| Exemption-status authority | **F1's `docs/features/epics/quickfiler-per-file-coverage/coverage-ledger.md`.** The recommended classification is `interface-only` (see §5), which is a third category distinct from both `testable` and `ratified-exempt`. |
| Per-file coverage measurement | Numeric per-file line coverage will be measured at execution time with F1's harness (derived from the Cobertura output of `scripts/vscode/Invoke-MSTestWithCoverage.ps1`). This file is expected to report either 0 sequence points or to be absent from the Cobertura class list entirely, because it emits no IL method bodies. |

---

## 2. Structural Inventory

The file contains **zero executable statements**. Every declaration is a member signature; the C# compiler emits abstract method/property metadata with no IL bodies, so there are no sequence points for a coverage collector to record.

| Lines | Declaration | Kind | Dependency surface |
| --- | --- | --- | --- |
| 1–5 | `using System; System.Collections.Generic; System.Threading.Tasks; System.Windows.Forms; QuickFiler.Controllers;` | usings | `System.Windows.Forms` (for `Keys`, `KeyEventArgs`, `PreviewKeyDownEventArgs`), `QuickFiler.Controllers` (for `KbdActions<>`, `KaChar`, `KaCharAsync`, `KaKey`, `KaKeyAsync`, `KaStringAsync`, `ItemViewer` resolution). **`System.Collections.Generic` (line 2) is unused** — no `Dictionary`/`List`/`IEnumerable` appears in any signature. |
| 7 | `namespace QuickFiler.Interfaces` | namespace | — |
| 9 | `public interface IQfcKeyboardHandler` | type decl | — |
| 11 | `bool KbdActive { get; set; }` | property sig | PURE |
| 12 | `void ToggleKeyboardDialog();` | method sig | PURE |
| 13 | `void ToggleKeyboardDialog(object sender, KeyEventArgs e);` | method sig | WinForms `KeyEventArgs` (in-memory constructible) |
| 14 | `Task ToggleKeyboardDialogAsync();` | method sig | PURE |
| 15 | `void ToggleKeyboardDialogAsync(object sender, KeyEventArgs e);` | method sig | WinForms. **Returns `void` while the name ends in `Async`** — this is the `async void` event-handler shape; see §7 R-2. |
| 16 | `void KeyboardHandler_PreviewKeyDownAsync(object sender, PreviewKeyDownEventArgs e);` | method sig | WinForms `PreviewKeyDownEventArgs` |
| 17 | `void KeyboardHandler_KeyDown(object sender, KeyEventArgs e);` | method sig | WinForms |
| 18 | `void KeyboardHandler_KeyDownAsync(object sender, KeyEventArgs e);` | method sig | WinForms |
| 20 | `//Dictionary<char, Action<char>> CharActions { get; set; }` | commented-out legacy signature | — |
| 21 | `KbdActions<char, KaChar, Action<char>> CharActions { get; set; }` | property sig | `QuickFiler.Controllers.KbdActions<>`/`KaChar` (first-party, host-neutral, already covered by `KaCharTests.cs` / `KbdActionsTests.cs`) |
| 22 | `KbdActions<char, KaCharAsync, Func<char, Task>> CharActionsAsync { get; set; }` | property sig | first-party |
| 23 | `KbdActions<Keys, KaKey, Action<Keys>> KeyActions { get; set; }` | property sig | first-party + WinForms `Keys` enum |
| 24 | `KbdActions<Keys, KaKeyAsync, Func<Keys, Task>> KeyActionsAsync { get; set; }` | property sig | first-party + `Keys` |
| 25 | `KbdActions<Keys, KaKeyAsync, Func<Keys, Task>> AlwaysOnKeyActionsAsync { get; set; }` | property sig | first-party + `Keys` |
| 26 | `KbdActions<string, KaStringAsync, Func<string, Task>> StringActionsAsync { get; set; }` | property sig | first-party |
| 28 | `void CboFolders_KeyDownAsync(object sender, KeyEventArgs e);` | method sig | WinForms |
| 30–31 | `// #351` rationale comment | comment | — |
| 32–35 | `void BreadcrumbArrowFallThrough(ItemViewer viewer, UtilitiesCS.OutlookObjects.Folder.BreadcrumbArrowDirection direction);` | method sig | **`QuickFiler.ItemViewer`** (concrete WinForms `UserControl`, F14-owned) + `UtilitiesCS.OutlookObjects.Folder.BreadcrumbArrowDirection` (enum) |
| 36–37 | closing braces | — | — |

**Totals: 1 type, 7 properties, 8 methods, 0 fields, 0 events, 0 nested types, 0 executable statements.**

### Members declared on `KeyboardHandler` but deliberately NOT on this interface

For completeness of the contract picture (all verified against `QuickFiler/Controllers/KeyboardHandler.cs`):

- `ClearFilter()` (`KeyboardHandler.cs:81`) — public on the class, absent from the interface, **no caller anywhere in the repo**.
- `KeyboardHandler_PreviewKeyDown(object, PreviewKeyDownEventArgs)` — the non-async variant (`KeyboardHandler.cs:96`), absent from the interface, **no caller anywhere**.
- `KeyDownTaskAsync(object, KeyEventArgs)` (`KeyboardHandler.cs:150`) — public `Task`-returning core, absent from the interface.
- `DdOpen_KeyDownAsync(ComboBox, KeyEventArgs)` / `DdClosed_KeyDownAsync(ComboBox, KeyEventArgs)` (`KeyboardHandler.cs:317, 391`) — public, absent from the interface.
- `GetItemViewer(Control)` (`KeyboardHandler.cs:247`) — `internal`, **no caller anywhere**.

This asymmetry is load-bearing for F3's test plan: the four public non-interface members (`KeyDownTaskAsync`, `DdOpen_KeyDownAsync`, `DdClosed_KeyDownAsync`, `KeyboardHandler_PreviewKeyDown`) can be invoked directly on the concrete type in tests **without any interface change**, which is precisely why the seam plan in `01-KeyboardHandler.md` stays additive.

---

## 3. Existing Test Coverage (static analysis)

**Not applicable in the usual sense: there is nothing executable to cover.** No test method can execute a line of this file, because no line of this file compiles to IL with a sequence point.

What does exist is extensive *contract* exercise through Moq proxies. Recording it here so the F1 ledger and the F16 capstone do not mistake "0% measured" for "untested contract":

| Member | Mocked / exercised by (representative) |
| --- | --- |
| `KbdActive` | `QuickFiler.Test/Controllers/QfcCollectionControllerTests.cs:348` (Loose mock), `QfcCollectionControllerDarkModeTests.cs:41` |
| `ToggleKeyboardDialog()` | `QfcItemController.NavigationTests.cs:62, 84, 107, 130, 178, 197` (`Verify(..., Times.Once/Never)`) |
| `ToggleKeyboardDialogAsync()` | `QfcItemController.NavigationTests.cs:226`, `QfcItemController.SeamDispatcherTests.cs:88, 93` |
| `KeyboardHandler_PreviewKeyDownAsync` | `QfcItemController.EventWiringTests.cs:291, 355` |
| `BreadcrumbArrowFallThrough` | `QfcItemControllerBreadcrumbDropDownTests.cs:161–164, 183` (Strict mock + `VerifyAll`) |
| `CharActions` / `CharActionsAsync` | exercised indirectly through the production registration paths in `EfcFormController.cs:926–951` and `EfcItemController.cs:688–734, 879–903`; the mock surface appears in `EfcHomeControllerLifecycleTests.cs:179, 301` |
| The type as a whole | `QfcHomeControllerTests.cs:129, 200`, `QfcHomeControllerPropertyTests.cs:164`, `QfcFormControllerSeamTests.cs:115`, `EfcHomeControllerDependenciesTests.cs:42, 63, 188`, `EfcHomeControllerDependenciesProductionFactoryTests.cs:400, 437, 464`, `EfcHomeControllerSeamTests.cs:230`, `QfcItemController.{Initialization,EventWiring,FocusAndTheme,SeamFactory,SeamDispatcher}Tests.cs`, `QfcItemControllerTests.cs:202, 225, 230` |
| `ToggleKeyboardDialog(object,e)`, `KeyboardHandler_KeyDown`, `KeyboardHandler_KeyDownAsync`, `KeyActions`, `KeyActionsAsync`, `AlwaysOnKeyActionsAsync`, `StringActionsAsync`, `CboFolders_KeyDownAsync` | no direct Moq `Setup`/`Verify` found; reachable only through the concrete implementation |

**Static-analysis conclusion: 0 executable lines, therefore 0 coverage gaps in this file.** The behavioral coverage obligation for every member above belongs to `KeyboardHandler.cs` and is discharged by the 73 test cases enumerated in `01-KeyboardHandler.md` §9.

---

## 4. Coverage Gaps

**None.**

`.claude/rules/general-unit-test.md` § Coverage Requirements states verbatim: *"Type-only / interface-only modules with no executable behavior may be omitted from coverage measurement. Examples: ... and C# interface-only files. Such modules legitimately report 0% executable coverage and may be excluded from measurement. This is a clarification only; it does not lower any coverage threshold."*

`docs/features/epics/quickfiler-per-file-coverage/epic.md:112` independently accounts for this: *"~24 are interface-only declarations with no executable behavior."* `IQfcKeyboardHandler.cs` is one of those 24.

Two consequences the planner must respect:

1. **Do not add `[ExcludeFromCodeCoverage]` to this file.** It is not needed (there is nothing to exclude), and adding it would create exactly the kind of unratified attribute the epic is removing. `.claude/rules/general-unit-test.md` § Coverage Exclusion Policy also forbids `exclude` entries for production paths; the interface-only clarification is the correct mechanism, not an attribute.
2. **Do not write a "test" that instantiates a Moq proxy of the interface and asserts nothing**, purely to produce a coverage artifact. That would be a test with no behavior under assertion and would violate the General Unit Test Policy's isolation and intent requirements.

---

## 5. Seam Requirements

**None required. This file is the seam.**

`IQfcKeyboardHandler` is already the level-1 interface seam (per `.claude/rules/csharp.md` § DI Seams) that every consumer in QuickFiler binds to instead of the concrete `KeyboardHandler`. It is the reason 17 test files across five sibling children can test their own controllers without a live keyboard handler. Introducing a further seam here would be the "heavy generic abstraction without need" that `.claude/rules/csharp.md` § Prohibited Behaviors rules out.

### Recommended F1 ledger classification

`interface-only` — no executable behavior, outside the coverage denominator by `.claude/rules/general-unit-test.md` § Coverage Requirements. Explicitly **not** `ratified-exempt` (there is nothing to exempt) and **not** `testable` (there is nothing to test). If F1's ledger schema supports only two states, request the addition of this third state; forcing `IQfcKeyboardHandler.cs` into `ratified-exempt` would misrepresent 24 files across the epic and would make the F16 capstone's exemption count meaningless.

### Additive changes this child WILL make in the vicinity (none of them to this file)

Per `01-KeyboardHandler.md` §5, F3 adds two **new** files under `QuickFiler/Interfaces/`:

- `QuickFiler/Interfaces/IQfcDialogPrompt.cs` — interface-only, same ledger classification requested.
- `QuickFiler/Interfaces/MyBoxDialogPrompt.cs` — a 1:1 adapter over the static `MyBox.ShowDialog`; ledger ratification requested for its single forwarding statement.

Neither touches `IQfcKeyboardHandler.cs`.

---

## 6. Cross-Child Contract Impact

### 6.1 Determination

**This file is FROZEN for the duration of F3. No member is added, removed, renamed, or re-typed. The change is ADDITIVE by construction — because the file is not changed at all** (the sole exception under consideration is the unused-`using` removal in §7 R-1, which alters no signature).

This is the load-bearing cross-child guarantee of child F3. `IQfcKeyboardHandler` is consumed by **five sibling children** (F6, F8, F9, F10, F11) plus F7 and F15, in 20 production locations and 17 test files. Any signature change would force edits into sibling-owned files, which the delegation mandate prohibits.

### 6.2 Compile-time consumers (exhaustive)

Verified by repo-wide grep for `IQfcKeyboardHandler` across all `*.cs`.

**Interface declarations that embed the type:**

| File : line | Owning child | Usage |
| --- | --- | --- |
| `QuickFiler/Interfaces/IFilerHomeController.cs:32` | F3 (this cluster's `IFilerHomeController` is not in F3's list; see §7 R-3) | `IQfcKeyboardHandler KeyboardHandler { get; set; }` |
| `QuickFiler/Interfaces/IQfcHomeController.cs:10` | F7 | `IQfcKeyboardHandler KbdHndlr { get; set; }` |
| `QuickFiler/Interfaces/IQfcFormViewer.cs:21` | F6 | `void SetKeyboardHandler(IQfcKeyboardHandler keyboardHandler);` |

**Production implementations / fields / properties:**

| File : line | Owning child | Usage |
| --- | --- | --- |
| `QuickFiler/Controllers/KeyboardHandler.cs:23` | **F3** | the sole implementer |
| `QuickFiler/Controllers/QfcHomeController.cs:187, 421–422` | F7 | loader `Func<>` return type, backing field, public property |
| `QuickFiler/Controllers/QfcItemController.cs:49` | F10 | `private IQfcKeyboardHandler _kbdHandler;` |
| `QuickFiler/Controllers/QfcCollectionController.cs:75` | F11 | `private IQfcKeyboardHandler _kbdHandler;` |
| `QuickFiler/Controllers/EfcHomeController.cs:369–370` | F8 | field + property |
| `QuickFiler/Controllers/EfcItemController.cs:374` | F9 | field |
| `QuickFiler/Controllers/EfcHomeControllerDependencies.cs:51, 103, 175, 187, 190` | F8 | factory delegate type, property, two factory methods |
| `QuickFiler/Controllers/EfcHomeControllerDependencyFactories.cs:45, 51, 141, 201` | F8 | delegate types + two production factory methods |
| `QuickFiler/Viewers/QfcFormViewer.cs:32, 51` | F15 | field + `SetKeyboardHandler` override point (`virtual`) |
| `QuickFiler/Viewers/QfcFormViewerExpanded.cs:29, 36` | **unassigned** (§7 R-3) | field + `SetKeyboardHandler` |
| `QuickFiler/Viewers/QfcFormViewerDark.cs:29, 36` | **unassigned** (§7 R-3) | field + `SetKeyboardHandler` |
| `QuickFiler/Viewers/EfcViewer.cs:55, 56, 61` | F9 | field, internal `KeyboardHandler` getter, `SetKeyboardHandler` |
| `QuickFiler/Viewers/EfcViewer3.cs:44, 46` | **unassigned / dead type** (§7 R-3) | field + `SetKeyboardHandler` |

**Test-assembly consumers (Moq targets — all break on any signature change):**

`QuickFiler.Test/Controllers/` — `EfcHomeControllerLifecycleTests.cs:179, 301`; `EfcHomeControllerDependenciesTests.cs:42, 63, 188`; `EfcHomeControllerDependenciesProductionFactoryTests.cs:400, 437, 464`; `EfcHomeControllerSeamTests.cs:230`; `QfcHomeControllerPropertyTests.cs:164`; `QfcHomeControllerTests.cs:129, 200`; `QfcFormControllerSeamTests.cs:115`; `QfcCollectionControllerTests.cs:332, 348`; `QfcCollectionControllerDarkModeTests.cs:41`; `QfcItemController.NavigationTests.cs:35, 37, 164, 187, 217`; `QfcItemController.InitializationTests.cs:25, 30, 45, 78, 113, 146`; `QfcItemControllerTests.cs:202, 225, 230`; `QfcItemControllerBreadcrumbDropDownTests.cs:161`; `QfcItemController.FocusAndThemeTests.cs:58, 67`; `QfcItemController.EventWiringTests.cs:28, 42, 47, 128, 133, 238, 329`; `QfcItemController.SeamFactoryTests.cs:242`; `QfcItemController.SeamDispatcherTests.cs:87, 204, 208`.

**17 distinct test files.** Adding a member to the interface would not break Loose mocks but **would** break every `MockBehavior.Strict` usage that then encounters an unconfigured call — specifically `EfcHomeControllerDependenciesTests.cs:42, 188`, `EfcHomeControllerDependenciesProductionFactoryTests.cs:400`, and `QfcItemControllerBreadcrumbDropDownTests.cs:161`. That is a further, concrete reason to add nothing.

### 6.3 Changes explicitly REJECTED

| Change | Determination | Minimum breaking delta if ever pursued |
| --- | --- | --- |
| Widen `BreadcrumbArrowFallThrough(ItemViewer, ...)` to `IItemViewer` (line 33) — removing the only concrete-WinForms coupling from the interface | **BREAKING** under the epic's additive mandate, even though it is source-compatible for all 2 in-repo callers (`QfcItemController.ViewerSetup.cs:187` passes a concrete `ItemViewer`; the Moq `Setup` at `QfcItemControllerBreadcrumbDropDownTests.cs:163` still compiles). **Not needed** — `01-KeyboardHandler.md` §5.3 proves the concrete `ItemViewer` is testable headlessly. | 1 line here (33) + 1 line at `KeyboardHandler.cs:293`, then re-verify the F10 Strict-mock setup. Would belong to a dedicated cross-cutting issue, not to F3. |
| Add `Task KeyDownTaskAsync(object, KeyEventArgs)` to the interface so consumers can await instead of firing `async void` | **BREAKING** — adds a member that all four Strict mocks would need configured; touches F8/F10 test files. | 1 added line here + Strict-mock setups in 4 sibling test files. Reject. |
| Split the interface into `IQfcKeyboardActions` (the six `KbdActions<>` properties) and `IQfcKeyboardEvents` (the eight handlers) | **BREAKING** — changes 20 production and 17 test binding sites. | Full re-typing of every field/property listed in §6.2. Reject. |
| Remove `KeyboardHandler_KeyDown` (line 17), whose only production wiring is commented out at `EfcItemController.cs:651` | **BREAKING** (member removal). | 1 removed line here + 1 removed method at `KeyboardHandler.cs:114–131`. Promote as a follow-up issue; do not do it in F3. |
| Remove the unused `using System.Collections.Generic;` (line 2) | **ADDITIVE / non-breaking** — no signature changes, no consumer effect. See §7 R-1. | n/a |

---

## 7. Proposed Test Cases

**None.**

This is a deliberate, evidence-based determination, not an omission:

1. The file contains zero executable statements (§2), so no test can raise its measured coverage above 0.
2. `.claude/rules/general-unit-test.md` § Coverage Requirements explicitly permits omitting interface-only modules from measurement.
3. Every *behavior* declared by this interface is implemented by exactly one type, `KeyboardHandler`, and every one of those behaviors is covered by the 73 discrete test cases enumerated in `01-KeyboardHandler.md` §9. Writing separate tests "for the interface" would duplicate those and violate the Core Principles requirement that a unit test target a single unit of behavior.

### One optional non-test verification the planner may consider

An architecture-style assertion — "`KeyboardHandler` is the only type in the `QuickFiler` assembly implementing `IQfcKeyboardHandler`" — would pin the single-implementer assumption on which §6's freeze argument rests. The repository has no `NetArchTest.Rules` dependency wired into `QuickFiler.Test` (the seven-stage loop in `.claude/rules/general-code-change.md` names architecture-boundary tests, but `CLAUDE.md`'s four-stage C# toolchain does not). **Recommendation: do not add a new test dependency for this.** If the planner wants the guarantee cheaply, a single reflection-based `[TestMethod]` in `KeyboardHandler.ConstructionTests.cs` asserting `typeof(IQfcKeyboardHandler).Assembly.GetTypes().Where(t => typeof(IQfcKeyboardHandler).IsAssignableFrom(t) && !t.IsInterface).Should().ContainSingle()` achieves it with no new package. Treat this as optional; it is a design invariant, not a coverage requirement.

---

## 8. Risks and Open Questions

| # | Risk / question | Assessment | Proposed handling |
| --- | --- | --- | --- |
| R-1 | **Unused `using System.Collections.Generic;` at line 2.** No `Dictionary`/`List`/`IEnumerable` appears in any signature in the file. | Low. Removal is non-breaking (§6.3) and would satisfy IDE0005 under the analyzer build. | Remove it, in the same commit as the three unused-`using` removals in `KeyboardHandler.cs` (see `01-KeyboardHandler.md` §10 R-7). Verify with `msbuild ... /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`. If IDE0005 is not enabled at warning level in `.editorconfig`, the change is cosmetic and may be dropped to keep the diff minimal — the planner should decide once and record it. |
| R-2 | **`void ToggleKeyboardDialogAsync(object sender, KeyEventArgs e)` (line 15) returns `void` despite the `Async` suffix**, and `KeyboardHandler_KeyDownAsync` (line 18) and `CboFolders_KeyDownAsync` (line 28) do the same. These are the `async void` event-handler shapes required by `KeyEventHandler` conversion at `QfcItemController.EventWiring.cs:41, 82` and `QfcFormController.SetupDisposal.cs:160, 188`. | Low as a defect (it is intentional WinForms wiring), medium as a determinism hazard for testing the implementations. | Do **not** change the signatures — they are required by the delegate conversions and by four sibling-owned call sites. The testing answer is the `InlineSynchronizationContext` technique in `01-KeyboardHandler.md` §9 File F, which makes the `async void` continuation run synchronously with no `Thread.Sleep`/`Task.Delay`. |
| R-3 | **`QfcFormViewerExpanded.cs`, `QfcFormViewerDark.cs`, and `EfcViewer3.cs` consume this interface but appear in no child's file assignment** in `epic.md`'s Feature File Assignments. Separately, `QuickFiler/Interfaces/IFilerHomeController.cs` and `QuickFiler/Interfaces/IItemControler.cs` are listed under F3 while `IFilerFormController.cs` and `IQfcFormViewer.cs` are listed under F6 — so this interface's transitive contract surface is split across two children. | Low for F3 (nothing to edit), but a real gap in the epic's "every one of the 121 compiled files is assigned to exactly one child" claim. | Report to the epic orchestrator and to the F16 capstone. Confirm against `QuickFiler/QuickFiler.csproj` whether the three viewer files are `<Compile Include>`d; if they are, the assignment table needs correcting. **Out of scope for F3 to fix.** |
| R-4 | **Concrete WinForms type `ItemViewer` in a public interface signature (line 33).** This couples the keyboard contract to an F14-owned `UserControl` and works against the long-term VSTO-exit direction recorded in `epic.md` § Non-Goals ("prefer host-neutral extraction that a future WebView2/Office.js port can reuse"). | Low near-term, medium long-term. | Record as a design observation and promote a follow-up issue proposing the `IItemViewer` widening (§6.3 gives the exact 2-line delta). **Do not perform it in F3** — the additive mandate binds. |
| R-5 | **F1's ledger schema may offer only `testable` / `ratified-exempt`,** with no `interface-only` state. Forcing this file into `ratified-exempt` would inflate the epic's exemption count by ~24 files and would make the F16 acceptance criterion "the count of QuickFiler files carrying `[ExcludeFromCodeCoverage]` on a testable seam falls to zero" harder to interpret. | Medium. | Raise with F1 before F3 executes. If the schema cannot be extended, request that `IQfcKeyboardHandler.cs` be recorded as `ratified-exempt` with rationale `interface-only-no-executable-behavior`, so the reason is at least machine-greppable and distinguishable from genuine host-bound exemptions. |
| R-6 | **Rebase collisions.** Two features are in flight on `main` (#400 `quickfiler-folder-selector-dropdown`, #424 `quickfiler-high-confidence-queue-init-stall`). #400 territory includes the breadcrumb path that produced lines 30–35 of this file. | Low — F3 does not modify this file, so a conflict is structurally impossible. | None required. |

---

## 9. Sources

All paths relative to `C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-aafcc2531072ca96b\`.

**Policy**
- `CLAUDE.md` — § UT2 (coverage exemption and testable denominator), § CUT1–CUT2
- `.claude/rules/general-unit-test.md` — § Coverage Requirements (interface-only clarification), § Coverage Exclusion Policy, § Core Principles
- `.claude/rules/csharp.md:47–63` (DI seam hierarchy), `:89–96` (Prohibited Behaviors)

**Feature / epic**
- `docs/features/epics/quickfiler-per-file-coverage/epic.md:108–121` (scope breakdown: "~24 are interface-only declarations with no executable behavior"), `:132–192` (Shared Design), `:267–275` (F3 assignment), `:242–246` (assignment preamble)
- `docs/features/active/2026-08-07-quickfiler-keyboard-actions-coverage-430/issue.md`

**File under research**
- `QuickFiler/Interfaces/IQfcKeyboardHandler.cs:1–37` (read in full)
- `QuickFiler/QuickFiler.csproj:366`

**Implementer and non-interface members**
- `QuickFiler/Controllers/KeyboardHandler.cs:23, 81, 96, 150, 247, 317, 391` (sole implementer; members deliberately off-interface)

**Consumers (production)**
- `QuickFiler/Interfaces/IFilerHomeController.cs:32`
- `QuickFiler/Interfaces/IQfcHomeController.cs:10`
- `QuickFiler/Interfaces/IQfcFormViewer.cs:21`
- `QuickFiler/Controllers/QfcHomeController.cs:187, 421–422`
- `QuickFiler/Controllers/QfcItemController.cs:49`
- `QuickFiler/Controllers/QfcCollectionController.cs:75`
- `QuickFiler/Controllers/EfcHomeController.cs:369–370`
- `QuickFiler/Controllers/EfcItemController.cs:374, 651`
- `QuickFiler/Controllers/EfcHomeControllerDependencies.cs:51, 103, 175, 187, 190`
- `QuickFiler/Controllers/EfcHomeControllerDependencyFactories.cs:45, 51, 141, 201`
- `QuickFiler/Controllers/QfcItemController.EventWiring.cs:41, 82`
- `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs:187`
- `QuickFiler/Controllers/QfcFormController.SetupDisposal.cs:160, 188`
- `QuickFiler/Viewers/QfcFormViewer.cs:32, 51`
- `QuickFiler/Viewers/QfcFormViewerExpanded.cs:29, 36`
- `QuickFiler/Viewers/QfcFormViewerDark.cs:29, 36`
- `QuickFiler/Viewers/EfcViewer.cs:55, 56, 61`
- `QuickFiler/Viewers/EfcViewer3.cs:44, 46`

**Consumers (tests — Moq targets, 17 files)**
- `QuickFiler.Test/Controllers/` — `EfcHomeControllerLifecycleTests.cs:179, 301`; `EfcHomeControllerDependenciesTests.cs:42, 63, 188`; `EfcHomeControllerDependenciesProductionFactoryTests.cs:400, 437, 464`; `EfcHomeControllerSeamTests.cs:230`; `QfcHomeControllerPropertyTests.cs:164`; `QfcHomeControllerTests.cs:129, 200`; `QfcFormControllerSeamTests.cs:115`; `QfcCollectionControllerTests.cs:332, 348`; `QfcCollectionControllerDarkModeTests.cs:41`; `QfcItemController.NavigationTests.cs:35, 37, 164, 187, 217`; `QfcItemController.InitializationTests.cs:25, 30, 45, 78, 113, 146`; `QfcItemControllerTests.cs:202, 225, 230`; `QfcItemControllerBreadcrumbDropDownTests.cs:161`; `QfcItemController.FocusAndThemeTests.cs:58, 67`; `QfcItemController.EventWiringTests.cs:28, 42, 47, 128, 133, 238, 329`; `QfcItemController.SeamFactoryTests.cs:242`; `QfcItemController.SeamDispatcherTests.cs:87, 204, 208`

**Related research**
- `docs/features/active/2026-08-07-quickfiler-keyboard-actions-coverage-430/research/01-KeyboardHandler.md` (this cluster's implementer analysis, seam plan, and 73 test cases)

**Tooling**
- `scripts/vscode/Invoke-MSTestWithCoverage.ps1` (F1 harness input)
- `TaskMaster.runsettings:1–30`
