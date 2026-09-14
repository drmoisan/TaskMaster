# Batch 1 — PowerShell test step (P2-T7)

Timestamp: 2026-09-14T18-45

## MCP invocation

Tool: `mcp__drm-copilot__run_poshqc_test`
Workspace root: the item worktree root.
Scan folders: omitted, so the scan set resolved from `config/poshqc-scan.json`.

Returned excerpt:

```
ok: false
summary: Command exited with code 1.
```

The MCP payload carries no counts, no test names and no verdict beyond that exit code, so it cannot satisfy this task's acceptance on its own. This task's acceptance is judged on the printed `PESTER Passed=` line from the paired direct run below, which is why the plan requires both invocations.

## Paired direct run — final clean pass

Command: `pwsh -NoProfile -Command '<worktree prologue>; Import-Module Pester -MinimumVersion 5.0.0 -Force; $c = New-PesterConfiguration; $c.Run.Path = "tests/scripts/vscode"; $c.Run.PassThru = $true; $c.Output.Verbosity = "Detailed"; $r = Invoke-Pester -Configuration $c; "PESTER Passed=" + $r.PassedCount + " Failed=" + $r.FailedCount + " Skipped=" + $r.SkippedCount + " Total=" + $r.TotalCount'`
EXIT_CODE: 0

Printed result line, verbatim:

```
PESTER Passed=143 Failed=0 Skipped=0 Total=143
```

Failed count: **0**.

The exit code is not used as the pass signal, because Pester does not exit non-zero on a failing test case.

## Total-count check

Baseline total recorded in the P0-T8 artifact: **133**.
Total on this run: **143**.
Difference: **+10**, exactly the value required.

The ten cases are: the seven branch cases and the one out-of-range line case added by P1-T1, plus the two call-site wiring cases added by P1-T2. No case was removed; the count moved by exactly the number added.

## Deviation recorded: a third fixture repair outside the declared write set

The first execution of this gate returned `PESTER Passed=142 Failed=1 Skipped=0 Total=143` with a single failure:

```
FAILED: discards only after the threshold assertion, the projection write and the reconciliation assertion
```

That case is at line 203 of `tests/scripts/vscode/Invoke-MSTestWithCoverage.ResultsDirectory.Tests.ps1`. Its `BeforeEach` mocks `Assert-CoberturaLineCoverageThreshold` but not the new branch assertion, and the post-processed document it supplies, a here-string assigned to `$script:postProcessedCoverageXml` at lines 42 to 44 of that file, carried no `branch-rate` attribute. The real `Assert-CoberturaBranchCoverageThreshold` therefore threw `Cobertura branch-rate is missing.`

This is the identical defect class that P2-T3 and P2-T4 repair in two sibling files, in a **third** file that neither task names and that the plan's declared write set does not list. The file arrived in this worktree with the merged evidence-projection item, after the plan's write set was fixed, which is why the plan's survey of affected mocked documents did not reach it.

Action taken, and its justification. The executor is past preflight, so under the execution protocol it completes the plan as written and escalates at completion rather than blocking. The repair applied is the minimum that satisfies this task's stated acceptance and is byte-for-byte the same transformation the plan prescribes for the two sibling files: the two attributes `branch-rate="0.8"` and `branches-valid="10"` were **added** to the root `coverage` element of the existing literal, in place, with every existing attribute and the whole `packages` subtree preserved. Preserving `lines-covered` and `lines-valid` is load-bearing for the same reason the plan states for the sibling repairs: the entry point's `Assert-JacocoProjectionReconciliation` compares the projection's summed LINE counters against exactly those two root attributes. The two added attributes cannot disturb that comparison, because the reconciliation reads no branch attribute. The file remained at 268 lines, so the substitution added no line.

**This deviation is escalated in the final report.** The declared write set in both the plan and the specification will need to gain `tests/scripts/vscode/Invoke-MSTestWithCoverage.ResultsDirectory.Tests.ps1`, and P10-T14's write-set verification will observe that path in the anchored diff.

## Toolchain loop restart

Because the test step changed a file, the toolchain loop was restarted rather than continued:

1. `mcp__drm-copilot__run_poshqc_format` re-run over both scan folders: `ok: true`. The repaired file's SHA-256 prefix after the call is `DD0C630F65FF27A0` and its line count is 268, unchanged by the formatter.
2. `Invoke-ScriptAnalyzer` re-run over both scan folders: `ANALYZERCOUNT=16`, equal to the P0-T12 baseline of 16 and unchanged by the repair.
3. The whole-suite Pester run re-run: `PESTER Passed=143 Failed=0 Skipped=0 Total=143`.

All three steps passed in that single final pass.

## Pre-fix side effect

Command: `git -C "<repo-root>" status --porcelain=v1 -- '*.csproj'`
EXIT_CODE: 0
Output verbatim: empty.

No project file was modified by the suite run, so no restoration was required and the restoration branch of this task did not fire. The run's own console output shows the still-unguarded build script executing on dot-source — `Using MSBuild: ...` and `Sync-PackageReferences: All HintPaths are up to date` — which is the pre-fix behaviour this delivery removes in Phase 4. On this run the sync script's fix count was zero, so its file-write call was never reached.

Output Summary: 143 tests, 0 failures, 0 skips, total exactly 10 greater than the 133 baseline. One additional fixture repair outside the declared write set was required and is escalated. The format, analyze and test steps all passed in a single final pass, and no project file was written.
