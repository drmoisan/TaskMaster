---
name: efcdatamodel-coverage-436
description: Issue #436 (epic #136 F5) EfcDataModel.cs research findings — non-obvious seam blockers and dead code that reading the file alone does not reveal
metadata:
  type: project
---

Findings from the 2026-08-08 per-file research on `QuickFiler/Controllers/EfcDataModel.cs` that are not
apparent from reading that file:

- **`EmailFiler.SortAsync(IList<MailItemHelper>)`, `OpenOlFolderAsync()` and `OpenFileSystemFolderAsync()` are
  non-virtual.** Copying `QfcItemController`'s `Func<EmailFilerConfig, EmailFiler> _emailFilerFactory` seam is
  therefore NOT sufficient for `EfcDataModel` — Moq cannot intercept the invocation. The seam must carry the
  whole construct-and-invoke step (`Func<EmailFilerConfig, IList<MailItemHelper>, Task<bool>>`).
- **`EfcDataModel.PackageItems(bool)` has no caller anywhere in the repo** (the similarly named
  `QfcItemController.PackageItems()` is a different signature). It is dead code sitting in the denominator.
- **`EfcDataModel` does not implement `IQfcDatamodel`** and shares no abstraction with `QfcDatamodel` — only
  fake/mock support can be shared between their test suites, never a fixture or base class.
- **The STA last-resort clause does not apply to this file.** Its only UI touch is one `MessageBox.Show`,
  removable with the `EfcHomeController.MoveFailureMessageAction` delegate pattern.
- `UtilitiesCS.IFolderSearchHandler` already declares `FindFolder` with the exact signature `FindMatches` calls,
  so the folder-search seam needs no new interface and no `UtilitiesCS` edit. It does NOT declare
  `RefreshSuggestions`, which is why that one call needs a delegate instead.

**Why:** epic #136 mandates per-file research; these are the load-bearing facts a planner would otherwise
rediscover by trial-and-error during implementation.

**How to apply:** when planning or executing F5's `EfcDataModel` phase, or any future work seaming
`EmailFiler`/`FolderPredictor` call sites elsewhere in QuickFiler.

Related: [[qfc-datamodel-coverage-436]], [[committed-cobertura-baselines]],
[[feedback-exemption-audit-check-proven-techniques]].
