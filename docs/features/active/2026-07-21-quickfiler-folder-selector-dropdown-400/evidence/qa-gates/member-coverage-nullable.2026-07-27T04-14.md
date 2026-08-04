# Member coverage nullable build restart

Timestamp: 2026-07-27T04-14
Command: `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU' /p:Nullable=enable /p:TreatWarningsAsErrors=true`
EXIT_CODE: 0
Output Summary: Nullable build passed after the P8-T65 assertion correction with zero errors and no scope change.
