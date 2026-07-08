# P2-T1 — Repo-Wide First-Party Coverage (Raw Parse) (Remediation Cycle 1, Issue #223)

Timestamp: 2026-06-28T21-50
Command: [xml] parse of artifacts/csharp/coverage.xml; per-`<line>` summation across all first-party packages
EXIT_CODE: 0

## Cobertura root aggregate (all first-party packages)
- `line-rate` = 0.741108 (74.11%)
- `lines-covered` = 71654
- `lines-valid` = 96685

## Per-`<line>` summation across all nine first-party packages (authoritative #197 method, vendored included)
- lines-covered = 39585
- lines-valid = 53969
- rate = 73.35%

## Per-package breakdown (per-`<line>` total lines, package line-rate)
| Package | line-rate | line elements |
|---|---|---|
| QuickFiler | 0.3222 | 6606 |
| UtilitiesCS | 0.8775 | 39215 |
| TaskMaster | 0.5337 | 2267 |
| Swordfish.NET.General (vendored, held constant) | 0.4653 | 1504 |
| SVGControl (vendored, held constant) | 0.1628 | 1720 |
| Tags | 0.3794 | 760 |
| ToDoModel | 0.2702 | 1822 |
| TaskVisualization | 0.1831 | 71 |
| VBFunctions | 1.0000 | 4 |

Output Summary:
The repo-wide first-party coverage parsed from the canonical Cobertura artifact is 74.11% by Cobertura root aggregate, and 73.35% (39585/53969) by the authoritative #197 per-`<line>` summation method (vendored `Swordfish.NET.General` and `SVGControl` held constant). `.Test` packages are excluded by the Koverage pipeline. The bulk of valid lines is UtilitiesCS (39215 lines at 87.75%); the COM/VSTO/WinForms-bound packages (QuickFiler, TaskMaster, ToDoModel, Tags, TaskVisualization) carry the low per-package rates.
