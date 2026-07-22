# P5-T185 — Pre-correction numeric-shortfall target inventory (read-only)

Timestamp: 2026-07-22T17-01Z

Command: `sha256sum QuickFiler.Test/Viewers/BreadcrumbDropDownOpenCoordinatorTests.Part2.cs QuickFiler.Test/Viewers/BreadcrumbPopupBoundaryCoverageTests.Part2.cs QuickFiler.Test/Viewers/BreadcrumbDropDownOpenCoordinatorTests.cs QuickFiler.Test/Viewers/BreadcrumbDropDownLifecycleCoverageTests.cs QuickFiler.Test/Viewers/BreadcrumbDropDownCoverageThresholdTests.cs coverage.config scripts/vscode/TaskMaster.cli.runsettings QuickFiler.Test/QuickFiler.Test.csproj QuickFiler/Viewers/BreadcrumbDropDownOpenCoordinator.cs QuickFiler/Viewers/BreadcrumbDropDownOpenLifetime.cs QuickFiler/Viewers/BreadcrumbDropDownHost.cs QuickFiler/Viewers/BreadcrumbPopupUiOperations.cs QuickFiler/Viewers/BreadcrumbUiDispatcher.cs QuickFiler/Viewers/BreadcrumbMessengerHub.cs QuickFiler/Viewers/BreadcrumbWebViewSurfaceFactory.cs QuickFiler/Viewers/ItemViewer.Breadcrumb.cs docs/features/active/2026-07-21-quickfiler-folder-selector-dropdown-400/evidence/qa-gates/coverage-p5-numeric-correction.2026-07-22T16-22.cobertura.xml && wc -l <each listed C# file> && python3 -c "<Cobertura per-method covered/valid/uncovered enumeration for QuickFiler.Viewers.BreadcrumbDropDownOpenCoordinator, BreadcrumbDropDownOpenLifetime, BreadcrumbDropDownHost>"`

EXIT_CODE: 0

## Cited authority (only these two artifacts)

- Nonpassing numeric decision: `evidence/qa-gates/p5-authoritative-focused-coverage-decision.2026-07-22T16-29.md`.
- Cobertura artifact: `evidence/qa-gates/coverage-p5-numeric-correction.2026-07-22T16-22.cobertura.xml`,
  SHA-256 `AC4E344AF35F929DD5B1FBE177A492FE13E5CBC9A639C747F3A09CA4384491C1` — verified byte-identical to the
  hash named by P5-T185.

## The exactly nine below-threshold applicable measurable units

Re-parsed independently from the cited Cobertura. The enumerated set matches the 16-29 decision's
"Units below the 90% line threshold" table exactly; the count is **nine**.

| # | Unit | Covered/Valid | Line % | Uncovered source lines |
|---:|---|---:|---:|---|
| 1 | `BreadcrumbDropDownOpenLifetime.<EnsureSurfaceAsync>d__21` | 28/43 | 65.12% | 292, 293, 294, 295, 296, 297, 298, 299, 300, 301, 310, 311, 312, 313, 315 |
| 2 | `BreadcrumbDropDownOpenCoordinator.<RollbackAsync>d__28` | 6/9 | 66.67% | 224, 225, 226 |
| 3 | `BreadcrumbDropDownOpenCoordinator.HandleSelectorOpenStateChanged()` | 4/5 | 80.00% | 118 |
| 4 | `BreadcrumbDropDownOpenCoordinator.Reset()` | 4/5 | 80.00% | 133 |
| 5 | `BreadcrumbDropDownOpenCoordinator.SetDroppedDown(bool)` | 5/6 | 83.33% | 99 |
| 6 | `BreadcrumbDropDownHost.<OnDropDownClosed>b__77_0()` | 5/6 | 83.33% | 413 |
| 7 | `BreadcrumbDropDownOpenLifetime.<CompleteOpenAsync>d__16` | 24/28 | 85.71% | 153, 154, 155, 156 |
| 8 | `BreadcrumbDropDownOpenCoordinator.<HandleSelectorOpenStateChanged>b__22_0()` | 7/8 | 87.50% | 122 |
| 9 | `BreadcrumbDropDownOpenLifetime.RetainCurrentSurface(...)` | 8/9 | 88.89% | 324 |

Disclosure (not a tenth unit): a raw all-method scan of the same Cobertura also reports the compiler-generated
closure `BreadcrumbDropDownHost.<>c__DisplayClass71_0.<DisposeCoreAsync>b__4` at 3/4 (75.00%, uncovered line 337).
The 16-29 authoritative decision deduplicates methods by `name+signature` and accounts for this closure inside the
`DisposeCoreAsync` member unit, which is at or above the threshold in its source-union table
(`BreadcrumbDropDownHost.cs` 280/282 = 99.29%, uncovered 337 and 413). This inventory therefore preserves the
authoritative count of exactly nine and does not silently add or drop a unit.

## Already-passing units that must not regress

| Unit | Covered/Valid |
|---|---:|
| Dispatcher | 144/144 |
| NavigationReadiness | 96/96 |
| Factory | 16/16 |
| Popup operations (host-neutral) | 75/76 |
| Hub | 155/155 |
| Attachment | 80/80 |
| Release | 16/16 |

## Candidate target-file headroom (SHA-256 and physical line count)

| File | Lines | Expected | SHA-256 | Headroom to 480 |
|---|---:|---:|---|---:|
| `QuickFiler.Test/Viewers/BreadcrumbDropDownOpenCoordinatorTests.Part2.cs` | 144 | 144 | `05c79bec3a35b5951bd3d93edde6400723cf440c685644b13e28996df1e63693` | 336 (usable) |
| `QuickFiler.Test/Viewers/BreadcrumbPopupBoundaryCoverageTests.Part2.cs` | 220 | 220 | `4a5e3f2680c0e53d9215f9c00e863d1ef8352d9878a261dd4807fd89eebfcdd8` | 260 (usable) |
| `QuickFiler.Test/Viewers/BreadcrumbDropDownOpenCoordinatorTests.cs` | 386 | 386 | `d468afc1fff84cb96c67bbfe9ba74c8719c0c7df751a16303d46310a11d75950` | 94 (not a target) |
| `QuickFiler.Test/Viewers/BreadcrumbDropDownLifecycleCoverageTests.cs` | 468 | 468 | `70d700c6f4ef145b106fdda5058fdcaea99471ce229d43448dc9917923f2b9d3` | 12 (excluded: effectively none) |
| `QuickFiler.Test/Viewers/BreadcrumbDropDownCoverageThresholdTests.cs` | 479 | 479 | `25ee741353db8cfa625f5783ed7ca17697768fbab826865f53d72f0df4bbbd77` | 1 (excluded: effectively none) |

Only the two `.Part2.cs` files carry usable headroom to the 480-line bound. `BreadcrumbDropDownLifecycleCoverageTests.cs`
and `BreadcrumbDropDownCoverageThresholdTests.cs` are excluded as targets for having effectively none.

## Protected baselines (must be hash-identical after the correction)

| Protected item | SHA-256 |
|---|---|
| `coverage.config` | `b9cd80356c6bdbe03807a0b8cb106ae03d24efbdbb2515097fbf003099050943` |
| `scripts/vscode/TaskMaster.cli.runsettings` | `98ef03a8d3b0ebb2ed7a765e3b5e1b58e774d20202df2f294c03a7260b9cef57` |
| `QuickFiler.Test/QuickFiler.Test.csproj` (100 `Compile Include` entries) | `06663711c83a1fe5de1b485d5b361db9edce43501e0c37a5af081dc0d0804fc7` |

Protected production sources (zero production files may change in this correction):

| Production source | SHA-256 |
|---|---|
| `QuickFiler/Viewers/BreadcrumbDropDownOpenCoordinator.cs` | `85dd1f82a251d00f742c363d6078e6f819b2d937c0b54e02696a5886518f087d` |
| `QuickFiler/Viewers/BreadcrumbDropDownOpenLifetime.cs` | `e53de9be76cb7ac3f69b43c12088a7b4b6da6f3f2455dcf7c6c10f5a010c53f1` |
| `QuickFiler/Viewers/BreadcrumbDropDownHost.cs` | `61769a470b0b891971994ee459649ccbb814ba3d429d37dd68e8adccdd046539` |
| `QuickFiler/Viewers/BreadcrumbPopupUiOperations.cs` | `a5cca5e401e3612de406464f4f03c11b3bbd6b1cd76d86fa5ad31af2c2d5a396` |
| `QuickFiler/Viewers/BreadcrumbUiDispatcher.cs` | `0764d49c8747276722853bf30fe32aca133cb19a3d634a9cda351217fd49017e` |
| `QuickFiler/Viewers/BreadcrumbMessengerHub.cs` | `ae307d76f01fb5c50289e9f50b6fc5f05c770a81ea4827ba010c00336a1006b2` |
| `QuickFiler/Viewers/BreadcrumbWebViewSurfaceFactory.cs` | `72a444d2545f3f1aae94b5ead2209077789e60ebd3cb5d5c49efa7959b0ace8e` |
| `QuickFiler/Viewers/ItemViewer.Breadcrumb.cs` | `d903d75387eb0ea6120ea1e1587fddbe1a5c794299d3f7eca6b59a06fe3972bb` |

Protected P5-T171/P5-T183 17-class filter string (must remain byte-identical at P5-T201):

```
FullyQualifiedName~BreadcrumbUiThreadDispatchTests|FullyQualifiedName~BreadcrumbSelectorToggleUiBoundaryTests|FullyQualifiedName~BreadcrumbPopupControlDispatchTests|FullyQualifiedName~BreadcrumbSelectorOpenRetryTests|FullyQualifiedName~BreadcrumbDropDownReadinessTests|FullyQualifiedName~BreadcrumbCollapsedSurfaceReadinessTests|FullyQualifiedName~BreadcrumbDropDownCoverageThresholdTests|FullyQualifiedName~BreadcrumbDuplicateIdentityIntegrationTests|FullyQualifiedName~BreadcrumbBridgeCoordinatorProbabilityTests|FullyQualifiedName~BreadcrumbDropDownHostTests|FullyQualifiedName~BreadcrumbMessengerHubTests|FullyQualifiedName~ItemViewerBreadcrumbDropDownContractTests|FullyQualifiedName~BreadcrumbDropDownOpenCoordinatorTests|FullyQualifiedName~BreadcrumbPopupBoundaryCoverageTests|FullyQualifiedName~BreadcrumbDropDownLifecycleCoverageTests|FullyQualifiedName~BreadcrumbMessengerHubCoverageTests|FullyQualifiedName~BreadcrumbDropDownIntegrationTests
```

## Output Summary

Read-only inventory only; no production, test, project, configuration, or evidence-source file was changed.
The cited Cobertura hash matches the value named by P5-T185 exactly. Independent re-parsing reproduces exactly
**nine** below-threshold applicable measurable units with the covered/valid pairs, percentages, and uncovered line
numbers listed above, and reproduces the seven already-passing units that must not regress. All five candidate
test-file line counts match their expected values (144, 220, 386, 468, 479) and every required SHA-256 is present.
Only the two `.Part2.cs` files carry usable headroom to the 480-line bound. Protected baselines
(`coverage.config`, `TaskMaster.cli.runsettings`, the 100-entry `QuickFiler.Test.csproj` `Compile` inventory, the
eight production sources, and the 17-class filter string) are recorded for post-correction comparison.
No count other than nine and no missing hash were observed, so the correction is authorized to proceed to P5-T186.
