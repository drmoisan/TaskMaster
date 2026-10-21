# Phase 5 — Source commit ([P5-T1])

Timestamp: 2026-09-01T23-24

Every gate in Phase 5 runs against committed state, because a three-dot diff compares the merge base to
the `HEAD` commit and cannot see uncommitted work. This task commits first.

## Staging

Command:

```
git add QuickFiler/Controllers/QfcFormKeyHandler.cs QuickFiler/Viewers/QfcFormViewer.cs QuickFiler.Test/Controllers/QfcFormKeyHandlerTests.cs
```

The three paths were staged explicitly. No all-paths stage was used: a blanket stage would sweep an
unrelated queued promotion file from `docs/features/potential/` onto this branch.

## Commit

Command: `git commit -F -` with a conventional-commit message naming issue #663.

EXIT_CODE: 0

Output, verbatim:

```
[bug/qfc-twin-processcmdkey-alt-chord-over-claim-663 ae2885e7] fix(quickfiler): narrow ProcessCmdKey Alt-chord claim to bare Alt (#663)
 3 files changed, 164 insertions(+), 4 deletions(-)
```

Commit subject line: `fix(quickfiler): narrow ProcessCmdKey Alt-chord claim to bare Alt (#663)`.
Commit SHA: `ae2885e7`.

## Acceptance reading 1 — porcelain span

Command: `git status --porcelain -- '*.cs'`

Output: **nothing**. No `.cs` path is modified, staged or untracked after the commit.

## Acceptance reading 2 — anchored name-listing diff

Command: `git diff --name-only origin/main...HEAD -- '*.cs'`

Output, verbatim — exactly three lines:

```
QuickFiler.Test/Controllers/QfcFormKeyHandlerTests.cs
QuickFiler/Controllers/QfcFormKeyHandler.cs
QuickFiler/Viewers/QfcFormViewer.cs
```

The two spans are complementary and each alone is wrong in one state: the anchored diff is blind to an
untracked file, while porcelain status goes empty once the change is committed. Together they establish
that the C# change set is exactly the three authorised paths and that nothing further is pending.

`origin/main` is at `9ca9e99a86428717891a4b54fed70f573a0a2d65` and was re-confirmed at the Phase 5
boundary, so the three-dot anchor is correct.

Output Summary: The three changed source files were staged explicitly and committed as `ae2885e7` with
164 insertions and 4 deletions across 3 files. `git status --porcelain -- '*.cs'` prints nothing and
`git diff --name-only origin/main...HEAD -- '*.cs'` prints exactly the three authorised paths.
