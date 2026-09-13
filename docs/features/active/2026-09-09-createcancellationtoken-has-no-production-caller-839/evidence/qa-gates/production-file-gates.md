# Production-file content gates (AC1, AC2, Decision D13) — issue #839

Timestamp: 2026-09-13T06-17
Command: pwsh -NoProfile -Command 'Set-Location -LiteralPath "REPO-ROOT"; (Get-Content -LiteralPath QuickFiler/Controllers/QfcHomeController.cs)[85..88]'
Command: git -c grep.patternType=fixed grep -n -e "CreateCancellationToken();" -- QuickFiler/Controllers/QfcHomeController.cs
Command: git diff --numstat 2405a829d6afd3b12eb7c228d57158a97cb4e2ca -- QuickFiler/Controllers/QfcHomeController.cs
Command: git diff 2405a829d6afd3b12eb7c228d57158a97cb4e2ca -- QuickFiler/Controllers/QfcHomeController.cs
Command: git -c grep.patternType=fixed grep -n -e "#nullable" -- QuickFiler/Controllers/QfcHomeController.cs
EXIT_CODE: 0

## Output Summary

Lines 86 to 89, trimmed, in order. This is the AC1 position assertion:

    public IQfcHomeController Init()
    {
        CreateCancellationToken();
        _datamodel = QfcDataModelLoader(Globals, this.Token);

The inserted statement is the first statement of `Init()`, immediately after the opening brace and before the datamodel loader call, which is the remedy the spec fixes.

Invocation grep, exactly one line, and it is line 88:

    QuickFiler/Controllers/QfcHomeController.cs:88:            CreateCancellationToken();

INVOCATION_GREP_EXIT=0
NULLABLE_GREP_EXIT=1

Numstat line, verbatim (tab-separated): `1` tab `2` tab `QuickFiler/Controllers/QfcHomeController.cs`

One line added and two removed. The single added line satisfies AC1's requirement that no other statement was added or reordered; the two removed lines satisfy AC2's single-dead-comment removal, with the Decision D1 reconciliation recorded below.

Full anchored diff, added lines excluding the `+++` header. Exactly one, trimmed:

    CreateCancellationToken();

Full anchored diff, removed lines excluding the `---` header. Exactly two, the first trimmed and the second empty after trimming:

    //public QfcFormViewer FormViewer { get => _formViewer; }
    (one blank line)

These are the same two hunks recorded in evidence/regression-testing/post-fix-structure.md, re-measured here after the clean format pass recorded by [P3-T6]. They are unchanged by that pass, consistent with it reporting `FORMAT_CHANGED_OWNED_PATCH=False`.

## Decision D1 reconciliation with AC2

AC2 requires a single line removed. The diff removes two, and the second is blank. The single NON-BLANK line removed is the dead comment that stood at line 465 of the base file. No executable line is removed. The blank line was deleted in the same hand edit deliberately, because base lines 464 and 466 were both blank and removing only the comment would have left two consecutive blank lines for CSharpier to collapse, making the diff non-deterministic and forcing a format-loop restart. The [P3-T1] clean pass confirms the hand edit matched formatter-canonical output.

## Decision D13, nullable

The `#nullable` search prints nothing and exits 1, recorded above as `NULLABLE_GREP_EXIT=1` inside this summary rather than as a second `EXIT_CODE:` row, because that field is per-file and a second row would make this artifact's normalized result ambiguous. The single `EXIT_CODE: 0` is the exit code of the `CreateCancellationToken();` invocation search.

The file carried no `#nullable` directive before this diff and carries none after, so it does not participate in nullable flow analysis and the inserted statement gains no `CS86xx` obligation under `/p:TreatWarningsAsErrors=true`. That is consistent with the [P3-T3] result `CS86_ERROR_LINES=0`.

## Command-transport note

`Set-Location -LiteralPath "REPO-ROOT";` was prepended to the line-range span, and the two `git grep` invocations were run inside one `pwsh -NoProfile -Command` invocation so that each one's exit code could be captured alongside its output. Forced by the Bash allowlist and by this executor's inherited working directory, which is a different worktree from the assigned one. Each grep retains the plan's pinned fixed-string engine, the `-n` switch, the `-e` token operand and the single pathspec exactly as written. The two `git diff` spans were addressed to the assigned worktree with a repository-location option; their refs, operands and pathspecs are exactly as the plan writes them.
