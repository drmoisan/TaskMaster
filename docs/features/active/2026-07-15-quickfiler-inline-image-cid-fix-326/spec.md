# quickfiler-inline-image-cid-fix (Spec)

- **Issue:** #326
- **Parent (optional):** none
- **Owner:** drmoisan
- **Last Updated:** 2026-07-15T17-10
- **Status:** Draft
- **Version:** 0.2

## Context
In QuickFiler expanded mode, inline images referenced by `cid:` (Content-ID) do not render in the message body; they appear as broken or missing images. This is a child bug of the `folder-tree-percentage-ui` epic (manifest child 9004).

Environment:
- OS/version: Windows 11
- Python version: N/A (C# / .NET Framework 4.8 VSTO add-in)
- Command/flags used: QuickFiler reading pane, expanded viewer mode
- Data source or fixture: An email whose HTML body references inline images by `cid:` (Content-ID), with the images present as attachments carrying a matching `Content-Id` / `PR_ATTACH_CONTENT_ID`

Impact / Severity:
- [ ] Blocker
- [ ] High
- [x] Medium
- [ ] Low


## Repro & Evidence
Steps to Reproduce:
1. Open QuickFiler and select a message whose HTML body embeds inline images via `<img src="cid:...">`.
2. Enlarge the pane to expanded viewer mode so the body region is visible.
3. Observe the message body rendered in the WebView2 control.

Expected:
Inline images referenced by `cid:` render in the message body, resolved against the matching attachment `Content-Id` / `PR_ATTACH_CONTENT_ID`, in both compact and expanded modes.

Actual:
`cid:` references do not resolve. Inline images appear broken or missing. Because compact and expanded modes use the identical render call and differ only in the on-screen size of the WebView2 control, the images are cropped or not visible in compact mode and become visibly broken only when the pane is enlarged in expanded mode.

Logs / Screenshots:
- [ ] Attached minimal logs or screenshot
- Snippet: Body renders via a single `WebView2.NavigateToString(ItemHelper.Html)` call (`QuickFiler/Controllers/QfcItemController.EventWiring.cs` -> `UtilitiesCS/OutlookObjects/MailItem/MailItemHelper.Html.cs`). No `WebResourceRequested` handler or `SetVirtualHostNameToFolderMapping` mapping exists anywhere in the C# sources (verified by grep).


## Scope & Non-Goals
- In scope:
  - Adding `cid:` reference resolution to the shared body-render path (`MailItemHelper.Html.cs` `GetHtml()` / `GetHtml(string htmlBody)`) so inline images resolve against the matching attachment `Content-Id` / `PR_ATTACH_CONTENT_ID`.
  - Registering a `CoreWebView2.WebResourceRequested` handler and `AddWebResourceRequestedFilter` in `QfcItemController.ViewerSetup.cs`'s `InitializeWebViewAsync` to serve intercepted, rewritten-virtual-host image requests from in-memory attachment bytes.
  - Adding a `ContentId` member to `IAttachment` and populating it in `AttachmentSerializable`, using the same in-memory `PropertyAccessor.GetProperty` pattern already used for `PR_ATTACH_DATA_BIN`.
  - A new host-neutral file, `UtilitiesCS/OutlookObjects/MailItem/CidImageResolver.cs`, exposing pure functions `RewriteCidReferences(...)` and `BuildContentIdMap(...)`.
  - Both compact and expanded QuickFiler viewer modes, since they share the single `ItemViewer` instance and the single `NavigateToString(ItemHelper.Html)` call site.
- Out of scope / non-goals:
  - Changing the compact-mode render call path beyond the shared `cid:` resolution (no new compact-mode-specific code path).
  - `SetVirtualHostNameToFolderMapping`-based resolution (rejected — requires files on disk; `WebResourceRequested` is not fired for virtual-host-mapped requests per Microsoft Learn documentation).
  - Fixing the pre-existing, unrelated defect in `GetHtml(string htmlBody)` where the `htmlBody` parameter is ignored and `_item.HTMLBody` is re-read instead; that is out of scope and must not be touched as part of this fix.
  - Any change to `EfcViewer.cs`, `EfcViewer3.cs`, `CboFolders`, `FolderScorer`, `FolderPredictor`, the folder list, or the folder-selection ComboBox.
  - Any change to the dead/legacy `ItemViewerExpanded.cs`, `QfcItemViewer.cs`, or `QfcItemViewerExpanded.cs` classes; they are not constructed in production and are unaffected by this fix.
- Explicitly excluded systems, integrations, or datasets:
  - Non-inline (regular) attachments that do not carry `PidTagAttachContentId` — the fix only rewrites `cid:` references that match a resolved `Content-Id`; unmatched references are left untouched by design (see Proposed Fix).
  - Sibling epic features 9001 (`FolderScorer`/`FolderPredictor`), 9002 (`EfcViewer.cs`/`EfcViewer3.cs`, `FolderListBox`), and 9003 (`CboFolders`/QuickFiler viewer Designer variants) — confirmed by research to share no files with this fix.

## Root Cause Analysis
Root cause (verified against current code): the email body renders via a single `WebView2.NavigateToString(ItemHelper.Html)` call. No code resolves `cid:` references against attachment `Content-Id` / `PR_ATTACH_CONTENT_ID`; there is no `WebResourceRequested` handler or virtual host mapping. Expanded and compact modes use the identical rendering call, differing only in the WebView2 control's on-screen size (`TlpCellSnapShot.ApplyState`), so broken inline images are a missing-feature defect (`cid:` resolution never existed), not a mode-specific regression.

Files to inspect:
- `UtilitiesCS/OutlookObjects/MailItem/MailItemHelper.Html.cs` (HTML body producer)
- `QuickFiler/Controllers/QfcItemController.EventWiring.cs` (WebView2 init/wiring, `NavigateToString`)
- WebView2 setup/wiring for the item viewer


## Proposed Fix

### Design summary (what changes where):
The body-render call site (`QfcItemController.EventWiring.cs` -> `_itemViewer.NavigateToString(ItemHelper.Html)`) stays byte-for-byte unchanged. `cid:` resolution is inserted upstream of that call, inside `MailItemHelper.Html.cs`'s `GetHtml()`, which rewrites `cid:<id>` references in the raw HTML to a fetchable virtual-host URL (`https://cid.quickfiler.local/<id>`) whenever a matching attachment `Content-Id` is found. Because Chromium does not treat `cid:` as a fetchable scheme, the rewrite must happen before `NavigateToString`; the rewritten `https://` sub-resource requests are then intercepted by a `CoreWebView2.WebResourceRequested` handler (registered once per pooled `ItemViewer`, in `QfcItemController.ViewerSetup.cs`'s already-`[ExcludeFromCodeCoverage]` `InitializeWebViewAsync`), which serves the matching attachment's bytes from an in-memory `MemoryStream` via `CoreWebView2Environment.CreateWebResourceResponse`. Since both compact and expanded modes share the single `ItemViewer` and the single `NavigateToString` call, both modes receive the fix automatically with no mode-specific branching.

### Boundaries and invariants to preserve:
- `QfcItemController.EventWiring.cs`'s `_itemViewer.NavigateToString(ItemHelper.Html)` call signature and call site remain unchanged.
- `IItemViewer`'s public surface (`NavigateToString(string html)`, `WebViewInitializationCompleted`) is not widened; any code needing `CoreWebView2` continues to concrete-cast to `ItemViewer`, matching the existing pattern at `QfcItemController.ViewerSetup.cs:63`.
- `GetHtml(string htmlBody)`'s existing (pre-existing, out-of-scope) behavior of ignoring its parameter and re-reading `_item.HTMLBody` is left as-is; the `cid:` rewrite is added without altering that behavior.
- Unmatched `cid:` references (no attachment with a matching `Content-Id`) are left untouched in the HTML, so the `WebResourceRequested` handler never has to invent a 404 response for a reference the resolver silently dropped.
- The `WebResourceRequested` filter is scoped narrowly to the dedicated virtual host (`https://cid.quickfiler.local/*`, `CoreWebView2WebResourceContext.Image`) so it does not intercept unrelated navigation or sub-resource requests.
- `ItemViewer` instances are pooled and reused (`ItemViewerQueue.cs`); the `WebResourceRequested` registration happens once per `ItemViewer`'s WebView2 lifetime, and the handler closure reads the currently-loaded `ItemHelper` at request time (not at registration time) so it always resolves against whichever mail item is currently loaded into that pooled viewer.
- No disk I/O is introduced anywhere in the new code path (attachment bytes and `Content-Id` are read via in-memory `PropertyAccessor.GetProperty`; the intercepted response is served from a `MemoryStream`).

### Dependencies or blocked work:
- None. This fix does not depend on other epic children (9001-9003 confirmed to share no files) and is not blocked by other in-flight work.

### Implementation strategy (what changes, not sequencing):

#### Files/modules to change:
1. `UtilitiesCS/OutlookObjects/MailItem/MailItemHelper.Html.cs` — call `CidImageResolver.RewriteCidReferences(...)` inside `GetHtml()` / `GetHtml(string htmlBody)`, passing the lazily-loaded `AttachmentsHelper`/`AttachmentsInfo` member and a fixed virtual-host constant.
2. `UtilitiesCS/OutlookObjects/MailItem/CidImageResolver.cs` — **new file**, host-neutral pure logic: `RewriteCidReferences(string html, IReadOnlyCollection<IAttachment> attachments, string virtualHost)` and `BuildContentIdMap(IReadOnlyCollection<IAttachment> attachments)`.
3. `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs` — extend `InitializeWebViewAsync` (already `[ExcludeFromCodeCoverage]`) to call `AddWebResourceRequestedFilter("https://cid.quickfiler.local/*", CoreWebView2WebResourceContext.Image)` and register a `WebResourceRequested` handler once per `ItemViewer`.
4. `UtilitiesCS/Interfaces/IEmailIntelligence/IAttachment.cs` — add `string ContentId { get; set; }`.
5. `UtilitiesCS/OutlookObjects/Attachment/AttachmentSerializable.cs` — populate `ContentId` in the constructor using the `TryFromAccessor`-style in-memory `PropertyAccessor.GetProperty` read against `http://schemas.microsoft.com/mapi/proptag/0x3712001F`.

No changes are anticipated to `EfcViewer.cs`, `EfcViewer3.cs`, `CboFolders`, any `QfcItemViewer*` Designer file, `FolderScorer`, or `FolderPredictor`.

#### Functions/classes/CLI commands impacted:
- `MailItemHelper.Html.cs`: `GetHtml()`, `GetHtml(string htmlBody)`.
- New: `CidImageResolver.RewriteCidReferences(string html, IReadOnlyCollection<IAttachment> attachments, string virtualHost)`.
- New: `CidImageResolver.BuildContentIdMap(IReadOnlyCollection<IAttachment> attachments)`.
- `QfcItemController.ViewerSetup.cs`: `InitializeWebViewAsync` (extended, not replaced).
- `IAttachment`: new `ContentId` property.
- `AttachmentSerializable`: constructor, populating the new `ContentId` property.
- No CLI commands are impacted (VSTO add-in, no CLI surface).

#### Data flow and validation changes:
- Input: `_item.HTMLBody` (raw Outlook HTML) plus the mail item's attachment collection (`AttachmentsHelper`/`AttachmentsInfo`, each item exposing the new `ContentId` and existing `AttachmentData`).
- `GetHtml()` builds/consumes a `Content-Id -> IAttachment` map (`BuildContentIdMap`) and passes it (or the attachment collection) to `RewriteCidReferences`, which regex-matches `src=['"]cid:([^'"]+)['"]` (case-insensitive) and replaces only matched references with `https://cid.quickfiler.local/<url-encoded id>`.
- At WebView2 request time, the `WebResourceRequested` handler re-derives the `Content-Id -> IAttachment` map from the currently-loaded `ItemHelper`'s attachments (via `BuildContentIdMap`), looks up the requested path segment, and serves `attachment.AttachmentData` as a `MemoryStream`-backed response with the appropriate `Content-Type`.
- Validation: `Content-Id` reads are wrapped in try/catch (properties absent on most non-inline attachments throw `COMException`) and default to `null`/empty on failure, mirroring `AttachmentSerializable.TryFromAccessor`'s existing pattern. `RewriteCidReferences` does not throw on unmatched `cid:` references; it leaves them in place.

#### Error handling and logging updates:
- `Content-Id` property reads follow the existing `AttachmentSerializable` pattern: wrap `PropertyAccessor.GetProperty(...)` in try/catch, catching `System.Exception` (consistent with the existing `TryFromAccessor` catch clause) and defaulting to `null`/empty rather than propagating.
- `RewriteCidReferences` and `BuildContentIdMap` are pure functions with no I/O; they do not throw for missing matches (by design, per the Boundaries/invariants section) and require no additional logging.
- The `WebResourceRequested` handler (host-bound, `[ExcludeFromCodeCoverage]` glue) must not silently swallow an unexpected exception when reading `AttachmentData`; on lookup failure it should decline to handle the request (allow WebView2's default not-found behavior) rather than crash the render, consistent with fail-fast-but-non-fatal handling for this UI glue path.

#### Rollback/feature-flag considerations (if applicable):
- No feature flag is introduced; the fix is additive and narrowly scoped (adding resolution where none existed). Rollback is a straight revert of the five touched files listed above.
- If a regression is discovered post-merge, the `WebResourceRequested` registration in `InitializeWebViewAsync` can be removed without affecting the unchanged `NavigateToString` call path or any other QuickFiler behavior.

### Technical specifications (interfaces/contracts):

#### Inputs/outputs and formats:
- `RewriteCidReferences(string html, IReadOnlyCollection<IAttachment> attachments, string virtualHost) -> string`: input HTML string, a collection of `IAttachment` (each optionally carrying `ContentId`), and a virtual-host string (e.g. `cid.quickfiler.local`); returns the HTML string with matched `cid:` references replaced by `https://<virtualHost>/<url-encoded id>` and unmatched references left as-is.
- `BuildContentIdMap(IReadOnlyCollection<IAttachment> attachments) -> IReadOnlyDictionary<string, IAttachment>`: keyed by `ContentId`, case-insensitive (`StringComparer.OrdinalIgnoreCase`); attachments with a null/empty `ContentId` are excluded from the map.
- `IAttachment.ContentId`: `string`, nullable/empty when the underlying MAPI property is absent or unreadable.

#### Required configuration keys and defaults:
- No new configuration keys. The virtual-host value (`cid.quickfiler.local`) is a fixed internal constant shared between the HTML rewrite step and the `WebResourceRequested` filter registration; it is not user-configurable and carries no default-override surface.

#### Backward-compatibility expectations:
- `IAttachment.ContentId` is a new, additive property. It does not remove or change any existing `IAttachment` member (`AttachmentData`, `IsImage`, `PropertyAccessor`), so existing implementers and consumers of `IAttachment` are unaffected except that any hand-written (non-generated) implementations of `IAttachment` outside `AttachmentSerializable` would need to add the new member to keep compiling; a repo-wide grep for other `IAttachment` implementers must be part of implementation to confirm no other production implementer exists.
- `MailItemHelper.Html.cs`'s public `GetHtml()`/`GetHtml(string htmlBody)` signatures are unchanged; only their internal behavior gains the `cid:` rewrite step.
- `QfcItemController.EventWiring.cs`'s `NavigateToString(ItemHelper.Html)` call and `IItemViewer`'s public surface are unchanged, so no caller of either is affected.

#### Performance constraints (latency/throughput/memory):
- The `cid:` rewrite is a single regex pass over the HTML body per render, performed once when `GetHtml()` is invoked (already a lazily-computed, cached property per `MailItemHelper.Properties.cs`); no additional per-frame or per-render cost is introduced.
- Attachment bytes are read once per matched image, in-memory, and served via `MemoryStream`; no disk I/O or network calls are introduced, so no new latency or throughput risk beyond the existing in-memory `PropertyAccessor.GetProperty` cost already incurred for `PR_ATTACH_DATA_BIN`.

## Assumptions, Constraints, Dependencies
- Assumptions (environment, data, access):
  - Inline images are carried as `olByValue` attachments with a populated `PidTagAttachContentId` (`0x3712001F`) MAPI property; attachments without this property are correctly left unresolved (out of scope by design).
  - The WebView2 runtime in the deployed environment supports `WebResourceRequested` and `AddWebResourceRequestedFilter` (standard WebView2 SDK surface, already a dependency of the existing `NavigateToString`-based render path).
  - Only one `ItemViewer` class is constructed in production (`ItemViewerQueue.cs`'s `CreateProductionViewer`); the dead/legacy `ItemViewerExpanded`/`QfcItemViewer*` classes are not reachable and require no changes.
- Constraints (budget, performance, compatibility):
  - No temp files or disk I/O may be introduced anywhere in the new code or its tests (repo I/O-isolation policy).
  - New/modified files must stay at or under the 500-line repo limit (`MailItemHelper.Html.cs` at 209 lines and `QfcItemController.ViewerSetup.cs` at 283 lines both have headroom; `CidImageResolver.cs` is a new, small file).
  - `cid:` is not a Chromium-dispatchable scheme; the HTML must be rewritten to a fetchable virtual host before `NavigateToString`, since `WebResourceRequested` cannot intercept a raw `cid:` request.
- External dependencies (services, libraries, releases):
  - Microsoft WebView2 SDK (`CoreWebView2.WebResourceRequested`, `AddWebResourceRequestedFilter`, `CoreWebView2Environment.CreateWebResourceResponse`) — already a project dependency via the existing `NavigateToString` usage.
  - Microsoft Outlook Interop (`PropertyAccessor.GetProperty`) — already a project dependency, same mechanism already used for `PR_ATTACH_DATA_BIN`.

## Data / API / Config Impact
- User-facing or API changes:
  - User-facing: inline `cid:` images now render correctly in the QuickFiler reading pane (compact and expanded modes). No new UI controls, menus, or settings are added.
  - API: `IAttachment` gains an additive `ContentId` property (see Backward-compatibility above). No other public API changes.
- Data or migration considerations:
  - None. No persisted data, database schema, or on-disk format changes.
- Logging/telemetry updates (if any):
  - No new telemetry is required. Existing try/catch-and-default handling around the new `Content-Id` property read follows the established `AttachmentSerializable` pattern and does not add new log statements beyond what that pattern already does (none).
- Compatibility notes (CLI flags, config schemas, versioning):
  - No CLI flags or config schema changes. No versioning impact; this is an internal add-in behavior fix with no external contract change.

## Test Strategy
Seeded from issue:

- Add `cid:` reference resolution to the WebView2 body-rendering path so inline images render, via a `WebResourceRequested` handler and virtual-host rewrite that resolves `cid:` references against attachment `Content-Id` / `PR_ATTACH_CONTENT_ID`.
- [x] Unit coverage areas: `cid:` -> attachment resolution logic (host-neutral, injectable seam) — `CidImageResolver.RewriteCidReferences` and `CidImageResolver.BuildContentIdMap`.
- [x] Integration scenario to retest: expanded-mode body render with inline `cid:` images (manual, live WebView2 verification — see below).
- [x] Manual verification notes: confirm compact-mode render call path is unchanged beyond the shared `cid:` resolution; do not touch the folder list, ComboBox, or scoring code.

Regression tests to add or update (Bugfix Workflow — failing test first, then minimal fix):
- New test file/class targeting `CidImageResolver.RewriteCidReferences`:
  - Positive case: HTML containing `<img src="cid:logo1">` and a fake/plain `IAttachment` with `ContentId = "logo1"`, `AttachmentData = new byte[] {1,2,3}`, `virtualHost = "cid.quickfiler.local"`. Assert the output contains `src="https://cid.quickfiler.local/logo1"` and does not contain `cid:logo1`.
  - This test must be written and observed to fail (compile-fail against the not-yet-existing `CidImageResolver` type, or run-and-fail against a stub) before `CidImageResolver` is implemented, per the repo's mandatory Bugfix Workflow.
- New test for `CidImageResolver.BuildContentIdMap`: given a collection of `IAttachment` fakes with mixed populated/empty `ContentId` values, assert the returned map contains only entries with non-empty `ContentId`, keyed case-insensitively.
- New/extended test in the `AttachmentSerializable`/`PropertyAccessor` test suite (mirroring the existing `AttachmentSerializableTests.cs` pattern with `Mock<PropertyAccessor>`): assert `ContentId` is populated from a mocked `GetProperty("http://schemas.microsoft.com/mapi/proptag/0x3712001F")` call, and defaults to null/empty when the mocked accessor throws.
- Extend the existing `MailItemHelperCoreTests.cs` `GetHtml`-related tests (e.g. alongside `GetHtml_ShouldInjectEmailHeaderIntoBodyMarkup`) to assert that `GetHtml()` output contains the rewritten virtual-host URL when a mocked attachment collection with a matching `Content-Id` is present.

Edge cases and negative scenarios (invalid inputs, missing data, boundary values):
- `cid:` reference with no matching attachment `Content-Id`: HTML must be left unchanged for that reference (`RewriteCidReferences` negative test — `<img src="cid:unknown">` remains present in output).
- Attachment with an absent/unreadable `PidTagAttachContentId` property: `ContentId` must default to null/empty without throwing (mocked `PropertyAccessor` throws `COMException`, code catches and defaults).
- HTML with no `cid:` references at all: `RewriteCidReferences` must return the input unchanged.
- Multiple `cid:` references, some matched and some unmatched, in the same HTML body: only matched references are rewritten.
- Case-insensitive `Content-Id` matching: a `cid:LOGO1` reference must match an attachment with `ContentId = "logo1"` (per `BuildContentIdMap`'s `StringComparer.OrdinalIgnoreCase`).

Error handling and logging verification:
- Test that a mocked `PropertyAccessor.GetProperty` throwing for the Content-Id proptag results in `ContentId` defaulting to null/empty rather than propagating the exception (mirrors existing `AttachmentSerializable` catch-and-default coverage).

Coverage impact and targets for changed lines/modules:
- `CidImageResolver.cs` (new, host-neutral): target high line/branch coverage on the testable denominator, consistent with the general policy's requirement that new modules target >=90% (per CLAUDE.md) / >=85% line, >=75% branch (per repo-wide rules); this file has no COM/WebView2 dependency and no coverage exemption applies.
- `AttachmentSerializable.cs`'s new `ContentId`-populating code: covered via the existing `Mock<PropertyAccessor>` test pattern; no exemption applies (it is a plain in-memory read, same seam as the existing `PR_ATTACH_DATA_BIN` read).
- `MailItemHelper.Html.cs`'s modified `GetHtml()`/`GetHtml(string htmlBody)`: covered via the existing `Mock<InteropMailItem>`-based test pattern in `MailItemHelperCoreTests.cs`; no exemption applies.
- `QfcItemController.ViewerSetup.cs`'s `InitializeWebViewAsync` extension: already `[ExcludeFromCodeCoverage]` under the repo's ratified host-bound WebView2/COM exemption; the new `WebResourceRequested` registration and handler closure remain within that exemption, since they cannot be exercised without a live `CoreWebView2` instance.
- Repository-wide coverage floor (testable denominator) must not regress below the applicable threshold as a result of this change.

Toolchain commands to run (format -> lint -> type-check -> test):
1. `dotnet tool run csharpier .` (or `csharpier .` if installed globally)
2. `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
3. `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true`
4. `vstest.console.exe <test-assembly-paths> /EnableCodeCoverage`

Rerun the full loop from step 1 if any step fails or auto-fixes files; do not stop until all four steps pass in a single pass.

Manual validation steps (if required):
- Load a real email with inline `cid:`-referenced images (matching attachment `Content-Id`) into QuickFiler; confirm the image renders in expanded mode.
- Confirm the same message's compact-mode render (same `ItemViewer`/`NavigateToString` call, smaller on-screen size) shows the resolved image and exhibits no new defects.
- Confirm an email with an unmatched/unknown `cid:` reference (no attachment with a matching `Content-Id`) does not crash the render and leaves the broken-image placeholder behavior unchanged for that specific reference (expected, out-of-scope case).
- Confirm the folder list, ComboBox, and folder-scoring behavior in QuickFiler are visually and functionally unaffected.


## Acceptance Criteria
- [ ] `CidImageResolver.RewriteCidReferences` rewrites a `cid:<id>` reference to `https://cid.quickfiler.local/<id>` when a supplied `IAttachment` has a matching `ContentId`, verified by a passing MSTest test (e.g. `CidImageResolverTests.RewriteCidReferences_ShouldRewriteMatchedContentId`).
- [ ] `CidImageResolver.RewriteCidReferences` leaves an unmatched `cid:` reference (no attachment with a matching `ContentId`) unchanged in the output HTML, verified by a passing negative-case MSTest test.
- [ ] `CidImageResolver.BuildContentIdMap` returns a case-insensitive map keyed by `ContentId`, excluding attachments with a null/empty `ContentId`, verified by a passing MSTest test.
- [ ] `IAttachment.ContentId` is added as an additive property and populated by `AttachmentSerializable` from `PidTagAttachContentId` (`http://schemas.microsoft.com/mapi/proptag/0x3712001F`) via `PropertyAccessor.GetProperty`, with a try/catch default to null/empty on read failure, verified by a passing MSTest test using `Mock<PropertyAccessor>`.
- [ ] `MailItemHelper.Html.cs`'s `GetHtml()` invokes `CidImageResolver.RewriteCidReferences` and its output contains the rewritten virtual-host URL for a matched `cid:` reference, verified by an extended `MailItemHelperCoreTests.cs` test using `Mock<InteropMailItem>`.
- [ ] `QfcItemController.EventWiring.cs`'s `_itemViewer.NavigateToString(ItemHelper.Html)` call site and signature are unchanged (diff-verified against the pre-fix version).
- [ ] `QfcItemController.ViewerSetup.cs`'s `InitializeWebViewAsync` registers `AddWebResourceRequestedFilter` and a `WebResourceRequested` handler scoped to `https://cid.quickfiler.local/*` with `CoreWebView2WebResourceContext.Image`, confirmed by code review (host-bound glue, `[ExcludeFromCodeCoverage]`, not unit-testable).
- [ ] No changes are present in the diff to `EfcViewer.cs`, `EfcViewer3.cs`, `CboFolders`, any `QfcItemViewer*` Designer file, `FolderScorer`, or `FolderPredictor`.
- [ ] Manual verification confirms inline `cid:` images render correctly in a live QuickFiler expanded-mode reading pane and that compact mode (same call path) is unaffected beyond the shared resolution (documented in the PR description or a linked manual-test note, since this cannot be asserted in MSTest).
- [ ] Full toolchain pass completed in order — CSharpier, .NET analyzers, nullable/`TreatWarningsAsErrors` build, and `vstest.console.exe /EnableCodeCoverage` — with all four steps passing in a single pass.
- [ ] New/changed lines in `CidImageResolver.cs`, the `ContentId`-related change in `AttachmentSerializable.cs`, and the `GetHtml()` change in `MailItemHelper.Html.cs` do not reduce repository-wide line/branch coverage on the testable denominator below the applicable threshold.

## Risks & Mitigations
- Technical or operational risks:
  - The Chromium `cid:`-scheme-not-fetchable constraint is corroborated by symptom and general WebView2/Chromium behavior but is not confirmed by an exact citation in the documentation fetched during research; if incorrect, the virtual-host rewrite step may be unnecessary complexity or may not fully explain the observed defect.
  - `WebResourceRequested` registration must be scoped correctly (filter + `CoreWebView2WebResourceContext.Image`) to avoid intercepting unrelated WebView2 requests from the same pooled `ItemViewer`.
  - Since `ItemViewer` instances are pooled and reused across mail items, an incorrect (e.g. registration-time-bound rather than request-time-bound) resolution of "which mail item is loaded" could resolve `cid:` references against the wrong message's attachments.
  - Adding `ContentId` to `IAttachment` could require changes in any other production implementer of `IAttachment` besides `AttachmentSerializable`, if one exists and was not surfaced by research.
- Mitigations and rollbacks:
  - Treat the Chromium `cid:`-scheme constraint as verified-by-manual-render (see Test Strategy manual validation steps) rather than solely by documentation citation; if manual verification shows `cid:` is in fact intercepted directly, simplify the design by removing the rewrite step in a follow-up, without needing to revert the `Content-Id`/`IAttachment` plumbing.
  - Scope the `WebResourceRequested` filter narrowly to `https://cid.quickfiler.local/*` with `CoreWebView2WebResourceContext.Image` to minimize the chance of intercepting unrelated requests.
  - Read `ItemHelper`'s attachments inside the `WebResourceRequested` handler closure at request time (not at registration time), so the resolution always reflects the currently-loaded mail item in the pooled `ItemViewer`.
  - Before implementation, grep the repository for other `IAttachment` implementers to confirm `AttachmentSerializable` is the only production implementer; if others exist, add `ContentId` to each with the same try/catch-default pattern.
  - Rollback is a straight revert of the five touched files; no feature flag or migration is required.

## Rollout & Follow-up
- Release/rollout steps:
  - Ship as part of the normal QuickFiler/TaskMaster release process; no feature flag, migration, or staged rollout is required since the change is additive and narrowly scoped to the existing body-render path.
- Post-fix monitoring or clean-up tasks:
  - Manually verify inline-image rendering across a small sample of real messages with inline `cid:` images post-merge, since this scenario cannot be asserted by the automated MSTest suite.
  - Confirm no other production implementer of `IAttachment` was missed when adding `ContentId` (see Risks & Mitigations).
- Links: issue, PRs, related docs
  - Issue: https://github.com/drmoisan/TaskMaster/issues/326
  - Epic manifest: `docs/features/epics/folder-tree-percentage-ui/epic.md` (child 9004)
  - Research: `docs/features/active/2026-07-15-quickfiler-inline-image-cid-fix-326/research/research.2026-07-15T17-10.md`
