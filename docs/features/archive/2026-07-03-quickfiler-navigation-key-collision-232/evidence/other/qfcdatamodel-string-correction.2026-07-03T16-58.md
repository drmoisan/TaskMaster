# QfcDatamodel.cs Caller-Context String Correction — Remediation Cycle 1 (Issue #232)

Timestamp: 2026-07-03T16-58

File: `QuickFiler/Controllers/QfcDatamodel.cs`
Location: `logger.Debug(...)` call at line 326, inside method `ScoreRemainingQueueMailItemAsync`
(method declared at line 316: `private async Task<long> ScoreRemainingQueueMailItemAsync(MailItem mailItem, CancellationToken cancel)`).

## Prior string (before correction)

```
$"Probability debug [QfcDatamodel.LoadRemainingEmailsToQueueAsync (master-queue admission)] "
```

## Corrected string (after correction)

```
$"Probability debug [QfcDatamodel.ScoreRemainingQueueMailItemAsync (master-queue admission)] "
```

## Confirmation

- The `logger.Debug(...)` call is physically located inside `ScoreRemainingQueueMailItemAsync`. The
  emitting method is therefore `ScoreRemainingQueueMailItemAsync`, not `LoadRemainingEmailsToQueueAsync`.
- The `(master-queue admission)` descriptor is retained.
- Only the method-name token inside the caller-context bracket changed. No control flow, no other tokens,
  and no interpolated arguments were modified.
- Post-edit grep confirms the file now contains
  `[QfcDatamodel.ScoreRemainingQueueMailItemAsync (master-queue admission)]` and no longer contains
  `[QfcDatamodel.LoadRemainingEmailsToQueueAsync (master-queue admission)]`.
