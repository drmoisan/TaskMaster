# P5-T2 — Factory Seam Verification (File Size and Call Sites)

Issue: #230
Task: [P5-T2]

## Step 1 — File-size check after the P5-T1 edit

- Timestamp: 2026-08-07T23-00
- Command: `dotnet tool run csharpier format QuickFiler/Controllers/QfcItemController.Initialization.cs` then `wc -l QuickFiler/Controllers/QfcItemController.Initialization.cs`
- EXIT_CODE: 0
- Output Summary: **485 lines**, within the 500-line repository limit. Baseline
  per D8 was 466 lines; the P5-T1 seam-parameter edit added 30 lines across the two
  factories while the Phase 2/3/4 attribute and comment rewrites removed 11 net,
  giving +19. Measured after a csharpier format pass, so the count is authoritative
  for this point in the change (P8-T2 re-measures after the final format).

## Step 2 — Call-site enumeration

- Timestamp: 2026-08-07T23-00
- Command:
  ```powershell
  Get-ChildItem -Path QuickFiler -Recurse -Include *.cs |
    Select-String -Pattern 'CreateSequentialAsync|CreateAsync'
  ```
  (plus a repository-wide search for `CreateSequentialAsync\s*\(` and
  `QfcItemController\.CreateAsync\s*\(` across every `**/*.cs`)
- EXIT_CODE: 0 (derived from `$?` = `True` per D14)
- Output Summary: **Zero in-repo callers of `QfcItemController.CreateAsync` or
  `QfcItemController.CreateSequentialAsync` other than their own declarations.**
  Every other `CreateAsync` hit belongs to an unrelated type.

### Full match list under `QuickFiler/`

| File:Line | Match | Relationship to the changed factories |
|---|---|---|
| `QuickFiler\Controllers\EfcDataModel.cs:88` | `public static async Task<EfcDataModel> CreateAsync(` | Different type (`EfcDataModel`) |
| `QuickFiler\Controllers\EfcDataModel.cs:102,108,136` | log-message strings | Not a call site |
| `QuickFiler\Controllers\EfcHomeController.cs:104,113` | `EfcHomeController.CreateAsync` declarations | Different type |
| `QuickFiler\Controllers\EfcHomeController.cs:110` | `await CreateAsync(globals, ...)` | `EfcHomeController`'s own overload |
| `QuickFiler\Controllers\EfcHomeControllerDependencyFactories.cs:37,111` | `EfcDataModel.CreateAsync` method group | Different type |
| `QuickFiler\Controllers\EfcItemController.cs:188,223` | `CoreWebView2Environment.CreateAsync` | WebView2 SDK |
| `QuickFiler\Controllers\QfcItemController.cs:64` | comment | Not a call site |
| `QuickFiler\Controllers\QfcItemController.Initialization.cs:379` | comment in `SaveParameters` | Not a call site |
| `QuickFiler\Controllers\QfcItemController.Initialization.cs:405` | **`QfcItemController.CreateAsync` declaration** | The changed member |
| `QuickFiler\Controllers\QfcItemController.Initialization.cs:447` | **`QfcItemController.CreateSequentialAsync` declaration** | The changed member |
| `QuickFiler\Controllers\QfcItemController.ViewerSetup.cs:118` | commented-out `CoreWebView2Environment.CreateAsync` | Not a call site |
| `QuickFiler\Controllers\QfcItemController.ViewerSetup.cs:259,277,282` | `QfcTipsDetails.CreateAsync` | Different type |
| `QuickFiler\Viewers\WebView2CoreInitializer.cs:22` | `CoreWebView2Environment.CreateAsync` | WebView2 SDK |

## Non-breaking-change assessment

- The change is **additive only**: three optional parameters
  (`UtilitiesCS.Threading.IUiDispatcher uiDispatcher = null`,
  `QuickFiler.Viewers.IWebViewCoreInitializer webViewInitializer = null`,
  `Func<MailItem, ConversationResolver> conversationResolverFactory = null`) were
  appended after the existing `CancellationToken token` parameter on both
  factories. No existing parameter was reordered, renamed, or retyped.
- The parameter names and types match the primary constructor's optional-seam
  parameters exactly (`QfcItemController.Initialization.cs:38-41`).
- The assignments happen after `new QfcItemController()` and **before**
  `controller.SaveParameters(...)`, so `SaveParameters`'s `??=` defaults still
  apply to every seam left null. Default behavior is byte-for-byte the previous
  behavior.
- **No call site passes the new parameters.** Because there are zero in-repo
  callers, no existing call site could break; and any external caller using the
  positional argument list compiles unchanged.
- Full-solution compilation is confirmed by the P5-T6 build (EXIT_CODE 0) and
  re-confirmed by the Phase 8 full-suite run.
