---
name: vs18-build-toolchain-paths
description: Canonical C# build/test toolchain paths and invocation quirks in TaskMaster worktrees (VS18 MSBuild, vstest, dotnet-coverage, resx trap)
metadata:
  type: project
---

Building/testing the TaskMaster net48 VSTO solution in a fresh worktree on this host.

**Why:** #324 execution burned significant time discovering the toolchain because msbuild/vstest are not on PATH and the repo-local Core SDK cannot build the legacy binary-resx projects.

**How to apply:**
- Canonical build tool is FULL-FRAMEWORK MSBuild from Visual Studio **18** (not "2022"): `C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\amd64\MSBuild.exe`. vstest: `C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe`.
- Do NOT build with the repo-local `.dotnet-sdk` `dotnet msbuild` (Core-hosted): it fails UtilitiesCS with MSB3822/MSB3823 (non-string binary .resx needs System.Resources.Extensions + GenerateResourceUsePreserializedResources). Workarounds via CustomAfterMicrosoftCommonProps/Targets CLOBBER NuGet package-import hooks → cascade of CS0246/CS0738. A scoped `System.Resources.Extensions` reference also breaks Outlook `EmbedInteropTypes`. Just use VS18 msbuild.exe, which handles binary resx natively.
- Fresh worktree bootstrap: (1) `scripts/vscode/Install-RepoDotNetSdk.ps1` via pwsh7 (needed only for the csharpier dotnet tool); (2) NuGet packages.config restore — `Invoke-Restore.ps1` needs VS/vswhere, but a bare `nuget.exe restore TaskMaster.sln` works (there is a `nuget.exe` at `/tmp/nuget.exe`). `dotnet msbuild -t:Restore` does NOT restore packages.config projects.
- git-bash: set `MSYS_NO_PATHCONV=1` so `/t:Build /p:...` switches are not path-mangled; quote `/p:Platform="Any CPU"` (project-level standalone build uses `AnyCPU`).
- Format: csharpier is a local tool v1.x → `dotnet csharpier check .` / `dotnet csharpier format <files>` (subcommands; bare `dotnet csharpier .` is v0 syntax). Needs `DOTNET_ROOT=.dotnet-sdk` on PATH.
- Coverage: `dotnet-coverage` global exe. It re-parses the command string and splits on spaces, so a quoted `"C:\Program Files\...vstest.console.exe"` breaks with `'C:\Program' ... parameter is incorrect`. Use the `--` separator form: `dotnet-coverage collect --output OUT --output-format cobertura -- "$VSTEST" "UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll" /InIsolation`. Cobertura `<class>`/`<method>` elements put `line-rate`/`branch-rate` BEFORE `name`.
- Nullable gate (`/p:Nullable=enable /p:TreatWarningsAsErrors=true`): incremental `/t:Build` after an analyzer build is a no-op (0/0). A genuine `/t:Rebuild` surfaces ~34 distinct pre-existing nullable errors confined to vendored `SVGControl.csproj` (see [[project_repo_sdk_and_nullable_rebuild]]); first-party UtilitiesCS/UtilitiesCS.Test are 0. Rebuild-with-dependencies double-reports (34→68) under parallel; use `-m:1` for distinct counts.
- `pwsh -NoProfile -Command` tokenization for MSBuild switches (measured 2026-08-08): `/p:Platform='Any CPU'` and `/flp:'logfile=<abs>.log;verbosity=normal'` each collapse to ONE argument with the quotes stripped, and the embedded `;` does NOT split the pwsh statement — the plan-style quoting is safe as written. `/fl /flp:logfile=...` needs the target directory to already exist. Inside a pwsh `-Command` string use `| Out-Null`, never `> /dev/null` (pwsh resolves it as the literal path `C:\dev\null` and the whole statement dies).
- Under coverage instrumentation ~17 Deedle/DataFrame/ETL tests (FromDefaultFolder_*, FromArray2D_*, DeedleDoodles, etc.) flake; they pass with 0 failures when instrumentation is off. Pre-existing, unrelated to Folder scoring.
