# Scoped Host Regression Run (P4-T1)

Timestamp: 2026-08-28T16-03
Command (CR-VSTEST, fully expanded):

```
pwsh -NoProfile -Command '$vswhere = Join-Path ${env:ProgramFiles(x86)} "Microsoft Visual Studio\Installer\vswhere.exe"; $vstest = & $vswhere -latest -products * -find "Common7\IDE\Extensions\TestPlatform\vstest.console.exe" | Select-Object -First 1; & $vstest "QuickFiler.Test\bin\Debug\QuickFiler.Test.dll" "/Settings:scripts\vscode\TaskMaster.cli.runsettings" /InIsolation "/TestCaseFilter:FullyQualifiedName~BreadcrumbDropDownHostTests" /Logger:trx "/ResultsDirectory:docs/features/active/2026-08-28-quickfiler-keyboard-hook-leaks-to-outlook-677/evidence/regression-testing/p4-t1"'
```

The results directory was deleted before the run so exactly one timestamp-named TRX can exist
under it.

EXIT_CODE: 0

## Output Summary

```
Test Run Successful.
Total tests: 29
     Passed: 29
 Total time: 1.5560 Seconds
```

Total / passed / failed triple: **29 / 29 / 0**. TRX `<Counters>`: `total="29" passed="29"
failed="0" notExecuted="0"`.

The 29 comprise the eight new P1-T1 tests plus the 21 pre-existing `BreadcrumbDropDownHostTests`
methods across the primary file and Part2. Every pre-existing method passed unmodified, which is
the AC-7 signal at this scope.

### All eight P1-T1 test names present among the passed tests

Each name occurs exactly once in the TRX `testName` attribute set and each is recorded as passed:

1. `FinishClose_DropDownClosedPath_PredicateFalse_DoesNotFocusAnchor`
2. `FinishClose_ProgrammaticClose_PredicateFalse_DoesNotFocusAnchor`
3. `FinishClose_PredicateTrue_FocusAnchorInvoked`
4. `FinishClose_PredicateFlipsFalseAfterScheduling_DoesNotFocusAnchor`
5. `AlreadyOpenRefocus_PredicateFalse_DoesNotFocusPending`
6. `AlreadyOpenRefocus_PredicateTrue_FocusPendingInvoked`
7. `FreshOpenFocus_PredicateFalse_DoesNotFocusPending`
8. `UnsetPredicate_DefaultsTrue_FocusAnchorStillInvoked`

## TRX artifact

Exactly one TRX exists under
`docs/features/active/2026-08-28-quickfiler-keyboard-hook-leaks-to-outlook-677/evidence/regression-testing/p4-t1/`:

- `p4-t1-breadcrumbdropdownhosttests.trx`

Renamed from the vstest default name and sanitised in binary mode with case-insensitive
substitutions (94 applied) over the workspace-root prefix, user-profile prefix, host identifier and
account identifier, per the repository-wide "never embed absolute host paths" rule. Post-condition
sweeps (case-insensitive, fixed-string) return 0 hits for the account identifier and the host
identifier.
