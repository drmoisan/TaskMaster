---
name: csharp-phase0-toolchain-bootstrap
description: C# Phase 0 must resolve the toolchain explicitly — prefer the global csharpier/dotnet-coverage exes, never `dotnet tool run`, and always include a NuGet restore task in a fresh agent worktree
metadata:
  type: project
---

Every C# atomic plan in this repo must resolve its toolchain explicitly in Phase 0 before any csharpier or coverage command task. Verify the current state each cycle; the shape below has changed at least once.

**Verified 2026-08-08 (issue #508 preflight, agent worktree):**

1. `dotnet tool run csharpier` is **broken and must not be planned**. There is no `.config/dotnet-tools.json` (the manifest sits at repo root as `dotnet-tools.json`, which `dotnet tool run` does not read), and `global.json` pins an SDK under an absent `.dotnet-sdk`, so every `dotnet` SDK command fails with the missing-SDK error.
2. Prefer the **global tools**, which were confirmed on PATH: `C:\Users\DanMoisan\.dotnet\tools\csharpier.exe` (1.3.0) and `C:\Users\DanMoisan\.dotnet\tools\dotnet-coverage.exe` (18.5.2). CSharpier 1.x needs the `format` / `check` subcommand; bare `csharpier .` is invalid.
3. `vstest.console.exe` is NOT on PATH; resolve via `C:\Program Files (x86)\Microsoft Visual Studio\Installer\vswhere.exe`.
4. A **NuGet restore task is mandatory** in a fresh agent worktree: `packages/` does not exist and there is no `bin\Debug` output, so analyzer and nullable baselines are vacuous (or fail CS0006) without it. Use `pwsh -File scripts/vscode/Invoke-Restore.ps1` (`msbuild /t:Restore /p:RestorePackagesConfig=true`, no .NET SDK required); fall back to the WinGet `nuget.exe restore TaskMaster.sln`. Watch for analyzer version skew between `<Analyzer Include>` HintPaths and the `packages.config` pins — that is an environment issue, not a plan defect.

**Why:** #418 preflight pass 1 blocked on unrunnable csharpier/coverage tasks; #508 preflight pass 1 blocked again on `dotnet tool run csharpier` plus a missing restore task. Coverage tasks carry the mandatory numeric evidence a minor-audit plan cannot report PASS without.

**How to apply:** Put the restore task in Phase 0 immediately after the formatter baseline, and write the literal resolved exe path into every command task rather than a generic tool name. Ask the caller for the resolved tool table if it was not supplied. Related: [[evidence-path-normalization]], [[csharp-coverage-gate-jacoco-format]], [[vstest-scoped-run-command]], [[csharpier-format-not-pipe-files-gate]].
