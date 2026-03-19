# 2026-03-19-outlook-folder-wrapper-tests — Spec

- **Issue:** #82
- **Parent (optional):** none
- **Owner:** drmoisan
- **Last Updated:** 2026-03-19T09-43
- **Status:** Draft
- **Version:** 0.1

## Overview

`UtilitiesCS\OutlookObjects\Folder` currently contains multiple production files, and `UtilitiesCS.Test\OutlookObjects\Folder` already contains multiple related MSTest files, but the coverage goal is not yet defined or enforced per production file. That leaves wrapper state, comparer behavior, traversal, navigation, conversion, scoring, and prediction logic exposed to regressions when existing tests miss lines or edge paths.

This work needs a targeted, per-file coverage uplift so every compiled production `.cs` file in the folder scope reaches at least 80% line coverage, using tests in `UtilitiesCS.Test\OutlookObjects\Folder` that comply with the repo unit-test policies. Research confirms 13 compiled in-scope production files, including nested `UtilitiesCS\OutlookObjects\Folder\MsgToMime\MAPIMethods.cs`, with current baseline line coverage ranging from `100%` for already-covered navigation/comparer files to `0%` for `MAPIMethods.cs`; `FolderPredictor.cs`, `FolderScorer.cs`, `FolderTree.cs`, `FolderConverter.cs`, `FolderWrapper .cs`, `FolderMinimalWrapper.cs`, and `FolderWrapperNameAndParentNameComparer.cs` are the primary uplift targets.


## Behavior

Implementation starts by extending the existing folder MSTest suite so each compiled production file in scope is exercised by deterministic tests under `UtilitiesCS.Test\OutlookObjects\Folder`. The main path is tests-first: reuse the existing one-file-per-area test layout, add focused branch coverage for comparers, wrapper state, traversal, tree construction, scoring, prediction, and conversion helpers, and register any new test file in `UtilitiesCS.Test\UtilitiesCS.Test.csproj` so it actually compiles.

The expected end-to-end flow is: capture baseline per-file coverage, extend the existing tests that already map to `FolderConverter`, `FolderMinimalWrapper`, `FolderNavigator`, `FolderPredictor`, `FolderScorer`, `FolderTree`, and `FolderWrapper`, add a dedicated `MAPIMethods` reflection/constant test if needed, rerun coverage, and verify each compiled file individually against the `>= 80%` threshold. Tests should continue to use Moq-backed in-memory `Outlook.Folder` / `Outlook.Folders` graphs, `InternalsVisibleTo("UtilitiesCS.Test")` access for internal members, and FluentAssertions for new assertions.

Notable alternative behavior is allowed only when tests alone cannot cover static UI or filesystem branches in `FolderPredictor.cs` or `FolderConverter.cs`. In that case, the implementation may add a narrow internal/protected seam around prompt, message-box, directory, or UI-thread calls, but the seam must preserve the current default production behavior, be limited to the blocking dependency boundary, and be covered by tests that exercise both the default and injected paths.


## Inputs / Outputs

- Inputs (CLI flags, files, env vars)
	- Source files in scope are the compiled folder production files listed by research: `FolderConverter.cs`, `FolderMinimalWrapper.cs`, `FolderNavigator.cs`, `FolderPredictor.cs`, `FolderScorer.cs`, `FolderTree.cs`, `FolderWrapper .cs`, `FolderWrapperNameAndParentNameComparer.cs`, `FolderWrapperNameComparer.cs`, `FolderWrapperNameCountSizeComparer.cs`, `FolderWrapperNodeComparer.cs`, `FolderWrapperNodeContentsComparer.cs`, and `MsgToMime\MAPIMethods.cs`.
	- Primary test inputs are the existing folder test files under `UtilitiesCS.Test\OutlookObjects\Folder` plus any newly added file required to cover a compiled production file not currently represented.
	- Validation inputs are the repo-standard C# commands: `csharpier .`, `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`, `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true`, and coverage-enabled `vstest.console.exe` on the relevant `*.Test.dll` assemblies.
	- No new environment variables or secrets are required for this feature.
- Outputs (artifacts, logs, telemetry)
	- Updated or newly added MSTest files under `UtilitiesCS.Test\OutlookObjects\Folder`.
	- An updated `UtilitiesCS.Test\UtilitiesCS.Test.csproj` entry if a new test file is introduced.
	- Reviewable validation artifacts: build output, test results (`.trx`), and per-file coverage evidence such as `coverage\coverage.cobertura.xml` or equivalent final coverage output used during verification.
	- No new production telemetry is expected; this is a test and coverage uplift feature.
- Config keys and defaults:
	- No new app settings, config keys, or feature flags are introduced.
	- Existing runtime behavior remains the default even if a narrow test seam is added for static UI/filesystem calls.
- Versioning or backward-compatibility constraints:
	- No user-facing CLI or API version change is intended.
	- Backward compatibility requirement is strict: folder resolution, scoring, comparison, conversion, and prediction behavior must remain unchanged except for behavior-preserving testability seams.

## API / CLI Surface

This feature does not add a new end-user API or CLI surface. The only command surface in scope is the engineering validation workflow and the existing project/test file contracts.

- Example invocations with expected outputs (concise):
	- `csharpier .`
		- Expected output: C# files are formatted with no remaining formatter changes in the final pass.
	- `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
		- Expected output: analyzer-enabled solution build succeeds without new diagnostics from the folder test changes.
	- `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true`
		- Expected output: nullable/type-safety build succeeds without warnings in touched paths.
	- `vstest.console.exe <relevant test assemblies> /EnableCodeCoverage`
		- Expected output: folder-related MSTest cases pass and the resulting coverage report shows every compiled in-scope folder file at `>= 80%` line coverage.
- Contracts and validation rules:
	- Any new folder test file must be declared explicitly in `UtilitiesCS.Test\UtilitiesCS.Test.csproj`; otherwise it is out of contract because it will not compile into the test assembly.
	- Tests must remain deterministic and isolated: no live Outlook instance, no external services, no external processes, and no runtime temp-file creation.
	- Aggregate coverage is insufficient. The contract is satisfied only when each compiled file in the documented scope individually meets the threshold.
	- If an internal/protected seam is introduced, the seam must default to existing behavior and must not widen the public API surface beyond what is needed for testing.

## Data & State

Data flow for this feature is test-driven rather than runtime-user-driven: Moq-backed folder graphs, wrapper objects, comparer inputs, score collections, and predictor inputs are created in memory by MSTest methods, executed through the existing production code, and then observed through assertions and coverage instrumentation. The implementation may also use reflection or existing internal access where the research already confirmed `InternalsVisibleTo("UtilitiesCS.Test")` is available.

- Data transformations and invariants:
	- `FolderPath`, child-folder collections, tree nodes, comparer inputs, and suggestion collections remain in-memory test data; tests validate transformations such as relative-path resolution, root/store traversal, flatten/filter behavior, score aggregation, and folder-selection fallback handling.
	- If a seam is introduced for static UI/filesystem branches, the invariant is that the default production path remains identical to current behavior while tests can substitute deterministic responses.
	- `MAPIMethods.cs` does not carry runtime business state; its executable lines are primarily declaration/static-initializer related, so constant/reflection assertions are sufficient for coverage without functional behavior changes.
- Caching or persistence details:
	- No new caching layer or persisted application state is added.
	- The only persisted changes are source/test files, optional test project compile includes, and generated validation artifacts such as coverage XML and test results.
- Migration or backfill requirements (if any):
	- None. There is no schema, config, or stored-data migration associated with this feature.

## Constraints & Risks

- The code targets Outlook interop-heavy types, so tests must avoid live Outlook dependencies and instead focus on deterministic seams, wrappers, pure logic, or mocks/fakes.
- Some files have tightly coupled constructors or static helpers that make them harder to cover without careful test-only seams, especially `FolderPredictor.cs` and the UI/filesystem branches inside `FolderConverter.cs`.
- `UtilitiesCS.Test\UtilitiesCS.Test.csproj` uses explicit compile includes, so any new test file must also be added to the project file or it will not build.
- The user explicitly requires long-path orchestration and does not accept completion below 80% line coverage for any in-scope production file.
- Current baseline risk is uneven: `FolderNavigator.cs`, `FolderWrapperNameComparer.cs`, `FolderWrapperNameCountSizeComparer.cs`, `FolderWrapperNodeComparer.cs`, and `FolderWrapperNodeContentsComparer.cs` are already compliant, but `FolderWrapperNameAndParentNameComparer.cs` is just below target and `FolderConverter.cs`, `FolderMinimalWrapper.cs`, `FolderWrapper .cs`, `FolderTree.cs`, `FolderScorer.cs`, `FolderPredictor.cs`, and `MAPIMethods.cs` require substantive new coverage.
- Because the production file `FolderWrapper .cs` contains a space before `.cs`, implementation and coverage verification must use the exact path already compiled by `UtilitiesCS.csproj`; path normalization mistakes could lead to the wrong file being analyzed.


## Implementation Strategy

- Implementation scope (what changes, not sequencing):
	- Extend the existing folder test files first, because most production classes already have an adjacent test home: `FolderConverterTests.cs`, `FolderMinimalWrapperTests.cs`, `FolderNavigatorTests.cs`, `FolderPredictorTests.cs`, `FolderScorerTests.cs`, `FolderTreeTests.cs`, `FolderWrapperStateTests.cs`, and `FolderWrapperTraversalTests.cs`.
	- Add focused coverage for the missing branches identified in research: comparer null/parent-name edge cases, root and UNC restore branches in `FolderMinimalWrapper`, progress-aware and selection/tree-detangling branches in `FolderTree`, folder size fallback and compare/load flows in `FolderWrapper`, additional query and object-array branches in `FolderScorer`, search/recents/suggestions/refresh branches in `FolderPredictor`, and argument-guard plus `MAPIFolder` overload paths in `FolderConverter`.
	- Add a dedicated `UtilitiesCS.Test\OutlookObjects\Folder\MAPIMethodsTests.cs` file if the compiled nested `MsgToMime\MAPIMethods.cs` file remains uncovered by the existing test layout, and register that file in `UtilitiesCS.Test\UtilitiesCS.Test.csproj`.
	- Limit any production changes to narrow testability seams only after a tests-only attempt shows that static prompt, filesystem, or UI-thread branches still block the documented threshold.
- New classes/functions/commands to add or update:
	- New code should be primarily test methods and small shared test helpers for building deterministic `Outlook.Folder`, `Outlook.Folders`, `Application`, `NameSpace`, `Stores`, and related object graphs.
	- Production updates, if required, should be small internal/protected wrappers or delegates around static UI/filesystem entry points rather than broad refactors or new public classes.
	- No new user-facing commands are added.
- Dependency changes (new/removed packages) and rationale:
	- No new NuGet packages are expected. Existing repository dependencies already provide the required test stack: MSTest, Moq, and FluentAssertions.
- Logging/telemetry additions and locations:
	- No new telemetry is planned.
	- No new production logging should be added solely to satisfy coverage. If a seam requires minimal diagnosability during testing, it should stay internal to the touched folder class and be documented in the implementation change summary.
- Rollout plan (feature flags, staged deploys, fallback path):
	- No feature flag or staged rollout is needed because the intended result is test coverage plus optional behavior-preserving seams.
	- The fallback path is tests-only coverage uplift; if that does not reach `>= 80%` for `FolderPredictor.cs` or `FolderConverter.cs`, introduce the smallest seam necessary and revalidate the full C# loop plus per-file coverage evidence before merge.

## Definition of Done

- [x] `user-story.md` and `spec.md` identify the full compiled folder scope, including nested `MsgToMime\MAPIMethods.cs`, and map the acceptance criteria to concrete test coverage work.
- [x] Final verified coverage evidence shows each compiled in-scope folder file at `>= 80%` line coverage, not just the project aggregate.
- [x] Existing folder MSTest files are extended and any newly added file is explicitly included in `UtilitiesCS.Test\UtilitiesCS.Test.csproj`.
- [x] Tests cover positive paths plus the documented null/error/boundary branches for comparers, wrappers, traversal, tree building, scoring, prediction, and conversion helpers.
- [x] Active feature docs are updated to reflect the final scope, seam decisions, and validation evidence; no broader product documentation changes are required unless implementation introduces an unexpected workflow impact.
- [x] Telemetry/logging remains unchanged unless a narrowly scoped internal diagnostic hook is required by an approved seam, and any such addition is documented.
- [x] Final validation completes with `csharpier .`, analyzer build, nullable build, and coverage-enabled `vstest.console.exe` in a clean pass.

## Seeded Test Conditions (from potential)
- [x] Unit coverage areas: comparer equality and hash semantics, wrapper construction and state transitions, traversal and navigation behavior, conversion helpers, prediction/scoring logic, null handling, and boundary cases.
- [x] Cross-class scenarios: compose small in-memory folder graphs/wrappers to validate interactions among `FolderWrapper`, `FolderTree`, `FolderNavigator`, `FolderConverter`, `FolderScorer`, and `FolderPredictor` without requiring Outlook.
- [x] Existing-test extension points: reuse and extend the current folder test files before adding new ones when that keeps behavior coverage clearer and compile includes simpler.

## Final Validation Snapshot

- QA artifacts: `docs/features/active/2026-03-19-outlook-folder-wrapper-tests-82/evidence/qa-gates/`
- Seam decision: the final implementation keeps the non-public prompt/UI/filesystem seams in `FolderPredictor.cs` and `FolderConverter.cs`, with no public API widening.
- Repo-wide coverage exception: repository-wide coverage remains below `80%`, moving from `42.2%` baseline coverage to `44.66%` final coverage, and further repo-wide uplift is outside approved folder scope.
- Threshold result: all 13 in-scope folder files meet `>= 80%` line coverage, and changed production lines meet `>= 90%` when applicable.
- Evidence references: `docs/features/active/2026-03-19-outlook-folder-wrapper-tests-82/evidence/qa-gates/final-qa-test-2026-03-19T21-39-29Z.md` and `docs/features/active/2026-03-19-outlook-folder-wrapper-tests-82/evidence/qa-gates/final-qa-coverage-delta-2026-03-19T21-39-29Z.md`
