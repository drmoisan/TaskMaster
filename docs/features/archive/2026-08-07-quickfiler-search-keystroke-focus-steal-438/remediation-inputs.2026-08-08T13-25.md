# Remediation Inputs — Issue #438 Review Cycle 1

- **Date:** 2026-08-08T13-25
- **Produced by:** feature-review agent
- **Source audit artifacts:**
  - `docs/features/active/2026-08-07-quickfiler-search-keystroke-focus-steal-438/policy-audit.2026-08-08T13-25.md` (§5, §8)
  - `docs/features/active/2026-08-07-quickfiler-search-keystroke-focus-steal-438/code-review.2026-08-08T13-25.md` (Findings Table)
  - `docs/features/active/2026-08-07-quickfiler-search-keystroke-focus-steal-438/feature-audit.2026-08-08T13-25.md`
- **Blocking finding count: 1** (R1). All 14 gating acceptance criteria PASS; this cycle is coverage-gate remediation only.
- **Handoff:** per `remediation-handoff-atomic-planner`, the remediation plan is authored by `atomic-planner` (delegated by the orchestrator), preflighted and executed by `atomic-executor`, then re-audited by `feature-review`. Note for the orchestrator: the MCP validators referenced by that skill (`validate_orchestration_artifacts`) are not available in this session's toolset; the operative repo gate is `.claude/hooks/validate-feature-review-coverage.ps1`, and this repository's hook requires the flat `policy-audit.<ts>.md` artifact naming (not the skill's `audit/<ts>/` folder layout).

## R1 (Blocking) — New-file branch coverage below the 75% floor

- **File:** `QuickFiler/Viewers/BreadcrumbItemViewerLifecycleCoordinator.Search.cs`
- **Measured:** 100% line (5/5), **50% branch (2/4)** in `artifacts/csharp/coverage.xml` (= `evidence/qa-gates/coverage-final.cobertura.xml`). Uniform gate for new code files: line >= 85%, branch >= 75% (`.claude/rules/quality-tiers.md`).
- **Root cause:** lines 40 (`_openCoordinator?.LatchNextOpenTakesNoFocus();`) and 42 (`_bridgeCoordinator?.PresentSearchResults(items);`) each measure `50% (1/2)` — only the non-null arms execute. The file's XML remark documents the no-open-coordinator fallback behavior ("the bare fallback performs no `Focus(focus)` call at all"), but no test constructs the coordinator in that configuration.
- **Expected remediation (test-only; no production change expected):** add unit test(s) to `QuickFiler.Test/Viewers/BreadcrumbItemViewerLifecycleCoordinatorTests.cs` (or a new partial `...Tests.Part2.cs` with a csproj `<Compile Include>` entry and a one-token `partial` on the existing class if the base file's length requires it — it is currently 345 lines, so extension in place is fine) that:
  1. constructs `BreadcrumbItemViewerLifecycleCoordinator` in the configuration where `_openCoordinator` is null (the no-open-coordinator wiring already exercised by existing lifecycle tests for `SetDroppedDown`) and calls `PresentSearchResults`, asserting no throw and that the bridge coordinator (when present) receives the items;
  2. covers the `_bridgeCoordinator == null` arm (construct without a bridge coordinator), asserting `PresentSearchResults` is a deterministic no-op.
- **Verification commands:**
  - `vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /Settings:scripts/vscode/TaskMaster.cli.runsettings /InIsolation /TestCaseFilter:"FullyQualifiedName~BreadcrumbItemViewerLifecycleCoordinator"` — all pass, count > previous.
  - Regenerate the coverage artifact via `./scripts/vscode/Invoke-MSTestWithCoverage.ps1` and confirm `BreadcrumbItemViewerLifecycleCoordinator.Search.cs` reports branch coverage >= 75% (target: 4/4).
  - Full four-stage toolchain pass per CLAUDE.md § C# Toolchain, restarted from formatting after any file change.
- **Acceptance:** file-level branch coverage >= 75% for `BreadcrumbItemViewerLifecycleCoordinator.Search.cs`; no existing test modified; repo-wide figures not lower than 0.858665 line / 0.792502 branch.

## R2 (Non-blocking — disposition decision) — Modified file below the per-file floor, pre-existing

- **File:** `QuickFiler/Controllers/QfcItemController.EventHandlers.cs`
- **Measured:** 78.65% line / 61.11% branch (floor: 85%/75%). Baseline: 79.57%/65.00% — already sub-floor before this branch.
- **Verified attribution:** every changed line is covered (hits >= 1 across the rewritten `TextBoxSearch_TextChanged` region); the uncovered-line set is identical pre/post (19 lines both sides); the percentage decrease is denominator arithmetic from deleting 4 covered (defective) lines. The "no regression on changed lines" policy requirement is satisfied.
- **Requested disposition (maintainer or follow-up issue, not a #438 code change):** either (a) a follow-up issue to cover the remaining untested handlers in this file (theme/menu WinForms handlers), or (b) a recorded exemption rationale if those handlers fall under the CLAUDE.md § UT2 WinForms exemption analysis. Widening #438 to lift this legacy file to the floor would violate the bugfix minimal-fix boundary.

## Follow-ups recorded (no action inside this remediation cycle)

- **F1:** Promote the pre-existing `UtilitiesCS.Test.csproj` duplicate `PercentageFormatterTests.cs` compile entry (CS2002; merge-base lines 302/354) to its own issue via the promotion lifecycle — promised in `issue.md`, still outstanding.
- **F2:** Recurring PR-context classifier defect: `collect_pr_context` summary misclassified 30 changed C# files as documentation ("Core logic changes: 0 files"). Corrected in place this review; the generator defect itself should be tracked as a tooling issue (recurring since #171).
- **F3:** Pre-existing `WinFormsPumpHost` load-flaky tests (visible window, real message pump) — candidate for a test-infrastructure issue; not touched by #438.
- **F4:** HV-1 human verification per `runbooks/verify-search-focus-retention.runbook.md` post-merge; negative outcome is promoted as a new issue.

## Do-not-do list

- Do not modify any production file to satisfy R1; it is a test-coverage gap, not a production defect. If a production seam change proves genuinely necessary, stop and re-plan.
- Do not add `[ExcludeFromCodeCoverage]` or any coverage-tool exclusion to satisfy R1 or R2 (converting a threshold gap into an exclusion is prohibited).
- Do not weaken, remove, or relax any existing test or assertion.
- Do not touch the EfcViewer search path (spec AC-13), the suggestions path, or any explicit-gesture semantics.
- Do not exceed 500 lines in any file; `BreadcrumbDropDownHostTests.cs` is at 499 — extend via its Part2 partial only.
- Do not edit policy documents, `spec.md` acceptance criteria, or the #400 feature folder.
