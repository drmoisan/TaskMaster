# Feature Audit: Coverage threshold policy reconciliation (#494)

**Audit Date:** 2026-08-13
**Feature Folder:** `docs/features/active/2026-08-10-coverage-threshold-policy-reconciliation-494`
**Base Branch:** `epic/build-ci-coverage-gate-fidelity-integration`
**Head Branch:** `bug/coverage-threshold-policy-reconciliation-494`
**Work Mode:** `full-bug`
**Audit Type:** Post-remediation acceptance verification

## Scope and Baseline

- **Base branch:** `epic/build-ci-coverage-gate-fidelity-integration` (`d863a5c3712776eee81bbf811e45523f13a380cb`).
- **Head branch/commit:** `bug/coverage-threshold-policy-reconciliation-494` (`8f36c21e324b6b9d04e65f659fad4c5ad1d6ef19`).
- **Merge base:** `c7d398c2aa0da6963de239ff6719b4b23a7d3f45`.
- **Evidence sources:** primary `artifacts/pr_context.summary.txt`; secondary `artifacts/pr_context.appendix.txt`; feature evidence under `evidence/`.
- **Requirements source:** `spec.md`, resolved from `issue.md` work mode `full-bug`.
- **Scope note:** The binding upstream prompt permits repository-specific `.claude/agent-memory/**` updates but the corrected issue/spec/plan prohibit all `.claude/**` changes. The branch range contains six memory paths and no non-memory Claude runtime changes.

## Acceptance Criteria Inventory

**Authoritative AC source files for this run:**

- `docs/features/active/2026-08-10-coverage-threshold-policy-reconciliation-494/spec.md` — only authoritative source for the persisted `full-bug` mode.

### Acceptance criteria

1. AC1 — Existing upstream prompt is retained as the complete TaskMaster deliverable; no TaskMaster `CLAUDE.md` or `.claude/**` file is changed; future application is deferred.
2. AC2 — Prompt requires upstream reconciliation of exclusion/exemption policy without a local Claude-runtime edit.
3. AC3 — Prompt requires one authoritative upstream coverage source without a local Claude-runtime edit.
4. AC4 — Local tooling rejects a valid synthetic result below 80%, accepts exact 80%, and has deterministic negative-path evidence.
5. AC5 — Prompt carries the #424/#230 precedent disposition without a local Claude-runtime edit.
6. AC6 — Prompt carries the false-claim disposition without editing `.claude/**` or `.agents/skills/**`.
7. AC7 — Corrected-arithmetic evidence is retained and validated as input, not threshold authority.
8. AC8 — Prompt carries the upstream feature-review hook reconciliation requirement without editing `.claude/hooks/**`.
9. AC9 — Added Pester tests mirror the coverage-tooling subjects, are deterministic, and create no temporary files.
10. AC10 — Prompt identifies future affected paths and records deferral without protected Claude or Codex runtime-policy edits.

## Acceptance Criteria Evaluation

| # | Criterion | Status | Evidence | Verification command(s) | Notes |
|---:|---|---|---|---|---|
| 1 | Upstream prompt only; no local `CLAUDE.md` or `.claude/**` change | FAIL | Prompt validation; six `.claude/agent-memory/**` paths in range | `git diff --name-status base...HEAD -- CLAUDE.md .claude` | Memory paths are permitted by the binding prompt but contradict this stale literal AC wording. |
| 2 | Upstream exclusion-policy requirement | PASS | `evidence/other/upstream-claude-policy-reconciliation-prompt.2026-08-11T12-41.md` | Prompt inspection | No Claude runtime path changed. |
| 3 | One upstream authoritative coverage source | PASS | Upstream prompt Required upstream work item 2 | Prompt inspection | Deferred locally as required. |
| 4 | 80% local threshold and negative-path evidence | PASS | Threshold tests and fail-before/pass-after evidence | Targeted MCP Pester; helper diff | 51 targeted tests passed. |
| 5 | #424/#230 upstream disposition | PASS | Upstream prompt Required upstream work item 2 | Prompt inspection | Deferred locally. |
| 6 | False-claim disposition without `.claude/**` or `.agents/skills/**` edit | FAIL | Prompt; range path set | `git diff --name-status base...HEAD -- .claude .agents/skills` | Stale literal wording conflicts with permitted memory exception. |
| 7 | Corrected-arithmetic evidence as input only | PASS | `evidence/other/ac7-remeasurement-input.2026-08-11T13-46.md` | Evidence inspection | No evidence selects or lowers the 80% threshold. |
| 8 | Upstream hook reconciliation requirement | PASS | Upstream prompt Required upstream work item 4 | Prompt inspection | No `.claude/hooks/**` path changed. |
| 9 | Deterministic Pester coverage-tooling tests | PASS | Targeted test run and regression-testing artifacts | `mcp__drm-copilot__run_poshqc_test` | 51 passed, 0 failed; in-memory XML and mocks. |
| 10 | Future affected paths and deferred runtime policy edits | PASS | Upstream prompt acceptance criteria and range inspection | Prompt inspection; path diff | No protected runtime policy surface changed. |

## Summary

**Overall Feature Readiness:** NEEDS REVISION

**Criteria summary:**

- **PASS:** 8 criteria
- **PARTIAL:** 0 criteria
- **UNVERIFIED:** 0 criteria
- **FAIL:** 2 criteria

**Top gaps preventing PASS:**

1. AC1 and AC6 state a blanket `.claude/**` prohibition that conflicts with the permitted repository-specific agent-memory exception.
2. `issue.md`, `spec.md`, and `plan.2026-08-10T14-10.md` need a single, consistent scope statement.

**Recommended follow-up verification steps:**

1. Amend the scope and AC wording to distinguish prohibited Claude runtime customizations from the permitted `.claude/agent-memory/**` path class.
2. Re-run the base-to-head protected-path check and targeted PowerShell test/analyzer checks.

## Acceptance Criteria Check-off

No acceptance-source checkbox was modified in this review. `spec.md` already marks all ten ACs checked, but AC1 and AC6 are not satisfied by their current literal wording. The remediation must reconcile the source wording and then revalidate individual criteria.

### AC Status Summary

- Source: `docs/features/active/2026-08-10-coverage-threshold-policy-reconciliation-494/spec.md`
- Total AC items: 10
- Checked off (source state): 10
- Remaining (audit disposition): 2
- Items remaining: AC1 and AC6, pending scope-text remediation.

| Source File | Total AC | Checked (source state) | Unchecked by audit disposition | Notes |
|---|---:|---:|---:|---|
| `spec.md` | 10 | 10 | 2 | Existing checkboxes were not modified during review; two checked criteria have stale wording. |
