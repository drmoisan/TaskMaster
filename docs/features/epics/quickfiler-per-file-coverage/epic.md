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
  - issue_num: 432
    feature_folder: 2026-08-07-quickfiler-coverage-ledger-432
    depends_on: []
  - issue_num: 431
    feature_folder: quickfiler-queue-admission-coverage
    depends_on: [432]
  - issue_num: 430
    feature_folder: 2026-08-07-quickfiler-keyboard-actions-coverage-430
    depends_on: [432]
  - issue_num: 434
    feature_folder: 2026-08-07-quickfiler-helper-classes-coverage-434
    depends_on: [432]
  - issue_num: 436
    feature_folder: 2026-08-07-quickfiler-datamodel-coverage-436
    depends_on: [432]
  - issue_num: 435
    feature_folder: 2026-08-07-quickfiler-qfc-form-explorer-controller-coverage-435
    depends_on: [432]
  - issue_num: 433
    feature_folder: 2026-08-07-quickfiler-qfc-home-controller-coverage-433
    depends_on: [432]
  - issue_num: 437
    feature_folder: 2026-08-07-quickfiler-efc-home-controller-coverage-437
    depends_on: [432]
  - issue_num: 452
    feature_folder: 2026-08-07-quickfiler-efc-form-item-controller-coverage-452
    depends_on: [432]
  - issue_num: 453
    feature_folder: 2026-08-07-quickfiler-item-controller-coverage-453
    depends_on: [432]
  - issue_num: 454
    feature_folder: 2026-08-07-quickfiler-collection-controller-coverage-454
    depends_on: [432]
  - issue_num: 1012
    feature_folder: quickfiler-breadcrumb-bridge-coverage
    depends_on: [432]
  - issue_num: 455
    feature_folder: 2026-08-07-quickfiler-breadcrumb-dropdown-webview-coverage-455
    depends_on: [432]
  - issue_num: 456
    feature_folder: 2026-08-07-quickfiler-itemviewer-coverage-456
    depends_on: [432]
  - issue_num: 496
    feature_folder: 2026-08-08-quickfiler-form-viewers-bayesian-coverage-496
    depends_on: [432]
  - issue_num: 1016
    feature_folder: quickfiler-per-file-coverage-capstone
    depends_on:
      - 431
      - 430
      - 434
      - 436
      - 435
      - 433
      - 437
      - 452
      - 453
      - 454
      - 1012
      - 455
      - 456
      - 496
---

# Epic: QuickFiler Per-File 80% Coverage (#136)

- Epic issue: https://github.com/drmoisan/TaskMaster/issues/136
- Integration branch: `epic/quickfiler-per-file-coverage-integration`
- Status: Planning phase — child preparation in progress.

> **Issue-number back-fill status (2026-08-08).** `issue_num` values were placeholders at
> manifest-authoring time and are replaced with the real GitHub issue number from each child's
> promotion receipt as its preparation completes. Twelve are now resolved — **432** (F1), **430**,
> **431**, **433**, **434**, **435**, **436**, **437**, **452**, **453**, **454**, **455**, **456**,
> **496**. Two remain placeholders — F12 (**#495**) and F16 (**#497**), whose issues are already
> promoted and whose children are in preparation and belong to children still in preparation. Every `depends_on` edge on the wave-0 enabler
> now points at the real **432**. The manifest is committed in final resolved form, with no
> placeholder remaining, before the kickoff artifact is written.

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

> **Exemption census — final ground truth (F1, 2026-08-08).** The original survey reported 33
> exempted files. That was inflated two ways: a plain `grep` also matched XML doc-comment
> *references* to the attribute, and it matched files outside the csproj compile set. A first
> correction gave 21 compiled files. F1's authoritative census refines this further:
>
> - **21 compiled files** carry a real attribute — the corrected file count stands.
> - **40 attribute usages** across them: **14 type-level** and **26 member-level**.
> - **24 files are fully suppressed**, because a type-level attribute on a partial type propagates
>   to every partial of that type. The suppressed-file count therefore exceeds the file count that
>   declares an attribute.
>
> The acceptance criterion requiring a disposition for every existing attribute is satisfied by
> **40 dispositions**, not 21. Five files mention the attribute only in a doc comment and are NOT
> exempt: `Controllers/QfcScanProgressBandMapper.cs`, `Viewers/ItemViewer.Commands.cs`,
> `Viewers/ItemViewer.DisplayState.cs`, `Viewers/ItemViewer.FolderSearch.cs`, and
> `Viewers/ItemViewer.WebViewThread.cs`. The `[X]` markers below reflect declaring files only, so a
> file with no marker may still be suppressed by inheritance. Children must verify against the file
> and against F1's ledger, which is the authoritative record. The 121-file count and the per-child
> assignment table were independently confirmed sound and are adopted verbatim.

That leaves roughly 87 files in the testable denominator, several of which need seam extraction
before any deterministic unit test can reach them.

## Measured Coverage Baseline (corrected 2026-08-08, indicative)

Committed Cobertura reports already exist in the repository, so the epic does not have to guess at
its starting point. The most recent QuickFiler-wide report is
`docs/features/active/2026-08-06-quickfiler-high-confidence-queue-init-stall-424/evidence/qa-gates/coverage-final.cobertura.xml`
(feature #424); feature #400 has a further set under its own `evidence/` tree.

> **This table was recomputed on 2026-08-08 and the earlier version was wrong.** The first pass
> counted lines with `class.iter('line')`, which unions the class-level `<lines>` block with each
> method's `<lines>` block and so double-counts every line appearing in both. F1's research
> independently found the same defect in the repository's own coverage scripts and filed **#441**.
> Concretely, `QfcQueue.cs` reported 504 lines where the true figure is 386. Because the duplicated
> method-level lines are better covered than the class-level remainder, **every original percentage
> was optimistic** — `EfcDataModel.cs` moved from 55.6% to 49.6%, `EmailMoveMonitor.cs` from 50.0%
> to 44.0%, `BayesianPerformanceController.cs` from 72.3% to 66.0%. The corrected method reads only
> the class-level `<lines>` block and unions classes sharing a filename with max-hits per line, per
> F4's requirement.

**This baseline is indicative, not authoritative.** It was captured on another feature's branch, so
it does not reflect the integration branch exactly. F1's harness, run on each child's own branch,
remains the authority. Children must still measure; they must not cite these numbers as acceptance
evidence.

Three facts from that report shape how children should plan.

**1. Twenty-two of the 70 instrumented files sit below the 80% line floor.** Many others are at or
near 100%, so the epic is mostly a gap-closure and exemption-removal exercise rather than a
build-from-zero effort. Any child whose research assumes its files are untested is working from a
false premise — verify first.

| File | Lines | Line % | Branch % | Child |
| --- | --- | --- | --- | --- |
| `Helper Classes/EfcThemeHelper.cs` | 440 | 0.0% | 0.0% | F4 |
| `Properties/Settings.Designer.cs` | 4 | 0.0% | n/a | F15 (exempt-candidate) |
| `Controllers/QfcFormController.Actions.cs` | 204 | 35.8% | 35.4% | F6 |
| `Controllers/FilerQueue.cs` | 49 | 36.7% | 50.0% | F2 |
| `Viewers/ItemViewerExpanded.cs` | 106 | 37.7% | 8.3% | F14 |
| `Controllers/QfcQueue.cs` | 386 | 40.7% | 47.6% | F2 |
| `Helper Classes/EmailMoveMonitor.cs` | 159 | 44.0% | 44.1% | F4 |
| `Controllers/QfcFormController.EventHandlers.cs` | 249 | 45.4% | 43.9% | F6 |
| `Controllers/EfcDataModel.cs` | 250 | 49.6% | 39.1% | F5 |
| `Viewers/BayesianPerformanceViewer.cs` | 35 | 54.3% | 12.5% | F15 |
| `Viewers/ToolStripMenuItemCb.cs` | 39 | 61.5% | 50.0% | F15 |
| `Controllers/QfcHomeController.Metrics.cs` | 139 | 63.3% | 54.5% | F7 |
| `Controllers/BayesianPerformanceController.cs` | 97 | 66.0% | 57.1% | F15 |
| `Controllers/QfcHomeController.cs` | 250 | 68.4% | 48.3% | F7 |
| `Helper Classes/ConversationResolver.Loading.cs` | 202 | 68.8% | 52.2% | F4 |
| `Controllers/QfcFormController.SetupDisposal.cs` | 155 | 70.3% | 58.8% | F6 |
| `Controllers/QfcItemController.ViewerSetup.cs` | 160 | 72.5% | 55.6% | F10 |
| `Viewers/ToolStripMenuItemCb.Designer.cs` | 11 | 72.7% | 75.0% | F15 (exempt-candidate) |
| `Controllers/QfcItemController.FocusAndTheme.cs` | 237 | 74.3% | 58.8% | F10 |
| `Controllers/QfcFormController.cs` | 91 | 74.7% | 34.6% | F6 |
| `Controllers/QfcItemController.MailActions.cs` | 125 | 76.8% | 72.7% | F10 |
| `Controllers/QfcItemController.EventHandlers.cs` | 93 | 79.6% | 65.0% | F10 |

**2. Branch coverage is the binding gate for twelve further files that already pass on line.** This
is the single most under-appreciated fact in the epic. A child that reads only the line column will
conclude it is finished and then fail final QC. Each of these meets the 80% line floor and misses
the 75% branch floor:

| File | Line % | Branch % | Child |
| --- | --- | --- | --- |
| `Controllers/EfcHomeController.Timing.cs` | 100.0% | 66.7% | F8 |
| `Helper Classes/QfcThemeControlSet.cs` | 100.0% | 53.3% | F4 |
| `Viewers/ItemViewerExpanded.Designer.cs` | 99.5% | 50.0% | F14 (exempt-candidate) |
| `Viewers/BayesianPerformanceViewer.Designer.cs` | 99.1% | 50.0% | F15 (exempt-candidate) |
| `Controllers/EmailSorter.cs` | 95.9% | 50.0% | F2 |
| `Helper Classes/QfcThemeHelper.cs` | 95.8% | 62.5% | F4 |
| `Controllers/QfcRemainingQueueAdmission.cs` | 92.0% | 60.0% | F2 |
| `Viewers/BreadcrumbItemViewerLifecycleCoordinator.cs` | 90.6% | 66.4% | F12 |
| `Controllers/QfcItemController.FolderHandling.cs` | 87.8% | 63.3% | F10 |
| `Helper Classes/ConversationResolver.cs` | 81.9% | 72.2% | F4 |
| `Controllers/QfcItemController.EventWiring.cs` | 81.5% | 65.0% | F10 |
| `Controllers/QfcHomeController.Iteration.cs` | 80.4% | 66.7% | F7 |

F12 is affected even though every one of its files clears the line floor — its
`BreadcrumbItemViewerLifecycleCoordinator.cs` sits at 66.4% branch across 146 branch points, so F12
is not the near-no-op the line figures alone suggested.

**3. The exempted files are invisible, and that is where the bulk of the work is.** Roughly 51
compiled files do not appear in the report at all, because `[ExcludeFromCodeCoverage]` removes them
from instrumentation entirely (interface-only files are also absent, legitimately). Every file whose
exemption F1's ledger orders removed will appear for the first time at an unknown coverage level,
most likely near zero. `QfcCollectionController.cs` (F11), `EfcFormController.cs` and
`EfcItemController.cs` (F9), `QfcDatamodel.cs` (F5), `KeyboardHandler.cs` (F3),
`QfcExplorerController.cs` (F6), `ItemViewer.cs` (F14), and the WebView2 trio (F13) are all in this
category. **An absent file is not a covered file.** Note also that an attribute on a partial *type*
suppresses every partial of that type, so absence can be inherited rather than declared.

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
be covered. `[ExcludeFromCodeCoverage]` on a *testable* seam is a Blocking finding.

### Correction: a prior maintainer ratification supersedes this epic's ledger

An earlier revision said the existing attributes were "treated as unratified until F1's ledger
judges them." **That was wrong, and F10 caught it.** `epic-planner` verified the correction against
GitHub directly:

- **Issue #227** (`Refactor: qfc-item-controller-testability`, now **closed**) already adjudicated
  the `QfcItemController` exemption boundary. Over five remediation cycles it was cut from 103
  members to 19, with the maintainer rejecting each intermediate count. **18 of F10's 19 attributes
  were ratified there.**
- **Issue #230** (still **open**) is titled *"Build a WinForms message-pump test seam
  (Application.Run() background thread) to unblock 9 QfcItemController orchestration members"* —
  the title alone corroborates that nine of those exemptions are a **deliberate, tracked deferral**,
  explicitly not a merge condition.

**Ruling: F1's ledger has no authority to overturn a maintainer decision.** The ledger records such
attributes with provenance `ratified-by-maintainer (#227)` and does not re-litigate them. This is
not a weakening of the epic's stance — the refactor-first rule still governs every attribute that
has *not* been through a maintainer adjudication, which is the large majority. It simply recognises
that the question was already asked and answered for one family, by the only authority that can
answer it.

Consequences:

- **Epic AC2, as originally worded, was unsatisfiable** for F10. The correct target is **19 → 15**,
  not 19 → 0: one genuinely unratified attribute is removed (`EnsureBreadcrumbPipeline` at
  `ViewerSetup.cs:132`, post-ratification drift that F10 isolated by diffing the ratified 18-member
  list against a live grep of 19 sites), and three more fall away with dead-code deletion.
- **No task may build the #230 seam.** That work is tracked, deferred, and out of this epic's scope.
- Any other child that meets an attribute traceable to a closed maintainer-ratification issue
  applies the same rule: record the provenance, do not re-litigate, and report it rather than
  removing it.

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

> **Two corrections from F5's research (2026-08-07), both re-verified by `epic-planner`.**
> First, `QfcDatamodel.FrameBuilding.cs` is **not** WinForms code: it has zero
> `System.Windows.Forms` references and its `Frame` is `Deedle.Frame<int, string>`. The STA
> apparatus this brief originally scoped in does not apply, and no `*.StaTests.cs` is created.
> Second, **F11 is not an `IQfcDatamodel` consumer** — `QfcCollectionController.cs` has no reference
> to it at all. The real additional consumers are **F2** (`QfcQueue.cs:476`) and **F6**
> (`QfcFormController.EventHandlers.cs:196`), both reaching it via `IQfcHomeController.DataModel`,
> which is invisible to a grep for the interface name. `IQfcDatamodel` itself takes zero production
> edits, so no sibling faces a compile break.
>
> The live hazard is different and subtler: there are **19 `Mock<IQfcDatamodel>` sites across six
> F7-owned test files** that silently return `default` rather than failing. A behavioral change
> behind that interface would not break the build; it would quietly change what those mocks imply.

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

Three documents state different coverage numbers and no child can satisfy an absolute
repository-wide floor on its own, so the targets are reconciled here once. This is a
reconciliation of which number gates which scope — **not a waiver of any policy**.

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
per-child gate here, because gating every child on a pre-existing shortfall would make the epic
unexecutable.

### The repository-wide comparison must be like-for-like, and no imported figure is valid

An earlier revision of this section cited a **merge-base repository line rate of 70.19%** as the
reference point. **That figure is withdrawn as a comparison baseline.** F11 flagged it and
`epic-planner` verified the cause directly against feature #424's two artifacts:

| Artifact | Root line-rate | Packages | Lines valid |
| --- | --- | --- | --- |
| `evidence/baseline/coverage-baseline.cobertura.xml` | **70.19%** | 14 | 79,957 |
| `evidence/qa-gates/coverage-final.cobertura.xml` | **85.65%** | 9 | 110,849 |

The two differ for two independent reasons, either of which alone invalidates the comparison:

1. **Different package sets.** The raw baseline includes five vendored third-party packages —
   `Microsoft.IO.RecyclableMemoryStream`, `Mono.Reflection`, `System.Interactive`,
   `System.Linq.Async`, `log4net` — which the post-processing step strips. Vendored code is poorly
   covered and drags the raw figure down.
2. **Different instrumented scope.** `lines-valid` *rises* from 79,957 to 110,849 despite the
   package count falling, so the two runs did not even instrument the same body of code.

A child that measures post-processed output against the raw 70.19% would report roughly fifteen
points of phantom improvement it did not produce.

**Rule: the repository-wide criterion is satisfied by a self-consistent before/after pair, never by
comparison against an imported number.** Each child captures repository-wide coverage on its own
branch using the identical command and identical post-processing, before and after its change, and
must not reduce it. Cite both figures and the command in the evidence artifact so the comparison is
auditable. Do not carry a repository-wide figure across branches, across tools, or between raw and
post-processed artifacts.

Note that the 80% per-file line figure and the 75% branch figure are independent gates. F8 found
`EfcHomeController.Timing.cs` at 100% line and 66.67% branch — passing one and failing the other.
Report both.

## Directives for F1's Ledger and Harness

Three children independently converged on the same gaps in F1's brief. These are binding
requirements on F1's deliverables.

### A third ledger bucket: `interface-only / not-measured`

F4 and F7 both found files with **zero coverable lines** — not files that are hard to test, files
with no executable IL at all. F4 identified four: `IConversationResolver.cs`, `IEmailMoveMonitor.cs`,
`QfEnums.cs`, and `cInfoMail.cs` (231 lines of entirely commented-out dead code whose only live
content is eight `using` directives). F7 evidenced the same for both its interface files, three ways,
using `MailItemActionsAdapter` as a positive control to prove the folder was instrumented.

These must **not** be classified `ratified-exempt`, which implies untestable production logic that
was argued away. They are a distinct category with no denominator, and **none receives
`[ExcludeFromCodeCoverage]`**. The ledger carries three buckets: `testable`, `ratified-exempt`, and
`interface-only / not-measured`. A file in the third bucket is reported N/A, never 0%, and never
counts as a failure. Shape-assertion tests written purely to manufacture coverage for such a file
are prohibited.

### Two harness correctness requirements

Both come from F4's reading of the actual Cobertura output, and both are silent-wrong-answer bugs
rather than crashes:

1. **Aggregate per file, not per class.** One source file can produce multiple Cobertura `<class>`
   elements sharing a single `filename` — a type plus its compiler-generated `<>c` closure class.
   The harness must union them, taking **max hits per line**. Reporting the first `<class>` alone
   understates coverage.
2. **Decide the denominator on `<line>` child count, never `line-rate`.** A declaration-only file
   reports `line-rate="0"` because it has no lines, not because it is uncovered. Keying on
   `line-rate` mis-reports every `interface-only` file as a 0% failure — exactly the false alarm the
   third bucket exists to prevent.
3. **Read only the class-level `<lines>` block, never a descendant axis.** `class.iter('line')` or
   an `.//lines/line` XPath unions the class-level block with each method's block and double-counts
   every line present in both. Filed as **#441** against the repository's own coverage scripts, and
   it corrupted this epic's first baseline table before correction.
4. **F11 found a second, distinct defect — filed as #478.** The merge step blends a correct
   class-level union with a primary-only method subtree. **Fixing #441's axis alone does not fix
   #478**; both must be addressed in one change or the harness stays wrong in a different way.
5. **Never trust the emitted `line-rate` / `branch-rate` attributes — recompute from the
   `<line>` elements.** Because of #441 the emitted rates are not per-file figures at all. F10
   documented two proofs: `FocusAndTheme.cs` emits `line-rate=0.756032 = 282/373` for a **326-line
   file**, and — far worse — `MailActions.cs` emits `branch-rate="0.75"`, **falsely passing** the
   75% branch gate against a true 72.7%. The distortion runs in both directions, so no correction
   factor exists.

   This is also why the corrected baseline table in this manifest can be relied on: it was computed
   by summing `condition-coverage` across the class-level `<line>` elements rather than reading the
   emitted attributes, and its 72.7% for `MailActions.cs` **independently matches F10's true
   figure** while the emitted attribute does not. Compute; do not read.

## Verified Toolchain and Tooling Facts

Each item below was verified directly by `epic-planner` against this checkout, because two children
reported contradictory claims and a wrong answer would cost real work at execution time.

### `csharpier .` is stale for the pinned version — use a subcommand

`dotnet-tools.json` at the repository root pins **csharpier 1.2.6**. The v1 CLI requires a
subcommand, so the bare `csharpier .` form given in `CLAUDE.md` §C#1 and §CUT3 does not work against
the pinned tool. Use `dotnet tool run csharpier format .` (or `check` for a non-mutating gate). An
existing `atomic-executor` memory note records the same conclusion independently.

`CLAUDE.md` is a policy document and is **not** amended by this epic; children apply the working
command form and record the deviation in their own evidence rather than editing policy.

### CRLF plans validate — do not normalize

F5 flagged a CRLF hazard and F8 reported the opposite. **F8 is correct.** Verified directly:

- `core.autocrlf=true` with `* text=auto` in `.gitattributes` means committed plans do materialize as
  **pure CRLF** on a fresh Windows checkout. That half of F5's observation is accurate — all six
  committed plans are 100% CRLF with zero bare LF.
- The MCP plan validator nonetheless **accepts them**. All six committed plans were re-validated in
  the integration worktree with `artifact_type: "plan"` and every one returned `ok: true`.

There is therefore **no CRLF normalization step** to perform before re-validating a plan at execution
time. Doing it anyway would be pointless churn against files that already pass.

### An `[ExcludeFromCodeCoverage]` on a partial type suppresses every partial

Confirmed on `QuickFiler/Controllers/QfcDatamodel.cs:25`, where the attribute sits on
`public partial class QfcDatamodel`. It suppresses instrumentation for `QfcDatamodel.cs`,
`QfcDatamodel.QueueProcessing.cs`, and `QfcDatamodel.FrameBuilding.cs` alike, so the latter two read
as zero measured coverage despite carrying no attribute of their own.

This generalises and matters beyond F5. **Absence from a coverage report never means zero coverage
and never means the file is exempt — check whether some other partial of the same type carries the
attribute.** F14's `ItemViewer` family is the other instance: one attribute on `ItemViewer.cs`
suppresses six files. Note that a partial type may be annotated only once; annotating two parts is
CS0579.

## Epic Ruling: delete the dead region in `QfcExplorerController.cs` (F6)

F6 escalated an open decision. `QfcExplorerController.cs` cannot reach the 80% line floor while
`#region Email Sorting To Rewrite` remains: roughly 50-60 uncoverable statements against 60 live
ones. F6 had routed the deletion to issue **#449** to avoid two children editing one file, which
left the floor unreachable and pushed the shortfall to a ledger exemption.

**Ruling: the deletion belongs in F6.** `epic-planner` verified the region independently before
deciding:

- It spans **lines 183-321 of a 323-line file** — 43% of the file.
- Every reference to its six members occurs between lines 185 and 278, i.e. **entirely inside the
  region**. Nothing in lines 1-182 touches it.
- Five of the six members are `private static` and so are unreachable from any other file by
  construction. The sixth, `internal static StripTabsCrLf`, is referenced only at lines 193 and 264,
  both inside the region, and nowhere else in `QuickFiler/` or `QuickFiler.Test/`.
- The same-named methods found elsewhere in the solution belong to different types in
  `UtilitiesCS.EmailIntelligence` and `ToDoModel`, not to this controller. The region is a
  superseded duplicate, consistent with its own "To Rewrite" name.

The region is therefore a self-contained island of unreachable code. Three reasons decide it:

1. **No sibling contention exists.** `QfcExplorerController.cs` is F6's exclusive assignment, and
   #449 is an issue F6 itself filed — not a child of this epic. The "two children editing one file"
   risk that motivated the deferral does not apply.
2. **Policy prefers the refactor.** `.claude/rules/general-unit-test.md` states the correct response
   to untestable lines is to refactor, not exclude. Deleting unreachable code is the cleanest
   available refactor.
3. **It preserves behavior.** Removing code that nothing can call satisfies the epic's
   no-behavior-change NFR.

The alternative — ratifying an exemption for code everyone agrees should be deleted — is exactly the
"exempt rather than fix" pattern this epic's policy reconciliation rejects, and would set a poor
precedent for the fifteen other children.

**Consequences.** F6's approved plan currently routes the deletion away and must be revised to add a
deletion phase before the coverage phases; the plan-path is otherwise unchanged and the revision
re-runs the normal planner/executor preflight cycle. Issue **#449** narrows to its two remaining
findings — `ExplConvView_Cleanup` throwing on a public interface member, and `OpenQFItem`
re-resolving the explorer. `OOS-7` in F6's `spec.md` and `open_risk_for_epic_planner` in its
checkpoint are resolved by this ruling.

## Epic Ruling: a fourth exemption ground for prohibited-to-execute adapters (F13)

F13 escalated a gap it could not resolve, and it is right. `CLAUDE.md` §UT2 enumerates three
exemption grounds — VSTO add-in lifecycle, WinForms form-derived and Designer-generated code, and
Outlook Interop event-handler classes — and **none of them textually covers the WebView2 files**.
None is VSTO lifecycle, none is form-derived, none touches `Microsoft.Office.Interop.Outlook`. All
three existing attributes therefore rest on a ground that does not exist. Two agents reached this
conclusion independently.

`epic-planner` verified `WebView2CoreInitializer.cs` directly. It is a `sealed`, non-`partial`
class carrying a type-level attribute, with exactly two expression-bodied members, each a 1:1
forward into the WebView2 SDK:

```csharp
public Task<CoreWebView2Environment> CreateEnvironmentAsync(string cacheFolder, CoreWebView2EnvironmentOptions options)
    => CoreWebView2Environment.CreateAsync(null, cacheFolder, options);
```

**Ruling: ratify a fourth ground — irreducible adapters whose execution is *prohibited* by test
policy.** The justification is derived from existing policy rather than invented.
`.claude/rules/general-unit-test.md` forbids unit tests that depend on external services or
processes, and forbids creating temporary files. Executing `CreateEnvironmentAsync` requires the
WebView2 Evergreen runtime (an external out-of-process dependency) and writes a user-data cache
folder (a filesystem side effect). A file whose only executable content requires exactly what the
test policy prohibits is not merely *hard* to test — it is unreachable by any **policy-compliant**
test. That is a categorically stronger argument than the "hard to test" reasoning the epic
otherwise rejects, and it is why this ground is admissible where a convenience exemption is not.

F13's own research is the proof that this ground is narrow rather than a loophole: of the three
WebView2 files, **two do not qualify**. `WebView2BreadcrumbHost.InitializeAsync` is already testable
behind a seam its own constructor injects, so its exemption must be removed and the code covered.
Only `WebView2CoreInitializer` survives.

A file qualifies under this fourth ground only when **all** of the following hold. Any failure means
extract a seam and cover the code instead:

1. Every member is a **pure 1:1 forward** to a third-party or host API — no branching, no
   computation, no state, nothing a test could meaningfully assert beyond the forward itself.
2. Executing any member would require an external process, an external runtime, or a filesystem
   side effect that `.claude/rules/general-unit-test.md` prohibits in a unit test.
3. A **seam interface exists** and the consuming code is tested against that interface, so the
   untested surface is the adapter alone and not the logic behind it.
4. The type is `sealed` and **not `partial`** (see the `#457` trap below), and the attribute is
   applied at **type level**.

This ground is ratified **for this epic only** and recorded here rather than in `CLAUDE.md`, which
this epic does not amend. Extending it repository-wide requires maintainer ratification, exactly as
the existing §UT2 exemption did.

## Measurement Trap: method-level exemptions do not suppress lambdas (#457)

F13 filed **#457**, and it has epic-wide reach because several children plan to adopt the
thin-forwarder seam pattern:

- A **method-level** `[ExcludeFromCodeCoverage]` does **not** suppress lambdas nested inside that
  method. The compiler lifts them into a separate generated closure type (`<>c`), which the
  attribute never marks.
- A **type-level** attribute does suppress them.

This is corroborated independently by F4's finding that a closure class surfaces as its own
`<class>` element sharing the source `filename` — and by this epic's own harness rule, which unions
classes by filename and therefore *counts* that closure against the file.

The consequence is a silent coverage cap: a file using the preferred thin-forwarder pattern with
method-level exemptions keeps its lambdas in the denominator, permanently uncovered, and no error is
raised. The fix is a class-level-exempt adapter **type** — which carries its own trap: **that type
must not be `partial`**, or the attribute silently exempts the entire type across every partial, the
same propagation confirmed on `QfcDatamodel.cs:25` and `ItemViewer.cs`.

## Epic Ruling: DEC-1 — unshown Form construction on an STA thread is ratified (F9)

F9's plan halts at `P0-T14` awaiting a literal `RATIFIED_APPROACH`. **The ruling is Approach A.**

The problem: `[ExcludeFromCodeCoverage]` at `EfcViewer.cs:20` sits on the partial *type*, so it also
suppresses `EfcViewer.Designer.cs` (4,277 lines). Removing it — which the epic requires — moves those
generated lines into the denominator in the same edit. F9 offered two routes:

- **Approach A:** construct one **unshown** `EfcViewer` on an STA thread, dispose in `finally`.
  Yields ~100% on `EfcViewer.cs` and ~99% on the Designer, *adding* roughly 2,000 covered lines.
- **Approach B:** no Form construction. ~82% line, forfeits those ~2,000 lines, and requires
  method-level attributes **inside generated code** that Visual Studio silently drops on
  regeneration.

`epic-planner` verified the precedent F9 cited rather than taking it on trust.
`QuickFiler.Test/Controllers/BayesianPerformanceController.TestSupport.cs` already does exactly
this, in merged code, in this very test project: `RunWithViewer` constructs a real Form-derived
`BayesianPerformanceViewer`, never shows it, disposes it in `finally`, saves and restores the
`SynchronizationContext`, runs on an `ApartmentState.STA` thread, and marshals exceptions back with
`ExceptionDispatchInfo`. The pattern is established, not invented.

Four reasons decide it:

1. **The precedent is real and already merged**, in the same assembly, doing the same thing.
2. **Approach A makes the generated file genuinely covered rather than exempted.** ~99% on the
   Designer is a better outcome than any exemption: the file is measured and passes on its own
   merit, and the ~2,000 added covered lines help every child's retain-or-improve obligation.
3. **Approach B's mechanism is self-destroying.** Attributes written into generated code are removed
   the next time the designer regenerates the file, so the exemption silently lapses and the file
   fails a later gate with no diff to explain why. It also has zero repo precedent.
4. The `winforms-testability-refactor` condition barring Form-derived types in tests is aimed at
   **live** forms. The operative distinction is *shown or message-pumping* versus
   *constructed-and-disposed-unshown*, and only the former is a policy hazard.

**Conditions.** Approach A is ratified only in this shape:

- Reuse the existing `RunWithViewer` harness shape verbatim: STA thread, never shown, `finally`
  dispose, `SynchronizationContext` save/restore, `ExceptionDispatchInfo` exception marshalling.
- STA-bound tests live in dedicated `*.StaTests.cs` files and each documents why no seam suffices.
- **Never** call `.Show()`, `.ShowDialog()`, or anything that pumps a message loop. A test that
  displays a popup remains a policy violation.
- This ratifies Form *construction* for coverage of designer-generated control-tree initialization.
  It does not license exercising interactive behavior through a live form.

**This supersedes the stricter "constructing a live form is never acceptable" wording given to F14
in its original brief.** F14 faces the identical situation on `ItemViewer.Designer.cs` (6,224 lines,
suppressed by the type-level attribute on `ItemViewer.cs`) and may apply Approach A under the same
four conditions. F15 may do likewise for its designer-backed viewers.

## Epic Ruling: DEC-5 — a `measured-not-gated` disposition for generated files

F9 raised that F1's three ledger buckets cannot express the state generated designer files land in
under Approach A. F14 then disputed the framing, and **F14 is substantially right**, so the
rationale below is corrected from F9's original.

`epic-planner` measured the actual branch surface. A designer file carries exactly **one** branching
line — `ItemViewerExpanded.Designer.cs` (612 lines) and `BayesianPerformanceViewer.Designer.cs`
(350 lines) each have a single `branch="True"` line carrying four conditions, of which two are
covered, giving the 50% figure. This is a lone generated construct, typically the `Dispose` null
check — not a broad structural branch deficit as F9's "capped near 50% by construction" implied.

F14's decisive point stands on measurement: **removing the type-level attribute improves repository
coverage by +0.57 pp, while exempting the designer costs −0.16 pp**, because `InitializeComponent()`
is branch-free across thousands of lines and one construction covers ~99.95% of it. **Generated
designer files are therefore NOT `ratified-exempt`**, correcting this manifest's own earlier
ground-1 classification of them as exempt-candidates. Exempting them destroys real, freely-obtained
coverage.

What remains unresolvable is only the branch gate on that single generated line, which no test can
meaningfully drive.

**Ruling: add a fourth ledger disposition, `measured-not-gated`,** for generated
`*.Designer.cs` and generated `Properties/` files. Such files:

- **are** instrumented, measured, and reported with real line and branch figures;
- **do** contribute to repository-wide totals;
- are **not** individually gated on either the 80% line or 75% branch floor;
- carry no `[ExcludeFromCodeCoverage]` attribute.

This is distinct from `interface-only / not-measured`, which has no denominator at all. Generated
files have a real denominator and a real, useful numerator — they simply are not code this epic
authored or controls. F16 verifies that every `measured-not-gated` row is genuinely generated code
and not a testable file parked in a convenient bucket.

### Zero-branch files report N/A, never 0% — binding on F1

F14 raised this as a blocking requirement and it is correct. This manifest previously stated the
N/A rule only for the **line** denominator. It applies identically to branches:

**A file whose `branches-valid` is 0 reports branch coverage as `N/A`, never `0%`, and never counts
as a failure.** `ItemViewer.WebViewThread.cs`, `ItemViewer.Commands.cs`, and
`ItemViewer.DisplayState.cs` contain no branch points at all and could not otherwise pass a 75%
branch gate no matter how thoroughly tested. F1's harness must implement this, and F14's Phase 0
carries a halt gate on it.

## Latent Defect Promotion

Preparation research surfaces real defects that are out of scope to fix under the epic's
no-behavior-change NFR. **Promote them to GitHub issues via the MCP promotion lifecycle; do not
leave them as prose in a feature folder**, where they are lost once the folder moves to
`completed/`. F3, F7, and F8 did this, producing issues #442-#447 and #451.

F4 recorded eight defects as plan follow-ups without promoting them. Its execution run must promote
them via the MCP promotion surface before it completes:

- Leaked `BeforeItemMove` subscription when a mail's parent folder changes.
- Handler predicate reading live COM instead of the cached ID.
- Unsynchronised `Queue<T>` across the dispatcher boundary.
- `Reset` double-dispose.
- `DequeueChunk` unbounded regrowth.
- Missing `[Flags]` on `QfEnums.InitTypeEnum`.
- `MailItemInfoTests.cs:25` uses banned `DateTime.Now`.
- `ConversationResolverTests.cs` at 578 lines, breaching the 500-line limit.

The last two are test-policy violations in existing tests, not production defects, and are
in-scope for F4's own execution rather than deferral.

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

### 1b. `QuickFiler.Test.csproj` is the larger shared-file surface

F6 identified a shared file the original decomposition did not assign, and it is a bigger conflict
surface than the production csproj. `QuickFiler.Test/QuickFiler.Test.csproj` is also a non-SDK
project — `<Project ToolsVersion="15.0" xmlns="...">` — with **107 explicit `<Compile Include>`
entries and no globbing**. Verified directly.

Every child in this epic adds test files, so **every child must edit it**, whereas only the subset
that creates production files touches `QuickFiler.csproj`. F6 alone adds 31 entries. The same rules
apply: own entries only, minimal adjacent hunks, preserve CRLF, and expect additive fan-in conflicts
resolved by keeping both sides. Neither csproj is owned by any child; both are shared infrastructure
that this epic edits by necessity.

### 1c. `QfcCollectionController` has two frozen public surfaces (F11)

F11's split of `QfcCollectionController.cs` into partials must preserve two contracts that siblings
already consume. Both verified directly:

- **`public static string xComma(string)` at line 2330** is called by **F8** from
  `EfcHomeController.Metrics.cs:79` as `QfcCollectionController.xComma(...)`, and by
  `Legacy/QfcGroupOperationsLegacy.cs` at four sites. The split must keep this member `public
  static` on a type still named `QfcCollectionController`. Moving it to a differently-named type,
  or reducing its accessibility, breaks F8's compile.
- **The constructor signature is frozen by F6**, which constructs the concrete type at
  `QfcFormController.Actions.cs` lines 49, 83, and 139.

Neither constraint blocks the split — partials share one type name and one accessibility surface —
but a split that reorganises members across *types* rather than across *files* would break both
siblings. Split by file, not by type.

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

- **#400** `2026-07-21-quickfiler-folder-selector-dropdown-400` — overlaps **both F13 and F14**.
  F14 established that #400's live remediation plan explicitly authorizes edits to
  `ItemViewer.Breadcrumb.cs`, which is F14-owned; an earlier revision named only F13.
- **#424** `2026-08-06-quickfiler-high-confidence-queue-init-stall-424` — overlaps F7
  (`QfcHomeController`) and possibly F2 (high-confidence queue admission) territory.
- **#426** `Bug: emailmovemonitor-rejected-item-hook-retention` (added 2026-08-07 from F4's
  research) — open but **not yet promoted to an active feature folder**, which is why it was missed
  in the initial survey. Its subject is the `EmailMoveMonitor` hook lifecycle, squarely inside F4's
  assignment, and its candidate fixes reach into F5- and F2-owned paths. Two of F4's own deferred
  defects (the leaked `BeforeItemMove` subscription and the handler predicate reading live COM
  instead of the cached ID) are plausibly the same underlying defect. Whoever schedules #426 should
  reconcile it against F4's plan first; if #426 is executed independently while F4 is in flight,
  expect a genuine semantic conflict rather than a merely textual one.

Note that a *promoted-but-not-yet-active* issue is invisible to a `docs/features/active/` scan. That
is how #426 was missed at decomposition time. Children whose research touches an area should search
open GitHub issues by keyword, not only the active feature folders.

Neither blocks planning. At execution time `epic-orchestrator` rebases the integration branch on
`main` before each wave, and any conflict is handled by the child's own R1–R5 remediation loop per
the `epic-orchestrate` skill. Children F2, F7, and F13 should read the current state of those two
feature folders during research so their plans account for whichever version has merged.
