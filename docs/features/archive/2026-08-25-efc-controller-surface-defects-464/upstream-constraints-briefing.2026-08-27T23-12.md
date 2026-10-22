# Upstream constraints briefing — efc-controller-surface-defects-464

- **Authored:** 2026-08-27T23-12 (UTC, `date -u` immediately before this write)
- **Authored by:** orchestrator (child of epic `quickfiler-bug-family`)
- **Sources read in full:** `docs/features/active/qfc-item-controller-defects-484/{spec.md,plan.2026-08-24T09-36.md}`
  and `docs/features/active/quickfiler-keyboard-action-defects-444/{spec.md,plan.2026-08-24T20-33.md}`
- **Status of those features:** both MERGED into this execution base (`69e83171`). Their design
  decisions are binding constraints, not suggestions.

## Authority

This briefing is subordinate to `plan.2026-08-25T07-01.md` and `spec.md` for **what to do**, and to
`plan-base-drift-addendum.2026-08-27T21-01.md` for **where things are on disk**. It adds only the
upstream obligations those documents do not already restate. Where this briefing and the plan disagree
about intent, the plan wins.

## Correction to the base-drift addendum

The addendum states `MoveFailureNotifier` is "a #444 member". It is a **#484** member
(`QuickFiler/Controllers/QfcItemController.MailActions.cs`). The addendum's conclusion is unaffected —
both features are merged into the base and its presence still proves the base carries #484 — but the
attribution is wrong and is corrected here rather than left to mislead a later reader.

## Directives addressed to feature 464 by name

484's spec contains three downstream notes written **to this feature**. They are directive:

1. **484 Downstream note 1 — the timer defect.** `EfcItemController.Cleanup()` has the identical
   undisposed-timer defect 484 fixed in its own controller: `_timer = null;` with no dispose, armed at
   `EfcItemController.cs:953-954`. Recommended correction, which this plan already carries: replace with
   `_timer?.Dispose(); _timer = null;` and add a null-collaborator early return to
   `EfcItemController.ApplyReadEmailFormat`.
2. **484 Downstream note 2 — unwiring audit.** `EfcItemController.Cleanup()` unwires only three of its
   subscriptions (`EfcItemController.cs:257-262`). Mirror each `+=` with a `-=` using the
   delegate-identity technique.
3. **484 Downstream note 3 — `ToggleNavigation` is NOT to be aligned.** `EfcItemController.ToggleNavigation(bool)`
   (`:958-979`) is a distinct implementation, is **not** defective in the #480 way, and **must not be
   aligned** with 484's fix. Leave it alone.

484's CHANGED table also states verbatim: **"Feature 464 must not copy the swallow-and-continue shape
into `EfcItemController`."**

## Binding contract — `KbdActions<TKey, UClass, VDelegate>` (444)

444's spec carries a section titled "Upstream contract (exhaustive) — required by features 464 and 489".
It binds this feature:

- **Class invariant:** at most one entry per `(SourceId, StoredKey)` pair. 444 added a guard to
  `public KbdActions(IEnumerable<UClass> list)` that now **throws** `ArgumentException` (param name
  `list`, message containing `already exists`) when the seed list repeats such a pair. 444 inventoried
  `EfcFormController.cs:358-367`, `:574-602` and `:631-676` as duplicate-free construction sites. If any
  edit in this feature introduces a duplicate `(SourceId, Key)` into one of those seed lists,
  **construction now throws at runtime**. This is live on our surface even though 444 declared the
  Explorer surface out of scope for its own edits.
- **Two equality paths, load-bearing:** `Add`/`Remove`/the new guard compare with the private static
  `StoredKeyEquals` (`EqualityComparer<TKey>.Default`); `Find`/`FindIndex`/`ContainsKey`/`FilterKeys`
  compare with the element-defined `KeyEquals`. `KaStringAsync.KeyEquals` is substring-matching and
  **side-effecting** (a per-keystroke `Activated` latch, `KaStringAsync.cs:57-105`) whose early return
  "must not be 'completed' into a fall-through". Do not collapse the distinction.
- **`Remove` is frozen.** A 444 acceptance criterion states `KbdActions.Remove` retains its `bool` return
  and its silent `false` for an absent pair, and that no `TryRemove`-style member is added. 444
  deliberately declined this. The discarded-`bool` question across 42 call sites (7 in
  `EfcItemController.cs`, 2 in `EfcFormController.cs`) was **promoted as a separate follow-up issue**, not
  solved. RC4 removal work in this feature must not absorb it.
- **`Keys.Down` product decision:** `ActionOk` binds to `Keys.Return` on every surface, with
  `EfcFormController.cs:365` cited as the Explorer precedent. Do not rebind.

## `CharActions` reachability — 444 is explicit, and it constrains #467

444 records that `CharActions` is read by `KeyboardHandler_KeyDown`
(`QuickFiler/Controllers/KeyboardHandler.cs:114-131`) and is **reached only from the Alt-key
`ProcessCmdKey` path**, while `CharActionsAsync` is read by `KeyDownTaskAsync` (`:170-177`) on the
ordinary keystroke path. 444 then declared a deliberate behaviour widening in its PR body: Alt+`B` and
Alt+`D` respond following an asynchronous expansion, because "both keys were always intended to be
available while an item is expanded."

Consequence for **#467** (RC10, the `ProcessCmdKey` Alt-chord guard in `EfcViewer`): the Alt-mnemonic
`ProcessCmdKey` → `CharActions` route is a **live, intended consumer path** on the QuickFiler surface.
The guard this feature adds is scoped to `EfcViewer` and must narrow only what `EfcViewer` claims. It must
not sever `CharActions` reachability. `KeyboardHandler.cs` is owned by #498 and is not ours to edit.

Consequence for **#459** (RC4): removing the `KbdActions<>` contract misuse must not break the
both-registries-in-sync invariant 444 established (`SyncExpandedRegistrations` is the sole caller of the
four expansion register/unregister methods in `QfcItemController.Navigation.cs`) nor the
`StoredKeyEquals`-vs-`KeyEquals` distinction. Note also `spec.md` §R-2 already records a disagreement with
444's spec about `CharActions` reachability; 444's statement above is the merged one and is the fixed
point for anything this feature asserts about the QFC surface. Where the disagreement concerns the **EFC**
surface, this feature's own reading governs, because 444 never inspected it.

## Established fault-boundary pattern — follow it, do not invent one

RC3 extracts five `async void` bodies in `EfcFormController.cs` into `internal async Task` members. The
in-repo patterns to mirror:

- **Inside a `Task`-returning member** (484's `MoveMailAsync`): log at error level, notify through an
  injectable seam, then **wrap-and-rethrow** with added context. A broad `catch (System.Exception)` is
  permitted only because it propagates with added context. `Token.ThrowIfCancellationRequested()` goes
  **outside the `try`** so `OperationCanceledException` is never re-wrapped.
- **At the `async void` rim itself** (444's `KeyboardHandler_KeyDownAsync`, `KeyboardHandler.cs:133-148`):
  catch and **log**; do not rethrow. 444 downgraded that defect's severity precisely because the
  exception is caught there. This is the shape RC3's `async void` adapters take, and it is why
  `spec.md` requires the boundary error sink's default delegate to be exactly one
  `logger.Error(message, exception)` call.
- **On a thread-pool timer callback post-teardown** (484's `ApplyReadEmailFormat`): a null-collaborator
  **early return that does not log**, because a post-teardown no-op is the expected steady state there.
  This is the shape RC1's guard on `EfcItemController.ApplyReadEmailFormat` takes.

`MoveFailureNotifier` itself is 484's seam: `internal Action<string> MoveFailureNotifier { get; set; }`
defaulting to `text => MessageBox.Show(text)`, marshalled through `_uiDispatcher.Invoke` when the
dispatcher is non-null and invoked directly when null. Its default body is deliberately uncovered
(modal dialog under headless test policy) and every failure-path test replaces it. RC3's error sink
should follow the same seam-and-default shape.

## 484 invariants in `QfcItemController` that must not break

This feature edits exactly one line of `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs` (the
incognito literal). 484 confirms it does **not** touch or depend on any WebView2 argument literal — zero
occurrences of `incognito`, `AdditionalBrowserArguments` or `CoreWebView2EnvironmentOptions` anywhere in
484's spec or plan — so the one-line edit intersects no 484 edit. Still binding if anything drifts:

- `Cleanup()` statement order: `UnwireEvents()` before `_itemViewer = null` and before `_kbdHandler = null`;
  `_emailIsReadTimer?.Dispose()` before nulling; `BreadcrumbUnhandledArrow` detach before
  `_breadcrumbViewer = null`. The convention is "detach then null".
- `Cleanup()` must remain callable on a partially-constructed controller.
- Surface stability: no member removed, no public member added, no interface modified.
  `IQfcItemController.cs:89` declares `ToggleNavigation(bool)` and `:78` declares `Task MoveMailAsync()`;
  both are implemented by `EfcItemController`. Those interface declarations are fixed points — which is
  an independent reason RC4/RC11 deletions must not remove either implementation.
- 484's line citations are **pre-change** locators. Resolve every 484 citation by member name, never by
  line number.

## Test conventions to reuse rather than reinvent

- MSTest + Moq + FluentAssertions only; no MSTest `Assert.*`. No `Thread.Sleep`, `Task.Delay`, wall-clock
  wait, `DateTime.Now`, unseeded randomness, or temporary files. No real `Form`, no `Show()`, no message
  pump; `NoLiveFormInTestAssemblyTests.cs` must stay green.
- **Deterministic timer technique (484, reused verbatim by this plan):** inject via reflection a
  `new Timer(_ => { }, null, Timeout.Infinite, Timeout.Infinite)` that can never fire; after `Cleanup()`
  assert the field is null and that `timer.Change(0, Timeout.Infinite)` throws `ObjectDisposedException`.
  Disposal is observed as state, not as a race. Keep `MailItemHelper.UnRead` at its `false` default and
  **never assign `UnRead`** — the setter writes through to `Item.Save()`.
- **Structural assertion for deletions:** prove removal with a repository-wide zero-hit identifier search
  recorded with command and zero-hit output. The executor must not recreate a deleted block in order to
  remove it. Use case-sensitive word-boundary regex so field names embedding a token do not false-match.
- **Detachment proof:** `Mock<IItemViewer>.VerifyRemove(v => v.X -= It.IsAny<EventHandler>(), Times.Once())`
  per event, plus a behavioural proof (raise after unwire, assert `Times.Never()`), rather than inspecting
  subscription lists.
- **Fixture patterns:** shared arrange helpers only (no test methods) in `*.TestSupport.cs`;
  `FormatterServices.GetUninitializedObject` plus reflection `SetField`/`GetField`/`InvokeNonPublic`;
  `BuildSyncDispatcher()` for synchronous `InvokeAsync`; Loose mocks must explicitly return
  `Task.CompletedTask` for `Task`-returning members, because a Loose mock otherwise returns a null `Task`.
- **Naming:** `Member_Condition_Expectation`.
- **Namespace trap:** in any test file that imports `Microsoft.Office.Interop.Outlook` without importing
  `System`, bare `Action` and `Exception` bind the **Outlook** types. Write `System.Action` and
  `System.Exception` fully qualified in every Efc-family test file that imports Outlook interop.
- **Runner conventions:** `vstest.console.exe` resolved via `vswhere`, `/InIsolation` mandatory,
  `/Settings:scripts\vscode\TaskMaster.cli.runsettings`, filter clauses joined with `|` (never `OR`),
  and **exclude any assembly path containing `\.claude\`** (match on the RELATIVE path). msbuild gates use
  `/t:Rebuild`, never `/p:Nullable=enable`. Run from PowerShell, never a POSIX shell — the Bash tool
  mangles a bare `/m` into `M:/`.
