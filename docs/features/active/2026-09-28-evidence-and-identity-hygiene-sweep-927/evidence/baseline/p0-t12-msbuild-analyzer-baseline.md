# P0-T12 Analyzer msbuild baseline (Rebuild)

Timestamp: 2026-09-29T08-57
Command: pwsh -NoProfile -Command '$vw = Join-Path ${env:ProgramFiles(x86)} "Microsoft Visual Studio\Installer\vswhere.exe"; $mb = & $vw -latest -prerelease -products * -requires Microsoft.Component.MSBuild -find "MSBuild/**/Bin/MSBuild.exe" | Select-Object -First 1; "MSBUILD-LEAF=" + (Split-Path -Leaf $mb); & $mb TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true 2>&1 | Tee-Object -FilePath coverage/logs/927-analyzers.log; exit $LASTEXITCODE'; then MSBUILD-OBSERVE over coverage/logs/927-analyzers.log
EXIT_CODE: 0
Output Summary:
- Outlook running-process count before the build: OUTLOOK-PROCESSES=0.
- MSBUILD-LEAF=MSBuild.exe
- OUT-LINES=36 (at least 1; CoreCompile ran)
- SKIP-CORECOMPILE=0
- ZERO-ERRORS=1
- SUCCEEDED=1
- Warning summary line transcribed: "    0 Warning(s)"; additional observation WARNING-LINES=0 (no "warning XX0000" diagnostic line in the log).
- The console display was limited to the last lines of output; the full console output is in the ignored log.

BASELINE-WARNINGS: 0

Deviation D-P0-T12-1 (environment provisioning, recorded; no tracked file changed):
- First attempt of the same command: EXIT_CODE 1, "0 Warning(s)", "4 Error(s)". Diagnostics (absolute prefix replaced with <repo-root>):
  - CSC : error CS0006: Metadata file '..\packages\MSTest.Analyzers.4.4.0\analyzers\dotnet\cs\MSTest.Analyzers.CodeFixes.dll' could not be found [<repo-root>\SVGControl.Test\SVGControl.Test.csproj]
  - CSC : error CS0006: Metadata file '..\packages\MSTest.Analyzers.4.4.0\analyzers\dotnet\cs\MSTest.Analyzers.dll' could not be found [<repo-root>\SVGControl.Test\SVGControl.Test.csproj]
  - CSC : error CS0006: Metadata file '..\packages\Meziantou.Analyzer.3.0.235\analyzers\dotnet\roslyn5.0\cs\Meziantou.Analyzer.dll' could not be found [<repo-root>\UtilitiesCS\UtilitiesCS.csproj]
- Cause (pre-existing, not introduced by this item): project-file `<Analyzer Include>` paths name Meziantou.Analyzer 3.0.235 (16 references) and MSTest.Analyzers 4.4.0 (18 references), while packages.config pins 3.0.290 and 4.4.1, so the P0-T7 restore installed only the newer folders. git status --porcelain over packages, "*.csproj" and "*packages.config" was empty before and after.
- Remedy (ignored output only): nuget install Meziantou.Analyzer -Version 3.0.235 -OutputDirectory packages -NonInteractive (exit 0); nuget install MSTest.Analyzers -Version 4.4.0 -OutputDirectory packages -NonInteractive (exit 0). The packages folder is ignored (.gitignore line 191). The build was then re-run and produced the figures above; this artifact records the second run as the baseline.
- The version skew in the project files is out of this item's scope (AC19 forbids project-file edits) and is reported in the completion report for follow-up.
