---
name: evidence-timestamp-labels-drift-ahead-of-write-time
description: Estimating an evidence artifact's <TS> instead of reading the clock produces labels minutes ahead of the real write time, which later breaks an ordering gate and forces a bulk rename plus cross-reference repair
metadata:
  type: project
---

Do not guess the `<TS>` in an evidence filename. Call `date +%Y-%m-%dT%H-%M`
immediately before each `Write`, or label from the file's own mtime afterwards.

**Why:** Across a single #440 plan execution the labels drifted 3 to 10 minutes ahead
of the actual write times (a step labelled `06-48` was written at `06:38:21`). The
drift compounds because each estimate is anchored on the previous estimate rather
than on the clock. Two concrete costs followed:

- A `p4-t7-consecutive-pass` task required the five Phase 4 artifact timestamps "in
  ascending order" as evidence that the toolchain loop ran uninterrupted. Fabricated
  ascending labels would have satisfied that gate while proving nothing, so the gate
  was only meaningful once the labels were re-derived from real mtimes.
- Repairing it meant renaming 18 files, rewriting each file's internal `Timestamp:`
  line, and then sweeping every artifact for stale cross-references — one artifact
  cited a sibling by its old filename and would otherwise have shipped a dead link.

**How to apply:** Get a timestamp per artifact, not one per phase. When a long
background run sits between two artifacts, re-read the clock afterwards rather than
extrapolating. If a rename is needed anyway, rename the file, patch its `Timestamp:`
line, and then grep the whole evidence tree for the old filename before moving on.
At minute granularity two artifacts written in the same minute will share a label;
that is acceptable, but say so explicitly in any artifact whose gate asserts ordering,
and cite a second-precision tiebreak such as the two distinct log files' creation
times.

Related: [[evidence-timestamp-collision-clobbers-artifacts]] covers the different
failure where a *reused* `<TS>` overwrites an earlier artifact outright.
