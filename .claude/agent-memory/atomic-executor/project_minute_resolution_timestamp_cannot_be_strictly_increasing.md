---
name: minute-resolution-timestamp-cannot-be-strictly-increasing
description: A plan clause demanding strictly increasing artifact timestamps is unsatisfiable when the artifacts use the repo's minute-resolution Timestamp field and two steps finish inside one minute
metadata:
  type: project
---

A single-pass attestation clause of the form "the four timestamps are strictly increasing, proving
they ran in order within one pass" cannot be satisfied from the artifacts' own `Timestamp:` fields.

**Why:** `evidence-and-timestamp-conventions` fixes the format at `yyyy-MM-ddTHH-mm`, which is
minute-resolution. Measured on #911 P9-T8: the analyzer rebuild finished at 01:23:12 and the nullable
rebuild at 01:23:41, 29 seconds apart and inside the same minute, so the two labels tie and the
clause reads FAIL on a correct run. Two solution-wide `/t:Rebuild` gates on this repo take 12 to 14
seconds each, so the collision is the normal case rather than a race.

**How to apply:**
- Satisfy the clause with a **second-resolution** observation taken in one capture, and record both
  series so the reader can see which one carries the claim. The filesystem modification times of the
  four artifacts work: `stat -c '%y'` over the four paths in a single invocation, given each artifact
  was written immediately after its own command returned and before the next was launched.
- State in the artifact that the deviation is one of resolution, not of substance — you are recording
  a finer observation than the minute label can carry, not relaxing the ordering property.
- Do not re-run a step merely to push it into a distinct minute. That manufactures the evidence.
- Do not switch the `Timestamp:` field itself to second resolution; it is consumed by collectors that
  expect the fixed format.
- At preflight this is worth reporting: the clause should name the observation it wants rather than
  "timestamps", because the field it appears to name cannot express the property.

Related: [[project_evidence_timestamp_labels_drift_ahead_of_write_time]],
[[project_evidence_timestamp_collision_clobbers_artifacts]],
[[feedback_never_predict_an_observation_into_an_artifact]].
