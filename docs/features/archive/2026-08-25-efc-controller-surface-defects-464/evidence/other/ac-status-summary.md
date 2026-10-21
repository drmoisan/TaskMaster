# [P11-T16] Acceptance-criteria status summary

Timestamp: 2026-08-28T02-14
Task: [P11-T16]
Command: checkbox census of `docs/features/active/efc-controller-surface-defects-464/spec.md`
EXIT_CODE: 0

### Acceptance Criteria Status

- Source: `docs/features/active/efc-controller-surface-defects-464/spec.md`
- Total AC items: **74**
- Checked off (delivered): **74**
- Remaining (unchecked): **0**
- Items remaining: **none**

## Source resolution

`docs/features/active/efc-controller-surface-defects-464/issue.md:6` carries the marker
`- Work Mode: full-bug`. Under the `acceptance-criteria-tracking` resolution table, `full-bug` resolves
the acceptance-criteria source to **`spec.md` only**; `user-story.md` is optional and, as `[P11-T12]`
verifies, absent. `spec.md` is therefore the sole AC source and the single file in this census.

## Distribution of the 74

The distribution `spec.md:909` declares, all now checked:

| Group | Criteria |
|---|---|
| #459 | 4 |
| #460 | 7 |
| #461 | 4 |
| #463 | 4 |
| #464 | 12 |
| #465 | 11 |
| #466 | 8 |
| #467 | 7 |
| cross-cutting | 17 |
| **Total** | **74** |

## Check-off progression across the whole plan

| Point | Checked | Unchecked |
|---|---|---|
| Start of this batch (after Phase 8) | 53 | 21 |
| After Phase 9 | 62 | 12 |
| After Phase 11 check-offs | **74** | **0** |

## Integrity

`[P11-T15]` proves that the delivered `spec.md` differs from its pre-batch state on exactly 21 lines and
that every one of the 21 differences is `- [ ] ` becoming `- [x] ` with the remainder of the line
byte-identical. **No criterion text was modified**, and no criterion was added or removed.

## Items that remain outstanding but are not acceptance criteria

Recorded here so the zero in "Items remaining" is not read as "nothing is left to do". None of these is
an acceptance criterion, so none affects the census above.

| Item | Status |
|---|---|
| Manual check 1 — Alt+F and Alt+M open the `Filters` and `Move Options` menus in a live Outlook host | `MANUAL_CHECK_DEFERRED`; not recorded as a pass. `evidence/other/manual-validation.md` |
| Promotion of the six `spec.md` follow-up items plus a seventh discovered on this base | NOT CREATED; promotion tool unavailable to this executor. `evidence/other/followup-promotions.md` |
| RC7 residual — `EfcSelectionGuard.BannerPrefix` third arity variant and the stale comment near `EfcFormController.cs:325` | Reported, deliberately not fixed; sibling-owned file. `evidence/qa-gates/sibling-ownership.md` |

Output Summary: All **74** of the 74 acceptance criteria in
`docs/features/active/efc-controller-surface-defects-464/spec.md` — the sole AC source under the
persisted `full-bug` work mode — are checked off against recorded evidence. **0** remain unchecked and
there is no remaining item to quote. No criterion text was modified. Three non-criterion items remain
outstanding and are itemised above rather than left implicit.
