# P2-T15 sweep of UtilitiesCS/app.config (issue #973)

Timestamp: 2026-10-03T11-04
Command: EDIT-ADAL on UtilitiesCS/app.config (one Edit-tool replacement, old_string the preceding `</dependentAssembly>` plus the four ADAL lines read by Grep -B 2 -A 2); observations: Grep `newVersion="(1\.62\.0\.0|10\.0\.0\.5|4\.89\.0\.0|8\.22\.0\.0|1\.3\.0\.0)"` before and after, Grep multiline content over ADAL and System.Linq.AsyncEnumerable, CMD-LINECOUNT, CMD-CRCOUNT, Grep `<bindingRedirect` count, git -C <execution-worktree-root> ls-files --eol -- UtilitiesCS/app.config, CMD-BOM, git -C <execution-worktree-root> diff --numstat a6915d62fe9d85218e5453fc5ac5cd5674b04984 -- UtilitiesCS/app.config
EXIT_CODE: 0
Output Summary: the ADAL block deleted; the file carries none of the 15 stale pair values before or after; every common-acceptance clause holds (L 293 to 289, CRLF and BOM preserved, numstat 0/4, System.Linq.AsyncEnumerable still at 10.0.0.7, bindingRedirect count 63 to 62).

Before: LINECOUNT 293, CRCOUNT 293, REDIRECTS-BEFORE: 63, ADAL block at lines 66-69 (preceded by `</dependentAssembly>` at 65); stale-pair value Grep: no match

(1) EDIT-REDIRECT gates: none applicable (P 0); stale-pair value Grep after: no match
(2) ADAL no match; LINECOUNT 289 = 293 - 4: met
(3) CRCOUNT 289 = LINECOUNT: met
(4) CMD-EOL w/crlf: met
(5) BOM-BYTES UtilitiesCS/app.config=239,187,191 (equals P0-T5): met
(6) NUMSTAT: 0	4	UtilitiesCS/app.config: met
(7) System.Linq.AsyncEnumerable 0.0.0.0-10.0.0.7 / 10.0.0.7 count 1 (211-212): met
(8) REDIRECTS-AFTER: 62 = 63 - 1: met
