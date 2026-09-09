# Research: #813 — unguarded `ArchiveRootPath` read in `AssignFolderComboBox`

- Issue: #813
- Feature folder: `docs/features/active/2026-09-08-assignfoldercombobox-unguarded-archiverootpath-read-813/`
- Related: #812 (hardens `AppOlObjects.ArchiveRootPath` getter itself — out of scope here)

## 1. Current shape of `AssignFolderComboBox` and control flow

File: `QuickFiler/Controllers/QfcItemController.FolderHandling.cs`, method starts line 191.

Sequence inside the `if (_folderHandler?.FolderArray?.Length > 0)` block (lines 200-249):

1. `EnsureBreadcrumbPipeline()` (line 206)
2. `_itemViewer.AddFolderItems(_folderHandler.FolderArray)` (line 212) — **combo box IS populated here**
3. `if (_folderHandler.Suggestions != null) _itemViewer.SetFolderSuggestions(_folderHandler.FolderRowArray)` (lines 219-222) — row/percentage model populated here
4. `string predetermined = ProjectPredeterminedFolder(_predeterminedFolder, _globals is null ? null : (_globals.Ol?.ArchiveRootPath ?? string.Empty));` (lines 231-234) — **the unguarded read**
5. Preselect-by-name or index fallback (lines 235-247)
6. `_selectedFolder = _itemViewer.GetSelectedFolder();` (line 248)

Confirmed: population (`AddFolderItems`) and suggestion-row population (`SetFolderSuggestions`) both run **before** step 4. If the `ArchiveRootPath` getter throws at step 4, the combo box is already populated with items and suggestion rows — only the preselection step (5) and the `_selectedFolder` assignment (6) are skipped, and the exception propagates out of `AssignFolderComboBox` uncaught. Because the async caller is `await _itemViewer.UiDispatcher.InvokeAsync(AssignFolderComboBox)` (line 188) and the sync caller marshals via `_itemViewer.Invoke(() => AssignFolderComboBox())` (lines 171/196), the exception surfaces on the UI/dispatcher thread with no local handler.

There is no `try`/`catch` anywhere in `AssignFolderComboBox`.

## 2. `ProjectPredeterminedFolder` and null/empty archive-root behavior

`ProjectPredeterminedFolder` (lines 262-268, same file) is a thin forwarder:

```csharp
internal static string ProjectPredeterminedFolder(string folderPath, string archiveRootPath) =>
    UtilitiesCS.OutlookObjects.Folder.ArchiveStemProjection.ToDisplayStem(folderPath, archiveRootPath);
```

`ArchiveStemProjection.ToDisplayStem` (`UtilitiesCS/OutlookObjects/Folder/ArchiveStemProjection.cs:40-61`):

```csharp
public static string? ToDisplayStem(string? folderPath, string? archiveRoot)
{
    if (folderPath is null || archiveRoot is null) return folderPath;   // identity projection
    if (ArchiveStemContract.TryMakeArchiveRelative(folderPath, archiveRoot, out var stem) && stem.Length > 0)
        return stem;
    return folderPath;
}
```

Verified: a **null** `archiveRoot` argument is already an explicit identity-projection branch (`return folderPath`) — same behavior as the existing `?? string.Empty` fallback for the non-throwing (`Ol` null) case, since an empty root also fails `TryMakeArchiveRelative` and falls through to `return folderPath` at line 60. This means a fix that substitutes **either `null` or `string.Empty`** for the archive-root argument on failure is behaviorally equivalent and requires no change to `ProjectPredeterminedFolder` or `ToDisplayStem`.

## 3. `IOlObjects.ArchiveRootPath` contract

`UtilitiesCS/Interfaces/IGlobals/IOlObjects.cs:15` declares `string ArchiveRootPath { get; }` with no XML doc / no documented exception contract on the interface member itself.

The concrete implementation, `TaskMaster/AppGlobals/AppOlObjects.cs:260-270`, carries the documented contract:

```csharp
/// <exception cref="InvalidOperationException">The archive root is unresolvable or lies
/// outside the composed path. The diagnostic names the rule only; the path is withheld
/// because it carries a mailbox address (#602).</exception>
public string ArchiveRootPath
{
    get
    {
        if (_archiveRootPath is null) _archiveRootPath = ResolveValidatedArchiveRootPath();
        return _archiveRootPath;
    }
}
```

`ResolveValidatedArchiveRootPath()` (instance wrapper, `AppOlObjects.ArchiveRoot.cs:86-93`) delegates to the delegate-driven core (lines 40-72), which:
- normalizes any `COMException` from either underlying read into `InvalidOperationException` (with the COM exception as `InnerException`), then
- forwards to `ArchiveRootPathGuard.RequireResolvedArchiveRoot` (`TaskMaster/AppGlobals/ArchiveRootPathGuard.cs:32-60`), which throws `InvalidOperationException` when the composed or resolved path is null/whitespace (unresolved archive root — the unset-profile case from #813/#797) or when the two paths disagree (cross-store/renamed archive).

Confirmed: `InvalidOperationException` is the sole documented and only reachable exception type from this getter's real implementation (per #736 finding-1 remediation and the #813 issue text). No other exception type is contractually possible from `Ol.ArchiveRootPath` as implemented today. A narrow `catch (InvalidOperationException)` at the `AssignFolderComboBox` call site is therefore sufficient and matches the interface's one concrete implementation; it does not require broadening to `catch (Exception)`.

## 4. Existing test doubles / mocking patterns

Confirmed via `QuickFiler.Test/Controllers/QfcItemController.FolderHandlingTests.cs`, `...FolderHandlingTests.Part2.cs`, `...FolderSuggestionsTests.cs`, and `QfcItemControllerTests.cs`:

- Mocking library used throughout: **Moq** (`using Moq;`), per repo policy. Assertions use **FluentAssertions** (`.Should()`). Tests are **MSTest** (`[TestClass]`/`[TestMethod]`).
- `SetPrivate(controller, "_globals", ...)` / `SetPrivate(controller, "_itemViewer", ...)` reflection helpers (private static, duplicated per test-partial-class file) are the established way to inject test doubles into `QfcItemController` without a public constructor seam.
- `QfcItemController.FolderHandlingTests.Part2.cs` already contains **two existing tests that stub `Ol.ArchiveRootPath` on a loose `Mock<IApplicationGlobals>`** via Moq's recursive-mock support, directly calling `AssignFolderComboBox()`:
  - `AssignFolderComboBox_WhenArchiveRootedPredeterminedFolder_PreselectsThatFolder` (line 163): `globals.SetupGet(g => g.Ol.ArchiveRootPath).Returns(ArchiveRoot);`
  - `AssignFolderComboBox_WhenEmptyArchiveRootAndLeadingSeparator_PreselectsProjectedFolder` (line 266): `globals.SetupGet(g => g.Ol.ArchiveRootPath).Returns(string.Empty);`

  Both use a loose (non-strict) `Mock<IApplicationGlobals>()`, letting Moq auto-mock the intermediate `Ol` getter. This is a direct, proven precedent for a new regression test that instead does `globals.SetupGet(g => g.Ol.ArchiveRootPath).Throws<InvalidOperationException>();` and asserts `AssignFolderComboBox()` does not throw, that `AddFolderItems`/`SetFolderSuggestions` were still invoked, and that `SetFolderSelectedItem` was never called (falls through to the existing `SetFolderSelectedIndex` else-branch).
  - A stricter variant, `new Mock<IOlObjects>(MockBehavior.Strict)` + `globals.SetupGet(x => x.Ol).Returns(olObjects.Object)`, is used elsewhere (`EfcDataModelArchiveRootTests.cs:330-352`, `EfcDataModelTests.cs:220-226`) and is also available if a stricter double is preferred, but the existing `Part2.cs` tests already establish the simpler loose-mock pattern for this exact call site and method.
- `QfcItemControllerTests.cs` (lines 65-166) tests `AssignFolderComboBox` via a `FakeFolderHandler` and `Mock<IItemViewer>` but does **not** set `_globals`/`_predeterminedFolder` in the two population-only tests (65, 111) — `_globals` stays `null` there, which already exercises today's `_globals is null` branch (never throws). Only the `Part2.cs` tests exercise a non-null `_globals.Ol`.

No existing test currently makes `ArchiveRootPath` throw. This is a genuine coverage gap matching the #813 defect exactly.

## 5. Minimal fix options

Both options are scoped to the single expression at lines 231-234 only; neither touches `ProjectPredeterminedFolder`, `ArchiveStemProjection`, or #812's getter-hardening work.

**Option A — narrow try/catch around just the archive-root read, substituting the null/empty fallback:**

```csharp
string archiveRootPath;
try
{
    archiveRootPath = _globals?.Ol?.ArchiveRootPath ?? string.Empty;
}
catch (InvalidOperationException)
{
    // Archive root unconfigured or unresolvable (#813): degrade to no preselection instead of
    // propagating onto the UI dispatcher thread. ProjectPredeterminedFolder/ToDisplayStem already
    // treat an empty/null root as the identity projection.
    archiveRootPath = string.Empty;
}
string predetermined = ProjectPredeterminedFolder(_predeterminedFolder, archiveRootPath);
```

This keeps the rest of the method (including the `FolderContains`/`SetFolderSelectedItem` vs. `SetFolderSelectedIndex` branching at 235-247 and the `_selectedFolder` assignment at 248) reachable and unchanged, satisfying the issue's "QuickFiler continues to operate" expectation. `catch (InvalidOperationException)` matches the one documented/verified contract of the concrete `ArchiveRootPath` getter (§3) — no broad `catch (Exception)`.

**Option B — same shape, but extract a small private helper (e.g. `TryReadArchiveRootPath(out string archiveRootPath)` or `GetArchiveRootPathOrEmpty()`) so the try/catch is a named, independently testable unit** rather than inline in `AssignFolderComboBox`. This is slightly more consistent with the repo's "small reusable helper" design principle and would let a unit test call the helper directly against a throwing `IOlObjects` mock without needing the full `AssignFolderComboBox` UI-plumbing (`InvokeRequired`, `_itemViewer` mock, `_folderHandler`), in addition to the `AssignFolderComboBox`-level regression test. Behaviorally identical to Option A.

Recommendation for the planner: Option A is the smaller diff and keeps the change contained to the one call site the issue names; Option B is preferable if the planner wants an additional narrowly-scoped unit test that isolates the read-and-substitute logic from `AssignFolderComboBox`'s WinForms marshaling guard. Either way, catch only `InvalidOperationException`, never a broad `Exception`, and substitute `string.Empty` (or `null` — both are identity-projection-safe per §2) rather than skipping `ProjectPredeterminedFolder` entirely, since the existing null-`_globals` branch already routes through the same call with the same substitution today.

## 6. Relationship to #812 — scope boundary

Per the issue/spec text and confirmed by reading `AppOlObjects.ArchiveRoot.cs`/`ArchiveRootPathGuard.cs`: #812 is understood to hardened/normalize the `ArchiveRootPath` getter's *internal* COM-failure handling (the `COMException` → `InvalidOperationException` normalization visible in `ResolveValidatedArchiveRootPath`, lines 49-65, and the `RequireResolvedArchiveRoot` guard). That work makes the getter's *contract* reliable (throws only `InvalidOperationException`) but does not — and per the issue text is not intended to — make every *consumer* of that getter safe. #813 is strictly about the one unguarded consumer call site in `AssignFolderComboBox` (lines 231-234). The fix here must not modify `AppOlObjects.cs`, `AppOlObjects.ArchiveRoot.cs`, or `ArchiveRootPathGuard.cs` — those are #812's files. The recommended fix (§5) touches only `QuickFiler/Controllers/QfcItemController.FolderHandling.cs`.

## Test strategy implications

- Add one MSTest regression test in `QuickFiler.Test/Controllers/QfcItemController.FolderHandlingTests.Part2.cs` (or a new `Part3` file if the 500-line file-size ceiling is at risk — check current line count before appending) following the exact pattern of the two existing `Ol.ArchiveRootPath` tests at lines 163 and 266: a loose `Mock<IApplicationGlobals>` with `globals.SetupGet(g => g.Ol.ArchiveRootPath).Throws<InvalidOperationException>();`, asserting:
  - `controller.AssignFolderComboBox()` does not throw (FluentAssertions `Action`/`.Should().NotThrow<InvalidOperationException>()`),
  - `mock.Verify(v => v.AddFolderItems(...), Times.Once())` and `SetFolderSuggestions` still ran (population happened before the throwing read, per §1),
  - `mock.Verify(v => v.SetFolderSelectedItem(It.IsAny<string>()), Times.Never())` and the index-fallback (`SetFolderSelectedIndex`) ran instead, matching the issue's "falls through to the existing else branch" expectation.
- If Option B (extracted helper) is chosen, add a second, narrower unit test directly against the helper with a throwing `IOlObjects`/`IApplicationGlobals` mock, independent of `_itemViewer`/`InvokeRequired` plumbing.
- No new production files are needed; no temp files; no COM/live-Outlook dependency — consistent with existing Moq-based coverage in this file family.
- Per repo coverage exemption policy, `QfcItemController` folder-handling members are not COM-event-handler exemption candidates (they already have direct, seam-based unit coverage in this test family), so the new/changed lines should meet the ordinary >=90%-of-new-code bar, not an exemption.
