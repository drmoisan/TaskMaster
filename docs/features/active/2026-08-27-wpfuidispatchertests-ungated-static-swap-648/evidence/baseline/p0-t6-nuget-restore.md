# P0-T6 — Restore NuGet Packages

Timestamp: 2026-09-01T13-29

Command: `pwsh -NoProfile -File scripts/vscode/Invoke-Restore.ps1` (run from the checkout root)

EXIT_CODE: 0

Output Summary:

`packages/` was absent from this checkout before the run. Every project declares an
`EnsureNuGetPackageBuildImports` target that raises an `<Error>` at
`BeforeTargets="PrepareForBuild"`; that target is at `QuickFiler.Test/QuickFiler.Test.csproj:481`,
confirmed by reading lines 478 through 484 of that file before running the restore. No build could
succeed until this task completed.

The wrapper runs MSBuild `/t:Restore` with `/p:RestorePackagesConfig=true`
(`scripts/vscode/Invoke-Restore.ps1:36`), so it restores both `packages.config` and
`PackageReference` projects. Its closing output was:

```
         Installed:
             172 package(s) to packages.config projects
     1>Done Building Project "<checkout-root>\TaskMaster.sln" (Restore target(s)).

Build succeeded.
    0 Warning(s)
    0 Error(s)

Time Elapsed 00:00:02.54
```

The absolute path MSBuild printed named this checkout's `TaskMaster.sln`; it is elided here so that
no machine-specific absolute path is written into an artifact.

After the run, `ls -d packages/MSTest.Analyzers.4.3.3` resolved to `packages/MSTest.Analyzers.4.3.3/`,
which is the directory the acceptance condition names.
