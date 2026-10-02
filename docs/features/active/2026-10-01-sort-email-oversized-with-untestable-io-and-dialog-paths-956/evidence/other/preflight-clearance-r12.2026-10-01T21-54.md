# Preflight Clearance - Plan Revision 1.2 (issue 956)

Timestamp: 2026-10-01T21-54
Directive: DIRECTIVE: PREFLIGHT VALIDATION ONLY
Plan: FEATURE/plan.2026-10-01T06-34.md (Version 1.2)
FEATURE: docs/features/active/2026-10-01-sort-email-oversized-with-untestable-io-and-dialog-paths-956
Worktree: <worktree-root> (item worktree for branch bug/sort-email-oversized-with-untestable-io-and-dialog-paths-956)

## Scope

Confirming review of revision 1.2 only (the coordinator ruling AC15 option (a)): plan header lines 6 to 8, PD-7 (plan lines 56 to 62), CMD-SORTEMAIL-COMPARE payload and prose (plan lines 1377 to 1427), P4-T8 (1688), P4-T9 wording (1690), P4-T30 (1732), AC-MAPPING AC15 (1848), the Revision Log (1740 to 1751), the revision-pass 1.2 self-review (1783 to 1790) and the new CITATION lines (1825 to 1827), and FEATURE/spec.md line 271. Phases 0 to 3 and P4-T1 to P4-T7 are executed and checked and were not re-reviewed. No plan task was executed and no file other than this artifact was written.

## Diff scope check

Command: git diff --numstat HEAD -- FEATURE/spec.md FEATURE/plan.2026-10-01T06-34.md
EXIT_CODE: 0
Output Summary:
- spec.md: 1 added, 1 deleted (line 271 only).
- plan: 62 added, 12 deleted; hunks at 6-8, 56-62, 1377, 1390-1399, 1409-1419, 1427, 1688, 1690, 1732, 1740-1752, 1783-1791, 1825-1827, 1848. Every hunk lies in a location the revision declares. No checked task line changed.

Command: git status --porcelain --untracked-files=all
EXIT_CODE: 0
Output Summary: only plan.2026-10-01T06-34.md and spec.md modified; no .cs, .csproj or other file changed (ruling condition (e) holds).

## Dry run of CMD-SORTEMAIL-COMPARE (read-only)

Command: pwsh -NoProfile -Command '<CMD-SORTEMAIL-COMPARE payload, lines joined by "; ", WORKTREE and STAGE=final substituted>' over coverage\baseline-956.cobertura.xml, coverage\final-956.cobertura.xml and the two jacoco projections (git-ignored ITERATION 1 documents), with one trailing diagnostic statement printing PAYLOAD-EXIT
EXIT_CODE: 0
Output Summary:
- SORTEMAIL-CLASS baseline UtilitiesCS\EmailIntelligence\EmailParsingSorting\SortEmail.cs valid=25 covered=24 uncovered=1
- SORTEMAIL-AGG baseline valid=25 covered=24 uncovered=1
- SORTEMAIL-CLASS final UtilitiesCS\EmailIntelligence\EmailParsingSorting\SortEmail.cs valid=5 covered=5 uncovered=0
- SORTEMAIL-CLASS final UtilitiesCS\EmailIntelligence\EmailParsingSorting\SortEmail.AttachmentSaving.cs valid=10 covered=10 uncovered=0
- SORTEMAIL-CLASS final UtilitiesCS\EmailIntelligence\EmailParsingSorting\SortEmail.TrySaveAttachment.cs valid=66 covered=63 uncovered=3
- SORTEMAIL-CLASS final UtilitiesCS\EmailIntelligence\EmailParsingSorting\SortEmail.UndoAndMoveLog.cs valid=8 covered=8 uncovered=0
- SORTEMAIL-CLASS final UtilitiesCS\EmailIntelligence\EmailParsingSorting\SortEmail.MailItemSort.cs valid=1 covered=0 uncovered=1
- SORTEMAIL-AGG final valid=90 covered=86 uncovered=4
- SORTEMAIL-DIR-CLASSES: 17
- EXEMPT-LAMBDA-LINES: 49
- EXEMPT-LAMBDA-COUNT: 1
- EXEMPT-ELSE-BRACE-LINES: 154
- EXEMPT-ELSE-BRACE-COUNT: 1
- EXEMPT-CATCH-BRACE-LINES: 155
- EXEMPT-CATCH-BRACE-COUNT: 1
- TRYSAVE-CLASS-FOUND: True
- EXEMPT-LINES: 49,154,155
- EXEMPT-LINE-COUNT: 3
- EXEMPT-UNCOVERED: 3
- SORTEMAIL-UNCOVERED-DELTA-RAW: 3
- SORTEMAIL-UNCOVERED-DELTA: 0
- CONTROL-LINE: 28
- CONTROL-DELTA: 1
- CONTROL-VERDICT: FAIL
- CONTROL-RAW-DELTA: 4
- CONTROL-RAW-VERDICT: FAIL
- PACKAGE UtilitiesCS LINE baseline=38821/43423 rate=0.894019 final=38909/43508 rate=0.894295 DEFICIT-POINTS=-0.0276 WITHIN-BAND=True
- PACKAGE UtilitiesCS BRANCH baseline=9410/11271 rate=0.834886 final=9434/11293 rate=0.835385 DEFICIT-POINTS=-0.0499 WITHIN-BAND=True
- BASELINE-FIRST-PARTY: First-party coverage: lines 56113/65760 (85.33%), branches 13594/17054 (79.71%)
- FINAL-FIRST-PARTY: First-party coverage: lines 56202/65845 (85.36%), branches 13618/17076 (79.75%)
- PAYLOAD-EXIT: True (diagnostic statement added for the dry run only; not part of the plan payload)

Every value the caller listed as expected was observed. The run executed under Set-StrictMode -Version Latest (set by line 1 of the dot-sourced Helpers script) without error. A git status --porcelain --untracked-files=all after the run showed no new or changed file, so the dry run wrote nothing.

## Findings

1. Command channel (check 1): PASS. The payload contains no single-quote character (a Grep for a single quote on any indented plan line returned no match); every string literal is double-quoted; backslashes in constructed names come from [char]92; no double-quoted literal ends in a backslash (a Grep for a backslash before a double quote returned no match); files are read with -LiteralPath after Set-Location. Every variable is assigned before use: $d, $u, $maps before the stage loop; $exemptUncovered, $ctrlLine, $ctrlU before their loops; $indent and $hdr inside the matching branch before the inner loop. LineMap is a hashtable keyed by [int] (Helpers lines 192 to 213), so $tsMap.Contains($ln) and the numeric Sort-Object of keys behave as the prose states.
2. Content rules (check 2): PASS. Against the current SortEmail.TrySaveAttachment.cs (172 lines): the lambda rule matches line 49 only; the else-brace rule matches line 154 only (line 159 fails because its third preceding trimmed line, 156, is catch (System.Exception), not else); the catch-brace rule matches line 155 only (line 159 fails because line 158 is not an else-brace line). For line 155 (indentation 12) the backward search skips line 125 catch (System.Exception inner) (indentation 20) and selects line 101 catch (System.UnauthorizedAccessException e) (indentation 12). Each rule is keyed by file and containing construct, not by line number, so a shifted line moves with its construct; a second matching construct would raise a count above 1 and fail P4-T8 acceptance rather than pass silently.
3. Negative control (check 3): PASS. CONTROL-LINE is the lowest-numbered line of the final TrySaveAttachment line map with Hits greater than 0 that is not in the exempt union; keys are integers sorted numerically, so the selection involves no executor choice (observed 28). The adjusted control recounts the final-stage maps in memory and observed CONTROL-DELTA 1 = SORTEMAIL-UNCOVERED-DELTA + 1, which confirms the recount agrees with the aggregate (no class node was lost to a filename collision). Both verdicts are computed and can read PASS, in which case P4-T8 stops with AC15 CHECK NOT DISCRIMINATING. Observation (not a defect): CONTROL-RAW-DELTA is computed arithmetically as raw delta + 1 rather than by recount; because CoveredLines counts entries with Hits greater than 0, this is equivalent to a recount for the same line.
4. One-to-one agreement (check 4): PASS. spec.md line 271, PD-7, P4-T8, P4-T30 and AC-MAPPING AC15 each state the same three exemptions identified by file and construct, adjusted delta at most 0 (spec: not greater than the baseline count), raw delta at most 3 (spec: exceeds the baseline count by at most three), and the ninety percent clause (P4-T9 / P4-T30). P4-T8 and P4-T30 additionally require both control verdicts FAIL, which implements ruling condition (b). Dated notes are present in spec AC15, P4-T8, P4-T9 and P4-T30, and each edit is recorded in the Revision Log (condition (c)); P4-T8 writes the ruling into coverage-comparison.md under a dedicated heading (condition (d)).
5. No change beyond the ruling (check 5): PASS. No threshold elsewhere changed; P4-T9 changed only its non-gating prediction wording; no production or test file changed. spec.md still carries pre-existing, unchanged exclusion on exactly lines 126, 127 and 269 (P4-T28 count 3), - L1: to - F3: remain at lines 290 to 296 (P4-T32), and 17 unchecked AC lines remain (P4-T33 regex anchor confirmed working).
6. Canonical format (check 6): PASS. Five phase headings of the form ### Phase N — Title and 75 task lines of the form - [ ] [P#-T#] or - [x] [P#-T#], sequential per phase (Phase 0: 13, Phase 1: 6, Phase 2: 10, Phase 3: 13, Phase 4: 33), matching the header Task Count. P0-T1 to P4-T7 checked; P4-T8 to P4-T33 unchecked.

## Limitations

- The ruling text quoted in PD-7 lines 57 to 62 could not be compared against the coordinator's original message, which is not on disk; the delegation prompt carries a paraphrase whose substance (three exemptions, two thresholds, conditions (a) to (e), stop clause) matches the PD-7 quotation.
- The validate_orchestration_artifacts MCP tool is not available to this agent and scripts/dev_tools/validate_orchestration_artifacts.py does not exist in this worktree; the canonical-format check above was performed with Grep against the phase-heading and task-line patterns.

## Signal

PREFLIGHT: ALL CLEAR
CONVERGENCE: NO FURTHER ROUNDS EXPECTED
