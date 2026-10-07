---
name: sortemail-966-consolidation-959
description: SortEmail #959/#966 consolidation facts that are not derivable quickly — live path builds AttachmentHelper with the mail FOLDER NAME, so A 235 re-roots only FilePathSave; ToDoModel SortItemsToExistingFolder.cs is not compiled; session Response cannot be set in tests
metadata:
  type: project
---

Three facts that cost a long trace during the #959 supplementary research (2026-10-02):

- `SaveAttachmentAsync(helper, destinationPath)` (SortEmail.AttachmentSaving.cs) sets only `FolderPathSave`; `FilePathSaveAlt` is a separate `FilePathHelper`. The live path (`MailItemHelper.Properties.cs` ~255, `MailItemHelper.cs` ~159) builds helpers with `FolderName` (the Outlook folder name), so the alternate-name save path stays relative until re-rooted. Fix pattern: set `FilePathHelperSaveAlt.FolderPath` too (the accessor is `internal`).
- `ToDoModel/Email Utilities/SortItemsToExistingFolder.cs` has no `<Compile Include>` in ToDoModel.csproj (only `CaptureEmailAddressesModule.cs` under that folder). Defects in it are unreachable; delete rather than fix.
- A production `YesNoToAllPromptSession` cannot be driven in a test: `Response` has a private setter and `Ask` on the production instance shows the real dialog. Reset coverage must be structural (enumerate the static session fields by reflection and compare with the collection `Cleanup_Files` iterates) or via a seam in the caller (`EfcDataModel` virtual).

**Why:** each of these looked like a one-grep question and was not; recording them avoids re-tracing.
**How to apply:** when a SortEmail, EfcDataModel or ToDoModel email-utilities item comes up, check these first.
