Timestamp: 2026-08-28T19-35
Command: dotnet tool run csharpier check . ; dotnet tool run csharpier format . ; dotnet tool run csharpier check .
EXIT_CODE: 0 (post-format check)
Output Summary: PRE_FORMAT_CHECK_EXIT = 1 (formatter found QuickFiler.Test/Viewers/BreadcrumbDropDownHostTests.Part3.cs
not formatted — the new P2-T1 test's multi-line FluentAssertions chain needed reflow). Format command
rewrote that one file ("Formatted 1560 files in 1324ms" — write-mode summary observed). Post-format
check EXIT_CODE 0 ("Checked 1560 files"). Per the Phase 4 loop rule, this iteration does not count as
final because the format step changed a file; the entire Phase 4 loop restarts from P4-T1. This
artifact is a restart-trigger record only; the final, non-restarted P4-T1 pass is recorded separately.
