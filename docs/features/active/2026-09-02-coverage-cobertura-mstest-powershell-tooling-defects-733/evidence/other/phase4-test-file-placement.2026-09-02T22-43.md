# Phase 4 test-file placement decision (P4-T1)

Timestamp: 2026-09-02T22-43

Task: [P4-T1]

## Command

Command: pwsh -NoProfile -Command reading tests/scripts/vscode/Invoke-MSTest.RunSettings.Tests.ps1
with `Get-Content -LiteralPath` and reporting `.Count`. This is the same physical-line idiom the
P0-T4 baseline and the P3-T5 check used; `Measure-Object -Line` is deliberately not used because
it omits blank lines and under-reports against the 500-line ceiling.

EXIT_CODE: 0

## Measurement

| Quantity | Lines |
|---|---|
| tests/scripts/vscode/Invoke-MSTest.RunSettings.Tests.ps1 at the planning pass (before Phase 2) | 459 |
| Lines added to that file by P2-T1 | 28 |
| tests/scripts/vscode/Invoke-MSTest.RunSettings.Tests.ps1 measured now | 487 |
| Remaining headroom against the 500-line ceiling | 13 |

The 459-line figure the plan records for the planning pass is confirmed by the measured 487 minus
the 28 lines P2-T1 added, so the two measurements are consistent with each other.

## Projection of the new Describe block

The block to be added by P4-T2 is a Describe 'Get-MSTestAssemblyPathList' containing three It
cases (zero matches, exactly one match, multiple matches). Projected structure:

| Element | Lines |
|---|---|
| Blank separator line before the block | 1 |
| `Describe 'Get-MSTestAssemblyPathList' {` and its closing brace | 2 |
| Comment citing issue #733 finding 7 and the StrictMode rationale | 3 |
| Three It cases at 9 lines each (It header, a Get-ChildItem mock, blank, the call, blank, the Count assertion, closing brace, blank separator) | 27 |
| **Projected block total** | **33** |

Projected file total if placed in tests/scripts/vscode/Invoke-MSTest.RunSettings.Tests.ps1:
487 + 33 = **520 lines**, which exceeds the 500-line ceiling by 20 lines.

The projection is not sensitive to the estimate. Even a minimum-plausible block — no leading
comment, three It cases of 7 lines each, plus the two Describe lines and one blank separator —
totals 24 lines and still yields 511, above the ceiling. There is no realistic shape of the
required three-case block that fits in the 13 lines of remaining headroom.

## Decision

The projected total exceeds 500 lines, so the plan's stated condition selects the split branch.

**Chosen target file: tests/scripts/vscode/Invoke-MSTest.AssemblyDiscovery.Tests.ps1** (new).

Every later Phase 4 task — P4-T2 (add the Describe block), P4-T3 (expect-fail run), P4-T6
(pass-after run), and P4-T7 (file-size check) — targets exactly that file. Nothing further is
added to tests/scripts/vscode/Invoke-MSTest.RunSettings.Tests.ps1 in this plan.

The new file carries its own `BeforeAll` that resolves the repository root from `$PSScriptRoot`
and dot-sources scripts/vscode/Invoke-MSTest.ps1 through the same
`. $script:mstestScript -NoExecute` pattern used at tests/scripts/vscode/Invoke-MSTest.RunSettings.Tests.ps1
line 10, including that line's surrounding try/catch, because the production script's top-level
body runs before its `-NoExecute` return and throws in a test host.

This file is the conditional test file already named in this plan's Conventions write set; it is
not an addition to that write set.

## Output Summary

Measured 487 lines in tests/scripts/vscode/Invoke-MSTest.RunSettings.Tests.ps1 (459 at the
planning pass plus 28 added by P2-T1), leaving 13 lines of headroom. The new three-case Describe
block projects to 33 lines, for a projected total of 520, which exceeds the 500-line ceiling.
Decision: the block goes in the new file
tests/scripts/vscode/Invoke-MSTest.AssemblyDiscovery.Tests.ps1.
