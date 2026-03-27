# `2026-03-13-utilities-coverage` — User Story

- Issue: #65
- Owner: drmoisan
- Status: Draft
- Last Updated: 2026-03-13T22-21

## Story Statement

- As a developer maintaining `UtilitiesCS`, I want the testable utility classes covered by focused MSTest suites in `UtilitiesCS.Test`, so that I can change extension methods, helpers, threading primitives, and reusable collections without relying on manual regression hunting.
- As a code reviewer responsible for repository quality gates, I want each non-excluded `UtilitiesCS` file to show clear per-file coverage evidence and matching test placement, so that I can verify policy compliance without reverse-engineering what was intentionally skipped.

## Problem / Why

`UtilitiesCS` is reused across the solution, so small regressions in extensions, helper classes, collection types, serializers, or threading helpers can ripple into many downstream projects. Research in this repo shows a large production surface with relatively sparse baseline coverage, which means many behavior changes are currently validated by instinct and luck — two quality tools with famously inconsistent versioning.

The goal of this feature is to replace that fragility with executable expectations: mirrored test files, deterministic unit tests, and per-file coverage evidence that makes it obvious what is protected, what is intentionally excluded, and where additional work is still required.


## Personas & Scenarios

- Persona: **The UtilitiesCS Maintainer**
  - A developer who regularly modifies or extends the shared utility library (extensions, helpers, serialization converters, threading primitives, data structures).
  - Cares about: correctness of utility behaviors, safe refactoring, fast feedback on regressions.
  - Constraints: Works within .NET Framework 4.8.1; cannot introduce new dependencies; must follow MSTest + Moq + FluentAssertions conventions.
  - Goals: Confidently change utility code knowing that tests will catch unintended side effects; reduce manual verification effort.
  - Frustrations: Current 13% coverage means most changes require manual inspection; no safety net for cross-cutting utility breakage; regressions discovered late in integration.

- Persona: **The Repository Reviewer / Maintainer of Quality Gates**
  - A reviewer who checks whether feature work satisfies the repo’s coverage and validation expectations before merge.
  - Cares about: evidence, reproducibility, clear exclusions, and whether new tests follow existing project structure and toolchain rules.
  - Constraints: Must judge the change using repo artifacts (`coverage.cobertura.xml`, `.trx` output, plan/spec docs), not informal claims.
  - Goals: Confirm that the proposed test expansion covers the intended production surface and that any exclusions are explicit and defensible.
  - Frustrations: High package-level coverage can hide poorly covered files; missing `Compile Include` entries or hand-wavy exclusions can make a large test initiative look more complete than it really is.

- Scenario: **Adding a new overload to ArrayExtensions**
  - The maintainer needs to add a new `Slice` overload to `ArrayExtensions.cs` that handles `ReadOnlySpan<T>`.
  - They open `UtilitiesCS.Test/Extensions/ArrayExtensions_Tests.cs` and see existing tests covering null inputs, empty arrays, single-element, and large collections.
  - They add tests for the new overload following the same pattern (null, empty, boundary, typical), run the C# toolchain (`dotnet format` → MSBuild analyzers → MSBuild nullable → `vstest.console`), and confirm all tests pass with ≥80% line coverage on the file.
  - The PR reviewer sees the coverage report confirms the file meets policy, and approves the change without needing to manually trace all affected call sites.

- Scenario: **Reviewing a file with exclusions and mocks**
  - The reviewer opens the feature artifacts after coverage generation and checks a `UtilitiesCS/OutlookObjects/` or `EmailIntelligence/` file that cannot use live Outlook COM objects in unit tests.
  - The spec and coverage evidence show whether the file was tested through isolated logic paths, mocked via existing interfaces, or excluded because it depends on live UI/COM runtime.
  - The reviewer compares the matching `UtilitiesCS.Test/` folder, the project file include list, and the per-file coverage evidence.
  - They can approve or request follow-up work based on explicit evidence instead of guessing whether a missing test was accidental.


## Acceptance Criteria

- [ ] For each in-scope production file in `UtilitiesCS/Extensions/`, `UtilitiesCS/HelperClasses/`, `UtilitiesCS/Threading/`, `UtilitiesCS/ReusableTypeClasses/`, and `UtilitiesCS/NewtonsoftHelpers/`, there is a matching new or expanded test file under the corresponding `UtilitiesCS.Test/` subtree.
- [ ] Testable logic-only files in `UtilitiesCS/EmailIntelligence/`, `UtilitiesCS/OutlookObjects/`, and `UtilitiesCS/Dialogs/` are covered in the matching `UtilitiesCS.Test/` folders, while COM-heavy, UI-heavy, designer-generated, deprecated, obsolete, and interface-only files are listed as exclusions.
- [ ] Every newly added test file is explicitly included in `UtilitiesCS.Test/UtilitiesCS.Test.csproj` so it is compiled and executed by the solution build.
- [ ] Each targeted public behavior has tests for the positive path, invalid or null input handling, edge/boundary cases, and error behavior; concurrency-sensitive types also include deterministic concurrency/state-transition coverage.
- [ ] No unit test depends on live Outlook COM objects, live UI rendering, file-system temp data, network calls, or mutable external state; those dependencies are mocked or the file is excluded from the unit-test target.
- [ ] Coverage output from `coverage/coverage.cobertura.xml` shows at least 80% line coverage for every non-excluded `UtilitiesCS` source file included in this feature’s scope.
- [ ] The validation run completes with zero failing tests and a clean toolchain pass in repo order: format → analyzers → nullable build → test/coverage.
- [ ] The feature artifacts document both successes and exclusions clearly enough that a reviewer can determine whether the coverage objective was fully met or only partially met.


## Non-Goals

- **Testing deprecated code:** The 5 files in `To Depricate/` are excluded from coverage targets and will not receive new tests.
- **Testing UI form rendering:** ~35 WinForms Viewer/Form/Control classes (e.g., `DvgForm`, `ProgressPane`, `ConfigViewer`, `ScreenHelper`) require a live WinForms runtime and are excluded.
- **Testing COM interop requiring live Outlook:** ~20 files with deep COM dependencies (`OutlookItem*`, `FolderWrapper`, `MailItemHelper`, `Calendar`, etc.) are excluded unless logic can be isolated behind existing mock interfaces.
- **Refactoring production code for testability:** This feature adds tests to the existing production API surface. No production code will be modified, restructured, or refactored to improve testability (e.g., no extracting interfaces from static classes, no injecting `IFileSystem` abstractions into SmartSerializable).
- **Testing interface-only files:** The 63 pure interface definitions in `Interfaces/` contain no logic and are excluded.
- **Testing auto-generated code:** Designer-generated `.Designer.cs` files are excluded.
- **Replacing integration or UI testing:** This feature does not attempt to prove live Outlook integration, live WinForms behavior, or end-to-end workflows that require runtime infrastructure outside deterministic unit tests.
- **Treating package-level coverage as sufficient:** Raising overall `UtilitiesCS` coverage is valuable, but this feature does not redefine success away from the per-file target; package-level improvement alone is not considered equivalent to full compliance.
