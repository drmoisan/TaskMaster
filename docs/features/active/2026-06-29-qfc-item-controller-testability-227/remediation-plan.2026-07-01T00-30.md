# qfc-item-controller-testability — Cycle-2 Seam-Redesign Remediation Plan

- **Issue:** #227
- **Parent (optional):** none
- **Owner:** drmoisan
- **Last Updated:** 2026-07-01T00-30
- **Status:** Draft (remediation cycle 2 — seam redesign, Option A)
- **Work Mode:** full-feature
- **Target plan path:** `docs/features/active/2026-06-29-qfc-item-controller-testability-227/remediation-plan.2026-07-01T00-30.md`

## Trigger and Cycle Scope

Maintainer DENIED ratification of the 103-member `[ExcludeFromCodeCoverage]` boundary
(`maintainer-decision.2026-07-01.md`); Option A approved. This cycle makes the cycle-1 exempted
members unit-testable through behavioral seams and exemption removal, rather than exempting them,
driving the exemption set toward zero. This is a behavior-preserving testability change; runtime
behavior of the QuickFiler item viewer must not change. Phases 0–4 (partial split, field-type
unblock, `IItemViewer` narrowing, test-file mirror, initial coverage uplift) were delivered in
cycle 1 (`bcc7d7e3`) and are NOT re-executed here; this plan covers the spec's Phases 5–7 plus the
mandatory baseline (Phase 0) and authoritative final QA loop (Phase 8).

## Authoritative Inputs

- Spec (Redesign scope §Phases 5–7, revised AC5, AC8/AC9/AC10, Invariants, Coverage Target):
  `docs/features/active/2026-06-29-qfc-item-controller-testability-227/spec.md` (v0.3)
- Issue + early Acceptance Criteria: `docs/features/active/2026-06-29-qfc-item-controller-testability-227/issue.md`
- Maintainer decision (R2 exemption DENIED, Option A directed):
  `docs/features/active/2026-06-29-qfc-item-controller-testability-227/maintainer-decision.2026-07-01.md`
- Remediation inputs (cycle-2 entry, in/out of scope, constraints, exit condition):
  `docs/features/active/2026-06-29-qfc-item-controller-testability-227/remediation-inputs.2026-07-01T00-30.md`
- Seam-redesign research (per-member barrier classification §1, existing-layer reuse §2, seam design §3,
  reducibility verdict/bucket table §4, scope/risk §5, testing implications §6):
  `artifacts/research/2026-07-01T00-00-qfc-item-controller-seam-redesign-research.md`
- Cycle-1 plan (evidence and gate patterns mirrored):
  `docs/features/active/2026-06-29-qfc-item-controller-testability-227/plan.2026-06-29T10-15.md`
- Denied boundary (superseded): `evidence/other/exemption-boundary.2026-06-29T12-40.md`
- Coverage-exception authority precedent:
  `docs/features/active/2026-06-28-qfc-form-viewer-testability-223/maintainer-decision.2026-06-29.md`
- Policy: `CLAUDE.md`, `.claude/rules/general-code-change.md`, `.claude/rules/general-unit-test.md`, `.claude/rules/csharp.md`.

The seam-redesign research is the source of truth for member names, signatures, barrier
classifications, and the four seam shapes (§3.2 `IUiDispatcher`, §3.3 `IWebViewCoreInitializer`,
§3.4 `IMailItemActions` + factory delegates, §3.5 thin-delegator handlers). This plan does not
restate those signatures; it sequences the edits and binds each to a verifiable outcome.

## Evidence Location Invariant

All evidence artifacts MUST be written under the canonical scheme
`docs/features/active/2026-06-29-qfc-item-controller-testability-227/evidence/<kind>/` using only
`evidence/remediation-baseline/` (cycle-2 baseline), `evidence/qa-gates/` (per-phase and final QA),
`evidence/regression-testing/` (coverage delta), and `evidence/other/` (exemption boundary, AC
traceability). Non-canonical paths such as `artifacts/baselines/`, `artifacts/qa/`,
`artifacts/coverage/`, or `evidence/coverage/` are prohibited and fail preflight. Each evidence
artifact must include `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:`; coverage-bearing
artifacts must record numeric coverage values, not placeholders. No non-canonical evidence path was
supplied in the delegation; no override rejection is required.

## C# Toolchain (run in this exact order; restart from step 1 on any failure or file change)

1. `dotnet tool run csharpier .`
2. `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
3. `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true`
4. `vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /EnableCodeCoverage`

## Phase-to-Spec Mapping

| Spec / Research phase | This plan |
|---|---|
| Cycle-2 baseline capture (atomic-plan-contract Phase 0) | Phase 0 |
| Spec Phase 5 — remove over-broad exemptions (AC8) | Phase 5 |
| Spec Phase 6 — four behavioral seams (AC9) | Phase 6 |
| Spec Phase 7 — final residual boundary (AC10) | Phase 7 |
| Final QA loop, coverage delta, disposition (AC5, AC6, AC7) | Phase 8 |

The spec's Phase numbering (5, 6, 7) is preserved for the seam-redesign work per the cycle-2 brief;
Phase 0 is the mandatory baseline-capture phase and Phase 8 is the authoritative final QA loop.

## Scope Snapshot

Production (existing, modified):
- `QuickFiler/Controllers/QfcItemController.*.cs` — the 9 partials + main file; remove
  `[ExcludeFromCodeCoverage]` from bucket-(i) and bucket-(ii) members; route bucket-(ii) members
  through the new seams; apply the thin-delegator split to the six `async void` handlers.
- `QuickFiler/Viewers/IItemViewer.cs` — no new raw-control members (leaf-control interfaces NOT
  introduced, §3.1); narrow further only if new raw-control leakage is found during implementation.
- `QuickFiler/QuickFiler.csproj`, `QuickFiler.Test/QuickFiler.Test.csproj` — explicit
  `<Compile Include>` entries for every new production and test file (legacy `packages.config`
  non-SDK projects, no glob).

Production (NEW seam files):
- `UtilitiesCS/Threading/IUiDispatcher.cs`, `UtilitiesCS/Threading/WpfUiDispatcher.cs`
- `QuickFiler/Viewers/IWebViewCoreInitializer.cs`, `QuickFiler/Viewers/WebView2CoreInitializer.cs`
- `QuickFiler/Interfaces/IMailItemActions.cs`, `QuickFiler/Interfaces/MailItemActionsAdapter.cs`
- Possible spill partial `QuickFiler/Controllers/QfcItemController.EventWiring.ControlTree.cs` if the
  `WireEvents` split pushes `EventWiring.cs` toward the 500-line cap (§5.1).

Test (existing + NEW, all `< 500` lines, wired into `QuickFiler.Test.csproj`):
- Existing cycle-1 tests preserved and continue to pass unchanged (regression guard).
- NEW as needed: `QfcItemController.InitializationTests.cs`, `QfcItemController.ViewerSetupTests.cs`,
  `QfcItemController.EventHandlersTests.cs`, `QfcItemController.FocusAndThemeTests.cs`, plus updates
  to `ConversationTests`, `FolderHandlingTests`, `EventWiringTests`, `NavigationTests`,
  `MailActionsTests`. Adapter smoke tests: `WpfUiDispatcherTests`, `WebView2CoreInitializerTests`,
  `MailItemActionsAdapterTests`.

## Scope Boundaries (Option A only)

- Leaf-control interfaces (`IButton`/`ILabel`/`ICheckBox`/`IComboBox`/`ITextBox`) and `IList<IButton>`
  retyping (Option B) are NOT introduced — research §1/§2/§3.1 found no exempted member is blocked by
  concrete-control typing.
- `IQfcItemController` is NOT changed; `QfcCollectionController.cs` is NOT split; line 140
  `grp.ItemViewer.LblItemNumber.Text` stays on the concrete `ItemViewer`.
- New seam constructor parameters are optional with production defaults (non-breaking for the 8
  existing construction sites in `QfcCollectionController.cs`/`QfcQueue.cs`).
- `TlpCellSnapShot.ApplyState(Control)` (barrier (f), the only genuinely out-of-scope collaborator
  behind `ToggleExpansionOn`/`Off`) is NOT seamed this cycle; a follow-up issue is recorded (P7-T5).

---

### Phase 0 — Cycle-2 Remediation Baseline Capture and Policy Reads

Mandatory baseline-capture phase. Establishes the pre-redesign toolchain state, the current
103-member exemption inventory, and the coverage baseline against which the cycle-2 delta is measured.

- [x] [P0-T1] Read the policy files in the required order (`CLAUDE.md`, `.claude/rules/general-code-change.md`, `.claude/rules/general-unit-test.md`, `.claude/rules/csharp.md`) and the seven authoritative inputs above. Write `evidence/remediation-baseline/phase0-instructions-read.<ISO-8601>.md` with `Timestamp:`, `Policy Order:`, and the explicit list of files read. Acceptance: artifact exists with all three fields populated.
- [x] [P0-T2] Run `dotnet tool run csharpier .` in check posture against the current (post cycle-1) tree. Write `evidence/remediation-baseline/baseline-csharpier.<ISO-8601>.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`. Acceptance: artifact records the format-check exit code and any pre-existing format drift.
- [x] [P0-T3] Run the analyzer build (toolchain step 2) on the clean tree. Write `evidence/remediation-baseline/baseline-analyzers.<ISO-8601>.md` with the four required fields and an analyzer-diagnostic-count summary. Acceptance: artifact records `EXIT_CODE:` and a diagnostic headline.
- [x] [P0-T4] Run the nullable/TreatWarningsAsErrors build (toolchain step 3) on the clean tree. Write `evidence/remediation-baseline/baseline-nullable.<ISO-8601>.md` with the four required fields. Acceptance: artifact records `EXIT_CODE:` and a warning headline.
- [x] [P0-T5] Run `vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /EnableCodeCoverage`. Write `evidence/remediation-baseline/baseline-tests-coverage.<ISO-8601>.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` recording numeric values: total passed/failed test count, repo-wide testable-denominator line-coverage percent, and `QfcItemController` (all partials) affected-non-exempt-denominator line-coverage percent. Acceptance: artifact contains numeric coverage values (no placeholders) and the passing test count to be preserved as the regression baseline across all later phases.
- [x] [P0-T6] Record the current 500-line-cap inventory for every file in the redesign blast radius: the 10 `QfcItemController*.cs` files, `IItemViewer.cs`, `ItemViewer.cs` and its four forwarding partials, and each existing `QfcItemController*Tests.cs` file. Write `evidence/remediation-baseline/baseline-file-sizes.<ISO-8601>.md` with `Timestamp:` and all measured counts. Acceptance: artifact records every count; each is confirmed `< 500` at baseline.
- [x] [P0-T7] Re-verify the current `[ExcludeFromCodeCoverage]` inventory against source: enumerate every member/partial carrying the attribute today and reconcile against the denied boundary `evidence/other/exemption-boundary.2026-06-29T12-40.md` (expected 101 methods + 2 properties = 103). Write `evidence/remediation-baseline/baseline-exemption-inventory.<ISO-8601>.md` with `Timestamp:`, `Command:` (grep used), `EXIT_CODE:`, `Output Summary:` listing the exempted members grouped by partial and the total count. Acceptance: artifact records the exact starting exemption count and per-member list; any drift from 103 is noted explicitly.

---

### Phase 5 — Remove Over-Broad Exemptions (AC8)

Remove `[ExcludeFromCodeCoverage]` from the ~38 bucket-(i) members that have no genuine testability
barrier (research §4 bucket (i)) and cover each with MSTest + Moq + FluentAssertions tests. No
production seam is introduced in this phase; members are exercised through the already-narrowed
`IItemViewer`, mockable collaborators (`_kbdHandler`, `_parent`, `_homeController`,
`_explorerController`, `_itemPositionTips`), plain data objects (`MailItemHelper`), and — for the
`FocusAndTheme` cluster — reflection-injected `_themes`. Each cluster task removes the attribute from
the named members AND lands the covering tests; the binary outcome is that those members no longer
carry `[ExcludeFromCodeCoverage]` and each is exercised by at least one passing test.

- [x] [P5-T1] Establish the `_themes` reflection-injection test capability mirroring the existing `_kbdHandler` reflection pattern (research §3.6, §6): confirm `_themes` is a private field on `QfcItemController` and add a test helper that injects a `Dictionary<string, Theme>` built from handle-less `Theme` instances (constructed with lightweight `new Label()`/`new Button()` doubles carrying only the `Color` properties under test, per `UtilitiesCS/HelperClasses/ThemeHelpers/Theme.cs`). No production code change is required beyond confirming field visibility. Write no evidence artifact for this task (covered by the phase test-coverage gate). Acceptance: a reusable test helper injects `_themes` via reflection and a smoke test confirms a subsequent `_themes[_activeTheme]` read returns the injected instance.
- [x] [P5-T2] Remove `[ExcludeFromCodeCoverage]` from the Initialization bucket-(i) members (the `protected` ctor, the three public ctors, and `SaveParameters`) in `QuickFiler/Controllers/QfcItemController.Initialization.cs`, and add tests covering constructor field delegation and `SaveParameters` field assignment (including `_itemViewer.Controller = this` via `Mock<IItemViewer>` `VerifySet`). Acceptance: none of the five members carry the attribute; each is covered by ≥1 passing test; `Initialize*`/`Create*` orchestration methods remain exempt for now (reclassified in Phase 6/7).
- [x] [P5-T3] Remove `[ExcludeFromCodeCoverage]` from the ViewerSetup bucket-(i) members (`PopulateControls(MailItemHelper,int)`, `AssignControlsAsync`, `AssignControls`, `Cleanup`) in `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs`, and add tests: `AssignControls` verifying `IItemViewer` setter writes via `Mock<IItemViewer>`, `Cleanup` verifying private fields are nulled (reflection field-read), `AssignControlsAsync` via a `Mock<IItemViewer>.UiDispatcher` executing the delegate. Acceptance: the four members carry no exemption; each is covered by ≥1 passing test.
- [x] [P5-T4] Remove `[ExcludeFromCodeCoverage]` from the Conversation bucket-(i) members (`PopulateConversation(ConversationResolver)`, `RenderConversationCount()`, `SetTopicThread`) in `QuickFiler/Controllers/QfcItemController.Conversation.cs`, and add tests (resolver-set and resolver-null branches for `RenderConversationCount()`; `SetTopicThread` verifying `SetConversationItems`/`SortConversationByDate` on `Mock<IItemViewer>`). Acceptance: the three members carry no exemption; each is covered by ≥1 passing test.
- [x] [P5-T5] Remove `[ExcludeFromCodeCoverage]` from the EventWiring registration-membership bucket-(i) members (`RegisterFocusActions`, `UnregisterFocusActions`, `UnregisterExpandedActions`) in `QuickFiler/Controllers/QfcItemController.EventWiring.cs`, and add dictionary-membership tests mirroring the existing `RegisterFocusAsyncActions` pattern (reflection-injected `_kbdHandler`, assert `KeyActions`/`CharActions` membership without invoking the lambda bodies). Acceptance: the three members carry no exemption; each is covered by ≥1 passing membership test.
- [x] [P5-T6] Remove `[ExcludeFromCodeCoverage]` from the EventHandlers bucket-(i) members (`CbxConversation_CheckedChanged`, `CbxEmailCopy_CheckedChanged`, `CboFolders_SelectedIndexChanged`, `CbxAttachments_CheckedChanged`, `TextBoxSearch_KeyDown`, `TopicThread_ItemSelectionChanged`, `BtnDelItem_Click`, and the four mouse-enter/leave handlers `Button_MouseEnter`/`Leave`, `MenuItem_MouseEnter`/`Leave`) in `QuickFiler/Controllers/QfcItemController.EventHandlers.cs`, and add tests (checkbox handlers verifying the field write / `SuppressEvents` branch; mouse handlers using reflection-injected `_themes` and a bare `new Button()`/`new ToolStripMenuItem()` sender asserting `BackColor`). Acceptance: the eleven members carry no exemption; each is covered by ≥1 passing test; the `SynchronizationContext.Current is null` guard is exercised as a no-op under the ambient MSTest context.
- [x] [P5-T7] Remove `[ExcludeFromCodeCoverage]` from the Navigation bucket-(i) members (`JumpToFolderDropDown`, `JumpToSearchTextbox`, `ToggleExpansion()`, `ToggleExpansionAsync()`) in `QuickFiler/Controllers/QfcItemController.Navigation.cs`, and add tests (`JumpTo*` verifying `_kbdHandler.ToggleKeyboardDialog()` + the `Mock<IItemViewer>` intent call; the two parameterless `ToggleExpansion` overloads verifying the `_expanded`-branch routing to the state-taking overload via a testable subclass override). Acceptance: the four members carry no exemption; each is covered by ≥1 passing test.
- [x] [P5-T8] Remove `[ExcludeFromCodeCoverage]` from the FocusAndTheme bucket-(i) members (the 14 members named in research §1 FocusAndTheme verdict: `ToggleFocus`/`Async` overloads, `ToggleFocusOnAsync`/`OffAsync`, `ToggleNavigation` overloads, `ToggleNavigationAsync`, `ToggleTips`/`Async`, `InvokeBeginInvoke`, `ToggleSaveAttachments`, `SetThemeDark`/`SetThemeLight`, `HtmlDarkConverter`) in `QuickFiler/Controllers/QfcItemController.FocusAndTheme.cs`, and add tests using reflection-injected `_themes` (P5-T1), mocked `IQfcTipsDetails`, and `Mock<IItemViewer>.Invoke`/`BeginInvoke` executing the delegate. Exclude `ToggleSaveCopyOfMail` and `ApplyReadEmailFormat` (deferred to Phase 6). Acceptance: the 14 named members carry no exemption; each is covered by ≥1 passing test.
- [x] [P5-T9] Remove `[ExcludeFromCodeCoverage]` from the MailActions bucket-(i) members at the getter/common-branch level (`RightKeyActions`, `RightKeyActionsAsync` getters via dictionary-membership tests asserting the `"&Pop Out"`/`"&Expand"`/`"&Cancel"` keys; `CollapseConversation` and `EnumerateConversation` common branch with `_convOriginID` non-empty to avoid the `Mail.EntryID` read) in `QuickFiler/Controllers/QfcItemController.MailActions.cs`, and add the covering tests. Acceptance: the four members carry no exemption at the getter/common-branch level; each is covered by ≥1 passing test; the COM-bound `Mail.EntryID` fallback branch remains deferred to the Phase 6 `IMailItemActions` seam.
- [x] [P5-T10] Wire every new Phase 5 test file into `QuickFiler.Test/QuickFiler.Test.csproj` with explicit `<Compile Include>` entries (new files as needed: `QfcItemController.InitializationTests.cs`, `QfcItemController.ViewerSetupTests.cs`, `QfcItemController.EventHandlersTests.cs`, `QfcItemController.FocusAndThemeTests.cs`; additions to existing per-cluster test files otherwise). Acceptance: every new test file has a csproj entry; the test assembly compiles and discovers the new tests.
- [x] [P5-T11] Measure line counts for every production partial and test file edited or created in Phase 5. Write `evidence/qa-gates/p5r-file-sizes.<ISO-8601>.md` with `Timestamp:` and the counts. Acceptance: every measured file is `< 500` lines.
- [x] [P5-T12] Run toolchain step 1 (`dotnet tool run csharpier .`). Write `evidence/qa-gates/p5r-csharpier.<ISO-8601>.md` with the four required fields. Acceptance: `EXIT_CODE: 0`; restart the loop if files changed.
- [x] [P5-T13] Run toolchain step 2 (analyzers). Write `evidence/qa-gates/p5r-analyzers.<ISO-8601>.md`. Acceptance: `EXIT_CODE: 0`, no new analyzer errors versus baseline.
- [x] [P5-T14] Run toolchain step 3 (nullable/TreatWarningsAsErrors). Write `evidence/qa-gates/p5r-nullable.<ISO-8601>.md`. Acceptance: `EXIT_CODE: 0`.
- [x] [P5-T15] Run toolchain step 4 (`vstest ... /EnableCodeCoverage`). Write `evidence/qa-gates/p5r-tests-coverage.<ISO-8601>.md` with numeric passing count, affected-denominator coverage, and the current exemption count. Acceptance: the pre-existing baseline tests still pass (regression guard equal to the P0-T5 passing count plus the new Phase 5 tests); `EXIT_CODE: 0`; the exemption count has decreased by the ~38 members de-exempted in this phase.

---

### Phase 6 — Introduce Four Behavioral Seams (AC9)

Introduce the four narrow seams from research §3 following the DI-seam ordering (interface > delegate
> adapter), route the ~40 bucket-(ii) members through them, and cover the unblocked logic to `>= 90%`
for new/extracted code. All new constructor parameters are optional with production defaults
(non-breaking). Adapter implementation bodies (thin forwarding shims) legitimately retain
`[ExcludeFromCodeCoverage]` and are classified in Phase 7.

- [x] [P6-T1] Create NEW `UtilitiesCS/Threading/IUiDispatcher.cs` declaring the dispatch seam (`void Invoke(Action)`, `Task InvokeAsync(Action)`, `Task InvokeAsync(Action, DispatcherPriority, CancellationToken)`, `IAsyncResult BeginInvoke(Action)`, and `Task<TResult> InvokeAsync<TResult>(Func<TResult>)` per research §3.2) and NEW `UtilitiesCS/Threading/WpfUiDispatcher.cs` implementing it as a 1:1 adapter over `UiThread.Dispatcher`, AND add `<Compile Include>` entries for both to the owning UtilitiesCS project file. Acceptance: both files compile; `WpfUiDispatcher` forwards each member to the corresponding `Dispatcher` call; `WpfUiDispatcher.InvokeAsync<TResult>(Func<TResult>)` forwards to `UiThread.Dispatcher.InvokeAsync(func).Task`; both files are wired into the csproj.
- [x] [P6-T2] Add an optional `IUiDispatcher uiDispatcher = null` constructor parameter to `QfcItemController` defaulting to a `WpfUiDispatcher` in production (stored in a private field), and replace every `UiThread.Dispatcher.Invoke(...)`/`.InvokeAsync(...)`/`.BeginInvoke(...)` call site across the partials with the injected `_uiDispatcher` equivalent (research §3.2 enumerates the ~11 affected members). Acceptance: no `UiThread.Dispatcher` reference remains in `QfcItemController*.cs`; behavior is preserved; the 8 existing construction sites still compile against the defaulted parameter. The value-returning sites `Reply`/`ReplyAll`/`Forward` (Navigation.cs:94/101/108) route through `_uiDispatcher.InvokeAsync<MailItem>(...)`; `reply.Display()`/`forward.Display()` remain OUTSIDE the dispatched delegate to preserve the original thread affinity (behavior preservation).
- [x] [P6-T3] Remove `[ExcludeFromCodeCoverage]` from the dispatcher-unblocked members (`PopulateConversation(int)`, `RenderConversationCountAsync`, `JumpToFolderDropDownAsync`, `MenuDropDown`, `ToggleConversationCheckbox()` and `ToggleConversationCheckbox(ToggleState)`, `ToggleSaveCopyOfMail`, `EnumerateConversationAsync`, `MarkItemForDeletionAsync`) and add tests using a `Mock<IUiDispatcher>` whose `Invoke`/`InvokeAsync` execute the passed delegate synchronously (research §6). Acceptance: the named members carry no exemption; each is covered by ≥1 passing test that verifies both the dispatched behavior and that `_uiDispatcher` was invoked.
- [x] [P6-T4] Create NEW `QuickFiler/Viewers/IWebViewCoreInitializer.cs` (`CreateEnvironmentAsync`, `EnsureCoreWebView2Async` per research §3.3) and NEW `QuickFiler/Viewers/WebView2CoreInitializer.cs` implementing it as a thin adapter over `CoreWebView2Environment.CreateAsync` and `WebView2.EnsureCoreWebView2Async`, AND add `<Compile Include>` entries for both to `QuickFiler/QuickFiler.csproj`. Acceptance: both files compile and forward 1:1 to the WebView2 SDK; both are wired into the csproj.
- [x] [P6-T5] Add an optional `IWebViewCoreInitializer webViewInitializer = null` constructor parameter (production default `WebView2CoreInitializer`) and route `InitializeWebViewAsync` (`QfcItemController.ViewerSetup.cs`) through it; remove `[ExcludeFromCodeCoverage]` from `InitializeWebViewAsync` and add a routing test using `Mock<IWebViewCoreInitializer>` verifying the adapter was invoked with the expected arguments (cache-folder path, environment). Acceptance: `InitializeWebViewAsync` routes through the injected initializer and carries no exemption; the WebView2 SDK calls are confined to the adapter body (which remains exempt); the routing logic is covered by ≥1 passing test.
- [x] [P6-T6] Create NEW `QuickFiler/Interfaces/IMailItemActions.cs` (`Reply()`, `ReplyAll()`, `Forward()`, `Display()`, `bool UnRead { get; set; }`, `Save()`, `string EntryID { get; }` per research §3.4.3) and NEW `QuickFiler/Interfaces/MailItemActionsAdapter.cs` implementing it as a thin adapter over a live `MailItem`, AND add `<Compile Include>` entries for both to `QuickFiler/QuickFiler.csproj`. Acceptance: both files compile and forward 1:1 to `MailItem`; both are wired into the csproj.
- [x] [P6-T7] Construct an `IMailItemActions` from `Mail` at `SaveParameters` time (stored in a private field) and atomically re-point every direct `Mail.*` COM call to it (`Reply`/`ReplyAll`/`Forward`, `TxtboxBody_DoubleClick`'s `Mail.Display()`, `ApplyReadEmailFormat`'s `Mail.UnRead`/`Mail.Save()`, and the `Mail.EntryID` reads in `CollapseConversation`/`EnumerateConversation`); remove `[ExcludeFromCodeCoverage]` from `Reply`/`ReplyAll`/`Forward` (combined with the P6-T2 dispatcher seam), `TxtboxBody_DoubleClick` (extracted core, P6-T9), `ApplyReadEmailFormat`, and the `CollapseConversation`/`EnumerateConversation` fallback branch; add tests using `Mock<IMailItemActions>`. Acceptance: no direct `Mail.Reply/ReplyAll/Forward/Display/UnRead/Save/EntryID` COM call remains in `QfcItemController*.cs` (all route through `_mailActions`); the named members carry no exemption; each is covered by ≥1 passing test; the migration is atomic (no half-migrated `Mail.*`/`_mailActions.*` coexistence).
- [x] [P6-T8] Add optional factory-delegate constructor parameters for `ConversationResolver`, `FlagTasks`, and `EmailFiler` (`Func<...>` with production defaults matching the current inline `new ...(...)` expressions, per research §3.4.2/§3.4.4), route `PopulateConversation()`, `FlagAsTask`, `FlagAsTaskAsync`, and `MoveMailAsync` through them, remove `[ExcludeFromCodeCoverage]` from those four members, and add tests injecting factories that return test doubles and verifying the factory is called with the expected arguments and the result applied (`_itemViewer.FlagTaskDialogResult` etc.). Acceptance: the four members route through injected factories and carry no exemption; each is covered by ≥1 passing test that does not launch a real dialog or construct a live `ConversationResolver`/`EmailFiler`; the factory/adapter bodies themselves remain a minimal residual (Phase 7).
- [x] [P6-T9] Apply the thin-delegator pattern to the six `async void` handlers (`BtnPopOut_Click`, `BtnReply_Click`, `BtnReplyAll_Click`, `BtnForward_Click`, `TxtboxBody_DoubleClick`, `WebView2Control_CoreWebView2InitializationCompleted`, research §3.5): extract each handler's substantive body into a testable `internal Task <Name>Core(...)` (for the WebView handler, `internal async Task HandleWebViewInitializedAsync(bool isSuccess, Exception initException)`), leaving the `async void` shell as a `SynchronizationContext`-guard + `await <Name>Core(...)`; remove `[ExcludeFromCodeCoverage]` from the extracted core methods and add tests verifying routing (via `Mock<IQfcCollectionController>`/`Mock<IMailItemActions>`/subclass override); the one-line `async void` shells retain a minimal exemption. Acceptance: each extracted core method carries no exemption and is covered by ≥1 passing test; the `SynchronizationContext.Current is null` guard placement and the WebView handler's try/catch exception behavior are preserved; only the thin shells remain exempt.
- [x] [P6-T10] Split `WireEvents` (`QfcItemController.EventWiring.cs`) into a concrete-bound `WireControlTreeEvents()` (the ~14-line `ForAllControls` traversal + `CboFolders` exclusion list, retains exemption) and a testable `WireIntentEvents()` (the `IItemViewer` intent-event subscriptions), both called from `WireEvents()` in the exact original order; remove `[ExcludeFromCodeCoverage]` from `WireIntentEvents` and add a test using `Mock<IItemViewer>` `SetupAdd`/`Raise` verifying each intent event is subscribed. If the split pushes `EventWiring.cs` toward 500 lines, spill the pair into `QfcItemController.EventWiring.ControlTree.cs` with its own `<Compile Include>` entry. Acceptance: `WireIntentEvents` carries no exemption and is covered; event-wiring order and the `ForAllControls` exclusion list are preserved verbatim; all edited/created files are `< 500` lines.
- [x] [P6-T11] Verify the cumulative constructor-signature changes (P6-T2, P6-T5, P6-T8) remain non-breaking: confirm all 8 existing construction sites in `QuickFiler/Controllers/QfcCollectionController.cs` (×6, including line 140 unchanged) and `QuickFiler/Controllers/QfcQueue.cs` (×1) plus the two static factory methods compile without modification against the defaulted parameters. Acceptance: no construction site outside `QfcItemController` is modified; the solution compiles; behavior at every site is unchanged.
- [x] [P6-T12] Wire every new Phase 6 test file into `QuickFiler.Test/QuickFiler.Test.csproj` (including the adapter smoke tests `WpfUiDispatcherTests`, `WebView2CoreInitializerTests`, `MailItemActionsAdapterTests`) with explicit `<Compile Include>` entries. Acceptance: every new test file has a csproj entry; the test assembly compiles and discovers the new tests.
- [x] [P6-T13] Measure line counts for every production and test file created or edited in Phase 6 (the 6 new seam files, the controller partials, and the new/updated test files). Write `evidence/qa-gates/p6r-file-sizes.<ISO-8601>.md` with `Timestamp:` and the counts. Acceptance: every measured file is `< 500` lines.
- [x] [P6-T14] Run toolchain step 1 (`dotnet tool run csharpier .`). Write `evidence/qa-gates/p6r-csharpier.<ISO-8601>.md` with the four required fields. Acceptance: `EXIT_CODE: 0`; restart the loop if files changed.
- [x] [P6-T15] Run toolchain step 2 (analyzers). Write `evidence/qa-gates/p6r-analyzers.<ISO-8601>.md`. Acceptance: `EXIT_CODE: 0`, no new analyzer errors versus baseline.
- [x] [P6-T16] Run toolchain step 3 (nullable/TreatWarningsAsErrors). Write `evidence/qa-gates/p6r-nullable.<ISO-8601>.md`. Acceptance: `EXIT_CODE: 0`.
- [x] [P6-T17] Run toolchain step 4 (`vstest ... /EnableCodeCoverage`). Write `evidence/qa-gates/p6r-tests-coverage.<ISO-8601>.md` with numeric passing count, new/extracted-code coverage, affected-denominator coverage, and the current exemption count. Acceptance: the regression baseline tests still pass; `EXIT_CODE: 0`; new/extracted seam code coverage is `>= 90%`; the exemption count has decreased by the ~40 bucket-(ii) members unblocked in this phase.

---

### Phase 7 — Final Residual Boundary and Ratification Artifact (AC10)

Confirm the residual exemption set is the small, individually-justified bucket-(iii) set (research
§4, estimate ~6–8), attach a specific per-member technical justification to each, and produce the
reduced boundary artifact for maintainer ratification. A blanket/category exemption is not acceptable.

- [x] [P7-T1] Confirm the usage disposition of the raw-WinForms-parameter overloads `ToggleCbMenuItemAsync(ToolStripMenuItemCb)`, `ToggleCbMenuItemAsync(..., ToggleState)`, `ToggleCheckboxAsync(CheckBox)`, and `ToggleCheckboxAsync(..., ToggleState)` (`QfcItemController.Navigation.cs`): grep every call site across the solution. If a member has zero live call sites (dead after the cycle-1 Seam B narrowing per research §3.2), remove it; otherwise retain it and record it in the residual boundary with a per-member justification. Write the usage findings into the P7-T4 boundary artifact. Acceptance: each of the four overloads is either removed (zero call sites, recorded) or retained-and-justified; no member is left silently exempt without a disposition.
- [x] [P7-T2] Enumerate every `[ExcludeFromCodeCoverage]` member remaining after Phases 5–6 and verify each is a genuine bucket-(iii) residual: `ResolveControlGroups`/`ResolveControlGroupsAsync` (control-tree traversal is the method's purpose), `ToggleExpansionOn`/`ToggleExpansionOff` (private, `TlpCellSnapShot.ApplyState(Control)` out-of-scope collaborator), `JumpToAsync(Control)` and the two `'B'`/`'D'` expanded-action lambda bodies (no intent-focus member by design), `LoadFolderHandler`/`LoadFolderHandlerAsync` (COM-bound `FolderPredictor` construction, out of scope), `DoLoadConversationResolverCoreAsync` (deliberate virtual override seam), the three adapter implementation bodies (`WpfUiDispatcher`, `WebView2CoreInitializer`, `MailItemActionsAdapter`), and the six thin `async void` shells. Write `evidence/qa-gates/p7r-residual-verification.<ISO-8601>.md` with `Timestamp:`, `Command:` (grep), `EXIT_CODE:`, `Output Summary:` listing each residual member and its bucket-(iii) reason. Acceptance: every remaining exemption is individually named with a specific technical reason; no member remains exempt on a blanket/category basis; the total residual count is recorded (target ~6–8 members plus the adapter/shell shims).
- [x] [P7-T3] Attach an inline per-member `[ExcludeFromCodeCoverage]` justification comment (specific technical reason, not a category label) at every residual exemption site confirmed in P7-T2. Acceptance: each residual `[ExcludeFromCodeCoverage]` attribute is immediately preceded by a comment stating the specific irreducible dependency; no residual exemption lacks a per-member comment.
- [x] [P7-T4] Write the reduced boundary artifact `evidence/other/exemption-boundary.<ISO-8601>.md` for maintainer ratification: record the starting count (103, from P0-T7), the members de-exempted in Phases 5–6, the final residual set with per-member technical justification (from P7-T2/P7-T3), and an explicit statement that no blanket/category exemption remains. Acceptance: artifact enumerates the full 103 → residual reduction, justifies each residual member individually, and is marked for maintainer ratification at review.
- [x] [P7-T5] Record a follow-up-issue recommendation for the out-of-scope `TlpCellSnapShot.ApplyState(Control)` seam (retype to `ApplyState(IContainerControlLocal)` from the existing `UtilitiesCS.Interfaces.IWinForm` layer) that would unblock `ToggleExpansionOn`/`Off` in a future cycle, in `evidence/other/exemption-boundary.<ISO-8601>.md` (the P7-T4 artifact) under a "Deferred follow-up" section. Acceptance: the artifact contains a named follow-up recommendation with the specific seam and the two members it would unblock; no scope expansion is performed this cycle.
- [x] [P7-T6] Measure line counts for every production and test file edited or created in Phase 7. Write `evidence/qa-gates/p7r-file-sizes.<ISO-8601>.md` with `Timestamp:` and the counts. Acceptance: every measured file is `< 500` lines.
- [x] [P7-T7] Run toolchain step 1 (`dotnet tool run csharpier .`). Write `evidence/qa-gates/p7r-csharpier.<ISO-8601>.md` with the four required fields. Acceptance: `EXIT_CODE: 0`; restart the loop if files changed.
- [x] [P7-T8] Run toolchain step 2 (analyzers). Write `evidence/qa-gates/p7r-analyzers.<ISO-8601>.md`. Acceptance: `EXIT_CODE: 0`.
- [x] [P7-T9] Run toolchain step 3 (nullable/TreatWarningsAsErrors). Write `evidence/qa-gates/p7r-nullable.<ISO-8601>.md`. Acceptance: `EXIT_CODE: 0`.
- [x] [P7-T10] Run toolchain step 4 (`vstest ... /EnableCodeCoverage`). Write `evidence/qa-gates/p7r-tests-coverage.<ISO-8601>.md` with numeric passing count and final exemption count. Acceptance: the regression baseline tests still pass; `EXIT_CODE: 0`; the exemption count equals the residual set recorded in P7-T2 (target ~6–8 plus adapter/shell shims).

---

### Phase 8 — Final QA Loop, Coverage Delta, and Disposition (AC5, AC6, AC7)

Authoritative final-QC block. Each command step produces its own artifact; no aggregate-only
artifact. If any step changes files, restart the full loop from P8-T1.

- [x] [P8-T1] Run toolchain step 1 (`dotnet tool run csharpier .`) on the final tree. Write `evidence/qa-gates/final-r2-csharpier.<ISO-8601>.md` with the four required fields. Acceptance: `EXIT_CODE: 0` with no remaining format drift; if files change, restart the full loop from this task.
- [x] [P8-T2] Run toolchain step 2 (analyzers). Write `evidence/qa-gates/final-r2-analyzers.<ISO-8601>.md`. Acceptance: `EXIT_CODE: 0`, no analyzer errors (AC7).
- [x] [P8-T3] Run toolchain step 3 (nullable/TreatWarningsAsErrors). Write `evidence/qa-gates/final-r2-nullable.<ISO-8601>.md`. Acceptance: `EXIT_CODE: 0` (AC7).
- [x] [P8-T4] Run toolchain step 4 (`vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /EnableCodeCoverage`). Write `evidence/qa-gates/final-r2-tests-coverage.<ISO-8601>.md` recording numeric post-change repo-wide coverage, affected testable-non-exempt-denominator coverage, and new/extracted seam-code coverage. Acceptance: all tests pass; `EXIT_CODE: 0` (AC7).
- [x] [P8-T5] Compute the coverage delta against the P0-T5 baseline. Write `evidence/regression-testing/coverage-delta-r2.<ISO-8601>.md` reporting baseline repo-wide coverage, post-change repo-wide coverage, affected testable-non-exempt-denominator coverage, new/extracted seam-code coverage, and `QfcItemController` changed-line coverage. Acceptance: affected testable non-exempt denominator `>= 80%`; new/extracted code (including the four new seam types) `>= 90%`; changed lines show no regression versus baseline (AC5). The repo-wide floor is satisfied-with-documented-exception under the #223 authority-scoped precedent; residual repo-wide uplift is tracked under #197. If any in-scope threshold is unmet, mark the outcome remediation-required, not PASS.
- [x] [P8-T6] Compute the exemption-count delta. Write `evidence/qa-gates/final-r2-exemption-delta.<ISO-8601>.md` recording the P0-T7 starting count (103), the final residual count (P7-T2), the list of de-exempted members with their covering tests, and confirmation that no blanket/category exemption remains (AC8, AC10). Acceptance: the exemption count is reduced from 103 to the residual bucket-(iii) set; every de-exempted member maps to ≥1 passing test; no member exercisable through the narrowed `IItemViewer` or a mockable collaborator retains an exemption (AC8).
- [x] [P8-T7] Final 500-line-cap audit: collect line counts for every production file modified/created across Phases 5–7 (the 10 `QfcItemController*.cs` files, any spill partial, the 6 new seam files, `IItemViewer.cs`) and every touched/created test file. Write `evidence/qa-gates/final-r2-file-sizes.<ISO-8601>.md` with `Timestamp:` and all counts. Acceptance: every modified/created production and test file is `< 500` lines; `QfcCollectionController.cs` is `<=` its baseline and recorded as not-split pre-existing debt (AC6).
- [x] [P8-T8] Write `evidence/other/ac-traceability-r2.<ISO-8601>.md` mapping AC5, AC6, AC7, AC8, AC9, AC10 to the satisfying tasks and evidence artifacts (mapping table below), recording the reduced exemption boundary, the four introduced seams, and the deferred `TlpCellSnapShot` follow-up. Acceptance: all six ACs mapped to at least one completed task and one evidence artifact; the exemption-boundary ratification note and the follow-up recommendation are present.

---

## Acceptance Criteria Traceability

| AC | Requirement | Satisfying tasks | Evidence |
|---|---|---|---|
| AC5 | Affected testable non-exempt denominator `>= 80%`; new/extracted (incl. new seam types) `>= 90%`; no changed-line regression; repo-wide floor under authority-scoped exception (#197) | P5-T15, P6-T17, P7-T10, P8-T4, P8-T5 | `evidence/qa-gates/final-r2-tests-coverage`, `evidence/regression-testing/coverage-delta-r2` |
| AC6 | No production file modified/created exceeds 500 lines after the redesign (incl. new seam files) | P5-T11, P6-T13, P7-T6, P8-T7 | `evidence/qa-gates/*-file-sizes`, `evidence/qa-gates/final-r2-file-sizes` |
| AC7 | Full C# toolchain passes in order — csharpier, analyzers, nullable/TWAE, MSTest with coverage — no regressions | P5-T12..T15, P6-T14..T17, P7-T7..T10, P8-T1..T4 | `evidence/qa-gates/final-r2-csharpier`, `final-r2-analyzers`, `final-r2-nullable`, `final-r2-tests-coverage` |
| AC8 | Cycle-1 exemption set reduced by de-exempting the ~38 no-barrier members and covering them; no member exercisable through `IItemViewer`/a mockable collaborator retains an exemption | P5-T2..T9, P8-T6 | `evidence/qa-gates/p5r-tests-coverage`, `evidence/qa-gates/final-r2-exemption-delta` |
| AC9 | Four behavioral seams (`IUiDispatcher`, `IWebViewCoreInitializer`, `IMailItemActions` + factory delegates, thin-delegator `async void` handlers) introduced per DI-seam ordering, covered `>= 90%`, behavior preserved; no leaf-control interface layer | P6-T1..T11 | `evidence/qa-gates/p6r-tests-coverage`, `evidence/qa-gates/p6r-file-sizes` |
| AC10 | Any residual `[ExcludeFromCodeCoverage]` individually justified per-member (no blanket/category exemption); reduced boundary documented for maintainer ratification | P7-T1..T5, P8-T6 | `evidence/qa-gates/p7r-residual-verification`, `evidence/other/exemption-boundary.<ISO-8601>` |

## Invariants Encoded in This Plan

- Runtime behavior of the QuickFiler item viewer is preserved; every edit is a testability refactor,
  re-confirmed by the per-phase passing-test-count regression gate against the P0-T5 baseline.
- `ItemViewer` remains a `UserControl`-derived, `[ExcludeFromCodeCoverage]` partial class; Designer
  code is untouched; event-wiring order and the `ForAllControls` exclusion list are preserved when
  `WireEvents` is split (P6-T10).
- `IQfcItemController` is NOT changed. `QfcCollectionController.cs` is NOT split; its line-140
  `grp.ItemViewer.LblItemNumber.Text` stays on the concrete `ItemViewer`. `QfcItemGroup.ItemViewer`
  stays concrete.
- New seam constructor parameters are optional with production defaults (non-breaking for the 8
  existing construction sites; verified P6-T11). COM-adapter substitution is atomic (P6-T7).
- No leaf-control interfaces (`IButton`/`ILabel`/etc.) or `IList<IButton>` retyping (Option B declined).
- `TlpCellSnapShot.ApplyState(Control)` is out of scope; a follow-up issue is recorded (P7-T5).
- MSTest + Moq + FluentAssertions only; no temporary files; deterministic tests.

## Notes

- Plan-path continuity: this is the single cycle-2 remediation plan file for issue #227; preflight
  revisions update this file in place. No sibling timestamped plan files are created this cycle.
- Phase ordering follows the seam-redesign research: remove no-barrier exemptions first (Phase 5,
  lowest risk, no new production types), then introduce the four seams and route bucket-(ii) members
  through them (Phase 6), then confirm and justify the small residual and produce the ratification
  artifact (Phase 7), before the authoritative final QA loop and coverage delta (Phase 8).
- Directional counts (~38 bucket-(i), ~40 bucket-(ii), ~6–8 bucket-(iii)) are re-verified per member
  as each phase's coverage gate runs (research §4 honest-caveat); the coverage-gate artifacts, not
  this plan's estimates, are the final tally.
