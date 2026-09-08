# P0-T10 — Baseline toolchain step 4 (nine-assembly coverage run)

Timestamp: 2026-09-08T09-25
Task: [P0-T10]
Command: dotnet-coverage collect --output coverage/p0-baseline.cobertura.xml --output-format cobertura --settings coverage.config -- <vstest> <nine assemblies> /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation "/Logger:trx;LogFileName=p0-t10.trx" /ResultsDirectory:coverage/trx/p0-t10 /Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None "/TestCaseFilter:TestCategory!=LiveOutlook&FullyQualifiedName!~ShellUtilities&FullyQualifiedName!~SysImageListHelper&FullyQualifiedName!~OSBrowser"
EXIT_CODE: 0

This is the mandatory numeric coverage baseline. It uses the D15 argument shape: the repository
wrapper's own shape, issued directly rather than through `Invoke-MSTestWithCoverage.ps1`, because
that wrapper hard-codes its filter and cannot take the shell-icon exclusion, and because its
assembly discovery rejects every assembly under a dot-claude path when run from an agent worktree.

## TRX counters

`coverage/trx/p0-t10/p0-t10.trx` exists.

```
total=7153 executed=7153 passed=7153 failed=0 error=0 timeout=0 aborted=0 notExecuted=0
```

PREEXISTING_FAILURE_SET: none

`failed` + `error` + `aborted` + `timeout` = 0. No test failed on this run, including the two
sentinels this item repairs:
`DictionaryExtensions_Tests.TryAddValuesAsync_UpdatesExistingValue` and
`DfDeedle_COM_Tests.GetEmailDataInViewAsync_SeparatesTableSnapshotFromDataFrameTransform`. Both
passed. That is the expected shape of a load-dependent defect: the failures are intermittent, not
deterministic, which is exactly why AC1 has no deterministic RED (D6) and why AC4 requires ten
consecutive runs rather than one. A green baseline is not evidence that the defect is absent.

## Root Cobertura figures

Read from the root element of `coverage/p0-baseline.cobertura.xml`. `line-rate` and `branch-rate`
are fractions, not percentages.

| Attribute | Value |
|---|---|
| `line-rate` | `0.8601092896174863` |
| `lines-covered` | `172353` |
| `lines-valid` | `200385` |
| `branch-rate` | `0.6630576006929406` |
| `branches-covered` | `21434` |
| `branches-valid` | `32326` |

Line coverage is 86.01 percent repository-wide on the merged denominator.

## Wall clock

RUN_SECONDS: 64

vstest reported `Total time: 1.0297 Minutes`. The #798 baseline measured 54.5 s for the same
nine-assembly shape, so this run is consistent with it.

## Acceptance evaluation

- `EXIT_CODE: 0` recorded; the run had no failing test, so the zero-failure branch of the
  acceptance applies and no `PREEXISTING_FAILURE_SET:` entries exist. PASS
- `coverage/trx/p0-t10/p0-t10.trx` exists and its counters are recorded in the
  `total= executed= passed= failed= error= timeout= aborted= notExecuted=` form. PASS
- `failed` + `error` + `aborted` + `timeout` equals 0. PASS
- `coverage/p0-baseline.cobertura.xml` exists and all six root figures are recorded as numbers.
  PASS

## Output Summary

Nine-assembly `/InIsolation` coverage baseline: 7153 tests, 7153 passed, 0 failed, 64 s wall
clock, exit 0. Repository-wide line coverage 0.8601 (172353 of 200385 lines); branch coverage
0.6631 (21434 of 32326 branches). Raw Cobertura and TRX documents stay under the gitignored
`coverage/` directory and are not committed (D15).
