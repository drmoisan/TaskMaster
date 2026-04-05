<!-- markdownlint-disable-file -->

# Task Research Notes: UtilitiesCS below-threshold file coverage orchestration

## Research Executed

### File Analysis

- `.github/copilot-instructions.md`
  - Repository tone and project guidance require neutral, factual reporting and confirm instruction-file governance.
- `.github/instructions/csharp-code-change.instructions.md`
  - C# work must follow repo toolchain ordering and environment-appropriate commands; this research stays planning-only.
- `.github/instructions/csharp-unit-test.instructions.md`
  - Test planning must align to MSTest, Moq, and FluentAssertions conventions already used by the repository.
- `artifacts/orchestration/utilitiescs-coverage-inventory.md`
  - Verified the 89 ordered `UtilitiesCS` compiled production files currently below the coverage threshold.
- `artifacts/orchestration/utilitiescs-coverage-missing.md`
  - Confirmed that missing-report items outside the 89-file list are intentionally excluded from this plan.
- `docs/features/active/2026-03-19-utilities-coverage-part-three-87/v1/plan.2026-03-19T21-49.md`
  - Provided prior grouping intent, especially where earlier planning had already leaned toward implementation vs skip evaluation.
- `artifacts/research/20260319-utilities-coverage-part-three-87-research.md`
  - Supplied prior repository-specific findings that were cross-checked against current source and test layout.
- `UtilitiesCS.Test/UtilitiesCS.Test.csproj`
  - Confirmed `UtilitiesCS.Test` is an old-style explicit-include MSTest project, so any future implementation must register new test files explicitly if new files are created.
- Ordered direct reads of all 89 below-threshold production files under `UtilitiesCS\...`
  - Verified actual public surfaces, behavioral seams, UI/runtime coupling, and deterministic-test constraints for each file in the user-specified order.
- Broad direct reads and searches under `UtilitiesCS.Test/**/*.cs`
  - Confirmed many exact or adjacent test homes already exist and can be extended instead of creating new files.

### Code Search Results

- `UtilitiesCS.Test/**/*.cs`
  - Found exact or adjacent tests for dialogs, threading, file-system wrappers, Bayesian helpers, Outlook table helpers, locking linked-list types, async serialization, downloader, dispatch helpers, and configuration-adjacent classifier logic.
- `ManagerAsyncLazy|AsyncSerialization|DelegateButton|TimedDiskWriter|UiThread|ClassifierGroup|LockingObservableLinkedList|OlToDoTable|FilePathHelper`
  - Confirmed exact test homes including `UtilitiesCS.Test\Threading\UiThread_Tests.cs`, `UtilitiesCS.Test\HelperClasses\FilePathHelper_Tests.cs`, `UtilitiesCS.Test\Dialogs\DelegateButton_Tests.cs`, `UtilitiesCS.Test\Extensions\AsyncSerialization_Tests.cs`, `UtilitiesCS.Test\HelperClasses\TimedDiskWriterTests.cs`, `UtilitiesCS.Test\ReusableTypeClasses\LockingObservableLinkedList_Tests.cs`, `UtilitiesCS.Test\ReusableTypeClasses\LockingObservableLinkedListNode_Tests.cs`, `UtilitiesCS.Test\OutlookObjects\Table\OlToDoTable_Tests.cs`, and `UtilitiesCS.Test\EmailIntelligence\ClassifierGroups\Triage_Tests.cs` for `ManagerAsyncLazy` coverage.
- `LockingObservableLinkedList`
  - Confirmed both node-level and list-level exact tests already exist and cover the same type family targeted by the remaining below-threshold files.
- `OlToDoTable`
  - Confirmed exact Outlook table tests already exist and use the same class under review.
- `ManagerAsyncLazy`
  - Confirmed existing manager-focused scenarios already live inside `Triage_Tests.cs` and related classifier-group test files.

### External Research

- #githubRepo:"not executed - direct GitHub repository search tool was unavailable in this environment"
  - No repository-search result was recorded; external validation instead used official framework documentation fetched below.
- #fetch:https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-mstest-writing-tests-attributes
  - Verified MSTest test methods may return `void`, `Task`, or `ValueTask`; `async void` should not be used; `PrivateObject`/`PrivateType` remain available but public-surface testing is preferred.
- #fetch:https://fluentassertions.com/introduction
  - Verified FluentAssertions supports MSTest, favors readable chained assertions, and supports `AssertionScope` for grouped assertion reporting in richer deterministic scenarios.

### Project Conventions

- Standards referenced: `.github/copilot-instructions.md`, `.github/instructions/csharp-code-change.instructions.md`, `.github/instructions/csharp-unit-test.instructions.md`, `UtilitiesCS.Test/UtilitiesCS.Test.csproj`, `artifacts/orchestration/utilitiescs-coverage-inventory.md`, `artifacts/orchestration/utilitiescs-coverage-missing.md`
- Instructions followed: planning-only research, no source/config edits outside `artifacts/research/`, deterministic unit-test focus, preserve exact file order, prefer existing MSTest + Moq + FluentAssertions homes, identify concrete blockers where UI/COM/PInvoke/static-environment coupling materially reduces unit-test value.

## Key Discoveries

### Project Structure

`UtilitiesCS` mixes pure helper classes, thin wrappers, WinForms shells, Outlook COM helpers, Bayesian/classifier orchestration, and UI-thread/progress infrastructure. `UtilitiesCS.Test` already contains broad coverage scaffolding, but the project is explicit-include, so the lowest-friction path is to extend existing exact test files whenever they already exist. The remaining below-threshold set is not uniformly risky: many files are straightforward candidates for more deterministic unit tests, while a smaller subset are mainly designer shells or static environment/PInvoke surfaces where additional unit coverage is disproportionately expensive.

### Implementation Patterns

- Repository test style already favors MSTest with Moq and FluentAssertions.
- Outlook and COM-heavy code is often covered by interface or COM-object mocks rather than live Outlook automation.
- Several dialog and WinForms types are already tested by instantiating controls/forms directly on the test thread for state changes and event routing.
- Bayesian and classifier-group tests commonly use existing fake globals, mocked engine containers, and synthetic token/category inputs rather than production datasets.
- File-system wrappers, serialization helpers, and collections are already tested with deterministic temporary in-memory constructs or synthetic wrappers; when a type hard-codes static OS facilities with no seam, skip evaluation is more appropriate than forcing brittle tests.

### Complete Examples

```csharp
[TestClass]
public class ExampleAsyncCoverageTests
{
    [TestMethod]
    public async Task MethodUnderTest_WithDeterministicInputs_ReturnsExpectedState()
    {
        // Arrange
        var sut = new SomeType();

        // Act
        var result = await sut.DoAsync();

        // Assert
        using var scope = new FluentAssertions.Execution.AssertionScope();
        result.Should().NotBeNull();
        result.Should().Be("expected");
    }
}
```

### API and Schema Documentation

- MSTest requires `[TestClass]` and `[TestMethod]` for discovered tests; async tests should return `Task` rather than `async void`.
- Microsoft Learn still documents `PrivateObject` and `PrivateType`, but the preferred plan here is public-surface testing first and reflective access only for narrow static/private routing cases with no better seam.
- FluentAssertions supports MSTest and `AssertionScope`, which fits multi-property UI/helper assertions already common in this repository.

### Configuration Examples

```xml
<Compile Include="Dialogs\DelegateButton_Tests.cs" />
<Compile Include="Extensions\AsyncSerialization_Tests.cs" />
<Compile Include="HelperClasses\FilePathHelper_Tests.cs" />
<Compile Include="Threading\UiThread_Tests.cs" />
```

### Technical Requirements

- Deterministic only: no live Outlook profile, no live network, no shell execution, no registry mutation, no multi-monitor assumptions, and no temp-file dependence unless the existing test pattern already uses a safe deterministic seam.
- Prefer extending exact existing tests first; create a new test file only when no adjacent home exists.
- Favor pure behavioral assertions over private-method coverage. Use reflection only when a private/static branch is the only stable way to validate an otherwise public behavior contract.
- For WinForms controls, test state mutations and event routing, not designer rendering.
- For classifier/group orchestration, use mocked globals, fake loaders, and synthetic corpora/token sets rather than production data sources.

#### Ordered file research

1. `UtilitiesCS\Dialogs\FolderNotFoundViewer.cs`
   - Surface: `FolderAction`, `FolderName`, save/discard style button handlers that set action and hide the viewer.
   - Test home: nearest `UtilitiesCS.Test\Dialogs`.
   - Candidate scenarios: clicking each action button sets the expected action; folder-name text is surfaced correctly; viewer hides rather than disposing during action routing.
   - Seams/mocks/fakes: direct form instance; invoke click handlers on STA test thread.
   - Constraints/blockers: WinForms shell, but behavior is local and deterministic.
   - Recommended disposition: IMPLEMENT.

2. `UtilitiesCS\Dialogs\InputBox.cs`
   - Surface: static `ShowDialog(prompt, title, defaultResponse)` orchestration around `InputBoxViewer`.
   - Test home: `UtilitiesCS.Test\Dialogs\InputBox_Test.cs`.
   - Candidate scenarios: default response populates textbox state; OK returns entered text; cancel path returns `null`.
   - Seams/mocks/fakes: extend existing dialog test pattern; if modal display is hard to control, factor future tests around viewer state and return routing through existing dialog seams.
   - Constraints/blockers: modal dialog orchestration requires STA and careful noninteractive execution.
   - Recommended disposition: IMPLEMENT.

3. `UtilitiesCS\Dialogs\InputBoxViewer.cs`
   - Surface: `DpiAware`, static `DpiCalled`, `Ok_Click`, `Cancel_Click`.
   - Test home: nearest `UtilitiesCS.Test\Dialogs\InputBox_Test.cs`.
   - Candidate scenarios: OK copies textbox text to response and closes; cancel clears response; DPI flag toggles expected static/property state.
   - Seams/mocks/fakes: direct viewer instance on STA.
   - Constraints/blockers: static state must be reset per test.
   - Recommended disposition: IMPLEMENT.

4. `UtilitiesCS\Dialogs\MyBox.cs`
   - Surface: overloaded `ShowDialog` APIs, button replacement helpers, button/action translation, `FunctionButtonGroup<T>` routing.
   - Test home: nearest `UtilitiesCS.Test\Dialogs` plus existing `DelegateButton_Tests.cs`, `FunctionButton_Tests.cs`, and `YesNoToAll_Tests.cs`.
   - Candidate scenarios: button conversion preserves dialog result ordering; replacement logic swaps custom buttons into viewer; generic function-button group returns mapped value.
   - Seams/mocks/fakes: reuse existing dialog button templates and direct viewer instantiation.
   - Constraints/blockers: some overloads remain modal and UI-bound; prefer pure helper-path coverage.
   - Recommended disposition: IMPLEMENT.

5. `UtilitiesCS\Dialogs\NotImplementedDialog.cs`
   - Surface: `StopAtNotImplemented` and keep-running vs throw behavior around not-implemented prompts.
   - Test home: nearest `UtilitiesCS.Test\Dialogs`.
   - Candidate scenarios: when stop flag is enabled, the exception path is taken; when disabled, execution continues without throwing.
   - Seams/mocks/fakes: likely reflective access or wrapper around private routing; isolate static state reset.
   - Constraints/blockers: message-box interaction reduces value, but the control flag itself is deterministic.
   - Recommended disposition: IMPLEMENT.

6. `UtilitiesCS\EmailIntelligence\Bayesian\Performance\ConfusionViewer.cs`
   - Surface: constructor-only WinForms shell.
   - Test home: nearest `UtilitiesCS.Test\EmailIntelligence\Bayesian\BayesianPerformanceMeasurement_Tests.cs`.
   - Candidate scenarios: constructor smoke test only.
   - Seams/mocks/fakes: direct instantiation.
   - Constraints/blockers: no meaningful non-designer logic was found.
   - Recommended disposition: SKIP_EVALUATION.

7. `UtilitiesCS\EmailIntelligence\Bayesian\Performance\MetricChartViewer.cs`
   - Surface: constructor-only WinForms shell.
   - Test home: nearest `UtilitiesCS.Test\EmailIntelligence\Bayesian\BayesianPerformanceMeasurement_Tests.cs`.
   - Candidate scenarios: constructor smoke test only.
   - Seams/mocks/fakes: direct instantiation.
   - Constraints/blockers: no meaningful non-designer logic was found.
   - Recommended disposition: SKIP_EVALUATION.

8. `UtilitiesCS\EmailIntelligence\EmailParsingSorting\AutoFile.cs`
   - Surface: `AutoFindPeople`, `AreConversationsGrouped`, category-selection guard logic.
   - Test home: nearest `UtilitiesCS.Test\EmailIntelligence`.
   - Candidate scenarios: grouped-conversation detection respects category/state inputs; already-selected categories are not duplicated; person auto-selection path chooses expected candidates.
   - Seams/mocks/fakes: mocked mail/category inputs and synthetic collections.
   - Constraints/blockers: Outlook object seams must stay mocked.
   - Recommended disposition: IMPLEMENT.

9. `UtilitiesCS\EmailIntelligence\EmailParsingSorting\SortEmail.cs`
   - Surface: `SortAsync` overloads, `UpdatePredictiveEngineAsync`, `ProcessMailItemAsync`, explicit `NotImplementedException` path in `InitializeSortToExisting`.
   - Test home: nearest `UtilitiesCS.Test\EmailIntelligence`.
   - Candidate scenarios: not-implemented method throws; overloads delegate to the same core path; process method short-circuits correctly on empty or invalid inputs.
   - Seams/mocks/fakes: mocked globals, mocked engine manager, fake mail items.
   - Constraints/blockers: broader filing behavior is Outlook-heavy, but early routing is still unit-testable.
   - Recommended disposition: IMPLEMENT.

10. `UtilitiesCS\EmailIntelligence\OlFolderTools\FilterOlFolders\FilterOlFoldersController.cs`
    - Surface: ctor wiring, `Discard`, `Save`, property-changed synchronization, check-state getters/setters.
    - Test home: nearest `UtilitiesCS.Test\EmailIntelligence`.
    - Candidate scenarios: save/discard forwards to backing model; tree property changes update viewer-facing state; checked-state helpers round-trip expected values.
    - Seams/mocks/fakes: fake folder-tree/viewer objects with Moq.
    - Constraints/blockers: BrightIdeas tree UI should remain mocked.
    - Recommended disposition: IMPLEMENT.

11. `UtilitiesCS\EmailIntelligence\OlFolderTools\FilterOlFolders\FilterOlFoldersViewer.cs`
    - Surface: `SetController`, tree/renderer setup, `FormatFileSize`, save/discard event forwarding.
    - Test home: nearest `UtilitiesCS.Test\EmailIntelligence`.
    - Candidate scenarios: controller wiring registers delegates; file-size formatting returns expected string cases; viewer buttons call controller methods.
    - Seams/mocks/fakes: mocked controller, direct viewer instance.
    - Constraints/blockers: renderer wiring depends on WinForms tree components but remains deterministic.
    - Recommended disposition: IMPLEMENT.

12. `UtilitiesCS\EmailIntelligence\OlFolderTools\FilterOlFolders\FolderInfoViewer.cs`
    - Surface: `FolderTree` property and `SetFolderTree` assignment.
    - Test home: nearest `UtilitiesCS.Test\EmailIntelligence`.
    - Candidate scenarios: setter updates the exposed tree reference; repeated assignment replaces prior reference.
    - Seams/mocks/fakes: direct viewer instance.
    - Constraints/blockers: logic is trivial but deterministic.
    - Recommended disposition: IMPLEMENT.

13. `UtilitiesCS\EmailIntelligence\OlFolderTools\FilterOlFolders\OSBrowser.cs`
    - Surface: drag/drop setup, tree/column setup, `FormatFileSize`.
    - Test home: nearest `UtilitiesCS.Test\EmailIntelligence`.
    - Candidate scenarios: setup methods initialize expected columns/tree options; file-size formatting covers bytes/KB/MB boundaries.
    - Seams/mocks/fakes: direct form/control instance.
    - Constraints/blockers: UI-only wiring, but still deterministic.
    - Recommended disposition: IMPLEMENT.

14. `UtilitiesCS\EmailIntelligence\OlFolderTools\FolderRemap\FolderRemapController.cs`
    - Surface: mapping synchronization, save/discard, drag/drop handlers, check-state handling, `ExpandTo`, `SyncGlobalMap`.
    - Test home: nearest `UtilitiesCS.Test\EmailIntelligence`.
    - Candidate scenarios: drag/drop updates mapping; save/discard reflects model state; `ExpandTo` selects the right node path.
    - Seams/mocks/fakes: fake remap tree/viewer objects; synthetic folder nodes.
    - Constraints/blockers: tree-node model must be faked; no live Outlook store needed.
    - Recommended disposition: IMPLEMENT.

15. `UtilitiesCS\EmailIntelligence\OlFolderTools\FolderRemap\FolderRemapViewer.cs`
    - Surface: controller binding, renderer/tree setup, file-size formatting, drag/drop event forwarding.
    - Test home: nearest `UtilitiesCS.Test\EmailIntelligence`.
    - Candidate scenarios: viewer forwards drag/drop to controller; setup methods establish expected state; file-size formatting is stable.
    - Seams/mocks/fakes: mocked controller; direct viewer instantiation.
    - Constraints/blockers: WinForms/BrightIdeas dependency only.
    - Recommended disposition: IMPLEMENT.

16. `UtilitiesCS\EmailIntelligence\OlFolderTools\FolderRemap\FolderSelector.cs`
    - Surface: static `SelectFolder`, initialization, `Selection` state.
    - Test home: nearest `UtilitiesCS.Test\EmailIntelligence`.
    - Candidate scenarios: initialization sets selection source; completed selection returns chosen node/folder; null/empty inputs return no selection.
    - Seams/mocks/fakes: fake folder tree and selector dialog state.
    - Constraints/blockers: modal selection path is UI-bound, so tests should focus on pre/post state rather than display.
    - Recommended disposition: IMPLEMENT.

17. `UtilitiesCS\EmailIntelligence\SubjectMap\SubjectMapEncoder.cs`
    - Surface: lazy `Encoder`/`Decoder`, `RebuildEncoding`, `AugmentTokenDict`, `Encode`, `Decode`.
    - Test home: nearest `UtilitiesCS.Test\EmailIntelligence`.
    - Candidate scenarios: rebuilding yields symmetric encode/decode maps; augmenting appends unseen tokens only; decode of encoded terms round-trips.
    - Seams/mocks/fakes: pure in-memory token dictionaries.
    - Constraints/blockers: none beyond keeping token order deterministic.
    - Recommended disposition: IMPLEMENT.

18. `UtilitiesCS\EmailIntelligence\SubjectMap\SubjectMapMetrics.cs`
    - Surface: constructors and metric binding into `DlvMetrics`.
    - Test home: nearest `UtilitiesCS.Test\EmailIntelligence`.
    - Candidate scenarios: constructor projections copy expected counts and rates; alternate constructor overloads stay equivalent.
    - Seams/mocks/fakes: pure in-memory metric inputs.
    - Constraints/blockers: none.
    - Recommended disposition: IMPLEMENT.

19. `UtilitiesCS\Extensions\DfDeedle.cs`
    - Surface: record/DataFrame conversions, date extraction, triage filtering, QFC column augmentation.
    - Test home: nearest `UtilitiesCS.Test\Extensions`.
    - Candidate scenarios: 2D email arrays convert to expected row/column layout; invalid triage values are filtered; date parsing handles null and invalid slots.
    - Seams/mocks/fakes: pure in-memory arrays/frames.
    - Constraints/blockers: Deedle frame construction must stay small and deterministic.
    - Recommended disposition: IMPLEMENT.

20. `UtilitiesCS\HelperClasses\DvgForm.cs`
    - Surface: resize-end behavior on the backing grid form.
    - Test home: nearest `UtilitiesCS.Test\HelperClasses\WindowsForms\ScreenAndTableLayoutTests.cs`.
    - Candidate scenarios: resize-end invokes expected layout behavior without throwing.
    - Seams/mocks/fakes: direct form instance.
    - Constraints/blockers: limited business logic, but more than a pure constructor shell.
    - Recommended disposition: IMPLEMENT.

21. `UtilitiesCS\HelperClasses\ToolTips\QfcTipsDetails.cs`
    - Surface: constructors, `CreateAsync`, parent-type resolution, `InitializeAsync`, multiple toggle/state properties.
    - Test home: nearest `UtilitiesCS.Test\HelperClasses`.
    - Candidate scenarios: parent-type resolution returns expected enum/type; initialization populates expected labels/toggles; visibility toggles update internal state consistently.
    - Seams/mocks/fakes: direct control/view model instances with mocked dependencies.
    - Constraints/blockers: significant UI state surface, but still deterministic if tested as control state rather than rendering.
    - Recommended disposition: IMPLEMENT.

22. `UtilitiesCS\HelperClasses\ToolTips\TipsController.cs`
    - Surface: label initialization, parent resolution, toggle methods, `ToggleColumnOnly`.
    - Test home: nearest `UtilitiesCS.Test\HelperClasses`.
    - Candidate scenarios: label setup reflects details state; toggle methods switch only the intended columns/sections; repeated toggles are idempotent.
    - Seams/mocks/fakes: fake detail controls.
    - Constraints/blockers: none beyond UI state management.
    - Recommended disposition: IMPLEMENT.

23. `UtilitiesCS\HelperClasses\Windows Forms\OlvExtension.cs`
    - Surface: `AutoScaleColumnsToContainer`.
    - Test home: nearest `UtilitiesCS.Test\HelperClasses\WindowsForms\ScreenAndTableLayoutTests.cs`.
    - Candidate scenarios: columns expand proportionally to container width; empty/no-column lists are no-ops.
    - Seams/mocks/fakes: test `ObjectListView` or light fake column collection.
    - Constraints/blockers: BrightIdeas dependency must remain local, not interactive.
    - Recommended disposition: IMPLEMENT.

24. `UtilitiesCS\ReusableTypeClasses\NewSmartSerializable\Config\ConfigGroupBox.cs`
    - Surface: wrapper properties exposing disk selection, textbox/combobox/label state.
    - Test home: nearest `UtilitiesCS.Test\ReusableTypeClasses`.
    - Candidate scenarios: wrapper getters/setters stay synchronized with child controls; active-disk selection maps correctly.
    - Seams/mocks/fakes: direct control instance.
    - Constraints/blockers: UI-wrapper only, but deterministic.
    - Recommended disposition: IMPLEMENT.

25. `UtilitiesCS\ReusableTypeClasses\NewSmartSerializable\Config\ConfigViewer.cs`
    - Surface: config-group setup, controller binding, save/cancel/open handlers, box activation logic.
    - Test home: nearest `UtilitiesCS.Test\ReusableTypeClasses`.
    - Candidate scenarios: save/cancel route to controller; disk group activation toggles the right controls; opening configuration respects current active group.
    - Seams/mocks/fakes: mocked `ConfigController`, direct viewer instance.
    - Constraints/blockers: WinForms state only.
    - Recommended disposition: IMPLEMENT.

26. `UtilitiesCS\Threading\IdleActionQueue.cs`
    - Surface: static queue registration, `AddEntry`, idle callback, unsubscribe timer.
    - Test home: nearest `UtilitiesCS.Test\Threading\ApplicationIdleTimer_Tests.cs`.
    - Candidate scenarios: first enqueue initializes queue; idle callback drains queued work in order; unsubscribe clears callback after inactivity.
    - Seams/mocks/fakes: reflective access to static queue state or wrapper delegates.
    - Constraints/blockers: `Application.Idle` static event coupling makes tests more invasive but still possible.
    - Recommended disposition: IMPLEMENT.

27. `UtilitiesCS\Threading\IdleAsyncQueue.cs`
    - Surface: async idle-queue variant with UI-thread option.
    - Test home: nearest `UtilitiesCS.Test\Threading\ApplicationIdleTimer_Tests.cs`.
    - Candidate scenarios: queued async work runs once; UI-thread flag routes through the expected scheduling path; exceptions do not break later items.
    - Seams/mocks/fakes: fake queued tasks, reflective static reset.
    - Constraints/blockers: same static idle coupling as `IdleActionQueue`.
    - Recommended disposition: IMPLEMENT.

28. `UtilitiesCS\Threading\ProgressMultiStepViewer.cs`
    - Surface: constructor-only progress form shell.
    - Test home: nearest `UtilitiesCS.Test\Threading`.
    - Candidate scenarios: constructor smoke test only.
    - Seams/mocks/fakes: direct instantiation.
    - Constraints/blockers: no meaningful non-designer logic was found.
    - Recommended disposition: SKIP_EVALUATION.

29. `UtilitiesCS\Threading\ProgressPane.cs`
    - Surface: pane setup, UI dispatcher/sync context/scheduler exposure, cancellation wiring.
    - Test home: nearest `UtilitiesCS.Test\Threading\ProgressTracker_Tests.cs`.
    - Candidate scenarios: initialization captures UI scheduler/context; cancellation token source is honored; visible/report state changes occur as expected.
    - Seams/mocks/fakes: direct control instance with fake sync context.
    - Constraints/blockers: UI-thread assumptions require controlled synchronization context.
    - Recommended disposition: IMPLEMENT.

30. `UtilitiesCS\Threading\ProgressViewer.cs`
    - Surface: form setup, cancellation wiring, UI-thread context properties.
    - Test home: nearest `UtilitiesCS.Test\Threading\ProgressTracker_Tests.cs`.
    - Candidate scenarios: cancel button trips the token source; exposed sync context and dispatcher values are populated after initialization.
    - Seams/mocks/fakes: direct form instance.
    - Constraints/blockers: UI initialization order must be controlled.
    - Recommended disposition: IMPLEMENT.

31. `UtilitiesCS\Threading\ThreadMonitor.cs`
    - Surface: `Run` and obsolete thread suspend/resume stack-trace capture flow.
    - Test home: nearest `UtilitiesCS.Test\Threading`.
    - Candidate scenarios: constructor stores threshold/state only.
    - Seams/mocks/fakes: none attractive for full runtime behavior.
    - Constraints/blockers: relies on obsolete `Thread.Suspend`/`Thread.Resume` style behavior and timing-sensitive diagnostics; brittle and unsafe for deterministic unit tests.
    - Recommended disposition: SKIP_EVALUATION.

32. `UtilitiesCS\To Depricate\CSVDictUtilities.cs`
    - Surface: `LoadDictCSV`, `WriteDictCSV`.
    - Test home: nearest `UtilitiesCS.Test\HelperClasses\FilePathHelper_Tests.cs`.
    - Candidate scenarios: parse and serialize simple CSV dictionaries.
    - Seams/mocks/fakes: would require real disk I/O because the static file APIs are hard-coded.
    - Constraints/blockers: deprecated utility with no injection seam and direct file-system dependence.
    - Recommended disposition: SKIP_EVALUATION.

33. `UtilitiesCS\To Depricate\FileIO2.cs`
    - Surface: text/CSV read-write helpers and 2D/jagged splitting.
    - Test home: nearest `UtilitiesCS.Test\HelperClasses\FilePathHelper_Tests.cs`.
    - Candidate scenarios: split helpers and text round-trip logic.
    - Seams/mocks/fakes: pure split helpers are testable, but main public usage paths are direct static file I/O.
    - Constraints/blockers: deprecated file helper with no seam; low-value unit coverage unless implementation work first introduces abstraction.
    - Recommended disposition: SKIP_EVALUATION.

34. `UtilitiesCS\EmailIntelligence\EmailParsingSorting\EmailDataMiner.cs`
    - Surface: mining orchestration, staging deletion, folder-tree extraction, chunking/staging flows.
    - Test home: nearest `UtilitiesCS.Test\EmailIntelligence`.
    - Candidate scenarios: empty source returns no mined rows; chunking groups expected counts; staging-delete routine short-circuits missing paths.
    - Seams/mocks/fakes: fake folder trees, mocked globals, synthetic file/path wrappers.
    - Constraints/blockers: avoid real Outlook and disk staging; cover only orchestration branches with seams.
    - Recommended disposition: IMPLEMENT.

35. `UtilitiesCS\HelperClasses\Windows Forms\ScreenHelper.cs`
    - Surface: screen lookup, toggling, switching, multi-screen helpers.
    - Test home: nearest `UtilitiesCS.Test\HelperClasses\WindowsForms\ScreenAndTableLayoutTests.cs`.
    - Candidate scenarios: only defensive null/lookup failure branches are safely unit-testable.
    - Seams/mocks/fakes: none for static `Screen.AllScreens` without environment coupling.
    - Constraints/blockers: behavior depends on actual machine monitor topology and active forms.
    - Recommended disposition: SKIP_EVALUATION.

36. `UtilitiesCS\EmailIntelligence\SubjectMap\SubjectMapSco.cs`
    - Surface: tokenizer regex setup, encode-all/add/find/repair/query helpers, rebuild/repopulate flows, summary metrics.
    - Test home: nearest `UtilitiesCS.Test\EmailIntelligence`.
    - Candidate scenarios: adding tokens updates lookup counts; `TryRepair` fixes recoverable missing encodings; query helpers return deterministic matches.
    - Seams/mocks/fakes: pure in-memory subject maps/token sets.
    - Constraints/blockers: keep tests focused on data logic, not viewer/reporting flows.
    - Recommended disposition: IMPLEMENT.

37. `UtilitiesCS\HelperClasses\ThemeHelpers\Theme.cs`
    - Surface: very large theme object spanning WinForms controls, color sets, alternate/hover state, and WebView-related values.
    - Test home: nearest `UtilitiesCS.Test\HelperClasses`.
    - Candidate scenarios: constructor/default-value smoke tests only.
    - Seams/mocks/fakes: direct object instantiation.
    - Constraints/blockers: broad UI/control graph and large mutable surface make meaningful unit coverage low-value compared with narrower `ThemeControlGroup` behavior.
    - Recommended disposition: SKIP_EVALUATION.

38. `UtilitiesCS\EmailIntelligence\IntelligenceConfig.cs`
    - Surface: `LoadAsync`, `InitAsync`, configuration read/write, loader property-changed routing, type discrimination.
    - Test home: nearest `UtilitiesCS.Test\EmailIntelligence`.
    - Candidate scenarios: derived-type detection matches expected classifier types; property changes trigger write path; missing config data initializes defaults.
    - Seams/mocks/fakes: mocked globals/loaders, synthetic serialized config strings.
    - Constraints/blockers: serialization side effects should be routed through mocked loaders rather than real files.
    - Recommended disposition: IMPLEMENT.

39. `UtilitiesCS\EmailIntelligence\EmailParsingSorting\EmailFiler.cs`
    - Surface: folder opening, sort overloads, process helpers, undo stack capture, label/training helpers, attachment/message save helpers.
    - Test home: nearest `UtilitiesCS.Test\EmailIntelligence`.
    - Candidate scenarios: open-folder helpers short-circuit invalid paths; tab/CRLF stripping is deterministic; undo-stack capture records move details correctly.
    - Seams/mocks/fakes: mocked Outlook wrappers, fake file-path helpers, synthetic mail items.
    - Constraints/blockers: broader file/move/save paths are Outlook and disk heavy; prefer helper-path coverage first.
    - Recommended disposition: IMPLEMENT.

40. `UtilitiesCS\ReusableTypeClasses\NewSmartSerializable\Config\ConfigController.cs`
    - Surface: show/init wiring, `Cancel`, `ChangeSpecialFolder`, `ActivateDiskGroup`, not-implemented file chooser, `SaveAsync`.
    - Test home: nearest `UtilitiesCS.Test\ReusableTypeClasses`.
    - Candidate scenarios: activating local/network disk toggles target group; cancel restores prior state; unimplemented chooser path throws or no-ops as coded.
    - Seams/mocks/fakes: mocked viewer/config model.
    - Constraints/blockers: no live file-picker interaction should be tested.
    - Recommended disposition: IMPLEMENT.

41. `UtilitiesCS\Threading\AsyncMultiTasker.cs`
    - Surface: chunking helpers across async/sync functions with progress and timing.
    - Test home: nearest `UtilitiesCS.Test\Threading`.
    - Candidate scenarios: chunk size partitions inputs correctly; async and sync overloads preserve result order/count; progress callback receives terminal completion.
    - Seams/mocks/fakes: pure in-memory delegates and inputs.
    - Constraints/blockers: avoid timing assertions; assert outputs only.
    - Recommended disposition: IMPLEMENT.

42. `UtilitiesCS\EmailIntelligence\OlFolderTools\FolderRemap\FolderRemapTree.cs`
    - Surface: remap-tree build/filter/notification logic and nested `OlFolderRemap`.
    - Test home: nearest `UtilitiesCS.Test\EmailIntelligence`.
    - Candidate scenarios: building from a mapping source yields expected nodes; filter removes excluded nodes; notifications fire on map updates.
    - Seams/mocks/fakes: synthetic folder hierarchy inputs.
    - Constraints/blockers: none if tree is exercised as data, not UI rendering.
    - Recommended disposition: IMPLEMENT.

43. `UtilitiesCS\EmailIntelligence\ClassifierGroups\ClassifierGroupUtilities.cs`
    - Surface: create/get classifier group, serialize/deserialize helpers, path-based persistence and diagnostics.
    - Test home: `UtilitiesCS.Test\EmailIntelligence\ClassifierGroups\ClassifierGroupUtilities_Tests.cs`.
    - Candidate scenarios: existing loader path resolves to the right group; missing config returns fallback/new classifier; serialize/deserialize helper preserves expected config fields.
    - Seams/mocks/fakes: mocked globals/loaders and in-memory serialized payloads.
    - Constraints/blockers: keep persistence mocked rather than file-backed.
    - Recommended disposition: IMPLEMENT.

44. `UtilitiesCS\EmailIntelligence\People\PeopleScoDictionaryNew.cs`
    - Surface: people/category matching, add/update flows, prompt-assisted category prefix logic.
    - Test home: `UtilitiesCS.Test\EmailIntelligence\PeopleScoDictionaryNew_Tests.cs`.
    - Candidate scenarios: matching prefers exact names/categories; add flow applies prefix rules; duplicate additions are ignored or merged as coded.
    - Seams/mocks/fakes: synthetic people/category values; mock prompt callbacks where needed.
    - Constraints/blockers: prompt paths should be abstracted or bypassed in favor of pure branch coverage.
    - Recommended disposition: IMPLEMENT.

45. `UtilitiesCS\ReusableTypeClasses\Serializable\Concurrent\SCO\SCODictionary.cs`
    - Surface: constructors, file-path properties, serialize/deserialize, backup loading, message-box error handling.
    - Test home: nearest `UtilitiesCS.Test\ReusableTypeClasses`.
    - Candidate scenarios: deserialize missing path returns empty/new object; backup loader selection prefers expected source; request-serialize path respects configuration state.
    - Seams/mocks/fakes: synthetic serialized strings or fake loaders.
    - Constraints/blockers: direct message-box and file-system side effects should be bypassed or kept to defensive branches.
    - Recommended disposition: IMPLEMENT.

46. `UtilitiesCS\HelperClasses\FileSystem\FileInfoWrapper.cs`
    - Surface: wrapper over `FileInfo` properties and methods.
    - Test home: `UtilitiesCS.Test\HelperClasses\FileInfoWrapper_Tests.cs`.
    - Candidate scenarios: wrapper forwards `Exists`, name/path properties, and method calls to the inner `FileInfo`; null inner info is handled as coded.
    - Seams/mocks/fakes: use the repository’s existing wrapper test pattern.
    - Constraints/blockers: none.
    - Recommended disposition: IMPLEMENT.

47. `UtilitiesCS\HelperClasses\FileSystem\DirectoryInfoWrapper.cs`
    - Surface: wrapper over `DirectoryInfo` properties and methods.
    - Test home: `UtilitiesCS.Test\HelperClasses\DirectoryInfoWrapper_Tests.cs`.
    - Candidate scenarios: wrapper forwards directory name/full-path/exists and selected method calls.
    - Seams/mocks/fakes: existing wrapper test pattern.
    - Constraints/blockers: none.
    - Recommended disposition: IMPLEMENT.

48. `UtilitiesCS\Extensions\DfMLNet.cs`
    - Surface: `ToDataFrame`, column/name/type helpers, `ToDataTable`, `Display`.
    - Test home: nearest `UtilitiesCS.Test\Extensions`.
    - Candidate scenarios: object sequences convert to expected columns/types; first-non-null selection works; data-table conversion preserves row count.
    - Seams/mocks/fakes: pure in-memory collections.
    - Constraints/blockers: ML.NET `DataFrame` construction should stay minimal.
    - Recommended disposition: IMPLEMENT.

49. `UtilitiesCS\HelperClasses\Windows Forms\TableLayoutHelper.cs`
    - Surface: row/column insert/remove helpers with invoke behavior.
    - Test home: `UtilitiesCS.Test\HelperClasses\WindowsForms\ScreenAndTableLayoutTests.cs`.
    - Candidate scenarios: adding/removing rows and columns updates count and control positions; invoke branch works when called from the owning thread.
    - Seams/mocks/fakes: direct `TableLayoutPanel` instances.
    - Constraints/blockers: avoid cross-thread timing; stay on same thread.
    - Recommended disposition: IMPLEMENT.

50. `UtilitiesCS\EmailIntelligence\ClassifierGroups\SpamBayes\SpamBayes.cs`
    - Surface: create/init/validation/missing-handler flows, classifier creation, prompt/config dependencies.
    - Test home: nearest `UtilitiesCS.Test\EmailIntelligence\ClassifierGroups\ClassifierGroups_Tests.cs`.
    - Candidate scenarios: create-new path returns configured classifier group; missing configuration invokes fallback handling; validation rejects incomplete setup.
    - Seams/mocks/fakes: mocked globals, loaders, manager, and prompt responses.
    - Constraints/blockers: interactive prompt branches must remain mocked.
    - Recommended disposition: IMPLEMENT.

51. `UtilitiesCS\ReusableTypeClasses\Serializable\Concurrent\ScBag.cs`
    - Surface: deserialize/create-empty/ask-user/serialize/request-serialization.
    - Test home: nearest `UtilitiesCS.Test\ReusableTypeClasses`.
    - Candidate scenarios: deserialize missing content creates empty bag; request-serialization routes only when configured; ask-user branch handles cancellation/default.
    - Seams/mocks/fakes: fake loaders and serialized payloads.
    - Constraints/blockers: prompt/file interactions should stay stubbed.
    - Recommended disposition: IMPLEMENT.

52. `UtilitiesCS\EmailIntelligence\Bayesian\CorpusInherit.cs`
    - Surface: corpus dictionary plus increment/decrement, deserialize/create-empty/serialize patterns.
    - Test home: `UtilitiesCS.Test\EmailIntelligence\Bayesian\CorpusInherit_Tests.cs`.
    - Candidate scenarios: increment/decrement adjusts counts correctly; empty deserialize returns initialized corpus; serialization preserves token frequency map.
    - Seams/mocks/fakes: pure in-memory corpus.
    - Constraints/blockers: none.
    - Recommended disposition: IMPLEMENT.

53. `UtilitiesCS\Dialogs\FunctionButton.cs`
    - Surface: constructor overloads, sync/async delegate routing, event-hook logic, `FromButton`, `Button` property behavior.
    - Test home: `UtilitiesCS.Test\Dialogs\FunctionButton_Tests.cs`.
    - Candidate scenarios: each constructor preserves metadata and delegate; button reassignment unwires old click handler; async callback executes once.
    - Seams/mocks/fakes: direct `Button` instances.
    - Constraints/blockers: async delegates should use `TaskCompletionSource` rather than sleeps.
    - Recommended disposition: IMPLEMENT.

54. `UtilitiesCS\Dialogs\MyBoxViewer.cs`
    - Surface: constructors, button delegate invocation, standard-button removal, size calculations, textbox growth.
    - Test home: nearest `UtilitiesCS.Test\Dialogs`.
    - Candidate scenarios: custom buttons invoke mapped delegate; removing standard buttons leaves only custom controls; text changes trigger growth/min-size recalculation.
    - Seams/mocks/fakes: direct viewer instance and synthetic buttons.
    - Constraints/blockers: form sizing can be asserted relatively, not pixel-perfect.
    - Recommended disposition: IMPLEMENT.

55. `UtilitiesCS\Dialogs\YesNoToAll.cs`
    - Surface: response enum/state setters and `ShowDialog`.
    - Test home: `UtilitiesCS.Test\Dialogs\YesNoToAll_Tests.cs`.
    - Candidate scenarios: each responder sets the expected enum; dialog result reflects current state.
    - Seams/mocks/fakes: direct dialog instance.
    - Constraints/blockers: modal path should be minimized.
    - Recommended disposition: IMPLEMENT.

56. `UtilitiesCS\EmailIntelligence\ClassifierGroups\Categories\CategoryClassifierGroup.cs`
    - Surface: engine init/build/load/expand/build-classifiers flows.
    - Test home: nearest `UtilitiesCS.Test\EmailIntelligence\ClassifierGroups\ClassifierGroups_Tests.cs`.
    - Candidate scenarios: category expansion creates expected classifier keys; build path skips empty categories; load path reuses existing manager entries.
    - Seams/mocks/fakes: mocked globals/manager and synthetic categories.
    - Constraints/blockers: none beyond keeping classifier data synthetic.
    - Recommended disposition: IMPLEMENT.

57. `UtilitiesCS\HelperClasses\Windows Forms\MouseDownFilter.cs`
    - Surface: `IMessageFilter` implementation, `FormClicked`, `PreFilterMessage`.
    - Test home: nearest `UtilitiesCS.Test\Extensions\WinFormsExtensions_Tests.cs`.
    - Candidate scenarios: left-button messages trigger the event; unrelated messages return false without raising; null subscribers are safe.
    - Seams/mocks/fakes: synthetic `Message` values.
    - Constraints/blockers: none.
    - Recommended disposition: IMPLEMENT.

58. `UtilitiesCS\HelperClasses\FileSystem\ShellUtilities.cs`
    - Surface: shell execute, file-type/icon/system-image helpers via PInvoke.
    - Test home: `UtilitiesCS.Test\HelperClasses\ShellUtilities_Tests.cs`.
    - Candidate scenarios: defensive argument validation only.
    - Seams/mocks/fakes: none for meaningful shell/PInvoke behavior without OS dependence.
    - Constraints/blockers: static Win32 shell interop and icon extraction are environment-dependent and brittle in unit tests.
    - Recommended disposition: SKIP_EVALUATION.

59. `UtilitiesCS\HelperClasses\FileSystem\ShellUtilitiesStatic.cs`
    - Surface: static shell helper equivalents of the same PInvoke behaviors.
    - Test home: `UtilitiesCS.Test\HelperClasses\ShellUtilities_Tests.cs`.
    - Candidate scenarios: defensive argument validation only.
    - Seams/mocks/fakes: none attractive for system-image and shell execution behavior.
    - Constraints/blockers: same Win32 shell dependence as `ShellUtilities.cs`.
    - Recommended disposition: SKIP_EVALUATION.

60. `UtilitiesCS\HelperClasses\ThemeHelpers\ThemeControlGroup.cs`
    - Surface: constructors, `ApplyTheme`, alternate/hover/object-setter and WebView2-aware logic.
    - Test home: nearest `UtilitiesCS.Test\HelperClasses`.
    - Candidate scenarios: applying a theme updates supported control properties; alternate/hover setters target the intended control set; unsupported controls are ignored safely.
    - Seams/mocks/fakes: simple WinForms controls, avoid WebView2-specific runtime assertions unless already available in tests.
    - Constraints/blockers: broad control surface, but materially more unit-testable than `Theme.cs` itself.
    - Recommended disposition: IMPLEMENT.

61. `UtilitiesCS\OutlookObjects\Table\OlTableExtensions.cs`
    - Surface: column dictionary helpers, retry helper, add/remove columns, ETL/extract methods.
    - Test home: `UtilitiesCS.Test\OutlookObjects\Table\OlTableExtensions_Tests.cs`.
    - Candidate scenarios: column-add/remove calls expected table members; retry wrapper retries transient failures the expected number of times; extract helpers map mock rows to expected records.
    - Seams/mocks/fakes: Moq COM table/columns/rows.
    - Constraints/blockers: keep all Outlook table behavior mocked.
    - Recommended disposition: IMPLEMENT.

62. `UtilitiesCS\Threading\ProgressTrackerAsync.cs`
    - Surface: async initialization and progress/viewer allocation properties.
    - Test home: `UtilitiesCS.Test\Threading\ProgressTrackerAsync_Tests.cs`.
    - Candidate scenarios: initialize populates root state; report updates percentage/message; child allocation inherits expected scheduler/token state.
    - Seams/mocks/fakes: direct tracker with stub viewer.
    - Constraints/blockers: async completion should use awaited tasks, not delays.
    - Recommended disposition: IMPLEMENT.

63. `UtilitiesCS\Extensions\WinFormsExtensions.cs`
    - Surface: control traversal, ancestor lookup, event-list helpers, `RemoveEventHandlers`.
    - Test home: `UtilitiesCS.Test\Extensions\WinFormsExtensions_Tests.cs`.
    - Candidate scenarios: tree traversal returns descendants in expected order; ancestor lookup handles missing parents; removing handlers prevents later invocation.
    - Seams/mocks/fakes: direct control trees and synthetic events.
    - Constraints/blockers: event removal assertions should avoid reflection-heavy platform assumptions beyond existing test patterns.
    - Recommended disposition: IMPLEMENT.

64. `UtilitiesCS\EmailIntelligence\ClassifierGroups\MulticlassEngine.cs`
    - Surface: generic engine init/build/progress/classifier loading flows.
    - Test home: `UtilitiesCS.Test\EmailIntelligence\ClassifierGroups\MulticlassEngine_Tests.cs`.
    - Candidate scenarios: init wires manager and globals; build path creates expected classifier count; load path short-circuits missing manager entries.
    - Seams/mocks/fakes: mocked globals, manager, classifier loaders.
    - Constraints/blockers: generic type parameter should be exercised with a small fake engine subclass.
    - Recommended disposition: IMPLEMENT.

65. `UtilitiesCS\EmailIntelligence\ClassifierGroups\Triage\Triage.cs`
    - Surface: create/init/validation/missing-handler/classification/training flows.
    - Test home: `UtilitiesCS.Test\EmailIntelligence\ClassifierGroups\Triage_Tests.cs`.
    - Candidate scenarios: create-new triage classifier sets expected config file name; validation rejects missing classifier group; training/classification routes through manager as expected.
    - Seams/mocks/fakes: mocked globals and manager.
    - Constraints/blockers: none; exact tests already exist.
    - Recommended disposition: IMPLEMENT.

66. `UtilitiesCS\Threading\ProgressTrackerPane.cs`
    - Surface: pane-backed hierarchical progress tracking with root/child spawn logic.
    - Test home: nearest `UtilitiesCS.Test\Threading\ProgressTracker_Tests.cs`.
    - Candidate scenarios: root tracker reports progress to pane; spawned child inherits scaled range; completion closes or updates pane correctly.
    - Seams/mocks/fakes: stub pane implementation or direct pane control.
    - Constraints/blockers: UI-thread assumptions must stay controlled.
    - Recommended disposition: IMPLEMENT.

67. `UtilitiesCS\EmailIntelligence\ClassifierGroups\OlFolder\OlFolderClassifierGroup.cs`
    - Surface: staging load, classifier-group build, folder classifier construction.
    - Test home: nearest `UtilitiesCS.Test\EmailIntelligence\ClassifierGroups\ClassifierGroups_Tests.cs`.
    - Candidate scenarios: build path creates classifier per eligible folder; empty staging source yields no classifiers; load path rehydrates existing group.
    - Seams/mocks/fakes: synthetic folder metadata and mocked globals.
    - Constraints/blockers: no live Outlook folder traversal.
    - Recommended disposition: IMPLEMENT.

68. `UtilitiesCS\Threading\ApplicationIdleTimer.cs`
    - Surface: idle event args, subscribe/unsubscribe, heartbeat, CPU/GUI activity computation, singleton timer.
    - Test home: `UtilitiesCS.Test\Threading\ApplicationIdleTimer_Tests.cs`.
    - Candidate scenarios: subscribe/unsubscribe changes listener count; heartbeat raises expected event args; singleton instance is reused.
    - Seams/mocks/fakes: extend existing idle timer tests and fake timer wrappers where possible.
    - Constraints/blockers: some CPU/input sampling is environment-sensitive, so cover deterministic wrapper logic rather than absolute idle timing.
    - Recommended disposition: IMPLEMENT.

69. `UtilitiesCS\EmailIntelligence\Recents\RecentsList.cs`
    - Surface: recent-list insertion ordering, deduplication, max-size trimming.
    - Test home: `UtilitiesCS.Test\EmailIntelligence\RecentsList_Tests.cs`.
    - Candidate scenarios: repeated add moves existing item to front; max count trims oldest; serialization order remains stable.
    - Seams/mocks/fakes: pure in-memory list.
    - Constraints/blockers: none.
    - Recommended disposition: IMPLEMENT.

70. `UtilitiesCS\OneDriveHelpers\OneDriveDownloader.cs`
    - Surface: `TryGetUrlStreamAsync`, `DownloadFileAsync`, `TryGetFileStreamWriter`, injected `ClientGetAsync` and writer factory.
    - Test home: `UtilitiesCS.Test\OneDriveHelpers\OneDriveDownloader_Tests.cs`.
    - Candidate scenarios: successful download writes stream contents via injected writer; missing writer fails gracefully; failed client call returns false without file output.
    - Seams/mocks/fakes: injected HTTP delegate and in-memory destination stream.
    - Constraints/blockers: none; the file already exposes the right seams.
    - Recommended disposition: IMPLEMENT.

71. `UtilitiesCS\EmailIntelligence\ClassifierGroups\ManagerAsyncLazy.cs`
    - Surface: configuration loading/reset, loader property-changed handling, write-back, classifier lazy-loader creation, restart/remove behavior.
    - Test home: `UtilitiesCS.Test\EmailIntelligence\ClassifierGroups\Triage_Tests.cs` and adjacent classifier-group tests.
    - Candidate scenarios: `ResetConfigAsyncLazy` recreates configuration task; inactive loader removal drops engine entry; `GetAsyncLazyClassifierLoader` attaches config-change handler and uses alternate loader when available.
    - Seams/mocks/fakes: mocked globals/engines/loaders and synthetic resource/config content.
    - Constraints/blockers: direct `.resx` write path should be isolated or covered only through mocked/synthetic configuration objects.
    - Recommended disposition: IMPLEMENT.

72. `UtilitiesCS\HelperClasses\FileSystem\FileSystemInfoWrapper.cs`
    - Surface: wrapper over `FileSystemInfo` members.
    - Test home: nearest `UtilitiesCS.Test\HelperClasses\FileInfoWrapper_Tests.cs` / `DirectoryInfoWrapper_Tests.cs`.
    - Candidate scenarios: forwarding of common properties/methods; null/invalid state handled consistently with the underlying wrapper family.
    - Seams/mocks/fakes: same wrapper pattern as file/directory tests.
    - Constraints/blockers: none.
    - Recommended disposition: IMPLEMENT.

73. `UtilitiesCS\HelperClasses\CloningFunctions\DispatchUtility.cs`
    - Surface: `ImplementsIDispatch`, COM type lookup, `TryGetDispId`, `Invoke`, and internal `IDispatchInfo` routing.
    - Test home: `UtilitiesCS.Test\HelperClasses\DispatchUtility_Tests.cs`.
    - Candidate scenarios: non-dispatch objects return false/null; dispatch-id lookup failure returns false without throw; invalid invoke arguments surface expected exception.
    - Seams/mocks/fakes: COM-visible test doubles or existing dispatch test pattern.
    - Constraints/blockers: avoid live Office COM; keep to synthetic COM-visible objects.
    - Recommended disposition: IMPLEMENT.

74. `UtilitiesCS\Threading\ProgressTracker.cs`
    - Surface: initialize/report/child-spawn logic and viewer close-on-complete behavior.
    - Test home: `UtilitiesCS.Test\Threading\ProgressTracker_Tests.cs`.
    - Candidate scenarios: `Report` updates percent and message; child tracker maps child completion into parent range; hitting 100 closes or finalizes viewer state.
    - Seams/mocks/fakes: direct tracker with stub viewer and token source.
    - Constraints/blockers: none.
    - Recommended disposition: IMPLEMENT.

75. `UtilitiesCS\HelperClasses\WipUnfinished\ComStreamWrapper.cs`
    - Surface: COM `IStream` adapter implementing `Stream` members; nonzero offset restrictions in `Read`/`Write`.
    - Test home: `UtilitiesCS.Test\HelperClasses\ComStreamWrapper_Tests.cs`.
    - Candidate scenarios: read/write with zero offset delegate correctly; nonzero offsets throw as expected; `Seek`, `Length`, and `Position` round-trip via COM stream.
    - Seams/mocks/fakes: mocked `IStream`.
    - Constraints/blockers: none.
    - Recommended disposition: IMPLEMENT.

76. `UtilitiesCS\EmailIntelligence\ClassifierGroups\Actionable\ActionableClassifierGroup.cs`
    - Surface: `InitAsync`, `CreateEngineAsync`, classifier build, category matching, `TestAsync`.
    - Test home: nearest `UtilitiesCS.Test\EmailIntelligence\ClassifierGroups\ClassifierGroups_Tests.cs`.
    - Candidate scenarios: actionable category filter returns expected subset; build path creates engine when prerequisites are met; test path short-circuits empty data.
    - Seams/mocks/fakes: mocked globals, manager, and synthetic category/token inputs.
    - Constraints/blockers: none beyond synthetic data setup.
    - Recommended disposition: IMPLEMENT.

77. `UtilitiesCS\OutlookObjects\Store\StoreWrapperController.cs`
    - Surface: launch/show/populate/save/select-folder/select-filesystem-path flows and change tracking.
    - Test home: `UtilitiesCS.Test\OutlookObjects\Store\StoreWrapperController_Tests.cs`.
    - Candidate scenarios: `PopulateWithCurrent` mirrors the backing wrapper; `AnyChanges` detects field differences; selecting folder/path updates target properties.
    - Seams/mocks/fakes: mocked store wrapper, picker dialogs, and folder selection callbacks.
    - Constraints/blockers: keep all Outlook interactions mocked.
    - Recommended disposition: IMPLEMENT.

78. `UtilitiesCS\EmailIntelligence\ClassifierGroups\Triage\Triage_OlLogic.cs`
    - Surface: filter-view composition/stripping and `TrainSelectionAsync`.
    - Test home: `UtilitiesCS.Test\EmailIntelligence\ClassifierGroups\Triage\Triage_OlLogicTests.cs`.
    - Candidate scenarios: filter-builder strips unsupported clauses; train-selection skips empty selection; selected rows are mapped into training examples correctly.
    - Seams/mocks/fakes: mocked views, selected items, and triage classifier.
    - Constraints/blockers: no live Outlook view needed.
    - Recommended disposition: IMPLEMENT.

79. `UtilitiesCS\HelperClasses\ThemeHelpers\SystemThemeDetector.cs`
    - Surface: `IsSystemDarkMode`, `TryGetIsSystemDarkMode` around registry access.
    - Test home: nearest `UtilitiesCS.Test\HelperClasses`.
    - Candidate scenarios: only catch-path behavior is realistically unit-testable without changing implementation.
    - Seams/mocks/fakes: none for static registry reads.
    - Constraints/blockers: static registry dependency with no injection seam; meaningful positive-path unit tests would couple to machine/user theme settings.
    - Recommended disposition: SKIP_EVALUATION.

80. `UtilitiesCS\EmailIntelligence\Bayesian\Performance\BayesianPerformanceMeasurement.cs`
    - Surface: classifier evaluation/build/test/split/score/save orchestration, confusion extraction, metrics summarization.
    - Test home: `UtilitiesCS.Test\EmailIntelligence\Bayesian\BayesianPerformanceMeasurement_Tests.cs`.
    - Candidate scenarios: split helpers partition datasets as expected; confusion-driver extraction returns expected rows; invalid or empty corpora short-circuit without writing output.
    - Seams/mocks/fakes: synthetic corpora/classifiers and mocked persistence hooks.
    - Constraints/blockers: avoid full end-to-end file/report generation; target pure orchestration helpers and defensive branches.
    - Recommended disposition: IMPLEMENT.

81. `UtilitiesCS\ReusableTypeClasses\Locking\Observable\LinkedList\LockingObservableLinkedListNode.cs`
    - Surface: `List`, `Next`, `Previous`, `Value`, movement helpers, `Invalidate`.
    - Test home: `UtilitiesCS.Test\ReusableTypeClasses\LockingObservableLinkedListNode_Tests.cs`.
    - Candidate scenarios: `Next`/`Previous` wrap the inner node correctly; movement helpers call back into owning list; `Invalidate` clears references.
    - Seams/mocks/fakes: direct list/node instances.
    - Constraints/blockers: none.
    - Recommended disposition: IMPLEMENT.

82. `UtilitiesCS\Extensions\AsyncSerialization.cs`
    - Surface: async read/write/copy helpers, progress reporting, JSON serialization with progress, byte-to-MB formatting.
    - Test home: `UtilitiesCS.Test\Extensions\AsyncSerialization_Tests.cs`.
    - Candidate scenarios: `ToMbString` formats expected values; copy helper reports monotonic progress and respects cancellation; progress message formatting handles zero-complete case.
    - Seams/mocks/fakes: memory streams, fake progress reporters, cancellation tokens.
    - Constraints/blockers: file-path overloads should be covered via stream-centric helpers first.
    - Recommended disposition: IMPLEMENT.

83. `UtilitiesCS\Dialogs\DelegateButton.cs`
    - Surface: constructor overloads, button creation helpers, `FromButton`, button event unwiring/rewiring, `Button_Click` delegate invocation.
    - Test home: `UtilitiesCS.Test\Dialogs\DelegateButton_Tests.cs`.
    - Candidate scenarios: constructors preserve template and dialog result; replacing `Button` unwires old click handler; image helper sets relation and replaces prior image.
    - Seams/mocks/fakes: direct `Button` and `Image` instances.
    - Constraints/blockers: none.
    - Recommended disposition: IMPLEMENT.

84. `UtilitiesCS\ReusableTypeClasses\TimedActions\TimedDiskWriter.cs`
    - Surface: enqueue/async enqueue, timer lifecycle, `OnTimedEvent`, configuration-change restart logic.
    - Test home: `UtilitiesCS.Test\HelperClasses\TimedDiskWriterTests.cs`.
    - Candidate scenarios: enqueue starts timer when inactive; timed event drains queue and invokes writer with batched items; repeated empty checks stop timer; config interval change restarts timer.
    - Seams/mocks/fakes: existing Moq-based partial mock and timer wrapper seam.
    - Constraints/blockers: none; the type already exposes a testable timer abstraction.
    - Recommended disposition: IMPLEMENT.

85. `UtilitiesCS\Threading\UiThread.cs`
    - Surface: `Init`, lazy synchronization-context capture, `SynchronizationContextAwaiter`, `UiSyncContext`, `UiThreadId`, `Dispatcher`, `AutoScaleFactor`.
    - Test home: `UtilitiesCS.Test\Threading\UiThread_Tests.cs`.
    - Candidate scenarios: awaiter rejects null context; `IsCompleted` reflects current context; `OnCompleted` posts continuation to the target context.
    - Seams/mocks/fakes: mocked `SynchronizationContext`; avoid testing full hidden-form bootstrap unless already covered.
    - Constraints/blockers: full static UI initialization is environment-sensitive, so keep tests focused on awaiter/public lazy properties.
    - Recommended disposition: IMPLEMENT.

86. `UtilitiesCS\EmailIntelligence\Bayesian\Obsolete\ClassifierGroup.cs`
    - Surface: classifier add/update/force-update, classify by source/tokens, log metrics/state, deserialize follow-up optimization helpers.
    - Test home: nearest `UtilitiesCS.Test\EmailIntelligence\Bayesian\BayesianClassifierGroup_Tests.cs`.
    - Candidate scenarios: add/update creates or appends to the right classifier; classify returns ordered predictions; dedicated/shared token counts contribute to metrics state.
    - Seams/mocks/fakes: synthetic token sequences and fake globals for progress dependencies.
    - Constraints/blockers: optimization/logging methods reference broader app globals; prioritize pure classification/update behavior.
    - Recommended disposition: IMPLEMENT.

87. `UtilitiesCS\ReusableTypeClasses\Locking\Observable\LinkedList\LockingObservableLinkedList.cs`
    - Surface: collection-changed notifications, add/move/remove/take helpers, partial observer registration/removal.
    - Test home: `UtilitiesCS.Test\ReusableTypeClasses\LockingObservableLinkedList_Tests.cs`.
    - Candidate scenarios: add/remove raise the expected action and node references; `AddOrMoveFirst` moves rather than duplicates; partial observers receive only keyed node changes; `RemoveAllObservers` returns prior registrations.
    - Seams/mocks/fakes: direct list and lightweight observer fakes.
    - Constraints/blockers: none.
    - Recommended disposition: IMPLEMENT.

88. `UtilitiesCS\OutlookObjects\Table\OlToDoTable.cs`
    - Surface: `GetToDoTable`, folder-field ensuring, item-value ensuring.
    - Test home: `UtilitiesCS.Test\OutlookObjects\Table\OlToDoTable_Tests.cs`.
    - Candidate scenarios: missing To-Do default folder returns `null`; columns are cleared and expected fields re-added; unreadable items are skipped without failing the table build.
    - Seams/mocks/fakes: mocked `Store`, `MAPIFolder`, `Items`, `Table`, and `PropertyAccessor`.
    - Constraints/blockers: keep all Outlook interactions mocked.
    - Recommended disposition: IMPLEMENT.

89. `UtilitiesCS\HelperClasses\FileSystem\FilePathHelper.cs`
    - Surface: path/name/stem synchronization, parse/extract helpers, max-path adjustment, clone/deep-copy/copy-changed behavior.
    - Test home: `UtilitiesCS.Test\HelperClasses\FilePathHelper_Tests.cs`.
    - Candidate scenarios: property changes recompute dependent path/name fields; `TryParseFileStem` handles empty/prefix/suffix combinations; `AdjustForMaxPath` truncates only the seed; `CopyChanged` reports just changed properties.
    - Seams/mocks/fakes: pure in-memory strings.
    - Constraints/blockers: none.
    - Recommended disposition: IMPLEMENT.

**Mandatory unachievable objective callout**:
- **No material objective in the requested research scope proved unachievable. The only implementation caveat is that some files are better marked `SKIP_EVALUATION` because the current code exposes little or no meaningful deterministic unit-test surface without first changing production design.**

## Recommended Approach

Use one implementation path only:

1. Extend exact existing test files first for the files that already have a clear home (`DelegateButton`, `FunctionButton`, `YesNoToAll`, `ProgressTracker*`, `UiThread`, `TimedDiskWriter`, `AsyncSerialization`, `OlTableExtensions`, `OlToDoTable`, `StoreWrapperController`, `OneDriveDownloader`, `FilePathHelper`, `FileInfoWrapper`, `DirectoryInfoWrapper`, `DispatchUtility`, `LockingObservableLinkedList*`, classifier-group utilities, triage, recents, corpus, Bayesian performance helpers).
2. For remaining implementable files without an exact home, add or extend adjacent folder-level test files under the same functional area rather than scattering single-class files prematurely.
3. Keep new tests narrow and deterministic: cover helper methods, event routing, state synchronization, configuration transitions, and mocked Outlook/HTTP/persistence orchestration.
4. Exclude the `SKIP_EVALUATION` set from the initial implementation plan because they are either constructor-only designer shells or static environment/PInvoke surfaces with poor deterministic unit-test ROI.

Rejected alternatives (brief summary):

- Add end-to-end Outlook/UI automation coverage for these files.
  - Rejected because the repository conventions and the requested planning scope favor deterministic unit tests without live Outlook, COM session dependence, or interactive UI.
- Force coverage on shell/PInvoke/registry-bound files through reflection-heavy or environment-coupled tests.
  - Rejected because it would create brittle tests with low signal and high maintenance cost.
- Create many new standalone test files immediately.
  - Rejected as first choice because `UtilitiesCS.Test.csproj` is explicit-include and many exact/adjacent homes already exist.

## Implementation Guidance

- **Objectives**: Raise coverage across the 89 below-threshold compiled `UtilitiesCS` files by extending existing MSTest suites first, targeting deterministic helper/state/orchestration logic, and excluding only the small `SKIP_EVALUATION` subset.
- **Key Tasks**: extend exact tests; add adjacent test files only where no exact home exists; mock Outlook/HTTP/config/persistence dependencies; cover public-surface helpers and event routing before reflective/private branches; leave shell/PInvoke/registry-only files out of the first implementation batch.
- **Dependencies**: MSTest, Moq, FluentAssertions, existing fake globals and wrapper seams in `UtilitiesCS.Test`, BrightIdeas/WinForms controls instantiated locally, mocked Outlook COM interfaces, in-memory streams and serialized strings.
- **Success Criteria**: every non-skipped file in the ordered list has a concrete deterministic test target; existing exact test homes are reused where available; no live Outlook/network/shell/multi-monitor/registry mutation is required; plan generation can separate implementable files from the low-ROI `SKIP_EVALUATION` subset immediately.