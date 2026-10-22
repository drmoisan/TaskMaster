---
name: 1002-review-residuals
description: "#1002 (EFC descendant tree, trash preselect, QFC nesting): cycle 0 4 blocking; cycle 1 1 autonomous (AC23 vs ratified exemption); cycle 2 14-10 PASS 0 blocking"
metadata:
  type: project
---

Full-bug review, base 8e4ad1ecb, session cwd = review tree (no mirror). All artifacts validated first try with `validate_orchestration_artifacts` using the #985 policy-audit shape (Template Provenance, 7-col metrics table incl. a JavaScript row, checklist bullets, 1.2.1 bullets, 1.2.2 verdict paragraphs).

**Cycle 0 (head e96e65302, 2026-10-22T10-39):** 26/26 AC PASS, 4 blocking (3 autonomous, 1 human).
- A renderer that marks selection by attribute (`aria-selected="true"`) needs a matching CSS rule; grep the asset CSS for the attribute. None existed -> blocking autonomous (B-4).
- Modified-file floor split: coverage LOWERED by the diff = autonomous; pre-existing MessageBox/WebView2 residue = human_decision_required (CLAUDE.md UT2 exemption needs maintainer ratification); trivial `?? throw` guards at 50% branch = autonomous.
- Hook simulation: `Invoke-FeatureReviewCoverageValidation -InputText (@{last_assistant_message=$t; stop_hook_active=$false}|ConvertTo-Json)`.
- Evidence filename stamps are UTC; local clock is -0400 — use `date -u` for the review stamp.

**Cycle 1 re-audit (head b72ac1dbb, 2026-10-22T13-37):** B-1..B-4 resolved; 1 new blocking R-1 autonomous.
- Reusable: when a maintainer ratifies an exemption mid-loop, grep the spec AC text for clauses it falsifies ("no coverage exclusion is added"). The decision lifted the plan's Do-Not-Do but nobody amended AC23, which stayed checked -> AC FAIL, autonomous clerical amendment (reclass route to human stated). See [[973-review-residuals]] for the AC-amendment precedent.
- Member-level `[ExcludeFromCodeCoverage]` on async methods removes the state machine and lambdas from Cobertura too (no residual class nodes); valid-line counts drop (Actions 111->28).
- net48 `ExcludeFromCodeCoverageAttribute` has no `Justification` (CS0246); comment form is correct.
- Freshness check that worked: trx `<Counters>` + per-test `outcome=` grep in `coverage/test-results/mstest-coverage-run.trx`, and source mtimes earlier than the trx.
- Reused scratchpad `filecov.py` (args: repo xml base files...) for per-file + changed-line union.

**Cycle 2 re-audit (head 54b36112a, 2026-10-22T14-10):** PASS, 0 blocking; 26/26 AC PASS. R-1 resolved by a clause-only AC23 edit plus `## Planner Amendments`. All three artifacts validated on the first try in the same shape.
- Wrapping JS child text in a span is safe only if every click branch resolves its target with `closest(...)`; grep the handler, not just the test.
- Orchestrator AC9 ruling: a client-side transient render (bridge `subfolderResult`) is not governed by a "server render unchanged" AC.

**Follow-ups owed:** manual Outlook check pending (AC26, non-blocking).
