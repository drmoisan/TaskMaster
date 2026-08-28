# QuickFiler.Test Whole-Assembly Baseline Count (P0-T11)

Timestamp: 2026-08-28T15-49
Command (CR-VSTEST, fully expanded):

```
pwsh -NoProfile -Command '$vswhere = Join-Path ${env:ProgramFiles(x86)} "Microsoft Visual Studio\Installer\vswhere.exe"; $vstest = & $vswhere -latest -products * -find "Common7\IDE\Extensions\TestPlatform\vstest.console.exe" | Select-Object -First 1; & $vstest "QuickFiler.Test\bin\Debug\QuickFiler.Test.dll" "/Settings:scripts\vscode\TaskMaster.cli.runsettings" /InIsolation "/TestCaseFilter:TestCategory!=LiveOutlook" /Logger:trx "/ResultsDirectory:docs/features/active/2026-08-28-quickfiler-keyboard-hook-leaks-to-outlook-677/evidence/baseline/p0-t11"'
```

Preceded by a clear of the results directory (`Remove-Item -Recurse -Force` when present; the
directory did not exist, so the pre-state was `absent`) so exactly one timestamp-named TRX can
exist under it.

EXIT_CODE: 0

## Output Summary

```
Test Run Successful.
Total tests: 1201
     Passed: 1201
 Total time: 9.8719 Seconds
```

TRX `<Counters>` element:

| total | executed | passed | failed | error | aborted | notExecuted |
|---|---|---|---|---|---|---|
| 1201 | 1201 | 1201 | 0 | 0 | 0 | 0 |

QFT_BASELINE_TOTAL: 1201

Total/passed/failed/skipped quadruple: **1201 / 1201 / 0 / 0**

P4-T3's non-vacuity floor is therefore `QFT_BASELINE_TOTAL + 17` = **1218**.

## TRX artifact

Exactly one TRX exists under
`docs/features/active/2026-08-28-quickfiler-keyboard-hook-leaks-to-outlook-677/evidence/baseline/p0-t11/`:

- `p0-t11-quickfiler-test-baseline.trx`

The file was renamed from the vstest default name and its contents were sanitised in binary mode
with case-insensitive substitutions, per the repository-wide "never embed absolute host paths"
rule, before it could be staged. Substituted token classes: workspace-root prefix (both separator
spellings), user-profile prefix (both separator spellings), the `computerName`/host identifier, and
the `runUser`/account identifier. 3610 substitutions were applied. Post-conditions verified:

- case-insensitive fixed-string sweep for the account identifier: 0 hits
- case-insensitive fixed-string sweep for the host identifier: 0 hits
- case-insensitive fixed-string sweep for `:\Users\`: 0 hits
- case-insensitive fixed-string sweep for `:/Users/`: 0 hits
- the document still parses under a strict XML parser and contains exactly 1201
  `<UnitTestResult>` elements, matching the reported total (so the substitution did not corrupt
  the file)

Placeholders were written XML-escaped (`&lt;repo-root&gt;` and so on) so the attribute values
decode to the required literal placeholders without making the document unparseable.
