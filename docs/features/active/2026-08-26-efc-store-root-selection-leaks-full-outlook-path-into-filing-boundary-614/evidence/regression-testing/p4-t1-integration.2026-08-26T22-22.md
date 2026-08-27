# Phase 4 cross-assembly integration evidence

Timestamp: 2026-08-26T22-22

## Commands and results

1. `& "C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe" TaskMaster.sln /t:Build /m /p:Configuration=Debug "/p:Platform=Any CPU"`
   - `EXIT_CODE: 0`
   - Build passed with 0 errors and the five previously recorded `System.Reactive` `packages.config` warnings.
2. `& $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /InIsolation "/TestCaseFilter:TestCategory!=LiveOutlook" "/Logger:trx;LogFileName=p4-t1-qf.trx" "/ResultsDirectory:coverage\trx\p4-t1-qf"`
   - `EXIT_CODE: 0`
   - 980 total, 980 passed, 0 failed.
3. `& $vstest TaskMaster.Test\bin\Debug\TaskMaster.Test.dll /InIsolation "/TestCaseFilter:TestCategory!=LiveOutlook" "/Logger:trx;LogFileName=p4-t1-tm.trx" "/ResultsDirectory:coverage\trx\p4-t1-tm"`
   - `EXIT_CODE: 0`
   - 380 total, 380 passed, 0 failed.
4. `& $vstest UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /InIsolation "/TestCaseFilter:TestCategory!=LiveOutlook" "/Logger:trx;LogFileName=p4-t1-ut.trx" "/ResultsDirectory:coverage\trx\p4-t1-ut"`
   - `EXIT_CODE: 0`
   - 4749 total, 4749 passed, 0 failed.

## Output Summary

All 6109 assembly tests passed. There were no new failures versus the P0-T9 baseline and no failures in the rule-8 set. The mandatory `TestCategory!=LiveOutlook` filter was applied to all three assembly runs.
