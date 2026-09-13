# Pre-fix facts the plan's gates are false-before against — issue #839

Timestamp: 2026-09-13T02-56
Command: pwsh -NoProfile -Command '"QFC_LINES=$((Get-Content -LiteralPath QuickFiler/Controllers/QfcHomeController.cs).Count)"'
Command: pwsh -NoProfile -Command '"TEST_LINES=$((Get-Content -LiteralPath QuickFiler.Test/Controllers/QfcHomeControllerTests.cs).Count)"'
Command: git -c grep.patternType=fixed grep -n -e "CreateCancellationToken()" -- "QuickFiler/*.cs" "QuickFiler.Test/*.cs"
Command: git -c grep.patternType=fixed grep -n -e "CreateCancellationToken();" -- "QuickFiler/*.cs" "QuickFiler.Test/*.cs"
Command: git -c grep.patternType=fixed grep -n -e "void CreateCancellationToken()" -- "QuickFiler/*.cs" "QuickFiler.Test/*.cs"
Command: pwsh -NoProfile -Command '(Get-Content -LiteralPath QuickFiler/Controllers/QfcHomeController.cs)[85..87]'
Command: pwsh -NoProfile -Command '(Get-Content -LiteralPath QuickFiler/Controllers/QfcHomeController.cs)[464..466]'
Command: git -c grep.patternType=fixed grep -n -e "#nullable" -- QuickFiler/Controllers/QfcHomeController.cs
Command: pwsh -NoProfile -Command '"TEST_EXISTS=$(Test-Path -LiteralPath QuickFiler.Test/bin/Debug/QuickFiler.Test.dll)"'
EXIT_CODE: 0

Output Summary:
- QFC_LINES=500. Exit code of this invocation, the one recorded in the EXIT_CODE field above: 0. The file sits exactly at the repository 500-line ceiling before the change.
- TEST_LINES=275. TEST_LINECOUNT_EXIT=0.
- CMD-FAMILY-ALL printed exactly 6 lines. FAMILY_ALL_EXIT=0. The lines, verbatim:
  QuickFiler.Test/Controllers/QfcHomeControllerMetricsTests.cs:124:            controller.CreateCancellationToken();
  QuickFiler/Controllers/EfcHomeController.cs:62:            CreateCancellationToken();
  QuickFiler/Controllers/EfcHomeController.cs:126:            home.CreateCancellationToken();
  QuickFiler/Controllers/EfcHomeController.cs:162:            home.CreateCancellationToken();
  QuickFiler/Controllers/EfcHomeController.cs:399:        internal void CreateCancellationToken()
  QuickFiler/Controllers/QfcHomeController.cs:467:        internal void CreateCancellationToken()
  The set matches the expected baseline: EfcHomeController.cs at 62, 126, 162 and 399; QfcHomeController.cs at 467; QfcHomeControllerMetricsTests.cs at 124.
- CMD-FAMILY-CALLS printed exactly 4 lines. FAMILY_CALLS_EXIT=0. They are QfcHomeControllerMetricsTests.cs 124 and EfcHomeController.cs 62, 126 and 162. None is in QfcHomeController.cs, which is the defect this item fixes: the Qfc controller declares the factory and invokes it nowhere.
- CMD-FAMILY-DECLS printed exactly 2 lines. FAMILY_DECLS_EXIT=0. They are EfcHomeController.cs 399 and QfcHomeController.cs 467.
- Lines 86 to 88 of the production controller, trimmed: `public IQfcHomeController Init()`, `{`, `_datamodel = QfcDataModelLoader(Globals, this.Token);`. LINES_86_88_EXIT=0. The datamodel loader is the first statement of the method body before the fix, which is what makes the insertion position load-bearing.
- Lines 465 to 467 of the production controller, trimmed: `//public QfcFormViewer FormViewer { get => _formViewer; }`, an empty line, `internal void CreateCancellationToken()`. LINES_465_467_EXIT=0. This confirms the Decision D1 deletion target and the blank line immediately below it.
- The `#nullable` search printed nothing and NULLABLE_GREP_EXIT=1. The non-zero code is the deliberate no-match outcome, not a failure, and confirms Decision D13: the file is in an oblivious nullable context before the change.
- TEST_EXISTS=True. TEST_EXISTS_EXIT=0. The QuickFiler.Test assembly produced by the [P0-T16] rebuild is present for [P0-T18].
- Every acceptance value matched the value re-derived at plan time, so the tree has not moved and no halt is required.
- This artifact carries exactly one EXIT_CODE row, the exit code of the QFC_LINES invocation, because that field is per-file; every other invocation's exit code is recorded above as a named line inside this summary.
