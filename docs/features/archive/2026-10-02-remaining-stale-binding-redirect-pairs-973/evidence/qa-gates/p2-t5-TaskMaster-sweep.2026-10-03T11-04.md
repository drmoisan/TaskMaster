# P2-T5 sweep of TaskMaster/app.config (issue #973)

Timestamp: 2026-10-03T11-04
Command: EDIT-REDIRECT on TaskMaster/app.config for GROUP-PROD (12 Edit-tool replacements, each old_string the assemblyIdentity line plus its bindingRedirect line read by Grep -A 1), then EDIT-ADAL; observations: Grep multiline content over the 16 corrected names plus ADAL, CMD-LINECOUNT, CMD-CRCOUNT, Grep `<bindingRedirect` count, Grep `name="netstandard"` -A 1, git -C <execution-worktree-root> ls-files --eol -- TaskMaster/app.config, CMD-BOM, git -C <execution-worktree-root> diff --numstat a6915d62fe9d85218e5453fc5ac5cd5674b04984 -- TaskMaster/app.config, git -C <execution-worktree-root> diff -U0 a6915d62fe9d85218e5453fc5ac5cd5674b04984 -- TaskMaster/app.config (CMD-HUNKS)
EXIT_CODE: 0
Output Summary: 12 redirects corrected in place and the ADAL block deleted; every common-acceptance clause holds (L 430 to 426, CRLF and BOM preserved, numstat 12/16, System.Linq.AsyncEnumerable still at 10.0.0.7, bindingRedirect count 59 to 58); the netstandard block at 38-39 is unchanged and no hunk starts between old lines 36 and 41 (AC15).

Before: LINECOUNT 430, CRCOUNT 430, REDIRECTS-BEFORE: 59, ADAL block at lines 73-76 (preceded by `</dependentAssembly>` at 72)

Corrected (post-edit line numbers): Microsoft.IdentityModel.Abstractions 8.23.0.0 (78-79); Azure.Core 1.63.0.0 (90-91); System.ClientModel 1.16.0.0 (110-111); System.IdentityModel.Tokens.Jwt 8.23.0.0 (118-119); Microsoft.IdentityModel.Protocols.OpenIdConnect 8.23.0.0 (122-123); Microsoft.IdentityModel.Tokens 8.23.0.0 (126-127); Microsoft.IdentityModel.Validators 8.23.0.0 (130-131); Microsoft.IdentityModel.Protocols 8.23.0.0 (134-135); Microsoft.Bcl.Numerics 10.0.0.12 (186-187); Microsoft.Extensions.Diagnostics.Abstractions 10.0.0.12 (206-207); Microsoft.Identity.Client 4.90.1.0 (222-223); Microsoft.Identity.Client.Extensions.Msal 4.90.1.0 (226-227). Each reads oldVersion="0.0.0.0-<corrected>" newVersion="<corrected>". Unedited: Microsoft.Bcl.Memory 10.0.0.12 (174-175).

(1) every EDIT-REDIRECT gate: met (12 corrected, no stale value for any of the 12 names)
(2) ADAL no match; LINECOUNT 426 = 430 - 4: met
(3) CRCOUNT 426 = LINECOUNT: met
(4) CMD-EOL w/crlf: met
(5) BOM-BYTES TaskMaster/app.config=239,187,191 (equals P0-T5): met
(6) NUMSTAT: 12	16	TaskMaster/app.config: met
(7) System.Linq.AsyncEnumerable 10.0.0.7 count 1 (218-219): met
(8) REDIRECTS-AFTER: 58 = 59 - 1: met

netstandard (Grep -A 1): 38 `<assemblyIdentity name="netstandard" publicKeyToken="cc7b13ffcd2ddd51" culture="neutral" />` / 39 `<bindingRedirect oldVersion="0.0.0.0-2.1.0.0" newVersion="2.0.0.0" />` (unchanged)

HUNK @@ -73,4 +72,0 @@
HUNK @@ -83 +79 @@
HUNK @@ -95 +91 @@
HUNK @@ -115 +111 @@
HUNK @@ -123 +119 @@
HUNK @@ -127 +123 @@
HUNK @@ -131 +127 @@
HUNK @@ -135 +131 @@
HUNK @@ -139 +135 @@
HUNK @@ -191 +187 @@
HUNK @@ -211 +207 @@
HUNK @@ -227 +223 @@
HUNK @@ -231 +227 @@
HUNKS-WITH-OLD-START-36-TO-41: none (AC15)
