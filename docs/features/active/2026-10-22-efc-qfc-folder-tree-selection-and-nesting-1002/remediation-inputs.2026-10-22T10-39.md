# Remediation Inputs: Issue #1002 (review 2026-10-22T10-39)

Review-Verdict: REMEDIATION_REQUIRED

Remediation-Mode: proportionate

- Feature folder: `docs/features/active/2026-10-22-efc-qfc-folder-tree-selection-and-nesting-1002`
- Branch head reviewed: `e96e65302de00d3b0f8174736c90a59f82525dc9`; merge base `8e4ad1ecbf50aae00fe2c092c22ae457f4feb7a7`
- Source audit artifacts:
  - `docs/features/active/2026-10-22-efc-qfc-folder-tree-selection-and-nesting-1002/policy-audit.2026-10-22T10-39.md`
  - `docs/features/active/2026-10-22-efc-qfc-folder-tree-selection-and-nesting-1002/code-review.2026-10-22T10-39.md`
  - `docs/features/active/2026-10-22-efc-qfc-folder-tree-selection-and-nesting-1002/feature-audit.2026-10-22T10-39.md`
- Next link: `atomic-planner` authors the remediation plan per `remediation-handoff-atomic-planner`; this review does not author the plan.
- Mode rationale: every autonomous finding is fixed inside files already in the branch diff plus their tests (`BreadcrumbDocumentAssets.cs`, `BreadcrumbMessages.cs`, `EfcFormController.Breadcrumb.cs`, new or existing Issue1002 test files); no acceptance criterion, public interface or file inventory change is needed.

## Remediation-Required Findings

### B-1 EfcFormController.Breadcrumb.cs coverage lowered by the change

- Severity: Blocking
Remediability: autonomous
Remediability-Evidence: the lost lines are internal controller members reachable from existing test fixtures (strict-mock router, controller harness in `EfcFormControllerTests`); the fix is additional unit tests in the branch's own Issue1002 test partial.
- File: `QuickFiler/Controllers/EfcFormController.Breadcrumb.cs`, members `BindFolderRows` and `BindSourceFolderRows`.
- Problem: whole-file coverage fell from 67.83% line (78/115) / 57.14% branch (24/42) at the merge base to 64.46% line (78/121) / 52.38% branch (22/42) at head, below the CLAUDE.md 80% line / 75% branch floor. The delete gesture previously reached `BindFolderRows` (6 of 7 lines covered); it now awaits `BindBreadcrumbRowsAsync` directly, and the remaining caller `BindSourceFolderRows` has no test.
- Expected behaviour after the fix: `BindFolderRows` and `BindSourceFolderRows` are covered on their null-viewer, null-router and bind paths, and the file's covered lines and covered branches are at least the baseline counts (78 lines, 24 branches) with the higher valid-line count.
- Fix: add tests (in `QuickFiler.Test/Controllers/EfcFormControllerTests.Issue1002.cs` or a new Issue1002 partial, keeping every file at or under 500 lines) that drive `BindSourceFolderRows` (for example through `RefreshSuggestionsAsync` with a stubbed data model, or directly as an internal member) and assert `_folderRows` retention and the router bind. No production change is required.
- Verification commands:
  - Toolchain in CLAUDE.md order, ending with `scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot .`.
  - Per-file union of Cobertura class line nodes for `QuickFiler\Controllers\EfcFormController.Breadcrumb.cs`: covered lines at least 84 of 121 and covered branches at least 24 of 42 (restores the baseline plus the six `BindFolderRows` lines); record in a new `evidence/qa-gates/coverage-comparison.<timestamp>.md`.

### B-2 BreadcrumbMessages.cs branch coverage below the floor

- Severity: Blocking
Remediability: autonomous
Remediability-Evidence: the uncovered arms are argument-null guards in public constructors; each is reached by a one-line unit test with no seam or external dependency.
- File: `UtilitiesCS/OutlookObjects/Folder/BreadcrumbMessages.cs`, constructor lines 84, 85, 124, 144, 173, 174 and 175.
- Problem: branch coverage 50.00% (7/14) at baseline and head, below the 75% branch floor for a modified file; lines 84-85 belong to the `BreadcrumbInboundMessage` constructor this diff modified.
- Expected behaviour after the fix: each `?? throw new ArgumentNullException(...)` arm is exercised; file branch coverage at least 75% (11 of 14 or more).
- Fix: add null-argument tests asserting `ArgumentNullException` with the parameter name for `BreadcrumbInboundMessage` (`type`, `rowId`) and for the guarded parameters of the three outbound message constructors, in a new Issue1002 test file under `UtilitiesCS.Test/OutlookObjects/Folder/` with its `Compile Include`, or in `BreadcrumbMessageCodecIssue1002Tests.cs` (108 lines).
- Verification commands:
  - Toolchain in CLAUDE.md order.
  - Cobertura `condition-coverage` on lines 84, 85, 124, 144, 173, 174 and 175 of `UtilitiesCS\OutlookObjects\Folder\BreadcrumbMessages.cs`: 100% (2/2) on each.

### B-3 Pre-existing sub-floor residue in EfcFormController partials

- Severity: Blocking
Remediability: human_decision_required
Remediability-Evidence: CLAUDE.md UT2 states the COM/VSTO/WinForms coverage exemption must be ratified by the project maintainer; the residue lies in MessageBox-bound action handlers and WebView2 host wiring, so the choice between ratifying that exemption and authorizing new seams outside this bug's scope is the maintainer's.
- Files: `QuickFiler/Controllers/EfcFormController.Actions.cs` (23.42% line, 33.33% branch; baseline 21.30% / 28.12%) and the residue of `QuickFiler/Controllers/EfcFormController.Breadcrumb.cs` remaining after B-1 (its `ConfigureBreadcrumbControl` wiring, 25 lines, caps the file at 79.3% line without a host seam).
- Problem: both files remain below the 80% line / 75% branch modified-file floor. Uncovered members: `ActionOkAsync` 0/28 lines, `CreateFolderAsync` 0/41, `ActionCancelAsync` 0/5, `RefreshSuggestionsAsync` 0/8 plus two lambdas, `ConfigureBreadcrumbControl` 0/22 plus three lambdas. None of these lines is changed by this branch, and this branch raised `EfcFormController.Actions.cs`.
- Decision required: (a) ratify the exemption for these members through `[ExcludeFromCodeCoverage]` or `coverage.config` as CLAUDE.md UT2 permits, or (b) authorize a follow-up issue for a message-box sink and a breadcrumb host factory seam with tests, or (c) accept the residue as recorded pre-existing debt for this item.
- Not for the atomic planner in this cycle: no plan task should add seams to these members without the decision.

### B-4 Selected EFC descendant has no visual highlight

- Severity: Blocking
Remediability: autonomous
Remediability-Evidence: the fix is one attribute-selector rule per theme block in a string constant already in the branch diff, plus a text-contract test; AC9's attribute-not-class decision is preserved.
- File: `UtilitiesCS/OutlookObjects/Folder/BreadcrumbDocumentAssets.cs` (`LightThemeCss`, `DarkThemeCss`).
- Problem: `BreadcrumbHtmlRenderer.AppendDescendants` marks the selected descendant with `aria-selected="true"`, but no CSS rule targets that attribute, and `.rowwrap.selected .row` styles only the row's crumb line, not the `.children` container. After activating a grandchild, the row shows its own path highlighted while the filing destination is unmarked; manual check step 3 ("confirm it is highlighted") cannot pass.
- Expected behaviour after the fix: a selected descendant `.child` element renders with the same selection background as a selected row in each theme (`#cce8ff` light, `#264f78` dark).
- Fix: add `.child[aria-selected="true"] { background: #cce8ff; }` to `LightThemeCss` and `.child[aria-selected="true"] { background: #264f78; }` to `DarkThemeCss` (or an equivalent single rule using the theme colour), and a C# text-contract test asserting both rules, in `UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbHtmlRendererIssue1002Tests.cs` (297 lines) or a new Issue1002 test file.
- Verification commands:
  - Toolchain in CLAUDE.md order.
  - Grep of `UtilitiesCS/OutlookObjects/Folder/BreadcrumbDocumentAssets.cs` for `aria-selected`: at least two hits.
  - Update `evidence/other/manual-outlook-check.<timestamp>.md` only if its steps change; the check remains pending human interaction.

## Recommended In-Scope Items (non-blocking)

- CR-1: `.child` became a flex container, so `text-overflow: ellipsis` no longer truncates long names; move `min-width: 0; overflow: hidden; text-overflow: ellipsis; white-space: nowrap;` to `.child .name` in `BaseCss`. Same file as B-4.
- CR-4: one direct test of `ApplyDeleteGestureAsync` with a null viewer covers the remaining arm on line 102 of `EfcFormController.Actions.cs`.
- CR-2, CR-3, CR-5 (transient minus affordance during a fetch, nested `data-child-index`, Error-level logging of expected races): no change required in this cycle.

## Non-Blocking Items

- O-1: the PR-context summary classifies every changed C# file as docs/tooling; language scope was derived from `git diff`.
- O-2: skill layout conflicts (flat review-artifact names versus `audit/<ts>/` folders; plan authorship) are recorded in the policy audit.
- O-3: the manual Outlook check stays pending human interaction by design (AC26).

## Do Not Do

- Do not change acceptance-criteria text, uncheck criteria, or add criteria.
- Do not add `ExcludeFromCodeCoverage` or `coverage.config` entries, or seams in `ActionOkAsync`, `CreateFolderAsync` or `ConfigureBreadcrumbControl`, without the B-3 decision.
- Do not edit files in the spec's excluded list (QFC router, selection session, coordinator, render projection, wiring, `BreadcrumbDescendantNode.cs`, `FolderBreadcrumb.html`, `Resources.resx`, `EfcDataModel.cs`).
- Do not extend the near-limit test files listed in the spec Constraints; keep every file at or under 500 lines.
- Do not add `ConfigureAwait(false)` or locks to `QuickFiler/Controllers/BreadcrumbBridgeRouter*.cs`.
- Do not edit policy documents under `.claude/rules/` or `.github/instructions/`.
- Do not commit raw `.trx`, `.coverage` or Cobertura documents; evidence stays as Markdown projections under `evidence/`.
