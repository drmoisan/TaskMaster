---
name: epic-manifest-all-clear-table-is-not-evidence
description: An epic manifest's per-child "PREFLIGHT: ALL CLEAR" table, and a resume brief repeating it, can both be false — grep the child's own folder before executing, because the plan may carry a wrong central derivation
metadata:
  type: feedback
---

Before executing a prepared epic child, grep the child's OWN feature folder for `preflight`,
`all clear` and `approved` (case-insensitive). Do not accept the epic manifest's Preparation
Outcome table or the dispatching resume brief as evidence that preflight ran.

**Why:** On epic `review-residuals-2026-09-08`, `epic.md` carried a Preparation Outcome table
marking all eight children `ALL CLEAR`, and the resume brief for child #826 repeated it as "an
APPROVED atomic plan carrying a recorded PREFLIGHT: ALL CLEAR verified against the child checkpoint
on disk". Both were false for #826. The whole-folder grep returned exactly one hit — the plan's own
`- **Status:** Ready for preflight (revision rounds 1 through 5 applied)`, which asserts the
opposite — plus no `PLANNER-INTERNAL-REVIEW`, no `SELF-REVIEW` and no `CONVERGENCE` line anywhere,
and none of the folder's four commits recording a preflight round.

This is not a bookkeeping nit. The single preflight round I ran found **12 defects, 7 blocking**,
including that the plan's central decision D2 asserted the exact opposite of what the code does.
D2 claimed both edited `catch` bodies were unreachable through the injected-factory seam and built
an entire alternative branch on it: a substitute test, a fail-before exception dossier, and an
authorised route to checking AC7 and AC15 off on a caveat note. Executing that plan would have
shipped a deliberately-wrong test and two falsely-checked acceptance criteria, and every gate in
the plan would have passed while it happened.

Five authoring/revision rounds do not substitute for one executor round. The planner had revised
the plan five times and still held the inverted derivation, because re-deriving your own citation
does not re-derive the *premise* the citation was selected to support.

**How to apply:**
- The grep costs one tool call. Run it on every prepared child before the first execution
  delegation, and record the result in the checkpoint whichever way it comes out.
- If clearance is absent, run one `DIRECTIVE: PREFLIGHT VALIDATION ONLY` round. This is *not* the
  forbidden "re-run planning": promotion, research and plan authoring stay untouched.
- `mcp__drm-copilot__validate_orchestration_artifacts` returning `ok:true` on the plan does not
  help here. It returned `ok:true` on this plan. Structural validity and gate-rules G1-G9 say
  nothing about whether a derivation is true.
- When the preflight overturns a plan decision, verify it yourself before relaying — see
  [[subagent-self-reported-correction-can-be-false]]. I read `TimeOutTask.cs` and the sibling's
  test directly, and re-derived every citation in the delta, before handing it to the planner.
- A sibling's completion notes can look like they contradict the finding and not actually
  contradict it. 825's checkpoint recorded "nothing escapes `RunWithTimeout` on the ordinary
  timeout path", which is about an exception raised *inside* the `try`; this finding is about the
  injected factory invoked *outside* it. Check the two against each other rather than assuming
  either compatibility or conflict.

Related: [[preflight-catches-vacuous-gates]], [[prepared-epic-child-invalidated-by-sibling-merge]],
[[resume-brief-worktree-contents-premise-can-be-false]],
[[my-relayed-delta-must-pass-the-same-satisfiability-check]].
