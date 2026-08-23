# [P3-T1] Pass-after regression run — all ten cases pass

Timestamp: 2026-08-11T01-30
Command (policy record): `mcp__drm-copilot__run_poshqc_test` with
`workspace_root = C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-a3f0c78078ca2265a` and
`scan_folders = ["tests/scripts/vscode/Invoke-MSTestWithCoverage.ClosureFilter.Tests.ps1", "tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1"]`
Command (paired direct run, source of every numeric value and per-test result below): the Conventions
Pester command with
`$c.CodeCoverage.Path = @("scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1", "scripts/vscode/Invoke-MSTestWithCoverage.ClosureFilter.ps1")`
and
`$c.CodeCoverage.OutputPath = "<FEATURE>/evidence/qa-gates/pester-coverage.pass-after.2026-08-11T01-30.xml"`
EXIT_CODE: **0** (from the paired direct run — the substantive gate at this task)

MCP Result:

```json
{"ok":true,"tool":"run_poshqc_test","workspace_root":"C:\\Users\\DanMoisan\\repos\\TaskMaster\\.claude\\worktrees\\agent-a3f0c78078ca2265a","summary":"Ran bundled PoshQC test against 'C:\\Users\\DanMoisan\\repos\\TaskMaster\\.claude\\worktrees\\agent-a3f0c78078ca2265a' with 2 selected scan folder(s)."}
```

No `ok:true` gate is imposed at this task; AC 7's "completed `run_poshqc_test` step" is discharged by
`[P3-T4]`'s MCP-completion clause.

Pester Coverage Artifact: `docs/features/active/2026-08-10-excludefromcodecoverage-nested-lambdas-457/evidence/qa-gates/pester-coverage.pass-after.2026-08-11T01-30.xml`

The distinct `pass-after` name is mandatory so `[P3-T4]`'s coverage XML, written into the same
`qa-gates/` folder, cannot clobber this one.

## Output Summary

```
Passed=29 Failed=0 Skipped=0 Coverage=88.3522727272727
ClosureFilterCommands=113 Executed=95 Percent=84.07
```

Pester v5.6.1. Discovery found 29 tests in 2 files. Tests completed in 39.72s.
Aggregate coverage across both analyzed files: 88.35% (352 analyzed commands in 2 files).
branch coverage: not emitted by Pester 5.

### All ten regression cases — individually named, all `Passed`

| Case | Fully-qualified Pester test name | Result |
|---|---|---|
| 1 | `Remove-CoberturaExemptClosureCoverage.drops closure lines whose declaring member is absent from the instrumented method set` | **Passed** (155ms) |
| 2 | `Remove-CoberturaExemptClosureCoverage.keeps closure lines whose declaring member is present in the instrumented method set` | **Passed** (13ms) |
| 3 | `Remove-CoberturaExemptClosureCoverage.keeps closure lines whose declaring member exists only as an async state-machine class` | **Passed** (10ms) |
| 4 | `Remove-CoberturaExemptClosureCoverage.drops only the exempt method from a mixed closure class and retains an underivable method` | **Passed** (36ms) |
| 5 | `Remove-CoberturaExemptClosureCoverage.removes a closure class outright when every method resolves to an absent member` | **Passed** (9ms) |
| 6 | `ConvertTo-KoverageCoberturaXml.removes exempt closure lines before the filename merge collapses the closure class` | **Passed** (9ms) |
| 7 | `Remove-CoberturaExemptClosureCoverage.leaves an async state-machine class untouched even when its member has no plain method` | **Passed** (7ms) |
| 8 | `Remove-CoberturaExemptClosureCoverage.removes covered closure lines from both the numerator and the denominator` | **Passed** (38ms) |
| 9 | `Cobertura closure name derivation.derives declaring member, declaring type and closure classification purely from names` | **Passed** (57ms) |
| 10 | `Cobertura closure name derivation.is idempotent and silent when applied twice to the same document` | **Passed** (12ms) |

### Every pre-existing test in `Invoke-MSTestWithCoverage.Helpers.Tests.ps1` still passes

| Describe | Tests | Result |
|---|---|---|
| `ConvertTo-KoverageCoberturaXml` | 13 (12 pre-existing + case 6) | all Passed |
| `Get-KoverageProjectAllowlist` | 3 | all Passed |
| `Get-CoberturaClassLineSummary` | 4 | all Passed |
| **file total** | **20** | **Passed=20, Failed=0, Skipped=0** |

Four pre-existing tests were specifically at risk from this change and all pass unchanged:

- `merges duplicate class entries that point to the same source file` and
  `normalizes stale TaskMaster roots before merging duplicate production class entries` — each
  carries a `<>c` closure class with an EMPTY `<methods />`, which `[P2-T7]` requires to be left
  untouched.
- `computes the merged per-file line-rate from the merged rollup alone` and
  `preserves the primary class methods subtree and every hits value when merging` — each carries a
  `Ns.Foo.<>c` closure class whose method is named `N`, a plain name yielding no derived declaring
  member, so the fail-safe retention path keeps it.

### Deviation from the plan's stated baseline figure

The plan states: "Baseline for that file, measured at preflight: Passed=8, Failed=0, Skipped=0.
Post-change the file must report Passed=9". The measured figures are **19 -> 20**, not 8 -> 9.

That plan figure was recorded against the pre-#441 form of the file; issue #441 (PR #538, merged into
this branch's base at `fb257cd6e0c56cbf5eacf7e6a73641cc0414c930`) added 11 tests to it. The plan
expectation is written against the pre-#441 file and is documented as a deviation rather than acted
on: no landed test was removed or weakened to match the stated figure. The substantive intent — every
pre-existing test still passes, and the file gains exactly one test (case 6) — is satisfied exactly:
the `[P0-T8]` baseline measured Passed=19 and this run measures Passed=20, a delta of exactly +1.

### Per-module coverage figure (recorded here; the verdict is rendered at [P3-T4])

`ClosureFilterCommands=113 Executed=95 Percent=84.07` for
`scripts/vscode/Invoke-MSTestWithCoverage.ClosureFilter.ps1`.

**84.07% is below the 85% line-coverage floor.** Per `[P3-T4]`, that is a blocking finding to be
remedied by adding tests, never by adjusting a threshold. The verdict and its remediation are
recorded at `[P3-T4]`; this task's own acceptance (all ten cases pass, `EXIT_CODE: 0`) is met.

The figure is taken from the `ClosureFilterCommands=` emission, never from the aggregate
`Coverage=88.35%` value: `$r.CodeCoverage.CoveragePercent` in Pester 5.6.1 is the aggregate
`hitCommands.Count / CommandCoverage.Count` across every analyzed file and cannot render a per-module
verdict.
