# P5-T185 — Authoritative focused numeric coverage decision (post-correction)

Timestamp: 2026-07-22T16-29Z

Command: `pwsh -NoProfile -File t185.ps1` where the script parses only the P5-T183 artifact `evidence/qa-gates/coverage-p5-numeric-correction.2026-07-22T16-22.cobertura.xml`, deduplicates every `<line>` by source line number across all Cobertura packages (a line is covered when any package records `hits > 0`), deduplicates `<method>` entries by `name+signature`, aggregates branch `condition-coverage` per line, and subtracts the enumerated direct-adapter sequence points from the numeric union. Full script text is retained at `scratchpad/t185.ps1`, `scratchpad/t185b.ps1`, `scratchpad/t185c.ps1`.

EXIT_CODE: 0

## DECISION: REMEDIATION REQUIRED — measurable units below the 90% line-coverage threshold

This task's acceptance criterion ("Require >=90% line coverage for every applicable measurable new/changed
type/member/method/state machine ... Any unavailable/below value stops for replanning") is **not met**. P5-T185 is left
unchecked and P5-T186 is not started. No production source was changed in response to this finding.

## Parsed artifact authority

- Source: `evidence/qa-gates/coverage-p5-numeric-correction.2026-07-22T16-22.cobertura.xml` (P5-T183 only).
- SHA-256: `AC4E344AF35F929DD5B1FBE177A492FE13E5CBC9A639C747F3A09CA4384491C1`.
- Producing run: natural completion, exit code 0, 160/160 passed.
- Superseded artifacts (`...14-44.cobertura.xml`, `...14-46.md`, and the historical `09-03`/`09-06` pair) were not
  parsed or cited as passing.

## Numeric exclusion set (enumerated, host-neutral bodies rejected)

Only these sequence points were removed from the numeric union, all in
`QuickFiler/Viewers/BreadcrumbPopupUiOperations.cs`, and all belonging to the seven unchanged
`[ExcludeFromCodeCoverage]` direct WebView2/WinForms adapter members plus the single production-constructor delegate
line that wires one of them:

| Excluded lines | Owning member |
|---|---|
| 50 | production constructor delegate `(core, control, html) => BeginProductionNavigation(...)` (line 50) |
| 406 | `BeginProductionNavigation` body |
| 447, 448, 449, 450, 451, 453, 455, 456, 457, 458, 459, 460, 461, 463 | `NavigateToDocument` local functions `Detach`, `Starting`, `Completed`, `Disposed` |

The seven adapter members are `ShowOwnedPopup`, `CreateProductionControl`, `BeginProductionInitialization`,
`ReadProductionCore`, `BeginProductionNavigation`, `DisposeProductionSurface`, `NavigateToDocument`; each is emitted
with `COBERTURA_ENTRY=OMITTED` (no `<method>` node), so only their generated closure classes carried lines, which is why
the exclusion is expressed as an explicit line list. `ItemViewer.Breadcrumb.cs` produces
`ITEMVIEWER_BREADCRUMB_CLASS_ENTRIES=0`. **No host-neutral body was excluded**; every host-neutral orchestration line
remains in the denominator.

## Source-union line coverage per P5 source file (after exclusion)

| Source file | Covered/Valid | Line % | Excluded points | Uncovered lines |
|---|---:|---:|---:|---|
| `BreadcrumbUiDispatcher.cs` | 187/187 | 100.00% | 0 | — |
| `BreadcrumbWebViewSurfaceFactory.cs` | 139/140 | 99.29% | 0 | 222 |
| `BreadcrumbPopupUiOperations.cs` | 221/228 | 96.93% | 16 | 239, 240, 241, 242, 263, 304, 373 |
| `BreadcrumbDropDownOpenLifetime.cs` | 280/302 | 92.72% | 0 | 153-156, 195, 292-301, 310-313, 315, 324, 381 |
| `BreadcrumbDropDownHost.cs` | 280/282 | 99.29% | 0 | 337, 413 |
| `BreadcrumbMessengerHub.cs` | 294/294 | 100.00% | 0 | — |
| `BreadcrumbDropDownOpenCoordinator.cs` | 181/190 | 95.26% | 0 | 99, 103, 118, 122, 133, 179, 224, 225, 226 |

Every source union is at or above 90%.

## Primary-type line and branch coverage

| Required unit | Primary lines | Line % | Primary branches | Branch % | Members >= 90% | Decision |
|---|---:|---:|---:|---:|---:|---|
| Dispatcher — `BreadcrumbUiDispatcher` | 144/144 | 100.00% | 29/30 | 96.67% | 10/10 | PASS |
| NavigationReadiness — `BreadcrumbNavigationReadiness` | 96/96 | 100.00% | 30/30 | 100.00% | 9/9 | PASS |
| Factory — `BreadcrumbWebViewSurfaceFactory` | 16/16 | 100.00% | 10/10 | 100.00% | 2/2 | PASS |
| Popup operations (host-neutral) — `BreadcrumbPopupUiOperations` | 75/76 | 98.68% | 29/36 | 80.56% | 24/24 | PASS |
| OpenLifetime — `BreadcrumbDropDownOpenLifetime` | 121/123 | 98.37% | 32/36 | 88.89% | 19/20 | **BELOW** (member + state machines) |
| Host — `BreadcrumbDropDownHost` | 220/221 | 99.55% | 65/70 | 92.86% | 39/40 | **BELOW** (member) |
| Hub — `BreadcrumbMessengerHub` | 155/155 | 100.00% | 58/58 | 100.00% | 13/13 | PASS |
| Attachment — `BreadcrumbCollapsedAttachment` | 80/80 | 100.00% | 42/44 | 95.45% | 8/8 | PASS |
| Release — `BreadcrumbCollapsedAttachment.Release` | 16/16 | 100.00% | 6/6 | 100.00% | n/a | PASS |
| OpenCoordinator — `BreadcrumbDropDownOpenCoordinator` | 146/151 | 96.69% | 62/74 | 83.78% | 17/21 | **BELOW** (4 members + 1 state machine) |

Branch counts are recorded above; no universal 90% branch threshold is imposed.

## Generated state machines

| State machine | Covered/Valid | Line % | Branches | Decision |
|---|---:|---:|---:|---|
| `BreadcrumbWebViewSurfaceFactory.<CreateSurfaceAsync>d__2` | 27/28 | 96.43% | 1/2 | PASS |
| `BreadcrumbPopupUiOperations.<CreateAndInstallSurfaceAsync>d__29` | 47/52 | 90.38% | 13/18 | PASS |
| `BreadcrumbPopupUiOperations.<IgnoreFailureAsync>d__31` | 6/6 | 100.00% | 0/0 | PASS |
| `BreadcrumbPopupUiOperations.<ObserveExternalAsync>d__30` | 10/10 | 100.00% | 3/4 | PASS |
| `BreadcrumbPopupUiOperations.<RetryAsync>d__32` | 23/23 | 100.00% | 13/14 | PASS |
| `BreadcrumbDropDownOpenLifetime.<CompleteOpenAsync>d__16` | 24/28 | **85.71%** | 4/4 | **BELOW** |
| `BreadcrumbDropDownOpenLifetime.<EnsureSurfaceAsync>d__21` | 28/43 | **65.12%** | 8/10 | **BELOW** |
| `BreadcrumbDropDownOpenLifetime.<HandleOpenFailureAsync>d__24` | 14/14 | 100.00% | 0/0 | PASS |
| `BreadcrumbDropDownOpenLifetime.<ObserveScheduledAsync>d__29` | 9/9 | 100.00% | 0/0 | PASS |
| `BreadcrumbDropDownOpenLifetime.<OpenCoreAsync>d__17` | 26/27 | 96.30% | 3/4 | PASS |
| `BreadcrumbDropDownOpenLifetime.<RunOnOwnerAsync>d__28<T>` | 11/11 | 100.00% | 2/2 | PASS |
| `BreadcrumbDropDownHost.<DisposeSurfaceAfterFailureAsync>d__73` | 13/13 | 100.00% | 0/0 | PASS |
| `BreadcrumbDropDownHost.<DisposeSurfaceAsync>d__72` | 8/8 | 100.00% | 0/0 | PASS |
| `BreadcrumbDropDownHost.<ResetCoreAsync>d__70` | 15/15 | 100.00% | 3/4 | PASS |
| `BreadcrumbCollapsedAttachment.<CompleteAsync>d__12` | 31/31 | 100.00% | 10/10 | PASS |
| `BreadcrumbDropDownOpenCoordinator.<OpenCoreAsync>d__25` | 12/12 | 100.00% | 0/0 | PASS |
| `BreadcrumbDropDownOpenCoordinator.<RollbackAsync>d__28` | 6/9 | **66.67%** | 0/0 | **BELOW** |

## Units below the 90% line threshold (exact covered/valid)

| # | Unit | Covered/Valid | Line % | Uncovered source lines |
|---:|---|---:|---:|---|
| 1 | `BreadcrumbDropDownOpenLifetime.<EnsureSurfaceAsync>d__21` (state machine) | 28/43 | 65.12% | `BreadcrumbDropDownOpenLifetime.cs` 292-301, 310-313, 315 |
| 2 | `BreadcrumbDropDownOpenCoordinator.<RollbackAsync>d__28` (state machine) | 6/9 | 66.67% | `BreadcrumbDropDownOpenCoordinator.cs` 224, 225, 226 |
| 3 | `BreadcrumbDropDownOpenCoordinator.HandleSelectorOpenStateChanged()` | 4/5 | 80.00% | `BreadcrumbDropDownOpenCoordinator.cs` 118 |
| 4 | `BreadcrumbDropDownOpenCoordinator.Reset()` | 4/5 | 80.00% | `BreadcrumbDropDownOpenCoordinator.cs` 133 |
| 5 | `BreadcrumbDropDownOpenCoordinator.SetDroppedDown(bool)` | 5/6 | 83.33% | `BreadcrumbDropDownOpenCoordinator.cs` 99 |
| 6 | `BreadcrumbDropDownHost.<OnDropDownClosed>b__77_0()` | 5/6 | 83.33% | `BreadcrumbDropDownHost.cs` 413 |
| 7 | `BreadcrumbDropDownOpenLifetime.<CompleteOpenAsync>d__16` (state machine) | 24/28 | 85.71% | `BreadcrumbDropDownOpenLifetime.cs` 153-156 |
| 8 | `BreadcrumbDropDownOpenCoordinator.<HandleSelectorOpenStateChanged>b__22_0()` | 7/8 | 87.50% | `BreadcrumbDropDownOpenCoordinator.cs` 122 |
| 9 | `BreadcrumbDropDownOpenLifetime.RetainCurrentSurface(Tuple<ToolStripControlHost, Control, IWebViewMessenger>, BreadcrumbDropDownOpenLease)` | 8/9 | 88.89% | `BreadcrumbDropDownOpenLifetime.cs` 324 |

All nine units are host-neutral bodies and none qualifies for the enumerated adapter exclusion. The uncovered regions
are, in substance: the `EnsureSurfaceAsync` stale-lease disposal and post-failure cleanup branches (lines 292-301,
310-313, 315), the `CompleteOpenAsync` nested recovery-failure `Report` path (lines 153-156), the `RetainCurrentSurface`
stale-lease early return (line 324), the `OnDropDownClosed` local-function branch (line 413), and the coordinator's
rollback secondary-failure path (lines 224-226) plus four one-line guard branches at coordinator lines 99
(`SetDroppedDown`), 118 (`HandleSelectorOpenStateChanged`), 122 (its dispatched closure), and 133 (`Reset`). Coordinator
lines 103 and 179 are additionally uncovered in the source union but belong to members that are otherwise at or above
the threshold.

## ItemViewer omission

`ItemViewer.Breadcrumb.cs` has zero Cobertura class entries. Per the task text this omission passes only with diff
proof that all changed host-neutral orchestration moved to the >=90%-covered coordinator. The coordinator's primary type
is 146/151 (96.69%), but four of its members and one of its state machines are below 90%, so the coordinator does not
currently satisfy the ">=90%-covered" precondition of that allowance. The ItemViewer omission is therefore recorded as
**not yet cleared** rather than passing.

## Consequences

- P5-T185 remains **unchecked**.
- P5-T186 is **not started** and remains unchecked.
- AC-3 and AC-18 remain unchecked.
- No production or test source was modified in response to this decision; no threshold, filter, exclusion,
  `coverage.config`, runsettings, or plan text was relaxed.
- The workflow stops here for atomic replanning to add the missing deterministic tests for the nine enumerated units.

Output Summary: REMEDIATION REQUIRED. Parsing only the naturally completed 160/160 P5-T183 artifact (SHA-256
`AC4E344A...384491C1`), every source union is >=90% and the Dispatcher (144/144, 100%), NavigationReadiness (96/96),
Factory (16/16), host-neutral Popup operations (75/76), Hub (155/155), Attachment (80/80), and Release (16/16) units all
pass, with the corrected `BreadcrumbUiDispatcher` at 100% line coverage. However nine applicable measurable host-neutral
units are below the 90% line threshold: `BreadcrumbDropDownOpenLifetime.<EnsureSurfaceAsync>d__21` 28/43 (65.12%),
`BreadcrumbDropDownOpenCoordinator.<RollbackAsync>d__28` 6/9 (66.67%),
`BreadcrumbDropDownOpenCoordinator.HandleSelectorOpenStateChanged()` 4/5 (80.00%),
`BreadcrumbDropDownOpenCoordinator.Reset()` 4/5 (80.00%),
`BreadcrumbDropDownOpenCoordinator.SetDroppedDown(bool)` 5/6 (83.33%),
`BreadcrumbDropDownHost.<OnDropDownClosed>b__77_0()` 5/6 (83.33%),
`BreadcrumbDropDownOpenLifetime.<CompleteOpenAsync>d__16` 24/28 (85.71%),
`BreadcrumbDropDownOpenCoordinator.<HandleSelectorOpenStateChanged>b__22_0()` 7/8 (87.50%), and
`BreadcrumbDropDownOpenLifetime.RetainCurrentSurface(...)` 8/9 (88.89%). Only 16 enumerated direct-adapter sequence
points were excluded and no host-neutral body was excluded. P5-T185 and P5-T186 remain unchecked and the workflow stops
for atomic replanning. EXIT_CODE: 0.
