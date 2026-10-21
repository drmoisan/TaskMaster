# P0-T8 Baseline re-measurement of the known-debt table

Timestamp: 2026-10-02T03-08
Command: Grep count mode glob `**/app.config` patterns `assemblyIdentity name=` and `<bindingRedirect`; Glob `**/*.csproj`; per section 7 row, Grep multiline count mode glob `**/app.config` pattern `name="<escaped name>"[^\n]*\n[^\n]*newVersion="<escaped stale version>"`; one Grep glob `**/*.csproj` content mode `-o` pattern `Include="(<the 15 escaped names joined by |>), Version=[^,"]+`; Grep glob `**/*.csproj` pattern `Include="(System\.Linq\.AsyncEnumerable|Microsoft\.IdentityModel\.Clients\.ActiveDirectory|netstandard)`; Grep count mode glob `**/app.config` pattern `name="<escaped name>"` for each of the 3 unverifiable names (path `<execution-worktree-root>`; the plan's `*/app.config` and `*/*.csproj` globs return no match under the Grep and Glob tools at this worktree depth, as recorded in p0-t4, so `**/...` globs were used; each result set was confirmed to contain only root-level files)
EXIT_CODE: 0

SUBSTITUTION: the 15 per-row `Include=` Greps were issued as one alternation Grep whose `-o` output carries the file, line and matched text for every name. The alternation requires `, Version=` immediately after each name, so `Microsoft.Identity.Client` does not match the `.Extensions.Msal` sibling.

(a) Totals:

- `assemblyIdentity name=` count: 1176 across 17 files.
- `<bindingRedirect` count: 1176 across 17 files.
- Glob `**/*.csproj`: 18 files, all of the form `<directory>/<directory>.csproj` at root level.

(b) Per-row results (config files carrying the stale pair, every per-file count 1; csproj Reference version printed):

```text
ROW 1  Azure.Core | 1.62.0.0 | files=6  | per-file count 1 | csproj Version=1.63.0.0 only
ROW 2  Microsoft.Bcl.Memory | 10.0.0.7 | files=10 | per-file count 1 | csproj Version=10.0.0.12 only
ROW 3  Microsoft.Bcl.Numerics | 10.0.0.5 | files=12 | per-file count 1 | csproj Version=10.0.0.12 only
ROW 4  Microsoft.Extensions.Diagnostics.Abstractions | 10.0.0.5 | files=6 | per-file count 1 | csproj Version=10.0.0.12 only
ROW 5  Microsoft.Identity.Client | 4.89.0.0 | files=6 | per-file count 1 | csproj Version=4.90.1.0 only
ROW 6  Microsoft.Identity.Client.Extensions.Msal | 4.89.0.0 | files=6 | per-file count 1 | csproj Version=4.90.1.0 only
ROW 7  Microsoft.IdentityModel.Abstractions | 8.22.0.0 | files=6 | per-file count 1 | csproj Version=8.23.0.0 only
ROW 8  Microsoft.IdentityModel.JsonWebTokens | 8.22.0.0 | files=7 | per-file count 1 | csproj Version=8.23.0.0 only
ROW 9  Microsoft.IdentityModel.Logging | 8.22.0.0 | files=7 | per-file count 1 | csproj Version=8.23.0.0 only
ROW 10 Microsoft.IdentityModel.Protocols | 8.22.0.0 | files=13 | per-file count 1 | csproj Version=8.23.0.0 only
ROW 11 Microsoft.IdentityModel.Protocols.OpenIdConnect | 8.22.0.0 | files=13 | per-file count 1 | csproj Version=8.23.0.0 only
ROW 12 Microsoft.IdentityModel.Tokens | 8.22.0.0 | files=13 | per-file count 1 | csproj Version=8.23.0.0 only
ROW 13 Microsoft.IdentityModel.Validators | 8.22.0.0 | files=13 | per-file count 1 | csproj Version=8.23.0.0 only
ROW 14 System.IdentityModel.Tokens.Jwt | 8.22.0.0 | files=13 | per-file count 1 | csproj Version=8.23.0.0 only
ROW 15 System.ClientModel | 1.3.0.0 | files=6 | per-file count 1 | csproj Version=1.16.0.0 only
```

Sum of the config counts: 6+10+12+6+6+6+6+7+7+13+13+13+13+13+6 = 137 entries, equal to section 7.

Every csproj `-o` match for the 15 names ends in the row's Reference version. Rows 1, 3 through 15 print their versions from the csproj files listed by the Grep (for example UtilitiesCS/UtilitiesCS.csproj lines 51 through 351 carry all 15 names); no other version was printed for any name.

(c) Unverifiable names:

- Grep `Include="(System.Linq.AsyncEnumerable|Microsoft.IdentityModel.Clients.ActiveDirectory|netstandard)` over `**/*.csproj`: no match.
- `name="System.Linq.AsyncEnumerable"` over `**/app.config`: 15 files, 15 total.
- `name="Microsoft.IdentityModel.Clients.ActiveDirectory"` over `**/app.config`: 13 files, 13 total.
- `name="netstandard"` over `**/app.config`: 1 file (TaskMaster/app.config), 1 total.

Acceptance: every figure equals section 7 (1176, 1176, 18; 15 rows with config counts 6, 10, 12, 6, 6, 6, 6, 7, 7, 13, 13, 13, 13, 13, 6 and the stated Reference versions; unverifiable counts 15, 13, 1 with no csproj Reference). No `KNOWN-DEBT-DRIFT`. The orchestrator's note that origin/main was merged after the plan's measurement does not change any figure: no numeric difference from the plan's section 4 or section 7 tables was observed in P0-T4 through P0-T8.

Output Summary: Known-debt table re-measured on the merged tree; all 15 rows (137 entries), 3 unverifiable names (29 entries), 1176 redirect entries, 17 configs and 18 csproj equal the plan. No drift.
