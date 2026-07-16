# Research: QuickFiler inline-image `cid:` fix (issue #326, epic child 9004)

- Date: 2026-07-15T17-10
- Scope: research only, no production code changed.
- Workspace: `C:/Users/DanMoisan/repos/TaskMaster/.claude/worktrees/agent-a1e77dc4a849cd790`

## 1. Root-cause confirmation and body-render path

Confirmed as stated in the issue/spec. No code in the repository reads `PR_ATTACH_CONTENT_ID`,
registers `CoreWebView2.WebResourceRequested`, or calls
`SetVirtualHostNameToFolderMapping` (grep across the full worktree for `0x3712`,
`ContentId`/`AttachContentId`, and `WebResourceRequested` returns no production matches).

Render path, file + line evidence:

- `UtilitiesCS/OutlookObjects/MailItem/MailItemHelper.Properties.cs:222-227` — `Html` is a
  `virtual`, lazily-computed property: `_html = new(() => GetHtml(HTMLBody), true)` is wired in
  `MailItemHelper.Loading.cs:112`.
- `UtilitiesCS/OutlookObjects/MailItem/MailItemHelper.Html.cs:190-205` — `GetHtml()` /
  `GetHtml(string htmlBody)` read `_item.HTMLBody` (raw Outlook `MailItem.HTMLBody`) and splice
  `EmailHeader` after the `<body...>` tag via regex. Neither overload touches attachments or
  `cid:` references. (Note, pre-existing and out of scope: `GetHtml(string htmlBody)` ignores its
  `htmlBody` parameter and re-reads `_item.HTMLBody` — do not "fix" this as part of #326; it is
  unrelated to the cid: defect and changing it would widen scope.)
- `QuickFiler/Controllers/QfcItemController.EventWiring.cs:106-153` —
  `HandleWebViewInitializedAsync(bool isSuccess, Exception initException)` is the single call site
  that renders the body: after the WebView2 `CoreWebView2InitializationCompleted` event fires
  (wired at `QfcItemController.EventWiring.cs:85-86`,
  `_itemViewer.WebViewInitializationCompleted += WebView2Control_CoreWebView2InitializationCompleted`),
  it waits for `ItemHelper` to be non-null, then calls
  `_itemViewer.NavigateToString(ItemHelper.Html)` (line 139 on the UI thread via `Invoke`, line 143
  otherwise). This is the only `NavigateToString` call in the codebase reachable from mail
  rendering.
- `QuickFiler/Viewers/ItemViewer.WebViewThread.cs:15` —
  `public void NavigateToString(string html) => L0v2h2_WebView2.NavigateToString(html);` forwards
  directly to the WebView2 SDK with no rewriting.
- `QuickFiler/Viewers/IItemViewer.cs:101-102` — the interface exposes only
  `void NavigateToString(string html)` and the `WebViewInitializationCompleted` event; it does not
  expose the underlying `CoreWebView2` object, so any code that needs `CoreWebView2` (e.g. to
  register `WebResourceRequested`) must concrete-cast to `ItemViewer`, matching the existing
  pattern at `QfcItemController.ViewerSetup.cs:63`
  (`((ItemViewer)_itemViewer).L0v2h2_WebView2`).

**Compact vs. expanded mode share the identical call.** Only one control class is instantiated in
production: `QuickFiler/Helper Classes/ItemViewerQueue.cs:103-106`
(`private static ItemViewer CreateProductionViewer() => new ItemViewer();`) is the sole
construction site for the pooled viewer (grepped `new ItemViewer(`, `new ItemViewerExpanded(`,
`new QfcItemViewer(`, `new QfcItemViewerExpanded(` across `QuickFiler/` — only the first matches).
`ItemViewer.cs:20-30` is `[ExcludeFromCodeCoverage] public partial class ItemViewer : UserControl,
IItemViewer` with a single Designer-declared `L0v2h2_WebView2` control
(`ItemViewer.Designer.cs:228-248`). `_expanded` (`QfcItemController.cs:144-146`) is a plain bool
field that only gates which keyboard actions get registered
(`QfcItemController.EventWiring.cs:206-210, 298-302`) — it is a layout/size state on the one
`ItemViewer`, not a distinct control class. `ItemViewerExpanded.cs` /
`QfcItemViewer.cs` / `QfcItemViewerExpanded.cs` exist in the tree but are not constructed anywhere
in production code; they are dead/legacy classes with their own separate `L0v2h2_WebView2`
Designer fields and are irrelevant to this fix. **Conclusion:** the hypothesis is confirmed exactly
— one `WebView2`, one `NavigateToString(ItemHelper.Html)` call, no `cid:` resolution anywhere; this
is a missing-feature defect, not a mode-specific regression.

## 2. WebView2 `cid:` resolution mechanism — evaluated and recommended

Both candidates were evaluated against current Microsoft Learn documentation (fetched during this
research session).

**(b) `SetVirtualHostNameToFolderMapping` — rejected.** Microsoft's own "Custom management of
network requests" article states explicitly: *"For URLs with virtual hostnames, using the
`WebResourceRequested` event isn't supported. This is because the `WebResourceRequested` event
isn't fired for the `SetVirtualHostNameToFolderMapping` method."* This mechanism requires the
mapped content to exist as real files under a folder on disk; WebView2 serves them directly,
bypassing any event-based interception. For per-message inline images (bytes that differ for every
mail item and exist only in COM/MAPI memory), this would require writing every inline attachment's
bytes to a scratch folder before every render and cleaning it up afterward. That conflicts with
this repo's I/O-isolation policy (`.claude/rules/general-code-change.md` "Core domain logic must
be testable without touching the network or filesystem") and adds per-render disk churn and cleanup
error-handling for no benefit over the in-memory alternative. Rejected.

**(a) `CoreWebView2.WebResourceRequested` + `AddWebResourceRequestedFilter` — recommended.** This
event/filter pair lets the host intercept a matched request and supply a response built from an
in-memory `MemoryStream` via `CoreWebView2Environment.CreateWebResourceResponse(stream, 200, "OK",
"Content-Type: <mime>")` (documented example: `webresourcerequested.md`, "Overriding a response, to
proactively replace it"). No disk file is required. `AddWebResourceRequestedFilter`'s `uri`
parameter is a glob-style wildcard matched against the full normalized request URI, confirmed by
the reference doc's match table (e.g. `*://contoso.com/*` matches all schemes under that host) —
this supports scoping the filter to a dedicated virtual host without needing a `SetVirtualHostName…`
mapping.

**Known WebView2/Chromium constraint and required workaround.** `cid:` is not a scheme Chromium's
renderer treats as fetchable ("special" scheme); an `<img src="cid:...">` reference is rejected by
the renderer before a network request is ever dispatched, which is consistent with the reported
symptom (broken/missing image, not a slow-loading or 404 image) and is why no
`WebResourceRequested` handler alone — however it is filtered — can intercept a raw `cid:` URI.
This is a widely corroborated constraint for Chromium-based embedded browsers but the docs fetched
in this session do not state it in those exact terms; treat it as a documented-by-symptom
observation to be verified manually against a live render (see §5), not as a citable guarantee.
The standard, low-risk mitigation — used in this design — is to **rewrite `cid:` references to a
scheme WebView2 will actually dispatch a request for** (an `https://` virtual host, e.g.
`https://cid.quickfiler.local/<content-id>`) inside the HTML string *before* calling
`NavigateToString`, and then intercept that rewritten URL with `WebResourceRequested`. Content
navigated via `NavigateToString` does not block this: `AddWebResourceRequestedFilter` matches on
the destination request URI, not the origin of the navigating document, so the null/opaque origin
of `NavigateToString` content does not prevent the filter from matching the rewritten
`https://cid.quickfiler.local/*` sub-resource requests it issues.

Citations fetched this session:
- Microsoft Learn — "Custom management of network requests" (`webview2/how-to/webresourcerequested`,
  updated 2026-06-12): `WebResourceRequested`/`WebResourceResponseReceived` semantics, the
  `SetVirtualHostNameToFolderMapping` incompatibility note, and the image-response-override sample.
- Microsoft Learn — `CoreWebView2.AddWebResourceRequestedFilter` API reference: wildcard filter
  syntax and matching semantics (glob `*`/`?`, matched against the full normalized URI regardless
  of scheme).

## 3. Attachment bytes and Content-Id access — in-memory path already established

An in-memory (no temp file) pattern for reading attachment bytes via `PropertyAccessor` already
exists and is the primary path in this codebase, established precedent for the same technique
applied to Content-Id:

- `UtilitiesCS/Interfaces/IEmailIntelligence/IAttachment.cs:5-24` — the interface already exposes
  `byte[] AttachmentData { get; set; }`, `bool IsImage { get; }`, and
  `PropertyAccessor PropertyAccessor { get; }`. **No `ContentId` member exists yet** — this is the
  gap to fill.
- `UtilitiesCS/OutlookObjects/Attachment/AttachmentSerializable.cs:179-194` (`TryFromAccessor`) —
  primary, in-memory byte read: `attachment.PropertyAccessor.GetProperty(PR_ATTACH_DATA_BIN)` where
  `PR_ATTACH_DATA_BIN = "http://schemas.microsoft.com/mapi/proptag/0x37010102"`. Only on failure
  does the code fall back to `TryFromSaveAsLoad` (`AttachmentSerializable.cs:158-177`, disk-based
  `attachment.SaveAsFile(tempFilePath)` + `File.ReadAllBytes` + `File.Delete`) or
  `attachment.GetTemporaryFilePath()` (`GetBytes`, line 130-156, used only for
  `Type != olByValue`, i.e. embedded-message/OLE attachments). For `olByValue` inline image
  attachments (the case in scope for `cid:` resolution), the accessor path is hit directly and no
  disk I/O occurs.
- `UtilitiesCS/OutlookObjects/Attachment/AttachmentHelper.cs:139-141` — the same
  `PR_ATTACH_DATA_BIN` constant is duplicated here (also unused disk-saving helper machinery, not
  relevant to this fix).
- **Content-Id proptag (not yet read anywhere):** the standard MAPI proptag for
  `PidTagAttachContentId` is `http://schemas.microsoft.com/mapi/proptag/0x3712001F` (Unicode
  string). Applying the exact same try/catch-wrapped `PropertyAccessor.GetProperty(...)` pattern as
  `TryFromAccessor` reads it in-memory with no disk I/O. Most non-inline attachments do not carry
  this property; `PropertyAccessor.GetProperty` throws a `COMException` when a property is absent,
  so the read must be wrapped in try/catch and default to `null`/empty, exactly like
  `TryFromAccessor`'s existing `catch (System.Exception) { return false; }` pattern.
- **Test mockability confirmed:** `UtilitiesCS.Test/OutlookObjects/Attachment/AttachmentSerializableTests.cs`
  and `AttachmentHelperTests.cs` already mock
  `Microsoft.Office.Interop.Outlook.Attachment` and `PropertyAccessor` directly with Moq (e.g.
  `AttachmentSerializableTests.cs:162-166`:
  `new Mock<PropertyAccessor>()...Setup(x => x.GetProperty("http://schemas.microsoft.com/mapi/proptag/0x37010102"))`).
  Both are COM interop **interfaces**, not sealed classes, so Moq can mock them without any
  wrapper — this is the established, repo-proven seam for testing MAPI-proptag reads without a
  live Outlook process, and the same technique applies unchanged to a new `ContentId` read.

## 4. Testability seam design

Recommended split, following the repo's existing exemption pattern (WebView2 SDK calls stay behind
a thin, `[ExcludeFromCodeCoverage]` concrete-bound seam; the substantive logic is pure/host-neutral
and tested directly):

1. **Extend `IAttachment` / `AttachmentSerializable`** with a `string ContentId { get; set; }`
   member, populated in the `AttachmentSerializable(Attachment a, ...)` constructor using the same
   `TryFromAccessor`-style try/catch read against
   `http://schemas.microsoft.com/mapi/proptag/0x3712001F`. Testable with the existing
   `Mock<PropertyAccessor>().Setup(x => x.GetProperty(...))` pattern already used in
   `AttachmentSerializableTests.cs`.

2. **New host-neutral helper file** (pure logic, no COM/WebView2 types), e.g.
   `UtilitiesCS/OutlookObjects/MailItem/CidImageResolver.cs`:
   - `RewriteCidReferences(string html, IReadOnlyCollection<IAttachment> attachments, string
     virtualHost)` — a pure string transform (`Regex.Replace` on `src=['"]cid:([^'"]+)['"]`,
     case-insensitive) that rewrites each `cid:<id>` reference to
     `https://<virtualHost>/<url-encoded id>` **only when a matching attachment's `ContentId`
     exists** (leave unmatched `cid:` references untouched so `WebResourceRequested` never needs to
     invent a 404). This is unit-testable with plain HTML strings and fake `IAttachment` records —
     no Outlook, no WebView2, no temp files.
   - A companion pure lookup builder, e.g. `BuildContentIdMap(IReadOnlyCollection<IAttachment>
     attachments)` returning `IReadOnlyDictionary<string, IAttachment>` keyed by `ContentId`
     (case-insensitive, e.g. `StringComparer.OrdinalIgnoreCase`), used later by the
     `WebResourceRequested` handler to resolve a request's content-id back to attachment bytes.
     Testable with `IAttachment[]` fakes/mocks; no COM required.

3. **`MailItemHelper.Html.cs`** — call `CidImageResolver.RewriteCidReferences(...)` inside
   `GetHtml()`/`GetHtml(string htmlBody)`, passing `AttachmentsHelper`/`AttachmentsInfo` (already a
   lazily-loaded member of `MailItemHelper`, `MailItemHelper.Properties.cs:243-265`) and a fixed
   virtual-host constant. This keeps `QfcItemController.EventWiring.cs`'s
   `_itemViewer.NavigateToString(ItemHelper.Html)` call **completely unchanged** — both compact and
   expanded modes get the fix automatically because they already share that one call, satisfying
   the epic's non-goal ("no change to compact-mode rendering beyond the shared `cid:` resolution").
   This method is already unit-tested without a live Outlook process
   (`MailItemHelperCoreTests.cs:68-87`, `GetHtml_ShouldInjectEmailHeaderIntoBodyMarkup`, using
   `Mock<InteropMailItem>` and reflection-based `SetField`/`SetLazyField` helpers) — the same
   pattern extends directly to asserting rewritten `cid:` output.

4. **WebView2 setup/wiring (host-bound, `[ExcludeFromCodeCoverage]`, thin glue only)** —
   `QfcItemController.ViewerSetup.cs`'s `InitializeWebViewAsync` (already
   `[ExcludeFromCodeCoverage]`, already the one-time-per-`ItemViewer` CoreWebView2 setup point,
   already concrete-casts to `((ItemViewer)_itemViewer).L0v2h2_WebView2`) is the natural place to
   register `AddWebResourceRequestedFilter("https://<virtualHost>/*",
   CoreWebView2WebResourceContext.Image)` and a `WebResourceRequested` handler once per
   `ItemViewer`'s WebView2 lifetime (not once per mail item, since `ItemViewer` instances are pooled
   and reused — `ItemViewerQueue.cs`). The handler closure should read `ItemHelper` (a mutable field
   on the owning `QfcItemController`) at request time via `CidImageResolver.BuildContentIdMap(...)`,
   so it always resolves against whichever mail is currently loaded into that pooled viewer, and
   build the response via `CoreWebView2Environment.CreateWebResourceResponse(new
   MemoryStream(attachment.AttachmentData), 200, "OK", $"Content-Type: {mimeType}")`. This glue
   cannot be exercised headlessly (no real `CoreWebView2` instance in a unit test host) and should
   carry the same `[ExcludeFromCodeCoverage]` justification already used for
   `InitializeWebViewAsync` and `WebView2CoreInitializer` — the substantive logic it calls
   (`RewriteCidReferences`, `BuildContentIdMap`) is what actually gets unit coverage.

This design keeps 100% of the new coverage-bearing logic (HTML rewrite, content-id lookup,
Content-Id property read) in plain C# classes/methods that take primitives, strings, and the
already-mockable `IAttachment`/`PropertyAccessor` interop interfaces — no new COM or WebView2
mocking infrastructure is required, and no temp files are used anywhere in the new code or its
tests.

## 5. Bugfix Workflow — smallest deterministic failing regression test

Per the repo's mandatory Bugfix Workflow, write the failing test first, targeting the host-neutral
unit — a live WebView2 render cannot be asserted in an MSTest unit test (no headless `CoreWebView2`
rendering surface, and per policy tests must not depend on external processes), so the regression
target must be the resolution/rewrite unit, not the rendered pixels.

Recommended smallest failing test (MSTest + FluentAssertions, no Moq needed for this specific
case since `IAttachment` is a plain interface):

- **Unit under test:** `CidImageResolver.RewriteCidReferences(string html, IReadOnlyCollection<IAttachment>
  attachments, string virtualHost)` (new class, §4.2).
- **Arrange:** an HTML string containing `<img src="cid:logo1">` and a fake `IAttachment` (e.g. a
  minimal test double or `AttachmentSerializable` instance with `ContentId = "logo1"` and
  `AttachmentData = new byte[] {1,2,3}` set directly via its property setters — no COM object
  needed since these are plain settable properties) with `virtualHost = "cid.quickfiler.local"`.
- **Act:** call `RewriteCidReferences(html, new[] { attachment }, "cid.quickfiler.local")`.
- **Assert:** `result.Should().Contain("src=\"https://cid.quickfiler.local/logo1\"")` and
  `result.Should().NotContain("cid:logo1")`.
- **Why it fails before the fix:** `CidImageResolver` does not exist yet, so the test does not
  compile/resolves against nothing — tag this `[expect-fail]` in the atomic plan as a
  not-yet-implemented-type failure (consistent with adding new coverage-bearing code per the
  Bugfix Workflow's "smallest deterministic test that reproduces the bug" — here the "bug" is
  reproduced by the absence of the rewrite, which the test's assertions on the *unmodified* `cid:`
  string would equally demonstrate once the type exists as a no-op stub).
- **Why it passes after:** once `RewriteCidReferences` performs the regex substitution keyed off
  the resolved `ContentId`, the assertions hold.
- A second test should cover the negative/no-match edge case: an `<img src="cid:unknown">`
  reference with no attachment whose `ContentId` matches must be left untouched (`result.Should()
  .Contain("cid:unknown")`), so the `WebResourceRequested` handler never has to serve a 404 for a
  reference the resolver silently dropped.
- Manual verification (not part of the automated suite, called out explicitly per policy): a live
  QuickFiler expanded-mode render against a real inline-image message, confirming the image
  renders and that compact mode (same call path) is unaffected beyond the fix.

## 6. File-overlap check against sibling epic features

Epic manifest (`docs/features/epics/folder-tree-percentage-ui/epic.md`) confirms 9004 is isolated:
*"Feature 9004 is isolated in the QuickFiler WebView2 body-rendering path and shares no files with
the tree/percentage work."* Sibling features 9001 (`FolderScorer`/`FolderPredictor` probability
plumbing), 9002 (`EfcViewer.cs`/`EfcViewer3.cs`, `FolderListBox`), and 9003 (`CboFolders` /
QuickFiler viewer Designer variants) touch none of the files below; confirmed by inspection during
this session that none of `MailItemHelper.Html.cs`, `AttachmentSerializable.cs`, `IAttachment.cs`,
`QfcItemController.ViewerSetup.cs`, or `QfcItemController.EventWiring.cs` reference `FolderScorer`,
`FolderPredictor`, `EfcViewer`, `CboFolders`, or folder-list/ComboBox rendering.

**Exact production files this fix will touch:**

1. `UtilitiesCS/OutlookObjects/MailItem/MailItemHelper.Html.cs` — call the new rewrite inside
   `GetHtml()` / `GetHtml(string htmlBody)`.
2. `UtilitiesCS/OutlookObjects/MailItem/CidImageResolver.cs` — **new file**, host-neutral
   `RewriteCidReferences` + `BuildContentIdMap` pure logic.
3. `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs` — extend `InitializeWebViewAsync`
   (already `[ExcludeFromCodeCoverage]`) to register `AddWebResourceRequestedFilter` +
   `WebResourceRequested` once per `ItemViewer`.

Small, necessarily-coupled additions (still zero overlap with sibling features, kept out of the
"target 2-3" count above because they are interface/property additions rather than behavior
changes to the render path itself):

4. `UtilitiesCS/Interfaces/IEmailIntelligence/IAttachment.cs` — add `string ContentId { get; set;
   }`.
5. `UtilitiesCS/OutlookObjects/Attachment/AttachmentSerializable.cs` — populate `ContentId` in the
   constructor using the `TryFromAccessor`-style in-memory `PropertyAccessor.GetProperty` read.

No changes are anticipated to `EfcViewer.cs`, `EfcViewer3.cs`, `CboFolders`, any `QfcItemViewer*`
Designer file, `FolderScorer`, or `FolderPredictor`.

## Constraints honored in this design

- No temp files anywhere in the new code or its tests (in-memory `PropertyAccessor.GetProperty` for
  both bytes and Content-Id; `MemoryStream`-backed `CreateWebResourceResponse` for the intercepted
  image bytes; `SetVirtualHostNameToFolderMapping` rejected specifically because it requires disk
  files).
- MSTest + FluentAssertions for the new tests; Moq is available but not strictly required for the
  `CidImageResolver` tests themselves (plain interface fakes suffice), and remains the established
  tool for the `AttachmentSerializable`/`PropertyAccessor` Content-Id test.
- File-size ceiling (500 lines): `MailItemHelper.Html.cs` is currently 209 lines with room for the
  new call; `CidImageResolver.cs` is a new, small, single-purpose file; `QfcItemController.
  ViewerSetup.cs` is 283 lines with room for the wiring addition.
- I/O isolation: all new coverage-bearing logic (`CidImageResolver`, `ContentId` property read) is
  plain C#/interop-interface code with no disk or network access; only the already-exempt WebView2
  SDK glue touches the live control.
