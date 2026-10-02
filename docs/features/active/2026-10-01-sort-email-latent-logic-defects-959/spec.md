# 2026-10-01-sort-email-latent-logic-defects (Spec)

- **Issue:** #959
- **Parent (optional):** none
- **Owner:** drmoisan
- **Last Updated:** 2026-10-02T05-07
- **Status:** Draft
- **Version:** 0.1

## Context
The #956 preparation research found four logic defects in `UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.cs`. #956 is a structural split plus testability seams, so it neither fixes nor depends on them:
- **L1:** `SaveCase` switch cases combine enum flags, so neither case can ever match.
- **L2:** with a sticky "Yes to all" answer, a save that keeps failing on access denied retries forever. This path is live in production through `EmailFiler`.
- **L3:** `Cleanup_Files` never resets `_attachmentsAltName`.
- **L4:** `WriteCSV_StartNewFileIfDoesNotExist` passes its `Path.Combine` arguments in reverse order, and its condition is inverted.

Environment:
- OS/version: Windows 11 (Outlook VSTO add-in)
- Python version: n/a (C#, .NET Framework 4.8)
- Command/flags used: static reading of `SortEmail.cs`
- Data source or fixture: n/a

Impact / Severity:
- [ ] Blocker
- [x] High
- [ ] Medium
- [ ] Low

L2 can hang a production filing operation. The other three are latent.


## Repro & Evidence
Steps to Reproduce:
1. Read the `SaveCase` switch, the `TrySaveAttachmentAsync` retry path, `Cleanup_Files`, and `WriteCSV_StartNewFileIfDoesNotExist` in `SortEmail.cs`.
2. Compare each one with its intended behavior as described above.

Expected:
- Switch cases match the intended flag combinations.
- A persistent access-denied failure ends the retries and surfaces the error.
- Cleanup resets all prompt state.
- The CSV helper creates the file at the correct path, and only when it is absent.

Actual:
As described in the summary. None of the four has been reproduced at runtime yet.

Logs / Screenshots:
- [ ] Attached minimal logs or screenshot
- Snippet: #956 research (`docs/features/active/2026-10-01-sort-email-oversized-with-untestable-io-and-dialog-paths-956/research/`), findings L1 to L4.


## Scope & Non-Goals
- In scope:
- Out of scope / non-goals:
- Explicitly excluded systems, integrations, or datasets:

## Root Cause Analysis
These are legacy code paths with no test coverage, partly because of the `[ExcludeFromCodeCoverage]` and dialog dependencies that #956 removes. Sequence this after #956 merges, so that each defect can get a regression test through #956's new seams.


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

- [ ] For each of L1 to L4, write a failing regression test first, using the #956 prompt and file-system seams, then apply the minimal fix.
- [ ] For L2, bound the retry, or stop retrying on a persistent `UnauthorizedAccessException`, and surface the error.

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
