# P0-T10 — UtilitiesCS.Test baseline run with Cobertura coverage

Timestamp: 2026-09-03T08-27

Command:
```text
env -C <worktree-root> MSYS_NO_PATHCONV=1 dotnet-coverage collect --output coverage/p0-t10.cobertura.xml --output-format cobertura --settings coverage.config -- "<resolved-vstest-dir-native>\vstest.console.exe" UtilitiesCS.Test/bin/Debug/UtilitiesCS.Test.dll /Settings:scripts/vscode/TaskMaster.cli.runsettings /InIsolation /Logger:trx /ResultsDirectory:TestResults/p0-t10 /TestCaseFilter:TestCategory!=LiveOutlook
```

EXIT_CODE: 0

## Output Summary

### Test counts

Console summary block, verbatim:

```text
Test Run Successful.
Total tests: 4785
     Passed: 4785
 Total time: 13.5747 Seconds
```

- **Total tests: 4785** (console summary block)
- **Passed: 4785** (console summary block)
- **Failed: 0** (read from the `failed` attribute of the single `<Counters .../>` element in the TRX
  written under `TestResults/p0-t10/`)
- **Skipped: 0** (derived as `total` minus `executed`)

TRX `<Counters .../>` values used for the derivation:

- `total` = **4785**
- `executed` = **4785**
- `failed` = **0**
- derived `Skipped` = 4785 - 4785 = **0**

The `notExecuted` attribute was NOT used, per constraint 5 of "Shell constraints measured in this
worktree"; the TRX logger hard-codes it to `0` regardless of the run's outcome.

TRX SELECTED: most recently modified .trx in TestResults/p0-t10/
Last-modified timestamp of the selected file: `2026-09-03 08:27:05.113345500 -0400`.
That directory held two `.trx` files at the moment this artifact was written (an earlier one dated
2026-09-02 from a prior preparation-cycle run, and the one this task produced), so the TRX selection
rule stated after constraint 5 applies. The selected file's own name is deliberately not recorded,
and the run's `Results File:` console line is deliberately not quoted, because
`vstest.console.exe` composes the default TRX filename from the host account name and the machine
name and prints it inside a full absolute host path.

BASELINE_FAILURE_SET: empty. `Failed` is 0, so the comparison target for P3-T3 and P4-T5 is the empty
set.

### Baseline coverage figures

Read from the root `<coverage>` element of `coverage/p0-t10.cobertura.xml`:

- `lines-covered` = **105901**
- `lines-valid` = **149719**
- `line-rate` = **0.7073317347831605**

Supporting attributes on the same element, recorded for completeness: `branch-rate`
= 0.4679954683849041, `branches-covered` = 13219, `branches-valid` = 28246, `complexity` = 31677.

These three values are the **baseline coverage figures** referred to by P0-T12, P4-T5, and P4-T7.
They are the raw unstripped `dotnet-coverage` figures for the `UtilitiesCS.Test` process and are not
the repository first-party figure CLAUDE.md's 80% refers to.

## Acceptance

All four test counts and all three coverage attribute values are recorded as concrete numbers, not
placeholders. The `total` and `executed` values from which `Skipped` was derived are recorded.
`TestResults/p0-t10/` is identified as the results directory `Failed` and `Skipped` were read from,
without recording a TRX filename and without quoting a `Results File:` line.
