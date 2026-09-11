---
name: git-grep-c-empty-is-the-allowlisted-line-count-oracle
description: Under TaskMaster Bash discipline, `git grep -c "" HEAD -- <paths>` is the only prompt-free way to get exact line counts; the Read tool renders a phantom trailing line, so eyeballing the last numbered row overcounts by one and silently confirms a wrong exact-count gate.
metadata:
  type: project
---

Use `git grep -c "" HEAD -- <path> [<path>...]` to obtain exact per-file line counts during preflight. It is a `git *` command, so it runs without a permission prompt, it accepts many paths in one call, and the count it returns equals `(Get-Content -LiteralPath $p).Count`.

**Why:** The Read tool renders one extra numbered row after the final closing brace (the empty string produced by splitting on the trailing newline). Reading that row as content overcounts by one. On the #821 preflight the plan, and the orchestrator's own "authoritative, re-derived twice" fact list, both asserted `UtilitiesCS/Threading/ProgressViewer.cs` at 93 lines; `git grep -c ""` returned 92 while returning the plan's exact figure for the other seven files in the same call. That single stale digit sat inside a Phase 0 exact-count gate whose task text said "a disagreement means the tree moved; stop and report", so it would have hard-stopped execution at the fourteenth task.

**How to apply:** Whenever a plan pins exact line counts, or a file-size budget lands a file at or near the 500-line ceiling, re-derive every count in one `git grep -c ""` call rather than trusting the plan, the caller's fact list, or a visual read of the last line number. Compare all files at once — a list where seven of eight agree is the shape this defect takes, and the one that disagrees is the load-bearing one. See [[project_caller_stated_preflight_count_drifts_before_execution]] and [[project_preflight_gate_literal_extract_from_plan_not_retype]].
