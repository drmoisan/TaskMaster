# Remediation Inputs — Cycle 5 (Issue #181)

Entry timestamp: 2026-06-08T21-53
Feature folder: docs/features/active/2026-06-08-csharp-analyzer-stack-hardening-181
Branch: feature/csharp-analyzer-stack-181
Base: main
Supersedes: remediation-inputs.2026-06-08T21-23.md (cycle 4, marked failed at a scope-change halt)

## Trigger

Cycle 4 executed Phase 0 plus the first fail-before task, then halted at P1-T2 under the
Scope-Change Rule. The cycle-4 premise — that Finding B (the People deserialization defect)
is fixable inside `FilePathHelper.cs` — was disproven by deterministic, fully-reverted
diagnostics captured in
`evidence/regression-testing/scope-change-finding-B.2026-06-08T21-23.md`. Cycle 5 carries
the corrected Finding B root cause and an expanded production budget. Findings A and C are
unchanged and remain in their original files.

## In-Scope Failing Tests (exactly four — unchanged)

1. `FromSeed_ShouldBuildFileNameFromParts` — `UtilitiesCS.Test/HelperClasses/FilePathHelper_Tests.cs`
2. `CalcMaxSeedLength_WhenInitialized_ShouldSubtractComponentLengths` — `UtilitiesCS.Test/HelperClasses/FilePathHelper_Tests.cs`
3. `People_Deserialize_CanDeserializePatternCorrectly` — `ToDoModel.Test/Data Model/People/PeopleScoDictionaryNewTests.cs`
4. `Consume_WhenSequenceProvided_ReturnsItemsAndReportsProgress` — `UtilitiesCS.Test/EmailIntelligence/SubjectMapSco_Orchestration_Tests.cs`

## Root-Cause Findings

### Finding A — FilePathHelper seed-constructor PropertyChanged re-entry (tests 1 and 2) — IN BUDGET, UNCHANGED

Confirmed correct in cycle 4. In `UtilitiesCS/HelperClasses/FileSystem/FilePathHelper.cs`,
commit `0883d0f7` moved `PropertyChanged += FilePathHelper_PropertyChanged;` to the first
line of the private seed constructor `FilePathHelper(fileNameSeed, fileExtension,
fileNameSuffix, folderPath)`. Its terminal `FilePath = Path.Combine(_folderPath, _fileName);`
now re-enters the `FilePath` handler case; because `_fileName` is empty at that point, the
combined value is the folder itself and the handler rewrites `_folderPath`/`_fileName`,
corrupting `FolderPath` (`C:\data` -> `C:\`). Fix: remove or guard the redundant terminal
`FilePath = Path.Combine(...)` assignment in the seed constructor (the setters already
recompute `_filePath` via the handler). This satisfies tests 1 and 2 and regresses no other
`FilePathHelper_Tests` case. Touch point: `FilePathHelper.cs`.

### Finding B — People deserialization defect (test 3) — CORRECTED ROOT CAUSE, EXPANDED BUDGET

Cycle-4 evidence (`scope-change-finding-B.2026-06-08T21-23.md`) established, with reverted
diagnostics:

- `new FilePathHelper("pplkey.json", "C:\\Users\\<user>\\AppData\\Roaming")` already yields
  `FileName == "pplkey.json"`. The `(fileName, folderPath)` constructor is not the defect.
- `FilePathHelperConverter.ReadJson` is never invoked for `Disk` in this test (a temporary
  `DIAGPROBE`, since reverted, logged zero hits while the test still failed).
- Real root cause is in the serialization layer:
  `UtilitiesCS/NewtonsoftHelpers/ScoDictionaryConverter.cs` →
  `UtilitiesCS/NewtonsoftHelpers/WrapperScoDictionary.cs`. `WrapperScoDictionary` declares
  `RemainingObject` as `object`; under the test's `TypeNameHandling.None`, Newtonsoft binds
  it to an untyped `JObject`, so `WrapperScoDictionary.ToDerived()`'s reflection for the
  `Config` member returns null and `Config` is left at its default
  (`new NewSmartSerializableConfig()` with an empty `Disk`). The sibling test
  `People_DeserializeShortcut_CanDeserializePatternCorrectly` passes via the different
  `PeopleScoConverter` path and is the working reference.

Required outcome: deserialization through `ScoDictionaryConverter` /
`WrapperScoDictionary.ToDerived()` under `TypeNameHandling.None` correctly reconstitutes
`Config` so `people.Config.Disk.FileName == "pplkey.json"`, without a temp file, without
weakening the assertion, and without regressing the working `PeopleScoConverter`/shortcut
path or any other serialization consumer. The planner must verify the exact reflection /
type-binding failure in `WrapperScoDictionary.ToDerived()` against the cycle-4 baseline
evidence before choosing the minimal fix (for example correct typed reconstruction of the
`Config` member from the `JObject`, or correct generic-type handling in
`ScoDictionaryConverter`). Touch points authorized for Finding B:
`UtilitiesCS/NewtonsoftHelpers/WrapperScoDictionary.cs` and/or
`UtilitiesCS/NewtonsoftHelpers/ScoDictionaryConverter.cs`.

### Finding C — Consume timer-dependent test (test 4) — IN BUDGET, UNCHANGED

`Consume_WhenSequenceProvided_ReturnsItemsAndReportsProgress` depends on `Thread.Sleep(20)`
plus `SpinWait.SpinUntil(... 1s)` and is non-deterministic under full-suite load (passes in
isolation). Per cycle-4 analysis, `SubjectMapSco.Consume<T>` reports progress once eagerly
plus via a wall-clock timer, while the per-item `WithProgressReporting` callback only updates
a local counter. Fix: report progress per item through the existing injected `progress`
tracker (deterministic `Reports.Count >= 2` with a `JobName` starting `"Consuming "`),
removing the wall-clock dependency. No sleeps/retries/timing slack; no banned symbol
introduced. Touch point: `UtilitiesCS/EmailIntelligence/SubjectMap/SubjectMapSco.Orchestration.cs`.

## Authorized Production Budget (expanded vs. cycle 4)

1. `UtilitiesCS/HelperClasses/FileSystem/FilePathHelper.cs` (Finding A)
2. `UtilitiesCS/EmailIntelligence/SubjectMap/SubjectMapSco.Orchestration.cs` (Finding C)
3. `UtilitiesCS/NewtonsoftHelpers/WrapperScoDictionary.cs` and/or
   `UtilitiesCS/NewtonsoftHelpers/ScoDictionaryConverter.cs` (Finding B — NEW this cycle)

Plus the carried-forward, csharpier-clean working-tree formatting fix to
`ToDoModel.Test/Data Model/ToDo/ToDoItemTests.cs`. Expand beyond this list only by halting
and opening a new cycle (Scope-Change Rule). If the Finding B fix proves to require a file
outside touch points 1-3, HALT — do not widen scope in place.

## Constraints (hard — unchanged from cycle 4)

- Allowed delegates this cycle: `atomic-planner`, `atomic-executor`, `feature-review` only.
  No direct typed-engineer worker invocation by the orchestrator.
- Do not re-add `[Ignore("ProductionBugSuspected")]` to any of the four tests. Do not weaken
  or delete assertions. Do not add sleeps/retries/timing hacks.
- No analyzer-config, `.editorconfig`/`.globalconfig`, vendored-project, `BannedSymbols.txt`,
  or `.claude/rules/` changes.
- C# toolchain order is mandatory and must pass in a single final pass: `dotnet tool run
  csharpier .` -> msbuild analyzers -> msbuild nullable/TreatWarningsAsErrors -> vstest with
  coverage. Restart from csharpier if any step changes files or fails.
- Zero regression across the full first-party suite (pre-`0883d0f7` baseline `aa11e036` was
  CI-green; 8 pre-existing flaky wall-clock-timer tests that pass in isolation are out of
  scope and must not be modified). Required CI check must be green against branch head after
  push.
- Do not relocate or reorganize existing committed feature-folder artifacts. Keep the
  canonical flat artifact layout (`<feature-folder>/<artifact>.<ts>.md`).

## Acceptance for Cycle Exit

- All four named tests pass deterministically in both IDE and CLI runs.
- Full toolchain passes in one final pass with zero new analyzer/nullable errors.
- No currently-passing test regressed; the `PeopleScoConverter`/shortcut deserialization
  path still passes.
- Three reaudit artifacts (code-review, feature-audit, policy-audit) produced by
  `feature-review` with `blocking_count == 0`.
- Required CI check green against branch head after push.
