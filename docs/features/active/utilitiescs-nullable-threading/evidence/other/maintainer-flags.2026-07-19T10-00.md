# Maintainer Flags — utilitiescs-nullable-threading (Issue #369)

This file collects the flag-only observations the feature surfaces for the maintainer. Sections are appended as their governing tasks complete (P4-T7, P7-T7, P8-T6, P8-T7).

## Environment / Pre-Existing Conditions (recorded during Phase 0)

- Timestamp: 2026-07-19T10-00
- Pre-existing dependabot drift: commit 7de9f11f bumped `packages.config` and each csproj's analyzer `<Import>`/`<Error>` props to new analyzer versions but left the `<Analyzer Include>` DLL paths pinned to older versions. Resolved as a gitignored-`packages/` restore action only (no tracked file edited). See `evidence/baseline/analyzer-build-baseline.*.md`.
- Pre-existing vendored `SVGControl` CS0649 fails the literal solution `/t:Rebuild ... /p:TreatWarningsAsErrors=true` command (the same shape as the CI nullable gate). Outside this feature's `Threading/`-only scope. See `evidence/baseline/nullable-build-baseline.*.md`.

## [P4-T7] ApplicationIdleTimer.cs line-count observation

- Timestamp: 2026-07-19T10-00
- Pre-change line count: 481.
- Post-annotation line count (after `#nullable enable` pragma + in-place `?`/`= null!` annotations + CSharpier): **482**.
- Result: **UNDER the 500-line limit. No annotation-induced breach.** Annotations were kept strictly in-place (no new multi-line guard blocks), so CSharpier reflow added no lines beyond the single pragma line. No maintainer action required for this file.

## [P7-T7] StoreLockupResponder null-branch preservation confirmation

- Timestamp: 2026-07-19T10-40
- Confirmation: the four documented null-store-model branches in `StoreLockupResponder.OnLockupDetected` are **unchanged in order and content**:
  1. No-context guard — `if (string.IsNullOrWhiteSpace(displayName)) return;`
  2. Unresolved-sentinel guard — `if (string.Equals(identity.Value, StoreIdentity.UnresolvedSentinel, ...)) return;`
  3. `<Stores-enumeration>` phase guard (issue #292) — emits one `autoDisabled: false` WARN then `return;`
  4. Already-disabled guard — `if (_disableService.IsDisabled(identity)) return;`
- Only annotations were applied around these branches: the two optional ctor params became `StoreLockupNotifier?`/`Action<string>?`, `displayName` is naturally `string?` (from the Batch-3 `LockupAttribution.StoreIdentity` -> `string?` chain), and a single justified `displayName!` was applied at the `_notify(displayName!, ...)` call site (the delegate's `identity` param is non-null; net481 `IsNullOrWhiteSpace` does not refine null-state).
- **No diagnostic required touching any branch.** No branch was added, removed, reordered, or altered. No maintainer action required.

## [P8-T6] AsyncMultiTasker `timer!` and TimeOutTask return-type-stability decisions

- Timestamp: 2026-07-19T10-55
- (a) **AsyncMultiTasker second-overload `timer!` (NOT `timer?.`)**: in `AsyncMultiTaskChunker<T>` (the `Func<T, Task>` overload), the `ITimerWrapper timer` local starts null and is assigned inside `await Task.Run(...)`; the `catch`/`finally` dereference it via `timer!.StopTimer()`/`timer!.Dispose()`. The `!` preserves the current NRE-if-unassigned behavior. Switching to `timer?.` was rejected because it would swallow the failure and change behavior. Recorded for reviewer confirmation.
- (b) **TimeOutTask keeps public `Task<TResult>`**: all `RunWithTimeout`/`TimeoutAfter` overloads retain the public return type `Task<TResult>`; the unconstrained-`TResult` default paths use `result = default!` / `return result!` rather than widening to the silently-different `Task<TResult?>`. Any genuine desire to widen the downstream contract should be escalated to the maintainer, not made here. Recorded for reviewer confirmation.

## [P8-T7] File-size breach flags (pre-existing and annotation-induced)

- Timestamp: 2026-07-19T10-55
- (a) **`TimeOutTask.cs` — PRE-EXISTING 500-line breach, FLAGGED not fixed.** Pre-change: 975 lines; post-annotation (pragma + in-place `!`/`default!` annotations + CSharpier): **976 lines**. This exceeds the repository 500-line limit as a pre-existing condition. Splitting the ~15 `RunWithTimeout`/`TimeoutAfter` overloads into multiple files is an out-of-scope refactor deferred to a separate issue. Not fixed by this annotation-only feature.
- (b) **`AsyncMultiTasker.cs` — no annotation-induced breach.** Pre-change: 465 lines; post-annotation (pragma + in-place `!` annotations with brief why-comments + CSharpier): **469 lines**. UNDER the 500-line limit. No maintainer action required.
- (c) **`ApplicationIdleTimer.cs` — no annotation-induced breach** (cross-referenced from P4-T7): pre-change 481, post-annotation **482 lines**. UNDER the 500-line limit. No maintainer action required.
- Summary: the only 500-line breach is the pre-existing `TimeOutTask.cs` (976). No file was split; no annotation-induced breach occurred on `ApplicationIdleTimer.cs` or `AsyncMultiTasker.cs`.
