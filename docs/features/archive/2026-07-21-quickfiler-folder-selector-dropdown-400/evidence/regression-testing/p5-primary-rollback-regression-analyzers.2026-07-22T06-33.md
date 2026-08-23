# P5 Primary Rollback Regression Analyzers

Timestamp: 2026-07-22T06:33:52.0000000Z

Command: `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU' /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`

EXIT_CODE: 1

Output Summary: The analyzer-enabled solution build compiled the changed test source but failed during `QuickFiler.Test` output copying with 15 warnings and 2 errors. `MSB3027` and `MSB3021` reported that `QuickFiler.Test\bin\Debug\QuickFiler.Test.dll` was locked by `testhost` PID 44124, whose parent `vstest.console` PID 37968 was running the unrelated `BreadcrumbDropDownLifecycleConcurrencyTests|BreadcrumbPendingOpenCloseTests` filter. This is not failure-first proof; P5-T45 through P5-T48 must restart after the external assembly lock is released.
