# P5-T17 — AC8 orphaned hint paths, both directions

Timestamp: 2026-09-19T09-44

## Command 1 — the in-memory AC8 cases

```
pwsh -NoProfile -Command 'Set-Location "<execution-worktree-root>"; Import-Module Pester -RequiredVersion 5.6.1; $c = New-PesterConfiguration; $c.Run.Path = @("tests/scripts/dependencies/ProjectConsistency.Tests.ps1"); $c.Filter.FullName = "*AC8-*"; $c.Run.PassThru = $true; $c.Output.Verbosity = "Detailed"; $c.CodeCoverage.Enabled = $true; $c.CodeCoverage.Path = @("scripts/dependencies","scripts/vscode"); $c.CodeCoverage.OutputFormat = "JaCoCo"; $c.CodeCoverage.OutputPath = "coverage/p5-t17-ac8-coverage.xml"; $r = Invoke-Pester -Configuration $c; "PESTER Passed=$($r.PassedCount) Failed=$($r.FailedCount) Skipped=$($r.SkippedCount) Total=$($r.TotalCount)"; if ($r.FailedCount -gt 0) { exit 1 } else { exit 0 }'
```

EXIT_CODE: 0

```
PESTER Passed=2 Failed=0 Skipped=0 Total=13
EXECUTED=2
NOTRUN=11
PASSED: AC8- reports a non-empty orphan set for the issue 903 pre-fix pair
PASSED: AC8- reports no orphan and a non-zero examined count when the manifest declares every hint path
```

## Command 2 — the live verifier invocation over the real pair

```
pwsh -NoProfile -Command 'Set-Location "<execution-worktree-root>"; Import-Module "<execution-worktree-root>\scripts\dependencies\ConsistencyVerifier.psm1" -Force -ErrorAction Stop; $project = [System.IO.File]::ReadAllText("<execution-worktree-root>\ToDoModel.Test\ToDoModel.Test.csproj"); $manifest = [System.IO.File]::ReadAllText("<execution-worktree-root>\ToDoModel.Test\packages.config"); $d = Find-OrphanedHintPath -ProjectText $project -ManifestText $manifest; Get-ExaminedElementCount -ProjectText $project'
```

EXIT_CODE: 0

```
LIVE_ORPHANS=0
LIVE_HINTPATH_EXAMINED=65
LIVE_ELEMENTS Import=13 Error=10 Reference=84 HintPath=65 Analyzer=11
```

Both file paths are absolute. `[System.IO.File]` resolves a relative path against the
**process** current directory rather than the PowerShell location, so a relative path here
would have read the session worktree's copy of these files and reported on a repository
this plan is not changing. That is gate rule 16's failure shape reaching a .NET API, and it
was observed once in this session before the paths were made absolute.

## Output Summary

The AC8 detector fires in one direction and stays silent in the other, and the live
invocation over `ToDoModel.Test` examines 65 hint paths and reports 0 orphans.

## Acceptance

| Clause | Required | Measured |
|---|---|---|
| `EXIT_CODE` | 0 | 0 |
| `Failed` | 0 | 0 |
| `Total` | at least 2 | 2 executed (13 discovered) |
| In-memory case reports a non-empty orphan set for the pre-fix pair | non-empty | 2 findings, naming `Deedle.3.0.0` and `FSharp.Core.11.0.100` |
| Live verifier reports orphaned `<HintPath>` entries for `ToDoModel.Test` | exactly 0 | 0 |
| Live verifier reports a non-zero count of `<HintPath>` entries examined | > 0 | 65 |

Both directions are asserted and the examined count guards the zero. A detector that
matched nothing would report 0 orphans **and** 0 examined, and would fail this criterion
on the second clause; it reports 65 examined.

## Why the live direction is now clean

`ToDoModel.Test/ToDoModel.Test.csproj` lines 93 and 96 carry `<HintPath>` entries for
`Deedle.3.0.0` and `FSharp.Core.11.0.100`. Those were the #903 orphan pair: the sibling
manifest declared neither. P1-T11 added both entries to
`ToDoModel.Test/packages.config`, so every one of the 65 hint paths now has a declaring
manifest entry and the live orphan count is 0. The in-memory case preserves the pre-fix
state so the detector is still demonstrated firing.

## Coverage document

`coverage/p5-t17-ac8-coverage.xml`, under the gitignored `coverage/` tree. This task
records no coverage figure.

**This task checks off AC8** in
`docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/spec.md`.
