# Coverage Comparison (P8-T10)

Timestamp: 2026-10-06T18-06
ITERATION: 3
SUPERSEDES: d1f6f85fcea83a7fcf634c089b5038b37dddc554
WRITTEN-BY: P8-T10
Command: (1) CMD-COVERAGE-TEXTS (STAGE final, BASELINE-HASH 408A19E922396ADCFF033FE8156DCFE3C818FA8A5484469691071CC2176E142D, the BASELINE-NONEXEMPT-HASH of coverage-baseline.md, as in P7-T12); (2) CMD-MEMBER-COVERAGE over coverage\final-959.cobertura.xml; (3) CMD-LINE-CONDITION (DOC coverage\final-959.cobertura.xml); each run as pwsh -NoProfile -Command with Set-Location to the item worktree over the merged-tree coverage\final-959.cobertura.xml written by P8-T9, coverage\baseline-959.cobertura.xml and the two JaCoCo projections
EXIT_CODE: 0 (scoped to the CMD-LINE-CONDITION payload, the last invocation, its process exit code)
Output Summary:
- CMD-COVERAGE-TEXTS:
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
- FINAL-FIRST-PARTY: First-party coverage: lines 56439/66084 (85.40%), branches 13687/17145 (79.83%)
- FIRST-PARTY-LINE-NOT-LOWER: True
- FIRST-PARTY-BRANCH-NOT-LOWER: True
- PACKAGE UtilitiesCS LINE baseline=38909/43508 final=39104/43704
- PACKAGE UtilitiesCS BRANCH baseline=9434/11293 final=9496/11356
- PACKAGE QuickFiler LINE baseline=10461/12754 final=10467/12761
- PACKAGE QuickFiler BRANCH baseline=2518/3217 final=2518/3217
- CMD-MEMBER-COVERAGE:
- CLASS-NODES T = 1
- CLASS-NODES E = 1
- CLASS-NODES A = 1
- CLASS-NODES U = 1
- MEMBER SaveAttachmentAsyncCore span=194-224 valid=21 covered=21 percent=100 uncovered=
- MEMBER SaveAttachmentCore span=130-156 valid=19 covered=19 percent=100 uncovered=
- MEMBER SaveCaseAsync span=253-287 valid=20 covered=20 percent=100 uncovered=
- MEMBER SaveCase span=289-309 valid=8 covered=8 percent=100 uncovered=
- MEMBER RedirectSaveFolder span=244-251 valid=4 covered=4 percent=100 uncovered=
- MEMBER Cleanup_Files span=40-46 valid=6 covered=6 percent=100 uncovered=
- MEMBER TrySaveAttachmentCoreAsync span=98-192 valid=70 covered=67 percent=95.71 uncovered=133,190,191
- MEMBER WriteCsvCore span=171-188 valid=10 covered=10 percent=100 uncovered=
- MEMBER ResetFilerPromptState span=344-347 valid=3 covered=3 percent=100 uncovered=
- E-CHANGED-LINE result = await InvokeFilerAsync(config, mailHelpers); matches=1 line=311 hits=1
- E-CHANGED-LINE ResetFilerPromptState(); matches=1 line=318 hits=1
- E-CHANGED-LINE return result; matches=1 line=320 hits=1
- E-CHANGED-LINES-COVERED: 3
- MEMBERS-AMBIGUOUS: 0
- MEMBERS-UNMEASURED: 0
- MEMBERS-BELOW-90: 0
- CMD-LINE-CONDITION:
- A-CLASS-NODES: 1
- A-LINE-143-COUNT: 1
- A-LINE-143-BRANCH: True
- A-LINE-143-CONDITION: 100% (2/2)
- A-SAVEATTACHMENT-BRANCH-RATE: 1
- PHASE7-LINE-NOT-LOWER: True (P8-T9 FIRST-PARTY-LINE-PERCENT 85.40 against the Phase 7 85.39; recorded, not gated)
- PHASE7-BRANCH-NOT-LOWER: True (P8-T9 FIRST-PARTY-BRANCH-PERCENT 79.83 against the Phase 7 79.81; recorded, not gated)
- Observation: no POST-MERGE RATE LOWER report applies, because all four NOT-LOWER flags are True. At package level the QuickFiler LINE counter reads 10467/12761 against 10468/12761 at Phase 7 (one covered line fewer, same denominator); the QuickFiler BRANCH counter is unchanged and the SortEmail family and EfcDataModel member rows are identical to Phase 7, so the one-line difference lies outside the lines this item changed. P8-T3 recorded an empty MAIN-TOUCHED-WRITE-SET-CODE, so the fixed exemption, baseline-hash and control expectations apply.

Reading: The merged tree carries origin/main f8ea1b5dcc6514bc0088bc80965c188bfd717557, which changed no SortEmail family file and no EfcDataModel source (P8-T3), so the SortEmail family statements, the four content-identified exemption sets of SortEmail.TrySaveAttachment.cs (lines 36, 133, 190 and 191; guard condition line 124) and the single non-exempt uncovered `ForEachAsync` statement of SortEmail.MailItemSort.cs are the same as in Phase 7, and the non-exempt set hash equals the baseline hash. The in-memory negative control (line 15 of T treated as uncovered) changes the hash, so the check can fail. All nine AC25 members read at least 90 percent (lowest 95.71, whose three uncovered lines are all exempt) and the three EfcDataModel changed lines are executed. Line 143 of SortEmail.AttachmentSaving.cs reads `100% (2/2)` with the synchronous SaveAttachment method at branch-rate 1, so the CR-1 arm remains covered after the merge. The first-party line and branch percentages are not lower than baseline (85.40 against 85.36, 79.83 against 79.75) or than Phase 7 (85.40 against 85.39, 79.83 against 79.81); the Phase 7 comparison is an observation because the merged tree carries main's own code and tests, and the P8-T9 floors remain the gate.

## Acceptance (P8-T10, all seven required)

1. SORTEMAIL-DIR-CLASSES 17 (at least 1) and TRYSAVE-CLASS-FOUND: True: met.
2. MAIN-TOUCHED-WRITE-SET-CODE empty at P8-T3: the four EXEMPT-*-COUNT values 1 with GUARD-CONDITION-LINES naming one line (124), NONEXEMPT-SET-MATCHES-BASELINE: True, CONTROL-DIFFERS-BASELINE: True with CONTROL-LINE 15 greater than 0: met.
3. FIRST-PARTY-LINE-NOT-LOWER, FIRST-PARTY-BRANCH-NOT-LOWER, PHASE7-LINE-NOT-LOWER and PHASE7-BRANCH-NOT-LOWER recorded (all True): met.
4. Every MEMBER row percent at least 90 with MEMBERS-BELOW-90: 0, MEMBERS-AMBIGUOUS: 0, MEMBERS-UNMEASURED: 0 and E-CHANGED-LINES-COVERED: 3: met.
5. A-CLASS-NODES: 1, A-LINE-143-COUNT: 1, A-LINE-143-BRANCH: True and A-LINE-143-CONDITION: 100% (2/2) with A-SAVEATTACHMENT-BRANCH-RATE: 1: met.
6. Every EXEMPT-*-LINES value and the NONEXEMPT-UNCOVERED row listed: met.
7. No absolute path in this artifact: met.
