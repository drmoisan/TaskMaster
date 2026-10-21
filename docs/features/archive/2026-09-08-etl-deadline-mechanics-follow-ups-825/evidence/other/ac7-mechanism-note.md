# AC7 — Exception-Injection Mechanism Note

Timestamp: 2026-09-09T16-48

SubstituteMechanismUsed: false

## The mechanism used

The regression test GetTableInViewAsync_TimeoutRetry_UsesCallerTimeoutMsNotLiteral2000 reaches the
TimeoutException retry branch by supplying a timeoutSourceFactory that throws a TimeoutException on
its first invocation and returns a never-cancelling CancellationTokenSource on every later
invocation. This is the mechanism spec.md Test Strategy names, used unchanged. No substitute was
found or applied, so SubstituteMechanismUsed is false and no substitution needs recording.

## Why the throw reaches the intended catch

The factory is invoked at UtilitiesCS/Threading/TimeOutTask.cs line 52, in the statement that
resolves `timeoutSourceFactory ?? (ms => new CancellationTokenSource(ms))` and applies it. That
statement sits outside the try opened at line 61. RunWithTimeout is an async method, so an exception
raised anywhere in its body before that try is captured into the task it returns rather than being
thrown synchronously at the call site.

The awaiting caller is UtilitiesCS/OutlookObjects/Table/OlTableExtensions.TableAccess.cs, whose try
opens at pre-change line 55 and whose await sits at pre-change line 57. The captured
TimeoutException is therefore observed at that await, inside that try, and is caught by the
`catch (TimeoutException)` opened at pre-change line 95. The recursion inside that catch invokes the
factory a second time, which is what makes the millisecond value the second attempt used observable
to the test.

## Why the mechanism is deliberate rather than incidental

The reachability note in spec.md Assumptions records that neither catch block in GetTableInViewAsync
is entered by the ordinary deadline-expiry path, because RunWithTimeout runs with `strict: false`
and `maxAttempts: 1` and absorbs the cancellation rather than propagating it. The factory-throw is
the only mechanism found in the current tree that reaches the TimeoutException branch
deterministically, and it reaches it without any wall-clock wait, retry loop or timing tolerance.
The alternative of driving the branch through a real timeout would require a wall-clock race, which
repository policy forbids in test code.

The fail-before record at evidence/regression-testing/ac7-fail-before.md confirms the mechanism
works as described: the factory recorded exactly two invocations against the pre-change file, which
can only happen if the retry branch was entered.
