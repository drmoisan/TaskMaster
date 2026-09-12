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
staleness check — the artifact comes from a sibling feature branch, not HEAD), then **recompute
from the class-level `<lines>` block alone** — never trust the `<class>` `line-rate`/`branch-rate`
attributes. Open issue **#441** (`Invoke-MSTestWithCoverage` double-counts `<line>` nodes) makes
those attributes wrong in **both** directions: measured on F13, `BreadcrumbCollapsedSurfaceController`
reported `branch-rate=0.858974` against a recomputed 85.71%, while `BreadcrumbUiDispatcher` reported
`0.969697` (= 64/66) against a recomputed 35/36 = 97.22% — and 66 matches no count derivable from the
element's own children. Recompute as: denominator = count of `<line>` children of the **class-level**
`<lines>`; branch = sum of the `condition-coverage="x% (n/m)"` numerators over denominators.

Three further parse rules, each verified twice on F13:
- **Key on `filename=`, never on `<class name=>`.** A file with two top-level types emits ONE
  `<class>` named after only one of them (`BreadcrumbPopupPlacement.cs` → `…PlacementResult`;
  `BreadcrumbWebViewSurfaceFactory.cs` → `…BreadcrumbNavigationReadiness`). A name-keyed reader
  reports the other type as absent/0%.
- **Never sum the `<method>` blocks.** The `<methods>` collection can omit an entire type whose lines
  ARE present in the class-level `<lines>` (both files above). Summing methods undercounts
  `BreadcrumbPopupPlacement.cs` by 91.7%.
- Epic Directive A ("union multiple `<class>` elements per filename") is a **no-op** for this writer —
  there is exactly one `<class>` per `filename`, with lambdas and closures pre-merged.

Always still cite F1's harness as the authority for acceptance evidence.

Related: [[feedback-exemption-audit-check-proven-techniques]], [[qfc-item-controller-227-r2-denial]].
