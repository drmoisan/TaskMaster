---
name: project-873-evidence-projection-plan-seams
description: Preflight revision seams for issue #873 (test-evidence projection convention and identity-leak tooling) - It-level mock overrides, a tracked state file a reset glob would delete, and a hits attribute that throws when absent
metadata:
  type: project
---

Round-2 preflight seams for issue #873's plan. Each was a sibling of a round-1 edit, not a defect in the round-1 edit itself.

**A Pester describe block can carry more mock overrides than the `BeforeEach`.** In `tests/scripts/vscode/Invoke-MSTest.RunSettings.Tests.ps1`, `Describe 'Invoke-MSTestWithCoverageMain'` mocks `ConvertTo-KoverageCoberturaXml` in three places: the `BeforeEach`, an It-level override inside the test that pins the returned string in its own assertion, and a second It-level override on the sub-threshold path. A plan task that says "update the mocked post-processor return value" reaches only the `BeforeEach`. Enumerate every override in the block and state a disposition for each.
**Why:** the pinned-string override runs the full entry point, so a fixture the new reconciliation assertion requires must be present there too, and its own assertion pins the old string.
**How to apply:** when a plan changes a mocked return value, grep the whole file for that mock name and count the occurrences before writing the task.

**`Get-CoberturaClassLineSummary` throws on a line with no `hits` attribute.** `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1` does `[int]$lineNode.GetAttribute('hits')`; `GetAttribute` returns an empty string for a missing attribute and `[int]''` throws under `Set-StrictMode -Version Latest`. A fixture spec that says "four of five lines carry a nonzero hits attribute" is ambiguous about the fifth and, read as "the fifth omits it", is unrunnable.
**How to apply:** spell an uncovered fixture line as carrying `hits` with the value 0, never as omitting the attribute.