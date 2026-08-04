# P9-T59 relative-output coverage delta

## Inputs and attribution method

- Canonical P9-T57 Cobertura: `coverage-nonnumeric-adapter-member-coverage-relative-output.2026-07-27T10-55.cobertura.xml`
- Cobertura SHA-256: `89DB6AC8BA9974515AF7D07A07B13F6BEAA08854DA645382005189F77971034C`
- P0 baseline: `evidence/baseline/coverage-accounting-remediation-baseline.2026-07-21T22-16.md`
- Live merge base: `e63ddc7c18ca71e2c968b3329e42d965d45af1eb` (`git merge-base HEAD origin/main`)

The calculation read each Cobertura class by its exact `filename`, normalizing only path separators. It then attributed sequence points by `(filename, line)` and retained the highest hit count for a repeated point. Changed/new source lines came from the zero-context C# diff against the live merge base and were intersected with that source-point map. No Cobertura class identity was merged with another source file.

## Repository and changed/new-line results

| Measure | P0 baseline | P9-T57 result | Result |
| --- | ---: | ---: | --- |
| Repository line coverage | 89,240/106,048 = 84.1506% | 92,380/109,252 = 84.5568% | PASS: at least 80% and above P0 |
| P9 host-neutral changed/new source points | P9-T34 corresponding source-point accounting: 498/568 = 87.6761% | 522/576 = 90.6250% | PASS: no regression; covered points and rate increased |

The changed/new P9 host-neutral set consists of the sequence points in the two source files below. Both files have changed source ranges relative to the live merge base, and every attributable sequence point in those ranges is counted once by exact source filename and line.

## Per-type and source-range results

| Exact source filename and Cobertura source range | Covered/valid | Coverage | Result |
| --- | ---: | ---: | --- |
| `QuickFiler/Viewers/BreadcrumbItemViewerLifecycleCoordinator.cs`, lines 29-479 | 288/318 | 90.5660% | PASS |
| `QuickFiler/Viewers/BreadcrumbPopupUiOperations.cs`, lines 53-490 | 234/258 | 90.6977% | PASS |
| Aggregate of the two changed measurable host-neutral source ranges | 522/576 | 90.6250% | PASS |

`QuickFiler/Viewers/ItemViewer.Breadcrumb.cs` has 238 additions and 81 deletions relative to the live merge base, but the canonical Cobertura contains no class entry for its exact source filename. It remains a direct excluded UI adapter surface and is nonnumeric for this computation; it was not silently counted as zero or folded into either measured host-neutral type. Its exclusion provenance and deterministic seam mapping are the subject of the independent P9-T60 review.

## Measurable named-member results

| Exact source filename and member | Covered/valid | Coverage | Result |
| --- | ---: | ---: | --- |
| `BreadcrumbItemViewerLifecycleCoordinator.cs` `SetBridgeCoordinator` | 13/13 | 100.0000% | PASS |
| `BreadcrumbItemViewerLifecycleCoordinator.cs` `AttachMessenger` | 16/16 | 100.0000% | PASS |
| `BreadcrumbItemViewerLifecycleCoordinator.cs` `ThrowIfDisposed` | 5/5 | 100.0000% | PASS |
| `BreadcrumbPopupUiOperations.cs` `NavigateToDocument` | 8/8 | 100.0000% | PASS |
| `BreadcrumbPopupUiOperations.cs` `NavigateToDocumentCore` | 7/7 | 100.0000% | PASS |

Every applicable named source range, measurable member, and measured type is at least 90%. Repository coverage is at least 80%, and the P9 host-neutral changed/new source-point result is not regressed from the prior P9-T34 corresponding accounting.
