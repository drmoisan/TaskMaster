# Baseline — CSharpier Formatting (Issue #208, [P0-T2])

Timestamp: 2026-07-09T09-29

Command: dotnet tool run csharpier check .
(Plan stated `dotnet tool run csharpier --check .`; the repo-pinned CSharpier is v1.2.6, whose
read-only verify uses the `check <directoryOrFile>` subcommand. The v0 `--check .` syntax is not
recognized by v1 and prints usage help. The v1 `check .` command captures the same baseline
formatting state the task intends.)

EXIT_CODE: 0

Output Summary: PASS. CSharpier checked 1313 files in ~2.7s. 0 files unformatted; no formatting
changes required. Baseline formatting state is clean before any Phase 1 edits.
