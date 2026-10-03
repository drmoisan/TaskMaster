# 2026-10-02-remaining-stale-binding-redirect-pairs (Spec)

- **Issue:** #973
- **Parent (optional):** none
- **Owner:** drmoisan
- **Last Updated:** 2026-10-02T23-01
- **Status:** Draft
- **Version:** 1.0
- **Work Mode:** full-bug. This file is the sole authoritative acceptance-criteria source; no user-story document exists for this feature.

> Footprint note (do not "fix" this formatting): every repository file this change creates or modifies appears at least once as an inline code span with its full repository-relative path, and the `## Write Set` section lists all of them. Files that are cited but not modified (the detector module, the SVGControl configs, the #418 runbook, the #879 spec, the research records, CI workflow files) are written as plain prose on purpose. Evidence artifacts are backticked in full feature-relative form because they live under this feature folder. The aliased project-file element is shown in a fenced block because its HintPath points into the gitignored packages tree, which is not a repository file.

Research records (read-only inputs, cited below as "research 1" and "research 2"):

- research 1: docs/features/active/2026-10-02-remaining-stale-binding-redirect-pairs-973/research/2026-10-02T22-35-stale-binding-redirect-pairs-research.md (the 15 pairs, the per-pair remedy table, the ADAL and netstandard analysis, the drift mechanism, the write set).
- research 2: docs/features/active/2026-10-02-remaining-stale-binding-redirect-pairs-973/research/2026-10-02T22-53-system-linq-asyncenumerable-install-research.md (the System.Linq.AsyncEnumerable install, the aliased Reference form, the compile-impact analysis, the additional write set).

## Context

Preparation for #953 found that, besides the 11 Fizzler redirects #953 fixed, 15 other assembly-and-version pairs in the repository's app.config files redirect to a `newVersion` that no deployed assembly has. #953 added a Pester test that records those 15 pairs as known exceptions and fails on any new mismatch. #953 has merged: the recording test is the It block beginning at line 275 of `tests/scripts/dependencies/BindingRedirectVerification.Tests.ps1`, with the 15-pair literal at lines 293-309 and a three-name unverifiable literal at line 310. This issue corrects the 15 pairs, empties that list, and (by orchestrator scope decision, see Scope) also removes the dead ADAL redirect blocks and installs the package behind the System.Linq.AsyncEnumerable redirects so that the unverifiable list shrinks to the single deliberate netstandard entry.

Environment:
- OS/version: Windows 11 (Outlook VSTO add-in, .NET Framework 4.8.1 projects)
- Python version: n/a (app.config binding redirects, packages.config, non-SDK csproj)
- Command/flags used: the #953 redirect-consistency Pester test (tests/scripts/dependencies, run in CI by the Pester workflow and locally through the PoshQC MCP tools)
- Data source or fixture: every app.config directly under a root-level directory, compared against every csproj Reference Include version (the detector's definition of "deployed"; research 1 section 1)

Impact / Severity:
- [ ] Blocker
- [ ] High
- [x] Medium
- [ ] Low

## Repro & Evidence

Steps to Reproduce (current branch, before this change):
1. Read lines 293-310 of `tests/scripts/dependencies/BindingRedirectVerification.Tests.ps1`: `$expectedDebt` lists 15 assembly|newVersion pairs and `$expectedUnverifiable` lists three names (Microsoft.IdentityModel.Clients.ActiveDirectory, System.Linq.AsyncEnumerable, netstandard).
2. For any pair, compare the config's `newVersion` with the only version any csproj declares for that assembly. Example: TaskMaster/app.config lines 94-95 redirect Azure.Core to 1.62.0.0; the only csproj Reference version anywhere is 1.63.0.0 (UtilitiesCS/UtilitiesCS.csproj line 51 and nine test csproj; research 1 section 2.2) and the only HintPath folder is Azure.Core.1.63.0.
3. Set `$expectedDebt = @()` in the test and run the Pester gate: the It fails and the `-Because` text lists the 15 observed pairs.

Expected:
Every `bindingRedirect` names, in its `newVersion` and in the upper bound of its `oldVersion` range, an assembly version that the build actually deploys, so the known-mismatch list is empty and the only unverifiable name is the deliberate netstandard hardening from #879.

Actual (all figures from research 1 section 3 and section 6, and research 2 section 7; independently re-counted in this session by Grep over the root-level app.config files):
- 15 pairs name versions that are not deployed. They occur as 137 redirect entries across 14 app.config files (per-file counts: Tags 13, TaskTree 13, TaskVisualization 13, QuickFiler 12, TaskMaster 12, ToDoModel 12, VBFunctions.Test 9, Tags.Test 8, TaskTree.Test 8, QuickFiler.Test 9, TaskMaster.Test 9, TaskVisualization.Test 9, ToDoModel.Test 9, UtilitiesCS.Test 1).
- 13 configs carry a `dependentAssembly` block for Microsoft.IdentityModel.Clients.ActiveDirectory (ADAL) redirecting to 2.22.0.0. No csproj references it, no packages.config installs it, no package folder exists for it and the solution-wide ResolveAssemblyReferences log from #825 records no dependency on it. The block is dead configuration.
- 15 configs (every root-level config except SVGControl and SVGControl.Test) carry a System.Linq.AsyncEnumerable redirect to 10.0.0.7, but no project installs or references that assembly. It is requested: System.Linq.Async 7.0.1 and System.Interactive.Async 7.0.1 (installed in UtilitiesCS, QuickFiler, ToDoModel, TaskMaster, UtilitiesCS.Test) reference System.Linq.AsyncEnumerable 10.0.0.6, and the #825 ResolveAssemblyReferences log records "Could not locate the assembly System.Linq.AsyncEnumerable, Version=10.0.0.7" with no copy in any packages or bin folder (research 1 section 6 row 2; research 2 section 2.2).
- The pairs are latent today: no production or test code path is known to construct a Graph, MSAL or Azure client (research 1 section 5.4), and the test suite passes under the stale configs. Each becomes a FileNotFoundException or FileLoadException the first time a dependency requests the assembly in the Outlook host, which is how #418 happened.

Logs / Screenshots:
- [ ] Attached minimal logs or screenshot
- Snippet: #953 plan task P2-T16 and its evidence artifact p2-t16-known-debt-followup.2026-10-02T03-54.md under the #953 feature folder (137 entries, matching research 1 section 3).

## Scope & Non-Goals

The following scope decisions were made by the orchestrator for this item and are binding on the plan.

- In scope:
  1. The 15 stale pairs: correct each `bindingRedirect` in place to the form oldVersion equal to "0.0.0.0-" followed by the corrected version, newVersion equal to the corrected version, where the corrected version is the csproj Reference version (research 1 section 3, "Corrected values" column), in these fourteen files: `Tags/app.config`, `TaskTree/app.config`, `TaskVisualization/app.config`, `QuickFiler/app.config`, `TaskMaster/app.config`, `ToDoModel/app.config`, `VBFunctions.Test/app.config`, `Tags.Test/app.config`, `TaskTree.Test/app.config`, `QuickFiler.Test/app.config`, `TaskMaster.Test/app.config`, `TaskVisualization.Test/app.config`, `ToDoModel.Test/app.config`, `UtilitiesCS.Test/app.config`. No csproj change for these pairs; no block removal.
  2. ADAL: delete the dead Microsoft.IdentityModel.Clients.ActiveDirectory `dependentAssembly` block from every config that carries it (research 1 section 6 row 1 lists the carriers). Twelve carriers are already in the list above; the deletion adds `UtilitiesCS/app.config` to the write set.
  3. System.Linq.AsyncEnumerable: install NuGet package System.Linq.AsyncEnumerable 10.0.12 in the five projects that install System.Linq.Async (UtilitiesCS, QuickFiler, ToDoModel, TaskMaster, UtilitiesCS.Test): one packages.config entry and one aliased csproj Reference each (Proposed Fix part C). Set the System.Linq.AsyncEnumerable redirect in all 15 carrier configs to the assembly version read from the restored DLL. This is the same defect class as the 15 pairs (a redirect naming an assembly that is not deployed); research 1 recommended promoting it separately and the orchestrator overrode that recommendation with research 2 as the basis.
  4. netstandard: unchanged. The downward redirect at TaskMaster/app.config lines 38-39 is the deliberate #879 hardening and remains the only name in `$expectedUnverifiable`.
  5. Regression test (Bugfix Workflow, test first): edit the existing It block at line 275 of `tests/scripts/dependencies/BindingRedirectVerification.Tests.ps1` so that `$expectedDebt = @()`, `$expectedUnverifiable = @('netstandard')`, and the collection comparison at line 331 becomes a count assertion with a diagnostic `-Because`. Run it red before the config and project edits, green after.
  6. Two additional It blocks in the same test file: (a) a range guard for the 16 corrected names (the 15 pairs plus System.Linq.AsyncEnumerable), (b) an Aliases durability guard for the System.Linq.AsyncEnumerable Reference. The file stays under the 500-line ceiling.
  7. Toolchain: PowerShell gates through the PoshQC MCP tools, and, because csproj files change, the full CLAUDE.md C# toolchain including the coverage test route; after the build the System.Linq.AsyncEnumerable assembly must be present in the Debug output of the five installing projects.
  8. One manual verification acceptance criterion: the #418 WinForms designer path for SVGControl PictureBoxSVG plus an Outlook add-in start check, using the runbook `docs/features/active/2026-10-02-remaining-stale-binding-redirect-pairs-973/runbooks/verify-designer-and-addin-load.runbook.md` (authored separately from this spec).
- Out of scope / non-goals (paths in this list are deliberately unbackticked because they are not written):
  - No C# source file (any file with the .cs extension) changes. The aliased Reference keeps the BCL assembly's types out of compile scope, so no call site changes (research 2 section 5.4).
  - SVGControl/app.config and SVGControl.Test/app.config are not written. They carry none of the 15 pairs, no ADAL block and no System.Linq.AsyncEnumerable block, and neither project loads System.Linq.Async (research 1 section 3; research 2 section 7).
  - scripts/dependencies/BindingRedirectVerification.psm1 and scripts/dependencies/PackageGraph.psm1 are not changed. The detector's rule (compare newVersion to csproj Reference versions; ignore oldVersion; report unknown names as unverifiable) stays as #953 defined it.
  - No new PowerShell file. Both added It blocks go into the existing test file.
  - No csproj or packages.config other than the ten named in the Write Set is changed; in particular no csproj change is made for any of the 15 pairs, and no package is installed in the ten projects that only receive System.Linq.Async.dll by ProjectReference copy (research 2 section 2.1).
  - Migrating call sites from System.Linq.Async to the BCL System.Linq.AsyncEnumerable API (the direction the Ix.NET package README recommends) is not attempted; research 2 section 6 item 4 rejects it for this item.
  - Adding System.Linq.AsyncEnumerable to the semver-major ignore list in .github/dependabot.yml (and the paired expectation in tests/scripts/dependencies/DependabotConfig.Tests.ps1) is optional hardening noted under Rollout & Follow-up, not part of this change.
  - Removing the unused deep `using Microsoft.Graph.*` directives found by research 1 section 5.4, and cleaning superseded package folders in the primary checkout, are follow-ups only.
- Explicitly excluded systems, integrations, or datasets: Visual Studio's own devenv.exe.config (the designer host configuration), NuGet.org package contents other than System.Linq.AsyncEnumerable 10.0.12, and the primary checkout's restored packages tree (host-local, gitignored).

## Root Cause Analysis

### Drift mechanism for the 15 pairs (research 1 section 2.5)

NuGet's packages.config client rewrites binding redirects only in the project it is updating. A config that redirects an assembly its own project does not install is never rewritten. The repository shows the pattern exactly:

- Microsoft.Bcl.Memory is 10.0.0.12 in the five configs whose packages.config lists it (UtilitiesCS, QuickFiler, TaskMaster, ToDoModel, UtilitiesCS.Test) and 10.0.0.7 in the ten that carry it only transitively.
- Microsoft.Bcl.Numerics is 10.0.0.12 only in UtilitiesCS/app.config, the one project that installs it, and 10.0.0.5 in the other twelve carriers.
- The six production configs of projects that install none of Azure.Core, System.ClientModel, MSAL or Diagnostics.Abstractions (Tags, TaskTree, TaskVisualization, QuickFiler, TaskMaster, ToDoModel) are stale for all four; the ten projects that install them are current.
- The seven IdentityModel assemblies are current only in UtilitiesCS and UtilitiesCS.Test, the two projects that install them.

Every csproj that references one of the 15 assemblies declares exactly one version, the current one, and its HintPath folder agrees (research 1 section 2.2). The stale side is therefore always the config, never the csproj, and the remedy is a config edit with no project change. This is the "package updates advanced deployed versions without a matching sweep of the redirects" cause recorded in the issue, shared with #953 and #418. The #953 gate detects the drift; it does not prevent it, and because it ignores `oldVersion` it cannot see a range that excludes the version it accepts.

### Why the current state is the failing one and the correction is not a redirect-down (research 1 section 5)

A correction raises `newVersion` from a version that is not on disk to the one version that is on disk and that every csproj compiles against. The nuspec evidence in research 1 section 5.2 shows each installed consumer's declared lower bound at or below the installed version (for example Microsoft.Kiota.Authentication.Azure 2.1.2 declares Azure.Core 1.50.0; Microsoft.Identity.Client 4.90.1 declares Microsoft.IdentityModel.Abstractions 8.14.0; Microsoft.Graph.Core 4.0.1 declares Microsoft.IdentityModel.Protocols.OpenIdConnect 8.18.0). Under the current configs each of those requests is redirected into a version that does not exist (the #418 class), and a request that falls between the stale upper bound and the installed version is left unredirected, which on .NET Framework is a strict bind that also fails. Under the corrected configs every such request binds to the installed assembly. No installed assembly carries a reference above the installed version, so no correction can be "below a compile-time reference".

### ADAL block

The Microsoft.IdentityModel.Clients.ActiveDirectory block (redirect 0.0.0.0-2.22.0.0 to 2.22.0.0) survives from a package that is no longer installed anywhere: no csproj Reference, no packages.config entry, no package folder, and no entry in the #825 solution-wide ResolveAssemblyReferences log (research 1 section 6 row 1). The detector lists it as unverifiable because no csproj declares a version for the name. It can only leave the unverifiable list by deletion; there is no package to reference.

### System.Linq.AsyncEnumerable (research 1 section 6 row 2; research 2 sections 2 and 5)

When System.Linq.Async moved to 7.x it made the BCL package System.Linq.AsyncEnumerable a dependency (nuspec floor 10.0.6 for net48). The repository's five installing projects were advanced to System.Linq.Async 7.0.1 without that package being installed, so NuGet wrote a redirect to 10.0.0.7 while no DLL was ever deployed. The build succeeds because every operator the repository calls is defined inside lib\net48\System.Linq.Async.dll itself; the missing assembly is only requested at run time by code paths that forward into the BCL implementation. Which members forward is unverified (research 2 risk 4); the test suite passes without the DLL.

The install cannot take NuGet's default form. System.Linq.Async 7.0.1 ships a reference assembly (ref\net48) that no longer defines the public type System.Linq.AsyncEnumerable and a runtime assembly (lib\net48) that still defines it with 334 documented members for binary compatibility. Every HintPath in the repository points at lib\net48, so the five projects compile against the runtime assembly. A global Reference to the BCL System.Linq.AsyncEnumerable.dll would put a second public type with the same metadata name in scope, and every shared-name extension call (ToAsyncEnumerable, ToListAsync, ToArrayAsync, CountAsync, Select; 60 production call lines listed in research 2 section 5.3) becomes CS0121, with CS0433 for any type-name use. This is the ambiguity Ix.NET fixed for PackageReference consumers by renaming the type in the reference assembly only (dotnet/reactive issue 2291 and pull requests 2292 and 2293), which does not reach a packages.config consumer compiling against lib. Hedge carried from research 2: the member inventory was read from the packages' shipped XML documentation files, not from the IL, and no restore or compile was run in the research session; the compile proof is therefore an acceptance criterion of this spec (AC11), not an assumption.

### Why the netstandard entry stays

netstandard.dll is a .NET Framework facade in the GAC at 2.0.0.0 only; no csproj references it with a version and no package deploys it. The redirect 0.0.0.0-2.1.0.0 to 2.0.0.0 in TaskMaster/app.config is deliberately downward: it is the #879 declarative hardening for Deedle and FSharp.Core requesting netstandard 2.1.0.0 and was verified to be honoured in the Outlook host (the #879 spec, lines 260-265). It is correct that the detector cannot verify it, and it must remain recorded.

## Proposed Fix

### Design summary (what changes where):

Invariant established by this change: after the change, every `bindingRedirect` in every app.config directly under a root-level directory either (i) names, in both its `oldVersion` upper bound and its `newVersion`, an assembly version that some csproj Reference Include declares for the same assembly name and that the build therefore copies to an output directory, or (ii) is the single deliberate netstandard exception in `TaskMaster/app.config`; and no `dependentAssembly` block remains for an assembly that nothing deploys and nothing requests.

Trace of one accepted value through the current and the fixed configuration (the path with no guard between request and bind):

1. Request point: Microsoft.Kiota.Authentication.Azure 2.1.2, installed in UtilitiesCS and deployed beside TaskMaster.dll, was compiled against Azure.Core 1.50.0 and requests Azure.Core, Version=1.50.0.0 when first used in the Outlook host (research 1 section 5.2).
2. Bind point today: the Outlook AppDomain applies TaskMaster.dll.config (copied from `TaskMaster/app.config`, the only production config that is honoured at run time; research 1 section 4). Its Azure.Core block reads oldVersion 0.0.0.0-1.62.0.0, newVersion 1.62.0.0, so the request is redirected to 1.62.0.0. No Azure.Core 1.62.0.0 exists in the output directory (every csproj HintPath is Azure.Core.1.63.0). The loader raises FileNotFoundException (or FileLoadException when a differently versioned file is found), the same failure class as #418.
3. Absorption point today: nothing absorbs it. There is no AssemblyResolve handler for this family, and the first caller is whatever code path constructs a Graph or MSAL client; research 1 section 5.4 found no such call today, which is why the defect is latent rather than observed.
4. After the fix: the same block reads oldVersion 0.0.0.0-1.63.0.0, newVersion 1.63.0.0; the 1.50.0.0 request is redirected to the one Azure.Core on disk and binds. The same trace holds for System.Linq.AsyncEnumerable with the request point System.Linq.Async 7.0.1 (reference to 10.0.0.6), the bind point today a redirect to an absent 10.0.0.7, and after the fix a redirect to the installed assembly version with the DLL present in TaskMaster's output directory.

Neither half suffices alone: correcting the redirect without deploying the assembly (the System.Linq.AsyncEnumerable case) still binds to a missing file, and deploying an assembly while leaving the redirect at a version that is not on disk (any of the 15 pairs) still redirects into a missing version.

The change has five parts:

- Part A, the 15 pairs. In the fourteen files listed under Scope item 1, edit each affected `bindingRedirect` element in place to oldVersion "0.0.0.0-" plus the corrected version, newVersion the corrected version. Corrected values (research 1 section 3): Azure.Core 1.63.0.0; Microsoft.Bcl.Memory 10.0.0.12; Microsoft.Bcl.Numerics 10.0.0.12; Microsoft.Extensions.Diagnostics.Abstractions 10.0.0.12; Microsoft.Identity.Client 4.90.1.0; Microsoft.Identity.Client.Extensions.Msal 4.90.1.0; Microsoft.IdentityModel.Abstractions, .JsonWebTokens, .Logging, .Protocols, .Protocols.OpenIdConnect, .Tokens, .Validators and System.IdentityModel.Tokens.Jwt all 8.23.0.0; System.ClientModel 1.16.0.0. Anchor every edit on the `assemblyIdentity` name text, never on a line number, because Part B shifts line numbers. Preserve CRLF line endings and touch only the attribute values.
- Part B, ADAL deletion. Delete the four-line `dependentAssembly` block whose `assemblyIdentity` name is Microsoft.IdentityModel.Clients.ActiveDirectory from every config that carries it. Carriers per research 1 section 6 row 1 (positions before any edit): VBFunctions.Test lines 50-53, TaskVisualization 62-65, QuickFiler.Test 62-65, Tags 62-65, TaskTree 62-65, QuickFiler 66-69, ToDoModel 67-70, TaskMaster.Test 70-73, TaskMaster 74-77, UtilitiesCS.Test 62-65, UtilitiesCS 66-69, TaskVisualization.Test 62-65, ToDoModel.Test 62-65. This adds `UtilitiesCS/app.config` to the write set; Tags.Test and TaskTree.Test carry no ADAL block.
- Part C, System.Linq.AsyncEnumerable install. In each of `UtilitiesCS/packages.config` (after the System.Linq.Async line, currently line 97), `QuickFiler/packages.config` (after line 47), `ToDoModel/packages.config` (after line 20), `TaskMaster/packages.config` (after line 43) and `UtilitiesCS.Test/packages.config` (after line 91) insert the entry `<package id="System.Linq.AsyncEnumerable" version="10.0.12" targetFramework="net481" />`, which keeps NuGet's case-insensitive id order. Restore (scripts/vscode/Invoke-Restore.ps1 route, msbuild /t:Restore with RestorePackagesConfig, or nuget restore as CI does). Read the restored DLL's AssemblyName.Version with one `pwsh -NoProfile -Command` invocation calling System.Reflection.AssemblyName.GetAssemblyName on the absolute path of packages\System.Linq.AsyncEnumerable.10.0.12\lib\net462\System.Linq.AsyncEnumerable.dll and record it as evidence (expected 10.0.0.12 by the .NET 10 servicing convention that places the patch in the fourth component; research 2 section 3 marks this as inference). Then insert, immediately after the closing tag of the System.Linq.Async Reference in each of `UtilitiesCS/UtilitiesCS.csproj` (after line 400), `QuickFiler/QuickFiler.csproj` (after line 185), `ToDoModel/ToDoModel.csproj` (after line 87), `TaskMaster/TaskMaster.csproj` (after line 246) and `UtilitiesCS.Test/UtilitiesCS.Test.csproj` (after line 898), the following element with the recorded version in place of VERSION:

```xml
    <!-- Aliased on purpose (issue #973): the project compiles against lib\net48\System.Linq.Async.dll,
         which still defines a public System.Linq.AsyncEnumerable type for binary compatibility; a global
         reference to the BCL assembly of the same type name makes every shared operator call CS0121
         (and any type-name use CS0433). The alias keeps the assembly deployed and resolvable without
         putting its types in scope; no source file declares extern alias SystemLinqAsyncEnumerable. -->
    <Reference Include="System.Linq.AsyncEnumerable, Version=VERSION, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a, processorArchitecture=MSIL">
      <HintPath>..\packages\System.Linq.AsyncEnumerable.10.0.12\lib\net462\System.Linq.AsyncEnumerable.dll</HintPath>
      <Aliases>SystemLinqAsyncEnumerable</Aliases>
    </Reference>
```

  Finally edit the System.Linq.AsyncEnumerable `bindingRedirect` in all 15 carrier configs (the fourteen Part A files plus `UtilitiesCS/app.config`; current positions in research 2 section 7, which shift by four lines in the thirteen files that lose an ADAL block) to oldVersion "0.0.0.0-" plus the recorded version, newVersion the recorded version. The csproj edit and the redirect edit must land in the same commit: a csproj Reference without the redirect edit produces 15 new gate findings (newVersion 10.0.0.7 not in the Reference set).
  Fallback if AC11 fails with CS0121 or CS0433 despite the alias: do not fall back to a global Reference (proven to fail) and do not edit call sites; revert the ten manifest and project edits and the 15 redirect edits of Part C, restore System.Linq.AsyncEnumerable to `$expectedUnverifiable`, record the observed diagnostics under the feature's qa-gates evidence folder, and report the install as blocked for separate promotion. Parts A, B, D and E remain deliverable on their own.
- Part D, the regression test and two guards, all in `tests/scripts/dependencies/BindingRedirectVerification.Tests.ps1`:
  - In the It beginning at line 275 ('reports exactly the recorded known-debt set and unverifiable set over every app.config against every csproj Reference'): `$expectedDebt = @()`; `$expectedUnverifiable = @('netstandard')`; replace the collection comparison at line 331 with `$actualDebt.Count | Should -Be 0 -Because ('every bindingRedirect newVersion must equal a csproj Reference version; observed: ' + ($actualDebt -join '; '))`; add a `-Because` to the unverifiable assertion at line 332 that joins `$actualUnverifiable` so a failure names the extra names. Keep the examined-count guard at line 330 and the Fizzler/Unsafe exclusion at line 333 unchanged. The count form is used because piping an empty array into `Should -Be @()` depends on Pester's empty-pipeline handling (research 1 section 7).
  - New It (a), range guard, suggested name 'bounds every corrected redirect range at its newVersion across the repository app.config files': enumerate every app.config directly under a root-level directory (same enumeration as the Fizzler It at lines 248-252), parse each with ConvertFrom-AppConfigText, select records whose Name is one of the 16 corrected names (the 15 pair names plus System.Linq.AsyncEnumerable) and whose NewVersion is non-empty, and assert that every such record has OldVersion equal to the string "0.0.0.0-" concatenated with its NewVersion and that its NewVersion is one of the versions the csproj Reference map (ConvertTo-ReferenceVersionMap over every csproj directly under a root-level directory, the same enumeration as lines 283-287) declares for that Name; assert non-vacuity by requiring the examined record count to be greater than zero and every one of the 16 names to have been observed at least once. This mirrors the Fizzler check at line 268 (which pins the version as well as the range) and proves the range half of each fix, which the detector cannot see. The version clause is what makes the block red before the sweep: today every stale redirect already reads oldVersion "0.0.0.0-X" for its own stale newVersion X, so a range-only assertion passes before and after the change (planner amendment, 2026-10-02; see Planner Amendments).
  - New It (b), alias durability guard, suggested name 'carries an Aliases child on every System.Linq.AsyncEnumerable project Reference': enumerate every csproj directly under a root-level directory (same enumeration as lines 283-287), match the raw text with a single-line regular expression capturing each Reference element whose Include attribute begins with "System.Linq.AsyncEnumerable," through its closing tag, and assert that every captured element contains an Aliases child whose text is SystemLinqAsyncEnumerable and that the set of project directories carrying such a Reference is exactly UtilitiesCS, QuickFiler, ToDoModel, TaskMaster and UtilitiesCS.Test. The raw-text match is required because the PackageGraph parser models only Reference and HintPath lines (research 2 section 6 item 2).
- Part E, netstandard. No edit to the netstandard block at TaskMaster/app.config lines 38-39.

### Boundaries and invariants to preserve:

- The detector module's contract is unchanged: newVersion compared to csproj Reference versions, oldVersion ignored, unknown names reported as unverifiable, examined count equal to the raw bindingRedirect count. The new range guard lives in the test file, not the module.
- `TaskMaster/app.config` keeps every block it has today except the ADAL block; in particular the netstandard block and every block for a family not among the 16 names are byte-identical after the change.
- No compile result changes: the aliased Reference is invisible to name lookup because no source file declares an extern alias (research 2 section 5.4). The 18 narrow `#pragma warning disable CS0618` regions around obsolete System.Linq.Async operators (research 2 section 2.3) are untouched.
- The System.Linq.Async and System.Interactive.Async References stay at Version 7.0.0.0 with their lib\net48 HintPaths. Nothing points at a ref folder.
- Repository convention for packages.config projects is preserved: a package is installed only in projects that compile against it; the ten ProjectReference-only dependents receive the DLL by copy.
- Line endings of every edited app.config stay CRLF; each config diff consists only of edited redirect attribute lines and deleted ADAL blocks.

### Dependencies or blocked work:

- #953 is merged and the recording test exists on this branch (verified by reading the test file in this session). No remaining dependency on #953.
- The item worktree has no restored packages tree. Part C requires a NuGet restore with network access to api.nuget.org before the assembly-version read and before any build gate.
- The manual runbook `docs/features/active/2026-10-02-remaining-stale-binding-redirect-pairs-973/runbooks/verify-designer-and-addin-load.runbook.md` is authored separately; AC18 cannot be executed until it exists.
- A concurrently open Dependabot branch touching the same app.config, csproj or packages.config lines would conflict (dependabot-repair.yml stages all three file kinds on bot branches); see Risks.

### Implementation strategy (what changes, not sequencing):

#### Files/modules to change:

See `## Write Set`. Summary: fifteen app.config files (fourteen for the pairs plus `UtilitiesCS/app.config` for the ADAL deletion and the System.Linq.AsyncEnumerable redirect), five packages.config files, five csproj files, one Pester test file, plus the feature's evidence artifacts and the separately authored runbook.

#### Functions/classes/CLI commands impacted:

- No C# type, method or command changes.
- No exported PowerShell function changes. The test file gains two It blocks and edits three literals and two assertions inside an existing It.
- MSBuild: ResolveAssemblyReferences now resolves one additional primary reference per installing project (copy-local by default for a HintPath reference) and dependents resolve it through the referenced project's output directory.

#### Data flow and validation changes:

- Build-time: each project's app.config is copied to the output as the assembly's .dll.config (every csproj carries None Include app.config; research 1 section 2.1); no project auto-generates redirects, so nothing overwrites the edits.
- Run-time: the Outlook AppDomain reads TaskMaster.dll.config; the vstest host reads each test assembly's .dll.config. Requests for the 16 corrected families now resolve to files that exist.
- Validation: the Pester gate (CI Pester workflow, tests/scripts/dependencies with coverage over scripts/dependencies) now fails on any new stale pair, any range that excludes its newVersion for the 16 names, and any System.Linq.AsyncEnumerable Reference that loses its alias.

#### Error handling and logging updates:

None in product code. Failure modes the plan must make observable (research 2 section 11): CS0121/CS0433 means the alias is missing or misspelt; 15 gate findings naming System.Linq.AsyncEnumerable|10.0.0.7 means the csproj landed without the redirect edit or with a version string different from the DLL's; a MissingReference or OrphanedHintPath finding from the dependency repair tooling means a packages.config/csproj pair is incomplete.

#### Rollback/feature-flag considerations (if applicable):

No feature flag. Rollback is a revert of the single commit (or the Part C fallback above). The change is deploy-time configuration plus one additional copied DLL; no data or settings migration.

### Technical specifications (interfaces/contracts):

#### Inputs/outputs and formats:

- app.config: the existing assemblyBinding schema; attribute edits and whole-block deletions only.
- packages.config: one package element per installing project, id System.Linq.AsyncEnumerable, version 10.0.12, targetFramework net481.
- csproj: one Reference element per installing project with Include naming the recorded assembly version, culture neutral, PublicKeyToken b03f5f7f11d50a3a, a HintPath into the package's lib\net462 folder (the nearest .NET Framework folder the package ships; research 2 section 3), and an Aliases child.
- Evidence artifacts: Markdown with Timestamp, Command and EXIT_CODE fields per the evidence conventions skill; coverage and test evidence as projections only (CLAUDE.md, Committed Test Evidence Format).

#### Required configuration keys and defaults:

None added.

#### Backward-compatibility expectations:

No public API change. Every correction widens the set of requests that bind successfully and narrows none (research 1 section 5.1). Dependents that copy System.Linq.Async.dll gain a System.Linq.AsyncEnumerable.dll beside it.

#### Performance constraints (latency/throughput/memory):

None. One additional assembly is probed only when a System.Linq.Async code path forwards into it.

## Write Set

Repository files created or modified by this change, one per line:

- `Tags/app.config`
- `TaskTree/app.config`
- `TaskVisualization/app.config`
- `QuickFiler/app.config`
- `TaskMaster/app.config`
- `ToDoModel/app.config`
- `UtilitiesCS/app.config`
- `VBFunctions.Test/app.config`
- `Tags.Test/app.config`
- `TaskTree.Test/app.config`
- `QuickFiler.Test/app.config`
- `TaskMaster.Test/app.config`
- `TaskVisualization.Test/app.config`
- `ToDoModel.Test/app.config`
- `UtilitiesCS.Test/app.config`
- `UtilitiesCS/packages.config`
- `QuickFiler/packages.config`
- `ToDoModel/packages.config`
- `TaskMaster/packages.config`
- `UtilitiesCS.Test/packages.config`
- `UtilitiesCS/UtilitiesCS.csproj`
- `QuickFiler/QuickFiler.csproj`
- `ToDoModel/ToDoModel.csproj`
- `TaskMaster/TaskMaster.csproj`
- `UtilitiesCS.Test/UtilitiesCS.Test.csproj`
- `tests/scripts/dependencies/BindingRedirectVerification.Tests.ps1`

Feature-folder artifacts created by this change (all under the feature folder):

- `docs/features/active/2026-10-02-remaining-stale-binding-redirect-pairs-973/runbooks/verify-designer-and-addin-load.runbook.md` (authored separately)
- the evidence artifacts named in Test Strategy

## Assumptions, Constraints, Dependencies

- Assumptions (environment, data, access): network access to api.nuget.org for the restore; the restored System.Linq.AsyncEnumerable 10.0.12 package ships lib\net462 as its only .NET Framework folder (research 2 section 3, verified from the nuspec); its assembly version is expected to be 10.0.0.12 but is read from the DLL, not assumed; the Pester job in CI runs tests/scripts/dependencies with an 80 percent line floor over scripts/dependencies, unaffected because the module does not change.
- Constraints (budget, performance, compatibility): PowerShell budget is zero production files and one test file (direct mode); the test file must stay under the 500-line ceiling (335 lines today by a Grep line count; a Read-tool count reports 336); the C# toolchain is mandatory because csproj files change; CSharpier ignores csproj, packages.config and app.config through .csharpierignore, so the format gate is a recorded no-op for this write set; the four net462 dependencies of the new package are already installed at exactly the declared lower bounds in all five projects, so no cascade (research 2 section 4, Count C).
- External dependencies (services, libraries, releases): System.Linq.AsyncEnumerable 10.0.12 (published 2026-09-08); System.Linq.Async 7.0.1 and System.Interactive.Async 7.0.1 (already installed; nuspec floor System.Linq.AsyncEnumerable 10.0.6).

## Data / API / Config Impact

- User-facing or API changes: none.
- Data or migration considerations: none.
- Logging/telemetry updates (if any): none.
- Compatibility notes (CLI flags, config schemas, versioning): the only behavioural change is that assembly requests for the 16 corrected families, which today fail or are left unredirected, now bind to the deployed assembly. Config correctness is now enforced for both halves of each redirect by the Pester gate.

## Test Strategy

Seeded from issue (both are carried by acceptance criteria below; the checkboxes live only in the Acceptance Criteria section):

- "For each pair, correct the redirect or remove it when nothing references the assembly. Remove the pair from the known list in the same change, so the test proves each fix." Resolution: correct all 15 pairs (removal would strip TaskMaster/app.config, the only honoured production config; research 1 section 4); remove the ADAL block because nothing deploys or requests it; the known list becomes empty in the same change (AC1-AC6).
- "Re-test the #418 designer path for PictureBoxSVG after the sweep." Carried as the manual AC18, with the insensitivity statement.

- Regression tests to add or update: the existing repository-level It at line 275 of `tests/scripts/dependencies/BindingRedirectVerification.Tests.ps1` is the regression test (Part D). Bugfix Workflow ordering is binding: the literal edits are made first and the test is observed failing before any app.config, packages.config or csproj edit. Because Pester stops an It at its first failing Should, the two halves of the fix get separate fail-before observations:
  1. `docs/features/active/2026-10-02-remaining-stale-binding-redirect-pairs-973/evidence/regression-testing/binding-redirect-gate-fail-before.md`: run after the Part D literal edits and before any config or project edit; non-zero EXIT_CODE; the failure text lists the 15 observed pairs from the count assertion's `-Because`.
  2. `docs/features/active/2026-10-02-remaining-stale-binding-redirect-pairs-973/evidence/regression-testing/binding-redirect-gate-fail-before-unverifiable.md`: run after Parts A and B and before Part C; the count assertion passes and the unverifiable assertion fails naming System.Linq.AsyncEnumerable. This is the negative control that shows the install half is load-bearing: the sweep alone cannot make the test green.
  3. `docs/features/active/2026-10-02-remaining-stale-binding-redirect-pairs-973/evidence/regression-testing/binding-redirect-gate-pass-after.md`: run after Part C; EXIT_CODE 0; all It blocks in the file pass.
  Both new It blocks (a) and (b) are expected to be red at observation 1 as well ((a) because the 15 pair names carry newVersion values no csproj Reference declares and System.Linq.AsyncEnumerable has no csproj Reference at all, so its version clause fails; the range clause alone would pass, since every stale redirect already bounds its range at its own stale newVersion; (b) because no csproj carries the Reference, which fails its exact-set assertion), red at observation 2 as well ((a) because System.Linq.AsyncEnumerable still has no csproj Reference; (b) for the same reason) and green at observation 3; record their outcomes in the same three artifacts.
- Unit tests (pytest) for the fixed behavior and boundaries: not applicable (no Python). The in-memory fixture tests in the same file (lines 100-242) continue to pin the detector's contract and are not edited.
- Edge cases and negative scenarios (invalid inputs, missing data, boundary values): a config whose block for one of the 16 names has a correct newVersion but a range that excludes it (the detector passes it; guard (a) fails it); a csproj that regains a plain System.Linq.AsyncEnumerable Reference after a package update (guard (b) fails before the C# build does); a csproj Include version string that differs from the DLL (the gate reports 15 findings against the redirects, or MSB3277/ResolveAssemblyReferences warnings in the build log).
- Error handling and logging verification: none in product code; the build logs and gate outputs are the verification surface.
- Coverage impact and targets for changed lines/modules: no C# production line changes, so no coverage movement is expected. Capture `docs/features/active/2026-10-02-remaining-stale-binding-redirect-pairs-973/evidence/baseline/mstest-coverage-baseline.md` from the coverage route on this branch before any edit, and compare the post-change projection against it. PowerShell coverage over scripts/dependencies is unchanged (module not edited).
- Toolchain commands to run (format, lint, type-check, test):
  - PowerShell, through the PoshQC MCP tools against the worktree path: format, analyze, test; projections at `docs/features/active/2026-10-02-remaining-stale-binding-redirect-pairs-973/evidence/qa-gates/poshqc-format.md`, `docs/features/active/2026-10-02-remaining-stale-binding-redirect-pairs-973/evidence/qa-gates/poshqc-analyze.md`, `docs/features/active/2026-10-02-remaining-stale-binding-redirect-pairs-973/evidence/qa-gates/poshqc-test.md`.
  - C#, in CLAUDE.md order, after the restore: `dotnet tool run csharpier check .`; `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`; `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`; the `test: MSTest with Coverage (Koverage)` VS Code task (or Invoke-MSTestWithCoverage.ps1 under scripts/vscode directly). Each msbuild gate is run with a file logger at normal verbosity so the projection can state the count of the literal text Skipping target "CoreCompile" (which must be zero for a genuine Rebuild) and the final warning and error totals. Projections: `docs/features/active/2026-10-02-remaining-stale-binding-redirect-pairs-973/evidence/qa-gates/csharpier-check.md`, `docs/features/active/2026-10-02-remaining-stale-binding-redirect-pairs-973/evidence/qa-gates/msbuild-analyzers.md`, `docs/features/active/2026-10-02-remaining-stale-binding-redirect-pairs-973/evidence/qa-gates/msbuild-treatwarningsaserrors.md`, `docs/features/active/2026-10-02-remaining-stale-binding-redirect-pairs-973/evidence/qa-gates/mstest-coverage-projection.md` (package-level JaCoCo projection plus the one-line first-party summary) and `docs/features/active/2026-10-02-remaining-stale-binding-redirect-pairs-973/evidence/qa-gates/mstest-test-results-summary.md` (summary derived from the trx). No raw trx, Cobertura or .coverage document is added to the repository.
  - Assembly version evidence: `docs/features/active/2026-10-02-remaining-stale-binding-redirect-pairs-973/evidence/other/system-linq-asyncenumerable-assembly-version.md` recording the exact pwsh command, the DLL path and the Version it printed.
  - Deployment evidence: `docs/features/active/2026-10-02-remaining-stale-binding-redirect-pairs-973/evidence/qa-gates/system-linq-asyncenumerable-bin-presence.md` recording, after the Rebuild, a Test-Path result for System.Linq.AsyncEnumerable.dll in the bin\Debug folder of each of UtilitiesCS, QuickFiler, ToDoModel, TaskMaster and UtilitiesCS.Test.
- Manual validation steps (if required): AC18, using the separately authored runbook; evidence file named designer-load- followed by the yyyy-MM-ddTHH-mm run timestamp and the .md extension, written under `docs/features/active/2026-10-02-remaining-stale-binding-redirect-pairs-973/evidence/regression-testing/`.

## Planner Amendments

Applied by the atomic planner on 2026-10-02 while authoring `docs/features/active/2026-10-02-remaining-stale-binding-redirect-pairs-973/plan.2026-10-02T22-16.md`, before any implementation task ran. The planning session had no shell clock, so the Last Updated field above is not re-stamped; the plan's revision log carries the same record.

1. AC5, Proposed Fix Part D item (a) and the Test Strategy red-state sentence: the range guard now also asserts that each record's NewVersion is a version the csproj Reference map declares for its Name. Reason: a Grep over the 15 configs in this session showed that every one of the 137 stale entries and the 15 System.Linq.AsyncEnumerable entries already reads oldVersion "0.0.0.0-X" for its own newVersion X, so the range-only assertion the previous text described passes before the sweep as well as after it, and the sentence "the block fails in the AC1 run" could not hold. The added clause is the version pin the Fizzler precedent at line 268 of the test file also carries; it strengthens the criterion and makes the stated red-before observation true.
2. AC14: the clause "line and branch figures are each greater than or equal to the figures in the baseline" is replaced by the CLAUDE.md floors plus a comparison tolerant of 0.10 percentage points when the first-party `lines` denominators are equal, with a single repeat and a one-sentence not-comparable record when they differ. Reason: the collector's first-party figures move between two runs of an identical tree (issue #940 evidence/qa-gates/coverage-final.md, measurements 1 and 2: 85.33 against 85.31 percent lines, 79.72 against 79.71 percent branches, same denominators), so a strict non-decrease over a change that edits no .cs file fails for reasons unrelated to the change. The denominator-equality condition is the falsifiable check that no production line was added or removed.
3. Constraints: the test file's line count is stated as 335 (Grep line count) rather than 336 (Read-tool count); no criterion depends on the figure.

## Acceptance Criteria

Count usage in this section: the figures 15 (stale pairs), 15 (configs carrying the System.Linq.AsyncEnumerable block) and 5 (installing projects) are backed by complete Numeric Derivation Evidence in research 1 and research 2 respectively. The fourteen pair-carrying files are enumerated by name rather than counted, and the ADAL removal is stated as an absence rather than a count, because neither research record carries a derivation block for those two populations.

- [ ] AC1 (regression test, red first). In `tests/scripts/dependencies/BindingRedirectVerification.Tests.ps1`, the It block titled 'reports exactly the recorded known-debt set and unverifiable set over every app.config against every csproj Reference' sets `$expectedDebt = @()` and `$expectedUnverifiable = @('netstandard')`, asserts `$actualDebt.Count | Should -Be 0` with a `-Because` that joins the observed pairs, and keeps the examined-count guard and the Fizzler/Unsafe exclusion unchanged; the artifact `docs/features/active/2026-10-02-remaining-stale-binding-redirect-pairs-973/evidence/regression-testing/binding-redirect-gate-fail-before.md` records a run of that file made after those edits and before any app.config, packages.config or csproj edit, with a non-zero EXIT_CODE and failure text that lists the 15 pairs enumerated in research 1 section 3.
- [ ] AC2 (install half has its own negative control). The artifact `docs/features/active/2026-10-02-remaining-stale-binding-redirect-pairs-973/evidence/regression-testing/binding-redirect-gate-fail-before-unverifiable.md` records a run made after the redirect sweep and the ADAL deletion and before any packages.config or csproj edit, in which the count assertion passes and the unverifiable-set assertion fails with text naming System.Linq.AsyncEnumerable.
- [ ] AC3 (regression test, green after). The artifact `docs/features/active/2026-10-02-remaining-stale-binding-redirect-pairs-973/evidence/regression-testing/binding-redirect-gate-pass-after.md` records a run of the same file after all Write Set edits with EXIT_CODE 0 and every It in the file passing, including the two added It blocks, with `$expectedUnverifiable` equal to exactly `@('netstandard')`.
- [ ] AC4 (the 15 pairs corrected in place). In each of `Tags/app.config`, `TaskTree/app.config`, `TaskVisualization/app.config`, `QuickFiler/app.config`, `TaskMaster/app.config`, `ToDoModel/app.config`, `VBFunctions.Test/app.config`, `Tags.Test/app.config`, `TaskTree.Test/app.config`, `QuickFiler.Test/app.config`, `TaskMaster.Test/app.config`, `TaskVisualization.Test/app.config`, `ToDoModel.Test/app.config` and `UtilitiesCS.Test/app.config`, every bindingRedirect for one of the 15 pair names has newVersion equal to the corrected value in research 1 section 3 and oldVersion equal to "0.0.0.0-" followed by that same value; no dependentAssembly block for those names is removed; no csproj is edited for these pairs. Verified by Grep over those files and by the pass-after run in AC3.
- [ ] AC5 (range guard). `tests/scripts/dependencies/BindingRedirectVerification.Tests.ps1` contains a new It block that, for every app.config directly under a root-level directory and for every record whose Name is one of the 16 corrected names (the 15 pair names plus System.Linq.AsyncEnumerable) and whose NewVersion is non-empty, asserts OldVersion equals "0.0.0.0-" concatenated with NewVersion and asserts NewVersion is one of the versions the csproj Reference map (ConvertTo-ReferenceVersionMap over every csproj directly under a root-level directory) declares for that Name, asserts the examined record count is greater than zero and asserts every one of the 16 names was observed at least once; the block fails in the AC1 run (the 15 pair names carry newVersion values no csproj declares and System.Linq.AsyncEnumerable has no csproj Reference) and passes in the AC3 run.
- [ ] AC6 (ADAL absence). After the change, no app.config directly under a root-level directory contains an assemblyIdentity element whose name attribute is Microsoft.IdentityModel.Clients.ActiveDirectory (Grep of that attribute text over the root-level app.config files returns no match, and the AC3 run's unverifiable set does not contain the name); `UtilitiesCS/app.config` is edited only for that deletion and for the System.Linq.AsyncEnumerable redirect in AC9; SVGControl/app.config and SVGControl.Test/app.config are not in the diff.
- [ ] AC7 (package installed in the 5 projects). Each of `UtilitiesCS/packages.config`, `QuickFiler/packages.config`, `ToDoModel/packages.config`, `TaskMaster/packages.config` and `UtilitiesCS.Test/packages.config` contains exactly one package element with id System.Linq.AsyncEnumerable, version 10.0.12 and targetFramework net481; each of `UtilitiesCS/UtilitiesCS.csproj`, `QuickFiler/QuickFiler.csproj`, `ToDoModel/ToDoModel.csproj`, `TaskMaster/TaskMaster.csproj` and `UtilitiesCS.Test/UtilitiesCS.Test.csproj` contains exactly one Reference element whose Include begins with "System.Linq.AsyncEnumerable, Version=" followed by the version recorded under AC8, with the HintPath shown in the Proposed Fix fenced element, an Aliases child whose text is SystemLinqAsyncEnumerable, and an immediately preceding XML comment stating that the alias exists because a global reference causes CS0121 and CS0433 against the lib\net48 System.Linq.Async assembly; no other csproj or packages.config is in the diff.
- [ ] AC8 (assembly version read, not assumed). `docs/features/active/2026-10-02-remaining-stale-binding-redirect-pairs-973/evidence/other/system-linq-asyncenumerable-assembly-version.md` records the exact pwsh command that called System.Reflection.AssemblyName.GetAssemblyName on the restored lib\net462 System.Linq.AsyncEnumerable DLL, the absolute DLL path and the Version value it printed; the version string in each AC7 csproj Include and the newVersion in each AC9 redirect equal that printed value character for character (expected 10.0.0.12; if the printed value differs, the printed value governs and the artifact says so).
- [ ] AC9 (the 15 System.Linq.AsyncEnumerable redirects corrected). In each of the 15 configs that carry a System.Linq.AsyncEnumerable dependentAssembly block (the fourteen AC4 files plus `UtilitiesCS/app.config`; research 2 Numeric Derivation Evidence, Count A), the bindingRedirect has newVersion equal to the AC8 value and oldVersion equal to "0.0.0.0-" followed by that value; no new dependentAssembly block is added to any config, and SVGControl/app.config and SVGControl.Test/app.config still carry no block for the name.
- [ ] AC10 (alias durability guard). `tests/scripts/dependencies/BindingRedirectVerification.Tests.ps1` contains a second new It block that, over every csproj directly under a root-level directory, matches each Reference element whose Include begins with "System.Linq.AsyncEnumerable," through its closing tag in the raw text, asserts every such element contains an Aliases child with text SystemLinqAsyncEnumerable, and asserts the set of project directories carrying such an element equals exactly UtilitiesCS, QuickFiler, ToDoModel, TaskMaster and UtilitiesCS.Test; the block fails in the AC1 run and passes in the AC3 run.
- [ ] AC11 (compile proof for the aliased Reference; a prose assertion of availability or a citation of research 2 alone is a FAIL). `docs/features/active/2026-10-02-remaining-stale-binding-redirect-pairs-973/evidence/qa-gates/msbuild-analyzers.md` and `docs/features/active/2026-10-02-remaining-stale-binding-redirect-pairs-973/evidence/qa-gates/msbuild-treatwarningsaserrors.md` each record the exact CLAUDE.md msbuild Rebuild command, EXIT_CODE 0, a final total of 0 errors, zero occurrences of the literal text Skipping target "CoreCompile" in the captured file log, and zero occurrences of CS0121, CS0433 and MSB3277 in that log.
- [ ] AC12 (assembly deployed). `docs/features/active/2026-10-02-remaining-stale-binding-redirect-pairs-973/evidence/qa-gates/system-linq-asyncenumerable-bin-presence.md` records, after the AC11 Rebuild, a true Test-Path result for System.Linq.AsyncEnumerable.dll in the bin\Debug folder of each of UtilitiesCS, QuickFiler, ToDoModel, TaskMaster and UtilitiesCS.Test.
- [ ] AC13 (PowerShell gates). `docs/features/active/2026-10-02-remaining-stale-binding-redirect-pairs-973/evidence/qa-gates/poshqc-format.md`, `docs/features/active/2026-10-02-remaining-stale-binding-redirect-pairs-973/evidence/qa-gates/poshqc-analyze.md` and `docs/features/active/2026-10-02-remaining-stale-binding-redirect-pairs-973/evidence/qa-gates/poshqc-test.md` record the PoshQC MCP format, analyze and test runs against the worktree path with passing results, and the test run includes `tests/scripts/dependencies/BindingRedirectVerification.Tests.ps1` with zero failures.
- [ ] AC14 (format gate, test route, coverage, projections only). `docs/features/active/2026-10-02-remaining-stale-binding-redirect-pairs-973/evidence/qa-gates/csharpier-check.md` records `dotnet tool run csharpier check .` with EXIT_CODE 0; `docs/features/active/2026-10-02-remaining-stale-binding-redirect-pairs-973/evidence/qa-gates/mstest-test-results-summary.md` records the `test: MSTest with Coverage (Koverage)` route with zero failed tests; `docs/features/active/2026-10-02-remaining-stale-binding-redirect-pairs-973/evidence/qa-gates/mstest-coverage-projection.md` carries the package-level JaCoCo projection and the one-line first-party summary; that summary's line percentage is at least 80 and its branch percentage at least 75 (the CLAUDE.md floors), and it is compared with the one-line first-party summary in `docs/features/active/2026-10-02-remaining-stale-binding-redirect-pairs-973/evidence/baseline/mstest-coverage-baseline.md` captured on this branch before any edit: when the two `lines` denominators are equal (no production line was added or removed, the expected state because the diff contains no .cs file) the final line percentage and branch percentage are each not lower than the baseline's by more than 0.10 percentage points (run-to-run variance of the collector on an identical tree measured 0.02 percentage points or less on this repository, issue #940 evidence; a real regression of 0.10 points is at least 66 first-party lines); when the denominators differ the run is repeated once, and if they still differ the comparison is recorded as not comparable in one sentence and the floors remain the gate; the diff adds no file with the .xml, .trx or .coverage extension.
- [ ] AC15 (netstandard untouched). After the change, `TaskMaster/app.config` still contains a dependentAssembly block whose assemblyIdentity name is netstandard with bindingRedirect oldVersion 0.0.0.0-2.1.0.0 and newVersion 2.0.0.0, and the diff for that file contains no hunk touching those lines; netstandard remains in `$expectedUnverifiable`.
- [ ] AC16 (footprint and hygiene). The diff of this change contains no file with the .cs extension, neither SVGControl config, no file under scripts/dependencies, and no new PowerShell file; the total line count of `tests/scripts/dependencies/BindingRedirectVerification.Tests.ps1` is under five hundred; each edited app.config diff consists only of changed bindingRedirect attribute lines and deleted ADAL blocks with no line-ending churn (every edited app.config remains CRLF); the Fizzler It block at lines 246-273 and the in-memory fixture Describe blocks are unchanged.
- [ ] AC17 (invariant trace delivered). Reading `TaskMaster/app.config` after the change shows the Azure.Core block at oldVersion 0.0.0.0-1.63.0.0 and newVersion 1.63.0.0 and the System.Linq.AsyncEnumerable block at the AC8 value, and the AC12 artifact shows the DLL in TaskMaster's bin\Debug, so the two traced requests in Proposed Fix (Azure.Core 1.50.0.0 from Microsoft.Kiota.Authentication.Azure; System.Linq.AsyncEnumerable 10.0.0.6 from System.Linq.Async) each redirect to an assembly that exists in the add-in's output directory.
- [ ] AC18 (manual verification; non-regression evidence only). Using `docs/features/active/2026-10-02-remaining-stale-binding-redirect-pairs-973/runbooks/verify-designer-and-addin-load.runbook.md`, on branch bug/remaining-stale-binding-redirect-pairs-973 built in Debug Any CPU with Visual Studio restarted before observing: (i) the #418 WinForms designer path for the SVGControl PictureBoxSVG control opens without a designer load error; and (ii) the Outlook add-in starts and the add-in debug log for that session (the debug log under TaskMaster's bin\Debug logs folder) contains no FileNotFoundException or FileLoadException naming any of the 16 corrected assemblies or Microsoft.IdentityModel.Clients.ActiveDirectory. The evidence file is named designer-load- followed by the yyyy-MM-ddTHH-mm run timestamp and the .md extension under `docs/features/active/2026-10-02-remaining-stale-binding-redirect-pairs-973/evidence/regression-testing/`, with the fields the runbook lists, and it states explicitly that the WinForms designer runs inside devenv.exe and never applies a project app.config, and that PictureBoxSVG's Svg, ExCSS and Fizzler binding chain is disjoint from the corrected families, so a Pass is non-regression evidence only and not proof of the sweep.

## Risks & Mitigations

- Technical or operational risks:
  1. Aliases durability. A future package update applied through nuget.exe update or Dependabot's packages.config path uninstalls and re-adds the Reference element, which is expected to drop the Aliases child; the next build then fails with CS0121 at the 60 production call lines (research 2 risk 2). The in-repo repair pass does not strip it because it rewrites only Reference and HintPath lines.
  2. Dependabot interaction. (a) dependabot-repair.yml stages app.config, csproj and packages.config on bot branches, so an open bot branch touching the same lines conflicts with this change. (b) System.Linq.AsyncEnumerable is not in the semver-major ignore list in .github/dependabot.yml, so the catch-all group will propose 11.0.0 when it reaches general availability (11.0.0-rc.1 is published); a bump that keeps the alias compiles, but a bump that also changes the assembly version makes the 15 redirects stale again unless the bot branch updates them. (c) A future bump of any of the 15 families in one installing project re-creates the drift in the non-installing configs.
  3. Assembly version assumption. 10.0.0.12 is inferred from the servicing convention, not read (research 2 section 3).
  4. Restore precondition. The item worktree has no packages tree; every C# gate and the version read depend on a successful restore with network access.
  5. Partial landing. A csproj Reference without the matching redirect edit, or redirect edits with a version string that differs from the DLL, produce 15 gate findings or ResolveAssemblyReferences conflicts.
  6. Line drift between the ADAL deletion and the System.Linq.AsyncEnumerable redirect edit in the same files.
  7. Manual check insensitivity. The designer check cannot observe any effect of this change by construction.
  8. Unknown run-time consumers of the BCL assembly. Which System.Linq.Async members forward into System.Linq.AsyncEnumerable is unverified; after install those paths bind instead of failing, and no compiled call target changes (research 2 risk 4).
- Mitigations and rollbacks:
  1. The XML comment beside each aliased Reference and the AC10 Pester guard, which fails in the Pester job before any C# build runs.
  2. (a) Land this change before the next weekly Dependabot run, or rebuild the next bot branch on main rather than resolving conflicts. (b) The AC1 gate reports any redirect left stale by a bump, and the AC10 guard reports a dropped alias; adding the id to the semver-major ignore list is an optional two-file follow-up. (c) The AC1 gate now runs at zero debt and the AC5 guard covers the range half, so the drift that produced this issue fails CI instead of accumulating.
  3. AC8 makes the recorded DLL version the source for the csproj Include and the redirects.
  4. Run the restore route first; record the restore in the qa-gates evidence if it needs more than one attempt.
  5. Parts C's csproj edit and redirect edit land in one commit; AC3 and AC11 observe the combined state.
  6. Anchor every config edit on the assemblyIdentity name text, never on a line number.
  7. AC18 states the insensitivity in the evidence so the result is not read as proof of the sweep; the automated gates are the proof.
  8. No mitigation needed; the change only converts failures to successful binds. Rollback for any part is a revert of the commit; the Part C fallback in Proposed Fix keeps Parts A, B, D and E deliverable if the compile proof fails.

## Rollout & Follow-up

- Release/rollout steps: single pull request on branch bug/remaining-stale-binding-redirect-pairs-973; CI must show the Pester job green with the three dependency test files, both msbuild gates green, and the MSTest coverage job green; merge to main; no deployment step beyond the next add-in build picking up the edited TaskMaster.dll.config and the additional DLL.
- Post-fix monitoring or clean-up tasks: watch the first Dependabot run after merge for a conflict on the three file kinds; optional follow-ups to promote through the potential-entry lifecycle: add System.Linq.AsyncEnumerable to the semver-major ignore list (two-file change); remove the unused deep `using Microsoft.Graph.*` directives in five UtilitiesCS files (research 1 section 5.4); a later item may migrate call sites from System.Linq.Async to the BCL API, after which the alias and the Ix packages can go. Note for the maintainer: the Status line in this feature's issue.md still names the un-dated folder path from promotion; it was not edited by this spec.
- Links: issue #973; #953 (gate and Fizzler sweep); #418 (designer load failure, runbook precedent); #879 (netstandard hardening); #825 (ResolveAssemblyReferences log cited as evidence); #911 (dependency repair tooling and Dependabot grouping); research 1 and research 2 as listed at the top of this document.
