# com-vsto-coverage-exemption - Refactor Spec

- **Issue:** #197
- **Parent (optional):** none
- **Owner:** drmoisan
- **Last Updated:** 2026-06-13
- **Status:** Draft
- **Version:** 0.2

## Intent & Outcomes

Repository-wide C# coverage cannot meaningfully reach the 80% policy floor because a
large fraction of first-party production code is bound to the live Outlook COM object
model and the WinForms form lifecycle, and cannot be unit-tested without a running
Outlook process (prohibited by the unit-test policy). Re-measurement (post-#189, test
assemblies excluded per Issue #193) shows production-only deduped coverage of 58.95%
(38,767 covered / 65,768 lines-valid), with the gap concentrated in COM/VSTO/WinForms
code: TaskVisualization 0.37%, ToDoModel 10.43%, QuickFiler 25.15%, TaskMaster 25.68%,
Tags 31.15%. Without a formal exemption, the 80% target is unreachable regardless of
test effort, and the metric does not distinguish genuinely-testable code from
architecturally-untestable interop code.

The maintainer ratified pursuing this exemption on 2026-06-13. The design basis is
`artifacts/research/2026-06-12-com-vsto-coverage-exemption-design.md` (referenced below
as "the design memo"). Supporting coverage evidence:
`artifacts/research/csharp-coverage-roadmap.2026-06-12.md` (§0 corrected figures) and
`artifacts/csharp/coverage-firstparty.cobertura.xml`.

Intended outcome: the 80% coverage floor is redefined to apply to a **testable
denominator** that excludes architecturally-untestable COM/VSTO/WinForms code, while all
genuinely-testable seams remain measured. After this change (attributes and config only,
no new tests), the reported rate on the testable remainder is estimated at **~75.2%**
(range 73.2%–77.6%). Reaching 80% still requires the roadmap increment tests, which are
out of scope for this feature.

## Invariants (must not change)

- **No production behavior change.** This feature adds only `[ExcludeFromCodeCoverage]`
  attributes and `coverage.config` exclude entries. No method bodies, signatures, public
  APIs, or runtime behavior change.
- **Public API surface** of TaskVisualization, QuickFiler, TaskMaster, ToDoModel, and
  Tags is unchanged. `[ExcludeFromCodeCoverage]` is a non-behavioral diagnostic
  attribute; it does not alter type contracts or member visibility.
- **Testable seams remain in the denominator.** The classes/methods enumerated as NOT
  exempt in the design memo §2 must continue to be instrumented and reported.
- **Coverage pipeline structure unchanged** apart from the single `coverage.config`
  module-path addition. No changes to `Get-KoverageProjectAllowlist`,
  `ConvertTo-KoverageCoberturaXml`, or the Koverage allowlist.
- Performance characteristics to preserve: none affected (test-tooling-only change).
- Compatibility guarantees: `coverage.config` and `TaskMaster.runsettings` schemas are
  unchanged in shape; only additional `ModulePath` exclude entries are added.

## Scope (structural changes)

Formally exempt Outlook-COM / VSTO / WinForms-bound code from the 80% coverage floor so
the remaining genuinely-testable first-party code can be held to a meaningful target,
using a two-layer hybrid mechanism (design memo §1.2). This feature delivers the
exemption only — it does not add tests that raise covered code.

### In scope

1. **Class-level (and where noted, method-level) `[ExcludeFromCodeCoverage]` for
   `TaskVisualization`** (revision 1.1, maintainer-directed 2026-06-13). The assembly is
   NO LONGER excluded at the `coverage.config`/`TaskMaster.runsettings` `ModulePaths` level;
   instead it is treated consistently with the other four assemblies. The
   COM/VSTO/WinForms-bound classes carry class-level `[ExcludeFromCodeCoverage]`:
   `TaskController`, `TaskViewer` (+ `TaskViewer.Designer`), `FlagTasks`,
   `AutoAssignContext`, `AutoAssignPeople`, `AutoCreateProject`, `EditFilterViewer`
   (+ Designer), `ManageFilters` (+ Designer), and `EditFilterController` (fully
   WinForms/Outlook-bound). `FlagChangeGroup` is treated at **method** granularity:
   its Outlook/WinForms-bound members (`ProcessGroupAsync`, `TryProcessFlagItemAsync`,
   `ProcessFlagItemAsync`, and the `MailItem`-bound constructor) are exempt, while the
   pure-logic `TryEnqueue` seam remains measured. The genuinely-testable seams
   `FlagChangeItem` (a pure POCO with no Outlook dependency) and the testable paths of
   `FlagChangeTrainingQueue` (`Enqueue`, `ConsumeAsync`, `Init`, queue state) remain in
   the measured denominator and receive NO class-level exemption. TaskVisualization
   therefore returns to the first-party denominator with only its architecturally-untestable
   members removed.

2. **Class-level (and where noted, method-level) `[ExcludeFromCodeCoverage]`** on the
   COM/VSTO/WinForms-bound classes enumerated in the design memo for the four mixed
   assemblies:
   - TaskMaster (memo §2.2): `ThisAddIn` (+ `ThisAddIn.Designer`), `RibbonViewer`,
     `AddInUtilities`, `Ribbon/TryFunctionalityInConstruction`, `RibbonController`,
     `AppItemEngines`.
   - ToDoModel (memo §2.3): `FileOperationsPST`, `ToDoSynchronizer`, `ToDoEvents`,
     `TreeOfToDoItems`, `ProjectController`, `ProjectViewer`; plus the Outlook-dependent
     members of `IDList` (`RefreshIDList`, any constructor taking `Outlook.Application`)
     at **method** granularity.
   - QuickFiler (memo §2.4): `QfcDatamodel`, `EfcItemController`, `QfcExplorerController`,
     `KeyboardHandler`, the viewer classes (`EfcViewer`, `QfcFormViewer`,
     `QfcItemViewer*`, `ItemViewer`), `EfcFormController`, `QfcCollectionController`.
   - Tags (memo §2.5): `TagLauncher`, `CheckBoxController`.

3. **Policy documentation updates** recording the exemption rationale and the
   testable-denominator definition (design memo §4):
   - `CLAUDE.md` — General Unit Test Policy / UT2 coverage section.
   - `.claude/rules/general-unit-test.md` — Coverage Requirements section.

4. **Coverage re-measurement and C# toolchain verification** — re-run the coverage
   pipeline and the full C# toolchain (csharpier → msbuild analyzers → msbuild nullable →
   vstest with coverage) and record the post-exemption figures.

### Explicitly preserved testable seams (NOT exempt)

The exempt/non-exempt boundary is authoritatively enumerated in the design memo §2. The
following testable seams must remain measured and must not receive the attribute:

- TaskMaster (memo §2.2): `AppFileSystemFolderPaths`, `AppStagingFilenames`, `AppEvents`,
  `ApplicationGlobals`, `AppToDoObjects`, `AppQuickFilerSettings`, `AppOlObjects`,
  `AppAutoFileObjects`.
- ToDoModel (memo §2.3): `IDList.GetNextToDoID` (the pure-arithmetic path; only the
  Outlook-dependent members are exempt), `ToDoLoader`, `ProjectEntry`, `BaseChanger`,
  `ToDoDefaults`, `PrefixItem`.
- QuickFiler (memo §2.4): `KbdActions<>`, `KaChar`, `KaKey`, `KaStringAsync`,
  `KaCharAsync`, `KaKeyAsync`, `QfcHighConfidencePreFilter`, `QfcFormController`,
  `ConversationResolver`, `EfcDataModel`, `FilerQueue`, `FilerQueueItem`, `QfcQueue`,
  `QfcItemGroup`, and the partially-bound `EfcHomeController` and `QfcItemController`
  (not annotated wholesale; addressed by later increments).
- Tags (memo §2.5): `TagController` pure-logic methods (`GetSelections`, `FilterArchive`,
  `ResolvePrefix`, `ToggleChoice`, `LoadSelections`, `LoadControls`), `PrefixItem`.
- TaskVisualization (revision 1.1): `FlagChangeItem` (pure POCO; no exemption), the
  testable paths of `FlagChangeTrainingQueue` (`Enqueue`, `ConsumeAsync`, `Init`, queue
  state; no class-level exemption), and the pure-logic `FlagChangeGroup.TryEnqueue` seam
  (the class is method-level exempt only on its Outlook-bound members).

## Non-Goals

- **Roadmap increment tests (memo Phases 4–8).** Adding tests that raise covered code
  toward the 80% floor (ToDoModel pure logic, QuickFiler keyboard/queue value objects,
  TaskMaster settings/path helpers, Tags pure-logic methods, QuickFiler EfcDataModel) is
  a separate follow-up and is not part of this feature.
- **Vendored assemblies** (`Swordfish.NET.General`, `SVGControl`). Whether vendored
  third-party code should be held to the same floor is a separate decision (memo §2.6).
- **Koverage allowlist/denominator tiering** (memo Option C) — explicitly rejected in
  favor of the hybrid mechanism; no Koverage post-processing changes.
- Any production-logic refactor, including extracting helper classes purely to improve
  testability (the memo notes these as optional within later increments, not here).

## Dependencies / Touchpoints

- **Issue #193** (`.Test` assembly exclusion from the denominator) is assumed in effect.
  Verify it is merged before the `coverage.config` change; the post-exemption arithmetic
  depends on it.
- `coverage.config` and `TaskMaster.runsettings` — coverage instrumentation config.
- `CLAUDE.md` and `.claude/rules/general-unit-test.md` — authority-level policy docs;
  edits are ratified by the maintainer (Dan Moisan).
- Production source files across QuickFiler, TaskMaster, ToDoModel, Tags (attribute
  additions; `using System.Diagnostics.CodeAnalysis;` must be present or added).
- The coverage pipeline (`Invoke-MSTestWithCoverage.Helpers.ps1`, Koverage) reads the
  results; no pipeline code changes are required beyond the config exclude.
- Required coordination: none beyond maintainer ratification (already obtained
  2026-06-13).

## Risks & Mitigations

- **Over-exemption risk** — exempting a testable seam would mask a real gap. Mitigation:
  the exempt/non-exempt boundary is enumerated in the design memo §2 and listed above;
  it must be reviewed against that table during code review. Use method-level
  annotation (e.g., `IDList`) where a class mixes testable and untestable members.
- **Large change budget** — `[ExcludeFromCodeCoverage]` touches ~35–45 production `.cs`
  files across four assemblies. Mitigation: phase into reviewable batches (memo §5):
  config + docs first, then TaskMaster/ToDoModel annotations, then QuickFiler/Tags
  annotations. Each batch is independently releasable.
- **Drift risk** — a future COM-bound class added without the attribute will not be
  flagged by CI until coverage drops. Mitigation: documented in the policy update so
  the convention is discoverable; not otherwise mitigated in this feature.
- **Authority** — policy-doc edits change the quality gate. Mitigation: maintainer has
  ratified the §2 scope and §4 language (2026-06-13).
- **Floor still not reached** — post-exemption estimate is ~75.2%, below 80%. This is
  expected and acceptable; the roadmap increments (out of scope) close the remaining gap.

## Technical Specifications

- **Files/modules expected to change:**
  - `coverage.config` — revision 1.1 REMOVES the prior
    `<ModulePath>.*TaskVisualization.*</ModulePath>` exclude; TaskVisualization is no
    longer assembly-excluded.
  - `TaskMaster.runsettings` — revision 1.1 REMOVES the matching TaskVisualization
    exclude.
  - `CLAUDE.md`, `.claude/rules/general-unit-test.md` — policy text per memo §4.1/§4.2.
  - Enumerated `.cs` files in QuickFiler, TaskMaster, ToDoModel, Tags, and
    TaskVisualization (revision 1.1) — attribute additions only (and
    `using System.Diagnostics.CodeAnalysis;` where missing).
- **Public interfaces/contracts affected:** none. `[ExcludeFromCodeCoverage]` is a
  non-behavioral attribute.
- **Data flow or validation adjustments:** none.
- **Logging/telemetry updates:** none.
- **Migration or backfill needs:** none.
- **Post-exemption denominator model (design memo §3):**
  - Lines-valid removed: ~15,326 (TaskVisualization ~3,501; TaskMaster ~1,200;
    ToDoModel ~1,950; QuickFiler ~8,200; Tags ~475).
  - Covered lines removed: ~833.
  - Post-exemption denominator: ~50,442 lines-valid; ~37,934 covered →
    estimated rate **~75.2%** (range 73.2%–77.6%).
  - Formal target definition (memo §3.3): >= 80% line rate on the post-exemption
    denominator; >= 90% for new or heavily modified classes.

## Test Strategy

This feature adds no production-logic tests. Verification is by re-measurement and
toolchain pass.

- **Regression tests to add or update:** none. The existing test suite is the behavior
  regression guard; it must remain green.
- **Invariant validation (behavior unchanged):** the full MSTest suite must pass with
  identical results before and after the attribute/config changes.
- **Coverage re-measurement:** re-run the coverage pipeline; confirm the
  `TaskVisualization` package is absent from `coverage-firstparty.cobertura.xml`; confirm
  the annotated classes are removed from the denominator; confirm the enumerated testable
  seams remain present; record the post-exemption lines-valid, covered, and rate, and
  compare against the ~75.2% estimate.
- **Edge cases:** confirm method-level annotation on `IDList` removes only the
  Outlook-dependent members and leaves `GetNextToDoID` measured.
- **Toolchain commands (run in order; restart on any change/failure):**
  1. `dotnet tool run csharpier .`
  2. `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
  3. `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true`
  4. `vstest.console.exe <test-assembly-paths> /EnableCodeCoverage`
- **Manual validation:** confirm the post-exemption Cobertura XML and recorded figures
  are written to the canonical evidence location
  `docs/features/active/2026-06-13-com-vsto-coverage-exemption-197/evidence/coverage/`.

## Acceptance Criteria

- [x] `coverage.config` and `TaskMaster.runsettings` no longer exclude the
  `TaskVisualization` module via a `ModulePaths/Exclude` `ModulePath` entry (revision 1.1
  reversed the assembly-level exclude in favor of class-level `[ExcludeFromCodeCoverage]`);
  TaskVisualization is present in the first-party denominator.
  <!-- Verified: P8-T1/P8-T2 removed the excludes; P10-T6 confirms 0 TaskVisualization
  matches in both configs and the TaskVisualization package present in the R2 denominator. -->;
- [x] `[ExcludeFromCodeCoverage]` (class- or method-level per the design memo §2 tables)
  is applied to all enumerated COM/VSTO/WinForms-bound classes/members in QuickFiler,
  TaskMaster, ToDoModel, Tags, and (revision 1.1) TaskVisualization, and to none of the
  enumerated testable seams.
  <!-- Verified: Phases 2-6 for the four assemblies (P7-T7); revision 1.1 Phase 9 for
  TaskVisualization (P10-T7) — class-level on TaskController/TaskViewer/FlagTasks/
  AutoAssignContext/AutoAssignPeople/AutoCreateProject/EditFilterViewer/ManageFilters/
  EditFilterController, method-level on FlagChangeGroup's 4 Outlook-bound members;
  FlagChangeItem and FlagChangeTrainingQueue testable paths left unexempted. -->;
- [x] Post-exemption coverage re-measurement confirms the annotated classes are removed
  from the denominator (and, revision 1.1, that the `TaskVisualization` package is back in
  the denominator carrying only its preserved testable seams), and the enumerated testable
  seams (`ToDoLoader`, `IDList.GetNextToDoID`, `KbdActions<>`, `TagController`
  pure-logic methods, settings/path helpers, `FlagChangeItem`,
  `FlagChangeTrainingQueue` testable paths, etc.) remain in the denominator.
  <!-- Verified: P10-T6 (coverage-r2-classlevel-checks.md) and P10-T7
  (exemption-boundary-verification-r2.md). -->;
- [ ] The recorded post-exemption rate is consistent with the design memo §3 estimate
  (~75.2%, range 73.2%–77.6%), and the figures are written to the feature evidence
  folder.
  <!-- Figures written (evidence/qa-gates/coverage-delta.md, and revision 1.1
  evidence/qa-gates/coverage-delta-r2.md). DEVIATION: the assembly-exclude variant measured
  71.73% (1.47 pp below the §3 lower bound 73.2%); the revision 1.1 class-level variant
  measures 71.65% (1.55 pp below). Scope is correct per §2 (P7-T7, P10-T7); more covered
  lines left the denominator than the §3 midpoint estimate assumed, and the class-level
  treatment re-includes lightly-covered TaskVisualization seams. Left unchecked because the
  rate is outside the stated range; AC4 remains a separate open maintainer-acknowledgement
  item; deviation note + remediation flag recorded in coverage-delta-r2.md per P10-T8. -->;
- [x] `CLAUDE.md` (UT2 coverage section) and `.claude/rules/general-unit-test.md`
  (Coverage Requirements section) record the COM/VSTO exemption policy, rationale, and
  the testable-denominator definition per the design memo §4.
- [x] The full C# toolchain passes in a single final pass: csharpier (no diff), msbuild
  with analyzers + code style, msbuild with nullable + warnings-as-errors, and the
  MSTest suite with coverage.
- [x] No production behavior change: no method bodies, signatures, or public APIs are
  modified; only `[ExcludeFromCodeCoverage]` attributes, required `using` directives,
  config excludes, and policy docs change.

## Definition of Done

- [x] Exemption scope matches the design memo §2 tables; no testable seam is exempted
- [x] Behavior unchanged; full MSTest suite green before and after (identical pre/post failing set: the same 2 pre-existing flaky timing tests)
- [x] `coverage.config` and `TaskMaster.runsettings` no longer exclude `TaskVisualization`
  (revision 1.1); TaskVisualization is treated at class level via `[ExcludeFromCodeCoverage]`
- [x] Enumerated classes/members annotated; required `using` directives present
- [x] `CLAUDE.md` and `.claude/rules/general-unit-test.md` updated per memo §4
- [x] Post-exemption coverage re-measured and recorded to the feature evidence folder
- [x] Toolchain pass completed (format → analyzers → nullable → test)

## Seeded Test Conditions (from potential)
- [x] Coverage re-measurement confirms exempt assemblies/classes are removed from the denominator and testable seams remain.
- [x] C# build + analyzers + nullable + MSTest pass after annotation passes.
- [x] No unintended behavioral or API change from attribute additions.
