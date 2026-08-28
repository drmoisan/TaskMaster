# Baseline Per-File Coverage — Three In-Scope Production Files ([P0-T12])

Timestamp: 2026-08-27T20-06

Command:
```
grep -o 'filename="[^"]*Viewers[^"]*"' docs/features/active/webview2-host-initializer-defects-476/evidence/baseline/coverage-baseline.cobertura.xml | sort -u
grep -c 'WebView2BreadcrumbHost'  docs/features/active/webview2-host-initializer-defects-476/evidence/baseline/coverage-baseline.cobertura.xml
grep -c 'WebView2CoreInitializer' docs/features/active/webview2-host-initializer-defects-476/evidence/baseline/coverage-baseline.cobertura.xml
grep -c 'IWebViewCoreInitializer' docs/features/active/webview2-host-initializer-defects-476/evidence/baseline/coverage-baseline.cobertura.xml
```

EXIT_CODE: 0

## Per-file rows (aggregated by the Cobertura `filename` attribute)

| File | Line rate | Branch rate |
| --- | --- | --- |
| `QuickFiler/Viewers/WebView2BreadcrumbHost.cs` | ABSENT | ABSENT |
| `QuickFiler/Viewers/WebView2CoreInitializer.cs` | ABSENT | ABSENT |
| `QuickFiler/Viewers/IWebViewCoreInitializer.cs` | ABSENT | ABSENT |

## Output Summary

- The baseline Cobertura document contains **no `filename` attribute** matching any of the three
  paths. A filtered enumeration of every `filename="QuickFiler\Viewers\...">` value in the document
  lists 20 sibling files in that folder — including `BreadcrumbUiDispatcher.cs`,
  `BreadcrumbMessengerHub.cs`, and `BreadcrumbPopupUiOperations.cs` — and none of the three
  WebView2 files. The enumeration is therefore non-vacuous: the absence is specific to these three
  files, not an artifact of a wrong path spelling or a missing package.
- Substring counts over the whole document: `WebView2BreadcrumbHost` = 0 occurrences,
  `WebView2CoreInitializer` = 0 occurrences, `IWebViewCoreInitializer` = 6 occurrences. All six
  `IWebViewCoreInitializer` occurrences are method-signature text inside other classes
  (`QuickFiler.Viewers.IWebViewCoreInitializer,` and
  `signature="(QuickFiler.Viewers.IWebViewCoreInitializer,`), not a `<class>` element whose
  `filename` is `IWebViewCoreInitializer.cs`.
- `ABSENT` is the expected and correct pre-change reading, recorded as such rather than as zero:
  - `WebView2BreadcrumbHost` carries a class-level `[ExcludeFromCodeCoverage]`
    (`WebView2BreadcrumbHost.cs:29`), so dotnet-coverage emits no entry for the type at all.
  - `WebView2CoreInitializer` carries the same attribute (`WebView2CoreInitializer.cs:15`).
  - `IWebViewCoreInitializer.cs` declares an interface only, with no executable body, so there is
    nothing to instrument.
- Consequence for `[P4-T5]`: the "no reduction in coverage on any line this change modified" gate has
  no pre-change per-line figure to regress against for these three files, because none of their lines
  were measured. Every line that enters measurement in Phase 3 is new denominator. The blocking
  change-scoped gate that does bite is the `>= 90%` line coverage requirement on the newly measured
  members.
