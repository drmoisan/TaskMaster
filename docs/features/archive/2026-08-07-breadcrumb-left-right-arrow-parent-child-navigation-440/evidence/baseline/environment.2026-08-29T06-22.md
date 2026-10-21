# Phase 0 — Toolchain Environment Bootstrap (issue #440)

Timestamp: 2026-08-29T06-22

Covers plan tasks P0-T5, P0-T6, P0-T7 and P0-T8. All commands were run from the
repository root of this worktree, which is redacted below as `<repo-root>`.

---

## [P0-T5] Repository-local .NET SDK

Command: `pwsh -NoProfile -File scripts\vscode\Install-RepoDotNetSdk.ps1`
EXIT_CODE: 0

Output Summary:

```
Downloading .NET SDK 8.0.205 from https://builds.dotnet.microsoft.com/dotnet/Sdk/8.0.205/dotnet-sdk-8.0.205-win-x64.zip...
Installed repo-local .NET SDK 8.0.205 to <repo-root>\.dotnet-sdk.
```

Command: `dotnet --version`
EXIT_CODE: 0
Output Summary: `8.0.205`

`.dotnet-sdk` directory now exists at the repository root: confirmed
(`Test-Path .dotnet-sdk` returned `True`).

---

## [P0-T6] CSharpier tool manifest

Command: `dotnet tool restore`
EXIT_CODE: 0

Output Summary:

```
Tool 'csharpier' (version '1.2.6') was restored. Available commands: csharpier
Restore was successful.
```

Command: `dotnet tool run csharpier --version`
EXIT_CODE: 0
Output Summary: `1.2.6` — matches the version pinned by the repository-root
dotnet-tools.json manifest, as P0-T6 requires.

---

## [P0-T7] NuGet restore and analyzer reconciliation

### Restore

Command: `pwsh -NoProfile -File scripts\vscode\Invoke-Restore.ps1`
EXIT_CODE: 0

Output Summary (restored package count on the final lines of the restore log):

```
Installed:
    172 package(s) to packages.config projects
Done Building Project "<repo-root>\TaskMaster.sln" (Restore target(s)).

Build succeeded.
    0 Warning(s)
    0 Error(s)
```

Restored package count: 172.

### Derived referenced-analyzer set

The set was derived mechanically, exactly as P0-T7 prescribes: the value of the
`Include` attribute of every `Analyzer` item in every project file listed by
`git ls-files "*.csproj"`, each resolved against the directory of the project file
that declares it.

- Project files enumerated by `git ls-files "*.csproj"`: 18
- Distinct analyzer paths in the derived set: **11**

### Referenced-but-missing count BEFORE provisioning

Referenced-but-missing count (before): **5**

Full list of the missing paths:

```
<repo-root>\packages\Meziantou.Analyzer.3.0.156\analyzers\dotnet\roslyn5.0\cs\Meziantou.Analyzer.dll
<repo-root>\packages\Roslynator.Analyzers.4.16.0\analyzers\dotnet\roslyn4.7\cs\Roslynator.CSharp.Analyzers.dll
<repo-root>\packages\Roslynator.Analyzers.4.16.0\analyzers\dotnet\roslyn4.7\cs\Roslynator_Analyzers_Roslynator.Common.dll
<repo-root>\packages\Roslynator.Analyzers.4.16.0\analyzers\dotnet\roslyn4.7\cs\Roslynator_Analyzers_Roslynator.Core.dll
<repo-root>\packages\Roslynator.Analyzers.4.16.0\analyzers\dotnet\roslyn4.7\cs\Roslynator_Analyzers_Roslynator.CSharp.dll
```

This is the pre-existing, repository-wide analyzer version skew recorded as plan
ground-truth item 9. The packages-config files pin Meziantou.Analyzer 3.0.174 and
Roslynator.Analyzers 4.16.1, while the hand-written `Analyzer` items name 3.0.156
and 4.16.0, so a restore installs neither referenced version. The skew is present at
`BASE` and is not introduced by this change.

### Provisioning action taken, one line per package directory

Enclosing main checkout resolved as the parent of the directory printed by
`git rev-parse --path-format=absolute --git-common-dir`.

- `Meziantou.Analyzer.3.0.156` — COPIED from the enclosing main checkout's packages
  directory into `<repo-root>\packages`. No `nuget install` fallback was required.
- `Roslynator.Analyzers.4.16.0` — COPIED from the enclosing main checkout's packages
  directory into `<repo-root>\packages`. No `nuget install` fallback was required.

No project file and no packages-config file was edited. The repository-root packages
directory is gitignored by the `.gitignore` pattern `**/[Pp]ackages/*`, so these two
writes are invisible to every gate in this plan.

### Referenced-but-missing count AFTER provisioning

Re-derived over the same 11-path set:

- Distinct analyzer paths in the derived set (after): 11
- Referenced-but-missing count (after): **0**

Gate half 1: PASS (after count is 0).

### Scoped ownership check

Command: `git status --porcelain -- "*.csproj" "*packages.config"`
EXIT_CODE: 0
Verbatim output: (empty)

Gate half 2: PASS (scoped status output is empty, proving the reconciliation edited
no project file and no packages-config file anywhere in the solution).

---

## [P0-T8] Resolved absolute tool paths

All three resolved and confirmed to exist on disk. The `dotnet-coverage` path lies
under the machine user profile and is redacted as `<user-profile>` per Global rule 8.

| Tool | Absolute path | Exists |
| --- | --- | --- |
| `$msbuild` | `C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe` | True |
| `$vstest` | `C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe` | True |
| `dotnet-coverage` | `<user-profile>\.dotnet\tools\dotnet-coverage.exe` | True |

Resolution commands used, exactly as Global rule 3 states:

```
$msbuild = & "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe" -latest -requires Microsoft.Component.MSBuild -find 'MSBuild\**\Bin\MSBuild.exe' | Select-Object -First 1
$vstest  = & "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe" -latest -products * -find 'Common7\IDE\Extensions\TestPlatform\vstest.console.exe' | Select-Object -First 1
Get-Command dotnet-coverage
```

EXIT_CODE: 0 for all three resolutions. No `TOOLCHAIN-BLOCKER:` was recorded.
