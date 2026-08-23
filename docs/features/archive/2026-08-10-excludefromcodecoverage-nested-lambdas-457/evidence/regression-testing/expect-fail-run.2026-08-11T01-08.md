# [P1-T11] `[expect-fail]` run — all ten regression cases fail for the correct reason

Timestamp: 2026-08-11T01-08
Command (policy record): `mcp__drm-copilot__run_poshqc_test` with
`workspace_root = C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-a3f0c78078ca2265a` and
`scan_folders = ["tests/scripts/vscode/Invoke-MSTestWithCoverage.ClosureFilter.Tests.ps1", "tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1"]`
Command (paired direct run, source of every numeric value and per-test name below):
`pwsh -NoProfile -Command 'Import-Module Pester -MinimumVersion 5.0; $c = New-PesterConfiguration; $c.Run.Path = @("tests/scripts/vscode/Invoke-MSTestWithCoverage.ClosureFilter.Tests.ps1", "tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1"); $c.Run.PassThru = $true; $c.Output.Verbosity = "None"; $c.CodeCoverage.Enabled = $true; $c.CodeCoverage.Path = "scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1"; $c.CodeCoverage.OutputPath = "<FEATURE>/evidence/regression-testing/pester-coverage.2026-08-11T01-08.xml"; $r = Invoke-Pester -Configuration $c; …; if ($r.FailedCount -gt 0) { exit 1 } else { exit 0 }'`

MCP Result:

```json
{
  "ok": false,
  "tool": "run_poshqc_test",
  "workspace_root": "C:\\Users\\DanMoisan\\repos\\TaskMaster\\.claude\\worktrees\\agent-a3f0c78078ca2265a",
  "summary": "Command exited with code 10."
}
```

`ok:false` is the correct and expected result at an `[expect-fail]` task: ten tests fail by design.
`run_poshqc_test` carries no counts, so the substantive evidence is the paired direct run.

EXIT_CODE: **1** (non-zero, from the paired direct Pester run, via the explicit trailing
`if ($r.FailedCount -gt 0) { exit 1 } else { exit 0 }` that makes the recorded exit code load-bearing)

Pester Coverage Artifact: `docs/features/active/2026-08-10-excludefromcodecoverage-nested-lambdas-457/evidence/regression-testing/pester-coverage.2026-08-11T01-08.xml`

`CodeCoverage.Path` for this task is exactly
`scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1`, because
`scripts/vscode/Invoke-MSTestWithCoverage.ClosureFilter.ps1` does not exist yet.

## Split decision applied to this scan set

The `[P1-T12]` pre-authorized split was NOT taken (see `[P1-T8]` evidence:
`Invoke-MSTestWithCoverage.ClosureFilter.Tests.ps1` measures 367 lines against a 500-line ceiling).
`tests/scripts/vscode/Invoke-MSTestWithCoverage.ClosureFilter.Unit.Tests.ps1` therefore does not
exist and is correctly absent from this scan set, which is complete on its first execution.

## Output Summary

```
Passed=19 Failed=10 Skipped=0 Coverage=90.2542372881356
ClosureFilterCommands=0 Executed=0 Percent=0
```

- Passed: 19 (the pre-existing tests in `Invoke-MSTestWithCoverage.Helpers.Tests.ps1`)
- Failed: **10** — exactly the ten regression cases
- Skipped: 0
- branch coverage: not emitted by Pester 5
- `ClosureFilterCommands=0` — expected: the module does not exist yet.

### Each of the ten failing tests, with its individual test name and observed failure reason

| Case | Fully-qualified Pester test name | Observed failure reason |
|---|---|---|
| 1 | `Remove-CoberturaExemptClosureCoverage.drops closure lines whose declaring member is absent from the instrumented method set` | `CommandNotFoundException`: The term 'Remove-CoberturaExemptClosureCoverage' is not recognized… |
| 2 | `Remove-CoberturaExemptClosureCoverage.keeps closure lines whose declaring member is present in the instrumented method set` | `CommandNotFoundException`: 'Remove-CoberturaExemptClosureCoverage' |
| 3 | `Remove-CoberturaExemptClosureCoverage.keeps closure lines whose declaring member exists only as an async state-machine class` | `CommandNotFoundException`: 'Remove-CoberturaExemptClosureCoverage' |
| 4 | `Remove-CoberturaExemptClosureCoverage.drops only the exempt method from a mixed closure class and retains an underivable method` | `CommandNotFoundException`: 'Remove-CoberturaExemptClosureCoverage' |
| 5 | `Remove-CoberturaExemptClosureCoverage.removes a closure class outright when every method resolves to an absent member` | `CommandNotFoundException`: 'Remove-CoberturaExemptClosureCoverage' |
| 6 | `ConvertTo-KoverageCoberturaXml.removes exempt closure lines before the filename merge collapses the closure class` | **assertion failure**: `Expected: '10,11'` `But was: '10,11,406,409'` — the exempt closure lines are still present in the merged class |
| 7 | `Remove-CoberturaExemptClosureCoverage.leaves an async state-machine class untouched even when its member has no plain method` | `CommandNotFoundException`: 'Remove-CoberturaExemptClosureCoverage' |
| 8 | `Remove-CoberturaExemptClosureCoverage.removes covered closure lines from both the numerator and the denominator` | `CommandNotFoundException`: 'Remove-CoberturaExemptClosureCoverage' |
| 9 | `Cobertura closure name derivation.derives declaring member, declaring type and closure classification purely from names` | `CommandNotFoundException`: 'Get-CoberturaClosureDeclaringMemberName' (surfaced through the `Should -Not -Throw` wrapper: "Expected no exception to be thrown, because input '<M>b__0'…") |
| 10 | `Cobertura closure name derivation.is idempotent and silent when applied twice to the same document` | `CommandNotFoundException`: 'Remove-CoberturaExemptClosureCoverage' |

### Conformance to the task's acceptance

- Cases 1, 2, 3, 4, 5, 7, 8, 9 and 10 fail with `CommandNotFoundException` on
  `Remove-CoberturaExemptClosureCoverage` or `Get-CoberturaClosureDeclaringMemberName` — both members
  of the named set (`Remove-CoberturaExemptClosureCoverage`,
  `Get-CoberturaClosureDeclaringMemberName`, `Test-CoberturaClosureClassName`,
  `Get-CoberturaDeclaringTypeName`). Case 9 exercises `Test-CoberturaClosureClassName` and
  `Get-CoberturaDeclaringTypeName` as well; the first missing command encountered short-circuits the
  test, and it is `Get-CoberturaClosureDeclaringMemberName`.
- Case 6 fails with an assertion failure showing the exempt closure lines still present in the merged
  class, because `ConvertTo-KoverageCoberturaXml` already exists and merely does not yet call the
  filter.
- **No** Pester discovery error, here-string syntax error, or malformed-XML harness error occurred.
  Pester discovered and ran 29 tests across the two files (19 pre-existing + 10 new); every fixture
  parsed as XML before its assertion was reached.
