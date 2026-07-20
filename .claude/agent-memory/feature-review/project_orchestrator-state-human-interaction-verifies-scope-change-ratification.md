---
name: orchestrator-state-human-interaction-verifies-scope-change-ratification
description: For a coverage-gap SCOPE_CHANGE disposition, cross-check the feature-folder evidence narrative against artifacts/orchestration/orchestrator-state.json's human_interaction.requirements block before accepting the ratification as adequate
metadata:
  type: project
---

On issue #392's remediation cycle 1 (R4 re-audit), the R2 disposition
(`evidence/qa-gates/coverage-disposition-decision.<ts>.md`) claimed a maintainer `scope_change`
ratification for a pre-existing `QuickFiler` package-wide coverage gap, citing open GitHub issue
#136 and the `#328` `StoreWrapper` precedent. Rather than accepting the evidence file's narrative at
face value, I independently read `artifacts/orchestration/orchestrator-state.json`'s
`human_interaction.requirements` array and confirmed a well-formed entry: `response: "scope_change"`
(valid enum member per [[project_taskmaster-validator-memories-are-cross-repo]]'s companion rule
`.claude/rules/orchestrator-state.md`), non-empty `resolution` text citing the same issue number and
precedent, and a `resolved_at` timestamp. This is what elevates a sub-floor coverage finding from
"unratified FAIL, remediation required" (cycle 1's original verdict) to "ratified, non-blocking
exception" (cycle-1-re-audit's verdict) — matching the `#328` `StoreWrapper` pattern exactly.

**Why:** A feature-folder Markdown evidence file is authored by the same executor/orchestrator being
reviewed and could assert a ratification that was never actually recorded in the checkpoint. The
`orchestrator-state.json` `human_interaction` block is the authoritative, schema-checked record (per
`.claude/rules/orchestrator-state.md`'s invariants: non-empty `requirements`, `response` in
`{scope_change, exception, halt}`, and `runbook_path` required only for `exception`). Treat the
Markdown evidence narrative as a claim and the checkpoint JSON as the verification.

**How to apply:** Whenever a re-audit evidence file asserts a maintainer ratification/exemption for
an open coverage or policy gap, read `artifacts/orchestration/orchestrator-state.json` directly (it
is gitignored/session-local, so it will not appear in the branch diff) and confirm the
`human_interaction` entry exists, matches the claimed disposition, and is shape-valid before
downgrading a FAIL from blocking to non-blocking in the policy audit.
