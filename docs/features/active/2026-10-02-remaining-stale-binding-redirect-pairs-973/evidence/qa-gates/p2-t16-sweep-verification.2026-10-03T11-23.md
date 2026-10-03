# P2-T16 sweep verification (issue #973)

Timestamp: 2026-10-03T11-23
Command: Grep tool (CMD-PAIR-COUNT, multiline, count mode, glob */app.config) for each section 8 row with its corrected version; one multiline Grep over the 15 pair names crossed with the six stale values (stale check, stricter than per-row); CMD-NAME-COUNT Microsoft.IdentityModel.Clients.ActiveDirectory; CMD-PAIR-COUNT System.Linq.AsyncEnumerable 10.0.0.7; the P0-T4 value-string Grep; git -C <execution-worktree-root> diff --name-only a6915d62fe9d85218e5453fc5ac5cd5674b04984 -- SVGControl/app.config SVGControl.Test/app.config; git -C <execution-worktree-root> status --porcelain -- SVGControl SVGControl.Test
EXIT_CODE: 0
Output Summary: every corrected-version file list equals the P0-T4 stale-plus-CURRENT union (sizes 16, 15, 13, 16, 16, 16, 16, 9, 9, 15, 15, 15, 15, 15, 16, every per-file count 1); no stale value remains for any of the 15 names; ADAL absent everywhere (positive control: P0-T4 recorded 13 files); System.Linq.AsyncEnumerable 10.0.0.7 still in 15 files; value-string count 15 across 15 files; SVGControl configs untouched.

## Corrected-version census (section 8 row order)

- Azure.Core 1.63.0.0: 16 files (6 stale + 10 CURRENT; includes SVGControl.Test)
- Microsoft.Bcl.Memory 10.0.0.12: 15 files (10 + 5)
- Microsoft.Bcl.Numerics 10.0.0.12: 13 files (12 + 1)
- Microsoft.Extensions.Diagnostics.Abstractions 10.0.0.12: 16 files (6 + 10)
- Microsoft.Identity.Client 4.90.1.0: 16 files (6 + 10)
- Microsoft.Identity.Client.Extensions.Msal 4.90.1.0: 16 files (6 + 10)
- Microsoft.IdentityModel.Abstractions 8.23.0.0: 16 files (6 + 10)
- Microsoft.IdentityModel.JsonWebTokens 8.23.0.0: 9 files (7 + 2)
- Microsoft.IdentityModel.Logging 8.23.0.0: 9 files (7 + 2)
- Microsoft.IdentityModel.Protocols 8.23.0.0: 15 files (13 + 2)
- Microsoft.IdentityModel.Protocols.OpenIdConnect 8.23.0.0: 15 files (13 + 2)
- Microsoft.IdentityModel.Tokens 8.23.0.0: 15 files (13 + 2)
- Microsoft.IdentityModel.Validators 8.23.0.0: 15 files (13 + 2)
- System.IdentityModel.Tokens.Jwt 8.23.0.0: 15 files (13 + 2)
- System.ClientModel 1.16.0.0: 16 files (6 + 10; includes SVGControl.Test)
Each file list was compared name by name with the union of the P0-T4 stale list and CURRENT list for that name: equal in every row; every per-file count 1.

## Other checks

- Stale (15 names x stale values 1.62.0.0, 10.0.0.7, 10.0.0.5, 4.89.0.0, 8.22.0.0, 1.3.0.0): no match
- CMD-NAME-COUNT Microsoft.IdentityModel.Clients.ActiveDirectory: no match
- CMD-PAIR-COUNT System.Linq.AsyncEnumerable 10.0.0.7: 15 files (unchanged)
- Value-string Grep: 15 across 15 files (only the System.Linq.AsyncEnumerable entries remain)
- git diff --name-only (SVGControl configs): (empty)
- git status --porcelain -- SVGControl SVGControl.Test: (empty)
