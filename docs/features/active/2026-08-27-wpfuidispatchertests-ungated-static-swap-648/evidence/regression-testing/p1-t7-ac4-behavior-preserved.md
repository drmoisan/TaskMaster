# P1-T7 — AC-4 Verified by Measurement and by Diff (Behavior Preserved)

Timestamp: 2026-09-01T14-12

Command:
```
grep -n -F -e 'sut.Invoke(' -e 'sut.InvokeAsync(' -e 'sut.BeginInvoke(' -e 'beginInvokeThreadId.Should().Be(dispatcherThreadId);' QuickFiler.Test/Controllers/WpfUiDispatcherTests.cs
git diff issue-648-diff-anchor -- QuickFiler.Test/Controllers/WpfUiDispatcherTests.cs
```
`issue-648-diff-anchor` is the local tag P0-T3 created on this branch's merge base with `origin/main`,
at commit `c7b4f08f6d80296840f9a351042cb2113892e95f`. The ref `origin/main` is not used as a diff
operand anywhere in this plan because P0-T3's fetch advances it.

EXIT_CODE: 0

Output Summary:

## Part 1 — the three assertion regions are preserved

All four tokens match:

| Token | Matching line |
|---|---|
| `sut.Invoke(` | `:69` |
| `sut.InvokeAsync(` | `:74` |
| `sut.BeginInvoke(` | `:84` |
| `beginInvokeThreadId.Should().Be(dispatcherThreadId);` | `:91` |

The `Invoke` and `InvokeAsync` thread-identity assertions are at `:70` and `:85` respectively
(`invokeThreadId.Should().Be(dispatcherThreadId);` and
`invokeAsyncThreadId.Should().Be(dispatcherThreadId);`), and the diff shows both as context lines
re-indented by four columns rather than as changed assertions.

## Part 2 — `Construction_YieldsAnIUiDispatcher` body is unchanged

The diff hunk covering that method is:

```
@@ -20,6 +18,8 @@ namespace QuickFiler.Controllers.Tests
     [TestClass]
     public class WpfUiDispatcherTests
     {
+        private const int GateTimeoutMs = 60000;
+
         [TestMethod]
         public void Construction_YieldsAnIUiDispatcher()
         {
```

The region the acceptance condition names runs from the `[TestMethod]` attribute of
`Construction_YieldsAnIUiDispatcher` to that method's closing brace. Within that region the diff
carries **no added line and no removed line**. The only two added lines in this hunk, the
`GateTimeoutMs` field and the blank line after it, sit above the `[TestMethod]` attribute and are
therefore outside the region.

The three lines shown inside the region — `[TestMethod]`, the method signature, and the opening brace
— are unchanged context lines printed because `git diff` prints three lines of context around each
change. Unchanged context lines inside the region do not count, which is the distinction the task
states. The method's four body lines do not appear in the diff at all, so no hunk touches them.

The field placement follows the sibling precedent at
`QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixtureTests.cs:33`, which places
`private const int GateTimeoutMs = 60000;` immediately after the class opening brace.

## Part 3 — full diff shape

The diff carries three hunks and touches exactly one file. The removals are the two `using`
directives (`using System.Reflection;`, `using UtilitiesCS;`), the six-line reflection block, and the
unconditional `field.SetValue(null, original);` restore. The additions are the `GateTimeoutMs` field,
the `[Timeout(GateTimeoutMs)]` attribute, the `async Task` signature, the issue #648 paragraph in the
method's doc comment, the two-statement gate acquisition, the `transaction.Install(dispatcher)` call,
the nested `try`/`finally` with `transaction.Dispose();` in the inner `finally`, and the
re-indentation of the preserved assertion body.

AC-4 holds: the test still asserts that `Invoke`, `InvokeAsync`, and `BeginInvoke` each execute their
delegate on the dispatcher's own thread, and the body of `Construction_YieldsAnIUiDispatcher` is
unchanged.
