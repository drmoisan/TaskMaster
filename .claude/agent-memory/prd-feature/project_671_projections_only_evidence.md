---
name: 671-projections-only-evidence
description: Maintainer decision on issue 671 (effective 2026-09-12) - commit Markdown evidence projections only; no new raw test-result XML/TRX or coverage XML in the repo; shapes every spec's Test Strategy and evidence ACs
metadata:
  type: project
---

By maintainer decision on issue 671, effective as of 2026-09-12, feature work commits **projections only**: numeric coverage figures and test-run summaries go into Markdown evidence artifacts under the feature folder's `evidence/<kind>/` tree, and the raw tool output (TRX, Cobertura XML, .coverage) is discarded, never added to the repository.

**Why:** Raw XML evidence bloated the repo and produced meaningless blast-radius entries; the maintainer ruled on #671 that the Markdown projection is the artifact of record.

**How to apply:** In every spec's Test Strategy and Acceptance Criteria, name Markdown projections (Timestamp / Command / EXIT_CODE / output summary) as the evidence, and add an AC that no `.xml`, `.trx` or `.coverage` file is added by the diff. Use fixed, non-timestamped filenames for projections that ACs must name, so no `<timestamp>` placeholder is needed inside a backticked path; the run timestamp goes in the artifact's `Timestamp:` field. Evidence paths must be written feature-relative and in full, e.g. `docs/features/active/<slug>/evidence/regression-testing/<name>.md`, never a bare repo-root `evidence/...` (a sibling item did that on 2026-09-12 and produced 44 meaningless blast-radius entries). When the caller's blast-radius rule says to backtick only the source files that will be written (as on #882, 2026-09-28), keep the evidence paths full and feature-relative but write them in plain text with no code span: they still name a fixed file the plan can assert, and the harvester does not register not-yet-existing evidence files as write targets. Related: [[backticked-paths-are-the-change-footprint]], [[full-bug-spec-only]].
