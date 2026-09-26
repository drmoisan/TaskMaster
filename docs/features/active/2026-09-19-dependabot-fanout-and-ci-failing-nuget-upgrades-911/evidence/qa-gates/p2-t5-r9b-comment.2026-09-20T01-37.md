# R9b — Call-Site Comment on the Unfiltered `Get-AnalyzerAssemblyPath`

- Timestamp: 2026-09-20T08-51-21
- Task: [P2-T5]
- Finding: R9b
- EXIT_CODE: 0

## The Comment Added

In `scripts/dependencies/Repair-PackageManifestConsistency.ps1`, immediately above the unfiltered
invocation:

```
            # Called without -PreservedSegment, so the result is a verification membership set: every consumable analyzer assembly in every Roslyn folder the package ships, consumed only by the -contains test below and never written to a project file.
            # A caller wanting a writable path supplies -PreservedSegment, which confines the result to the folder the item already names.
```

All three statements the plan requires are present: the unfiltered result is a **verification
membership set**; it is consumed only by the `-contains` test below and must never be written to a
project file; and a caller wanting a writable path supplies `-PreservedSegment`.

## The Call Site Was Located by Text, Not by Line Number

The review cited **line 320**. That citation no longer names this call site: [P2-T1] removed 36
lines above it, so the same code now sits 34 lines earlier. Locating it by the review's number
would have placed the comment beside unrelated code, which is **gate rule 14**'s class reached
through a positional citation.

The call site was located by its literal text instead:

```
Select-String -SimpleMatch 'Get-AnalyzerAssemblyPath -PackageId $identity.Id'
```

**Exactly 1 hit.**

## Placement Measurement

| Measurement | Value |
|---|---|
| Line carrying `verification membership set` | **284** |
| Line carrying `Get-AnalyzerAssemblyPath -PackageId $identity.Id` | **286** |
| Distance, call line minus comment line | **2** |
| Within 3 lines above | **yes** |
| `Select-String -SimpleMatch 'verification membership set'` hits | **1** |

The distance check is what fails if the comment lands beside a different call site. A comment
placed correctly in prose but 40 lines from the invocation would satisfy a bare presence check and
would not satisfy this one.

## A First Placement Failed This Check and Is Recorded

The first form of the comment was six lines long, and the line carrying the phrase
`verification membership set` sat **6** lines above the call rather than within 3. The measurement
reported `WITHIN3=False`. The comment was recompressed to two lines with the phrase on the first,
putting the distance at 2. No content was dropped: all three required statements survive the
compression.

This is recorded rather than silently corrected because the failing measurement is the evidence
that the distance clause is a live gate and not a restatement of the presence clause.

## File Size

`scripts/dependencies/Repair-PackageManifestConsistency.ps1` measures **464** lines, against its
[P0-T5] baseline of 498 and the 500-line cap. The file is now 36 lines below where R9d found it,
because [P2-T1]'s extraction removed more than this comment adds.

## Why the Unfiltered Mode Is Safe Here and Still Worth Commenting

The review recorded this as Minor and explicitly not as an active defect: the result is used only
as a membership set for a `-contains` test on line 288, and nothing writes from it. What the
review objected to is that the **contract permits** producing a full derived path set, which is
the shape a future caller could mistake for a selection. The preserve rule is the load-bearing
invariant of this change, so the one function capable of producing a non-preserved path should be
hard to misuse. The comment is the discharge the review offered as its second option.

## Output Summary

One comment, two lines, placed 2 lines above the call site located by literal text. Exactly one
occurrence of the phrase in the file. The composition root measures 464 lines.
