# Issue #483 — Defect-Preserving Notifier Seam Compiles

Timestamp: 2026-08-26T09-20
Task: [P3-T2]

Command (under `pwsh -NoProfile`, `[P0-T4]`-resolved MSBuild):

```
MSBuild.exe TaskMaster.sln /t:Build /m /p:Configuration=Debug "/p:Platform=Any CPU"
```

EXIT_CODE: **0**

```
Build succeeded.
    5 Warning(s)
    0 Error(s)
```

## Scope of this task

Not an analyzer or nullable gate (decision D2). `/t:Build` without `/p:EnableNETAnalyzers`,
`/p:EnforceCodeStyleInBuild`, or `/p:TreatWarningsAsErrors`; its sole purpose is to confirm the
`[P3-T1]` seam compiles. The gates are `[P7-T3]` and `[P7-T4]` with `/t:Rebuild`.

The 5 warnings are the unchanged pre-existing `System.Reactive` `packages.config` notices.

## What was verified to compile

- `internal System.Action<string> MoveFailureNotifier { get; set; } = text => MessageBox.Show(text);`
  at `QuickFiler/Controllers/QfcItemController.MailActions.cs:30`.
- `private void NotifyMoveFailure(string message)` at `:35`, which invokes the notifier directly when
  `_uiDispatcher` is null and marshals through `dispatcher.Invoke` when it is not.
- The `catch` block's direct `MessageBox.Show(...)` call replaced by `NotifyMoveFailure(...)` at `:140`.

`System.Action<string>` is written fully qualified because `MailActions.cs` imports
`Microsoft.Office.Interop.Outlook`, whose non-generic `Action` type would otherwise be a candidate; the
file already writes `System.Action` at `:54` and `System.Exception` at `:115` for the same reason.

**No rethrow and no cancellation check were added in `[P3-T1]`.** `MoveMailAsync` still returns normally
from the `catch`, so the #483 defects are still present and the regression tests in `[P3-T4]` are
expected to fail.

Output Summary: The solution builds with exit code 0 and 0 errors after the defect-preserving seam
introduction.
