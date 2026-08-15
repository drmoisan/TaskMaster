# Issue #494 — Scope-Corrected Remediation Plan

- **Issue:** #494
- **Work mode:** `full-bug`; `docs/features/active/2026-08-10-coverage-threshold-policy-reconciliation-494/spec.md` is the sole acceptance-criteria source.
- **Plan purpose:** implement the remaining TaskMaster-owned coverage-tooling and test work, and verify the existing upstream prompt as the complete local deliverable for prohibited Claude-runtime changes.
- **Scope correction:** `upstream-release-validation-receipt` has response `scope_change`. No upstream receipt, release, validation, repository write, or release evidence is required to complete TaskMaster work.

## Scope and evidence conventions

- `docs/features/active/2026-08-10-coverage-threshold-policy-reconciliation-494/evidence/other/upstream-claude-policy-reconciliation-prompt.2026-08-11T12-41.md` is the complete TaskMaster deliverable for every required change to `CLAUDE.md` or `.claude/**`. Future application of that prompt is explicitly deferred outside TaskMaster.
- Do not write `CLAUDE.md`, `.claude/**`, `.agents/skills/**`, or any external repository path. Do not stage, commit, publish, or refresh `artifacts/pr_context.*`.
- The only TaskMaster production paths this plan may modify are `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1` and `scripts/vscode/Invoke-MSTestWithCoverage.ps1`. The only TaskMaster test paths this plan may modify are `tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1` and `tests/scripts/vscode/Invoke-MSTest.RunSettings.Tests.ps1`.
- Evidence is written only below `docs/features/active/2026-08-10-coverage-threshold-policy-reconciliation-494/evidence/<kind>/`. Every command artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:`. Baseline and final test artifacts include numeric total and changed-file coverage values.
- Existing baseline evidence may be cited only after its schema and applicability are verified. The corrected-arithmetic evidence under `evidence/baseline/` is an observation; it must not select or lower a threshold.

### Phase 0 — Baseline and Scope Guards

- [x] [P0-T1] Read `AGENTS.md`, `.agents/skills/policy-compliance-order/SKILL.md`, `.agents/skills/atomic-plan-contract/SKILL.md`, `.agents/skills/evidence-and-timestamp-conventions/SKILL.md`, `.agents/skills/acceptance-criteria-tracking/SKILL.md`, `.agents/skills/powershell/SKILL.md`, `.github/instructions/general-code-change.instructions.md`, `.github/instructions/general-unit-test.instructions.md`, `.github/instructions/powershell-code-change.instructions.md`, and `.github/instructions/powershell-unit-test.instructions.md` in that order; write `docs/features/active/2026-08-10-coverage-threshold-policy-reconciliation-494/evidence/remediation-baseline/phase0-instructions-read.<runtime ISO timestamp>.md`.
  - Acceptance: the artifact lists every file in order and records the full-bug `spec.md` AC source, the scope-change disposition, the prohibited-path guard, and the two permitted production paths.

- [x] [P0-T2] Capture `git status --porcelain`, `git diff --name-only`, and `git diff --check` in `C:\Users\DanMoisan\repos\TaskMaster`; write `docs/features/active/2026-08-10-coverage-threshold-policy-reconciliation-494/evidence/remediation-baseline/taskmaster-scope-baseline.<runtime ISO timestamp>.md`.
  - Acceptance: the artifact lists pre-existing changed paths, reports the `git diff --check` exit code, and identifies the four permitted implementation/test paths without asserting that unrelated concurrent changes are clean.

- [x] [P0-T3] Validate the existing baseline artifacts `evidence/baseline/powershell-analyze.2026-08-11T13-15.md`, `evidence/baseline/powershell-pester-mcp.2026-08-11T13-15.md`, `evidence/baseline/powershell-baseline-coverage.2026-08-11T13-15.md`, `evidence/baseline/coverage-remeasurement-spread.2026-08-11T13-46.md`, and `evidence/other/ac7-remeasurement-input.2026-08-11T13-46.md`; write `docs/features/active/2026-08-10-coverage-threshold-policy-reconciliation-494/evidence/remediation-baseline/reused-baseline-validation.<runtime ISO timestamp>.md`.
  - Acceptance: the artifact records each source path, required schema fields, current applicability, the 64-pass/0-fail/69.4047619047619% PowerShell baseline, and the AC7 restriction that measured figures cannot choose a threshold.

- [x] [P0-T4] Run `mcp__drm-copilot__run_poshqc_format` for `C:\Users\DanMoisan\repos\TaskMaster`; write `docs/features/active/2026-08-10-coverage-threshold-policy-reconciliation-494/evidence/baseline/powershell-format.<runtime ISO timestamp>.md`.
  - Acceptance: the artifact records the exact MCP request and every changed path; if formatting changes a path outside the four permitted implementation/test paths, record `REMEDIATION-REQUIRED` and do not continue to Phase 1.

- [x] [P0-T5] Run `mcp__drm-copilot__run_poshqc_analyze` for `C:\Users\DanMoisan\repos\TaskMaster`; write `docs/features/active/2026-08-10-coverage-threshold-policy-reconciliation-494/evidence/baseline/powershell-analyze.<runtime ISO timestamp>.md`.
  - Acceptance: the artifact records the exact MCP request, exit code, and diagnostic count for later zero-regression comparison.

- [x] [P0-T6] Run `mcp__drm-copilot__run_poshqc_test` for `C:\Users\DanMoisan\repos\TaskMaster`, scoped to `tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1` and `tests/scripts/vscode/Invoke-MSTest.RunSettings.Tests.ps1`, then run direct `Invoke-Pester` with a `New-PesterConfiguration` whose `Run.Path` is exactly those two tests and whose `CodeCoverage.Path` is exactly `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1` and `scripts/vscode/Invoke-MSTestWithCoverage.ps1`; write `docs/features/active/2026-08-10-coverage-threshold-policy-reconciliation-494/evidence/baseline/powershell-coverage.<runtime ISO timestamp>.md`.
  - Acceptance: the artifact records the exact MCP request and completion result, then the exact direct `Invoke-Pester` command, its exit code, passed/failed/skipped/total counts, numeric overall coverage, and numeric per-file coverage for both permitted production files. The direct command supplies the numeric evidence because the MCP response does not provide it; neither command uses `scripts/powershell/PoshQC/settings/pester.runsettings.psd1`.

### Phase 1 — Deterministic Threshold Gate

- [x] [P1-T1] Add focused Pester cases to `tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1` for a missing Cobertura summary, malformed numeric data, a result below 80%, exactly 80%, and above 80%; also add one `[expect-fail]` Pester case to `tests/scripts/vscode/Invoke-MSTest.RunSettings.Tests.ps1` that mocks the collection and conversion dependencies, exercises `Invoke-MSTestWithCoverageMain`, and verifies that the generated Cobertura result is evaluated before P1-T3 and P1-T4; write `docs/features/active/2026-08-10-coverage-threshold-policy-reconciliation-494/evidence/regression-testing/threshold-gate-test-added.<runtime ISO timestamp>.md`.
  - Acceptance: each case is isolated, uses in-memory XML or objects only, creates no temporary file, and has a descriptive `It` name that defines the expected failure or pass result. The `Invoke-MSTestWithCoverageMain` case uses mocks registered before invocation, proves its generated Cobertura result reaches the absent evaluator, and fails before P1-T3 and P1-T4 because that evaluator does not yet exist.

- [x] [P1-T2] [expect-fail] Before P1-T3 and P1-T4, run `mcp__drm-copilot__run_poshqc_test` scoped to `tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1` and `tests/scripts/vscode/Invoke-MSTest.RunSettings.Tests.ps1`, then run direct `Invoke-Pester` against exactly those two tests; write `docs/features/active/2026-08-10-coverage-threshold-policy-reconciliation-494/evidence/regression-testing/threshold-gate-fail-before.<runtime ISO timestamp>.md`.
  - Acceptance: the `[expect-fail]` artifact records the exact MCP request and result plus the exact direct `Invoke-Pester` command and result. The direct command has a non-zero exit caused by the absent evaluator, identifies the failing test names including the mocked `Invoke-MSTestWithCoverageMain` case, and records no passing outcome.

- [x] [P1-T3] Add one pure threshold-evaluation function to `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1` that validates a Cobertura line-coverage summary and throws a specific error when the percentage is below 80; write `docs/features/active/2026-08-10-coverage-threshold-policy-reconciliation-494/evidence/regression-testing/threshold-evaluator-implementation.<runtime ISO timestamp>.md`.
  - Acceptance: the function accepts explicit inputs, has no filesystem or process dependency, rejects absent or non-numeric coverage deterministically, and treats 80% as passing.

- [x] [P1-T4] Invoke the threshold-evaluation function from `scripts/vscode/Invoke-MSTestWithCoverage.ps1` immediately after `ConvertTo-KoverageCoberturaXml` produces the final coverage document and before the success completion message; write `docs/features/active/2026-08-10-coverage-threshold-policy-reconciliation-494/evidence/regression-testing/threshold-gate-wiring.<runtime ISO timestamp>.md`.
  - Acceptance: a below-80 final coverage document makes `Invoke-MSTestWithCoverageMain` terminate non-zero, while a valid 80-or-higher document preserves the existing success path.

- [x] [P1-T5] After P1-T3 and P1-T4, run `mcp__drm-copilot__run_poshqc_test` scoped to `tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1` and `tests/scripts/vscode/Invoke-MSTest.RunSettings.Tests.ps1`, then run direct `Invoke-Pester` against exactly those two tests; write `docs/features/active/2026-08-10-coverage-threshold-policy-reconciliation-494/evidence/regression-testing/threshold-gate-pass-after.<runtime ISO timestamp>.md`.
  - Acceptance: the artifact records the exact MCP request and completion result plus the exact direct `Invoke-Pester` command and a zero direct exit code. All five helper scenarios and the mocked `Invoke-MSTestWithCoverageMain` result-evaluation case pass, including the below-threshold failure behavior and exact 80% boundary, with no temporary-file, executable, network, or ambient-path dependency.

### Phase 2 — Final PowerShell QA

- [x] [P2-T1] Run `mcp__drm-copilot__run_poshqc_format` for `C:\Users\DanMoisan\repos\TaskMaster`; write `docs/features/active/2026-08-10-coverage-threshold-policy-reconciliation-494/evidence/qa-gates/powershell-format.<runtime ISO timestamp>.md`.
  - Acceptance: the artifact records the exact MCP request, exit code, and changed paths; if formatting changes a path outside the four permitted implementation/test paths, record `REMEDIATION-REQUIRED` and restart the scope assessment instead of continuing as a pass.

- [x] [P2-T2] Run `mcp__drm-copilot__run_poshqc_analyze` for `C:\Users\DanMoisan\repos\TaskMaster`; write `docs/features/active/2026-08-10-coverage-threshold-policy-reconciliation-494/evidence/qa-gates/powershell-analyze.<runtime ISO timestamp>.md`.
  - Acceptance: the artifact records the exact MCP request, exit code, and diagnostic delta from P0-T5; no new analyzer diagnostic is permitted.

- [x] [P2-T3] Run `mcp__drm-copilot__run_poshqc_test` for `C:\Users\DanMoisan\repos\TaskMaster`, scoped to `tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1` and `tests/scripts/vscode/Invoke-MSTest.RunSettings.Tests.ps1`, then run direct coverage-enabled `Invoke-Pester` with a `New-PesterConfiguration` whose `Run.Path` is exactly those two tests and whose `CodeCoverage.Path` is exactly `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1` and `scripts/vscode/Invoke-MSTestWithCoverage.ps1`; write `docs/features/active/2026-08-10-coverage-threshold-policy-reconciliation-494/evidence/qa-gates/powershell-test-coverage.<runtime ISO timestamp>.md`.
  - Acceptance: the artifact records the exact MCP request and completion result, then the exact direct `Invoke-Pester` command, its exit code, passed/failed/skipped/total counts, numeric overall and per-file coverage, and changed-line coverage. The direct command provides numeric evidence absent from the MCP response. The MCP result completes, the direct exit is zero, each changed production function has changed-line coverage of at least 90%, and neither permitted production file regresses from the P0-T6 baseline.

- [x] [P2-T4] If P2-T1 changes files or P2-T2 or P2-T3 fails, repeat P2-T1 through P2-T3 from the start and write `docs/features/active/2026-08-10-coverage-threshold-policy-reconciliation-494/evidence/qa-gates/powershell-qa-loop.<runtime ISO timestamp>.md`.
  - Acceptance: the artifact identifies the final clean iteration and cites the three final command artifacts; no `SKIPPED` command outcome is reported as passing.

### Phase 3 — Scope and Acceptance Verification

- [x] [P3-T1] Run `git diff --check` and inspect `git diff --name-only` against the P0-T2 baseline; write `docs/features/active/2026-08-10-coverage-threshold-policy-reconciliation-494/evidence/qa-gates/final-scope-validation.<runtime ISO timestamp>.md`.
  - Acceptance: the artifact reports zero prohibited TaskMaster `CLAUDE.md`, `.claude/**`, or `.agents/skills/**` changes and no external-repository path; it identifies only the permitted source/test paths and feature-document/evidence paths introduced by this plan.

- [x] [P3-T2] Verify `docs/features/active/2026-08-10-coverage-threshold-policy-reconciliation-494/evidence/other/upstream-claude-policy-reconciliation-prompt.2026-08-11T12-41.md` remains present and unchanged; write `docs/features/active/2026-08-10-coverage-threshold-policy-reconciliation-494/evidence/other/upstream-prompt-deliverable-validation.<runtime ISO timestamp>.md`.
  - Acceptance: the artifact records the prompt path, its issue #494 identity, the no-TaskMaster-Claude-runtime boundary, and the explicit future-application-outside-TaskMaster disposition without requesting or validating an upstream receipt, release, or external action.

- [x] [P3-T3] Evaluate AC1 in `docs/features/active/2026-08-10-coverage-threshold-policy-reconciliation-494/spec.md` against P3-T2 and, if it passes, change only its marker from `- [ ]` to `- [x]`; write `docs/features/active/2026-08-10-coverage-threshold-policy-reconciliation-494/evidence/issue-updates/ac1-status.<runtime ISO timestamp>.md`.
  - Acceptance: the artifact cites the prompt-validation evidence and records either `PASS` with the sole marker change or `UNVERIFIED` with no marker change.

- [x] [P3-T4] Evaluate AC2 in `docs/features/active/2026-08-10-coverage-threshold-policy-reconciliation-494/spec.md` against P3-T2 and, if it passes, change only its marker from `- [ ]` to `- [x]`; write `docs/features/active/2026-08-10-coverage-threshold-policy-reconciliation-494/evidence/issue-updates/ac2-status.<runtime ISO timestamp>.md`.
  - Acceptance: the artifact cites the prompt-validation evidence and records either `PASS` with the sole marker change or `UNVERIFIED` with no marker change.

- [x] [P3-T5] Evaluate AC3 in `docs/features/active/2026-08-10-coverage-threshold-policy-reconciliation-494/spec.md` against P3-T2 and, if it passes, change only its marker from `- [ ]` to `- [x]`; write `docs/features/active/2026-08-10-coverage-threshold-policy-reconciliation-494/evidence/issue-updates/ac3-status.<runtime ISO timestamp>.md`.
  - Acceptance: the artifact cites the prompt-validation evidence and records either `PASS` with the sole marker change or `UNVERIFIED` with no marker change.

- [x] [P3-T6] Evaluate AC4 in `docs/features/active/2026-08-10-coverage-threshold-policy-reconciliation-494/spec.md` against P1-T5 and P2-T3 and, if it passes, change only its marker from `- [ ]` to `- [x]`; write `docs/features/active/2026-08-10-coverage-threshold-policy-reconciliation-494/evidence/issue-updates/ac4-status.<runtime ISO timestamp>.md`.
  - Acceptance: the artifact cites the negative-path and coverage evidence and records either `PASS` with the sole marker change or `UNVERIFIED` with no marker change.

- [x] [P3-T7] Evaluate AC5 in `docs/features/active/2026-08-10-coverage-threshold-policy-reconciliation-494/spec.md` against P3-T2 and, if it passes, change only its marker from `- [ ]` to `- [x]`; write `docs/features/active/2026-08-10-coverage-threshold-policy-reconciliation-494/evidence/issue-updates/ac5-status.<runtime ISO timestamp>.md`.
  - Acceptance: the artifact cites the prompt-validation evidence and records either `PASS` with the sole marker change or `UNVERIFIED` with no marker change.

- [x] [P3-T8] Evaluate AC6 in `docs/features/active/2026-08-10-coverage-threshold-policy-reconciliation-494/spec.md` against P3-T2 and, if it passes, change only its marker from `- [ ]` to `- [x]`; write `docs/features/active/2026-08-10-coverage-threshold-policy-reconciliation-494/evidence/issue-updates/ac6-status.<runtime ISO timestamp>.md`.
  - Acceptance: the artifact cites the prompt-validation evidence and records either `PASS` with the sole marker change or `UNVERIFIED` with no marker change.

- [x] [P3-T9] Evaluate AC7 in `docs/features/active/2026-08-10-coverage-threshold-policy-reconciliation-494/spec.md` against P0-T3 and, if it passes, change only its marker from `- [ ]` to `- [x]`; write `docs/features/active/2026-08-10-coverage-threshold-policy-reconciliation-494/evidence/issue-updates/ac7-status.<runtime ISO timestamp>.md`.
  - Acceptance: the artifact cites the reused corrected-arithmetic evidence and records either `PASS` with the sole marker change or `UNVERIFIED` with no marker change.

- [x] [P3-T10] Evaluate AC8 in `docs/features/active/2026-08-10-coverage-threshold-policy-reconciliation-494/spec.md` against P3-T2 and, if it passes, change only its marker from `- [ ]` to `- [x]`; write `docs/features/active/2026-08-10-coverage-threshold-policy-reconciliation-494/evidence/issue-updates/ac8-status.<runtime ISO timestamp>.md`.
  - Acceptance: the artifact cites the prompt-validation evidence and records either `PASS` with the sole marker change or `UNVERIFIED` with no marker change.

- [x] [P3-T11] Evaluate AC9 in `docs/features/active/2026-08-10-coverage-threshold-policy-reconciliation-494/spec.md` against P1-T5 and P2-T3 and, if it passes, change only its marker from `- [ ]` to `- [x]`; write `docs/features/active/2026-08-10-coverage-threshold-policy-reconciliation-494/evidence/issue-updates/ac9-status.<runtime ISO timestamp>.md`.
  - Acceptance: the artifact cites deterministic Pester and coverage evidence and records either `PASS` with the sole marker change or `UNVERIFIED` with no marker change.

- [x] [P3-T12] Evaluate AC10 in `docs/features/active/2026-08-10-coverage-threshold-policy-reconciliation-494/spec.md` against P3-T2 and, if it passes, change only its marker from `- [ ]` to `- [x]`; write `docs/features/active/2026-08-10-coverage-threshold-policy-reconciliation-494/evidence/issue-updates/ac10-status.<runtime ISO timestamp>.md`.
  - Acceptance: the artifact cites the prompt-validation evidence and records either `PASS` with the sole marker change or `UNVERIFIED` with no marker change.

- [x] [P3-T13] Write `docs/features/active/2026-08-10-coverage-threshold-policy-reconciliation-494/evidence/issue-updates/ac-status-summary.<runtime ISO timestamp>.md` from P3-T3 through P3-T12.
  - Acceptance: the summary names `spec.md` as the sole full-bug AC source, lists all ten ACs individually with evidence paths, reports checked and unchecked totals, and does not change the contextual `issue.md` checkboxes.
