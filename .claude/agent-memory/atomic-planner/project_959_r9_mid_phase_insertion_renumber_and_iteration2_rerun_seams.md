---
name: project-959-r9-mid-phase-insertion-renumber-and-iteration2-rerun-seams
description: Inserting tasks into a partially executed final phase (#959 rev 1.9): collision-free renumbering with a temporary token, an orphan stop record under the old task number, ITERATION-2 re-runs that keep the earlier artifact name, and a doc-comment-only widening gated by diff identity instead of a regression test
metadata:
  type: project
---

Seams found while widening an approved, partially executed plan in place (#959 revision 1.9: three tasks inserted before the first unchecked Phase 6 task, former P6-T13..T43 renumbered +3).

**Why:** a maintainer directive absorbed a related doc-comment defect after the final toolchain pass had run and been checked off; the plan had 89 cross-references to the unchecked tasks plus lowercase artifact names tied to task IDs.

**How to apply:**
- Renumber with the Edit tool in two steps: every `P6-Tnn` to `P6-Txmm` (replace_all, any order, no collisions because the targets carry the `x`), then one `P6-Tx` to `P6-T` collapse; repeat for the lowercase artifact-name forms (`p6-t13-identity-and-sweep` and the like). Verify with a `-o` Grep of the task lines (gap-free sequence) and the same reference regex count before and after (89 = 89). Do all renumbering BEFORE inserting new text that uses the new numbers.
- Renumber the revision log and self-review history too, and record the mapping (new = old + 3) in the new revision note; otherwise an ID in the file names two tasks.
- A stop record may already exist on disk under the OLD task number for an unchecked task (hook refusal at P6-T13 wrote `p6-t13-identity-and-sweep.<TS>.md`). Name it in the artifact-filenames convention as an orphan that matches no glob, and have the renumbered task write its new name at `ITERATION: 2`; insert that filename only after the renumbering so replace_all does not rewrite it.
- A re-run of an executed gate over an edited tree (footprint after the edit) should write the EARLIER task's artifact name with `ITERATION: 2` so every later `p6-t12 shows ...` precondition resolves to the post-edit record under the existing highest-ITERATION glob rule; state this in the convention and add `WRITTEN-BY:`.
- A doc-comment-only widening needs no failing test (say so in a PD and the revision note); gate it with a single-line backslash-free Grep token (present once after, zero before) plus a zero-count search for the old phrase, and add removed/added-line labels to the anchored identity payload so the diff gate fails on any other change. Check the new comment text against every census token with whitespace stripped and keep the line count equal so LINES/AC23 values stay as last recorded.
- When a hooked payload is relayed by the coordinator under a standing approval, append new lines but never rename its identifiers; say so in the heading.
- The recorded toolchain final pass predates the edit; state the disposition (read-only `csharpier check` is the only step a summary-prose change can fail; rebuilds and tests not repeated) and name the loop-rule restart as the reversal so the orchestrator can override without a task-text change.
