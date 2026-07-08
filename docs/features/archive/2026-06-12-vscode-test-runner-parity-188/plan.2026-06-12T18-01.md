# vscode-test-runner-parity (Plan)

- **Issue:** #188
- **Parent (optional):** none
- **Owner:** drmoisan
- **Last Updated:** 2026-06-12T18-01
- **Status:** Draft
- **Version:** 1.0
- **Work Mode:** minor-audit (small path)
- **Directive:** MINIMAL-AUDIT PLAN

## Requirements Source

- Sole requirements source: `docs/features/active/2026-06-12-vscode-test-runner-parity-188/issue.md`, `## Acceptance Criteria` section (AC1–AC7) plus the explicit "Out of scope (explicitly deferred)" note.
- No `spec.md`, `user-story.md`, or `research.md` is required or consulted for this minor-audit plan. Presence of `spec.md` or `user-story.md` in the active folder is a fail-closed condition.

## Scope Constraints (do not exceed)

- Production PowerShell files in scope (max 3): `scripts/vscode/Invoke-MSTest.ps1`, `scripts/vscode/Invoke-MSTestWithCoverage.ps1`, and `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1` (only if a seam must live there).
- Test files in scope (max 3): Pester test file(s) under `tests/scripts/vscode/`.
- Do NOT modify `TaskMaster.runsettings` content (AC6).
- Do NOT touch Tesseract/OCR tests, `ImageStripper`, `EmailTokenizer`, or `MailItemHelper`.
- Do NOT modify `.vscode/tasks.json` unless strictly required to wire the change; prefer leaving task definitions unchanged.

## Deferred-Failure Scoping Note (read before execution)

The Tesseract/OCR external-file defect (18 failures from loading a real `eng.traineddata` from `%LOCALAPPDATA%\TaskMaster\tessdata`) is explicitly OUT OF SCOPE and deferred to a separate tracked change. This plan delivers MSTest configuration parity only; it does NOT drive the full C# MSTest suite to zero failures. Test-evidence tasks below are therefore scoped to the new Pester tests and the PowerShell toolchain for the changed scripts. The executor MUST NOT block on, attempt to fix, or report failure for the deferred OCR MSTest failures. A non-green full C# MSTest run is the expected, intended parity outcome after this change.

## Evidence Conventions

- All evidence artifacts are written under the canonical feature evidence root:
  `docs/features/active/2026-06-12-vscode-test-runner-parity-188/evidence/<kind>/`.
- Each command-step artifact MUST include: `Timestamp:` (ISO-8601 `yyyy-MM-ddTHH-mm`), `Command:`, `EXIT_CODE:`, and `Output Summary:`.
- Non-canonical evidence paths (e.g., `artifacts/baselines/`, `artifacts/qa/`, `artifacts/coverage/`) are forbidden and fail preflight.

**Fail-closed evidence rule:** If any required Phase 0 baseline artifact, Phase 2 final-QC artifact, or coverage-comparison artifact is missing or has incomplete fields, the audit verdict must be BLOCKED or INCOMPLETE, never PASS.

---

### Phase 0 — Baseline Capture

- [x] [P0-T1] Read policy files in required order and record `evidence/baseline/phase0-instructions-read.md`. Artifact MUST include `Timestamp:`, `Policy Order:`, and an explicit list of files read: `CLAUDE.md`, `.claude/rules/general-code-change.md`, `.claude/rules/general-unit-test.md`, `.claude/rules/powershell.md`. Verification: artifact exists with all three required fields populated.
- [x] [P0-T2] Confirm minor-audit preconditions and record `evidence/baseline/phase0-mode-precondition.md`: confirm `docs/features/active/2026-06-12-vscode-test-runner-parity-188/issue.md` contains a `## Acceptance Criteria` section (AC1–AC7); confirm `spec.md` and `user-story.md` are ABSENT from the active folder. Verification: artifact records `Work Mode: minor-audit`, `AcceptanceCriteriaSection: present`, `spec.md: absent`, `user-story.md: absent`. If either spec/user-story file is present, halt fail-closed.
- [x] [P0-T3] Record branch/commit baseline in `evidence/baseline/phase0-branch-commit.md`: current branch name and HEAD commit SHA. Verification: artifact contains a resolved branch name and a 40-char (or short) commit SHA.
- [x] [P0-T4] Capture baseline PowerShell format state for the in-scope scripts via `mcp__drm-copilot__run_poshqc_format` against `scripts/vscode/Invoke-MSTest.ps1`, `scripts/vscode/Invoke-MSTestWithCoverage.ps1`, `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1`. Record `evidence/baseline/phase0-poshqc-format.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` (formatting clean vs. files changed). Verification: artifact present with all four fields.
- [x] [P0-T5] Capture baseline PSScriptAnalyzer state for the in-scope scripts via `mcp__drm-copilot__run_poshqc_analyze`. Record `evidence/baseline/phase0-poshqc-analyze.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` (analyzer diagnostic count by severity). Verification: artifact present with all four fields.
- [x] [P0-T6] Capture baseline Pester state for the in-scope test directory `tests/scripts/vscode/` via `mcp__drm-copilot__run_poshqc_test` using `scripts/powershell/PoshQC/settings/pester.runsettings.psd1`. Record `evidence/baseline/phase0-pester.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` including passed/failed/total counts and the numeric line-coverage headline for the in-scope scripts (baseline percent). Verification: artifact present with all four fields and a numeric coverage value (no placeholder).

---

### Phase 1 — Constrained Small-Path Implementation

- [x] [P1-T1] In `scripts/vscode/Invoke-MSTest.ps1`, introduce the wrapper-function seam `Invoke-VsTestExe` with signature `Invoke-VsTestExe -VsTestArgs <string[]>` (parameter name is NOT `Args`) that splats into the real executable (`& $vstestPath @VsTestArgs`). Verification: function exists with the exact parameter name `VsTestArgs`; the production code path calls it instead of invoking `& $vstestPath` inline. (AC4)
- [x] [P1-T2] In `scripts/vscode/Invoke-MSTest.ps1`, resolve `TaskMaster.runsettings` deterministically from the existing `$repoRoot` (`Join-Path $repoRoot 'TaskMaster.runsettings'`) and fail fast with a clear, specific error if the file is absent (e.g., `throw "Runsettings file not found: <path>"`). Verification: a guard throws a specific message naming the missing path when the file does not exist. (AC3)
- [x] [P1-T3] In `scripts/vscode/Invoke-MSTest.ps1`, construct the vstest argument list so it includes `/Settings:<repo-root>\TaskMaster.runsettings` in addition to the existing assemblies and `/InIsolation`, and pass that list through `Invoke-VsTestExe -VsTestArgs`. Prefer extracting argument construction into a small testable function (mirroring the `Get-MSBuildBuildArguments` pattern in `Invoke-VSBuild.ps1`). Add a `-NoExecute` switch parameter consistent with `Invoke-VSBuild.ps1` so the script can be dot-sourced in tests without executing. Verification: constructed argument list contains a `/Settings:` entry whose path resolves to the repo-root `TaskMaster.runsettings`. (AC1)
- [x] [P1-T4] In `scripts/vscode/Invoke-MSTestWithCoverage.ps1`, introduce the smallest seam needed to make the inner vstest argument construction assertable (a `Invoke-VsTestExe` wrapper and/or a small argument-construction function; if a wrapper for the `dotnet-coverage` invocation is also required, name it `Invoke-DotnetCoverageExe -DotnetCoverageArgs <string[]>` — parameter name NOT `Args`). Seam helpers may live in `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1` if needed. Add a `-NoExecute` switch consistent with the other scripts. Verification: argument construction is reachable from a dot-sourced test scope without launching `dotnet-coverage` or `vstest.console.exe`. (AC4)
- [x] [P1-T5] In `scripts/vscode/Invoke-MSTestWithCoverage.ps1`, resolve `TaskMaster.runsettings` deterministically from `$repoRoot` and fail fast with a clear, specific error if absent; then include `/Settings:<repo-root>\TaskMaster.runsettings` in the inner vstest portion of the `dotnet-coverage` command (after `--` and `$vstestPath`, alongside the assemblies and `/InIsolation`). The existing `dotnet-coverage --settings coverage.config` (instrumentation excludes) MUST remain unchanged and distinct from the vstest runsettings. Verification: the constructed `dotnet-coverage` argument list still contains `--settings <coverage.config>` AND the inner vstest segment contains `/Settings:` pointing at the repo-root `TaskMaster.runsettings`. (AC2, AC3)
- [x] [P1-T6] Confirm `TaskMaster.runsettings` content is unchanged and still contains `<MSTest><Parallelize><Workers>0</Workers><Scope>ClassLevel</Scope></Parallelize></MSTest>`. Verification: `git diff -- TaskMaster.runsettings` produces no output. (AC6)
- [x] [P1-T7] Add Pester tests under `tests/scripts/vscode/` (mirroring the `Invoke-VSBuild.Tests.ps1` dot-source-and-assert layout) that assert, for BOTH `Invoke-MSTest.ps1` and `Invoke-MSTestWithCoverage.ps1`, the constructed argument list includes `/Settings:` pointing at the repo-root `TaskMaster.runsettings`. Tests MUST mock only the wrapper seam(s) (`Invoke-VsTestExe`, and `Invoke-DotnetCoverageExe` if introduced) with mock parameter signatures matching production exactly (`param([string[]]$VsTestArgs)`); tests MUST NOT mock the real `vstest.console.exe` or `dotnet-coverage`. Include a negative test asserting the fail-fast error is thrown when the runsettings file is absent. Tests must be deterministic and produce identical results in terminal and VS Code Test Explorer (register mocks before code resolves commands; no PATH/CWD assumptions). Verification: new tests assert the `/Settings:` argument for both scripts and the missing-runsettings throw. (AC5)

---

### Phase 2 — Final QC Loop

- [x] [P2-T1] Run PowerShell formatter on all changed scripts and tests via `mcp__drm-copilot__run_poshqc_format`. Record `evidence/qa-gates/final-poshqc-format.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`. If formatting changes any file, restart the QC loop from this task. Verification: artifact present with all four fields and EXIT_CODE 0 with no residual changes.
- [x] [P2-T2] Run PSScriptAnalyzer on all changed scripts and tests via `mcp__drm-copilot__run_poshqc_analyze`. Record `evidence/qa-gates/final-poshqc-analyze.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` (diagnostic count by severity). Verification: artifact present with all four fields and no new analyzer debt relative to the Phase 0 baseline (AC7). If the analyzer changes files, restart from P2-T1.
- [x] [P2-T3] (Type checking not applicable to PowerShell — recorded explicitly, no command.) Record `evidence/qa-gates/final-typecheck-na.md` noting `Type checking: N/A for PowerShell per .claude/rules/powershell.md`. Verification: artifact present documenting the N/A determination.
- [x] [P2-T4] Run the in-scope Pester tests in coverage mode via `mcp__drm-copilot__run_poshqc_test` using `scripts/powershell/PoshQC/settings/pester.runsettings.psd1`, scoped to `tests/scripts/vscode/`. Record `evidence/qa-gates/final-pester.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` including passed/failed/total counts and the numeric post-change line-coverage headline for the changed scripts. Verification: all in-scope Pester tests pass and the artifact records a numeric post-change coverage value (no placeholder). This task is scoped to the PowerShell test suite for the changed scripts; it does NOT run the full C# MSTest suite and MUST NOT be blocked by the deferred OCR failures.
- [x] [P2-T5] Record coverage delta/threshold verification in `evidence/qa-gates/final-coverage-comparison.md` comparing Phase 0 baseline line coverage (from `evidence/baseline/phase0-pester.md`) to post-change line coverage (from `final-pester.md`) for the changed scripts, plus new/changed-line coverage. Verification: artifact reports baseline percent, post-change percent, and new/changed-line coverage; confirms no coverage regression on changed lines and that new code meets the >= 90% target (AC7). If required coverage values are unavailable, mark the outcome remediation-required (not PASS).
- [x] [P2-T6] Record acceptance-criteria reconciliation in `evidence/qa-gates/final-ac-reconciliation.md` mapping AC1–AC7 to the implementing task(s) and verifying evidence; confirm the deferred OCR item remains out of scope and untouched. Verification: every AC1–AC7 is mapped to a completed, evidence-backed task; out-of-scope items confirmed unchanged.
