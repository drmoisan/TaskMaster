# Phase 1 — Post-Split File Line Counts (P1-T6)

Timestamp: 2026-07-20T22-58

Command: `wc -l` over the four resulting test files.

EXIT_CODE: 0

Output Summary (R1 remediated — all four files < 500 lines):
- UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbStateModelTests.cs = 320 lines (was 536; kept shared
  helpers + Positive/Negative/Edge-case groups; now `public sealed partial class`).
- UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbStateModelSequenceTests.cs = 235 lines (new partial;
  State-transition-sequence group + #398 ReplaceRows group with its PlainRows helper).
- UtilitiesCS.Test/OutlookObjects/Folder/FolderBreadcrumbBridgeRouterTests.cs = 314 lines (was 545; kept
  shared helpers + Positive-routing/Negative-routing/Edge-fall-through groups; now
  `public sealed partial class`).
- UtilitiesCS.Test/OutlookObjects/Folder/FolderBreadcrumbBridgeRouterInFlightTests.cs = 256 lines (new
  partial; multi-message sequence group + misc constructor/null/plain-row tests + #398 in-flight rebuild
  invariant group with SecondPath/SecondKey/GatedTwoRowProvider/TwoScoredRows helpers).

Every original test method exists in exactly one file after the split (no duplicates, no losses). Shared
helpers remain present exactly once per class. The two new files are wired into UtilitiesCS.Test.csproj
via explicit `<Compile Include>` items adjacent to their sibling entries; the csproj retains CRLF line
endings and only two lines were inserted (existing entries unchanged).
