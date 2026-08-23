# P8-T6 — Coverage Delta and Threshold Verification

Issue: #230
Task: [P8-T6]

Sources compared:

- Baseline: `evidence/baseline/coverage-baseline.cobertura.xml` (P0-T6, 6272 tests)
- Post-change: `evidence/qa-gates/coverage-final.cobertura.xml` (P8-T5, 6293 tests)

Both were produced by the same command form
(`scripts/vscode/Invoke-MSTestWithCoverage.ps1`, `dotnet-coverage collect` wrapping
`vstest.console.exe /InIsolation`), so the counting method is identical on both
sides.

---

## Gate (a) — Full-suite line rate must not regress

### Raw figures (Cobertura root `<coverage>` element)

| Metric | Baseline | Post-change | Delta |
|---|---:|---:|---:|
| `line-rate` | 0.856453 (**85.6453%**) | 0.858333 (**85.8333%**) | **+0.1880 pts** |
| `branch-rate` | 0.790039 (**79.0039%**) | 0.792226 (**79.2226%**) | **+0.2187 pts** |
| `lines-covered` | 94,937 | 95,293 | +356 |
| `lines-valid` (denominator) | 110,849 | 111,021 | **+172** |
| `branches-covered` | 22,001 | 22,073 | +72 |
| `branches-valid` | 27,848 | 27,862 | +14 |

### Denominator-adjusted figures

Removing 8 `[ExcludeFromCodeCoverage]` attributes moved previously-uninstrumented
members into the denominator, so the raw comparison is not denominator-stable. The
denominator grew by **172 lines**; the 8 de-exempted members account for **171
instrumented lines, 159 of them covered** (see the per-member table below; the
remaining 1 line is a lambda-attribution shift within the same files).

Excluding the newly-instrumented member lines from both numerator and denominator
gives the like-for-like figure:

| Metric | Baseline | Post-change (denominator-adjusted) | Delta |
|---|---:|---:|---:|
| Line rate | 94,937 / 110,849 = **85.6453%** | 95,134 / 110,850 = **85.8223%** | **+0.1769 pts** |

The adjusted rate is **above** baseline as well, because the new pump-hosted tests
also reached 197 previously-uncovered lines that were already in the denominator.

### Gate (a) result: **PASS**

Post-change line rate exceeds the baseline line rate on **both** the raw
(85.8333% vs 85.6453%) and denominator-adjusted (85.8223% vs 85.6453%) comparisons.
The increase is therefore not an artifact of denominator movement in either
direction, and the delta must not be misread as a regression.

Per D5/D12, the absolute repo-wide figure is reported, not hard-gated; it is
nonetheless above the `.claude/rules/general-unit-test.md` uniform floors of 85%
line and 75% branch, and above the CLAUDE.md 80% repo floor.

---

## Gate (b) — Changed lines covered >= 90%

The P5-T1 factory-seam edit is the only production change that introduces
executable statements. (Every other production edit in this feature is a comment
rewrite or an attribute removal, neither of which produces instrumented lines.)

| Line | File | Source | Hits | Covered |
|---:|---|---|---:|---|
| 430 | `QfcItemController.Initialization.cs` | `controller._uiDispatcher = uiDispatcher;` | 1 | yes |
| 431 | `QfcItemController.Initialization.cs` | `controller._webViewInitializer = webViewInitializer;` | 1 | yes |
| 432 | `QfcItemController.Initialization.cs` | `controller._conversationResolverFactory = conversationResolverFactory;` | 1 | yes |
| 472 | `QfcItemController.Initialization.cs` | `controller._uiDispatcher = uiDispatcher;` | 1 | yes |
| 473 | `QfcItemController.Initialization.cs` | `controller._webViewInitializer = webViewInitializer;` | 1 | yes |
| 474 | `QfcItemController.Initialization.cs` | `controller._conversationResolverFactory = conversationResolverFactory;` | 1 | yes |

**Changed-line coverage: 6 / 6 = 100.00%.**

### Gate (b) result: **PASS** (100.00% >= 90%)

---

## Gate (c) — Per-member line coverage for the 8 de-exempted members, each > 0%

Figures aggregated from the Cobertura `<line>` elements attributed to each member's
source-line span in `QuickFiler/Controllers/QfcItemController.Initialization.cs` and
`QuickFiler/Controllers/QfcItemController.ViewerSetup.cs`. Per D15, seven of the
eight members are `async`; in this Cobertura output `dotnet-coverage` attributes the
compiler-generated state-machine lines back to the declaring source file and line
numbers, so a source-line-range aggregation captures them (no separate
`+<Member>d__NN` class nodes exist in this XML — verified, `//class` count 534,
none matching `d__`).

| # | Member | Covered / Total | Line coverage | Uncovered lines | > 0% |
|---:|---|---:|---:|---|---|
| 1 | `Initialize(...)` (private 9-arg) | 13 / 13 | **100.00%** | none | PASS |
| 2 | `Initialize(bool async)` | 14 / 14 | **100.00%** | none | PASS |
| 3 | `InitializeAsync()` | 20 / 24 | **83.33%** | 217-219, 257 | PASS |
| 4 | `InitializeGraphicsAsync()` | 19 / 22 | **86.36%** | 282-284 | PASS |
| 5 | `InitializeSequentialAsync()` | 19 / 22 | **86.36%** | 309-311 | PASS |
| 6 | `CreateAsync(...)` | 17 / 19 | **89.47%** | 444-445 | PASS |
| 7 | `CreateSequentialAsync(...)` | 19 / 19 | **100.00%** | none | PASS |
| 8 | `ResolveControlGroupsAsync(ItemViewer)` | 38 / 38 | **100.00%** | none | PASS |
| | **Aggregate** | **159 / 171** | **92.98%** | | |

Every member is well above 0%; the aggregate for the newly de-exempted surface is
92.98%, above the CLAUDE.md 90% new-code bar.

### Uncovered-line accounting

- **`InitializeAsync` lines 217-219** and **`InitializeSequentialAsync` lines
  309-311** are the `if (_globals.Ol.DarkMode) { SetThemeDark(async: true); }`
  branch; those two tests run the light branch (`darkMode: false`).
- **`InitializeGraphicsAsync` lines 282-284** are the mirror-image
  `else { SetThemeLight(async: false); }` branch; that test deliberately runs the
  dark branch (`darkMode: true`) so the two theme paths are exercised across the
  suite rather than duplicated.
- **`InitializeAsync` line 257** and **`CreateAsync` lines 444-445**
  (`await controller.InitializeAsync();` tail / `return controller;` / closing
  brace) are the **structurally unreachable terminal statements** covered by the D5
  carve-out: the tail after `await InitializeWebViewAsync()` faults at the mocked
  seam by design, because reaching it requires the real CoreWebView2 runtime — an
  external process barred by the unit-test policy. Their expected coverage is
  partial by construction and the gate for these two members is "> 0%", not "no
  uncovered lines".

### Gate (c) result: **PASS** — all 8 members > 0%, with the documented carve-out
applied to `InitializeAsync` and `CreateAsync`.

---

## Overall result

| Gate | Requirement | Measured | Result |
|---|---|---|---|
| (a) | Final line-rate >= baseline line-rate, reported raw and denominator-adjusted | raw 85.8333% vs 85.6453%; adjusted 85.8223% vs 85.6453% | **PASS** |
| (b) | Changed lines covered >= 90% | 6/6 = 100.00% | **PASS** |
| (c) | Per-member line coverage reported for all 8 de-exempted members, each > 0% | 83.33% - 100.00%, aggregate 92.98% | **PASS** |

No placeholder values appear in this artifact; every figure is a measured number.
All three blocking gates pass, so the outcome is PASS rather than
remediation-required.
