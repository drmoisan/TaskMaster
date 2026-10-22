# P2-T7 sweep of VBFunctions.Test/app.config (issue #973)

Timestamp: 2026-10-03T11-04
Command: EDIT-REDIRECT on VBFunctions.Test/app.config for GROUP-TEST plus Microsoft.Bcl.Numerics (9 Edit-tool replacements, each old_string the assemblyIdentity line plus its bindingRedirect line read by Grep -A 1), then EDIT-ADAL; observations: Grep multiline content over the 16 corrected names plus ADAL, CMD-LINECOUNT, CMD-CRCOUNT, Grep `<bindingRedirect` count, git -C <execution-worktree-root> ls-files --eol -- VBFunctions.Test/app.config, CMD-BOM, git -C <execution-worktree-root> diff --numstat a6915d62fe9d85218e5453fc5ac5cd5674b04984 -- VBFunctions.Test/app.config
EXIT_CODE: 0
Output Summary: 9 redirects corrected in place and the ADAL block deleted; every common-acceptance clause holds (L 351 to 347, CRLF and BOM preserved, numstat 9/13, System.Linq.AsyncEnumerable still at 10.0.0.7, bindingRedirect count 86 to 85).

Before: LINECOUNT 351, CRCOUNT 351, REDIRECTS-BEFORE: 86, ADAL block at lines 49-52 (preceded by `</dependentAssembly>` at 48)

Corrected (post-edit line numbers): System.IdentityModel.Tokens.Jwt 8.23.0.0 (102-103); Microsoft.IdentityModel.JsonWebTokens 8.23.0.0 (106-107); Microsoft.IdentityModel.Logging 8.23.0.0 (110-111); Microsoft.IdentityModel.Protocols.OpenIdConnect 8.23.0.0 (114-115); Microsoft.IdentityModel.Tokens 8.23.0.0 (118-119); Microsoft.IdentityModel.Validators 8.23.0.0 (122-123); Microsoft.IdentityModel.Protocols 8.23.0.0 (126-127); Microsoft.Bcl.Memory 10.0.0.12 (174-175); Microsoft.Bcl.Numerics 10.0.0.12 (186-187). Each reads oldVersion="0.0.0.0-<corrected>" newVersion="<corrected>". Already current and unedited: Microsoft.IdentityModel.Abstractions 8.23.0.0, Azure.Core 1.63.0.0, System.ClientModel 1.16.0.0, Microsoft.Extensions.Diagnostics.Abstractions 10.0.0.12, Microsoft.Identity.Client 4.90.1.0, Microsoft.Identity.Client.Extensions.Msal 4.90.1.0.

(1) every EDIT-REDIRECT gate: met (9 corrected, no stale value for any of the 9 names)
(2) ADAL no match; LINECOUNT 347 = 351 - 4: met
(3) CRCOUNT 347 = LINECOUNT: met
(4) CMD-EOL w/crlf: met
(5) BOM-BYTES VBFunctions.Test/app.config=239,187,191 (equals P0-T5): met
(6) NUMSTAT: 9	13	VBFunctions.Test/app.config: met
(7) System.Linq.AsyncEnumerable 10.0.0.7 count 1 (242-243): met
(8) REDIRECTS-AFTER: 85 = 86 - 1: met
