# ci-flaky-test-isolation (Spec)

- **Issue:** #176
- **Parent (optional):** none
- **Owner:** drmoisan
- **Last Updated:** 2026-06-08T09-16
- **Status:** Approved
- **Version:** 1.0

## Context
- CI run #197 (databaseId 27138963879), the push-merge of PR #174 into `main`, failed in the "Run MSTest suite with coverage" step. Formatting, .NET analyzers, and nullable analysis all passed.
- Two tests failed intermittently. Both are test-isolation defects surfaced under parallel CI execution, not production regressions. The merging PR (#174) contained only documentation/archive changes.
- Customer impact: `main` CI is red. This blocks tightening branch-protection rules on `main`.
- First observed: 2026-06-08 on `main` after the #174 merge. The same defects exist on `development` (identical code) and can recur there.

## Repro & Evidence
- Failed test 1: `UtilitiesCS.Test.EmailIntelligence.OlFolderClassifierGroup_AdditionalTests.BuildClassifiersAsync_WithFixtureAndFolderConfig_StoresBuiltFolderClassifier`
  - Error: `Expected group.BuiltGroupingKeys {<null>, "Inbox"} to contain {"Inbox", "Projects"}, but could not find {"Projects"}.`
- Failed test 2: `UtilitiesCS.Test.HelperClasses.PhysicalFileSystemAdapters_Tests.PhysicalFileInfoAdapter_PropertiesStreamsAndAccessors_MirrorFileInfo`
  - Error: `System.IO.IOException: The process cannot access the file 'TaskMaster.sln' because it is being used by another process.` at `PhysicalFileInfoAdapter.AppendText()` (test line 206).
- Frequency: intermittent / parallel-execution-dependent. Both pass in isolation and have passed on prior runs.

## Scope & Non-Goals
- In scope: test-isolation fixes in the two affected test files only.
  - `UtilitiesCS.Test/EmailIntelligence/OlFolderClassifierGroup_Tests.cs`
  - `UtilitiesCS.Test/HelperClasses/PhysicalFileSystemAdapters_Tests.cs`
- Out of scope: production code changes; broader refactors of the file-system adapters or classifier groups; the same fixes on `development` (tracked separately).
- Non-goals: weakening assertions, adding sleeps/retries, or marking tests inconclusive to mask the defects.

## Root Cause Analysis
- **Test 1 — non-thread-safe tracking list.** Production `OlFolderClassifierGroup.BuildFolderClassifiersAsync` runs `BuildClassifierAsync` concurrently via `AsyncMultiTasker.AsyncMultiTaskChunker` (OlFolderClassifierGroup.cs:124-133). The test double `TrackingOlFolderClassifierGroup` records each group key into a plain `List<string>` (`BuiltGroupingKeys`, OlFolderClassifierGroup_Tests.cs:229) from inside that concurrent callback (line 241). Concurrent `List<T>.Add` is not thread-safe; interleaved resize/write corrupts the backing array, producing a null slot and a lost element — exactly the observed `{<null>, "Inbox"}`.
- **Test 2 — real shared-file write handle.** `PhysicalFileInfoAdapter_PropertiesStreamsAndAccessors_MirrorFileInfo` opens write/append handles (`AppendText()` line 206, plus `Open(write)` / `OpenWrite()`) against the real `TaskMaster.sln`. Under parallel CI the solution file is held open by another process, so write-mode opens throw `IOException`. This violates the deterministic / no-external-dependency unit-test policy.

## Proposed Fix

### Design summary (what changes where)
- Test 1: make the test double's key-tracking collection thread-safe so concurrent callbacks cannot corrupt it. Use a concurrent collection (e.g., `ConcurrentQueue<string>` / `ConcurrentBag<string>`) or guard `Add` with a lock. The assertion uses `Should().Contain(...)`, which accepts any `IEnumerable<string>`.
- Test 2: move the write/append/open-mode coverage onto a mocked `IFileInfo` (the class already has mock-based delegation tests) so no real shared file is opened for writing. Keep read-only delegation against the real solution file where it is safe and deterministic. Coverage of the write-path delegation must be preserved.

### Boundaries and invariants to preserve
- No production code changes.
- No reduction in assertion strength or branch/line coverage for the adapter or classifier-group code.
- Tests remain MSTest + Moq + FluentAssertions and pass in both the IDE runner and CLI/CI.

### Implementation strategy
#### Files/modules to change
- `UtilitiesCS.Test/EmailIntelligence/OlFolderClassifierGroup_Tests.cs`
- `UtilitiesCS.Test/HelperClasses/PhysicalFileSystemAdapters_Tests.cs`

## Test Strategy
- The change is test-only; the existing tests are the spec. After the fix both must pass deterministically.
- Re-run the full C# toolchain: csharpier -> analyzers -> nullable -> MSTest with coverage.
- Confirm no coverage regression on changed lines for the affected production types.

## Acceptance Criteria
- [x] AC1: `BuildClassifiersAsync_WithFixtureAndFolderConfig_StoresBuiltFolderClassifier` passes deterministically under parallel execution (no null/lost keys). Tracking store changed to `ConcurrentBag<string>`; verified 5/5 coverage runs of the affected-class set, 14/14 pass each.
- [x] AC2: `PhysicalFileInfoAdapter_PropertiesStreamsAndAccessors_MirrorFileInfo` no longer opens a write/append/read-write handle on the real `TaskMaster.sln`, and creates no temporary/scratch file. Read-only members stay on the `.sln`; write-mode members (`AppendText`, `Open(FileMode.Open)`, `OpenWrite`) are covered through a narrow injectable-delegate production seam on `PhysicalFileInfoAdapter` (a new `internal` constructor accepting `Func<StreamWriter>`/`Func<FileMode,FileStream>`/`Func<FileStream>`). The test constructs the seam-injected adapter with test-owned sentinel streams (read-only opens of the test assembly DLL; an in-memory stream backs the append `StreamWriter`) and asserts delegation with `BeSameAs`. This preserves coverage of the adapter's write-mode delegation lines without acquiring a real write/append handle and without any scratch file. The public constructor binds the three delegates to the wrapped `FileInfo`, so production behavior is unchanged.
- [x] AC3: A narrow, behavior-preserving production seam was added to `PhysicalFileInfoAdapter.cs` (a new `internal` constructor and three private delegate fields); the public constructor's runtime behavior is unchanged. The two test files were changed as planned. Confirmed scope via `git diff --name-only`: `UtilitiesCS/HelperClasses/FileSystem/PhysicalFileInfoAdapter.cs`, `UtilitiesCS.Test/HelperClasses/PhysicalFileSystemAdapters_Tests.cs`, `UtilitiesCS.Test/EmailIntelligence/OlFolderClassifierGroup_Tests.cs`.
- [x] AC4: No assertions weakened; no sleeps/retries/timing hacks added.
- [x] AC5: Full toolchain pass completed (csharpier -> analyzers -> nullable -> MSTest with coverage). See QA-GATE.md.
- [x] AC6: No coverage regression. `PhysicalFileInfoAdapter.cs` per-file cobertura line-rate rose from baseline 0.8909 to 0.9155 (the new internal constructor is fully covered); the three write-mode delegation members (`AppendText`, `Open(FileMode.Open)`, `OpenWrite`) remain hit. Other affected files unchanged from baseline. Evidence: `evidence/qa-gates/2026-06-08T13-58-59Z/postchange.cobertura.xml`.
- [ ] AC7: PR CI on `main` is green; the post-merge `main` CI is green. (Pending PR/CI; not verifiable locally.)

### Verification deviation from the original proposed fix
The Proposed Fix anticipated routing write-mode coverage through a mocked `IFileInfo`. That is infeasible for `PhysicalFileInfoAdapter` because its public constructor takes a concrete `FileInfo`. An earlier revision exercised the write-mode members against a private, test-owned scratch file; that approach was rejected because creating a temporary file on the local filesystem violates the unit-test policy (CLAUDE.md UT4 and `.claude/rules/general-unit-test.md`). The accepted approach adds a narrow injectable-delegate seam to `PhysicalFileInfoAdapter` (a new `internal` constructor accepting `Func<StreamWriter>`/`Func<FileMode,FileStream>`/`Func<FileStream>`, with the public constructor binding the defaults to the wrapped `FileInfo`). The test injects test-owned sentinel streams and asserts delegation with `BeSameAs`, preserving coverage of the adapter's write-mode lines without any real write/append handle and without any scratch file. Production runtime behavior is unchanged. Evidence: `evidence/baseline/2026-06-08T13-21-38Z/` and `evidence/qa-gates/2026-06-08T13-58-59Z/`.

## Risks & Mitigations
- Risk: reworking Test 2 reduces coverage of the real-file delegation paths. Mitigation: retain read-only real-file delegation and add equivalent mock-based coverage for write-mode methods.
- Risk: the same flaky tests recur on `development`. Mitigation: port the fix to `development` as a follow-up (per base-branch decision, `main` is fixed first).

## Rollout & Follow-up
- Open PR into `main`; iterate until CI is green.
- Follow-up: apply the same fix to `development` to prevent reintroduction on the next `development`->`main` merge.
- Links: issue #176; CI run 27138963879.
