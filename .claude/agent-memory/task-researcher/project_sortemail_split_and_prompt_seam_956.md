---
name: sortemail-split-and-prompt-seam-956
description: "#956 research: SortEmail static-prompt state can't be tested in parallel; per-call session object is the seam; AsyncLocal/ref rejected; most SortEmail orchestration has no compiled caller"
metadata:
  type: project
---

Issue #956 (SortEmail.cs 1,454 lines, 28 ExcludeFromCodeCoverage), researched 2026-10-01.

- Live production paths into SortEmail are only four: Cleanup_Files (EfcDataModel 309), the
  SaveAttachmentAsync(AttachmentHelper,string) extension (EmailFiler 445), UndoAsync (RibbonController 230),
  WriteCSV_StartNewFileIfDoesNotExist (AppOlObjects 301). Every SortAsync/Sort overload, SaveAttachmentsOld
  and IsPicture have no compiled caller (QuickFiler/Legacy/QfcController.cs is not in QuickFiler.csproj).
- Static sticky YesNoToAll fields cannot be tested under ClassLevel parallelism (Cleanup_Files_DoesNotThrow
  resets them concurrently). Recommended seam: an internal per-call `YesNoToAllPromptSession` (prompt Func +
  sticky state) with a static readonly production instance. Rejected: `ref` param (CS1988 in async),
  AsyncLocal (a value set in an awaited callee doesn't flow back, breaking YesToAll stickiness).
- Latent defects found: SaveCase uses `case (A | B)` bitwise constants that never match; sticky YesToAll
  + persistent UnauthorizedAccess recurses forever; Cleanup_Files omits _attachmentsAltName.
- The #945 plan's claim that Invoke-MSTestWithCoverage.ps1 discovers no assemblies in a worktree looks wrong
  on reading (exclusion is relative to the search root); unverified at runtime.

**Why:** the next SortEmail item (follow-up F1: overwrite/alt-name prompts) can reuse the session type.
**How to apply:** the split landed 2026-10-01 (files are SortEmail.*.cs); post-split line numbers and the
L1-L4 fix designs are in [[sortemail-latent-defects-959]]. Follow-ups F1-F3 are tracked under #966.
