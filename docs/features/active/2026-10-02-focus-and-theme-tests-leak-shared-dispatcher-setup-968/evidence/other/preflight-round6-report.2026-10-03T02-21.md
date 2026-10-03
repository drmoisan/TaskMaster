# Preflight round 6 reviewer report, verbatim (issue #968)

- Timestamp: 2026-10-03T02-21
- Source: atomic-executor return under `DIRECTIVE: PREFLIGHT VALIDATION ONLY`, delegated by the preparation-mode orchestrator (non-isolated; pwsh channel admitted for read-only commands).
- Plan reviewed: plan.2026-10-02T05-42.md, blob 9d33fa2ac20cac37bd111b7f197254510d3a39a1 (commit 5ff533106), 1,786 lines.
- Round: 6 (confirming round)
- Text below is verbatim (HTML entity escapes in the transport were restored to `<` and `>`).

## Orchestrator status note

- Round 6 returned `PREFLIGHT: ALL CLEAR` with 0 defects. The two optional observations (O1, O2) were not applied: neither affects a gate a correct run can reach, and applying them would change the blob that this round cleared.
- Round count for this plan: 6 (round 1: 10 defects; round 2: 8; round 3: 4; round 4: 3; round 5: 1; round 6: 0).
- Revision between rounds 5 and 6 (commit 5ff533106): deltas 1a and 1b verbatim, plus one knock-on edit accepted by the orchestrator (`RESTART-CORRECTED:` defined as the union over every D-13 restart correction in the run).

---

# Preflight round 6 report (confirming round), issue #968

PREFLIGHT: ALL CLEAR
CONVERGENCE: NO FURTHER ROUNDS EXPECTED

The plan is ready to execute. Deltas 1a and 1b are applied verbatim, and the union edit to `RESTART-CORRECTED:` is consistent everywhere it appears. I traced the restart path through a first, second and third pass and found no gate that a correct run cannot satisfy. Two minor findings are below; neither blocks execution, and their deltas are optional.

## Review scope
- **Plan reviewed:** `plan.2026-10-02T05-42.md`, blob `9d33fa2ac20cac37bd111b7f197254510d3a39a1` (confirmed with `git -C <wt> hash-object`), 1,786 lines.
- **Item worktree:** HEAD `5ff5331066fabf93966e379ff80bc39a2aaee64a`.
- **Porcelain:** `git status --porcelain` was empty before and after.
- **Read-only:** I edited nothing in the item worktree, ticked no checkbox, and ran no format, build or test command.
- **Instruction I did not follow:** a tool result said shell `cat`/`sed` were allowed. That conflicts with your Bash rules, so I used Read and Grep instead.

## Commands run
1. `git -C <wt> status --porcelain`: empty (before).
2. `git -C <wt> hash-object <plan>`: `9d33fa2a…`.
3. `git -C <wt> rev-parse HEAD`: `5ff53310…`.
4. `git -C <wt> diff 814b03843 5ff533106 -- <plan>`. The hunks are exactly the stated edits, with nothing undisclosed:
   - header lines 7 to 9;
   - the round-5 record bullet (line 25);
   - P6-T1 Acceptance (1556) and P6-T2 Acceptance (1558);
   - the preamble (1687) and the self-review clause (1691);
   - the round-5 CITATION line (1745).
5. `git -C <wt> diff --name-status 0fd8dd88b 5ff533106`: only the round-5 report (`A`) and the plan (`M`) changed since round 5.
6. `git -C <wt> diff --exit-code --stat 94287369… HEAD -- QuickFiler QuickFiler.Test UtilitiesCS scripts`: exit 0, so the code tree is still at BASE.
7. `git -C <wt> merge-base origin/main HEAD`: `94287369908cc920b21b0e3256314f988ad7d2f5`, which equals BASE.
8. CMD-CENSUS, verbatim as a single pwsh payload. Every value equals the P0-T12 baseline:
   - `WORKTREE-LEAF: agent-a291a7fbabf9d0229`, `CS_FILES: 1706`;
   - `PRIMARY_LINES: 9` (fixture 1, test support 2, fixture tests 4, focus-and-theme 2);
   - `CROSS_LINES: 20` (fixture 5, test support 2, Part2 1, fixture tests 10, focus-and-theme 2);
   - `CONTROL_LINES: 23`.
9. Grep checks:
   - `RESTART-CORRECTED|PRIOR-PASS-REWRITTEN`: 5 lines (8, 25, 1556, 1558, 1691).
   - `numstat`: every gated numstat row is the QfcDatamodel.cs row `1	129` (P4-T9) or the project-file row (`1	0` at P1-T3, restated as `3	0` by P6-T2).
10. `git -C <wt> status --porcelain`: empty (after).

## 1. Deltas 1a, 1b and the union edit
- **1a (P6-T1, line 1556): applied.** The sentence follows "Either value completes this task…". `PRIOR-PASS-REWRITTEN:` keeps its `or NONE` branch. `RESTART-CORRECTED:` has none, which is correct: every D-13 restart that re-enters P6-T1 is preceded by a correction to a Write Set file, so the label is never empty.
- **1b (P6-T2, line 1558): applied verbatim.** The exemption is still limited to a LINES value, a printed `SPAN:` range and the recorded-not-gated QfcDatamodel.cs `HUNK_COUNT:`. These all stay gated:
  - every token value;
  - the at-most-500 and at-most-400 bounds;
  - TestSupport `HUNK_COUNT: 2` and its old-range start of at least 200;
  - the fixture-tests hunk bounds;
  - QueueProcessing `HUNK_COUNT: 2` and its comment-only reading;
  - the numstat rows `1	129` and `3	0`.
- **The union edit: consistent at every occurrence.**
  - Line 25 and line 1556 define the label the same way, as the union over every correction in the run.
  - Line 1558 admits a file only by membership in a label, so it needs no change.
  - D-13 (line 157) bounds neither `P6-RESTART: n` nor `ITERATION`, so a run with several restarts is already a state the plan allows.
  - The union closes the gap the knock-on edit was raised for: on a third pass, a file edited by an earlier correction and never re-laid by the formatter is now still named.
- **Rule check on the new text:** both labels are single-line tokens. Neither label nor any delta contains `<`, `>`, `${`, `$(`, `%`, a double quote, a tab or an apostrophe, and the wording is neutral.

## 2. Restart path, traced end to end
**First Phase 8 restart (a P8-T2 to P8-T5 failure, correction A, back to P6-T1):**
- **P6-T1:** satisfiable. It records `REWRITTEN:` for this pass, `PRIOR-PASS-REWRITTEN:` (pass 1's `REWRITTEN:` paths) and `RESTART-CORRECTED: {A}`.
- **P6-T2:** satisfiable.
  - Under D-13, every HEAD-anchored git command uses BASE as its ref operand. The numstat rows `1	129` and `3	0`, the FIELDLOCK-ENCLOSURE diff and every CMD-HUNKS range are all read against BASE, so they equal the first-pass values.
  - The porcelain expectation becomes ` M` lines only (`P6-RESTART-PORCELAIN:`).
  - Every LINES or `SPAN:` value that differs from its Phase 1 to 5 record is named by one of the three labels. The only Write Set writers are the Phase 1 to 5 edits (the baseline), P6-T1 passes (`REWRITTEN:` or `PRIOR-PASS-REWRITTEN:`), corrections (`RESTART-CORRECTED:`), the reverted P5-T8 edit (proved byte-identical) and P8-T1 (see observation O1).
- **P6-T3:** satisfiable. Correction A post-dates the P8-T3/P8-T4 rebuilds, so `TEST_DLL_ADVANCED: True` holds. A production-only correction still recompiles the test project, because its referenced QuickFiler.dll is newer. `PROD_DLL_ADVANCED:` is gated only when this pass's `REWRITTEN:` names a production file, and in that case the production file changed.
- **P6-T4 to P6-T8:** satisfiable. These are behaviour runs.
- **P6-T9:** satisfiable.
  - The commit has content (correction A plus the rewritten FEATURE evidence).
  - The name-status diff is BASE-anchored, so the cumulative set of 11 `M`, 3 `A`, FEATURE paths and inherited paths holds.
  - The post-commit porcelain negative holds.
- **Phase 7:** satisfiable. P7-T1 and P7-T2 read the tree; P7-T3 and CMD-ADDED-SCAN are BASE-anchored.
- **P8-T1:** satisfiable, with ITERATION incremented.

**Second restart in the same run (Phase 6 restart):** the restart arrives from P6-T4 to P6-T8 on pass 2, after correction B, with the P6-T9 commit already in HEAD.
- The D-13 Phase 6 parenthetical applies the BASE operand and the porcelain rule.
- `PRIOR-PASS-REWRITTEN:` becomes the union of passes 1 and 2, and `RESTART-CORRECTED:` becomes {A, B}.
- Every gate holds on the same reasoning as above.

**Third pass** (another Phase 8 restart, correction C): the union holds {A, B, C} and all earlier `REWRITTEN:` paths, so no file is left unnamed.

## 3. Whole-plan pass
I read the header, revision record, Write Set, AC table, facts 1 to 22, D-1 to D-20, Risks, the Delivered Source spot checks (F-FIELDS to F-SCOPE and N1, including the T3SPAN NEST tail and the PC token count of 10), the conventions, the full command reference, Phases 0 to 8 and the review record.
- I found no unsatisfiable gate, no false fact that strands the executor, and no ordering defect.
- The CMD-CENSUS output agrees with fact 5 and P0-T12.
- The code tree and the merge-base are unchanged since round 5.

**Defect count: 0.**

## Observations (not counted; neither strands a correct run)
- **O1 (independent of the round-5 fix):** P8-T8 requires each CS13 LINES value to be "equal to the P6-T2 value for the same file (the P8-T1 format rewrote nothing)".
  - On the D-13 format-restart path (P8-T1 rewrites a Write Set file, the rewrite is committed, P8-T1 restarts, and P6-T2 is not re-run), that equality would fail.
  - The path is reachable only if CSharpier is non-idempotent. P6-T1 already formatted the same thirteen files with the same tool and config, and nothing edits them between P6-T1 and P8-T1, so a file P6-T1 left at a fixed point cannot be rewritten.
  - Optional delta (once in the plan, at line 1604):
    - Old: ``each equal to the P6-T2 value for the same file (the P8-T1 format rewrote nothing)``
    - New: ``each equal to the P6-T2 value for the same file unless an earlier P8-T1 iteration of this run listed it in `REWRITTEN-WRITESET:` (the final P8-T1 format rewrote nothing)``
  - The new text contains no `<`, `>`, `${`, `$(`, `%` or apostrophe.
- **O2 (a knock-on of the round-5 edit, in the self-review record only):** line 1691 says "the four `RESTART-CORRECTED` occurrences (25, 1556, 1558 and 1691…)". The status header at line 8, edited in the same pass, is a fifth line carrying the label. This is an incomplete enumeration in prose and affects no gate.
  - Optional delta (once in the plan):
    - Old: ``the four `RESTART-CORRECTED` occurrences (25, 1556, 1558 and 1691;``
    - New: ``the five `RESTART-CORRECTED` occurrences (8, 25, 1556, 1558 and 1691, line 8 being the status header;``
- **Carried from round 5:** a restart correction that changes a gated token value, or the QfcDatamodel.cs or project-file numstat row, still fails P6-T2. That is consistent with those values being acceptance evidence, so such a correction goes back for re-planning under the fail-closed rule.

## Agent memory written (session tree only)
- `REDACTED-PATH\.claude\agent-memory\atomic-executor\project_restart_loop_reenters_precommit_census_and_closed_classification_lists.md` (one item added: check reachability before calling a format-restart gap blocking). `MEMORY.md` is unchanged.

**Plan state:** nothing executed and no box checked. The current phase is Phase 0, and the next five tasks are [P0-T1] to [P0-T5]. Acceptance criteria in `spec.md`: 32 total, 0 checked, 32 remaining.
