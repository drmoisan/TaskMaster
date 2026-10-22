# Remediation Baseline — Git Identity

Timestamp: 2026-08-23T18-59

Command:
```
git rev-parse --abbrev-ref HEAD
git rev-parse HEAD
git merge-base origin/main HEAD
git status --porcelain | wc -l
```

EXIT_CODE: 0

Output Summary:

| Field | Value |
| --- | --- |
| Branch | `bug/winformspumphost-suite-determinism-511-exec` |
| HEAD sha (provenance only) | `733b91ca71393f6723e2cdbf20a1e8ebee7cd1fc` |
| Merge base with `origin/main` (`$MergeBase`) | `f85a36faebaaec29fe5233c9d9f69d223d80e4c5` |
| `git status --porcelain` line count | 6 |

The recorded merge-base sha is a 40-character hexadecimal string and is the commit `main` sits on
(`f85a36fa`), confirming the branch is rebased current. All later scope-lock tasks in this cycle gate
on tree invariants measured against `$MergeBase = f85a36faebaaec29fe5233c9d9f69d223d80e4c5`, never
against a pinned HEAD.

The six porcelain lines at baseline are:

```
 M .claude/agent-memory/atomic-planner/MEMORY.md
 M .claude/agent-memory/atomic-planner/project_csharp_coverage_gate_jacoco_format.md
 M docs/features/active/winformspumphost-suite-determinism-511/remediation-plan.2026-08-23T20-57.md
?? .claude/agent-memory/atomic-planner/csharpier-formatted-n-is-processed-count.md
?? .claude/agent-memory/atomic-planner/project_511_r1_preflight_delta_seams.md
?? docs/features/active/winformspumphost-suite-determinism-511/evidence/remediation-baseline/
```

Two of them are prior-agent memory writes under `.claude/agent-memory/` (permitted by plan
prohibition 6 and admitted to the P4-T9 commit), one is this cycle's own plan file carrying the
P0-T1 through P0-T5 check-offs, and one is the evidence directory this artifact is being written
into.
