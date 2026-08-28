---
name: invoke-vsbuild-rewrites-csproj-hintpaths
description: Invoke-VSBuild.ps1 runs Sync-PackageReferences.ps1, which rewrites <HintPath> in EVERY csproj — so when a feature has a forbidden .csproj, the build wrapper itself can commit the scope violation; use direct MSBuild via vswhere instead
metadata:
  type: feedback
---

When a plan carries a forbidden-file list that includes a `.csproj`, do NOT plan `scripts/vscode/Invoke-VSBuild.ps1`. Resolve `MSBuild.exe` through `vswhere` and pass the `CLAUDE.md` argument list directly.

**Why:** `Invoke-VSBuild.ps1:152-155` unconditionally invokes `scripts/vscode/Sync-PackageReferences.ps1 -SolutionRoot $repoRoot` before building. That script scans every `*.csproj` for `<HintPath>..\packages\...\lib\...</HintPath>` values that do not resolve on disk and rewrites them in place (`Sync-PackageReferences.ps1:55-154`). In a fresh agent worktree — where `packages/` is absent or holds versions skewed from `packages.config` — that is a live rewrite path, not a theoretical one. On #476 the forbidden list included `QuickFiler/QuickFiler.csproj`, so a single wrapper-driven build would have produced a scope violation that the feature's own scope-containment gate then reports against the executor.

Two related facts:

- The wrapper otherwise supports everything the mandated commands need (`-Target Rebuild`, `-EnableNETAnalyzers`, `-EnforceCodeStyleInBuild`, `-TreatWarningsAsErrors`), so the reason to skip it is exclusively the csproj rewrite. The general "prefer repo-defined tasks" guidance in `policy-compliance-order` still holds everywhere else.
- The wrapper `throw`s on a non-zero MSBuild exit (`:165-167`), so a `pwsh -File` run of it exits 1 rather than surfacing MSBuild's own code. `scripts/vscode/Invoke-MSTestWithCoverage.ps1:236` does the same. Baseline tasks must take `EXIT_CODE:` from the pwsh process and must not gate on `EXIT_CODE: 0`.

**How to apply:** grep a draft plan for `Invoke-VSBuild`; if the feature forbids any `.csproj`, swap to the vswhere-resolved `& $msbuild TaskMaster.sln /t:Rebuild ...` form and say in the plan why. Also add a Phase 5 gate that no `.csproj` outside the writable set appears in the change inventory. Related: [[poshqc-mcp-and-msbuild-invocation-facts]], [[project-csharp-phase0-toolchain-bootstrap]].
