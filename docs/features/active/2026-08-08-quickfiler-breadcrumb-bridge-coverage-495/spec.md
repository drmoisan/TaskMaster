# quickfiler-breadcrumb-bridge-coverage — Spec

- **Issue:** #495
- **Parent:** epic #136 `quickfiler-per-file-coverage`, child F12
- **Owner:** drmoisan
- **Last Updated:** 2026-08-08T02-45
- **Status:** Prepared (preparation mode — authored now, executed later by `epic-orchestrator`)
- **Version:** 1.0
- **Work Mode:** `full-feature` (`spec.md` + `user-story.md` are the authoritative AC sources)
- **Integration branch:** `epic/quickfiler-per-file-coverage-integration`
- **Upstream dependency:** F1 (#432) `quickfiler-coverage-denominator-and-exemption-ledger`

## 1. Overview

Child F12 owns the QuickFiler breadcrumb bridge, messenger, and lifecycle coordination cluster —
five production files totalling **2,183 physical lines**. None carries an
`[ExcludeFromCodeCoverage]` attribute, so there is no exemption-disposition work in this child.

Every file clears the 80% per-file line floor, which caused an early assessment to treat this child
as a near-no-op. That assessment was wrong.
`Viewers/BreadcrumbItemViewerLifecycleCoordinator.cs` sits at **66.44% branch against the 75% floor
across 146 branch points**, with **exactly 49 untaken outcomes** — the largest single branch gap in
the epic. Line and branch coverage are independent gates per the epic's
"Coverage-Target Reconciliation", and this child fails the branch gate today.

## 2. Measured Baseline — recomputed per file, not read

All figures below were independently recomputed by per-file research from the class-level `<lines>`
block of
`docs/features/active/2026-08-06-quickfiler-high-confidence-queue-init-stall-424/evidence/qa-gates/coverage-final.cobertura.xml`,
never from `class.iter('line')`, never from an `.//lines/line` axis, and never from the emitted
`line-rate` / `branch-rate` attributes (issues #441 and #478).

| File | Physical | Coverable | Line % | Branch pts | Branch % | Untaken | Headroom |
| --- | --- | --- | --- | --- | --- | --- | --- |
| `Viewers/BreadcrumbItemViewerLifecycleCoordinator.cs` | 481 | 318 | 90.57% (288/318) | 146 | **66.44%** (97/146) | **49** | 19 |
| `Viewers/BreadcrumbBridgeCoordinator.cs` | 487 | 280 | 100.00% | 87 | 87.36% (76/87) | 11 | 13 |
| `Viewers/BreadcrumbMessengerHub.cs` | 456 | 294 | 100.00% | 118 | 96.61% (114/118) | 4 | 44 |
| `Viewers/BreadcrumbCoordinatorUpgradeLifetime.cs` | 309 | 204 | 99.02% (202/204) | 54 | 92.59% (50/54) | 4 | 191 |
| `Controllers/BreadcrumbBridgeRouter.cs` | 450 | 282 | 97.87% (276/282) | 90 | 92.22% (83/90) | 7 | 50 |

**All five rows confirm the brief's coverage figures.** F12 is the first child in the epic in which
every file's stated coverage survives re-measurement unchanged. The corrections this child found are
structural rather than numeric, and are listed in §7.

**This baseline remains indicative.** It was captured on another feature's branch. F1's harness, run
on this child's own branch, is the authority, and these numbers are not acceptance evidence.

### 2.1 Emitted attributes are wrong on every file — recompute, never read

Each file provides an independent specimen of #441 / #478:

- `BreadcrumbItemViewerLifecycleCoordinator.cs` — emitted `line-rate="0.939516"` and
  `branch-rate="0.688073"` were reconstructed exactly as `466/496` and `150/218`, i.e.
  `(class-level + method-level)` over `(class-level + method-level)`. A direct per-file proof of #441.
- `BreadcrumbBridgeRouter.cs` — emitted `branch-rate="0.926471"` encodes `63/68` against a true
  `83/90`. **Trap:** the unrelated UtilitiesCS type emits `branch-rate="0.922222"`, which matches this
  file's *correct* recomputed figure to six digits by coincidence, so the right answer and a
  wrong-type answer are indistinguishable by inspection.
- `BreadcrumbMessengerHub.cs` — emitted `branch-rate="0.977273"` (`43/44`, the primary-method
  subtree) errs **optimistically** against a true `114/118`.
- `BreadcrumbCoordinatorUpgradeLifetime.cs` — emitted `branch-rate="0.910714"` (`51/56`) against a
  true `50/54`.

### 2.2 The union-by-filename rule is load-bearing here, and it inverts

Three of the five files declare more than one top-level type, which the brief did not state:

| File | Types declared |
| --- | --- |
| `BreadcrumbItemViewerLifecycleCoordinator.cs` | coordinator (`:13`), `NavigationSubscriptionFactory` delegate (`:330`), `BreadcrumbNavigationSubscription` (`:337`), `BreadcrumbPopupLifecycleOperations` (`:355`) |
| `BreadcrumbMessengerHub.cs` | `BreadcrumbMessengerHub` (`:15`) with nested `Attachment` (`:17`) and `CachedState` (`:35`), `BreadcrumbCollapsedAttachment` (`:277`), `BreadcrumbResourceOwner` (`:436`) |
| `BreadcrumbCoordinatorUpgradeLifetime.cs` | `BreadcrumbUpgradeLease` (`:9`), `BreadcrumbCoordinatorUpgradeLifetime` (`:35`) |

**The consequence is not the one the epic's harness rule anticipates.** In every case exactly one
`<class>` element carries the filename, so no union is required — but that element may be **named
after a secondary type**. `BreadcrumbCoordinatorUpgradeLifetime.cs`'s element is named
`QuickFiler.Viewers.BreadcrumbUpgradeLease`; `BreadcrumbMessengerHub.cs`'s single element covers all
294 lines while `BreadcrumbCollapsedAttachment` and `BreadcrumbResourceOwner` have no element of
their own. A harness keyed on `<class name=>` would report the principal type as **absent** and
silently drop 124 of the hub's 294 lines. Key on `filename=`, always.

## 3. Behavior

Raise per-file coverage for the five assigned files to at least 80% line and at least 75% branch,
verified with F1's harness, with **no observable behavior change** to QuickFiler flows.

Branch-gap closure in this cluster means covering untaken guard-clause sides, disposal and
post-disposal paths, double-invoke and re-entrancy guards, out-of-order state transitions, and
malformed-payload handling — not additional happy-path tests.

### 3.1 Projected result

| File | Line before → after | Branch before → after |
| --- | --- | --- |
| `BreadcrumbItemViewerLifecycleCoordinator.cs` | 90.57% → **100.00%** | 66.44% → **97.95%** (46 reachable) |
| `BreadcrumbBridgeCoordinator.cs` | 100.00% → 100.00% | 87.36% → **100.00%** |
| `BreadcrumbMessengerHub.cs` | 100.00% → 100.00% | 96.61% → **100.00%** |
| `BreadcrumbCoordinatorUpgradeLifetime.cs` | 99.02% → **100.00%** | 92.59% → **100.00%** |
| `BreadcrumbBridgeRouter.cs` | 97.87% → **100.00%** | 92.22% → **100.00%** |

The lifecycle coordinator clears the 75% branch floor by 22.95 points even waiving all three
structurally unreachable outcomes. If the two reflection-dependent router gaps (J5, J6) are rejected
at review, that file lands at 99.65% line / 95.56% branch — still comfortably above both floors.

## 4. Production Edit Verdict — none, on any file

All five research artifacts independently reached the same verdict: **no production edit is required
or recommended.** Every untaken outcome except six is reachable from `QuickFiler.Test` through the
existing surface, using the `[assembly: InternalsVisibleTo("QuickFiler.Test")]` grant at
`QuickFiler/Properties/AssemblyInfo.cs:5`.

Consequences:

- **No `QuickFiler/QuickFiler.csproj` edit**, no ledger row under "Mid-Wave File Creation", and no
  new-file >= 90% obligation on production code.
- **The #457 measurement trap does not engage.** No `[ExcludeFromCodeCoverage]` is introduced at
  either level, so there is no lifted-lambda leak. Recorded for completeness: had a thin-forwarder
  been required, it would have to be a class-level-exempt adapter **type**, `sealed` and **not
  `partial`**.
- **Tight headroom is preserved.** `BreadcrumbBridgeCoordinator.cs` has 13 lines and
  `BreadcrumbItemViewerLifecycleCoordinator.cs` 19 against the 500-line ceiling; neither could absorb
  a seam class without a partial split.

### 4.1 Structurally unreachable outcomes — not targets, with proofs

Six outcomes are excluded from the target set. No task may attempt them:

- `BreadcrumbItemViewerLifecycleCoordinator.cs:135` and `:138` — every call site of `_rowCount` /
  `_cancelSelector` is gated on `_isSelectorOpen()` returning true, which a null bridge makes false.
- `BreadcrumbItemViewerLifecycleCoordinator.cs:234` — `_openCoordinator` is nulled only inside
  `ReleaseHostCore`, which either reassigns immediately or runs after `_disposed` is set.
- `BreadcrumbBridgeRouter.cs` J5 (`:288` c1, `:372` c1/c2) and J6 (`:426` loop-exit, `:434`) are
  unreachable through the public surface and require reflective `_rows` seeding or a reflective
  `HandleUpArrow` call. They are **optional**; if review rejects reflection, they are dropped and the
  file still passes both floors.

## 5. Determinism — the brief's instruction is refuted unanimously

**The instruction "use an injected clock and fake timers" must be struck.** All four newly researched
files return **zero** matches for `DateTime`, `Stopwatch`, `Timer`, `Task.Delay`, `Thread.Sleep`, and
`TimeProvider`, as did `BreadcrumbBridgeCoordinator.cs`. There is no time dependency anywhere in this
cluster to control. Introducing a clock seam would add a seam with no dependency behind it.

This adopts sibling F13's ratified ruling at
`docs/features/active/2026-08-07-quickfiler-breadcrumb-dropdown-webview-coverage-455/spec.md:381-390`
(§8.1). Determinism in this cluster is **scheduler and completion-source control**, and for
`BreadcrumbBridgeRouter.cs` it is weaker still — already-completed-task control via Moq.

Deterministic vehicles that already exist in-repo and are green:

1. `BreadcrumbUiDispatcher.CreateForCurrentThreadTests()` (`BreadcrumbUiDispatcher.cs:62-65`) — runs
   every `Dispatch(...)` inline, no context, no pump.
2. `BreadcrumbBridgeCoordinatorTests.InlineSynchronizationContext` (`:90-93`), installed and restored
   in `try`/`finally` (`:95-112`).
3. `BreadcrumbSelectorToggleUiBoundaryTests.CapturingSynchronizationContext` (`:346-401`) — a
   manually-pumped queue exposing `WaitForPost()`, `DrainAll()`, `DrainUntil(Task)`.
4. Test-owned `TaskCompletionSource<T>` gates (`BreadcrumbCoordinatorLifecycleTests.cs:394-397`).

### 5.1 Two brief checklist items are not applicable

- **"Cancellation and cancelled-token paths"** — `BreadcrumbMessengerHub.cs` and
  `BreadcrumbItemViewerLifecycleCoordinator.cs` contain no `CancellationToken` at all. Applicable
  only to `BreadcrumbBridgeCoordinator.cs` and, partially, `BreadcrumbBridgeRouter.cs`.
- **"Disposal and post-disposal invocation paths"** — `BreadcrumbBridgeRouter.cs` implements no
  `IDisposable`, holds no disposable resource, and has no disposal flag. Report N/A for that file
  rather than leaving it unchecked.

## 6. Cross-Child Constraints

### 6.1 Four sibling children compile against F12 types

Verified directly. This is a frozen-signature obligation the brief did not state:

| F12 type | Consumer | Owner |
| --- | --- | --- |
| `BreadcrumbItemViewerLifecycleCoordinator` | `ItemViewer.Breadcrumb.cs:15,50,155,191,253,268` | F14 (#456) |
| `BreadcrumbMessengerHub` | `ItemViewer.Breadcrumb.cs:263` | F14 (#456) |
| `BreadcrumbPopupLifecycleOperations` | `ItemViewer.Breadcrumb.cs:84`; `BreadcrumbPopupUiOperations.cs:401,414,466` | F14 + F13 (#455) |
| `BreadcrumbNavigationSubscription` | `BreadcrumbPopupUiOperations.cs:484` | F13 (#455) |
| `BreadcrumbBridgeRouter` | `EfcFormController.cs:141,843` | F9 (#452) |

**F14 has issued an explicit freeze** at
`docs/features/active/2026-08-07-quickfiler-itemviewer-coverage-456/spec.md` § "To F12 — FREEZE":
the coordinator's six-argument constructor
(`BreadcrumbItemViewerLifecycleCoordinator.cs:29-36`) and `BreadcrumbBridgeCoordinator`'s internal
three-argument constructor (`:45-59`) are consumed verbatim and must not be reordered or retyped.

**F12's own protection is symmetrical:** F13 commits to no public or internal signature changes to
its 15 files (`.../455/spec.md:49-50`), which is what keeps `BreadcrumbUiDispatcher`,
`BreadcrumbCollapsedSurfaceController`, and `BreadcrumbNavigationReadiness` stable underneath F12's
fixtures. Cite that commitment in the plan.

### 6.2 F13's plan will strip incidental coverage from F12-declared members

Five of the eight direct-invocation sites of `BreadcrumbPopupLifecycleOperations` and
`BreadcrumbNavigationSubscription` live in
`QuickFiler.Test/Viewers/BreadcrumbPopupUiOperationsDirectAdapterTests.cs` — an **F13-owned test
file that F13's approved plan actively rewrites**, relocating two of its three production call sites
(`BreadcrumbPopupUiOperations.cs:401`, `:466`) into a class-level-exempt adapter. F13 correctly notes
the dependence is "reduced, not deepened" *from its side*; from F12's side that coverage is hostage
to a sibling's plan. **F12 must own direct tests for these members rather than inherit them.**

### 6.3 F14 requests that live `ItemViewer` construction be retained

`BreadcrumbCoordinatorLifecycleTests.ViewerScope` (`:469-487`) constructs `new QuickFiler.ItemViewer()`.
F14 explicitly asks F13/F12/F10 **not** to replace that with a mock while F14 is in flight, because
`ItemViewer.cs` and `ItemViewer.Designer.cs` derive real coverage from it. F12 must not "clean up"
that construction.

### 6.4 An unrecorded cross-child test-file ownership

`Controllers/BreadcrumbOutboundQueue.cs` is **F2-owned (#431)** per the epic manifest, yet its tests
live in the F12-owned `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterQueueTests.cs:207-220`. This
is recorded in neither child's spec nor the epic's "Cross-Child Constraints". Expect an additive
fan-in conflict on that file.

### 6.5 Open issue #440 will rewrite semantics in two F12 files

**#440** `breadcrumb-left-right-arrow-parent-child-navigation` is open, is not yet promoted to an
active folder, and was absent from the epic's "Known Conflict Risks" until this child added it. It
names `BreadcrumbBridgeRouter` and `BreadcrumbRow` on the Efc path and `BreadcrumbBridgeCoordinator`
on the Qfc path. **F12 tests pin current behavior, not corrected behavior**, and every affected test
carries an in-code comment naming #440 so a future break is legible.

## 7. Corrections to the Brief (documented deviations)

1. **"Injected clock and fake timers" is refuted on all five files** (§5). Struck and replaced with
   scheduler/completion-source control, adopting F13 §8.1.
2. **The brief's "Lines" column is coverable lines, not physical lines.** The hub is 456 physical
   against 294 quoted; the cluster is 2,183 physical against a 1,378 coverable sum. This materially
   changes the seam budget: real headroom is 13–191 lines per file, not the 150–250 the table implies.
3. **Three of five files declare multiple top-level types** (§2.2), and the resulting Cobertura
   `<class name=>` mismatch inverts the epic's union trap.
4. **`FolderBreadcrumbBridgeRouter` (UtilitiesCS) is a different type from `BreadcrumbBridgeRouter`
   (QuickFiler, F12-owned).** The type constructed at `BreadcrumbBridgeCoordinator.cs:52` is the
   UtilitiesCS one. A substring grep returns 71 hits across 14 files; the true surface for F12's file
   is 3 test files and 1 production consumer.
5. **`BreadcrumbPopupLifecycleOperations.CreateCollapsedCandidate` (`:380-409`) is 0% covered
   end-to-end** — 20 uncovered lines and 10 untaken outcomes, appearing in no brief, spec, or epic
   document. The likely cause is that the test named `CandidateFailure_CleansMessengerAndReadiness`
   actually exercises `CreateNavigationSurface`.
6. **The existing-test surface is far wider than the brief states** — 15 referencing files for the
   bridge coordinator, 9 for the lifecycle coordinator, 3 for the router. Any retain-or-improve
   analysis limited to the named files is incomplete.
7. **"Cancellation" and "disposal" checklist items are N/A on specific files** (§5.1).
8. **`BreadcrumbNavigationReadiness` is declared in `BreadcrumbWebViewSurfaceFactory.cs:19` and is
   F13-owned**, not an F12 type.
9. **Line-number drift: none, on any of the five files.** Every gap line re-anchors exactly.

## 8. Retain-or-Improve Risks

Three files sit at or near 100% line, so they can only regress. The material risks:

- **R1 — `BreadcrumbResourceOwner` has zero direct test references.** Its 13 lines are covered only
  as a side effect of live `ItemViewer` construction in six F13/F14-owned test files. Loss takes the
  hub to 95.58% line / 93.22% branch. Mitigated by eliminating the dependency, not monitoring it.
- **R2 — `BreadcrumbCollapsedAttachment` (111 lines) rests on two test files.** Loss takes the hub to
  62.24% line / 52.54% branch, **failing both floors**.
- **R3 — one currently-covered hub outcome depends on garbage-collection timing.**
  `BreadcrumbMessengerHub.cs:447`'s `disposing == false` arm is reachable only via `Component`'s
  finalizer and is covered today by GC scheduling, not by any test. A different schedule silently
  drops branch to 95.76% with no diff to explain it. Converting it to an asserted outcome is
  worthwhile even though it closes no current gap.
- **R4 — `BreadcrumbBridgeCoordinator`'s `PostSelectorStateCore` concrete-type gate.** Lines
  `:302-320` execute only when the injected `IWebViewMessenger` is the concrete `BreadcrumbMessengerHub`.
  Only `BreadcrumbSelectorCoordinatorTests.cs` and `BreadcrumbDuplicateIdentityIntegrationTests.cs`
  pass a real hub; if either is retargeted, 19 lines drop and the file falls to roughly 93%.
- **R5 — F8-owned `EfcHomeControllerExecuteMovesTests` covers `BreadcrumbBridgeRouter.cs`
  incidentally**, so an F8 refactor can move F12's numbers.

## 9. Acceptance Criteria

- [ ] **AC-1** Every one of the five assigned files reaches >= 80% line **and** >= 75% branch,
      verified with F1's harness on this branch, recorded as numeric per-file evidence under
      `<FEATURE>/evidence/qa-gates/`. Where a file already clears a floor, the bar on that axis is
      retain-or-improve against the Phase 0 baseline measured on this same branch.
- [ ] **AC-2** `Viewers/BreadcrumbItemViewerLifecycleCoordinator.cs` closes its branch gap from
      66.44% to >= 75%, with the projected target being 97.95% across the 46 reachable outcomes.
- [ ] **AC-3** Per-file figures are recomputed from the class-level `<lines>` block keyed on
      `filename=`, never from `<class name=>`, never from `class.iter('line')` or an `.//lines/line`
      axis, and never from the emitted `line-rate` / `branch-rate` attributes. The evidence artifact
      states explicitly that no emitted attribute was used.
- [ ] **AC-4** Repository-wide coverage is retained or improved, measured as a self-consistent
      before/after pair captured on this branch in the same session with an identical command and
      identical post-processing. No imported figure is cited as a comparison baseline.
- [ ] **AC-5** No production file exceeds 500 lines. No production source file is modified at all;
      if that verdict changes during execution, any newly created production file reaches >= 90% line
      coverage, gains a `<Compile Include>` entry in `QuickFiler/QuickFiler.csproj`, and gains a
      ledger row per the epic's "Mid-Wave File Creation" rules.
- [ ] **AC-6** All new tests use MSTest, Moq, and FluentAssertions in Arrange–Act–Assert form, and
      are deterministic and isolated: no `Thread.Sleep`, no `Task.Delay`, no wall-clock wait, no
      real-time polling, no temporary files, no filesystem writes, no external services or processes,
      no shown forms, no popups, and no STA attributes. **No injected clock and no `TimeProvider`** —
      §5 is binding.
- [ ] **AC-7** No new test file exceeds 500 lines, and every ambient `SynchronizationContext`
      assignment is restored in a `finally`.
- [ ] **AC-8** The full C# toolchain passes in order in a single final pass: `dotnet tool run
      csharpier format .`, then the analyzer build, then the nullable build, then the MSTest run with
      coverage. Each stage records `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:`
      under `<FEATURE>/evidence/qa-gates/`.
- [ ] **AC-9** No behavior change to observable QuickFiler flows. No production `.cs` file appears in
      the child's diff.
- [ ] **AC-10** Tests pin **current** behavior, not corrected behavior, for every defect promoted
      from this child's research (#498, #499, #500, #501, #502) and for open #440. No test asserts a
      defective outcome as desirable; where current behavior is the defect, the path is left
      untested and the reason recorded.
- [ ] **AC-11** F12 owns direct tests for `BreadcrumbPopupLifecycleOperations` and
      `BreadcrumbNavigationSubscription` rather than relying on the F13-owned
      `BreadcrumbPopupUiOperationsDirectAdapterTests.cs` (§6.2), and
      `BreadcrumbPopupLifecycleOperations.CreateCollapsedCandidate` reaches >= 80% line from 0%.
- [ ] **AC-12** The frozen contracts in §6.1 are preserved byte-for-byte in signature: the
      coordinator's six-argument constructor, `BreadcrumbBridgeCoordinator`'s internal three-argument
      constructor, and every `internal`/`public` member consumed by F9, F13, or F14.
- [ ] **AC-13** `BreadcrumbCoordinatorLifecycleTests.ViewerScope`'s live `new QuickFiler.ItemViewer()`
      construction is retained, per F14's request (§6.3).
- [ ] **AC-14** `BreadcrumbSelectorCoordinatorTests.cs` and
      `BreadcrumbDuplicateIdentityIntegrationTests.cs` continue to construct a real
      `BreadcrumbMessengerHub`, and `BreadcrumbMessengerHubTests.cs` /
      `BreadcrumbMessengerHubCoverageTests.cs` retain their `BreadcrumbCollapsedAttachment` coverage
      (§8 R1, R2, R4).
- [ ] **AC-15** Every `<Compile Include>` added to `QuickFiler.Test/QuickFiler.Test.csproj` is an
      F12-owned entry only, inserted in minimal adjacent hunks within the breadcrumb block
      (lines 58-91), with CRLF preserved. No `sed -i`.
- [ ] **AC-16** The six structurally unreachable outcomes in §4.1 are documented as excluded with
      their proofs, and no task attempts them.

## 10. Definition of Done

- [ ] All acceptance criteria AC-1 .. AC-16 satisfied with evidence.
- [ ] All user-story criteria US-1 .. US-8 satisfied.
- [ ] Per-file coverage evidence committed under `<FEATURE>/evidence/qa-gates/`.
- [ ] Full toolchain pass recorded, in order, green in a single final pass.
- [ ] No production `.cs` file modified.
