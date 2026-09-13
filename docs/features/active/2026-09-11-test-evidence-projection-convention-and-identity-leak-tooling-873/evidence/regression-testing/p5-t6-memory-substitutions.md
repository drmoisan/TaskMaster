# P5-T6 through P5-T10 — Agent-Memory Token Substitutions

Timestamp: 2026-09-13T06-18
Tasks: [P5-T6], [P5-T7], [P5-T8], [P5-T9], [P5-T10]

This artifact records counts only. It contains no account token, no host token, no organization name and
no absolute host path. Both tokens are derived at run time — the account token as the leaf name of the
current user profile directory, the host token as the computer name — exactly as the Phase 0 baseline
task derived them. Neither is hardcoded in any command below or anywhere in this text, and no "before"
value is quoted: an artifact that documented its own substitution with a before column would reintroduce
into a committed file exactly the identifiers it just removed.

## After-state observation, all five files

Command: pwsh -NoProfile -Command '<for each of the five files, read it, derive the account token as the leaf of the user profile directory and the host token as the computer name, count case-insensitive fixed-string matches of each, and count the backtick delimiters on the edited line>'
EXIT_CODE: 0

```
feedback_measure_whole_volume_before_blaming_worktrees.md ACCOUNT=0 HOST=0 EDITED_LINE_BACKTICKS=2 EVEN=True
project_464-review-residuals.md ACCOUNT=0 HOST=0 EDITED_LINE_BACKTICKS=10 EVEN=True
project_488-review-residuals.md ACCOUNT=0 HOST=0 EDITED_LINE_BACKTICKS=16 EVEN=True
collect-pr-context-lands-in-main-checkout.md ACCOUNT=0 HOST=0 EDITED_LINE_BACKTICKS=4 EVEN=True
angle-bracket-redaction-breaks-trx-xml.md ACCOUNT=0 HOST=0 EDITED_LINE_BACKTICKS=0 EVEN=True
```

Every one of the five files returns 0 matches for the account token and 0 for the host token under a
case-insensitive fixed-string search, and every edited line carries an even number of backtick
delimiters, so no inline-code span was left unbalanced.

The Phase 0 baseline recorded these as counts only across fourteen per-file token counts summing to a
value greater than zero, so this after-state check is falsifiable rather than vacuous.

## Per-task record

**[P5-T6]** `.claude/agent-memory/epic-orchestrator/feedback_measure_whole_volume_before_blaming_worktrees.md`,
line 14. The leaked user-profile prefix of a temporary-file path inside an existing inline-code span was
replaced by the `<user-profile>` angle-bracket placeholder, leaving the remainder of the path and the
byte figure intact. Angle-bracket placeholders are safe here because the substitution target is Markdown
prose, not XML markup. Edited line backticks: 2, even.

**[P5-T7]** `.claude/agent-memory/feature-review/project_464-review-residuals.md`, line 17. The leaked
absolute path to the common exclude file inside an existing inline-code span was replaced by the
`<repo-root>` placeholder followed by the repository-relative remainder. Edited line backticks: 10, even.

**[P5-T8]** `.claude/agent-memory/feature-review/project_488-review-residuals.md`, line 13. The leaked
host token inside its own existing inline-code span was replaced by the `<host>` placeholder. The
account placeholder already present on that line was left unchanged, as the task requires. Edited line
backticks: 16, even.

**[P5-T9]** `.claude/agent-memory/orchestrator/collect-pr-context-lands-in-main-checkout.md`, line 10.
The leaked absolute path to the primary checkout's artifacts directory inside an existing inline-code
span was replaced by the `<repo-root>` placeholder followed by the repository-relative remainder. Edited
line backticks: 4, even.

**[P5-T10]** `.claude/agent-memory/orchestrator/angle-bracket-redaction-breaks-trx-xml.md`, line 18. This
one was rewritten rather than token-substituted. The sentence used the account token twice on one line,
once as originally cased and once lowercased, to draw a contrast between a case-sensitive search that
returns clean and a case-insensitive search that finds nineteen files. Substituting the same placeholder
into both positions would have destroyed the contrast, because the two positions would then read
identically and the sentence would assert that the same search both returns clean and finds nineteen
files.

The rewritten sentence describes the two searches instead of quoting either invocation: a case-sensitive
fixed-string search for the account token as originally cased returns clean, while a case-insensitive
search for that same token finds 19 files.

Command: pwsh -NoProfile -Command '<read the rewritten sentence across its full span and count the two contrast words, the file figure, the account token and any repeated angle-bracket placeholder>'
EXIT_CODE: 0

```
CASE_SENSITIVE_WORD_COUNT: 1
CASE_INSENSITIVE_WORD_COUNT: 1
NINETEEN_FILE_FIGURE_COUNT: 1
ACCOUNT_TOKEN_IN_SENTENCE: 0
PLACEHOLDER_COUNT: 0
DUPLICATE_PLACEHOLDER_COUNT: 0
```

The rewritten sentence contains the word case-sensitive and the word case-insensitive, preserves the
nineteen-file figure, contains neither token, and places no placeholder twice — it contains no
angle-bracket placeholder at all, so the same-placeholder-twice condition cannot be violated. The
contrast is carried by the two descriptive phrases rather than by two differently-cased copies of an
identifier.

Output Summary: All five files return 0 case-insensitive matches for both the account token and the host
token. Every edited line's backtick delimiters remain even in number. The rewritten sentence in the fifth
file preserves the case-sensitivity contrast and the nineteen-file figure, contains neither token, and
does not place the same placeholder twice.
