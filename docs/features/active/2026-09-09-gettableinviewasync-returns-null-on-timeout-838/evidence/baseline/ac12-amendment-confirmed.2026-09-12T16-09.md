# P0-T7 — Acceptance criterion 12 amendment and criterion inventory

Timestamp: 2026-09-13T00-48

Command: `pwsh -NoProfile -Command '$f = "docs/features/active/2026-09-09-gettableinviewasync-returns-null-on-timeout-838/spec.md"; $a = @(Select-String -LiteralPath $f -SimpleMatch -CaseSensitive -Pattern "GetTableInViewAsync_AbsorbedDefaultWithOuterTokenCancelled_ThrowsOperationCanceledNotTimeout").Count; $b = @(Select-String -LiteralPath $f -CaseSensitive -Pattern "^- \[ \] [0-9]+\. ").Count; "AC12_VERIFIER_COUNT=$a"; "AC_UNCHECKED_COUNT=$b"; if ($a -ge 1 -and $b -eq 16) { exit 0 } else { exit 1 }'`

Execution note: invoked with a leading `Set-Location` to the item worktree root, which the plan assumes is the current directory. No absolute path is transcribed.

EXIT_CODE: 0

Output Summary:

```
AC12_VERIFIER_COUNT=1
AC_UNCHECKED_COUNT=16
```

- The amended criterion-12 verifier name `GetTableInViewAsync_AbsorbedDefaultWithOuterTokenCancelled_ThrowsOperationCanceledNotTimeout` is present in `spec.md` exactly once, which satisfies the at-least-one requirement. The executor is therefore judged against criterion 12 in its amended form.
- The count of unchecked numbered acceptance-criteria lines in `spec.md` is 16, which matches the count the plan's check-off tasks P4-T21 through P4-T36 and its acceptance-criteria mapping are written against.
- Both clauses passed, so the run continues. Neither halt condition of this task fired.
