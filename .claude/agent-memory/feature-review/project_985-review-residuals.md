---
name: 985-review-residuals
description: '#985 (borrowed test packages + solution-wide binding-redirect sync) full-bug review 2026-10-09T14-55 REMEDIATION_REQUIRED, 6/7 AC + AC7 pending CI, 1 blocking (account name inside encoded scratchpad dir name in plan, autonomous); policy-audit validator rejects PASS when the template-asset tool is described as missing/not exposed'
metadata:
  type: project
---

Review of `bug/dependabot-repair-borrowed-packages-and-transitive-redirects-985` (head de397b992, base 9911fe138): REMEDIATION_REQUIRED, 1 blocking (B-1 autonomous), AC1-6 PASS, AC7 PENDING CI (post-merge `@dependabot recreate`), 6 non-blocking.

**Reusable points:**
- Host-identifier sweep must include ENCODED forms: the session scratchpad name `C--Users-<account>-repos-...` carries the account name even when the plan carefully avoids drive-rooted paths (written via `$env:LOCALAPPDATA`). A drive-root regex misses it; sweep added lines for the bare account name case-insensitively. Precedent [[752-review-residuals]] makes it blocking.
- `validate_orchestration_artifacts` (policy-audit) FAILS with "cannot report PASS or READY when resolve_policy_audit_template_asset is reported as missing or not exposed" if a deviation section names that tool as unavailable. Use a neutral `## Template Provenance` section ("the template-asset selector step was not run from this agent session") instead.
- Hook simulation: dot-source `.claude/hooks/validate-feature-review-coverage.ps1` from the repo root and call `Invoke-FeatureReviewCoverageValidation` with `@{output=...}|ConvertTo-Json`. Summary detected only PowerShell (`.psm1` classified as tooling but `.ps1` lines still match). Bundled PoshQC artifact again 0/10477 (.claude/.codex only) -> need an explicit FAIL line plus PASS line, per [[poshqc-bundled-coverage-artifact-reads-zero]].
- Manifest/csproj-only C# change: C# PASS from committed JaCoCo projection + summary (85.41/79.83); 1.2.1 C# bullet without `New/changed-code coverage:` field validated.
- Bash `pwsh -NoProfile -Command` with New-PesterConfiguration ran the dependencies suite in-session (178 tests, ~1 min) — usable for independent PS coverage.

**Follow-ups owed:** workflow comment drift at dependabot-repair.yml 87-105; downgrade visibility in sync report; WrittenPath de-dup; rehearsal worktree `rehearsal-985-throwaway` cleanup.
