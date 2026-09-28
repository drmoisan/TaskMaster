# P3-T4 — `scripts/vscode/Sync-PackageReferences.ps1` rewritten

Timestamp: 2026-09-20T00-04

Commands:

```
pwsh -NoProfile -Command 'Set-Location "<execution-worktree-root>"; $p = (Resolve-Path "scripts/vscode/Sync-PackageReferences.ps1").Path; "LINECOUNT=" + ([System.IO.File]::ReadAllLines($p)).Count; "TFM_HITS=" + @(Select-String -Path $p -Pattern "tfmPreference").Count; "NS21_HITS=" + @(Select-String -Path $p -Pattern "netstandard2\.1").Count; "IMPORT_STATEMENTS=" + @(Select-String -Path $p -Pattern "^\s*Import-Module.*PackageCompatibility\.psm1").Count'

git -C <W> diff --name-only 734112ed25bba293cb074e71fee2286bc3b72fae -- scripts/vscode/Invoke-VSBuild.ps1
git -C <W> status --porcelain --untracked-files=all -- scripts/vscode
git -C <W> diff --numstat 734112ed25bba293cb074e71fee2286bc3b72fae -- scripts/vscode/
```

EXIT_CODE: 0

The file was rewritten with the `Write` tool and amended with the `Edit` tool. No heredoc, no shell
redirection and no `sed` was used, per gate rule 15 and Scope Decision 4.

## Verbatim measurement output

```
LINECOUNT=423
TFM_HITS=0
NS21_HITS=0
IMPORT_STATEMENTS=1
MENTION_LINES=2
  line 13: delegated to scripts/dependencies/PackageCompatibility.psm1, which excludes
  line 36: Import-Module (Join-Path $PSScriptRoot '..\dependencies\PackageCompatibility.psm1') -Force
MODULE_CALL_LINES=2
  line 162: Select-CompatibleAssetFolder. A framework the target cannot consume is therefore
  line 190: return [string](Select-CompatibleAssetFolder -AssetFolder $offered)
```

## The two zero counts, and the positive assertions that guard them

`tfmPreference` returns **0** lines and `netstandard2.1` returns **0** lines. Neither is an
unguarded absence. Both are guarded by the positive import assertion the task requires and by two
further positive observations:

| Positive guard | Value |
|---|---|
| `Import-Module` statements naming `PackageCompatibility.psm1` | **1**, at line 36 |
| Call sites of `Select-CompatibleAssetFolder` in executable code | **1**, at line 190, inside `Resolve-PackageAssetFolder` |
| Function definitions in the rewritten file | **8**, enumerated below |
| `git diff --numstat` line total for the file | **407 added, 143 deleted** |

A measurement that resolved no file would report 0 for the two prohibited tokens **and** 0 for the
import, the call site, the function count and the numstat total, so the clean result and the
vacuous one are distinguishable. Per gate rule 15 the numstat line total is recorded rather than a
changed-file count, because a pure line-ending rewrite produces a changed-file count of 1 with no
content change at all; 407 added and 143 deleted against a 159-line original is a content rewrite.

## The deleted ordering and what replaced it

The merge-base file carried `$tfmPreference` at lines 14-19: a 16-member ordered array ending
`'netstandard2.1', 'netstandard2.0'`, which ranked the unconsumable framework second-to-last rather
than excluding it. That array is gone. The rewritten file declares no framework name of any kind in
any collection; the only framework literals that remain are `net481` and `net48` inside the
`.EXAMPLE` and `.DESCRIPTION` prose of the compatibility module, not in this file.

Selection now reaches the shared module by exactly one route. `Resolve-PackageAssetFolder`
enumerates the asset folders the restored package actually ships through the injected seam,
narrows them to those containing the required file, and hands the resulting set to
`Select-CompatibleAssetFolder`. There is no fallback path, no second ordering and no local
tie-break, so a framework the module excludes cannot be selected by this script under any input.

## Function structure and the injectable filesystem seam

Eight advanced functions, each with `CmdletBinding()`:

| Line | Function | Role |
|---|---|---|
| 43 | `Get-PackageSyncSeam` | returns the delegate table; the only place in the file that calls a filesystem cmdlet or a reflection API |
| 96 | `Get-PackageVersionMap` | manifest text to identifier/version lookup, parsed through `PackageGraph` |
| 122 | `Resolve-ManifestPackageId` | pure; identifies which declared package a restore folder belongs to |
| 154 | `Resolve-PackageAssetFolder` | asset-folder selection, delegated to the compatibility module |
| 193 | `Get-HintPathRepair` | produces one repair record per unresolved hint path |
| 265 | `Repair-ProjectReferenceVersion` | pure over text; rewrites the reference `Version=` attribute |
| 299 | `Invoke-ProjectReferenceSync` | applies the repairs for one project; declares `SupportsShouldProcess` and calls `ShouldProcess` before writing |
| 370 | `Invoke-PackageReferenceSync` | entry point; accepts `-SolutionRoot` and an optional `-Seam` |

The seam is a hashtable of seven scriptblocks — `ListManifestPath`, `ListProjectPath`,
`ListAssetFolder`, `TestPath`, `ReadText`, `WriteText`, `ReadAssemblyIdentity`. Every function that
needs the outside world takes the table as a parameter, so the whole repair path is exercisable in
memory with an injected table and creates no temporary file.

The file ends with the repository's standard invocation guard,
`if ($MyInvocation.InvocationName -ne '.') { $null = Invoke-PackageReferenceSync @PSBoundParameters }`,
so dot-sourcing the file for test defines the functions and performs no work. The script parameter
block still declares `-SolutionRoot`, which is the contract
`scripts/vscode/Invoke-VSBuild.ps1` line 168 invokes it by.

## End-to-end behavioural check with an injected seam

The entry point was driven against an in-memory seam: one manifest declaring
`Contoso.Widgets 2.0.0`, one project file whose hint path points at
`..\packages\Contoso.Widgets.1.0.0\lib\net45\Contoso.Widgets.dll`, and a restored package offering
the asset folders `netstandard2.1`, `net45` and `net472`.

```
DOTSOURCE=ok
  [Proj] Fixed 1 broken HintPath(s)
Sync-PackageReferences: Fixed 1 HintPath(s) total
EXAMINED=1 FIXED=1
WROTE C:\fake\Proj\Proj.csproj
  <Project><ItemGroup><Reference Include="Contoso.Widgets, Version=2.0.0.0"><HintPath>..\packages\Contoso.Widgets.2.0.0\lib\net472\Contoso.Widgets.dll</HintPath></Reference></ItemGroup></Project>
```

The repaired path names `net472`. `netstandard2.1` was offered and was not selected, and `net45`
was offered and was passed over in favour of the more preferred consumable folder. No path on disk
was read and none was written; the write landed in the in-memory table. This is a behavioural
smoke check, not the acceptance for AC7 — that is P3-T6.

## `scripts/vscode/Invoke-VSBuild.ps1` is unchanged

```
$ git diff --name-only 734112ed25bba293cb074e71fee2286bc3b72fae -- scripts/vscode/Invoke-VSBuild.ps1
<no output>
```

Anchored to `MERGE_BASE`, as the plan's diff-anchor rule requires, because the file exists at the
base. The empty output is paired with its porcelain companion per gate rule 8, which shows the one
file this task did change and does not show `Invoke-VSBuild.ps1`:

```
$ git status --porcelain --untracked-files=all -- scripts/vscode
 M scripts/vscode/Sync-PackageReferences.ps1
```

The two are complementary: the anchored diff enumerates tracked changes and is blind to a newly
created file, while porcelain goes empty once the change is committed.

## Analyzer state of the rewritten file

```
scripts/vscode/Sync-PackageReferences.ps1 FINDINGS=0
```

Recorded as context; the analyzer gate is P4-T2. The three `PSAvoidUsingWriteHost` findings the
baseline recorded at lines 150, 154 and 157 of this file are gone: the rewrite uses
`Write-Information ... -InformationAction Continue` for its two status lines and `Write-Warning`
for its two diagnostics, and contains **0** occurrences of `Write-Host`. That is what makes P4-T2
expect the repository total to fall from 16 to 13.

The file was additionally checked for non-ASCII bytes and reports **0**, matching the
`PackageGraph` precedent in the same change; a non-ASCII byte without a byte-order mark raises
`PSUseBOMForUnicodeEncodedFile`, which was observed and cleared on the two `PackageCompatibility`
files during this phase.

## Acceptance evaluation

| Clause | Required | Measured | Verdict |
|---|---|---|---|
| Lines matching `tfmPreference` | exactly 0 | 0 | PASS |
| Lines matching `netstandard2.1` | exactly 0 | 0 | PASS |
| Imports of `PackageCompatibility.psm1` | at least 1 | 1 `Import-Module` statement at line 36 | PASS |
| File length | at most 500 lines | 423 | PASS |
| `scripts/vscode/Invoke-VSBuild.ps1` unchanged | empty anchored diff | empty, with porcelain companion listing only `Sync-PackageReferences.ps1` | PASS |
| Framework selection resolved through the module | at least one call site | 1 executable call site at line 190; no alternative selection path exists | PASS |

Output Summary: `scripts/vscode/Sync-PackageReferences.ps1` was rewritten from 159 to **423** lines,
407 added and 143 deleted against `MERGE_BASE`. The `$tfmPreference` array formerly at lines 14-19
is deleted: the file now reports **0** lines matching `tfmPreference` and **0** matching
`netstandard2.1`, guarded by **1** `Import-Module` of `PackageCompatibility.psm1` at line 36 and
**1** executable call of `Select-CompatibleAssetFolder` at line 190, which is the script's only
framework-selection route. The script is restructured into **8** advanced functions with a
seven-member injectable filesystem seam and the repository's standard dot-source guard, and an
end-to-end run against an in-memory seam repaired a hint path to `net472` while passing over the
offered `netstandard2.1`. `scripts/vscode/Invoke-VSBuild.ps1` is unchanged against `MERGE_BASE`,
with the porcelain companion listing only the one modified file. PSScriptAnalyzer reports **0**
findings on the rewritten file, removing the three baseline `PSAvoidUsingWriteHost` findings.
