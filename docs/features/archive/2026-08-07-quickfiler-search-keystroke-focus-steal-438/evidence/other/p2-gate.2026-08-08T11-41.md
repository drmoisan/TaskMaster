# [P2-T5] Phase 2 Gate — Session/Router Suites Green Unmodified

- **Issue:** #438
- **Task:** [P2-T5]
- **Timestamp:** 2026-08-08T11-41

## Command 1 — scoped session/router/map suites

`pwsh -NoProfile -Command "& 'C:/Program Files/Microsoft Visual Studio/18/Community/Common7/IDE/Extensions/TestPlatform/vstest.console.exe' UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /Settings:scripts/vscode/TaskMaster.cli.runsettings /InIsolation /TestCaseFilter:\"FullyQualifiedName~BreadcrumbSelectionSession|FullyQualifiedName~FolderBreadcrumbBridgeRouter|FullyQualifiedName~BreadcrumbSelectionMap\" ; exit $LASTEXITCODE"`

- **EXIT_CODE:** 0

```
Test Run Successful.
Total tests: 78
     Passed: 78
 Total time: 1.9616 Seconds
```

Zero failures. The 78 tests include the pre-existing `BreadcrumbSelectionSessionTests`, `BreadcrumbSubfolderSelectorSessionTests`, `FolderBreadcrumbBridgeRouterTests` (+ `InFlightTests`, `EdgeTests`), `FolderBreadcrumbRouterSelectionConcurrencyTests`, and `BreadcrumbSelectionMapTests` suites, plus the two new suites added in this phase (11 + 10 tests).

## Command 2 — modification audit of `UtilitiesCS.Test/`

`pwsh -NoProfile -Command "git diff --name-only -- UtilitiesCS.Test/; git ls-files --others --exclude-standard -- UtilitiesCS.Test/"`

- **EXIT_CODE:** 0

Tracked-file diffs:

```
UtilitiesCS.Test/UtilitiesCS.Test.csproj
```

Untracked (new) files:

```
UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbSelectionSessionHighlightTests.cs
UtilitiesCS.Test/OutlookObjects/Folder/FolderBreadcrumbBridgeRouterReplaceItemsTests.cs
```

**Zero existing `UtilitiesCS.Test` `.cs` files were modified.** The only tracked change is the `.csproj`, which gains exactly the two `<Compile Include>` entries required for the new files (AC-14).

## Production changes delivered in Phase 2

| File | Change |
|---|---|
| `UtilitiesCS/OutlookObjects/Folder/BreadcrumbSelectionSession.cs` | one-token `partial` on the class declaration (P2-T1) |
| `UtilitiesCS/OutlookObjects/Folder/BreadcrumbSelectionSession.Highlight.cs` | **new** — `HighlightRow(int)` pending-only transition |
| `UtilitiesCS/OutlookObjects/Folder/FolderBreadcrumbBridgeRouter.cs` | one-token `partial` on the class declaration (P2-T2) |
| `UtilitiesCS/OutlookObjects/Folder/FolderBreadcrumbBridgeRouter.SearchPresentation.cs` | **new** — `ReplaceItemsPreservingSession(IReadOnlyList<string>)`, the `HighlightRow(int)` router pass-through, and the private `BuildPlainRows` projection |
| `UtilitiesCS/UtilitiesCS.csproj` | two `<Compile Include>` entries |

### Note on the router `HighlightRow` pass-through

`BreadcrumbSelectionSession` is a router-private field, so the P4-T1 coordinator composite cannot reach `HighlightRow` without a router-level member. The one-line pass-through `Mutate(() => _selectionSession.HighlightRow(index))` is therefore mechanically required by the P2-T1 contract and is placed in the P2-T2 search-presentation partial, which is the cohesive home for the router's search surface. It adds no behavior beyond exposing the P2-T1 transition and is covered by `HighlightRow_OnAnOpenSession_MovesPendingWithoutChangingTheSelectedFolder`.

## GUI-seam compliance

Both new suites are host-neutral: `BreadcrumbStateModel`, `BreadcrumbSelectionSession`, and `FolderBreadcrumbBridgeRouter` have no WinForms, WebView2, or COM dependency, and the hierarchy provider is a strict Moq mock with no setups (proving the search path never reaches it). No control, window handle, or message pump is created, so no window can appear.

## Result

- **Output Summary:** EXIT_CODE 0 with 78 of 78 tests passing across the existing and new session/router/map suites. `git diff --name-only UtilitiesCS.Test/` shows only the `.csproj`; the two new test files are untracked additions. No existing `UtilitiesCS.Test` test file was modified and no test method was added, removed, weakened, or altered. Accept criteria met.
