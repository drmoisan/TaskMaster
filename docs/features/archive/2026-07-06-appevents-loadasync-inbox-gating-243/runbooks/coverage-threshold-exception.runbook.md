# Coverage Threshold Exception Runbook

## Cue

Use this runbook when issue #243 orchestration reaches final QA with all functional checks passing, no changed-code coverage regression, and repository-wide C# line coverage below the 80.0000% policy threshold only by the documented 0.0080 percentage-point gap.

## Prerequisites

- The full C# coverage command has completed with exit code 0.
- The full coverage run reports 4,972 tests passed and 0 tests failed.
- `artifacts/csharp/coverage.xml` exists and parses as Cobertura XML.
- Final repository line coverage is 79.9920%.
- Baseline repository line coverage is 79.9234%.
- Changed executable line coverage for issue #243 is 100.0000%.
- The repository owner has explicitly authorized the coverage-threshold exception for this orchestration closeout.

## Step-by-step Instructions

1. Confirm the full coverage result from `docs/features/active/2026-07-06-appevents-loadasync-inbox-gating-243/evidence/qa-gates/remediation-csharp-coverage-delta.2026-07-06T12-29.md`.
2. Confirm `artifacts/csharp/coverage.xml` parses and reports line rate `0.79992`.
3. Confirm the exception applies only to the 80.0000% repository-wide threshold and not to failed tests, changed-code coverage, missing artifacts, formatting, analyzer, nullable, file-size, or whitespace gates.
4. Record the exception in `artifacts/orchestration/orchestrator-state.json` under `human_interaction.requirements[]` with `response` set to `exception` and `runbook_path` set to this file.
5. Mark orchestration complete only after the final checkpoint validates with `require_complete=true`.

## Verification

- `validate_orchestration_artifacts` accepts `artifacts/orchestration/orchestrator-state.json` with `require_complete=true`.
- `git diff --check` reports no whitespace errors.
- The final response reports the exception and the exact coverage values.

## Source and Citation

- Source: Current Codex conversation, user authorization message: "I authorize the exception. Please finish orchestration." Captured: 2026-07-06.
- Source: `docs/features/active/2026-07-06-appevents-loadasync-inbox-gating-243/evidence/qa-gates/remediation-csharp-coverage-delta.2026-07-06T12-29.md`. Captured: 2026-07-06.
- Source: `artifacts/csharp/coverage.xml`. Captured: 2026-07-06.
