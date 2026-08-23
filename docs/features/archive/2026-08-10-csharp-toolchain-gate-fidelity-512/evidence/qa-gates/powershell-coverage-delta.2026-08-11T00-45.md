# PowerShell coverage comparison ([P6-T4])

Timestamp: 2026-08-11T00-45
Command: (none — analysis artifact; figures sourced from [P0-T16] and [P6-T3], plus the changed-line probe `pwsh -NoProfile -ExecutionPolicy Bypass -File coverage/check-changed-line-coverage.ps1`)
EXIT_CODE: (none — analysis artifact; the changed-line probe returned 0)

Sources:
- Baseline: `FEATURE/evidence/baseline/baseline-poshqc-test.2026-08-10T23-08.md`
- Post-change: `FEATURE/evidence/qa-gates/final-poshqc-test.2026-08-11T00-40.md`

Scope: `scripts/vscode/Invoke-VSBuild.ps1` — the only production PowerShell file this feature
changes.

## 1. Line coverage: baseline vs post-change

| Metric | Baseline ([P0-T16]) | Post-change ([P6-T3]) | Delta |
|---|---|---|---|
| **Line coverage** | **85.71%** | **85.71%** | **0.00 pp** |
| Commands analyzed | 49 | 49 | 0 |
| Commands executed | 42 | 42 | 0 |
| Commands missed | 7 | 7 | 0 |

**No regression against the baseline.** Post-change line coverage (85.71%) is `>=` the recorded
baseline (85.71%).

**Policy floor:** `.claude/rules/powershell.md` line 63 requires line coverage `>= 85%`. Measured
**85.71% >= 85%** — **PASS**.

`FEATURE/evidence/baseline/baseline-poshqc-test.2026-08-10T23-08.md` records **no**
`PREEXISTING_COVERAGE_SHORTFALL:` marker, because the measured baseline meets the floor. The
conditional remediation-required branch of [P6-T4]'s acceptance is therefore **not** engaged.

## 2. Branch coverage

`.claude/rules/powershell.md` line 64 requires branch coverage `>= 75%`.

**Branch coverage is structurally unavailable from this runner.** Evidence:

- Pester version: **5.6.1**.
- `PesterConfiguration.CodeCoverage` property list, enumerated at run time:
  `Enabled, OutputFormat, OutputPath, OutputEncoding, Path, ExcludeTests, RecursePaths,
  CoveragePercentTarget, UseBreakpoints, SingleHitBreakpoints`.
- Result-object `CodeCoverage` property list, enumerated at run time:
  `CoveragePercent, CoveragePercentTarget, CoverageReport, CommandsAnalyzedCount,
  CommandsExecutedCount, CommandsMissedCount, FilesAnalyzedCount, CommandsMissed, CommandsExecuted,
  FilesAnalyzed`.

Neither list contains a branch-coverage counter. Pester 5's code-coverage model is **command-based**,
not branch-based, so no branch metric exists to report.

`CoverageCapability: branch coverage is structurally unavailable from Pester 5.6.1; the two property
lists above are the evidence.`

Per [P6-T4]'s acceptance, this statement with its evidence **discharges** the branch-coverage
obligation on the same terms [P0-T16] granted, and is **not** a remediation-required outcome. The
remediation-required outcome applies only when the **line**-coverage figure is unobtainable from both
channels; it was obtained (85.71%).

## 3. Changed-line coverage for the four edited regions

Measured with a dedicated probe that enumerates `$r.CodeCoverage.CommandsExecuted` and
`CommandsMissed` by line and classifies each edited line.

```
EXECUTED_LINES: 41,45,46,49,71,72,78,79,82,84,106,108,109,112,113,116,117,120,121,124,127,128,130,
                131,133,137,138,142,143,147,152,153,154,157,158,160
MISSED_LINES:   42,134,139,144,164,165,166
```

| # | Edited region ([P2-Tn]) | Line(s) | Classification | Covered? |
|---|---|---|---|---|
| 1 | `-Target` parameter in the script `param(...)` block ([P2-T1]) | 11-13 (`[ValidateSet('Build','Rebuild')]` at 12) | `NOT-AN-ANALYZED-COMMAND` | n/a — parameter declarations and attributes are not executable commands in Pester's model, so they are neither in the numerator nor the denominator |
| 1b | Deprecation comment in the script `param(...)` block ([P2-T3], spec row 20) | 24 | `NOT-AN-ANALYZED-COMMAND` | n/a — a comment |
| 2 | `-Target` parameter in `Get-MSBuildBuildArguments` ([P2-T2]) | 63-65 (`[ValidateSet(...)]` at 64) | `NOT-AN-ANALYZED-COMMAND` | n/a — parameter declaration |
| 2b | `'/t:Build'` -> `"/t:$Target"` ([P2-T2]) | 73 | `NOT-AN-ANALYZED-COMMAND` at line 73 itself; the **enclosing command** is the `$arguments = @( ... )` assignment, which Pester attributes to lines **71-72**, both **EXECUTED** | **YES** |
| 3 | Deprecation comment in the function `param(...)` block ([P2-T3], spec row 20) | 98 | `NOT-AN-ANALYZED-COMMAND` | n/a — a comment |
| 3b | `$properties += 'Nullable=enable'` -> `Write-Warning '...'` ([P2-T3]) | **117** | **EXECUTED** | **YES** |
| 4 | `-Target $Target` added to the call site ([P2-T4]) | **158** | **EXECUTED** | **YES** |

**Every changed line that is an analyzed command is covered.** Changed-line coverage is therefore
**100%** (3 of 3 analyzed changed commands executed; the remaining changed lines are parameter
declarations, attributes and comments, which are not analyzed commands).

Line 73 warrants the explicit note above: it is an element of the array literal

```powershell
    $arguments = @(
        $ResolvedSolutionPath,
        "/t:$Target",
        "/p:Configuration=$Configuration",
        "/p:Platform=$Platform"
    )
```

Pester instruments the enclosing assignment, not each element, and attributes it to lines 71-72 —
both executed. The behaviour of the changed element is directly asserted by the `It` added in
[P1-T1], which requires `/t:Rebuild` in that exact array position.

## 4. Missed-command set unchanged

| Baseline line | Post-change line | Command | In an edited region? |
|---|---|---|---|
| 37 | 42 | `throw 'MSBuildProperty entries must not be empty.'` | no |
| 124 | 134 | `throw "Solution not found: $resolvedSolutionPath"` | no |
| 129 | 139 | `throw 'vswhere.exe was not found. ...'` | no |
| 134 | 144 | `throw 'MSBuild.exe not found via vswhere. ...'` | no |
| 154 | 164 | `& $msbuildPath @msbuildArguments` | no |
| 155 | 165 | `if ($LASTEXITCODE -ne 0) { ... }` | no |
| 156 | 166 | `throw "MSBuild failed with exit code $LASTEXITCODE"` | no |

The missed set is identical to the baseline; only line numbers moved, by the recorded
[P2-T1]-[P2-T4] insertion deltas. All seven sit in the un-seamed I/O tail, which this feature does
not touch. Introducing a wrapper seam to cover them is out of scope per `spec.md` option (g).

## Output Summary

Baseline line coverage **85.71%**, post-change line coverage **85.71%** — **no regression**, and
**above** the 85% policy floor. Changed-line coverage for the four edited regions of
`scripts/vscode/Invoke-VSBuild.ps1` is **100%**: all three analyzed changed commands (lines 71-72,
117 and 158) are executed, and the remaining changed lines are parameter declarations, attributes
and comments that Pester does not treat as analyzed commands. Branch coverage is structurally
unavailable from Pester 5.6.1, evidenced by two enumerated property lists; that statement discharges
the branch-coverage obligation and is **not** a remediation-required outcome. No
`PREEXISTING_COVERAGE_SHORTFALL:` was recorded at [P0-T16], so no shortfall is folded into [P7-T1].
