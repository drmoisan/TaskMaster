# Research: `QuickFiler/Controllers/BreadcrumbOutboundQueue.cs`

- Parent epic: #136 (`quickfiler-per-file-coverage`)
- Child feature: #431 F2 (`quickfiler-queue-admission-coverage`)
- File under research: `QuickFiler/Controllers/BreadcrumbOutboundQueue.cs` (67 lines, verified by direct read)
- Evidence basis: direct read of the file; direct read of
  `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterQueueTests.cs` (the only test file that references
  this type); grep confirming no dedicated `BreadcrumbOutboundQueueTests.cs` file exists.

## Current structure

- `public sealed class BreadcrumbOutboundQueue` — `#nullable enable` file. Public surface:
  constructor(`IBreadcrumbWebHost host`), `PendingCount` (get-only `int`), `PostOrQueue(string json)`,
  `OnInitializationCompleted()`.
- Constructor-injected: `IBreadcrumbWebHost host` (interface — clean seam already in place). No other
  dependencies.
- No dependency on `Microsoft.Office.Interop.Outlook.*` anywhere in this file — it belongs to the
  breadcrumb/WebView2 bridge feature area (`#349`), owned jointly by F2 (this file only) and F12/F13
  (`BreadcrumbBridgeRouter` and the WebView2 host implementations).
- No concurrency primitives: a plain `Queue<string> _pending` field, single-threaded FIFO buffer. No
  locks — the class's own doc comment states it is "event-driven only — no polling, no timers, no
  delays," which the source confirms (no `Task`, no `async`, no `Thread.Sleep`/`Task.Delay`).
- No wall-clock or RNG usage.

## Existing test coverage

No dedicated test file exists (`BreadcrumbOutboundQueueTests.cs` is absent from
`QuickFiler.Test/Controllers/`). Coverage instead comes from `BreadcrumbBridgeRouterQueueTests.cs`
(447 lines), whose own doc comment states it exists for "negative/edge-path tests for
`BreadcrumbBridgeRouter` and `BreadcrumbOutboundQueue` (#349)." Relevant tests:

- `OutboundPayloads_BeforeInitialization_AreQueuedAndFlushedInOrder` — drives `PostOrQueue` through the
  router while `IsCoreInitialized` is false (buffering into `_pending`), then flips the flag and calls
  `NotifyCoreInitialized()` (which the router wires to `OnInitializationCompleted()`), asserting the two
  buffered payloads flush in enqueue order.
- `DuplicateInitializationCompletion_IsIdempotent` — calls the completion path twice and asserts no
  double-flush (exercises `OnInitializationCompleted()`'s `while (_pending.Count > 0)` loop when already
  empty on the second call).
- `OutboundQueue_NullArguments_ThrowArgumentNullException` — directly (not via the router) asserts
  `new BreadcrumbOutboundQueue(null)` throws `ArgumentNullException("host")` and
  `queue.PostOrQueue(null)` throws `ArgumentNullException("json")`.

## Coverage gap

- **The "post immediately" branch of `PostOrQueue`** — `if (_host.IsCoreInitialized) { _host.PostMessageJson(json); }` (the `true` branch, taken when the host is already initialized at call time) — is not directly, positively asserted anywhere. Every test that reaches `PostOrQueue` with `_initialized == true` set beforehand (several tests in `BreadcrumbBridgeRouterQueueTests.cs`) asserts that `_posted.Count` stays **unchanged**, i.e. those scenarios exercise router-level no-op paths (unknown row id, provider failure) that never call `PostOrQueue` at all in that state — they do not prove the immediate-post branch itself. No test asserts that `PostOrQueue` posts immediately (rather than buffering) when `IsCoreInitialized` is already `true` at call time.
- **`PendingCount`** is never asserted directly (only implicitly, via flush-order assertions on `_posted`). A direct assertion that `PendingCount` increases while buffering and returns to `0` after flush is missing.
- No test constructs `BreadcrumbOutboundQueue` and exercises it **directly** (outside the router) for the buffering/flush behavior — today's positive-path coverage of buffering and flush is only reached indirectly through `BreadcrumbBridgeRouter`. This is not wrong (it is legitimate integration-style coverage of the collaboration), but a direct unit test isolates this file's own contract from the router's, which is the more appropriate unit for a per-file coverage child.

## `[ExcludeFromCodeCoverage]` disposition

Not applicable — this file carries no such attribute.

## Seam requirements

None. `IBreadcrumbWebHost` is already a clean interface seam and is already used by the existing tests
via `Mock<IBreadcrumbWebHost>`.

## Candidate test cases

| # | Case | Type | Notes |
|---|---|---|---|
| 1 | `PostOrQueue` with `IsCoreInitialized == true` posts immediately via `PostMessageJson` and does not buffer (`PendingCount` stays 0) | Positive | Direct unit test, not via the router |
| 2 | `PostOrQueue` with `IsCoreInitialized == false` buffers the payload and does not call `PostMessageJson`; `PendingCount` increases by one per call | Positive | Direct unit test |
| 3 | `OnInitializationCompleted()` flushes every buffered payload via `PostMessageJson` in enqueue order and drains `PendingCount` back to 0 | Positive/state-transition | Direct unit test; the router-level test already covers the same behavior indirectly, so this closes the direct-unit gap without duplicating assertions |
| 4 | `OnInitializationCompleted()` on an empty buffer is a no-op (`PostMessageJson` never called) | Boundary | Direct unit test of the idempotent-empty-buffer case, isolated from the router |
| 5 | Constructor with `host == null` throws `ArgumentNullException("host")` | Negative | Already covered via `BreadcrumbBridgeRouterQueueTests.OutboundQueue_NullArguments_ThrowArgumentNullException` — no new test needed, listed here only to record that it is already closed |
| 6 | `PostOrQueue(null)` throws `ArgumentNullException("json")` | Negative | Already covered by the same existing test — no new test needed |

## Determinism constraints

None required. The class is synchronous and event-driven with no clock, RNG, or background work.
