# Remediation Inputs: Issue #1007 (review 2026-10-25T23-25)

Review-Verdict: REMEDIATION_REQUIRED

Remediation-Mode: proportionate

- Feature folder: `docs/features/active/2026-10-22-efc-arrow-keys-skip-subfolders-1007`
- Branch head reviewed: `8bdd970ee`; merge base `3b9a1ebdd`
- Source audit artifacts:
  - `docs/features/active/2026-10-22-efc-arrow-keys-skip-subfolders-1007/policy-audit.2026-10-25T23-25.md`
  - `docs/features/active/2026-10-22-efc-arrow-keys-skip-subfolders-1007/code-review.2026-10-25T23-25.md`
  - `docs/features/active/2026-10-22-efc-arrow-keys-skip-subfolders-1007/feature-audit.2026-10-25T23-25.md`
- Next link: `atomic-planner` authors the remediation plan per `remediation-handoff-atomic-planner`; this review does not author the plan.
- Mode rationale: one stylesheet rule in an embedded page resource plus one string-contract test; no C# production logic, acceptance criterion or design decision changes.

## Remediation-Required Findings

### B-1 The QFC pending highlight on a descendant has no stylesheet rule and is not visible

- Severity: Blocking
Remediability: autonomous
Remediability-Evidence: the fix is one CSS rule in a repository resource file and one string-contract test; the existing `.sub.selected` and `.row.active` rules already define the highlight colours, so no maintainer choice or external system is involved.
- Files: `QuickFiler/Resources/FolderBreadcrumb.html` (stylesheet); a C# string-contract test in `QuickFiler.Test/Viewers/FolderBreadcrumbKeysIssue1007Tests.cs` (158 lines) or `QuickFiler.Test/Viewers/FolderBreadcrumbAssetContractTests.cs` (429 lines); optionally `tests/quickfiler-breadcrumb/issue-1007-keys.test.mjs` (136 lines).
- Problem: `FolderBreadcrumb.js` `makeSubfolders` (line 169) gives a pending descendant that is not the committed subfolder the class `"sub active"`, and `render` (lines 243 to 246) passes `pendingNodeOwnsActive` into `makeRow`, which then omits the row's `active` class. `FolderBreadcrumb.html` styles `.row.selected`, `.row.active` (lines 109 to 112) and `.sub.selected` (line 158) only. With the QFC list open, Down from an expanded row or Right on an expanded entry moves the pending highlight onto a subfolder, and the list then shows no highlighted element. AC4 is evaluated PARTIAL and its box is unchecked in `spec.md` and `issue.md`.
- Expected state after the fix: a pending descendant (`.sub.active`) is drawn with the same selection background and foreground as `.sub.selected` (or a documented pending style with equivalent contrast in the light and dark themes); a C# test asserts that the QFC document stylesheet contains a rule matching `.sub.active`; AC4 is checked off again in `spec.md` and the `issue.md` mirror.
- Fix:
  - In `QuickFiler/Resources/FolderBreadcrumb.html`, change `.sub.selected { ... }` to `.sub.selected, .sub.active { ... }` (or add a separate `.sub.active` rule using `var(--bc-sel-back)` and `var(--bc-sel-fore)`).
  - Add a C# MSTest + FluentAssertions string-contract test over the QFC document HTML (the same source the existing asset-contract tests read) asserting the `.sub.active` selector is present in the stylesheet; keep the test file at or under 500 lines.
  - Optionally extend the node test "pending node carries sub active" to assert the stylesheet rule through the harness, if the harness loads the HTML stylesheet.
  - Run the full C# toolchain in CLAUDE.md order (CSharpier format and check, analyzer `/t:Rebuild`, warnings-as-errors `/t:Rebuild`, `scripts/vscode/Invoke-MSTestWithCoverage.ps1`), restarting from format if any step changes files or fails, and run `node --test` over `tests/quickfiler-breadcrumb/`.
  - Record the fail-before (the new contract test fails against the current HTML) and pass-after projections under `evidence/regression-testing/`, then check off AC4 in `spec.md` and mirror it in `issue.md`.
- Verification commands:
  - Grep `QuickFiler/Resources/FolderBreadcrumb.html` for `\.sub\.active` and confirm one match inside the `<style>` block.
  - `vstest.console.exe QuickFiler.Test.dll /Settings:scripts/vscode/TaskMaster.cli.runsettings /InIsolation "/TestCaseFilter:FullyQualifiedName~FolderBreadcrumbKeysIssue1007Tests|FullyQualifiedName~FolderBreadcrumbAssetContractTests"` passes.
  - `node --test --test-concurrency=1 tests/quickfiler-breadcrumb/` reports 0 failures.

## Non-Blocking Items

Recommended in the same cycle because they touch only test files of this item:

- N-1: restore direct tests for unchanged paths that lost their only test execution: EFC `leafExpandToggle` on a row collapsed away from its active segment (`BreadcrumbBridgeRouter.Arrows.cs` lines 24 to 30); QFC `affordanceToggle` on a non-anchored collapsed row (`FolderBreadcrumbBridgeRouter.Descendants.cs` lines 70 to 72) and `BreadcrumbStateModel.Expand` non-anchored branch; a coordinator test that a router `unhandledArrow` output raises `UnhandledArrow` (`BreadcrumbBridgeCoordinator.cs` lines 321 to 324). Delete any branch shown to be unreachable instead.
- N-2: rename the six rewritten #440 tests whose names contradict their assertions (list in the code review).

Informational only:

- N-3: `FolderBreadcrumbBridgeRouter.Navigation.cs` branch coverage 78.33% (line 93.44%).
- N-4: `BreadcrumbSelectionSession.cs` grew from 446 to 492 lines; keep further session logic in the Tree partial.
- N-5: AC14 defence-in-depth edge after a zero-children fetch followed by a Left collapse (code review).
- O-1: regenerate `artifacts/pr_context.summary.txt` and `.appendix.txt` before the PR is authored.
- O-2: TaskMaster CI checks have not run (no PR yet); they remain a PR-time gate.
- O-3: the manual Outlook verification is a human step recorded by the orchestrator.

## Do Not Do

- Do not change the keyboard state machine, the visible-order helper or any C# production file for B-1; the defect is in the stylesheet.
- Do not add `[ExcludeFromCodeCoverage]`, `coverage.config` entries, `[DoNotParallelize]`, sleeps, delays, retries or temporary files.
- Do not edit or add acceptance-criteria text in `spec.md`; only re-check AC4 after verification.
- Do not commit raw `.trx`, `.coverage` or Cobertura documents; evidence stays as Markdown projections under `evidence/`.
- Do not edit policy documents under `.claude/rules/` or `.github/instructions/`, or any file under `.claude/`.
