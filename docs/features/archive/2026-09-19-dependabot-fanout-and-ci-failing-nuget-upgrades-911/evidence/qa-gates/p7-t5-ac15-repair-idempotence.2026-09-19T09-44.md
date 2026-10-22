# P7-T5 — AC15: the repair pass leaves a formatting-stable tree

Timestamp: 2026-09-20T01-34

Commands:

```
pwsh -NoProfile -Command 'Set-Location "<execution-worktree-root>"; $r1 = & "<execution-worktree-root>\scripts\dependencies\Repair-PackageManifestConsistency.ps1"; <porcelain capture>; $r2 = & "<execution-worktree-root>\scripts\dependencies\Repair-PackageManifestConsistency.ps1"; <porcelain capture>'
git diff --name-only 734112ed25bba293cb074e71fee2286bc3b72fae -- "*.csproj"
git diff --stat 734112ed25bba293cb074e71fee2286bc3b72fae -- "*/packages.config" "*/app.config" "*.csproj"
dotnet tool run csharpier check .
```

EXIT_CODE: 0

## Output Summary

```
RUN1 repairs=0 elements=2559 written=0 success=True
RUN2 repairs=0 elements=2559 written=0 success=True
PORCELAIN-IDENTICAL: True
CSPROJ-PORCELAIN-1-EMPTY: True CSPROJ-PORCELAIN-2-EMPTY: True
Checked 1623 files in 4137ms.
CSHARPIER-EXIT: 0
```

Both runs were real runs, not what-if runs. Neither wrote a file.

## Acceptance conditions

| Condition | Observed | Result |
|---|---|---|
| Second run produces no change: porcelain between the runs byte-identical to porcelain after the second | `PORCELAIN-IDENTICAL: True` | PASS |
| `dotnet tool run csharpier check .` exit code | 0, `Checked 1623 files in 4137ms.` | PASS |
| Files reported with findings by CSharpier | 0 | PASS |
| Second run's report: repairs applied | 0 | PASS |
| Second run's report: elements examined | 2559, non-zero | PASS |
| `git status --porcelain --untracked-files=all -- "*.csproj"` after run 1 | empty | PASS |
| `git status --porcelain --untracked-files=all -- "*.csproj"` after run 2 | empty | PASS |
| `git diff --name-only <MERGE_BASE> -- "*.csproj"` lists exactly the 15 Write Set paths and no sixteenth | 15 paths, enumerated below | PASS |

The examined count guards the zero-repairs figure: a pass that discovered nothing would also report
zero repairs, and it would report zero examined elements with it.

## Porcelain, identical between the two runs and after the second

```
 M docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/plan.2026-09-19T09-44.md
 M docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/spec.md
?? docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/evidence/other/p6-t7-batch-c-boundary.2026-09-19T09-44.md
?? docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/evidence/qa-gates/p6-t6-commit.2026-09-19T09-44.md
?? docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/evidence/qa-gates/p7-t1-composition-root.2026-09-19T09-44.md
?? docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/evidence/qa-gates/p7-t2-repair-tests-authored.2026-09-19T09-44.md
?? docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/evidence/qa-gates/p7-t3-ac10-skip-and-proceed.2026-09-19T09-44.md
?? docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/evidence/qa-gates/p7-t4-ac5-analyzer-verifier.2026-09-19T09-44.md
?? scripts/dependencies/Repair-PackageManifestConsistency.ps1
?? tests/scripts/dependencies/Repair-PackageManifestConsistency.Tests.ps1
```

No entry names a `.csproj`, a `packages.config` or an `app.config`, which is the same fact the
type-scoped capture states directly.

## The 15 project files in the merge-base diff, enumerated

```
QuickFiler.Test/QuickFiler.Test.csproj
QuickFiler/QuickFiler.csproj
Tags.Test/Tags.Test.csproj
Tags/Tags.csproj
TaskMaster.Test/TaskMaster.Test.csproj
TaskTree.Test/TaskTree.Test.csproj
TaskTree/TaskTree.csproj
TaskVisualization.Test/TaskVisualization.Test.csproj
TaskVisualization/TaskVisualization.csproj
ToDoModel.Test/ToDoModel.Test.csproj
ToDoModel/ToDoModel.csproj
UtilitiesCS.Test/UtilitiesCS.Test.csproj
UtilitiesCS/UtilitiesCS.csproj
VBFunctions.Test/VBFunctions.Test.csproj
VBFunctions/VBFunctions.csproj
```

Fifteen paths, element-for-element the spec `## Write Set` list under "Project files carrying a
stranded analyzer item (#898)", and no sixteenth. A sixteenth would mean a folder-selection rule
had survived into the implementation: it would have rewritten the committed `roslyn5.0` and
`roslyn4.7` segments to the highest folders the restored packages ship and pulled
`TaskMaster/TaskMaster.csproj` and the Roslynator-bearing projects into the footprint. The 162
analyzer items are byte-identical after both runs, which P7-T4 measures directly as 0 disagreements
and 0 missing segments over 162 examined items.

The configuration-file diff against the merge base is unchanged by these two runs; its tail reads
`49 files changed, 1218 insertions(+), 6092 deletions(-)`, which is the one-time normalisation
P1-T7 performed plus the #898 correction P1-T9 performed, neither of them produced here.

## Finding recorded rather than repaired: stale binding redirects predating this branch

The composition root reconciles a binding redirect only for a package the run upgraded. With no
candidate upgrade supplied, as in these two runs, no redirect is rewritten. That scoping is a
design decision taken in P7-T1 and it is recorded here because a measurement taken while choosing
it found real drift that this pass therefore leaves in place:

| Application configuration | Assembly | Redirect declares | Restored assembly and project reference declare |
|---|---|---|---|
| `QuickFiler/app.config` | `Microsoft.Bcl.Memory` | 10.0.0.11 | 10.0.0.12 |
| `SVGControl/app.config` | `Fizzler` | lower than 1.3.1.0 | 1.3.1.0 |
| `SVGControl/app.config` | `System.Runtime.CompilerServices.Unsafe` | lower than 6.0.3.0 | 6.0.3.0 |
| `SVGControl.Test/app.config` | `MSTest.TestFramework` | lower than 4.4.0.0 | 4.4.0.0 |
| `ToDoModel/app.config` | `Microsoft.Bcl.Memory` | 10.0.0.11 | 10.0.0.12 |
| `UtilitiesCS/app.config` | `AngleSharp` | 1.7.1.0 | 1.8.1.0 |
| `UtilitiesCS/app.config` | `Microsoft.Bcl.Memory` | 10.0.0.11 | 10.0.0.12 |
| `UtilitiesCS/app.config` | `Microsoft.Bcl.Numerics` | 10.0.0.11 | 10.0.0.12 |
| `UtilitiesCS/app.config` | `Microsoft.Extensions.Diagnostics.Abstractions` | 10.0.0.11 | 10.0.0.12 |
| `UtilitiesCS.Test/app.config` | `Microsoft.Bcl.Memory` | 10.0.0.11 | 10.0.0.12 |

Ten redirects across six files. The drift pre-dates this branch:
`git show 734112ed25bba293cb074e71fee2286bc3b72fae:UtilitiesCS/app.config` carries
`<bindingRedirect oldVersion="0.0.0.0-1.7.1.0" newVersion="1.7.1.0" />` for `AngleSharp` while the
manifest at that commit already declared 1.8.1. It is reported to the coordinator for a follow-up
issue rather than repaired here, because rewriting a redirect for a package this run did not
upgrade is a behaviour change outside the upgrade the pass was asked to repair, and because the six
`app.config` files are outside every commit pathspec Phase 7 authorises.

This task checks off **AC15** in
`docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/spec.md`.
