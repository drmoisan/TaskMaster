# [P2-T14] — Corrected Coverage-Exemption Rationale on `WebView2CoreInitializer`

Timestamp: 2026-08-27T20-37

Command:
```
Select-String -SimpleMatch -Pattern '1:1'            -Path 'QuickFiler\Viewers\WebView2CoreInitializer.cs'
Select-String -SimpleMatch -Pattern 'forwards every' -Path 'QuickFiler\Viewers\WebView2CoreInitializer.cs'
& $msbuild TaskMaster.sln /t:Build /m /p:Configuration=Debug "/p:Platform=Any CPU"
```

EXIT_CODE: 0

## Search output before this task ran

```
PRE  TOKEN='1:1'            COUNT=1
  /// <see cref="IWebViewCoreInitializer"/> member 1:1 to the WebView2 SDK. The body is a thin
PRE  TOKEN='forwards every'  COUNT=1
  /// Production adapter (DI-seam "adapter" tier, research 3.3) that forwards every
```

Each token returned at least one matching line before the change, so the gate was falsifiable.

## Search output after this task ran

```
POST TOKEN='1:1'            COUNT=0   (no output)
POST TOKEN='forwards every'  COUNT=0   (no output)
```

Both searches return zero matching lines.

## What was implemented

The class documentation on `WebView2CoreInitializer` was rewritten. The false claim that the type
"forwards every `IWebViewCoreInitializer` member 1:1 to the WebView2 SDK" is gone, and with it the
exemption rationale that rested on it. The replacement states the accurate ground:

- both SDK calls require the external Evergreen WebView2 runtime, a separate process, which the
  external-dependency rule for unit tests prohibits;
- `CreateEnvironmentAsync` additionally creates a user-data folder on disk, which the
  no-temporary-files rule prohibits.

It also records that the type does not surface the SDK's `browserExecutableFolder` argument, so the
exemption does not rest on any claim of a mechanical member-for-member forward, and that the new
argument guards are pure validation, are a testable seam, are not exempt, and are measured.

The class-level `[ExcludeFromCodeCoverage]` is still present at this point in the plan;
`[P3-T3]` removes it and moves the attribute to the two extracted SDK forwards.

Build after the documentation change: `EXIT_CODE=0`, `0 Error(s)`.
