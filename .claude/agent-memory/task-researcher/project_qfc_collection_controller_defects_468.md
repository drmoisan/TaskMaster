---
name: qfc-collection-controller-defects-468
description: Issue #468 feature research (2026-08-24) — two promoted-document claims that are FALSE against source, plus the owned-file-set boundary that blocks the #474-2 fix
metadata:
  type: project
---

Research for feature `qfc-collection-controller-defects` (issues #286/#468/#469/#470/#471/#473/#474)
found three things a future reader would otherwise re-derive or get wrong by trusting the promoted
potential documents.

**1. The #474 document's core premise is false against source.** It says
`QuickFiler.Controllers.IQfcFormController` and `QuickFiler.Interfaces.IFilerFormController` are
unrelated siblings and "neither is a superset of the other." In fact `IQfcFormController` DERIVES
from `IFilerFormController`. That makes retyping the `_parent` field the cheap fix (2 owned files)
rather than the expensive one. Also: there are THREE types named `IQfcFormController` in this repo
(`Controllers/`, `Interfaces/`, and a non-compiled `Notes/notes_interfaces.cs`); only the
`Controllers` one has an implementer.

**2. The #469 defect-4 "undo record silently dropped" hypothesis is wrong; undo is NOT broken.**
`MoveEmailsAsync` really does ignore its `SloStack<IMovedMailInfo>` parameter, but the caller passes
`_globals.AF.MovedMails` — the same global instance that `EmailFiler.PushToUndoStack` writes to
several layers down the move path. The parameter is redundant, not load-bearing. The document said
this triage could raise severity to High; it does not.

**3. #474 defect 2 (MessageBox in the `ReadyForMove` getter) cannot be fully fixed inside the
declared owned file set.** Its only consumer is `QfcFormController.EventHandlers.cs`, which is not
owned, and the dialog is the only user feedback on that path — so "let the caller present the UI"
requires a scope extension. The in-scope answer is a behaviour-preserving split (pure predicate plus
thin UI wrapper), not relocation.

**Why:** All seven promoted documents' line numbers matched the source exactly, which makes them
feel authoritative; the two wrong claims above are analysis conclusions, not line citations, and they
did not survive verification. Trusting them would have produced a much larger and partly
out-of-bounds change.

**How to apply:** When planning or reviewing any of these seven issues, verify interface inheritance
and cross-layer data flow against source before accepting a promoted document's root-cause
narrative. Related: [[feedback-exemption-audit-check-proven-techniques]].

Secondary facts worth keeping: `QfcCollectionController` carries `[ExcludeFromCodeCoverage]`, so
#468's "inflates the coverage denominator" rationale does not currently hold. And `KbdActions`'
`IEnumerable` constructor performs NO duplicate check while both `Add` overloads throw — that
asymmetry is why issue #444's duplicate `Keys.Down` is silent rather than an exception.
