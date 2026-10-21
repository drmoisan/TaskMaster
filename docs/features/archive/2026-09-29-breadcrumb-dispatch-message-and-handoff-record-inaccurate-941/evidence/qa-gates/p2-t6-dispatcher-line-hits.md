# P2-T6 final line hits, BreadcrumbUiDispatcher.cs lines 180-188
Timestamp: 2026-10-01T07-25
Command: pwsh payload: XmlDocument.Load(coverage\coverage.cobertura.xml); SelectNodes with XPath: //class[contains(@filename,'BreadcrumbUiDispatcher.cs')]//line[number(@number)>=180 and number(@number)<=188] ; max hits per line number (read after P2-T5 measurement 2, the raw document then on disk; measurement 1 rows were identical)
EXIT_CODE: 0
Output Summary:
LINE 180 HITS 1
LINE 181 HITS 1
LINE 182 HITS 1
LINE 183 HITS 1
LINE 184 HITS 1
LINE 185 HITS 1
LINE 186 HITS 1
LINE 187 HITS 1
Matched line elements: 16; rows with HITS >= 1: 8
Note: no row for line 188, matching the baseline. A first selection attempt lost its quote characters through shell quoting and matched 2532 elements (all classes); it was discarded and the selection re-run with explicit quote characters, which reproduced the baseline element count of 16.
Loop iteration: 1
Loop history: none
