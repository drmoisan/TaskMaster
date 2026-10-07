# P2-T14 sweep of UtilitiesCS.Test/app.config (issue #973)

Timestamp: 2026-10-03T11-04
Command: EDIT-REDIRECT on UtilitiesCS.Test/app.config for Microsoft.Bcl.Numerics (1 Edit-tool replacement, old_string the assemblyIdentity line plus its bindingRedirect line read by Grep -A 1), then EDIT-ADAL; observations: Grep multiline content over the 16 corrected names plus ADAL, CMD-LINECOUNT, CMD-CRCOUNT, Grep `<bindingRedirect` count, git -C <execution-worktree-root> ls-files --eol -- UtilitiesCS.Test/app.config, CMD-BOM, git -C <execution-worktree-root> diff --numstat a6915d62fe9d85218e5453fc5ac5cd5674b04984 -- UtilitiesCS.Test/app.config, git -C <execution-worktree-root> diff -U0 a6915d62fe9d85218e5453fc5ac5cd5674b04984 -- UtilitiesCS.Test/app.config
EXIT_CODE: 0
Output Summary: the Microsoft.Bcl.Numerics redirect corrected in place and the ADAL block deleted; every common-acceptance clause holds (L 383 to 379, CRLF and BOM preserved, numstat 1/5, System.Linq.AsyncEnumerable still at 10.0.0.7, bindingRedirect count 94 to 93). The zero-context diff shows exactly the ADAL deletion hunk and the one redirect line.

Before: LINECOUNT 383, CRCOUNT 383, REDIRECTS-BEFORE: 94, ADAL block at lines 61-64 (preceded by `</dependentAssembly>` at 60)

Corrected (post-edit line numbers): Microsoft.Bcl.Numerics oldVersion="0.0.0.0-10.0.0.12" newVersion="10.0.0.12" (186-187). Every other corrected-name block in this file was already current (CURRENT lists of P0-T4).

(1) every EDIT-REDIRECT gate: met (corrected 1, stale none)
(2) ADAL no match; LINECOUNT 379 = 383 - 4: met
(3) CRCOUNT 379 = LINECOUNT: met
(4) CMD-EOL w/crlf: met
(5) BOM-BYTES UtilitiesCS.Test/app.config=239,187,191 (equals P0-T5): met
(6) NUMSTAT: 1	5	UtilitiesCS.Test/app.config: met
(7) System.Linq.AsyncEnumerable 10.0.0.7 count 1 (242-243): met
(8) REDIRECTS-AFTER: 93 = 94 - 1: met

HUNK @@ -61,4 +60,0 @@
HUNK @@ -191 +187 @@
