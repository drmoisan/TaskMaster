# New PowerShell code coverage (P6-T5)

Timestamp: 2026-09-14T19-58

Source document: `coverage/pester-coverage.xml`, written by the P6-T4 run.

## Selector used

The document offers named `method` elements, as the P0-T9 artifact recorded, so the selector recorded there is the one used and **no corrected per-function expression was required**:

```
@($j.SelectNodes("//method")) | Where-Object { $_.GetAttribute("name") -eq "<FunctionName>" }
```

and then, on the matched element, the LINE `counter` child:

```
@($matched.counter) | Where-Object { $_.type -eq "LINE" }
```

The enclosing `class` element's `sourcefilename` attribute identifies the production file each method belongs to.

## Per-method LINE counters

| Method | Production file | Declared at | LINE covered | LINE missed |
| --- | --- | --- | --- | --- |
| `Assert-CoberturaBranchCoverageThreshold` | `Invoke-MSTestWithCoverage.Threshold.ps1` | 91 | 18 | 0 |
| `Invoke-VSBuildMain` | `Invoke-VSBuild.ps1` | 228 | 20 | 0 |
| `Get-MSBuildPath` | `Invoke-VSBuild.ps1` | 144 | 2 | 0 |
| `Invoke-SyncPackageReferences` | `Invoke-VSBuild.ps1` | 168 | 0 | 1 |
| `Invoke-MSBuildExe` | `Invoke-VSBuild.ps1` | 190 | 1 | 0 |
| `Invoke-RestoreMain` | `Invoke-Restore.ps1` | 84 | 16 | 0 |
| `Get-RestoreMSBuildPath` | `Invoke-Restore.ps1` | 31 | 2 | 0 |
| `Invoke-RestoreMSBuildExe` | `Invoke-Restore.ps1` | 54 | 1 | 0 |

Every one of the eight identifiers was found; none reported `NOT FOUND`.

## Aggregate

- Aggregate LINE covered: **60**
- Aggregate LINE total: **61**
- **Aggregate percentage: 98.36**

98.36 is at least 90, so the new-code floor is met and the authorized `Decision: NEW-CODE FLOOR NOT MET` branch did not fire. No further test case was written under that branch, no batch-4 budget reset was performed under it, and no re-measurement was required.

## The one uncovered line

`Invoke-SyncPackageReferences` reports 0 covered of 1. Its body is a single statement that invokes the package-reference sync script through the call operator. Every `Invoke-VSBuildMain` scenario mocks that seam, which is the whole point of the seam: executing it for real would run `Sync-PackageReferences.ps1` against the repository and reintroduce the non-deterministic measurement this delivery removes. Unlike `Get-MSBuildPath` and `Invoke-MSBuildExe`, it is not driven by a direct wrapper-seam case, because the two direct seam cases P3-T1 specifies cover those two seams only. Driving this seam against an in-process stand-in would be possible, but P3-T1 fixes the wrapper-seam case list at exactly two and this task's aggregate already clears the floor by 8.36 points, so no case was added beyond the plan.

## Guard-body invocation statements, named as a separate line

The guard-body invocation statement in each restructured file is uncovered by construction, because it never runs while the file is dot-sourced:

- `Invoke-VSBuildMain @PSBoundParameters`, inside the `if ($MyInvocation.InvocationName -ne '.')` guard at the foot of `scripts/vscode/Invoke-VSBuild.ps1`;
- `Invoke-RestoreMain @PSBoundParameters`, inside the equivalent guard at the foot of `scripts/vscode/Invoke-Restore.ps1`.

**Two such statements exist, and neither is in the denominator of the aggregate above.** Both fall inside their file's `<script>` method entry rather than inside any of the eight named methods, so they are counted separately. The document confirms this: the `<script>` entry of `Invoke-VSBuild.ps1` reads covered 3, missed 1, and the `<script>` entry of `Invoke-Restore.ps1` reads covered 3, missed 1; in each case the single missed line is the guard-body invocation. This is the same permanently-uncovered shape the repository already accepts for `Invoke-MSTest.ps1`, `Invoke-MSTestWithCoverage.ps1` and `Install-RepoDotNetSdk.ps1`, and which `Get-CoberturaFirstPartyCoverageReport`'s own docstring names as a known property.

Output Summary: the eight functions this delivery adds reach 60 covered LINE entries of 61, which is 98.36 percent, against a 90 percent new-code floor. The single uncovered line is the body of the deliberately-mocked package-reference sync seam. Two guard-body invocation statements are uncovered by construction and sit outside this aggregate's denominator.
