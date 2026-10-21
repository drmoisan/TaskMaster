# P3-T7 — Pre-Existing Queue Tests Unaffected

Timestamp: 2026-08-26T09-32

Command: `pwsh -NoProfile -Command '& "scripts\vscode\Invoke-VSBuild.ps1" -Target Build; $vsw = Join-Path ${env:ProgramFiles(x86)} "Microsoft Visual Studio\Installer\vswhere.exe"; $vs = & $vsw -latest -products * -find "Common7\IDE\Extensions\TestPlatform\vstest.console.exe" | Select-Object -First 1; & $vs "QuickFiler.Test\bin\Debug\QuickFiler.Test.dll" /InIsolation "/TestCaseFilter:FullyQualifiedName~MalformedInboundJson_ThrowsCodecExceptionWithoutCorruptingState|FullyQualifiedName~MalformedInboundJson_ViaHostEvent_IsContainedAtTheBoundary" "/Logger:trx;LogFileName=results.trx" "/ResultsDirectory:docs/features/active/breadcrumb-router-navigation-defects-498/evidence/regression-testing/trx/p3-t7"; "EXIT_CODE: $LASTEXITCODE"'`

EXIT_CODE: 0

## Output Summary

**Both pass, and neither method body was edited.** TRX at
`docs/features/active/breadcrumb-router-navigation-defects-498/evidence/regression-testing/trx/p3-t7/results.trx`
records `<Counters total="2" executed="2" passed="2" failed="0" error="0" timeout="0" aborted="0" ... />`.

| Test | Location | Outcome |
|---|---|---|
| `MalformedInboundJson_ThrowsCodecExceptionWithoutCorruptingState` | `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterQueueTests.cs:175-191` | Passed |
| `MalformedInboundJson_ViaHostEvent_IsContainedAtTheBoundary` | same file, `:194-205` | Passed |

### Proof that neither method body was edited

`git diff <BASELINE_COMMIT> -- QuickFiler.Test/Controllers/BreadcrumbBridgeRouterQueueTests.cs`
reports `1 file changed, 1 insertion(+), 1 deletion(-)`, and the whole diff is the single hunk that
`P2-T1` required:

```
@@ -20,7 +20,7 @@ namespace QuickFiler.Test.Controllers
     [TestClass]
-    public class BreadcrumbBridgeRouterQueueTests
+    public partial class BreadcrumbBridgeRouterQueueTests
```

No other line of that file changed, so both cited method bodies are byte-identical to their state
before this feature began. Their line numbers are also unchanged, because the edit was within a
line rather than an insertion.

### Why these two are the right control

Both exercise the same `async void` host-message boundary that #498 concerned, from the opposite
direction: the codec's `BreadcrumbMessageException` must still propagate on the direct
`ProcessInboundAsync` path and must still be contained by the single
`catch (BreadcrumbMessageException)` on the host-event path. The `P2-T4` range guard could have
broken either behavior by short-circuiting too early or by widening the catch, and the `P3-T3`
`SelectedFolderPath` clear could have broken the first test's post-exception state assertions.
Neither occurred: the first test still observes `SelectedFolderPath` null after the malformed
payload and `Inbox\Projects\Alpha` after the subsequent valid selection.
