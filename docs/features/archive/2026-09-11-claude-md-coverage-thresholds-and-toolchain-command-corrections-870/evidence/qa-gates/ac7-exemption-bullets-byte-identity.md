# QA Gate — AC7: the three UT2 exemption bullets are byte-identical to their pre-change text

Timestamp: 2026-09-14T08-17

Execution note: the locating command was run against this worktree. The Bash invocation supplied the worktree root through an explicit `-C` operand, which is equivalent to running it from the worktree root.

Command: `git grep -n -F -- "VSTO add-in lifecycle classes" CLAUDE.md`

EXIT_CODE: 0

Output Summary: one matching line, line 307. The three consecutive exemption bullets therefore occupy lines 307, 308 and 309 after the change; before the change they occupied lines 305, 306 and 307. The downward shift of two lines is exactly the line-count delta introduced by replacing one UT2 coverage line with three, and it is a position change only.

## Post-change text, read at lines 307 through 309

```
    - (a) VSTO add-in lifecycle classes (entry points, ribbon event handlers, COM utility registration) that cannot be unit-tested without a live Outlook process;
    - (b) WinForms form-derived classes and Designer-generated code;
    - (c) Outlook Interop event handler classes in `TaskVisualization`, `QuickFiler`, `TaskMaster`, `ToDoModel`, and `Tags` that directly depend on `Microsoft.Office.Interop.Outlook.Application`, `MailItem`, `Store`, or `MAPIFolder` without an injectable seam.
```

## Comparison result

Compared character for character against the Phase 0 baseline capture at `docs/features/active/2026-09-11-claude-md-coverage-thresholds-and-toolchain-command-corrections-870/evidence/baseline/ut2-exemption-bullets-verbatim.md`.

All three lines are byte-identical to the Phase 0 baseline text, with no difference of any kind: the four-space leading indent, the bullet markers, the parenthesised letters, the prose, the inline-code backtick spans, and the terminal punctuation all match exactly.

Mechanical corroboration: `git diff --stat <base>..HEAD -- CLAUDE.md` reports 8 insertions and 6 deletions across the whole file, EXIT_CODE 0. That total is accounted for entirely by the five intended edit sites (two step 4 entries replaced one-for-one, one UT2 coverage line replaced by three, two analyzer-citation lines replaced one-for-one, and one paragraph replaced one-for-one), leaving no budget for an unintended change to any of the three exemption bullets.

Result: PASS.
