Timestamp: 2026-09-03T14-25
AC8 verification.

Command: git diff --name-only 687f15fbf164d5aeff044a5ec17de18bc8622b27 -- ":(exclude).claude"
(BASE_SHA substituted from evidence/baseline/p0-t7-base-ref.md)
EXIT_CODE: 0

Result: 360 paths returned (includes the prior reconciliation merge of origin/main into this branch, plus this plan's own two footprint files). Searched the full returned list for the two excluded caller paths:
- TaskMaster/AppGlobals/AppOlObjects.cs — NOT FOUND
- QuickFiler/Controllers/QfcHomeController.Metrics.cs — NOT FOUND

Output Summary: Neither excluded caller path appears in the diff. AC8 (neither production caller requires a code change) confirmed.
