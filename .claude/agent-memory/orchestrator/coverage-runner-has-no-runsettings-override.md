---
name: coverage-runner-has-no-runsettings-override
description: The Deedle/ClassLevel-parallelism defect has an ASYMMETRIC remedy - a plan's direct vstest spans can be fixed plan-side, but any task invoking Invoke-MSTestWithCoverage.ps1 cannot, because its param block exposes no runsettings override
metadata:
  type: project
---

`repo-coverage-runner-parallelism-poisons-deedle` records the defect and says "do not put
`/Settings:<TaskMaster.cli.runsettings>` in a plan's vstest spans". That guidance is only *half*
actionable, and the half that is not is the half that blocks a delivery. Count your exposures before
deciding whether you are blocked.

**The split.** On issue #872 the plan had ten references to the affected runsettings:

- **Six direct `vstest.console.exe` spans** that appended `/Settings:scripts\vscode\TaskMaster.cli.runsettings`
  themselves. These ARE fixable plan-side: delete the switch from the span. Nothing else in the span
  changes, the pinned `/TestCaseFilter:` stays byte-identical between phases, and parallelism affects
  which tests pass rather than how many are discovered — so a per-assembly count delta gate survives
  the edit untouched.
- **Two tasks invoking `scripts/vscode/Invoke-MSTestWithCoverage.ps1`.** These are NOT fixable
  plan-side. Its top-level `param()` block exposes only `SearchRoot`, `Configuration`,
  `CoverageOutput` and `NoExecute`. The runsettings path is resolved internally by
  `Resolve-RunSettingsPath -ScriptRoot $PSScriptRoot` and appended at line 76. There is no override
  parameter, so the only routes are editing the runner or editing the runsettings file.
- (The other two of the ten were dot-sources of the sibling `Invoke-MSTestWithCoverage.Helpers.ps1`,
  which only supplies `Get-CoberturaClassLineSummary` and executes nothing. Do not count those as
  exposures.)

**Why the runner tasks are hard-blocked rather than merely risky.** The runner `throw`s at line 236 on
any non-zero test exit, so the three Deedle failures do not degrade the coverage figure — they abort the
task. It also hard-codes `/TestCaseFilter:TestCategory!=LiveOutlook` with no extension point, so it
cannot exclude the four shell-icon test classes that stall vstest on this workstation
(see [[project_local_shell_icon_tests_hang_shgetfileinfo]]). Two independent failure modes, neither
avoidable from the call site.

**How to apply.** When a coordinator forbids editing the runner and the runsettings file — the correct
prohibition, since both are outside any item's Write Set — that prohibition does NOT make you blocked
outright. Fix the direct spans, which is where most exposure usually lives, and report only the
runner-invoking tasks as the residual. Removing `/Settings:` from a direct span is repairing an
acceptance condition that *cannot pass*, which is the mirror of the plan-gate rule about conditions that
cannot fail, and is inside an orchestrator's remit rather than a scope change. Corroborating signal: a
correct reference form handed down from another item (#816) carried
`/EnableCodeCoverage /InIsolation "/Logger:trx;LogFileName=..."` and no `/Settings:` at all.

The underlying repository defect still deserves its own issue. See
[[invoke-mstest-with-coverage-three-traps]] for the runner's other hazards.
