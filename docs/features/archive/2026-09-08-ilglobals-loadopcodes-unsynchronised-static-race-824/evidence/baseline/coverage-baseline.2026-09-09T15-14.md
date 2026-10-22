# Baseline coverage run (Issue #824, task P0-T11)

Timestamp: 2026-09-09T15-14

Command: `pwsh -NoProfile -Command 'Set-Location "<worktree-root>"; pwsh -NoProfile -File ./scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug -CoverageOutput coverage/baseline.cobertura.xml 2>&1 | Tee-Object -FilePath coverage/coverage-baseline.log | Select-Object -Last 30'`

EXIT_CODE: 0

## Coverage Command Of Record

The primary command above is the **Coverage Command Of Record** for this plan. It terminated
normally, so the D8 harness-liveness branch was not taken and the P0-T12 contingency form was not
authored. P5-T8 must re-run this same form byte-for-byte, varying only the output document path
(`coverage/post-change.cobertura.xml`) and the console log path
(`coverage/coverage-post-change.log`), so the two runs measure the same population.

The recorded command string above is the form as issued, including the `Set-Location` prefix
required because the executing session's process working directory is a different worktree, and the
trailing `| Select-Object -Last 30` that bounds the console transcript. `Tee-Object` writes the
complete stream to `coverage/coverage-baseline.log` before that stage, so the derivations below read
the full log.

## Output Summary

Document-level coverage, read from `coverage/baseline.cobertura.xml`:

| Attribute | Value | As a percentage |
|---|---|---|
| `/coverage/@line-rate` | 0.856204 | 85.6204 % |
| `/coverage/@branch-rate` | 0.79807 | 79.807 % |

The runner additionally printed its own first-party figure:
`First-party coverage: lines 56030/65440 (85.62%), branches 13481/16892 (79.81%)`.

85.6204 % is at or above the 80 percent document-level floor the runner enforces
(`scripts/vscode/Invoke-MSTestWithCoverage.Threshold.ps1:52-54`) and at or above the 85 percent
figure in `.claude/rules/general-unit-test.md`. The blocked branch of this task was therefore not
taken.

Test counts, derived by the rule this task states, because the tool does not print all four values
on its success path:

| Count | Value | Source |
|---|---|---|
| Total | 7210 | read from the line `Total tests: 7210`; the count of lines matching `^\s*Total tests:` is 1 |
| Passed | 7210 | read from the line `Passed: 7210`; the count of lines matching `^\s*Passed:` is 1 |
| Failed | 0 | no line matching `^\s*Failed:` is present (count 0) and the log contains `Test Run Successful.` |
| Skipped | 0 | no line matching `^\s*Skipped:` is present (count 0) |

Terminal console lines:

```
Test Run Successful.
Total tests: 7210
     Passed: 7210
 Total time: 37.5738 Seconds
Post-processing coverage XML for Koverage compatibility...
First-party coverage: lines 56030/65440 (85.62%), branches 13481/16892 (79.81%)
```

Baseline failing set: **empty**. The failed count is 0, so no failing test is enumerated and the
`UtilitiesCS.Test/NewtonsoftHelpers/SDILReader/` halt condition does not apply. P5-T8 compares its
own failing set against this empty set.

`coverage/baseline.cobertura.xml` exists. It is not copied into the feature folder: `coverage/*` is
gitignored at `.gitignore:144` and a full-repository Cobertura document for this solution is on the
order of 10 MB. Per plan D7 and D22 the committed coverage evidence is the compact markdown extract
written by P0-T13.

The D15 TRX command does not apply to this task: the runner passes no `/Logger:trx` (its inner
argument list at `scripts/vscode/Invoke-MSTestWithCoverage.ps1:76` supplies only `/Settings:`,
`/InIsolation` and `/TestCaseFilter:`) and `scripts/vscode/TaskMaster.cli.runsettings` configures no
logger, so the console summary is the only source of the counts.

The baseline was captured once and was not re-run to obtain a different figure.
