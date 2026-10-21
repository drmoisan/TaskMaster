# P1-T2 — Compile seam only

Timestamp: 2026-09-01T22-22

## What was landed

Only the compile seam. **No adoption logic was added to `LoadFolderHandlerAsync`.**

1. `QuickFiler/Controllers/QfcItemController.cs` — declared the carried member
   `private IFolderSearchHandler _carriedFolderHandler;` immediately after `_predeterminedFolder`,
   with an XML documentation block stating the contract. The narrow `IFolderSearchHandler` seam is
   used rather than the concrete `FolderPredictor`, as AC1 requires.
2. `QuickFiler/Controllers/QfcItemController.Initialization.cs` — added
   `IFolderSearchHandler carriedFolderHandler = null` as the last **optional** parameter of the
   primary constructor (after `folderPredictorEmptyFactory`) and of the `predeterminedFolder`
   constructor (after `predeterminedFolder`), each storing the value into `_carriedFolderHandler`.
   A `<param>` documentation entry was added for the `predeterminedFolder` constructor.

The parameter is optional in both constructors deliberately: every existing construction site,
production and test, continues to bind and compile unchanged, so this task introduces no collateral
edit anywhere and the seam can be landed without touching any test.

## Acceptance condition 1 — analyzer build exits 0

Command: `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
EXIT_CODE: 0
Summary: `5 Warning(s)`, `0 Error(s)`. The warning count equals the `BASELINE_ANALYZER_SUMMARY` count
of 5 and every one is the same uncoded System.Reactive `packages.config` warning. No coded warning
of any kind was emitted. `CoreCompile:` ran 65 times, so the gate was not vacuous.

## Acceptance condition 2 — nullable build exits 0

Command: `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`
EXIT_CODE: 0
Summary: `5 Warning(s)`, `0 Error(s)`. No `CS86` diagnostic was reported, matching the empty P0-T7
baseline. `CoreCompile:` ran 75 times.

## Acceptance condition 3 — `_folderPredictorFactory(` still inside the `varList is null` branch

`QuickFiler/Controllers/QfcItemController.FolderHandling.cs` was **not modified by this task**. The
token `_folderPredictorFactory(` occurs at lines 31, 44, 67 and 112.

- `:67` sits inside `LoadFolderHandlerAsync`'s `varList is null` branch, which spans `:60-106`
  (`if (varList is null)` opens at `:60`; the branch's closing brace is at `:106`, followed by
  `else` at `:107`). The condition holds.
- `:112` is the `else` (`FromArrayOrString`) branch of the same method, unchanged.
- `:31` and `:44` are the two branches of the synchronous `LoadFolderHandler`, unchanged.

Note on the citation: the plan states the branch "spans `:60-106` before this change". That is
correct as written. The enclosing method `LoadFolderHandlerAsync` spans `:57-131`, which is a
different span and is not what the plan cites.

## Acceptance condition 4 — reflection-based constructor assertions in `QuickFiler.Test`

Every reflection constructor lookup in `QuickFiler.Test`, enumerated by file and line with a verdict:

| File | Line | Target | Verdict |
|---|---:|---|---|
| QuickFiler.Test/Controllers/EfcFormControllerTests.cs | 26 | `EfcFormController` | **Unaffected.** Different type; this change does not touch it. |
| QuickFiler.Test/Controllers/EfcHomeControllerTests.cs | 33 | `EfcHomeController` | **Unaffected.** Different type. |
| QuickFiler.Test/Controllers/QfcCollectionControllerDefects468Tests.cs | 115 | `QfcCollectionController.GetConstructors()` | **Still holds.** See below. |
| QuickFiler.Test/Controllers/QfcItemController.FolderHandlingTests.cs | 103 | `FolderPredictor.GetConstructors()` | **Unaffected.** See below. |
| QuickFiler.Test/Controllers/QfcStreamingDequeueConfidenceGateTests.cs | 48 | `QfcStreamingDequeueConfidenceGate` nine-parameter constructor, by exact type array | **Affected by P1-T4, not by this task.** The type array at `:51-62` names `Func<MailItem, CancellationToken, Task<(long Score, string TopFolder)>>` at `:54`; P1-T4 widens that tuple and must widen this lookup with it. The lookup fails CLOSED by design, as its own comment at `:43-47` records, so leaving it unwidened makes every test in the partial class fail rather than degrade quietly. Unchanged by this task. |
| QuickFiler.Test/Viewers/BreadcrumbDropDownHostTests.cs | 427 | `BreadcrumbDropDownHost` | **Unaffected.** Different type. |
| QuickFiler.Test/Viewers/BreadcrumbDropDownLifecycleTests.cs | 169 | `BreadcrumbDropDown` type | **Unaffected.** Different type. |
| QuickFiler.Test/Viewers/BreadcrumbDropDownReadinessTests.cs | 273, 285 | `BreadcrumbDropDownHost` | **Unaffected.** Different type. |
| QuickFiler.Test/Viewers/BreadcrumbPopupBoundaryCoverageTests.cs | 126 | `BreadcrumbUiDispatcher` | **Unaffected.** Different type. |

### The two assertions the plan names explicitly

- **`QuickFiler.Test/Controllers/QfcItemController.FolderHandlingTests.cs:102-107`** selects the
  single `FolderPredictor` constructor whose one parameter is named `Application`. Its target is
  `FolderPredictor`, a type under `UtilitiesCS` that this change may not modify (AC23). **It is
  unaffected.**
- **`QuickFiler.Test/Controllers/QfcCollectionControllerDefects468Tests.cs`** — the plan cites
  `:110-131`. The `[TestMethod]` attribute is at `:109` and the method
  `ParentFieldAndConstructorParameterAreTypedIQfcFormController` spans `:110-150`; the plan's span
  is truncated at `:131`, which is the `parameters[4].ParameterType.FullName` read, and omits the
  two `Should().Be(...)` assertions at `:134-149`. The substantive requirement the plan states is
  correct and is what matters: the test asserts `typeof(QfcCollectionController).GetConstructors()`
  contains exactly one entry (`ContainSingle` at `:116-120`) and that parameter 5 is typed
  `QuickFiler.Controllers.IQfcFormController` (`:142-149`). **The assertion still holds** after this
  task, which changed no constructor on `QfcCollectionController`.

  **Constraint carried forward to P1-T5:** when P1-T5 introduces a new partial part of
  `QfcCollectionController`, that part must add **no second public constructor**, or
  `ContainSingle` fails.

## What this task deliberately did not do

- No adoption of the carried handler in `LoadFolderHandlerAsync`. That is P1-T7.
- No release of the carried handler in `Cleanup`. That is P1-T7.
- No change to `QfcPreScoredItem`, `IFolderScoringService`, the gate, the datamodel, `QfcItemGroup`,
  `QfcCollectionController`, `QfcHomeController` or `QfcQueue`. Those are P1-T4, P1-T5 and P1-T6.
- No test file was modified.

## File sizes after this task

| Path | Before | After |
|---|---:|---:|
| QuickFiler/Controllers/QfcItemController.cs | 323 | 334 |
| QuickFiler/Controllers/QfcItemController.Initialization.cs | 489 | 497 |

`QfcItemController.Initialization.cs` is at 497, below the 500-line cap with 3 lines of headroom.
The plan's first permitted remedy therefore applies: the constructor stays in place because the
addition keeps the file at or below 500 lines, and no relocation into a new part is required.
`dotnet tool run csharpier check` on both edited files reports `Checked 2 files`, with neither
listed as needing formatting, so these counts are post-format counts and will not move in P2-T1.
