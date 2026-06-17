# Phase 0 — Baseline Line Counts (Cycle 4, #177 / AC25)

Timestamp: 2026-06-16T10-26
Command: `wc -l UtilitiesCS/HelperClasses/FileSystem/FilePathHelper.cs UtilitiesCS.Test/HelperClasses/FilePathHelper_Tests.cs`
EXIT_CODE: 0

Baseline counts:
- `UtilitiesCS/HelperClasses/FileSystem/FilePathHelper.cs` = 494 lines
- `UtilitiesCS.Test/HelperClasses/FilePathHelper_Tests.cs` = 370 lines

Over-cap determination (INV-4):
- `FilePathHelper.cs` is 494 lines, which is UNDER the 500-line cap. NOT a pre-existing overage.
  The minimal 3-line guard will bring it to ~497, still under 500.
- Test file is 370 lines, UNDER the 500-line cap. The ~30-line additions keep it under 500.

Output Summary: Both files under the 500-line cap. FilePathHelper.cs has 6 lines of headroom
before the cap; the minimal guard fits within it. No pre-existing overage.
