# Phase 5 — AC14 trace correspondence

Timestamp: 2026-09-09T13-34
Task: [P5-T8]

The spec's "Trace of one accepted value through Site B" has four steps. Each row below names the
delivered file and the member that now discharges that step, as the members stand in the delivered
tree.

| # | Trace step | Delivered file | Delivered member | How it discharges the step |
|---|---|---|---|---|
| 1 | **Accept point** — previously assigned the parameter and set `Enabled = true` with no null check | `UtilitiesCS/Threading/ProgressViewer.cs` | `SetCancellationTokenSource`, lines 68-74 | Validates by delegating to the `CancelSource` property setter (line 73, `CancelSource = tokenSource;`), whose body at line 64 reads `ButtonCancel.Enabled = value != null;`. The button can no longer be enabled for a null source. |
| 1 | **Accept point**, sibling surface | `UtilitiesCS/Threading/ProgressPane.cs` | `SetCancellationTokenSource`, lines 51-58 | Validates inline at line 57, `this.ButtonCancel.Enabled = tokenSource is not null;`. The pane has no `CancelSource` property to delegate to, so it keeps the inline check; that asymmetry is the recorded OQ3 resolution. |
| 2 | **Throw point** — previously `_cancelSource!.Cancel();` with the null-forgiving operator | `UtilitiesCS/Threading/ProgressViewer.cs` | `RequestCancel`, lines 82-101 | The null-forgiving dereference is replaced by a guarded call: `?? throw new InvalidOperationException(...)` at line 86 yields a provably non-null local, and `source.Cancel();` at line 93 runs inside a `try` whose `catch (ObjectDisposedException)` at line 95 returns quietly. |
| 2 | **Throw point**, sibling surface | `UtilitiesCS/Threading/ProgressPane.cs` | `RequestCancel`, lines 66-85 | Same shape: `?? throw new InvalidOperationException(...)` at line 70, guarded `source.Cancel();` at line 77, `catch (ObjectDisposedException)` at line 79. |
| 3 | **Absorption point** — previously did not exist anywhere between the click and the message loop | `UtilitiesCS/Threading/ProgressViewer.cs` | `CancelButton_Click`, lines 103-121 | The `catch (System.Exception ex)` at line 113 is the absorption point. It logs through `logger.Error` at line 115 and does not rethrow, so nothing reaches the WinForms message loop. |
| 3 | **Absorption point**, sibling surface | `UtilitiesCS/Threading/ProgressPane.cs` | `CancelButton_Click`, lines 87-105 | `catch (System.Exception ex)` at line 97, logging through `logger.Error` at line 99, no rethrow. |
| 4 | **Closure** — previously `this.Close()` was skipped whenever the cancel threw | `UtilitiesCS/Threading/ProgressViewer.cs` | `CancelButton_Click` `finally`, lines 117-120 | `this.Close();` at line 119 sits in a `finally`, so the form closes on the success path, the null path and the disposed path alike. |
| 4 | **Closure**, sibling surface | `UtilitiesCS/Threading/ProgressPane.cs` | `CancelButton_Click` `finally`, lines 101-104 | `this.Dispose();` at line 103 sits in a `finally`. `ProgressPane` is a `UserControl` rather than a `Form`, so disposal is the correct closure operation. |

## Every named member exists in the delivered tree

Verified by literal search in `[P3-T1]` through `[P3-T8]`:

- `internal void RequestCancel()` — 1 match in each progress file. Neither existed before this change.
- `catch (ObjectDisposedException)` — 1 match in each progress file.
- `catch (System.Exception ex)` — 1 match in each progress file. Neither existed before this change.
- `this.ButtonCancel.Enabled = true;` — **0** matches in both files, so the unconditional enabling
  path is gone from both.
- `!.Cancel()` — **0** matches in both files, so the suppressed dereference is gone from both.

Output Summary: the artifact contains four trace rows, each expressed for both progress surfaces, and
every row names a file and a member that exists in the delivered tree with its delivered line range.
The accept point now validates its argument, the throw point is replaced by a guarded call, the
absorption point exists inside the handler where none existed before, and the surface closes in all
three outcomes.
