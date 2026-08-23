# `Views` Indexer Parameter Type — Compile-Time Confirmation (Issue #449, [P6-T1])

Timestamp: 2026-08-22T09-16
WORKTREE: `C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-a5600546d71e73061`

Command:
```
"C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe" `
  TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" `
  /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true /v:n /nologo
```
EXIT_CODE: 0

## Confirmed parameter type: `object`

Research section 5.3 expected the `Microsoft.Office.Interop.Outlook.Views` indexer parameter to be
`object`, and predicted the Moq setup would therefore be
`views.Setup(v => v[It.IsAny<object>()]).Returns(view.Object)`. **That expectation is confirmed.**

The setup was written in that exact form and the solution compiled with EXIT_CODE 0 and zero errors:

```csharp
private Mock<Views> ArrangeViewsIndexer(Mock<Outlook.View> view)
{
    var views = _repo.Create<Views>();
    views.Setup(v => v[It.IsAny<object>()]).Returns(view.Object);
    return views;
}
```

**The compiling parameter type is `object`.** No adjustment was required. Had the PIA declared a typed
overload instead, this build would have failed with a CS1503 argument-conversion error or a CS1061
"no suitable indexer" error, and the remedy would have been the one-token change of the type argument
inside `It.IsAny<...>()` — which cannot invalidate the surrounding mock harness because it changes
only the matcher's type parameter.

## Why the confirmation was needed

No `Mock<Views>` existed anywhere in this repository before this change, so there was no in-repo
precedent for this specific collection. The indexer-mocking FORM is nonetheless proven on another
Outlook collection, at `QuickFiler.Test/Helper Classes/MailItemInfoTests.cs:64-65`:

```csharp
this.mockRecipients.Setup(x => x[It.IsAny<int>()])
    .Returns<int>(i => recipientList.ElementAt(i));
```

That precedent uses `int` because the `Recipients` indexer is typed `int`. The type argument is
per-collection, which is exactly why [P6-T1] required a compile-time confirmation for `Views` rather
than an assumption carried over from `Recipients`.

## Production call site this supports

`QuickFiler/Controllers/QfcExplorerController.cs:126`, inside `ExplConvView_ToggleOn`:

```csharp
_objView = _activeExplorer.CurrentFolder.Views[_objViewMem];
```

`_objViewMem` is a `string`, and it binds to the `object` indexer parameter by implicit reference
conversion. This is the call path exercised by the tests added in [P6-T5] and [P6-T6].

## Output Summary

The `Microsoft.Office.Interop.Outlook.Views` indexer parameter type is **`object`**, confirmed at
compile time: the setup `views.Setup(v => v[It.IsAny<object>()]).Returns(view.Object)` was written and
the full-solution `/t:Rebuild` returned **EXIT_CODE 0** with zero errors. Research section 5.3's
expectation is confirmed and no type adjustment was needed. The indexer-mocking form follows the
in-repo precedent at `QuickFiler.Test/Helper Classes/MailItemInfoTests.cs:64-65`, which uses `int`
because the `Recipients` indexer is typed differently — the per-collection difference is why this
confirmation was required.
