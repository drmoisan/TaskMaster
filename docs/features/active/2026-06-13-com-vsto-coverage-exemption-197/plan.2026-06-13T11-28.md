# com-vsto-coverage-exemption - Atomic Implementation Plan

- **Issue:** #197
- **Parent (optional):** none
- **Owner:** drmoisan
- **Last Updated:** 2026-06-13T16-05
- **Status:** Revised for maintainer-directed scope change (2026-06-13T16-05). Phases 0-6 (other four assemblies: TaskMaster, ToDoModel, QuickFiler, Tags) and the original Phase 7 final QA remain complete and are NOT re-opened. New revision Phases 8-10 switch TaskVisualization from assembly-level exclude to class-level `[ExcludeFromCodeCoverage]` and re-measure. AC4 deviation (prior post-exemption rate 71.73%) remains a separate open maintainer-acknowledgement item; this scope change is expected to lower the measured rate slightly and does not by itself resolve AC4.
- **Version:** 1.1
- **Work Mode:** full-feature
- **Revision (1.1, 2026-06-13T16-05):** Maintainer-directed scope change per `remediation-inputs.2026-06-13T16-05.md`. Switch `TaskVisualization` from the assembly-level `coverage.config`/`TaskMaster.runsettings` ModulePath exclude (original P1-T1/P1-T2) to class-level `[ExcludeFromCodeCoverage]`, consistent with the other four assemblies, preserving the testable seams (`FlagChangeItem`, the testable paths of `FlagChangeTrainingQueue`) in the coverage denominator. This is a maintainer-directed scope change, not a review-blocking finding (prior feature-review 2026-06-13T15-45 returned PASS with 0 blocking findings).

## Inputs (authoritative)

- Spec: `docs/features/active/2026-06-13-com-vsto-coverage-exemption-197/spec.md`
- Issue: `docs/features/active/2026-06-13-com-vsto-coverage-exemption-197/issue.md`
- Design memo (authoritative exempt/non-exempt scope tables, §2; target model §3; policy text §4):
  `artifacts/research/2026-06-12-com-vsto-coverage-exemption-design.md`
- Corrected coverage data: `artifacts/research/csharp-coverage-roadmap.2026-06-12.md` (§0) and
  `artifacts/csharp/coverage-firstparty.cobertura.xml`

## Strategy

This is a test-tooling and policy refactor: it adds `[ExcludeFromCodeCoverage]` attributes and
two policy-doc additions. It adds no production logic and no new tests. The full MSTest suite is
the behavior regression guard and must remain green with identical pass/fail results before and
after the change.

**Revision 1.1 strategy (TaskVisualization scope change):** The original Phase 1 excluded
`TaskVisualization` at the assembly level via a `coverage.config`/`TaskMaster.runsettings`
ModulePath entry. Per the maintainer directive (`remediation-inputs.2026-06-13T16-05.md`), this
treatment is reversed and replaced with class-level `[ExcludeFromCodeCoverage]` on only the
COM/VSTO/WinForms-bound classes of TaskVisualization, mirroring the discipline already applied to
the other four assemblies. The genuinely-testable seams (`FlagChangeItem` and the testable paths
of `FlagChangeTrainingQueue`) MUST remain measured (in the denominator) and MUST NOT receive a
class-level attribute that would exempt their testable half. `FlagChangeGroup` and
`EditFilterController` are assessed by inspection: exempt only if every member is genuinely
Outlook/WinForms-bound with no testable pure-logic seam; otherwise leave measured. The other four
assemblies' annotation phases (Phases 2-6) are unchanged by this directive and are NOT re-opened.

The annotation work touches many production `.cs` files across four assemblies. Per the C#
small-path budget (csharp-typed-engineer handles 1-3 production files per batch), annotation is
phased into reviewable batches grouped by assembly and by the design memo §2 class lists. Each
annotation phase is independently verifiable and ends with the mandatory C# toolchain loop.

The exempt/non-exempt boundary is authoritatively the design memo §2 tables (mirrored in
spec.md §"In scope" and §"Explicitly preserved testable seams"). The enumerated testable seams
MUST NOT receive the attribute. `IDList` is annotated at method granularity (Outlook-dependent
members only) so `GetNextToDoID` stays measured.

**Fail-closed evidence rule:** every phase includes explicit baseline/QA/coverage artifact tasks.
If any required baseline artifact, QA artifact, or coverage-comparison artifact is missing or has
incomplete fields, the audit verdict must be BLOCKED or INCOMPLETE, never PASS.

**Evidence accounting rule:** each evidence-producing task records its exact artifact path.
Work is not complete without the artifact on disk.

**Evidence location invariant:** all evidence artifacts resolve to
`docs/features/active/2026-06-13-com-vsto-coverage-exemption-197/evidence/<kind>/` per
`evidence-and-timestamp-conventions`. Non-canonical paths (`artifacts/baselines/`,
`artifacts/qa/`, `artifacts/coverage/`, etc.) are rejected. The spec's reference to
`evidence/coverage/` is normalized to the canonical `evidence/baseline/` (baseline coverage)
and `evidence/qa-gates/` (post-change coverage) sub-paths used by this plan.
`EVIDENCE_LOCATION_OVERRIDE_REJECTED: evidence/coverage/ replaced with evidence/baseline/ and evidence/qa-gates/`.

## Acceptance-Criteria Map (spec §Acceptance Criteria -> task IDs)

> **Revision 1.1 update:** AC1 originally mapped to the assembly-level TaskVisualization exclude
> (P1-T1/P1-T2). Per the maintainer directive that exclude is reversed (P8-T1/P8-T2) and
> TaskVisualization is now treated at class level (Phase 9). AC1 is re-scoped accordingly: the
> assembly exclude must be ABSENT for TaskVisualization, and the original P1-T1/P1-T2 are
> superseded for TaskVisualization only.

- AC1 (revised — `coverage.config` + `TaskMaster.runsettings` no longer exclude `TaskVisualization`): P8-T1, P8-T2, P10-T6
- AC2 (`[ExcludeFromCodeCoverage]` on all enumerated COM/VSTO/WinForms classes, none on testable seams):
  P2-T1, P2-T2, P3-T1, P3-T2, P3-T3, P4-T1, P4-T2, P4-T3, P5-T1, P5-T2, P5-T3, P6-T1, P9-T1, P9-T2, P9-T3; non-exemption verified by P7-T7 and P10-T7
- AC3 (re-measurement confirms exempt classes removed and testable seams remain): P10-T6, P10-T7
- AC4 (recorded post-exemption rate; the class-level TaskVisualization treatment raises the denominator vs the assembly-exclude variant — AC4 remains a separate open maintainer-acknowledgement item): P10-T8
- AC5 (`CLAUDE.md` UT2 + `.claude/rules/general-unit-test.md` record exemption policy/rationale/denominator): P1-T3, P1-T4
- AC6 (full C# toolchain passes in a single final pass): P10-T1, P10-T2, P10-T3, P10-T4
- AC7 (no production behavior change; only attributes/usings/config/docs): enforced by every annotation phase toolchain loop and P10-T5 (test result parity)
- AC8 (revision 1.1 — `spec.md` exempt-scope section reflects class-level TaskVisualization treatment with preserved seams enumerated): P8-T3

---

### Phase 0 — Baseline Capture & Policy Reads

- [x] [P0-T1] Read policy files in required order and record a Phase 0 policy-read evidence artifact at `docs/features/active/2026-06-13-com-vsto-coverage-exemption-197/evidence/baseline/phase0-instructions-read.md` containing `Timestamp:`, `Policy Order:`, and the explicit file list: `CLAUDE.md`, `.claude/rules/general-code-change.md`, `.claude/rules/general-unit-test.md`, `.claude/rules/csharp.md` (if present), `.claude/skills/atomic-plan-contract/SKILL.md`, `.claude/skills/evidence-and-timestamp-conventions/SKILL.md`. Acceptance: artifact exists with all three required fields populated.
- [x] [P0-T2] Confirm Issue #193 (`.Test` assembly exclusion from denominator) is merged into the current branch base and record the verification (commit/PR reference and a one-line confirmation) at `docs/features/active/2026-06-13-com-vsto-coverage-exemption-197/evidence/baseline/dependency-193-check.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`. Acceptance: artifact records that #193 is in effect (or a BLOCKED note if not). Reference: spec §Dependencies/Touchpoints.
- [x] [P0-T3] Capture baseline csharpier formatting state by running `dotnet tool run csharpier --check .` and record the result at `docs/features/active/2026-06-13-com-vsto-coverage-exemption-197/evidence/baseline/csharpier-baseline.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`. Acceptance: artifact exists with all four fields.
- [x] [P0-T4] Capture baseline analyzer/code-style build by running `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` and record at `docs/features/active/2026-06-13-com-vsto-coverage-exemption-197/evidence/baseline/analyzer-baseline.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`. Acceptance: artifact exists with all four fields.
- [x] [P0-T5] Capture baseline nullable/warnings-as-errors build by running `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true` and record at `docs/features/active/2026-06-13-com-vsto-coverage-exemption-197/evidence/baseline/nullable-baseline.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`. Acceptance: artifact exists with all four fields.
- [x] [P0-T6] Capture baseline MSTest run with coverage by running `vstest.console.exe <test-assembly-paths> /EnableCodeCoverage /Settings:TaskMaster.runsettings` (the repo coverage pipeline `scripts/vscode/Invoke-MSTestWithCoverage.ps1` may be used to produce the deduped first-party Cobertura) and record at `docs/features/active/2026-06-13-com-vsto-coverage-exemption-197/evidence/baseline/mstest-coverage-baseline.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:` including the numeric test pass/fail counts AND the production-only deduped coverage headline (baseline expected ~58.95%, ~38,767 covered / ~65,768 lines-valid per roadmap §0.2). Acceptance: artifact exists with all fields and numeric coverage values (no placeholders).
- [x] [P0-T7] Copy or reference the pre-change first-party Cobertura (`artifacts/csharp/coverage-firstparty.cobertura.xml`) as the authoritative baseline coverage artifact into `docs/features/active/2026-06-13-com-vsto-coverage-exemption-197/evidence/baseline/coverage-firstparty.baseline.cobertura.xml` and record the per-assembly baseline rates (QuickFiler 25.15%, Tags 31.15%, TaskMaster 25.68%, TaskVisualization 0.37%, ToDoModel 10.43%) in a companion note `coverage-firstparty.baseline-summary.md` with `Timestamp:`. Acceptance: both artifacts exist; per-assembly baseline rates recorded.

---

### Phase 1 — Coverage Config Excludes & Policy Documentation

- [x] [P1-T1] In `coverage.config`, add `<ModulePath>.*TaskVisualization.*</ModulePath>` inside the existing `ModulePaths/Exclude` block (after the `.*MSTest.*` entry). Acceptance: the `TaskVisualization` ModulePath exclude is present and the file remains valid XML with no other entries changed. Reference: design memo §2.1, spec §In scope item 1.
- [x] [P1-T2] In `TaskMaster.runsettings`, add the matching `<ModulePath>.*TaskVisualization.*</ModulePath>` inside the `DataCollectionRunSettings`/`CodeCoverage`/`ModulePaths/Exclude` block. Acceptance: the `TaskVisualization` ModulePath exclude is present and the file remains valid XML with no other entries changed. Reference: spec §Technical Specifications, design memo §2.1.
- [x] [P1-T3] In `CLAUDE.md`, add the COM/VSTO exemption text to the General Unit Test Policy / UT2 coverage section, immediately after the `Repository-wide line coverage must remain >= 80%.` line, using the verbatim policy language from design memo §4.1 (testable-denominator definition, exclusion categories (a)/(b)/(c), `[ExcludeFromCodeCoverage]` + assembly-exclude mechanisms, maintainer-authority note, and the explicit not-exempt seams list `ToDoLoader`, `IDList` arithmetic, `KbdActions<>`, path/settings helpers). Acceptance: the §4.1 language is present in the UT2 coverage section and the prior coverage requirement line is preserved. Reference: design memo §4.1, spec §In scope item 3.
- [x] [P1-T4] In `.claude/rules/general-unit-test.md`, add the parallel exemption note to the Coverage Requirements section using the verbatim language from design memo §4.2 (architectural COM-host binding rationale, `[ExcludeFromCodeCoverage]`/assembly-exclude mechanisms, cross-reference that exemption scope/rationale/boundary is documented in `CLAUDE.md`, maintainer-ratification requirement). Acceptance: the §4.2 note is present in the Coverage Requirements section and the prior `>= 80%` line is preserved. Reference: design memo §4.2, spec §In scope item 3. NOTE: This edits a `.claude/rules/` policy file; the maintainer ratified this specific edit on 2026-06-13 (spec §Dependencies, design memo §4.3), which authorizes the otherwise-prohibited policy-doc change for this feature only.
- [x] [P1-T5] Run the full C# toolchain loop for Phase 1 changes and record each step artifact under `docs/features/active/2026-06-13-com-vsto-coverage-exemption-197/evidence/qa-gates/`: csharpier (`phase1-csharpier.md`), analyzer build (`phase1-analyzer.md`), nullable build (`phase1-nullable.md`), MSTest with coverage (`phase1-mstest.md`). Each artifact requires `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`; the MSTest artifact `Output Summary:` must include test pass/fail counts and the coverage headline (TaskVisualization expected absent from the first-party denominator, denominator dropped by ~3,501 lines). Restart the loop from csharpier if any step changes files or fails. Acceptance: all four step artifacts exist; final pass is clean; TaskVisualization package confirmed absent from regenerated `coverage-firstparty.cobertura.xml`. Reference: design memo §5 Phase 1 verification.

---

### Phase 2 — Annotation Pass: TaskMaster (memo §2.2)

- [x] [P2-T1] Add `[ExcludeFromCodeCoverage]` (class-level) and the `using System.Diagnostics.CodeAnalysis;` directive where missing to: `TaskMaster\ThisAddIn.cs`, `TaskMaster\ThisAddIn.Designer.cs`, `TaskMaster\AddInUtilities.cs`. Acceptance: each named type carries the attribute; required `using` present; no method body, signature, or public API changed. Do NOT annotate `AppFileSystemFolderPaths`, `AppStagingFilenames`, `AppEvents`, `ApplicationGlobals`, `AppToDoObjects`, `AppQuickFilerSettings`, `AppOlObjects`, or `AppAutoFileObjects`. Reference: design memo §2.2.
- [x] [P2-T2] Add `[ExcludeFromCodeCoverage]` (class-level) and the `using System.Diagnostics.CodeAnalysis;` directive where missing to: `TaskMaster\Ribbon\RibbonViewer.cs`, `TaskMaster\Ribbon\TryFunctionalityInConstruction.cs`, `TaskMaster\Ribbon\RibbonController.cs`, and `TaskMaster\AppGlobals\AppItemEngines.cs` (split into two sub-edits of <=3 files each if the executor's per-batch budget requires; both sub-edits belong to this task). Acceptance: each named type carries the attribute; required `using` present; no logic change; the testable seams in P2-T1's do-not-annotate list remain unannotated. Reference: design memo §2.2.
- [x] [P2-T3] Run the full C# toolchain loop for Phase 2 and record step artifacts under `docs/features/active/2026-06-13-com-vsto-coverage-exemption-197/evidence/qa-gates/`: `phase2-csharpier.md`, `phase2-analyzer.md`, `phase2-nullable.md`, `phase2-mstest.md`. Each requires `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`; MSTest artifact records pass/fail counts and coverage headline. Restart from csharpier on any file change or failure. Acceptance: all four artifacts exist; final pass clean; TaskMaster annotated classes confirmed removed from the denominator while the §2.2 testable seams remain present.

---

### Phase 3 — Annotation Pass: ToDoModel (memo §2.3)

- [x] [P3-T1] Add `[ExcludeFromCodeCoverage]` (class-level) and `using System.Diagnostics.CodeAnalysis;` where missing to: `ToDoModel\Common Functions\FileOperationsPST.cs`, `ToDoModel\Data Model\ToDo\ToDoSynchronizer.cs`, `ToDoModel\Data Model\ToDo\ToDoEvents.cs`. Acceptance: each named type carries the attribute; required `using` present; no logic change. Reference: design memo §2.3.
- [x] [P3-T2] Add `[ExcludeFromCodeCoverage]` (class-level) and `using System.Diagnostics.CodeAnalysis;` where missing to: `ToDoModel\Data Model\Tree\TreeOfToDoItems.cs`, `ToDoModel\Data Model\Project\ProjectController.cs`, `ToDoModel\Data Model\Project\ProjectViewer.cs`. Acceptance: each named type carries the attribute; required `using` present; no logic change. Reference: design memo §2.3.
- [x] [P3-T3] In `ToDoModel\Data Model\ID\IDList.cs`, apply `[ExcludeFromCodeCoverage]` at METHOD granularity to only the Outlook-dependent members (`RefreshIDList` and any constructor taking `Microsoft.Office.Interop.Outlook.Application`), add `using System.Diagnostics.CodeAnalysis;` if missing, and leave `GetNextToDoID(string seed)` and the `IDList(IList<string>)` constructor UNANNOTATED. Acceptance: only the Outlook-dependent members carry the attribute; `GetNextToDoID` is not annotated; no logic change. Do NOT annotate `ToDoLoader`, `ProjectEntry`, `BaseChanger`, `ToDoDefaults`, or `PrefixItem`. Reference: design memo §2.3, spec §Test Strategy edge case.
- [x] [P3-T4] Run the full C# toolchain loop for Phase 3 and record step artifacts under `docs/features/active/2026-06-13-com-vsto-coverage-exemption-197/evidence/qa-gates/`: `phase3-csharpier.md`, `phase3-analyzer.md`, `phase3-nullable.md`, `phase3-mstest.md`. Each requires `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`; MSTest artifact records pass/fail counts and coverage headline. Restart from csharpier on any change or failure. Acceptance: all four artifacts exist; final pass clean; ToDoModel annotated classes/members removed from the denominator; `IDList.GetNextToDoID`, `ToDoLoader`, `ProjectEntry`, `BaseChanger` confirmed still present.

---

### Phase 4 — Annotation Pass: QuickFiler Controllers (memo §2.4, controller subset)

- [x] [P4-T1] Add `[ExcludeFromCodeCoverage]` (class-level) and `using System.Diagnostics.CodeAnalysis;` where missing to: `QuickFiler\Controllers\QfcDatamodel.cs`, `QuickFiler\Controllers\EfcItemController.cs`, `QuickFiler\Controllers\QfcExplorerController.cs`. Acceptance: each named type carries the attribute; required `using` present; no logic change. Reference: design memo §2.4.
- [x] [P4-T2] Add `[ExcludeFromCodeCoverage]` (class-level) and `using System.Diagnostics.CodeAnalysis;` where missing to: `QuickFiler\Controllers\KeyboardHandler.cs`, `QuickFiler\Controllers\EfcFormController.cs`, `QuickFiler\Controllers\QfcCollectionController.cs`. Acceptance: each named type carries the attribute; required `using` present; no logic change. Do NOT annotate `EfcHomeController`, `QfcItemController`, `KbdActions<>`, `KaChar`, `KaKey`, `KaStringAsync`, `KaCharAsync`, `KaKeyAsync`, `QfcHighConfidencePreFilter`, `QfcFormController`, `ConversationResolver`, `EfcDataModel`, `FilerQueue`, `FilerQueueItem`, `QfcQueue`, or `QfcItemGroup`. Reference: design memo §2.4.
- [x] [P4-T3] Run the full C# toolchain loop for Phase 4 and record step artifacts under `docs/features/active/2026-06-13-com-vsto-coverage-exemption-197/evidence/qa-gates/`: `phase4-csharpier.md`, `phase4-analyzer.md`, `phase4-nullable.md`, `phase4-mstest.md`. Each requires `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`; MSTest artifact records pass/fail counts and coverage headline. Restart from csharpier on any change or failure. Acceptance: all four artifacts exist; final pass clean; the annotated controllers removed from the denominator; the §2.4 testable seams (`KbdActions<>`, `EfcDataModel`, `QfcFormController`, etc.) confirmed still present.

---

### Phase 5 — Annotation Pass: QuickFiler Viewers (memo §2.4, viewer subset)

- [x] [P5-T1] Add `[ExcludeFromCodeCoverage]` (class-level) and `using System.Diagnostics.CodeAnalysis;` where missing to the viewer code-behind and Designer pair for `EfcViewer` and `QfcFormViewer`: `QuickFiler\Viewers\EfcViewer.cs`, `QuickFiler\Viewers\EfcViewer.Designer.cs`, `QuickFiler\Viewers\QfcFormViewer.cs`, `QuickFiler\Viewers\QfcFormViewer.Designer.cs` (this 4-file edit may be split into two sub-edits of 2 files each to respect the per-batch budget; both belong to this task). Acceptance: each named type carries the attribute; required `using` present; no logic change. Reference: design memo §2.4 (viewer classes: `EfcViewer`/`QfcFormViewer`/`QfcItemViewer*`/`ItemViewer`).
- [x] [P5-T2] Add `[ExcludeFromCodeCoverage]` (class-level) and `using System.Diagnostics.CodeAnalysis;` where missing to the `QfcItemViewer*` family and `ItemViewer`: `QuickFiler\Viewers\QfcItemViewer.cs` (+ `.Designer.cs`), `QuickFiler\Viewers\QfcItemViewerExpanded.cs` (+ `.Designer.cs`), `QuickFiler\Viewers\QfcItemViewerExpandedLight.cs` (+ `.Designer.cs`), `QuickFiler\Viewers\QfcItemViewerLightSelected.cs` (+ `.Designer.cs`), `QuickFiler\Viewers\QfcItemViewerV1.cs` (+ `.Designer.cs`), `QuickFiler\Viewers\ItemViewer.cs` (+ `.Designer.cs`). Split into sequential sub-edits of <=3 files each (each code-behind + its Designer counts as one logical type); all sub-edits belong to this task. Acceptance: every `QfcItemViewer*` type and `ItemViewer` carries the attribute; required `using` present; no logic change. Reference: design memo §2.4. NOTE: scope is limited to the memo-enumerated `QfcItemViewer*`/`ItemViewer` viewer set; viewers NOT named in memo §2.4 (e.g., `BayesianPerformanceViewer`, `Form1`, `ToolStripMenuItemCb`, `QFCItemViewerDarkNew`, `QFCItemViewerLightNew`, `ItemViewerExpanded`, `EfcViewer3`, `QfcFormViewerDark`, `QfcFormViewerExpanded`) are out of scope and MUST NOT be annotated in this feature.
- [x] [P5-T3] Run the full C# toolchain loop for Phase 5 and record step artifacts under `docs/features/active/2026-06-13-com-vsto-coverage-exemption-197/evidence/qa-gates/`: `phase5-csharpier.md`, `phase5-analyzer.md`, `phase5-nullable.md`, `phase5-mstest.md`. Each requires `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`; MSTest artifact records pass/fail counts and coverage headline (QuickFiler line rate expected to rise materially as the viewers leave the denominator). Restart from csharpier on any change or failure. Acceptance: all four artifacts exist; final pass clean.

---

### Phase 6 — Annotation Pass: Tags (memo §2.5)

- [x] [P6-T1] Add `[ExcludeFromCodeCoverage]` (class-level) and `using System.Diagnostics.CodeAnalysis;` where missing to `Tags\TagLauncher.cs` and the WinForms CheckBox event-handler `CheckBoxController` class identified in memo §2.5 (resolve the correct file between `Tags\CheckBoxController.cs` and `Tags\Helper Classes\CheckBoxController.cs` by matching the memo's description "WinForms `CheckBox` event handler"; annotate only that one). Acceptance: `TagLauncher` and the correct `CheckBoxController` carry the attribute; required `using` present; no logic change. Do NOT annotate `TagController` (pure-logic methods `GetSelections`, `FilterArchive`, `ResolvePrefix`, `ToggleChoice`, `LoadSelections`, `LoadControls`) or `PrefixItem`. Reference: design memo §2.5.
- [x] [P6-T2] Run the full C# toolchain loop for Phase 6 and record step artifacts under `docs/features/active/2026-06-13-com-vsto-coverage-exemption-197/evidence/qa-gates/`: `phase6-csharpier.md`, `phase6-analyzer.md`, `phase6-nullable.md`, `phase6-mstest.md`. Each requires `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`; MSTest artifact records pass/fail counts and coverage headline. Restart from csharpier on any change or failure. Acceptance: all four artifacts exist; final pass clean; `TagLauncher`/`CheckBoxController` removed from the denominator; `TagController` pure-logic methods confirmed still present.

---

### Phase 7 — Final QA Loop, Coverage Re-measurement & Acceptance Verification

- [x] [P7-T1] Run `dotnet tool run csharpier --check .` as the final-QC formatting gate and record `docs/features/active/2026-06-13-com-vsto-coverage-exemption-197/evidence/qa-gates/final-csharpier.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`. Acceptance: EXIT_CODE 0 (no formatting diff). If csharpier reformats any file, restart the final loop at P7-T1.
- [x] [P7-T2] Run `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` as the final-QC analyzer/code-style gate and record `docs/features/active/2026-06-13-com-vsto-coverage-exemption-197/evidence/qa-gates/final-analyzer.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`. Acceptance: EXIT_CODE 0, no analyzer/code-style errors.
- [x] [P7-T3] Run `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true` as the final-QC nullable/type-check gate and record `docs/features/active/2026-06-13-com-vsto-coverage-exemption-197/evidence/qa-gates/final-nullable.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`. Acceptance: EXIT_CODE 0, no nullable or warnings-as-errors failures.
- [x] [P7-T4] Run the MSTest suite with coverage as the final-QC test gate (`vstest.console.exe <test-assembly-paths> /EnableCodeCoverage /Settings:TaskMaster.runsettings`, or the `scripts/vscode/Invoke-MSTestWithCoverage.ps1` pipeline producing the deduped first-party Cobertura) and record `docs/features/active/2026-06-13-com-vsto-coverage-exemption-197/evidence/qa-gates/final-mstest-coverage.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:` including numeric test pass/fail counts and the post-change production-only coverage headline. Acceptance: artifact exists with numeric coverage values (no placeholders); test results recorded. If any of P7-T1..P7-T4 fails or changes files, restart the loop from P7-T1.
- [x] [P7-T5] Verify behavior parity by comparing the final-QC MSTest pass/fail set (P7-T4) against the Phase 0 baseline (P0-T6) and record `docs/features/active/2026-06-13-com-vsto-coverage-exemption-197/evidence/qa-gates/test-result-parity.md` with `Timestamp:` and an explicit before/after pass/fail comparison. Acceptance: the post-change pass/fail set is identical to baseline (allowing for the 2 pre-existing failures noted in roadmap §0.1); any new failure is a BLOCKED outcome. Reference: spec §Invariants "No production behavior change", AC7.
- [x] [P7-T6] Save the post-change first-party Cobertura to `docs/features/active/2026-06-13-com-vsto-coverage-exemption-197/evidence/qa-gates/coverage-firstparty.postexemption.cobertura.xml` and confirm in a companion note `coverage-postexemption-checks.md` (with `Timestamp:`) that: (a) the `TaskVisualization` package is absent from the first-party denominator; (b) `coverage.config` and `TaskMaster.runsettings` both contain the `TaskVisualization` ModulePath exclude. Acceptance: post-change Cobertura saved; both config checks recorded as confirmed. Reference: AC1, AC3, spec §Test Strategy.
- [x] [P7-T7] Verify the exempt/non-exempt boundary against design memo §2 and record `docs/features/active/2026-06-13-com-vsto-coverage-exemption-197/evidence/qa-gates/exemption-boundary-verification.md` with `Timestamp:`, confirming (a) every enumerated COM/VSTO/WinForms class/member in memo §2.2-§2.5 carries `[ExcludeFromCodeCoverage]` (or is excluded via `coverage.config` for TaskVisualization) and is absent from the post-change denominator; (b) every enumerated testable seam (`ToDoLoader`, `IDList.GetNextToDoID`, `ProjectEntry`, `BaseChanger`, `KbdActions<>`, `KaChar`/`KaKey`/`KaStringAsync`/`KaCharAsync`/`KaKeyAsync`, `QfcHighConfidencePreFilter`, `QfcFormController`, `ConversationResolver`, `EfcDataModel`, `FilerQueue`/`FilerQueueItem`/`QfcQueue`/`QfcItemGroup`, `AppFileSystemFolderPaths`, `AppStagingFilenames`, `AppEvents`, `ApplicationGlobals`, `AppToDoObjects`, `AppQuickFilerSettings`, `AppOlObjects`, `AppAutoFileObjects`, `TagController` pure-logic methods, `PrefixItem`) is NOT annotated and remains present in the post-change denominator. Acceptance: both lists verified; any mismatch is a BLOCKED outcome requiring remediation. Reference: AC2, AC3, spec §Explicitly preserved testable seams.
- [x] [P7-T8] Compute and record the coverage delta at `docs/features/active/2026-06-13-com-vsto-coverage-exemption-197/evidence/qa-gates/coverage-delta.md` with `Timestamp:` reporting: baseline coverage (P0-T6/P0-T7), post-change coverage (P7-T4/P7-T6), the change in lines-valid and lines-covered, and the resulting post-exemption rate. Acceptance: the recorded post-exemption rate is consistent with the design memo §3 estimate (~75.2%, range 73.2%-77.6%); record a deviation note and remediation flag if the rate falls outside that range. Reference: AC4, design memo §3.

---

> **Revision 1.1 — Maintainer-directed scope change (2026-06-13T16-05).** Phases 0-7 above are
> complete and are NOT re-opened. The phases below reverse the assembly-level TaskVisualization
> exclude and replace it with class-level `[ExcludeFromCodeCoverage]`, then re-measure. Source:
> `docs/features/active/2026-06-13-com-vsto-coverage-exemption-197/remediation-inputs.2026-06-13T16-05.md`.

### Phase 8 — Revision: Remove TaskVisualization Assembly Exclude & Update Spec

- [x] [P8-T1] In `coverage.config`, REMOVE the `<ModulePath>.*TaskVisualization.*</ModulePath>` entry that was added by the original P1-T1 (inside the `ModulePaths/Exclude` block). Leave all pre-existing third-party excludes unchanged (Deedle, FSharp, Castle.Core, FluentAssertions, Moq, Microsoft.Testing, MSTest). Acceptance: the `TaskVisualization` ModulePath exclude is absent from `coverage.config`; the file remains valid XML; no pre-existing entry is changed or removed. Reference: directive Required-changes item 1.
- [x] [P8-T2] In `TaskMaster.runsettings`, REMOVE the `<ModulePath>.*TaskVisualization.*</ModulePath>` entry that was added by the original P1-T2 (inside the `DataCollectionRunSettings`/`CodeCoverage`/`ModulePaths/Exclude` block). Leave all pre-existing excludes unchanged. Acceptance: the `TaskVisualization` ModulePath exclude is absent from `TaskMaster.runsettings`; the file remains valid XML; no pre-existing entry is changed or removed. Reference: directive Required-changes item 2.
- [x] [P8-T3] In `docs/features/active/2026-06-13-com-vsto-coverage-exemption-197/spec.md`, update the exempt-scope section that references `TaskVisualization` to describe the class-level `[ExcludeFromCodeCoverage]` treatment (replacing the prior assembly-exclude description), and enumerate the explicitly-preserved testable seams (`FlagChangeItem` and the testable paths of `FlagChangeTrainingQueue`). Keep AC wording consistent. Acceptance: spec.md no longer describes TaskVisualization as assembly-excluded; the class-level treatment and preserved seams are documented; AC text remains consistent. Reference: directive Required-changes item 4.
- [x] [P8-T4] Run the full C# toolchain loop for the Phase 8 config/spec changes and record each step artifact under `docs/features/active/2026-06-13-com-vsto-coverage-exemption-197/evidence/qa-gates/`: csharpier (`phase8-csharpier.md`), analyzer build (`phase8-analyzer.md`), nullable build (`phase8-nullable.md`), MSTest with coverage (`phase8-mstest.md`). Each artifact requires `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`; the MSTest artifact `Output Summary:` must include test pass/fail counts and the coverage headline, and must confirm the `TaskVisualization` package has RETURNED to the regenerated first-party `coverage-firstparty.cobertura.xml` denominator (it is no longer assembly-excluded). Restart the loop from csharpier if any step changes files or fails. Acceptance: all four step artifacts exist with all four fields; final pass is clean; `TaskVisualization` confirmed present in the first-party denominator. Reference: directive Required-changes item 5.

---

### Phase 9 — Revision: Class-Level Annotation Pass for TaskVisualization

> Batch sized to the C# small-path budget (1-3 production files per batch). Each code-behind plus
> its `.Designer.cs` counts as one logical type. The exempt set is the COM/VSTO/WinForms-bound
> classes; `FlagChangeItem` and the testable paths of `FlagChangeTrainingQueue` MUST NOT be
> annotated at class level. `FlagChangeGroup` and `EditFilterController` are assess-by-inspection.
> Apply the scope-change rule: if inspection shows a class listed exempt is actually a testable
> seam (or vice versa), record the finding and adjust rather than blindly annotating.

- [x] [P9-T1] Add `[ExcludeFromCodeCoverage]` (class-level) and the `using System.Diagnostics.CodeAnalysis;` directive where missing to the Outlook/WinForms-bound classes: `TaskVisualization\TaskController.cs`, `TaskVisualization\TaskViewer.cs` (+ `TaskVisualization\TaskViewer.Designer.cs`), `TaskVisualization\FlagTasks.cs`. Acceptance: each named type carries the attribute; required `using` present; no method body, signature, or public API changed. Reference: directive Required-changes item 3 (exempt list).
- [x] [P9-T2] Add `[ExcludeFromCodeCoverage]` (class-level) and `using System.Diagnostics.CodeAnalysis;` where missing to: `TaskVisualization\AutoAssignContext.cs`, `TaskVisualization\AutoAssignPeople.cs`, `TaskVisualization\AutoCreateProject.cs`. Acceptance: each named type carries the attribute; required `using` present; no logic change. Reference: directive Required-changes item 3 (exempt list).
- [x] [P9-T3] Add `[ExcludeFromCodeCoverage]` (class-level) and `using System.Diagnostics.CodeAnalysis;` where missing to the WinForms editor/filter classes: `TaskVisualization\EditFilterViewer.cs` (+ `TaskVisualization\EditFilterViewer.designer.cs`), `TaskVisualization\ManageFilters.cs` (+ `TaskVisualization\ManageFilters.Designer.cs`). Acceptance: each named type carries the attribute; required `using` present; no logic change. Reference: directive Required-changes item 3 (exempt list).
- [x] [P9-T4] Assess `TaskVisualization\FlagChangeGroup.cs` and `TaskVisualization\EditFilterController.cs` by inspection and record the determination at `docs/features/active/2026-06-13-com-vsto-coverage-exemption-197/evidence/other/taskvis-inspection-assessment.md` with `Timestamp:`. For each class, record whether every member is genuinely Outlook/WinForms-bound with no testable pure-logic seam. If genuinely fully bound: apply class-level `[ExcludeFromCodeCoverage]` (+ `using` if missing). If a testable pure-logic seam exists: leave the class unannotated (or annotate only the genuinely Outlook-bound methods at method level, mirroring the `IDList` method-level approach), and record which members were annotated. Acceptance: the assessment artifact records the per-class determination and the applied treatment; any annotation applied carries no logic change. Reference: directive Required-changes item 3 (assess-by-inspection).
- [x] [P9-T5] Confirm the preserved testable seams are NOT exempted: `TaskVisualization\FlagChangeItem.cs` carries no `[ExcludeFromCodeCoverage]`, and the testable paths of `TaskVisualization\FlagChangeTrainingQueue.cs` carry no class-level exemption (only genuinely Outlook-bound methods, if any, may carry a method-level attribute). Record the confirmation at `docs/features/active/2026-06-13-com-vsto-coverage-exemption-197/evidence/other/taskvis-preserved-seams.md` with `Timestamp:` listing each preserved seam and its annotation state. Acceptance: artifact confirms `FlagChangeItem` and `FlagChangeTrainingQueue` testable paths remain unexempted; any exemption found on a preserved seam is a BLOCKED outcome requiring remediation. Reference: directive Required-changes item 3 (preserve list), Acceptance-for-this-cycle bullet 2.
- [x] [P9-T6] Run the full C# toolchain loop for Phase 9 and record step artifacts under `docs/features/active/2026-06-13-com-vsto-coverage-exemption-197/evidence/qa-gates/`: `phase9-csharpier.md`, `phase9-analyzer.md`, `phase9-nullable.md`, `phase9-mstest.md`. Each requires `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`; the MSTest artifact records pass/fail counts and the coverage headline. Restart from csharpier on any file change or failure. Acceptance: all four artifacts exist with all four fields; final pass clean; the annotated TaskVisualization COM/WinForms classes are confirmed absent from the post-change denominator while `FlagChangeItem` and the `FlagChangeTrainingQueue` testable paths remain present in it.

---

### Phase 10 — Revision: Final QA Loop & Coverage Re-measurement (post class-level TaskVisualization)

- [x] [P10-T1] Run `dotnet tool run csharpier --check .` as the final-QC formatting gate and record `docs/features/active/2026-06-13-com-vsto-coverage-exemption-197/evidence/qa-gates/final-r2-csharpier.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`. Acceptance: EXIT_CODE 0 (no formatting diff). If csharpier reformats any file, restart the final loop at P10-T1.
- [x] [P10-T2] Run `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` as the final-QC analyzer/code-style gate and record `docs/features/active/2026-06-13-com-vsto-coverage-exemption-197/evidence/qa-gates/final-r2-analyzer.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`. Acceptance: EXIT_CODE 0, no analyzer/code-style errors.
- [x] [P10-T3] Run `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true` as the final-QC nullable/type-check gate and record `docs/features/active/2026-06-13-com-vsto-coverage-exemption-197/evidence/qa-gates/final-r2-nullable.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`. Acceptance: EXIT_CODE 0, no nullable or warnings-as-errors failures.
- [x] [P10-T4] Run the MSTest suite with coverage as the final-QC test gate (`vstest.console.exe <test-assembly-paths> /EnableCodeCoverage /Settings:TaskMaster.runsettings`, or the `scripts/vscode/Invoke-MSTestWithCoverage.ps1` pipeline producing the deduped first-party Cobertura) and record `docs/features/active/2026-06-13-com-vsto-coverage-exemption-197/evidence/qa-gates/final-r2-mstest-coverage.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:` including numeric test pass/fail counts and the post-change production-only coverage headline (numeric, no placeholders). Acceptance: artifact exists with numeric coverage values; test results recorded. If any of P10-T1..P10-T4 fails or changes files, restart the loop from P10-T1.
- [x] [P10-T5] Verify behavior parity by comparing the Phase 10 final-QC MSTest pass/fail set (P10-T4) against the Phase 0 baseline (P0-T6) and record `docs/features/active/2026-06-13-com-vsto-coverage-exemption-197/evidence/qa-gates/test-result-parity-r2.md` with `Timestamp:` and an explicit before/after pass/fail comparison. Acceptance: the post-change pass/fail set is identical to baseline (allowing for the 2 pre-existing failures noted in roadmap §0.1); any new failure is a BLOCKED outcome. Reference: spec §Invariants "No production behavior change", AC7.
- [x] [P10-T6] Save the post-change first-party Cobertura to `docs/features/active/2026-06-13-com-vsto-coverage-exemption-197/evidence/qa-gates/coverage-firstparty.r2-classlevel.cobertura.xml` and confirm in a companion note `coverage-r2-classlevel-checks.md` (with `Timestamp:`) that: (a) the `TaskVisualization` package is PRESENT in the first-party denominator (no longer assembly-excluded); (b) `coverage.config` and `TaskMaster.runsettings` no longer contain the `TaskVisualization` ModulePath exclude; (c) the annotated COM/WinForms TaskVisualization classes are absent from the denominator while `FlagChangeItem` and the `FlagChangeTrainingQueue` testable paths are present. Acceptance: post-change Cobertura saved; all three checks recorded as confirmed; any failure is a BLOCKED outcome. Reference: AC1, AC3, directive Acceptance-for-this-cycle bullets 1-2.
- [x] [P10-T7] Verify the revised TaskVisualization exempt/non-exempt boundary and record `docs/features/active/2026-06-13-com-vsto-coverage-exemption-197/evidence/qa-gates/exemption-boundary-verification-r2.md` with `Timestamp:`, confirming (a) every TaskVisualization COM/VSTO/WinForms class annotated in Phase 9 (`TaskController`, `TaskViewer`, `FlagTasks`, `AutoAssignContext`, `AutoAssignPeople`, `AutoCreateProject`, `EditFilterViewer`, `ManageFilters`, plus any `FlagChangeGroup`/`EditFilterController` determination from P9-T4) carries `[ExcludeFromCodeCoverage]` and is absent from the post-change denominator; (b) `FlagChangeItem` and the testable paths of `FlagChangeTrainingQueue` are NOT class-level annotated and remain present in the post-change denominator; (c) the other four assemblies' annotations (Phases 2-6) are unchanged. Acceptance: all three checks verified; any mismatch is a BLOCKED outcome requiring remediation. Reference: AC2, AC3, directive Required-changes item 3.
- [x] [P10-T8] Compute and record the coverage delta at `docs/features/active/2026-06-13-com-vsto-coverage-exemption-197/evidence/qa-gates/coverage-delta-r2.md` with `Timestamp:` reporting: the prior assembly-exclude post-change coverage (original P7-T8 / `coverage-delta.md`, 71.73%), the new class-level post-change coverage (P10-T4/P10-T6), the change in lines-valid and lines-covered (the denominator is expected to RISE as the preserved TaskVisualization testable lines return), and the resulting production-only rate. Acceptance: the artifact records the prior rate, the new rate, and the denominator/numerator deltas; note that AC4 (measured rate vs the design §3 estimate) remains a separate open maintainer-acknowledgement item and that the class-level treatment is expected to lower the measured rate slightly relative to the assembly-exclude variant. Reference: AC4, directive Required-changes item 5, Constraints bullet 4.

---

## Test Plan

- Behavior regression guard: full MSTest suite, identical pass/fail set before (P0-T6) and after (P7-T4), verified by P7-T5.
- Coverage evidence:
  - Baseline coverage artifacts: P0-T6 (`evidence/baseline/mstest-coverage-baseline.md`), P0-T7 (`evidence/baseline/coverage-firstparty.baseline.cobertura.xml`).
  - Post-change coverage artifacts: P7-T4 (`evidence/qa-gates/final-mstest-coverage.md`), P7-T6 (`evidence/qa-gates/coverage-firstparty.postexemption.cobertura.xml`).
  - Delta/threshold comparison: P7-T8 (`evidence/qa-gates/coverage-delta.md`).
- Exemption boundary verification: P7-T7 (`evidence/qa-gates/exemption-boundary-verification.md`).
- Toolchain gates: per-phase loops (P1-T5, P2-T3, P3-T4, P4-T3, P5-T3, P6-T2) and final loop (P7-T1..P7-T4).
- Revision 1.1 (TaskVisualization class-level treatment):
  - Config reversal verification: P8-T1, P8-T2, P10-T6.
  - Preserved-seam confirmation: P9-T5 (`evidence/other/taskvis-preserved-seams.md`), re-verified by P10-T7.
  - Assess-by-inspection determination: P9-T4 (`evidence/other/taskvis-inspection-assessment.md`).
  - Revision coverage artifacts: P10-T4 (`evidence/qa-gates/final-r2-mstest-coverage.md`), P10-T6 (`evidence/qa-gates/coverage-firstparty.r2-classlevel.cobertura.xml`), delta in P10-T8 (`evidence/qa-gates/coverage-delta-r2.md`).
  - Revision toolchain gates: per-phase loops (P8-T4, P9-T6) and final loop (P10-T1..P10-T4).

## Out of Scope (per spec §Non-Goals)

- Roadmap increment tests (design memo Phases 4-8) that raise covered code toward 80%. Not planned here.
- Vendored assemblies (`Swordfish.NET.General`, `SVGControl`) exemption decision.
- Koverage allowlist/denominator tiering (memo Option C).
- Any production-logic refactor or helper-class extraction.

## Rollback / Contingency

All changes are non-behavioral (attributes, `using` directives, two XML config excludes, two policy-doc additions). Rollback is a `git revert` of the feature branch; no data migration or downstream consumer impact. If P7-T5 detects a test-result regression, the offending annotation batch is identified by the per-phase MSTest artifacts and reverted before re-running the toolchain.

## Open Questions / Notes

- `Tags` contains two `CheckBoxController.cs` files; P6-T1 instructs the executor to annotate only the WinForms CheckBox event-handler class per memo §2.5's description.
- The spec's `evidence/coverage/` reference is normalized to canonical `evidence/baseline/` and `evidence/qa-gates/` per the evidence-location invariant.
- `RibbonController`/`AppItemEngines` are annotated whole-class per memo §2.2 (helper-extraction is optional and out of scope for this feature).
- Revision 1.1: the original P1-T1/P1-T2 added an assembly-level `TaskVisualization` exclude; the maintainer directive reverses this (P8-T1/P8-T2) in favor of class-level `[ExcludeFromCodeCoverage]` (Phase 9). The completed P1-T1/P1-T2 checkboxes record the original execution history and are NOT mutated; their effect on TaskVisualization is superseded by Phase 8.
- Revision 1.1: `TaskVisualization\FlagChangeGroup.cs` and `TaskVisualization\EditFilterController.cs` both `using Microsoft.Office.Interop.Outlook` (and `EditFilterController` also `using System.Windows.Forms`), but a `using` directive alone is not determinative of full COM/WinForms binding; P9-T4 requires per-member inspection before annotating, and any testable pure-logic seam must remain measured.
- Revision 1.1: `spec.md` must be updated to reflect the class-level treatment of TaskVisualization; this is task P8-T3 in the revised plan.
