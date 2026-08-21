# qfc-item-controller-mailactions-error-handling-defects (Issue #483)

- Date captured: 2026-08-07
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/qfc-item-controller-mailactions-error-handling-defects/ (Issue #483)
- Discovered during: preparation research for epic #136 child F10 (issue #453)

- Issue: #483
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/483
- Last Updated: 2026-08-08
## Summary

`MoveMailAsync` swallows every exception behind a broad `catch (System.Exception)`, reports it
through a blocking `MessageBox.Show` from a possibly non-UI thread, and does not rethrow. Sibling
mail actions omit cancellation checks that `MarkItemForDeletionAsync` performs.

## Affected Code

### 1. Broad catch that swallows move failures

`QuickFiler/Controllers/QfcItemController.MailActions.cs:115-122`

```csharp
catch (System.Exception e)
{
    //logger.Debug($"Error moving mail {Subject} from {Sender} on {SentDate}. Skipping");
    logger.Error($"{e}");
    MessageBox.Show(
        $"Error moving mail {ItemHelper.Subject} from {ItemHelper.Sender} on {ItemHelper.SentDate}. Skipping"
    );
}
```

Three problems:

- **Broad catch without propagation.** `.claude/rules/general-code-change.md` requires failing fast
  and explicitly, and prohibits broad catch-all handlers "unless you immediately re-raise or
  propagate with added context." This handler does neither. The caller cannot distinguish a
  successful move from a failed one, so the queue proceeds as though the mail was filed.
- **Modal dialog from a non-UI thread.** `MoveMailAsync` is async and the catch is not marshalled to
  the UI thread. `MessageBox.Show` from a thread-pool thread produces a dialog with no owner, which
  can appear behind the Outlook window and block a background thread indefinitely with no user
  affordance to discover it.
- **Message loses the cause.** The user-facing text reports which mail failed but not why; the
  exception detail goes only to the log.

### 2. Missing cancellation checks

`MarkItemForDeletionAsync` checks the cancellation token, but `MoveMailAsync`, `FlagAsTaskAsync`,
and `EnumerateConversationAsync` do not perform the equivalent check on the same paths. During a
bulk operation that the user cancels, these continue to completion.

## Why This Is a Defect

The swallowed exception is the more serious of the two. A move that throws is reported to the user
as a transient message and then forgotten; the item remains in place while the surrounding flow
treats the operation as complete. There is no retry and no durable record beyond the log line.

## Suspected Fix

Narrow the catch to the exception types actually anticipated on the filer path, add context and
rethrow (or return a failure result the caller can act on), and marshal any user-facing notification
through the existing UI dispatcher seam rather than calling `MessageBox.Show` directly. Add
cancellation checks to the three actions that lack them, matching `MarkItemForDeletionAsync`.

## Severity

Medium-High. Silent failure to file mail, with a user-visible message that does not prevent the
surrounding flow from treating the operation as successful.

## Scope

Out of scope for epic #136 child F10, whose NFR prohibits behavior change to observable QuickFiler
flows. Both changes alter observable error behavior and must be scheduled with their own regression
tests.
