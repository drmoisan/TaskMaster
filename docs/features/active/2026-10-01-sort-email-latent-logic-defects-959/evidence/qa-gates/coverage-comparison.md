# Coverage Comparison (P7-T12)

Timestamp: 2026-10-06T17-21
ITERATION: 2
SUPERSEDES: a71d0c61d67252808c648c418631bfb333c70b1c
WRITTEN-BY: P7-T12
Command: CMD-COVERAGE-TEXTS (STAGE final, BASELINE-HASH 408A19E922396ADCFF033FE8156DCFE3C818FA8A5484469691071CC2176E142D, the BASELINE-NONEXEMPT-HASH of coverage-baseline.md), run as pwsh -NoProfile -Command with Set-Location to the item worktree over the Phase 7 coverage\final-959.cobertura.xml, coverage\baseline-959.cobertura.xml and the two JaCoCo projections
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
- FINAL-FIRST-PARTY: First-party coverage: lines 56410/66058 (85.39%), branches 13680/17141 (79.81%)
- FIRST-PARTY-LINE-NOT-LOWER: True
- FIRST-PARTY-BRANCH-NOT-LOWER: True
- PACKAGE UtilitiesCS LINE baseline=38909/43508 final=39100/43704
- PACKAGE UtilitiesCS BRANCH baseline=9434/11293 final=9494/11356
- PACKAGE QuickFiler LINE baseline=10461/12754 final=10468/12761
- PACKAGE QuickFiler BRANCH baseline=2518/3217 final=2518/3217
- PHASE6-LINE-NOT-LOWER: True (P7-T11 FIRST-PARTY-LINE-PERCENT 85.39 against the Phase 6 85.39; recorded, not gated)
- PHASE6-BRANCH-NOT-LOWER: True (P7-T11 FIRST-PARTY-BRANCH-PERCENT 79.81 against the Phase 6 79.81; recorded, not gated)
- Observation: at two decimals the Phase 7 first-party rates equal Phase 6; the branch numerator is 13680 against 13681 at Phase 6, and the UtilitiesCS BRANCH counter reads 9494/11356 against 9495/11356 at Phase 6, so a one-branch difference outside the SortEmail family offsets the SS4 arm (P7-T13 reads the SortEmail.AttachmentSaving.cs line-143 condition directly). No PHASE 7 FIRST-PARTY RATE BELOW PHASE 6 report applies because both Phase 6 comparison rows are True.

Reading: Under PD-8 the comparison aggregates every Cobertura class whose filename matches `*EmailParsingSorting\SortEmail*.cs` (one merged class per file) and derives four content-identified exemption sets from the source text of SortEmail.TrySaveAttachment.cs: (1) the unchanged #945 wrapper lambda line containing `System.IO.Directory.CreateDirectory(path)`, observed at line 36; (2) the `}` that closes the `else { throw; }` block, observed at line 190; (3) the `}` that directly follows it, closes the `catch (System.UnauthorizedAccessException e)` handler and is followed by the method's closing brace, observed at line 191; (4) the `}` that directly follows the first `throw;` after the single guard condition line containing `isRetryAfterClear` (line 124), observed at line 133. Every other uncovered line in the family is printed with its repository-relative path and trimmed source text, the sorted path-and-text set is hashed, and the hash is compared with the baseline hash, so the comparison is by statement identity rather than by line number. The single non-exempt uncovered statement is the `ForEachAsync` call in SortEmail.MailItemSort.cs, already uncovered at baseline; the final hash equals the baseline hash. The in-memory negative control adds the lowest-numbered covered non-exempt line of T (line 15) as if it were uncovered and the resulting hash differs from the baseline hash, so the check can fail. The first-party line and branch percentages are not lower than baseline at two decimals (85.39 against 85.36, 79.81 against 79.75). The package rows are observations. Phase 7 changed no production line; the SortEmail family rows are identical to Phase 6, which is the no-regression rule for the lines this item changed.

## Acceptance (P7-T12, all ten required)

1. SORTEMAIL-DIR-CLASSES 17 (at least 1) and TRYSAVE-CLASS-FOUND: True: met.
2. EXEMPT-LAMBDA-COUNT, EXEMPT-ELSE-BRACE-COUNT, EXEMPT-CATCH-BRACE-COUNT and EXEMPT-GUARD-BRACE-COUNT each 1, with GUARD-CONDITION-LINES naming one line (124): met.
3. NONEXEMPT-SET-MATCHES-BASELINE: True: met.
4. CONTROL-LINE 15 (greater than 0) and CONTROL-DIFFERS-BASELINE: True: met.
5. FIRST-PARTY-LINE-NOT-LOWER: True and FIRST-PARTY-BRANCH-NOT-LOWER: True: met.
6. EXEMPT-UNCOVERED recorded (4): met.
7. The PACKAGE rows recorded with no MISSING: met.
8. Every EXEMPT-*-LINES value and the NONEXEMPT-UNCOVERED row listed: met.
9. No absolute path in this artifact: met.
10. SortEmail family no-regression: SORTEMAIL-AGG reads valid=286 covered=281 uncovered=5 and the five SORTEMAIL-CLASS rows carry the Phase 6 values (SortEmail.cs 5/5, SortEmail.AttachmentSaving.cs 142/142, SortEmail.TrySaveAttachment.cs 93/89, SortEmail.UndoAndMoveLog.cs 45/45, SortEmail.MailItemSort.cs 1/0), with PHASE6-LINE-NOT-LOWER and PHASE6-BRANCH-NOT-LOWER recorded as observations (both True): met.

## Per-member coverage and the CR-1 arm (P7-T13)

Timestamp: 2026-10-06T17-22
ITERATION: 1
Command: (1) CMD-MEMBER-COVERAGE over coverage\final-959.cobertura.xml (the Phase 7 document); (2) CMD-LINE-CONDITION over coverage\final-959.cobertura.xml; each run as pwsh -NoProfile -Command with Set-Location to the item worktree
EXIT_CODE: 0 (scoped to the CMD-LINE-CONDITION payload, the last invocation, its process exit code)

CMD-MEMBER-COVERAGE:

```
CLASS-NODES U = 1
CLASS-NODES A = 1
CLASS-NODES T = 1
CLASS-NODES E = 1
MEMBER SaveAttachmentAsyncCore span=194-224 valid=21 covered=21 percent=100 uncovered=
MEMBER SaveAttachmentCore span=130-156 valid=19 covered=19 percent=100 uncovered=
MEMBER SaveCaseAsync span=253-287 valid=20 covered=20 percent=100 uncovered=
MEMBER SaveCase span=289-309 valid=8 covered=8 percent=100 uncovered=
MEMBER RedirectSaveFolder span=244-251 valid=4 covered=4 percent=100 uncovered=
MEMBER Cleanup_Files span=40-46 valid=6 covered=6 percent=100 uncovered=
MEMBER TrySaveAttachmentCoreAsync span=98-192 valid=70 covered=67 percent=95.71 uncovered=133,190,191
MEMBER WriteCsvCore span=171-188 valid=10 covered=10 percent=100 uncovered=
MEMBER ResetFilerPromptState span=344-347 valid=3 covered=3 percent=100 uncovered=
E-CHANGED-LINE result = await InvokeFilerAsync(config, mailHelpers); matches=1 line=311 hits=1
E-CHANGED-LINE ResetFilerPromptState(); matches=1 line=318 hits=1
E-CHANGED-LINE return result; matches=1 line=320 hits=1
E-CHANGED-LINES-COVERED: 3
MEMBERS-AMBIGUOUS: 0
MEMBERS-UNMEASURED: 0
MEMBERS-BELOW-90: 0
```

CMD-LINE-CONDITION (Phase 7 document; the P7-T1 reading of the Phase 6 document was `A-LINE-143-CONDITION: 50% (1/2)` with `A-SAVEATTACHMENT-BRANCH-RATE: 0.75`):

```
A-CLASS-NODES: 1
A-LINE-143-COUNT: 1
A-LINE-143-BRANCH: True
A-LINE-143-CONDITION: 100% (2/2)
A-SAVEATTACHMENT-BRANCH-RATE: 1
```

### Acceptance (P7-T13, all seven required)

1. Every CLASS-NODES value at least 1 (1, 1, 1, 1): met.
2. MEMBERS-AMBIGUOUS: 0 and MEMBERS-UNMEASURED: 0: met.
3. Every MEMBER row shows percent at least 90 (lowest 95.71) and MEMBERS-BELOW-90: 0, with MEMBER SaveAttachmentCore span=130-156 valid=19 covered=19 percent=100: met.
4. Each E-CHANGED-LINE row shows matches=1 and hits greater than 0, and E-CHANGED-LINES-COVERED: 3: met.
5. The TrySaveAttachmentCoreAsync uncovered value 133,190,191 is a subset of the P7-T12 EXEMPT-LINES 36,133,190,191: met.
6. A-CLASS-NODES: 1, A-LINE-143-COUNT: 1 and A-LINE-143-BRANCH: True: met.
7. A-LINE-143-CONDITION: 100% (2/2) with A-SAVEATTACHMENT-BRANCH-RATE: 1 (50% (1/2) and 0.75 at P7-T1, the false-before state): met.
