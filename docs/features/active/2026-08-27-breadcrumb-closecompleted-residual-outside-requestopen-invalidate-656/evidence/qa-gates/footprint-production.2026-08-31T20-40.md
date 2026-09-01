# QA Gate — Production Footprint (Issue #656)

Timestamp: 2026-09-01T14-55
Task: [P4-T11]
Satisfies: AC-10

Command (authoritative):
```
git diff --name-only origin/main...HEAD -- QuickFiler
git status --porcelain -- QuickFiler
```

EXIT_CODE: 0

## Authoritative diff output (base `origin/main`, verbatim)

```
QuickFiler/Viewers/BreadcrumbDropDownOpenCoordinator.cs
```

## Porcelain output (verbatim)

```
```

(empty)

The diff output is exactly the single line `QuickFiler/Viewers/BreadcrumbDropDownOpenCoordinator.cs`
and the porcelain output is empty. AC-10 is satisfied: no file under `QuickFiler/` other than the
sole authorized production file appears in the change set.

The porcelain span is the required companion because a name-listing diff enumerates tracked changes
only and can never report a newly created untracked file. Its emptiness confirms there is no
untracked or uncommitted file under `QuickFiler/` that the diff would have missed.

## Base-ref substitution (recorded, not silent)

The plan anchors this assertion to the pinned base `2b85134b42872e405602e6064e02dc9cda6c319b`. That
base is stale: it predates the reconciliation of this branch against `origin/main`, and because it
is an ancestor of HEAD the three-dot form degenerates to a plain two-dot diff against it, which
conflates every change `main` gained in the interval with this item's change set. Measured here:

```
git diff --name-only 2b85134b42872e405602e6064e02dc9cda6c319b...HEAD -- QuickFiler
QuickFiler/Controllers/FilerQueue.cs
QuickFiler/Controllers/QfcFormController.EventHandlers.cs
QuickFiler/Controllers/QfcHomeController.Metrics.cs
QuickFiler/Viewers/BreadcrumbDropDownOpenCoordinator.cs
```

Three of those four paths are pre-existing changes that arrived on `main` and were merged into this
branch before execution began. None was touched by this item; the working tree confirms it, since
the scoped porcelain output is empty and the staged set recorded in
`evidence/other/commit.2026-08-31T20-40.md` names only
`QuickFiler/Viewers/BreadcrumbDropDownOpenCoordinator.cs` under `QuickFiler/`.

`origin/main` resolves to `5670b3cfe6a52e3b890bf80f0cd85a20d4fe4723` and is an ancestor of HEAD, so
`origin/main...HEAD` isolates this branch's own contribution. It is therefore the correct footprint
base and is used as authoritative here. Both measurements are recorded above so the substitution is
auditable. The same substitution applies to P4-T12, P4-T13 and P4-T14.

Output Summary: Production footprint verified. Against `origin/main`, exactly one file under
`QuickFiler/` changed: `QuickFiler/Viewers/BreadcrumbDropDownOpenCoordinator.cs`. The scoped
porcelain output is empty. AC-10 is satisfied. The plan's pinned base is stale and its output is
recorded alongside for audit.
