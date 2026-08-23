# P9-T8 final remediation diff integrity

Timestamp: 2026-07-27T06:44:00-04:00

## Command

```powershell
$base = git merge-base HEAD origin/main
git diff --check $base
```

## Output Summary

`git diff --check` exited `0` with no output after removal of eleven pre-existing
trailing blank EOF lines in canonical issue-400 evidence files. The repair changed
only those evidence EOF lines; it did not change source, tests, projects, resources,
packages, configuration, coverage policy, or test behavior.

The authoritative issue-400 C# scope is the P8-T83 65-path ordinal ledger with LF
SHA-256 `ACAE6BE16F5893475F0BA6B6147FB62AB7766A87DB85955C9976B21CBB5DA1B7`.
It contains the exact planned QuickFiler, QuickFiler.Test, UtilitiesCS, and
UtilitiesCS.Test source/test paths, including both required SpamBayes paths. It is
the source/test/project scope for this final gate; canonical feature documents and
evidence are the only related resource artifacts.

All 65 C# paths in that ledger are at most 500 physical lines. The retained P5
at-most-480 headroom obligations also pass:

| Path | Physical lines |
| --- | ---: |
| `QuickFiler/Viewers/BreadcrumbPopupUiOperations.cs` | 441 |
| `QuickFiler/Viewers/BreadcrumbDropDownHost.cs` | 435 |
| `QuickFiler.Test/Viewers/BreadcrumbPopupControlDispatchTests.cs` | 435 |
| `QuickFiler.Test/Viewers/BreadcrumbSelectorToggleUiBoundaryTests.cs` | 450 |
| `QuickFiler.Test/Viewers/BreadcrumbSelectorOpenRetryTests.cs` | 422 |
| `QuickFiler.Test/Viewers/BreadcrumbUiThreadDispatchTests.cs` | 433 |
| `QuickFiler.Test/Viewers/BreadcrumbDropDownOpenCoordinatorTests.cs` | 404 |
| `QuickFiler.Test/Viewers/BreadcrumbDropDownOpenCoordinatorTests.Part2.cs` | 336 |
| `QuickFiler.Test/Viewers/BreadcrumbPopupBoundaryCoverageTests.cs` | 330 |
| `QuickFiler.Test/Viewers/BreadcrumbPopupBoundaryCoverageTests.Part2.cs` | 450 |
| `QuickFiler.Test/Viewers/BreadcrumbDropDownLifecycleCoverageTests.cs` | 429 |

The project inventory reports 51 added C# files and zero compile-include violations:
each has exactly one adjacent legacy-project `<Compile Include=... />` entry. The
two P8-T83 additions remain included exactly once in
`QuickFiler.Test/QuickFiler.Test.csproj` and `UtilitiesCS.Test/UtilitiesCS.Test.csproj`.

Protected policy hashes are unchanged:

| File | SHA-256 |
| --- | --- |
| `coverage.config` | `B9CD80356C6BDBE03807A0B8CB106AE03D24EFBDBB2515097FBF003099050943` |
| `.csharpierignore` | `362211A82C6C6887E023B1B1936408715D6C35CC42E0D91B46E8E76A29318C25` |

No package, runsettings, solution, props, targets, coverage-policy, or exclusion
file differs from the merge base. No manual or temporary artifact exists in the
issue-400 implementation scope; generated outputs are retained only under the
feature's canonical `evidence/` directories. The broader dirty worktree contains
pre-existing parallel repository customization and tooling paths; those paths are
outside the issue-400 ledger and were neither modified nor relied upon by this task.
Within the ledger and canonical feature folder, every change is accounted for by
issue #400 implementation, plan, specification, or evidence work.

Result: PASS.
