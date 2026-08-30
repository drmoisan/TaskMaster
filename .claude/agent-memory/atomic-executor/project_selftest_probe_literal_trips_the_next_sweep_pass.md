---
name: selftest-probe-literal-trips-the-next-sweep-pass
description: Quoting a synthetic probe literal in a sweep's own evidence artifact makes the NEXT sweep pass return nonzero; describe probes instead of quoting them.
metadata:
  type: project
---

A host-identity (or any forbidden-literal) sweep task whose evidence artifact quotes its
own positive-control probe verbatim will be flagged by the next pass of the same sweep.
The artifact is written after the sweep runs, so the sweep never sees itself; the
follow-up pass that exists precisely to cover it does.

Observed on issue #644 cycle 2: `[P3-T2]`'s artifact quoted the forward-slash probe
`C:/Users/someone/x` in its self-test table. `[P3-T2]` passed with `0`. `[P3-T3]`'s
pre-staging re-sweep — documented in the plan as "the pass that covers `[P3-T2]`'s own
artifact" — returned `1`, forcing the authorized redaction branch plus a full `[P3-T1]`
re-run before the commit.

**Why:** plans deliberately place a second sweep after the first precisely because the
first cannot observe its own output. Any literal the artifact quotes is therefore live
input to the second sweep, and a synthetic placeholder is indistinguishable from a real
leak to a mechanical pattern match.

**How to apply:** when evidencing that a detector is not vacuous, *describe* the probes
("a synthetic drive-letter path using forward-slash separators") and report only the
boolean result. Never paste the probe string. The same applies to any gate that greps
for a banned token: the evidence artifact is inside the search scope. Related:
[[doubled-backslash-dedoubles-bash-to-native-exe]] — both defects surfaced in the same
sweep and both make a `0` result untrustworthy for opposite reasons (one under-matches,
one over-matches).
