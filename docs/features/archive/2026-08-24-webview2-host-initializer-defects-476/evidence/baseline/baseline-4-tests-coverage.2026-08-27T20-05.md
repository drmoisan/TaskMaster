# Baseline 4 of 4 — Full Suite with Coverage ([P0-T11])

Timestamp: 2026-08-27T20-05

Command:
```
pwsh -NoProfile -File scripts\vscode\Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug -CoverageOutput docs\features\active\webview2-host-initializer-defects-476\evidence\baseline\coverage-baseline.cobertura.xml
```
(launched detached per the plan's Execution Conventions; PID 84656; stdout and stderr redirected to
scratchpad logs outside the repository)

EXIT_CODE: 0

Total Tests: 6701
Passed: 6701
Failed: 0
Skipped: 0
Line Rate: 0.851302
Branch Rate: 0.791973

## Failed Tests:

none

## Output Summary

- `Test Run Successful. Total tests: 6701 / Passed: 6701 / Total time: 48.0107 Seconds`. vstest
  emitted no `Failed:` and no `Skipped:` summary line, so both counts are zero.
- Nine test assemblies were discovered under the workspace root for configuration `Debug`:
  `QuickFiler.Test`, `SVGControl.Test`, `Tags.Test`, `TaskMaster.Test`, `TaskTree.Test`,
  `TaskVisualization.Test`, `ToDoModel.Test`, `UtilitiesCS.Test`, `VBFunctions.Test`.
- The runner drives all nine through one `dotnet-coverage collect` wrapping `vstest.console.exe`
  with `/Settings:scripts\vscode\TaskMaster.cli.runsettings`, `/InIsolation`, and
  `/TestCaseFilter:TestCategory!=LiveOutlook`, so no live Outlook process is launched. This
  satisfies the `vstest.console.exe ... /EnableCodeCoverage` toolchain step while emitting Cobertura
  with numeric rates.
- Coverage artifact written to
  `docs/features/active/webview2-host-initializer-defects-476/evidence/baseline/coverage-baseline.cobertura.xml`
  and post-processed for Koverage compatibility by the runner.

### Root `<coverage>` element attributes read verbatim

```
line-rate="0.851302" branch-rate="0.791973" complexity="25244" version="1.9"
lines-covered="54382" lines-valid="63881" branches-covered="12925" branches-valid="16320"
```

`Line Rate:` and `Branch Rate:` above are those two attribute values, not placeholders.

### Position of this baseline against each policy floor

This determines which branch of Decisions Record item 8 `[P4-T5]` applies.

| Floor | Source | Baseline figure | Baseline meets floor? |
| --- | --- | --- | --- |
| Line >= 80% | `CLAUDE.md` §UT2, `.claude/rules/csharp.md` | 85.1302% | Yes |
| Line >= 85% | `.claude/rules/general-unit-test.md`, `.claude/rules/quality-tiers.md` | 85.1302% | Yes |
| Branch >= 75% | `.claude/rules/general-unit-test.md`, `.claude/rules/quality-tiers.md` | 79.1973% | Yes |

Because the baseline meets every floor, a post-change repository-wide figure below any of them is
**blocking** at `[P4-T5]`; the non-blocking branch of Decisions Record item 8 does not apply to any
floor. The margin above the 85% line floor is 0.13 percentage points (85.1302% - 85%), so removing
the class-level coverage exemptions in Phase 3 must not add a materially uncovered denominator.

The `CLAUDE.md` 80/90 versus `.claude/rules/general-unit-test.md` 85/75 threshold conflict is
recorded here and is reported again in the Phase 5 status summary as the plan requires.
