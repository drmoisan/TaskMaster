---
name: branch-decision-artifact-names-the-literal-it-declares-absent
description: A plan gate keyed on "artifact X records literal L" inverts when X declares L absent by quoting it; resolve on the recorded branch, never on a grep hit count
metadata:
  type: project
---

When a plan makes a later gate conditional on an earlier decision artifact *recording* a
literal — e.g. "the fifth expect-fail row is present only when
`evidence/qa-gates/p4-t7-park-focus-decision.md` records `PARK-FOCUS-SUPPRESSION: IN
SCOPE FOR P4-T8`" — do NOT resolve it with a literal search. A well-written decision
artifact that takes the NO branch typically says, in its consequences list:

> The line `PARK-FOCUS-SUPPRESSION: IN SCOPE FOR P4-T8` is deliberately ABSENT from this artifact

so the token occurs exactly once, and the one occurrence is the artifact declaring that
it does not carry the line. A grep hit count of 1 reads as "condition satisfied" and
selects the opposite branch from the one the artifact recorded.

**Why:** observed on issue #796 at P9-T5 (2026-09-07). The plan's expect-fail inventory
is four rows on the NO branch and five on the YES branch, and the fifth row names a test
that was never written. A hit-count reading would have demanded a Passed result for a
nonexistent test and failed a gate that in fact passed.

**How to apply:** resolve the branch on the artifact's own `## Branch taken` /
`NO branch` statement and its quoted basis, and corroborate the consequence
independently — for #796 that was enumerating every `testName` in the TRX containing
`Park` and confirming no paired negative test exists. Then record the caution in the
gate artifact, because the next reader will run the same grep. Note the substring trap in
that corroboration too: `ParkFocus` matches nothing because the existing test spells the
verb `Parks`, so a zero-match from the narrow substring proves nothing; widen to `Park`.

Same failure family as [[selftest-probe-literal-trips-the-next-sweep-pass]] and
[[banned-api-zero-hit-gate-hits-doc-comments]] — a literal named in prose is
indistinguishable from a literal in force.
