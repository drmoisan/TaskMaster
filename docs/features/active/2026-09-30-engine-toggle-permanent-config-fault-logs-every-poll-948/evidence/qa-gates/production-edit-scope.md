# Production Edit Scope (P2-T7)

Timestamp: 2026-10-01T23-55
Command: CMD-TOKEN-COUNT with FILE TaskMaster\Ribbon\EngineToggleStateCoordinator.cs and TOKENS-PROD; CMD-STRIPPED-COUNT with the two declaration tokens; CMD-COMPLETEPRIME-SHAPE; the P2-T7 added-lines payload over git diff -U0 MERGE-BASE -- TaskMaster/Ribbon/EngineToggleStateCoordinator.cs (MERGE-BASE 59cbab04f1c854baa2a03b6cbf755c1df4f961b4); git diff --numstat MERGE-BASE -- TaskMaster/Ribbon/EngineToggleStateCoordinator.cs; git status --porcelain -- TaskMaster/Ribbon
EXIT_CODE: 0
Output Summary: every TOKENS-PROD clause holds (first fifteen tokens 1 each, old E3 line 0, _reportedPrimeFaults 4, banned tokens 0, lock ( 1, both STRIPPED 1); SHAPE S re-read; reportKey 420 < guard 421 < try 423 < sink 425 < record 426 < catch 428 < catch end 431 < TryRemove 434 (last statement); span try/catch/finally/lock 1/1/0/0 and file catch/try/finally 3/4/1 equal the P0-T6 baseline; NET try/catch/finally lines 0; ADDED-LINE-COUNT 29; OBSERVED-DELTA 20 equals EXPECTED-DELTA 20; every dropped line is REPLACED and is a summary, comment, E3 or E4 line; numstat 29/9; porcelain names only the production file.

This run is on the edited file before the P2-T9 scoped format.

## TOKENS-PROD

```
TOKEN [keyed by engine and base-exception type;] = 1
TOKEN [Never cleared: a cached key never primes.] = 1
TOKEN [(string EngineName, Type FaultType),] = 1
TOKEN [unless the same failure kind was already reported for this engine] = 1
TOKEN [Repeat suppression (issue #948)] = 1
TOKEN [directly after the sink call, so a sink that throws leaves the report owed.] = 1
TOKEN [report (if any) has returned] = 1
TOKEN [deliberately skipped as an already reported kind] = 1
TOKEN [var reportKey = (EngineName: engineName, FaultType: failure.GetType());] = 1
TOKEN [if (!_reportedPrimeFaults.ContainsKey(reportKey))] = 1
TOKEN [_reportedPrimeFaults[reportKey] = 0;] = 1
TOKEN [_logError(BuildPrimeFailedMessage(engineName), failure);] = 1
TOKEN [_primeTasks.TryRemove(engineName, out _);] = 1
TOKEN [+ "report unchecked. Further failures of this kind for this engine are not "] = 1
TOKEN [+ "logged again.",] = 1
TOKEN [+ "report unchecked.",] = 0
TOKEN [deliberately suppressed as a repeat of] = 1
TOKEN [_reportedPrimeFaults] = 4
TOKEN [_reportedPrimeFaults.TryAdd(] = 0
TOKEN [_reportedPrimeFaults.TryRemove(] = 0
TOKEN [_reportedPrimeFaults.Clear(] = 0
TOKEN [until the report has] = 0
TOKEN [lock (] = 1
TOKEN [Monitor.] = 0
TOKEN [SemaphoreSlim] = 0
TOKEN [Mutex] = 0
TOKEN [ReaderWriterLockSlim] = 0
TOKEN [#nullable] = 0
STRIPPED [ConcurrentDictionary<(stringEngineName,TypeFaultType),byte>_reportedPrimeFaults] = 1
STRIPPED [>_reportedPrimeFaults=newConcurrentDictionary<(string,Type),byte>();] = 1
```

## Shape (CMD-COMPLETEPRIME-SHAPE)

```
FILE-LINES: 496
FILE-CATCH-CODE-LINES: 3
FILE-TRY-CODE-LINES: 4
FILE-FINALLY-CODE-LINES: 1
FILE-LOCK-LINES: 1
SINK-GUARD-TOKEN: 1
SPAN [CompletePrime] = 405-435
SPAN-TRY: 1
SPAN-CATCH: 1
SPAN-FINALLY: 0
SPAN-LOCK: 0
SINK-LINE: 425 count=1
REMOVE-LINE: 434 count=1
COMMENT-LINE: 416 count=1
REPORTKEY-LINE: 420 count=1
GUARD-LINE: 421 count=1
RECORD-LINE: 426 count=1
TRY-LINE: 423
CATCH-LINE: 428
CATCH-END-LINE: 431
FINALLY-LINE: 0
LAST-STATEMENT-LINE: 434
REMOVE-IS-LAST: True
COMMENT-LINES: 4
SHAPE: S
E4-ANCHOR-TEXT: /// been attempted or deliberately suppressed as a repeat of a kind already reported.
ANCHOR [>(StringComparer.Ordinal);] = 1
ANCHOR [Observes the outcome of a prime.] = 1
ANCHOR [/// </remarks>] = 6
ANCHOR ["Reading the activation state for engine '{0}' failed; its toggle continues to "] = 1
ANCHOR [+ "report unchecked.",] = 0
ANCHOR [until the report has] = 0
ANCHOR [internal Task GetPrimeTask(string engineName)] = 1
ANCHOR [private void CompletePrime(Task completed, string engineName)] = 1
ANCHOR [private static string BuildPrimeFailedMessage(string engineName)] = 1
```

## Added lines

```
ADDED-LINE-COUNT: 29
DROPPED-LINE-COUNT: 9
ADDED-CATCH-LINES: 0
DROPPED-CATCH-LINES: 0
NET-CATCH-LINES: 0
ADDED-TRY-LINES: 0
DROPPED-TRY-LINES: 0
NET-TRY-LINES: 0
ADDED-FINALLY-LINES: 0
DROPPED-FINALLY-LINES: 0
NET-FINALLY-LINES: 0
ADDED-TOKEN [lock (] = 0
ADDED-TOKEN [lock(] = 0
ADDED-TOKEN [Monitor] = 0
ADDED-TOKEN [SemaphoreSlim] = 0
ADDED-TOKEN [Mutex] = 0
ADDED-TOKEN [ReaderWriterLockSlim] = 0
ADDED-TOKEN [TimeProvider] = 0
ADDED-TOKEN [DateTime] = 0
DROPPED [REPLACED]:         /// been attempted.
DROPPED [REPLACED]:         /// <c>logError</c>, and only then is the in-flight marker cleared so a later read may
DROPPED [REPLACED]:         /// re-prime. A failure thrown by the sink itself is contained here, so the marker is
DROPPED [REPLACED]:         /// cleared whether or not the report succeeded.
DROPPED [REPLACED]:             // Report-then-clear is load-bearing: the marker stays registered until the report has
DROPPED [REPLACED]:             // returned or thrown, so a caller that observes the marker absent — including one that
DROPPED [REPLACED]:             // fetched the prime handle after the fault — is guaranteed the report has already been
DROPPED [REPLACED]:             // attempted.
DROPPED [REPLACED]:                     + "report unchecked.",
```

Every DROPPED [REPLACED] line is the E4 anchor line, a summary line, a comment line or the E3 string line; no DROPPED [REINDENTED] line exists in this pre-format run.

OBSERVED-DELTA: 20 (FILE-LINES 496 minus MERGE-BASE-LINES 476; equals EXPECTED-DELTA 20)

Numstat:

```
29	9	TaskMaster/Ribbon/EngineToggleStateCoordinator.cs
```

Porcelain (TaskMaster/Ribbon):

```
 M TaskMaster/Ribbon/EngineToggleStateCoordinator.cs
```

The untracked new partial lives under TaskMaster.Test/Ribbon, outside this porcelain scope; the span names only the production file.

## Acceptance (AC-K and AC-L observations)

- SHAPE equals the P0-T6 value (S)
- SINK, REMOVE, COMMENT, GUARD, RECORD and REPORTKEY lines each count=1
- REPORTKEY 420 < GUARD 421 < SINK 425; RECORD 426 equals SINK plus 1; REMOVE 434 > RECORD; REMOVE-IS-LAST True
- COMMENT-LINES 4; COMMENT 416 < REPORTKEY 420
- SPAN-TRY 1, SPAN-CATCH 1, SPAN-FINALLY 0, SPAN-LOCK 0 equal BASELINE-SPAN-*; FILE-CATCH 3, FILE-TRY 4, FILE-FINALLY 1 equal BASELINE-FILE-*; FILE-LOCK-LINES 1
- shape S: GUARD 421 < TRY 423 < SINK 425 < CATCH 428 < CATCH-END 431 < REMOVE 434

## PRECOMMIT-FORMAT-RECHECK: (P2-T9, after the scoped CSharpier format)

Timestamp: 2026-10-02T00-01. TOKENS-PROD, both STRIPPED tokens and CMD-COMPLETEPRIME-SHAPE re-printed values identical to the pre-format run above (every token count unchanged; FILE-LINES 496; SHAPE S; reportKey 420, guard 421, try 423, sink 425, record 426, catch 428, catch end 431, TryRemove 434 last; COMMENT-LINES 4; span try/catch/finally/lock 1/1/0/0; file catch/try/finally/lock 3/4/1/1). The added-lines payload on the formatted file:

```
ADDED-LINE-COUNT: 35
DROPPED-LINE-COUNT: 15
ADDED-CATCH-LINES: 1
DROPPED-CATCH-LINES: 1
NET-CATCH-LINES: 0
ADDED-TRY-LINES: 1
DROPPED-TRY-LINES: 1
NET-TRY-LINES: 0
ADDED-FINALLY-LINES: 0
DROPPED-FINALLY-LINES: 0
NET-FINALLY-LINES: 0
ADDED-TOKEN [lock (] = 0
ADDED-TOKEN [lock(] = 0
ADDED-TOKEN [Monitor] = 0
ADDED-TOKEN [SemaphoreSlim] = 0
ADDED-TOKEN [Mutex] = 0
ADDED-TOKEN [ReaderWriterLockSlim] = 0
ADDED-TOKEN [TimeProvider] = 0
ADDED-TOKEN [DateTime] = 0
DROPPED [REPLACED]:         /// been attempted.
DROPPED [REPLACED]:         /// <c>logError</c>, and only then is the in-flight marker cleared so a later read may
DROPPED [REPLACED]:         /// re-prime. A failure thrown by the sink itself is contained here, so the marker is
DROPPED [REPLACED]:         /// cleared whether or not the report succeeded.
DROPPED [REPLACED]:             // Report-then-clear is load-bearing: the marker stays registered until the report has
DROPPED [REPLACED]:             // returned or thrown, so a caller that observes the marker absent — including one that
DROPPED [REPLACED]:             // fetched the prime handle after the fault — is guaranteed the report has already been
DROPPED [REPLACED]:             // attempted.
DROPPED [REINDENTED]:             try
DROPPED [REINDENTED]:             {
DROPPED [REINDENTED]:                 _logError(BuildPrimeFailedMessage(engineName), failure);
DROPPED [REINDENTED]:             }
DROPPED [REINDENTED]:             catch (Exception)
DROPPED [REINDENTED]:                 // Intentionally discarded: see the remarks on this method.
DROPPED [REPLACED]:                     + "report unchecked.",
```

Numstat after format: `35	15	TaskMaster/Ribbon/EngineToggleStateCoordinator.cs`; porcelain (TaskMaster/Ribbon): ` M TaskMaster/Ribbon/EngineToggleStateCoordinator.cs`. Every REINDENTED line, trimmed, is on the allow-list (`try`, `{`, `}`, `catch (Exception)`, the sink line, the discarded comment); every REPLACED line is a summary, comment, E3 or E4 line; NET try/catch/finally 0; ADDED-LINE-COUNT 35 is at least 24; OBSERVED-DELTA 20 equals EXPECTED-DELTA. Every P2-T7 clause holds; no repair was needed.
