# P3-T4 Cleanup_Files resets the production prompt session

Timestamp: 2026-10-01T21-06
Command: one Edit on UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.AttachmentSaving.cs replacing `            _removeReadOnly = YesNoToAllResponse.Empty;` (the only `_removeReadOnly` occurrence in that file) with `            RemoveReadOnlyPrompt.Reset();`; then CMD-MOVE-CENSUS with STATE seam and ONLY SortEmail.AttachmentSaving.cs (one trailing `Get-Date -Format "yyyy-MM-ddTHH-mm"` statement appended to read the write time)
EXIT_CODE: 0
Output Summary:
FILE-EXACT SortEmail.AttachmentSaving.cs = True
HEADER-PREFIX SortEmail.AttachmentSaving.cs = True
CLOSING-BRACES SortEmail.AttachmentSaving.cs = True
FIRST-LINE SortEmail.AttachmentSaving.cs = True
LINES SortEmail.AttachmentSaving.cs = 342
Note: FILE-EXACT True in the seam state means the file equals its assembly with S10P in place of S10, so it differs from its Phase 2 content only by that one statement; the other three statements of Cleanup_Files (including the absent `_attachmentsAltName` reset of L3) are unchanged.
Acceptance: FILE-EXACT SortEmail.AttachmentSaving.cs = True in the seam state (holds).
