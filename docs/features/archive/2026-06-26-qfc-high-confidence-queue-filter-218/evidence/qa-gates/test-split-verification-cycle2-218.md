# Test Split Verification (Post-Trim) — Cycle 2, Issue #218

Timestamp: 2026-06-28T17-31

Command:
- Line count: `(Get-Content -LiteralPath $f).Count` over the six `QfcHomeController*Tests.cs` files.
- Active `[TestMethod]` count: `Select-String -Pattern '^\s*\[TestMethod\]'`.
- Duplicate-name check: collect each method name following a `[TestMethod]` attribute across the six files and group for duplicates.

EXIT_CODE: 0

## Line counts and active [TestMethod] counts

| File | Lines | <=500 | Active [TestMethod] |
|------|-------|-------|---------------------|
| QfcHomeControllerTests.cs | 287 | PASS | 3 |
| QfcHomeControllerRunAsyncTests.cs | 448 | PASS | 6 |
| QfcHomeControllerIterationTests.cs | 352 | PASS | 6 |
| QfcHomeControllerMetricsTests.cs | 241 | PASS | 2 |
| QfcHomeControllerPropertyTests.cs | 345 | PASS | 13 |
| QfcHomeControllerIssue218Tests.cs | 219 | PASS | 2 |

File-size gate: every `QfcHomeController*Tests.cs` file is <=500 lines — PASS for all six.

## Compiled active [TestMethod] count

Total = 3 + 6 + 6 + 2 + 13 + 2 = 32. Equals the anchor 32 (Tests 3 + RunAsync 6 + Iteration 6 + Metrics 2 + Property 13 + Issue218 2). ASSERTION PASS.

## Duplicate definition check

32 test-method names collected across the compiled suite; ZERO duplicate `[TestMethod]` definitions. ASSERTION PASS.

## QfcFormViewerDerived disposition

`QfcFormViewerDerived` (nested public class) is defined ONLY in `QfcHomeControllerTests.cs` (the residual file) and is referenced by NO compiled test (neither residual Constructor/Init/InitAsync nor any of the 27 moved tests). Per the execution directive it is RETAINED in `QfcHomeControllerTests.cs` as residual scaffolding. Verified provably unreferenced via `Grep QfcFormViewerDerived` across `QuickFiler.Test` (only the definition lines matched). No split file references it, so no move was required.

## Residual scaffolding note

The residual `QfcHomeControllerTests.cs` retains only the scaffolding required by its three residual tests: `Setup` (TestInitialize), `SetUpMockIntelRes` (called by Setup), `SetupMockProgressTracker` (used by InitAsync_InitializesCorrectly), and the seven instance fields. The moved-test-only private helpers `SetPrivateField` and `SetupQfSettings`, and the `ArrangeRunAsyncController` helper, were removed from the residual file (each is reproduced verbatim in the RunAsync split where still needed). The two commented-out test blocks (LaunchAsync_InitializesCorrectly; QuickFileMetrics_WRITE_ExecutesCorrectly + WriteMetricsAsync_ExecutesCorrectly) are retained.

Output Summary: Test split completed. All six `QfcHomeController*Tests.cs` files are <=500 lines (largest 448). Compiled active `[TestMethod]` count = 32 (matches anchor). Zero duplicates across the compiled suite. QfcFormViewerDerived retained in the residual file. Build verification in P2-T5.
