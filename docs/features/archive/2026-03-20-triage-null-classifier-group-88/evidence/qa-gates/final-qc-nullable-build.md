# Final QC — Nullable Build

- **Timestamp:** 2026-03-20T09-56
- **Command:** `& 'C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe' TaskMaster.sln /t:Build /p:Configuration=Debug "/p:Platform=Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true`
- **EXIT_CODE:** 1
- **Output Summary:** The nullable/type-safety build hit the same VSTO `FindRibbons` Application Control failure as the analyzer build (`TaskMaster.dll` blocked, `MSB4018`, `HRESULT 0x800711C7`). No nullable warnings from the touched changes were reported before the environment-specific failure. This differs from the recorded baseline nullable build, which had succeeded with one pre-existing `MSB3277` warning.