# 2026-09-08-assignfoldercombobox-unguarded-archiverootpath-read (Spec)

- **Issue:** #813
- **Parent (optional):** none
- **Owner:** drmoisan
- **Last Updated:** 2026-09-08T23-49
- **Status:** Draft
- **Version:** 0.1

## Context
`QfcItemController.AssignFolderComboBox` reads `_globals.Ol?.ArchiveRootPath` at
`QuickFiler/Controllers/QfcItemController.FolderHandling.cs:233` with no `try`, and the method is
reached from the UI dispatcher. The null-conditional operator guards a null `Ol`; it does not guard
an `ArchiveRootPath` getter that throws when no archive root is configured. Guarding
`FolderPredictor` alone therefore does not prevent the failure at this call site.

Environment:
- OS/version: Windows 11 Pro 10.0.26200, .NET Framework 4.8 VSTO add-in hosted by Outlook
- Python version: not applicable; this is C# in `QuickFiler`
- Command/flags used: not a command-line defect; reached through the QuickFiler item pane
- Data source or fixture: a profile whose Archive Root is unset, which is the state reported in
  issue 797 (`Archive Root` -> `Outlook` and `File System` both showing "Please select an archive")

Impact / Severity:
- [ ] Blocker
- [ ] High
- [x] Medium
- [ ] Low

Medium: it is a user-visible failure on a supported configuration (archive root unset), and it is
the reason issue 812's AC1 outcome can be verified at unit level but not end to end in QuickFiler.


## Repro & Evidence
Steps to Reproduce:
1. Open a profile whose Archive Root has never been set, so `ArchiveRootPath` has no configured value.
2. Open QuickFiler on a mail item so the folder combo box is populated.
3. Observe the folder-handling path reach `AssignFolderComboBox` with `_folderHandler.FolderArray`
   non-empty.

Expected:
An unset archive root degrades the suggestion display — the predetermined folder is simply not
preselected — and QuickFiler continues to operate.

Actual:
The read at `QuickFiler/Controllers/QfcItemController.FolderHandling.cs:233` propagates the getter's
exception out of `AssignFolderComboBox`. Because line 188 of the same file invokes the method through
`_itemViewer.UiDispatcher.InvokeAsync(AssignFolderComboBox)`, the exception surfaces on the UI
dispatcher rather than at a handled boundary.

`AssignFolderComboBox` begins at line 191 and contains no `try` block; the read sits six lines after
`_itemViewer.SetFolderSuggestions(_folderHandler.FolderRowArray)` consumes `FolderRowArray`.

Logs / Screenshots:
- [ ] Attached minimal logs or screenshot
- Snippet: none captured; the finding is from static reading of the call path, not from a runtime
  trace. A runtime trace should be captured as part of the fix.


## Scope & Non-Goals
- In scope:
  - The single unguarded read at `QuickFiler/Controllers/QfcItemController.FolderHandling.cs` lines
    231-234, inside `AssignFolderComboBox`: wrap `_globals?.Ol?.ArchiveRootPath ?? string.Empty` in a
    narrow `catch (InvalidOperationException)` that substitutes `string.Empty` on catch, so the
    method continues past the preselection step exactly as it does today when `_globals` is null.
  - One regression test added to `QuickFiler.Test/Controllers/QfcItemController.FolderHandlingTests.Part2.cs`
    (or a new `...Part3.cs` file if the 500-line file-size cap is at risk — the executor must check
    the current line count of `Part2.cs` before appending and split into a new part file if needed).
- Out of scope / non-goals:
  - Issue #812's getter-hardening work. `TaskMaster/AppGlobals/AppOlObjects.cs`,
    `TaskMaster/AppGlobals/AppOlObjects.ArchiveRoot.cs`, and `TaskMaster/AppGlobals/ArchiveRootPathGuard.cs`
    must not be modified.
  - `ProjectPredeterminedFolder` and `UtilitiesCS/OutlookObjects/Folder/ArchiveStemProjection.ToDisplayStem`
    need no change — research confirmed both already treat a null or empty archive root as an
    identity projection (`return folderPath`).
  - No other methods on `QfcItemController` are in scope.
- Explicitly excluded systems, integrations, or datasets:
  - Sibling-feature files belonging to other in-flight epic children sharing the same integration
    branch: `QfcHomeController.cs`, `ProgressViewer.cs`, `StoreWrapperController*.cs`,
    `BreadcrumbPopupOwnerRegistry.cs`, `BreadcrumbDropDownHost.Open.cs`, the SDIL reader files,
    `OlTableExtensions.*`, `TimeOutTask.cs`, `DfDeedle.cs`, `.editorconfig`, `BannedSymbols.txt`, and
    `FolderPredictorTests.cs`. None of these files are to be touched by this fix.

## Root Cause Analysis
Found by the preparation child for issue 812 while establishing that item's acceptance criteria, and
independently verified against the tree on 2026-09-08. It is outside 812's frozen write set, so it
could not be fixed or filed from that item's branch without breaching the footprint constraint.

Sequence with 812: 812 hardens the archive-root read path itself. This call site needs its own
handling regardless, because a guarded provider does not make a throwing property read safe at an
unguarded consumer.


## Proposed Fix

### Design summary (what changes where):
In `AssignFolderComboBox`, replace the single unguarded expression
`_globals?.Ol?.ArchiveRootPath ?? string.Empty` (lines 231-234) with the same expression wrapped in
a `try`/`catch (InvalidOperationException)` that substitutes `string.Empty` in the catch block. This
is Option A from the research record (`research/research.2026-09-08T23-58.md` §5): the smaller diff,
contained to the one call site the issue names. No other production code changes.

### Boundaries and invariants to preserve:
- The rest of `AssignFolderComboBox` (population at lines 206-222, preselect-by-name vs.
  index-fallback branching at lines 235-247, and the `_selectedFolder = _itemViewer.GetSelectedFolder()`
  assignment at line 248) is unchanged.
- Catch only `InvalidOperationException` — never a broad `catch (Exception)`. This matches the sole
  documented and reachable exception type from `IOlObjects.ArchiveRootPath`'s concrete implementation
  (`AppOlObjects.cs`, confirmed in the research record §3).
- `#812`'s files (`AppOlObjects.cs`, `AppOlObjects.ArchiveRoot.cs`, `ArchiveRootPathGuard.cs`) remain
  untouched.
- `ProjectPredeterminedFolder` / `ArchiveStemProjection.ToDisplayStem` remain untouched; substituting
  `string.Empty` on catch is behaviorally equivalent to the existing null-`_globals` path through the
  same call, per the research record §2.

### Dependencies or blocked work:
None. This fix is self-contained to one file and does not depend on #812 landing first or after.

### Implementation strategy (what changes, not sequencing):

#### Files/modules to change:
- `QuickFiler/Controllers/QfcItemController.FolderHandling.cs` (production fix, lines 231-234).
- `QuickFiler.Test/Controllers/QfcItemController.FolderHandlingTests.Part2.cs` (new regression test),
  or a new `QfcItemController.FolderHandlingTests.Part3.cs` if appending would push `Part2.cs` over
  the 500-line file-size limit — check the current line count first.

#### Functions/classes/CLI commands impacted:
- `QfcItemController.AssignFolderComboBox` (the only function changed).

#### Data flow and validation changes:
None. The `archiveRootPath` value fed to `ProjectPredeterminedFolder` is unchanged in shape (a
`string`); only the failure path now produces `string.Empty` instead of propagating an exception.

#### Error handling and logging updates:
Add a narrow `catch (InvalidOperationException)` around the archive-root read only, substituting
`string.Empty`. This degrades to "no preselection" (the documented expected behavior) rather than
silently swallowing unrelated failures — no other exception types are caught, and no new logging is
introduced by this narrow fix; the existing add-in logging pattern already covers unhandled
dispatcher exceptions, which this change prevents from reaching that boundary in the first place.

#### Rollback/feature-flag considerations (if applicable):
None. This is a narrow defect fix with no config surface or feature flag; rollback is a plain
revert of the one-file change.

### Technical specifications (interfaces/contracts):

#### Inputs/outputs and formats:
No change to `AssignFolderComboBox`'s signature or to `IOlObjects.ArchiveRootPath`'s contract.

#### Required configuration keys and defaults:
None.

#### Backward-compatibility expectations:
Fully backward compatible — the only observable behavior change is that a previously-uncaught
exception on an unset/unresolvable archive root no longer propagates out of `AssignFolderComboBox`;
the method now degrades to the index-fallback selection path instead.

#### Performance constraints (latency/throughput/memory):
Not applicable; a `try`/`catch` around a single property read has no measurable performance impact.

## Assumptions, Constraints, Dependencies
- Assumptions (environment, data, access):
  - `InvalidOperationException` remains the sole exception type thrown by the concrete
    `IOlObjects.ArchiveRootPath` implementation, per the research record's confirmation of the
    contract in `AppOlObjects.cs`.
  - The existing Moq-based test doubles for `IApplicationGlobals`/`IOlObjects` in
    `QuickFiler.Test/Controllers/` remain usable without modification for the new test.
- Constraints (budget, performance, compatibility):
  - Change is confined to `QuickFiler/Controllers/QfcItemController.FolderHandling.cs` and its test
    file; #812's frozen write set (`AppOlObjects.cs`, `AppOlObjects.ArchiveRoot.cs`,
    `ArchiveRootPathGuard.cs`) and sibling epic-child files listed under Scope & Non-Goals must not
    be touched.
  - `QfcItemController.FolderHandlingTests.Part2.cs` must stay under the repository's 500-line file
    cap; if adding the new test would exceed it, create `...Part3.cs` instead.
- External dependencies (services, libraries, releases):
  - None beyond the repository's existing MSTest/Moq/FluentAssertions toolchain already in use in
    this test family.

## Data / API / Config Impact
- User-facing or API changes:
  - None to public APIs. The user-visible effect is behavioral: QuickFiler no longer surfaces an
    unhandled dispatcher exception when the archive root is unset; the folder combo box populates
    and simply has no preselection.
- Data or migration considerations:
  - None.
- Logging/telemetry updates (if any):
  - None required by this fix; no new logging is introduced (see Error handling and logging updates
    above).
- Compatibility notes (CLI flags, config schemas, versioning):
  - Not applicable; no CLI flags, config schemas, or versioned contracts are touched.

## Test Strategy
Seeded from issue:

- [ ] Unit coverage areas: a `QfcItemController` folder-handling test in which the globals stub's
      `ArchiveRootPath` getter throws, asserting `AssignFolderComboBox` completes and leaves the
      combo box populated without a preselection.
- [ ] Integration scenario to retest: QuickFiler item pane on a profile with no archive root set.
- [ ] Manual verification notes: confirm the folder combo box still populates and the add-in log
      records the degraded path rather than an unhandled dispatcher exception.

- Regression tests to add or update:
- Unit tests (pytest) for the fixed behavior and boundaries:
- Edge cases and negative scenarios (invalid inputs, missing data, boundary values):
- Error handling and logging verification:
- Coverage impact and targets for changed lines/modules:
- Toolchain commands to run (format → lint → type-check → test):
- Manual validation steps (if required):


## Acceptance Criteria
- [ ] A regression test exists in `QuickFiler.Test/Controllers/QfcItemController.FolderHandlingTests.Part2.cs`
      (or a new `...Part3.cs` if the file-size cap requires it) that stubs
      `IApplicationGlobals.Ol.ArchiveRootPath` to throw `InvalidOperationException` and asserts
      `AssignFolderComboBox()` completes without throwing.
- [ ] The same test asserts the folder combo box and suggestion rows are still populated
      (`AddFolderItems` and `SetFolderSuggestions` were invoked) despite the archive-root read
      failing.
- [ ] The same test asserts no preselection occurs (`SetFolderSelectedItem` is never called) and that
      the index-fallback path (`SetFolderSelectedIndex`) runs instead.
- [ ] The fix in `QuickFiler/Controllers/QfcItemController.FolderHandling.cs` catches only
      `InvalidOperationException`, not a broader exception type.
- [ ] No files owned by issue #812 (`AppOlObjects.cs`, `AppOlObjects.ArchiveRoot.cs`,
      `ArchiveRootPathGuard.cs`) or by sibling epic features (listed under Scope & Non-Goals) are
      modified.
- [ ] Full C# toolchain passes with no regression: CSharpier format check, `.NET` analyzer rebuild
      (`/p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`), nullable rebuild
      (`/p:TreatWarningsAsErrors=true`), and MSTest execution via `vstest.console.exe`.

## Risks & Mitigations
- Technical or operational risks:
  - There may be other call sites that read `Ol.ArchiveRootPath` (or similar throwing getters)
    without a guard; this fix does not search for or remediate any such sites beyond the one the
    issue names.
  - Appending the new test to `Part2.cs` could push that file over the 500-line cap.
- Mitigations and rollbacks:
  - Scope is intentionally limited to the single call site named in the issue (lines 231-234 of
    `QfcItemController.FolderHandling.cs`); any other unguarded call sites found during
    implementation should be filed as separate follow-up issues rather than folded into this fix.
  - Check `Part2.cs`'s line count before appending; split into `...Part3.cs` if needed.
  - Rollback is a plain revert of the one production file and its accompanying test file, since the
    change has no config/data surface.

## Rollout & Follow-up
- Release/rollout steps:
  - Standard PR review and merge; no phased rollout, feature flag, or migration is required for a
    fix this narrow.
- Post-fix monitoring or clean-up tasks:
  - None beyond normal post-merge verification that the regression test passes in CI.
- Links: issue #813; related issue #812 (archive-root getter hardening, out of scope here); related
  issue #797 (originating report of the unset-archive-root state).
