# Pre-change Tree Facts (P0-T4)

Timestamp: 2026-10-09T14-04
Command: Grep tool (count mode) with patterns `^`, `\r$`, `id="Microsoft\.Web\.WebView2"|id="ObjectListView\.Official"`, `Include="Microsoft\.Web\.WebView2\.Core,`; Glob for the three new files
EXIT_CODE: 0
Output Summary:
- Every observed value equals the plan's expected value; no anchor re-derivation needed.

| Fact | Expected | Observed |
|---|---|---|
| QuickFiler.Test/packages.config lines | 74 | 74 |
| UtilitiesCS.Test/packages.config lines | 110 | 110 |
| TaskTree.Test/packages.config lines | 69 | 69 |
| QuickFiler.Test/QuickFiler.Test.csproj lines | 572 | 572 |
| scripts/dependencies/Repair-PackageManifestConsistency.ps1 lines | 475 | 475 |
| tests/scripts/dependencies/RepositoryTreeConsistency.Tests.ps1 lines | 152 | 152 |
| QuickFiler.Test/packages.config CR count | 73 | 73 |
| UtilitiesCS.Test/packages.config CR count | 109 | 109 |
| TaskTree.Test/packages.config CR count | 68 | 68 |
| WebView2/ObjectListView declarations: QuickFiler.Test | 0 | 0 |
| WebView2/ObjectListView declarations: UtilitiesCS.Test | 1 (line 70 ObjectListView.Official) | 1 (line 70 ObjectListView.Official 2.9.1) |
| WebView2/ObjectListView declarations: TaskTree.Test | 0 | 0 |
| `Include="Microsoft.Web.WebView2.Core,` in QuickFiler.Test.csproj | 2 | 2 (lines 388, 529) |
| scripts/dependencies/BindingRedirectSync.psm1 | absent | absent (Glob none; control Glob for BindingRedirect*.psm1 returned BindingRedirectVerification.psm1) |
| tests/scripts/dependencies/BindingRedirectSync.Tests.ps1 | absent | absent |
| tests/scripts/dependencies/Repair-PackageManifestConsistency.RedirectSync.Tests.ps1 | absent | absent |
