# [P6-T5] Coverage Delta Verification — PASS

- **Issue:** #424
- **Task:** [P6-T5]
- **Baseline artifact:** `evidence/baseline/coverage-baseline.cobertura.xml` (merge-base state, `[P0-T7]`)
- **Post-change artifact:** `evidence/qa-gates/coverage-final.cobertura.xml` (`[P6-T4]`)

Timestamp: 2026-08-07T00-48

Command: `pwsh -NoProfile -File cov.ps1` / `uncov.ps1` / `changedlines.ps1` against both Cobertura reports (per-line dedup by `(filename, line number)`, because Cobertura repeats each line under both `<method><lines>` and the class-level `<lines>`)

EXIT_CODE: 0

Output Summary: **All blocking gates PASS.** New module 100%, changed module 96.63%, changed-line coverage 100% on every changed production file, zero coverage regression.

---

## A. BLOCKING — no coverage regression on changed lines

| File | Changed executable lines | Covered | Changed-line coverage |
|---|---|---|---|
| `QuickFiler/Controllers/QfcStreamingDequeueConfidenceGate.cs` | 30 | 30 | **100%** |
| `QuickFiler/Controllers/QfcHomeController.cs` | 7 | 7 | **100%** |
| `QuickFiler/Controllers/QfcScanProgressBandMapper.cs` (all new) | 25 | 25 | **100%** |
| `QuickFiler/Controllers/QfcDatamodel.cs` | n/a | n/a | `[ExcludeFromCodeCoverage]` (`QfcDatamodel.cs:25`) — outside the denominator |
| `QuickFiler/Controllers/QfcDatamodel.QueueProcessing.cs` | n/a | n/a | same partial class, therefore also excluded |
| `QuickFiler/Interfaces/IQfcDatamodel.cs` | 0 | 0 | interface-only file: **no executable lines**; absent from the report by design |

`QfcHomeController.cs` changed lines 298-304 all report `hits=1`; lines 294-297 are non-executable (comment/brace). Gate changed lines: 30/30 executable lines covered.

**Result: PASS — zero regression on changed lines.**

## B. BLOCKING — >= 90% on the new and changed modules

| Module | Baseline | Post-change | Gate | Result |
|---|---|---|---|---|
| `QfcScanProgressBandMapper.cs` (new) | n/a | **100.00%** (25/25) | >= 90% | **PASS** |
| `QfcStreamingDequeueConfidenceGate.cs` (changed) | 95.00% (57/60) | **96.63%** (86/89) | >= 90% | **PASS** |
| Changed methods in `QfcDatamodel.cs` / `QfcDatamodel.QueueProcessing.cs` | excluded | excluded | n/a | `[ExcludeFromCodeCoverage]`; correctness proven by the `QfcDatamodelLivenessTests` seam tests instead |
| Changed member in `IQfcDatamodel.cs` | n/a | n/a | n/a | interface declaration, no executable lines |
| `QfcHomeController.cs` (changed) | 67.62% (165/244) | **68.40%** (171/250) | changed-line no-regression | **PASS** (improved; whole-file rate is dominated by pre-existing COM/WinForms-bound members, and all 7 changed lines are covered) |

The mapper reports **zero** uncovered lines and **zero** partially-covered branch lines — 100% line and 100% branch.

The gate's 3 uncovered lines are **pre-existing constructs, unchanged in kind from baseline**:

| Baseline uncovered | Final uncovered | Construct |
|---|---|---|
| 29 | 40 | the 5-parameter convenience constructor's `: this(...)` chain |
| 58, 59 | 97, 98 | the `if (quantity <= 0) { return accepted; }` early return |

Same three constructs, shifted by the insertions. Baseline had 3 uncovered of 60; final has 3 uncovered of 89. **Result: PASS.**

## C. REPORTED, NON-BLOCKING — repository-wide rates

| Metric | Merge-base baseline | Post-change |
|---|---|---|
| `line-rate` | **0.7019272859161799** (70.19%) | **0.856453** (85.65%) |
| `branch-rate` | **0.5829763295685664** (58.30%) | **0.790039** (79.00%) |
| `lines-covered` / `lines-valid` | 56124 / 79957 | 94937 / 110849 |
| `branches-covered` / `branches-valid` | 13472 / 23109 | 22001 / 27848 |

**Explicit statement required by `[P6-T5]`:** the raw repository-wide line rate was **already below the 80% floor at the merge-base** (70.19%, measured in `[P0-T7]` before any change in this plan). **This change does not lower it.**

**Interpretation caveat — the two figures are not like-for-like.** The denominator grew from 79,957 to 110,849 valid lines (+38.6%), which this plan's ~600 added lines cannot explain. This is the known `dotnet-coverage` denominator instability for this repository: which assemblies get instrumented, and therefore how much uninstrumented vendored code lands in the denominator, varies between full-suite runs. The apparent +15.5-point line-rate improvement is therefore **not** a claim this change made; it is a measurement artifact. The trustworthy, stable figures are the per-file and changed-line numbers in sections A and B, which are computed against fixed, identified source files.

Per Decisions Record item 13, the repository-wide raw figure is reported for the record and is **not** treated as a pass/fail gate for this change. `CLAUDE.md` scopes the 80% floor to a testable denominator that excludes VSTO lifecycle classes, WinForms/Designer code, and Outlook-interop event handlers without injectable seams; the raw whole-report rate above includes all of those plus vendored assemblies (`SVGControl`, Swordfish collections).

## D. Documented threshold conflict (Decisions Record item 7)

Two repository policy documents state different coverage thresholds:

- `.claude/rules/csharp.md:39-41` — repository line >= 80%, new module/class/method >= 90%, changed-line regression blocking.
- `.claude/rules/general-unit-test.md` — uniform >= 85% line / >= 75% branch across tiers T1-T4.

Both figure sets are recorded above so a reviewer can apply either. Against the **85/75** set, the post-change repository figures are 85.65% line (>= 85) and 79.00% branch (>= 75) — both satisfied, subject to the same denominator caveat. Against the **80/90** set applied by this plan, the blocking change-scoped gates in sections A and B all pass. No policy document was modified.

## E. Verdict

| Gate | Binding? | Result |
|---|---|---|
| No coverage regression on changed lines | blocking | **PASS** (100% on all three changed production files) |
| >= 90% on `QfcScanProgressBandMapper.cs` | blocking | **PASS** (100.00%) |
| >= 90% on `QfcStreamingDequeueConfidenceGate.cs` | blocking | **PASS** (96.63%) |
| >= 90% on changed members of `QfcDatamodel*.cs`, `IQfcDatamodel.cs` | blocking | **N/A** — `[ExcludeFromCodeCoverage]` / no executable lines; recorded explicitly rather than silently omitted |
| Repository-wide line and branch rates | reported, non-blocking | recorded above with the below-floor-at-merge-base statement and the denominator caveat |

All values are numeric; no placeholders. **No remediation required.**
