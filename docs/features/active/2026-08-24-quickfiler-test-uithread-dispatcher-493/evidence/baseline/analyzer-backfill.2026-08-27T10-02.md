# Analyzer Version Back-Fill (P0-T6)

Timestamp: 2026-08-27T10-02
Task: [P0-T6]
Command: `git -C $WS rev-parse --git-common-dir`; then `Copy-Item -Recurse -Force <main-checkout>/packages/Meziantou.Analyzer.3.0.156 packages/Meziantou.Analyzer.3.0.156` and `Copy-Item -Recurse -Force <main-checkout>/packages/Roslynator.Analyzers.4.16.0 packages/Roslynator.Analyzers.4.16.0`
EXIT_CODE: 0
Output Summary: `QuickFiler.Test/packages.config` pins `Meziantou.Analyzer` at 3.0.174 and
`Roslynator.Analyzers` at 4.16.1, but the project's `<Analyzer Include>` items name 3.0.156 and
4.16.0, so `P0-T5`'s restore left both skewed versions absent. Both package folders were copied from
the main checkout resolved via `git rev-parse --git-common-dir`. All five DLL paths named by the
`<Analyzer Include>` items for those two packages now exist. `nuget.exe install` was not needed
because both folders were present in the main checkout.

## Skew confirmed before the copy

| Package id | Version pinned by `QuickFiler.Test/packages.config` | Version named by `<Analyzer Include>` | Present in worktree `packages/` after P0-T5 |
| --- | --- | --- | --- |
| `Meziantou.Analyzer` | 3.0.174 | 3.0.156 | 3.0.174 only |
| `Roslynator.Analyzers` | 4.16.1 | 4.16.0 | 4.16.1 only |

Without the back-fill, compilation fails with `error CS0006` on the two missing analyzer assemblies,
which kills every project in the solution rather than only the analyzer gate.

## Main checkout resolution

`git -C $WS rev-parse --git-common-dir` returned an absolute path ending in `TaskMaster/.git`. The
main checkout is that path's parent directory. Both source folders were present there, so the
`nuget.exe install` fallback branch of the task was not taken.

## Acceptance verification — five DLL paths

Each path is repo-relative to `<repo-root>` and was checked for existence after the copy.

| # | Repo-relative path | Exists |
| --- | --- | --- |
| 1 | `packages/Meziantou.Analyzer.3.0.156/analyzers/dotnet/roslyn5.0/cs/Meziantou.Analyzer.dll` | True |
| 2 | `packages/Roslynator.Analyzers.4.16.0/analyzers/dotnet/roslyn4.7/cs/Roslynator.CSharp.Analyzers.dll` | True |
| 3 | `packages/Roslynator.Analyzers.4.16.0/analyzers/dotnet/roslyn4.7/cs/Roslynator_Analyzers_Roslynator.Common.dll` | True |
| 4 | `packages/Roslynator.Analyzers.4.16.0/analyzers/dotnet/roslyn4.7/cs/Roslynator_Analyzers_Roslynator.Core.dll` | True |
| 5 | `packages/Roslynator.Analyzers.4.16.0/analyzers/dotnet/roslyn4.7/cs/Roslynator_Analyzers_Roslynator.CSharp.dll` | True |

## Recorded discrepancy — plan line citation is stale

The task text locates the five `<Analyzer Include>` items at
`QuickFiler.Test/QuickFiler.Test.csproj` lines 466-470. Those line numbers do not hold in the tree
at `BASE_SHA` `125c36b0669d9dd6095f156901bba138e2272f56`: lines 466-470 are
`EnsureNuGetPackageBuildImports` `<Error Condition=...>` elements. The five items the task means are
at lines 480-484, verified with a line-numbered search for the literal `Analyzer Include`. The set of
five is unambiguous regardless of the citation, because the two packages the task names contribute
exactly five `<Analyzer Include>` DLL paths between them (one Meziantou, four Roslynator), and no
other `<Analyzer Include>` item in the file references either package. The existence check above was
therefore performed against the five paths identified by package identity, not by the stale line
range. No file was modified by this task.
