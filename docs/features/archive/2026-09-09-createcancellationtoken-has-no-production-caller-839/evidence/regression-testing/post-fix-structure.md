# Post-fix structure — QuickFiler/Controllers/QfcHomeController.cs — issue #839

Timestamp: 2026-09-13T05-51
Command: pwsh -NoProfile -Command 'Set-Location -LiteralPath "REPO-ROOT"; "QFC_LINES=$((Get-Content -LiteralPath QuickFiler/Controllers/QfcHomeController.cs).Count)"'
Command: git -c grep.patternType=fixed grep -n -e "CreateCancellationToken();" -- "QuickFiler/*.cs" "QuickFiler.Test/*.cs"
Command: git diff --numstat 2405a829d6afd3b12eb7c228d57158a97cb4e2ca -- QuickFiler/Controllers/QfcHomeController.cs
Command: git diff 2405a829d6afd3b12eb7c228d57158a97cb4e2ca -- QuickFiler/Controllers/QfcHomeController.cs
EXIT_CODE: 0

## Output Summary

QFC_LINES=499

Numstat line, verbatim (tab-separated): `1` tab `2` tab `QuickFiler/Controllers/QfcHomeController.cs`

Added lines in the anchored diff, excluding the `+++` header. Exactly one, trimmed:

    CreateCancellationToken();

Removed lines in the anchored diff, excluding the `---` header. Exactly two, the first trimmed and the second empty after trimming:

    //public QfcFormViewer FormViewer { get => _formViewer; }
    (one blank line)

The diff carries two hunks. The first, `@@ -85,6 +85,7 @@`, inserts the fix as line 88, immediately after the opening brace of `Init()` and immediately before the datamodel loader call, which moves from line 88 to line 89. The second, `@@ -462,8 +463,6 @@`, removes the dead comment and the blank line below it, leaving exactly one blank line between `private IQfcFormViewer _formViewer;` and `internal void CreateCancellationToken()`.

CMD-FAMILY-CALLS prints exactly 5 lines, one of which is QfcHomeController.cs line 88:

    QuickFiler.Test/Controllers/QfcHomeControllerMetricsTests.cs:124:            controller.CreateCancellationToken();
    QuickFiler/Controllers/EfcHomeController.cs:62:            CreateCancellationToken();
    QuickFiler/Controllers/EfcHomeController.cs:126:            home.CreateCancellationToken();
    QuickFiler/Controllers/EfcHomeController.cs:162:            home.CreateCancellationToken();
    QuickFiler/Controllers/QfcHomeController.cs:88:            CreateCancellationToken();

## Decision D1 reconciliation with AC2

AC2 requires that a single line be removed. The diff removes two lines, and the second is blank. The single NON-BLANK line removed is the dead comment that stood at line 465 of the base file; no executable line is removed, and the file is 499 lines, under the 500-line ceiling.

The blank line was deleted deliberately by hand rather than left to the formatter. Lines 464 and 466 of the base file were both blank, so removing only the comment would leave two consecutive blank lines, which CSharpier collapses to one on the Phase 3 format pass. The diff would then show two removed lines either way; deleting the blank line in the same hand edit makes the result deterministic and makes the file CSharpier-stable at 499 lines before the format gate runs, rather than after it.

Line 465 was chosen over the other dead-line candidate, the commented-out debug log at line 41 inside `LaunchAsync`, because every line number at or above 41 and below 465 is then unchanged, which keeps `Init()` at lines 86 to 107 after the insertion and keeps the inserted statement at line 88 exactly as the coverage and content gates assert.

## Command-transport adaptation

`Set-Location -LiteralPath "REPO-ROOT";` was prepended to the line-count span. Forced by the Bash allowlist and by this executor's inherited working directory, which is a different worktree from the assigned one. The three git spans were addressed to the assigned worktree with a repository-location option in place of a working-directory change; their operands, refs and pathspecs are exactly as the plan writes them and their output is unaffected.
