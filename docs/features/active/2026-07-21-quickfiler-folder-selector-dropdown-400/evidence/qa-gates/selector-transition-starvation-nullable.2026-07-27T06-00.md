# P8-T80 selector transition nullable gate

Command:

```powershell
msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU' /p:Nullable=enable /p:TreatWarningsAsErrors=true
```

Result: exit code `0`; build succeeded with zero compiler or nullable errors. The output reported the same five existing System.Reactive `packages.config` compatibility warnings in `UtilitiesCS`, `ToDoModel`, `QuickFiler`, `TaskMaster`, and `UtilitiesCS.Test`.
