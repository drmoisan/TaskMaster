# Production Edit Scope (P2-T6)

Timestamp: 2026-09-30T13-38
Command: CMD-TOKEN-COUNT (FILE TaskMaster\Ribbon\EngineToggleStateCoordinator.cs, TOKEN list TOKENS-PROD); CMD-PRIME-SPANS; CMD-PHRASE-COUNT; the P2-T6 added-lines payload over git diff -U0 ANCHOR-SHA -- TaskMaster/Ribbon/EngineToggleStateCoordinator.cs; git diff --numstat ANCHOR-SHA -- TaskMaster/Ribbon/EngineToggleStateCoordinator.cs (ANCHOR-SHA b305903e275b8abf58e8e65831c189f517568fe4)
EXIT_CODE: 0
Output Summary:
Every token clause holds: the 26 exactly-1 tokens each count 1 and the 19 zero tokens each count 0.
JOINED [The returned continuation task always completes successfully] = 0 (it was 1 at P0-T6).
StartPrimeIfNeeded span 264-289: SPAN-LOCK 1, SPAN-TRY 0, SPAN-CATCH 0; lock 272 < ContainsKey 274 < Registration comment 279 < marker 283; RunContinuationsAsynchronously 284 = 283 + 1; store 286 > 284; call 287 = 286 + 1 and < 288; SPAN-BEFORE-END-IS-LOCK-CLOSE True.
StartObservedPrime span 303-327: SPAN-TRY 1, SPAN-FINALLY 1, SPAN-CATCH 0, SPAN-LOCK 0; 310 < try 314 < 316 < finally 318 < 320 < 323 < 324 < 325.
CompletePrime span 366-382: _logError 380 < TryRemove 381; SPAN-TRY 0, SPAN-CATCH 0, SPAN-LOCK 0.
ADDED-LINE-COUNT: 34; REMOVED-LINE-COUNT: 12; ADDED-CATCH-LINES: 0; every ADDED-TOKEN count is 0.
numstat: 34	12	TaskMaster/Ribbon/EngineToggleStateCoordinator.cs
Verdict: every P2-T6 acceptance clause (tokens, spans, added lines) holds.

Substitutions recorded: (1) the added-lines payload was run with each `"...$(...)"` interpolated string rewritten as string concatenation, because the interpolated form with nested double quotes exited 1 with no output under pwsh -Command; the computed expressions are unchanged. (2) The first attempt of that payload was refused by a PreToolUse hook (EPIC_WORKTREE_REMOVAL_BLOCKED, a pattern false positive on a read-only command; no worktree operation was involved), so the variable holding the deleted lines and its printed labels were renamed: the command printed `DELETED-LINE-COUNT:` and `DELETED:`, which are transcribed below under the plan's labels `REMOVED-LINE-COUNT:` and `REMOVED:` with the values unchanged.

## Details: tokens (CMD-TOKEN-COUNT, TOKENS-PROD)

```
TOKEN [Serializes the at-most-one-prime decision.] = 1
TOKEN [marker registration, and the start of the prime] = 1
TOKEN [task start; no await occurs inside it.] = 0
TOKEN [The registration marker per engine key: registered before the prime starts] = 1
TOKEN [prime per engine key. Its presence is the] = 0
TOKEN [private void StartPrimeIfNeeded(] = 1
TOKEN [lock (_primeGate)] = 1
TOKEN [if (_primeTasks.ContainsKey(engineName))] = 1
TOKEN [Registration precedes the start (issue #944)] = 1
TOKEN [var marker = new TaskCompletionSource<bool>(] = 1
TOKEN [TaskCreationOptions.RunContinuationsAsynchronously] = 1
TOKEN [_primeTasks[engineName] = marker.Task;] = 1
TOKEN [StartObservedPrime(engines, engineName, controlId, marker);] = 1
TOKEN [_primeTasks[engineName] = StartObservedPrime(] = 0
TOKEN [private void StartObservedPrime(] = 1
TOKEN [private Task StartObservedPrime(] = 0
TOKEN [TaskCompletionSource<bool> marker] = 1
TOKEN [_ = ApplyPrimeAsync(engines, engineName, controlId)] = 1
TOKEN [return ApplyPrimeAsync(] = 0
TOKEN [completed => CompletePrime(completed, engineName),] = 0
TOKEN [CompletePrime(completed, engineName);] = 1
TOKEN [marker.SetResult(true);] = 1
TOKEN [SetResult(] = 1
TOKEN [SetException(] = 0
TOKEN [SetCanceled(] = 0
TOKEN [TrySet] = 0
TOKEN [CancellationToken.None,] = 1
TOKEN [TaskContinuationOptions.None,] = 1
TOKEN [TaskScheduler.Default] = 1
TOKEN [ExecuteSynchronously] = 0
TOKEN [The continuation task itself is discarded;] = 1
TOKEN [the value a test awaits is the marker] = 1
TOKEN [The returned continuation task always] = 0
TOKEN [catch (] = 1
TOKEN [lock (] = 1
TOKEN [_primeTasks[] = 1
TOKEN [_primeTasks.TryRemove(engineName, out _);] = 1
TOKEN [_primeTasks.TryAdd(] = 0
TOKEN [_primeTasks.AddOrUpdate(] = 0
TOKEN [_primeTasks.GetOrAdd(] = 0
TOKEN [_primeTasks.Clear(] = 0
TOKEN [Monitor.] = 0
TOKEN [SemaphoreSlim] = 0
TOKEN [Mutex] = 0
TOKEN [ReaderWriterLockSlim] = 0
FIRST-LINE [Serializes the at-most-one-prime decision.] = 59
FIRST-LINE [marker registration, and the start of the prime] = 60
FIRST-LINE [task start; no await occurs inside it.] = 0
FIRST-LINE [The registration marker per engine key: registered before the prime starts] = 73
FIRST-LINE [prime per engine key. Its presence is the] = 0
FIRST-LINE [private void StartPrimeIfNeeded(] = 264
FIRST-LINE [lock (_primeGate)] = 272
FIRST-LINE [if (_primeTasks.ContainsKey(engineName))] = 274
FIRST-LINE [Registration precedes the start (issue #944)] = 279
FIRST-LINE [var marker = new TaskCompletionSource<bool>(] = 283
FIRST-LINE [TaskCreationOptions.RunContinuationsAsynchronously] = 284
FIRST-LINE [_primeTasks[engineName] = marker.Task;] = 286
FIRST-LINE [StartObservedPrime(engines, engineName, controlId, marker);] = 287
FIRST-LINE [_primeTasks[engineName] = StartObservedPrime(] = 0
FIRST-LINE [private void StartObservedPrime(] = 303
FIRST-LINE [private Task StartObservedPrime(] = 0
FIRST-LINE [TaskCompletionSource<bool> marker] = 307
FIRST-LINE [_ = ApplyPrimeAsync(engines, engineName, controlId)] = 310
FIRST-LINE [return ApplyPrimeAsync(] = 0
FIRST-LINE [completed => CompletePrime(completed, engineName),] = 0
FIRST-LINE [CompletePrime(completed, engineName);] = 316
FIRST-LINE [marker.SetResult(true);] = 320
FIRST-LINE [SetResult(] = 320
FIRST-LINE [SetException(] = 0
FIRST-LINE [SetCanceled(] = 0
FIRST-LINE [TrySet] = 0
FIRST-LINE [CancellationToken.None,] = 323
FIRST-LINE [TaskContinuationOptions.None,] = 324
FIRST-LINE [TaskScheduler.Default] = 325
FIRST-LINE [ExecuteSynchronously] = 0
FIRST-LINE [The continuation task itself is discarded;] = 298
FIRST-LINE [the value a test awaits is the marker] = 299
FIRST-LINE [The returned continuation task always] = 0
FIRST-LINE [catch (] = 182
FIRST-LINE [lock (] = 272
FIRST-LINE [_primeTasks[] = 286
FIRST-LINE [_primeTasks.TryRemove(engineName, out _);] = 381
FIRST-LINE [_primeTasks.TryAdd(] = 0
FIRST-LINE [_primeTasks.AddOrUpdate(] = 0
FIRST-LINE [_primeTasks.GetOrAdd(] = 0
FIRST-LINE [_primeTasks.Clear(] = 0
FIRST-LINE [Monitor.] = 0
FIRST-LINE [SemaphoreSlim] = 0
FIRST-LINE [Mutex] = 0
FIRST-LINE [ReaderWriterLockSlim] = 0
```

## Details: spans (CMD-PRIME-SPANS) and phrase (CMD-PHRASE-COUNT)

```
SPAN [private void StartPrimeIfNeeded(] = 264-289
SPAN-TRY [private void StartPrimeIfNeeded(] = 0
SPAN-FINALLY [private void StartPrimeIfNeeded(] = 0
SPAN-CATCH [private void StartPrimeIfNeeded(] = 0
SPAN-LOCK [private void StartPrimeIfNeeded(] = 1
SPAN-BEFORE-END-IS-LOCK-CLOSE [private void StartPrimeIfNeeded(] = True
SPAN-KEYWORD [private void StartPrimeIfNeeded(] [try] = 0
SPAN-KEYWORD [private void StartPrimeIfNeeded(] [finally] = 0
SPAN-LINE [private void StartPrimeIfNeeded(] [lock (_primeGate)] = 272
SPAN-LINE [private void StartPrimeIfNeeded(] [if (_primeTasks.ContainsKey(engineName))] = 274
SPAN-LINE [private void StartPrimeIfNeeded(] [Registration precedes the start (issue #944)] = 279
SPAN-LINE [private void StartPrimeIfNeeded(] [var marker = new TaskCompletionSource<bool>(] = 283
SPAN-LINE [private void StartPrimeIfNeeded(] [TaskCreationOptions.RunContinuationsAsynchronously] = 284
SPAN-LINE [private void StartPrimeIfNeeded(] [_primeTasks[engineName] = marker.Task;] = 286
SPAN-LINE [private void StartPrimeIfNeeded(] [StartObservedPrime(engines, engineName, controlId, marker);] = 287
SPAN-LINE [private void StartPrimeIfNeeded(] [_ = ApplyPrimeAsync(engines, engineName, controlId)] = 0
SPAN-LINE [private void StartPrimeIfNeeded(] [CompletePrime(completed, engineName);] = 0
SPAN-LINE [private void StartPrimeIfNeeded(] [marker.SetResult(true);] = 0
SPAN-LINE [private void StartPrimeIfNeeded(] [CancellationToken.None,] = 0
SPAN-LINE [private void StartPrimeIfNeeded(] [TaskContinuationOptions.None,] = 0
SPAN-LINE [private void StartPrimeIfNeeded(] [TaskScheduler.Default] = 0
SPAN-LINE [private void StartPrimeIfNeeded(] [_logError(BuildPrimeFailedMessage(engineName), failure);] = 0
SPAN-LINE [private void StartPrimeIfNeeded(] [_primeTasks.TryRemove(engineName, out _);] = 0
SPAN [private void StartObservedPrime(] = 303-327
SPAN-TRY [private void StartObservedPrime(] = 1
SPAN-FINALLY [private void StartObservedPrime(] = 1
SPAN-CATCH [private void StartObservedPrime(] = 0
SPAN-LOCK [private void StartObservedPrime(] = 0
SPAN-BEFORE-END-IS-LOCK-CLOSE [private void StartObservedPrime(] = False
SPAN-KEYWORD [private void StartObservedPrime(] [try] = 314
SPAN-KEYWORD [private void StartObservedPrime(] [finally] = 318
SPAN-LINE [private void StartObservedPrime(] [lock (_primeGate)] = 0
SPAN-LINE [private void StartObservedPrime(] [if (_primeTasks.ContainsKey(engineName))] = 0
SPAN-LINE [private void StartObservedPrime(] [Registration precedes the start (issue #944)] = 0
SPAN-LINE [private void StartObservedPrime(] [var marker = new TaskCompletionSource<bool>(] = 0
SPAN-LINE [private void StartObservedPrime(] [TaskCreationOptions.RunContinuationsAsynchronously] = 0
SPAN-LINE [private void StartObservedPrime(] [_primeTasks[engineName] = marker.Task;] = 0
SPAN-LINE [private void StartObservedPrime(] [StartObservedPrime(engines, engineName, controlId, marker);] = 0
SPAN-LINE [private void StartObservedPrime(] [_ = ApplyPrimeAsync(engines, engineName, controlId)] = 310
SPAN-LINE [private void StartObservedPrime(] [CompletePrime(completed, engineName);] = 316
SPAN-LINE [private void StartObservedPrime(] [marker.SetResult(true);] = 320
SPAN-LINE [private void StartObservedPrime(] [CancellationToken.None,] = 323
SPAN-LINE [private void StartObservedPrime(] [TaskContinuationOptions.None,] = 324
SPAN-LINE [private void StartObservedPrime(] [TaskScheduler.Default] = 325
SPAN-LINE [private void StartObservedPrime(] [_logError(BuildPrimeFailedMessage(engineName), failure);] = 0
SPAN-LINE [private void StartObservedPrime(] [_primeTasks.TryRemove(engineName, out _);] = 0
SPAN [private void CompletePrime(] = 366-382
SPAN-TRY [private void CompletePrime(] = 0
SPAN-FINALLY [private void CompletePrime(] = 0
SPAN-CATCH [private void CompletePrime(] = 0
SPAN-LOCK [private void CompletePrime(] = 0
SPAN-BEFORE-END-IS-LOCK-CLOSE [private void CompletePrime(] = False
SPAN-KEYWORD [private void CompletePrime(] [try] = 0
SPAN-KEYWORD [private void CompletePrime(] [finally] = 0
SPAN-LINE [private void CompletePrime(] [_logError(BuildPrimeFailedMessage(engineName), failure);] = 380
SPAN-LINE [private void CompletePrime(] [_primeTasks.TryRemove(engineName, out _);] = 381
(every other SPAN-LINE row for CompletePrime printed 0)
JOINED [The returned continuation task always completes successfully] = 0
```

## Details: added lines

```
ADDED-LINE-COUNT: 34
REMOVED-LINE-COUNT: 12
ADDED-CATCH-LINES: 0
ADDED-TOKEN [lock (] = 0
ADDED-TOKEN [lock(] = 0
ADDED-TOKEN [Monitor] = 0
ADDED-TOKEN [SemaphoreSlim] = 0
ADDED-TOKEN [Mutex] = 0
ADDED-TOKEN [ReaderWriterLockSlim] = 0
ADDED-TOKEN [ExecuteSynchronously] = 0
REMOVED:         /// Serializes the at-most-one-prime decision. Held only across a dictionary probe and a
REMOVED:         /// task start; no await occurs inside it.
REMOVED:         /// The in-flight — or most recently completed — prime per engine key. Its presence is the
REMOVED:         /// at-most-one-prime guard; its value is the test-observable handle returned by
REMOVED:         /// <see cref="GetPrimeTask"/>.
REMOVED:                 _primeTasks[engineName] = StartObservedPrime(engines, engineName, controlId);
REMOVED:         /// observed, so no unobserved task remains. The returned continuation task always
REMOVED:         /// completes successfully, which is what makes it safe for a test to await.
REMOVED:         private Task StartObservedPrime(
REMOVED:             string controlId
REMOVED:             return ApplyPrimeAsync(engines, engineName, controlId)
REMOVED:                     completed => CompletePrime(completed, engineName),
34	12	TaskMaster/Ribbon/EngineToggleStateCoordinator.cs
```

## POST-FORMAT:

P3-T2, pass 1, Timestamp: 2026-09-30T13-44. Commands: CMD-TOKEN-COUNT (TOKENS-PROD), CMD-PRIME-SPANS, CMD-PHRASE-COUNT, the P2-T6 added-lines payload and git diff --numstat ANCHOR-SHA -- TaskMaster/Ribbon/EngineToggleStateCoordinator.cs, re-run on the tree after the P3-T1 repository-wide format (production hash B3C6FEB2A86E36E95AC34F6108D87C8E117A94949F6FCE0B3AF26D824D6E3086, unchanged by P3-T1 and equal to the committed text of edc5c3af2). EXIT_CODE: 0.

Output Summary: every P2-T6 clause (tokens, spans, added lines, documentation) holds on the post-format tree. The same substitutions as the top section apply (concatenated output strings; the added-lines payload run in its own invocation; DELETED labels transcribed as REMOVED; token count and FIRST-LINE printed on one row per token; span rows printed on one line per signature).

Tokens (count, FIRST-LINE):

```
TOKEN [Serializes the at-most-one-prime decision.] = 1 FIRST-LINE=59
TOKEN [marker registration, and the start of the prime] = 1 FIRST-LINE=60
TOKEN [task start; no await occurs inside it.] = 0 FIRST-LINE=0
TOKEN [The registration marker per engine key: registered before the prime starts] = 1 FIRST-LINE=73
TOKEN [prime per engine key. Its presence is the] = 0 FIRST-LINE=0
TOKEN [private void StartPrimeIfNeeded(] = 1 FIRST-LINE=264
TOKEN [lock (_primeGate)] = 1 FIRST-LINE=272
TOKEN [if (_primeTasks.ContainsKey(engineName))] = 1 FIRST-LINE=274
TOKEN [Registration precedes the start (issue #944)] = 1 FIRST-LINE=279
TOKEN [var marker = new TaskCompletionSource<bool>(] = 1 FIRST-LINE=283
TOKEN [TaskCreationOptions.RunContinuationsAsynchronously] = 1 FIRST-LINE=284
TOKEN [_primeTasks[engineName] = marker.Task;] = 1 FIRST-LINE=286
TOKEN [StartObservedPrime(engines, engineName, controlId, marker);] = 1 FIRST-LINE=287
TOKEN [_primeTasks[engineName] = StartObservedPrime(] = 0 FIRST-LINE=0
TOKEN [private void StartObservedPrime(] = 1 FIRST-LINE=303
TOKEN [private Task StartObservedPrime(] = 0 FIRST-LINE=0
TOKEN [TaskCompletionSource<bool> marker] = 1 FIRST-LINE=307
TOKEN [_ = ApplyPrimeAsync(engines, engineName, controlId)] = 1 FIRST-LINE=310
TOKEN [return ApplyPrimeAsync(] = 0 FIRST-LINE=0
TOKEN [completed => CompletePrime(completed, engineName),] = 0 FIRST-LINE=0
TOKEN [CompletePrime(completed, engineName);] = 1 FIRST-LINE=316
TOKEN [marker.SetResult(true);] = 1 FIRST-LINE=320
TOKEN [SetResult(] = 1 FIRST-LINE=320
TOKEN [SetException(] = 0 FIRST-LINE=0
TOKEN [SetCanceled(] = 0 FIRST-LINE=0
TOKEN [TrySet] = 0 FIRST-LINE=0
TOKEN [CancellationToken.None,] = 1 FIRST-LINE=323
TOKEN [TaskContinuationOptions.None,] = 1 FIRST-LINE=324
TOKEN [TaskScheduler.Default] = 1 FIRST-LINE=325
TOKEN [ExecuteSynchronously] = 0 FIRST-LINE=0
TOKEN [The continuation task itself is discarded;] = 1 FIRST-LINE=298
TOKEN [the value a test awaits is the marker] = 1 FIRST-LINE=299
TOKEN [The returned continuation task always] = 0 FIRST-LINE=0
TOKEN [catch (] = 1 FIRST-LINE=182
TOKEN [lock (] = 1 FIRST-LINE=272
TOKEN [_primeTasks[] = 1 FIRST-LINE=286
TOKEN [_primeTasks.TryRemove(engineName, out _);] = 1 FIRST-LINE=381
TOKEN [_primeTasks.TryAdd(] = 0 FIRST-LINE=0
TOKEN [_primeTasks.AddOrUpdate(] = 0 FIRST-LINE=0
TOKEN [_primeTasks.GetOrAdd(] = 0 FIRST-LINE=0
TOKEN [_primeTasks.Clear(] = 0 FIRST-LINE=0
TOKEN [Monitor.] = 0 FIRST-LINE=0
TOKEN [SemaphoreSlim] = 0 FIRST-LINE=0
TOKEN [Mutex] = 0 FIRST-LINE=0
TOKEN [ReaderWriterLockSlim] = 0 FIRST-LINE=0
```

Spans (SPAN-LINES in CMD-PRIME-SPANS token order: lock, ContainsKey, Registration comment, marker, RunContinuationsAsynchronously, store, call, discard ApplyPrimeAsync, CompletePrime call, SetResult, CancellationToken.None, TaskContinuationOptions.None, TaskScheduler.Default, _logError, TryRemove):

```
SPAN [private void StartPrimeIfNeeded(] = 264-289 TRY=0 FINALLY=0 CATCH=0 LOCK=1 BEFORE-END-IS-LOCK-CLOSE=True KEYWORD try/finally=0/0 SPAN-LINES=272,274,279,283,284,286,287,0,0,0,0,0,0,0,0
SPAN [private void StartObservedPrime(] = 303-327 TRY=1 FINALLY=1 CATCH=0 LOCK=0 BEFORE-END-IS-LOCK-CLOSE=False KEYWORD try/finally=314/318 SPAN-LINES=0,0,0,0,0,0,0,310,316,320,323,324,325,0,0
SPAN [private void CompletePrime(] = 366-382 TRY=0 FINALLY=0 CATCH=0 LOCK=0 BEFORE-END-IS-LOCK-CLOSE=False KEYWORD try/finally=0/0 SPAN-LINES=0,0,0,0,0,0,0,0,0,0,0,0,0,380,381
JOINED [The returned continuation task always completes successfully] = 0
```

Span clauses: StartPrimeIfNeeded 272 < 274 < 279 < 283; 284 = 283 + 1; 286 > 284; 287 = 286 + 1 and 287 < 289 - 1; lock close before end True. StartObservedPrime 310 < 314 < 316 < 318 < 320 < 323 < 324 < 325. CompletePrime 380 < 381 with TRY, CATCH and LOCK 0. All hold.

Added lines:

```
ADDED-LINE-COUNT: 34
REMOVED-LINE-COUNT: 12
ADDED-CATCH-LINES: 0
ADDED-TOKEN [lock (] = 0
ADDED-TOKEN [lock(] = 0
ADDED-TOKEN [Monitor] = 0
ADDED-TOKEN [SemaphoreSlim] = 0
ADDED-TOKEN [Mutex] = 0
ADDED-TOKEN [ReaderWriterLockSlim] = 0
ADDED-TOKEN [ExecuteSynchronously] = 0
REMOVED: (the same twelve lines as the top section, in the same order)
34	12	TaskMaster/Ribbon/EngineToggleStateCoordinator.cs
```
