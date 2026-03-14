# utilities-coverage (Issue #65)
title: "utilities-coverage - Plan"
issue: "TBD"
parent: "none"
owner: "Dan Moisan"
last_updated: "2026-03-13T22-06"
status: "Draft"
status_color: "lightgrey"
version: "0.1"
---

# utilities-coverage (Potential)

- Date captured: 2026-03-13
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/utilities-coverage/ (Issue #65)

- Issue: #65
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/65
- Last Updated: 2026-03-14
- Work Mode: full-feature

## Problem / Why

The UtilitiesCS library is a critical shared utility project used across the TaskMaster solution, providing extensions, helper classes, threading utilities, serialization converters, reusable type classes, and more. Current unit test coverage sits at approximately 13%, leaving the vast majority of public APIs untested. This creates risk for regressions, makes refactoring dangerous, and violates the repository policy requiring ≥80% line coverage. Increasing coverage to ≥80% per file will establish a safety net for ongoing development, improve confidence in the library's correctness, and bring the project into compliance with repo policy.

## Proposed Behavior

Add comprehensive MSTest unit tests within `UtilitiesCS.Test` to bring per-file line coverage of `UtilitiesCS` production code to at least 80%. Tests will:

1. Mirror the directory structure of `UtilitiesCS` (e.g., `Extensions/`, `HelperClasses/`, `Threading/`, `ReusableTypeClasses/`, `NewtonsoftHelpers/`).
2. Use MSTest as the framework, Moq for mocking, and FluentAssertions for assertions (per repo policy).
3. Follow Arrange–Act–Assert pattern with descriptive test names documenting intent.
4. Cover positive flows, negative flows, edge cases, boundary conditions, and error-handling behavior for each public method.
5. Mock external dependencies (Outlook COM interop, file system, network) to keep tests isolated and deterministic.
6. Target ≥90% coverage for newly tested classes per repo policy for new test modules.

## Acceptance Criteria (early draft)

- [ ] Every public class/method in `UtilitiesCS/Extensions/` has corresponding tests in `UtilitiesCS.Test/Extensions/`
- [ ] Every public class/method in `UtilitiesCS/HelperClasses/` has corresponding tests in `UtilitiesCS.Test/HelperClasses/`
- [ ] Every public class/method in `UtilitiesCS/Threading/` has corresponding tests in `UtilitiesCS.Test/Threading/`
- [ ] Every public class/method in `UtilitiesCS/ReusableTypeClasses/` has corresponding tests in `UtilitiesCS.Test/ReusableTypeClasses/`
- [ ] Every public class/method in `UtilitiesCS/NewtonsoftHelpers/` has corresponding tests in `UtilitiesCS.Test/NewtonsoftHelpers/`
- [ ] Per-file line coverage ≥80% for each testable UtilitiesCS source file
- [ ] All tests pass with zero failures
- [ ] Tests are independent, isolated, fast, and deterministic
- [ ] No external dependencies (no file I/O, no network, no COM interop) in tests—all mocked
- [ ] Tests mirror the production directory structure in `UtilitiesCS.Test`
- [ ] Full C# toolchain passes: format → analyzers → nullable build → test

## Constraints & Risks

- **COM Interop:** Many classes in `OutlookObjects/` and `EmailIntelligence/` depend heavily on Outlook COM interop. These require significant Moq interface mocking and may have portions that are impractical to unit test without integration test infrastructure. Focus on testable logic paths first.
- **UI Dependencies:** Classes in `Dialogs/` and `WinFormsExtensions` depend on Windows Forms UI. Tests must isolate logic from UI rendering. Some dialog classes may require UI thread simulation.
- **Static Classes:** Several utility classes are static (e.g., `YesNoToAll`, `InputBox`, `MyBox`), making them harder to mock. Focus on testing their deterministic logic paths.
- **.NET Framework 4.8.1:** Target framework constrains available testing patterns (no top-level statements, limited pattern matching).
- **Scope Management:** 75+ production files means careful phased planning to avoid overwhelming single PRs.

## Test Conditions to Consider

- [ ] Extension methods: null inputs, empty collections, single-element, large collections, type mismatches
- [ ] Helper classes: boundary values, invalid arguments, concurrent access patterns
- [ ] Threading utilities: thread-safety verification, timeout behavior, cancellation
- [ ] Serialization converters: round-trip serialize/deserialize, malformed JSON, missing properties, type discrimination
- [ ] Reusable collections: add/remove/clear/enumerate, concurrent modification, serialization persistence
- [ ] Bayesian classifiers: training with empty corpus, single token, edge probability values

## Next Step

- [ ] Promote to GitHub issue (feature request template)
- [ ] Create `docs/features/active/utilities-coverage/` folder from the template