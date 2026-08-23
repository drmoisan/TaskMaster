# P5 collapsed-readiness disposal-ownership analyzer gate

Timestamp: `2026-07-22T08:42:16.0542787+00:00`

Command: `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU' /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`

EXIT_CODE: `0`

Output Summary: `PASS. The analyzer-enabled solution build completed in 8.37 seconds with zero errors and six pre-existing package/source-duplication warnings. The two authorized source hashes remained unchanged after the build.`

## Result

- Build result: succeeded.
- Errors: `0`.
- Warnings: `6`.
- Existing warnings: five `System.Reactive` `packages.config` compatibility warnings and one existing duplicate `PercentageFormatterTests.cs` source warning.
- `BreadcrumbMessengerHub.cs` SHA-256 after build: `AE307D76F01FB5C50289E9F50B6FC5F05C770A81EA4827BA010C00336A1006B2`.
- `BreadcrumbCollapsedSurfaceReadinessTests.cs` SHA-256 after build: `DAEA37BB2DA09CDA8E1B845DA4336D6CF4DEEE803B7BBEF89D9E9BB9486832B3`.
