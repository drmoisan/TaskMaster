# Phase 0 — NuGet Package Restore

Timestamp: 2026-09-13T05-02
Task: [P0-T4]

Command: pwsh -File scripts/vscode/Invoke-Restore.ps1
EXIT_CODE: 0
PackagesDirectoryPresent: True
InstalledPackageCount: 172

Output Summary: the restore succeeded. The script resolved MSBuild through vswhere and invoked the
solution's Restore target with `/p:RestorePackagesConfig=true`, which is the property that reaches the
legacy packages.config projects as well as any PackageReference project. NuGet printed an `Installed:`
block whose value line reads `172 package(s) to packages.config projects`, and MSBuild printed
`Build succeeded.` with `0 Warning(s)` and `0 Error(s)`, followed by `Time Elapsed 00:00:02.07`.

The observation is not the exit code alone. Before this task the worktree had no repository-root
packages directory; after it the directory exists and contains 172 package sub-directories, a count
that agrees exactly with the installed count NuGet printed. Both figures were read after the run:

```
PackagesDirectoryPresent: True
PackageFolderCount: 172
```

## Why This Task Precedes P0-T6

An unbootstrapped worktree produces CS0006 reference errors in the analyzer and nullable rebuild gates,
because the legacy projects reference assembly HintPaths under the packages directory. Those errors are
an artefact of the missing restore rather than a property of the base tree, so admitting them as a
baseline would make every Phase 2 exit-zero demand unmeetable. This task completed before P0-T6 was
started.

## Build Lock

The cross-item build lock was held across the Invoke-Restore invocation only. Acquired
2026-09-13T05:01:53, released 2026-09-13T05:02:06. The post-run filesystem observation was taken after
the release, because it reads the tree and contends on nothing.
