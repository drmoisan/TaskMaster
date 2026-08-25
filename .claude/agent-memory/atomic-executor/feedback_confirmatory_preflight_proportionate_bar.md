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

**Same bar on the FINAL round of an N-round bound**, where not clearing means the child is
reported `NOT CLEARED WITHIN BOUND`. The test to apply to a formal-contract nit is: *could an
executor reading this task verbatim be misled into a wrong action?* On #488 round 5, `[P6-T6]`
produced the seventh fail-before artifact and required `EXIT_CODE:` non-zero with
`ExpectedExitCode: 1`, yet its header carried no `[expect-fail]` tag while its own body said
"this task's `[expect-fail]` acceptance"; the six sibling fail-before tasks were tagged. Not a
blocker: the header had been untagged since original authoring, survived four rounds *including
one that rewrote that task's body and introduced the phrase*, the acceptance text is unambiguous,
and no downstream consumer keys off the header tag (the fail-before index reads the artifact's
`ExpectedExitCode` field). Two signals that a nit is settled rather than missed: `git log -p
--follow` shows it present since authoring, and the round that most recently edited that exact
task left it alone.

Cheap mechanical confirmations that make a confirmatory pass fast and defensible: diff the two
committed plan revisions with `git diff -U0` and read only the hunk headers (a fifth undisclosed
hunk is itself a finding); compare the `file:line` citation MULTISET across revisions to prove
"no citation was touched"; compare the sorted task-ID list to prove "no addition, removal or
renumbering". Still verify every factual assertion you make against the file — see
[[feedback_verify_line_citations_with_numbered_output]].
