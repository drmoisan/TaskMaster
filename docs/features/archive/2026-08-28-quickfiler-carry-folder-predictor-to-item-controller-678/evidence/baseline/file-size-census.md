# Phase 0 — BASELINE_SIZE_CENSUS (P0-T12)

Timestamp: 2026-09-01T21-40

Derivation: D8 — `(Get-Content -LiteralPath '<path>').Count`. `Measure-Object -Line` was not used: it
reports a different value on a file without a trailing newline.

Base ref: `807fb0bb6e5e49f43efa6b256b05960bf078ca19`. Counts taken from the worktree at `HEAD`
(`fc6784accb040bca164e13ba35adb1ef0db4db75`), which merges that base ref and carries no change under
`QuickFiler/` or `QuickFiler.Test/` relative to it.

## Production paths (12)

| Path | Lines | Headroom to 500 |
|---|---:|---:|
| QuickFiler/Controllers/QfcHighConfidencePreFilter.cs | 191 | 309 |
| QuickFiler/Controllers/QfcStreamingDequeueConfidenceGate.cs | 245 | 255 |
| QuickFiler/Controllers/QfcDatamodel.QueueProcessing.cs | 288 | 212 |
| QuickFiler/Controllers/QfcHomeController.cs | 449 | 51 |
| QuickFiler/Controllers/QfcHomeController.Iteration.cs | 95 | 405 |
| QuickFiler/Controllers/QfcItemGroup.cs | 52 | 448 |
| QuickFiler/Controllers/QfcCollectionController.cs | 2446 | -1946 |
| QuickFiler/Controllers/QfcQueue.cs | 610 | -110 |
| QuickFiler/Controllers/QfcItemController.cs | 323 | 177 |
| QuickFiler/Controllers/QfcItemController.Initialization.cs | 489 | 11 |
| QuickFiler/Controllers/QfcItemController.FolderHandling.cs | 239 | 261 |
| QuickFiler/Controllers/QfcItemController.ViewerSetup.cs | 499 | 1 |

## Test paths (13)

| Path | Lines | Headroom to 500 |
|---|---:|---:|
| QuickFiler.Test/Controllers/QfcItemController.FolderHandlingTests.cs | 498 | 2 |
| QuickFiler.Test/Controllers/QfcItemController.InitializationTests.cs | 261 | 239 |
| QuickFiler.Test/Controllers/QfcHomeControllerIssue218Tests.cs | 261 | 239 |
| QuickFiler.Test/Controllers/QfcHomeControllerRunAsyncHighConfidenceTests.cs | 473 | 27 |
| QuickFiler.Test/Controllers/QfcHighConfidencePreFilterTests.cs | 359 | 141 |
| QuickFiler.Test/Controllers/QfcDatamodelTests.cs | 391 | 109 |
| QuickFiler.Test/Controllers/QfcQueuePurePathsTests.cs | 262 | 238 |
| QuickFiler.Test/Controllers/QfcStreamingDequeueConfidenceGateTests.cs | 468 | 32 |
| QuickFiler.Test/Controllers/QfcStreamingDequeueConfidenceGateTests.Part2.cs | 460 | 40 |
| QuickFiler.Test/Controllers/QfcStreamingDequeueConfidenceGateTests.Part3.cs | 270 | 230 |
| QuickFiler.Test/Controllers/QfcFormControllerTests.cs | 827 | -327 |
| QuickFiler.Test/Controllers/QfcCollectionControllerTests.cs | 499 | 1 |
| QuickFiler.Test/Controllers/QfcHomeControllerIterationTests.cs | 497 | 3 |

Every listed path has a numeric count. No path was missing from the tree.

## Paths with headroom under 20 lines — new partial part required

Nine paths have headroom below 20. For each, the mandated edit is classified as a **whole member**,
which can be relocated to a new partial part, or a **change inside an existing signature or method
body**, which cannot be relocated on its own.

| Path | Headroom | Mandated edit | Relocatable? |
|---|---:|---|---|
| QuickFiler/Controllers/QfcCollectionController.cs | -1946 | P1-T5 adds a parameter to `EncapsulateItemGroup` (`:646`) and to the `QfcPreScoredItem` overload of `LoadControlsAndHandlers_01Async` (`:487`) | **Whole members.** Both methods relocate in full into a new part. Requires `partial` on the class declaration at `:22` (`:21` is the `[ExcludeFromCodeCoverage]` attribute) and a `<Compile Include>` entry in `QuickFiler/QuickFiler.csproj`. |
| QuickFiler/Controllers/QfcQueue.cs | -110 | P1-T6 adds a parameter to `EnqueueAsync` (`:211`) and to `LoadControllersViewersAsync` (`:380`), whose body contains the `new QfcItemController(` construction at `:405` | **Whole members.** Both relocate in full. The construction at `:405` sits inside a lambda in `LoadControllersViewersAsync` and is not itself a relocatable unit, so the enclosing member moves. Requires `partial` on the declaration at `:20`, which is `public class QfcQueue(` and carries a primary constructor whose parameter list must stay on that part alone. |
| QuickFiler/Controllers/QfcItemController.Initialization.cs | 11 | P1-T2 adds a parameter to the `predeterminedFolder` constructor declared at `:86` with its parameter list at `:87-95` | **Change inside an existing signature — not relocatable on its own.** The declaring member is relocatable in full: the constructor `:86-109` together with its complete XML documentation block `:77-85`, which opens with the `/// <summary>` line at `:77`. Preferred remedy is to leave it in place if the addition keeps the file at or below 500; otherwise move constructor plus documentation in full, leaving no orphan documentation line. |
| QuickFiler/Controllers/QfcItemController.ViewerSetup.cs | 1 | P1-T7 adds one statement inside the `Cleanup` method body, alongside the first `_folderHandler = null;` at `:465` | **Change inside an existing method body — not relocatable.** One added line takes the file from 499 to 500, which is at the cap and not over it. |
| QuickFiler.Test/Controllers/QfcItemController.FolderHandlingTests.cs | 2 | P1-T3, P1-T8 and P1-T9 add three new `[TestMethod]` members | **Whole members.** They are placed directly in the new part `QfcItemController.FolderHandlingTests.Part2.cs` rather than added here. Requires `partial` on the declaration at `:19`, no second `[TestClass]` attribute on the new part (mirroring `QfcItemController.InitializationTests.cs:30`), and a `<Compile Include>` entry in `QuickFiler.Test/QuickFiler.Test.csproj`. |
| QuickFiler.Test/Controllers/QfcFormControllerTests.cs | -327 | P1-T4 collateral: the `new QfcPreScoredItem(` site at `:814` gains an argument | **Change inside an existing method body — not relocatable on its own.** The enclosing `[TestMethod]` is relocatable in full. This file is already over the cap at 827 and must not grow at all, so its post-change count is measured against its `BASELINE_SIZE_CENSUS` value of 827 rather than against 500. Relocation, if needed, requires `partial` at `:20`. |
| QuickFiler.Test/Controllers/QfcCollectionControllerTests.cs | 1 | P1-T4 collateral: the `new QfcPreScoredItem(` site at `:307` gains an argument | **Change inside an existing method body — not relocatable on its own.** The enclosing `[TestMethod]` `CarrierLoad_SetsPredeterminedFolderOnItemGroup` (`:302-326`) is relocatable in full. Relocation requires `partial` at `:24`. |
| QuickFiler.Test/Controllers/QfcHomeControllerIterationTests.cs | 3 | P1-T6 collateral: the `IQfcQueue.EnqueueAsync` setup at `:133` and verifications at `:175` and `:282`, plus the `DequeueNextItemGroupWithOutcomeAsync` setups and verifications at `:118`, `:194`, `:221` and `:253` | **Changes inside existing method bodies — not relocatable on their own.** The enclosing `[TestMethod]` members are relocatable in full. Relocation requires `partial` at `:26`. |

`QuickFiler/Controllers/QfcHomeController.cs` (headroom 51),
`QuickFiler.Test/Controllers/QfcHomeControllerRunAsyncHighConfidenceTests.cs` (headroom 27) and
`QuickFiler.Test/Controllers/QfcStreamingDequeueConfidenceGateTests.cs` (headroom 32) are above the
20-line threshold and are not flagged. They are still audited by P2-T10 against the 500-line cap
after CSharpier reflow.

## Paths edited by this plan that deliberately carry no census row

The following three paths are edited by this plan and are recorded here as deliberate census
omissions rather than oversights:

- `QuickFiler/QuickFiler.csproj`
- `QuickFiler.Test/QuickFiler.Test.csproj`
- `docs/features/active/2026-08-28-quickfiler-carry-folder-predictor-to-item-controller-678/issue.md`

Reason: the 500-line audit in P2-T10 enumerates `.cs` files only, so neither `.csproj` is in its
scope; and the General Code Change Policy exempts Markdown documentation files from the file-size
limit, so `issue.md` is not subject to the cap. Both `.csproj` files gain `<Compile Include>` entries
because both projects use explicit compile item lists, so every new `.cs` file requires an entry.
