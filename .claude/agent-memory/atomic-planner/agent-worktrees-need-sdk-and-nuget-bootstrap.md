---
name: agent-worktrees-need-sdk-and-nuget-bootstrap
description: A fresh agent worktree has neither .dotnet-sdk nor packages/, a clean nuget restore still leaves the analyzer version skew unresolved, and dotnet-coverage is a global tool tool-restore never supplies — a C# plan needs FOUR explicit Phase 0 bootstrap steps
metadata:
  type: project
---

A C# plan executing inside a `.claude/worktrees/<agent-id>` worktree needs FOUR Phase 0 bootstrap steps
before the first `dotnet tool restore` and before the first `msbuild`, in this order:

1. **Provision `.dotnet-sdk`.** `global.json` pins `sdk.version 8.0.205` with `rollForward: latestFeature`
   and `paths: [".dotnet-sdk", "$host$"]`. A fresh worktree has no `.dotnet-sdk`, and a host that only
   carries a 10.x SDK cannot satisfy `8.0.205` under `latestFeature`, so `dotnet --version` from the
   worktree root prints the `global.json` `errorMessage` instead of a version. Remedy:
   `pwsh -NoProfile -File .\scripts\vscode\Install-RepoDotNetSdk.ps1`, or mirror the populated
   `.dotnet-sdk` tree from the main checkout. Falsifiable acceptance: `dotnet --version` prints
   `8.0.205` AND `dotnet --list-sdks` includes a path ending `.dotnet-sdk\sdk`.
2. **`nuget restore TaskMaster.sln`.** Prefer `pwsh -NoProfile -File .\scripts\vscode\Invoke-Restore.ps1`,
   which resolves MSBuild through `vswhere` and runs `/t:Restore /p:RestorePackagesConfig=true /m` — the
   `packages.config`-aware form. `nuget.exe` is not guaranteed to be on `PATH` in an agent worktree, so a
   plan task whose only stated command is `nuget restore` can be unrunnable.
   A fresh worktree has no `packages/`. Every project declares
   `<Target Name="EnsureNuGetPackageBuildImports" BeforeTargets="PrepareForBuild">` whose `<Error>`
   fires before compilation when the tree is missing (e.g. `QuickFiler.Test.csproj:452-466`), and every
   `Reference` `HintPath` under `..\packages\` is unresolvable. CI does not hit this because
   `.github/workflows/_build-analyzers.yml:45` runs `nuget restore` explicitly.
3. **Verify every `<Analyzer Include>` path resolves; back-fill any that does not.** Step 2 alone is
   not always enough. All 16 first-party `.csproj` files carry UNCONDITIONAL `<Analyzer Include>` items
   under `..\packages\<id>.<version>\...`. Those hand-authored items (Issue #181) and the
   NuGet-generated `Condition`-guarded `Import`/`Error` lines plus `packages.config` are bumped by
   DIFFERENT mechanisms, so a Dependabot bump can leave them skewed. A missing `Analyzer` path is
   `error CS0006`, NOT a warning — the compile FAILS.
   **Do NOT write a version number into the acceptance clause; it goes stale.** As of 2026-08-31 the
   tree carries `Meziantou.Analyzer 3.0.194` and `Roslynator.Analyzers 5.0.0` and the `Analyzer` items
   AGREE with `packages.config` (checked on `QuickFiler.Test.csproj:3, 493, 502-506` vs its
   `packages.config:11-16, 139-144`) — the historical `3.0.156`/`4.16.0` skew is resolved there.
   Write a version-agnostic gate instead: enumerate every `Analyzer` `Include` from every non-`packages`
   `.csproj` and `Test-Path` it joined to **that project's own directory** (`Include` resolves against
   the declaring project's dir, NOT the repo root — a check that joins to a hard-coded project folder
   silently reports garbage). Remedy for a false: `nuget install` the exact version named in the
   offending path into `packages`, or copy the folder from the main checkout.

4. **Provision the `dotnet-coverage` GLOBAL tool** when any task uses
   `scripts/vscode/Invoke-MSTestWithCoverage.ps1`. That script throws
   `dotnet-coverage not found. Install it with: dotnet tool install --global dotnet-coverage` at
   `:292-293` BEFORE it runs anything, so a baseline coverage task dies without it and no numeric
   coverage value is ever recorded. It is a global tool, NOT in `dotnet-tools.json`, so
   `dotnet tool restore` does not supply it. Guarded form:
   `if (-not (Get-Command dotnet-coverage -ErrorAction SilentlyContinue)) { dotnet tool install --global dotnet-coverage }`.

Note also that `Install-RepoDotNetSdk.ps1` must run under **pwsh 7**, not Windows PowerShell 5.1, and
that `scripts/vscode/Invoke-Restore.ps1` takes `-SolutionPath`, `-Configuration` and `-Platform`
(defaults `TaskMaster.sln` / `Debug` / `Any CPU`) — verify the parameter names before writing a
command into an acceptance clause.

**Do not conclude from green CI that the compile tolerates the skew.**
`_build-analyzers.yml:38` caches `path: packages` with a PREFIX `restore-keys` fallback (lines 40-41).
The bump guarantees an exact-key miss (the key hashes `**/packages.config`), so the fallback restores a
pre-bump tree still holding the old versions and line 45 only adds the new ones beside them. The main
checkout shows the same accumulation (Meziantou `.101/.123/.156/.174`). Green CI is lingering folders.

None of the three dirties the tree: `.gitignore:350` is `.dotnet*/` and `.gitignore:191` is `**/[Pp]ackages/*`.
Note `.gitignore:191` is the packages pattern — NOT line 349, which is blank. Verify a `.gitignore`
line citation before writing it into an acceptance clause.

**Why:** Preflight on the #511 plan returned REVISIONS REQUIRED twice: iteration 1 for the missing SDK
and `nuget restore` steps, iteration 2 for the missing analyzer back-fill. In both cases every
`EXIT_CODE: 0` acceptance on a dotnet/msbuild task was unreachable by environment.

**How to apply:** Add all three as their own Phase 0 tasks with their own binary acceptance — never fold
them into the head of an existing task, which puts two independent outcomes under one task ID. See
[[project_csharp_phase0_toolchain_bootstrap]] and [[one-ac-per-checkoff-task]].
