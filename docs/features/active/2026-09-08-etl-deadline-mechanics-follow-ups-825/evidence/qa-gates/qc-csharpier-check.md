# QC Step 2 — CSharpier Check

Timestamp: 2026-09-09T17-15

Command: dotnet tool run csharpier check .

EXIT_CODE: 0

CheckedFiles: 1623

Output Summary: CSharpier printed "Checked 1623 files in 4340ms." and reported no unformatted file,
exiting 0. The read-only check is CI parity for .github/workflows/_format-check.yml, which runs the
manifest-pinned 1.2.6 after `dotnet tool restore`; every invocation in this plan goes through
`dotnet tool run` so no globally installed CSharpier can produce a disagreeing diff.

CheckedFiles is 1623, one greater than the 1622 P0-T5 recorded at baseline. That is the expected
relation: this feature adds exactly one C# file,
UtilitiesCS.Test/OutlookObjects/Table/GetTableInViewAsyncClockTests.cs, and deletes none.
