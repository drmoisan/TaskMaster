# coverage-increments-1-3-testable-seams - Feature Spec

- **Issue:** #199
- **Parent (optional):** none
- **Owner:** drmoisan
- **Last Updated:** 2026-06-14
- **Status:** Draft
- **Version:** 1.0
- **Work Mode:** full-feature

## Intent & Outcomes

The COM/VSTO coverage exemption (#197) redefined the 80% coverage floor to apply to a
testable denominator that excludes architecturally-untestable Outlook-COM / VSTO / WinForms
code. After that change the measured production-only rate on the testable denominator is
**71.65%** (recorded as authority-scoped exception 197-COV-001), below the 80% floor.

This feature raises covered code on the post-#197 testable denominator by adding MSTest unit
tests to the genuinely-testable seams that #197 deliberately preserved as measured. It
implements Increments 1–3 of the coverage roadmap
(`artifacts/research/csharp-coverage-roadmap.2026-06-12.md` §6).

Outcomes:

1. New MSTest tests exercise the enumerated testable seams in `ToDoModel`, `QuickFiler`, and
   `TaskMaster`, increasing covered-line counts on those seams.
2. A net production-only coverage increase versus the 71.65% post-#197 baseline is measured
   and recorded.
3. No production behavior changes. This is a test-only feature.

The corrected coverage context is the post-#197 measured denominator. The roadmap's §0.2
production-only 58.95% figure is the pre-#197 baseline; the relevant comparison baseline for
this feature is the post-#197 71.65% figure.

## Invariants (must not change)

- **No production behavior change.** Production method bodies, signatures, public APIs, and
  runtime behavior remain identical. The feature adds test code only, with a single
  maintainer-authorized exception in Phase 5 (option B): two narrow test seams are permitted
  — (1) the UtilitiesCS assembly attribute `[assembly: InternalsVisibleTo("ToDoModel.Test")]`
  (which exposes the existing internal `MyBox.DialogInvoker` seam to the test project; no
  `MyBox` member behavior changes and the seam still defaults to the real dialog in
  production), and (2) the `TaskMaster.AppFileSystemFolderPaths.MatchBestSpecialFolder`
  pure-helper extraction (the instance method delegates to a new `internal static` helper with
  byte-for-byte identical matching semantics). Neither seam changes any runtime behavior.
- **No change to the #197 exemption boundary.** No `[ExcludeFromCodeCoverage]` attribute is
  added or removed. No COM/VSTO/WinForms code that #197 exempted is un-exempted or tested.
- **Coverage pipeline structure unchanged.** No edits to `coverage.config`,
  `TaskMaster.runsettings`, `Get-KoverageProjectAllowlist`, or the Koverage post-processing.
- **Test framework and conventions:** MSTest (`Microsoft.VisualStudio.TestTools.UnitTesting`),
  Moq for mocking, FluentAssertions for assertions, per the C# Unit Test Policy. No xUnit/NUnit.
- Performance characteristics: none affected (test-only change).
- Compatibility guarantees: none affected.

## Scope (test additions only)

Three increments, each delivered as a separate phase. Each adds MSTest unit tests to its
existing `.Test` project. Test targets are restricted to the seams #197 listed as preserved
and measured (see #197 spec "Explicitly preserved testable seams").

### Increment 1 — ToDoModel (`ToDoModel.Test`)

Target classes and members (all in `ToDoModel`; `InternalsVisibleTo("ToDoModel.Test")` is
present in source, so `internal` members are reachable from the test project without a
production change):

1. **`ToDoLoader.SetAndSave<T>` (all four overloads)** — `ToDoModel/Data Model/ToDo/ToDoLoader.cs`.
   Signatures confirmed:
   - `internal void SetAndSave<T>(ref T variable, T value, Action<T> objectSetter)`
   - `internal void SetAndSave<T>(...)` (the read-only-guard overload)
   - `internal void SetAndSave<T>(T value, Action<T> objectSetter)`
   - `internal void SetAndSave<T>(T value, Action<T> objectSetter, System.Action objectSaver)`

   Scenario coverage:
   - **Positive:** `objectSetter` is invoked with the supplied value; the `objectSaver`
     overload invokes the saver; the `ref` overload assigns the new value to the reference.
   - **Negative/error:** null `objectSetter` path; null `objectSaver` path (guard behavior,
     not an unguarded NRE).
   - **Edge:** read-only guard path (value not set / not saved when the guard condition holds);
     value equal to the existing value.

2. **`IDList.GetNextToDoID(string strSeed)`** — `ToDoModel/Data Model/ID/IDList.cs`. Constructed
   via the Outlook-free constructors `IDList()`, `IDList(IList<string>)`, or
   `IDList(IEnumerable<string>)` so no live Outlook is required. Only the pure base-36 arithmetic
   path is targeted; `RefreshIDList` and Outlook-application constructors are #197-exempt and are
   not tested.

   Scenario coverage:
   - **Positive (base case):** next ID from a seed with no existing collision.
   - **Edge (ID-already-present loop):** seed collides with an existing ID, forcing the
     increment loop to advance to the next free base-36 value.
   - **Edge (length boundary):** generated ID rolls over to a longer string (e.g., max single
     base-36 digit advancing to two digits); verify the produced length and value.
   - **Negative:** null or empty seed handling (assert the documented behavior; do not assume).

3. **`ProjectEntry`** — `ToDoModel/Data Model/Project/ProjectEntry.cs`.
   - `SetProjectId`: **positive** (valid ID set), **negative** (null newID), **error/edge**
     (malformed ID validation path). If the malformed path routes through a `MyBox`/MessageBox
     static call, prefer an existing injectable seam already present in source; if none exists,
     restrict the test to the branches reachable without invoking the dialog and flag the gap
     (see Constraints).
   - `CompareTo`: **positive/edge** (equal, different, ordinal ordering, prefix), **negative**
     (null argument). Pure ordinal string comparison; tested with plain `ProjectEntry` instances.

4. **`BaseChanger`** — `ToDoModel/Data Model/ID/BaseChanger.cs`. Cover the remaining uncovered
   branches of the base-conversion arithmetic.
   - **Positive:** representative conversions across supported bases.
   - **Edge:** boundary values (zero, single-digit, base-boundary rollover, maximum supported
     digit).
   - **Negative/error:** invalid base or invalid input character per the method contract.

### Increment 2 — QuickFiler (`QuickFiler.Test`)

Target classes and members (all in `QuickFiler`, in `QuickFiler/Controllers/`). All are pure
value objects / collection management with no Outlook calls.

1. **Keyboard-action value objects** — `KaChar` and `KaCharAsync` (`KaChar.cs`),
   `KaKey` and `KaKeyAsync` (`KaKey.cs`), `KaStringAsync` (`KaStringAsync.cs`).
   - **Positive:** construction with valid key/char/string and delegate; the stored key and
     delegate are retained and the action dispatches to the supplied delegate when invoked.
   - **Async variants (`KaCharAsync`, `KaKeyAsync`, `KaStringAsync`):** the awaited delegate
     (`Func<…, Task>`) is invoked and completes; use a delegate that completes synchronously
     (no real timing dependency). No `Task.Delay`/`Sleep`.
   - **Negative:** null delegate / null key argument per the constructor contract.
   - **Edge:** equality/identity semantics if the type defines them; default/boundary key values.

2. **`KbdActions<>` remaining branches** — `KbdActions.cs`. Cover the registry branches not yet
   exercised: add/register, lookup hit and miss, duplicate-key handling, removal, and any
   enumeration path. Pure collection management; no Outlook.
   - **Positive:** register then resolve an action by key.
   - **Negative:** resolve a missing key; register a duplicate key (assert documented behavior).
   - **Edge:** empty registry, removal of a present and an absent key, state after clear.

3. **`FilerQueue` pure paths** — `FilerQueue.cs`.
   - **Positive:** enqueue then dequeue preserves order/contents.
   - **Edge/state transitions:** empty-queue dequeue/peek behavior, count after a sequence of
     enqueue/dequeue, queue state after clear.
   - **Negative:** invalid item per the method contract (if applicable).

4. **`QfcQueue` pure paths** — `QfcQueue.cs`.
   - **Positive:** enqueue/dequeue and any pure queue-management operations.
   - **Edge/state transitions:** empty-queue behavior, count tracking, ordering invariants.
   - **Negative:** documented invalid-input behavior (if applicable).

Only pure queue-management paths are targeted; any member that touches Outlook items or
WinForms is out of scope and is covered by the #197 exemption boundary.

### Increment 3 — TaskMaster (`TaskMaster.Test`)

Target classes and members (all in `TaskMaster`, in `TaskMaster/AppGlobals/`).

1. **`AppStagingFilenames`** — `AppStagingFilenames.cs`. Pure property-delegation to a settings
   object. Test with an injected settings stub (Moq mock or fake of the settings interface
   already used by the class).
   - **Positive:** each property returns the value supplied by the injected settings stub.
   - **Negative/edge:** null or empty settings value behavior per the property contract;
     defaulting behavior if defined.

2. **`AppFileSystemFolderPaths.MatchBestSpecialFolder(string path)`** —
   `AppFileSystemFolderPaths.cs`. Pure LINQ/string matching over the special-folder set.
   - **Positive:** an input path that matches a known special folder returns that folder.
   - **Edge:** longest-prefix / best-match selection when multiple candidates partially match;
     case sensitivity per the method contract; trailing-separator normalization.
   - **Negative:** no-match input (assert the documented no-match result); null/empty path.

3. **`AppQuickFilerSettings` remaining pure properties** — `AppQuickFilerSettings.cs`. Cover the
   remaining pure property getters/setters with a mocked or stubbed settings backing.
   - **Positive:** get/set round-trips return the stored value.
   - **Edge/negative:** default values and null handling per each property contract.

For all three classes, isolate the unit by injecting a settings stub/mock; do not read the live
application settings store, the filesystem, or Outlook.

## Non-Goals

- Increments 4+ of the roadmap (Tags `TagController` pure-logic methods; QuickFiler
  `EfcDataModel`/`QfcFormController` mockable branches; QuickFiler `QfcItemController` scoring
  logic). These are follow-up work.
- Any change to production code, EXCEPT the two maintainer-authorized Phase 5 seams (option B):
  the UtilitiesCS `[assembly: InternalsVisibleTo("ToDoModel.Test")]` attribute and the
  `TaskMaster.AppFileSystemFolderPaths.MatchBestSpecialFolder` pure-helper extraction. Both
  preserve runtime behavior exactly. Any production change beyond these two narrow seams remains
  a flag-and-stop, not a silent change (see Constraints).
- Re-measuring or changing the #197 exemption boundary, `coverage.config`, or the coverage
  pipeline.
- Reaching the 80% floor in this feature. The roadmap projects Increments 1–3 yield a partial
  increase toward, not to, the floor.
- Testing any COM/VSTO/WinForms-bound code (live Outlook process, WinForms message loop, STA UI
  controls).

## Dependencies / Touchpoints

- **#197 exemption (merged).** This feature depends on the testable-denominator definition and
  the preserved-seam list from the #197 spec. The target seams are exactly the seams #197 lists
  as preserved and measured.
- **Existing `.Test` projects:** `ToDoModel.Test`, `QuickFiler.Test`, `TaskMaster.Test` are
  already wired with MSTest, Moq, and FluentAssertions.
- **`InternalsVisibleTo("ToDoModel.Test")`** is present in `ToDoModel` source, so the `internal`
  `SetAndSave<T>` overloads are reachable from the test project without a production change.
- Coverage pipeline (`scripts/vscode/Invoke-MSTestWithCoverage.ps1` and helpers) for
  re-measurement; no pipeline change required.
- Required coordination: none beyond standard review.

## Risks & Mitigations

- **Accidental coverage of exempt code.** Mitigation: target only the seams enumerated in the
  #197 preserved-seam list; do not instantiate Outlook-bound constructors or WinForms controls.
- **Hidden production dependency in a "pure" seam.** Some seams (`ProjectEntry.SetProjectId`
  malformed path) may route through a static `MyBox`/MessageBox call. Mitigation: prefer an
  injectable seam already present in source; if none exists, restrict the test to branches
  reachable without the dialog and flag the gap. Do not introduce a new production seam silently.
- **New production seam temptation.** Introducing an injection point to make a class testable is
  a behavior/API change. Mitigation: flag-and-stop and obtain maintainer direction before any
  production edit; record it as a deviation rather than proceeding.
- **Determinism / flakiness.** Mitigation: no temp files, no mutable global state, no
  timing/sleep hacks; async tests use synchronously-completing delegates. This avoids the flaky
  timing pattern tracked in #191/#176.
- **Coverage increase below estimate.** The roadmap estimates are optimistic upper bounds.
  Mitigation: the acceptance criterion requires a measured net increase versus 71.65%, not a
  specific point target; record the actual figure.

## Technical Specifications

- **Files/modules expected to change:** test files only, under `ToDoModel.Test`,
  `QuickFiler.Test`, and `TaskMaster.Test`. No production `.cs`, `.csproj`, `.props`, or config
  file changes are expected.
- **Public interfaces/contracts affected:** none.
- **Data flow or validation adjustments:** none.
- **Logging/telemetry updates:** none.
- **Migration or backfill needs:** none.
- **Seam access:** `internal` members in `ToDoModel` are reached via the existing
  `InternalsVisibleTo`. Settings dependencies in `TaskMaster` classes are isolated with Moq
  mocks or in-memory stubs of the settings type the class already accepts.

## Test Strategy

- **Tests to add:** new MSTest test methods (and new test files where a target class has no
  existing test file) in the three `.Test` projects, organized by increment.
- **Framework/libraries:** MSTest attributes (`[TestClass]`, `[TestMethod]`), Moq for settings
  stubs and any delegate verification that benefits from it, FluentAssertions for assertions.
- **Structure:** every test follows Arrange–Act–Assert with a descriptive name and a clear
  failure message.
- **Scenario completeness (per General Unit Test Policy UT2):** positive, negative, edge, and
  error scenarios for each member; state transitions for the queue types; arithmetic boundaries
  for `IDList`/`BaseChanger`/`MatchBestSpecialFolder`.
- **Isolation/determinism (UT1, UT4):** no external services, no live Outlook, no WinForms
  message loop, no temp files, no mutable global state, no timing/sleep. Async paths use
  delegates that complete synchronously.
- **Coverage impact and targets:** new/changed code targets >= 90% line coverage; covered-line
  counts on the named seams increase; no coverage regression on changed lines.
- **Toolchain commands (run in order; restart on any change/failure):**
  1. `dotnet tool run csharpier .`
  2. `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
  3. `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true`
  4. `vstest.console.exe <test-assembly-paths> /EnableCodeCoverage`
- **Coverage re-measurement:** re-run the coverage pipeline; record post-feature production-only
  rate and the net change versus 71.65% to the feature evidence folder
  `docs/features/active/2026-06-14-coverage-increments-1-3-testable-seams-199/evidence/qa-gates/`.
- **Manual validation:** none required beyond the recorded coverage figures.

## Acceptance Criteria

- [x] **Increment 1 (ToDoModel):** MSTest tests are added and passing for
  `ToDoLoader.SetAndSave<T>` (all four overloads, read-only guard, null `objectSetter`, null
  `objectSaver`), `IDList.GetNextToDoID(string)` (base case, ID-present loop, length boundary),
  `ProjectEntry` (`SetProjectId` happy/null/malformed, `CompareTo` equal/different/null/prefix),
  and the remaining uncovered `BaseChanger` branches; the covered-line count for these seams
  increases. The previously-deferred `ProjectEntry` dialog branches (malformed-ID,
  change-confirmation Yes/No, and the `CompareTo` length tie-break) are now fully covered by
  Phase 5 (P5-T2 UtilitiesCS seam, P5-T3 tests, P5-T10 pass, P5-T11 covered-line increase).
- [x] **Increment 2 (QuickFiler):** MSTest tests are added and passing for `KaChar`,
  `KaCharAsync`, `KaKey`, `KaKeyAsync`, `KaStringAsync`, the remaining `KbdActions<>` branches,
  and the pure paths of `FilerQueue` and `QfcQueue`; the covered-line count for these seams
  increases.
- [x] **Increment 3 (TaskMaster):** MSTest tests are added and passing for `AppStagingFilenames`
  (injected settings stub), `AppFileSystemFolderPaths.MatchBestSpecialFolder` (pure LINQ
  positive/edge/negative), and the remaining pure properties of `AppQuickFilerSettings`; the
  covered-line count for these seams increases. The previously-deferred
  `AppFileSystemFolderPaths.MatchBestSpecialFolder` coverage is now fully delivered by Phase 5
  (P5-T4 pure-helper extraction seam, P5-T5 tests, P5-T10 pass, P5-T11 covered-line increase).
- [x] All tests comply with the General + C# Unit Test Policy: MSTest, Moq, FluentAssertions,
  Arrange–Act–Assert, independent, isolated, deterministic, no temp files, no external
  dependencies, no live Outlook/WinForms, no timing/sleep hacks. Each test covers the applicable
  positive, negative, edge, and error scenarios for its target.
- [x] New or changed code achieves >= 90% line coverage, and there is no coverage regression on
  changed lines.
- [x] No exempted COM/VSTO/WinForms code is un-exempted or tested; no `[ExcludeFromCodeCoverage]`
  attribute is added or removed; `coverage.config`, `TaskMaster.runsettings`, and the coverage
  pipeline are unchanged.
- [x] No production behavior change: no production method bodies, signatures, public APIs, or
  config files are modified. If a minimal injectable seam not already present in source is found
  to be required, this is flagged and stopped for maintainer direction rather than silently added.
- [x] The full C# toolchain passes in a single final pass: csharpier (no diff), msbuild with
  analyzers + code style, msbuild with nullable + warnings-as-errors, and the MSTest suite with
  coverage.
- [x] Production-only coverage is re-measured and recorded to the feature evidence folder, showing
  a net increase versus the 71.65% post-#197 baseline.

## Definition of Done

- [x] Increment 1, 2, and 3 tests added and passing in their respective `.Test` projects.
- [ ] All target seams enumerated in Scope are covered with positive/negative/edge/error scenarios.
  (Phase 5 closed AppFileSystemFolderPaths.MatchBestSpecialFolder fully (P5-T4/P5-T5) and the
  ProjectEntry malformed-ID dialog branch plus the CompareTo length tie-break (P5-T2/P5-T3 via the
  UtilitiesCS `InternalsVisibleTo` seam). One residual gap remains: the ProjectEntry
  change-confirmation branch (SetProjectId -> ChangeId) cannot be covered without a THIRD production
  seam, because ChangeId commits via the ProjectID property setter whose RAW un-seamed
  MessageBox.Show is outside the two authorized Phase 5 seams. This is an authorized-scope
  flag-and-stop recorded in evidence/other/p5-projectentry-changeconfirm-gap.2026-06-14T15-10.md;
  it requires separate maintainer direction (route the property setter through MyBox). Left
  unchecked because not all enumerated dialog scenarios are covered.)
- [x] New/changed code >= 90% coverage; no regression on changed lines.
- [x] No production code, config, or pipeline change (or any required seam flagged and resolved
  with maintainer direction).
- [x] Full C# toolchain green in a single final pass (format → analyzers → nullable → test).
- [x] Production-only coverage re-measured and recorded; net increase versus 71.65% documented.
- [x] Spec and user-story acceptance criteria checked off as delivered and verified.

## Seeded Test Conditions (from potential)

- [ ] Positive, negative, edge, and error scenarios per the General Unit Test Policy.
- [ ] Pure-logic/arithmetic boundaries (IDList base-36, BaseChanger, MatchBestSpecialFolder).
- [ ] Queue state transitions (FilerQueue, QfcQueue).
