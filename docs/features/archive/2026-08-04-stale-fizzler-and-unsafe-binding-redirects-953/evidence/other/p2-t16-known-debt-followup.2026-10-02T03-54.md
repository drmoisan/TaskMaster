# P2-T16 Follow-up note for the coordinator: known-debt binding redirects

Timestamp: 2026-10-02T03-54
Command: none (documentation artifact derived from plan section 7, re-measured at P0-T8 and pinned by test 14 at P1-T15 and P2-T3)
EXIT_CODE: 0

FOLLOW-UP: known-debt redirect correction, 15 pairs, 137 entries; promotion by the coordinator

Known-debt pairs (assembly | config newVersion | only csproj Reference version | configs carrying it):

1. Azure.Core | 1.62.0.0 | 1.63.0.0 | 6
2. Microsoft.Bcl.Memory | 10.0.0.7 | 10.0.0.12 | 10
3. Microsoft.Bcl.Numerics | 10.0.0.5 | 10.0.0.12 | 12
4. Microsoft.Extensions.Diagnostics.Abstractions | 10.0.0.5 | 10.0.0.12 | 6
5. Microsoft.Identity.Client | 4.89.0.0 | 4.90.1.0 | 6
6. Microsoft.Identity.Client.Extensions.Msal | 4.89.0.0 | 4.90.1.0 | 6
7. Microsoft.IdentityModel.Abstractions | 8.22.0.0 | 8.23.0.0 | 6
8. Microsoft.IdentityModel.JsonWebTokens | 8.22.0.0 | 8.23.0.0 | 7
9. Microsoft.IdentityModel.Logging | 8.22.0.0 | 8.23.0.0 | 7
10. Microsoft.IdentityModel.Protocols | 8.22.0.0 | 8.23.0.0 | 13
11. Microsoft.IdentityModel.Protocols.OpenIdConnect | 8.22.0.0 | 8.23.0.0 | 13
12. Microsoft.IdentityModel.Tokens | 8.22.0.0 | 8.23.0.0 | 13
13. Microsoft.IdentityModel.Validators | 8.22.0.0 | 8.23.0.0 | 13
14. System.IdentityModel.Tokens.Jwt | 8.22.0.0 | 8.23.0.0 | 13
15. System.ClientModel | 1.3.0.0 | 1.16.0.0 | 6

Total: 137 redirect entries across the 15 pairs (6 + 10 + 12 + 6 + 6 + 6 + 6 + 7 + 7 + 13 + 13 + 13 + 13 + 13 + 6).

Unverifiable names (no csproj `Reference Include="Name, Version=` anywhere): System.Linq.AsyncEnumerable (15 entries), Microsoft.IdentityModel.Clients.ActiveDirectory (13 entries), netstandard (1 entry); 29 entries in all.

Scope statement: correcting these 15 pairs is out of scope for issue 953. The repository-level ratchet (test 14 in `tests/scripts/dependencies/BindingRedirectVerification.Tests.ps1`) pins exactly this set, so correcting a pair makes test 14 fail on a stale known-debt entry until the recorded literal is updated in the same change.

Creation statement: this plan created no issue and no potential entry. The coordinator promotes this follow-up through the potential-entry lifecycle.

Output Summary: 15 known-debt pairs (137 entries) and 3 unverifiable names (29 entries) recorded for coordinator promotion; no issue or potential entry created.
