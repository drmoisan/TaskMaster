# QA Gate — No New Production Seam (Issue #656)

Timestamp: 2026-09-01T14-56
Task: [P4-T14]
Satisfies: AC-20

Command (authoritative):
```
git diff --name-only origin/main...HEAD -- QuickFiler/Viewers/IBreadcrumbDropDownHost.cs
git status --porcelain -- QuickFiler/Viewers/IBreadcrumbDropDownHost.cs
@(Select-String -Path QuickFiler\Viewers\BreadcrumbDropDownOpenCoordinator.cs -Pattern '^\s+(internal|public)\s').Count
```

EXIT_CODE: 0

## Results

| Check | Required | Observed | Met |
|---|---|---|---|
| `git diff --name-only origin/main...HEAD -- QuickFiler/Viewers/IBreadcrumbDropDownHost.cs` | empty | empty | yes |
| `git status --porcelain -- QuickFiler/Viewers/IBreadcrumbDropDownHost.cs` | empty | empty | yes |
| Declared `internal`/`public` member count in the coordinator | 12 | **12** | yes |

The declared-member count is unchanged from the P0-T12 baseline of 12, and the same twelve
declarations are present; only their line numbers shifted, by the number of documentation lines the
change inserted above them. The enumeration is recorded under `Declared Member Lines:` in
`evidence/other/lock-discipline.2026-08-31T20-40.md`.

Both halves of AC-20 therefore hold:

- **No new member on `BreadcrumbDropDownOpenCoordinator`.** The count is identical and the member
  set is identical. The change adds one method-local `bool` inside a `private` method; `CloseCore`
  itself remains `private`. The search pattern cannot be inflated by the two `remarks` blocks
  because a `///` line's first non-whitespace character is a forward slash, which
  `^\s+(internal|public)\s` cannot match.
- **No member added to `IBreadcrumbDropDownHost`.** The interface file is absent from the changed
  file list and is clean in the working tree.

No new seam was needed because `[assembly: InternalsVisibleTo("QuickFiler.Test")]` already exists in
`QuickFiler/Properties/AssemblyInfo.cs` and the test host already exposes the required bypass
through `ControlledHost.SetOpen`. The regression test uses only members that existed on the
unmodified tree, which is also why it compiled cleanly in P1-T2 before any production change.

## Base-ref substitution (recorded, not silent)

For this task the two bases agree: the pinned base
`2b85134b42872e405602e6064e02dc9cda6c319b` also returns an empty diff for
`QuickFiler/Viewers/IBreadcrumbDropDownHost.cs`, because that file was not touched by this item nor
by the changes inherited from `main`. The authoritative base remains `origin/main`
(`5670b3cfe6a52e3b890bf80f0cd85a20d4fe4723`) for consistency with P4-T11 through P4-T13; here the
substitution changes nothing.

Output Summary: No new production seam. The interface file is absent from the change set and clean
in the tree, and the coordinator's declared `internal`/`public` member count is unchanged at 12.
AC-20 is satisfied.
