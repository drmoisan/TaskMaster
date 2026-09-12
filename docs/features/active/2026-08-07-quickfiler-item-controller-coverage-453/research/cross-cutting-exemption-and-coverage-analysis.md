# F10 (#453) Cross-Cutting Research — Exemption Boundary, Branch Coverage, and Existing Test Inventory

- Epic: #136 `quickfiler-per-file-coverage`, child F10 `quickfiler-item-controller-coverage` (issue #453)
- Branch: `feature/quickfiler-item-controller-coverage`
- Worktree: `C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-a359b62de7a79b16e`
- Research date: 2026-08-07
- Companion artifact: `open-issues-and-sibling-boundaries.md` (same folder)

All findings below were read directly from the files on this branch. Every claim carries a
`file:line` citation. Where a claim in `epic.md` or in the delegation brief is contradicted by the
code, the contradiction is stated explicitly under **CORRECTION**.

**Tooling limitation for this session.** No Bash/PowerShell tool was available. All figures were
derived by reading files with `Read`/`Grep`; no build, no test run, and no `gh` invocation was
possible. Coverage figures are read from a committed Cobertura artifact and are **indicative only**
(see §2.1).

---

## 1. The `[ExcludeFromCodeCoverage]` contradiction — resolved

### 1.0 CORRECTION to `epic.md`: no partial in the F10 family is file-level exempt

`epic.md:395-399` marks six of the ten `QfcItemController` partials with `[X]` and states at
`epic.md:399` that "six of ten partials are currently exempted". **This is incorrect.**

Verified: `grep -n ExcludeFromCodeCoverage QuickFiler/Controllers/QfcItemController*.cs` returns
exactly 19 hits, all at member level, all inside a class body (indentation of 8 spaces, immediately
preceding a method declaration). No occurrence sits on a `partial class` declaration. This is
consistent with the C# rule that annotating a partial type in two files would be **CS0579
(duplicate attribute)** — but it is confirmed positively here, not merely inferred.

The observable consequence is decisive and it is the resolution of the contradiction:

- Because the attribute is member-level, **all ten partials are instrumented** and all ten appear in
  the Cobertura report as `<class>` elements (verified: `coverage-final.cobertura.xml` lines 22740,
  23126, 23519, 24004, 24222, 24601, 25411, 25754, 26058, 26662). The real percentages the epic saw
  are therefore genuine — they are just computed over a denominator from which the 19 exempted
  member bodies have been removed.
- The `[X]` markers in `epic.md` must be read as "this file contains at least one exempted member",
  not "this file is exempt". F1's ledger should record the disposition per **member**, not per file,
  for this family.

### 1.1 Second, non-obvious instrumentation fact: the attribute does not reach lambdas

`[ExcludeFromCodeCoverage]` on a method **does not** propagate to lambdas/closures declared inside
that method under the instrumentation used to produce the committed report. Positive proof, read
directly from the artifact:

`coverage-final.cobertura.xml:23308-23332` emits four closure methods under
`QfcItemController.Initialization.cs` whose containing methods are all exempt:

```
<method line-rate="0" ... name="&lt;InitializeAsync&gt;b__115_0" ...>   -> line 253, hits 0
<method line-rate="0" ... name="&lt;InitializeGraphicsAsync&gt;b__116_0" -> line 264, hits 0
<method line-rate="0" ... name="&lt;InitializeGraphicsAsync&gt;b__116_1" -> lines 267-272, hits 0
<method line-rate="0" ... name="&lt;InitializeSequentialAsync&gt;b__117_0" -> line 297, hits 0
```

The same effect appears in `QfcItemController.ViewerSetup.cs` (lines 82-102, the
`WebResourceRequested` lambda inside the exempt `InitializeWebViewAsync`, and lines 276-306, the
LINQ/`ForEach` lambdas inside the exempt `ResolveControlGroupsAsync`) and in
`QfcItemController.Navigation.cs` (lines 197 and 202, the two `_uiDispatcher.InvokeAsync` lambdas
inside the exempt `ToggleExpansionAsync(ToggleState)`).

Planning consequence: **those lambda lines are already in the denominator and already counted as
uncovered.** Removing the attribute does not add them; it adds only the enclosing method's own
statement lines.

### 1.2 The 19 attribute sites — full record and classification

Classification key (per `epic.md` "Shared Design § 1", irreducible-remainder standard, seam
hierarchy interface > injectable delegate > adapter):

- `removable-as-is` — the member is already reachable with fixtures that exist in
  `QuickFiler.Test` today; the attribute is gratuitous.
- `removable-with-seam` — reachable after a bounded, F10-local seam or extraction.
- `irreducible-candidate` — a genuine barrier remains that F10 cannot remove within its own file
  assignment.

`Δlines` is the estimated number of coverable line entries the member adds to its file's
denominator when the attribute is removed, counting non-blank, non-comment physical lines in the
member span and excluding lines already present (lambda lines per §1.1). Signature lines are
excluded except where they carry default parameter values, following the emission pattern observed
at `coverage-final.cobertura.xml:23181-23234`. Estimates are ±2 lines.

#### `QfcItemController.Conversation.cs`

| # | Site | Member | Span | Δlines | Class |
|---|---|---|---|---|---|
| 1 | `:79` | `protected virtual Task<ConversationResolver> DoLoadConversationResolverCoreAsync(CancellationTokenSource tokenSource, CancellationToken token, bool loadAll)` | 80-92 | +8 | `removable-with-seam` |

What it does: a one-expression override point that forwards to the static
`ConversationResolver.LoadAsync(_globals, ItemHelper, tokenSource, token, loadAll, SetTopicThread)`
(the `MailItemHelper` overload at `QuickFiler/Helper Classes/ConversationResolver.cs:126`).

Rationale: the barrier is real but mis-sized. Every existing consumer test
(`QfcItemController.ConversationTests.cs:56,77,100`; `QfcItemControllerTests.cs:58,87,116,144`)
subclasses the controller and overrides this method, so the base body is structurally unreachable
from a test. That is a *design* consequence, not a host barrier: replacing the virtual-override seam
with an injectable delegate (`Func<CancellationTokenSource, CancellationToken, bool,
Task<ConversationResolver>>`, defaulted in `SaveParameters` exactly as the other six factory seams
are at `QfcItemController.Initialization.cs:380-397`) reduces the unreachable remainder from 8 lines
to a single default-lambda line, which needs no attribute — it is simply uncovered, like the
existing `_conversationResolverFactory` default at `Initialization.cs:382-388`.

#### `QfcItemController.EventHandlers.cs` — five `async void` shells

| # | Site | Member | Span | Δlines | Class |
|---|---|---|---|---|---|
| 2 | `:60` | `internal async void BtnPopOut_Click(object sender, EventArgs e)` | 61-68 | +7 | `removable-as-is` |
| 3 | `:83` | `internal async void BtnReply_Click(object sender, EventArgs e)` | 84-91 | +7 | `removable-as-is` |
| 4 | `:97` | `internal async void BtnReplyAll_Click(object sender, EventArgs e)` | 98-105 | +7 | `removable-as-is` |
| 5 | `:111` | `internal async void BtnForward_Click(object sender, EventArgs e)` | 112-119 | +7 | `removable-as-is` |
| 6 | `:125` | `internal async void TxtboxBody_DoubleClick(object sender, EventArgs e)` | 126-133 | +7 | `removable-as-is` |

Each does the same two things: install a `WindowsFormsSynchronizationContext` when the ambient
context is null, then `await` its already-tested `*Core()` counterpart (`BtnPopOutCore` at `:70`,
`BtnReplyCore` at `:93`, `BtnReplyAllCore` at `:107`, `BtnForwardCore` at `:121`,
`TxtboxBodyDoubleClickCore` at `:135`).

Rationale — direct in-file sibling inconsistency, the pattern this repository has twice rejected
(see the #227 cycle-2 precedent): `BtnDelItem_Click` (`:72-79`) and `BtnFlagTask_Click` (`:49-56`)
have the *identical* shape (same guard + one delegated call) and are **not** exempt, and both are
tested (`QfcItemController.EventHandlersTests.cs:241` and `:272`). The only structural difference is
`async void` + `await`. An `async void` method can be invoked directly from a test method; because
each core routes through a mock (`_parent`, `_mailActions`, `_uiDispatcher`) that returns an already
completed `Task`, the continuation runs synchronously before the call returns, so there is no
unobserved-exception hazard and no timing dependency. Coverage parity is exact: the not-exempt
siblings' guard lines (`:52-54`, `:75-77`) are themselves uncovered today, so removing the five
attributes puts these members in exactly the same, already-accepted position.

#### `QfcItemController.EventWiring.cs`

| # | Site | Member | Span | Δlines | Class |
|---|---|---|---|---|---|
| 7 | `:99` | `internal async void WebView2Control_CoreWebView2InitializationCompleted(object sender, CoreWebView2InitializationCompletedEventArgs e)` | 100-106 | +3 | `removable-as-is` |

What it does: destructures the WebView2 event args and forwards to the already-tested
`HandleWebViewInitializedAsync(bool, Exception)` at `:108`
(`QfcItemController.SeamCoreTests.cs:167,187,208`).

Rationale: the stated barrier is the sealed, publicly non-constructible
`CoreWebView2InitializationCompletedEventArgs`. A proven in-repo technique defeats it:
`QfcItemControllerBreadcrumbDropDownTests.cs:30-31` already fabricates the equally sealed
`CoreWebView2Environment` with
`(CoreWebView2Environment)FormatterServices.GetUninitializedObject(typeof(CoreWebView2Environment))`.
The same call plus reflective backing-field assignment produces an args instance with the two
properties this method reads. Three lines; no new production code.

#### `QfcItemController.Initialization.cs` — seven sites

| # | Site | Member | Span | Δlines | Class |
|---|---|---|---|---|---|
| 8 | `:138` | `private void Initialize(IApplicationGlobals, IFilerHomeController, IQfcCollectionController, IItemViewer, int, int, MailItem, TlpCellStates, bool)` | 139-163 | **-13 (delete)** | `removable-as-is` — **dead member** |
| 9 | `:168` | `public void Initialize(bool async)` | 169-195 | +14 | `removable-with-seam` |
| 10 | `:200` | `public async Task InitializeAsync()` | 201-256 | +24 | `irreducible-candidate` |
| 11 | `:260` | `public async Task InitializeGraphicsAsync()` | 261-287 | +16 | `irreducible-candidate` |
| 12 | `:291` | `public async Task InitializeSequentialAsync()` | 292-322 | +22 | `irreducible-candidate` |
| 13 | `:403` | `public static async Task<QfcItemController> CreateAsync(IApplicationGlobals, IFilerHomeController, IQfcCollectionController, ItemViewer, int, int, MailItem, TlpCellStates, CancellationToken)` | 404-431 | **-16 (delete)** | `removable-as-is` — **dead member** |
| 14 | `:436` | `public static async Task<QfcItemController> CreateSequentialAsync(... same shape ...)` | 437-464 | **-16 (delete)** | `removable-as-is` — **dead member** |

**Call-site verification (solution-wide grep) — three of these seven members are dead.** The
disposition for a dead member is deletion, which *removes* its lines from the denominator rather
than adding them.

| Member | Live call sites |
|---|---|
| `Initialize(9 params)` (private, `:139`) | **none.** `Initialization.cs:162` is inside its own body. |
| `Initialize(bool async)` | **live**: `QfcCollectionController.cs:813, 1897, 1945` |
| `InitializeAsync()` | **live**: `QfcQueue.cs:415`; `QfcCollectionController.cs:790, 854` |
| `InitializeGraphicsAsync()` | **live**: `QfcCollectionController.cs:384, 479` |
| `InitializeSequentialAsync()` | **live**: `QfcCollectionController.cs:692` |
| `CreateAsync` | **none** |
| `CreateSequentialAsync` | **none** |

- **#8** is a private method with zero callers. Recommended disposition: **delete** (removes 13
  lines from the file and one exemption), subject to the #447 precedent's condition — confirm no
  reflection-based caller first.
- **#13 and #14** are `public static` factories on an `internal` class with zero callers anywhere in
  the solution. Recommended disposition: **delete** (removes 32 lines and two exemptions), same
  reflection caveat. Deleting them also removes two of the six #230-blocked sites, reducing the
  #230-dependent residual from six to **four** (sites 10, 11, 12, 19).
- The three deletions are behaviour-preserving by construction (unreachable code) and so are
  compatible with the epic's no-behaviour-change NFR. If the orchestrator prefers to keep scope
  minimal, the fallback is to leave them in place and retain their attributes; but they must not be
  *tested*, since writing tests for unreachable production code manufactures coverage.
- **#9** orchestrates `ResolveControlGroups((ItemViewer)_itemViewer)` (`:172`),
  `QfcThemeHelper.SetupThemes(...)` (`:175-180`), `PopulateControls` (`:183`), `ToggleTips` (`:186`),
  `ToggleNavigation` (`:187`), `WireEvents()` (`:190`) and a fire-and-forget
  `_itemViewer.UiDispatcher.InvokeAsync(InitializeWebViewAsync)` (`:193`). **The stated barrier is
  partly stale**: `ResolveControlGroups` was de-exempted in cycle 5 and is covered by a headless
  real-`ItemViewer` test (`QfcItemController.ViewerSetup.cs:204`;
  `QfcItemController.ViewerSetupTests.cs:379`), and `WireEvents`/`WireControlTreeEvents` likewise
  (`QfcItemController.EventWiring.cs:27,34`; `QfcItemController.EventWiringTests.cs:229,320`). The
  method is **synchronous** and never awaits `IItemViewer.UiSyncContext`, so the issue-#230 deadlock
  hazard does not apply to it. Residual barriers: `QfcThemeHelper.SetupThemes` (F4-owned file, must
  not be edited by F10) must tolerate a headless viewer, and the `:193` fire-and-forget must not
  reach live WebView2 — both addressable with an F10-local seam (a `Func<Task>` web-view bootstrap
  delegate defaulted in `SaveParameters`).
- **#10, #11, #12** all drive `ResolveControlGroups*`/`SetupThemes` against the concrete viewer and,
  in #10's case, `await ResolveControlGroupsAsync(...)` which awaits `itemViewer.UiSyncContext`
  (`ViewerSetup.cs:265`). This is precisely the barrier tracked by **open issue #230** ("Build a
  WinForms message-pump test seam (`Application.Run()` background thread) to unblock 9
  `QfcItemController` orchestration members"), which names these members explicitly. Classify
  `irreducible-candidate` **pending #230**; F10 cannot build a general WinForms pump seam inside its
  own file assignment.
- **#13, #14** are dead (see the call-site table above). Even if retained, their barrier would be
  wholly inherited from #10/#12 rather than independent; the ledger must not record them as
  independent justifications.

#### `QfcItemController.Navigation.cs`

| # | Site | Member | Span | Δlines | Class |
|---|---|---|---|---|---|
| 15 | `:173` | `public virtual void ToggleExpansion(Enums.ToggleState desiredState)` | 174-187 | +13 | `removable-as-is` |
| 16 | `:191` | `public virtual async Task ToggleExpansionAsync(Enums.ToggleState desiredState)` | 192-205 | +11 | `removable-as-is` |

**CORRECTION — the justification comments at `Navigation.cs:171-172` and `:189-190` are stale.**
Both say the body is "(TlpCellSnapShot-bound, out-of-scope) state-taking". That was true when
`ApplyState` took a concrete `Control`. It is no longer true on this branch:

- `QuickFiler/Helper Classes/TlpCellSnapShot.cs:72` — `public void ApplyState(IContainerControlLocal root)`
- `QuickFiler/Helper Classes/TlpCellSnapShot.cs:192` — `public void ApplyState(IContainerControlLocal root)`
- `QuickFiler/Viewers/IItemViewer.cs:15` — `public interface IItemViewer : IUserControl, IContainerControlLocal`
- `QfcItemController.Navigation.cs:209,219` — `_tlpStates["Compressed"|"Expanded"].ApplyState(_itemViewer)`

`ToggleExpansionOff` and `ToggleExpansionOn` are therefore already directly unit-tested against a
`Mock<IItemViewer>` (`QfcItemController.NavigationTests.cs:292` and `:345`). The remaining
collaborators of the two exempt overloads are `_parent.ToggleExpansionStyle[Async]`
(`Mock<IQfcCollectionController>`), the `Register/UnregisterExpanded[Async]Actions` pair (already
tested at `QfcItemController.EventWiringTests.cs:89,104,185,200`), and `_uiDispatcher.InvokeAsync`,
which `QfcItemControllerTestSupport.BuildSyncDispatcher()` (`TestSupport.cs:102-137`) already runs
synchronously. **No barrier remains.** The overloads were made `virtual` only so the existing
parameterless-overload routing tests (`NavigationTests.cs:230,244,258,272`) could stub them; that is
a test-convenience decision, not an irreducible-remainder justification.

#### `QfcItemController.ViewerSetup.cs`

| # | Site | Member | Span | Δlines | Class |
|---|---|---|---|---|---|
| 17 | `:38` | `internal async Task InitializeWebViewAsync()` | 39-125 | +36 | `removable-with-seam` (partial) |
| 18 | `:132` | `internal void EnsureBreadcrumbPipeline()` | 133-158 | +23 | `removable-as-is` |
| 19 | `:253` | `internal async Task ResolveControlGroupsAsync(ItemViewer itemViewer)` | 254-307 | +21 | `irreducible-candidate` |

- **#17** awaits `_itemViewer.UiSyncContext` (`:55`) and dereferences
  `((ItemViewer)_itemViewer).L0v2h2_WebView2.CoreWebView2` (`:76`), which requires a WebView2 core
  that has actually completed initialization. The method as written is a genuine residual. However,
  the substantive logic in its `WebResourceRequested` lambda (`:81-102`) is pure and is *already*
  in the denominator, uncovered: it resolves a request URI to an attachment via
  `CidImageResolver.BuildContentIdMap` and picks a MIME type. `CidImageResolver` is a **public**
  static class (`UtilitiesCS/OutlookObjects/MailItem/CidImageResolver.cs:14,20,34`) returning
  `IReadOnlyDictionary<string, IAttachment>` over the public `IAttachment` interface
  (`UtilitiesCS/Interfaces/IEmailIntelligence/IAttachment.cs:8,13`), so extracting that decision
  logic into a named internal method taking `(string requestUri, IEnumerable<IAttachment>)` makes
  ~15 of the 21 lambda lines testable with no host dependency. Recommended disposition: keep a
  narrow attribute on the residual WebView2 wiring only, after extraction.
- **#18** is gratuitous. Its first statement is a type test —
  `if (!(_itemViewer is ItemViewer viewer)) { return; }` (`:135-138`) — so a `Mock<IItemViewer>`
  exercises the early-return path with zero host risk, and the repository already has a headless real
  `ItemViewer` fixture (`QfcItemControllerBreadcrumbDropDownTests.cs:365-383`, `ViewerScope`) that
  reaches the rest. The event subscribe/unsubscribe half is already exercised indirectly by
  `OnBreadcrumbUnhandledArrow_ForViewer_RoutesOnceToKeyboardHandler`
  (`QfcItemControllerBreadcrumbDropDownTests.cs:156`). The only new mock needed is
  `Mock<IOlObjects>.FolderTreeService` for `:142-144`.
- **#19** awaits `itemViewer.UiSyncContext` at `:265`. Same #230 barrier as #10. Note its *sync*
  twin `ResolveControlGroups` was de-exempted in cycle 5 (`:204`), and 17 of #19's lambda lines are
  already counted uncovered — so the attribute is buying only 21 lines of concealment.

### 1.3 Classification totals

| Class | Count | Sites |
|---|---|---|
| `removable-as-is` | **12** | 2, 3, 4, 5, 6 (EventHandlers shells); 7 (EventWiring); 15, 16 (Navigation); 18 (EnsureBreadcrumbPipeline); **8, 13, 14 (dead members — remove by deletion)** |
| `removable-with-seam` | **3** | 1 (DoLoadConversationResolverCoreAsync); 9 (`Initialize(bool)`); 17 (InitializeWebViewAsync, partial extraction) |
| `irreducible-candidate` | **4** | 10, 11, 12 (async init orchestration); 19 (ResolveControlGroupsAsync) |

All four `irreducible-candidate` sites share one root cause and one upstream remedy: **open issue
#230**, which names these members explicitly. If #230 lands before or during F10, the defensible
residual for this family is **zero**. If it does not, F10's honest ledger position is: **15
attributes removed** (12 by de-exemption or deletion, 3 after a bounded F10-local seam) and **4
retained** with a single shared, externally-tracked justification.

### 1.4 Denominator budget — the cost of removing the attributes

This is the answer to "quantify how many lines each exempted member adds to the denominator".
Baseline figures are the true per-file union figures from §2.

| File | Now: covered / total | Now % | Δlines added | After removal, no new tests | After % |
|---|---|---|---|---|---|
| `QfcItemController.cs` | 73 / 73 | 100.00 | 0 | 73 / 73 | 100.00 |
| `.Initialization.cs` | 123 / 134 | 91.79 | **+76** | 123 / 210 | 58.57 |
| `.ViewerSetup.cs` | 116 / 160 | 72.50 | **+80** | 116 / 240 | 48.33 |
| `.Conversation.cs` | 90 / 102 | 88.24 | **+8** | 90 / 110 | 81.82 |
| `.FolderHandling.cs` | 129 / 147 | 87.76 | 0 | 129 / 147 | 87.76 |
| `.EventWiring.cs` | 247 / 303 | 81.52 | **+3** | 247 / 306 | 80.72 |
| `.EventHandlers.cs` | 74 / 93 | 79.57 | **+35** | 74 / 128 | 57.81 |
| `.Navigation.cs` | 107 / 118 | 90.68 | **+24** | 107 / 142 | 75.35 |
| `.FocusAndTheme.cs` | 176 / 237 | 74.26 | 0 | 176 / 237 | 74.26 |
| `.MailActions.cs` | 96 / 125 | 76.80 | 0 | 96 / 125 | 76.80 |
| **Family total** | **1231 / 1492** | **82.51** | **+226** | **1231 / 1718** | **71.65** |

The `Δlines` column already accounts for deleting the three dead members (sites 8, 13, 14): their
lines are excluded from instrumentation today, so deletion neither adds nor removes denominator
lines — it simply removes three exemptions at zero coverage cost. That is why the Initialization
delta is +76 (sites 9-12 only) rather than +121, and the family delta is +226 rather than +271.

The brief's expectation is confirmed and quantified: removing all 19 attributes with no new tests
drops the family from 82.51% to 71.65%, and drops four files that pass today
(`.Initialization.cs`, `.Conversation.cs`, `.EventWiring.cs`, `.Navigation.cs`) at or below the
80% line. **The plan must budget new tests against the post-removal denominator, not the current
one.** Removing an attribute is only safe when paired, in the same phase, with the tests that cover
the newly exposed lines.

Ordering recommendation that follows directly from the table:

1. **Delete** sites 8, 13, 14 (three dead members, three exemptions, zero coverage cost, no new
   tests) after a reflection-caller check.
2. Remove the nine de-exemptable attributes (sites 2-7, 15, 16, 18 — +80 lines, all coverable with
   fixtures that exist today) together with their tests in the same phase.
3. Add the two additive F10-local seams and remove sites 1 and 9 and narrow site 17.
4. Retain sites 10, 11, 12, 19 with **open issue #230** as the single named justification.

---

## 2. Branch coverage (75% gate) versus line coverage (80% gate)

### 2.1 Source, staleness, and a correctness finding about the artifact

Source: `docs/features/active/2026-08-06-quickfiler-high-confidence-queue-init-stall-424/evidence/qa-gates/coverage-final.cobertura.xml`.

**Indicative only.** This artifact was captured on feature #424's branch, not on
`feature/quickfiler-item-controller-coverage`. It must not be cited as F10 acceptance evidence;
F1's harness run on this branch is the authority. A staleness spot-check passes: the first method
entry for `QfcItemController.cs` is `get_Buttons` at line 98
(`coverage-final.cobertura.xml:22742-22744`), and `QfcItemController.cs:98` is indeed
`get => _buttons;`. Line numbering is aligned with the current tree.

**Union requirement (epic.md "Directives for F1's Ledger and Harness", item 1) — checked and
satisfied trivially here.** A grep for `filename="QuickFiler\Controllers\QfcItemController` returns
exactly 10 matches, one per file, and a grep for `QfcItemController/` (the nested/closure-type name
form) returns **zero**. There is no second `<class>` element sharing any of these ten filenames, so
union-with-max-hits reduces to reading the single element. This was verified rather than assumed.

**Correctness finding — the `line-rate` / `branch-rate` attributes on `<class>` are not the per-file
figures.** The attribute value is computed over the union of the `<methods>` entries *plus* the
class-level `<lines>` entries, so every line that appears in both blocks is counted twice. Proved
twice, arithmetically exactly:

- `QfcItemController.Conversation.cs`: class-level `<lines>` block
  (`coverage-final.cobertura.xml:24082-24219`) holds 102 unique entries, 12 with `hits="0"`, i.e.
  **90/102 = 88.24%**. The `<methods>` block (`:24006-24079`) holds 34 further entries, 0 uncovered.
  Combined: (34+102-12)/(34+102) = 124/136 = **0.911765** — bit-for-bit the emitted
  `line-rate="0.911765"` at `:24004`. The same holds for branches: class block 17/18, methods block
  8/8, combined 25/26 = **0.961538** = the emitted `branch-rate`.
- `QfcItemController.Initialization.cs`: class block (`:23346-23516`) holds 134 entries, 11
  uncovered (123/134 = 91.79%). The `<methods>` block (`:23128-23343`) holds 139 entries, 16
  uncovered. Combined: 246/273 = **0.901099** = the emitted `line-rate="0.901099"` at `:23126`.

Note the direction of the error is **not consistent**: Conversation is over-reported (91.18% vs a
true 88.24%) because covered lines are the ones duplicated, while Initialization is under-reported
(90.11% vs a true 91.79%) because the four uncovered closure methods contribute uncovered entries
that the class-level union masks by taking max-hits. This is the same defect as **open issue #441**
("Cobertura post-processing double-counts `<line>` nodes"), but #441's title asserts inflation only;
the evidence here shows it can deflate as well. See the companion artifact.

**Method used for every figure below**: read the class-level `<lines>` block only, count unique
`<line>` entries and `hits="0"` entries for lines, and sum the `condition-coverage="p% (c/t)"`
numerators and denominators for branches. This is the semantically correct per-file measure and is
what F1's harness must produce.

### 2.2 Per-file results

| File | Line cov/total | Line % | 80% line gate | Branch cov/total | Branch % | 75% branch gate | Cobertura attr. `line-rate` / `branch-rate` |
|---|---|---|---|---|---|---|---|
| `Controllers/QfcItemController.cs` | 73 / 73 | 100.00 | PASS | 11 / 14 | 78.57 | PASS | 1 / 0.785714 |
| `Controllers/QfcItemController.Initialization.cs` | 123 / 134 | 91.79 | PASS | 25 / 26 | 96.15 | PASS | 0.901099 / 0.961538 |
| `Controllers/QfcItemController.ViewerSetup.cs` | 116 / 160 | 72.50 | **FAIL** | 30 / 54 | 55.56 | **FAIL** | 0.743682 / 0.56 |
| `Controllers/QfcItemController.Conversation.cs` | 90 / 102 | 88.24 | PASS | 17 / 18 | 94.44 | PASS | 0.911765 / 0.961538 |
| `Controllers/QfcItemController.FolderHandling.cs` | 129 / 147 | 87.76 | PASS | 38 / 60 | 63.33 | **FAIL** | 0.896861 / 0.686275 |
| `Controllers/QfcItemController.EventWiring.cs` | 247 / 303 | 81.52 | PASS | 13 / 20 | 65.00 | **FAIL** | 0.81993 / 0.65625 |
| `Controllers/QfcItemController.EventHandlers.cs` | 74 / 93 | 79.57 | **FAIL** | 13 / 20 | 65.00 | **FAIL** | 0.795699 / 0.65 |
| `Controllers/QfcItemController.Navigation.cs` | 107 / 118 | 90.68 | PASS | 18 / 22 | 81.82 | PASS | 0.89071 / 0.766667 |
| `Controllers/QfcItemController.FocusAndTheme.cs` | 176 / 237 | 74.26 | **FAIL** | 40 / 66 | 60.61 | **FAIL** | 0.756032 / 0.576087 |
| `Controllers/QfcItemController.MailActions.cs` | 96 / 125 | 76.80 | **FAIL** | 16 / 22 | 72.73 | **FAIL** | 0.777778 / 0.75 |
| `Interfaces/IQfcItemController.cs` | **N/A** | N/A | N/A (see below) | N/A | N/A | N/A | no `<class>` element |

**Files failing the branch gate but passing the line gate — the answer to the brief's question:**

- `QfcItemController.FolderHandling.cs` — 87.76% line (PASS), **63.33% branch (FAIL)**
- `QfcItemController.EventWiring.cs` — 81.52% line (PASS), **65.00% branch (FAIL)**

**Files failing both:** `.ViewerSetup.cs`, `.EventHandlers.cs`, `.FocusAndTheme.cs`,
`.MailActions.cs`.

**Files passing both:** `QfcItemController.cs`, `.Initialization.cs`, `.Conversation.cs`,
`.Navigation.cs` — every one of which is pushed below the line gate by attribute removal per §1.4,
so none is safe to leave untouched.

`Interfaces/IQfcItemController.cs` (107 lines, `QuickFiler.csproj:365`) emits **no** `<class>`
element — verified: `filename="QuickFiler\Interfaces\IQfcItemController.cs"` has zero matches, and
the only three textual occurrences of `IQfcItemController` in the report are inside consumers'
`signature` attributes. It is a pure interface declaration with no executable IL and belongs in the
third ledger bucket (`interface-only / not-measured`, `epic.md` "Directives for F1's Ledger and
Harness"). It must be reported **N/A, never 0%**, and must not receive `[ExcludeFromCodeCoverage]`.
There is exactly one file of this name in the tree (`QuickFiler/Interfaces/IQfcItemController.cs`);
there is no orphan duplicate.

### 2.3 Uncovered-line map (planning input)

Line numbers are current-tree line numbers.

**`.ViewerSetup.cs` — 44 uncovered**
- `82-102` (17) — the `WebResourceRequested` lambda inside exempt `InitializeWebViewAsync`. Extractable (§1.2 #17).
- `178` (1) — `throw new ArgumentNullException(nameof(attachCollapsed))` in `ConfigureAndAttachBreadcrumbAsync`. One trivial test.
- `195-202` (8, plus branch `0/12` at `:195`) — `ResolveImageMimeType`, a **pure static** `switch` expression. Completely untested; the single highest-value target in the family (8 lines + 12 branch conditions for one table-driven test).
- `276-306` (17) — LINQ/`ForEach` lambdas inside exempt `ResolveControlGroupsAsync`. Blocked by #230.
- `424` (1) — `GetItemSummary()`. One trivial test.

**`.FocusAndTheme.cs` — 61 uncovered, and essentially all of it is reachable today**
- `37-39`, `53-55`, `93-95`, `109-111`, `142-144`, `157-159` (18) — the six `"Dark*"` theme-selection branches. Reachable by setting `_activeTheme` to a `"Dark..."` value via `QfcItemControllerTestSupport.InjectThemes`.
- `73-75` (3) — `await ToggleFocusOffAsync()` inside `ToggleFocusAsync(desiredState)`.
- `132-134` (3) — `await ToggleFocusOnAsync()` inside `ToggleFocusAsync()`.
- `172-174` (3) and `184-188` (5) — the `async == true` branches of both `ToggleNavigation` overloads.
- `211-213` (3) and `237-241`, `245` (6) — the `ListTipsExpanded` paths of `ToggleTips`/`ToggleTipsAsync`, gated on `_expanded || desiredState.HasFlag(Force)`.
- `229-231` (3) — the `foreach` body of `ToggleTipsAsync`; the only existing test uses empty collections (`FocusAndThemeTests.cs:382`).
- `283-286` (4) and `311-314` (4) — the `"DarkActive"` / `"LightActive"` else-branches of `SetThemeDark`/`SetThemeLight`.
- `292-300` (9) — `HtmlDarkConverter` body; gated on `_isWebViewerInitialized`, a private bool.

All of the above are private-field-gated branch selection. `QfcItemControllerTestSupport.SetField`
(`TestSupport.cs:37-47`) reaches every gate. **`.FocusAndTheme.cs` is the largest single win in the
family and needs no new production seam.**

**`.MailActions.cs` — 29 uncovered**
- `59`, `63-66`, `68` (6) and `77-79` (3) — the lambda bodies inside the `RightKeyActions` /
  `RightKeyActionsAsync` dictionary getters. The existing tests
  (`MailActionsTests.cs:107,122`) assert only that the keys are present. Invoking each dictionary
  value covers all nine lines. No new seam.
- `115-116`, `118-122` (7) — the `catch` in `MoveMailAsync`. **Blocked by `MessageBox.Show` at
  `:119-121`** (unit-test policy prohibits popups). Needs an F10-local dialog seam; see §4 and the
  latent-defect list.
- `176-181` (6) and `194-200` (7) — `FlagAsTask` / `FlagAsTaskAsync` beyond the factory call.
  Blocked because `_flagTasksFactory` returns the concrete `TaskVisualization.FlagTasks`, whose
  `Run(bool)` is neither virtual nor mockable (`TaskVisualization/FlagTasks.cs:88-104`) and whose
  constructor calls `globals.Ol.App.ActiveExplorer()` and can raise a `MessageBox`
  (`FlagTasks.cs:42-63`). The existing tests dodge this by throwing a `SentinelException` from the
  factory (`SeamFactoryTests.cs:110,135`), which is exactly why these lines are uncovered.

**`.EventHandlers.cs` — 19 uncovered**
- `30-32`, `52-54`, `75-77` (9) — the `SynchronizationContext.SetSynchronizationContext(new WindowsFormsSynchronizationContext())` guard bodies. Reachable by nulling the ambient context for the duration of one test; `ViewerScope` (`QfcItemControllerBreadcrumbDropDownTests.cs:365-383`) shows the save/restore pattern to copy.
- `37-46` (9, plus branches at `:36` and `:38`) — the `!SuppressEvents` body of `CbxConversation_CheckedChanged`, routing to `CollapseConversation`/`EnumerateConversation`. Both callees are already tested (`MailActionsTests.cs:137,157`; `SeamCoreTests.cs:81`). One test each covers all nine lines and both branches. **This single gap is what puts the file at 79.57%.**
- `56` (1) — the closing brace of `BtnFlagTask_Click`; uncovered because the existing test's `FlagAsTask` throws a sentinel.

**`.EventWiring.cs` — 56 uncovered**
- `125-137` (11, plus branches at `:124`, `:128`) — the `while (ItemHelper is null)` polling loop in `HandleWebViewInitializedAsync`, containing `await Task.Delay(newDelay)` at `:135`. Not deterministically testable without a delay seam; also a latent production defect (see §4).
- `162, 167, 172, 177, 182, 187, 195, 200, 206` (9) — lambda bodies registered by `RegisterFocusActions`.
- `224, 230-233, 238, 243, 248, 253, 258, 263, 269-272, 277, 282, 288, 293, 298` (21) — lambda bodies registered by `RegisterFocusAsyncActions`.
- `311, 316` (2) and `325, 330` (2) — lambda bodies registered by `RegisterExpanded[Async]Actions`. These four reference `((ItemViewer)_itemViewer).L0v2h2_WebView2` / `.TopicThread`, so they need the headless real-`ItemViewer` fixture, not a mock.
- `209-211`, `301-303`, `350-352`, `374-376` (12, plus branches at `:208`, `:300`, `:349`, `:373`) — the `if (_expanded)` tails of the four Register/Unregister methods. Reachable by setting `_expanded` via `SetField`.

The 32 registered-lambda lines are covered by **invoking** the registered delegate rather than only
asserting its presence. `KbdActions<TKey, UClass, VDelegate>` exposes `public VDelegate this[TKey key]`
(`QuickFiler/Controllers/KbdActions.cs:36-47`), and the existing fixture already builds real
`KbdActions` instances behind a `Mock<IQfcKeyboardHandler>`
(`QfcItemController.EventWiringTests.cs:41-53`). So `charActionsAsync['C']('C')` executes the lambda.
No upstream change required.

**`.FolderHandling.cs` — 18 uncovered**
- `73-77` (5) — `return await fp.InitAsync(...)` inside the `Task.Run` in `LoadFolderHandlerAsync`.
- `81-86` (6, plus branch `0/8` at `:81`) — the `logger.Debug` interpolation after a *successful* `InitAsync`.
- `95-98` (4) — the inner `catch (Exception e2) { logger.Error(...); throw; }` fallback. Reachable by making `_folderPredictorEmptyFactory` throw.
- `165-167` (3, plus branch at `:164`) — the `InvokeRequired` marshalling branch of `AssignFolderComboBox`.
- Partly-covered branches: `:36`, `:49`, `:125` are each `4/8` — the null-propagation operators in `_folderHandler?.Suggestions?.TopScore() ?? 0` inside the three `logger.Debug` interpolations. Reaching the remaining 12 conditions requires only a null `Suggestions` and a null `_folderHandler` case; that alone lifts branch coverage from 63.33% to ~83%.
- `:170` is `5/6` — one missing null case in `if (_folderHandler?.FolderArray?.Length > 0)`.

**`.Navigation.cs` — 11 uncovered**
- `141-142` (2) — the `default:` case of `ToggleConversationCheckbox(ToggleState)`.
- `197`, `202` (2) — dispatcher lambdas inside exempt `ToggleExpansionAsync(ToggleState)`; covered automatically once site 16 is de-exempted and tested.
- `212-214` (3, branch at `:211`) — the `_emailIsReadTimer.Dispose()` path of `ToggleExpansionOff`.
- `222-225` (4, branch `2/4` at `:221`) — the timer-creation path of `ToggleExpansionOn`.

**`.Conversation.cs` — 12 uncovered**
- `130-139` (9) — the entire `PopulateConversationAsync(ConversationResolver, CancellationToken, bool)` overload. Never called by any test.
- `212-214` (3, branch `1/2` at `:211`) — the `count == 0` red-background path of `RenderConversationCountAsync`.

**`.Initialization.cs` — 11 uncovered**
- `253`, `264`, `267-272`, `297` (9) — closure lines inside exempt async init methods (§1.1).
- `390`, `396` (2) — the bodies of the `_flagTasksFactory` and `_folderPredictorFactory` default
  lambdas in `SaveParameters`. Reachable by constructing the controller through a ctor that leaves
  those seams null and then invoking the resulting delegate.

**`QfcItemController.cs` — 0 uncovered lines**; the only branch gap is `:254`
(`TopFolderScore => _folderHandler?.Suggestions?.TopScore() ?? 0`) at `1/4`. Three more conditions
are reachable with a non-null `_folderHandler` whose `Suggestions` is non-null and null respectively.
`PropertiesTests.cs:36` covers only the all-null case.

---

## 3. Existing test inventory — 17 files, 166 test methods

All 17 are explicitly listed in `QuickFiler.Test/QuickFiler.Test.csproj:90,132-147` (the project uses
explicit `<Compile Include>`, no globbing — **any new test file F10 adds must be registered there**).
`QuickFiler/Properties/AssemblyInfo.cs:5` grants `InternalsVisibleTo("QuickFiler.Test")`, so
`internal partial class QfcItemController` and all its internal members are directly reachable.

| File | Lines | >500? | Test class(es) | `[TestMethod]` | Production partial targeted |
|---|---|---|---|---|---|
| `QfcItemController.TestSupport.cs` | 365 | No | `HarnessController` (fixture), `QfcItemControllerTestSupport` (static helpers), `QfcItemController_TestSupportSmokeTests` | 1 | shared fixture (all) |
| `QfcItemControllerTests.cs` | 377 | No | `QfcItemControllerTests`, `QfcItemController_KeyboardRegistrationTests` | 8 | `.Conversation.cs`, `.EventWiring.cs`, `.FolderHandling.cs` |
| `QfcItemController.ConversationTests.cs` | 352 | No | `QfcItemController_ConversationTests` | 12 | `.Conversation.cs` |
| `QfcItemController.EventHandlersTests.cs` | 438 | No | `QfcItemController_EventHandlersTests` | 16 | `.EventHandlers.cs` |
| `QfcItemController.EventWiringTests.cs` | 374 | No | `QfcItemController_EventWiringTests` | 10 | `.EventWiring.cs` (+ headless `ItemViewer`) |
| `QfcItemController.FocusAndThemeTests.cs` | 497 | No (3 to spare) | `QfcItemController_FocusAndThemeTests` | 17 | `.FocusAndTheme.cs` |
| `QfcItemController.FolderHandlingTests.cs` | 498 | No (2 to spare) | `QfcItemController_FolderHandlingTests` | 17 | `.FolderHandling.cs` |
| `QfcItemController.FolderSuggestionsTests.cs` | 191 | No | `QfcItemController_FolderSuggestionsTests` | 4 | `.FolderHandling.cs` (#325 row model) |
| `QfcItemController.InitializationTests.cs` | 193 | No | `QfcItemController_InitializationTests` | 4 | `.Initialization.cs` (ctors + `SaveParameters`) |
| `QfcItemController.MailActionsTests.cs` | 184 | No | `QfcItemController_MailActionsTests` | 7 | `.MailActions.cs` |
| `QfcItemController.NavigationTests.cs` | 391 | No | `QfcItemController_NavigationTests` | 13 | `.Navigation.cs` |
| `QfcItemController.PropertiesTests.cs` | 168 | No | `QfcItemController_PropertiesTests` | 9 | `QfcItemController.cs` |
| `QfcItemController.SeamCoreTests.cs` | 226 | No | `QfcItemController_SeamCoreTests` | 12 | `.Navigation.cs`, `.MailActions.cs`, `.EventHandlers.cs` cores, `.EventWiring.cs` |
| `QfcItemController.SeamDispatcherTests.cs` | 352 | No | `QfcItemController_SeamDispatcherTests` | 14 | `.Conversation.cs`, `.Navigation.cs`, `.FocusAndTheme.cs` |
| `QfcItemController.SeamFactoryTests.cs` | 284 | No | `QfcItemController_SeamFactoryTests` | 7 | `.Conversation.cs`, `.MailActions.cs`, `.EventWiring.cs` |
| `QfcItemController.ViewerSetupTests.cs` | 407 | No | `QfcItemController_ViewerSetupTests` | 9 | `.ViewerSetup.cs` |
| `QfcItemControllerBreadcrumbDropDownTests.cs` | 385 | No | `QfcItemControllerBreadcrumbDropDownTests` | 6 | `.ViewerSetup.cs` (breadcrumb, #400) |
| **Total** | **5,682** | — | 18 classes | **166** | — |

**No file exceeds 500 lines.** The two closest are `FolderHandlingTests.cs` (498) and
`FocusAndThemeTests.cs` (497) — both have almost no headroom, so any new test targeting
`.FolderHandling.cs` or `.FocusAndTheme.cs` must go into a **new** file (and a new
`<Compile Include>` entry). Note the precedent that a sibling has already had to do this:
open issue #450 is `Refactor: quickfiler-formcontroller-tests-file-size-split`.

### 3.1 `QfcItemController.TestSupport.cs` — the shared harness F10 must reuse

Reuse this file; do not re-create its helpers. Precise inventory:

- `HarnessController : QfcItemController` (`:25-29`) — exposes the `protected` parameterless
  constructor (`QfcItemController.Initialization.cs:27`) so a controller can be built with **no**
  collaborators at all.
- `SetField(controller, name, value)` (`:37-47`) and `GetField(controller, name)` (`:49-59`) —
  reflection over `BindingFlags.NonPublic | Instance` on `typeof(QfcItemController)`. This is the
  primary injection mechanism; every private field listed at `QfcItemController.cs:36-89` is
  reachable (`_globals`, `_itemViewer`, `_parent`, `_homeController`, `_kbdHandler`, `_themes`,
  `_activeTheme`, `_expanded`, `_folderHandler`, `_tlpStates`, `_uiDispatcher`, `_webViewInitializer`,
  `_mailActions`, `_isWebViewerInitialized`, the six factory delegates, `_predeterminedFolder`,
  `_emailIsReadTimer`, `_tableLayoutPanels`, `_itemPositionTips`, `_listTipsDetails`,
  `_listTipsExpanded`).
- `InvokeNonPublic(controller, name, args)` (`:66-80`) — calls private/internal instance methods by
  name; used for the private WinForms handlers (`Button_MouseEnter`, `MenuItem_MouseLeave`,
  `TopicThread_ItemSelectionChanged`, `CbxEmailCopy_CheckedChanged`,
  `CboFolders_SelectedIndexChanged`, `CbxAttachments_CheckedChanged`). Documents that all targets
  have unique names — F10 must preserve that when adding targets.
- `EnsureSynchronizationContext()` (`:87-93`) — installs a plain `SynchronizationContext` when the
  ambient one is null, so the `if (SynchronizationContext.Current is null)` guards behave as
  deterministic no-ops. **This is the helper that keeps `.EventHandlers.cs:30-32,52-54,75-77`
  uncovered.** Covering those lines requires the opposite fixture (a scope that *nulls* the context
  and restores it), which `ViewerScope` demonstrates.
- `BuildSyncDispatcher()` (`:102-137`) — `Mock<IUiDispatcher>` whose `Invoke(Action)`,
  `InvokeAsync(Action)`, `InvokeAsync(Action, DispatcherPriority, CancellationToken)` and
  `BeginInvoke(Action)` all execute the delegate synchronously. Note the documented limitation:
  the generic `InvokeAsync<TResult>` must be set up per-test (Moq cannot stub an open generic).
- `InjectThemes(controller, themes, activeTheme)` (`:143-151`) — sets `_themes` and `_activeTheme`.
- `BuildColorTheme(mouseOver, clicked, back)` (`:166-178`) — handle-less `Theme` carrying the three
  button colours, with a non-executing `Mock<IUiDispatcher>` reflected into `Theme._uiDispatcher`.
- `BuildThemeDictionary(activeTheme, theme)` (`:184-192`).
- `BuildDispatchableTheme(dispatcher)` (`:201-211`) — handle-less `Theme` with an injected
  dispatcher plus a handle-less `Label` in `Theme._lblSender` so `Theme.SetMailRead(bool)`'s null
  guard passes.
- `EnsureUiThreadDispatcher()` (`:238-249`) — seeds the **static** `UiThread._dispatcher` (only when
  unset) with a parked, never-pumped background STA dispatcher from `GetDedicatedDispatcher()`
  (`:257-285`).
- `StartRunningDispatcher()` (`:297-317`) / `ShutdownDispatcher(dispatcher)` (`:323-326`) — a real
  `Dispatcher.Run()` on a dedicated background STA thread, for members that route through the sealed
  `IItemViewer.UiDispatcher` (`QuickFiler/Viewers/IItemViewer.cs:36`, a concrete
  `System.Windows.Threading.Dispatcher` that cannot be mocked). Every call site pairs them in a
  `finally` (`ViewerSetupTests.cs:199/224`, `:307/333`; `FolderHandlingTests.cs:385/411`).

A second, **file-private** fixture exists that F10 will need and cannot currently reach:
`ViewerScope` (`QfcItemControllerBreadcrumbDropDownTests.cs:365-383`) — saves the ambient
`SynchronizationContext`, installs a plain one, constructs a real headless `new QuickFiler.ItemViewer()`,
and restores/disposes on `Dispose()`. It is `private sealed`, so `EventWiringTests` and
`ViewerSetupTests` each re-implement the same pattern inline. **Recommendation: promote a
`HeadlessViewerScope` (and a matching `NullSynchronizationContextScope`) into
`QfcItemController.TestSupport.cs`** rather than adding a fourth copy. This is an F10-owned test file,
so no sibling coordination is needed. The safety of headless `new ItemViewer()` construction is
established precedent in this repository (`UtilitiesCS/Threading/ProgressPane.cs` and
`UtilitiesCS.Test/Threading/ProgressPane_Tests.cs`), and it is already exercised by six passing
tests here.

### 3.2 What is already covered — do not re-test

Cross-referencing the 166 test names against the §2.3 uncovered map, these production areas are
already covered and should receive **no new tests**:

- All scalar properties, `ItemNumber`/`ItemNumberDigits` formatting, `ItemIndex`, `Height`,
  `NotifyPropertyChanged` (`PropertiesTests.cs`, 9 methods) — `QfcItemController.cs` is at 100% line.
- All four constructors and `SaveParameters` field assignment (`InitializationTests.cs`, 4 methods).
- `PopulateControls` (both overloads), `PopulateControlsAsync`, `AssignControls` (both
  `InvokeRequired` branches), `AssignControlsAsync`, `Cleanup`, `ResolveControlGroups`
  (`ViewerSetupTests.cs`, 9 methods).
- `PopulateConversation` (all overloads), `RenderConversationCount` (both overloads and the
  `InvokeRequired` branch), `RenderConversationCountAsync` non-zero path, `SetTopicThread` (both
  branches), and the three `LoadConversationResolverAsync` failure modes
  (`ConversationTests.cs` + `QfcItemControllerTests.cs`, 20 methods).
- `LoadFolderHandler` / `LoadFolderHandlerAsync` factory routing and the `ArgumentNullException`
  fallback, `PopulateFolderComboBox[Async]`, `AssignFolderComboBox` (predetermined / index-1 /
  single-item / null-handler), and the static `PopulateAndSelectFolder`
  (`FolderHandlingTests.cs` + `FolderSuggestionsTests.cs` + `QfcItemControllerTests.cs`, 25 methods).
- `Reply`, `ReplyAll`, `Forward`, all five `*Core()` handlers, `HandleWebViewInitializedAsync`
  success/`InvokeRequired`/failure (`SeamCoreTests.cs`, 12 methods).
- `KbdExecuteAsync` (both overloads, both flags), `JumpToFolderDropDown[Async]`,
  `JumpToSearchTextbox`, `JumpToAsync`, `MenuDropDown`, `ToggleConversationCheckbox` (all three
  states), `ToggleExpansion[Async]` parameterless routing, `ToggleExpansionOn`/`ToggleExpansionOff`
  happy paths (`NavigationTests.cs` + `SeamDispatcherTests.cs`, 27 methods).
- `ToggleFocus` (both overloads), `ToggleFocusOnAsync`/`ToggleFocusOffAsync`, `ToggleNavigation`
  sync paths, `ToggleNavigationAsync`, `ToggleTips` sync, `InvokeBeginInvoke`, `SetThemeDark`/
  `SetThemeLight` from-Normal, `ApplyReadEmailFormat` (`FocusAndThemeTests.cs` +
  `SeamDispatcherTests.cs`, 19 methods).
- All six mouse/menu colour handlers, `CbxEmailCopy`/`CbxAttachments`/`CboFolders` handlers,
  `TextBoxSearch_TextChanged`, `TextBoxSearch_KeyDown` (both branches),
  `TopicThread_ItemSelectionChanged` (both branches), `BtnDelItem_Click`, `BtnFlagTask_Click`
  (`EventHandlersTests.cs`, 16 methods).
- Register/Unregister focus and expanded actions, sync and async, and `WireIntentEvents`,
  `WireControlTreeEvents`, `WireEvents` (`EventWiringTests.cs` + `SeamFactoryTests.cs`, 13 methods).
- `MoveMailAsync` null-helper / missing-OneDrive / happy paths; `FlagAsTask[Async]` factory-argument
  assertions (`SeamFactoryTests.cs`, 6 methods).
- Breadcrumb drop-down configuration, theme caching, pooled-viewer reset, arrow fall-through
  (`QfcItemControllerBreadcrumbDropDownTests.cs`, 6 methods).

**The genuine gaps are exactly the §2.3 list and nothing else.** A plan that re-tests any of the
above is duplicating existing coverage.

---

## 4. Test-policy compliance audit of the existing 17 files

Scanned for: `DateTime.Now`, `DateTime.UtcNow`, `Random`, `Thread.Sleep`, `Task.Delay`, real
wall-clock waits, `Path.GetTempPath`, `GetTempFileName`, `File.WriteAllText`, `MessageBox`,
`ShowDialog`, live form construction, xUnit/NUnit references, MSTest `Assert.` in place of
FluentAssertions, and files over 500 lines.

**Result: zero hard violations.** Specifically:

- No `DateTime.Now` / `DateTime.UtcNow` anywhere in the 17 files. (Contrast with F4's
  `MailItemInfoTests.cs:25`, recorded in `epic.md` "Latent Defect Promotion".) The one `DateTime`
  literal is a fixed value, `new DateTime(2026, 1, 1)` at `ViewerSetupTests.cs:128` — deterministic.
- No `Thread.Sleep`, no `Task.Delay`, no polling, no wall-clock waits. Thread synchronisation uses
  `ManualResetEventSlim` around deterministic signals only (`TestSupport.cs:263-279`, `:299-314`).
- No `Random` / `Random.Shared`.
- No temporary-file creation; no filesystem writes.
- No `MessageBox` and no `ShowDialog`.
- No live *form* construction. Real `UserControl` construction (`new QuickFiler.ItemViewer()`) occurs
  in `ViewerScope` and in the `EventWiringTests`/`ViewerSetupTests` headless tests; this is permitted
  under `epic.md` "Shared Design § 3" (STA last-resort clause) — the control is never shown, is
  disposed, and the ambient `SynchronizationContext` is saved and restored.
- MSTest only; Moq only; FluentAssertions only. No `Assert.` call sites, no xUnit, no NUnit.
- **No file exceeds 500 lines** (max 498).

Three items that are *not* violations but are policy risks F10 should tidy while it is in these files:

1. **Static mutable global state that is never restored.**
   `QfcItemControllerTestSupport.EnsureUiThreadDispatcher()` (`TestSupport.cs:238-249`) writes the
   static `UtilitiesCS` `UiThread._dispatcher` field and never restores it, and
   `GetDedicatedDispatcher()` (`:257-285`) parks a process-lifetime background STA thread in a static
   field. It is guarded (`only when unset`), so it is idempotent, but it makes assembly-wide test
   state order-dependent, which sits against `.claude/rules/general-unit-test.md` ("Tests must not
   rely on mutable global state"). Recommendation: wrap in a save/restore scope, or document the
   deviation explicitly in F10's policy audit.
2. **`EnsureSynchronizationContext()` (`:87-93`) mutates ambient thread state without restoring it.**
   `ViewerScope` (`QfcItemControllerBreadcrumbDropDownTests.cs:365-383`) shows the correct
   save/restore shape. Because MSTest reuses threads, a test that runs after
   `EnsureSynchronizationContext` sees an installed context it did not ask for. This is also the
   direct blocker on covering `.EventHandlers.cs:30-32,52-54,75-77`. Recommendation: convert to a
   disposable scope and add a complementary null-context scope.
3. **Fixture duplication.** The headless-`ItemViewer` construction pattern is implemented three
   times (`QfcItemControllerBreadcrumbDropDownTests.cs:365`, and inline in `EventWiringTests` and
   `ViewerSetupTests`). Consolidate into `TestSupport.cs` before adding a fourth.

Per `epic.md` "Latent Defect Promotion", test-policy items in existing tests are **in scope for this
child's own execution**, not deferred. Items 1-3 above are the complete list for F10.

---

## 5. Latent defects for promotion

These are production defects found while reading the F10 file set. **None was fixed.** They are out
of scope under the epic's no-behaviour-change NFR and should be promoted to GitHub issues via the
MCP promotion lifecycle before F10 completes. Severity is this researcher's judgement.

| # | Location | Defect | Severity |
|---|---|---|---|
| L1 | `QuickFiler/Controllers/QfcItemController.FocusAndTheme.cs:170-178` | `ToggleNavigation(bool async)` calls `_itemPositionTips.Toggle(false)` **unconditionally at `:170`** and then calls it **again** in both the `if (async)` and `else` branches (`:173`, `:177`). Every invocation therefore toggles the navigation tips twice, returning them to their original state. The existing test `ToggleNavigation_Synchronous_TogglesPositionTips` (`FocusAndThemeTests.cs:310`) pins the current behaviour, so a fix must update that test. | **High** — user-visible feature is inert |
| L2 | `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs:52` | `CoreWebView2EnvironmentOptions options = new("–incognito ");` uses an **en dash (U+2013)** instead of the `--` double hyphen required for a Chromium command-line switch, so the incognito flag is silently ignored. The commented-out line directly above (`:51`) uses the correct `--disk-cache-size=1` form, which makes this a transcription error rather than an intentional choice. | **Medium** — privacy/caching intent not applied |
| L3 | `QuickFiler/Controllers/QfcItemController.Navigation.cs:222-225` and `QfcItemController.ViewerSetup.cs:420` | `ToggleExpansionOn` assigns a new `System.Threading.Timer` to `_emailIsReadTimer` without disposing any previous instance. Two consecutive expands (with no intervening collapse) leak a timer. `Cleanup()` sets `_emailIsReadTimer = null` (`ViewerSetup.cs:420`) without disposing it, leaking one per pooled-viewer release. | **Medium** — resource leak proportional to session length |
| L4 | `QuickFiler/Controllers/QfcItemController.MailActions.cs:119-121` | `MessageBox.Show(...)` is raised from inside a `catch` in `MoveMailAsync`, a method reached from the filing/queue path. Besides being a modal interruption on a non-interactive path, it makes the catch block unreachable in a unit test (unit-test policy prohibits popups), which is why `.MailActions.cs:115-122` is permanently uncovered. Fix direction: route through an injectable prompt seam local to QuickFiler. | **Medium** — UX + permanently untestable branch |
| L5 | `QuickFiler/Controllers/QfcItemController.EventWiring.cs:117` | `throw (initException);` rethrows the caught exception, resetting its stack trace; and when `isSuccess == false` with a null `initException`, `throw null` raises a `NullReferenceException` that the enclosing `catch` (`:148`) then logs under the misleading message "Error in WebView2Control Initialization Completed Event". | **Medium** — diagnostics loss |
| L6 | `QuickFiler/Controllers/QfcItemController.EventWiring.cs:121-137` | `HandleWebViewInitializedAsync` polls with `await Task.Delay(newDelay)` where `newDelay = 100 * ++delayCount`, up to a 10 s budget, waiting for `ItemHelper` to become non-null. The growing interval means the final wait alone is ~1.4 s. There is no injectable delay/`TimeProvider` seam, so lines `125-137` cannot be covered deterministically. | **Medium** — latency + permanently untestable loop |
| L7 | `QuickFiler/Controllers/QfcItemController.FocusAndTheme.cs:260-266` | `ToggleSaveAttachments()` has an entirely commented-out body and is a no-op, yet it is registered as the `'A'` keyboard action at `QfcItemController.EventWiring.cs:179-183`. Pressing `A` silently does nothing. The existing test `ToggleSaveAttachments_DoesNotThrow` (`FocusAndThemeTests.cs:433`) pins the no-op. | **Medium** — advertised keyboard action is dead |
| L8 | `QuickFiler/Controllers/QfcItemController.Initialization.cs:111-133` | The `QfcItemController(..., bool async)` constructor accepts an `async` parameter and never reads it; the body is identical to the other overloads. Callers cannot tell that requesting the async form has no effect. | **Low** — misleading public API |
| L9 | `QuickFiler/Controllers/QfcItemController.EventHandlers.cs:166-171` | `TextBoxSearch_TextChanged` dereferences `_folderHandler.FindFolder(...)` with no null guard. `_folderHandler` is assigned only by `LoadFolderHandler[Async]`, so typing in the search box before folder suggestions have loaded throws a `NullReferenceException`. Related to open issue #438. | **Low-Medium** |
| L10 | `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs:402-420` | `Cleanup()` assigns `_itemViewer = null` twice (`:403` and `:419`) and `_folderHandler = null` twice (`:408` and `:411`). Harmless, but it is dead code inside a method whose correctness is asserted line-by-line by `Cleanup_NullsTrackedPrivateFields` (`ViewerSetupTests.cs:338`). | **Low** — dead code |
| L11 | `QuickFiler/Controllers/QfcItemController.Initialization.cs:392-394` | `_mailActions ??= mailItem is null ? null : new MailItemActionsAdapter(mailItem);` deliberately leaves the seam null when `mailItem` is null, so later `_mailActions.EntryID` (`.MailActions.cs:32,42`) and `_mailActions.Display()` (`.EventHandlers.cs:135`) throw a `NullReferenceException` rather than a diagnosable error. | **Low** |
| L12 | `QuickFiler/Controllers/QfcItemController.Conversation.cs:121` | `SetTopicThread(ConversationResolver.ConversationInfo.Expanded)` in the deferred (`loadAll == false`) path has no null guard on `ConversationInfo`, although the immediately preceding code (`:102-103`) does guard `ConversationResolver` itself. | **Low** |

L4 and L6 are distinctive: they are the only two defects that are *also* the reason a specific set
of lines can never reach the coverage gate. They should be promoted with that framing so whoever
schedules them understands the coverage dependency.

---

## 6. Summary of corrections to `epic.md` and to the delegation brief

| Claim | Source | Verdict | Evidence |
|---|---|---|---|
| "six of ten partials are currently exempted"; `[X]` on six F10 files | `epic.md:395-399` | **Wrong at file level.** All 19 attributes are member-level; no partial declaration carries one; all ten partials are instrumented and all appear in the Cobertura report. | `QfcItemController*.cs` 19 grep hits, all at 8-space indent before a method; `coverage-final.cobertura.xml:22740,23126,23519,24004,24222,24601,25411,25754,26058,26662` |
| F10 file/line inventory (`~3,180 lines / 11 files`, per-file line counts) | `epic.md:395-399` | **Correct.** 3,073 production lines across 10 partials + 107 for `IQfcItemController.cs`. Every individual line count matches. | line counts per file, §1.4 |
| F10 measured baseline: ViewerSetup 74.4%, FocusAndTheme 75.6%, MailActions 77.8%, EventHandlers 79.7% | `epic.md:174-178` | **Right set of files, wrong numbers.** Those are the `<class line-rate>` attribute values, which double-count. True per-file union figures are 72.50%, 74.26%, 76.80%, 79.57%. | §2.1, §2.2 |
| Branch coverage is "a separate, unmet gate" and children must report both | `epic.md:189-192` | **Correct and materially understated for F10.** Six of ten files fail the branch gate, and two of them (`FolderHandling`, `EventWiring`) pass the line gate while failing it. | §2.2 |
| Known conflict risks are #400 and #424, both "active on `main` concurrently" | `epic.md:636-641` | **Stale.** #400 and #424 are both **Closed**. The live risks for F10 are #230, #427, #438, #440, #441 — none of which the epic names. | companion artifact §5 |
| "the epic marked six QfcItemController partials `[X]` yet the Cobertura report shows real coverage percentages" | delegation brief | **Correct observation; resolved.** Cause is member-level attributes, as the brief anticipated. | §1.0 |
| "all 19 attributes are METHOD-level (or nested-type level)" | delegation brief | **Confirmed** — all 19 are method-level; none is on a nested type. | §1.2 |
| The 19 site line numbers listed in the brief | delegation brief | **All 19 verified exact.** | §1.2 |
| "Removing an attribute will likely LOWER the reported percentage before new tests raise it" | delegation brief | **Confirmed and quantified**: family 82.51% -> 69.82%; four currently-passing files fall below 80%. | §1.4 |
| "17 existing test files ... flag any over 500 lines" | delegation brief | **17 confirmed; none over 500** (max 498). All 17 registered in `QuickFiler.Test.csproj`. | §3 |
| "`QfcItemController.TestSupport.cs` appears to be a shared support file" | delegation brief | **Confirmed**, and it is the only shared one; `ViewerScope` is file-private and duplicated. | §3.1 |
| Implicit assumption that every exempted member is live production code | delegation brief and `epic.md` | **Three of the 19 sites are on dead members** with zero call sites solution-wide: `Initialize(9 params)`, `CreateAsync`, `CreateSequentialAsync`. Correct disposition is deletion, not testing. Independently corroborated by a sibling researcher's finding on the same branch. | §1.2 call-site table |
| `IQfcDatamodel` named as an F10 sibling dependency | delegation brief | **False positive.** No occurrence of `IQfcDatamodel`, `QfcDatamodel` or `EfcDataModel` anywhere in the F10 file set. | companion artifact §4.3 |
