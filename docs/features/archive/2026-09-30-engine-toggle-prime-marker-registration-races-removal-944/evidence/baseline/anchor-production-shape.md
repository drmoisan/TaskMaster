# Anchor Production Shape (P0-T6)

Timestamp: 2026-09-30T13-19
Command: git diff --exit-code ANCHOR-SHA -- TaskMaster TaskMaster.Test; CMD-PRIME-SPANS; CMD-PHRASE-COUNT; CMD-TOKEN-COUNT (FILE TaskMaster\Ribbon\EngineToggleStateCoordinator.cs, the twenty-token list of P0-T6)
EXIT_CODE: 0
Output Summary: ANCHOR-CODE-DIFF-EXIT=0. The first fifteen tokens each count 1; lock ( and catch ( each count 1; TaskCompletionSource, SetResult( and ExecuteSynchronously each count 0. JOINED phrase count = 1. CompletePrime span 344-360: _logError at 358 precedes TryRemove at 359; SPAN-TRY, SPAN-CATCH, SPAN-LOCK all 0. COMPLETEPRIME-SHAPE: REPORT-THEN-CLEAR. SPAN [private void StartObservedPrime(] = 0-0.

ANCHOR-SHA: b305903e275b8abf58e8e65831c189f517568fe4

AC1 execution-record statement: at ANCHOR-SHA b305903e275b8abf58e8e65831c189f517568fe4 (origin/main, the merge of the issue 942 fix), CompletePrime reports through the error sink (_logError at line 358) before its marker removal (_primeTasks.TryRemove at line 359). COMPLETEPRIME-SHAPE: REPORT-THEN-CLEAR.

## Diff

ANCHOR-CODE-DIFF-EXIT=0

## Token counts (CMD-TOKEN-COUNT)

| Token | Count | FIRST-LINE |
|---|---|---|
| `_primeTasks[engineName] = StartObservedPrime(engines, engineName, controlId);` | 1 | 278 |
| `if (_primeTasks.ContainsKey(engineName))` | 1 | 273 |
| `private Task StartObservedPrime(` | 1 | 292 |
| `completed => CompletePrime(completed, engineName),` | 1 | 300 |
| `TaskContinuationOptions.None,` | 1 | 302 |
| `The returned continuation task always` | 1 | 289 |
| `private void CompletePrime(Task completed, string engineName)` | 1 | 344 |
| `_primeTasks.TryRemove(engineName, out _);` | 1 | 359 |
| `_logError(BuildPrimeFailedMessage(engineName), failure);` | 1 | 358 |
| `Report-then-clear is load-bearing` | 1 | 355 |
| `cleared only after that report has returned` | 1 | 246 |
| `Serializes the at-most-one-prime decision.` | 1 | 59 |
| `prime per engine key. Its presence is the` | 1 | 73 |
| `internal Task GetPrimeTask(string engineName)` | 1 | 249 |
| `private void StartPrimeIfNeeded(` | 1 | 263 |
| `lock (` | 1 | 271 |
| `catch (` | 1 | 181 |
| `TaskCompletionSource` | 0 | 0 |
| `SetResult(` | 0 | 0 |
| `ExecuteSynchronously` | 0 | 0 |

## Phrase count (CMD-PHRASE-COUNT)

JOINED [The returned continuation task always completes successfully] = 1

## Spans (CMD-PRIME-SPANS)

- SPAN [private void StartPrimeIfNeeded(] = 263-280; SPAN-TRY 0; SPAN-FINALLY 0; SPAN-CATCH 0; SPAN-LOCK 1; SPAN-BEFORE-END-IS-LOCK-CLOSE True; SPAN-LINE lock (_primeGate) = 271; SPAN-LINE if (_primeTasks.ContainsKey(engineName)) = 273; every other SPAN-LINE 0; SPAN-KEYWORD try 0, finally 0
- SPAN [private void StartObservedPrime(] = 0-0 (the method still returns Task at the anchor)
- SPAN [private void CompletePrime(] = 344-360; SPAN-TRY 0; SPAN-FINALLY 0; SPAN-CATCH 0; SPAN-LOCK 0; SPAN-BEFORE-END-IS-LOCK-CLOSE False; SPAN-LINE _logError(BuildPrimeFailedMessage(engineName), failure); = 358; SPAN-LINE _primeTasks.TryRemove(engineName, out _); = 359; every other SPAN-LINE 0; SPAN-KEYWORD try 0, finally 0

COMPLETEPRIME-SHAPE: REPORT-THEN-CLEAR

Verdict: no ANCHOR SHAPE MISMATCH and no COMPLETEPRIME SHAPE MISMATCH.
