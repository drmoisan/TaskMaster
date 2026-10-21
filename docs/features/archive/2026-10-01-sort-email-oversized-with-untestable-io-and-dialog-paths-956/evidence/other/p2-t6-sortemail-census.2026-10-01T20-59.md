# P2-T6 SortEmail.cs rewrite and verbatim move census

Timestamp: 2026-10-01T20-59
Command: Write tool rewrote UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.cs (read first) by the File Assembly Rule (SRC lines 1 to 22, `    public static partial class SortEmail` at line 23, `    {`, segments S01, S02, S05, S06, S07, S27 separated by one empty line, `    }`, `}`; the three class-level region pairs dropped); one Edit then restored the UTF-8 byte-order mark by prefixing line 1 with U+FEFF; then CMD-MOVE-CENSUS with STATE split and ONLY SortEmail.cs (one trailing `Get-Date -Format "yyyy-MM-ddTHH-mm"` statement appended to read the write time)
EXIT_CODE: 0
Output Summary:
FILE-EXACT SortEmail.cs = True
HEADER-PREFIX SortEmail.cs = True
CLOSING-BRACES SortEmail.cs = True
FIRST-LINE SortEmail.cs = True
LINES SortEmail.cs = 277
BOM disposition: the Write tool wrote the file without the merge-base byte-order mark (first three bytes observed `23 6E 75`). One Edit prefixed line 1 with U+FEFF; the first three bytes observed afterwards are `EF BB BF` (one U+FEFF character in the file). The census above ran after the BOM restore: Get-Content -Encoding UTF8 consumes the mark, so FIRST-LINE, FILE-EXACT and LINES are unaffected. P2-T8 re-observes the bytes after formatting.
Acceptance: FILE-EXACT True, FIRST-LINE True and LINES 277 (all hold).
