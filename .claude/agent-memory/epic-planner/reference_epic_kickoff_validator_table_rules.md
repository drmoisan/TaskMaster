---
name: epic-kickoff-validator-table-rules
description: The epic-kickoff MCP validator parses the first line after "## Feature Summary" as the table header and requires integer-only issue_num, so the epic-plan skill's own template shape can fail validation
metadata:
  type: reference
---

`mcp__drm-copilot__validate_orchestration_artifacts` with `artifact_type: "epic-kickoff"` enforces
structural rules the `epic-plan` skill template does not spell out. Three of them cost a
validate-fix-revalidate cycle each on the `build-ci-coverage-gate-fidelity` run (2026-08-11):

1. **The feature table must immediately follow the `## Feature Summary` heading.** The validator
   treats the first non-blank line after that heading as the header row. A sentence of prose there
   (for example "Nine issues, five features, three waves") is parsed as the header and fails with
   `feature table headers must be: issue_num | feature_folder | wave | complexity | plan-path`.
   Put any narrative sentence *before* the heading, not between the heading and the table.
2. **`issue_num` cells must be bare integers.** A feature that closes several issues cannot write
   `441 (+478)` or `512 (+492, +509, +522)`; those fail with
   `feature row N issue_num must be an integer`. Record the primary `issue_num` in the table and
   list the additional closed issues in a separate section.
3. **Trailing prose immediately after the table is parsed as another table row.** Leave a heading
   between the table and whatever follows.

Also keep the `## Invocation Prompt` paragraph on unwrapped lines. The template in the skill shows
it hard-wrapped; validation is easier to satisfy when the manifest path, the integration branch,
and the atomic-execution resume clause are each contiguous rather than split across a newline.

**How to apply:** write the kickoff artifact, validate it with the `epic-kickoff` type BEFORE
committing, and fix these first. Validate the mirrored copy under
`artifacts/orchestration/epic-kickoff-<slug>.md`; the committed copy at
`docs/features/epics/<slug>/epic-kickoff.md` must be byte-identical, so copy after validating.
Related: [[epic-planner-state-required-fields]], [[epic-plan-tooling-not-vendored]].
