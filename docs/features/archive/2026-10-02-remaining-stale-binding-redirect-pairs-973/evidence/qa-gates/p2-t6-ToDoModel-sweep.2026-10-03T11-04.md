# P2-T6 sweep of ToDoModel/app.config (issue #973)

Timestamp: 2026-10-03T11-04
Command: EDIT-REDIRECT on ToDoModel/app.config for GROUP-PROD (12 Edit-tool replacements, each old_string the assemblyIdentity line plus its bindingRedirect line read by Grep -A 1), then EDIT-ADAL; observations: Grep multiline content over the 16 corrected names plus ADAL, CMD-LINECOUNT, CMD-CRCOUNT, Grep `<bindingRedirect` count, git -C <execution-worktree-root> ls-files --eol -- ToDoModel/app.config, CMD-BOM, git -C <execution-worktree-root> diff --numstat a6915d62fe9d85218e5453fc5ac5cd5674b04984 -- ToDoModel/app.config
EXIT_CODE: 0
Output Summary: 12 redirects corrected in place and the ADAL block deleted; every common-acceptance clause holds (L 323 to 319, CRLF and BOM preserved, numstat 12/16, System.Linq.AsyncEnumerable still at 10.0.0.7, bindingRedirect count 58 to 57). Microsoft.Bcl.Memory was already 10.0.0.12 and was not edited.

Before: LINECOUNT 323, CRCOUNT 323, REDIRECTS-BEFORE: 58, ADAL block at lines 66-69 (preceded by `</dependentAssembly>` at 65)

Corrected (post-edit line numbers): Microsoft.IdentityModel.Abstractions 8.23.0.0 (71-72); Azure.Core 1.63.0.0 (83-84); System.ClientModel 1.16.0.0 (103-104); System.IdentityModel.Tokens.Jwt 8.23.0.0 (111-112); Microsoft.IdentityModel.Protocols.OpenIdConnect 8.23.0.0 (115-116); Microsoft.IdentityModel.Tokens 8.23.0.0 (119-120); Microsoft.IdentityModel.Validators 8.23.0.0 (123-124); Microsoft.IdentityModel.Protocols 8.23.0.0 (127-128); Microsoft.Bcl.Numerics 10.0.0.12 (179-180); Microsoft.Extensions.Diagnostics.Abstractions 10.0.0.12 (203-204); Microsoft.Identity.Client 4.90.1.0 (215-216); Microsoft.Identity.Client.Extensions.Msal 4.90.1.0 (219-220). Each reads oldVersion="0.0.0.0-<corrected>" newVersion="<corrected>". Unedited: Microsoft.Bcl.Memory 10.0.0.12 (167-168).

(1) every EDIT-REDIRECT gate: met (12 corrected, no stale value for any of the 12 names)
(2) ADAL no match; LINECOUNT 319 = 323 - 4: met
(3) CRCOUNT 319 = LINECOUNT: met
(4) CMD-EOL w/crlf: met
(5) BOM-BYTES ToDoModel/app.config=239,187,191 (equals P0-T5): met
(6) NUMSTAT: 12	16	ToDoModel/app.config: met
(7) System.Linq.AsyncEnumerable 10.0.0.7 count 1 (211-212): met
(8) REDIRECTS-AFTER: 57 = 58 - 1: met
