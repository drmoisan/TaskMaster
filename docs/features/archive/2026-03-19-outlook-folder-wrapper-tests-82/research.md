<!-- markdownlint-disable-file -->

# Task Research Notes: outlook-folder-wrapper-tests-82

## Research Executed

### File Analysis

- `docs/features/active/2026-03-19-outlook-folder-wrapper-tests-82/issue.md`
  - Scope requires every production C# file under `UtilitiesCS/OutlookObjects/Folder` to reach `>= 80%` line coverage, with deterministic MSTest tests under `UtilitiesCS.Test/OutlookObjects/Folder`; user also explicitly called out `MsgToMime/MAPIMethods.cs` if compiled/in scope.
- `docs/features/active/2026-03-19-outlook-folder-wrapper-tests-82/spec.md`
  - Spec is still draft-level and does not yet contain evidence-backed implementation strategy, per-file baseline coverage, or seam analysis.
- `docs/features/active/2026-03-19-outlook-folder-wrapper-tests-82/user-story.md`
  - User story is still draft-level and acceptance criteria align with the issue but need evidence-backed implementation details.
- `UtilitiesCS/UtilitiesCS.csproj`
  - Verified compiled folder production files: `FolderConverter.cs`, `FolderMinimalWrapper.cs`, `FolderNavigator.cs`, `FolderPredictor.cs`, `FolderScorer.cs`, `FolderTree.cs`, `FolderWrapper .cs`, `FolderWrapperNameAndParentNameComparer.cs`, `FolderWrapperNameComparer.cs`, `FolderWrapperNameCountSizeComparer.cs`, `FolderWrapperNodeComparer.cs`, `FolderWrapperNodeContentsComparer.cs`, and nested `OutlookObjects/Folder/MsgToMime/MAPIMethods.cs`.
- `UtilitiesCS.Test/UtilitiesCS.Test.csproj`
  - Verified explicit `<Compile Include=...>` test registration for the existing folder tests; there is no `MAPIMethods` test file today, so any new folder test file must be added explicitly to the project.
- `UtilitiesCS/Properties/AssemblyInfo.cs`
  - Verified `[assembly: InternalsVisibleTo("UtilitiesCS.Test")]`, so internal folder members are directly testable from the test project without widening public APIs.
- `UtilitiesCS/OutlookObjects/Folder/FolderConverter.cs`
  - Mixed pure path-mapping logic and static UI prompts (`MyBox.ShowDialog`, `InputBox.ShowDialog`) plus filesystem conversion helpers; current tests cover only the pure path/resolve helpers.
- `UtilitiesCS/OutlookObjects/Folder/FolderMinimalWrapper.cs`
  - Mostly deterministic wrapper logic over `FolderPath` and `Folders`, but the UNC/root-walking branch in `RestoreFromRelativePath` depends on `Parent`, `NameSpace`, and `Stores` traversal.
- `UtilitiesCS/OutlookObjects/Folder/FolderNavigator.cs`
  - Simple traversal over mocked `Application.Session.Folders`; existing tests already cover the file to 100% line coverage.
- `UtilitiesCS/OutlookObjects/Folder/FolderPredictor.cs`
  - Large mixed-responsibility class spanning folder search, folder resolution, suggestion aggregation, UI prompts, and folder creation; current tests cover only normalization, basic constructors, and `FromArrayOrString` validation.
- `UtilitiesCS/OutlookObjects/Folder/FolderScorer.cs`
  - Contains both easy pure collection logic and difficult Outlook/classifier logic (`MailItem.UserProperties`, CTF map, SubjectMap, SmithWaterman, async manager); current tests cover only small pure slices.
- `UtilitiesCS/OutlookObjects/Folder/FolderTree.cs`
  - Tree construction/filtering/comparison is largely mockable with in-memory folder graphs; progress-aware overloads, selection constructors, root detangling, and compare overloads remain uncovered.
- `UtilitiesCS/OutlookObjects/Folder/FolderWrapper .cs`
  - Already has decent baseline coverage through serialized-state and traversal tests, but significant uncovered logic remains in COM-release safety, folder size fallback paths, async loading, and item comparison/loading.
- `UtilitiesCS/OutlookObjects/Folder/FolderWrapperNameComparer.cs`
  - Already above threshold; current comparer tests cover the file sufficiently.
- `UtilitiesCS/OutlookObjects/Folder/FolderWrapperNameCountSizeComparer.cs`
  - Already above threshold; current comparer tests cover the file sufficiently.
- `UtilitiesCS/OutlookObjects/Folder/FolderWrapperNameAndParentNameComparer.cs`
  - Very close to threshold but still below it; remaining branches are null/parent-name edge conditions.
- `UtilitiesCS/OutlookObjects/Folder/FolderWrapperNodeComparer.cs`
  - Already above threshold; current node-comparer tests cover most meaningful branches.
- `UtilitiesCS/OutlookObjects/Folder/FolderWrapperNodeContentsComparer.cs`
  - Already above threshold.
- `UtilitiesCS/OutlookObjects/Folder/MsgToMime/MAPIMethods.cs`
  - Pure declarations (enums, GUID fields, COM interfaces) with zero coverage; straightforward reflection/constant tests can cover the only executable lines (`.cctor`).
- `UtilitiesCS.Test/OutlookObjects/Folder/FolderConverterTests.cs`
  - Covers `SanitizeFilename`, root resolution, and `Folder` overload with `IApplicationGlobals`; does not cover UI prompt helpers, argument guards, `MAPIFolder` overloads, or private alternatives dictionary behavior.
- `UtilitiesCS.Test/OutlookObjects/Folder/FolderMinimalWrapperTests.cs`
  - Covers happy-path relative-path projection and basic restore behavior; does not cover UNC parent-store traversal or restore error branches.
- `UtilitiesCS.Test/OutlookObjects/Folder/FolderNavigatorTests.cs`
  - Uses Moq-backed `Application`/`NameSpace`/`Folders` graphs and already covers the production file completely.
- `UtilitiesCS.Test/OutlookObjects/Folder/FolderScorerTests.cs`
  - Covers additive suggestion behavior and one reflective `QueryCombined` case, but not `LoadFromField`, `AddOlFolderKeys`, word-sequence query builders, or conversation-based suggestions.
- `UtilitiesCS.Test/OutlookObjects/Folder/FolderPredictorTests.cs`
  - Very shallow baseline; does not exercise search, folder resolution, recents/suggestions population, creation helpers, or refresh logic.
- `UtilitiesCS.Test/OutlookObjects/Folder/FolderTreeTests.cs`
  - Covers some compare/filter/flatten behavior with mocked folder trees, but not multi-root detangling, selection constructors, progress overloads, or event notification API surface.
- `UtilitiesCS.Test/OutlookObjects/Folder/FolderWrapperStateTests.cs`
  - Covers JSON constructor, subscription flags, and relative-path behavior, but not folder size fallback, async loading, COM release helper, or most compare-item flows.
- `UtilitiesCS.Test/OutlookObjects/Folder/FolderWrapperTraversalTests.cs`
  - Covers simple percentage math and one null-globals async failure path, but not successful item comparisons or item-helper loading.
- `coverage/coverage.cobertura.xml`
  - Verified current per-file line-rate baseline for all folder production files, including which files already exceed `80%` and which remain far below target.

### Code Search Results

- `OutlookObjects\\Folder\\|OutlookObjects\\Folder\\MsgToMime\\MAPIMethods.cs`
  - Coverage report contains explicit entries for all 13 compiled folder files. Current line-rate baseline: `FolderNavigator 1.0000`, `FolderWrapperNameComparer 1.0000`, `FolderWrapperNameCountSizeComparer 1.0000`, `FolderWrapperNodeContentsComparer 0.9286`, `FolderWrapperNodeComparer 0.8242`, `FolderWrapperNameAndParentNameComparer 0.7955`, `FolderMinimalWrapper 0.7200`, `FolderWrapper 0.7060`, `FolderConverter 0.4232`, `FolderTree 0.2985`, `FolderScorer 0.1721`, `FolderPredictor 0.1511`, `MAPIMethods 0.0000`.
- `InternalsVisibleTo("UtilitiesCS.Test")`
  - Found in `UtilitiesCS/Properties/AssemblyInfo.cs`, confirming internal folder members are already test-accessible.
- `MAPIMethods|FolderPredictor|FolderScorer|FolderTree`
  - `FolderPredictor`, `FolderScorer`, and `FolderTree` are used by live production flows (`SortEmail`, `EmailFilerConfig`, `EmailDataMiner`, folder tooling); `MAPIMethods` is compiled but current usage in `MailItemExtensions.cs` is commented out, so declaration-only coverage is sufficient and low risk.
- `UtilitiesCS.Test\\OutlookObjects\\Folder\\`
  - Existing test files already map one-to-one with most production files except `MsgToMime/MAPIMethods.cs`, which currently has no corresponding test file.

### External Research

- #githubRepo:"microsoft/testfx MSTest STA threading"
  - Verified from the authoritative `microsoft/testfx` repository page that MSTest/TestFX is the official, actively maintained Microsoft testing framework and that the repo is the source of MSTest + Microsoft.Testing.Platform documentation and packages. This supports using MSTest-native attributes/features rather than introducing another test framework.
- #fetch:https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-mstest-intro
  - Microsoft Learn confirms MSTest v4 is current, supports .NET Framework 4.6.2+, and integrates with Test Explorer/CI; this matches the repo’s MSTest 4.1 usage in `UtilitiesCS.Test.csproj`.
- #fetch:https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-mstest-writing-tests-controlling-execution#threading-attributes
  - Microsoft Learn documents `STATestClassAttribute` and `STATestMethodAttribute` for COM/Windows STA scenarios, with `UseSTASynchronizationContext` support in MSTest 4.1. This is relevant if any remaining Outlook/WinForms-bound folder tests need STA-safe execution rather than plain `[TestMethod]`.
- #fetch:https://learn.microsoft.com/en-us/office/vba/api/outlook.folder.folderpath
  - Microsoft Learn confirms `Folder.FolderPath` is a read-only string property, supporting the existing mock-based approach for relative-path and path-conversion tests.
- #fetch:https://learn.microsoft.com/en-us/office/vba/api/outlook.folder.folders
  - Microsoft Learn confirms `Folder.Folders` returns the child-folder collection, which aligns with the repo’s existing Moq strategy of faking folder graphs by mocking `Folders` indexers/enumerators rather than requiring live Outlook.

### Project Conventions

- Standards referenced: `MSTest` for unit tests, `Moq` for mocking only where needed, `FluentAssertions` preferred for new assertions, no live Outlook dependency, explicit test compile includes, deterministic tests, and per-file coverage evidence.
- Instructions followed: `.github/copilot-instructions.md`, `.github/instructions/general-code-change.instructions.md`, `.github/instructions/general-unit-test.instructions.md`, `.github/instructions/csharp-code-change.instructions.md`, `.github/instructions/csharp-unit-test.instructions.md`, plus repository skill guidance for policy order and C# routing.

## Key Discoveries

### Project Structure

The active feature folder already contains `issue.md`, `spec.md`, `user-story.md`, and a draft plan, but the implementation-facing evidence was missing. The compiled production scope is larger than the “directly under folder” wording alone suggests because `UtilitiesCS.csproj` explicitly includes nested `OutlookObjects/Folder/MsgToMime/MAPIMethods.cs`, and the user also called that file out for inclusion if compiled.

Current in-scope compiled production files and line-rate baseline from `coverage/coverage.cobertura.xml`:

- `UtilitiesCS/OutlookObjects/Folder/FolderConverter.cs` — `42.3237%`
- `UtilitiesCS/OutlookObjects/Folder/FolderMinimalWrapper.cs` — `72.0000%`
- `UtilitiesCS/OutlookObjects/Folder/FolderNavigator.cs` — `100.0000%`
- `UtilitiesCS/OutlookObjects/Folder/FolderPredictor.cs` — `15.1099%`
- `UtilitiesCS/OutlookObjects/Folder/FolderScorer.cs` — `17.2078%`
- `UtilitiesCS/OutlookObjects/Folder/FolderTree.cs` — `29.8539%`
- `UtilitiesCS/OutlookObjects/Folder/FolderWrapper .cs` — `70.5977%`
- `UtilitiesCS/OutlookObjects/Folder/FolderWrapperNameAndParentNameComparer.cs` — `79.5455%`
- `UtilitiesCS/OutlookObjects/Folder/FolderWrapperNameComparer.cs` — `100.0000%`
- `UtilitiesCS/OutlookObjects/Folder/FolderWrapperNameCountSizeComparer.cs` — `100.0000%`
- `UtilitiesCS/OutlookObjects/Folder/FolderWrapperNodeComparer.cs` — `82.4176%`
- `UtilitiesCS/OutlookObjects/Folder/FolderWrapperNodeContentsComparer.cs` — `92.8571%`
- `UtilitiesCS/OutlookObjects/Folder/MsgToMime/MAPIMethods.cs` — `0.0000%`

That means 5 files are already at or above `80%`, 1 file is just below threshold (`FolderWrapperNameAndParentNameComparer.cs`), and 7 files are materially below threshold (`FolderConverter.cs`, `FolderPredictor.cs`, `FolderScorer.cs`, `FolderTree.cs`, `FolderWrapper .cs`, `FolderMinimalWrapper.cs`, plus `MAPIMethods.cs`).

### Implementation Patterns

The existing test suite already proves the preferred pattern for this subsystem:

- Mock `Outlook.Folder`, `Outlook.Folders`, `Application`, and `NameSpace` with Moq.
- Back collection traversal with `ArrayList` enumerators and string-key indexers.
- Keep the test graph fully in-memory and deterministic.
- Use `FluentAssertions` for scenario-focused assertions.

Files divide into three practical buckets:

1. **Already-sufficient / near-sufficient pure comparers**
   - `FolderWrapperNameComparer`, `FolderWrapperNameCountSizeComparer`, `FolderWrapperNodeComparer`, `FolderWrapperNodeContentsComparer`, `FolderNavigator`
   - `FolderWrapperNameAndParentNameComparer` only needs a few edge-case tests.

2. **Mockable with existing seams and likely no production change needed**
   - `FolderMinimalWrapper`, `FolderTree`, `FolderWrapper`, much of `FolderScorer`
   - Existing tests already mock folder graphs successfully, so more targeted branch/constructor/compare tests should move these files materially upward.

3. **High-risk for “tests only” because of static UI / filesystem / Outlook-heavy branching**
   - `FolderPredictor`
   - `FolderConverter` (specifically the private prompt/alternative-selection path)
   - These files call static UI/filesystem helpers (`InputBox.ShowDialog`, `MyBox.ShowDialog`, `MessageBox.Show`, `Directory.CreateDirectory`, `UiThread.UiSyncContext`) that are not currently abstracted.

### Complete Examples

```csharp
// Source: UtilitiesCS.Test/OutlookObjects/Folder/FolderNavigatorTests.cs
private static Mock<OutlookFolders> CreateFoldersCollection(
    IDictionary<string, OutlookFolder> foldersByName,
    params OutlookFolder[] enumerableChildren
)
{
    var folders = new Mock<OutlookFolders>();
    var enumerableItems = enumerableChildren ?? [];
    var collection = new ArrayList(enumerableItems);

    folders
        .Setup(x => x[It.IsAny<object>()])
        .Returns<object>(key =>
        {
            if (
                key is string name
                && foldersByName.TryGetValue(name, out OutlookFolder folder)
            )
            {
                return folder;
            }

            return null;
        });
    folders.Setup(x => x.GetEnumerator()).Returns(() => collection.GetEnumerator());

    return folders;
}
```

This existing helper is the canonical seam for most remaining folder tests: it already supports deterministic traversal, missing-child branches, and nested folder graph setup without live Outlook.

### API and Schema Documentation

- `Folder.FolderPath` is a read-only string property per Microsoft Learn, matching the repo’s current mock-driven path assertions.
- `Folder.Folders` is the child-folder collection per Microsoft Learn, matching the repo’s current mock-driven traversal tests.
- MSTest officially supports `STATestClass` / `STATestMethod` for Windows COM/STA scenarios; use only when a remaining Outlook or WinForms-dependent test genuinely needs STA thread affinity.
- `UtilitiesCS.Test` explicitly references `MSTest.TestFramework`, `MSTest.TestAdapter`, `Moq`, and `FluentAssertions` in `UtilitiesCS.Test.csproj`, so no new dependency is required.

### Configuration Examples

```xml
<!-- Source: UtilitiesCS/UtilitiesCS.csproj -->
<Compile Include="OutlookObjects\Folder\FolderMinimalWrapper.cs" />
<Compile Include="OutlookObjects\Folder\FolderPredictor.cs" />
<Compile Include="OutlookObjects\Folder\FolderScorer.cs" />
<Compile Include="OutlookObjects\Folder\FolderTree.cs" />
<Compile Include="OutlookObjects\Folder\FolderWrapper .cs" />
<Compile Include="OutlookObjects\Folder\FolderConverter.cs" />
<Compile Include="OutlookObjects\Folder\MsgToMime\MAPIMethods.cs" />

<!-- Source: UtilitiesCS.Test/UtilitiesCS.Test.csproj -->
<Compile Include="OutlookObjects\Folder\FolderConverterTests.cs" />
<Compile Include="OutlookObjects\Folder\FolderMinimalWrapperTests.cs" />
<Compile Include="OutlookObjects\Folder\FolderNavigatorTests.cs" />
<Compile Include="OutlookObjects\Folder\FolderScorerTests.cs" />
<Compile Include="OutlookObjects\Folder\FolderPredictorTests.cs" />
<Compile Include="OutlookObjects\Folder\FolderTreeTests.cs" />
<Compile Include="OutlookObjects\Folder\FolderWrapperStateTests.cs" />
<Compile Include="OutlookObjects\Folder\FolderWrapperTraversalTests.cs" />
```

### Technical Requirements

- The final implementation must produce **per-file** coverage evidence, not only aggregate coverage.
- `UtilitiesCS.Test` uses explicit compile includes, so any new test file (for example `MAPIMethodsTests.cs`) must also be added to `UtilitiesCS.Test.csproj`.
- The current repo already permits testing internal members via `InternalsVisibleTo("UtilitiesCS.Test")`.
- For `FolderPredictor` and `FolderConverter`, a strict tests-only approach may stall below `80%` because major uncovered regions are bound to static UI/filesystem calls. A minimal seam may be needed if post-test-only coverage remains below threshold.
- `MAPIMethods.cs` is compiled and currently uncovered, but it is low-risk because the executable coverage target is only its static field initialization; reflection/constant tests are sufficient.

**Mandatory unachievable objective callout**:
- **No objective is currently proven unachievable from evidence gathered.** However, the `>= 80%` target for `FolderPredictor.cs` and possibly `FolderConverter.cs` may require narrowly scoped production testability seams if a tests-only pass cannot cover the static UI/filesystem branches.

## Recommended Approach

Use a **staged hybrid approach** that starts with tests-only expansion everywhere a deterministic seam already exists, then introduces the smallest possible production seam only for the static UI/filesystem choke points that block the `>= 80%` per-file requirement.

Chosen path:

1. **Raise the near-threshold files first with tests only**
   - Add edge-case tests for `FolderWrapperNameAndParentNameComparer`, `FolderMinimalWrapper`, `FolderWrapper`, and `FolderTree` using the existing Moq + in-memory folder graph pattern.
   - These are the cheapest percentage gains and reduce the remaining risk surface quickly.

2. **Fill the easy uncovered branches in `FolderConverter` and `FolderScorer` without changing production code**
   - Cover argument guards, `MAPIFolder` overloads, null/error object branches, `FromArray`, `AddArray(object, int)`, and additional `Query*` combinations.
   - Use reflection where necessary for private helpers/structs rather than widening public APIs.

3. **Build a focused mock harness for `FolderPredictor` public behavior**
   - Add tests for `FolderArray`, `AddRecents`, `AddMatches`, `AddSuggestions`, `GetFolder` overloads, `GetMatchingFolders`, `LoopFolders`, `GetOlSubpath`, and refresh/folder-key failure cases using mock folder graphs and mock globals.
   - This should recover a large amount of coverage before any seam discussion.

4. **Only if coverage remains below 80%, add minimal seam(s) for static UI/filesystem calls**
   - Preferred seam shape: internal/protected delegate or virtual wrapper around `InputBox.ShowDialog`, `MyBox.ShowDialog`, `MessageBox.Show`, `Directory.CreateDirectory`, and any required UI-thread handoff.
   - Keep default behavior identical in production; use the seam only to unblock deterministic tests.

5. **Add a tiny dedicated `MAPIMethods` test file**
   - Validate enum constants, GUID fields, interface visibility/import attributes, and force the type initializer to run.
   - This should move the file from `0%` to compliant coverage with almost no risk.

Why this is the best path:

- It reuses the repo’s proven testing pattern instead of starting a broader refactor.
- It respects the user’s hard `>= 80% per production file` constraint by allowing seams only where evidence shows the current surface is otherwise too rigid.
- It minimizes production risk because the likely code changes are isolated to UI/filesystem indirection, not domain behavior.

Rejected alternatives (brief, non-exhaustive):

- **Live Outlook integration tests** — rejected because repo unit-test policy forbids external/runtime dependencies and the current suite already demonstrates a mock-first design.
- **Broad refactor of the folder subsystem before testing** — rejected because it expands scope far beyond the coverage objective and would add review risk without evidence it is needed.
- **Exclude `MAPIMethods.cs` from scope** — rejected because the file is compiled and the user explicitly asked to include it if compiled/in scope.

## Implementation Guidance

- **Objectives**: Raise every compiled folder production file named above to `>= 80%` line coverage with deterministic MSTest tests under `UtilitiesCS.Test/OutlookObjects/Folder`, while preserving current behavior and avoiding live Outlook/UI dependencies.
- **Key Tasks**:
  - Extend existing folder test files for near-threshold and mock-friendly classes.
  - Add one new `MAPIMethods` test file and register it explicitly in `UtilitiesCS.Test.csproj`.
  - Add richer mock builders for `Outlook.Folder`, `Folders`, `Stores`, `Items`, `Application`, `NameSpace`, and `IApplicationGlobals` so `FolderPredictor`, `FolderScorer`, and `FolderTree` can be exercised in-memory.
  - Re-run coverage after the tests-only pass and check per-file results.
  - If `FolderPredictor` and/or `FolderConverter` still miss `80%`, introduce the smallest internal/protected seam required for static UI/filesystem calls, then add deterministic seam-driven tests.
- **Dependencies**: Existing repo dependencies are sufficient (`MSTest`, `Moq`, `FluentAssertions`). If any remaining COM-thread-affine test must execute against STA-sensitive code, use MSTest’s Windows-only `STATestMethod`/`STATestClass` support rather than another framework.
- **Success Criteria**:
  - Every in-scope compiled production file listed in this note reaches `>= 80%` line coverage in the final verified coverage report.
  - Any new test file is explicitly included in `UtilitiesCS.Test.csproj`.
  - No test requires live Outlook, external services, or runtime temp-file creation.
  - Any production seam added is narrowly scoped, behavior-preserving, and itself covered by tests.