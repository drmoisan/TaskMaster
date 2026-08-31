---
name: project-644-pa7-redaction-plan-seams
description: Issue #644 remediation cycle 1 (CR-1 + PA-7) — preflight R2 seams; untracked audit artifact enters main, name-status diff is blind to it, mandated replacement text conflicted with its own stated intent
metadata:
  type: project
---

Issue #644, remediation cycle 1, preflight round 2 delta (2026-08-30). Plan file: `docs/features/active/2026-08-27-qfc-unregister-navigation-count-mismatch-orphan-644/remediation-plan.2026-08-29T23-23.md`. Final shape: 18 tasks, P0×4 / P1×5 / P2×5 / P3×4.

Seams worth carrying:

- **"Historical record, not in scope" is false for an untracked artifact.** The plan declared a leaked line out of scope as a record of what the audit found at audit time. But `policy-audit.<ts>.md` was untracked at cycle entry and staged for the first time by the commit task, so the line entered `main` with the PR. An artifact's *authoring* date does not make its content historical; its *tracked* status at cycle entry does.
- **An anchored `git diff --name-status` cannot see the file the plan edits most.** The same untracked audit artifact is correctly absent from the diff even though two tasks edited it. Pair the diff with a positive porcelain clause asserting the `??` entry for that exact path. See [[diff-gates-need-a-commit-task]] for the converse failure.
- **Porcelain collapses a wholly-untracked directory.** A new `evidence/remediation-baseline/` lists as one entry ending in `/`, while `evidence/other/` and `evidence/qa-gates/` already hold tracked prior-cycle artifacts and list new files individually. Write the `??` clause to accept either form.
- **Spec checkbox invariant.** `spec.md` measured 21 checked / 5 unchecked / 26 total at cycle entry; `^- \[x\]` + `^- \[ \]` summed to the `\[[ xX]\]` total, proving no indented or upper-case-`X` checkbox. Stating both figures at entry and re-measuring at exit is how "no AC changes state" becomes falsifiable. Related: [[project_644_ac16_referral_revision_seams]].
- **A delta can specify a literal that contradicts its own stated intent.** The mandated replacement for a two-line bullet's first line ended mid-phrase, so the bullet did not read as the single sentence the delta said it would. Resolution taken: write the mandated literal exactly (it satisfies both acceptance clauses), soften only the descriptive prose so the plan asserts nothing false, and report the fragment to the caller with the one-phrase completion. Do not silently extend a literal the caller wrote "exactly" in front of.
- **Sibling observation, reported not planned:** `QfcCollectionControllerNavigationDigitsTests.cs` line 222 carries the same superseded "grown loop bound" mechanism inside a FluentAssertions `.BeEmpty(...)` because-message. CR-1 is comment-only by constraint, so it stays; it matches no acceptance token in the plan.
