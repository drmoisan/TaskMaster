---
name: verify-brief-constraints-before-propagating
description: Delegation-brief "KEY CONSTRAINTS" can be factually wrong; have researchers verify each one against source instead of inheriting it, and correct the promoted issue.md when refuted
metadata:
  type: feedback
---

Treat the constraints in an epic-child delegation brief as **hypotheses to verify**, not facts. Put
each load-bearing constraint in front of a researcher as "confirm or refute against source", and when
one is refuted, correct `issue.md` and record the correction in `spec.md`.

**Why:** During F5 preparation of the `quickfiler-per-file-coverage` epic (#136, child #436), two of
the brief's five KEY CONSTRAINTS were false, and I had already copied both verbatim into the potential
entry and the promoted `issue.md`:

- "`IQfcDatamodel` is consumed by the collection controller (sibling F11)" — false.
  `QfcCollectionController.cs` (2,349 lines) has **zero** matches for `DataModel|Datamodel|_datamodel`.
  The real extra consumers were F2 (`QfcQueue.cs:476`) and F6
  (`QfcFormController.EventHandlers.cs:196`), reached indirectly via `IQfcHomeController.DataModel` —
  so they never appear in a grep for the interface name.
- "`QfcDatamodel.FrameBuilding.cs` interacts with WinForms layout" — false. Zero
  `System.Windows.Forms` references; `Frame` means `Deedle.Frame<int,string>`. The whole STA
  last-resort apparatus (dedicated `*.StaTests.cs`, runsettings, per-test justification) was scoped
  into the child for a file that has no UI at all.

Both survived my own reading of the brief and would have shipped into the plan. They were caught only
because the per-file researchers were told to verify rather than assume.

Related: a sibling's conclusion is also a hypothesis. The `IQfcDatamodel` agent **confirmed** the
first agent's "no cross-child contract note required" verdict but found its evidence base incomplete
(two missed consumers). Ask the later agent to *verify or refute*, never to inherit.

**How to apply:** When a brief hands you constraints about consumers, coupling, or framework
dependencies, phrase the research prompt as "confirm or correct this claim; report what you actually
find, including consumers the epic did not anticipate." Grep for indirect reach (property chains like
`IFoo.Bar`), not just the type name. When refuted, fix `issue.md` (your own child's file) and have the
correction written into `spec.md` as a cited note — do not edit `epic.md` or any sibling file. See
[[epic-child-plan-phase0-paths-are-stale]] for the related "inherited path is stale" failure.
