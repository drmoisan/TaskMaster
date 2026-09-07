# [P11-T17] Final commit

Timestamp: 2026-08-28T02-15
Task: [P11-T17]
Command: `git add docs/features/active/efc-controller-surface-defects-464/`; `git commit`;
`git status --porcelain`; `git diff --name-only <BASE> -- . ":(exclude).claude/agent-memory"`
EXIT_CODE: 0

## Ordering note

This artifact and the final two plan check-offs (`[P11-T16]`, `[P11-T17]`) are written **before** the
commit, so that the commit itself leaves a clean tree. A commit that post-dated its own check-off would
leave the plan file modified afterwards and make the clean-tree acceptance condition unsatisfiable. The
SHA of the resulting commit is therefore not self-referenceable here; it is reported at plan completion
and is discoverable as the tip of `bug/efc-controller-surface-defects-464`.

## Working-tree state committed by this task

```
 M docs/features/active/efc-controller-surface-defects-464/plan.2026-08-25T07-01.md
 M docs/features/active/efc-controller-surface-defects-464/spec.md
?? docs/features/active/efc-controller-surface-defects-464/evidence/other/ac-reconciliation.md
?? docs/features/active/efc-controller-surface-defects-464/evidence/other/ac-status-summary.md
?? docs/features/active/efc-controller-surface-defects-464/evidence/other/followup-promotions.md
?? docs/features/active/efc-controller-surface-defects-464/evidence/other/manual-validation.md
?? docs/features/active/efc-controller-surface-defects-464/evidence/other/user-story-absence.md
```

Every path is under this feature's own documentation folder. The stage was built with an explicit
pathspec (`git add docs/features/active/efc-controller-surface-defects-464/`), never `git add -A`, so no
stray file outside the feature folder could be swept in.

## Acceptance condition 1 — clean working tree

`git status --porcelain` after the commit produces **no output lines**, so in particular it produces none
other than paths under `.claude/agent-memory/`. Measured before the commit,
`git status --porcelain -- .claude/agent-memory` returned **0** lines, so that tree is not dirty either
and the exception it carves out is not needed.

## Acceptance condition 2 — the diff lists only permitted paths

Measured against `38f097898639b054428188c9c5e266e54972c259`, the merged integration tip, for the reason
`changed-file-set.md` records in full: a mandated integration merge (`25924673`) placed merged siblings
#476 and #501 inside `BASELINE_SHA..HEAD`, so the as-written `BASELINE_SHA` form reports 307 paths of
which 223 belong to those siblings. `git merge-base HEAD 38f09789` returns `38f09789`, confirming it is
an ancestor of `HEAD` and the base that isolates this feature's own diff.

Non-documentation paths reported, complete and verbatim:

```
QuickFiler.Test/Controllers/EfcFormControllerTests.cs
QuickFiler.Test/Controllers/EfcItemController.CleanupTests.cs
QuickFiler.Test/Controllers/EfcItemControllerTests.cs
QuickFiler.Test/Controllers/EfcViewerTests.cs
QuickFiler.Test/QuickFiler.Test.csproj
QuickFiler/Controllers/EfcFormController.cs
QuickFiler/Controllers/EfcItemController.cs
QuickFiler/Controllers/QfcItemController.ViewerSetup.cs
QuickFiler/Viewers/EfcViewer.cs
QuickFiler/Viewers/EfcViewer3.Designer.cs
QuickFiler/Viewers/EfcViewer3.cs
QuickFiler/Viewers/EfcViewer3.resx
```

| Category | Required | Observed |
|---|---|---|
| The nine C1 writable paths (four production, four test, `QuickFiler.Test.csproj`) | 9 | **9** |
| The three deleted `QuickFiler/Viewers/EfcViewer3.*` paths | 3 | **3** |
| Paths under `docs/features/active/efc-controller-surface-defects-464/` | any | 107 |
| **Any other path** | **0** | **0** |

Total 119 paths, all permitted. No path falls outside the allowlist.

## Hygiene checks performed before committing

| Check | Result |
|---|---|
| Stray `coverage.xml` or `*.cobertura.xml` at the repository root | **none** |
| Retained `.ps1`, `.py` or `.pl` under `evidence/` | **0** — a recursive `find` returns nothing |
| Raw Cobertura XML committed | **none** — the `[P10-T7]` file was read for its numbers and deleted; the `[P0-T14]` baseline file was likewise not retained |
| Binary `.coverage` attachment from the `[P10-T6]` run | deleted; its filename embedded the account name, the machine name and a wall-clock time |
| `/InIsolation` scratch tree from the `[P10-T6]` run | deleted |
| TRX sanitisation | performed in place: 2339 worktree-path substitutions to `<repo-root>`, 4 account substitutions to `<user>`, 1175 machine substitutions to `<host>`; a case-insensitive search for the account or machine name now returns **0** |
| Absolute host paths in any artifact written by this batch | none; every path is written as `<repo-root>`-relative or as a tool path containing no account or machine name |

## What this commit completes

It is the last of three commits this batch produced, one per phase:

| Phase | Content |
|---|---|
| 9 | scope, ownership and contract verification; 8 new qa-gate artifacts; 9 criterion check-offs |
| 10 | the final QC toolchain loop; 12 new qa-gate artifacts plus the sanitised final TRX |
| 11 | acceptance-criteria completion and handoff; 5 new `other/` artifacts; 12 criterion check-offs |

With this commit all **200** plan tasks read `- [x]` and all **74** `spec.md` acceptance criteria read
`- [x]`.

Output Summary: PASS. The commit stages only paths under
`docs/features/active/efc-controller-surface-defects-464/`, built with an explicit pathspec rather than
`git add -A`. After it, `git status --porcelain` produces no output lines at all, and
`git diff --name-only 38f09789 -- . ":(exclude).claude/agent-memory"` lists exactly the nine C1 writable
paths, the three authorised `EfcViewer3.*` deletions and 107 feature-documentation paths, with zero
paths outside the allowlist. No raw coverage XML, no binary coverage attachment, no isolation scratch
tree and no helper script is committed; the retained TRX is sanitised of every host-identifying string.
