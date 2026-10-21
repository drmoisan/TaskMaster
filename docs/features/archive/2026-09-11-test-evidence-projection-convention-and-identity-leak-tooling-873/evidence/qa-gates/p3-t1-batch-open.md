# P3-T1 — Phase 3 PowerShell Change Batch Opened and Coverage Builder Extended

Timestamp: 2026-09-13T06-03
Task: [P3-T1]

## Batch open

Command: pwsh -NoProfile -Command "Set-Location -LiteralPath '<worktree>'; Get-ChildItem -Path '.claude/state/powershell-batch-budget.*.json' -ErrorAction SilentlyContinue | Where-Object Name -ne 'powershell-batch-budget.default.json' | Remove-Item -Force -ErrorAction SilentlyContinue; (Get-ChildItem -Path '.claude/state/powershell-batch-budget.*.json' -ErrorAction SilentlyContinue | Where-Object Name -ne 'powershell-batch-budget.default.json' | Measure-Object).Count"
EXIT_CODE: 0

A `Set-Location` was prepended inside the same command body, because the command's paths are
repository-relative and this executor's inherited working directory is not this worktree. No other
part of the command was altered.

### Printed integer

```
0
```

REMAINING_BATCH_BUDGET_STATE_FILE_COUNT: 0

The recorded integer is 0, so the batch was open before any edit in this task was attempted.

### Exclusion, restated as observed

`powershell-batch-budget.default.json` is excluded from both the deletion clause and the count
clause. A directory listing taken immediately after the reset reports it as the only file present,
so it was neither deleted nor counted:

```
STATE| powershell-batch-budget.default.json
```

It is tracked in this repository, and deleting it would stage a deletion that neither the Phase 7
inventory gate nor the Phase 7 clean-tree gate admits. The batch-budget hook resolves its state file
name from the session identifier and falls back to a worktree-derived name, so it never reads the
default-named file in this worktree.

## Builder extension

`Get-DotnetCoverageArgumentList` in `scripts/vscode/Invoke-MSTestWithCoverage.ps1` gained two
mandatory parameters, `ResultsDirectory` and `LogFileName`, and appends one results-directory switch
and one trx logger switch to the inner test-console segment, that is after the argument separator.

Command: pwsh -NoProfile -Command '<dot-source the coverage entry point, invoke Get-DotnetCoverageArgumentList with both new arguments, print every element with its index, then print the index of the separator, of each appended element and of each pre-existing inner element, and the mandatory flag of each new parameter>'
EXIT_CODE: 0

```
ARG[0]=collect
ARG[1]=--output
ARG[2]=C:\repo\coverage\coverage.cobertura.xml
ARG[3]=--output-format
ARG[4]=cobertura
ARG[5]=--settings
ARG[6]=C:\repo\coverage.config
ARG[7]=--
ARG[8]=C:\vstest.console.exe
ARG[9]=C:\repo\A.Test.dll
ARG[10]=/Settings:C:\repo\scripts\vscode\TaskMaster.cli.runsettings
ARG[11]=/InIsolation
ARG[12]=/TestCaseFilter:TestCategory!=LiveOutlook
ARG[13]=/ResultsDirectory:C:\repo\coverage\test-results
ARG[14]=/Logger:trx;LogFileName=mstest-coverage-run.trx
SEPARATOR_INDEX=7
RESULTS_DIRECTORY_INDEX=13
LOGGER_INDEX=14
SETTINGS_INDEX=10
INISOLATION_INDEX=11
TESTCASEFILTER_INDEX=12
BUILDER_RESULTSDIRECTORY_MANDATORY=True
BUILDER_LOGFILENAME_MANDATORY=True
```

The argument values above are the fabricated fixture values the probe supplied, not paths on this
machine.

### Acceptance mapping

- The parameter block declares both new parameters as mandatory:
  `BUILDER_RESULTSDIRECTORY_MANDATORY=True` and `BUILDER_LOGFILENAME_MANDATORY=True`.
- Index of each new element is strictly greater than the index of the argument separator element:
  13 > 7 and 14 > 7.
- The three pre-existing inner elements are unchanged in content and in relative order: the
  `/Settings:` element at index 10, `/InIsolation` at 11 and `/TestCaseFilter:` at 12, in that
  order, exactly as before this change. Nothing was removed, reordered or rewritten.
- `scripts/vscode/TaskMaster.cli.runsettings` was not edited. The switches were appended to the
  argument list only; the `/Settings:` element still names the same runsettings file.
- Verified by the tests P3-T6 adds, recorded in the P3-T6 artifact, rather than by reading the plan.

## Output Summary

EXIT_CODE: 0. The count of batch-budget state files excluding the default-named one is 0, so Phase
3's batch is open; the tracked default-named state file survives. The builder now declares
`ResultsDirectory` and `LogFileName` as mandatory and emits both switches at indices 13 and 14,
after the separator at index 7, with the three pre-existing inner elements unchanged at 10, 11 and
12.
