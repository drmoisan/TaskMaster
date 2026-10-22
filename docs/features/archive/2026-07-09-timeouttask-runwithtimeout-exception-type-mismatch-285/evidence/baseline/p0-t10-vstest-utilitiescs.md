# P0-T10 — `UtilitiesCS.Test` Baseline Test Run with Cobertura Coverage

Timestamp: 2026-09-01T08-10 (test run started 2026-09-01T08-09)

## vstest Resolution

Resolved with:

```text
& "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe" -latest -products * -find "Common7\IDE\Extensions\TestPlatform\vstest.console.exe" | Select-Object -First 1
```

Resolved absolute path:
`C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe`

This is the resolved vstest path used by P0-T11, P1-T6, P2-T4, P2-T5, P3-T5, P3-T6, and P3-T7.

## Command

```text
dotnet-coverage collect --output coverage\p0-t10.cobertura.xml --output-format cobertura --settings coverage.config -- <resolved-vstest> UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation /Logger:trx /ResultsDirectory:TestResults\p0-t10 /TestCaseFilter:TestCategory!=LiveOutlook
```

The assembly was run **on its own**, not as part of an aggregate assembly list. The
`/TestCaseFilter:TestCategory!=LiveOutlook` operand was present, so no test requiring a live Outlook
process was executed.

EXIT_CODE: 0

## Output Summary

vstest's trailing summary, verbatim:

```text
Test Run Successful.
Total tests: 4770
     Passed: 4770
 Total time: 33.1633 Seconds
Code coverage results: coverage\p0-t10.cobertura.xml.
```

| Count | Value |
| --- | --- |
| Total tests | 4770 |
| **Passed** | **4770** |
| **Failed** | **0** |
| **Skipped** | **0** |

vstest omits the `Failed:` and `Skipped:` summary lines when those counts are zero. Confirmed
independently against the captured log: a line-anchored count of result lines beginning `Failed ` or
`Skipped ` returns 0, and the run header reads `Test Run Successful.` A `Passed` count equal to the
`Total tests` count corroborates both zeros.

## UtilitiesCS BASELINE_FAILURE_SET

**The UtilitiesCS BASELINE_FAILURE_SET is the EMPTY SET. Cardinality: 0.**

There are no failing test identities to enumerate. Phase 3's P3-T5 subtracts this empty set, so its
required post-change `Failed` count is an unqualified 0, and its `Passed`-count assertion reduces to
requiring a post-change `Passed` count of at least 4771 (the baseline 4770 plus the cardinality 0,
plus at least one — the new regression test).

This empty set is also consumed by P4-T11: because it is empty, AC11's literal `0 failures` wording
is satisfiable for the UtilitiesCS assembly with no `REMEDIATION-REQUIRED` entry arising from this
side.

## Coverage

`coverage\p0-t10.cobertura.xml` exists. Confirmed by direct file existence check after the run.

Root `<coverage>` element `line-rate` attribute: `0.7082975641163215`.

**Repository-wide baseline line coverage: 70.83%.**

This percentage is recorded as a reported figure only. This plan asserts no repository-wide
coverage-percentage threshold; see the plan's "Out of Scope for This Plan" section. The figure is
carried forward to P3-T7 for side-by-side comparison.

Note: the TRX log and the raw Cobertura file are working artifacts. `.gitignore` line 39 ignores
`[Tt]est[Rr]esult*/` and lines 144-145 ignore `coverage/*` except `coverage/.gitkeep`, so neither
enters the change footprint.

Acceptance: met. The artifact records all three integer test counts (4770 / 0 / 0) and one numeric
coverage percentage (70.83%), and `coverage\p0-t10.cobertura.xml` exists. The exit code was 0, so no
failing-test enumeration was required; the BASELINE_FAILURE_SET is recorded as empty.
