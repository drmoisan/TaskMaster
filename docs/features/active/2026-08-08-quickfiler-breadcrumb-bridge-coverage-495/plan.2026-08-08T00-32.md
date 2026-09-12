# quickfiler-breadcrumb-bridge-coverage — Atomic Implementation Plan

- **Issue:** #495
- **Parent:** epic #136 `quickfiler-per-file-coverage`, child F12
- **Owner:** drmoisan
- **Last Updated:** 2026-08-08T00-32
- **Status:** Draft (preparation mode — authored now, executed later by `epic-orchestrator` in a different worktree)
- **Version:** 1.0
- **Work Mode:** `full-feature` (`spec.md` + `user-story.md` are the authoritative AC sources)
- **Integration branch:** `epic/quickfiler-per-file-coverage-integration`
- **Execution branch:** `feature/quickfiler-breadcrumb-bridge-coverage-r2`
- **Upstream dependency:** F1 (#432) `quickfiler-coverage-denominator-and-exemption-ledger`

## Task Inventory (mechanical count, re-derive after any delta)

| Phase | Title | Tasks |
| --- | --- | --- |
| 0 | Baseline Capture and Upstream Gate | 16 |
| 1 | BreadcrumbCoordinatorUpgradeLifetime.cs | 8 |
| 2 | BreadcrumbBridgeCoordinator.cs | 9 |
| 3 | BreadcrumbMessengerHub.cs | 9 |
| 4 | BreadcrumbBridgeRouter.cs | 9 |
| 5 | BreadcrumbItemViewerLifecycleCoordinator.cs | 33 |
| 6 | Cross-Cutting Acceptance Verification | 12 |
| 7 | Final QC Loop | 12 |
| **Total** | | **108** |

## Path Conventions (read before executing any task)

- **All paths in this plan are repository-relative.** No absolute path appears anywhere in this
  document, and no task may introduce one. Every path resolves from the repository root of whatever
  worktree executes this plan.
- `<FEATURE>` expands to `docs/features/active/2026-08-08-quickfiler-breadcrumb-bridge-coverage-495`.
- `<EPIC>` expands to `docs/features/epics/quickfiler-per-file-coverage`.
- `<ts>` expands to the ISO-8601 timestamp `yyyy-MM-ddTHH-mm` at the moment the task runs.
- Evidence locations are non-overridable and are exactly `<FEATURE>/evidence/baseline/`,
  `<FEATURE>/evidence/qa-gates/`, `<FEATURE>/evidence/regression-testing/`,
  `<FEATURE>/evidence/other/`. **No `artifacts/` sub-path may be used for evidence.**
- Every command-step evidence artifact records `Timestamp:`, `Command:`, `EXIT_CODE:`,
  `Output Summary:`. Every coverage-bearing step additionally records **numeric line AND branch
  percentages** with their raw numerator/denominator pairs.
- **csproj insertion coordinates are pre-insertion.** Every `QuickFiler.Test/QuickFiler.Test.csproj`
  line number cited below (`:58`, `:61`, `:64`, `:87`) is read against the file as it stands at the
  start of Phase 1. Each phase inserts entries, so later coordinates shift. Every csproj acceptance
  in this plan is **content-based** — it names the anchor entry, not the line number. An executor
  must locate by anchor entry text, never by line number, and must not treat a shifted line number
  as a defect.
- **No production `.cs` file is modified by this plan.** Five research artifacts independently
  reached that verdict (`spec.md` §4). Consequently there is no `QuickFiler/QuickFiler.csproj` edit,
  no `<EPIC>/coverage-ledger.md` row under the epic's "Mid-Wave File Creation" rules, and the #457
  measurement trap does not engage. Any task that would touch a production file is mis-scoped.

## Required References

- `CLAUDE.md`
- `.claude/rules/general-code-change.md`
- `.claude/rules/general-unit-test.md`
- `.claude/rules/quality-tiers.md`
- `.claude/rules/tonality.md`
- `<FEATURE>/spec.md` (AC-1 .. AC-16, plus §10 Definition of Done)
- `<FEATURE>/user-story.md` (US-1 .. US-8)
- `<FEATURE>/issue.md` (context; its determinism instruction is struck — see Hard Constraints)
- `<FEATURE>/research/2026-08-08T01-15-breadcrumb-bridge-coordinator.md`
- `<FEATURE>/research/2026-08-08T01-15-breadcrumb-coordinator-upgrade-lifetime.md`
- `<FEATURE>/research/2026-08-08T02-10-breadcrumb-messenger-hub.md`
- `<FEATURE>/research/2026-08-08T02-10-breadcrumb-bridge-router.md`
- `<FEATURE>/research/2026-08-08T02-10-breadcrumb-item-viewer-lifecycle-coordinator.md`
- `<EPIC>/epic.md`

All work must comply with these policies; this plan does not restate their content.

## Gap-Label Namespacing (binding — the research artifacts collide)

Two research artifacts independently use the label `G1`. Every gap reference in this plan carries a
file-scoped prefix, and no task may use a bare research label.

| Prefix | Production file | Research artifact labels |
| --- | --- | --- |
| `UL-H#` | `QuickFiler/Viewers/BreadcrumbCoordinatorUpgradeLifetime.cs` | H1 .. H4 |
| `BC-G#` | `QuickFiler/Viewers/BreadcrumbBridgeCoordinator.cs` | G1 .. G6 |
| `HUB-G#` | `QuickFiler/Viewers/BreadcrumbMessengerHub.cs` | G1 .. G5 |
| `RT-J#` | `QuickFiler/Controllers/BreadcrumbBridgeRouter.cs` | J1 .. J6 |
| `LC-G#` | `QuickFiler/Viewers/BreadcrumbItemViewerLifecycleCoordinator.cs` | G1 .. G11 |

## Upstream Dependency Handling — Halt Gate, Evaluated at Execution Time

`<EPIC>/coverage-ledger.md` and F1's per-file coverage harness **do not exist on the branch this
plan was authored against, and that is expected**. F1 (#432) is being prepared concurrently and
lands on the integration branch before F12 executes.

`[P0-T7]` is therefore an **execution-time** existence test, not a planning-time precondition. When
this plan runs, the executor tests for `<EPIC>/coverage-ledger.md` from the repository root; if the
file is absent at that moment, execution **halts at Phase 0**, no Phase 1 task runs, and the
executor reports `BLOCKED ON F1 (#432)`. Genuine absence at execution time is an epic-orchestrator
sequencing failure raised then, not a defect in this plan.

F1's per-file harness is a **soft** dependency. If the ledger exists but no harness script is
published, every per-file coverage figure in this plan is derived from the Cobertura produced by
`scripts/vscode/Invoke-MSTestWithCoverage.ps1`, applying the Measurement Rules below.

## Measurement Rules (binding on every coverage task in this plan)

1. **Key on the Cobertura `filename=` attribute. Never on `<class name=>`.** This is not theoretical
   for F12. `BreadcrumbCoordinatorUpgradeLifetime.cs`'s sole `<class>` element is named after the
   secondary type `QuickFiler.Viewers.BreadcrumbUpgradeLease`, so a name-keyed harness reports the
   principal type as absent. `BreadcrumbMessengerHub.cs`'s single element covers all 294 lines while
   `BreadcrumbCollapsedAttachment` and `BreadcrumbResourceOwner` have no element of their own — a
   name-keyed harness silently drops 124 of that file's 294 lines.
2. **Sum class-level `<lines>` children only, deduplicated by line number with `max(hits)`.** Never
   sum `<method>` blocks. Never use `class.iter('line')` or an `.//lines/line` axis (#441).
3. **Never read an emitted `line-rate` / `branch-rate` attribute (#478).** Every one of F12's five
   files emits a wrong value, and on `BreadcrumbMessengerHub.cs` the error runs **optimistically**
   (`0.977273` emitted against a true `0.96610`) — the direction that falsely passes a gate. On
   `BreadcrumbBridgeRouter.cs` the emitted `0.926471` encodes `63/68` against a true `83/90`, and an
   unrelated UtilitiesCS type emits `0.922222`, which coincides with this file's *correct* recomputed
   figure to six digits. Compute; do not read.
4. **Repository-wide figures are captured before and after in the same session on the same branch**,
   with an identical command, identical post-processing, and the full `*.Test.dll` set
   (`-SearchRoot '.'`), run from the executing worktree root. **No imported figure from another
   branch or another feature folder is a valid comparison baseline.**

## Irreducible Outcomes — No Task May Target These

| # | Location | Outcome excluded | Proof (`spec.md` §4.1) |
| --- | --- | --- | --- |
| 1 | `QuickFiler/Viewers/BreadcrumbItemViewerLifecycleCoordinator.cs:135` | `_bridgeCoordinator == null` side of the `_rowCount` lambda | every call site of `_rowCount` (`BreadcrumbDropDownOpenCoordinator.cs:193`) is gated on `_isSelectorOpen()` returning true, which a null bridge makes false |
| 2 | `QuickFiler/Viewers/BreadcrumbItemViewerLifecycleCoordinator.cs:138` | `_bridgeCoordinator == null` side of the `_cancelSelector` lambda | every call site (`:144`, `:207`, `:226`, `:265`) is gated the same way |
| 3 | `QuickFiler/Viewers/BreadcrumbItemViewerLifecycleCoordinator.cs:234` | `DropDownHost == null` side | `_openCoordinator` is nulled only inside `ReleaseHostCore`, which either reassigns immediately (`:130`) or runs after `_disposed` is set (`:208`) |
| 4 | `QuickFiler/Controllers/BreadcrumbBridgeRouter.cs:288` condition 1 (RT-J5) | the `leaf?.` null side | `BreadcrumbRowBuilder` cannot produce a segment-less `Suggestion` row; reachable only by reflective `_rows` seeding |
| 5 | `QuickFiler/Controllers/BreadcrumbBridgeRouter.cs:372` condition 1 (RT-J5) | the `row.LeafSegment?` null side | same proof |
| 6 | `QuickFiler/Controllers/BreadcrumbBridgeRouter.cs:372` condition 2 (RT-J5) | the `??` left-is-null side | same proof |
| 7 | `QuickFiler/Controllers/BreadcrumbBridgeRouter.cs:426` loop-exit (RT-J6) | `i >= _rows.Count` | `IndexOf` is only ever called with a row `FindRow` just returned from `_rows`, with no suspension point between; reachable only by a reflective `HandleUpArrow` call |

`QuickFiler/Controllers/BreadcrumbBridgeRouter.cs:434` (`return -1;`) is the uncovered **line**
attached to entry 7 and is likewise excluded.

**Counting note.** `spec.md` §4.1 and AC-16 describe this set as "the six structurally unreachable
outcomes". Enumerated at branch-outcome granularity the same set contains **seven** untaken outcomes
plus one uncovered line, because `spec.md` §4.1 names `:372` c1/c2 as a single item. **Set
membership is identical; only the counting granularity differs.** AC-16 is satisfied by documenting
this exact set with its proofs and by no task targeting any member.

**Consequence for `BreadcrumbBridgeRouter.cs`.** Excluding RT-J5 and RT-J6 means this plan targets
**99.65% line (281/282) and 95.56% branch (86/90)** for that file, not the 100%/100% projected in
`spec.md` §3.1. `spec.md` §3.1 itself states this fallback verbatim ("If the two
reflection-dependent router gaps (J5, J6) are rejected at review, that file lands at 99.65% line /
95.56% branch — still comfortably above both floors"). AC-1 and AC-4 are satisfied; no acceptance
criterion requires 100% on this file.

## Hard Constraints (binding on every task)

- **Determinism — the `issue.md` "injected clock and fake timers" instruction is struck.** All five
  files return zero matches for `DateTime`, `Stopwatch`, `Timer`, `Task.Delay`, `Thread.Sleep`, and
  `TimeProvider`. **No injected clock, no `TimeProvider`, no `FakeTimeProvider`, no fake-timer
  facility** — a clock seam would have nothing behind it. Determinism here is scheduler and
  completion-source control, and for `BreadcrumbBridgeRouter.cs` it is weaker still: already-completed
  task control via Moq `ReturnsAsync` / `ThrowsAsync`. Green in-repo vehicles:
  `BreadcrumbUiDispatcher.CreateForCurrentThreadTests()` (`QuickFiler/Viewers/BreadcrumbUiDispatcher.cs:62`),
  `BreadcrumbBridgeCoordinatorTests.InlineSynchronizationContext` (`:90-93`, restored in `finally` at
  `:95-112`), `BreadcrumbSelectorToggleUiBoundaryTests.CapturingSynchronizationContext` (`:346-401`),
  the private `QueuedCreatorThreadSynchronizationContext` pattern
  (`QuickFiler.Test/Viewers/BreadcrumbItemViewerLifecycleCoordinatorTests.cs:299-325`), and
  test-owned `TaskCompletionSource<T>` gates.
- **Prohibited in every new or modified test:** `Thread.Sleep`, `Task.Delay`, any wall-clock wait,
  real-time polling, temporary files, any filesystem write, external services or processes, the
  WebView2 Evergreen runtime, live or shown forms, `.Show()` / `.ShowDialog()`, popups, mutable
  static state, ordering dependencies between `[TestMethod]`s, **STA attributes, and any
  `*.StaTests.cs` file**.
- **Every ambient `SynchronizationContext` assignment is restored in a `finally`.**
- **No new test file exceeds 500 lines**, measured after `csharpier format`.
- **Frozen signatures.** Four sibling children compile against F12 types (`spec.md` §6.1). F14 has
  issued an explicit FREEZE on `BreadcrumbItemViewerLifecycleCoordinator`'s six-argument constructor
  (`:29-36`) and `BreadcrumbBridgeCoordinator`'s internal three-argument constructor (`:45-59`).
  **Because this plan makes no production edit, the constraint is satisfied by construction**; it is
  stated so a reviewer can see it was considered, and `[P6-T1]` verifies it from the diff.
- **Sibling boundaries — do not edit.** F13 (#455) owns all `BreadcrumbDropDown*`,
  `BreadcrumbPopup*`, `QuickFiler/Viewers/BreadcrumbUiDispatcher.cs`, `WebView2*`, `IWebView*`,
  `QuickFiler/Viewers/BreadcrumbCollapsedSurfaceController.cs`,
  `QuickFiler/Viewers/BreadcrumbWebViewSurfaceFactory.cs`, and their test files — **including
  `QuickFiler.Test/Viewers/BreadcrumbPopupUiOperationsDirectAdapterTests.cs`**. F14 (#456) owns
  `QuickFiler/Viewers/ItemViewer.cs` and every `ItemViewer.*.cs`. `UtilitiesCS/**` belongs to no
  child. F2 (#431) owns `QuickFiler/Controllers/BreadcrumbOutboundQueue.cs`; F8 (#437) owns
  `QuickFiler.Test/Controllers/EfcHomeControllerExecuteMovesTests.cs`.
- **AC-13.** `QuickFiler.Test/Viewers/BreadcrumbCoordinatorLifecycleTests.cs` `ViewerScope`
  (`:469-487`) retains its live `new QuickFiler.ItemViewer()` construction. F14 explicitly requested
  this; no task may replace it with a mock.
- **AC-11.** F12 owns direct tests for `BreadcrumbPopupLifecycleOperations` and
  `BreadcrumbNavigationSubscription`, because five of their eight direct-invocation sites live in the
  F13-owned `BreadcrumbPopupUiOperationsDirectAdapterTests.cs` that F13's plan rewrites. F12's new
  tests must not consume any private helper declared in that file; every double, fake, and pumped
  context is re-declared privately in F12's own files.
- **AC-10 — tests pin CURRENT behavior, never corrected behavior**, for promoted defects **#498,
  #499, #500, #501, #502** and open **#440**. Where current behavior *is* the defect, the path is
  left untested and the reason recorded. Each affected test carries an in-code comment naming the
  issue. The router research explicitly warns that RT-J assertions must be confined so they do not
  pin #498 or #499.
- **csproj mechanics.** `QuickFiler.Test/QuickFiler.Test.csproj` is a non-SDK project with **107
  explicit `<Compile Include>` entries and no globbing**, CRLF on every line, breadcrumb block at
  lines 58-91. Own entries only, minimal adjacent hunks, four-space indentation, **CRLF preserved —
  use the `Edit` tool, never a git-bash `sed -i`**. Additive fan-in conflicts are expected and are
  resolved by keeping both sides. Final entry count after this child: **112**.

## Decisions Record (rationale a reviewer would otherwise re-derive)

- **D-1. Reflection is used in exactly two tests, and both reuse an existing in-repo precedent.**
  `[P2-T3]` (BC-G3) resolves the private `BreadcrumbBridgeCoordinator.PostRenderAndSelectorAsync`,
  reusing the anchor already committed at
  `QuickFiler.Test/Viewers/BreadcrumbCoordinatorLifecycleTests.cs:381-392`. `[P3-T5]` (HUB-G5)
  resolves the protected `BreadcrumbResourceOwner.Dispose(bool)`. No other test in this plan uses
  reflection, and **no reflection reaches an F13-owned private member** — the research proposal to
  read `BreadcrumbDropDownOpenCoordinator._rowCount` / `_cancelSelector` is rejected here because it
  would create a cross-child runtime coupling onto private field names F13's signature-stability
  commitment does not cover, and its two outcomes are in the Irreducible Outcomes table.
- **D-2. RT-J5, RT-J6, LC-G10(d) and LC-G11(b) are not scheduled.** They appear in the Irreducible
  Outcomes table. This costs `BreadcrumbBridgeRouter.cs` 3 branch outcomes and 1 line (final 99.65%
  line / 95.56% branch) and `BreadcrumbItemViewerLifecycleCoordinator.cs` 3 branch outcomes (final
  100.00% line / 97.95% branch, clearing the 75% floor by 22.95 points). Both files pass both floors.
- **D-3. Per-file phases are ordered by ascending shared-csproj impact**, so the highest-conflict
  edit lands last: Phase 1 adds 0 entries (107), Phase 2 adds 1 (108), Phase 3 adds 1 (109), Phase 4
  adds 1 (110), Phase 5 adds 2 (112).
- **D-4. `BreadcrumbCoordinatorUpgradeLifetimeTests.cs` is extended, not superseded.** It is 122
  lines with 4 `[TestMethod]`s and 378 lines of headroom, is already registered at
  `QuickFiler.Test/QuickFiler.Test.csproj:63`, and is the sole driver of roughly 24 lines of
  exception-path coverage in that production file (research §5.2 R1). Extending removes this file
  from the shared-csproj conflict surface entirely and keeps the R1 coverage in the same class as the
  new tests.
- **D-5. Every other new test lives in a new standalone `[TestClass]`, never a `.Part2.cs`
  companion.** All the candidate host classes — `BreadcrumbBridgeCoordinatorTests` (488 lines),
  `BreadcrumbMessengerHubCoverageTests` (478), `BreadcrumbBridgeRouterQueueTests` (446),
  `BreadcrumbItemViewerLifecycleCoordinatorTests` (327),
  `BreadcrumbPopupUiOperationsDirectAdapterTests` (302, **F13-owned**) — are declared non-`partial`,
  so a companion would require editing a class declaration on a fan-in surface F13 and F14 also
  touch. `BreadcrumbBridgeRouterQueueTests.cs` additionally hosts an F2-owned
  `BreadcrumbOutboundQueue` test at `:207-220`.
- **D-6. The lifecycle coordinator needs two new test files, not one.** 26 scheduled test methods
  plus a fixture and a private pumped context would land a single file near 550 lines. The split is
  natural: the coordinator tests need a pumped `SynchronizationContext`, a `BreadcrumbMessengerHub`,
  and a `BreadcrumbCollapsedAttachment`; the static-helper tests need two delegates. Projected sizes
  are ~360 and ~360 lines post-format against the 500 cap.
- **D-7. `[P5-T29]` renames the misnamed existing test
  `BreadcrumbItemViewerLifecycleCoordinatorTests.CandidateFailure_CleansMessengerAndReadiness`
  (`:52`).** It exercises `CreateNavigationSurface`, not `CreateCollapsedCandidate`, and that
  mislabel is the most plausible reason `CreateCollapsedCandidate` sat at 0% coverage undetected. The
  change is test-only, F12-owned, assertion-preserving, and adds no csproj entry.
- **D-8. HUB-G5 closes no current gap and is scheduled anyway.** `BreadcrumbMessengerHub.cs:447`'s
  `disposing == false` arm is reachable only through `Component`'s finalizer and is covered today by
  GC scheduling, not by any test. A different GC schedule silently drops branch coverage to 95.76%
  with no diff to explain it. HUB-G5 converts an incidental outcome into an asserted one and, with
  HUB-G3 and HUB-G4, eliminates rather than monitors risk R1 (13 lines of `BreadcrumbResourceOwner`
  currently held up only by F13/F14-owned `ItemViewer`-constructing tests).
- **D-9. HUB-G1 and HUB-G2 are not merged.** A single factory that both disposes the attachment and
  returns a non-disposable messenger would close both outcomes, but it would report one failure for
  two independent contract regressions.
- **D-10. `NonDisposableMessenger` (HUB-G2) and the `IWebViewMessenger`+`IDisposable` fake
  (LC-G4d) are hand-written doubles, not Moq proxies.** Both assertions turn on the runtime interface
  set of the object, which is an implementation detail of a mocking-library proxy. Moq remains the
  default everywhere else; this is a narrow, documented exception.

---

## Implementation Plan (Atomic Tasks)

### Phase 0 — Baseline Capture and Upstream Gate

- [ ] [P0-T1] Bootstrap the C# toolchain by running `pwsh -NoProfile -File scripts/vscode/Install-RepoDotNetSdk.ps1`, then `dotnet tool restore`, then `dotnet tool install --global dotnet-coverage` (or confirm it already resolves), from the repository root; acceptance: `<FEATURE>/evidence/baseline/toolchain-bootstrap.<ts>.md` records `Timestamp:`, `Command:`, `EXIT_CODE: 0`, and `Output Summary:` for all three plus a resolving `dotnet tool run csharpier --version` reporting 1.2.6 and a resolving `dotnet-coverage --version`, and states that `global.json` pins a repo-local SDK at `.dotnet-sdk` which is absent in a fresh worktree so every later `dotnet` command depends on this task
- [ ] [P0-T2] Restore NuGet packages by running `pwsh -NoProfile -File scripts/vscode/Invoke-Restore.ps1` from the repository root; acceptance: `<FEATURE>/evidence/baseline/nuget-restore.<ts>.md` records `Timestamp:`, `Command:`, `EXIT_CODE: 0`, and `Output Summary:` confirming a populated `packages/` directory, with a note that `packages/` is gitignored and msbuild does not restore `packages.config` projects, so build and test tasks would otherwise fail at `PrepareForBuild`
- [ ] [P0-T3] Resolve the `msbuild.exe` and `vstest.console.exe` paths with `vswhere -latest -products * -find MSBuild\**\Bin\MSBuild.exe` and `vswhere -latest -products * -find Common7\IDE\Extensions\TestPlatform\vstest.console.exe`, because neither tool is on `PATH`; acceptance: `<FEATURE>/evidence/baseline/toolchain-paths.<ts>.md` records both resolved paths, `EXIT_CODE: 0` for both `vswhere` invocations, and the runsettings path `scripts/vscode/TaskMaster.cli.runsettings`
- [ ] [P0-T4] Read `CLAUDE.md`, `.claude/rules/general-code-change.md`, `.claude/rules/general-unit-test.md`, `.claude/rules/quality-tiers.md`, and `.claude/rules/tonality.md` in that order; acceptance: `<FEATURE>/evidence/baseline/phase0-instructions-read.<ts>.md` records `Timestamp:`, `Policy Order:`, and the explicit list of the five files read
- [ ] [P0-T5] Read `<FEATURE>/spec.md`, `<FEATURE>/user-story.md`, and `<FEATURE>/issue.md` and transcribe the acceptance inventory into `<FEATURE>/evidence/baseline/ac-inventory.<ts>.md`; acceptance: the artifact lists AC-1 .. AC-16 from `spec.md` §9, the five `spec.md` §10 Definition of Done items, and US-1 .. US-8 from `user-story.md`, with no gaps and no item invented
- [ ] [P0-T6] Read all five research artifacts under `<FEATURE>/research/` and transcribe the namespaced gap index into `<FEATURE>/evidence/baseline/research-gap-index.<ts>.md`; acceptance: the artifact enumerates `UL-H1..H4`, `BC-G1..G6`, `HUB-G1..G5`, `RT-J1..J6`, `LC-G1..G11`, marks each as `SCHEDULED` or `EXCLUDED`, and cites the Irreducible Outcomes table for every `EXCLUDED` entry (`RT-J5`, `RT-J6`, `LC-G10(d)`, `LC-G11(b)`)
- [ ] [P0-T7] **HALT GATE.** At execution time, verify from the repository root that `<EPIC>/coverage-ledger.md` exists; if it is absent, halt immediately, run no Phase 1 task, and report `BLOCKED ON F1 (#432)`; acceptance: `<FEATURE>/evidence/qa-gates/f1-ledger-halt-gate.<ts>.md` records the tested path, the boolean existence result, and either `GATE: PASS` or `GATE: HALT — BLOCKED ON F1 (#432)`
- [ ] [P0-T8] Read `<EPIC>/coverage-ledger.md` and transcribe verbatim its bucket definitions, its mid-wave file-creation classification rules, and the per-file harness command if one is published; acceptance: `<FEATURE>/evidence/qa-gates/f1-ledger-contract.<ts>.md` records either `HARNESS: <command>` or `HARNESS: ABSENT — using scripts/vscode/Invoke-MSTestWithCoverage.ps1 fallback per Measurement Rules`, and states `LEDGER ROWS REQUIRED BY F12: NONE (no production file created or modified)`
- [ ] [P0-T9] Record the executing branch name, `git rev-parse HEAD`, `git merge-base HEAD origin/epic/quickfiler-per-file-coverage-integration`, and `git status --porcelain`; acceptance: `<FEATURE>/evidence/baseline/tree-state.<ts>.md` records all four values with `EXIT_CODE: 0` and an empty porcelain output
- [ ] [P0-T10] Run `dotnet tool run csharpier check .` from the repository root; acceptance: `<FEATURE>/evidence/baseline/csharpier-check.<ts>.md` records `Timestamp:`, `Command:`, `EXIT_CODE:`, an `Output Summary:` naming any pre-existing unformatted file, and the deviation note that csharpier 1.2.6 requires the `check` / `format` subcommand so the bare `csharpier .` form in `CLAUDE.md` does not work, recorded in evidence rather than by editing `CLAUDE.md`
- [ ] [P0-T11] Run `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` using the path resolved in `[P0-T3]`; acceptance: `<FEATURE>/evidence/baseline/msbuild-analyzers.<ts>.md` records `Timestamp:`, `Command:`, `EXIT_CODE:`, and the baseline error and warning counts in `Output Summary:`
- [ ] [P0-T12] Run `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true` using the path resolved in `[P0-T3]`; acceptance: `<FEATURE>/evidence/baseline/msbuild-nullable.<ts>.md` records `Timestamp:`, `Command:`, `EXIT_CODE:`, and the baseline nullable-diagnostic count in `Output Summary:`
- [ ] [P0-T13] Run `pwsh -NoProfile -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot '.' -Configuration Debug -CoverageOutput '<FEATURE>/evidence/baseline/coverage-baseline.<ts>.cobertura.xml'` from the executing worktree root; acceptance: `<FEATURE>/evidence/baseline/coverage-baseline.<ts>.md` records `Timestamp:`, `Command:`, `EXIT_CODE:`, total/passed/failed test counts, and the repository-wide **numeric line-rate and branch-rate** read from the Cobertura root `<coverage>` element, together with the package count and `lines-valid` so the after-run in `[P7-T5]` can be confirmed like-for-like
- [ ] [P0-T14] Recompute per-file baseline figures for all five in-scope production files from `<FEATURE>/evidence/baseline/coverage-baseline.<ts>.cobertura.xml`, keying on `filename=` and summing deduplicated class-level `<line>` children with `max(hits)`; acceptance: `<FEATURE>/evidence/baseline/per-file-baseline.<ts>.md` reproduces six figures per file — physical lines, coverable lines, line %, branch points, branch %, untaken outcomes — matching `BreadcrumbItemViewerLifecycleCoordinator.cs` 481/318/90.57%/146/66.44%/49, `BreadcrumbBridgeCoordinator.cs` 487/280/100.00%/87/87.36%/11, `BreadcrumbMessengerHub.cs` 456/294/100.00%/118/96.61%/4, `BreadcrumbCoordinatorUpgradeLifetime.cs` 309/204/99.02%/54/92.59%/4, `BreadcrumbBridgeRouter.cs` 450/282/97.87%/90/92.22%/7, and states explicitly that no emitted `line-rate` or `branch-rate` attribute was used and that the `<class name=>` attribute was not used as a key
- [ ] [P0-T15] Record a line-count listing for the five in-scope production files and for every F12-relevant test file under `QuickFiler.Test/Viewers/` and `QuickFiler.Test/Controllers/`; acceptance: `<FEATURE>/evidence/baseline/line-counts.<ts>.md` records `BreadcrumbCoordinatorUpgradeLifetimeTests.cs` at 122, `BreadcrumbItemViewerLifecycleCoordinatorTests.cs` at 327, `BreadcrumbBridgeCoordinatorTests.cs` at 488, `BreadcrumbCoordinatorLifecycleTests.cs` at 489, `BreadcrumbMessengerHubCoverageTests.cs` at 478, `BreadcrumbMessengerHubTests.cs` at 414, `BreadcrumbBridgeRouterTests.cs` at 435, and `BreadcrumbBridgeRouterQueueTests.cs` at 446
- [ ] [P0-T16] Verify and record that every line of `QuickFiler.Test/QuickFiler.Test.csproj` is CRLF-terminated, together with its total line count and its `<Compile Include>` entry count, and that the breadcrumb block spans the entries from `Controllers\BreadcrumbBridgeRouterQueueTests.cs` through `Viewers\FolderBreadcrumbAssetContractTests.cs`; acceptance: `<FEATURE>/evidence/baseline/csproj-crlf.<ts>.md` records full CRLF, **107 entries**, and the four anchor entries this plan inserts against — `Controllers\BreadcrumbBridgeRouterQueueTests.cs`, `Viewers\BreadcrumbBridgeCoordinatorProbabilityTests.cs`, `Viewers\BreadcrumbItemViewerLifecycleCoordinatorTests.cs`, and `Viewers\BreadcrumbMessengerHubCoverageTests.cs`

### Phase 1 — BreadcrumbCoordinatorUpgradeLifetime.cs

- [ ] [P1-T1] Add a `private sealed class CountingCancellationTokenSource : CancellationTokenSource` helper to `QuickFiler.Test/Viewers/BreadcrumbCoordinatorUpgradeLifetimeTests.cs`, overriding `Dispose(bool)` to increment a `DisposeCount` and call `base.Dispose(disposing)`, modelled on the committed `ThrowingCancellationTokenSource` at `:107-120`; acceptance: the file compiles, `Cancel()` is not overridden (it is non-virtual on `CancellationTokenSource`), and the helper is `private` to the existing test class
- [ ] [P1-T2] Add `[TestMethod] LeaseConstructor_NullSource_ThrowsForTheSourceParameter` (UL-H1) to `QuickFiler.Test/Viewers/BreadcrumbCoordinatorUpgradeLifetimeTests.cs`, asserting `new BreadcrumbUpgradeLease(1, null)` throws `ArgumentNullException` `.WithParameterName("source")` plus a positive control that `new BreadcrumbUpgradeLease(7, cts)` yields `Generation == 7` and a `Token` equal to `cts.Token`; acceptance: the test passes and closes `BreadcrumbCoordinatorUpgradeLifetime.cs:16` (AC-1, US-3)
- [ ] [P1-T3] Add `[TestMethod] BeginPopulation_CancellableCallerToken_LinksAndDeactivatesTheLeaseWithoutSuperseding` (UL-H2) to `QuickFiler.Test/Viewers/BreadcrumbCoordinatorUpgradeLifetimeTests.cs`, using two lifetimes and a `using` `CancellationTokenSource`, asserting after `cts.Cancel()` that `linked.Token.IsCancellationRequested` is true, `unlinked.Token.IsCancellationRequested` is false, `IsCurrent(linked)` is false, `TryRunCurrent(linked, …)` returns false with the action not run, `linked.Cancelled` is false, and the report sink is empty; acceptance: the test passes, closes `BreadcrumbCoordinatorUpgradeLifetime.cs:52`, and carries an in-code comment naming **#502** because the discarded-currency-signal path it borders is a promoted defect that must not be pinned as desirable (AC-1, AC-10, US-3)
- [ ] [P1-T4] Add `[TestMethod] Guard_WithoutLease_ReturnsTheActionUnwrappedAndRunsAfterDisposal` (UL-H3) to `QuickFiler.Test/Viewers/BreadcrumbCoordinatorUpgradeLifetimeTests.cs`, asserting `Guard(null, action)` is reference-identical to `action`, `Guard(lease, action)` is not, and that after `TryDispose()` on both lifetimes only the unleased delegate runs (`runs == 1`) with an empty report sink; acceptance: the test passes and closes `BreadcrumbCoordinatorUpgradeLifetime.cs:130` independently of `[P2-T3]` (AC-1, US-3)
- [ ] [P1-T5] Add `[TestMethod] Abandon_CalledTwice_IsIdempotentAndReportsNothing` (UL-H4) to `QuickFiler.Test/Viewers/BreadcrumbCoordinatorUpgradeLifetimeTests.cs`, seeding the lifetime with the existing `SetCurrentLease` reflection helper (`:93-105`) and a `CountingCancellationTokenSource`, calling `Abandon(lease)` twice, and asserting `CancellationStarted`, `Cancelled`, `Settled`, and `SourceDisposed` are all true, `DisposeCount == 1`, and the report sink is empty; acceptance: the test passes and closes `BreadcrumbCoordinatorUpgradeLifetime.cs:266` plus lines `:267` and `:268` (AC-1, AC-2, US-3)
- [ ] [P1-T6] Run `dotnet tool run csharpier format QuickFiler.Test/Viewers/BreadcrumbCoordinatorUpgradeLifetimeTests.cs` and then record the file's post-format line count and `[TestMethod]` count; acceptance: `<FEATURE>/evidence/qa-gates/phase1-file-size.<ts>.md` records `EXIT_CODE: 0`, a line count `<= 500` (projected 212-232 from a 122-line base), exactly 8 `[TestMethod]` declarations, and confirms that **no `QuickFiler.Test/QuickFiler.Test.csproj` edit was required** because the file is already registered
- [ ] [P1-T7] Run the scoped test pass with `& <vstest-path> QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation /TestCaseFilter:"FullyQualifiedName~BreadcrumbCoordinatorUpgradeLifetimeTests"` using the path resolved in `[P0-T3]`; acceptance: `<FEATURE>/evidence/regression-testing/phase1-scoped-run.<ts>.md` records `Timestamp:`, `Command:`, `EXIT_CODE: 0`, and 8 passed / 0 failed
- [ ] [P1-T8] Recompute per-file coverage for `QuickFiler/Viewers/BreadcrumbCoordinatorUpgradeLifetime.cs` from a fresh run of `pwsh -NoProfile -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot '.' -Configuration Debug -CoverageOutput '<FEATURE>/evidence/qa-gates/coverage-phase1.<ts>.cobertura.xml'` under the Measurement Rules; acceptance: `<FEATURE>/evidence/qa-gates/upgradelifetime-coverage.<ts>.md` records **100.00% line (204/204) and 100.00% branch (54/54)** with raw numerator/denominator pairs, states that the file's sole `<class>` element is named `QuickFiler.Viewers.BreadcrumbUpgradeLease` and was selected by `filename=`, and states that no emitted rate attribute was read (AC-1, AC-3, US-1, US-2, US-4)

### Phase 2 — BreadcrumbBridgeCoordinator.cs

- [ ] [P2-T1] Create `QuickFiler.Test/Viewers/BreadcrumbBridgeCoordinatorGuardTests.cs` as a new standalone `[TestClass]` (not `partial`, no `.Part2.cs`) containing a compact private harness modelled on `BreadcrumbBridgeCoordinatorProbabilityTests.cs:142-152` (`Mock<IWebViewMessenger>` + `Mock<IFolderHierarchyProvider>` + `BreadcrumbUiDispatcher.CreateForCurrentThreadTests()`) plus `[TestMethod] InternalConstructor_NullArgument_ThrowsForTheExpectedParameter` (BC-G1) invoking the internal three-argument constructor three times with exactly one null each and asserting `ArgumentNullException` `.WithParameterName("messenger" | "provider" | "dispatcher")`, and add `<Compile Include="Viewers\BreadcrumbBridgeCoordinatorGuardTests.cs" />` immediately after the `Viewers\BreadcrumbBridgeCoordinatorProbabilityTests.cs` entry in `QuickFiler.Test/QuickFiler.Test.csproj` using the `Edit` tool with four-space indentation and CRLF preserved; acceptance: the test passes, closes `BreadcrumbBridgeCoordinator.cs:51`, `:52`, `:55`, pins the guard ordering messenger-before-provider-before-dispatcher, and the csproj entry count is 108 (AC-1, AC-15, US-3)
- [ ] [P2-T2] Add `[TestMethod] NullCollectionArguments_ThrowBeforeAnyLeaseOrPost` (BC-G2) to `QuickFiler.Test/Viewers/BreadcrumbBridgeCoordinatorGuardTests.cs`, asserting `SetSuggestionsAsync(null, CancellationToken.None)` faults with `ArgumentNullException` `.WithParameterName("rows")` and `AddItems(null)` throws with `.WithParameterName("items")`, and additionally that `IWebViewMessenger.PostJson` was never invoked so the guard is proven to precede `BeginPopulation`; acceptance: the test passes and closes `BreadcrumbBridgeCoordinator.cs:94` and `:133` (AC-1, US-3)
- [ ] [P2-T3] Add `[TestMethod] PostRenderAndSelectorAsync_NoLease_PublishesUnconditionally` (BC-G3) to `QuickFiler.Test/Viewers/BreadcrumbBridgeCoordinatorGuardTests.cs`, resolving the private instance method `PostRenderAndSelectorAsync` with `BindingFlags.Instance | BindingFlags.NonPublic` exactly as the committed precedent at `QuickFiler.Test/Viewers/BreadcrumbCoordinatorLifecycleTests.cs:381-392` does, invoking it with `new object[] { "render", null, null }`, and asserting the returned task is completed and `PostJson` received exactly `"render"`; acceptance: the test passes, closes `BreadcrumbBridgeCoordinator.cs:262` condition 0, and carries an in-code comment naming **#500** because the guarded action it drives executes while `BreadcrumbCoordinatorUpgradeLifetime._sync` is held and that behavior is pinned as current, not endorsed (AC-1, AC-10, US-3, US-5)
- [ ] [P2-T4] Add `[TestMethod] RouterSelectionOutput_WithNoSubscriber_StillPostsAndUpdatesSelection` (BC-G4) to `QuickFiler.Test/Viewers/BreadcrumbBridgeCoordinatorGuardTests.cs`, populating the coordinator with `AddItems(new[] { "A", "B" })`, **not** subscribing to `SelectionChanged`, raising `MessageReceived` with `{"type":"selectionChange","rowIndex":0}`, awaiting `LastDispatch`, and asserting no exception, the selection message was posted back, and `GetSelectedFolder()` reflects the new row; acceptance: the test passes and closes `BreadcrumbBridgeCoordinator.cs:382` (AC-1, US-3)
- [ ] [P2-T5] Add `[TestMethod] InboundSelectorViewMessage_MatchesNoSelectorArmAndIsIgnored` (BC-G5) to `QuickFiler.Test/Viewers/BreadcrumbBridgeCoordinatorGuardTests.cs`, raising `MessageReceived` with `{"type":"selectorView","mode":"collapsed","isOpen":false}`, awaiting `LastDispatch`, and asserting the dispatch completed non-faulted, `PostJson` was never invoked, and `IsSelectorOpen`, `CommittedIdentity`, and `PendingIdentity` are unchanged; acceptance: the test passes and closes `BreadcrumbBridgeCoordinator.cs:397` condition 3 (the no-arm-matched side) (AC-1, US-3)
- [ ] [P2-T6] Add `[DataTestMethod] MalformedTypeToken_IsNotTreatedAsASelectorMessage` (BC-G6) with three `[DataRow]`s — `{"type"}`, `{"type":5}`, and `{"type":"` — to `QuickFiler.Test/Viewers/BreadcrumbBridgeCoordinatorGuardTests.cs`, asserting for each input that no selector state changed and the router surfaced a `BridgeErrorMessage` on the outbound channel, matching the contract already asserted for `"{oops"` at `BreadcrumbBridgeCoordinatorTests.cs:221-227`; acceptance: all three rows pass, they close `BreadcrumbBridgeCoordinator.cs:441`, `:442`, and `:443`, and the method carries an in-code comment naming **#440** because the path traverses `RaiseSyntheticArrowKey` whose semantics #440 will rewrite (AC-1, AC-10, US-3, US-5)
- [ ] [P2-T7] Run `dotnet tool run csharpier format QuickFiler.Test/Viewers/BreadcrumbBridgeCoordinatorGuardTests.cs`, then verify the post-format line count and the csproj edit; acceptance: `<FEATURE>/evidence/qa-gates/phase2-file-size-and-csproj.<ts>.md` records `EXIT_CODE: 0`, a line count `<= 500` (projected 200-230), 6 `[TestMethod]`/`[DataTestMethod]` declarations with 9 executions, a csproj entry count of 108, full CRLF, and a diff hunk confined to the single inserted line adjacent to the `Viewers\BreadcrumbBridgeCoordinatorProbabilityTests.cs` anchor (AC-7, AC-15)
- [ ] [P2-T8] Run the scoped test pass with `& <vstest-path> QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation /TestCaseFilter:"FullyQualifiedName~BreadcrumbBridgeCoordinatorGuardTests|FullyQualifiedName~BreadcrumbBridgeCoordinatorTests|FullyQualifiedName~BreadcrumbBridgeCoordinatorProbabilityTests"`; acceptance: `<FEATURE>/evidence/regression-testing/phase2-scoped-run.<ts>.md` records `Timestamp:`, `Command:`, `EXIT_CODE: 0`, 9 new executions passed, and zero failures across the pre-existing coordinator tests
- [ ] [P2-T9] Recompute per-file coverage for `QuickFiler/Viewers/BreadcrumbBridgeCoordinator.cs` from a fresh run of `pwsh -NoProfile -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot '.' -Configuration Debug -CoverageOutput '<FEATURE>/evidence/qa-gates/coverage-phase2.<ts>.cobertura.xml'` under the Measurement Rules; acceptance: `<FEATURE>/evidence/qa-gates/bridgecoordinator-coverage.<ts>.md` records **100.00% line (280/280) and 100.00% branch (87/87)** with raw pairs and states that no emitted rate attribute was read (AC-1, AC-3, US-1, US-4)

### Phase 3 — BreadcrumbMessengerHub.cs

- [ ] [P3-T1] Create `QuickFiler.Test/Viewers/BreadcrumbHubDisposalContractTests.cs` as a new standalone `[TestClass]` containing an F12-local `TrackingMessenger` (implementing `IWebViewMessenger` and `IDisposable` with subscribe/post/dispose counters) and a local `Readiness(...)` helper mirroring the shape at `BreadcrumbMessengerHubCoverageTests.cs:390-406`, plus `[TestMethod] AttachAsync_FactoryDisposesAttachment_ReleasesCandidateWithoutAttaching` (HUB-G1) whose factory calls `attachment.Dispose()` before returning a candidate, asserting the awaited result is false, the candidate surface was disposed exactly once, the readiness lease's detach callback fired exactly once, the hub was never asked to attach, and a following `AttachAsync` throws `ObjectDisposedException`, and add `<Compile Include="Viewers\BreadcrumbHubDisposalContractTests.cs" />` immediately after the `Viewers\BreadcrumbMessengerHubCoverageTests.cs` entry in `QuickFiler.Test/QuickFiler.Test.csproj` using the `Edit` tool with four-space indentation and CRLF preserved; acceptance: the test passes, closes `BreadcrumbMessengerHub.cs:326` condition 0, and the csproj entry count is 109 (AC-1, AC-15, US-3)
- [ ] [P3-T2] Add `[TestMethod] AttachAsync_StaleCandidateWithNonDisposableMessenger_StillReleasesReadinessLease` (HUB-G2) to `QuickFiler.Test/Viewers/BreadcrumbHubDisposalContractTests.cs` with a hand-written `private sealed class NonDisposableMessenger : IWebViewMessenger` (per D-10) and a factory that calls `attachment.Reset()` before returning that messenger, asserting the task completes false, no exception is thrown, the readiness lease was still disposed, and a subsequent `AttachAsync` with a disposable candidate still succeeds; acceptance: the test passes and closes `BreadcrumbMessengerHub.cs:329`, and it is a separate declaration from HUB-G1 per D-9 (AC-1, US-3)
- [ ] [P3-T3] Add `[TestMethod] ResourceOwner_NullDisposalCallback_ThrowsForTheExpectedParameter` (HUB-G3) to `QuickFiler.Test/Viewers/BreadcrumbHubDisposalContractTests.cs`, asserting `new BreadcrumbResourceOwner(null)` throws `ArgumentNullException` whose `ParamName` is `"dispose"`; acceptance: the test passes and closes `BreadcrumbMessengerHub.cs:442` (AC-1, US-3)
- [ ] [P3-T4] Add `[TestMethod] ResourceOwner_DoubleDispose_RunsTheCallbackExactlyOnce` (HUB-G4) to `QuickFiler.Test/Viewers/BreadcrumbHubDisposalContractTests.cs`, constructing `new BreadcrumbResourceOwner(() => calls++)`, calling `Dispose()` twice, and asserting `calls == 1`; acceptance: the test passes and closes `BreadcrumbMessengerHub.cs:451` (AC-1, US-3)
- [ ] [P3-T5] Add `[TestMethod] ResourceOwner_FinalizerPath_DoesNotRunTheManagedCallback` (HUB-G5) to `QuickFiler.Test/Viewers/BreadcrumbHubDisposalContractTests.cs`, resolving `Dispose(bool)` via `typeof(BreadcrumbResourceOwner).GetMethod("Dispose", BindingFlags.Instance | BindingFlags.NonPublic, null, new[] { typeof(bool) }, null)` and invoking it with `false`, then calling the public `Dispose()`, asserting the callback did not run for the `false` invocation and ran exactly once afterwards; acceptance: the test passes, hardens `BreadcrumbMessengerHub.cs:447` so its `disposing == false` arm no longer depends on garbage-collection timing (D-8, risk R3), and uses no `GC.Collect`, no wall-clock wait, and no polling (AC-1, AC-6, US-3)
- [ ] [P3-T6] Run `dotnet tool run csharpier format QuickFiler.Test/Viewers/BreadcrumbHubDisposalContractTests.cs`, then verify the post-format line count and the csproj edit; acceptance: `<FEATURE>/evidence/qa-gates/phase3-file-size-and-csproj.<ts>.md` records `EXIT_CODE: 0`, a line count `<= 500` (projected 150-190), 5 `[TestMethod]` declarations, a csproj entry count of 109, full CRLF, and a diff hunk confined to the single inserted line adjacent to the `Viewers\BreadcrumbMessengerHubCoverageTests.cs` anchor (AC-7, AC-15)
- [ ] [P3-T7] Run the scoped test pass with `& <vstest-path> QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation /TestCaseFilter:"FullyQualifiedName~BreadcrumbHubDisposalContractTests|FullyQualifiedName~BreadcrumbMessengerHubTests|FullyQualifiedName~BreadcrumbMessengerHubCoverageTests"`; acceptance: `<FEATURE>/evidence/regression-testing/phase3-scoped-run.<ts>.md` records `Timestamp:`, `Command:`, `EXIT_CODE: 0`, 5 new tests passed, and zero failures across the 22 pre-existing hub tests
- [ ] [P3-T8] Recompute per-file coverage for `QuickFiler/Viewers/BreadcrumbMessengerHub.cs` from a fresh run of `pwsh -NoProfile -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot '.' -Configuration Debug -CoverageOutput '<FEATURE>/evidence/qa-gates/coverage-phase3.<ts>.cobertura.xml'` under the Measurement Rules; acceptance: `<FEATURE>/evidence/qa-gates/messengerhub-coverage.<ts>.md` records **100.00% line (294/294) and 100.00% branch (118/118)** with raw pairs, records the per-type attribution (hub 170 lines, `BreadcrumbCollapsedAttachment` 111, `BreadcrumbResourceOwner` 13) proving the `filename=` key captured all three types, and states that the emitted `branch-rate="0.977273"` was not read (AC-1, AC-3, US-1, US-4)
- [ ] [P3-T9] Verify the AC-14 retain-or-improve fixture guards for this file by confirming that `QuickFiler.Test/Viewers/BreadcrumbSelectorCoordinatorTests.cs` and `QuickFiler.Test/Viewers/BreadcrumbDuplicateIdentityIntegrationTests.cs` still construct a real `BreadcrumbMessengerHub`, and that `QuickFiler.Test/Viewers/BreadcrumbMessengerHubTests.cs` and `QuickFiler.Test/Viewers/BreadcrumbMessengerHubCoverageTests.cs` still construct `BreadcrumbCollapsedAttachment`; acceptance: `<FEATURE>/evidence/qa-gates/ac14-hub-fixture-retention.<ts>.md` records one grep result per file with its construction-site line numbers, confirms none of the four files was modified by this child, and states the measured consequence of loss (hub falls to 62.24% line / 52.54% branch if `BreadcrumbCollapsedAttachment` coverage is lost) (AC-14)

### Phase 4 — BreadcrumbBridgeRouter.cs

- [ ] [P4-T1] Create `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterEdgeTests.cs` as a new standalone `[TestClass]` containing a local harness modelled on `BreadcrumbBridgeRouterQueueTests.cs:37-74` (`Mock<IFolderHierarchyProvider>` + `Mock<IBreadcrumbWebHost>` + real codec, renderer, and outbound queue, with `_navigated` / `_posted` capture lists) plus `[TestMethod] BindRows_WithNullPresentedRow_SkipsChainLookupAndStillBindsEveryRow` (RT-J1a) binding `new string[] { null, LeafPath }` and asserting no exception, `ResolveLeafKeyAsync` invoked `Times.Once`, and the rendered document contains both `row-0` and `row-1`, and add `<Compile Include="Controllers\BreadcrumbBridgeRouterEdgeTests.cs" />` immediately **before** the `Controllers\BreadcrumbBridgeRouterQueueTests.cs` entry in `QuickFiler.Test/QuickFiler.Test.csproj` (preserving the block's alphabetical order) using the `Edit` tool with four-space indentation and CRLF preserved; acceptance: the test passes, closes `BreadcrumbBridgeRouter.cs:90` condition 0, asserts only provider call counts and rendered row structure so it does not pin **#499**, carries an in-code comment naming #499, and the csproj entry count is 110 (AC-1, AC-10, AC-15, US-3, US-5)
- [ ] [P4-T2] Add `[TestMethod] BindRows_WithRepeatedSuggestionText_ResolvesTheChainOnlyOnce` (RT-J1b) to `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterEdgeTests.cs`, binding `new[] { LeafPath, LeafPath }` and asserting `ResolveLeafKeyAsync` and `GetAncestorChainAsync` were each invoked `Times.Once` while the document contains both `row-0` and `row-1` each rendering the full chain; acceptance: the test passes, closes `BreadcrumbBridgeRouter.cs:90` condition 1, and carries an in-code comment naming **#499** confirming its assertions are confined to call counts and rendered structure (AC-1, AC-10, US-3)
- [ ] [P4-T3] Add `[TestMethod] BindRows_WhenChainFetchFaults_DegradesThatRowAndCompletesTheBind` (RT-J2) to `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterEdgeTests.cs`, setting up `GetAncestorChainAsync` to throw `InvalidOperationException("hierarchy unavailable")` for the first path and return a normal two-segment chain for the second (discriminating on `k.FolderPath` as `BreadcrumbBridgeRouterTests.cs:285-292` already does), asserting `BindRowsAsync` does not throw, `_navigated` has exactly one entry, the failing row renders leaf-only while the healthy row renders its full chain, and `_posted` contains no error payload; acceptance: the test passes and closes lines `BreadcrumbBridgeRouter.cs:356`, `:357`, `:359`, `:360` (AC-1, US-3)
- [ ] [P4-T4] Add `[TestMethod] HostMessageReceived_WithValidPayload_RoutesToSelectionWithoutThrowing` (RT-J3) to `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterEdgeTests.cs`, raising `_host.Raise(h => h.MessageReceived += null, _host.Object, "{\"type\":\"rowSelected\",\"rowId\":\"row-0\"}")` on a freshly bound, initialized host and asserting `SelectedFolderPath` equals the leaf path, the `SelectedFolderPathChanged` event fired once, and `_posted` contains a `"type":"render"` payload, with no pump, no polling, and no `SynchronizationContext` install because a `rowSelected` payload traverses `ProcessInboundAsync` with no executed `await`; acceptance: the test passes, closes line `BreadcrumbBridgeRouter.cs:192`, asserts only against a freshly bound row set so it does not pin the stale-selection behavior of **#499**, and carries an in-code comment naming #499 (AC-1, AC-6, AC-10, US-3, US-5)
- [ ] [P4-T5] Add `[DataTestMethod] LeafExpandToggle_OnBannerOrTrashRow_IsANoOpWithoutProviderQuery` (RT-J4) with two `[DataRow]`s (`"row-0"` banner, `"row-1"` trash) to `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterEdgeTests.cs`, binding `{ "==== SUGGESTIONS ====", BreadcrumbRowBuilder.TrashRowText, LeafPath }` and asserting `_posted.Count` is unchanged, `GetImmediateSubfoldersAsync` was invoked `Times.Never`, and `SelectedFolderPath` is still null; acceptance: both rows pass, they close `BreadcrumbBridgeRouter.cs:288` condition 0, and the method carries an in-code comment naming **#440** recording that the guard behavior asserted here survives #440's arrow-key rewrite while the Left/Right semantics deliberately are not extended (AC-1, AC-10, US-3, US-5)
- [ ] [P4-T6] Record the RT-J5 and RT-J6 exclusion decision and the #498/#499 assertion confinement for this file; acceptance: `<FEATURE>/evidence/qa-gates/router-excluded-outcomes.<ts>.md` names `BreadcrumbBridgeRouter.cs:288` c1, `:372` c1, `:372` c2, `:426` loop-exit, and line `:434` with their reachability proofs, states `NO TASK TARGETS THESE`, records the resulting targets 99.65% line / 95.56% branch against the 80%/75% floors, and lists which promoted defect each new test is confined away from (#498 out-of-range `segmentIndex` at `:169`; #499 stale `SelectedFolderPath` after re-bind at `:114`) (AC-10, AC-16, US-5, US-6)
- [ ] [P4-T7] Run `dotnet tool run csharpier format QuickFiler.Test/Controllers/BreadcrumbBridgeRouterEdgeTests.cs`, then verify the post-format line count and the csproj edit; acceptance: `<FEATURE>/evidence/qa-gates/phase4-file-size-and-csproj.<ts>.md` records `EXIT_CODE: 0`, a line count `<= 500` (projected 150-190 for the five scheduled declarations), 5 declarations with 6 executions, a csproj entry count of 110, full CRLF, and a diff hunk confined to the single inserted line adjacent to the `Controllers\BreadcrumbBridgeRouterQueueTests.cs` anchor (AC-7, AC-15)
- [ ] [P4-T8] Run the scoped test pass with `& <vstest-path> QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation /TestCaseFilter:"FullyQualifiedName~BreadcrumbBridgeRouterEdgeTests|FullyQualifiedName~BreadcrumbBridgeRouterTests|FullyQualifiedName~BreadcrumbBridgeRouterQueueTests"`; acceptance: `<FEATURE>/evidence/regression-testing/phase4-scoped-run.<ts>.md` records `Timestamp:`, `Command:`, `EXIT_CODE: 0`, 6 new executions passed, and zero failures across the 30 pre-existing router tests
- [ ] [P4-T9] Recompute per-file coverage for `QuickFiler/Controllers/BreadcrumbBridgeRouter.cs` from a fresh run of `pwsh -NoProfile -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot '.' -Configuration Debug -CoverageOutput '<FEATURE>/evidence/qa-gates/coverage-phase4.<ts>.cobertura.xml'` under the Measurement Rules; acceptance: `<FEATURE>/evidence/qa-gates/bridgerouter-coverage.<ts>.md` records **99.65% line (281/282) and 95.56% branch (86/90)** with raw pairs, confirms the `<class>` element was selected by `filename="QuickFiler\Controllers\BreadcrumbBridgeRouter.cs"` and not by class name or rate (the UtilitiesCS `FolderBreadcrumbBridgeRouter` emits a numerically confusable `0.922222`), and states that the emitted `branch-rate="0.926471"` was not read (AC-1, AC-3, AC-4, US-1, US-4)

### Phase 5 — BreadcrumbItemViewerLifecycleCoordinator.cs

- [ ] [P5-T1] Create `QuickFiler.Test/Viewers/BreadcrumbItemViewerLifecycleCoordinatorGuardTests.cs` as a new standalone `[TestClass]` containing an F12-local fixture (real `BreadcrumbMessengerHub`, `BreadcrumbCollapsedAttachment`, three no-op delegates) and an F12-local `private sealed class QueuedCreatorThreadSynchronizationContext` with `DrainOnCreatorThread()` replicating the pattern at `BreadcrumbItemViewerLifecycleCoordinatorTests.cs:299-325` (declared privately, never consuming the F13-owned copy), plus `[DataTestMethod] Constructor_NullArgument_ThrowsForTheExpectedParameterInDeclarationOrder` (LC-G1) with six `[DataRow]`s asserting `ArgumentNullException` `.WithParameterName("hub" | "collapsedAttachment" | "operations" | "selectionChanged" | "folderArrow" | "unhandledArrow")`, and add `<Compile Include="Viewers\BreadcrumbItemViewerLifecycleCoordinatorGuardTests.cs" />` immediately after the `Viewers\BreadcrumbItemViewerLifecycleCoordinatorTests.cs` entry in `QuickFiler.Test/QuickFiler.Test.csproj` using the `Edit` tool with four-space indentation and CRLF preserved; acceptance: all six rows pass, they close `BreadcrumbItemViewerLifecycleCoordinator.cs:38`, `:39`, `:41`, `:42`, `:43`, `:44` and pin the guard ordering, and the csproj entry count is 111 (AC-1, AC-2, AC-15, US-3)
- [ ] [P5-T2] Add `[TestMethod] SetBridgeCoordinator_Null_ThrowsBeforeAnySubscription` (LC-G2a) to `QuickFiler.Test/Viewers/BreadcrumbItemViewerLifecycleCoordinatorGuardTests.cs` asserting `ArgumentNullException` `.WithParameterName("bridgeCoordinator")`; acceptance: the test passes and closes `BreadcrumbItemViewerLifecycleCoordinator.cs:65` (AC-1, AC-2, US-3)
- [ ] [P5-T3] Add `[TestMethod] AttachCollapsedWithReadinessAsync_NullArguments_ThrowWithParameterNames` (LC-G2b) to `QuickFiler.Test/Viewers/BreadcrumbItemViewerLifecycleCoordinatorGuardTests.cs` asserting `ArgumentNullException` `.WithParameterName("messenger")` and `.WithParameterName("readiness")`; acceptance: the test passes and closes `BreadcrumbItemViewerLifecycleCoordinator.cs:93` and `:94` (AC-1, AC-2, US-3)
- [ ] [P5-T4] Add `[TestMethod] ConfigureHost_NullArguments_ThrowBeforeAnythingIsPosted` (LC-G2c) to `QuickFiler.Test/Viewers/BreadcrumbItemViewerLifecycleCoordinatorGuardTests.cs` asserting `ArgumentNullException` `.WithParameterName("host" | "anchorBounds" | "workingArea")` **and** that the pumped context's queue is still empty after each throwing call, so a guard placed after the post would fail; acceptance: the test passes and closes `BreadcrumbItemViewerLifecycleCoordinator.cs:115`, `:116`, `:117` (AC-1, AC-2, US-3)
- [ ] [P5-T5] Add `[TestMethod] Focus_NullAction_ThrowsBeforeAnythingIsPosted` (LC-G2d) to `QuickFiler.Test/Viewers/BreadcrumbItemViewerLifecycleCoordinatorGuardTests.cs` asserting `ArgumentNullException` `.WithParameterName("focus")` and an empty context queue afterwards; acceptance: the test passes and closes `BreadcrumbItemViewerLifecycleCoordinator.cs:165` (AC-1, AC-2, US-3)
- [ ] [P5-T6] Add `[TestMethod] SetTheme_WithNoBridgeAndNoHost_IsASilentNoOp` (LC-G10a) to `QuickFiler.Test/Viewers/BreadcrumbItemViewerLifecycleCoordinatorGuardTests.cs`, calling `SetTheme("dark")` on a freshly constructed coordinator before any `SetBridgeCoordinator` or `ConfigureHost`, and asserting no exception is thrown and nothing is posted; acceptance: the test passes, closes `BreadcrumbItemViewerLifecycleCoordinator.cs:158` and `:159`, and carries an in-code comment naming **#488** because this null-tolerance is what turns #488 Defect 2's race into a silently lost theme rather than a crash and must be pinned as current behavior (AC-1, AC-2, AC-10, US-3, US-5)
- [ ] [P5-T7] Add `[TestMethod] CurrentOpenTask_WithNoHost_IsAlreadyCompletedFalse` (LC-G10b) to `QuickFiler.Test/Viewers/BreadcrumbItemViewerLifecycleCoordinatorGuardTests.cs`, reading `CurrentOpenTask` on a fresh coordinator and again after `ConfigureHost` plus a drain, asserting the property is never null and that on a fresh coordinator the task satisfies `IsCompleted` with `Result == false`; acceptance: the test passes and closes both `BreadcrumbItemViewerLifecycleCoordinator.cs:56` outcomes (AC-1, AC-2, US-3)
- [ ] [P5-T8] Add `[TestMethod] ConfigureHost_ThenReset_DiscardsTheQueuedHostConfiguration` (LC-G10c-i) to `QuickFiler.Test/Viewers/BreadcrumbItemViewerLifecycleCoordinatorGuardTests.cs`, calling `ConfigureHost(host, …)`, then `Reset()`, then draining, and asserting the recording host's `PopupMessengerReady` was never subscribed; acceptance: the test passes and closes `BreadcrumbItemViewerLifecycleCoordinator.cs:122` plus lines `:123` and `:124` (AC-1, AC-2, US-3)
- [ ] [P5-T9] Add `[TestMethod] ConfigureHost_ThenDispose_DiscardsTheQueuedHostConfiguration` (LC-G10c-ii) to `QuickFiler.Test/Viewers/BreadcrumbItemViewerLifecycleCoordinatorGuardTests.cs`, calling `ConfigureHost(host, …)`, then `Dispose()`, then draining, and asserting no subscription occurred; acceptance: the test passes and closes the `_disposed == true` short-circuit at `BreadcrumbItemViewerLifecycleCoordinator.cs:319` (AC-1, AC-2, US-3)
- [ ] [P5-T10] Add `[TestMethod] SelectorOpenStateChanged_WithNoHost_IsIgnored` (LC-G10e) to `QuickFiler.Test/Viewers/BreadcrumbItemViewerLifecycleCoordinatorGuardTests.cs`, driving a selector-open transition on a coordinator with a bridge but no configured host and asserting no exception and no post; acceptance: the test passes and closes the null side of `BreadcrumbItemViewerLifecycleCoordinator.cs:222` (AC-1, AC-2, US-3)
- [ ] [P5-T11] Add `[TestMethod] SetDroppedDown_WithHostButNoBridge_DoesNotOpenThePopup` (LC-G11a) to `QuickFiler.Test/Viewers/BreadcrumbItemViewerLifecycleCoordinatorGuardTests.cs`, calling `ConfigureHost(host, …)` on a fixture that never calls `SetBridgeCoordinator`, draining, calling `SetDroppedDown(true, focus)`, draining again, and asserting the host's `OpenAsync` was never called and nothing threw; acceptance: the test passes and closes the null side of `BreadcrumbItemViewerLifecycleCoordinator.cs:137` (AC-1, AC-2, US-3)
- [ ] [P5-T12] Run `dotnet tool run csharpier format QuickFiler.Test/Viewers/BreadcrumbItemViewerLifecycleCoordinatorGuardTests.cs` and record the post-format line count and declaration count; acceptance: `<FEATURE>/evidence/qa-gates/phase5-guard-file-size.<ts>.md` records `EXIT_CODE: 0`, a line count `<= 500` (projected ~360), exactly 11 `[TestMethod]`/`[DataTestMethod]` declarations with 16 executions, and confirms the file declares its own private pumped `SynchronizationContext` rather than referencing any F13-owned helper (AC-7, AC-11)
- [ ] [P5-T13] Create `QuickFiler.Test/Viewers/BreadcrumbPopupLifecycleOperationsTests.cs` as a new standalone `[TestClass]` containing an F12-local recording `NavigationSubscriptionFactory` that captures the navigate, detach, and owner-disposed callbacks handed to it (replacing, not consuming, the F13-owned private `RecordingNavigationBinding`), plus `[TestMethod] NavigationSubscription_NullDetach_Throws` (LC-G3a) asserting `new BreadcrumbNavigationSubscription(null)` throws `ArgumentNullException` `.WithParameterName("detach")`, and add `<Compile Include="Viewers\BreadcrumbPopupLifecycleOperationsTests.cs" />` immediately after the entry inserted by `[P5-T1]` in `QuickFiler.Test/QuickFiler.Test.csproj` using the `Edit` tool with four-space indentation and CRLF preserved; acceptance: the test passes, closes `BreadcrumbItemViewerLifecycleCoordinator.cs:343`, and the csproj entry count is 112 (AC-1, AC-11, AC-15, US-3, US-8)
- [ ] [P5-T14] Add `[TestMethod] NavigationSubscription_DisposedTwice_InvokesDetachOnce` (LC-G9) to `QuickFiler.Test/Viewers/BreadcrumbPopupLifecycleOperationsTests.cs`, constructing a subscription over a counting detach action, calling `Dispose()` twice, and asserting the count is 1; acceptance: the test passes and closes the null side of `BreadcrumbItemViewerLifecycleCoordinator.cs:349`, justifying the `Interlocked.Exchange` at `:348` (AC-1, AC-11, US-3, US-8)
- [ ] [P5-T15] Add `[TestMethod] CreateNavigationSurface_NullArguments_ThrowWithoutDisposingTheLease` (LC-G3b) to `QuickFiler.Test/Viewers/BreadcrumbPopupLifecycleOperationsTests.cs` asserting `ArgumentNullException` `.WithParameterName("readiness")` and `.WithParameterName("createMessenger")`, and additionally that after the `createMessenger`-null throw the caller-supplied lease is untouched (`readiness.Completion.IsCanceled` is false), proving the guard precedes the `try`; acceptance: the test passes and closes `BreadcrumbItemViewerLifecycleCoordinator.cs:362` and `:363` (AC-1, AC-11, US-3)
- [ ] [P5-T16] Add `[TestMethod] CreateNavigationSurface_Success_ReturnsTheLeaseCompletionAndRetainsIt` (LC-G5a) to `QuickFiler.Test/Viewers/BreadcrumbPopupLifecycleOperationsTests.cs`, supplying a factory that returns a fake messenger and asserting the returned tuple's second element is the lease's own `Completion` task and the lease is not disposed; acceptance: the test passes and closes the non-null side of `BreadcrumbItemViewerLifecycleCoordinator.cs:366` plus lines `:371` and `:378` (AC-1, AC-11, US-3)
- [ ] [P5-T17] Add `[TestMethod] CreateNavigationSurface_NullMessenger_DisposesTheLease` (LC-G5b) to `QuickFiler.Test/Viewers/BreadcrumbPopupLifecycleOperationsTests.cs`, supplying a factory that returns `null`, and asserting the thrown message is `"Popup navigation did not provide a messenger."` and `readiness.Completion.IsCanceled` is true; acceptance: the test passes and closes the null side of `BreadcrumbItemViewerLifecycleCoordinator.cs:366` (AC-1, AC-11, US-3)
- [ ] [P5-T18] Add `[TestMethod] CreateCollapsedCandidate_NullArguments_ThrowWithParameterNames` (LC-G3c) to `QuickFiler.Test/Viewers/BreadcrumbPopupLifecycleOperationsTests.cs` asserting `ArgumentNullException` `.WithParameterName("createMessenger")` and `.WithParameterName("createReadiness")`; acceptance: the test passes and closes the throw sides of `BreadcrumbItemViewerLifecycleCoordinator.cs:388` and `:389` (AC-1, AC-11, US-3, US-8)
- [ ] [P5-T19] Add `[TestMethod] CreateCollapsedCandidate_Success_ReturnsBothFactoryResults` (LC-G4a) to `QuickFiler.Test/Viewers/BreadcrumbPopupLifecycleOperationsTests.cs`, supplying a fake messenger and a real `BreadcrumbNavigationReadiness`, and asserting the returned tuple's `Item1` and `Item2` are reference-identical to the exact instances the factories produced; acceptance: the test passes and closes the pass-through sides of `:388` and `:389` and the non-null sides of `BreadcrumbItemViewerLifecycleCoordinator.cs:390` and `:397`, plus lines `:387`-`:402` (AC-1, AC-11, US-3, US-8)
- [ ] [P5-T20] Add `[TestMethod] CreateCollapsedCandidate_NullMessenger_ThrowsWithoutDisposing` (LC-G4b) to `QuickFiler.Test/Viewers/BreadcrumbPopupLifecycleOperationsTests.cs`, supplying a `createMessenger` that returns `null`, and asserting the message is `"Collapsed navigation did not provide a messenger."` and that no disposal was attempted; acceptance: the test passes and closes the null side of `BreadcrumbItemViewerLifecycleCoordinator.cs:390` plus lines `:391`-`:394` (AC-1, AC-11, US-3)
- [ ] [P5-T21] Add `[TestMethod] CreateCollapsedCandidate_NullReadiness_DisposesADisposableMessenger` (LC-G4c) to `QuickFiler.Test/Viewers/BreadcrumbPopupLifecycleOperationsTests.cs`, supplying a hand-written fake implementing both `IWebViewMessenger` and `IDisposable` (per D-10) and a `createReadiness` that returns `null`, and asserting the message is `"Collapsed navigation did not provide a readiness lease."` and the messenger was disposed exactly once; acceptance: the test passes and closes the null side of `:397`, the non-null side of `BreadcrumbItemViewerLifecycleCoordinator.cs:406`, and line `:409` (AC-1, AC-11, US-3)
- [ ] [P5-T22] Add `[TestMethod] CreateCollapsedCandidate_NullReadiness_ToleratesANonDisposableMessenger` (LC-G4d) to `QuickFiler.Test/Viewers/BreadcrumbPopupLifecycleOperationsTests.cs`, supplying `new Mock<IWebViewMessenger>().Object` (whose proxy does not implement `IDisposable`) and a null-returning `createReadiness`, and asserting the same `InvalidOperationException` message with no exception from the disposal attempt; acceptance: the test passes and closes the null side of `BreadcrumbItemViewerLifecycleCoordinator.cs:406` plus lines `:404`-`:405` (AC-1, AC-11, US-3)
- [ ] [P5-T23] Add `[TestMethod] DisposeTwoResources_NullArguments_ThrowWithParameterNames` (LC-G3d) to `QuickFiler.Test/Viewers/BreadcrumbPopupLifecycleOperationsTests.cs` asserting `ArgumentNullException` `.WithParameterName("disposeMessenger")` and `.WithParameterName("disposeControl")`; acceptance: the test passes and closes `BreadcrumbItemViewerLifecycleCoordinator.cs:413` and `:414` (AC-1, AC-11, US-3)
- [ ] [P5-T24] Add `[TestMethod] DisposeTwoResources_BothSucceed_RunsBothAndThrowsNothing` (LC-G6) to `QuickFiler.Test/Viewers/BreadcrumbPopupLifecycleOperationsTests.cs`, supplying two recording actions that both succeed and asserting both ran in declaration order (`disposeMessenger` then `disposeControl`) with no exception escaping; acceptance: the test passes and closes the false side of `BreadcrumbItemViewerLifecycleCoordinator.cs:428` plus line `:432` (AC-1, AC-11, US-3)
- [ ] [P5-T25] Add `[TestMethod] NavigateWithSubscription_NullArguments_ThrowWithParameterNames` (LC-G3e) to `QuickFiler.Test/Viewers/BreadcrumbPopupLifecycleOperationsTests.cs` asserting `ArgumentNullException` `.WithParameterName("dispatcher")`, `.WithParameterName("navigate")`, and `.WithParameterName("createSubscription")`; acceptance: the test passes and closes `BreadcrumbItemViewerLifecycleCoordinator.cs:441`, `:442`, `:443` (AC-1, AC-11, US-3)
- [ ] [P5-T26] Add `[TestMethod] NavigateWithSubscription_NullSubscription_ThrowsAndCancelsTheLease` (LC-G7) to `QuickFiler.Test/Viewers/BreadcrumbPopupLifecycleOperationsTests.cs`, supplying a `NavigationSubscriptionFactory` of `(_, __, ___) => null`, and asserting `InvalidOperationException` with message `"Popup navigation did not provide an event subscription."` and `readiness.Completion.IsCanceled == true`; acceptance: the test passes and closes `BreadcrumbItemViewerLifecycleCoordinator.cs:450`, `:463`, `:475` plus lines `:464`-`:467` (AC-1, AC-11, US-3)
- [ ] [P5-T27] Add `[TestMethod] NavigateWithSubscription_OwnerDisposed_CancelsTheLease` (LC-G8) to `QuickFiler.Test/Viewers/BreadcrumbPopupLifecycleOperationsTests.cs`, driving `NavigateWithSubscription` to success with the F12-local recording factory from `[P5-T13]`, then invoking the captured owner-disposed callback and draining, and asserting `readiness.Completion.IsCanceled == true` and the detach ran exactly once; acceptance: the test passes, closes line `BreadcrumbItemViewerLifecycleCoordinator.cs:461`, and consumes no helper declared in the F13-owned `BreadcrumbPopupUiOperationsDirectAdapterTests.cs` (AC-1, AC-11, US-3, US-8)
- [ ] [P5-T28] Run `dotnet tool run csharpier format QuickFiler.Test/Viewers/BreadcrumbPopupLifecycleOperationsTests.cs` and record the post-format line count and declaration count; acceptance: `<FEATURE>/evidence/qa-gates/phase5-ops-file-size.<ts>.md` records `EXIT_CODE: 0`, a line count `<= 500` (projected ~360), exactly 15 `[TestMethod]` declarations, and confirms every double is declared locally in this file (AC-7, AC-11)
- [ ] [P5-T29] Rename the misnamed `[TestMethod] CandidateFailure_CleansMessengerAndReadiness` at `QuickFiler.Test/Viewers/BreadcrumbItemViewerLifecycleCoordinatorTests.cs:52` to `NavigationSurfaceMessengerFailure_CleansMessengerAndReadiness`, changing only the method name and leaving every Arrange, Act, and Assert statement byte-identical; acceptance: `<FEATURE>/evidence/qa-gates/misnamed-test-rename.<ts>.md` records the before/after names, confirms the diff for that file touches exactly one line, confirms the method still calls `CreateNavigationSurface`, and cites the research finding that the mislabel is the most plausible reason `CreateCollapsedCandidate` sat at 0% coverage (D-7, US-3)
- [ ] [P5-T30] Verify the `QuickFiler.Test/QuickFiler.Test.csproj` state after both Phase 5 insertions; acceptance: `<FEATURE>/evidence/qa-gates/phase5-csproj.<ts>.md` records an entry count of **112**, full CRLF on every line, a diff hunk confined to the two inserted lines adjacent to the `Viewers\BreadcrumbItemViewerLifecycleCoordinatorTests.cs` anchor, and confirms no property change, no reference change, and no reordering of unrelated entries (AC-15)
- [ ] [P5-T31] Run the scoped test pass with `& <vstest-path> QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation /TestCaseFilter:"FullyQualifiedName~BreadcrumbItemViewerLifecycleCoordinatorGuardTests|FullyQualifiedName~BreadcrumbPopupLifecycleOperationsTests|FullyQualifiedName~BreadcrumbItemViewerLifecycleCoordinatorTests"`; acceptance: `<FEATURE>/evidence/regression-testing/phase5-scoped-run.<ts>.md` records `Timestamp:`, `Command:`, `EXIT_CODE: 0`, 31 new executions passed, and zero failures across the 10 pre-existing tests including the renamed one
- [ ] [P5-T32] Recompute per-file coverage for `QuickFiler/Viewers/BreadcrumbItemViewerLifecycleCoordinator.cs` from a fresh run of `pwsh -NoProfile -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot '.' -Configuration Debug -CoverageOutput '<FEATURE>/evidence/qa-gates/coverage-phase5.<ts>.cobertura.xml'` under the Measurement Rules; acceptance: `<FEATURE>/evidence/qa-gates/lifecyclecoordinator-coverage.<ts>.md` records **100.00% line (318/318) and 97.95% branch (143/146)** with raw pairs, states the branch figure numerically rather than as a pass mark, records that `BreadcrumbPopupLifecycleOperations.CreateCollapsedCandidate` moved from 0% to 100% line, and states that the emitted `line-rate="0.939516"` / `branch-rate="0.688073"` were not read (AC-1, AC-2, AC-3, AC-11, US-1, US-2, US-4)
- [ ] [P5-T33] Record the three excluded outcomes for this file with their proofs; acceptance: `<FEATURE>/evidence/qa-gates/lifecyclecoordinator-excluded-outcomes.<ts>.md` names `BreadcrumbItemViewerLifecycleCoordinator.cs:135`, `:138`, and `:234`, reproduces the reachability proofs from the Irreducible Outcomes table, states `NO TASK TARGETS THESE`, records that the file clears the 75% branch floor by 22.95 points with all three waived, and states that no reflection into an F13-owned private member was used (AC-16, US-6)

### Phase 6 — Cross-Cutting Acceptance Verification

- [ ] [P6-T1] Verify AC-9 and AC-12 by listing every path in `git diff --name-only <merge-base>...HEAD` using the merge base recorded in `[P0-T9]` and confirming that **zero** production `.cs` files appear and that `QuickFiler/QuickFiler.csproj` is unmodified; acceptance: `<FEATURE>/evidence/qa-gates/ac09-ac12-no-production-change.<ts>.md` records the full path list, states `PRODUCTION .cs FILES CHANGED: 0`, and confirms by construction that the frozen six-argument `BreadcrumbItemViewerLifecycleCoordinator` constructor (`:29-36`) and the internal three-argument `BreadcrumbBridgeCoordinator` constructor (`:45-59`) are byte-identical (AC-9, AC-12, US-7)
- [ ] [P6-T2] Author the consolidated irreducible-outcome record; acceptance: `<FEATURE>/evidence/qa-gates/irreducible-outcomes.<ts>.md` reproduces all seven entries of the Irreducible Outcomes table with their proofs and the attached uncovered line `BreadcrumbBridgeRouter.cs:434`, reproduces the counting note reconciling `spec.md` §4.1's "six" with the seven-outcome enumeration, and confirms no task in this plan targets any member (AC-16, US-6)
- [ ] [P6-T3] Verify AC-10 by enumerating every test added or modified by this child that traverses a path described by promoted defect **#498**, **#499**, **#500**, **#501**, or **#502**, or by open **#440**, and confirming each carries an in-code comment naming the issue and asserts current behavior only; acceptance: `<FEATURE>/evidence/qa-gates/ac10-current-behavior-pinning.<ts>.md` maps each of the six issues to the tests that border it (#498 → `[P4-T6]` confinement record; #499 → `[P4-T1]`, `[P4-T2]`, `[P4-T4]`; #500 → `[P2-T3]`; #501 → the hub broadcast path, left untested with the reason recorded; #502 → `[P1-T3]`; #440 → `[P2-T6]`, `[P4-T5]`), states for each that no test asserts a defective outcome as desirable, and records for #501 that the `PostJson` multi-surface broadcast path is deliberately left untested because its current behavior is the defect (AC-10, US-5)
- [ ] [P6-T4] Verify AC-6 and AC-7 determinism by scanning every test file added or modified by this child for `Thread.Sleep`, `Task.Delay`, wall-clock waits, real-time polling, temporary files, filesystem writes, external services or processes, `.Show()`, `.ShowDialog()`, popups, STA attributes, `*.StaTests.cs` filenames, `GC.Collect`, injected clocks, `TimeProvider`, and `FakeTimeProvider`, and confirming every ambient `SynchronizationContext` assignment is restored in a `finally`; acceptance: `<FEATURE>/evidence/qa-gates/ac06-ac07-determinism.<ts>.md` records a zero-match result for every prohibited pattern across the six touched test files and enumerates each ambient-context site with its `finally` restore (AC-6, AC-7, US-3)
- [ ] [P6-T5] Verify AC-5 and AC-7 file sizes by producing a line-count listing for every file created or modified by this child plus the five in-scope production files; acceptance: `<FEATURE>/evidence/qa-gates/ac05-ac07-line-counts.<ts>.md` shows every listed file at `<= 500` lines and records the five production files unchanged at 487, 481, 456, 450, and 309 (AC-5, AC-7)
- [ ] [P6-T6] Verify AC-15 csproj mechanics end to end; acceptance: `<FEATURE>/evidence/qa-gates/ac15-csproj-mechanics.<ts>.md` records exactly five added `<Compile Include>` entries (`Controllers\BreadcrumbBridgeRouterEdgeTests.cs`, `Viewers\BreadcrumbBridgeCoordinatorGuardTests.cs`, `Viewers\BreadcrumbHubDisposalContractTests.cs`, `Viewers\BreadcrumbItemViewerLifecycleCoordinatorGuardTests.cs`, `Viewers\BreadcrumbPopupLifecycleOperationsTests.cs`), a final entry count of 112, full CRLF on every line, every insertion inside the breadcrumb block, no `sed -i` in the command history, and no property, reference, or ordering change (AC-15)
- [ ] [P6-T7] Verify AC-11 by confirming that `BreadcrumbPopupLifecycleOperations` and `BreadcrumbNavigationSubscription` are now covered by F12-owned direct tests independent of the F13-owned `QuickFiler.Test/Viewers/BreadcrumbPopupUiOperationsDirectAdapterTests.cs`; acceptance: `<FEATURE>/evidence/qa-gates/ac11-f12-owned-coverage.<ts>.md` records the 15 declarations in `QuickFiler.Test/Viewers/BreadcrumbPopupLifecycleOperationsTests.cs`, a zero-match grep proving that file references no helper declared in the F13-owned file, a zero-diff confirmation that the F13-owned file was not modified, and the measured `CreateCollapsedCandidate` figure at `>= 80%` line from a 0% baseline (AC-11, US-8)
- [ ] [P6-T8] Verify AC-13 by confirming that `QuickFiler.Test/Viewers/BreadcrumbCoordinatorLifecycleTests.cs` `ViewerScope` still constructs a live `new QuickFiler.ItemViewer()`; acceptance: `<FEATURE>/evidence/qa-gates/ac13-viewerscope-retained.<ts>.md` records the grep hit with its line number, confirms the file appears nowhere in this child's diff, and cites F14's request in `spec.md` §6.3 (AC-13, US-7)
- [ ] [P6-T9] Verify AC-14 retain-or-improve fixture guards not already covered by `[P3-T9]`, namely that no F12 change weakened, disabled, or deleted any pre-existing assertion anywhere in `QuickFiler.Test`; acceptance: `<FEATURE>/evidence/qa-gates/ac14-no-weakened-assertions.<ts>.md` records a zero count of removed or weakened pre-existing assertions across the diff and confirms the only modification to an existing test file is the single-line method rename in `[P5-T29]` (AC-14, US-7)
- [ ] [P6-T10] Verify scope containment by confirming every path in this child's diff is one of `QuickFiler.Test/Viewers/`, `QuickFiler.Test/Controllers/`, `QuickFiler.Test/QuickFiler.Test.csproj`, or `<FEATURE>/`; acceptance: `<FEATURE>/evidence/qa-gates/scope-containment.<ts>.md` records the full path list and states `F13-OWNED PATHS: 0`, `F14-OWNED PATHS: 0`, `F2-OWNED PATHS: 0`, `F8-OWNED PATHS: 0`, `UtilitiesCS PATHS: 0`, `<EPIC>/coverage-ledger.md CHANGED: NO` (AC-9, US-7)
- [ ] [P6-T11] Record the harness reading directives actually applied by every coverage task in this plan; acceptance: `<FEATURE>/evidence/qa-gates/harness-directives.<ts>.md` states that every figure was keyed on `filename=`, summed from deduplicated class-level `<line>` children with `max(hits)`, and that **no emitted `line-rate` or `branch-rate` attribute was relied upon**, and names the two positive controls for the `filename=` rule — `BreadcrumbCoordinatorUpgradeLifetime.cs` reporting as `QuickFiler.Viewers.BreadcrumbUpgradeLease`, and `BreadcrumbMessengerHub.cs` carrying 124 lines belonging to two types with no `<class>` element of their own (AC-3, US-4)
- [ ] [P6-T12] Record the cross-child dependency citations this child relies on; acceptance: `<FEATURE>/evidence/qa-gates/cross-child-dependencies.<ts>.md` cites F13's no-public-or-internal-signature-change commitment at `docs/features/active/2026-08-07-quickfiler-breadcrumb-dropdown-webview-coverage-455/spec.md:49-50` as the protection for `BreadcrumbUiDispatcher`, `BreadcrumbCollapsedSurfaceController`, `BreadcrumbNavigationReadiness`, `IWebViewMessenger`, and `IBreadcrumbWebHost`; records that no equivalent written commitment exists from F2 for `BreadcrumbOutboundQueue`'s surface; records the unrecorded ownership overlap whereby F2-owned `BreadcrumbOutboundQueue.cs` is tested from the F12-owned `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterQueueTests.cs:207-220`; and records that F8-owned `EfcHomeControllerExecuteMovesTests.cs` covers `BreadcrumbBridgeRouter.cs` incidentally so this child's coverage must not depend on it (AC-12, US-7)

### Phase 7 — Final QC Loop

- [ ] [P7-T1] Run `dotnet tool run csharpier format .` from the repository root; acceptance: `<FEATURE>/evidence/qa-gates/final-csharpier-format.<ts>.md` records `Timestamp:`, `Command:`, `EXIT_CODE: 0`, and the list of files reformatted (empty list expected on a clean pass) (AC-8)
- [ ] [P7-T2] Run `dotnet tool run csharpier check .` from the repository root; acceptance: `<FEATURE>/evidence/qa-gates/final-csharpier-check.<ts>.md` records `Timestamp:`, `Command:`, `EXIT_CODE: 0`, and an `Output Summary:` stating zero unformatted files (AC-8)
- [ ] [P7-T3] Run `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` using the path resolved in `[P0-T3]`; acceptance: `<FEATURE>/evidence/qa-gates/final-msbuild-analyzers.<ts>.md` records `Timestamp:`, `Command:`, `EXIT_CODE:`, zero errors, and a warning count no greater than the `[P0-T11]` baseline (AC-8)
- [ ] [P7-T4] Run `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true` using the path resolved in `[P0-T3]`; acceptance: `<FEATURE>/evidence/qa-gates/final-msbuild-nullable.<ts>.md` records `Timestamp:`, `Command:`, `EXIT_CODE:`, and zero nullable diagnostics introduced relative to the `[P0-T12]` baseline (AC-8)
- [ ] [P7-T5] Run `pwsh -NoProfile -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot '.' -Configuration Debug -CoverageOutput '<FEATURE>/evidence/qa-gates/coverage-final.<ts>.cobertura.xml'` from the executing worktree root; acceptance: `<FEATURE>/evidence/qa-gates/final-test-coverage.<ts>.md` records `Timestamp:`, `Command:`, `EXIT_CODE: 0`, total/passed/failed counts with **zero failures**, and the repository-wide **numeric line-rate and branch-rate** from the Cobertura root element together with the package count and `lines-valid` for like-for-like comparison against `[P0-T13]` (AC-8)
- [ ] [P7-T6] Verify that no step in `[P7-T1]` through `[P7-T5]` failed or modified a tracked file; if any did, restart the loop from `[P7-T1]` and re-record every artifact for the new pass; acceptance: `<FEATURE>/evidence/qa-gates/final-toolchain-loop.<ts>.md` records the pass number, the `git status --porcelain` output taken after `[P7-T5]`, the ordered command list, and `LOOP: CLEAN PASS` (AC-8, US-3)
- [ ] [P7-T7] Recompute post-change per-file coverage for all five in-scope production files from `<FEATURE>/evidence/qa-gates/coverage-final.<ts>.cobertura.xml` under the Measurement Rules; acceptance: `<FEATURE>/evidence/qa-gates/ac01-per-file-report.<ts>.md` records line and branch for each file with raw numerator/denominator pairs — `BreadcrumbCoordinatorUpgradeLifetime.cs` 100.00%/100.00%, `BreadcrumbBridgeCoordinator.cs` 100.00%/100.00%, `BreadcrumbMessengerHub.cs` 100.00%/100.00%, `BreadcrumbBridgeRouter.cs` 99.65%/95.56%, `BreadcrumbItemViewerLifecycleCoordinator.cs` 100.00%/97.95% — confirms every file clears both the 80% line and 75% branch floors, and states explicitly that no emitted `line-rate` or `branch-rate` attribute was used and that selection was by `filename=` (AC-1, AC-2, AC-3, US-1, US-2, US-4)
- [ ] [P7-T8] Verify retain-or-improve per file by comparing `<FEATURE>/evidence/qa-gates/ac01-per-file-report.<ts>.md` against `<FEATURE>/evidence/baseline/per-file-baseline.<ts>.md`, both measured on this branch; acceptance: `<FEATURE>/evidence/qa-gates/ac01-retain-or-improve.<ts>.md` shows every post-change figure greater than or equal to its baseline on **both** axes, reports any file that passes one axis and fails the other as failing, and records the branch delta for `BreadcrumbItemViewerLifecycleCoordinator.cs` as +31.51 points (AC-1, AC-2, US-1, US-2)
- [ ] [P7-T9] Compare the repository-wide line and branch rates from `[P0-T13]` and `[P7-T5]`; acceptance: `<FEATURE>/evidence/qa-gates/ac04-repo-wide-delta.<ts>.md` records both numeric pairs, the delta, the identical command and post-processing used for both, the matching package count and comparable `lines-valid`, an explicit statement that **no figure inherited from another branch or feature folder was used**, and `RESULT: RETAINED OR IMPROVED` (AC-4, US-1)
- [ ] [P7-T10] Re-verify post-format file sizes for every test file this child created or modified, measured after `[P7-T1]`; acceptance: `<FEATURE>/evidence/qa-gates/ac07-final-line-counts.<ts>.md` records `QuickFiler.Test/Viewers/BreadcrumbCoordinatorUpgradeLifetimeTests.cs`, `QuickFiler.Test/Viewers/BreadcrumbBridgeCoordinatorGuardTests.cs`, `QuickFiler.Test/Viewers/BreadcrumbHubDisposalContractTests.cs`, `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterEdgeTests.cs`, `QuickFiler.Test/Viewers/BreadcrumbItemViewerLifecycleCoordinatorGuardTests.cs`, `QuickFiler.Test/Viewers/BreadcrumbPopupLifecycleOperationsTests.cs`, and `QuickFiler.Test/Viewers/BreadcrumbItemViewerLifecycleCoordinatorTests.cs` each at `<= 500` lines (AC-7)
- [ ] [P7-T11] Check off every acceptance criterion in `<FEATURE>/spec.md` §9 (AC-1 .. AC-16), every item in `<FEATURE>/spec.md` §10 Definition of Done, and every criterion in `<FEATURE>/user-story.md` (US-1 .. US-8), citing the evidence artifact path for each; acceptance: `<FEATURE>/evidence/qa-gates/ac-status-summary.<ts>.md` maps all 16 + 5 + 8 items to a cited evidence path, marks no item PASS without an artifact, and records AC-5's second clause as `NOT ENGAGED — no production file created or modified` (AC-1 .. AC-16, US-1 .. US-8)
- [ ] [P7-T12] Commit every evidence artifact and all test and csproj changes; acceptance: `git status --porcelain` returns empty and `<FEATURE>/evidence/other/final-commit-state.<ts>.md` records the final commit SHA, the full list of committed paths, and the clean-tree confirmation

---

## Test Plan

- **Unit (new, MSTest + Moq + FluentAssertions, Arrange–Act–Assert with explicit section comments):**
  - `QuickFiler.Test/Viewers/BreadcrumbCoordinatorUpgradeLifetimeTests.cs` — **extended** with 4 cases
    (UL-H1 .. UL-H4) plus one `CountingCancellationTokenSource` helper. No csproj entry required.
  - `QuickFiler.Test/Viewers/BreadcrumbBridgeCoordinatorGuardTests.cs` — **new**, 6 declarations
    (BC-G1 .. BC-G6), 9 executions counting `[DataRow]`s.
  - `QuickFiler.Test/Viewers/BreadcrumbHubDisposalContractTests.cs` — **new**, 5 declarations
    (HUB-G1 .. HUB-G5).
  - `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterEdgeTests.cs` — **new**, 5 declarations
    (RT-J1a, RT-J1b, RT-J2, RT-J3, RT-J4), 6 executions. RT-J5 and RT-J6 are excluded per D-2.
  - `QuickFiler.Test/Viewers/BreadcrumbItemViewerLifecycleCoordinatorGuardTests.cs` — **new**, 11
    declarations, 16 executions. LC-G10(d) and LC-G11(b) are excluded per D-2.
  - `QuickFiler.Test/Viewers/BreadcrumbPopupLifecycleOperationsTests.cs` — **new**, 15 declarations.
  - **Totals: 46 new declarations, 55 executions, 5 new files, 1 extended file, 5 new csproj entries.**
- **Modified for clarity only:** `QuickFiler.Test/Viewers/BreadcrumbItemViewerLifecycleCoordinatorTests.cs`
  — one method rename (`[P5-T29]`), assertions unchanged.
- **Regression:** none is `[expect-fail]`. Every new test asserts behavior that is already correct on
  the current tree; the child adds coverage, not fixes. The complete pre-existing `QuickFiler.Test`
  suite is the integration gate and runs in `[P7-T5]`.
- **Integration boundary note.** The `BreadcrumbCollapsedAttachment` tests in Phase 3 and the
  `BreadcrumbPopupLifecycleOperations` tests in Phase 5 construct real F13-owned `internal sealed`
  types (`BreadcrumbCollapsedSurfaceController`, `BreadcrumbNavigationReadiness`) because neither can
  be mocked. They are integration tests across a child boundary, are deterministic, allocate no I/O,
  and depend on F13's signature-stability commitment recorded in `[P6-T12]`.
- **Coverage evidence:**
  - Baseline: `<FEATURE>/evidence/baseline/coverage-baseline.<ts>.cobertura.xml` and
    `<FEATURE>/evidence/baseline/per-file-baseline.<ts>.md` (`[P0-T13]`, `[P0-T14]`).
  - Per-phase: `<FEATURE>/evidence/qa-gates/coverage-phase{1,2,3,4,5}.<ts>.cobertura.xml` with one
    per-file report each (`[P1-T8]`, `[P2-T9]`, `[P3-T8]`, `[P4-T9]`, `[P5-T32]`).
  - Post-change: `<FEATURE>/evidence/qa-gates/coverage-final.<ts>.cobertura.xml` and
    `<FEATURE>/evidence/qa-gates/ac01-per-file-report.<ts>.md` (`[P7-T5]`, `[P7-T7]`).
  - Comparison: `<FEATURE>/evidence/qa-gates/ac01-retain-or-improve.<ts>.md` and
    `<FEATURE>/evidence/qa-gates/ac04-repo-wide-delta.<ts>.md` (`[P7-T8]`, `[P7-T9]`).

## Projected Coverage Outcome

| File | Line before → after | Branch before → after | Floors |
| --- | --- | --- | --- |
| `QuickFiler/Viewers/BreadcrumbCoordinatorUpgradeLifetime.cs` | 99.02% (202/204) → **100.00%** (204/204) | 92.59% (50/54) → **100.00%** (54/54) | pass / pass |
| `QuickFiler/Viewers/BreadcrumbBridgeCoordinator.cs` | 100.00% (280/280) → 100.00% | 87.36% (76/87) → **100.00%** (87/87) | pass / pass |
| `QuickFiler/Viewers/BreadcrumbMessengerHub.cs` | 100.00% (294/294) → 100.00% | 96.61% (114/118) → **100.00%** (118/118) | pass / pass |
| `QuickFiler/Controllers/BreadcrumbBridgeRouter.cs` | 97.87% (276/282) → **99.65%** (281/282) | 92.22% (83/90) → **95.56%** (86/90) | pass / pass |
| `QuickFiler/Viewers/BreadcrumbItemViewerLifecycleCoordinator.cs` | 90.57% (288/318) → **100.00%** (318/318) | 66.44% (97/146) → **97.95%** (143/146) | pass / **pass (was failing)** |

Branch-outcome arithmetic, for auditability: 4 (UL) + 11 (BC) + 4 (HUB) + 3 (RT) + 46 (LC) = **68
untaken outcomes closed**; 7 outcomes plus 1 line remain excluded per the Irreducible Outcomes table.

## Open Questions / Notes

- No `<EPIC>/coverage-ledger.md` row is added or changed by this child, because no production file is
  created or modified. `[P0-T8]` records that determination against the ledger contract.
- The epic's "Mid-Wave File Creation" rules and the `>= 90%` new-production-file target do not engage.
- The #457 measurement trap does not engage: no `[ExcludeFromCodeCoverage]` is added or removed at any
  level. Recorded for completeness — had a thin-forwarder been required, it would have to be a
  class-level-exempt adapter **type**, `sealed` and **not `partial`**.
- Open issue **#440** will rewrite the Left/Right arrow-key semantics in `BreadcrumbBridgeRouter.cs`
  and `BreadcrumbBridgeCoordinator.cs`. Whoever schedules #440 should expect to update `[P2-T6]` and
  `[P4-T5]` as part of the fix rather than treat them as a regression.
- Latent defects recorded but not promoted by this child's research (upgrade-lifetime LD-C and LD-D,
  bridge-coordinator LD-2 and LD-4, hub LD-3, lifecycle LD-3 through LD-6) remain prose in the
  research artifacts. Promotion, if any, is an orchestrator decision outside this plan's scope.
