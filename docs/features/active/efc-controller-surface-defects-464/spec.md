# efc-controller-surface-defects (Spec)

- **Issue:** #464
- **Parent (optional):** epic `quickfiler-bug-family`, wave 2
- **Owner:** drmoisan
- **Last Updated:** 2026-08-25T13-05
- **Status:** Ready for Planning
- **Version:** 1.0
- **Work Mode:** `full-bug`

> **Authoritative acceptance-criteria source.** Under the `acceptance-criteria-tracking` skill, work
> mode `full-bug` resolves the AC source to **`spec.md` only**. This file is that source. There is no
> `user-story.md` for this feature and none is to be created. `issue.md` names this file as the AC
> source and deliberately carries no criteria of its own.

> **Precedence of inputs.** Where this spec disagrees with `issue.md`, this spec wins, because it
> incorporates the research artifact
> `research/2026-08-25T12-20-efc-controller-surface-defects.md`, which re-read every citation against
> the working tree at `036a205d` (merge base `2300becf`). Where the research artifact disagrees with
> `issue.md`, the research wins. Upstream member lists, detach ordering, `Cleanup()` statement-order
> constraints, the post-`Cleanup()` lifecycle invariant, and the deterministic timer technique are
> **consumed as written** from `docs/features/active/qfc-item-controller-defects-484/spec.md`
> §`Upstream contract (exhaustive)` (line 329) and its §`Deterministic timer test for #484` (line 634).
> They are not re-derived from source here.

---

## Context

This feature closes eight pre-existing defect issues on the Email Filer (EFC) controller surface:
**#459, #460, #461, #463, #464 (primary), #465, #466, #467**. All eight were filed on 2026-08-07 during
preparation research for epic #136 and were deferred out of that work because its non-functional
requirement prohibited behavior change to observable QuickFiler flows.

- **Observed environment.** Outlook VSTO add-in, `Debug|Any CPU`, `net48`. The defects are in
  `QuickFiler/Controllers/EfcFormController.cs` (1084 lines), `QuickFiler/Controllers/EfcItemController.cs`
  (1170 lines), `QuickFiler/Viewers/EfcViewer.cs` (162 lines), and one literal in
  `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs` (430 lines).
- **Customer impact and severity.** Mixed, and materially different from what the eight issue bodies
  claim. Four of the eight issues carry defects that are **latent** — real defects on real members that
  no compiled call path can currently reach. The per-issue table in §`Severity and Latency` states this
  precisely. The live defects affect the Email Filer's Alt-key menu routing (two mnemonics lost every
  session), its folder-suggestion refresh (an illegal cross-thread control read), repeated delete
  gestures (a duplicated "Trash to Delete" row), banner-row classification, WebView2 incognito isolation
  (browsing data persists), and five fault paths that terminate the process or silently blank the folder
  list instead of reaching a logged boundary.
- **First observed.** All eight were identified by static reading on 2026-08-07 and re-verified against
  the tree on 2026-08-25. None was reported from the field; none is a regression introduced by a recent
  change.
- **Why one feature and not eight.** The defects are confined to three files of one subsystem plus one
  shared line, and three issues share a single root cause (RC1). Splitting would produce eight concurrent
  branches editing the same three files.

## Repro & Evidence

Two classes of defect, with different evidence requirements.

**Live defects — reproducible through the UI.**

| Issue | Steps | Expected | Actual |
|---|---|---|---|
| #467 | Open the Email Filer form; press Alt+F or Alt+M. | The `Filters` / `Move Options` menu opens via its `&` mnemonic. | `ProcessCmdKey` (`EfcViewer.cs:94-105`) returns `true` for *every* Alt-modified key whenever a handler is attached, so `base.ProcessCmdKey` never runs and both mnemonics are dead. Deterministic, every press. |
| #465 C | Invoke the delete action twice in one session. | One `"Trash to Delete"` row. | `ActionDeleteAsync` (`:740-748`) inserts at index 0 and `BindFolderRows` writes the result back into `_folderRows` at `:879`, so the second invocation inserts a second trash row. Deterministic. |
| #465 B | Type in the search box. | Suggestions refresh. | `RefreshSuggestionsAsync` reads `_formViewer.SearchText.Text` inside the `Task.Run` lambda at `:799` — an illegal cross-thread control read. Data-dependent; may surface as `InvalidOperationException` or as a stale read. |
| #465 D | Select a folder row beginning with exactly three `=` characters. | Consistent classification. | `IsValidSelection` tests `Substring(0, 3) == "==="` (`:1047`); `ActionOkAsync` tests `StartsWith("====")` (`:706`); `BreadcrumbRowBuilder.BannerPrefix` is `"===="`. A three-`=` row classifies inconsistently across the three sites. |
| #463 | Open an item preview; inspect WebView2 browsing data. | Incognito; nothing persists. | The additional-browser-arguments literal is `"–incognito "`, whose first character is **U+2013 EN DASH**, not two ASCII hyphen-minus characters. Chromium ignores the unrecognised token silently. |
| #464 B / C / E | Force a fault on any of the five `async void` handlers, on `PopulateFolderCombobox`, or on WebView2 initialization. | The fault reaches a logged boundary. | B: `logger.Error(...); throw;` inside `async void` re-raises onto the synchronization context. C: `_ = PopulateFolderCombobox()` with no `catch` in the callee, so the folder list silently stays empty. E: `throw (e.InitializationException)` at `EfcItemController.cs:777` resets the captured stack trace. |
| #461 | Load a conversation in the background. | Rows reach the topic thread. | They do — but through `UpdateUI`, not through the handler that appears to do it. See §`Root Cause Analysis` RC6. |

**Latent defects — not reproducible through the UI; evidence is metadata, not behavior.**

`EfcItemController.Cleanup()` (`:255-278`) has **zero call sites repo-wide** (research §Q7.4):
`EfcFormController` never calls it, `IItemControler` (`QuickFiler/Interfaces/IItemControler.cs:9-14`)
does not declare it, and `EfcItemController` never registers itself as `_itemViewer.Controller` (the
assignment is commented out at `:129`). The synchronous expansion path
`ToggleExpansion()` (`:838-848`) / `ToggleExpansion(Enums.ToggleState)` (`:862-905`) likewise has zero
reachable call sites (research §Q7.8). Consequently:

- **#459's stated reproduction ("expand through the sync path") is not reachable through the UI.**
- **#460's stated reproduction ("reach `Cleanup()` through the constructor") is not reachable at all.**
- `#466` is latent by definition — it *is* the dead-code issue.
- `#464 D`'s two `async void` lambdas live inside the dead sync expansion member.

**A latent defect's regression test must pin the post-change contract rather than reproduce a
user-visible failure.** This is stated once here and governs every test named in §`Test Strategy`: for a
latent defect the test asserts the member's post-change behavior under direct invocation, or asserts the
member's absence from type metadata after removal. It does not attempt to drive the defect through a
call path that does not exist.

---

## Root-Cause Grouping

The epic brief requires an explicit root-cause grouping, because eight issues is a large surface and the
plan must not sprawl into eight independent workstreams. The grouping below starts from `issue.md`'s
RC1–RC11, applies research revisions **R1–R5**, and is **authoritative** for planning. Every group names
the issues it covers and whether its remedy is a **GUARD**, a **CORRECTION**, or a **DELETION**.

| RC | Cause | Issues / sub-defects covered | Remedy class | Revision vs `issue.md` |
|---|---|---|---|---|
| **RC1** | No post-teardown null-state contract | **#460 A, #460 C, #464 A, #465 A** | **GUARD** | **R1** — adds `EfcItemController.ActiveTheme` (`:395`) and `EfcItemController.LoadTheme` (`:407`) |
| **RC2** | Dereference-instead-of-dispose on teardown | #460 B | **CORRECTION** | unchanged |
| **RC3** | Fault escapes an unlogged boundary | **#464 B, #464 C, #464 E** | **CORRECTION** | **R2** — #464 D removed from this group; three of `issue.md`'s six cited lambdas are not defects |
| **RC4** | `KbdActions<>` contract misuse | #459 A, #459 B, #459 C | **DELETION** | **R3(a)** — closed by removing the dead sync path, not by repair |
| **RC5** | Non-ASCII character in a machine-parsed literal | #463 | **CORRECTION** (2 sites) + **DELETION** (1 site) | two live sites, not three |
| **RC6** | `nameof` bound to a name the publisher never raises | #461 | **DELETION** | remedy changed from rename to removal |
| **RC7** | Duplicated magic constant with divergent arity | #465 D | **CORRECTION** | a fifth site exists and is out of scope |
| **RC8** | Illegal cross-thread WinForms control read | #465 B | **CORRECTION** | unchanged |
| **RC9** | Read-modify-write through a rebind that writes back | #465 C | **CORRECTION** | write-back is `:879`, not `:871` |
| **RC10** | Input-routing over-claim | #467 | **GUARD** | the QFC twin shares the defect and supplies only the testability pattern |
| **RC11** | Dead code carrying a latent trap | **#466 A/B/C/D**, plus `ToggleExpansion` ×2 and `ConversationResolverPropertyChanged` | **DELETION** | **R4** (RC11-A narrowed), **R3**, §Q7.8 |

### Where two or more issues share ONE cause

This is the load-bearing part of the grouping. Four sharings exist and each collapses work that would
otherwise be planned separately.

1. **RC1 is one cause spanning three issues — #460, #464, and #465.** `Cleanup()` on both controllers
   nulls its fields and nothing downstream guards the resulting state: property getters, re-entrant
   action paths, and dependency-passing helpers all assume live fields. The remedy shape is single —
   an explicit post-cleanup contract consisting of an idempotent `Cleanup`, lazily-evaluated dependency
   checks, and consistent accessor backing fields — and the already-merged QFC twins
   (`QfcFormController.cs:100-155`, `QfcFormController.SetupDisposal.cs:208-228`) carry exactly those
   guards. Fixing #464 A without fixing #460 A/C and #465 A would leave the same class of defect live on
   adjacent members of the same two files. **RC1 must be delivered as one unit.**

2. **RC4 and RC11 share one deletion — #459 and #466 close together.**
   `EfcItemController.ToggleExpansion(Enums.ToggleState)` (`:862-905`) is the sole writer of the
   `'B'`/`'D'` `CharActions` entries and the sole home of the two genuine `async void` lambdas. It is
   dead, and `ToggleExpansion()` (`:838-848`) is its only caller and is itself dead. Deleting both
   overloads closes **#459 B**, **#459 C**, and **#464 D** by removal. Deleting `RegisterActions`
   (`:680-692`) closes **#459 A** by removal.

3. **Deleting `ToggleExpansion(Enums.ToggleState)` dissolves the shared-edit-site sequencing
   constraint that `issue.md` flagged.** `issue.md` RC4 records that "the `'B'`/`'D'` registration block
   that #459 B must change contains the `async void` lambdas that #464 D must change
   (`EfcItemController.cs:882`, `:887`)… two causes at one edit site [that] must be sequenced in one
   phase, not two." Under the deletion remedy **there is no edit site**: both causes vanish with the
   member. The plan is therefore free to place #459 and #464 D in the same removal phase with no
   ordering constraint between them, and no phase edits a member a later phase deletes.

4. **RC5 and RC11 share one deletion, and RC6 and RC3 share one.** The EN DASH literal at
   `EfcItemController.cs:184` sits inside `InitializeWebView()`, which RC11-B deletes; the literal is
   therefore not edited, it is removed with its container. Separately,
   `ConversationResolverPropertyChanged` (`:741-755`) is `public async void`, so RC6's deletion also
   discharges one `async void` member that RC3 would otherwise have to wrap.

**RC10 and RC11-A are both in `EfcViewer.cs` but are distinct causes with distinct remedies** — an
over-claiming input guard and a dead trap — and must not be merged.

---

## Severity and Latency

Research revision **R5** established that four of the eight issues carry latent defects. The table
below is the truthful severity statement that replaces the uniform "High" ratings in `issue.md`'s issue
table.

| Issue | Live / latent | Reachability evidence | Severity as filed | Severity as verified |
|---|---|---|---|---|
| **#459** | **LATENT (wholly)** | `RegisterActions` (`:680`) zero call sites; `ToggleExpansion()` (`:838`) zero reachable call sites, and `ToggleExpansion(ToggleState)` (`:862`) reachable only from it (research §Q7.3, §Q7.8) | High | Latent — no user-visible path |
| **#460** | **LATENT (wholly)** | `EfcItemController.Cleanup()` (`:255-278`) has zero call sites repo-wide (§Q7.4). A/B are named latent by R5; **C manifests only in the post-`Cleanup()` state, so it is latent by the same argument** — this extension is stated here, not in the research | High | Latent — public members, unreachable defects |
| **#461** | **LIVE** | Subscription at `:666-669` fires on every background conversation load; the handler body at `:749-753` never executes | High | Medium — the intended behavior is already delivered by `UpdateUI`, so no user-visible loss; the defect is a member that reads as live and is not |
| **#463** | **LIVE** (2 of 3 sites) | `:217` reached via `Task.Run(() => InitializeWebViewAsync())` at `:110` and `:164`; `ViewerSetup.cs:55` reached on the QFC path. `:184` is dead | Medium | Medium — WebView2 browsing data persists |
| **#464** | **LIVE, one latent sub-defect** | A, B, C, E all sit on live paths. **D is latent**: its two lambdas at `:882`/`:887` are inside the dead sync expansion member | High | High for A/B/C/E; latent for D |
| **#465** | **LIVE** | A reachable from five `Cleanup()` call sites (`:479`, `:510`, `:727`, `:737`, `:790`); B, C, D on live action paths | High | High |
| **#466** | **LATENT (by definition)** | This is the dead-code issue. `EfcViewer._formController` is permanently null; `EfcViewer.EditFiltersMenuItem_Click` is unreachable because the Designer never wires it | Medium | Latent — the trap is armed by a routine Designer regeneration, not by user action |
| **#467** | **LIVE** | Every Alt keypress on the Email Filer form while a handler is attached | Medium | Medium — exactly two Alt chords lost, `Alt+F` (`"&Filters"`) and `Alt+M` (`"&Move Options"`) |

**Consequence for testing.** Latency does not reduce scope. Every one of these is a real defect on a
public or `internal` member, and CLAUDE.md's Bugfix Workflow requires a failing regression test for each
regardless. What latency changes is the *instrument*: for #459, #460, #464 D and #466 the regression test
pins the post-change contract by direct invocation or by a type-metadata assertion, because there is no
user-visible failure to reproduce.

---

## Scope & Non-Goals

### In scope — production files this feature may write

| File | Lines at merge base | Scope of change |
|---|---|---|
| `QuickFiler/Controllers/EfcFormController.cs` | 1084 | RC1 (form side), RC3, RC7, RC8, RC9 |
| `QuickFiler/Controllers/EfcItemController.cs` | 1170 | RC1 (item side), RC2, RC3 (`:777`), RC5 (`:217`), RC6, RC11-B/C |
| `QuickFiler/Viewers/EfcViewer.cs` | 162 | RC10, RC11-A |
| `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs` | 430 | **RC5 only — the single literal at `:55`. Exactly one line.** |
| `QuickFiler/Viewers/EfcViewer3.cs`, `.Designer.cs`, `.resx` | — | **Deleted** (RC11-D). These three files have no `<Compile Include>` entry. |

### In scope — test files

| File | Lines at merge base | Scope |
|---|---|---|
| `QuickFiler.Test/Controllers/EfcFormControllerTests.cs` | 168 | Extend — #464 A/B/C, #465 A/B/C/D |
| `QuickFiler.Test/Controllers/EfcItemControllerTests.cs` | new | Create — #459, #461, #463, #464 D/E, #466 structural |
| `QuickFiler.Test/Controllers/EfcItemController.CleanupTests.cs` | new | Create — #460 A/B/C |
| `QuickFiler.Test/Controllers/EfcViewerTests.cs` | new | Create — #467, #466 A structural |
| `QuickFiler.Test/QuickFiler.Test.csproj` | — | New `<Compile Include>` entries only, inserted contiguously after line 112 |

### Out of scope / non-goals

- **Splitting `EfcFormController.cs` or `EfcItemController.cs` to satisfy the 500-line ceiling.** Both
  exceed it at the merge base (1084 and 1170) and predate this feature. Reducing them is a refactor, not
  a bug fix, and would collide with every other epic child touching these files. Sibling features #498
  and #484 record the identical position. **No acceptance criterion in this feature asserts a line count
  under 500 for either file.** The ceiling is asserted only over files this feature creates.
- **Adding an overload to `UtilitiesCS/HelperClasses/Initializer.cs`.** Rejected — the QFC twin solves
  RC1's eager-argument problem with a plain conditional expression at the call site
  (`QfcFormController.cs:131-142`, `:100-105`), needing zero change to a repo-wide consumed file.
- **Repairing dead code rather than deleting it.** See §`Root Cause Analysis` RC4 and RC11 for the
  justification.
- **Any new Edit Filters functionality.** RC11-A is resolved by removal; the command already works.
- **Consolidating the fifth banner-prefix constant** at
  `UtilitiesCS/OutlookObjects/Folder/FolderSuggestionTree.cs:16`. Outside the owned set; recorded as a
  downstream note.
- **Deleting the seventeen other uncompiled `QuickFiler/Viewers/*.cs` orphans**, or `QuickFiler/Legacy/**`
  and `QuickFiler/Notes/**`. Repository hygiene, not a bug fix; promote separately.

### Sibling-owned files this feature must NOT touch

Ownership confirmed reciprocally in research §Q11.1. Every path below is forbidden to this feature's diff.

| Path | Owner |
|---|---|
| `QuickFiler/Controllers/EfcHomeController.cs`, `EfcHomeController.ExecuteMoves.cs`, and all `EfcHomeController.*` | **#442** |
| `QuickFiler/Controllers/BreadcrumbBridgeRouter.cs` and the breadcrumb router surface | **#498** |
| `QuickFiler/Controllers/KeyboardHandler.cs` | **#498** |
| `QuickFiler/Viewers/BreadcrumbBridgeCoordinator*.cs`, `BreadcrumbMessengerHub.cs` | **#501** |
| `QuickFiler/Controllers/QfcItemController.*.cs` — **all partials, beyond the single `ViewerSetup.cs:55` literal** | **#484** (`Navigation.cs` to **#444**) |
| `QuickFiler/Controllers/KbdActions.cs` | **#444** |
| `QuickFiler.Test/Controllers/KbdActionsTests.cs`, `KbdActionsRemainingBranchesTests.cs` | **#444** |
| `QuickFiler/Interfaces/IQfcCollectionController.cs`, `QuickFiler/Controllers/QfcCollectionController.cs` | **#468** |
| `QuickFiler.Test/Controllers/QfcCollectionControllerTests.cs` — **exactly 500 lines; `[TestMethod]` count frozen** | **#468** |
| `QuickFiler.Test/Controllers/QfcItemController.*Tests.cs` (11 files) | **#484**, **#489** |
| `QuickFiler/Viewers/ItemViewer*.cs`, `QuickFiler/Viewers/IItemViewer.cs` | **#489** |
| `QuickFiler/Viewers/WebView2BreadcrumbHost.cs`, `WebView2CoreInitializer.cs` | **#476** |
| `UtilitiesCS/OutlookObjects/Folder/BreadcrumbRowBuilder.cs` — **read-only reference for RC7** | **#498** (its AC asserts the file is unmodified) |
| `UtilitiesCS/HelperClasses/Initializer.cs` | unowned; repo-wide consumer, do not extend |
| `QuickFiler/Interfaces/IQfcKeyboardHandler.cs`, `IItemControler.cs` | unowned; read-only reference |
| **`QuickFiler/QuickFiler.csproj`** | **#501 adds one line after `:392`.** RC11-D needs no csproj edit because the `EfcViewer3.*` files carry no project entries. **This feature declares the file untouched.** |

**One additional cross-feature constraint.** Feature #476's design depends on the
`new WebView2BreadcrumbHost(...)` construction at `EfcFormController.cs:834-837` compiling unchanged.
**This feature must not move or reshape that call site.**

---

## Root Cause Analysis

### RC1 — No post-teardown null-state contract (GUARD; #460 A, #460 C, #464 A, #465 A)

`Cleanup()` on both controllers nulls its fields and nothing downstream guards the resulting state.
Confirmed sites:

- `EfcFormController.Cleanup()` (`:187-194`) dereferences `_globals.Ol` unguarded at `:189`, and invokes
  `_parentCleanup.Invoke()` unguarded at `:193` **without nulling it**. A second call throws
  `NullReferenceException` at `:189`; guarding only that line would instead **double-invoke
  `_parentCleanup`** — a second defect the promoted document does not name. `Cleanup()` is reachable from
  five sites: `:479`, `:510`, `:727`, `:737`, `:790`.
- `EfcFormController.ActiveTheme` (`:255`) passes `strict: true` with a possibly-null `_themes` as the
  sole dependency, so `Initializer.DependenciesNotNull` throws `ArgumentNullException` instead of
  returning the default. `LoadTheme` (`:267`) dereferences a null `_themes` directly.
- `EfcFormController.DarkMode` getter (`:274-281`) passes `_globals.Ol` as a `params object[]` element at
  `:280`. Because every `Initializer.GetOrLoad` overload takes `params object[] dependencies`, the array
  is materialised **before** the method is entered, so a null `_globals` throws before the dependency
  check can run. The failure path would have returned `default(bool) == false`, which is the intended
  default.
- `EfcItemController.DarkMode` (`:441-448`, eager arg at `:447`) repeats the shape. **R1 adds the two
  members `issue.md` omits: `EfcItemController.ActiveTheme` (`:395`, `strict: true` with `_themes`) and
  `EfcItemController.LoadTheme` (`:404-409`, unguarded `_themes[activeTheme]` at `:407`).**
- `EfcItemController.Cleanup()` (`:255-278`) unconditionally dereferences `Buttons` (backing `_buttons`)
  at `:257`, which the 5-argument constructor (`:59-74`) never assigns; never nulls `_buttons` while
  nulling fifteen siblings; and writes `_itemViewer = null` twice, at `:264` and `:276`.
- `EfcItemController.Subject` reads `_itemViewer.LblSubject.Text` (`:610-613`) while `Sender` (`:595-598`)
  and `To` (`:621-624`) read `_itemInfo`, so `Subject` throws post-`Cleanup` while its siblings still work.

### RC2 — Dereference-instead-of-dispose on teardown (CORRECTION; #460 B)

`EfcItemController.cs:277` assigns `_timer = null` without disposing the `System.Threading.Timer`
declared at `:377` and armed at `:875-876` and `:953-954`, leaking an armed OS timer per item cleaned up
while expanded and unread. This lives in the same method as RC1 but is a distinct cause: the field is
correctly nulled, the resource is not released. The callback `ApplyReadEmailFormat(object state)` is
`public` at `:1125-1129` and dereferences `_itemInfo` (`:1127`) and `_themes[_activeTheme]` (`:1128`),
both of which `Cleanup()` invalidates, so the guard is genuinely needed and not defensive padding.

### RC3 — Fault escapes an unlogged boundary (CORRECTION; #464 B, C, E)

- **B**: `logger.Error(ex.Message, ex); throw;` inside five `async void` handlers in
  `EfcFormController.cs` — the `throw;` statements are at `:425`, `:441`, `:457`, `:517`, `:530`.
- **C**: `_ = PopulateFolderCombobox()` at `:95` and `:115` with no `try`/`catch` in the callee
  (`:1022-1036`), so the folder list silently stays empty on fault. The sibling fire-and-forget in
  `InitializeBreadcrumbHostAsync` (`:851`, `:856-866`) does carry a logged boundary and is the in-repo
  remedy pattern.
- **E**: `throw (e.InitializationException)` at `EfcItemController.cs:777` rethrows a captured exception
  and resets its stack trace, from inside a WebView2 UI-thread event handler (`:770-799`).

**R2 correction.** `issue.md` lists six `async void` lambdas for #464 D. Verified against source, only
two qualify. `EfcItemController.cs:704`, `:711`, `:716` register into **`CharActionsAsync`**, whose
delegate type is `Func<char, Task>` (`IQfcKeyboardHandler.cs:22`); an `async` lambda bound to that type
is an async **`Task`** lambda, not `async void`, and its fault **is** observed — awaited at
`KeyboardHandler.cs:176` inside a `try` with `catch` + `logger.Error` at `:139-147`. `:699` is likewise a
`Func<char, Task>` registration. **These four are not defects.** `:741` is a genuine `async void`, but it
is the `INotifyPropertyChanged` handler, which RC6 deletes. Only `:882` and `:887`, registered into
`CharActions` (`Action<char>`, `IQfcKeyboardHandler.cs:21`) via `CharActions.Add` at `:879` and `:884`,
are genuine `async void` — and both are inside the dead member RC11 deletes.

### RC4 — `KbdActions<>` contract misuse (DELETION; #459 A, B, C)

**The `KbdActions<>` contract, documented here so the remedy needs no change to a file this feature does
not own.** Verified at `QuickFiler/Controllers/KbdActions.cs`:

| Member | Location | Behavior |
|---|---|---|
| indexer `get` | `:38` | `Find(key).Delegate` — throws `NullReferenceException` when the key is absent |
| indexer `set` | `:39-46` | assign-if-present. **A missing key is a silent no-op, never an insert.** |
| `Find(TKey)` | `:53-69` | 0 → `default`; 1 → element; **2 or more → `InvalidOperationException`** (`:67`) |
| `Add(string, TKey, VDelegate)` | `:90-104` | **throws `ArgumentException`** on a duplicate `(SourceId, key)` pair (`:97`) |
| `Remove(string, TKey)` | `:123-135` | absent → returns `false` silently |

`overwriteDuplicates` is a parameter of `EfcItemController.RegisterActions`, not of `KbdActions<>`. Its
complete truth table: `false` + key present → filtered out at `:687-689`, nothing happens; `false` + key
absent → survives the filter, indexer setter silently no-ops; `true` + present → overwrites; `true` +
absent → silently no-ops. **So `overwriteDuplicates: false` registers nothing, and `true` overwrites but
never inserts.**

**POST-444 contract, which this feature is authored against.** Feature #444 changes **exactly one**
member of `KbdActions.cs` — the `KbdActions(IEnumerable<UClass>)` constructor gains a duplicate
`(SourceId, StoredKey)` guard. The indexer, `Add`, `Remove`, `ContainsKey`, `Find`, `FindIndex` and
`Keys` are all **unchanged**. All three enumerable-constructor call sites in `EfcFormController`
(`:354-366`, `:570-601`, `:627-675`) seed distinct keys, so none throws under the new guard. **#444
imposes no work on this feature.**

**Why deletion and not repair.** `RegisterActions` (`:680-692`) has zero call sites and is `internal`, so
no consumer exists. Its only correct repair would require deciding the intended indexer-setter contract
(upsert versus assign-if-present) — a change to **`KbdActions.cs`, which is owned by feature #444** and
which #444 restricts to "Constructor guard only". Repairing it inside `EfcItemController` instead (swap
the indexer assignment for `Remove` + `Add`) would create correct-but-uncalled code carrying a `>= 90%`
coverage obligation on a path with no consumer. **CLAUDE.md's Bugfix Workflow step 2 mandates the
minimal, targeted fix and directs that deeper design problems become new issues rather than widening
scope. Repairing unreachable code adds risk with no behavioral benefit.** #459 A's promoted acceptance
idea — "decide and document the intended indexer-setter contract, then align `RegisterActions`" — is
satisfied by documenting the contract in this section and removing the sole mis-user. The contract
decision itself belongs with the owner of `KbdActions.cs`.

**Disposition R3(a) is adopted:** delete `ToggleExpansion()` (`:838-848`) and
`ToggleExpansion(Enums.ToggleState)` (`:862-905`) under RC11, closing #459 B, #459 C and #464 D by
removal. Disposition R3(b) — repair in place via a `SyncExpandedRegistrations(bool)` owner — is
**rejected**: it repairs code no user can reach, it preserves a sequencing constraint that deletion
dissolves, and it adds a member to a file already 670 lines over the ceiling.

### RC5 — Non-ASCII character in a machine-parsed literal (CORRECTION + DELETION; #463)

The literal is `"–incognito "`, whose first character is **U+2013 EN DASH** rather than two ASCII
hyphen-minus characters (U+002D U+002D). Chromium command-line switches are introduced by two ASCII
hyphens; `CoreWebView2EnvironmentOptions.AdditionalBrowserArguments` is passed through verbatim and an
unrecognised token is ignored silently. The in-file counter-evidence is the commented alternative
directly above two of the three sites, which uses ASCII correctly (`"--disk-cache-size=1 "` at
`EfcItemController.cs:182`, `:215`, and `QfcItemController.ViewerSetup.cs:54`).

Three sites, three dispositions:

| Site | Enclosing member | Reachable | Disposition |
|---|---|---|---|
| `EfcItemController.cs:184` | `InitializeWebView()` (`:174-205`) | **No** — zero call sites | **Do not edit. Deleted with its container under RC11-B.** Editing a literal inside a member the same feature deletes is churn, and under the opposite phase ordering the edit would be lost. |
| `EfcItemController.cs:217` | `InitializeWebViewAsync()` (`:207-240`) | **Yes** — `Task.Run(() => InitializeWebViewAsync())` at `:110`, `:164` | **Correct**, via a hoisted `internal const`. |
| `QfcItemController.ViewerSetup.cs:55` | `InitializeWebViewAsync()` (`:42-128`) | **Yes** | **Correct**, in place, one line. |

**#463 is the one-line exception to the "no other change to `QfcItemController.ViewerSetup.cs`" rule.**
`issue.md:212-214` already carves it out; this section states why the exception is unavoidable.
Upstream feature #484 does **not** touch line 55: research searched the entire
`docs/features/active/qfc-item-controller-defects-484/` folder for `incognito`,
`CoreWebView2EnvironmentOptions`, and `ViewerSetup.cs:5x` and found **zero matches**, and the statements
484's prose describes itself as touching begin at `:79` (the `CoreWebView2` capture) and run to `:105`
(the `WebResourceRequested` lambda). Line 55 sits **24 lines above** `:79`. **This feature must make
that edit itself** — if it does not, the defect survives on the QFC path with no other owner.

**Two corrections to the earlier framing of this risk, neither of which changes the disposition.**
First, `484/spec.md:363` declares 484's edit region for `InitializeWebViewAsync` as
`ViewerSetup.cs:42-128`, and that **declared region includes line 55**; the `:79` start above is an
inference from 484's prose about which statements it intends to touch, not its declared region. Second,
this feature does **not** branch from a base that already carries 484 (see §`Dependencies or blocked
work`), so the two edits are concurrent rather than sequential. The residual risk remains textual rather
than semantic — the two features change different characters for different reasons — but a merge
conflict on this one line is a realistic outcome, and it must be resolved by keeping both edits, never
by dropping this one.

### RC6 — `nameof` bound to a name the publisher never raises (DELETION; #461)

`ConversationResolverPropertyChanged` (`EfcItemController.cs:741-755`) guards at `:746` on
`nameof(_dataModel.ConversationResolver.ConversationInfo.Expanded)`, which resolves at compile time to
the literal `"Expanded"`. `ConversationResolver` raises exactly four distinct names — `"ConversationInfo"`
(`Loading.cs:26`, `:33`), `"ConversationItems"` (`:167`, `:174`), `"Df"` (`:205`, `:227`), and
`"UpdateUI"` (`ConversationResolver.cs:277`). `"Expanded"` is never raised. The subscription at `:666-669`
fires; the body at `:749-753` never executes.

**The remedy is REMOVAL, not renaming, and this is the decisive finding for #461.** The intended
behavior is **already delivered by a different, live path**:

1. `EfcItemController.PopulateConversation()` assigns
   `_dataModel.ConversationResolver.UpdateUI = SetTopicThread;` at **`EfcItemController.cs:314`**.
2. `SetTopicThread(List<MailItemHelper>)` (`:354-359`) performs
   `_itemViewer.TopicThread.SetObjects(conversationInfo)` then
   `Sort(_itemViewer.SentDate, SortOrder.Descending)`.
3. The dead handler body (`:750-753`) performs `SetObjects(...Expanded)` then the **identical** sort.
4. `ConversationResolver.LoadConversationInfoAsync` assigns `ConversationInfo = pair` at `Loading.cs:138`
   and, when `UpdateUI is not null` (`:140`), awaits
   `UiThread.Dispatcher.InvokeAsync(() => UpdateUI(pair.Expanded))` at **`Loading.cs:150`** — on the UI
   thread.

So background-loaded conversation rows **do** reach the topic thread. The subscription is redundant with
that path and the handler body duplicates `SetTopicThread`.

**Retargeting the guard to `"ConversationInfo"` is rejected.** It would make the handler fire and run a
**second** `SetObjects` + `Sort` in addition to the `UpdateUI` dispatch at `Loading.cs:150`, doubling the
work on every background load; it would marshal on a different path (`await _itemViewer.UiSyncContext` at
`:749` versus `UiThread.Dispatcher.InvokeAsync`); and it would read the **lazy** `ConversationInfo.Expanded`
getter, which `Loading.cs:148-149` explicitly documents as something the resolver avoids ("Pass
`pair.Expanded` directly to avoid triggering the lazy property getter and the associated synchronous
`LoadConversationInfo()` call"). Renaming therefore converts a dead-but-harmless member into a live
performance regression that re-enters a getter the publisher deliberately bypasses.

### RC7 — Duplicated magic constant with divergent arity (CORRECTION; #465 D)

The banner prefix is tested three ways: `Substring(0, 3) == "==="` in `IsValidSelection`
(`EfcFormController.cs:1047`), `StartsWith("====")` in `ActionOkAsync` (`:706`), and
`BreadcrumbRowBuilder.BannerPrefix` = `"===="` (`BreadcrumbRowBuilder.cs:19`). A row beginning with
exactly three `=` characters classifies inconsistently. A **fifth** site exists at
`FolderSuggestionTree.cs:16` and is out of scope.

### RC8 — Illegal cross-thread WinForms control read (CORRECTION; #465 B)

`RefreshSuggestionsAsync` (`EfcFormController.cs:795-804`) evaluates `_formViewer.SearchText.Text` inside
the `Task.Run` lambda at `:799`. `SearchText_TextChanged` (`:554-557`) reads the same property correctly
on the UI thread at `:556`.

### RC9 — Read-modify-write through a rebind that writes back (CORRECTION; #465 C)

`ActionDeleteAsync` (`:740-748`) reads `_folderRows`, inserts `"Trash to Delete"` at index 0 (`:746`), and
calls `BindFolderRows` (`:747`), whose body at **`:879`** stores the result — now containing the trash
row — back into `_folderRows`. A second invocation inserts a second trash row. (`issue.md` cites `:871`,
which is the method signature, not the write-back.)

### RC10 — Input-routing over-claim (GUARD; #467)

```csharp
// EfcViewer.cs:94-105
protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
{
    if ((_keyboardHandler is not null) && (keyData.HasFlag(Keys.Alt)))
    {
        object sender = FromHandle(msg.HWnd);
        var e = new KeyEventArgs(keyData);
        _keyboardHandler.ToggleKeyboardDialogAsync(sender, e);
        return true;
    }
    return base.ProcessCmdKey(ref msg, keyData);
}
```

**There is no "does the handler claim this key" query on `IQfcKeyboardHandler`.** The interface declares
`KbdActive`, four `ToggleKeyboardDialog*` members, three `KeyboardHandler_*` members, six `KbdActions<>`
registries, `CboFolders_KeyDownAsync`, and `BreadcrumbArrowFallThrough` — no `Claims`, `CanHandle`,
`Handles`, `ShouldHandle`, or `TryHandle`. `IQfcKeyboardHandler.cs` is not this feature's file.

**The gesture the handler actually services is the bare-Alt toggle, not an Alt chord.**
`KeyboardHandler.ToggleKeyboardDialogAsync(object sender, KeyEventArgs e)`
(`KeyboardHandler.cs:238-245`) sets a synchronization context, awaits the parameterless
`ToggleKeyboardDialogAsync()` (`:225-236`) which flips `_kbdActive` and calls
`ToggleOnNavigationAsync` / `ToggleOffNavigationAsync`, then sets `e.Handled = true`. **It never inspects
`e.KeyData`.** The key code is discarded. Registered character actions such as `'M'`
(`EfcFormController.cs:594-598`, bound to `ShowMenu(_formViewer.MoveOptionsMenu)`) are reached by a bare
character keypress *after* keyboard mode is already active — not by an Alt chord.

**Therefore the claim is exactly: Alt with no other key code.** Any `Alt`+*key* chord is a WinForms
mnemonic and must reach `base.ProcessCmdKey`. This settles the Alt+M question research §Q5.5 raised: Alt+M
is a mnemonic, not a handler claim, and there is no collision with the `'M'` registration.

Exactly two Alt chords are currently lost, both top-level menu mnemonics constructed in
`EfcViewer.Designer.cs`: `FiltersMenu.Text = "&Filters"` (`:4102`, on `FilterMenuStrip`) and
`MoveOptionsMenu.Text = "&Move Options"` (`:4162`, on `MoveOptionsStrip`, which is
`this.MainMenuStrip` at `:4224`). The four drop-down mnemonics (`"Move &Conversation"`,
`"Save &Attachments"`, `"Save E&mail Copy"`, `"Save &Pictures"`) are reached only once a menu is open, at
which point form-level `ProcessCmdKey` is not the routing path. `EditFiltersMenuItem.Text` (`:4138`)
carries no mnemonic.

The QFC twin `QfcFormViewer.cs:56-73` **shares this defect** — `QfcFormKeyHandler.IsAltKeyCommand`
(`QfcFormKeyHandler.cs:18`) is just `keyData.HasFlag(Keys.Alt)`. **This feature does not change the QFC
twin.** What it adopts from the twin is the **testability pattern**: an `internal static` predicate
lifted out of the `ProcessCmdKey` override so the key-command logic can be unit tested without a live
`Form` window handle, exercised by tests carrying no `Form` instance
(`QfcFormKeyHandlerTests.cs`, 67 lines, four `[TestMethod]`s).

### RC11 — Dead code carrying a latent trap (DELETION; #466 A/B/C/D + §Q7.8 additions)

**RC11-A — corrected. `issue.md`'s claim that `EditFiltersMenuItem_Click` "would throw" is incorrect and
the Edit Filters command is NOT broken.** `EfcFormController.WireEventHandlers` performs
`_formViewer.EditFiltersMenuItem.Click += EditFiltersMenuItem_Click;` at **`EfcFormController.cs:398`**,
targeting `EfcFormController.EditFiltersMenuItem_Click` (`:559-564`), which constructs `ManageFilters`,
calls `LoadFilters(_globals)`, and shows it. The controller subscribes directly to the Designer control,
bypassing the viewer entirely. **Edit Filters is wired and functional.** The dead surface is only
`EfcViewer.SetController` (`:50-53`), `EfcViewer._formController` (`:48`, permanently null because
`EfcFormController` never calls `SetController`, unlike `QfcFormController.cs:44`), and the viewer-side
duplicate `EfcViewer.EditFiltersMenuItem_Click` (`:157-160`). **Deleting all three is
behavior-preserving**, removes the trap that a routine `EfcViewer.Designer.cs` regeneration would arm,
and makes the "wire it up" alternative moot.

**RC11-B.** `InitializeWebView()` (`:174-205`) and `RegisterActions` (`:680-692`) have zero call sites.
`_selectorsCtrls` (`:381`) is initialized to `null`, never assigned, and passed to `SetupThemes` at `:97`
and `:144`.

**RC11-C.** The 7-argument `EfcItemController` constructor (`:44-57`) has zero call sites. `new
EfcItemController(` occurs at exactly two sites, both in `EfcFormController.cs`: `:67-73` (the 5-argument
overload) and `:85-92` (the 6-argument overload).

**RC11-D.** `QuickFiler/Viewers/EfcViewer3.cs`, `.Designer.cs` and `.resx` exist on disk with **no**
`<Compile Include>` entry, yet `EfcViewer3.cs:17` carries a misleading `[ExcludeFromCodeCoverage]` on
`public partial class EfcViewer3 : Form` (`:18`). **Deleting these three files requires no
`QuickFiler.csproj` edit**, which eliminates all contention with feature #501.

**§Q7.8 additions to RC11**, beyond `issue.md`: `ToggleExpansion()` (`:838-848`) and
`ToggleExpansion(Enums.ToggleState)` (`:862-905`). The live EFC expansion path is
`RegisterAsyncFocusActions` `'E'` (`:701-705`) → `KbdExecuteAsync(ToggleExpansionAsync)` →
`ToggleExpansionAsync()` (`:850-860`) → `ToggleExpansionAsync(ToggleState)` (`:907-929`) →
`ToggleExpansionOn`/`Off`, and is untouched.

**`EfcItemController.ToggleNavigation(bool async)` (`:958-979`) is dead but is RETAINED.** See
§`Risks & Mitigations` D9.

---

## Proposed Fix

### Design summary (what changes where)

Four remedy classes across three owned production files plus one line of a fourth.

1. **Deletions first** (RC11, RC6, RC4-by-removal, RC5's dead site), so that no later change edits a
   member an earlier change would have deleted.
2. **One-line and constant corrections** (RC5).
3. **Guards** mirroring the merged QFC twins verbatim (RC1) plus the dispose-before-null correction (RC2).
4. **Boundary extraction** for the fault paths (RC3) and **pure-helper extraction** for the three
   `EfcFormController` action defects (RC7, RC8, RC9), each of which doubles as the fix and as the
   testability seam.
5. **Predicate extraction** for the input guard (RC10).

### Boundaries and invariants to preserve

- **`EfcFormController.cs:834-837`** — the `new WebView2BreadcrumbHost(...)` construction feature #476
  depends on — must not move or be reshaped.
- **`BreadcrumbRowBuilder.BannerPrefix`** (`BreadcrumbRowBuilder.cs:19`) is consumed read-only. Feature
  #498's acceptance criteria assert that file is unmodified.
- **The live EFC expansion path** (`:701-705` → `:850-860` → `:907-929` → `ToggleExpansionOn`/`Off`) is
  behavior-identical after the sync overloads are deleted.
- **The Edit Filters subscription** at `EfcFormController.cs:398` must survive RC11-A untouched.
- **`Cleanup()` must remain callable on a partially-constructed controller** on both sides. Upstream
  484's three `Cleanup()` statement-order constraints (`484/spec.md:385-398`) and its post-`Cleanup()`
  lifecycle invariant (`:400-408`) are consumed as written and are not re-derived; the EFC-side
  reordering must not violate the analogous detach-then-null convention.
- **Feature 484's `MoveMailAsync` now wraps and rethrows** rather than swallowing
  (`484/spec.md:359`). This feature **must not copy the swallow-and-continue shape** into
  `EfcItemController`.
- `EfcItemController` carries a class-level `[ExcludeFromCodeCoverage]` (`:25`) and `EfcViewer` carries
  one (`:20`). **No new coverage exemption is added by this feature.** Reusing a file's pre-existing
  class-level attribute adds none; 484's prohibition on *adding* an exemption (`484/spec.md:235`) is
  respected. Exemption is a measurement decision, not a testability barrier — tests against exempt types
  run and assert normally.

### Dependencies or blocked work

**Correction.** `issue.md:10` lists #484 and #444 as upstream dependencies, and this section previously
asserted that both were already on the branch this feature builds on. **That assertion is false.** The
branch point is the feature-493 fan-in commit `2300becf`, at which neither `TryResolveCidResource` (a
#484 member) nor `MoveFailureNotifier` (a #444 member) exists anywhere in the tree. The dispositions
below are restated against that fact; none of them changes what this feature does.

- **Upstream #484** (**not** on the branch point). Its exhaustive upstream contract is consumed as
  written, as a set of conventions rather than as a symbol reference, so nothing here depends on a #484
  member existing. `QfcItemController.ViewerSetup.cs` is therefore **not** already changed, and the
  `:55` edit lands on the pre-#484 text of that file. Because the two edits are concurrent rather than
  sequential, and because 484's declared edit region (`484/spec.md:363`, `ViewerSetup.cs:42-128`)
  includes line 55, the textual-conflict risk on that one line is materially higher than "no collision";
  see §RC5.
- **Upstream #444** (**not** on the branch point). Verified to impose **no work** on this feature:
  it changes only the `KbdActions(IEnumerable<UClass>)` constructor, and all three
  `EfcFormController` enumerable-constructor call sites seed distinct keys. That holds whether or not
  #444 has landed, because this feature neither reads nor writes `KbdActions.cs`.
- No other epic child blocks this feature, and this feature blocks none.

### Implementation strategy (what changes, not sequencing)

#### Files/modules to change

Exactly as enumerated in §`Scope & Non-Goals`. **`QuickFiler/QuickFiler.csproj` is untouched.**

#### Functions/classes/CLI commands impacted

**Removed** (`EfcItemController.cs`): `InitializeWebView()`, `RegisterActions`, the 7-argument
constructor, `_selectorsCtrls`, `ConversationResolverPropertyChanged` and its subscription at `:666-669`,
`ToggleExpansion()`, `ToggleExpansion(Enums.ToggleState)`.
**Removed** (`EfcViewer.cs`): `SetController`, `_formController`, `EditFiltersMenuItem_Click`.
**Removed** (tree): `EfcViewer3.cs`, `EfcViewer3.Designer.cs`, `EfcViewer3.resx`.

`_selectorsCtrls` is deleted and its two `SetupThemes` call sites (`:97`, `:144`) pass `null` explicitly
with a comment recording that the EFC surface has no selector controls. This is behavior-identical — the
field is always null today — and makes the contract explicit instead of concealed. Assigning a real list
instead is rejected: it would change theme-setup behavior, which is a functional change, not a bug fix.

**Added** (all `internal` except where the row says otherwise, all in owned files, none on any
interface). The table is exhaustive: every member this feature adds appears in it.

| Member | Accessibility | File | Purpose |
|---|---|---|---|
| `const string IncognitoArgument = "--incognito ";` | `internal` | `EfcItemController.cs` | RC5, and the assertion target for #463 |
| `static bool ClaimsAltChord(IQfcKeyboardHandler handler, Keys keyData)` | `internal` | `EfcViewer.cs` | RC10 |
| `static string[] WithTrashRow(string[] rows)` | `internal` | `EfcFormController.cs` | RC9 — idempotent trash-row insertion |
| `static bool IsBannerRow(string row)` | `internal` | `EfcFormController.cs` | RC7 — the single classification owner |
| `static void ThrowInitializationFailure(System.Exception initializationException)` | `internal` | `EfcItemController.cs` | RC3-E — `ExceptionDispatchInfo.Capture(...).Throw()` |
| five `async Task` boundary members wrapped by the existing `async void` handlers | `internal` | `EfcFormController.cs` | RC3-B |
| `System.Action<string, System.Exception> BoundaryErrorSink { get; set; }` | `internal` | `EfcFormController.cs` | RC3-B — the injectable boundary error sink; its default delegate is exactly one `logger.Error(message, exception)` call on the pre-existing static logger |
| `const string TrashRowText = "Trash to Delete";` | `internal` | `EfcFormController.cs` | RC9 — the single owner of the trash-row literal |
| `static string[] MatchesForSearchText(System.Func<string, string[]> findMatches, string searchText)` | `internal` | `EfcFormController.cs` | RC8 — pure matching helper, so the control read can be hoisted above `Task.Run` |
| `void ApplyDeleteGesture()` | `internal` | `EfcFormController.cs` | RC9 — the delete gesture: apply `WithTrashRow`, retain, then bind |
| `void BindSourceFolderRows(string[] rows)` | **`private`** | `EfcFormController.cs` | RC9 — the source path: retain the fresh suggestion rows, then bind |
| `static bool IsSelectableFolder(string selectedFolder)` | `internal` | `EfcFormController.cs` | RC7 — the rest of `IsValidSelection`'s pure logic, classifying through `IsBannerRow` |

`[assembly: InternalsVisibleTo("QuickFiler.Test")]` exists at `QuickFiler/Properties/AssemblyInfo.cs:5`,
so every added member is visible to the test assembly without a project-file change.

`ClaimsAltChord` is placed on `EfcViewer` itself rather than in a new
`QuickFiler/Controllers/EfcViewerKeyHandler.cs`. A new file would require a `QuickFiler.csproj`
`<Compile Include>` edit, which this feature has otherwise eliminated and which would contend with
feature #501's one-line addition after `QuickFiler.csproj:392`. `EfcViewer.cs` is owned, is 162 lines,
and a static member is callable without instantiating the `Form`. Extending `QfcFormKeyHandler.cs` is
rejected — not in the owned set.

#### Data flow and validation changes

- **RC1**: every guarded accessor returns its existing backing-field value (or `default`) instead of
  throwing, mirroring `QfcFormController.cs:100-105` and `:131-142` verbatim. `EfcItemController.Subject`
  is retargeted to `_itemInfo`, matching `Sender` and `To`.
- **RC10**: `ProcessCmdKey` returns `true` only when `ClaimsAltChord` is true; otherwise control reaches
  `base.ProcessCmdKey`, restoring both mnemonics.
- **RC9**: `_folderRows` is never written back with a trash row; `WithTrashRow` is idempotent.
- **RC7**: all classification sites route through `IsBannerRow`, which uses one prefix of one arity.

#### Error handling and logging updates

- Each of the five `async void` handlers becomes a thin wrapper over an `internal async Task` member.
  The `throw;` statement is removed; the boundary logs and contains. The in-repo precedent for the
  wrapped shape is `InitializeBreadcrumbHostAsync` (`:856-866`).
- `PopulateFolderCombobox` gains a `try`/`catch` with a logged boundary so the fire-and-forget at `:95`
  and `:115` can no longer fault silently.
- `ThrowInitializationFailure` uses `ExceptionDispatchInfo.Capture(...).Throw()`, preserving the original
  stack trace that `throw (e.InitializationException)` destroys.
- No log level changes and no new log sinks.

#### Rollback/feature-flag considerations

None. Every change is a source-level correction or deletion in a VSTO add-in with no runtime feature-flag
infrastructure. Rollback is `git revert` of the feature branch.

### Technical specifications (interfaces/contracts)

#### Inputs/outputs and formats

- `IncognitoArgument` is the exact string `"--incognito "` — two ASCII hyphen-minus characters (U+002D
  U+002D), the token `incognito`, and one trailing space (the separator required when
  `AdditionalBrowserArguments` carries more than one switch).
- `ClaimsAltChord(handler, keyData)` returns `true` if and only if `handler is not null`, `keyData` has
  the `Keys.Alt` flag, and the key-code portion of `keyData` is `Keys.Menu` or `Keys.None`.
- `IsBannerRow(row)` returns `true` if and only if `row` is non-null and starts with the four-character
  prefix. It must agree with `BreadcrumbRowBuilder.BannerPrefix` and must not use `Substring`, which
  throws on a row shorter than the index.
- `WithTrashRow(rows)` returns a new array with the trash row at index 0, and returns its input unchanged
  when the trash row is already at index 0.

#### Required configuration keys and defaults

None. This feature introduces no configuration.

#### Backward-compatibility expectations

**No interface is modified. No public member is added. No public member's signature changes.** Every
added member is `internal` or `private`. The removed members are `internal` or `private` with zero call
sites, verified individually. `IItemControler`, `IQfcItemController`, `IQfcKeyboardHandler` and
`IItemViewer` are untouched.

#### Performance constraints

No performance target is set. One performance-relevant decision is recorded: RC6's removal remedy avoids
the doubled `SetObjects` + `Sort` per background conversation load, and the lazy-getter re-entry, that
the rename alternative would have introduced.

---

## Assumptions, Constraints, Dependencies

- **Assumption (UNVERIFIED, inherited from research).** Chromium ignores an unrecognised
  `AdditionalBrowserArguments` token silently. This is standard Chromium behavior, not something
  established from this repository; no runtime WebView2 observation was performed and none is permitted
  under the unit-test policy.
- **Assumption (UNVERIFIED).** The behavior of `await (SynchronizationContext)null` under this
  repository's awaiter extension. **The plan must verify this before any test relies on it.** The
  pure-helper extraction strategy for RC8 and RC9 is chosen partly so that no test depends on the answer.
- **Constraint.** CLAUDE.md's Bugfix Workflow: failing regression test first, then the minimal targeted
  fix, then the full toolchain in order. Deeper design problems become new issues, not scope.
- **Constraint.** MSTest, Moq, FluentAssertions. No xUnit, no NUnit.
- **Constraint.** `QuickFiler.Test/NoLiveFormInTestAssemblyTests.cs` (54 lines) asserts that no
  `System.Windows.Forms.Form`-derived type is **compiled into** the test assembly (`:17-36`). It
  constrains type *declarations*, not instantiation. **No new test fixture class may derive from `Form`.**
- **Dependency (corrected).** The branch point does **not** carry #484 or #444. `issue.md:10` lists both
  as upstream dependencies, but at the feature-493 fan-in commit `2300becf` neither
  `TryResolveCidResource` (#484) nor `MoveFailureNotifier` (#444) exists in the tree. Nothing in this
  feature depends on a member either of them adds; see §`Dependencies or blocked work` and §RC5.
- **Recorded policy discrepancy, not resolved here.** CLAUDE.md states a repo-wide 80% line-coverage
  floor and 90% for new modules; `.claude/rules/general-unit-test.md` states 85% line / 75% branch.
  Under `policy-compliance-order` CLAUDE.md is authoritative. This feature reports the discrepancy and
  does not resolve it, and its acceptance criteria use a **baseline comparison** rather than an absolute
  coverage number, so the discrepancy cannot silently gate the work.

---

## Data / API / Config Impact

- **User-facing changes.** Alt+F and Alt+M open their menus again. A repeated delete gesture no longer
  accumulates trash rows. WebView2 preview sessions become genuinely incognito. Fault paths log instead
  of terminating or silently blanking the folder list. No UI layout, wording, or workflow changes.
- **API changes.** None. See §`Backward-compatibility expectations`.
- **Data or migration considerations.** None. No persisted schema, no settings key, no stored format is
  read or written by any changed member.
- **Logging/telemetry updates.** Five new logged boundaries in `EfcFormController` (RC3-B), one in
  `PopulateFolderCombobox` (RC3-C). All use the existing `logger.Error(message, exception)` pattern. No
  new sink, no new level, no new category.
- **Compatibility notes.** No CLI flag, no config schema, no version bump.

---

## Test Strategy

Grounded in research Q8. Every test named below is deterministic MSTest using Moq and FluentAssertions.

### Existing harness and seams

- **`QuickFiler.Test/Controllers/EfcFormControllerTests.cs` is 168 lines and carries 2 `[TestMethod]`s.**
  Its seam is a **private no-argument constructor plus reflection field injection**:
  `EfcFormController` declares `private EfcFormController() { }` at `EfcFormController.cs:77`;
  `CreateMinimalController()` (`EfcFormControllerTests.cs:22-32`) invokes it through
  `GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, Type.EmptyTypes, null)`, and
  `SetPrivateField` (`:159-166`) injects fields. This produces exactly the all-fields-null
  post-`Cleanup()` state RC1 describes, and is the seam for every `EfcFormController` defect
  (#464 A/B/C, #465 A/B/C/D). The existing
  `PopulateFolderCombobox_WhenFormViewerIsNull_...` test (`:38-57`) is the shape to copy.
- **`EfcItemControllerTests.cs` does not exist. `EfcViewerTests.cs` does not exist.** Both are created by
  this feature.
- **`FormatterServices.GetUninitializedObject`** is used in 26 `QuickFiler.Test` files and is the seam for
  `EfcItemController` state-only tests, which has no parameterless constructor.
- **The 5-argument `EfcItemController` constructor** (`:59-74`) is fully injectable —
  `IApplicationGlobals` and `IFilerHomeController` are interfaces, `EfcFormController parent` comes from
  the reflection seam, `ItemViewer` is constructible headlessly, plus a `CancellationToken`. It does
  **not** call `Initialize()`, so no theme setup and no `Task.Run(InitializeWebViewAsync)` runs. That is
  precisely the constructed-but-uninitialised state #460 A requires.
- **A headless real `ItemViewer`** (constructed, never shown, no message loop, bare
  `SynchronizationContext` installed and restored in `finally`) is the authorised pattern at
  `484/spec.md:617-625`, precedent `QfcItemController.EventWiringTests.cs:229-309`.

### The three named blockers, with priced remedies

**Blocker 1 — `_formViewer` is the concrete `EfcViewer`, a `Form` with no interface.** Affects **#465 B**
and **#465 C**, plus every `_formViewer.UiSyncContext` await on the paths under test. Adding an
`IEfcViewer` interface is **rejected** — it would require writing `EfcViewer.cs` extensively and changing
`EfcFormController`'s field type, a refactor CLAUDE.md's Bugfix Workflow step 2 prohibits.
**Priced remedy: extract a pure helper, which *is* the fix in both cases.** For #465 B, "hoist the read
out of the `Task.Run` lambda" is literally the remedy, and the matching logic becomes a pure static
member. For #465 C, `WithTrashRow(string[])` is a pure function testable with no seam at all. Cost: two
small `internal static` members in an owned file; zero seam infrastructure.

**Blocker 2 — an `async void` fault cannot be awaited by a test.** Affects **#464 B** (five handlers). A
test cannot observe whether an `async void` method's continuation threw.
**Priced remedy: the fix removes the blocker.** Replace `logger.Error(ex.Message, ex); throw;` with a
log-and-contain boundary, and extract each handler body to an `internal async Task` member that the
`async void` handler wraps. The test targets the extracted `Task`-returning member and asserts
`NotThrowAsync` plus exactly one logged error. Cost: five wrappers in an owned file; precedent
`InitializeBreadcrumbHostAsync` (`:856-866`).

**Blocker 3 — `CoreWebView2InitializationCompletedEventArgs` is not constructible.** Affects **#464 E**.
`WebView2Control_CoreWebView2InitializationCompleted` (`:770-799`) reads `e.IsSuccess` (`:775`) and
`e.InitializationException` (`:777`); the SDK type's constructor is non-public.
**Priced remedy: extract `internal static void ThrowInitializationFailure(System.Exception)`** carrying
the `ExceptionDispatchInfo.Capture(...).Throw()` call, and reduce the handler's failure branch to a
one-line adapter. The extracted member takes a plain `System.Exception` and is fully testable: assert the
rethrown exception preserves the original stack trace, which is exactly what the current
`throw (e.InitializationException)` destroys. Cost: one static member; the handler itself remains
untested and that limitation is stated, not concealed.

### Deterministic timer test for #460 B — 484's technique reused verbatim

The technique at `484/spec.md:634-650` transfers with **no adaptation** and is reused as written:

- **T1 — disposal is observable via `ObjectDisposedException` on `Change`.** Arrange
  `new Timer(_ => { }, null, Timeout.Infinite, Timeout.Infinite)` — armed with `Timeout.Infinite` so it
  can never fire during the test — and reflection-inject it into `_timer` (`EfcItemController.cs:377`).
  Act: call `Cleanup()`. Assert: the field is null, and
  `Action act = () => timer.Change(0, Timeout.Infinite);` throws `ObjectDisposedException`. The assertion
  is on disposal state, not on a race.
- **T2 — the callback guard is directly invocable.** `ApplyReadEmailFormat(object state)` is `public`
  (`:1125-1129`). Call it on a freshly-`Cleanup()`ed controller and assert `act.Should().NotThrow()`.
  It dereferences `_itemInfo` (`:1127`) and `_themes[_activeTheme]` (`:1128`), both invalidated by
  `Cleanup()` (`_itemInfo = null` at `:275`, `_themes = null` at `:269`), so the guard is genuinely
  needed.

No existing EFC test arms a real timer, so T1 and T2 introduce no cross-test timing coupling.

### #463 has no executable test — use the `internal const` plus assertion

The literal at `EfcItemController.cs:217` sits inside a member of a class carrying
`[ExcludeFromCodeCoverage]` (`:25`) and requires the real WebView2 runtime; the literal at
`QfcItemController.ViewerSetup.cs:55` sits inside a member carrying its own `[ExcludeFromCodeCoverage]`
(`:41`), which 484's spec states will remain. **Neither can be executed under the unit-test policy.**

**A test that reads production source text from disk is NOT the instrument.** It is unverified whether
any existing `QuickFiler.Test` test reads a production `.cs` file, path resolution from a test host is
not deterministic in the way UT4 requires, and the assertion would be about file bytes rather than about
the value the program uses.

**The instrument is a hoisted constant plus a direct assertion.** `EfcItemController` declares
`internal const string IncognitoArgument = "--incognito ";` and `InitializeWebViewAsync` passes it
instead of a literal. A named test asserts, against `EfcItemController.IncognitoArgument`, that the value
equals the expected string, that every character is ASCII (`<= 0x7F`), and that it starts with two
U+002D characters. This needs no file I/O, and it removes the duplication that produced the defect.

**The `QfcItemController.ViewerSetup.cs:55` site is verified by code review, not by test**, and this
limitation is stated rather than concealed. The file is owned by #484 and this feature's diff over it is
constrained to exactly one line, so a hoisted constant is not available there. The acceptance criterion
for that site is the one-line-diff assertion plus review confirmation that the replacement character
sequence is two U+002D characters.

### Structural assertions for removals (#459, #461, #464 D, #466)

Removal is asserted by absence, using reflection over type metadata — the idiom already used at
`NoLiveFormInTestAssemblyTests.cs:20-28`. The executor verifies each site is **absent**; it must not
recreate a block in order to remove it. Concretely: `GetMethod(..., NonPublic | Instance)` returns null
for `SetController`, the viewer-side `EditFiltersMenuItem_Click`, `InitializeWebView`, `RegisterActions`,
`ToggleExpansion` (both overloads) and `ConversationResolverPropertyChanged`;
`typeof(EfcItemController).GetConstructors(NonPublic | Public | Instance)` no longer contains a
7-parameter constructor; `GetField("_selectorsCtrls", NonPublic | Instance)` and
`typeof(EfcViewer).GetField("_formController", NonPublic | Instance)` return null.

For #461 the structural assertion is paired with a **live-path** test proving removal cost nothing:
a named test asserts `PopulateConversation` assigns `SetTopicThread` to
`ConversationResolver.UpdateUI`, so the surviving route is pinned and a future regression that removes
it fails loudly.

### Test-file routing

| File | At merge base | Disposition |
|---|---|---|
| `EfcFormControllerTests.cs` | 168 | **Extend** — 332 lines of headroom |
| `EfcItemControllerTests.cs` | new | **Create** — #459, #461, #463, #464 D/E, #466 structural |
| `EfcItemController.CleanupTests.cs` | new | **Create** — #460 A/B/C. Research recommends planning for **two** item-side test files from the outset rather than discovering the 500-line ceiling mid-execution |
| `EfcViewerTests.cs` | new | **Create** — #467 (four methods, modelled on `QfcFormKeyHandlerTests.cs`'s 67 lines) and #466 A structural |
| `QfcCollectionControllerTests.cs` | **exactly 500** | **DO NOT TOUCH** — at the ceiling, `[TestMethod]` count frozen by #468 |
| `QfcItemController.*Tests.cs`, `KbdActions*Tests.cs` | — | **DO NOT TOUCH** — #484, #489, #444 |

`QuickFiler.Test.csproj`: insert the three new entries contiguously immediately after line 112
(`EfcHomeControllerTests.cs`) and before line 113 (`EmailSorterTests.cs`), in alphabetical order, so the
`Efc*` cluster reads `EfcData… < EfcForm… < EfcHome… < EfcItem… < EfcViewer…` and the whole diff is one
contiguous insertion. Features #444 and #476 each append one line to the same item group; inserting at
`:112` rather than at the group's end reduces, but does not eliminate, textual-conflict risk.

### Determinism — non-negotiable

Every test in this feature must satisfy all of the following. These are hard prohibitions, not
preferences:

- **No `Thread.Sleep`.** The token `Thread.Sleep` must not appear in any test file this feature writes.
- **No `Task.Delay`.** The token `Task.Delay` must not appear in any test file this feature writes.
- **No temporary files** of any kind (CLAUDE.md §UT4; currently approved exceptions: none).
- **No live Outlook**, no COM interop against a running host.
- **No real `BackgroundWorker`**, no message loop, no started thread whose completion is awaited by
  polling.
- **No shown WinForms form.** A headless `ItemViewer` may be constructed but never shown; no test fixture
  class may derive from `Form`.
- The only timer test arms with `Timeout.Infinite` and observes `ObjectDisposedException`; it never waits.

### Toolchain commands

Run in this exact order, restarting from step 1 if any step fails or auto-fixes a file:

1. `dotnet tool run csharpier format .` (verify with `dotnet tool run csharpier check .`)
2. `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
3. `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`
4. `vstest.console.exe <test-assembly-paths> /EnableCodeCoverage`

Step 3 is character-for-character CI's nullable step. **`/p:Nullable=enable` must not be added** and
`/t:Build` must not be substituted for `/t:Rebuild`; both deviations are documented defects of a stricter
local reading and are excluded deliberately.

### Manual validation

Two checks a reviewer performs by hand, because no automated instrument exists:

1. Open the Email Filer form, press Alt+F, then Alt+M; both menus open.
2. Confirm the replacement bytes at `QfcItemController.ViewerSetup.cs:55` are two U+002D characters.

---

## Acceptance Criteria

**74 criteria**, distributed as: #459 — 4, #460 — 7, #461 — 4, #463 — 4, #464 — 12, #465 — 11,
#466 — 8, #467 — 7, cross-cutting — 17. Each is objectively verifiable and capable of failing. Named
tests are preferred over prose searches throughout; where a search is used, the exact token is quoted
verbatim in this document so the assertion is anchored.

**A Phase 0 baseline is a precondition for the cross-cutting criteria.** Before any source change, the
plan records analyzer-diagnostic counts, nullable/type-check results, the `QuickFiler.Test` pass/fail
tally, and coverage under
`docs/features/active/efc-controller-surface-defects-464/evidence/baseline/`. The criteria below compare
against that baseline; **no criterion asserts an absolute diagnostic count or absolute test count over
files this feature does not own.**

### #459 — `KbdActions<>` contract misuse (latent; remedy DELETION)

- [x] A named test asserts `typeof(EfcItemController).GetMethod("RegisterActions", BindingFlags.NonPublic | BindingFlags.Instance)` is null.
- [x] A named test asserts `typeof(EfcItemController)` declares no method named `ToggleExpansion` at any arity.
- [x] A named test invokes the dispatched bodies of the surviving async expansion path in the order On → Off → On against a `Mock<IQfcKeyboardHandler>` backed by a real `KbdActions<char, KaChar, Action<char>>`, asserts no exception is thrown, and asserts the registry is not touched. The `ToggleExpansionAsync(ToggleState)` marshal itself is not awaited, because `ItemViewer.UiDispatcher` is an unpumped WPF dispatcher.
- [ ] The `KbdActions<>` indexer-setter contract (assign-if-present, never insert) and the `overwriteDuplicates` truth table are documented in this spec's §RC4, and `git diff --name-only` for the feature branch contains no path matching `KbdActions`.

### #460 — cleanup NRE and timer leak (latent; remedies GUARD + CORRECTION)

- [x] A named test constructs `EfcItemController` through the 5-argument constructor without calling `Initialize()`, calls `Cleanup()`, and asserts no exception is thrown.
- [x] A named test asserts `Cleanup()` is idempotent on `EfcItemController`: a second consecutive call throws no exception.
- [x] A named test asserts the `_buttons` field is null after `Cleanup()` returns.
- [x] `Cleanup()` contains exactly one assignment of `_itemViewer`; the duplicate previously at `EfcItemController.cs:276` is absent from the post-change method body.
- [x] A named test injects `new Timer(_ => { }, null, Timeout.Infinite, Timeout.Infinite)` into `_timer`, calls `Cleanup()`, asserts the field is null, and asserts `timer.Change(0, Timeout.Infinite)` throws `ObjectDisposedException`.
- [x] A named test calls `ApplyReadEmailFormat(null)` on a freshly-`Cleanup()`ed controller and asserts no exception is thrown.
- [x] A named test asserts `Subject`, `Sender` and `To` all read from `_itemInfo`: with `_itemInfo` injected, all three return the injected values; after `Cleanup()`, all three behave uniformly and none throws.

### #461 — dead conversation-expanded handler (live; remedy DELETION)

- [x] A named test asserts `typeof(EfcItemController).GetMethod("ConversationResolverPropertyChanged", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)` is null.
- [x] A named test asserts `PopulateConversation` assigns `SetTopicThread` to `ConversationResolver.UpdateUI`, pinning the surviving live route.
- [x] No `PropertyChanged +=` subscription to a conversation resolver remains in `EfcItemController.cs`; the block previously at `:666-669` is absent.
- [x] The guard literal is not retargeted: `EfcItemController.cs` contains no occurrence of the token `nameof(_dataModel.ConversationResolver.ConversationInfo.Expanded)`.

### #463 — WebView2 incognito argument (live at 2 of 3 sites; remedies CORRECTION + DELETION)

- [x] `EfcItemController` declares `internal const string IncognitoArgument` and `InitializeWebViewAsync` passes that constant rather than a string literal.
- [x] A named test asserts `EfcItemController.IncognitoArgument` equals `"--incognito "`, that every character satisfies `c <= 0x7F`, and that the first two characters are both U+002D.
- [x] `QfcItemController.ViewerSetup.cs:55` uses two U+002D characters; confirmed by review and by the one-line-diff criterion below.
- [x] The EN DASH site at `EfcItemController.cs:184` is removed together with its containing method rather than edited in place.

### #464 — null-guard and async-void boundary defects (primary; A/B/C/E live, D latent)

- [x] A named test asserts `EfcFormController.DarkMode` on an all-fields-null controller returns `false` and does not throw.
- [x] A named test asserts `EfcFormController.ActiveTheme` on an all-fields-null controller returns its backing-field value and does not throw.
- [x] A named test asserts `EfcFormController.LoadTheme` on an all-fields-null controller does not throw.
- [x] A named test asserts `EfcItemController.DarkMode` on a null-`_globals` controller returns `false` and does not throw.
- [x] A named test asserts `EfcItemController.ActiveTheme` and `EfcItemController.LoadTheme` on a null-`_themes` controller do not throw. **(R1 — the two members `issue.md` omits.)**
- [x] Each of the five `async void` handlers in `EfcFormController.cs` delegates to an `internal async Task` member; a named test per handler faults the collaborator, asserts the extracted member does not throw, and verifies exactly one invocation of the controller's boundary error sink, whose default delegate is verified by source inspection to be exactly one `logger.Error(message, exception)` call on the pre-existing static logger.
- [x] The token `throw;` does not appear inside any `async void` member of `EfcFormController.cs`; the five occurrences previously at `:425`, `:441`, `:457`, `:517` and `:530` are absent.
- [x] A named test faults `PopulateFolderCombobox`'s collaborator and asserts the returned task does not fault and one error is logged.
- [x] `EfcItemController` declares `internal static void ThrowInitializationFailure(System.Exception)`, and the failure branch of `WebView2Control_CoreWebView2InitializationCompleted` is a one-line adapter over it.
- [x] A named test asserts `ThrowInitializationFailure` rethrows the supplied exception with its original stack trace preserved (the rethrown exception's `StackTrace` contains the originating frame).
- [x] The token `throw (e.InitializationException)` does not appear in `EfcItemController.cs`.
- [x] #464 D is closed by the deletion asserted under #459: no `async void` lambda is registered into `CharActions` anywhere in `EfcItemController.cs`.

### #465 — form-controller lifecycle and selection defects (live; remedies GUARD + CORRECTION)

- [x] A named test calls `EfcFormController.Cleanup()` twice and asserts no exception is thrown.
- [x] A named test asserts `_parentCleanup` is invoked exactly once across two consecutive `Cleanup()` calls (`Times.Once()` on an injected `System.Action`), and that the field is null after the first call.
- [x] `RefreshSuggestionsAsync` evaluates `_formViewer.SearchText.Text` on the UI thread before entering `Task.Run`; no member access on `_formViewer` appears inside the `Task.Run` lambda.
- [x] A named test exercises the extracted pure matching helper for the search path with no `EfcViewer` instance and asserts the expected matches for a representative input.
- [x] `EfcFormController` declares `internal static string[] WithTrashRow(string[] rows)`, and a named test asserts that applying it twice yields exactly one trash row.
- [x] A named test drives `ActionDeleteAsync` twice against an injected `_folderRows` and asserts the resulting row set contains exactly one `"Trash to Delete"` entry.
- [x] `BindFolderRows` no longer writes its result back into `_folderRows`; the write-back previously at `:879` is absent.
- [x] `EfcFormController` declares `internal static bool IsBannerRow(string row)`, and both `IsValidSelection` and `ActionOkAsync` classify through it.
- [x] A named test asserts a row of exactly three `=` characters and a row of exactly four `=` characters classify identically in `IsValidSelection` and in `ActionOkAsync`'s guard.
- [x] A named test asserts `IsBannerRow` returns `false` for null and for a row shorter than the prefix, without throwing.
- [ ] `IsBannerRow`'s prefix agrees with `BreadcrumbRowBuilder.BannerPrefix`, and `git diff --name-only` contains no path matching `BreadcrumbRowBuilder`.

### #466 — dead code and latent NRE traps (latent; remedy DELETION)

- [x] A named test asserts `typeof(EfcViewer).GetMethod("SetController", BindingFlags.NonPublic | BindingFlags.Instance)` is null and `typeof(EfcViewer).GetField("_formController", BindingFlags.NonPublic | BindingFlags.Instance)` is null.
- [x] A named test asserts `typeof(EfcViewer)` declares no method named `EditFiltersMenuItem_Click`.
- [x] The Edit Filters subscription at `EfcFormController.cs:398` and its target `EfcFormController.EditFiltersMenuItem_Click` are unchanged; a named test asserts `typeof(EfcFormController)` still declares `EditFiltersMenuItem_Click`.
- [x] A named test asserts `typeof(EfcItemController).GetMethod("InitializeWebView", BindingFlags.NonPublic | BindingFlags.Instance)` is null.
- [x] A named test asserts `typeof(EfcItemController)` declares no 7-parameter instance constructor.
- [x] A named test asserts `typeof(EfcItemController).GetField("_selectorsCtrls", BindingFlags.NonPublic | BindingFlags.Instance)` is null.
- [x] The files `QuickFiler/Viewers/EfcViewer3.cs`, `EfcViewer3.Designer.cs` and `EfcViewer3.resx` are absent from the working tree.
- [ ] `git diff --name-only` for the feature branch contains no entry for `QuickFiler/QuickFiler.csproj`.

### #467 — `ProcessCmdKey` swallows Alt mnemonics (live; remedy GUARD)

- [x] `EfcViewer` declares `internal static bool ClaimsAltChord(IQfcKeyboardHandler handler, Keys keyData)`, and `ProcessCmdKey` returns `true` only when it returns `true`.
- [x] A named test asserts `ClaimsAltChord` returns `true` for a bare Alt chord with a non-null handler.
- [x] A named test asserts `ClaimsAltChord` returns `false` for `Keys.Alt | Keys.F` and for `Keys.Alt | Keys.M`, so both mnemonics reach `base.ProcessCmdKey`.
- [x] A named test asserts `ClaimsAltChord` returns `false` for a non-Alt chord.
- [x] A named test asserts `ClaimsAltChord` returns `false` when the handler is null.
- [x] No test in `EfcViewerTests.cs` constructs, shows, or derives from a `System.Windows.Forms.Form`.
- [ ] The QFC twin is unchanged: `git diff --name-only` contains no entry for `QuickFiler/Viewers/QfcFormViewer.cs` or `QuickFiler/Controllers/QfcFormKeyHandler.cs`.

### Cross-cutting

- [ ] A Phase 0 baseline exists under `docs/features/active/efc-controller-surface-defects-464/evidence/baseline/` recording pre-change analyzer diagnostics, nullable/type-check results, `QuickFiler.Test` pass/fail tally, and coverage.
- [ ] `dotnet tool run csharpier check .` reports no formatting differences.
- [ ] The analyzer build introduces **no new diagnostics relative to the Phase 0 baseline**.
- [ ] The nullable/type-check build (CI's exact command, without `/p:Nullable=enable`) introduces **no new errors relative to the Phase 0 baseline**.
- [ ] `vstest.console.exe` reports no failure that is not in the Phase 0 `BASELINE_FAILED` set, and the passing-test count is **greater than the Phase 0 baseline count** by at least the number of tests this feature adds.
- [ ] No pre-existing `[TestMethod]` is deleted or renamed, and no assertion in a pre-existing test is weakened.
- [ ] Each test file this feature **creates** is under 500 lines. (The ceiling is asserted only over created files; `EfcFormController.cs` and `EfcItemController.cs` are pre-existing violations and are explicitly out of scope.)
- [ ] `EfcFormController.cs` has at most 1204 lines after the change — its 1084-line merge-base count plus at most 120 net lines for the RC1 guards, the five RC3 boundary extractions, the RC3 `PopulateFolderCombobox` try/catch, and the RC7/RC8/RC9 pure helpers — and the file-size evidence artifact itemises the delivered net delta per remedy against the merge-base count. (The 500-line ceiling is not asserted: this file is a pre-existing violation whose splitting is out of scope.)
- [ ] `EfcItemController.cs` has **fewer** lines after the change than its 1170-line merge-base count.
- [ ] The diff for `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs` is **exactly one changed line**, at the incognito literal.
- [ ] `git diff --name-only` for the feature branch intersected with the sibling-owned path set in §`Scope & Non-Goals` is **empty**.
- [ ] `EfcFormController.cs:834-837` (the `new WebView2BreadcrumbHost(...)` construction feature #476 depends on) is not moved or reshaped.
- [ ] No new `[ExcludeFromCodeCoverage]` attribute is added anywhere in the diff.
- [ ] No interface file is modified: the diff contains no path under `QuickFiler/Interfaces/`.
- [ ] The tokens `Thread.Sleep` and `Task.Delay` appear in no test file this feature writes.
- [ ] No test this feature writes creates a temporary file, contacts a live Outlook instance, starts a `BackgroundWorker`, or shows a WinForms form.
- [ ] `docs/features/active/efc-controller-surface-defects-464/user-story.md` does not exist. (Work mode is `full-bug`; this spec is the sole AC source.)

---

## Risks & Mitigations

### R-1 — Upstream 484's spec contains a factual error about `EfcItemController` (D9)

**Risk.** `484/spec.md:369-370` and `:226-227` state that `ToggleNavigation(bool async)` "is retained
specifically because it is declared on `IQfcItemController.cs:89` and implemented by
`EfcItemController.cs:958`." **This is not supported by the source.** `EfcItemController` is declared
`internal class EfcItemController : IItemControler` (`EfcItemController.cs:26`), and `IItemControler`
(`QuickFiler/Interfaces/IItemControler.cs`) declares **exactly three** members — `CounterEnter`,
`CounterComboRight`, `RightKeyActions`. `EfcItemController` does **not** implement `IQfcItemController`.
The member at `EfcItemController.cs:958` is a coincidentally same-named method, not an interface
implementation, and research §Q7.8 established it is itself dead: `EfcFormController.cs:927` and `:943`
call the **two-argument** overload `ToggleNavigation(bool, Enums.ToggleState)` (`:981-994`).

**Impact.** No interface obligation protects the dead EFC overloads. This is what makes RC11's deletion
remedy safe: `EfcFormController` declares `private EfcItemController _itemController;`
(`EfcFormController.cs:142`) — a **concrete** type — so every call binds directly to the class and no
interface dispatch can reach a removed member.

**Mitigation.** This feature **retains `EfcItemController.ToggleNavigation(bool)` untouched** despite it
being dead, and **must retain `ToggleNavigation(bool, Enums.ToggleState)`, which is LIVE**. Deleting the
single-argument overload on the strength of a reading that a sibling feature has publicly relied on the
opposite of would be a cross-feature hazard out of proportion to the ~20 lines saved. The disagreement is
recorded here as a documented upstream-contract disagreement and reported to 484's owner. **It does not
block this feature.**

### R-2 — Disagreement with feature 444's spec about `CharActions` reachability (D10)

**Risk.** `444/spec.md:234` states that `CharActions` is "reached only from the Alt-key `ProcessCmdKey`
path". Research §Q7.6 found its only reader is `KeyboardHandler.KeyboardHandler_KeyDown`
(`KeyboardHandler.cs:114-131`), whose only call sites are in files with **no `<Compile Include>` entry**
(`QfcFormViewerDark.cs:48`, `QfcFormViewerExpanded.cs:48`); the compiled `QfcFormViewer.cs:68` calls
`ToggleKeyboardDialogAsync()` instead. **`CharActions` has no compiled reader.**

**Impact on this feature.** It reinforces the RC4 deletion remedy — the `'B'`/`'D'` registrations were
unreadable as well as unreachable — and it is the second independent reason RC10's narrowing is safe:
narrowing the Alt claim cannot strand a `CharActions` entry, because nothing reads them on a compiled
path.

**Mitigation.** Recorded and reported to 444's owner. This feature does not act on it, does not edit
`KeyboardHandler.cs` (owned by #498), and does not edit `KbdActions.cs` (owned by #444).

### R-3 — The remedy set is deletion-heavy

**Risk.** Seven members, one field, and three files are deleted. A deletion is irreversible within the
branch and a wrong deletion breaks the build or, worse, silently removes a behavior that a call path this
analysis missed depends on.

**Mitigations, in order of strength.**

1. **Every deletion is backed by an individually verified zero-call-site finding** at `036a205d`, listed
   per member in §RC11 with the search that established it — not by a global "looks unused" judgment.
2. **The type is concrete at the call site.** `EfcFormController._itemController` is
   `EfcItemController` (`EfcFormController.cs:142`), so no interface dispatch route exists to any removed
   member, and `EfcItemController` never sets `_itemViewer.Controller` (commented at `:129`).
3. **The compiler is the backstop for reachability.** Any surviving caller of a removed `internal` or
   `private` member is a compile error, caught at toolchain step 2 before any test runs.
4. **Removals are sequenced first**, so no later phase edits a member an earlier phase deletes and no
   correction is lost to a subsequent deletion.
5. **The one member whose deletion would have been contested — `ToggleNavigation(bool)` — is retained**
   (R-1), so the deletion set contains no item on which a sibling feature has stated a dependency.
6. **Behavior preservation is asserted positively, not just by absence.** The Edit Filters criterion and
   the `PopulateConversation`/`UpdateUI` criterion each pin a surviving live route, so a deletion that
   removed real behavior fails a test rather than passing silently.

### R-4 — `EfcItemControllerTests.cs` file-size risk

**Risk.** Six sub-defect groups with Moq arrange blocks land in a file that does not yet exist. Research
rates this **AT RISK** of exceeding the 500-line ceiling, and discovering that mid-execution forces a
disruptive split after tests are already written and wired into the `.csproj`.

**Mitigation.** **Plan for TWO item-side test files from the outset**, as research recommends:
`EfcItemControllerTests.cs` (#459, #461, #463, #464 D/E, #466 structural) and
`EfcItemController.CleanupTests.cs` (#460 A/B/C). Both `<Compile Include>` entries are added in the same
contiguous `.csproj` insertion, so the split costs nothing if it turns out to be unnecessary and saves a
rework cycle if it does not. The precedent for a small focused viewer-test file is
`QfcFormKeyHandlerTests.cs` at 67 lines for four methods.

### R-5 — Stale line citations in four sibling specs

**Risk.** #498's, #476's, #444's and the eight promoted potentials' `EfcFormController.cs` citations are
uniformly stale by ±2 to ±4 lines because PR #605 landed an independent fix for issue #439 in that file.
Carrying a stale number forward would send an executor to the wrong line.

**Mitigation.** Every citation in this spec was re-read from the working tree at `036a205d`; none is
carried forward from `issue.md`, from a promoted document, or from a sibling spec without re-reading.
The specific stale ranges are enumerated in research §Q11.2 items 5-7 and §1.1, and the plan must not
consume any number from those sources.

### R-6 — Textual conflict on `QuickFiler.Test.csproj`

**Risk.** Features #444 and #476 each append one line to the same `<Compile Include>` item group.

**Mitigation.** Insert contiguously after line 112 rather than at the group's end. This reduces but does
not eliminate the risk; the conflict, if it occurs, is a trivial three-line resolution with no semantic
content.

---

## Rollout & Follow-up

### Release / rollout steps

1. Phase 0 baseline capture into `evidence/baseline/`.
2. Deletions (RC11, RC6, RC4-by-removal, RC5's dead site) with their structural tests first.
3. RC5's two live corrections, including the isolated one-line `ViewerSetup.cs` edit.
4. RC1 across both controllers plus RC2.
5. RC3 boundary extraction.
6. RC7, RC8, RC9 with their pure helpers.
7. RC10 with the extracted predicate.
8. Full toolchain, then PR against the epic integration branch
   `epic/quickfiler-bug-family-integration`.

### Post-fix monitoring and clean-up

- The two manual Alt-chord checks in §`Test Strategy`.
- Confirm no `EfcViewer.Designer.cs` regeneration reintroduces a viewer-side `EditFiltersMenuItem.Click`
  wiring, which was the latent trap RC11-A removes.

### Follow-ups to promote as separate issues (do not absorb into this feature)

1. **Delete the seventeen other uncompiled `QuickFiler/Viewers/*.cs` orphans**, plus
   `QuickFiler/Legacy/**` and `QuickFiler/Notes/**`, which are wholly uncompiled. Repository hygiene, not
   a bug fix.
2. **Decide the intended `KbdActions<>` indexer-setter contract** (upsert versus assign-if-present) and
   align it. Belongs with the owner of `KbdActions.cs`.
3. **Consolidate the fifth banner-prefix constant** at `FolderSuggestionTree.cs:16` with
   `BreadcrumbRowBuilder.BannerPrefix`.
4. **Fix the shared `ProcessCmdKey` over-claim in the QFC twin** (`QfcFormViewer.cs:56-73` /
   `QfcFormKeyHandler.cs:18`), which this feature deliberately does not touch.
5. **Correct `484/spec.md`'s `ToggleNavigation(bool)` retention rationale** (R-1) and
   **`444/spec.md`'s `CharActions` reachability claim** (R-2).
6. **Resolve the coverage-threshold discrepancy** between CLAUDE.md (80% / 90%) and
   `.claude/rules/general-unit-test.md` (85% / 75%).

### Links

- Issue: https://github.com/drmoisan/TaskMaster/issues/464
- Also closes: #459, #460, #461, #463, #465, #466, #467
- Requirement source: `docs/features/active/efc-controller-surface-defects-464/issue.md`
- Research: `docs/features/active/efc-controller-surface-defects-464/research/2026-08-25T12-20-efc-controller-surface-defects.md`
- Upstream contract: `docs/features/active/qfc-item-controller-defects-484/spec.md` (line 329, line 634)
- Promoted potentials: the eight `docs/features/potential/promoted/2026-08-07-efc-*.md` and
  `2026-08-07-quickfiler-webview2-incognito-arg-en-dash.md`
