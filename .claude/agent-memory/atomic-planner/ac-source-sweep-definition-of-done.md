---
name: ac-source-sweep-definition-of-done
description: Before finalizing a full-feature plan, sweep spec.md for EVERY checkbox list (AC1-ACn plus the ## Definition of Done list) and confirm every AC has at least one verification task
metadata:
  type: feedback
---

A `full-feature` plan's final AC-mapping task demands an evidence path per acceptance criterion, so two sweeps are
mandatory before the plan is handed to preflight:

1. **Every AC must be referenced by at least one earlier task.** An AC that no task names makes the final mapping task
   unsatisfiable. In #433 F7, AC22 (MSTest + Moq + FluentAssertions, Arrange-Act-Assert, per-test `<summary>`, new
   tests only in new files under the right folder) was the sole unreferenced criterion while every peer had a dedicated
   Phase-7 audit task; the fix was a new recorded convention-audit task with `SearchScope:` / `SearchPatterns:` /
   `SearchResult:`.
2. **`spec.md` checkbox lists other than AC1-ACn are also AC sources.** `spec.md` carries a `## Definition of Done`
   checkbox list after the numbered ACs. It is part of the AC source file, so the final task must check those boxes
   too and include them in the AC status summary counts. A `## Seeded Test Conditions` checkbox list is **not** an
   acceptance criterion — those are planning seeds and must be left untouched.

**Why:** both gaps were returned by `atomic-executor` preflight on #433 F7, costing a revision pass each.
**How to apply:** run the sweep while drafting the final phase, not after. Grep `spec.md` for `^- \[ \]` and for
`^## ` to enumerate every checkbox list, then cross-check each AC id against the plan body. Note that inserting a task
mid-phase forces renumbering all later IDs — see [[plan-validator-task-id-sequential-constraint]] — so do this sweep
before the validator run, not after.
