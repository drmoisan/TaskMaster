---
name: 1002-review-residuals
description: '#1002 (EFC descendant tree, trash preselect, QFC nesting) review 2026-10-22T10-39 REMEDIATION_REQUIRED 26/26 AC, 4 blocking (3 autonomous, 1 human); aria-selected with no CSS; modified-file floor split; hook sim now -InputText with last_assistant_message'
metadata:
  type: project
---

Full-bug review, head e96e65302, base 8e4ad1ecb, session cwd = review tree (no mirror). All 26 AC PASS (pre-checked by executor), verdict REMEDIATION_REQUIRED on findings outside AC text. All three artifacts validated first try with `validate_orchestration_artifacts` using the #985 policy-audit shape (Template Provenance, 7-col metrics table incl. a JavaScript row, checklist bullets, 1.2.1 bullets, 1.2.2 verdict paragraphs).

**Reusable points:**
- A renderer that marks selection by attribute (`aria-selected="true"`) needs a matching CSS rule; grep the asset CSS for the attribute. Here none existed, so the spec's manual step "confirm it is highlighted" could not pass -> blocking autonomous (B-4).
- Modified-file floor split that worked: coverage LOWERED by the diff (caller removed -> unchanged lines lost hits) = autonomous; pre-existing residue in MessageBox/WebView2 wiring = human_decision_required (CLAUDE.md UT2 exemption needs maintainer ratification); trivial `?? throw` ctor guards at 50% branch = autonomous. Compute the max attainable % without the wiring member before calling a floor reachable.
- Per-file union script (class/lines/line by filename + `git diff -U0` new-side hunks) reproduced executor figures exactly; executor baseline/final Cobertura sat in the SAME session scratchpad (`issue-1002/`), and `cmp` proved the canonical `artifacts/csharp/coverage.xml` identical to the final.
- Hook simulation signature changed: `Invoke-FeatureReviewCoverageValidation -InputText (@{last_assistant_message=$t; stop_hook_active=$false}|ConvertTo-Json)`; `-RawPayload` (see [[985-review-residuals]]) is obsolete. Floors now read from root CLAUDE.md (80/75).
- Evidence filename stamps are UTC; local clock is -0400 — use `date -u` for the review stamp.

**Follow-ups owed:** B-3 maintainer decision (exempt vs seam follow-up for EfcFormController Actions/Breadcrumb residue); manual Outlook check pending.
