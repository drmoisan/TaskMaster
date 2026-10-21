# P0-T4 redirect census (issue #973; read-only)

Timestamp: 2026-10-03T10-43
Command: Grep tool (CMD-PAIR-COUNT), multiline, count mode, path <execution-worktree-root>, glob */app.config, pattern name="<name>"[^\n]*\n[^\n]*newVersion="<version>" for each section 8 row (stale and corrected), ADAL 2.22.0.0, System.Linq.AsyncEnumerable 10.0.0.7, netstandard 2.0.0.0 (content mode, -n); value-string Grep newVersion="(1\.62\.0\.0|10\.0\.0\.7|10\.0\.0\.5|4\.89\.0\.0|8\.22\.0\.0|1\.3\.0\.0|2\.22\.0\.0)" in count mode
EXIT_CODE: 0
Output Summary: stale totals 6, 10, 12, 6, 6, 6, 6, 7, 7, 13, 13, 13, 13, 13, 6 (sum 137), every per-file count 1, lists equal fact 3; CURRENT lists equal fact 9 and are disjoint from the stale lists; ADAL 13 files; System.Linq.AsyncEnumerable 15 files; netstandard only TaskMaster/app.config lines 38-39; value-string count 165 across 15 files. No CENSUS-DRIFT.

## Stale census (every per-file count 1)

- Azure.Core 1.62.0.0 = 6: Tags, TaskTree, TaskVisualization, QuickFiler, TaskMaster, ToDoModel
- Microsoft.Bcl.Memory 10.0.0.7 = 10: Tags, TaskTree, TaskVisualization, VBFunctions.Test, Tags.Test, TaskTree.Test, QuickFiler.Test, TaskMaster.Test, TaskVisualization.Test, ToDoModel.Test
- Microsoft.Bcl.Numerics 10.0.0.5 = 12: Tags, TaskTree, TaskVisualization, QuickFiler, TaskMaster, ToDoModel, VBFunctions.Test, QuickFiler.Test, TaskMaster.Test, TaskVisualization.Test, ToDoModel.Test, UtilitiesCS.Test
- Microsoft.Extensions.Diagnostics.Abstractions 10.0.0.5 = 6: Tags, TaskTree, TaskVisualization, QuickFiler, TaskMaster, ToDoModel
- Microsoft.Identity.Client 4.89.0.0 = 6: Tags, TaskTree, TaskVisualization, QuickFiler, TaskMaster, ToDoModel
- Microsoft.Identity.Client.Extensions.Msal 4.89.0.0 = 6: Tags, TaskTree, TaskVisualization, QuickFiler, TaskMaster, ToDoModel
- Microsoft.IdentityModel.Abstractions 8.22.0.0 = 6: Tags, TaskTree, TaskVisualization, QuickFiler, TaskMaster, ToDoModel
- Microsoft.IdentityModel.JsonWebTokens 8.22.0.0 = 7: VBFunctions.Test, Tags.Test, TaskTree.Test, QuickFiler.Test, TaskMaster.Test, TaskVisualization.Test, ToDoModel.Test
- Microsoft.IdentityModel.Logging 8.22.0.0 = 7: same seven test configs
- Microsoft.IdentityModel.Protocols 8.22.0.0 = 13: six production configs plus the seven test configs
- Microsoft.IdentityModel.Protocols.OpenIdConnect 8.22.0.0 = 13: same thirteen
- Microsoft.IdentityModel.Tokens 8.22.0.0 = 13: same thirteen
- Microsoft.IdentityModel.Validators 8.22.0.0 = 13: same thirteen
- System.IdentityModel.Tokens.Jwt 8.22.0.0 = 13: same thirteen
- System.ClientModel 1.3.0.0 = 6: Tags, TaskTree, TaskVisualization, QuickFiler, TaskMaster, ToDoModel
- TOTAL = 137

## Corrected-version census (every per-file count 1)

CURRENT Azure.Core=VBFunctions.Test, Tags.Test, TaskTree.Test, QuickFiler.Test, TaskMaster.Test, TaskVisualization.Test, ToDoModel.Test, UtilitiesCS.Test, SVGControl.Test, UtilitiesCS (10)
CURRENT Microsoft.Bcl.Memory=QuickFiler, TaskMaster, ToDoModel, UtilitiesCS, UtilitiesCS.Test (5)
CURRENT Microsoft.Bcl.Numerics=UtilitiesCS (1)
CURRENT Microsoft.Extensions.Diagnostics.Abstractions=same 10 as Azure.Core (10)
CURRENT Microsoft.Identity.Client=same 10 (10)
CURRENT Microsoft.Identity.Client.Extensions.Msal=same 10 (10)
CURRENT Microsoft.IdentityModel.Abstractions=same 10 (10)
CURRENT Microsoft.IdentityModel.JsonWebTokens=UtilitiesCS, UtilitiesCS.Test (2)
CURRENT Microsoft.IdentityModel.Logging=UtilitiesCS, UtilitiesCS.Test (2)
CURRENT Microsoft.IdentityModel.Protocols=UtilitiesCS, UtilitiesCS.Test (2)
CURRENT Microsoft.IdentityModel.Protocols.OpenIdConnect=UtilitiesCS, UtilitiesCS.Test (2)
CURRENT Microsoft.IdentityModel.Tokens=UtilitiesCS, UtilitiesCS.Test (2)
CURRENT Microsoft.IdentityModel.Validators=UtilitiesCS, UtilitiesCS.Test (2)
CURRENT System.IdentityModel.Tokens.Jwt=UtilitiesCS, UtilitiesCS.Test (2)
CURRENT System.ClientModel=same 10 as Azure.Core (10)

Each CURRENT list is disjoint from the stale list of the same name (checked name by name).

## Other families

- Microsoft.IdentityModel.Clients.ActiveDirectory 2.22.0.0 = 13: VBFunctions.Test, TaskVisualization.Test, TaskMaster.Test, TaskVisualization, TaskTree, TaskMaster, UtilitiesCS.Test, Tags, QuickFiler.Test, ToDoModel, UtilitiesCS, ToDoModel.Test, QuickFiler (fact 4 list)
- System.Linq.AsyncEnumerable 10.0.0.7 = 15: every root-level config except SVGControl and SVGControl.Test
- netstandard 2.0.0.0 (content, -n): TaskMaster\app.config:38 `<assemblyIdentity name="netstandard" publicKeyToken="cc7b13ffcd2ddd51" culture="neutral" />`; TaskMaster\app.config:39 `<bindingRedirect oldVersion="0.0.0.0-2.1.0.0" newVersion="2.0.0.0" />`
- Value-string count: 165 across 15 files (Tags 15, TaskTree 15, TaskVisualization 15, QuickFiler 14, TaskMaster 14, ToDoModel 14, VBFunctions.Test 11, TaskMaster.Test 11, QuickFiler.Test 11, TaskVisualization.Test 11, ToDoModel.Test 11, Tags.Test 9, TaskTree.Test 9, UtilitiesCS.Test 3, UtilitiesCS 2)
