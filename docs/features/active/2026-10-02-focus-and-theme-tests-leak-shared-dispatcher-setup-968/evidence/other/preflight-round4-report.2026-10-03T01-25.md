# Preflight round 4 reviewer report, verbatim (issue #968)

- Timestamp: 2026-10-03T01-25
- Source: atomic-executor return under `DIRECTIVE: PREFLIGHT VALIDATION ONLY`, delegated by the preparation-mode orchestrator (non-isolated; pwsh channel admitted).
- Plan reviewed: plan.2026-10-02T05-42.md, blob d7a5defaeceeaeefd69647580a2af882d4111cfb (commit d6a007976), 1,782 lines.
- Round: 4
- Text below is verbatim (HTML entity escapes in the transport were restored to `<` and `>`).

---

PREFLIGHT: REVISIONS REQUIRED
CONVERGENCE: FURTHER ROUNDS LIKELY (the three defects below all have mechanical deltas, but one confirming round is needed to check they were applied. I found no structural or design defect.)

The round-3 deltas are applied correctly, and every baseline value I re-ran matches the plan. Three defects remain: one I found by running the legacy-caller census, one on a restart path, and one that the round-3 wording of P6-T2 introduced.

## Review scope
- **Plan reviewed:** `plan.2026-10-02T05-42.md`, blob `d7a5defaeceeaeefd69647580a2af882d4111cfb` (confirmed with `git -C <wt> hash-object`), 1,782 lines.
- **Item worktree:** branch `bug/focus-and-theme-tests-leak-shared-dispatcher-setup-968`, even with origin. `git status --porcelain` was empty before and after the review.
- **Read-only:** I ran no write-mode command, edited nothing in the item worktree and ticked no checkbox.
- **Validator:** the plan validator MCP tool is not available to me, so I did not run it. The round-3 record says it returned `ok`.

## pwsh commands run (both read-only, both in the plan's single-payload form)
1. **CMD-LEGACY-CALLERS (P4-T1), verbatim:**
   - `WORKTREE-LEAF: agent-a291a7fbabf9d0229`, `CS_FILES: 1706`, `PRIMARY_LINES: 25`, `LOG_LINES: 3`, `CROSS_LINES: 2`, `INTERFACE_LINES: 0`, `QFCDATAMODEL_LINES: 495`.
   - The SWEEP-CS rows name exactly the five files of fact 15. The four IVT lines match fact 15.
   - Every count matches the revised P4-T1.
2. **T1-LIVE span on its own:** `SPAN: 110-165`, values 5, 0, 3, 3, 1, 0, 1. This matches P0-T13, and the hook let the payload through.

Other checks:
- With the Grep tool, spec.md has 334 lines and `acquired and released inside a held` appears on 4 lines. This matches fact 22 and P0-T2.
- `git diff fd85053a0 d6a007976` on the plan shows exactly the round-3 deltas, the A1 sentence, the revision record, the self-review addition and the new round-3 report citation.

## Round-3 confirmations
1. **Fact 15, P4-T1 `PRIMARY_LINES: 25` and the self-review "25, 3 and 2":** applied, and the census output above confirms them.
2. **`METHOD-GROUP-ONE-ARG-OVERLOAD` (lines 40 and 52):** applied. Classifying all 25 PRIMARY lines against the revised list:
   - 40, 52: method group.
   - 130: cref.
   - 194, 209, 210, 363, 462: commented out.
   - 246, 335, 378, 418: declaration.
   - 369: `nameof` retargeted.
   - 404, 410: self-reference.
   - `QfcHomeController.cs` 92, 132, 344, 379 and `QfcHomeControllerRunAsyncTests.cs` 325, 376: other type with the same name.
   - Liveness tests 104 and ZeroBatch 28: doc prose.
   - The LOG lines (109, 71, 90) and CROSS lines (130, 376) all classify.
   - **Lines 469 and 472 fit no category** (defect 1).
3. **P6-T2 "last recorded for its file":** I checked each file against the task order.
   - PC: P1-T3 is its only token record.
   - FIX: P2-T6. Nothing edits the fixture after Phase 2, and P3-T9's LINES value of 375 agrees.
   - FAT, TS and FT: P3-T9. Nothing edits them later.
   - TD, ZB, QDM and SBW: P4-T9. Phase 5 does not edit them; the P5-T8 edit is to QQP and is reverted.
   - The QDM numstat row `1	129` holds at P6-T2 because HEAD is still the P0-T19 commit. I re-derived 128 removed lines and one replaced line from P1. Line 369 is already a lone argument (I read lines 368 to 370), so formatting does not change the row.
   - LIV, DMT, AFTP and PROJ: P5-T5 is genuinely the last record. The P5-T3 and P5-T4 values match my arithmetic from L-T1 and M-T:
     - T1-LIVE: `await` 3 (lines 765, 790 and 813; the doc comment with "awaits" sits above the span start), `using (NoSynchronizationContext())` 2.
     - LIV: `FakeTimeProvider` 2, `NoSynchronizationContext` 3.
     - DMT: `FakeTimeProvider` 6, `fake.Advance` 2.
     - PROJ: plus 3.
   - QQP: P5-T12. Every token value and both hunk ranges match Q1 and Q2.
   - The project-file numstat `3	0` is correct for the three added `Compile Include` lines against HEAD at P6-T2.
   - The porcelain set of eleven ` M` and three `??` (fourteen paths) is correct.
   - On the first pass, every per-file pointer names that file's final value.
4. **Fact 22 and P0-T2 (334 lines; 4 lines on 10, 105, 266 and 282):** applied and verified.
5. **A1 sentence at line 1051:** applied. Its tone is neutral, and no task tells the executor to merge payloads. I also checked every payload whose text contains `gh` against the hook's `issue` plus `create`/`new` rule:
   - NAMES-LIVENESS in CMD-VSTEST and CMD-COVERAGE-POST, T-SIB and DMT each contain `gh` and `New`/`new`, but no `issue`.
   - T1-LIVE, FT, R4SPAN and R4TAIL each contain `issue`, but no `gh` substring.
   - So no single payload triggers the hook.
6. **The planner's decision to leave P6-T2's task line unchanged is sound.** That line lists the commands to re-run, and the acceptance line governs the values. The commands it lists do not include CMD-EOL, the only write-mode payload among the source tasks.

## Whole-plan pass
I re-checked the delivered-source arithmetic:
- N1 and the PC tokens.
- The F-blocks: 375 lines, FIX, ENSURE and SCOPE tokens.
- A1 to A4: 482 lines, FAT tokens.
- S1 and S2: 442 lines, TS tokens, the two hunk ranges.
- R-DOC and R-BODY: 472 lines; R4SPAN, R4HEAD and R4TAIL; the FT hunk ranges.
- W1 and W2 tokens.
- L1 to L7, TD1, Z1 to Z3, M1 to M3, P1 (128 lines removed, 21 QDM tokens), Q1 and Q2.
- The post-change CMD-CENSUS figures: 16, 32 and 28 lines, plus the per-file splits.

All of these are consistent. The defects are below.

## Defects (3)

**1. P4-T1: the two `Linked List Locking` region lines have no category (line 1501).**
- **Evidence:** CMD-LEGACY-CALLERS prints `PRIMARY \QuickFiler\Controllers\QfcDatamodel.cs:469 :: #region Linked List Locking` and `...:472 :: #endregion Linked List Locking`.
- A region directive is not a declaration, commented-out code, doc prose, a cref, a method group, a `nameof`, a self-reference or an other-type match. That leaves only `INVOCATION`, which forces `LEGACY MEMBER HAS A CALLER` on a correct run. This is the same class as round-3 defect 2.
- **Delta (P4-T1):**
  - Old: ``, `METHOD-GROUP-ONE-ARG-OVERLOAD` (`QfcDatamodel.cs` 40 and 52, the assignment that binds the surviving one-argument overload; fact 14), ``
  - New: ``, `METHOD-GROUP-ONE-ARG-OVERLOAD` (`QfcDatamodel.cs` 40 and 52, the assignment that binds the surviving one-argument overload; fact 14), `REGION-DIRECTIVE` (`QfcDatamodel.cs` 469 and 472, the `#region` and `#endregion` lines of the empty `Linked List Locking` region that P1 removes), ``

**2. D-13: a restart from Phase 8 back to P6-T1 makes P6-T2 impossible to pass (line 155, which governs line 1556).**
- **Evidence:** D-13 sends a Phase 8 failure (P8-T2 to P8-T5) back to P6-T1. By then the P6-T9 commit is in HEAD. On that path:
  - P6-T2 re-runs the HEAD-anchored diffs (P1-T3, P2-T6, P3-T9, P4-T9 and P5-T5: `git diff --numstat HEAD`, and `git diff HEAD` for the FIELDLOCK-ENCLOSURE reading). Against the new HEAD they print nothing for the committed files, so the gated rows `1	129` and `3	0` cannot appear.
  - The three new files are tracked by then, so the required "exactly the fourteen ... three `??`" porcelain set cannot appear either.
- **Delta (D-13).** Append one sentence after the existing sentence.
  - Old: `If P8-T2, P8-T3, P8-T4 or P8-T5 fails because of a Write Set file, the executor corrects it, restarts at P6-T1 (scoped format, census, build, pass-after runs, commit), re-runs Phase 7 in full, increments ITERATION and resumes at P8-T1.`
  - New: `If P8-T2, P8-T3, P8-T4 or P8-T5 fails because of a Write Set file, the executor corrects it, restarts at P6-T1 (scoped format, census, build, pass-after runs, commit), re-runs Phase 7 in full, increments ITERATION and resumes at P8-T1. On that restart the P6-T9 commit is already in HEAD, so P6-T2 runs each of its HEAD-anchored git commands with 94287369908cc920b21b0e3256314f988ad7d2f5 as the ref operand instead (the numstat rows it gates and the FIELDLOCK-ENCLOSURE diff are read from those), and its porcelain expectation becomes: every porcelain line under QuickFiler/ or QuickFiler.Test/ names one of the fourteen Write Set code paths with status ` M`, recorded as `P6-RESTART-PORCELAIN:`.`
- At restart time the BASE-anchored numstat still yields `1	129` for `QfcDatamodel.cs` and `3	0` for the project file, so no value is weakened.

**3. P6-T2 now requires values that earlier tasks recorded as ungated (line 1556; introduced by the round-3 wording).**
- **Evidence:**
  - "Every ... span, hunk ... value holds ... as last recorded" now covers the P4-T9 QfcDatamodel.cs `HUNK_COUNT:`, which P4-T9 states is "recorded, not gated".
  - It also covers the printed `SPAN:` ranges that P5-T5 records but never gates.
  - P6-T1 explicitly allows `REWRITTEN:` to be non-empty, and the Risks section expects CSharpier to re-lay the delivered code. When that happens, these observations shift and the gate fails, although only the LINES value is exempt today.
- **Delta (P6-T2 Acceptance):**
  - Old: ``(a LINES value may differ from its pre-format value only if `REWRITTEN:` named the file;``
  - New: ``(a LINES value, a printed `SPAN:` range and the recorded-not-gated QfcDatamodel.cs `HUNK_COUNT:` may differ from their pre-format values only if `REWRITTEN:` named the file;``
- No token value is relaxed, and files that were not rewritten are still held to their recorded ranges.

**Rule check on the deltas:**
- Each delta stays on one line.
- None contains `<`, `>`, `${`, `$(`, `%`, a double quote or a tab.
- No acceptance criterion is weakened.
- The wording is neutral.

## Advisory (not counted)
- P6-T2 re-runs the P1-T3 PROJ token list, which includes `Controllers\QfcItemController.UiThreadDispatcherFixtureTests.cs` = 1. The per-file pointer sends PROJ to P5-T5, whose list does not contain that token, so the value is never named. It holds at 1 regardless, so the outcome is unaffected.
- **Optional delta:** after `P5-T12 for QQP)` insert ``, with the P1-T3 PROJ token `Controllers\QfcItemController.UiThreadDispatcherFixtureTests.cs` still 1``.

## Agent memory written (session tree only)
- `REDACTED-PATH\.claude\agent-memory\atomic-executor\project_restart_loop_reenters_precommit_census_and_closed_classification_lists.md` (new)
- `REDACTED-PATH\.claude\agent-memory\atomic-executor\MEMORY.md` (one index line added)

**Defect count:** 3. **Plan blob reviewed:** `d7a5defaeceeaeefd69647580a2af882d4111cfb`.

**Plan state:** nothing executed and no box checked. The current phase is Phase 0, and the next five tasks are [P0-T1] to [P0-T5]. Acceptance criteria in `spec.md`: 32 total, 0 checked, 32 remaining.
