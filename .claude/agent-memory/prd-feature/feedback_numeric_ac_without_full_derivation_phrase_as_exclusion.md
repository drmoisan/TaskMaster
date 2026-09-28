---
name: numeric-ac-without-full-derivation-phrase-as-exclusion
description: When a caller wants a terminal-value AC ("exactly one file remains") but the research record's Numeric Derivation Evidence covers a different family, phrase the AC as an exclusion or absence instead of a count and say so in the spec
metadata:
  type: feedback
---

When a caller mandates an acceptance criterion with a numeric terminal value (e.g. "the profile-path
population reaches exactly one file") but the supplied research record carries complete
`## Numeric Derivation Evidence` for a different family only, do not drop the criterion and do not
write the bare number. Rewrite it as an exclusion ("lists no tracked file other than the single
named out-of-scope file") or an absence ("lists no file"), keep the measured figures in Repro &
Evidence as informational, and add one sentence in the spec stating why the AC is phrased that way.

**Why:** The prd-feature system prompt requires omitting a numeric assertion when the derivation
record is missing or narrow, while the caller (issue 602, 2026-09-12) required the terminal-value
criterion. An exclusion-shaped AC satisfies both: it is falsifiable, it names the sole surviving
member, and it makes no count claim the record cannot back. The only fully derived family in that
research record was the three-file host-stem difference set, which could be cited as-is.

**How to apply:** Any sweep, migration or population-reduction spec where the caller supplies
orchestrator-measured counts (F-findings) but the research artifact's derivation block covers one
sub-population. Cite the derived family directly; convert every other count-shaped AC to
exclusion/absence; keep "non-zero baseline" wording rather than a specific baseline number.
Related: [[ac-gates-verify-satisfiability]], [[backticked-paths-are-the-change-footprint]].
