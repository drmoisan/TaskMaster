---
name: csharp-agent-worktree-needs-three-bootstrap-steps
description: A fresh agent worktree cannot run any C# gate until .dotnet-sdk, packages/, AND two back-filled analyzer versions exist; green CI is NOT evidence the third is unnecessary
metadata:
  type: project
---

Any C# plan executing inside a `.claude/worktrees/<agent-id>` worktree needs THREE bootstrap steps
before the first `dotnet` command and the first `msbuild`. Preflight will flag their absence as
blocking, because every `EXIT_CODE: 0` acceptance downstream is unreachable without them.

1. **`.dotnet-sdk` is absent.** `global.json` pins `sdk.version 8.0.205` with
   `rollForward: latestFeature` and `paths: [".dotnet-sdk", "$host$"]`. A fresh worktree has none, and
   the host SDK (10.0.302) cannot satisfy it. `dotnet --version` from the worktree root prints the
   `global.json` `errorMessage` instead of a version. Fix: `scripts/vscode/Install-RepoDotNetSdk.ps1`,
   or mirror `C:\Users\DanMoisan\repos\TaskMaster\.dotnet-sdk`. Ignored by `.gitignore:350` (`.dotnet*/`).
2. **`packages/` is absent.** Every project declares `EnsureNuGetPackageBuildImports` whose `<Error>`
   fires at `BeforeTargets="PrepareForBuild"`, so msbuild hard-fails. Fix: `nuget restore TaskMaster.sln`
   (what CI does at `.github/workflows/_build-analyzers.yml:45`). Restored content is ignored by
   `.gitignore:191` (`**/[Pp]ackages/*`) — NOT by line 349, which is blank.
3. **A clean restore still breaks the build.** All 16 first-party `.csproj` files carry UNCONDITIONAL
   `<Analyzer Include>` items naming `Meziantou.Analyzer.3.0.156` and `Roslynator.Analyzers.4.16.0`,
   while all 16 `packages.config` pin `3.0.174` and `4.16.1`. Dependabot `f8e22af7` updated only the
   `Condition`-guarded `<Import>`/`<Error>` lines and `packages.config`, missing the hand-authored
   Issue-#181 analyzer items. A missing `/analyzer:` path is **`error CS0006`, not a warning** —
   verified by direct `csc.exe` probe, exit 1. Fix without touching tracked files:
   `nuget install Meziantou.Analyzer -Version 3.0.156 -OutputDirectory packages` and the same for
   `Roslynator.Analyzers -Version 4.16.0`. Only these two packages are skewed; AsyncFixer,
   BannedApiAnalyzers, MSTest.Analyzers and SonarAnalyzer all match their pins.

**Why:** Item 3 is a live repository-wide latent defect, and the obvious disconfirming evidence is
misleading. `_build-analyzers.yml:38-41` caches `path: packages` with a PREFIX `restore-keys`
fallback, so a cache-key miss (which any `packages.config` change guarantees) restores a pre-bump
tree still holding `3.0.156`/`4.16.0`, and `nuget restore` merely adds the new versions beside them.
The main checkout shows the same accumulation (`3.0.101/.123/.156/.174`). So **green CI does not
prove the compile tolerates a missing analyzer path** — it proves the old folders lingered. A cold
cache or a fresh clone would fail.

**How to apply:** When routing C# work into an agent worktree, expect preflight to require these
three tasks and do not argue them away. Never "fix" item 3 by editing a `.csproj` inside a scoped
child — that breaches the no-project-file-edit constraint; back-fill the untracked `packages/` tree
instead and file a follow-up issue for the real realignment. See
[[whole-repo-ci-gate-not-out-of-scope]] and [[bash-tool-mangles-msbuild-switches]].
