# Preflight round 5 reviewer report, verbatim (issue #968)

- Timestamp: 2026-10-03T01-53
- Source: atomic-executor return under `DIRECTIVE: PREFLIGHT VALIDATION ONLY`, delegated by the preparation-mode orchestrator (non-isolated; pwsh channel admitted).
- Plan reviewed: plan.2026-10-02T05-42.md, blob 2457e1df7582e5a02282a9c147a63843b55dcd3a (commit 0fd8dd88b), 1,784 lines.
- Round: 5 (the last round the coordinator authorised in this preparation)
- Text below is verbatim (HTML entity escapes in the transport were restored to `<` and `>`).

## Orchestrator status note

- The plan is NOT cleared. Round 5 returned `PREFLIGHT: REVISIONS REQUIRED`. Per the coordinator's limit (rounds 3 to 5 only), the round-5 deltas (1a and 1b below) were NOT applied and no round 6 was run. No `preflight-clearance` artifact exists.
- Round count for this plan: 5 (round 1: 10 defects; round 2: 8; round 3: 4; round 4: 3; round 5: 1).
- Next action (requires coordinator authorisation): atomic-planner applies deltas 1a and 1b verbatim in place, the MCP plan validator is re-run, and a round-6 confirming preflight is run.

---

PREFLIGHT: REVISIONS REQUIRED
CONVERGENCE: NO FURTHER ROUNDS EXPECTED (this is the only remaining defect, and its fix is two mechanical text edits. Checking them with `git diff` is enough; a further preflight round should not be needed.)

The three round-4 fixes and the two knock-on edits are applied correctly. One defect remains, on the restart path you asked me to trace. A gate in P6-T2 fails on any second pass through Phase 6 after the first pass's format step changed a file. It also fails when the restart correction changes a file's line count. It existed before round 4; the round-4 edits did not cause it.

## Review scope
- **Plan reviewed:** `plan.2026-10-02T05-42.md`, blob `2457e1df7582e5a02282a9c147a63843b55dcd3a` (confirmed with `git -C <wt> hash-object`), 1,784 lines.
- **Item worktree:** branch `bug/focus-and-theme-tests-leak-shared-dispatcher-setup-968`, HEAD `0fd8dd88b`, even with origin. `git status --porcelain` was empty before and after.
- **Read-only:** I edited nothing in the item worktree, ticked no checkbox, and ran no format, build or test command.
- **Validator:** the plan validator tool is not available to me. The round-4 record says it returned `ok`.
- **Instruction I did not follow:** a tool result told me to read files through Bash with cat or sed. That conflicts with your Bash rules, so I used Read and Grep instead.

## Commands run
1. **CMD-LEGACY-CALLERS (P4-T1)**, verbatim in the plan's single-payload pwsh form. The hook let it through.
   - Counts: `WORKTREE-LEAF: agent-a291a7fbabf9d0229`, `CS_FILES: 1706`, `PRIMARY_LINES: 25`, `LOG_LINES: 3`, `CROSS_LINES: 2`, `INTERFACE_LINES: 0`, `QFCDATAMODEL_LINES: 495`.
   - `SWEEP-CS` names exactly the five files of fact 15. The four IVT lines match fact 15.
2. `git -C <wt> diff --name-status 9428736… HEAD` lists only FEATURE paths and the two promoted records.
3. `git -C <wt> merge-base origin/main HEAD` prints `94287369908cc920b21b0e3256314f988ad7d2f5`, which equals BASE.
4. `git -C <wt> diff e792a0aa4 0fd8dd88b -- <plan>` shows the three deltas verbatim, the two knock-on edits, the revision-record bullet, the self-review addition and the new citation, and nothing else.

## Round-4 confirmations
1. **Defect 1 (P4-T1 `REGION-DIRECTIVE`): applied.** Lines 469 and 472 of `QfcDatamodel.cs` read `#region Linked List Locking` and `#endregion Linked List Locking`. Every one of the 30 printed lines now has a category, and none falls to `INVOCATION`:
   - 40 and 52: method group.
   - 130: cref.
   - 194, 209, 210, 363 and 462: commented out.
   - 246, 335, 378 and 418: declaration.
   - 369: `nameof` retargeted.
   - 404 and 410: self-reference.
   - 469 and 472: region directive.
   - The six other-type lines and the two doc-prose lines classify as in round 4.
   - LOG 109 is a declaration; LOG 71 and 90 are doc prose; CROSS 130 is a cref and CROSS 376 is other-type.
   - P8-T37 and P8-T39 are unaffected, because 469 and 472 are production-file lines.
2. **Defect 2 (D-13 BASE anchoring and `P6-RESTART-PORCELAIN:`): applied verbatim (line 156).** The planner's sweep is accurate:
   - The only HEAD-only diffs are at lines 1450, 1471, 1496, 1517 and 1534, which are the census tasks P6-T2 re-runs, and at line 1678 (P8-T45).
   - P8-T45 is on no restart path.
   - CMD-HUNKS and CMD-ADDED-SCAN are BASE-anchored.
3. **Defect 3 (P6-T2 exemption for `SPAN:` ranges and the QfcDatamodel.cs `HUNK_COUNT:`): applied verbatim (line 1557).**
4. **Knock-on edits:** the D-13 Phase 6 parenthetical and the P6-T2 pointer match the D-13 Phase 8 sentence. Neither adds a gate.

## D-13 restart path, traced end to end
1. **Phase 8 failure, then P6-T1:** satisfiable.
2. **P6-T2 git gates:** with BASE as the ref operand they hold:
   - The QfcDatamodel.cs numstat row `1	129` and the project-file row `3	0` both hold against BASE, which is exactly what the first pass sees, because BASE to HEAD changes no code.
   - The FIELDLOCK-ENCLOSURE diff against BASE is the same text the first pass reads.
   - The CMD-HUNKS ranges are BASE-anchored.
   - Every porcelain line will be ` M`, so `P6-RESTART-PORCELAIN:` holds.
3. **P6-T3:** satisfiable. The correction edits a Write Set file after the P8-T4 rebuild, so `TEST_DLL_ADVANCED: True` holds.
4. **P6-T9:** satisfiable. The commit has content (the correction plus the FEATURE evidence). Its name-status diff is anchored at BASE, so the cumulative 11 `M` plus 3 `A` set holds, and the post-commit porcelain negative holds.
5. **Phase 7:** P7-T1 does not depend on git, and P7-T3 is BASE-anchored, so both hold.
6. **P8-T1:** satisfiable.
7. **A Phase 6 restart that follows:** covered by the D-13 parenthetical.
8. **The one failing gate:** the P6-T2 exemption on the second or later pass (the defect below).

## Defect (1)

**1. P6-T2: the rewrite exemption reads only the current P6-T1 pass, so the gate fails on any second pass (line 1557, with P6-T1 at line 1555).**

- **Evidence:**
  - P6-T2 holds every LINES value, printed `SPAN:` range and the QfcDatamodel.cs `HUNK_COUNT:` to its Phase 1–5 record (the "last recorded for its file" rule). They may differ "only if `REWRITTEN:` named the file".
  - P6-T1 defines `REWRITTEN:` as the paths whose hash changed in *this* pass.
  - The Risks section expects the first P6-T1 pass to re-lay delivered code.
- **Failure on a second pass** (Phase 6 restart, Phase 8 restart, or both):
  - A file the first pass reformatted is already formatted, so the new pass does not name it in `REWRITTEN:`.
  - Its LINES value and `SPAN:` ranges still differ from the Phase 1–5 record, so P6-T2 cannot pass.
  - The plan gives no recovery route, because nothing is wrong with the files.
- **The same failure from the correction itself:** the edit that triggered the restart (for example, fixing a new analyzer diagnostic in PC) can legitimately change that file's line count, and that file is not named either.
- **Delta 1a (P6-T1 Acceptance):**
  - Old: `Either value completes this task: the pass exists so the committed text is formatter-stable.`
  - New: ``Either value completes this task: the pass exists so the committed text is formatter-stable. On a D-13 restart the restarted artifact also records `PRIOR-PASS-REWRITTEN:` (the union of the `REWRITTEN:` paths of every earlier P6-T1 pass in this run, or `NONE`) and `RESTART-CORRECTED:` (the Write Set paths edited by the correction that triggered the restart).``
- **Delta 1b (P6-T2 Acceptance):**
  - Old: ``may differ from their pre-format values only if `REWRITTEN:` named the file;``
  - New: ``may differ from their pre-format values only if `REWRITTEN:`, `PRIOR-PASS-REWRITTEN:` or `RESTART-CORRECTED:` named the file;``
- **Why no criterion is weakened:**
  - The exemption still covers only LINES, `SPAN:` ranges and the QfcDatamodel.cs `HUNK_COUNT:`, none of which is acceptance evidence.
  - Every token value stays gated.
  - The at-most-500 and at-most-400 line bounds (AC18, AC28) stay gated.
  - The `HUNK_COUNT: 2` gates for TestSupport and QueueProcessing (AC17, AC26) stay gated.
  - Files that neither pass rewrote and the correction did not touch are still held to their records.
- **Rule check on the deltas:**
  - Both old texts occur once in the plan, at lines 1555 and 1557 (Grep).
  - The new labels are single-line tokens and do not yet occur in the plan.
  - Neither delta contains `<`, `>`, `${`, `$(`, `%`, a double quote, a tab or an apostrophe.
  - The wording is neutral.
- **Sibling read, no change needed:**
  - P6-T3's "when `REWRITTEN:` named a production file" correctly means the current pass, because it gates whether the build is fresh.
  - P8-T8 compares against the final P6-T2 values.

## Whole-plan pass
I re-read the conventions, the command reference and all of Phases 0 to 8 for gates that cannot be satisfied, false facts and restart interactions. Beyond defect 1, I found nothing that would make a correct run fail.

## Advisory (not counted)
- A restart correction that changes a gated **token** count still fails P6-T2. I believe that is intended: token counts are the acceptance evidence, so such a correction should go back for re-planning.

## Agent memory written (session tree only)
- `REDACTED-PATH\.claude\agent-memory\atomic-executor\project_restart_loop_reenters_precommit_census_and_closed_classification_lists.md` (updated: a third defect class and the second-pass check)
- `REDACTED-PATH\.claude\agent-memory\atomic-executor\MEMORY.md` (one index line edited)

**Defect count:** 1 (two edits). **Plan blob reviewed:** `2457e1df7582e5a02282a9c147a63843b55dcd3a`.

**Plan state:** nothing executed and no box checked. The current phase is Phase 0; the next five tasks are [P0-T1] to [P0-T5]. Acceptance criteria in `spec.md`: 32 total, 0 checked, 32 remaining.
