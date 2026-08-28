# quickfiler-search-box-loses-focus-on-dropdown-expand (Spec)

- **Issue:** #680
- **Parent (optional):** none
- **Owner:** drmoisan
- **Last Updated:** 2026-08-28T12-56
- **Status:** Draft
- **Version:** 0.1

## Context
In QuickFiler's folder search box, typing a character correctly auto-opens/expands the search results drop-down, but the search box then loses keyboard focus, so no further characters can be typed until the user manually closes the drop-down and refocuses the search box — making auto-open effectively unusable for multi-character searches.

Environment:
- OS/version: Windows (VSTO add-in host, Outlook desktop)
- Component: QuickFiler folder search box (`QuickFiler/Viewers/ItemViewer.FolderSearch.cs`, `QuickFiler/Viewers/BreadcrumbBridgeCoordinator.Search.cs`, `QuickFiler/Viewers/BreadcrumbDropDownHost.cs`)
- Data source or fixture: N/A (manual interactive repro)

Impact / Severity:
- [ ] Blocker
- [x] High
- [ ] Medium
- [ ] Low


## Repro & Evidence
Steps to Reproduce:
1. Launch Outlook with the TaskMaster VSTO add-in loaded and run QuickFiler against a mail item.
2. Click into (or navigate to) a QuickFiler folder search box.
3. Type a single character. The search results drop-down auto-opens/expands as expected.
4. Attempt to type a second character to continue/narrow the search.

Expected:
Typing should be able to continue uninterrupted while the search drop-down is open, letting the user type a full multi-character search term and see the results narrow live.

Actual:
After the first character opens the drop-down, the search box loses keyboard focus. Additional keystrokes are not received by the search box. The user must close the drop-down and click back into the search box to type again, which reopens the drop-down after only one more character — making it effectively impossible to enter a multi-character search term through normal typing.

Logs / Screenshots:
- [ ] Attached minimal logs or screenshot
- Snippet: N/A — behavioral repro, no exception/log signature identified yet.


## Scope & Non-Goals
- In scope:
- Out of scope / non-goals:
- Explicitly excluded systems, integrations, or datasets:

## Root Cause Analysis
Likely in the same breadcrumb/search drop-down family as issue #677 (QuickFiler keyboard focus leaking away from an intended control while a `BreadcrumbDropDownHost`-hosted popup is open/closing) and possibly related to previously-archived issue **#438** ("quickfiler-search-keystroke-focus-steal", `docs/features/archive/2026-08-07-quickfiler-search-keystroke-focus-steal-438/`), which addressed per-keystroke close/reopen churn in this same search-to-dropdown pipeline. This may be a regression of the #438 fix, or a related-but-uncovered edge case (e.g. specifically the auto-open-on-first-character path) that #438's fix did not fully cover — the research phase for this bug should explicitly check the #438 fix's scope/tests against this exact repro before proposing a new fix, to avoid re-solving already-solved ground or reverting the prior fix.

Files likely relevant (not yet confirmed as root cause): `QuickFiler/Viewers/ItemViewer.FolderSearch.cs`, `QuickFiler/Viewers/BreadcrumbBridgeCoordinator.Search.cs`, `QuickFiler/Viewers/BreadcrumbDropDownHost.cs`, `QuickFiler/Controllers/QfcItemController.EventWiring.cs`/`.EventHandlers.cs`/`.Navigation.cs`.


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

- [ ] Unit coverage areas: search-box keystroke handling while the results drop-down is open/auto-opening; focus retention/restoration on drop-down open.
- [ ] Integration scenario to retest: type a multi-character search term (3+ chars) continuously without manual refocus; also retest the #438 acceptance criteria to confirm no regression.
- [ ] Manual verification notes: confirm the drop-down still narrows/updates live as characters are typed, and that Escape/commit/selection behavior from #438 is unaffected.

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
