---
name: mark-observations-vs-conclusions-when-relaying-a-summary
description: When passing an upstream summary to a subagent, separate what was observed from what the summarizer concluded, or the conclusion gets used as evidence
metadata:
  type: feedback
---

When relaying a human's or another agent's summary into a delegation prompt, label which
statements are observations and which are the summarizer's inferences. Present the inferences as
claims to be checked, not as inputs.

**Why:** On issue #796 the operator's observation summary said candidate 3 was "not directly
observable and there is no room for it". I forwarded that phrase to the executor as part of my
stated reading. The executor pushed back correctly: the ordering evidence shows only that a third
close was not the *first cause*; it is silent on whether `TextBoxSearch_Leave` runs at all, which
is the separate question AC4 addresses, and the handler had no logging site so its absence from
the transcript is uninformative. Had the phrase been accepted, it would have supplied a false
warrant for treating AC4 as already satisfied and the acceptance criterion would have been
checked off on nothing.

**How to apply:** In a delegation prompt, write the raw observation and the conclusion separately,
and say explicitly that the conclusion is the orchestrator's or the reporter's reading and that a
disagreement is a wanted signal rather than a problem. On #796 doing this worked twice in one
run: the same executor also disagreed with my expectation about project-file citations and was
right. Phrase it so disagreement is cheap — "follow your derivation and say so plainly" — because
a subagent that reads your summary as settled will not re-derive it.

The failure mode is specific: summaries compress an observation and an inference into one
sentence, and the inference inherits the observation's authority. Related:
[[my-own-negative-claims-need-a-scoped-search]] and
[[reconcile-plan-numbers-against-your-own-measurements]].
