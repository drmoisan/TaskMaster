# Issue #614 — Remediation Cycle 3 Inputs

- **Issue:** #614
- **Cycle:** 3
- **Timestamp:** 2026-08-27T02-55
- **Branch:** `bug/efc-store-root-selection-leaks-full-outlook-path-into-filing-boundary-614`
- **Entry HEAD:** `e8d8f52952f978a20ae056748e6fa9fd40b5fdb0`
- **Source checkpoint:** `artifacts/orchestration/orchestrator-state.json`
- **PR:** #639, `https://github.com/drmoisan/TaskMaster/pull/639`
- **Workflow run:** `33034033583`
- **Failed job:** `98392718650`, `mstest-coverage / Run MSTest suite with coverage`
- **Work mode:** `full-bug` remediation cycle

## Remediation Trigger

Exact-head CI at `e8d8f52952f978a20ae056748e6fa9fd40b5fdb0` executed 6,586 tests: 6,564 passed and 22 failed. Every failure has the same causal chain:

`AppFileSystemFolderPaths.ResolveOneDriveRoot` -> `LoadFolders` -> `AppFileSystemFolderPaths` constructor -> `ApplicationGlobals.LoadBasicMethod`.

The GitHub-hosted runner has no non-empty `OneDriveCommercial`, `OneDrive`, or `OneDrivePersonal` variable. The production D7 change therefore fails during eager construction of `ApplicationGlobals` in tests whose subject is unrelated to OneDrive. The four other exact-head CI jobs passed: actionlint, formatting, analyzer build, and nullable build.

## Independent Source and Test Inspection

`TaskMaster/AppGlobals/AppFileSystemFolderPaths.cs` already has the correct deterministic seam: an injected `Func<string, string>` supplies environment values to `ResolveOneDriveRoot`, while the public default constructor reads the real process environment and fails explicitly when no root is resolvable.

`TaskMaster/AppGlobals/ApplicationGlobals.cs` does not expose that seam. `LoadBasicMethod` always executes `new AppFileSystemFolderPaths()`. This forces unrelated tests to depend on the host environment even when they already construct `ApplicationGlobals` with mocked Outlook collaborators.

The hosted failures map to eight test files:

1. `TaskMaster.Test/AppGlobals/ApplicationGlobalsTests.cs` — the lazy-load regression constructs with the one-argument constructor and reflectively forces the real `LoadBasicMethod`.
2. `UtilitiesCS.Test/NewtonsoftHelpers/PeopleScoConverter_Tests.cs`.
3. `UtilitiesCS.Test/NewtonsoftHelpers/ScDictionaryConverter_Tests.cs`.
4. `UtilitiesCS.Test/NewtonsoftHelpers/ScoDictionaryConverterTests.cs`.
5. `UtilitiesCS.Test/NewtonsoftHelpers/WrapperScoDictionaryTest.cs`.
6. `UtilitiesCS.Test/NewtonsoftHelpers/WrapperScDictionaryTest.cs`.
7. `UtilitiesCS.Test/ReusableTypeClasses/SmartSerializableLoader_Tests.cs`.
8. `UtilitiesCS.Test/EmailIntelligence/PeopleScoDictionaryNew_Tests.cs`.

The UtilitiesCS.Test files contain ten eager two-argument calls of the form `new TaskMaster.ApplicationGlobals(mockApplication, true)`. The TaskMaster.Test failure is a separate one-argument construction followed by explicit lazy materialization. A one-production/one-test estimate cannot close all 22 failures. The corrected scope is one production file and eight existing test files, executed in 3/3/2 test-file batches.

## Required Outcome

Add a normal constructor-injection path to `ApplicationGlobals` that accepts the existing environment-reader seam and uses it only when constructing `AppFileSystemFolderPaths`. Existing one- and two-argument constructors must retain their current runtime behavior and continue to use `Environment.GetEnvironmentVariable` through the default `AppFileSystemFolderPaths` constructor. Real runtime callers, including `TaskMaster/ThisAddIn.cs`, must therefore continue to fail explicitly with `OneDriveUnresolvableRule` when no OneDrive root is configured.

Adapt the eight environment-independent test files to pass an in-memory reader that returns a fabricated OneDrive root. Do not mutate process environment variables, add a static/global test hook, create temporary files, detect the test host at runtime, or restore an AppData/arbitrary-folder fallback.

## Regression Contract

The executor must add a strongly typed regression test before production implementation. The test calls the new three-argument `ApplicationGlobals` constructor with `loadBasic: true` and an in-memory reader, then asserts that `FS.SpecialFolders["OneDrive"]` equals the fabricated root. Before implementation, the unchanged test must fail to compile with the missing-constructor diagnostic; record that build as `[expect-fail]` evidence with `ExpectedExitCode: 1`. After implementation, the same test body must compile and pass without process-environment access.

The existing `ResolveOneDriveRoot_NoVariableSet_FailsExplicitlyWithARedactedDiagnostic` test remains the runtime fail-fast guard. The existing lazy-load regression must retain its assertions that construction is deferred and explicit force materializes collaborators; only its forced-load environment reader may become deterministic.

## Scope

Production:

- `TaskMaster/AppGlobals/ApplicationGlobals.cs`

Tests:

- `TaskMaster.Test/AppGlobals/ApplicationGlobalsTests.cs`
- `UtilitiesCS.Test/NewtonsoftHelpers/PeopleScoConverter_Tests.cs`
- `UtilitiesCS.Test/NewtonsoftHelpers/ScDictionaryConverter_Tests.cs`
- `UtilitiesCS.Test/NewtonsoftHelpers/ScoDictionaryConverterTests.cs`
- `UtilitiesCS.Test/NewtonsoftHelpers/WrapperScoDictionaryTest.cs`
- `UtilitiesCS.Test/NewtonsoftHelpers/WrapperScDictionaryTest.cs`
- `UtilitiesCS.Test/ReusableTypeClasses/SmartSerializableLoader_Tests.cs`
- `UtilitiesCS.Test/EmailIntelligence/PeopleScoDictionaryNew_Tests.cs`

Planning, evidence, and handoff artifacts may be written only under this feature folder's canonical paths and the repository automation adapter's canonical commit-context path.

## Rejected Alternatives

- Setting OneDrive variables in CI or test setup is rejected because tests may not depend on or mutate process environment state.
- A static factory changed by assembly initialization is rejected because it is mutable process-global state and is unsafe under parallel tests.
- Detecting Moq, VSTest, `CI`, or non-COM Outlook objects in production is rejected as test-host-specific runtime behavior.
- Deferring OneDrive resolution or omitting the `OneDrive` entry is rejected because it weakens AC14/D7's explicit runtime failure contract and would allow consumers using `TryGetValue` to fail silently.
- Editing only one test file is rejected because the exact-head failure census proves that eight independent test files reach the real constructor path.
- Restoring AppData or arbitrary special-folder fallback is rejected because it recreates the Issue #614 misfiling defect.

## Preserved Human Scope Decision

The checkpoint's `human_interaction.requirements[]` entry `issue-614-approved-documentation-findings-scope-change` remains binding. This cycle must not modify either of these files solely for the waived findings:

- `evidence/qa-gates/final-test-coverage.2026-08-26T22-27.md`
- `change-description.2026-08-26.md`

The normalized evidence row remains reported as fail, its underlying 6,586/6,586 local run remains a separate fact, and AC24 remains unchecked. This cycle does not alter PR #639, merge, or publish.

## Verification Requirements

- Phase 0 captures policy reads and one artifact per C# baseline command.
- Red/green evidence proves the new constructor seam before and after implementation.
- A before/after constructor-call census accounts for all ten eager UtilitiesCS.Test calls and the separate TaskMaster.Test lazy-force caller.
- Full C# QA runs in exact order: CSharpier, analyzer rebuild, nullable rebuild, MSTest with coverage. Any failure or formatter mutation restarts the sequence from CSharpier.
- Final evidence records numeric test totals and numeric line/branch coverage, plus baseline/post-change/new-or-changed-code coverage.
- File-size and scope gates enforce the repository's 500-line limit and the one-production/eight-test boundary.
- AC14 remains satisfied, AC24 remains unchecked, and the user-approved documentation scope change remains intact.
- Staging and commit-context collection are prepared for the orchestrator. The executor does not commit, push, edit PR #639, merge, or publish.
