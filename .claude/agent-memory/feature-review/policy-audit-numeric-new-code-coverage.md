---
name: policy-audit-numeric-new-code-coverage
description: policy-audit and feature-audit validators reject prose in the new/changed-code coverage field and require a literal numeric percent; feature-audit also needs a `## Scope and Baseline` heading
metadata:
  type: feedback
---

The THIS-repo orchestration validators (`validate_orchestration_artifacts`) enforce two things that prose-style writeups fail:

1. **policy-audit** — the per-language coverage line under `### 1.2.1` AND the coverage table `New Code Coverage` cell must contain a literal numeric percent for `New/changed-code coverage:` (e.g. `100%` or `91.1%`). Descriptive text like "seam/hook lines exercised (+33 covered)" fails with: `Policy audit missing numeric new/changed-code coverage for C#` / `comparison line missing numeric new/changed-code coverage`.

2. **feature-audit** — requires a literal `## Scope and Baseline` heading (in addition to `## Acceptance Criteria Inventory`, `## Acceptance Criteria Evaluation`, `## Acceptance Criteria Check-off`, `## Summary`). Omitting it fails with: `Feature audit missing required heading: ## Scope and Baseline`.

**Why:** caught on the issue #181 cycle-6 reaudit. When a remediation cycle only adds small seam/parameter lines, there's a temptation to describe coverage qualitatively; the validator will not accept that — state a numeric (100% when every added line is exercised by the converted tests is defensible and verifiable from the covered-line delta).

**How to apply:** when writing these artifacts, always put a numeric percent in the new/changed-code coverage field, and always include the `## Scope and Baseline` section in feature-audit. See [[policy-audit-required-structure]], [[policy-audit-comparison-line-schema]], [[feature-audit-checkoff-heading-case]], [[feature-audit-requires-summary-heading]].
