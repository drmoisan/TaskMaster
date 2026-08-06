# Cycle 4 nullable restart result

- Task: `[P6-T3]` restart after the P6-T6 whitespace correction.
- Command: `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU' /p:Nullable=enable /p:TreatWarningsAsErrors=true`
- Exit status: 0.
- Result: build succeeded with 0 errors and no nullable diagnostics.
- Existing warnings: five `System.Reactive.PackagesConfigCheck.targets` packages.config compatibility warnings in UtilitiesCS, ToDoModel, QuickFiler, TaskMaster, and UtilitiesCS.Test.
- Result: pass.
