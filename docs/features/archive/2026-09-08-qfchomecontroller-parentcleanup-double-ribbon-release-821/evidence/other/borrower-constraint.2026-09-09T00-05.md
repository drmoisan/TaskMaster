# Phase 3 — Borrower constraint across both progress surfaces (AC12)

Timestamp: 2026-09-09T13-08
Task: [P3-T9]

Both progress surfaces are **borrowers** of the cancellation token source, not owners. The fix may
guard and may swallow the disposed case; it must not dispose the source and must not construct one.
A borrower that disposed a source another holder still uses would convert a benign cancel click into
a repository-wide `ObjectDisposedException` generator.

## Search 1 — construction, over both progress files

Command:

```text
pwsh -NoProfile -Command 'Select-String -SimpleMatch -Path "UtilitiesCS/Threading/ProgressViewer.cs","UtilitiesCS/Threading/ProgressPane.cs" -Pattern "new CancellationTokenSource"'
```

EXIT_CODE: 0
Result: **0 matches** across both files.

Neither surface constructs a cancellation token source. The source is always supplied from outside —
through the `CancelSource` property or `SetCancellationTokenSource` on the viewer, and through
`SetCancellationTokenSource` on the pane.

## Search 2 — disposal of the borrowed field, over both progress files

Command:

```text
pwsh -NoProfile -Command 'Select-String -Path "UtilitiesCS/Threading/ProgressViewer.cs","UtilitiesCS/Threading/ProgressPane.cs" -Pattern "_(cancel|token)Source\??\.Dispose"'
```

EXIT_CODE: 0
Result: **0 matches** across both files.

This is a regular-expression search rather than `-SimpleMatch`, deliberately: the alternation
`(cancel|token)` and the optional-null-conditional `\??` are the pattern's purpose. It covers both
field spellings — `_cancelSource` on the viewer and `_tokenSource` on the pane — and both the plain
`.Dispose` and null-conditional `?.Dispose` dereference forms. Neither surface disposes its borrowed
source, before or after this change.

## Search 3 — the single disposal site survives, unmoved

Command:

```text
pwsh -NoProfile -Command 'Select-String -SimpleMatch -Path "QuickFiler/Controllers/QfcHomeController.cs" -Pattern "_tokenSource?.Dispose();"'
```

EXIT_CODE: 0
Result: **1 match, at line 389.**

The single disposal site for the QuickFiler session source remains
`QuickFiler/Controllers/QfcHomeController.cs` line 389, present exactly once and still at line 389.
It is unmoved because the Site A edit region begins at line 405, below it.

## Rethrow constraint

Neither progress surface rethrows out of its event handler. Verified in `[P3-T4]` and `[P3-T8]`: the
only `throw` statement in either file is the `?? throw new InvalidOperationException(...)` inside
`RequestCancel` — `ProgressViewer.cs` line 86 and `ProgressPane.cs` line 70 — and each
`CancelButton_Click` catches `System.Exception`, logs through log4net, and contains no `throw`. The
handler therefore absorbs every exception type, including the one `RequestCancel` raises.

Output Summary: both searches return **0 matches** across both progress files — neither constructs a
`CancellationTokenSource` and neither disposes its borrowed field — and the
`QuickFiler/Controllers/QfcHomeController.cs` disposal statement is still present exactly once and
still at line 389. Neither surface rethrows out of its handler. AC12 is discharged.
