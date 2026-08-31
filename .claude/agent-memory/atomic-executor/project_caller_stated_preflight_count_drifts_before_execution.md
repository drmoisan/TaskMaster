---
name: caller-stated-preflight-count-drifts-before-execution
description: A path count quoted in a delegation prompt as "preflight confirmed N paths" is a measurement of a superseded tree; measure it yourself and evaluate the plan's actual clause, which is usually a membership test rather than an equality.
metadata:
  type: project
---

When a delegation prompt asserts a concrete measured figure ("preflight confirmed the anchored
listing returns exactly 42 paths"), treat it as an observation of the tree at preflight time, not as
the gate. Re-measure, then evaluate the clause the **plan** actually writes.

**Why:** on #644 `[P5-T20]`, the caller stated 42 paths (6 code + 36 feature-folder). The measured
post-commit figure was 48 (6 code + 42 feature-folder). Nothing was wrong: the plan's own tasks keep
writing evidence artifacts into the feature folder *after* preflight ran, and `[P5-T20]` stages the
whole feature folder, so every artifact written between preflight and the commit becomes newly
tracked and joins an anchored `git diff --name-only`. The count is monotonically increasing by
construction across the tail of the plan. An executor that treated the quoted 42 as a gate would have
raised a spurious `REMEDIATION-REQUIRED` on a passing task.

The plan's actual clause was a **membership test** — "lists all six code paths, and every other path
it lists is under `<feature-folder>/`" — which is stable under that growth. The planner wrote it that
way deliberately; the caller's prose summarised it as an equality.

**How to apply:** when a delegation prompt quotes a figure, (1) find the clause in the plan file and
read whether it is an equality or a membership test, (2) measure the figure yourself, (3) evaluate the
plan's clause, and (4) report both the measured value and the divergence from the quoted one in the
final report, without adjusting the clause either way. Related: [[project_preflight_selfderived_gate_thresholds_are_blind]],
[[project_exact_count_gate_vs_remediation_loop]].
