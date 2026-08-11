---
name: preflight-ac-checkoff-and-tooloutput-paths
description: Two recurring preflight calls — "record the evidence pointer inline" in an AC check-off task is a policy conflict, and artifacts/pester|csharp coverage XML is a tool-output path, not an evidence path
metadata:
  type: project
---

Two judgments that recur when preflighting plans that end with per-AC check-off tasks.

1. **An AC check-off task that says "with the evidence pointer recorded inline" is a defect.**
   `acceptance-criteria-tracking` rule 3 permits changing only `- [ ]` to `- [x]` in the AC source
   file and forbids modifying criterion text or adding phantom content. Require the delta: the only
   change to `issue.md`/`spec.md` is the marker; the pointer goes in the task output and in the
   `evidence/issue-updates/ac-status-summary.<TS>.md` artifact.

2. **`artifacts/pester/powershell-coverage.xml` and `artifacts/csharp/coverage.xml` are NOT evidence
   paths.** They are the producer/consumer paths that `.claude/hooks/validate-feature-review-coverage.ps1`
   reads, and `artifacts/` is gitignored at repo root. Do not flag them under the
   `evidence-and-timestamp-conventions` forbidden-path clause; that clause targets
   `artifacts/baseline*/`, `artifacts/qa*/`, `artifacts/evidence/`, `artifacts/coverage/`.

**Why:** #494 preflight. Flagging (2) would have sent the planner to rewrite a correct producer
decision; missing (1) would have had the executor edit protected requirement text.

**How to apply:** apply during any preflight of a plan with a Phase-5-style per-AC check-off block,
or with PowerShell/C# coverage gates. Related: [[project_agent_memory_tracked_breaks_unscoped_git_gates]],
[[project_poshqc_pester_mcp_exit_minus1]].
