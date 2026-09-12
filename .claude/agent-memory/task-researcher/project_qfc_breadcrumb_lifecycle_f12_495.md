---
name: qfc-breadcrumb-lifecycle-f12-495
description: "#495/epic #136 F12: one .cs file can hold several types and still emit ONE Cobertura class element; a 0/2 branch on `X() ?? throw` means the factory threw, not returned null"
metadata:
  type: project
---

Research for `QuickFiler/Viewers/BreadcrumbItemViewerLifecycleCoordinator.cs` (child F12, issue
#495) surfaced three measurement facts that are not derivable by reading the file and that cost real
time to establish.

**Fact 1 — a multi-type source file emits ONE Cobertura `<class>` element, not one per type.**
That file declares three types plus a delegate, yet the report carries a single `<class>` whose
class-level `<lines>` block spans all three. The epic's "union `<class>` elements sharing a
`filename`" harness rule is therefore a *may*, not a *must*: the absence of extra elements (a `<>c`
closure, a second type) is NOT evidence that those types went uninstrumented. Corollary for
scoping: the majority of a file's gap can live in a type whose name does not match the filename.

**Fact 2 — `condition-coverage="0% (0/2)"` on a `factory() ?? throw ...` line means the factory
*threw*, not that it returned null.** The exception escapes before either branch target is reached,
so neither outcome counts, while the multi-line sequence point still records `hits="1"` on the throw
expression's continuation lines. A sibling `?? throw` reached with a genuine `null` registers 2/2 —
use one of those as the positive control before concluding the instrumentation is broken.

**Fact 3 — the emitted `line-rate`/`branch-rate` can be reconstructed exactly as
(class-level + method-level) / (class-level + method-level).** On this file that reproduced
`0.939516` and `0.688073` to seven significant figures. If a recomputation does *not* reproduce the
emitted attribute that way, the class-level `<lines>` extraction is probably wrong — it is a cheap
self-check on any #441 recomputation.

**Why:** every F12/F13/F14 sibling has had to redo this measurement, and two of the three facts look
like tooling bugs until the mechanism is understood.

**How to apply:** when auditing any QuickFiler file for epic #136, (a) enumerate the file's *types*
before scoping, not just its name; (b) treat a 0/2 `?? throw` as "the argument-producing call threw"
and look for the test that throws; (c) use the reconstruction check above to validate the
recomputation. See also [[cobertura-line-double-count]],
[[cobertura-perfile-attribution-contract]], [[quickfiler-percoverage-epic-136]].
