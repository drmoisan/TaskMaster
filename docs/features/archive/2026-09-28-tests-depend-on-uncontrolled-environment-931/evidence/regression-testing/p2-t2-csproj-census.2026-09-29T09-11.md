# P2-T2 Project-File Census

Timestamp: 2026-09-29T09-11
Command: CMD-CENSUS with PATH = QuickFiler.Test/QuickFiler.Test.csproj; git diff --numstat HEAD -- QuickFiler.Test/QuickFiler.Test.csproj; git status --porcelain -- QuickFiler.Test/QuickFiler.Test.csproj
EXIT_CODE: 0

Output Summary:
- Edits applied exactly as Target Source F states (Edit tool; no PreToolUse refusal): the Part2 entry inserted immediately after the primary affinity entry (line 98), the helper entry inserted immediately after TestSupport\WinFormsPumpHost.cs.
- INCLUDE-AFFINITY 1, INCLUDE-PART2 1, INCLUDE-HELPER 1 - HOLD.
- numstat: 2 added, 0 deleted - HOLDS.
- porcelain begins " M" for the project file - HOLDS.

## Project-file TOKEN lines

    TOKEN Include="Viewers\ItemViewerBreadcrumbThreadAffinityTests.cs" = 1
    TOKEN Include="Viewers\ItemViewerBreadcrumbThreadAffinityTests.Part2.cs" = 1
    TOKEN Include="TestSupport\DedicatedWorkerThread.cs" = 1
    LINES = 570
    SHA256 = 5538F090CDBC43F57E03000C1776D9881E147302E513D1D03D58C6102E4DC39F

## numstat

    2	0	QuickFiler.Test/QuickFiler.Test.csproj

## porcelain

     M QuickFiler.Test/QuickFiler.Test.csproj
