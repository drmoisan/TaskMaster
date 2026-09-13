# Gate: SVGControl is unmodified — issue #877

Timestamp: 2026-09-13T11-01
Command: `git -C <repo-root> diff --name-only main...HEAD -- SVGControl SVGControl.Test` and `git -C <repo-root> status --porcelain --untracked-files=all -- SVGControl SVGControl.Test`
EXIT_CODE: 0
Output Summary: Both spans printed ZERO lines. Neither `SVGControl` nor `SVGControl.Test` was modified, either as a committed change relative to the merge base of `main` and `HEAD`, or as an uncommitted working-tree change.

## Span 1 — anchored diff

Command: `git -C <repo-root> diff --name-only main...HEAD -- SVGControl SVGControl.Test`

Output: zero lines.

## Span 2 — porcelain companion

Command: `git -C <repo-root> status --porcelain --untracked-files=all -- SVGControl SVGControl.Test`

Output: zero lines.

## Class (b) conflict branch not reached

The [P0-T10] baseline recorded a clean CSharpier check with `Class (b) list: EMPTY`, and [P2-T4] confirmed the mandatory repo-wide `csharpier format .` rewrote nothing outside `docs` and `.claude`. There is therefore no class-(b) path anywhere, and in particular none under `SVGControl` or `SVGControl.Test`. The STOP AND REPORT branch described for this task, which would trigger if the mandatory format command had repaired a pre-existing unformatted file inside the prohibited directory, was not reached.

## Scope statement

`issue.md` `## Out of Scope (non-negotiable)` forbids modifying `SVGControl`. Three near-identical `AssemblyResolve` handlers exist after this change: the one in `SVGControl/SvgAssemblyResolver.cs`, and the two compiled copies of the shared `TestSupport/TestAssemblyResolver.cs` linked into `QuickFiler.Test` and `UtilitiesCS.Test`. That duplication is accepted for now. Consolidating them into production code has a different blast radius and is tracked separately as issue #879.

## Base ref

`main` resolves as a local ref at `e4349a62c0fe6a5daece0b0554a0da5f508129d6`, so no `origin/main...HEAD` substitution was made.
