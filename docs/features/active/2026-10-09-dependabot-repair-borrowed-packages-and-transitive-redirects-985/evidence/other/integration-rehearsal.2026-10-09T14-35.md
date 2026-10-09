# Integration Rehearsal (Phase 7, AC6)

Timestamp: 2026-10-09T14-35
Command: git fetch origin dependabot/nuget/QuickFiler.Test/all-nuget-updates-2542c001c9
EXIT_CODE: 0
Output Summary:
- DEPENDABOT-SHA: fff92fe83032d00dfa84225c960928a82a6843f2 (git rev-parse origin/dependabot/nuget/QuickFiler.Test/all-nuget-updates-2542c001c9; the PR #984 repair commit)
- Fetch exit 0.
- Paths: the throwaway rehearsal worktree is written as REHEARSAL-ROOT (outside the repository, under the session scratchpad); this worktree is WORKSPACE-ROOT.

## P7-T1 Fetch

Command: git fetch origin dependabot/nuget/QuickFiler.Test/all-nuget-updates-2542c001c9
EXIT_CODE: 0

```
From https://github.com/drmoisan/TaskMaster
 * branch                dependabot/nuget/QuickFiler.Test/all-nuget-updates-2542c001c9 -> FETCH_HEAD
DEPENDABOT-SHA: fff92fe83032d00dfa84225c960928a82a6843f2
```

## P7-T2 Rehearsal worktree

Command: git worktree add -b rehearsal-985-throwaway REHEARSAL-ROOT origin/dependabot/nuget/QuickFiler.Test/all-nuget-updates-2542c001c9; git -C REHEARSAL-ROOT rev-parse HEAD
EXIT_CODE: 0

```
Preparing worktree (new branch 'rehearsal-985-throwaway')
branch 'rehearsal-985-throwaway' set up to track 'origin/dependabot/nuget/QuickFiler.Test/all-nuget-updates-2542c001c9'.
HEAD is now at fff92fe83 chore(deps): repair manifest and project-file consistency
REHEARSAL HEAD: fff92fe83032d00dfa84225c960928a82a6843f2 (equals DEPENDABOT-SHA)
```

## P7-T3 Merge (D2, -X ours)

Command: git -C REHEARSAL-ROOT merge -X ours --no-edit bug/dependabot-repair-borrowed-packages-and-transitive-redirects-985; git -C REHEARSAL-ROOT rev-parse HEAD; git -C REHEARSAL-ROOT status --porcelain; git -C REHEARSAL-ROOT merge-base --is-ancestor FIX-SHA HEAD
EXIT_CODE: 0

```
Auto-merging QuickFiler.Test/QuickFiler.Test.csproj
Auto-merging QuickFiler.Test/packages.config
Auto-merging TaskTree.Test/packages.config
Auto-merging UtilitiesCS.Test/packages.config
Merge made by the 'ort' strategy.
62 files changed, 3444 insertions(+), 604 deletions(-)
(manifest lines of the stat: QuickFiler.Test/packages.config | 1 +; UtilitiesCS.Test/packages.config | 1 +; TaskTree.Test/packages.config and QuickFiler.Test/QuickFiler.Test.csproj absent from the stat, so -X ours dropped part of the fix there; P7-T4 verifies and re-applies)
MERGE-SHA: 37a619f88b82b544c3ef0fa87dd318e917d0d2ad
porcelain: (empty)
merge-base --is-ancestor FIX-SHA HEAD: exit 0
```

## P7-T4 Fix content verification and re-application

Command: git -C REHEARSAL-ROOT rev-parse HEAD:<path> versus git rev-parse FIX-SHA:<path> for the five PowerShell paths; Grep for the four declarations and the WebView2.Core Include in REHEARSAL-ROOT; Edit tool re-application
EXIT_CODE: 0

(a) Blob equality (REHEARSAL HEAD versus FIX-SHA), all five equal:

```
scripts/dependencies/BindingRedirectSync.psm1 ef8eee178ce3296a872281467320860f759ffd27 = ef8eee178ce3296a872281467320860f759ffd27
scripts/dependencies/Repair-PackageManifestConsistency.ps1 eb9ff10ab693d4dc97f90d22149579838151f426 = eb9ff10ab693d4dc97f90d22149579838151f426
tests/scripts/dependencies/BindingRedirectSync.Tests.ps1 61b018e7270d5ca11d849a0300fe8514457c6533 = 61b018e7270d5ca11d849a0300fe8514457c6533
tests/scripts/dependencies/Repair-PackageManifestConsistency.RedirectSync.Tests.ps1 d82cfd5c6de703e82425c7be5cf2d078b071c990 = d82cfd5c6de703e82425c7be5cf2d078b071c990
tests/scripts/dependencies/RepositoryTreeConsistency.Tests.ps1 d5b96cc3b46fda690e3b8b7edbaf60742664d88b = d5b96cc3b46fda690e3b8b7edbaf60742664d88b
```

(b) After the merge: QuickFiler.Test WebView2 present (line 42), UtilitiesCS.Test WebView2 present (line 64); QuickFiler.Test ObjectListView.Official absent; TaskTree.Test ObjectListView.Official absent; QuickFiler.Test.csproj `Include="Microsoft.Web.WebView2.Core,` count 2 (lines 390 and 531; the duplicate item group 530-534 survived the merge).

```
REAPPLIED: QuickFiler.Test/packages.config inserted `  <package id="ObjectListView.Official" version="2.9.1" targetFramework="net481" />` before the OpenTelemetry line (first id comparing greater), now line 47
REAPPLIED: TaskTree.Test/packages.config inserted `  <package id="ObjectListView.Official" version="2.9.1" targetFramework="net481" />` before the OpenTelemetry line, now line 41
REAPPLIED: QuickFiler.Test/QuickFiler.Test.csproj deleted the surviving duplicate WebView2.Core item group (five lines, formerly 530-534)
```

Afterwards: each of the four declarations count 1 (QuickFiler.Test WebView2 line 42, QuickFiler.Test ObjectListView line 47, UtilitiesCS.Test WebView2 line 64, TaskTree.Test ObjectListView line 41); WebView2.Core Include count 1 in QuickFiler.Test.csproj.

Note: on the Dependabot branch the production siblings declare Microsoft.Web.WebView2 1.0.4258.31 (QuickFiler/packages.config line 19, UtilitiesCS/packages.config line 57) and ObjectListView.Official 2.9.1 (QuickFiler line 22, TaskTree line 8); P7-T7 decides the D1 simulation rows.

## P7-T5 Restore (pre-simulation)

Command: pwsh -NoProfile -File CMDDIR\985-restore.ps1 -WorkspaceRoot REHEARSAL-ROOT (nuget restore TaskMaster.sln -NonInteractive)
EXIT_CODE: 0

```
LINE-COUNT: 587
TAIL: Installed:
TAIL:     174 package(s) to packages.config projects
```

## P7-T6 DIAGNOSTIC (literal merge, -WhatIf)

Command: pwsh -NoProfile -File CMDDIR\985-repair.ps1 -WorkspaceRoot REHEARSAL-ROOT -WhatIfRun (& REHEARSAL-ROOT\scripts\dependencies\Repair-PackageManifestConsistency.ps1 -WhatIf)
EXIT_CODE: 0

```
COMMAND: & WORKSPACE-ROOT\scripts\dependencies\Repair-PackageManifestConsistency.ps1 -WhatIf
Manifest discovery: enumerated directories 34, returned files 53 (printed 4 times before the sync and 2 times after)
What if: Performing the operation "Synchronise binding redirects" on target "REHEARSAL-ROOT\Tags.Test\app.config".
What if: Performing the operation "Synchronise binding redirects" on target "REHEARSAL-ROOT\TaskTree.Test\app.config".
What if: Performing the operation "Synchronise binding redirects" on target "REHEARSAL-ROOT\TaskVisualization.Test\app.config".
What if: Performing the operation "Synchronise binding redirects" on target "REHEARSAL-ROOT\ToDoModel.Test\app.config".
What if: Performing the operation "Synchronise binding redirects" on target "REHEARSAL-ROOT\VBFunctions.Test\app.config".
Binding redirect sync: examined 17 application configuration file(s), synchronised 5 redirect(s), unverifiable 1, unresolvable 0
IS-SUCCESS: True
WRITTEN-COUNT: 0
REPAIR-COUNT: 0
BEYOND-KNOWN-WEAK: 0
REPORT-REPAIR-KINDS:
SKIP-COUNT: 0
REDIRECTSYNC-REPAIR-COUNT: 5
REDIRECTSYNC: Tags.Test log4net 3.4.0.0 to 3.5.0.0 HighestDeployed
REDIRECTSYNC: TaskTree.Test log4net 3.4.0.0 to 3.5.0.0 HighestDeployed
REDIRECTSYNC: TaskVisualization.Test log4net 3.4.0.0 to 3.5.0.0 HighestDeployed
REDIRECTSYNC: ToDoModel.Test log4net 3.4.0.0 to 3.5.0.0 HighestDeployed
REDIRECTSYNC: VBFunctions.Test log4net 3.4.0.0 to 3.5.0.0 HighestDeployed
REDIRECTSYNC-UNVERIFIABLE: netstandard
REDIRECTSYNC-UNRESOLVABLE:
BODY-BEGIN
## Repairs applied
No repairs were applied.

## Binding redirects synchronised
- Tags.Test: log4net 3.4.0.0 to 3.5.0.0 (HighestDeployed)
- TaskTree.Test: log4net 3.4.0.0 to 3.5.0.0 (HighestDeployed)
- TaskVisualization.Test: log4net 3.4.0.0 to 3.5.0.0 (HighestDeployed)
- ToDoModel.Test: log4net 3.4.0.0 to 3.5.0.0 (HighestDeployed)
- VBFunctions.Test: log4net 3.4.0.0 to 3.5.0.0 (HighestDeployed)
BODY-END
```

The script completed; WRITTEN-COUNT 0 under -WhatIf; the five transitive log4net redirects research section 4.2 predicted are the five proposed syncs.

## P7-T7 D1 simulation rows

Command: Grep `id="(Microsoft\.Web\.WebView2|ObjectListView\.Official)"` over REHEARSAL-ROOT production and test manifests
EXIT_CODE: 0

Production values: QuickFiler/packages.config line 19 Microsoft.Web.WebView2 1.0.4258.31; UtilitiesCS/packages.config line 57 Microsoft.Web.WebView2 1.0.4258.31 (the two WebView2 values agree); QuickFiler/packages.config line 22 ObjectListView.Official 2.9.1; TaskTree/packages.config line 8 ObjectListView.Official 2.9.1 (the two ObjectListView values agree).

```
SIM-ROW: QuickFiler.Test Microsoft.Web.WebView2 1.0.4191.47 1.0.4258.31 UPDATE
SIM-ROW: UtilitiesCS.Test Microsoft.Web.WebView2 1.0.4191.47 1.0.4258.31 UPDATE
SIM-ROW: QuickFiler.Test ObjectListView.Official 2.9.1 2.9.1 NONE
SIM-ROW: TaskTree.Test ObjectListView.Official 2.9.1 2.9.1 NONE
```

## P7-T8 Simulated Dependabot update (D1)

Command: pwsh -NoProfile -File CMDDIR\985-nuget-update.ps1 -WorkspaceRoot REHEARSAL-ROOT -ProjectDirectory <test project> -PackageId Microsoft.Web.WebView2 -PackageVersion 1.0.4258.31 (for QuickFiler.Test, then UtilitiesCS.Test); git -C REHEARSAL-ROOT diff --stat; git -C REHEARSAL-ROOT status --porcelain
EXIT_CODE: 0

```
COMMAND: nuget update QuickFiler.Test\packages.config -Id Microsoft.Web.WebView2 -Version 1.0.4258.31 -RepositoryPath WORKSPACE-ROOT\packages -NonInteractive -FileConflictAction Ignore
EXIT_CODE: 0
TAIL: Removed package 'Microsoft.Web.WebView2 1.0.4191.47' from 'packages.config'
TAIL: Successfully uninstalled 'Microsoft.Web.WebView2 1.0.4191.47' from QuickFiler.Test
TAIL: Added package 'Microsoft.Web.WebView2.1.0.4258.31' to 'packages.config'
TAIL: Successfully installed 'Microsoft.Web.WebView2 1.0.4258.31' to QuickFiler.Test
COMMAND: nuget update UtilitiesCS.Test\packages.config -Id Microsoft.Web.WebView2 -Version 1.0.4258.31 -RepositoryPath WORKSPACE-ROOT\packages -NonInteractive -FileConflictAction Ignore
EXIT_CODE: 0
TAIL: Removed package 'Microsoft.Web.WebView2 1.0.4191.47' from 'packages.config'
TAIL: Successfully uninstalled 'Microsoft.Web.WebView2 1.0.4191.47' from UtilitiesCS.Test
TAIL: Added package 'Microsoft.Web.WebView2.1.0.4258.31' to 'packages.config'
TAIL: Successfully installed 'Microsoft.Web.WebView2 1.0.4258.31' to UtilitiesCS.Test
diff --stat:
 QuickFiler.Test/QuickFiler.Test.csproj   | 21 ++++++++++++---------
 QuickFiler.Test/packages.config          |  7 ++++---
 TaskTree.Test/packages.config            |  1 +
 UtilitiesCS.Test/UtilitiesCS.Test.csproj | 20 ++++++++++++++------
 UtilitiesCS.Test/packages.config         |  6 +++---
 5 files changed, 34 insertions(+), 21 deletions(-)
porcelain:
 M QuickFiler.Test/QuickFiler.Test.csproj
 M QuickFiler.Test/packages.config
 M TaskTree.Test/packages.config
 M UtilitiesCS.Test/UtilitiesCS.Test.csproj
 M UtilitiesCS.Test/packages.config
```

Afterwards: QuickFiler.Test/packages.config line 42 and UtilitiesCS.Test/packages.config line 64 declare Microsoft.Web.WebView2 1.0.4258.31; Grep `Microsoft\.Web\.WebView2\.1\.0\.4191\.47` over the two updated test csproj files returns 0 (positive control: `Microsoft\.Web\.WebView2\.1\.0\.4258\.31` returns 5 in each). The diff stat includes the P7-T4 re-applications (TaskTree.Test/packages.config and the QuickFiler.Test csproj group removal).

## P7-T9 Workflow step "Restore solution" (dependabot-repair.yml line 74)

Command: pwsh -NoProfile -File CMDDIR\985-restore.ps1 -WorkspaceRoot REHEARSAL-ROOT (nuget restore TaskMaster.sln -NonInteractive)
EXIT_CODE: 0

```
LINE-COUNT: 8
TAIL:   OK https://api.nuget.org/v3-vulnerabilities/2026.10.06.23.47.38/2026.10.09.05.47.56/vulnerability.update.json 48ms
```

## P7-T10 Workflow step "Repair package manifest consistency" (dependabot-repair.yml line 80, no arguments)

Command: pwsh -NoProfile -File CMDDIR\985-repair.ps1 -WorkspaceRoot REHEARSAL-ROOT (& REHEARSAL-ROOT\scripts\dependencies\Repair-PackageManifestConsistency.ps1)
EXIT_CODE: 0

```
COMMAND: & WORKSPACE-ROOT\scripts\dependencies\Repair-PackageManifestConsistency.ps1
Manifest discovery: enumerated directories 34, returned files 53 (printed 4 times before the sync and 2 times after)
Binding redirect sync: examined 17 application configuration file(s), synchronised 7 redirect(s), unverifiable 1, unresolvable 0
IS-SUCCESS: True
WRITTEN-COUNT: 9
WRITTEN: QuickFiler.Test\app.config
WRITTEN: Tags.Test\app.config
WRITTEN: TaskTree.Test\app.config
WRITTEN: TaskVisualization.Test\app.config
WRITTEN: ToDoModel.Test\app.config
WRITTEN: UtilitiesCS.Test\app.config
WRITTEN: VBFunctions.Test\app.config
WRITTEN: QuickFiler.Test\packages.config
WRITTEN: UtilitiesCS.Test\packages.config
REPAIR-COUNT: 0
BEYOND-KNOWN-WEAK: 0
REPORT-REPAIR-KINDS:
SKIP-COUNT: 0
REDIRECTSYNC-REPAIR-COUNT: 7
REDIRECTSYNC: QuickFiler.Test Microsoft.Web.WebView2.Core 1.0.4191.47 to 1.0.4258.31 OwnReference
REDIRECTSYNC: Tags.Test log4net 3.4.0.0 to 3.5.0.0 HighestDeployed
REDIRECTSYNC: TaskTree.Test log4net 3.4.0.0 to 3.5.0.0 HighestDeployed
REDIRECTSYNC: TaskVisualization.Test log4net 3.4.0.0 to 3.5.0.0 HighestDeployed
REDIRECTSYNC: ToDoModel.Test log4net 3.4.0.0 to 3.5.0.0 HighestDeployed
REDIRECTSYNC: UtilitiesCS.Test Microsoft.Web.WebView2.Core 1.0.4191.47 to 1.0.4258.31 OwnReference
REDIRECTSYNC: VBFunctions.Test log4net 3.4.0.0 to 3.5.0.0 HighestDeployed
REDIRECTSYNC-UNVERIFIABLE: netstandard
REDIRECTSYNC-UNRESOLVABLE:
BODY-BEGIN
## Repairs applied
No repairs were applied.

## Binding redirects synchronised
- QuickFiler.Test: Microsoft.Web.WebView2.Core 1.0.4191.47 to 1.0.4258.31 (OwnReference)
- Tags.Test: log4net 3.4.0.0 to 3.5.0.0 (HighestDeployed)
- TaskTree.Test: log4net 3.4.0.0 to 3.5.0.0 (HighestDeployed)
- TaskVisualization.Test: log4net 3.4.0.0 to 3.5.0.0 (HighestDeployed)
- ToDoModel.Test: log4net 3.4.0.0 to 3.5.0.0 (HighestDeployed)
- UtilitiesCS.Test: Microsoft.Web.WebView2.Core 1.0.4191.47 to 1.0.4258.31 (OwnReference)
- VBFunctions.Test: log4net 3.4.0.0 to 3.5.0.0 (HighestDeployed)
BODY-END
```

The sync rewrote the five transitive log4net redirects and the two test-project WebView2.Core redirects the packages.config updater leaves untouched; the normalisation pass reflowed the two manifests the simulated update wrote.

## P7-T11 Idempotence (post-push re-fire)

Command: pwsh -NoProfile -File CMDDIR\985-repair.ps1 -WorkspaceRoot REHEARSAL-ROOT (second run, no arguments)
EXIT_CODE: 0

```
Binding redirect sync: examined 17 application configuration file(s), synchronised 0 redirect(s), unverifiable 1, unresolvable 0
IS-SUCCESS: True
WRITTEN-COUNT: 0
REPAIR-COUNT: 0
BEYOND-KNOWN-WEAK: 0
REPORT-REPAIR-KINDS:
SKIP-COUNT: 0
REDIRECTSYNC-REPAIR-COUNT: 0
REDIRECTSYNC-UNVERIFIABLE: netstandard
REDIRECTSYNC-UNRESOLVABLE:
BODY-BEGIN
## Repairs applied
No repairs were applied.
BODY-END
```

## P7-T12 Restore after the repair writes

Command: pwsh -NoProfile -File CMDDIR\985-restore.ps1 -WorkspaceRoot REHEARSAL-ROOT (nuget restore TaskMaster.sln -NonInteractive)
EXIT_CODE: 0

```
LINE-COUNT: 5
TAIL: All packages listed in packages.config are already installed.
```

## P7-T13 Analyzer build of REHEARSAL-ROOT\TaskMaster.sln

Command: pwsh -NoProfile -File CMDDIR\985-msbuild.ps1 -WorkspaceRoot REHEARSAL-ROOT -Gate Analyzers -LogName 985-rehearsal-analyzers.log (msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true)
EXIT_CODE: 0

```
EXIT_CODE: 0
OUTPUT-ASSEMBLIES: 18
SUMMARY: 0 Warning(s)
SUMMARY: 0 Error(s)
ERROR-LINE-COUNT: 0
PATH-LENGTH-SIGNATURE-LINES: 0
```

## P7-T14 Nullable build of REHEARSAL-ROOT\TaskMaster.sln

Command: pwsh -NoProfile -File CMDDIR\985-msbuild.ps1 -WorkspaceRoot REHEARSAL-ROOT -Gate Nullable -LogName 985-rehearsal-nullable.log (msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true)
EXIT_CODE: 0

```
EXIT_CODE: 0
OUTPUT-ASSEMBLIES: 18
SUMMARY: 0 Warning(s)
SUMMARY: 0 Error(s)
ERROR-LINE-COUNT: 0
PATH-LENGTH-SIGNATURE-LINES: 0
```

## P7-T15 Pester in REHEARSAL-ROOT

Command: pwsh -NoProfile -File CMDDIR\985-pester.ps1 -WorkspaceRoot REHEARSAL-ROOT -TestPath tests/scripts/dependencies/BindingRedirectVerification.Tests.ps1,tests/scripts/dependencies/RepositoryTreeConsistency.Tests.ps1
EXIT_CODE: 0

```
TEST-PATH-COUNT: 2
COVERAGE-PATH-COUNT: 0
PESTER-VERSION: 5.6.1
PESTER Passed=21 Failed=0 Skipped=0 NotRun=0 Total=21
FILE-TESTS BindingRedirectVerification.Tests.ps1 Passed=16 Failed=0 Total=16
FILE-TESTS RepositoryTreeConsistency.Tests.ps1 Passed=5 Failed=0 Total=5
```

The BindingRedirectVerification repository invariant (the assertion that failed on CI run #1059 with `log4net|3.4.0.0`) passes on the repaired rehearsal tree, and the new orphaned-HintPath gate passes there too.

## P7-T16 Verdict

REHEARSAL-VERDICT: PASS

- P7-T10 repair (workflow-faithful, no arguments): EXIT_CODE 0, IS-SUCCESS True, WRITTEN-COUNT 9, 7 synchronised redirects (5 transitive log4net, 2 test WebView2.Core), BEYOND-KNOWN-WEAK 0.
- P7-T11 idempotent re-fire: EXIT_CODE 0, IS-SUCCESS True, WRITTEN-COUNT 0, REDIRECTSYNC-REPAIR-COUNT 0.
- P7-T13 analyzer Rebuild: EXIT_CODE 0, 18 output assemblies, 0 Error(s), ERROR-LINE-COUNT 0.
- P7-T14 nullable Rebuild (/p:TreatWarningsAsErrors=true): EXIT_CODE 0, 18 output assemblies, 0 Error(s), ERROR-LINE-COUNT 0.
- P7-T15 Pester: BindingRedirectVerification.Tests.ps1 16/16 and RepositoryTreeConsistency.Tests.ps1 5/5 passed.
- Fidelity note (plan D1): the two test-project WebView2 declarations were bumped to 1.0.4258.31 by a simulated packages.config update (P7-T8) to model the post-merge `@dependabot recreate`; binding redirects were left to the repair script's sync pass.
- C4 sanitize after the verdict: FEATURE count 0; positive control (.git pointer file) count 1.

## P7-T17 Cleanup (BLOCKED, not completed)

Command: git worktree remove --force REHEARSAL-ROOT; git branch -D rehearsal-985-throwaway
EXIT_CODE: 1

```
CLEANUP: git worktree remove refused by the PreToolUse hook before execution: EPIC_WORKTREE_REMOVAL_BLOCKED: TARGET_WORKTREE_NOT_DERIVABLE: no live worktree's run checkpoint records worktree_path 'REHEARSAL-ROOT' in the epic checkpoint; removal requires an epic or parallel-orchestrator checkpoint record with merge_status in {merged, worktree_removed}.
CLEANUP: git branch -D rehearsal-985-throwaway not run (same command; the branch is still checked out in the rehearsal worktree).
CLEANUP: git worktree list --porcelain still lists REHEARSAL-ROOT (HEAD 37a619f88b82b544c3ef0fa87dd318e917d0d2ad, branch refs/heads/rehearsal-985-throwaway).
CLEANUP: git branch --list rehearsal-985-throwaway: "+ rehearsal-985-throwaway" (present, checked out in the rehearsal worktree).
CLEANUP: git ls-remote --heads origin rehearsal-985-throwaway: empty (exit 0); the branch was never pushed.
```

The hook was not bypassed. Removing the rehearsal worktree and branch needs either the user's explicit approval to route around the hook or a checkpoint record that authorizes the removal. The rehearsal content is not in the repository; the worktree sits under the session scratchpad.
