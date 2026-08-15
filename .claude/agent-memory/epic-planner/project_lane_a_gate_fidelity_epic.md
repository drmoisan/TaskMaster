---
name: lane-a-gate-fidelity-epic
description: The build-ci-coverage-gate-fidelity epic is Lane A / Flight 0 of the blocked parallel-bug run; it must land before the other 59 open bugs can be certified
metadata:
  type: project
---

Epic `build-ci-coverage-gate-fidelity` (planned 2026-08-10, integration branch
`epic/build-ci-coverage-gate-fidelity-integration`) delivers Lane A of
`docs/research/2026-08-10-parallel-bug-flighting-and-surface-blockers.md` as a conventional epic.

**Why:** The `/parallel-plan` surface is hard-blocked (missing `compute_cohorts`, missing
`config/blast-radius.json` — see the user-level memory `parallel-surface-not-ported-taskmaster`).
§9 of that research document names `/epic-plan` on Lane A as the fallback available today. Lane A
is Flight 0: it must run first and alone, because the repository's coverage gates measure an
inflated denominator and the nullable gate cannot fail, so running any of the other 59 open bugs
first would certify them against gates that cannot fail.

**How to apply:**
- Nine issues, five features, three waves. Wave 0: 441 (+478), 512 (+492, +509, +522), 394.
  Wave 1: 457. Wave 2: 494.
- **Issue 513 is excluded and cannot be fixed in TaskMaster.** `collect_pr_context`'s
  classification step lives at `extensions/drm-copilot/src/lib/pr-context/collector-output.ts` in
  the `drm-copilot` repository. File it upstream; do not re-investigate whether TaskMaster can
  close it.
- Nullable *debt* burn-down (~195-220 `CS86xx` in `UtilitiesCS.csproj`) is deliberately a
  follow-on epic. Issue 492 separates "make the gate report truthfully" from "fix what it
  reports"; only the first is in this epic.
- Non-DAG coordination risk: feature 441 changes every coverage figure in the repository, which
  invalidates the committed baselines that the 21 unmerged branches of the QuickFiler per-file
  coverage epic (#136) gate on. No file conflict, but decide merge order before landing either
  epic on `main`.

Related: [[governance-doc-edits-need-execution-authorization]],
[[preexisting-issues-skip-potential-to-issue]].
