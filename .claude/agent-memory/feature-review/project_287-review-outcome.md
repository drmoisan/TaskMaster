---
name: 287-review-outcome
description: Review outcome for #287 (StoreWrapper dialog imprecise message) — PASS/0 blocking, coverage floor conflict noted
metadata:
  type: project
---

Reviewed `bug/storewrapper-dialog-imprecise-for-genuine-failure-287` on 2026-09-01. Verdict: PASS
across policy-audit, code-review, feature-audit; 16/16 spec.md AC independently verified; 0 blocking
findings.

**Why worth remembering:** Two independently-verifiable coverage floors coexist unreconciled in this
repo (CLAUDE.md: 80% repo-wide / 90% new-code; `.claude/rules/quality-tiers.md` +
`general-unit-test.md`: uniform 85% line / 75% branch, explicitly no tier relief). This PR's measured
figures (85.297% line / 79.293% branch repo-wide; 100%/100% new code) cleared both, so the conflict
didn't change the verdict — but it will matter for a future PR landing between 80% and 85%. Also: the
plan's D12 base-anchor literal was stale (main advanced after plan authoring); the executor correctly
recomputed and documented the divergence in `evidence/baseline/base-anchor.md` rather than either
trusting the stale literal or silently substituting — a positive pattern worth reinforcing, contrast
with [[Stale caller-supplied merge-base]] (#244) where the stale base went undetected.

**How to apply:** When reviewing further C# PRs, check both coverage-floor documents and report
against whichever is stricter unless the PR clears both (as here). See
[[storewrapper-controller-absent-from-cobertura]] for a coverage-artifact quirk found during this
review.
