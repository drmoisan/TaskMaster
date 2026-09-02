# `issue.md` Acceptance-Criteria Preimage — Remediation Cycle 1

- Timestamp: 2026-09-02T01-02
- Issue: #678
- Task: [P0-T3]
- Subject file: `docs/features/active/2026-08-28-quickfiler-carry-folder-predictor-to-item-controller-678/issue.md` (186 lines)

## Why a whole-file digest rather than an anchored diff

P2-T11 compares this preimage against the end state. A diff anchored at
`807fb0bb6e5e49f43efa6b256b05960bf078ca19` cannot serve that purpose: the previous cycle's
commits `8782db56` and `d1f51e3a` already modified `issue.md` relative to the base ref, so
an anchored diff is non-empty before this cycle does anything. A whole-file digest captured
now is the only comparison that isolates this cycle.

## Clause 1 — work-mode marker

Token `- Work Mode: minor-audit` occurs exactly **1** time, at line **13**.

Command: `grep -c -- "- Work Mode: minor-audit" <issue.md>` → `1`

## Clause 2 — acceptance-criteria heading

Heading `## Acceptance Criteria` occurs exactly **1** time, at line **62**.

Command: `grep -c "^## Acceptance Criteria$" <issue.md>` → `1`

## Clause 3 — acceptance-criteria line count

Lines matching the regular expression `^- \[[ x]\] AC`: **23**.

## Clause 4 — checked / unchecked split

- Checked (`- [x] AC`): **22**
- Unchecked (`- [ ] AC`): **1**

## Clause 5 — the single unchecked line, verbatim

Line **115**:

```
- [ ] AC20. Coverage does not regress on the changed lines and every new or modified member reaches at least 90% line coverage. Baseline and post-change coverage figures are recorded numerically. No `[ExcludeFromCodeCoverage]` attribute is added or removed anywhere in the change.
```

Its identifier is **AC20**. AC20 stays unchecked for this cycle; the plan's scope-boundary
constraint 2 forbids any checkbox transition and the remediation inputs defer NB-4 (AC20
per-member coverage) out of this cycle entirely.

## Clause 6 — SHA-256 digest (`R_ISSUE_DIGEST`)

Command: `Get-FileHash -Algorithm SHA256 -LiteralPath <issue.md>`

```
R_ISSUE_DIGEST = A34C27BB10D2081018E659FFB472D5A7FC9433232BC09FEF837E13FF46E0DD4C
```

## Clause 7 — absence of `spec.md` and `user-story.md`

- SearchScope: `docs/features/active/2026-08-28-quickfiler-carry-folder-predictor-to-item-controller-678/` (feature root; the feature is not versioned, so there is no `v1/` scope to search)
- SearchPatterns: `spec.md`, `user-story.md`
- SearchResult: none. `Test-Path` returned `False` for both. The complete file listing of
  the feature root is:
  `code-review.2026-09-01T23-35.md`, `feature-audit.2026-09-01T23-35.md`, `issue.md`,
  `plan.2026-08-31T21-12.md`, `policy-audit.2026-09-01T23-35.md`,
  `remediation-inputs.2026-09-01T23-44.md`, `remediation-plan.2026-09-01T23-44.md`.

This is the expected state for work mode `minor-audit`, for which `issue.md` is the sole
acceptance-criteria source.

## Output Summary

All seven clauses hold. Work-mode marker once at line 13; `## Acceptance Criteria` once at
line 62; 23 AC lines split 22 checked / 1 unchecked; the single unchecked line is AC20 at
line 115; `R_ISSUE_DIGEST` =
`A34C27BB10D2081018E659FFB472D5A7FC9433232BC09FEF837E13FF46E0DD4C`; neither `spec.md` nor
`user-story.md` exists.
