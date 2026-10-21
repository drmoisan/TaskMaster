# Phase 0 — feature documents read proof

Timestamp: 2026-08-27T23-17
Task: [P0-T2]
Command: `grep -c '^- \[[ x]\] ' spec.md` and a per-section awk tally over `spec.md`
EXIT_CODE: 0

## Documents read

1. `docs/features/active/efc-controller-surface-defects-464/issue.md` (240 lines) — READ. Scope and
   ownership statement; carries no acceptance criteria of its own and names `spec.md` as the AC source.
2. `docs/features/active/efc-controller-surface-defects-464/spec.md` (1167 lines) — READ in full.
3. `docs/features/active/efc-controller-surface-defects-464/research/2026-08-25T12-20-efc-controller-surface-defects.md`
   (1355 lines) — READ (executive summary §0, verified anchor table §1, stale-citation table §1.1, and
   the section outline for Q1 through Q11); technical input only, not an acceptance source.

## Resolved work mode

- Work Mode: **`full-bug`** — persisted marker at `issue.md:6` (`- Work Mode: full-bug`) and restated at
  `spec.md:9`.
- Under `acceptance-criteria-tracking`, `full-bug` resolves the AC source to **`spec.md` only**.
  `user-story.md` is intentionally absent and its absence is not a blocker.

## Acceptance-criterion inventory

- Total acceptance criteria in `spec.md`: **74**
- Currently checked: **0**

Per-section counts, in document order:

| # | Section | Criteria |
|---|---|---|
| 1 | `### #459 — KbdActions<> contract misuse` | 4 |
| 2 | `### #460 — cleanup NRE and timer leak` | 7 |
| 3 | `### #461 — dead conversation-expanded handler` | 4 |
| 4 | `### #463 — WebView2 incognito argument` | 4 |
| 5 | `### #464 — null-guard and async-void boundary defects` | 12 |
| 6 | `### #465 — form-controller lifecycle and selection defects` | 11 |
| 7 | `### #466 — dead code and latent NRE traps` | 8 |
| 8 | `### #467 — ProcessCmdKey swallows Alt mnemonics` | 7 |
| 9 | `### Cross-cutting` | 17 |
| | **Total** | **74** |

Observed counts match the plan's stated distribution 4, 7, 4, 4, 12, 11, 8, 7, 17 exactly.

## Additional binding documents read

Two documents supplied by the orchestrator modify how the plan's locators resolve. Both were read
before `[P0-T1]` executed and are recorded here for the audit trail:

- `plan-base-drift-addendum.2026-08-27T21-01.md` — authoritative for pre-change file:line locators
  against the real execution base. Corrects five locators and two premises.
- `upstream-constraints-briefing.2026-08-27T23-12.md` — binding obligations inherited from merged
  features #484 and #444.

Output Summary: Work mode resolves to `full-bug`; `spec.md` is the sole acceptance-criteria source and
carries exactly 74 checkbox criteria distributed 4/7/4/4/12/11/8/7/17 across nine sections, matching the
plan. Zero criteria are checked at baseline.
