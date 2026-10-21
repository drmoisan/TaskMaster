# QC Step 1 — CSharpier Format

Timestamp: 2026-09-09T17-14

Command: dotnet tool run csharpier format .

EXIT_CODE: 0

FormattedFileCount: 1623
ChangedFileCount: 0
ConsoleWriteLineDiffLinesAfterFormat: 0

Output Summary: The recorded values are those of the second and final pass of Phase 8, which is the
pass that completed the phase. CSharpier printed "Formatted 1623 files in 1750ms." and exited 0, and
the before-and-after anchored numstat comparison found no path whose insertion or deletion figure
differed, so the formatter rewrote nothing.

## Why the exit code is not the observation

`csharpier format` exits 0 both when it changed nothing and when it repaired drift, so the exit code
alone cannot distinguish a clean run from a repairing one. The falsifiable observation is the
before-and-after tree comparison. It is taken as
`git diff --numstat $b -- . ":(exclude).claude"` with the D3 anchor and the D4 exclusion,
immediately before and immediately after the format, counting the paths whose insertion or deletion
figure differs between the two runs.

A porcelain status is not usable here. Every file this plan edits is already modified before the
format runs and is therefore already listed, and P0-T5 recorded a clean formatter baseline of 1622
checked files with no unformatted file, so no still-clean file could enter the status set either. A
status-set comparison would report zero however much content the formatter rewrote. The numstat form
compares content and is therefore falsifiable, and it did in fact report a change on the first pass.

Each numstat run is preceded by `git add --intent-to-add -- . ":(exclude).claude"`, because a
numstat enumerates tracked changes only and
UtilitiesCS.Test/OutlookObjects/Table/GetTableInViewAsyncClockTests.cs, which P2-T1 created and no
task has yet committed, would otherwise be invisible to both runs and its reflow undetectable.

FormattedFileCount is a processed count rather than a repaired count. It is recorded but is never
used as a restart trigger; the restart trigger is a non-zero ChangedFileCount.

## Pass 1 — restarted the phase

ChangedFileCount was 1 on the first pass and Phase 8 restarted from T1, as the phase preamble
requires. The single path whose content changed was UtilitiesCS/Extensions/DfDeedle.cs, whose
numstat moved from 4 insertions and 4 deletions to 8 insertions and 4 deletions. That is the reflow
the plan predicted: P3-T6 wrote the DfDeedle.cs line 148 call as a single-line
113-column statement, there is no .csharpierrc in this repository, and CSharpier's default 100-column
print width broke it into a four-line call.

The three BuildExplorer call sites in UtilitiesCS.Test/Extensions/DfDeedleEtlTimeoutTests.cs did not
reflow, because P3-T7 and P3-T8 wrote them multi-line rather than at the 105, 102 and 102 columns
the plan anticipated. No other file in the tree was rewritten, which is the consequence of the clean
P0-T5 baseline: pre-existing drift anywhere else would have entered the branch diff and falsified
the P9-T4 Write Set accounting.

## Ownership boundary re-check

P3-T14 ran before any formatter pass, so a reflow introduced by `csharpier format .` could have
falsified AC26 after its check-off at P3-T22. The P3-T14 boundary filter was therefore re-run after
each format pass: using the D3 anchor,
`git diff $b -- UtilitiesCS/OutlookObjects/Table/OlTableExtensions.TableAccess.cs` filtered for
lines matching `^[+-][^+-].*Console\.WriteLine` produced zero lines on both passes. Both diagnostics
survive byte-identical, including their 20-space and 16-space leading indentation.
