# P5 selector-toggle worker-boundary analyzer gate

Timestamp: `2026-07-22T09-01`

Command: `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU' /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`

EXIT_CODE: `0`

Output Summary: `PASS. The analyzer-enabled solution build completed with zero errors and five existing System.Reactive packages.config compatibility warnings. BreadcrumbSelectorToggleUiBoundaryTests.cs retained SHA-256 98DCF6E455A135C41C0ED5529C3EA0AEFAC50DF64CCE912A7CB14F2211465104 and 480 physical lines.`

## Result

- Build result: succeeded.
- Errors: `0`.
- Warnings: `5`.
- Warning classification: existing `System.Reactive` `packages.config` compatibility warnings.
- Authorized test hash after build: `98DCF6E455A135C41C0ED5529C3EA0AEFAC50DF64CCE912A7CB14F2211465104`.
- Authorized test physical lines after build: `480`.
