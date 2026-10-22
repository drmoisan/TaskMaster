# Toolchain single-clean-pass declaration

Timestamp: 2026-08-26T22-30

| Step | Exact command | Exit code |
| --- | --- | ---: |
| Format | `dotnet tool run csharpier format .` followed by `dotnet tool run csharpier check .` | 0 |
| Analyzer | `& "C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe" TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` | 0 |
| Nullable/type-check | `& "C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe" TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true` | 0 |
| Test and coverage | `pwsh -NoProfile -File scripts\vscode\Invoke-MSTestWithCoverage.ps1 -SearchRoot .` | 0 |

Restart count: 0.

P5-T1 through the authoritative P5-T4 run passed in one uninterrupted sequence, with no repository source file rewritten between the steps. Both MSBuild gates used `/t:Rebuild`; neither substituted `/t:Build`. The nullable/type-check command did not add `/p:Nullable=enable`.

The later raw-preservation and E3 confirmation collection is separately documented in the P5-T4 and P5-T5 evidence. Its pre-existing #592 pump-host timeout does not alter the authoritative four-step sequence or its exit codes.
