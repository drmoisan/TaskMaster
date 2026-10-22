# P2-T2 SortEmail.AttachmentSaving.cs verbatim move census

Timestamp: 2026-10-01T20-55
Command: Write tool created UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.AttachmentSaving.cs by the File Assembly Rule (SRC lines 1 to 22, class line, `    {`, segments S12, S10, S15, S16, S17, S18, S19, S20, S24, S25, S28, S29 separated by one empty line, `    }`, `}`); then CMD-MOVE-CENSUS with STATE split and ONLY SortEmail.AttachmentSaving.cs (one trailing `Get-Date -Format "yyyy-MM-ddTHH-mm"` statement was appended to the same invocation to read the write time; it prints only the timestamp and does not affect the census)
EXIT_CODE: 0
Output Summary:
FILE-EXACT SortEmail.AttachmentSaving.cs = True
HEADER-PREFIX SortEmail.AttachmentSaving.cs = True
CLOSING-BRACES SortEmail.AttachmentSaving.cs = True
FIRST-LINE SortEmail.AttachmentSaving.cs = True
LINES SortEmail.AttachmentSaving.cs = 342
Note: in the split state this file still assigns `_removeReadOnly` in Cleanup_Files (S10 verbatim); the field itself lives in SortEmail.TrySaveAttachment.cs (S13) after P2-T3, as the File Map prescribes.
Acceptance: FILE-EXACT True, FIRST-LINE True and LINES 342 (all hold).
