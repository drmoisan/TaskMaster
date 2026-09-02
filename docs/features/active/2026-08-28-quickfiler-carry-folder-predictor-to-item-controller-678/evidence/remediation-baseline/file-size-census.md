# Baseline — File-size census (`R_BASELINE_SIZE_CENSUS`)

- Timestamp: 2026-09-02T01-10
- Issue: #678
- Task: [P0-T11]
- Derivation: D8, `(Get-Content -LiteralPath X).Count`

`Measure-Object -Line` reports a different value on a file without a trailing newline and is
not used for the 500-line cap. `wc` is likewise not used.

## `R_BASELINE_SIZE_CENSUS`

| Path | Lines | Headroom to 500 |
|---|---|---|
| `QuickFiler/Controllers/QfcHighConfidencePreFilter.cs` | 228 | 272 |
| `QuickFiler/Controllers/QfcQueue.Enqueue.cs` | 216 | 284 |
| `QuickFiler/Controllers/QfcHomeController.cs` | 465 | **35** |
| `QuickFiler/Controllers/QfcDatamodel.QueueProcessing.cs` | 292 | 208 |
| `QuickFiler/Controllers/QfcItemController.FolderHandling.cs` | 293 | 207 |
| `QuickFiler.Test/Controllers/QfcItemController.FolderHandlingTests.Part2.cs` | 241 | 259 |
| `QuickFiler.Test/Controllers/QfcHomeControllerRunAsyncHighConfidenceTests.cs` | 333 | 167 |

## Lowest-headroom path this cycle edits

**`QuickFiler/Controllers/QfcHomeController.cs`**, at 465 lines, headroom **35**.

It is the binding constraint because the R1 edit sits inside the body of the existing
`RunAsync` method (the assignment at `:307`) and cannot be relocated to a new partial part.
Every other production edit this cycle makes is either an in-place rewrite of existing lines
or an addition to a file with more than 200 lines of headroom. P1-T3 therefore carries an
explicit at-most-500 acceptance clause for this file, and P2-T9 re-measures it after
CSharpier reflow has settled.

## Files at or over the cap that this plan does not edit

Recorded so their exclusion is auditable rather than silent. None appears in the census
above and none is edited by this plan:

| Path | Lines | Status |
|---|---|---|
| `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs` | 500 | at the cap, zero headroom, not edited |
| `QuickFiler/Controllers/QfcQueue.cs` | 505 | over the cap, pre-existing (NB-6), not edited |
| `QuickFiler/Controllers/QfcCollectionController.cs` | 2336 | over the cap, pre-existing (NB-6), not edited |
| `QuickFiler.Test/Controllers/QfcFormControllerTests.cs` | 792 | over the cap, pre-existing (NB-6), not edited |

Any addition that would otherwise land in one of these four would go into a new partial part
with a matching `<Compile Include>` entry. Both `QuickFiler.csproj` and
`QuickFiler.Test.csproj` use explicit `<Compile Include>` item lists, so every new `.cs` file
requires an entry.

## Project file, deliberately without a census row

`QuickFiler.Test/QuickFiler.Test.csproj` **is** edited by this plan: P1-T1 adds one
`<Compile Include>` entry for the new test part. It deliberately carries no census row,
because the P2-T9 audit enumerates `.cs` files only and the 500-line cap in
`.claude/rules/general-code-change.md` applies to production code, test code and reusable
script files rather than to project files.

## Output Summary

Seven paths measured with Derivation D8. `QuickFiler/Controllers/QfcHomeController.cs` at
465 lines is the lowest-headroom path this cycle edits, with 35 lines of headroom. No census
path is at or over the 500-line cap. Four pre-existing at-or-over-cap paths are recorded as
out of scope; `QuickFiler.Test/QuickFiler.Test.csproj` is edited but carries no row by
design.
