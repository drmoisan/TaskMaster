---
name: feature-audit-requires-summary-heading
description: feature-audit validator requires a literal `## Summary` heading in addition to the inventory/evaluation headings
metadata:
  type: project
---

The `validate_orchestration_artifacts` validator for `feature-audit` requires a literal `## Summary` heading, separate from `## Acceptance Criteria Inventory` and `## Acceptance Criteria Evaluation`.

**Why:** Cycle-5 feature-audit (#181) initially used `## Verdict` for the closing section and failed with "Feature audit missing required heading: ## Summary". Renaming to `## Summary` passed.

**How to apply:** When authoring a feature-audit, include all of: `## Acceptance Criteria Inventory`, `## Acceptance Criteria Evaluation`, `## Acceptance Criteria Check-off` (lowercase off, see [[feature-audit-checkoff-heading-case]]), and `## Summary`. Do not name the closing verdict section `## Verdict` alone.
