# P2-T4 sweep of QuickFiler/app.config (issue #973)

Timestamp: 2026-10-03T11-04
Command: EDIT-REDIRECT on QuickFiler/app.config for GROUP-PROD (12 Edit-tool replacements, each old_string the assemblyIdentity line plus its bindingRedirect line read by Grep -A 1), then EDIT-ADAL; observations: Grep multiline content over the 16 corrected names plus ADAL, CMD-LINECOUNT, CMD-CRCOUNT, Grep `<bindingRedirect` count, git -C <execution-worktree-root> ls-files --eol -- QuickFiler/app.config, CMD-BOM, git -C <execution-worktree-root> diff --numstat a6915d62fe9d85218e5453fc5ac5cd5674b04984 -- QuickFiler/app.config
EXIT_CODE: 0
Output Summary: 12 redirects corrected in place and the ADAL block deleted; every common-acceptance clause holds (L 243 to 239, CRLF and BOM preserved, numstat 12/16, System.Linq.AsyncEnumerable still at 10.0.0.7, bindingRedirect count 59 to 58). Microsoft.Bcl.Memory was already 10.0.0.12 (CURRENT list) and was not edited.

Before: LINECOUNT 243, CRCOUNT 243, REDIRECTS-BEFORE: 59, ADAL block at lines 65-68 (preceded by `</dependentAssembly>` at 64)

Corrected (post-edit line numbers): Microsoft.IdentityModel.Abstractions 8.23.0.0 (70-71); Azure.Core 1.63.0.0 (82-83); System.ClientModel 1.16.0.0 (98-99); System.IdentityModel.Tokens.Jwt 8.23.0.0 (106-107); Microsoft.IdentityModel.Protocols.OpenIdConnect 8.23.0.0 (110-111); Microsoft.IdentityModel.Tokens 8.23.0.0 (114-115); Microsoft.IdentityModel.Validators 8.23.0.0 (118-119); Microsoft.IdentityModel.Protocols 8.23.0.0 (122-123); Microsoft.Bcl.Numerics 10.0.0.12 (174-175); Microsoft.Extensions.Diagnostics.Abstractions 10.0.0.12 (194-195); Microsoft.Identity.Client 4.90.1.0 (210-211); Microsoft.Identity.Client.Extensions.Msal 4.90.1.0 (214-215). Each reads oldVersion="0.0.0.0-<corrected>" newVersion="<corrected>". Unedited: Microsoft.Bcl.Memory 10.0.0.12 (162-163).

(1) every EDIT-REDIRECT gate: met (12 corrected, no stale value for any of the 12 names)
(2) ADAL no match; LINECOUNT 239 = 243 - 4: met
(3) CRCOUNT 239 = LINECOUNT: met
(4) CMD-EOL w/crlf: met
(5) BOM-BYTES QuickFiler/app.config=239,187,191 (equals P0-T5): met
(6) NUMSTAT: 12	16	QuickFiler/app.config: met
(7) System.Linq.AsyncEnumerable 10.0.0.7 count 1 (206-207): met
(8) REDIRECTS-AFTER: 58 = 59 - 1: met
