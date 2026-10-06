---
name: sortemail-latent-defects-959
description: "#959 research (SortEmail L1-L4): every production caller discards TrySaveAttachmentAsync's bool so 'return false' never surfaces; L2 bound must check isRetry && YesToAll to keep T11; L4 has a THIRD defect (null strOutput) that makes a condition-only fix crash at add-in start; createDirectory tripwire detects unbounded retry without timing"
metadata:
  type: project
---

Issue #959 (post-#956 partials A=SortEmail.AttachmentSaving.cs, T=SortEmail.TrySaveAttachment.cs,
U=SortEmail.UndoAndMoveLog.cs), researched 2026-10-02. Research file:
docs/features/active/2026-10-01-sort-email-latent-logic-defects-959/research/2026-10-02T05-15-*.md

- L2 surface decision: A 221/266/281, EmailFiler 445 and SaveAttachmentsPicturesAsync 279-282 all
  `await` and discard the Task<bool>; EmailFiler.SortAsync 146-149 and EfcDataModel.MoveToFolderAsync
  308 have no catch and no try/finally (Cleanup_Files at 309 is skipped on any exception, pre-existing).
  So only a rethrow surfaces; `false` moves the mail without its attachment.
- L2 bound rule: `isRetryAfterClear && Response == YesToAll` (private core with a flag; keep the
  internal 5-arg signature). `isRetry` alone breaks TST2 T11 (single Yes must re-prompt).
- Deterministic unbounded-loop detection: the `createDirectory` seam runs once per attempt OUTSIDE the
  UAE handler, so a counting fake that throws InvalidOperationException after N calls ends the pre-fix
  loop; make SaveAsFile persistently throw with plain `Setup().Throws()` (SetupSequence exhaustion
  semantics unverified: Moq changelog WebFetch returned 404 twice).
- L4 is three defects: reversed Path.Combine, inverted condition, AND `strOutput` null when passed to
  SanitizeArray (`strOutput![j]`). Fixing only the two named in the issue turns a guaranteed no-op into
  an NRE in AppOlObjects.LoadEmailMoveWriter at add-in start. No fileExists/write seam exists in
  SortEmail; precedent for the delegate seam is EmailDataMiner.Serialization.cs 37-43. Use method
  groups (File.Exists, FileIO2.WriteTextFile) in the excluded wrapper, not lambdas.
- L3 observability: only reader is dialog-bound SaveCaseAsync; reflection on the private static field
  (precedent IdleAsyncQueue_Tests.cs 52-54) is order-independent because the only concurrent writer
  (Cleanup_Files_DoesNotThrow) writes the expected value Empty.
- L1: enum has no [Flags]; `case (NoToAll | No)` is constant 10, never matches; SaveCase only reachable
  from `Sort` (no compiled caller).
- ToDoModel/Email Utilities/SortItemsToExistingFolder.cs 285-315 is a private duplicate of L4.

**Why:** these are judgment calls the planner/executor would otherwise re-derive or get wrong (the
false-return trap and the L4 null array are the two most likely mistakes).
**How to apply:** re-verify line numbers against the branch before citing; #966 owns F1-F3.
