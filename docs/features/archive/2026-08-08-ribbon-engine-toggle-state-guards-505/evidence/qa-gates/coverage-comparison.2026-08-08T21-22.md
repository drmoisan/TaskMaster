# P5-T8 — Coverage Comparison, Baseline vs Final (AC-19)

Timestamp: 2026-08-08T21-22

Sources:

- Baseline: `<FEATURE>\evidence\baseline\tests-with-coverage.2026-08-08T20-44.md` (P0-T9), dump
  `coverage\coverage-baseline-505.cobertura.xml`
- Final: `<FEATURE>\evidence\qa-gates\tests-with-coverage.2026-08-08T21-20.md` (P5-T6), dump
  `coverage\coverage-final-505.cobertura.xml`
- New-code figures: `<FEATURE>\evidence\qa-gates\new-type-coverage.2026-08-08T21-21.md` (P5-T7)

Both dumps are read with the same separator-agnostic query used at P5-T7, so the two sides are
counted by an identical method.

## 1. Repo-wide comparison (root `<coverage>` attributes)

| Attribute | Baseline (P0-T9) | Final (P5-T6) | Delta |
|---|---|---|---|
| `line-rate` | 0.858904 | **0.859154** | **+0.000250** |
| `branch-rate` | 0.793353 | **0.793460** | **+0.000107** |
| `lines-covered` | 95706 | **95989** | **+283** |
| `lines-valid` | 111428 | **111725** | **+297** |
| `branches-covered` | 22225 | **22274** | +49 |
| `branches-valid` | 28014 | **28072** | +58 |
| `<package>` node count | 9 | **9** | 0 |

Both rates moved **up**, not down. The denominator grew by 297 lines (the two new production
files) and the numerator by 283, so the delivery is coverage-accretive at the repo level.

The nine packages in both documents are `QuickFiler`, `UtilitiesCS`, `TaskVisualization`,
`SVGControl`, `ToDoModel`, `Tags`, `TaskMaster`, `TaskTree`, `VBFunctions` — production
assemblies only. The nine `*.Test` assemblies are stripped during
`Invoke-MSTestWithCoverage.ps1` post-processing, so test code is not in the denominator
(`CLAUDE.md` § UT2 requirement).

`TaskMaster` package: line-rate 0.719020 -> **0.733595** (+0.014575), branch-rate 0.667712 ->
**0.680597** (+0.012885). The assembly this change touches improved.

## 2. New-code figures (from P5-T7)

| New file | `<class>` nodes matched | `line-rate` | `branch-rate` | 0.90 floor |
|---|---|---|---|---|
| `TaskMaster\Ribbon\EngineToggleCatalog.cs` | 1 | **1.000000** | 1.000000 | PASS |
| `TaskMaster\Ribbon\EngineToggleStateCoordinator.cs` | 1 | **0.991489** | 0.944444 | PASS |

Neither file carries `[ExcludeFromCodeCoverage]` (P4-T4), so both are genuinely measured.

## 3. Changed-line no-regression statement — every `.cs` path in the section 4 scope lock

| Scope-locked `.cs` path | Baseline | Final | Disposition |
|---|---|---|---|
| `TaskMaster\Ribbon\EngineToggleCatalog.cs` | absent (file did not exist) | `line-rate=1` | **New code.** No prior figure to regress from; clears the 0.90 new-code floor. |
| `TaskMaster\Ribbon\EngineToggleStateCoordinator.cs` | absent (file did not exist) | `line-rate=0.991489` | **New code.** Clears the 0.90 new-code floor. |
| `TaskMaster\Ribbon\EngineCommandCatalog.cs` | `line-rate=1` | `line-rate=1` | **No regression.** The six added `Map` entries are covered; the file remains fully covered at 1.000000 with 1.000000 branch-rate. |
| `TaskMaster\Ribbon\RibbonController.EngineCommands.cs` | absent (0 class nodes) | absent (0 class nodes) | **No regression, and no change in measurement status.** The type carries a pre-existing type-level `[ExcludeFromCodeCoverage]` (the change adds no new exemption — P4-T4); it is outside the denominator both before and after, so no changed line moved from covered to uncovered. |
| `TaskMaster\Ribbon\RibbonViewer.EngineCommands.cs` | absent (0 class nodes) | absent (0 class nodes) | **No regression, and no change in measurement status.** Same pre-existing type-level exemption; outside the denominator both before and after. |
| `TaskMaster.Test\Ribbon\RibbonViewerEngineCallbackShapeTests.cs` | n/a | n/a | Test code, excluded from the denominator by `CLAUDE.md` § UT2. Not a coverage subject. |
| `TaskMaster.Test\Ribbon\EngineToggleCatalogTests.cs` | n/a | n/a | Test code — same. |
| `TaskMaster.Test\Ribbon\EngineToggleStateCoordinatorTests.cs` | n/a | n/a | Test code — same. |
| `TaskMaster.Test\Ribbon\EngineCommandCatalogTests.cs` | n/a | n/a | Test code — same. |
| `TaskMaster.Test\Ribbon\RibbonExplorerXmlTests.cs` | n/a | n/a | Test code — same. |

**Conclusion: no changed line in the scope lock regressed in coverage.** Every measured
production file either held its baseline rate exactly (`EngineCommandCatalog.cs`, 1.0 -> 1.0) or
is new code clearing the 0.90 floor. The two exempted glue files are outside the denominator on
both sides, so a regression is not expressible there.

## 4. Exemption narrative — expected, but NOT asserted as verified

The modified handlers in `RibbonController.EngineCommands.cs` and
`RibbonViewer.EngineCommands.cs` live in types carrying a **type-level
`[ExcludeFromCodeCoverage]`** attribute under the ratified `CLAUDE.md` § UT2 COM/VSTO/WinForms
exemption. A flat (not rising) repo-wide figure attributable to those files is therefore the
**expected** outcome and would not be a regression.

That narrative carries a stated uncertainty, which this artifact records rather than resolves:
`coverage.config` supplies a custom `<CodeCoverage>` block that contains **only** a
`<ModulePaths>` element (verified by reading `<REPO>\coverage.config`, lines 10-24 — no
`<Attributes>` element is present). A custom `<CodeCoverage>` block can displace the default
`<Attributes>` exclude list, which would silently stop honoring `[ExcludeFromCodeCoverage]`.

Corroborating observation, offered as evidence and not as proof: both exempted glue files return
**0 `<class>` nodes** in both the baseline and the final dump. If the attribute excludes had been
displaced, those files would be expected to appear with a low but non-zero-node entry rather than
be absent entirely. This is consistent with the attribute still being honored, but it is a single
indirect observation over two files, not a verification of attribute-exclude behavior across the
solution. The exemption narrative is therefore stated as **expected-but-unverified**.

This uncertainty does **not** affect the P5-T7 binary gate: both target files are non-exempt and
were measured directly.

## 5. Repo-wide figure — record and report

The repo-wide `line-rate` of **0.859154** is recorded and reported against the `CLAUDE.md` § UT2
testable denominator. Any shortfall against a policy floor is **pre-existing debt** and is
**non-blocking for this bug fix**, per the #424 precedent. This delivery moves the figure upward.

The blocking gates for this delivery are, and remain:

1. the **changed-line no-regression** requirement (section 3 above — satisfied), and
2. the **0.90 new-code floor** from P5-T7 (satisfied at 1.000000 and 0.991489).

## 6. Known threshold conflict — recorded, not silently resolved

Two repository policy documents state different coverage floors:

| Source | Line floor | Branch floor | New-code floor |
|---|---|---|---|
| `CLAUDE.md` § UT2 | **80%** repo-wide (on the testable denominator) | not stated | **90%** for new modules/classes/methods |
| `.claude/rules/general-unit-test.md` | **85%** across all tiers | **75%** across all tiers | not stated |

This is an **unresolved policy conflict**. It is recorded here rather than resolved by this
delivery, which has no authority to reconcile the two documents. For the record, the measured
figures clear **every** floor named in either document: line 0.859154 >= 0.85 and >= 0.80;
branch 0.793460 >= 0.75; new code 1.000000 and 0.991489 >= 0.90. The conflict is therefore not
outcome-determinative for this change, but it remains open.

Binary outcome: **PASS** — no changed-line regression, new code above the 0.90 floor, and both
repo-wide rates improved.
