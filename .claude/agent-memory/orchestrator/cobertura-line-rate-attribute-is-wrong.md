---
name: cobertura-line-rate-attribute-is-wrong
description: Never read the Cobertura <class> line-rate attribute in this repo - two separate defects (#441, #478) make it wrong; recompute per-file rates from deduplicated class-level <line> nodes
metadata:
  type: project
---

The per-file `line-rate` attribute in this repository's Cobertura reports is **not trustworthy**.
Two independent defects corrupt it:

- **#441** — `Get-CoberturaCoverageSummary` (`scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1:98`)
  selects `.//lines/line`, the descendant axis. Each `<class>` carries its `<line>` nodes twice,
  once under each `<method>` and once as a class-level rollup, so every line is double-counted.
  Confirmed: `lines-valid="110849"` equals the raw `<line number=` count, not the distinct count.
- **#478** — `Merge-CoberturaClassesByFilename` (`:167`) unions the class-level `<lines>` correctly
  (`:217-268`, max hits per line) but never merges the non-primary members' `<methods>` subtrees,
  then recomputes over the same descendant axis. The emitted rate blends two denominators.
  Proven arithmetically: `QfcHomeController.Iteration.cs` is truly 45/56 = 80.36%; the attribute
  reads 0.8625 = exactly 69/80.

Fixing #441's axis alone does **not** fix #478. Schedule them together.

**Correct recipe:** union `./lines/line` — class-level only, max hits per `@number` — and recompute.

**Why this matters:** epic #136 gates all fifteen children on per-file line rates. An uncorrected
attribute produces false passes and false failures across the whole epic.

**How to apply:**
- Per-file attribution **does** survive a partial-class split — Cobertura emits one `<class>` per
  `(type, source file)` pair, verified against `QfcItemController`'s 10 partials. Splitting a file
  does not make its coverage unverifiable.
- Branch data **is** fully emitted (`branch`, `condition-coverage`, `<conditions>`), so the 75%
  branch gate is enforceable. A file with no branching lines yields `0/0` and must report **N/A**,
  never 0%.
- `ci.yml` produces **no** Cobertura at all, so any repository-wide figure is produced locally, and
  CI does not filter `.claude/worktrees` from its assembly enumeration.
- **Like-for-like trap:** feature #424's committed *baseline* artifact is raw (`line-rate=0.7019`,
  no `<sources>`) while its *final* is post-processed (`0.856453`). Epic #136 imported the raw
  70.19% as its authoritative repo-wide baseline, so a "retain or improve" comparison against a
  post-processed figure is not like-for-like. Check which form a baseline is before comparing.

Related: [[feature-review-coverage-85-floor-trap]], [[feedback_repowide_coverage_run_full_suite]].
