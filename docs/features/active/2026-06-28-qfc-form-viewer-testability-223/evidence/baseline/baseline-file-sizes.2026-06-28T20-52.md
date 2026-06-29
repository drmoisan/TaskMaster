# Baseline — 500-Line-Cap Inventory (Issue #223)

Timestamp: 2026-06-28T20-52
Command: wc -l on the three tracked files
EXIT_CODE: 0

Measured line counts (baseline, before any edit this cycle):
- QuickFiler/Controllers/QfcFormController.cs: 1142 lines (matches plan expectation 1142). To be split into partial classes in Phase 1.
- QuickFiler/Controllers/QfcCollectionController.cs: 2299 lines (plan expected ~2300). Carries `[ExcludeFromCodeCoverage]` (verified at line 20).
- QuickFiler.Test/Controllers/QfcFormControllerTests.cs: 823 lines (matches plan expectation 823). Pre-existing test-code 500-line-cap violation.

Disposition statements:
(a) QfcCollectionController.cs is pre-existing production debt. This cycle it receives ONLY a net-negative edit (Seam C `ActivateQueuedTlp` rewrite, net -3 lines per plan P3-T9). It is NOT to be split this cycle (splitting a 2299-line `[ExcludeFromCodeCoverage]` class is a broad out-of-scope refactor). Its post-edit count must be <= this baseline (2299). (AC6 disposition basis.)
(b) QfcFormControllerTests.cs is pre-existing test-code debt at 823 lines. It must remain net-neutral this cycle (its count must NOT exceed 823). All new seam tests are routed to a separate new file QfcFormControllerSeamTests.cs (P3-T13), keeping this file from growing further. The in-place member migration (P3-T11) adds no new [TestMethod] cases. (AC6 disposition basis.)

Output Summary: Three counts captured (1142, 2299, 823). Both AC6 disposition statements recorded. QfcCollectionController.cs confirmed `[ExcludeFromCodeCoverage]`.
