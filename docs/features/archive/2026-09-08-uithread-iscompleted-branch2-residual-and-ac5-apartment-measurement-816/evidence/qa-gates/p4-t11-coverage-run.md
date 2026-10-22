# P4-T11 — Post-change Cobertura collection run

Timestamp: 2026-09-13T23-49

Command:

```
$vstest = & "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe" -latest -products * -find "Common7\IDE\Extensions\TestPlatform\vstest.console.exe" | Select-Object -First 1
dotnet-coverage collect --output coverage\p4-t11-postchange.cobertura.xml --output-format cobertura --settings coverage.config -- $vstest UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll QuickFiler.Test\bin\Debug\QuickFiler.Test.dll "/Settings:scripts\vscode\TaskMaster.cli.runsettings" /InIsolation "/TestCaseFilter:TestCategory!=LiveOutlook&FullyQualifiedName!~ShellUtilities&FullyQualifiedName!~SysImageListHelper&FullyQualifiedName!~OSBrowser"
```

EXIT_CODE: 0

Output Summary:

The inner test run printed `Test Run Successful.` and completed in 29.1116 seconds. The collector
then reported `Code coverage results: coverage\p4-t11-postchange.cobertura.xml.`

| Count | Value | Read or derived |
|---|---|---|
| Total | 6336 | Read, from the `Total tests:` line |
| Passed | 6336 | Read, from the `Passed:` line |
| Failed | 0 | Derived: the summary carries no `Failed:` line |
| Skipped | 0 | Derived: Total minus Passed minus Failed. The summary carries no `Skipped:` line |

The exit code is zero, so the non-deterministic-test clause was not triggered: no re-run of the
inner test run was performed and no `ExpectedExitCode` is declared.

### Per-test observation for this task

This command form passes no TRX logger, so the per-test observation is the console transcript. The
default console logger prints only the Failed and Skipped tests by name.

- Lines matching `^\s*Failed `: **0**. The console reported no test Failed.
- Lines matching `^\s*Skipped `: **0**. The console reported no test Skipped.

The structural guard
`UtilitiesCS.Test.NoLiveFormInTestAssemblyTests.ExecutingAssembly_ContainsNoFormDerivedType` and
each of the new tests appear in neither list and are therefore recorded **Passed** by absence. No
test other than that absence set was recorded Failed, so no FAIL condition is met.

**This inference is not a discovery proof and must not be cited as one.** A test that never ran is
also absent from both lists, so Passed-by-absence cannot distinguish a test that passed from one
that was never discovered. The AC12 discovery proof rests on the TRX-based executed list from
P2-T5. The Total count of 6336 recorded here matches the three repetitions and is four greater than
the P0-T15 baseline of 6332, which would have dropped had discovery failed.

The Cobertura document is written under the gitignored coverage directory. It is neither committed
nor named as an evidence artifact; its figures are transcribed by P4-T12.
