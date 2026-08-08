---
name: pre-applied-deltas-reconcile-to-stated-wording
description: When a caller says part of a delta is "already applied — verify, do not duplicate", diff the in-file text against the delta's literal wording; partial pre-application often leaves the weaker phrasing in place
metadata:
  type: feedback
---

When a revision directive marks some deltas as "already applied by the orchestrator — verify, do not
duplicate", do not treat the presence of the corrected *number* or *fact* as satisfying the delta.
Compare the in-file sentence against the delta's literal replacement text and reconcile to the
delta's wording wherever they differ.

**Why:** In the #497 F16 capstone revision, `[P0-T14]` had already been changed to read "all fifteen
`feature_folder` values from the manifest" — the right count — but the delta's required text was
"every `features[]` entry other than F16 — fifteen at planning time — with the count re-derived from
the manifest at execution time rather than asserted". The pre-applied version still *asserted* a
count; the delta made the task re-derive it. Same for the stale-folder sentence: the file said
"Manifest folder names may be stale", the delta named which entries were verified and why F2 is the
only resolution edge case. Accepting the pre-applied text would have shipped a weaker gate under a
"verified" label.

**How to apply:** For every delta flagged as already-applied, quote the current line and the delta's
target line side by side before deciding. If they are not equivalent in *strength* (asserted vs
re-derived, "may be" vs a named verified state), apply the delta's wording. Only skip when the two
are semantically identical. Related: [[verify-caller-supplied-citation-corrections]],
[[re-derive-plan-aggregate-claims-after-every-delta]].
