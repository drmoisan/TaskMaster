---
name: cross-child-annotation-fanin-debt
description: Parallel per-file-opt-in annotation epics fan in NEW compiler diagnostics on the integrated tree that no scope-locked child can fix; reserve a capstone integrated-tree gate
metadata:
  type: project
---

When an epic parallelizes per-file opt-in annotation work (e.g. per-file `#nullable enable`) across
sibling child features that own disjoint file clusters within the SAME compilation unit, expect
**cross-child fan-in debt** on the integration tree: children each verify clean on their own
pre-fan-in branch, but annotation propagation across the integrated set emits NEW diagnostics in
already-opted-in files owned by other children.

**Concrete instance (utilitiescs-nullable-remediation, 2026-07-19):** #372's post-merge
whole-project scoped pragma gate showed 76 CS86xx across cross-child EmailIntelligence files at
integration tip 0b000511; tip aa154796 already carried 15 CS86xx BEFORE #372 merged, in
sibling-owned files (EmailParsingSorting, SubjectMap, People, Evaluation). No child could remediate
under its per-cluster scope lock.

**Why:** each child's per-file pragma gate only sees its own opted-in files on its own branch; the
new diagnostics only appear once sibling annotations coexist on the integrated tree. A green
per-child gate is NOT evidence of a green integrated-tree gate.

**How to apply:** when decomposing such an epic, plan a wave-N+1 capstone whose job is a FULL
integrated-tree gate (e.g. `/t:Rebuild /p:TreatWarningsAsErrors=true`) that remediates fan-in debt.
Carry each child's residual-diagnostic observation into `features[].capstone_inputs` as
`blocking_for_capstone_gate`. Expect the capstone to accumulate MULTIPLE integrated-tree build debts
(this epic carried two: SVGControl CS0649 + the cross-child CS86xx fan-in). Do not treat a child's
clean per-file gate as satisfying the epic-level gate. Related: [[feedback_merged_child_worktree_still_locked_defer_removal]].
