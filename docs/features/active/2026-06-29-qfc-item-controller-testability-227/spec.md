# qfc-item-controller-testability - Refactor Spec

- **Issue:** #227
- **Parent (optional):** none
- **Owner:** drmoisan
- **Last Updated:** 2026-06-29T10-10
- **Status:** Draft
- **Version:** 0.2
- **Research:** `artifacts/research/2026-06-29T10-00-qfc-item-controller-testability-research.md`

## Intent & Outcomes

`QuickFiler/Controllers/QfcItemController.cs` is approximately 2,498 lines — far above the
repository 500-line file cap — and has roughly 5% line coverage (74 of 1,288 lines). It is
the controller for `QuickFiler/Viewers/ItemViewer.cs`, whose interface
`QuickFiler/Viewers/IItemViewer.cs` re-exposes raw WinForms control types (`ButtonSVG`,
`ComboBox`, `TableLayoutPanel`, `Label`, `TextBox`, `WebView2`, `FastObjectListView`,
`ToolStripMenuItemCb`) directly to consumers. The controller couples directly to these UI
types and to Outlook Interop objects (`MailItem`, `FolderPredictor`, `ConversationResolver`,
`FlagTasks`, `EmailFiler`), so its logic cannot be unit-tested without a live Outlook process
and window handles. This is the same coupling pattern remediated for
`QfcFormViewer`/`IQfcFormViewer` under issue #223.

Outcome: the controller is split into logical, sub-500-line partial-class files; the view
interface is narrowed toward intent-level seams; the concrete-`ItemViewer` field-type wall
that blocks `Mock<IItemViewer>` injection is removed; test files mirror the new structure;
and unit-test coverage of the affected testable (non-exempt) denominator reaches >= 80%.

## Invariants (must not change)

- Runtime behavior of the QuickFiler item viewer is unchanged: button clicks (Reply,
  ReplyAll, Forward, PopOut, Delete, FlagTask), conversation enumerate/collapse, folder
  combobox selection/search, WebView rendering, topic-thread selection, expansion/focus
  toggles, theme switching, and keyboard registration must behave exactly as before. This is
  a structural and testability refactor, not a behavior change.
- `ItemViewer` remains a `UserControl`-derived, `[ExcludeFromCodeCoverage]` partial class;
  Designer-generated code is untouched.
- The set of controls rendered and their wiring outcomes are preserved; only the seam through
  which the controller reaches them changes.
- `IQfcItemController` (the controller's own interface, which exposes `MailItem`/
  `MailItemHelper`) is NOT changed in this work; only `QfcItemController` internals and
  `IItemViewer` change.
- No new end-user behavior, performance change, or UX change.

## Scope (structural changes)

Apply the issue #223 strategy, phased per the research sequencing (research §7.2):

1. **Phase 0 — partial-class split (no behavior change).** Add `partial` to the class and
   distribute the existing `#region` clusters verbatim into 9 new files alongside the main
   file, each under 500 lines (research §1):
   - `QfcItemController.cs` (main: fields, properties, INotifyPropertyChanged)
   - `QfcItemController.Initialization.cs`
   - `QfcItemController.ViewerSetup.cs`
   - `QfcItemController.Conversation.cs`
   - `QfcItemController.FolderHandling.cs`
   - `QfcItemController.EventWiring.cs`
   - `QfcItemController.EventHandlers.cs`
   - `QfcItemController.Navigation.cs`
   - `QfcItemController.FocusAndTheme.cs`
   - `QfcItemController.MailActions.cs`

   Each new `.cs` file gets an explicit `<Compile Include>` in `QuickFiler.csproj` (legacy
   non-SDK project — no glob, same constraint as #223).

2. **Phase 1 — field-type unblock (Seam A).** Change `private ItemViewer _itemViewer` to
   `private IItemViewer _itemViewer` and the three public constructor parameters from
   `ItemViewer` to `IItemViewer`. `ItemViewer : IItemViewer`, so the 8 construction sites
   that pass concrete `ItemViewer` remain compatible. This removes the concrete-type wall
   that blocks `Mock<IItemViewer>` injection.

3. **Phase 2 — narrow `IItemViewer` to intent-level members (Seams B/C/D).** Replace raw
   WinForms control properties with display-state properties, command events, and intent
   methods (research §3.3). Update `ItemViewer.cs` with forwarding implementations (its
   Designer fields remain private and `[ExcludeFromCodeCoverage]`), and update the
   `QfcItemController` call sites that consumed the removed members. Update
   `QfcThemeHelper.SetupThemes` to take `IItemViewer`. Leave `QfcItemGroup.ItemViewer` and
   the one `QfcCollectionController` line that reads `grp.ItemViewer.LblItemNumber.Text`
   (line 140) on the concrete `ItemViewer` type to bound the blast radius.

4. **Phase 3 — mirror test files.** Reorganize/extend
   `QuickFiler.Test/Controllers/QfcItemControllerTests.cs` into test files mirroring the
   partial-class clusters (research §5.2), each under 500 lines; add `<Compile Include>`
   entries to `QuickFiler.Test.csproj`.

5. **Phase 4 — coverage uplift.** Add MSTest + Moq + FluentAssertions tests until the
   affected testable (non-exempt) denominator reaches >= 80%; new/extracted code reaches
   >= 90%; changed lines do not regress.

The atomic plan may split Phase 2 into cluster-sized sub-phases (2a Labels, 2b Buttons,
2c Folder/Search, 2d WebView/TopicThread) per research §7.2.

## Non-Goals

- Changing `IQfcItemController` (the controller interface) or removing COM types from it.
- Splitting `QfcCollectionController.cs` (~2,300 lines) — pre-existing debt, out of scope.
- Adding interfaces to `ItemViewerExpanded`/`QfcItemViewerExpanded` or making those
  UserControls unit-testable.
- Introducing an injectable `Dispatcher` for the `UiThread.Dispatcher` static (research
  Phase 4 / Seam C) unless required to meet the coverage target; otherwise deferred to #197.
- Any new end-user behavior, performance change, or UX change.

## Dependencies / Touchpoints

In-repo consumers updated as needed (research §4):
- `QuickFiler/Controllers/QfcCollectionController.cs` — 6 construction sites (compatible);
  line 140 `grp.ItemViewer.LblItemNumber.Text` stays on concrete `ItemViewer`.
- `QuickFiler/Controllers/QfcQueue.cs` — 1 construction site (compatible).
- `QuickFiler/Helper Classes/QfcThemeHelper.cs` — `SetupThemes` `ItemViewer` parameter →
  `IItemViewer`.
- `QuickFiler/Controllers/QfcItemGroup.cs` — `ItemViewer` property stays concrete.
- `QuickFiler/Viewers/ItemViewer.cs` — gains forwarding implementations for new intent
  members.
- `QuickFiler.csproj` and `QuickFiler.Test.csproj` — explicit `<Compile Include>` entries.

No external (out-of-QuickFiler) consumers of `IItemViewer` exist. Required coordination:
none beyond required CI checks on the PR.

## Risks & Mitigations

- `IItemViewer` narrowing is materially larger than the #223 `IQfcFormViewer` narrowing
  (~50 members replaced vs. 7). Mitigation: phase Phase 2 by cluster; each sub-phase gated.
- `QfcItemController` is predominantly COM/Outlook/WinForms-bound; literal >= 80% coverage of
  its full line count is not achievable. Mitigation: apply the testable-denominator
  interpretation (see Coverage Target below) and `[ExcludeFromCodeCoverage]` at method level
  for unresolvably COM/WinForms-bound members per the CLAUDE.md exemption.
- `UiThread.Dispatcher` static (~20 call sites) limits async-dispatch testability.
  Mitigation: deferred (Non-Goal) unless required for the target.
- 10-file split increases csproj entries; mitigated by the proven #223 `<Compile Include>`
  pattern.
- Adding `partial` to a non-partial class is syntactic-only; no behavior impact.

## Technical Specifications

- Files/modules expected to change: ~15–16 production files (9 new partial files; edits to
  `QfcItemController.cs`, `IItemViewer.cs`, `ItemViewer.cs`, `QfcThemeHelper.cs`,
  `QuickFiler.csproj`; possible edits to `QfcCollectionController.cs`), and ~6–8 test files.
- Public interfaces/contracts affected: `IItemViewer` (narrowed — breaking change, all
  consumers in-repo). `IQfcItemController` unchanged.
- Data flow: intent methods return plain C# values (e.g., `GetSelectedFolder()` returns a
  string); display-state setters replace direct `Label.Text` writes. No data-format change.
- Logging/telemetry: unchanged.
- Migration/backfill: none.

## Test Strategy

- Regression/new tests (research §5.2): preserve the existing 8 tests; add
  `QfcItemController.ConversationTests.cs`, `QfcItemController.FolderHandlingTests.cs`,
  `QfcItemController.EventWiringTests.cs`, `QfcItemController.NavigationTests.cs`,
  `QfcItemController.MailActionsTests.cs`, and `QfcItemController.PropertiesTests.cs` as
  needed, each under 500 lines.
- Techniques: `Mock<IItemViewer>` event raising (`Raise`), `VerifySet`/`Verify`, the existing
  virtual-seam subclass (`DoLoadConversationResolverCoreAsync`), reflection-based
  `_kbdHandler` injection, and pure static-seam calls (`PopulateAndSelectFolder`).
- Invariant validation: existing tests must continue to pass unchanged after the seam
  migration.
- Edge/negative: empty/all-missing folder arrays; null `ConversationResolver`; cancellation
  vs. non-cancellation in `LoadConversationResolverAsync`; "Trash to Delete" present/absent.
- Coverage targets (see below).
- Toolchain (in order): `csharpier .` → `msbuild ... /p:EnableNETAnalyzers=true
  /p:EnforceCodeStyleInBuild=true` → `msbuild ... /p:Nullable=enable
  /p:TreatWarningsAsErrors=true` → `vstest.console.exe <QuickFiler.Test assembly>
  /EnableCodeCoverage`.
- Manual validation: none required (structural refactor; behavior preserved).

### Coverage Target (interpretation of "above 80%")

Per CLAUDE.md and `.claude/rules/general-unit-test.md`, the 80% floor applies to the
**testable denominator** — production-only first-party code after excluding COM/VSTO/WinForms
code that cannot be unit-tested without a live Outlook process. For this work:

- The affected non-exempt testable code of `QfcItemController` (notably the Conversation,
  FolderHandling, EventWiring-registration, Properties/INotifyPropertyChanged, and
  PackageItems/navigation-routing clusters) reaches >= 80%.
- New or extracted code reaches >= 90%.
- Changed lines do not regress coverage.
- Unresolvably COM/Outlook/WinForms-bound members (e.g., `async void` UI event handlers,
  `Mail.Reply/ReplyAll/Forward`, `FlagTasks`, `EmailFiler`, `_itemViewer.Invoke`-only
  methods) carry method-level `[ExcludeFromCodeCoverage]` with rationale.
- The repo-wide >= 80% floor remains satisfied-with-documented-exception under the
  maintainer-ratified authority-scoped exception precedent
  (`docs/features/active/2026-06-28-qfc-form-viewer-testability-223/maintainer-decision.2026-06-29.md`);
  residual repo-wide uplift is tracked under #197. Maintainer ratification of the exemption
  boundary for this controller is required at review time.

## Definition of Done

- [ ] Structure matches this spec; legacy paths retired or redirected
- [ ] Invariants validated with tests or comparisons
- [ ] Imports/tooling/entry points updated
- [ ] Edge cases and error handling verified
- [ ] Tests, linting, and type checks clean
- [ ] Docs updated (initiative/README/tasks as needed)
- [ ] Toolchain pass completed (format → lint → type-check → test)

## Acceptance Criteria

- [x] AC1: `QfcItemController` is split into partial-class files, each under 500 lines, with
  a logical responsibility-based structure; no behavior change; all existing tests pass.
- [x] AC2: `private ItemViewer _itemViewer` and the public constructor parameters are
  changed to `IItemViewer`; `Mock<IItemViewer>` is injectable into the controller.
- [x] AC3: `IItemViewer` is narrowed to intent-level members (display-state properties,
  command events, intent methods); raw clickable/raw control types are removed from the
  interface; `ItemViewer.cs` provides forwarding implementations and remains
  `[ExcludeFromCodeCoverage]`.
- [x] AC4: Test files mirror the new partial-class structure (one test file per testable
  cluster), each under 500 lines, with explicit csproj entries.
- [ ] AC5: Coverage of the affected testable (non-exempt) denominator is >= 80%; new/
  extracted code >= 90%; changed lines do not regress. Repo-wide floor handled under the
  authority-scoped exception precedent; exemption boundary ratified by the maintainer.
  PARTIAL / REMEDIATION-REQUIRED: the >= 80% affected testable non-exempt denominator is MET
  (484/585 = 82.74%), changed lines show no regression, and the exemption boundary is documented
  for maintainer ratification; the >= 90% new/extracted sub-target is UNMET after exhausting
  testable seams (binding constraint: EventWiring inline async-registration lambda bodies, plus
  the injectable-`Dispatcher`-deferred Conversation render and the `GetItemSummary` COM read) and
  is recorded as remediation-required, tracked under #197. AC5 remains unchecked pending the 90%
  sub-target and maintainer ratification of the exemption boundary.
- [x] AC6: No production file modified in this cycle exceeds 500 lines after the change.
- [x] AC7: Full C# toolchain passes in order — csharpier, .NET analyzers,
  nullable/TreatWarningsAsErrors, MSTest with coverage — with no regressions.

## Seeded Test Conditions (from potential)
- [x] Unit coverage of extracted pure logic and seam-routed controller behavior via Moq
  event raising / `VerifySet` / `Verify`.
- [x] No temporary files; deterministic; MSTest + Moq + FluentAssertions only.
