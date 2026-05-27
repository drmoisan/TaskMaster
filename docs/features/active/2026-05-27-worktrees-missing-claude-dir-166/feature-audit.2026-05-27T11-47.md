# Feature Audit — Issue #166 (worktrees-missing-claude-dir)

- Generated: 2026-05-27T11-47
- Reviewer: feature-review agent
- Work Mode (from issue.md): minor-audit
- Acceptance-criteria source (per work mode): the explicit `## Acceptance Criteria` section in `issue.md`

> Template note: the MCP `feature-audit-template` asset is not available in this branch.
> This artifact is constructed to the required shape (the five mandated sections).

## Scope and Baseline

- Base branch (resolved): development
- Merge-base SHA: b7bd81626a512c70c264a8badad5fa5691ca1c16
- Head SHA: ca531c67b1a3605562894a5fe49d7cd38b382819
- Range: b7bd81626a512c70c264a8badad5fa5691ca1c16..ca531c67b1a3605562894a5fe49d7cd38b382819
- Scope: full branch diff against the merge-base (70 files: 45 `.md`, 17 `.ps1`, 9 `.txt`, 1 `.json`, 1 `.gitignore`). The single hand-edited production change is `.gitignore`; the remainder are previously-untracked `.claude/` content that becomes tracked as a consequence of the fix.

## Acceptance Criteria Inventory

Work mode is `minor-audit`, for which the authoritative AC source is the explicit
`## Acceptance Criteria` section in `issue.md`.

Finding: `issue.md` does NOT contain a `## Acceptance Criteria` section. The issue digest in
`artifacts/pr_context.summary.txt` confirms: "Acceptance Criteria: (not provided in potential
file)". Per the workflow fail-closed rule, `minor-audit` selected with no `## Acceptance
Criteria` section in `issue.md` requires remediation.

To allow a substantive evaluation rather than a bare procedural failure, the following
implicit criteria are derived (and labeled as derived, not authoritative) from the issue's
`## Expected Behavior` and `## Resolution` sections:

- AC-D1 (derived): A new git worktree contains the `.claude/` agentic environment (`agents/`, `hooks/`, `rules/`, `skills/`, `settings.json`) because that content is tracked.
- AC-D2 (derived): The Issue #149 invariant is preserved — `.claude/settings.local.json` and `.claude/agent-memory/` remain git-ignored.
- AC-D3 (derived): The change is limited to the repository-root `.gitignore` (no edits to `.claude/` file contents).
- AC-D4 (derived): Verification is captured via deterministic `git check-ignore` evidence (pre-fix defect proof and post-fix allowed/invariant proofs).

## Acceptance Criteria Evaluation

| Criterion | Verdict | Evidence |
|---|---|---|
| Authoritative `## Acceptance Criteria` section present in issue.md | FAIL | No such heading in `issue.md`; digest confirms AC not provided. This is a procedural remediation trigger for `minor-audit`. |
| AC-D1: `.claude/` tooling now tracked / would materialize in worktrees | PASS | Live `git check-ignore .claude/skills .claude/agents .claude/hooks .claude/rules .claude/settings.json` prints nothing and exits 1 (no longer ignored). `git add -n .claude` dry-run (`evidence/qa/166-git-add-dryrun.txt`) stages all five tooling areas. |
| AC-D2: Issue #149 invariant preserved | PASS | Live `git check-ignore .claude/settings.local.json .claude/agent-memory/orchestrator/MEMORY.md` prints both and exits 0 (still ignored). Dry-run confirms neither is staged. |
| AC-D3: change limited to `.gitignore` | PASS | `git diff <merge-base>..<head> -- .gitignore` is the only hand-edited production change; no `.claude/` file contents were modified (added-as-is). |
| AC-D4: deterministic verification evidence captured | PASS | `evidence/regression/166-pre-fix-check-ignore.txt`, `evidence/qa/166-post-fix-check-ignore-allowed.txt`, `evidence/qa/166-post-fix-check-ignore-still-ignored.txt`. |
| Coverage obligation for changed PowerShell files | FAIL | 17 added `.ps1` files; only stale 0% coverage artifact exists. See policy-audit Coverage Verification. |

## Summary

The functional defect described in Issue #166 is resolved: the `.gitignore` edit causes the
`.claude/` tooling to be tracked so it materializes in git worktrees, while the Issue #149
exclusions are preserved. All four derived behavioral criteria pass under live verification.

However, the feature audit verdict is FAIL for two reasons that block PR readiness:

1. Procedural: `minor-audit` requires an explicit `## Acceptance Criteria` section in
   `issue.md`, which is absent. Remediation is required to add it (or to re-mode the work).
2. Coverage: 17 PowerShell production files enter the branch diff without valid coverage;
   repo-wide PowerShell line coverage is 0.00% in the only available artifact.

Overall feature-audit verdict: FAIL (remediation required).

## Acceptance Criteria Check-off

No authoritative `## Acceptance Criteria` checklist exists in `issue.md`, so there are no
authoritative criteria to check off per `acceptance-criteria-tracking`. No source files were
modified to record check-offs (the reviewer does not author acceptance criteria). The derived
criteria AC-D1 through AC-D4 are evaluated above for completeness but are not checked off in
any source document because they are reviewer-derived, not authoritative.

Action required before check-off is possible: add an explicit `## Acceptance Criteria` section
to `issue.md` (remediation item RI-3 below in remediation-inputs).
