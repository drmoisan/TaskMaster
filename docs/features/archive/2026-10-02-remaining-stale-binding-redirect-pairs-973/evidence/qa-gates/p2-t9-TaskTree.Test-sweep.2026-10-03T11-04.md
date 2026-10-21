# P2-T9 sweep of TaskTree.Test/app.config (issue #973)

Timestamp: 2026-10-03T11-04
Command: EDIT-REDIRECT on TaskTree.Test/app.config for GROUP-TEST (8 Edit-tool replacements, each old_string the assemblyIdentity line plus its bindingRedirect line read by Grep -A 1); no ADAL block (no match before and after); observations: Grep multiline content over the 16 corrected names plus ADAL, CMD-LINECOUNT, CMD-CRCOUNT, Grep `<bindingRedirect` count, git -C <execution-worktree-root> ls-files --eol -- TaskTree.Test/app.config, CMD-BOM, git -C <execution-worktree-root> diff --numstat a6915d62fe9d85218e5453fc5ac5cd5674b04984 -- TaskTree.Test/app.config
EXIT_CODE: 0
Output Summary: 8 redirects corrected in place; no ADAL block before or after; every common-acceptance clause holds (L 343 unchanged, CRLF and BOM preserved, numstat 8/8, System.Linq.AsyncEnumerable still at 10.0.0.7, bindingRedirect count 84 unchanged).

Before: LINECOUNT 343, CRCOUNT 343, REDIRECTS-BEFORE: 84; ADAL before: no match

Corrected (line numbers unchanged): System.IdentityModel.Tokens.Jwt 8.23.0.0 (142-143); Microsoft.IdentityModel.JsonWebTokens 8.23.0.0 (146-147); Microsoft.IdentityModel.Logging 8.23.0.0 (150-151); Microsoft.IdentityModel.Protocols.OpenIdConnect 8.23.0.0 (154-155); Microsoft.IdentityModel.Tokens 8.23.0.0 (158-159); Microsoft.IdentityModel.Validators 8.23.0.0 (170-171); Microsoft.IdentityModel.Protocols 8.23.0.0 (174-175); Microsoft.Bcl.Memory 10.0.0.12 (206-207). Each reads oldVersion="0.0.0.0-<corrected>" newVersion="<corrected>".

(1) every EDIT-REDIRECT gate: met (8 corrected, no stale value for any of the 8 names)
(2) ADAL no match after; LINECOUNT 343 = L: met
(3) CRCOUNT 343 = LINECOUNT: met
(4) CMD-EOL w/crlf: met
(5) BOM-BYTES TaskTree.Test/app.config=239,187,191 (equals P0-T5): met
(6) NUMSTAT: 8	8	TaskTree.Test/app.config: met
(7) System.Linq.AsyncEnumerable 10.0.0.7 count 1 (238-239): met
(8) REDIRECTS-AFTER: 84 = REDIRECTS-BEFORE 84: met
