---
name: confirmatory-preflight-proportionate-bar
description: On a confirmatory preflight over a small delta to an already-cleared plan, block only on an actually-wrong edit or an actual regression; incomplete enumerations are non-blocking observations
metadata:
  type: feedback
---

When a directive says "CONFIRMATORY PASS" over N named edits to a plan that already earned
`PREFLIGHT: ALL CLEAR`, say `REVISIONS REQUIRED` only if one of those edits is actually wrong
or actually regressed something the prior pass verified. Wording preferences, belt-and-braces
suggestions, and *incomplete enumerations* go under an ALL CLEAR as non-blocking observations.

**Why:** the prior ALL CLEAR is authority, not a starting point to re-litigate. Re-auditing the
whole plan burns a cycle and risks the "make the citation exist" failure mode recorded in
[[project_preflight_citation_match_propagates_false_fact]] — a delta that edits correct text to
match a re-derived opinion. The user states the bar explicitly: "Hold a proportionate bar and
converge."

**How to apply:** distinguish a FALSE claim from an INCOMPLETE one. On #498 v2.3, `P4-T1` said
its recorded value "is the sole input to the branch conditions in `P5-T4` and `P5-T5`" after
`P5-T3` also gained a gate on it. That sentence is still true of the two tasks it names; it is
merely no longer exhaustive, and the task's own governing text plus the binding "Conditional
branches" rule both state the condition. Nothing is unsatisfiable and no executor is stranded,
so it is an observation, not a blocker. Contrast with the genuinely blocking shapes in
[[project_exact_count_gate_vs_remediation_loop]] and
[[project_preflight_absolute_zero_gate_on_sibling_owned_assembly]], where a gate is
*unsatisfiable* or *unwaivable*.

**Polarity test for a clause an earlier task invalidates.** When phase N asserts a fact about a
file that phases 1..N-1 change, ask which way the clause breaks. A clause that becomes FALSE at its
own point strands the executor — it counts literally, records a mismatch, and leaves the task
unchecked under the fail-closed rule (#464 `[P5-T11]`: "the two timer arming sites still bind" after
`[P1-T6]` deleted the method holding one of them). A clause that becomes TRIVIALLY TRUE only loses
gating power (#464 `[P6-T13]`: a zero-match search for `throw (e.InitializationException)` that
`[P6-T10]` had already driven to zero). The first is blocking; the second is an observation, provided
the task carries at least one other clause that can still fail. Same defect family, opposite cost.

**Convergence is a real terminal state; report it plainly.** A long defect streak is not evidence
that the next round holds a defect. On #677 the caller framed round 9 as "8 consecutive rounds each
found something real, so this determination carries weight — but a plan does converge, and
manufacturing a marginal finding past that point is not useful." That framing is the correct bar:
run the full pass, then say `ALL CLEAR` without inventing a hypothetical. The convergent round is
also where a NEW finding is most likely to be a re-derived opinion rather than a defect, because the
cheap real defects are already gone — so hold any late finding to "unsatisfiable, unreachable, or
provably false against the file", not "I would have written it differently".

Cheap mechanical confirmations that make a confirmatory pass fast and defensible: diff the two
committed plan revisions with `git diff -U0` and read only the hunk headers (a fifth undisclosed
hunk is itself a finding); compare the `file:line` citation MULTISET across revisions to prove
"no citation was touched"; compare the sorted task-ID list to prove "no addition, removal or
renumbering". Still verify every factual assertion you make against the file — see
[[feedback_verify_line_citations_with_numbered_output]].
