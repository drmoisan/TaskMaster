# AC23 — The Two Classes That Keep Their DoNotParallelize Attributes Are Untouched

Timestamp: 2026-09-09T17-12

Command: Select-String over the three named test files for [DoNotParallelize], plus git diff $b -- <file> for each, with $b re-derived from evidence/baseline/base-commit.md per D3

EXIT_CODE: 0

## Assertions

1. UtilitiesCS.Test/OutlookObjects/Table/OlTableExtensionsEtlClockTests.cs contains exactly one
   occurrence of [DoNotParallelize], at line 21. PASS.
2. Its reason comment is intact at line 20 and reads
   `// Not parallelized: this class drives a real Task.Run gate.` PASS.
3. UtilitiesCS.Test/Extensions/DfDeedleEtlTimeoutTests.cs contains exactly one occurrence of
   [DoNotParallelize], at line 24, with its reason comment intact at lines 22 and 23 reading
   `// Not parallelized: this class drives a real Task.Run gate and shares the DfDeedle logger,` and
   `// exactly as DfDeedleQfcColumnTimeoutTests does.` PASS.
4. The anchored diff for each of those two files contains zero added and zero removed lines matching
   DoNotParallelize. PASS. This holds for DfDeedleEtlTimeoutTests.cs even though the file is edited
   by this feature: the P3-T7 and P3-T8 timer-ordering update touches the BuildExplorer declaration,
   its GetTable setup, the three call sites and the gate and re-arm lines inside one test, and none
   of those is the attribute or its comment.
5. UtilitiesCS.Test/OutlookObjects/Table/OlTableExtensionsRetryTests.cs still contains zero
   occurrences of [DoNotParallelize]. PASS. It never carried the attribute and this feature adds
   none.

Output Summary: All five assertions pass. Both classes state accurately that they drive a real
Task.Run gate, which remains true after this change, so both keep their attributes and their
comments. The attribute removed by this feature is the one on OlTableExtensions_Tests, whose stated
reason was factually wrong and whose actual hazard the item 2 change eliminated; that removal is
justified separately at evidence/other/ac21-justification.md.
