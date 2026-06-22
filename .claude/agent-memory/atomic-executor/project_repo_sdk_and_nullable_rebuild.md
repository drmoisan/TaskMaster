---
name: repo-sdk-install-and-nullable-rebuild
description: repo-local .NET SDK (global.json 8.0.205 in .dotnet-sdk) must be installed via pwsh7 not Win-PS5.1; csharpier v1 uses subcommands; forced -t:Rebuild under nullable gate surfaces only vendored-project errors
metadata:
  type: project
---

The TaskMaster worktree pins a repo-local .NET SDK and routes `dotnet` through a shim that fails ("repo-local .NET SDK is missing") until it is installed.

**Why:** `global.json` sets `sdk.version=8.0.205` with `paths: [".dotnet-sdk", "$host$"]`. A bare `dotnet tool run ...` fails until `.dotnet-sdk/` exists.

**How to apply:**
- Install once per worktree: `pwsh -NoProfile -ExecutionPolicy Bypass -File ./scripts/vscode/Install-RepoDotNetSdk.ps1`. It MUST run under PowerShell 7 (`pwsh`), not Windows PowerShell 5.1 — the script uses `System.Net.Http.HttpCompletionOption`, absent in 5.1, so WinPS errors with TypeNotFound. pwsh 7.6 is at `/c/Program Files/PowerShell/7/pwsh`.
- After install, invoke tools with `export PATH="$(pwd)/.dotnet-sdk:$PATH" DOTNET_ROOT="$(pwd)/.dotnet-sdk"` then `./.dotnet-sdk/dotnet tool run csharpier ...`.
- CSharpier is a local tool pinned at 1.2.6 (`dotnet-tools.json`). v1 uses SUBCOMMANDS: `csharpier check .` and `csharpier format .` (the old `--check`/bare-path v0 syntax is gone). A separate global csharpier (1.3.0) exists at `~/.dotnet/tools` — prefer the manifest version via `dotnet tool run`.

**Nullable gate quirk:** the CLAUDE.md policy command is `msbuild ... -t:Build -p:Nullable=enable -p:TreatWarningsAsErrors=true` and it passes clean (0/0) for first-party. Do NOT substitute `-t:Rebuild`: a forced Rebuild under those flags fails fast (~0.5s) with ~84 errors confined entirely to the two vendored/exempt projects (SVGControl, UtilitiesSwordfish.NET.General — CS8603/CS0649) before first-party even compiles. Those are pre-existing vendored issues outside the analyzer-stack scope, not a real gate failure. Use `-t:Build` as the policy gate; if you run a Rebuild for rigor, re-run a plain `-t:Build` afterward to restore the Debug build state before vstest.

**Pre-existing CS0618 in first-party:** UtilitiesCS/TaskMaster use obsolete IAsyncEnumerable `SelectAwait`/`WhereAwait`/`ForEachAwaitAsync` overloads (warning-only; not promoted under the analyzer `-t:Build` step which omits TWAE). An incremental baseline build may not re-emit them while a later full build does — explains baseline-vs-final warning-count deltas without any new diagnostic.

**Coverage:** repo uses `dotnet-coverage collect ... -- <vstest> <asm> /Settings:scripts/vscode/TaskMaster.cli.runsettings /InIsolation` producing Cobertura. Raw repo-wide @line-rate (~71.6%) includes vendored Swordfish/SVGControl; first-party denominator (#197) excludes them. The helper `Invoke-MSTestWithCoverage.ps1` throws on a single-assembly SearchRoot (`$testAssemblies.Count` under StrictMode when the filter yields one string) — invoke dotnet-coverage directly instead. See [[project_coverage_firstparty_denominator_method]].
