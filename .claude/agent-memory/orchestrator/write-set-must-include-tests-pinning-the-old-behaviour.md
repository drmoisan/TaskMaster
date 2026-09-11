---
name: write-set-must-include-tests-pinning-the-old-behaviour
description: A write set derived from the files the fix EDITS misses pre-existing tests asserting the behaviour an AC deliberately changes; find them at planning time, not at the final gate
metadata:
  type: feedback
---

When an acceptance criterion deliberately changes existing behaviour, the write set must include
every pre-existing test that asserts the old behaviour — not only the files the fix edits.

**Why:** On issue #796, AC4 changed the #680 search-leave latch so a mouse-opened breadcrumb popup
is no longer dismissed when the search box loses focus. The sixteen-path write set was derived
from the production and test files the fix would touch. It missed
`QuickFiler.Test/Controllers/QfcItemController.SearchDismissalTests.cs`, which arrived with the
original #680 fix (commit `660793e5`) and whose
`TextBoxSearchLeave_WhileDropDownOpen_RoutesExactlyOneCloseIntent` arranges an open drop-down with
no search-driven open and asserts exactly one close intent — precisely the state AC4 stops
dismissing. It surfaced only at the Phase 9 whole-assembly gate, after four phases were committed,
and forced a plan amendment plus a renumbered phase. Six preflight rounds did not catch it,
because every round reviewed the plan against the tree rather than simulating the delivered
behaviour change against the existing test population.

**How to apply:** At planning time, for each AC that changes behaviour, grep the test tree for the
API the change affects (here, `SetFolderDroppedDown`) and read every assertion over it, not just
the ones in files you already plan to touch. Add each such file to the write set with a
*deliberate update* task modelled on the "keep the name, keep the assertion, add one Arrange line"
pattern — never a deletion or a weakened assertion.

Two traps when remediating:

- Check the path is expressible as a write claim. Blast-radius derivation splits on whitespace, so
  a path containing a space is silently lost. This one had none; a different candidate on the same
  item was rejected for exactly that reason.
- Sequence the test update **before** the AC check-off task, or the criterion gets recorded as
  delivered while a red gate still pins the superseded contract.

Establish provenance with `git log --follow` before writing it up: if the file predates the branch
it is a derivation oversight, and calling it merge damage would be false. Related:
[[revert-plans-must-check-test-provenance]] and [[footprint-ac-forbids-onbranch-followup-promotion]].
