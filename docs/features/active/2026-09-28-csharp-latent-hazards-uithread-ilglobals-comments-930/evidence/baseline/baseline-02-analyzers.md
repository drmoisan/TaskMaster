# Baseline 02: analyzer Rebuild ([P0-T10])

Timestamp: 2026-09-29T08-55
Command: pwsh -NoProfile -Command '$vswhere = Join-Path ${env:ProgramFiles(x86)} "Microsoft Visual Studio\Installer\vswhere.exe"; $msbuild = & $vswhere -latest -requires Microsoft.Component.MSBuild -find "MSBuild\**\Bin\MSBuild.exe" | Select-Object -First 1; New-Item -ItemType Directory -Force -Path coverage | Out-Null; & $msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true /v:minimal "/flp:Verbosity=detailed;LogFile=coverage/930-baseline-analyzers.detailed.log"; $code = $LASTEXITCODE; "MSBUILD_EXIT=$code"; exit $code'
Command: CMD-MSBUILD-SUMMARY with LOG = 930-baseline-analyzers.detailed.log
EXIT_CODE: 0
Output Summary:
- MSBUILD_EXIT=0; all 18 projects built (SVGControl through UtilitiesCS.Test).
- CSC_TASK_LINES=18
- ZERO_ERROR_SUMMARY_LINES=1
- ERROR_SUMMARY_LINE=0 Error(s)
- WARNING_SUMMARY_LINE=0 Warning(s)
- CS86_ERROR_LINES=0
- BASELINE-ANALYZER-WARNINGS: 0 Warning(s) (a measured value: the Rebuild compiled 18 projects)
- Environment provisioning note (pre-existing, not caused by this branch): the first invocation at 2026-09-29T08-54 exited 1 with 4 Error(s), all `CS0006: Metadata file ... could not be found`, because the `<Analyzer Include>` items in UtilitiesCS.csproj and VBFunctions.csproj name Meziantou.Analyzer 3.0.235 and SVGControl.Test.csproj names MSTest.Analyzers 4.4.0, while the packages.config restore installs 3.0.290 and 4.4.1. `git diff --name-only origin/main HEAD -- "*.csproj" "*packages.config"` printed nothing, so the skew is on origin/main. The two HintPath-named versions were installed into the gitignored packages directory with `nuget install Meziantou.Analyzer -Version 3.0.235 -OutputDirectory packages -DependencyVersion Ignore` and `nuget install MSTest.Analyzers -Version 4.4.0 -OutputDirectory packages -DependencyVersion Ignore` (both exit 0); `git status --porcelain -- "*.csproj" "*.config" "*.props" "*.targets" packages` printed nothing, so no tracked file changed. Every `<Analyzer Include>` path in the tracked .csproj files then resolved (MISSING_ANALYZER_PATHS=0), and the unmodified command above was re-run. This is the same class of action as the SDK and package restore bootstrap tasks.
