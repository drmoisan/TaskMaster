# P9-T34 Coverage Gate Failure Evidence

Timestamp: 2026-07-27T09:58:00-04:00

## Command

```powershell
$coverageOutput = 'docs/features/active/2026-07-21-quickfiler-folder-selector-dropdown-400/evidence/qa-gates/coverage-nonnumeric-adapter-fixture-final.2026-07-27T09-58.cobertura.xml'
pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug -CoverageOutput $coverageOutput
```

## Wrapper Result

- Exit code: `0`
- Discovered: `6066`
- Passed: `6066`
- Failed: `0`
- Skipped: `0`
- Duration: `56.8121 s`
- Related test processes after completion: none

## Integrity Checks

- Coverage configuration hash before and after: `B9CD80356C6BDBE03807A0B8CB106AE03D24EFBDBB2515097FBF003099050943`
- `effective-coverage.config` files found: `0`
- Cobertura report: `coverage-nonnumeric-adapter-fixture-final.2026-07-27T09-58.cobertura.xml`
- Cobertura SHA-256: `E084DC79E665D01380B0E02A7256EED74F5517B1E03880BE9872A5034518E364`
- Repository line coverage: `92327 / 109236 = 84.5207%` (meets the repository-wide 80% threshold)

## Acceptance Result

P9-T34 does not pass. Although the complete wrapper run passed and repository-wide coverage meets the 80% threshold, the required at-least-90% coverage for every changed measurable host-neutral coordinator/member is not met.

| Changed member | Covered lines | Valid lines | Coverage |
| --- | ---: | ---: | ---: |
| `BreadcrumbItemViewerLifecycleCoordinator.SetBridgeCoordinator` | 11 | 13 | 84.6% |
| `BreadcrumbItemViewerLifecycleCoordinator.AttachMessenger` | 12 | 16 | 75.0% |
| `BreadcrumbItemViewerLifecycleCoordinator.ThrowIfDisposed` | 3 | 5 | 60.0% |
| `BreadcrumbPopupUiOperations.NavigateToDocument` | 0 | 1 | 0.0% |
| `BreadcrumbPopupUiOperations.NavigateToDocumentCore` | 0 | 6 | 0.0% |

The plan requires this coverage shortfall to stop execution for an in-place plan revision. P9-T34 remains unchecked. P9-T35 and P9-T36 were not executed, and no retry was performed.
