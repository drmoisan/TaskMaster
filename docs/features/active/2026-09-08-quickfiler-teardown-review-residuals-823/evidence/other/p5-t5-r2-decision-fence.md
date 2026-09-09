# Phase 5 — R2 decision fence

Timestamp: 2026-09-09T14-34

Task: [P5-T5]

R2 produces no code change. The decision is recorded here and the fence proves the owning file was
not touched.

## The four decision points

Each is transcribed from the R2 section of
`docs/features/active/2026-09-08-quickfiler-teardown-review-residuals-823/spec.md`, which occupies
lines 252 through 281 under the heading `## R2 — Decision only, no code change` at line 252.

1. **The issue-818 relocation is an intermediate state.** The change relocated the throw from the
   `FolderArray` consumption to the direct archive-root read, within the same unguarded method
   `AssignFolderComboBox`, and the throw still reaches a UI-dispatcher boundary unhandled because
   that method contains no `try` and none of its callers wraps the call.
   Read from `spec.md:254-262`.

2. **The exception type is unchanged on both sides of the relocation.** It is
   `InvalidOperationException`, raised by
   `TaskMaster/AppGlobals/ArchiveRootPathGuard.RequireResolvedArchiveRoot`.
   Read from `spec.md:256-261`.

3. **The remaining work is owned by sibling issue #813, which owns the file.** #813 is severity
   Medium, promoted at
   `docs/features/potential/promoted/2026-09-08-assignfoldercombobox-unguarded-archiverootpath-read.md`,
   and is a wave-0 sibling of this same epic. `QuickFiler/Controllers/QfcItemController.FolderHandling.cs`
   is therefore off limits to this feature, no code change is made under #823, and the scope is not
   widened. Read from `spec.md:270-275`.

4. **The first read of `FolderArray` in `AssignFolderComboBox` occurs at `:200`, not at `:212`,** so
   `:212` is the second read rather than the unique pre-change throw site. The conclusion is
   unaffected because both reads are in the same method and the same call frame. The correction is
   recorded so a later reader does not quote `:212` as the unique site.
   Read from `spec.md:277-280`.

## The fence

Command: `git diff --name-only d636b0f28f548181685260d929de6d7d2940d1da...HEAD -- QuickFiler/Controllers/QfcItemController.FolderHandling.cs`
Command: `git status --porcelain --untracked-files=all -- QuickFiler/Controllers/QfcItemController.FolderHandling.cs`

The 40-character SHA is transcribed from the `BASE-SHA:` field of
`evidence/baseline/p0-t2-branch-and-base.md` per D2. The porcelain span is present because a
name-listing diff cannot report an untracked path.

EXIT_CODE: 0

R2-DIFF-LINES: 0
R2-PORCELAIN-LINES: 0

Both commands printed nothing and exited 0.

Output Summary: The R2 decision is recorded as four points with their `spec.md` line ranges, and
both fence commands report zero lines for
`QuickFiler/Controllers/QfcItemController.FolderHandling.cs`. No code change was made for R2.
