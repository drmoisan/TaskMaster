# Per-file research — `QuickFiler/Controllers/QfcItemController.Navigation.cs`

- Epic: #136 QuickFiler Per-File 80% Coverage — child F10 (`quickfiler-item-controller-coverage`, issue #453)
- Branch: `feature/quickfiler-item-controller-coverage`
- Worktree: `C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-a359b62de7a79b16e`
- File length verified on this branch: **228 lines** (matches the brief).

---

## 0. Measured baseline (indicative) and reconciliation

From `docs/features/active/2026-08-06-quickfiler-high-confidence-queue-init-stall-424/evidence/qa-gates/coverage-final.cobertura.xml`,
line 25754:

```
line-rate="0.89071" branch-rate="0.766667" complexity="29"
name="QuickFiler.Controllers.QfcItemController"
filename="QuickFiler\Controllers\QfcItemController.Navigation.cs"
```

**Staleness check passed.** Method spans align exactly with the current file:
`JumpToFolderDropDown` = 28–38, `ToggleConversationCheckbox()` = 115–119,
`ToggleConversationCheckbox(ToggleState)` = 127–145, `ToggleExpansionOff` = 208–215,
`ToggleExpansionOn` = 218–226, and the two closures `<ToggleExpansionAsync>b__205_0/1` at 197 / 202.

| Gate | Target | Measured | Verdict |
| --- | --- | --- | --- |
| Line | >= 80% | **89.07 %** | PASS |
| Branch | >= 75% | **76.67 %** (23 of 30 conditions) | PASS — **by 1.67 points / 1 condition** |

Branch arithmetic reconciles exactly: per-method blocks contribute 8 conditions (5 covered) —
`ToggleExpansion()` 149 (2/2), `ToggleExpansionOff` 211 (1/2), `ToggleExpansionOn` 221 (2/4); the
class-level block contributes 22 (18 covered). 23/30 = 0.766667.

**Correction to the brief.** The brief presents this file as comfortably compliant at 89.1 %. On
line coverage it is. On branch coverage the margin is **one condition** — losing a single branch
direction (for example if a refactor introduces a guard) drops it under 75 %. Treat this file as
*at risk*, not as *safe*.

**The more significant finding is that this file's two `[ExcludeFromCodeCoverage]` attributes are
stale.** See §1 and §5.2 — the barrier each one cites was defeated by a later cycle in this same
file, and removing them is the correct action under the #227 maintainer precedent.

F1's harness on this child's branch is the acceptance authority.

---

## 1. Member inventory

No fields, properties, constructors, events, or nested types. Eighteen methods.

| # | Member | Lines | Accessibility | `[ExcludeFromCodeCoverage]` |
| --- | --- | --- | --- | --- |
| 1 | `JumpToFolderDropDown()` | 27–38 | `public void` | No |
| 2 | `JumpToFolderDropDownAsync()` | 40–49 | `public async Task` | No |
| 3 | `JumpToSearchTextbox()` | 51–55 | `public void` | No |
| 4 | `JumpToAsync(Control)` | 57–61 | `internal async Task` | No |
| 5 | `KbdExecuteAsync(Func<Task>, bool)` | 63–70 | `public async Task` | No |
| 6 | `KbdExecuteAsync<T>(Func<T,Task>, T, bool)` | 72–79 | `public async Task` | No |
| 7 | `MenuDropDown()` | 81–84 | `public async Task` | No |
| 8 | `Reply()` | 86–92 | `public async Task` | No |
| 9 | `ReplyAll()` | 94–98 | `public async Task` | No |
| 10 | `Forward()` | 100–104 | `public async Task` | No |
| 11 | `ToggleConversationCheckbox()` | 111–119 | `public void` | No |
| 12 | `ToggleConversationCheckbox(Enums.ToggleState)` | 121–145 | `public void` | No |
| 13 | `ToggleExpansion()` | 147–157 | `public void` | No |
| 14 | `ToggleExpansionAsync()` | 159–169 | `public async Task` | No |
| 15 | `ToggleExpansion(Enums.ToggleState)` | 174–187 | `public virtual void` | **Yes — line 173** |
| 16 | `ToggleExpansionAsync(Enums.ToggleState)` | 192–205 | `public virtual async Task` | **Yes — line 191** |
| 17 | `ToggleExpansionOff()` | 207–215 | `private void` | No |
| 18 | `ToggleExpansionOn()` | 217–226 | `private void` | No |

Compiler-generated closures: `<JumpToFolderDropDown>b__190_0` (32–36),
`<JumpToFolderDropDownAsync>b__191_0` (44–48), `<MenuDropDown>b__196_0` (83), `<Reply>b__197_0` (90),
`<ReplyAll>b__198_0` (96), `<Forward>b__199_0` (102), `<ToggleConversationCheckbox>b__200_0` (117),
`<ToggleExpansionAsync>b__205_0` (197), `<ToggleExpansionAsync>b__205_1` (202).

### 1.1 Toolchain finding — method-level exemption does NOT cover its lambdas

`<ToggleExpansionAsync>b__205_0` (line 197) and `b__205_1` (line 202) are the two lambda bodies
*inside* `ToggleExpansionAsync(Enums.ToggleState)`, which carries `[ExcludeFromCodeCoverage]` at
line 191. Both appear in the Cobertura report as their own `<method>` entries at
`line-rate="0"` (artifact lines 25888–25897), and both appear in the class-level `<lines>` block
with `hits="0"` (artifact lines 26028–26029). **The attribute did not propagate to the
compiler-generated closure class.**

Consequence, and it generalises across the whole epic: **every exempt method that contains a lambda
silently contributes uncovered lines to its file's denominator.** `ToggleExpansion(Enums.ToggleState)`
(line 174) has no lambda and so contributes nothing; `ToggleExpansionAsync(Enums.ToggleState)` has
two and contributes two permanently-uncovered lines that no test can reach while the method is
exempt-by-convention (nothing stops a test calling it — see §5.2 — but the intent of the attribute
is that none does).

**Raise this with F1** as a ledger/harness note: a `ratified-exempt` classification at method level
does not fully remove the member from measurement when lambdas are present.

### 1.2 Exemption assessment (per the #227 maintainer precedent)

Both attributes carry the same justification comment (lines 171–172 and 189–190):

> Made virtual so tests can override the (TlpCellSnapShot-bound, out-of-scope) state-taking body and
> verify the parameterless-overload routing without the control-tree collaborator.

**That barrier no longer exists.** The comment's premise is that `ToggleExpansionOff()` /
`ToggleExpansionOn()` are unreachable because `_tlpStates[...].ApplyState(...)` needs a real control
tree. A later cycle (labelled "Cycle-5 (R2)" in the current source and tests) retyped `ApplyState`
to accept the narrowed `IItemViewer` — `ToggleExpansionOff` line 209 and `ToggleExpansionOn`
line 219 now read `_tlpStates["…"].ApplyState(_itemViewer)` with **no `(ItemViewer)` cast** — and
`QfcItemController.NavigationTests.cs:291` and `:344` prove both private methods run against a
`Mock<IItemViewer>` whose `Controls` returns a bare `new Control()` host, with a real
`TlpCellSnapShot`. Both tests pass today.

Everything else the two exempt methods touch is already seamed: `_parent.ToggleExpansionStyle` /
`ToggleExpansionStyleAsync` (`IQfcCollectionController`), `_uiDispatcher.InvokeAsync`
(`IUiDispatcher`, with `QfcItemControllerTestSupport.BuildSyncDispatcher()` at TestSupport.cs:102),
and `Register/UnregisterExpanded[Async]Actions` (real `KbdActions<>`, proven at
`EventWiringTests.cs:185`, `:200`).

**Recommendation: remove both attributes** (lines 173 and 191) **in the same change that adds the
four covering tests N-1…N-4 in §8.** This is the third consecutive cycle in which a residual
`QfcItemController` exemption turned out to be reducible by cross-checking its stated barrier
against a technique already proven elsewhere in the same repository — precisely the audit method the
maintainer's 2026-07-01 ratification denial established.

**Measurement impact — must be planned for.** These are *method-level* attributes, so removing them
adds their bodies to the denominator: approximately **18–20 new coverable line entries** and **2 new
branch points (4 conditions after per-method/class double counting)**. If the attributes are removed
without the covering tests, measured line coverage falls from 89.07 % to roughly **77 %** — below the
80 % gate. Removal and coverage must land as one atomic change, and the plan must measure
immediately after. With the four tests, projected end state is **line ≈ 99 %, branch = 100 %**.

Keep both methods `virtual`: `ExpansionSpyController` (`NavigationTests.cs:139–157`) still overrides
them to test the parameterless routing, and those tests must not be disturbed. New direct tests must
target `HarnessController`, not the spy.

---

## 2. What is already covered

Existing test file: `QuickFiler.Test/Controllers/QfcItemController.NavigationTests.cs`
(392 lines, class `QfcItemController_NavigationTests`), plus the shared
`QfcItemController.TestSupport.cs` harness.

| Member | Status | Covering test(s) |
| --- | --- | --- |
| `JumpToFolderDropDown()` | **COVERED** 100 % | `JumpToFolderDropDown_TogglesKeyboardAndFocusesFolderDropDown` (NavigationTests.cs:160) |
| `JumpToFolderDropDownAsync()` | **COVERED** 100 % (class-level lines 41–49 all hit) | covered transitively; no dedicated test — the closure `b__191_0` (44–48) is hit |
| `JumpToSearchTextbox()` | **COVERED** 100 % | `JumpToSearchTextbox_TogglesKeyboardAndFocusesSearch` (:184) |
| `JumpToAsync(Control)` | **COVERED** 100 % | `JumpToAsync_FocusesHandlelessControlAndTogglesKeyboardDialog` (:208) |
| `KbdExecuteAsync(Func<Task>, bool)` | **COVERED** 100 % / 100 % | `…_WhenDeactivateKbdTrue_TogglesKeyboardAndRunsAction` (:44), `…_WhenDeactivateKbdFalse_RunsActionWithoutToggling` (:66) |
| `KbdExecuteAsync<T>` | **COVERED** 100 % / 100 % | `KbdExecuteAsyncGeneric_WhenDeactivateKbdTrue_TogglesAndPassesArgument` (:88), `…False_DoesNotToggle` (:111) |
| `MenuDropDown()` | **COVERED** 100 % | covered transitively (lines 82–84 hit) |
| `Reply()` | **COVERED** 100 % | covered transitively (87, 90–92 hit) |
| `ReplyAll()` | **COVERED** 100 % | covered transitively (95–98 hit) |
| `Forward()` | **COVERED** 100 % | covered transitively (101–104 hit) |
| `ToggleConversationCheckbox()` | **COVERED** 100 % | covered transitively (115–119 hit) |
| `ToggleConversationCheckbox(ToggleState)` | **PARTIALLY COVERED** — 100 % line at method level, but the class-level block shows **141, 142 uncovered** and branch 130 at 3/4 | `On` and `Off` cases covered; `default` case never taken |
| `ToggleExpansion()` | **COVERED** 100 % / 100 % | `ToggleExpansion_WhenCollapsed_RoutesToOnState` (:230), `…WhenExpanded_RoutesToOffState` (:244) |
| `ToggleExpansionAsync()` | **COVERED** 100 % / 100 % | `ToggleExpansionAsync_WhenCollapsed_RoutesToOnState` (:258), `…WhenExpanded_RoutesToOffState` (:272) |
| `ToggleExpansion(ToggleState)` | EXEMPT — not measured (no lambdas, contributes nothing) | body never executed by any test (the spy overrides it) |
| `ToggleExpansionAsync(ToggleState)` | EXEMPT — but its two lambdas (197, 202) **are** measured and are at **0 %** | body never executed |
| `ToggleExpansionOff()` | **PARTIALLY COVERED** — 62.5 % line, 50 % branch | `ToggleExpansionOff_AppliesCompressedSnapshotAndClearsExpandedFlag` (:292) — `_emailIsReadTimer == null` path only |
| `ToggleExpansionOn()` | **PARTIALLY COVERED** — 55.6 % line, 50 % branch | `ToggleExpansionOn_AppliesExpandedSnapshotAndSetsExpandedFlag` (:345) — `ItemHelper == null` path only, explicitly noted in the test's own doc comment |

**Do not re-author any of the above.** Twelve of the sixteen measured members are at 100 % line and
branch. The entire gap is four members.

---

## 3. The gap list

### 3.1 Uncovered lines — all 13

| Member | Uncovered lines | Count | What reaches them |
| --- | --- | --- | --- |
| `ToggleConversationCheckbox(ToggleState)` `default:` case | 141, 142 | 2 | any `Enums.ToggleState` value that is neither exactly `On` nor exactly `Off` |
| `ToggleExpansionAsync(ToggleState)` lambdas (inside the exempt method) | 197, 202 | 2 | calling the exempt method with a synchronous `IUiDispatcher` |
| `ToggleExpansionOff()` timer-dispose branch | 212, 213, 214 | 3 | `_emailIsReadTimer != null` on entry |
| `ToggleExpansionOn()` read-timer branch | 222, 223, 224, 225 | 4 | `ItemHelper != null` **and** `ItemHelper.UnRead == true` |
| **Total** | | **13** | |

### 3.2 Uncovered branches

| Line | Construct | Coverage | Missing direction | Weight (method + class) |
| --- | --- | --- | --- | --- |
| 130 | `switch (desiredState)` | 3/4 | the `default` arm | 1 (class only — the per-method block for this overload records no conditions) |
| 211 | `if (_emailIsReadTimer is not null)` | 1/2 | true | 2 |
| 221 | `if ((ItemHelper is not null) && ItemHelper.UnRead == true)` | 2/4 | both sub-conditions' true side | 4 |

Lines 65, 74, 133, 137, 149, 161 are already fully covered.

**Branch-heavy members:** `ToggleConversationCheckbox(Enums.ToggleState)` (a 3-arm switch, complexity
4 at class level) and `ToggleExpansionOn` (a short-circuit `&&`, the only 2-condition line in the
file).

**Path to a comfortable margin:**

| Step | Test | Line effect | Branch effect |
| --- | --- | --- | --- |
| 1 | `ToggleExpansionOn` with `ItemHelper.UnRead == true` | +4 | +4 → 27/30 = 90 % |
| 2 | `ToggleExpansionOff` with an armed timer | +3 | +2 → 29/30 = 96.7 % |
| 3 | `ToggleConversationCheckbox(default)` | +2 | +1 → **30/30 = 100 %** |
| 4 | de-exempt + cover both `Toggle*(ToggleState)` overloads | +~20 (all covered) | +4 (all covered) |

Steps 1–3 are three tests that take branch coverage from a one-condition margin to 100 % and line
coverage from 89.07 % to ~98 %. Step 4 is the exemption-removal work.

---

## 4. Event subscription lifecycle

This file makes **no `+=` or `-=` subscription**. Verified: grepping `-=` across the whole
`QfcItemController.*` family returns only the three `BreadcrumbUnhandledArrow` hits in
`QfcItemController.ViewerSetup.cs`. The complete subscribe/unsubscribe map is in
`file-QfcItemController.EventWiring.md` § 4 and is not duplicated here.

What this file *does* manage is the **keyboard-action registry** and a **`System.Threading.Timer`** —
two lifecycle resources with the same symmetry questions.

### 4.1 Keyboard-action registration pairing

| Transition | Registers | Unregisters | Symmetric? |
| --- | --- | --- | --- |
| `ToggleExpansion(On)` (line 180) | `RegisterExpandedActions()` — **sync** `CharActions['B','D']` | — | — |
| `ToggleExpansion(Off)` (line 185) | — | `UnregisterExpandedActions()` — **sync** | Yes, *within* the sync variant |
| `ToggleExpansionAsync(On)` (line 198) | `RegisterExpandedAsyncActions()` — **async** `CharActionsAsync['B','D']` | — | — |
| `ToggleExpansionAsync(Off)` (line 203) | — | `UnregisterExpandedAsyncActions()` — **async** | Yes, *within* the async variant |

**Finding V-1 (Medium) — the sync and async expansion variants maintain disjoint registries, and
production mixes them.** `_expanded` is a single shared flag (`QfcItemController.cs:146`) written by
both `ToggleExpansionOff` (line 210) and `ToggleExpansionOn` (line 220), but the registry each
variant touches is different. `KbdActions.Remove` returns `false` silently when nothing matches
(`QuickFiler/Controllers/KbdActions.cs:126–128`) and every call site discards the result, so a
cross-variant collapse removes nothing and reports nothing.

Both variants have live production call sites:

- **sync** `ToggleExpansion()` — `QfcCollectionController.cs:1140`, `:1414` (`ActivateBySelection`),
  **`:1439` (inside the *async* `ActivateBySelectionAsync`)**, `:1679`.
- **async** `ToggleExpansionAsync()` — `QfcCollectionController.cs:1212`, and the keyboard `'E'` /
  `Keys.Right` registrations at `QfcItemController.EventWiring.cs:224` and `:263`.

Line 1439 is decisive: `ActivateBySelectionAsync` (an `async Task` method) calls the **synchronous**
`itemController.ToggleExpansion()`. A single item can therefore be expanded by one variant and
collapsed by the other. Reachable failing sequence:

1. `ActivateBySelectionAsync(n, blExpanded: true)` → sync `ToggleExpansion()` → `ToggleExpansion(On)`
   → `_expanded = true`, **sync** `'B'`/`'D'` registered.
2. User presses `'E'` → async `ToggleExpansionAsync()` → `_expanded` true → `ToggleExpansionAsync(Off)`
   → `UnregisterExpandedAsyncActions()` removes **nothing** (the async entries were never added);
   the sync `'B'`/`'D'` remain. `_expanded = false`.
3. `ActivateBySelectionAsync(n, blExpanded: true)` again → sync `ToggleExpansion()` → `_expanded`
   false → `ToggleExpansion(On)` → `RegisterExpandedActions()` → `KbdActions.Add` finds the stale
   sync `'B'` and **throws `ArgumentException("Cannot add key because it already exists. Key B
   SourceId <entryId>")`** (`KbdActions.cs:94–97`).

This is the highest-severity finding in the file and is a genuine state-transition defect, not a
coverage artefact.

**Finding V-2 (Low) — expansion registration is not idempotent and the guard is one level up.**
`ToggleExpansion(Enums.ToggleState)` and its async twin call `Register*` unconditionally on the `On`
arm. The `_expanded` check lives only in the parameterless overloads (lines 149, 161). The
state-taking overloads are `public` on the class but **not** on `IQfcItemController`
(`QuickFiler/Interfaces/IQfcItemController.cs:43` declares only `void ToggleExpansion();` and `:93`
only `Task ToggleExpansionAsync();`), so today no external caller can bypass the guard — but nothing
enforces that, and the `public` accessibility invites it.

### 4.2 Timer lifecycle

`ToggleExpansionOn` arms a 4-second one-shot timer (223–224); `ToggleExpansionOff` disposes it
(211–214). Within this file the pairing is symmetric. It is **not** symmetric with `Cleanup()`:
`QfcItemController.ViewerSetup.cs:420` sets `_emailIsReadTimer = null` **without disposing**, so an
armed timer survives cleanup and fires `ApplyReadEmailFormat`
(`QfcItemController.FocusAndTheme.cs:318`) against nulled collaborators. See D-1 below.

`ToggleExpansionOn` also **overwrites** `_emailIsReadTimer` (line 223) without disposing a previous
instance. Reachable only by a second `ToggleExpansion(On)` without an intervening `Off`, which
V-2 shows is currently guarded — but see V-1 for how the guard is defeated.

---

## 5. Seam analysis

### 5.1 Members needing no new seam

Three of the four gap members are reachable **today** with the existing harness:

| Uncovered element | Barrier | What it needs |
| --- | --- | --- |
| 141, 142 (`default:` arm) | **none** | `controller.ToggleConversationCheckbox((Enums.ToggleState)0)` or `Off \| Force`, with `BuildSyncDispatcher()` and a `Mock<IItemViewer>` — exactly the fixture already used at `NavigationTests.cs:164–172` |
| 197, 202 (exempt-method lambdas) | **none** — the exemption is a convention, not a compile barrier | `Mock<IQfcCollectionController>` for `ToggleExpansionStyleAsync`, `BuildSyncDispatcher()`, real `TlpCellStates`/`TlpCellSnapShot` (fixture already built at `NavigationTests.cs:295–327`), `Mock<IQfcKeyboardHandler>` with real `KbdActions<>` |
| 212, 213, 214 (timer dispose) | **none** for the dispose itself | inject a non-null `IDisposable`/`Timer` into `_emailIsReadTimer` via `SetField` and assert it was disposed |
| 222, 223, 224, 225 (timer arm) | **determinism** — see §5.2 | `ItemHelper` non-null with `UnRead == true`; but the arming itself starts a real 4-second timer |

### 5.2 The one real seam gap — the 4-second `System.Threading.Timer`

```csharp
// QfcItemController.Navigation.cs:217–226
private void ToggleExpansionOn()
{
    _tlpStates["Expanded"].ApplyState(_itemViewer);
    _expanded = true;
    if ((ItemHelper is not null) && ItemHelper.UnRead == true)
    {
        _emailIsReadTimer = new System.Threading.Timer(ApplyReadEmailFormat);
        _emailIsReadTimer.Change(4000, System.Threading.Timeout.Infinite);
    }
}
```

Covering lines 222–225 as written arms a **live 4 000 ms wall-clock timer on the thread pool** that
outlives the test method. When it fires it calls `ApplyReadEmailFormat`
(`QfcItemController.FocusAndTheme.cs:318`) on a controller whose collaborators are Moq stubs — an
unobserved exception on a thread-pool thread, arriving during an unrelated later test.
`.claude/rules/general-unit-test.md` § "Core Principles" 1 (independence) and 4 (determinism) both
forbid this. Waiting for it is equally forbidden (banned wall-clock waits). net481 has no
`TimeProvider`/`FakeTimeProvider`.

**Minimum seam, per the hierarchy (interface > injectable delegate > adapter):** no interface is
warranted for "arm a one-shot timer"; an adapter type would be heavier than the two call sites. Use
an **injectable delegate**, matching the six factory-delegate seams already on this class
(`QfcItemController.cs:66–89`):

```csharp
// QfcItemController.cs — retype the existing field (currently line 53)
private IDisposable _emailIsReadTimer;

private Func<TimerCallback, int, IDisposable> _readTimerFactory = (callback, dueTimeMs) =>
{
    var timer = new System.Threading.Timer(callback);
    timer.Change(dueTimeMs, System.Threading.Timeout.Infinite);
    return timer;
};
```

and at `Navigation.cs:223–224`:

```csharp
_emailIsReadTimer = _readTimerFactory(ApplyReadEmailFormat, MailReadDelayMilliseconds);
```

**Blast radius is fully contained inside F10.** `_emailIsReadTimer` is referenced at exactly five
places, all F10-owned: declaration `QfcItemController.cs:53`; `Navigation.cs:211, 213, 223, 224`;
`ViewerSetup.cs:420`. `System.Threading.Timer` implements `IDisposable`, so `Dispose()` at line 213
is unaffected by the retype. Verified by grep — no other file in the solution names the field.

Effects: lines 222–225 collapse to 1–2 lines, all deterministically coverable; the test asserts the
callback and due-time the factory received, and no real timer is ever created. The magic `4000`
becomes a named constant (General Code Change Policy § 5 — comment/name the *why*).

### 5.3 No other barriers

Every remaining collaborator in this file is already behind a seam:

| Collaborator | Seam | Proven at |
| --- | --- | --- |
| `_uiDispatcher` (lines 43, 59, 83, 90, 96, 102, 116, 128, 197, 202) | `UtilitiesCS.Threading.IUiDispatcher` | `QfcItemControllerTestSupport.BuildSyncDispatcher()` (TestSupport.cs:102–137) |
| `_itemViewer` (30, 33, 34, 54, 83, 117, 133–141, 209, 219) | `IItemViewer` | `Mock<IItemViewer>` throughout `NavigationTests.cs` |
| `_kbdHandler` (29, 42, 53, 60) | `IQfcKeyboardHandler` | `Mock<IQfcKeyboardHandler>` (NavigationTests.cs:164) |
| `_homeController.KeyboardHandler` (67, 76) | `IFilerHomeController` | `NavController` (NavigationTests.cs:24–41) |
| `_mailActions` (90, 96, 102) | `IMailItemActions` | `Mock<IMailItemActions>` (SeamCoreTests.cs:155) |
| `_parent` (176, 194) | `IQfcCollectionController` | `Mock<IQfcCollectionController>` |
| `_tlpStates` (209, 219) | real `TlpCellStates` + `TlpCellSnapShot` against a `Mock<IItemViewer>` whose `Controls` is a bare `new Control()` | `NavigationTests.cs:292–336`, `:345–389` |
| `Control` parameter of `JumpToAsync` (59) | handle-less `new Control()`; `Focus()` returns `false` silently | `NavigationTests.cs:208–227` |

**No Outlook Interop type is dereferenced except `MailItem` as a return value** at lines 90, 96, 102
(`_uiDispatcher.InvokeAsync<MailItem>(() => _mailActions.Reply())` then `reply.Display()`).
`MailItem` is an interop **interface** and is already `Mock<MailItem>`-able in this test project (used
at `EventHandlersTests.cs:296`). **No STA thread and no live WinForms form is required for any test
in this file.** The epic's STA last-resort clause does not apply.

---

## 6. State-transition invariants

| # | Invariant | Held by | Pinning test (§8) |
| --- | --- | --- | --- |
| I-1 | `JumpToFolderDropDown` toggles the keyboard dialog **before** marshalling the focus change | 29 then 30 | already pinned (`:160`) |
| I-2 | `JumpToAsync` reverses that order — focus **then** toggle | 59 then 60 | already pinned (`:208`); inconsistency recorded as D-5 |
| I-3 | `JumpToFolderDropDown[Async]` resets `_intEnterCounter` to 0 inside the marshalled delegate | 35, 47 | NV-9 |
| I-4 | `KbdExecuteAsync` toggles only when `deactivateKbd`, and **always** awaits the action | 65–69, 74–78 | already pinned, all four combinations (`:44`, `:66`, `:88`, `:111`) |
| I-5 | `ToggleConversationCheckbox()` (parameterless) inverts the current state | 116–118 | already pinned transitively |
| I-6 | `ToggleConversationCheckbox(On)` sets true **only if** currently false; `(Off)` sets false **only if** currently true (no redundant write, so no spurious `ConversationModeChanged`) | 132–139 | branches 133, 137 already 2/2 |
| I-7 | Any other `ToggleState` value falls to `default:` and **inverts** — including the composite `Off \| Force` used elsewhere in this class | 140–142 | **NV-3** (and D-4) |
| I-8 | `ToggleExpansion()` / `ToggleExpansionAsync()` route on `_expanded` and never double-apply | 149, 161 | already pinned all four ways (`:230`, `:244`, `:258`, `:272`) |
| I-9 | **Ordering:** `ToggleExpansion(state)` calls `_parent.ToggleExpansionStyle` **first**, then `ToggleExpansionOn/Off`, then `Register/UnregisterExpandedActions` | 176 → 179/183 → 180/185 | **NV-1, NV-2** |
| I-10 | **Ordering (async):** awaits `_parent.ToggleExpansionStyleAsync` first, then dispatches `ToggleExpansionOn/Off` through `_uiDispatcher`, then registers | 194 → 197/202 → 198/203 | **NV-4, NV-5** |
| I-11 | `ToggleExpansionOn` sets `_expanded = true` **before** evaluating the read-timer condition, so the flag is set even when the timer is not armed | 220 before 221 | NV-6 |
| I-12 | `ToggleExpansionOff` clears `_expanded` **before** disposing the timer, so the flag is cleared even if disposal throws | 210 before 211 | NV-7 |
| I-13 | `ToggleExpansionOff` is idempotent with respect to the timer: a second call with a null timer is a no-op | 211 | already pinned (`:292`) |
| I-14 | **Re-entrancy:** a second `ToggleExpansion(On)` without an intervening `Off` throws `ArgumentException` from `KbdActions.Add`, and leaks the previous timer | 180 + 223; `KbdActions.cs:94–97` | **NV-8** |
| I-15 | **Cross-variant:** sync and async expansion maintain disjoint `'B'`/`'D'` registries; a sync-expand followed by an async-collapse leaves the sync entries registered, and the next sync-expand throws | 180/185 vs 198/203 | **NV-10** (characterisation of D-2) |
| I-16 | **Dispose-before-setup:** after `Cleanup()`, `ToggleExpansionOff` throws `NullReferenceException` on `_tlpStates`/`_itemViewer`, and an armed timer still fires because `Cleanup()` nulls the field without disposing | `ViewerSetup.cs:410–420` | **NV-11** (characterisation of D-1) |
| I-17 | `Reply()`/`ReplyAll()`/`Forward()` call `Display()` **outside** the dispatched delegate, deliberately preserving the original thread affinity (documented at lines 88–89) | 90–91, 96–97, 102–103 | NV-12 (guards the documented ordering against a well-meaning refactor) |

---

## 7. Determinism requirements

| Concern | Location | Disposition |
| --- | --- | --- |
| **`new System.Threading.Timer(...)` + `Change(4000, Infinite)`** — a real 4-second wall-clock timer on the thread pool | 223–224 | **The principal determinism hazard in this file, and an in-scope finding in a file this child touches.** Covering lines 222–225 as written leaves a live timer running past the end of the test that will invoke `ApplyReadEmailFormat` against Moq stubs on a thread-pool thread. Resolve with the `_readTimerFactory` injectable-delegate seam (§5.2). **No test may arm the real timer**, and no test may wait for it — `Thread.Sleep`, `Task.Delay`, and wall-clock waits are prohibited. |
| `DateTime.Now` / `DateTime.UtcNow` / `Random` / `Random.Shared` | none | **Verified absent from this file.** No banned direct-clock read here. (Note: epic.md records a separate banned-`DateTime.Now` finding in F4's `MailItemInfoTests.cs:25`; nothing analogous exists in this file or its tests.) |
| `Thread.Sleep` / `Task.Delay` | none | Verified absent from production and from `NavigationTests.cs`. |
| UI-thread marshalling | 30 (`_itemViewer.Invoke`), 43, 59, 83, 90, 96, 102, 116, 128, 197, 202 (`_uiDispatcher.Invoke[Async]`) | All seamed. `BuildSyncDispatcher()` (TestSupport.cs:102) executes delegates inline; the `_itemViewer.Invoke` path at line 30 is covered by a `Callback<Delegate>(d => d.DynamicInvoke())` stub (`NavigationTests.cs:166–169`). **Exception:** line 30 uses `_itemViewer.Invoke` while its async twin at line 43 uses `_uiDispatcher.InvokeAsync` — an inconsistent seam, recorded as D-5. |
| Thread pool | the timer callback only | Removed by the §5.2 seam. |
| `async void` | none in this file | All async members return `Task`; every one is awaitable in tests. |
| Ambient `SynchronizationContext` | not mutated in this file | Contrast `QfcItemController.EventHandlers.cs`, which does. |

---

## 8. Proposed test case list

Fixtures: **A** = `HarnessController` + `QfcItemControllerTestSupport.SetField/GetField/InvokeNonPublic`;
**C** = `Mock<IItemViewer>`; **D** = `QfcItemControllerTestSupport.BuildSyncDispatcher()`;
**P** = `Mock<IQfcCollectionController>`; **K** = `Mock<IQfcKeyboardHandler>` with real
`KbdActions<>` instances (pattern at `EventWiringTests.cs:127–139`); **T** = real `TlpCellStates` +
`TlpCellSnapShot` over a bare `new Control()` host (pattern at `NavigationTests.cs:295–327`);
**F** = injected `_readTimerFactory` recording stub.

| ID | Target member | Scenario | Fixture | Closes |
| --- | --- | --- | --- | --- |
| **NV-1** | `ToggleExpansion(Enums.ToggleState.On)` — de-exempted | ordering — `_parent.ToggleExpansionStyle(ItemIndex, On)` called first, then `ToggleExpansionOn` runs, then sync `'B'`/`'D'` are registered | A+C+P+K+T+F | I-9; ~10 new lines, 2 new conditions |
| **NV-2** | `ToggleExpansion(Enums.ToggleState.Off)` — de-exempted | ordering — style first, then `ToggleExpansionOff`, then sync `'B'`/`'D'` removed | A+C+P+K+T | I-9; remaining new lines/conditions |
| **NV-3** | `ToggleConversationCheckbox(Enums.ToggleState)` | edge — an unrecognised value (e.g. `Off \| Force`) falls to `default:` and **inverts** the checkbox | A+C+D | lines 141–142, branch 130 |
| **NV-4** | `ToggleExpansionAsync(On)` — de-exempted | ordering — awaits `ToggleExpansionStyleAsync` first, dispatches `ToggleExpansionOn` through `_uiDispatcher`, then registers async `'B'`/`'D'` | A+C+P+K+T+D+F | line 197, I-10, ~10 new lines |
| **NV-5** | `ToggleExpansionAsync(Off)` — de-exempted | ordering — same for the Off arm | A+C+P+K+T+D | line 202, I-10 |
| **NV-6** | `ToggleExpansionOn()` | positive — `ItemHelper.UnRead == true` arms the read timer via the injected factory with callback `ApplyReadEmailFormat` and due time 4000 | A+C+T+F | lines 222–225, branch 221 (both sub-conditions) |
| **NV-7** | `ToggleExpansionOff()` | dispose — a non-null `_emailIsReadTimer` is disposed exactly once and `_expanded` is cleared first | A+C+T | lines 212–214, branch 211 |
| **NV-8** | `ToggleExpansion(On)` twice | re-entrancy — the second call throws `ArgumentException` naming key `'B'`, and the first timer instance is not disposed | A+C+P+K+T+F | I-14, characterises D-3 |
| **NV-9** | `JumpToFolderDropDown()` / `JumpToFolderDropDownAsync()` | state — `CounterEnter` is reset to 0 by both overloads | A+C+D+K | I-3 |
| **NV-10** | sync expand → async collapse → sync expand | ordering / cross-variant — the async collapse removes nothing and the second sync expand throws `ArgumentException` | A+C+P+K+T+D+F | I-15, characterises **D-2** |
| **NV-11** | `Cleanup()` then `ToggleExpansionOff()` | dispose-before-setup — throws `NullReferenceException`; separately, `Cleanup()` leaves an armed timer undisposed | A+C+T+F | I-16, characterises **D-1** |
| **NV-12** | `Reply()` | ordering — `_mailActions.Reply()` runs **inside** the dispatched delegate and `Display()` runs **outside** it | A+D+`Mock<MailItem>` | I-17 (guards the documented thread-affinity choice) |
| NV-13 | `Reply()` / `ReplyAll()` / `Forward()` | error — the dispatcher returns `null`; `Display()` throws `NullReferenceException` | A+D | characterises D-6 |
| NV-14 | `MenuDropDown()` | positive — dispatches `ShowMoveOptionsMenu()` exactly once through the seam | A+C+D | dedicated coverage for a currently only-transitively-covered member |

**Minimum set to raise the branch margin from 1 condition to 7: NV-3, NV-6, NV-7** — three tests,
taking branch to 30/30 (100 %) and line to ~98 %. **NV-1, NV-2, NV-4, NV-5 must land in the same
atomic change as the removal of the two `[ExcludeFromCodeCoverage]` attributes**, never before it and
never after it (see §1.2 — removal without coverage drops the file to ~77 %, below the gate). NV-8,
NV-10, NV-11 are the state-transition characterisation tests the epic's F10 brief specifically asks
for ("cover ordering, re-entrancy, and dispose-before-setup explicitly").

**Test-file placement.** `QfcItemController.NavigationTests.cs` is at **392 lines** against the
500-line ceiling; 14 new tests will breach it. Plan the split up front:
`QfcItemController.NavigationTests.cs` (jump/execute/toggle-checkbox routing, existing) and a new
`QfcItemController.NavigationExpansionTests.cs` (NV-1, NV-2, NV-4, NV-5, NV-6, NV-7, NV-8, NV-10,
NV-11). If `QuickFiler.Test.csproj` is a legacy non-SDK project with explicit `<Compile Include=...>`
(as `QuickFiler.csproj` is), the new file needs its own entry — verify before planning.

---

## 9. File-size and creation impact

- `QfcItemController.Navigation.cs` is **228 lines**. The §5.2 seam replaces two lines (223–224)
  with one; removing the two exemption attributes removes 2 lines (and their 4 comment lines may
  stay, rewritten as behavioural documentation). Net change is **negative**. No split needed.
- `QfcItemController.cs` is **323 lines**. Retyping `_emailIsReadTimer` to `IDisposable` is a
  1-line edit; adding `_readTimerFactory` and the `MailReadDelayMilliseconds` constant costs 6–8
  lines including comments. Projected ~331 lines — well under 500. No split needed.
- **No new production file is required.** Therefore no `<Compile Include=...>` entry in
  `QuickFiler/QuickFiler.csproj` and no ledger row are owed by this file. Should that change:
  `QuickFiler.csproj` is a legacy non-SDK project with **no globbing**, so the `<Compile Include>`
  entry is mandatory; **preserve CRLF** by using the Edit tool or `perl -0777` with explicit `\r\n`
  — never a git-bash `sed -i`, which strips CRLF and produces a whole-file diff guaranteed to
  conflict at fan-in; keep the hunk minimal and adjacent to related entries; and append a ledger row
  classified `testable` at **>= 90 %** per epic.md § "Mid-Wave File Creation".
- **Ledger action required even without a new file:** removing the two `[ExcludeFromCodeCoverage]`
  attributes changes this file's ledger disposition. F1's ledger records a disposition instruction
  per existing attribute; this child must record that both were removed as *reducible*, with the
  §1.2 evidence, rather than ratified.

---

## 10. Sibling boundaries — do not edit

| Artefact | Owner | Dependency from this file | Rule |
| --- | --- | --- | --- |
| `QuickFiler/Controllers/KeyboardHandler.cs` (414 lines, `[ExcludeFromCodeCoverage]`) | **F3 (#430)** | `_kbdHandler.ToggleKeyboardDialog()` (lines 29, 53), `ToggleKeyboardDialogAsync()` (42, 60), and `_homeController.KeyboardHandler.ToggleKeyboardDialog()` (67, 76). All consumed **through `IQfcKeyboardHandler`**, never the concrete type. | **Do not edit.** No change needed — `Mock<IQfcKeyboardHandler>` already suffices (`NavigationTests.cs:164`, `:187`, `:217`). Record as an interface-level dependency only. |
| `QuickFiler/Controllers/KbdActions.cs` (and `KaChar`/`KaKey`) | **F3 (#430)** | The registry semantics NV-8 and NV-10 assert: throw-on-duplicate `Add` (`KbdActions.cs:90–104`) and silent-`false` `Remove` (`:123–135`). | **Do not edit.** Tests construct real `KbdActions<>` instances and assert current behaviour. **Cross-child contract note:** if F3 makes `Add` idempotent, NV-8 and NV-10 change from "throws" to "no-op". |
| `QuickFiler/Helper Classes/TlpCellSnapShot.cs`, `QfcThemeControlSet.cs` | **F4 (#434)** | `_tlpStates["Expanded"/"Compressed"].ApplyState(_itemViewer)` (lines 209, 219); `TlpCellStates` and `TlpCellSnapShot.SnapCell` are constructed directly in the existing tests (`NavigationTests.cs:313`, `:325–326`). | **Do not edit.** Depend on the **current** `TlpCellSnapShot()` parameterless constructor, `SnapCell(TableLayoutPanel, Control)`, `TlpCellStates.TryAddState(string, List<TlpCellSnapShot>)`, and `ApplyState(IItemViewer)` shapes. Pin them with direct `new`/call sites so an F4 change breaks the build rather than the behaviour. |
| `QuickFiler/Helper Classes/ConversationResolver.cs` | **F4 (#434)** | Not referenced from this file. If a fixture needs one, use the inert two-argument constructor `(IApplicationGlobals, MailItem)` at `ConversationResolver.cs:64` **positionally** — do not change its shape. | No direct contact from this file. |
| `QuickFiler/Interfaces/IQfcDatamodel.cs` | **F5** | Not referenced from this file. | No contact. |
| `QuickFiler/Viewers/IItemViewer.cs` | **F14** | `FocusFolderDropDown`, `SetFolderDroppedDown`, `FocusSearch`, `ShowMoveOptionsMenu`, `ConversationModeChecked`, `Controls`, `Invoke` — all already on the interface. | **Do not edit.** No change needed. |
| `QuickFiler/Interfaces/IQfcCollectionController.cs`, `QfcCollectionController.cs` | **F11** | `_parent.ToggleExpansionStyle(ItemIndex, state)` (176) and `ToggleExpansionStyleAsync` (194); the mixed-variant call sites evidencing D-2 are at `QfcCollectionController.cs:1140, 1212, 1414, 1439, 1679`. | **Do not edit.** Consume through `Mock<IQfcCollectionController>`. **Cross-child contract note to F11:** D-2's fix, when scheduled, most likely belongs at `QfcCollectionController.cs:1439` (an `async` method calling the synchronous `ToggleExpansion()`), not in this file. |
| `QuickFiler/Controllers/QfcItemController.FocusAndTheme.cs` | **F10 — this child** | `ApplyReadEmailFormat(object)` (`:318`) is the timer callback referenced at line 223. | In scope, but owned by a **different per-file artifact**. The `_readTimerFactory` seam does not change that method; coordinate so the two plans do not both edit `QfcItemController.cs`'s field region in conflicting hunks. |
| `QuickFiler/Controllers/QfcItemController.EventWiring.cs` | **F10 — this child** | `Register/UnregisterExpanded[Async]Actions()` (lines 180, 185, 198, 203). | In scope, different per-file artifact. The V-1/D-2 analysis is shared between the two artifacts; NV-10 should be planned once, in this file's phase, and cross-referenced from the other. |
| `UtilitiesCS/Properties/AssemblyInfo.cs` | out of every child's assignment | `QuickFiler.Test` has no `InternalsVisibleTo` grant from `UtilitiesCS` (epic.md § "Cross-Child Constraints" 2). | **Do not edit.** Nothing here needs a `UtilitiesCS` internal — `IUiDispatcher`, `Enums.ToggleState`, and `TimerCallback` are all public. |

---

## Latent defects for promotion

Per epic.md § "Latent Defect Promotion", promote via the MCP promotion lifecycle. **Do not fix under
this child** — the epic's no-behaviour-change NFR forbids it, and prose in a feature folder is lost
at merge.

| ID | Defect | Location | Severity |
| --- | --- | --- | --- |
| **D-1** | **`Cleanup()` nulls `_emailIsReadTimer` without disposing it.** `ToggleExpansionOn` arms a 4-second one-shot timer; `Cleanup()` sets the field to null but never calls `Dispose()`, so an armed timer survives cleanup and invokes `ApplyReadEmailFormat` against a controller whose `_itemViewer`, `_themes`, and `_globals` are all null. Contrast `ToggleExpansionOff`, which does dispose correctly. | arm: `Navigation.cs:223–224`; correct dispose: `Navigation.cs:211–214`; missing dispose: `ViewerSetup.cs:420` | **Medium** |
| **D-2** | **Sync and async expansion variants maintain disjoint keyboard registries, and production mixes them.** `ToggleExpansion(On/Off)` registers/unregisters the **sync** `CharActions['B','D']` while `ToggleExpansionAsync(On/Off)` uses the **async** `CharActionsAsync['B','D']`, but `_expanded` is a single shared flag. `ActivateBySelectionAsync` (`QfcCollectionController.cs:1426`) calls the **synchronous** `ToggleExpansion()` at `:1439`, so a single item can be expanded by one variant and collapsed by the other. A cross-variant collapse removes nothing (`KbdActions.Remove` returns `false` silently, `KbdActions.cs:126–128`, and every call site discards it) and the next same-variant expand throws `ArgumentException("Cannot add key because it already exists. Key B …")` from `KbdActions.cs:94–97`. Full reproduction sequence in § 4.1. | `Navigation.cs:180, 185, 198, 203`; call sites `QfcCollectionController.cs:1140, 1212, 1414, 1439, 1679` | **Medium** — user-visible crash |
| **D-3** | **`ToggleExpansionOn` overwrites `_emailIsReadTimer` without disposing the previous instance.** A second `ToggleExpansion(On)` without an intervening `Off` leaks a live timer (and also throws from `RegisterExpandedActions` — see D-2). | `Navigation.cs:223` | **Low** |
| **D-4** | **`ToggleConversationCheckbox(Enums.ToggleState)`'s `switch` is not flag-aware, but the enum is used as flags elsewhere in this class.** `QfcItemController.Initialization.cs:186` passes the composite `Enums.ToggleState.Off \| Enums.ToggleState.Force` to a sibling toggle method. Passing any composite value here matches neither `case On` nor `case Off` and silently falls to `default:`, which **inverts** the checkbox — the opposite of what `Off \| Force` requests. No current caller passes a composite value to this overload, so it is latent, not live. | `Navigation.cs:130–143`; flag usage at `Initialization.cs:186` | **Low–Medium** |
| **D-5** | **Inconsistent dispatch seam between the two folder-jump overloads.** `JumpToFolderDropDown()` marshals through `_itemViewer.Invoke(...)` (line 30) while `JumpToFolderDropDownAsync()` marshals through the injectable `_uiDispatcher.InvokeAsync(...)` (line 43), for the same operation. The sync path bypasses the dispatch seam introduced for exactly this purpose. Ordering also differs from `JumpToAsync`, which focuses before toggling (59–60) where the others toggle before focusing (29–30, 53–54). | `Navigation.cs:29–30, 43, 53–54, 59–60` | **Low** |
| **D-6** | **`Reply()`, `ReplyAll()`, and `Forward()` dereference the dispatcher result without a null check.** `_mailActions.Reply()` returning null yields `reply.Display()` → `NullReferenceException` with no context. Three identical occurrences. | `Navigation.cs:90–91, 96–97, 102–103` | **Low** |
| **D-7** | **Magic number `4000` and unexplained `Timeout.Infinite`.** The mail-read delay is an unnamed literal with no comment explaining why 4 seconds. General Code Change Policy § 5.3 requires the *why* to be commented. | `Navigation.cs:224` | **Low** |
| **D-8** | **Toolchain gap (report to F1, not a production defect): a method-level `[ExcludeFromCodeCoverage]` does not exempt the method's compiler-generated lambda closures.** `<ToggleExpansionAsync>b__205_0/1` appear in the report at 0 % despite their containing method being exempt. Every exempt method containing a lambda silently contributes permanently-uncovered lines to its file's denominator, across the whole epic. | `Navigation.cs:191` vs report lines 25888–25897, 26028–26029 | **Informational — epic-wide** |

## Checked and clear

- **No `DateTime.Now`, `DateTime.UtcNow`, `Random`, `Random.Shared`, `Thread.Sleep`, or `Task.Delay`**
  in this file or in `QfcItemController.NavigationTests.cs`.
- **No live-COM predicate.** The only Outlook Interop contact is `MailItem` as a return value at
  lines 90, 96, 102; `MailItem` is an interop interface and is already `Mock<MailItem>`-able in this
  test project. No `Store`, `MAPIFolder`, or `Application` access.
- **No `+=`/`-=` event subscription is made in this file.**
- **No STA thread and no live WinForms form is required** for any test proposed here; the existing
  tests already prove a bare `new Control()` host with `Mock<IItemViewer>.Controls` suffices for the
  `TlpCellSnapShot` path (`NavigationTests.cs:295`, `:348`). The epic's STA last-resort clause does
  not apply to this file.
- **Register/unregister pairing is symmetric *within* each variant** (sync↔sync, async↔async). The
  defect is cross-variant only (D-2).
