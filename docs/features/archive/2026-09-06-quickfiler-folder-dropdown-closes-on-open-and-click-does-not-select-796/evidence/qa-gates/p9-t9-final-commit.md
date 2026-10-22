# P9-T9 — Final commit

Timestamp: 2026-09-07T16-09
Task: [P9-T9]
Issue: #796
Channel used: A

## Staging

Explicit pathspecs were staged rather than everything, because a repository-wide stage
can sweep an unrelated queued promotion file into this item's branch:

```
pwsh -NoProfile -Command 'git add QuickFiler QuickFiler.Test docs/features/active/2026-09-06-quickfiler-folder-dropdown-closes-on-open-and-click-does-not-select-796'
```

EXIT_CODE: 0

The command emitted 13 `LF will be replaced by CRLF` advisory warnings, one per Markdown
file staged. These are line-ending normalisation notices from the repository's
`core.autocrlf` configuration, not errors, and they change no content.

## Commit

EXIT_CODE: 0

SHA: 676966ef2d36e20f231cfb3949591d44a9adb92d

Short SHA: 676966ef
Branch: bug/quickfiler-folder-dropdown-closes-on-open-796
Parent: 5b8e0bf58417dcaf25e69aa05e7c6e5962b3e1aa

Statistics: 15 files changed, 1377 insertions(+), 40 deletions(-).

Subject line:

```
test(796): re-pin the search-dismissal suite to AC4 and record the Phase 9 QA gates
```

## `--name-status` listing

```
M	QuickFiler.Test/Controllers/QfcItemController.SearchDismissalTests.cs
A	docs/features/active/2026-09-06-quickfiler-folder-dropdown-closes-on-open-and-click-does-not-select-796/evidence/issue-updates/issue-796.2026-09-07T15-03.md
A	docs/features/active/2026-09-06-quickfiler-folder-dropdown-closes-on-open-and-click-does-not-select-796/evidence/qa-gates/p9-t1-csharpier-format.md
A	docs/features/active/2026-09-06-quickfiler-folder-dropdown-closes-on-open-and-click-does-not-select-796/evidence/qa-gates/p9-t2-csharpier-check.md
A	docs/features/active/2026-09-06-quickfiler-folder-dropdown-closes-on-open-and-click-does-not-select-796/evidence/qa-gates/p9-t3-analyzer-rebuild.md
A	docs/features/active/2026-09-06-quickfiler-folder-dropdown-closes-on-open-and-click-does-not-select-796/evidence/qa-gates/p9-t4-nullable-rebuild.md
A	docs/features/active/2026-09-06-quickfiler-folder-dropdown-closes-on-open-and-click-does-not-select-796/evidence/qa-gates/p9-t5-full-assembly-tests.md
A	docs/features/active/2026-09-06-quickfiler-folder-dropdown-closes-on-open-and-click-does-not-select-796/evidence/qa-gates/p9-t6-coverage-final.md
A	docs/features/active/2026-09-06-quickfiler-folder-dropdown-closes-on-open-and-click-does-not-select-796/evidence/qa-gates/p9-t7-coverage-delta.md
A	docs/features/active/2026-09-06-quickfiler-folder-dropdown-closes-on-open-and-click-does-not-select-796/evidence/qa-gates/p9-t8-file-size-audit.md
A	docs/features/active/2026-09-06-quickfiler-folder-dropdown-closes-on-open-and-click-does-not-select-796/evidence/regression-testing/p8-t1-search-dismissal-repin.md
A	docs/features/active/2026-09-06-quickfiler-folder-dropdown-closes-on-open-and-click-does-not-select-796/evidence/regression-testing/p8-t2-search-dismissal-verification.md
M	docs/features/active/2026-09-06-quickfiler-folder-dropdown-closes-on-open-and-click-does-not-select-796/issue.md
M	docs/features/active/2026-09-06-quickfiler-folder-dropdown-closes-on-open-and-click-does-not-select-796/plan.2026-09-06T21-59.md
M	docs/features/active/2026-09-06-quickfiler-folder-dropdown-closes-on-open-and-click-does-not-select-796/spec.md
```

Fifteen paths: one write-set test file and fourteen feature-folder paths. No path lies
outside the write set or the feature folder, and no path under UtilitiesCS/,
UtilitiesCS.Test/, .claude/, .codex/, .agents/, config/, .github/, or naming
TaskMaster.sln or a repository-root build property file appears. The scope-boundary gate
that measures this formally is P9-T10.

Four evidence artifacts written after this commit are necessarily absent from the listing
above and are folded in by the P9-T11 amend: this artifact itself, the P9-T10
scope-boundary artifact, the P9-T11 loop artifact, and the P9-T10 and P9-T11 check-offs
in the plan file.

Output Summary: staged with the explicit three-pathspec form and committed as
676966ef2d36e20f231cfb3949591d44a9adb92d, 15 files changed with 1377 insertions and 40
deletions. Both acceptance clauses are met: the SHA is recorded and the commit's
`--name-status` listing is reproduced in full.
