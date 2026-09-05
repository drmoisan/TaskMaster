---
name: taskmaster-validator-memories-are-cross-repo
description: "SUPERSEDED IN PART - see the CORRECTION section at the end: validate_orchestration_artifacts is NOT on the feature-review agent tool surface but IS run by the orchestrator, so the canonical policy-audit template structure is mandatory from the first draft"
metadata:
  type: project
---

On the issue #244 cycle-2 re-audit (2026-07-06), before authoring fresh artifacts I checked whether the structural rules in [[policy-audit-validator-uses-full-template]], [[policy-audit-comparison-line-schema]], [[policy-audit-required-structure]], [[policy-audit-section7-row-label-parser]], [[policy-audit-numeric-new-code-coverage]], [[code-review-findings-table-header]], [[code-review-required-headings]], [[feature-audit-checkoff-heading-case]], and [[feature-audit-requires-summary-heading]] actually apply to this repository. They do not: `find . -iname "*validate*polic*audit*"` and `find . -iname "validate_orchestration*"` and `find . -iname "validate_evidence_locations.py"` all returned nothing in this TaskMaster checkout. Those memory entries explicitly name "mix-calculator" and reference `drm-copilot/scripts/dev_tools/` — a different repository (visible as an "Additional working directory" in this environment's session config), not TaskMaster.

TaskMaster's actual feature-review enforcement is two local hooks, both read directly in this cycle:
- `.claude/hooks/validate-feature-review-coverage.ps1` (SubagentStop): requires the agent's final output to advertise `policy-audit-path`/`code-review-path`/`feature-audit-path` (and optionally `remediation-inputs-path`) pointing at existing files in `docs/features/active/<feature>/<stem>.<timestamp>.md` sharing one feature-folder+timestamp; then, for each language with changed files detected from `artifacts/pr_context.summary.txt`'s `- <path> (+N/-N)` bullets (via `Get-ChangedLanguageSet`), it requires the policy-audit to (a) mention the language, (b) have at least one line mentioning that language AND a coverage keyword, (c) contain no scope-narrowing phrase (`informational only|context only|out of plan scope|out of scope|not applicable|N/A|UNVERIFIED`, case-insensitive) on that line, (d) contain PASS or FAIL on that line, and (e) if the canonical coverage artifact IS present and parseable and repo-wide/branch coverage is below 85%/75%, contain an explicit FAIL. There is no required-heading-set check anywhere in this hook.
- `.claude/hooks/enforce-evidence-locations.ps1` (referenced by the evidence-and-timestamp-conventions skill; a `validate_evidence_locations.py` PreToolUse-style scanner is NOT present in this repo — a manual `git diff --name-only <merge-base>..HEAD | grep -E "^artifacts/(baselines|qa|evidence|coverage)/"` is the working substitute).

**Why:** Chasing the cross-repo validator's exact heading/label requirements (`## Executive Summary` + `## 1`..`## 7` literal, 7-column findings-table header, `## Acceptance Criteria Check-off` lowercase, `### 1.2.1` bullet label parsing, etc.) is unnecessary effort in TaskMaster and risks over-fitting artifact prose to a tool that never runs here. The cycle-1 policy-audit for #244 (which used a similar structure to what this note now documents, not the cross-repo template) passed review and proceeded normally, confirming the local hook is the only gate that matters.

**How to apply:** In TaskMaster, write policy-audit/code-review/feature-audit artifacts to satisfy (1) the three-path-plus-optional-fourth advertisement contract, (2) the acceptance-criteria-tracking skill's check-off protocol, and (3) the coverage-row PASS/FAIL-with-no-narrowing-phrase rule in `validate-feature-review-coverage.ps1` — verified by grepping your own drafted policy-audit for any line where a C#/TypeScript/Python/PowerShell label token and a coverage keyword co-occur with a banned narrowing word (see the Python one-liner pattern used on #244: `label_pat` + `cov_pat` + `narrow_pat` regex intersection over the file's lines). Do not port the cross-repo heading/template requirements from the memories listed above into a TaskMaster artifact; they will not be enforced and add drafting overhead. If those memories resurface, re-verify with `find`/`grep` before trusting them, per this repo's own before-recommending-from-memory rule.

## CORRECTION (Issue #781, 2026-09-05): the validator IS reachable — via the ORCHESTRATOR

This entry's central claim was too strong and cost a rework cycle. On #781 the reviewer wrote a
policy audit using only the local hook's rules and skipped the canonical template structure,
citing this memory. The orchestrator then ran `validate_orchestration_artifacts` on the three
artifacts and the policy audit **FAILED** with exactly the errors the sibling memories predicted:
missing `TypeScript/PowerShell baseline|post-change coverage artifact:` checklist lines, missing
`Per-language comparison summary:`, missing `### 1.2.1 Per-Language Coverage Comparison`, and
"missing numeric baseline/post-change/new-code coverage" for every row of the wrong table.

**The correct model:** the validator and the MCP template asset are not on the *feature-review
agent's* tool surface, but they ARE on the *orchestrator's*. "I cannot call it" is not "it does
not run." Therefore [[policy-audit-required-structure]], [[policy-audit-comparison-line-schema]],
[[policy-audit-numeric-new-code-coverage]], and [[policy-audit-section7-row-label-parser]] are
LIVE for TaskMaster policy audits and must be applied on the first draft, not after a rejection.

**How to apply:** author every `policy-audit.*.md` against the canonical template structure from
the outset — `**Coverage Metrics by Language:**` with the exact 7-column header and LANGUAGE-only
rows, the full Coverage Evidence Checklist, `### 1.2.1` bullets, a `### 1.2.2` terminator heading,
sections 1-10, and both appendices. If the template asset is not on your tool surface, ask the
orchestrator to resolve it rather than improvising, and record the fallback as a method deviation.

**Row-label hazard confirmed on #781:** absent the `**Coverage Metrics by Language:**` marker the
validator bound to the *section 6 test-execution table* and reported its first-column labels
("Run", "Baseline full suite at merge base", "RED reproduced by reviewer", ...) as languages
missing numeric coverage. Fix: add the marker + canonical header table, and render test-run
inventories as BULLET LISTS, not tables. Keep only one table in the document whose first column is
`Language`. This extends [[policy-audit-section7-row-label-parser]] beyond section 7.
