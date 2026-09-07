# P9-T1 — Final CSharpier format pass

Timestamp: 2026-09-07T15-04
Task: [P9-T1]
Issue: #796
Channel used: A

## Branch taken

The verdict line quoted verbatim from evidence/baseline/p0-t7-csharpier-check-baseline.md:

```
CSHARPIER-BASELINE: CLEAN
```

Branch taken: the REPO-WIDE branch. A repo-wide pass on a clean baseline cannot reformat
a file outside this plan's write set, so the scoped QuickFiler and QuickFiler.Test form
was not used and no pre-existing drift is recorded out of scope.

Command:

```
pwsh -NoProfile -Command 'dotnet tool run csharpier format .; "EXIT_CODE=$LASTEXITCODE"'
```

EXIT_CODE: 0

Formatter stdout, verbatim:

```
Formatted 1608 files in 7156ms.
```

The exit code of a write-mode invocation is identical on a clean run and on a repairing
one, so it is not the acceptance evidence. The before-and-after tree observation below is.

## `git status --porcelain --untracked-files=all` immediately BEFORE the format

```
 M QuickFiler.Test/Controllers/QfcItemController.SearchDismissalTests.cs
 M docs/features/active/2026-09-06-quickfiler-folder-dropdown-closes-on-open-and-click-does-not-select-796/issue.md
 M docs/features/active/2026-09-06-quickfiler-folder-dropdown-closes-on-open-and-click-does-not-select-796/plan.2026-09-06T21-59.md
 M docs/features/active/2026-09-06-quickfiler-folder-dropdown-closes-on-open-and-click-does-not-select-796/spec.md
?? docs/features/active/2026-09-06-quickfiler-folder-dropdown-closes-on-open-and-click-does-not-select-796/evidence/issue-updates/issue-796.2026-09-07T15-03.md
?? docs/features/active/2026-09-06-quickfiler-folder-dropdown-closes-on-open-and-click-does-not-select-796/evidence/regression-testing/p8-t1-search-dismissal-repin.md
?? docs/features/active/2026-09-06-quickfiler-folder-dropdown-closes-on-open-and-click-does-not-select-796/evidence/regression-testing/p8-t2-search-dismissal-verification.md
```

## `git status --porcelain --untracked-files=all` immediately AFTER the format

```
 M QuickFiler.Test/Controllers/QfcItemController.SearchDismissalTests.cs
 M docs/features/active/2026-09-06-quickfiler-folder-dropdown-closes-on-open-and-click-does-not-select-796/issue.md
 M docs/features/active/2026-09-06-quickfiler-folder-dropdown-closes-on-open-and-click-does-not-select-796/plan.2026-09-06T21-59.md
 M docs/features/active/2026-09-06-quickfiler-folder-dropdown-closes-on-open-and-click-does-not-select-796/spec.md
?? docs/features/active/2026-09-06-quickfiler-folder-dropdown-closes-on-open-and-click-does-not-select-796/evidence/issue-updates/issue-796.2026-09-07T15-03.md
?? docs/features/active/2026-09-06-quickfiler-folder-dropdown-closes-on-open-and-click-does-not-select-796/evidence/regression-testing/p8-t1-search-dismissal-repin.md
?? docs/features/active/2026-09-06-quickfiler-folder-dropdown-closes-on-open-and-click-does-not-select-796/evidence/regression-testing/p8-t2-search-dismissal-verification.md
```

## Paths appearing only in the AFTER capture

```
NONE
```

The two captures are identical, so the repo-wide pass rewrote no file that was clean
before it. The Phase 4 through Phase 7 sources were already formatted by their own
in-phase CSharpier tasks and this pass left them alone.

## The one already-modified file, checked separately

Porcelain cannot distinguish "unchanged" from "rewritten" for a file that was already
`M` before the pass, because its status code is `M` in both captures. The one such .cs
file is the P8-T1 edit, so it was checked by diff shape rather than by status code:

```
git diff --numstat -- QuickFiler.Test/Controllers/QfcItemController.SearchDismissalTests.cs
7	0	QuickFiler.Test/Controllers/QfcItemController.SearchDismissalTests.cs
```

Seven added and none removed, identical to the reading taken at P8-T1 before this pass,
so the formatter did not rewrite that file either. The single added Arrange statement is
still one physical line under the 100-column default print width.

Every path present after the pass is inside the feature folder or is the write-set path
`QuickFiler.Test/Controllers/QfcItemController.SearchDismissalTests.cs`.

Output Summary: baseline verdict CLEAN, so the repo-wide branch was taken; EXIT_CODE 0;
1608 files processed; the before and after porcelain captures are identical with no path
appearing only in the after capture; and the one already-modified .cs file is provably
unrewritten by its unchanged diff shape.
