# `2026-03-19-outlook-folder-wrapper-tests` — User Story

- Issue: #82
- Owner: drmoisan
- Status: Draft
- Last Updated: 2026-03-19T09-43

## Story Statement

- As a `UtilitiesCS` maintainer, I want every compiled production file under `UtilitiesCS\OutlookObjects\Folder` covered by deterministic folder tests, so that wrapper, comparer, traversal, and prediction regressions are caught before they reach the add-in.
- As an engineer extending Outlook folder selection behavior, I want an in-memory MSTest harness for the folder wrappers and related helpers, so that I can change logic confidently without requiring a live Outlook session or manual UI prompts.

## Problem / Why

`UtilitiesCS\OutlookObjects\Folder` currently contains multiple production files, and `UtilitiesCS.Test\OutlookObjects\Folder` already contains multiple related MSTest files, but the coverage goal is not yet defined or enforced per production file. That leaves wrapper state, comparer behavior, traversal, navigation, conversion, scoring, and prediction logic exposed to regressions when existing tests miss lines or edge paths.

This work needs a targeted, per-file coverage uplift so every production `.cs` file in the compiled folder scope reaches at least 80% line coverage, using tests in `UtilitiesCS.Test\OutlookObjects\Folder` that comply with the repo unit-test policies. Research shows the current baseline ranges from `100%` for already-covered comparer/navigation files down to `0%` for compiled `MsgToMime\MAPIMethods.cs`, with `FolderPredictor.cs`, `FolderScorer.cs`, `FolderTree.cs`, `FolderConverter.cs`, `FolderWrapper .cs`, and `FolderMinimalWrapper.cs` all materially below target.


## Personas & Scenarios

- Persona: `UtilitiesCS` maintainer responsible for Outlook folder wrapper reliability
  - Cares about preventing regressions in folder comparison, traversal, prediction, and wrapper-state logic before solution-wide validation.
  - Must keep tests deterministic, fast, and independent of live Outlook, external services, or runtime temp files.
  - Wants coverage evidence per compiled file, not just aggregate project coverage, because the risky code is concentrated in a small subsystem.
  - Is constrained by explicit test compile includes in `UtilitiesCS.Test\UtilitiesCS.Test.csproj` and by the repo-standard MSTest + Moq + FluentAssertions stack.
- Persona: engineer modifying folder scoring or prediction behavior
  - Cares about being able to refactor `FolderPredictor`, `FolderScorer`, `FolderTree`, and `FolderWrapper` without breaking existing folder-routing behavior.
  - Is frustrated by static UI/filesystem branches that are hard to exercise unless a narrow test seam exists.
  - Needs a clear list of in-scope files and boundary scenarios so coverage work stays focused instead of becoming a broad Outlook refactor.
- Scenario: a maintainer prepares a change to folder prediction and wrapper traversal code
  - The trigger is a planned change in `UtilitiesCS\OutlookObjects\Folder` where current coverage is below the repo expectation for several compiled files.
  - The maintainer reviews the per-file baseline and extends the existing MSTest files under `UtilitiesCS.Test\OutlookObjects\Folder` for comparers, wrappers, traversal, tree construction, scoring, and prediction behaviors.
  - When an uncovered branch depends on static UI or filesystem helpers, the maintainer first attempts a tests-only approach and adds a minimal internal/protected seam only if the final per-file `>= 80%` target remains blocked.
  - If a new test file is needed, such as coverage for compiled `MsgToMime\MAPIMethods.cs`, the maintainer adds it to `UtilitiesCS.Test\UtilitiesCS.Test.csproj` so the test is compiled and executed.
  - The expected outcome is a clean C# validation pass plus reviewable coverage evidence showing every compiled file in the folder scope at or above `80%` line coverage.


## Acceptance Criteria

- [x] Tests under `UtilitiesCS.Test\OutlookObjects\Folder` cover every compiled production file in scope: `FolderConverter.cs`, `FolderMinimalWrapper.cs`, `FolderNavigator.cs`, `FolderPredictor.cs`, `FolderScorer.cs`, `FolderTree.cs`, `FolderWrapper .cs`, `FolderWrapperNameAndParentNameComparer.cs`, `FolderWrapperNameComparer.cs`, `FolderWrapperNameCountSizeComparer.cs`, `FolderWrapperNodeComparer.cs`, `FolderWrapperNodeContentsComparer.cs`, and compiled nested `MsgToMime\MAPIMethods.cs`.
- [x] The final verified coverage evidence records each listed file at `>= 80%` line coverage; aggregate or project-level coverage alone does not satisfy the requirement.
- [x] New or updated tests cover both main-path and negative/boundary behavior that the research identified as risky, including null comparer inputs, missing folder lookups, empty or single-item collections, relative-path restore failures, root or UNC parent-store traversal, wrapper fallback logic, and prediction/suggestion edge cases.
- [x] New or updated tests follow repository policies: MSTest attributes, Moq only where isolation requires it, FluentAssertions preferred for new assertions, no live Outlook dependency, no external service or process dependency, and no runtime temp-file creation.
- [x] Any newly added test file under `UtilitiesCS.Test\OutlookObjects\Folder` is explicitly added to `UtilitiesCS.Test\UtilitiesCS.Test.csproj` so it is compiled and executed.
- [x] If tests alone cannot raise `FolderPredictor.cs` or `FolderConverter.cs` to the threshold, only narrowly scoped internal/protected seams for static UI/filesystem calls are introduced, default production behavior remains unchanged, and the seam behavior is itself covered by tests.
- [x] Final validation completes with the repo C# loop: `csharpier .`, analyzer build, nullable build, and coverage-enabled `vstest.console.exe`, with reviewable per-file coverage output for the entire folder scope.

## Final Validation Snapshot

- QA artifacts: `docs/features/active/2026-03-19-outlook-folder-wrapper-tests-82/evidence/qa-gates/`
- Seam decision: `FolderPredictor.cs` and `FolderConverter.cs` retain the final non-public prompt/UI/filesystem seams required to reach deterministic coverage, and default production behavior remains unchanged.
- Repo-wide coverage exception: repository-wide coverage remains below `80%`, improving from `42.2%` baseline coverage to `44.66%` final coverage, and further repo-wide uplift is outside approved folder scope.
- Coverage evidence: `docs/features/active/2026-03-19-outlook-folder-wrapper-tests-82/evidence/qa-gates/final-qa-test-2026-03-19T21-39-29Z.md` and `docs/features/active/2026-03-19-outlook-folder-wrapper-tests-82/evidence/qa-gates/final-qa-coverage-delta-2026-03-19T21-39-29Z.md`
- Final threshold result: all 13 in-scope files are `>= 80%`, and changed production lines are `>= 90%` when applicable.


## Non-Goals

- Live Outlook integration testing, manual UI prompt testing, or any dependency on an installed Outlook profile.
- Broad redesign of the Outlook folder subsystem beyond the minimal testability work needed to reach the documented coverage goal.
- Relaxing the `>= 80%` per-file requirement by relying on aggregate coverage or by excluding compiled folder files that are already part of `UtilitiesCS.csproj`.
- User-facing behavior changes to folder resolution, scoring, or prediction that are not required to preserve behavior while making the code deterministically testable.
