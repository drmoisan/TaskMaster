# P2-T1 SortEmail.MailItemSort.cs verbatim move census

Timestamp: 2026-10-01T20-54
Command: Write tool created UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.MailItemSort.cs by the File Assembly Rule (SRC lines 1 to 22, `    public static partial class SortEmail`, `    {`, segments S03, S04, S08, S09, S26 separated by one empty line, `    }`, `}`); then CMD-MOVE-CENSUS with STATE split and ONLY SortEmail.MailItemSort.cs (pwsh -NoProfile -Command, first statement Set-Location to the item worktree)
EXIT_CODE: 0
Output Summary:
FILE-EXACT SortEmail.MailItemSort.cs = True
HEADER-PREFIX SortEmail.MailItemSort.cs = True
CLOSING-BRACES SortEmail.MailItemSort.cs = True
FIRST-LINE SortEmail.MailItemSort.cs = True
LINES SortEmail.MailItemSort.cs = 388
Observation: before writing, a scratch probe outside the repository confirmed that the Write tool preserves the doubled backslash in the S26 interpolated string (backslash count 2); FILE-EXACT True confirms it in the written file.
Acceptance: FILE-EXACT True, FIRST-LINE True and LINES 388 (all hold).
