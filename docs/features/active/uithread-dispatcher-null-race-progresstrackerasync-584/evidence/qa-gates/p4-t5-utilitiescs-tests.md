# P4-T5 — UtilitiesCS.Test run with Cobertura coverage (second pass)

Timestamp: 2026-09-03T21-52

Command:
```text
env -C <worktree-root> MSYS_NO_PATHCONV=1 dotnet-coverage collect --output coverage/p4-t5.cobertura.xml --output-format cobertura --settings coverage.config -- "<resolved-vstest-dir-native>\vstest.console.exe" UtilitiesCS.Test/bin/Debug/UtilitiesCS.Test.dll /Settings:scripts/vscode/TaskMaster.cli.runsettings /InIsolation /Logger:trx /ResultsDirectory:TestResults/p4-t5 /TestCaseFilter:TestCategory!=LiveOutlook
```

The flag set is identical to P0-T10's; the two differ only in the `--output` filename and the
`/ResultsDirectory` value. `/EnableCodeCoverage` is deliberately absent from both.

EXIT_CODE: 0

## Output Summary

### Test counts

Console summary block, verbatim:

```text
Test Run Successful.
Total tests: 4787
     Passed: 4787
```

- **Total tests: 4787** (console summary block)
- **Passed: 4787** (console summary block)
- **Failed: 0** (read from the `failed` attribute of the single `<Counters .../>` element in the TRX
  written under `TestResults/p4-t5/` by this task's own `/Logger:trx` switch)
- **Skipped: 0** (derived as `total` minus `executed`)

TRX `<Counters .../>` values used for the derivation:

- `total` = **4787**
- `executed` = **4787**
- `failed` = **0**
- derived `Skipped` = 4787 - 4787 = **0**

The `notExecuted` attribute was NOT used, per constraint 5 of "Shell constraints measured in this
worktree"; the TRX logger hard-codes it to `0` regardless of the run's outcome.

TRX SELECTED: most recently modified .trx in TestResults/p4-t5/
Last-modified timestamp of the selected file: `2026-09-03 21:52:13.419159700 -0400`.
That directory held two `.trx` files at the moment this artifact was written (the one written by the
first Phase 4 pass on 2026-09-03 and the one this second pass produced), so the TRX selection rule
stated after constraint 5 applies. The selected file's own name is deliberately not recorded, and the
run's `Results File:` console line is deliberately not quoted, because `vstest.console.exe` composes
the default TRX filename from the host account name and the machine name and prints it inside a full
absolute host path.

FAILING_TEST_SET: empty.

### Post-change coverage figures

Read from the root `<coverage>` element of `coverage/p4-t5.cobertura.xml`:

- `lines-covered` = **105935**
- `lines-valid` = **149761**
- `line-rate` = **0.7073603942281368**

Supporting attributes on the same element, recorded for completeness: `branch-rate`
= 0.46792920353982304, `branches-covered` = 13219, `branches-valid` = 28250, `complexity` = 31682.

These are the raw unstripped `dotnet-coverage` figures for the `UtilitiesCS.Test` process and are not
the repository first-party figure CLAUDE.md's 80% refers to. They are the post-change figures P4-T7
compares against the P0-T10 baseline.

## Acceptance

Satisfied on all four clauses:

1. The failing-test set is empty, which is a subset of the empty `BASELINE_FAILURE_SET` recorded in
   P0-T10, with no new member.
2. `Total tests` is 4787. The baseline `Total tests` recorded in P0-T10 is 4785, so 4787 is greater
   than or equal to 4785 + 2. The two added tests are the pair P1-T2 added to
   `UtilitiesCS.Test/Threading/UiThread_Tests.cs`.
3. The `total` (4787) and `executed` (4787) values from which `Skipped` was derived are recorded.
4. All three coverage attribute values are recorded as concrete numbers.
