# 2026-05-14-ci-format-and-vs-test-failures (Spec)

- **Issue:** #155
- **Parent (optional):** none
- **Owner:** drmoisan
- **Last Updated:** 2026-05-14T08-32
- **Status:** Draft
- **Version:** 0.1

## Context
CI run #156 fails at the "Verify formatting" step because committed git merge-conflict
artifact files are invalid, and `TaskMaster.csproj` lacks a trailing newline. Separately,
several MSTest unit tests fail under the Visual Studio / `vstest.console.exe` runner
(including CI) while passing under the VS Code runner.

Environment:
- OS/version: Windows 11 Pro 10.0.26200; CI runner windows-latest (Windows Server 2025)
- Toolchain: .NET Framework / VSTO classic projects; CSharpier (latest); MSBuild; vstest.console.exe
- Command/flags used: `csharpier check .` (CI "Verify formatting"); MSTest run via Visual Studio and vstest.console.exe
- Data source or fixture: TaskMaster.sln solution; UtilitiesCS.Test, ToDoModel.Test, and related test assemblies

Impact / Severity:
- [x] Blocker
- [ ] High
- [ ] Medium
- [ ] Low

CI is red on the `development` branch; the formatting gate blocks all downstream build,
analyzer, nullable, and test steps. The runner-dependent test failures undermine confidence
in the local Visual Studio test signal.


## Repro & Evidence
Steps to Reproduce:
1. Run the CI workflow `.github/workflows/ci.yml` (or `csharpier check .` locally) — the "Verify formatting" step exits with code 1.
2. Open `TaskMaster.sln` in Visual Studio and run the full MSTest suite, or run `vstest.console.exe` against the built test assemblies.
3. Observe the failing tests listed under Actual Behavior, which pass when run from the VS Code test runner.

Expected:
`csharpier check .` passes with exit code 0, and the full MSTest suite passes identically
under the Visual Studio runner, `vstest.console.exe` (CI), and the VS Code runner.

Actual:
CI #156 "Verify formatting" output:

```
Warning The csproj at ...\TaskMaster\TaskMaster_BACKUP_1250.csproj failed to load with the
  following exception Name cannot begin with the '<' character, hexadecimal value 0x3C.
  Line 471, position 2.
Warning .\TaskMaster\TaskMaster_BACKUP_1250.csproj - Appeared to be invalid xml so was not formatted.
Error .\TaskMaster\TaskMaster.csproj - Was not formatted.
  The file did not end with a single newline.
Checked 1054 files in 25617ms.
Error: Process completed with exit code 1.
```

Visual-Studio-runner test failures (pass under VS Code):

- `TraceExtensions_Tests.GetCallerByName_ReturnsMatchingMethodAndHandlesEmptySpecialAndMissingNames` — `Expected object not to be <null>`.
- `TraceUtility_Tests.GetMethodCallLogString_ShouldIncludeCallerMethodAndParameters` — produced string contains `mscorlib.dll.InvokeMethod` instead of `CaptureMethodCallLogString`.
- `TraceUtility_Tests.GetMethodTraceString_ShouldIncludeCurrentCallChain` — produced string does not contain `CaptureMethodTraceString`.
- `AngleSharpParsedEmailBodyTests.ExtractLinks_StateUnderTest_ExpectedBehavior` and `FilterLinksByDomain_StateUnderTest_ExpectedBehavior` — `System.IO.FileLoadException: Could not load file or assembly 'System.Text.Encoding.CodePages, Version=10.0.0.7'`.
- `SCODictionary_Additional_Tests.Deserialize_WhenInvalidJsonSimpleOverloadAndPromptDisabled_SerializesCurrentState` — `Expected boolean to be True, but found False`.

Logs / Screenshots:
- [x] Attached minimal logs or screenshot
- Snippet: see Actual Behavior above; CI run #156 (databaseId 25676969961) step "Verify formatting".


## Scope & Non-Goals
- In scope:
  - Stream 1 — Remove four committed git mergetool `.csproj` artifacts (`TaskMaster/TaskMaster_BACKUP_1250.csproj`, `TaskMaster_BASE_1250.csproj`, `TaskMaster_LOCAL_1250.csproj`, `TaskMaster_REMOTE_1250.csproj`) and three stray unreferenced backup files (`ToDoModel/ToDoItem_Backup.bkp`, `ToDoModel/Data Model/People/PeopleScoDictionaryNewBackup.cs`, `UtilitiesCS/EmailIntelligence/People/PeopleScoDictionaryNewBackup.cs`); restore the single trailing newline on `TaskMaster/TaskMaster.csproj`.
  - Stream 2 — Test-only fix to `UtilitiesCS.Test/Extensions/TraceExtensions_Tests.cs`: remove the `ResolveCaller` helper indirection so the captured stack frame is the test method's own frame.
  - Stream 3 — Product defect fix to `UtilitiesCS/HelperClasses/Logging/TraceUtility.cs` (`GetCallerMethod` fallback returns a non-project frame) plus `[MethodImpl(MethodImplOptions.NoInlining)]` annotations on the helper methods in `UtilitiesCS.Test/HelperClasses/TraceUtility_Tests.cs`, with a regression-test-first authored in the same test file.
  - Stream 4 — Test-only isolation fix to `UtilitiesCS.Test/ReusableTypeClasses/SCODictionary_Additional_Tests.cs`: wrap the `HasWrittenPath` assertion in the existing `SpinWait.SpinUntil` pattern.
  - Stream 5 — Build-config fix aligning `UtilitiesCS.Test` to `System.Text.Encoding.CodePages` 10.0.7 across `packages.config`, `UtilitiesCS.Test.csproj`, and `app.config`.
  - All five streams land on the single combined branch `bug/ci-format-and-vs-test-failures-155`.
- Out of scope / non-goals:
  - No redesign of `TraceExtensions.GetCallerByName`, `TraceUtility.GetFirstMethodOfMine`, or the broader Trace caller-resolution suite beyond the single `GetCallerMethod` fallback defect.
  - No change to the `async void Serialize(string filepath)` design in `SCODictionary`; the test is corrected to tolerate the existing fire-and-forget behavior.
  - No NuGet package upgrades beyond aligning `UtilitiesCS.Test` to the already-restored 10.0.7 package; no changes to other projects (already aligned to 10.0.7).
  - No opportunistic refactors, formatting sweeps, or analyzer-debt cleanup outside the touched files.
- Explicitly excluded systems, integrations, or datasets:
  - The VS Code test runner configuration (the failures are runner-divergence symptoms; the fix targets runner-independence, not the VS Code adapter).
  - CI workflow definitions (`.github/workflows/ci.yml`); the formatting gate is satisfied by correcting the source files, not by changing the workflow.

## Root Cause Analysis
- `TaskMaster/` contains four committed git mergetool artifacts: `TaskMaster_BACKUP_1250.csproj`,
  `TaskMaster_BASE_1250.csproj`, `TaskMaster_LOCAL_1250.csproj`, `TaskMaster_REMOTE_1250.csproj`
  (the `_BACKUP_` file still contains raw `<<<<<<< / ======= / >>>>>>>` markers). They were
  committed by a "WIP" commit and are not part of any solution.
- `TaskMaster/TaskMaster.csproj` does not end with a single newline.
- Additional stray, unreferenced backup files: `ToDoModel/ToDoItem_Backup.bkp`,
  `ToDoModel/Data Model/People/PeopleScoDictionaryNewBackup.cs`,
  `UtilitiesCS/EmailIntelligence/People/PeopleScoDictionaryNewBackup.cs` (none referenced by any `.csproj`).
- AngleSharp `FileLoadException`: `UtilitiesCS.csproj` references `System.Text.Encoding.CodePages`
  Version 10.0.0.7 (package 10.0.7) while `UtilitiesCS.Test.csproj` still references Version 10.0.0.5
  (package 10.0.5) — the recent "Upgrade Nuget Packages" commit missed the test project.
- Trace tests assert on the runtime call stack, which differs between the VS test host and the
  VS Code runner (reflection `InvokeMethod` frame, method inlining).
- SCODictionary deserialize test: root cause under investigation (test isolation vs. product defect).


## Proposed Fix

### Design summary (what changes where):
Five independent corrections, one combined branch. Stream 1 removes invalid/stray files and restores a trailing newline so `csharpier check .` exits 0. Stream 2 removes a test helper so the captured stack frame is the test method's own. Stream 3 is the only product change: the `GetCallerMethod` fallback in `TraceUtility.cs` is corrected to return `null` rather than a non-project assembly frame, made consistent with the existing `IsMine()` project-name filter; the corresponding test helpers are annotated `[MethodImpl(MethodImplOptions.NoInlining)]`. Stream 4 wraps a test assertion in the established `SpinWait.SpinUntil` polling pattern to tolerate the fire-and-forget `async void Serialize`. Stream 5 aligns the `UtilitiesCS.Test` project to the already-restored `System.Text.Encoding.CodePages` 10.0.7 package across three config files.

### Boundaries and invariants to preserve:
- `TraceUtility.GetCallerMethod` must continue to return a valid project-assembly `MethodBase` when one is present at the search depth; only the non-project fallback result changes (to `null`).
- The `ProjectNames` / `IsMine()` filter remains the single source of truth for "is this a project frame".
- No public API signatures change. `TraceUtility`, `TraceExtensions`, and `SCODictionary` public surfaces are unchanged.
- The full MSTest suite must remain green: CI baseline of 3989 passing / 2 skipped, plus the one new regression test, with zero failures.
- Repository-wide line coverage must remain >= 80%; changed lines in `TraceUtility.cs` must not lose coverage and the changed `GetCallerMethod` path targets >= 90%.

### Dependencies or blocked work:
- None blocking. The `packages\System.Text.Encoding.CodePages.10.0.7\` folder is already present on disk; no NuGet restore is strictly required, though a restore after the `packages.config` edit is recommended for lock consistency.

### Implementation strategy (what changes, not sequencing):

#### Files/modules to change:
- `TaskMaster/TaskMaster.csproj` — restore single trailing newline.
- Deletions: `TaskMaster/TaskMaster_BACKUP_1250.csproj`, `TaskMaster/TaskMaster_BASE_1250.csproj`, `TaskMaster/TaskMaster_LOCAL_1250.csproj`, `TaskMaster/TaskMaster_REMOTE_1250.csproj`, `ToDoModel/ToDoItem_Backup.bkp`, `ToDoModel/Data Model/People/PeopleScoDictionaryNewBackup.cs`, `UtilitiesCS/EmailIntelligence/People/PeopleScoDictionaryNewBackup.cs`.
- `UtilitiesCS/HelperClasses/Logging/TraceUtility.cs` — `GetCallerMethod` fallback (~line 217).
- `UtilitiesCS.Test/Extensions/TraceExtensions_Tests.cs` — remove `ResolveCaller` helper.
- `UtilitiesCS.Test/HelperClasses/TraceUtility_Tests.cs` — add `NoInlining` attributes; add regression test.
- `UtilitiesCS.Test/ReusableTypeClasses/SCODictionary_Additional_Tests.cs` — add `SpinWait.SpinUntil` wait (~line 244).
- `UtilitiesCS.Test/packages.config`, `UtilitiesCS.Test/UtilitiesCS.Test.csproj`, `UtilitiesCS.Test/app.config` — CodePages 10.0.7 alignment.

#### Functions/classes/CLI commands impacted:
- `TraceUtility.GetCallerMethod` (production fallback behavior).
- Test classes `TraceExtensions_Tests`, `TraceUtility_Tests`, `SCODictionary_Additional_Tests`.
- `csharpier check .` (CI "Verify formatting" step) — moves from exit 1 to exit 0.

#### Data flow and validation changes:
- In `GetCallerMethod`, the fallback path's result is validated against the `IsMine()` project-name filter before being returned; a non-project frame yields `null` instead of the raw `mscorlib` frame.

#### Error handling and logging updates:
- No new error handling. The `GetCallerMethod` change makes the fallback fail "soft" by returning `null` (an already-handled state) rather than surfacing an uninformative non-project frame in trace output.

#### Rollback/feature-flag considerations (if applicable):
- No feature flags. Rollback is a straightforward revert of the branch; each stream is independently revertible.

### Technical specifications (interfaces/contracts):

#### Inputs/outputs and formats:
- `GetCallerMethod` continues to accept a `StackTrace` plus a `ref` level and return `MethodBase?`; the contract is tightened so the return is either a project-assembly method or `null`, never a non-project method.

#### Required configuration keys and defaults:
- `UtilitiesCS.Test/packages.config`: new entry `System.Text.Encoding.CodePages` version `10.0.7`, `targetFramework="net481"`.
- `UtilitiesCS.Test/UtilitiesCS.Test.csproj`: Reference `Version=10.0.0.7` with HintPath under `System.Text.Encoding.CodePages.10.0.7\lib\net462\`.
- `UtilitiesCS.Test/app.config`: `bindingRedirect oldVersion="0.0.0.0-10.0.0.7" newVersion="10.0.0.7"` for `System.Text.Encoding.CodePages`.

#### Backward-compatibility expectations:
- No breaking changes. All callers of `GetCallerMethod` already handle a `null` result. No public API or serialized-format changes.

#### Performance constraints (latency/throughput/memory):
- The `SpinWait.SpinUntil` in the Stream 4 test caps at a 1-second timeout, consistent with the existing pattern at lines 43-50 of the same file. No production performance impact.

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

- [ ] Remove the four merge-conflict `.csproj` artifacts and the three stray backup files; confirm no `.csproj` references them.
- [ ] Add a trailing newline to `TaskMaster.csproj`; re-run `csharpier check .` to green.
- [ ] Align every project consuming `UtilitiesCS` to `System.Text.Encoding.CodePages` 10.0.7 (packages.config, csproj Reference + HintPath, app.config binding redirect).
- [ ] Make Trace caller-resolution and/or its tests runner-independent; add deterministic regression coverage.
- [ ] Diagnose and fix the SCODictionary deserialize runner dependency; add regression coverage.
- [ ] Validation: `csharpier check .` green; full MSTest suite green under `vstest.console.exe`.

- Regression tests to add or update:
  - Stream 3 (product defect) — Author one new failing-first regression test in `UtilitiesCS.Test/HelperClasses/TraceUtility_Tests.cs` asserting that `TraceUtility.GetCallerMethod` does not return a non-project-assembly frame when all project frames are exhausted (it must return `null` or a project-assembly method). This is the only stream requiring a new regression test, per the Bugfix Workflow.
  - Streams 1, 2, 4, 5 — No new regression tests. The regression gate for each is an existing artifact: Stream 1 is gated by `csharpier check .` exiting 0; Stream 2 by the existing `GetCallerByName_...` test; Stream 4 by the existing `Deserialize_WhenInvalidJsonSimpleOverloadAndPromptDisabled_SerializesCurrentState` test (it is its own regression gate once the timing wait is added); Stream 5 by the existing `ExtractLinks_StateUnderTest_ExpectedBehavior` and `FilterLinksByDomain_StateUnderTest_ExpectedBehavior` AngleSharp tests.
- Unit tests (MSTest) for the fixed behavior and boundaries:
  - MSTest framework, Moq for any required mock isolation, FluentAssertions for assertions, per the C# Unit Test Policy. The seven previously failing tests (`GetCallerByName_...`, `GetMethodCallLogString_...`, `GetMethodTraceString_...`, `ExtractLinks_StateUnderTest_ExpectedBehavior`, `FilterLinksByDomain_StateUnderTest_ExpectedBehavior`, `Deserialize_WhenInvalidJsonSimpleOverloadAndPromptDisabled_SerializesCurrentState`) plus the new Stream 3 regression test must pass under `vstest.console.exe`.
- Edge cases and negative scenarios (invalid inputs, missing data, boundary values):
  - `GetCallerMethod` fallback reached with the frame at `frameLevel` belonging to a non-project assembly (the defect scenario) — must return `null`.
  - `GetCallerMethod` with a valid project frame still present — must continue returning that frame (no regression to the positive path).
  - Trace tests run under both the VS / `vstest.console.exe` runner and the VS Code runner must produce identical results (runner-independence is the acceptance bar).
- Error handling and logging verification:
  - Confirm the corrected `GetCallerMethod` fallback does not throw and does not emit a non-project frame name into trace strings; verified by the absence of `mscorlib.dll.InvokeMethod` in the `GetMethodCallLogString_...` test output.
- Coverage impact and targets for changed lines/modules:
  - Repository-wide line coverage must remain >= 80% and not drop below the Phase 0 baseline.
  - Changed lines in `TraceUtility.cs` (`GetCallerMethod` fallback) must not lose coverage; the changed path targets >= 90%.
  - A coverage comparison artifact is produced in the final QA phase.
- Toolchain commands to run (format → lint → type-check → test):
  1. `dotnet tool run csharpier .`
  2. `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
  3. `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true`
  4. `vstest.console.exe <test-assembly-paths> /EnableCodeCoverage` (most relevant: `UtilitiesCS.Test`; the full suite must also pass)
  Restart from step 1 if any step fails or modifies files.
- Manual validation steps (if required):
  - Run `csharpier check .` and confirm exit code 0.
  - Confirm the seven backup/artifact files are removed and `git grep` shows zero references to them in any `.csproj` or `TaskMaster.sln`.


## Acceptance Criteria
- [ ] `dotnet tool run csharpier check .` exits with code 0, with no "Was not formatted" or "invalid xml" findings.
- [ ] The seven backup/artifact files are removed from the repository: `TaskMaster/TaskMaster_BACKUP_1250.csproj`, `TaskMaster/TaskMaster_BASE_1250.csproj`, `TaskMaster/TaskMaster_LOCAL_1250.csproj`, `TaskMaster/TaskMaster_REMOTE_1250.csproj`, `ToDoModel/ToDoItem_Backup.bkp`, `ToDoModel/Data Model/People/PeopleScoDictionaryNewBackup.cs`, `UtilitiesCS/EmailIntelligence/People/PeopleScoDictionaryNewBackup.cs`.
- [ ] `git grep` confirms none of the seven removed files are referenced by `TaskMaster.sln` or any `.csproj` (by file name or class name).
- [ ] `TaskMaster/TaskMaster.csproj` ends with exactly one trailing newline.
- [ ] All seven previously failing tests pass under `vstest.console.exe`: `GetCallerByName_ReturnsMatchingMethodAndHandlesEmptySpecialAndMissingNames`, `GetMethodCallLogString_ShouldIncludeCallerMethodAndParameters`, `GetMethodTraceString_ShouldIncludeCurrentCallChain`, `ExtractLinks_StateUnderTest_ExpectedBehavior`, `FilterLinksByDomain_StateUnderTest_ExpectedBehavior`, `Deserialize_WhenInvalidJsonSimpleOverloadAndPromptDisabled_SerializesCurrentState`.
- [ ] A new regression test in `UtilitiesCS.Test/HelperClasses/TraceUtility_Tests.cs` asserting `GetCallerMethod` does not return a non-project-assembly frame is added, failed before the fix, and passes after.
- [ ] No net regression in the full suite: total passing >= 3989 + 1 (new regression test), 2 skipped, zero failures under `vstest.console.exe`.
- [ ] No unintended behavior changes outside the five defined streams; `TraceUtility`, `TraceExtensions`, and `SCODictionary` public APIs unchanged.
- [ ] The corrected `GetCallerMethod` fallback returns `null` rather than a non-project frame and does not throw.
- [ ] Repository-wide line coverage remains >= 80% and >= the Phase 0 baseline; changed lines in `TraceUtility.cs` retain coverage with the changed path at >= 90%.
- [ ] Full toolchain pass completed in a single clean pass in order: csharpier → analyzers msbuild → nullable msbuild → vstest.
- [ ] `spec.md` and `issue.md` updated to match the new behavior; baseline and final-QA evidence artifacts present under `docs/features/active/2026-05-14-ci-format-and-vs-test-failures-155/evidence/`.

## Risks & Mitigations
- Technical or operational risks:
  - The `GetCallerMethod` fallback change could alter trace-string output for production callers that previously received a non-project frame name. Impact is limited: the prior behavior surfaced uninformative `mscorlib` frames, so returning `null` is a net improvement, and all callers already handle `null`.
  - JIT inlining behavior may still differ on future runner or .NET updates; `[MethodImpl(MethodImplOptions.NoInlining)]` mitigates the known cases but the Trace caller-resolution suite remains structurally sensitive to stack shape.
  - The `SpinWait.SpinUntil` 1-second timeout could still flake on a severely contended CI runner; the timeout matches the existing established pattern in the same test file, so the risk is bounded and consistent with current practice.
  - The Stream 5 `packages.config` edit without a NuGet restore could leave the package lock inconsistent even though the DLL is present on disk.
- Mitigations and rollbacks:
  - Author the Stream 3 regression test first (expect-fail) so the production change is proven to fix the documented defect and nothing else; the full-suite zero-regression gate catches unintended trace-output changes.
  - Run the full toolchain loop and the complete MSTest suite under `vstest.console.exe`; compare against the Phase 0 baseline before declaring completion.
  - Run `nuget restore TaskMaster.sln` (or MSBuild restore) after the `packages.config` edit to keep the lock consistent.
  - Each of the five streams is independently revertible; rollback is a branch revert of `bug/ci-format-and-vs-test-failures-155` with no migration or data impact.
  - If future runner changes reintroduce inlining-related failures, escalate a follow-up issue to redesign the Trace caller-resolution suite rather than widening this fix's scope.

## Rollout & Follow-up
- Release/rollout steps:
- Post-fix monitoring or clean-up tasks:
- Links: issue, PRs, related docs
