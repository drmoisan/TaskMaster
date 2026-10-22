# AC-7 Unmodified-Green Gate (P4-T3)

Timestamp: 2026-08-28T16-05

## (1) Unmodified-files assertion

Command: `git status --porcelain -- QuickFiler.Test/`

Output (verbatim):

```
 M "QuickFiler.Test/Helper Classes/QfcThemeHelperTests.cs"
 M QuickFiler.Test/QuickFiler.Test.csproj
?? QuickFiler.Test/Controllers/QfcFormControllerDeactivateTests.cs
?? QuickFiler.Test/Controllers/QfcItemController.CancelBreadcrumbSelectorTests.cs
?? QuickFiler.Test/Viewers/BreadcrumbDropDownHostTests.Part3.cs
```

The listing contains ONLY:

- the three new files from P1-T1..P1-T3,
- `QuickFiler.Test.csproj` (three additive `<Compile Include>` items),
- `QfcThemeHelperTests.cs`, the D8 manual fake, whose entire diff is the three-line additive
  `CancelBreadcrumbSelector` member inside `FakeQfcItemController` (reproduced in the P3-T10
  artifact). No `[TestMethod]`, assertion, or test body is touched.

Every other pre-existing breadcrumb and controller test file is byte-unmodified. In particular
`QuickFiler.Test/Viewers/BreadcrumbDropDownHostTests.cs` and
`QuickFiler.Test/Viewers/BreadcrumbDropDownHostTests.Part2.cs` do not appear, so the reflection-bound
constructor harnesses that decision D3 protects were never edited to accommodate the fix.

## (2) Whole-assembly run

Command (CR-VSTEST, fully expanded — the exact scope of the P0-T11 baseline run; no namespace
filter):

```
pwsh -NoProfile -Command '$vswhere = Join-Path ${env:ProgramFiles(x86)} "Microsoft Visual Studio\Installer\vswhere.exe"; $vstest = & $vswhere -latest -products * -find "Common7\IDE\Extensions\TestPlatform\vstest.console.exe" | Select-Object -First 1; & $vstest "QuickFiler.Test\bin\Debug\QuickFiler.Test.dll" "/Settings:scripts\vscode\TaskMaster.cli.runsettings" /InIsolation "/TestCaseFilter:TestCategory!=LiveOutlook" /Logger:trx "/ResultsDirectory:docs/features/active/2026-08-28-quickfiler-keyboard-hook-leaks-to-outlook-677/evidence/regression-testing/p4-t3"'
```

The results directory was deleted before the run so exactly one timestamp-named TRX can exist
under it.

EXIT_CODE: 0

### Output Summary

```
Test Run Successful.
Total tests: 1218
     Passed: 1218
 Total time: 9.1375 Seconds
```

TRX `<Counters>`: `total="1218" passed="1218" failed="0" error="0" notExecuted="0"`.

### (a) Failures not in `BASELINE_FAILURE_SET`

`BASELINE_FAILURE_SET` from P0-T9 is empty. This run recorded **0** failures, so the count of
failures not present in `BASELINE_FAILURE_SET` is **0**.

### (b) Non-vacuity floor

| Quantity | Value |
|---|---|
| `QFT_BASELINE_TOTAL` (P0-T11) | 1201 |
| New P1 tests in this assembly | 17 (8 + 7 + 2) |
| Required floor `QFT_BASELINE_TOTAL + 17` | **1218** |
| Executed total in this run | **1218** |

1218 >= 1218, so the gate is satisfied and the run is provably non-vacuous: the executed count
equals the baseline count plus exactly the seventeen new tests, which also confirms no pre-existing
test was dropped or filtered away.

## TRX artifact

Exactly one TRX exists under
`docs/features/active/2026-08-28-quickfiler-keyboard-hook-leaks-to-outlook-677/evidence/regression-testing/p4-t3/`:

- `p4-t3-quickfiler-test-whole-assembly.trx`

Renamed from the vstest default name and sanitised in binary mode with case-insensitive
substitutions (3661 applied) over the workspace-root prefix, user-profile prefix, host identifier
and account identifier, per the repository-wide "never embed absolute host paths" rule.
Post-condition sweeps (case-insensitive, fixed-string) return 0 hits for the account identifier and
the host identifier.
