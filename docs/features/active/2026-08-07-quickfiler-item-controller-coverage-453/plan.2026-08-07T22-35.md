# quickfiler-item-controller-coverage — Atomic Implementation Plan

- **Issue:** #453 (https://github.com/drmoisan/TaskMaster/issues/453)
- **Parent epic:** #136 QuickFiler Per-File 80% Coverage — child F10 (wave 1, band C3)
- **Work Mode:** `full-feature` (`spec.md` + `user-story.md` are the authoritative AC sources)
- **Owner:** drmoisan
- **Last Updated:** 2026-08-07T22-35
- **Status:** Draft — pending preflight
- **Version:** 1.0

## Required References

Apply in this order (`policy-compliance-order`):

1. `CLAUDE.md`
2. `.claude/rules/general-code-change.md`
3. `.claude/rules/general-unit-test.md`
4. `.claude/rules/csharp.md`

Requirements sources: `docs/features/active/2026-08-07-quickfiler-item-controller-coverage-453/spec.md`
(AC-1..AC-21, §15 Definition of Done) and
`docs/features/active/2026-08-07-quickfiler-item-controller-coverage-453/user-story.md` (US-1..US-14).
Research inputs: the 13 artifacts under
`docs/features/active/2026-08-07-quickfiler-item-controller-coverage-453/research/`.

**All work must comply with these policies; this plan does not duplicate their content.**

---

## Conventions Used By Every Task

- `<FEATURE>` = `docs/features/active/2026-08-07-quickfiler-item-controller-coverage-453`.
  All evidence resolves to `<FEATURE>/evidence/<kind>/` — `baseline/`, `qa-gates/`, `other/`,
  `issue-updates/`. No evidence is written to any `artifacts/` path (AC-19).
- All paths are **repo-relative**. This plan is executed in a different worktree from the one that
  prepared it.
- Every command-bearing task writes an artifact carrying `Timestamp:`, `Command:`, `EXIT_CODE:`,
  `Output Summary:`.
- Coverage figures are **recomputed from the class-level `<line>` children** per `spec.md` §4.1
  (unique `<line>` entries and `hits="0"` for lines; summed `condition-coverage` numerators and
  denominators for branches). The emitted `line-rate` / `branch-rate` attributes are **never** used
  for a gate decision (open issue #441). Both figures are committed side by side (AC-5).
- Every per-file coverage verification task asserts **>= 80% line AND >= 75% branch, reported
  independently** (AC-1, US-8). A line figure at or above 80% is never accepted as branch evidence.
- Test conventions: MSTest `[TestClass]`/`[TestMethod]`, Moq, FluentAssertions, Arrange–Act–Assert.
  Prohibited in any test: `Thread.Sleep`, `Task.Delay`, real wall-clock waits, temporary files,
  external services, live forms, popups, `DateTime.Now`/`UtcNow`, `Random.Shared`, arming the real
  `_emailIsReadTimer`, starting a real WebView2 core, reaching `FlagTasks.Run` or `MessageBox.Show`,
  constructing a real `WindowsFormsSynchronizationContext` on the MSTest thread (AC-14).
- Every test that mutates ambient `SynchronizationContext` restores it in a `finally`.
- **No `*.StaTests.cs` file is created** (US-11). Headless real `ItemViewer` construction runs in
  plain `[TestClass]`/`[TestMethod]`, matching the existing convention at
  `QuickFiler.Test/Controllers/QfcItemController.ViewerSetupTests.cs:379` and
  `QuickFiler.Test/Controllers/QfcItemController.EventWiringTests.cs:229`/`:320`.
- Every new test file gets its own `<Compile Include="Controllers\....cs" />` entry in
  `QuickFiler.Test/QuickFiler.Test.csproj`; the one new production file gets its entry in
  `QuickFiler/QuickFiler.csproj`. Both are legacy non-SDK, no globbing, CRLF-terminated. Use the
  **Edit tool** or `perl -0777` with explicit `\r\n`. **Never** a git-bash `sed -i`. One minimal
  adjacent hunk per edit (AC-13, spec §8.4).
- No file listed in `spec.md` §2.2 is edited. No member is added to `IQfcItemController` or
  `IItemViewer` (AC-10).

### Binding scope decisions carried into this plan

1. **Exemption outcome is exactly 19 → 15.** 19 − 3 dead members deleted − 1 unratified resolved
   (`ViewerSetup.cs:132`) = 15. The remaining 15 are ratified under #227. **No task removes a
   ratified attribute**, and **no task builds the issue-#230 WinForms message-pump seam** (AC-8,
   spec §3.4, §3.6). Research recommendations that would take the count below 15 are therefore
   **not scheduled**, specifically: `file-QfcItemController.Initialization.md` Group B (de-exempting
   `:168`, `:260`, `:291`), `file-QfcItemController.ViewerSetup.md` Group D (de-exempting `:253`),
   and `file-QfcItemController.Navigation.md` NV-1/NV-2/NV-4/NV-5 (de-exempting `:173`, `:191`).
   Every one of those files clears both gates without them (see the per-phase projections).
2. **The two `Navigation.cs` attributes are RETAINED** and their stale comments are corrected
   (AC-21, US-3). Retention is verified explicitly in `[P8-T14]`.
3. **AC-4 (atomic de-exemption) applies to exactly one attribute**, `EnsureBreadcrumbPipeline` at
   `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs:132`. AC-4's operative clause is "no
   per-file coverage measurement taken between tasks shows a file below either floor as a result of
   a de-exemption". The covering tests C1–C5 (`[P3-T19]`..`[P3-T23]`) therefore land **before** the
   removal task `[P3-T24]`, which carries the post-removal measurement as its own acceptance
   criterion. Removing the attribute earlier would drive `ViewerSetup.cs` to roughly 78% line
   mid-flight; deferring the tests would do the same. This ordering is the only AC-4-compliant one.
4. **Permitted production change is closed.** Only: the three deletions; the `Func<int, Task>` delay
   seam; the `Func<FlagTasks, bool, DialogResult>` runner seam; the `Func<TimerCallback, int,
   IDisposable>` timer factory; the `Func<SynchronizationContext>` factory plus one extracted
   `EnsureUiSynchronizationContext()` helper; the `QfcCidImageResolver` extraction; the two
   `Navigation.cs` comment corrections; the stale exemption-comment corrections; and the one
   permitted attribute removal. **The `Action<string> _showUserMessage` seam is NOT scheduled** — it
   is outside the permitted set, so `QfcItemController.MailActions.cs:115-122` stays uncovered and
   is recorded as an accepted residual in `[P10-T18]`.
5. **net481 constraint.** `QuickFiler/QuickFiler.csproj` targets `v4.8.1`. No `record`, no
   `record struct`, no `init`-only setter anywhere in the change set (AC-11).
6. **Out of scope to fix:** #480, #481, #482, #483, #484, #485. Characterisation tests that pin
   current behaviour are in scope and are marked as such so a future fix knows to update them. Do
   not re-file #441, #457, #463, #444, #450, #230, #427, #438, #440 (AC-16).

---

## Implementation Plan (Atomic Tasks)

### Phase 0 — Baseline Capture, Policy Reads, and the F1 Upstream Gate

- [ ] [P0-T1] Read `CLAUDE.md` in full before any other work in this feature
  - Acceptance: file read recorded as entry 1 of the Phase 0 evidence artifact
- [ ] [P0-T2] Read `.claude/rules/general-code-change.md` in full
  - Acceptance: file read recorded as entry 2 of the Phase 0 evidence artifact
- [ ] [P0-T3] Read `.claude/rules/general-unit-test.md` in full
  - Acceptance: file read recorded as entry 3 of the Phase 0 evidence artifact
- [ ] [P0-T4] Read `.claude/rules/csharp.md` in full
  - Acceptance: file read recorded as entry 4 of the Phase 0 evidence artifact
- [ ] [P0-T5] Write `<FEATURE>/evidence/baseline/phase0-instructions-read.md`
  - Acceptance: artifact contains `Timestamp:`, `Policy Order:` naming the four files in the order
    above, and the explicit list of files read
- [ ] [P0-T6] Evaluate the **UPSTREAM DEPENDENCY HALT GATE on F1** (`quickfiler-coverage-ledger`,
      issue #432, wave 0) and record the outcome
  - Gate: HALT if, **at execution time**, either
    `docs/features/epics/quickfiler-per-file-coverage/coverage-ledger.md` or F1's per-file coverage
    harness is absent from the working tree
  - **Preparation/preflight note, binding on the preflight reviewer:** as of plan preparation F1 had
    not been executed and **neither deliverable exists on disk** — the epic folder contains only
    `docs/features/epics/quickfiler-per-file-coverage/epic.md`. That absence during PREPARATION and
    PREFLIGHT is EXPECTED, is legitimate, and **must not be treated as a blocker or a preparation
    defect**. This gate is an execution-time read, not a preflight-evaluable condition
  - F1's per-file report derives from the base harness `scripts/vscode/Invoke-MSTestWithCoverage.ps1`,
    which exists today alongside `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1`
  - Acceptance: `<FEATURE>/evidence/baseline/f1-upstream-gate.<timestamp>.md` records
    `Timestamp:`, the resolved path of the ledger, the resolved path of F1's per-file report script,
    and `GATE: PASS` or `GATE: HALT` with the missing artifact named
- [ ] [P0-T7] Record the baseline tree state: `git rev-parse HEAD` and `git status --porcelain`
  - Acceptance: `<FEATURE>/evidence/baseline/tree-state.<timestamp>.md` records `Timestamp:`,
    `Command:`, `EXIT_CODE:`, `Output Summary:` with the HEAD sha and a clean porcelain result.
    The sha is recorded for provenance only; later gates compare tree invariants, not the sha
- [ ] [P0-T8] Capture the formatting baseline: `dotnet tool run csharpier check .`
  - Acceptance: `<FEATURE>/evidence/baseline/csharpier.<timestamp>.md` with `Timestamp:`,
    `Command:`, `EXIT_CODE:`, `Output Summary:`
- [ ] [P0-T9] Capture the analyzer baseline:
      `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
  - Acceptance: `<FEATURE>/evidence/baseline/msbuild-analyzers.<timestamp>.md` with the four required
    fields and the warning/error counts in `Output Summary:`
- [ ] [P0-T10] Capture the nullable/type-check baseline:
      `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true`
  - Acceptance: `<FEATURE>/evidence/baseline/msbuild-nullable.<timestamp>.md` with the four required
    fields
- [ ] [P0-T11] Capture the test + coverage baseline on this child's branch using F1's per-file
      harness (base runner `scripts/vscode/Invoke-MSTestWithCoverage.ps1`)
  - Acceptance: `<FEATURE>/evidence/baseline/coverage-per-file.<timestamp>.md` with `Timestamp:`,
    `Command:`, `EXIT_CODE:`, `Output Summary:`, and a table carrying **numeric per-file line AND
    branch coverage for all ten production files** — `QfcItemController.cs`, `.Initialization.cs`,
    `.ViewerSetup.cs`, `.Conversation.cs`, `.FolderHandling.cs`, `.EventWiring.cs`,
    `.EventHandlers.cs`, `.Navigation.cs`, `.FocusAndTheme.cs`, `.MailActions.cs`. Figures are
    **recomputed per `spec.md` §4** from the class-level `<line>` children; the emitted
    `line-rate`/`branch-rate` values are recorded alongside as the second column with an explicit
    note citing open issue **#441**. `QuickFiler/Interfaces/IQfcItemController.cs` is reported
    **N/A**, never 0%. The raw Cobertura XML is committed under `<FEATURE>/evidence/qa-gates/`
- [ ] [P0-T12] Record the repository-wide baseline line coverage figure from the same run
  - Acceptance: `<FEATURE>/evidence/baseline/coverage-repository-wide.<timestamp>.md` with the four
    required fields and the numeric repository-wide line rate in `Output Summary:` (AC-9)
- [ ] [P0-T13] Confirm the exemption inventory **before any attribute is touched**
  - Acceptance: `<FEATURE>/evidence/other/exemption-inventory-baseline.<timestamp>.md` records that
    a grep for `ExcludeFromCodeCoverage` across the ten `QuickFiler/Controllers/QfcItemController*.cs`
    files plus `QuickFiler/Interfaces/IQfcItemController.cs` returns **exactly 19** hits, all
    member-level and none on a `partial class` declaration, at
    `Initialization.cs:138, 168, 200, 260, 291, 403, 436`; `ViewerSetup.cs:38, 132, 253`;
    `Navigation.cs:173, 191`; `EventHandlers.cs:60, 83, 97, 111, 125`; `EventWiring.cs:99`;
    `Conversation.cs:79`; and that the **18 ratified / 1 unratified** split matches `spec.md` §3
    (the unratified site being `ViewerSetup.cs:132`). HALT and notify if the count or the split
    differs
- [ ] [P0-T14] Record the file-size headroom baseline for every size-constrained file in the change
      set
  - Acceptance: `<FEATURE>/evidence/baseline/file-sizes.<timestamp>.md` records current line counts
    for `QuickFiler/Controllers/QfcItemController.Initialization.cs` (expected 466),
    `QfcItemController.ViewerSetup.cs` (426), `QfcItemController.cs` (323),
    `QfcItemController.EventHandlers.cs` (219),
    `QuickFiler.Test/Controllers/QfcItemController.FolderHandlingTests.cs` (498),
    `QfcItemController.FocusAndThemeTests.cs` (497), `QfcItemController.EventHandlersTests.cs` (438),
    `QfcItemController.ViewerSetupTests.cs` (407), `QfcItemController.NavigationTests.cs` (391),
    `QfcItemController.EventWiringTests.cs` (374), `QfcItemController.TestSupport.cs` (365), and the
    count of `<Compile Include=...>` entries in `QuickFiler.Test/QuickFiler.Test.csproj` (expected 107)

### Phase 1 — QfcItemController.cs

Baseline 100% line / 78.57% branch (11/14). Zero attributes, zero production change. Target after
this phase: 100% line / 100% branch. All three tests extend the existing
`QuickFiler.Test/Controllers/QfcItemController.PropertiesTests.cs`, so no csproj edit is required.

- [ ] [P1-T1] Add test `TopFolderScore_WhenHandlerHasSuggestions_ReturnsTopScore` (research T1) to
      `QuickFiler.Test/Controllers/QfcItemController.PropertiesTests.cs`
  - Acceptance: `PropController` with a `Mock<IFolderSearchHandler>` whose `Suggestions` returns a
    suggestion set with a known top score, injected by
    `QfcItemControllerTestSupport.SetField(controller, "_folderHandler", ...)`; asserts
    `TopFolderScore` equals that score; closes the non-null side of both conditions at
    `QuickFiler/Controllers/QfcItemController.cs:254`
- [ ] [P1-T2] Add test `TopFolderScore_WhenSuggestionsNull_ReturnsZero` (research T2) to the same file
  - Acceptance: handler non-null with `Suggestions` returning `null`; asserts `TopFolderScore == 0`
    and that no exception is thrown; closes the null side of condition 1 at `:254`
- [ ] [P1-T3] Add test `Height_WhenViewerNotAttached_Throws` (research T3) to the same file
  - Acceptance: `PropController` with `_itemViewer` left null; asserts
    `Invoking(...).Should().Throw<NullReferenceException>()` on `Height`
    (`QuickFiler/Controllers/QfcItemController.cs:132`). Characterisation only — pins the current
    unguarded dereference without changing it
- [ ] [P1-T4] Verify per-file coverage for `QuickFiler/Controllers/QfcItemController.cs`
  - Acceptance: `<FEATURE>/evidence/qa-gates/coverage-QfcItemController.<timestamp>.md` records
    line >= 80% and branch >= 75% **independently**, recomputed per `spec.md` §4, with both the
    harness figure and the class-level-union figure. Expected 100% line / 100% branch

### Phase 2 — QfcItemController.Initialization.cs

Baseline 91.79% line (123/134) / 96.15% branch (25/26) — both gates already pass because the seven
exempt bodies sit outside the denominator. The four ratified attributes that survive
(`:168`, `:200`, `:260`, `:291`) are **retained** per binding decision 1, so the denominator does not
move. Work here is: delete the three dead exempt members (AC-7), close the last uncovered branch and
the two default-factory lambda lines, pin the ordering/re-entrancy/dispose-before-setup invariants,
and correct the four stale exemption comments (US-2). All tests extend the existing
`QuickFiler.Test/Controllers/QfcItemController.InitializationTests.cs` (193 lines), which has room.

- [ ] [P2-T1] Confirm no reflection-based caller exists for the three dead members before deleting them
  - Acceptance: `<FEATURE>/evidence/other/dead-member-reflection-check.<timestamp>.md` records a
    solution-wide search for `"Initialize"`, `"CreateAsync"`, `"CreateSequentialAsync"` used as
    reflection name literals (`GetMethod`, `InvokeMember`, `CreateInstance`, `nameof`) and reports
    zero hits targeting these three members, following the #447 precedent. HALT if any hit is found
- [ ] [P2-T2] Delete the dead private 9-argument `Initialize(...)` at
      `QuickFiler/Controllers/QfcItemController.Initialization.cs:135-163` (comment, attribute, and body)
  - Acceptance: the member and its `[ExcludeFromCodeCoverage]` at `:138` are gone; the solution
    builds; **no test is written for the deleted member**
- [ ] [P2-T3] Delete the dead `public static CreateAsync(...)` (`:400-431`) and
      `public static CreateSequentialAsync(...)` (`:433-464`) in a single dedicated task so the
      **`public static` API reduction is reviewable in isolation** (AC-7, US-12)
  - Acceptance: both members and their `[ExcludeFromCodeCoverage]` attributes at `:403` and `:436`
    are gone; the solution builds; the task's evidence note states the mitigating facts — the
    declaring type is `internal partial class QfcItemController`, neither member is declared on
    `IQfcItemController`, and a solution-wide grep finds zero call sites including in
    `QuickFiler.Test`. **No test is written for either deleted member**
- [ ] [P2-T4] Verify the post-deletion state of `QuickFiler/Controllers/QfcItemController.Initialization.cs`
  - Acceptance: line count is <= 500 (AC-12), computed as 466 − 29 for `[P2-T2]` − 65 for
    `[P2-T3]` = 372 raw; accept **370-372**, because `csharpier format` normalizes the raw result
    down to 370 by removing the doubled blank line left where `:135-163` sat between the blanks at
    `:134` and `:164`, and the blank line left immediately before the class-closing brace where
    `:400-464` sat after the blank at `:399` (verified empirically during preflight; csharpier can
    only shrink this file, never grow it); the file now carries exactly
    **4** `[ExcludeFromCodeCoverage]` attributes; the family-wide count is now **16**; recorded in
    `<FEATURE>/evidence/other/exemption-count-after-deletions.<timestamp>.md`
- [ ] [P2-T5] Add test `SaveParameters_WhenMailItemNonNull_BuildsMailItemActionsAdapter` (research A1)
      to `QuickFiler.Test/Controllers/QfcItemController.InitializationTests.cs`
  - Acceptance: `HarnessController` + `Mock<IFilerHomeController>` + `Mock<IItemViewer>` + a
    **non-null** `Mock<MailItem>`; asserts `GetField(controller, "_mailActions")` is a
    `MailItemActionsAdapter`; closes condition 1 of the `??=` at `:392`, taking the file to 26/26
    branch
- [ ] [P2-T6] Add test `SaveParameters_DefaultFolderPredictorFactory_ConstructsFolderPredictor`
      (research A3) to the same file
  - Acceptance: constructed without injecting `folderPredictorFactory`; reads the field, invokes the
    captured delegate with `Mock<IApplicationGlobals>`, an object, and a
    `FolderPredictor.InitOptions`; asserts a `FolderPredictor` is returned; covers `:396`.
    Precondition confirmed by research: `FolderPredictor(IApplicationGlobals, object, InitOptions)`
    ignores both parameters and performs no COM work
- [ ] [P2-T7] Add test `Constructor_WhenAllSeamsSupplied_NeverOverwritesInjectedSeam` (research A4)
      to the same file
  - Acceptance: constructs with **all eight** optional seams supplied as distinct mocks/delegates;
    asserts each private field is reference-equal to what was passed, proving the `??=` block at
    `:380-397` never overwrote an injected seam; pins invariants INIT-5 and INIT-6
- [ ] [P2-T8] Add test `SaveParameters_WhenHomeControllerKeyboardHandlerNull_LeavesHandlerNull`
      (research A5) to the same file
  - Acceptance: `Mock<IFilerHomeController>` returning `null` for `KeyboardHandler`; asserts
    `_kbdHandler` is null and no exception is thrown; characterises the unguarded collaborator pull
    at `:372-375`
- [ ] [P2-T9] Add test `SaveParameters_AfterCleanup_LeavesMailActionsBoundToPreviousMailItem`
      (research A6) to the same file
  - Acceptance: constructs with `Mock<MailItem>` A, captures `_mailActions`, calls `Cleanup()`, calls
    `SaveParameters` again with a different `Mock<MailItem>` B; asserts `_mailActions` is still the
    instance bound to A while `_globals`/`_itemViewer`/`Parent` are restored. **Characterisation of
    open issue #484** — pins current behaviour, changes nothing, and the test's doc comment names
    #484 so a future fix knows to update it
- [ ] [P2-T10] Record the accepted uncovered residual at
      `QuickFiler/Controllers/QfcItemController.Initialization.cs:390`
  - Acceptance: `<FEATURE>/evidence/other/uncovered-residuals.<timestamp>.md` records that research
    proposal A2 (invoking the default `_flagTasksFactory`) is **not scheduled** because
    `TaskVisualization/FlagTasks.cs`'s constructor calls `globals.Ol.App.ActiveExplorer()` at `:52`
    and can raise a `MessageBox` at `:56-61`, which the unit-test policy prohibits; line 390 is
    therefore left uncovered by design, and the file still clears both gates
- [ ] [P2-T11] Correct the four surviving stale exemption comments in
      `QuickFiler/Controllers/QfcItemController.Initialization.cs` (US-2)
  - Acceptance: the comments preceding `:168`, `:200`, `:260`, `:291` no longer claim "not
    unit-reachable without a live `ItemViewer`" — that barrier is defeated by headless
    `new QuickFiler.ItemViewer()` construction proven at
    `QuickFiler.Test/Controllers/QfcItemController.ViewerSetupTests.cs:379` and
    `QfcItemControllerBreadcrumbDropDownTests.cs:365-383`. Each rewritten comment states the real
    residual barrier and cites issue **#230** by number as the externally-tracked justification
    (AC-8). The attributes themselves are **retained**; no attribute is removed by this task
- [ ] [P2-T12] Verify per-file coverage for `QuickFiler/Controllers/QfcItemController.Initialization.cs`
  - Acceptance: `<FEATURE>/evidence/qa-gates/coverage-Initialization.<timestamp>.md` records line
    >= 80% and branch >= 75% **independently**, recomputed per `spec.md` §4, dual-figure. Expected
    approximately 93% line / 100% branch, with the denominator unchanged at 134 because the deleted
    members' lines were never in it

### Phase 3 — QfcItemController.ViewerSetup.cs

Baseline 72.5% line (116/160) / 55.6% branch (30/54) — **the only file failing both gates**.
Test-only work plateaus at 78.8%, so the host-neutral `QfcCidImageResolver` extraction is
unavoidable here. This phase also carries the one permitted de-exemption
(`EnsureBreadcrumbPipeline`, `:132`, unratified drift) and the shared headless-viewer fixture
consolidation required by AC-15. Group D (de-exempting `ResolveControlGroupsAsync` at `:253`) is
**not scheduled** per binding decision 1.

- [ ] [P3-T1] Create `QuickFiler.Test/Controllers/QfcItemController.TestSupport.Fixtures.cs` with
      `HeadlessViewerScope` and `NullSynchronizationContextScope`, and add its
      `<Compile Include="Controllers\QfcItemController.TestSupport.Fixtures.cs" />` entry to
      `QuickFiler.Test/QuickFiler.Test.csproj`
  - Acceptance: a second shared-fixture file exists because
    `QuickFiler.Test/Controllers/QfcItemController.TestSupport.cs` is at 365 lines and cannot absorb
    the full promoted helper set without breaching 500. `HeadlessViewerScope : IDisposable` installs
    a plain `SynchronizationContext`, constructs `new QuickFiler.ItemViewer()`, and restores the
    context on dispose. The csproj hunk is minimal, adjacent to the existing
    `Controllers\QfcItemController*` entries, and **CRLF is preserved**. The solution builds
- [ ] [P3-T2] Consolidate the triplicated headless-`ItemViewer` construction onto `HeadlessViewerScope`
      (AC-15)
  - Acceptance: the `private sealed class ViewerScope` at
    `QuickFiler.Test/Controllers/QfcItemControllerBreadcrumbDropDownTests.cs:365-383` and the inline
    copies in `QfcItemController.EventWiringTests.cs` and `QfcItemController.ViewerSetupTests.cs` all
    consume the shared scope; no fourth copy is created; the existing tests that depend on them still
    pass unchanged in behaviour
- [ ] [P3-T3] Create `QuickFiler.Test/Controllers/QfcItemController.ViewerSetupLifecycleTests.cs`
      with a `[TestClass]` skeleton and add its `<Compile Include>` entry to
      `QuickFiler.Test/QuickFiler.Test.csproj`
  - Acceptance: new file created because `QfcItemController.ViewerSetupTests.cs` at 407 lines cannot
    absorb four more tests without approaching 500; CRLF preserved; solution builds
- [ ] [P3-T4] Add test `ConfigureAndAttachBreadcrumbAsync_WhenAttachCollapsedNull_ThrowsBeforeAnyConfiguration`
      (research A1) to `QfcItemController.ViewerSetupLifecycleTests.cs`
  - Acceptance: asserts `ArgumentNullException` with `ParamName == "attachCollapsed"` and that no
    configuration side effect occurred (`Mock<IWebViewCoreInitializer>(MockBehavior.Strict)` with
    `VerifyNoOtherCalls()`); covers line 178 and takes branch 177 to 2/2; pins invariant VS-8
- [ ] [P3-T5] Add test `OnBreadcrumbUnhandledArrow_WhenKeyboardHandlerNull_DoesNotThrow`
      (research A2) to the same file
  - Acceptance: `HeadlessViewerScope` viewer as `sender`, `_kbdHandler` left null, invoked through
    `QfcItemControllerTestSupport.InvokeNonPublic`; asserts no throw; takes branch 187 to 2/2.
    Asserts **only** the null-handler path — adds nothing that constrains
    `BreadcrumbArrowFallThrough` semantics, so open issue **#440** stays fixable (AC-16, US-6)
- [ ] [P3-T6] Add test `GetItemSummary_WithPopulatedHelper_RendersSubjectSenderAndDate`
      (research A3) to the same file
  - Acceptance: `HarnessController` with `ItemHelper = new MailItemHelper { Subject, SentDate,
    SenderName }`; asserts the summary contains subject and sender and the date rendered by the
    **same** culture-dependent `ToString` calls used at
    `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs:424`. The test **must not** mutate
    `Thread.CurrentThread.CurrentCulture` and must not hard-code a literal date string; covers
    line 424. This is the recorded disposition for `GetItemSummary()` under AC-7: **retained and
    covered by a behavioural test, not exempted and not deleted**
- [ ] [P3-T7] Add test `Cleanup_CalledTwice_IsIdempotentAndDoesNotThrow` (research A4) to the same file
  - Acceptance: extends the arrangement of the existing `Cleanup_NullsTrackedPrivateFields`; calls
    `Cleanup()` twice; asserts the second call does not throw and all tracked fields remain null;
    pins invariant VS-5
- [ ] [P3-T8] Extract the host-neutral CID image resolver: create
      `QuickFiler/Controllers/QfcCidImageResolver.cs`, add its
      `<Compile Include="Controllers\QfcCidImageResolver.cs" />` entry to
      `QuickFiler/QuickFiler.csproj`, move `ResolveImageMimeType` out of
      `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs:191-202`, and replace the
      `WebResourceRequested` lambda body at `:81-102` with the thin adapter (research B0)
  - Acceptance: the new file declares `internal static class QfcCidImageResolver` with
    `internal static QfcCidImage Resolve(string requestUri, IAttachment[] attachments)` (null when
    unresolvable) and `internal static string ResolveMimeType(string fileExtension)`, plus a DTO
    that is a **plain sealed class or `readonly struct` with an ordinary constructor — no `record`,
    no `record struct`, no `init`-only setter** (net481, AC-11). `UtilitiesCS.CidImageResolver.
    BuildContentIdMap` is **called, not moved**; no `UtilitiesCS` file is edited. The extraction is
    behaviour-preserving: same URI parsed, same map built at request time (preserving the
    pooled-viewer semantics documented at `ViewerSetup.cs:71-75`), same MIME defaults. The csproj
    hunk is minimal, adjacent to the existing `Controllers\QfcItemController*` entries at lines
    328-337, and **CRLF is preserved**. The file is created, wired, and consumed in this single task
    so no unreferenced type exists between tasks. The solution builds with analyzers enabled
- [ ] [P3-T9] Create `QuickFiler.Test/Controllers/QfcCidImageResolverTests.cs` with a `[TestClass]`
      skeleton and add its `<Compile Include>` entry to `QuickFiler.Test/QuickFiler.Test.csproj`
  - Acceptance: file mirrors the production tree; CRLF preserved; solution builds
- [ ] [P3-T10] Add `[DataTestMethod]` `ResolveMimeType_ForKnownImageExtensions_ReturnsImageMimeType`
      (research B1) to `QfcCidImageResolverTests.cs`
  - Acceptance: rows `.jpg`→`image/jpeg`, `.jpeg`→`image/jpeg`, `.png`→`image/png`,
    `.gif`→`image/gif`, `.bmp`→`image/bmp`; positive flow
- [ ] [P3-T11] Add `[DataTestMethod]`
      `ResolveMimeType_ForUnknownOrMissingExtension_ReturnsOctetStream` (research B2) to the same file
  - Acceptance: rows `null`, `""`, `.pdf`, `.docx` → `application/octet-stream`; negative/edge flow
- [ ] [P3-T12] Add test `ResolveMimeType_WhenExtensionUpperCase_NormalisesToLowerInvariant`
      (research B3) to the same file
  - Acceptance: `ResolveMimeType(".PNG")` returns `image/png`; pins the `ToLowerInvariant`
    normalisation. B1–B3 together close all 12 conditions previously at `ViewerSetup.cs:195`
- [ ] [P3-T13] Add test `Resolve_WhenUriLastSegmentMatchesContentId_ReturnsAttachmentBytesAndMimeType`
      (research B4) to the same file
  - Acceptance: positive flow; also pins invariant VS-9 by proving the content-id map is built from
    the attachment array supplied at call time
- [ ] [P3-T14] Add test `Resolve_WhenContentIdUnmatched_ReturnsNull` (research B5) to the same file
  - Acceptance: negative flow; returns null rather than throwing
- [ ] [P3-T15] Add test `Resolve_WhenUriHasEmptyLastSegment_ReturnsNull` (research B6) to the same file
  - Acceptance: edge flow, URI ending in `/`; returns null
- [ ] [P3-T16] Add test `Resolve_WhenUriMalformed_ThrowsUriFormatException` (research B7) to the same file
  - Acceptance: **characterisation** of the current unguarded `new Uri(...)` behaviour, matching open
    issue **#485**. The test's doc comment names #485 so a future guard fix tightens rather than
    inverts the assertion (US-5). Behaviour is not changed
- [ ] [P3-T17] Add test `Resolve_WhenAttachmentArrayNull_CharacterisesBuildContentIdMapBehaviour`
      (research B8) to the same file
  - Acceptance: negative flow; documents the current behaviour of
    `UtilitiesCS.CidImageResolver.BuildContentIdMap(null)` without changing it
- [ ] [P3-T18] Create `QuickFiler.Test/Controllers/QfcItemController.BreadcrumbPipelineTests.cs`
      with a `[TestClass]` skeleton and add its `<Compile Include>` entry to
      `QuickFiler.Test/QuickFiler.Test.csproj`
  - Acceptance: CRLF preserved; solution builds
- [ ] [P3-T19] Add test `EnsureBreadcrumbPipeline_WhenViewerIsMock_ReturnsEarlyWithoutSubscribing`
      (research C1) to `QfcItemController.BreadcrumbPipelineTests.cs`
  - Acceptance: `_itemViewer` is a `Mock<IItemViewer>` rather than a concrete `ItemViewer`; asserts
    nothing is subscribed and `_breadcrumbViewer` stays null; covers the early return at
    `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs:135-138`
- [ ] [P3-T20] Add test `EnsureBreadcrumbPipeline_WithHeadlessViewer_CreatesCoordinatorAndTracksViewer`
      (research C2) to the same file
  - Acceptance: `HeadlessViewerScope` viewer with no coordinator plus
    `Mock<IOlObjects>.FolderTreeService`; asserts `BreadcrumbCoordinator` becomes non-null and
    `_breadcrumbViewer` is set; covers the coordinator-creation path including the
    `OutlookFolderHierarchyProvider` construction at `:142-144`
- [ ] [P3-T21] Add test `EnsureBreadcrumbPipeline_CalledTwiceWithSameViewer_IsIdempotent`
      (research C3) to the same file
  - Acceptance: coordinator is the same instance across both calls and `BreadcrumbUnhandledArrow`
    fires the handler exactly once per raise; pins invariants VS-1 and VS-2
- [ ] [P3-T22] Add test `EnsureBreadcrumbPipeline_WhenViewerChanges_UnsubscribesOldAndSubscribesNew`
      (research C4) to the same file
  - Acceptance: called with viewer A then viewer B; asserts A is unsubscribed, B is subscribed
    exactly once, and `_breadcrumbViewer` is B; covers the subscribe/unsubscribe swap at `:148-157`;
    pins invariant VS-2
- [ ] [P3-T23] Add test `EnsureBreadcrumbPipeline_AfterCleanup_IsSafeNoOp` (research C5) to the same file
  - Acceptance: `Cleanup()` runs first, then `EnsureBreadcrumbPipeline()`; asserts the early return
    is taken and nothing throws; pins invariant VS-4 (dispose-before-setup) and satisfies the
    post-`Cleanup()` clause of AC-3
- [ ] [P3-T24] Remove the **unratified** `[ExcludeFromCodeCoverage]` at
      `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs:132` and rewrite its comment at
      `:127-131` (AC-3, AC-4)
  - Preconditions: `[P3-T19]`..`[P3-T23]` complete. This ordering is mandated by AC-4's clause "no
    per-file coverage measurement taken between tasks shows a file below either floor as a result of
    a de-exemption" — removing the attribute before its covering tests would drive the file to
    roughly 78% line
  - Acceptance: the attribute is gone; the rewritten comment describes the member's actual behaviour
    rather than the false "Skipped for mock viewers" barrier claim (US-2); an immediate per-file
    measurement shows `ViewerSetup.cs` at **>= 80% line and >= 75% branch**; the family attribute
    count falls from 16 to **15**; this removal is asserted on **F10's own authority** and is
    **never** attributed to the #227 ratification, which does not cover this site
- [ ] [P3-T25] Rewrite the stale exemption comment on `InitializeWebViewAsync` at
      `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs:30-37` (US-2)
  - Acceptance: the comment no longer cites the concrete `L0v2h2_WebView2` cast as the barrier; it
    states the operative barrier — the direct `.CoreWebView2` dereference at `:76`, outside the
    injected `IWebViewCoreInitializer` seam, requiring the Edge WebView2 runtime (an external process
    dependency prohibited by `.claude/rules/general-unit-test.md`) — and notes that all host-neutral
    logic has moved to `QfcCidImageResolver`. **The attribute at `:38` is retained** (ratified)
- [ ] [P3-T26] Verify file sizes after the extraction (AC-12)
  - Acceptance: `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs` is <= 500 lines and
    approximately 399; `QuickFiler/Controllers/QfcCidImageResolver.cs` is <= 500 lines; no new test
    file created in this phase exceeds 500 lines
- [ ] [P3-T27] Verify per-file coverage for `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs`
      and for the new `QuickFiler/Controllers/QfcCidImageResolver.cs`
  - Acceptance: `<FEATURE>/evidence/qa-gates/coverage-ViewerSetup-and-CidResolver.<timestamp>.md`
    records, recomputed per `spec.md` §4 and dual-figure: `ViewerSetup.cs` **>= 80% line and >= 75%
    branch** reported independently (projected ~85% / ~91%), and `QfcCidImageResolver.cs`
    **>= 90% line** per the epic's new-file rule (projected 100% / 100%) (AC-11)

### Phase 4 — QfcItemController.Conversation.cs

Baseline 88.24% line (90/102) / 94.44% branch (17/18) — both gates already pass. Work here closes the
entirely-uncovered resolver-taking async overload and the zero-count async render branch. The
ratified attribute at `:79` (`DoLoadConversationResolverCoreAsync`) is **retained**; research CT-4 is
**not scheduled** (binding decision 1, and `file-QfcItemController.Conversation.md` §7 supersedes the
stale `removable-with-seam` classification in the cross-cutting artifact §1.2).

- [ ] [P4-T1] Create `QuickFiler.Test/Controllers/QfcItemController.ConversationAsyncTests.cs` with a
      `[TestClass]` skeleton and add its `<Compile Include>` entry to
      `QuickFiler.Test/QuickFiler.Test.csproj`
  - Acceptance: new file created rather than growing `QfcItemController.ConversationTests.cs`
    (352 lines); CRLF preserved; solution builds
- [ ] [P4-T2] Add test
      `PopulateConversationAsync_WithResolver_StoresResolverAndRendersCount` (research CT-1) to
      `QfcItemController.ConversationAsyncTests.cs`
  - Acceptance: `HarnessController` + `BuildSyncDispatcher()` + `Mock<IItemViewer>` + a resolver
    built through the **current positional** two-argument constructor
    `ConversationResolver(IApplicationGlobals, MailItem)` with `Count = new Pair<int>(7, 7)`;
    asserts `ConversationResolver` is same-as the supplied resolver and
    `ConversationCountText` is set to `"7"` once; covers lines 130-139
- [ ] [P4-T3] Add test
      `PopulateConversationAsync_WithResolver_WhenTokenCancelled_ThrowsBeforeAssigningResolver`
      (research CT-2) to the same file
  - Acceptance: pre-cancelled token; asserts `OperationCanceledException`, that
    `ConversationResolver` remains null (proving the guard at `:131` precedes the assignment at
    `:133`), and that the dispatcher was never invoked; pins invariant I-6
- [ ] [P4-T4] Add test `RenderConversationCountAsync_WhenCountZero_SetsZeroTextAndRedBackColor`
      (research CT-3) to the same file
  - Acceptance: asserts `ConversationCountText` is `"0"` once, `ConversationCountBackColor` is
    `Color.Red` once, and `InvokeAsync` was called with `DispatcherPriority.Normal`; covers lines
    212-214 and the true side of the branch at `:211`
- [ ] [P4-T5] Record the AC-7 disposition of
      `PopulateConversationAsync(ConversationResolver, CancellationToken, bool)`
      (`QuickFiler/Controllers/QfcItemController.Conversation.cs:125-139`)
  - Acceptance: `<FEATURE>/evidence/other/dead-member-dispositions.<timestamp>.md` records the
    decision **retained and covered by behavioural tests** (`[P4-T2]`, `[P4-T3]`) rather than deleted
    or exempted, with the reason: it is `public` on the concrete class, absent from
    `IQfcItemController`, and cheaply coverable with the existing harness. The same artifact records
    the `GetItemSummary()` disposition (retained and covered by `[P3-T6]`). Neither member is given
    `[ExcludeFromCodeCoverage]`
- [ ] [P4-T6] Verify per-file coverage for `QuickFiler/Controllers/QfcItemController.Conversation.cs`
  - Acceptance: `<FEATURE>/evidence/qa-gates/coverage-Conversation.<timestamp>.md` records line
    >= 80% and branch >= 75% independently, recomputed per `spec.md` §4, dual-figure. Expected
    100% line / 100% branch on a denominator of 102

### Phase 5 — QfcItemController.FolderHandling.cs

Baseline 87.76% line (129/147) / **63.33% branch (38/60) — fails the branch gate**. Zero production
change; every dependency is already behind an injectable seam. Twenty of the twenty-two uncovered
branch outcomes live in four repeated `logger.Debug` interpolations. `FH-9` (async FromField success)
is an optional stretch with an unverified `AddConversationBasedSuggestions` tolerance and is **not
scheduled** — both gates clear without it.

- [ ] [P5-T1] Add a shared `internal sealed class FakeFolderSearchHandler` (settable `FolderArray`,
      `Suggestions`, `FolderRowArray`) to
      `QuickFiler.Test/Controllers/QfcItemController.TestSupport.Fixtures.cs`
  - Acceptance: the existing `private sealed` fake inside
    `QuickFiler.Test/Controllers/QfcItemController.FolderSuggestionsTests.cs:30-45` is left untouched
    so no existing test is modified; the solution builds
- [ ] [P5-T2] Create `QuickFiler.Test/Controllers/QfcItemController.FolderHandlingBranchTests.cs`
      with a `[TestClass]` skeleton and add its `<Compile Include>` entry to
      `QuickFiler.Test/QuickFiler.Test.csproj`
  - Acceptance: **new file is mandatory** — `QfcItemController.FolderHandlingTests.cs` is at 498 of
    500 lines and cannot absorb a single `[TestMethod]` (AC-13). CRLF preserved; solution builds
- [ ] [P5-T3] Add test `AssignFolderComboBox_WhenInvokeRequired_MarshalsThroughViewerInvoke`
      (research FH-1) to `QfcItemController.FolderHandlingBranchTests.cs`
  - Acceptance: `Mock<IItemViewer>` with `InvokeRequired == true`; asserts `Invoke` called once and
    `SetFolderItems` never; covers lines 165-167 and takes branch 164 to 2/2; pins invariant I-5
- [ ] [P5-T4] Add test `AssignFolderComboBox_WhenFolderArrayNull_LeavesViewerUntouched`
      (research FH-2) to the same file
  - Acceptance: `FakeFolderSearchHandler { FolderArray = null }` injected into `_folderHandler`;
    asserts no `SetFolderItems`, no `SetFolderSelectedIndex`, no `SetFolderSuggestions`, and
    `SelectedFolder` unchanged; takes branch 170 to 6/6; pins invariant I-6
- [ ] [P5-T5] Add test `LoadFolderHandler_WhenItemHelperNull_LogsWithoutThrowing` (research FH-3)
      to the same file
  - Acceptance: `ItemHelper` left null, factory returns a real `FolderPredictor` with
    `Suggestions = new FolderScorer()`; asserts no throw and `_folderHandler` is the returned
    instance; closes three conditions at line 36; pins invariant I-10
- [ ] [P5-T6] Add test `LoadFolderHandler_WhenFactoryReturnsNull_LogsWithoutThrowing`
      (research FH-4) to the same file
  - Acceptance: factory returns `null`; asserts no throw and `_folderHandler` is null; closes the
    null side of `_folderHandler?` at line 36
- [ ] [P5-T7] Add test `LoadFolderHandler_WithVarList_WhenItemHelperNull_LogsWithoutThrowing`
      (research FH-5) to the same file
  - Acceptance: as `[P5-T5]` but invoked with a non-null `varList`; closes three conditions at line 49
- [ ] [P5-T8] Add test `LoadFolderHandler_WithVarList_WhenFactoryReturnsNull_LogsWithoutThrowing`
      (research FH-6) to the same file
  - Acceptance: as `[P5-T6]` but with a non-null `varList`; closes the remaining condition at line 49
- [ ] [P5-T9] Add test
      `LoadFolderHandlerAsync_WithVarList_WhenItemHelperNull_CompletesAndAssignsHandler`
      (research FH-7) to the same file
  - Acceptance: `ItemHelper` left null, `varList = new[] { @"\\A\one" }`, factory returns a real
    `FolderPredictor` so `InitAsync` → `FromArrayOrString` completes with no COM; awaits the returned
    `Task`; asserts `_folderHandler` is the instance; closes three conditions at line 125. No
    `Thread.Sleep`, no `Task.Delay`, no polling
- [ ] [P5-T10] Add test
      `LoadFolderHandlerAsync_WhenFallbackFactoryAlsoThrows_SurfacesFallbackException`
      (research FH-8) to the same file
  - Acceptance: primary factory throws `ArgumentNullException`, `_folderPredictorEmptyFactory` throws
    `InvalidOperationException`; asserts `ThrowsAsync<InvalidOperationException>()`; covers lines
    95-98; pins invariant I-4
- [ ] [P5-T11] Resolve the filesystem-reading, source-text-asserting test at
      `QuickFiler.Test/Controllers/QfcItemController.FolderHandlingTests.cs:120-148` (AC-15)
  - Acceptance: `ReadControllerSource` (which calls `File.ReadAllText` on a path derived from
    `AppDomain.CurrentDomain.BaseDirectory`) and the
    `LoadFolderHandler_ProbabilityDebugLog_IncludesCallerSubjectEntryIdAndTopScore` test that asserts
    on production **source text** are removed, and the diagnostic intent is preserved by the
    behavioural coverage added in `[P5-T5]`..`[P5-T8]`. The file no longer contains any filesystem
    read. If removal is declined, an explicit reasoned deviation is recorded instead in
    `<FEATURE>/evidence/other/test-policy-deviations.<timestamp>.md`. Post-edit line count of
    `QfcItemController.FolderHandlingTests.cs` is recorded
- [ ] [P5-T12] Record the open-issue **#427** structural-drift note for this file
  - Acceptance: `<FEATURE>/evidence/other/cross-issue-notes.<timestamp>.md` records that a #427
    ("quickfiler-post-show-duplicate-scoring") fix would most likely add a "prediction already
    computed" short-circuit at the top of `LoadFolderHandlerAsync`
    (`QuickFiler/Controllers/QfcItemController.FolderHandling.cs:57-131`), changing this file's line
    and branch sets. F10 **does not pre-empt or partially implement** #427 and does not re-file it
- [ ] [P5-T13] Verify per-file coverage for `QuickFiler/Controllers/QfcItemController.FolderHandling.cs`
  - Acceptance: `<FEATURE>/evidence/qa-gates/coverage-FolderHandling.<timestamp>.md` records line
    >= 80% and branch >= 75% independently, recomputed per `spec.md` §4, dual-figure. Expected
    ~92.5% line / ~85% branch

### Phase 6 — QfcItemController.EventWiring.cs

Baseline 81.52% line (247/303) / **65.63% branch (21/32) — fails the branch gate**. One required
production seam (S1, the delay delegate) unblocks 11 lines and the file's only 0/2 branch. The four
`if (_expanded)` guards plus the WebView poll tests clear both gates; the 27 registered-lambda tests
are what actually assert user-visible keyboard behaviour and are scheduled in full. The single
attribute at `:99` (`WebView2Control_CoreWebView2InitializationCompleted`) is **retained** — it is a
ratified bucket-3 `async void` shell whose substantive body is already at 100% through
`HandleWebViewInitializedAsync`.

- [ ] [P6-T1] Add the S1 delay seam: declare
      `private Func<int, Task> _delayAsync = milliseconds => Task.Delay(milliseconds);` in the
      private-field region of `QuickFiler/Controllers/QfcItemController.cs`, and replace
      `await Task.Delay(newDelay);` at `QuickFiler/Controllers/QfcItemController.EventWiring.cs:135`
      with `await _delayAsync(newDelay);`
  - Acceptance: one field added, one line replaced 1:1, no behaviour change (the field initialiser
    reproduces the current expression exactly); a field initialiser is used rather than a
    `SaveParameters` `??=` default so no path can leave the seam null;
    `QuickFiler/Controllers/QfcItemController.cs` stays under 500 lines; the solution builds. The
    justification is recorded: reaching the timeout as written costs 14 iterations totalling
    **10,500 ms of real wall-clock time**, which `.claude/rules/general-unit-test.md` prohibits, and
    net481 has no `FakeTimeProvider`
- [ ] [P6-T2] Create `QuickFiler.Test/Controllers/QfcItemController.EventWiringBranchTests.cs` with a
      `[TestClass]` skeleton and add its `<Compile Include>` entry to
      `QuickFiler.Test/QuickFiler.Test.csproj`
  - Acceptance: new file created rather than growing `QfcItemController.EventWiringTests.cs`
    (374 lines); CRLF preserved; solution builds
- [ ] [P6-T3] Add test `RegisterFocusActions_WhenExpanded_AlsoRegistersBAndDCharActions` (EW-1) to
      `QfcItemController.EventWiringBranchTests.cs`
  - Acceptance: `_expanded = true` via `SetField`; asserts `'B'` and `'D'` are present in the real
    `KbdActions<>` registry; covers lines 209-211 and takes branch 208 to 2/2; pins invariant I-7
- [ ] [P6-T4] Add test `RegisterFocusAsyncActions_WhenExpanded_AlsoRegistersBAndDAsyncCharActions`
      (EW-2) to the same file
  - Acceptance: covers lines 301-303 and takes branch 300 to 2/2; pins invariant I-9
- [ ] [P6-T5] Add test `UnregisterFocusActions_WhenExpanded_AlsoRemovesBAndD` (EW-3) to the same file
  - Acceptance: covers lines 350-352 and takes branch 349 to 2/2; pins invariant I-8
- [ ] [P6-T6] Add test `UnregisterFocusAsyncActions_WhenExpanded_AlsoRemovesAsyncBAndD` (EW-4) to
      the same file
  - Acceptance: covers lines 374-376 and takes branch 373 to 2/2; pins invariant I-9
- [ ] [P6-T7] Add test `HandleWebViewInitializedAsync_WhenItemHelperArrivesOnSecondPoll_NavigatesOnce`
      (EW-5) to the same file
  - Acceptance: `_delayAsync` stubbed to `_ => Task.CompletedTask`; `ItemHelper` null at entry then
    supplied; covers lines 121-126 and 135-137 and takes branch 124 to 2/2; **no real wait occurs**
- [ ] [P6-T8] Add test `HandleWebViewInitializedAsync_WhenItemHelperNeverArrives_TimesOutAndLogs`
      (EW-6) to the same file
  - Acceptance: `_delayAsync` stubbed; `ItemHelper` never supplied; asserts the `TimeoutException` is
    raised, swallowed by the enclosing catch, and logged; covers lines 128-133 and takes branch 128
    to 2/2 (the file's only 0/2 branch)
- [ ] [P6-T9] Add test `HandleWebViewInitializedAsync_InvokesDelaySeamWithGrowingBackoff` (EW-7) to
      the same file
  - Acceptance: a recording `_delayAsync` stub captures the requested delays; asserts the
    `100 * n` growth sequence is preserved by the seam, proving the seam is behaviour-preserving
- [ ] [P6-T10] Add test
      `HandleWebViewInitializedAsync_WhenNotSuccessfulAndExceptionNull_CharacterisesThrowNull`
      (EW-8) to the same file
  - Acceptance: `isSuccess == false` with a null initialization exception; **characterisation** of
    the current `throw (initException)` behaviour at
    `QuickFiler/Controllers/QfcItemController.EventWiring.cs:117` (a `NullReferenceException` logged
    under a misleading message). Behaviour is not changed; the doc comment names the promoted issue
    for this defect so a future fix tightens rather than inverts the assertion
- [ ] [P6-T11] Add test `HandleWebViewInitializedAsync_SetsInitializedFlagBeforeWaitAndKeepsItOnTimeout`
      (EW-9) to the same file
  - Acceptance: asserts `_isWebViewerInitialized` is set at `:119` before the `ItemHelper` wait and
    remains set after a timeout; pins invariant I-14
- [ ] [P6-T12] Create `QuickFiler.Test/Controllers/QfcItemController.EventWiringLifecycleTests.cs`
      with a `[TestClass]` skeleton and add its `<Compile Include>` entry to
      `QuickFiler.Test/QuickFiler.Test.csproj`
  - Acceptance: CRLF preserved; solution builds
- [ ] [P6-T13] Add table-driven test `WireIntentEvents_EachViewerEvent_ReachesItsExpectedHandler`
      (EW-10) to `QfcItemController.EventWiringLifecycleTests.cs`
  - Acceptance: a `Mock<IItemViewer>` raises each of the 16 intent events wired at
    `QuickFiler/Controllers/QfcItemController.EventWiring.cs:68-93` and each reaches its documented
    handler; guards the wiring map; pins invariant I-3
- [ ] [P6-T14] Add test
      `WireControlTreeEvents_WhenMenuItemsContainsNonToolStripMenuItem_ThrowsAfterKeyAndButtonWiring`
      (EW-11) to the same file
  - Acceptance: asserts `InvalidCastException` from the unguarded downcast at `:59` and that key and
    button handlers were already attached — a partial-wiring characterisation with no rollback.
    Behaviour is not changed
- [ ] [P6-T15] Add test `WireControlTreeEvents_BeforeResolveControlGroups_ThrowsNullReference`
      (EW-12) to the same file
  - Acceptance: `Buttons` null because `ResolveControlGroups` has not run; asserts
    `NullReferenceException` at `:53`; pins the one-way ordering precondition I-2
- [ ] [P6-T16] Add test `WireIntentEvents_AttachesExactlySixteenSubscriptions` (EW-13) to the same file
  - Acceptance: raising every `IItemViewer` intent event on the mock and counting handler entries
    yields exactly 16; pins invariant I-3. Note the count is **16**, correcting the brief's "14"
- [ ] [P6-T17] Add test `WireEvents_CalledTwice_DoubleDispatchesEachIntentEvent` (EW-14) to the same file
  - Acceptance: **characterisation** of the missing idempotence guard (invariant I-4); the doc
    comment states that a future idempotence fix must update this test
- [ ] [P6-T18] Add test `RegisterFocusActions_CalledTwice_ThrowsArgumentExceptionNamingDuplicateKey`
      (EW-15) to the same file
  - Acceptance: asserts `ArgumentException` from `KbdActions.Add` naming the duplicate key and
    `SourceId`; pins invariant I-5. The doc comment records the **cross-child conditional**: if F3's
    fix for open issue **#444** makes `Add` idempotent, this test changes from "throws" to "no-op"
- [ ] [P6-T19] Add test `RegisterFocusAsyncActions_CalledTwice_ThrowsArgumentException` (EW-16) to
      the same file
  - Acceptance: as `[P6-T18]` for the async registry; pins invariant I-6; carries the same #444
    conditional note
- [ ] [P6-T20] Add test `RegisterThenUnregisterFocusActions_LeavesRegistryEmptyForThatSourceId`
      (EW-17) to the same file
  - Acceptance: round-trips **all** registered keys, not the current 4-key sample; asserts zero
    entries remain for that `SourceId`; pins invariant I-10 (register/unregister sets are exact
    inverses)
- [ ] [P6-T21] Add test `UnregisterFocusActions_WhenNothingRegistered_IsSilentNoOp` (EW-18) to the
      same file
  - Acceptance: asserts no throw and no observable change; pins invariant I-11 (the discarded `bool`
    from `KbdActions.Remove`)
- [ ] [P6-T22] Add test `RegisterFocusActions_ForTwoControllersWithDistinctEntryIds_DoNotCollide`
      (EW-19) to the same file
  - Acceptance: two controllers with different `ItemHelper.EntryId` register into one shared
    `KbdActions<>` without collision; pins invariant I-12
- [ ] [P6-T23] Add test `UnregisterFocusActions_WhenEntryIdChangedAfterRegistration_OrphansOriginalEntries`
      (EW-20) to the same file
  - Acceptance: **characterisation** of invariant I-13 — the original registrations survive because
    `Remove` matches on `SourceId`; behaviour is not changed
- [ ] [P6-T24] Add test `UnregisterFocusActions_AfterCleanup_ThrowsNullReference` (EW-21) to the
      same file
  - Acceptance: `Cleanup()` nulls `ItemHelper` at `ViewerSetup.cs:418`, so the first
    `ItemHelper.EntryId` read throws; **characterisation** of open issue **#481**; the doc comment
    names #481 (dispose-before-setup, invariant I-16)
- [ ] [P6-T25] Add test `WireIntentEvents_AfterCleanup_LeavesAllHandlersAttached` (EW-22) to the
      same file
  - Acceptance: after `Cleanup()` the mock viewer still holds all 16 handlers and raising
    `ConversationModeChanged` reaches a controller with a null `_itemViewer`; **characterisation** of
    open issue **#481** (no unwiring path); behaviour is not changed
- [ ] [P6-T26] Create `QuickFiler.Test/Controllers/QfcItemController.EventWiringActionsTests.cs`
      with a `[TestClass]` skeleton and add its `<Compile Include>` entry to
      `QuickFiler.Test/QuickFiler.Test.csproj`
  - Acceptance: CRLF preserved; solution builds. Each test in this file registers the actions, then
    retrieves the stored delegate through the `KbdActions<>` indexer and **invokes** it
- [ ] [P6-T27] Add test for the registered sync lambda at `EventWiring.cs:162`
      (`ToggleConversationCheckbox`) to `QfcItemController.EventWiringActionsTests.cs` (EW-23)
  - Acceptance: `BuildSyncDispatcher()` + `Mock<IItemViewer>`; invoking the retrieved delegate
    toggles the conversation checkbox exactly once; covers line 162
- [ ] [P6-T28] Add test for the registered sync lambda at `:167` (`ToggleConversationCheckbox`) (EW-24)
  - Acceptance: same fixture; covers line 167
- [ ] [P6-T29] Add test for the registered sync lambda at `:172`
      (`_explorerController.OpenQFItem(Mail)`) (EW-25)
  - Acceptance: `Mock<IQfcExplorerController>`; asserts `OpenQFItem` called once with `Mail`;
    covers line 172
- [ ] [P6-T30] Add test for the registered sync lambda at `:177` (`ToggleConversationCheckbox`) (EW-26)
  - Acceptance: same fixture as `[P6-T27]`; covers line 177
- [ ] [P6-T31] Add test for the registered sync lambda at `:182` (`ToggleSaveAttachments`) (EW-27)
  - Acceptance: `Mock<IItemViewer>`; **characterisation** — `ToggleSaveAttachments`'s body is
    entirely commented out, so the `'A'` action is a no-op; the doc comment names the promoted issue
    for this defect. Covers line 182; behaviour is not changed
- [ ] [P6-T32] Add test for the registered sync lambda at `:187` (`ToggleSaveCopyOfMail`) (EW-28)
  - Acceptance: `Mock<IItemViewer>`; asserts the email-copy toggle occurs once; covers line 187
- [ ] [P6-T33] Add test for the registered sync lambda at `:189` (`ToggleExpansion()`) (EW-29)
  - Acceptance: uses the existing virtual-override spy pattern from
    `QuickFiler.Test/Controllers/QfcItemController.NavigationTests.cs:139-157` so no real expansion
    state is mutated; covers line 189
- [ ] [P6-T34] Add test for the registered sync lambda at `:190` (`JumpToSearchTextbox()`) (EW-30)
  - Acceptance: `Mock<IQfcKeyboardHandler>` + `Mock<IItemViewer>`; asserts the keyboard dialog toggle
    and search focus; covers line 190
- [ ] [P6-T35] Add test for the registered sync lambda at `:191` (`FlagAsTask()`) (EW-31)
  - Acceptance: uses the `_flagTasksFactory` sentinel-throw pattern already established at
    `QuickFiler.Test/Controllers/QfcItemController.EventHandlersTests.cs:264-304` so **no modal
    dialog and no `FlagTasks.Run` is reached**; asserts the factory was invoked; covers line 191
- [ ] [P6-T36] Add test for the registered sync lambda at `:195`
      (`_parent.PopOutControlGroup(ItemNumber)`) (EW-32)
  - Acceptance: `Mock<IQfcCollectionController>`; asserts the call once with this controller's
    `ItemNumber`; covers line 195
- [ ] [P6-T37] Add test for the registered sync lambda at `:200`
      (`_parent.RemoveSpecificControlGroup(ItemNumber)`) (EW-33)
  - Acceptance: `Mock<IQfcCollectionController>`; covers line 200
- [ ] [P6-T38] Add test for the registered sync lambda at `:202` (`MarkItemForDeletion()`) (EW-34)
  - Acceptance: `Mock<IItemViewer>`; covers line 202
- [ ] [P6-T39] Add test for the registered sync lambda at `:206` (`JumpToFolderDropDown()`) (EW-35)
  - Acceptance: `Mock<IQfcKeyboardHandler>` + `Mock<IItemViewer>`; covers line 206
- [ ] [P6-T40] Create `QuickFiler.Test/Controllers/QfcItemController.EventWiringAsyncActionsTests.cs`
      with a `[TestClass]` skeleton and add its `<Compile Include>` entry to
      `QuickFiler.Test/QuickFiler.Test.csproj`
  - Acceptance: CRLF preserved; solution builds. Every test awaits the retrieved delegate; no test
    polls or waits on wall-clock time
- [ ] [P6-T41] Add test for the registered async lambda at `:224` (`ToggleExpansionAsync()`) (EW-36)
      to `QfcItemController.EventWiringAsyncActionsTests.cs`
  - Acceptance: virtual-override spy; awaited; covers line 224
- [ ] [P6-T42] Add test for the registered async lambda at `:230-233`
      (`ToggleConversationCheckbox(); return Task.CompletedTask;`) (EW-37)
  - Acceptance: `BuildSyncDispatcher()` + `Mock<IItemViewer>`; covers lines 230-233
- [ ] [P6-T43] Add test for the registered async lambda at `:238`
      (`_explorerController.OpenQFItem(Mail)`) (EW-38)
  - Acceptance: `Mock<IQfcExplorerController>`; covers line 238
- [ ] [P6-T44] Add test for the registered async lambda at `:243` (`KbdExecuteAsync`) (EW-39)
  - Acceptance: `Mock<IFilerHomeController>` exposing `KeyboardHandler` (pattern at
    `QfcItemController.NavigationTests.cs:35-41`); covers line 243
- [ ] [P6-T45] Add test for the registered async lambda at `:248` (`KbdExecuteAsync`) (EW-40)
  - Acceptance: same fixture; covers line 248
- [ ] [P6-T46] Add test for the registered async lambda at `:253` (`KbdExecuteAsync`) (EW-41)
  - Acceptance: same fixture; covers line 253
- [ ] [P6-T47] Add test for the registered async lambda at `:258` (`KbdExecuteAsync`) (EW-42)
  - Acceptance: same fixture; covers line 258
- [ ] [P6-T48] Add test for the registered async lambda at `:263` (`ToggleExpansionAsync()`) (EW-43)
  - Acceptance: virtual-override spy; covers line 263
- [ ] [P6-T49] Add test for the registered async lambda at `:269-272` (`JumpToSearchTextbox()`) (EW-44)
  - Acceptance: `Mock<IQfcKeyboardHandler>` + `Mock<IItemViewer>`; covers lines 269-272
- [ ] [P6-T50] Add test for the registered async lambda at `:277` (`KbdExecuteAsync`) (EW-45)
  - Acceptance: same fixture as `[P6-T44]`; covers line 277
- [ ] [P6-T51] Add test for the registered async lambda at `:282`
      (`_parent.PopOutControlGroupAsync(ItemNumber)`) (EW-46)
  - Acceptance: `Mock<IQfcCollectionController>` returning `Task.CompletedTask`; covers line 282
- [ ] [P6-T52] Add test for the registered async lambda at `:288`
      (`_parent.RemoveSpecificControlGroupAsync(ItemNumber)`) (EW-47)
  - Acceptance: `Mock<IQfcCollectionController>`; covers line 288
- [ ] [P6-T53] Add test for the registered async lambda at `:293` (`MarkItemForDeletionAsync()`) (EW-48)
  - Acceptance: `Mock<IItemViewer>` + `BuildSyncDispatcher()`; covers line 293
- [ ] [P6-T54] Add test for the registered async lambda at `:298` (`JumpToFolderDropDownAsync()`) (EW-49)
  - Acceptance: `Mock<IQfcKeyboardHandler>` + `BuildSyncDispatcher()`; covers line 298
- [ ] [P6-T55] Create
      `QuickFiler.Test/Controllers/QfcItemController.EventWiringExpandedActionsTests.cs` with a
      plain `[TestClass]` skeleton and add its `<Compile Include>` entry to
      `QuickFiler.Test/QuickFiler.Test.csproj`
  - Acceptance: the file is a **plain `[TestClass]`, not a `*.StaTests.cs` file** (US-11). It uses
    `HeadlessViewerScope` from `[P3-T1]`, matching the existing convention at
    `QfcItemController.EventWiringTests.cs:229`/`:320`. CRLF preserved; solution builds
- [ ] [P6-T56] Add test for the expanded sync lambda at `:311`
      (`JumpToAsync(((ItemViewer)_itemViewer).L0v2h2_WebView2)`) (EW-50)
  - Acceptance: `HeadlessViewerScope`; asserts the handle-less control focus attempt and the keyboard
    dialog toggle; covers line 311. No real WebView2 core is started
- [ ] [P6-T57] Add test for the expanded sync lambda at `:316`
      (`JumpToAsync(((ItemViewer)_itemViewer).TopicThread)`) (EW-51)
  - Acceptance: `HeadlessViewerScope`; covers line 316
- [ ] [P6-T58] Add test for the expanded async lambda at `:325` (EW-52)
  - Acceptance: `HeadlessViewerScope`; awaited; covers line 325
- [ ] [P6-T59] Add test for the expanded async lambda at `:330` (EW-53)
  - Acceptance: `HeadlessViewerScope`; awaited; covers line 330
- [ ] [P6-T60] Verify per-file coverage for `QuickFiler/Controllers/QfcItemController.EventWiring.cs`
      and confirm no test file added in this phase exceeds 500 lines
  - Acceptance: `<FEATURE>/evidence/qa-gates/coverage-EventWiring.<timestamp>.md` records line
    >= 80% and branch >= 75% independently, recomputed per `spec.md` §4, dual-figure (projected
    ~100% line / 100% branch); `QuickFiler/Controllers/QfcItemController.EventWiring.cs` remains at
    391 lines (the seam is a 1:1 replacement); `QuickFiler/Controllers/QfcItemController.cs` is under
    500 lines; the attribute at `:99` is still present (retained)

### Phase 7 — QfcItemController.EventHandlers.cs

Baseline 79.57% line (74/93) / **65.00% branch (26/40) — fails both gates**. Two tests (EH-1, EH-2)
clear both. The five ratified `async void` shell attributes at `:60`, `:83`, `:97`, `:111`, `:125` are
**retained**; the per-file research governs over the cross-cutting artifact's `removable-as-is`
classification, and the re-verified rationale is "`async void` cannot be awaited deterministically",
not "the routing is untestable" (the routing is already proven).

- [ ] [P7-T1] Add the optional `SynchronizationContext` factory seam: declare
      `private Func<SynchronizationContext> _uiSyncContextFactory = () => new WindowsFormsSynchronizationContext();`
      in `QuickFiler/Controllers/QfcItemController.cs`, add a single
      `private void EnsureUiSynchronizationContext()` helper to
      `QuickFiler/Controllers/QfcItemController.EventHandlers.cs`, and replace all **eight**
      duplicated inline guard blocks (`:29-32`, `:51-54`, `:63-66`, `:74-77`, `:86-89`, `:100-103`,
      `:114-117`, `:128-131`) with a call to it
  - Acceptance: **explicit justification recorded** as required by AC-10 — without this seam, lines
    30-32, 52-54 and 75-77 and three half-covered branch points are permanently uncovered, because
    forcing the true arm as written installs a real `WindowsFormsSynchronizationContext` on the
    MSTest thread for the remainder of the process, breaking test independence
    (`.claude/rules/general-unit-test.md` § Core Principles 1). The default delegate reproduces the
    current expression exactly, so there is **no behaviour change**; 24 lines of verbatim
    duplication are removed; `QfcItemController.EventHandlers.cs` shrinks from 219 to roughly 202
    lines; `QfcItemController.cs` stays under 500; the solution builds
- [ ] [P7-T2] Convert `QfcItemControllerTestSupport.EnsureSynchronizationContext()`
      (`QuickFiler.Test/Controllers/QfcItemController.TestSupport.cs:87-93`) into a disposable
      save/restore scope and add the complementary `NullSynchronizationContextScope` usage (AC-15)
  - Acceptance: the helper no longer mutates ambient thread state without restoring it; the
    save/restore shape matches `QfcItemControllerBreadcrumbDropDownTests.cs:365-383`; all existing
    callers still pass; `QfcItemController.TestSupport.cs` stays under 500 lines
- [ ] [P7-T3] Create `QuickFiler.Test/Controllers/QfcItemController.EventHandlersConversationTests.cs`
      with a `[TestClass]` skeleton and add its `<Compile Include>` entry to
      `QuickFiler.Test/QuickFiler.Test.csproj`
  - Acceptance: new file created because `QfcItemController.EventHandlersTests.cs` is at 438 lines
    and cannot absorb the new tests; CRLF preserved; solution builds
- [ ] [P7-T4] Add test
      `CbxConversation_CheckedChanged_WhenNotSuppressedAndChecked_CollapsesConversation` (EH-1) to
      `QfcItemController.EventHandlersConversationTests.cs`
  - Acceptance: `SuppressEvents = false`, `ConversationModeChecked = true`; asserts
    `_parent.ToggleGroupConv(entryId)` called once and `EnumerateConversation` not called; covers
    lines 37-41 and takes branches 36 and 38 true; ambient `SynchronizationContext` restored in a
    `finally`; pins invariant I-5
- [ ] [P7-T5] Add test
      `CbxConversation_CheckedChanged_WhenNotSuppressedAndUnchecked_EnumeratesConversation` (EH-2)
      to the same file
  - Acceptance: `SuppressEvents = false`, `ConversationModeChecked = false`; a real
    `ConversationResolver` built through the **current positional** two-argument constructor
    `(IApplicationGlobals, MailItem)` with `Count` seeded; asserts `_parent.ToggleUnGroupConv(...)`
    called once; covers lines 43-46 and takes branch 38 false. `ConversationResolver.cs` is **not
    edited**; the direct `new` pins the F4-owned shape so a signature change surfaces as a build break
- [ ] [P7-T6] Add test `CbxConversation_CheckedChanged_WhenSuppressed_DoesNotRecurse` (EH-3) to the
      same file
  - Acceptance: with `SuppressEvents = true`, a nested raise from inside `ToggleGroupConv` does not
    recurse into `CollapseConversation`; pins invariant I-6 (re-entrancy)
- [ ] [P7-T7] Add test `CbxConversation_CheckedChanged_AfterCleanup_ThrowsNullReference` (EH-10) to
      the same file
  - Acceptance: `_itemViewer` nulled by `Cleanup()`; asserts `NullReferenceException`;
    **characterisation** of open issue **#481**; the doc comment names #481; behaviour is not changed
- [ ] [P7-T8] Create `QuickFiler.Test/Controllers/QfcItemController.EventHandlersGuardTests.cs` with
      a `[TestClass]` skeleton and add its `<Compile Include>` entry to
      `QuickFiler.Test/QuickFiler.Test.csproj`
  - Acceptance: CRLF preserved; solution builds
- [ ] [P7-T9] Add test `TopicThread_ItemSelectionChanged_WhenSelectionNull_DoesNotNavigate` (EH-4)
      to `QfcItemController.EventHandlersGuardTests.cs`
  - Acceptance: `GetSelectedConversationItems()` returns `null`; asserts no `NavigateToString` and no
    `NullReferenceException`, proving the short-circuit order at `:197`; closes both remaining
    conditions there; pins invariant I-7
- [ ] [P7-T10] Add test `TextBoxSearch_TextChanged_WhenFewerThanTwoFolders_DoesNotSetSelectedIndex`
      (EH-5) to the same file
  - Acceptance: the handler returns 0 or 1 folders; asserts `SetFolderSelectedIndex` is never called.
    **`TextBoxSearch_TextChanged` behaviour is unchanged and this test adds no new assertion pinning
    `SetFolderDroppedDown(true)`**, so open issue **#438** stays fixable (AC-16, US-6)
- [ ] [P7-T11] Add test
      `EnsureUiSynchronizationContext_WhenAmbientContextNull_InstallsFactoryResult` (EH-6) to the
      same file
  - Acceptance: `_uiSyncContextFactory` injected as `() => new SynchronizationContext()` — a plain,
    inert context, **never** a `WindowsFormsSynchronizationContext`; the ambient context is captured
    and restored in a `finally`; covers the guard's true arm
- [ ] [P7-T12] Add test
      `EnsureUiSynchronizationContext_WhenAmbientContextPresent_DoesNotInvokeFactory` (EH-7) to the
      same file
  - Acceptance: asserts the factory is never invoked and the ambient context is unchanged; covers the
    guard's false arm; context restored in a `finally`
- [ ] [P7-T13] Add test `MouseEnterHandlers_WhenSenderIsWrongControlType_ThrowInvalidCast` (EH-8) to
      the same file
  - Acceptance: a handle-less `Label` passed as `sender` to `Button_MouseEnter` /
    `MenuItem_MouseEnter`; asserts `InvalidCastException`; **characterisation** of the unconditional
    cast; no live form is constructed
- [ ] [P7-T14] Add test `Button_MouseLeave_AfterCleanup_ThrowsNullReference` (EH-9) to the same file
  - Acceptance: `_themes` nulled by `Cleanup()`; asserts `NullReferenceException`;
    **characterisation** of open issue **#481**; the doc comment names #481
- [ ] [P7-T15] Add test `BtnFlagTask_Click_WhenFlagTasksFactoryReturnsNormally_CompletesHandler`
      (EH-11) to the same file
  - Acceptance: a non-throwing `_flagTasksFactory` stub replaces the existing sentinel-throw stub for
    this test only; **no modal dialog is shown and `FlagTasks.Run` is not reached** (the runner seam
    added in `[P10-T2]` is not required here because the factory is stubbed); covers line 56
- [ ] [P7-T16] Verify the line count of `QuickFiler/Controllers/QfcItemController.EventHandlers.cs`
      and of the two new test files
  - Acceptance: the production file is <= 500 lines and approximately 202; neither new test file
    exceeds 500 lines (AC-12, AC-13)
- [ ] [P7-T17] Verify per-file coverage for `QuickFiler/Controllers/QfcItemController.EventHandlers.cs`
  - Acceptance: `<FEATURE>/evidence/qa-gates/coverage-EventHandlers.<timestamp>.md` records line
    >= 80% and branch >= 75% independently, recomputed per `spec.md` §4, dual-figure. Expected
    ~100% line / ~100% branch; the five ratified attributes are confirmed still present **at their
    post-refactor positions** (baseline `:60`, `:83`, `:97`, `:111`, `:125`; `[P7-T1]` removes 24 net
    lines above them, so expect roughly `:38`, `:58`, `:69`, `:80`, `:91`). The criterion is presence
    and count, not line number

### Phase 8 — QfcItemController.Navigation.cs

Baseline 89.07% line / 76.67% branch (23/30) — passes branch by **one condition**, so the file is
treated as at risk, not safe. The two attributes at `:173` and `:191` are **RETAINED** per `spec.md`
§3.5 and AC-21; only their stale comments are corrected. Research NV-1, NV-2, NV-4 and NV-5 were
de-exemption tasks and are therefore **not scheduled** (binding decision 1); the remaining ten tests
take branch to 30/30 and line to roughly 98%.

- [ ] [P8-T1] Add the read-timer factory seam: retype `_emailIsReadTimer` to `IDisposable` at
      `QuickFiler/Controllers/QfcItemController.cs:53`, declare
      `private Func<TimerCallback, int, IDisposable> _readTimerFactory` with a field-initialiser
      default that constructs the `System.Threading.Timer` and calls `Change(dueTimeMs, Timeout.Infinite)`,
      introduce a named `MailReadDelayMilliseconds` constant for the literal `4000`, and replace
      `QuickFiler/Controllers/QfcItemController.Navigation.cs:223-224` with a single call through the
      factory
  - Acceptance: **explicit justification recorded** as required by AC-10 — covering lines 222-225 as
    written arms a live 4,000 ms thread-pool timer that outlives the test method and fires
    `ApplyReadEmailFormat` against Moq stubs during an unrelated later test, violating the
    independence and determinism rules. The default reproduces the current expression exactly, so
    there is **no behaviour change**. `System.Threading.Timer` implements `IDisposable`, so the
    `Dispose()` at `Navigation.cs:213` is unaffected by the retype. The blast radius is verified
    contained: `_emailIsReadTimer` has one declaration plus exactly five reference sites, all
    F10-owned — declaration `QfcItemController.cs:53`; references `Navigation.cs:211, 213, 223, 224`
    and `ViewerSetup.cs:420`.
    `QfcItemController.cs` stays under 500 lines; the solution builds
- [ ] [P8-T2] Create `QuickFiler.Test/Controllers/QfcItemController.NavigationExpansionTests.cs`
      with a `[TestClass]` skeleton and add its `<Compile Include>` entry to
      `QuickFiler.Test/QuickFiler.Test.csproj`
  - Acceptance: new file created because `QfcItemController.NavigationTests.cs` is at 391 lines; the
    existing `ExpansionSpyController` at `QfcItemController.NavigationTests.cs:139-157` is left
    untouched and new direct tests target `HarnessController`; CRLF preserved; solution builds
- [ ] [P8-T3] Add test `ToggleConversationCheckbox_WithUnrecognisedToggleState_FallsToDefaultAndInverts`
      (NV-3) to `QfcItemController.NavigationExpansionTests.cs`
  - Acceptance: passes a composite value such as `Off | Force`; asserts the checkbox is **inverted**;
    covers lines 141-142 and closes the `default:` arm at branch 130. **Characterisation** of the
    non-flag-aware switch; the doc comment records that no current caller passes a composite value to
    this overload
- [ ] [P8-T4] Add test `ToggleExpansionOn_WhenItemHelperUnread_ArmsReadTimerThroughFactory` (NV-6)
      to the same file
  - Acceptance: `ItemHelper` non-null with `UnRead == true`; a recording `_readTimerFactory` stub
    captures the callback and due time; asserts the callback is `ApplyReadEmailFormat` and the due
    time is `MailReadDelayMilliseconds` (4000); covers lines 222-225 and closes **both** sub-conditions
    of the `&&` at line 221. **No real timer is created and no wall-clock wait occurs**
- [ ] [P8-T5] Add test `ToggleExpansionOff_WhenTimerArmed_DisposesItAndClearsExpandedFirst` (NV-7)
      to the same file
  - Acceptance: a non-null `IDisposable` injected into `_emailIsReadTimer`; asserts it is disposed
    exactly once and that `_expanded` is cleared before disposal; covers lines 212-214 and takes
    branch 211 to 2/2; pins invariant I-12
- [ ] [P8-T6] Add test `ToggleExpansionOn_CalledTwiceWithoutIntermediateOff_ThrowsAndLeaksTimer`
      (NV-8) to the same file
  - Acceptance: asserts the second call throws `ArgumentException` naming key `'B'` from
    `KbdActions.Add`, and that the first timer instance is not disposed; **characterisation** of open
    issue **#484**; the doc comment names #484; pins invariant I-14
- [ ] [P8-T7] Add test `JumpToFolderDropDown_BothOverloads_ResetCounterEnterToZero` (NV-9) to the
      same file
  - Acceptance: asserts `CounterEnter` is reset to 0 by both the sync and async overloads inside the
    marshalled delegate; pins invariant I-3
- [ ] [P8-T8] Add test `SyncExpandThenAsyncCollapseThenSyncExpand_ThrowsOnDuplicateKey` (NV-10) to
      the same file
  - Acceptance: reproduces the cross-variant sequence — the async collapse removes nothing because
    the registries are disjoint, and the second sync expand throws `ArgumentException`;
    **characterisation** of open issue **#482**; the doc comment names #482 and records that the fix
    most likely belongs at `QuickFiler/Controllers/QfcCollectionController.cs:1439` (F11-owned), not
    in an F10 file. Behaviour is not changed; pins invariant I-15
- [ ] [P8-T9] Add test `ToggleExpansionOff_AfterCleanup_ThrowsAndLeavesArmedTimerUndisposed` (NV-11)
      to the same file
  - Acceptance: asserts `NullReferenceException` on `_tlpStates`/`_itemViewer` after `Cleanup()`, and
    separately that `Cleanup()` nulls `_emailIsReadTimer` without disposing it; **characterisation**
    of open issue **#484**; pins invariant I-16 (dispose-before-setup)
- [ ] [P8-T10] Add test `Reply_RunsMailActionInsideDispatchAndDisplayOutside` (NV-12) to the same file
  - Acceptance: asserts `_mailActions.Reply()` runs **inside** the dispatched delegate and
    `Display()` runs **outside** it; pins invariant I-17, guarding the deliberate thread-affinity
    choice documented at `Navigation.cs:88-89` against a well-meaning refactor
- [ ] [P8-T11] Add test `ReplyReplyAllForward_WhenDispatcherReturnsNull_ThrowNullReference` (NV-13)
      to the same file
  - Acceptance: **characterisation** of the unchecked dispatcher result at `Navigation.cs:90-91`,
    `:96-97`, `:102-103`; behaviour is not changed; the doc comment names the promoted issue
- [ ] [P8-T12] Add test `MenuDropDown_DispatchesShowMoveOptionsMenuOnce` (NV-14) to the same file
  - Acceptance: `BuildSyncDispatcher()` + `Mock<IItemViewer>`; asserts exactly one dispatch; converts
    a currently only-transitively-covered member into a directly-asserted one
- [ ] [P8-T13] Correct the two stale in-code justification comments at
      `QuickFiler/Controllers/QfcItemController.Navigation.cs:171-172` and `:189-190` (AC-21, US-2, US-3)
  - Acceptance: both comments state the **ratified** rationale — "deliberate `virtual` override
    point; the body is intentionally unexercised because tests override it" — instead of the false
    `TlpCellSnapShot`-bound barrier claim, which the `IContainerControlLocal` retrofit removed
    (`ApplyState(_itemViewer)` at `:209`/`:219` now carries no `(ItemViewer)` cast). **Both
    attributes are retained**; this task removes no attribute
- [ ] [P8-T14] Verify that the `[ExcludeFromCodeCoverage]` attributes at
      `QuickFiler/Controllers/QfcItemController.Navigation.cs:173` and `:191` are **still present**
      after `[P8-T13]` (AC-8, AC-21, US-3 guard)
  - Acceptance: a grep confirms both attributes remain; the family attribute count is still **15**;
    recorded in `<FEATURE>/evidence/other/navigation-attributes-retained.<timestamp>.md`. Removing
    either on the executor's own authority is prohibited — each is ratified under #227 and
    overturning a ratified exemption requires a maintainer decision
- [ ] [P8-T15] Verify per-file coverage for `QuickFiler/Controllers/QfcItemController.Navigation.cs`
      and confirm file sizes
  - Acceptance: `<FEATURE>/evidence/qa-gates/coverage-Navigation.<timestamp>.md` records line
    >= 80% and branch >= 75% independently, recomputed per `spec.md` §4, dual-figure. Expected ~98%
    line / 100% branch (30/30), replacing today's one-condition margin.
    `QuickFiler/Controllers/QfcItemController.Navigation.cs` and the new test file are each <= 500 lines

### Phase 9 — QfcItemController.FocusAndTheme.cs

Baseline 74.26% line (176/237) / **58.82% branch (40/68) — fails both gates**. Zero attributes and
**zero production change**: every uncovered line is reachable through the existing seams. The binding
constraint is that
`QuickFiler.Test/Controllers/QfcItemController.FocusAndThemeTests.cs` is at **497 of 500 lines**, so
all 21 tests go into new files (AC-13). Tier A (`[P9-T5]`..`[P9-T13]`, `[P9-T14]`, `[P9-T15]`) clears
both gates; Tier B takes the file to 100%/100%.

- [ ] [P9-T1] Promote `BuildAllThemes`, `BuildFocusController`, `BuildExecutingViewer` and
      `EnableHandlelessThemeInvoke` from
      `QuickFiler.Test/Controllers/QfcItemController.FocusAndThemeTests.cs:41-158` into the shared
      `QuickFiler.Test/Controllers/QfcItemController.TestSupport.Fixtures.cs`, and rewire
      `QfcItemController.FocusAndThemeTests.cs` to consume them (AC-15, spec §9.2)
  - Acceptance: the helpers are shared rather than copied; `EnableHandlelessThemeInvoke` remains the
    **single** point of reflection into `UtilitiesCS.Theme`'s private fields so that coupling surface
    does not grow; `QfcItemController.FocusAndThemeTests.cs` drops below 497 lines;
    `QfcItemController.TestSupport.Fixtures.cs` stays under 500 lines; all existing tests still pass
- [ ] [P9-T2] Correct the stale doc comment at
      `QuickFiler.Test/Controllers/QfcItemController.FocusAndThemeTests.cs:20-24`
  - Acceptance: the claim that the two `ToggleFocusAsync` overloads "retain a per-member bucket-(iii)
    exemption and are excluded here" is removed, because both are now exercised through
    `QfcItemController.SeamDispatcherTests.cs:223` and `:269`. Documentation only, no behaviour change
- [ ] [P9-T3] Resolve the unrestored static `UiThread._dispatcher` write in
      `QfcItemControllerTestSupport.EnsureUiThreadDispatcher()`
      (`QuickFiler.Test/Controllers/QfcItemController.TestSupport.cs:238-249`) (AC-15)
  - Acceptance: the static write is wrapped in a save/restore scope so assembly-wide test state is no
    longer order-dependent; if a save/restore proves infeasible because
    `GetDedicatedDispatcher()` parks a process-lifetime background STA thread, an **explicit reasoned
    deviation** is recorded in `<FEATURE>/evidence/other/test-policy-deviations.<timestamp>.md`
    naming the file, the lines, and why. One of the two outcomes must be present
- [ ] [P9-T4] Create `QuickFiler.Test/Controllers/QfcItemController.FocusThemeSelectionTests.cs` with
      a `[TestClass]` skeleton and add its `<Compile Include>` entry to
      `QuickFiler.Test/QuickFiler.Test.csproj`
  - Acceptance: CRLF preserved; solution builds
- [ ] [P9-T5] Add test `ToggleFocus_StateOn_FromDarkNormal_SwitchesToDarkActive` (FT-01) to
      `QfcItemController.FocusThemeSelectionTests.cs`
  - Acceptance: `_activeUI = false`, `_activeTheme = "DarkNormal"`; asserts `"DarkActive"` and
    `_activeUI == true`; empty `_tableLayoutPanels`; covers lines 37-39 and one condition at L36;
    pins invariants I1 and I2
- [ ] [P9-T6] Add test `ToggleFocus_StateOff_FromDarkActive_SwitchesToDarkNormal` (FT-02) to the
      same file
  - Acceptance: covers lines 53-55 and one condition at L52
- [ ] [P9-T7] Add test `ToggleFocus_StateOn_WhenAlreadyActive_ReappliesThemeWithoutStateChange`
      (FT-03) to the same file
  - Acceptance: no state change; the theme is still re-applied and `ToggleTips` is not invoked
    (`Invoke` fires exactly once); closes one condition at L48; pins invariant I4 (idempotency)
- [ ] [P9-T8] Add test `ToggleFocus_Parameterless_FromDarkActive_SwitchesToDarkNormal` (FT-04) to
      the same file
  - Acceptance: covers lines 93-95 and one condition at L92
- [ ] [P9-T9] Add test `ToggleFocus_Parameterless_FromDarkNormal_SwitchesToDarkActive` (FT-05) to
      the same file
  - Acceptance: covers lines 109-111 and one condition at L108
- [ ] [P9-T10] Add test `ToggleFocusAsync_StateOff_WhenActive_RoutesToOffAndAwaitsThemeAsync` (FT-06)
      to the same file
  - Acceptance: `BuildDispatchableTheme` pattern; covers lines 73-75 and two conditions at L72
- [ ] [P9-T11] Add test `ToggleFocusAsync_Parameterless_WhenInactive_RoutesToOnAsync` (FT-07) to the
      same file
  - Acceptance: covers lines 132-134 and one condition at L127
- [ ] [P9-T12] Add test `ToggleFocusOnAsync_FromDarkNormal_SwitchesToDarkActive` (FT-08) to the same file
  - Acceptance: invoked through `QfcItemControllerTestSupport.InvokeNonPublic`; covers lines 142-144
    and one condition at L141
- [ ] [P9-T13] Add test `ToggleFocusOffAsync_FromDarkActive_SwitchesToDarkNormal` (FT-09) to the
      same file
  - Acceptance: covers lines 157-159 and one condition at L156
- [ ] [P9-T14] Add test `SetThemeDark_FromLightActive_SelectsDarkActiveAndLeavesActiveUiUnchanged`
      (FT-18) to the same file
  - Acceptance: `async: true` with `EnsureUiThreadDispatcher()` so theme work is enqueued on the
    parked dispatcher and never executes; covers lines 283-286 and two conditions at L277; pins
    invariant I5
- [ ] [P9-T15] Add test `SetThemeLight_FromDarkActive_SelectsLightActiveAndLeavesActiveUiUnchanged`
      (FT-19) to the same file
  - Acceptance: covers lines 311-314 and two conditions at L305; pins invariant I5
- [ ] [P9-T16] Create `QuickFiler.Test/Controllers/QfcItemController.FocusTipsTests.cs` with a
      `[TestClass]` skeleton and add its `<Compile Include>` entry to
      `QuickFiler.Test/QuickFiler.Test.csproj`
  - Acceptance: CRLF preserved; solution builds
- [ ] [P9-T17] Add test `ToggleNavigation_WhenAsync_UsesBeginInvokeExactlyTwice` (FT-10) to
      `QfcItemController.FocusTipsTests.cs`
  - Acceptance: asserts `BeginInvoke` fires **exactly twice** and `Invoke` never — an **exact count,
    not `Times.AtLeastOnce()`** — so the double toggle at
    `QuickFiler/Controllers/QfcItemController.FocusAndTheme.cs:170` becomes visible without being
    fixed. **Characterisation** of open issue **#480**; the doc comment names #480 (US-5). Covers
    lines 172-174 and one condition at L171
- [ ] [P9-T18] Tighten the existing masking assertion at
      `QuickFiler.Test/Controllers/QfcItemController.FocusAndThemeTests.cs:310` from
      `Times.AtLeastOnce()` to an exact count (US-5)
  - Acceptance: `ToggleNavigation_Synchronous_TogglesPositionTips` asserts the exact number of
    `_itemPositionTips.Toggle` calls, so open issue **#480**'s double toggle is no longer masked; the
    assertion is **tightened, never inverted**, so a future #480 fix updates rather than reverses it;
    the doc comment names #480; behaviour is not changed
- [ ] [P9-T19] Add test `ToggleNavigation_WithStateAndAsync_PassesSuppliedState` (FT-11) to
      `QfcItemController.FocusTipsTests.cs`
  - Acceptance: covers lines 184-188 and one condition at L183
- [ ] [P9-T20] Add test `ToggleTips_Synchronous_WhenExpanded_TogglesExpandedTipWithShareColumnFalse`
      (FT-12) to the same file
  - Acceptance: `_expanded = true` with a one-element `_listTipsExpanded`; `_tableLayoutPanels` stays
    an **empty** `List<TableLayoutPanel>` so no WinForms control is constructed; covers lines 211-213
    and three conditions at L210/L212
- [ ] [P9-T21] Add test `ToggleTips_Synchronous_WhenForceFlagSet_TogglesExpandedTipsEvenIfCollapsed`
      (FT-13) to the same file
  - Acceptance: `_expanded = false` with `On | Force`; closes one condition at L210; pins invariant I6
- [ ] [P9-T22] Add test `ToggleTipsAsync_WithDetailTips_AwaitsToggleAsyncOnce` (FT-14) to the same file
  - Acceptance: `Mock<IQfcTipsDetails>` returning `Task.CompletedTask`; covers lines 229-231 and one
    condition at L228
- [ ] [P9-T23] Add test `ToggleTipsAsync_WhenExpanded_TogglesExpandedTips` (FT-15) to the same file
  - Acceptance: covers lines 237-241 and 245 and three conditions at L236/L238
- [ ] [P9-T24] Add test `ToggleTipsAsync_WhenForceFlagSet_TogglesExpandedTipsEvenIfCollapsed`
      (FT-16) to the same file
  - Acceptance: closes one condition at L236; pins invariant I6
- [ ] [P9-T25] Add test `ToggleTipsAsync_WhenTokenPreCancelled_ThrowsBeforeTogglingAnyTip` (FT-17)
      to the same file
  - Acceptance: pre-cancelled `Token`; asserts `OperationCanceledException` and that no tip was
    toggled; pins invariant I8
- [ ] [P9-T26] Create `QuickFiler.Test/Controllers/QfcItemController.HtmlDarkConverterTests.cs` with
      a `[TestClass]` skeleton and add its `<Compile Include>` entry to
      `QuickFiler.Test/QuickFiler.Test.csproj`
  - Acceptance: CRLF preserved; solution builds
- [ ] [P9-T27] Add test `HtmlDarkConverter_WhenInitialisedAndNoExpandedItems_NavigatesOnce` (FT-20)
      to `QfcItemController.HtmlDarkConverterTests.cs`
  - Acceptance: `_isWebViewerInitialized = true` via `SetField`; a default `MailItemHelper`; a
    `ConversationResolver` built through the **current positional** two-argument constructor with
    `Count = new Pair<int>(0, 0)`; asserts `NavigateToString` called once and no per-item toggle;
    covers lines 292-294 and 300 and two conditions at L291/L294
- [ ] [P9-T28] Add test `HtmlDarkConverter_WhenExpandedItemPresent_AppliesToggleDarkToThatItem`
      (FT-21) to the same file
  - Acceptance: `Count.Expanded == 1` with a one-element `ConversationInfo.Expanded`; covers lines
    295-299 and one condition at L294. `ConversationResolver.cs` is **not edited**
- [ ] [P9-T29] Verify per-file coverage for `QuickFiler/Controllers/QfcItemController.FocusAndTheme.cs`
      and confirm file sizes
  - Acceptance: `<FEATURE>/evidence/qa-gates/coverage-FocusAndTheme.<timestamp>.md` records line
    >= 80% and branch >= 75% independently, recomputed per `spec.md` §4, dual-figure (projected
    100% line / 100% branch; Tier A alone projects 88.6% / 77.9%). The production file remains at
    326 lines with **zero production change**;
    `QuickFiler.Test/Controllers/QfcItemController.FocusAndThemeTests.cs` and each of the three new
    test files are <= 500 lines

### Phase 10 — QfcItemController.MailActions.cs

Baseline 76.80% line (96/125) / **72.73% branch (16/22) — fails both gates**, and this is the file
that **falsely passes** on the emitted `branch-rate="0.75"`, which is the concrete reason the
recomputed method is mandatory. Tier A clears both gates with no production change; the S2
`FlagTasks` runner seam then removes a real "one line further shows a modal dialog" hazard. The
`Action<string> _showUserMessage` seam (research Tier C, MA-15) is **not scheduled** — it is outside
the permitted production-change set — so the `MoveMailAsync` catch block stays uncovered and is
recorded as an accepted residual.

- [ ] [P10-T1] Verify the current line count of
      `QuickFiler/Controllers/QfcItemController.Initialization.cs` **immediately before** adding the
      S2 seam default (risk R6)
  - Acceptance: the file is at or below 500 lines with at least 2 lines of headroom (expected ~370
    after `[P2-T2]` and `[P2-T3]`, or 372 before csharpier normalization); recorded in
    `<FEATURE>/evidence/other/initialization-size-precheck.<timestamp>.md`. If headroom is
    insufficient, HALT and notify rather than pushing the file past 500
- [ ] [P10-T2] Add the S2 `FlagTasks` runner seam: declare
      `private Func<FlagTasks, bool, DialogResult> _flagTasksRunner;` in
      `QuickFiler/Controllers/QfcItemController.cs`, add
      `_flagTasksRunner ??= (flagTask, modal) => flagTask.Run(modal);` to the `??=` default block in
      `QuickFiler/Controllers/QfcItemController.Initialization.cs:380-397`, and replace
      `flagTask.Run(modal: true)` at `QuickFiler/Controllers/QfcItemController.MailActions.cs:176`
      and `:194` with `_flagTasksRunner(flagTask, true)`
  - Acceptance: **explicit justification recorded** as required by AC-10 —
    `TaskVisualization.FlagTasks.Run(bool)` (`FlagTasks.cs:89`) is **non-virtual** and calls
    `_viewer.ShowDialog()` at `:95`, so Moq cannot intercept it and any test reaching it would open a
    live modal dialog; `TaskVisualization` is outside every epic child's assignment, so the remedy
    must be F10-local. The seam follows the pattern already used eight times in `SaveParameters`
    (`Initialization.cs:380-397`), is
    additive and non-breaking, and reproduces the current expression exactly, so there is **no
    behaviour change**. The `MailActions.cs` replacements are 1:1, leaving its line count and
    coverage denominator unchanged. `Initialization.cs` remains <= 500 lines; the solution builds
- [ ] [P10-T3] Add test `RightKeyActions_PopOut_InvokesPopOutControlGroupWithItemNumber` (MA-01) to
      the existing `QuickFiler.Test/Controllers/QfcItemController.MailActionsTests.cs`
  - Acceptance: `Mock<IQfcCollectionController>`; invoking the retrieved delegate calls
    `PopOutControlGroup(ItemNumber)` once; covers line 59. New tests consume the shared
    `HarnessController` rather than the file-local `MailController` subclass
- [ ] [P10-T4] Add test `RightKeyActions_Expand_FocusesSubjectBeforeEnumeratingConversation` (MA-02)
      to the same file
  - Acceptance: an ordered/`MockSequence` assertion proves `FocusSubject()` runs before
    `EnumerateConversation()`; covers lines 63-66; pins invariant J4
- [ ] [P10-T5] Add test `RightKeyActions_Cancel_TouchesNoCollaborator` (MA-03) to the same file
  - Acceptance: strict mocks or `VerifyNoOtherCalls()`; covers line 68; pins invariant J3
- [ ] [P10-T6] Add test `RightKeyActionsAsync_PopOut_InvokesPopOutControlGroupAsync` (MA-04) to the
      same file
  - Acceptance: awaited; covers line 77
- [ ] [P10-T7] Add test `RightKeyActionsAsync_Expand_RoutesThroughDispatcherIntoEnumerate` (MA-05)
      to the same file
  - Acceptance: `BuildSyncDispatcher()`; awaited; covers line 78
- [ ] [P10-T8] Add test `RightKeyActionsAsync_Cancel_ReturnsCompletedTaskAndTouchesNoCollaborator`
      (MA-06) to the same file
  - Acceptance: covers line 79
- [ ] [P10-T9] Add test `PackageItems_WhenConversationChecked_ReturnsResolverSameFolderList` (MA-07)
      to the same file
  - Acceptance: `_optionConversationChecked = true` with `ConversationInfo` seeded through its public
    setter; closes the remaining condition at L162; pins invariant J8
- [ ] [P10-T10] Add test `MoveMailAsync_WhenDestinationIsTrashPseudoFolder_DisablesSaveAttachments`
      (MA-08) to the same file
  - Acceptance: `_selectedFolder = "Trash to Delete"` with `_optionAttachments = true`; asserts the
    captured `EmailFilerConfig.SaveAttachments` is `false`; closes the remaining condition at L90;
    pins invariant J6. Reuses the existing fixture pattern that pre-trips the `FilerQueue`
    `ThreadSafeSingleShotGuard` by reflection so **no consumer thread starts**
- [ ] [P10-T11] Create `QuickFiler.Test/Controllers/QfcItemController.MailActionsSeamTests.cs` with
      a `[TestClass]` skeleton and add its `<Compile Include>` entry to
      `QuickFiler.Test/QuickFiler.Test.csproj`
  - Acceptance: CRLF preserved; solution builds
- [ ] [P10-T12] Add test `SaveParameters_LeavesFlagTasksRunnerNonNullWhenNotInjected` (MA-10) to
      `QfcItemController.MailActionsSeamTests.cs`
  - Acceptance: guards the production default added in `[P10-T2]`; the default delegate is captured
    but **never invoked**, so no `FlagTasks` is constructed and no dialog is reached
- [ ] [P10-T13] Add test `FlagAsTask_WhenRunnerReturnsOk_SetsDialogResultAndClickedBackColor`
      (MA-11) to the same file
  - Acceptance: injected runner returns `DialogResult.OK`; the factory returns `null` (the runner
    never dereferences it); asserts `FlagTaskDialogResult` is OK and `FlagTaskBackColor` is
    `_themes[_activeTheme].ButtonClickedColor`; covers lines 176 and 178-181 and one condition at
    L177; pins invariant J9. **No modal dialog is shown**
- [ ] [P10-T14] Add test `FlagAsTask_WhenRunnerReturnsCancel_RecordsResultWithoutBackColorWrite`
      (MA-12) to the same file
  - Acceptance: covers line 177 and the other condition at L177; pins invariant J9
- [ ] [P10-T15] Add test `FlagAsTaskAsync_WhenRunnerReturnsOk_CompletesInsideOneDispatch` (MA-13) to
      the same file
  - Acceptance: `BuildSyncDispatcher()`; asserts factory, runner and colour write all occur inside a
    single `InvokeAsync` callback; covers lines 194 and 196-200 and one condition at L195; pins
    invariant J10
- [ ] [P10-T16] Add test `FlagAsTaskAsync_WhenRunnerReturnsCancel_WritesNoBackColor` (MA-14) to the
      same file
  - Acceptance: covers line 195 and the other condition at L195
- [ ] [P10-T17] Add test `MarkItemForDeletionAsync_WhenTokenPreCancelled_ThrowsBeforeDispatch`
      (MA-16) to the same file
  - Acceptance: pre-cancelled `Token`; asserts the throw occurs before any `IUiDispatcher` call;
    pins invariant J12. Requires no seam
- [ ] [P10-T18] Record the accepted uncovered residual at
      `QuickFiler/Controllers/QfcItemController.MailActions.cs:115-122`
  - Acceptance: appended to `<FEATURE>/evidence/other/uncovered-residuals.<timestamp>.md` — research
    Tier C (MA-15) would cover the `MoveMailAsync` catch block via an
    `Action<string> _showUserMessage` seam defaulted to `MessageBox.Show`, but that seam is **outside
    the permitted production-change set** for this child, so the seven lines remain uncovered by
    design. The file still clears both gates (projected 94.4% line / 90.9% branch). The record names
    open issue **#483** as the tracked defect and states that the seam is the correct future remedy
- [ ] [P10-T19] Verify per-file coverage for `QuickFiler/Controllers/QfcItemController.MailActions.cs`
  - Acceptance: `<FEATURE>/evidence/qa-gates/coverage-MailActions.<timestamp>.md` records line
    >= 80% and branch >= 75% independently, recomputed per `spec.md` §4, and commits **both** the
    emitted figure and the recomputed figure side by side with an explicit note that the emitted
    `branch-rate="0.75"` was a false pass against a true 72.73% (AC-5, US-7). Expected ~94.4% line /
    ~90.9% branch. `QfcItemController.MailActions.cs` remains at 224 lines

### Phase 11 — IQfcItemController.cs

`QuickFiler/Interfaces/IQfcItemController.cs` has **zero coverable lines**: 107 lines comprising 57
bodiless member declarations, no default interface implementation, no `static` or `const` member, no
field, no nested type, no attribute, no static constructor. This phase contains **only** its ledger
classification task and **explicitly no test tasks**.

- [ ] [P11-T1] Classify `QuickFiler/Interfaces/IQfcItemController.cs` as
      `interface-only / not-measured` in F1's coverage ledger at
      `docs/features/epics/quickfiler-per-file-coverage/coverage-ledger.md`
  - Acceptance: the appended row reports **N/A for both line and branch, never 0%**, and never counts
    as a failure. The rationale records the positive control that proves the folder was instrumented
    — the sibling `QuickFiler/Interfaces/MailItemActionsAdapter.cs` appears in the Cobertura report
    at `line-rate="1"` — and the corroborating platform constraint that
    `QuickFiler/QuickFiler.csproj:13` targets `v4.8.1`, which cannot support a default interface
    implementation. **No `[ExcludeFromCodeCoverage]` is added to this file**, the file is not deleted
    or trimmed, and **no test is written for it**. Shape-assertion and reflection-over-interface
    tests are prohibited (AC-6, US-9). The ledger edit preserves CRLF and is one minimal hunk

### Phase 12 — Exemption Boundary, Ledger Rows, Cross-Child Notes, and Defect Promotion

Cross-file governance deliverables that belong to no single production file. Phases 1-11 satisfy the
one-phase-per-production-file structure; this phase carries the artifacts those phases feed.

- [ ] [P12-T1] Re-verify the **bucket-1** ratified attributes that survive in the F10 file set
      against current source and record the outcome per member
  - Acceptance: `QfcItemController.Initialization.cs:168, 200, 260, 291` and
    `QfcItemController.ViewerSetup.cs:253` are each checked with the three-question method in
    `spec.md` §3.5 (same shape? barrier still present? technique now proven elsewhere?). Each is
    recorded `holds` with current-code evidence, or `lapsed` with the defeating evidence. Open issue
    **#230** is cited by number as the externally-tracked justification for every retained
    bucket-1 attribute. **No task in this plan attempts to build the #230 seam** (AC-2, AC-8)
- [ ] [P12-T2] Re-verify the **bucket-2** "deliberate `virtual` test seam" attributes and record the
      outcome per member
  - Acceptance: `QfcItemController.Conversation.cs:79` (`DoLoadConversationResolverCoreAsync`) is
    recorded `holds` with the three current-code checks — still `protected virtual` at
    `Conversation.cs:80`; exactly two `protected override` declarations exist solution-wide
    (`QfcItemController.ConversationTests.cs:37`, `QfcItemControllerTests.cs:46`); no direct or
    reflective call to the base body exists. `QfcItemController.Navigation.cs:173` and `:191` are
    recorded `holds` with the distinction that the **in-code comment** was stale (corrected in
    `[P8-T13]`) while the **ratified rationale** was written with the R2 `IContainerControlLocal`
    retrofit explicitly in view ("now de-exempted at the leaf via R2") and has therefore not lapsed.
    The artifact additionally records the observation that, post-R2, the deliberate-seam argument is
    materially **weaker** for the `Navigation.cs` pair than for `DoLoadConversationResolverCoreAsync`,
    and **refers that observation to the maintainer for re-review without acting on it** (AC-21, US-3)
- [ ] [P12-T3] Re-verify the **bucket-3** `async void` shell attributes and record the outcome per member
  - Acceptance: `QfcItemController.EventHandlers.cs:60, 83, 97, 111, 125` and
    `QfcItemController.EventWiring.cs:99` are each recorded `holds`, with the re-verified rationale
    stated as "**`async void` cannot be awaited deterministically**" rather than "the routing is
    untestable" — the routing is already proven, and each shell's `*Core` body is already at 100%
    (`SeamCoreTests.cs:104`, `:120`, `:131`, `:142`, `:153`) or covered through
    `HandleWebViewInitializedAsync`. The in-file inconsistency with the non-exempt `BtnDelItem_Click`
    and `BtnFlagTask_Click` is acknowledged and answered on the narrower `void` vs `async void`
    reading, which the per-file research governs
- [ ] [P12-T4] Record the three deletions as **removals of dead members**, not de-exemptions
  - Acceptance: `Initialization.cs:138` (`Initialize`, 9-arg private), `:403` (`CreateAsync`) and
    `:436` (`CreateSequentialAsync`) are recorded as deletions with the no-reflection-caller evidence
    from `[P2-T1]`, and it is stated explicitly that they cost **zero denominator lines** because
    their bodies were outside the denominator (AC-7)
- [ ] [P12-T5] Record the disposition of the one **unratified** attribute,
      `EnsureBreadcrumbPipeline` at `QfcItemController.ViewerSetup.cs:132`
  - Acceptance: recorded as removed and covered, asserted on **F10's own authority**, with the
    covering tests named (`[P3-T19]`..`[P3-T23]`) and an explicit statement that the #227
    ratification does **not** cover this site and was not appealed to (AC-3)
- [ ] [P12-T6] Write the consolidated fresh exemption-boundary artifact at
      `<FEATURE>/evidence/other/exemption-boundary.<ISO-8601>.md`
  - Acceptance: the artifact (a) states the #227 ratified boundary as the baseline — 18 members
    within F10's file set; (b) records the per-member re-verification outcome from `[P12-T1]`..
    `[P12-T3]`; (c) records the three deletions separately per `[P12-T4]`; (d) records the unratified
    disposition per `[P12-T5]`; (e) states the final count and reconciles it against the
    **19 − 3 − 1 = 15** arithmetic; (f) cites
    `docs/features/archive/2026-06-29-qfc-item-controller-testability-227/maintainer-decision.2026-07-02.md`
    and
    `docs/features/archive/2026-06-29-qfc-item-controller-testability-227/evidence/other/exemption-boundary.2026-07-02T17-00.md`
    **by path**. Every remaining attribute has an entry naming the member as `file:line`, whether it
    is covered by the #227 ratification or asserted on F10's own authority, the current barrier, and
    what would remove it. **No entry says only "retained" or points at a category without naming the
    member** (AC-2, US-1)
- [ ] [P12-T7] Verify the actual `[ExcludeFromCodeCoverage]` count in the F10 file set is **exactly 15**
  - Acceptance: a grep across the ten `QuickFiler/Controllers/QfcItemController*.cs` files plus
    `QuickFiler/Interfaces/IQfcItemController.cs` returns exactly 15 hits; the surviving sites are
    `Initialization.cs` (4), `ViewerSetup.cs` (2), `Navigation.cs` (2), `EventHandlers.cs` (5),
    `EventWiring.cs` (1), `Conversation.cs` (1). Recorded in
    `<FEATURE>/evidence/other/exemption-count-final.<timestamp>.md`. A count **below 15 is a
    failure**, not an improvement — each remaining attribute is ratified under #227 and an executor
    must not reduce the count further on its own authority (AC-8)
- [ ] [P12-T8] Append F1 coverage-ledger rows for the ten measured production partials at
      `docs/features/epics/quickfiler-per-file-coverage/coverage-ledger.md`
  - Acceptance: one row per file with the measured line and branch figures from this child's own
    harness run, the bucket, and — for every retained attribute — a **per-member** disposition citing
    `maintainer-decision.2026-07-02.md` and the fresh exemption-boundary artifact as the basis, per
    `spec.md` §3.2. Rows are member-level for this family, correcting the epic's file-level marking
    (deviation D4). CRLF preserved; one minimal additive hunk; fan-in conflicts on this file are
    expected and are resolved by keeping both sides
- [ ] [P12-T9] Append the F1 coverage-ledger row for the new production file
      `QuickFiler/Controllers/QfcCidImageResolver.cs`
  - Acceptance: bucket `testable`, target **>= 90% line** per the epic's "Mid-Wave File Creation"
    rule 4, with the measured figure from `[P3-T27]`. The row lands in the same change set as the
    `<Compile Include>` entry added in `[P3-T8]` (AC-11)
- [ ] [P12-T10] Commit the dual-figure coverage evidence with the recomputation method stated (AC-5, US-7)
  - Acceptance: `<FEATURE>/evidence/qa-gates/coverage-method-and-dual-figures.<timestamp>.md` states
    the recomputation method in reproducible terms — unique class-level `<line>` children and
    `hits="0"` entries for lines; summed `condition-coverage` numerators and denominators for
    branches — shows the harness figure and the recomputed figure side by side for all ten files, and
    names open issue **#441** as the reason they differ. The raw Cobertura XML is committed alongside
    so a reviewer can reproduce the arithmetic without re-running the suite. **No new issue is filed
    for #441**
- [ ] [P12-T11] Deliver the cross-child contract note to **F4 (#434)** and mirror it locally (US-13)
  - Acceptance: posted as a comment on issue #434 stating that F10 binds the **concrete**
    `ConversationResolver` at three positional sites (`Conversation.cs:34`;
    `Initialization.cs:382-388`, 5 positional; `Conversation.cs:85-92`, the `MailItemHelper`
    `LoadAsync` overload at `ConversationResolver.cs:126-133`, 6 positional); that F4 **may append
    defaulted parameters** but must not reorder, retype or remove existing positional parameters,
    must not tighten `Count`'s setter below `internal`, must not make `ConversationInfo`'s setter
    non-public, and must not add a `LoadAsync` overload that makes the `:126` binding ambiguous;
    that **retyping `ConversationResolver` to `IConversationResolver` is a three-child breaking
    change** (F4 owns it, F10 passes it, F11 receives it via `ToggleUnGroupConv`); that
    `QfcThemeHelper.SetupThemes`'s four-argument shape must survive and an additive
    `IItemViewer`-accepting overload would be welcome but **F10 will not add it**; and that reverting
    `TlpCellSnapShotList.ApplyState` from `IContainerControlLocal` to a concrete `Control` would
    re-block F10. Mirrored to `<FEATURE>/evidence/issue-updates/issue-434.<timestamp>.md` with
    `Timestamp:`, the exact text, `PostedAs: comment`, and the comment URL
- [ ] [P12-T12] Deliver the cross-child contract note to **F14 (#456)** and mirror it locally (US-13)
  - Acceptance: posted as a comment on issue #456 stating that F10 depends on `IItemViewer`
    continuing to derive from `IContainerControlLocal` (`IItemViewer.cs:15`); on
    `new QuickFiler.ItemViewer()` remaining constructible headlessly under a plain
    `SynchronizationContext`; and on the cast-reached concrete members keeping their names and
    signatures (`L0v2h2_WebView2`, `L0vhBreadcrumb_WebView2`, `TopicThread`, `LblItemNumber`,
    `GetAllChildren()`, `ForAllControls(...)`, `BreadcrumbCoordinator`, `InitializeBreadcrumbPipeline`,
    `BreadcrumbUnhandledArrow`, `ResetBreadcrumb`, `ConfigureBreadcrumbDropDown`, `SetBreadcrumbTheme`,
    `AttachBreadcrumbWebViewAsync`). It also records that F10 keeps using
    `QfcItemControllerTestSupport.StartRunningDispatcher()` rather than proposing a re-type of the
    concrete sealed `IItemViewer.UiDispatcher` — that is F14's decision. Mirrored to
    `<FEATURE>/evidence/issue-updates/issue-456.<timestamp>.md`
- [ ] [P12-T13] Deliver the cross-child contract note to **F3 (#430)** and mirror it locally (US-13)
  - Acceptance: posted as a comment on issue #430 stating that `IQfcKeyboardHandler` must keep the
    four `KbdActions<...>` properties with exactly their current three type arguments, and that
    `KbdActions<TKey, UClass, VDelegate>` must keep `Add(string, TKey, VDelegate)`,
    `Remove(string, TKey)`, `ContainsKey(TKey)` and the indexer `public VDelegate this[TKey key]`
    (`KbdActions.cs:36-47`) — **the indexer is load-bearing**, being the only way a test retrieves a
    registered lambda to invoke it, which is how the registered-lambda lines in `EventWiring.cs`
    become covered. It also records the conditional: if F3's fix for **#444** makes `Add` idempotent,
    F10's re-entrancy tests `[P6-T18]` and `[P6-T19]` change from "throws" to "no-op". Mirrored to
    `<FEATURE>/evidence/issue-updates/issue-430.<timestamp>.md`
- [ ] [P12-T14] Deliver the cross-child contract note to **F11 (#454)** and mirror it locally (US-13)
  - Acceptance: posted as a comment on issue #454 stating that `IQfcCollectionController` is consumed
    only through mocks, that `ToggleUnGroupConv`'s first parameter is the **concrete**
    `ConversationResolver` so a retype is a three-child break, and that the cross-variant
    expansion-registry defect promoted as **#482** most likely belongs at
    `QuickFiler/Controllers/QfcCollectionController.cs:1439` — an `async` method calling the
    synchronous `ToggleExpansion()` — not in an F10 file. Mirrored to
    `<FEATURE>/evidence/issue-updates/issue-454.<timestamp>.md`
- [ ] [P12-T15] Propagate deviations **D1** and **D4** to F1 (#432) and to the epic (#136), and mirror
      locally (US-14, AC-20)
  - Acceptance: posted as comments on issues #432 and #136 recording (D1) that
    `QuickFiler.Test/QuickFiler.Test.csproj` is **also** a legacy non-SDK, non-globbing shared file
    with 107 explicit `<Compile Include=...>` entries that every child adding a test file must edit,
    making it a **higher-conflict surface than the production csproj**, which epic.md's "Cross-Child
    Constraints" §1 omits; and (D4) that all 19 attributes in this family are **member-level**, none
    sits on a `partial class` declaration, all ten partials are instrumented, and F1's ledger must
    therefore record disposition **per member** for this family. The same comment reports the
    instrumentation finding that `[ExcludeFromCodeCoverage]` **does not propagate to lambdas declared
    inside the exempt method**, which silently contributes permanently-uncovered lines to a file's
    denominator epic-wide (AC-17). `docs/features/epics/quickfiler-per-file-coverage/epic.md` is
    **not edited** by this child. Mirrored to
    `<FEATURE>/evidence/issue-updates/issue-432.<timestamp>.md` and `issue-136.<timestamp>.md`
- [ ] [P12-T16] Add the "#441 can deflate as well as inflate" refinement as a **comment on issue
      #441** and mirror it locally (AC-17)
  - Acceptance: the comment states that the defect's direction is **data-dependent**, so no
    correction factor exists: `QfcItemController.Conversation.cs` is over-reported (91.18% emitted vs
    88.24% true) because covered lines are the ones duplicated, while
    `QfcItemController.Initialization.cs` is under-reported (90.11% emitted vs 91.79% true) because
    the class-level union masks uncovered closure entries by taking max hits. **#441 is not
    re-filed.** Mirrored to `<FEATURE>/evidence/issue-updates/issue-441.<timestamp>.md`
- [ ] [P12-T17] Promote every latent defect found during execution that is **not already covered** by
      #480-#485 to a GitHub issue via the MCP promotion lifecycle, before this child completes (AC-17)
  - Acceptance: the candidate set from research is triaged and each item is either promoted or
    recorded as a duplicate of an existing issue with that issue named. Candidates include: the
    en-dash in `CoreWebView2EnvironmentOptions("–incognito ")` at `ViewerSetup.cs:52`; the
    `throw (initException)` capable of throwing `null` at `EventWiring.cs:117`; the `'A'`
    `ToggleSaveAttachments` keyboard action bound to an entirely commented-out body; the
    non-flag-aware `ToggleConversationCheckbox(Enums.ToggleState)` switch at `Navigation.cs:130-143`;
    `ApplyReadEmailFormat` writing the unread state and saving twice; `PopulateFolderComboBoxAsync`
    double-wrapping `Task.Run` at `FolderHandling.cs:157`; the ignored `async` constructor parameter
    at `Initialization.cs:111-133`; the missing `_folderHandler` null guard at
    `EventHandlers.cs:166-171`; the unchecked `as` result at `EventHandlers.cs:199-200`; the
    inconsistent cancellation handling across `MailActions.cs`'s async members; and the discarded
    `bool` from `KbdActions.Remove` at all 33 `EventWiring.cs` call sites. **Nothing is left as prose
    in this feature folder.** `#441`, `#457`, `#463`, `#444`, `#450`, `#230`, `#427`, `#438`, `#440`
    are **not** re-filed, and none of #480-#485 is fixed
- [ ] [P12-T18] Record deviations **D2, D3, D5, D6, D7, D8, D9** in this child's completion summary
      (AC-20)
  - Acceptance: `<FEATURE>/evidence/other/documented-deviations.<timestamp>.md` records: D2 (#400 and
    #424 are Closed; the live risks are #230, #427, #438, #440, #441); D3 (`IQfcDatamodel` is a false
    positive with zero references in the F10 file set); D5 (branch is the binding gate on **seven of
    ten** files, not four sub-floor files on line coverage); D6 (the epic's per-file percentages are
    emitted double counts; the true union figures are 72.5%, 74.3%, 76.8%, 79.6%); D7 (the emitted
    rates cannot be trusted for gate decisions); D8 (three of the 19 sites were on dead members);
    D9 (the attribute does not propagate to lambdas). Each entry names the source claim, the
    correction, and the evidence
  - Acceptance (continued): the same artifact ALSO records two interpretation deviations that
    `[P13-T8]` must check AC compliance against, rather than against the ACs' literal wording:
    **D10** — spec AC-15 says the headless-`ItemViewer` fixture "is consolidated into
    `QfcItemController.TestSupport.cs`", but `[P3-T1]` places it in a new
    `QfcItemController.TestSupport.Fixtures.cs` because that file is already at 365 lines and the
    `[P9-T1]` promotions would breach the 500-line limit; the AC's intent (one shared fixture, not
    three duplicates) is met while its literal file name is not.
    **D11** — spec AC-4's first sentence requires an attribute removal to land "in the same atomic
    task" as its covering tests, which is unsatisfiable alongside issue #136's one-test-case-per-task
    mandate; the plan satisfies AC-4's operative second clause instead by strict ordering
    (`[P3-T19]`..`[P3-T23]` land the tests, then `[P3-T24]` removes the attribute), so no
    between-task measurement shows a file below either floor

### Phase 13 — Final QC Loop and Acceptance Verification

Run the full C# toolchain in this exact order, unconditionally. There is no `SKIPPED` path for any
task in this phase. If any step fails or changes files, restart from `[P13-T1]`.

- [ ] [P13-T1] Run `dotnet tool run csharpier .`
  - Acceptance: `<FEATURE>/evidence/qa-gates/final-qc-format.<timestamp>.md` with `Timestamp:`,
    `Command:`, `EXIT_CODE:`, `Output Summary:`. If the command reformats any file, restart the loop
    from this task
- [ ] [P13-T2] Run
      `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
  - Acceptance: `<FEATURE>/evidence/qa-gates/final-qc-analyzers.<timestamp>.md` with the four
    required fields; zero errors; the warning delta against the `[P0-T9]` baseline stated in
    `Output Summary:`
- [ ] [P13-T3] Run
      `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true`
  - Acceptance: `<FEATURE>/evidence/qa-gates/final-qc-nullable.<timestamp>.md` with the four required
    fields; zero errors
- [ ] [P13-T4] Run `vstest.console.exe <test-assembly-paths> /EnableCodeCoverage` (coverage mode is
      mandatory), driven through F1's per-file harness so the per-file report is produced from the
      same run
  - Acceptance: `<FEATURE>/evidence/qa-gates/final-qc-test.<timestamp>.md` with `Timestamp:`,
    `Command:`, `EXIT_CODE:`, `Output Summary:`; all tests pass; `Output Summary:` carries the
    **numeric post-change per-file line AND branch coverage** for all ten production partials plus
    `QfcCidImageResolver.cs`, and the repository-wide line rate. Figures are recomputed per
    `spec.md` §4. The raw Cobertura XML is committed under `<FEATURE>/evidence/qa-gates/`
- [ ] [P13-T5] Confirm a single clean pass of `[P13-T1]`..`[P13-T4]` in order, with no step failing
      and no step changing files
  - Acceptance: `<FEATURE>/evidence/qa-gates/final-qc-loop.<timestamp>.md` states the four commands
    run, in order, in the final pass, and that all four completed without errors. If any earlier
    iteration failed or auto-fixed files, the artifact records the restart count (AC-18)
- [ ] [P13-T6] Produce the delta and threshold report
  - Acceptance: `<FEATURE>/evidence/qa-gates/coverage-delta.<timestamp>.md` reports, per file, the
    **baseline** figure from `[P0-T11]`, the **post-change** figure from `[P13-T4]`, and the delta —
    for **line and branch independently** — for all ten production partials; the **new-file**
    coverage for `QuickFiler/Controllers/QfcCidImageResolver.cs` against the **>= 90% line** target;
    `QuickFiler/Interfaces/IQfcItemController.cs` as **N/A, never 0%**; and the **repository-wide
    line coverage before and after**, evidencing "retained or improved" (AC-1, AC-9, AC-11, US-8,
    US-9). The summary states plainly that **branch was the binding gate on seven of the ten
    partials**. Every file is confirmed at **>= 80% line and >= 75% branch**, with no file described
    as passing on the strength of its line figure alone
- [ ] [P13-T7] Verify the 500-line limit across the entire change set (AC-12, AC-13)
  - Acceptance: no production file in scope exceeds 500 lines — with
    `QuickFiler/Controllers/QfcItemController.Initialization.cs` and
    `QfcItemController.ViewerSetup.cs` verified explicitly — and **no test file in the delivered
    change set exceeds 500 lines**, including `QfcItemController.FolderHandlingTests.cs`,
    `QfcItemController.FocusAndThemeTests.cs`, `QfcItemController.TestSupport.cs`, and every file
    created by this child. Recorded in `<FEATURE>/evidence/qa-gates/file-sizes-final.<timestamp>.md`
- [ ] [P13-T8] Verify and check off every acceptance criterion per the
      `acceptance-criteria-tracking` skill
  - Acceptance: `spec.md` **AC-1 through AC-21** and `user-story.md` **US-1 through US-14** are each
    verified against committed evidence and checked off in their source documents, and `spec.md` §15
    **Definition of Done** is checked off. **AC-4 and AC-15 are verified against the recorded
    interpretations D11 and D10 in `[P12-T18]`'s deviations artifact, not against their literal
    wording** — D11 for AC-4's "same atomic task" clause (satisfied by strict ordering) and D10 for
    AC-15's literal `QfcItemController.TestSupport.cs` filename (satisfied by
    `QfcItemController.TestSupport.Fixtures.cs`). A status summary at
    `<FEATURE>/evidence/qa-gates/acceptance-criteria-status.<timestamp>.md` lists every criterion
    with its verdict and the evidence path that satisfies it. Any criterion that cannot be evidenced
    makes the outcome **remediation-required**, never PASS
- [ ] [P13-T9] Verify the working tree is clean and all evidence is committed
  - Acceptance: `git status --porcelain` is empty; every artifact named in this plan exists under
    `<FEATURE>/evidence/<kind>/`; **no evidence file exists under `artifacts/qa-gates/`,
    `artifacts/baseline/`, `artifacts/baselines/`, `artifacts/coverage/`, or any other non-canonical
    `artifacts/` path** (AC-19); the diff touches no file listed in `spec.md` §2.2; and
    `QuickFiler/Interfaces/IQfcItemController.cs`, `IItemViewer.cs` and
    `UtilitiesCS/Properties/AssemblyInfo.cs` are unmodified

---

## Test Plan

- **Unit:** MSTest, Moq, FluentAssertions, Arrange–Act–Assert. **145 new test cases**, one per atomic
  task, distributed across **17 new test fixtures plus 1 new shared-fixture file**, with edits to 6
  existing fixtures — all under `QuickFiler.Test/Controllers/`. Per-file counts: `QfcItemController.cs`
  3, `.Initialization.cs` 5, `.ViewerSetup.cs` 17, `.Conversation.cs` 3, `.FolderHandling.cs` 8,
  `.EventWiring.cs` 53, `.EventHandlers.cs` 11, `.Navigation.cs` 10, `.FocusAndTheme.cs` 21,
  `.MailActions.cs` 14; `IQfcItemController.cs` **0 by design**. No integration tests, no manual
  steps, no `*.StaTests.cs` file.
- **Determinism:** injected delegates in place of every wait, timer and dialog. No `Thread.Sleep`, no
  `Task.Delay`, no wall-clock wait, no temporary file, no external service, no live form, no popup.
- **Coverage evidence:**
  - Baseline: `<FEATURE>/evidence/baseline/coverage-per-file.<timestamp>.md`,
    `coverage-repository-wide.<timestamp>.md`
  - Per-phase gates: `<FEATURE>/evidence/qa-gates/coverage-<File>.<timestamp>.md` (Phases 1-10)
  - Post-change: `<FEATURE>/evidence/qa-gates/final-qc-test.<timestamp>.md`
  - Comparison: `<FEATURE>/evidence/qa-gates/coverage-delta.<timestamp>.md`
  - Method and dual figures: `<FEATURE>/evidence/qa-gates/coverage-method-and-dual-figures.<timestamp>.md`
- **Missing-evidence rule:** if any baseline, QA or comparison artifact is absent or incomplete, the
  outcome is BLOCKED or INCOMPLETE — never PASS.

## Open Questions / Notes

1. **F1 is an execution-time dependency.** `[P0-T6]` halts if F1's ledger or per-file harness is
   absent at execution time. Their absence during preparation and preflight is expected and is not a
   defect.
2. **Research recommendations deliberately not scheduled**, each because AC-8 caps the exemption
   reduction at 15: `Initialization.md` Group B, `ViewerSetup.md` Group D, and `Navigation.md`
   NV-1/NV-2/NV-4/NV-5. Every affected file clears both gates without them.
3. **Research proposals not scheduled for policy reasons:** `Initialization.md` A2 (would construct a
   real `FlagTasks`, whose constructor touches live COM and can raise a `MessageBox`);
   `MailActions.md` MA-15 and its `Action<string> _showUserMessage` seam (outside the permitted
   production-change set); `Conversation.md` CT-4 (the `:79` attribute is ratified and retained);
   `FolderHandling.md` FH-9 (optional stretch with an unverified
   `AddConversationBasedSuggestions` tolerance). Each is recorded as an accepted residual in
   `[P2-T10]`, `[P10-T18]`, `[P4-T5]` and `[P5-T13]` respectively.
4. **`EventWiring.md` EW-50..EW-53 are placed in a plain `[TestClass]` file, not `*.StaTests.cs`.**
   US-11 forbids creating an STA file, and the existing convention for headless real-`ItemViewer`
   tests in this project is a plain `[TestClass]`. The pre-existing convention gap is recorded for
   F1 rather than propagated.
5. **`LoadFolderHandlerAsync` structural drift.** A future #427 fix will change that method's line
   and branch sets. `[P5-T12]` records this; the plan does not pre-empt it.
