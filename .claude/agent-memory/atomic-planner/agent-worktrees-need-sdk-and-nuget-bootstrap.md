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
2. **`nuget restore TaskMaster.sln`.** A fresh worktree has no `packages/`. Every project declares
   `<Target Name="EnsureNuGetPackageBuildImports" BeforeTargets="PrepareForBuild">` whose `<Error>`
   fires before compilation when the tree is missing (e.g. `QuickFiler.Test.csproj:452-466`), and every
   `Reference` `HintPath` under `..\packages\` is unresolvable. CI does not hit this because
   `.github/workflows/_build-analyzers.yml:45` runs `nuget restore` explicitly.
3. **Back-fill `Meziantou.Analyzer 3.0.156` and `Roslynator.Analyzers 4.16.0`.** Step 2 alone is NOT
   enough. All 16 first-party `.csproj` files carry UNCONDITIONAL `<Analyzer Include>` items naming
   `..\packages\Meziantou.Analyzer.3.0.156\...` and four `..\packages\Roslynator.Analyzers.4.16.0\...`
   DLLs (`QuickFiler.Test.csproj:474-478`), while all 16 `packages.config` pin `3.0.174` and `4.16.1`.
   Dependabot commit `f8e22af7` bumped only the NuGet-generated `Condition`-guarded `Import`/`Error`
   lines and `packages.config`; the hand-authored Issue #181 `Analyzer` items were never realigned.
   A missing `Analyzer` path is `error CS0006`, NOT a warning — the compile FAILS. Remedy:
   `nuget install <id> -Version <v> -OutputDirectory packages`, or copy the folders from the main
   checkout. Both versions exist there and are verifiable with a glob before you write the claim.

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
