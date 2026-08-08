---
name: verify-caller-supplied-citation-corrections
description: Re-read the source before applying a caller/preflight-supplied "correct this line citation" delta — preflight citations are themselves sometimes off by one, and applying one blindly writes a new defect into an already-validated plan
metadata:
  type: feedback
---

When a revision prompt hands over a list of "minor citation corrections", treat each supplied line
number as a claim to verify, not an instruction to transcribe. Read the cited file region first.
Apply the correction only when the source agrees; when the source shows the plan was already right,
leave the citation alone, make it unambiguous instead, and report the discrepancy to the caller.

**Why:** In the #452 F9 plan revision the caller listed four non-blocking citation fixes. Three were
correct (`epic.md:520` for the `interface-only / not-measured` token; `EfcViewer.cs:20` for the
attribute vs `:21` for the type declaration). One was not: the prompt said the `[DoNotParallelize]`
precedent in `QuickFiler.Test/Helper Classes/ViewerQueueStaticWrapperTests.cs` is at `:10` with
`[TestClass]` at `:11`. Direct read shows `:11` is `[DoNotParallelize]` and `:12` is `[TestClass]` —
the plan's existing `:11` was already correct, and "fixing" it would have introduced an error into a
file the MCP validator had already passed.

**How to apply:**
- Budget one `Read` per supplied citation. These deltas are cheap to check and expensive to get wrong,
  because a targeted revision is not re-validated as carefully as a fresh plan.
- When the plan is already correct but the citation was ambiguous enough to mislead preflight, resolve
  the ambiguity rather than reverting: name what is at the cited line
  (``:11`` — verified that `:11` is the `[DoNotParallelize]` attribute and `:12` the `[TestClass]`).
  That prevents the same false correction on the next pass.
- Same rule for any cross-reference the revision text tells you to add. Verify the target task's
  subject matches. In the same revision a reworded bullet needed a reconciliation cross-ref; the
  natural-looking `P7-T10` is the AC10 `#439` characterization task, not the AC9 coverage comparison
  (`P7-T5`).
- Report corrections-not-applied explicitly in the final message so the caller can reconcile.

See also [[verify-line-spans-and-computed-literals]],
[[stale-build-output-is-not-evidence-of-existence]],
[[plan-validator-task-id-sequential-constraint]].
