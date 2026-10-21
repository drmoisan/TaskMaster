# [P1-T3] — PA-7 Instance 2 Redacted (policy audit, line 482)

Timestamp: 2026-08-29T23-23
Run performed: 2026-08-30T01-17
Task: [P1-T3]
File: `docs/features/active/2026-08-27-qfc-unregister-navigation-count-mismatch-orphan-644/policy-audit.2026-08-29T23-06.md`, line 482
EXIT_CODE: 0

Redaction note: this artifact reproduces no raw absolute-path text and no account token. The
redacted content is described generically.

## The edit

Line 482 is the `- **Content:**` citation inside the PA-7 finding. It was a Markdown bullet of
the form `` - **Content:** `- Worktree: \`<value>\`` `` whose value was the same Windows
absolute path described in `[P1-T2]` — drive letter, `Users`, the account name, the repository
path, and `.claude/worktrees/<agent-worktree-id>`, joined with forward slashes.

The entire line was replaced with exactly the text the plan mandates. The plan's fenced
replacement carries two spaces of Markdown indentation; that indent was stripped before
applying, so the replacement begins at column 1.

Line 482 on disk after the edit, verbatim:

```
- **Content:** `- Worktree: \`<repo-root>/.claude/worktrees/<agent-worktree>\``
```

The nested backslash-escaped backticks are preserved, so the bullet still renders as a quoted
citation of the research artifact's line.

## Scope containment

One line was replaced by one line. The `Location:` lines at 480-481 and the `Verification:`,
`Standing:`, `Why non-blocking:`, and `Recommendation:` lines at 484-491 were not touched by
this task, and no finding, verdict, or measured figure anywhere else in the document was
altered. The file is 634 lines after the edit.

Line-numbering invariant verified directly: reading lines 480-484 after the edit shows line 483
is still the `- **Verification:**` bullet, so `[P1-T4]`'s citation of line 483 remains correct.
The `Verification:` line at 483 is remediated by `[P1-T4]`, not by this task.

## Acceptance clauses

Both clauses hold.

### Clause 1 — shape pattern eliminated from this file

Command: `@(Select-String -Path docs\features\active\2026-08-27-qfc-unregister-navigation-count-mismatch-orphan-644\policy-audit.2026-08-29T23-06.md -Pattern '[A-Za-z]:[\\/]Users[\\/]').Count`
EXIT_CODE: 0
Before: `1`   After: `0`   Required: `0`   Result: PASS

This clause is satisfiable after this task alone even though line 483 still carries an account
token at this point. Line 483 writes its path with doubled backslashes, and the shape pattern's
`[\\/]` class matches exactly one separator character, so line 483 has never been a
shape-pattern match. `[P0-T3]` recorded this directly: the shape pattern matched line 482 only,
while the account-token pattern matched both 482 and 483.

The regular expression delivered to the search engine was verified for case-sensitive equality
against a shell-independent construction from `[char]92` before the run, confirming it is
character-for-character the 24-character pattern the plan mandates.

### Clause 2 — the line still exists, redacted rather than deleted

Command: `@(Select-String -Path docs\features\active\2026-08-27-qfc-unregister-navigation-count-mismatch-orphan-644\policy-audit.2026-08-29T23-06.md -SimpleMatch -Pattern '- **Content:**').Count`
EXIT_CODE: 0
Before: `1`   After: `1`   Required: `1`   Result: PASS

## Supplementary observation

Account-token pattern over this file after this edit: `1` remaining match, at line 483. This is
the expected residual — PA-7 instance 3 — and is remediated by `[P1-T4]`. The count was `2` for
this file at the `[P0-T3]` baseline (lines 482 and 483).

## Output Summary

PA-7 instance 2 redacted to the generic form. Both acceptance clauses pass: the shape-pattern
count for this file fell from `1` to `0`, and the `- **Content:**` bullet still exists, redacted
rather than deleted. One line was replaced by one line, so line 483 keeps its number and
`[P1-T4]`'s citation stays correct. No finding, verdict, or measured figure was altered.
