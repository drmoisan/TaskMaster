# Code Review — quickfiler-home-controller-metrics-442

- Timestamp: 2026-08-27T14-35 (UTC)
- Branch: `bug/quickfiler-home-controller-metrics-442`, HEAD `6a1b9ca4`
- Base: `origin/epic/quickfiler-bug-family-integration` @ `0ddab410` (three-dot diff; merge base equals the base tip)
- Files reviewed line-by-line: the five owned production files, the two owned metrics test files, and the one-line change to `QuickFiler.Test/Controllers/EfcHomeControllerTests.cs`

## Findings

| ID | Severity | File:Line | Finding |
|---|---|---|---|
| CR-1 | Minor | `QuickFiler/Controllers/QfcHomeController.Metrics.cs:179` | `WriteMetricsAsync` invokes `MetricsFileWriter` even when the null/whitespace filter leaves `lines` empty. The default writer `FileIO2.WriteTextFileAsync` opens an append `StreamWriter`, so an empty-diagnostics session now creates or touches a zero-content session-metrics file. The EFC sibling path guards this (`EfcHomeController.Metrics.cs:74-77`, `if (dataLines.Length == 0) return;`). Pre-fix behavior wrote nothing in this case (the queue was never drained). Recommend adding the matching `if (lines.Length == 0) return;` guard in a follow-up. Behavioral impact is a spurious empty file at worst; not blocking. |
| CR-2 | Minor (pre-existing, surfaced) | `UtilitiesCS/To Depricate/FileIO2.cs:50-89` | The default writer's `IOException` retry loop runs up to 100 iterations of `Task.Delay(100)` (about 10 seconds) and, on final failure, logs and returns as if successful (`success = true`), so a persistently failed flush is silent. Because `WriteMetricsAsync` now awaits this writer directly with `CancellationToken.None`, a locked file stalls the awaiting continuation for the full bounded window with no cancellation path. `FileIO2.cs` is unchanged by this feature and is outside its owned files; the seam design means tests never hit it. Record as a candidate for the promotion lifecycle against `FileIO2` (deprecation-marked module), not as a defect of this change. |
| CR-3 | Minor | `QuickFiler/Controllers/EfcHomeController.Metrics.cs:120-121`, `QfcHomeController.Metrics.cs:48,125-127` | Date/time fields remain culture-sensitive: in .NET custom format strings the `/` and `:` specifiers map to the current culture's date/time separators, so `SentDate.ToString("MM/dd/yyyy")` renders `06.30.2026` under `de-DE`. This cannot change the CSV field count (no comma is introduced), which is why AC-16 deliberately scoped invariance to the six numeric sites, and the related timestamp-content defect (`"hh:mm"` 12-hour) is already promoted as CFN-4 / issue #645 (verified OPEN this session). Recommend widening #645, or a sibling issue, to cover the culture-sensitive date/time separators so the whole timestamp-content question is resolved in one place. |
| CR-4 | Info | `QuickFiler/Controllers/QfcHomeController.Metrics.cs:111,135` | `LOC_TXT_FILE` is assigned and never read (pre-existing; survives the rewrite). Now that the write path passes `filename` + `myDocuments` to the seam, the local is dead and could be deleted along with its declaration in a later cleanup. No analyzer flags it under the current configuration. |
| CR-5 | Info | `QuickFiler/Controllers/QfcHomeController.Metrics.cs:36-105` | The synchronous single-argument `QuickFileMetrics_WRITE(string)` still writes through `FileIO2.WriteTextFile` directly rather than an injectable seam (only its duration source and culture sites were changed, per plan scope). The asymmetry with `WriteMetricsAsync` is intentional and documented by the plan, but it leaves one metrics writer without a test seam; the coverage residue in this member (39/49) is the consequence. Candidate for the coverage-uplift work tracked under #433/#437. |
| CR-6 | Info | `QuickFiler/Controllers/QfcCollectionController.cs:2418-2430` (read-only) | `xComma` sanitizes by replacing commas with `_`, which is lossy (a sanitized value is indistinguishable from a genuine underscore). Consistent with the long-standing precedent for `Subject`; extending it to the other three free-text fields (AC-13) is the right minimal fix. A quoting-based CSV writer would be the durable fix but belongs to the file's owner (feature 468), and CFN-2 already routes collection-controller concerns there. |

**Blocking findings: 0.**

## Review of the load-bearing design points

### 1. Interlocked re-entrancy guard — correct

`EfcHomeController.ExecuteMoves.cs:53-65`:

```csharp
internal bool TryBeginExecuteMoves() =>
    Interlocked.CompareExchange(ref _isExecuting, 1, 0) == 0;   // paraphrased; body verified verbatim
internal void ResetExecuteMovesState() => Interlocked.Exchange(ref _isExecuting, 0);
```

- The compare-and-set is atomic, closing the check-then-act race the `volatile bool` had (spec RC-6). Exactly one caller can observe `0 -> 1`.
- `ExecuteMovesAsync` (`:32-46`) takes the guard, then wraps `ExecuteMovesCoreAsync()` in `try`/`finally` with the reset in `finally`, so an exception in the core cannot leak a permanently-taken guard.
- `Interlocked.Exchange` on release (rather than a plain write) keeps the release visible with full-fence semantics; correct, if slightly stronger than required after an awaited continuation.
- The field is `private int` with a comment explaining why an atomic primitive, not a memory barrier, is required (`EfcHomeController.cs:389-393`). The tests assert the sequential contract (true, false, true-after-reset) and deliberately avoid a non-deterministic concurrent assertion — the right call under the determinism policy.

### 2. Async metrics flush and the `CancellationToken.None` choice — correct and well-reasoned

`QfcHomeController.Metrics.cs:107-180`: the `BlockingCollection`/timer machinery (an `async void` timer handler on a timer that was never started, a guard that could never pass, and a `CompleteAdding` that was never called) is deleted rather than repaired — the simplest design that works, per policy. The flush is now a single awaited call through an injectable delegate:

- The awaited `MetricsFileWriter(...)` task completes before `WriteMetricsAsync`'s task completes (pinned by `WriteMetricsAsync_CompletesWriterTaskBeforeReturning` using `Task.Yield`, no wall clock).
- `CancellationToken.None` is deliberate and documented in-code: the dispatcher continuation carrying this write is not awaited to completion by its caller (CFN-3, owned by a sibling), so a session cancel can arrive mid-write and must not destroy the metrics. Pinned by `WriteMetricsAsync_PassesUncancelledTokenToWriter`, which cancels `TokenSource` before invoking. This preserves the pre-existing `default`-token precedent at the old consumer site.
- The seam is an `internal` settable property defaulting to the production writer; production behavior is unchanged while tests capture arguments without filesystem access. The delegate's parameter order is documented against the EFC precedent.
- Residual risk is confined to the default writer's own semantics (CR-1, CR-2 above).

### 3. `int` to `double` widening and rounding — correct

`EfcHomeController.Metrics.cs:53-58, 84-88`: both internal writers now take `double elapsedSeconds`, and the interface-driven call site passes `_stopWatch.Elapsed.TotalSeconds`. `duration /= moved.Count` is now real division; the behavior change (2.6667 renders as `3` under `##0` where integer division rendered `2`) is pinned by `BuildQuickFileMetricLines_WithMultipleMovedItems_PinsRealDivisionRounding` and stated for the PR body. `git grep "int elapsedSeconds"` over `QuickFiler/` returns no match (run this session). The QFC side reads `_stopWatchMoved.Elapsed.TotalSeconds` in both writers, and the calendar span is reconstructed by subtracting the measured `TimeSpan` rather than a truncated integer cast, so the appointment span and the CSV duration agree by construction.

### 4. Culture invariance — the six numeric sites are correct; dates remain culture-sensitive by scope decision

All six numeric format sites now pass `CultureInfo.InvariantCulture` (verified in the diff: four in `QfcHomeController.Metrics.cs`, two in `EfcHomeController.Metrics.cs`), closing the field-count corruption a decimal comma would cause. Both `de-DE` tests assert the invariant separator and the 12-field shape and restore culture in `finally`. The remaining date/time separator sensitivity is CR-3.

### 5. CSV field sanitization — correct within precedent

`BuildQuickFileMetricLines` now applies `QfcCollectionController.xComma` to all four free-text fields (`Subject`, `ToRecipientsName`, `SenderName`, `selectedFolder`), and the missing separator between recipient and sender is restored, moving the row from 11 concatenated-adjacent fields to 12. Pinned by the 12-field split test, the `,Recipient,Sender,` substring assertion, and the embedded-comma test. The previously pinning assertion for the concatenated form was updated deliberately in the same change, as the spec required. Lossiness of `_`-substitution is CR-6.

### 6. Resource and exception handling

- No new disposable state is introduced; the deleted code removes an undisposed `System.Timers.Timer` and an async-void handler that rethrew after logging (an unobservable crash vector on a timer thread). Net improvement.
- The new one-argument EFC overload replaces a public `NotImplementedException` throw with a guarded no-op plus delegation; the silent no-op mirrors the three-argument overload's existing precedent and is documented in XML remarks. Fail-fast purists could argue for logging the skip, but matching the established precedent at an interface boundary is the defensible choice here.
- `SelectMoveMetricsItems` is pure and shared between the execute path and the new overload — no copy-paste.

### 7. Test quality

Both test files follow Arrange-Act-Assert, carry intent docstrings on every non-obvious test, assert with explanatory `because` strings, and avoid the entire banned-API list (verified by grep this session). Reflection seams (`SetPrivateField`, `StoppedStopwatchWithElapsed`, `FormatterServices.GetUninitializedObject` for headless WinForms types) are each justified in comments. The one-line `EfcHomeControllerTests.cs` change is the minimal caller update for the field-type change and is covered by the ownership-deviation record.
