# P5-T19 — AC23 reference completeness, demonstrably detectable

Timestamp: 2026-09-19T09-44

Command:

```
pwsh -NoProfile -Command 'Set-Location "<execution-worktree-root>"; Import-Module Pester -RequiredVersion 5.6.1; $c = New-PesterConfiguration; $c.Run.Path = @("tests/scripts/dependencies/ProjectConsistency.Tests.ps1"); $c.Filter.FullName = "*AC23-*"; $c.Run.PassThru = $true; $c.Output.Verbosity = "Detailed"; $c.CodeCoverage.Enabled = $true; $c.CodeCoverage.Path = @("scripts/dependencies","scripts/vscode"); $c.CodeCoverage.OutputFormat = "JaCoCo"; $c.CodeCoverage.OutputPath = "coverage/p5-t19-ac23-coverage.xml"; $r = Invoke-Pester -Configuration $c; "PESTER Passed=$($r.PassedCount) Failed=$($r.FailedCount) Skipped=$($r.SkippedCount) Total=$($r.TotalCount)"; if ($r.FailedCount -gt 0) { exit 1 } else { exit 0 }'
```

EXIT_CODE: 0

## Output Summary

```
PESTER Passed=2 Failed=0 Skipped=0 Total=13
EXECUTED=2
NOTRUN=11
```

## Acceptance

| Clause | Required | Measured |
|---|---|---|
| `EXIT_CODE` | 0 | 0 |
| `Failed` | 0 | 0 |
| `Total` | at least 2 | 2 executed (13 discovered) |

## Named cases in the Detailed output

```
PASSED: AC23- reports a missing reference when one Reference and HintPath pair is removed
PASSED: AC23- reports no missing reference and a non-zero examined count for a complete fixture
```

Both cases this task requires to be named are present.

- The **firing** case asserts exactly one finding for a fixture from which one
  `<Reference>` and `<HintPath>` pair has been removed, and asserts the finding names the
  package `Fabrikam.Core` and the asset `Fabrikam.Core.dll`. The detector is therefore
  demonstrated firing rather than assumed able to.
- The **clean** case asserts zero findings with a non-zero examined count. The examined
  count is the guard: a detector that resolved no assets at all would report zero findings
  and zero examined, and would fail on the second clause.

This check exists to falsify the assumption that the NuGet CLI adds references for newly
introduced assemblies. A check that cannot be made to fail tests nothing, which is why the
firing direction is asserted first.

The companion `ConsistencyVerifier.Tests.ps1` case exercises the same detector from the
other side, asserting `HasHintPath` is false when the `<Reference>` survives but the
`<HintPath>` does not, so the two halves of the requirement are distinguished rather than
folded into one boolean.

## Coverage document

`coverage/p5-t19-ac23-coverage.xml`, under the gitignored `coverage/` tree. This task
records no coverage figure.

**This task checks off AC23** in
`docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/spec.md`.
