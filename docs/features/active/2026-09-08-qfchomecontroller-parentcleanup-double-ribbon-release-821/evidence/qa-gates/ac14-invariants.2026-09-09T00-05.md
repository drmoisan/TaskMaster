# Phase 5 — AC14 invariants hold as written

Timestamp: 2026-09-09T13-35
Task: [P5-T9]

## Invariant 1 — Sites A and A'

> A home controller's parent-cleanup callback is invoked **at most once per controller instance**,
> unconditionally with respect to whether an earlier teardown stage threw, because the delegate is
> read into a local and the stored reference is cleared **before** the local is invoked.

### Site A — `QuickFiler/Controllers/QfcHomeController.cs`, delivered lines 403-409

```csharp
            finally
            {
                System.Action parentCleanup = ParentCleanup; // #810 idiom carried up by #821.
                ParentCleanup = null;
                parentCleanup?.Invoke();
                logger.Info("Home cleanup complete; ribbon release callback invoked.");
            }
```

Read into local (405), stored reference cleared (406), local invoked (407) — in that order, so the
clear strictly precedes the invoke. The three statements remain inside the `finally`, so a throwing
earlier stage still reaches the callback. The target is the auto-property `ParentCleanup`, not a
`_parentCleanup` field, which is the one adaptation this site required.

### Site A' — `QuickFiler/Controllers/EfcHomeController.cs`, delivered lines 342-352

```csharp
        public void Cleanup()
        {
            _globals = null;
            _formViewer = null;
            _explorerController = null;
            _formController = null;
            _keyboardHandler = null;
            System.Action parentCleanup = _parentCleanup; // #810 idiom carried up by #821.
            _parentCleanup = null;
            parentCleanup?.Invoke();
        }
```

Read into local (349), cleared (350), invoked (351). The invocation is additionally null-conditional,
which removes the `NullReferenceException` exposure the previous bare `_parentCleanup.Invoke();`
carried. The five field nullings at 344-348 are unchanged.

### Which outcome each new test exercises

| Test | Outcome exercised |
|---|---|
| `QfcHomeControllerCleanupTests.Cleanup_CalledTwice_InvokesParentCleanupOnce` | Two `Cleanup()` calls on one instance; asserts exactly one invocation via `Times.Once`. Exercises the at-most-once half. |
| `QfcHomeControllerCleanupTests.Cleanup_DisposesTokenSourceAndDetachesWorkerCompleted` | Same, with both calls now preceding the pre-existing `Times.Once` verification that was previously blind to the second call. |
| `EfcHomeControllerLifecycleTests.Cleanup_CalledTwice_InvokesParentCleanupOnce` | Two `Cleanup()` calls on one instance; asserts a **count** equal to 1, not a boolean flag. |
| `QfcHomeControllerCleanupTests.Cleanup_DatamodelCleanupThrows_StillInvokesParentCleanup` (pre-existing) | A throwing datamodel stage still reaches the callback. Exercises the "unconditionally with respect to whether an earlier stage threw" half, and is the direct proof AC15 requires. |

## Invariant 2 — Sites B and B'

> A cancel gesture on a borrowed CancellationTokenSource either requests cancellation, or throws
> `InvalidOperationException` carrying a diagnosable message when no source has been supplied, or
> returns quietly when the owner has already disposed the source — and **no exception of any type
> escapes the WinForms event handler**, which closes or disposes its surface in all three cases.

### Site B — `UtilitiesCS/Threading/ProgressViewer.cs`, delivered lines 82-121

```csharp
        internal void RequestCancel()
        {
            CancellationTokenSource source =
                _cancelSource
                ?? throw new InvalidOperationException(
                    "ProgressViewer cancellation was requested with no CancellationTokenSource. "
                        + "Assign CancelSource or call SetCancellationTokenSource before enabling ButtonCancel."
                );

            try
            {
                source.Cancel();
            }
            catch (ObjectDisposedException)
            {
                logger.Debug(
                    "Cancel requested after the token source was disposed; nothing to cancel."
                );
            }
        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            try
            {
                RequestCancel();
            }
            catch (System.Exception ex)
            {
                logger.Error("ProgressViewer cancel request failed.", ex);
            }
            finally
            {
                this.Close();
            }
        }
```

### Site B' — `UtilitiesCS/Threading/ProgressPane.cs`, delivered lines 66-105

```csharp
        internal void RequestCancel()
        {
            CancellationTokenSource source =
                _tokenSource
                ?? throw new InvalidOperationException(
                    "ProgressPane cancellation was requested with no CancellationTokenSource. "
                        + "Call SetCancellationTokenSource before enabling ButtonCancel."
                );

            try
            {
                source.Cancel();
            }
            catch (ObjectDisposedException)
            {
                logger.Debug(
                    "Cancel requested after the token source was disposed; nothing to cancel."
                );
            }
        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            try
            {
                RequestCancel();
            }
            catch (System.Exception ex)
            {
                logger.Error("ProgressPane cancel request failed.", ex);
            }
            finally
            {
                this.Dispose();
            }
        }
```

The pane's message names `SetCancellationTokenSource` and deliberately does **not** name
`CancelSource`, which this type does not have; a `Select-String -SimpleMatch` search for
`CancelSource` over `ProgressPane.cs` returns 0 matches.

### Which of the three outcomes each new test exercises

| Test (both surfaces carry the same five) | Outcome exercised |
|---|---|
| `RequestCancel_WhenSourceIsNull_ThrowsInvalidOperationExceptionWithMessage` | Outcome 2 — throws `InvalidOperationException` and asserts the message names the supplying member. |
| `RequestCancel_WhenSourceIsDisposed_DoesNotThrow` | Outcome 3 — returns quietly after owner disposal. |
| Handler test, null source | Outcome 2 reached through the handler; asserts nothing escapes and the surface is closed or disposed. |
| Handler test, disposed source | Outcome 3 reached through the handler; asserts nothing escapes and the surface is closed or disposed. |
| `SetCancellationTokenSource_WithNull_DoesNotEnableButton` | The accept-point guard that keeps outcome 2 from being reachable by a user click at all. |
| `CancelSource_WhenAssigned_EnablesButtonAndCancelsSameSourceOnClick` (pre-existing, viewer) | Outcome 1 — a live source is actually cancelled. |
| `CancelButtonClick_WhenInvoked_CancelsTokenSource` (pre-existing, pane) | Outcome 1 — a live source is actually cancelled. |

All three outcomes are therefore exercised on both surfaces, and the "no exception of any type
escapes" clause is asserted by the four handler tests via `NotThrow`, with the closure clause
asserted alongside by `IsDisposed.Should().BeTrue(`.

Output Summary: the delivered statements are quoted for all four sites — the two cleanup sites for
invariant 1 and the two progress surfaces for invariant 2 — and each new test is mapped to the
outcome it exercises. Both invariant sentences hold as written against the delivered code.
