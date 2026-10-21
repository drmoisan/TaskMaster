# P0-T13 — Before Line Counts Of Every Existing PowerShell File This Delivery Edits

Timestamp: 2026-09-13T05-02
Task: [P0-T13]

Command: pwsh -NoProfile -Command 'Set-Location -LiteralPath <worktree>; foreach ($f in $files) { Write-Output ($f + " " + (Get-Content -LiteralPath $f).Count) }'
EXIT_CODE: 0

Each figure is the element count of the file's content lines, measured in the restored tree after the
P0-T10 format baseline, so it is directly comparable with every later measurement in this plan.

## Line counts, six lines, each a path followed by a bare integer

```
scripts/vscode/Invoke-MSTest.ps1 202
scripts/vscode/Invoke-MSTestWithCoverage.ps1 351
scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1 470
tests/scripts/vscode/Invoke-MSTest.RunSettings.Tests.ps1 496
tests/scripts/vscode/Invoke-MSTest.Main.Tests.ps1 144
tests/scripts/vscode/Invoke-MSTestWithCoverage.AssemblyDiscovery.Tests.ps1 99
```

## Output Summary

Six files measured. Every integer is at most 500, so the ceiling holds in the before state.
LARGEST_RECORDED_COUNT: 496
SMALLEST_RECORDED_COUNT: 99
CEILING_HEADROOM_HELPERS_FILE: 30
CEILING_HEADROOM_SHARED_ARGUMENT_BUILDER_TEST_FILE: 4

These are the before values AC20's ceiling check and the helpers growth check compare against. They
agree exactly with the `POST_FORMAT_BASELINE_LINE_COUNTS:` figures recorded in the P0-T10 artifact,
which is expected because the formatter rewrote nothing. Two consequences are recorded for later
phases: the helpers file may grow by at most one line under AC20, and the shared argument-builder
test file has only four lines of headroom, which is why the plan converts its call sites to splatting
rather than adding arguments in the existing line-continuation style.

EXIT_CODE: 0
