# QC Format — CSharpier (P12-T1)

Timestamp: 2026-07-19T11-57

Command: `dotnet tool run csharpier format .` (repo root; then `dotnet tool run csharpier check .`)

EXIT_CODE: 0

Output Summary:
- First `format` pass: Formatted 1406 files; it reflowed 2 opted-in files (OneDriveDownloader.cs
  wrapped `_clientGetAsync = null!` to a new line; FileIO2.cs wrapped `CsvRead` parameters). These
  are formatting-only reflows of annotation edits. Per the toolchain loop, the loop restarts after a
  format change.
- Restart: `csharpier format .` re-run produced no further content changes; `csharpier check .` exits 0
  (all 1406 files already formatted). Format stage is clean and idempotent.
