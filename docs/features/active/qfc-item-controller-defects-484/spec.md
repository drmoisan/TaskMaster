# qfc-item-controller-defects (Spec)

- **Issue:** #484
- **Parent (optional):** epic `quickfiler-bug-family`; integration branch `epic/quickfiler-bug-family-integration`
- **Owner:** drmoisan
- **Last Updated:** 2026-08-24
- **Status:** Approved
- **Version:** 1.0

Work mode is `full-bug`. This document is the sole authoritative acceptance-criteria source for the
feature. `user-story.md` is intentionally absent.

Authoritative technical input: `docs/features/active/qfc-item-controller-defects-484/research/research.2026-08-24T09-45.md`.
Where that research corrects a promoted potential's "Suspected Fix", the research governs; research §0
tabulates all five corrections and each is carried forward below. In every other respect this document
governs: the research is a point-in-time technical input captured before the design was finalised, so
where the two disagree on a figure, a design detail, or a line citation, this document is authoritative
and the research is not to be treated as a correction of it. Two known divergences of that kind:

1. **Subscription count.** The `Cleanup()` row of research §7.2 states that the method detaches **22**
   additional subscriptions, a figure that predates the decision to reach the `WebResourceRequested`
   subscription through `DetachWebResourceRequestedHandler()`. The delivered figure is **23**, as stated
   in the CHANGED-members table below. Research §2.1's "22 `+=` in `EventWiring.cs`", §2.4's "22 of 24
   unwired symmetrically and testably", and §9.1's "22/24 yes" are not divergences: each counts a
   different quantity and each agrees with this document.
2. **`QuickFiler.Test.csproj` item-group ordering.** Research §8.4 describes the `Compile Include` item
   group at `QuickFiler.Test/QuickFiler.Test.csproj:57-175` as alphabetically ordered. It is not: the
   group is ordered by area and by insertion history (for example `Controllers\QfcItemController.EventWiringTests.cs`
   at `:142` precedes `Controllers\QfcItemController.NavigationTests.cs` at `:143` and
   `Controllers\QfcItemController.MailActionsTests.cs` at `:144`). The delivered statement is under
   Constraints below. The ordering claim is not load-bearing for any decision in this document: the
   `.csproj` prohibition rests on the item group being shared with sibling epic children, which is
   independently true.

Additionally, research §8.5 illustrates routing the new #480 `async: true` test into
`QfcItemController.EventWiringTests.cs`. That is an illustrative recommendation, not a divergence in
this document's sense, because this document names no file for that test: routing is delegated to the
plan's constraint C2 capacity table (see Test Strategy governing constraint 6), which is binding.

**Line-citation anchor (read this before resolving any `file:line` reference).** Every `file:line`
citation in this document is anchored to the **pre-change** source at the plan's `<BASE_SHA>`, not to
the delivered post-change source, unless the citation is one of the two the plan's capacity rule C2.7
explicitly preserves (`QuickFiler/Controllers/QfcItemController.EventWiring.cs:50` and
`QuickFiler.Test/Controllers/QfcItemController.EventWiringTests.cs:229-309`). This feature edits nine
files, and several of its own edits — the deletion at `QfcItemController.FocusAndTheme.cs:170`, the
statements inserted into `Cleanup()`, the `TryResolveCidResource` extraction, and the
`MoveMailAsync` catch replacement — shift the lines that follow them. Features 464 and 489 branch from
an integration branch that already carries this change and will therefore read this document against
post-change source: **resolve every citation by the member name it accompanies, not by the line number.**
Each citation names its member or statement in prose for exactly that reason. In particular, the
`Cleanup()` acceptance criteria below cite `ViewerSetup.cs:407`, `:420`, and `:424` as pre-change
locators for `_itemViewer = null`, `_kbdHandler = null`, and `_emailIsReadTimer = null`; the delivered
line numbers differ and are recorded by the plan's `[P6-T4]` from the delivered source.

---

## Context

- **Summary of the bug and its impact.** This feature closes five pre-existing defects in the
  `QfcItemController` partial classes. Four are teardown/lifecycle defects and one is a failure-path
  defect. Together they cause: a navigation-tips affordance that does nothing; 23 event
  subscriptions that are never detached (the 22 in `EventWiring.cs` plus the `WebResourceRequested`
  subscription at `ViewerSetup.cs:84`; the 24th, `BreadcrumbUnhandledArrow`, is already detached),
  so a torn-down controller keeps receiving events from a pooled viewer; an armed 4-second
  `System.Threading.Timer` that survives `Cleanup()` and fires against nulled fields; a mail-move
  failure that is swallowed so the caller treats a failed file as successful; and an
  inline-image handler that dereferences unguarded external input on a WebView2 callback thread.

- **Observed environment(s).** VSTO add-in hosted in Microsoft Outlook on Windows; `QuickFiler`
  assembly, target framework `v4.8.1` (`QuickFiler.Test/QuickFiler.Test.csproj:18`). All five defects are
  present on the current source of the four owned partials.

- **Customer impact and severity.** Per-issue severity as recorded in the promoted potentials:
  - #480 Medium — a UI affordance silently does nothing.
  - #481 Medium — handler execution against torn-down state, swallowed exceptions, controller-graph
    retention across pooled-viewer reuse.
  - #483 Medium-High — silent failure to file mail; the surrounding bulk flow proceeds as though the
    move succeeded.
  - #484 Medium — reachable `NullReferenceException` on a thread-pool thread, or read-formatting
    applied to a recycled viewer's wrong item.
  - #485 Low-Medium — inline image silently fails to render; the exception is undiagnosable because
    neither fault path logs.

- **First observed date and version(s) impacted.** All five were identified on 2026-08-07 during
  preparation research for epic #136 child F10 (issue #453) and were deferred out of that child because
  its non-functional requirement prohibited behaviour change to observable QuickFiler flows. Each defect
  alters observable behaviour on a teardown or failure path, so each requires its own regression test.

---

## Repro & Evidence

Reproduction is by source inspection and by unit-level exercise of the affected members. No live Outlook
session is required to observe four of the five; #485's production path additionally requires a live
WebView2 runtime, which is why the fix is extracted into a directly callable member (see Proposed Fix).

### #480 — `ToggleNavigation(bool)` toggles twice

- **Steps.** Call `ToggleNavigation(async: false)` or `ToggleNavigation(async: true)` on a controller
  whose `_itemPositionTips` is initialized, then observe the tip label's visibility.
- **Expected vs actual.** Expected: visibility flips once. Actual: unchanged.
  `QfcItemController.FocusAndTheme.cs:170` dispatches `_itemPositionTips.Toggle(false)`
  unconditionally, and exactly one of `:173` (async) or `:177` (sync) dispatches it again. There is no
  path through the method that toggles once. `QfcTipsDetails.Toggle(bool)`
  (`UtilitiesCS/HelperClasses/ToolTips/QfcTipsDetails.cs:193-203`) is a flip, not an idempotent set, so
  two flips restore the starting state.
- **Determinism.** Always.
- **Masking evidence.** The existing test
  `QuickFiler.Test/Controllers/QfcItemController.FocusAndThemeTests.cs:310-324` asserts
  `Times.AtLeastOnce()` at `:323`, which two invocations satisfy. It is green before and after the fix.

### #481 — no event unwiring path

- **Steps.** Wire a controller against a viewer, call `Cleanup()`, then raise any wired event on the
  viewer. The handler still executes against a controller whose fields have been nulled.
- **Expected vs actual.** Expected: a released controller receives no further events. Actual: it does.
- **Evidence (verified counts, research §2.1).** `QfcItemController.EventWiring.cs` carries **22** event
  `+=` operators (6 control-tree in `WireControlTreeEvents()` at `:40-62`, 16 intent in
  `WireIntentEvents()` at `:68-93`). `QfcItemController.ViewerSetup.cs` carries **2** (`:84-105`
  `WebResourceRequested`; `:159` `BreadcrumbUnhandledArrow`). The other eight partials carry none.
  Repository total across the ten `QfcItemController.*.cs` partials: **24**. `-=` operators total
  **3**, all for `BreadcrumbUnhandledArrow` (`ViewerSetup.cs:155`, `:158`, `:403`).
- **Correction to the promoted potential.** The potential states "25 `+=` in `EventWiring.cs`". That
  figure includes two commented-out lines (`EventWiring.cs:43`, `:80`) and the arithmetic
  `totalDelay += newDelay` (`:136`). The correct figure is 22.
- **Determinism.** Always.

### #483 — `MoveMailAsync` swallows every exception; missing cancellation checks

- **Steps.** Drive `MoveMailAsync` with a filer factory or a `FilerQueue` that faults (for example a
  `FilerQueueItem` constructor rejection for a null helper element,
  `QuickFiler/Controllers/FilerQueue.cs:70-78`), then observe the caller.
- **Expected vs actual.** Expected: the caller can distinguish a failed move from a successful one.
  Actual: the broad `catch (System.Exception e)` at `MailActions.cs:115-122` logs, shows a modal
  `MessageBox.Show` from a possibly non-UI thread, and returns normally. The sole production caller
  `QfcCollectionController.TryMoveEmailByGroupAsync` (`QuickFiler/Controllers/QfcCollectionController.cs:2236-2258`)
  therefore records nothing and continues as though the item was filed.
- **Second defect.** `MarkItemForDeletionAsync` checks cancellation as its first statement
  (`MailActions.cs:213`). `MoveMailAsync` (`:83-126`), `FlagAsTaskAsync` (`:183-200`), and
  `EnumerateConversationAsync` (`:49-52`) perform no equivalent check, so a cancelled bulk operation
  runs them to completion.
- **Determinism.** Always, on any faulting input.

### #484 — `Cleanup()` nulls an armed timer without disposing it

- **Steps.** Arm the read timer by selecting an unread item (`QfcItemController.Navigation.cs:221-225`
  arms `new System.Threading.Timer(ApplyReadEmailFormat)` for 4000 ms), then tear the controller down
  within four seconds.
- **Expected vs actual.** Expected: teardown cancels the pending callback. Actual:
  `ViewerSetup.cs:424` performs `_emailIsReadTimer = null;` only. The orphaned timer still fires
  `ApplyReadEmailFormat` (`FocusAndTheme.cs:318-324`) on a thread-pool thread, where it dereferences
  `ItemHelper` (nulled at `ViewerSetup.cs:422`), `_themes` (nulled at `:414`), and `_mailActions`.
- **Second defect.** `_mailActions` is not nulled by `Cleanup()`, and `SaveParameters` binds it with a
  null-coalescing assignment (`Initialization.cs:395-397`), so a re-parameterised controller retains the
  adapter bound to the previous `MailItem`.
- **Determinism.** Data-dependent: reachable whenever teardown occurs inside the 4000 ms window on an
  unread item. Silent when it occurs — the callback runs with no logging on the fault path.

### #485 — `WebResourceRequested` handler dereferences unguarded external inputs

- **Steps.** Render a mail body that requests a `cid:` resource with a malformed URI, or whose matching
  attachment entry has a null `AttachmentData`.
- **Expected vs actual.** Expected: the request is ignored with a debug-level diagnostic. Actual:
  `ViewerSetup.cs:86` calls `new Uri(e.Request.Uri)` unguarded (`UriFormatException` on malformed
  input); `:100` calls `new MemoryStream(match.AttachmentData)` unguarded (`ArgumentNullException` on a
  null payload — `CidImageResolver.BuildContentIdMap` does not filter on `AttachmentData`, so a map hit
  does not imply a payload, `UtilitiesCS/OutlookObjects/MailItem/CidImageResolver.cs:34-46`).
- **Third defect, not named in the promoted potential.** `ViewerSetup.cs:92` dereferences
  `ItemHelper.AttachmentsInfo` unguarded; `ItemHelper` is null after `Cleanup()` and the subscription is
  never detached (#481), so this is reachable.
- **Line-number correction.** The promoted potential cites `:83` and `:97`. The current lines are **86**
  and **100**.
- **Determinism.** Data-dependent on the rendered body's requested URIs and on attachment load state.

---

## Scope & Non-Goals

### In scope

The five defects above, confined to four production files and five test files.

**Production files this feature owns and may write:**

- `QuickFiler/Controllers/QfcItemController.FocusAndTheme.cs`
- `QuickFiler/Controllers/QfcItemController.EventWiring.cs`
- `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs`
- `QuickFiler/Controllers/QfcItemController.MailActions.cs`

**Test files this feature owns and may write** (all five already carry `Compile Include` entries in
`QuickFiler.Test/QuickFiler.Test.csproj` at lines 142, 144, 146, 150, and 153, so no `.csproj`
edit is required):

- `QuickFiler.Test/Controllers/QfcItemController.FocusAndThemeTests.cs`
- `QuickFiler.Test/Controllers/QfcItemController.EventWiringTests.cs`
- `QuickFiler.Test/Controllers/QfcItemController.ViewerSetupTests.cs`
- `QuickFiler.Test/Controllers/QfcItemController.MailActionsTests.cs`
- `QuickFiler.Test/Controllers/QfcItemController.TestSupport.cs` (shared arrange helpers only; no test method may be added; edits are additive only)

### Out of scope / non-goals

**Forbidden files.** These are owned by sibling epic children running concurrently on the same
integration branch and must not be created, modified, or deleted by this feature:

- `QuickFiler/Controllers/QfcItemController.Navigation.cs` (feature 444)
- `QuickFiler/Viewers/ItemViewer*.cs` (feature 489)
- `QuickFiler/Controllers/KbdActions.cs` (feature 444)

Additionally out of scope, and not to be written by this feature:

- `QuickFiler/Interfaces/IQfcItemController.cs`, `QuickFiler/Viewers/IItemViewer.cs` — no interface
  change is made (see Proposed Fix).
- `QuickFiler/Controllers/EfcItemController.cs`, `QuickFiler/Controllers/EfcFormController.cs` — owned
  by feature 464.
- `QuickFiler/Controllers/QfcCollectionController.cs` — the sole production caller of `MoveMailAsync`
  already catches, logs with subject context, and continues (`:2236-2258`); it needs no change.
- `QuickFiler/Controllers/QfcItemController.Initialization.cs` — `SaveParameters`' `??=` rebinding
  (`:395-397`) already produces the correct behaviour once `Cleanup()` nulls `_mailActions`.
- `QuickFiler.Test/Controllers/QfcItemController.SeamFactoryTests.cs` — left untouched to minimise
  the sibling-conflict surface; its existing tests are cited as the pattern source only.
- `QuickFiler.Test/QuickFiler.Test.csproj` and `QuickFiler/QuickFiler.csproj` — no `.csproj` edit.

**Excluded behaviour changes:**

- Removing the one-argument `ToggleNavigation(bool)` overload. It is declared on the public interface
  `IQfcItemController.cs:89` and implemented by `EfcItemController.cs:958`; deletion would require
  writing two unowned files. It is retained as dead production code with one test caller.
- Changing `MoveMailAsync`'s return type to a result object. `Task MoveMailAsync()` is declared on
  `IQfcItemController.cs:78` and implemented by the out-of-scope `EfcItemController`.
- Resetting the remaining `Cleanup()` field asymmetries (`_listTipsExpanded`, `_tlpStates`,
  `_conversationResolver`, `_activeTheme`, `_selectedFolder`, `_isWebViewerInitialized`, `_activeUI`,
  `_expanded`). No issue backs them, and `_isWebViewerInitialized` and `_activeUI` are read by the
  pooled-reuse path, so resetting them is an unbacked behaviour change (research §4.6).
- Introducing an injectable timer seam. The arming site is `Navigation.cs:223-224`, a forbidden file.
- Adding any new `[ExcludeFromCodeCoverage]` attribute anywhere.

### Downstream notes (out of scope; recorded for the named owners)

These are findings this feature verified but must not act on. They are recorded here because
`issue.md` requires that a fix appearing to need a forbidden file be captured in `spec.md` rather than
in the plan.

1. **Feature 464 (`EfcItemController.cs`, issue #463) — the identical undisposed-timer defect exists.**
   `EfcItemController.Cleanup()` at `EfcItemController.cs:277` performs `_timer = null;` without
   disposing, while `_timer` is armed at `:953-954` with the same
   `new System.Threading.Timer(ApplyReadEmailFormat)` + `Change(4000, Timeout.Infinite)` pattern.
   Recommended correction for that owner: replace `_timer = null;` with
   `_timer?.Dispose(); _timer = null;`, and add a null-collaborator early return to
   `EfcItemController.ApplyReadEmailFormat`.

2. **Feature 464 — `EfcItemController.Cleanup()` unwires only three of its subscriptions**
   (`EfcItemController.cs:257-262`: button `MouseEnter`/`MouseLeave` and `_globals.Ol.PropertyChanged`),
   while its wiring subscribes more. Recommended: audit `+=` across `EfcItemController.cs` and
   `EfcFormController.cs` and mirror each with a `-=` in `Cleanup()`, using the delegate-identity
   technique in research §2.2.

3. **Feature 464 — `EfcItemController.ToggleNavigation(bool)` (`:958-979`) is a distinct
   implementation** whose tips call is commented out (`:962-967`) and which flips `_activeUI`. It is not
   defective in the #480 way and must not be aligned with this fix, but its owner may wish to confirm
   the `_activeUI`-flipping semantics are intentional given that `:981-994` and `:996` are
   idempotent-by-state.

4. **Feature 444 (`Navigation.cs`, `KbdActions.cs`) — timer-seam opportunity.** `Navigation.cs:223-224`
   hard-constructs a `System.Threading.Timer` with a 4000 ms literal. A
   `Func<TimerCallback, System.Threading.Timer>` factory seam defaulted in `SaveParameters` (mirroring
   the five factory-delegate seams already at `QfcItemController.cs:69-89`) would make the arming branch
   deterministically testable without this feature's callback guard. This feature does not do it and
   does not depend on it.

5. **Feature 489 (`ItemViewer*.cs`) — viewer-side WebView2 detachment.** An
   `ItemViewer.ResetWebResourceInterception()` intent member on `IItemViewer`, called from
   `QfcItemController.UnwireEvents()`, which is where this feature places
   `DetachWebResourceRequestedHandler()` (third statement), reached from `Cleanup()`, would replace the
   two capture fields this feature adds
   (`_coreWebView2`, `_webResourceRequestedHandler`) with one mockable interface call, converting the
   single untestable residue of #481 into a covered assertion. This feature does not create that member
   because it would require writing `ItemViewer*.cs` and `IItemViewer.cs`.

6. **Report-only, no issue yet.** The `Cleanup()` field asymmetries and duplicate assignments listed
   under "Excluded behaviour changes" (duplicates at `ViewerSetup.cs:412`/`:415` for `_folderHandler`
   and `:407`/`:423` for `_itemViewer`) should be promoted as a new potential rather than absorbed here.

### Partial-fix boundary for issue #481 (research §2.4)

The owned-file fix reaches 23 of the 24 subscriptions with test coverage or pre-existing coverage:

- **16 intent subscriptions** — detached in `UnwireIntentEvents()`, verifiable with
  `Mock<IItemViewer>.VerifyRemove`.
- **6 control-tree subscriptions** — detached in `UnwireControlTreeEvents()`, verifiable with the
  existing headless-`ItemViewer` fixture.
- **1 `BreadcrumbUnhandledArrow` subscription** — already detached at `ViewerSetup.cs:403`.
- **1 `WebResourceRequested` subscription** — this is the boundary. It can be detached in code by
  capturing the delegate and the `CoreWebView2` source into two private fields in `ViewerSetup.cs`, but
  the detach cannot carry a regression test: the wiring site is inside `InitializeWebViewAsync`, which
  carries `[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]` (`ViewerSetup.cs:41`) because
  `.CoreWebView2` is null unless the real WebView2 runtime initialized the control, which the unit-test
  policy bars. **Decision: implement the capture-and-detach anyway** (a two-field, three-line change with
  no test cost) and record that this one member's detachment is verified by inspection, not by test. The
  member remains coverage-exempt under its pre-existing attribute; no new exemption is added.

---

## Root Cause Analysis

All five root causes are confirmed by source reading, not hypothesised.

| Issue | Confirmed root cause | Affected component |
|---|---|---|
| #480 | An unconditional statement precedes a branch that repeats it. `FocusAndTheme.cs:170` calls the flip-semantics `Toggle(bool)`; `:173`/`:177` call it again. The sibling overload at `:181-195` correctly calls the idempotent `Toggle(desiredState, false)` once per branch, indicating the one-argument overload was written by analogy without accounting for flip semantics. | `QuickFiler/Controllers/QfcItemController.FocusAndTheme.cs` |
| #481 | Asymmetric lifecycle: `WireEvents()` (`EventWiring.cs:28-32`) has no counterpart. `Cleanup()` (`ViewerSetup.cs:396-425`) nulls 16 distinct fields in 18 statements and detaches only `BreadcrumbUnhandledArrow`. Because QuickFiler pools and reuses item viewers, the event source outlives the controller, so every remaining subscription keeps a released controller reachable and live. Same defect class as #426 (`EmailMoveMonitor`) at larger scale. | `QfcItemController.EventWiring.cs`, `QfcItemController.ViewerSetup.cs` |
| #483 | A broad `catch (System.Exception)` at `MailActions.cs:115-122` that neither re-raises nor propagates, violating the General Code Change Policy §3.1 "unless you immediately re-raise or propagate with added context". Compounded by a raw `MessageBox.Show` inside the catch, which is modal and is not marshalled to the UI thread even though an `IUiDispatcher` seam is already injected (`QfcItemController.cs:66`, `Initialization.cs:38, 57, 383`). Separately, the cancellation convention established at `MailActions.cs:213` was not applied to three sibling async members. | `QuickFiler/Controllers/QfcItemController.MailActions.cs` |
| #484 | `Cleanup()` nulls a disposable field instead of disposing it (`ViewerSetup.cs:424`), while the re-arm path in the same type does dispose (`Navigation.cs:211-214`) — an oversight, not an intentional handoff. Disposal alone is insufficient: `System.Threading.Timer.Dispose()` does not abort a callback already executing, and `ApplyReadEmailFormat` (`FocusAndTheme.cs:318-324`) dereferences four collaborators that `Cleanup()` invalidates. The `_mailActions` staleness has a distinct root cause: the null-coalescing assignment at `Initialization.cs:395-397` only binds when the field is null, so a field retained across `Cleanup()` is never rebound. | `QfcItemController.ViewerSetup.cs`, `QfcItemController.FocusAndTheme.cs` |
| #485 | Externally-supplied values are validated after use rather than before: `new Uri(...)` runs before the emptiness test on the following line, and `TryGetValue` success is treated as implying a non-null payload. The `ItemHelper` dereference at `:92` is unguarded because the handler was written on the assumption that it can only run while the controller is live — an assumption #481 invalidates. | `QfcItemController.ViewerSetup.cs` |

---

## Proposed Fix

### Design summary (what changes where)

| Issue | Change | Owned file(s) |
|---|---|---|
| #480 | Delete the unconditional toggle at `FocusAndTheme.cs:170`. | `FocusAndTheme.cs` |
| #481 | Add `UnwireEvents()` / `UnwireControlTreeEvents()` / `UnwireIntentEvents()` mirroring the three wire methods; call `UnwireEvents()` from `Cleanup()` before `_itemViewer` and `_kbdHandler` are nulled; guard all three for null or non-`ItemViewer` collaborators; capture the `WebResourceRequested` delegate and its `CoreWebView2` source in private fields and detach. | `EventWiring.cs`, `ViewerSetup.cs` |
| #483 | Add the `MoveFailureNotifier` seam and the `NotifyMoveFailure` helper; in the catch, log, notify through the dispatcher, and wrap-and-rethrow; add `Token.ThrowIfCancellationRequested()` as the first statement of `MoveMailAsync`, `FlagAsTaskAsync`, and `EnumerateConversationAsync`. | `MailActions.cs` |
| #484 | `_emailIsReadTimer?.Dispose();` before the null at `ViewerSetup.cs:424`; null `_mailActions` in `Cleanup()`; add a null-collaborator early-return guard to `ApplyReadEmailFormat`. | `ViewerSetup.cs`, `FocusAndTheme.cs` |
| #485 | Extract `internal static bool TryResolveCidResource(...)` carrying the `Uri.TryCreate`, null-map, null-match, and null-`AttachmentData` guards; reduce the lambda to a two-statement adapter that also null-guards `ItemHelper`. | `ViewerSetup.cs` |

### Upstream contract (exhaustive) — required by features 464 and 489

Feature 464 (EFC controllers, via #463) and feature 489 (`ItemViewer`, via #486 and #489) branch from an
integration branch that already carries this change and will be authored against this surface. The
following is the complete surface delta.

#### ADDED members — accessibility, static-ness, exact signature

| Member | File | Accessibility | Static | Signature |
|---|---|---|---|---|
| `UnwireEvents` | `QfcItemController.EventWiring.cs` | `internal` | no | `void UnwireEvents()` |
| `UnwireControlTreeEvents` | `QfcItemController.EventWiring.cs` | `internal` | no | `void UnwireControlTreeEvents()` |
| `UnwireIntentEvents` | `QfcItemController.EventWiring.cs` | `internal` | no | `void UnwireIntentEvents()` |
| `MoveFailureNotifier` | `QfcItemController.MailActions.cs` | `internal` | no | `Action<string> MoveFailureNotifier { get; set; }`, defaulted to `text => MessageBox.Show(text)` |
| `NotifyMoveFailure` | `QfcItemController.MailActions.cs` | `private` | no | `void NotifyMoveFailure(string message)` — composes `_uiDispatcher` and `MoveFailureNotifier`; not part of the consumable surface |
| `TryResolveCidResource` | `QfcItemController.ViewerSetup.cs` | `internal` | **yes** | `static bool TryResolveCidResource(string requestedUri, IReadOnlyDictionary<string, IAttachment> contentIdMap, out byte[] payload, out string mimeType)` |
| `_webResourceRequestedHandler` | `QfcItemController.ViewerSetup.cs` | `private` field | no | `EventHandler<CoreWebView2WebResourceRequestedEventArgs>` |
| `_coreWebView2` | `QfcItemController.ViewerSetup.cs` | `private` field | no | `CoreWebView2` |
| `DetachWebResourceRequestedHandler` | `QfcItemController.ViewerSetup.cs` | `private` | no | `void DetachWebResourceRequestedHandler()` — detaches `_webResourceRequestedHandler` from `_coreWebView2` inside a guard requiring both fields to be non-null, then nulls both fields unconditionally in statements placed after and outside that guard; not part of the consumable surface |

`UnwireEvents`, `UnwireControlTreeEvents`, `UnwireIntentEvents`, `MoveFailureNotifier`, and
`TryResolveCidResource` are visible to `QuickFiler.Test` through
`[assembly: InternalsVisibleTo("QuickFiler.Test")]` (`QuickFiler/Properties/AssemblyInfo.cs:5`).

#### CHANGED members — behavioural delta, no signature change

| Member | File:line | Behavioural delta | Downstream consequence |
|---|---|---|---|
| `ToggleNavigation(bool async)` | `FocusAndTheme.cs:168-179` | One `_itemPositionTips.Toggle(false)` call instead of two. The dispatch count on `_itemViewer` drops from 2 to 1. | A downstream assertion of `Times.Exactly(2)` on `Invoke`/`BeginInvoke` or on `Toggle(false)` for this member would break. None exists today. |
| `Cleanup()` | `ViewerSetup.cs:396-425` | (a) Detaches 23 additional event subscriptions before nulling: 6 control-tree and 16 intent through `UnwireEvents()`, plus the `WebResourceRequested` subscription through `DetachWebResourceRequestedHandler()`. (b) Disposes `_emailIsReadTimer` before nulling it. (c) Nulls `_mailActions`. (d) Must tolerate a null `_itemViewer`, a null `_kbdHandler`, a null `Buttons`, and an `_itemViewer` that is not a concrete `ItemViewer`. Signature `public void Cleanup()` unchanged; still declared on `IQfcItemController.cs:77`. | **The most significant item for downstream.** See the lifecycle invariant below. `_mailActions` becomes null after `Cleanup()`; `SaveParameters`' `??=` (`Initialization.cs:395`) rebinds it on reuse. |
| `MoveMailAsync()` | `MailActions.cs:83-126` | (a) Wraps and rethrows (`InvalidOperationException` carrying the subject and destination folder, with the original as inner) instead of swallowing. (b) `Token.ThrowIfCancellationRequested()` becomes the first statement, so `OperationCanceledException` can escape. (c) The user-facing message is routed through `MoveFailureNotifier` on `_uiDispatcher` instead of a direct `MessageBox.Show`. Return type stays `Task`. | **Behavioural contract change.** Any caller must handle a faulted task. The sole production caller already does (`QfcCollectionController.cs:2238-2257`), so the bulk loop `MoveEmailsAsync` (`:2206-2228`) cannot be aborted by the rethrow. Feature 464 must not copy the swallow-and-continue shape into `EfcItemController`. |
| `FlagAsTaskAsync()` | `MailActions.cs:183-200` | Adds `Token.ThrowIfCancellationRequested()` as the first statement, before the `Mail` COM read. | Can now throw `OperationCanceledException`. |
| `EnumerateConversationAsync()` | `MailActions.cs:49-52` | Adds `Token.ThrowIfCancellationRequested()` as the first statement. | Can now throw `OperationCanceledException`. Reachable via `RightKeyActionsAsync["&Expand"]` (`MailActions.cs:78`). |
| `ApplyReadEmailFormat(object state)` | `FocusAndTheme.cs:318-324` | Adds an early-return guard on a null `ItemHelper`, `_themes`, `_activeTheme`, or `_mailActions`. Signature unchanged; still declared on `IQfcItemController.cs:50`. | Becomes a silent no-op against a torn-down controller instead of throwing `NullReferenceException`. A downstream test asserting the throw would break; none exists. |
| `InitializeWebViewAsync()` | `ViewerSetup.cs:42-128` | The `WebResourceRequested` lambda body is replaced by a two-statement adapter over `TryResolveCidResource`; the delegate and its `CoreWebView2` source are captured into fields. Remains `internal async Task` and remains `[ExcludeFromCodeCoverage]`. | No signature change. Feature 489 should note that the handler now tolerates a malformed URI, a null attachment payload, and a null `ItemHelper` by returning without setting `e.Response`, so the request falls through to the runtime's default handling. |

#### REMOVED members and interface changes

**No member is removed. No public member is added. No interface is modified.** All added members are
`internal` or `private`. `IQfcItemController`, `IItemControler`, and `IItemViewer` are untouched.
`ToggleNavigation(bool async)` is retained specifically because it is declared on
`IQfcItemController.cs:89` and implemented by `EfcItemController.cs:958`. This is the stability guarantee
features 464 and 489 may rely on: the only changes visible to a downstream consumer are behavioural, and
they are exhaustively enumerated in the CHANGED table above.

#### Event-wiring ORDER facts

- **Wiring order is unchanged.** `WireEvents()` continues to call `WireControlTreeEvents()` then
  `WireIntentEvents()` (`EventWiring.cs:28-32`), and neither method's internal ordering changes.
- **Unwiring order is newly defined.** `UnwireEvents()` calls `UnwireControlTreeEvents()`, then
  `UnwireIntentEvents()`, then `DetachWebResourceRequestedHandler()`, in that order. The first two
  mirror the wiring order; the third has no wiring counterpart because the `WebResourceRequested`
  subscription is made inside `InitializeWebViewAsync` rather than in a wire method. Detachment is
  order-independent across disjoint event sources, so this is a convention rather than a correctness
  constraint, but downstream code that mirrors the pattern should follow it.

#### `Cleanup()` statement-order constraints (three; all must be preserved by any downstream reordering)

1. The `UnwireEvents()` call must precede `_itemViewer = null` (`ViewerSetup.cs:407`) — `_itemViewer` is
   the intent-event source and the `ForAllControls` root — and must precede `_kbdHandler = null`
   (`:420`), because `_kbdHandler` is the delegate **target** for the two control-tree keyboard
   subscriptions and for `FolderKeyDown`; a null `_kbdHandler` at re-form time throws
   `NullReferenceException`, and a different instance would make the `-=` a silent no-op. Placing
   `UnwireEvents()` immediately after `ResetBreadcrumb()` (`:400`), or immediately after the existing
   breadcrumb detach block and before `:406`, satisfies every constraint. `Buttons` is never nulled by
   `Cleanup()` (`_buttons`, `QfcItemController.cs:95`), so the button loop is safe at either placement.
2. The `_emailIsReadTimer?.Dispose()` call must precede `_emailIsReadTimer = null` (`:424`).
3. The existing `BreadcrumbUnhandledArrow` detach (`:403`) must continue to precede
   `_breadcrumbViewer = null` (`:404`). This pre-existing pairing establishes the "detach then null"
   convention the change extends.

#### New post-`Cleanup()` lifecycle invariant (feature 489 may rely on this)

**After `Cleanup()` returns, a pooled `ItemViewer` handed back to the pool carries zero event
subscriptions from the released controller**, with one documented exception: the WebView2
`WebResourceRequested` subscription on `((ItemViewer)_itemViewer).L0v2h2_WebView2.CoreWebView2`, whose
detachment is implemented but is verified by inspection rather than by test (see the partial-fix boundary
in Scope & Non-Goals). Feature 489 must not assume post-`Cleanup()` handler delivery from a pooled
viewer. A second consequence of the same invariant: `_mailActions` is null immediately after `Cleanup()`
and is rebound to the new `MailItem` by the next `SaveParameters` call.

### Boundaries and invariants to preserve

- `Cleanup()` must remain callable on a partially-constructed controller. Two existing tests rely on
  this: `QfcItemController.ViewerSetupTests.cs:347-376` (`Cleanup_NullsTrackedPrivateFields`) leaves
  `_kbdHandler` and `Buttons` null and supplies a plain `Mock<IItemViewer>` that cannot be cast to the
  concrete `ItemViewer`; `QfcItemControllerBreadcrumbDropDownTests.cs:125-153`
  (`Cleanup_ResetsInjectedHostForPooledViewerReuse`) also calls `Cleanup()` with a null `_kbdHandler`.
  Both fail if the unwire path is unguarded. The required guards are:
  - `UnwireIntentEvents()` — `if (_itemViewer is null) { return; }`, plus a `_kbdHandler`-null guard
    around the `FolderKeyDown` detach only.
  - `UnwireControlTreeEvents()` — `if (!(_itemViewer is ItemViewer viewer)) { return; }` for the
    `ForAllControls` walk, mirroring the existing `EnsureBreadcrumbPipeline` guard at
    `ViewerSetup.cs:138-141`; the `ForAllControls` keyboard-detach walk additionally guarded by
    `if (_kbdHandler is not null)`, because `_kbdHandler` is the delegate target for both keyboard
    detachments and `QfcItemControllerBreadcrumbDropDownTests.cs:125-153` calls `Cleanup()` with a
    concrete `ItemViewer` and a null `_kbdHandler`, which the type guard does not catch; the `Buttons`
    and `MenuItems` loops guarded against null.
  - The asymmetry with the unguarded `WireControlTreeEvents()` is intentional — wiring runs only on the
    initialized path, teardown must tolerate a partially-constructed controller — and must be commented
    in the source.
- `UnwireControlTreeEvents()` must pass the same exclusion list as `WireControlTreeEvents()`
  (`new List<Control> { ((ItemViewer)_itemViewer).L0vhBreadcrumb_WebView2 }`, matching
  `EventWiring.cs:50`) so that `ForAllControls`
  (`UtilitiesCS/Extensions/WinFormsExtensions.cs:57-71`, a deterministic depth-first recursion with an
  exclusion set) visits exactly the control set that was wired.
- Delegate identity: detachment re-forms the delegate at the unwire site.
  `System.Delegate` equality compares `Method` and `Target`, so a fresh
  `new KeyEventHandler(_kbdHandler.KeyboardHandler_KeyDownAsync)` removes the earlier subscription
  provided `_kbdHandler` is the same instance. The in-repo precedent is
  `EfcItemController.Cleanup()` at `EfcItemController.cs:257-262`.
- `Navigation.cs` must continue to work unchanged: `:211-213` null-checks before disposing and
  `Timer.Dispose()` is idempotent; `:223` unconditionally overwrites the field with a new `Timer`.
- `Uri.TryCreate` must use `UriKind.Absolute`. `Uri.Segments` throws `InvalidOperationException` on a
  relative `Uri`, so `UriKind.RelativeOrAbsolute` would move the throw one line later rather than
  removing it.
- `ResolveImageMimeType` (`ViewerSetup.cs:197-205`) must stay `static`, because the extracted
  `TryResolveCidResource` is static. `logger` is a `static readonly` field
  (`QfcItemController.cs:30`), so it is reachable from a static member.
- `Token.ThrowIfCancellationRequested()` must be placed **outside** the `try` block in `MoveMailAsync`
  (as the first statement of the body, before the `if (ItemHelper is not null)` at `:87`), so the new
  catch cannot swallow or re-wrap `OperationCanceledException`. `Token` defaults to
  `default(CancellationToken)` (`QfcItemController.cs:267`, assigned from `_homeController.Token` at
  `Initialization.cs:377`), on which the call is a no-op, so every existing test that leaves `Token`
  unset is unaffected.

### Dependencies or blocked work

None blocking. This feature is an upstream dependency of features 464 and 489; both branch from an
integration branch that already carries this change.

### Implementation strategy (what changes, not sequencing)

#### Files/modules to change

Production: the four owned partials listed in Scope & Non-Goals. Tests: the five owned test files.
Nothing else.

#### Functions/classes/CLI commands impacted

`ToggleNavigation(bool)`, `ApplyReadEmailFormat(object)`, `Cleanup()`, `InitializeWebViewAsync()`,
`MoveMailAsync()`, `FlagAsTaskAsync()`, `EnumerateConversationAsync()`, plus the nine added members
tabulated above. No CLI surface exists for this assembly.

#### Data flow and validation changes

The `WebResourceRequested` decision path is restructured. The lambda adapter builds the content-ID map
from `ItemHelper?.AttachmentsInfo` (null-tolerant; `CidImageResolver.BuildContentIdMap` returns an empty
ordinal-ignore-case dictionary for a null argument, `CidImageResolver.cs:38-42`) and delegates the
decision to `TryResolveCidResource`, which returns `false` — serving no response and letting the request
fall through to the runtime's default handling — for each of: an unparsable or non-absolute URI, an empty
final URI segment, a null map, a map miss, a null match, and a null `AttachmentData`. Only on `true` does
the adapter construct the `MemoryStream` and call `CreateWebResourceResponse`. No other data flow changes.

#### Error handling and logging updates

- `MoveMailAsync`'s catch logs at error level with the existing `logger`, invokes `NotifyMoveFailure`,
  and throws `new InvalidOperationException($"Failed to file mail '<subject>' to '<folder>'.", e)`. A
  bare `throw;` is an acceptable simpler alternative that also satisfies the policy; the wrapped form is
  preferred because it carries the subject and destination folder that the caller otherwise obtains only
  by re-reading COM (`QfcCollectionController.cs:2245-2252` wraps that read in its own try/catch
  precisely because it can fail). A type-narrowed multi-catch is explicitly rejected: no narrow type set
  covers `ArgumentNullException` + `InvalidOperationException` + `ObjectDisposedException` +
  `COMException` without either omitting a real case or admitting `NullReferenceException`, and the
  General Code Change Policy §3.1 permits a broad catch that propagates with added context.
- `TryResolveCidResource` logs at debug level for an unparsable URI and for a map hit whose
  `AttachmentData` is null, so a missing payload becomes diagnosable.
- `ApplyReadEmailFormat`'s new guard returns silently; it does not log, because it runs on a thread-pool
  timer callback where a post-teardown no-op is the expected steady state.

#### Rollback/feature-flag considerations

None. No feature flag is introduced. Rollback is a revert of the branch.

### Technical specifications (interfaces/contracts)

#### Inputs/outputs and formats

`TryResolveCidResource(string requestedUri, IReadOnlyDictionary<string, IAttachment> contentIdMap, out byte[] payload, out string mimeType)`
returns `true` with a non-null `payload` and a non-null `mimeType` only when the URI is absolute, its
last segment (trimmed of `/`) is non-empty, the map contains that key, the match is non-null, and
`match.AttachmentData` is non-null. On `false` both `out` values are `null`. `mimeType` is produced by
the existing `ResolveImageMimeType` (`ViewerSetup.cs:197-205`), which already lower-cases null-safely and
defaults to `"application/octet-stream"`.

`MoveFailureNotifier` accepts the fully-composed user-facing message string; the default implementation
forwards it to `MessageBox.Show(text)`. `NotifyMoveFailure` marshals through `_uiDispatcher.Invoke` when
the dispatcher is non-null and invokes the notifier directly when it is null, because existing tests
(`SeamFactoryTests.cs` `MoveMailAsync_*`) do not set `_uiDispatcher`.

#### Required configuration keys and defaults

None. No configuration key, app setting, or environment variable is added or read.

#### Backward-compatibility expectations

No public member and no interface changes, so no source or binary compatibility break for any consumer
of the `QuickFiler` assembly. The three behavioural changes a consumer can observe are the `MoveMailAsync`
rethrow, the new `OperationCanceledException` paths on three async members, and the post-`Cleanup()`
lifecycle invariant. All three are enumerated above.

#### Performance constraints (latency/throughput/memory)

No latency or throughput target changes. `Cleanup()` performs additional work proportional to the control
tree already walked by `WireControlTreeEvents()`, which is bounded and executed once per teardown.
Disposing the timer and detaching subscriptions reduces retained memory: the released controller graph is
no longer reachable from a pooled viewer's event sources. `Timer.Dispose(WaitHandle)` is explicitly not
used, because it would introduce a blocking wait on the UI thread during teardown with a real deadlock
risk (`ApplyReadEmailFormat` calls `Theme.SetMailRead(async: true)`, which itself dispatches).

---

## Assumptions, Constraints, Dependencies

### Assumptions

- `_kbdHandler` is the same instance at unwire time as at wire time. Verified: it is assigned once in
  `SaveParameters` and is not reassigned between wiring and `Cleanup()`.
- `[assembly: InternalsVisibleTo("QuickFiler.Test")]` remains in force
  (`QuickFiler/Properties/AssemblyInfo.cs:5`; a duplicate exists at
  `QuickFiler/Controllers/QfcHomeController.cs:18`). The new `internal` members are only test-reachable
  through it.
- The worktree can complete `nuget restore TaskMaster.sln` and `dotnet tool restore`. Neither
  `packages/` nor `.dotnet-sdk/` is present in this worktree, and `global.json` declares
  `paths: [".dotnet-sdk", "$host$"]`, so `dotnet` resolves through the machine-installed SDK or fails
  with the declared error message pointing at `scripts/vscode/Install-RepoDotNetSdk.ps1`.

### Constraints

- **File-size ceiling of 500 lines** (General Code Change Policy §4.1) applies to every production and
  test file touched. Current sizes and headroom are recorded under Risks & Mitigations.
- **No `.csproj` edit.** All nine owned files already carry `Compile Include` entries — the four
  production partials in `QuickFiler/QuickFiler.csproj` and the five test files in
  `QuickFiler.Test/QuickFiler.Test.csproj`. The
  `QuickFiler.Test.csproj` `Compile Include` item group (lines 57-175) is shared with sibling epic
  children and is appended to by each of them, so an edit risks a merge conflict. The group is ordered
  by area and by insertion history, not alphabetically — research §8.4's "alphabetically-ordered item
  group" is superseded here (see the divergence list at the top of this document) — so there is no
  stable insertion position that would avoid such a conflict.
- **No forbidden-file write** (Scope & Non-Goals).
- **Test policy**: MSTest, Moq, FluentAssertions only; no `Thread.Sleep`, no `Task.Delay`, no wall-clock
  wait, no temporary file.
- **Repository-wide line coverage floor of 80%**, with `>= 90%` for new modules, classes, and methods,
  and no coverage reduction on changed lines (CLAUDE.md General Unit Test Policy UT2).

### External dependencies

No new package reference, no new project reference, no new assembly reference. `Moq`, `MSTest`, and
`FluentAssertions` are already referenced by `QuickFiler.Test`. `CoreWebView2` and
`CoreWebView2WebResourceRequestedEventArgs` are already referenced by `QuickFiler` and are used only in
the existing `[ExcludeFromCodeCoverage]` member and in the two new private fields.

---

## Data / API / Config Impact

- **User-facing or API changes.** No public API change and no interface change. User-visible behaviour
  changes: the navigation-tips affordance now works (#480); a failed mail move now surfaces through the
  same dialog text but no longer leaves the bulk flow believing the move succeeded (#483); an inline
  image whose attachment payload is missing now fails silently with a debug log line instead of throwing
  on a callback thread (#485).
- **Data or migration considerations.** None. No persisted data, schema, or stored format is touched.
- **Logging/telemetry updates.** Two debug-level log statements are added inside `TryResolveCidResource`
  (unparsable URI; null `AttachmentData`). The existing error-level log in the `MoveMailAsync` catch is
  retained and is joined by the wrapped rethrow. No logging framework, sink, or level configuration
  changes.
- **Compatibility notes.** No CLI flag, no config schema, no version bump. `QuickFiler` consumers compile
  unchanged.

---

## Test Strategy

### Governing constraints

1. **Regression test first.** Per the CLAUDE.md Bugfix Workflow, each of the five defects gets its
   failing regression test written and observed failing against the unfixed code before the
   corresponding production change is made. #480's test is a *tightening* of an existing assertion, and
   the tightened form must be demonstrated to fail against the unfixed code (`Times.Once()` against two
   actual invocations).
2. **Framework and libraries.** MSTest (`[TestClass]`, `[TestMethod]`, `[Timeout]`), Moq for every
   collaborator, FluentAssertions for every assertion. This matches all five owned test files today; none
   of them uses an MSTest `Assert.*` call.
3. **Banned APIs.** `Thread.Sleep`, `Task.Delay`, and any real wall-clock wait are prohibited in test
   code (`.claude/rules/general-unit-test.md`). The #484 timer test must use the deterministic approach
   below.
4. **No temporary files.** Creation and use of temporary files in tests is prohibited without an
   authorized exception; none is authorized.
5. **Seam-and-inject only.** No new test may construct a real `ItemViewer` or start a real WinForms
   message pump, with **one documented exception**: the #481 control-tree unwire test, which must mirror
   the existing headless fixture at `QfcItemController.EventWiringTests.cs:229-309` — no `Show()`, no
   message loop, no worker; events raised by reflecting onto `Control.OnPreviewKeyDown`, `OnKeyDown`, and
   `OnMouseEnter`, with a bare `SynchronizationContext` installed and restored in `try`/`finally`.
   Pre-existing real-`ItemViewer` and `WinFormsPumpHost` usages elsewhere in the owned test files
   (`EventWiringTests.cs:236`, `:327`; `ViewerSetupTests.cs:395`, `:433`, with the `WinFormsPumpHost` at
   `ViewerSetupTests.cs:429`) are precedent for that single
   exception only and are not a licence for new tests.
6. **File-size routing.** `QfcItemController.FocusAndThemeTests.cs` is at 497 lines (3 spare) and
   `QfcItemController.ViewerSetupTests.cs` at 474 (26 spare), so neither can absorb a test group. New
   tests are routed per the plan's constraint C2 capacity table, which supersedes the illustrative
   routing recorded here; the binding requirement is only that no owned test file exceeds 500 lines.
   The #480 assertion tightening itself is a zero-line, in-place edit to `FocusAndThemeTests.cs:323`
   and is unaffected by routing. Each relocated test group carries a header comment naming its issue.
   All five owned test files already have `.csproj` entries, so **no `.csproj` edit is required**.

### Deterministic timer test for #484 (no `Thread.Sleep`, no `Task.Delay`)

**T1 — disposal is observable via `ObjectDisposedException` on `Change`.** Arrange a
`new Timer(_ => { }, null, Timeout.Infinite, Timeout.Infinite)` — armed with `Timeout.Infinite` so it can
never fire during the test — and reflection-inject it into `_emailIsReadTimer`. Act: call `Cleanup()`.
Assert: the field is null, and `Action act = () => timer.Change(0, Timeout.Infinite);` throws
`ObjectDisposedException`. The assertion is on the disposal state, not on a race; it is fully
deterministic with no wall-clock dependency and no temporary file.

**T2 — the callback guard is directly invocable.** `ApplyReadEmailFormat(object state)` is public. Call
it on a freshly-`Cleanup()`ed controller and assert `act.Should().NotThrow()`, plus
`mailActions.Verify(m => m.Save(), Times.Never())` against a `Mock<IMailItemActions>` captured before
`Cleanup()`. This proves the post-teardown callback is inert without ever scheduling a timer.

No existing test arms a real timer — `QfcItemController.NavigationTests.cs:345-389` deliberately leaves
`ItemHelper` null so the arming branch at `Navigation.cs:221-225` is skipped — so T1 and T2 introduce no
cross-test timing coupling.

### Regression tests to add or update, by issue

- **#480.** Tighten `FocusAndThemeTests.cs:323` from `Times.AtLeastOnce()` to `Times.Once()` in place;
  add the currently-untested `async: true` branch with the same exact-count assertion, routed to the
  owned test file named by the plan's constraint C2 capacity table per governing constraint 6 above.
  No file is named for it here: the capacity table is the binding routing, and a file named in this
  bullet would contradict it. The new test arranges its own executing `Mock<IItemViewer>` inline,
  mirroring `BuildExecutingViewer()` (`FocusAndThemeTests.cs:99-115`), which executes both `Invoke` and
  `BeginInvoke` delegates synchronously so that both branches produce a countable `Toggle(false)` call
  on the `Mock<IQfcTipsDetails>`; that helper is `private static` to `FocusAndThemeTests.cs` and is
  therefore not reachable from another test file.
- **#481 intent half.** `Mock<IItemViewer>` with
  `VerifyRemove(v => v.ConversationModeChanged -= It.IsAny<EventHandler>(), Times.Once())` for each of
  the 16 intent events, or a representative subset plus a "no subscription remains" assertion. `VerifyAdd`
  is already used against this exact mock at `SeamFactoryTests.cs:250-259`, so `VerifyRemove` is a proven
  technique on this surface.
- **#481 control-tree half.** The single documented real-`ItemViewer` exception: wire, unwire, raise
  `OnPreviewKeyDown`/`OnKeyDown`/`OnMouseEnter` by reflection, assert
  `mockKbd.Verify(..., Times.Never())` and an unchanged `BackColor`.
- **#481 teardown robustness.** `Cleanup()` on a controller with a null `_kbdHandler`, a null `Buttons`,
  and an `_itemViewer` that is a plain `Mock<IItemViewer>` (not castable to `ItemViewer`) must not throw.
  This protects the two existing `Cleanup()` tests.
- **#483.** Faulting `_emailFilerFactory` asserts the wrapped rethrow, exactly one `MoveFailureNotifier`
  invocation, and an error-level log; faulting `FilerQueue.Enqueue` via a null helper element asserts the
  `ArgumentNullException` is wrapped; a pre-cancelled `Token` asserts `OperationCanceledException` with
  the factory never invoked, for each of `MoveMailAsync`, `FlagAsTaskAsync`, and
  `EnumerateConversationAsync`; the existing happy-path test (`SeamFactoryTests.cs:191`) must stay green.
  Collaborators: `Mock<IApplicationGlobals>`, `Mock<IFileSystemFolderPaths>`,
  `Mock<IFilerHomeController>`, the injected `_emailFilerFactory`, and the new `MoveFailureNotifier`.
- **#484.** T1 and T2 above, plus: `_mailActions` is null after `Cleanup()`; a `SaveParameters` call
  after `Cleanup()` rebinds `_mailActions` to the new `MailItem`, proving the `??=` reuse fix.
- **#485.** Seven cases against `TryResolveCidResource`, with no controller instance at all: malformed
  URI (`"::not a uri::"`), relative URI (`"/x/y"`), absolute URI with an empty final segment
  (`"https://cid.quickfiler.local/"`), map miss, map hit with a null `AttachmentData`, map hit with real
  bytes asserting payload identity and the MIME type for a known extension, and an unrecognised extension
  yielding `"application/octet-stream"`. Built from `Mock<IAttachment>` objects and plain strings; no
  `CoreWebView2*` type, no `MailItemHelper`, no `ItemViewer`.

### Edge cases and negative scenarios

Covered by the list above: malformed and relative URIs, an empty URI segment, a missing map entry, a null
attachment payload, an unrecognised file extension, a null `ItemHelper`, a null `_kbdHandler`, a null
`Buttons`, an `_itemViewer` that is not a concrete `ItemViewer`, a pre-cancelled token, a faulting filer
factory, and a faulting queue enqueue.

### Error handling and logging verification

The `MoveMailAsync` tests assert that exactly one `MoveFailureNotifier` invocation occurs and that the
thrown exception carries the original as `InnerException`. `TryResolveCidResource`'s debug logging is
exercised by the malformed-URI and null-payload cases; the assertion is on the returned `false` and the
null `out` values, since the logger is a static field and is not seam-injected.

### Coverage impact and targets

- Repository-wide line coverage must remain `>= 80%`; new members must reach `>= 90%`; no reduction in
  coverage for changed lines.
- Every added production member is fully coverable **except** the three carve-outs below, which are the
  complete set and are restated in the acceptance criterion under "File-size, toolchain, and coverage":
  (a) the two capture-field assignments and the two-statement lambda adapter added inside the
  pre-existing `[ExcludeFromCodeCoverage]` `InitializeWebViewAsync`, which retains that attribute
  (`ViewerSetup.cs:41`) and its documented residual barrier (the `.CoreWebView2` property is null without
  a live WebView2 runtime); (b) `DetachWebResourceRequestedHandler`, whose guarded `-=` statement is
  unreachable for that same reason, so the member is partially rather than fully covered; and (c) the
  default `MoveFailureNotifier` delegate `text => MessageBox.Show(text)`, whose body the headless
  unit-test policy forbids executing because invoking it opens a modal dialog.
- **No new `[ExcludeFromCodeCoverage]` attribute is introduced anywhere by this feature.**
- `coverage.config` at repository root excludes only third-party modules; `QuickFiler` is not
  assembly-excluded, so exemption remains per-member.

### Toolchain commands to run (format → lint → type-check → test)

Bootstrap, required once in this worktree because `packages/` and `QuickFiler.Test/bin/Debug/` are absent:

1. `nuget restore TaskMaster.sln` — mandatory. The `.csproj` files import `..\packages\...\*.props`
   conditionally (`QuickFiler.Test.csproj:3-8`); without restore the analyzer, MSTest adapter, and
   AltCover props silently do not import and the build produces a weaker diagnostic set.
2. `dotnet tool restore` — mandatory before the first `dotnet tool run csharpier` invocation. If it
   fails with the `global.json` error message, run `scripts/vscode/Install-RepoDotNetSdk.ps1` from the
   repository root first.

Then the four-stage loop, restarting from stage 1 on any failure or auto-fix:

1. `dotnet tool run csharpier format .` (verify with `dotnet tool run csharpier check .`)
2. `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
3. `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`
4. `vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /EnableCodeCoverage /InIsolation`

Notes on stage 4: `/InIsolation` is load-bearing and matches CI (`_mstest-coverage.yml:83`). A run
started from the repository root must exclude `\.claude\worktrees\` from assembly discovery, or it will
pick up sibling agent worktrees. Control `/ResultsDirectory:` and the TRX log file name so that no host
account name or machine name reaches a committed evidence artifact.

### Baseline requirement

Because this feature changes behaviour, the pre-change pass/fail counts for `QuickFiler.Test` must be
recorded before any production edit, so that the three assertions this feature tightens or must
accommodate are attributable:

- `QfcItemController_FocusAndThemeTests.ToggleNavigation_Synchronous_TogglesPositionTips` (tightened)
- `QfcItemController_ViewerSetupTests.Cleanup_NullsTrackedPrivateFields` (must survive the unwire path)
- `QfcItemControllerBreadcrumbDropDownTests.Cleanup_ResetsInjectedHostForPooledViewerReuse` (same)

All evidence artifacts (baselines, QA gates, coverage, regression results) are written to
`docs/features/active/qfc-item-controller-defects-484/evidence/<kind>/`.

### Manual validation steps

None required. Every defect except the WebView2 subscription detachment is covered by an automated
regression test. The one exception is verified by source inspection and is recorded as such.

---

## Acceptance Criteria

### Issue #480 — `ToggleNavigation(bool)` double toggle

- [x] The unconditional `_itemPositionTips.Toggle(false)` dispatch at
      `QuickFiler/Controllers/QfcItemController.FocusAndTheme.cs:170` is deleted, leaving exactly one
      `Toggle(false)` dispatch per branch of `ToggleNavigation(bool async)`.
- [x] `ToggleNavigation(async: false)` invokes `IQfcTipsDetails.Toggle(false)` exactly once, asserted
      with Moq `Times.Once()` (not `Times.AtLeastOnce()`).
- [x] `ToggleNavigation(async: true)` invokes `IQfcTipsDetails.Toggle(false)` exactly once, asserted
      with Moq `Times.Once()`, in a test that did not exist before this feature.
- [x] The existing assertion at `QuickFiler.Test/Controllers/QfcItemController.FocusAndThemeTests.cs:323`
      is tightened in place from `Times.AtLeastOnce()` to `Times.Once()`, and evidence records that the
      tightened assertion failed against the unfixed code.
- [x] `ToggleNavigation(bool async)` is still declared and implemented; it is not removed, and
      `QuickFiler/Interfaces/IQfcItemController.cs` is unmodified.

### Issue #481 — event unwiring path

- [ ] `internal void UnwireEvents()`, `internal void UnwireControlTreeEvents()`, and
      `internal void UnwireIntentEvents()` exist in
      `QuickFiler/Controllers/QfcItemController.EventWiring.cs`, with `UnwireEvents()` calling
      `UnwireControlTreeEvents()` then `UnwireIntentEvents()`, mirroring `WireEvents()`, and
      `UnwireEvents()` additionally calls `DetachWebResourceRequestedHandler()` as its third statement.
- [ ] All 16 intent subscriptions made by `WireIntentEvents()` are detached by `UnwireIntentEvents()`,
      verified by `Mock<IItemViewer>.VerifyRemove` assertions.
- [ ] All 6 control-tree subscriptions made by `WireControlTreeEvents()` are detached by
      `UnwireControlTreeEvents()`, verified by a wire-unwire-raise test asserting `Times.Never()` on the
      keyboard-handler mock and an unchanged `BackColor`, mirroring the fixture at
      `QfcItemController.EventWiringTests.cs:229-309`.
- [ ] `UnwireControlTreeEvents()` passes the same `ForAllControls` exclusion list as
      `WireControlTreeEvents()` (`EventWiring.cs:50`).
- [ ] `Cleanup()` calls `UnwireEvents()` before `_itemViewer = null` (`ViewerSetup.cs:407`) and before
      `_kbdHandler = null` (`:420`).
- [ ] `UnwireIntentEvents()` returns early when `_itemViewer` is null and guards the `FolderKeyDown`
      detach against a null `_kbdHandler`; `UnwireControlTreeEvents()` returns early when `_itemViewer`
      is not a concrete `ItemViewer`, skips the `ForAllControls` keyboard-detach walk when `_kbdHandler`
      is null, and guards the `Buttons` and `MenuItems` loops against null.
- [ ] A regression test asserts that `Cleanup()` does not throw on a controller whose `_kbdHandler` and
      `Buttons` are null and whose `_itemViewer` is a plain `Mock<IItemViewer>`.
- [ ] The two pre-existing `Cleanup()` tests
      (`QfcItemController.ViewerSetupTests.cs:347-376` `Cleanup_NullsTrackedPrivateFields` and
      `QfcItemControllerBreadcrumbDropDownTests.cs:125-153`
      `Cleanup_ResetsInjectedHostForPooledViewerReuse`) both pass unchanged after the fix.
- [ ] The `WebResourceRequested` delegate and its `CoreWebView2` source are captured into the private
      fields `_webResourceRequestedHandler` and `_coreWebView2` in `ViewerSetup.cs`, and the subscription
      is detached during teardown. Its verification is recorded as inspection-only, with the reason
      (`InitializeWebViewAsync` is `[ExcludeFromCodeCoverage]` at `ViewerSetup.cs:41` and requires a live
      WebView2 runtime), and no new `[ExcludeFromCodeCoverage]` attribute is added.

### Issue #483 — `MoveMailAsync` error handling and cancellation

- [ ] The `catch` block in `MoveMailAsync` (`QfcItemController.MailActions.cs:115-122`) logs at error
      level, invokes the failure notification, and then propagates — it does not return normally.
- [ ] A regression test drives a faulting `_emailFilerFactory` and asserts that `MoveMailAsync` faults
      with an exception whose `InnerException` is the original fault.
- [ ] `internal Action<string> MoveFailureNotifier { get; set; }` exists in `MailActions.cs` with the
      default `text => MessageBox.Show(text)`, and a regression test asserts it is invoked exactly once
      on the failure path with no modal dialog reached.
- [ ] The failure notification is marshalled through `_uiDispatcher` when the dispatcher is non-null, and
      is invoked directly when it is null (so existing tests that leave `_uiDispatcher` unset still pass).
- [ ] `Token.ThrowIfCancellationRequested()` is the first statement of `MoveMailAsync` (outside the
      `try`), of `FlagAsTaskAsync`, and of `EnumerateConversationAsync`.
- [ ] For each of the three methods, a regression test with a pre-cancelled `Token` asserts
      `OperationCanceledException` and asserts that the downstream collaborator (for `MoveMailAsync`, the
      `_emailFilerFactory`) was never invoked.
- [ ] `Task MoveMailAsync()`'s return type is unchanged and
      `QuickFiler/Controllers/QfcCollectionController.cs` is not modified.

### Issue #484 — `Cleanup()` timer disposal and stale `_mailActions`

- [ ] `Cleanup()` disposes `_emailIsReadTimer` before nulling it
      (`_emailIsReadTimer?.Dispose(); _emailIsReadTimer = null;` at `ViewerSetup.cs:424`).
- [ ] Test T1 injects a `Timer` armed with `Timeout.Infinite`, calls `Cleanup()`, and asserts that the
      field is null and that `timer.Change(0, Timeout.Infinite)` throws `ObjectDisposedException`. The
      test contains no `Thread.Sleep`, no `Task.Delay`, and no wall-clock wait.
- [ ] `ApplyReadEmailFormat(object state)` returns early when `ItemHelper`, `_themes`, `_activeTheme`, or
      `_mailActions` is null, and its signature is unchanged.
- [ ] Test T2 calls `ApplyReadEmailFormat(null)` on a `Cleanup()`ed controller and asserts it does not
      throw and that `IMailItemActions.Save()` is never called.
- [ ] `Cleanup()` nulls `_mailActions`, and a regression test asserts that a `SaveParameters` call after
      `Cleanup()` rebinds `_mailActions` to the new `MailItem`.
- [ ] `QuickFiler/Controllers/QfcItemController.Navigation.cs` is not modified.

### Issue #485 — WebView2 handler unguarded inputs

- [x] `internal static bool TryResolveCidResource(string requestedUri, IReadOnlyDictionary<string, IAttachment> contentIdMap, out byte[] payload, out string mimeType)`
      exists in `ViewerSetup.cs` and carries the URI, map, match, and `AttachmentData` guards.
- [x] `TryResolveCidResource` uses `Uri.TryCreate(..., UriKind.Absolute, ...)` and returns `false` with
      null `out` values for a malformed URI, for a relative URI, and for an absolute URI whose final
      segment is empty. Each case has its own regression test.
- [x] `TryResolveCidResource` returns `false` with null `out` values for a map miss, for a null map, and
      for a map hit whose `AttachmentData` is null. Each case has its own regression test.
- [x] `TryResolveCidResource` returns `true` with the exact payload reference and the expected MIME type
      for a map hit with real bytes and a known file extension, and returns
      `"application/octet-stream"` for an unrecognised extension. Both cases have regression tests.
- [x] The `WebResourceRequested` lambda is reduced to an adapter that builds the map from
      `ItemHelper?.AttachmentsInfo` (null-safe) and constructs the response only when
      `TryResolveCidResource` returns `true`.
- [x] Every #485 regression test runs without constructing a controller, an `ItemViewer`, a
      `MailItemHelper`, or any `CoreWebView2*` type.

### Upstream contract and scope discipline

- [ ] No public member is added, and no member is removed, from any of the four owned partials.
- [ ] `QuickFiler/Interfaces/IQfcItemController.cs` and `QuickFiler/Viewers/IItemViewer.cs` are byte-identical
      to their pre-change state.
- [ ] The set of files changed by this feature is a subset of the four owned production files plus the
      five owned test files. In particular `QfcItemController.Navigation.cs`, `QuickFiler/Viewers/ItemViewer*.cs`,
      `QuickFiler/Controllers/KbdActions.cs`, `QuickFiler/QuickFiler.csproj`, and
      `QuickFiler.Test/QuickFiler.Test.csproj` are unmodified.
- [ ] All three `Cleanup()` statement-order constraints hold in the delivered source: unwire before
      `_itemViewer` and `_kbdHandler` are nulled; timer disposal before `_emailIsReadTimer` is nulled;
      the existing `BreadcrumbUnhandledArrow` detach before `_breadcrumbViewer` is nulled.
- [ ] The post-`Cleanup()` lifecycle invariant is demonstrated: a pooled viewer carries zero
      subscriptions from the released controller, with the single documented `WebResourceRequested`
      exception.

### File-size, toolchain, and coverage

- [ ] Every production and test file touched by this feature is at most 500 lines after the change.
      All nine owned files are recorded with their post-change line counts. Specifically, the seven
      files that receive added lines under the plan's constraint C2 assignment —
      `QfcItemController.ViewerSetup.cs`, `QfcItemController.EventWiring.cs`,
      `QfcItemController.MailActions.cs`, `QfcItemController.FocusAndTheme.cs`,
      `QfcItemController.EventWiringTests.cs`, `QfcItemController.MailActionsTests.cs`, and
      `QfcItemController.TestSupport.cs` — are each verified at or under 500 lines, and the two owned
      test files that receive no added lines, `QfcItemController.FocusAndThemeTests.cs` and
      `QfcItemController.ViewerSetupTests.cs`, are verified at their unchanged 497 and 474 lines.
- [ ] Every new test uses MSTest, Moq, and FluentAssertions, and no new test contains `Thread.Sleep`,
      `Task.Delay`, a wall-clock wait, or a temporary file.
- [ ] Exactly one new test constructs a real `QuickFiler.ItemViewer` (the #481 control-tree unwire test),
      it starts no message pump and calls no `Show()`, and it saves and restores the
      `SynchronizationContext` in `try`/`finally`.
- [ ] `dotnet tool run csharpier check .` reports no formatting differences.
- [ ] `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
      completes with zero errors.
- [ ] `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`
      completes with zero errors.
- [ ] `vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /EnableCodeCoverage /InIsolation`
      reports zero failures, and the pass count is greater than or equal to the recorded Phase 0 baseline
      pass count plus the number of tests added.
- [ ] All four toolchain stages pass in a single consecutive pass with no intervening file modification.
- [ ] Repository-wide line coverage is `>= 80%`, and coverage for the changed lines is not reduced
      relative to the Phase 0 baseline.
- [ ] Each new production member added by this feature reaches `>= 90%` line coverage, except the
      two capture-field assignments and the two-statement lambda adapter added inside the pre-existing
      `[ExcludeFromCodeCoverage]` `InitializeWebViewAsync`; except `DetachWebResourceRequestedHandler`,
      whose guarded `-=` statement is unreachable without a live WebView2 runtime per research
      section 2.4, so its measured per-method line rate is non-zero but below that figure for that
      reason alone, as verified by the fail-before exception dossier; and except the default
      `MoveFailureNotifier` delegate
      `text => MessageBox.Show(text)`, whose body cannot be executed under the headless unit-test
      policy because invoking it opens a modal dialog, and which every `MoveMailAsync` failure-path
      test replaces with an injected notifier, so its measured line rate is zero. That statement is
      the relocation of the pre-existing uncovered `MessageBox.Show` call at
      `QfcItemController.MailActions.cs:119-121`, so no changed line loses coverage relative to the
      Phase 0 baseline.
- [ ] No new `[ExcludeFromCodeCoverage]` attribute is introduced anywhere by this feature.
- [ ] For each of the five issues, evidence records the regression test failing against the unfixed code
      before the corresponding production change, per the CLAUDE.md Bugfix Workflow.

---

## Risks & Mitigations

### R1 — `ViewerSetup.cs` file-size ceiling (highest-likelihood scope risk)

`QuickFiler/Controllers/QfcItemController.ViewerSetup.cs` is **430 of 500 lines (70 spare)**. This
feature adds the `TryResolveCidResource` extraction (roughly 28 lines including its doc comment), two
capture fields (roughly 4 lines), the `DetachWebResourceRequestedHandler` method (roughly 12 lines), and
the `Cleanup()` changes (roughly 6 lines), net of roughly 11 lines removed by reducing the
`WebResourceRequested` lambda to an adapter — about 40 lines net, leaving about 30 spare.
`QuickFiler/Controllers/QfcItemController.EventWiring.cs` is **391 of 500 (109 spare)**
and must absorb three unwire methods with their guards.

**Impact if breached.** Adding a new partial file (for example `QfcItemController.Teardown.cs`) would
require editing the shared `QuickFiler/QuickFiler.csproj`, which risks a merge conflict with sibling epic
children on the same integration branch.

**Mitigation.** The change must stay within the four existing owned files. Keep the extracted method and
the unwire methods tight; do not add explanatory prose beyond the required "why" comments; do not
opportunistically refactor. Verify the line count of both files as an explicit acceptance criterion after
every production edit, not only at the end.

### R2 — Test-file size ceiling

`QfcItemController.FocusAndThemeTests.cs` is at **497 of 500 lines (3 spare)** and
`QfcItemController.ViewerSetupTests.cs` at **474 (26 spare)**.

**Mitigation.** New tests are routed per the plan's constraint C2 capacity table, which supersedes the
illustrative routing recorded here; the binding requirement is only that no owned test file exceeds 500
lines. Each relocated test group carries a header comment naming its issue. The #480 assertion
tightening is a zero-line in-place edit. Do not delete the `EnableHandlelessThemeInvoke` helper
(`FocusAndThemeTests.cs:136-158` and following) to make room — it is load-bearing for four other tests.
Do not create a `.Part2.cs` test file; that would require a `.csproj` edit.

### R3 — `MoveMailAsync` now rethrows (behavioural risk)

Converting a swallowed failure into a propagating one changes what every caller observes.

**Assessment.** The blast radius is verified and small. The sole production caller,
`QfcCollectionController.TryMoveEmailByGroupAsync` (`QfcCollectionController.cs:2236-2258`), already
wraps the call in `try`/`catch (System.Exception)`, logs with subject context, and returns, so the bulk
loop `MoveEmailsAsync` (`:2206-2228`) cannot be aborted. The three live test callers
(`SeamFactoryTests.cs:162, 185, 228`) exercise the `ItemHelper is null`, OneDrive-missing, and happy
paths respectively, and none drives the catch block. The only other reference is a commented-out line at
`QfcCollectionController.cs:2227` and a `NotImplementedException` stub at `QfcThemeHelperTests.cs:442`.

**Mitigation.** Do not modify `QfcCollectionController.cs`. Record the rethrow explicitly in the upstream
contract (done above) so feature 464 does not copy the swallow-and-continue shape. Keep
`Token.ThrowIfCancellationRequested()` outside the `try` so `OperationCanceledException` is not rewrapped
as `InvalidOperationException`.

### R4 — The unwire path breaks the two existing `Cleanup()` tests

Both call `Cleanup()` with a null `_kbdHandler` and a `Mock<IItemViewer>` that cannot be cast to the
concrete `ItemViewer`. An unguarded unwire path throws.

**Mitigation.** The defensive null and type guards specified under "Boundaries and invariants to
preserve" are mandatory, not optional, and are encoded as acceptance criteria. Write the teardown
robustness test before the unwire methods so the guard requirement is demonstrated, not assumed.

### R5 — Delegate identity fails silently if `_kbdHandler` is reassigned

A `-=` against a differently-targeted delegate is a no-op with no diagnostic.

**Mitigation.** Order `UnwireEvents()` before `_kbdHandler = null` (encoded as an acceptance criterion),
and verify detachment behaviourally (raise the event after unwiring and assert `Times.Never()`) rather
than by inspecting subscription lists.

### R6 — The WebView2 subscription detachment cannot carry a regression test

It sits inside an `[ExcludeFromCodeCoverage]` member requiring a live WebView2 runtime.

**Mitigation.** Implement it anyway (two fields, three lines, no test cost) and record the
inspection-only verification explicitly in this spec and in the delivered evidence. Do not add a new
coverage exemption. Feature 489 may later replace the capture fields with a mockable
`IItemViewer` intent member (downstream note 5).

### R7 — Sibling epic children editing the same integration branch

**Mitigation.** No `.csproj` edit, no new file, and no write to a forbidden file. The changed-file set is
encoded as an acceptance criterion so a reviewer can check it mechanically.

### R8 — Worktree bootstrap failure

`packages/`, `QuickFiler.Test/bin/Debug/*.dll`, and `.dotnet-sdk/` are all absent from this worktree, and
`global.json` pins SDK `8.0.205` with `paths: [".dotnet-sdk", "$host$"]`.

**Mitigation.** Run `nuget restore TaskMaster.sln` and `dotnet tool restore` unconditionally before the
first toolchain stage; `dotnet tool restore` is idempotent and cheap. If it fails with the `global.json`
error message, run `scripts/vscode/Install-RepoDotNetSdk.ps1` from the repository root first. Use
`/t:Rebuild` (not `/t:Build`) for both msbuild stages, per CLAUDE.md, because MSBuild's up-to-date check
does not invalidate on a command-line `/p:` change.

---

## Rollout & Follow-up

### Release/rollout steps

1. Complete the Phase 0 baseline (restore, build, test, coverage) and commit the evidence to
   `docs/features/active/qfc-item-controller-defects-484/evidence/`.
2. Deliver each of the five defects regression-test-first, in any order; they are independent except that
   #481's `UnwireEvents()` call and #484's timer disposal both edit `Cleanup()` and must be sequenced to
   avoid a conflicting edit within the same statement block.
3. Run the four-stage toolchain to a clean consecutive pass.
4. Check off every acceptance criterion above against evidence.
5. Open the pull request against the integration branch
   `epic/quickfiler-bug-family-integration`. The pull request closes #480, #481, #483, #484, and #485.
6. Notify the owners of features 464 and 489 that the upstream contract in this spec is now landed on the
   integration branch.

### Post-fix monitoring or clean-up tasks

- Promote the report-only `Cleanup()` field asymmetries (downstream note 6) as a new potential document.
- Confirm that feature 464 has picked up downstream notes 1 and 2 (the identical `EfcItemController`
  timer defect at `EfcItemController.cs:277` and the partial unwire at `:257-262`).
- Confirm that feature 444 has recorded downstream note 4 (the timer-seam opportunity at
  `Navigation.cs:223-224`).
- Confirm that feature 489 has recorded downstream note 5 (the viewer-side WebView2 detachment that
  would retire this feature's two capture fields).

### Links

- Issue: https://github.com/drmoisan/TaskMaster/issues/484 (primary); closes #480, #481, #483, #484, #485
- Feature folder: `docs/features/active/qfc-item-controller-defects-484/`
- Scope and file ownership: `docs/features/active/qfc-item-controller-defects-484/issue.md`
- Research: `docs/features/active/qfc-item-controller-defects-484/research/research.2026-08-24T09-45.md`
- Promoted potentials:
  - `docs/features/potential/promoted/2026-08-07-qfc-item-controller-togglenavigation-double-toggle.md` (#480)
  - `docs/features/potential/promoted/2026-08-07-qfc-item-controller-no-event-unwiring-path.md` (#481)
  - `docs/features/potential/promoted/2026-08-07-qfc-item-controller-mailactions-error-handling-defects.md` (#483)
  - `docs/features/potential/promoted/2026-08-07-qfc-item-controller-cleanup-timer-and-stale-field-defects.md` (#484)
  - `docs/features/potential/promoted/2026-08-07-qfc-item-controller-webview-handler-unguarded-inputs.md` (#485)
- Related, not closed here: #426 (`EmailMoveMonitor` subscription retention, same defect class),
  #458 (`WebView2BreadcrumbHost` handler retention), #453 (epic #136 child F10, which deferred all five).
