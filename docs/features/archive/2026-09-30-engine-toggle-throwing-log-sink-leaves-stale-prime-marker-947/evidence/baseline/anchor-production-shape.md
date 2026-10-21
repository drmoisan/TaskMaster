# Anchor: Production File Base Shape (P0-T3)

Timestamp: 2026-10-01T17-37
Task: P0-T3
Command: CMD-TOKEN-COUNT (FILE TaskMaster\Ribbon\EngineToggleStateCoordinator.cs, 14 tokens); CMD-SPANS; CMD-WINDOWS (BASE-SHA 2e6ce2cabe7136bbbc8897fcb39d3e13d654ff85)
EXIT_CODE: 0

Output Summary:
- Tokens: all fourteen counts match the plan (catch ( 1; catch (Exception ex) 1; catch (Exception) 0; lock ( 1; exactly one <c>catch</c> 1; the three removed-base doc tokens 1 each; Intentionally discarded 0; RibbonCommandBoundary.SafeLog 0; last four tokens 1 each).
- Spans: HandleToggleClickAsync TRY 1, CATCH 1, catch (Exception) line 0; StartObservedPrime TRY 1, FINALLY 1, CATCH 0; CompletePrime TRY 0, CATCH 0, report line 380 < clear line 381.
- COMPLETEPRIME-SHAPE: REPORT-THEN-CLEAR
- Windows: every start and end non-zero; three WINDOW- flags True; width relations E1 +1, E2 +2, E3 +6, E4 31, E5 +1, E6 +3 all hold; HUNK-COUNT: 0.
- Result: no ANCHOR SHAPE MISMATCH.

## Token counts (CMD-TOKEN-COUNT)

```
TOKEN [catch (] = 1
TOKEN [catch (Exception ex)] = 1
TOKEN [catch (Exception)] = 0
TOKEN [lock (] = 1
TOKEN [exactly one <c>catch</c>] = 1
TOKEN [the only place in this type that observes a fault with a] = 1
TOKEN [can rely on the fault having been reported.] = 1
TOKEN [This method never throws, because its caller is an] = 1
TOKEN [Intentionally discarded] = 0
TOKEN [RibbonCommandBoundary.SafeLog] = 0
TOKEN [_logError(BuildToggleFailedMessage(engineName), ex);] = 1
TOKEN [_logError(BuildPrimeFailedMessage(engineName), failure);] = 1
TOKEN [_primeTasks.TryRemove(engineName, out _);] = 1
TOKEN [Report-then-clear is load-bearing] = 1
FIRST-LINE [catch (] = 182
FIRST-LINE [catch (Exception ex)] = 182
FIRST-LINE [catch (Exception)] = 0
FIRST-LINE [lock (] = 272
FIRST-LINE [exactly one <c>catch</c>] = 296
FIRST-LINE [the only place in this type that observes a fault with a] = 154
FIRST-LINE [can rely on the fault having been reported.] = 248
FIRST-LINE [This method never throws, because its caller is an] = 167
FIRST-LINE [Intentionally discarded] = 0
FIRST-LINE [RibbonCommandBoundary.SafeLog] = 0
FIRST-LINE [_logError(BuildToggleFailedMessage(engineName), ex);] = 184
FIRST-LINE [_logError(BuildPrimeFailedMessage(engineName), failure);] = 380
FIRST-LINE [_primeTasks.TryRemove(engineName, out _);] = 381
FIRST-LINE [Report-then-clear is load-bearing] = 377
```

## Spans (CMD-SPANS)

```
SPAN [internal async Task HandleToggleClickAsync(] = 170-186
SPAN-TRY [internal async Task HandleToggleClickAsync(] = 1
SPAN-FINALLY [internal async Task HandleToggleClickAsync(] = 0
SPAN-CATCH [internal async Task HandleToggleClickAsync(] = 1
SPAN-LOCK [internal async Task HandleToggleClickAsync(] = 0
SPAN-KEYWORD [internal async Task HandleToggleClickAsync(] [try] = 178
SPAN-KEYWORD [internal async Task HandleToggleClickAsync(] [finally] = 0
SPAN-LINE [internal async Task HandleToggleClickAsync(] [_logError(BuildToggleFailedMessage(engineName), ex);] = 184
SPAN-LINE [internal async Task HandleToggleClickAsync(] [catch (Exception ex)] = 182
SPAN-LINE [internal async Task HandleToggleClickAsync(] [CompletePrime(completed, engineName);] = 0
SPAN-LINE [internal async Task HandleToggleClickAsync(] [marker.SetResult(true);] = 0
SPAN-LINE [internal async Task HandleToggleClickAsync(] [_logError(BuildPrimeFailedMessage(engineName), failure);] = 0
SPAN-LINE [internal async Task HandleToggleClickAsync(] [_primeTasks.TryRemove(engineName, out _);] = 0
SPAN-LINE [internal async Task HandleToggleClickAsync(] [catch (Exception)] = 0
SPAN-LINE [internal async Task HandleToggleClickAsync(] [Intentionally discarded: see the remarks on this method.] = 0
SPAN-LINE [internal async Task HandleToggleClickAsync(] [Report-then-clear is load-bearing] = 0
SPAN [private void StartObservedPrime(] = 303-327
SPAN-TRY [private void StartObservedPrime(] = 1
SPAN-FINALLY [private void StartObservedPrime(] = 1
SPAN-CATCH [private void StartObservedPrime(] = 0
SPAN-LOCK [private void StartObservedPrime(] = 0
SPAN-KEYWORD [private void StartObservedPrime(] [try] = 314
SPAN-KEYWORD [private void StartObservedPrime(] [finally] = 318
SPAN-LINE [private void StartObservedPrime(] [_logError(BuildToggleFailedMessage(engineName), ex);] = 0
SPAN-LINE [private void StartObservedPrime(] [catch (Exception ex)] = 0
SPAN-LINE [private void StartObservedPrime(] [CompletePrime(completed, engineName);] = 316
SPAN-LINE [private void StartObservedPrime(] [marker.SetResult(true);] = 320
SPAN-LINE [private void StartObservedPrime(] [_logError(BuildPrimeFailedMessage(engineName), failure);] = 0
SPAN-LINE [private void StartObservedPrime(] [_primeTasks.TryRemove(engineName, out _);] = 0
SPAN-LINE [private void StartObservedPrime(] [catch (Exception)] = 0
SPAN-LINE [private void StartObservedPrime(] [Intentionally discarded: see the remarks on this method.] = 0
SPAN-LINE [private void StartObservedPrime(] [Report-then-clear is load-bearing] = 0
SPAN [private void CompletePrime(] = 366-382
SPAN-TRY [private void CompletePrime(] = 0
SPAN-FINALLY [private void CompletePrime(] = 0
SPAN-CATCH [private void CompletePrime(] = 0
SPAN-LOCK [private void CompletePrime(] = 0
SPAN-KEYWORD [private void CompletePrime(] [try] = 0
SPAN-KEYWORD [private void CompletePrime(] [finally] = 0
SPAN-LINE [private void CompletePrime(] [_logError(BuildToggleFailedMessage(engineName), ex);] = 0
SPAN-LINE [private void CompletePrime(] [catch (Exception ex)] = 0
SPAN-LINE [private void CompletePrime(] [CompletePrime(completed, engineName);] = 0
SPAN-LINE [private void CompletePrime(] [marker.SetResult(true);] = 0
SPAN-LINE [private void CompletePrime(] [_logError(BuildPrimeFailedMessage(engineName), failure);] = 380
SPAN-LINE [private void CompletePrime(] [_primeTasks.TryRemove(engineName, out _);] = 381
SPAN-LINE [private void CompletePrime(] [catch (Exception)] = 0
SPAN-LINE [private void CompletePrime(] [Intentionally discarded: see the remarks on this method.] = 0
SPAN-LINE [private void CompletePrime(] [Report-then-clear is load-bearing] = 377
```

COMPLETEPRIME-SHAPE: REPORT-THEN-CLEAR

Observation (not gated): the StartObservedPrime span ends at 327, the first line of exactly eight spaces and a closing brace after the signature; fact 1 describes the body as 310 to 326 (the continuation's inner closing lines), so 327 is the method's own closing brace.

## Windows (CMD-WINDOWS, base text at BASE-SHA)

```
WINDOW E1 = 154-155
WINDOW E2 = 246-248
WINDOW E3 = 295-301
WINDOW E4 = 351-382
WINDOW E5 = 167-168
WINDOW E6 = 182-185
WINDOW-E1-LINE2-IS-CATCH-CLAUSE: True
WINDOW-E4-STARTS-AT-SUMMARY: True
WINDOW-E6-LINE3-IS-TOGGLE-SINK-CALL: True
HUNK-COUNT: 0
HUNKS-OUTSIDE-WINDOWS: 0
WINDOWS-TOUCHED:
```

BASE-WINDOW-E1: 154-155
BASE-WINDOW-E2: 246-248
BASE-WINDOW-E3: 295-301
BASE-WINDOW-E4: 351-382
BASE-WINDOW-E5: 167-168
BASE-WINDOW-E6: 182-185
