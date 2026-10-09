---
name: 985-review-residuals
description: '#985 (borrowed test packages + solution-wide binding-redirect sync) cycle 1 2026-10-09T14-55 REMEDIATION_REQUIRED (account name in encoded scratchpad dir); cycle 2 2026-10-09T15-35 PASS, 0 blocking, AC7 pending CI; hook simulation needs -RawPayload'
metadata:
  type: project
---

Cycle 1 (head de397b992, base 9911fe138): REMEDIATION_REQUIRED, 1 blocking (B-1 autonomous: account name inside encoded session-scratchpad dir name in the committed plan). Cycle 2 (head 07d3a8a98, review 2026-10-09T15-35): PASS, 0 blocking, AC1-6 PASS, AC7 PENDING CI; R1 also fixed CR-1 (Direction field), CR-2 (WrittenPath de-dup), CR-3 (case-insensitive handled set) in scope.

**Reusable points:**
- Host-identifier sweep must include ENCODED forms: the scratchpad name `C--Users-<account>-repos-...` carries the account name. Sweep added lines for the bare account name, its 8.3 short form, host, both e-mails, plus a drive-rooted users regex; use the `.git` pointer file as a positive control. Precedent [[752-review-residuals]] makes it blocking.
- `validate_orchestration_artifacts` (policy-audit) FAILS with "cannot report PASS or READY when resolve_policy_audit_template_asset is reported as missing or not exposed" if a section names that tool as unavailable. Use a neutral `## Template Provenance` section instead.
- Hook simulation: dot-source `.claude/hooks/validate-feature-review-coverage.ps1` and call `Invoke-FeatureReviewCoverageValidation -RawPayload (@{output=...}|ConvertTo-Json)` (parameter is `-RawPayload`, not `-HookInput`). Bundled PoshQC artifact again 0/10477 -> explicit FAIL line plus PASS line, per [[poshqc-bundled-coverage-artifact-reads-zero]].
- A re-audit where R1 touched no C#-family file may carry cycle-0 C# evidence forward; cite the executor's not-applicable proof and confirm with `git show --stat <R1 sha>`.
- Direct Pester run (New-PesterConfiguration over tests/scripts/{dependencies,hygiene,vscode}, coverage scripts/{...}) reproduced the executor figures exactly (447, 94.99%, 1898/1998) in ~1-2 min.

**Follow-ups owed:** rehearsal worktree `rehearsal-985-throwaway` cleanup (still present at cycle 2); workflow comment drift promoted to #986; remediation-plan status line stale ("Draft").
