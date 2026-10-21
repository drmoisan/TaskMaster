# P4-T2 — Formatter verification in read-only check mode

Timestamp: 2026-09-13T03-04

Canonical CLAUDE.md command text: `dotnet tool run csharpier check .`

Command: `pwsh -NoProfile -Command '$out = & ".\.dotnet-sdk\dotnet.exe" tool run csharpier check . 2>&1; $code = $LASTEXITCODE; $out; $errs = @($out | Where-Object { "$_".StartsWith("Error") }).Count; "CHECK_EXIT=$code ERROR_LINES=$errs"; exit $code'`

EXIT_CODE: 0

CHECK_EXIT=0 ERROR_LINES=0

The check printed `Checked 1626 files in 5401ms.` and reported no line beginning with `Error`.

Output Summary: both acceptance clauses hold. This is the first of the four CLAUDE.md toolchain steps in the final clean pass. The check command is read-only and exits non-zero on drift, so its exit code is a falsifiable signal on its own, unlike the write-mode pass in P4-T1 whose exit code is identical on a clean run and a repairing one. No processed-file count is asserted. The file count of 1626 is two higher than the 1624 P0-T16 observed, which accounts for exactly the two C# files this change creates.
