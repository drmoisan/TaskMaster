# 2026-10-22-efc-qfc-folder-tree-selection-and-nesting (Spec)

- **Issue:** #1002
- **Parent (optional):** none
- **Owner:** drmoisan
- **Last Updated:** 2026-10-22T05-08
- **Status:** Draft
- **Version:** 0.1

## Context
Three defects in the QuickFiler folder-selection surfaces. (1) The EFC viewer does not support selecting a subfolder of a listed folder the way the QFC viewer now does after issue #997 (PR #998). (2) Pressing the EFC trash button creates a trash entry but does not select it. (3) In the QFC viewer, a subfolder that has its own subfolders shows its expand affordance (+/−) on the left instead of the right, and deeper levels are not indented beneath their visible parent.

Environment:
- OS/version: Windows 11, Outlook desktop (VSTO add-in), WebView2 breadcrumb surface
- Python version: not applicable (C# .NET Framework 4.8, JavaScript breadcrumb asset)
- Command/flags used: interactive use of the EFC and QFC viewers
- Data source or fixture: any mailbox whose folder tree has at least three nested levels

Impact / Severity:
- [ ] Blocker
- [ ] High
- [x] Medium
- [ ] Low


## Repro & Evidence
Steps to Reproduce:
1. Open the EFC viewer, open the folder suggestions, and try to drill into and select a subfolder of a suggested folder.
2. In the EFC viewer, press the trash button.
3. Open the QFC viewer, expand a suggested folder, then look at a child subfolder that itself has subfolders and expand it.

Expected:
1. EFC offers the same recursive subfolder expansion and selection that QFC gained in #997, and the selected subfolder becomes the filing destination.
2. After the trash button adds the trash entry, that entry is the selected entry.
3. In QFC, the expand affordance of every expandable subfolder row sits on the right of the row, matching the top-level rows, and each nested level is indented one step further than its visible parent.

Actual:
1. EFC does not offer recursive subfolder selection.
2. The trash entry is created but the selection stays where it was.
3. The expand affordance of a nested subfolder renders on the left, and nested levels share one indent.

Logs / Screenshots:
- [ ] Attached minimal logs or screenshot
- Snippet: reported by the maintainer on 2026-10-22; no log excerpt.


## Scope & Non-Goals
- In scope:
- Out of scope / non-goals:
- Explicitly excluded systems, integrations, or datasets:

## Root Cause Analysis
- The #997 fix (merge 59b888d38) added descendant support in `QuickFiler/Resources/FolderBreadcrumb.js`, `UtilitiesCS/OutlookObjects/Folder/FolderBreadcrumbBridgeRouter.Descendants.cs`, `BreadcrumbSelectionSession.Descendants.cs`, `QuickFiler/Viewers/BreadcrumbBridgeCoordinator.Descendants.cs`, and wiring in `QfcItemController.BreadcrumbWiring.cs` and `ItemViewer.Breadcrumb.cs`. The EFC path (`EfcViewer`, `EfcFormController`) is likely not wired to the descendant messages.
- Trash handling lives in `EfcFormController.Actions.cs` and `EfcDataModel.cs`.
- The subfolder toggle is appended after the label in `FolderBreadcrumb.js` (around the `subfolderElement.appendChild(toggle)` call); the left placement and flat indent are likely CSS or DOM-order issues in the descendant row rendering.


## Proposed Fix

### Design summary (what changes where):

### Boundaries and invariants to preserve:

### Dependencies or blocked work:

### Implementation strategy (what changes, not sequencing):
	
#### Files/modules to change:

#### Functions/classes/CLI commands impacted:

#### Data flow and validation changes:

#### Error handling and logging updates:

#### Rollback/feature-flag considerations (if applicable):

### Technical specifications (interfaces/contracts):

#### Inputs/outputs and formats:

#### Required configuration keys and defaults:

#### Backward-compatibility expectations:

#### Performance constraints (latency/throughput/memory):

## Assumptions, Constraints, Dependencies
- Assumptions (environment, data, access):
- Constraints (budget, performance, compatibility):
- External dependencies (services, libraries, releases):

## Data / API / Config Impact
- User-facing or API changes:
- Data or migration considerations:
- Logging/telemetry updates (if any):
- Compatibility notes (CLI flags, config schemas, versioning):

## Test Strategy
Seeded from issue:

- [ ] Unit coverage areas: EFC descendant wiring, trash selection, descendant row rendering order and depth indentation
- [ ] Integration scenario to retest: three-level folder tree in EFC and QFC
- [ ] Manual verification notes: affordance on the right at every level; indent increases per level

- Regression tests to add or update:
- Unit tests (pytest) for the fixed behavior and boundaries:
- Edge cases and negative scenarios (invalid inputs, missing data, boundary values):
- Error handling and logging verification:
- Coverage impact and targets for changed lines/modules:
- Toolchain commands to run (format → lint → type-check → test):
- Manual validation steps (if required):


## Acceptance Criteria
- [ ] Repro steps now produce the expected behavior in all documented environments.
- [ ] Regression test(s) added and passing (list file path and test name).
- [ ] Edge cases and invalid inputs are handled with correct errors or fallbacks.
- [ ] No unintended behavior changes outside the defined scope.
- [ ] Required logs/telemetry updated and validated (if applicable).
- [ ] Performance constraints met or explicitly waived with rationale.
- [ ] Full toolchain pass completed (format → lint → type-check → test).
- [ ] Docs/config references updated to match the new behavior.

## Risks & Mitigations
- Technical or operational risks:
- Mitigations and rollbacks:

## Rollout & Follow-up
- Release/rollout steps:
- Post-fix monitoring or clean-up tasks:
- Links: issue, PRs, related docs
