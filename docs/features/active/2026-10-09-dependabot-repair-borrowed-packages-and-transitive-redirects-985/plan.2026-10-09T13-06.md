# 2026-10-09-dependabot-repair-borrowed-packages-and-transitive-redirects (Plan)

- **Issue:** #985
- **Parent (optional):** none
- **Owner:** drmoisan
- **Last Updated:** 2026-10-09
- **Status:** Draft (planner round 1; preflight round 1 deltas 1-9 and the background-run note applied; awaiting confirming preflight)
- **Version:** 1.1
- **Work Mode:** full-bug (AC source: `spec.md` only, per `issue.md` metadata `- Work Mode: full-bug`)

**Fail-closed evidence rule:** Every baseline, final-QA and coverage-comparison artifact named below is required. If any required artifact is missing or carries a placeholder where a number is required, the audit verdict is BLOCKED or INCOMPLETE, never PASS.

**Evidence accounting rule:** Every evidence-producing task names its artifact path. No task is checked off without its artifact on disk.

## 1. Objective

Make every grouped NuGet Dependabot pull request pass CI after the `dependabot-repair` workflow runs, with no human edit, by (a) declaring the four borrowed (project, package) pairs in the owning test projects' `packages.config` and removing the duplicate `Microsoft.Web.WebView2.Core` reference in `QuickFiler.Test/QuickFiler.Test.csproj`, (b) adding an unconditional, solution-wide binding-redirect sync pass (`scripts/dependencies/BindingRedirectSync.psm1`) wired into `scripts/dependencies/Repair-PackageManifestConsistency.ps1`, and (c) adding a main-branch Pester gate that fails when any project hint-paths a package its own manifest does not declare. Requirements: `spec.md` (Acceptance Criteria AC1 to AC7), `issue.md`, `research/research.2026-10-09T14-10.md`.

Out of scope (spec "Scope & Non-Goals"): any file under `.github/workflows/**` (including its README), `ConsistencyVerifier.psm1`, `ProjectConsistency.psm1`, `BindingRedirectVerification.psm1`, production C# source, Dependabot configuration.

## 2. Conventions

**C1. Symbols.**
- `FEATURE` = `docs/features/active/2026-10-09-dependabot-repair-borrowed-packages-and-transitive-redirects-985`.
- `WORKTREE` = the executor's worktree root (the checkout of branch `bug/dependabot-repair-borrowed-packages-and-transitive-redirects-985`). Every repository path in this plan is relative to `WORKTREE`.
- `SCRATCH` = the directory `$env:LOCALAPPDATA\Temp\claude\C--Users-DanMoisan-repos-TaskMaster-wt-2026-10-09T12-47\a87f8bd5-b2ae-46b5-8bba-6b4ed90e8d09\scratchpad` (the caller-supplied session scratchpad, written through the environment variable so no drive-rooted profile path appears in this tracked file). Resolve it once with `pwsh -NoProfile -Command "Join-Path $env:LOCALAPPDATA 'Temp\claude\C--Users-DanMoisan-repos-TaskMaster-wt-2026-10-09T12-47\a87f8bd5-b2ae-46b5-8bba-6b4ed90e8d09\scratchpad'"`.
- `CMDDIR` = `SCRATCH\985-cmd` (helper scripts, outside the repository).
- `REHEARSAL` = `SCRATCH\rehearsal-985` (the throwaway rehearsal worktree, outside the repository).
- `FIX-BRANCH` = `bug/dependabot-repair-borrowed-packages-and-transitive-redirects-985`. `REHEARSAL-BRANCH` = `rehearsal-985-throwaway`. `DEPENDABOT-REF` = `origin/dependabot/nuget/QuickFiler.Test/all-nuget-updates-2542c001c9`.
- `<TS>` = the local timestamp read from the host clock when the task runs: `pwsh -NoProfile -Command "Get-Date -Format yyyy-MM-ddTHH-mm"`. Never composed or estimated.

**C2. Evidence schema.** Every command-step artifact carries, in this order, `Timestamp:`, `Command:`, `EXIT_CODE:`, optional `ExpectedExitCode:` (only on `[expect-fail]` artifacts), and `Output Summary:` (1 to 20 lines carrying the essential result signal, including every gate value the task names). Evidence lives only under `FEATURE/evidence/<kind>/` with kind `baseline`, `regression-testing`, `qa-gates` or `other`. No caller-supplied non-canonical evidence path was received, so no `EVIDENCE_LOCATION_OVERRIDE_REJECTED` record applies.

**C3. Hygiene (tracked-file content).** The repository hygiene guard (`scripts/hygiene/Test-RepositoryHygiene.Rules.ps1` lines 5-55) fails CI on any tracked line matching a drive-letter user-profile path, and lines 57-123 reject a raw JaCoCo document (one carrying `sourcefile`/`class`/`line` elements), a Cobertura document and a trx document. Therefore: (a) evidence text never carries an absolute path; write `WORKSPACE-ROOT` for a worktree root and `USER-PROFILE` for a profile path (every helper script below already performs that substitution on every line it prints); `Command:` fields write `workspace_root = WORKSPACE-ROOT`; (b) raw Pester JaCoCo documents are written only under the ignored `coverage/` directory and never copied into `FEATURE`; (c) the only coverage files copied into `FEATURE` are the MSTest runner's package-level projection (`*.jacoco.xml`) and its trx-derived `*.summary.txt`, which CLAUDE.md "Committed Test Evidence Format" permits.

**C4. CMD-SANITIZE (run before every commit).** With the Grep tool, pattern `(?i)[a-z]:[\\/]+users[\\/]+[a-z0-9_.~-]`, output mode count: (1) path `FEATURE` must return 0; (2) positive control, path `.git` (the worktree's gitdir pointer file, which names a profile path) must return 1. A control of 0 is STOP: SANITIZE-BLIND. A non-zero count in (1) is fixed by replacing the matched path with the C3 token and re-running C4.

**C5. Shell discipline.** Bash permits only `git *` and `pwsh *` segments: never `cd`, use `git -C <dir>` and absolute script paths (`pwsh -NoProfile -File <CMDDIR>\<script>.ps1 ...`); each helper script does its own `Set-Location -LiteralPath`. Repository text searches use the Grep tool; the Grep tool is a regex engine, so the literals `[`, `]`, `.`, `(`, `)` in a counted token are escaped as written in each task.

**C6. Helper scripts.** Phase 0 writes the nine helper scripts of section 5 into `CMDDIR` verbatim (P0-T2 writes `985-probe.ps1`, P0-T3 the other eight) with the Write tool (they are outside the repository, so they are neither committed nor counted by the PowerShell batch budget, whose hook ignores paths outside the repository root). Every helper prints only sanitized lines (C3).

**C7. Loop rules.**
- PowerShell loop = P4-T1 through P4-T5. If any of those steps fails, or P4-T1 rewrites any file, fix the cause inside the Write Set (section 4) and restart at P4-T1. Each iteration's artifacts carry `ITERATION: <n>`; only a single iteration in which P4-T1 through P4-T5 all pass satisfies Phase 4.
- C# loop = P5-T1 through P5-T5, same rule, restart at P5-T1. A fix that touches a PowerShell file restarts at P4-T1 and then re-runs Phase 5 from P5-T1.
- Flaky-test re-measurement (not a retry mechanism): if CMD-MSTEST prints `RUNNER_RESULT=THREW`, `TRX-FAILED-COUNT: 1`, and its single `TRX-FAILED-TEST:` line ends with `TryAddValuesAsync_UpdatesExistingValue` (issue #780; the runner throws at `Invoke-MSTestWithCoverage.ps1` line 262 before writing its summary, so the failed set is read from the trx), run the same command once more and record both runs; the second run governs. Any other failure is a loop failure.

**C8. Stop list.** STOP (record the reason in the task's artifact, leave the task unchecked, report to the caller) on: STOP: PWSH-UNAVAILABLE, STOP: TOOL-MISSING (P0-T2), STOP: BASELINE-FORMAT-DRIFT (P0-T5), STOP: ANALYZE-BASELINE-RED (P0-T6), STOP: BASELINE-TEST-RED (P0-T7, P0-T8, P0-T13), STOP: CS-BASELINE-RED (P0-T9, P0-T10, P0-T11, P0-T12), STOP: WRONG-REASON (P1-T3, P1-T4), STOP: LIVE-TREE-SYNC-NONZERO (P3-T6), STOP: SIZE-LIMIT (P3-T3, P4-T6), STOP: COMMIT-BLOCKED (P6-T1, P8-T2), STOP: SANITIZE-BLIND (C4, P4-T5), and every rehearsal failure in Phase 7 (STOP: REHEARSAL-FAILED, rule R-FAIL in Phase 7). No other condition stops the plan; loop failures follow C7.

**C9. Commit command (CMD-COMMIT).** `git add -- <explicit paths>` (never `-A`, never `.`), then `git commit -m "<subject>" -m "<body>"` followed by the trailer `-m` arguments dictated by the executing session's attribution instructions, then `git rev-parse HEAD` and `git status --porcelain`. The task's own plan checkbox is flipped to `[x]` before `git add`; if `git add` or `git commit` fails or a hook denies it, restore the checkbox to `[ ]` and STOP: COMMIT-BLOCKED. No push is performed by this plan.

**C10. Agent-memory porcelain lines.** Porcelain lines whose path begins `.claude/agent-memory/` are excluded from every `PreExistingWorktreePaths` comparison (P0-T14, P6-T1, P8-T2) and are recorded separately as `AGENT-MEMORY-LINES:`. Agent-memory files ARE committed: P6-T1 and P8-T2 add every path listed on their `AGENT-MEMORY-LINES:` line to the CMD-COMMIT path list (repository policy: commit all audit-trail evidence; the worktree must end clean).

**C11. Long-running commands.** The MSBuild Rebuild runs (P0-T11, P0-T12, P5-T3, P5-T4, P7-T13, P7-T14) and the MSTest coverage runs (P0-T13, P5-T5) can exceed the 10-minute Bash tool limit. The executor runs each of them with `run_in_background` and waits for the background command to complete before reading its output; a run cut off by the tool limit is never treated as a result.

## 3. Planner Decisions

- **D1 (rehearsal fidelity: simulated Dependabot update).** A literal "merge the fix into PR #984's branch, then repair and build" leaves `QuickFiler.Test` and `UtilitiesCS.Test` declaring and referencing `Microsoft.Web.WebView2` 1.0.4191.47 while their production project references compile against the bumped version. That state never occurs after merge: research section 4.1 and spec "Rollout" state that `@dependabot recreate` regenerates the group from main, where the test projects now declare WebView2, so Dependabot bumps them too. The repair script cannot perform that bump: it does not rewrite a `Reference Include` version for an assembly not named for its package (research 3.1, `ProjectConsistency.psm1` `Get-RewrittenReferenceVersionLine`). Phase 7 therefore inserts one step between the merge and the workflow-faithful repair run: for each newly declared (test project, package) pair whose production sibling on the Dependabot branch declares a different version, run the NuGet packages.config updater (`nuget update`) for that pair only. Binding redirects are deliberately left to the repair script's new sync pass, which exercises it on real content. A read-only `-WhatIf` repair run on the merged tree before the simulation (P7-T6) records what the sync pass proposes on the literal merge state. This decision does not change AC6's text; it is flagged to the caller for confirmation.
- **D2 (rehearsal merge strategy).** The merge uses `-X ours` (the Dependabot side wins any conflicting hunk), because the fix's insertions sit beside lines Dependabot bumps (for example `MSTest.TestFramework` directly above the `ObjectListView.Official` insertion point in `TaskTree.Test/packages.config`). P7-T4 then verifies every fix element by content and re-applies any element the strategy dropped, recording each re-application.
- **D3 (MSTest route).** `scripts/vscode/Invoke-MSTestWithCoverage.ps1` hard-codes its test filter (lines 82-94) and exposes no filter parameter. Per the caller's authorization and the recorded workstation hang of the shell-icon classes, CMD-MSTEST dot-sources the runner (its entry guard is line 459) and redefines `Get-DotnetCoverageArgumentList` with the runner's own list plus the exclusion `FullyQualifiedName!~HelperClasses.ShellUtilities_Tests`, `FullyQualifiedName!~HelperClasses.ShellUtilitiesStatic_Tests`, `FullyQualifiedName!~HelperClasses.SysImageListHelperTests`, `FullyQualifiedName!~EmailIntelligence.OSBrowser_Tests` and a blame hang collector. The same route is used at baseline and final so the figures are comparable. Every MSTest artifact records `SHELL-ICON-EXCLUSION: APPLIED` and the four class names; CI runs the full set.
- **D4 (MSBuild route).** `scripts/vscode/Invoke-VSBuild.ps1` runs `Sync-PackageReferences.ps1` before building (lines 247-253), which can rewrite `.csproj` HintPaths and would mask exactly the manifest defects this item fixes. CMD-MSBUILD therefore resolves MSBuild through vswhere and runs the CLAUDE.md argument lists verbatim (`/t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU"` plus the gate properties).
- **D5 (PowerShell numbers).** The PoshQC MCP tools return only `{ok, tool, workspace_root, summary}` and the MCP test tool's coverage document does not instrument `scripts/dependencies`. Every MCP step is therefore run unconditionally for the policy record and paired with a direct Pester run (CMD-PESTER) whose configuration mirrors `.github/workflows/_pester.yml` lines 40-47, which supplies pass/fail counts and line coverage.
- **D6 (coverage gates).** PowerShell: aggregate line coverage over `scripts/dependencies`, `scripts/hygiene`, `scripts/vscode` at or above 80.00 (CLAUDE.md UT2 and the `_pester.yml` line 71 floor; the 85 figure in `.claude/rules/quality-tiers.md` is recorded as an observation because CLAUDE.md takes precedence and the two documents disagree); `BindingRedirectSync.psm1` line coverage at or above 90.00 (new-code rule); `Repair-PackageManifestConsistency.ps1` per-file line coverage not lower than its baseline figure (changed-lines no-regression). An aggregate not-lower gate is not used because adding a module covered at 90 to a folder covered above 90 lowers the aggregate without any regression. C#: no C# source line changes (proved by an empty `*.cs` numstat), so the runner's own floors (line 80, branch 75) are the gate and the baseline-to-final delta is recorded.
- **D7 (AC7).** AC7 is CI-dependent and is checked off by the item's orchestrator after merge (acceptance-criteria-tracking "CI-Dependent Criteria"). This plan records it as pending and never checks it off.
- **D8 (rehearsal path length).** `REHEARSAL` is a long path; .NET Framework MSBuild can fail on paths over 260 characters. P0-T2 records the resolved length and the `LongPathsEnabled` registry value. A rehearsal build failure whose text names a path-length condition is still a STOP (caller rule), classified `REHEARSAL-ENVIRONMENT-PATH-LENGTH` so the caller can relocate the rehearsal.
- **D9 (spec premises re-verified).** The four borrowed pairs and their insertion anchors (research 4.1) were re-derived against this tree: see section 9.

## 4. Write Set

Production and manifest files (the only files outside `FEATURE` this plan modifies):
- `QuickFiler.Test/packages.config`, `UtilitiesCS.Test/packages.config`, `TaskTree.Test/packages.config`, `QuickFiler.Test/QuickFiler.Test.csproj`
- `scripts/dependencies/BindingRedirectSync.psm1` (new), `scripts/dependencies/Repair-PackageManifestConsistency.ps1`

Test files:
- `tests/scripts/dependencies/RepositoryTreeConsistency.Tests.ps1` (one new `It`)
- `tests/scripts/dependencies/Repair-PackageManifestConsistency.RedirectSync.Tests.ps1` (new)
- `tests/scripts/dependencies/BindingRedirectSync.Tests.ps1` (new)

Feature folder: this plan (checkboxes), `FEATURE/spec.md` (AC checkbox flips only, AC1 to AC6), and the evidence files named in the tasks. Agent-memory files under `.claude/agent-memory/` that are dirty at commit time are committed by P6-T1 and P8-T2 (C10); they are not code changes. Production PowerShell file count: 2 (direct-mode budget 1-3).

## 5. Helper Script Definitions (written by P0-T3 into CMDDIR)

Every script below starts with the same sanitizer. It is shown once here and is pasted at the top of each script after its `param` block.

```powershell
function Format-SafeLine {
    param([string]$Line, [string[]]$Root = @())
    $text = [string]$Line
    foreach ($entry in $Root) {
        if (-not [string]::IsNullOrEmpty($entry)) {
            $text = [regex]::Replace($text, [regex]::Escape($entry), 'WORKSPACE-ROOT', 'IgnoreCase')
        }
    }
    return [regex]::Replace($text, '[A-Za-z]:[\\/]+[Uu][Ss][Ee][Rr][Ss][\\/]+[^\\/\s"]+', 'USER-PROFILE')
}
```

**985-probe.ps1**

```powershell
param([Parameter(Mandatory = $true)][string]$RehearsalRoot)
Set-StrictMode -Version Latest
"PWSH-VERSION: $($PSVersionTable.PSVersion)"
$pester = Get-Module -ListAvailable Pester | Sort-Object Version -Descending | Select-Object -First 1
"PESTER-AVAILABLE: $(if ($pester) { $pester.Version } else { 'NONE' })"
$nuget = Get-Command nuget -ErrorAction SilentlyContinue
"NUGET: $(if ($nuget) { 'FOUND' } else { 'NONE' })"
if ($nuget) { "NUGET-VERSION: $(@(& nuget help) | Select-Object -First 1)" }
$vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'
"VSWHERE: $(Test-Path -LiteralPath $vswhere)"
if (Test-Path -LiteralPath $vswhere) {
    $msbuild = & $vswhere -latest -requires Microsoft.Component.MSBuild -find 'MSBuild\**\Bin\MSBuild.exe' | Select-Object -First 1
    "MSBUILD: $(if ($msbuild) { 'FOUND' } else { 'NONE' })"
}
"DOTNET-COVERAGE: $(if (Get-Command dotnet-coverage -ErrorAction SilentlyContinue) { 'FOUND' } else { 'NONE' })"
$longPath = Get-ItemProperty -Path 'HKLM:\SYSTEM\CurrentControlSet\Control\FileSystem' -Name LongPathsEnabled -ErrorAction SilentlyContinue
"LONG-PATHS-ENABLED: $(if ($longPath) { $longPath.LongPathsEnabled } else { 'UNSET' })"
"REHEARSAL-ROOT-LENGTH: $($RehearsalRoot.Length)"
```

**985-restore.ps1**

```powershell
param([Parameter(Mandatory = $true)][string]$WorkspaceRoot)
Set-StrictMode -Version Latest
Set-Location -LiteralPath $WorkspaceRoot
$line = @(& nuget restore TaskMaster.sln -NonInteractive 2>&1 | ForEach-Object { [string]$_ })
$exitCode = $LASTEXITCODE
"COMMAND: nuget restore TaskMaster.sln -NonInteractive"
"EXIT_CODE: $exitCode"
"LINE-COUNT: $($line.Count)"
foreach ($entry in @($line | Select-Object -Last 5)) { "TAIL: $(Format-SafeLine -Line $entry -Root @($WorkspaceRoot))" }
exit $exitCode
```

**985-csharpier.ps1**

```powershell
param([Parameter(Mandatory = $true)][string]$WorkspaceRoot, [Parameter(Mandatory = $true)][ValidateSet('restore', 'format', 'check')][string]$Mode)
Set-StrictMode -Version Latest
Set-Location -LiteralPath $WorkspaceRoot
$argument = if ($Mode -eq 'restore') { @('tool', 'restore') } else { @('tool', 'run', 'csharpier', $Mode, '.') }
$line = @(& dotnet @argument 2>&1 | ForEach-Object { [string]$_ })
$exitCode = $LASTEXITCODE
"COMMAND: dotnet $($argument -join ' ')"
"EXIT_CODE: $exitCode"
foreach ($entry in @($line | Select-Object -Last 8)) { "TAIL: $(Format-SafeLine -Line $entry -Root @($WorkspaceRoot))" }
exit $exitCode
```

**985-msbuild.ps1**

```powershell
param(
    [Parameter(Mandatory = $true)][string]$WorkspaceRoot,
    [Parameter(Mandatory = $true)][ValidateSet('Analyzers', 'Nullable')][string]$Gate,
    [Parameter(Mandatory = $true)][string]$LogName
)
Set-StrictMode -Version Latest
Set-Location -LiteralPath $WorkspaceRoot
$vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'
$msbuild = & $vswhere -latest -requires Microsoft.Component.MSBuild -find 'MSBuild\**\Bin\MSBuild.exe' | Select-Object -First 1
if (-not $msbuild) { 'MSBUILD-NOT-FOUND'; exit 2 }
$gateArgument = if ($Gate -eq 'Analyzers') { @('/p:EnableNETAnalyzers=true', '/p:EnforceCodeStyleInBuild=true') } else { @('/p:TreatWarningsAsErrors=true') }
$argument = @('TaskMaster.sln', '/t:Rebuild', '/m', '/p:Configuration=Debug', '/p:Platform=Any CPU') + $gateArgument
"COMMAND: msbuild $($argument -join ' ')"
$logDirectory = Join-Path $WorkspaceRoot 'coverage'
New-Item -ItemType Directory -Force -Path $logDirectory | Out-Null
$logPath = Join-Path $logDirectory $LogName
& $msbuild @argument 2>&1 | ForEach-Object { [string]$_ } | Set-Content -LiteralPath $logPath -Encoding utf8
$exitCode = $LASTEXITCODE
$line = @(Get-Content -LiteralPath $logPath)
"EXIT_CODE: $exitCode"
"OUTPUT-ASSEMBLIES: $(@($line | Where-Object { $_ -match ' -> .+\.(dll|exe)\s*$' }).Count)"
foreach ($summary in @($line | Where-Object { $_ -match '^\s*\d+ (Warning|Error)\(s\)\s*$' })) { "SUMMARY: $($summary.Trim())" }
$errorLine = @($line | Where-Object { $_ -match ': error [A-Z]+[0-9]+' } | Sort-Object -Unique)
"ERROR-LINE-COUNT: $($errorLine.Count)"
foreach ($entry in @($errorLine | Select-Object -First 40)) { "ERROR: $(Format-SafeLine -Line $entry -Root @($WorkspaceRoot))" }
$pathLength = @($line | Where-Object { $_ -match 'PathTooLong|MSB3491|exceeds the maximum|too long' }).Count
"PATH-LENGTH-SIGNATURE-LINES: $pathLength"
exit $exitCode
```

**985-mstest.ps1**

```powershell
param([Parameter(Mandatory = $true)][string]$WorkspaceRoot, [Parameter(Mandatory = $true)][string]$EvidencePrefix)
Set-StrictMode -Version Latest
Set-Location -LiteralPath $WorkspaceRoot
$summaryPath = Join-Path $WorkspaceRoot 'coverage\test-results\mstest-coverage-run.summary.txt'
$trxPath = Join-Path $WorkspaceRoot 'coverage\test-results\mstest-coverage-run.trx'
$projectionPath = Join-Path $WorkspaceRoot 'coverage\coverage.cobertura.jacoco.xml'
foreach ($stale in @($summaryPath, $trxPath, $projectionPath)) { Remove-Item -LiteralPath $stale -Force -ErrorAction SilentlyContinue }
. (Join-Path $WorkspaceRoot 'scripts\vscode\Invoke-MSTestWithCoverage.ps1')
function Get-DotnetCoverageArgumentList {
    param(
        [Parameter(Mandatory = $true)][string]$OutputPath,
        [Parameter(Mandatory = $true)][string]$CoverageConfig,
        [Parameter(Mandatory = $true)][string]$VsTestPath,
        [Parameter(Mandatory = $true)][string[]]$TestAssembly,
        [Parameter(Mandatory = $true)][string]$RunSettingsPath,
        [Parameter(Mandatory = $true)][string]$ResultsDirectory,
        [Parameter(Mandatory = $true)][string]$LogFileName
    )
    $filter = '/TestCaseFilter:TestCategory!=LiveOutlook' +
    '&FullyQualifiedName!~HelperClasses.ShellUtilities_Tests' +
    '&FullyQualifiedName!~HelperClasses.ShellUtilitiesStatic_Tests' +
    '&FullyQualifiedName!~HelperClasses.SysImageListHelperTests' +
    '&FullyQualifiedName!~EmailIntelligence.OSBrowser_Tests'
    return @('collect', '--output', $OutputPath, '--output-format', 'cobertura', '--settings', $CoverageConfig, '--', $VsTestPath) +
    @($TestAssembly) +
    @("/Settings:$RunSettingsPath", '/InIsolation', $filter, '/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None',
        "/ResultsDirectory:$ResultsDirectory", "/Logger:trx;LogFileName=$LogFileName")
}
"SHELL-ICON-EXCLUSION: APPLIED ShellUtilities_Tests ShellUtilitiesStatic_Tests SysImageListHelperTests OSBrowser_Tests"
$status = 'COMPLETED'
try {
    Invoke-MSTestWithCoverageMain -ScriptRoot (Join-Path $WorkspaceRoot 'scripts\vscode') |
        ForEach-Object { Format-SafeLine -Line ([string]$_) -Root @($WorkspaceRoot) }
}
catch {
    $status = 'THREW'
    "RUNNER-ERROR: $(Format-SafeLine -Line $_.Exception.Message -Root @($WorkspaceRoot))"
}
"RUNNER_RESULT=$status"
if (Test-Path -LiteralPath $summaryPath) {
    Get-Content -LiteralPath $summaryPath | ForEach-Object { "TRX-SUMMARY: $_" }
    Copy-Item -LiteralPath $summaryPath -Destination ($EvidencePrefix + '.summary.txt') -Force
}
else { 'TRX-SUMMARY-MISSING' }
if (Test-Path -LiteralPath $projectionPath) {
    Copy-Item -LiteralPath $projectionPath -Destination ($EvidencePrefix + '.jacoco.xml') -Force
    'PROJECTION-COPIED'
}
else { 'PROJECTION-MISSING' }
if ($status -eq 'THREW' -and (Test-Path -LiteralPath $trxPath)) {
    [xml]$trx = Get-Content -LiteralPath $trxPath -Raw
    $failedName = @($trx.SelectNodes("//*[local-name()='UnitTestResult'][@outcome='Failed']") | ForEach-Object { $_.GetAttribute('testName') } | Sort-Object -Unique)
    "TRX-FAILED-COUNT: $($failedName.Count)"
    foreach ($name in $failedName) { "TRX-FAILED-TEST: $name" }
}
if ($status -eq 'THREW') { exit 1 }
exit 0
```

**985-pester.ps1**

```powershell
param(
    [Parameter(Mandatory = $true)][string]$WorkspaceRoot,
    [Parameter(Mandatory = $true)][string[]]$TestPath,
    [string[]]$CoveragePath = @(),
    [string]$CoverageOutput = 'coverage/985-pester-coverage.xml'
)
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
Set-Location -LiteralPath $WorkspaceRoot
$testPathList = @($TestPath | ForEach-Object { $_ -split ',' } | ForEach-Object { $_.Trim() } | Where-Object { $_ })
$coveragePathList = @($CoveragePath | ForEach-Object { $_ -split ',' } | ForEach-Object { $_.Trim() } | Where-Object { $_ })
"TEST-PATH-COUNT: $($testPathList.Count)"
"COVERAGE-PATH-COUNT: $($coveragePathList.Count)"
Import-Module Pester -MinimumVersion 5.6.1
"PESTER-VERSION: $((Get-Module Pester).Version)"
$configuration = New-PesterConfiguration
$configuration.Run.Path = $testPathList
$configuration.Run.PassThru = $true
$configuration.Output.Verbosity = 'Normal'
if ($coveragePathList.Count -gt 0) {
    New-Item -ItemType Directory -Force -Path (Split-Path -Parent (Join-Path $WorkspaceRoot $CoverageOutput)) | Out-Null
    $configuration.CodeCoverage.Enabled = $true
    $configuration.CodeCoverage.Path = $coveragePathList
    $configuration.CodeCoverage.OutputFormat = 'JaCoCo'
    $configuration.CodeCoverage.OutputPath = $CoverageOutput
}
$result = Invoke-Pester -Configuration $configuration
"PESTER Passed=$($result.PassedCount) Failed=$($result.FailedCount) Skipped=$($result.SkippedCount) NotRun=$($result.NotRunCount) Total=$($result.TotalCount)"
foreach ($group in @($result.Tests | Group-Object { [System.IO.Path]::GetFileName([string]$_.ScriptBlock.File) })) {
    $passed = @($group.Group | Where-Object { $_.Result -eq 'Passed' }).Count
    $failed = @($group.Group | Where-Object { $_.Result -eq 'Failed' }).Count
    "FILE-TESTS $($group.Name) Passed=$passed Failed=$failed Total=$($group.Count)"
}
foreach ($container in @($result.Containers | Where-Object { $_.Result -ne 'Passed' })) {
    "CONTAINER-NOT-PASSED $([System.IO.Path]::GetFileName([string]$container.Item)) Result=$($container.Result)"
}
foreach ($test in @($result.Failed)) {
    "FAILED-TEST: $($test.ExpandedPath)"
    $record = @($test.ErrorRecord) | Select-Object -First 1
    $message = if ($null -ne $record) { [string]$record.Exception.Message } else { '' }
    "FAILED-MESSAGE: $(Format-SafeLine -Line (($message -split "`r?`n") -join ' ') -Root @($WorkspaceRoot))"
}
if ($coveragePathList.Count -gt 0) {
    [xml]$jacoco = Get-Content -LiteralPath $CoverageOutput -Raw
    $lineCounter = @($jacoco.SelectNodes('/report/counter')) | Where-Object { $_.type -eq 'LINE' }
    $covered = [int]$lineCounter.covered
    $total = $covered + [int]$lineCounter.missed
    $percent = if ($total -gt 0) { 100 * $covered / $total } else { 0 }
    "COVERAGE LinePercent=$($percent.ToString('0.00')) Covered=$covered Total=$total"
    foreach ($file in @($jacoco.SelectNodes('/report/package/sourcefile'))) {
        $fileCounter = @($file.SelectNodes('counter')) | Where-Object { $_.type -eq 'LINE' }
        if ($null -eq $fileCounter) { continue }
        $fileCovered = [int]$fileCounter.covered
        $fileTotal = $fileCovered + [int]$fileCounter.missed
        $filePercent = if ($fileTotal -gt 0) { 100 * $fileCovered / $fileTotal } else { 0 }
        "FILE-COVERAGE $($file.ParentNode.name)/$($file.name) LinePercent=$($filePercent.ToString('0.00')) Covered=$fileCovered Total=$fileTotal"
    }
}
if ($result.FailedCount -gt 0 -or @($result.Containers | Where-Object { $_.Result -eq 'Failed' }).Count -gt 0) { exit 1 }
exit 0
```

**985-junit.ps1**

```powershell
param([Parameter(Mandatory = $true)][string]$WorkspaceRoot, [switch]$Clear)
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$path = Join-Path $WorkspaceRoot 'artifacts\pester\pester-junit.xml'
if ($Clear) {
    Remove-Item -LiteralPath $path -Force -ErrorAction SilentlyContinue
    "JUNIT-CLEARED: $(-not (Test-Path -LiteralPath $path))"
    exit 0
}
if (-not (Test-Path -LiteralPath $path)) { 'JUNIT-MISSING'; exit 1 }
[xml]$junit = Get-Content -LiteralPath $path -Raw
$root = $junit.DocumentElement
"JUNIT tests=$($root.GetAttribute('tests')) failures=$($root.GetAttribute('failures')) errors=$($root.GetAttribute('errors')) disabled=$($root.GetAttribute('disabled'))"
foreach ($suite in @($junit.SelectNodes('//testsuite'))) {
    "JUNIT-SUITE $([System.IO.Path]::GetFileName($suite.GetAttribute('name'))) tests=$($suite.GetAttribute('tests')) failures=$($suite.GetAttribute('failures'))"
}
exit 0
```

**985-repair.ps1**

```powershell
param([Parameter(Mandatory = $true)][string]$WorkspaceRoot, [switch]$WhatIfRun)
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
Set-Location -LiteralPath $WorkspaceRoot
$entry = Join-Path $WorkspaceRoot 'scripts\dependencies\Repair-PackageManifestConsistency.ps1'
"COMMAND: & WORKSPACE-ROOT\scripts\dependencies\Repair-PackageManifestConsistency.ps1$(if ($WhatIfRun) { ' -WhatIf' })"
$result = if ($WhatIfRun) { & $entry -WhatIf } else { & $entry }
"IS-SUCCESS: $($result.IsSuccess)"
"WRITTEN-COUNT: $(@($result.WrittenPath).Count)"
foreach ($path in @($result.WrittenPath)) { "WRITTEN: $([System.IO.Path]::GetRelativePath($WorkspaceRoot, $path))" }
"REPAIR-COUNT: $($result.RepairCount)"
$kind = @($result.Verification | ForEach-Object { $_.Report.Repair } | Where-Object { $null -ne $_ } | ForEach-Object { $_.Kind })
"BEYOND-KNOWN-WEAK: $(@($kind | Where-Object { $_ -ne 'Analyzer' }).Count)"
"REPORT-REPAIR-KINDS: $(@($kind | Sort-Object -Unique) -join ',')"
"SKIP-COUNT: $(@($result.Skipped).Count)"
"REDIRECTSYNC-REPAIR-COUNT: $(@($result.RedirectSync.Repair).Count)"
foreach ($record in @($result.RedirectSync.Repair)) {
    "REDIRECTSYNC: $($record.ProjectDirectory) $($record.AssemblyName) $($record.From) to $($record.To) $($record.Rule)"
}
"REDIRECTSYNC-UNVERIFIABLE: $(@($result.RedirectSync.Unverifiable) -join ',')"
"REDIRECTSYNC-UNRESOLVABLE: $(@($result.RedirectSync.Unresolvable) -join ',')"
foreach ($failure in @($result.Failure)) { "FAILURE: $($failure.ProjectName): $($failure.Condition)" }
'BODY-BEGIN'
foreach ($line in @($result.Body -split "`r?`n")) { Format-SafeLine -Line $line -Root @($WorkspaceRoot) }
'BODY-END'
if (-not $result.IsSuccess) { exit 1 }
exit 0
```

**985-nuget-update.ps1**

```powershell
param(
    [Parameter(Mandatory = $true)][string]$WorkspaceRoot,
    [Parameter(Mandatory = $true)][string]$ProjectDirectory,
    [Parameter(Mandatory = $true)][string]$PackageId,
    [Parameter(Mandatory = $true)][string]$PackageVersion
)
Set-StrictMode -Version Latest
Set-Location -LiteralPath $WorkspaceRoot
$manifest = Join-Path (Join-Path $WorkspaceRoot $ProjectDirectory) 'packages.config'
$repository = Join-Path $WorkspaceRoot 'packages'
$line = @(& nuget update $manifest -Id $PackageId -Version $PackageVersion -RepositoryPath $repository -NonInteractive -FileConflictAction Ignore 2>&1 | ForEach-Object { [string]$_ })
$exitCode = $LASTEXITCODE
"COMMAND: nuget update $ProjectDirectory\packages.config -Id $PackageId -Version $PackageVersion -RepositoryPath WORKSPACE-ROOT\packages -NonInteractive -FileConflictAction Ignore"
"EXIT_CODE: $exitCode"
foreach ($entry in @($line | Select-Object -Last 8)) { "TAIL: $(Format-SafeLine -Line $entry -Root @($WorkspaceRoot))" }
exit $exitCode
```

## 6. Implementation Contracts

### 6.1 `scripts/dependencies/BindingRedirectSync.psm1` (new, ASCII-only, at most 500 lines)

Header comment-based help states: detection lives in `BindingRedirectVerification.psm1`; this module is the writer that keeps every `bindingRedirect` `newVersion` inside the solution-wide csproj `Reference` version map, covering assemblies a project receives only transitively (issue #985). `Set-StrictMode -Version Latest`. Imports, without `-Force` (same reason as `BindingRedirectVerification.psm1` lines 26-29): `PackageGraph.psm1`, `BindingRedirectVerification.psm1`, `ProjectConsistency.psm1`, each via `Join-Path $PSScriptRoot`. `Export-ModuleMember -Function` exactly `Invoke-BindingRedirectSync`, `Invoke-SolutionBindingRedirectSync`, `Format-BindingRedirectSyncReport`.

1. `Invoke-BindingRedirectSync` (`[CmdletBinding()]`, `[OutputType([pscustomobject])]`): parameters `-AppConfigText` (mandatory, `[AllowEmptyString()]`), `-DeployedVersionProvider` (mandatory scriptblock: assembly name in, version strings out), `-PreferredVersionProvider` (optional scriptblock, default `$null`: assembly name in, the project's own Reference versions out). Returns `[pscustomobject]` with `PSTypeName = 'BindingRedirectSync.Result'`, `Text`, `Repair` (array), `Unverifiable` (distinct names, array), `Unresolvable` (distinct names, array), `ExaminedCount`.
   - Empty or whitespace text: `ExaminedCount` 0, text unchanged, no throw. Otherwise records come from `ConvertFrom-AppConfigText` (which throws for non-configuration text; the exception is not caught).
   - Each record with a non-empty `NewVersion` increments `ExaminedCount`. A name already handled in this call is skipped.
   - Deployed set = distinct non-empty strings from the provider. Empty set: add the name to `Unverifiable`, no change.
   - `NewVersion` in the deployed set (ordinal string comparison, as `Find-StaleBindingRedirect` line 118): no change.
   - Otherwise target selection: (a) when `PreferredVersionProvider` is supplied and returns exactly one distinct non-empty version and that version is in the deployed set, target = that version, `Rule = 'OwnReference'`; (b) otherwise, if every deployed version parses with `[System.Version]::TryParse`, target = the maximum by `[System.Version]` comparison, `Rule = 'HighestDeployed'`; (c) otherwise add the name to `Unresolvable`, no change.
   - Apply with `Invoke-BindingRedirectReconciliation -AppConfigText <current text> -AssemblyName <name> -AssemblyVersion <target>` and continue with its `.Text`. Add one repair record: `PSTypeName = 'BindingRedirectSync.Repair'`, `Kind = 'BindingRedirectSync'`, `AssemblyName`, `From` (the original `NewVersion`), `To` (target), `Rule`.
2. `Invoke-SolutionBindingRedirectSync` (`[CmdletBinding(SupportsShouldProcess = $true)]`): parameters `-DirectoryLister`, `-TextReader`, `-TextWriter` (mandatory scriptblocks, same contracts as `Invoke-ManifestNormalization`, `PackageGraph.psm1` lines 405-418) and `-ProjectTextOverride` (hashtable, default empty; key = csproj path, value = post-repair text).
   - Project paths = lister output whose extension is `.csproj` (ordinal case-insensitive) and that has no `packages`, `bin`, `obj` or `node_modules` parent segment (same exclusion as `Get-PackageManifestPath`, lines 112-124), sorted. Project text = override value when the override contains the path, else reader output.
   - Map = `ConvertTo-ReferenceVersionMap -ProjectText <all project texts>`. Deployed provider = map lookup (empty when absent).
   - For every path from `Get-PackageManifestPath -Kind AppConfig -DirectoryLister`: preferred provider = `ConvertTo-ReferenceVersionMap` over the project text(s) in the same directory (empty provider when none); call function 1; when the returned text differs (`-cne`) and `$PSCmdlet.ShouldProcess(<path>, 'Synchronise binding redirects')`, call the writer and add the path to `ChangedPath`. Each repair record gains `Path` (the app.config path) and `ProjectDirectory` (`Split-Path -Leaf (Split-Path -Parent <path>)`).
   - Emits exactly one `Write-Information` line with `-InformationAction Continue`: `Binding redirect sync: examined <n> application configuration file(s), synchronised <m> redirect(s), unverifiable <u>, unresolvable <r>`.
   - Returns `PSTypeName = 'BindingRedirectSync.SolutionResult'`, `ChangedPath`, `Repair`, `Unverifiable` (distinct, sorted), `Unresolvable` (distinct, sorted), `ExaminedAppConfig`.
3. `Format-BindingRedirectSyncReport`: parameter `-Repair` (mandatory, `[AllowEmptyCollection()]`, `[object[]]`). Empty input returns `''`. Otherwise returns the lines `## Binding redirects synchronised` then one line per record `- <ProjectDirectory>: <AssemblyName> <From> to <To> (<Rule>)`, joined with `[System.Environment]::NewLine`.

Parameters referenced only inside nested scriptblocks are bound to locals first (the pattern at `ProjectConsistency.psm1` lines 323-326) so PSReviewUnusedParameter stays clean.

### 6.2 `scripts/dependencies/Repair-PackageManifestConsistency.ps1` wiring (wiring only; final length at most 500 lines)

1. After the `ConsistencyVerifier.psm1` import (line 71): `Import-Module (Join-Path $PSScriptRoot 'BindingRedirectSync.psm1')` (no `-Force`).
2. After `$written = ...` (line 382): `$projectTextOverride = @{}`.
3. Inside the manifest loop, immediately after the block that writes the repaired project (lines 413-417): `$projectTextOverride[$project[0]] = $repaired.Text`.
4. After the loop's closing brace (line 441) and before the normalisation comment (line 443): a two-line comment stating that the sync runs on every invocation (with or without `-CandidateUpgrade`) and before normalisation so normalisation stays the last writer, and that `-WhatIf` is passed explicitly for the module-preference reason given at lines 443-444; then `$redirectSync = Invoke-SolutionBindingRedirectSync -DirectoryLister $lister -TextReader $reader -TextWriter $writer -ProjectTextOverride $projectTextOverride -WhatIf:$WhatIfPreference` and `foreach ($path in @($redirectSync.ChangedPath)) { $written.Add($path) }`.
5. Result object: add `RedirectSync = [pscustomobject]@{ Repair = @($redirectSync.Repair); Unverifiable = @($redirectSync.Unverifiable); Unresolvable = @($redirectSync.Unresolvable) }`. `Body`: the existing `& $script:ReportBody ...` text, followed, only when `Format-BindingRedirectSyncReport -Repair @($redirectSync.Repair)` is non-empty, by `[System.Environment]::NewLine` twice and that block. `IsSuccess` and `Verification` are unchanged; no sync record enters `Verification[].Report.Repair`.
6. Update the `.DESCRIPTION` help sentence listing the passes so it names the binding-redirect sync pass after binding-redirect reconciliation, if the line budget allows; the 500-line ceiling takes precedence.

### 6.3 Test files

`tests/scripts/dependencies/Repair-PackageManifestConsistency.RedirectSync.Tests.ps1` (new). Top: `Set-StrictMode -Version Latest`; `BeforeAll` sets `$script:RepoRoot` and `$script:EntryPoint` exactly as `Repair-PackageManifestConsistency.Tests.ps1` lines 4-5 and defines a local `Get-RepairFixture` with the same body as that file's lines 86-120. No block or test name matches the regex `AC` followed by a digit. Fixture store (all in memory; CRLF normalisation exactly as that file's lines 59-61: manifests `-replace "`r?`n", "`r`n"` plus a trailing `"`r`n"`, project and app.config texts `-replace` only):
- `X:\fixture\Prod\packages.config`: `log4net` version `3.5.0`, `targetFramework="net481"`.
- `X:\fixture\Prod\Prod.csproj`: `<Reference Include="log4net, Version=3.5.0.0, Culture=neutral, PublicKeyToken=669e0ddf0bb1aa2a, processorArchitecture=MSIL">` with HintPath `..\packages\log4net.3.5.0\lib\net462\log4net.dll`.
- `X:\fixture\Test\packages.config`: `Fabrikam.Core` version `1.0.0`.
- `X:\fixture\Test\Test.csproj`: Reference `Fabrikam.Core, Version=1.0.0.0` with HintPath `..\packages\Fabrikam.Core.1.0.0\lib\net472\Fabrikam.Core.dll` (no log4net reference).
- `X:\fixture\Test\app.config`: two `dependentAssembly` blocks: `log4net` (`publicKeyToken="669e0ddf0bb1aa2a"`) `oldVersion="0.0.0.0-3.4.0.0" newVersion="3.4.0.0"`; `Fabrikam.Core` `oldVersion="0.0.0.0-1.0.0.0" newVersion="1.0.0.0"`.
- Identity map: `log4net|3.5.0` to `net462`/`3.5.0.0`; `Fabrikam.Core|1.0.0` to `net472`/`1.0.0.0`. Asset and listing maps empty.

`Describe 'Repair-PackageManifestConsistency redirect synchronisation (issue 985)'`:
- `Context 'A transitive redirect left stale by an upgrade elsewhere, with no candidate upgrade supplied'`; its `BeforeAll` runs the entry point twice over one store without `-CandidateUpgrade`, keeping both results; it reads no `RedirectSync` property. Tests:
  - R1 `It 'rewrites the transitive redirect in both positions'`: the Test app.config matches `oldVersion="0\.0\.0\.0-3\.5\.0\.0"` and `newVersion="3\.5\.0\.0"` and still matches the Fabrikam `newVersion="1\.0\.0\.0"`.
  - R2 `It 'reports the rewritten application configuration in the written paths'`: first-run `WrittenPath` contains `X:\fixture\Test\app.config`.
  - R3 `It 'carries the synchronised-redirect block in the body'`: first-run `Body` matches `## Binding redirects synchronised` and `log4net 3\.4\.0\.0 to 3\.5\.0\.0`.
  - R4 `It 'exposes the synchronisation in the RedirectSync field and not in the per-project repair records'`: `@($result.RedirectSync.Repair).Count` is 1 with `AssemblyName` `log4net`, `From` `3.4.0.0`, `To` `3.5.0.0`, `Rule` `HighestDeployed`; the `Kind` values across `Verification[].Report.Repair` do not contain `BindingRedirectSync`.
  - R5 `It 'writes nothing on a second run over the repaired store'`: second-run `WrittenPath` count 0.
- `Context 'The same tree run with -WhatIf'`:
  - R6 `It 'leaves the stale redirect unchanged and writes nothing'`: fresh fixture, `-WhatIf`; store app.config still matches `newVersion="3\.4\.0\.0"`; `WrittenPath` count 0.

`tests/scripts/dependencies/BindingRedirectSync.Tests.ps1` (new; imports the module with `-Force` from `$script:RepoRoot`; every fixture in memory; CRLF fixtures built with `-replace "`r?`n", "`r`n"`):
- `Describe 'Invoke-BindingRedirectSync (in-memory fixtures)'`: S1 `'rewrites a stale redirect in both positions and keeps the oldVersion lower bound'` (lower bound `1.0.0.0` kept); S2 `'replaces a single-version oldVersion outright'`; S3 `'leaves a redirect whose newVersion is already deployed unchanged and reports no repair on a second pass'`; S4 `'reports an assembly with no deployed version as unverifiable and leaves it unchanged'`; S5 `'prefers the project own reference version when several versions are deployed'` (deployed `3.4.0.0`,`3.5.0.0`, preferred `3.4.0.0`, stale `3.3.0.0`, Rule `OwnReference`); S6 `'selects the highest deployed version by numeric comparison when no own reference exists'` (deployed `3.9.0.0`,`3.10.0.0`, target `3.10.0.0`); S7 `'falls back to the highest deployed version when the own reference names more than one version'`; S8 `'reports an unparsable deployed version as unresolvable and leaves the redirect unchanged'`; S9 `'skips a dependentAssembly block that carries no bindingRedirect'`; S10 `'examines zero entries and changes nothing for empty text'`; S11 `'throws when the text is not an application configuration document'`; S12 `'keeps every byte outside the substituted attribute values identical in CRLF text'` (replacing the two new values by the old ones yields text `-ceq` the input).
- `Describe 'Invoke-SolutionBindingRedirectSync (in-memory store)'` (hashtable store, lister returns the keys): S13 `'synchronises the transitive redirect of a project that does not reference the assembly'`; S14 `'prefers the project text override over the text the reader returns'` (reader csproj says `3.5.0.0`, override says `3.6.0.0`, target `3.6.0.0`); S15 `'writes nothing when run with -WhatIf'`; S16 `'reports exactly the changed application configuration paths'` (`ChangedPath` equals the single Test app.config path; an already-current Prod app.config is absent).
- `Describe 'Format-BindingRedirectSyncReport'`: S17 `'returns an empty string when there are no repairs'`; S18 `'returns the heading and one line per repair'` (two records give three lines).

`tests/scripts/dependencies/RepositoryTreeConsistency.Tests.ps1`: one new `It` appended inside `Describe 'Repository tree consistency (issue 929)'` after the existing Import `It` (which ends at line 90):

```powershell
    It 'reports no HintPath whose package folder the sibling manifest does not declare, for every project directory that carries a manifest' {
        # Arrange: the pair discovery of the Import gate above, so both gates read one population.
        $pair = @(Get-ChildItem -LiteralPath $script:RepoRoot -Directory | ForEach-Object {
                $manifest = Join-Path $_.FullName 'packages.config'
                $project = @(Get-ChildItem -LiteralPath $_.FullName -File | Where-Object { $_.Extension -eq '.csproj' })
                if ((Test-Path -LiteralPath $manifest) -and $project.Count -eq 1) {
                    [pscustomobject]@{ Project = $project[0].FullName; Manifest = $manifest }
                }
            })

        # Act
        $examined = 0
        $finding = [System.Collections.Generic.List[string]]::new()
        foreach ($entry in $pair) {
            $detection = Find-OrphanedHintPath -ProjectText ([System.IO.File]::ReadAllText($entry.Project)) -ManifestText ([System.IO.File]::ReadAllText($entry.Manifest))
            $examined += $detection.ExaminedCount
            foreach ($item in @($detection.Finding)) {
                $finding.Add(('{0}: line {1} {2}' -f (Split-Path -Leaf $entry.Project), $item.LineNumber, $item.PackageFolder))
            }
        }

        # Assert: the examined count guards the zero finding count.
        $examined | Should -BeGreaterThan 0 -Because 'a zero examined count would mean the detector never fired'
        $finding.Count | Should -Be 0 -Because ('these HintPath elements name a package folder the sibling manifest does not declare: ' + ($finding -join '; '))
    }
```

## 7. Phases

### Phase 0 — Policy Reads and Baselines

- [x] [P0-T1] Read, in this order, `CLAUDE.md`, `.claude/rules/general-code-change.md`, `.claude/rules/general-unit-test.md`, `.claude/rules/quality-tiers.md`, `.claude/rules/powershell.md`, `.claude/rules/csharp.md`, `.claude/rules/tonality.md`, `.claude/rules/plan-acceptance-gates.md`, `.claude/skills/atomic-plan-contract/SKILL.md`, `.claude/skills/evidence-and-timestamp-conventions/SKILL.md`, `.claude/skills/acceptance-criteria-tracking/SKILL.md`, then `FEATURE/issue.md`, `FEATURE/spec.md`, `FEATURE/research/research.2026-10-09T14-10.md`; write `FEATURE/evidence/baseline/phase0-instructions-read.md` with `Timestamp:`, `Policy Order:` (the order above) and `Files Read:` listing all fourteen paths. Acceptance: the artifact exists with the three fields and fourteen listed paths. Evidence: `FEATURE/evidence/baseline/phase0-instructions-read.md`.
- [x] [P0-T2] Record git state and tool availability in `FEATURE/evidence/baseline/git-and-tools.<TS>.md`: run `git rev-parse --abbrev-ref HEAD` (must print `FIX-BRANCH`), `git rev-parse HEAD`, `git merge-base HEAD origin/main` (record as `BASE-SHA:`), `git diff --name-only BASE-SHA HEAD` (record; every listed path must lie under `FEATURE`, `.claude/agent-memory/` or `docs/features/potential/`), and `git status --porcelain` (record every line verbatim as `PreExistingWorktreePaths:`); then write `CMDDIR\985-probe.ps1` (section 5) and run `pwsh -NoProfile -File <CMDDIR>\985-probe.ps1 -RehearsalRoot <REHEARSAL>`. Acceptance: `PESTER-AVAILABLE` is 5.6.1 or later, `NUGET: FOUND`, `VSWHERE: True`, `MSBUILD: FOUND`, `DOTNET-COVERAGE: FOUND`; `LONG-PATHS-ENABLED` and `REHEARSAL-ROOT-LENGTH` recorded. A missing tool is STOP: TOOL-MISSING; a refused `pwsh` is STOP: PWSH-UNAVAILABLE. Evidence: `FEATURE/evidence/baseline/git-and-tools.2026-10-09T14-04.md`.
- [x] [P0-T3] Write the remaining eight helper scripts of section 5 (`985-restore.ps1`, `985-csharpier.ps1`, `985-msbuild.ps1`, `985-mstest.ps1`, `985-pester.ps1`, `985-junit.ps1`, `985-repair.ps1`, `985-nuget-update.ps1`) into `CMDDIR` verbatim, each with the `Format-SafeLine` function pasted after its `param` block, and record the nine file names plus `git hash-object --no-filters` of each in `FEATURE/evidence/baseline/helper-scripts.<TS>.md`. Acceptance: nine names and nine hashes recorded; the hashes are re-checked unchanged by P8-T1. Evidence: `FEATURE/evidence/baseline/helper-scripts.2026-10-09T14-04.md`.
- [x] [P0-T4] Record pre-change tree facts in `FEATURE/evidence/baseline/tree-facts.<TS>.md` with the Grep tool: line counts (pattern `^`, count) of `QuickFiler.Test/packages.config` (74), `UtilitiesCS.Test/packages.config` (110), `TaskTree.Test/packages.config` (69), `QuickFiler.Test/QuickFiler.Test.csproj` (572), `scripts/dependencies/Repair-PackageManifestConsistency.ps1` (475), `tests/scripts/dependencies/RepositoryTreeConsistency.Tests.ps1` (152); carriage-return counts (pattern `\r$`) of the three manifests (73, 109, 68); count of `id="Microsoft\.Web\.WebView2"|id="ObjectListView\.Official"` in the three test manifests (QuickFiler.Test 0, UtilitiesCS.Test 1 — the declared ObjectListView.Official at line 70 —, TaskTree.Test 0); count of `Include="Microsoft\.Web\.WebView2\.Core,` in the csproj (2); and the absence of `scripts/dependencies/BindingRedirectSync.psm1`, `tests/scripts/dependencies/BindingRedirectSync.Tests.ps1` and `tests/scripts/dependencies/Repair-PackageManifestConsistency.RedirectSync.Tests.ps1` (Glob returns none). Acceptance: every observed value equals the parenthesised value; any difference is recorded and reported, and the affected task anchors in Phases 1-3 are re-derived before continuing. Evidence: `FEATURE/evidence/baseline/tree-facts.2026-10-09T14-04.md`.
- [x] [P0-T5] PowerShell baseline format: record `git status --porcelain` and `git hash-object --no-filters` of `scripts/dependencies/Repair-PackageManifestConsistency.ps1` and `tests/scripts/dependencies/RepositoryTreeConsistency.Tests.ps1`, call `mcp__drm-copilot__run_poshqc_format` with `workspace_root = WORKSPACE-ROOT` and `scan_folders = [scripts/dependencies, tests/scripts/dependencies]`, then repeat both observations; write `FEATURE/evidence/baseline/ps-format.<TS>.md` (`EXIT_CODE:` 0 when `ok` is true, else 1; the MCP `summary` verbatim). Acceptance (success-case observation, not the exit code): porcelain identical before and after and both hashes identical; a rewrite is STOP: BASELINE-FORMAT-DRIFT (revert with `git checkout -- <file>` first). Evidence: `FEATURE/evidence/baseline/ps-format.2026-10-09T14-04.md`.
- [x] [P0-T6] PowerShell baseline lint: call `mcp__drm-copilot__run_poshqc_analyze` with `workspace_root = WORKSPACE-ROOT` and `scan_folders = [scripts/dependencies, tests/scripts/dependencies]`; write `FEATURE/evidence/baseline/ps-analyze.<TS>.md` (`EXIT_CODE:` 0 when `ok` is true, else 1). Acceptance: `ok` true; false is STOP: ANALYZE-BASELINE-RED. Type checking does not apply to PowerShell (`.claude/rules/powershell.md` toolchain step 3); the artifact states so. Evidence: `FEATURE/evidence/baseline/ps-analyze.2026-10-09T14-04.md`.
- [x] [P0-T7] PowerShell baseline MCP test: run `pwsh -NoProfile -File <CMDDIR>\985-junit.ps1 -WorkspaceRoot <WORKTREE> -Clear`, call `mcp__drm-copilot__run_poshqc_test` with `workspace_root = WORKSPACE-ROOT` and `scan_folders = [tests/scripts/dependencies]`, then run `985-junit.ps1` without `-Clear`; write `FEATURE/evidence/baseline/ps-test-mcp.<TS>.md` with the `ok` flag and the `JUNIT` and `JUNIT-SUITE` lines (`EXIT_CODE:` 1 when failures plus errors is above 0, else 0). Acceptance: `JUNIT-CLEARED: True`, a `JUNIT` line present, failures 0 and errors 0; otherwise STOP: BASELINE-TEST-RED. Evidence: `FEATURE/evidence/baseline/ps-test-mcp.2026-10-09T14-04.md`.
- [x] [P0-T8] PowerShell baseline direct coverage (CI mirror, `.github/workflows/_pester.yml` lines 41 and 45): run `pwsh -NoProfile -File <CMDDIR>\985-pester.ps1 -WorkspaceRoot <WORKTREE> -TestPath tests/scripts/dependencies,tests/scripts/hygiene,tests/scripts/vscode -CoveragePath scripts/dependencies,scripts/hygiene,scripts/vscode -CoverageOutput coverage/985-pester-baseline.xml`; write `FEATURE/evidence/baseline/ps-coverage.<TS>.md` carrying the `PESTER`, `COVERAGE` and every `FILE-TESTS` line and the `FILE-COVERAGE` line ending `/Repair-PackageManifestConsistency.ps1`. Acceptance: `TEST-PATH-COUNT: 3` and `COVERAGE-PATH-COUNT: 3`; `Failed=0` (otherwise STOP: BASELINE-TEST-RED); `BASELINE-PS-AGGREGATE:` and `BASELINE-REPAIR-SCRIPT-LINE:` recorded as numbers; the raw document stays under the ignored `coverage/` directory. Evidence: `FEATURE/evidence/baseline/ps-coverage.2026-10-09T14-04.md`.
- [x] [P0-T9] C# baseline format: first run `pwsh -NoProfile -File <WORKTREE>\scripts\vscode\Install-RepoDotNetSdk.ps1` (installs the `global.json` SDK into `.dotnet-sdk/`, ignored by the `.gitignore` rule `.dotnet*/`; without it csharpier exits -2147450725 with the global.json message) and record its exit code as `SDK-INSTALL-EXIT_CODE:`; only when it is 0, run `pwsh -NoProfile -File <CMDDIR>\985-csharpier.ps1 -WorkspaceRoot <WORKTREE> -Mode restore`, then `-Mode check` (read-only, so no rewrite occurs at baseline); write `FEATURE/evidence/baseline/cs-format.<TS>.md`. Acceptance: SDK install, restore and check all `EXIT_CODE: 0`; a non-zero SDK install or check is STOP: CS-BASELINE-RED (the check never modifies `QuickFiler.Test/packages.config` or any manifest, which `.csharpierignore` lines 15-18 exclude). Evidence: `FEATURE/evidence/baseline/cs-format.2026-10-09T14-04.md`.
- [x] [P0-T10] C# baseline restore: run `pwsh -NoProfile -File <CMDDIR>\985-restore.ps1 -WorkspaceRoot <WORKTREE>` (restores `TaskMaster.sln` into `packages/`); write `FEATURE/evidence/baseline/cs-restore.<TS>.md`. Acceptance: `EXIT_CODE: 0`; otherwise STOP: CS-BASELINE-RED. Evidence: `FEATURE/evidence/baseline/cs-restore.2026-10-09T14-04.md`.
- [x] [P0-T11] C# baseline analyzers: run `pwsh -NoProfile -File <CMDDIR>\985-msbuild.ps1 -WorkspaceRoot <WORKTREE> -Gate Analyzers -LogName 985-baseline-analyzers.log` (log under ignored `coverage/`); write `FEATURE/evidence/baseline/cs-analyzers.<TS>.md`. Acceptance: `EXIT_CODE: 0`, a `SUMMARY: 0 Error(s)` line, `OUTPUT-ASSEMBLIES:` recorded as `BASELINE-OUTPUT-ASSEMBLIES:`; otherwise STOP: CS-BASELINE-RED. Evidence: `FEATURE/evidence/baseline/cs-analyzers.2026-10-09T14-04.md`.
- [x] [P0-T12] C# baseline type-check: run `985-msbuild.ps1 -WorkspaceRoot <WORKTREE> -Gate Nullable -LogName 985-baseline-nullable.log` from `CMDDIR`; write `FEATURE/evidence/baseline/cs-nullable.<TS>.md`. Acceptance: `EXIT_CODE: 0`, `SUMMARY: 0 Error(s)`, `OUTPUT-ASSEMBLIES:` equal to P0-T11's; otherwise STOP: CS-BASELINE-RED. Evidence: `FEATURE/evidence/baseline/cs-nullable.2026-10-09T14-04.md`.
- [x] [P0-T13] C# baseline tests with coverage (route D3): run `pwsh -NoProfile -File <CMDDIR>\985-mstest.ps1 -WorkspaceRoot <WORKTREE> -EvidencePrefix <WORKTREE>\FEATURE\evidence\baseline\mstest-coverage-baseline.<TS>`; write `FEATURE/evidence/baseline/cs-test.<TS>.md` with `SHELL-ICON-EXCLUSION`, the `Discovered ... test assemblies` line, the `First-party coverage:` line, `RUNNER_RESULT` and every `TRX-SUMMARY` line; the script copies `mstest-coverage-baseline.<TS>.summary.txt` and `mstest-coverage-baseline.<TS>.jacoco.xml` beside it. Acceptance: `RUNNER_RESULT=COMPLETED` (after the C7 flaky rule, if it applies), `TRX-SUMMARY: Failed tests: none`, both copies present, `BASELINE-CS-LINE:` and `BASELINE-CS-BRANCH:` recorded as numbers; otherwise STOP: BASELINE-TEST-RED. Evidence: `FEATURE/evidence/baseline/cs-test.2026-10-09T14-11.md`.
- [x] [P0-T14] Baseline side-effect check for `QuickFiler.Test/QuickFiler.Test.csproj` and every other tracked file: run `git status --porcelain`; write `FEATURE/evidence/baseline/post-baseline-porcelain.<TS>.md`. Acceptance: excluding `.claude/agent-memory/` lines (recorded as `AGENT-MEMORY-LINES:`, C10), every line is either in `PreExistingWorktreePaths` or under `FEATURE/evidence/`; a modified tracked file (for example a csproj rewritten by a build step) is recorded, reverted with `git checkout -- <file>`, and reported. Evidence: `FEATURE/evidence/baseline/post-baseline-porcelain.2026-10-09T14-13.md`.

### Phase 1 — Regression Tests First

- [x] [P1-T1] Append the new `It` of section 6.3 to `tests/scripts/dependencies/RepositoryTreeConsistency.Tests.ps1` inside the existing `Describe`, after the Import `It` that ends at line 90, with the Edit tool (CRLF preserved). Acceptance: Grep count of `It 'reports no HintPath whose package folder the sibling manifest does not declare` is 1, of `Find-OrphanedHintPath -ProjectText` is 1, and of `AC[0-9]` in the file's `Describe`/`Context`/`It` lines is 0. Evidence: Grep counts 1, 1, 0 observed; file 178 lines, 178 CRLF (verified at execution; recorded in `FEATURE/evidence/regression-testing/fail-before-orphaned-hintpath.<TS>.md`).
- [x] [P1-T2] Create `tests/scripts/dependencies/Repair-PackageManifestConsistency.RedirectSync.Tests.ps1` per section 6.3 (six `It` blocks R1 to R6, in-memory store only). Acceptance: Grep count of `^\s+It '` is 6; Grep count of `New-TemporaryFile|GetTempPath|GetTempFileName|TestDrive|WriteAllText|Set-Content|Out-File|New-Item` is 0; Grep count of `RedirectSync` inside the `BeforeAll` blocks is 0 (read the file to confirm). Evidence: Grep counts 6, 0, 0 observed (RedirectSync only at lines 161-170, inside an `It`); recorded in `FEATURE/evidence/regression-testing/fail-before-redirect-sync.<TS>.md`.
- [x] [P1-T3] [expect-fail] Run `pwsh -NoProfile -File <CMDDIR>\985-pester.ps1 -WorkspaceRoot <WORKTREE> -TestPath tests/scripts/dependencies/RepositoryTreeConsistency.Tests.ps1`; write `FEATURE/evidence/regression-testing/fail-before-orphaned-hintpath.<TS>.md` with `ExpectedExitCode: 1` and the findings list copied from `FAILED-MESSAGE`. Acceptance (right reason): `EXIT_CODE: 1`; `PESTER ... Failed=1 ... Total=5`; the single `FAILED-TEST:` names the new `It`; its `FAILED-MESSAGE:` contains each of `QuickFiler.Test.csproj: line`, `UtilitiesCS.Test.csproj: line` and `TaskTree.Test.csproj: line`. Any other failure shape is STOP: WRONG-REASON. The finding count is recorded (research predicts 7) but not gated. Evidence: `FEATURE/evidence/regression-testing/fail-before-orphaned-hintpath.2026-10-09T14-15.md`.
- [x] [P1-T4] [expect-fail] Run `985-pester.ps1 -WorkspaceRoot <WORKTREE> -TestPath tests/scripts/dependencies/Repair-PackageManifestConsistency.RedirectSync.Tests.ps1` from `CMDDIR`; write `FEATURE/evidence/regression-testing/fail-before-redirect-sync.<TS>.md` with `ExpectedExitCode: 1`. Acceptance (right reason): `EXIT_CODE: 1`; `Failed=4 ... Total=6`; the failed set is exactly R1, R2, R3, R4 (names as in section 6.3); R1's `FAILED-MESSAGE:` contains `newVersion`; R5 and R6 passed. Any other shape is STOP: WRONG-REASON. Evidence: `FEATURE/evidence/regression-testing/fail-before-redirect-sync.2026-10-09T14-15.md`.

### Phase 2 — Manifest Declarations

- [x] [P2-T1] In `QuickFiler.Test/packages.config`, insert `  <package id="Microsoft.Web.WebView2" version="1.0.4191.47" targetFramework="net481" />` as a new line between the `Microsoft.TestPlatform.ObjectModel` line (41) and the `Moq` line (42) with the Edit tool. Acceptance: Grep `id="Microsoft\.Web\.WebView2" version="1\.0\.4191\.47" targetFramework="net481"` count 1 at line 42. Evidence: Grep observed count 1 at line 42; recorded in `FEATURE/evidence/other/manifest-edit-facts.<TS>.md`.
- [x] [P2-T2] In `QuickFiler.Test/packages.config`, insert `  <package id="ObjectListView.Official" version="2.9.1" targetFramework="net481" />` between the `MSTest.TestFramework` line and the `OpenTelemetry` line (46 and 47 after P2-T1). Acceptance: Grep `id="ObjectListView\.Official" version="2\.9\.1" targetFramework="net481"` count 1 at line 47; file line count 76. Evidence: Grep observed count 1 at line 47, 76 lines; recorded in `FEATURE/evidence/other/manifest-edit-facts.<TS>.md`.
- [x] [P2-T3] In `UtilitiesCS.Test/packages.config`, insert the WebView2 line of P2-T1 between the `Microsoft.TestPlatform.ObjectModel` line (63) and the `Mono.Reflection` line (64). Acceptance: Grep count 1 at line 64; file line count 111. Evidence: Grep observed count 1 at line 64, 111 lines; recorded in `FEATURE/evidence/other/manifest-edit-facts.<TS>.md`.
- [x] [P2-T4] In `TaskTree.Test/packages.config`, insert the ObjectListView line of P2-T2 between the `MSTest.TestFramework` line (40) and the `OpenTelemetry` line (41). Acceptance: Grep count 1 at line 41; file line count 70. Evidence: Grep observed count 1 at line 41, 70 lines; recorded in `FEATURE/evidence/other/manifest-edit-facts.<TS>.md`.
- [x] [P2-T5] In `QuickFiler.Test/QuickFiler.Test.csproj`, delete the second WebView2 item group: the five lines 528-532 (`  <ItemGroup>`, the `<Reference Include="Microsoft.Web.WebView2.Core, Version=1.0.4191.47, ...">` line, its `<HintPath>` line, `    </Reference>`, `  </ItemGroup>`), leaving the `ProjectReference` group closing at line 527 directly followed by the `Analyzer` item group. Acceptance: Grep count of `Include="Microsoft\.Web\.WebView2\.Core,` is 1 (line 388); file line count 567; Grep count of `Microsoft\.Web\.WebView2\.1\.0\.4191\.47` is 2 (lines 389 and 392). Evidence: observed Include count 1 (line 388), 567 lines, folder count 2 (lines 389, 392); recorded in `FEATURE/evidence/other/manifest-edit-facts.<TS>.md`.
- [x] [P2-T6] Record manifest-edit facts in `FEATURE/evidence/other/manifest-edit-facts.<TS>.md`: for `QuickFiler.Test/packages.config`, `UtilitiesCS.Test/packages.config`, `TaskTree.Test/packages.config` the line counts (76, 111, 70) and carriage-return counts (75, 110, 69, so every inserted line kept CRLF), the inserted lines, and the production sibling declarations they match (`QuickFiler/packages.config` lines 19 and 22, `UtilitiesCS/packages.config` line 57, `TaskTree/packages.config` line 8, each re-read in this task); `git diff --numstat BASE-SHA -- QuickFiler.Test/packages.config UtilitiesCS.Test/packages.config TaskTree.Test/packages.config QuickFiler.Test/QuickFiler.Test.csproj` (expected `2 0`, `1 0`, `1 0`, `0 5`) and `git status --porcelain` over the same four paths. Acceptance: every value equals the expectation. Evidence: `FEATURE/evidence/other/manifest-edit-facts.2026-10-09T14-17.md`.
- [x] [P2-T7] Run `985-pester.ps1 -WorkspaceRoot <WORKTREE> -TestPath tests/scripts/dependencies/RepositoryTreeConsistency.Tests.ps1` from `CMDDIR`; write `FEATURE/evidence/regression-testing/pass-after-orphaned-hintpath.<TS>.md`. Acceptance: `EXIT_CODE: 0`, `Passed=5 Failed=0`, no `FAILED-TEST:` line. Evidence: `FEATURE/evidence/regression-testing/pass-after-orphaned-hintpath.2026-10-09T14-17.md`.
- [x] [P2-T8] Check off AC1 in `FEATURE/spec.md` (change only `- [ ] AC1` to `- [x] AC1`), citing P2-T6 and P2-T7. Acceptance: Grep `^- \[x\] AC1 ` count 1 and `^- \[ \] AC1 ` count 0. Evidence: spec.md line 193 `- [x] AC1`; cites `FEATURE/evidence/other/manifest-edit-facts.2026-10-09T14-17.md` and `FEATURE/evidence/regression-testing/pass-after-orphaned-hintpath.2026-10-09T14-17.md`.
- [x] [P2-T9] Check off AC3 in `FEATURE/spec.md`, citing P1-T1, P1-T3 and P2-T7 (fail-first and passing runs under `FEATURE/evidence/regression-testing/`). Acceptance: Grep `^- \[x\] AC3 ` count 1. Evidence: spec.md `- [x] AC3`; cites `FEATURE/evidence/regression-testing/fail-before-orphaned-hintpath.2026-10-09T14-15.md` and `FEATURE/evidence/regression-testing/pass-after-orphaned-hintpath.2026-10-09T14-17.md`.

### Phase 3 — Redirect Sync Module and Wiring

- [x] [P3-T1] Create `scripts/dependencies/BindingRedirectSync.psm1` per section 6.1 (ASCII only). Acceptance: Grep `^Export-ModuleMember` count 1 and each of the three function names appears once in a `function` line; Grep `Import-Module .*-Force` count 0; Grep `[^\x00-\x7F]` count 0. Evidence: observed Export-ModuleMember 1 (line 303), function lines 75/170/276, `-Force` import 0, non-ASCII 0; recorded in `FEATURE/evidence/other/line-counts-post-impl.<TS>.md`.
- [x] [P3-T2] Wire `scripts/dependencies/Repair-PackageManifestConsistency.ps1` per section 6.2 with the Edit tool (CRLF preserved). Acceptance: Grep counts in that file: `BindingRedirectSync\.psm1` 1, `Invoke-SolutionBindingRedirectSync` 1, `-WhatIf:\$WhatIfPreference` 2, `RedirectSync\s+=` 1, `Format-BindingRedirectSyncReport` 1, `\$projectTextOverride\[` 1; `-CandidateUpgrade` gating absent from the new call (read the inserted block). Evidence: observed counts 1, 1, 2, 1, 1, 1 (lines 73, 449-450, 456, 487, 464, 421); the call at lines 449-450 sits after the loop with no `-CandidateUpgrade` condition; recorded in `FEATURE/evidence/other/line-counts-post-impl.<TS>.md`.
- [x] [P3-T3] Record line counts of `scripts/dependencies/BindingRedirectSync.psm1` and `scripts/dependencies/Repair-PackageManifestConsistency.ps1` (Grep `^`, count) in `FEATURE/evidence/other/line-counts-post-impl.<TS>.md`. Acceptance: both at most 500 and the script above 475; above 500 is STOP: SIZE-LIMIT. Evidence: `FEATURE/evidence/other/line-counts-post-impl.2026-10-09T14-20.md` (307 and 493).
- [x] [P3-T4] Create `tests/scripts/dependencies/BindingRedirectSync.Tests.ps1` per section 6.3 (S1 to S18, in-memory only). Acceptance: Grep count of `^\s+It '` is 18; Grep count of `New-TemporaryFile|GetTempPath|GetTempFileName|TestDrive|WriteAllText|Set-Content|Out-File|New-Item` is 0. Evidence: observed 18 and 0; recorded in `FEATURE/evidence/regression-testing/pass-after-redirect-sync.<TS>.md`.
- [x] [P3-T5] Run `985-pester.ps1 -WorkspaceRoot <WORKTREE> -TestPath tests/scripts/dependencies/BindingRedirectSync.Tests.ps1,tests/scripts/dependencies/Repair-PackageManifestConsistency.RedirectSync.Tests.ps1,tests/scripts/dependencies/Repair-PackageManifestConsistency.Tests.ps1` from `CMDDIR`; write `FEATURE/evidence/regression-testing/pass-after-redirect-sync.<TS>.md`. Acceptance: `TEST-PATH-COUNT: 3`; `EXIT_CODE: 0`; `FILE-TESTS BindingRedirectSync.Tests.ps1 ... Failed=0 Total=18`; `FILE-TESTS Repair-PackageManifestConsistency.RedirectSync.Tests.ps1 ... Failed=0 Total=6`; `FILE-TESTS Repair-PackageManifestConsistency.Tests.ps1 ... Failed=0` with the same `Total=` as that file's `FILE-TESTS` line in P0-T8 (existing contexts unchanged and green). Evidence: `FEATURE/evidence/regression-testing/pass-after-redirect-sync.2026-10-09T14-21.md`.
- [x] [P3-T6] Live-tree check of `scripts/dependencies/Repair-PackageManifestConsistency.ps1` with `-WhatIf`: run `pwsh -NoProfile -File <CMDDIR>\985-repair.ps1 -WorkspaceRoot <WORKTREE> -WhatIfRun`; write `FEATURE/evidence/other/whatif-live-tree.<TS>.md` (`IS-SUCCESS` recorded, not gated). Acceptance: `WRITTEN-COUNT: 0`, `REDIRECTSYNC-REPAIR-COUNT: 0` (main's redirects already satisfy the invariant `BindingRedirectVerification.Tests.ps1` line 275 asserts), `REDIRECTSYNC-UNRESOLVABLE:` empty, and `git status --porcelain` recorded immediately before and immediately after the `985-repair.ps1 -WhatIfRun` call, with the two outputs identical; a non-zero sync count is STOP: LIVE-TREE-SYNC-NONZERO with the `REDIRECTSYNC:` lines reported. Evidence: `FEATURE/evidence/other/whatif-live-tree.2026-10-09T14-21.md`.

### Phase 4 — PowerShell QA Loop

- [x] [P4-T1] Format `scripts/dependencies` and `tests/scripts/dependencies`: record `git status --porcelain` and `git hash-object --no-filters` of the five PowerShell Write Set files, call `mcp__drm-copilot__run_poshqc_format` (`workspace_root = WORKSPACE-ROOT`, `scan_folders = [scripts/dependencies, tests/scripts/dependencies]`), repeat both observations; write `FEATURE/evidence/qa-gates/ps-format.<TS>.md` with `ITERATION:`. Acceptance (success-case observation): porcelain identical and all five hashes identical before and after; any rewrite is a loop restart (C7). Evidence: iteration 2 (passing) `FEATURE/evidence/qa-gates/ps-format.2026-10-09T14-26.md`; iteration 1 `FEATURE/evidence/qa-gates/ps-format.2026-10-09T14-25.md`.
- [x] [P4-T2] Lint `scripts/dependencies` and `tests/scripts/dependencies` with `mcp__drm-copilot__run_poshqc_analyze` (same arguments); write `FEATURE/evidence/qa-gates/ps-analyze.<TS>.md` with `ITERATION:` and the type-check not-applicable statement. Acceptance: `ok` true. Evidence: iteration 2 (passing) `FEATURE/evidence/qa-gates/ps-analyze.2026-10-09T14-26.md`; iteration 1 (7 findings, fixed) `FEATURE/evidence/qa-gates/ps-analyze.2026-10-09T14-25.md`.
- [x] [P4-T3] MCP test over `tests/scripts/dependencies`: `985-junit.ps1 -Clear`, `mcp__drm-copilot__run_poshqc_test` (`scan_folders = [tests/scripts/dependencies]`), `985-junit.ps1`; write `FEATURE/evidence/qa-gates/ps-test-mcp.<TS>.md` with `ITERATION:`. Acceptance: `JUNIT-CLEARED: True`; failures 0 and errors 0; `JUNIT-SUITE` lines include `BindingRedirectSync.Tests.ps1` and `Repair-PackageManifestConsistency.RedirectSync.Tests.ps1` with non-zero `tests=`. Evidence: `FEATURE/evidence/qa-gates/ps-test-mcp.2026-10-09T14-26.md` (iteration 2).
- [x] [P4-T4] Direct coverage run (CI mirror) of `scripts/dependencies`, `scripts/hygiene` and `scripts/vscode`: `985-pester.ps1` with the P0-T8 arguments and `-CoverageOutput coverage/985-pester-final.xml`; write `FEATURE/evidence/qa-gates/ps-coverage.<TS>.md` with `ITERATION:`. Acceptance: `TEST-PATH-COUNT: 3` and `COVERAGE-PATH-COUNT: 3`; `EXIT_CODE: 0`, `Failed=0`; `COVERAGE LinePercent` at or above 80.00; the `FILE-COVERAGE` line ending `/BindingRedirectSync.psm1` at or above 90.00; the line ending `/Repair-PackageManifestConsistency.ps1` at or above `BASELINE-REPAIR-SCRIPT-LINE`; `POLICY-85-OBSERVATION: MET|NOT MET` recorded for the aggregate (D6). Evidence: `FEATURE/evidence/qa-gates/ps-coverage.2026-10-09T14-26.md` (iteration 2; aggregate 94.98, module 100.00, repair script 94.14).
- [x] [P4-T5] Temporary-file audit of `tests/scripts/dependencies/BindingRedirectSync.Tests.ps1`, `tests/scripts/dependencies/Repair-PackageManifestConsistency.RedirectSync.Tests.ps1` and `tests/scripts/dependencies/RepositoryTreeConsistency.Tests.ps1`: Grep `New-TemporaryFile|GetTempPath|GetTempFileName|TestDrive|WriteAllText|Set-Content|Out-File|New-Item` (count) over the three files; positive control, the same pattern over `scripts/dependencies/Repair-PackageManifestConsistency.ps1` (its default writer at line 371 uses `WriteAllText`); write `FEATURE/evidence/qa-gates/ps-temp-file-audit.<TS>.md` with `ITERATION:`. Acceptance: 0 for each test file and at least 1 for the control (0 is STOP: SANITIZE-BLIND, the search tool is not reading the files). Evidence: `FEATURE/evidence/qa-gates/ps-temp-file-audit.2026-10-09T14-26.md` (iteration 2).
- [x] [P4-T6] Final size check of the five PowerShell Write Set files (`scripts/dependencies/BindingRedirectSync.psm1`, `scripts/dependencies/Repair-PackageManifestConsistency.ps1` and the three test files) with Grep `^` counts; write `FEATURE/evidence/qa-gates/ps-line-counts.<TS>.md`. Acceptance: every count at most 500; otherwise STOP: SIZE-LIMIT. Evidence: `FEATURE/evidence/qa-gates/ps-line-counts.2026-10-09T14-26.md`.
- [x] [P4-T7] Write `FEATURE/evidence/qa-gates/coverage-comparison-powershell.<TS>.md` from P0-T8 and the final-iteration P4-T4: baseline aggregate, final aggregate, delta; `BindingRedirectSync.psm1` (new code) final percent; `Repair-PackageManifestConsistency.ps1` baseline and final percent (changed-file no-regression). Acceptance: all five numbers present; gates as P4-T4; PowerShell branch coverage recorded as "not measured by Pester; no branch gate applies" (`.claude/rules/powershell.md`). Evidence: `FEATURE/evidence/qa-gates/coverage-comparison-powershell.2026-10-09T14-26.md`.
- [x] [P4-T8] Check off AC2 in `FEATURE/spec.md`, citing P3-T5 (behaviours S3, S5, S6, S4, S8, S15/R5 and R1-R4), P3-T2, P4-T6 and P4-T4. Acceptance: Grep `^- \[x\] AC2 ` count 1. Evidence: spec.md `- [x] AC2`; cites `FEATURE/evidence/regression-testing/pass-after-redirect-sync.2026-10-09T14-21.md`, `FEATURE/evidence/other/line-counts-post-impl.2026-10-09T14-20.md`, `FEATURE/evidence/qa-gates/ps-line-counts.2026-10-09T14-26.md`, `FEATURE/evidence/qa-gates/ps-coverage.2026-10-09T14-26.md`.

### Phase 5 — C# QA Loop

- [x] [P5-T1] C# format over the whole tree including `QuickFiler.Test/QuickFiler.Test.csproj`'s project: record `git status --porcelain`; run `985-csharpier.ps1 -WorkspaceRoot <WORKTREE> -Mode format` from `CMDDIR`; record porcelain again; run `-Mode check`; write `FEATURE/evidence/qa-gates/cs-format.<TS>.md` with `ITERATION:`. Acceptance (success-case observation): porcelain identical before and after `format`, and `check` `EXIT_CODE: 0`; a rewrite is a loop restart (C7). Evidence: `FEATURE/evidence/qa-gates/cs-format.2026-10-09T14-30.md`.
- [x] [P5-T2] Restore `TaskMaster.sln` (now resolving the declarations in `QuickFiler.Test/packages.config`, `UtilitiesCS.Test/packages.config`, `TaskTree.Test/packages.config`) with `985-restore.ps1`; write `FEATURE/evidence/qa-gates/cs-restore.<TS>.md` with `ITERATION:`. Acceptance: `EXIT_CODE: 0`. Evidence: `FEATURE/evidence/qa-gates/cs-restore.2026-10-09T14-30.md`.
- [x] [P5-T3] Analyzer gate: `985-msbuild.ps1 -WorkspaceRoot <WORKTREE> -Gate Analyzers -LogName 985-final-analyzers.log`; write `FEATURE/evidence/qa-gates/cs-analyzers.<TS>.md` with `ITERATION:`. Acceptance: `EXIT_CODE: 0`, `SUMMARY: 0 Error(s)`, `OUTPUT-ASSEMBLIES:` equal to `BASELINE-OUTPUT-ASSEMBLIES`. Evidence: `FEATURE/evidence/qa-gates/cs-analyzers.2026-10-09T14-30.md`.
- [x] [P5-T4] Type-check gate: `985-msbuild.ps1 -WorkspaceRoot <WORKTREE> -Gate Nullable -LogName 985-final-nullable.log`; write `FEATURE/evidence/qa-gates/cs-nullable.<TS>.md` with `ITERATION:`. Acceptance: `EXIT_CODE: 0`, `SUMMARY: 0 Error(s)`, `OUTPUT-ASSEMBLIES:` equal to the baseline. Evidence: `FEATURE/evidence/qa-gates/cs-nullable.2026-10-09T14-30.md`.
- [x] [P5-T5] Test gate with coverage: `985-mstest.ps1 -WorkspaceRoot <WORKTREE> -EvidencePrefix <WORKTREE>\FEATURE\evidence\qa-gates\mstest-coverage-final.<TS>`; write `FEATURE/evidence/qa-gates/cs-test.<TS>.md` with `ITERATION:` and the same lines as P0-T13. Acceptance: `RUNNER_RESULT=COMPLETED` (runner floors line 80 and branch 75 enforced inside the runner), `TRX-SUMMARY: Failed tests: none`, both copies present, `FINAL-CS-LINE:` and `FINAL-CS-BRANCH:` recorded. Evidence: `FEATURE/evidence/qa-gates/cs-test.2026-10-09T14-31.md`.
- [x] [P5-T6] Write `FEATURE/evidence/qa-gates/coverage-comparison-csharp.<TS>.md`: baseline and final line and branch figures and their deltas, and the changed C# code population from `git diff --numstat BASE-SHA -- *.cs` plus `git status --porcelain -- *.cs` (both empty, so new or changed C# code coverage is not applicable with this proof). Acceptance: four numbers and both empty outputs recorded; a non-empty output is a scope violation reported to the caller. Evidence: `FEATURE/evidence/qa-gates/coverage-comparison-csharp.2026-10-09T14-31.md`.
- [x] [P5-T7] Verify AC4 for `.github/workflows`: run `git diff --name-only BASE-SHA -- .github/workflows` and `git status --porcelain -- .github/workflows`; write `FEATURE/evidence/qa-gates/workflows-untouched.<TS>.md`; then check off AC4 in `FEATURE/spec.md`. Acceptance: both outputs empty; Grep `^- \[x\] AC4 ` count 1. (P8-T2 repeats the committed-range form after the last commit.) Evidence: `FEATURE/evidence/qa-gates/workflows-untouched.2026-10-09T14-31.md`; spec.md `- [x] AC4`.
- [x] [P5-T8] Check off AC5 in `FEATURE/spec.md`, citing P4-T5 (in-memory only), P4-T4 (new module at or above 90), the single passing iteration of P4-T1 to P4-T5 and of P5-T1 to P5-T5. Acceptance: Grep `^- \[x\] AC5 ` count 1. Evidence: spec.md `- [x] AC5`; cites `FEATURE/evidence/qa-gates/ps-temp-file-audit.2026-10-09T14-26.md`, `FEATURE/evidence/qa-gates/coverage-comparison-powershell.2026-10-09T14-26.md`, PowerShell iteration 2 (P4-T1 to P4-T5 all passing, `*.2026-10-09T14-26.md`) and C# iteration 1 (P5-T1 to P5-T5 all passing, `*.2026-10-09T14-30.md` / `*.2026-10-09T14-31.md`).

### Phase 6 — Commit the Fix

- [x] [P6-T1] Run C4 over `FEATURE`, then CMD-COMMIT with paths `QuickFiler.Test/packages.config`, `UtilitiesCS.Test/packages.config`, `TaskTree.Test/packages.config`, `QuickFiler.Test/QuickFiler.Test.csproj`, `scripts/dependencies/BindingRedirectSync.psm1`, `scripts/dependencies/Repair-PackageManifestConsistency.ps1`, the three test files of section 4, `FEATURE`, and every `.claude/agent-memory/` path from a `git status --porcelain` taken just before staging (recorded as `AGENT-MEMORY-LINES:`, C10) (this task's checkbox flipped first), subject `fix(deps): declare borrowed test packages and sync transitive binding redirects (#985)`; then run `git rev-parse HEAD` (`FIX-SHA:`), `git diff --name-only HEAD~1 HEAD` and `git status --porcelain`, in that order and before writing any file, and record the three outputs in `FEATURE/evidence/other/commit-fix.<TS>.md`. Acceptance: the committed name list contains the nine Write Set manifest, code and test paths named in this task and only `FEATURE` and `.claude/agent-memory/` paths besides; excluding `.claude/agent-memory/` lines (C10), every porcelain line is in `PreExistingWorktreePaths` (the feature-folder entry excepted, now committed). The commit-fix artifact itself is written after the commit and is committed by P8-T2. Evidence: `FEATURE/evidence/other/commit-fix.2026-10-09T14-34.md`.

### Phase 7 — Integration Rehearsal (AC6, verification evidence only)

Rule R-FAIL: when any acceptance below fails, append a section `REHEARSAL-VERDICT: FAIL` to the rehearsal file naming the task, the observed values, the sanitized error lines and a classification (`REHEARSAL-ENVIRONMENT-PATH-LENGTH` when `PATH-LENGTH-SIGNATURE-LINES` is above 0, otherwise `REHEARSAL-DEFECT`), leave `REHEARSAL` and `REHEARSAL-BRANCH` in place for diagnosis, and STOP: REHEARSAL-FAILED. The only file this phase adds to the branch is the rehearsal file; every task appends a section to it with its own `Command:`, `EXIT_CODE:` and output lines.

- [ ] [P7-T1] Run `git fetch origin dependabot/nuget/QuickFiler.Test/all-nuget-updates-2542c001c9` and `git rev-parse DEPENDABOT-REF`; create `FEATURE/evidence/other/integration-rehearsal.<TS>.md` with `Timestamp:`, `Command:` (the fetch), `EXIT_CODE:`, `Output Summary:` (`DEPENDABOT-SHA:`), and `ExpectedExitCode` absent; record the created path as REHEARSAL-FILE (every later Phase 7 append targets REHEARSAL-FILE). Acceptance: fetch exit 0 and a 40-character SHA recorded.
- [ ] [P7-T2] Run `git worktree add -b rehearsal-985-throwaway <REHEARSAL> origin/dependabot/nuget/QuickFiler.Test/all-nuget-updates-2542c001c9` (path written as `REHEARSAL-ROOT` in the evidence); append `git -C <REHEARSAL> rev-parse HEAD`. Acceptance: exit 0 and `HEAD` equal to `DEPENDABOT-SHA`.
- [ ] [P7-T3] Merge the fix into the rehearsal branch per D2: `git -C <REHEARSAL> merge -X ours --no-edit bug/dependabot-repair-borrowed-packages-and-transitive-redirects-985`; append its output (sanitized), `git -C <REHEARSAL> rev-parse HEAD` as `MERGE-SHA:` and `git -C <REHEARSAL> status --porcelain`. Acceptance: exit 0, porcelain empty, `git -C <REHEARSAL> merge-base --is-ancestor FIX-SHA HEAD` exits 0.
- [ ] [P7-T4] Verify the fix content in `REHEARSAL` and re-apply what D2 dropped: (a) for each of `scripts/dependencies/BindingRedirectSync.psm1`, `scripts/dependencies/Repair-PackageManifestConsistency.ps1`, `tests/scripts/dependencies/BindingRedirectSync.Tests.ps1`, `tests/scripts/dependencies/Repair-PackageManifestConsistency.RedirectSync.Tests.ps1`, `tests/scripts/dependencies/RepositoryTreeConsistency.Tests.ps1`, `git -C <REHEARSAL> rev-parse HEAD:<path>` equals `git rev-parse FIX-SHA:<path>`; (b) Grep in `REHEARSAL` for the four declarations (`id="Microsoft\.Web\.WebView2"` in `QuickFiler.Test/packages.config` and `UtilitiesCS.Test/packages.config`, `id="ObjectListView\.Official"` in `QuickFiler.Test/packages.config` and `TaskTree.Test/packages.config`) and for `Include="Microsoft\.Web\.WebView2\.Core,` in `QuickFiler.Test/QuickFiler.Test.csproj`; insert any missing declaration (the fix's version, immediately before the first package line whose id compares greater, ordinal case-insensitive) and delete a surviving duplicate WebView2.Core item group with the Edit tool; append `REAPPLIED:` lines (or `REAPPLIED: none`). Acceptance: all five blob equalities hold; afterwards each declaration count is 1 and the WebView2.Core `Include` count is 1.
- [ ] [P7-T5] Restore `REHEARSAL\TaskMaster.sln` into `REHEARSAL\packages` (pre-simulation; `nuget update` needs the installed versions): `985-restore.ps1 -WorkspaceRoot <REHEARSAL>`; append. Acceptance: `EXIT_CODE: 0`.
- [ ] [P7-T6] Diagnostic, read-only repair run of `REHEARSAL\scripts\dependencies\Repair-PackageManifestConsistency.ps1` on the literal merge state: `985-repair.ps1 -WorkspaceRoot <REHEARSAL> -WhatIfRun`; append every output line under `DIAGNOSTIC (literal merge, -WhatIf)`. Acceptance: the script completes and prints `WRITTEN-COUNT: 0` and a `REDIRECTSYNC-REPAIR-COUNT:` line (values recorded, not gated; research section 4.2 predicts the five transitive `log4net` redirects); a thrown error is R-FAIL.
- [ ] [P7-T7] Decide the D1 simulation rows from `REHEARSAL` manifests: read the `Microsoft.Web.WebView2` version in `QuickFiler/packages.config` and `UtilitiesCS/packages.config` and the `ObjectListView.Official` version in `QuickFiler/packages.config` and `TaskTree/packages.config`; append `SIM-ROW:` lines `<test project> <package id> <declared version> <production version> <UPDATE|NONE>` for the four pairs (`UPDATE` when they differ). Acceptance: four `SIM-ROW` lines; the two production WebView2 values agree with each other and the two ObjectListView values agree with each other (otherwise R-FAIL: the branch content is not the expected PR #984 state).
- [ ] [P7-T8] Simulated Dependabot update (D1) of each `UPDATE` row in `REHEARSAL` (for example `QuickFiler.Test/packages.config`): `985-nuget-update.ps1 -WorkspaceRoot <REHEARSAL> -ProjectDirectory <test project> -PackageId <id> -PackageVersion <production version>`; append each output and then `git -C <REHEARSAL> diff --stat` and `git -C <REHEARSAL> status --porcelain`. Acceptance: each update `EXIT_CODE: 0`; afterwards each updated manifest declares the production version (Grep) and Grep for the old version folder (for example `Microsoft\.Web\.WebView2\.1\.0\.4191\.47`) in the updated test csproj returns 0; `SIM: none required` is recorded when no row says `UPDATE`.
- [ ] [P7-T9] Workflow step "Restore solution" (`.github/workflows/dependabot-repair.yml` line 74) over `REHEARSAL\TaskMaster.sln`: `985-restore.ps1 -WorkspaceRoot <REHEARSAL>`; append. Acceptance: `EXIT_CODE: 0`.
- [ ] [P7-T10] Workflow step "Repair package manifest consistency" (`dependabot-repair.yml` line 80: the branch's script, no arguments): `985-repair.ps1 -WorkspaceRoot <REHEARSAL>` (runs `REHEARSAL\scripts\dependencies\Repair-PackageManifestConsistency.ps1`); append all lines. Acceptance: `EXIT_CODE: 0` and `IS-SUCCESS: True` (the workflow's own failure condition, line 81); `WRITTEN-COUNT`, `REDIRECTSYNC:` lines, `BEYOND-KNOWN-WEAK` and the body recorded.
- [ ] [P7-T11] Idempotence (the post-push re-fire) of `REHEARSAL\scripts\dependencies\Repair-PackageManifestConsistency.ps1`: run `985-repair.ps1 -WorkspaceRoot <REHEARSAL>` again; append. Acceptance: `EXIT_CODE: 0`, `IS-SUCCESS: True`, `WRITTEN-COUNT: 0`, `REDIRECTSYNC-REPAIR-COUNT: 0`.
- [ ] [P7-T12] Restore `REHEARSAL\TaskMaster.sln` again after the repair's writes (as CI does on the pushed head): `985-restore.ps1 -WorkspaceRoot <REHEARSAL>`; append. Acceptance: `EXIT_CODE: 0`.
- [ ] [P7-T13] Analyzer build of `REHEARSAL\TaskMaster.sln`: `985-msbuild.ps1 -WorkspaceRoot <REHEARSAL> -Gate Analyzers -LogName 985-rehearsal-analyzers.log`; append the projection lines (`EXIT_CODE`, `OUTPUT-ASSEMBLIES`, `SUMMARY`, `ERROR-LINE-COUNT`, `ERROR`, `PATH-LENGTH-SIGNATURE-LINES`). Acceptance: `EXIT_CODE: 0`, `SUMMARY: 0 Error(s)`, `ERROR-LINE-COUNT: 0`.
- [ ] [P7-T14] Nullable build of `REHEARSAL\TaskMaster.sln`: `985-msbuild.ps1 -WorkspaceRoot <REHEARSAL> -Gate Nullable -LogName 985-rehearsal-nullable.log`; append the same projection lines. Acceptance: `EXIT_CODE: 0`, `SUMMARY: 0 Error(s)`, `ERROR-LINE-COUNT: 0`.
- [ ] [P7-T15] Pester in `REHEARSAL`: `985-pester.ps1 -WorkspaceRoot <REHEARSAL> -TestPath tests/scripts/dependencies/BindingRedirectVerification.Tests.ps1,tests/scripts/dependencies/RepositoryTreeConsistency.Tests.ps1`; append. Acceptance: `TEST-PATH-COUNT: 2`; `EXIT_CODE: 0`, `Failed=0`, both `FILE-TESTS` lines present with `Total=` above 0.
- [ ] [P7-T16] Append `REHEARSAL-VERDICT: PASS` with a one-line summary per gate (P7-T10, P7-T11, P7-T13, P7-T14, P7-T15) to REHEARSAL-FILE (under `FEATURE/evidence/other/`), then run C4 over `FEATURE`. Acceptance: verdict line present; C4 count 0 and control 1.
- [ ] [P7-T17] Remove the rehearsal: `git worktree remove --force <REHEARSAL>`, `git branch -D rehearsal-985-throwaway`; then `git worktree list --porcelain` (no `rehearsal-985` entry), `git branch --list rehearsal-985-throwaway` (empty) and `git ls-remote --heads origin rehearsal-985-throwaway` (empty: never pushed); append these as `CLEANUP:` lines to REHEARSAL-FILE (under `FEATURE/evidence/other/`). Acceptance: all three observations as stated.
- [ ] [P7-T18] Check off AC6 in `FEATURE/spec.md`, citing REHEARSAL-FILE (P7-T10, P7-T13, P7-T14, P7-T15, P7-T16) and D1. Acceptance: Grep `^- \[x\] AC6 ` count 1.

### Phase 8 — Closeout

- [ ] [P8-T1] Write `FEATURE/evidence/other/ac-status.<TS>.md`: the acceptance-criteria-tracking summary (`Source: FEATURE/spec.md`, total 7, checked 6, remaining 1, items remaining: AC7), `AC7: PENDING-CI (checked off by the item's orchestrator after merge and @dependabot recreate on PR #984; D7)`, and the nine helper-script hashes re-read with `git hash-object --no-filters` compared with P0-T3. Acceptance: Grep `^- \[x\] AC[1-6] ` count 6 and `^- \[ \] AC7 ` count 1 in `FEATURE/spec.md`; hashes equal.
- [ ] [P8-T2] Run C4 over `FEATURE`, then CMD-COMMIT with path `FEATURE` plus every `.claude/agent-memory/` path from a `git status --porcelain` taken just before staging (recorded as `AGENT-MEMORY-LINES:`, C10) (this task's checkbox flipped first), subject `docs(985): record integration rehearsal and acceptance status`; then run `git diff --name-only HEAD~1 HEAD`, `git diff --name-only BASE-SHA HEAD -- .github/workflows` and `git status --porcelain`; write nothing further. Acceptance: the commit succeeds; `git diff --name-only HEAD~1 HEAD` lists only `FEATURE` and `.claude/agent-memory/` paths; the workflows diff is empty; excluding `.claude/agent-memory/` lines (C10, expected none after the commit), every porcelain line is in `PreExistingWorktreePaths`.

## 8. Acceptance Criteria Traceability

| AC | Implementation | Tests | Evidence | Check-off |
|---|---|---|---|---|
| AC1 | P2-T1 to P2-T5 | P1-T1 gate `It`, P2-T7 | `FEATURE/evidence/other/manifest-edit-facts.<TS>.md` | P2-T8 |
| AC2 | P3-T1, P3-T2 | P1-T2, P3-T4, P3-T5 | `FEATURE/evidence/regression-testing/pass-after-redirect-sync.<TS>.md`, `FEATURE/evidence/qa-gates/ps-line-counts.<TS>.md` | P4-T8 |
| AC3 | P1-T1 | P1-T3, P2-T7 | `FEATURE/evidence/regression-testing/fail-before-orphaned-hintpath.<TS>.md`, `FEATURE/evidence/regression-testing/pass-after-orphaned-hintpath.<TS>.md` | P2-T9 |
| AC4 | scope rule (section 1) | P5-T7, P8-T2 | `FEATURE/evidence/qa-gates/workflows-untouched.<TS>.md` | P5-T7 |
| AC5 | P1-T2, P3-T4 | P4-T1 to P4-T5, P5-T1 to P5-T5 | `FEATURE/evidence/qa-gates/ps-temp-file-audit.<TS>.md`, `FEATURE/evidence/qa-gates/coverage-comparison-powershell.<TS>.md` | P5-T8 |
| AC6 | Phase 7 | P7-T10 to P7-T15 | REHEARSAL-FILE (created by P7-T1 under `FEATURE/evidence/other/`) | P7-T18 |
| AC7 | whole change | CI on PR #984 after recreate | orchestrator post-merge record | not in this plan (D7) |

## 9. Self-Review (this pass)

SELF-REVIEW: RE-DERIVED THIS PASS
- `QuickFiler.Test/packages.config`: 74 lines, 73 CR; line 41 `Microsoft.TestPlatform.ObjectModel`, 42 `Moq`, 45 `MSTest.TestFramework`, 46 `OpenTelemetry`; no WebView2 or ObjectListView entry.
- `UtilitiesCS.Test/packages.config`: 110 lines, 109 CR; line 63 `Microsoft.TestPlatform.ObjectModel`, 64 `Mono.Reflection`, 70 `ObjectListView.Official` 2.9.1 (declared, not borrowed).
- `TaskTree.Test/packages.config`: 69 lines, 68 CR; line 40 `MSTest.TestFramework`, 41 `OpenTelemetry`.
- `QuickFiler.Test/QuickFiler.Test.csproj`: 572 lines; WebView2.Core `Include` at 388 and 529; HintPaths 389, 392, 530 (WebView2) and 407 (ObjectListView); item group 528-532; line 527 closes the `ProjectReference` group; 533 opens the `Analyzer` group.
- `UtilitiesCS.Test/UtilitiesCS.Test.csproj`: borrowed WebView2 HintPaths at 815 and 818; ObjectListView HintPath 827. `TaskTree.Test/TaskTree.Test.csproj`: ObjectListView HintPath 178.
- Production siblings: `QuickFiler/packages.config` 19 (WebView2 1.0.4191.47) and 22 (ObjectListView 2.9.1); `UtilitiesCS/packages.config` 57; `TaskTree/packages.config` 8.
- `scripts/dependencies/Repair-PackageManifestConsistency.ps1`: 475 lines; imports 67-71; `$written` 382; project write 413-417; app.config pass 424-440, gated at 425 on applied upgrades; loop ends 441; normalisation comment 443-444, call 445-447; default writer `WriteAllText` 371; result object 454-475.
- `scripts/dependencies/ConsistencyVerifier.psm1`: `Get-DetectionResult` 41-57 (`ExaminedCount`, `Finding`); `Find-OrphanedHintPath` 122-154.
- `scripts/dependencies/BindingRedirectVerification.psm1`: `ConvertTo-ReferenceVersionMap` 31-72; `Find-StaleBindingRedirect` 74-134 (ordinal `-contains` at 118); import rationale 26-29.
- `scripts/dependencies/ProjectConsistency.psm1`: `Invoke-BindingRedirectReconciliation` 271-375 (range upper bound and single-version rules 346-358; locals 323-326); export 377-381.
- `scripts/dependencies/PackageGraph.psm1`: `Get-PackageManifestPath` 87-128 (exclusions 113); `ConvertFrom-AppConfigText` 285-335 (throws at 305); `$script:NewLine` CRLF at 32; `Invoke-ManifestNormalization` 395-455.
- `tests/scripts/dependencies/RepositoryTreeConsistency.Tests.ps1`: 152 lines; module import line 5; Import gate `It` 65-90 (pair discovery 67-73).
- `tests/scripts/dependencies/Repair-PackageManifestConsistency.Tests.ps1`: 429 lines; CRLF normalisation 59-61; `Get-RepairFixture` 86-120; agreeing tree 232-266; `-WhatIf` 268-279; SVGControl scoped `-WhatIf` 356-381.
- `tests/scripts/dependencies/BindingRedirectVerification.Tests.ps1`: repository invariant `It` at 275 with empty debt (293) and `netstandard` unverifiable (294); range `It` at 320.
- `.github/workflows/dependabot-repair.yml`: restore 72-74; repair invocation 80; failure check 81-84; comment 87-94; `beyond-known-weak` 95; staging 136.
- `.github/workflows/_pester.yml`: configuration 40-47; floor 71.
- `scripts/vscode/Invoke-MSTestWithCoverage.ps1`: argument list 82-94; `Invoke-MSTestWithCoverageMain` parameters 293-301; entry guard 459. `scripts/vscode/Invoke-VSBuild.ps1`: package-reference sync 247-253.
- `scripts/hygiene/Test-RepositoryHygiene.Rules.ps1`: profile pattern 21; raw-document classifier 57-123. The worktree `.git` pointer file matches the profile pattern once (C4 control).
- Round 1 (preflight deltas): `UtilitiesCS.Test/packages.config` line 70 declares `ObjectListView.Official` 2.9.1 (P0-T4 now expects 1 there); `scripts/vscode/Install-RepoDotNetSdk.ps1` exists; `.gitignore` line 356 `.dotnet*/` ignores `.dotnet-sdk/`; `scripts/vscode/Invoke-MSTestWithCoverage.ps1` line 262 throws on a non-zero collector exit before the summary is written (lines 430-454), so C7 now reads the trx failed set; `985-pester.ps1` has no remaining direct use of `$TestPath` or `$CoveragePath` after the split lists; no remaining reference to the fixed rehearsal file name outside P7-T1.
- Sibling re-check: inserting one line in each manifest shifts the following anchors by one (P2-T2 uses 46/47 after P2-T1); deleting 528-532 leaves 388/389/392 unchanged and moves the `Analyzer` group up by five lines; the existing Import `It` and the three other `It` blocks of `RepositoryTreeConsistency.Tests.ps1` are unchanged, so its total becomes 5 (P1-T3, P2-T7).

## 10. Planner Internal Review Record

PLANNER-INTERNAL-REVIEW: PASS
CITATION-TO-TREE: PASS
AC-TRACEABILITY: PASS
SCOPE-BOUNDARY: PASS
CITATION: QuickFiler.Test/packages.config | lines 41-42 and 45-46 insertion anchors
CITATION: UtilitiesCS.Test/packages.config | lines 63-64 insertion anchor; line 70 ObjectListView declared
CITATION: TaskTree.Test/packages.config | lines 40-41 insertion anchor
CITATION: QuickFiler.Test/QuickFiler.Test.csproj | lines 388-392 references; lines 528-532 duplicate item group
CITATION: QuickFiler/packages.config | lines 19 and 22 production versions
CITATION: UtilitiesCS/packages.config | line 57 production WebView2 version
CITATION: TaskTree/packages.config | line 8 production ObjectListView version
CITATION: scripts/dependencies/Repair-PackageManifestConsistency.ps1 | lines 67-71, 382, 413-417, 424-447, 454-475
CITATION: scripts/dependencies/ConsistencyVerifier.psm1 | Find-OrphanedHintPath lines 122-154
CITATION: scripts/dependencies/BindingRedirectVerification.psm1 | ConvertTo-ReferenceVersionMap lines 31-72
CITATION: scripts/dependencies/ProjectConsistency.psm1 | Invoke-BindingRedirectReconciliation lines 271-375
CITATION: scripts/dependencies/PackageGraph.psm1 | Get-PackageManifestPath lines 87-128; Invoke-ManifestNormalization lines 395-455
CITATION: tests/scripts/dependencies/RepositoryTreeConsistency.Tests.ps1 | Import gate It lines 65-90
CITATION: tests/scripts/dependencies/Repair-PackageManifestConsistency.Tests.ps1 | Get-RepairFixture lines 86-120
CITATION: tests/scripts/dependencies/BindingRedirectVerification.Tests.ps1 | repository invariant It line 275
CITATION: .github/workflows/dependabot-repair.yml | lines 72-95 restore, invocation and beyond-known-weak
CITATION: .github/workflows/_pester.yml | lines 40-47 and 71
CITATION: scripts/vscode/Invoke-MSTestWithCoverage.ps1 | lines 82-94 and 293-301
CITATION: scripts/vscode/Invoke-VSBuild.ps1 | lines 247-253 package-reference sync
CITATION: scripts/hygiene/Test-RepositoryHygiene.Rules.ps1 | lines 5-55 and 57-123
AC-INVENTORY: AC1, AC2, AC3, AC4, AC5, AC6, AC7
AC-MAPPING: AC1 | IMPLEMENTATION: P2-T1 to P2-T5 | TESTS: P1-T1 gate It and P2-T7 | EVIDENCE: FEATURE/evidence/other/manifest-edit-facts.TS.md
AC-MAPPING: AC2 | IMPLEMENTATION: P3-T1 and P3-T2 | TESTS: P1-T2, P3-T4, P3-T5 | EVIDENCE: FEATURE/evidence/regression-testing/pass-after-redirect-sync.TS.md
AC-MAPPING: AC3 | IMPLEMENTATION: P1-T1 | TESTS: P1-T3 and P2-T7 | EVIDENCE: FEATURE/evidence/regression-testing/fail-before-orphaned-hintpath.TS.md
AC-MAPPING: AC4 | IMPLEMENTATION: scope rule in section 1 | TESTS: P5-T7 and P8-T2 | EVIDENCE: FEATURE/evidence/qa-gates/workflows-untouched.TS.md
AC-MAPPING: AC5 | IMPLEMENTATION: P1-T2 and P3-T4 | TESTS: P4-T1 to P4-T5 and P5-T1 to P5-T5 | EVIDENCE: FEATURE/evidence/qa-gates/coverage-comparison-powershell.TS.md
AC-MAPPING: AC6 | IMPLEMENTATION: Phase 7 | TESTS: P7-T10 to P7-T15 | EVIDENCE: REHEARSAL-FILE created by P7-T1 under FEATURE/evidence/other/
AC-MAPPING: AC7 | IMPLEMENTATION: whole change | TESTS: required CI checks on PR 984 after recreate | EVIDENCE: orchestrator post-merge CI record
UNRESOLVED-GAPS: NONE
