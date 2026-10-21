# [P10-T2] Full `QuickFiler.Test` suite at the issue #471 `ShrinkByRows` seam

Timestamp: 2026-08-26T11-13

Command:

```
$vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'
$vstest  = & $vswhere -latest -products * -find 'Common7\IDE\Extensions\TestPlatform\vstest.console.exe' | Select-Object -First 1
& $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll `
    /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation `
    /Logger:"trx;LogFileName=p10-t2.trx" `
    /ResultsDirectory:docs\features\active\qfc-collection-controller-defects-468\evidence\qa-gates\p10-t2
```

EXIT_CODE: 0

ExpectedExitCode: 0

## Output Summary

`Test Run Successful. Total tests: 958  Passed: 958`. Total time 9.79 s, first attempt, no flaky
retry.

TRX `<Counters>`, verbatim from `evidence/qa-gates/p10-t2/p10-t2.trx`:

```
total="958" executed="958" passed="958" failed="0" error="0" timeout="0" aborted="0" inconclusive="0"
```

## Seam neutrality (the point of this task)

| Run | Total | Passed | Failed |
|---|---|---|---|
| P9-T4 (immediately prior full suite) | 958 | 958 | 0 |
| P10-T2 (this run, at the seam) | 958 | 958 | 0 |

The passed count is **identical** to the P9-T4 run and the failed count is exactly `0`. The
`ShrinkByRows` extraction therefore changed no observable behaviour, which is what D8 requires of
the seam commit.

## Why the seam is arithmetically identical

Before the seam the removal path computed:

```
heightChange = -(int)Math.Round(_template.Height * removalCount, 0);
newHeight    = oldHeight - heightChange;
```

which expands to `oldHeight + Round(templateHeight * removalCount)`.

After the seam it computes `ShrinkByRows(size, _template.Height, -removalCount)`, whose body is
`oldHeight - Round(templateHeight * -removalCount)`, i.e. the same value.
`Math.Round` with the default `MidpointRounding.ToEven` is symmetric about zero, so
`Round(-x) == -Round(x)` for every input and the two forms agree bit-for-bit, not merely
approximately.

The insertion path previously computed `oldHeight + Round(templateHeight * insertCount)` and now
computes `ShrinkByRows(size, _template.Height, -insertCount)`, again the same value. The insertion
site's negative argument is permanent and carries an in-code comment; only the removal site's
argument is corrected later, by P10-T8.

## Toolchain state at this run

| Step | Command | Result |
|---|---|---|
| Format | `dotnet tool run csharpier format QuickFiler/Controllers/QfcCollectionController.cs` | `EXIT_CODE 0`, 1 file processed |
| Build | `pwsh -NoProfile -File scripts/vscode/Invoke-VSBuild.ps1 -Target Build` | `EXIT_CODE 0`, 0 errors, 5 pre-existing warnings |
| Test | this run | `EXIT_CODE 0`, 958 passed, 0 failed |

## Host-identifier sanitisation

The TRX was sanitised **case-insensitively in binary mode** before commit: 2,881 substitutions.
vstest writes the `storage=` attribute of every `<UnitTest>` in all-lower-case, so a
case-sensitive pass would silently miss one path per test; the substitution used
`re.IGNORECASE` over the raw bytes for exactly that reason. Post-sanitisation the file contains
zero occurrences of any of the token classes recorded in
`evidence/other/host-identifier-sanitisation.2026-08-26T10-11.md` (account name, machine name,
`Users` absolute-path prefix in either slash direction, and the 8.3 short-name form). No raw
"before" token is reproduced here.
