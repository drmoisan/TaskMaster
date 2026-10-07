# P2-T3 sweep of TaskVisualization/app.config (issue #973)

Timestamp: 2026-10-03T11-04
Command: EDIT-REDIRECT on TaskVisualization/app.config for GROUP-PROD plus Microsoft.Bcl.Memory (13 Edit-tool replacements, each old_string the assemblyIdentity line plus its bindingRedirect line read by Grep -A 1), then EDIT-ADAL; observations: Grep multiline content over the 16 corrected names plus ADAL, CMD-LINECOUNT, CMD-CRCOUNT, Grep `<bindingRedirect` count, git -C <execution-worktree-root> ls-files --eol -- TaskVisualization/app.config, CMD-BOM, git -C <execution-worktree-root> diff --numstat a6915d62fe9d85218e5453fc5ac5cd5674b04984 -- TaskVisualization/app.config
EXIT_CODE: 0
Output Summary: 13 redirects corrected in place and the ADAL block deleted; every common-acceptance clause holds (L 239 to 235, CRLF and BOM preserved, numstat 13/17, System.Linq.AsyncEnumerable still at 10.0.0.7, bindingRedirect count 58 to 57).

Before: LINECOUNT 239, CRCOUNT 239, REDIRECTS-BEFORE: 58, ADAL block at lines 61-64 (preceded by `</dependentAssembly>` at 60)

Corrected (post-edit line numbers): Microsoft.IdentityModel.Abstractions 8.23.0.0 (66-67); Azure.Core 1.63.0.0 (78-79); System.ClientModel 1.16.0.0 (98-99); System.IdentityModel.Tokens.Jwt 8.23.0.0 (106-107); Microsoft.IdentityModel.Protocols.OpenIdConnect 8.23.0.0 (110-111); Microsoft.IdentityModel.Tokens 8.23.0.0 (114-115); Microsoft.IdentityModel.Validators 8.23.0.0 (118-119); Microsoft.IdentityModel.Protocols 8.23.0.0 (122-123); Microsoft.Bcl.Memory 10.0.0.12 (162-163); Microsoft.Bcl.Numerics 10.0.0.12 (174-175); Microsoft.Extensions.Diagnostics.Abstractions 10.0.0.12 (198-199); Microsoft.Identity.Client 4.90.1.0 (210-211); Microsoft.Identity.Client.Extensions.Msal 4.90.1.0 (214-215). Each reads oldVersion="0.0.0.0-<corrected>" newVersion="<corrected>".

(1) every EDIT-REDIRECT gate: met (13 corrected, no stale value for any of the 13 names)
(2) ADAL no match; LINECOUNT 235 = 239 - 4: met
(3) CRCOUNT 235 = LINECOUNT: met
(4) CMD-EOL w/crlf: met
(5) BOM-BYTES TaskVisualization/app.config=239,187,191 (equals P0-T5): met
(6) NUMSTAT: 13	17	TaskVisualization/app.config: met
(7) System.Linq.AsyncEnumerable 10.0.0.7 count 1 (206-207): met
(8) REDIRECTS-AFTER: 57 = 58 - 1: met
