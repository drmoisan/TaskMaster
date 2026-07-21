# Coverage Delta Verification — Cycle 1 (#298)

Timestamp: 2026-07-10T08-10

## Project-level coverage

- Baseline coverage: 89.45% (1424/1592 lines) — from P0-T6 pre-remediation snapshot.
- Post-remediation coverage: 89.72% (1431/1595 lines) — from P2-T4 final vstest run.
- Delta: +0.27 percentage points. No regression; coverage increased.
- Testable-denominator floor (>= 80%): MET (89.72%).
- Scope note: coverage.runsettings restricts ModulePaths to TaskVisualization.dll, so both figures are the TaskVisualization project line coverage.

## Changed-code coverage (modified members)

- `AutoAssignPeople.AddChoicesToDict` — now measured (B1 exemption removed). Per-method line-rate = 1.0 (100%), covered by the P1-T2 Moq `IPeopleScoDictionaryNew` pass-through test.
- `AutoAssignPeople.AddColorCategory` — now measured (B2 exemption removed; body delegates to the injected `_createCategory` seam). Per-method line-rate = 1.0 (100%), covered by the P1-T4 stub-delegate forwarding test.
- `AutoAssignPeople.DefaultCreateCategory` — the single new method-level exemption; the live MAPI `CreateCategoryModule.CreateCategory(...)` call moved here behind the seam. Recorded as `[ExcludeFromCodeCoverage]`; absent from the measured method list (exemption honored), so it is the single new exempt line and does not enter the denominator.

## Removed members (M1/M2)

- `EditFilterController.DeleteFilterDialog` (static) — deleted (M1). Was `[ExcludeFromCodeCoverage]` (not in denominator); absent from post-remediation coverage.
- `EditFilterController.SetUpDeleteDialog` (empty no-op) — deleted (M2). Was previously measured; absent from post-remediation coverage. Its sole test caller was also removed.
- The private single-arg `EditFilterController(IApplicationGlobals)` constructor and `using System.Windows.Forms;` were removed as orphans; the analyzer build (P2-T2) reports no IDE0005/IDE0051/CS0246 for the file.

## Conclusion

The `>= 80%` testable-denominator floor is met with no regression on changed lines. Both members that had their coverage exemptions removed (AddChoicesToDict, AddColorCategory) are covered at 100% by the two new tests. The one new exempt line (DefaultCreateCategory) is a single irreducible live-MAPI call behind an injectable seam.
