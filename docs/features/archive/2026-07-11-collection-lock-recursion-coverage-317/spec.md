# collection-lock-recursion-coverage-317 (Spec)

- **Issue:** #317
- **Parent (optional):** none
- **Owner:** drmoisan
- **Last Updated:** 2026-07-11T19-27
- **Status:** Draft
- **Version:** 0.1

## Context
- During epic child F5 (#308, swordfish-interface-project-teardown), WI-4 deleted
  `UtilitiesCS.Test/ReusableTypeClasses/Concurrent/Observable/Collection/ConcurrentObservableCollectionLockRecursionTests.cs`
  along with two genuinely Swordfish-bound test files. This file was not Swordfish-bound: F2
  (#307) had already re-expressed it against the clean, first-party
  `ConcurrentObservableCollection<T>` base in task P4-T7. F5's own AC-12 preflight recognized this
  and, per its documented scope boundary, raised this issue rather than authoring replacement
  coverage for a type it does not own.
- Observed environment(s): repository build/test only; no runtime/production impact.
- Customer impact and severity: none — this is a regression-coverage gap, not a functional defect.
  No end user is affected today because the hazard the deleted test guarded against (a
  `LockRecursionException` on re-entrant `Count`/`NewItems` reads inside a synchronous
  `CollectionChanged` handler) cannot occur on the current lock-free `ObservableCollection<T>`-based
  implementation. The gap is exposure to a *future* silent regression if a lock is ever
  reintroduced.
- First observed: 2026-07-11, raised by F5 execution (#308) per its AC-12 gate.

## Repro & Evidence
- Not applicable in the traditional sense (no failing behavior to reproduce today). The "repro" is
  a coverage gap: `rg` across `UtilitiesCS.Test/` for `LockRecursion` / `CollectionChanged`+`DoesNotThrow`
  combinations returns zero matches (confirmed in research), where before F5's deletion it returned
  two test methods.
- Expected vs actual: expected — a test asserts that reading `Count` (or `e.NewItems`) from inside a
  `CollectionChanged` handler fired synchronously during `Add` does not throw. Actual — no such test
  exists after the F5 deletion.
- Frequency/determinism: the coverage gap is permanent (not intermittent) until fixed; the tests
  themselves, once restored, are fully deterministic.

## Scope & Non-Goals
- In scope: restore the two lock-recursion regression tests against the clean
  `UtilitiesCS.ReusableTypeClasses.Concurrent.Observable.Collection.ConcurrentObservableCollection<T>`
  base, plus the matching `<Compile Include>` csproj entry.
- Out of scope / non-goals: any change to `ConcurrentObservableCollection<T>` production code (the
  API surface is already confirmed unchanged and lock-free); re-litigating F5's WI-4 deletion of the
  other two files (`ObservableDictionary_Tests.cs`, genuinely Swordfish-bound, and
  `ConcurrentObservableCollectionSenderTests.cs`, whose sender-identity coverage already survives in
  `ConcurrentObservableCollection_Tests.cs`); any change to the swordfish-removal epic's merged work.
- Explicitly excluded: no changes to `UtilitiesCS/UtilitiesCS.csproj`, `TaskMaster.sln`, or any
  production `.cs` file.

## Root Cause Analysis
- Confirmed root cause: epic child F5 (#308) deleted a test file as part of its WI-4 "remove
  direct-Swordfish tests" work item. The file was not actually Swordfish-bound — F2 (#307) had
  already re-expressed it against the clean collection base in a prior feature. F5's own AC-12 audit
  correctly identified this and, following its documented scope boundary ("F5 does not author
  regression coverage against a clean type it does not own"), raised this issue instead of silently
  either keeping the file or writing new tests outside its scope.
- Signals/evidence: F5's own evidence artifacts (`f2-regression-coverage-confirmation.md`,
  `wi4-test-swordfish-zero.md`), a persistent atomic-executor memory note recorded during F5's run,
  and F2's own atomic plan (task P4-T7, the origin of the deleted file's content) all independently
  corroborate this — see `research/research.2026-07-11T21-15.md`.
- Affected components: `UtilitiesCS.Test` project only; the production
  `ConcurrentObservableCollection<T>` type is untouched and its API surface confirmed unchanged.

## Proposed Fix

### Design summary (what changes where):
Restore the deleted test file's content (recovered via `git show` against the pre-deletion commit),
normalize its namespace to match its two living siblings in the same folder
(`UtilitiesCS.Test.ReusableTypeClasses.Concurrent.Observable.Collection`, since the original used the
older non-folder-mirroring `ConcurrentObservableCollection.Tests` convention), and re-add the single
`<Compile Include>` line removed from `UtilitiesCS.Test.csproj` in the same F5 commit.

### Boundaries and invariants to preserve:
No production code change. No other test file touched. No change to `UtilitiesCS.csproj`,
`TaskMaster.sln`, or any other project file.

### Dependencies or blocked work:
None — the clean `ConcurrentObservableCollection<T>` base (F2, #307) is already merged to `main`.

### Implementation strategy (what changes, not sequencing):

#### Files/modules to change:
- Restore: `UtilitiesCS.Test/ReusableTypeClasses/Concurrent/Observable/Collection/ConcurrentObservableCollectionLockRecursionTests.cs`
- Edit (1 line): `UtilitiesCS.Test/UtilitiesCS.Test.csproj`

#### Functions/classes/CLI commands impacted:
Two `[TestMethod]`s: `Add_WhenCollectionChangedHandlerReadsCountFromCollection_DoesNotThrow`,
`Add_WhenCollectionChangedHandlerUsesNewItemsFromEventArgs_DoesNotThrow`. No production classes or
CLI commands impacted.

#### Data flow and validation changes:
None.

#### Error handling and logging updates:
None.

#### Rollback/feature-flag considerations (if applicable):
Not applicable — test-only restoration, trivially revertible.

### Technical specifications (interfaces/contracts):

#### Inputs/outputs and formats:
Not applicable (no interface/contract change).

#### Required configuration keys and defaults:
None.

#### Backward-compatibility expectations:
None — no public API touched.

#### Performance constraints (latency/throughput/memory):
None applicable.

## Assumptions, Constraints, Dependencies
- Assumptions: the pre-deletion file content (recoverable via `git show 0ec111b2~1:<path>`) remains
  the correct target content, with only the namespace normalized to match its living siblings.
- Constraints: MSTest + Moq + FluentAssertions per repository policy; no temp files; deterministic,
  isolated tests.
- External dependencies: none.

## Data / API / Config Impact
- User-facing or API changes: none.
- Data or migration considerations: none.
- Logging/telemetry updates: none.
- Compatibility notes: none.

## Test Strategy
- Regression tests to add: `ConcurrentObservableCollectionLockRecursionTests.cs` (restored), two
  `[TestMethod]`s as named above, using FluentAssertions (`Should().NotThrow()`), MSTest attributes,
  no mocks needed (concrete `ConcurrentObservableCollection<int>` under test).
- Edge cases and negative scenarios: not applicable — this is a guard-rail regression test for a
  hazard that cannot occur today by construction; both methods assert the safe (non-throwing) path.
- Error handling and logging verification: not applicable.
- Coverage impact and targets: expected net-neutral on production line/branch coverage (the restored
  tests re-exercise `Add`/`OnCollectionChanged`/`Count`, already covered by the surviving sibling
  test file); no new production surface is introduced. Verify via baseline-vs-final
  `vstest.console.exe /EnableCodeCoverage` diff per standard practice, not because a regression is
  expected.
- Toolchain commands to run (format → lint → type-check → test):
  1. `dotnet tool run csharpier .` (or `csharpier .`)
  2. `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
  3. `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true`
  4. `vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /EnableCodeCoverage`
- Manual validation steps: none required.

## Acceptance Criteria
- [x] AC-1: `ConcurrentObservableCollectionLockRecursionTests.cs` exists at its original path,
      containing `Add_WhenCollectionChangedHandlerReadsCountFromCollection_DoesNotThrow` and
      `Add_WhenCollectionChangedHandlerUsesNewItemsFromEventArgs_DoesNotThrow`, both passing.
      (evidence: `evidence/regression-testing/restored-tests-pass.2026-07-11T20-07.md`,
      `evidence/qa-gates/post-change-test-coverage.2026-07-11T20-25.md`)
- [x] AC-2: The restored file's namespace is
      `UtilitiesCS.Test.ReusableTypeClasses.Concurrent.Observable.Collection`, matching its two
      living siblings in the same folder.
      (evidence: `evidence/regression-testing/namespace-verification.2026-07-11T20-09.md`)
- [x] AC-3: `UtilitiesCS.Test/UtilitiesCS.Test.csproj` carries the matching `<Compile Include>` entry.
      (evidence: `evidence/regression-testing/csproj-wiring-verification.2026-07-11T20-10.md`)
- [x] AC-4: No production file is modified; a repo-wide diff against `main` shows only the two files
      above touched.
      (evidence: `evidence/regression-testing/repo-wide-diff-scope.2026-07-11T20-12.md`)
- [x] AC-5: Full C# toolchain passes in a single final pass (csharpier → analyzers →
      nullable/TreatWarningsAsErrors → MSTest via vstest), with zero test regressions and no
      coverage regression on changed lines.
      (evidence: `evidence/qa-gates/csharpier-check.2026-07-11T20-15.md`,
      `evidence/qa-gates/post-change-analyzer-build.2026-07-11T20-17.md`,
      `evidence/qa-gates/post-change-nullable-build.2026-07-11T20-20.md`,
      `evidence/qa-gates/post-change-test-coverage.2026-07-11T20-25.md`,
      `evidence/qa-gates/coverage-delta-verification.2026-07-11T20-27.md`)

## Risks & Mitigations
- Technical risk: namespace normalization could be mis-typed, breaking compilation. Mitigation:
  verify via a clean build immediately after the edit.
- Operational risk: none — test-only change, trivially revertible.

## Rollout & Follow-up
- Release/rollout steps: standard PR → CI → merge to `main`.
- Post-fix monitoring or clean-up tasks: none.
- Links: issue #317; originating epic child F5 (#308); originating feature F2 (#307).
