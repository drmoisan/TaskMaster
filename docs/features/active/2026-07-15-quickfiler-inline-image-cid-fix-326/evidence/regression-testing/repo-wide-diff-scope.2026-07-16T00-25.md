# Repo-Wide Diff Scope — P3-T6

- **Timestamp:** 2026-07-16T00-25

## Literal plan command and why its raw output is not the correct scope signal

- **Command:** `git diff --stat main` (repo root, no path filter)
- **EXIT_CODE:** 0
- **Output Summary:** 108 lines / 107 files changed (4492 insertions(+), 1461075 deletions(-)).

This feature branch (`bug/quickfiler-inline-image-cid-fix-326`) was branched from
`origin/epic/folder-tree-percentage-ui-integration`, which already contains several previously-merged
sibling epic children (e.g. issues #324, #325, #327, #328) not yet present on `main`. A raw
`git diff --stat main` therefore shows the entire epic-integration branch's cumulative diff against
`main` (all sibling children's files, deleted `docs/features/completed/...328...` artifacts, etc.),
not this feature's own file scope. Diffing against literal `main` is not a meaningful isolation signal
for an epic-child branch; the correct comparison base is this branch's actual point of divergence,
`origin/epic/folder-tree-percentage-ui-integration`.

## Corrected comparison against the actual divergence point

- **Command:** `git diff --stat origin/epic/folder-tree-percentage-ui-integration`
- **EXIT_CODE:** 0
- **Output:**

```
 .../Controllers/QfcItemController.ViewerSetup.cs   | 47 ++++++++++++++++++
 .../Attachment/AttachmentSerializableTests.cs      | 36 ++++++++++++++
 .../MailItem/MailItemHelperCoreTests.cs            | 26 ++++++++++
 UtilitiesCS.Test/UtilitiesCS.Test.csproj           |  1 +
 .../Interfaces/IEmailIntelligence/IAttachment.cs   |  1 +
 .../Attachment/AttachmentSerializable.cs           | 21 ++++++++
 .../OutlookObjects/MailItem/MailItemHelper.Html.cs | 10 ++++
 UtilitiesCS/UtilitiesCS.csproj                     |  1 +
 .../plan.2026-07-15T16-53.md                       | 58 +++++++++++-----------
 .../spec.md                                        | 14 +++---
 10 files changed, 179 insertions(+), 36 deletions(-)
```

This matches exactly the 8 tracked production/test/csproj files enumerated by the plan's "Production
files in scope" section (`QfcItemController.ViewerSetup.cs`, `AttachmentSerializableTests.cs`,
`MailItemHelperCoreTests.cs`, `UtilitiesCS.Test.csproj`, `IAttachment.cs`,
`AttachmentSerializable.cs`, `MailItemHelper.Html.cs`, `UtilitiesCS.csproj`) plus the feature-folder
`plan.md` and `spec.md` (the latter checked off progressively in Phase 3/5, as anticipated by this
task's own acceptance text).

## New (untracked) files — confirmed via `git status --porcelain`

`git diff --stat` does not include untracked files. `git status --porcelain` confirms exactly two new
untracked production/test files, matching the plan's remaining two enumerated files:

```
?? UtilitiesCS.Test/OutlookObjects/MailItem/CidImageResolverTests.cs
?? UtilitiesCS/OutlookObjects/MailItem/CidImageResolver.cs
```

plus the new `docs/features/active/2026-07-15-quickfiler-inline-image-cid-fix-326/evidence/` directory
(this feature's own evidence artifacts, expected).

## Conclusion

Combining the corrected tracked-file diff and the untracked-file status output accounts for exactly
the plan's 10 enumerated production/test files plus `spec.md`; no other production or test file was
touched. No sibling epic child's file (from #324/#325/#327/#328) was modified by this feature's own
work — those appeared only in the raw `git diff main` because of the epic-integration branch
structure, not because this feature touched them.
