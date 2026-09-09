# P3-T2 — Both Computations' Counts Over The Fixture (AC6 Evidence)

Timestamp: 2026-09-09T11-06
Task: [P3-T2]
EXIT_CODE: 0

## How these values were obtained

Six of the eight figures are asserted directly by test T-A
(`counts a line repeated across constructor rows once, in both the line and the branch totals`),
which passes: the de-duplicated `LinesValid` `'4'`, `LinesCovered` `'3'`, `BranchesValid` `'8'` and
`BranchesCovered` `'4'`, and the descendant-axis `LinesValid` 8 and `BranchesValid` 16. T-A does not
assert the descendant-axis `LinesCovered` or `BranchesCovered`, so those two were obtained by
executing both computations over the same fixture, extracted verbatim from the committed test file
at run time rather than retyped.

Command: `pwsh -NoProfile -Command '. "./scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1"; $t = Get-Content -Raw -LiteralPath "tests/scripts/vscode/Invoke-MSTestWithCoverage.FirstParty.Tests.ps1"; $s = $t.IndexOf("<?xml"); $e = $t.IndexOf("</coverage>") + 11; [xml]$doc = $t.Substring($s, $e - $s); $sum = Get-CoberturaFirstPartyCoverageSummary -XmlDocument $doc -ProjectNames @("Ns"); ... reproduce the pinned descendant-axis tally over the same document and print both sets'`
EXIT_CODE: 0

```
FIXTURE_LINE_ELEMENTS=8
NAIVE LinesValid=8 LinesCovered=6 BranchesValid=16 BranchesCovered=10 LineRate=0.75 BranchRate=0.625
DEDUP LinesValid=4 LinesCovered=3 BranchesValid=8 BranchesCovered=4 LineRate=0.75 BranchRate=0.5 LinePercent=75.00 BranchPercent=50.00
```

The document was not retyped into the command: the fixture text is read out of the committed test
file between its first `<?xml` and its first `</coverage>`, so both computations ran over exactly the
bytes the passing test uses. The fixture carries eight `<line>` elements, as plan decision D8 states.

No file was created by this measurement. The command is inline and transient; the single throwaway
helper plan decision D7 authorizes is created and deleted by P3-T3 and P4-T1 and is unrelated to
this task.

## Executed values

| Quantity | Descendant-axis `.//line` (the defect) | De-duplicated (the new function) |
| --- | --- | --- |
| LinesValid | **8** | **4** |
| LinesCovered | **6** | **3** |
| BranchesValid | **16** | **8** |
| BranchesCovered | **10** | **4** |
| LineRate | `0.75` | `0.75` |
| BranchRate | `0.625` | `0.5` |

The four required pairs are therefore 8 against 4, 6 against 3, 16 against 8, and 10 against 4.

## Why AC5 forbids a rate assertion

`LineRate` is `0.75` under **both** computations. A test written against `LineRate` over this fixture
would pass whether or not the fix is correct, because a proportional duplication scales numerator and
denominator by the same factor and leaves the ratio unchanged. That is the property Repro & Evidence
in `spec.md` records, and it is the reason AC5 requires the four counts and forbids a rate or
percentage assertion. Test T-A accordingly asserts counts and asserts no rate.

The branch rate does move here, from `0.625` to `0.5`, only because the fixture duplicates rows at
differing multiplicities by design: line 20 appears four times, line 30 twice, line 40 only in the
class rollup and line 50 only in the method view. A uniformly duplicated fixture would leave the
branch rate unchanged as well and would discriminate nothing.

## Comparison against the plan's hand-derived D8 table

Every executed figure equals the corresponding hand-derived value in the plan's D8 table:
LinesValid 8 and 4, LinesCovered 6 and 3, BranchesValid 16 and 8, BranchesCovered 10 and 4,
LineRate `0.75` on both sides, BranchRate `0.625` and `0.5`. **No discrepancy was observed and no
assertion was edited to match an observed value.** The D8 figures were hand-derived and unconfirmed
by execution when the plan was written; they are now confirmed by execution.

Output Summary: The descendant-axis computation returns 8/6 lines and 16/10 branches over the
fixture; the de-duplicated computation returns 4/3 lines and 8/4 branches. All four required pairs
match. `LineRate` is `0.75` under both computations, which is why AC5 forbids a rate assertion. All
eight executed figures agree with the plan's hand-derived D8 table.
