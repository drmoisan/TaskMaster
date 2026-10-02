# Anchor Production Shape (P0-T6)

Timestamp: 2026-10-01T22-59
Command: git diff --exit-code MERGE-BASE -- TaskMaster TaskMaster.Test (MERGE-BASE 59cbab04f1c854baa2a03b6cbf755c1df4f961b4); CMD-COMPLETEPRIME-SHAPE; CMD-TOKEN-COUNT with FILE TaskMaster\Ribbon\EngineToggleStateCoordinator.cs
EXIT_CODE: 0
Output Summary: ANCHOR-CODE-DIFF-EXIT=0; SHAPE: S; CompletePrime span 391-416; sink 408, TryRemove 415 (last statement), comment 402 (four lines); try 406 < sink 408 < catch 410 < catch end 413 < TryRemove 415; E4-ANCHOR-TEXT `/// been attempted.`; every single-instance anchor counts 1; MERGE-BASE-LINES 476, EXPECTED-DELTA 20, SIZE-BUDGET 23 (delta within budget).

ANCHOR-CODE-DIFF-EXIT=0

## CMD-COMPLETEPRIME-SHAPE output

```
FILE-LINES: 476
FILE-CATCH-CODE-LINES: 3
FILE-TRY-CODE-LINES: 4
FILE-FINALLY-CODE-LINES: 1
FILE-LOCK-LINES: 1
SINK-GUARD-TOKEN: 1
SPAN [CompletePrime] = 391-416
SPAN-TRY: 1
SPAN-CATCH: 1
SPAN-FINALLY: 0
SPAN-LOCK: 0
SINK-LINE: 408 count=1
REMOVE-LINE: 415 count=1
COMMENT-LINE: 402 count=1
REPORTKEY-LINE: 0 count=0
GUARD-LINE: 0 count=0
RECORD-LINE: 0 count=0
TRY-LINE: 406
CATCH-LINE: 410
CATCH-END-LINE: 413
FINALLY-LINE: 0
LAST-STATEMENT-LINE: 415
REMOVE-IS-LAST: True
COMMENT-LINES: 4
SHAPE: S
E4-ANCHOR-TEXT: /// been attempted.
ANCHOR [>(StringComparer.Ordinal);] = 1
ANCHOR [Observes the outcome of a prime.] = 1
ANCHOR [/// </remarks>] = 6
ANCHOR ["Reading the activation state for engine '{0}' failed; its toggle continues to "] = 1
ANCHOR [+ "report unchecked.",] = 1
ANCHOR [until the report has] = 1
ANCHOR [internal Task GetPrimeTask(string engineName)] = 1
ANCHOR [private void CompletePrime(Task completed, string engineName)] = 1
ANCHOR [private static string BuildPrimeFailedMessage(string engineName)] = 1
```

`/// </remarks>` = 6 is recorded as an observation (the E2b insertion point is the first one after the CompletePrime summary, located by position).

## CMD-TOKEN-COUNT output

```
TOKEN [_reportedPrimeFaults] = 0
TOKEN [deliberately suppressed as a repeat of] = 0
TOKEN [Repeat suppression (issue #948)] = 0
TOKEN [report (if any) has returned] = 0
TOKEN [logged again.] = 0
TOKEN [lock (] = 1
TOKEN [TaskCompletionSource] = 2
TOKEN [SetResult(] = 1
TOKEN [#nullable] = 0
FIRST-LINE [_reportedPrimeFaults] = 0
FIRST-LINE [deliberately suppressed as a repeat of] = 0
FIRST-LINE [Repeat suppression (issue #948)] = 0
FIRST-LINE [report (if any) has returned] = 0
FIRST-LINE [logged again.] = 0
FIRST-LINE [lock (] = 283
FIRST-LINE [TaskCompletionSource] = 294
FIRST-LINE [SetResult(] = 333
FIRST-LINE [#nullable] = 0
```

## Acceptance checks

- SHAPE: S (no SHAPE-M-ADMITTED-BY record exists; S is the expected shape)
- SINK-LINE, REMOVE-LINE and COMMENT-LINE each count=1; REMOVE-IS-LAST: True
- GUARD-LINE, RECORD-LINE and REPORTKEY-LINE each count=0
- COMMENT-LINES: 4 (expected 4 under S)
- every ANCHOR count is 1 except `/// </remarks>` (observation, 6)
- E4-ANCHOR-TEXT: `/// been attempted.` (shape S)
- shape S rows: SPAN-TRY 1, SPAN-CATCH 1, SPAN-FINALLY 0, SPAN-LOCK 0, SINK-GUARD-TOKEN 1; TRY-LINE 406 < SINK-LINE 408 < CATCH-LINE 410 < CATCH-END-LINE 413 < REMOVE-LINE 415
- first five TOKEN counts 0; `#nullable` 0; `lock (` 1; `TaskCompletionSource` 2 and `SetResult(` 1 (each at least 1; the #944 shape is present)

## AC-L baseline

- BASELINE-SPAN-TRY: 1
- BASELINE-SPAN-CATCH: 1
- BASELINE-SPAN-FINALLY: 0
- BASELINE-SPAN-LOCK: 0
- BASELINE-FILE-CATCH-CODE-LINES: 3
- BASELINE-FILE-TRY-CODE-LINES: 4
- BASELINE-FILE-FINALLY-CODE-LINES: 1

## Size budget

- MERGE-BASE-LINES: 476
- EXPECTED-DELTA: 20 (shape S, Delivered Source arithmetic)
- SIZE-BUDGET: 23 (499 minus 476)
- EXPECTED-DELTA does not exceed SIZE-BUDGET.

## Re-derived anchor positions

CompletePrime span 391-416; comment 402; try 406; sink 408; catch 410; catch end 413; TryRemove 415; last statement 415. These values are re-derived positions; no line number written in the plan is used by a later gate.
