---
epic: quickfiler-suite-determinism-foundation
integration_branch: epic/quickfiler-suite-determinism-foundation-integration
created_at: 2026-08-21T17-45
intent:
  epic_type: enabler
  business_outcome_hypothesis: >-
    Removing the two sources of nondeterminism from the QuickFiler test suite, and settling the
    three isolated contract defects that later QuickFiler work must build on, produces a suite
    whose red is trustworthy — so that the remaining 43 open QuickFiler defects can be certified
    against evidence rather than against a suite that fails on some runs and passes on others.
  leading_indicators:
    - The full nine-assembly suite passes on ten consecutive runs under induced CPU load.
    - No unit-test run creates a visible window on the desktop.
    - The IKbdAction contract has no commented-out members and no implementer reporting a
      delegate type it does not store.
  nfrs:
    - No test is stabilized by adding a sleep, a retry, or a timing tolerance.
    - Coverage of QuickFiler.csproj is retained or improved at every child merge.
    - No production file exceeds 500 lines after change.
    - Full C# toolchain (csharpier, analyzers, nullable, MSTest with coverage) green per child.
features:
  - issue_num: 511
    feature_folder: winformspumphost-suite-determinism-511
    depends_on: []
  - issue_num: 445
    feature_folder: 2026-08-07-quickfiler-keyboard-action-contract-defects-445
    depends_on: []
  - issue_num: 491
    feature_folder: 2026-08-07-quickfiler-test-form1-live-form-491
    depends_on: []
  - issue_num: 449
    feature_folder: 2026-08-07-quickfiler-explorer-controller-latent-defects-449
    depends_on: []
---

# Epic: QuickFiler Suite Determinism Foundation

## Goal

Make the QuickFiler test suite deterministic and headless, and settle three isolated contract
defects, so that the remaining QuickFiler defect backlog can be delivered against a suite whose
failures mean something.

This epic is the first of three planned over the QuickFiler defect corpus. It is deliberately the
smallest and the least entangled: every child here owns a file set that no other child in this
epic and no child of the two later epics contends on, with the single exception of the shared
test project file discussed under Shared-Surface Coordination.

## Scope

Five issues across four children:

- **#511 + #571 — `WinFormsPumpHost` suite determinism.** `QuickFiler.Test/TestSupport/WinFormsPumpHost.cs`
  runs `Application.Run(new ApplicationContext())` on a dedicated STA thread and never adds a form
  or control, so no window handle is ever created. Eight consumer tests plus thirteen self-tests
  depend on it. #571's two intermittent failures
  (`InitializeNineArgOverload_ThroughThePumpHost_SavesParametersAndDelegates` and
  `InitializeBool_ThroughThePumpHost_CompletesAndInitializesState`) fail inside
  `QfcItemController.InvokeBeginInvoke` at `QuickFiler/Controllers/QfcItemController.FocusAndTheme.cs:256`
  precisely because `Control.Invoke` is reached before a handle exists. #511 and #571 are one
  feature, not two, for the reason in Decomposition Rationale below.
- **#445 — keyboard-action contract defects.** Three defects across `KaChar.cs`, `KaKey.cs`,
  `KaStringAsync.cs`, `KbdActions.cs`, and `QuickFiler/Interfaces/IKbdAction.cs`: an inconsistent
  `Activated` gate in `KaStringAsync.KeyEquals`, an `ArgumentOutOfRangeException` on
  `KeyEquals("")`, and `KaChar.DelegateType` reporting `typeof(Action<Keys>)` while storing an
  `Action<char>`.
- **#491 — live form in the test project.** `QuickFiler.Test/Form1.cs` and its designer are
  compiled into the test assembly and construct a real form.
- **#449 — explorer-controller latent defects.** Two latent defects in
  `QuickFiler/Controllers/QfcExplorerController.cs` plus a block of dead duplicated code.

## Non-Goals

- The `IItemViewer` UI-thread seam consolidation (#489) is **not** in this epic. It rewrites
  `IItemViewer`, `ItemViewer.cs`, and `ItemViewer.WebViewThread.cs`, which the third epic's
  ItemViewer child owns. It is scheduled there.
- Replacing the real message pump with a synchronization-context seam wholesale is **not**
  mandated here. See Decomposition Rationale.
- No `.claude/**` file is edited by any child of this epic. Where an issue cites a rule file, the
  citation is the policy the fix is measured against, not an edit target.

## Shared Design

The suite's nondeterminism has one shape: a real WinForms control is reached through a real
`Control.Invoke` before its window handle exists, and whether the handle exists depends on OS
scheduling. The existing seam is already interface-typed and already mockable — see Decomposition
Rationale — so the correction is deterministic fixture setup, not new abstraction.

## Decomposition Rationale

**#511 and #571 are one child, not two.** `QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part2.cs:51`
defines a `UiThreadDispatcherGate` and a `SwapUiThreadDispatcher` helper that mutate the
process-wide static `UtilitiesCS.UiThread._dispatcher` by reflection, serializing the pump tests
across two test classes. Any change to the host or its harness must preserve that serialization or
`QfcItemController.SeamFactoryTests` and `QfcItemController.InitializationTests` deadlock against
each other under class-level parallelization. Two branches cannot safely make that change
independently.

The two issues are also in **tension rather than dependency**, and the child must reconcile them
rather than assume an order. #511 proposes replacing the real pump with an injectable
synchronization-context seam. Executed literally, that deletes or reclassifies the very tests #571
wants to stabilize, along with the coverage justifications recorded at
`QuickFiler/Controllers/QfcItemController.Initialization.cs:166, 261, 293, 404, 448` and
`QuickFiler/Controllers/QfcItemController.ViewerSetup.cs:31, 256`. The child's spec must decide the
direction, not inherit it.

**The marshalling seam already exists.** `QfcItemController` holds `IItemViewer` (not a concrete
control) at `QuickFiler/Controllers/QfcItemController.cs:51`, and `Invoke`, `BeginInvoke`, and
`InvokeRequired` are re-declared on the interface at `QuickFiler/Viewers/IItemViewer.cs:95-100`
specifically to stay mockable. A second seam, `UtilitiesCS.Threading.IUiDispatcher`, is held at
`QuickFiler/Controllers/QfcItemController.cs:66`. Both are already exercised without a pump in
`QuickFiler.Test/Controllers/QfcItemController.FocusAndThemeTests.cs:99-115`. This epic therefore
introduces no new seam; the planning premise that a shared test-support seam must be built first
did not survive inspection.

**Forcing a handle is not a prohibited timing hack.** `.claude/rules/csharp.md:95` prohibits
"adding sleeps, retries, or timing hacks to mask flaky behavior." Deterministically establishing a
control's window handle on the pump thread before the act removes the race rather than masking it,
and is therefore permitted. The child must still record this reading in its spec, because #571's
own text raises the question.

## Shared-Surface Coordination

`QuickFiler.Test/QuickFiler.Test.csproj` is a legacy non-SDK project with 116 explicit
`<Compile Include>` entries, so any child that adds or removes a test file must edit it. Two
children here do, and their regions are partitioned:

- **#491 owns the `Form1` region** — `QuickFiler.Test/QuickFiler.Test.csproj:161-165` (the
  `Form1.cs` and `Form1.Designer.cs` compile entries) and `:180-181` (the `Form1.resx` embedded
  resource). No other child may touch those lines.
- **#449 owns one appended `Compile Include`** for the explorer-controller test file it must
  create, because no `*Explorer*` test file exists today. It appends to the `Controllers` item
  group and must not touch the `Form1` region.
- **#511/#571 and #445 add no compile entry.** Their regression tests belong in existing files
  that already carry entries: `QuickFiler.Test/TestSupport/WinFormsPumpHostTests.cs` and
  `QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part3.cs` for the former;
  `KaCharTests.cs`, `KaKeyTests.cs`, `KaStringAsyncTests.cs`, `KbdActionsTests.cs`, and
  `KbdActionsRemainingBranchesTests.cs` for the latter.

With those regions partitioned, all four children sit in wave 0 and the dependency graph is empty.

## Waves

Wave 0 (all four, no dependency edges): #511/#571, #445, #491, #449.

The graph is intentionally empty. Ordering in this epic comes from the csproj region partition
above, not from `depends_on` edges, because no child's fix changes a contract another child in this
epic consumes.

## Complexity Assessment

| child | band | rationale |
| --- | --- | --- |
| #511 + #571 | C3 | Two issues in tension that the child must reconcile; a process-wide static mutated by reflection; 8 consumer tests and 13 self-tests in blast radius; a policy reading to settle. |
| #449 | C3 | Two latent defects plus dead duplicated code in a 1,065-line legacy neighbour; no existing test file, so the harness is new; touches `UtilitiesCS` mail-filing collaborators. |
| #445 | C2 | Five small files, but one genuine behavioural decision (whether the third `KeyEquals` branch should be `Activated`-gated) and a `DelegateType` removal that must not break `KaCharAsync`/`KaKeyAsync`. |
| #491 | C2 | Bounded removal of a live form from the test assembly, plus the csproj compile and embedded-resource entries. Corrected 2026-08-22: an earlier revision of this row claimed "one dependent test file". There is none - nothing under `QuickFiler.Test/` references `Form1` outside its own two files, so the removal is self-contained and the plan adds a guard test rather than editing a dependent one. |

## Execution Notes for epic-orchestrator

1. **Re-normalize prepared plans to LF before revalidating.** `core.autocrlf` is `true` in this
   repository, so each prepared plan committed here as LF materializes as CRLF in a freshly created
   child worktree, and the MCP `plan` validator has rejected CRLF plans. A validator failure on a
   plan that passed during preparation is this effect, not a defect in the plan.
2. **`vstest` requires `/InIsolation`.** Without it, binding redirects in each assembly's
   `app.config` are ignored and roughly 1,695 phantom failures appear with empty messages and
   sub-millisecond durations, surfacing as a `TypeInitializationException` from Moq via
   `System.Threading.Tasks.Extensions`. Use
   `vstest.console.exe <assemblies> /EnableCodeCoverage /InIsolation /TestCaseFilter:"TestCategory!=LiveOutlook"`.
3. **Exclude `\.claude\` from recursive `*.Test.dll` discovery.** Six stale agent worktrees exist
   under `.claude/worktrees/`. None currently holds build output, but any of them will once built,
   and a CI-style recursive search would then load stale assemblies.
4. **Do not rely on any `PreToolUse` hook.** Every hook in this repository currently reads
   `$toolInput.command` while the payload nests the value at `$toolInput.tool_input.command`, so
   the property is always null and each hook returns `permissionDecision: allow`. The epic wave
   barrier, merge gate, and worktree-removal gate are all inert. Confirm every wave transition from
   `git worktree list --porcelain`, `git branch`, and `gh pr view --json state,mergedAt,headRefOid`.
5. **No Python toolchain exists here.** There is no `scripts/dev_tools/` and no Poetry manifest, so
   any skill step naming `poetry run python -m scripts.dev_tools.*` is unrunnable by absence. The
   PowerShell equivalents live under `.claude/lib/`.
6. **The four child issues are already open.** Each child must call only
   `mcp__drm-copilot__new_active_feature_folder`; `potential_to_issue` has no idempotent path and
   would file a duplicate.

## Known-Stale Potential-Document References

The promoted potential documents are the authoritative requirements source for this corpus, but
their line references have drifted against `main`. Every child MUST re-derive its own line numbers
by reading the target file and MUST NOT trust a `file:line` citation in its potential document. A
child that edits a region named by a drifted reference will edit the wrong code.

Measured drift, recorded for the whole four-epic corpus (only the first two rows affect this epic
indirectly; the rest are recorded so later epics do not re-discover them):

| Document | Drift |
| --- | --- |
| `286.md` | Stale by +17 lines. `RemoveSpecificControlGroupAsync` is at `:1159-1248`, not `:1142-1233`. |
| `462.md` | Stale by approximately +46 lines. |
| `474.md` | Premise false. The document asserts `IQfcFormController` and `IFilerFormController` are unrelated; `QuickFiler/Controllers/IQfcFormController.cs:13` already inherits `IFilerFormController`, which reduces the defect to a field and constructor retype. |
| `482.md` | Misattributes the divergent expansion registries to `QfcItemController.Navigation.cs`; they are in `QuickFiler/Controllers/QfcItemController.EventWiring.cs:306-389`. |
| `498.md` | Places `BreadcrumbRow.cs` and `BreadcrumbMessageCodec.cs` under `QuickFiler/Controllers/`; both are under `UtilitiesCS/OutlookObjects/Folder/`. |
| `440.md` | Asserts the two breadcrumb surfaces share `BreadcrumbRow`. They do not: the EFC surface uses `BreadcrumbRow`, the QFC surface uses `BreadcrumbStateRow`. |

## Hard Constraints for Children

1. **Do not edit anything under `.claude/**`.** That tree is push-down-owned: a sync overwrites all
   of it (skills, lib, hooks, agents, rules, `settings.json`) plus `config/blast-radius.json` and
   `config/orchestration-routing.json` from an upstream bundle with no merge, so any local edit is
   destroyed. Where an issue cites a rule file, the citation is the policy the fix is measured
   against, not an edit target. Safe to edit: `CLAUDE.md`, `coverage.config`,
   `Directory.Build.targets`, `quality-tiers.yml`, `.github/workflows/**`, `scripts/**`, `tests/**`,
   every C# project, and `.claude/agent-memory/**`.
2. **`vstest` requires `/InIsolation`.** Without it, each assembly's `app.config` binding redirects
   are ignored and roughly 1,695 phantom failures appear with empty messages and sub-millisecond
   durations, surfacing as a Moq `TypeInitializationException` via
   `System.Threading.Tasks.Extensions`. A child that omits the flag will see a fabricated mass
   regression and must not attempt to "fix" it. Use
   `vstest.console.exe <assemblies> /EnableCodeCoverage /InIsolation /TestCaseFilter:"TestCategory!=LiveOutlook"`,
   and exclude `\.claude\` from recursive `*.Test.dll` discovery so stale agent-worktree builds are
   not loaded.
3. **#511 must not delete #571's coverage.** #511's proposed remedy — replacing the real pump with
   an injectable synchronization-context seam — executed literally would delete or reclassify the
   very tests #571 stabilizes, along with the coverage justifications at
   `QuickFiler/Controllers/QfcItemController.Initialization.cs:166, 261, 293, 404, 448` and
   `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs:31, 256`. #571's root cause is narrow:
   `WinFormsPumpHost.RunPumpThread` calls `Application.Run(new ApplicationContext())` and never adds
   a form or control, so no window handle is ever created, and only the two synchronous `Initialize`
   paths reach `Control.Invoke`. The spec must reconcile the two issues and retain the coverage.
4. **No Python toolchain exists.** There is no `scripts/dev_tools/` and no Poetry manifest, so any
   skill step naming `poetry run python -m scripts.dev_tools.*` is unrunnable by absence. Report it
   as such; do not fabricate a result and do not silently skip it. PowerShell equivalents are under
   `.claude/lib/`.
5. **Evidence paths are non-overridable**: `<FEATURE>/evidence/<kind>/` only. No `artifacts/`
   sub-path other than `artifacts/orchestration/` may hold evidence.

## Recorded Preconditions for Later Epics

Recorded here so they are not re-litigated, and deliberately NOT solved by this epic:

- **`QuickFiler/Controllers/QfcCollectionController.cs` is 2,349 lines**, 4.7x the 500-line cap in
  `.claude/rules/general-code-change.md`, with no partial-class siblings. Nine corpus issues target
  it, and only #468 reduces it — to approximately 2,114 lines, still 4.2x. `feature-review` will
  raise the cap violation on every pull request touching the file. **Epic 2
  (`quickfiler-qfc-controllers`) must either carry a ratified exemption or add a partial-class split
  child before its collection-controller work.** `QuickFiler.Test/Controllers/QfcCollectionControllerTests.cs`
  is exactly at the 500-line cap, so every new regression test there needs a new file and therefore
  a new project-file compile entry.
- **`quality-tiers.yml` does not exist at the repository root**, although
  `.claude/rules/quality-tiers.md` states every project must be classified there and that an
  unclassified project fails CI. No QuickFiler tier classification is available to cite. Owned
  outside this epic.
- **The remaining potential-document restoration is in flight.** 61 files under
  `docs/features/potential/promoted/` exist on `origin/epic/quickfiler-per-file-coverage-integration`
  and are absent from `main` (98 versus 55). They are being restored to `main` under a separate pull
  request. Epics 2 through 4 are gated on that landing. No child of this epic may write under
  `docs/features/potential/**`.
- **No in-flight collision hazard.** All 20 `feature/quickfiler-*` branches are reachable from
  `epic/quickfiler-per-file-coverage-integration`, and that branch differs from `origin/main` by
  zero files under `QuickFiler/` and `QuickFiler.Test/`; the per-file-coverage epic's code has
  already landed on `main`. No unmerged non-QuickFiler branch touches QuickFiler.

## Defects Found During Preparation (not fixed here)

Recorded so they are not lost when this epic's feature folders are archived. Neither is in scope
for any child of this epic.

- **A second live WinForms form survives #491, in a different assembly.**
  `UtilitiesCS.Test/ResourceTests.cs:20` executes `Form1 frm = new Form1();` inside `[TestMethod]
  TestMethod1`, resolving to `UtilitiesCS.Test/Form1.cs` and `UtilitiesCS.Test/Form1.Designer.cs` —
  a pair entirely separate from the `QuickFiler.Test/Form1.*` pair that #491 removes. It is the
  exact hazard #491 exists to eliminate (a real form constructed during a unit-test run, which
  `.claude/rules/general-unit-test.md` forbids), and it is untouched by this epic because it lives
  in `UtilitiesCS.Test`. **Promote as its own bug.** Verified by direct read on 2026-08-22.

- **`.claude/agent-memory/` writes by preparation subagents are left uncommitted by design.**
  The `atomic-planner` and `atomic-executor` write to their own memory namespaces during a
  preparation run. Those writes are not feature deliverables and are deliberately not committed
  onto a child branch. Hard Constraint 1 of this manifest lists `.claude/agent-memory/**` as safe
  to edit, so this is a delegation-level choice rather than a violation of that constraint.
