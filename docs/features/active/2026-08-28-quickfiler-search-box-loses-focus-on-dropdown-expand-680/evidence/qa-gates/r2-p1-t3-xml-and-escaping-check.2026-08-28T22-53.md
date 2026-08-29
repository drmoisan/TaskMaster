Timestamp: 2026-08-28T22-53
Command: (a) [xml](Get-Content -Raw -Path <path>) per file; (b) Select-String -SimpleMatch -Pattern
'<repo-root>','<user>','<host>' -AllMatches across the four files; (c) Select-String -SimpleMatch
-Pattern '&lt;repo-root&gt;','&lt;user&gt;','&lt;host&gt;' -AllMatches across the four files
EXIT_CODE: 0
Output Summary: (a) 0 of 4 files threw an XML parse exception — all four remain well-formed after
sanitization. (b) Raw unescaped placeholder search returned 0 combined matches. (c) Escaped placeholder
search returned 7641 combined matches (>= 4 required, confirming at least one escaped placeholder landed
in each file). All three acceptance conditions hold.

Correction note: PowerShell's `Select-String -SimpleMatch -AllMatches` does not populate the returned
MatchInfo objects' `.Matches` collection (that field is populated only in regex mode); reading
`.Matches.Count` under `-SimpleMatch` always evaluates to 0 regardless of hits, which would make both the
zero-hit and the >=4-hit sub-conditions read as 0 and vacuously pass/fail depending on interpretation.
The combined-match count reported above uses `Measure-Object` over the returned MatchInfo collection
(one element per matching line) instead, which is the correct observable signal for this cmdlet mode and
matches the same non-vacuous result the underlying `rg` byte-level sweep already confirmed in P1-T2.
