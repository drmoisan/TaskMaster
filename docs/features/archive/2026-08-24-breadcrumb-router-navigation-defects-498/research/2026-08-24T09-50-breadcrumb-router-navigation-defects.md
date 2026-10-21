# Research — breadcrumb-router-navigation-defects (#439, #440, #498, #499)

- Timestamp: 2026-08-24T09-50
- Worktree: `<repo-root>`
- Verified against HEAD `988e819b`
- Scope: one feature closing four pre-existing bug issues in the breadcrumb bridge router and folder navigation.

Every code claim below was read at HEAD in this worktree. Where a potential document's citation is wrong,
the correction is stated explicitly. Anything I could not establish is marked **unverified**.

---

## 0. Corrections to the authoritative potential documents (read these first)

| Potential | Claim | Verdict at HEAD |
|---|---|---|
| #498 | `QuickFiler/Controllers/BreadcrumbRow.cs:111-118` | **Wrong path, correct lines.** The file is `UtilitiesCS/OutlookObjects/Folder/BreadcrumbRow.cs`; the throw is at `:111-118`. |
| #498 | `QuickFiler/Controllers/BreadcrumbMessageCodec.cs:100`, `:103-106`, `:142-158` | **Wrong path, correct lines.** File is `UtilitiesCS/OutlookObjects/Folder/BreadcrumbMessageCodec.cs`; `OptionalInt` call `:100`, presence gate `:103-106`, `OptionalInt` body `:142-158`. |
| #498 | Router unguarded call `:169`, catch `:193` | **Correct.** `BreadcrumbBridgeRouter.cs:169` and `:193`. |
| #499 | `:114`, `:58`, `:372`, `:399` | **All correct.** |
| #499 | `EfcFormController.cs:289-294`, `:873-883` | **Correct** (the `SelectedFolder` property block actually spans `:289-295`). |
| #439 | `FolderPredictor.cs:894-930` LoopFolders, `:934` GetOlSubpath, `:804` AddSuggestions | **Off by a few lines.** `LoopFolders` is `:883-931`, `folderStem` assignment `:898`, `matchingFolders.Add(folderStem)` `:919`; `GetOlSubpath` is `:933-951`; `AddSuggestions` is `:804-808`. |
| #439 | `OutlookFolderHierarchyProvider.cs:52-71` with comment at `:64-65` | **Correct.** |
| #439 | `BreadcrumbBridgeRouter.cs:333-352` FetchChainAsync | **Slightly wrong.** The method spans `:334-362`; the `key == null -> return null` branch is `:345-348`. |
| #439 | `BreadcrumbRowBuilder.cs:119-129` fallback | **Correct** (`:119-129`, fallback segment constructed at `:125-129`). |
| #439 | `FolderBreadcrumb.html:250-257` segment dblclick; `:258-261` arrow separator `→` | **Correct lines, WRONG SURFACE.** See §3f — `FolderBreadcrumb.html` is the **Qfc** document. The **Efc** surface (the surface #439 reports) generates its document from `BreadcrumbDocumentAssets` and its separator is `&gt;`, not `→`. |
| #440 | `BreadcrumbBridgeRouter.cs:225-250` HandleArrowKeyAsync | **Method spans `:225-260`**; the Right/Left cases are `:229-249`. |
| #440 | `BreadcrumbRow.cs:195-216` LeftArrow | **Correct.** |
| #440 | `FolderBreadcrumbBridgeRouter.cs:385-386` | **Correct** (inside `ArrowAsync`, `:378-406`). |
| #440 | `BreadcrumbStateModel.cs:424-455` | **Correct** (`RightArrow` `:424-437`, `LeftArrow` `:443-455`). |
| #440 | `FolderBreadcrumb.html:395-404` onArrow | **Correct.** |
| #440 | `KeyboardHandler.cs:288-314` | **Method spans `:288-315`.** |

---

## Q1 — #498 out-of-range `segmentIndex` host crash

### Q1a. The three cited sites, verified

**Unguarded call — `QuickFiler/Controllers/BreadcrumbBridgeRouter.cs:166-174`:**

```csharp
switch (message.Type)
{
    case BreadcrumbMessageTypes.SegmentDoubleClick:
        if (row.CollapseAfter(message.SegmentIndex!.Value))
        {
            PostRowRender(row);
        }

        break;
```

The `!` at `:169` asserts only that the codec validated *presence* (`BreadcrumbMessageCodec.cs:103-106`).

**The throw — `UtilitiesCS/OutlookObjects/Folder/BreadcrumbRow.cs:104-118`:**

```csharp
public bool CollapseAfter(int segmentIndex)
{
    if (Kind != BreadcrumbRowKind.Suggestion)
    {
        return false; // Banner/pseudo rows never collapse.
    }

    if (segmentIndex < 0 || segmentIndex >= _segments.Count)
    {
        throw new ArgumentOutOfRangeException(
            nameof(segmentIndex),
            segmentIndex,
            $"Segment index must be within [0, {_segments.Count - 1}] for row '{RowId}'."
        );
    }
```

Note the ordering: a banner or trash row returns `false` **before** the range check
(`BreadcrumbRow.cs:106-109`), so only suggestion rows can throw.

**The narrow catch — `BreadcrumbBridgeRouter.cs:187-198`:**

```csharp
private async void OnHostMessageReceived(object? sender, string json)
{
    try
    {
        await ProcessInboundAsync(json);
    }
    catch (BreadcrumbMessageException)
    {
        // Boundary: the codec already logged the specific malformed-payload error; the
        // router state is unchanged and the UI message pump must not be crashed.
    }
}
```

### Q1b. `async void` confirmed; enumeration of unvalidated fields

- `OnHostMessageReceived` **is** `async void` today (`BreadcrumbBridgeRouter.cs:187`). It is subscribed in
  the constructor at `BreadcrumbBridgeRouter.cs:54` (`_host.MessageReceived += OnHostMessageReceived;`)
  against `IBreadcrumbWebHost.MessageReceived` (`QuickFiler/Viewers/IBreadcrumbWebHost.cs:22`).
- The codec produces exactly four inbound fields (`BreadcrumbMessageCodec.cs:93-113`):

| Field | Codec validation | Where it lands | Crash risk |
|---|---|---|---|
| `type` (string) | required + enum membership (`:93-97`, `:116-122`) | `switch` at router `:166` | none (unmatched types fall out of the switch) |
| `rowId` (string) | required, must be a JSON string (`:99`, `:124-140`) | `FindRow` (`:159`, `:411-422`); unknown id is a logged no-op (`:160-164`) | none |
| `segmentIndex` (int?) | **type only** — `OptionalInt` (`:100`, `:142-158`) checks `JTokenType.Integer` and nothing else; presence is required only for `segmentDoubleClick` (`:103-106`) | `row.CollapseAfter(...)` at router `:169` | **CRASH** — `ArgumentOutOfRangeException` |
| `key` (string?) | non-empty for `arrowKey` only (`:101`, `:108-111`) | `HandleArrowKeyAsync` (`:179`, `:225-260`) | none — an unknown key hits the `default:` branch and is logged (`:256-258`) |

So `segmentIndex` is the **only** codec-validated-presence-only value that reaches a throwing member.
There is no second instance of this defect class on the Efc surface.

Two adjacent notes:

- `ExpandLeafAsync` already has a broad `catch (Exception ex)` at `BreadcrumbBridgeRouter.cs:324-331`, so the
  provider path is contained; only the `SegmentDoubleClick` case is not.
- The **Qfc** router is not exposed to this class of failure: `FolderBreadcrumbBridgeRouter.RouteAsync`
  wraps its whole dispatch in `catch (Exception ex) { return ErrorResponse(ex.Message); }`
  (`UtilitiesCS/OutlookObjects/Folder/FolderBreadcrumbBridgeRouter.cs:348-351`), and
  `BreadcrumbStateModel.SelectSubfolder` performs its own explicit range guard
  (`BreadcrumbStateModel.cs:406-413`). The Qfc surface is therefore an in-repo precedent for the guard shape.

### Q1c. Minimal fix shape — recommendation

**Recommend the range guard at the router, not a widened catch.**

The router's own XML doc comment (`BreadcrumbBridgeRouter.cs:151-154`) states the contract:

```csharp
/// Routes one inbound bridge payload. Malformed payloads fail fast with the codec's
/// <see cref="BreadcrumbMessageException"/> (already logged) and leave state unchanged.
```

- A **guard** at `:168-174` (reject an out-of-range index, log via the existing `log.Error` pattern used at
  `:162` and `:257`, and return without a render post) satisfies "leave state unchanged" literally: no
  transition is attempted at all, and the contract sentence remains true for this input.
- A **widened catch** at `:193` would suppress the throw *after* the transition attempt. `CollapseAfter`
  mutates nothing before it throws (`BreadcrumbRow.cs:111-118` precedes the two assignments at `:130-131`),
  so state is in fact preserved — but the contract sentence says the *codec* rejects malformed payloads, and a
  broad catch at the async-void boundary would silently absorb genuinely unexpected exception classes from
  the whole `ProcessInboundAsync` tree, which is the "broad-catch without added context" pattern the General
  Code Change Policy prohibits.

A defensible belt-and-braces variant: add the guard **and** keep the catch narrow. If the spec also wants the
async-void boundary hardened generally, add a *separate*, explicitly-logging `catch (Exception ex)` at `:193`
that re-logs with the payload text — but that is a second decision and should be an explicit AC, not a
side effect of the #498 fix.

Where to put the guard: inside the `case BreadcrumbMessageTypes.SegmentDoubleClick:` arm of
`ProcessInboundAsync` (`:168-174`). Do **not** change `BreadcrumbRow.CollapseAfter` to return `false`
instead of throwing — its throw is a documented contract (`BreadcrumbRow.cs:101-103`) with existing test
coverage, and `BreadcrumbRow.cs` is a shared type used by both surfaces.

### Q1d. Exact test home and reusable seam

**File: `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterQueueTests.cs`** (446 lines; already
`<Compile Include="Controllers\BreadcrumbBridgeRouterQueueTests.cs" />` at `QuickFiler.Test/QuickFiler.Test.csproj:58`).

That file is the negative/edge-path partner to `BreadcrumbBridgeRouterTests.cs`, and — decisively — it is the
**only** file in the repo that already drives a message through the `async void` host-event boundary rather
than calling `ProcessInboundAsync` directly. `BreadcrumbBridgeRouterQueueTests.cs:194-205`:

```csharp
[TestMethod]
public void MalformedInboundJson_ViaHostEvent_IsContainedAtTheBoundary()
{
    // Arrange: the async void host-event boundary catches only the codec exception.
    _initialized = true;
    Bind();

    // Act: raising the host event with a malformed payload must not throw or corrupt state.
    _host.Raise(h => h.MessageReceived += null, _host.Object, "{not valid json");

    // Assert
    _router.SelectedFolderPath.Should().BeNull();
}
```

`_host.Raise(h => h.MessageReceived += null, _host.Object, "<json>")` is the exact seam the #498 regression
test must reuse. Because `Moq`'s `Raise` invokes the handler synchronously and `OnHostMessageReceived`
completes synchronously when every awaited task is already completed (the provider mock uses `ReturnsAsync`),
the `ArgumentOutOfRangeException` surfaces **on the `Raise` call itself** in a test process — no
`SynchronizationContext` and no timing dependency. The RED test is therefore deterministic:
`Action act = () => _host.Raise(...); act.Should().NotThrow();`.

Full arrange block, verbatim, from `BreadcrumbBridgeRouterQueueTests.cs:34-96`:

```csharp
[TestInitialize]
public void Setup()
{
    _provider = new Mock<IFolderHierarchyProvider>();
    _host = new Mock<IBreadcrumbWebHost>();
    _initialized = false;
    _navigated = new List<string>();
    _posted = new List<string>();
    _host.SetupGet(h => h.IsCoreInitialized).Returns(() => _initialized);
    _host
        .Setup(h => h.NavigateToString(It.IsAny<string>()))
        .Callback<string>(html => _navigated.Add(html));
    _host
        .Setup(h => h.PostMessageJson(It.IsAny<string>()))
        .Callback<string>(json => _posted.Add(json));
    _provider
        .Setup(p =>
            p.ResolveLeafKeyAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())
        )
        .ReturnsAsync(
            (string path, CancellationToken ct) =>
                new FolderTreeNodeKey("store-1", "entry", path)
        );
    _provider
        .Setup(p =>
            p.GetAncestorChainAsync(
                It.IsAny<FolderTreeNodeKey>(),
                It.IsAny<CancellationToken>()
            )
        )
        .ReturnsAsync(
            new[] { Segment("Inbox", "Inbox", true), Segment(LeafPath, "Alpha", true) }
        );
    _router = new BreadcrumbBridgeRouter(
        _provider.Object,
        _host.Object,
        new BreadcrumbMessageCodec(),
        new BreadcrumbHtmlRenderer(),
        new BreadcrumbOutboundQueue(_host.Object)
    );
}

private static FolderBreadcrumbSegment Segment(string path, string name, bool hasChildren)
{
    return new FolderBreadcrumbSegment(
        new FolderTreeNodeKey("store-1", "entry", path),
        name,
        path,
        hasChildren
    );
}

private void Bind()
{
    _router
        .BindRowsAsync(
            new[] { LeafPath },
            Enumerable.Empty<FolderScore>(),
            CancellationToken.None
        )
        .GetAwaiter()
        .GetResult();
}

private void Inbound(string json)
{
    _router.ProcessInboundAsync(json).GetAwaiter().GetResult();
}
```

`Bind()` here produces a **two-segment** row (`row-0`), so `segmentIndex: 99` and `segmentIndex: -1` are both
out of range and `segmentIndex: 0` remains the valid control case. "State unchanged" is assertable with
`_posted.Count.Should().Be(postedBefore)` — the exact idiom already used at `:140`/`:146`, `:164`/`:170`,
`:314`/`:320`, `:379`/`:385`, `:410`/`:416`.

Note: `QuickFiler.Test` deliberately carries no Newtonsoft reference, so all outbound assertions are raw-JSON
substring assertions (`BreadcrumbBridgeRouterTests.cs:19-20`). Keep that constraint.

**File-size note:** at 446 lines, `BreadcrumbBridgeRouterQueueTests.cs` has ~54 lines of headroom against the
500-line limit. Two or three compact regression tests fit; a large block does not. If more is needed, the
established repo precedent is a `.Part2.cs` sibling (`QuickFiler.Test/Viewers/BreadcrumbDropDownHostTests.Part2.cs`),
which would require a new `Compile Include` in `QuickFiler.Test.csproj` alphabetically adjacent to line 58.

---

## Q2 — #499 stale `SelectedFolderPath` after rebind

### Q2a. Confirmed

`BreadcrumbBridgeRouter.cs:109-116`:

```csharp
_rows = _builder.BuildRows(
    presentedRows,
    text => chains.TryGetValue(text, out var chain) ? chain : null,
    scores
);
_selectedRowId = null;
DeliverDocument();
```

`_selectedRowId = null;` at `:114`. `SelectedFolderPath` is declared at `:58` with a private setter and is
assigned in exactly one place, `SelectRow` (`:372-375`):

```csharp
_selectedRowId = row.RowId;
SelectedFolderPath =
    row.Kind == BreadcrumbRowKind.TrashPseudoRow
        ? BreadcrumbRowBuilder.TrashRowText
        : row.LeafSegment?.FullPath ?? string.Empty;
```

`DeliverDocument` (`:397-409`) renders with `_selectedRowId` (`:399`), so post-rebind the document has no
`rowwrap selected` element while `SelectedFolderPath` still holds the pre-rebind value. Confirmed.

### Q2b. Every consumer, repo-wide

`SelectedFolderPath` (property read), all non-test occurrences:

- `QuickFiler/Controllers/EfcFormController.cs:294` — `get => _router?.SelectedFolderPath;`, the body of
  `public string SelectedFolder` (`:289-295`). **This is the only production reader.**

`SelectedFolderPathChanged` (event), all occurrences repo-wide:

- `QuickFiler/Controllers/BreadcrumbBridgeRouter.cs:61` (declaration), `:379` (raise).
- `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterTests.cs:219`, `:235` (test subscriptions only).

**There is no production subscriber to `SelectedFolderPathChanged` anywhere in the repository.**
(Contrast `FocusSearchRequested`, which *is* subscribed at `EfcFormController.cs:851`.)

What `EfcFormController` does with `SelectedFolder` (all read-only observations; we do not own that file):

- `:493-494` and `:772-773` — passed into the move operation together with `_globals.Ol.ArchiveRootPath`.
- `:478`, `:504`, `:722`, `:760`, `:783` — passed into the folder-open paths, again alongside
  `_globals.Ol.ArchiveRootPath`.
- `:873-883` `BindFolderRows` — the rebind entry point, reached from the `SearchText.TextChanged` path and
  from the delete-path trash rebind, delegating to `BindBreadcrumbRowsAsync` (`:886-899`) which calls
  `_router.BindRowsAsync(rows, scores, Token)` at `:893`.

**The fix can be confined entirely to `BreadcrumbBridgeRouter.cs`.** No `EfcFormController` change is needed:
`SelectedFolder` is a pure pass-through of the router property, and there is no subscriber whose behavior a
raised event could disturb.

### Q2c. Decision — clear, and raise `SelectedFolderPathChanged(null)`

**Recommend: clear `SelectedFolderPath` to `null` in `BindRowsAsync`, alongside `_selectedRowId = null`, and
raise `SelectedFolderPathChanged(this, null)` — but only when the value actually changed.**

Justification against the two options:

*Option A — restore the prior selection when the same folder is still present in the new row set.*
Rejected for this feature.
1. It does not fix the reported defect on the path where it matters. `BindFolderRows` runs on every search
   keystroke; the common case is that the previously selected folder has just been filtered *out* of the row
   set, which is exactly the divergent window #499 describes. Restoration only helps the sub-case where the
   folder survived, and in that sub-case the current behavior is already benign.
2. Restoration must match a row by identity. The only stable identity the router has is
   `row.LeafSegment?.FullPath` — which is precisely the value #439 changes form on (see §3). Landing a
   restore-by-path rule in the same feature that changes the path form couples two defects that are otherwise
   independent.
3. It is a strictly larger observable contract change: it would make a re-bind *preserve* a selection that the
   re-rendered document does not visually show as selected unless the render is also changed to re-mark it —
   i.e. it forces a second change to `DeliverDocument`/`_selectedRowId` to stay coherent.

*Option B — clear without raising the event.* Rejected because it leaves the API self-inconsistent: the event
is documented at `:60` as "Raised when `SelectedFolderPath` changes via a selection action". Clearing is a
change to the property; if it is silent, a future subscriber cannot track the property from the event alone.
Since there is no production subscriber today, raising costs nothing and closes the gap permanently.

**Observable behavior change to state in the spec:** after any `BindRowsAsync` re-bind that follows a
selection, `EfcFormController.SelectedFolder` returns `null` instead of the previous folder, until the user
re-selects. A move or folder-open triggered in that window will therefore act on a null selection rather than
a stale folder. Two follow-on facts the spec must acknowledge:

- `EfcFormController` already has an `IsValidSelection` guard referenced in the comment at `:291-293`; whether
  it tolerates `null` is **unverified** — the planner should read `EfcFormController.IsValidSelection` before
  finalizing the AC, because a null-intolerant guard would turn a silent mis-file into a
  `NullReferenceException`. That reading is read-only and does not require writing the file.
- `SelectFirstRow` (`:119-126`) is *not* called from `BindRowsAsync`. If the product wants a row auto-selected
  after every rebind, that is a separate decision and a separate AC; do not add it as a side effect.

### Q2d. Test home and seam

**File: `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterQueueTests.cs`** (same file as #498; both are
negative/edge-path router behaviors, both need `Bind()` twice).

Seam: the `Setup()` block quoted in §Q1d, plus `Inbound("{\"type\":\"rowSelected\",\"rowId\":\"row-0\"}")`
(the exact idiom at `:189` and `:432`), then a second `Bind()`, then
`_router.SelectedFolderPath.Should().BeNull()`. The event assertion reuses the subscription idiom from
`BreadcrumbBridgeRouterTests.cs:219`:
`string observed = "sentinel"; _router.SelectedFolderPathChanged += (s, path) => observed = path;`

An existing test that must be checked for interaction (it will still pass, but read it):
`BreadcrumbBridgeRouterQueueTests.cs:175-191` `MalformedInboundJson_ThrowsCodecExceptionWithoutCorruptingState`
asserts `_router.SelectedFolderPath.Should().BeNull()` after a single `Bind()` — that assertion is unaffected
because nothing was selected before the bind.

---

## Q3 — #439 missing ancestor lineage (the path-form mismatch)

### Q3a. End-to-end trace of the presented-row text form

**(i) Search matches** — `UtilitiesCS/OutlookObjects/Folder/FolderPredictor.cs`:

- `GetMatchingFolders` (`:864-881`) calls `LoopFolders(folders, ref matchingFolders, strEmailFolderPath, true, exclusions)` at `:877`.
- `LoopFolders` (`:883-931`) defaults `olAncestor` to `_globals.Ol.ArchiveRootPath` when empty (`:891-894`),
  computes `var folderStem = GetOlSubpath(f.FolderPath, olAncestor, true);` at `:898`, and adds **that stem**
  to the match list at `:919` (`matchingFolders.Add(folderStem);`).
- `GetOlSubpath` (`:933-951`) with `includeChildren: true` returns
  `path.Substring(olAncestor.Length + 1)` (`:943`) — i.e. the archive-root prefix is stripped.
- `AddMatches` (`:794-802`) prepends the `======= SEARCH RESULTS =======` banner and appends those stems verbatim.

**Search-match presented text = archive-root-relative stem.** Confirmed.

**(ii) Suggestion rows** — the potential explicitly says this was not traced. Traced now:

- `AddSuggestions` (`:804-808`) appends `Suggestions.ToArray(5)`; `AddSuggestionRows` (`:832-841`) is its
  row-model mirror and emits `score.FolderPath` at `:839`.
- `FolderScorer.ToArray(int)` (`FolderScorer.cs:252-253`) and `ToScoredArray(int)` (`:273`) both project the
  same `OrderedScores()` enumeration (`:245-248`) over `_folderNameScores`; `FolderScore.FolderPath` is
  `x.Key` (`:296-297`), i.e. **the scorer's dictionary key**.
- Those keys are written only by `FolderScorer.AddSuggestion(string folderPath, long score)` (`:196-200`),
  reached from three sources:
  - Bayesian classifier: `AddSuggestion(prediction.Class!, score)` at `:178`.
  - Conversation map: `AddSuggestion(match.EmailFolder!, score)` at `:323`.
  - Word-sequence map: `AddSuggestion(entry.FolderPath, entry.Score)` at `:401`, whose entries come from
    `appGlobals.AF.SubjectMap`, populated from `node.RelativePath` of the folder-tree snapshot
    (`UtilitiesCS/EmailIntelligence/SubjectMap/SubjectMapSco.Orchestration.cs:30`, `:33`).

**Answer:** the suggestion rows use a **relative** path form, not a full Outlook path — consistent with the
search matches and consistent with the reported symptom affecting both sections. The exact relative *root* is
not provably identical across all three suggestion sources: the word-sequence source is store-root-relative
(`RelativePath`), while the search matches are archive-root-relative (`GetOlSubpath` against
`ArchiveRootPath`). The Bayesian `prediction.Class` and the conversation-map `EmailFolder` forms are
**unverified** — I did not trace the classifier's class-label provenance or the CtfMap population to a
definitive string form. What *is* verified is that none of the three is a full Outlook `FolderPath`
beginning with `\\<store>`, which is what `ResolveLeafKeyAsync` compares against.

The corroborating downstream fact: `EfcDataModel` passes the presented text as `DestinationOlStem` together
with `OlAncestor = Globals.Ol.ArchiveRootPath` (`QuickFiler/Controllers/EfcDataModel.cs:286-289`, `:307-310`,
`:325-328`). The presented text is therefore *contractually* an archive-root-relative stem on the filing side.

**Archive-root provenance:** `IOlObjects.ArchiveRootPath` (`UtilitiesCS/Interfaces/IGlobals/IOlObjects.cs:15`)
is implemented at `TaskMaster/AppGlobals/AppOlObjects.cs:238-248` as
`Path.Combine(Root.FolderPath, "Archive")`, where `Root` is `App.Session.DefaultStore.GetRootFolder()`
(`AppOlObjects.cs:202-210`). So `ArchiveRootPath == "<storeRootFolderPath>\Archive"`.

### Q3b. `ResolveLeafKeyAsync`'s matching rule and what `node.FolderPath` carries

`UtilitiesCS/OutlookObjects/Folder/OutlookFolderHierarchyProvider.cs:52-71`:

```csharp
public async Task<FolderTreeNodeKey?> ResolveLeafKeyAsync(
    string folderPath,
    CancellationToken cancellationToken
)
{
    if (string.IsNullOrWhiteSpace(folderPath))
    {
        return null;
    }

    var snapshot = await AcquireSnapshotAsync(cancellationToken).ConfigureAwait(false);

    // First-match on duplicate paths is the documented behavior; real Outlook full paths embed
    // the store name and are unique in practice.
    var match = snapshot.NodesByKey.Values.FirstOrDefault(node =>
        string.Equals(node.FolderPath, folderPath, StringComparison.OrdinalIgnoreCase)
    );

    return match?.Key;
}
```

Exact `OrdinalIgnoreCase` equality against `node.FolderPath` only. Confirmed.

`node.FolderPath` is the **raw Outlook `MAPIFolder.FolderPath`**: it is captured verbatim at
`UtilitiesCS/OutlookObjects/Folder/OutlookFolderHierarchyReader.cs:143` (`folder.FolderPath`) from the
adapter at `:265` and carried into the node constructor at `:198`. For a store-rooted archive folder that is
`\\<store>\Archive\Projects\Alpha`.

**Key additional finding not in the potential:** `FolderTreeSnapshotNode` carries a **second** path field,
`RelativePath` (`FolderTreeSnapshotNode.cs:53`, set at `:33`), computed at
`OutlookFolderHierarchyReader.cs:206-211`:

```csharp
private static string GetRelativePath(string rootPath, IOutlookFolderAdapter folder)
{
    return string.Equals(rootPath, folder.FolderPath, StringComparison.OrdinalIgnoreCase)
        ? folder.Name
        : folder.FolderPath.Replace(rootPath + "\\", string.Empty);
}
```

`rootPath` here is the **store** root (`OutlookFolderHierarchyReader.cs:127`,
`stack.Push(Tuple.Create(root, string.Empty, root.FolderPath))`, with `root = store.GetRootFolder()` at `:97`).
So `RelativePath` for the example is `Archive\Projects\Alpha` — **store**-relative, while the presented stem
is **archive**-relative (`Projects\Alpha`). `RelativePath` therefore does *not* match the presented text
either, and a naive "also compare `RelativePath`" fix would not work. This is the single most important
correction to the mental model in the potential.

### Q3c. `BreadcrumbRowBuilder.BuildRow`'s fallback — READ ONLY, confirmed as the source

`UtilitiesCS/OutlookObjects/Folder/BreadcrumbRowBuilder.cs:119-134`:

```csharp
default:
    IReadOnlyList<BreadcrumbSegment> segments = MapSegments(ancestorChain);
    if (segments.Count == 0)
    {
        // Unknown/empty chain fallback: render the presented path as a single
        // leaf-only segment so the suggestion stays visible and selectable.
        segments = new[]
        {
            new BreadcrumbSegment(presentedText, LeafToken(presentedText), false),
        };
    }

    string joinPath = segments[segments.Count - 1].FullPath;
    double? probability = probabilityByPath.TryGetValue(joinPath, out double p)
        ? p
        : (double?)null;
```

Confirmed: this is the single-segment presentation. `LeafToken` (`:229-234`) takes the last `\`/`/`-delimited
token, which is why the user sees only the leaf folder name. The fallback segment's `FullPath` is the
**presented text** (`:127`), and `HasSubfolders` is hard-coded `false`, which additionally disables the
leaf-expand affordance today (`BreadcrumbRow.CanExpandLeaf`, `BreadcrumbRow.cs:260-263`).

The chain path is reached only when `FetchChainAsync` returns non-null (`BreadcrumbBridgeRouter.cs:99-107`),
and `FetchChainAsync` (`:334-362`) returns `null` at `:345-348` when `ResolveLeafKeyAsync` returns `null`
without ever calling `GetAncestorChainAsync`. Chain confirmed end to end.

### Q3d. The probability join — checked, not assumed. **The potential's fear is inverted.**

`BuildProbabilityIndex` (`BreadcrumbRowBuilder.cs:208-227`) keys on `score.FolderPath`
(`index[score.FolderPath] = score.Probability;` at `:222`), case-insensitively (`:217`). The scores reaching
the router come from `EfcFormController.cs:891` (`_dataModel?.FolderHelper?.Suggestions?.ToScoredArray()`),
so the index keys are the **scorer keys** — the same relative form as the presented suggestion text (§Q3a).

The join key is `joinPath = segments[last].FullPath` (`:131`).

- **Today (chain unresolved):** `joinPath == presentedText == scorer key`. **The percentage joins correctly.**
  The percentage works *because* the lineage is broken.
- **After a naive #439 fix (chain resolved):** `joinPath` becomes `FolderBreadcrumbSegment.FolderPath` —
  the **full** Outlook path (`MapSegments`, `:196-202`, maps `segment.FolderPath` into `FullPath`). The index
  is still keyed on the relative stem, so `TryGetValue` misses and **every suggestion row loses its percentage**.

**This is a regression the #439 fix will cause unless it is handled explicitly.** It is the exact inverse of
the risk the potential anticipated, and it is directly adjacent to issue #400 (AC-1/AC-10 pin the percentage
contract — `docs/features/archive/2026-07-21-quickfiler-folder-selector-dropdown-400/spec.md:239`, `:248`).

The existing test `BreadcrumbBridgeRouterTests.cs:126-136` asserts `_navigated[0].Should().Contain("90%")` and
passes today only because the mock provider returns a chain whose leaf `FolderPath` equals the presented text
(`SetupProviderChain`, `:77-106`, and `Bind()`'s `new FolderScore(LeafPath, 1000, 0.9)` at `:113`). The mock
masks the production mismatch; the test will *not* catch this regression.

**A second, larger regression of the same shape — the filing target.** `SelectRow` sets
`SelectedFolderPath = row.LeafSegment?.FullPath` (`BreadcrumbBridgeRouter.cs:372-375`). Once the chain
resolves, that becomes the **full** Outlook path, and `EfcFormController.SelectedFolder` feeds it in as
`DestinationOlStem` next to `OlAncestor = ArchiveRootPath` (`EfcDataModel.cs:286-289`). **Filing would break.**
The identical hazard exists on the Qfc surface:
`BreadcrumbSelectionMap.GetSelectedFolder` returns `row.Chain[row.Chain.Count - 1].FolderPath` for a
suggestion row (`UtilitiesCS/OutlookObjects/Folder/BreadcrumbSelectionMap.cs:109`).

Both hazards must be explicit ACs. Both are fixable inside owned files on the Efc side (see Q3e); the Qfc
side's `BreadcrumbSelectionMap.cs` is **not owned** (see Q6a).

### Q3e. Where to establish the canonical path form — recommendation

**Recommend: `UtilitiesCS/OutlookObjects/Folder/OutlookFolderHierarchyProvider.ResolveLeafKeyAsync` (owned),
with a compensating pair of adjustments in `BreadcrumbBridgeRouter` (owned).**

Evaluation of the three candidates:

| Candidate | Verdict |
|---|---|
| `FolderPredictor` (owned) — present full paths instead of stems | **Reject.** The presented text is contractually a stem on the filing side (`EfcDataModel.cs:286-289`, `:307-310`, `:325-328`; `EfcFormController.cs:493-494`, `:772-773`). Changing it changes the filing contract for both the Efc and Qfc surfaces and for the recents list. It is also a 983-line file already over the 500-line limit. |
| `BreadcrumbBridgeRouter.BindRowsAsync` (owned) — normalize at the boundary | **Reject as the primary site.** The router has no access to `ArchiveRootPath`: its constructor takes only `(IFolderHierarchyProvider, IBreadcrumbWebHost, BreadcrumbMessageCodec, BreadcrumbHtmlRenderer, BreadcrumbOutboundQueue)` (`:40-55`) and is constructed in `EfcFormController.cs:843-849`, which we may not write. Adding a root parameter requires editing an unowned file. |
| `OutlookFolderHierarchyProvider.ResolveLeafKeyAsync` (owned) | **Recommend.** Requires no signature change, no constructor change, and no unowned-file edit. It is precisely "the boundary between the predictor's presented rows and `IFolderHierarchyProvider`" that the potential names, and it is explicitly *not* `BreadcrumbRowBuilder`, so the potential's prohibition is respected. The file is 98 lines — the most headroom of any owned file. And because the Qfc router resolves through the same method (`FolderBreadcrumbBridgeRouter.cs:49-54`), one change fixes **both** surfaces. |

**Concrete rule.** Keep the existing exact `OrdinalIgnoreCase` match as the first pass (identity case,
zero behavior change for a caller that already supplies a full path). When it misses, fall back to a
**suffix match with a segment boundary**: a node matches when
`node.FolderPath.EndsWith("\\" + folderPath, OrdinalIgnoreCase)`. Accept the fallback **only when it is
unique**; on zero or multiple candidates, log via the existing `log4net` pattern and return `null`, which
preserves today's single-segment fallback rendering. Uniqueness is what makes this safe: it prevents
`Projects\Alpha` from silently binding to `\\store\Inbox\Projects\Alpha` when
`\\store\Archive\Projects\Alpha` also exists.

Note the archive-root value is **not needed** by this rule, which is why it works despite `ArchiveRootPath`
being unavailable at every owned site. That is the decisive practical advantage over any prefix-reconstruction
approach. (`OutlookFolderHierarchyProvider` has no `IApplicationGlobals` dependency, and adding one would
require editing `EfcFormController.cs:840-842` — not owned.)

**Compensating adjustments inside `BreadcrumbBridgeRouter.cs` (owned), both required to avoid the §Q3d
regressions:**

1. **Preserve the filing target.** In `BindRowsAsync`, retain a `rowId -> presentedText` map (the loop at
   `:88-107` already visits each presented text, and `BuildRows` assigns `row-{i}` over the same sequence,
   `BreadcrumbRowBuilder.cs:53-57`, so the correspondence is positional and exact). In `SelectRow`
   (`:364-380`), set `SelectedFolderPath` from the **presented text** rather than
   `row.LeafSegment?.FullPath`. This makes the filing contract independent of whether the chain resolved,
   which is a strict improvement over today.
2. **Preserve the percentage.** In `BindRowsAsync`, before calling `_builder.BuildRows`, extend the `scores`
   sequence with an **alias** `FolderScore` for each presented text whose chain resolved:
   `new FolderScore(resolvedChain[last].FolderPath, originalScore.Score, originalScore.Probability)`.
   `FolderScore` is a net48-safe `readonly struct` with a public
   `(string folderPath, long score, double probability)` constructor (`UtilitiesCS/OutlookObjects/Folder/FolderScore.cs:22-27`),
   and `BuildProbabilityIndex` is a last-write-wins dictionary build (`BreadcrumbRowBuilder.cs:217-224`), so
   adding aliases is additive and cannot drop an existing key. This keeps `BreadcrumbRowBuilder` untouched.

Diagnosability, per the potential: when the fallback also misses, `ResolveLeafKeyAsync` should log at
`Error` (matching `BreadcrumbBridgeRouter.cs:162`, `:257`, `:302-305`) so a systematic resolution failure is
visible rather than presenting as a cosmetic omission.

### Q3f. `FolderBreadcrumb.html` separator rendering — and a surface correction

`QuickFiler/Resources/FolderBreadcrumb.html:249-268` (verified):

```javascript
var element;
if (cell.kind === "segment") {
  element = document.createElement("span");
  element.className = cell.truncationEligible ? "seg trunc" : "seg";
  element.textContent = cell.text;
  element.title = cell.text;
  element.addEventListener("dblclick", function (event) {
    event.stopPropagation();
    post({ type: "segmentDoubleClick", rowIndex: row.rowIndex, segmentIndex: cell.segmentIndex });
  });
} else if (cell.kind === "arrow") {
  element = document.createElement("span");
  element.className = "arrow";
  element.textContent = "→";
} else { /* affordance */ }
```

Multi-segment rendering with the `→` separator works once a chain resolves — the renderer is cell-driven and
emits one `arrow` cell per inter-segment gap. Confirmed.

**But this is the Qfc document.** The **Efc** surface — the one #439 reports — does not use
`FolderBreadcrumb.html` at all. `EfcFormController.ConfigureBreadcrumbControl` (`:834-854`) wires a
`BreadcrumbHtmlRenderer`, which assembles the document from `BreadcrumbDocumentAssets`
(`UtilitiesCS/OutlookObjects/Folder/BreadcrumbHtmlRenderer.cs:40-49`). Two consequences:

- The Efc separator is `<span class="sep"> &gt; </span>`
  (`BreadcrumbHtmlRenderer.cs:147-150`), i.e. a **`>` character, not `→`**. Multi-segment rendering on the
  Efc surface *does* already work — the loop is `BreadcrumbHtmlRenderer.cs:144-162` — but the glyph the user
  described as the "arrow separator" is not what the Efc surface emits. If the spec wants `→` on the Efc
  surface, that is a change to `BreadcrumbHtmlRenderer.cs` or `BreadcrumbDocumentAssets.cs`, **neither of
  which is owned**. Flag as a cross-feature note; a purely cosmetic glyph change is the weakest part of #439
  and should be dropped from scope unless the maintainer asks for it.
- The Efc segment double-click handler is `BreadcrumbDocumentAssets.cs:59-67` (a delegated `dblclick`
  listener posting `{ type: 'segmentDoubleClick', rowId, segmentIndex }`), **not** `FolderBreadcrumb.html:255-258`.
  Adding an Efc ancestor-navigation *gesture* therefore requires writing `BreadcrumbDocumentAssets.cs` — not
  owned. See Q6a.

---

## Q4 — #440 Left/Right parent-child tree navigation, both surfaces

### Q4a. Current arrow handling

**Efc — `QuickFiler/Controllers/BreadcrumbBridgeRouter.HandleArrowKeyAsync` (`:225-260`):**

```csharp
switch (key)
{
    case "Right":
        if (row.IsCollapsed)
        {
            if (row.ReExpand()) { PostRowRender(row); }
        }
        else if (!row.IsLeafExpanded)
        {
            await ExpandLeafAsync(row);
        }
        break;
    case "Left":
        if (row.LeftArrow()) { PostRowRender(row); }
        break;
    case "Up":   HandleUpArrow(row);          break;
    case "Down": MoveSelection(row, step: 1); break;
    default:     log.Error(...);              break;
}
```

Note Right does **not** call `BreadcrumbRow.RightArrow()`; it re-implements the branch inline so it can await
`ExpandLeafAsync` (`:285-332`), which is the only provider-backed expansion path.

**Efc row model — `UtilitiesCS/OutlookObjects/Folder/BreadcrumbRow.cs`:**
- `CollapseAfter(int)` `:104-133` (throws out of range, `:111-118`).
- `ReExpand()` `:139-148`.
- `LeftArrow()` `:195-216` — closes an open leaf expansion first (`:202-206`), else decrements the terminal
  index (`:208-215`), returning `false` when only the root segment remains (`:209-212`).
- `RightArrow()` `:224-243` — present but **unused by the Efc router**; `HandleArrowKeyAsync` inlines the
  equivalent logic. Only `BreadcrumbRowStateTests` exercises it.
- `VisibleSegments()` `:250-258`.

Every transition mutates **view state only**; the class doc says so explicitly at `:23-33`
("Transitions mutate ONLY view state"). No selected-node concept exists.

**Qfc — `UtilitiesCS/OutlookObjects/Folder/FolderBreadcrumbBridgeRouter.ArrowAsync` (`:378-406`):**

```csharp
bool handled =
    direction == BreadcrumbArrowDirection.Right
        ? _model.RightArrow()
        : _model.LeftArrow();
if (!handled)
{
    return new[]
    {
        BreadcrumbBridgeSerializer.Serialize(new UnhandledArrowMessage(direction)),
    };
}
```

**Qfc state model — `UtilitiesCS/OutlookObjects/Folder/BreadcrumbStateModel.cs`:**
- `RightArrow()` `:424-437` — `ReExpand()` when collapsed, else `row.TryExpandLeaf()`.
- `LeftArrow()` `:443-455` — clears `_selectedSubfolderIndex` (`:450-453`), then `row.TryCollapseLeaf()`.
- `SelectSubfolder` `:400-416` — carries the explicit range guard cited in §Q1b.

Neither reassigns the selected row or node to a parent. Confirmed.

### Q4b. `onArrow` gating and the Qfc legacy fall-through

`QuickFiler/Resources/FolderBreadcrumb.html:395-427` (verified):

```javascript
function onArrow(direction) {
  var row = selectedRow();
  var canRight =
    row !== null && (row.collapsed || (!row.leafExpanded && rowHasOpenAffordance(row)));
  var canLeft = row !== null && row.leafExpanded;
  var can = direction === "right" ? canRight : canLeft;
  post(can
    ? { type: "arrowKey", direction: direction }
    : { type: "unhandledArrow", direction: direction });
}
```

Wired from the keydown handler at `:420-426` (`ArrowRight` -> `onArrow("right")`, `ArrowLeft` -> `onArrow("left")`,
each `preventDefault()`ed).

**What the Qfc legacy fall-through does today, exactly.** Route:
`unhandledArrow` -> `BreadcrumbBridgeCoordinator.UnhandledArrow` (`QuickFiler/Viewers/BreadcrumbBridgeCoordinator.cs:65`,
raised at `:372-375`) -> `ItemViewer.BreadcrumbUnhandledArrow` (`QuickFiler/Viewers/ItemViewer.Breadcrumb.cs:32`,
`:271-272`) -> `QfcItemController.OnBreadcrumbUnhandledArrow` (`QuickFiler/Controllers/QfcItemController.ViewerSetup.cs:186-190`)
-> `KeyboardHandler.BreadcrumbArrowFallThrough`.

`QuickFiler/Controllers/KeyboardHandler.cs:288-315`:

```csharp
public void BreadcrumbArrowFallThrough(
    ItemViewer viewer,
    UtilitiesCS.OutlookObjects.Folder.BreadcrumbArrowDirection direction
)
{
    if (viewer is null)
    {
        throw new ArgumentNullException(nameof(viewer));
    }

    if (direction == UtilitiesCS.OutlookObjects.Folder.BreadcrumbArrowDirection.Right)
    {
        MyBox.ShowDialog(
            "Pop Out Item or Enumerate Conversation?",
            "Dialog",
            BoxIcon.Question,
            viewer.Controller.RightKeyActions
        );
    }
    else
    {
        viewer.SetFolderDroppedDown(false);
    }
}
```

So: unhandled **Right** opens a modal `MyBox` dialog; unhandled **Left** closes the folder drop-down. There is
**no** Efc analogue — the Efc router simply does nothing when a transition returns `false`
(`BreadcrumbBridgeRouter.cs:243-249`) and emits no `unhandledArrow` message at all.

Testability note: `MyBox.ShowDialog` at `:304-309` is a modal WinForms call with no injectable seam, so any
test that reaches the Right fall-through will block. Regression tests must assert at the
`BreadcrumbArrowFallThrough` *call site* (the existing precedent is
`QuickFiler.Test/Controllers/QfcItemControllerBreadcrumbDropDownTests.cs:156-168`, which mocks
`IQfcKeyboardHandler.BreadcrumbArrowFallThrough` — the interface member is declared at
`QuickFiler/Interfaces/IQfcKeyboardHandler.cs:32`) rather than inside the handler body.

### Q4c. Reconciliation with issue #400 — **#440 contradicts a landed, checked-off acceptance criterion**

Found at `docs/features/archive/2026-07-21-quickfiler-folder-selector-dropdown-400/`. Verbatim:

- `spec.md:247` — `- [x] AC-9: Left and Right preserve the existing breadcrumb expand, collapse, and unhandled-key behavior in both view modes and do not mutate the committed/original/pending selector session.`
- `spec.md:47` (Expected Behavior) — `- Left and Right retain their current breadcrumb behavior.`
- `spec.md:107` (Boundaries and invariants to preserve) — `- Left and Right keep the current expand, collapse, and unhandled-key routing semantics.`
- `issue.md:44` — `- Left and Right retain the existing breadcrumb expansion, collapse, and fall-through behavior.`

The `[x]` marks AC-9 as verified and the feature is archived, so this is a **landed** criterion, not a draft.

**Verdict: yes, #440 as scoped directly contradicts #400 AC-9.** #440's Expected Behavior asks Left to
"select that row's parent as the current node" and Right to "expand that node into its children", which is by
construction *not* "the existing breadcrumb expand, collapse, and unhandled-key behavior".

The contradiction is not merely textual — it is enforced by a landed test.
`QuickFiler.Test/Viewers/FolderBreadcrumbAssetContractTests.cs:359-367`:

```csharp
[TestMethod]
public void LeftAndRightBreadcrumbMessages_RemainSupported()
{
    // Assert
    Html.Should().MatchRegex(@"(?:event|ev)\.key\s*===\s*\""ArrowRight\""");
    Html.Should().MatchRegex(@"(?:event|ev)\.key\s*===\s*\""ArrowLeft\""");
    Html.Should().Contain("{ type: \"arrowKey\", direction: direction }");
    Html.Should().Contain("{ type: \"unhandledArrow\", direction: direction }");
}
```

Any change to `onArrow`'s message shape breaks this test. (It is a test file, so amending it is permitted —
but doing so is a deliberate retraction of an AC-9 assertion and must be recorded as such.)

**This is a scope decision the spec must resolve explicitly.** The spec must state, as a numbered decision:
whether #440 supersedes #400 AC-9; whether the Qfc `unhandledArrow` fall-through (`MyBox` Pop Out /
Enumerate Conversation on Right; `SetFolderDroppedDown(false)` on Left) is retained, re-gated, or removed;
and, if retained, by what gesture the Pop Out dialog stays reachable. #400 AC-9 also protects the
committed/original/pending **selector session** (`BreadcrumbSelectionSession`) — a #440 implementation must
either leave that session untouched or explicitly amend that half of AC-9 too.

The #400 Up/Down/Enter/Escape contract (AC-5 through AC-8, `spec.md:243-246`) is untouched by #440 and should
be stated as preserved.

### Q4d. Existing selected-node concept — **none exists; do not invent a new shared type without saying so**

Enumerated:

| Type | Location | What it actually is |
|---|---|---|
| `BreadcrumbSelectionSession` | `UtilitiesCS/OutlookObjects/Folder/BreadcrumbSelectionSession.cs:98-107` | Row-level selector session: `CommittedIdentity` / `OriginalIdentity` / `PendingIdentity` / `IsOpen`, all `string?` **row identities**. Transitions `SelectRow(int)`, `MoveSelector(bool)`, `CommitSelector()`, `CancelSelector()`, `ActivateSubfolder(string, int)`. **Row-level, not node-level.** |
| `BreadcrumbSelectionMap` | `UtilitiesCS/OutlookObjects/Folder/BreadcrumbSelectionMap.cs:15-51` | Static projection of the model to the legacy string outputs (`GetSelectedFolder`, `GetFolderItems`, `FolderContains`). No node concept. |
| `FolderTreeSelectionOverlay` | `UtilitiesCS/OutlookObjects/Folder/FolderTreeSelectionOverlay.cs:12-37` | Immutable multi-select set keyed on `relativePath` (`IsSelected(node)`, `WithSelection(relativePath, bool)`). Belongs to the folder-filter surface, not the breadcrumb. |
| `FolderNavigator` | `UtilitiesCS/OutlookObjects/Folder/FolderNavigator.cs:10` | `static Folder? GetOutlookFolder(string FolderPath, Application OlApp)` — a **live COM** path walk. Unusable in unit tests; not a selection concept. |
| `BreadcrumbRow` / `BreadcrumbStateRow` | `BreadcrumbRow.cs:34`, `BreadcrumbStateModel.cs` | Carry `Segments`/`Chain` plus collapse/leaf view state. `CollapsedAfterIndex` is a **display** index, explicitly documented as view state (`BreadcrumbRow.cs:23-33`). |

**Conclusion: no selected-node concept exists on either surface.** #440 requires one. The lowest-cost shape,
and the only one expressible inside owned files, is a **new index field on `BreadcrumbRow`** (owned) —
e.g. `SelectedSegmentIndex`, defaulting to the leaf — plus transitions that move it. `BreadcrumbRow` is the
type both surfaces already share (`BreadcrumbStateRow` is a separate Qfc type; **unverified** whether it can
reuse `BreadcrumbRow` without an unowned-file change — the planner must read
`UtilitiesCS/OutlookObjects/Folder/BreadcrumbStateModel.cs` `BreadcrumbStateRow` definition before committing
to a shared-transition design, which the #440 potential's third design bullet assumes).

**Child-retrieval seam — exact signature** (`UtilitiesCS/OutlookObjects/Folder/IFolderHierarchyProvider.cs:46-49`):

```csharp
Task<IReadOnlyList<FolderBreadcrumbSegment>> GetImmediateSubfoldersAsync(
    FolderTreeNodeKey segmentKey,
    CancellationToken cancellationToken
);
```

Reuse this; do not add an interface member (`IFolderHierarchyProvider.cs` is **not** owned).

**Blocking mechanical detail.** `GetImmediateSubfoldersAsync` needs a `FolderTreeNodeKey`, but the Efc row
model's `BreadcrumbSegment` **carries no key** — it has only `FullPath`, `DisplayName`, `HasSubfolders`
(`UtilitiesCS/OutlookObjects/Folder/BreadcrumbSegment.cs:29-43`), because `MapSegments` drops
`FolderBreadcrumbSegment.Key` (`BreadcrumbRowBuilder.cs:196-202`). So expanding an **ancestor** segment on the
Efc surface must re-resolve by path: `ResolveLeafKeyAsync(ancestorSegment.FullPath, ct)` then
`GetImmediateSubfoldersAsync(key, ct)` — exactly the two-call pattern `ExpandLeafAsync` already uses at
`BreadcrumbBridgeRouter.cs:296-309`. That is achievable entirely inside the owned router. The Qfc side has it
easier: `BreadcrumbStateRow.Chain` holds `FolderBreadcrumbSegment`, which **does** carry `Key`
(used at `FolderBreadcrumbBridgeRouter.cs:416`).

### Q4e. Boundary no-ops and what the html does with `unhandledArrow`

- **Left at the root.** Efc: `BreadcrumbRow.LeftArrow()` returns `false` when `terminalIndex == 0`
  (`BreadcrumbRow.cs:209-212`); the router then does nothing (`BreadcrumbBridgeRouter.cs:243-248`) — a silent
  no-op with no message emitted. Qfc: `BreadcrumbStateModel.LeftArrow()` returns
  `row.TryCollapseLeaf()` (`BreadcrumbStateModel.cs:454`), which is `false` when the leaf is not expanded, so
  `ArrowAsync` emits `UnhandledArrowMessage` (`FolderBreadcrumbBridgeRouter.cs:387-393`) and the legacy
  fall-through closes the drop-down. **The two surfaces already disagree at this boundary today.**
- **Right on a childless node.** Efc: `ExpandLeafAsync` returns early when
  `leaf?.HasSubfolders != true` (`BreadcrumbBridgeRouter.cs:287-291`) — a documented no-op with no message.
  Qfc: `row.TryExpandLeaf()` returns `false`, so `unhandledArrow` fires and Right opens the `MyBox` dialog.
- **What the html does with `unhandledArrow`.** `FolderBreadcrumb.html:401-403` posts it and does nothing
  locally; the *page* takes no action. It always `preventDefault()`s the key first (`:421`, `:424`), so the
  arrow never reaches the browser's default handling either way. The Efc document
  (`BreadcrumbDocumentAssets.cs:81-89`) has **no** `unhandledArrow` concept at all — it posts every mapped
  arrow unconditionally as `arrowKey`.

So a unified contract requires the spec to state, per direction and per surface, one of: silent no-op /
`unhandledArrow` + fall-through / new behavior. This is the boundary decision the #440 potential defers to
planning, and it cannot be deferred past the spec.

### Q4f. Dependency of #440 on #439 — confirmed, with a caveat

**#440 does depend on #439, and the dependency is stronger than the potential states.** Verified mechanism:
today the Efc chain never resolves (§Q3), so `BreadcrumbRowBuilder` emits a **one-segment** row
(`BreadcrumbRowBuilder.cs:121-129`). With one segment:

- `LeftArrow()` returns `false` immediately (`terminalIndex == 0`, `BreadcrumbRow.cs:208-212`), so there is no
  parent to select — a "select parent" transition would be a permanent no-op on every production row.
- The fallback segment is constructed with `hasSubfolders: false` (`BreadcrumbRowBuilder.cs:127`), so
  `CanExpandLeaf()` is always `false` (`BreadcrumbRow.cs:260-263`) and Right's expansion is *also* a
  permanent no-op today.

So #440 implemented before #439 would be untestable against production data and unobservable to the user.

**Recommended intra-feature sequencing: #439 before #440.** See the final section.

---

## Q5 — Test infrastructure inventory

### Q5a. Regression-test home per defect

| Defect | File | Why |
|---|---|---|
| **#498** | `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterQueueTests.cs` | Only file with the `_host.Raise(h => h.MessageReceived += null, ...)` async-void-boundary seam (`:201`); it is the designated negative/edge-path file (`:15-21`). |
| **#499** | `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterQueueTests.cs` | Same `Setup()`; needs `Bind()` twice plus a `rowSelected` between — the existing `Inbound(...)` helper (`:98-101`) and the double-`Bind` pattern at `:428-444` apply directly. |
| **#439 — provider resolution** | `UtilitiesCS.Test/OutlookObjects/Folder/OutlookFolderHierarchyProviderTests.cs` | Owns `ResolveLeafKeyAsync` coverage (found / not-found / duplicate-first-match, `:100-192`). 282 lines — ample headroom. |
| **#439 — Efc bind/join/selection** | `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterTests.cs` | Owns the bind-to-document assertions including the `"90%"` join (`:126-136`) and `SelectedFolderPath` (`:214-227`). The percentage-preservation and filing-target ACs from §Q3d belong here. 435 lines — ~65 headroom; watch it. |
| **#439 — Qfc bind** | `UtilitiesCS.Test/OutlookObjects/Folder/FolderBreadcrumbBridgeRouterTests.cs` | Owns `SetSuggestionsAsync` -> chain resolution (`:72-85`). 314 lines. |
| **#440 — Efc transitions** | `UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbRowStateTests.cs` (row model, 334 lines) **and** `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterTests.cs` (router routing + provider query) | The state machine and its routing are separately covered today; keep that split. |
| **#440 — Qfc transitions** | `UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbStateModelTests.cs` (320 lines) **and** `UtilitiesCS.Test/OutlookObjects/Folder/FolderBreadcrumbBridgeRouterTests.cs` | `BreadcrumbStateModelSelectorTests.cs` / `BreadcrumbStateModelSequenceTests.cs` are the #400 selector-session and multi-message-sequence files — use them only if the change touches the selector session (which per #400 AC-9 it should not). |
| **#440 — html contract** | `QuickFiler.Test/Viewers/FolderBreadcrumbAssetContractTests.cs` | The landed arrow-message assertions at `:359-367` must be amended in lockstep with any `onArrow` change. 405 lines. |
| *(Not needed)* | `FolderPredictorTests.cs` / `FolderPredictorCoverageExpansionTests.cs` | Only relevant if the fix lands in `FolderPredictor` — which §Q3e rejects. `FolderPredictorTests.cs` is already **985 lines**, far over the 500 limit; adding to it would deepen an existing violation. |

### Q5b. `Compile Include` audit — **no project-file edit is required**

Verified present:

- `QuickFiler.Test/QuickFiler.Test.csproj:58` — `<Compile Include="Controllers\BreadcrumbBridgeRouterQueueTests.cs" />`
- `QuickFiler.Test/QuickFiler.Test.csproj:59` — `<Compile Include="Controllers\BreadcrumbBridgeRouterTests.cs" />`
- `QuickFiler.Test/QuickFiler.Test.csproj:95` — `<Compile Include="Viewers\FolderBreadcrumbAssetContractTests.cs" />`
- `UtilitiesCS.Test/UtilitiesCS.Test.csproj:279` — `BreadcrumbRowStateTests.cs`
- `:282` `FolderBreadcrumbBridgeRouterEdgeTests.cs`, `:283` `FolderBreadcrumbBridgeRouterTests.cs`,
  `:284` `...ReplaceItemsTests.cs`, `:285` `...InFlightTests.cs`, `:286` `FolderBreadcrumbRouterSelectionConcurrencyTests.cs`
- `:290` `BreadcrumbStateModelTests.cs`, `:291` `BreadcrumbStateModelSequenceTests.cs`, `:296` `BreadcrumbStateModelSelectorTests.cs`
- `:301` `OutlookFolderHierarchyProviderTests.cs`
- `:313-318` all six `OutlookObjects\Folder\Fakes\*.cs`
- `:396` `FolderPredictorCoverageExpansionTests.cs`, `:397` `FolderPredictorTests.cs`

**Every named file is already included. No `.csproj` edit is needed** unless the plan adds a *new* file.

If a new file becomes necessary (most likely a `BreadcrumbBridgeRouterQueueTests.Part2.cs` for headroom), its
`Compile Include` goes in `QuickFiler.Test.csproj` immediately after line 58, and — per #400 AC-17
(`spec.md:255`) — every added test `.cs` **must** be explicitly included in the legacy `.csproj`.

### Q5c. Existing fake/mock patterns, reusable verbatim

**`IFolderHierarchyProvider` — three established patterns, all Moq (no hand-rolled fake exists):**

1. *Loose mock, path-echoing key* — `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterTests.cs:77-106`
   (`SetupProviderChain`), quoted below. Note `ResolveLeafKeyAsync` returns `Key(path)` for **any** input, so
   this mock cannot reproduce the #439 mismatch; a #439 test must set up a *path-form-sensitive* mock:
   ```csharp
   _provider
       .Setup(p => p.ResolveLeafKeyAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
       .ReturnsAsync((string path, CancellationToken ct) => Key(path));
   _provider
       .Setup(p => p.GetAncestorChainAsync(It.IsAny<FolderTreeNodeKey>(), It.IsAny<CancellationToken>()))
       .ReturnsAsync(chain);
   _provider
       .Setup(p => p.GetImmediateSubfoldersAsync(It.IsAny<FolderTreeNodeKey>(), It.IsAny<CancellationToken>()))
       .ReturnsAsync(new[] { ProviderSegment(leafPath + "\\Kid", "Kid", false) });
   ```
2. *Strict mock, exact-path setups* — `UtilitiesCS.Test/OutlookObjects/Folder/FolderBreadcrumbBridgeRouterTests.cs:51-70`
   (`ProviderMock`), using `new Mock<IFolderHierarchyProvider>(MockBehavior.Strict)` and per-path `Setup`s.
   **This is the right pattern for #439**: with `MockBehavior.Strict`, resolving the *wrong* path form throws
   rather than silently succeeding, which makes the RED test fail for the intended reason.
3. *Null-returning resolve* — `BreadcrumbBridgeRouterQueueTests.cs:329-333`, the existing "unresolved leaf key
   falls back to a single-segment row" arrangement — the exact state the #439 fix must move away from.

**Real provider over a mocked `IOutlookFolderTreeService`** —
`UtilitiesCS.Test/OutlookObjects/Folder/OutlookFolderHierarchyProviderTests.cs:231-280`:

```csharp
private static Mock<IOutlookFolderTreeService> ServiceReturning(FolderTreeSnapshot snapshot)
{
    var service = new Mock<IOutlookFolderTreeService>();
    service
        .Setup(s =>
            s.GetSnapshotAsync(It.IsAny<FolderTreeRequest>(), It.IsAny<CancellationToken>())
        )
        .ReturnsAsync(snapshot);
    return service;
}

private static FolderTreeSnapshotNode Node(
    FolderTreeNodeKey key,
    string displayName,
    FolderTreeNodeKey parentKey,
    params FolderTreeNodeKey[] childKeys
)
{
    return new FolderTreeSnapshotNode(
        key, displayName, key.StoreId, key.EntryId, parentKey,
        key.FolderPath, displayName, childKeys, false, string.Empty
    );
}
```

**Caution for the #439 test:** this helper passes `displayName` as the `relativePath` argument (the seventh
parameter, `:275`). That is fine for the existing tests but is *not* a realistic relative path. A #439 test
must construct nodes with a realistic full path (`\\store\Archive\Projects\Alpha`) so the suffix-match rule is
exercised against the real shape, and must include a **decoy** node (`\\store\Inbox\Projects\Alpha`) to pin
the uniqueness requirement.

**`IBreadcrumbWebHost`** — Moq only, no hand-rolled fake. The canonical arrangement is quoted in full in
§Q1d. The `Raise` idiom (`BreadcrumbBridgeRouterQueueTests.cs:201`) is the only way to exercise the async-void
boundary.

**`UtilitiesCS.Test/OutlookObjects/Folder/Fakes/`** contains exactly six hand-rolled fakes, none of which is a
breadcrumb or hierarchy-provider fake:
`FakeDeadlineClock.cs`, `FakeDispatcherYield.cs`, `FakeFolderHandleResolver.cs`,
`FakeFolderHierarchyRecord.cs`, `FakeOutlookFolderHierarchyReader.cs`, `FakeOutlookFolderNotificationSink.cs`.
They serve the folder-tree **reader/service** layer, not the breadcrumb layer. Do not add a new fake here
without justification; Moq is the established pattern for all three breadcrumb seams.

### Q5d. `FolderBreadcrumb.html` test coverage — **yes, and it is deterministic and browser-free**

`QuickFiler.Test/Viewers/FolderBreadcrumbAssetContractTests.cs` (405 lines) asserts against the **compiled
resource string**:

```csharp
private static readonly string Html = QuickFiler.Properties.Resources.FolderBreadcrumb;   // :19
```

It uses `FluentAssertions` `Should().Contain(...)` and `Should().MatchRegex(...)` over that string — no
browser, no WebView2, no JS engine. Representative assertions: `:22-32` (self-contained + theme-aware),
`:35-45` (collapsed mode), `:359-367` (the Left/Right arrow message contract). Resource wiring is confirmed at
`QuickFiler/Properties/Resources.Designer.cs:184`.

**How a #440 html regression test asserts deterministically:** amend
`LeftAndRightBreadcrumbMessages_RemainSupported` (`:359-367`) — or add a sibling `[TestMethod]` in the same
file — asserting the *new* `onArrow` gating text with `MatchRegex`, and asserting the *absence* of the
superseded predicate. The repo precedent for capturing a JS function body and asserting within it is
`:340-357` (a named `(?<body>...)` regex group). That is the pattern to copy.

The Efc document has **no** equivalent asset-contract test; its JS lives in
`BreadcrumbDocumentAssets.BridgeJs` (a `const string`) and is asserted only indirectly through
`BreadcrumbHtmlRendererTests.cs` / the router tests' `_navigated[0].Should().Contain(...)` assertions
(e.g. `BreadcrumbBridgeRouterTests.cs:132-135`).

---

## Q6 — Risks and cross-feature notes

### Q6a. Fixes that appear to require writing an unowned file

| Unowned file | Why a fix appears to need it | In-ownership alternative |
|---|---|---|
| `UtilitiesCS/OutlookObjects/Folder/BreadcrumbRowBuilder.cs` (feature-neutral; potential forbids heuristics here) | The probability join key (`joinPath`, `:131`) and the single-segment fallback (`:121-129`) both live here. | **Do not write it.** Alias the score keys at the `BindRowsAsync` boundary in `BreadcrumbBridgeRouter.cs` (owned) — §Q3e item 2. `BuildProbabilityIndex` is last-write-wins over the supplied enumerable (`:217-224`), so aliasing is sufficient and the builder's "derives no hierarchy from row text" contract is untouched. **I do not believe writing this file is required.** |
| `QuickFiler/Controllers/EfcFormController.cs` (feature 464) | `SelectedFolder` (`:294`), the router construction (`:843-849`), the provider construction (`:840-842`), and `BindFolderRows` (`:873-883`) all live here. | Not required for any of the four defects. #499's fix is confined to the router (§Q2b). #439's fix is confined to the provider + router (§Q3e). **Cross-feature note only:** feature 464 must be told that after #499, `SelectedFolder` can now return `null` immediately after a rebind, and that `IsValidSelection` must tolerate `null`. |
| `UtilitiesCS/OutlookObjects/Folder/BreadcrumbDocumentAssets.cs` (unowned) | The Efc surface's segment gestures live in `BridgeJs` (`:59-89`). #439 part B ("clicking a non-leaf segment navigates to the ancestor") and any Efc arrow-message change require editing it. | **No in-ownership alternative exists.** Recommend **descoping #439 part B** (the ancestor-click gesture) from this feature and recording it as a cross-feature note / follow-up issue. #439 part A (lineage resolution) is fully achievable in owned files and is the high-severity half. |
| `UtilitiesCS/OutlookObjects/Folder/BreadcrumbHtmlRenderer.cs` (unowned) | The Efc separator glyph is `&gt;` here (`:149`), not `→`. | **No in-ownership alternative.** Descope the glyph change; record as a cross-feature note. |
| `UtilitiesCS/OutlookObjects/Folder/BreadcrumbSelectionMap.cs` (unowned) | `GetSelectedFolder` returns `row.Chain[last].FolderPath` for a suggestion row (`:109`). Once #439 makes Qfc chains resolve, the Qfc **filing target** flips from stem to full path — the same hazard as §Q3d, on a file we cannot fix. | Partial: `FolderBreadcrumbBridgeRouter.cs` **is** owned, and `CreateFallbackRow` / `ReplaceRowsPreservingSession` run there. Whether the stem can be preserved through `BreadcrumbStateRow` without touching `BreadcrumbSelectionMap.cs` is **unverified** — the planner must read `BreadcrumbStateRow` and `CreateFallbackRow` before committing. **If it cannot, the safe scoping is to apply the #439 provider fix but gate the Qfc consumption**, or to raise it as a blocking cross-feature dependency. This is the single largest open risk in the feature. |
| `QuickFiler/Controllers/KbdActions.cs` (feature 444) | Not reached by any of the four defects. `KeyboardHandler.cs` (owned) is the arrow fall-through site; `KbdActions.cs` is a different file. | None needed. |
| `UtilitiesCS/OutlookObjects/Folder/IFolderHierarchyProvider.cs` (unowned) | #440 might tempt a new `GetParentAsync`-style member. | **Not needed.** `GetImmediateSubfoldersAsync` + `ResolveLeafKeyAsync` are sufficient (§Q4d), and the ancestor chain is already materialized on the row. |

### Q6b. Nullable-annotation implications

`/p:TreatWarningsAsErrors=true` promotes `CS86xx` to errors **only in files carrying `#nullable enable`**.
Verified line 1 of each owned file:

| Owned file | `#nullable enable` |
|---|---|
| `QuickFiler/Controllers/BreadcrumbBridgeRouter.cs` | **Yes** (`:1`) |
| `QuickFiler/Controllers/KeyboardHandler.cs` | **No** — outside nullable enforcement |
| `UtilitiesCS/OutlookObjects/Folder/FolderPredictor.cs` | **Yes** (`:1`) |
| `UtilitiesCS/OutlookObjects/Folder/OutlookFolderHierarchyProvider.cs` | **Yes** (`:1`) |
| `UtilitiesCS/OutlookObjects/Folder/BreadcrumbStateModel.cs` | **Yes** (`:1`) |
| `UtilitiesCS/OutlookObjects/Folder/BreadcrumbRow.cs` | **Yes** (`:1`) |
| `UtilitiesCS/OutlookObjects/Folder/FolderBreadcrumbBridgeRouter.cs` | **Yes** (`:1`) |
| `QuickFiler/Resources/FolderBreadcrumb.html` | n/a |

Practical consequences:
- The #498 guard replaces `message.SegmentIndex!.Value` (`BreadcrumbBridgeRouter.cs:169`). Removing the `!` in
  favour of a `HasValue` check is *safer* under nullable analysis, not riskier.
- The #499 clear assigns `SelectedFolderPath = null` — the property is already `string?` (`:58`) and the event
  is `EventHandler<string?>?` (`:61`), so no annotation change is needed.
- The #439 provider change adds a second query over `snapshot.NodesByKey.Values`; the return type is already
  `FolderTreeNodeKey?`. No new nullable surface.
- Per the repo's CLAUDE.md, do **not** add `/p:Nullable=enable` to the msbuild command; the CI command is the
  authority and this repo has no `Directory.Build.props`.

### Q6c. File-size risk against the 500-line limit

Line counts at HEAD (`rg ^` count):

| Owned file | Lines | Headroom | Risk |
|---|---:|---:|---|
| `QuickFiler/Controllers/BreadcrumbBridgeRouter.cs` | 450 | 50 | **HIGH** — #498 guard, #499 clear, #439 presented-text map + score aliasing, and #440 Efc transitions **all land here**. This file will very likely exceed 500. |
| `UtilitiesCS/OutlookObjects/Folder/FolderBreadcrumbBridgeRouter.cs` | 485 | 15 | **VERY HIGH** — the #440 Qfc transitions land here. Almost any addition breaches the limit. |
| `UtilitiesCS/OutlookObjects/Folder/BreadcrumbStateModel.cs` | 457 | 43 | **HIGH** — #440 Qfc `RightArrow`/`LeftArrow` rewrite plus a selected-node field. |
| `QuickFiler/Resources/FolderBreadcrumb.html` | 489 | 11 | **HIGH** — #440 `onArrow` gating change. (The 500-line exemption covers Markdown, not html resources.) |
| `QuickFiler/Controllers/KeyboardHandler.cs` | 414 | 86 | Low. |
| `UtilitiesCS/OutlookObjects/Folder/BreadcrumbRow.cs` | 265 | 235 | Low. |
| `UtilitiesCS/OutlookObjects/Folder/OutlookFolderHierarchyProvider.cs` | 98 | 402 | None — reinforces §Q3e. |
| `UtilitiesCS/OutlookObjects/Folder/FolderPredictor.cs` | 983 | **already over by 483** | **Pre-existing violation.** Any write here inherits it. Another argument against the FolderPredictor option in §Q3e. |

Unowned files touched only for reading: `EfcFormController.cs` = 1086 lines (also already over);
`BreadcrumbRowBuilder.cs` = 236; `BreadcrumbHtmlRenderer.cs` = 225; `BreadcrumbDocumentAssets.cs` = 118.

Test files: `BreadcrumbBridgeRouterQueueTests.cs` = 446 (54 headroom, and it carries **two** of the four
regression tests); `BreadcrumbBridgeRouterTests.cs` = 435; `FolderBreadcrumbAssetContractTests.cs` = 405;
`BreadcrumbRowBuilderTests.cs` = 366; `BreadcrumbRowStateTests.cs` = 334; `BreadcrumbStateModelTests.cs` = 320;
`FolderBreadcrumbBridgeRouterTests.cs` = 314; `OutlookFolderHierarchyProviderTests.cs` = 282;
`FolderPredictorTests.cs` = **985 (already over)**.

**Mitigation with in-repo precedent:** `FolderBreadcrumbBridgeRouter.cs` already has a partial sibling,
`FolderBreadcrumbBridgeRouter.SearchPresentation.cs` (`UtilitiesCS/UtilitiesCS.csproj:629-630`), and
`FolderBreadcrumbBridgeRouterTests.cs` is declared `public sealed partial class` (`:24`) with named partial
siblings. The plan should **pre-authorize partial-class splits** for `BreadcrumbBridgeRouter.cs`,
`FolderBreadcrumbBridgeRouter.cs`, and `BreadcrumbStateModel.cs`, each with its own `Compile Include`, rather
than discovering the 500-line breach mid-execution. Note a partial split creates a **new** file, which needs a
`.csproj` entry (`UtilitiesCS.csproj` for the UtilitiesCS files, `QuickFiler.csproj` for the router).

---

## Recommended intra-feature sequencing

1. **#498 (segment-index guard) first.** Smallest, fully self-contained in `BreadcrumbBridgeRouter.cs:168-174`,
   no interaction with any other defect, and the highest severity (host-process termination). Landing it first
   also establishes the `_host.Raise` regression-test pattern the plan reuses.
2. **#499 (clear `SelectedFolderPath` on rebind) second.** Also confined to `BreadcrumbBridgeRouter.cs`
   (`:114`, `:364-380`), also independent, and it must land **before** #439 because #439 changes how
   `SelectedFolderPath` is derived — sequencing it second keeps the two changes to that assignment separable
   and separately bisectable.
3. **#439 (lineage resolution) third.** It is the largest change (provider suffix-match + router presented-text
   map + score aliasing), it carries the two regression hazards of §Q3d, and it is the prerequisite for #440.
   Land part A (lineage) only; descope part B (ancestor-click gesture) and the `→` glyph, both of which need
   unowned files.
4. **#440 (Left/Right tree navigation) last.** It depends on #439 producing multi-segment rows (§Q4f — before
   #439 both transitions are permanent no-ops), it is the only defect that contradicts a landed acceptance
   criterion (#400 AC-9, §Q4c) and therefore needs an explicit spec decision before any task is written, and it
   is the change most likely to force the partial-class splits of §Q6c.

---

## Open items the spec must resolve (not resolvable by research)

1. **#400 AC-9 supersession.** Does #440 retract `spec.md:247`? If yes, state it; if no, #440 cannot proceed as
   scoped.
2. **Qfc `unhandledArrow` fall-through disposition.** Retained, re-gated, or removed — and if removed, by what
   gesture the Pop Out / Enumerate Conversation dialog stays reachable (`KeyboardHandler.cs:302-310`).
3. **Boundary contract, per direction, per surface.** Left at root and Right on a childless node currently
   behave *differently* on Efc vs Qfc (§Q4e). Pick one.
4. **Qfc filing-target preservation after #439.** Whether the stem can be preserved through
   `FolderBreadcrumbBridgeRouter` (owned) without writing `BreadcrumbSelectionMap.cs:109` (unowned) is
   **unverified** and is the largest open risk (§Q6a).
5. **#439 part B and the `→` glyph descope.** Both require `BreadcrumbDocumentAssets.cs` /
   `BreadcrumbHtmlRenderer.cs`, neither owned. Confirm they become follow-up issues.
6. **`EfcFormController.IsValidSelection` null tolerance** after #499 (§Q2c) — read-only verification the
   planner should do before finalizing the #499 AC.
