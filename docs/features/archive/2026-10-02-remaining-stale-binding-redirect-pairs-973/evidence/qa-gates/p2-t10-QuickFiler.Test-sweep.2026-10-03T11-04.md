# P2-T10 sweep of QuickFiler.Test/app.config (issue #973)

Timestamp: 2026-10-03T11-04
Command: EDIT-REDIRECT on QuickFiler.Test/app.config for GROUP-TEST plus Microsoft.Bcl.Numerics (9 Edit-tool replacements, each old_string the assemblyIdentity line plus its bindingRedirect line read by Grep -A 1), then EDIT-ADAL; observations: Grep multiline content over the 16 corrected names plus ADAL, CMD-LINECOUNT, CMD-CRCOUNT, Grep `<bindingRedirect` count, git -C <execution-worktree-root> ls-files --eol -- QuickFiler.Test/app.config, CMD-BOM, git -C <execution-worktree-root> diff --numstat a6915d62fe9d85218e5453fc5ac5cd5674b04984 -- QuickFiler.Test/app.config
EXIT_CODE: 0
Output Summary: 9 redirects corrected in place and the ADAL block deleted; every common-acceptance clause holds (L 367 to 363, CRLF and BOM preserved, numstat 9/13, System.Linq.AsyncEnumerable still at 10.0.0.7, bindingRedirect count 90 to 89).

Before: LINECOUNT 367, CRCOUNT 367, REDIRECTS-BEFORE: 90, ADAL block at lines 61-64 (preceded by `</dependentAssembly>` at 60)

Corrected (post-edit line numbers): System.IdentityModel.Tokens.Jwt 8.23.0.0 (106-107); Microsoft.IdentityModel.JsonWebTokens 8.23.0.0 (110-111); Microsoft.IdentityModel.Logging 8.23.0.0 (114-115); Microsoft.IdentityModel.Protocols.OpenIdConnect 8.23.0.0 (118-119); Microsoft.IdentityModel.Tokens 8.23.0.0 (122-123); Microsoft.IdentityModel.Validators 8.23.0.0 (126-127); Microsoft.IdentityModel.Protocols 8.23.0.0 (130-131); Microsoft.Bcl.Memory 10.0.0.12 (178-179); Microsoft.Bcl.Numerics 10.0.0.12 (190-191). Each reads oldVersion="0.0.0.0-<corrected>" newVersion="<corrected>".

(1) every EDIT-REDIRECT gate: met (9 corrected, no stale value for any of the 9 names)
(2) ADAL no match; LINECOUNT 363 = 367 - 4: met
(3) CRCOUNT 363 = LINECOUNT: met
(4) CMD-EOL w/crlf: met
(5) BOM-BYTES QuickFiler.Test/app.config=239,187,191 (equals P0-T5): met
(6) NUMSTAT: 9	13	QuickFiler.Test/app.config: met
(7) System.Linq.AsyncEnumerable 10.0.0.7 count 1 (246-247): met
(8) REDIRECTS-AFTER: 89 = 90 - 1: met
