# Phase 0 — Analyzer Version Back-fill

Timestamp: 2026-08-26T08-30
Task: [P0-T7]

## Skew observed after `[P0-T6]` restore

Command: `ls -d packages/Meziantou.Analyzer.* packages/Roslynator.Analyzers.*`
EXIT_CODE: 0

```
packages/Meziantou.Analyzer.3.0.174/
packages/Roslynator.Analyzers.4.16.1/
```

`QuickFiler.Test/QuickFiler.Test.csproj:466-470` carries **unconditional** `Analyzer Include` items naming
`Meziantou.Analyzer.3.0.156` and `Roslynator.Analyzers.4.16.0`. Restore installed `3.0.174` and `4.16.1`
instead, so the two named analyzer paths did not exist. An unconditional `Analyzer Include` pointing at a
missing file is compile error **CS0006**, not a warning, so the build would fail before any analyzer ran.

No `.csproj` edit is permitted by constraint C1, so the fix is to add the two missing versions to the
untracked `packages/` tree.

## Back-fill

Command: `cp -r "<primary-checkout>/packages/Meziantou.Analyzer.3.0.156" packages/`
EXIT_CODE: 0

Command: `cp -r "<primary-checkout>/packages/Roslynator.Analyzers.4.16.0" packages/`
EXIT_CODE: 0

The two package folders were copied from the primary checkout's `packages` directory, which is the
alternative the task text authorizes alongside `nuget install <id> -Version <version> -OutputDirectory packages`.

## Acceptance check

Command: `ls packages/Meziantou.Analyzer.3.0.156/analyzers/dotnet/roslyn5.0/cs/Meziantou.Analyzer.dll packages/Roslynator.Analyzers.4.16.0/analyzers/dotnet/roslyn4.7/cs/Roslynator.CSharp.Analyzers.dll`
EXIT_CODE: 0

```
packages/Meziantou.Analyzer.3.0.156/analyzers/dotnet/roslyn5.0/cs/Meziantou.Analyzer.dll
packages/Roslynator.Analyzers.4.16.0/analyzers/dotnet/roslyn4.7/cs/Roslynator.CSharp.Analyzers.dll
```

Both required analyzer assemblies exist.

## Working-tree impact

Command: `git status --porcelain -- packages`
EXIT_CODE: 0

```
(no output)
```

`packages/` is ignored, so the back-fill adds nothing to the tracked changed-file set. No `.csproj`,
`.props`, or `.targets` file was edited.

Output Summary: The two back-filled analyzer package versions named by unconditional `Analyzer Include`
items in `QuickFiler.Test.csproj` are now present on disk. No project file was modified and the working
tree is unaffected.
