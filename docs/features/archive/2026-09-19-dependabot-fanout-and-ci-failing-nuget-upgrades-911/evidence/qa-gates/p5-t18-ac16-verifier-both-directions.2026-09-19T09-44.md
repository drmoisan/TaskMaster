# P5-T18 — AC16 the verifier repairs freely and fails only on residual inconsistency

Timestamp: 2026-09-19T09-44

Command:

```
pwsh -NoProfile -Command 'Set-Location "<execution-worktree-root>"; Import-Module Pester -RequiredVersion 5.6.1; $c = New-PesterConfiguration; $c.Run.Path = @("tests/scripts/dependencies/ProjectConsistency.Tests.ps1"); $c.Filter.FullName = "*AC16-*"; $c.Run.PassThru = $true; $c.Output.Verbosity = "Detailed"; $c.CodeCoverage.Enabled = $true; $c.CodeCoverage.Path = @("scripts/dependencies","scripts/vscode"); $c.CodeCoverage.OutputFormat = "JaCoCo"; $c.CodeCoverage.OutputPath = "coverage/p5-t18-ac16-coverage.xml"; $r = Invoke-Pester -Configuration $c; "PESTER Passed=$($r.PassedCount) Failed=$($r.FailedCount) Skipped=$($r.SkippedCount) Total=$($r.TotalCount)"; if ($r.FailedCount -gt 0) { exit 1 } else { exit 0 }'
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
| `Total` | exactly 2 | **executed 2**; discovered 13 |

The exact-2 clause is evaluated against the executed population for the reason P5-T5
records.

## Named cases in the Detailed output

```
PASSED: AC16- returns a success result whose report enumerates the repairs performed
PASSED: AC16- returns a failure result naming the condition and the project for an unrepairable divergence
```

Both directions are present.

- **Repairable direction.** A project whose `<Import>`, `<Error>`, `<Reference>` and
  `<HintPath>` all name stale and mutually different versions is repaired to the manifest
  version, the entry point returns `IsSuccess` true with an empty failure array, and the
  report carries a non-zero repair count whose `Kind` values include `Import` and
  `HintPath`. The report enumerating the repairs is asserted, not just the success flag.
- **Unrepairable direction.** The same project text against a manifest that additionally
  declares a second package for which no `<Reference>` and `<HintPath>` pair exists. The
  entry point returns `IsSuccess` false, exactly one failure, `Condition` exactly
  `MissingReference` and `ProjectName` exactly `Contoso.Test`. The repair pass never edits
  a manifest and never synthesises a reference, so this divergence genuinely cannot be
  resolved by it.

The two fixtures differ in **one** thing — a single extra manifest entry — so the failing
direction is attributable to that difference rather than to two unrelated projects. The
failing direction is what proves the verifier is not a pass-through: it was a pass-through
until P5-T8, and at that point this case would have returned success.

## Coverage document

`coverage/p5-t18-ac16-coverage.xml`, under the gitignored `coverage/` tree. This task
records no coverage figure.

**This task checks off AC16** in
`docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/spec.md`.
