# P1-T1 — Split the Tlp Manipulation region into QfcQueue.Tlp.cs

Timestamp: 2026-09-13T15-15
Command: (Get-Content -LiteralPath <P>).Count over the two paths below, plus a literal search for `#region Tlp Manipulation` over QuickFiler/Controllers
EXIT_CODE: 0
Output Summary: The whole Tlp Manipulation region moved verbatim out of the base part into a new
partial part. The region literal now occurs exactly once in the tree, in the new file. The base
part's measured line count fell from the 507 P0-T13 recorded to 284.

## Recorded anchor

BASE_SHA: 8213826f695439e86e3ed34faa575de493a11ec7

## Region literal occurrences

- `QuickFiler/Controllers/QfcQueue.cs` occurrences of `#region Tlp Manipulation`: 0
- `QuickFiler/Controllers/QfcQueue.Tlp.cs` occurrences of `#region Tlp Manipulation`: 1 (line 30)

## Measured line counts (CMD-LINECOUNT)

- QuickFiler/Controllers/QfcQueue.cs = 284
- QuickFiler/Controllers/QfcQueue.Tlp.cs = 255

P0-T13 recorded 507 for QuickFiler/Controllers/QfcQueue.cs. 284 is strictly less than 507, which is
the acceptance condition of this task.

## Verbatim-move verification

Lines 230 through 453 inclusive of QuickFiler/Controllers/QfcQueue.cs at the recorded anchor were
compared line by line, in order, against lines 30 through 253 inclusive of the new file, using
Compare-Object with a sync window of zero over the anchor content read with `git show`. The result
was no differences across all 224 lines.

Verification output: `VERBATIM-MOVE: IDENTICAL (224 lines)`

## Breadcrumb left in the base part

A single comment line replaces the relocated region in the base part, in the style of the two
breadcrumb comments the base file already carried for the enqueue part:

```
        // The Tlp Manipulation region lives in the partial part QfcQueue.Tlp.cs; see that file.
```

## Using directives

The new file carries the full directive set of QuickFiler/Controllers/QfcQueue.cs at the recorded
anchor, all sixteen directives, unmodified. Per the task text, directives are removed only when the
analyzer gate in P1-T5 reports them as unnecessary. No nullable pragma was added.
