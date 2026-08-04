# P5 numeric coverage remediation baseline

Timestamp: `2026-07-22T09-31`

Command: `$decision='docs/features/active/2026-07-21-quickfiler-folder-selector-dropdown-400/evidence/qa-gates/p5-authoritative-focused-coverage-decision.2026-07-22T09-06.md'; $xml='docs/features/active/2026-07-21-quickfiler-folder-selector-dropdown-400/evidence/qa-gates/coverage-popup-ui-boundary-composition.2026-07-22T09-03.cobertura.xml'; Get-Content -Raw $decision; Get-FileHash -Algorithm SHA256 $xml; [xml]$coverage=Get-Content -Raw $xml; "ROOT=$($coverage.DocumentElement.Name)|LINES=$($coverage.coverage.'lines-covered')/$($coverage.coverage.'lines-valid')|BRANCHES=$($coverage.coverage.'branches-covered')/$($coverage.coverage.'branches-valid')"`

EXIT_CODE: `0`

Output Summary: `EXPECTED FAIL-BEFORE. The read-only reconciliation confirmed the authoritative P5-T100 numeric remediation decision, its exact Cobertura hash, and the underlying 70/70 passing test state. Multiple required measurable values are below 90%, the factory wrapper is 0/9, and changed ItemViewer breadcrumb behavior is omitted by the pre-existing ItemViewer class exclusion.`

## Authority and command state

- Decision: `p5-authoritative-focused-coverage-decision.2026-07-22T09-06.md`.
- Cobertura: `coverage-popup-ui-boundary-composition.2026-07-22T09-03.cobertura.xml`.
- Cobertura SHA-256: `63246A377D836B51A5EE2FF87C75790F62E88873A6BC9BCEAD1530C6B293DD1F`.
- Filtered command: natural completion, exit code zero, exactly 70 discovered and 70 passed, zero failed/skipped.
- Cobertura root headline: 5,731/84,039 lines and 1,020/19,240 branches.

## Exact required baseline

| Unit | Source-union lines | Primary lines | Primary branches | Members at least 90% | State-machine lines | State-machine branches |
|---|---:|---:|---:|---:|---:|---:|
| `BreadcrumbUiDispatcher` | 166/185 (89.73%) | 127/142 (89.44%) | 27/32 (84.38%) | 8/10 | 0/0 (N/A) | 0/0 (N/A) |
| `BreadcrumbWebViewSurfaceFactory` | 123/156 (78.85%) | 14/25 (56.00%) | 9/10 (90.00%) | 1/3 | 28/35 (80.00%) | 4/8 (50.00%) |
| `BreadcrumbPopupUiOperations` | 216/244 (88.52%) | 72/76 (94.74%) | 27/36 (75.00%) | 23/24 | 86/91 (94.51%) | 29/36 (80.56%) |
| `BreadcrumbDropDownOpenLifetime` | 249/302 (82.45%) | 97/123 (78.86%) | 25/36 (69.44%) | 16/20 | 112/132 (84.85%) | 17/20 (85.00%) |
| `BreadcrumbDropDownHost` | 215/280 (76.79%) | 155/219 (70.78%) | 34/68 (50.00%) | 28/40 | 36/36 (100.00%) | 3/4 (75.00%) |
| `BreadcrumbMessengerHub` | 233/294 (79.25%) | 119/155 (76.77%) | 37/58 (63.79%) | 2/13 | 0/0 (N/A) | 0/0 (N/A) |
| `BreadcrumbCollapsedAttachment` | 233/294 (79.25%) | 61/80 (76.25%) | 25/44 (56.82%) | 6/8 | 25/31 (80.65%) | 8/10 (80.00%) |
| Changed `ItemViewer.Breadcrumb` behavior | 0/0 unavailable | unavailable | unavailable | unavailable | unavailable | unavailable |

`BreadcrumbCollapsedAttachment.Release(bool)` is 15/16 lines (93.75%) and 5/6 branches (83.33%). The measurable `BreadcrumbWebViewSurfaceFactory.NavigateToDocument` wrapper is 0/9 lines.

## Uncovered sequences

- Dispatcher: lines 26, 39, 73, 97, and 240.
- Factory: line 168; generated `CreateSurfaceAsync` lines 208 and 229.
- Popup operations: lines 63-65, 67-69, 79, and 371; generated lines 238, 303, 309, and 343.
- Open lifetime: lines 41, 42, 56, 97, 126, 217, 323, and 380; generated lines 194, 291, and 314.
- Host: lines 47, 68, 154-156, 158-160, 162-164, 239, 251, 260, 378, 406, 410, and 466; generated line 315.
- Messenger hub: lines 66, 74, 100, 107, 121, 143, 162, 172, 185, 223-226, 231, 240, 246-248, and 255.
- Collapsed attachment: lines 293, 294, 301, 320-322, 326, 329, 340-342, 402, and 427; generated line 382.
- ItemViewer breadcrumb: no sequence points were emitted because the existing class-level ItemViewer exclusion applies to the partial class.

## Existing bounded direct adapters

The seven existing method-level `BreadcrumbPopupUiOperations` direct adapters remain separately nonnumeric:

1. `ShowOwnedPopup`
2. `CreateProductionControl`
3. `BeginProductionInitialization`
4. `ReadProductionCore`
5. `BeginProductionNavigation`
6. `DisposeProductionSurface`
7. `NavigateToDocument`

The instrumented factory wrapper is not included in that seven-method exclusion set and remains measurable at 0/9.

## Baseline classification

This is the intended numeric fail-before for the correction. P5-T100 remains historical decision evidence. P5-T67, P5-T68, and P5-T101 remain incomplete. This task made no code, test, requirement, project, exclusion, configuration, filter, or threshold edit.
