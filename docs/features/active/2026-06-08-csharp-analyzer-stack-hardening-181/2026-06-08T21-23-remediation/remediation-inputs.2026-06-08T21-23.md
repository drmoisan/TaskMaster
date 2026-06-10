# Remediation Inputs — Cycle 4 (Issue #181)

Entry timestamp: 2026-06-08T21-23
Feature folder: docs/features/active/2026-06-08-csharp-analyzer-stack-hardening-181
Branch: feature/csharp-analyzer-stack-181
Base: main

## Trigger

User direction to fix four failing tests, resolving the cycle-3 fix-vs-revert fork
toward the **fix** path. Cycle 3 halted at a scope-change escalation (re-enabled
regression test `People_Deserialize_CanDeserializePatternCorrectly` failed on an unfixed
production defect, plus FilePathHelper path-test regressions introduced by branch commit
`0883d0f7`). This cycle authorizes the production fix and the deterministic-test fix for
exactly the four named tests.

## In-Scope Failing Tests (exactly four)

1. `FromSeed_ShouldBuildFileNameFromParts` — `UtilitiesCS.Test/HelperClasses/FilePathHelper_Tests.cs`
2. `CalcMaxSeedLength_WhenInitialized_ShouldSubtractComponentLengths` — `UtilitiesCS.Test/HelperClasses/FilePathHelper_Tests.cs`
3. `People_Deserialize_CanDeserializePatternCorrectly` — `ToDoModel.Test/Data Model/People/PeopleScoDictionaryNewTests.cs`
4. `Consume_WhenSequenceProvided_ReturnsItemsAndReportsProgress` — `UtilitiesCS.Test/EmailIntelligence/SubjectMapSco_Orchestration_Tests.cs`

## Root-Cause Findings

### Finding A — FilePathHelper constructor PropertyChanged ordering regression (tests 1 and 2)

Commit `0883d0f7` moved `PropertyChanged += FilePathHelper_PropertyChanged;` to the FIRST
line of both the `FilePathHelper(string fileName, string folderPath)` constructor and the
private seed constructor `FilePathHelper(fileNameSeed, fileExtension, fileNameSuffix, folderPath)`
in `UtilitiesCS/HelperClasses/FileSystem/FilePathHelper.cs`.

Consequence: the terminal assignment `FilePath = Path.Combine(_folderPath, _fileName);` now
re-enters `FilePathHelper_PropertyChanged` on the `FilePath` case, which recomputes
`_folderPath = Path.GetDirectoryName(_filePath)` and `_fileName = Path.GetFileName(_filePath)`.
For the seed constructor `_fileName` is empty at that point, so the combined path is the
folder itself (`C:\data`), and the handler rewrites `FolderPath` to `C:\` and `FileName` to
`data`. This corrupts the seed-derived state the two tests assert:

- `FromSeed_ShouldBuildFileNameFromParts` expects `FolderPath == @"C:\data"` (now `C:\`).
- `CalcMaxSeedLength_WhenInitialized_ShouldSubtractComponentLengths` expects
  `MAX_PATH - @"C:\output".Length - ".json".Length - "_bk".Length`; the corrupted
  `FolderPath` length breaks the subtraction.

Required outcome: both constructors initialize derived path state correctly AND the
deserialization/init path that motivated `0883d0f7` continues to work (see Finding B).
The planner must determine the correct initialization sequence rather than a blind revert,
because the reorder was introduced to fix the People deserialization path. Both behaviors
must hold simultaneously. The constructor tests are part of the spec (CLAUDE.md §7.3) and
must not be weakened.

### Finding B — People deserialization defect (test 3)

`People_Deserialize_CanDeserializePatternCorrectly` deserializes `Resources.pplkey` JSON via
`SmartSerializableNonTyped.DeserializeObject<PeopleScoDictionaryNew>` with a
`ScoDictionaryConverter<PeopleScoDictionaryNew, string, string>` and `TypeNameHandling.None`,
then asserts `people.Config.Disk.FileName == "pplkey.json"`. Observed: `Config.Disk.FileName`
is empty (length 0). `Config.Disk` is a `FilePathHelper`. The defect is in how the
`FilePathHelper` (or its containing config) reconstitutes `FileName`/`FolderPath`/`FilePath`
during JSON deserialization — property set order during deserialization interacts with the
`PropertyChanged` handler. The sibling test `People_DeserializeShortcut_CanDeserializePatternCorrectly`
(uses `PeopleScoConverter`) is not ignored and is expected to pass; the planner should use it
as a reference for the working path.

Required outcome: deserialization yields `Config.Disk.FileName == "pplkey.json"` without a
temp file and without weakening the assertion. Coordinate with Finding A so the chosen
FilePathHelper initialization satisfies both the direct-constructor tests and the
deserialization path.

### Finding C — Consume timer-dependent test (test 4)

`Consume_WhenSequenceProvided_ReturnsItemsAndReportsProgress` spaces a 3-element sequence with
`Thread.Sleep(20)` and asserts via `SpinWait.SpinUntil(() => tracker.Reports.Count >= 2,
TimeSpan.FromSeconds(1))`. This is wall-clock-timer dependent and flaky in CLI/CI runs (per
the documented flaky-timer test class). `Thread.Sleep` is also a banned symbol (RS0030,
currently `suggestion`). Per `.claude/rules/csharp.md` "Deterministic Test Rules" and
"Prohibited Behaviors", the fix must make the test deterministic through a seam (for example
removing the wall-clock dependency or driving progress reporting through an injected clock /
deterministic enumeration), NOT by adding sleeps, retries, or timing slack. Do not weaken the
"reports progress at least twice" intent.

## Constraints (hard)

- Allowed delegates this cycle: `atomic-planner`, `atomic-executor`, `feature-review` only.
  No direct typed-engineer worker invocation by the orchestrator.
- Do not re-add `[Ignore("ProductionBugSuspected")]` to any of the four tests. Do not weaken
  or delete assertions. Do not add sleeps/retries/timing hacks.
- No analyzer-config, `.editorconfig`/`.globalconfig`, vendored-project, `BannedSymbols.txt`,
  or `.claude/rules/` changes in this cycle (those are the prior feature scope and are GREEN).
- C# toolchain order is mandatory and must pass in a single final pass: csharpier ->
  msbuild analyzers -> msbuild nullable/TreatWarningsAsErrors -> vstest with coverage.
- Zero regression: the full suite must not lose any currently-passing test. The pre-`0883d0f7`
  baseline (commit `aa11e036`) was CI-green; the end state of this cycle must be green on the
  required CI check after push.
- Production file change budget should stay minimal and targeted (bugfix workflow,
  CLAUDE.md "Bugfix Workflow"): the likely production touch points are
  `UtilitiesCS/HelperClasses/FileSystem/FilePathHelper.cs` and the SubjectMapSco `Consume`
  production path; expand only if root cause requires it.
- Carry forward the uncommitted cycle-3 working-tree formatting fix to
  `ToDoModel.Test/Data Model/ToDo/ToDoItemTests.cs` (misaligned commented attribute) and
  ensure csharpier-clean output for it as part of this cycle.

## Acceptance for Cycle Exit

- All four named tests pass deterministically in both IDE and CLI runs.
- Full toolchain passes in one final pass with zero new analyzer/nullable errors.
- No currently-passing test regressed.
- Three reaudit artifacts (code-review, feature-audit, policy-audit) produced by
  `feature-review` with `blocking_count == 0`.
- Required CI check green against branch head after push.
