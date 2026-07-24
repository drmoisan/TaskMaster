# Surface factory owner-thread analyzer gate

- Timestamp: `2026-07-23T14-07Z`
- Command: `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU' /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true /v:minimal`
- EXIT_CODE: `0`
- Output Summary: `All solution projects built successfully with analyzer enforcement; zero errors and five existing System.Reactive packages.config warnings; no source file changed.`

`QuickFiler.Test/Viewers/BreadcrumbPopupControlDispatchTests.cs` retained SHA-256
`3FE231161F91AB05FE28F4E99AE047B5D56B95FC8C09EF263B2FC4FB39676D38`.
The five warnings are the existing System.Reactive packages.config compatibility warning
reported for `UtilitiesCS`, `ToDoModel`, `QuickFiler`, `TaskMaster`, and
`UtilitiesCS.Test`; the owner-thread batch introduced no analyzer diagnostic.

