# quickfiler-datamodel-coverage — Atomic Implementation Plan

- **Issue:** [#436](https://github.com/drmoisan/TaskMaster/issues/436)
- **Parent epic issue:** [#136](https://github.com/drmoisan/TaskMaster/issues/136) (`quickfiler-per-file-coverage`, child F5, wave 1)
- **Integration branch:** `epic/quickfiler-per-file-coverage-integration`
- **Upstream dependency:** F1 `quickfiler-coverage-denominator-and-exemption-ledger`
- **Owner:** drmoisan
- **Last Updated:** 2026-08-07T20-42
- **Status:** Revised for preflight re-validation
- **Version:** 1.2 — revision 1 applied preflight findings R1–R6 and advisories A1–A2: Phase 11 reordered so the
  AC4 determinism audit precedes check-off and Phase 11 closes only AC2/AC3/AC5/AC7/AC8; AC1/AC4/AC6 now close
  in Phase 12 after their evidence exists; a post-format file-size re-verification was inserted as the
  authoritative AC3 evidence; the Test Plan new-file count was corrected to 19 plus one test-support file;
  [P7-T17] was decomposed into a decision task plus dedicated split, csproj and re-measure tasks; the four
  bundled `.Part2.cs` measurement tasks were decomposed the same way; D-18 records the two AC6 command-form
  substitutions; [P0-T5] uses the escalation form; Q4 covers the F2-owned S1 declaration site.
  Revision 2 fixes the defect revision 1 introduced: the post-format size gate [P12-T3] bundled measurement,
  split and `<Compile Include>` registration into one task — the same shape R5 removed from [P2-T51],
  [P3-T48], [P6-T41] and [P8-T56], and a shape D-14 forbids because an unregistered companion fails silently
  in these legacy non-SDK projects. [P12-T3] is now a pure measurement emitting a `SPLIT REQUIRED` /
  `SPLIT NOT REQUIRED` verdict; the new [P12-T4] performs the split (test companions as `.Part2.cs`,
  production companions as `.PartN.cs`); the new [P12-T5] registers each companion in its owning project,
  which for a production companion is `QuickFiler/QuickFiler.csproj`. The rest of Phase 12 was renumbered
  (old T4–T12 became T6–T14, commit last), and D-03, D-14, D-18, the Phase 11 and Phase 12 headers,
  [P11-T17] and Q5 were updated to the new IDs.
- **Task count:** 329 atomic tasks across Phases 0–12 (327 before revision 2; [P12-T3]'s decomposition added 2).
- **Work Mode:** `full-feature` — `spec.md` and `user-story.md` are the authoritative acceptance-criteria sources (AC1–AC8, byte-identical in both).

## Required References

- `CLAUDE.md` (standing instructions)
- `.claude/rules/general-code-change.md`
- `.claude/rules/general-unit-test.md`
- `.claude/rules/csharp.md`
- `docs/features/active/2026-08-07-quickfiler-datamodel-coverage-436/spec.md` (engineering contract)
- `docs/features/active/2026-08-07-quickfiler-datamodel-coverage-436/user-story.md`
- `docs/features/active/2026-08-07-quickfiler-datamodel-coverage-436/issue.md`
- `docs/features/active/2026-08-07-quickfiler-datamodel-coverage-436/research/2026-08-08T00-43-qfcdatamodel.md`
- `docs/features/active/2026-08-07-quickfiler-datamodel-coverage-436/research/2026-08-08T00-43-qfcdatamodel-queueprocessing.md`
- `docs/features/active/2026-08-07-quickfiler-datamodel-coverage-436/research/2026-08-08T00-43-qfcdatamodel-framebuilding.md`
- `docs/features/active/2026-08-07-quickfiler-datamodel-coverage-436/research/2026-08-08T00-43-efcdatamodel.md`
- `docs/features/active/2026-08-07-quickfiler-datamodel-coverage-436/research/2026-08-08T00-43-iqfcdatamodel.md`
- `docs/features/epics/quickfiler-per-file-coverage/epic.md`

**All work must comply with these policies; this plan does not duplicate their content.**

## Evidence Locations (canonical, non-overridable)

Let `<FEATURE>` = `docs/features/active/2026-08-07-quickfiler-datamodel-coverage-436`.

- Baseline: `<FEATURE>/evidence/baseline/`
- QA gates (including all per-file coverage results): `<FEATURE>/evidence/qa-gates/`
- Regression testing: `<FEATURE>/evidence/regression-testing/`
- Issue-update mirrors: `<FEATURE>/evidence/issue-updates/`
- Other: `<FEATURE>/evidence/other/`

Writing evidence to `artifacts/baselines/`, `artifacts/baseline/`, `artifacts/qa/`, `artifacts/qa-gates/`,
`artifacts/coverage/`, `artifacts/evidence/`, or any other non-canonical location is a policy violation.
Every evidence artifact records `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:`.
`<ts>` below denotes an ISO-8601 `yyyy-MM-ddTHH-mm` stamp captured at execution time.

## Command Register

Every command-bearing task names one of these identifiers. Commands run from the worktree root
`C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-a923053598cf4ccea`.

- **CMD-BOOTSTRAP** — `pwsh -File scripts/vscode/Install-RepoDotNetSdk.ps1`, then `dotnet tool restore`,
  then `dotnet-coverage --version` (installing the global tool with `dotnet tool install --global dotnet-coverage` if it does not resolve).
- **CMD-RESTORE** — `pwsh -File scripts/vscode/Invoke-Restore.ps1`
- **CMD-FORMAT** — `dotnet tool run csharpier format .`
- **CMD-FORMAT-CHECK** — `dotnet tool run csharpier check .` (must exit 0; `pipe-files` is not an enforcing gate)
- **CMD-ANALYZER** — `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
- **CMD-NULLABLE** — `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true`
- **CMD-COVERAGE-FULL** — `pwsh -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -CoverageOutput <FEATURE>/evidence/<kind>/coverage-<stage>.cobertura.xml`
  (full-suite `*.Test.dll` discovery through `dotnet-coverage collect` wrapping `vstest.console.exe`; emits Cobertura XML with numeric `line-rate`/`branch-rate`).
- **CMD-PERFILE** — F1's per-file line-coverage harness (derived from `scripts/vscode/Invoke-MSTestWithCoverage.ps1`).
  Its exact path and invocation are resolved and recorded at [P0-T5]; every later reference means the invocation recorded there.
- **CMD-TEST-SCOPED** — scoped MSTest run:
  ```
  $vstest = vswhere -latest -products * -find Common7\IDE\Extensions\TestPlatform\vstest.console.exe
  & $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation /TestCaseFilter:"<filter>"
  ```
  `vstest.console.exe` is not on PATH; `/InIsolation` is mandatory for the Moq-based assemblies; vstest 18.x rejects
  `OR` inside `/TestCaseFilter`, so multiple clauses are joined with `|`.

## Decisions Record

Binding decisions made at planning time. An executor that departs from one must record the departure.

- **D-01 — Seam namespace.** `spec.md` §6 renumbering is authoritative: **S1–S7** for the `QfcDatamodel`
  partial family, **E1–E7** for `EfcDataModel`. The individual research artifacts' colliding labels are superseded.
- **D-02 — One DI surface.** Every `QfcDatamodel` seam declaration (S1–S7) lands in the new
  `QuickFiler/Controllers/QfcDatamodel.Construction.cs`, never in `QfcDatamodel.QueueProcessing.cs` or
  `QfcDatamodel.FrameBuilding.cs`. This is why Phase 1 must land before Phases 3, 5 and 6.
- **D-03 — Attribute removal is the last production task.** `[ExcludeFromCodeCoverage]` at
  `QuickFiler/Controllers/QfcDatamodel.cs:25` is type-scoped and admits all three partials into the
  denominator at once. It is removed in Phase 10, after every seam and test phase. No production behavior
  change may land after [P10-T1]; the Phase 12 formatter pass is a toolchain step, not a production change.
  A post-format split at [P12-T4] is likewise not a production behavior change: it is a partial-class
  restructure that moves existing members verbatim into a companion file of the same type and namespace,
  changing no signature, no accessibility and no member body. Its paired `<Compile Include>` registration at
  [P12-T5] is a build-wiring edit for the same reason. Neither violates the rule above.
- **D-04 — Clock seam.** `System.TimeProvider` + `FakeTimeProvider` (`Microsoft.Extensions.TimeProvider.Testing`,
  `QuickFiler.Test/packages.config:85`). The repository has no `IClock` and none is introduced.
  `Thread.Sleep`, `Task.Delay`, and real wall-clock waits are prohibited in tests. Bounded, condition-driven
  `SpinWait.SpinUntil` (`QfcDatamodelLivenessTests.cs:54-57`) is the only permitted state-observation helper.
- **D-05 — The `TimeProvider` trap.** `TimeProvider` (`QfcDatamodel.cs:112`) is an auto-property with an
  initializer, so `FormatterServices.GetUninitializedObject` leaves it **null**, and
  `QfcStreamingDequeueConfidenceGate` then silently falls back to `TimeProvider.System` — a forgotten
  assignment yields a real 12-second wall-clock test that still appears to pass. Every test task that
  constructs via `GetUninitializedObject` and touches a timing path assigns `model.TimeProvider` explicitly,
  through a per-file `CreateModelWithFakeClock(out FakeTimeProvider fake)` helper.
- **D-06 — No STA anywhere.** `QfcDatamodel.FrameBuilding.cs` is `Deedle.Frame`, not WinForms (zero
  `System.Windows.Forms` references, verified). No `*.StaTests.cs` file is created in this child, and
  `QuickFiler.Test` still has none when this child completes.
- **D-07 — `IQfcDatamodel.cs` receives zero production edits.** Its three cases are `SortOptionsEnum`
  characterization tests in a new `QuickFiler.Test/Interfaces/SortOptionsEnumTests.cs`. They deliberately do
  not construct `EmailSorter`, so F5 claims no coverage credit on F2's file. They earn zero line-coverage
  credit for `IQfcDatamodel.cs` and the plan says so; they are justified by `CLAUDE.md` § UT2.
- **D-08 — Test file location.** `SortOptionsEnumTests.cs` goes to `QuickFiler.Test/Interfaces/` per
  `.claude/rules/general-unit-test.md` § Test File Location, not to `Controllers/`.
- **D-09 — Severable scope.** The `QfcEmailFrameShaper` host-neutral extraction is Phase 5 and is
  **severable**: AC2 rests on S5/S6 alone. If it is dropped, Phase 5 is removed in full and Phase 6 cases 1–15
  retarget the public instance methods `model.SortTriageDate(...)` / `model.MostRecentByConversation(...)` on
  an uninitialized model, with case 14 folding into cases 16 and 22. No other phase changes.
- **D-10 — De-duplication decisions (157 cases retained, 0 removed).**
  (a) QueueProcessing case 21 `UnhookDequeuedNodes_NullBatch_ReturnsNull` is marked *conditional* in research
  because cases 7 and 13 reach the same lines transitively. **Retained** as its own task, authored as a direct
  reflection invocation, so no task carries a skip branch.
  (b) `EfcDataModel` cases 25, 44 and 45 are marked *no line-coverage delta*. **Retained** for AC5
  negative-flow completeness.
  (c) `QfcDatamodel.cs` case 4 (`Constructor_WithNullFrameBuilder_FallsBackToInitDf`) and FrameBuilding cases
  16–18 both touch `InitDf`. **Not duplicates** — case 4 asserts the constructor's null-builder binding, cases
  16–18 assert `InitDf`'s own behavior through S5.
  (d) FrameBuilding case 19 and case 25 both reach the true arm of `FrameBuilding.cs:36`. **Both retained** —
  19 exercises `ToggleOfflineMode` directly, 25 exercises it inside `GetEmailsInViewDfAsync`.
  (e) FrameBuilding case 19 explicitly does **not** duplicate `QfcDatamodelTests.cs:250`; no second test of
  B2's already-covered lines is authored.
- **D-11 — Coverage gates.** AC1's per-file floor of **80% line coverage** for every `testable` file is the
  blocking gate, measured by CMD-PERFILE. New production files (`QfcDatamodel.Construction.cs`, and
  `QfcEmailFrameShaper.cs` if Phase 5 is taken) additionally target **>= 90%** per `CLAUDE.md` § UT2.
  `.claude/rules/general-unit-test.md` states repository-wide floors of 85% line / 75% branch while
  `CLAUDE.md` § UT2 states 80% / new-code 90%; the repository-wide figure is **reported, non-blocking** for
  this child (precedent: #424), and the change-scoped per-file figures are blocking.
- **D-12 — No behavior change (AC7).** Every defect and observation in `spec.md` §12 (D1–D9, O1–O3) is
  promoted to a GitHub issue in Phase 11, never fixed. Where a test pins one of them, it is characterization.
- **D-13 — Files this child must not modify.** `QuickFiler/Interfaces/IQfcDatamodel.cs`, `coverage.config`,
  any shared build property file, `epic.md`, `QfcQueue.cs`, `QfcRemainingQueueAdmission.cs`, `EmailSorter.cs`,
  `EmailSorterTests.cs`, `QfcStreamingDequeueConfidenceGate.cs`, `QfcHighConfidencePreFilter.cs` (F2);
  `QfcFormController*.cs` (F6); `QfcHomeController*.cs`, `IQfcHomeController.cs`, `IFilerHomeController.cs`
  and the six `QfcHomeController*Tests.cs` files (F7); `QfcCollectionController.cs` (F11);
  `QuickFiler/Helper Classes/**` (F4); `UtilitiesCS/**`.
- **D-14 — Every new `.cs` file is paired with an explicit `<Compile Include>` task.** Both
  `QuickFiler/QuickFiler.csproj` (`:312-315`, `:361`) and `QuickFiler.Test/QuickFiler.Test.csproj` (`:90-145`)
  are legacy non-SDK projects with explicit item lists; an unregistered file silently does not compile and its
  tests never run. 20 test-side `<Compile Include>` tasks are unconditional within their own phase, as is the
  production entry for `QfcDatamodel.Construction.cs`; the production entry for `QfcEmailFrameShaper.cs`
  ([P5-T2]) is unconditional within Phase 5 but Phase 5 as a whole is severable under D-09; a third production
  entry (`EfcDataModel.Seams.cs`) is contingent on `[P7-T17]`. Every file-creating contingency branch in this
  plan (the `.Part2.cs` splits at [P2-T52], [P3-T49], [P6-T42], [P8-T57], the seam split at [P7-T18], and the
  post-format size-gate split at [P12-T4]) carries its own dedicated `<Compile Include>` task rather than
  folding registration into a measurement or split task. [P12-T4] is the only split whose scope spans
  production files as well as test files, so its registration task [P12-T5] may target
  `QuickFiler/QuickFiler.csproj` rather than `QuickFiler.Test/QuickFiler.Test.csproj`; it routes each
  companion to the project that owns its parent file.
- **D-15 — Additivity.** Every seam is an additive `internal` member, an additive `internal` constructor, or an
  additive `internal static` overload on a concrete class, with a null-means-production default and
  null-coalescing at the call site (not a property initializer, because every existing datamodel test
  constructs via `FormatterServices.GetUninitializedObject`). Additive overloads use **distinct arity**, never
  optional parameters (spec R4).
- **D-16 — `ConfigureAwait(false)` discipline.** `QfcDatamodel.FrameBuilding.cs:50` and `:89`, and the awaited
  chain reached from `ScoreRemainingQueueMailItemAsync`, must preserve `.ConfigureAwait(false)` verbatim.
  Dropping it deadlocks the Outlook UI thread. No test is proposed for this; it is a review rule.
- **D-17 — Mock strictness.** `Mock<MailItem>` is **loose** (the confidence gate logs `Subject`/`EntryID`).
  `Mock<ProgressTracker>` requires `Increment(It.IsAny<double>())` and `SpawnChild(It.IsAny<int>())` configured
  to return the mock itself; a bare `new ProgressTracker(cts)` NREs inside `Report(double)`.
- **D-18 — AC6 command-form reconciliation.** AC6 names four literal stages. This plan executes two of them
  through substituted command forms. Both substitutions are deliberate and are recorded here so that a feature
  audit reads them as reconciled rather than as deviations.
  (a) **Formatting.** AC6 names `csharpier .`. This plan uses **CMD-FORMAT** (`dotnet tool run csharpier
  format .`) followed by **CMD-FORMAT-CHECK** (`dotnet tool run csharpier check .`). Reason: the repository
  pins CSharpier **1.2.6** in `.config/dotnet-tools.json`, and 1.2.x requires an explicit `format` or `check`
  subcommand — the bare `csharpier .` form of 0.x is not a valid invocation under the pinned version.
  `check` is the enforcing gate; `pipe-files` is stdout-only and is not accepted as a substitute.
  (b) **Testing.** AC6 names `vstest.console.exe <test-assembly-paths> /EnableCodeCoverage`. This plan uses
  **CMD-COVERAGE-FULL** (`dotnet-coverage collect` wrapping `vstest.console.exe`, emitting Cobertura XML).
  Reason: `/EnableCodeCoverage` emits a binary `.coverage` file that carries no branch rate and exposes no
  numeric `line-rate`/`branch-rate` element, so it cannot supply the numeric values that [P0-T10], [P10-T4]
  and [P12-T8] are each required to record. The substituted form runs the same `vstest.console.exe` against
  the same full-suite `*.Test.dll` set and additionally satisfies the Coverage Evidence Contract.
  The five command steps executed in Phase 12 (CMD-FORMAT, CMD-FORMAT-CHECK, CMD-ANALYZER, CMD-NULLABLE,
  CMD-COVERAGE-FULL) map onto AC6's four stages as: format → CMD-FORMAT plus CMD-FORMAT-CHECK;
  lint → CMD-ANALYZER; type-check → CMD-NULLABLE; test → CMD-COVERAGE-FULL.

---

## Implementation Plan (Atomic Tasks)

### Phase 0 — Baseline Capture and Policy Reads

- [ ] [P0-T1] Run CMD-BOOTSTRAP and record the toolchain bootstrap result to `<FEATURE>/evidence/baseline/toolchain-bootstrap.<ts>.md`
  - Acceptance: artifact records `Timestamp:`, `Command:` (all three invocations), `EXIT_CODE: 0` for each, and an `Output Summary:` that includes the resolved `dotnet tool run csharpier --version` string and the resolved `dotnet-coverage --version` string. `.dotnet-sdk/` exists after the run.
- [ ] [P0-T2] Run CMD-RESTORE and record the NuGet restore result to `<FEATURE>/evidence/baseline/restore.<ts>.md`
  - Acceptance: artifact carries the four required fields with `EXIT_CODE: 0`; `packages/` contains `Deedle`, `Microsoft.Bcl.TimeProvider`, `Microsoft.Extensions.TimeProvider.Testing`, `Moq`, `FluentAssertions`, and `MSTest.TestFramework`.
- [ ] [P0-T3] Read the policy documents in `policy-compliance-order` order and record the read receipt to `<FEATURE>/evidence/baseline/phase0-instructions-read.<ts>.md`
  - Acceptance: artifact records `Timestamp:`, `Policy Order:`, and the explicit list of files read — `CLAUDE.md`, `.claude/rules/general-code-change.md`, `.claude/rules/general-unit-test.md`, `.claude/rules/csharp.md`, `.claude/rules/quality-tiers.md`, `.claude/rules/tonality.md`.
- [ ] [P0-T4] Read the feature documents and all five research artifacts and record the read receipt to `<FEATURE>/evidence/baseline/feature-documents-read.<ts>.md`
  - Acceptance: artifact lists `spec.md`, `user-story.md`, `issue.md`, the five files under `<FEATURE>/research/`, and `docs/features/epics/quickfiler-per-file-coverage/epic.md`, and confirms the AC set read is AC1–AC8 identical in `spec.md` and `user-story.md`.
- [ ] [P0-T5] Verify F1's outputs exist on the integration branch and record the resolution to `<FEATURE>/evidence/baseline/f1-upstream-verification.<ts>.md`
  - Acceptance: artifact records that `docs/features/epics/quickfiler-per-file-coverage/coverage-ledger.md` exists, records the resolved path and exact invocation of F1's per-file coverage harness (fixing CMD-PERFILE for the remainder of this plan), and records `EXIT_CODE: 0` for a `--help`-equivalent smoke invocation of that harness. If either artifact is absent, the artifact records `ESCALATION REQUIRED — F1 outputs absent on the integration branch`, names which of the two is missing, and the condition is escalated to `epic-orchestrator` as an upstream sequencing matter; no projected or spec-derived figure is substituted for the missing harness output.
- [ ] [P0-T6] Record F1's ledger classification for the five in-scope production files to `<FEATURE>/evidence/baseline/ledger-classification.<ts>.md`
  - Acceptance: artifact quotes the ledger entry for `QfcDatamodel.cs`, `QfcDatamodel.QueueProcessing.cs`, `QfcDatamodel.FrameBuilding.cs`, `EfcDataModel.cs` and `IQfcDatamodel.cs`, each as `testable`, `ratified-exempt`, or `not-measurable (declaration-only)`. If the ledger classifies `IQfcDatamodel.cs` as `testable`, or ratifies a type-wide exemption for any of the other four, the artifact records `ESCALATION REQUIRED` with the `spec.md` §5.2 / §6.3 evidence and the plan halts.
- [ ] [P0-T7] Run CMD-FORMAT-CHECK and record the formatter baseline to `<FEATURE>/evidence/baseline/csharpier-check.<ts>.md`
  - Acceptance: artifact carries the four required fields; `Output Summary:` states the exit code and the count of files reported unformatted (zero or otherwise), establishing the pre-change formatter state.
- [ ] [P0-T8] Run CMD-ANALYZER and record the analyzer baseline to `<FEATURE>/evidence/baseline/msbuild-analyzers.<ts>.md`
  - Acceptance: artifact carries the four required fields; `Output Summary:` states the error and warning counts and confirms the solution builds.
- [ ] [P0-T9] Run CMD-NULLABLE and record the nullable/type-check baseline to `<FEATURE>/evidence/baseline/msbuild-nullable.<ts>.md`
  - Acceptance: artifact carries the four required fields; `Output Summary:` states the error and warning counts for the nullable-enabled build.
- [ ] [P0-T10] Run CMD-COVERAGE-FULL with `-CoverageOutput <FEATURE>/evidence/baseline/coverage-baseline.cobertura.xml` and record the suite baseline to `<FEATURE>/evidence/baseline/coverage-suite.<ts>.md`
  - Acceptance: artifact carries the four required fields; `Output Summary:` records **numeric** total passed/failed test counts and the numeric repository-wide `line-rate` and `branch-rate` read from the emitted Cobertura root `<coverage>` element. `UNVERIFIED` is not an acceptable value.
- [ ] [P0-T11] Run CMD-PERFILE against the baseline Cobertura output and record numeric per-file baseline coverage to `<FEATURE>/evidence/baseline/coverage-per-file-baseline.<ts>.md`
  - Acceptance: artifact records a numeric line-coverage percentage for `EfcDataModel.cs`, and for each of `QfcDatamodel.cs`, `QfcDatamodel.QueueProcessing.cs`, `QfcDatamodel.FrameBuilding.cs` records either a numeric value or the literal `ABSENT (type-scoped [ExcludeFromCodeCoverage] at QfcDatamodel.cs:25)`; `IQfcDatamodel.cs` is recorded with its ledger classification. No projected or read-derived figure from `spec.md` or the research artifacts appears in this artifact.
- [ ] [P0-T12] Record the pre-change line counts of the in-scope files to `<FEATURE>/evidence/baseline/file-line-counts.<ts>.md`
  - Acceptance: artifact records measured line counts for `QuickFiler/Controllers/QfcDatamodel.cs`, `QfcDatamodel.QueueProcessing.cs`, `QfcDatamodel.FrameBuilding.cs`, `EfcDataModel.cs`, `QuickFiler/Interfaces/IQfcDatamodel.cs`, and for `QuickFiler.Test/Controllers/QfcDatamodelTests.cs`, `QfcDatamodelLivenessTests.cs`, `EfcDataModelTests.cs`, and states the 500-line headroom for each.
- [ ] [P0-T13] Record the baseline repository state to `<FEATURE>/evidence/baseline/tree-state.<ts>.md`
  - Acceptance: artifact records the current `HEAD` SHA, the current branch name, and `git status --porcelain` output showing a clean worktree. The SHA is recorded as the baseline-capture reference for later diff scoping; it is not asserted as an expected value for any later task.

### Phase 1 — QfcDatamodel.cs Dead-Code Removal, Construction Split, and Seam Declarations

Production-only phase. It must complete before Phases 3, 5 and 6, because those phases' tests bind seams
declared here (D-02). No test is added in this phase.

- [ ] [P1-T1] Delete the unused `log` static readonly field at `QuickFiler/Controllers/QfcDatamodel.cs:96-99`
  - Acceptance: the field is gone; a repository-wide `\blog\b` search across `QuickFiler/Controllers/QfcDatamodel*.cs` returns no reference; `logger` (lines 28–30) is untouched.
- [ ] [P1-T2] Delete the dead `Worker_RunWorkerCompleted(object, RunWorkerCompletedEventArgs)` member and its explanatory comment at `QuickFiler/Controllers/QfcDatamodel.cs:212-236`
  - Acceptance: the member and its two `MessageBox.Show` call sites are gone; the only remaining textual reference in the file is the commented-out subscription at former line 170, which is also removed; no live call site exists.
- [ ] [P1-T3] Delete the dead `LoadRemainingEmailsToQueue(BackgroundWorker, CancellationToken)` member at `QuickFiler/Controllers/QfcDatamodel.cs:378-417`
  - Acceptance: the member is gone; the `nameof(...)` uses that referenced it inside the surviving `LoadRemainingEmailsToQueueAsync(CancellationToken)` log strings are updated to name a surviving member; the solution still compiles at [P1-T23].
- [ ] [P1-T4] Delete the dead `LoadRemainingEmailsToQueueAsync(BackgroundWorker, CancellationToken)` member at `QuickFiler/Controllers/QfcDatamodel.cs:418-466`
  - Acceptance: the member is gone together with its `#pragma warning disable CS0618` / `restore` pair; the method-group assignments that default `RemainingEmailLoader` still bind the surviving one-argument overload `LoadRemainingEmailsToQueueAsync(CancellationToken)`.
- [ ] [P1-T5] Delete the empty `#region Linked List Locking` block at `QuickFiler/Controllers/QfcDatamodel.cs:469-473`
  - Acceptance: the region and its `#endregion` are gone; no member was removed with it.
- [ ] [P1-T6] Verify the post-deletion size and reference cleanliness of `QuickFiler/Controllers/QfcDatamodel.cs` and record it to `<FEATURE>/evidence/other/qfcdatamodel-deadcode-removal.<ts>.md`
  - Acceptance: measured line count is approximately 373 (496 minus 123); the artifact lists the five deletions with their removed line counts and confirms zero remaining references to each deleted symbol repository-wide.
- [ ] [P1-T7] Create `QuickFiler/Controllers/QfcDatamodel.Construction.cs` with the namespace, `public partial class QfcDatamodel` scaffolding, and the `using` set required by the members it will receive
  - Acceptance: the file declares the same namespace `QuickFiler.Controllers` and the same partial type; it includes `using System.Threading;` and the `Deedle`/`Microsoft.Office.Interop.Outlook` usings needed by S3–S6; it compiles as an empty partial before members are moved.
- [ ] [P1-T8] Add `<Compile Include="Controllers\QfcDatamodel.Construction.cs" />` to `QuickFiler/QuickFiler.csproj` beside the existing `QfcDatamodel.cs` entry
  - Acceptance: the entry sits with the other `Controllers\QfcDatamodel*.cs` items near `:312-315`; a build shows the new file compiled (a missing entry fails silently).
- [ ] [P1-T9] Move the two existing constructors `QfcDatamodel(IApplicationGlobals)` and `QfcDatamodel(IApplicationGlobals, CancellationToken)` from `QfcDatamodel.cs:34-52` into `QuickFiler/Controllers/QfcDatamodel.Construction.cs`
  - Acceptance: both bodies are byte-identical after the move including statement order; the public two-argument constructor keeps its arity, parameter types, order, names and accessibility so `QfcHomeController.cs:163` binds identically.
- [ ] [P1-T10] Move the public static `LoadAsync(IApplicationGlobals, CancellationToken, CancellationTokenSource, ProgressTracker)` from `QfcDatamodel.cs:54-73` into `QuickFiler/Controllers/QfcDatamodel.Construction.cs`
  - Acceptance: the body is byte-identical after the move; the public four-argument signature bound by `QfcHomeController.cs:173` is unchanged.
- [ ] [P1-T11] Move `Cleanup()` from `QfcDatamodel.cs:75-91` into `QuickFiler/Controllers/QfcDatamodel.Construction.cs`
  - Acceptance: the body is byte-identical after the move; no idempotency guard is added (defect D1 is promoted, not fixed, per D-12).
- [ ] [P1-T12] Move the `TimeProvider` and `RemainingEmailLoader` seam properties from `QfcDatamodel.cs:108-128` into `QuickFiler/Controllers/QfcDatamodel.Construction.cs`
  - Acceptance: both declarations including the CS0236 explanatory comment move verbatim; consumers in `QfcDatamodel.QueueProcessing.cs:173`, `QfcDatamodel.FrameBuilding.cs:43`, `QfcDatamodelTests.cs`, `QfcDatamodelLivenessTests.cs` and `QfcInitEmailQueueZeroBatchTests.cs` bind by member name and are not edited.
- [ ] [P1-T13] Declare seam **S1** `internal IFolderScoringService ScoringService { get; set; }` in `QuickFiler/Controllers/QfcDatamodel.Construction.cs`
  - Acceptance: the property carries an XML doc stating "null means the production `FolderScoringService`"; it reuses the existing interface at `QfcHighConfidencePreFilter.cs:130` and introduces no new abstraction; no property initializer is used.
- [ ] [P1-T14] Consume seam **S1** at the scoring call site in `QuickFiler/Controllers/QfcDatamodel.cs` (`ScoreRemainingQueueMailItemAsync`, former line 368)
  - Acceptance: the line reads `var scoringService = ScoringService ?? new FolderScoringService();` and the subsequent `ScoreAsync` call uses it; the surrounding `.ConfigureAwait(false)` discipline is unchanged (D-16).
- [ ] [P1-T15] Declare seam **S2** `internal Func<string, DialogResult> MessageBoxInvoker { get; set; }` in `QuickFiler/Controllers/QfcDatamodel.Construction.cs`
  - Acceptance: declared as an **instance** property (not the mutable `static` of the `DfDeedle.MessageBoxInvoker` precedent) so tests remain independent; XML doc states null means the production `MessageBox.Show(string)`.
- [ ] [P1-T16] Consume seam **S2** at the empty-frame branch of `LoadRemainingEmailsToQueueAsync(CancellationToken)` in `QuickFiler/Controllers/QfcDatamodel.cs` (former line 309)
  - Acceptance: the call reads `(MessageBoxInvoker ?? MessageBox.Show)("Email Frame is empty");`; the returned `false` and the surrounding branch shape are unchanged.
- [ ] [P1-T17] Add seam **S3** — the additive `internal QfcDatamodel(IApplicationGlobals, CancellationToken, Func<Explorer, Frame<int, string>> frameBuilder)` constructor in `QuickFiler/Controllers/QfcDatamodel.Construction.cs`, with the existing public two-argument constructor rewritten as `: this(appGlobals, token, null) { }`
  - Acceptance: statement order inside the internal constructor matches the original exactly; the frame build reads `_frame = (frameBuilder ?? InitDf)(_activeExplorer);`; the public constructor's arity, parameter types, order and accessibility are unchanged; distinct arity, no optional parameter (spec R4).
- [ ] [P1-T18] Add seam **S4** — the additive `internal static Task<QfcDatamodel> LoadAsync(IApplicationGlobals, CancellationToken, CancellationTokenSource, ProgressTracker, Func<QfcDatamodel, Explorer, ProgressTracker, Task> dataFrameInitializer)` overload in `QuickFiler/Controllers/QfcDatamodel.Construction.cs`, with the public four-argument `LoadAsync` retained verbatim as a delegating wrapper
  - Acceptance: the internal overload's body preserves the original statement order and reads `var initializer = dataFrameInitializer ?? ((m, e, p) => m.InitDfAsync(e, p));` followed by `await initializer(model, appGlobals.Ol.App.ActiveExplorer(), progress.Increment(2)).ConfigureAwait(false);`; the public overload is `=> LoadAsync(appGlobals, token, tokenSource, progress, null);`; distinct arity, no optional parameter.
- [ ] [P1-T19] Declare seam **S5** `internal Func<Explorer, Frame<int, string>> EmailDataInViewProvider { get; set; }` in `QuickFiler/Controllers/QfcDatamodel.Construction.cs`
  - Acceptance: declared here rather than in `QfcDatamodel.FrameBuilding.cs` per D-02; XML doc states null means the production `DfDeedle.GetEmailDataInView(Explorer)` and records that the production path shows modal dialogs this test assembly cannot suppress.
- [ ] [P1-T20] Declare seam **S6** `internal Func<Explorer, CancellationToken, CancellationTokenSource, ProgressTracker, Task<Frame<int, string>>> EmailDataInViewAsyncProvider { get; set; }` in `QuickFiler/Controllers/QfcDatamodel.Construction.cs`
  - Acceptance: declared here per D-02; XML doc states null means the production `DfDeedle.GetEmailDataInViewAsync`; the file's `using System.Threading;` is present.
- [ ] [P1-T21] Resolve the **S7** `NewMailEx` contingency and record the outcome to `<FEATURE>/evidence/other/newmailex-proxy-determination.<ts>.md`
  - Acceptance: the artifact records whether `Moq` can proxy the `[ComEventInterface]` add/remove accessors of `Microsoft.Office.Interop.Outlook.Application.NewMailEx` (verified by building and running a `VerifyAdd` probe). If it can, S7 is **not** declared and the artifact records that decision. If it cannot, `internal Action<Outlook.Application> NewMailSubscriber` / `NewMailUnsubscriber` are declared in `QuickFiler/Controllers/QfcDatamodel.Construction.cs` with call sites `(NewMailSubscriber ?? (app => app.NewMailEx += Application_NewMailEx))(_globals.Ol.App);` in both constructors and the mirror in `Cleanup()`. Either outcome closes this task; the artifact states which was taken and binds tests 3 and 10 in Phase 2.
- [ ] [P1-T22] Verify the post-split file sizes and record them to `<FEATURE>/evidence/other/file-line-counts-phase1.<ts>.md`
  - Acceptance: measured line counts for `QuickFiler/Controllers/QfcDatamodel.cs` and `QuickFiler/Controllers/QfcDatamodel.Construction.cs` are each **below 500**; the artifact also re-records `QfcDatamodel.QueueProcessing.cs` (177) and `QfcDatamodel.FrameBuilding.cs` (154) as unchanged.
- [ ] [P1-T23] Run CMD-ANALYZER and record the Phase 1 build gate to `<FEATURE>/evidence/qa-gates/phase1-analyzer-build.<ts>.md`
  - Acceptance: artifact carries the four required fields with `EXIT_CODE: 0`; `Output Summary:` confirms zero errors and no new warnings relative to the [P0-T8] baseline.
- [ ] [P1-T24] Verify that Phase 1 touched no forbidden file and record the diff scope to `<FEATURE>/evidence/other/phase1-diff-scope.<ts>.md`
  - Acceptance: `git diff --name-only` against the [P0-T13] baseline SHA lists only `QuickFiler/Controllers/QfcDatamodel.cs`, `QuickFiler/Controllers/QfcDatamodel.Construction.cs`, `QuickFiler/QuickFiler.csproj`, and files under `<FEATURE>/evidence/`; `QuickFiler/Interfaces/IQfcDatamodel.cs` and every file named in D-13 are absent from the list.

### Phase 2 — QfcDatamodel.cs Test Coverage

Forty test cases from `research/2026-08-08T00-43-qfcdatamodel.md` §8, one task each, across five new test
files. Every task uses MSTest, Moq and FluentAssertions with Arrange–Act–Assert; no `Thread.Sleep`, no
`Task.Delay`, no wall-clock wait, no temporary file, no external process, no live form, no modal dialog, no
STA apartment. Shared helpers (`CreateUninitializedDatamodel`, `SetPrivateField`, `CreateTwoRowEmailFrame`,
`WaitForState`) are duplicated per test file per the convention at `QfcDatamodelLivenessTests.cs:18-24`.

- [ ] [P2-T1] Create `QuickFiler.Test/Controllers/QfcDatamodelLifecycleTests.cs` with the `[TestClass]` shell and the shared arrangement helpers
  - Acceptance: file declares namespace mirroring the production tree, `[TestClass]`, and private helpers `CreateUninitializedDatamodel()` (via `FormatterServices.GetUninitializedObject`), `SetPrivateField`, `CreateTwoRowEmailFrame()`, and `CreateProgressMock()` configured per D-17; the file contains no `[TestMethod]` yet and compiles.
- [ ] [P2-T2] Add `<Compile Include="Controllers\QfcDatamodelLifecycleTests.cs" />` to `QuickFiler.Test/QuickFiler.Test.csproj`
  - Acceptance: the entry sits beside the existing `Controllers\QfcDatamodelTests.cs` item near `:114`; a build shows the file compiled.
- [ ] [P2-T3] Add test `Constructor_WithInjectedFrameBuilder_AssignsGlobalsExplorerAndFrame` (M3 via S3, positive) to `QuickFiler.Test/Controllers/QfcDatamodelLifecycleTests.cs`
  - Acceptance: mocked `Explorer`, `Application.ActiveExplorer()`, `IOlObjects.App`, `IApplicationGlobals.Ol`; a frame builder returning a fixture frame; after `new QfcDatamodel(globals, CancellationToken.None, e => frame)` the reflected `_globals`, `_olApp`, `_activeExplorer` and `_frame` are the supplied instances and the builder received the explorer from `ActiveExplorer()`; test passes.
- [ ] [P2-T4] Add test `Constructor_DefaultsRemainingEmailLoaderToTheLiveLoader` (M3, positive) to `QuickFiler.Test/Controllers/QfcDatamodelLifecycleTests.cs`
  - Acceptance: after construction `RemainingEmailLoader` is non-null and its `Method.Name` is `LoadRemainingEmailsToQueueAsync`, pinning the CS0236 workaround; test passes.
- [ ] [P2-T5] Add test `Constructor_SubscribesToApplicationNewMailEx` (M3, positive) to `QuickFiler.Test/Controllers/QfcDatamodelLifecycleTests.cs`
  - Acceptance: if [P1-T21] recorded that Moq can proxy the event, the test asserts `application.VerifyAdd(a => a.NewMailEx += It.IsAny<ApplicationEvents_11_NewMailExEventHandler>(), Times.Once)`; otherwise it asserts the injected `NewMailSubscriber` delegate ran exactly once; test passes.
- [ ] [P2-T6] Add test `Constructor_WithNullFrameBuilder_FallsBackToInitDf` (M3 / S3, boundary) to `QuickFiler.Test/Controllers/QfcDatamodelLifecycleTests.cs`
  - Acceptance: constructed with a `null` builder and a `Mock<Explorer>` whose `GetTableInView()` throws a sentinel exception; the sentinel escapes the constructor, proving the null path binds `InitDf`; the test does not reach `DfDeedle` internals; test passes.
- [ ] [P2-T7] Add test `LoadAsync_ReportsZeroProgressBeforeConstructingTheModel` (M4 / S4, ordering) to `QuickFiler.Test/Controllers/QfcDatamodelLifecycleTests.cs`
  - Acceptance: a `Mock<ProgressTracker>` records call order; `Report(0, "Initializing Data Model")` is invoked and is invoked before the injected initializer; test passes.
- [ ] [P2-T8] Add test `LoadAsync_AssignsTokenAndTokenSourceToTheReturnedModel` (M4, state-transition) to `QuickFiler.Test/Controllers/QfcDatamodelLifecycleTests.cs`
  - Acceptance: the returned model's `Token` equals the supplied token and `TokenSource` is the supplied source; test passes.
- [ ] [P2-T9] Add test `LoadAsync_PassesActiveExplorerAndIncrementedProgressToTheInitializer` (M4 / S4, positive) to `QuickFiler.Test/Controllers/QfcDatamodelLifecycleTests.cs`
  - Acceptance: the initializer received the `Explorer` from `appGlobals.Ol.App.ActiveExplorer()` and the tracker returned by `progress.Increment(2)`; `Increment(2)` is verified once; test passes.
- [ ] [P2-T10] Add test `LoadAsync_WhenInitializerThrows_PropagatesAndReturnsNoModel` (M4, error-handling) to `QuickFiler.Test/Controllers/QfcDatamodelLifecycleTests.cs`
  - Acceptance: with a faulted initializer, `await act.Should().ThrowAsync<InvalidOperationException>()` holds, confirming `LoadAsync` has no swallow path; test passes.
- [ ] [P2-T11] Add test `Cleanup_CancelsTokenSourceAndBackgroundWorker` (M5, positive) to `QuickFiler.Test/Controllers/QfcDatamodelLifecycleTests.cs`
  - Acceptance: with a real `CancellationTokenSource`, a `BackgroundWorker { WorkerSupportsCancellation = true }` and a `Mock<IEmailMoveMonitor>`, after `Cleanup()` the source reports `IsCancellationRequested` and the worker reports `CancellationPending`; test passes.
- [ ] [P2-T12] Add test `Cleanup_UnsubscribesNewMailExAndUnhooksAllMonitoredItems` (M5, positive) to `QuickFiler.Test/Controllers/QfcDatamodelLifecycleTests.cs`
  - Acceptance: `moveMonitor.Verify(m => m.UnhookAll(), Times.Once)`; the `NewMailEx` unsubscribe is asserted with `VerifyRemove` or, under the [P1-T21] fallback, with the injected `NewMailUnsubscriber`; test passes.
- [ ] [P2-T13] Add test `Cleanup_NullsEveryRetainedReference` (M5, state-transition) to `QuickFiler.Test/Controllers/QfcDatamodelLifecycleTests.cs`
  - Acceptance: after `Cleanup()`, each of `_moveMonitor`, `_activeExplorer`, `_olApp`, `_globals`, `_frame`, `_masterQueue`, `_worker` reads back null by reflection; test passes.
- [ ] [P2-T14] Add test `Cleanup_WithNullTokenSourceAndWorker_DoesNotThrow` (M5, boundary) to `QuickFiler.Test/Controllers/QfcDatamodelLifecycleTests.cs`
  - Acceptance: with `_tokenSource` and `_worker` left null, `act.Should().NotThrow()` holds, covering the null-conditional arms; the test does not call `Cleanup()` twice (defect D1 is not pinned); test passes.
- [ ] [P2-T15] Create `QuickFiler.Test/Controllers/QfcDatamodelWorkerTests.cs` with the `[TestClass]` shell and shared helpers
  - Acceptance: includes `CreateUninitializedDatamodel`, `SetPrivateField`, and the bounded condition-driven `WaitForState` helper modelled on `QfcDatamodelLivenessTests.cs:54-57`; no fixed sleep is used; compiles.
- [ ] [P2-T16] Add `<Compile Include="Controllers\QfcDatamodelWorkerTests.cs" />` to `QuickFiler.Test/QuickFiler.Test.csproj`
  - Acceptance: entry present beside the other `Controllers\QfcDatamodel*Tests.cs` items; build shows the file compiled.
- [ ] [P2-T17] Add test `SetupWorker_EnablesCancellationSupportAndAttachesDoWorkHandler` (M14, positive) to `QuickFiler.Test/Controllers/QfcDatamodelWorkerTests.cs`
  - Acceptance: after `SetupWorker(worker)`, `WorkerSupportsCancellation` is true and running the worker with an injected inert `RemainingEmailLoader` reaches that loader, observed through a `TaskCompletionSource` and `WaitForState`; test passes.
- [ ] [P2-T18] Add test `SetupWorker_WhenTokenIsCancelled_RequestsWorkerCancellation` (M14, state-transition) to `QuickFiler.Test/Controllers/QfcDatamodelWorkerTests.cs`
  - Acceptance: with `_token` from a real source, `SetupWorker(worker)` followed by `cts.Cancel()` leaves `worker.CancellationPending` true, covering the `_token.Register(...)` callback; test passes.
- [ ] [P2-T19] Add test `WorkerDoWork_AssignsLoaderResultToEventArgsResult` (M15, positive) to `QuickFiler.Test/Controllers/QfcDatamodelWorkerTests.cs`
  - Acceptance: `Worker_DoWork` invoked by reflection with a `BackgroundWorker` sender and a locally constructed `DoWorkEventArgs`; after releasing the loader's `TaskCompletionSource`, `WaitForState(() => e.Result is bool)` succeeds and `e.Result` is `true`; test passes.
- [ ] [P2-T20] Add test `WorkerDoWork_WhenCancellationPending_SetsEventArgsCancel` (M15, state-transition) to `QuickFiler.Test/Controllers/QfcDatamodelWorkerTests.cs`
  - Acceptance: the loader is held open, `worker.CancelAsync()` is called, then the loader is released; `WaitForState(() => e.Cancel)` succeeds, covering the only wholly uncovered branch of M15; test passes.
- [ ] [P2-T21] Create `QuickFiler.Test/Controllers/QfcDatamodelInitEmailQueueTests.cs` with the `[TestClass]` shell and shared helpers
  - Acceptance: includes `CreateTwoRowEmailFrame()` adapted from `QfcInitEmailQueueZeroBatchTests.cs:63-87` with the six `IEmailSortInfo` columns; kept separate from `QfcInitEmailQueueZeroBatchTests.cs` so these tasks do not serialise against that issue-scoped file; compiles.
- [ ] [P2-T22] Add `<Compile Include="Controllers\QfcDatamodelInitEmailQueueTests.cs" />` to `QuickFiler.Test/QuickFiler.Test.csproj`
  - Acceptance: entry present; build shows the file compiled.
- [ ] [P2-T23] Add test `InitEmailQueue_NegativeBatchSize_ReturnsEmptyListAndStartsWorker` (M17, invalid-input) to `QuickFiler.Test/Controllers/QfcDatamodelInitEmailQueueTests.cs`
  - Acceptance: `InitEmailQueue(-1, worker)` returns a non-null empty list, `WorkerSupportsCancellation` is true, and the inert loader is reached; distinct from the existing zero-batch case; test passes.
- [ ] [P2-T24] Add test `InitEmailQueue_BatchSmallerThanRowCount_TakesRequestedRowsAndRetainsRemainder` (M17, boundary) to `QuickFiler.Test/Controllers/QfcDatamodelInitEmailQueueTests.cs`
  - Acceptance: on a two-row frame with `NameSpace.GetItemFromID` mocked, `InitEmailQueue(1, worker)` returns one item matching `EntryId-1` and leaves `_frame.RowCount == 1`, covering the currently-uncovered true arm of the clamp; test passes.
- [ ] [P2-T25] Add test `InitEmailQueue_BatchLargerThanRowCount_ClampsToRowCount` (M17, boundary) to `QuickFiler.Test/Controllers/QfcDatamodelInitEmailQueueTests.cs`
  - Acceptance: `InitEmailQueue(5, worker)` on a two-row frame returns two items, leaves `_frame.RowCount == 0`, and throws nothing; test passes.
- [ ] [P2-T26] Add test `InitEmailQueue_PositiveBatch_SetsProducerLivenessFlagBeforeStartingWorker` (M17, ordering) to `QuickFiler.Test/Controllers/QfcDatamodelInitEmailQueueTests.cs`
  - Acceptance: with the loader held open by a `TaskCompletionSource`, `_remainingLoadActive` reads true while held, covering the positive-batch twin of the already-covered zero-batch assignment; test passes.
- [ ] [P2-T27] Add test `InitEmailQueueAsync_DelegatesToInitEmailQueueAndReturnsTheSameItems` (M18, positive) to `QuickFiler.Test/Controllers/QfcDatamodelInitEmailQueueTests.cs`
  - Acceptance: `await InitEmailQueueAsync(1, worker, CancellationToken.None, new CancellationTokenSource())` returns the same list shape as the synchronous `InitEmailQueue(1, ...)` result; test passes.
- [ ] [P2-T28] Add test `InitEmailQueueAsync_AssignsTokenTokenSourceAndWorkerFields` (M18, state-transition) to `QuickFiler.Test/Controllers/QfcDatamodelInitEmailQueueTests.cs`
  - Acceptance: reflected `_token`, `_tokenSource` and `_worker` are the supplied instances; test passes.
- [ ] [P2-T29] Add test `InitEmailQueueAsync_WithPreCancelledToken_ThrowsOperationCanceled` (M18, error-handling) to `QuickFiler.Test/Controllers/QfcDatamodelInitEmailQueueTests.cs`
  - Acceptance: `await act.Should().ThrowAsync<OperationCanceledException>()` holds and `_worker` is unchanged, proving the guard short-circuits before field assignment; test passes.
- [ ] [P2-T30] Create `QuickFiler.Test/Controllers/QfcDatamodelRemainingLoadTests.cs` with the `[TestClass]` shell and shared helpers
  - Acceptance: includes `CreateUninitializedDatamodel`, `SetPrivateField`, `CreateTwoRowEmailFrame`, and a helper that wires a real `LockingLinkedList<MailItem>` into `_masterQueue` and a `Mock<IEmailMoveMonitor>` into `_moveMonitor`; compiles.
- [ ] [P2-T31] Add `<Compile Include="Controllers\QfcDatamodelRemainingLoadTests.cs" />` to `QuickFiler.Test/QuickFiler.Test.csproj`
  - Acceptance: entry present; build shows the file compiled.
- [ ] [P2-T32] Add test `LoadRemainingEmailsToQueueAsync_WithNullFrame_ReportsEmptyFrameAndReturnsFalse` (M19 / S2, invalid-input) to `QuickFiler.Test/Controllers/QfcDatamodelRemainingLoadTests.cs`
  - Acceptance: with `_frame` null and `MessageBoxInvoker` recording the message and returning `DialogResult.OK`, the result is `false`, the recorded message is `"Email Frame is empty"`, the queue is unmutated, and no modal dialog is shown; test passes.
- [ ] [P2-T33] Add test `LoadRemainingEmailsToQueueAsync_WithZeroRowFrame_ReportsEmptyFrameAndReturnsFalse` (M19 / S2, boundary) to `QuickFiler.Test/Controllers/QfcDatamodelRemainingLoadTests.cs`
  - Acceptance: as the null-frame case but with a zero-row `Frame`, covering the second disjunct of the empty-frame guard; test passes.
- [ ] [P2-T34] Add test `LoadRemainingEmailsToQueueAsync_QueuesEveryResolvedMailItemInFrameOrder` (M19, positive) to `QuickFiler.Test/Controllers/QfcDatamodelRemainingLoadTests.cs`
  - Acceptance: with a two-row frame and `GetItemFromID` returning distinct loose `Mock<MailItem>`s, the result is `true`, the queue holds both in row order, and `HookItem` was called twice; test passes.
- [ ] [P2-T35] Add test `LoadRemainingEmailsToQueueAsync_SkipsRowsThatDoNotResolveToAMailItem` (M19, negative) to `QuickFiler.Test/Controllers/QfcDatamodelRemainingLoadTests.cs`
  - Acceptance: with `GetItemFromID` returning null for row 1 and a mail item for row 2, the result is `true` and the queue holds exactly one item, covering the false arm of the resolution guard; test passes.
- [ ] [P2-T36] Add test `LoadRemainingEmailsToQueueAsync_WithPreCancelledToken_ReturnsFalseWithoutQueueing` (M19, error-handling) to `QuickFiler.Test/Controllers/QfcDatamodelRemainingLoadTests.cs`
  - Acceptance: with an already-cancelled token the result is `false`, the queue is empty, and `HookItem` was never called, covering the `OperationCanceledException` arm; test passes.
- [ ] [P2-T37] Add test `LoadRemainingEmailsToQueueAsync_WhenItemResolutionThrows_Rethrows` (M19, error-handling) to `QuickFiler.Test/Controllers/QfcDatamodelRemainingLoadTests.cs`
  - Acceptance: with `GetItemFromID` throwing `InvalidOperationException`, `await act.Should().ThrowAsync<InvalidOperationException>()` holds; the test asserts type and message only, so it survives a future `throw e;` to `throw;` fix (defect D6); test passes.
- [ ] [P2-T38] Add test `TryQueueRemainingMailItemAsync_AddsItemToMasterQueueAndHooksTheMoveMonitor` (M20, positive) to `QuickFiler.Test/Controllers/QfcDatamodelRemainingLoadTests.cs`
  - Acceptance: `await model.TryQueueRemainingMailItemAsync(mail, CancellationToken.None)` returns true, the real `LockingLinkedList<MailItem>` holds one item, and `HookItem(mail, It.IsAny<Action<MailItem>>())` was called once; this is the first test that invokes the datamodel method of this name; test passes.
- [ ] [P2-T39] Add test `TryQueueRemainingMailItemAsync_NullMailItem_ReturnsFalseWithoutTouchingTheQueue` (M20, invalid-input) to `QuickFiler.Test/Controllers/QfcDatamodelRemainingLoadTests.cs`
  - Acceptance: result is `false`, the queue is empty, and `HookItem` was never called; test passes.
- [ ] [P2-T40] Add test `TryQueueRemainingMailItemAsync_HookCallback_RemovesTheItemFromTheMasterQueue` (M20, state-transition) to `QuickFiler.Test/Controllers/QfcDatamodelRemainingLoadTests.cs`
  - Acceptance: the `Action<MailItem>` handed to `HookItem` is captured and invoked; the queue no longer contains the item, covering the removal closure that nothing exercises today; test passes.
- [ ] [P2-T41] Add test `ScoreRemainingQueueMailItemAsync_ReturnsTheScoreFromTheInjectedScoringService` (M21 / S1, positive) to `QuickFiler.Test/Controllers/QfcDatamodelRemainingLoadTests.cs`
  - Acceptance: with `Mock<IFolderScoringService>` returning `(1234L, "Some\\Folder")` and a loose `Mock<MailItem>` supplying `Subject`/`EntryID` for the log line, the reflection-invoked method returns `1234`; no live Outlook is required; test passes.
- [ ] [P2-T42] Add test `ScoreRemainingQueueMailItemAsync_PropagatesScoringServiceFailure` (M21 / S1, error-handling) to `QuickFiler.Test/Controllers/QfcDatamodelRemainingLoadTests.cs`
  - Acceptance: when the scoring mock throws, the exception escapes unchanged, pinning that M21 has no catch; test passes.
- [ ] [P2-T43] Create `QuickFiler.Test/Controllers/QfcDatamodelStateTests.cs` with the `[TestClass]` shell and shared helpers
  - Acceptance: includes `CreateUninitializedDatamodel` and `SetPrivateField`; compiles.
- [ ] [P2-T44] Add `<Compile Include="Controllers\QfcDatamodelStateTests.cs" />` to `QuickFiler.Test/QuickFiler.Test.csproj`
  - Acceptance: entry present; build shows the file compiled.
- [ ] [P2-T45] Add test `Complete_RoundTripsTheAssignedValue` (M10, state-transition) to `QuickFiler.Test/Controllers/QfcDatamodelStateTests.cs`
  - Acceptance: the property defaults to `false` and reads `true` after assignment; test passes.
- [ ] [P2-T46] Add test `MovedItems_ReturnsTheMovedMailsStackFromGlobals` (M11, positive) to `QuickFiler.Test/Controllers/QfcDatamodelStateTests.cs`
  - Acceptance: with `Mock<IAppAutoFileObjects>.MovedMails` returning a `SloStack<IMovedMailInfo>` and `Mock<IApplicationGlobals>.AF` returning it, `model.MovedItems` is the same instance; test passes.
- [ ] [P2-T47] Add test `TokenAndTokenSource_RoundTripTheAssignedValues` (M12, M13, positive) to `QuickFiler.Test/Controllers/QfcDatamodelStateTests.cs`
  - Acceptance: both getters return what was assigned; test passes.
- [ ] [P2-T48] Add test `ApplicationNewMailEx_WithResolvableMailItem_AddsItToTheFrontOfTheMasterQueue` (M25, positive/ordering) to `QuickFiler.Test/Controllers/QfcDatamodelStateTests.cs`
  - Acceptance: with the queue pre-seeded and `Session.GetItemFromID(entryId)` returning a loose `Mock<MailItem>`, the reflection-invoked handler places the new item **first**, pinning `AddFirst` rather than `AddLast`; test passes.
- [ ] [P2-T49] Add test `ApplicationNewMailEx_WhenItemIsNotAMailItem_DoesNotEnqueue` (M25, negative) to `QuickFiler.Test/Controllers/QfcDatamodelStateTests.cs`
  - Acceptance: with `GetItemFromID` returning a non-`MailItem` object the queue is unchanged, covering the false arm of the cast guard; test passes.
- [ ] [P2-T50] Add test `ApplicationNewMailEx_WhenSessionThrows_SwallowsTheException` (M25, error-handling) to `QuickFiler.Test/Controllers/QfcDatamodelStateTests.cs`
  - Acceptance: with the `Session` getter throwing, `act.Should().NotThrow()` holds and the queue is unchanged, covering the catch block; test passes.
- [ ] [P2-T51] Measure the Phase 2 test file sizes and record them to `<FEATURE>/evidence/other/file-line-counts-phase2.<ts>.md`
  - Acceptance: the artifact records a measured line count for each of the five new test files and closes with a verdict line reading either `SPLIT REQUIRED: <file>` (one line per file measuring 500 lines or more) or `SPLIT NOT REQUIRED` when all five measure below 500. This task changes no `.cs` file and no `.csproj` file.
- [ ] [P2-T52] Split every file the [P2-T51] artifact marked `SPLIT REQUIRED` into a `.Part2.cs` companion under `QuickFiler.Test/Controllers/`
  - Acceptance: if [P2-T51] recorded `SPLIT NOT REQUIRED`, this task records `NO ACTION` in the [P2-T51] artifact and makes no change — this branch is explicitly authorized. Otherwise each named file is split into a `<Name>.Part2.cs` companion declaring the same namespace and a `[TestClass] partial` twin, both halves measure below 500 lines, and no test method is lost.
- [ ] [P2-T53] Add a `<Compile Include>` entry to `QuickFiler.Test/QuickFiler.Test.csproj` for every `.Part2.cs` companion created at [P2-T52]
  - Acceptance: if [P2-T52] recorded `NO ACTION`, this task records `NO ACTION` in the [P2-T51] artifact and makes no change — this branch is explicitly authorized. Otherwise each companion carries an entry beside its parent `Controllers\` item and a build shows every companion compiled.
- [ ] [P2-T54] Run CMD-TEST-SCOPED with filter `FullyQualifiedName~QfcDatamodelLifecycleTests|FullyQualifiedName~QfcDatamodelWorkerTests|FullyQualifiedName~QfcDatamodelInitEmailQueueTests|FullyQualifiedName~QfcDatamodelRemainingLoadTests|FullyQualifiedName~QfcDatamodelStateTests` and record the result to `<FEATURE>/evidence/regression-testing/phase2-tests.<ts>.md`
  - Acceptance: artifact carries the four required fields with `EXIT_CODE: 0`; `Output Summary:` records 40 passed and 0 failed, and confirms no test in the run performed a wall-clock wait.

### Phase 3 — QfcDatamodel.QueueProcessing.cs Test Coverage

`QuickFiler/Controllers/QfcDatamodel.QueueProcessing.cs` receives **no production edit** — no seam, no split,
no attribute. Thirty-nine cases from `research/2026-08-08T00-43-qfcdatamodel-queueprocessing.md` §7, one task
each, across four new test files. Tasks [P3-T29] through [P3-T39] depend on seam **S1** from [P1-T13];
[P3-T12] depends on S1 for its assertion mechanism only. Every task in T-files C and D routes construction
through a shared `CreateModelWithFakeClock(out FakeTimeProvider fake)` helper per D-05; `Mock<MailItem>` is
loose per D-17; `TryUnhookOrReplace` and `WaitForQueue` are called **directly** (both are `internal` and
`QuickFiler/Properties/AssemblyInfo.cs:5` grants `InternalsVisibleTo("QuickFiler.Test")`), with reflection
reserved for the genuinely private `UnhookDequeuedNodes`.

- [ ] [P3-T1] Create `QuickFiler.Test/Controllers/QfcDatamodelDequeueRoutingTests.cs` with the `[TestClass]` shell and shared helpers
  - Acceptance: includes `CreateUninitializedDatamodel`, `SetPrivateField`, `CreateModelWithFakeClock(out FakeTimeProvider)`, and a real `LockingLinkedList<MailItem>` seeding helper; compiles.
- [ ] [P3-T2] Add `<Compile Include="Controllers\QfcDatamodelDequeueRoutingTests.cs" />` to `QuickFiler.Test/QuickFiler.Test.csproj`
  - Acceptance: entry present; build shows the file compiled.
- [ ] [P3-T3] Add test `UndoMove_IsNotImplemented_Throws` (N2, error-handling) to `QuickFiler.Test/Controllers/QfcDatamodelDequeueRoutingTests.cs`
  - Acceptance: `model.Invoking(m => m.UndoMove()).Should().Throw<NotImplementedException>()`; the test pins the declared-but-unimplemented `IQfcDatamodel` member as characterization (observation O1), not as endorsement; test passes.
- [ ] [P3-T4] Add test `DequeueNextItemGroupAsync_WithCancelledToken_ThrowsBeforeTouchingTheQueue` (N5, invariant I2, error-handling) to `QuickFiler.Test/Controllers/QfcDatamodelDequeueRoutingTests.cs`
  - Acceptance: with an already-cancelled `_token`, a two-item queue, and `_globals` unset, `await act.Should().ThrowAsync<OperationCanceledException>()` holds and the queue still holds two items, proving the cancellation check precedes mode selection; test passes.
- [ ] [P3-T5] Add test `DequeueNextItemGroup_WithCancelledToken_ThrowsBeforeTouchingTheQueue` (N8, invariant I2, error-handling) to `QuickFiler.Test/Controllers/QfcDatamodelDequeueRoutingTests.cs`
  - Acceptance: the same assertions for the synchronous entry point, covering its currently-uncovered guard line; test passes.
- [ ] [P3-T6] Add test `DequeueNextItemGroupAsync_WithNullGlobals_UsesTheDirectPath` (N5/N6, invariant I3, invalid-input) to `QuickFiler.Test/Controllers/QfcDatamodelDequeueRoutingTests.cs`
  - Acceptance: with `_globals` null and a two-item queue, `await model.DequeueNextItemGroupAsync(2, 0)` returns both items in order without throwing, proving the null-conditional selects the direct path; test passes.
- [ ] [P3-T7] Add test `DequeueNextItemGroupAsync_WithNullQfSettings_UsesTheDirectPath` (N5/N6, invariant I3, invalid-input) to `QuickFiler.Test/Controllers/QfcDatamodelDequeueRoutingTests.cs`
  - Acceptance: with `Mock<IApplicationGlobals>.QfSettings` returning null the direct path is taken and both items return, proving the second null-conditional; test passes.
- [ ] [P3-T8] Add test `DequeueNextItemGroupAsync_NormalMode_ReturnsQueueHeadInFifoOrder` (N6, invariants I4/I15, positive/ordering) to `QuickFiler.Test/Controllers/QfcDatamodelDequeueRoutingTests.cs`
  - Acceptance: with three items and quantity 2, the result equals the first two in insertion order, the queue retains the third as its head, and both unhooks are verified once on a strict `IEmailMoveMonitor`; distinct from the existing whole-queue-drain test; test passes.
- [ ] [P3-T9] Add test `DequeueNextItemGroupAsync_NormalModeZeroQuantity_ReturnsNull` (N6/N9, invariant I5, boundary) to `QuickFiler.Test/Controllers/QfcDatamodelDequeueRoutingTests.cs`
  - Acceptance: `DequeueNextItemGroupAsync(0, 0)` returns `null` and the queue still holds two items; the XML doc states this pins current behavior (defect D2) and is not an endorsement; test passes.
- [ ] [P3-T10] Add test `DequeueNextItemGroupAsync_NormalModeNegativeQuantity_ReturnsNull` (N6/N9, invariant I5, invalid-input) to `QuickFiler.Test/Controllers/QfcDatamodelDequeueRoutingTests.cs`
  - Acceptance: the same assertions with quantity `-1`; test passes.
- [ ] [P3-T11] Add test `DequeueNextItemGroupAsync_HighConfidenceZeroQuantity_ReturnsEmptyListNotNull` (N7, invariant I5, boundary) to `QuickFiler.Test/Controllers/QfcDatamodelDequeueRoutingTests.cs`
  - Acceptance: with high-confidence globals, a `FakeTimeProvider` assigned, and an empty queue, the result is non-null and empty, pinning the null-vs-empty asymmetry against [P3-T9]; test passes.
- [ ] [P3-T12] Add test `DequeueNextItemGroupAsync_NormalMode_NeverScoresCandidates` (N5/N6, invariant I18, negative) to `QuickFiler.Test/Controllers/QfcDatamodelDequeueRoutingTests.cs`
  - Acceptance: with high-confidence disabled and a `Mock<IFolderScoringService>` (seam S1) configured to fail the test if invoked, the batch returns normally and the scorer is never called; test passes.
- [ ] [P3-T13] Add test `DequeueNextItemGroupAsync_NormalMode_IgnoresProgressSink` (N5/N6, invariant I18, negative) to `QuickFiler.Test/Controllers/QfcDatamodelDequeueRoutingTests.cs`
  - Acceptance: the four-argument overload with `Timeout.InfiniteTimeSpan` and a throwing progress delegate completes without throwing and never invokes the sink, pinning that the extra arguments are dropped in normal mode; test passes.
- [ ] [P3-T14] Add test `DequeueNextItemGroup_NormalMode_TakesWithoutWaitingAndUnhooks` (N8, invariants I15/I19, positive) to `QuickFiler.Test/Controllers/QfcDatamodelDequeueRoutingTests.cs`
  - Acceptance: `model.DequeueNextItemGroup(2)` returns both items in order, both are unhooked on a strict monitor, and the queue is empty; no `TimeProvider` assignment is required because the synchronous direct path never waits; test passes.
- [ ] [P3-T15] Add test `DequeueNextItemGroup_NormalModeZeroQuantity_ReturnsNull` (N8/N9, invariant I5, boundary) to `QuickFiler.Test/Controllers/QfcDatamodelDequeueRoutingTests.cs`
  - Acceptance: the synchronous twin of [P3-T9] returns `null`, covering the null-batch return through the synchronous path; test passes.
- [ ] [P3-T16] Add test `DequeueNextItemGroupAsync_QueueShorterThanQuantityAndProducerIdle_ReturnsWhatIsAvailable` (N6/N10, invariant I12, boundary) to `QuickFiler.Test/Controllers/QfcDatamodelDequeueRoutingTests.cs`
  - Acceptance: with one queued item, `_remainingLoadActive` false, and a `FakeTimeProvider` that is never advanced, `DequeueNextItemGroupAsync(2, 0)` completes and returns exactly one item, covering the wait call site and proving the wait short-circuits; test passes.
- [ ] [P3-T17] Create `QuickFiler.Test/Controllers/QfcDatamodelUnhookTests.cs` with the `[TestClass]` shell and shared helpers
  - Acceptance: includes `CreateUninitializedDatamodel`, `SetPrivateField`, and helpers to seed `_masterQueue` and `_moveMonitor`; compiles.
- [ ] [P3-T18] Add `<Compile Include="Controllers\QfcDatamodelUnhookTests.cs" />` to `QuickFiler.Test/QuickFiler.Test.csproj`
  - Acceptance: entry present; build shows the file compiled.
- [ ] [P3-T19] Add test `TryUnhookOrReplace_NullNodeList_LogsAndReturnsWithoutThrowing` (N3, invariant I13, invalid-input) to `QuickFiler.Test/Controllers/QfcDatamodelUnhookTests.cs`
  - Acceptance: with `List<MailItem> nodes = null` passed by `ref` and a strict monitor with no setups, nothing throws, `nodes` is still null, and `UnhookItem` was never called; test passes.
- [ ] [P3-T20] Add test `TryUnhookOrReplace_EmptyNodeList_ReturnsWithoutThrowing` (N3, invariant I13, boundary) to `QuickFiler.Test/Controllers/QfcDatamodelUnhookTests.cs`
  - Acceptance: with an empty list at index 0, nothing throws and no unhook occurs, covering the empty-count disjunct; test passes.
- [ ] [P3-T21] Add test `TryUnhookOrReplace_IndexBeyondListLength_ReturnsWithoutThrowing` (N3, invariant I13, boundary) to `QuickFiler.Test/Controllers/QfcDatamodelUnhookTests.cs`
  - Acceptance: with a one-element list at index 1, nothing throws, the list is unchanged, and no unhook occurs, covering the range guard that absorbs batch shrink; test passes.
- [ ] [P3-T22] Add test `TryUnhookOrReplace_WhenUnhookFails_ReplacesFailedNodeInPlaceFromQueueHead` (N3, invariant I14, state-transition/ordering) to `QuickFiler.Test/Controllers/QfcDatamodelUnhookTests.cs`
  - Acceptance: with `nodes = [bad, tail]`, `_masterQueue = [repl]`, and the monitor throwing `COMException` for `bad` and succeeding for `repl`, `nodes` equals `[repl, tail]` (replacement at index 0, not appended), the master queue is empty, and each unhook was attempted once; test passes.
- [ ] [P3-T23] Add test `TryUnhookOrReplace_WhenReplacementsAlsoFail_RetriesUntilTheQueueIsExhausted` (N3, invariant I14, error-handling/loop-termination) to `QuickFiler.Test/Controllers/QfcDatamodelUnhookTests.cs`
  - Acceptance: with `nodes = [bad]`, `_masterQueue = [r1, r2]`, and the monitor throwing for all three, `UnhookItem` is called exactly three times, the master queue is empty, and `nodes` is empty, pinning loop termination; test passes.
- [ ] [P3-T24] Add test `UnhookDequeuedNodes_WhenTheBatchShrinks_StopsAtTheGuardAndReturnsTheSurvivor` (N9 with N3, invariant I13, state-transition/ordering) to `QuickFiler.Test/Controllers/QfcDatamodelUnhookTests.cs`
  - Acceptance: with `_masterQueue = [n1, n2]` and the monitor throwing for `n1`, `await DequeueNextItemGroupAsync(2, 0)` returns `[n2]` and `UnhookItem(n2)` was **never** called; the XML doc records this as characterization of defect D3; test passes.
- [ ] [P3-T25] Add test `UnhookDequeuedNodes_NullBatch_ReturnsNull` (N9, invariant I5, boundary) to `QuickFiler.Test/Controllers/QfcDatamodelUnhookTests.cs`
  - Acceptance: the private `UnhookDequeuedNodes` is invoked by reflection with a null batch and returns null; per D-10(a) this task is authored unconditionally as a direct unit-level assertion rather than relying on the transitive coverage from [P3-T9] and [P3-T15]; test passes.
- [ ] [P3-T26] Add test `UnhookDequeuedNodes_WhenUnhookingItselfThrows_LogsAndRethrows` (N9, invariant I16, error-handling) to `QuickFiler.Test/Controllers/QfcDatamodelUnhookTests.cs`
  - Acceptance: with `_masterQueue` null and a throwing monitor, the reflection-invoked call raises `TargetInvocationException` whose `InnerException` is `NullReferenceException`, covering the log-and-rethrow boundary; the XML doc documents the indirect trigger; test passes.
- [ ] [P3-T27] Create `QuickFiler.Test/Controllers/QfcDatamodelHighConfidenceDequeueTests.cs` with the `[TestClass]` shell, `CreateModelWithFakeClock(out FakeTimeProvider)`, and a scorer helper
  - Acceptance: the file declares `CreateModelWithFakeClock` so no test can forget the `TimeProvider` assignment (D-05), plus a helper configuring `Mock<IFolderScoringService>.ScoreAsync` to return a per-item score from a dictionary; all `MailItem` mocks are loose; compiles.
- [ ] [P3-T28] Add `<Compile Include="Controllers\QfcDatamodelHighConfidenceDequeueTests.cs" />` to `QuickFiler.Test/QuickFiler.Test.csproj`
  - Acceptance: entry present; build shows the file compiled.
- [ ] [P3-T29] Add test `DequeueNextItemGroupAsync_HighConfidence_ReturnsOnlyAboveThresholdItemsInQueueOrder` (N7, invariant I6, positive/ordering) to `QuickFiler.Test/Controllers/QfcDatamodelHighConfidenceDequeueTests.cs`
  - Acceptance: with threshold `0.90`, a three-item queue scoring 950 / 100 / 990 and `timeOut: 0`, the result equals the first and third items in queue order and both are unhooked; `model.TimeProvider` is assigned from `CreateModelWithFakeClock`; test passes.
- [ ] [P3-T30] Add test `DequeueNextItemGroupAsync_HighConfidence_DiscardsRejectedCandidatesFromTheMasterQueue` (N7, invariant I6, state-transition) to `QuickFiler.Test/Controllers/QfcDatamodelHighConfidenceDequeueTests.cs`
  - Acceptance: the master queue is empty afterwards and `UnhookItem` was never called for the rejected item, pinning the discard-and-leave-hooked behavior as characterization of defect D3; test passes.
- [ ] [P3-T31] Add test `DequeueNextItemGroupAsync_HighConfidence_StopsAtTheRequestedQuantity` (N7, invariant I6, boundary) to `QuickFiler.Test/Controllers/QfcDatamodelHighConfidenceDequeueTests.cs`
  - Acceptance: with four above-cutoff items and quantity 2, the first two return, the queue retains the remaining two, and the scorer was invoked exactly twice, proving the gate does not over-scan; test passes.
- [ ] [P3-T32] Add test `DequeueNextItemGroupAsync_HighConfidence_EmptyQueueWithIdleProducer_ReturnsEmpty` (N7, invariant I9, boundary) to `QuickFiler.Test/Controllers/QfcDatamodelHighConfidenceDequeueTests.cs`
  - Acceptance: with an empty queue, `_remainingLoadActive` false and `timeOut: 0`, the result is empty and non-null with no clock advance required; test passes.
- [ ] [P3-T33] Add test `DequeueNextItemGroupAsync_TwoArgumentOverload_AppliesTheTwelveSecondDefaultDeadline` (N4/N7, invariants I1/I7, ordering/timeout) to `QuickFiler.Test/Controllers/QfcDatamodelHighConfidenceDequeueTests.cs`
  - Acceptance: with 20 below-cutoff items, quantity 5, `timeOut: 0`, and the scorer callback advancing the `FakeTimeProvider` by one second per invocation, the result is empty and the scorer was invoked **exactly 12 times**; the clock advances only from inside the scorer, so the test is fully deterministic and performs no wall-clock wait; test passes.
- [ ] [P3-T34] Add test `DequeueNextItemGroupAsync_FourArgumentOverload_HonoursAnExplicitDeadline` (N5/N7, invariant I7, boundary) to `QuickFiler.Test/Controllers/QfcDatamodelHighConfidenceDequeueTests.cs`
  - Acceptance: as [P3-T33] but with an explicit three-second deadline; the scorer was invoked exactly three times and the result is empty, proving the parameter is threaded rather than ignored; test passes.
- [ ] [P3-T35] Add test `DequeueNextItemGroupAsync_WithInfiniteDeadline_ScansTheWholeQueue` (N5/N7, invariant I7, boundary) to `QuickFiler.Test/Controllers/QfcDatamodelHighConfidenceDequeueTests.cs`
  - Acceptance: with `Timeout.InfiniteTimeSpan` and 20 below-cutoff items, the scorer was invoked exactly 20 times, the result is empty and the queue is empty, covering the deadline-disabled arm; test passes.
- [ ] [P3-T36] Add test `DequeueNextItemGroupAsync_ReportsProgressPerScoredCandidateAfterTheAcceptDecision` (N5/N7, invariant I18, ordering) to `QuickFiler.Test/Controllers/QfcDatamodelHighConfidenceDequeueTests.cs`
  - Acceptance: with three items scoring 950 / 100 / 990, quantity 5, `timeOut: 0` and a recording sink, the recorded triples are exactly `(1,1,5)`, `(2,1,5)`, `(3,2,5)`, proving one invocation per scored candidate in scan order after the accept decision; test passes.
- [ ] [P3-T37] Add test `DequeueNextItemGroupAsync_WhenTheProgressSinkThrows_TheExceptionPropagates` (N5/N7, invariant I18, error-handling) to `QuickFiler.Test/Controllers/QfcDatamodelHighConfidenceDequeueTests.cs`
  - Acceptance: with one above-cutoff item and a throwing sink, `await act.Should().ThrowAsync<InvalidOperationException>()` holds, pinning the documented fail-fast contract; test passes.
- [ ] [P3-T38] Add test `DequeueNextItemGroupAsync_WhenCancelledMidScan_ThrowsAndStopsScanning` (N5/N7, invariant I2, error-handling/concurrency) to `QuickFiler.Test/Controllers/QfcDatamodelHighConfidenceDequeueTests.cs`
  - Acceptance: with three items and the scorer cancelling the source backing `_token` on its first invocation, `await act.Should().ThrowAsync<OperationCanceledException>()` holds and the scorer was invoked exactly once; no timing dependency is introduced; test passes.
- [ ] [P3-T39] Add test `DequeueNextItemGroup_HighConfidence_BlocksOnTheGateAndReturnsTheFilteredBatch` (N8/N7, invariants I7/I8, positive) to `QuickFiler.Test/Controllers/QfcDatamodelHighConfidenceDequeueTests.cs`
  - Acceptance: with items scoring 950 / 100, high-confidence enabled and a `FakeTimeProvider` assigned, the synchronous `model.DequeueNextItemGroup(2)` returns the single above-cutoff item and empties the queue, covering the synchronous high-confidence arm and pinning its `timeOut: 0` behavior; test passes.
- [ ] [P3-T40] Create `QuickFiler.Test/Controllers/QfcDatamodelWaitForQueueTests.cs` with the `[TestClass]` shell and `CreateModelWithFakeClock(out FakeTimeProvider)`
  - Acceptance: the helper assigns `model.TimeProvider` so no test can omit it; `WaitForQueue` is called directly as an `internal` member; compiles.
- [ ] [P3-T41] Add `<Compile Include="Controllers\QfcDatamodelWaitForQueueTests.cs" />` to `QuickFiler.Test/QuickFiler.Test.csproj`
  - Acceptance: entry present; build shows the file compiled.
- [ ] [P3-T42] Add test `WaitForQueue_WhenProducerIsIdle_ReturnsWithoutDelaying` (N10, invariant I11, boundary) to `QuickFiler.Test/Controllers/QfcDatamodelWaitForQueueTests.cs`
  - Acceptance: with `_remainingLoadActive` false and an empty queue, the returned task is already completed with no clock advance, pinning the first disjunct's short-circuit; test passes.
- [ ] [P3-T43] Add test `WaitForQueue_WhenQueueAlreadyHoldsQuantity_ReturnsWithoutDelaying` (N10, invariant I11, boundary) to `QuickFiler.Test/Controllers/QfcDatamodelWaitForQueueTests.cs`
  - Acceptance: with the producer active and the queue already holding the requested quantity, the task completes with no clock advance, pinning the second disjunct; test passes.
- [ ] [P3-T44] Add test `WaitForQueue_WhenTheQueueFillsWhileWaiting_ExitsOnTheNextPoll` (N10, invariant I11, state-transition) to `QuickFiler.Test/Controllers/QfcDatamodelWaitForQueueTests.cs`
  - Acceptance: the task is started and asserted pending, an item is added, the fake clock advances 200 ms, the task completes, and the item remains in the queue (the wait does not consume); test passes.
- [ ] [P3-T45] Add test `WaitForQueue_DelayIsExactlyTwoHundredMilliseconds` (N10, invariant I10, boundary) to `QuickFiler.Test/Controllers/QfcDatamodelWaitForQueueTests.cs`
  - Acceptance: after advancing 199 ms the task is still pending; after making the exit condition true and advancing a further 1 ms it completes, pinning the poll interval magnitude; test passes.
- [ ] [P3-T46] Add test `WaitForQueue_WhenCancelledWhileWaiting_ThrowsOperationCanceled` (N10, invariant I2, error-handling) to `QuickFiler.Test/Controllers/QfcDatamodelWaitForQueueTests.cs`
  - Acceptance: with the producer active and an empty queue, cancelling the source and advancing 200 ms yields `await act.Should().ThrowAsync<OperationCanceledException>()`; test passes.
- [ ] [P3-T47] Add test `WaitForQueue_WithNullMasterQueue_ReturnsWithoutDelaying` (N10, invariant I11, invalid-input) to `QuickFiler.Test/Controllers/QfcDatamodelWaitForQueueTests.cs`
  - Acceptance: with `_masterQueue` left null and `_remainingLoadActive` true, the task completes immediately, documenting the nullable-comparison behavior of the loop guard; test passes.
- [ ] [P3-T48] Measure the Phase 3 test file sizes and record them to `<FEATURE>/evidence/other/file-line-counts-phase3.<ts>.md`
  - Acceptance: the artifact records a measured line count for each of the four new test files, re-measures `QuickFiler/Controllers/QfcDatamodel.QueueProcessing.cs` at 177 lines and confirms it unmodified, and closes with a verdict line reading either `SPLIT REQUIRED: <file>` (one line per file measuring 500 lines or more — `QfcDatamodelHighConfidenceDequeueTests.cs` is the most likely candidate) or `SPLIT NOT REQUIRED`. This task changes no `.cs` file and no `.csproj` file.
- [ ] [P3-T49] Split every file the [P3-T48] artifact marked `SPLIT REQUIRED` into a `.Part2.cs` companion under `QuickFiler.Test/Controllers/`
  - Acceptance: if [P3-T48] recorded `SPLIT NOT REQUIRED`, this task records `NO ACTION` in the [P3-T48] artifact and makes no change — this branch is explicitly authorized. Otherwise each named file is split into a `<Name>.Part2.cs` companion declaring the same namespace and a `[TestClass] partial` twin, both halves measure below 500 lines, and no test method is lost.
- [ ] [P3-T50] Add a `<Compile Include>` entry to `QuickFiler.Test/QuickFiler.Test.csproj` for every `.Part2.cs` companion created at [P3-T49]
  - Acceptance: if [P3-T49] recorded `NO ACTION`, this task records `NO ACTION` in the [P3-T48] artifact and makes no change — this branch is explicitly authorized. Otherwise each companion carries an entry beside its parent `Controllers\` item and a build shows every companion compiled.
- [ ] [P3-T51] Run CMD-TEST-SCOPED with filter `FullyQualifiedName~QfcDatamodelDequeueRoutingTests|FullyQualifiedName~QfcDatamodelUnhookTests|FullyQualifiedName~QfcDatamodelHighConfidenceDequeueTests|FullyQualifiedName~QfcDatamodelWaitForQueueTests` and record the result to `<FEATURE>/evidence/regression-testing/phase3-tests.<ts>.md`
  - Acceptance: artifact carries the four required fields with `EXIT_CODE: 0`; `Output Summary:` records 39 passed and 0 failed, and states the total elapsed time, which must be inconsistent with any real 12-second deadline having elapsed.

### Phase 4 — QfcDatamodel.FrameBuilding.cs Seam Call Sites

Production-only phase. The S5 and S6 declarations already landed in `QfcDatamodel.Construction.cs` at
[P1-T19] and [P1-T20] (D-02), so this file's production edit is limited to two call sites.

- [ ] [P4-T1] Consume seam **S5** at the synchronous data-frame fetch in `QuickFiler/Controllers/QfcDatamodel.FrameBuilding.cs:15`
  - Acceptance: the line reads `var df = (EmailDataInViewProvider ?? DfDeedle.GetEmailDataInView)(activeExplorer);`; the surrounding pipeline and `InitDf`'s signature are unchanged; the file gains no new `using` because S5's declaration lives in `QfcDatamodel.Construction.cs`.
- [ ] [P4-T2] Consume seam **S6** at the asynchronous data-frame fetch in `QuickFiler/Controllers/QfcDatamodel.FrameBuilding.cs:82-89`
  - Acceptance: the call reads `var fetch = EmailDataInViewAsyncProvider ?? DfDeedle.GetEmailDataInViewAsync;` followed by `df = await fetch(activeExplorer, Token, TokenSource, progress.Increment(3).SpawnChild(78)).ConfigureAwait(false);`; the argument order and the `Increment(3).SpawnChild(78)` progress allocation are preserved exactly.
- [ ] [P4-T3] Verify `.ConfigureAwait(false)` is preserved verbatim at `QuickFiler/Controllers/QfcDatamodel.FrameBuilding.cs:50` and at the edited S6 await, and record it to `<FEATURE>/evidence/other/configureawait-review.<ts>.md`
  - Acceptance: the artifact quotes both await expressions post-edit and confirms both carry `.ConfigureAwait(false)`; it also confirms the awaited chain reached from `ScoreRemainingQueueMailItemAsync` is unchanged (D-16). Dropping either would deadlock the Outlook UI thread.
- [ ] [P4-T4] Verify the size of `QuickFiler/Controllers/QfcDatamodel.FrameBuilding.cs` and record it to `<FEATURE>/evidence/other/file-line-counts-phase4.<ts>.md`
  - Acceptance: measured line count is **below 500** (expected approximately 158 before the Phase 5 extraction).
- [ ] [P4-T5] Run CMD-ANALYZER and record the Phase 4 build gate to `<FEATURE>/evidence/qa-gates/phase4-analyzer-build.<ts>.md`
  - Acceptance: artifact carries the four required fields with `EXIT_CODE: 0`; zero errors and no new warnings relative to [P0-T8].

### Phase 5 — QfcEmailFrameShaper Host-Neutral Extraction

**Severable (D-09).** AC2 does not depend on this phase; it rests on S5/S6 alone. If the change budget is
tight, drop Phase 5 in full and retarget [P6-T3] through [P6-T19] at the public instance methods
`model.SortTriageDate(...)` / `model.MostRecentByConversation(...)` on an uninitialized model, folding
[P6-T18] into [P6-T22] and [P6-T30]. Every other phase is unaffected. Taken as planned, this phase is
justified by `.claude/rules/general-unit-test.md` § Coverage Exclusion Policy, `epic.md` Non-Goals
(host-neutral extraction a future WebView2/Office.js port can reuse), and `CLAUDE.md` Design Principles 2
and 4. This phase must complete before the attribute removal in Phase 10 (D-03).

- [ ] [P5-T1] Create `QuickFiler/Controllers/QfcEmailFrameShaper.cs` as an `internal static class` with `MailItemMessageClass`, `FilterToMailItems`, `MostRecentByConversation`, `SortTriageDate` and `Shape`
  - Acceptance: the file contains zero COM types, zero WinForms types and no datamodel state; `Shape(Frame<int, string>)` applies **filter then dedup then sort** in that exact order; the bodies of `SortTriageDate` and `MostRecentByConversation` are moved verbatim from `QfcDatamodel.FrameBuilding.cs`; measured size is under 500 lines.
- [ ] [P5-T2] Add `<Compile Include="Controllers\QfcEmailFrameShaper.cs" />` to `QuickFiler/QuickFiler.csproj`
  - Acceptance: the entry sits beside the existing `Controllers\QfcDatamodel*.cs` items near `:312-315`; a build shows the file compiled (a missing entry fails silently).
- [ ] [P5-T3] Replace the body of `SortTriageDate` in `QuickFiler/Controllers/QfcDatamodel.FrameBuilding.cs` with a one-line delegation to `QfcEmailFrameShaper.SortTriageDate`
  - Acceptance: the public signature is byte-identical; the defensive `df.Clone()` and the reverse-index re-keying now live in the shaper and are exercised through the wrapper.
- [ ] [P5-T4] Replace the body of `MostRecentByConversation` in `QuickFiler/Controllers/QfcDatamodel.FrameBuilding.cs` with a one-line delegation to `QfcEmailFrameShaper.MostRecentByConversation`
  - Acceptance: the public signature is byte-identical; behavior including the first-match tie-break is unchanged.
- [ ] [P5-T5] Collapse the three-step pipeline inside `InitDf` in `QuickFiler/Controllers/QfcDatamodel.FrameBuilding.cs` to a single `QfcEmailFrameShaper.Shape(df)` call
  - Acceptance: the filter, dedup and sort steps are removed from the method body and replaced by one call; `InitDf`'s signature and return value are unchanged.
- [ ] [P5-T6] Collapse the three-step pipeline inside `InitDfAsync` in `QuickFiler/Controllers/QfcDatamodel.FrameBuilding.cs` to a single `QfcEmailFrameShaper.Shape(df)` call
  - Acceptance: the duplicated pipeline is removed; the null-frame guard, the progress completion report and `InitDfAsync`'s signature are unchanged.
- [ ] [P5-T7] Verify pipeline-order preservation and record it to `<FEATURE>/evidence/other/frame-shape-order.<ts>.md`
  - Acceptance: the artifact quotes the pre-change ordering from both original call sites and the post-change `Shape` body, and states that both now resolve to filter then dedup then sort; [P6-T19] is named as the executable guardrail for this move.
- [ ] [P5-T8] Verify the Phase 5 file sizes and record them to `<FEATURE>/evidence/other/file-line-counts-phase5.<ts>.md`
  - Acceptance: `QuickFiler/Controllers/QfcEmailFrameShaper.cs` and `QuickFiler/Controllers/QfcDatamodel.FrameBuilding.cs` each measure **below 500 lines**; the artifact records that the shaper adds one file to the epic's compiled-file denominator for the F16 capstone to account for.
- [ ] [P5-T9] Run CMD-ANALYZER and record the Phase 5 build gate to `<FEATURE>/evidence/qa-gates/phase5-analyzer-build.<ts>.md`
  - Acceptance: artifact carries the four required fields with `EXIT_CODE: 0`; zero errors and no new warnings relative to [P0-T8].

### Phase 6 — QfcDatamodel.FrameBuilding.cs Test Coverage

Thirty cases from `research/2026-08-08T00-43-qfcdatamodel-framebuilding.md` §7, one task each, across five
new test files. No `*.StaTests.cs` file is created (D-06). Frame fixtures are adapted from
`CreateTwoRowEmailFrame` (`QfcInitEmailQueueZeroBatchTests.cs:63-87`) with the six `IEmailSortInfo` columns
`EntryId`, `MessageClass`, `SentOn`, `ConversationId`, `Triage`, `StoreId`. Unlike the confidence-gate paths,
a forgotten `TimeProvider` assignment fails **loudly** here (`TimeProvider.Delay` is an extension method), but
tests still assign a `FakeTimeProvider` uniformly.

- [ ] [P6-T1] Create `QuickFiler.Test/Controllers/QfcEmailFrameShaperSortTests.cs` with the `[TestClass]` shell and frame fixtures
  - Acceptance: includes builders for frames with controllable `Triage`, `SentOn`, `ConversationId`, `MessageClass` and `EntryId` values; no COM mock, no clock, no seam required; compiles.
- [ ] [P6-T2] Add `<Compile Include="Controllers\QfcEmailFrameShaperSortTests.cs" />` to `QuickFiler.Test/QuickFiler.Test.csproj`
  - Acceptance: entry present beside the other `Controllers\` items near `:116`; build shows the file compiled.
- [ ] [P6-T3] Add test `SortTriageDate_WithMixedTriageValues_OrdersImportantTriageFirst` (B5, positive) to `QuickFiler.Test/Controllers/QfcEmailFrameShaperSortTests.cs`
  - Acceptance: on a three-row frame with `Triage` values `"Z"`, `"A"`, `"B"` and identical `SentOn`, the resulting `EntryId` order is the `"A"` row, then `"B"`, then `"Z"`; test passes.
- [ ] [P6-T4] Add test `SortTriageDate_WithinTheSameTriage_OrdersMostRecentFirst` (B5, positive) to `QuickFiler.Test/Controllers/QfcEmailFrameShaperSortTests.cs`
  - Acceptance: two rows with the same `Triage` and `SentOn` of 2026-01-01 and 2026-01-05 order the later row first, pinning the date component of the composite key; test passes.
- [ ] [P6-T5] Add test `SortTriageDate_ReindexesResultRowKeysFromZeroAscending` (B5, state-transition) to `QuickFiler.Test/Controllers/QfcEmailFrameShaperSortTests.cs`
  - Acceptance: the result's row keys equal `[0, 1, 2]`, pinning the re-index and sort-by-key pair that the queue initialiser depends on; test passes.
- [ ] [P6-T6] Add test `SortTriageDate_RemovesTheTemporarySortKeyColumnFromTheResult` (B5, positive) to `QuickFiler.Test/Controllers/QfcEmailFrameShaperSortTests.cs`
  - Acceptance: the result's column keys exclude `"NewKey"` and equal the input's column set; test passes.
- [ ] [P6-T7] Add test `SortTriageDate_DoesNotMutateTheInputFrame` (B5, positive/isolation) to `QuickFiler.Test/Controllers/QfcEmailFrameShaperSortTests.cs`
  - Acceptance: the input frame's column keys and row keys are unchanged after the call, pinning the defensive clone that makes the method safe on a caller-owned frame; test passes.
- [ ] [P6-T8] Add test `SortTriageDate_WithUnrecognizedTriageValue_ThrowsKeyNotFound` (B5, error-handling) to `QuickFiler.Test/Controllers/QfcEmailFrameShaperSortTests.cs`
  - Acceptance: a row with `Triage = "Q"` produces `Should().Throw<KeyNotFoundException>()`; the XML doc records that this incidentally executes lines in the F2-owned `EmailSorter.cs` and that F5 claims no coverage credit there; test passes.
- [ ] [P6-T9] Add test `SortTriageDate_WithSingleRow_ReturnsThatRowAtKeyZero` (B5, boundary) to `QuickFiler.Test/Controllers/QfcEmailFrameShaperSortTests.cs`
  - Acceptance: a one-row frame returns `RowCount == 1` with row key `[0]` and the `EntryId` preserved, covering the degenerate case of the reverse-index arithmetic; test passes.
- [ ] [P6-T10] Add test `SortTriageDate_WithMissingTriageColumn_Throws` (B5, invalid-input) to `QuickFiler.Test/Controllers/QfcEmailFrameShaperSortTests.cs`
  - Acceptance: a frame built without a `Triage` column causes a throw; the implementer runs the test once, records the observed Deedle exception type in the test's doc comment, and asserts that exact type — a bare `Should().Throw<Exception>()` is not acceptable because it would also pass on an NRE; test passes.
- [ ] [P6-T11] Create `QuickFiler.Test/Controllers/QfcEmailFrameShaperConversationTests.cs` with the `[TestClass]` shell and frame fixtures
  - Acceptance: includes builders for multi-conversation frames with controllable `ConversationId`, `SentOn` and `MessageClass`; compiles.
- [ ] [P6-T12] Add `<Compile Include="Controllers\QfcEmailFrameShaperConversationTests.cs" />` to `QuickFiler.Test/QuickFiler.Test.csproj`
  - Acceptance: entry present; build shows the file compiled.
- [ ] [P6-T13] Add test `MostRecentByConversation_WithSeveralEmailsPerConversation_KeepsOnlyTheLatestOfEach` (B6, positive) to `QuickFiler.Test/Controllers/QfcEmailFrameShaperConversationTests.cs`
  - Acceptance: four rows across two conversations reduce to two rows whose `EntryId`s are the later of each pair; test passes.
- [ ] [P6-T14] Add test `MostRecentByConversation_WithOneEmailPerConversation_ReturnsEveryRow` (B6, boundary) to `QuickFiler.Test/Controllers/QfcEmailFrameShaperConversationTests.cs`
  - Acceptance: three rows in three distinct conversations all survive, pinning that the dedup is not lossy in the common case; test passes.
- [ ] [P6-T15] Add test `MostRecentByConversation_WithTiedMaximumSentOn_KeepsTheFirstMatchingRow` (B6, boundary) to `QuickFiler.Test/Controllers/QfcEmailFrameShaperConversationTests.cs`
  - Acceptance: two rows sharing a conversation and an identical `SentOn` reduce to exactly one row whose `EntryId` is the first in input order, pinning the currently-undocumented determinism guarantee; test passes.
- [ ] [P6-T16] Add test `MostRecentByConversation_ReturnsOrdinalRowKeysStartingAtZero` (B6, state-transition) to `QuickFiler.Test/Controllers/QfcEmailFrameShaperConversationTests.cs`
  - Acceptance: the result's row keys equal `[0, 1, 2]`; if the observed keying differs, the test records actual behavior and the extraction is unaffected because the call moved verbatim; test passes.
- [ ] [P6-T17] Add test `MostRecentByConversation_WithMissingConversationIdColumn_Throws` (B6, invalid-input) to `QuickFiler.Test/Controllers/QfcEmailFrameShaperConversationTests.cs`
  - Acceptance: a frame without a `ConversationId` column causes a throw; the observed exception type is recorded in the doc comment and asserted explicitly, as in [P6-T10]; test passes.
- [ ] [P6-T18] Add test `FilterToMailItems_DropsRowsWhoseMessageClassIsNotIpmNote` (pipeline, positive) to `QuickFiler.Test/Controllers/QfcEmailFrameShaperConversationTests.cs`
  - Acceptance: from `MessageClass` values `"IPM.Note"`, `"IPM.Appointment"`, `"IPM.Note"`, two rows survive and both are `"IPM.Note"`; if Phase 5 was severed this task is dropped and the scenario folds into [P6-T22] and [P6-T30] per D-09; test passes.
- [ ] [P6-T19] Add test `Shape_FiltersNonMailItemsBeforeSelectingTheMostRecentPerConversation` (pipeline, ordering) to `QuickFiler.Test/Controllers/QfcEmailFrameShaperConversationTests.cs`
  - Acceptance: for one conversation containing an `"IPM.Appointment"` row with the **latest** `SentOn` and an `"IPM.Note"` row with an earlier `SentOn`, the surviving row is the `"IPM.Note"` one; this is the only test that distinguishes filter-then-dedup from dedup-then-filter and is the guardrail for [P5-T7]; test passes.
- [ ] [P6-T20] Create `QuickFiler.Test/Controllers/QfcDatamodelInitDfTests.cs` with the `[TestClass]` shell and shared helpers
  - Acceptance: includes `CreateUninitializedDatamodel`, `SetPrivateField` and a `FakeTimeProvider` assignment helper; compiles.
- [ ] [P6-T21] Add `<Compile Include="Controllers\QfcDatamodelInitDfTests.cs" />` to `QuickFiler.Test/QuickFiler.Test.csproj`
  - Acceptance: entry present; build shows the file compiled.
- [ ] [P6-T22] Add test `InitDf_ReturnsTheShapedFrameFromTheInjectedDataSource` (B1 via S5, positive) to `QuickFiler.Test/Controllers/QfcDatamodelInitDfTests.cs`
  - Acceptance: with `EmailDataInViewProvider` returning a frame of one `"IPM.Appointment"` row and two `"IPM.Note"` rows sharing a conversation, `model.InitDf(explorer.Object)` returns a single row — the later of the two notes — at row key 0; no modal dialog is shown and `DfDeedle` internals are never reached; test passes.
- [ ] [P6-T23] Add test `InitDf_PassesTheSuppliedExplorerToTheDataSource` (B1 via S5, positive) to `QuickFiler.Test/Controllers/QfcDatamodelInitDfTests.cs`
  - Acceptance: the provider captures its argument and it is the same `Explorer` instance passed to `InitDf`, pinning the argument flow seam S3 depends on; test passes.
- [ ] [P6-T24] Add test `InitDf_WhenTheDataSourceThrows_PropagatesWithoutSwallowing` (B1 via S5, error-handling) to `QuickFiler.Test/Controllers/QfcDatamodelInitDfTests.cs`
  - Acceptance: a provider throwing `InvalidOperationException("fetch failed")` propagates with that message, pinning that a swallow here would hand the public constructor a null frame; test passes.
- [ ] [P6-T25] Add test `ToggleOfflineMode_WhenAlreadyOffline_ReturnsTrueWithoutTouchingCommandBars` (B2, boundary) to `QuickFiler.Test/Controllers/QfcDatamodelInitDfTests.cs`
  - Acceptance: with `model.TimeProvider` assigned a `FakeTimeProvider` and `_activeExplorer` a strict `Mock<Explorer>` with no setups, the reflection-invoked `ToggleOfflineMode(true)` completes without a clock advance, returns `true`, and `explorer.VerifyGet(x => x.CommandBars, Times.Never)` holds; this is the only new test B2 needs and it does not duplicate `QfcDatamodelTests.cs:250`; test passes.
- [ ] [P6-T26] Create `QuickFiler.Test/Controllers/QfcDatamodelInitDfAsyncTests.cs` with the `[TestClass]` shell, `CreateModelWithFakeClock(out FakeTimeProvider)` and `CreateProgressMock()`
  - Acceptance: `CreateProgressMock()` returns a `Mock<ProgressTracker>` constructed with a `CancellationTokenSource` whose `Increment(It.IsAny<double>())` and `SpawnChild(It.IsAny<int>())` both return the mock itself (D-17); compiles.
- [ ] [P6-T27] Add `<Compile Include="Controllers\QfcDatamodelInitDfAsyncTests.cs" />` to `QuickFiler.Test/QuickFiler.Test.csproj`
  - Acceptance: entry present; build shows the file compiled.
- [ ] [P6-T28] Add test `InitDfAsync_WithRowsReturned_AssignsTheShapedFrameAndReportsCompletion` (B3 via S6, positive) to `QuickFiler.Test/Controllers/QfcDatamodelInitDfAsyncTests.cs`
  - Acceptance: with globals reporting `Offline == true` so the toggle short-circuits and no clock advance is needed, and `EmailDataInViewAsyncProvider` returning a three-row frame, the reflected `_frame` has the deduped and sorted shape and `progress.Verify(p => p.Report(100), Times.Once)` holds; test passes.
- [ ] [P6-T29] Add test `InitDfAsync_WhenTheDataSourceReturnsNull_LeavesTheFrameUnchangedAndDoesNotReportCompletion` (B3 via S6, invalid-input) to `QuickFiler.Test/Controllers/QfcDatamodelInitDfAsyncTests.cs`
  - Acceptance: with `_frame` pre-seeded with a sentinel and the provider returning a null frame, `_frame` is still the sentinel instance and `Report(100)` was never called, covering the cancellation-tolerant path that must not clobber an existing frame; test passes.
- [ ] [P6-T30] Add test `InitDfAsync_DropsNonMailItemRowsBeforeAssigningTheFrame` (B3 via S6, positive) to `QuickFiler.Test/Controllers/QfcDatamodelInitDfAsyncTests.cs`
  - Acceptance: a provider frame containing an `"IPM.Appointment"` row yields a `_frame` that excludes it, covering the asynchronous path's own filter line rather than relying on [P6-T22]; test passes.
- [ ] [P6-T31] Add test `InitDfAsync_WhenTheDataSourceThrows_PropagatesAndLeavesTheFrameUnchanged` (B3 via S6, error-handling) to `QuickFiler.Test/Controllers/QfcDatamodelInitDfAsyncTests.cs`
  - Acceptance: with `_frame` pre-seeded and the provider throwing, `await act.Should().ThrowAsync<InvalidOperationException>()` holds and `_frame` is still the sentinel, pinning that the async initialiser adds no catch of its own; test passes.
- [ ] [P6-T32] Create `QuickFiler.Test/Controllers/QfcDatamodelEmailsInViewTests.cs` with the `[TestClass]` shell, `CreateModelWithFakeClock(out FakeTimeProvider)`, `CreateProgressMock()` and an ordered call-log helper
  - Acceptance: the call-log helper records `ExecuteMso` invocations and provider invocations into one ordered list so ordering assertions need no timing; compiles.
- [ ] [P6-T33] Add `<Compile Include="Controllers\QfcDatamodelEmailsInViewTests.cs" />` to `QuickFiler.Test/QuickFiler.Test.csproj`
  - Acceptance: entry present; build shows the file compiled.
- [ ] [P6-T34] Add test `GetEmailsInViewDfAsync_WhenOutlookIsOnline_TogglesBeforeTheFetchAndRestoresAfter` (B4, ordering) to `QuickFiler.Test/Controllers/QfcDatamodelEmailsInViewTests.cs`
  - Acceptance: with `Offline == false`, the ordered log is `["ToggleOnline", "fetch", "ToggleOnline"]`; every wait is satisfied by a `FakeTimeProvider` advance of 5 ms, never by a real wait; test passes.
- [ ] [P6-T35] Add test `GetEmailsInViewDfAsync_WhenOutlookIsAlreadyOffline_NeverTouchesCommandBars` (B4, boundary) to `QuickFiler.Test/Controllers/QfcDatamodelEmailsInViewTests.cs`
  - Acceptance: with `Offline == true` the result is the provider's frame, `explorer.VerifyGet(x => x.CommandBars, Times.Never)` holds, and no clock advance was required; test passes.
- [ ] [P6-T36] Add test `GetEmailsInViewDfAsync_ReturnsTheFrameFromTheDataSourceUnmodified` (B4, positive) to `QuickFiler.Test/Controllers/QfcDatamodelEmailsInViewTests.cs`
  - Acceptance: the result is the **same reference** the provider returned, pinning that this member performs no shaping; test passes.
- [ ] [P6-T37] Add test `GetEmailsInViewDfAsync_PassesTokenTokenSourceAndAChildProgressTrackerToTheDataSource` (B4 via S6, positive) to `QuickFiler.Test/Controllers/QfcDatamodelEmailsInViewTests.cs`
  - Acceptance: the provider received the model's `Token` and `TokenSource`, and `progress.Verify(p => p.Increment(3), Times.Once)` and `progress.Verify(p => p.SpawnChild(78), Times.Once)` both hold, pinning the progress allocation the startup band depends on; test passes.
- [ ] [P6-T38] Add test `GetEmailsInViewDfAsync_WhenTheFetchIsCancelled_RestoresOnlineStateAndReturnsNull` (B4 via S6, error-handling) to `QuickFiler.Test/Controllers/QfcDatamodelEmailsInViewTests.cs`
  - Acceptance: with `Offline == false` and the provider returning a task faulted with `TaskCanceledException`, the result is `null` rather than an exception and `ExecuteMso("ToggleOnline")` was invoked twice, proving the restore ran; test passes.
- [ ] [P6-T39] Add test `GetEmailsInViewDfAsync_WhenTheFetchFailsUnexpectedly_RestoresOnlineStateThenRethrows` (B4 via S6, error-handling) to `QuickFiler.Test/Controllers/QfcDatamodelEmailsInViewTests.cs`
  - Acceptance: a provider throwing `InvalidOperationException("boom")` yields that exception type and message and `ExecuteMso("ToggleOnline")` invoked twice; the test asserts type and message only, so it survives a future `throw e;` to `throw;` fix (defect D6); test passes.
- [ ] [P6-T40] Add test `GetEmailsInViewDfAsync_WhenTheOfflineProbeThrows_PropagatesWithoutFetchingOrRestoring` (B4 via S6, error-handling) to `QuickFiler.Test/Controllers/QfcDatamodelEmailsInViewTests.cs`
  - Acceptance: with the `NamespaceMAPI` getter throwing `COMException`, the exception escapes, the provider was never invoked, and `ExecuteMso` was never called; the XML doc records this as characterization of defect D8 (the probe sits outside the try), not an endorsement; test passes.
- [ ] [P6-T41] Measure the Phase 6 test file sizes and record them to `<FEATURE>/evidence/other/file-line-counts-phase6.<ts>.md`
  - Acceptance: the artifact records a measured line count for each of the five new test files and closes with a verdict line reading either `SPLIT REQUIRED: <file>` (one line per file measuring 500 lines or more) or `SPLIT NOT REQUIRED`. This task changes no `.cs` file and no `.csproj` file.
- [ ] [P6-T42] Split every file the [P6-T41] artifact marked `SPLIT REQUIRED` into a `.Part2.cs` companion under `QuickFiler.Test/Controllers/`
  - Acceptance: if [P6-T41] recorded `SPLIT NOT REQUIRED`, this task records `NO ACTION` in the [P6-T41] artifact and makes no change — this branch is explicitly authorized. Otherwise each named file is split into a `<Name>.Part2.cs` companion declaring the same namespace and a `[TestClass] partial` twin, both halves measure below 500 lines, and no test method is lost.
- [ ] [P6-T43] Add a `<Compile Include>` entry to `QuickFiler.Test/QuickFiler.Test.csproj` for every `.Part2.cs` companion created at [P6-T42]
  - Acceptance: if [P6-T42] recorded `NO ACTION`, this task records `NO ACTION` in the [P6-T41] artifact and makes no change — this branch is explicitly authorized. Otherwise each companion carries an entry beside its parent `Controllers\` item and a build shows every companion compiled.
- [ ] [P6-T44] Run CMD-TEST-SCOPED with filter `FullyQualifiedName~QfcEmailFrameShaperSortTests|FullyQualifiedName~QfcEmailFrameShaperConversationTests|FullyQualifiedName~QfcDatamodelInitDfTests|FullyQualifiedName~QfcDatamodelInitDfAsyncTests|FullyQualifiedName~QfcDatamodelEmailsInViewTests` and record the result to `<FEATURE>/evidence/regression-testing/phase6-tests.<ts>.md`
  - Acceptance: artifact carries the four required fields with `EXIT_CODE: 0`; `Output Summary:` records 30 passed and 0 failed and confirms no modal dialog appeared during the run.

### Phase 7 — EfcDataModel.cs Seams and Pure-Function Extraction

Production-only phase, independent of Phases 1–6 in both directions. Seams **E1–E7** per `spec.md` §6.2. All
are additive `internal` members plus behavior-preserving restructuring of three method bodies; no signature,
accessibility or return type consumed by `EfcHomeController*`, `EfcFormController.cs`, `EfcItemController.cs`
or `EfcHomeControllerDependencies.cs` changes. `EfcDataModel.cs` is 397 lines with 103 lines of headroom;
size is measured at [P7-T16], not assumed at the end.

- [ ] [P7-T1] Declare seam **E1** in `QuickFiler/Controllers/EfcDataModel.cs` — `internal IFolderSearchHandler FolderSearchOverride { get; set; }` plus `private IFolderSearchHandler FolderSearchHandler => FolderSearchOverride ?? _folderHelper;`
  - Acceptance: reuses the existing `UtilitiesCS.IFolderSearchHandler` that `FolderPredictor` already implements, with **no change to `UtilitiesCS`**; the public `FolderHelper` property stays typed `FolderPredictor` so `EfcFormController.cs:492,771,891,1037` are unaffected.
- [ ] [P7-T2] Consume seam **E1** in `FindMatches` at `QuickFiler/Controllers/EfcDataModel.cs:381`
  - Acceptance: the call reads `FolderSearchHandler.FindFolder(...)` with the same four named arguments in the same order; `FindMatches`'s `public string[] FindMatches(string)` signature is unchanged.
- [ ] [P7-T3] Declare seam **E2** in `QuickFiler/Controllers/EfcDataModel.cs` — `internal Func<IApplicationGlobals, FolderPredictor> FolderPredictorEmptyFactory` and `internal Func<IApplicationGlobals, object, FolderPredictor.InitOptions, Task<FolderPredictor>> FolderPredictorInitializer`, each defaulted with `??=` in both constructors
  - Acceptance: defaults are `globals => new FolderPredictor(globals)` and `(globals, item, options) => new FolderPredictor(globals, item, options).InitAsync(item, options)`, mirroring the `QfcItemController.Initialization.cs:389-397` precedent; both constructor signatures are unchanged so `EfcHomeControllerDependencies` delegate compatibility is preserved.
- [ ] [P7-T4] Restructure `InitFolderHandlerAsync` at `QuickFiler/Controllers/EfcDataModel.cs:179-212` to route all three branches through seam **E2**
  - Acceptance: the three branches keep their existing conditions and their `Task.Run(..., Token)` wrappers; the `FromField` and `FromArrayOrString` option values are passed unchanged; `public async Task InitFolderHandlerAsync(object folderList = null)` keeps its signature so `EfcFormController.cs:1033` binds identically.
- [ ] [P7-T5] Declare seam **E3** in `QuickFiler/Controllers/EfcDataModel.cs` — `internal Func<EmailFilerConfig, IList<MailItemHelper>, Task<bool>> SortAsyncAction`, `internal Func<EmailFilerConfig, Task> OpenOlFolderAction`, `internal Func<EmailFilerConfig, Task> OpenFsFolderAction`
  - Acceptance: each default is one physical line carrying the **whole construct-and-invoke step** — `(config, helpers) => new EmailFiler(config).SortAsync(helpers)`, `config => new EmailFiler(config).OpenOlFolderAsync()`, `config => new EmailFiler(config).OpenFileSystemFolderAsync()` — because the `EmailFiler` methods are non-virtual and a construction-only factory seam would not intercept them.
- [ ] [P7-T6] Consume seam **E3** at the sorter invocation in `MoveToFolderAsync(string, …)`, `QuickFiler/Controllers/EfcDataModel.cs:292-293`
  - Acceptance: the `EmailFilerConfig` is built exactly as before and handed to `SortAsyncAction`; `SortEmail.Cleanup_Files()` remains in the covered path and is recorded in the policy audit as a known static touch; the method's return value semantics are unchanged.
- [ ] [P7-T7] Consume seam **E3** at the folder-open invocation in `OpenOlFolderAsync`, `QuickFiler/Controllers/EfcDataModel.cs:313-314`
  - Acceptance: the `"OneDrive"` guard and its silent return are unchanged; the config construction is unchanged; the invocation routes through `OpenOlFolderAction`.
- [ ] [P7-T8] Consume seam **E3** at the folder-open invocation in `OpenFsFolderAsync`, `QuickFiler/Controllers/EfcDataModel.cs:331-332`
  - Acceptance: as [P7-T7] for the file-system path; no `Process.Start` is reachable from a test after this edit.
- [ ] [P7-T9] Declare seam **E4** in `QuickFiler/Controllers/EfcDataModel.cs` — `internal Action<string> MoveFailureMessageAction { get; set; } = text => MessageBox.Show(text);`
  - Acceptance: copied verbatim in shape from the `EfcHomeController.ExecuteMoves.cs:22-23` precedent; declared as an instance property.
- [ ] [P7-T10] Consume seam **E4** at the failure-dialog call site in `MoveToFolderAsync(MAPIFolder, …)`, `QuickFiler/Controllers/EfcDataModel.cs:358`
  - Acceptance: the interpolated message text `"Cannot move to folderpath {folderpath}"` is unchanged; no modal dialog is reachable from a test after this edit.
- [ ] [P7-T11] Declare seam **E5** in `QuickFiler/Controllers/EfcDataModel.cs` — `internal Action<MailItem> RefreshSuggestionsAction { get; set; }`
  - Acceptance: a delegate rather than an interface member, because adding `RefreshSuggestions` to `IFolderSearchHandler` would edit `UtilitiesCS` (spec R6); no property initializer is used.
- [ ] [P7-T12] Consume seam **E5** at `QuickFiler/Controllers/EfcDataModel.cs:392`
  - Acceptance: the line reads `(RefreshSuggestionsAction ?? (mail => _folderHelper.RefreshSuggestions(mailItem: mail)))(Mail);`; `public void RefreshSuggestions()` keeps its signature so `EfcFormController.cs:799` binds identically.
- [ ] [P7-T13] Extract **E6** pure function `internal static string BuildSearchPattern(string searchText)` from `QuickFiler/Controllers/EfcDataModel.cs:376-379` and call it from `FindMatches`
  - Acceptance: the extracted body reproduces the existing wrap-in-wildcards logic exactly including the `searchText != ""` guard; `FindMatches` calls the new method and its observable output is unchanged.
- [ ] [P7-T14] Extract **E6** pure function `internal static bool ShouldSaveAttachments(string folderpath, bool saveAttachments)` from `QuickFiler/Controllers/EfcDataModel.cs:271` and call it from `MoveToFolderAsync(string, …)`
  - Acceptance: the extracted body reproduces the `"Trash to Delete"` suppression exactly; the literal folder name is preserved byte-for-byte.
- [ ] [P7-T15] Extract **E6** pure function `internal static string StripAncestorPrefix(string folderPath, string olAncestor)` from `QuickFiler/Controllers/EfcDataModel.cs:344-348` and call it from `MoveToFolderAsync(MAPIFolder, …)`
  - Acceptance: the extracted body reproduces the ancestor strip and the single-leading-separator trim exactly; the caller's behavior for a path that does not contain the ancestor is unchanged.
- [ ] [P7-T16] Measure `QuickFiler/Controllers/EfcDataModel.cs` after every seam edit and record it to `<FEATURE>/evidence/other/file-line-counts-phase7.<ts>.md`
  - Acceptance: the artifact records the measured line count (projected 435–450) and states explicitly whether it exceeds 480, which is the trigger for [P7-T17].
- [ ] [P7-T17] Record the size-contingency decision for `QuickFiler/Controllers/EfcDataModel.cs` in the [P7-T16] artifact
  - Acceptance: the artifact gains a verdict line reading `CONTINGENCY NOT TRIGGERED` with the [P7-T16] measured count when that count is **480 lines or fewer**, or `CONTINGENCY TRIGGERED` with the measured count when it exceeds 480. This task records the decision only; it changes no `.cs` file and no `.csproj` file, and the `NOT TRIGGERED` branch is explicitly authorized as a complete outcome.
- [ ] [P7-T18] Add `partial` to the `EfcDataModel` class declaration and create `QuickFiler/Controllers/EfcDataModel.Seams.cs` holding the E1–E5 declarations
  - Acceptance: if [P7-T17] recorded `CONTINGENCY NOT TRIGGERED`, this task records `NO ACTION` in the [P7-T16] artifact and makes no change — this branch is explicitly authorized. Otherwise the class declaration in `EfcDataModel.cs` gains `partial`, the new file declares the same namespace and `partial class EfcDataModel`, and the E1–E5 seam declarations move into it verbatim with no signature or accessibility change.
- [ ] [P7-T19] Add `<Compile Include="Controllers\EfcDataModel.Seams.cs" />` to `QuickFiler/QuickFiler.csproj`
  - Acceptance: if [P7-T18] recorded `NO ACTION`, this task records `NO ACTION` in the [P7-T16] artifact and makes no change — this branch is explicitly authorized. Otherwise the entry sits with the other `Controllers\` items near `:312-315` and a build shows the new file compiled (a missing entry fails silently).
- [ ] [P7-T20] Re-measure `QuickFiler/Controllers/EfcDataModel.cs` and `QuickFiler/Controllers/EfcDataModel.Seams.cs` and record both counts in the [P7-T16] artifact
  - Acceptance: if [P7-T18] recorded `NO ACTION`, this task records `NO ACTION` and restates the single [P7-T16] count — this branch is explicitly authorized. Otherwise both files measure **below 500 lines** and both measured counts are recorded.
- [ ] [P7-T21] Run CMD-ANALYZER and record the Phase 7 build gate to `<FEATURE>/evidence/qa-gates/phase7-analyzer-build.<ts>.md`
  - Acceptance: artifact carries the four required fields with `EXIT_CODE: 0`; zero errors and no new warnings relative to [P0-T8].
- [ ] [P7-T22] Verify that no `EfcDataModel` consumer file was modified and record the diff scope to `<FEATURE>/evidence/other/phase7-diff-scope.<ts>.md`
  - Acceptance: `git diff --name-only` shows no change to `EfcHomeController.cs`, `EfcHomeController.ExecuteMoves.cs`, `EfcHomeControllerDependencies.cs`, `EfcFormController.cs`, `EfcItemController.cs`, `UtilitiesCS/**`, or any file named in D-13; the artifact quotes the ten consumer call sites from `spec.md` §4.1 and the `EfcDataModel` consumer list and confirms each still compiles unchanged.

### Phase 8 — EfcDataModel.cs Test Coverage

Forty-five cases from `research/2026-08-08T00-43-efcdatamodel.md` §6, one task each, across one shared
support file and four new test files plus two cases appended to the existing `EfcDataModelTests.cs`. No test
constructs a live form, shows a popup, touches the filesystem, starts an external process, or waits on a real
clock. Construction uses `FormatterServices.GetUninitializedObject` plus reflection field assignment (seam E7);
`EfcDataModel` has no `TimeProvider` member, so D-05 does not apply here.

- [ ] [P8-T1] Create `QuickFiler.Test/Controllers/EfcDataModel.TestSupport.cs` with the shared fakes and helpers
  - Acceptance: the file contains **no `[TestMethod]`**; it provides `CreateUninitialized<T>()`, `SetPrivateField(EfcDataModel, string, object)`, `FakeApplicationGlobals` and `FakeFileSystemFolderPaths` (a real `ConcurrentDictionary` for `SpecialFolders`, a `Mock<IOlObjects>` for `ArchiveRootPath`/`App`), a recording `FakeFolderSearchHandler : IFolderSearchHandler`, and `CreateResolverWith(...)` building a `ConversationResolver` via `GetUninitializedObject` plus `ConversationInfo`/`ConversationItems` assignment; it mirrors the `QuickFiler.Test/Controllers/QfcItemController.TestSupport.cs` convention and measures below 500 lines; compiles.
- [ ] [P8-T2] Add `<Compile Include="Controllers\EfcDataModel.TestSupport.cs" />` to `QuickFiler.Test/QuickFiler.Test.csproj`
  - Acceptance: entry present; build shows the file compiled.
- [ ] [P8-T3] Create `QuickFiler.Test/Controllers/EfcDataModelPureLogicTests.cs` with the `[TestClass]` shell
  - Acceptance: the file targets only the E6 `internal static` helpers and needs no mock, no globals and no construction; compiles.
- [ ] [P8-T4] Add `<Compile Include="Controllers\EfcDataModelPureLogicTests.cs" />` to `QuickFiler.Test/QuickFiler.Test.csproj`
  - Acceptance: entry present; build shows the file compiled.
- [ ] [P8-T5] Add test `BuildSearchPattern_WithNonEmptyText_WrapsValueInWildcards` (E6, positive) to `QuickFiler.Test/Controllers/EfcDataModelPureLogicTests.cs`
  - Acceptance: `"invoice"` yields `"*invoice*"`; test passes.
- [ ] [P8-T6] Add test `BuildSearchPattern_WithEmptyString_ReturnsEmptyStringUnwrapped` (E6, boundary) to `QuickFiler.Test/Controllers/EfcDataModelPureLogicTests.cs`
  - Acceptance: `""` yields `""`, covering the guard's false arm; test passes.
- [ ] [P8-T7] Add test `BuildSearchPattern_WithNull_ProducesBareWildcardPair` (E6, invalid-input) to `QuickFiler.Test/Controllers/EfcDataModelPureLogicTests.cs`
  - Acceptance: `null` yields `"**"`; the XML doc states explicitly that this is a characterization of current behavior, not an endorsement (AC7); test passes.
- [ ] [P8-T8] Add test `ShouldSaveAttachments_ForTrashToDeleteFolder_ReturnsFalseEvenWhenRequested` (E6, boundary) to `QuickFiler.Test/Controllers/EfcDataModelPureLogicTests.cs`
  - Acceptance: `("Trash to Delete", true)` yields `false`; test passes.
- [ ] [P8-T9] Add test `ShouldSaveAttachments_ForOrdinaryFolder_ReturnsRequestedValue` (E6, positive) to `QuickFiler.Test/Controllers/EfcDataModelPureLogicTests.cs`
  - Acceptance: an ordinary folder path mirrors the requested flag for both `true` and `false`; test passes.
- [ ] [P8-T10] Add test `StripAncestorPrefix_WhenRemainderStartsWithBackslash_RemovesAncestorAndSeparator` (E6, positive) to `QuickFiler.Test/Controllers/EfcDataModelPureLogicTests.cs`
  - Acceptance: an archive-rooted folder path with its ancestor stripped yields the bare remainder with no leading separator; test passes.
- [ ] [P8-T11] Add test `StripAncestorPrefix_WhenRemainderHasNoLeadingBackslash_LeavesRemainderIntact` (E6, boundary) to `QuickFiler.Test/Controllers/EfcDataModelPureLogicTests.cs`
  - Acceptance: when the ancestor consumes the prefix with no separator left, no leading character is removed; test passes.
- [ ] [P8-T12] Add test `StripAncestorPrefix_WhenAncestorNotPresentInPath_ReturnsPathUnchanged` (E6, invalid-input) to `QuickFiler.Test/Controllers/EfcDataModelPureLogicTests.cs`
  - Acceptance: a path that does not contain the ancestor is returned unchanged; test passes.
- [ ] [P8-T13] Create `QuickFiler.Test/Controllers/EfcDataModelFolderHandlingTests.cs` with the `[TestClass]` shell
  - Acceptance: uses the [P8-T1] support types for construction and fakes; compiles.
- [ ] [P8-T14] Add `<Compile Include="Controllers\EfcDataModelFolderHandlingTests.cs" />` to `QuickFiler.Test/QuickFiler.Test.csproj`
  - Acceptance: entry present; build shows the file compiled.
- [ ] [P8-T15] Add test `InitFolderHandlerAsync_WithNoFolderListAndNoMailInfo_UsesEmptyPredictorFactory` (E2, positive) to `QuickFiler.Test/Controllers/EfcDataModelFolderHandlingTests.cs`
  - Acceptance: with `_conversationResolver` null, the recording empty factory is called once with the injected globals, the initializer is never called, and `FolderHelper` is the sentinel it returned; test passes.
- [ ] [P8-T16] Add test `InitFolderHandlerAsync_WithNoFolderListAndMailInfo_InitializesFromFieldWithMailInfo` (E2, positive) to `QuickFiler.Test/Controllers/EfcDataModelFolderHandlingTests.cs`
  - Acceptance: the initializer records `options == FromField` and an item identical to `MailInfo`, the empty factory is never called, and `FolderHelper` is assigned; test passes.
- [ ] [P8-T17] Add test `InitFolderHandlerAsync_WithFolderList_InitializesFromArrayOrString` (E2, positive) to `QuickFiler.Test/Controllers/EfcDataModelFolderHandlingTests.cs`
  - Acceptance: with a string-array folder list the initializer records `options == FromArrayOrString` and the same array instance, and `FolderHelper` is assigned; test passes.
- [ ] [P8-T18] Add test `InitFolderHandlerAsync_WithAlreadyCancelledToken_ThrowsWithoutInvokingFactory` (E2, concurrency) to `QuickFiler.Test/Controllers/EfcDataModelFolderHandlingTests.cs`
  - Acceptance: with `_token` from a pre-cancelled source, `await act.Should().ThrowAsync<TaskCanceledException>()` holds and neither factory was invoked; the pre-cancelled `Task.Run` completes as cancelled synchronously, so no timer and no wait are used; test passes.
- [ ] [P8-T19] Add test `InitFolderHandlerAsync_WhenFactoryThrows_PropagatesAndLeavesFolderHelperUnchanged` (E2, error-handling) to `QuickFiler.Test/Controllers/EfcDataModelFolderHandlingTests.cs`
  - Acceptance: with `_folderHelper` pre-seeded and the initializer throwing, the exception surfaces and `FolderHelper` is still the pre-seeded sentinel; test passes.
- [ ] [P8-T20] Add test `FolderHelper_AfterInitialization_ReturnsAssignedPredictor` (E7, positive) to `QuickFiler.Test/Controllers/EfcDataModelFolderHandlingTests.cs`
  - Acceptance: with `_folderHelper` set by reflection on an uninitialized model, the getter returns the same instance, covering the currently-uncovered accessor lines; test passes.
- [ ] [P8-T21] Add test `FindMatches_WithNonEmptySearchText_PassesWildcardPatternToHandler` (E1, positive) to `QuickFiler.Test/Controllers/EfcDataModelFolderHandlingTests.cs`
  - Acceptance: with `FolderSearchOverride` set to the recording fake, `FindMatches("invoice")` records `searchString == "*invoice*"`; test passes.
- [ ] [P8-T22] Add test `FindMatches_PassesFixedFlagsAndCurrentMailAsObjItem` (E1, positive) to `QuickFiler.Test/Controllers/EfcDataModelFolderHandlingTests.cs`
  - Acceptance: the recorded call has `reloadCTFStagingFiles == false`, `recalcSuggestions == false`, and `objItem` the same instance as `Mail`; test passes.
- [ ] [P8-T23] Add test `FindMatches_WithEmptySearchText_PassesEmptyPatternThrough` (E1, boundary) to `QuickFiler.Test/Controllers/EfcDataModelFolderHandlingTests.cs`
  - Acceptance: `FindMatches("")` records `searchString == ""`; test passes.
- [ ] [P8-T24] Add test `FindMatches_ReturnsHandlerResultUnmodified` (E1, positive) to `QuickFiler.Test/Controllers/EfcDataModelFolderHandlingTests.cs`
  - Acceptance: the array returned by the fake is returned by the method as the same reference with equal contents; test passes.
- [ ] [P8-T25] Add test `RefreshSuggestions_InvokesRefreshActionWithCurrentMail` (E5, positive) to `QuickFiler.Test/Controllers/EfcDataModelFolderHandlingTests.cs`
  - Acceptance: `RefreshSuggestionsAction` is invoked exactly once with the same `MailItem` instance as `Mail`; test passes.
- [ ] [P8-T26] Create `QuickFiler.Test/Controllers/EfcDataModelSelectionTests.cs` with the `[TestClass]` shell
  - Acceptance: uses the [P8-T1] support types; the Outlook selection chain is mocked as `IOlObjects.App` to `Application.ActiveExplorer()` to `Explorer.Selection`, with the indexer set up as `x => x[1]` on a boxed `int` per the `Mock<Columns>` precedent; compiles.
- [ ] [P8-T27] Add `<Compile Include="Controllers\EfcDataModelSelectionTests.cs" />` to `QuickFiler.Test/QuickFiler.Test.csproj`
  - Acceptance: entry present; build shows the file compiled.
- [ ] [P8-T28] Add test `MailInfo_WhenConversationResolverIsNull_ReturnsNull` (boundary) to `QuickFiler.Test/Controllers/EfcDataModelSelectionTests.cs`
  - Acceptance: with `_conversationResolver` null, `MailInfo` is null, covering the null arm of the null-propagating projection; test passes.
- [ ] [P8-T29] Add test `MailInfo_WhenConversationResolverPresent_ReturnsItsMailHelper` (positive) to `QuickFiler.Test/Controllers/EfcDataModelSelectionTests.cs`
  - Acceptance: with a resolver carrying a stub `MailHelper`, `MailInfo` is that same instance, covering the non-null arm; test passes.
- [ ] [P8-T30] Add test `Mail_WhenBackingFieldNull_ReturnsFirstSelectedMailItem` (positive) to `QuickFiler.Test/Controllers/EfcDataModelSelectionTests.cs`
  - Acceptance: with `Selection.Count == 1` and `Selection[1]` returning a loose `Mock<MailItem>`, `Mail` is that item; if Moq cannot intercept the `Selection` indexer, the implementer substitutes a hand-written `Selection` stub and records that in the test's doc comment; test passes.
- [ ] [P8-T31] Add test `Mail_WhenSelectionIsEmpty_ReturnsNull` (boundary) to `QuickFiler.Test/Controllers/EfcDataModelSelectionTests.cs`
  - Acceptance: with `Selection.Count == 0`, `Mail` is null, covering the empty-selection arm; test passes.
- [ ] [P8-T32] Add test `Mail_WhenFirstSelectedItemIsNotAMailItem_ReturnsNull` (invalid-input) to `QuickFiler.Test/Controllers/EfcDataModelSelectionTests.cs`
  - Acceptance: with `Selection[1]` returning a non-`MailItem` object, the `as` cast yields null and `Mail` is null; test passes.
- [ ] [P8-T33] Add test `Mail_WhenExplorerAccessThrows_ReturnsNullWithoutPropagating` (error-handling) to `QuickFiler.Test/Controllers/EfcDataModelSelectionTests.cs`
  - Acceptance: with `IOlObjects.App` throwing, `Mail` is null and no exception escapes; per D-10(b) this task is authored despite carrying no line-coverage delta, for AC5 error-handling completeness; test passes.
- [ ] [P8-T34] Add test `PackageItems_WithMoveConversationTrue_ReturnsSameFolderConversationItems` (positive) to `QuickFiler.Test/Controllers/EfcDataModelSelectionTests.cs`
  - Acceptance: with a resolver whose `ConversationItems` carries distinct same-folder and expanded lists, the result equals the same-folder list; the member is covered rather than deleted, the conservative choice under AC7, with deletion recorded as an option in `spec.md` §12 D4; test passes.
- [ ] [P8-T35] Add test `PackageItems_WithMoveConversationFalse_ReturnsSingletonOfCurrentMail` (positive) to `QuickFiler.Test/Controllers/EfcDataModelSelectionTests.cs`
  - Acceptance: the result is a single-element list whose element is the same instance as `Mail`; test passes.

- [ ] [P8-T36] Create `QuickFiler.Test/Controllers/EfcDataModelMoveTests.cs` with the `[TestClass]` shell and an `EmailFilerConfig` capture helper
  - Acceptance: the helper assigns `SortAsyncAction`, `OpenOlFolderAction`, `OpenFsFolderAction` and `MoveFailureMessageAction` to recording delegates so no `EmailFiler`, no `Process.Start` and no `MessageBox` is reachable; compiles.
- [ ] [P8-T37] Add `<Compile Include="Controllers\EfcDataModelMoveTests.cs" />` to `QuickFiler.Test/QuickFiler.Test.csproj`
  - Acceptance: entry present; build shows the file compiled.
- [ ] [P8-T38] Add test `MoveToFolderAsync_WhenMailInfoIsNull_ReturnsFalseWithoutInvokingSorter` (E3, invalid-input) to `QuickFiler.Test/Controllers/EfcDataModelMoveTests.cs`
  - Acceptance: with `_conversationResolver` null and `SortAsyncAction` set to fail the test if invoked, the result is `false` and the action was never invoked; test passes.
- [ ] [P8-T39] Add test `MoveToFolderAsync_WhenOneDriveSpecialFolderMissing_ReturnsFalseWithoutInvokingSorter` (E3, error-handling) to `QuickFiler.Test/Controllers/EfcDataModelMoveTests.cs`
  - Acceptance: with an empty `SpecialFolders` dictionary the result is `false` and `SortAsyncAction` was never invoked, covering the guard branch; test passes.
- [ ] [P8-T40] Add test `MoveToFolderAsync_ForTrashToDeleteFolder_SuppressesAttachmentSaving` (E3/E6, boundary) to `QuickFiler.Test/Controllers/EfcDataModelMoveTests.cs`
  - Acceptance: with `folderpath = "Trash to Delete"` and `saveAttachments: true`, the captured `EmailFilerConfig.SaveAttachments` is `false`; test passes.
- [ ] [P8-T41] Add test `MoveToFolderAsync_ForOrdinaryFolder_HonoursRequestedAttachmentFlag` (E3/E6, positive) to `QuickFiler.Test/Controllers/EfcDataModelMoveTests.cs`
  - Acceptance: with an ordinary folder path and `saveAttachments: true`, the captured config's flag is `true`; test passes.
- [ ] [P8-T42] Add test `MoveToFolderAsync_WithMoveConversationTrue_PassesSameFolderConversationHelpers` (E3, state-transition) to `QuickFiler.Test/Controllers/EfcDataModelMoveTests.cs`
  - Acceptance: with a resolver whose `ConversationInfo.SameFolder` holds two helpers, the helper list handed to `SortAsyncAction` equals those two in order; test passes.
- [ ] [P8-T43] Add test `MoveToFolderAsync_WithMoveConversationFalse_PassesOnlyCurrentMailInfo` (E3, positive) to `QuickFiler.Test/Controllers/EfcDataModelMoveTests.cs`
  - Acceptance: the helper list is a single element, the resolver's `MailHelper`; test passes.
- [ ] [P8-T44] Add test `MoveToFolderAsync_BuildsConfigFromRequestAndGlobals` (E3, positive) to `QuickFiler.Test/Controllers/EfcDataModelMoveTests.cs`
  - Acceptance: the captured `EmailFilerConfig` has `SaveMsg`, `SavePictures`, `DestinationOlStem` equal to the requested folder path, `Globals` the same instance, `OlAncestor` equal to `Ol.ArchiveRootPath`, and `FsAncestorEquivalent` equal to `SpecialFolders["OneDrive"]`; test passes.
- [ ] [P8-T45] Add test `MoveToFolderAsync_WhenSorterSucceeds_ReturnsTrue` (E3, positive) to `QuickFiler.Test/Controllers/EfcDataModelMoveTests.cs`
  - Acceptance: with `SortAsyncAction` returning a completed `true`, the method returns `true`; test passes.
- [ ] [P8-T46] Add test `MoveToFolderAsync_WhenSorterReportsFailure_ReturnsFalse` (E3, error-handling) to `QuickFiler.Test/Controllers/EfcDataModelMoveTests.cs`
  - Acceptance: with `SortAsyncAction` returning a completed `false`, the method returns `false`; test passes.
- [ ] [P8-T47] Add test `MoveToFolderAsync_FolderOverload_StripsAncestorAndLeadingSeparatorBeforeDelegating` (E6, positive) to `QuickFiler.Test/Controllers/EfcDataModelMoveTests.cs`
  - Acceptance: with a `Mock<MAPIFolder>` whose `FolderPath` is an archive-rooted path and a matching `olAncestor`, the captured `config.DestinationOlStem` is the bare remainder with no leading separator; test passes.
- [ ] [P8-T48] Add test `MoveToFolderAsync_FolderOverload_WhenInnerMoveFails_ShowsFailureMessageWithStrippedPath` (E4, error-handling) to `QuickFiler.Test/Controllers/EfcDataModelMoveTests.cs`
  - Acceptance: with the inner move forced false through an empty `SpecialFolders`, the recorded `MoveFailureMessageAction` text is `"Cannot move to folderpath <stripped path>"` and no modal dialog appears; test passes.
- [ ] [P8-T49] Add test `MoveToFolderAsync_FolderOverload_WhenInnerMoveSucceeds_DoesNotShowFailureMessage` (E4, positive) to `QuickFiler.Test/Controllers/EfcDataModelMoveTests.cs`
  - Acceptance: `MoveFailureMessageAction` was never invoked; test passes.
- [ ] [P8-T50] Add test `OpenOlFolderAsync_WhenOneDriveMissing_DoesNotInvokeOpenAction` (E3, error-handling) to `QuickFiler.Test/Controllers/EfcDataModelMoveTests.cs`
  - Acceptance: with an empty `SpecialFolders` and `OpenOlFolderAction` set to fail the test if invoked, the call completes and the action was never invoked, closing the currently-missing arm of that guard; test passes.
- [ ] [P8-T51] Add test `OpenOlFolderAsync_WithOneDrivePresent_InvokesOpenActionWithResolvedConfig` (E3, positive) to `QuickFiler.Test/Controllers/EfcDataModelMoveTests.cs`
  - Acceptance: the action is invoked once and the captured config carries the expected `DestinationOlStem`, `OlAncestor`, `FsAncestorEquivalent` and `Globals`; test passes.
- [ ] [P8-T52] Add test `OpenFsFolderAsync_WhenOneDriveMissing_DoesNotInvokeOpenAction` (E3, error-handling) to `QuickFiler.Test/Controllers/EfcDataModelMoveTests.cs`
  - Acceptance: as [P8-T50] for the file-system path; no external process is started; test passes.
- [ ] [P8-T53] Add test `OpenFsFolderAsync_WithOneDrivePresent_InvokesOpenActionWithResolvedConfig` (E3, positive) to `QuickFiler.Test/Controllers/EfcDataModelMoveTests.cs`
  - Acceptance: as [P8-T51] for the file-system path; test passes.
- [ ] [P8-T54] Add test `CreateAsync_WithNullGlobals_ThrowsArgumentNullException` (guard, invalid-input) to the existing `QuickFiler.Test/Controllers/EfcDataModelTests.cs`
  - Acceptance: the test is appended to the existing file so it can reuse the existing `CreateGlobals`/`CreateMailItem` scaffolding; `CreateAsync(null, ...)` throws `ArgumentNullException`; per D-10(b) it is authored despite carrying no line-coverage delta; no `<Compile Include>` change is required because the file is already registered; test passes.
- [ ] [P8-T55] Add test `CreateAsync_WithEmptyMailItemList_Throws` (guard, invalid-input) to the existing `QuickFiler.Test/Controllers/EfcDataModelTests.cs`
  - Acceptance: `CreateAsync(globals, new List<MailItem>(), ...)` throws; the test asserts the observed exception type explicitly; test passes.
- [ ] [P8-T56] Measure the Phase 8 test file sizes and record them to `<FEATURE>/evidence/other/file-line-counts-phase8.<ts>.md`
  - Acceptance: the artifact records a measured line count for `EfcDataModel.TestSupport.cs`, `EfcDataModelPureLogicTests.cs`, `EfcDataModelFolderHandlingTests.cs`, `EfcDataModelSelectionTests.cs`, `EfcDataModelMoveTests.cs` (sixteen cases, the most likely candidate) and the existing `EfcDataModelTests.cs` after the two appended cases (it began at 409 with roughly 90 lines of headroom), and closes with a verdict line reading either `SPLIT REQUIRED: <file>` (one line per file measuring 500 lines or more) or `SPLIT NOT REQUIRED`. This task changes no `.cs` file and no `.csproj` file.
- [ ] [P8-T57] Split every file the [P8-T56] artifact marked `SPLIT REQUIRED` into a `.Part2.cs` companion under `QuickFiler.Test/Controllers/`
  - Acceptance: if [P8-T56] recorded `SPLIT NOT REQUIRED`, this task records `NO ACTION` in the [P8-T56] artifact and makes no change — this branch is explicitly authorized. Otherwise each named file is split into a `<Name>.Part2.cs` companion declaring the same namespace and a `[TestClass] partial` twin, both halves measure below 500 lines, and no test method is lost.
- [ ] [P8-T58] Add a `<Compile Include>` entry to `QuickFiler.Test/QuickFiler.Test.csproj` for every `.Part2.cs` companion created at [P8-T57]
  - Acceptance: if [P8-T57] recorded `NO ACTION`, this task records `NO ACTION` in the [P8-T56] artifact and makes no change — this branch is explicitly authorized. Otherwise each companion carries an entry beside its parent `Controllers\` item and a build shows every companion compiled.
- [ ] [P8-T59] Run CMD-TEST-SCOPED with filter `FullyQualifiedName~EfcDataModelPureLogicTests|FullyQualifiedName~EfcDataModelFolderHandlingTests|FullyQualifiedName~EfcDataModelSelectionTests|FullyQualifiedName~EfcDataModelMoveTests|FullyQualifiedName~EfcDataModelTests` and record the result to `<FEATURE>/evidence/regression-testing/phase8-tests.<ts>.md`
  - Acceptance: artifact carries the four required fields with `EXIT_CODE: 0`; `Output Summary:` records the 45 new cases passing alongside the pre-existing `EfcDataModelTests` cases, with 0 failed, and confirms no external process was started and no dialog appeared.

### Phase 9 — IQfcDatamodel.cs and SortOptionsEnum Characterization Tests

`QuickFiler/Interfaces/IQfcDatamodel.cs` receives **zero production edits** (D-07, AC7). These three cases
earn **zero line-coverage credit** for that file, because the instrumenter emits no `<class>` element for a
declaration-only file; they are justified by `CLAUDE.md` § UT2 ("untested critical behavior is not acceptable
even if the overall percentage looks good"). They deliberately do not construct `EmailSorter`, so F5 executes
no IL in F2's `EmailSorter.cs` and claims no coverage credit there.

- [ ] [P9-T1] Create `QuickFiler.Test/Interfaces/SortOptionsEnumTests.cs` with the `[TestClass]` shell in namespace `QuickFiler.Interfaces.Tests`
  - Acceptance: the file lives under `QuickFiler.Test/Interfaces/` per D-08, mirrors the production tree, needs no Moq, no clock and no COM object, and measures well below 500 lines; compiles.
- [ ] [P9-T2] Add `<Compile Include="Interfaces\SortOptionsEnumTests.cs" />` to `QuickFiler.Test/QuickFiler.Test.csproj`
  - Acceptance: the entry is present even though no `Interfaces\` item exists today; no folder registration is needed in a non-SDK project; a build shows the file compiled.
- [ ] [P9-T3] Add test `Default_DecomposesToTriageImportantFirstDateRecentFirstAndConversationUniqueOnly` (characterization/boundary) to `QuickFiler.Test/Interfaces/SortOptionsEnumTests.cs`
  - Acceptance: `((int)SortOptionsEnum.Default).Should().Be(42, ...)` and `SortOptionsEnum.Default.Should().Be(SortOptionsEnum.TriageImportantFirst | SortOptionsEnum.DateRecentFirst | SortOptionsEnum.ConversationUniqueOnly)` both hold; the test fails loudly if any contributing flag value changes even while `Default` stays at 42; test passes.
- [ ] [P9-T4] Add test `Default_SatisfiesBothFlagsRequiredForTriageDateSortKeyGeneration` (cross-child characterization) to `QuickFiler.Test/Interfaces/SortOptionsEnumTests.cs`
  - Acceptance: `Default.HasFlag(TriageImportantFirst)` and `Default.HasFlag(DateRecentFirst)` are both true, each with a failure message naming the F2 consumer predicate at `EmailSorter.cs:45-48` and the frame-ordering consequence at `QfcDatamodel.FrameBuilding.cs:114`; `EmailSorter` is not constructed; test passes.
- [ ] [P9-T5] Add test `FlagMembers_AreDistinctSingleBitValues` (invariant) to `QuickFiler.Test/Interfaces/SortOptionsEnumTests.cs`
  - Acceptance: for each of `TriageIgnore`, `TriageImportantFirst`, `TriageImportantLast`, `DateRecentFirst`, `DateOldestFirst`, `ConversationUniqueOnly`, the integer value is greater than zero and has a single bit set, and the six values are pairwise distinct; this converts the "member appended without an explicit initializer inherits 33" trap into a visible failure; test passes.
- [ ] [P9-T6] Verify `QuickFiler/Interfaces/IQfcDatamodel.cs` is byte-identical to the [P0-T13] baseline and record it to `<FEATURE>/evidence/other/iqfcdatamodel-unmodified.<ts>.md`
  - Acceptance: `git diff` against the baseline SHA shows no change to the file; the artifact confirms all nine interface members keep byte-identical signatures, `SortOptionsEnum` is unchanged including `Default = 42`, and no `[ExcludeFromCodeCoverage]` was added at type or member level (spec R1–R5).
- [ ] [P9-T7] Run CMD-TEST-SCOPED with filter `FullyQualifiedName~SortOptionsEnumTests` and record the result to `<FEATURE>/evidence/regression-testing/phase9-tests.<ts>.md`
  - Acceptance: artifact carries the four required fields with `EXIT_CODE: 0`; `Output Summary:` records 3 passed and 0 failed and repeats that these tests earn no line-coverage credit for `IQfcDatamodel.cs`.

### Phase 10 — Exemption Removal and Per-File Coverage Verification

[P10-T1] is the **last production task of the feature** (D-03). It must follow Phases 1, 3, 4, 5, 6 and 8,
because the attribute is type-scoped and admits `QfcDatamodel.cs`, `QfcDatamodel.QueueProcessing.cs` and
`QfcDatamodel.FrameBuilding.cs` into the coverage denominator in one commit.

- [ ] [P10-T1] Remove the `[ExcludeFromCodeCoverage]` attribute at `QuickFiler/Controllers/QfcDatamodel.cs:25`
  - Acceptance: the attribute and any now-unused `using System.Diagnostics.CodeAnalysis;` are removed; the `public partial class QfcDatamodel : IQfcDatamodel` declaration is otherwise unchanged; no member-level `[ExcludeFromCodeCoverage]` is introduced as a substitute.
- [ ] [P10-T2] Verify that no `[ExcludeFromCodeCoverage]` exists anywhere in the in-scope file set and record it to `<FEATURE>/evidence/other/exemption-audit.<ts>.md`
  - Acceptance: a search across `QuickFiler/Controllers/QfcDatamodel.cs`, `QfcDatamodel.Construction.cs`, `QfcDatamodel.QueueProcessing.cs`, `QfcDatamodel.FrameBuilding.cs`, `QfcEmailFrameShaper.cs`, `EfcDataModel.cs`, any `EfcDataModel.Seams.cs`, and `QuickFiler/Interfaces/IQfcDatamodel.cs` returns zero matches at type or member level; the artifact also confirms `coverage.config` is unmodified.
- [ ] [P10-T3] Run CMD-ANALYZER and record the post-removal build gate to `<FEATURE>/evidence/qa-gates/phase10-analyzer-build.<ts>.md`
  - Acceptance: artifact carries the four required fields with `EXIT_CODE: 0`; zero errors and no new warnings relative to [P0-T8].
- [ ] [P10-T4] Run CMD-COVERAGE-FULL with `-CoverageOutput <FEATURE>/evidence/qa-gates/coverage-post-change.cobertura.xml` and record the suite result to `<FEATURE>/evidence/qa-gates/coverage-suite-post-change.<ts>.md`
  - Acceptance: artifact carries the four required fields with `EXIT_CODE: 0`; `Output Summary:` records numeric passed/failed counts and the numeric repository-wide `line-rate` and `branch-rate` from the emitted Cobertura root element.
- [ ] [P10-T5] Run CMD-PERFILE against the post-change Cobertura output and record numeric per-file coverage to `<FEATURE>/evidence/qa-gates/coverage-per-file-post-change.<ts>.md`
  - Acceptance: the artifact records a **numeric** line-coverage percentage for `QuickFiler/Controllers/QfcDatamodel.cs`, `QfcDatamodel.Construction.cs`, `QfcDatamodel.QueueProcessing.cs`, `QfcDatamodel.FrameBuilding.cs`, `EfcDataModel.cs`, and `QfcEmailFrameShaper.cs` (and `EfcDataModel.Seams.cs` if [P7-T18] created it); `IQfcDatamodel.cs` is recorded with its ledger classification rather than a percentage. No projected or read-derived figure appears.
- [ ] [P10-T6] Verify the AC1 per-file floor against the [P10-T5] numbers and record the verdict to `<FEATURE>/evidence/qa-gates/ac1-floor-verification.<ts>.md`
  - Acceptance: every file F1's ledger classifies `testable` measures **>= 80% line coverage**; the two new production files additionally measure **>= 90%** per D-11; the artifact states each file's measured value against its threshold and returns PASS or REMEDIATION-REQUIRED. A shortfall is remediated by adding tests, never by adding an exemption.
- [ ] [P10-T7] Verify the `IQfcDatamodel.cs` classification and record the outcome to `<FEATURE>/evidence/qa-gates/iqfcdatamodel-classification.<ts>.md`
  - Acceptance: if F1's ledger classifies the file `not-measurable (declaration-only)`, the artifact records that it is outside the numeric gate and the reason (no `<class>` element is emitted for any interface or enum across the instrumented assembly). If the ledger classifies it `testable`, the artifact records `ESCALATION REQUIRED` with the measured evidence from `spec.md` §5.2 and the plan halts rather than attempting to comply.
- [ ] [P10-T8] Produce the coverage delta and threshold report at `<FEATURE>/evidence/qa-gates/coverage-delta.<ts>.md`
  - Acceptance: the artifact tabulates, per in-scope file, the **baseline** value from [P0-T11] (or `ABSENT` where the type-scoped attribute suppressed it), the **post-change** value from [P10-T5], and the **new/changed-code** value for the lines this feature added or modified; it also records the repository-wide baseline and post-change `line-rate`/`branch-rate` from [P0-T10] and [P10-T4] as **reported, non-blocking** figures per D-11, and states that no changed line's coverage regressed.
- [ ] [P10-T9] Take the **pre-format** AC3 measurement of every in-scope file and record it to `<FEATURE>/evidence/qa-gates/file-size-compliance.<ts>.md`
  - Acceptance: measured counts for `QfcDatamodel.cs`, `QfcDatamodel.Construction.cs`, `QfcDatamodel.QueueProcessing.cs`, `QfcDatamodel.FrameBuilding.cs`, `QfcEmailFrameShaper.cs`, `EfcDataModel.cs`, any `EfcDataModel.Seams.cs`, and `IQfcDatamodel.cs` are each **<= 500**; every new and modified test file is also listed and is **<= 500**. The artifact states in its header that this is the **pre-format AC3 measurement; the Phase 12 post-format measurement at [P12-T3] is authoritative**, because `csharpier format .` can rewrite files and change line counts after this point.

### Phase 11 — Defect Promotion and Acceptance-Criteria Check-off

AC7 forbids fixing any item in the `spec.md` §12 register. Each is promoted to its own GitHub issue through
the MCP promotion lifecycle, because prose in a feature folder disappears at merge. Each promotion task
mirrors its issue text to `<FEATURE>/evidence/issue-updates/issue-<N>.<ts>.md` with `PostedAs:` recorded.

This phase closes **five** of the eight acceptance criteria. AC1, AC4 and AC6 depend on Phase 12 evidence and
are deliberately left unchecked here; they are closed at [P12-T12] and [P12-T13].

- [ ] [P11-T1] Promote defect **D1** (`QfcDatamodel.Cleanup()` is not idempotent; a second call NREs after `_moveMonitor` is nulled) to a GitHub issue through the MCP promotion lifecycle
  - Acceptance: an issue exists citing `QfcDatamodel.cs:79-84`; no guard was added and no test pins the NRE; the issue number and URL are recorded in the mirror artifact.
- [ ] [P11-T2] Promote defect **D2** (null-vs-empty return asymmetry at `quantity <= 0`, which `QfcHomeController.Iteration.cs:25` dereferences without a null guard) to a GitHub issue
  - Acceptance: the issue cites `QueueProcessing.cs:147-150`, the gate's empty-list return, and `Iteration.cs:21-25`, and notes that the fix would sit in F7's file; [P3-T9], [P3-T10], [P3-T11] and [P3-T15] pin the current shape.
- [ ] [P11-T3] Promote defect **D3** (items can leave the master queue still hooked to the move monitor) to a GitHub issue
  - Acceptance: the issue cites the gate's discard of below-threshold candidates and `QueueProcessing.cs:31,52,154`, and records that the stale callback runs against a queue the item has already left; [P3-T24] and [P3-T30] pin current behavior.
- [ ] [P11-T4] Promote defect **D4** (`EfcDataModel.PackageItems(bool)` has no caller repo-wide) to a GitHub issue
  - Acceptance: the issue cites `EfcDataModel.cs:362-372`, records that F5 covered rather than deleted the member as the conservative choice under AC7, and records deletion as an explicit option that would remove seven lines from the denominator and two test cases.
- [ ] [P11-T5] Promote defect **D5** (unreachable nested condition re-testing `!offline` inside an already-guarded block) to a GitHub issue
  - Acceptance: the issue cites `FrameBuilding.cs:36,39` and records that this is one permanently-uncoverable branch arm with no line-coverage impact.
- [ ] [P11-T6] Promote defect **D6** (`throw e;` resets the stack trace, at both sites) to a GitHub issue
  - Acceptance: one issue covers `FrameBuilding.cs:108` and the same pattern in `QfcDatamodel.cs`; the issue records that [P2-T37] and [P6-T39] assert only exception type and message, so both pass before and after a future fix.
- [ ] [P11-T7] Promote defect **D7** (XML doc contradicts `ToggleOfflineMode` behavior) to a GitHub issue
  - Acceptance: the issue cites `FrameBuilding.cs:29-33` against `:34-46` and is classified as a documentation defect.
- [ ] [P11-T8] Promote defect **D8** (asymmetric error handling around the offline probe) to a GitHub issue
  - Acceptance: the issue cites `FrameBuilding.cs:77,80,102-108` and records that [P6-T40] pins current behavior so a future fix is deliberate and visible.
- [ ] [P11-T9] Promote defect **D9** (restore failure masks the original exception) to a GitHub issue
  - Acceptance: the issue cites `FrameBuilding.cs:96-108`; it may be filed as a linked low-priority companion to D6 if the maintainer prefers one exception-handling issue, and the mirror artifact records which shape was used.
- [ ] [P11-T10] Promote observation **O1** (two dead `IQfcDatamodel` members — `UndoMove()` throws unconditionally, `MovedItems` has no production consumer) to a GitHub issue
  - Acceptance: the issue cites `IQfcDatamodel.cs:47-48`, `QueueProcessing.cs:23-27` and `QfcDatamodel.cs:141-144`, and records that removing a member from a public interface re-exposed through `IQfcHomeController` is a breaking change (spec R1); [P3-T3] pins the throw.
- [ ] [P11-T11] Promote observation **O2** (four dead `SortOptionsEnum` flags, with `ConversationUniqueOnly` misleadingly a component of `Default = 42`) to a GitHub issue
  - Acceptance: the issue cites `IQfcDatamodel.cs:16,18,20,21`, records that spec R3 forbids changing the enum in this child, and states that the choice between deleting the flags and wiring `ConversationUniqueOnly` to the existing filter spans F5 and F2.
- [ ] [P11-T12] Promote observation **O3** (misleading identifier in the F2-owned `EmailSorter.GetSortKey`) to a GitHub issue
  - Acceptance: the issue cites `EmailSorter.cs:21-35,46,53`, records that behavior appears correct but the name inverts the apparent intent, and is explicitly assigned against F2's file, which this child does not modify.
- [ ] [P11-T13] Record every resulting issue number in the `spec.md` §12 register at `<FEATURE>/spec.md`
  - Acceptance: each of the twelve rows in §12 carries its GitHub issue number and URL; this is the AC8 evidence and it is written into `spec.md`, not only into an evidence artifact.
- [ ] [P11-T14] Post the cross-child observations to epic issue #136 and mirror the text to `<FEATURE>/evidence/issue-updates/issue-136.<ts>.md`
  - Acceptance: the comment records the five mislabelled `TryQueueRemainingMailItemAsync_*` tests in `QfcDatamodelTests.cs` (F2's subject, left in place), the incidental `EmailSorter` coverage from four sort tests, the F2 gate coupling of the high-confidence dequeue tests, the misplaced `ConversationResolver` test in `EfcDataModelTests.cs:83` (F4's subject, left in place), and the vestigial `ref` on `TryUnhookOrReplace`; the mirror records `PostedAs:` and the comment URL. No sibling-owned file is edited.
- [ ] [P11-T15] Record the compiled-file denominator delta for the F16 capstone in `<FEATURE>/evidence/other/denominator-delta.<ts>.md`
  - Acceptance: the artifact lists every production file this feature added — `QfcDatamodel.Construction.cs`, `QfcEmailFrameShaper.cs` if Phase 5 was taken, `EfcDataModel.Seams.cs` if [P7-T18] created it — and states the resulting change to the epic's 121-file denominator.
- [ ] [P11-T16] Record the AC4 determinism audit to `<FEATURE>/evidence/qa-gates/determinism-audit.<ts>.md`
  - Acceptance: a search across every new and modified test file returns zero matches for `Thread.Sleep`, `Task.Delay`, `DateTime.Now`, `DateTime.UtcNow`, `Stopwatch`, `Path.GetTempFileName`, `Process.Start` and `MessageBox.Show`; the artifact confirms zero `*.StaTests.cs` files exist in `QuickFiler.Test`, and lists every test class that assigns `TimeProvider` through a `CreateModelWithFakeClock` helper. This task precedes the check-off tasks so that AC4's evidence exists before any task consumes it.
- [ ] [P11-T17] Check off **AC2, AC3, AC5, AC7 and AC8 only** in `<FEATURE>/spec.md` § 16
  - Acceptance: each of those five criteria is checked only when its named evidence artifact exists and shows PASS — AC2 from [P10-T1] and [P10-T2], AC3 from [P10-T9] (pre-format, superseded at [P12-T3]), AC5 from the scenario-completeness tables, AC7 from [P7-T22] and [P9-T6], AC8 from [P11-T13]. **AC1, AC4 and AC6 remain unchecked at the end of Phase 11 by design**, because their authoritative evidence is produced in Phase 12; they are closed at [P12-T12] and [P12-T13]. Any criterion whose evidence is absent stays unchecked.
- [ ] [P11-T18] Check off the same five criteria — **AC2, AC3, AC5, AC7 and AC8 only** — in `<FEATURE>/user-story.md` § Acceptance Criteria
  - Acceptance: the checkbox state is identical to `spec.md` § 16 after [P11-T17], including AC1, AC4 and AC6 left unchecked; the two documents do not diverge.

### Phase 12 — Final QC Toolchain Loop

This phase contains **five command steps** — CMD-FORMAT ([P12-T1]), CMD-FORMAT-CHECK ([P12-T2]),
CMD-ANALYZER ([P12-T6]), CMD-NULLABLE ([P12-T7]) and CMD-COVERAGE-FULL ([P12-T8]) — **plus the three-task
AC3 size gate at [P12-T3] through [P12-T5]**, which sits between the two formatter steps and the build steps.
The size gate is **not** a command step and does not extend AC6's four-stage mapping in D-18: AC6 is still
satisfied by exactly the five commands named above. The five command steps run in that exact order with no
intervening file change, mapping onto AC6's four stages per D-18. If any stage fails **or changes any file**
— including a split at [P12-T4] or a `.csproj` edit at [P12-T5] — the loop restarts from [P12-T1]. No task in
this phase may record `EXIT_CODE: SKIPPED`. This phase also closes AC1, AC4 and AC6, which Phase 11
deliberately left unchecked, and its final task is the commit gate.

- [ ] [P12-T1] Run CMD-FORMAT and record the result to `<FEATURE>/evidence/qa-gates/final-csharpier-format.<ts>.md`
  - Acceptance: artifact carries the four required fields; `Output Summary:` states how many files the formatter rewrote. If any file was rewritten, the loop restarts at [P12-T1] after the rewrite is committed.
- [ ] [P12-T2] Run CMD-FORMAT-CHECK and record the result to `<FEATURE>/evidence/qa-gates/final-csharpier-check.<ts>.md`
  - Acceptance: artifact carries the four required fields with `EXIT_CODE: 0` and zero files reported unformatted; `check` is the enforcing gate, `pipe-files` is not acceptable as a substitute.
- [ ] [P12-T3] Measure post-format file sizes and record them to `<FEATURE>/evidence/qa-gates/file-size-compliance-post-format.<ts>.md`
  - Acceptance: the artifact records the **post-format measured line count as the file stands after [P12-T1]** for every in-scope production file (`QfcDatamodel.cs`, `QfcDatamodel.Construction.cs`, `QfcDatamodel.QueueProcessing.cs`, `QfcDatamodel.FrameBuilding.cs`, `QfcEmailFrameShaper.cs`, `EfcDataModel.cs`, any `EfcDataModel.Seams.cs`, `IQfcDatamodel.cs`) and for every new or modified test file, **including every `.Part2.cs` companion** created at [P2-T52], [P3-T49], [P6-T42] or [P8-T57]. The artifact closes with a verdict line reading either `SPLIT REQUIRED: <file>` — one such line per file measuring **500 lines or more** — or the single line `SPLIT NOT REQUIRED`. The artifact states in its header that it **supersedes [P10-T9] as the authoritative AC3 evidence**, because `csharpier format .` can rewrite files and change line counts after the Phase 10 measurement. This task is a **pure measurement**: it changes no `.cs` file and no `.csproj` file.
- [ ] [P12-T4] Split every file the [P12-T3] artifact marked `SPLIT REQUIRED` into a companion partial file — test companions as `<Name>.Part2.cs` under `QuickFiler.Test/Controllers/`, production companions as `<Name>.PartN.cs` under `QuickFiler/Controllers/`
  - Acceptance: if [P12-T3] recorded `SPLIT NOT REQUIRED`, this task records `NO ACTION` in the [P12-T3] artifact and makes no change — this branch is explicitly authorized. Otherwise each named file is split into a companion declaring the same namespace and a `partial` twin of the same type (a `[TestClass] partial` twin for test companions), both halves measure **below 500 lines**, and no test method or type member is lost.
- [ ] [P12-T5] Add a `<Compile Include>` entry for every companion created at [P12-T4] to its owning project — `QuickFiler.Test/QuickFiler.Test.csproj` for test companions, `QuickFiler/QuickFiler.csproj` for production companions
  - Acceptance: if [P12-T4] recorded `NO ACTION`, this task records `NO ACTION` in the [P12-T3] artifact and makes no change — this branch is explicitly authorized. Otherwise each companion carries an entry beside its parent `Controllers\` item in the correct project file and a build shows every companion compiled (a missing entry fails silently in these legacy non-SDK projects, per D-14); the loop then restarts at [P12-T1].
- [ ] [P12-T6] Run CMD-ANALYZER and record the result to `<FEATURE>/evidence/qa-gates/final-msbuild-analyzers.<ts>.md`
  - Acceptance: artifact carries the four required fields with `EXIT_CODE: 0`; zero errors and no new warnings relative to the [P0-T8] baseline.
- [ ] [P12-T7] Run CMD-NULLABLE and record the result to `<FEATURE>/evidence/qa-gates/final-msbuild-nullable.<ts>.md`
  - Acceptance: artifact carries the four required fields with `EXIT_CODE: 0`; zero errors and no new nullable warnings relative to the [P0-T9] baseline.
- [ ] [P12-T8] Run CMD-COVERAGE-FULL with `-CoverageOutput <FEATURE>/evidence/qa-gates/coverage-final.cobertura.xml` and record the result to `<FEATURE>/evidence/qa-gates/final-coverage-suite.<ts>.md`
  - Acceptance: artifact carries the four required fields with `EXIT_CODE: 0`; `Output Summary:` records **numeric** passed and failed counts (failed must be zero) and the numeric repository-wide `line-rate` and `branch-rate`. `UNVERIFIED` and `SKIPPED` are invalid.
- [ ] [P12-T9] Run CMD-PERFILE against the final Cobertura output and record numeric per-file coverage to `<FEATURE>/evidence/qa-gates/final-coverage-per-file.<ts>.md`
  - Acceptance: numeric line coverage is recorded for every in-scope production file; the values are the ones AC1 is closed against, superseding [P10-T5] if the loop restarted.
- [ ] [P12-T10] Produce the final delta and threshold verification at `<FEATURE>/evidence/qa-gates/final-coverage-delta.<ts>.md`
  - Acceptance: the artifact reports, per in-scope file, the baseline value from [P0-T11], the post-change value from [P12-T9], and the new/changed-code value; it confirms every `testable` file is at or above 80%, both new production files are at or above 90%, and no changed line regressed; it repeats the repository-wide figures as reported, non-blocking per D-11. If any required value is unavailable, the outcome is REMEDIATION-REQUIRED, never PASS.
- [ ] [P12-T11] Record the clean-pass declaration to `<FEATURE>/evidence/qa-gates/final-toolchain-pass.<ts>.md`
  - Acceptance: the artifact names the five commands in order (CMD-FORMAT, CMD-FORMAT-CHECK, CMD-ANALYZER, CMD-NULLABLE, CMD-COVERAGE-FULL), maps them onto AC6's four stages per D-18, and records that all completed in a single pass with `EXIT_CODE: 0` and with no file changed between them — including that the [P12-T3] size gate produced no split — and states how many restarts the loop required; this is the AC6 evidence.
- [ ] [P12-T12] Check off **AC1, AC4 and AC6** in `<FEATURE>/spec.md` § 16
  - Acceptance: AC1 is checked against the [P12-T10] figures, which supersede [P10-T6] if the loop restarted; AC4 is checked against the [P11-T16] determinism audit together with the Phase 12 test artifacts [P12-T8] and [P12-T9]; AC6 is checked against the [P12-T11] clean-pass declaration. Each is checked only when its named artifact exists and shows PASS; any criterion whose evidence is absent stays unchecked. AC2, AC3, AC5, AC7 and AC8 retain the state set at [P11-T17], except that AC3's evidence reference is updated to [P12-T3]. After this task all eight criteria are resolved.
- [ ] [P12-T13] Check off **AC1, AC4 and AC6** in `<FEATURE>/user-story.md` § Acceptance Criteria
  - Acceptance: the checkbox state is identical to `spec.md` § 16 after [P12-T12]; the two documents do not diverge on any of the eight criteria.
- [ ] [P12-T14] Commit every evidence artifact and verify a clean worktree, recording the result to `<FEATURE>/evidence/other/final-tree-state.<ts>.md`
  - Acceptance: `git status --porcelain` produces no output; the artifact records the final HEAD SHA and confirms that every file under `<FEATURE>/evidence/` produced by Phases 0 through 12 is committed, together with the `spec.md` and `user-story.md` check-off edits from [P12-T12] and [P12-T13]; no evidence was written under `artifacts/`. This is the last task of the phase and of the plan.

## Test Plan

- **Unit:** 157 MSTest cases across 19 new test files plus one shared test-support file
  (`EfcDataModel.TestSupport.cs`, no `[TestMethod]`), plus two cases appended to the existing
  `QuickFiler.Test/Controllers/EfcDataModelTests.cs` — 40 for `QfcDatamodel.cs`, 39 for
  `QfcDatamodel.QueueProcessing.cs`, 30 for `QfcDatamodel.FrameBuilding.cs`, 45 for `EfcDataModel.cs`, and 3
  characterization cases for `SortOptionsEnum`. Moq for mocks and stubs, FluentAssertions for assertions,
  Arrange–Act–Assert throughout.
- **Integration:** none. Every dependency on Outlook COM, `DfDeedle`, `EmailFiler`, `FolderPredictor`, the
  file system and modal dialogs is removed from the test path by seams S1–S7 and E1–E7.
- **Determinism:** all timing is driven by `System.TimeProvider` with `FakeTimeProvider`; state transitions on
  a `BackgroundWorker` are observed with the bounded condition-driven `SpinWait.SpinUntil` helper. Audited by
  [P11-T16].
- **Manual/CLI:** none.
- **Coverage evidence:**
  - baseline suite — `<FEATURE>/evidence/baseline/coverage-suite.<ts>.md` and `coverage-baseline.cobertura.xml`
  - baseline per file — `<FEATURE>/evidence/baseline/coverage-per-file-baseline.<ts>.md`
  - post-change suite — `<FEATURE>/evidence/qa-gates/coverage-suite-post-change.<ts>.md`
  - post-change per file — `<FEATURE>/evidence/qa-gates/coverage-per-file-post-change.<ts>.md`
  - final suite — `<FEATURE>/evidence/qa-gates/final-coverage-suite.<ts>.md` and `coverage-final.cobertura.xml`
  - final per file — `<FEATURE>/evidence/qa-gates/final-coverage-per-file.<ts>.md`
  - comparison — `<FEATURE>/evidence/qa-gates/coverage-delta.<ts>.md` and `final-coverage-delta.<ts>.md`

## Open Questions / Notes

- **Q1 — `NewMailEx` proxying.** Whether Moq can proxy the `[ComEventInterface]` add/remove accessors of
  `Application.NewMailEx` could not be verified without building. [P1-T21] resolves it and binds [P2-T5] and
  [P2-T12]; seam S7 is adopted only if the direct approach fails.
- **Q2 — Deedle exception types.** The exact exception Deedle raises for a missing column key was not verified
  from source. [P6-T10] and [P6-T17] are written as characterization tests: the implementer records the
  observed type in the doc comment and asserts it explicitly. A bare `Should().Throw<Exception>()` is not
  acceptable because it would also pass on an NRE.
- **Q3 — `Mock<Selection>` indexer.** [P8-T30] assumes Moq can intercept `Selection`'s `this[object]`
  indexer, by analogy with the existing `Mock<Columns>` precedent. If it cannot, the fallback is a
  hand-written `Selection` stub; a `FirstSelectedItemProvider` seam is **not** pre-committed.
- **Q4 — F2 gate coupling, and the S1 declaration site.** [P3-T29] through [P3-T39] assert behavior jointly
  produced by `QfcDatamodel.QueueProcessing.cs` and the F2-owned `QfcStreamingDequeueConfidenceGate.cs`. A
  failure at the epic's integration rebase is a coordination signal, not a defect in F5.
  The coupling is tighter than test behavior alone. Seam **S1** reuses `IFolderScoringService`, which is
  declared at `QuickFiler/Controllers/QfcHighConfidencePreFilter.cs:130` — an **F2-owned file** this child must
  not modify (D-13) — and the `[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]` attribute that lets
  Moq proxy that `internal` interface sits at line 11 of the same F2-owned file. If sibling F2 relocates either
  the interface declaration or that attribute during its own coverage work, every S1-dependent F5 test
  ([P2-T41], [P2-T42], [P3-T12], and [P3-T29] through [P3-T39]) breaks at the integration rebase, and
  [P1-T13]/[P1-T14] would need to bind a moved type. This child does not pre-empt the move: it is raised to
  `epic-orchestrator` as a coordination item, and the integration rebase is the detection point.
- **Q5 — Percentage fragility.** If Phase 5 is taken, `QfcDatamodel.FrameBuilding.cs` shrinks to roughly 35
  instrumented lines, where four uncovered lines is 89% and eight is 77%. [P12-T9] measures rather than
  projects; if the margin is thin, Phase 5 is the clean severing point (D-09).
- **Q6 — `SortEmail.Cleanup_Files()`.** It remains in the covered path and mutates static
  `YesNoToAllResponse` fields. It resets rather than accumulates and is proven non-throwing, so test
  independence holds; it is recorded in the policy audit as a known static touch rather than left silent.
- **Q7 — Region structure.** After the Phase 1 split, the existing `#region` names are preserved in whichever
  file each member lands in, so the move diff stays reviewable.

