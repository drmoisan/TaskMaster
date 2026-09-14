# Format scope check — issue #877

Timestamp: 2026-09-13T10-55
Command: `git -C <repo-root> status --porcelain --untracked-files=all -- ':!docs' ':!.claude'`
EXIT_CODE: 0
Output Summary: The span printed ZERO lines. The mandatory repo-wide `csharpier format .` at [P2-T2] rewrote no file anywhere in the tree outside `docs` and `.claude`. No path required classification, no path required restoring with `git checkout --`, and the loop was not restarted.

## Path list and classification

Path list returned by the span: empty.

- Class (a), one of the five write-set paths: none present in the list. All five were committed at [P1-T13] and the format pass left them byte-identical, so they do not appear as working-tree modifications.
- Class (b), a path the [P0-T10] baseline artifact already enumerated as unformatted: none, because that baseline recorded a clean check.

## Class (b) pre-existing format drift repaired by this run

Class (b) list: EMPTY.

The `evidence/baseline/csharpier-check-before.2026-09-13T10-44.md` artifact from [P0-T10] recorded exit code 0 with zero lines containing `Was not formatted`, so there was no pre-existing CSharpier drift for the repo-wide `format .` to repair incidentally. Consequently:

- [P2-T14] has no class-(b) path under `SVGControl` or `SVGControl.Test` to consider, so its STOP AND REPORT branch is not reached.
- [P2-T15] permits only the five write-set paths, with no class-(b) additions.
- [P2-T16] excludes nothing from its token search.
- [P2-T28] makes no separate class-(b) commit.
