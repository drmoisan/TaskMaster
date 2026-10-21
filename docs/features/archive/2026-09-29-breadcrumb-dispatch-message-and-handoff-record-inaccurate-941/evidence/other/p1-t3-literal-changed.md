# P1-T3 DispatchValue literal changed

Timestamp: 2026-10-01T06-58
Command: Edit of the two-line span (var failure = new InvalidOperationException( plus the literal at line 183) in QuickFiler/Viewers/BreadcrumbUiDispatcher.cs; then git grep -n -F per token, [regex]::Matches CRLF/LF count, and git diff --numstat 9b3eea58447c264eae6f95a4bfee3bfcec7fb17f over the file
EXIT_CODE: 0
Output Summary:
- New literal token occurs exactly 1 time, on line 183.
- Token "cannot marshal cross-thread UI work" occurs exactly 1 time, on line 101 (the Dispatch site, untouched).
- Line count 285; CRLF count 285, LF count 285 (all equal).
- git diff --numstat MERGE-BASE: 1 added, 1 deleted, QuickFiler/Viewers/BreadcrumbUiDispatcher.cs.
- AC1 checked off in FEATURE/issue.md.
