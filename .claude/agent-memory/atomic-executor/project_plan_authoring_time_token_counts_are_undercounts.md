---
name: plan-authoring-time-token-counts-are-undercounts
description: a plan's "count observed while authoring" for a single-line token is an unmeasured claim; measure it yourself, because the authoring pass often counts only the occurrences that stand alone on their own line
metadata:
  type: project
---

A plan task that says "the count observed while authoring this plan was N" is stating an authoring-time observation, not a binding acceptance value. Measure it. Two independent drifts in one execution of the #647 plan:

- `BASELINE_FILENAME_PARAM_COUNT` for the token `string filename,` in `FileIO2.cs`: plan said **5**, measured **7**. The authoring pass counted the five occurrences that sit alone on their own line in a wrapped parameter list and missed the two embedded inside single-line declarations (`DELETE_TextFile(string filename, string stagingPath)` and `WriteTextFile(string filename, ...)`).
- `BASELINE_IVT_COUNT` for `InternalsVisibleTo`: plan said **36**, measured **37**, because the branch was reconciled against `origin/main` after the plan was authored.

**Why:** the later gate is almost always phrased "equals the integer **recorded** in P<x>-T<y> plus 1", with a parenthetical naming the authoring-time number ("which is 6 when that recorded value is the 5 observed while authoring"). The *recorded* value governs and the parenthetical is a conditional that does not apply. Copying the plan's number into the artifact instead of measuring makes the artifact false AND can make the later gate unsatisfiable — the post-change count was 8, which satisfies "measured 7 + 1" but not "plan's 5 + 1 = 6".

**How to apply:** in any baseline task whose acceptance is "records an integer under this field name", run the count yourself and write the measured value. Record the divergence under a `DRIFT:` line naming both numbers and explaining the mechanism, and state which later gate reads the field and what it now requires. Never re-type a figure from plan prose into an evidence artifact.

Beware the counting-method mismatch too: `grep -c` counts matching *lines*, not matches, and driven through `xargs` it silently skips tracked paths containing a space (`UtilitiesCS/To Depricate/`). It reported 31 files where a PowerShell `[regex]::Matches` sweep found 35 files and 37 matches. Fix the counting method in the baseline artifact so the post-change gate reproduces it exactly.

Related: [[feedback_verify_line_citations_with_numbered_output]], [[project_preflight_gate_literal_extract_from_plan_not_retype]], [[project_preflight_selfderived_gate_thresholds_are_blind]]
