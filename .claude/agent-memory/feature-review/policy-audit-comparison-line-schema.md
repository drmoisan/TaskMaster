---
name: policy-audit-comparison-line-schema
description: Per-language coverage comparison bullets under `### 1.2.1 Per-Language Coverage Comparison` need exact `Baseline:` `Post-change:` `Change:` `Disposition:` `Evidence:` labels (and `New/changed-code coverage:` when a numeric value is in the row)
metadata:
  type: feedback
---

The `validate_policy_audit_artifact.py` validator (in `drm-copilot/scripts/dev_tools/`) parses bullets inside the `### 1.2.1 Per-Language Coverage Comparison` heading with `line[2:].partition(":")` and keys them by the lowercase first token. Each bullet must additionally contain these labels with a numeric percentage immediately after:

- `Baseline:` followed by `\d+(\.\d+)?%`
- `Post-change:` followed by `\d+(\.\d+)?%`
- `Change:` (any text — required keyword)
- `Disposition:` followed by `PASS|FAIL|N/A|INCOMPLETE|BLOCKED`
- `New/changed-code coverage:` followed by `\d+(\.\d+)?%` (only when the coverage-metrics table row's `New Code Coverage` column is non-N/A)
- `Evidence:` (any text — required keyword)

**Why:** the validator silently rejects bullets without these exact-labelled fragments and emits messages like "Policy audit comparison line missing explicit change text for Python." Discovered while validating the v2 feature-review for mix-pipeline-gui (2026-05-28).

**How to apply:** when authoring `policy-audit.*.md`, format each language bullet as `- <Language>: Baseline: <X>% line / <Y>% branch (...). Post-change: <X>% line / <Y>% branch (...). Change: <text>. New/changed-code coverage: <Z>% line / <W>% branch. Disposition: PASS. Evidence: <paths>.`. Use `- TypeScript: Baseline: N/A. Post-change: N/A. Change: N/A. Disposition: N/A. Evidence: N/A — no TypeScript files changed on this branch.` for unused languages.

**Confirmed on issue #197 R4 (2026-06-13):** the safest approach is to copy the prior passing cycle's exact 1.2.1 bullet wording and only swap the numbers. Two concrete gotchas observed:
- A bullet written as `Baseline: 59.03% lines -> Post-change: 71.65% lines. Change: +12.62 pp lines.` (em-dash-arrow joining Baseline and Post-change on one segment, `pp` unit on Change) FAILED with `Policy audit missing per-language comparison line for C#`. Rewriting to the prior-cycle form `Baseline: 59.03% lines (38,820/65,768) -> Post-change: 71.65% lines (37,019/51,665). Change: +12.62% lines (...).` PASSED. Use `%` (not `pp`) and the parenthetical covered/valid counts.
- `New/changed-code coverage: N/A - no new executable production code (...)` is ACCEPTED for an attribute/config/doc-only C# change (no numeric percent required) — the numeric-percent requirement in [[policy-audit-numeric-new-code-coverage]] applies only when the coverage-metrics table row's New Code Coverage cell is non-N/A.

**Confirmed on issue #791 (2026-09-06), two more exact-shape gotchas, each one rejection cycle:**

- The percent must be **immediately** followed by the sentence-ending period.
  `New/changed-code coverage: 90.8% lines (119/131 executable changed lines covered, 0 regressions).`
  FAILED with `missing numeric new/changed-code coverage for C#`; rewriting to
  `New/changed-code coverage: 90.8%.` PASSED. Move any parenthetical or unit word into the `Evidence:`
  clause or the prose paragraph below the bullets.
- For a zero-file language the bullet must be the bare #781 five-field form and must **omit** the
  `New/changed-code coverage:` field entirely:
  `- PowerShell: Baseline: N/A. Post-change: N/A. Change: N/A. Disposition: N/A. Evidence: N/A - zero PowerShell files changed on this branch.`
  Writing `Baseline: N/A - out of scope. ... New/changed-code coverage: N/A - out of scope.` FAILED
  with `comparison line missing numeric baseline, post-change, and new/changed-code coverage`. This
  form is also safer against the local coverage hook, because it drops `out of scope` from the bullet.
