# Phase 0 — Analyzer Package Back-fill ([P0-T7])

Timestamp: 2026-08-28T05-11

Command:
`nuget install Meziantou.Analyzer -Version 3.0.156 -OutputDirectory packages -DependencyVersion Ignore`
and
`nuget install Roslynator.Analyzers -Version 4.16.0 -OutputDirectory packages -DependencyVersion Ignore`,
run from the worktree root during worktree bootstrap, immediately before plan execution began.
EXIT_CODE: 0

## Acceptance checks

| Required path | Exists | Size |
| --- | --- | --- |
| `packages\Meziantou.Analyzer.3.0.156\analyzers\dotnet\roslyn5.0\cs\Meziantou.Analyzer.dll` | yes | 2,749,952 bytes |
| `packages\Roslynator.Analyzers.4.16.0\analyzers\dotnet\roslyn4.7\cs\Roslynator.CSharp.Analyzers.dll` | yes | 382,464 bytes |

Both required analyzer assemblies are present. A missing `Analyzer Include` path is compile error
**CS0006**, not a warning, so this back-fill is a precondition of every build gate in this plan rather
than a quality improvement.

## DOCUMENTED DEVIATION — corrected file and line citation

This task's text cites the skewed `Analyzer Include` items at
`QuickFiler.Test/QuickFiler.Test.csproj:467-471`. **That line range is stale.** It was resolved
independently against the current tree with
`grep -rn "Meziantou.Analyzer.3.0.156\|Roslynator.Analyzers.4.16.0" --include=*.csproj .`, per the
plan's own standing instruction to resolve every citation by the entry it accompanies rather than by
line number.

The corrected finding has two parts.

**Part 1 — the file named by the task is correct; only the line range drifted.**
`QuickFiler.Test/QuickFiler.Test.csproj` does carry the five skewed items, at lines **493-497**, not
467-471. Lines 463-466 hold a *different* analyzer `ItemGroup` containing the MSTest.Analyzers 4.3.3
and SonarAnalyzer.CSharp 10.32.0.713 items, which are not skewed. The project has two analyzer item
groups, and the stale citation lands on the wrong one.

**Part 2 — the skew is repository-wide, not confined to one project.** Fifteen project files carry
the same five pinned items:

`QuickFiler/QuickFiler.csproj:586-590`, `QuickFiler.Test/QuickFiler.Test.csproj:493-497`,
`Tags/Tags.csproj:97-101`, `Tags.Test/Tags.Test.csproj:314-318`,
`TaskMaster/TaskMaster.csproj:571-575`, `TaskMaster.Test/TaskMaster.Test.csproj:379-383`,
`TaskTree/TaskTree.csproj:100-104`, `TaskTree.Test/TaskTree.Test.csproj:315-319`,
`TaskVisualization/TaskVisualization.csproj:150-154`,
`TaskVisualization.Test/TaskVisualization.Test.csproj:339-343`,
`ToDoModel/ToDoModel.csproj:189-193`, `ToDoModel.Test/ToDoModel.Test.csproj:357-361`,
`UtilitiesCS/UtilitiesCS.csproj:1303-1307`, `UtilitiesCS.Test/UtilitiesCS.Test.csproj:962-966`,
`VBFunctions/VBFunctions.csproj:58-62`, `VBFunctions.Test/VBFunctions.Test.csproj:294-298`.

Because the gate commands in this plan build `TaskMaster.sln` rather than a single project, the
back-fill has to satisfy every one of those projects, not only `QuickFiler.Test`. It does: the two
package directories are shared and are referenced by the same relative `..\packages\...` path from
every project.

## Root cause of the skew, and the justification for back-filling rather than editing

`packages.config` pins the **newer** versions:

```
QuickFiler.Test/packages.config:  <package id="Meziantou.Analyzer"   version="3.0.174" targetFramework="net481" developmentDependency="true" />
QuickFiler.Test/packages.config:  <package id="Roslynator.Analyzers" version="4.16.1" targetFramework="net481" developmentDependency="true" />
```

`[P0-T6]`'s restore therefore materialises `packages\Meziantou.Analyzer.3.0.174` and
`packages\Roslynator.Analyzers.4.16.1`, while the `<Analyzer Include>` items in all fifteen `.csproj`
files continue to name `3.0.156` and `4.16.0`. The two sets do not intersect, so every referenced
analyzer path is missing after a plain restore, and each affected project emits **five `CS0006`
errors** — one per `Analyzer Include` item.

The remedy applied is to install the two pinned versions **alongside** the restored ones, leaving all
four package directories present:

```
packages/Meziantou.Analyzer.3.0.156/
packages/Meziantou.Analyzer.3.0.174/
packages/Roslynator.Analyzers.4.16.0/
packages/Roslynator.Analyzers.4.16.1/
```

**No `.csproj` was edited.** Editing the fifteen project files to point at the restored versions would
have been a repository-wide change to files this feature does not own, in direct conflict with
constraint C1, and would have produced a diff that the changed-file-set criterion `[P7-T2]` checks and
`[P7-T10]` flips must reject. Editing `packages.config` instead would have the same ownership problem
and would additionally change what the solution restores for every other agent worktree. Back-filling
the packages is inert with respect to tracked files: `packages/` is not tracked, so this remedy
produces no entry in `git status --porcelain`, which `[P0-T3]` independently confirmed.

Output Summary: Both back-filled analyzer assemblies exist at the exact paths the fifteen `.csproj`
files name, so the CS0006 condition is cleared and the build gates can run. The task's citation
`QuickFiler.Test/QuickFiler.Test.csproj:467-471` is recorded here as a documented deviation: the file
is correct but the items sit at lines 493-497, and the same five items appear in fourteen further
project files. No `.csproj` and no `packages.config` was modified.
