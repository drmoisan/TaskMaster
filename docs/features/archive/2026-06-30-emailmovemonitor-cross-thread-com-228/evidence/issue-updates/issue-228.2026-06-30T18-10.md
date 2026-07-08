# Issue #228 Update Mirror

Timestamp: 2026-06-30T22-56
PostedAs: unknown

POSTING STATUS: Not yet posted to GitHub. This mirror records the intended update text for issue #228; posting to the GitHub issue is a downstream step performed by the orchestrator/PR-author flow. No `gh` issue edit/comment was executed by the executor.

## Intended Update Text

Issue #228 — EmailMoveMonitor cross-thread COM access — implementation complete (pending review/merge).

### What changed
- All Outlook COM access in `EmailMoveMonitor` (`HookItem`, `UnhookItem`, `UnhookAll`) is now marshaled to the captured Outlook STA thread via an injectable `Action<System.Action> _marshalToSta` delegate that defaults to `UiThread.Dispatcher.Invoke`. This fixes the cross-thread `COMException` raised when the unhook path ran on a ThreadPool thread.
- New narrow interface `IEmailMoveMonitor` (`QuickFiler\Interfaces\IEmailMoveMonitor.cs`); the three production consumers (`QfcDatamodel`, `QfcQueue`, `QfcCollectionController`) now hold the field as `IEmailMoveMonitor` (construction unchanged: `new EmailMoveMonitor()`).
- The redundant `await Task.Run(...)` wrapper around the unhook loop in `QfcDatamodel.DequeueNextItemGroupAsync` is removed; the loop now runs directly since the unhook path self-marshals. Returned-node behavior and the surrounding log4net try/catch are unchanged.
- `EmailMoveAction` now caches stable `MailEntryId`/`FolderEntryId` strings captured on the STA thread at hook time; unhook comparisons use the cached IDs.
- The dormant `UnhookItemAsync`/`GetParentFolderAsync` had the same marshal seam applied to their retained COM access (replacing the prior `Task.Run` hop in `GetParentFolderAsync`) but were NOT re-wired into the active call path; the commented-out `UnhookItemAsync` call site stays commented out.

### Tests
- `QuickFiler.Test\Helper Classes\EmailMoveMonitorTests.cs` — 8 MSTest + Moq + FluentAssertions tests covering: first-item-per-folder subscribe (shared folder does not resubscribe), last-item-per-folder unsubscribe, `UnhookItem(null)` no-op, cached-EntryID match/remove, all-COM-via-delegate, `UnhookAll` clears state, duplicate-hook / unhook-never-hooked edge cases, and a ThreadPool-thread regression proving COM access runs on the marshal-target thread.

### Verification
- Full toolchain clean final pass: csharpier check (EXIT 0), analyzer msbuild (EXIT 0), nullable msbuild with TreatWarningsAsErrors (EXIT 0, no QuickFiler-own nullable errors), vstest with coverage (EXIT 0, 209/209 passed).
- Changed/new `EmailMoveMonitor` bookkeeping line coverage: 96.92% (>= 90% floor). QuickFiler first-party coverage 32.94% -> 33.74% (no changed-line regression).
- No banned-API regressions; `TimeProvider.Delay` preserved.

### Acceptance Criteria
AC1–AC9 all met; see `spec.md` and `evidence/qa-gates/` for per-criterion evidence references.

## P5-T3 Decision (recorded per plan)
The dormant `UnhookItemAsync`/`GetParentFolderAsync` retained Outlook COM access. The marshal seam was applied to that access (no member left with un-marshaled live COM reads), and no new active caller was introduced. The members remain dormant.
