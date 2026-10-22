# Fail-before exception dossier (issue #826, [P1-T2])

Timestamp: 2026-09-09T19-12

BRANCH: REACHABLE

The value above is copied from the [P1-T1] artifact
`<FEATURE>/evidence/other/item2-branch-reachability.2026-09-09T19-11.md`.

WhyFailingRunImpossible: A failing run is structurally impossible for both of the test methods [P2-T2]
authors, because AC7 itself forbids the property that would make one possible. AC7 requires that
reverting the item-2 production edit alone must not be what makes the test pass or fail, so each test
pins a control-flow branch of `GetTableInViewAsync` rather than the statement substitution inside it;
both branches exist and behave identically before and after the substitution, so a test written against
them is green on the pre-change tree and stays green on the post-change tree. Its purpose is to bring the
two changed production lines under coverage, not to detect the change.

## Alternative proof

### Observed pre-change figure

[P0-T4] observed, at base commit `dea7b49dae31a9bda8d35ecb73b8c8d646b1a460`, that the
`-SimpleMatch` count of `Console.WriteLine` in
`UtilitiesCS/OutlookObjects/Table/OlTableExtensions.TableAccess.cs` is **2**. That is the pre-change
figure and it is an observation, recorded in
`<FEATURE>/evidence/baseline/upstream-825-precondition.md`.

The corresponding post-change figure is **not** recorded here. It is not observable when this task runs,
and this plan forbids stating a command's result in advance. The task that observes and asserts it is
[P6-T1], whose acceptance requires a post-change `Console.WriteLine` count of 0 in that file, with
evidence at `<FEATURE>/evidence/qa-gates/p6-t1-ac5-ac6.md`. [P2-T1] records the same count earlier at
`<FEATURE>/evidence/qa-gates/p2-t1-item2-substitution.md`.

### Why the branch the AC7 test exercises is a property of the seam, not of the substitution

[P1-T1] step 3 cites `UtilitiesCS/Threading/TimeOutTask.cs`: the injected timeout-source factory is
invoked at lines 52 to 54, and the `try` block that carries the `catch (TaskCanceledException)` clause at
line 65 and the `catch (System.Exception e)` clause at line 85 does not open until line 61. An exception
thrown by an injected factory is therefore raised before the `try` is entered, escapes `RunWithTimeout`
carrying its own type, and reaches the corresponding `catch` clause in `GetTableInViewAsync`.

That routing is determined entirely by where the factory is invoked relative to the `try`. It is
unaffected by whether the statement inside the receiving catch body is `Console.WriteLine(...)` or
`logger.Warn(...)`, because neither statement participates in exception routing and neither alters the
`counter < 2` retry decision that follows it. A test asserting the factory invocation count, the
`GetTable` call count and the returned object therefore observes the same values on both trees.

Independent corroboration that the mechanism is live rather than merely derivable: feature 825's test
`GetTableInViewAsync_TimeoutRetry_UsesCallerTimeoutMsNotLiteral2000`
(`UtilitiesCS.Test/OutlookObjects/Table/GetTableInViewAsyncClockTests.cs` line 112) already exploits it,
and this feature's own [P0-T9] baseline coverage run measured a hit count of 2 on the changed line inside
`catch (TimeoutException)` before any edit was made. A branch that is already executing at baseline
cannot be one the fix makes reachable.

### Negative-evidence record

SearchScope: `docs/features/active/2026-09-08-console-out-aggressors-and-banned-symbol-promotion-826/evidence/regression-testing/`

SearchPatterns: `fail-before-exception.*.md`

SearchResult: `docs/features/active/2026-09-08-console-out-aggressors-and-banned-symbol-promotion-826/evidence/regression-testing/fail-before-exception.2026-09-09T19-12.md` — this file.

EXIT_CODE: 0

Output Summary: fail-before dossier recorded for the two [P2-T2] test methods. A red-then-green run is
structurally impossible because AC7 forbids the test from depending on the substitution. The alternative
proof is the observed pre-change `Console.WriteLine` count of 2, the named later task that observes the
post-change count, and the [P1-T1] step-3 citation showing the exercised reachability is a property of
the `timeoutSourceFactory` seam rather than of the substituted statement.
