# P5-T11 CR-1 Edits to the #956 Spec (D9)

Timestamp: 2026-10-03T12-32
Command: Edits E-SPEC956-99, E-SPEC956-149, E-SPEC956-155 (in that order, Edit tool) on docs/features/active/2026-10-01-sort-email-oversized-with-untestable-io-and-dialog-paths-956/spec.md; then CMD-SPEC-CHECK (STAGE final), run as pwsh -NoProfile -Command with Set-Location to the item worktree; then git diff --numstat 94287369908cc920b21b0e3256314f988ad7d2f5 -- docs/features/active/2026-10-01-sort-email-oversized-with-untestable-io-and-dialog-paths-956/spec.md; then git status --porcelain -- docs/features/active/2026-10-01-sort-email-oversized-with-untestable-io-and-dialog-paths-956 (each git command issued as git -C WORKTREE, before this task's commit)
EXIT_CODE: 0 (scoped to the CMD-SPEC-CHECK payload, its process exit code)
Output Summary: the three CR-1 replacements are present once each and the two replaced texts are absent; the #956 check-box counts are unchanged (17 checked, 0 unchecked); the file grew by exactly two lines (313 to 315); numstat 4 added, 2 deleted; only spec.md is modified under the #956 folder. P5-T11 acceptance met.

## CMD-SPEC-CHECK output (STAGE final)

```
WORKMODE-LINES: 1
AC-HEADING-LINES: 1
AC-UNCHECKED: 27
AC-CHECKED: 0
AC6-UNCHECKED: 1
AC27-UNCHECKED: 1
AC-ANY-UNCHECKED: 27
USERSTORY-EXISTS: False
AC15-SEAM-STEP: 3
AC6-RESET-LITERAL: 2
AC25-NINETY: 2
S956-OLD-99: 0
S956-NEW-99: 1
S956-149: 1
S956-NEW-149: 1
S956-OLD-155: 0
S956-NEW-155: 1
S956-AC-CHECKED: 17
S956-AC-UNCHECKED: 0
S956-LINES: 315
```

## Numstat (git diff --numstat 94287369908cc920b21b0e3256314f988ad7d2f5 -- docs/features/active/2026-10-01-sort-email-oversized-with-untestable-io-and-dialog-paths-956/spec.md)

```
4	2	docs/features/active/2026-10-01-sort-email-oversized-with-untestable-io-and-dialog-paths-956/spec.md
```

## Porcelain (git status --porcelain -- docs/features/active/2026-10-01-sort-email-oversized-with-untestable-io-and-dialog-paths-956)

```
 M docs/features/active/2026-10-01-sort-email-oversized-with-untestable-io-and-dialog-paths-956/spec.md
```

## Acceptance (P5-T11, all five required)

1. S956-OLD-99: 0, S956-NEW-99: 1, S956-149: 1, S956-NEW-149: 1, S956-OLD-155: 0, S956-NEW-155: 1: met.
2. S956-AC-CHECKED 17 equals S956-AC-CHECKED-BASE 17 and S956-AC-UNCHECKED 0 equals S956-AC-UNCHECKED-BASE 0 (P0-T2): met.
3. S956-LINES 315 equals S956-LINES-BASE 313 plus 2: met.
4. The numstat line reads 4, 2 and the path: met.
5. The porcelain output lists only spec.md under that folder (the code-review record is not in the diff): met.
