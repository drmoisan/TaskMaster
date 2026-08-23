# [P3-T4] PoshQC Pester in coverage mode — toolchain loop iteration 2 (GATE PASSED)

Timestamp: 2026-08-11T01-46
Iteration: **2**
Command (policy record): `mcp__drm-copilot__run_poshqc_test` with
`workspace_root = C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-a3f0c78078ca2265a` and
`scan_folders = ["tests/scripts/vscode/Invoke-MSTestWithCoverage.ClosureFilter.Tests.ps1", "tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1"]`
Command (paired direct run, source of every numeric value): the Conventions Pester command with
`$c.CodeCoverage.Path = @("scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1", "scripts/vscode/Invoke-MSTestWithCoverage.ClosureFilter.ps1")`
and
`$c.CodeCoverage.OutputPath = "<FEATURE>/evidence/qa-gates/pester-coverage.final-qc.iter2.2026-08-11T01-46.xml"`
EXIT_CODE: **0**

MCP Result: **`ok:true`**

```json
{"ok":true,"tool":"run_poshqc_test","workspace_root":"C:\\Users\\DanMoisan\\repos\\TaskMaster\\.claude\\worktrees\\agent-a3f0c78078ca2265a","summary":"Ran bundled PoshQC test against 'C:\\Users\\DanMoisan\\repos\\TaskMaster\\.claude\\worktrees\\agent-a3f0c78078ca2265a' with 2 selected scan folder(s)."}
```

The MCP-completion clause is satisfied: `MCP Result: ok:true`. AC 7 requires a **completed**
`run_poshqc_test` step, not merely a recorded one.

Pester Coverage Artifact: `docs/features/active/2026-08-10-excludefromcodecoverage-nested-lambdas-457/evidence/qa-gates/pester-coverage.final-qc.iter2.2026-08-11T01-46.xml`

The `final-qc.iter2` name is distinct from `[P3-T1]`'s `pass-after` artifact in the same folder, so
neither run can clobber the other; both files are present in `qa-gates/`.

## Output Summary

```
Passed=31 Failed=0 Skipped=0 Coverage=93.4285714285714
ClosureFilterCommands=111 Executed=111 Percent=100
```

- Passed: **31**
- Failed: **0**
- Skipped: **0**
- Aggregate line/command coverage across both analyzed production files: **93.43%**
- branch coverage: **not emitted by Pester 5**
- `ClosureFilterCommands=111 Executed=111 Percent=100`

Type checking is not applicable to PowerShell and is skipped by policy
(`.claude/rules/powershell.md` § Toolchain, step 3), not by omission.

No file was changed by this step: `git status --porcelain -uall -- scripts/vscode tests/scripts/vscode`
after the run is byte-identical to the listing recorded before it. No failure, no file change, so the
loop does not restart. **Iteration 2 is the clean pass.**

## Per-module coverage verdict — `scripts/vscode/Invoke-MSTestWithCoverage.ClosureFilter.ps1`

| Measure | Value |
|---|---|
| Analyzed commands in the module | 111 |
| Executed | 111 |
| **Line/command coverage** | **100%** |
| Floor (`.claude/rules/powershell.md`, `.claude/rules/quality-tiers.md`) | >= 85% |
| **VERDICT** | **PASS** |

The verdict is rendered against the `Percent=` value of the `ClosureFilterCommands=` emission, never
against the aggregate `Coverage=93.43%` value. `$r.CodeCoverage.CoveragePercent` in Pester 5.6.1 is
the aggregate `hitCommands.Count / CommandCoverage.Count` across every analyzed file and cannot render
a per-module verdict; the per-file breakdown exists only in `$r.CodeCoverage.CommandsExecuted` and
`$r.CodeCoverage.CommandsMissed` via their `.File` property.

### How the figure moved from 84.07% to 100%

`[P3-T1]` measured `ClosureFilterCommands=113 Executed=95 Percent=84.07`, below the floor. That was a
blocking finding, and it was remedied **by adding tests**, never by adjusting a threshold. Two tests
were added to `tests/scripts/vscode/Invoke-MSTestWithCoverage.ClosureFilter.Tests.ps1`:

- `Remove-CoberturaExemptClosureCoverage.creates a missing rollup and merges a line number shared by two retained methods`
  — exercises the rollup-creation branch (a closure class with no class-level `<lines>` element), the
  de-duplication precedence rules for a repeated line number across two retained methods (maximum
  `hits`, `branch` promotion to `True`, richer `condition-coverage` retained), and a non-zero branch
  denominator so the `branch-rate` recomputation path executes.
- `Remove-CoberturaExemptClosureCoverage.emits a zero rate when every retained method contributes no line`
  — exercises the zero-denominator `'0'` fallback for both rates, where one method is dropped so the
  rebuild runs but the sole retained method carries an empty `<lines>` element.

In addition, one genuinely unreachable branch was removed rather than tested: the `else { $null }` of
`$presentMembers = if ($presence.Contains($key)) { $presence[$key] } else { $null }`. The presence set
is built over the same class-node set the removal loop walks, so a closure class's own key always
exists. Removing dead code is a simplification, not a coverage manoeuvre — the remaining 111 commands
are all genuinely reachable and all genuinely executed. The analyzed-command count moved 113 -> 111
accordingly.

The ten named regression cases are untouched by this remediation. The two additions are separate,
individually named tests, so the ten-case set required by spec AC 10 is unchanged; the file now holds
11 tests and the suite 31.

## Branch-coverage measurement gap (recorded observation, no threshold touched)

`spec.md` § Coverage impact and `.claude/rules/powershell.md` both require branch coverage >= 75% for
the new module. **Pester 5.6.1's JaCoCo output emits no branch counter**, so this floor is
unmeasurable with the repository's configured PowerShell tooling. Pester reports a command/line
coverage percent only; the header line it prints, `Covered 93.43% / 75%`, compares the LINE figure
against a configured target and is not a branch measurement.

This is recorded as a measured observation and handed to the epic. It is not adjusted, waived, or
reinterpreted here: threshold ownership remains with issue #494 per the scope prohibitions. The
observation is repeated in `[P3-T9]`'s threshold assessment for the #494 handoff.

## Output Summary (restated)

31 passed, 0 failed, 0 skipped, `EXIT_CODE: 0`, `MCP Result: ok:true`, no file changed. The new module
measures 100% line/command coverage against an 85% floor: **PASS**. Branch coverage is not emitted by
Pester 5 and the gap is handed to #494. Iteration 2 is the clean toolchain pass.
