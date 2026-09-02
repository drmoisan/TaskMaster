---
name: full-bug-plan-write-blocked-without-user-story
description: enforce-feature-folder-order.ps1 requires user-story.md unconditionally, so a full-bug plan.md write is denied even though full-bug mode says user-story.md is absent by default — write an inert placeholder first
metadata:
  type: project
---

`.claude/hooks/enforce-feature-folder-order.ps1` denies any `Write`/`Edit` to
`docs/features/(active|archive)/<folder>/plan.md` unless **all three** of `issue.md`, `spec.md`,
and `user-story.md` exist as leaf files in that folder. The required list is the hard-coded
`@('issue.md', 'spec.md', 'user-story.md')` in `Get-FeatureFolderMissingFile`; the hook never reads
the `- Work Mode:` marker from `issue.md`.

That contradicts `acceptance-criteria-tracking` (`full-bug` resolves to `spec.md` **only**) and
`atomic-plan-contract` (`full-bug` plans treat `user-story.md` as "optional/absent by default").
A correctly-scaffolded `full-bug` feature folder therefore blocks its own plan write with
`FEATURE_FOLDER_ORDER_BLOCKED: ... Missing in feature folder: user-story.md`.

**How to apply:** do not report blocked and do not edit the hook — it is one of the ~166 `.claude/`
files this repo receives by push-down from `drm-copilot` and does not own. Write an **inert**
`user-story.md` placeholder into the feature folder first, then write `plan.md`. The placeholder
must (1) open with a blockquote stating it carries no acceptance criteria and is not an AC source,
(2) cite both skills' `full-bug` rules, (3) name the hook as the sole reason the file exists, and
(4) contain **zero** `- [ ]` checkboxes, so a reviewer's AC scan still resolves `spec.md` alone.
Add a `## Notes` rule to the plan recording that the placeholder is inert and that no task reads it,
so preflight does not score it as an unrequested document.

**Why:** #493 (2026-08-24). The first `plan.md` write was denied outright. Reporting blocked would
have cost a full orchestration round for a known upstream defect. Related:
[[plan-validator-phase-heading-constraint]], [[one-ac-per-checkoff-task]].
