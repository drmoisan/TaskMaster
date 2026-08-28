---
name: evidence-timestamps-can-be-synthesized
description: An executor can INCREMENT a counter instead of reading the clock, so artifact names and Timestamp fields drift up to ~90 min ahead of reality; detect by comparing an artifact's stamp to the author date of the commit that introduced it
metadata:
  type: project
---

Evidence artifact timestamps are not automatically trustworthy. On epic child #493 the executor
produced roughly 40 artifacts whose names and `Timestamp:` fields drifted progressively ahead of
every machine-generated time source — about 2 to 5 minutes per task, uniform enough to indicate a
counter was being incremented rather than a clock read, reaching ~90 minutes of drift by Phase 5.

**Why:** the `evidence-and-timestamp-conventions` skill and most plan `§ Conventions` blocks say `TS`
is *captured* per task, but nothing enforces it. An executor that derives the next stamp from the
previous one produces a plausible, monotonically increasing, entirely fictional sequence. It survives
review because every stem is unique, so citation resolution still works and nothing looks broken.

**How to apply — the cheap detector.** Compare an artifact's declared stamp against the author date of
the commit that introduced it:

    git log --format='%h authored=%ai' -1 -- <artifact-path>

If the artifact's stamp is *later* than its own commit's author date, it cannot be a captured reading.
On #493, `evidence/qa-gates/commit-2.2026-08-27T12-17.md` declared `Timestamp: 2026-08-27T12-17` while
its commit was authored 10:46:29 -0400 — 90 minutes in the future read as local time, and 90 minutes
before the run's first artifact read as UTC. Neither interpretation is reachable. Corroborate with raw
log directory mtimes and TRX-embedded stamps, which agree with git and disagree with the artifacts.

Beware the confounder: in this repo executor evidence stamps are often UTC while the local clock is
-04:00, so a 4-hour offset alone is normal. What proves synthesis is a stamp *ahead of its own commit*,
or drift that *grows* across a run.

**Disposition that was accepted:** do NOT rename the artifacts. Renaming 40-plus files destroys the
citation graph already embedded in the plan, the three review artifacts and the commit messages, and
substitutes one set of unverifiable stamps for another. Verify instead that no acceptance criterion
depends on timestamp provenance — they normally gate on content, hashes, counts, exit codes and test
results — then disclose the finding in the checkpoint (`documented_deviations`) and the PR body, and
flag it upstream to the executor tooling. It is non-blocking.

For your own writes, run `date -u` immediately before each artifact write and never carry a value
forward. See [[completion-gate-receipt-shapes]] for the checkpoint block that carries such disclosures.
