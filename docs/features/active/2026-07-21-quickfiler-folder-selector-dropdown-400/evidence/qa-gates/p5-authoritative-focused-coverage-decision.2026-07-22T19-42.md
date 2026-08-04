# P5-T211 — Authoritative focused numeric coverage decision (post-dead-code-removal)

Timestamp: 2026-07-22T19-42Z

Command: `python parse (source-union by filename with adapter-line exclusion; primary-type exact-class totals; per-member and generated-state-machine covered/valid; branch condition-coverage aggregation) over ONLY evidence/qa-gates/coverage-p5-deadcode-removal.2026-07-22T19-32.cobertura.xml`

EXIT_CODE: 0

## DECISION: PASS — every applicable measurable new/changed unit is at or above 90% line coverage

Parsing only the P5-T209 authoritative Cobertura (`coverage-p5-deadcode-removal.2026-07-22T19-32.cobertura.xml`,
natural exit 0, 170/170), every applicable measurable new/changed type, member, method, and generated
state machine reports at least 90% line coverage. This supersedes the nonpassing `2026-07-22T16-29`
decision; its "REMEDIATION REQUIRED" status and "not yet cleared" ItemViewer omission are not carried
forward.

## Parsed artifact authority

- Source: `evidence/qa-gates/coverage-p5-deadcode-removal.2026-07-22T19-32.cobertura.xml` (P5-T209 only).
- This run superseded the P5-T201 `2026-07-22T18-58` Cobertura (which still recorded d__16 at 24/28).
- Superseded/non-authoritative below-threshold artifacts (`...16-22.cobertura.xml`, `...16-29.md`,
  `...14-44`, `...14-46`) were not cited as passing.

## Numeric exclusion set (unchanged from the 16-29 decision; host-neutral bodies rejected)

Only these 16 sequence points were removed from the numeric union, all in
`QuickFiler/Viewers/BreadcrumbPopupUiOperations.cs`, all belonging to the seven unchanged
`[ExcludeFromCodeCoverage]` direct WebView2/WinForms adapter members plus the single production-constructor
delegate line that wires one of them: line 50 (constructor delegate), 406 (`BeginProductionNavigation`),
447-451, 453, 455-461, 463 (`NavigateToDocument` local functions `Detach`/`Starting`/`Completed`/`Disposed`).
No host-neutral body was excluded; every host-neutral orchestration line remains in the denominator.

## Source-union line coverage per P5 source file (after exclusion)

| Source file | Covered/Valid | Line % | Uncovered lines |
|---|---:|---:|---|
| `BreadcrumbUiDispatcher.cs` | 187/187 | 100.00% | — |
| `BreadcrumbWebViewSurfaceFactory.cs` | 139/140 | 99.29% | 222 |
| `BreadcrumbPopupUiOperations.cs` | 221/228 | 96.93% | 239, 240, 241, 242, 263, 304, 373 |
| `BreadcrumbDropDownOpenLifetime.cs` | 292/294 | 99.32% | 186, 306 |
| `BreadcrumbDropDownHost.cs` | 281/282 | 99.65% | 337 |
| `BreadcrumbMessengerHub.cs` | 294/294 | 100.00% | — |
| `BreadcrumbDropDownOpenCoordinator.cs` | 188/190 | 98.95% | 103, 179 |

Every source union is at or above 90%. `BreadcrumbDropDownOpenLifetime.cs` rose from 280/302 (92.72%) to
292/294 (99.32%): the four unreachable dead lines 153-156 left the denominator (P5-T203) and the
previously-uncovered 292-301, 310-313, 315, 324 are now covered by the P5-T195 cases. Coordinator lines
103 and 179 remain uncovered but belong to members otherwise at or above the threshold.

## Primary-type line and branch coverage — all PASS

| Required unit | Primary lines | Line % | Primary branches | Branch % | Decision |
|---|---:|---:|---:|---:|---|
| Dispatcher — `BreadcrumbUiDispatcher` | 144/144 | 100.00% | 29/30 | 96.67% | PASS |
| NavigationReadiness — `BreadcrumbNavigationReadiness` | 96/96 | 100.00% | 30/30 | 100.00% | PASS |
| Factory — `BreadcrumbWebViewSurfaceFactory` | 16/16 | 100.00% | 10/10 | 100.00% | PASS |
| Popup operations (host-neutral) — `BreadcrumbPopupUiOperations` | 75/76 | 98.68% | 29/36 | 80.56% | PASS |
| OpenLifetime — `BreadcrumbDropDownOpenLifetime` | 123/123 | 100.00% | 34/36 | 94.44% | PASS |
| Host — `BreadcrumbDropDownHost` | 221/221 | 100.00% | 66/70 | 94.29% | PASS |
| Hub — `BreadcrumbMessengerHub` | 155/155 | 100.00% | 58/58 | 100.00% | PASS |
| Attachment — `BreadcrumbCollapsedAttachment` | 80/80 | 100.00% | 42/44 | 95.45% | PASS |
| Release — `BreadcrumbCollapsedAttachment.Release` | 16/16 | 100.00% | 6/6 | 100.00% | PASS |
| OpenCoordinator — `BreadcrumbDropDownOpenCoordinator` | 150/151 | 99.34% | 66/74 | 89.19% | PASS |

Branch counts are recorded; no universal 90% branch threshold is imposed. OpenLifetime (32/36 -> 34/36),
Host (65/70 -> 66/70), and OpenCoordinator (62/74 -> 66/74) branch coverage all improved from the 16-29
decision as a consequence of the ten new deterministic cases exercising the previously-uncovered
branches, and all three primary types moved from BELOW to PASS on line coverage.

## Nine P5-T185 units — all now at or above 90% (restated from P5-T210)

| # | Unit | Covered/Valid | Line % |
|---:|---|---:|---:|
| 1 | `BreadcrumbDropDownOpenLifetime.<EnsureSurfaceAsync>d__21` | 42/43 | 97.67% |
| 2 | `BreadcrumbDropDownOpenCoordinator.<RollbackAsync>d__28` | 9/9 | 100.00% |
| 3 | `BreadcrumbDropDownOpenCoordinator.HandleSelectorOpenStateChanged()` | 5/5 | 100.00% |
| 4 | `BreadcrumbDropDownOpenCoordinator.Reset()` | 5/5 | 100.00% |
| 5 | `BreadcrumbDropDownOpenCoordinator.SetDroppedDown(bool)` | 6/6 | 100.00% |
| 6 | `BreadcrumbDropDownHost.<OnDropDownClosed>b__77_0()` | 6/6 | 100.00% |
| 7 | `BreadcrumbDropDownOpenLifetime.<CompleteOpenAsync>d__16` | 24/24 | 100.00% |
| 8 | `BreadcrumbDropDownOpenCoordinator.<HandleSelectorOpenStateChanged>b__22_0()` | 8/8 | 100.00% |
| 9 | `BreadcrumbDropDownOpenLifetime.RetainCurrentSurface(...)` | 9/9 | 100.00% |

A single unit below 90% would be a nonpassing decision; none is below. Named critical branches on the
reachable failure, cancellation, rollback, retention, and late-callback paths are exercised by the P5-T188
/ P5-T195 deterministic cases (see P5-T210 mapping), and the reachable `CompleteOpenAsync` failure path
plus the `finally` settlement remain covered after the dead-line removal.

## ITEMVIEWER OMISSION: CLEARED

The omission's stated precondition is that all changed host-neutral orchestration moved to a
>=90%-covered `BreadcrumbDropDownOpenCoordinator`. That precondition is now satisfied:

- `BreadcrumbDropDownOpenCoordinator` primary type is 150/151 = 99.34% (>=90%).
- Every coordinator member is at or above 90% — in fact all at 100%:
  - `SetDroppedDown(bool)` 6/6 = 100.00%
  - `HandleSelectorOpenStateChanged()` 5/5 = 100.00%
  - `<HandleSelectorOpenStateChanged>b__22_0()` (inner lambda) 8/8 = 100.00%
  - `Reset()` 5/5 = 100.00%
  - `<RollbackAsync>d__28` 9/9 = 100.00%
- Diff proof: the changed host-neutral open/close/selector orchestration was moved out of
  `ItemViewer.Breadcrumb.cs` into `BreadcrumbDropDownOpenCoordinator` during the P5 extraction and the
  P5-T120 -> P5-T130 replacement; `ItemViewer.Breadcrumb.cs` retains only one-line delegation into the
  coordinator/host seams. With the coordinator now >=90% on every member, the orchestration that formerly
  lived in the ItemViewer is measured and passing in the coordinator.

Recorded: **ITEMVIEWER OMISSION: CLEARED**. The prior "not yet cleared" status from the nonpassing
`2026-07-22T16-29` decision is superseded and not carried forward.

## Consequences

- AC-3 and AC-18 remain unchecked (deferred to the P5-T212 audit and the P9 full-repository obligation;
  final repository coverage and no-regression remain mandatory after all phases).
- No threshold, `coverage.config`, runsettings, exclusion, or filter was changed to reach this decision.

## Output Summary

PASS. Parsing only the naturally completed 170/170 P5-T209 Cobertura, every source union is >=90%, all
ten named primary-type units pass line coverage (Dispatcher 144/144, NavigationReadiness 96/96, Factory
16/16, Popup operations 75/76, OpenLifetime 123/123, Host 221/221, Hub 155/155, Attachment 80/80, Release
16/16, OpenCoordinator 150/151), and all nine P5-T185 units are at or above 90% (eight at 100%,
`<EnsureSurfaceAsync>d__21` at 97.67%), with `<CompleteOpenAsync>d__16` at 100% (24/24) after the P5-T203
dead-line removal. Branch counts are recorded without a universal 90% branch threshold. ITEMVIEWER
OMISSION: CLEARED, proven by the coordinator's per-member numeric coverage (all members at 100%) and the
extraction diff. EXIT_CODE: 0.
