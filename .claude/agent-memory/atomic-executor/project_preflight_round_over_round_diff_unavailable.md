---
name: preflight-round-over-round-diff-unavailable
description: A confirming preflight round asked to prove "nothing outside the delta changed" usually cannot diff round N against round N+1, because the plan file's only commit predates every revision round
metadata:
  type: project
---

When an orchestrator asks a confirming preflight round to verify that the planner
"edited nothing outside the N replacements", check whether a round-N snapshot actually
exists before promising that comparison. It normally does not: the plan file is committed
once when the feature folder is opened, and every revision round edits the working copy
without committing, so `git diff` against HEAD returns the whole plan rather than the
round-over-round delta. `git log -- <plan path>` returning a single "open the active folder"
commit is the tell.

**Why:** on issue #826 round 5 the orchestrator stated "`git diff` against the base is
available to you read-only, and the plan file is tracked", which is true but does not yield
what the obligation asks for. Claiming the comparison was made would be a false evidence
statement; silently skipping it would drop a load-bearing obligation.

**How to apply:** substitute an equivalent that is actually performable and say plainly
which one you ran — a full re-read of every line plus independent re-derivation of every
citation, gate literal, task-ID sequence, phase heading and write-set entry against the
current tree. Then state the limit explicitly: no file-level proof that only the delta
changed is available, and here is what was verified instead. Related:
[[project_preflight_citation_match_propagates_false_fact]],
[[feedback_verify_line_citations_with_numbered_output]],
[[feedback_confirmatory_preflight_proportionate_bar]].
