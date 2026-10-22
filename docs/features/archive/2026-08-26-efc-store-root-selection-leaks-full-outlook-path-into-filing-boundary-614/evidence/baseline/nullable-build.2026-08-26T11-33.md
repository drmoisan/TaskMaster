# [P0-T8] Baseline Nullable / Type-Check Gate

Timestamp: 2026-08-26T11-33
Task: [P0-T8]
Issue: #614

Command: `& "C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe" TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`
Working directory: `<repo-root>`
Shell: `pwsh -NoProfile`
EXIT_CODE: 0

`/p:Nullable=enable` was NOT added (this command is character-for-character the CI step in
`.github/workflows/ci.yml`; adding the flag conscripts every unannotated file and is red on main by
construction). `/t:Build` was NOT substituted for `/t:Rebuild`.

## Result counts

- `5 Warning(s)`
- `0 Error(s)`
- Time Elapsed 00:00:28.41

## Non-vacuity

18 projects entered the `Rebuild` target; the log contains CoreCompile target entries and csc
references, proving compilation and nullable-flow analysis actually ran rather than being skipped
by MSBuild incrementality.

## Warnings observed

The same 5 pre-existing `System.Reactive.PackagesConfigCheck.targets(31,5)` packages.config
warnings recorded in the P0-T7 artifact. These originate from a NuGet package's own `.targets`
file, are not compiler or nullable diagnostics, and are not promoted to errors by
`/p:TreatWarningsAsErrors=true` (MSBuild-target warnings without a warning code are not subject to
`TreatWarningsAsErrors`, which is a csc property). Zero CS86xx nullable diagnostics were emitted.

Output Summary: Baseline nullable/type-check gate PASSES with EXIT_CODE 0 and 0 errors. The 5
warnings are the same pre-existing System.Reactive packages.config messages seen in P0-T7. This
establishes that any CS86xx introduced by this change in the three `#nullable enable` files
(`BreadcrumbBridgeRouter.cs`, `EmailFilerConfig.cs`, `FolderConverter.cs`) will be a hard error at
the P9-T3 gate.
