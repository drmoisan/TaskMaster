---
name: 671-projections-only-evidence
description: Maintainer decision on issue 671 (effective 2026-09-12) - commit Markdown evidence projections only; no new raw test-result XML/TRX or coverage XML in the repo; shapes every spec's Test Strategy and evidence ACs
metadata:
  type: project
---

By maintainer decision on issue 671, effective as of 2026-09-12, feature work commits **projections only**: numeric coverage figures and test-run summaries go into Markdown evidence artifacts under the feature folder's `evidence/<kind>/` tree, and the raw tool output (TRX, Cobertura XML, .coverage) is discarded, never added to the repository.

**Why:** Raw XML evidence bloated the repo and produced meaningless blast-radius entries; the maintainer ruled on #671 that the Markdown projection is the artifact of record.

**How to apply:** In every spec's Test Strategy and Acceptance Criteria, name Markdown projections (Timestamp / Command / EXIT_CODE / output summary) as the evidence, and add an AC that no `.xml`, `.trx` or `.coverage` file is added by the diff. Use fixed, non-timestamped filenames for projections that ACs must name, so no `<timestamp>` placeholder is needed inside a backticked path; the run timestamp goes in the artifact's `Timestamp:` field. Evidence paths must be written feature-relative and in full, e.g. `docs/features/active/<slug>/evidence/regression-testing/<name>.md`, never a bare repo-root `evidence/...` (a sibling item did that on 2026-09-12 and produced 44 meaningless blast-radius entries). Related: [[backticked-paths-are-the-change-footprint]], [[full-bug-spec-only]].

**Collision with the zero-digit AC rule (seen on #927, 2026-09-28).** A full feature-relative evidence path such as `docs/features/active/2026-09-28-<slug>-927/evidence/...` contains standalone integer tokens (the folder date and the issue number), so putting it inside a `- [ ]` line trips the numeric-derivation validator described in [[ac-gates-verify-satisfiability]] item 7. Resolution: list every artifact with its full backticked path in the Test Strategy evidence list, and have each AC line name the artifact by evidence kind plus file stem in plain prose ("recorded in the qa-gates artifact guard-post-sweep-run"), with one sentence in the top blockquote stating that convention. Also avoid "research section 3.4"-style citations inside AC lines for the same reason; write "the research record's legacy-token section". Verify with a grep for `^- \[ \] .*\b\d+\b` returning nothing.
