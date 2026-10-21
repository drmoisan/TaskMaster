# QA Gate 4 of 4 — Full Suite with Coverage ([P4-T4], post-base-merge re-run)

Timestamp: 2026-08-27T23-17

Command:
```
pwsh -NoProfile -File scripts\vscode\Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug -CoverageOutput docs\features\active\webview2-host-initializer-defects-476\evidence\qa-gates\coverage-postchange.cobertura.xml
```
(run from the workspace root in the foreground; stdout and stderr tee'd to a scratchpad log outside
the repository)

EXIT_CODE: 0

Total Tests: 6734
Passed: 6734
Failed: 0
Skipped: 0
Line Rate: 0.851435
Branch Rate: 0.792018

## Output Summary

- `Test Run Successful. Total tests: 6734 / Passed: 6734 / Total time: 40.6723 Seconds`. vstest
  emitted no `Failed:` and no `Skipped:` summary line and the log contains zero `Failed ` result
  lines, so both counts are zero.
- Because `Failed: 0` and `EXIT_CODE: 0`, the first branch of the acceptance is satisfied and no
  `Pre-Existing Failures:` section is required. The baseline-comparison clause of `[P4-T4]` is not
  invoked.
- The same nine test assemblies the Phase 0 baseline discovered were discovered again:
  `QuickFiler.Test`, `SVGControl.Test`, `Tags.Test`, `TaskMaster.Test`, `TaskTree.Test`,
  `TaskVisualization.Test`, `ToDoModel.Test`, `UtilitiesCS.Test`, `VBFunctions.Test`.
- The wrapper drives all nine through one `dotnet-coverage collect` wrapping `vstest.console.exe`
  with `/Settings:scripts\vscode\TaskMaster.cli.runsettings`, `/InIsolation`, and
  `/TestCaseFilter:TestCategory!=LiveOutlook`. `vstest.console.exe` was not invoked directly, so no
  live Outlook process was launched and the run stays comparable with the Phase 0 baseline.
- Test count moved from 6701 (Phase 0 baseline) to 6734, a rise of 33. Fifteen of those are this
  feature's own tests; the remaining eighteen arrived with the base merge of the integration branch
  at `9cb2c4f6`, which brought in the merged sibling features' tests. No test present in the Phase 0
  baseline is missing from this run's pass set, because both runs report zero failures and zero
  skips.
- No tracked source file was rewritten by this step. `git status --porcelain` taken immediately
  afterwards reported only the coverage artifact this command emits, the plan file's check-off edits,
  and the three preceding gate artifacts.

### Root `<coverage>` element attributes read verbatim

```
line-rate="0.851435" branch-rate="0.792018" complexity="25287" version="1.9"
lines-covered="54514" lines-valid="64026" branches-covered="12959" branches-valid="16362"
```

`Line Rate:` and `Branch Rate:` above are those two attribute values, not placeholders. The
denominator measured here is the **unfiltered repository-wide** Cobertura denominator — every
`<package>` the collector emitted, including vendored and generated code — which is the same
denominator the Phase 0 baseline recorded, so the two figures are directly comparable.
