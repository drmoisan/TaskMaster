# Remediation Inputs — Issue #166 (worktrees-missing-claude-dir)

- Generated: 2026-05-27T11-47
- Reviewer: feature-review agent
- Base branch (resolved): development
- Merge-base SHA: b7bd81626a512c70c264a8badad5fa5691ca1c16
- Head SHA: ca531c67b1a3605562894a5fe49d7cd38b382819

## Why Remediation Is Triggered

Remediation is required because all of the following workflow triggers are met:

- The policy audit contains FAIL results (PowerShell coverage; PowerShell tests).
- Coverage is below the policy threshold for a language with changed files
  (PowerShell repo-wide line coverage is 0.00%, below the 80% floor).
- The code review contains Blocker findings.
- The feature audit is FAIL, and the required acceptance-criteria source is absent.

## Source Artifacts

- policy-audit-path: docs/features/active/2026-05-27-worktrees-missing-claude-dir-166/policy-audit.2026-05-27T11-47.md
- code-review-path: docs/features/active/2026-05-27-worktrees-missing-claude-dir-166/code-review.2026-05-27T11-47.md
- feature-audit-path: docs/features/active/2026-05-27-worktrees-missing-claude-dir-166/feature-audit.2026-05-27T11-47.md

## Remediation-Required Findings

### RI-1 (Blocker) — PowerShell coverage is absent/stale for newly-tracked hooks

- Problem: 17 PowerShell production files under `.claude/hooks/` are added in the branch-vs-base
  diff. The only PowerShell coverage artifact (`artifacts/pester/powershell-coverage.xml`) is
  dated 2026-05-06 (predating the feature), reports 0.00% repo-wide line coverage
  (`missed="284" covered="0"`), enumerates only 5 of the 17 changed hooks (all at 0% covered),
  and has no data for the other 12.
- Required outcome: produce current Pester coverage against this branch via
  `mcp__drm-copilot__run_poshqc_test`; repo-wide PowerShell line coverage must be >= 80%, and
  each newly-tracked hook (new file) must reach >= 90% line coverage. If a subset of hooks is
  genuinely out of test scope, that must be justified against the repo scope invariant rather
  than recorded as "N/A."
- Affected files: all 17 `.ps1` files listed in policy-audit Coverage Verification.

### RI-2 (Blocker) — QA toolchain summary misclassifies the change as having no source files

- Problem: `evidence/qa/166-toolchain-summary.txt` states "No source files changed" and
  "Coverage — N/A," which is incorrect: the branch diff adds 17 `.ps1` source files (confirmed
  by `evidence/qa/166-git-add-dryrun.txt`). A coverage-N/A determination for a language with
  changed files is a rejected scope narrowing.
- Required outcome: correct the toolchain determination to include the PowerShell
  format -> analyze -> test -> coverage chain for the changed hooks, and record the results.

### RI-3 (Major / procedural) — Missing authoritative Acceptance Criteria for minor-audit

- Problem: Work Mode is `minor-audit`, which requires an explicit `## Acceptance Criteria`
  section in `issue.md`. That section is absent (digest: "Acceptance Criteria: (not provided)").
- Required outcome: add an explicit `## Acceptance Criteria` section to
  `docs/features/active/2026-05-27-worktrees-missing-claude-dir-166/issue.md` capturing the
  behavioral criteria (e.g., worktree contains `.claude/` tooling; Issue #149 paths remain
  ignored; verification via `git check-ignore`), enabling authoritative check-off. Alternatively,
  re-mode the work if minor-audit is inappropriate.

### RI-4 (Major) — PowerShell format/analyze not run against the changed hooks on this branch

- Problem: No PoshQC format (`Invoke-Formatter`) or PSScriptAnalyzer (`run_poshqc_analyze`)
  evidence exists for the 17 changed `.ps1` files on this branch.
- Required outcome: run `mcp__drm-copilot__run_poshqc_format` and
  `mcp__drm-copilot__run_poshqc_analyze` against the changed hooks; resolve any diagnostics and
  record the results under the feature `evidence/` tree.

## Non-Blocking / Informational

- The `.gitignore` fix itself is correct, minimal, well-commented, and behaviorally verified
  live; no change is requested to the `.gitignore` line.
- No files were written to non-canonical evidence paths; evidence-location compliance passed.

## Handoff Note

The workflow specifies creating the target remediation plan file from the canonical plan
template and handing off via `remediation-handoff-atomic-planner`. That skill and the canonical
plan template are not present in this branch's `.claude/skills/` tree, so the remediation plan
file could not be instantiated from the canonical template in this environment. This
remediation-inputs artifact is the authoritative blocking-findings record for the downstream
planner. The planner should consume RI-1 through RI-4 and produce the phased remediation plan
in the active feature folder.
