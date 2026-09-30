# Production reorder scope (issue 942)

Timestamp: 2026-09-30T07-41
Task: P2-T6 (creates this file); P3-T2 appends POST-FORMAT.
Command: CMD-TOKEN-COUNT with FILE TaskMaster\Ribbon\EngineToggleStateCoordinator.cs and the eleven-token list of P2-T6; git diff -U0 231e1c0b55105aeb626bf5a6e8d0266a567cacad -- TaskMaster/Ribbon/EngineToggleStateCoordinator.cs; git diff --numstat 231e1c0b55105aeb626bf5a6e8d0266a567cacad -- TaskMaster/Ribbon/EngineToggleStateCoordinator.cs; the P2-T6 span measurement (adapted as noted below)
EXIT_CODE: 0

Output Summary:

Token counts (file level):

- TOKEN [private void CompletePrime(] = 1
- TOKEN [_logError(BuildPrimeFailedMessage(engineName), failure);] = 1
- TOKEN [_primeTasks.TryRemove(engineName, out _);] = 1
- TOKEN [Report-then-clear is load-bearing] = 1
- TOKEN [and only then is the in-flight marker cleared] = 1
- TOKEN [cleared only after that report has returned] = 1
- TOKEN [internal Task GetPrimeTask(] = 1
- TOKEN [catch (] = 1
- TOKEN [lock (] = 1
- TOKEN [new TaskCanceledException(completed)] = 1
- TOKEN [GetBaseException()] = 1
- FIRST-LINE [private void CompletePrime(] = 344
- FIRST-LINE [_logError(BuildPrimeFailedMessage(engineName), failure);] = 358
- FIRST-LINE [_primeTasks.TryRemove(engineName, out _);] = 359
- FIRST-LINE [Report-then-clear is load-bearing] = 355
- FIRST-LINE [and only then is the in-flight marker cleared] = 332
- FIRST-LINE [cleared only after that report has returned] = 246
- FIRST-LINE [internal Task GetPrimeTask(] = 249
- FIRST-LINE [catch (] = 181
- FIRST-LINE [lock (] = 271
- FIRST-LINE [new TaskCanceledException(completed)] = 353
- FIRST-LINE [GetBaseException()] = 352

Span measurement:

- SPAN=344-360
- SPAN_RETURN=1
- SPAN_RANTOCOMPLETION=1
- SPAN_TRY=0
- SPAN_CATCH=0
- SPAN_LOCK=0
- Adaptation (recorded, not silent): the plan's verbatim span payload exits 1 with no output from both Bash and a pwsh host, because the nested literal `"lock ("` inside a `"$( ... )"` subexpression carries an unbalanced parenthesis that the expandable-string scanner cannot parse. Probes: the same subexpression with `"lock"` prints a count; with `"lock ("` it exits 1; the `"return;"` and `"\btry\b"` forms parse. The literal was built outside the subexpression as `$lk = "lock" + " ("` (length probe PROBE_LK_LENGTH=6) and passed as `$_.Contains($lk)`, which is the identical predicate. All other span statements were run verbatim.

Anchored diff (-U0) hunk headers and hunk-window rule:

- G = FIRST-LINE of `internal Task GetPrimeTask(` = 249; window [G-6, G-1] = [243, 248]
- S = FIRST-LINE of `and only then is the in-flight marker cleared` = 332; R = FIRST-LINE of `_primeTasks.TryRemove(engineName, out _);` = 359; window [S-2, R] = [330, 359]
- `@@ -245 +245,3 @@` new-side span 245-247: inside [243, 248]
- `@@ -329,2 +331,3 @@` new-side span 331-333: inside [330, 359]
- `@@ -348,2 +350,0 @@` new-side single line 350 (count 0): inside [330, 359]
- `@@ -353,0 +355,3 @@` new-side span 355-357: inside [330, 359]
- `@@ -354,0 +359 @@` new-side span 359-359 (omitted count is 1): inside [330, 359]
- Every hunk lies wholly within one of the two windows; no HUNK OUTSIDE EDIT WINDOWS.

Numstat: `10	5	TaskMaster/Ribbon/EngineToggleStateCoordinator.cs` (deletions 5, at most 8).

Removed lines, quoted and classified:

1. `        /// itself and reported through <c>logError</c>.` — a line of the GetPrimeTask returns element.
2. `        /// is left unset — so the key still reports unchecked — the in-flight marker is cleared so` — a line of the CompletePrime summary element.
3. `        /// a later read may re-prime, and the failure is reported through <c>logError</c>.` — a line of the CompletePrime summary element.
4. `            _primeTasks.TryRemove(engineName, out _);` — the TryRemove statement.
5. (empty line) — the blank line adjacent to the TryRemove statement.

No line of the early return, the failure computation or the synthesized exception was removed or rewritten.

Clause check (P2-T1, P2-T2, P2-T6 production acceptance):

- `_logError(` line 358 is less than the `TryRemove` line 359; both are greater than the CompletePrime line 344.
- `Report-then-clear is load-bearing` line 355 equals the `_logError(` line minus 3.
- `catch (` = 1 and `lock (` = 1 (unchanged from the anchor, fact 1).
- The three documentation tokens count 1 each: the summary token (332) lies within the twelve lines above 344; the returns token (246) lies within the eight lines above 249. The `remarks` element is unchanged (no hunk touches it).
- `new TaskCanceledException(completed)` = 1 and `GetBaseException()` = 1.
- SPAN start 344 equals FIRST-LINE of `private void CompletePrime(`.
- All clauses hold.
