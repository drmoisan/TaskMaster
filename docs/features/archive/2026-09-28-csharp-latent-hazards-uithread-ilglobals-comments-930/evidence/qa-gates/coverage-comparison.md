# Coverage comparison: baseline, post-change, changed lines ([P2-T7])

Timestamp: 2026-09-29T09-23
Command: CMD-COVERAGE-PARSE with STAGE = baseline (recorded in EVIDENCE/baseline/baseline-04-mstest-coverage.md); CMD-COVERAGE-PARSE with STAGE = final (recorded in EVIDENCE/qa-gates/final-06-mstest-coverage.md)
EXIT_CODE: 0
Output Summary:
- FIRST-PARTY-BASELINE: First-party coverage: lines 56079/65737 (85.31%), branches 13593/17052 (79.71%)
- FIRST-PARTY-FINAL: First-party coverage: lines 56084/65736 (85.32%), branches 13597/17054 (79.73%)
- LINES-VALID-DELTA-PERCENT: 0.0015 (|65736 - 65737| / 65737 x 100; see the lines-valid note below)
- COMPARABILITY-BRANCH: A (the two lines-valid figures differ by 1, at most 1 percent of the baseline figure). Under Branch A the first-party line percentage moved from 85.31 to 85.32 (+0.01 points) and the branch percentage from 79.71 to 79.73 (+0.02 points); neither fell, so both are within the 0.5-point tolerance.
- UITHREAD-UNCOVERED: baseline 3, final 3 (lines 38, 39, 40 in both runs; not in the changed region)
- ILGLOBALS-UNCOVERED: baseline 2, final 2 (baseline lines 160 and 161, final lines 157 and 158: the same two statements shifted up by the three deleted lines)
- RETURN-LINE: baseline line 197 HITS=1 COND=100% (2/2); final line 198 HITS=1 COND=100% (4/4)
- GUARD-LINE: final return line plus 1 is line 199; HITS=1 (a line element is present)
- CHANGED-LINES-COVERAGE: 100 (1 of 1 changed executable line hit: UiThread.cs line 199, the `&& _dispatcher is not null` operand, HITS=1; the inserted comment line 197 is not executable; ILGlobals.cs has deletions only; the two QuickFiler files changed comment lines only)
- Condition coverage: BASELINE-RETURN-COND was a condition-coverage value (100% (2/2)), and FINAL-RETURN-COND 100% (4/4) has covered equal to total, so all four outcomes of the three-operand return expression are exercised (the new test covers the null-dispatcher outcome, the owning-thread test the true outcome, and the different-dispatcher test the false reference outcome).
- Note on lines-valid: the final UiThread.cs block reports 134 valid lines (baseline 133; the added operand line) and the ILGlobals.cs block 38 (baseline 40; the two removed field initializers), a net change of minus 1, which matches the document-level change from 65737 to 65736.

Acceptance: UITHREAD-UNCOVERED final 3 is at most baseline 3; ILGLOBALS-UNCOVERED final 2 is at most baseline 2; final return-line HITS 1 is at least 1; guard-line HITS 1 is at least 1; FINAL-RETURN-COND covered equals total; Branch A named, and neither first-party percentage fell by more than 0.5 points. All conditions hold.
