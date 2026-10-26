# Acceptance Criteria Reconciliation (Issue #1007, remediation cycle 1, P5-T15)

Timestamp: 2026-10-26T00-08
Command: SCRATCH helper: count `- [x]` and `- [ ]` lines inside the `## Acceptance Criteria` section of `<FEATURE>/spec.md` and `<FEATURE>/issue.md`, and compare the criterion lines (marker normalized) with `git show HEAD:<file>`
EXIT_CODE: 0
Output Summary: PASS. `spec.md` and `issue.md` each have 14 checked and 0 unchecked. In both files the criterion text is unchanged against `HEAD` once the marker is normalized. The only change in each file is the AC4 marker.

## AC4 check-off evidence (P5-T13, P5-T14)

| Evidence | Result |
|---|---|
| P2-T2 `qfc-pending-style-pass-after.2026-10-25T23-55.md` | `Stylesheet_StylesPendingDescendantLikeSelectedSubfolder` Passed; 24 of 24 passed |
| P5-T7 `qfc-pending-style.2026-10-26T00-05.md` | one `.sub.active` match inside `<style>`, with the selection colour declarations |
| P5-T4 `final-toolchain-tests.2026-10-26T00-03.md` | `Stylesheet_StylesPendingDescendantLikeSelectedSubfolder`, `OpenMove_DoesNotChangeCommittedValue`, `CommitPending_WithPendingNode_SelectsDescendant`, `Cancel_RestoresOriginalAndClearsPendingNode`, `OpenLeft_OnNonExpandedEntry_CommitsAndCloses` all Passed; 7761 of 7761 passed |

### Acceptance Criteria Status
- Source: `docs/features/active/2026-10-22-efc-arrow-keys-skip-subfolders-1007/spec.md` (work mode `full-bug`), mirrored in `docs/features/active/2026-10-22-efc-arrow-keys-skip-subfolders-1007/issue.md`
- Total AC items: 14
- Checked off (delivered): 14
- Remaining (unchecked): 0
- Items remaining: none

| File | Checked | Unchecked | Criterion text unchanged vs `HEAD` |
|---|---|---|---|
| spec.md | 14 | 0 | yes |
| issue.md | 14 | 0 | yes |
