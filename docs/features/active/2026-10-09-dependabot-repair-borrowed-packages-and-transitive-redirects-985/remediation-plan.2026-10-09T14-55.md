# 2026-10-09-dependabot-repair-borrowed-packages-and-transitive-redirects (Remediation Plan R1)

- **Issue:** #985
- **Cycle:** remediation cycle 1 (R1), from review `2026-10-09T14-55`
- **Owner:** drmoisan
- **Last Updated:** 2026-10-09
- **Status:** Complete (executed in commit 07d3a8a98; re-audit 2026-10-09T15-35 PASS)
- **Work Mode:** full-bug (AC source: `spec.md`; this cycle changes no AC text and checks off no AC)
- **Inputs:** `FEATURE/remediation-inputs.2026-10-09T14-55.md` (B-1), `FEATURE/code-review.2026-10-09T14-55.md` (Minor rows 2 and 4, Info row 1)

**Fail-closed evidence rule:** every artifact named below is required. A missing artifact, or a placeholder where a number is required, makes the outcome INCOMPLETE, never PASS.

## 1. Objective

Close the four remediation items of review `2026-10-09T14-55` on this item's own files:

- **B-1 (Blocking).** Remove the operator account name from `FEATURE/plan.2026-10-09T13-06.md` line 26 (the encoded session-scratchpad segment, two occurrences), then sweep every line the branch adds against the merge base (tracked added lines plus every untracked file, including `.claude/agent-memory/`) for the account name, the host name, the user e-mail and drive-rooted user-profile paths, and fix every further hit.
- **CR-1.** `scripts/dependencies/BindingRedirectSync.psm1`: every sync repair record carries a `Direction` (`Upgrade`, `Downgrade` or `Unknown`) and the report line states it, so a downward rewrite is visible in the pull-request body.
- **CR-2.** `scripts/dependencies/Repair-PackageManifestConsistency.ps1`: `WrittenPath` lists each path once (ordinal, case-insensitive), first occurrence order kept.
- **CR-3.** `scripts/dependencies/BindingRedirectSync.psm1`: the already-handled assembly-name check compares names case-insensitively (assembly names bind case-insensitively in .NET), pinned by a test.

Out of scope (do not edit): every file under `.github/workflows/**` (AC4), `.claude/rules/**`, `.github/instructions/**`, every C# source, project, `packages.config` or `app.config` file, every AC line in `FEATURE/spec.md`.

## 2. Conventions

**C1. Symbols.**
- `FEATURE` = `docs/features/active/2026-10-09-dependabot-repair-borrowed-packages-and-transitive-redirects-985`.
- `WORKTREE` = the executor's worktree root (checkout of branch `bug/dependabot-repair-borrowed-packages-and-transitive-redirects-985`). Every repository path is relative to `WORKTREE`.
- `SCRATCH` = the session scratchpad directory named in the executing agent's own environment block. This plan never spells that directory, because its encoded name carries the operator account name (the B-1 defect). `CMDDIR` = `SCRATCH\985r1-cmd` (outside the repository; never committed).
- `BASE-SHA` = `git merge-base HEAD origin/main`, recorded by P0-T2 (the remediation inputs name `9911fe138952e2b93476850582847c2831e1cbbd`; P0-T2 records the observed value and any difference).
- `R1-START-SHA` = `git rev-parse HEAD` recorded by P0-T2 (not pinned in this plan).
- `<TS>` = local time read from the host clock when the task runs: `pwsh -NoProfile -Command "Get-Date -Format yyyy-MM-ddTHH-mm"`. Never composed or estimated.

**C2. Evidence schema.** Every command-step artifact carries, in this order, `Timestamp:`, `Command:`, `EXIT_CODE:`, `ExpectedExitCode:` (only on `[expect-fail]` artifacts), and `Output Summary:` (1 to 20 lines carrying every value the task gates). Evidence lives only under `FEATURE/evidence/<kind>/`, kinds `remediation-baseline`, `regression-testing`, `qa-gates`, `other`. No non-canonical evidence path was supplied by the caller, so no `EVIDENCE_LOCATION_OVERRIDE_REJECTED` record applies.

**C3. Hygiene of written text.** No artifact, plan edit or agent-memory note written in this cycle may contain the account name, the host name, the user e-mail, or a drive-rooted user-profile path. `Command:` fields write `<SCRATCH>\985r1-cmd\<script>` and `WORKSPACE-ROOT`. Placeholders for a removed identifier follow `.claude/agent-memory/_shared_no_absolute_host_paths.md`: `<user>`, `<host>`, `<user-profile>`, `<repo-root>`; an e-mail address becomes `<user-email>`. In an XML-family file the escaped form (`&lt;user&gt;`) is used and the file is re-parsed after the edit. An artifact that records a substitution names the token class only and never quotes the removed value.

**C4. Shell discipline.** Bash permits only `git *` and `pwsh *` segments: no `cd`, use `git -C <dir>` and absolute script paths (`pwsh -NoProfile -File <CMDDIR>\<script>.ps1 ...`). Repository text searches use the Grep tool (a regex engine; `.`, `(`, `)`, `[`, `]`, `{`, `}` in counted tokens are escaped as written).

**C5. Loop rule.** PowerShell loop = P4-T1 to P4-T6. If any step fails, or P4-T1 rewrites any file, fix the cause inside the Write Set (section 4) and restart at P4-T1. Each iteration's artifacts carry `ITERATION: <n>`; only an iteration in which P4-T1 to P4-T6 all pass satisfies Phase 4.

**C6. Stop list.** STOP (record the reason in the task's artifact, leave the task unchecked, report to the caller) on: `STOP: PWSH-UNAVAILABLE`, `STOP: BASELINE-FORMAT-DRIFT` (P0-T5), `STOP: ANALYZE-BASELINE-RED` (P0-T6), `STOP: BASELINE-TEST-RED` (P0-T7, P0-T8), `STOP: SWEEP-BLIND` (any sweep whose controls fail), `STOP: TOKEN-EMPTY` (sweep exit 2), `STOP: WRONG-REASON` (P1-T1, P2-T4, P2-T5), `STOP: LIVE-TREE-SYNC-NONZERO` (P3-T6), `STOP: SIZE-LIMIT` (P3-T3, P4-T6), `STOP: SCOPE-VIOLATION` (P5-T1, P5-T2), `STOP: UNEXPECTED-WORKTREE-PATH` and `STOP: COMMIT-BLOCKED` (P6-T3), `STOP: IDENTITY-RESIDUAL-AFTER-COMMIT` (P6-T3). Loop failures follow C5.

**C7. Agent memory.** Every agent-memory write by the executor happens before P6-T2. A memory write after P6-T2 requires P6-T2 to be re-run before P6-T3. Agent-memory paths dirty at P6-T3 are committed (repository policy: commit all audit-trail evidence; the worktree ends clean).

**C8. Line endings.** `scripts/dependencies/Repair-PackageManifestConsistency.ps1` is CRLF (493 of 493 lines end in a carriage return); `scripts/dependencies/BindingRedirectSync.psm1` and both test files in the Write Set are LF (0 carriage returns). Edits keep each file's existing ending.

## 3. Planner Decisions

- **D1 (scope).** `remediation-inputs.2026-10-09T14-55.md` lists the code-review Minor and Info items as non-blocking and its "Do Not Do" section excludes production and test code. The caller directs this cycle to remediate CR-1, CR-2 and CR-3 in scope, consistent with the repository practice of remediating related defects on the item's own files rather than deferring them. That direction governs; the scope is limited to the two production files and two test files of section 4. The remaining Minor item (comment drift in `.github/workflows/dependabot-repair.yml` lines 87-105) stays out of scope under AC4; the orchestrator has promoted it to issue #986 (promoted record `docs/features/potential/promoted/2026-10-09-dependabot-repair-workflow-comments-predate-redirect-sync.md`), which this cycle commits without content change.
- **D2 (identity tokens are run-time derived).** The sweep script (section 5) derives the account name from the leaf of `$env:USERPROFILE` (plus the 8.3 short form found in `$env:TEMP`, when it differs), the host name from `$env:COMPUTERNAME` and the e-mail from `git config user.email`. The plan carries the derivation, never the values, so the plan file is itself inside the sweep without an exclusion. Each run proves the matcher is live with two controls: a synthetic string per token (`CONTROL-SYNTHETIC`, every value 1) and the worktree's `.git` pointer file, which names a profile path (`CONTROL-GITFILE-ACCOUNT` at least 1).
- **D3 (sweep scope).** Before the final commit, the sweep reads every added line of `git diff BASE-SHA` (merge base against the working tree, so committed branch changes and uncommitted edits are both covered) and every line of every untracked, non-ignored file (`git ls-files --others --exclude-standard`), which covers this plan, the review documents, the new evidence and new agent-memory files. After the commit the same script runs in `Committed` mode over `git diff BASE-SHA...HEAD`, the exact range the reviewer swept.
- **D4 (Direction contract).** `Direction` is `Downgrade` when both versions parse with `[System.Version]::TryParse` and the target is lower, `Upgrade` when both parse and the target is higher, and `Unknown` otherwise (either value unparsable, or the two numerically equal while textually different, for example `3.05.0.0` and `3.5.0.0`). The report line becomes `- <ProjectDirectory>: <AssemblyName> <From> to <To> (<Rule>, <Direction>)`. The `From to To` substring is unchanged, so the existing body assertion in `Repair-PackageManifestConsistency.RedirectSync.Tests.ps1` line 158 still holds.
- **D5 (WrittenPath de-duplication).** Implemented at the publication point in the script with a `HashSet[string]` built on `[System.StringComparer]::OrdinalIgnoreCase`, filtering `$written` in order. `WrittenPath` stays `[string[]]`. The regression test reproduces the duplicate the reviewer described (the sync pass and the normalisation pass both rewrite one `app.config`). A case-variant duplicate is not reachable through the entry point, because every pass enumerates paths from the same lister, so the case-insensitive comparer is fixed by code reading (P3-T2 acceptance) and not by a test.
- **D6 (CR-3 behaviour).** `Invoke-BindingRedirectReconciliation` already matches the name case-insensitively (`ProjectConsistency.psm1` line 330 `-eq` and line 344 `-notmatch`), so the first record rewrites every case variant of a name; the second record only produced a redundant repair record. The fix replaces the `List[string]` at `BindingRedirectSync.psm1` line 114 with a `HashSet[string]` on `OrdinalIgnoreCase` and lines 121-122 with one `Add` test. The regression test asserts one repair record, two examined entries and both blocks rewritten.
- **D7 (C# toolchain not re-run).** This cycle changes no `*.cs`, `*.csproj`, `*.props`, `*.targets`, `*.sln` or `*.config` file. P5-T1 proves it with `git diff --name-only R1-START-SHA` plus `git status --porcelain` over those patterns; the cycle-0 C# QA iteration (`FEATURE/evidence/qa-gates/cs-*.2026-10-09T14-30.md` and `cs-test.2026-10-09T14-31.md`) remains the C# record. A non-empty result is `STOP: SCOPE-VIOLATION`.
- **D8 (PowerShell numbers).** The PoshQC MCP tools return only an `ok` flag and a summary, and the MCP coverage document does not instrument `scripts/dependencies` (review observation O-1). Each MCP step runs for the policy record and is paired with the direct Pester run of section 5 (`985-pester.ps1`, mirroring `.github/workflows/_pester.yml`), which prints pass/fail counts and per-file line coverage.
- **D9 (coverage gates).** Aggregate line coverage over `scripts/dependencies`, `scripts/hygiene`, `scripts/vscode` at or above 80.00 (CLAUDE.md UT2; the 85 figure in `.claude/rules/quality-tiers.md` is recorded as `POLICY-85-OBSERVATION`); `BindingRedirectSync.psm1` at or above 90.00 (new-code rule); `Repair-PackageManifestConsistency.ps1` per-file line coverage not below its P0-T8 figure (changed lines no-regression). Pester measures no branch coverage, so no branch gate applies to PowerShell.

## 4. Write Set

- Production: `scripts/dependencies/BindingRedirectSync.psm1`, `scripts/dependencies/Repair-PackageManifestConsistency.ps1` (2 production PowerShell files; direct-mode budget 1-3).
- Tests: `tests/scripts/dependencies/BindingRedirectSync.Tests.ps1`, `tests/scripts/dependencies/Repair-PackageManifestConsistency.RedirectSync.Tests.ps1`.
- Feature folder: `FEATURE/plan.2026-10-09T13-06.md` (line 26 only, plus any further identity hit P1-T1 lists), this plan (checkboxes), the evidence files named below, and any further file P1-T1 lists as carrying an identity hit (identifier substitution only).
- Committed without content change by this cycle: the review artifacts already in the worktree (`FEATURE/code-review.2026-10-09T14-55.md`, `FEATURE/feature-audit.2026-10-09T14-55.md`, `FEATURE/policy-audit.2026-10-09T14-55.md`, `FEATURE/remediation-inputs.2026-10-09T14-55.md`), the issue #986 promoted record `docs/features/potential/promoted/2026-10-09-dependabot-repair-workflow-comments-predate-redirect-sync.md`, and dirty `.claude/agent-memory/` files (C7), unless P1-T1 lists an identity hit in one of them.

## 5. Helper Scripts (written by P0-T3 into CMDDIR)

Three helpers are copied verbatim from `FEATURE/plan.2026-10-09T13-06.md` section 5, each with the `Format-SafeLine` function (that file's lines 85-94) pasted directly after its `param` block:
- `985-pester.ps1` = that file's lines 248-307.
- `985-junit.ps1` = that file's lines 313-329.
- `985-repair.ps1` = that file's lines 335-361.

The fourth helper is new. **985r1-identity-sweep.ps1**

```powershell
param(
    [Parameter(Mandatory = $true)][string]$WorkspaceRoot,
    [Parameter(Mandatory = $true)][string]$Base,
    [Parameter(Mandatory = $true)][ValidateSet('WorkingTree', 'Committed')][string]$Mode
)
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
Set-Location -LiteralPath $WorkspaceRoot
$token = [ordered]@{
    ACCOUNT = [string](Split-Path -Leaf $env:USERPROFILE)
    HOST    = [string]$env:COMPUTERNAME
    EMAIL   = [string](& git config user.email)
}
if ([string]$env:TEMP -match '(?i)[\\/]users[\\/](?<short>[^\\/]+)[\\/]' -and $Matches['short'] -ne $token['ACCOUNT']) {
    $token['ACCOUNT_SHORT'] = [string]$Matches['short']
}
foreach ($key in @($token.Keys)) {
    if ([string]::IsNullOrWhiteSpace($token[$key])) { "TOKEN-EMPTY: $key"; exit 2 }
}
"MODE: $Mode"
"TOKENS: $(@($token.Keys) -join ',')"
$pattern = [ordered]@{}
foreach ($key in @($token.Keys)) { $pattern[$key] = [regex]::new([regex]::Escape($token[$key]), 'IgnoreCase') }
$pattern['USERS_PATH'] = [regex]::new('[a-z]:[\\/]+users[\\/]+[a-z0-9_.~-]', 'IgnoreCase')
$synthetic = @(foreach ($key in @($token.Keys)) { "$key=$($pattern[$key].Matches('x' + $token[$key] + 'y').Count)" })
"CONTROL-SYNTHETIC: $($synthetic -join ' ')"
$gitFile = Join-Path $WorkspaceRoot '.git'
$gitFileHit = if (Test-Path -LiteralPath $gitFile -PathType Leaf) { $pattern['ACCOUNT'].Matches([System.IO.File]::ReadAllText($gitFile)).Count } else { -1 }
"CONTROL-GITFILE-ACCOUNT: $gitFileHit"
$range = if ($Mode -eq 'Committed') { "$Base...HEAD" } else { $Base }
$diff = @(& git -c core.quotepath=off diff --no-color --no-ext-diff --unified=0 $range)
if ($LASTEXITCODE -ne 0) { "GIT-DIFF-EXIT: $LASTEXITCODE"; exit 2 }
$line = [System.Collections.Generic.List[pscustomobject]]::new()
$file = ''
$inHunk = $false
$number = 0
foreach ($entry in $diff) {
    if ($entry.StartsWith('diff --git ')) { $inHunk = $false; $file = ''; continue }
    if ($entry.StartsWith('@@')) {
        $inHunk = $true
        if ($entry -match '\+(?<start>\d+)') { $number = [int]$Matches['start'] }
        continue
    }
    if (-not $inHunk) {
        if ($entry.StartsWith('+++ ')) { $file = $entry.Substring(4) -replace '^b/', '' }
        continue
    }
    if ($entry.StartsWith('+')) {
        $line.Add([pscustomobject]@{ File = $file; Number = $number; Text = $entry.Substring(1) })
        $number++
    }
}
$untracked = @()
if ($Mode -eq 'WorkingTree') {
    $untracked = @(& git -c core.quotepath=off ls-files --others --exclude-standard)
    foreach ($path in $untracked) {
        $index = 0
        foreach ($text in [System.IO.File]::ReadAllLines((Join-Path $WorkspaceRoot $path))) {
            $index++
            $line.Add([pscustomobject]@{ File = $path; Number = $index; Text = $text })
        }
    }
}
"ADDED-OR-UNTRACKED-LINES: $($line.Count)"
"UNTRACKED-FILE-COUNT: $($untracked.Count)"
$total = 0
foreach ($key in @($pattern.Keys)) {
    $hit = @($line | Where-Object { $pattern[$key].IsMatch($_.Text) })
    "HITS-$($key): $($hit.Count)"
    foreach ($entry in $hit) { "HIT-$($key): $($entry.File):$($entry.Number)" }
    $total += $hit.Count
}
"HITS-TOTAL: $total"
if ($total -gt 0) { exit 1 }
exit 0
```

The script prints token names, counts and `file:line` locations only; it never prints a token value or a matched line.

## 6. Implementation Contracts

### 6.1 `scripts/dependencies/BindingRedirectSync.psm1` (LF, ASCII only, at most 500 lines; 306 lines before this cycle)

1. Insert a private function `Get-RedirectDirection` after `Test-SyncProjectPath` (which ends at line 72) and before `function Invoke-BindingRedirectSync` (line 74), preceded by a `# Private.` comment stating that it classifies a rewrite by numeric comparison so a downward rewrite is visible in the report, and that an unparsable value or a numerically equal pair yields `Unknown`. Contract: `[CmdletBinding()]`, `[OutputType([string])]`, mandatory `[string]$From` and `[string]$To`; body: `$fromVersion = $null`, `$toVersion = $null`; when `[System.Version]::TryParse($From, [ref]$fromVersion) -and [System.Version]::TryParse($To, [ref]$toVersion)`: return `'Downgrade'` when `$toVersion -lt $fromVersion`, `'Upgrade'` when `$toVersion -gt $fromVersion`; otherwise fall through to `return 'Unknown'`. Not exported.
2. Replace line 114 (`$handled = [System.Collections.Generic.List[string]]::new()`) with a one-line `#` comment stating that assembly names bind case-insensitively, so a block differing only in letter case names the same assembly, followed by `$handled = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::OrdinalIgnoreCase)`.
3. Replace lines 121-122 (`if ($handled.Contains($record.Name)) { continue }` and `$handled.Add($record.Name)`) with the single line `if (-not $handled.Add($record.Name)) { continue }`.
4. In the repair record (lines 148-155) add `Direction    = (Get-RedirectDirection -From $record.NewVersion -To $target)` after `Rule`.
5. In `Format-BindingRedirectSyncReport` change the format string at line 296 to `'- {0}: {1} {2} to {3} ({4}, {5})'` and append `$record.Direction` as the sixth argument.
6. Help text: in `Invoke-BindingRedirectSync` `.DESCRIPTION`, change "a name already handled in this call is skipped" to state the comparison is case-insensitive, and state in `.OUTPUTS` that each repair record carries `Direction` (`Upgrade`, `Downgrade` or `Unknown`); in `Format-BindingRedirectSyncReport` `.OUTPUTS`, state that each line ends with the rule and the direction.

### 6.2 `scripts/dependencies/Repair-PackageManifestConsistency.ps1` (CRLF, at most 500 lines; 493 before this cycle)

1. After line 465 (`if ($syncReport) { ... }`) insert two lines: a `#` comment stating that a file rewritten by more than one pass is listed once and that Windows paths compare case-insensitively, then `$distinctWritten = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::OrdinalIgnoreCase)`.
2. Replace the right-hand side of the `WrittenPath` entry (line 474, `$written.ToArray()`) with `[string[]]@($written | Where-Object { $distinctWritten.Add($_) })`, keeping the existing `=` column alignment.
3. No other line changes. Expected length 495.

### 6.3 Test additions

`tests/scripts/dependencies/BindingRedirectSync.Tests.ps1` (LF; 358 lines before this cycle). Every fixture in memory, built with the file's existing `Get-AppConfigText` and `Get-VersionProvider`; Arrange-Act-Assert comments as in the existing tests; no name matches the regex `AC` followed by a digit.

Append inside `Describe 'Invoke-BindingRedirectSync (in-memory fixtures)'`, after the CRLF byte-identity `It` (ends at line 270):
- N1 `It 'marks a rewrite to a higher version as an upgrade'`: redirect `log4net` `0.0.0.0-3.4.0.0` / `3.4.0.0`, deployed `log4net` = `3.5.0.0`. Assert `$result.Repair[0].Direction | Should -BeExactly 'Upgrade'`.
- N2 `It 'marks a rewrite to a lower version as a downgrade'`: redirect `log4net` `0.0.0.0-3.6.0.0` / `3.6.0.0`, deployed `3.5.0.0`. Assert `To` `3.5.0.0`, `Rule` `HighestDeployed`, `Direction` exactly `Downgrade`, text matches `newVersion="3\.5\.0\.0"`.
- N3 `It 'marks the direction unknown when the stale version does not parse'`: redirect `log4net` `0.0.0.0-3.4.0.0` / `latest`, deployed `3.5.0.0`. Assert `To` `3.5.0.0` and `Direction` exactly `Unknown`.
- N4 `It 'marks the direction unknown when the two versions are numerically equal'`: redirect `log4net` `0.0.0.0-3.05.0.0` / `3.05.0.0`, deployed `3.5.0.0`. Assert `To` `3.5.0.0` and `Direction` exactly `Unknown`.
- N5 `It 'processes an assembly name once when two blocks differ only in letter case'`: two redirects, `log4net` and `Log4Net`, each `0.0.0.0-3.4.0.0` / `3.4.0.0`; deployed map `@{ 'log4net' = @('3.5.0.0') }` (a PowerShell hashtable literal looks keys up case-insensitively, which a comment states). Assert `@($result.Repair).Count | Should -Be 1`, `$result.ExaminedCount | Should -Be 2`, and `[regex]::Matches($result.Text, 'newVersion="3\.5\.0\.0"').Count | Should -Be 2`.

In `Describe 'Format-BindingRedirectSyncReport'`:
- Modify the existing `It 'returns the heading and one line per repair'` (lines 343-357): add `Direction = 'Upgrade'` to both fixture records and change the two expected lines to `'- Tags.Test: log4net 3.4.0.0 to 3.5.0.0 (HighestDeployed, Upgrade)'` and `'- Prod: Contoso 1.0.0.0 to 2.0.0.0 (OwnReference, Upgrade)'`.
- N6 `It 'states a downgrade in the report line'`: one record `ProjectDirectory = 'Tags.Test'; AssemblyName = 'log4net'; From = '3.6.0.0'; To = '3.5.0.0'; Rule = 'HighestDeployed'; Direction = 'Downgrade'`. Assert the second line is exactly `'- Tags.Test: log4net 3.6.0.0 to 3.5.0.0 (HighestDeployed, Downgrade)'`.

`tests/scripts/dependencies/Repair-PackageManifestConsistency.RedirectSync.Tests.ps1` (LF; 187 lines before this cycle):
- N7, inside `Context 'A transitive redirect left stale by an upgrade elsewhere, with no candidate upgrade supplied'`, after the `It` ending at line 171: `It 'states the direction of the synchronised redirect in the body'`, asserting `$script:FirstResult.Body | Should -Match 'log4net 3\.4\.0\.0 to 3\.5\.0\.0 \(HighestDeployed, Upgrade\)'`.
- New `Context 'An application configuration rewritten by both the redirect sync and the normalisation pass'`, inside the `Describe` after the `-WhatIf` context. Its `BeforeAll` builds `$reflowed = $script:TestAppConfig.Replace('    <assemblyBinding xmlns=', "    <assemblyBinding`r`n        xmlns=")` (the `assemblyBinding` start tag split across two lines, which `ConvertTo-AppConfigText` collapses), builds a fixture with `Get-RepairFixture` over the same four files as `Get-TransitiveFixture` plus `$script:TestAppConfigPath = $reflowed` and `-Identity $script:AssemblyIdentity`, and runs the entry point once without `-CandidateUpgrade`, keeping the result and the store. A comment states that the sync pass writes the stale redirect and the normalisation pass then writes the collapsed start tag, so one file is written twice.
  - N8 `It 'applies both rewrites to the application configuration'`: the stored text matches `newVersion="3\.5\.0\.0"` and `<assemblyBinding xmlns="urn:schemas-microsoft-com:asm\.v1">` (guards N9 against a fixture in which only one pass writes).
  - N9 `It 'lists the application configuration once in the written paths'`: `@($result.WrittenPath | Where-Object { $_ -eq $script:TestAppConfigPath }).Count | Should -Be 1`.

## 7. Phases

### Phase 0 — Policy Reads and Baselines

- [x] [P0-T1] Read, in this order, `CLAUDE.md`, `.claude/rules/general-code-change.md`, `.claude/rules/general-unit-test.md`, `.claude/rules/quality-tiers.md`, `.claude/rules/powershell.md`, `.claude/rules/tonality.md`, `.claude/rules/plan-acceptance-gates.md`, `.claude/skills/atomic-plan-contract/SKILL.md`, `.claude/skills/evidence-and-timestamp-conventions/SKILL.md`, `.claude/skills/acceptance-criteria-tracking/SKILL.md`, `.claude/agent-memory/_shared_no_absolute_host_paths.md`, then `FEATURE/remediation-inputs.2026-10-09T14-55.md`, `FEATURE/code-review.2026-10-09T14-55.md`, `FEATURE/spec.md`; write `FEATURE/evidence/remediation-baseline/phase0-instructions-read.md` with `Timestamp:`, `Policy Order:` (the order above) and `Files Read:` listing all fourteen paths. Acceptance: the artifact exists with the three fields and fourteen paths. Evidence: `FEATURE/evidence/remediation-baseline/phase0-instructions-read.md`.
- [x] [P0-T2] Record git state in `FEATURE/evidence/remediation-baseline/git-state.<TS>.md`: `git rev-parse --abbrev-ref HEAD` (must print `bug/dependabot-repair-borrowed-packages-and-transitive-redirects-985`), `git rev-parse HEAD` (record as `R1-START-SHA:`), `git merge-base HEAD origin/main` (record as `BASE-SHA:` and state whether it equals the value in C1), `git status --porcelain` (every line verbatim as `PreExistingWorktreePaths:`). Acceptance: branch name matches; both SHAs are 40-character hex; the porcelain lines are recorded. Evidence: `FEATURE/evidence/remediation-baseline/git-state.<TS>.md`.
- [x] [P0-T3] Write the four helper scripts of section 5 into `CMDDIR` with the Write tool (`985-pester.ps1`, `985-junit.ps1`, `985-repair.ps1` copied from the cited line spans of `FEATURE/plan.2026-10-09T13-06.md` with `Format-SafeLine` pasted after each `param` block; `985r1-identity-sweep.ps1` verbatim from section 5), then record the four names and `git hash-object --no-filters <CMDDIR>\<name>` of each in `FEATURE/evidence/remediation-baseline/helper-scripts.<TS>.md` (paths written as `<SCRATCH>\985r1-cmd\<name>`). Acceptance: four names and four hashes recorded. Evidence: `FEATURE/evidence/remediation-baseline/helper-scripts.<TS>.md`.
- [x] [P0-T4] Record pre-change tree facts in `FEATURE/evidence/remediation-baseline/tree-facts.<TS>.md` with the Grep tool: line counts (pattern `^`, count) of `scripts/dependencies/BindingRedirectSync.psm1` (306), `scripts/dependencies/Repair-PackageManifestConsistency.ps1` (493), `tests/scripts/dependencies/BindingRedirectSync.Tests.ps1` (358), `tests/scripts/dependencies/Repair-PackageManifestConsistency.RedirectSync.Tests.ps1` (187); carriage-return counts (pattern `\r$`) of the same four files (0, 493, 0, 0); in `scripts/dependencies/BindingRedirectSync.psm1` the counts of `\$handled\.Contains\(` (1), `function Get-RedirectDirection` (0) and `Direction` (0); in `scripts/dependencies/Repair-PackageManifestConsistency.ps1` the count of `distinctWritten` (0); in `FEATURE/plan.2026-10-09T13-06.md` the content-mode `-o` matches of `claude.C--Users-` (2, both on line 26) and of `<encoded-worktree>` (0). Acceptance: every observed value equals the parenthesised value; a difference is recorded and reported, and the affected anchors in sections 6.1 to 6.3 are re-derived before Phase 1. Evidence: `FEATURE/evidence/remediation-baseline/tree-facts.<TS>.md`.
- [x] [P0-T5] PowerShell baseline format: record `git status --porcelain` and `git hash-object --no-filters` of the four PowerShell Write Set files, call `mcp__drm-copilot__run_poshqc_format` with `workspace_root = WORKSPACE-ROOT` and `scan_folders = [scripts/dependencies, tests/scripts/dependencies]`, then repeat both observations; write `FEATURE/evidence/remediation-baseline/ps-format.<TS>.md` (`EXIT_CODE:` 0 when `ok` is true, else 1; the MCP `summary` verbatim). Acceptance (success-case observation, not the exit code): porcelain identical before and after and all four hashes identical; a rewrite is `STOP: BASELINE-FORMAT-DRIFT` (revert with `git checkout -- <file>` first). Evidence: `FEATURE/evidence/remediation-baseline/ps-format.<TS>.md`.
- [x] [P0-T6] PowerShell baseline lint: call `mcp__drm-copilot__run_poshqc_analyze` with `workspace_root = WORKSPACE-ROOT` and `scan_folders = [scripts/dependencies, tests/scripts/dependencies]`; write `FEATURE/evidence/remediation-baseline/ps-analyze.<TS>.md` (`EXIT_CODE:` 0 when `ok` is true, else 1), stating that type checking does not apply to PowerShell (`.claude/rules/powershell.md` toolchain step 3). Acceptance: `ok` true; false is `STOP: ANALYZE-BASELINE-RED`. Evidence: `FEATURE/evidence/remediation-baseline/ps-analyze.<TS>.md`.
- [x] [P0-T7] PowerShell baseline MCP test: run `pwsh -NoProfile -File <CMDDIR>\985-junit.ps1 -WorkspaceRoot <WORKTREE> -Clear`, call `mcp__drm-copilot__run_poshqc_test` with `workspace_root = WORKSPACE-ROOT` and `scan_folders = [tests/scripts/dependencies]`, then run `985-junit.ps1 -WorkspaceRoot <WORKTREE>` without `-Clear`; write `FEATURE/evidence/remediation-baseline/ps-test-mcp.<TS>.md` with the `ok` flag and the `JUNIT` and `JUNIT-SUITE` lines (`EXIT_CODE:` 1 when failures plus errors is above 0, else 0). Acceptance: `JUNIT-CLEARED: True`, a `JUNIT` line present, failures 0 and errors 0; otherwise `STOP: BASELINE-TEST-RED`. Evidence: `FEATURE/evidence/remediation-baseline/ps-test-mcp.<TS>.md`.
- [x] [P0-T8] PowerShell baseline direct coverage: run `pwsh -NoProfile -File <CMDDIR>\985-pester.ps1 -WorkspaceRoot <WORKTREE> -TestPath tests/scripts/dependencies,tests/scripts/hygiene,tests/scripts/vscode -CoveragePath scripts/dependencies,scripts/hygiene,scripts/vscode -CoverageOutput coverage/985r1-pester-baseline.xml`; write `FEATURE/evidence/remediation-baseline/ps-coverage.<TS>.md` with the `PESTER` and `COVERAGE` lines, every `FILE-TESTS` line, and the `FILE-COVERAGE` lines ending `/BindingRedirectSync.psm1` and `/Repair-PackageManifestConsistency.ps1`. Acceptance: `TEST-PATH-COUNT: 3`, `COVERAGE-PATH-COUNT: 3`, `Failed=0` (otherwise `STOP: BASELINE-TEST-RED`); recorded as numbers: `R1-BASELINE-PS-AGGREGATE:`, `R1-BASELINE-MODULE-LINE:`, `R1-BASELINE-REPAIR-SCRIPT-LINE:`, and `R1-BASELINE-REPAIR-TESTS-TOTAL:` (the `Total=` of `FILE-TESTS Repair-PackageManifestConsistency.Tests.ps1`); the raw document stays under the ignored `coverage/` directory. Evidence: `FEATURE/evidence/remediation-baseline/ps-coverage.<TS>.md`.

### Phase 1 — Identity Hygiene (B-1)

- [x] [P1-T1] [expect-fail] Run `pwsh -NoProfile -File <CMDDIR>\985r1-identity-sweep.ps1 -WorkspaceRoot <WORKTREE> -Base <BASE-SHA> -Mode WorkingTree`; write `FEATURE/evidence/regression-testing/fail-before-identity-sweep.<TS>.md` with `ExpectedExitCode: 1` and every output line. Acceptance (right reason): `EXIT_CODE: 1`; `CONTROL-SYNTHETIC:` shows `=1` for every token and `CONTROL-GITFILE-ACCOUNT:` is at least 1 (otherwise `STOP: SWEEP-BLIND`); a `HIT-ACCOUNT:` line names `FEATURE/plan.2026-10-09T13-06.md:26` (repository-relative); exit 2 is `STOP: TOKEN-EMPTY`; any other exit is `STOP: WRONG-REASON`. Every `HIT-` line other than `plan.2026-10-09T13-06.md:26` is recorded as `FURTHER-HIT:` for P1-T3. Evidence: `FEATURE/evidence/regression-testing/fail-before-identity-sweep.<TS>.md`.
- [x] [P1-T2] In `FEATURE/plan.2026-10-09T13-06.md` line 26, with the Edit tool, replace each of the two occurrences of the encoded directory segment and the session identifier that follow `Temp\claude\` (the text between `Temp\claude\` and `\scratchpad`) with `<encoded-worktree>\<session-id>`, leaving every other character of the bullet unchanged. Acceptance: Grep content mode `-o` over that file returns 2 matches of `<encoded-worktree>.<session-id>.scratchpad`, both on line 26, and 0 matches of `claude.C--Users-`; the file's line count is unchanged (Grep `^` count equal before and after). Evidence: counts recorded in `FEATURE/evidence/regression-testing/pass-after-identity-sweep.<TS>.md` (P1-T4).
- [x] [P1-T3] For every `FURTHER-HIT:` line recorded by P1-T1, replace the identifier in that file and line with the C3 placeholder for its token class (`<user>`, `<host>`, `<user-email>`, or a `<user-profile>`-composed path for `USERS_PATH`), escaped in an XML-family file, changing nothing else on the line; record each fix as `FIXED: <file>:<line> <token class>` (never the removed value) in `FEATURE/evidence/regression-testing/pass-after-identity-sweep.<TS>.md`, or record `FURTHER-HITS: NONE` when P1-T1 listed none. Acceptance: one `FIXED:` line per `FURTHER-HIT:` line, or `FURTHER-HITS: NONE`; each rewritten XML-family file parses with `[xml]`. Evidence: `FEATURE/evidence/regression-testing/pass-after-identity-sweep.<TS>.md`.
- [x] [P1-T4] Re-run `985r1-identity-sweep.ps1 -WorkspaceRoot <WORKTREE> -Base <BASE-SHA> -Mode WorkingTree` from `CMDDIR`; write its output, with the P1-T2 counts and the P1-T3 lines, to `FEATURE/evidence/regression-testing/pass-after-identity-sweep.<TS>.md`. Acceptance: `EXIT_CODE: 0`, `HITS-TOTAL: 0`, every `HITS-` line 0, `CONTROL-SYNTHETIC:` all `=1`, `CONTROL-GITFILE-ACCOUNT:` at least 1. Evidence: `FEATURE/evidence/regression-testing/pass-after-identity-sweep.<TS>.md`.

### Phase 2 — Regression Tests First (CR-1, CR-2, CR-3)

- [x] [P2-T1] Add N1 to N5 of section 6.3 to `tests/scripts/dependencies/BindingRedirectSync.Tests.ps1` inside `Describe 'Invoke-BindingRedirectSync (in-memory fixtures)'` with the Edit tool (LF kept). Acceptance: Grep count of `^\s+It '` in the file is 23; each of the five `It` names of N1 to N5 has Grep count 1; Grep count of `New-TemporaryFile|GetTempPath|GetTempFileName|TestDrive|WriteAllText|Set-Content|Out-File|New-Item` is 0. Evidence: counts recorded in `FEATURE/evidence/regression-testing/fail-before-sync-module.<TS>.md` (P2-T4).
- [x] [P2-T2] In `tests/scripts/dependencies/BindingRedirectSync.Tests.ps1` `Describe 'Format-BindingRedirectSyncReport'`, modify the existing `It 'returns the heading and one line per repair'` and add N6, per section 6.3. Acceptance: Grep count of `^\s+It '` is 24; Grep counts of `\(HighestDeployed, Upgrade\)'`, `\(OwnReference, Upgrade\)'` and `\(HighestDeployed, Downgrade\)'` are each 1; Grep count of `\(HighestDeployed\)'` is 0. Evidence: counts recorded in `FEATURE/evidence/regression-testing/fail-before-sync-module.<TS>.md` (P2-T4).
- [x] [P2-T3] Add N7, and the new `Context` with N8 and N9, to `tests/scripts/dependencies/Repair-PackageManifestConsistency.RedirectSync.Tests.ps1` per section 6.3. Acceptance: Grep count of `^\s+It '` is 9; Grep count of `^\s+Context '` is 3; Grep count of `New-TemporaryFile|GetTempPath|GetTempFileName|TestDrive|WriteAllText|Set-Content|Out-File|New-Item` is 0. Evidence: counts recorded in `FEATURE/evidence/regression-testing/fail-before-repair-written-path.<TS>.md` (P2-T5).
- [x] [P2-T4] [expect-fail] Run `pwsh -NoProfile -File <CMDDIR>\985-pester.ps1 -WorkspaceRoot <WORKTREE> -TestPath tests/scripts/dependencies/BindingRedirectSync.Tests.ps1`; write `FEATURE/evidence/regression-testing/fail-before-sync-module.<TS>.md` with `ExpectedExitCode: 1`, the P2-T1 and P2-T2 counts, and every `FAILED-TEST:` and `FAILED-MESSAGE:` line. Acceptance (right reason): `EXIT_CODE: 1`; `PESTER Passed=17 Failed=7` with `Total=24`; the failed set is exactly N1, N2, N3, N4, N5, N6 and `returns the heading and one line per repair`; N5's `FAILED-MESSAGE:` contains `but got 2`. Any other shape is `STOP: WRONG-REASON`. Evidence: `FEATURE/evidence/regression-testing/fail-before-sync-module.<TS>.md`.
- [x] [P2-T5] [expect-fail] Run `985-pester.ps1 -WorkspaceRoot <WORKTREE> -TestPath tests/scripts/dependencies/Repair-PackageManifestConsistency.RedirectSync.Tests.ps1` from `CMDDIR`; write `FEATURE/evidence/regression-testing/fail-before-repair-written-path.<TS>.md` with `ExpectedExitCode: 1`, the P2-T3 counts and every `FAILED-TEST:` and `FAILED-MESSAGE:` line. Acceptance (right reason): `EXIT_CODE: 1`; `PESTER Passed=7 Failed=2` with `Total=9`; the failed set is exactly N7 and N9; N9's `FAILED-MESSAGE:` contains `but got 2`; N8 passed. Any other shape is `STOP: WRONG-REASON`. Evidence: `FEATURE/evidence/regression-testing/fail-before-repair-written-path.<TS>.md`.

### Phase 3 — Implementation

- [x] [P3-T1] Apply section 6.1 items 1 to 6 to `scripts/dependencies/BindingRedirectSync.psm1` with the Edit tool (LF, ASCII only). Acceptance: Grep counts in that file: `function Get-RedirectDirection` 1; `HashSet\[string\]\]::new\(\[System\.StringComparer\]::OrdinalIgnoreCase\)` 1; `\$handled\.Contains\(` 0; `if \(-not \$handled\.Add\(` 1; `Direction\s+= \(Get-RedirectDirection` 1; `\(\{4\}, \{5\}\)` 1; `'Get-RedirectDirection'` 0 (not exported); `^Export-ModuleMember` 1; `[^\x00-\x7F]` 0. Evidence: counts recorded in `FEATURE/evidence/other/line-counts-r1.<TS>.md` (P3-T3).
- [x] [P3-T2] Apply section 6.2 to `scripts/dependencies/Repair-PackageManifestConsistency.ps1` with the Edit tool (CRLF kept). Acceptance: Grep counts in that file: `distinctWritten` 2; `OrdinalIgnoreCase` at least 1 on the `$distinctWritten =` line (read the line to confirm the comparer); `\$written\.ToArray\(\)` 0; carriage-return count (pattern `\r$`) equal to the line count. Evidence: counts recorded in `FEATURE/evidence/other/line-counts-r1.<TS>.md` (P3-T3).
- [x] [P3-T3] Record line counts (Grep `^`) and carriage-return counts (Grep `\r$`) of `scripts/dependencies/BindingRedirectSync.psm1` and `scripts/dependencies/Repair-PackageManifestConsistency.ps1`, plus the P3-T1 and P3-T2 counts, in `FEATURE/evidence/other/line-counts-r1.<TS>.md`. Acceptance: both line counts at most 500 (expected about 330 and 495); the module's carriage-return count 0 and the script's equal to its line count; above 500 is `STOP: SIZE-LIMIT`. Evidence: `FEATURE/evidence/other/line-counts-r1.<TS>.md`.
- [x] [P3-T4] Pass-after for the module tests: run `985-pester.ps1 -WorkspaceRoot <WORKTREE> -TestPath tests/scripts/dependencies/BindingRedirectSync.Tests.ps1` from `CMDDIR`; write `FEATURE/evidence/regression-testing/pass-after-sync-module.<TS>.md`. Acceptance: `EXIT_CODE: 0`; `PESTER Passed=24 Failed=0`; no `FAILED-TEST:` line. Evidence: `FEATURE/evidence/regression-testing/pass-after-sync-module.<TS>.md`.
- [x] [P3-T5] Pass-after for the entry-point tests: run `985-pester.ps1 -WorkspaceRoot <WORKTREE> -TestPath tests/scripts/dependencies/Repair-PackageManifestConsistency.RedirectSync.Tests.ps1,tests/scripts/dependencies/Repair-PackageManifestConsistency.Tests.ps1` from `CMDDIR`; write `FEATURE/evidence/regression-testing/pass-after-repair-written-path.<TS>.md`. Acceptance: `TEST-PATH-COUNT: 2`; `EXIT_CODE: 0`; `FILE-TESTS Repair-PackageManifestConsistency.RedirectSync.Tests.ps1 Passed=9 Failed=0 Total=9`; `FILE-TESTS Repair-PackageManifestConsistency.Tests.ps1` with `Failed=0` and `Total=` equal to `R1-BASELINE-REPAIR-TESTS-TOTAL`. Evidence: `FEATURE/evidence/regression-testing/pass-after-repair-written-path.<TS>.md`.
- [x] [P3-T6] Live-tree smoke of `scripts/dependencies/Repair-PackageManifestConsistency.ps1` with `-WhatIf`: record `git status --porcelain`, run `pwsh -NoProfile -File <CMDDIR>\985-repair.ps1 -WorkspaceRoot <WORKTREE> -WhatIfRun`, record `git status --porcelain` again; write `FEATURE/evidence/other/whatif-live-tree-r1.<TS>.md` (`IS-SUCCESS` recorded, not gated). Acceptance: the script completes; `WRITTEN-COUNT: 0`; `REDIRECTSYNC-REPAIR-COUNT: 0`; `REDIRECTSYNC-UNRESOLVABLE:` empty; the two porcelain outputs identical. A non-zero sync count is `STOP: LIVE-TREE-SYNC-NONZERO` with the `REDIRECTSYNC:` lines reported. Evidence: `FEATURE/evidence/other/whatif-live-tree-r1.<TS>.md`.

### Phase 4 — PowerShell QA Loop

- [x] [P4-T1] Format: record `git status --porcelain` and `git hash-object --no-filters` of the four PowerShell Write Set files, call `mcp__drm-copilot__run_poshqc_format` (`workspace_root = WORKSPACE-ROOT`, `scan_folders = [scripts/dependencies, tests/scripts/dependencies]`), repeat both observations; write `FEATURE/evidence/qa-gates/ps-format-r1.<TS>.md` with `ITERATION:`. Acceptance (success-case observation): porcelain identical and all four hashes identical before and after; any rewrite is a loop restart (C5). Evidence: `FEATURE/evidence/qa-gates/ps-format-r1.<TS>.md`.
- [x] [P4-T2] Lint `scripts/dependencies` and `tests/scripts/dependencies` with `mcp__drm-copilot__run_poshqc_analyze` (same arguments); write `FEATURE/evidence/qa-gates/ps-analyze-r1.<TS>.md` with `ITERATION:` and the type-check not-applicable statement. Acceptance: `ok` true. Evidence: `FEATURE/evidence/qa-gates/ps-analyze-r1.<TS>.md`.
- [x] [P4-T3] MCP test over `tests/scripts/dependencies`: `985-junit.ps1 -WorkspaceRoot <WORKTREE> -Clear`, `mcp__drm-copilot__run_poshqc_test` (`scan_folders = [tests/scripts/dependencies]`), `985-junit.ps1 -WorkspaceRoot <WORKTREE>`; write `FEATURE/evidence/qa-gates/ps-test-mcp-r1.<TS>.md` with `ITERATION:`. Acceptance: `JUNIT-CLEARED: True`; failures 0 and errors 0; the `JUNIT-SUITE` line for `BindingRedirectSync.Tests.ps1` shows `tests=24` and the line for `Repair-PackageManifestConsistency.RedirectSync.Tests.ps1` shows `tests=9`. Evidence: `FEATURE/evidence/qa-gates/ps-test-mcp-r1.<TS>.md`.
- [x] [P4-T4] Direct coverage run of `scripts/dependencies`, `scripts/hygiene` and `scripts/vscode`: `985-pester.ps1` with the P0-T8 arguments and `-CoverageOutput coverage/985r1-pester-final.xml`; write `FEATURE/evidence/qa-gates/ps-coverage-r1.<TS>.md` with `ITERATION:`. Acceptance: `TEST-PATH-COUNT: 3`, `COVERAGE-PATH-COUNT: 3`, `EXIT_CODE: 0`, `Failed=0`; `COVERAGE LinePercent` at or above 80.00; the `FILE-COVERAGE` line ending `/BindingRedirectSync.psm1` at or above 90.00; the line ending `/Repair-PackageManifestConsistency.ps1` at or above `R1-BASELINE-REPAIR-SCRIPT-LINE`; `POLICY-85-OBSERVATION: MET|NOT MET` recorded for the aggregate (D9). Evidence: `FEATURE/evidence/qa-gates/ps-coverage-r1.<TS>.md`.
- [x] [P4-T5] Temporary-file audit: Grep `New-TemporaryFile|GetTempPath|GetTempFileName|TestDrive|WriteAllText|Set-Content|Out-File|New-Item` (count) over `tests/scripts/dependencies/BindingRedirectSync.Tests.ps1` and `tests/scripts/dependencies/Repair-PackageManifestConsistency.RedirectSync.Tests.ps1`; positive control, the same pattern over `scripts/dependencies/Repair-PackageManifestConsistency.ps1` (its default writer uses `WriteAllText`, line 373); write `FEATURE/evidence/qa-gates/ps-temp-file-audit-r1.<TS>.md` with `ITERATION:`. Acceptance: 0 for each test file and at least 1 for the control (0 is `STOP: SWEEP-BLIND`). Evidence: `FEATURE/evidence/qa-gates/ps-temp-file-audit-r1.<TS>.md`.
- [x] [P4-T6] Size check of the four PowerShell Write Set files with Grep `^` counts; write `FEATURE/evidence/qa-gates/ps-line-counts-r1.<TS>.md` with `ITERATION:`. Acceptance: every count at most 500; otherwise `STOP: SIZE-LIMIT`. Evidence: `FEATURE/evidence/qa-gates/ps-line-counts-r1.<TS>.md`.
- [x] [P4-T7] Write `FEATURE/evidence/qa-gates/coverage-comparison-powershell-r1.<TS>.md` from P0-T8 and the passing-iteration P4-T4: baseline and final aggregate and their delta; `BindingRedirectSync.psm1` baseline and final percent (new code, gate 90.00); `Repair-PackageManifestConsistency.ps1` baseline and final percent (changed lines, no regression); PowerShell branch coverage recorded as "not measured by Pester; no branch gate applies". Acceptance: all six numbers present and the P4-T4 gates hold. Evidence: `FEATURE/evidence/qa-gates/coverage-comparison-powershell-r1.<TS>.md`.

### Phase 5 — Scope Proofs (C# and Workflows)

- [x] [P5-T1] C# not-applicable proof (D7): run `git diff --name-only <R1-START-SHA> -- "*.cs" "*.csproj" "*.props" "*.targets" "*.sln" "*.config"` and `git status --porcelain -- "*.cs" "*.csproj" "*.props" "*.targets" "*.sln" "*.config"`; write `FEATURE/evidence/qa-gates/csharp-not-applicable-r1.<TS>.md` with both outputs, `CSHARP-TOOLCHAIN: NOT RE-RUN (no C#-family file changed in R1)`, and the cycle-0 C# QA artifact names found by Glob `FEATURE/evidence/qa-gates/cs-*.md`. Acceptance: both outputs empty and at least four cycle-0 artifact names listed; a non-empty output is `STOP: SCOPE-VIOLATION` (the C# toolchain then becomes mandatory and the caller is notified). Evidence: `FEATURE/evidence/qa-gates/csharp-not-applicable-r1.<TS>.md`.
- [x] [P5-T2] AC4 re-check for `.github/workflows`: run `git diff --name-only <BASE-SHA> -- .github/workflows` and `git status --porcelain -- .github/workflows`; write `FEATURE/evidence/qa-gates/workflows-untouched-r1.<TS>.md`. Acceptance: both outputs empty; non-empty is `STOP: SCOPE-VIOLATION`. Evidence: `FEATURE/evidence/qa-gates/workflows-untouched-r1.<TS>.md`.

### Phase 6 — Final Hygiene and Commit

- [x] [P6-T1] Write `FEATURE/evidence/other/remediation-r1-summary.<TS>.md` mapping B-1, CR-1, CR-2, CR-3, AC4-RECHECK and AC5-RECHECK to their fail-before and pass-after artifacts and QA-gate artifacts by path, stating that AC7 remains pending CI and no AC checkbox changed in this cycle, and recording the residual of D1 (workflow comment drift) as promoted to issue #986. Acceptance: six item rows, each naming at least one existing artifact path; Grep `^- \[[ x]\] AC` over `FEATURE/spec.md` shows AC1 to AC6 `[x]` and AC7 `[ ]`, unchanged from the start of the cycle. Evidence: `FEATURE/evidence/other/remediation-r1-summary.<TS>.md`.
- [x] [P6-T2] Final pre-commit identity sweep: run `985r1-identity-sweep.ps1 -WorkspaceRoot <WORKTREE> -Base <BASE-SHA> -Mode WorkingTree` from `CMDDIR` (this covers this plan, the review documents, every evidence file of this cycle and every dirty or new agent-memory file); write `FEATURE/evidence/qa-gates/identity-sweep-final-r1.<TS>.md`. Acceptance: `EXIT_CODE: 0`, `HITS-TOTAL: 0`, `CONTROL-SYNTHETIC:` all `=1`, `CONTROL-GITFILE-ACCOUNT:` at least 1. A hit is fixed per P1-T3 and this task re-run. Evidence: `FEATURE/evidence/qa-gates/identity-sweep-final-r1.<TS>.md`.
- [x] [P6-T3] Commit: flip this task's checkbox to `[x]`; run `git status --porcelain` and confirm every line is under `FEATURE/`, under `.claude/agent-memory/`, is one of the four Write Set code and test paths, or is `docs/features/potential/promoted/2026-10-09-dependabot-repair-workflow-comments-predate-redirect-sync.md` (issue #986 promoted record) (any other line is `STOP: UNEXPECTED-WORKTREE-PATH`); `git add --` those explicit paths, including `docs/features/potential/promoted/2026-10-09-dependabot-repair-workflow-comments-predate-redirect-sync.md` (never `-A`, never `.`); `git commit -m "fix(deps): remediate review 2026-10-09T14-55 for #985" -m "<body naming B-1, CR-1, CR-2, CR-3>"` plus the trailer `-m` arguments dictated by the executing session's attribution instructions. If `git add` or `git commit` fails or a hook denies it, restore the checkbox to `[ ]` and `STOP: COMMIT-BLOCKED`. Then, writing no file: `git rev-parse HEAD`, `git status --porcelain`, and `985r1-identity-sweep.ps1 -WorkspaceRoot <WORKTREE> -Base <BASE-SHA> -Mode Committed` from `CMDDIR`; report the three outputs in the executor's return message. Acceptance: commit exit 0; post-commit porcelain empty; the Committed-mode sweep prints `HITS-TOTAL: 0` with both controls passing (a non-zero count is `STOP: IDENTITY-RESIDUAL-AFTER-COMMIT`). No push. Evidence: the commit itself (`git log -1 --stat`) and the executor's return message.

## 8. Acceptance Mapping

| ID | Implementation | Tests / checks | Evidence |
|---|---|---|---|
| B-1 | P1-T2, P1-T3 | P1-T1 (fail-before), P1-T4, P6-T2, P6-T3 Committed sweep | `regression-testing/fail-before-identity-sweep.<TS>.md`, `regression-testing/pass-after-identity-sweep.<TS>.md`, `qa-gates/identity-sweep-final-r1.<TS>.md` |
| CR-1 | P3-T1 items 1, 4, 5, 6 | N1 to N4, N6, modified report test, N7 (P2-T4, P2-T5, P3-T4, P3-T5) | `regression-testing/fail-before-sync-module.<TS>.md`, `regression-testing/pass-after-sync-module.<TS>.md` |
| CR-2 | P3-T2 | N8, N9 (P2-T5, P3-T5) | `regression-testing/fail-before-repair-written-path.<TS>.md`, `regression-testing/pass-after-repair-written-path.<TS>.md` |
| CR-3 | P3-T1 items 2, 3 | N5 (P2-T4, P3-T4) | `regression-testing/fail-before-sync-module.<TS>.md`, `regression-testing/pass-after-sync-module.<TS>.md` |
| AC4-RECHECK | none (scope guard) | P5-T2 | `qa-gates/workflows-untouched-r1.<TS>.md` |
| AC5-RECHECK | Phase 4 loop | P4-T1 to P4-T7, P5-T1 | `qa-gates/ps-coverage-r1.<TS>.md`, `qa-gates/ps-temp-file-audit-r1.<TS>.md`, `qa-gates/csharp-not-applicable-r1.<TS>.md` |
