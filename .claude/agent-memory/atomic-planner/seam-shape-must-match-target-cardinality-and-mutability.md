---
name: seam-shape-must-match-target-cardinality-and-mutability
description: before writing a seam adapter task, check the target's cardinality (per-instance vs per-collection-element) and mutability (reassigned fields); pick stateless facade or accessor delegates accordingly
metadata:
  type: feedback
---

When planning an adapter/wrapper seam over WinForms or viewer members, resolve two questions BEFORE
writing the task text, because getting either wrong makes the stated default impossible to implement.

**Cardinality.** If the rewired call sites target a per-element member (`grp.ItemViewer` inside an
`_itemGroups.ForEach`, one viewer per group), a single instance-wrapping adapter FIELD cannot serve them.
Make the interface a **stateless facade** whose every member takes the target as its first parameter
(`void SetItemNumberText(ItemViewer viewer, string text)`), with a parameterless production default.
Prefer this over a `Func<TTarget, IAdapter>` per-instance factory: the facade is one field, one
construction, and no per-call allocation.

**Mutability.** If the wrapped field is REASSIGNED or nulled anywhere (page swaps, `Cleanup`), a
constructor-time captured reference goes stale and silently breaks "bit-identical in effect" plus any
cleanup-nulls-the-field test. Have the adapter resolve its target through **accessor delegates**
(`Func<TableLayoutPanel>`) supplied by the owner and invoked at call time; construct as
`new Surface(() => _itemTlp, () => _itemPanel, ...)`. Prefer accessors over rebuilding the adapter at each
assignment site — accessors are robust without the plan having to enumerate every reassignment.

**Why:** #454 preflight rejected an S1 seam that declared ONE `_viewerSurface` field "constructed over
`_itemTlp`/`_itemPanel`/`_itemTlpToMove`". Those are `TableLayoutPanel`/`Panel` fields while the viewer
facade adapts `ItemViewer` members, so the default did not even typecheck; and the TLP half was declared
`readonly` over a field reassigned at three sites.

**After narrowing a facade's site list, re-attribute every downstream task that names it.** Splitting one
seam into two (or pinning a facade to an explicit line-range list) silently orphans coverage tasks that
still say "forwards to the *other* surface". Decide attribution by asking **where the target comes from**,
not by which type appears in the body: a member whose target arrives as a METHOD PARAMETER needs no facade
member at all and is read directly off the parameter. In #454, `ScrollIntoView(ItemViewer item)` reads
`item.Top`/`item.Bottom` from its parameter and touches `_itemPanel` for everything else — so it belongs to
the TLP surface, not the viewer facade, even though `ItemViewer` is in its signature. Adding a member to
the facade means adding a coverage task for it too; fold it into the nearest existing adapter test rather
than inserting a task, so the phase does not need renumbering.

**How to apply:** grep the rewire site list for `<collection-element>.<Member>` patterns (cardinality
signal) and grep the wrapped field name for assignment (`_field =`) outside the constructor (mutability
signal). Then state the chosen shape AND the rejected alternative in the acceptance text so the executor
does not re-litigate it. Add one test that proves call-time resolution (re-point the accessor holder
between two calls) and one that proves statelessness (two different targets in one test).
