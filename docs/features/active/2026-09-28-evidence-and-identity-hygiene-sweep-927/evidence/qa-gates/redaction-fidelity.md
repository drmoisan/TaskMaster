# P4-T7 Redaction fidelity (-VerifyDiff) and AC11 consolidation

Timestamp: 2026-09-29T20-06
Command: pwsh -NoProfile -Command '$d = Join-Path $env:TEMP "hygiene-927"; & (Join-Path $d "Invoke-IdentifierRedaction.ps1") -VerifyDiff 2>&1 | Tee-Object -FilePath (Join-Path $d "verify-diff.txt"); exit $LASTEXITCODE'
EXIT_CODE: 0
Output Summary:
- REMOVED-LINES=31761
- ADDED-LINES=31761 (equals REMOVED-LINES)
- REMOVED-UNMATCHED=8 (the acceptance requires 0: NOT MET as measured; see DEVIATION below)
- VERIFY-DECODED-FILES=1 (equals the P4-T4 UTF16-SCANNED=1)
- VERIFY-WRITTEN-SET=1052 (equals the P4-T4 FILES-WRITTEN=1052)
- P4-T7 is left unchecked in the plan because one of its five figures does not meet the stated condition.

DEVIATION (REMOVED-UNMATCHED=8):
- All 8 unmatched removed lines are MSBuild parallel-log lines of the form `<N>>CoreClean:` in two evidence files of the issue 629 feature folder: 3 in evidence/qa-gates/p2-t3-analyzer-build.md and 5 in evidence/qa-gates/p2-t4-nullable-build.md. None carries an identifier token.
- Cause: git diff -U0 aligns runs of near-identical log lines ambiguously. In each file it reports these `CoreClean:` lines as removed and the same lines as added at a shifted position. The redaction did not change them.
- Positional proof (read-only; the working file compared line by line against git show HEAD:<path>):
  - p2-t3-analyzer-build.md: HEAD-LINES=12022, WORK-LINES=12022, POSITIONAL-CHANGED=7764, POSITIONAL-UNMATCHED=0; diff removed 7767, added 7767; CoreClean lines removed 3, added 3; CoreClean line count 3355 in HEAD and 3355 in the working file.
  - p2-t4-nullable-build.md: HEAD-LINES=11798, WORK-LINES=11798, POSITIONAL-CHANGED=7764, POSITIONAL-UNMATCHED=0; diff removed 7769, added 7769; CoreClean lines removed 5, added 5; CoreClean line count 3387 in HEAD and 3387 in the working file.
  - The difference between the diff counts and the positional counts (3 and 5) equals the realigned CoreClean lines in each file.
- The helper was not changed and no alternative diff algorithm was selected to bring the figure to 0, because that would choose the evidence the task is judged against. The deviation is recorded for the orchestrator's decision (C12: no stop for this task).

AC11 consolidation (each figure named with the task that produced it):
- P4-T3: EOL-LINES=14850, EOL-BINARY=44, EOL-CRLF=14754, EOL-LF=39, EOL-MIXED=0
- P4-T4: FILES-WRITTEN=1052 (the reference figure for the compared counts); XML-REWRITTEN=0; XML-REPARSE-FAILED=0; UTF16-SCANNED=1
- P4-T5: second run FILES-WITH-MATCHES=0, FILES-WRITTEN=0, RULE1=0 through RULE8=0; NUMSTAT-HASH-BEFORE equals NUMSTAT-HASH-AFTER (5590E8BE8529637EC4BE96EF2B96B65ED941153BEB98A1FA74584FB63DA250C9)
- P4-T6: WRITTEN-COMPARED=1052, EOL-MISMATCH=0, BOM-COMPARED=1052, BOM-MISMATCH=0
- P4-T7 (this task): REMOVED-LINES=31761, ADDED-LINES=31761, REMOVED-UNMATCHED=8, VERIFY-DECODED-FILES=1, VERIFY-WRITTEN-SET=1052

## Multiset comparison (revision 1.14)

Timestamp: 2026-09-29T22-12
Command: the P4-T7 multiset payload of plan revision 1.16, verbatim, run as one pwsh segment from Bash and prefixed only by a Set-Location to the item worktree (the path assembled at run time from the user-profile environment variable with the worktrees segment split across two string literals, per C1). The payload as run, with that prefix shown by expression only:

```text
pwsh -NoProfile -Command '$w = $env:USERPROFILE + "/repos/TaskMaster/.claude/" + "work" + "trees/<worktree-leaf>"; Set-Location -LiteralPath $w; [System.IO.Directory]::SetCurrentDirectory((Get-Location).Path); $b = [char]92; $sep = "[" + $b + $b + "/]+"; $profileRx = "[a-z]:" + $sep + "users" + $sep + "[a-z0-9_.~-]"; ... (remainder identical to the P4-T7 Command in plan revision 1.16, through "CONTROL-MULTISET-UNMATCHED=" + $cc.Unmatched; exit 0)'
```

EXIT_CODE: 0 (the `-VerifyDiff` exit code recorded above stays the artifact's EXIT_CODE row, per C3)
Output Summary:
- MULTISET-EXIT=0 (the multiset payload's exit code)
- TOKENS=6 (at most 7; one derived token was empty or a duplicate)
- PROFILE-RX-LENGTH=35
- MULTISET-FILES=1052 (equals the P4-T4 FILES-WRITTEN=1052 and the P4-T9 COMMIT-OUTSIDE-FEATURE=1052)
- MULTISET-UTF16| docs/features/archive/2026-05-14-ci-format-and-vs-test-failures-155/evidence/baseline/2026-05-14T12-41-05Z/msbuild-analyzers.txt (exactly one line)
- MULTISET-UTF16-DECODED=1
- LINECOUNT-MISMATCH=0 (no LINECOUNT| line printed)
- MULTISET-REMOVED=31753
- MULTISET-ADDED=31753 (equals MULTISET-REMOVED; 31753 is the diff-reported 31761 less the eight moved lines)
- MULTISET-UNMATCHED=0 (no UNMATCHED| line printed)
- CORECLEAN-FILE| docs/features/active/2026-08-26-qfc-remove-stackmoveditems-parameter-629/evidence/qa-gates/p2-t3-analyzer-build.md | removed=7764 added=7764 unmatched=0
- CORECLEAN-FILE| docs/features/active/2026-08-26-qfc-remove-stackmoveditems-parameter-629/evidence/qa-gates/p2-t4-nullable-build.md | removed=7764 added=7764 unmatched=0
- Negative control (in memory, nothing written): CONTROL-DROPPED-LINE-NUMBER=8, CONTROL-DROPPED-LINE-TOKEN-FREE=True, CONTROL-MULTISET-REMOVED=7765 (the p2-t3 removed=7764 plus 1), CONTROL-MULTISET-ADDED=7764 (equal to the p2-t3 added=7764), CONTROL-MULTISET-UNMATCHED=1

Ruling 2 disposition:
- The former acceptance clause REMOVED-UNMATCHED=0 is withdrawn by Ruling 2 and replaced by MULTISET-UNMATCHED=0, which is met.
- REMOVED-UNMATCHED=8 above is the diff-reported figure and is superseded as a gate. The eight diff-reported `CoreClean:` lines are 3 in the issue 629 folder's evidence/qa-gates/p2-t3-analyzer-build.md and 5 in its evidence/qa-gates/p2-t4-nullable-build.md. The diff aligner reported them as removed and re-added at a shifted position while their per-file counts (3355 and 3387) are identical on both sides, so they were moved by the aligner and not changed by the redaction. The multiset comparison nets a moved line to zero, and both files show unmatched=0.
- The negative control drops one token-free line from an in-memory copy of the post side and yields exactly one unmatched removed line, which shows the genuine zero is a result of the data and not a property of the method.

AC11 consolidation, multiset figures (P4-T7): MULTISET-FILES=1052, MULTISET-UNMATCHED=0, LINECOUNT-MISMATCH=0, MULTISET-UTF16-DECODED=1, CONTROL-MULTISET-UNMATCHED=1.
