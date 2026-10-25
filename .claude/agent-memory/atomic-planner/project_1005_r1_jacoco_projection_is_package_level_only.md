---
name: project-1005-r1-jacoco-projection-is-package-level-only
description: TaskMaster's committed JaCoCo projection holds package LINE/BRANCH counters only; per-file or per-member coverage ACs must read the Cobertura document
metadata:
  type: project
---

`ConvertTo-JacocoPackageProjection` (scripts/vscode/Invoke-MSTestWithCoverage.Projection.ps1) emits package-level LINE and BRANCH counters only, no class/method/sourcefile/line data. An AC that says a file or member reaches N% "read from the committed coverage projection" is unsatisfiable (#1005 preflight R1 substantive defect).

**Why:** CLAUDE.md permits committing only the projection, so authors reach for it as the source; it cannot carry per-file figures.

**How to apply:** word per-file/per-member coverage ACs as "read from the post-processed Cobertura document and recorded in the committed coverage-comparison evidence"; amend the spec before P0 copies it. Also: never leave a `PREFLIGHT:` line in the plan body. See [[project-1005-r0-settings-store-seam-and-probe-seams]].
