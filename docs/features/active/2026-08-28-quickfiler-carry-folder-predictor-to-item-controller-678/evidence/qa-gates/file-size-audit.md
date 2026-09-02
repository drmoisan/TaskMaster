# P2-T10 — File-size audit (AC21)

Timestamp: 2026-09-02T00-18

Run **after** P2-T1, because CSharpier reflow changes line counts; every count below is a post-format
count taken from the tree that passed the final toolchain loop.

## Commands

```
git add -A -- QuickFiler QuickFiler.Test
git diff --cached --name-only 807fb0bb6e5e49f43efa6b256b05960bf078ca19 -- QuickFiler QuickFiler.Test
```

Staging first is required: the name-listing diff enumerates tracked changes only, so the seven files
this change creates would otherwise be invisible to it. All seven appear below.

Line counts use Derivation D8, `(Get-Content -LiteralPath '<path>').Count`. `Measure-Object -Line`
was not used: it reports a different value on a file without a trailing newline.

## Every `.cs` file in the anchored diff, with its post-format count

| Path | Lines | Verdict |
|---|---:|---|
| QuickFiler/Controllers/IQfcQueue.cs | 53 | OK |
| QuickFiler/Controllers/QfcCollectionController.CarrierLoad.cs | 158 | OK |
| QuickFiler/Controllers/QfcCollectionController.cs | 2336 | Over 500, at or below census 2446 |
| QuickFiler/Controllers/QfcDatamodel.QueueProcessing.cs | 292 | OK |
| QuickFiler/Controllers/QfcHighConfidencePreFilter.cs | 228 | OK |
| QuickFiler/Controllers/QfcHomeController.Iteration.cs | 98 | OK |
| QuickFiler/Controllers/QfcHomeController.cs | 465 | OK |
| QuickFiler/Controllers/QfcItemController.FolderHandling.cs | 293 | OK |
| QuickFiler/Controllers/QfcItemController.Initialization.cs | 497 | OK |
| QuickFiler/Controllers/QfcItemController.ViewerSetup.cs | **500** | OK — at the cap, not over it |
| QuickFiler/Controllers/QfcItemController.cs | 334 | OK |
| QuickFiler/Controllers/QfcItemGroup.cs | 61 | OK |
| QuickFiler/Controllers/QfcQueue.Enqueue.cs | 216 | OK |
| QuickFiler/Controllers/QfcQueue.cs | 505 | Over 500, at or below census 610 |
| QuickFiler/Controllers/QfcStreamingDequeueConfidenceGate.cs | 262 | OK |
| QuickFiler.Test/Controllers/QfcCollectionControllerTests.Part2.cs | 73 | OK |
| QuickFiler.Test/Controllers/QfcCollectionControllerTests.cs | 464 | OK |
| QuickFiler.Test/Controllers/QfcDatamodelTests.cs | 401 | OK |
| QuickFiler.Test/Controllers/QfcFormControllerTests.Part2.cs | 68 | OK |
| QuickFiler.Test/Controllers/QfcFormControllerTests.cs | 792 | Over 500, at or below census 827 |
| QuickFiler.Test/Controllers/QfcHighConfidencePreFilterTests.cs | 363 | OK |
| QuickFiler.Test/Controllers/QfcHomeControllerIssue218Tests.cs | 290 | OK |
| QuickFiler.Test/Controllers/QfcHomeControllerIterationTests.Part2.cs | 101 | OK |
| QuickFiler.Test/Controllers/QfcHomeControllerIterationTests.cs | 477 | OK |
| QuickFiler.Test/Controllers/QfcHomeControllerRunAsyncHighConfidenceTests.Part2.cs | 241 | OK |
| QuickFiler.Test/Controllers/QfcHomeControllerRunAsyncHighConfidenceTests.cs | 333 | OK |
| QuickFiler.Test/Controllers/QfcItemController.FolderHandlingTests.Part2.cs | 241 | OK |
| QuickFiler.Test/Controllers/QfcItemController.FolderHandlingTests.cs | 498 | OK |
| QuickFiler.Test/Controllers/QfcItemController.InitializationTests.cs | 271 | OK |
| QuickFiler.Test/Controllers/QfcQueuePurePathsTests.cs | 413 | OK |
| QuickFiler.Test/Controllers/QfcStreamingDequeueConfidenceGateTests.Part2.cs | 465 | OK |
| QuickFiler.Test/Controllers/QfcStreamingDequeueConfidenceGateTests.Part3.cs | 280 | OK |
| QuickFiler.Test/Controllers/QfcStreamingDequeueConfidenceGateTests.cs | 477 | OK |

33 `.cs` files. Two further paths appear in the diff and carry no row because the audit enumerates
`.cs` files only: `QuickFiler/QuickFiler.csproj` and `QuickFiler.Test/QuickFiler.Test.csproj`.

## Acceptance conditions

### 1. Every `.cs` file listed by the anchored diff has its post-format count recorded

All 33 are in the table.

### 2. No listed file exceeds 500 lines, except a file already over 500 at baseline whose count is at or below its `BASELINE_SIZE_CENSUS` value; and no listed file over 500 lacks a census entry

Thirty of the 33 are at or below 500. Three exceed it, and **all three were already over 500 at
baseline**; each is below its census value, and each is **smaller than it was at the base ref**:

| Path | Census (baseline) | Post-change | Change |
|---|---:|---:|---:|
| QuickFiler/Controllers/QfcCollectionController.cs | 2446 | 2336 | **-110** |
| QuickFiler/Controllers/QfcQueue.cs | 610 | 505 | **-105** |
| QuickFiler.Test/Controllers/QfcFormControllerTests.cs | 827 | 792 | **-35** |

That is the direct consequence of the plan's file-size strategy: every member that had to gain a
parameter was relocated **in full** into a new partial part rather than being extended in place, so
the oversized files shed lines instead of accumulating them.

**No listed file over 500 lacks a `BASELINE_SIZE_CENSUS` entry**, so no census gap is reported. The
audit was written to report such a gap by name rather than treat it as a pass; the check ran and
found none.

`QuickFiler/Controllers/QfcItemController.ViewerSetup.cs` is at exactly **500**, which is at the cap
and not over it. That is the outcome the plan's file-size section predicted for the single statement
P1-T7 adds inside `Cleanup`, which cannot be relocated to another part.

`QuickFiler/Controllers/QfcQueue.cs` at 505 remains five lines over the general limit. It was 610 at
baseline and is not brought under the limit by this change, because doing so would mean relocating a
member this change has no other reason to touch. It is recorded as a confirmed pre-existing defect in
`evidence/other/out-of-scope-register.md`, item 3, and referred for separate promotion.

### 3. Every new file is named with the `<Compile Include>` entry that references it

Seven files were created, all reported as added (`A`) by
`git diff --cached --name-status`:

| New file | `<Compile Include>` entry | In project file |
|---|---|---|
| QuickFiler/Controllers/QfcCollectionController.CarrierLoad.cs | `Controllers\QfcCollectionController.CarrierLoad.cs` | QuickFiler/QuickFiler.csproj |
| QuickFiler/Controllers/QfcQueue.Enqueue.cs | `Controllers\QfcQueue.Enqueue.cs` | QuickFiler/QuickFiler.csproj |
| QuickFiler.Test/Controllers/QfcCollectionControllerTests.Part2.cs | `Controllers\QfcCollectionControllerTests.Part2.cs` | QuickFiler.Test/QuickFiler.Test.csproj |
| QuickFiler.Test/Controllers/QfcFormControllerTests.Part2.cs | `Controllers\QfcFormControllerTests.Part2.cs` | QuickFiler.Test/QuickFiler.Test.csproj |
| QuickFiler.Test/Controllers/QfcHomeControllerIterationTests.Part2.cs | `Controllers\QfcHomeControllerIterationTests.Part2.cs` | QuickFiler.Test/QuickFiler.Test.csproj |
| QuickFiler.Test/Controllers/QfcHomeControllerRunAsyncHighConfidenceTests.Part2.cs | `Controllers\QfcHomeControllerRunAsyncHighConfidenceTests.Part2.cs` | QuickFiler.Test/QuickFiler.Test.csproj |
| QuickFiler.Test/Controllers/QfcItemController.FolderHandlingTests.Part2.cs | `Controllers\QfcItemController.FolderHandlingTests.Part2.cs` | QuickFiler.Test/QuickFiler.Test.csproj |

Both projects use explicit `<Compile Include>` item lists, so a missing entry would silently exclude
the file from compilation. Every entry is present; the P2-T3 analyzer build compiled all seven, and
the P2-T5 run discovered the eight tests they contain.
