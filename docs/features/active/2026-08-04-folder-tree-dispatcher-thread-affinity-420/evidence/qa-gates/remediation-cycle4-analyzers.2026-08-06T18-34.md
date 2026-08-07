# Cycle 4 analyzer restart result

- Task: `[P6-T2]` restart after the P6-T6 whitespace correction.
- Command: `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU' /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
- Exit status: 0.
- Result: build succeeded with 0 errors.
- Existing warnings: five `System.Reactive.PackagesConfigCheck.targets` packages.config compatibility warnings in UtilitiesCS, ToDoModel, QuickFiler, TaskMaster, and UtilitiesCS.Test.
- Result: pass; no analyzer errors were reported.
