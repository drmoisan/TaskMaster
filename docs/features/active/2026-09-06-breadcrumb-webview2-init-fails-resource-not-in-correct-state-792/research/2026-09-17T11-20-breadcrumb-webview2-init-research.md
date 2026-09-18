# Breadcrumb WebView2 initialization fails with 0x8007139F — refreshed diagnosis (issue #792)

- **Issue:** #792
- **Date:** 2026-09-17
- **Status:** Research complete (revision 2). Both open questions are answered. One new causal finding,
  now labelled for provenance.
- **Method:** every citation below was derived against the **item worktree**
  `C:/Users/DanMoisan/repos/TaskMaster-wt/item-792` (HEAD `8d3ea6426`, which has merged current
  `origin/main` `e7cbb5722`) using Read/Grep on absolute paths. External SDK behavior was
  established from Microsoft Learn reference pages and is marked `[V-web]`.
- **Revision note.** Revision 1 of this artifact measured the session worktree
  `.../2026-09-12T10-15` (HEAD `2405a829d`), which does not contain issues #742 (`96ea96318`) or
  #743 (`cc236c8d2`, `bce810495`). Every line citation and line count below has been re-derived
  against item-792. Revision 1's "premise disagreements" section asserted the delegation brief was
  wrong; it was measuring a superseded tree and **is retracted**. See §1.
- **Tooling constraint:** the Bash tool is disabled in this session, so no `git log`, `git diff` or
  `pwsh` command was run. Line counts were derived by full-line regex match count (`^`), which
  equals the newline-terminated line count. No git archaeology was possible; where history would
  have been the natural instrument, in-tree test documentation is cited instead and labelled.

---

## 1. Retraction of revision 1's premise-disagreement section

**Revision 1 reported three "brief is wrong" corrections and one dangling link. All four are
withdrawn.** They were artifacts of measuring a stale checkout.

- The `EfcItemController.cs` and `QfcItemController.ViewerSetup.cs` citations in the delegation
  brief are **correct for the merged tree**. Revision 1's uniformly `-1` values were correct only
  for the pre-#742 tree, because #742 inserted a line above them.
- The line-count totals in the delegation brief (ViewerSetup 479, `QfcCollectionController` 2333,
  `EfcItemController` 1122) are **correct for the merged tree** and are reproduced in §3.
- **`spec.md`'s figures are stale, not correct.** `spec.md:89,103,104,155` were authored at
  `be6c1c3b3`, before #742/#743 landed, and carry the pre-merge values. They are being corrected to
  the merged-tree values by this item; no discrepancy remains to adjudicate.
- **The "dangling link at `spec.md:388`" finding is FALSE and is removed.** The research directory is
  not empty. `docs/features/active/2026-09-06-breadcrumb-webview2-init-fails-resource-not-in-correct-state-792/research/2026-09-12T10-30-breadcrumb-webview2-init-research.md`
  is tracked and present in item-792 (verified by directory enumeration). **Do not re-point that
  link.**

Everything else in the delegation brief verified exactly against item-792, including all
`TryReportBoundaryFault` citations, the sole `CoreInitialized` subscription, every
`WebView2BreadcrumbHost.cs` line, and all five Designer `CreationProperties` sites.

The one substantive addition to the brief is §5 (the #463 regression account), now carrying an
explicit provenance label.

---

## 2. Confirmed mechanism

`0x8007139F` is `HRESULT_FROM_WIN32(ERROR_INVALID_STATE)`. Microsoft documents it for WebView2
initialization as `[V-web]`:

> `HRESULT_FROM_WIN32(ERROR_INVALID_STATE)` — Specified options do not match the options of the
> WebViews that are currently running in the shared browser process.

and, on the `options` parameter of `CoreWebView2Environment.CreateAsync` `[V-web]`:

> As a browser process may be shared among WebViews, WebView creation fails if the specified
> `options` does not match the options of the WebViews that are currently running in the shared
> browser process.

The add-in has exactly three production sites that construct `CoreWebView2EnvironmentOptions`
(derivation under **Numeric Derivation Evidence** below). All three resolve the same user-data
folder, `%LOCALAPPDATA%\WindowsFormsWebView2`, but supply divergent additional browser arguments:

| Site | File:line (item-792) | Argument |
|---|---|---|
| 1 | `QuickFiler/Viewers/WebView2BreadcrumbHost.cs:250` | none — `var options = new CoreWebView2EnvironmentOptions();` |
| 2 | `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs:62` | inline literal — `CoreWebView2EnvironmentOptions options = new("--incognito ");` |
| 3 | `QuickFiler/Controllers/EfcItemController.cs:188-190` | `IncognitoArgument`, declared at `EfcItemController.cs:177` |

The user-data folder string `"WindowsFormsWebView2"` is likewise duplicated as an inline literal at
`EfcItemController.cs:182-185`, `QfcItemController.ViewerSetup.cs:56-59` and
`WebView2BreadcrumbHost.cs:246-249` rather than shared.

Site 3 bypasses the `IWebViewCoreInitializer` seam entirely, calling
`CoreWebView2Environment.CreateAsync(null, cacheFolder, options)` directly at
`EfcItemController.cs:195-199`. Sites 1 and 2 route through the seam
(`WebView2BreadcrumbHost.cs:265-268`, `QfcItemController.ViewerSetup.cs:71-74`), whose sole SDK
forward is `WebView2CoreInitializer.cs:72`.

Site 1 is the odd one out and is the only one that fails, which matches the logged symptom.

### 2.1 The QFC path already implements the intended contract

`QuickFiler/Controllers/QfcItemController.ViewerSetup.cs:110-123` reuses ONE environment for the
message-body pane, the ItemViewer breadcrumb control and the popup dropdown surface, with an
explicit comment at lines 110-112, quoted verbatim:

```
            // #351: initialize the breadcrumb WebView2 through the same injected seam and the
            // same CoreWebView2Environment/options object created above for the message-body
            // pane (G7); no second environment is negotiated against the user-data folder.
```

So the "one environment per user-data folder" discipline already exists in the codebase and is
documented; the EFC path simply does not follow it. The fix direction proposed in `spec.md`
generalises an existing in-repo convention rather than inventing one.

---

## 3. Measured line counts for the write set (item-792)

TOTAL lines (the convention the 500-line ceiling uses). Files over 500 are marked.

### Production

| File | Lines | Over 500? |
|---|---|---|
| `QuickFiler/Controllers/QfcCollectionController.cs` | **2333** | **YES** |
| `QuickFiler/Controllers/EfcFormController.cs` | **1321** | **YES** |
| `QuickFiler/Controllers/EfcItemController.cs` | **1122** | **YES** |
| `QuickFiler/Controllers/EfcDataModel.cs` | 499 | no (1 under) |
| `QuickFiler/Viewers/BreadcrumbPopupUiOperations.cs` | 489 | no |
| `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs` | **479** | no (21 headroom) |
| `QuickFiler/Viewers/ItemViewer.Breadcrumb.cs` | 460 | no |
| `QuickFiler/Controllers/EfcHomeController.cs` | 447 | no (53 headroom) |
| `QuickFiler/Controllers/BreadcrumbBridgeRouter.cs` | 407 | no |
| `QuickFiler/Viewers/WebView2BreadcrumbHost.cs` | 368 | no |
| `QuickFiler/Controllers/QfcItemController.cs` | 334 | no |
| `UtilitiesCS/OutlookObjects/Folder/BreadcrumbHtmlRenderer.cs` | 234 | no |
| `QuickFiler/Controllers/BreadcrumbBridgeRouter.Selection.cs` | 221 | no |
| `QuickFiler/Viewers/EfcViewer.cs` | 169 | no |
| `QuickFiler/Viewers/WebView2CoreInitializer.cs` | 103 | no |
| `QuickFiler/Helper Classes/EfcViewerQueue.cs` | 101 | no |
| `QuickFiler/Controllers/BreadcrumbOutboundQueue.cs` | 67 | no |
| `QuickFiler/Controllers/EfcItemController.WebViewFaultBoundary.cs` | 67 | no |
| `QuickFiler/Viewers/IWebViewCoreInitializer.cs` | 66 | no |
| `QuickFiler/Viewers/IBreadcrumbWebHost.cs` | 27 | no |

The three bolded values changed from revision 1; the remaining seventeen are unchanged and were
re-measured against item-792.

### Test files in or adjacent to the write set

| File | Lines | Headroom to 500 |
|---|---|---|
| `QuickFiler.Test/Controllers/EfcFormControllerTests.cs` | 485 | **15** |
| `QuickFiler.Test/Controllers/EfcItemControllerTests.cs` | 470 | 30 |
| `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterQueueTests.cs` | 462 | 38 |
| `QuickFiler.Test/Viewers/WebView2BreadcrumbHostTests.cs` | 440 | 60 |

**Planning consequence (unchanged and re-verified).** `spec.md` requires strengthening the existing
folder-combobox fault test inside `EfcFormControllerTests.cs`, which has only **15 lines** of
headroom. A strengthened test with an Arrange-Act-Assert body and a doc comment will not fit. That
file must either be split or the strengthened assertion must be placed in a new
`EfcFormControllerIssue792Tests.cs`. Neither `spec.md` nor the write set currently accounts for
this. Flagging it as a write-set gap, not a blocker.

`EfcDataModel.cs` at 499 has one line of headroom, which is why `spec.md` routes the carry work into
a new partial. Confirmed correct.

`QfcItemController.ViewerSetup.cs` at 479 now has only 21 lines of headroom (it had 33 before #742).
Any edit that routes site 2 through a shared contract must not grow that file by more than 21 lines.

---

## Numeric Derivation Evidence

This section supports the numeric assertion used by `spec.md` AC-U6: **exactly three production
`CoreWebView2EnvironmentOptions` construction sites**.

- **Complete Family:** WebView2BreadcrumbHost.cs, QfcItemController.ViewerSetup.cs, EfcItemController.cs
- **Exhaustive Search Scope:** the entire repository source tree, covering all tracked C# files in every project directory
- **Inclusion Rules:** a production C# file that constructs a CoreWebView2EnvironmentOptions instance and supplies it to WebView2 environment creation.
- **Exclusion Rules:** files in test projects; doc-comment references and parameter-type references that name the type without constructing it; interface declarations and seam parameter lists.
- **Primary Search Strategy or Query Expression:** enumerate by type name across the whole tree, matching both the explicit form and the target-typed form, then classify every hit by hand, which reaches WebView2BreadcrumbHost.cs, QfcItemController.ViewerSetup.cs and EfcItemController.cs.
- **Cross-check Search Strategy or Query Expression:** independently follow every call of CoreWebView2Environment.CreateAsync and of CreateEnvironmentAsync back to the options instance each receives, which reaches EfcItemController.cs, WebView2BreadcrumbHost.cs and QfcItemController.ViewerSetup.cs.
- **Primary Member Set:** WebView2BreadcrumbHost.cs, QfcItemController.ViewerSetup.cs, EfcItemController.cs
- **Cross-check Member Set:** EfcItemController.cs, WebView2BreadcrumbHost.cs, QfcItemController.ViewerSetup.cs
- **Primary Count:** 3
- **Cross-check Count:** 3
- **Member-set Comparison:** the primary and cross-check member sets are equal ignoring order and case.

### Supporting enumeration — primary (by type name, 12 raw hits in item-792)

| File:line | Classification |
|---|---|
| `QuickFiler/Viewers/WebView2BreadcrumbHost.cs:250` | **INCLUDE — construction, parameterless overload** |
| `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs:62` | **INCLUDE — construction, target-typed `new("--incognito ")`** |
| `QuickFiler/Controllers/EfcItemController.cs:188` | **INCLUDE — construction, `(string)` overload** |
| `QuickFiler/Controllers/EfcItemController.cs:187` | exclude — commented-out `--disk-cache-size=1` line |
| `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs:61` | exclude — commented-out `--disk-cache-size=1` line |
| `QuickFiler/Controllers/EfcItemController.cs:169` | exclude — `<see cref>` XML doc reference |
| `QuickFiler/Viewers/WebView2CoreInitializer.cs:37` | exclude — parameter declaration |
| `QuickFiler/Viewers/WebView2CoreInitializer.cs:69` | exclude — parameter declaration |
| `QuickFiler/Viewers/IWebViewCoreInitializer.cs:17` | exclude — XML doc reference |
| `QuickFiler/Viewers/IWebViewCoreInitializer.cs:51` | exclude — interface parameter declaration |
| `QuickFiler.Test/Viewers/WebView2BreadcrumbHostTests.cs:406` | exclude — test project, `It.IsAny<>` matcher |
| `QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part2.cs:254` | exclude — test project, `It.IsAny<>` matcher |

### Supporting enumeration — cross-check (by consumer, options-argument origin)

| Invocation site | `options` argument origin |
|---|---|
| `QuickFiler/Controllers/EfcItemController.cs:195` (`CoreWebView2Environment.CreateAsync`, seam bypassed) | `EfcItemController.cs:188` |
| `QuickFiler/Viewers/WebView2BreadcrumbHost.cs:265` (`_initializer.CreateEnvironmentAsync`) | `WebView2BreadcrumbHost.cs:250` |
| `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs:71` (`_webViewInitializer.CreateEnvironmentAsync`) | `QfcItemController.ViewerSetup.cs:62` |
| `QuickFiler/Viewers/WebView2CoreInitializer.cs:55,72` (`ForwardCreateEnvironmentAsync` -> SDK static) | **not an origin** — shared adapter; forwards the `options` parameter received from the two seam callers above |
| `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs:124` | exclude — commented out |

The adapter at `WebView2CoreInitializer.cs:72` is a forwarder, not an independent owner, so it
collapses onto its two callers and adds no member. Test-project invocations
(`WebView2BreadcrumbHostTests.cs:404`, `QfcItemController.InitializationTests.Part2.cs:252`,
`WebView2CoreInitializerTests.cs:43,70,129,136`) are excluded by the Exclusion Rules, as are the
three doc-comment references (`IWebViewCoreInitializer.cs:15,17`, `WebView2CoreInitializer.cs:23,58`).

The two strategies are structurally independent: the primary never names an environment-creation
API, and the cross-check never names the options type. A search restricted to either consumer API
alone would be non-exhaustive, because site 3 bypasses the seam and sites 1-2 never reference the
SDK static.

### Trap recorded for reviewers — AC-U6 vacuity

A source-text search for the construction syntax `new\s+CoreWebView2EnvironmentOptions` returns only
**2** of the 3 sites. It misses `QfcItemController.ViewerSetup.cs:62`, which uses the C# 9
target-typed form:

```csharp
            CoreWebView2EnvironmentOptions options = new("--incognito ");
```

Consequence: **an AC-U6 structural-parity test implemented as a search for the literal
`new CoreWebView2EnvironmentOptions` PASSES ON THE UNFIXED TREE.** It would find two sites, observe
that they agree, and report success while a divergent third site exists. Any such test must
enumerate by type name, not by `new` expression.

**The trap is worse at file granularity, and this is the form a reviewer is most likely to write.**
The orchestrator measured the same query counted per file rather than per line. It matches **all
three files**, because `QfcItemController.ViewerSetup.cs` contains the string on its commented-out
dead line 61:

```
EfcItemController.cs:187:            // CoreWebView2EnvironmentOptions options = new CoreWebView2EnvironmentOptions("--disk-cache-size=1 ");
EfcItemController.cs:188:            CoreWebView2EnvironmentOptions options = new CoreWebView2EnvironmentOptions(
QfcItemController.ViewerSetup.cs:61:            // CoreWebView2EnvironmentOptions options = new CoreWebView2EnvironmentOptions("--disk-cache-size=1 ");
WebView2BreadcrumbHost.cs:250:            var options = new CoreWebView2EnvironmentOptions();
```

So a file-level assertion of the form "three files construct environment options" reaches a count of
three **on the strength of a dead comment** and reports success with no fix present. That is the
second and more dangerous failure mode named in the OBSERVED-FAILING CRITERIA block: not a gate that
cannot pass, but one that passes for a reason unrelated to correctness. Any AC-U6 gate must
therefore (a) enumerate by type name, (b) exclude comment text, and (c) be observed failing on the
unfixed tree before it is trusted. The cross-check leg must likewise name both
`CoreWebView2Environment.CreateAsync` and `CreateEnvironmentAsync`; naming either alone is
non-exhaustive for the reason given above. Both queries were run and compared.

---

## 5. NEW FINDING — the conflict appears to have been introduced by issue #463's fix

This was not in the brief and it changes the regression narrative. **Provenance is labelled
explicitly below; part of this is inference, not assertion by the cited sources.**

### What the in-tree test documentation asserts (verbatim, item-792)

`QuickFiler.Test/Controllers/EfcItemControllerTests.cs:360-364`:

```csharp
        /// <summary>
        /// #463. The additional-browser-arguments literal opened with U+2013 EN DASH rather than two
        /// ASCII hyphen-minus characters, so Chromium silently ignored the unrecognised token and the
        /// preview WebView2 was never incognito.
        /// </summary>
```

`QuickFiler.Test/Controllers/EfcItemControllerTests.cs:89-93`:

```csharp
        /// <summary>
        /// #466 B, and the dead third site of #463. <c>InitializeWebView()</c> had zero call sites,
        /// so the EN DASH incognito literal it contained is removed with its container rather than
        /// edited in place.
        /// </summary>
```

### What is asserted versus what is inferred

- **ASSERTED by the sources.** That the additional-browser-arguments literal previously opened with
  U+2013 EN DASH; that Chromium silently ignored the unrecognised token; and that in consequence
  "the preview WebView2 was never incognito." These are first-party, contemporaneous statements in
  the doc comments quoted above. They are statements about the **EFC preview site**
  (`EfcItemController`) and about a now-deleted third site inside `InitializeWebView()`.
- **INFERRED by me, not stated by any source.** That this en-dash history is the cause of #792. The
  inference chain is: if the pre-#463 literal was discarded by Chromium, the preview environment's
  *effective* option set was empty; the breadcrumb host's option set is empty
  (`WebView2BreadcrumbHost.cs:250`); therefore all environments agreed and `ERROR_INVALID_STATE`
  could not arise; #463 made the literal a real Chromium switch, which is what first created the
  divergence. **This is my reconstruction. Label it as inference wherever it is repeated.**
- **NOT ESTABLISHED BY THIS AGENT.** Whether the QFC site (`QfcItemController.ViewerSetup.cs:62`)
  ever carried the en dash, and the dates of any of these changes. The cited doc comments name only
  the EFC sites. I could not run `git log`/`git blame` (Bash disabled), so no commit-level
  confirmation exists in this section. **See the subsection immediately below: the orchestrator
  subsequently supplied that confirmation, and it resolves this item.**

### Commit-level confirmation supplied by the orchestrator (resolves the item above)

The orchestrator ran the git archaeology this agent could not. Commit
`abb825d94827ce4c3f825e1f79ddaa407b5d48a0`, *"fix(efc-464): correct the WebView2 incognito argument
at both live sites (#463)"*, dated Thu Aug 27 2026, states in its own message:

> The additional-browser-arguments literal was "-incognito " with a leading U+2013 EN DASH rather
> than two ASCII hyphen-minus characters. Chromium introduces command-line switches with two ASCII
> hyphens and passes CoreWebView2EnvironmentOptions.AdditionalBrowserArguments through verbatim, so
> the unrecognised token was discarded silently and the item preview retained browsing data.

Its diff changes `"–incognito "` to `IncognitoArgument` in `EfcItemController.cs` and
`new("–incognito ")` to `new("--incognito ")` in `QfcItemController.ViewerSetup.cs`. The commit
message additionally records the byte comparison for the QFC site: `E2 80 93` becoming `2D 2D`.

This upgrades the provenance as follows:

- The QFC site **did** carry the en dash. The "NOT ESTABLISHED" item above is resolved in the
  affirmative by first-party commit evidence.
- Both live sites became real Chromium switches **for the first time** on 2026-08-27, and the
  breadcrumb host was not touched by that commit.
- The same commit records that the third site, inside the dead `InitializeWebView()`, was removed
  with its container rather than edited, which confirms three live sites today.

What remains inference is only the final causal step — that this divergence is the mechanism behind
the `ERROR_INVALID_STATE` report in #792. Every premise feeding that step is now first-party
confirmed, but the causal conclusion itself is still a reconstruction and must keep that label.

### What follows regardless of the inference

The measured present-day facts are independent of the history: the breadcrumb host constructs empty
options, and the other two sites each supply `"--incognito "`. The divergence is real and measured.

Two existing tests pin the corrected ASCII value — `EfcItemControllerTests.cs:371-396` asserts
`EfcItemController.IncognitoArgument == "--incognito "` and that every character is `<= 0x7F`, and
`EfcItemControllerTests.cs:94-...` asserts the dead `InitializeWebView` member is absent. Those
tests constrain the fix on their own terms, independent of whether my causal reconstruction is
right: **converging the third site onto `"--incognito "` satisfies them; reverting #463 would break
them.**

**Instruction, stated with the correct strength.** Do not revert #463 — not because the regression
narrative is proven, but because two existing green tests pin the ASCII value and because reverting
would reinstate a documented privacy defect (§6). The regression narrative is a supporting
explanation of *timing*, not the basis of the fix direction.

**Recommended confirmation for anyone with shell access:** `git log -S "incognito" -- QuickFiler/`
and `git log -S $'\u2013incognito' -- QuickFiler/`. Not required for the fix.

---

## 6. OPEN QUESTION 1 — does the breadcrumb document depend on persisted browsing storage?

**Answer: No. The evidence is sufficient and it selects direction (a) — converge on `--incognito `
at all three sites. The prerequisite verification task this decision depends on is hereby
discharged.**

### Evidence (re-verified against item-792)

The document is produced by `UtilitiesCS/OutlookObjects/Folder/BreadcrumbHtmlRenderer.cs`
(234 lines) and assembled at `BreadcrumbHtmlRenderer.cs:32-52`:

- Inline `<style>` from `BreadcrumbDocumentAssets.BaseCss` / `LightThemeCss` / `DarkThemeCss`.
- Row markup built entirely in-process from the `BreadcrumbRow` model.
- Inline `<script>` from `BreadcrumbDocumentAssets.BridgeJs`.

I read `UtilitiesCS/OutlookObjects/Folder/BreadcrumbDocumentAssets.cs` (147 lines) in full. The
emitted document contains:

- **No `localStorage`, `sessionStorage`, `indexedDB`, `document.cookie`, `navigator.storage` or
  `caches` usage.** A repository-wide case-insensitive search for
  `localStorage|sessionStorage|indexedDB|document\.cookie|navigator\.storage|caches\.|XMLHttpRequest|fetch\(`
  returned **zero hits in any production `.cs`, `.html`, `.css` or `.js` asset**. Every hit was in
  `docs/migration/**` planning prose, `.github/agents/**` sample code, or an unrelated Deedle
  `fetch` local variable in an archived research note.
- **No network-reachable resource.** No `<link>`, no `<script src>`, no `<img>`. CSS and JS are both
  inline string constants. The only external channel is `window.chrome.webview.postMessage` /
  `addEventListener('message', ...)` (`BreadcrumbDocumentAssets.cs:53,117-118`), which is the
  host-process bridge, not browser storage.
- **Delivery is `NavigateToString`**, never a `Source` assignment
  (`WebView2BreadcrumbHost.cs:157-179`, `BreadcrumbBridgeRouter.Selection.cs:173`,
  `BreadcrumbBridgeRouter.cs:324`). A `NavigateToString` document runs on an opaque origin, so even
  if it attempted storage access the data would not be durably keyed.
- **All state is re-pushed from .NET on every change.** `DeliverDocument()`
  (`BreadcrumbBridgeRouter.Selection.cs:168-180`) re-renders the whole document from `_rows`,
  `_darkMode` and `_selectedRowId`; `CommitSelection` and `PostRowRender`
  (`BreadcrumbBridgeRouter.Selection.cs:143-161`) re-render on every mutation. Theme is pushed by
  `ApplyTheme` (`BreadcrumbBridgeRouter.cs:310-314`). Nothing is read back from the page.

### Why this selects (a) converge on `--incognito ` rather than (b) remove it

The user-data folder is shared by all three sites, so the argument decision is **environment-scoped,
not control-scoped**. It governs the email preview panes as well as the breadcrumb. That asymmetry
is the whole argument:

- Adding `--incognito ` to site 1 costs nothing: the breadcrumb document provably has no persisted
  storage to lose.
- Removing `--incognito ` from sites 2 and 3 would newly persist browsing data generated by
  **untrusted email HTML** into `%LOCALAPPDATA%\WindowsFormsWebView2` — tracking-pixel cookies,
  remote-image cache entries, and any storage an attacker-authored message body chooses to write.
  The preview navigates arbitrary message bodies (`EfcItemController.cs` message-body navigation
  path; `QfcItemController.EventWiring.cs`; `QfcItemController.EventHandlers.cs`). Only
  `cid:`-rewritten inline images are intercepted and served from memory — the sole
  `AddWebResourceRequestedFilter` in the repository is at `QfcItemController.ViewerSetup.cs:87-90`,
  scoped to `https://{CidImageResolver.DefaultVirtualHost}/*` with
  `CoreWebView2WebResourceContext.Image`. Every other remote reference in a message body reaches the
  network normally. No repository code sets any blocking WebView2 setting (searched for
  `IsScriptEnabled`, `IsWebMessageEnabled`, `AreDefaultScriptDialogsEnabled`, `AreHostObjectsAllowed`,
  `IsBuiltInErrorPageEnabled`: zero production hits).
- The stated intent supports (a). `EfcItemController.cs:168-176`, verbatim:

```csharp
        /// <summary>
        /// The additional browser argument handed to <see cref="CoreWebView2EnvironmentOptions"/>
        /// so that the item preview keeps no browsing data.
        /// </summary>
        /// <remarks>
        /// Hoisted to a constant so the value has exactly one owner and can be asserted directly.
        /// A direct assertion is the only instrument available for it: the enclosing member needs
        /// the real WebView2 runtime, so it cannot be executed under the unit-test policy.
        /// </remarks>
```

Direction (b) would therefore fix the HRESULT at the cost of a privacy regression on the
highest-risk content in the add-in. Direction (a) fixes the HRESULT at no behavioral cost.
**`spec.md`'s settled decision is confirmed by audit and the prerequisite verification task it
depends on is discharged.**

### Residual caveat

`--incognito ` is a Chromium command-line switch, not the SDK's supported
`CoreWebView2CreationProperties.IsInPrivateModeEnabled` property. Its precise effect under WebView2
is not documented by Microsoft and was not verified empirically here. This does not affect the
convergence decision — the goal is that all three sites pass the *same* string, whatever it does —
but it means no test should assert a *behavioral* consequence of incognito mode. Assert string
equality across the three sites, which is what AC-U6 already specifies.

---

## 7. OPEN QUESTION 2 — SDK precedence for Designer-set `CoreWebView2CreationProperties`

**Answer: established from official documentation. The Designer-set properties are inert for the
environment in this codebase, but the reason is narrower and more fragile than "an explicit
environment was passed."**

### Where they are set (re-verified against item-792 — unchanged by #742/#743)

Five Designer files. I enumerated every `CreationProperties` occurrence under `QuickFiler/` and
confirmed **all six members are assigned `null` at each of the five sites**:

| File:lines | Control | Members set to `null` |
|---|---|---|
| `QuickFiler/Viewers/ItemViewer.Designer.cs:195-201` | `_l0vhBreadcrumb_WebView2` (QFC breadcrumb) | `AdditionalBrowserArguments`, `BrowserExecutableFolder`, `IsInPrivateModeEnabled`, `Language`, `ProfileName`, `UserDataFolder` |
| `QuickFiler/Viewers/ItemViewer.Designer.cs:241-247` | `_l0v2h2_WebView2` (QFC message body) | same six |
| `QuickFiler/Viewers/ItemViewerExpanded.Designer.cs:256-262` | `L0v2h2_WebView2` | same six |
| `QuickFiler/Viewers/QfcItemViewer.Designer.cs:847-853` | `L0v2h2_WebView2` | same six |
| `QuickFiler/Viewers/QfcItemViewerExpanded.Designer.cs:830-836` | `L0v2h2_Web` | same six |

Both the count of five and the "all six members null" claim are re-confirmed on the merged tree; the
Designer files did not move.

### Correction to the brief's framing (retained — re-verified)

The brief states the Designer sites are the ones AC-U4/#792 governs. They are not. The control that
`WebView2BreadcrumbHost` initializes on the **EFC** path is `_formViewer.BreadcrumbWebView`
(`EfcFormController.cs:1050`), which resolves to `EfcViewer.FolderListBox`:

```csharp
        internal Microsoft.Web.WebView2.WinForms.WebView2 BreadcrumbWebView => FolderListBox;
```
(`QuickFiler/Viewers/EfcViewer.cs:85`)

I searched `EfcViewer.Designer.cs` for every `FolderListBox` line — **14 hits**, at lines 50, 126,
880, 882-891 and 4250. They are construction, `Tlp.Controls.Add`, `BeginInit`/`EndInit`,
`SetColumnSpan`, `Dock`, `Location`, `Margin`, `Name`, `Size`, `TabIndex`, `ZoomFactor` and the
field declaration. **`FolderListBox` has no `CreationProperties` assignment and no `Source`
assignment.** The failing control in issue #792 is therefore not among the five Designer-configured
controls.

The five Designer sites remain relevant only because they sit on controls that share the same
user-data folder.

### What the SDK actually does `[V-web]`

`WebView2.CreationProperties` reference:

> Gets or sets a bag of options which are used during initialization of the control's CoreWebView2.
> This property cannot be modified (an exception will be thrown) after initialization of the
> control's CoreWebView2 has started.

Two things follow. First, **setting the property does not itself begin initialization** — otherwise
the Designer could not set six members in sequence. Second, it is "used during initialization",
which leaves precedence unresolved on its own.

`WebView2.EnsureCoreWebView2Async` Remarks resolves it:

> Unless previous initialization has already failed, calling this method with a different
> environment after initialization has begun will result in an `ArgumentException`. For example,
> this can happen if you begin initialization by setting the `Source` property and then call this
> method with a new environment, **if you begin initialization with `CreationProperties` and then
> call this method with a new environment**, or if you begin initialization with one environment and
> then call this method with no environment specified.

and, on the two-argument overload:

> If you pass a `controllerOptions` to this method then it will override any settings specified on
> the `CreationProperties` property.

**Derived rule.** `CreationProperties` is consumed only when it is the *origin* of initialization —
that is, when initialization begins implicitly, or when `EnsureCoreWebView2Async` is called with a
null environment. When an explicit non-null environment is the first thing to begin initialization
for a control, `CreationProperties` is not consulted for the environment. The documented failure
mode is not "silently wins" — it is a hard `ArgumentException`.

### Application to this codebase

The Designer-set properties are inert here, for two independent reasons:

1. **Ordering.** Every production initialization passes an explicit non-null environment
   (`WebView2BreadcrumbHost.cs:269`, `QfcItemController.ViewerSetup.cs:75-78` and `:114-117`,
   `EfcItemController.cs:208`, `BreadcrumbPopupUiOperations.cs` popup path), and nothing begins
   initialization earlier. I searched `QuickFiler/**/*.cs` for `\.Source\s*=`, `Navigate(` and
   `NavigateToString`: there is **no `Source` assignment anywhere**, and every `NavigateToString`
   call is on a post-initialization path.
2. **Value.** Even if consulted, all six members are `null` at every one of the five sites, which is
   equivalent to supplying no creation properties. They cannot contribute a divergent additional
   browser argument. They are therefore not a fourth divergent site and are not part of the #792
   root cause.

**Confidence: established, with one named residual.** Reason 2 is unconditional and measured, so the
conclusion that the Designer properties do not contribute a divergent argument is firm. Reason 1
depends on ordering, and one behavior I could not establish from documentation is whether the
WinForms `WebView2` control begins implicit initialization on handle creation in any circumstance
other than a `Source` assignment. If it ever did, `EnsureCoreWebView2Async(env)` would throw
`ArgumentException` rather than produce a wrong environment — a different, louder failure than
#792's HRESULT, and one not present in the logs. **No verification task is required for the #792
fix.** If a future change introduces a `Source` assignment or a design-time-set `Source` on any of
these controls, this analysis must be redone.

---

## 8. Defect mechanism 2 — one defect, ONE trigger

Verified against item-792 exactly as briefed. Stated precisely:

- `BreadcrumbBridgeRouter.Selection.cs:168-180` (`DeliverDocument()`) stashes the rendered document
  into `_pendingDocument` when `_host.IsCoreInitialized` is false (`Selection.cs:171,178`).
- `BreadcrumbBridgeRouter.cs:320-329` (`NotifyCoreInitialized()`) is the **only** drain of
  `_pendingDocument` (lines 322-326) **and** it calls `_outboundQueue.OnInitializationCompleted()`
  at line 328.
- `BreadcrumbOutboundQueue.OnInitializationCompleted()` at `BreadcrumbOutboundQueue.cs:59-65`
  **does drain the outbound buffer fully, FIFO, in a `while` loop.** The outbound queue is **not**
  "never drained". Do not write that. Verbatim:

```csharp
        public void OnInitializationCompleted()
        {
            while (_pending.Count > 0)
            {
                _host.PostMessageJson(_pending.Dequeue());
            }
        }
```

- Both drains share ONE trigger: the sole repo-wide `CoreInitialized` subscription at
  `EfcFormController.cs:1064`. I enumerated every `CoreInitialized` occurrence in the repository;
  the only production subscription is that line, the only declaration is
  `WebView2BreadcrumbHost.cs:146`, and the only raise is `WebView2BreadcrumbHost.cs:353`.
- That event is raised only on the success path. On failure,
  `WebView2BreadcrumbHost.cs:335-342` logs and `return`s at line 341 — no event, no retry, no error
  document, and `_isCoreInitialized` is never published (the `Volatile.Write` at line 352 is after
  the early return). `InitializeAsync` at `WebView2BreadcrumbHost.cs:239-270` contains no retry loop.

**Therefore: the trigger never fires on the failure path, stranding both buffers.** One defect, one
trigger, two stranded buffers. Not two defects.

Corollary for AC-U2/AC-U7 design: a failure notification must be a *sibling* entry point to
`NotifyCoreInitialized`, not a modification of it, because `spec.md` requires that a later
successful initialization still navigate a stash produced before a failure. The success path at
`BreadcrumbBridgeRouter.cs:322-326` must remain intact.

---

## 9. Defect mechanism 3 — failure surfacing is split

Verified against item-792; all citations re-derived. **One correction to revision 1's own
arithmetic:** `TryReportBoundaryFault` has **eight** call sites, not seven.

- `EfcFormController.TryReportBoundaryFault` is defined at `EfcFormController.cs:150-168`.
- Its eight call sites are lines **556, 573, 591, 653, 668, 1015, 1127, 1270**. Enumerated
  exhaustively; the only other repository occurrences are the definition, the `spec.md`/promoted-doc
  AC text, one line inside an archived `.cobertura.xml`, and two test doc comments in
  `EfcFormControllerTests.Part2.cs`.
- `PopulateFolderCombobox` correctly calls it at `EfcFormController.cs:1270`. **That half of AC-U4
  is already satisfied**; the work there is to strengthen the existing test, not re-implement. See
  the §3 headroom warning about where that strengthened test can live, and §11 for why it must not
  be presented as an observed-failing criterion.
- `InitializeBreadcrumbHostAsync` at `EfcFormController.cs:1072-1082` is log-only at line 1080:

```csharp
                logger.Error($"Breadcrumb WebView2 initialization failed: {ex.Message}", ex);
```

The default sink does reach the user: `DefaultBoundaryErrorSink` at `EfcFormController.cs:137-141`
logs and then invokes `UserFaultNotifier`. So routing `InitializeBreadcrumbHostAsync` through
`TryReportBoundaryFault` is sufficient to satisfy the "to the user, not log-only" clause with no new
notification surface.

---

## 10. Adjacent context — issue #726

Verified against item-792. `EfcItemController.cs:158` routes through
`InitializeWebViewGuardedAsync()`, defined at `EfcItemController.WebViewFaultBoundary.cs:25-42`,
which catches faults rather than letting a discarded task be silently finalized. The call site,
verbatim at `EfcItemController.cs:156-158`:

```csharp
            // Issue #726 finding 4: routed through the guarded wrapper so a fault is logged
            // instead of silently finalized away as an unobserved discarded-task exception.
            _ = InitializeWebViewGuardedAsync();
```

Its sink `WebViewInitializationErrorSink` (`EfcItemController.WebViewFaultBoundary.cs:14-15`)
defaults to `logger.Error` with **no user-visible path**.

Note carefully, because an earlier draft got this wrong: the doc comment at
`EfcItemController.WebViewFaultBoundary.cs:8-13` disclaims a shared contract **for that error
sink only** — "Named distinctly so no shared contract with the QFC sink or with
`EfcFormController.BoundaryErrorSink` is implied." It says nothing about `IncognitoArgument`.

The doc on `IncognitoArgument` at `EfcItemController.cs:168-176` (quoted verbatim in §6) reads the
opposite way — a **single-owner** rationale: "Hoisted to a constant so the value has exactly one
owner and can be asserted directly." **There is no anti-contract on the incognito argument.**
Hoisting it further, into a shared `WebView2EnvironmentContract`, extends the documented intent
rather than contradicting it. `spec.md`'s invariant applies to the fault sink, not to the argument.

---

## 11. Test strategy implications

No test code is proposed here. Strategy notes only, consistent with repository policy.

### Observed-failing obligation

[BLOCK: OBSERVED-FAILING CRITERIA — propagate verbatim]
Every acceptance criterion used as a gate must be OBSERVED FAILING on the unfixed tree before it is accepted, and the plan must include running it against the unfixed tree to demonstrate that. A criterion never seen failing is unproven. Two failure modes: one that cannot be satisfied at all, and one satisfied with no fix present — the second is more dangerous because it reports success. Prove non-vacuity by mutation where practical, with each mutation failing on its pre-predicted assertion.
[END BLOCK]

Per-criterion notes on what "observed failing" means here:

- **AC-U6 is the criterion most at risk of being vacuously satisfied.** A structural-parity test
  written as a source search for `new CoreWebView2EnvironmentOptions` **passes on the unfixed tree**
  for the wrong reason: it matches only 2 of the 3 sites, because `ViewerSetup.cs:62` uses the
  target-typed `new(...)` form (see the trap subsection under **Numeric Derivation Evidence**). The
  predicted pre-fix failure must be "three sites found, arguments differ", and the test must
  enumerate by type name. Its cross-check must name both `CoreWebView2Environment.CreateAsync` and
  `CreateEnvironmentAsync`. Mutation: change one site's argument string and predict the parity
  assertion fails.
- **AC-U4's `PopulateFolderCombobox` half cannot be observed failing** — it is already satisfied at
  `EfcFormController.cs:1270`. The honest statement is that the *test* is strengthened, and its
  non-vacuity must be proven by mutation: remove the `TryReportBoundaryFault` call at line 1270 and
  predict the new sink assertion fails. The current test, which asserts only "logs once and does not
  fault", would survive that mutation — that is precisely why it needs strengthening. **Do not
  present this half as an observed-failing criterion.**
- **AC-U1/AC-U2/AC-U7** can all be observed failing today at the router/queue level using the
  existing `Mock<IBreadcrumbWebHost>` with a settable `IsCoreInitialized`, the pattern already used
  in `BreadcrumbBridgeRouterQueueTests.cs`. Pre-fix, no failure-notification entry point exists, so
  the tests fail to compile or fail on a missing member — note that a compile failure is a weaker
  signal than an assertion failure; prefer shaping the test so it fails on an assertion about
  `PendingCount` and `_pendingDocument` state once the member exists.
- **AC-U5 is a human-executed live-Outlook verification and is NOT automatable.** It must never be
  described as an automated gate, must not be represented by a `[TestMethod]`, and must not be
  counted toward any coverage or gate-pass figure. Its runbook must start QuickFiler before the EFC
  open so that an `--incognito` WebView is already running in the shared browser process.

### Determinism

[BLOCK: TEST PARALLELISM — propagate verbatim]
Tests must ALWAYS run in parallel. `scripts/vscode/TaskMaster.cli.runsettings` with Workers=0 and Scope=ClassLevel is correct and must remain byte-identical. Never fix a parallel-execution failure with Workers=1, removing Parallelize, [DoNotParallelize], dropping /Settings:, retries, timing tolerance, or sleeps. If a test needs a distinct thread, create and join a dedicated Thread so the property is controlled, not assumed — see issue #900 for the working pattern.
[END BLOCK]

Specific hazard for this item: `EfcFormController._userFaultNotifier` is an
`AsyncLocal<System.Action<string>>` (`EfcFormController.cs:170-174`), and the in-code comment at
lines 170-172 states it is written that way precisely because "a shared static races under the
ClassLevel parallelization configured in the CLI runsettings." Any new test that substitutes the
boundary sink (`BoundaryErrorSink`) or the fault notifier (`UserFaultNotifier`,
`EfcFormController.cs:181-185`) must preserve that per-async-flow discipline and restore the
previous value, as `EfcItemControllerTests.cs:354-357` already does for `SynchronizationContext`.
The retry required by AC-U1 must use an injected delegate with a no-op substitute, never
`Task.Delay` or `Thread.Sleep` (both are in `BannedSymbols.txt`).

### Seam availability

- Sites 1 and 2 are already mockable through `IWebViewCoreInitializer`. Site 3
  (`EfcItemController.cs:195-199`) calls the SDK static directly and must be routed through the seam
  to be assertable, as `spec.md` anticipates.
- Real `WebView2` controls are constructible in `QuickFiler.Test` without the Evergreen runtime; the
  barrier is core initialization, not control construction. Do not justify a coverage exemption on
  construction grounds.
- `WebView2BreadcrumbHost.OnCoreInitializationCompleted` (`WebView2BreadcrumbHost.cs:329-354`) is
  `[ExcludeFromCodeCoverage]` for a real reason stated at `WebView2BreadcrumbHost.cs:327`:
  `CoreWebView2InitializationCompletedEventArgs` has no public constructor, "so a unit test cannot
  invoke it with a valid argument." Retry and error surfacing must therefore live on the
  awaited-task path in `EfcFormController`, not in that handler.
- `QfcItemController.InitializeWebViewAsync` (`QfcItemController.ViewerSetup.cs:49`) is likewise
  `[ExcludeFromCodeCoverage]` (attribute at line 48, rationale at lines 44-47, tracked under #230).
  Changes to site 2 will not move coverage; do not plan coverage deltas there.

---

## 12. Residual unknowns

Stated plainly rather than guessed.

1. **Shared-browser-process teardown.** Whether closing every QuickFiler and EFC viewer releases the
   shared browser process is not established from the code and was not verified. It affects only the
   AC-U5 pre-fix repro recipe, not the fix. Mitigated by opening QuickFiler first in the runbook.
2. **`--incognito` semantics under WebView2.** Not documented by Microsoft as a supported option and
   not empirically verified. Assert string equality across sites, never a behavioral consequence.
3. **Commit history for the #463 en-dash correction.** Not verifiable in this session (Bash
   disabled). §5's causal chain is explicitly labelled as inference from in-tree test documentation
   plus measured current state. The fix direction does not depend on it.
4. **Whether the QFC site ever carried the en dash.** Not established; the cited doc comments name
   only the EFC sites.
5. **Pop-out continuation thread affinity.** Still unverified; the AC-U3 UI-thread clause remains
   defence-in-depth rather than a reproduced failure.

---

## 13. Recommended approach

**Converge all three sites on a shared owner supplying `--incognito ` and the
`WindowsFormsWebView2` folder**, exactly as `spec.md` proposes. Question 1 is discharged in favour
of this direction (§6); Question 2 confirms the Designer properties do not obstruct it (§7).

Concretely: introduce one `internal static class WebView2EnvironmentContract` holding the argument
string and the user-data-folder name, have all three sites read from it, and route
`EfcItemController.InitializeWebViewAsync` through `IWebViewCoreInitializer` so the third site
becomes assertable. Mind the 21-line headroom in `QfcItemController.ViewerSetup.cs` (§3).

**Rejected alternative:** removing the additional browser argument at all three sites. It resolves
the HRESULT equally well and is a smaller diff, but it newly persists cookies, cache and
remote-image state generated by untrusted email HTML into a shared on-disk profile, contradicts the
documented intent at `EfcItemController.cs:168-171`, and would require reverting or rewriting the
two existing tests that pin the ASCII `--incognito ` value from issue #463. Rejected on
privacy-regression grounds.

---

## 14. Constraint blocks — propagate verbatim to any downstream agent

[BLOCK: OBSERVED-FAILING CRITERIA — propagate verbatim]
Every acceptance criterion used as a gate must be OBSERVED FAILING on the unfixed tree before it is accepted, and the plan must include running it against the unfixed tree to demonstrate that. A criterion never seen failing is unproven. Two failure modes: one that cannot be satisfied at all, and one satisfied with no fix present — the second is more dangerous because it reports success. Prove non-vacuity by mutation where practical, with each mutation failing on its pre-predicted assertion.
[END BLOCK]

[BLOCK: TEST PARALLELISM — propagate verbatim]
Tests must ALWAYS run in parallel. `scripts/vscode/TaskMaster.cli.runsettings` with Workers=0 and Scope=ClassLevel is correct and must remain byte-identical. Never fix a parallel-execution failure with Workers=1, removing Parallelize, [DoNotParallelize], dropping /Settings:, retries, timing tolerance, or sleeps. If a test needs a distinct thread, create and join a dedicated Thread so the property is controlled, not assumed — see issue #900 for the working pattern.
[END BLOCK]

[BLOCK: DIFF BASES — propagate verbatim]
Anchor every diff base, merge base and change-footprint gate to `origin/main` after a fetch, never to bare local `main`. Only the pulling checkout advances local `main`, so it is almost always stale and any gate anchored to it is unsatisfiable by construction. Three-dot does not rescue it: when the pinned ref is an ancestor of HEAD, `merge-base(PINNED,HEAD)` IS the pinned SHA. Run both forms and compare.
[END BLOCK]

[BLOCK: SCOPE AND COMMIT — propagate verbatim]
Commit your work to the item worktree branch before ending. Never write to another item's canonical state — if a hook can only be satisfied by modifying a file another item owns, STOP AND REPORT. Do not write to `.claude/agent-memory` in a session worktree you do not own; note that `Set-Location` does not update .NET's `CurrentDirectory`, so `System.IO` calls with relative paths can escape the worktree.
[END BLOCK]

[BLOCK: BASH DISCIPLINE — propagate verbatim]
Settings allow only `git *`, `pwsh *`, `poetry run *` and three lib scripts; every chained segment must match, so `cd X && ...`, grep, sed and cat via Bash will prompt. Use `git -C` and the Read/Grep/Glob tools. Gated-command hooks scan the WHOLE command string, so prose merely naming a gated tool inside a commit message or a `-b` body trips the block — write text to a file and use `-F body=@file`. Never chain a state write with a gated command.
[END BLOCK]
