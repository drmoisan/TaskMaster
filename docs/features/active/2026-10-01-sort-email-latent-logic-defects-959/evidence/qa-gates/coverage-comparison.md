# Coverage Comparison (P6-T8)

Timestamp: 2026-10-03T12-48
ITERATION: 1
Command: CMD-COVERAGE-TEXTS (STAGE final, BASELINE-HASH 408A19E922396ADCFF033FE8156DCFE3C818FA8A5484469691071CC2176E142D, the BASELINE-NONEXEMPT-HASH of coverage-baseline.md), run as pwsh -NoProfile -Command with Set-Location to the item worktree over coverage\final-959.cobertura.xml, coverage\baseline-959.cobertura.xml and the two JaCoCo projections
EXIT_CODE: 0 (the payload's process exit code)
Output Summary:
- SORTEMAIL-CLASS final UtilitiesCS\EmailIntelligence\EmailParsingSorting\SortEmail.cs valid=5 covered=5 uncovered=0
- SORTEMAIL-CLASS final UtilitiesCS\EmailIntelligence\EmailParsingSorting\SortEmail.AttachmentSaving.cs valid=142 covered=142 uncovered=0
- SORTEMAIL-CLASS final UtilitiesCS\EmailIntelligence\EmailParsingSorting\SortEmail.TrySaveAttachment.cs valid=93 covered=89 uncovered=4
- SORTEMAIL-CLASS final UtilitiesCS\EmailIntelligence\EmailParsingSorting\SortEmail.UndoAndMoveLog.cs valid=45 covered=45 uncovered=0
- SORTEMAIL-CLASS final UtilitiesCS\EmailIntelligence\EmailParsingSorting\SortEmail.MailItemSort.cs valid=1 covered=0 uncovered=1
- SORTEMAIL-AGG final valid=286 covered=281 uncovered=5
- SORTEMAIL-DIR-CLASSES: 17
- GUARD-CONDITION-LINES: 124
- EXEMPT-LAMBDA-LINES: 36
- EXEMPT-LAMBDA-COUNT: 1
- EXEMPT-ELSE-BRACE-LINES: 190
- EXEMPT-ELSE-BRACE-COUNT: 1
- EXEMPT-CATCH-BRACE-LINES: 191
- EXEMPT-CATCH-BRACE-COUNT: 1
- EXEMPT-GUARD-BRACE-LINES: 133
- EXEMPT-GUARD-BRACE-COUNT: 1
- EXEMPT-LINES: 36,133,190,191
- TRYSAVE-CLASS-FOUND: True
- EXEMPT-UNCOVERED-LINE UtilitiesCS\EmailIntelligence\EmailParsingSorting\SortEmail.TrySaveAttachment.cs:36
- EXEMPT-UNCOVERED-LINE UtilitiesCS\EmailIntelligence\EmailParsingSorting\SortEmail.TrySaveAttachment.cs:133
- EXEMPT-UNCOVERED-LINE UtilitiesCS\EmailIntelligence\EmailParsingSorting\SortEmail.TrySaveAttachment.cs:190
- EXEMPT-UNCOVERED-LINE UtilitiesCS\EmailIntelligence\EmailParsingSorting\SortEmail.TrySaveAttachment.cs:191
- NONEXEMPT-UNCOVERED UtilitiesCS\EmailIntelligence\EmailParsingSorting\SortEmail.MailItemSort.cs:144 :: await attachments.ForEachAsync(async x => await x.SaveAttachmentAsync());
- EXEMPT-UNCOVERED: 4
- NONEXEMPT-COUNT: 1
- NONEXEMPT-SET-SHA256: 408A19E922396ADCFF033FE8156DCFE3C818FA8A5484469691071CC2176E142D
- NONEXEMPT-SET-MATCHES-BASELINE: True
- CONTROL-LINE: 15
- CONTROL-SET-SHA256: D27D0D3C5B00BADEE7352563C5BC2F5BFD03A519092298572D4B3EDB90599DE3
- CONTROL-DIFFERS-BASELINE: True
- BASELINE-FIRST-PARTY: First-party coverage: lines 56212/65855 (85.36%), branches 13620/17078 (79.75%)
- FINAL-FIRST-PARTY: First-party coverage: lines 56410/66058 (85.39%), branches 13681/17141 (79.81%)
- FIRST-PARTY-LINE-NOT-LOWER: True
- FIRST-PARTY-BRANCH-NOT-LOWER: True
- PACKAGE UtilitiesCS LINE baseline=38909/43508 final=39100/43704
- PACKAGE UtilitiesCS BRANCH baseline=9434/11293 final=9495/11356
- PACKAGE QuickFiler LINE baseline=10461/12754 final=10468/12761
- PACKAGE QuickFiler BRANCH baseline=2518/3217 final=2518/3217

Reading: Under PD-8 the comparison aggregates every Cobertura class whose filename matches `*EmailParsingSorting\SortEmail*.cs` (one merged class per file) and derives four content-identified exemption sets from the source text of SortEmail.TrySaveAttachment.cs: (1) the unchanged #945 wrapper lambda line containing `System.IO.Directory.CreateDirectory(path)`, observed at line 36; (2) the `}` that closes the `else { throw; }` block, observed at line 190; (3) the `}` that directly follows it, closes the `catch (System.UnauthorizedAccessException e)` handler and is followed by the method's closing brace (the final shape), observed at line 191; (4) the `}` that directly follows the first `throw;` after the single guard condition line containing `isRetryAfterClear` (line 124), observed at line 133. All four exempt lines are uncovered (EXEMPT-UNCOVERED 4, as predicted). Every other uncovered line in the family is printed with its repository-relative path and trimmed source text. The sorted path-and-text set is hashed, and the hash is compared with the baseline hash, so the comparison is by statement identity rather than by line number. The single non-exempt uncovered statement is the `ForEachAsync` call in SortEmail.MailItemSort.cs, which was already uncovered at baseline (line 153 then, line 144 now). The final hash equals the baseline hash. The in-memory negative control adds the lowest-numbered covered non-exempt line of T (line 15) to the set as if it were uncovered, and the resulting hash differs from the baseline hash, so the check can fail. The first-party line and branch percentages are not lower than baseline at two decimals (85.39 against 85.36, 79.81 against 79.75). The package rows are recorded as observations.

## Acceptance (P6-T8, all nine required)

1. SORTEMAIL-DIR-CLASSES 17 (at least 1) and TRYSAVE-CLASS-FOUND: True: met.
2. EXEMPT-LAMBDA-COUNT: 1, EXEMPT-ELSE-BRACE-COUNT: 1, EXEMPT-CATCH-BRACE-COUNT: 1 and EXEMPT-GUARD-BRACE-COUNT: 1, with GUARD-CONDITION-LINES naming exactly one line (124): met.
3. NONEXEMPT-SET-MATCHES-BASELINE: True: met.
4. CONTROL-LINE 15 (greater than 0) and CONTROL-DIFFERS-BASELINE: True: met.
5. FIRST-PARTY-LINE-NOT-LOWER: True and FIRST-PARTY-BRANCH-NOT-LOWER: True: met.
6. EXEMPT-UNCOVERED recorded (4, as predicted): met.
7. The PACKAGE rows for UtilitiesCS and QuickFiler are recorded as observations with no MISSING: met.
8. The artifact lists every EXEMPT-*-LINES value and every NONEXEMPT-UNCOVERED row: met.
9. The artifact contains no absolute path: met.
