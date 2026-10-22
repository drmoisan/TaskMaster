# [P1-T6] [expect-fail] — Disposed-Control Self-Detach Test, Compile-Red State

Timestamp: 2026-08-27T20-17

Command:
```
& $msbuild TaskMaster.sln /t:Build /m /p:Configuration=Debug "/p:Platform=Any CPU"
```

EXIT_CODE: 1
ExpectedExitCode: 1

## Output Summary

- Build summary: `6 Error(s)`, `5 Warning(s)`. The build is red.
- `[TestMethod] public async Task ControlDisposed_DetachesTheHost()` exists in
  `QuickFiler.Test/Viewers/WebView2BreadcrumbHostTests.cs` at line 224.

### Complete distinct compiler error list

All six errors are in `QuickFiler.Test/Viewers/WebView2BreadcrumbHostTests.cs`. Message texts are
identical to those recorded in `p1-t5-null-core-tolerance-red.2026-08-27T20-16.md`; only the
locations differ, so the messages are not repeated at length here.

| Location | Error | Missing member named |
| --- | --- | --- |
| `(45,33)` | `CS1729` | 3-argument constructor |
| `(95,33)` | `CS1729` | 3-argument constructor |
| `(157,26)` | `CS1061` | `IsAttached` |
| `(162,26)` | `CS1061` | `IsAttached` |
| `(206,26)` | `CS1061` | `IsAttached` |
| `(241,22)` | `CS1061` | `IsAttached` |

`CS1729`: `'WebView2BreadcrumbHost' does not contain a constructor that takes 3 arguments`.
`CS1061`: `'WebView2BreadcrumbHost' does not contain a definition for 'IsAttached' and no accessible
extension method 'IsAttached' accepting a first argument of type 'WebView2BreadcrumbHost' could be
found (are you missing a using directive or an assembly reference?)`.

Line 241 is the `IsAttached` reference this task added.

### Test design points

- A distinct `WebView2` control is constructed on `WinFormsPumpHost` for this test, so the
  process-wide owner registry cannot couple it to any other test.
- `control.Dispose()` is invoked on the pump thread that owns the control, which is where WinForms
  disposal must happen.
- The control is deliberately not disposed a second time in a `finally` block, because disposing it
  is the act under test.
