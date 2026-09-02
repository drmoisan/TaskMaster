---
name: scope-gate-cannot-list-artifacts-written-after-it
description: A terminal scope/porcelain gate that must "list every evidence artifact this plan names" is unsatisfiable, because its own artifact plus every later phase's artifacts do not exist when its capture runs
metadata:
  type: project
---

A late scope-boundary gate that captures `git status --porcelain --untracked-files=all` and asserts
the capture "lists every `EVIDENCE/<kind>/` artifact path this plan names" can never pass. Three
classes of artifact are missing at capture time:

1. the gate's **own** artifact, written after the capture inside the same task;
2. artifacts written by **later tasks in the same phase** (e.g. a following file-size audit);
3. artifacts written by **later phases** (e.g. an AC-traceability table in the reconciliation phase).

**Why:** the plan author reads the artifact inventory as a static list of what the plan produces,
but the gate observes the tree at one instant partway through producing it. Observed on issue #731
[P5-T9], where the clause named all of `scope-boundary.md` (self), `file-size-audit.md` ([P5-T10])
and `ac-traceability.md` ([P6-T1]).

**How to apply:** at preflight, for every "must list every artifact" clause, resolve the artifact
inventory against the task ordering and confirm each named artifact is written by a task that runs
*strictly before* the capture. The fix is to bound the assertion by task range ("every artifact named
by [P0-T1] through [P5-T8]") and to name the excluded later artifacts explicitly, so the exclusion
is auditable rather than implicit.

Related: [[project_artifact_output_summary_breaks_its_own_exact_count_gate]],
[[project_sanitisation_task_cannot_sweep_its_own_record]],
[[project_agent_memory_tracked_breaks_unscoped_git_gates]].
