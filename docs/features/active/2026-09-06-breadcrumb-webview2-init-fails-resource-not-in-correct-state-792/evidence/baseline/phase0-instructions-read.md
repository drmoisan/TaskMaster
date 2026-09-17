# Phase 0 — Policy and Requirements Read Evidence ([P0-T1])

- Issue: #792
- Timestamp: 2026-09-17T18-33
- Work Mode: full-bug
- Work Mode source: `issue.md` line 12 (`- Work Mode: full-bug`), re-derived by `Select-String -Pattern '^- Work Mode:'` returning line 12.
- AC source (full-bug): `spec.md` only.

## Policy Order

Files read in the `policy-compliance-order` sequence, each from the item worktree:

1. `CLAUDE.md`
2. `.claude/rules/general-code-change.md`
3. `.claude/rules/general-unit-test.md`
4. `.claude/rules/csharp.md`
5. `.claude/rules/quality-tiers.md`
6. `.claude/rules/tonality.md`
7. `.claude/rules/plan-acceptance-gates.md`

## Files Read

All ten files were read in full with the Read tool from the item worktree on branch `bug/breadcrumb-webview2-init-fails-resource-not-in-correct-state-792`:

1. `CLAUDE.md`
2. `.claude/rules/general-code-change.md`
3. `.claude/rules/general-unit-test.md`
4. `.claude/rules/csharp.md`
5. `.claude/rules/quality-tiers.md`
6. `.claude/rules/tonality.md`
7. `.claude/rules/plan-acceptance-gates.md`
8. `docs/features/active/2026-09-06-breadcrumb-webview2-init-fails-resource-not-in-correct-state-792/spec.md`
9. `docs/features/active/2026-09-06-breadcrumb-webview2-init-fails-resource-not-in-correct-state-792/user-story.md`
10. `docs/features/active/2026-09-06-breadcrumb-webview2-init-fails-resource-not-in-correct-state-792/research/2026-09-17T11-20-breadcrumb-webview2-init-research.md`

## Acceptance Criteria Count (mechanical)

- Command: `Select-String -LiteralPath docs/features/active/2026-09-06-breadcrumb-webview2-init-fails-resource-not-in-correct-state-792/spec.md -Pattern '^- \[ \] AC-U[1-9]:'`
- EXIT_CODE: 0
- Output Summary: 9 matches at `spec.md` lines 306, 307, 308, 309, 310, 311, 312, 313, 314 (AC-U1 through AC-U9 in order). All nine are unchecked `- [ ]` lines. `spec.md` lines 306-314 hold exactly nine `- [ ] AC-U` checkbox lines.

## Notes

- The plan's `## Acceptance Criteria` heading in `spec.md` is at line 304; the nine criteria immediately follow it.
- `user-story.md` states that it carries no acceptance criteria of its own; it holds the AC-U5 manual runbook (lines 52-86).
- The superseded research record `research/2026-09-12T10-30-breadcrumb-webview2-init-research.md` was not read and is not relied on, per the plan's Inputs section.
