# Remediation Inputs: Issue #985 (review 2026-10-09T14-55)

Review-Verdict: REMEDIATION_REQUIRED

- Feature folder: `docs/features/active/2026-10-09-dependabot-repair-borrowed-packages-and-transitive-redirects-985`
- Branch head reviewed: `de397b992868882530a447c662d21b3fbff123eb`; merge base `9911fe138952e2b93476850582847c2831e1cbbd`
- Source audit artifacts:
  - `docs/features/active/2026-10-09-dependabot-repair-borrowed-packages-and-transitive-redirects-985/policy-audit.2026-10-09T14-55.md`
  - `docs/features/active/2026-10-09-dependabot-repair-borrowed-packages-and-transitive-redirects-985/code-review.2026-10-09T14-55.md`
  - `docs/features/active/2026-10-09-dependabot-repair-borrowed-packages-and-transitive-redirects-985/feature-audit.2026-10-09T14-55.md`
- Next link: `atomic-planner` authors the remediation plan per `remediation-handoff-atomic-planner`; this review does not author the plan.

## Remediation-Required Findings

### B-1 Account name embedded in the committed plan

- Severity: Blocking
Remediability: autonomous
Remediability-Evidence: a one-line text substitution in a tracked Markdown document inside the feature folder; no external system, policy decision, or CI result is involved.
- File: `docs/features/active/2026-10-09-dependabot-repair-borrowed-packages-and-transitive-redirects-985/plan.2026-10-09T13-06.md`, line 26 (the `SCRATCH` definition bullet).
- Problem: the line writes the encoded session-scratchpad directory name twice (once in prose, once inside the `Join-Path` command). That encoded segment contains the operator account name. The repository host-identifier rule (`.claude/agent-memory/_shared_no_absolute_host_paths.md`) prohibits an account name in any committed file.
- Expected behaviour after the fix: no added line in `git diff 9911fe138...HEAD` contains the account name, a drive-rooted path, the host name, or the user e-mail address.
- Fix: replace the encoded directory segment in both occurrences with a placeholder such as `<encoded-worktree>\<session-id>` (keep the remaining text of the bullet unchanged), then commit.
- Verification commands:
  - `git diff 9911fe138952e2b93476850582847c2831e1cbbd...HEAD` piped to a case-insensitive fixed-string search for the account name, restricted to added lines: expect 0 matches.
  - The same sweep for the host name and the user e-mail: expect 0 matches.
  - Re-run the sweep over the remediation cycle's own plan and audit documents (cycle-document sweep scope).

## Non-Blocking Items (no remediation required in this cycle)

- AC7 is pending CI by design (post-merge, `@dependabot recreate` on PR #984); the item's orchestrator run checks it off after CI is observed.
- Code-review Minor items: downgrade visibility in the sync report; workflow comment drift in `.github/workflows/dependabot-repair.yml` lines 87-105 (requires a separate change because this item excludes workflow edits); possible duplicate entries in `WrittenPath`. Recommend follow-up issues through the MCP promotion lifecycle.
- O-1: the bundled PoshQC coverage document `artifacts/pester/powershell-coverage.xml` instruments only `.claude/` and `.codex/`; direct Pester figures govern.
- O-3: rehearsal worktree and local branch `rehearsal-985-throwaway` remain outside the repository.

## Do Not Do

- Do not modify production or test code, manifests, or `.github/workflows/**` in this cycle.
- Do not edit policy documents under `.claude/rules/` or `.github/instructions/`.
- Do not alter acceptance-criteria text or check off AC7.
- Do not rewrite evidence files other than to remove the identifier if the sweep finds it elsewhere.
