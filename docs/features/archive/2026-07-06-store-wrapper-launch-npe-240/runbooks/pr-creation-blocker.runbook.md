# Human-Exception Runbook — PR Creation Blocked by Orchestrator-State Preflight (Issue #240)

This runbook is the human follow-up for the `exception` response recorded against the "create the
GitHub pull request for issue #240" requirement. It is contract-conformant per
`.claude/skills/human-exception-runbook/SKILL.md` (Cue, Prerequisites, Step-by-step Instructions,
Verification, Source and Citation).

## Cue

Act on this runbook when the orchestrator has recorded an `exception` response for the "create PR"
requirement on issue #240, or when a subsequent `gh pr create --body-file artifacts/pr_body_240.md
--base main` attempt is denied with `permissionDecision: deny` and reason
`ORCHESTRATOR_STATE_PREFLIGHT_FAILED`.

Verified root cause: the registered PreToolUse hook
`.claude/hooks/enforce-pr-author-skill.ps1` runs `Invoke-OrchestratorStatePreflight`, whose default
`$Invoker` script block calls:

```
python -m scripts.dev_tools.validate_orchestration_artifacts orchestrator-state
  artifacts/orchestration/orchestrator-state.json --require-pr-creation-ready
```

The Python package `scripts/dev_tools` does not exist in this repository (`ModuleNotFoundError: No
module named 'scripts.dev_tools'`), so the invoker exits non-zero on every invocation regardless of
checkpoint content. `Invoke-OrchestratorStatePreflight` treats any non-zero exit as
`HasErrors = $true`, and `Get-PrAuthorBypassReason` (`.claude/hooks/enforce-pr-author-skill.ps1`,
lines 359-371) returns `ORCHESTRATOR_STATE_PREFLIGHT_FAILED` for every `--body-file` command,
including a well-formed one. This was confirmed by directly simulating the hook against
`gh pr create --body-file artifacts/pr_body_240.md --base main`. Every other precondition for PR
creation (code committed, tests passing, audit artifacts written, branch pushed) is otherwise
satisfied; only the final `gh pr create` step is blocked. Direct `gh api` calls or an inline
`--body` argument are prohibited paths (Case A/B in the same hook) because they bypass the mandatory
`pr-author` skill and its SHA-256 receipt binding.

## Prerequisites

- Read/write access to the `drmoisan/TaskMaster` GitHub repository, sufficient to open a pull
  request from branch `TaskMaster-wt-2026-07-06-06-35`.
- Confirmation that the branch `TaskMaster-wt-2026-07-06-06-35` is pushed to `origin` and contains
  the committed fix for issue #240 (already verified in this case).
- Read access to the feature folder
  `docs/features/active/2026-07-06-store-wrapper-launch-npe-240/`, specifically:
  - `issue.md` (acceptance criteria; AC6 — "All required PR CI checks are green against the PR head
    SHA" — remains open pending PR creation and CI).
  - `code-review.2026-07-06T13-00.md`, `feature-audit.2026-07-06T13-00.md`,
    `policy-audit.2026-07-06T13-00.md` (most recent audit artifacts).
- For Option 1 only: repository-maintainer or CI-infrastructure authority to add a Python module or
  modify `.claude/hooks/enforce-pr-author-skill.ps1`, since this is out-of-scope, maintainer-owned
  infrastructure work rather than issue #240 application code.
- For Option 2 only: no additional tooling access is required beyond standard GitHub web access; the
  human bypasses the local hook entirely by using the GitHub web UI directly rather than the `gh`
  CLI.

## Step-by-step Instructions

Two independent resolution paths are documented. Option 2 creates the PR immediately without
changing repository infrastructure. Option 1 additionally restores autonomous PR creation for future
issues and is maintainer/CI-infrastructure work outside issue #240's scope; it is not required to
merge issue #240's fix.

### Option 1 — Unblock automation (maintainer/CI-infrastructure scope, outside issue #240)

1. Confirm the gap: run `python -m scripts.dev_tools.validate_orchestration_artifacts
   orchestrator-state artifacts/orchestration/orchestrator-state.json --require-pr-creation-ready`
   from the repository root and confirm it fails with `ModuleNotFoundError: No module named
   'scripts.dev_tools'`.
2. Choose one of two remediations for `.claude/hooks/enforce-pr-author-skill.ps1`
   (`Invoke-OrchestratorStatePreflight`, default `$Invoker` parameter, lines 70-78):
   - **2a. Provision the missing module.** Add a `scripts/dev_tools/validate_orchestration_artifacts`
     Python module that implements the `orchestrator-state <path> --require-pr-creation-ready`
     subcommand contract already assumed by the hook (exit 0 on a checkpoint whose
     `next_step`/`blocked_reason` indicate PR-creation readiness per steps 5-8 of the orchestrator
     state machine; non-zero otherwise). This is new infrastructure code, not part of issue #240's
     fix, and must go through its own change-plan, toolchain, and review.
   - **2b. Repoint the invoker to the available MCP validator.** The MCP tool
     `mcp__drm-copilot__validate_orchestration_artifacts` is already registered in
     `.claude/settings.json` and performs the equivalent checkpoint validation. Modify the default
     `$Invoker` script block (or add an MCP-backed override) in
     `.claude/hooks/enforce-pr-author-skill.ps1` so the preflight calls this MCP tool instead of the
     nonexistent Python module, preserving the same `HasErrors`/`ErrorText` contract consumed by
     `Get-PrAuthorBypassReason`.
3. Apply the repository's PowerShell toolchain to any hook change (format, analyze, Pester test) per
   `.claude/rules/powershell.md`, and add/adjust Pester coverage for
   `Invoke-OrchestratorStatePreflight` to exercise the corrected invoker.
4. Re-run the standard `pr-author` flow: `mcp__drm-copilot__collect_pr_context`, then the `pr-author`
   skill to produce `artifacts/pr_body_240.md` and its sibling receipt, then
   `gh pr create --body-file artifacts/pr_body_240.md --base main` from branch
   `TaskMaster-wt-2026-07-06-06-35`, and confirm the hook now returns `permissionDecision: allow`.

### Option 2 — Create the PR now, manually (recommended to unblock issue #240 immediately)

1. Open the repository `drmoisan/TaskMaster` in a browser and confirm the pushed branch
   `TaskMaster-wt-2026-07-06-06-35` is visible in the branch list (GitHub shows a yellow "Compare &
   pull request" banner for a recently pushed branch with no open PR).
2. Select "Compare & pull request" for `TaskMaster-wt-2026-07-06-06-35`, or navigate to
   `https://github.com/drmoisan/TaskMaster/compare/main...TaskMaster-wt-2026-07-06-06-35` directly.
3. In the branch-selection dropdowns, set the base branch to `main` and confirm the compare
   (head) branch is `TaskMaster-wt-2026-07-06-06-35`.
4. Enter the PR title and description. Use the feature folder
   `docs/features/active/2026-07-06-store-wrapper-launch-npe-240/` as the source for the
   description content:
   - Summary and root cause: `issue.md` (Summary, Suspected Cause / Notes, Acceptance Criteria
     sections).
   - Verification/audit evidence: `code-review.2026-07-06T13-00.md`,
     `feature-audit.2026-07-06T13-00.md`, `policy-audit.2026-07-06T13-00.md`.
   - Note explicitly in the description that AC6 ("All required PR CI checks are green against the
     PR head SHA") is verified only after CI runs against this PR's head SHA, not before.
5. Select "Create Pull Request" (not "Create Draft Pull Request", unless a draft is otherwise
   required by team convention).
6. Record the resulting PR number and URL in the feature folder's evidence trail (for example a new
   `evidence/other/pr-created.md` entry) so the checkpoint's `human_interaction` record can reference
   the completed exception.

## Verification

- The pull request exists in `drmoisan/TaskMaster` with base branch `main` and head branch
  `TaskMaster-wt-2026-07-06-06-35`. Confirm via the PR's "Files changed" / "Commits" tabs, or with
  `gh pr view <number> --json baseRefName,headRefName` showing `"baseRefName": "main"` and
  `"headRefName": "TaskMaster-wt-2026-07-06-06-35"`.
- Required CI checks begin running against the PR head SHA (visible in the PR's "Checks" tab).
  Acceptance criterion AC6 in `issue.md` ("All required PR CI checks are green against the PR head
  SHA") is satisfied only once those checks complete and pass; this runbook creates the PR and
  triggers CI but does not itself satisfy AC6.
- If Option 1 was also completed: re-attempt `gh pr create --body-file artifacts/pr_body_<N>.md
  --base main` (or `gh pr edit`) for a subsequent issue and confirm the hook returns
  `permissionDecision: allow` rather than `ORCHESTRATOR_STATE_PREFLIGHT_FAILED`.

## Source and Citation

- Non-UI root-cause citation (repository source, primary): `.claude/hooks/enforce-pr-author-skill.ps1`,
  `Invoke-OrchestratorStatePreflight` (lines 49-88) and `Get-PrAuthorBypassReason` (lines 293-382).
  Captured/read: 2026-07-06.
- Non-UI citation for the available MCP alternative: `.claude/settings.json`, line 23
  (`mcp__drm-copilot__validate_orchestration_artifacts` registered as an allowed MCP tool). Captured/
  read: 2026-07-06.
- Non-UI citation for the `pr-author` skill contract that Option 1 step 4 and Option 2 must not
  bypass: `.claude/skills/pr-author/SKILL.md`. Captured/read: 2026-07-06.
- Sourcing-order note: per the two-axis-model-selection spec's Out of Scope section, no callable MCP
  documentation-retrieval tool is currently wired in this repository, so the skill's "MCP-first"
  clause for the third-party UI step below could not be satisfied with an MCP source; `WebFetch` was
  used as the sole available web-second mechanism. This is a repository-wide limitation, not specific
  to this runbook, and is not resolved by this agent.
- Third-party UI step source (Option 2, web-second, MCP unavailable per the note above): GitHub Docs
  — "Creating a pull request" (Compare & pull request button; base/head branch selection). Source
  URL: https://docs.github.com/en/pull-requests/collaborating-with-pull-requests/proposing-changes-to-your-work-with-pull-requests/creating-a-pull-request
  — updated_at (capture date): 2026-07-06.
- Feature-folder content sources for the PR description (Option 2, step 4):
  `docs/features/active/2026-07-06-store-wrapper-launch-npe-240/issue.md`,
  `docs/features/active/2026-07-06-store-wrapper-launch-npe-240/code-review.2026-07-06T13-00.md`,
  `docs/features/active/2026-07-06-store-wrapper-launch-npe-240/feature-audit.2026-07-06T13-00.md`,
  `docs/features/active/2026-07-06-store-wrapper-launch-npe-240/policy-audit.2026-07-06T13-00.md`.
  Captured/read: 2026-07-06.
