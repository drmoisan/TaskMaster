# P4-T2 — Compile proof after the D5 notifier seam landed

Timestamp: 2026-09-04T00-09

Command:

```
& $msbuild TaskMaster.sln /t:Build /m /p:Configuration=Debug "/p:Platform=Any CPU"
```

EXIT_CODE: 0

## Printed error and warning counts

```
Build succeeded.
    0 Warning(s)
    0 Error(s)
```

Errors: **0**. Warnings: **0**.

## Output-assembly write-time proof

QuickFiler's Debug output assembly is `QuickFiler/bin/Debug/QuickFiler.dll`.

| Observation | `LastWriteTimeUtc` |
|---|---|
| Before the command | 2026-09-04T04:07:16.0744384Z |
| After the command | 2026-09-04T04:09:13.2590392Z |

The after value is later than the before value.

## P4-T1's recorded observations

P4-T1 is a source edit that writes no evidence artifact of its own. Its observations are recorded
here.

**D5's attribute-spelling choice.** `System.Diagnostics.CodeAnalysis` is not imported by
`QuickFiler/Controllers/EfcFormController.cs` — the file imports `System.Diagnostics` but not that
child namespace — so D5 left the implementation a choice between adding a using directive and
spelling the attribute fully qualified. **The implementation spells the attribute fully qualified as
`[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]` and adds no using directive**, so the
file's using block is untouched by this item. `using System.Threading;` was already present at line
10, so `AsyncLocal<T>` resolved with no new directive, as D5 predicted.

**Counted values established by P4-T1**, all measured in
`QuickFiler/Controllers/EfcFormController.cs` after formatting:

| Observation | Value |
|---|---|
| `UserFaultNotifier` declarations | **1** |
| `ShowModelessFaultNotice` declarations | **1** |
| `System.Windows.Forms.Application` occurrences | **1** |
| `MessageBox` occurrences | **3** — equal to the value P0-T9 recorded, so no modal dialog was introduced |
| `.Dispose()` occurrences | **3** — exactly one greater than the recorded 2 |
| File line count | **1308**, within the D7 budgeted ceiling of 1330 |

**The single added `.Dispose()` occurrence is inside `ShowModelessFaultNotice`.** That member is
declared at line 189; the added occurrence is at line 215, `notice.FormClosed += (sender, args) => notice.Dispose();`.
The two pre-existing occurrences, at lines 857 and 942, are both `_formViewer.Dispose();` and are
untouched.

**The `BoundaryErrorSink` property initializer is not changed by P4-T1** and still reads:

```
internal System.Action<string, System.Exception> BoundaryErrorSink { get; set; } =
    (message, exception) => logger.Error(message, exception);
```

It still contains the token `logger.Error`, which is the acceptance clause distinguishing this
declaration-only task from P4-T5's behavioural change.

Output Summary: `/t:Build` exited 0 with `0 Warning(s)` and `0 Error(s)`; the QuickFiler Debug output
assembly's `LastWriteTimeUtc` advanced from 2026-09-04T04:07:16.0744384Z to
2026-09-04T04:09:13.2590392Z. P4-T1 spelled the exclusion attribute fully qualified rather than
adding a using directive, and established `UserFaultNotifier` = 1, `ShowModelessFaultNotice` = 1,
`System.Windows.Forms.Application` = 1, `MessageBox` = 3 (unchanged), `.Dispose()` = 3 with the added
occurrence at line 215 inside `ShowModelessFaultNotice`, and a file line count of 1308.
