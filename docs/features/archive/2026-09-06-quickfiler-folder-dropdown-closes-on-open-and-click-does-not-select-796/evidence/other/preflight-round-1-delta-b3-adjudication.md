# B3 scope adjudication — issue #796

- Timestamp: 2026-09-07T04:05
- Raised by: atomic-planner, revision round 1, as `SELF-REVIEW: BLOCKED`
- Decided by: orchestrator
- Supersedes: Delta 3a, 3b and 3c of evidence/other/preflight-round-1-delta.md

## What the planner found, and it is correct

Preflight round 1 defect B3 established that the per-item `SelectorWasOpen` value the AC6 diagnostic
must report has no source reachable from the write set. The orchestrator adjudicated that by adding
`QuickFiler/Interfaces/IQfcItemController.cs` as a seventeenth write-set path and declaring a
get-only member on it.

Applying that adjudication, the planner found it incomplete rather than wrong, and refused to hand
off. Adding a member to that interface breaks a compiled hand-written implementor that lies outside
the write set:

- `QuickFiler.Test/Helper Classes/QfcThemeHelperTests.cs` line 337 declares
  `private sealed class FakeQfcItemController : IQfcItemController` with concrete members for the
  whole interface.
- That file is compiled: `QuickFiler.Test/QuickFiler.Test.csproj` line 217 carries its
  `Compile Include` entry.
- The target is .NET Framework 4.8, so a default interface member is not available as an escape.

The result is CS0535, and P1-T4's acceptance clause requiring the solution to compile is
unsatisfiable inside the seventeen-path write set.

Both facts were independently re-verified by the orchestrator before this ruling: the
`FakeQfcItemController` declaration is at that line in that file, and it is the only implementor of
that interface in the test project.

The planner also correctly established that `QuickFiler/Legacy/QfcController.cs` line 20 declares the
same interface but is NOT compiled, because `QuickFiler/QuickFiler.csproj` carries no `Compile
Include` entry matching Legacy. That one is therefore not a problem.

Refusing to hand off rather than silently widening the write set was the correct call. A backticked
path is a write claim feeding blast-radius scheduling against three concurrently prepared sibling
items, and that decision was reserved to the orchestrator.

## Ruling: take neither of the two options offered; take a third

The planner offered three resolutions. The ruling rejects the first, adopts the second, and records
why the third stays rejected.

### Rejected — add an eighteenth path for the fake implementor

The proposal was to add `QuickFiler.Test/Helper Classes/QfcThemeHelperTests.cs` to the write set and
give the fake a get-only auto-property. It is rejected for two independent reasons.

First, that path contains a space. Blast-radius derivation splits on whitespace, so the token would
be split into two fragments and the write claim would be silently lost rather than recorded. This
item would then be schedulable concurrently with any sibling that edits that file, which is the exact
failure the write-set discipline exists to prevent. A path containing a space can only be carried by
also stating it in prose for a human reader, which is a weaker guarantee than the extractor provides
for every other path.

Second, it widens the diff for a debug-log field. The interface change was never the goal; the value
was.

### ADOPTED — reach the value through the concrete controller, changing no interface

The write set returns to SIXTEEN paths. `QuickFiler/Interfaces/IQfcItemController.cs` is removed from
it and is now named in the exclusion paragraph, unbackticked, alongside
QuickFiler.Test/Helper Classes/QfcThemeHelperTests.cs.

The mechanism, verified against the tree by the orchestrator:

- `QfcItemController` is declared `internal partial class QfcItemController` across eleven parts, one
  of which is `QuickFiler/Controllers/QfcItemController.EventHandlers.cs` at line 25. That file is
  already a write-set path.
- `QfcItemGroup.ItemController` is declared `internal IQfcItemController ItemController` at
  `QuickFiler/Controllers/QfcItemGroup.cs` line 39, in the same assembly.
- `QuickFiler/Controllers/QfcFormController.Deactivate.cs` is in that same assembly and is already a
  write-set path, so a cast from the interface to the concrete internal type compiles there.
- The expression `_itemViewer.IsFolderDropDownOpen` is ALREADY USED in
  `QuickFiler/Controllers/QfcItemController.EventHandlers.cs`, at lines 200 and 225. The new member
  introduces no new dependency and no new field access; it names an expression the file already
  evaluates.

So the value is reachable with two edits, both inside the existing sixteen paths, and no interface
changes.

### Still rejected — drop `SelectorWasOpen` and amend the runbook

This was rejected in the round-1 adjudication and stays rejected. The per-item open state is what
distinguishes a cancel that hit an open selector from a cancel that was a no-op. The cancel runs
unconditionally on every item today, so a cancel count alone carries no such information, and
candidate 1's refute rule depends on the distinction.

## Replacement text

### Delta 3a-R — replaces Delta 3a in full

Revert the write set to sixteen paths. `QuickFiler/Interfaces/IQfcItemController.cs` is NOT a
write-set path. Every place in the plan that Delta 3a changed to say "seventeen write-set paths" —
the `## Write Set` introductory sentence, the P0-T14 acceptance, the P9-T10 acceptance, and the
structural self-check paragraph — reverts to "sixteen write-set paths".

Add both of these to the plan's "Explicitly not in the write set" paragraph, WITHOUT backticks,
matching the form that paragraph already uses:
QuickFiler/Interfaces/IQfcItemController.cs, QuickFiler.Test/Helper Classes/QfcThemeHelperTests.cs

Add this sentence to that same paragraph:

> QuickFiler/Interfaces/IQfcItemController.cs is excluded deliberately rather than by omission:
> preflight round 1 proposed adding a member to it, and the planner established that doing so breaks
> the compiled hand-written implementor FakeQfcItemController in
> QuickFiler.Test/Helper Classes/QfcThemeHelperTests.cs with CS0535, because the target framework has
> no default interface members. The adopted resolution reaches the same value through an internal
> member on the concrete controller instead, so neither file is edited.

The spec.md half of this revert has already been applied by the orchestrator. spec.md now states
sixteen paths, records the withdrawn seventeenth and why, folds the AC6 obligation into the
`QuickFiler/Controllers/QfcItemController.EventHandlers.cs` entry, and names both excluded files
unbackticked. Do not edit spec.md.

### Delta 3b-R — replaces Delta 3b in full

Replace the second sentence of [P1-T4] with:

> Add the pure method `internal static string FormatDeactivationDiagnostics(bool webView2Focused,
> bool activeFormIsNull, int groupCount)` returning a single interpolated line containing the labels
> `WebView2Focused=`, `ActiveFormNull=` and `Groups=`, and the pure method `internal static string
> FormatItemCancelDiagnostics(int itemNumber, bool selectorWasOpen)` returning a single line
> containing the labels `ItemNumber=` and `SelectorWasOpen=`. The `selectorWasOpen` value has no
> source on the item-controller INTERFACE, which declares `ItemNumber` and
> `CancelBreadcrumbSelector()` and no selector-open state and no viewer accessor. It is not obtained
> by changing that interface: adding a member there breaks the compiled hand-written implementor
> FakeQfcItemController in QuickFiler.Test/Helper Classes/QfcThemeHelperTests.cs with CS0535, and the
> target framework offers no default interface member. It is obtained instead through the concrete
> controller, which is internal to the same assembly as the deactivate handler. This task therefore
> also adds one internal get-only member to
> `QuickFiler/Controllers/QfcItemController.EventHandlers.cs`, reporting whether this item's
> breadcrumb selector is currently open by forwarding to the item viewer's existing
> `IsFolderDropDownOpen`; that expression is already evaluated in that same file at lines 200 and
> 225, so no new dependency is introduced. The deactivate handler reads it by casting the loop's
> interface-typed item controller to the concrete internal type and reports `SelectorWasOpen=` from
> the result. Both edits are observational: no caller other than the diagnostic reads the member, and
> neither changes control flow, so Phase 1 remains free of behavioural change.

### Delta 3c-R — replaces Delta 3c in full

Replace the acceptance sentence of [P1-T4] with:

> Acceptance: the file compiles, the existing catch block at lines 58-69 is unchanged in the diff, no
> `if`, `return`, `throw` or assignment other than the two log statements is added inside
> `ParkFocusAndCancelSelectors`, and the new internal member on the concrete item controller is
> declared and forwarded with no branching of its own. No file outside the sixteen write-set paths
> appears in the diff, and in particular neither QuickFiler/Interfaces/IQfcItemController.cs nor
> QuickFiler.Test/Helper Classes/QfcThemeHelperTests.cs is modified. The solution compiles under the
> P0-T8 command form, which is what proves both the cast and the forward are well typed. When the
> cast yields null, which cannot occur for any production item group but is reachable in principle,
> the diagnostic records the literal `SelectorWasOpen=unavailable` rather than a fabricated boolean,
> so the AC6 evidence never carries a value that was not observed.

## Sweep consequences of this revert

Re-run these, because the revert changes state the previous pass observed:

- P1-T14's permitted set was widened from seven paths to nine by the withdrawn Delta 3. Re-derive it.
  `QuickFiler/Controllers/QfcItemController.EventHandlers.cs` remains permitted, because P1-T4 still
  edits it. `QuickFiler/Interfaces/IQfcItemController.cs` is removed.
- P6-T1's "unchanged by this task" qualification stays as the planner wrote it, because P1-T4 still
  touches `QuickFiler/Controllers/QfcItemController.EventHandlers.cs`.
- Re-check every task that counts write-set paths or asserts that a file was not modified.
- Confirm the AC6 no-behavioural-change constraint still holds for Phase 1 under the revised
  mechanism.

## Everything else from round 1 stands

The fourteen other dispositions the planner reported are accepted as applied. Do not revisit them,
and do not revisit the items listed under "Items round 1 confirmed as already correct" in
evidence/other/preflight-round-1-delta.md. The sweep changes the planner made beyond the supplied
text — the line-count idiom propagation, the P1-T2 band re-derivation to 485 against the [480, 486]
band, the conditional fifth expect-fail row, the P7-T3 combined-total correction, and the removal of
the absolute worktree path from P0-T14, P1-T15 and P9-T9 — are all accepted and must be preserved.
