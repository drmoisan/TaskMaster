---
name: qfc-collection-defects-468
description: Issue #468 test-feasibility findings - MovedMails already populated by EmailFiler (so the MoveEmailsAsync param is redundant), log4net absent from QuickFiler.Test, ConcurrentDictionary ordering blocks a deterministic pre-fix test
metadata:
  type: project
---

Four non-obvious findings from the #468 (`QfcCollectionController`) test-feasibility research
(2026-08-24). Full artifact:
`docs/features/active/qfc-collection-controller-defects-468/research/test-harness-feasibility.md`.

1. **#469 defect 4 is probably "remove the parameter", not "populate it".** The undo stack handed to
   `MoveEmailsAsync` is `_globals.AF.MovedMails`, and that same stack IS already pushed to on the real
   move path (`QfcItemController.MoveMailAsync` enqueues an `EmailFiler`, which pushes at
   `UtilitiesCS/EmailIntelligence/EmailParsingSorting/EmailFiler.cs:188`). The potential document's
   worry that "undo-after-move is broken" is therefore not supported.
   **Why:** the resolution choice changes the owned-file footprint - removing the parameter forces an
   edit to `QuickFiler/Controllers/QfcFormController.EventHandlers.cs:225`, which #468's `issue.md`
   does NOT list as owned.
   **How to apply:** re-verify the EmailFiler push before recommending either resolution; do not
   assume the parameter is load-bearing just because it is on the interface.

2. **`QuickFiler.Test` has no log4net reference at all** - only a binding redirect in `app.config`.
   Any "assert exactly N log entries" acceptance criterion in a QuickFiler feature needs either a new
   package + csproj reference, or a delegate error-sink seam.
   **Why:** the file's own ratified precedent is a delegate seam (`_removeGroupByEntryId`,
   `QfcCollectionController.cs:1060-1074`), which is smaller than a `MemoryAppender` and avoids
   process-wide log4net state.
   **How to apply:** prefer an observable proxy first - e.g. `VerifyGet(x => x.Subject, Times.Never())`
   proves a double-dereference was removed without counting logs at all.

3. **A `ConcurrentDictionary`-ordering defect has no deterministic pre-fix red state.** Reference-type
   keys hash by runtime identity, so "index i resolves to a different group after a mutation" passes on
   some runs.
   **Why:** the determinism rule in `.claude/rules/general-unit-test.md` forbids shipping that test.
   **How to apply:** pair a deterministic structural guard (assert the backing field's declared type is
   order-preserving, in the style of `QuickFiler.Test/NoLiveFormInTestAssemblyTests.cs`) with a
   post-fix behavioural ordering test, and record the missing pre-fix red in the dossier.

4. **`UiThread.Init()` shows a real form** (`UtilitiesCS/Threading/UiThread.cs:54`), so
   `UiThread.Dispatcher` is permanently null in tests and any code path reaching
   `await UiThread.Dispatcher.InvokeAsync(...)` NREs.
   **Why:** that NRE is useful - it is the cheapest way to force a mid-method throw when testing
   try/finally hygiene.
   **How to apply:** never call `UiThread.Init()` from a test; treat the null dispatcher as a seam.

See also [[winforms-testability-epic-298]] for the ratified STA-control last-resort rule
(`*.StaTests.cs` + `[STATestClass]`), which applies to the one `TableLayoutPanel`-bound test here.
