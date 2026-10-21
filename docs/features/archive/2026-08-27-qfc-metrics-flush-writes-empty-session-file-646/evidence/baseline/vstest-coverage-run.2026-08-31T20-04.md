# Baseline — vstest.console.exe with Code Coverage (P0-T10)

Timestamp: 2026-09-01T12-15

Working directory: repository root (worktree for branch
`bug/qfc-metrics-flush-writes-empty-session-file-646`)
HEAD: `8a2054cd6c857195712c7db6cee0a34b631f3ca7`

## Resolution of vstest.console.exe

`vstest.console.exe` is not on `PATH` in this environment and was resolved via `vswhere`,
exactly as the plan task specifies:

```
$vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'
$vstest  = & $vswhere -latest -products * -find 'Common7\IDE\Extensions\TestPlatform\vstest.console.exe' | Select-Object -First 1
```

## Command

Command: `& $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /EnableCodeCoverage`
EXIT_CODE: 0

This is the bare CUT3 form: the exact assembly path and the single `/EnableCodeCoverage`
flag, with no `/Settings`, no `/InIsolation`, and no `/TestCaseFilter` added. No fallback
form was needed; the run completed on the first attempt.

## Verbatim Printed Summary Lines

```
Test Run Successful.
Total tests: 1284
     Passed: 1284
 Total time: 13.9326 Seconds
```

(`vstest.console.exe` prints `Test Run Successful.` with a `Total tests:` / `Passed:`
block. The `Passed!` / `Failed!` single-line form named in the plan task is the `dotnet
test` spelling; the lines above are what this runner actually printed. No `Failed:` line
was printed, which `vstest.console.exe` omits when the failed count is zero.)

## Output Summary

Baseline test state for `QuickFiler.Test` is green: 1284 tests run, 1284 passed, 0 failed,
exit code 0, elapsed 13.93 s. This total is the floor that the P2-T5 final run must meet
or exceed. A code-coverage attachment was produced under `TestResults/` and is consumed by
P0-T11 to generate the baseline Cobertura report.

## Coverage Attachment

A single `.coverage` attachment was written under a GUID-named subdirectory of
`TestResults/` at the repository root. Its filename is machine- and account-derived and is
therefore not reproduced here; P0-T11 locates it by recency rather than by literal name.
`TestResults/` is excluded from version control by `.gitignore`, so it does not enter the
change footprint checked by P2-T8.
