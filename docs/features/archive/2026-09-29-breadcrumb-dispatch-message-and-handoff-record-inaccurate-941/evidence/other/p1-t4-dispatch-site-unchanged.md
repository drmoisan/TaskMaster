# P1-T4 Dispatch site (line 101) unchanged

Timestamp: 2026-10-01T07-00
Command: git diff -U0 9b3eea58447c264eae6f95a4bfee3bfcec7fb17f -- QuickFiler/Viewers/BreadcrumbUiDispatcher.cs ; git grep -n -F "cannot marshal cross-thread UI work" -- QuickFiler/Viewers/BreadcrumbUiDispatcher.cs
EXIT_CODE: 0
Output Summary:
- The diff contains exactly one hunk header line, `@@ -183 +183 @@ namespace QuickFiler.Viewers`; its old-range token begins `@@ -183 +183 @@`.
- The search prints exactly one hit, on line 101: `QuickFiler/Viewers/BreadcrumbUiDispatcher.cs:101:` followed by the unchanged literal `"The owner-thread-only test dispatcher cannot marshal cross-thread UI work."`.
- AC2 checked off in FEATURE/issue.md.
