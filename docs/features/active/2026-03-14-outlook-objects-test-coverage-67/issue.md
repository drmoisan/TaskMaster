# outlook-objects-test-coverage (Issue #67)
title: "outlook-objects-test-coverage - Plan"
issue: "TBD"
parent: "none"
owner: "Dan Moisan"
last_updated: "2026-03-14T11-01"
status: "Draft"
status_color: "lightgrey"
version: "0.1"
---

# outlook-objects-test-coverage (Potential)

- Date captured: 2026-03-14
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/outlook-objects-test-coverage/ (Issue #67)

- Issue: #67
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/67
- Last Updated: 2026-03-14
## Problem / Why

The `UtilitiesCS\OutlookObjects` module is a critical part of the TaskMaster solution, providing Outlook COM interop wrappers, folder navigation, mail item helpers, recipient/store utilities, and table extensions. Current unit test coverage sits at approximately 27%, leaving the large majority of its ~52 production C# files untested across 15 subdirectories (AppointmentItem, Attachment, Calendar, Category, Com, Conversation, Explorer, Fields, Filter DASL, Folder, Item, MailItem, Recipient, Store, Table). This creates risk for regressions, makes refactoring dangerous, and violates the repository policy requiring ≥80% line coverage per file. Increasing coverage to ≥80% per file will establish a safety net for ongoing development, improve confidence in the correctness of all Outlook wrappers, and bring the project into compliance with repo policy.

## Proposed Behavior

Add comprehensive MSTest unit tests within `UtilitiesCS.Test\OutlookObjects` to bring per-file line coverage of `UtilitiesCS\OutlookObjects` production code to at least 80%. Tests will:

1. Mirror the directory structure of `UtilitiesCS\OutlookObjects` (e.g., `AppointmentItem/`, `Attachment/`, `Calendar/`, `Category/`, `Com/`, `Conversation/`, `Explorer/`, `Fields/`, `Filter DASL/`, `Folder/`, `Item/`, `MailItem/`, `Recipient/`, `Store/`, `Table/`).
2. Use MSTest as the framework, Moq for mocking, and FluentAssertions for assertions (per repo policy).
3. Follow Arrange–Act–Assert pattern with descriptive test names documenting intent.
4. Cover positive flows, negative flows, edge cases, boundary conditions, and error-handling behavior for each public method.
5. Mock external dependencies (Outlook COM interop) to keep tests isolated and deterministic.
6. Target ≥90% coverage for newly tested classes per repo policy for new test modules.

## Acceptance Criteria (early draft)

- [ ] Every public class/method in `UtilitiesCS/OutlookObjects/AppointmentItem/` has corresponding tests
- [ ] Every public class/method in `UtilitiesCS/OutlookObjects/Attachment/` has corresponding tests
- [ ] Every public class/method in `UtilitiesCS/OutlookObjects/Calendar/` has corresponding tests
- [ ] Every public class/method in `UtilitiesCS/OutlookObjects/Category/` has corresponding tests
- [ ] Every public class/method in `UtilitiesCS/OutlookObjects/Com/` has corresponding tests
- [ ] Every public class/method in `UtilitiesCS/OutlookObjects/Conversation/` has corresponding tests
- [ ] Every public class/method in `UtilitiesCS/OutlookObjects/Explorer/` has corresponding tests
- [ ] Every public class/method in `UtilitiesCS/OutlookObjects/Fields/` has corresponding tests
- [ ] Every public class/method in `UtilitiesCS/OutlookObjects/Filter DASL/` has corresponding tests
- [ ] Every public class/method in `UtilitiesCS/OutlookObjects/Folder/` has corresponding tests (including MsgToMime)
- [ ] Every public class/method in `UtilitiesCS/OutlookObjects/Item/` has corresponding tests
- [ ] Every public class/method in `UtilitiesCS/OutlookObjects/MailItem/` has corresponding tests
- [ ] Every public class/method in `UtilitiesCS/OutlookObjects/Recipient/` has corresponding tests
- [ ] Every public class/method in `UtilitiesCS/OutlookObjects/Store/` has corresponding tests
- [ ] Every public class/method in `UtilitiesCS/OutlookObjects/Table/` has corresponding tests
- [ ] Per-file line coverage ≥80% for each testable OutlookObjects source file
- [ ] All tests pass with zero failures
- [ ] Tests are independent, isolated, fast, and deterministic
- [ ] No external COM/Outlook dependencies in tests — all Outlook interfaces mocked with Moq
- [ ] Tests mirror the production directory structure in `UtilitiesCS.Test\OutlookObjects`
- [ ] Full C# toolchain passes: format → analyzers → nullable build → test
- [ ] New `<Compile Include=...>` entries added to `UtilitiesCS.Test.csproj` for every new test file

## Constraints & Risks

- **COM Interop:** All classes in `OutlookObjects/` depend heavily on Outlook COM interop. Outlook interfaces (e.g., `Outlook.MAPIFolder`, `Outlook.MailItem`, `Outlook.Recipient`) must be mocked via Moq. Focus on testing deterministic logic paths; COM boundary calls that cannot be meaningfully isolated should be minimal stubs.
- **Windows Forms:** `StoreWrapperViewer.cs` contains a WinForms form; only its non-UI logic should be tested.
- **Static Members:** Some classes may have static methods — tests should cover distinct logic branches of statics.
- **.NET Framework 4.8.1:** Target framework constrains available testing patterns.
- **Scope Management:** 52+ production files means careful phased planning with subdirectory batches.
- **Existing Tests:** Existing test files in `UtilitiesCS.Test\OutlookObjects` must not regress.

## Test Conditions to Consider

- [ ] Null/empty inputs for all public method arguments
- [ ] COM object mock behavior: null returns, exception throwing, valid returns
- [ ] Comparers: equal objects, unequal objects, null comparisons, sort ordering
- [ ] String/enum conversions: valid values, invalid values, edge cases
- [ ] Folder navigation: deep hierarchies, empty folders, missing parents
- [ ] Filter/DASL parsing: valid filter strings, malformed strings, edge cases
- [ ] Mail item helpers: missing properties, varied property types
- [ ] Serializable types: round-trip serialization, default states
- [ ] Wrappers: property delegation, lazy initialization, null guards

## Next Step

- [ ] Promote to GitHub issue (feature request template)
- [ ] Create `docs/features/active/outlook-objects-test-coverage/` folder from the template