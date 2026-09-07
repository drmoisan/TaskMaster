# Issue #485 — Defect-Preserving Extraction Compiles

Timestamp: 2026-08-26T09-02
Task: [P2-T2]

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

This is **not** an analyzer or nullable gate (decision D2). It uses `/t:Build`, which MSBuild's
incremental up-to-date check can satisfy without recompiling, and it omits
`/p:EnableNETAnalyzers`, `/p:EnforceCodeStyleInBuild`, and `/p:TreatWarningsAsErrors`. Its sole purpose
is to confirm that the `[P2-T1]` extraction compiles. The analyzer and nullable gates are `[P7-T3]` and
`[P7-T4]`, which use `/t:Rebuild` against `TaskMaster.sln`.

The 5 warnings are the same pre-existing `System.Reactive` `packages.config` notices recorded in the
Phase 0 baseline; the count is unchanged from the baseline.

## What was verified to compile

- `internal static bool TryResolveCidResource(string requestedUri, IReadOnlyDictionary<string, IAttachment> contentIdMap, out byte[] payload, out string mimeType)`
  at `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs:227`, carrying the current **unguarded**
  decision logic.
- `private EventHandler<CoreWebView2WebResourceRequestedEventArgs> _webResourceRequestedHandler;` at `:34`.
- `private CoreWebView2 _coreWebView2;` at `:35`.
- The reduced two-statement lambda adapter at `:91-112`, assigned to `_webResourceRequestedHandler`
  before the `+=` at `:113`, with the event source captured into `_coreWebView2` at `:86`.

No guard was added in `[P2-T1]`, so the three #485 defects are still present and the regression tests in
`[P2-T4]` are expected to fail.

Output Summary: The solution builds with exit code 0 and 0 errors after the defect-preserving extraction.
