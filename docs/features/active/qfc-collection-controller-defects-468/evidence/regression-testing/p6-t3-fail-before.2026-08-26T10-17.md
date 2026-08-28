# [P6-T3] [expect-fail] `GetMoveDiagnostics_WithNullItemController_ReturnsUnknownLineWithoutThrowing`

Timestamp: 2026-08-26T10-17

Issue #469 defect 2 (null guard placed below the dereference it guards, making its else branch
unreachable).

Command:

```
pwsh -NoProfile -File scripts/vscode/Invoke-VSBuild.ps1 -Target Build   # precondition, EXIT_CODE 0

$vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'
$vstest  = & $vswhere -latest -products * -find 'Common7\IDE\Extensions\TestPlatform\vstest.console.exe' | Select-Object -First 1
& $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll `
    /Settings:scripts\vscode\TaskMaster.cli.runsettings `
    /InIsolation `
    /TestCaseFilter:"FullyQualifiedName~GetMoveDiagnostics_WithNullItemController_ReturnsUnknownLineWithoutThrowing" `
    /Logger:"trx;LogFileName=p6-t3.trx" `
    /ResultsDirectory:docs\features\active\qfc-collection-controller-defects-468\evidence\regression-testing\p6-t3
```

EXIT_CODE: 1

ExpectedExitCode: 1

## Output Summary

Build precondition: `EXIT_CODE 0`, 0 errors, 5 warnings (all pre-existing
`System.Reactive.PackagesConfigCheck` `packages.config` notices).

Test run: `Test Run Failed. Total tests: 1  Failed: 1`.

TRX `<Counters>`, verbatim from `evidence/regression-testing/p6-t3/p6-t3.trx`:

```
total="1" executed="1" passed="0" failed="1" error="0" timeout="0" aborted="0" inconclusive="0"
```

Failed count is exactly `1`, as the task's acceptance requires.

## Acceptance: the observed failure is named `NullReferenceException`

Recorded failure message:

```
Did not expect any exception because issue #469 defect 2 requires the null guard to run before the
first dereference, so a group with no item controller degrades to an Unknown diagnostics line
instead of raising NullReferenceException, but found System.NullReferenceException: Object
reference not set to an instance of an object.
```

The recorded stack trace names the throwing frame:

```
QuickFiler.Controllers.QfcCollectionController.GetMoveDiagnostics(...)
    in <repo-root>\QuickFiler\Controllers\QfcCollectionController.cs:line 2097
```

Line 2097 is `var helper = qf.ItemHelper;`, the first of two unconditional dereferences of `qf` that
precede the `if (qf is not null)` test. The second is the `xComma(qf.ItemHelper.Subject)`
interpolation in the data-line prefix.

## Why this proves the else branch is dead

`qf` is assigned from `TryGetItemGroupByIndex(k)?.ItemController`. Because both dereferences above
the guard are unconditional, control can only reach the `if` when `qf` is non-null; when it is null
the method has already thrown. The `else` branch that emits
`To Unknown,Sender Unknown,Email,Folder Unknown,Sent Date Unknown,Sent Time Unknown` is therefore
unreachable in the pre-fix source, which is exactly what issue #469 defect 2 reports. The
post-fix assertion on `Folder Unknown` is the observable proof that the branch became reachable.

## Post-hoc amendment to the test's assertion literal (disclosed)

After the P6-T4 fix landed, the string assertion in this test was widened from `Folder Unknown` to
the full `To Unknown,Sender Unknown,Email,Folder Unknown`, which is the exact literal AC-4 in
`spec.md` requires the returned line to contain. The committed test therefore differs from the one
that produced the TRX recorded here by that one literal.

The recorded red state is unaffected, and the amendment is disclosed rather than hidden:

- The pre-fix failure is a `NullReferenceException` raised inside `GetMoveDiagnostics` at
  `QfcCollectionController.cs:2097`, i.e. inside the `Act` delegate. It is caught and reported by
  the first assertion, `act.Should().NotThrow(...)`. Execution never reaches the string assertion,
  so its literal cannot influence the pre-fix outcome.
- Widening a `Contain` assertion can only make a test stricter. It cannot convert a pre-fix failure
  into a pass.

The post-fix green state was re-established against the amended test: see
`p6-t5-pass-after.2026-08-26T10-22.md`, whose run directory retains both the pre-amendment TRX and
the authoritative post-amendment TRX.

## Host-identifier sanitisation

The TRX was sanitised case-insensitively before commit: 14 substitutions. This TRX carries more
substitutions than the two preceding ones because the recorded stack trace embeds the source path
of the throwing frame in addition to the assembly `storage=` attributes. Post-sanitisation the file
contains zero occurrences of any of the four host-identifier patterns recorded in
`evidence/other/host-identifier-sanitisation.2026-08-26T10-11.md`.
