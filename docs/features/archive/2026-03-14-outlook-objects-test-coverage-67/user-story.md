# `2026-03-14-outlook-objects-test-coverage-67` — User Story

- Issue: #67
- Owner: Dan Moisan
- Status: Draft
- Last Updated: 2026-03-14

## Story Statement

- As a developer maintaining `UtilitiesCS/OutlookObjects`, I want high-confidence MSTest coverage that mirrors the OutlookObjects production folder layout, so that I can refactor wrapper classes, comparers, parsers, DTOs, and helper logic without breaking behavior hidden behind Outlook COM boundaries.
- As a reviewer responsible for repository quality gates, I want explicit per-file coverage evidence, `UtilitiesCS.Test.csproj` compile-include updates, and documented exclusions for blocked Outlook branches, so that I can verify policy compliance and refactoring safety without reverse-engineering which gaps are intentional.

## Problem / Why

`UtilitiesCS/OutlookObjects` is a large, high-risk module that mixes pure helper logic with Outlook COM interop, WinForms prompts, filesystem access, reflection wrappers, and lazy wrapper behavior. The research verified 51 production files across 15 subdirectories plus root `MailResolution.cs`, with especially large and risky clusters in `Folder`, `Item`, `MailItem`, `Table`, `Store`, `Fields`, and `Conversation`.

The current low baseline coverage means even small edits to folder navigation, recipient parsing, mail-item helpers, store filtering, table transforms, or property-accessor logic can regress silently and are expensive to review with confidence. This feature turns the module’s current behavior into executable documentation: mirrored tests in `UtilitiesCS.Test/OutlookObjects`, deterministic Moq/subclass seams for testable logic, explicit evidence in `coverage.cobertura.xml` and `.trx` outputs, and explicit documentation for the branches that remain blocked by repo policy or high-risk runtime coupling.

## Personas & Scenarios

- Persona: **OutlookObjects Maintainer**
  - A developer who changes `UtilitiesCS/OutlookObjects` to fix bugs, improve parsing, reduce duplication, or refactor wrappers around Outlook COM objects.
  - Cares about: preserving behavior while cleaning up legacy logic, moving faster with confidence, and knowing exactly which files are still risky because they need minimal seams.
  - Constraints: must stay on `.NET Framework 4.8.1`; must use MSTest, Moq, and FluentAssertions; cannot rely on live Outlook, WinForms runtime behavior, external services, or temp-file tests; must remember that `UtilitiesCS.Test.csproj` uses explicit compile includes.
  - Goals and frustrations: wants fast deterministic feedback when changing `RecipientStatic`, `StoreWrapper`, `FolderMinimalWrapper`, `MailItemHelper`, `OutlookItem`, or `OlTableExtensions`, but is frustrated that today many failures would only surface during later manual or integration-style verification.
  - Their context and motivations: works inside an older solution with explicit `Compile Include` project wiring, path hazards like `FolderWrapper .cs`, and a module where namespace-to-folder mappings are not always intuitive.
- Scenario: **Refactoring folder/store logic without breaking OutlookObjects behavior**
  - The OutlookObjects maintainer needs to simplify logic in `UtilitiesCS/OutlookObjects/Store/StoresWrapper.cs` and `UtilitiesCS/OutlookObjects/Folder/FolderMinimalWrapper.cs`.
  - They open the mirrored tests in `UtilitiesCS.Test/OutlookObjects/Store/` and `UtilitiesCS.Test/OutlookObjects/Folder/` and add or adjust assertions for store include/exclude rules, relative-path restoration, null handling, comparer behavior, and edge conditions.
  - If a new test file is created, they add the matching `<Compile Include=...>` line to `UtilitiesCS.Test/UtilitiesCS.Test.csproj` in the same batch.
  - They run the repo C# validation loop and inspect `.trx` plus `coverage.cobertura.xml` to confirm the changed production files still meet the per-file threshold.
  - They expect the tests to catch contract drift immediately, rather than learning about it later from manual Outlook usage or downstream review.

- Persona: **Coverage Gate Reviewer**
  - A maintainer or reviewer who checks whether the feature satisfies repository quality and documentation expectations before merge.
  - Cares about: explicit scope by folder/class type, measurable per-file coverage, deterministic tests, honest blocker documentation, and compatibility with downstream atomic planning.
  - Constraints: must review evidence from docs, `.trx` output, `coverage.cobertura.xml`, and the test project file rather than trusting summary claims; cannot accept temp-file or live-Outlook-dependent unit tests.
  - Goals and frustrations: wants to confirm that each testable OutlookObjects file is covered or explicitly blocked, and gets frustrated when coverage initiatives improve package totals while leaving risky files under-tested, misplaced, or missing from the project file.
  - Their context and motivations: is protecting a shared codebase where Outlook interop regressions are costly and where repo policy requires strong unit-test discipline plus precise documentation of exceptions.
- Scenario: **Reviewing a high-risk batch that includes seam work**
  - The reviewer examines a batch that adds tests for `RecipientStatic`, `StoreWrapper`, and part of `OlTableExtensions`.
  - They compare the production folders to the mirrored `UtilitiesCS.Test/OutlookObjects` layout and verify every new test file is also listed in `UtilitiesCS.Test/UtilitiesCS.Test.csproj`.
  - They check that the tests cover positive flows, invalid input, edge conditions, and failure branches using Moq or subclass seams instead of live Outlook, dialogs, or temp files.
  - They review the docs to see which branches remain blocked because they still require minimal seams around UI, COM lifetime, filesystem calls, or live namespace resolution.
  - They expect enough specificity to approve the batch or request targeted follow-up work without reverse-engineering the module or guessing whether a low-coverage file was intentionally deferred.

- Persona: **Atomic Planning Agent / Technical Lead**
  - A planner preparing the downstream C# implementation batches for this feature.
  - Cares about: a scope map that is small enough to turn into atomic tasks, realistic file-to-test mappings, and an explicit list of blocked branches that should not be promised in the first batch.
  - Constraints: must keep batches narrow, preserve current behavior, and avoid accidental scope growth into a repo-wide Outlook abstraction rewrite.
  - Goals and frustrations: wants a clean split between seam-ready files and seam-needed hotspots, and gets frustrated when specs say “increase coverage” without telling execution which files are safe first wins versus risky late-stage work.
  - Their context and motivations: needs this user story and the companion spec to be specific enough that Phase/Task planning is mechanical rather than interpretive.
- Scenario: **Planning the first implementation batch**
  - The planner starts with the feature docs and selects the pure and seam-ready slice: `Filter DASL`, DTO/comparer files, `MailResolution.cs`, and `Item/OutlookItem*` helpers.
  - They derive matching tests under mirrored `UtilitiesCS.Test/OutlookObjects` folders and add the required explicit project includes.
  - They defer `FolderPredictor`, `StoreWrapperController`, and temp-file branches of `AttachmentSerializable` because the docs mark those branches as blocked until minimal seams exist.
  - They expect the docs to tell them where to start, what evidence to collect, and which hazards could otherwise burn a sprint on accidental complexity.

## Acceptance Criteria

- [ ] `UtilitiesCS.Test/OutlookObjects` mirrors the production OutlookObjects folder layout for every new or relocated test file created by this feature, including `AppointmentItem`, `Attachment`, `Conversation`, `Fields`, `Filter DASL`, `Folder`, `Item`, `MailItem`, `Recipient`, `Store`, and `Table` where work is performed.
- [ ] Every new OutlookObjects test file is explicitly added to `UtilitiesCS.Test/UtilitiesCS.Test.csproj` with a matching `<Compile Include=...>` entry so the file is compiled and executed.
- [ ] The implementation is batched in a way that follows the documented testability split: pure/seam-ready files first, existing-pattern wrappers second, reflection/lazy wrappers third, and high-risk seam-needed files last.
- [ ] Tests for each in-scope production file cover the positive path, invalid or null input handling, boundary or edge conditions, and error-handling behavior that can be exercised without live Outlook, WinForms UI, or temp-file creation.
- [ ] Tests use MSTest for structure, Moq for Outlook COM or interface mocking, and FluentAssertions for new assertions, matching repo policy and existing project conventions.
- [ ] Per-file coverage evidence from `coverage.cobertura.xml` or the emitted Cobertura-equivalent artifact demonstrates at least 80% line coverage for each testable file under `UtilitiesCS/OutlookObjects`; files that cannot currently reach the threshold because of dialog, filesystem, temp-file, COM lifetime, or live namespace/profile branches are listed explicitly with the blocking reason.
- [ ] Validation evidence includes passing `.trx` results from `vstest.console.exe` and successful completion of the repo-standard C# loop: format, analyzer build, nullable build, and coverage-enabled test execution.
- [ ] High-risk files such as `StoreWrapperController.cs`, `FolderPredictor.cs`, `ConversationHelper.cs`, `OlTableExtensions.cs`, `UserDefinedFields.cs`, and `FolderWrapper .cs` use only minimal seams needed to unit test deterministic logic, rather than broad architectural rewrites.
- [ ] Repo-specific hazards are handled explicitly in both implementation and evidence, including `UtilitiesCS.Test.csproj` explicit compile includes and the exact `FolderWrapper .cs` filename with its embedded space before `.cs`.
- [ ] No test depends on live Outlook profiles, real mailboxes, WinForms rendering, network calls, or temporary filesystem data; all such dependencies are mocked, subclassed behind existing seams, or called out as blocked in the feature documentation.
- [ ] The feature docs identify remaining blocked branches clearly enough that a reviewer or planner can determine whether a follow-up seam/refactor task is required.

## Non-Goals

- Proving live Outlook integration end to end; this feature is limited to deterministic unit coverage and does not replace manual or integration validation against a real Outlook profile.
- Testing WinForms rendering or interactive dialogs in files such as `StoreWrapperController.cs` or `FolderPredictor.cs`; only logic reachable through minimal non-UI seams is in scope.
- Creating temp-file-based tests for `AttachmentSerializable` or other filesystem-dependent branches; repo policy currently forbids temporary-file tests.
- Rewriting the entire Outlook interop surface behind a new abstraction layer; only narrow, local seams are acceptable when needed to unlock otherwise blocked deterministic tests.
- Changing public runtime behavior of `UtilitiesCS/OutlookObjects`; the purpose is to document and protect current behavior, not redesign the module.
- Treating solution-wide or package-wide coverage gains as sufficient; the success condition remains per-file coverage and explicit documentation of blocked or excluded files.
- Normalizing namespaces across OutlookObjects as part of this feature; namespace cleanup may be a future follow-up, but it is not part of the coverage deliverable.
- Modifying `issue.md`, `research.md`, or repo policy documents; those remain authoritative inputs, not rewrite targets.
