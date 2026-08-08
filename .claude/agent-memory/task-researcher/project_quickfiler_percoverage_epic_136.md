---
name: quickfiler-percoverage-epic-136
description: Epic #136 per-file coverage research — committed Cobertura artifacts give exact per-file line rates without running tests; several QuickFiler "coverage gap" files are already >80%
metadata:
  type: project
---

For epic #136 (`quickfiler-per-file-coverage`, children F1–F16), per-file line coverage can be read
directly from Cobertura artifacts already committed under
`docs/features/active/<feature>/evidence/qa-gates/coverage-final.cobertura.xml` — no build or test
run needed. Grep the file for `filename="QuickFiler\Controllers\<File>.cs"` and read the
`line-rate` / `branch-rate` on the `<class>` element, then read the `<lines>` block for the exact
`hits="0"` line numbers.

**Why:** the epic mandates per-file (not per-assembly) evidence, and a child researcher who assumes
"low coverage" and plans a broad test suite will duplicate large amounts of existing test code. Two
F8 files were measured at 93.16% (`EfcHomeController.ExecuteMoves.cs`) and 97.59%
(`EfcHomeController.Metrics.cs`) — both already past the 80% target, with 8 and 1 uncovered lines
respectively. 2,502 lines of existing tests already cover this family.

**How to apply:** before proposing any tests for an epic-#136 child, locate a committed Cobertura
artifact, confirm its method line-sets align with the current file's line numbering (that is the
staleness check — the artifact comes from a sibling feature branch, not HEAD), and reconcile the
arithmetic: the tool double-counts (per-method `<lines>` + class-level `<lines>`), so
`line-rate = (total_entries - hits_zero_entries) / total_entries` across both blocks. If that
reconciles exactly, the parse is correct. Always still cite F1's harness as the authority for
acceptance evidence.

Related: [[feedback-exemption-audit-check-proven-techniques]], [[qfc-item-controller-227-r2-denial]].
