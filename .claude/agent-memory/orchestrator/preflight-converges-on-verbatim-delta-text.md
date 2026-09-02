---
name: preflight-converges-on-verbatim-delta-text
description: Preflight round overrun is caused by the planner substituting its own wording for supplied delta items; supply verbatim replacement text and demand a per-item applied/not-applied disposition to converge
metadata:
  type: feedback
---

When relaying a preflight delta to `atomic-planner`, supply the reviewer's **verbatim replacement
text** and require an explicit per-item disposition (`applied-verbatim`,
`applied-with-mechanical-reassembly`, or `not-applied-with-reason`).

**Why.** On issue 662 the plan took three preflight rounds against a two-round target. The round-2
reviewer diagnosed the cause precisely: the planner had substituted its own wording for five of
fourteen round-1 delta items, and substituted text is unreviewed until the following round. So each
paraphrase silently converts a closed defect into a new unreviewed region. Round 3 supplied verbatim
text and demanded a disposition per item; it cleared with zero blocking defects.

**How to apply.**

- Write the delta to a committed artifact under `<FEATURE>/evidence/other/preflight-round-N-delta.md`
  and point the planner at the path, rather than pasting a long delta into the prompt. It survives a
  rate-limit termination, it is auditable, and it separates defects found *before* execution from
  defects found during it.
- Tell the planner explicitly: if it judges a supplied item wrong, do **not** silently rewrite —
  apply as given or leave unapplied and report the disagreement for the orchestrator to adjudicate.
  This is what surfaces reviewer error instead of burying it. On 662 the planner caught two genuinely
  unsatisfiable delta items this way (a verification search whose scope included the file that
  legitimately held the value being searched for, and a status gate whose artifact sat inside its own
  pathspec).
- Also tell it that where a command or path was **wrapped for the artifact's width**, reassemble it
  onto one line. A wrapped command is not a command.
- Expect a delta item to be internally inconsistent when it adds a rule the plan itself violates. On
  662 the new hygiene rule forbade an absolute host path in any committed file while the plan's own
  Working Directory section carried one — the rule could never pass over its own plan. Fix the plan
  rather than carving the plan out of the rule.

**Do not read a high round count as reviewer failure.** Rounds 1 and 2 each returned a complete
enumeration (9+6 and 4+6) rather than one defect at a time, which is the behaviour that holds the
count down. The overrun came from the revision step, not the review step.

Related: [[atomic-planner-lacks-mcp-validator-tool]], [[preflight-catches-vacuous-gates]],
[[multi-location-fact-residuals-drive-preflight-rounds]].
