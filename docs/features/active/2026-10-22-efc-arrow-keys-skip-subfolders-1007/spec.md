# efc-arrow-keys-skip-subfolders (Spec)

- **Issue:** #1007
- **Parent (optional):** none
- **Owner:** drmoisan
- **Last Updated:** 2026-10-22T13-03
- **Status:** Draft
- **Version:** 0.1

## Context
In the EFC viewer's folder tree, expanded subfolder rows can be selected with the mouse but not with the Up and Down arrow keys. Keyboard navigation does not move the selection onto any subfolder. This is a gap in the fix delivered for issue #1002 (PR #1003).

Environment:
- OS/version: Windows 11, Outlook desktop (VSTO add-in), QuickFiler EFC viewer (WebView2 breadcrumb tree)
- Python version: not applicable (C# .NET Framework 4.8 and JavaScript in `QuickFiler/Resources/FolderBreadcrumb.js`)
- Command/flags used: interactive use of the EFC viewer in Outlook
- Data source or fixture: any mail folder hierarchy with nested subfolders

Impact / Severity:
- [ ] Blocker
- [x] High
- [ ] Medium
- [ ] Low


## Repro & Evidence
Steps to Reproduce:
1. Open the EFC viewer on a mail item.
2. Expand a folder row so that its subfolder rows are shown.
3. Click a subfolder row: it is selected, as expected after #1002.
4. Press the Up or Down arrow key to move through the list.

Expected:
The arrow keys move the selection through the visible rows, including expanded subfolder rows, the same way mouse selection reaches them.

Actual:
The arrow keys do not select any subfolder row. Keyboard navigation reaches only the top-level rows.

Logs / Screenshots:
- [ ] Attached minimal logs or screenshot
- Snippet: reported by the maintainer from interactive use; no log captured.


## Scope & Non-Goals
- In scope:
- Out of scope / non-goals:
- Explicitly excluded systems, integrations, or datasets:

## Root Cause Analysis
- #1002 ported the QFC #997 subfolder selection to EFC for mouse clicks. The arrow-key path (`QuickFiler/Controllers/BreadcrumbBridgeRouter.Arrows.cs`, the EFC breadcrumb wiring in `EfcFormController.Breadcrumb.cs`, and the key handling in `QuickFiler/Resources/FolderBreadcrumb.js`) appears to iterate only the top-level rows and not the expanded descendant rows.
- Compare with how QFC handles arrow keys over expanded subfolder rows after #997.
- The exact expected keyboard behaviour is being settled in a grill-me round and will be recorded as acceptance criteria.


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

- [ ] Unit coverage areas: arrow navigation over a row list that contains expanded descendant rows (C# router tests and node tests under `tests/quickfiler-breadcrumb/`)
- [ ] Integration scenario to retest: Up and Down across parent, nested subfolder, and next top-level rows
- [ ] Manual verification notes: confirm in Outlook that the arrow keys select subfolder rows in the EFC viewer

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
