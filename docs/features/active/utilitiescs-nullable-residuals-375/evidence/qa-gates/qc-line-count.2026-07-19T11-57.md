# QC Line-Count Check (P12-T6) — AC8

Timestamp: 2026-07-19T11-57

Command: `git diff dffadd5a --name-only -- 'UtilitiesCS/**/*.cs'` then `wc -l` per file.

EXIT_CODE: 0

Output Summary: 37 opted-in hand-written files. Post-edit line counts (descending):

| Lines | File | Status |
|---|---|---|
| 849 | OutlookObjects/AppointmentItem/MeetingItemHelper.cs | pre-existing >500 breach (was 847); FLAGGED, not split |
| 774 | OutlookObjects/Recipient/RecipientStatic.cs | pre-existing >500 breach (was 773); FLAGGED, not split |
| 725 | OutlookObjects/Fields/UserDefinedFields.cs | pre-existing >500 breach (was 722); FLAGGED, not split |
| 377 | EmailIntelligence/OlFolderTools/OlFolderHelper/SmithWaterman.cs | under 500 (largest non-breach) |
| ... | (33 remaining files) | all under 500 |

Only the three pre-existing >500-line files exceed 500; each was already over 500 before any edit and
is flagged (spec.md Maintainer Decisions item 6), not split. No other in-scope file newly crosses 500
(next largest is SmithWaterman at 377). Adding a `#nullable enable` pragma and annotation-in-place edits
kept the three breaches at 849/774/725 (already over, not newly breached, not status-changing). AC8 satisfied.
