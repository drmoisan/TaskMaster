# outlook-folder-wrapper-tests (Issue #82)

- Date captured: 2026-03-19
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/outlook-folder-wrapper-tests/ (Issue #82)

- Issue: #82
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/82
- Last Updated: 2026-03-19
- Work Mode: full-feature

## Problem / Why

`UtilitiesCS\OutlookObjects\Folder` currently contains multiple production files, and `UtilitiesCS.Test\OutlookObjects\Folder` already contains multiple related MSTest files, but the coverage goal is not yet defined or enforced per production file. That leaves wrapper state, comparer behavior, traversal, navigation, conversion, scoring, and prediction logic exposed to regressions when existing tests miss lines or edge paths.

This work needs a targeted, per-file coverage uplift so every production `.cs` file in `UtilitiesCS\OutlookObjects\Folder` reaches at least 80% line coverage, using tests in `UtilitiesCS.Test\OutlookObjects\Folder` that comply with the repo unit-test policies.

## Proposed Behavior

Extend the existing MSTest suite so each production `.cs` file directly under `UtilitiesCS\OutlookObjects\Folder` is exercised by deterministic unit tests under `UtilitiesCS.Test\OutlookObjects\Folder`. New coverage should use Moq only where isolation requires it, prefer FluentAssertions for new assertions, and preserve current production behavior.

The work should reuse existing tests where they already map to the folder wrappers and comparers, add or extend targeted tests for uncovered branches, register any new test files in `UtilitiesCS.Test\UtilitiesCS.Test.csproj`, and end with coverage evidence that shows every in-scope production file at or above the 80% line threshold.

## Final Validation Snapshot

- QA artifacts: `docs/features/active/2026-03-19-outlook-folder-wrapper-tests-82/evidence/qa-gates/`
- Seam decision: the final implementation keeps the non-public prompt/UI/filesystem seams in `FolderPredictor.cs` and `FolderConverter.cs`; no public API was widened.
- Repo-wide coverage exception: repository-wide coverage remains below `80%`, moving from `42.2%` baseline coverage to `44.66%` final coverage, and further repo-wide uplift is outside approved folder scope.
- Scoped gate result: every in-scope folder file remains at or above `80%` line coverage, changed production lines are `>= 90%` when applicable, and the repository-wide no-regression rule is satisfied.
- Coverage gate evidence: `docs/features/active/2026-03-19-outlook-folder-wrapper-tests-82/evidence/qa-gates/final-qa-coverage-delta-2026-03-19T21-39-29Z.md`

## Acceptance Criteria (early draft)

- [ ] Every production `.cs` file directly under `UtilitiesCS\OutlookObjects\Folder` is covered by deterministic MSTest unit tests located under `UtilitiesCS.Test\OutlookObjects\Folder`, whether by existing tests, extended tests, or newly added tests.
- [ ] The final verified coverage run shows each in-scope production file under `UtilitiesCS\OutlookObjects\Folder` at `>= 80%` line coverage; aggregate coverage alone is not sufficient evidence.
- [ ] New or updated tests follow repository policies: MSTest attributes, Moq only when needed for isolation, FluentAssertions preferred for new assertions, no live Outlook dependency, no external service or process dependency, and no runtime temp-file creation.
- [ ] Any newly added test file under `UtilitiesCS.Test\OutlookObjects\Folder` is explicitly added to `UtilitiesCS.Test\UtilitiesCS.Test.csproj` so it is compiled and executed.
- [ ] The work preserves current production behavior unless a narrowly scoped test seam is required, and any such seam is demonstrably safe and covered by tests.
- [ ] Final validation includes the repo C# formatter/build/test loop plus coverage evidence that can be reviewed per file for the full folder scope.

## Constraints & Risks

- The code targets Outlook interop-heavy types, so tests must avoid live Outlook dependencies and instead focus on deterministic seams, wrappers, pure logic, or mocks/fakes.
- Some files may have tightly coupled constructors or static helpers that make them harder to cover without careful test-only seams.
- `UtilitiesCS.Test\UtilitiesCS.Test.csproj` uses explicit compile includes, so any new test file must also be added to the project file or it will not build.
- The user explicitly requires long-path orchestration and does not accept completion below 80% line coverage for any in-scope production file.

## Test Conditions to Consider

- [ ] Unit coverage areas: comparer equality and hash semantics, wrapper construction and state transitions, traversal and navigation behavior, conversion helpers, prediction/scoring logic, null handling, and boundary cases.
- [ ] Cross-class scenarios: compose small in-memory folder graphs/wrappers to validate interactions among `FolderWrapper`, `FolderTree`, `FolderNavigator`, `FolderConverter`, `FolderScorer`, and `FolderPredictor` without requiring Outlook.
- [ ] Existing-test extension points: reuse and extend the current folder test files before adding new ones when that keeps behavior coverage clearer and compile includes simpler.

## Next Step

- [ ] Promote this potential entry to a GitHub issue as a long-path feature request for the per-file coverage uplift.
- [ ] Create the active feature folder and complete research/spec/planning for the `UtilitiesCS\OutlookObjects\Folder` coverage work, including the per-file `>= 80%` audit requirement.