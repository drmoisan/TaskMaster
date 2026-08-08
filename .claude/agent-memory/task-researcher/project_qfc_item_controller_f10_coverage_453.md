---
name: qfc-item-controller-f10-coverage-453
description: Epic #136 F10 (#453) research — QuickFiler.Test.csproj also uses explicit Compile Includes; FocusAndThemeTests.cs 497/500 and FolderHandlingTests.cs 498/500; FlagTasks.Run non-virtual; Theme lives in UtilitiesCS; branch gaps come from null-conditionals inside logger.Debug strings
metadata:
  type: project
---

Findings from F10 (`quickfiler-item-controller-coverage`, issue #453) per-file research, 2026-08-07.
Each is non-obvious and would cost a later child a failed toolchain pass or a wrong seam choice.

- **`QuickFiler.Test/QuickFiler.Test.csproj` also uses explicit `<Compile Include=...>` with no
  globbing** (see `:58-128`). epic.md's "Cross-Child Constraints" section names only
  `QuickFiler/QuickFiler.csproj`, so a child that adds a **test** file and does not edit the test
  csproj gets a silently-unbuilt test file. Same CRLF-preservation rule applies.
- **`QuickFiler.Test/Controllers/QfcItemController.FocusAndThemeTests.cs` is 497 lines** against the
  500-line limit. Any new FocusAndTheme test must go in a new file; the reusable helpers
  (`BuildAllThemes`, `BuildFocusController`, `BuildExecutingViewer`, `EnableHandlelessThemeInvoke`)
  are file-local there and should be promoted into `QfcItemController.TestSupport.cs` (365 lines).
- **`TaskVisualization.FlagTasks.Run(bool)` is non-virtual** (`TaskVisualization/FlagTasks.cs:89`), so
  Moq cannot stop it opening a live modal. The existing tests dodge it by having the injected
  `_flagTasksFactory` throw a sentinel. The minimum seam is a `Func<FlagTasks,bool,DialogResult>`
  runner delegate defaulted in `QfcItemController.Initialization.cs`'s `??=` block (`:389-395`) —
  `TaskVisualization` is outside every epic-#136 child's assignment.
- **`Theme` is `UtilitiesCS/HelperClasses/ThemeHelpers/Theme.cs`, not a QuickFiler/F4 file.**
  `QuickFiler/Helper Classes/QfcThemeHelper.cs` (F4/#434) is only the *factory* (`SetupThemes`) called
  from `QfcItemController.Initialization.cs`. So F10 theme work does not cross the F4 boundary.
- **`QfcItemController.Initialization.cs` is 466/500** — the tightest file in the F10 change set; any
  seam default added there must be re-measured immediately before editing.
- **`QuickFiler.Test/Controllers/QfcItemController.FolderHandlingTests.cs` is 498/500** — a *second*
  test file in this family at the wall, with 2 lines of headroom. Treat every
  `QfcItemController.*Tests.cs` as potentially full until measured.
- **The class-level `<lines>` block already applies the epic's max-hits union rule.** Verified on
  `QfcItemController.FolderHandling.cs`: the `<AssignFolderComboBox>b__152_0` / `<PopulateFolderComboBox>b__150_0`
  closure methods report line 141 `hits="0"` while the class-level block reports line 141 `hits="1"`.
  So reading the class-level block *is* the de-duplication; no separate merge pass is needed for a
  single-`<class>` file.
- **`QuickFiler`'s four `logger.Debug($"…{ItemHelper?.Subject}…{_folderHandler?.Suggestions?.TopScore() ?? 0}")`
  interpolations in `QfcItemController.FolderHandling.cs` (lines 36, 49, 81, 125) emit 4 conditions /
  8 outcomes EACH — 32 of that file's 60 branch conditions (53%).** Branch-coverage gaps in this
  family come from null-conditional operators inside diagnostic log strings, not from ranking or
  selection logic (lines 193/202/227/230 are already 100%). Cover them by varying `ItemHelper`-null,
  factory-returns-null, and `Suggestions`-null, not by testing folder ranking.
- **`FolderPredictor` built via the navigation-only `FolderPredictor(Outlook.Application)` ctor has a
  NULL `Suggestions`** — `UtilitiesCS/OutlookObjects/Folder/FolderPredictor.cs:263` is
  `private FolderScorer _suggestions = null!;` with no initializer, contradicting the ctor comment at
  `:28-32`. `Suggestions` has a public setter, so a test can drive both null and non-null sides.

**Why:** F10's two below-floor files turned out to need zero seams (FocusAndTheme) and only optional
delegate seams (MailActions); the real execution risks are build-file and file-size mechanics, not
testability.

**How to apply:** when planning any QuickFiler test work, check both csproj files and the target test
file's line count before writing tasks. Related: [[quickfiler-percoverage-epic-136]],
[[qfc-item-controller-227-r2-denial]].
