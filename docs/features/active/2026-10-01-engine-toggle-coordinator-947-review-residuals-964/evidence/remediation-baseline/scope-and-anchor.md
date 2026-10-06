# Scope and anchor (P0-T2)

Timestamp: 2026-10-03T09-23 (host clock read at correction; the label first written was composed, not read)
Command: git status --porcelain --untracked-files=all; git merge-base 6b8e935c177128d2f455f7bcd2fedc7deff6e30f HEAD; git diff --exit-code --stat 6b8e935c177128d2f455f7bcd2fedc7deff6e30f -- TaskMaster TaskMaster.Test; Grep tool over issue.md (patterns `^- Work Mode: minor-audit`, `^## Acceptance Criteria`, `^- \[x\] AC[1-8] `, `^- \[ \] AC`); Read/Grep probes for spec.md, user-story.md, research*.md in the feature folder.
EXIT_CODE: 0
Output Summary: merge-base equals the cycle base; code diff against the cycle base is empty; issue.md counts 1, 1, 8, 0; no spec.md, user-story.md or research*.md; inherited porcelain entries all under the feature folder or .claude/agent-memory/.

MERGE-BASE: 6b8e935c177128d2f455f7bcd2fedc7deff6e30f
ANCHOR-CODE-DIFF-EXIT=0
ISSUE-GREP-COUNTS: work-mode=1, ac-heading=1, checked-AC1-8=8, unchecked-AC=0
REQUIREMENTS-DOCUMENTS: none (Glob returned none; Read of spec.md and user-story.md reported not found; Grep over research*.md found 0)

INHERITED-PORCELAIN:
?? .claude/agent-memory/atomic-planner/project_964_partial_split_sink_guard_plan_seams.md
?? .claude/agent-memory/atomic-planner/project_964_r2_preparation_record_closed_evidence_set.md
?? .claude/agent-memory/atomic-planner/project_964_r3_glob_backslash_and_hit_attribution_seams.md
?? .claude/agent-memory/orchestrator/preparation-clearance-record-breaks-closed-evidence-set.md
?? docs/features/active/2026-10-01-engine-toggle-coordinator-947-review-residuals-964/evidence/remediation-baseline/phase0-instructions-read.md
?? docs/features/active/2026-10-01-engine-toggle-coordinator-947-review-residuals-964/remediation-plan.2026-10-03T08-43.md

Write Set code path (verbatim): TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.SinkGuard.cs

Constraints of the remediation inputs (verbatim):
- Test-only change set: `TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.SinkGuard.cs` only; no production file, csproj or other test file changes. The SinkGuard partial must stay at or below 500 lines.
- MSTest, Moq and FluentAssertions; no temporary files; no `Thread.Sleep` / `Task.Delay`; deterministic.
- Full CLAUDE.md C# toolchain in order (csharpier format and check, analyzer `/t:Rebuild`, `TreatWarningsAsErrors` `/t:Rebuild`, `Invoke-MSTestWithCoverage.ps1`) with numeric coverage evidence, restart from step 1 on any failure or file change.
- Evidence under `docs/features/active/2026-10-01-engine-toggle-coordinator-947-review-residuals-964/evidence/<kind>/` only.
- Do not weaken or edit any acceptance criterion in `issue.md`.

Plan of record: docs/features/active/2026-10-01-engine-toggle-coordinator-947-review-residuals-964/remediation-plan.2026-10-03T08-43.md
