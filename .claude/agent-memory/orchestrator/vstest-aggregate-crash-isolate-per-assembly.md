---
name: vstest-aggregate-crash-isolate-per-assembly
description: A "Test host process crashed" abort in the aggregate 9-assembly vstest run is environmental, not a failure — re-run per assembly with /InIsolation to get the real verdict
metadata:
  type: project
---

The repo's aggregate test command (`scripts/vscode/Invoke-MSTestWithCoverage.ps1`, all 9 `*.Test.dll`
in one vstest process) intermittently aborts with:

```
The active test run was aborted. Reason: Test host process crashed
Test Run Aborted.  Total tests: Unknown
```

On #505 this happened **twice at different points** (1476 and 1840 tests in). It is not a test
failure and `Total tests: Unknown` means you cannot read a verdict out of it at all.

**The decisive check is per-assembly isolation.** Loop the 9 assemblies through
`vstest.console.exe <dll> /InIsolation`. On #505 every one passed:

| assembly | result |
|---|---|
| QuickFiler.Test | 903/903 |
| SVGControl.Test | 75/75 |
| Tags.Test | 65/65 |
| TaskMaster.Test | 367 passed + 1 skipped of 368 |
| TaskTree.Test | 51/51 |
| TaskVisualization.Test | 163/163 |
| ToDoModel.Test | 122/122 |
| UtilitiesCS.Test | 4688/4688 |
| VBFunctions.Test | 1/1 |

Total **6435 passed, 1 skipped, 0 failures** — a clean green that the aggregate run could not report.

**Why:** the instability is load-driven and concentrated in the `QuickFiler.Test`
`WinFormsPumpHost` message-pump family, which drives a real WinForms pump against a real
`ItemViewer` (tracked as **#511**). Resident MSBuild `/m` node-reuse workers from the preceding
rebuilds saturate the box; killing them makes the same isolated tests pass 4/4. A sibling symptom is
`InvalidOperationException: Invoke or BeginInvoke cannot be called on a control until the window
handle has been created`.

**How to apply.**
- Do not report an aggregate-run crash as a blocking test failure. Isolate first, then judge.
- Reach for `Get-Process MSBuild,vstest.console,testhost,dotnet-coverage | Stop-Process -Force` and
  `/nodeReuse:false` on the preceding builds to remove the cause rather than adding retries or
  sleeps to tests.
- Scope the blame properly before accepting it as yours: `QuickFiler.csproj` does **not** reference
  `TaskMaster`, so a `TaskMaster`-only change cannot reach those tests. Check the csproj reference
  graph before treating a cross-assembly failure as a regression.
- Discovery filter caveat still applies — filter on the path **relative** to the worktree root; see
  [[project_agent_worktree_discovery_and_evidence_hygiene]]. 9 assemblies is correct; 0 is a filter bug.
