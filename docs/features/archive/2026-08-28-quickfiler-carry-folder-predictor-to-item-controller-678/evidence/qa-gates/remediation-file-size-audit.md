# P2-T9 — File-size audit, remediation cycle 1

Timestamp: 2026-09-02T01-41

Run **after** P2-T1, because CSharpier reflow changes line counts; every count below is a
post-format count taken from the tree that passed the final toolchain loop. Counts use
Derivation D8, `(Get-Content -LiteralPath X).Count`; `Measure-Object -Line` and `wc` are not
used.

## Commands

```
git add -A -- QuickFiler QuickFiler.Test
git diff --cached --name-only 4b43e31d042da2b3f670d131bc225fdb30972069 -- QuickFiler QuickFiler.Test
```

The staging step is required first so that files this cycle created are visible to a
name-listing diff, which enumerates tracked changes only.

## Why the ref operand is the cycle HEAD and not the base SHA

The ref operand is `4b43e31d042da2b3f670d131bc225fdb30972069`, the HEAD SHA that P0-T2
recorded, **not** the base SHA `807fb0bb6e5e49f43efa6b256b05960bf078ca19`.

A base-anchored diff lists 33 `.cs` files changed by the previous cycle, three of which are
already over the 500-line cap, and none of the three is edited by this plan or carried in
`R_BASELINE_SIZE_CENSUS`. A base-anchored audit would therefore report three census gaps for
files this cycle neither caused nor is authorised to close.

## Clause 1 and 2 — the listed set in full, with post-format counts

The diff listed exactly eight paths. Seven are `.cs` files and are audited below; the eighth,
`QuickFiler.Test/QuickFiler.Test.csproj`, is a project file and is outside the `.cs`-only
scope of this audit.

| # | Path | Post-format lines | Headroom to 500 | Edited or created by this cycle |
|---|---|---|---|---|
| 1 | `QuickFiler.Test/Controllers/QfcHomeControllerRunAsyncHighConfidenceTests.Part3.cs` | 247 | 253 | created (P1-T1) |
| 2 | `QuickFiler.Test/Controllers/QfcItemController.FolderHandlingTests.Part2.cs` | 354 | 146 | edited (P1-T6) |
| 3 | `QuickFiler/Controllers/QfcDatamodel.QueueProcessing.cs` | 298 | 202 | edited (P1-T4) |
| 4 | `QuickFiler/Controllers/QfcHighConfidencePreFilter.cs` | 301 | 199 | edited (P1-T3) |
| 5 | `QuickFiler/Controllers/QfcHomeController.cs` | **469** | **31** | edited (P1-T3) |
| 6 | `QuickFiler/Controllers/QfcItemController.FolderHandling.cs` | 312 | 188 | edited (P1-T8, P1-T9) |
| 7 | `QuickFiler/Controllers/QfcQueue.Enqueue.cs` | 200 | 300 | edited (P1-T3) |
| — | `QuickFiler.Test/QuickFiler.Test.csproj` | not a `.cs` file | — | edited (P1-T1) |

**Every member of the listed set is a file this cycle edited or created.** No unexpected path
appears.

## Clause 3 — no listed file exceeds 500 lines

The largest is `QuickFiler/Controllers/QfcHomeController.cs` at **469**. No listed file is
over the cap, so the "already over 500 at baseline" branch and the "over 500 with no census
entry" census-gap branch both have empty result sets. No census gap is reported.

## Clause 4 — the lowest-headroom file this cycle edits

**`QuickFiler/Controllers/QfcHomeController.cs`**: post-format count **469**, remaining
headroom **31**.

It was 465 at the P0-T11 baseline and 472 immediately after the P1-T3 edit; CSharpier's pass-1
reflow then collapsed the `ReconcileCarriersToItems(batch.Items, batch.PreScored)` call from
three lines onto one, bringing it to 469. It is the binding constraint because the R1 edit
sits inside the body of the existing `RunAsync` method and cannot be relocated to a new
partial part.

## Clause 5 — the one new file and its `<Compile Include>` entry

New file: `QuickFiler.Test/Controllers/QfcHomeControllerRunAsyncHighConfidenceTests.Part3.cs`.

Entry in `QuickFiler.Test/QuickFiler.Test.csproj` at line 158, quoted verbatim:

```xml
    <Compile Include="Controllers\QfcHomeControllerRunAsyncHighConfidenceTests.Part3.cs" />
```

Both projects use explicit `<Compile Include>` item lists, so this entry is what makes the new
file part of the compilation.

## Pre-existing over-cap paths, recorded so their exclusion is auditable rather than silent

None of the three appears in the listed set above, and none is edited by this plan. They are
out of scope under **NB-6 (pre-existing oversized files)**, which the remediation inputs
explicitly defer out of this cycle.

| Path | Current lines |
|---|---|
| `QuickFiler.Test/Controllers/QfcFormControllerTests.cs` | 792 |
| `QuickFiler/Controllers/QfcCollectionController.cs` | 2336 |
| `QuickFiler/Controllers/QfcQueue.cs` | 505 |

A fourth file, `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs`, sits exactly at the
500-line cap with zero headroom. It is not over the cap and is not edited by this cycle, so it
is neither a violation nor a census gap; it is recorded here because any future addition to it
would have to go into a new partial part with a matching `<Compile Include>` entry.
