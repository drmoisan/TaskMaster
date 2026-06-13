# Atomic Implementation Plan — Issue #194: global.json SDK pin regressed to 10.0.200

- Issue: #194
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/194
- Feature folder: docs/features/active/2026-06-12-global-json-sdk-pin-regressed-to-10-194/
- Work Mode: minor-audit
- Requirements source: docs/features/active/2026-06-12-global-json-sdk-pin-regressed-to-10-194/issue.md (`## Acceptance Criteria`, AC1–AC4)
- Supporting research: artifacts/research/2026-06-12-global-json-sdk-version-research.md
- Plan timestamp: 2026-06-13T09-00

## Mode Resolution

- `issue.md` metadata declares `- Work Mode: minor-audit`. Plan follows the minor-audit minimal-audit contract: Phase 0 baseline, Phase 1 constrained small-path implementation, Phase 2 final QC loop.
- Sole requirements source is `issue.md`; `spec.md` and `user-story.md` are not required and are expected absent. If either exists in the active folder, execution must fail closed.
- Acceptance Criteria source is the explicit `## Acceptance Criteria` section of `issue.md` only (AC1–AC4).

## Scope and Constraints (already determined; do not re-investigate)

- Exactly one production/config file changes: `global.json`.
- Change: revert `sdk.version` from `10.0.200` to `8.0.205`. Single field only.
- Do NOT modify `rollForward`, `allowPrerelease`, `paths`, or `errorMessage`.
- Do NOT modify `tests/scripts/vscode/Install-RepoDotNetSdk.Tests.ps1`. The regression test already exists and currently fails (line 22 expects `8.0.205`).
- The stale "retry dotnet format" message in `global.json` / install script is cosmetic and OUT OF SCOPE.
- No new test files. No new production scope.

## Evidence Location Invariant

All evidence artifacts MUST be written under `docs/features/active/2026-06-12-global-json-sdk-pin-regressed-to-10-194/evidence/<kind>/` per `evidence-and-timestamp-conventions`. Writing to `artifacts/baselines/`, `artifacts/qa/`, `artifacts/coverage/`, or any other non-canonical path is a policy violation enforced by the `enforce-evidence-locations.ps1` PreToolUse hook.

---

### Phase 0 — Baseline Capture

- [x] [P0-T1] Read policy files in the order defined in `policy-compliance-order`: `CLAUDE.md`, `.claude/rules/general-code-change.md`, `.claude/rules/general-unit-test.md`, `.claude/rules/powershell.md`. Record the read evidence at `docs/features/active/2026-06-12-global-json-sdk-pin-regressed-to-10-194/evidence/baseline/phase0-instructions-read.md` with fields `Timestamp:`, `Policy Order:`, and an explicit list of files read. Acceptance: artifact exists with all three fields populated and the four files listed.
- [x] [P0-T2] Confirm minor-audit preconditions: `docs/features/active/2026-06-12-global-json-sdk-pin-regressed-to-10-194/issue.md` contains an explicit `## Acceptance Criteria` section (AC1–AC4), and neither `spec.md` nor `user-story.md` exists in the active feature folder. Record results at `docs/features/active/2026-06-12-global-json-sdk-pin-regressed-to-10-194/evidence/baseline/phase0-mode-preconditions.md` with fields `Timestamp:`, `AcceptanceCriteriaPresent: yes/no`, `SpecPresent: yes/no`, `UserStoryPresent: yes/no`. Acceptance: AC section present is `yes`; spec and user-story present are both `no`. If any condition fails, halt with remediation-required.
- [x] [P0-T3] Capture baseline current value of `global.json` `sdk.version` by reading the repo-root `global.json`. Record at `docs/features/active/2026-06-12-global-json-sdk-pin-regressed-to-10-194/evidence/baseline/global-json-baseline.md` with fields `Timestamp:`, `Command:` (the read/inspection performed), `EXIT_CODE:`, and `Output Summary:` capturing the current `sdk.version` value and confirming `rollForward`/`allowPrerelease`/`paths`/`errorMessage` present. Acceptance: artifact records `sdk.version` baseline as `10.0.200`.
- [x] [P0-T4] [expect-fail] Capture baseline Pester run of the regression suite `tests/scripts/vscode/Install-RepoDotNetSdk.Tests.ps1` using the MCP command `mcp__drm-copilot__run_poshqc_test` with the repo Pester config `scripts/powershell/PoshQC/settings/pester.runsettings.psd1`. The `global.json SDK selection` test is expected to FAIL before the change. Record at `docs/features/active/2026-06-12-global-json-sdk-pin-regressed-to-10-194/evidence/regression-testing/baseline-pester-2026-06-13T09-00.md` with fields `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:` including the failing assertion (expected `8.0.205`, actual `10.0.200`) and pass/fail counts. Acceptance: artifact shows the `global.json SDK selection` test failing on the version assertion (fail-before evidence for AC2).
- [x] [P0-T5] Capture baseline PowerShell format and analyzer state for the regression test file and any related scripts using `mcp__drm-copilot__run_poshqc_format` and `mcp__drm-copilot__run_poshqc_analyze`. Record at `docs/features/active/2026-06-12-global-json-sdk-pin-regressed-to-10-194/evidence/baseline/baseline-poshqc-format-analyze-2026-06-13T09-00.md` with fields `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:` capturing format clean/dirty state and analyzer finding counts. Acceptance: artifact records format and analyzer baseline counts.

---

### Phase 1 — Implementation (constrained small-path)

- [x] [P1-T1] In `global.json`, change the `sdk.version` field value from `"10.0.200"` to `"8.0.205"`. Make no other edits: `rollForward` remains `latestFeature`, `allowPrerelease` remains `false`, `paths` remains `[".dotnet-sdk", "$host$"]`, and `errorMessage` is unchanged. Acceptance (AC1, AC3): `global.json` `sdk.version` equals `8.0.205`; a diff of `global.json` shows exactly one changed line (the version value) and no other key changed; no other repository file is modified.

---

### Phase 2 — Final QC Loop (PowerShell toolchain: format → analyze → test)

Run the PowerShell toolchain in order. If any step changes files or fails, restart from the first step. There is no PowerShell production-code change; the Pester suite under `tests/scripts/vscode` validates the reverted `global.json` config.

- [x] [P2-T1] Run PowerShell formatting via `mcp__drm-copilot__run_poshqc_format`. Record at `docs/features/active/2026-06-12-global-json-sdk-pin-regressed-to-10-194/evidence/qa-gates/final-qa-format-2026-06-13T09-00.md` with fields `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:`. Acceptance (AC4): format step completes with no required changes to changed/related files; if files change, restart from this step.
- [x] [P2-T2] Run PSScriptAnalyzer via `mcp__drm-copilot__run_poshqc_analyze` with repo settings. Record at `docs/features/active/2026-06-12-global-json-sdk-pin-regressed-to-10-194/evidence/qa-gates/final-qa-analyze-2026-06-13T09-00.md` with fields `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:` including finding counts. Acceptance (AC4): no new analyzer findings on changed/related files.
- [x] [P2-T3] Run the Pester suite (coverage-enabled) via `mcp__drm-copilot__run_poshqc_test` using `scripts/powershell/PoshQC/settings/pester.runsettings.psd1`, targeting at minimum `tests/scripts/vscode/Install-RepoDotNetSdk.Tests.ps1`. Record at `docs/features/active/2026-06-12-global-json-sdk-pin-regressed-to-10-194/evidence/qa-gates/final-qa-pester-2026-06-13T09-00.md` with fields `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:` including pass/fail counts and numeric coverage headline (repository-wide line coverage percent). Acceptance (AC2): the `global.json SDK selection` test passes (version `8.0.205`, `rollForward` `latestFeature`, `allowPrerelease` false, `paths` contains `.dotnet-sdk` and `$host$`); suite passes with no failures; coverage remains `>= 80%`. If any step in Phase 2 changed files, restart from P2-T1.
- [x] [P2-T4] Reduced minor-audit reconciliation: verify each Acceptance Criterion against evidence on disk. Record at `docs/features/active/2026-06-12-global-json-sdk-pin-regressed-to-10-194/evidence/qa-gates/minor-audit-reconciliation-2026-06-13T09-00.md` with fields `Timestamp:` and a per-AC status (AC1–AC4) citing the supporting evidence artifact path for each. Acceptance: AC1 (version reverted, other keys unchanged), AC2 (Pester passes), AC3 (only `global.json` modified, single field), AC4 (format/analyze/test pass, no new findings) are each marked PASS with a cited evidence artifact. If any AC cannot be confirmed from evidence, mark remediation-required and do not report PASS.

---

## Acceptance Criteria Traceability

- AC1 — `global.json` `sdk.version` is `8.0.205`; `rollForward`, `allowPrerelease`, `paths` unchanged: P1-T1, P2-T4.
- AC2 — `Install-RepoDotNetSdk.Tests.ps1` `global.json SDK selection` assertions pass: P0-T4 (fail-before), P2-T3 (pass-after), P2-T4.
- AC3 — No other `global.json` keys or unrelated files modified: P1-T1, P2-T4.
- AC4 — PowerShell toolchain (PoshQC format, PSScriptAnalyzer, Pester) passes with no new findings: P2-T1, P2-T2, P2-T3, P2-T4.
