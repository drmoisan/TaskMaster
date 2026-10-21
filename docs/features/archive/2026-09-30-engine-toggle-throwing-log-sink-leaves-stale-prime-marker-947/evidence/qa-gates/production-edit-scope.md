# QA Gate: Fixed Shape and Documentation Tokens of the Production File (P1-T9)

Timestamp: 2026-10-01T17-54
Task: P1-T9
Command: CMD-TOKEN-COUNT (FILE TaskMaster\Ribbon\EngineToggleStateCoordinator.cs, TOKEN TOKENS-PROD-947); CMD-SPANS
EXIT_CODE: 0

Output Summary:
- Tokens: every count equals the Delivered Source paragraph `Expected post-fix file-level counts`: `catch (` 3; `catch (Exception)` 2; `Intentionally discarded: see the remarks on this method.`, `RibbonCommandBoundary.SafeLog`, `returned or thrown`, `no remaining throw source of its own` 2 each; `catch (Exception ex)`, `lock (`, both `_logError(` statements and the nine documentation tokens 1 each; the four removed base tokens 0 each.
- CompletePrime span 391-416: SPAN-TRY 1, SPAN-CATCH 1, SPAN-FINALLY 0, SPAN-LOCK 0; order Report-then-clear 402 < try 406 < _logError(Prime) 408 < catch (Exception) 410 < Intentionally discarded 412 < _primeTasks.TryRemove 415 (report precedes clear; the clear sits after the guard, outside it).
- HandleToggleClickAsync span 173-196: SPAN-TRY 2, SPAN-CATCH 2, SPAN-FINALLY 0, SPAN-LOCK 0; order try 181 < catch (Exception ex) 185 < _logError(Toggle) 189 < catch (Exception) 191 < Intentionally discarded 193; _logError(Prime) 0 in this span.
- StartObservedPrime span 316-340: SPAN-TRY 1, SPAN-FINALLY 1, SPAN-CATCH 0 (unchanged from P0-T3).
- No FORMATTER SPLITS GATED TOKEN; no repair was needed.
- Result: P1-T9 acceptance holds.

## TOKEN lines

```
TOKEN [catch (] = 3
TOKEN [catch (Exception ex)] = 1
TOKEN [catch (Exception)] = 2
TOKEN [lock (] = 1
TOKEN [Intentionally discarded: see the remarks on this method.] = 2
TOKEN [RibbonCommandBoundary.SafeLog] = 2
TOKEN [returned or thrown] = 2
TOKEN [no remaining throw source of its own] = 2
TOKEN [_logError(BuildToggleFailedMessage(engineName), ex);] = 1
TOKEN [_logError(BuildPrimeFailedMessage(engineName), failure);] = 1
TOKEN [the only <c>catch</c> clause in this type that observes an] = 1
TOKEN [The other two are sink guards, here and in] = 1
TOKEN [the click boundary and the two sink guards.] = 1
TOKEN [can rely on the report having] = 1
TOKEN [A failure thrown by the sink itself is contained here] = 1
TOKEN [The sink call is guarded (issue #947).] = 1
TOKEN [The sink call is itself guarded (issue #947)] = 1
TOKEN [This method therefore never throws, even when the] = 1
TOKEN [is guaranteed the report has already been] = 1
TOKEN [exactly one <c>catch</c>] = 0
TOKEN [the only place in this type that observes a fault with a] = 0
TOKEN [can rely on the fault having been reported.] = 0
TOKEN [This method never throws, because its caller is an] = 0
```

## FIRST-LINE lines (observations)

```
FIRST-LINE [catch (] = 185
FIRST-LINE [catch (Exception ex)] = 185
FIRST-LINE [catch (Exception)] = 191
FIRST-LINE [lock (] = 283
FIRST-LINE [Intentionally discarded: see the remarks on this method.] = 193
FIRST-LINE [RibbonCommandBoundary.SafeLog] = 169
FIRST-LINE [returned or thrown] = 257
FIRST-LINE [no remaining throw source of its own] = 314
FIRST-LINE [_logError(BuildToggleFailedMessage(engineName), ex);] = 189
FIRST-LINE [_logError(BuildPrimeFailedMessage(engineName), failure);] = 408
FIRST-LINE [the only <c>catch</c> clause in this type that observes an] = 154
FIRST-LINE [The other two are sink guards, here and in] = 155
FIRST-LINE [the click boundary and the two sink guards.] = 308
FIRST-LINE [can rely on the report having] = 258
FIRST-LINE [A failure thrown by the sink itself is contained here] = 368
FIRST-LINE [The sink call is guarded (issue #947).] = 382
FIRST-LINE [The sink call is itself guarded (issue #947)] = 167
FIRST-LINE [This method therefore never throws, even when the] = 169
FIRST-LINE [is guaranteed the report has already been] = 404
FIRST-LINE [exactly one <c>catch</c>] = 0
FIRST-LINE [the only place in this type that observes a fault with a] = 0
FIRST-LINE [can rely on the fault having been reported.] = 0
FIRST-LINE [This method never throws, because its caller is an] = 0
```

## CMD-SPANS output

```
SPAN [internal async Task HandleToggleClickAsync(] = 173-196
SPAN-TRY [internal async Task HandleToggleClickAsync(] = 2
SPAN-FINALLY [internal async Task HandleToggleClickAsync(] = 0
SPAN-CATCH [internal async Task HandleToggleClickAsync(] = 2
SPAN-LOCK [internal async Task HandleToggleClickAsync(] = 0
SPAN-KEYWORD [internal async Task HandleToggleClickAsync(] [try] = 181
SPAN-KEYWORD [internal async Task HandleToggleClickAsync(] [finally] = 0
SPAN-LINE [internal async Task HandleToggleClickAsync(] [_logError(BuildToggleFailedMessage(engineName), ex);] = 189
SPAN-LINE [internal async Task HandleToggleClickAsync(] [catch (Exception ex)] = 185
SPAN-LINE [internal async Task HandleToggleClickAsync(] [CompletePrime(completed, engineName);] = 0
SPAN-LINE [internal async Task HandleToggleClickAsync(] [marker.SetResult(true);] = 0
SPAN-LINE [internal async Task HandleToggleClickAsync(] [_logError(BuildPrimeFailedMessage(engineName), failure);] = 0
SPAN-LINE [internal async Task HandleToggleClickAsync(] [_primeTasks.TryRemove(engineName, out _);] = 0
SPAN-LINE [internal async Task HandleToggleClickAsync(] [catch (Exception)] = 191
SPAN-LINE [internal async Task HandleToggleClickAsync(] [Intentionally discarded: see the remarks on this method.] = 193
SPAN-LINE [internal async Task HandleToggleClickAsync(] [Report-then-clear is load-bearing] = 0
SPAN [private void StartObservedPrime(] = 316-340
SPAN-TRY [private void StartObservedPrime(] = 1
SPAN-FINALLY [private void StartObservedPrime(] = 1
SPAN-CATCH [private void StartObservedPrime(] = 0
SPAN-LOCK [private void StartObservedPrime(] = 0
SPAN-KEYWORD [private void StartObservedPrime(] [try] = 327
SPAN-KEYWORD [private void StartObservedPrime(] [finally] = 331
SPAN-LINE [private void StartObservedPrime(] [_logError(BuildToggleFailedMessage(engineName), ex);] = 0
SPAN-LINE [private void StartObservedPrime(] [catch (Exception ex)] = 0
SPAN-LINE [private void StartObservedPrime(] [CompletePrime(completed, engineName);] = 329
SPAN-LINE [private void StartObservedPrime(] [marker.SetResult(true);] = 333
SPAN-LINE [private void StartObservedPrime(] [_logError(BuildPrimeFailedMessage(engineName), failure);] = 0
SPAN-LINE [private void StartObservedPrime(] [_primeTasks.TryRemove(engineName, out _);] = 0
SPAN-LINE [private void StartObservedPrime(] [catch (Exception)] = 0
SPAN-LINE [private void StartObservedPrime(] [Intentionally discarded: see the remarks on this method.] = 0
SPAN-LINE [private void StartObservedPrime(] [Report-then-clear is load-bearing] = 0
SPAN [private void CompletePrime(] = 391-416
SPAN-TRY [private void CompletePrime(] = 1
SPAN-FINALLY [private void CompletePrime(] = 0
SPAN-CATCH [private void CompletePrime(] = 1
SPAN-LOCK [private void CompletePrime(] = 0
SPAN-KEYWORD [private void CompletePrime(] [try] = 406
SPAN-KEYWORD [private void CompletePrime(] [finally] = 0
SPAN-LINE [private void CompletePrime(] [_logError(BuildToggleFailedMessage(engineName), ex);] = 0
SPAN-LINE [private void CompletePrime(] [catch (Exception ex)] = 0
SPAN-LINE [private void CompletePrime(] [CompletePrime(completed, engineName);] = 0
SPAN-LINE [private void CompletePrime(] [marker.SetResult(true);] = 0
SPAN-LINE [private void CompletePrime(] [_logError(BuildPrimeFailedMessage(engineName), failure);] = 408
SPAN-LINE [private void CompletePrime(] [_primeTasks.TryRemove(engineName, out _);] = 415
SPAN-LINE [private void CompletePrime(] [catch (Exception)] = 410
SPAN-LINE [private void CompletePrime(] [Intentionally discarded: see the remarks on this method.] = 412
SPAN-LINE [private void CompletePrime(] [Report-then-clear is load-bearing] = 402
```

## POST-FORMAT:

Timestamp: 2026-10-01T18-03
Task: P2-T2 (P1-T9 commands re-run unchanged on the tree after the P2-T1 repository-wide format)
Command: CMD-TOKEN-COUNT (FILE TaskMaster\Ribbon\EngineToggleStateCoordinator.cs, TOKEN TOKENS-PROD-947); CMD-SPANS
EXIT_CODE: 0

Output Summary:
- Every TOKEN count, FIRST-LINE value and CMD-SPANS row is identical to the P1-T9 section above; every P1-T9 acceptance clause holds on the post-format tree.
- CompletePrime span 391-416: SPAN-TRY 1, SPAN-CATCH 1, SPAN-FINALLY 0, SPAN-LOCK 0; order Report-then-clear 402 < try 406 < _logError(Prime) 408 < catch (Exception) 410 < Intentionally discarded 412 < _primeTasks.TryRemove 415.
- HandleToggleClickAsync span 173-196: SPAN-TRY 2, SPAN-CATCH 2, SPAN-FINALLY 0, SPAN-LOCK 0; order try 181 < catch (Exception ex) 185 < _logError(Toggle) 189 < catch (Exception) 191 < Intentionally discarded 193; _logError(Prime) 0 in this span.
- StartObservedPrime span 316-340: SPAN-TRY 1, SPAN-FINALLY 1, SPAN-CATCH 0.
- Result: POST-FORMAT clauses hold.

TOKEN lines:

```
TOKEN [catch (] = 3
TOKEN [catch (Exception ex)] = 1
TOKEN [catch (Exception)] = 2
TOKEN [lock (] = 1
TOKEN [Intentionally discarded: see the remarks on this method.] = 2
TOKEN [RibbonCommandBoundary.SafeLog] = 2
TOKEN [returned or thrown] = 2
TOKEN [no remaining throw source of its own] = 2
TOKEN [_logError(BuildToggleFailedMessage(engineName), ex);] = 1
TOKEN [_logError(BuildPrimeFailedMessage(engineName), failure);] = 1
TOKEN [the only <c>catch</c> clause in this type that observes an] = 1
TOKEN [The other two are sink guards, here and in] = 1
TOKEN [the click boundary and the two sink guards.] = 1
TOKEN [can rely on the report having] = 1
TOKEN [A failure thrown by the sink itself is contained here] = 1
TOKEN [The sink call is guarded (issue #947).] = 1
TOKEN [The sink call is itself guarded (issue #947)] = 1
TOKEN [This method therefore never throws, even when the] = 1
TOKEN [is guaranteed the report has already been] = 1
TOKEN [exactly one <c>catch</c>] = 0
TOKEN [the only place in this type that observes a fault with a] = 0
TOKEN [can rely on the fault having been reported.] = 0
TOKEN [This method never throws, because its caller is an] = 0
```

FIRST-LINE lines:

```
FIRST-LINE [catch (] = 185
FIRST-LINE [catch (Exception ex)] = 185
FIRST-LINE [catch (Exception)] = 191
FIRST-LINE [lock (] = 283
FIRST-LINE [Intentionally discarded: see the remarks on this method.] = 193
FIRST-LINE [RibbonCommandBoundary.SafeLog] = 169
FIRST-LINE [returned or thrown] = 257
FIRST-LINE [no remaining throw source of its own] = 314
FIRST-LINE [_logError(BuildToggleFailedMessage(engineName), ex);] = 189
FIRST-LINE [_logError(BuildPrimeFailedMessage(engineName), failure);] = 408
FIRST-LINE [the only <c>catch</c> clause in this type that observes an] = 154
FIRST-LINE [The other two are sink guards, here and in] = 155
FIRST-LINE [the click boundary and the two sink guards.] = 308
FIRST-LINE [can rely on the report having] = 258
FIRST-LINE [A failure thrown by the sink itself is contained here] = 368
FIRST-LINE [The sink call is guarded (issue #947).] = 382
FIRST-LINE [The sink call is itself guarded (issue #947)] = 167
FIRST-LINE [This method therefore never throws, even when the] = 169
FIRST-LINE [is guaranteed the report has already been] = 404
FIRST-LINE [exactly one <c>catch</c>] = 0
FIRST-LINE [the only place in this type that observes a fault with a] = 0
FIRST-LINE [can rely on the fault having been reported.] = 0
FIRST-LINE [This method never throws, because its caller is an] = 0
```

CMD-SPANS output:

```
SPAN [internal async Task HandleToggleClickAsync(] = 173-196
SPAN-TRY [internal async Task HandleToggleClickAsync(] = 2
SPAN-FINALLY [internal async Task HandleToggleClickAsync(] = 0
SPAN-CATCH [internal async Task HandleToggleClickAsync(] = 2
SPAN-LOCK [internal async Task HandleToggleClickAsync(] = 0
SPAN-KEYWORD [internal async Task HandleToggleClickAsync(] [try] = 181
SPAN-KEYWORD [internal async Task HandleToggleClickAsync(] [finally] = 0
SPAN-LINE [internal async Task HandleToggleClickAsync(] [_logError(BuildToggleFailedMessage(engineName), ex);] = 189
SPAN-LINE [internal async Task HandleToggleClickAsync(] [catch (Exception ex)] = 185
SPAN-LINE [internal async Task HandleToggleClickAsync(] [CompletePrime(completed, engineName);] = 0
SPAN-LINE [internal async Task HandleToggleClickAsync(] [marker.SetResult(true);] = 0
SPAN-LINE [internal async Task HandleToggleClickAsync(] [_logError(BuildPrimeFailedMessage(engineName), failure);] = 0
SPAN-LINE [internal async Task HandleToggleClickAsync(] [_primeTasks.TryRemove(engineName, out _);] = 0
SPAN-LINE [internal async Task HandleToggleClickAsync(] [catch (Exception)] = 191
SPAN-LINE [internal async Task HandleToggleClickAsync(] [Intentionally discarded: see the remarks on this method.] = 193
SPAN-LINE [internal async Task HandleToggleClickAsync(] [Report-then-clear is load-bearing] = 0
SPAN [private void StartObservedPrime(] = 316-340
SPAN-TRY [private void StartObservedPrime(] = 1
SPAN-FINALLY [private void StartObservedPrime(] = 1
SPAN-CATCH [private void StartObservedPrime(] = 0
SPAN-LOCK [private void StartObservedPrime(] = 0
SPAN-KEYWORD [private void StartObservedPrime(] [try] = 327
SPAN-KEYWORD [private void StartObservedPrime(] [finally] = 331
SPAN-LINE [private void StartObservedPrime(] [_logError(BuildToggleFailedMessage(engineName), ex);] = 0
SPAN-LINE [private void StartObservedPrime(] [catch (Exception ex)] = 0
SPAN-LINE [private void StartObservedPrime(] [CompletePrime(completed, engineName);] = 329
SPAN-LINE [private void StartObservedPrime(] [marker.SetResult(true);] = 333
SPAN-LINE [private void StartObservedPrime(] [_logError(BuildPrimeFailedMessage(engineName), failure);] = 0
SPAN-LINE [private void StartObservedPrime(] [_primeTasks.TryRemove(engineName, out _);] = 0
SPAN-LINE [private void StartObservedPrime(] [catch (Exception)] = 0
SPAN-LINE [private void StartObservedPrime(] [Intentionally discarded: see the remarks on this method.] = 0
SPAN-LINE [private void StartObservedPrime(] [Report-then-clear is load-bearing] = 0
SPAN [private void CompletePrime(] = 391-416
SPAN-TRY [private void CompletePrime(] = 1
SPAN-FINALLY [private void CompletePrime(] = 0
SPAN-CATCH [private void CompletePrime(] = 1
SPAN-LOCK [private void CompletePrime(] = 0
SPAN-KEYWORD [private void CompletePrime(] [try] = 406
SPAN-KEYWORD [private void CompletePrime(] [finally] = 0
SPAN-LINE [private void CompletePrime(] [_logError(BuildToggleFailedMessage(engineName), ex);] = 0
SPAN-LINE [private void CompletePrime(] [catch (Exception ex)] = 0
SPAN-LINE [private void CompletePrime(] [CompletePrime(completed, engineName);] = 0
SPAN-LINE [private void CompletePrime(] [marker.SetResult(true);] = 0
SPAN-LINE [private void CompletePrime(] [_logError(BuildPrimeFailedMessage(engineName), failure);] = 408
SPAN-LINE [private void CompletePrime(] [_primeTasks.TryRemove(engineName, out _);] = 415
SPAN-LINE [private void CompletePrime(] [catch (Exception)] = 410
SPAN-LINE [private void CompletePrime(] [Intentionally discarded: see the remarks on this method.] = 412
SPAN-LINE [private void CompletePrime(] [Report-then-clear is load-bearing] = 402
```
