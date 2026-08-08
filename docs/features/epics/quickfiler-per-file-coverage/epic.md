---
epic: quickfiler-per-file-coverage
integration_branch: epic/quickfiler-per-file-coverage-integration
created_at: 2026-08-07T21-10
intent:
  epic_type: enabler
  business_outcome_hypothesis: >-
    Bringing every testable production file compiled by QuickFiler.csproj to at least 80% line
    coverage, with an authoritative ratified exemption ledger covering the irreducible
    WinForms/COM/designer remainder, reduces regression escapes in QuickFiler and makes the
    project safe for autonomous agentic maintenance.
  leading_indicators:
    - Per-file line coverage for every non-exempt file compiled by QuickFiler.csproj reaches >= 80%.
    - The count of QuickFiler files carrying [ExcludeFromCodeCoverage] on a testable seam falls to zero.
    - Repository-wide line coverage is retained or improved at each child merge.
  nfrs:
    - No behavior change to end-user QuickFiler flows; testability refactors preserve observable behavior.
    - Tests remain deterministic, isolated, and free of temporary files, live forms, and external services.
    - No production file exceeds 500 lines after refactor.
    - Full C# toolchain (csharpier, analyzers, nullable, MSTest with coverage) green for every child.
features:
  - issue_num: 1001
    feature_folder: quickfiler-coverage-denominator-and-exemption-ledger
    depends_on: []
  - issue_num: 431
    feature_folder: quickfiler-queue-admission-coverage
    depends_on: [1001]
  - issue_num: 430
    feature_folder: 2026-08-07-quickfiler-keyboard-actions-coverage-430
    depends_on: [1001]
  - issue_num: 1004
    feature_folder: quickfiler-helper-classes-coverage
    depends_on: [1001]
  - issue_num: 1005
    feature_folder: quickfiler-datamodel-coverage
    depends_on: [1001]
  - issue_num: 1006
    feature_folder: quickfiler-qfc-form-explorer-controller-coverage
    depends_on: [1001]
  - issue_num: 433
    feature_folder: 2026-08-07-quickfiler-qfc-home-controller-coverage-433
    depends_on: [1001]
  - issue_num: 437
    feature_folder: 2026-08-07-quickfiler-efc-home-controller-coverage-437
    depends_on: [1001]
  - issue_num: 1009
    feature_folder: quickfiler-efc-form-item-controller-coverage
    depends_on: [1001]
  - issue_num: 1010
    feature_folder: quickfiler-item-controller-coverage
    depends_on: [1001]
  - issue_num: 1011
    feature_folder: quickfiler-collection-controller-coverage
    depends_on: [1001]
  - issue_num: 1012
    feature_folder: quickfiler-breadcrumb-bridge-coverage
    depends_on: [1001]
  - issue_num: 1013
    feature_folder: quickfiler-breadcrumb-dropdown-webview-coverage
    depends_on: [1001]
  - issue_num: 1014
    feature_folder: quickfiler-itemviewer-coverage
    depends_on: [1001]
  - issue_num: 1015
    feature_folder: quickfiler-form-viewers-bayesian-coverage
    depends_on: [1001]
  - issue_num: 1016
    feature_folder: quickfiler-per-file-coverage-capstone
    depends_on:
      - 431
      - 430
      - 1004
      - 1005
      - 1006
      - 433
      - 437
      - 1009
      - 1010
      - 1011
      - 1012
      - 1013
      - 1014
      - 1015
---

# Epic: QuickFiler Per-File 80% Coverage (#136)

- Epic issue: https://github.com/drmoisan/TaskMaster/issues/136
- Integration branch: `epic/quickfiler-per-file-coverage-integration`
- Status: Planning phase — child preparation in progress.

> `issue_num` values `1001`-`1016` are placeholders assigned at manifest-authoring time. Each is
> back-filled with the real GitHub issue number from the child's promotion receipt as its
> preparation completes. The manifest is committed in final resolved form before the kickoff
> artifact is written.

## Goal

Bring every production `.cs` file compiled by `QuickFiler/QuickFiler.csproj` to at least 80% line
coverage, or onto an explicitly ratified exemption ledger, while retaining or improving
repository-wide coverage. Issue #136 mandates that research and planning proceed one production
file at a time; this epic satisfies that mandate by giving each child feature a per-file research
artifact and a per-file atomic-plan phase, with each individual test case planned as an atomic
task.

## Scope

The compiled surface is 121 files. `QuickFiler/Legacy/**` and `QuickFiler/Notes/**` are present in
the working tree but are **not** listed as `<Compile Include=...>` in `QuickFiler.csproj`, so they
are outside the coverage denominator and outside this epic. Of the 121 compiled files:

- ~24 are interface-only declarations with no executable behavior.
- 7 are WinForms `*.Designer.cs` generated files (including `ItemViewer.Designer.cs` at 6,224 lines
  and `EfcViewer.Designer.cs` at 4,276 lines).
- 3 are generated `Properties/` files (`AssemblyInfo.cs`, `Resources.Designer.cs`,
  `Settings.Designer.cs`).
- 21 currently carry a real `[ExcludeFromCodeCoverage]` attribute, including
  `QfcCollectionController.cs` (2,349 lines), `EfcItemController.cs` (1,170), and
  `EfcFormController.cs` (1,086).

> **Marker accuracy note (corrected 2026-08-07 during F2 preparation).** An initial survey
> reported 33 exempted files. That figure was inflated two ways: a plain `grep` for
> `ExcludeFromCodeCoverage` also matched XML doc-comment *references* to the attribute, and it
> matched files that are not in the csproj compile set. The accurate figure is **21 compiled files
> carrying a real attribute**. Five files mention the attribute only in a doc comment and are NOT
> exempt: `Controllers/QfcScanProgressBandMapper.cs`, `Viewers/ItemViewer.Commands.cs`,
> `Viewers/ItemViewer.DisplayState.cs`, `Viewers/ItemViewer.FolderSearch.cs`, and
> `Viewers/ItemViewer.WebViewThread.cs`. The `[X]` markers below have been corrected accordingly.
> Children must verify a marker against the file before acting on it; F1's ledger is the
> authoritative record.

That leaves roughly 87 files in the testable denominator, several of which need seam extraction
before any deterministic unit test can reach them.

## Measured Coverage Baseline (added 2026-08-07, indicative)

Preparation of F8 discovered that committed Cobertura reports already exist in the repository, so
the epic does not have to guess at its starting point. The most recent QuickFiler-wide report is
`docs/features/active/2026-08-06-quickfiler-high-confidence-queue-init-stall-424/evidence/qa-gates/coverage-final.cobertura.xml`
(feature #424); feature #400 has a further set under its own `evidence/` tree.

**This baseline is indicative, not authoritative.** It was captured on another feature's branch, so
it does not reflect the integration branch exactly. F1's harness, run on each child's own branch,
remains the authority. Children must still measure; they must not cite these numbers as their
acceptance evidence.

Two facts from that report change how children should plan:

**1. QuickFiler is far better covered than the file inventory suggests.** Only 22 of the 70
instrumented files sit below the 80% line floor. Many are already at or near 100%. The epic is
therefore mostly a *gap-closure and exemption-removal* exercise, not a build-from-zero effort. Any
child whose research assumes its files are untested is working from a false premise — verify first.
Measured below 80% line coverage:

| File | Lines | Line % | Child |
| --- | --- | --- | --- |
| `Helper Classes/EfcThemeHelper.cs` | 872 | 0.0% | F4 |
| `Properties/Settings.Designer.cs` | 8 | 0.0% | F15 (exempt-candidate) |
| `Controllers/QfcFormController.Actions.cs` | 289 | 37.0% | F6 |
| `Viewers/ItemViewerExpanded.cs` | 205 | 39.0% | F14 |
| `Controllers/FilerQueue.cs` | 69 | 40.6% | F2 |
| `Controllers/QfcFormController.EventHandlers.cs` | 298 | 43.3% | F6 |
| `Controllers/QfcQueue.cs` | 504 | 46.2% | F2 |
| `Helper Classes/EmailMoveMonitor.cs` | 208 | 50.0% | F4 |
| `Viewers/BayesianPerformanceViewer.cs` | 70 | 54.3% | F15 |
| `Controllers/EfcDataModel.cs` | 356 | 55.6% | F5 |
| `Viewers/ToolStripMenuItemCb.cs` | 78 | 61.5% | F15 |
| `Controllers/QfcHomeController.Metrics.cs` | 212 | 65.1% | F7 |
| `Controllers/QfcFormController.SetupDisposal.cs` | 307 | 70.7% | F6 |
| `Controllers/QfcHomeController.cs` | 395 | 71.4% | F7 |
| `Helper Classes/ConversationResolver.Loading.cs` | 298 | 71.8% | F4 |
| `Controllers/BayesianPerformanceController.cs` | 173 | 72.3% | F15 |
| `Viewers/ToolStripMenuItemCb.Designer.cs` | 22 | 72.7% | F15 (exempt-candidate) |
| `Controllers/QfcItemController.ViewerSetup.cs` | 277 | 74.4% | F10 |
| `Controllers/QfcFormController.cs` | 180 | 75.6% | F6 |
| `Controllers/QfcItemController.FocusAndTheme.cs` | 373 | 75.6% | F10 |
| `Controllers/QfcItemController.MailActions.cs` | 189 | 77.8% | F10 |
| `Controllers/QfcItemController.EventHandlers.cs` | 187 | 79.7% | F10 |

**2. The exempted files are invisible, and that is where the bulk of the work is.** Roughly 51
compiled files do not appear in the report at all, because `[ExcludeFromCodeCoverage]` removes them
from instrumentation entirely (interface-only files are also absent, legitimately). Every file whose
exemption F1's ledger orders removed will appear for the first time at an unknown coverage level,
most likely near zero. `QfcCollectionController.cs` (F11), `EfcFormController.cs` and
`EfcItemController.cs` (F9), `QfcDatamodel.cs` (F5), `KeyboardHandler.cs` (F3),
`QfcExplorerController.cs` (F6), `ItemViewer.cs` (F14), and the WebView2 trio (F13) are all in this
category. **An absent file is not a covered file.**

**3. Branch coverage is a separate, unmet gate.** `.claude/rules/general-unit-test.md` sets a 75%
branch floor alongside the 85% line figure, and issue #136's own target is 80% line. F8 found
`EfcHomeController.Timing.cs` at 100% line but **66.67% branch** — passing the line gate and failing
the branch gate. Children must report both, and F1's harness must emit both.

## Non-Goals

- No behavior change to end-user QuickFiler flows.
- No coverage work on `QuickFiler/Legacy/**` or `QuickFiler/Notes/**` (not compiled).
- No conversion of QuickFiler away from VSTO/WinForms; that is the separate long-term migration
  effort. Where a seam choice is open, prefer host-neutral extraction that a future
  WebView2/Office.js port can reuse.
- No change to the repository-wide coverage thresholds themselves.

## Shared Design (applies to every child)

### 1. Policy reconciliation — the load-bearing epic-level decision

Two repository policies meet head-on in this project and the reconciliation is settled once here,
in child F1, rather than 15 times independently:

- `CLAUDE.md` § UT2 ratifies a **COM/VSTO/WinForms coverage exemption** with a "testable
  denominator", naming QuickFiler explicitly: Outlook Interop event-handler classes that directly
  depend on `Microsoft.Office.Interop.Outlook.Application`, `MailItem`, `Store`, or `MAPIFolder`
  **without an injectable seam**, plus WinForms form-derived and Designer-generated code.
- `.claude/rules/general-unit-test.md` § Coverage Exclusion Policy states that no production file
  may be excluded from coverage measurement, and that the correct response to untestable lines is
  to refactor — extracting logic into host-neutral testable modules and leaving only the thinnest
  possible wiring in the host-bound entry point.

These are reconciled the same way the `winforms-testability-refactor` epic reconciled them
(precedent, ratified 2026-07-09): **refactor first, exempt only the irreducible remainder.** The
qualifier "without an injectable seam" in the CLAUDE.md exemption is read as a live obligation, not
a standing permission — if a seam can be introduced, the exemption does not apply and the file must
be covered. `[ExcludeFromCodeCoverage]` on a *testable* seam is a Blocking finding. The 33 existing
attributes are treated as unratified until F1's ledger either justifies each one against the
irreducible-remainder test or marks it for removal by the owning child.

### 2. Seam hierarchy

Per `.claude/rules/csharp.md`: interface seam > injectable delegate > adapter. Unit tests must never
construct live forms, never show popups (a popup requiring human interaction is a unit-test-policy
violation), and never depend on the UI thread. Running COM elements on the UI thread is a
production-only last resort, never in tests.

### 3. STA last-resort clause (inherited precedent)

In-memory, never-shown WinForms **controls** (`TableLayoutPanel`, `Label`, `Panel`, `CheckBox`) MAY
be constructed in unit tests on an STA thread, strictly as a LAST RESORT where no seam can isolate
the logic. Conditions: (a) seams remain the required first approach and each STA-bound test
documents why no seam is feasible; (b) all STA-bound tests live in dedicated `*.StaTests.cs` files
with `[STATestClass]`/`[STATestMethod]` or equivalent runsettings scoping, so the STA surface stays
minimal.

### 4. Test conventions

MSTest (`[TestClass]`/`[TestMethod]`), Moq for mocks/stubs, FluentAssertions for new assertions.
Arrange–Act–Assert. Deterministic: injected clock, seeded RNG, no `Thread.Sleep`/`Task.Delay`, no
temporary files, no external services. Tests live in `QuickFiler.Test/` mirroring the production
tree.

### 5. File-size compliance

No production file may exceed 500 lines after refactor. Files currently over the limit that this
epic touches: `QfcCollectionController.cs` (2,349), `EfcItemController.cs` (1,170),
`EfcFormController.cs` (1,086), `QfcQueue.cs` (610). Generated `*.Designer.cs` files are exempt from
the 500-line rule as generated code and are on the coverage exemption ledger.

### 6. Per-file coverage measurement

F1 delivers the repeatable per-file line-coverage report derived from the Cobertura output of
`Invoke-MSTestWithCoverage.ps1`. Every child verifies its own files with that harness and commits
the numeric per-file result as coverage evidence under
`<FEATURE>/evidence/qa-gates/`. Aggregate assembly coverage alone does not satisfy any child's
acceptance criteria — issue #136 measures success per production file.

## Decomposition Rationale

Children are cut along cohesive architectural clusters so that each child's production file set is
**disjoint** from every sibling's. Disjointness is what makes fan-in a clean merge and lets wave 1
run fully parallel. Two consequences shaped the cuts:

- **Partial-class families stay together.** `QfcItemController.*` (10 files, 3,073 lines) and
  `QfcFormController.*` (4 files) are partials of a single type. Splitting a partial family across
  children would put two children in the same type and the same test fixture, guaranteeing
  conflicts. Each family is therefore one child even where that makes the child large.
- **A single 2,349-line file is its own child.** `QfcCollectionController.cs` needs a 500-line
  partial split plus seam extraction plus coverage. Bundling it with anything else would exceed a
  single feature's practical change budget.

`quickfiler-form-viewers-bayesian-coverage` (F15) is the smallest child and absorbs the generated
`Properties/` files and the small designer-backed viewers, so no compiled file is left unassigned.

### Why F1 is a real dependency, not stylistic ordering

Every wave-1 child depends on F1 and on nothing else. That edge is a genuine upstream contract:

1. F1 fixes the **denominator**. Until each file is classified testable vs ratified-exempt, a
   child cannot state its own acceptance criteria — it would not know whether
   `ItemViewer.Designer.cs` is in its target set.
2. F1 delivers the **measurement harness** every child needs to produce per-file coverage evidence.
   Fifteen children independently building per-file coverage reporting would produce fifteen
   inconsistent numbers and a capstone that cannot close.
3. F1 settles the **policy reconciliation and seam conventions** above. Without it, children would
   each decide independently whether to remove or keep `[ExcludeFromCodeCoverage]`, and would
   collide on the shared `coverage.config`.

No sibling edges exist among F2–F15 because their file sets are disjoint and none consumes another's
production contract. The capstone F16 depends on all fourteen.

## Wave Assignment

Computed by longest-path layering: `wave(f) = 0` when `depends_on` is empty, else
`1 + max(wave(d))`. Verified cycle-free; every `depends_on` entry resolves to a feature in the set.

| Wave | Features | Count |
| --- | --- | --- |
| 0 | F1 denominator-and-exemption-ledger | 1 |
| 1 | F2 queue-admission, F3 keyboard-actions, F4 helper-classes, F5 datamodel, F6 qfc-form-explorer-controller, F7 qfc-home-controller, F8 efc-home-controller, F9 efc-form-item-controller, F10 item-controller, F11 collection-controller, F12 breadcrumb-bridge, F13 breadcrumb-dropdown-webview, F14 itemviewer, F15 form-viewers-bayesian | 14 |
| 2 | F16 capstone | 1 |

Wave 1 has 14 members against a `max_parallel_features` cap of 8, so execution proceeds in two
batches.

## Feature File Assignments

Every one of the 121 compiled files is assigned to exactly one child. Line counts are as of
`origin/main` at 74be1964. `[X]` marks a file currently carrying `[ExcludeFromCodeCoverage]`.

### F1 — quickfiler-coverage-denominator-and-exemption-ledger (wave 0, C3)

No production behavior change. Deliverables: the per-file classification ledger for all 121
compiled files (testable vs ratified-exempt, with a rationale per exempt file tested against the
irreducible-remainder standard); the repeatable per-file coverage report harness; the shared
seam/STA conventions and the policy reconciliation recorded above; and disposition instructions for
each of the 33 existing `[ExcludeFromCodeCoverage]` attributes assigned to its owning child.

### F2 — quickfiler-queue-admission-coverage (wave 1, C2)

`Controllers/QfcQueue.cs` (610), `Controllers/FilerQueue.cs` (83),
`Controllers/QfcRemainingQueueAdmission.cs` (48),
`Controllers/QfcStreamingDequeueConfidenceGate.cs` (171),
`Controllers/QfcHighConfidencePreFilter.cs` (191) `[X]`,
`Controllers/QfcScanProgressBandMapper.cs` (79),
`Controllers/BreadcrumbOutboundQueue.cs` (67), `Controllers/EmailSorter.cs` (85),
`Controllers/QfcItemGroup.cs` (52), `Controllers/IQfcQueue.cs` (41),
`Controllers/IQfcQueue1.cs` (44). ~1,471 lines / 11 files. Predominantly pure logic with existing
partial coverage; `QfcQueue.cs` also needs a 500-line split.

### F3 — quickfiler-keyboard-actions-coverage (wave 1, C3)

`Controllers/KaChar.cs` (99), `Controllers/KaKey.cs` (99), `Controllers/KaStringAsync.cs` (95),
`Controllers/KbdActions.cs` (146), `Controllers/KeyboardHandler.cs` (414) `[X]`,
`Controllers/QfcFormKeyHandler.cs` (20), `Interfaces/IKbdAction.cs` (18),
`Interfaces/IQfcKeyboardHandler.cs` (37), `Interfaces/IMailItemActions.cs` (35),
`Interfaces/MailItemActionsAdapter.cs` (47), `Interfaces/IItemControler.cs` (15). ~1,025 lines / 11
files. `KeyboardHandler.cs` needs seams to drop its exemption.

### F4 — quickfiler-helper-classes-coverage (wave 1, C3)

All 13 files under `QuickFiler/Helper Classes/`: `cInfoMail.cs` (231),
`ConversationResolver.cs` (358), `ConversationResolver.Loading.cs` (329),
`EfcThemeHelper.cs` (499), `EfcViewerQueue.cs` (101), `EmailMoveMonitor.cs` (262),
`IConversationResolver.cs` (33), `ItemViewerQueue.cs` (123), `QfcThemeControlSet.cs` (110),
`QfcThemeHelper.cs` (375), `QfEnums.cs` (16), `TlpCellSnapShot.cs` (223),
`ViewerQueueCore.cs` (161), plus `Interfaces/IEmailMoveMonitor.cs` (39). ~2,860 lines / 14 files.

### F5 — quickfiler-datamodel-coverage (wave 1, C3)

`Controllers/QfcDatamodel.cs` (496) `[X]`, `Controllers/QfcDatamodel.FrameBuilding.cs` (154),
`Controllers/QfcDatamodel.QueueProcessing.cs` (177), `Controllers/EfcDataModel.cs` (397),
`Interfaces/IQfcDatamodel.cs` (59). ~1,283 lines / 5 files.

### F6 — quickfiler-qfc-form-explorer-controller-coverage (wave 1, C3)

`Controllers/QfcFormController.cs` (196), `Controllers/QfcFormController.SetupDisposal.cs` (232),
`Controllers/QfcFormController.EventHandlers.cs` (399),
`Controllers/QfcFormController.Actions.cs` (302), `Controllers/QfcExplorerController.cs` (323) `[X]`,
`Controllers/IQfcFormController.cs` (43), `Interfaces/IQfcFormController.cs` (25),
`Interfaces/IQfcExplorerController.cs` (15), `Interfaces/IQfcFormViewer.cs` (51),
`Interfaces/IFilerFormController.cs` (25). ~1,611 lines / 10 files.

### F7 — quickfiler-qfc-home-controller-coverage (wave 1, C3)

`Controllers/QfcHomeController.cs` (487), `Controllers/QfcHomeController.Metrics.cs` (234),
`Controllers/QfcHomeController.Iteration.cs` (86), `Controllers/IQfcHomeController.cs` (20),
`Interfaces/IFilerHomeController.cs` (45). ~872 lines / 5 files. Coordinate with in-flight issue
#424 (see Known Conflict Risks).

### F8 — quickfiler-efc-home-controller-coverage (wave 1, C3)

`Controllers/EfcHomeController.cs` (441), `Controllers/EfcHomeController.Metrics.cs` (87),
`Controllers/EfcHomeController.ExecuteMoves.cs` (144),
`Controllers/EfcHomeController.Timing.cs` (43),
`Controllers/EfcHomeControllerDependencies.cs` (428),
`Controllers/EfcHomeControllerDependencyFactories.cs` (268). ~1,411 lines / 6 files.

### F9 — quickfiler-efc-form-item-controller-coverage (wave 1, C3)

`Controllers/EfcFormController.cs` (1,086) `[X]`, `Controllers/EfcItemController.cs` (1,170) `[X]`,
`Viewers/EfcViewer.cs` (162) `[X]`, `Viewers/EfcViewer.Designer.cs` (4,276, exempt-candidate).
~2,418 testable lines / 4 files. Heaviest seam-extraction child; both controllers also breach the
500-line rule.

### F10 — quickfiler-item-controller-coverage (wave 1, C3)

`Controllers/QfcItemController.cs` (323), `.Initialization.cs` (466) `[X]`,
`.ViewerSetup.cs` (426) `[X]`, `.Conversation.cs` (235) `[X]`, `.FolderHandling.cs` (235),
`.EventWiring.cs` (391) `[X]`, `.EventHandlers.cs` (219) `[X]`, `.Navigation.cs` (228) `[X]`,
`.FocusAndTheme.cs` (326), `.MailActions.cs` (224), `Interfaces/IQfcItemController.cs` (107).
~3,180 lines / 11 files. One partial-class family; six of ten partials are currently exempted.

### F11 — quickfiler-collection-controller-coverage (wave 1, C3)

`Controllers/QfcCollectionController.cs` (2,349) `[X]`,
`Interfaces/IQfcCollectionController.cs` (118). 2,467 lines / 2 files. Requires a 500-line partial
split, seam extraction, then coverage.

### F12 — quickfiler-breadcrumb-bridge-coverage (wave 1, C3)

`Controllers/BreadcrumbBridgeRouter.cs` (450), `Viewers/BreadcrumbBridgeCoordinator.cs` (487),
`Viewers/BreadcrumbCoordinatorUpgradeLifetime.cs` (309),
`Viewers/BreadcrumbItemViewerLifecycleCoordinator.cs` (481),
`Viewers/BreadcrumbMessengerHub.cs` (456). ~2,183 lines / 5 files.

### F13 — quickfiler-breadcrumb-dropdown-webview-coverage (wave 1, C3)

`Viewers/BreadcrumbDropDownHost.cs` (480), `Viewers/BreadcrumbDropDownOpenLifetime.cs` (477),
`Viewers/BreadcrumbDropDownOpenCoordinator.cs` (309), `Viewers/BreadcrumbPopupPlacement.cs` (87),
`Viewers/BreadcrumbPopupUiOperations.cs` (494) `[X]`,
`Viewers/BreadcrumbCollapsedSurfaceController.cs` (308), `Viewers/BreadcrumbUiDispatcher.cs` (285),
`Viewers/BreadcrumbWebViewSurfaceFactory.cs` (225), `Viewers/IBreadcrumbDropDownHost.cs` (42),
`Viewers/IBreadcrumbWebHost.cs` (27), `Viewers/WebView2BreadcrumbHost.cs` (143) `[X]`,
`Viewers/WebView2CoreInitializer.cs` (30) `[X]`, `Viewers/WebView2Messenger.cs` (147) `[X]`,
`Viewers/IWebViewCoreInitializer.cs` (30), `Viewers/IWebViewMessenger.cs` (27). ~3,111 lines / 15
files. Coordinate with in-flight issue #400 (see Known Conflict Risks).

### F14 — quickfiler-itemviewer-coverage (wave 1, C3)

`Viewers/ItemViewer.cs` (432) `[X]`, `.DisplayState.cs` (81), `.Commands.cs` (109),
`.Breadcrumb.cs` (298), `.FolderSearch.cs` (74), `.WebViewThread.cs` (37),
`Viewers/ItemViewerExpanded.cs` (181), `Viewers/IItemViewer.cs` (133), plus exempt-candidates
`Viewers/ItemViewer.Designer.cs` (6,224) and `Viewers/ItemViewerExpanded.Designer.cs` (821).
~1,345 testable lines / 10 files. Form-derived; the STA last-resort clause is most likely to apply
here.

### F15 — quickfiler-form-viewers-bayesian-coverage (wave 1, C2)

`Viewers/QfcFormViewer.cs` (262) `[X]`, `Viewers/QfcItemViewerExpanded.cs` (63) `[X]`,
`Viewers/BayesianPerformanceViewer.cs` (67), `Controllers/BayesianPerformanceController.cs` (156),
`Viewers/ToolStripMenuItemCb.cs` (87), plus exempt-candidates
`Viewers/QfcFormViewer.Designer.cs` (257), `Viewers/QfcItemViewerExpanded.Designer.cs` (942),
`Viewers/BayesianPerformanceViewer.Designer.cs` (498),
`Viewers/ToolStripMenuItemCb.Designer.cs` (40), `Properties/AssemblyInfo.cs` (38),
`Properties/Resources.Designer.cs` (432), `Properties/Settings.Designer.cs` (107). ~635 testable
lines / 12 files.

### F16 — quickfiler-per-file-coverage-capstone (wave 2, C3)

No new production files. Verifies that every one of the 121 compiled files is either at >= 80% line
coverage or on the ratified exemption ledger; runs the full C# toolchain in order; confirms
repository-wide coverage is retained or improved; and closes each acceptance criterion of issue
#136 with numeric evidence.

## Complexity Assessment

Bands use the `model_policy` scale in `config/orchestration-routing.json`.

| Feature | Band | Rationale |
| --- | --- | --- |
| F1 | C3 | Defines a cross-cutting measurement and exemption contract consumed by all 15 siblings; reconciles two conflicting repository policies. `cross_module_contract_change` signal present. |
| F2 | C2 | Localized to queue/admission logic that is already largely pure and partially covered; no contract change. |
| F3 | C3 | `KeyboardHandler.cs` seam extraction changes an internal contract consumed by form and item controllers. |
| F4 | C3 | 14 files spanning theme, conversation-resolution, and viewer-queue concerns; `ConversationResolver` touches Outlook Interop and ordering behavior. |
| F5 | C3 | Datamodel seam work alters contracts consumed by home and collection controllers; `concurrency_or_ordering` signal in queue processing. |
| F6 | C3 | Partial-class family with event-handler and disposal paths requiring seams across the form/viewer boundary. |
| F7 | C3 | Home-controller iteration and metrics carry ordering/state-transition invariants. |
| F8 | C3 | Dependency-factory seams form a contract consumed across the EFC controller family. |
| F9 | C3 | Two 1,000+ line COM-bound controllers requiring both a 500-line split and new injectable seams. |
| F10 | C3 | 11-file partial family, six partials currently exempted; event wiring and navigation carry state-transition invariants. |
| F11 | C3 | Single 2,349-line file requiring partial split plus seam extraction before any coverage is reachable. |
| F12 | C3 | Bridge/messenger lifecycle carries concurrency and ordering invariants across the WebView boundary. |
| F13 | C3 | Drop-down open/close lifetime and WebView2 host initialization carry concurrency and ordering invariants. |
| F14 | C3 | Form-derived viewer partials; STA last-resort determination and seam extraction across the WebView thread boundary. |
| F15 | C2 | Small testable surface; the bulk is generated designer code resolved by ledger classification rather than by refactor. |
| F16 | C3 | Repository-wide verification gate closing all eight acceptance criteria with numeric per-file evidence. |

## Coverage-Target Reconciliation (authoritative for this epic)

Three documents state different coverage numbers, and F7's research established that the
repository baseline sits below all of them: issue #424's evidence recorded a **merge-base
repository line rate of 70.19%**. No child can satisfy an absolute repository-wide floor on its
own, so the targets are reconciled here once. This is a reconciliation of which number gates
which scope — **not a waiver of any policy**.

| Scope | Target | Source |
| --- | --- | --- |
| Per production file, line | **>= 80%** | Issue #136 AC1 — the operative acceptance bar for this epic |
| Per production file, branch | **>= 75%** | `.claude/rules/general-unit-test.md` — the only branch figure stated anywhere |
| Files newly created by this epic, line | **>= 90%** | `CLAUDE.md` §UT2 new-module rule |
| Changed lines | **No regression** | `CLAUDE.md` §UT2 and `.claude/rules/general-unit-test.md` |
| Repository-wide | **Retain or improve against the measured baseline** | Issue #136 AC8 |

The repository-wide row is the one that needed a decision. Issue #136's own acceptance criterion
reads "Repository-wide coverage expectations are **retained or improved**" — retained, not met. The
absolute repository-wide floors in `CLAUDE.md` (80%) and `.claude/rules/general-unit-test.md` (85%)
remain the standing repository aspiration and are untouched by this epic; they are simply not the
per-child gate here, because the baseline was already below them before this epic began and gating
every child on a pre-existing shortfall would make the epic unexecutable. Each child measures
repository-wide coverage before and after and must not reduce it.

Note that the 80% per-file line figure and the 75% branch figure are independent gates. F8 found
`EfcHomeController.Timing.cs` at 100% line and 66.67% branch — passing one and failing the other.
Report both.

## Mid-Wave File Creation and the Ledger Denominator

F7 identified a gap that would otherwise surface only as a capstone failure. F1 authors its ledger
against the 121 files compiled today, but several children **create new production files** during
execution, after the ledger exists:

- F2 — `QfcQueue.cs` (610 lines) partial split
- F3 — two K1 seam files
- F7 — `QfcHomeController.Properties.cs` (the split is mandatory, not optional: the file is at 487 of
  a 500-line limit and the minimum seam set projects it to ~502)
- F9, F11 — `EfcFormController.cs`, `EfcItemController.cs`, `QfcCollectionController.cs` splits

Rules that close the gap:

1. **The denominator is dynamic.** The coverage denominator is the set of `<Compile Include=...>`
   entries in `QuickFiler/QuickFiler.csproj` **at evaluation time** — never a frozen 121-file list.
2. **The ledger carries rules, not just rows.** F1's ledger must state the classification rules
   (the three exemption grounds) so a file that did not exist at authoring time can still be
   classified without re-running F1.
3. **Creating child appends its own row.** Any child that adds a production file appends a ledger
   row for it in the same change that adds the `<Compile Include>` entry. Like the csproj, the
   ledger is therefore an additive shared file; fan-in conflicts on it are expected and resolved by
   keeping both sides.
4. **New files default to `testable` at >= 90%.** A file extracted from existing code is new
   production code and takes the `CLAUDE.md` new-module target. Claiming `ratified-exempt` for a
   newly created file requires a rationale meeting one of the three grounds.
5. **F16 re-derives and reconciles.** The capstone recomputes the denominator from the csproj and
   fails if any compiled file lacks a ledger row.

## Cross-Child Constraints Discovered During Preparation

Both constraints below were verified directly against the working tree during F3's preparation.
They apply to every child, not just F3.

### 1. `QuickFiler.csproj` is an unavoidable shared file

`QuickFiler/QuickFiler.csproj` is a legacy non-SDK project that uses **no globbing**: every source
file is listed as an explicit `<Compile Include=...>` entry. Any child that adds a new production
`.cs` file — a seam extraction, or a partial split to satisfy the 500-line rule — **must** edit that
one file. F2 (`QfcQueue.cs` at 610 lines), F3 (two K1 seam files), F9, and F11
(`QfcCollectionController.cs` at 2,349 lines) are all in this position.

This is a deliberate, accepted exception to the epic's shared-file prohibition: a project's own
`.csproj` is not a shared *build property* file, and there is no alternative — the code cannot
compile without the entry. Rules for every child:

- Edit `QuickFiler.csproj` **only** to add `<Compile Include=...>` entries for files that child owns.
  No property changes, no reference changes, no reordering of unrelated entries.
- Keep the edit to minimal adjacent hunks so concurrent children conflict on as few lines as
  possible.
- **Preserve CRLF.** The file is CRLF-terminated; a git-bash `sed -i` will strip it and produce a
  whole-file diff that is guaranteed to conflict. Use the Edit tool or `perl -0777` with explicit
  `\r\n`.

Expect merge conflicts on this file during execution fan-in. They are additive on both sides, so
the correct resolution is nearly always to keep both sets of entries. This is handled by the
child's own R1-R5 remediation loop per the `epic-orchestrate` skill; it is not a decomposition
defect and must not be treated as one.

### 2. `QuickFiler.Test` has no `InternalsVisibleTo` grant from `UtilitiesCS`

`UtilitiesCS/Properties/AssemblyInfo.cs` grants `InternalsVisibleTo` to `DynamicProxyGenAssembly2`,
`UtilitiesCS.Test`, and `ToDoModel.Test` — but **not** to `QuickFiler.Test`. Any `UtilitiesCS`
internal is therefore unreachable from a QuickFiler test. F3 hit this on `MyBox.DialogInvoker`: the
existing dialog seam cannot be used, so a test reaching `KeyboardHandler.cs:304` or `:350` without a
local seam would display a modal dialog — a unit-test-policy violation.

**Resolution for this epic: build a local seam in the child's own assignment; do not edit
`UtilitiesCS/Properties/AssemblyInfo.cs`.** That file is outside every child's file assignment, and
widening the internals grant to another assembly is an encapsulation change this epic has no mandate
to make. F3 set the precedent with its K1 dialog seam; later children hitting the same wall should
follow it rather than reaching into `UtilitiesCS` internals.

## Known Conflict Risks

Two QuickFiler features are active on `main` concurrently with this epic and touch files assigned to
wave-1 children:

- **#400** `2026-07-21-quickfiler-folder-selector-dropdown-400` — overlaps F13
  (breadcrumb drop-down) territory.
- **#424** `2026-08-06-quickfiler-high-confidence-queue-init-stall-424` — overlaps F7
  (`QfcHomeController`) and possibly F2 (high-confidence queue admission) territory.

Neither blocks planning. At execution time `epic-orchestrator` rebases the integration branch on
`main` before each wave, and any conflict is handled by the child's own R1–R5 remediation loop per
the `epic-orchestrate` skill. Children F2, F7, and F13 should read the current state of those two
feature folders during research so their plans account for whichever version has merged.
