---
name: vstest-testcasefilter-or-operator-and-env-setup
description: vstest.console.exe 18.7.0 rejects literal "OR" in /TestCaseFilter (needs "|"); fresh worktree needs repo-local SDK install + NuGet restore before any MSBuild/vstest command works
metadata:
  type: project
---

Two environment/tooling facts discovered during issue #244 execution that cost setup time and would otherwise cause a plan's literal command text to silently match zero tests.

**Why:** Surfaced in a fresh worktree (`TaskMaster-wt-2026-07-06-11-13`) that had never run the repo's dotnet/MSBuild tooling before, and while executing a plan whose baseline/regression-test tasks specified `/TestCaseFilter:"FullyQualifiedName~A OR FullyQualifiedName~B"`.

**How to apply:**

1. **`/TestCaseFilter` boolean operator.** This repo's vstest.console.exe (18.7.0, under `C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\CommonExtensions\Microsoft\TestWindow\`) does NOT accept the literal keyword `OR` between two `FullyQualifiedName~X` clauses — it prints `Incorrect format for TestCaseFilter Error: Invalid Condition` and then reports "No test matches" even for tests that exist. The correct operator is the pipe character: `FullyQualifiedName~A|FullyQualifiedName~B`. Verified by probing two known-existing test names: `OR` matched 0/2, `|` matched 2/2 and ran both. If a plan's task text literally specifies `OR`, substitute `|` when executing and document the substitution in evidence (same test-name targets, only the boolean-operator token differs) rather than treating the plan text as broken.

2. **Fresh-worktree bootstrap order.** A brand-new git worktree of this repo has neither `.dotnet-sdk/` (global.json pins SDK 8.0.205 via a path-based `dotnet` shim that errors "repo-local .NET SDK is missing" until installed) nor `packages/` (legacy `packages.config` NuGet packages are not checked in). Before any `dotnet tool run csharpier ...`, `MSBuild`/`Invoke-VSBuild.ps1`, or `vstest.console.exe` command will succeed, run in this order:
   - `pwsh -NoProfile -ExecutionPolicy Bypass -File ./scripts/vscode/Install-RepoDotNetSdk.ps1` (must be `pwsh` 7, not Windows PowerShell 5.1 — see [[project_repo_sdk_and_nullable_rebuild]]).
   - `dotnet tool restore` — `Install-RepoDotNetSdk.ps1` does NOT do this. The manifest is at repo-root `dotnet-tools.json` (the legacy location, not `.config/dotnet-tools.json`; the SDK probes both) and pins csharpier `1.2.6`. Without it every `dotnet tool run csharpier check/format .` step fails.
   - `dotnet tool install --global dotnet-coverage` — `scripts/vscode/Invoke-MSTestWithCoverage.ps1:129-131` throws `"dotnet-coverage not found"` before running anything. This is a *global* tool, so it can be absent even when `.dotnet-sdk/` and `packages/` are both fine; check `Get-Command dotnet-coverage` rather than assuming.
   - `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-Restore.ps1 -SolutionPath TaskMaster.sln -Configuration Debug -Platform "Any CPU"` (installs ~169 packages via `MSBuild /t:Restore /p:RestorePackagesConfig=true`). Skipping this makes the very first `Invoke-VSBuild.ps1` analyzer/lint baseline fail with `error : This project references NuGet package(s) that are missing on this computer` for every legacy project (CS0006 for every `<Analyzer Include>` in first-party projects, since the analyzer packages themselves are also missing).

   When preflighting a plan, treat a missing `.dotnet-sdk/` or missing `dotnet-coverage` as a blocking finding: the plan needs an explicit Phase 0 bootstrap task, because csharpier and coverage steps carry mandatory gate/coverage evidence and cannot be deferred. Note that `packages/` being restore-driven is deliberate — it is gitignored (`.gitignore:190`) with 0 tracked files, so "package folder absent" is normal, not a defect.
