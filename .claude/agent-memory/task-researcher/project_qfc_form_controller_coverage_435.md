---
name: qfc-form-controller-coverage-435
description: "Issue #435 (epic #136 child F6) EventHandlers.cs/Actions.cs findings — RECONSTRUCTED after an accidental overwrite; re-verify before relying on it"
metadata:
  type: project
---

> **INTEGRITY NOTICE (2026-08-07).** This file was accidentally overwritten by the concurrent
> `QfcFormController.cs` / `QfcFormController.SetupDisposal.cs` researcher during the F6 research
> wave. The body below is reconstructed **solely from this memory's own `MEMORY.md` index line**;
> the original full text was not read before the overwrite and is not recoverable from this session.
> Treat every claim below as a pointer to re-verify against the source, not as verified fact. If the
> file is tracked in git, prefer `git show HEAD -- .claude/agent-memory/task-researcher/project_qfc_form_controller_coverage_435.md`
> to restore the original.

Issue #435 is child F6 of epic #136 (QuickFiler per-file 80% coverage). This memory belongs to the
researcher who covered `QuickFiler/Controllers/QfcFormController.EventHandlers.cs` and
`QuickFiler/Controllers/QfcFormController.Actions.cs`.

Claims preserved from the index line (each needs re-verification):

1. **`UndoConsumer`'s `while (!_undoQueue.IsCompleted || exit)` busy-spins forever** — the `|| exit`
   makes the loop condition true once `exit` is set, so the `_undoConsumerTask = null` reset after
   the loop is unreachable. Located in `QfcFormController.Actions.cs`.
2. **A seam test string-matches `Actions.cs` method signatures.** `QfcFormControllerSeamTests`
   reads the production source off disk and asserts on literal signature text, so renaming a
   `LoadItemsAsync` overload silently breaks that test.
3. **`_formViewer.Invoke(...)` and `SloStack.Serialize` need no seam** — both are reachable from a
   deterministic unit test as-is.

**Why:** epic #136 mandates per-file research and per-file atomic planning across concurrent
sibling researchers, so per-file findings have to survive between cycles.

**How to apply:** re-read `QfcFormController.Actions.cs` and `QfcFormControllerSeamTests.cs` to
confirm each claim before citing it in a plan.

Related: [[qfc-form-controller-setup-disposal-435]], [[qfc-explorer-controller-435]]
