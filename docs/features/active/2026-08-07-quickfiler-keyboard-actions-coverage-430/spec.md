# quickfiler-keyboard-actions-coverage — Spec

- **Issue:** #430
- **Parent epic issue:** [#136](https://github.com/drmoisan/TaskMaster/issues/136)
- **Epic:** `quickfiler-per-file-coverage` (child F3, wave 1)
- **Epic manifest:** `docs/features/epics/quickfiler-per-file-coverage/epic.md`
- **Integration branch:** `epic/quickfiler-per-file-coverage-integration`
- **Branch:** `feature/quickfiler-keyboard-actions-coverage`
- **Depends on:** F1 `quickfiler-coverage-denominator-and-exemption-ledger` (wave 0)
- **Owner:** drmoisan
- **Last Updated:** 2026-08-07T22-30
- **Status:** Specified
- **Version:** 1.0
- **Work Mode:** full-feature (AC sources: `spec.md` **and** `user-story.md`)

## Overview

Epic #136 requires every production `.cs` file compiled by `QuickFiler/QuickFiler.csproj` to reach at
least 80% line coverage or to sit on an explicitly ratified exemption ledger. This child owns the
QuickFiler keyboard-handling and mail-item-action cluster: 11 compiled files totalling roughly 1,025
lines.

The cluster's central file, `QuickFiler/Controllers/KeyboardHandler.cs` (414 lines), carries
`[ExcludeFromCodeCoverage]` at line 22 and has no tests at all. Per the epic's ratified policy
reconciliation (`epic.md` Shared Design §1, lines 134–154), the `CLAUDE.md` § UT2 COM/VSTO exemption
qualifier "without an injectable seam" is a live obligation rather than a standing permission: an
`[ExcludeFromCodeCoverage]` attribute on a *testable* seam is a Blocking finding. Research artifact
`research/01-KeyboardHandler.md` establishes that the attribute is not justified and that every
host-bound dependency in the file except one constructor overload is reachable behind an interface
seam, an existing repository seam, or a narrow injectable delegate.

This specification is written after the eleven per-file research artifacts in
`docs/features/active/2026-08-07-quickfiler-keyboard-actions-coverage-430/research/`. Those
artifacts are the authoritative evidence base. Where `issue.md` (written before research) conflicts
with them, research wins; every such divergence is recorded in the Correction Log below so the
change is auditable.

## Correction Log — divergences from the `issue.md` draft

### C1 — `KaStringAsync` does NOT require a fake timer or an injected clock

`issue.md` lines 73–74 state: *"`Thread.Sleep`, `Task.Delay`, and real wall-clock waits are
prohibited in tests; `KaStringAsync` requires a fake-timer or injected-clock approach."* The first
clause stands. The second clause is not supported by the code.

`research/07-KaStringAsync.md` §1.1 verified all 95 lines of `QuickFiler/Controllers/KaStringAsync.cs`
and found **zero** occurrences of `async`, `await`, `Task.Delay`, `Thread.Sleep`, any timer type,
`DateTime`, `DateTimeOffset`, `TimeProvider`, `Stopwatch`, `Random`, `SynchronizationContext`,
`ConfigureAwait`, or `Task.Run`. The `Async` suffix names only the shape of the stored delegate
(`Func<string, Task>`, line 44); the type stores and returns that delegate and never invokes,
awaits, schedules, or times anything. `KeyEquals` (lines 57–79), the only method with logic, is
entirely synchronous. The asynchrony lives in the caller, `KeyboardHandler.KeyDownTaskAsync`
(`KeyboardHandler.cs:150–204`), which performs the single `await` at line 194.

`research/07-KaStringAsync.md` §1.2 additionally audited the existing
`QuickFiler.Test/Controllers/KaStringAsyncTests.cs` (lines 1–168) against
`.claude/rules/general-unit-test.md` § Determinism Infrastructure and found no `Thread.Sleep`, no
`Task.Delay`, no `.Wait()`/`.Result`/`GetAwaiter().GetResult()`, no `DateTime.Now`/`UtcNow`, and no
`Stopwatch`. **There is no policy defect in the existing suite and nothing to remediate on the
determinism axis.**

**Correction:** no `TimeProvider`, no `FakeTimeProvider`, no fake-timer facility, and no injected
clock is introduced anywhere in this child. Introducing one would add a sixth parameter to a
five-parameter constructor consumed by F11 (`QfcCollectionController.cs:1376–1383`) for no
testability benefit, and would be the "heavy generic abstraction without need" that
`.claude/rules/csharp.md` § Prohibited Behaviors rules out. The determinism obligation is discharged
by `Task.CompletedTask` / `Task.FromException` (which complete synchronously) and, for the
`async void` members of `KeyboardHandler`, by the `InlineSynchronizationContext` precedent at
`QuickFiler.Test/Viewers/BreadcrumbPendingOpenCloseTests.cs:375–378`.

### C2 — `.claude/rules/csharp.md` does not name `KbdActions<>`; only `CLAUDE.md` § UT2 does

`issue.md` lines 31–33 attribute the `KbdActions<>` non-exemption clause to both
`.claude/rules/csharp.md` and `CLAUDE.md` § UT2. `research/04-KbdActions.md` §1.1 verified by search
across all `*.md` that only `CLAUDE.md` § UT2 names the type, in the sentence: *"Testable seams
within otherwise-COM-bound assemblies (e.g., `ToDoLoader`, `IDList` arithmetic, `KbdActions<>`,
path/settings helpers) are explicitly NOT exempt and must meet the `>= 80%` floor."*
`.claude/rules/csharp.md` contains no occurrence of `KbdActions`; it supplies the general `>= 80%`
floor (line 39), the `>= 90%` new-code floor (line 40), the changed-line regression rule (line 41),
and the seam hierarchy (lines 49–53).

**Correction:** the obligation is unchanged and remains binding — `KbdActions.cs` cannot be placed on
the exemption ledger. The citation is singular: `CLAUDE.md` § UT2 only.

### C3 — There is no epic file-assignment gap; the three flagged viewer files are not compiled

`research/01-KeyboardHandler.md` §10 R-4, `research/02-IQfcKeyboardHandler.md` §8 R-3, and
`research/03-QfcFormKeyHandler.md` §8 R-2 each flag `QuickFiler/Viewers/QfcFormViewerExpanded.cs`,
`QuickFiler/Viewers/QfcFormViewerDark.cs`, and `QuickFiler/Viewers/EfcViewer3.cs` as consuming
`IQfcKeyboardHandler` and `QfcFormKeyHandler` while appearing in no child's assignment in
`epic.md` § Feature File Assignments, and recommend verification against the project file.

**Resolution (verified by the epic orchestrator):** none of the three appears in any of the 121
`<Compile Include>` entries in `QuickFiler/QuickFiler.csproj`. They are not compiled. Like
`QuickFiler/Legacy/**` and `QuickFiler/Notes/**` (`epic.md:108–110`), they are correctly outside the
coverage denominator and outside the epic. The epic's claim that every one of the 121 compiled files
is assigned to exactly one child is intact; there is no assignment gap to correct.

**Consequence for the divergence those artifacts report.** `research/03-QfcFormKeyHandler.md` §8 R-3
records that call sites S2 (`QfcFormViewerExpanded.cs:41–53`) and S3 (`QfcFormViewerDark.cs:41–53`)
invoke `_keyboardHandler.KeyboardHandler_KeyDown(sender, e)` **without** the null guard that S1
(`QfcFormViewer.cs:56–73`) applies, and would therefore carry a latent `NullReferenceException` if
ALT were pressed before `SetKeyboardHandler` had been called. Because those two files are not
compiled, that divergence exists only in non-compiled code and **is not a live defect**. It is
recorded here for completeness and requires no action from any child.

## Scope — 11 compiled production files, ~1,025 lines

Per `epic.md:267–275` (F3 assignment). `[X]` marks a file currently carrying
`[ExcludeFromCodeCoverage]`.

| # | File | Lines | Attribute | Research artifact |
| --- | --- | --- | --- | --- |
| 1 | `QuickFiler/Controllers/KeyboardHandler.cs` | 414 | `[X]` line 22 | `01-KeyboardHandler.md` |
| 2 | `QuickFiler/Interfaces/IQfcKeyboardHandler.cs` | 37 | — | `02-IQfcKeyboardHandler.md` |
| 3 | `QuickFiler/Controllers/QfcFormKeyHandler.cs` | 20 | — | `03-QfcFormKeyHandler.md` |
| 4 | `QuickFiler/Controllers/KbdActions.cs` | 146 | — | `04-KbdActions.md` |
| 5 | `QuickFiler/Controllers/KaChar.cs` | 99 | — | `05-KaChar.md` |
| 6 | `QuickFiler/Controllers/KaKey.cs` | 99 | — | `06-KaKey.md` |
| 7 | `QuickFiler/Controllers/KaStringAsync.cs` | 95 | — | `07-KaStringAsync.md` |
| 8 | `QuickFiler/Interfaces/IKbdAction.cs` | 18 | — | `08-IKbdAction.md` |
| 9 | `QuickFiler/Interfaces/IMailItemActions.cs` | 35 | — | `09-IMailItemActions.md` |
| 10 | `QuickFiler/Interfaces/MailItemActionsAdapter.cs` | 47 | — | `10-MailItemActionsAdapter.md` |
| 11 | `QuickFiler/Interfaces/IItemControler.cs` | 15 | — | `11-IItemControler.md` |

## Current per-file status

These figures are **static analysis**, established by reading each file and mapping every member and
branch to the existing test method that reaches it. Numeric per-file measurement happens at
execution time with F1's per-file coverage harness (`epic.md` Shared Design §6), derived from the
Cobertura output of `scripts/vscode/Invoke-MSTestWithCoverage.ps1`.

| File | Static status | Genuine gap profile | Proposed new cases |
| --- | --- | --- | --- |
| `KeyboardHandler.cs` | **0%, zero tests.** No test anywhere constructs the type; all 17 referencing test files use `Mock<IQfcKeyboardHandler>`, which contributes zero coverage. | Every member. Highest weight: `KeyDownTaskAsync` (150–204, 55 lines, 12 decision points), `DdOpen_KeyDownAsync` (317–389), `BreadcrumbArrowFallThrough` (292–315). ~120 lines and 22 decision points are reachable today with no production change at all. | 73 |
| `QfcFormKeyHandler.cs` | **100% line, 100% branch.** One executable statement, four existing tests. | No line gap. Two scenario-completeness gaps; the highest-value is `Keys.Menu` (`0x12`, the ALT key code) versus `Keys.Alt` (`0x40000`, the modifier flag) — `Keys.Menu.HasFlag(Keys.Alt)` is `false` and nothing currently documents which shape the predicate is meant to catch. | 8 |
| `MailItemActionsAdapter.cs` | **100% line, 100% branch (measured).** All 12 statement lines at `hits="1"` in `docs/features/active/2026-08-06-quickfiler-high-confidence-queue-init-stall-424/evidence/qa-gates/coverage-final.cobertura.xml:14448–14513`. | No line or branch gap. 7 scenario gaps (G1–G7): unguarded null constructor argument, null returns from `Reply`/`ReplyAll`/`Forward`, exception propagation, the unasserted `Display` `Modal` argument, half-covered `UnRead` values, null `EntryID`, and no `IMailItemActions` contract assertion. | 8 recommended (T1–T8), 4 optional (T9–T12) |
| `KbdActions.cs` | High but incomplete. Unexecuted lines: 28 (`IEnumerable` constructor) and 139 (explicit `IEnumerable.GetEnumerator`). | Branch gaps: indexer-set no-match silent no-op, indexer-get `NullReferenceException`, `Remove` under a different `SourceId`, empty `FilterKeys` result, `Add(UClass)` with a null instance. Only 2 of 5 closed generic instantiations are exercised. | 13 |
| `KaChar.cs` | High but incomplete. Unexecuted lines: 45, 52–53, 60, 94–95. | Orphaned `DelegateType` and `Update` members, `KaCharAsync()` parameterless constructor, post-construction setters, `char` boundary values, delegate error propagation. | 13 |
| `KaKey.cs` | High but incomplete. Unexecuted lines: 45, 52–53, 60, 94–95. `KaKeyAsync` has no direct coverage and, unlike `KaKey`, receives no indirect coverage through `KbdActionsRemainingBranchesTests.cs`. | Same orphan profile as `KaChar.cs`, plus the `[Flags]` `Keys` contract (a modifier-combined registration can never match through `KeyboardHandler`, which looks up with `e.KeyCode`). | 14 |
| `KaStringAsync.cs` | **Every executable line already reached.** | Branch and contract gaps only: the `KeyEquals("")` `Substring(-1, 1)` path, `KeyEquals(null)`, the `Update` gate asymmetry between line 61 and line 72, the `Activated = false` reset at line 77, `Substring` boundary arithmetic, ordinal case sensitivity, and delegate fault propagation. | 15 |
| `IKbdAction.cs`, `IQfcKeyboardHandler.cs`, `IMailItemActions.cs`, `IItemControler.cs` | **Zero executable IL.** Verified per file by exhaustive construct check (no default interface member, no static member, no static constructor, no constant initializer, no attribute constructor, no nested type, no auto-property initializer, no event accessor, no operator). | None. There is nothing to cover. | **0 by design** |

**Total proposed new test cases: approximately 128** — 73 for `KeyboardHandler.cs`, 55 across the
`KbdActions`/`Ka*` cluster (13 + 13 + 14 + 15), plus the `QfcFormKeyHandler` hardening set (8) and
the `MailItemActionsAdapter` hardening set (8 recommended). Each case is individually nameable and
becomes its own atomic plan task per the epic's per-file mandate.

Two of those figures deserve explicit framing so that a reviewer does not read effort as waste:
`QfcFormKeyHandler.cs` and `MailItemActionsAdapter.cs` are already at 100% line coverage, and
`KaStringAsync.cs` has no unexecuted line. Their new cases discharge the **boundary,
invalid-input, and error-handling** limbs of the acceptance criteria and the branch-coverage limb;
they are not expected to move the line-coverage number.

## Behavior

- Establish current per-file line coverage for all 11 in-scope files using F1's per-file coverage
  harness and target the genuine gaps rather than duplicating the existing test files
  (`KaCharTests.cs`, `KaKeyTests.cs`, `KaStringAsyncTests.cs`, `KbdActionsTests.cs`,
  `KbdActionsRemainingBranchesTests.cs`, `QfcFormKeyHandlerTests.cs`, `MailItemActionsAdapterTests.cs`).
- Extract seams K1–K5 from `KeyboardHandler.cs` following the epic seam hierarchy (interface seam >
  injectable delegate > adapter), remove its `[ExcludeFromCodeCoverage]` attribute, and cover the
  file.
- Add MSTest/Moq/FluentAssertions unit tests in `QuickFiler.Test/`, covering the positive path plus
  invalid-input, boundary, and error-handling behavior per file.
- Record numeric per-file coverage evidence under `<FEATURE>/evidence/qa-gates/`.
- Change no observable QuickFiler keyboard behavior.

## Seam design — `KeyboardHandler.cs`

`research/01-KeyboardHandler.md` §5 establishes five items, applied in strict hierarchy order per
`.claude/rules/csharp.md` § DI Seams (lines 49–53) and `epic.md` Shared Design §2.

### The exemption is not justified

`KeyboardHandler.cs` declares `using Microsoft.Office.Interop.Outlook;` at line 15, but **no member
in the file references any Outlook Interop type**. Lines 12 (`System.Web.UI.WebControls`) and 14
(`System.Windows.Input`) are likewise unused. Line 15 in particular makes the file *appear*
Outlook-Interop-bound, which is the most plausible reason it acquired `[ExcludeFromCodeCoverage]`.
Removing the three unused directives, verified by the analyzer build, is the concrete evidence that
the exemption was never warranted.

### K1 — `IQfcDialogPrompt` (hierarchy level 1: interface seam; level-3 adapter as its production implementation)

Removes `MyBox.ShowDialog(...)` at lines 304–309 and 350–355.

- New file `QuickFiler/Interfaces/IQfcDialogPrompt.cs` — one member:
  `DialogResult ShowActionDialog(string message, string title, BoxIcon icon, Dictionary<string, Action> actions);`
- New file `QuickFiler/Interfaces/MyBoxDialogPrompt.cs` — `sealed class MyBoxDialogPrompt :
  IQfcDialogPrompt`, a 1:1 forward to `MyBox.ShowDialog`.

**K1 is mandatory, not optional.** `UtilitiesCS/Dialogs/MyBox.cs:41–45` does expose a replaceable
`internal static Func<MyBoxViewer, DialogResult> DialogInvoker` seam, but
`UtilitiesCS/Properties/AssemblyInfo.cs:18–20` grants `InternalsVisibleTo` only to
`DynamicProxyGenAssembly2`, `UtilitiesCS.Test`, and `ToDoModel.Test` — **not `QuickFiler.Test`**.
That seam is therefore unreachable from this child's tests. Without K1, any test that reached line
304 or line 350 would display a modal dialog requiring human interaction, which is a direct
violation of the unit-test policy and of `epic.md` Shared Design §2. Adding
`InternalsVisibleTo("QuickFiler.Test")` to `UtilitiesCS` is out of scope: that file is not in this
child's assignment and must not be edited.

Level 1 is chosen over level 2 because two call sites share one four-argument shape with a return
value, and a named interface is Moq-verifiable with argument matchers on the
`Dictionary<string, Action>` payload — the assertion that actually matters, namely that the correct
`RightKeyActions` dictionary reaches the dialog. The in-cluster precedent is
`QuickFiler/Interfaces/IMailItemActions.cs` plus `QuickFiler/Interfaces/MailItemActionsAdapter.cs`,
whose XML doc at lines 5–11 states the pattern verbatim.

### K2 — reuse the existing `UtilitiesCS.Threading.IUiDispatcher` (hierarchy level 1; already exists)

Removes `UiThread.Dispatcher.Invoke(...)` at lines 362 and 370 and
`UiThread.Dispatcher.InvokeAsync(...)` at line 401. `UtilitiesCS/Threading/UiThread.cs:135–140`
backs the static `Dispatcher` with a field set only in `Initialize()`; in a unit-test process it is
null, so any test reaching those lines would throw. `UtilitiesCS/Threading/IUiDispatcher.cs:15–42`
already abstracts exactly this, with `WpfUiDispatcher` as the 1:1 production implementation, and it
is already consumed by `QuickFiler/Controllers/QfcItemController.cs:66` (injected as an optional
parameter at `QfcItemController.Initialization.cs:38`) and by
`QuickFiler/Helper Classes/QfcThemeControlSet.cs` and `QfcThemeHelper.cs`. **Nothing new is
created.** A Moq `IUiDispatcher` that records but does not execute the `Action` keeps the
`cbo.DroppedDown = true/false` assignments from ever running, which is what keeps the tests
handle-free.

### K3 — additive core constructor with defaulted optional parameters (the injection point for K1/K2/K4)

Both existing public constructors delegate to a new private core constructor, and each public
constructor gains optional trailing parameters defaulted to `null`, resolved inside the core to
`new MyBoxDialogPrompt()`, `new WpfUiDispatcher()`, and `cb => cb.DroppedDown`:

```
public KeyboardHandler(
    IQfcFormViewer viewer,
    IFilerHomeController parent,
    IQfcDialogPrompt prompt = null,
    IUiDispatcher uiDispatcher = null,
    Func<ComboBox, bool> isDroppedDown = null)
```

This mirrors `QfcItemController.Initialization.cs:38` exactly. Defaulting to `null` and resolving
inside the core is required, because a C# optional-parameter default must be a compile-time
constant. K3 is not itself one of the three seam tiers; it is the wiring that delivers K1, K2, and
K4, and it is the member whose shape the cross-child determination turns on.

### K4 — `Func<ComboBox, bool>` dropped-down predicate (hierarchy level 2: injectable delegate)

Removes the block at line 278, `if (cb.DroppedDown)`. `ComboBox.DroppedDown` returns `false`
unconditionally on a handle-free control and its setter force-creates a handle, so the `true` branch
of `CboFolders_KeyDownAsync` is unreachable without either a real window or a seam.

**Level 1 was rejected here and the reason is load-bearing.** An interface seam would mean
abstracting the `ComboBox` itself, but `CboFolders_KeyDownAsync(object sender, KeyEventArgs e)` is a
`KeyEventHandler`-shaped interface member (`IQfcKeyboardHandler.cs:28`) wired directly to
`IItemViewer.FolderKeyDown` at `QuickFiler/Controllers/QfcItemController.EventWiring.cs:82`.
Abstracting the parameter would change the interface contract (breaking) and would require touching
F10-owned and F14-owned files. Level 1 is not feasible without a breaking cross-child change, so
level 2 applies. The default `cb => cb.DroppedDown` preserves production behavior exactly.

### K5 — `EnsureSyncContext()` helpers (refactor, not a seam)

The pattern `if (SynchronizationContext.Current is null) SynchronizationContext.SetSynchronizationContext(...)`
is duplicated seven times in two variants: the parent-context variant at lines 106–107, 135–136,
152–153, 240–241, and the WinForms-context variant at lines 268–271, 319–322, 393–396. Extracting
each into a private helper satisfies the General Code Change Policy reusability principle, reduces 14
duplicated lines to two helpers, and collapses seven separately-uncovered branch pairs into two.
This is a pure refactor with identical observable behavior. It is optional; if the planner prefers a
minimum diff, all seven sites are individually testable and K5 may be dropped.

### File size

`KeyboardHandler.cs` is 414 lines today and projects to approximately **456 lines** after K1–K5,
XML docs, and the removal of the three unused `using` directives — under the 500-line limit with
roughly 44 lines of headroom. **No split is required.** A contingency split at the existing
conceptual boundary at line 262 is documented in `research/01-KeyboardHandler.md` §8 and is to be
performed **only** if the measured line count exceeds 500.

### What needs no seam (recorded so the planner does not over-engineer)

- `new QuickFiler.ItemViewer()` already constructs headlessly in three ordinary `[TestClass]` files
  (`BreadcrumbPendingOpenCloseTests.cs:363`, `BreadcrumbCoordinatorLifecycleTests.cs:477`,
  `QfcItemControllerBreadcrumbDropDownTests.cs:373`), despite `ItemViewer.Designer.cs` being 6,224
  lines with 64 `WebView2`/`FastObjectListView`/`ButtonSVG` occurrences.
- `viewer.SetFolderDroppedDown(false)` at line 313 is inert on a bare viewer:
  `ItemViewer.FolderSearch.cs:31–32` forwards to `SetBreadcrumbDropDownState`, which at
  `ItemViewer.Breadcrumb.cs:223–232` returns immediately when the lifecycle coordinator is null and
  `droppedDown` is `false`. It touches no `ComboBox` and creates no handle.
- `viewer.Controller` is `IItemControler` and `RightKeyActions` is `Dictionary<string, Action>`, so a
  Moq `IItemControler` supplies the dictionary with no COM.
- `KeyEventArgs` and `PreviewKeyDownEventArgs` are plain in-memory argument objects.

**No `*.StaTests.cs` file is warranted anywhere in this child.** No `*.StaTests.cs` file exists in
`QuickFiler.Test` today and the STA last-resort clause (`epic.md` Shared Design §3) does not engage
for any of the 11 files.

## Cross-Child Contract Note

`epic.md` mandates that F3's change remain additive with respect to sibling-owned files. This
section records the determination and its evidence.

### Determination: ADDITIVE

**Construction sites of the concrete `KeyboardHandler` type (exhaustive, verified by grep for
`new KeyboardHandler` across all `*.cs`):**

| # | Site | Owning child | Shape |
| --- | --- | --- | --- |
| C1 | `QuickFiler/Controllers/QfcHomeController.cs:184–189` | F7 | `(formViewer, homeController) => new KeyboardHandler(formViewer, homeController)` |
| C2 | `QuickFiler/Controllers/EfcHomeControllerDependencyFactories.cs:141–147` | F8 | `new KeyboardHandler(viewer, homeController)` |

Both are two-argument invocations. Adding optional trailing parameters (K3) leaves both expressions
compiling unchanged. Overload resolution is unaffected: the new private core constructor has a
distinct first-parameter type (`IFilerHomeController`) and cannot be selected by either call. The
indirect factory layers that also stay intact are
`EfcHomeControllerDependencyFactories.cs:201–207` and
`EfcHomeControllerDependencies.cs:51, 175, 187–190`, plus the eight test-side factory doubles, all
of which bind to `IQfcKeyboardHandler` rather than to the concrete constructor.

**`IQfcKeyboardHandler` is frozen.** No member is added, removed, renamed, or re-typed. All five
seams live on the concrete `KeyboardHandler` class and in two new files. The interface is consumed by
**20 production locations across F6, F7, F8, F9, F10, F11, and F15**, and by **17 test files**. Four
of those test files use `MockBehavior.Strict` — `EfcHomeControllerDependenciesTests.cs:42, 188`,
`EfcHomeControllerDependenciesProductionFactoryTests.cs:400`, and
`QfcItemControllerBreadcrumbDropDownTests.cs:161` — so even *adding* an interface member would break
siblings when the strict mock encountered an unconfigured call. That is a further, concrete reason to
add nothing.

### Changes explicitly REJECTED as breaking

| Change | Determination | Minimum delta if ever pursued |
| --- | --- | --- |
| Widen `IQfcKeyboardHandler.BreadcrumbArrowFallThrough(ItemViewer, BreadcrumbArrowDirection)` to `IItemViewer` | **BREAKING** under the additive mandate. It is source-compatible for both in-repo callers (`QfcItemController.ViewerSetup.cs:187` passes a concrete `ItemViewer`; the Moq setup at `QfcItemControllerBreadcrumbDropDownTests.cs:163` still compiles), but it is an interface signature change on a contract consumed by F6/F9/F10/F11/F15. It is also **not needed** — the concrete `ItemViewer` is testable headlessly. | 1 line at `QuickFiler/Interfaces/IQfcKeyboardHandler.cs:33` + 1 line at `QuickFiler/Controllers/KeyboardHandler.cs:293`, then re-verify the F10 strict-mock setup. Belongs to a dedicated cross-cutting issue, not to F3. |
| Add an `IKeyboardHandlerHost { void SetKeyboardHandler(IQfcKeyboardHandler); }` and make `EfcViewer` implement it, to make constructor #2 testable | **BREAKING** — requires editing `QuickFiler/Viewers/EfcViewer.cs`, an **F9-owned** file. Prohibited by the delegation mandate. | n/a — rejected outright. |
| Change `CboFolders_KeyDownAsync(object, KeyEventArgs)` to take a typed parameter | **BREAKING** — breaks the `KeyEventHandler` delegate conversion at `QfcItemController.EventWiring.cs:82` (F10-owned). | n/a — rejected outright. |
| Add `Task KeyDownTaskAsync(object, KeyEventArgs)` to `IQfcKeyboardHandler` | **BREAKING** — all four strict mocks would need configuring; touches F8 and F10 test files. | n/a — rejected outright. |
| Add `ArgumentNullException` guards to the `KeyboardHandler` constructors | **Behavior change**, prohibited by the no-behavior-change criterion. Characterize instead (proposed case 5 asserts the current `NullReferenceException`). | n/a — promote as a follow-up issue. |
| Add `InternalsVisibleTo("QuickFiler.Test")` to `UtilitiesCS` to reach `MyBox.DialogInvoker` | Modifies a shared, non-F3 assembly's public-surface policy for one child's convenience. K1 achieves the same end inside F3's own boundary. | n/a — rejected outright. |

### `IItemControler.cs` stays byte-identical

`research/11-IItemControler.md` §6 enumerates every contemplated change to
`QuickFiler/Interfaces/IItemControler.cs` and finds that each one touches F9-, F10-, or F14-owned
files:

- Renaming the misspelled `IItemControler` to `IItemController` requires edits to
  `QfcItemController.cs:28` (F10), `EfcItemController.cs:26` (F9), `ItemViewer.cs:52–53` (F14),
  `ItemViewerExpanded.cs:50–51` (F14), and `IItemViewer.cs:17` (F14) — five sibling-owned files.
- Changing the namespace from `QuickFiler` to `QuickFiler.Interfaces` compile-breaks every consumer's
  `using` list.
- Adding a member forces edits to `QfcItemController` (F10) and `EfcItemController` (F9).
- Consolidating the triplicated `CounterEnter` / `CounterComboRight` / `RightKeyActions`
  declarations by making `IQfcItemController : IItemControler` lands the edit in
  `QuickFiler/Interfaces/IQfcItemController.cs`, which `epic.md:327` assigns to **F10**.
- Removing the three unused `using` directives (lines 3–5) or the redundant `public` at line 13 is
  non-breaking but delivers zero coverage benefit and creates merge-conflict surface during a
  14-child parallel wave.

**F3 leaves the file byte-identical**, which satisfies the additive mandate trivially and removes the
file from the epic's conflict surface. The interface is nonetheless load-bearing for this child's
test work: a `Mock<IItemControler>` supplies `RightKeyActions` at `KeyboardHandler.cs:308` and `:354`
without constructing any controller.

### Files this child creates or modifies

| File | Action | Rationale |
| --- | --- | --- |
| `QuickFiler/Controllers/KeyboardHandler.cs` | Modify — remove attribute and unused usings, add K1–K5 | The child's central work |
| `QuickFiler/Interfaces/IQfcDialogPrompt.cs` | Create | K1 interface |
| `QuickFiler/Interfaces/MyBoxDialogPrompt.cs` | Create | K1 production adapter |
| `QuickFiler/Interfaces/MailItemActionsAdapter.cs` | Modify — constructor null guard only | See "One production change beyond seams" below |
| The other 8 in-scope production files | **No change** | Every research artifact recommends an empty production change set |
| `QuickFiler.Test/Controllers/KaCharTests.cs`, `KaKeyTests.cs`, `KaStringAsyncTests.cs`, `QfcFormKeyHandlerTests.cs`, `MailItemActionsAdapterTests.cs` | Append test methods | Already registered in `QuickFiler.Test.csproj` (lines 94, 95, 96, and 148 respectively); appending avoids a `.csproj` edit |
| New `QuickFiler.Test/Controllers/KeyboardHandler.*Tests.cs` files and `KbdActionsConstructionAndEdgeTests.cs` | Create + one `<Compile Include>` entry each in `QuickFiler.Test/QuickFiler.Test.csproj` | The test csproj is a per-project file, not a shared build property file, so it is not covered by the shared-file prohibition. It is a known merge hot spot shared with F9/F10/F11; append new entries adjacent to the existing block at lines 92–96 to keep the conflict hunk small |

## One production change beyond the seams

`QuickFiler/Interfaces/MailItemActionsAdapter.cs:17–20` has a bare `_mail = mail;` with no
precondition check, so a null argument produces a `NullReferenceException` at some later, unrelated
call site rather than at construction. That violates `CLAUDE.md` § C#4.3 ("Validate constructor and
method preconditions") and § 3 ("Enforce invariants at construction/initialization time").

**Recommendation: add a fail-fast `ArgumentNullException` guard.** It is **provably unreachable in
production**: the sole production construction site,
`QuickFiler/Controllers/QfcItemController.Initialization.cs:392–394`, already null-checks before
constructing —

```csharp
_mailActions ??= mailItem is null
    ? null
    : new QuickFiler.Interfaces.MailItemActionsAdapter(mailItem);
```

— and a repository-wide grep for `new MailItemActionsAdapter` returns exactly two hits: that line and
the test helper at `MailItemActionsAdapterTests.cs:20`. The guard therefore causes **zero observable
behavior change** and converts a latent, deferred `NullReferenceException` into an explicit
documented contract. The public constructor shape `MailItemActionsAdapter(MailItem)` is
byte-identical; no sibling-owned file changes.

**The guard and its test (T1, `Constructor_WithNullMailItem_ThrowsArgumentNullException`) are a
single indivisible unit.** Shipping the guard without T1 adds an uncovered branch and takes the file
off 100% branch coverage, which is a coverage regression on changed lines and a blocking finding
under `.claude/rules/csharp.md:41`. Shipping T1 without the guard cannot compile a passing
assertion. If the planner defers the change, both must be deferred and the deferral recorded
explicitly as a decision, not an omission.

## Dependencies on F1

F1 `quickfiler-coverage-denominator-and-exemption-ledger` (wave 0) merges to the integration branch
before this child executes. This child consumes F1's ledger and harness at execution time and
raises four requirements on them.

### D1 — A third ledger category, `interface-only`, is required

`QuickFiler/Interfaces/IKbdAction.cs`, `IQfcKeyboardHandler.cs`, `IMailItemActions.cs`, and
`IItemControler.cs` have zero executable IL. Each was verified by exhaustive construct check and
corroborated empirically: a grep for
`name="QuickFiler\.(Interfaces\.IMailItemActions|IItemControler)"` against
`docs/features/active/2026-08-06-quickfiler-high-confidence-queue-init-stall-424/evidence/qa-gates/coverage-final.cobertura.xml`
returns no matches, because the instrumenter emits no `<class>` element for a type with no method
bodies.

They are **not `testable`**: an `>= 80%` obligation against an empty denominator is 0/0 and
unsatisfiable by construction. They are **not `ratified-exempt`**: that category means an
irreducible untestable remainder was accepted *after* a refactor attempt, and filing these there
would misfile them alongside the genuine COM/WinForms remainder and inflate the epic's exemption-count
leading indicator (`epic.md:14`). The correct citation is
`.claude/rules/general-unit-test.md` § Coverage Requirements: *"Type-only / interface-only modules
with no executable behavior may be omitted from coverage measurement. Examples: ... C# interface-only
files. Such modules legitimately report 0% executable coverage and may be excluded from measurement.
This is a clarification only; it does not lower any coverage threshold."*

`epic.md:112` records that roughly **24 of the 121 compiled files** are interface-only declarations.
F1's choice for these four therefore sets a 24-file precedent, and retrofitting a category across 24
ledger rows after the fact is more expensive than adding it up front. Raise D1 with F1 before this
child executes.

### D2 — 0/0 must report `N/A`, never `0%`

A file that produces no Cobertura `<class>` element has 0 measurable lines. F1's per-file harness
must report it as `N/A` / `not measured` and exclude it from the `>= 80%` gate arithmetic. Reporting
it as `0%` would create roughly 24 permanently-failing false gate failures across the epic; dropping
it silently without a marker would make F16's "every one of the 121 compiled files is accounted for"
capstone check unverifiable.

### D3 — The harness must key on the `<class>` element's `filename` attribute

`QuickFiler.IItemControler` appears in the current Cobertura report **only** as a method-signature
substring on a consumer: `<method ... name="set_Controller" signature="(QuickFiler.IItemControler)">`
at `coverage-final.cobertura.xml:5401`, which is an `ItemViewer.cs` line. A harness that attributes
coverage by naive substring match on the type name rather than by the `<class>` element's `filename`
attribute would mis-attribute `ItemViewer.cs` lines to `IItemControler.cs`.

### D4 — Path normalization is required

Committed Cobertura reports show both relative and absolute `filename` forms across runs: compare
`docs/features/active/2026-08-06-quickfiler-high-confidence-queue-init-stall-424/evidence/qa-gates/coverage-final.cobertura.xml:14448`
(relative, `QuickFiler\Interfaces\MailItemActionsAdapter.cs`) with
`.../evidence/baseline/coverage-baseline.cobertura.xml:14529` (absolute). Without normalization the
same file can appear twice or be missed entirely.

### D5 — F1's ledger is the sole authority

**No in-scope file may be assumed `testable` or `ratified-exempt` by this child.** This spec records
the evidence supporting a recommended classification per file; F1's ledger makes the call and this
child consumes it at execution time. Two escalation rules apply:

- If F1 classifies `KeyboardHandler.cs` as `ratified-exempt` in whole, contradicting the seam
  analysis above, escalate to the epic orchestrator with `research/01-KeyboardHandler.md` §5 as the
  counter-evidence before accepting.
- If F1 classifies any of the four interface-only files as `testable` with an `>= 80%` target, that
  target is 0/0 and unreachable; escalate rather than fabricate tests.

**Recommended classifications for F1 to ratify:**

| File | Recommended classification |
| --- | --- |
| `KeyboardHandler.cs` | `testable` after seam extraction; ledger entry requested only for the irreducible remainder in "Irreducible remainder" below |
| `QfcFormKeyHandler.cs`, `KbdActions.cs`, `KaChar.cs`, `KaKey.cs`, `KaStringAsync.cs`, `MailItemActionsAdapter.cs` | `testable` |
| `IKbdAction.cs`, `IQfcKeyboardHandler.cs`, `IMailItemActions.cs`, `IItemControler.cs` | `interface-only — zero executable lines — not in the coverage denominator` |
| `MyBoxDialogPrompt.cs` (new) | ratification requested for its single forwarding statement; see below |
| `IQfcDialogPrompt.cs` (new) | `interface-only` |

## Irreducible remainder (small and contingent)

Only two candidates survive the seam analysis.

### R1 — `KeyboardHandler(EfcViewer viewer, IFilerHomeController parent)`, lines 35–39 (5 lines, ~1.2%)

`EfcViewer` is `public partial class EfcViewer : Form` (`QuickFiler/Viewers/EfcViewer.cs:20–21`),
itself `[ExcludeFromCodeCoverage]`; its constructor runs `InitializeComponent()` against a 4,276-line
designer file and calls `TaskScheduler.FromCurrentSynchronizationContext()`.

**This is contingent, not settled.** `EfcViewer.Designer.cs` contains 12
`WebView2`/`FastObjectListView`/`ButtonSVG` occurrences against 64 in `ItemViewer.Designer.cs`, and
`ItemViewer` already constructs headlessly in three ordinary `[TestClass]` files. The
`SynchronizationContext.Current` precondition is satisfiable by the same scope pattern already used
at `BreadcrumbPendingOpenCloseTests.cs:359–362`. **The plan must therefore include one exploratory
task** that attempts `new EfcViewer()` inside a `SynchronizationContext` scope and asserts
`viewer.KeyboardHandler` (internal getter, `EfcViewer.cs:56–59`, reachable via `InternalsVisibleTo`)
is the handler. If it constructs, R1 is covered and no ledger entry is needed. Only if it fails is
ratification of lines 35–39 requested, with the reason: *the parameter type is a concrete
`Form`-derived, already-exempt, sibling-owned viewer (F9), and the only non-breaking alternative
would require adding an interface to `EfcViewer.cs`, a sibling-owned file.*

### R2 — the single forwarding statement in the new `MyBoxDialogPrompt` adapter

`MyBoxDialogPrompt.ShowActionDialog` is one expression forwarding to the static `MyBox.ShowDialog`,
which constructs and shows a `MyBoxViewer` form. Because `UtilitiesCS` does not grant
`InternalsVisibleTo("QuickFiler.Test")`, the `DialogInvoker` stub is unreachable and the statement
cannot execute without a human-interactive modal dialog. Request an F1 ledger entry classifying the
file `ratified-exempt` under the "thinnest possible wiring in the host-bound entry point" standard of
`.claude/rules/general-unit-test.md` § Coverage Exclusion Policy; the file must contain nothing but
the forward — no branching, no state. If F1 declines, the fallback is to keep the default as a
`private static readonly Func<...>` field inside `KeyboardHandler.cs`, which reduces R2 to one line
of an existing file and creates no new ledger entry. **The planner must choose one before Phase 1
and record the choice.**

Nothing else is a remainder. `UiThread.Dispatcher`, `ComboBox.DroppedDown`, `SynchronizationContext`,
`ItemViewer`, the `MyBox` call sites, all seven properties, both toggle pairs, all key-routing
methods, and `GetItemViewer` are all reducible. Any residual exemption is **F1's ledger to ratify,
not this child's to self-grant.**

## Determinism

- `async void` members of `KeyboardHandler` (`KeyboardHandler_KeyDownAsync`,
  `ToggleKeyboardDialogAsync(object, KeyEventArgs)`, `CboFolders_KeyDownAsync`) are exercised under
  an `InlineSynchronizationContext` whose `Post` invokes the callback synchronously — the precedent
  at `QuickFiler.Test/Viewers/BreadcrumbPendingOpenCloseTests.cs:375–378`. Every awaited task in
  those paths is already completed, so no continuation is ever posted and the assertion is
  deterministic.
- Every test that touches `SynchronizationContext.Current` snapshots and restores it in a
  `finally`/`IDisposable` scope (precedent: `BreadcrumbPendingOpenCloseTests.cs:353–373`). This is
  mandatory: `TaskMaster.runsettings:4–7` configures `ClassLevel` parallelization, so a leaked
  ambient context could contaminate a sibling class on the same thread.
- Async delegate assertions use `Task.CompletedTask` and `Task.FromException`, both of which complete
  synchronously.
- **No `Thread.Sleep`, no `Task.Delay`, no `.Wait()`/`.Result`, no wall-clock wait, no fake timer,
  and no injected clock appears anywhere in this child** (see Correction C1).
- The `async void` signatures on `IQfcKeyboardHandler` are **not** changed. They are required by the
  `KeyEventHandler` delegate conversions at `QfcItemController.EventWiring.cs:41, 82` and
  `QfcFormController.SetupDisposal.cs:160, 188`.

## Out of scope — latent defects (report-only)

The following are real findings from research. **Fixing any of them would violate the no-behavior-change
criterion**, and each is being promoted to a separate GitHub issue by the epic orchestrator so the
finding survives the feature-folder merge. New tests must **characterize current behavior, not the
desired behavior**, and each characterization test must carry an XML comment naming it as such and
citing the promoted issue number.

| # | Defect | Evidence | Why not fixed here |
| --- | --- | --- | --- |
| L1 | `KbdActions(IEnumerable<UClass>)` (`KbdActions.cs:26–29`) bypasses the duplicate guard that both `Add` overloads enforce. `QfcCollectionController.cs:1268–1270` registers two `KaKey` entries sharing `SourceId="Collection"` and `Key=Keys.Down`; `Find(Keys.Down)` (`KbdActions.cs:63–67`) and therefore `KeyboardHandler.cs:122` then throw `InvalidOperationException`. | `research/04-KbdActions.md` G2 | Adding a constructor guard would make `QfcCollectionController` (F11-owned) throw at registration time — a breaking runtime change remediated in a sibling's file. Whether the synchronous registration path is live at runtime is an F11 determination. |
| L2 | `KaChar.DelegateType` (`KaChar.cs:43–46`) returns `typeof(Action<Keys>)` while `KaChar` stores `Action<char>`. A copy-paste defect from `KaKey.cs:45`. | `research/05-KaChar.md` G1 | Correcting the return value changes a public member's observable value on a type consumed by F9 and F11. **No consumer exists today** — a grep for `DelegateType` returns only the two declarations and the commented-out interface line — so production impact is nil. |
| L3 | `Update` and `DelegateType` are orphaned public API on four types (`KaChar`, `KaCharAsync`, `KaKey`, `KaKeyAsync`), explained by the commented-out contract members at `IKbdAction.cs:15–16`. Only `KaStringAsync.Update` still has a consumer (its own `KeyEquals` at lines 62 and 73). | `research/05-KaChar.md` G2–G3, `research/06-KaKey.md` G1–G3, `research/08-IKbdAction.md` §2.2 | Deleting public API from types consumed by F9 and F11 is non-additive. Note for the promoted issue: **restoring `DelegateType` to the interface will not compile** — `KaCharAsync` and `KaKeyAsync` lack the member — so the cleanup direction is removal from the implementers, not restoration to the contract. File **one** issue covering all four types, not four. |
| L4 | `KaStringAsync.KeyEquals("")` throws `ArgumentOutOfRangeException` via `Substring(-1, 1)` at line 62, because `Key.Contains("")` is always true and the empty probe enters the contains branch. Double-shielded today: the driver always appends a character before probing (`KeyboardHandler.cs:180–181`), and `Update` is always null in production wiring (`QfcCollectionController.cs:1376–1383`). Separately, `KeyEquals(null)` throws an unguarded `ArgumentNullException` at line 59, and the `Key` setter / constructor throw `NullReferenceException` rather than `ArgumentNullException`. | `research/07-KaStringAsync.md` G1, G2, G8 | Changing the throw contract of `KeyEquals` alters behavior observed by `KbdActions.ContainsKey`/`FilterKeys`/`Find`, which F9, F10, and F11 consume. |
| L5 | **The highest-value untested behavior in the cluster.** `KaStringAsync.KeyEquals` line 72 invokes `Update` **without** the `Activated` gate that lines 61 and 74 apply. With a multi-character non-matching probe and `Activated == false`, `Update` fires while `ToggleControl` does not. The existing `KeyEquals_MultiCharNonMatch_...` test sets `Activated = true` and therefore cannot distinguish the two gates. | `research/07-KaStringAsync.md` G5 | Gating line 72 to match line 61 changes observable side-effect behavior. Flag the promoted issue as "intent unclear", not "confirmed bug" — no evidence establishes which behavior was intended. Test TC-7 is the only case that separates the two gates and must be authored. |
| L6 | Pre-existing test-layout deviation: `QuickFiler.Test/Controllers/MailItemActionsAdapterTests.cs` sits in `Controllers/` while its production file is `QuickFiler/Interfaces/MailItemActionsAdapter.cs`, against the mirroring rule in `.claude/rules/general-unit-test.md` § Test File Location. `QuickFiler.Test` has no `Interfaces/` folder. | `research/10-MailItemActionsAdapter.md` R2 | Moving it requires a `<Compile Include>` path edit in the legacy non-SDK `QuickFiler.Test.csproj` and creates a rename-vs-edit merge conflict during a 14-child parallel wave, for zero coverage benefit. **Recommendation: append new methods to the existing file; refer the deviation to F16 to adjudicate project-wide.** |

Additional report-only observations that are not defects:

- `KbdActions.cs` line 189 (`if (actions.Length == 0)`) is unreachable: line 181 guards with
  `ContainsKey` (`_list.Any(x => x.KeyEquals(key))`) and line 188 calls `FilterKeys`
  (`_list.Where(x => x.KeyEquals(key)).ToArray()`) over the same predicate and list. Record as an
  unreachable-branch note; do not contrive a test.
- `ClearFilter()` (`KeyboardHandler.cs:81`), `KeyboardHandler_PreviewKeyDown` (96–102), and
  `GetItemViewer` (247–261) have no callers anywhere in the repository. Cover them (they are cheap
  and remain on the `internal`/public surface) and open a follow-up issue proposing removal. Do not
  delete them here — deletion is a public-surface change.
- `KaKey`/`KaKeyAsync` `KeyEquals` uses plain `==` against the `[Flags]` `Keys` enum, so a
  modifier-combined registration can never match through `KeyboardHandler`, which looks up with
  `e.KeyCode` (modifiers stripped). This is a legitimate contract given the lookup path, not a
  defect; the tests that pin it must be worded as contract documentation.

## Constraints & Risks

- **Cross-child contract risk.** Addressed by the Cross-Child Contract Note above. The determination
  is ADDITIVE; no sibling-owned file is edited.
- **Event-driven surface.** Keyboard handling is event-driven. Tests never construct live forms,
  never show popups, and never depend on the UI thread. Headless `ItemViewer` / `ComboBox` /
  `Panel` / `Label` construction with no handle creation is permitted and has established precedent.
- **Determinism.** See the Determinism section. Correction C1 supersedes the `issue.md` fake-timer
  constraint.
- **Shared-file isolation.** This child must not modify `coverage.config`,
  `UtilitiesCS/Properties/AssemblyInfo.cs`, any shared build property file, or any sibling-owned
  production or test file. `QuickFiler.Test/QuickFiler.Test.csproj` `<Compile Include>` additions are
  permitted (per-project test file, not a shared build property file) and are a known merge hot spot
  shared with F9/F10/F11.
- **Upstream dependency.** F1's ledger is the sole authority on classification and F1's harness is the
  per-file evidence mechanism. F1 merges to the integration branch before this child executes.
- **`WindowsFormsSynchronizationContext` leakage.** Two proposed cases deliberately install one.
  Mandate the disposable restore scope in every test that touches `SynchronizationContext`.
- **Behavior-change temptation.** Several defensive improvements suggest themselves — constructor
  null guards, `ConfigureAwait(false)`, converting `async void` to `async Task`. The plan must state
  that constructors gain no guards (except the `MailItemActionsAdapter` guard justified above),
  `async void` signatures are unchanged, and no `ConfigureAwait` is added.
- **Rebase collisions.** Two QuickFiler features were in flight on `main` (#400 folder-selector
  dropdown, #424 high-confidence queue init stall). F3 edits neither `QfcHomeController.cs` (F7 /
  #424 territory) nor `QfcItemController.ViewerSetup.cs` (F10 / #400 territory), so the additive
  determination keeps the merge clean.

## Implementation Strategy

- **Sequencing.** The `QfcFormKeyHandler` boundary cases are the cheapest, lowest-risk tasks and
  depend on nothing beyond F1's classification; they are good Phase 1 candidates that establish the
  test-file conventions. The `Ka*` and `KbdActions` cases follow. The `KeyboardHandler.cs` seam work
  is the largest phase and carries the child's entire coverage risk.
- **Shared test-support file (not a test case):**
  `QuickFiler.Test/Controllers/KeyboardHandler.TestSupport.cs` — an internal static helper providing
  (a) `BuildHandler(...)` returning a `KeyboardHandler` plus its `Mock<IQfcFormViewer>`,
  `Mock<IFilerHomeController>`, `Mock<IFilerFormController>`, `Mock<IQfcDialogPrompt>`, and
  `Mock<IUiDispatcher>`; (b) a `SyncContextScope : IDisposable`; (c) an
  `InlineSynchronizationContext`.
- **Use the real types, not mocks, for `KbdActions<>` / `KaChar` / `KaKey` / `KaKeyAsync` /
  `KaCharAsync` / `KaStringAsync`** inside `KeyboardHandler` tests. They are first-party and
  host-neutral, and `QfcCollectionControllerTests.cs:332` already documents this practice.
- **Characterization tests that cite a promoted issue number must be authored after that issue
  exists**, or authored last.
- **New production files** (`IQfcDialogPrompt.cs`, `MyBoxDialogPrompt.cs`) are subject to the
  `>= 90%` new-code floor in `.claude/rules/csharp.md:40`; that is the reason R2 needs an explicit
  disposition before Phase 1.
- **No new package dependency is introduced.** No `NetArchTest.Rules`, no
  `Microsoft.Extensions.TimeProvider.Testing`.

## Acceptance Criteria

- [ ] **AC1 — Per-file coverage floor.** Every file in the F3 assignment that F1's ledger classifies
      `testable` reaches at least 80% line coverage, verified with F1's per-file harness, with the
      numeric per-file result committed under
      `docs/features/active/2026-08-07-quickfiler-keyboard-actions-coverage-430/evidence/qa-gates/`.
      Files that F1's ledger classifies `interface-only` report `N/A` and are excluded from the
      numeric floor.
- [ ] **AC2 — `KeyboardHandler.cs` de-exemption.** `[ExcludeFromCodeCoverage]` is removed from
      `QuickFiler/Controllers/KeyboardHandler.cs:22`, the three unused `using` directives (lines 12,
      14, 15) are removed, and the file reaches the floor via seams K1–K5 — unless F1's ledger
      ratifies a specific irreducible remainder (candidate: lines 35–39 only, the `EfcViewer`
      constructor overload, 5 lines / ~1.2%). Any residual exemption is recorded in F1's ledger and
      is not self-granted by this child.
- [ ] **AC3 — Additive cross-child contract.** `QuickFiler/Interfaces/IQfcKeyboardHandler.cs` has no
      member added, removed, renamed, or re-typed, and both existing two-argument construction sites
      — `QuickFiler/Controllers/QfcHomeController.cs:184–189` and
      `QuickFiler/Controllers/EfcHomeControllerDependencyFactories.cs:141–147` — compile unmodified.
- [ ] **AC4 — File size.** No production file in scope exceeds 500 lines. `KeyboardHandler.cs` is
      measured after the refactor; the contingency split at line 262 is applied only if the measured
      count exceeds 500.
- [ ] **AC5 — Test framework and determinism.** Every new or modified test uses MSTest, Moq, and
      FluentAssertions in Arrange–Act–Assert form, and is deterministic and isolated: no temporary
      files, no external services, no live forms, no popups, no UI-thread dependency, and no
      `Thread.Sleep`, `Task.Delay`, `.Wait()`/`.Result`, or wall-clock wait. `async void` paths use
      the `InlineSynchronizationContext` precedent, and every test touching
      `SynchronizationContext.Current` restores it in a disposable scope.
- [ ] **AC6 — Scenario completeness per file.** For each in-scope file with executable behavior,
      coverage spans the positive path plus invalid-input, boundary, and error-handling behavior.
      Where a category is structurally inapplicable (for example, `QfcFormKeyHandler.IsAltKeyCommand`
      takes a non-nullable enum and cannot throw), that fact is recorded rather than a test being
      manufactured to satisfy the form.
- [ ] **AC7 — Toolchain.** The full C# toolchain passes in final form in one uninterrupted pass, and
      the commands run are stated: `csharpier .`; the analyzer build
      (`/p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`); the nullable build
      (`/p:Nullable=enable /p:TreatWarningsAsErrors=true`); and coverage-enabled
      `vstest.console.exe ... /EnableCodeCoverage`.
- [ ] **AC8 — No behavior change.** No observable QuickFiler keyboard flow changes. Latent defects
      L1–L6 are characterized, not fixed; every characterization test carries an XML comment naming
      it as a characterization test and citing the promoted issue number.
- [ ] **AC9 — File-boundary isolation.** No file outside the F3 assignment is modified —
      specifically not `coverage.config`, not `UtilitiesCS/Properties/AssemblyInfo.cs`, not any
      shared build property file, and not any sibling-owned production or test file. Exactly two
      edits are permitted outside the F3 production set, both limited to the addition of
      `<Compile Include>` entries and neither touching any other element: (a) new test files added to
      `QuickFiler.Test/QuickFiler.Test.csproj`; (b) the two new F3-authored production files added to
      `QuickFiler/QuickFiler.csproj`. Item (b) is unavoidable — the legacy non-SDK project uses no
      globbing, so a new production file that is not listed does not compile. A `.csproj` is the
      project's own file and is not a shared build property file such as `Directory.Build.props`;
      the shared-file prohibition in `epic.md` is not engaged. Both edits are merge hot spots shared
      with sibling children, so each must be a minimal hunk placed adjacent to the existing
      `<Compile Include>` block rather than reordering or reformatting the file.
- [ ] **AC10 — `MailItemActionsAdapter` guard is atomic with its test.** The
      `ArgumentNullException` constructor guard and test T1
      (`Constructor_WithNullMailItem_ThrowsArgumentNullException`) ship together, or both are
      deferred and the deferral is recorded as an explicit decision. Neither ships alone.
- [ ] **AC11 — No timer or clock seam.** No `TimeProvider`, `FakeTimeProvider`, fake-timer facility,
      or injected clock is introduced in any production or test file in this child. The `issue.md`
      lines 73–74 fake-timer expectation for `KaStringAsync` is recorded as corrected (see Correction
      C1), and the existing `KaStringAsyncTests.cs` is confirmed free of wall-clock waits with no
      remediation performed.
- [ ] **AC12 — Evidence.** Baseline and final per-file coverage figures are written under
      `<FEATURE>/evidence/qa-gates/` per
      `.claude/skills/evidence-and-timestamp-conventions/SKILL.md`. The evidence states whether the
      harness aggregates Cobertura entries by `filename` or by class (which determines whether
      `KaChar.cs` and `KaKey.cs`, each declaring two classes, report as one figure or two) and how
      0/0 files are reported.
- [ ] **AC13 — F1 ledger consumed, not presumed.** F1's ledger classification is read and cited for
      each of the 11 in-scope files. No file is assumed `testable` or `ratified-exempt` by this
      child. If F1's classification conflicts with the evidence recorded in this spec, the conflict is
      escalated to the epic orchestrator rather than resolved by fabricating tests or self-granting an
      exemption.
- [ ] **AC14 — Repository-wide coverage recorded, not regressed.** Repository-wide line and branch
      coverage figures are recorded before and after this child's work as a
      record-and-report obligation. This child does not lower them. The repository-wide floor is not
      a blocking gate for this child, whose change-scoped obligations are AC1, AC2, and the
      `>= 90%` new-code floor on the new files `IQfcDialogPrompt.cs` and `MyBoxDialogPrompt.cs` per
      `.claude/rules/csharp.md:40`.

## Definition of Done

The authoritative acceptance-criteria set for this feature is the 14 checkboxes under
`## Acceptance Criteria` above (in this file) and the matching 14 in `user-story.md`. The items below
are completion reminders, deliberately **not** written as checkboxes so that no automated AC tally
counts them twice.

1. All 14 acceptance criteria in this file are checked off with evidence.
2. The same 14 criteria are checked off in `user-story.md` — work mode `full-feature` tracks both AC
   sources independently.
3. Per-file numeric coverage evidence is committed under `<FEATURE>/evidence/qa-gates/`.
4. The four F1 requirements D1–D4 have been raised with F1 and their disposition recorded.
5. The R2 disposition (separate `MyBoxDialogPrompt.cs` file versus an in-file
   `private static readonly Func<...>` default) is chosen and recorded before Phase 1.
6. The R1 exploratory task (headless `new EfcViewer()`) has been run and its outcome recorded —
   either covered, or a 5-line ledger entry requested.
7. Latent defects L1–L6 are recorded as promoted GitHub issues, and every characterization test cites
   its issue number.
8. The working tree is clean and all audit-trail evidence is committed.

## Sources

**Policy**
- `CLAUDE.md` — § UT2 (COM/VSTO/WinForms coverage exemption, testable denominator, and the
  `KbdActions<>` non-exemption clause), § C#4.3, § CUT1–CUT3
- `.claude/rules/general-unit-test.md` — § Coverage Requirements (interface-only clarification),
  § Coverage Exclusion Policy, § Scenario Completeness, § Test File Location,
  § Determinism Infrastructure
- `.claude/rules/csharp.md` — DI seam hierarchy (49–53), coverage floors (39–41), Prohibited
  Behaviors (89–96)
- `.claude/rules/general-code-change.md` — § Design Principles, § File Size Limit
- `.claude/rules/tonality.md`

**Epic and feature**
- `docs/features/epics/quickfiler-per-file-coverage/epic.md` — intent and leading indicators (1–20),
  Scope (106–121), Non-Goals (123–130), Shared Design §§1–6 (132–192), F3 assignment (267–275),
  Known Conflict Risks (405–418)
- `docs/features/active/2026-08-07-quickfiler-keyboard-actions-coverage-430/issue.md`

**Research artifacts (authoritative evidence base)**
- `research/01-KeyboardHandler.md` through `research/11-IItemControler.md` (all eleven, read in full)
