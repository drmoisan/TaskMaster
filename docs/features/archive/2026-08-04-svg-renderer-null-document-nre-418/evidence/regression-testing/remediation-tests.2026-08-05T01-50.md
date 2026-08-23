# Targeted Verification Before Final QC — Remediation Cycle 1

- Task: `[P1-T19]`
- Issue: #418
- Evidence series: `2026-08-05T01-50`

Timestamp: 2026-08-05T01-59 (UTC)

Command:

```
pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug
```

`-SearchRoot .` was used as mandated; the single-project form of the shared MSTest wrapper is defective
under `Set-StrictMode` (`PropertyNotFoundException`).

EXIT_CODE: 0

## Execution metrics

| Metric | `[P0-T9]` baseline | This run | Delta |
|---|---|---|---|
| Test assemblies discovered | 9 | **9** | 0 |
| Total tests | 6140 | **6150** | **+10** |
| Passed | 6140 | **6150** | +10 |
| **Failed** | 0 | **0** | 0 |
| Skipped | 0 | **0** | 0 |
| Wall time | not recorded | 52.7048 s | — |
| Result line | `Test Run Successful.` | `Test Run Successful.` | — |

`grep -c "^  Failed "` returns **0**. `grep -ci "test host process crashed"` returns **0**: no rerun and
no environmental contention occurred in this invocation.

The +10 is exactly the ten tests this cycle's plan authorizes: 1 from `[P1-T12]`, 1 from `[P1-T14]`, and
8 from `[P1-T15]`. No test was added outside those three tasks, and no test was deleted or renamed.

## Individual outcome of each test added by `[P1-T12]`, `[P1-T14]`, and `[P1-T15]`

| # | Task | Test | Outcome |
|---|---|---|---|
| 1 | `[P1-T12]` | `GetProbeDirectories_WithAnInvalidCharacterInTheBaseDirectory_DropsThatCandidateWithoutThrowing` | **Passed** (< 1 ms) |
| 2 | `[P1-T14]` | `Constructor_WithTheBuiltInDefaultImageAndNoMargin_LeavesDocumentNonNull` | **Passed** (46 ms) |
| 3 | `[P1-T15]` | `PublicKeyTokensEqual_WithBothArgumentsNull_ReturnsTrue` | **Passed** (< 1 ms) |
| 4 | `[P1-T15]` | `PublicKeyTokensEqual_WithFirstNullAndSecondZeroLength_ReturnsTrue` | **Passed** (< 1 ms) |
| 5 | `[P1-T15]` | `PublicKeyTokensEqual_WithFirstZeroLengthAndSecondNull_ReturnsTrue` | **Passed** (< 1 ms) |
| 6 | `[P1-T15]` | `PublicKeyTokensEqual_WithFirstNullAndSecondNonEmpty_ReturnsFalse` | **Passed** (< 1 ms) |
| 7 | `[P1-T15]` | `PublicKeyTokensEqual_WithFirstNonEmptyAndSecondNull_ReturnsFalse` | **Passed** (< 1 ms) |
| 8 | `[P1-T15]` | `PublicKeyTokensEqual_WithEqualNonEmptyTokens_ReturnsTrue` | **Passed** (< 1 ms) |
| 9 | `[P1-T15]` | `PublicKeyTokensEqual_WithUnequalTokensOfEqualLength_ReturnsFalse` | **Passed** (< 1 ms) |
| 10 | `[P1-T15]` | `PublicKeyTokensEqual_WithTokensOfUnequalLength_ReturnsFalse` | **Passed** (< 1 ms) |

10 of 10 passed.

## No previously passing test now fails

`evidence/qa-gates/test-coverage.2026-08-04T14-36.md` records 6140 total, **6140 passed, 0 failed**,
across nine assemblies at this HEAD. This run records 6150 total, **6150 passed, 0 failed**, across the
same nine assemblies. Since failures are zero and the total rose by exactly the ten tests this cycle
added, every test that passed in that artifact passes here. **Confirmed: no regression.**

In particular the 28 tests this branch authored before this cycle all still pass with unchanged
assertions and unchanged names, including the four AC-1 constructor regression tests and the nine
`SvgAssemblyProbeDirectoryTests`.

## `[P1-T15]` acceptance clause — measured coverage of `PublicKeyTokensEqual`

Read from `coverage/coverage.cobertura.xml` produced by this run:

| Member | `line-rate` | Lines | `branch-rate` | Branches |
|---|---|---|---|---|
| `SVGControl.SvgAssemblyProbe.PublicKeyTokensEqual(byte[], byte[])` | **100.0000%** | **15/15** | **100.0000%** | **18/18** |

Both figures are 100%, satisfying `[P1-T15]`'s acceptance. Before this cycle the member measured
**0/15 = 0.000%** line-rate and **0/18 = 0.000%** branch-rate. The eight cases drive all fifteen lines
and all eighteen condition outcomes the instrumenter records for the member; the eighth case
(`first non-empty and second null`) was required to reach the last two, as the plan states.

`SVGControl.SvgAssemblyProbe` as a whole is at **102/102 = 100.0000% line** and **92/92 = 100.0000%
branch**, which is the figure `[P2-T7]` and `remediation-inputs.2026-08-04T20-25.md` § R-3 Verification
both require to be stated.

## Coverage headlines from this run (full comparison is `[P2-T7]`'s task)

| Scope | Covered / Total | Percent |
|---|---|---|
| Repository line | 93537 / 109518 | **85.4079%** |
| Repository branch | 21582 / 27418 | **78.7147%** |
| `SVGControl` package line | 1696 / 3532 | 48.0181% |
| `SVGControl.SvgRenderer` class line | 332 / 414 | 80.1932% |
| `SVGControl.SvgAssemblyProbe` class line | 102 / 102 | 100.0000% |
| `SVGControl.SvgAssemblyResolver` class line | 106 / 172 | 61.6279% |
| `SvgAssemblyResolver.Install()` | 6 / 6 | 100.0000% (branch 4/4 = 100%) |
| `SvgRenderer.ctor(byte[], Size, AutoSize)` | 17 / 17 | 100.0000% |

Both repository floors pass. `SvgAssemblyResolver.Install()`, the only genuinely new member this cycle
adds, is at 100% line-rate, above the `>= 90%` gate.

## Output Summary

`EXIT_CODE: 0`. **9 assemblies discovered, 6150 total, 6150 passed, 0 failed, 0 skipped**, no test host
crash, no rerun. All ten tests added by `[P1-T12]`, `[P1-T14]`, and `[P1-T15]` passed individually. No
test that passed in `evidence/qa-gates/test-coverage.2026-08-04T14-36.md` now fails. `PublicKeyTokensEqual`
measures 100% line-rate (15/15) and 100% branch-rate (18/18), up from 0%, and `SvgAssemblyProbe` is at
100% line and branch overall.
