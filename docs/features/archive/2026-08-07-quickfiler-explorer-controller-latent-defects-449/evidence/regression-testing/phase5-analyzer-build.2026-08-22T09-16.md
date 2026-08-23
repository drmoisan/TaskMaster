# Phase 5 — Analyzer Build After Attribute Removal and Seam Introduction (Issue #449, [P5-T5])

Timestamp: 2026-08-22T09-16
WORKTREE: `C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-a5600546d71e73061`

Command:
```
"C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe" `
  TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" `
  /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true /v:n /nologo
```
EXIT_CODE: 0

Log: `.../scratchpad/449/p5t5-analyzer.log`, 11,648 lines.

## Result

```
5 Warning(s)
0 Error(s)
```

Count of `Skipping target "CoreCompile"`: **0 (zero)**. `/t:Rebuild` was used, not `/t:Build`.
The 5 warnings are the unchanged pre-existing `System.Reactive` v7.0 `packages.config` advisory.

## The decisive check — `using System;` was NOT restored

[P5-T5] instructs that if the build reports `CS0246` for the fully-qualified `System.Func` form, then
`using System;` must be restored and the restoration recorded in the [P4-T3] artifact.

Command: `grep -c 'CS0246' p5t5-analyzer.log`
EXIT_CODE: 1
Output: `0`

**Zero `CS0246`.** The seam declaration

```csharp
internal System.Func<
    string,
    string,
    MessageBoxButtons,
    MessageBoxIcon,
    DialogResult
> NotInViewDialogInvoker { get; set; } =
    (text, caption, buttons, icon) => MessageBox.Show(text, caption, buttons, icon);
```

compiles cleanly with **no `using System;` directive in the file**. The fully-qualified spelling is
what makes this possible: an unqualified `Func<...>` would have required `using System;` and would
therefore have re-introduced the very directive D4 removed, contradicting AC-8.

**No directive was restored.** The [P4-T3] artifact records "Restorations recorded: NONE". This is
the self-verifying property D4 relies on, and it resolved in favour of the removal: the file's `using`
count stands at **six**, not seven.

## Why this is the correct resolution of spec detail (b)

D4 removed `using System;` because the file had exactly two consumers of it, and this plan deleted
both: the `NotImplementedException` in the `ExplConvView_Cleanup` body (removed by [P3-T2]) and a
second consumer inside `SaveMessageAsMSG` in the dead region (removed by [P4-T1]). Declaring the new
seam with an unqualified `Func<...>` would have created a THIRD consumer and made the removal wrong.
The fully-qualified `System.Func<...>` form matches the file's existing fully-qualified style at
`log4net.ILog` and `System.Reflection.MethodBase` (lines 12-13) — which is precisely the stylistic
fact D4 cited when judging `using System;` orphaned in the first place. The style is therefore
self-consistent rather than an exception carved out for the seam.

## Changes covered by this build

- [P5-T1]: the class-level `[ExcludeFromCodeCoverage]` attribute was removed from
  `internal class QfcExplorerController : IQfcExplorerController`.
- [P5-T2]: `using System.Diagnostics.CodeAnalysis;` — the tenth and final directive of the D4
  disposition table — was removed, deferred from [P4-T2] because the attribute was its only consumer.
  Removing it before the attribute would have failed this build with `CS0246`, which is why the
  ordering is load-bearing.
- [P5-T3]: the `NotInViewDialogInvoker` seam was added as an `internal` settable auto-property with a
  production default, following the `QfcHomeController.QfcExplorerControllerLoader` idiom at
  `QuickFiler/Controllers/QfcHomeController.cs:175-182`.
- [P5-T4]: the not-in-view dialog call was routed through the seam, with the four argument values
  byte-identical to the pre-change `MessageBox.Show` call.

The file measures **182** lines after CSharpier formatting, well under the 500-line cap.

## Output Summary

Phase 5 analyzer build PASSED: **EXIT_CODE 0, 5 warnings, 0 errors**, warning count unchanged from
baseline, **zero** `Skipping target "CoreCompile"` occurrences. The decisive result is **zero
`CS0246`**: the fully-qualified `System.Func<string, string, MessageBoxButtons, MessageBoxIcon,
DialogResult>` seam declaration compiles with no `using System;` directive present, so **`using
System;` was NOT restored** and AC-8's ten-directive removal stands intact. Exactly six `using`
directives remain in the file.
