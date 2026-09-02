---
name: absence-gates-do-not-prove-validity
description: A hygiene gate that asserts only that the bad token is GONE never checks that the rewritten artifact is still valid, so a redaction rule can corrupt every file it touches while the gate reports success
metadata:
  type: feedback
---

When a plan carries a redaction, sanitization, or hygiene rule over generated artifacts, check that its
gate asserts the artifact is still **well-formed** after the rewrite — not merely that the forbidden
token is absent.

**Why:** On item 662 all six committed test-result files were invalid XML, and the corruption was
*mandated by the plan*. The hygiene rule prescribed angle-bracket placeholders as the redaction text;
`vstest` writes that text into XML **attribute values**, where a bare `<` is illegal. So an executor
following the rule *correctly* produced six unparseable documents, on every run. The gate asserted only
`ResidualMatchCount=0` — that the identifiers were removed — and therefore reported success over
corrupt output. The defect is invisible to the executor, to the gate, and to CI, because nothing
downstream parses the artifact.

**How to apply:**

- Read every hygiene or redaction rule for a **format collision**: the replacement text must be legal in
  the syntactic position the producer writes it into. Angle brackets in XML attributes, quotes in JSON
  strings, and colons in YAML keys are the recurring cases.
- Treat "the forbidden token is gone" as a **necessary and insufficient** gate. Pair it with a parse or
  schema check of the rewritten artifact. An absence assertion and a validity assertion fail on disjoint
  inputs, so neither substitutes for the other.
- The repair is to escape in place, keeping the redaction byte-identical, rather than to change the
  placeholder — changing it silently alters what earlier runs recorded.
- There is a payoff beyond correctness: once the artifacts parse, acceptance criteria can be verified
  from the structured counters rather than from prose. On 662 that is what let the test-count delta be
  confirmed as exactly the one added test.
- This is the same family as
  [[reconcile-derived-radius-against-branch-diff]]'s pinned-base trap and the
  `atomic-plan-contract` rule "observe a command's success-case output before asserting over that
  output". All three are gates that **cannot fail**, and none is caught by the shipped G1 through G9
  validator rules. Expect to find them by reading the gate against the state it will actually run in.
