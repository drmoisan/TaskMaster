# Baseline — NuGet package restore ([P0-T4])

- Issue: #644
- Task: `[P0-T4]`
- Timestamp: 2026-08-29T08-15

Command: `pwsh -NoProfile -File .\scripts\vscode\Invoke-Restore.ps1 -SolutionPath TaskMaster.sln -Configuration Debug -Platform "Any CPU"`
Working directory: repository root (`<repo-root>`)
EXIT_CODE: 0

Output tail (host paths redacted):

```
         NuGet Config files used:
             <user-profile>\AppData\Roaming\NuGet\NuGet.Config
             C:\Program Files (x86)\NuGet\Config\Microsoft.VisualStudio.FallbackLocation.config
             C:\Program Files (x86)\NuGet\Config\Microsoft.VisualStudio.Offline.config

         Feeds used:
             <user-profile>\.nuget\packages\
             https://api.nuget.org/v3/index.json
             C:\Program Files (x86)\Microsoft SDKs\NuGetPackages\

         Installed:
             172 package(s) to packages.config projects
     1>Done Building Project "<repo-root>\TaskMaster.sln" (Restore target(s)).

Build succeeded.
    0 Warning(s)
    0 Error(s)
```

## Acceptance verification

Command: `Test-Path packages` and `@(Get-ChildItem -Directory -Path packages).Count`
EXIT_CODE: 0

Output:

```
packages-exists=True
package-dir-count=172
```

Output Summary: The restore exited 0 with 0 warnings and 0 errors and installed 172 packages to
the `packages.config` projects. The `packages` directory exists afterwards and contains **172**
package directories. `**/[Pp]ackages/*` is matched by `.gitignore` line 191, so the restored
packages do not dirty the tree.
