# P6-T1 — Modified-Workflow Green-Run Requirement Recorded

Timestamp: 2026-07-20T04-50

## Modified workflow

`.github/workflows/ci.yml` is modified by this feature (Phase 3, P3-T1: the "Build with nullable
warnings treated as errors" step's `run:` block, dropping the global `/p:Nullable=enable`
override).

## Rule citation

This modification triggers the `modified-workflow-needs-green-run` policy rule documented in
`.claude/skills/feature-review-workflow/SKILL.md`: a diff under a GitHub Actions workflow file is
Blocking at feature-review time unless a green workflow run against the branch head is present in
remediation inputs.

## Status

**NOT SATISFIED BY THIS PLAN.** Capturing a green CI run against this branch's head on the actual
GitHub Actions runner is an execution/merge-time obligation that this planning-and-execution pass
cannot itself satisfy (it requires a real GitHub Actions workflow dispatch/PR run). This
obligation is carried by **epic-orchestrator**, which is responsible for triggering and recording
the green run against the branch head (or the fan-in integration branch head) before this
capstone's changes are merged to `main`.
