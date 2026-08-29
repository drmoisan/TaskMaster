# 2026-08-27-qfc-unregister-navigation-count-mismatch-orphan (Spec)

- **Issue:** #644
- **Parent (optional):** none
- **Owner:** drmoisan
- **Last Updated:** 2026-08-29T08-05
- **Status:** Ready for planning
- **Version:** 1.0

> **Work mode `full-bug`.** This document is the sole authoritative acceptance-criteria source for
> issue #644. No user-story.md exists for this feature and none is to be created; the check-off
> protocol in .claude/skills/acceptance-criteria-tracking/SKILL.md resolves `full-bug` to
> `spec.md` only.

## Context
`QfcCollectionController.UnregisterNavigation` bounds its unregister loop with the *current*
`_itemGroups.Count`, while `RemoveSpecificControlGroup(int)` mutates `_itemGroups` with no
unregister/register bracket around the mutation. When a group is removed through that unbracketed
path, the count the unregister loop later reads no longer matches the count in force when the
navigation keys were registered, so the loop stops short and leaves orphaned `KbdActions`
navigation registrations behind. Every production call site discards `KbdActions.Remove`'s `bool`
result, so the divergence is silent until a later `Add` or `Find` throws
(`ArgumentException` or `InvalidOperationException`). The unbracketed mutation is reachable from
`RemoveBelowThresholdAsync` via the `RemoveGroupByEntryId` seam, and from the synchronous `'R'` char
action registered in QuickFiler/Controllers/QfcItemController.EventWiring.cs. This is a distinct
defect from the register/unregister digit-width mismatch filed as #472: #472 concerns the *format*
of the keys removed, this concerns the *number* of them. Fixing it requires the key-ledger design —
recording the exact set of keys registered and replaying that set on unregistration — which changes
the outcome of the existing characterisation tests in
`QuickFiler.Test/Controllers/QfcCollectionControllerTests.cs`, a file at the 500-line ceiling whose
`[TestMethod]` count issue #468 froze. It was therefore deliberately kept out of #472's scope under
the `CLAUDE.md` Bugfix Workflow rule that a deeper design problem uncovered mid-fix opens a new
issue instead of widening scope.

Environment:
- OS/version: Windows 11, Outlook VSTO add-in host
- Python version: n/a (C# / .NET Framework)
- Command/flags used: `vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /InIsolation /TestCaseFilter:"TestCategory!=LiveOutlook"`
- Data source or fixture: `QfcCollectionController` with an `_itemGroups` collection crossing a group removal

Impact / Severity:
- [ ] Blocker
- [ ] High
- [x] Medium
- [ ] Low

Medium: the failure is silent at the point of divergence and surfaces later as a thrown exception in
a keyboard path. In the QuickFiler surface the resulting exception is caught and logged in
`KeyboardHandler`, so the user-visible symptom is a dead navigation key rather than a crash. It is
not a blocker because the unbracketed removal paths are not on the common navigation flow.


## Repro & Evidence
Steps to Reproduce:
1. Bring up the QuickFiler collection surface with enough item groups that navigation keys are
   registered for each group.
2. Remove a group through an unbracketed path — `RemoveBelowThresholdAsync` (which reaches
   `RemoveSpecificControlGroup(int)` through the `RemoveGroupByEntryId` seam), the synchronous `'R'`
   char action wired in QuickFiler/Controllers/QfcItemController.EventWiring.cs, or
   `PopOutControlGroup(int)`. No `UnregisterNavigation` / `RegisterNavigation` bracket surrounds
   this mutation.
3. Trigger `UnregisterNavigation`. Its loop bound is now the reduced `_itemGroups.Count`, so it
   iterates fewer times than the registration did.
4. Re-register navigation, or press a navigation key that resolves against the stale registry.

Expected:
Unregistration removes exactly the set of navigation keys that registration added, regardless of any
`_itemGroups` mutation that occurred in between. A subsequent registration succeeds, and a navigation
keypress resolves against exactly one handler.

Actual:
One or more navigation registrations are orphaned in the `KbdActions` registry. Because every call
site discards `Remove`'s `bool` return, nothing reports the failure at the point it happens. The
symptom surfaces later as an `ArgumentException` from a duplicate `Add`, or an
`InvalidOperationException` from a `Find` that resolves against a multi-element match set.

Logs / Screenshots:
- [ ] Attached minimal logs or screenshot
- Snippet: not captured. The defect was established by static reading of the control flow during
  #472's root-cause analysis, not by a captured runtime trace. The residual orphan it produces IS
  observable: #472's width-fidelity regression test in
  `QuickFiler.Test/Controllers/QfcCollectionControllerNavigationDigitsTests.cs` asserts the residual
  entry explicitly and attributes it, by XML documentation comment, to this follow-up issue rather
  than silently absorbing it.

Registry asymmetry that makes the orphan dangerous (verified by reading
QuickFiler/Controllers/KbdActions.cs and QuickFiler/Controllers/KaStringAsync.cs at
`ecdb1c84ba8541ab67042985919cfed4df768c01`): `Add` and `Remove` compare keys with
`EqualityComparer<TKey>.Default` (exact equality), whereas `Find`, `FindIndex`, `ContainsKey` and
the indexer compare with the element-defined `KeyEquals`, which for `KaStringAsync` is a substring
test (`Key.Contains(other)`). An orphaned `"10"` therefore collides with a probe of `"1"` under
`Find` — producing `InvalidOperationException` — even though `Remove("Collection", "1")` would never
have removed it.


## Scope & Non-Goals

**In scope**

- Make `UnregisterNavigation` remove exactly the `(SourceId, Key)` pairs that the matching
  `RegisterNavigation` added, independent of any `_itemGroups` mutation between the two calls.
- Introduce a private navigation-key ledger on `QfcCollectionController`
  (`QuickFiler/Controllers/QfcCollectionController.cs`), populated after each successful registry
  `Add` and drained and cleared by `UnregisterNavigation`.
- Remove the now-unreachable `_registeredDigits` field, its assignment in `RegisterNavigation`, and
  the `var format = _registeredDigits == 2 ? "00" : "";` expression in `UnregisterNavigation`, in a
  single commit (see "Supersession of #472" below for why these three edits are indivisible).
- Add regression coverage in a new test file,
  `QuickFiler.Test/Controllers/QfcCollectionControllerNavigationLedgerTests.cs`, registered in
  `QuickFiler.Test/QuickFiler.Test.csproj`.
- Re-arrange three existing tests in `QuickFiler.Test/Controllers/QfcCollectionControllerTests.cs`
  so their arrangement goes through `RegisterNavigation()` rather than the out-of-band
  `SeedCollectionKey` helper.
- Flip one assertion and rewrite one XML-documentation paragraph in
  `QuickFiler.Test/Controllers/QfcCollectionControllerNavigationDigitsTests.cs`, where the #644
  residual is currently pinned.
- Correct one XML-documentation block and one `because:` string in
  `QuickFiler.Test/Controllers/QfcCollectionControllerDefects468Tests.cs` so the comments match the
  post-fix statement at which the documented `NullReferenceException` originates.

**Out of scope / non-goals**

- Changing the `KbdActions` contract — for example making `Remove` remove all matches, making it
  throw when it removes nothing, or reconciling the exact-match `Add`/`Remove` semantics with the
  substring `Find`/`ContainsKey` semantics. That is a cross-cutting change across every keyboard
  surface in QuickFiler and ExpandedFiler and is deferred to `### Downstream notes` item 5 of the
  #444 spec.
- Auditing or changing the 39 production call sites that discard `KbdActions.Remove`'s `bool`
  return. Same deferral.
- Adding `UnregisterNavigation`/`RegisterNavigation` brackets around `RemoveSpecificControlGroup(int)`,
  `PopOutControlGroup(int)`, or the `'R'` char action. See "Rejected alternatives".
- Splitting `QuickFiler/Controllers/QfcCollectionController.cs` (2437 lines) to bring it under the
  500-line ceiling. That pre-existing violation is recorded below and is a separate refactor per the
  `CLAUDE.md` Bugfix Workflow rule.
- Reopening, reverting, or re-litigating #472. Its landed fix is present on this base and its
  guarantee is strictly strengthened, not withdrawn.
- Any interface change. `IQfcCollectionController.RegisterNavigation()` / `UnregisterNavigation()`
  and `IQfcKeyboardHandler.StringActionsAsync` keep their existing signatures.
- Any production-side project-file change. The recommended design adds no production `.cs` file.
- Any change to the `SetVisualDigits` / WinForms rendering path.

**Explicitly excluded systems, integrations, or datasets**

- No live Outlook process, no COM object, no WinForms handle, no STA apartment, and no
  QuickFiler.Test/TestSupport/WinFormsPumpHost.cs usage in any test added or changed by this fix.
- No network, filesystem, database, or external-process dependency; no temporary files.
- No `LiveOutlook`-categorised test is added or relied on.
- The following files are cited in this document as context and are deliberately **not** wrapped in
  code spans, because the downstream change-footprint harvester treats a backticked repository path
  as an in-scope file: QuickFiler/Controllers/QfcItemController.EventWiring.cs,
  QuickFiler/Controllers/KbdActions.cs, QuickFiler/Controllers/KaStringAsync.cs,
  QuickFiler/Controllers/EfcFormController.cs, QuickFiler/Interfaces/IQfcCollectionController.cs,
  QuickFiler/Interfaces/IQfcKeyboardHandler.cs, QuickFiler/QuickFiler.csproj,
  QuickFiler.Test/Controllers/QfcCollectionController.TestSupport.cs,
  QuickFiler.Test/TestSupport/WinFormsPumpHost.cs, and
  QuickFiler.Test/Controllers/QfcFormControllerTests.cs. None of them is modified by this fix. Do
  not add backticks to them.

## Root Cause Analysis
- `QfcCollectionController.UnregisterNavigation` — loop bound reads the live `_itemGroups.Count`.
- `QfcCollectionController.RemoveSpecificControlGroup(int)` — mutates `_itemGroups`
  (`_itemGroups.RemoveAt(selection - 1)`) with no unregister/register bracket.
- `QfcCollectionController.RemoveBelowThresholdAsync` — reaches the above through the
  `RemoveGroupByEntryId` seam, once per below-threshold group.
- `QfcCollectionController.PopOutControlGroup(int)` — a **third** unbracketed reach into
  `RemoveSpecificControlGroup(int)`, not named in the issue text. Its keyboard entry point is the
  `'P'` char action in QuickFiler/Controllers/QfcItemController.EventWiring.cs.
- **Correction to the issue text (`'R'` vs `'Z'`).** The reach through
  QuickFiler/Controllers/QfcItemController.EventWiring.cs is the **synchronous** `'R'` char action
  registered in `RegisterFocusActions`, whose delegate calls
  `this._parent.RemoveSpecificControlGroup(ItemNumber)`. The **async** `'R'` action in the same file
  is Reply, not remove. The async remove is bound to `'Z'` and routes to
  `_parent.RemoveSpecificControlGroupAsync`, which **is** bracketed by
  `UnregisterNavigation()`/`RegisterNavigation()` and is therefore not defective. Only the
  synchronous `'R'` path participates in this defect.
- **Correction to the issue text (call-site count).** A content search of `*.cs` in this worktree
  returns 41 raw `…Actions*.Remove(` occurrences across four production files; two of them, in
  QuickFiler/Controllers/QfcItemController.EventWiring.cs, are commented out. The live figure is
  therefore **39 production call sites, not 42**. The issue's "31 in EventWiring" figure is exact.
  All 39 discard the `bool` return; two of the 39 do so structurally inside `Action<T>` lambdas
  passed to `ForEach` in QuickFiler/Controllers/EfcFormController.cs. That cross-cutting question is
  recorded separately in `### Downstream notes` item 5 of the #444 spec and is not this issue.
- Line-number citations are deliberately omitted here: the #444/#472/#482 work and epic sibling #468
  both edit `QuickFiler/Controllers/QfcCollectionController.cs`, so any line number recorded today is
  stale on arrival. Every anchor above is a member name.
- Mutation-path census: 17 `_itemGroups` mutation paths were enumerated and classified in
  research/research.2026-08-29T07-55.md section 2. Fourteen are either bracketed
  (`SwapItemGroups`, `RemovedItemMonitor`, `RemoveSpecificControlGroupAsync`,
  `PopOutControlGroupAsync`, `ToggleGroupConv`, `ToggleUnGroupConv`, `AddItemGroup`) or occur before
  any registration is live (`LoadItemGroupsAndViewers_02`) or after teardown (`RemoveControls`,
  `RemoveControlsAsync`, `Cleanup`, `CleanupAsync`). Three are unbracketed with a live registration:
  `RemoveSpecificControlGroup(int)`, `RemoveBelowThresholdAsync` via the `RemoveGroupByEntryId` seam,
  and `PopOutControlGroup(int)`.
- The orphan is always a *tail* orphan: at width 1 the registry holds `"1".."N"` and unregistration
  removes only the first `Count` of them; at width 2 with ten groups shrunk to nine, the residual is
  exactly `{"10"}`, which is what the digits-file assertion pins today.


## Proposed Fix

### Design summary (what changes where):

Option A — a private key ledger field on `QfcCollectionController`, in
`QuickFiler/Controllers/QfcCollectionController.cs`.

Add one lazily-initialised private field alongside the existing seam fields:

```
private List<(string SourceId, string Key)> _registeredNavigationKeys;

private List<(string SourceId, string Key)> RegisteredNavigationKeys =>
    _registeredNavigationKeys ??= new List<(string, string)>();
```

- `RegisterNavigation` keeps its `for (int i = 0; i < _itemGroups.Count; i++)` loop and its
  `SetVisualDigits` refresh. The registration helper holds the constructed `KaStringAsync` instance,
  calls `StringActionsAsync.Add(instance)`, and **then** appends `(instance.SourceId, instance.Key)`
  to the ledger. Appending after the `Add` is load-bearing: a duplicate-key `ArgumentException`
  thrown by `Add` must leave the ledger unpolluted.
- The recorded key is the **stored** `KaStringAsync.Key`, read from the constructed instance rather
  than the pre-construction string. `KaStringAsync`'s constructor and `Key` setter both apply
  `.ToLower()`; for digit keys that transform is the identity, but recording the stored value makes
  the ledger exact by definition rather than by coincidence.
- `UnregisterNavigation` becomes a `foreach` over the ledger calling
  `StringActionsAsync.Remove(sourceId, key)` for each recorded pair, followed by `Clear()`. It no
  longer reads `_itemGroups` at all, and no longer recomputes a width.
- The `_registeredDigits` field, its `// Issue #472:` comment, its assignment in
  `RegisterNavigation`, and the `format` expression in `UnregisterNavigation` are deleted together.

Diff surface on the production side: one file, approximately +8/-6 lines, confined to the private
field block and the `RegisterNavigation` / `UnregisterNavigation` / `RegisterNavigationAsyncAction`
members.

**Supersession of #472 (hard constraint).** #472's fix is already merged on this base — commit
`9494ca35` — so `_registeredDigits` exists in the tree today; the issue text's "the field that #472
introduces" is past tense here. #472 owns the key **format** (the `format` argument to
`(i + 1).ToString(format)`); #644 owns the key **cardinality** (the loop header). A ledger replaces
both, because it replays recorded strings verbatim. Deleting only the `format` expression while
retaining `_registeredDigits` leaves a private field that is assigned and never read, which the C#
compiler reports as **CS0414**; the repository type-check gate runs with
`/p:TreatWarningsAsErrors=true` and promotes CS0414 to an error, and `.editorconfig`'s catch-all
`dotnet_analyzer_diagnostic.severity = suggestion` covers analyzer rule IDs, not compiler `CSxxxx`
diagnostics. The minimum-scope change therefore *necessarily* deletes the field and its assignment
in the same commit as the `format` expression. This is supersession, not a revert: #472's guarantee
— "unregistration removes keys in the width they were registered at" — is strictly strengthened,
because verbatim replay cannot reconstruct a wrong width. #472 is not reopened and is not
re-litigated; this paragraph is the supersession record and the key ledger supersedes both prior
artifacts.

### Boundaries and invariants to preserve:

**Primary invariant (the thing this fix establishes).**

> After any `RegisterNavigation()` / `UnregisterNavigation()` pair, the `"Collection"`-sourced key
> set in `IQfcKeyboardHandler.StringActionsAsync` is exactly what it was before the
> `RegisterNavigation()` call, for every interleaving of `_itemGroups` mutations between the two.

Supporting boundaries that must not change:

- Public surface: `RegisterNavigation()` and `UnregisterNavigation()` keep their signatures and stay
  on `IQfcCollectionController`. No interface file is edited, so the existing
  `Mock<IQfcCollectionController>` verification of the pair in
  QuickFiler.Test/Controllers/QfcFormControllerTests.cs remains valid.
- `RegisterNavigation` continues to throw `ArgumentException` on a second call without an
  intervening unregister, with the existing message shape `*SourceId Collection*`.
- The `_digitRefreshNeeded` / `SetVisualDigits` behaviour inside `RegisterNavigation` is untouched.
- Bracketed mutation paths keep working unchanged: each already unregisters before the mutation and
  registers after, so the ledger is drained and refilled exactly as before.
- The ledger is controller-scoped private state. It is not exposed on any interface and no other
  type reads it.
- Ordering rule: record only after a successful `Add`, so a partially-completed registration can
  never leave the ledger claiming keys the registry does not hold.

### Dependencies or blocked work:

- **None blocking.** #472's fix is already merged on the base commit
  `ecdb1c84ba8541ab67042985919cfed4df768c01`, so no upstream work is pending.
- Epic sibling #468 also edits `QuickFiler/Controllers/QfcCollectionController.cs`; the two changes
  touch different members, but a rebase conflict in that file is possible and is covered under Risks.
- The #468 `[TestMethod]`-count pin on `QuickFiler.Test/Controllers/QfcCollectionControllerTests.cs`
  is treated as **in force**. It is attested second-hand through the #444 evidence artifact
  docs/features/active/quickfiler-keyboard-action-defects-444/evidence/qa-gates/p2-t11-frozen-test-file.2026-08-27T09-45.md;
  the #468 decision log itself is not present in this checkout. The chosen remediation satisfies the
  pin regardless.

### Implementation strategy (what changes, not sequencing):

1. Production: add the ledger field and lazy accessor; record after `Add` in the registration helper;
   replay and clear in `UnregisterNavigation`; delete the three `_registeredDigits` artifacts.
2. New test file: add `QuickFiler.Test/Controllers/QfcCollectionControllerNavigationLedgerTests.cs`
   with the six tests listed under Test Strategy, and register it in
   `QuickFiler.Test/QuickFiler.Test.csproj` with a `Compile Include` item placed in the existing
   block of Controllers\QfcCollectionController items, using the same relative form as its neighbours.
   The project is legacy non-SDK style; an unlisted `.cs` file is silently not compiled.
3. Existing characterisation tests: in
   `QuickFiler.Test/Controllers/QfcCollectionControllerTests.cs`, replace the out-of-band
   `SeedCollectionKey(...)` arrangement with a real `controller.RegisterNavigation()` call in the
   three tests whose outcome changes. No `[TestMethod]` is added or removed; net line delta is at
   most -1.
4. Digits file: in `QuickFiler.Test/Controllers/QfcCollectionControllerNavigationDigitsTests.cs`,
   change the `remaining.Should().Equal(new[] { "10" }, …)` assertion to an empty-collection
   assertion with a `because:` string naming #644, and rewrite the XML-documentation paragraph that
   currently attributes the residual to this follow-up issue so it records the residual as closed.
   The sibling assertion filtering keys that start with `"0"` stays as it is.
5. Comment synchronisation: in
   `QuickFiler.Test/Controllers/QfcCollectionControllerDefects468Tests.cs`, correct the XML doc and
   the one `because:` string that state the `NullReferenceException` originates in
   `UnregisterNavigation()` dereferencing a null `_itemGroups`. Under the ledger the exception still
   occurs and still propagates, but it originates two statements later at `_itemGroups[selection - 1]`
   inside `RemoveSpecificControlGroupAsync`. Assertions are not touched and the test outcome is
   unchanged; the edit is required because `CLAUDE.md` C#6.3 requires comments to stay synchronised
   with behaviour.

#### Files/modules to change:

Complete change footprint. Every path below is repository-relative and is written as a code span so
the downstream footprint harvester can read it. Six are modified; one is created.

| Path | Disposition | Reason |
|---|---|---|
| `QuickFiler/Controllers/QfcCollectionController.cs` | modified | The ledger field, the recording call, the replay loop, and deletion of the three `_registeredDigits` artifacts. |
| `QuickFiler.Test/Controllers/QfcCollectionControllerNavigationLedgerTests.cs` | **created** | New regression coverage for the ledger invariant. |
| `QuickFiler.Test/QuickFiler.Test.csproj` | modified | One `Compile Include` item for the new test file; the project is legacy non-SDK style. |
| `QuickFiler.Test/Controllers/QfcCollectionControllerTests.cs` | modified | Three arrangement lines move from `SeedCollectionKey` to `RegisterNavigation()`. |
| `QuickFiler.Test/Controllers/QfcCollectionControllerNavigationDigitsTests.cs` | modified | One assertion flips from `Equal(new[] { "10" })` to empty; one XML-doc paragraph rewritten. |
| `QuickFiler.Test/Controllers/QfcCollectionControllerDefects468Tests.cs` | modified | XML doc and one `because:` string only; no assertion change. |
| `docs/features/active/2026-08-27-qfc-unregister-navigation-count-mismatch-orphan-644/spec.md` | modified | This document, including acceptance-criteria check-off during execution. |

Timestamped plan, AC-tracking, and evidence artifacts written under
docs/features/active/2026-08-27-qfc-unregister-navigation-count-mismatch-orphan-644/evidence/ during
execution are process outputs of the orchestration workflow, not part of the fix's code diff. They
are deliberately left unbackticked so they do not widen the harvested footprint. Evidence artifacts
must be written under that feature-folder evidence/ tree, in a subfolder named for the artifact kind,
per .claude/skills/evidence-and-timestamp-conventions/SKILL.md. The paths artifacts/baselines/,
artifacts/qa/, and artifacts/coverage/ are not valid destinations.

#### Functions/classes/CLI commands impacted:

- `QfcCollectionController._registeredNavigationKeys` — new private field.
- `QfcCollectionController.RegisteredNavigationKeys` — new private lazy accessor.
- `QfcCollectionController.RegisterNavigation()` — records each key after a successful `Add`; loses
  the `_registeredDigits` assignment.
- `QfcCollectionController.RegisterNavigationAsyncAction(int, int)` — holds the constructed
  `KaStringAsync` instance so the caller can record the stored key.
- `QfcCollectionController.UnregisterNavigation()` — replays and clears the ledger; loses the
  `format` expression and stops reading `_itemGroups`.
- `QfcCollectionController._registeredDigits` — deleted.
- No CLI command surface exists in this project; no CLI change.

#### Data flow and validation changes:

Registration: `Digits` → `GenerateStringKbdAction(i, digits)` → `KaStringAsync` instance →
`StringActionsAsync.Add(instance)` → append `(instance.SourceId, instance.Key)` to the ledger.

Unregistration: ledger → `StringActionsAsync.Remove(sourceId, key)` per recorded pair → ledger
`Clear()`.

The removed data flow is `_itemGroups.Count` → loop bound and `_registeredDigits` → `format` →
`(i + 1).ToString(format)`. After this change, `_itemGroups` is not an input to unregistration at
all, which is the point of the fix. No new validation is introduced; the uniqueness invariant
continues to be enforced by `KbdActions.Add`.

Ledger state model:

| State | Meaning | Transition |
|---|---|---|
| `null` | instance allocated without running field initialisers (a reflection-built test instance) | first access through the lazy accessor → `empty` |
| `empty` | no navigation keys registered by this controller | `RegisterNavigation()` with `_itemGroups.Count == n` → `populated(n)` |
| `populated(n)` | exactly the `n` recorded pairs are live in the registry | `UnregisterNavigation()` → `empty`; a second `RegisterNavigation()` throws `ArgumentException` on the first key and leaves the state at `populated(n)` |

`_itemGroups` mutations do not appear in this table.

#### Error handling and logging updates:

- No new logging. `KbdActions.Add` already logs before throwing `ArgumentException`, and the
  QuickFiler `KeyboardHandler` already catches and logs at the keyboard boundary.
- No new exception type is introduced and no exception is swallowed.
- `UnregisterNavigation` continues to discard `Remove`'s `bool`. Under the ledger a `false` return is
  only reachable if a key was removed from the registry out of band; converting the discard into a
  check is part of the deferred cross-cutting `KbdActions` question, not this fix.
- Behaviour change worth recording: after this fix a post-`Cleanup` `UnregisterNavigation()` — where
  `_itemGroups` has been set to `null` — is a no-op instead of a `NullReferenceException`. This
  removes a latent failure mode and does not alter the documented outcome of
  `QuickFiler.Test/Controllers/QfcCollectionControllerDefects468Tests.cs`, whose expected
  `NullReferenceException` arises in `RemoveSpecificControlGroupAsync` rather than in
  `UnregisterNavigation`.

#### Rollback/feature-flag considerations (if applicable):

No feature flag. The change is a single-commit, self-contained edit across one production file and
five test-side files; rollback is `git revert` of that commit. Reverting restores the #472 state
exactly, including `_registeredDigits`, because the field deletion and the ledger addition are in the
same commit. No data, configuration, or persisted state is involved, so there is nothing to migrate
back.

### Technical specifications (interfaces/contracts):

- Ledger element type: `(string SourceId, string Key)` value tuple, stored in a
  `List<(string SourceId, string Key)>`. Value tuples are available on this target framework; no
  `init` accessor, `record`, or `record struct` is used, since none is available on net48 in this
  repository.
- Registry contract consumed, unchanged:
  `KbdActions<string, KaStringAsync, Func<string, Task>>.Add(KaStringAsync)` throws
  `ArgumentException` on a duplicate `(SourceId, Key)`; `Remove(string sourceId, TKey key)` returns
  `bool` and removes at most one element by exact match.
- No public or `internal` API is added. The ledger field and accessor are `private`.
- No new NuGet package, analyzer, or project reference.

#### Inputs/outputs and formats:

- Input to registration: `Digits` (1 or 2) and `_itemGroups.Count`. Key format is unchanged —
  `(i + 1).ToString()` at width 1 and `(i + 1).ToString("00")` at width 2, `SourceId` the literal
  `"Collection"`.
- Output of unregistration: the `"Collection"`-sourced subset of `StringActionsAsync` returns to its
  pre-registration contents; the ledger returns to empty.
- No serialised format, wire format, or file format is read or written.

#### Required configuration keys and defaults:

None. No configuration key, app setting, registry value, or environment variable is added, read, or
changed.

#### Backward-compatibility expectations:

- Fully backward compatible at every boundary: no interface signature changes, no public member is
  added or removed, no persisted state exists.
- The only observable behaviour differences are (a) unregistration is now total across unbracketed
  mutations, and (b) a post-`Cleanup` unregister no longer throws. Both are corrections in the
  direction of the documented expected behaviour.
- Existing callers require no change.

#### Performance constraints (latency/throughput/memory):

- Unregistration changes from `O(n)` list scans bounded by `_itemGroups.Count` to `O(n)` scans
  bounded by the ledger length; `n` is the number of item groups on a page, in practice below 20.
- Memory: one `List<(string, string)>` per controller instance, lazily allocated, holding one entry
  per registered navigation key. Negligible.
- No new allocation on a hot path, no I/O, no async work added. No measurable latency impact is
  expected and none needs to be measured.

## Assumptions, Constraints, Dependencies

**Assumptions (environment, data, access)**

- The base commit is `ecdb1c84ba8541ab67042985919cfed4df768c01` on branch
  `bug/qfc-unregister-navigation-count-mismatch-orphan-644`, and #472's fix (commit `9494ca35`) is
  present in it. Every line-anchored claim in the research artifact was read at that SHA.
- The executor has a working shell with `git`, `dotnet`, `msbuild`, and `vstest.console.exe`
  available. `vstest.console.exe` is located via
  `vswhere -latest -products * -find 'Common7\IDE\Extensions\TestPlatform\vstest.console.exe'`.
- `dotnet tool restore` has been run at least once in this worktree before the first CSharpier
  invocation.
- **Baseline caveat.** The research session had no shell. The "exactly 500 lines" and "13
  `[TestMethod]`" figures for `QuickFiler.Test/Controllers/QfcCollectionControllerTests.cs`, the
  "226 lines / 3 `[TestMethod]`" figures for
  `QuickFiler.Test/Controllers/QfcCollectionControllerNavigationDigitsTests.cs`, the 2437-line figure
  for `QuickFiler/Controllers/QfcCollectionController.cs`, and the 39/41/31 call-site counts were all
  derived by reading files, not by running counting commands. The executor must re-derive them in
  Phase 0 with actual commands before treating any of them as a gate baseline, and must record the
  measured values as evidence.

**Constraints (budget, performance, compatibility)**

- `QuickFiler.Test/Controllers/QfcCollectionControllerTests.cs` is at the 500-line repository ceiling
  and may not grow by even one line; its `[TestMethod]` count is pinned at 13 by the #468 freeze.
  Both constraints are hard.
- `QuickFiler/Controllers/QfcCollectionController.cs` is 2437 lines — already 4.9x over the 500-line
  ceiling — and carries `[ExcludeFromCodeCoverage]`. This is a **known pre-existing violation,
  recorded here and deliberately not fixed**. Adding roughly eight lines does not create the
  violation, and splitting the file is a much larger refactor that the `CLAUDE.md` Bugfix Workflow
  directs to a separate issue rather than into a bugfix's scope.
- The `[ExcludeFromCodeCoverage]` attribute means the production side of this fix contributes nothing
  to the coverage denominator and cannot move the repository coverage figure. Coverage is therefore
  not the instrument that proves this fix; the new tests are.
- Tests must keep `_digits` equal to the width the page needs. A mismatch sets `_digitRefreshNeeded`
  and routes `RegisterNavigation` into `SetVisualDigits`, which dereferences `grp.ItemController` and
  `grp.ItemViewer` and requires WinForms.
- `RemoveSpecificControlGroup(int)` cannot be called from a unit test: its body reaches
  `TableLayoutHelper.RemoveSpecificRow`, `_moveMonitor.UnhookItem`, `ResetPanelHeight()`, and
  `_parent.ActionOkAsync()`. Tests must model that mutation through the `_removeGroupByEntryId` seam
  or by mutating the injected `_itemGroups` list directly.
- The minimum-scope constraint from the Bugfix Workflow: change only what is needed to make the
  failing regression test pass.

**External dependencies (services, libraries, releases)**

- CSharpier 1.2.6, pinned by `dotnet-tools.json`; always invoked through `dotnet tool run`.
- MSTest, Moq, and FluentAssertions, all already referenced by `QuickFiler.Test/QuickFiler.Test.csproj`.
  No new package is added.
- No external service, network endpoint, or release dependency.

## Data / API / Config Impact

- **User-facing or API changes:** none. No public API, no interface signature, and no user-visible
  surface changes. The user-observable effect is the removal of a defect: navigation digit keys stop
  going dead after a group is removed through an unbracketed path.
- **Data or migration considerations:** none. The ledger is transient in-memory state scoped to a
  controller instance. Nothing is persisted, serialised, or migrated.
- **Logging/telemetry updates:** none added. Existing `KbdActions` logging on duplicate `Add` and
  existing `KeyboardHandler` boundary logging are unchanged. The fix reduces the frequency with which
  those error paths are reached.
- **Compatibility notes:** no CLI flag, config schema, or version number changes.
  QuickFiler/QuickFiler.csproj is not modified. The only project-file edit is a single
  `Compile Include` item in `QuickFiler.Test/QuickFiler.Test.csproj` for the new test file.

## Test Strategy
Seeded from issue:

- [x] Unit coverage areas: a key-ledger in `QfcCollectionController` that records the exact
  `(SourceId, Key)` set produced by `RegisterNavigation` and replays that recorded set in
  `UnregisterNavigation`, making unregistration total and independent of any intervening
  `_itemGroups` mutation. This supersedes both the count bound and the `_registeredDigits` width
  field that #472 introduces.
- [x] Integration scenario to retest: remove a group through `RemoveBelowThresholdAsync` and through
  the `'R'` char action, then unregister and re-register navigation, asserting the registry is empty
  between the two.
- [x] Manual verification notes: the key-ledger design changes the outcome of the existing
  characterisation tests in `QuickFiler.Test/Controllers/QfcCollectionControllerTests.cs`. That file
  is at the 500-line ceiling and its `[TestMethod]` count was frozen by issue #468, so this fix must
  either be scheduled after that freeze is lifted or place its tests in a new file.

Corrections to the seeded text above: `_registeredDigits` is already present in this checkout rather
than being introduced by pending work, and the defective char action is the synchronous `'R'`, not
the async one. The chosen resolution of the third bullet is the new-file option.

**Regression tests to add** — all in
`QuickFiler.Test/Controllers/QfcCollectionControllerNavigationLedgerTests.cs`, MSTest `[TestClass]`,
Moq for `IQfcKeyboardHandler` / `MailItem` / `IQfcItemController`, FluentAssertions throughout:

| # | Test name | Scenario | Pre-fix result |
|---|---|---|---|
| T1 | `UnregisterNavigation_AfterGroupRemovedThroughRemoveGroupByEntryIdSeam_RemovesEveryRegisteredKey` | Register a 10-group page at width 2, remove one group through a recording `_removeGroupByEntryId` delegate that performs only the list mutation (models `RemoveBelowThresholdAsync`), unregister; assert zero `"Collection"` entries remain. | **Red** — leaves `"10"`. |
| T2 | `UnregisterNavigation_AfterUnbracketedItemGroupsRemoval_ThenReRegister_DoesNotThrow` | Register a 5-group page at width 1, remove one group directly from the injected `_itemGroups` list (models the synchronous `'R'` char action and `PopOutControlGroup(int)`, which share the `RemoveSpecificControlGroup(int)` reach), unregister, then register again; assert no throw and exactly one entry per key of the new page. | **Red** — `ArgumentException`. |
| T3 | `RegisterAndUnregisterNavigation_RepeatedCycles_LeaveRegistryEmpty` | Register, unregister, register, unregister; assert registry empty and no throw. | Green (state-transition coverage). |
| T4 | `UnregisterNavigation_WithNoPriorRegistration_DoesNotThrowAndLeavesRegistryUnchanged` | Unregister on a controller that never registered, with unrelated entries present; assert no throw and registry unchanged. | Green (empty-ledger negative case). |
| T5 | `UnregisterNavigation_AfterItemGroupsSetToNull_DoesNotThrow` | Register, set `_itemGroups` to null (models the post-`Cleanup` state), unregister; assert no throw and registry empty. Structurally proves `UnregisterNavigation` no longer reads `_itemGroups`. | **Red** — `NullReferenceException`. |
| T6 | `UnregisterNavigation_AfterTwoDigitRegistrationAndShrinkToNine_LeavesNoCollectionKeys` | Register 10 groups at width 2, shrink `_itemGroups` to 9, unregister; assert no residual. #644-side companion to the #472 width test. | **Red** — leaves `"10"`. |

**Unit tests for boundaries (MSTest, not pytest — the template's pytest wording does not apply to
this repository).** T3 through T5 cover the boundary and negative cases: empty ledger, repeated
cycles, and a null `_itemGroups`. T2 additionally covers the re-registration boundary, which is
where the pre-fix defect becomes an observable exception.

**Existing tests that change:**

- `QuickFiler.Test/Controllers/QfcCollectionControllerTests.cs` — three tests change arrangement
  only, from `SeedCollectionKey(...)` to `controller.RegisterNavigation()`:
  `LoadControlsAndHandlers_01_ReportedRepro_SwapToOverlappingCachedPage_ThrowsBeforeFix` (keeps one
  surviving `SeedCollectionKey(kbd, "2")` call to model the pre-existing orphan, so its
  `*Key 2 SourceId Collection*` message assertion is preserved),
  `LoadControlsAndHandlers_01_SwapsPage_RemovesOutgoingKeysAndAddsIncomingKeys` (both seeds collapse
  into one `RegisterNavigation()` call), and
  `SwapItemGroups_ThenSkipGuardedTrailingRegister_LeavesExactlyOneEntryPerIncomingKey`. All
  assertions are preserved verbatim; the file keeps 13 `[TestMethod]` attributes and stays at or
  under 500 lines. `SeedCollectionKey` remains used and does not become dead code.
  `RegisterNavigation_CalledTwiceWithoutInterveningUnregister_ThrowsArgumentException` needs no
  change and must keep passing.
- `QuickFiler.Test/Controllers/QfcCollectionControllerNavigationDigitsTests.cs` — the
  `remaining.Should().Equal(new[] { "10" }, …)` assertion in
  `UnregisterNavigation_AfterRegisteringAtTwoDigitsAndShrinkingToNine_RemovesTheTwoDigitKeys` becomes
  an empty-collection assertion, and its XML-documentation paragraph is rewritten from "The single
  residual `"10"` entry is expected and is NOT this fix's scope" to a record that #644 has closed the
  residual. The `.Where(k => k.StartsWith("0")).Should().BeEmpty(…)` sibling assertion is unchanged.
  The other two `[TestMethod]`s in the file pass unchanged; the file keeps 3 `[TestMethod]`
  attributes.
- `QuickFiler.Test/Controllers/QfcCollectionControllerDefects468Tests.cs` — XML doc and one
  `because:` string in
  `RemoveSpecificControlGroupAsync_ThrowAtFirstStatement_RestoresReentrancyCounter` are corrected.
  No assertion changes and no outcome change.

**Edge cases and negative scenarios:** empty page (`_itemGroups.Count == 0`) yields an empty ledger
and a no-op unregister; unregister with no prior register (T4); null `_itemGroups` after cleanup
(T5); width crossing at the 9/10 boundary (T1, T6); duplicate registration leaving the ledger
unpolluted, which is what preserves the existing
`RegisterNavigation_CalledTwiceWithoutInterveningUnregister_ThrowsArgumentException` outcome.

**Error handling and logging verification:** the existing `ArgumentException` message shape is
asserted by the retained duplicate-registration test and by the `*Key 2 SourceId Collection*`
assertion in the amended reported-repro test. No new logging is added, so none is asserted.

**Testability posture (all tests are host-free):** allocate the controller with
`FormatterServices.GetUninitializedObject` via the existing `CreateUninitializedController()` helper;
supply a real parameterless `KbdActions<string, KaStringAsync, Func<string, Task>>` through a
`Mock<IQfcKeyboardHandler>.SetupGet(x => x.StringActionsAsync)`; inject `_kbdHandler`, `_digits`, and
`_itemGroups` by reflection; build item groups from `Mock<MailItem>` with `EntryID` set up; use the
`_removeGroupByEntryId` seam for the removal path. No live Outlook, no COM object, no WinForms
handle, no STA apartment, no temporary file, no wall-clock wait, and no mutable static state. Keep
`_digits` equal to the page width so `RegisterNavigation` does not route into `SetVisualDigits`.

**Coverage impact and targets for changed lines/modules:** `QfcCollectionController` carries
`[ExcludeFromCodeCoverage]`, so the changed production lines are outside the coverage denominator and
this fix cannot move the repository figure. The gate to satisfy is therefore "no regression on
changed lines": the repository-wide figure must not fall relative to the Phase 0 baseline, captured
before any edit. New test code is exercised by definition.

**Toolchain commands to run (format → lint → type-check → test).** Run in this exact order; restart
from step 1 if any step fails or rewrites a file.

1. `dotnet tool restore` (first run in this worktree only), then `dotnet tool run csharpier format .`
   and verify with `dotnet tool run csharpier check .`
2. `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
3. `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`
   — do not add `/p:Nullable=enable` and do not substitute `/t:Build`; this is the gate that makes
   the CS0414 constraint load-bearing.
4. `vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /EnableCodeCoverage /InIsolation /Logger:trx /TestCaseFilter:"TestCategory!=LiveOutlook"`
   — when discovering assemblies locally, exclude `\.claude\`, `\obj\`, and `\ref\` paths so nested
   agent worktrees do not contribute duplicate assemblies.

**Manual validation steps:** none required. The issue's "bring up the QuickFiler collection surface"
note describes the user-visible symptom, not a required validation step; the defect and its fix are
both fully observable through the `KbdActions` registry in a unit test, which is how #472 pinned the
same code path.


## Acceptance Criteria

Check off only after the named test or command has actually been run and passed. AC-0 must be
completed before any other criterion is evaluated.

- [ ] **AC-0 (Phase 0 baselines).** Before any edit, re-derive and record as evidence: the line count
      and `[TestMethod]` count of `QuickFiler.Test/Controllers/QfcCollectionControllerTests.cs`
      (expected 500 and 13), the line count and `[TestMethod]` count of
      `QuickFiler.Test/Controllers/QfcCollectionControllerNavigationDigitsTests.cs` (expected 226 and
      3), the line count of `QuickFiler/Controllers/QfcCollectionController.cs` (expected 2437), and
      the repository coverage figure from a clean run of the step-4 `vstest.console.exe` command.
      Verified by executed commands with captured output — for example
      `(Get-Content QuickFiler.Test\Controllers\QfcCollectionControllerTests.cs).Count` and
      `(Select-String -Path QuickFiler.Test\Controllers\QfcCollectionControllerTests.cs -Pattern '\[TestMethod\]').Count`.
      Any figure that differs from the expected value above is recorded as a discrepancy and the plan
      is re-checked against the measured value, not the expected one.
- [ ] **AC-1 (red before green).** T1
      (`UnregisterNavigation_AfterGroupRemovedThroughRemoveGroupByEntryIdSeam_RemovesEveryRegisteredKey`)
      is demonstrated failing against the unmodified production code, with the failure output
      captured as evidence, before the production fix is applied.
- [ ] **AC-2 (`RemoveBelowThresholdAsync` path).** After a group is removed through the
      `_removeGroupByEntryId` seam between register and unregister, zero `"Collection"`-sourced keys
      remain in `StringActionsAsync`. Verified by T1 passing.
- [ ] **AC-3 (`RemoveSpecificControlGroup(int)` path — synchronous `'R'` char action and
      `PopOutControlGroup(int)`).** After an unbracketed `_itemGroups` removal between register and
      unregister, unregistration is total and a subsequent `RegisterNavigation()` throws no
      `ArgumentException`. Verified by T2
      (`UnregisterNavigation_AfterUnbracketedItemGroupsRemoval_ThenReRegister_DoesNotThrow`) passing.
- [ ] **AC-4 (width-crossing path).** Registering ten groups at width 2 and shrinking to nine leaves
      no residual key. Verified by T6
      (`UnregisterNavigation_AfterTwoDigitRegistrationAndShrinkToNine_LeavesNoCollectionKeys`)
      passing.
- [ ] **AC-5 (state transitions).** Repeated register/unregister cycles leave the registry empty and
      throw nothing. Verified by T3 (`RegisterAndUnregisterNavigation_RepeatedCycles_LeaveRegistryEmpty`)
      passing.
- [ ] **AC-6 (empty-ledger negative case).** `UnregisterNavigation()` with no prior registration
      throws nothing and leaves unrelated registry entries untouched. Verified by T4
      (`UnregisterNavigation_WithNoPriorRegistration_DoesNotThrowAndLeavesRegistryUnchanged`) passing.
- [ ] **AC-7 (`UnregisterNavigation` no longer reads `_itemGroups`).** With `_itemGroups` set to
      null, `UnregisterNavigation()` completes without throwing and drains the ledger. Verified by T5
      (`UnregisterNavigation_AfterItemGroupsSetToNull_DoesNotThrow`) passing.
- [ ] **AC-8 (new test file exists and is compiled).**
      `QuickFiler.Test/Controllers/QfcCollectionControllerNavigationLedgerTests.cs` exists, and a
      `Compile Include` item naming it is present in `QuickFiler.Test/QuickFiler.Test.csproj`.
      Verified by the step-2 `msbuild` command succeeding and by all six of T1–T6 appearing as
      executed results in the `/Logger:trx` output of the step-4 `vstest.console.exe` command; a
      missing csproj entry manifests as the six tests being absent from the trx.
- [ ] **AC-9 (amended characterisation tests pass).** All three amended tests in
      `QuickFiler.Test/Controllers/QfcCollectionControllerTests.cs` pass:
      `LoadControlsAndHandlers_01_ReportedRepro_SwapToOverlappingCachedPage_ThrowsBeforeFix` (still
      asserting `*Key 2 SourceId Collection*`),
      `LoadControlsAndHandlers_01_SwapsPage_RemovesOutgoingKeysAndAddsIncomingKeys`, and
      `SwapItemGroups_ThenSkipGuardedTrailingRegister_LeavesExactlyOneEntryPerIncomingKey`. The
      unchanged `RegisterNavigation_CalledTwiceWithoutInterveningUnregister_ThrowsArgumentException`
      also passes. Verified in the step-4 trx.
- [ ] **AC-10 (frozen-file constraints hold).**
      `QuickFiler.Test/Controllers/QfcCollectionControllerTests.cs` still contains exactly 13
      `[TestMethod]` attributes and is at or under 500 lines after the edit. Verified by re-running
      the two AC-0 counting commands against the edited file and comparing to the recorded baseline.
- [ ] **AC-11 (digits-file assertion flipped and passing).** In
      `QuickFiler.Test/Controllers/QfcCollectionControllerNavigationDigitsTests.cs`,
      `UnregisterNavigation_AfterRegisteringAtTwoDigitsAndShrinkingToNine_RemovesTheTwoDigitKeys` now
      asserts an empty `"Collection"` key set with a `because:` string naming #644, its XML
      documentation records the residual as closed rather than as out of scope, the file still
      contains exactly 3 `[TestMethod]` attributes, and all three tests in the file pass. Verified in
      the step-4 trx plus a read of the edited assertion and doc block.
- [ ] **AC-12 (`_registeredDigits` fully removed, no CS0414).** A content search for
      `_registeredDigits` across the repository returns zero occurrences, and the step-3 command
      `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`
      completes with exit code 0 and no CS0414 diagnostic in its output.
- [ ] **AC-13 (comment synchronisation, no assertion drift).** In
      `QuickFiler.Test/Controllers/QfcCollectionControllerDefects468Tests.cs`, the XML doc and
      `because:` string of `RemoveSpecificControlGroupAsync_ThrowAtFirstStatement_RestoresReentrancyCounter`
      attribute the `NullReferenceException` to `_itemGroups[selection - 1]` in
      `RemoveSpecificControlGroupAsync` rather than to `UnregisterNavigation()`; `git diff` for that
      file shows changes to comment and string literals only, with no assertion edit; and the test
      passes in the step-4 trx.
- [ ] **AC-14 (footprint containment).** `git diff --name-only` against the base commit
      `ecdb1c84ba8541ab67042985919cfed4df768c01` lists only the seven paths enumerated in the
      Blast Radius section — no production file is added, QuickFiler/QuickFiler.csproj is
      unchanged, and no interface file is touched. `git diff --stat` for
      `QuickFiler/Controllers/QfcCollectionController.cs` shows a net addition of no more than 10
      lines, confined to the private field block and the `RegisterNavigation` /
      `UnregisterNavigation` / `RegisterNavigationAsyncAction` members.
- [ ] **AC-15 (full toolchain pass).** In one uninterrupted pass with no file rewritten by any step:
      `dotnet tool run csharpier check .` reports no unformatted file;
      `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
      exits 0 with no new analyzer diagnostic;
      `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`
      exits 0; and
      `vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /EnableCodeCoverage /InIsolation /Logger:trx /TestCaseFilter:"TestCategory!=LiveOutlook"`
      reports zero failed tests. Each command and its result is recorded as evidence.
- [ ] **AC-16 (no coverage regression on changed lines).** The repository coverage figure from the
      AC-15 step-4 run is greater than or equal to the AC-0 baseline. Changed production lines live
      in a `[ExcludeFromCodeCoverage]` class and are therefore outside the denominator; that fact is
      stated explicitly in the coverage evidence artifact so the gate is not read as vacuously
      satisfied.
- [ ] **AC-17 (evidence location).** Every baseline, QA-gate, coverage, and regression artifact
      produced for this issue is written under
      docs/features/active/2026-08-27-qfc-unregister-navigation-count-mismatch-orphan-644/evidence/
      in a subfolder named for the artifact kind, with ISO-8601 timestamps, per
      .claude/skills/evidence-and-timestamp-conventions/SKILL.md. No artifact is written to
      artifacts/baselines/, artifacts/qa/, or artifacts/coverage/.

## Blast Radius (change footprint)

Every repository path the fix's diff will create or modify, as concrete repository-relative paths:

- `QuickFiler/Controllers/QfcCollectionController.cs` — **modified**
- `QuickFiler.Test/Controllers/QfcCollectionControllerNavigationLedgerTests.cs` — **created**
- `QuickFiler.Test/Controllers/QfcCollectionControllerTests.cs` — **modified**
- `QuickFiler.Test/Controllers/QfcCollectionControllerNavigationDigitsTests.cs` — **modified**
- `QuickFiler.Test/Controllers/QfcCollectionControllerDefects468Tests.cs` — **modified**
- `QuickFiler.Test/QuickFiler.Test.csproj` — **modified**
- `docs/features/active/2026-08-27-qfc-unregister-navigation-count-mismatch-orphan-644/spec.md` — **modified**

Seven paths total: one created, six modified. Any path outside this list appearing in
`git diff --name-only` is a scope violation and is a blocking finding at review.

## Risks & Mitigations

**Technical or operational risks**

1. *A fourth unbracketed mutation path is added later and reintroduces the defect.* Low after this
   fix: the ledger makes the invariant structural rather than a coincidence between two
   independently-computed counts, so `_itemGroups` mutations are irrelevant to unregistration by
   construction. **Mitigation:** T5 pins that `UnregisterNavigation` does not read `_itemGroups` at
   all, so a regression that reintroduces a `_itemGroups`-derived bound fails a test.
2. *Ledger/registry drift if `StringActionsAsync` is reassigned between register and unregister.*
   No such path exists today — `StringActionsAsync` is assigned nowhere in production, and the
   `CharActions` / `CharActionsAsync` reassignments in `LoadItemGroupsAndViewers_02` target different
   registries. **Mitigation:** recorded here as a known precondition; if a future change makes
   `StringActionsAsync` assignable, this invariant must be revisited.
3. *`NullReferenceException` on a reflection-allocated controller* — `GetUninitializedObject`
   bypasses field initialisers, so the ledger field is null on every test instance.
   **Mitigation:** the lazy `??=` accessor, which is the idiom already used twice in the same file
   for the `_removeGroupByEntryId` and `_notifyNotReady` seams. T4 and T5 exercise the null-ledger
   path directly.
4. *Ledger polluted by a failed `Add`, causing a later `Remove` of a key that was never registered.*
   **Mitigation:** record strictly after a successful `Add`. The retained test
   `RegisterNavigation_CalledTwiceWithoutInterveningUnregister_ThrowsArgumentException` fails if this
   ordering is inverted.
5. *Recorded key differs from the stored key* because `KaStringAsync`'s constructor and `Key` setter
   lower-case their input. **Mitigation:** record `instance.Key` read back from the constructed
   instance, not the pre-construction string, making the ledger exact by definition.
6. *Rebase conflict with epic sibling #468*, which also edits
   `QuickFiler/Controllers/QfcCollectionController.cs`. **Mitigation:** the edits are confined to the
   private field block and three navigation members; resolve by re-applying those hunks and re-running
   the full toolchain rather than by accepting a merged hunk.
7. *Accidental growth of the frozen test file.* **Mitigation:** AC-10 re-measures the line and
   `[TestMethod]` counts after the edit and compares to the AC-0 baseline; the planned edit is
   arrangement-only with a net delta of at most -1 line.
8. *The #468 freeze is known only second-hand* — the #468 decision log is not present in this
   checkout and could not be verified at first hand. **Mitigation:** treat the pin as in force. The
   chosen remediation satisfies it regardless of whether it is still binding, so the uncertainty does
   not change the plan.
9. *Perceived reversal of #472.* Deleting `_registeredDigits` in this commit could be mistaken for a
   revert. **Mitigation:** the supersession record under "Design summary" and the rewritten XML doc
   in `QuickFiler.Test/Controllers/QfcCollectionControllerNavigationDigitsTests.cs` both state that
   #472's guarantee is strengthened, not withdrawn, and that CS0414 under the repository type-check
   gate makes the deletion mandatory rather than optional.
10. *Coverage gate reads as vacuously satisfied* because the production class is
    `[ExcludeFromCodeCoverage]`. **Mitigation:** AC-16 requires this to be stated explicitly in the
    coverage evidence, and the fix's proof rests on the six new tests rather than on the coverage
    figure.

**Mitigations and rollbacks**

Rollback is a single `git revert` of the fix commit, which restores the #472 state including
`_registeredDigits`. No feature flag, no data migration, and no staged rollout is involved.

**Rejected alternatives**

- **Option B — an extracted `NavigationKeyLedger` type** in a new production file, composed into
  `QfcCollectionController` as a readonly field. Advantages: unit-testable with no reflection, and it
  nudges the 2437-line file toward decomposition. Rejected because it requires a `Compile Include`
  edit to QuickFiler/QuickFiler.csproj (the project is legacy non-SDK style), adding a second
  production file and a production project-file change to the diff; because the type would still need
  the registry or `_kbdHandler` passed in, reintroducing the coupling the extraction was meant to
  remove; and because roughly 30 lines wrapping a `List<T>` is thinner than the indirection costs.
  The `CLAUDE.md` Bugfix Workflow mandates the minimal targeted fix. Option B remains the natural
  design if the file is later decomposed.
- **Bracket the three unbracketed paths** with `UnregisterNavigation()`/`RegisterNavigation()`.
  Rejected: it fixes the three reaches known today while leaving the "unregistration must be total"
  invariant expressed as a coincidence between two independently-computed counts, so the next
  unbracketed mutation reintroduces the defect. It also changes runtime behaviour on the removal path
  by forcing a full re-registration per removal inside `RemoveBelowThresholdAsync`'s loop, which is a
  wider behavioural change than the ledger.
- **Ledger plus a retained count-bounded loop (belt and braces).** Would keep all four navigation
  tests in `QuickFiler.Test/Controllers/QfcCollectionControllerTests.cs` green with no edits.
  Rejected: it leaves the defective count bound in the source as apparently-live code, forces
  `_registeredDigits` to be retained to feed the residual loop's format, and violates "simplicity
  first". The test edits it avoids amount to three arrangement lines.
- **Change `KbdActions.Remove` to remove all matches, or to throw when it removes nothing.**
  Rejected: `KbdActions` is shared by every keyboard surface in QuickFiler and ExpandedFiler across
  39 production call sites, all of which discard the `bool`; changing its contract is a cross-cutting
  change the issue itself defers to `### Downstream notes` item 5 of the #444 spec. It also would not
  fix #644, because the un-visited tail keys are never passed to `Remove` at all.

## Rollout & Follow-up

**Release/rollout steps**

1. Land the fix as a single commit on `bug/qfc-unregister-navigation-count-mismatch-orphan-644`,
   containing the production change, the new test file, the csproj registration, and the three
   test-side edits together. Splitting the `_registeredDigits` deletion from the `format` deletion
   produces a commit that fails the type-check gate on CS0414.
2. Complete the full four-step toolchain pass (AC-15) and record the evidence artifacts.
3. Open a PR to `main` referencing #644, and cross-reference #472 in the body with the supersession
   statement so the `_registeredDigits` deletion is not read as a revert.
4. No deployment step, configuration change, or migration is required; the change ships with the
   next add-in build.

**Post-fix monitoring or clean-up tasks**

- No telemetry to watch. The relevant signal is the absence of `ArgumentException` "Cannot add key
  because it already exists" and `InvalidOperationException` "Multiple sources have registered
  actions for Key" entries from the QuickFiler `KeyboardHandler` log for `SourceId` `"Collection"`.
- Confirm at review that the rewritten XML documentation in
  `QuickFiler.Test/Controllers/QfcCollectionControllerNavigationDigitsTests.cs` no longer points a
  reader at #644 as an open residual.

**Follow-up work deliberately not done here (each needs its own issue if pursued)**

- The cross-cutting question of the 39 production call sites that discard `KbdActions.Remove`'s
  `bool`, recorded in `### Downstream notes` item 5 of the #444 spec.
- The `Add`/`Remove` exact-match versus `Find`/`ContainsKey` substring-match asymmetry in
  `KbdActions`, which is what turns an orphaned `"10"` into a collision on a probe of `"1"`.
- Splitting `QuickFiler/Controllers/QfcCollectionController.cs` (2437 lines, 4.9x the 500-line
  ceiling) and reducing its reliance on `[ExcludeFromCodeCoverage]`. This is a pre-existing violation
  recorded, not introduced, by this fix.
- Applying the same "unregister by replaying a recorded set" shape more widely; ExpandedFiler's
  `ToggleOffNavigation` already uses a recorded catalogue rather than a count, so this fix brings
  `QfcCollectionController` into line with the sibling controller rather than inventing a new pattern.

**Links**

- Issue: https://github.com/drmoisan/TaskMaster/issues/644
- Feature folder: docs/features/active/2026-08-27-qfc-unregister-navigation-count-mismatch-orphan-644/
- Research: docs/features/active/2026-08-27-qfc-unregister-navigation-count-mismatch-orphan-644/research/research.2026-08-29T07-55.md
- Related issues: #472 (key format — superseded by the ledger, not reverted), #444 and #482
  (`quickfiler-keyboard-action-defects`), #468 (test-file freeze), epic `quickfiler-bug-family`
- Base commit: `ecdb1c84ba8541ab67042985919cfed4df768c01`
