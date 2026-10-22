# Affected Assembly Regression Verification

Timestamp: 2026-08-27T03-28-00Z

Command: `$vstest = Join-Path (& "C:\Program Files (x86)\Microsoft Visual Studio\Installer\vswhere.exe" -latest -property installationPath) "Common7\IDE\Extensions\TestPlatform\vstest.console.exe"`

EXIT_CODE: 0

Output Summary: The VSTest executable resolved successfully.

Command: `& $vstest UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /InIsolation "/TestCaseFilter:TestCategory!=LiveOutlook" "/Logger:trx;LogFileName=p4-t2-utilities.trx" "/ResultsDirectory:coverage\trx\p4-t2-utilities"`

EXIT_CODE: 0

Output Summary: 4,750 total, 4,750 executed, 4,750 passed, 0 failed, 0 errors, 0 timeouts, and 0 aborted.

Command: `& $vstest TaskMaster.Test\bin\Debug\TaskMaster.Test.dll /InIsolation "/TestCaseFilter:TestCategory!=LiveOutlook" "/Logger:trx;LogFileName=p4-t2-taskmaster.trx" "/ResultsDirectory:coverage\trx\p4-t2-taskmaster"`

EXIT_CODE: 0

Output Summary: 380 total, 380 executed, 380 passed, 0 failed, 0 errors, 0 timeouts, and 0 aborted.

The 22 exact-head hosted-CI failures are contained in the eight mapped classes covered by these assembly runs. All 22 passed. Both runs used only the injected in-memory `C:\OneDrive` value for the affected construction paths.
