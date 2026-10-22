# Research — quickfiler-teardown-review-residuals (Issue #823)

- **Timestamp:** 2026-09-08T23-50
- **Work Mode:** full-bug
- **Worktree:** `<repo-root>/.claude/worktrees/agent-ad314e82c3b26da90`
- **Method:** Read / Grep / Glob only. No Bash, no `pwsh`, no build, no test run was performed in this
  session, so no toolchain evidence is produced here. Every claim below is a static reading of the
  current tree unless marked INFERRED.

## 0. Scope confirmation

The epic `docs/features/epics/review-residuals-2026-09-08/epic.md` constrains this feature:

- `:110-114` — R6 is out of scope (`.claude/` is pushed down from `drm-copilot` with no templating).
- `:115-116` — R5 is a flake watch; the feature records observations and does not stabilize the test.
- `:117-118` — R2 is a decision, not a code change.
- `:100` — the declared primary surface is `UtilitiesCS/OutlookObjects/Store/StoreWrapperController*.cs`
  and `QuickFiler/Viewers/Breadcrumb*`.

Code-bearing entries are therefore **R1, R3, R4** only.

### R6 push-down claim — confirmed cheaply, no fix researched

Confirmed against the repository's own record rather than re-derived: the epic states the claim at
`epic.md:110-113`, and the promoted record states it at
`docs/features/potential/promoted/2026-09-08-quickfiler-teardown-review-residuals.md:56-60`. No
`.claude/` file was read for a remedy and none is proposed. Recorded as the stated rationale.

---

## 1. Q1 — R1: rescope the SMTP retry budget per store

### 1.1 Current state (verified)

| Fact | Location |
|---|---|
| `private bool _userEmailRetryAttempted;` | `UtilitiesCS/OutlookObjects/Store/StoreWrapperController.cs:105` |
| XML doc stating "one attempt per controller instance (issue #812)" and "never reset" | `StoreWrapperController.cs:96-104` |
| Latch read `&& !_userEmailRetryAttempted` | `StoreWrapperController.Display.cs:51` |
| Latch set `_userEmailRetryAttempted = true;` | `StoreWrapperController.Display.cs:54` |
| Guarded by `Current is not null && Current.UserEmailAddress is null` | `StoreWrapperController.Display.cs:49-50` |
| Retried call `Current.RefreshUserEmailAddress();` | `StoreWrapperController.Display.cs:55` |

Repo-wide grep for `_userEmailRetryAttempted` returns exactly three hits: the declaration and the two
consumption sites above. There is no other reader and no reset anywhere.

**The multi-store path is real.** `StoreWrapperController.DisplayName_SelectedValueChanged`
(`StoreWrapperController.cs:163`) assigns `Current = Model.Stores!.Find(store => store.DisplayName == displayName);`
at `:179` and then calls `PopulateWithCurrent()` at `:180`. One controller therefore observes many
distinct `StoreWrapper` instances, and the single `bool` is consumed by whichever store happens to be
selected first with a null address. R1's premise is correct.

The sole production construction site is `TaskMaster/Ribbon/RibbonController.cs:261`
(`var wrapper = new StoreWrapperController(Globals);`), which is what makes the current bound equal
one attempt per dialog open. Verified by repo-wide grep for `new StoreWrapperController`; every other
hit is in `UtilitiesCS.Test`.

### 1.2 Identity available on `StoreWrapper`

| Member | Declaration | Type | Nullability / reliability |
|---|---|---|---|
| `StoreId` | `UtilitiesCS/OutlookObjects/Store/StoreWrapper.cs:162` | `public string? StoreId { get; set; }` (`[JsonProperty]`) | Nullable. Assigned in `Init()` at `:48` inside a `try`/`catch (System.Exception)` at `:46-55`; an unreadable `StoreID` leaves the default (`null`) and only logs. A legacy payload without the key deserializes to the default (`:156-159`). |
| `DisplayName` | `StoreWrapper.cs:153` | `public string? DisplayName { get; set; }` | Nullable. Read from COM at `Init()` `:38` with no guard. Not unique by contract; it is the value the dialog's list search matches on (`StoreWrapperController.cs:179`), so within one populated model it is de facto distinct, but nothing enforces that. |
| the `StoreWrapper` object itself | `StoreWrapper.cs:16` (`public class StoreWrapper`) | reference | Never null on the latch path — `Current is not null` is already asserted at `Display.cs:49` before the latch is consulted. The class declares no `Equals`/`GetHashCode` override anywhere in the file (whole file read, 306 lines), so it uses reference equality. |

The repository already treats `StoreId` as *unreliable* and codes fail-safe branches for it:
`StoreWrapperController.ExcludeStoreSelectionChanged` (`StoreWrapperController.cs:260-266`) returns
`false` on `string.IsNullOrWhiteSpace(storeId)`; `ApplyExcludeStoreSelection` (`:310-317`) returns;
`BindExcludeStoreCheckbox` (`Display.cs:115-121`) disables and clears the checkbox. All three are
documented as "fail-safe per AC10" of issue #328.

### 1.3 Is the retried state per-store instance state? Yes

- `RefreshUserEmailAddress()` — `StoreWrapper.cs:192`, an **instance** method that assigns the
  instance property and returns it (`:203-204`). Its own doc at `:194-202` states that the member
  "guarantees nothing about how often the lookup runs: it re-runs the lookup on every call ... The
  bound lives in the caller."
- `UserEmailAddress` — `StoreWrapper.cs:174`, `[JsonIgnore] public string? UserEmailAddress { get; internal set; }`, instance state.
- `LastSmtpLookupError` — `StoreWrapper.cs:184`, `[JsonIgnore] internal string? LastSmtpLookupError { get; private set; }`, instance state, written at `:247`, `:265`, `:281`, `:285`.
- The underlying lookup `GetSmtpAddressFromStore()` (`StoreWrapper.cs:207`) reads only instance state
  (`RootFolder`, `DisplayName`).

Every quantity the retry touches is per-store instance state. A per-store latch is therefore coherent;
the current per-controller latch is the only thing that is not.

### 1.4 Existing tests that pin the current bound

All in `UtilitiesCS.Test/OutlookObjects/Store/StoreWrapperController_Tests.Display.cs` (373 lines).
No test anywhere references `_userEmailRetryAttempted` by name (repo-wide grep); the bound is pinned
behaviourally through Moq `VerifyGet` on the `PrimarySmtpAddress` getter.

| # | Test | Line | Changes meaning under a per-store latch? | Still passes unchanged? |
|---|---|---|---|---|
| 1 | `PopulateWithCurrent_CalledTwiceOnOneController_RetriesLookupOnlyOnce` | `:168` (doc `:161-166`) | **Yes.** Two calls, one controller, the *same* `StoreWrapper` instance. It currently demonstrates "once per controller"; under a per-store latch it demonstrates "once per store per controller". | **Yes.** Same key both passes, so `Times.Once()` at `:190` still holds. Its XML doc wording is what changes. |
| 2 | `PopulateWithCurrent_OnASecondControllerOverTheSameFailingStore_RetriesOnceMore` | `:200` (doc `:193-198`) | **Yes.** Its doc asserts in prose "The bound is per controller instance, **not per store**". | **Yes, provided the latch stays an instance field.** Two controllers, one shared store (`:208-215`); a per-controller collection gives each controller its own budget, so `Times.Exactly(2)` at `:222` still holds. **This test is the guard that forbids a `static` latch**: a process-wide per-store latch would produce 1 and fail here. Its doc must be reworded to "per controller instance *per store*". |
| 3 | `PopulateWithCurrent_WhenUserEmailIsAlreadyPopulated_DoesNotRetryLookup` | `:116` (Arrange comment `:118-121` names #812) | No behavioural change; the comment names the bound. | Yes. |
| 4 | `PopulateWithCurrent_WhenUserEmailIsAlreadyPopulated_NeverInvokesExchangeUserLookup` | `:231` (doc `:225-229`) | No. Pins that the latch does not replace the null check. | Yes (`Times.Never()` at `:250`). |
| 5 | `PopulateWithCurrent_WhenUserEmailIsNull_RetriesLookupAndRendersAddress` | `:94` | No. | Yes. |
| 6 | `PopulateWithCurrent_WhenRetryFails_RendersSpecificMessageWithReason` | `:140` | No. | Yes. |
| 7 | `PopulateWithCurrent_WithNullCurrent_RendersPlaceholdersAndDoesNotThrow` | `:333` | No, **but it constrains the implementation**: `Current` is null (`:338`), so any key derivation must sit *after* the existing `Current is not null` conjunct at `Display.cs:49` or this test fails with an NRE. | Yes, if the guard order is preserved. |

Also affected (prose only, no assertion): `StoreWrapperController_Tests.ExcludeStore.cs` calls
`PopulateWithCurrent()` at `:44`, `:57`, `:71`, `:145`, `:156` — none of them sets up an SMTP chain,
so none is sensitive to the latch shape.

### 1.5 Recommended minimal shape

**Replace the `bool` with a reference-identity set of stores already attempted, held as an instance
field on the controller.**

```
private readonly HashSet<StoreWrapper> _userEmailRetryAttemptedStores = new HashSet<StoreWrapper>();
```

and at `StoreWrapperController.Display.cs:48-56` change the third conjunct from
`!_userEmailRetryAttempted` to `!_userEmailRetryAttemptedStores.Contains(Current)` and the set-line
from the assignment to `_userEmailRetryAttemptedStores.Add(Current);`, leaving the first two conjuncts
and the statement order (latch **before** the call) untouched. `StoreWrapperController.cs` is 399
lines, so the declaration fits under the 500-line ceiling with wide margin; `Display.cs` is 184 lines.

`System.Collections.Generic` is already imported at `StoreWrapperController.cs:3`; `Display.cs`
imports only `System` and `System.Linq` (`:2-3`) but needs no new import because the field lives in
the other partial part.

**Invariant that must continue to hold (one sentence):** the number of SMTP lookup attempts a single
controller instance performs is bounded by the number of distinct stores it has been given and is
independent of how many times the user re-selects any store, so no sequence of user gestures can
produce an unbounded series of blocking UI-thread COM lookups — which is the property #812 exists to
establish (`StoreWrapperController.cs:100-104`).

Two secondary properties preserved by this shape:
- The latch is still set *before* `RefreshUserEmailAddress()` is called, so a throw out of the lookup
  still consumes that store's single attempt. This ordering was explicitly reviewed and endorsed at
  `docs/features/active/2026-09-07-utilitiescs-archive-root-read-and-user-email-retry-801-805-812/code-review.2026-09-08T17-00.md:131-135`.
- The set is never reset, matching `StoreWrapperController.cs:102-104`; a fresh dialog open still
  yields a fresh budget because `RibbonController.cs:261` constructs a fresh controller.

**Accepted cost, which must be stated in the spec:** the worst case rises from 1 blocking lookup per
dialog open to N, where N is the number of stores in `Model.Stores` whose address is null. That is
bounded by configuration, not by user gestures, which is the distinction #812 drew.

### 1.6 Alternatives considered and rejected

- **Key by `StoreId` string.** Rejected. `StoreId` is nullable and is documented and coded as
  possibly unreadable at four existing fail-safe sites (`StoreWrapper.cs:46-55`,
  `StoreWrapperController.cs:262`, `:312`, `Display.cs:116`). Every store with an unreadable
  `StoreId` would collapse onto one sentinel key, so those stores — precisely the ones whose COM
  reads are already failing — would go on sharing a single budget. That is R1's defect, reintroduced
  in the worst place.
- **Reset the `bool` in `DisplayName_SelectedValueChanged`.** Rejected. This is exactly the unbounded
  per-re-selection retry #812 removed: re-selecting one failing store repeatedly would run the
  blocking COM chain on every gesture. It is also the shape #812's own spec recorded as contradicted
  by evidence (spec `:278-282`).
- **Make the latch `static`.** Rejected. It would fail the existing test at `:200`/`:222`
  (`Times.Exactly(2)`) and would silently reduce the bound to once per process, which the field doc
  at `StoreWrapperController.cs:99-102` already warns against in its per-lifetime form.

### 1.7 Unreadable / null store identity — failure mode and safe default

With reference-identity keying **the failure mode does not arise**: the key is the `StoreWrapper`
object, and `Current is not null` is already established by the first conjunct at `Display.cs:49`
before the latch is consulted, so the key can never be null and can never be unreadable. No COM read
is required to compute it, which also means the keying itself cannot throw — relevant because every
other identity candidate on this type is populated from a COM read that is already known to fail.

If, contrary to this recommendation, a string key were adopted, the safe default for a null or
whitespace identity is **retry once and then latch under that store's object identity** rather than
"do not retry at all": suppressing the retry entirely would restore, for exactly the stores with the
least readable COM state, the #797 defect of a permanent placeholder in the address label
(`Display.cs:35-39`). It must not be "retry every time", which is the #812 defect.

### 1.8 Documentation sites that state the superseded bound

Changing the behaviour invalidates prose in five living places. All five are code/test comments; none
is a historical artifact.

1. `UtilitiesCS/OutlookObjects/Store/StoreWrapperController.cs:96-104` — the field XML doc.
2. `UtilitiesCS/OutlookObjects/Store/StoreWrapperController.Display.cs:40-47` — the retry-gate comment.
3. `UtilitiesCS/OutlookObjects/Store/StoreWrapper.cs:196-201` — the `RefreshUserEmailAddress` comment
   ("attempts this at most once per controller instance").
4. `UtilitiesCS.Test/.../StoreWrapperController_Tests.Display.cs:118-121` — Arrange comment.
5. `UtilitiesCS.Test/.../StoreWrapperController_Tests.Display.cs:193-198` — the "not per store" doc.

INFERRED recommendation, following #812's own precedent (its Non-Goals items 3 and 4, spec `:142-150`,
which amended the #797 *living* `spec.md` with dated corrections while leaving `plan`, `research`,
`code-review` and `feature-audit` untouched): amend
`docs/features/active/2026-09-07-...-801-805-812/spec.md` with a dated correction referencing #823,
and leave that folder's `plan`, `code-review.2026-09-08T17-00.md`, `feature-audit.2026-09-08T17-00.md`
and `evidence/**` unmodified.

---

## 2. Q2 — R2: the relocated throw. Decision input only

### 2.1 Which throw the reviewer meant

The finding is CR-3 of the #812 review, at
`docs/features/active/2026-09-07-utilitiescs-archive-root-read-and-user-email-retry-801-805-812/code-review.2026-09-08T17-00.md:226-249`.
Its exact sentence at `:240-241`: "**The net effect of this change on the QuickFiler path is that the
throw site moves from `:212` to `:233`, not that it disappears.**"

| | Before #818 | After #818 |
|---|---|---|
| File | `QuickFiler/Controllers/QfcItemController.FolderHandling.cs` | same file |
| Line | `:212` — `_itemViewer.AddFolderItems(_folderHandler.FolderArray);` | `:233` — `_globals is null ? null : (_globals.Ol?.ArchiveRootPath ?? string.Empty)` |
| Method | `AssignFolderComboBox` (declared `:191`) | `AssignFolderComboBox` (same method, same call frame) |
| Mechanism | the archive-root read reached through `FolderPredictor.ProjectSuggestionPath` off `FolderArray` | a direct, unguarded read of the same property at the consumer |
| Exception type | `InvalidOperationException` (unchanged) | `InvalidOperationException` (unchanged) |

The exception type is `InvalidOperationException`, thrown by
`TaskMaster/AppGlobals/ArchiveRootPathGuard.RequireResolvedArchiveRoot` at `:44` (`UnresolvableRule`)
and `:56` (`CrossStoreRule`), documented on the member at `:30-31`. The type is the same on both
sides of #818; only the throwing statement moved.

Note for precision: the read of `FolderArray` actually occurs first at `:200`
(`if (_folderHandler?.FolderArray?.Length > 0)`), so the review's "`:212`" names the second read in
the same method rather than the first. This does not change the conclusion — both are inside
`AssignFolderComboBox`, in the same call frame — but the planner should not quote "`:212`" as the
unique pre-change throw site.

### 2.2 Is the relocated throw handled?

**No.** Verified in the current tree:

- `AssignFolderComboBox` runs `:191-...` and contains no `try` (read of `:185-249`).
- It is reached from `:188` — `await _itemViewer.UiDispatcher.InvokeAsync(AssignFolderComboBox);` —
  inside `PopulateFolderComboBoxAsync`, so the exception surfaces on the UI dispatcher.
- Other in-repo callers: `:171` and `:175` (self-marshalling from `PopulateFolderComboBox`), `:196`
  (the method's own `InvokeRequired` re-entry), and `QfcCollectionController.cs:548`
  (`grp.ItemController.AssignFolderComboBox();`). None of these wraps the call in a `try`.
- It reaches a UI boundary unhandled. `QuickFiler/Interfaces/IQfcItemController.cs:18` declares it on
  the interface, so no handling is contributed by the abstraction either.

### 2.3 Was the change deliberate, and is this throw one of the five?

Deliberate: yes, and named twice.
- Spec Non-Goals item 1 (`spec.md:133-135`): the five functional `FolderPredictor` reads "all keep
  their current throwing behaviour. If they need attention they belong in a separate issue."
- Spec Non-Goals item 2 (`spec.md:136-141`) names `QfcItemController.FolderHandling.cs:233`
  explicitly, states the `?.` "guards a null `Ol`, not a throwing property", and instructs "File as a
  follow-up defect."
- Risk 3 (`spec.md:632-635`) repeats it and commits to the filing.

**Not one of the five.** The five functional reads the PR left unguarded are inside
`UtilitiesCS/OutlookObjects/Folder/FolderPredictor.cs` at `:305`, `:376`, `:687`, `:752` and `:909`
(enumerated in `code-review...:58-80`). The relocated throw is at
`QuickFiler/Controllers/QfcItemController.FolderHandling.cs:233` — a different file, a different
project, and a consumer rather than a provider. It is a sixth, distinct site.

### 2.4 Evidence for the decision

**The evidence supports "intermediate state", and the remaining work is already filed as issue #813 —
so this issue records the decision and defers, with no scope widening.**

- The relocation is not the intended end state: `spec.md:139-141` states the consequence the reader
  must accept — "after this change AC1's outcome is verifiable at the `FolderPredictor` unit level,
  **not** end to end in QuickFiler, until that site is also guarded."
- The follow-up **was** filed. `docs/features/potential/promoted/2026-09-08-assignfoldercombobox-unguarded-archiverootpath-read.md:1-11`
  records **issue #813**, `https://github.com/drmoisan/TaskMaster/issues/813`, severity **Medium**
  (`:56-64`), with the fix and test shape already specified (`:76-83`). This settles the
  code-review's "recorded here as **owed**" (`code-review...:245-249`), which was written before the
  filing could be confirmed from the working tree.
- #813 is unstarted: `docs/features/active/*813*` does not exist (glob returned no files).
- #813 is a sibling in this same epic: `epic.md:31-33` lists it as a wave-0 feature with primary
  surface `QuickFiler/Controllers/QfcItemController.FolderHandling.cs` (`epic.md:96`), and
  `epic.md:20-21` names its leading indicator. That file is on this feature's OFF LIMITS list.

**Recommended spec wording for R2:** *Decision — intermediate state. The #818 change relocated the
throw from the `FolderArray` consumption at `QfcItemController.FolderHandling.cs:212` to the direct
archive-root read at `:233` within the same unguarded method; the exception type
(`InvalidOperationException` from `ArchiveRootPathGuard`) and the unhandled UI-dispatcher boundary are
unchanged. This was disclosed at the time (#812 spec Non-Goals item 2 and Risk 3) and the remaining
work is tracked as issue #813, a sibling feature of the same epic that owns the file. No code change
is made under #823 and the scope is not widened.*

---

## 3. Q3 — R3: which side is wrong

### 3.1 Nullable annotation context

- `QuickFiler/Viewers/BreadcrumbPopupOwnerRegistry.cs:1` carries `#nullable enable`.
- **`QuickFiler/Viewers/QfcFormViewer.cs` does NOT carry `#nullable enable`.** Verified by a
  `^#nullable` grep across `QuickFiler/Viewers/`, which returned 23 files and did not include it.
- **`QuickFiler/Viewers/ItemViewer.Breadcrumb.cs` does NOT carry it either** (same grep).
- **`QuickFiler.Test/Viewers/BreadcrumbPopupOwnerRegistryTests.cs` does not carry it** (whole file
  read; line 1 is `using System;`).
- No `.csproj` in the repository declares a `<Nullable>` property (grep over all 16 `*.csproj`
  returned only the identical #181 comment line in each). Nullable is per-file opt-in, matching
  `CLAUDE.md` § C#1.3.

Consequence: `SetBreadcrumbPopupOwner`'s parameters at `QfcFormViewer.cs:227` are in an *oblivious*
context, so nothing it forwards at `:228` is nullable in its own flow analysis — the file has no flow
analysis at all — and oblivious-to-non-nullable argument passing produces no diagnostic today.

### 3.2 Every call site

`SetBreadcrumbPopupOwner`
- **Declaration:** `QuickFiler/Viewers/QfcFormViewer.cs:227`, body `:228`.
- **Production call site (exactly one, repo-wide):** `QuickFiler/Viewers/ItemViewer.Breadcrumb.cs:216`
  ```
  (FindForm() as QfcFormViewer)?.SetBreadcrumbPopupOwner(this, () => host.IsOpen);
  ```
  Neither argument can be null. `this` is the item viewer itself. `() => host.IsOpen` is a lambda
  literal, which is never a null delegate. The `host` local is assigned at `:197-207` before the call.
- **No test call site.** Grep for `SetBreadcrumbPopupOwner` returns only the two lines above.

`BreadcrumbPopupOwnerRegistry.Register`
- **Declaration:** `QuickFiler/Viewers/BreadcrumbPopupOwnerRegistry.cs:42`; guard `:44-47`; store `:49`.
- **Production call site (exactly one):** `QfcFormViewer.cs:228`, forwarding the above.
- **Test call sites:** `QuickFiler.Test/Viewers/BreadcrumbPopupOwnerRegistryTests.cs:56`, `:77`,
  `:100`, `:101`, `:128`, `:129` (all non-null), and `:161`, `:162` (the two literal nulls).

### 3.3 The documented rationale is false

`BreadcrumbPopupOwnerRegistry.cs:31-33` justifies null tolerance with "the registration hop runs from
a form lookup that can legitimately find no form". The form lookup is `FindForm() as QfcFormViewer` at
`ItemViewer.Breadcrumb.cs:216`, and it is consumed by the **null-conditional operator `?.`**. When the
lookup finds no form — or finds a form that is not a `QfcFormViewer` — the entire invocation is
skipped and `Register` is never entered. The lookup produces a null **receiver**, never a null
**argument**. The same false rationale is mirrored on the forwarder at `QfcFormViewer.cs:220-221`
("Ignored when null").

**Decision: the CONTRACT is wrong; the SIGNATURE is right.** The non-nullable parameters correctly
describe every reachable call. The doc and the test encode a condition that the only call site
provably cannot produce.

### 3.4 Consequence of each candidate fix

**(a) Annotate `Control?` / `Func<bool>?`, keep the silent-return guard.**
- The signature would then match the doc and the test, and nothing else changes.
- No caller changes: both callers are in oblivious files, so neither gains nor loses a diagnostic.
- Cost: it makes permanent a defensive branch that no production path can reach, and it keeps a
  written contract asserting a null-producing mechanism that does not exist. It also conflicts with
  the fail-fast requirement in `.claude/rules/general-code-change.md` ("Do not silently ignore
  errors") and `CLAUDE.md` § C#4.1.

**(b) Keep the non-nullable signature, remove the silent-return guard, change the test to assert
rejection.** This is only safe if "remove the guard" means "replace it with an explicit
`ArgumentNullException`", not "delete it":
- A **null control** with no guard reaches `_owners[itemViewer] = popupIsOpen;` at `:49`. The
  `Dictionary<Control, Func<bool>>` indexer setter throws `ArgumentNullException` on a null key, so
  the rejection is real but its message names the framework parameter, not `itemViewer`.
- A **null predicate** with no guard is *stored successfully*. The failure is then deferred to
  `AnyOpen` at `:59` (`_owners.Values.Any(popupIsOpen => popupIsOpen())`), where it becomes a
  `NullReferenceException` on a completely different member, on the issue-#677 deactivation path.
  That is strictly worse than today.
- Therefore the correct form of (b) is an explicit guard: `throw new ArgumentNullException(nameof(itemViewer))`
  / `nameof(popupIsOpen)` (net48 has no `ArgumentNullException.ThrowIfNull`), plus rewriting the XML
  doc at `:30-36` and `QfcFormViewer.cs:220-221` to state rejection, and changing
  `Register_NullControlOrNullPredicate_IsIgnored` (`:154`) to a rejection test.
- **At the real call sites, if a null ever arrived**, the throw would propagate out of
  `SetBreadcrumbPopupOwner` into `ItemViewer.Breadcrumb.cs:216`, inside the breadcrumb host
  configuration sequence, before `ConfigureBreadcrumbDropDown` at `:217`. Since neither argument can be
  null there, this is unreachable in production; it converts an unreachable silent branch into an
  unreachable loud branch, which is the direction the repository's error-handling policy prefers.

**Recommendation: (b) in its explicit-throw form.** It is the only option that removes the false
statement rather than ratifying it, and the test then pins the real contract, which is what the issue
asks for (`issue.md:95-96`: "R3 needs a test that pins whichever of the signature or the documented
contract is correct").

### 3.5 New CS86xx diagnostics from an annotation change

**None, under either option.** Enumeration of every `#nullable enable` file that could be affected:

- The registry is `internal sealed` (`BreadcrumbPopupOwnerRegistry.cs:21`) and is compiled into
  `QuickFiler` (`QuickFiler.csproj:417`). Internals are visible to `QuickFiler.Test`
  (`QuickFiler/Properties/AssemblyInfo.cs:5`) and to `DynamicProxyGenAssembly2`.
- The complete set of files that mention the type or the member is: `BreadcrumbPopupOwnerRegistry.cs`,
  `QfcFormViewer.cs`, `ItemViewer.Breadcrumb.cs`, `BreadcrumbPopupOwnerRegistryTests.cs` (repo-wide
  grep for `SetBreadcrumbPopupOwner|BreadcrumbPopupOwnerRegistry`).
- Of those four, exactly **one** carries `#nullable enable`: `BreadcrumbPopupOwnerRegistry.cs` itself.
  The other three are oblivious, so no CS86xx can be raised in them regardless of the annotation.
- Inside the annotated file, the existing `if (itemViewer == null || popupIsOpen == null) return;`
  at `:44-47` narrows both to non-null on the fall-through path (neither `Control` nor `Func<bool>`
  defines a user-defined `operator ==`, so these are ordinary null tests that flow analysis honours).
  `_owners[itemViewer] = popupIsOpen;` at `:49` therefore produces no CS8604/CS8601 after annotating
  the parameters nullable. Under option (b) the parameters stay non-nullable and nothing changes at
  all.

This is a static reading of the nullable rules, not a compiler run — no `msbuild` was executed in
this session. The planner should still treat step 3 of the toolchain
(`msbuild ... /p:TreatWarningsAsErrors=true`) as the confirming gate.

### 3.6 Must the test file gain `#nullable enable`?

**No, and it should not.**

- Nullable annotations are compile-time metadata and are erased at runtime. Whether `Register` ignores
  or throws on a null is observable from an oblivious test file exactly as well as from an annotated
  one, so the test is fully meaningful without the directive.
- If the directive were added while the parameters stayed non-nullable (option b), the two literal
  `null` arguments at `:161-162` would raise **CS8625** ("Cannot convert null literal to non-nullable
  reference type"). `.github/workflows/_build-nullable.yml`'s command, mirrored in `CLAUDE.md` § C#1.3,
  runs `/p:TreatWarningsAsErrors=true`, which promotes CS8625 in an opted-in file to a build **error** —
  the nullable gate would fail. Working around it would require `null!` at both sites or a
  `#pragma warning disable CS8625` block (precedent exists at
  `UtilitiesCS/OutlookObjects/Store/StoreWrapperController.cs:55-57`), which adds noise for no gain.

---

## 4. Q4 — R5: where the flake observation goes

### 4.1 The test

- **File:** `QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixtureTests.cs`
- **Class:** `QfcItemController_UiThreadDispatcherFixtureTests`, declared `:31`, namespace
  `QuickFiler.Controllers.Tests` (`:8`).
- **Method:** `Transaction_SecondCallerCannotInstallUntilTheFirstRestores`, `:197`; XML doc `:190-194`;
  `[TestMethod]` `:195`, `[Timeout(GateTimeoutMs)]` `:196` where `GateTimeoutMs = 60000` (`:33`).
- The test body (`:199-249`) uses `ManualResetEventSlim` and awaited `Task` completion only. The class
  doc at `:23-28` already states there is "no sleep, no delay, no wall-clock wait, and no temporary
  file", so the prohibition in `issue.md:55-56` is already satisfied by the current code; nothing needs
  removing.
- The class doc at `:13-22` already records that this test (labelled "R4" in issue-#493 terms) "fails
  only probabilistically" under a broken implementation, because "nothing can force the second caller
  to reach its acquisition point while the first still holds the gate and there is no deterministic
  way to prove the second caller is currently blocked without a timed wait, which the repository's
  determinism rules forbid". **This is the pre-existing, in-code acknowledgement that the test is
  probabilistic.** Any observation record should reference it rather than restate it.

No diagnosis is offered here; per `epic.md:115-116` this entry is a watch, not a fix.

### 4.2 Repository precedent for flake handling

Searched `docs/**` and all test assemblies for `flake|flaky|intermittent` (case-insensitive).

- **In test source: exactly one file.** `QuickFiler.Test/Controllers/QfcCollectionControllerDefects468MoveTests.cs:72-80`
  — an XML `<para>` on the test explaining that a deterministic pre-fix red state does not exist
  because "a `ConcurrentDictionary`'s enumeration order is unspecified rather than
  guaranteed-wrong, so a pre-fix run could happen to return the right order and the assertion would be
  flaky by construction", and pointing at the sibling structural assertion that carries the
  deterministic proof. This is precedent for **explaining a probabilistic property in a doc comment on
  the test**, but it is a one-time explanation, not an accumulating log.
- **In `docs/`, the established mechanism is a promoted potential record → GitHub issue.** Three
  precedents, all with the observation, the exact command, and the captured failure text in the record
  rather than in code:
  - `docs/features/potential/promoted/2026-09-04-tryaddvaluesasync-wall-clock-timeout-flaky.md`
    (issue #780) — carries the exact vstest command line (`:20`), reproduction steps (`:23-28`), the
    verbatim stack trace (`:38-42`), and a run summary (`:47`).
  - `docs/features/potential/promoted/2026-08-15-qfc-item-controller-init-tests-flaky-window-handle.md`.
  - `docs/features/potential/promoted/2026-08-08-winformspumphost-tests-load-flaky-visible-window.md`.
- A search for the literal phrase "flake watch" across `docs/` returns only issue #823's own three
  copies and `epic.md`. **There is no existing accumulating flake-observation register in this
  repository.** R5 would be the first.

### 4.3 Where the record should live — evaluation

| Candidate | Survives the merge? | Discoverable from the test? | Accumulates? | Assessment |
|---|---|---|---|---|
| Comment on the test method | Yes (source is merged) | Yes | Poorly — every new observation is a source edit to a test file, producing diff churn on a file no behaviour change touches, and the file would grow toward the 500-line ceiling with dated prose | Suitable for **one** durable pointer, not for the log |
| Issue #823 body | Yes, but #823 closes when this feature merges, after which new observations have no open home | No | Yes while open | Not durable past this feature |
| Evidence artifact under `<FEATURE>/evidence/other/` | Yes — feature-folder content is preserved on merge | No, unless the test points at it | Yes — append-only, one dated row per observation, no source churn | The correct home for the log |

**Recommendation (INFERRED, a judgement about documentation placement, not a verified fact):** use
both, with a single link between them.

1. **The log:** one append-only artifact at
   `docs/features/active/2026-09-08-quickfiler-teardown-review-residuals-823/evidence/other/flake-watch-uithread-dispatcher-transaction.2026-09-08T23-50.md`.
   `evidence/other/` is a canonical location under
   `.claude/skills/evidence-and-timestamp-conventions/SKILL.md:19` and `:53`, and the ISO-8601
   `yyyy-MM-ddTHH-mm` filename form is required by that skill at `:44-47`. Seed it with the single
   known observation (one failure, three passes, from the item-810 run) and a per-row schema of
   date, command, assembly set, parallelism setting, and the failure text, mirroring the level of
   detail issue #780's record carries at `:20` and `:38-42`.
2. **The pointer:** two or three lines added to the existing XML doc on the test method at
   `QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixtureTests.cs:190-194`, naming
   issue #823 and the artifact path, so a future engineer who sees the test fail finds the log from
   the failure. This is the only change that makes the record discoverable from the test, and it is
   the same technique the one in-repo precedent uses
   (`QfcCollectionControllerDefects468MoveTests.cs:72-80`).

This adds no sleep, no retry and no timing tolerance, and it does not alter a single executable
statement in the test.

**Caveat the planner must resolve:** an evidence artifact inside a *closed* feature folder is
append-only in practice only until that folder stops being touched. If the intent is a register that
outlives #823, the alternative is to promote R5 to its own potential record following the #780
precedent so it has an open issue to accumulate against. The epic explicitly declines to fix R5
(`epic.md:115-116`) but does not rule this out. This is a decision for the orchestrator, not a fact
this research can settle.

---

## 5. Q5 — Scope and file budget for R1, R3, R4

### 5.1 Files requiring change

**R1 (production):**
| File | Current lines | Change |
|---|---|---|
| `UtilitiesCS/OutlookObjects/Store/StoreWrapperController.cs` | 399 | replace the `bool` at `:105` with the per-store set; rewrite the XML doc at `:96-104` |
| `UtilitiesCS/OutlookObjects/Store/StoreWrapperController.Display.cs` | 184 | latch read `:51` and set `:54`; rewrite the comment at `:40-47` |
| `UtilitiesCS/OutlookObjects/Store/StoreWrapper.cs` | 306 | comment only, `:196-201` |

**R1 (test):**
| File | Current lines | Change |
|---|---|---|
| `UtilitiesCS.Test/OutlookObjects/Store/StoreWrapperController_Tests.Display.cs` | 373 | add the per-store regression test(s); reword the docs at `:118-121` and `:193-198`. Existing assertions at `:190` and `:222` are unchanged. |

**R3:**
| File | Current lines | Change |
|---|---|---|
| `QuickFiler/Viewers/BreadcrumbPopupOwnerRegistry.cs` | 61 | guard at `:44-47` and XML doc at `:30-36` |
| `QuickFiler/Viewers/QfcFormViewer.cs` | (not counted; not at risk) | XML doc `:220-221` only, to remove the mirrored false claim |
| `QuickFiler.Test/Viewers/BreadcrumbPopupOwnerRegistryTests.cs` | 175 | rewrite `Register_NullControlOrNullPredicate_IsIgnored` (`:147-173`) |

**R4:**
| File | Current lines | Change |
|---|---|---|
| `QuickFiler/Viewers/BreadcrumbDropDownHost.Open.cs` | 178 | one comment token at `:11`: `480` → `459` |

**R5 (if the Q4 recommendation is adopted):** the new evidence artifact plus the XML-doc pointer at
`QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixtureTests.cs:190-194`.

No file in this set is near the 500-line ceiling; the largest is `UtilitiesCS.Test/.../Display.cs` at
373. No partial-part split is required for any entry, which is a material difference from issue #810
(where two files sat at 496).

### 5.2 R4 — verified line counts

- `QuickFiler/Viewers/BreadcrumbDropDownHost.cs` — **459** lines (Grep `^` count: 459; Read of the
  tail confirms line 459 is the file's closing `}`).
- `QuickFiler/Viewers/BreadcrumbDropDownHost.Open.cs:11` currently reads
  `/// Held on a second partial-class part so <c>BreadcrumbDropDownHost.cs</c> (480 lines) stays`.
  The correct figure is **459**. (Ground truth supplied by the orchestrator; re-confirmed here.)

**Adjacent observation, outside R4's named scope — reported, not proposed.** Three sibling comments
in the same directory carry the same class of stale figure:
- `QuickFiler/Viewers/BreadcrumbBridgeCoordinator.Search.cs:11` says `BreadcrumbBridgeCoordinator.cs`
  is 487 lines; it is **437**.
- `QuickFiler/Viewers/BreadcrumbItemViewerLifecycleCoordinator.Search.cs:10` says 481 lines; the file
  is **497**.
- `QuickFiler/Viewers/BreadcrumbDropDownOpenLifetime.Focus.cs:8` says 477 lines (not measured).

R4 names only `BreadcrumbDropDownHost.Open.cs`, and the epic scopes this feature to that entry.
Correcting the other three would be unrequested scope; the `:10` figure in
`BreadcrumbItemViewerLifecycleCoordinator.Search.cs` is the one worth a follow-up record, because it
*understates* a file that is now 497 of 500.

### 5.3 `Compile Include` entries

**None are required.** Both target test classes already exist and are already listed:
- `UtilitiesCS.Test/UtilitiesCS.Test.csproj:542` — `<Compile Include="OutlookObjects\Store\StoreWrapperController_Tests.Display.cs" />`
  (the partial's base is at `:538`).
- `QuickFiler.Test/QuickFiler.Test.csproj:84` — `<Compile Include="Viewers\BreadcrumbPopupOwnerRegistryTests.cs" />`.

Both are also the natural homes for the new tests: `StoreWrapperController_Tests.Display.cs` is the
partial that already owns the AC6 retry region (`:21-253`) and reuses the base harness
`CreateControllerWithViewer()` from `StoreWrapperController_Tests.cs:163`;
`BreadcrumbPopupOwnerRegistryTests.cs` already owns every `Register` case.

Adding no new files also removes this feature from the epic's project-file contention discussion
(`epic.md:136-143`), which anticipated that several children would need to touch the same `.csproj`.

---

## 6. Test strategy (no test code written)

Consistent with `CLAUDE.md` § CUT1-CUT2 (MSTest, Moq, FluentAssertions) and the general unit-test
policy. Every test below is headless: no live Outlook, no window handle, no file.

**R1 — the new behaviour needs one test that fails before the change.**
1. *Per-store budget (the RED test).* One controller, two distinct `StoreWrapper` instances each with
   its own failing SMTP chain built by the existing helper
   `CreateDisplayFailingSmtpRootFolderWithUser` (`StoreWrapperController_Tests.Display.cs:72-91`);
   set `Current` to A, `PopulateWithCurrent()`, set `Current` to B, `PopulateWithCurrent()`; assert
   `Times.Once()` on **each** store's own `ExchangeUser.PrimarySmtpAddress` getter. Pre-change this
   fails on store B with `Times.Never()` observed, which is the deterministic fail-before proof.
2. *The #812 invariant still holds (regression).* Same controller, same store, re-selected three
   times: assert `Times.Once()`. This generalizes the existing `:168` test and must be stated as the
   bound that may not regress.
3. *Null `Current` is still safe.* The existing `:333` test covers it; confirm it remains green,
   since it is the constraint that forbids computing a key before the null check.

**R3 — one behaviour test replacing one.** Rewrite `Register_NullControlOrNullPredicate_IsIgnored`
(`:154`) to assert `ArgumentNullException` for each of the two arguments, using FluentAssertions
`.Should().Throw<ArgumentNullException>().WithParameterName(...)`, and add an assertion that
`AnyOpen` remains `false` after both rejected calls so the registry is provably unchanged. Do **not**
add `#nullable enable` to the test file (§ 3.6).

**R4 — no test.** A comment correction has no observable behaviour. Its verification is the
CSharpier + analyzer + nullable passes plus a reviewer reading the corrected figure against a line
count.

**R5 — no test change.** Observation record only.

**Coverage.** `BreadcrumbPopupOwnerRegistry` is a plain internal class with existing tests covering
all four of its reachable behaviours; the R3 change does not add an uncovered branch (it converts one
covered silent branch into one covered throwing branch). The R1 change adds one collection-membership
branch, covered by tests 1 and 2 above. Neither should move the repository coverage figure
measurably.

**Toolchain.** The full four-step pass from `CLAUDE.md` § "C# Toolchain" is required, in order, with
the nullable step being the one that would surface any CS86xx consequence predicted in § 3.5.

---

## 7. Open decisions for the orchestrator

1. **R2 wording** — adopt § 2.4 verbatim, or restate. No code change either way; the deferral target
   (#813) already exists and is a sibling in the same epic.
2. **R5 placement** — the `evidence/other/` artifact plus a test XML-doc pointer (§ 4.3), or promote
   R5 to its own potential record so the log outlives this feature.
3. **#812 artifact amendment** — whether to add a dated correction to that feature's living `spec.md`
   for the changed retry bound (§ 1.8), following #812's own treatment of the #797 spec. Its
   `plan`, `code-review`, `feature-audit` and `evidence/**` must not be touched in any case.
4. **Stale sibling line counts** (§ 5.2) — leave alone as out of scope, or file one follow-up record
   for `BreadcrumbItemViewerLifecycleCoordinator.Search.cs:10`.
