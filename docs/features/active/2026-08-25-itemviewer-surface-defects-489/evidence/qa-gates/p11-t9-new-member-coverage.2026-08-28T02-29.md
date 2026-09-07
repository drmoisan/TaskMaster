# P11-T9 — Line coverage of the new production member `CbxPictures_CheckedChanged`

Timestamp: 2026-08-28T02-29
Command: [xml]$x = Get-Content -Raw coverage\coverage.cobertura.xml; select the <class> whose filename attribute equals 'QuickFiler\Controllers\QfcItemController.EventHandlers.cs', then its <method> named 'CbxPictures_CheckedChanged', then count its <lines><line> children with hits greater than zero divided by their total
EXIT_CODE: 0

NewMemberLineRate: 1.0

Loop iteration: **1**. The report read is the post-processed Cobertura document P11-T8 run 2 left at
`coverage/coverage.cobertura.xml`.

## The separator is load-bearing, and the measurement proves it

```
filename = "QuickFiler\Controllers\QfcItemController.EventHandlers.cs"   ->  1 class matched
filename = "QuickFiler/Controllers/QfcItemController.EventHandlers.cs"   ->  0 classes matched
```

The backslash form is the one that selects the element. This is not incidental:
`scripts/vscode/Invoke-MSTestWithCoverage.ps1:340` calls `ConvertTo-KoverageCoberturaXml` without
`-PathSeparator`, and that parameter defaults to the platform separator
(`Invoke-MSTestWithCoverage.Helpers.ps1:408`), which on Windows is `\`. A forward-slash match would
have selected zero `<class>` elements, `NewMemberLineRate:` could not have been produced at all, and
AC57 would have had no deliverable value. The forward-slash count of **0** is recorded above so that
the backslash result is demonstrably a real selection rather than a lucky one.

The selected class is `QuickFiler.Controllers.QfcItemController`, class line-rate
`0.7934782608695652`.

## The member

```
<method name="CbxPictures_CheckedChanged" signature="(object, System.EventArgs)" line-rate="1">
```

Its `<lines>` children: **3** lines total, **3** with `hits` greater than zero.

```
NewMemberLineRate = coveredLines / totalLines = 3 / 3 = 1.0
```

The source is `QuickFiler/Controllers/QfcItemController.EventHandlers.cs:223-226`:

```csharp
private void CbxPictures_CheckedChanged(object sender, EventArgs e)
{
    _optionsPictures = _itemViewer.PicturesChecked;
}
```

Three sequence points — the signature line, the opening brace and the assignment — all covered.

## Acceptance

**`NewMemberLineRate:` is at least `0.90`.** It is `1.0`.

**The covering test is named.** `PicturesChanged_WhenRaised_RefreshesOptionsPictures`, declared at
`QuickFiler.Test/Controllers/QfcItemController.EventWiringTests.Part2.cs:56`. It is recorded as
`Passed` in `evidence/qa-gates/p11-t7.trx` and is listed in the P11-T7 named-pin block. It exercises
the handler by raising the viewer's `PicturesChanged` event and asserting the controller refreshed
its `_optionsPictures` state, which is exactly the assignment the handler performs.

## No coverage figure is attributed to any `ItemViewer*.cs` change

`QuickFiler/Viewers/ItemViewer.cs:20` carries `[ExcludeFromCodeCoverage]`, so that type contributes
no measured line, and this feature's edits to it are deletions, which change no measured line either
way. `CbxPictures_CheckedChanged` in `QfcItemController.EventHandlers.cs` is the **only** new
production member this feature adds, and it is the only member this task reports on.

Output Summary: The new-member coverage gate **passes**. `NewMemberLineRate: 1.0` — all 3 of the 3
`<line>` children of the `CbxPictures_CheckedChanged` `<method>` element have non-zero hits — which
is at least the required `0.90`. The `<class>` was located by an exact match on the backslash
filename form `QuickFiler\Controllers\QfcItemController.EventHandlers.cs`, which matched 1 element;
the forward-slash spelling matched 0, confirming the separator is load-bearing rather than
incidental. The covering test is `PicturesChanged_WhenRaised_RefreshesOptionsPictures`
(`QuickFiler.Test/Controllers/QfcItemController.EventWiringTests.Part2.cs:56`), recorded as passed in
`p11-t7.trx`. No coverage figure is attributed to any `ItemViewer*.cs` change, since that type
carries `[ExcludeFromCodeCoverage]` and its edits are deletions.
