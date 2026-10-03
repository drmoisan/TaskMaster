# Preflight round 3 reviewer report, verbatim (issue #968)

- Timestamp: 2026-10-03T01-01
- Source: atomic-executor return under `DIRECTIVE: PREFLIGHT VALIDATION ONLY`, delegated by the preparation-mode orchestrator (non-isolated; pwsh channel admitted after the session-tree checkpoint was seeded).
- Plan reviewed: plan.2026-10-02T05-42.md, blob ff2d67a45b10b2c952ee7aeb261c324b91ee659c (commit fd85053a0), 1,780 lines.
- Round: 3
- Text below is verbatim (HTML entity escapes in the transport were restored to `<` and `>`).

---

PREFLIGHT: REVISIONS REQUIRED
CONVERGENCE: FURTHER ROUNDS LIKELY (all four defects have mechanical deltas, but the plan needs one more round to confirm they were applied. I found no structural or design defect.)

## Review scope

- **Plan reviewed:** `plan.2026-10-02T05-42.md`, blob `ff2d67a45b10b2c952ee7aeb261c324b91ee659c`. I confirmed this with `git -C <wt> hash-object`. The plan has 1,780 lines.
- **Item worktree:** HEAD is `fd85053a0`, branch `bug/focus-and-theme-tests-leak-shared-dispatcher-setup-968`. `git status --porcelain` was empty before and after the review.
- **Read-only:** I ran no write-mode command, edited nothing in the item worktree, and ticked no checkbox.
- **Validator:** the plan validator MCP tool is not available to me, so I did not run it.

## pwsh commands run (all read-only, each printed `WORKTREE-LEAF: agent-a291a7fbabf9d0229`)

1. **P0-T13 tokens (my own combined payload for LIV, TD, ZB and DMT):** every value matches the plan.
   - LIV: 1, 3, 0, 2, 0, 4, 3, 3, 1, 0, 0, 0, 0, 4.
   - TD: 1, 2, 0, 1, 1, 0, 5.
   - ZB: 1, 4, 0, 3, 0, 1, 1, 1, 1, 1, 0, 3.
   - DMT: 2, 0, 0, 0, 0, 1, 0, 1, 4, 5, 0, 9.
2. **A second combined payload was refused by a hook.** It held the QDM, QQP and PROJ token lists plus the T1-LIVE, HELD, T-SIB and GATE-LAMBDA spans. Deny text, verbatim:
   `PreToolUse:Bash hook error: PROMOTION_MCP_ONLY_BLOCKED: Direct GitHub issue creation via `gh` bypasses the approved drm-copilot MCP promotion path (`mcp__drm-copilot__new_potential_entry` -> `mcp__drm-copilot__potential_to_issue` -> `mcp__drm-copilot__new_active_feature_folder`). Use those MCP tools instead.`
   - I did not retry that payload or the T1-LIVE command it contained. I verified T1-LIVE by reading the file instead (see confirmation 3).
   - I did run the other plan commands it had combined, each as its own payload in the plan's form (items 3 to 6). Strictly, those are re-runs of parts of the refused command; I am stating this openly.
   - The cause is in Advisory A1 below.
3. **QDM (plan's single-file form):** 2, 4, 0, 1, 4, 1, 1, 1, 2, 2, 7, 7, 1, 2, 1, 1, **2**, 1, 2, 2, 3. This matches the plan, including the corrected `ForEachAwaitWithCancellationAsync` value of 2.
4. **QQP:** 1, 1, 2, 1, 3, 0, 0, 1, 1, 0, 1. Matches.
5. **PROJ:** 187 for the `<Compile Include=` token (recorded only), then 0, 0, 0, 1, 1. Matches.
6. **Spans:**
   - HELD printed `SPAN: 183-217` with 1, 0, 0.
   - T-SIB printed `SPAN: 96-133` with 1, 0, 0, 0, 1.
   - GATE-LAMBDA printed `SPAN: 299-310` with 1, 0.
   - All match.
7. **CMD-LINECOUNT on CS4 and FOLD6:** 342, 497, 440, 470, 312, 244, 232, 371, 495, 413. Matches.
8. **P0-T12 tokens:**
   - FIX: 0, 0, 4, 3, 2, 1, 1, 0, 0, 0.
   - FAT: 2, 1, 0, 9, 0, 0, 0, 17.
   - TS: 1, 1, 1, 1, 0, 0, 1, 1.
   - FT: 1, 0, 1, 8, 1, 4, 1, 1, 8.
   - All match. The FT payload contains `issue #230` and was admitted.
9. **P0-T12 spans:**
   - R4SPAN printed `212-284` with 1, 2, 1, 2, 1, 1, 1.
   - R4HEAD printed `212-224` with 1, 1, 1.
   - R4TAIL printed `267-273` with 3, 1, 0.
   - ENSURE printed `122-145` with 0, 0, 1, 2.
   - SCOPE printed `249-283` with 1, 0, 0, 0, 0.
   - All match.
10. **CMD-CENSUS:**
    - `CS_FILES: 1706`.
    - `PRIMARY_LINES: 9`, split fixture 1, test support 2, fixture tests 4, focus-and-theme 2.
    - `CROSS_LINES: 20`, split 5, 2, 1, 10, 2.
    - `CONTROL_LINES: 23`.
    - All match.
11. **P0-T2 spec tokens:** 32, 0, 1, 1, 1, **4**, 1, 2. The plan gate is "at least 1", so it passes, but fact 22 says 3 (defect 4).
12. **CMD-LEGACY-CALLERS (P4-T1):** **`PRIMARY_LINES: 25`**, against the plan's 24 (defect 1).
    - `LOG_LINES: 3`, `CROSS_LINES: 2` and `INTERFACE_LINES: 0` match.
    - The SWEEP-CS rows name exactly the five `.cs` files of fact 15.
    - The four IVT grants match.
    - `QFCDATAMODEL_LINES: 495` matches.
13. **Spec and issue line counts:** the spec is **334** lines, against fact 22's 335. issue.md line 12 reads `- Work Mode: full-bug`, and line 65 is the Coordinator Scope Amendment heading.

**Read-only git commands (Bash):**
- `merge-base origin/main HEAD` returned `94287369…`.
- `rev-parse --abbrev-ref HEAD` returned the expected branch.
- The scoped `diff --exit-code BASE HEAD -- QuickFiler QuickFiler.Test …` exited 0.
- `diff --name-status BASE HEAD` lists only FEATURE paths and the two promoted records.

**Cross-checks with the Grep tool:**
- `SynchronousBackgroundWorker`: 13 lines in 3 files.
- `_remainingLoadActive|_remainingLoadTask`: 20 lines in 7 files.
- `ArmingFakeTimeProvider|NoSynchronizationContext`: 0.
- The ripgrep legacy-member pattern also gives **25 lines in 5 files**.

## Specific confirmations

1. **All eight round-2 deltas are applied correctly.**
   - F-SCOPE (plan lines 228 to 270) is 43 lines. The text says "forty-three", and the 375 total appears at line 272, P2-T6 and P3-T9.
   - The four fold spans and the T1-LIVE `(await pending)` value of 1 are in place.
   - Fact 14 and P0-T13 both carry the QDM `ForEachAwaitWithCancellationAsync` value of 2.
   - The `FakeTimeProvider` substring counts at line 888, P5-T3, line 1015 and P5-T4 are correct.
   - P4-T9 records the hunk count without gating it, and gates the numstat row `1	129`. That row is tab-separated, which I confirmed with Grep.
   - Both rewritten tests' doc comments now say "a scheduler yield".
   - The D-10 refusal list now includes evidence-file Writes and pwsh payloads, including `PREIMPLEMENTATION_GATE_BLOCKED`.
2. **The planner's deviation is correct.**
   - DMT `FakeTimeProvider` reads 5 by the plan's own token command, on lines 99, 216, 224, 249 and 258. The `using` directive does not contain the token.
   - M2 replaces lines 95 to 131, which removes line 99. M-T adds lines 960 (cref) and 968 (`new ArmingFakeTimeProvider()`), so the post-change value is 4 + 2 = **6**, not 7.
   - LIV reads 1 (line 114 only). After L3 it is 2 (L-T1 lines 743 and 754).
3. **Every P0-T13 and P0-T12 baseline matches the actual output.**
   - T1-LIVE was checked by reading, because its combined run was refused. The span is 110 to 165.
     - `await` appears on 5 lines (124, 140, 142, 157, 163).
     - `Task.Yield` 3, `fake.Advance` 3 and `for (int i` 1 also match.
     - The plan's T1-LIVE command on its own should not trip the hook: none of its text contains `gh` or `new`. I worked this out from the hook source; I did not run it.
   - The Phase 0 values that fail are the P4-T1 count (defect 1) and fact 22, which nothing gates (defect 4).

## Defects (4)

**1. Legacy-caller census: the primary strategy returns 25 lines, not 24.**
- **Where:** fact 15 (plan line 131), P4-T1 (line 1500), and the self-review (line 1688).
- **Evidence:** the plan's own CMD-LEGACY-CALLERS prints `PRIMARY_LINES: 25`, and ripgrep with the same pattern gives 25 lines in 5 files.
  - `QfcDatamodel.cs` has 17 hits: lines 40, 52, 130, 194, 209, 210, 246, 335, 363, 369, 378, 404, 410, 418, 462, 469 and 472.
  - The plan lists 8 hits outside that file.
- **Impact:** P4-T1 stops with `LEGACY MEMBER HAS A CALLER` on a correct execution.
- **Deltas:**
  - Fact 15. Old: ``over `*.cs`: 24 lines in 5 files; outside `QfcDatamodel.cs` the hits are``. New: ``over `*.cs`: 25 lines in 5 files (17 in `QfcDatamodel.cs`: 40, 52, 130, 194, 209, 210, 246, 335, 363, 369, 378, 404, 410, 418, 462, 469, 472); outside `QfcDatamodel.cs` the hits are``.
  - P4-T1. Old: ``; `PRIMARY_LINES: 24`; `` New: ``; `PRIMARY_LINES: 25`; ``.
  - Line 1688. Old: `24, 3 and 2 lines for the legacy-member proof`. New: `25, 3 and 2 lines for the legacy-member proof`.

**2. P4-T1 has no classification for the two method-group assignments at `QfcDatamodel.cs` lines 40 and 52.**
- **Evidence:** both lines are `RemainingEmailLoader = LoadRemainingEmailsToQueueAsync;` and appear in the PRIMARY output.
  - They bind the surviving one-argument overload (fact 14).
  - They are neither a declaration nor a cref, so the remaining category is `INVOCATION`, which forces the stop.
- **Delta (P4-T1).** Old: ``, `CREF-ONE-ARG-OVERLOAD` (`QfcDatamodel.cs` 130), `NAMEOF-RETARGETED` ``. New: ``, `CREF-ONE-ARG-OVERLOAD` (`QfcDatamodel.cs` 130), `METHOD-GROUP-ONE-ARG-OVERLOAD` (`QfcDatamodel.cs` 40 and 52, the assignment that binds the surviving one-argument overload; fact 14), `NAMEOF-RETARGETED` ``.

**3. P6-T2 requires values that later tasks deliberately supersede, so it cannot pass.**
- **Where:** line 1555, "every P2-T6, P3-T9, P1-T3, P4-T9, P5-T5 and P5-T12 token, span, hunk and numstat value holds after formatting".
- **Interim values that no longer hold at P6-T2:**
  - P4-T6 LIV values: `using (var worker = new SynchronousBackgroundWorker())` 3 (becomes 4), `Task.Yield` 3 (becomes 0), `fake.Advance` 3 (becomes 0), `FakeTimeProvider` 1 (becomes 2), `new ArmingFakeTimeProvider()` 0 (becomes 1).
  - P4-T7 DMT `using (var worker = new BackgroundWorker())` 1 (becomes 2).
  - P4-T9 PROJ "plus 2" and `TestSupport\ArmingFakeTimeProvider.cs` 0 (become plus 3 and 1).
  - The P1-T3 project-file numstat `1	0` becomes `3	0`.
- **Delta (P6-T2 Acceptance).** The two numstat values below are tab-separated, as at P1-T3.
  - Old: `every P2-T6, P3-T9, P1-T3, P4-T9, P5-T5 and P5-T12 token, span, hunk and numstat value holds after formatting (`
  - New: ``every token, span, hunk and numstat value holds after formatting as last recorded for its file (P1-T3 for PC; P2-T6 for FIX; P3-T9 for FAT, TS and FT; P4-T9 for TD, ZB, QDM, SBW and the QfcDatamodel.cs numstat row `1	129`; P5-T5 for LIV, DMT, AFTP and PROJ, superseding the interim P4-T6, P4-T7 and P4-T9 values for those files; P5-T12 for QQP), the project-file numstat row reads `3	0` in place of the P1-T3 value, and the porcelain span below replaces every earlier porcelain expectation (``

**4. Fact 22 misstates two spec values.**
- **Evidence:**
  - The spec is 334 lines (pwsh count and Grep `^` both).
  - `acquired and released inside a held` appears on 4 lines (10, 105, 266, 282).
- **Impact:** none on execution, because the P0-T2 gate is "at least 1". It is still a false citation.
- **Deltas:**
  - Fact 22. Old: ``spec.md`, 335 lines)`` New: ``spec.md`, 334 lines)``
  - Fact 22. Old: `` `acquired and released inside a held` 3 (105, 266, 282) `` New: `` `acquired and released inside a held` 4 (10, 105, 266, 282) ``
  - P0-T2. Old: `(fact 22: 1, 1, 3, 1, 2;` New: `(fact 22: 1, 1, 4, 1, 2;`

**Rule check on the deltas:**
- Every changed token stays on one line.
- No delta contains a placeholder character (`<`, `>`, `${`, `$(` or `%`) or a double quote.
- No acceptance criterion is weakened. Defect 3 keeps every final value, and the project-file numstat restatement is stricter than before.
- The wording is neutral.

## Advisory A1 (not counted as a defect)

- **Mechanism (from the hook source):**
  - `pwsh` is listed as a wrapper in `hook-command-scanner.ps1`, so the hook treats the whole payload as raw text.
  - It then refuses the call if the text contains `gh` + `issue` + (`create` or `new`) anywhere, case-insensitively, as plain substrings.
  - `gh` is satisfied by words such as `High` or `through`.
- **What happened:** the refused payload combined the T1-LIVE anchor (`Reads the issue #424`) with the T-SIB anchor (`HighConfidenceMode`) and a token containing `new BackgroundWorker()`. Run separately, as the plan requires, each payload passes.
- **Why it matters:** P5-T1 records both span payloads in one artifact. Under the revised D-10 rule a pwsh refusal stops the run.
- **Optional delta (Payload channel, line 1050).** Append after `…runs as one Bash tool call of the form `pwsh -NoProfile -Command '<payload>'`, newlines included, with the substitutions applied.`: `Payloads are never merged into one call: the hook layer scans a pwsh payload as raw text, so a merged payload can combine words that each payload alone does not.`

## Agent memory

- I updated `C:\Users\DanMoisan\repos\TaskMaster-wt\2026-10-02T21-24\.claude\agent-memory\atomic-executor\project_phrase_count_payload_trips_promotion_gh_issue_hook.md` with the hook mechanism found in this round.
- I added a pointer line for it in `C:\Users\DanMoisan\repos\TaskMaster-wt\2026-10-02T21-24\.claude\agent-memory\atomic-executor\MEMORY.md`.
- Both files are in the session tree. I wrote nothing in the item worktree.

**Defect count:** 4.

**Plan state:** nothing executed, no box checked. The next five tasks are [P0-T1] to [P0-T5]. Acceptance criteria in `spec.md`: 32 total, 0 checked.
