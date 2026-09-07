# Archive-root projection and breadcrumb lineage (Issue #799)

- Date: 2026-09-06T18-20
- Worktree: `<repo-root>/.claude/worktrees/<agent-worktree>` on `bug/breadcrumb-lineage-below-archive-root-799` (branched from `origin/main` at `c431dc32`)
- Scope: research only. No source, config, or project file was modified.
- Every file path below is repository-relative and was read in THIS worktree. Line numbers are
  from the files as they exist here, not copied from `issue.md`.

---

## R1. `ArchiveStemContract`

**Location confirmed**: `UtilitiesCS/OutlookObjects/Folder/ArchiveStemContract.cs` (147 lines, `#nullable enable`,
`public static class`, namespace `UtilitiesCS.OutlookObjects.Folder`).

**Compile entry**: `UtilitiesCS/UtilitiesCS.csproj:623` — `<Compile Include="OutlookObjects\Folder\ArchiveStemContract.cs" />`

### Full public surface (three members, no others)

```csharp
public static bool IsFullOutlookPath(string value)                                  // :41
public static void RequireArchiveRelativeStem(string value, string paramName)       // :68
public static bool TryMakeArchiveRelative(string fullPath, string archiveRoot, out string stem)  // :106
```

Private constants only: `BackslashSeparator = '\\'` (:20), `ForwardSeparator = '/'` (:21).

### `TryMakeArchiveRelative` exact semantics (read from :106-145)

| Aspect | Verified behavior | Evidence |
|---|---|---|
| Anchoring | Prefix-anchored via `StartsWith`; never a `Contains` or `Replace` | `:131` `!fullPath.StartsWith(root, StringComparison.OrdinalIgnoreCase)` |
| Comparison | Ordinal, case-INsensitive on both the equality and the prefix test | `:124`, `:131` both `StringComparison.OrdinalIgnoreCase` |
| Separator termination | The character at `root.Length` must be `\` or `/`, else `false` | `:137-141` |
| Trailing separators on root | Trimmed before use (`\` and `/`) | `:118` `archiveRoot.TrimEnd(BackslashSeparator, ForwardSeparator)` |
| Null / empty / whitespace `archiveRoot` | Returns `false`, `stem = string.Empty`. Also `false` when the root is nothing but separators (`root.Length == 0` after trim) | `:113-122` |
| Null / empty `fullPath` | Returns `false`, `stem = string.Empty` | `:113` |
| Path EQUALS the root | Returns **`true`** with `stem = string.Empty` | `:124-127` |
| Path not under root | Returns `false` and yields `string.Empty` — it never passes the input through | `:112`, `:129-141` |
| `Archive2` false prefix | `\\mbx\Archive2\X` against root `\\mbx\Archive` -> `fullPath[root.Length] == '2'`, not a separator -> `false` | `:137-141` |
| Leading separator on the stem | Stripped (`TrimStart` of both separator chars), so the stem never leads with `\` | `:143` |
| Purity | No I/O, no COM, no logging, no per-call regex allocation (documented at `:13-16`) | class doc |

### Which of the seven drifted sites the contract can serve as-is

| Site | Servable as-is? | Note |
|---|---|---|
| (a) `FolderPredictor.ProjectSuggestionPath` | Yes | Needs a thin wrapper that returns the input unchanged on `false`, to preserve the current display behavior for out-of-root paths |
| (b) `QfcItemController.ProjectPredeterminedFolder` | Yes | Same wrapper |
| (c) `FolderPredictor.AddRecents` / `AddRecentRows` | Yes | Same wrapper applied per recent entry |
| (d) `FolderPredictor.GetOlSubpath` | **Partially** | Only the `includeChildren == true` branch is a stem strip. The `false` branch returns the LEAF NAME, which the contract does not compute |
| (e) `FolderMinimalWrapper.ToRelativePath` | Yes technically, but see R2(e) — a caller depends on the divergence |
| (f) `FolderWrapper.LoadRelativePath` | Same as (e) |
| (g) `SortItemsToExistingFolder` | N/A — the file is not compiled (R2(g)) |

### Is a NEW contract method needed?

**Yes, one — but as a separate small type, not as a new member on `ArchiveStemContract`.**

`ArchiveStemContract` is deliberately a hard boundary type: on failure it yields empty and never
passes the input through (`:95-97`). Every display site (a), (b), (c) needs the OPPOSITE fallback
(return the input unchanged so an unexpected value still renders). Bolting a
"lenient" overload onto the strict contract would blur the very invariant #614 created it for.

Recommended new pure helper, in a NEW file so `FolderPredictor.cs` (1003 lines, already double the
500-line ceiling) does not grow:

```csharp
// UtilitiesCS/OutlookObjects/Folder/ArchiveStemProjection.cs
public static class ArchiveStemProjection
{
    /// <summary>Display projection: the archive-relative stem when the path is strictly UNDER the
    /// root; otherwise the input unchanged. An empty/whitespace root yields the input unchanged
    /// (this is what eliminates the AC4 one-separator strip).</summary>
    public static string ToDisplayStem(string folderPath, string archiveRoot);
}
```

`ToDisplayStem` must treat "path equals root" as **not** projectable (`TryMakeArchiveRelative`
returns `true` with an empty stem there, and an empty display row is worse than the full path),
so the implementation is `TryMakeArchiveRelative(...) && stem.Length > 0 ? stem : folderPath`.
That reproduces the current `folderPath.Length > archivePrefix.Length` guard at
`FolderPredictor.cs:858` exactly.

A second new pure helper is needed for AC1/AC2 chain trimming — see R3.

---

## R2. The seven drifted archive-root stripping sites

### (a) `FolderPredictor.ProjectSuggestionPath` — CONVERT

`UtilitiesCS/OutlookObjects/Folder/FolderPredictor.cs:848-861`. Issue citation (848-861) is CORRECT.

```csharp
private string ProjectSuggestionPath(string folderPath)
{
    if (_globals is null)
    {
        return folderPath;
    }

    var archivePrefix = _globals.Ol.ArchiveRootPath + "\\";
    return
        folderPath.StartsWith(archivePrefix, StringComparison.OrdinalIgnoreCase)
        && folderPath.Length > archivePrefix.Length
            ? folderPath.Substring(archivePrefix.Length)
            : folderPath;
}
```

Divergences from `ArchiveStemContract.TryMakeArchiveRelative`:
1. Backslash only. A `/`-separated root or path is not matched.
2. Appends `"\\"` unconditionally, so a root already ending in `\` produces the prefix `...\\` and
   never matches.
3. With an EMPTY root the prefix becomes `"\"`, so ANY path leading with a separator has exactly one
   leading separator stripped — the AC4 defect.
4. Correct anchoring and ordinal-ignore-case otherwise; the `Archive2` boundary is handled by
   accident because the prefix carries the trailing `\`.

**Callers**: `FolderPredictor.cs:810` (`AddSuggestions`) and `FolderPredictor.cs:842`
(`AddSuggestionRows`). Both are display paths. CONVERT to `ArchiveStemProjection.ToDisplayStem`.

### (b) `QfcItemController.ProjectPredeterminedFolder` — CONVERT

`QuickFiler/Controllers/QfcItemController.FolderHandling.cs:272-285`. Issue citation (272-285) is CORRECT.

```csharp
internal static string ProjectPredeterminedFolder(string folderPath, string archiveRootPath)
{
    if (string.IsNullOrEmpty(folderPath) || archiveRootPath is null)
    {
        return folderPath;
    }

    string archivePrefix = archiveRootPath + "\\";
    return
        folderPath.StartsWith(archivePrefix, StringComparison.OrdinalIgnoreCase)
        && folderPath.Length > archivePrefix.Length
            ? folderPath.Substring(archivePrefix.Length)
            : folderPath;
}
```

Identical stripping expression to (a); the only differences are the null guards documented in the
XML comment at `:252-271`. Sole caller: `QuickFiler/Controllers/QfcItemController.FolderHandling.cs:231-234`
inside `AssignFolderComboBox`. Its own comment at `:228-230` states the duplication exists only
because `ProjectSuggestionPath` is private — that reason disappears once the shared helper is
public. CONVERT; keep the member as a one-line delegation so
`QuickFiler.Test/Controllers/QfcItemController.FolderHandlingTests.Part2.cs:213-241` keeps a target.

Note the existing test at `:220-223` pins the EMPTY-root one-separator strip. AC4 explicitly
eliminates that behavior, so that assertion must be updated as part of the change, not preserved.

### (c) `FolderPredictor.AddRecents` — CONVERT

`UtilitiesCS/OutlookObjects/Folder/FolderPredictor.cs:788-795`. Issue citation is CORRECT.

```csharp
public void AddRecents(ref List<string> folderList) // internal
{
    if (_globals.AF.RecentsList.Count > 0)
    {
        folderList.Add("======= RECENT SELECTIONS ========");
        folderList.AddRange(_globals.AF.RecentsList);
    }
}
```

No projection at all. **The issue under-reports this site**: the row-model mirror
`AddRecentRows` at `FolderPredictor.cs:866-882` has the same gap
(`rows.Add(new FolderRow(recent, FolderRowKind.Recent, null))` at `:879`) and is the one the
breadcrumb surfaces actually consume. AC5 requires BOTH be projected, otherwise the string list and
the row list disagree — and `FolderRowArray`'s XML doc at `:233-242` asserts they are text-identical.
CONVERT both.

### (d) `FolderPredictor.GetOlSubpath` — CONVERT the `includeChildren == true` branch only

`UtilitiesCS/OutlookObjects/Folder/FolderPredictor.cs:953-971`. Issue citation is CORRECT.

```csharp
public string GetOlSubpath(string path, string olAncestor, bool includeChildren)
{
    if (includeChildren)
    {
        if (olAncestor.EndsWith('\\'.ToString()))
        {
            return path.Substring(olAncestor.Length);
        }
        else
        {
            return path.Substring(olAncestor.Length + 1);
        }
    }
    else
    {
        var pathParts = path.Substring(olAncestor.Length).Split(@"\");
        return pathParts[pathParts.Count() - 1];
    }
}
```

Divergences: no prefix verification at all. `path` is assumed to start with `olAncestor`. If it does
not, the result is a garbage substring; if `path.Length <= olAncestor.Length` it throws
`ArgumentOutOfRangeException`. Case is irrelevant because no comparison happens. `Archive2` is
mis-stripped silently.

`olAncestor` here is a SEARCH root, not necessarily the archive root — `LoopFolders` at `:911-913`
falls back to `_globals.Ol.ArchiveRootPath` but `FindFolder` at `:314-326` passes each entry of the
caller-supplied `emailSearchRoots`. `TryMakeArchiveRelative` is root-agnostic (its parameter is only
NAMED `archiveRoot`), so it serves this site.

**Callers**: `FolderPredictor.cs:918` and `:935` (both inside `LoopFolders`, which only ever passes
strict descendants), plus the public test caller
`ToDoModel.Test/Email Utilities/FolderHandlerTests_Written.cs:46,63`. Those two tests pin
`("\\\\email@company.com\\Folder 1\\Folder 2\\Folder 3", "\\\\email@company.com", true) -> "Folder 1\\Folder 2\\Folder 3"`
and the `false` variant `-> "Folder 3"`; both survive a contract-based rewrite unchanged.
CONVERT the `true` branch; leave the `false` branch's leaf-name computation alone (it is a different
function and the contract does not compute it).

### (e) `FolderMinimalWrapper.ToRelativePath` — LEAVE ALONE

`UtilitiesCS/OutlookObjects/Folder/FolderMinimalWrapper.cs:56-86` (issue cites `:84`, which is the
`Replace` line specifically — correct, but the member spans 56-86).

```csharp
return OlFolder.FolderPath.Replace(OlRoot.FolderPath + "\\", "");   // :84
```

Divergences: unanchored `Replace` (removes EVERY occurrence anywhere in the path, not just a
prefix); case-SENSITIVE (`string.Replace(string,string)` is ordinal case-sensitive on .NET
Framework); no `Archive2` protection beyond the appended `\`; and on `OlRoot is null` (:58-65) or a
non-containing path (:74-81) it returns the FULL path, only logging a warning.

**Reason to leave alone**: a caller depends on the full-path fallback.
`FolderMinimalWrapper.RestoreFromRelativePath` at `:88-131` has an explicit branch
`if (RelativePath.StartsWith("\\\\"))` (`:105-114`) that walks the COM parent chain to recover a
folder whose stored `RelativePath` is a FULL store path. That branch exists only because
`ToRelativePath` can return a rooted value. Converting to the contract (which yields empty on
failure) would silently break that restore path, and `RelativePath` is `[JsonProperty]`-persisted,
so the change touches serialized data. No AC1-AC8 covers it.

### (f) `FolderWrapper.LoadRelativePath` — LEAVE ALONE

**On-disk filename, character for character**: `UtilitiesCS/OutlookObjects/Folder/FolderWrapper .cs`
— `FolderWrapper`, one U+0020 SPACE, then `.cs`. Confirmed by `Glob` output and by the csproj entry
at `UtilitiesCS/UtilitiesCS.csproj:824`.

The issue names the member `FolderWrapper.RelativePath` at `:194-224`. **The citation's line span is
correct but the member name is wrong**: `RelativePath` is the `[JsonProperty]` accessor at
`:187-192`; the stripping lives in `internal virtual string? LoadRelativePath()` at `:194-224`. The
body is character-identical to (e), including `return OlFolder.FolderPath.Replace(OlRoot.FolderPath + "\\", "");`
at `:222`.

**Reasons to leave alone**:
1. The file is **532 lines** — already over the repository's 500-line ceiling. Any edit invites a
   split that is out of scope here.
2. The full-path fallback feeds the persisted classifier corpus through
   `UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.cs:597`
   (`.UnTrain(helper.FolderInfo!.RelativePath, ...)`) and
   `QuickFiler/Controllers/QfcFormController.Actions.cs:250`. Changing the value shape changes
   training data. That is a genuine defect but a DIFFERENT one, with no AC in #799.

Recommend promoting (e)+(f) as a follow-up issue rather than folding them into #799.

### (g) `SortItemsToExistingFolder` — LEAVE ALONE (not compiled)

**On-disk directory name, character for character**: `ToDoModel/Email Utilities/` — `Email`, one
U+0020 SPACE, `Utilities`. File: `ToDoModel/Email Utilities/SortItemsToExistingFolder.cs` (403 lines).

The issue cites `:67-109`. Verified content at those lines:

```csharp
if (string.IsNullOrEmpty(StrRoot)) { StrRoot = _globals.Ol.ArchiveRootPath; }   // :65-68
loc = StrRoot + @"\";                                                          // :70
...
else if (folderCurrent.FolderPath.Contains(StrRoot) & (folderCurrent.FolderPath != StrRoot))  // :90-92
...
strTemp2 = _globals.Ol.ArchiveRootPath.Substring(_globals.Ol.EmailRootPath.Length);  // :109
```

Divergences: `Contains` (unanchored, case-sensitive), bitwise `&` instead of `&&`, and a blind
`Substring` against a DIFFERENT root (`EmailRootPath`, not the archive root).

**Decisive reason to leave alone: the file is not compiled.**
`ToDoModel/ToDoModel.csproj` is a non-SDK-style project (`<Project ToolsVersion="15.0" xmlns="http://schemas.microsoft.com/developer/msbuild/2003">`
at `:2`) with explicit `<Compile Include>` items and no globbing. Its complete `Email Utilities`
membership is one line:

```xml
<Compile Include="Email Utilities\CaptureEmailAddressesModule.cs" />   <!-- ToDoModel.csproj:145 -->
```

`SortItemsToExistingFolder.cs` (and `CaptureEmailDetailsModule.cs`) appear only in the stale
`ToDoModel/ToDoModel.csproj.bak:138`.

**Searches run**: (1) `Grep` for `SortItemsToExistingFolder\.cs` restricted to `*.csproj` -> "No matches found";
(2) `Grep` for `MASTER_SortEmailsToExistingFolder|SortItemsToExistingFolder` repo-wide -> the only
non-doc hits are the source file itself, `ToDoModel.csproj.bak:138`, and test files that reference
the class name only inside comments (`ToDoModel.Test/Email Utilities/SortItemsToExistingFolderTests.cs:62`
is a commented-out call). There is no live caller of `MASTER_SortEmailsToExistingFolder` anywhere.

Adding the file back to the build to fix it would introduce a new compile unit with ~7 unrelated
compiler problems (unused locals, `null` defaults on value contexts) and is unquestionably out of
scope.

### Additional site found by the cross-check search, NOT named in the issue

`UtilitiesCS/OutlookObjects/MailItem/MailItemHelper.Loading.cs:122-130`:

```csharp
internal static Folder ResolveFolderRoot(IApplicationGlobals appGlobals, string folderPath)
{
    if (folderPath.Contains(appGlobals.Ol.ArchiveRootPath)) { return appGlobals.Ol.ArchiveRoot; }
    return appGlobals.Ol.Inbox;
}
```

This is an unanchored archive-root COMPARISON, not a stripper — it selects which `OlRoot` is handed
to the wrappers in (e)/(f). Recorded for completeness; out of scope for #799.

---

## R3. Chain trimming below the archive root (AC1, AC2)

### What the chain is and where it starts

`FolderTreeSnapshotQueries.GetAncestorChain` — `UtilitiesCS/OutlookObjects/Folder/FolderTreeSnapshotQueries.cs:109-146`.
The issue cites `:109-140`; the member actually runs to `:146`. Behavior:

```csharp
while (current != null && visited.Add(current.Key))
{
    chain.Add(current);
    if (current.ParentKey == null || !snapshot.TryGetNode(current.ParentKey, out var parent)) { break; }
    current = parent;
}
chain.Reverse();
```

**The snapshot DOES contain the store/mailbox node.** `OutlookFolderHierarchyReader.ReadStoreAsync`
(`UtilitiesCS/OutlookObjects/Folder/OutlookFolderHierarchyReader.cs:117-160`) pushes the store root
folder itself (`stack.Push(Tuple.Create(root, string.Empty, root.FolderPath))` at `:127`) with an
empty `parentEntryId`, and `ToNode` (`:177-204`) resolves `parentKey` to `null` for it. So
`ParentKey == null` identifies exactly the store root, and the walk terminates there. This is why the
rendered lineage begins `<mailbox> -> Archive -> ...`.

`BreadcrumbRowBuilder.MapSegments` — `UtilitiesCS/OutlookObjects/Folder/BreadcrumbRowBuilder.cs:178-208`
(issue cites `:178-208`, CORRECT). It is a pure 1:1 map with no trimming and no archive-root
knowledge.

### What identifies the archive-root node

Not an EntryID, not `ParentKey == null`, and not a literal child named "Archive". The reliable
identifier is **`FolderTreeSnapshotNode.FolderPath` compared ordinal-case-insensitively against
`IOlObjects.ArchiveRootPath`, with trailing separators trimmed** — i.e. exactly
`ArchiveStemContract.TryMakeArchiveRelative`'s root handling.

Evidence that the two strings are the same shape:
- `AppOlObjects.ResolveValidatedArchiveRootPath()` (`TaskMaster/AppGlobals/AppOlObjects.ArchiveRoot.cs:86-93`)
  computes `Path.Combine(Root.FolderPath, "Archive")` and requires it to equal
  `ArchiveRoot.FolderPath` via `ArchiveRootPathGuard.RequireResolvedArchiveRoot`. It throws rather
  than returning null (`:60-64`, and `TaskMaster/AppGlobals/AppOlObjects.cs:260-269` caches it).
- `OutlookFolderHierarchyProvider.ResolveLeafKeyAsync` (`:70-72`) already matches presented paths
  against `node.FolderPath`, and `BreadcrumbBridgeRouter.ToHierarchyPath`
  (`QuickFiler/Controllers/BreadcrumbBridgeRouter.cs:152-167`) builds `_boundRoot + "\\" + stem` for
  that same comparison. The two are interoperable today.

`FolderTreeSnapshotNode.RelativePath` is NOT usable: `OutlookFolderHierarchyReader.GetRelativePath`
(`:206-211`) makes it relative to the STORE root and uses the same unanchored
`Replace(rootPath + "\\", "")` pattern as (e)/(f).

### Where to apply the trim — smallest diff AND best sibling isolation

**Recommendation: `OutlookFolderHierarchyProvider.GetAncestorChainAsync`
(`UtilitiesCS/OutlookObjects/Folder/OutlookFolderHierarchyProvider.cs:34-42`, file is 141 lines).**

```csharp
public async Task<IReadOnlyList<FolderBreadcrumbSegment>> GetAncestorChainAsync(
    FolderTreeNodeKey leafKey, CancellationToken cancellationToken)
{
    var snapshot = await AcquireSnapshotAsync(cancellationToken).ConfigureAwait(false);
    var chain = FolderTreeSnapshotQueries.GetAncestorChain(snapshot, leafKey);
    return MapNodes(chain);
}
```

Rationale, in order of weight:

1. **It is the only point both surfaces share.** The QuickFiler drop-down reaches the chain through
   `FolderBreadcrumbBridgeRouter.SetSuggestionsAsync` (`UtilitiesCS/.../FolderBreadcrumbBridgeRouter.cs:52-60`)
   and the Efc list through `BreadcrumbBridgeRouter.FetchChainAsync`
   (`QuickFiler/Controllers/BreadcrumbBridgeRouter.Selection.cs:50-80`). Both call
   `_provider.GetAncestorChainAsync`. One change satisfies AC1 on both.
2. **It keeps this item's diff entirely off the sibling's files** — see R9. Trimming inside
   `FolderBreadcrumbBridgeRouter.SetSuggestionsAsync` would require a new field and a constructor
   parameter on the exact type the sibling owns.
3. `GetImmediateSubfoldersAsync` (`:45-53`) is unaffected: subfolders are always below the leaf, so
   below the archive root by construction.
4. The AC7 log site (`ResolveByUniqueSuffix`, `:90-114`) is in the same 141-line file, so AC2 and
   AC7 land together.

**Data the trim needs that is not available at that call site**: only the archive root string. The
provider is constructed with `IOutlookFolderTreeService` alone (`:28-31`). Supply it as an OPTIONAL
second constructor parameter of type `Func<string>` (a lazy accessor, NOT an eagerly-read string):

- Lazy is mandatory. `IOlObjects.ArchiveRootPath` THROWS `InvalidOperationException` when the root is
  unresolvable (`AppOlObjects.ArchiveRoot.cs:35-39`), and
  `QuickFiler.Test/Controllers/EfcFormControllerTests.Part2.cs:242`
  (`BindBreadcrumbRowsAsync_WhenArchiveRootThrows_ReportsOnceAndDoesNotThrow`) exists specifically
  because of that. Reading it eagerly at construction creates a NEW throw site inside
  `EfcFormController.ConfigureBreadcrumbControl` and `QfcItemController.EnsureBreadcrumbPipeline`,
  neither of which is inside a try.
- Optional (defaulting to `null` = no trim) keeps all 20 existing test constructions in
  `UtilitiesCS.Test/OutlookObjects/Folder/OutlookFolderHierarchyProviderTests.cs` and
  `UtilitiesCS.Test/OutlookObjects/Folder/FolderHierarchyProviderAdapterTests.cs` compiling
  unchanged, including `new OutlookFolderHierarchyProvider(null)` at
  `OutlookFolderHierarchyProviderTests.cs:316` (a single `null` argument binds unambiguously to the
  first parameter).

Put the trim itself in a NEW pure static so it is unit-testable without a provider or a snapshot:

```csharp
// UtilitiesCS/OutlookObjects/Folder/ArchiveChainProjection.cs
public static bool TryTrimBelowArchiveRoot(
    IReadOnlyList<FolderBreadcrumbSegment> chain,
    string archiveRoot,
    out IReadOnlyList<FolderBreadcrumbSegment> trimmed);
```

Implementation: find the first index `i` where
`ArchiveStemContract.TryMakeArchiveRelative(chain[i].FolderPath, archiveRoot, out var s) && s.Length == 0`
(that is the root node itself, since `TryMakeArchiveRelative` returns `true` with an empty stem on
exact equality, `:124-127`). Return `chain[(i+1)..]`. Return `false` when no such index exists, or
when `i` is the last index (the leaf IS the root, so there is nothing to render below it).

### Detecting a chain that does NOT pass through the archive root, and the fallback

Detection is exactly the `false` return above: no chain element's `FolderPath` equals the archive
root. `OutlookFolderHierarchyProvider` then emits `logger.Error(...)` — the same `log4net.ILog`
already declared at `:17-19` — and returns `Array.Empty<FolderBreadcrumbSegment>()`.

**The existing single-segment fallback code path, named exactly:**

- **Efc surface**: `BreadcrumbRowBuilder.BuildRow`, `UtilitiesCS/OutlookObjects/Folder/BreadcrumbRowBuilder.cs:122-131`:
  ```csharp
  IReadOnlyList<BreadcrumbSegment> segments = MapSegments(ancestorChain);
  if (segments.Count == 0)
  {
      // Unknown/empty chain fallback: render the presented path as a single
      // leaf-only segment so the suggestion stays visible and selectable.
      segments = new[] { new BreadcrumbSegment(presentedText, LeafToken(presentedText), false) };
  }
  ```
  An EMPTY (non-null) chain reaches this branch: `BreadcrumbBridgeRouter.BindRowsAsync:126-129`
  stores any non-null chain in `chains`, and `MapSegments` returns empty for an empty input
  (`BreadcrumbRowBuilder.cs:182-185`). `LeafToken` (`:231-236`) yields the last path segment, so the
  row renders as a genuine SINGLE segment. Confirmed correct.

- **QuickFiler surface**: `FolderBreadcrumbBridgeRouter.CreateFallbackRow`,
  `UtilitiesCS/OutlookObjects/Folder/FolderBreadcrumbBridgeRouter.cs:247-257`, selected by the
  `chain != null && chain.Count > 0` ternary at `:61-71`. It produces the scored-fallback row
  (`new BreadcrumbStateRow(identity, row.Score.Value.FolderPath, row.Score.Value.Probability)`,
  which sets `IsScoredFallback = true` via `BreadcrumbStateModel.Row.cs:112-113`).

  **Caveat the planner must know**: on this surface the fallback is NOT single-segment.
  `BreadcrumbRenderProjection.ProjectRow` (`UtilitiesCS/OutlookObjects/Folder/BreadcrumbRenderProjection.cs:176-178`)
  routes non-`IsSuggestion` rows through `SplitVerbatim` (`:242-246`), which splits on `\`. A
  fallback for `_Active Projects\Build RGF Org and Team\Sales Lead` therefore renders as three
  segments, not one. This is the EXISTING behavior and it already satisfies AC2's real requirement
  ("never with a mailbox prefix"), because the verbatim text is the archive-relative stem. AC2's
  phrase "existing single-segment fallback" is literally true only on the Efc surface. Do not
  "fix" the QFC surface to one segment — that would be a new regression against the search-row
  rendering that AC1 says must match.

---

## R4. AC6 — the Efc score join

### Read chain

- `EfcFormController.BindBreadcrumbRowsAsync` — `QuickFiler/Controllers/EfcFormController.cs:1111-1128`.
  Issue cites `:1115-1118`; the member spans 1111-1128, and the two load-bearing lines are
  `:1115-1117` and `:1118`. Correct region.
  ```csharp
  var scores = _dataModel?.FolderHelper?.Suggestions?.ToScoredArray() ?? Array.Empty<FolderScore>();
  await _router.BindRowsAsync(rows, scores, _globals.Ol.ArchiveRootPath, Token);
  ```
- `BreadcrumbBridgeRouter.BindRowsAsync` (internal 4-arg) — `QuickFiler/Controllers/BreadcrumbBridgeRouter.cs:92-150`.
- `BreadcrumbRowBuilder.BuildRows` -> `BuildProbabilityIndex` -> `BuildRow` —
  `UtilitiesCS/OutlookObjects/Folder/BreadcrumbRowBuilder.cs:35-60`, `:210-229`, `:133-135`.
- `FolderPredictor.AddSuggestionRows` — `UtilitiesCS/OutlookObjects/Folder/FolderPredictor.cs:835-846`.
  Issue cites `:835-846`. CORRECT.

### How the percentage is lost

`AddSuggestionRows` projects BOTH the row text and the score:

```csharp
var folderPath = ProjectSuggestionPath(score.FolderPath);                       // :842
var projectedScore = new FolderScore(folderPath, score.Score, score.Probability);  // :843
rows.Add(new FolderRow(folderPath, FolderRowKind.Suggestion, projectedScore));     // :844
```

`BindBreadcrumbRowsAsync` does not use that row model. It takes `rows` (a `string[]` that came from
`FolderPredictor.FolderArray`, i.e. already projected by `ProjectSuggestionPath` at `:810`) and
pairs them with `Suggestions.ToScoredArray()` — the **raw, unprojected** scorer output.

`BreadcrumbRowBuilder.BuildProbabilityIndex` keys the dictionary on `score.FolderPath` (`:222-224`),
and `BuildRow` looks up `probabilityByPath.TryGetValue(presentedText, out double p)` (`:133`). The
comparer is `StringComparer.OrdinalIgnoreCase` (`:219`), so case is not the problem — the KEY is.
For an archive-rooted suggestion the key is `\\<mailbox>\Archive\Forums\Pricing` while
`presentedText` is `Forums\Pricing`. Lookup misses, `probability` is `null`, and
`PercentageFormatter.FormatPercent(null)` renders an empty `.pct` cell
(`UtilitiesCS/OutlookObjects/Folder/BreadcrumbHtmlRenderer.cs:205-211`).

### Minimal correct fix, and which is smaller/safer

Two candidates:

**(A) Project the score paths at `EfcFormController.cs:1115-1117`.** One statement, but
`EfcFormController.cs` is **1320 lines** — already 2.6x the 500-line ceiling. Every line added there
worsens a standing violation.

**(B) Project the score paths inside `BreadcrumbBridgeRouter.BindRowsAsync`, using `_boundRoot`.**
RECOMMENDED. `QuickFiler/Controllers/BreadcrumbBridgeRouter.cs` is **304 lines**, has room, and
already computes and normalizes the root at `:107-109`:
```csharp
_boundRoot = string.IsNullOrWhiteSpace(archiveRootPath) ? string.Empty : archiveRootPath.TrimEnd('\\', '/');
```
and already consumes `ArchiveStemContract` at `:157` and `:164`. Insert, immediately before the
`_builder.BuildRows(...)` call at `:132`, a projection of each `FolderScore.FolderPath` through
`ArchiveStemProjection.ToDisplayStem(path, _boundRoot)`.

Why (B) is safer as well as smaller:
- The public 3-arg overload (`:75-82`) forwards `string.Empty`, so `_boundRoot` is empty and
  `ToDisplayStem` is the identity. No existing caller of the public overload changes behavior; that
  covers every `BindRowsAsync(rows, scores, token)` call in `QuickFiler.Test`.
- It fixes the join for ALL callers of the internal overload, not just the one in
  `EfcFormController`.
- It leaves `EfcFormController.cs` untouched, which matters because that file cannot absorb growth.

Keying the join on something other than presented text was considered and rejected: `BuildRows`
takes only `IReadOnlyList<string>` plus `IEnumerable<FolderScore>` (`:35-39`), so there is no
correlating identity to key on without changing that public signature and every test that calls it
(`UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbRowBuilderTests.cs` plus 12 router test files).

---

## R5. AC7 — stale-label logging and distinguishable rendering

### The log site

`OutlookFolderHierarchyProvider.ResolveByUniqueSuffix` —
`UtilitiesCS/OutlookObjects/Folder/OutlookFolderHierarchyProvider.cs:90-114`. The issue cites
`:85-112`; the correct span is `:90-114`, with the emission at `:108-112`:

```csharp
logger.Error(
    candidates.Length == 0
        ? $"No snapshot node path ends with '{suffix}'; leaving '{folderPath}' unresolved."
        : $"Multiple snapshot node paths end with '{suffix}'; leaving '{folderPath}' unresolved."
);
return null;
```

### Runtime evidence (redacted)

`<taskmaster-bin>/logs/debug_2026-09-06.log` contains **18** occurrences of that error family across
one session, but only **two distinct labels**: `Scorecards\Monthly Scans` and `Forums\Pricing`.
Confirms once-per-render, not once-per-label. All 18 are on the `VSTA_Main` thread.

The log also shows a second, richer failure the issue did not report: some entries carry a suffix of
`'\\\<mailbox>\Archive\Forums\Pricing'` — i.e. a FULL rooted path reached
`ResolveByUniqueSuffix`. That is the Efc route: `ToHierarchyPath`
(`QuickFiler/Controllers/BreadcrumbBridgeRouter.cs:152-167`) re-roots a relative presented target to
`_boundRoot + "\\" + target`, `ResolveLeafKeyAsync`'s exact-path match fails, and the suffix pass is
then handed a rooted string. Both surfaces log; the gate must cover both.

### Where a once-per-label-per-session gate can live

Inside `OutlookFolderHierarchyProvider` as a private instance field, next to the existing
`_treeService` (`:21`). The existing logging pattern in this file and assembly is:

```csharp
private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(
    System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);   // :17-19
```
(the same shape as `FolderWrapper .cs`, `FolderMinimalWrapper.cs`, and
`QuickFiler/Controllers/BreadcrumbBridgeRouter.cs:21-23`, which names its field `log`).

**State required**: a set of already-reported labels. `ResolveByUniqueSuffix` is currently `private
static`, so the gate forces it to become an instance member (or the set becomes static).

**Concurrency and lifetime — this matters because the item is scheduled against siblings:**
- The provider is a per-surface instance (`EfcFormController.cs:1053`,
  `QfcItemController.ViewerSetup.cs:147`), created once per form/viewer. An INSTANCE set therefore
  scopes to the surface, not the session. "Per session" in AC7 is satisfied more literally by a
  `static` set — but a static set is process-wide mutable state shared across every QuickFiler
  viewer instance and across test methods in the same assembly, which breaks the repository's test
  independence rule (`.claude/rules/general-unit-test.md`, "Tests must not rely on mutable global
  state").
- Recommendation: **instance-scoped `HashSet<string>` guarded by a `lock`, injected-clock-free**, and
  spell out in the spec that "per session" is realized as "per provider instance". Calls are
  observed on `VSTA_Main` in the log, but `GetAncestorChainAsync`/`ResolveLeafKeyAsync` are `async`
  and `AcquireSnapshotAsync` awaits, so continuations are not guaranteed to be on one thread —
  `ConcurrentDictionary<string, byte>` with `TryAdd`, or a `lock`ed `HashSet`, is required. A bare
  `HashSet` is not safe here.
- Lifetime: the provider is referenced by the router, which the coordinator/controller disposes with
  the viewer. No leak beyond viewer lifetime. Note that QuickFiler viewers are POOLED
  (`Reset()`/`Clear()` on `BreadcrumbBridgeCoordinator.cs:100-110`), but the provider is recreated
  only when `viewer.BreadcrumbCoordinator == null` (`QfcItemController.ViewerSetup.cs:145-151`), so a
  pooled viewer keeps its provider and its gate across items — which is the desired behavior.

### "Rendered distinguishably" — is there an existing flag or CSS class?

**No. A new field on the row DTO and its JSON contract IS required.** Stated plainly.

Verified absences:
- Efc row model `BreadcrumbRow` (`UtilitiesCS/OutlookObjects/Folder/BreadcrumbRow.cs:34-80`) carries
  `RowId`, `Kind`, `Segments`, `Probability`, `FilingTarget`, `ActiveSegmentIndex` — no
  unresolved/fallback flag.
- Efc renderer emits a fixed class string for every suggestion:
  `sb.Append("<div class=\"row selectable suggestion\">...")`
  (`UtilitiesCS/OutlookObjects/Folder/BreadcrumbHtmlRenderer.cs:142`). `BreadcrumbDocumentAssets.BaseCss`
  (`:17-29`) defines `.rows .row .crumb .pct .rowwrap .row.selectable .seg .sep .affordance
  .row.banner .children .child` — no stale/unresolved class.
- QFC render DTO `BreadcrumbRowRender` (`UtilitiesCS/OutlookObjects/Folder/BreadcrumbRenderProjection.cs:74-120`)
  exposes `RowIndex, IsSuggestion, Selected, Collapsed, LeafExpanded, PercentText, Cells, Subfolders`
  — no flag. `RowToJson` (`UtilitiesCS/OutlookObjects/Folder/BreadcrumbBridgeMessages.cs:315-336`)
  and `ParseRows` (`:350-367`) mirror exactly those eight fields.
- QFC page CSS (`QuickFiler/Resources/FolderBreadcrumb.html:100-158`) defines
  `.row .row.selected .row.active .crumbs .seg .seg.trunc .arrow .aff .pct .subs .sub .sub.selected`
  — no stale class.

One piece of luck: the QFC state model ALREADY carries the signal.
`BreadcrumbStateRow.IsScoredFallback` (`UtilitiesCS/OutlookObjects/Folder/BreadcrumbStateModel.Row.cs:139`)
is `true` exactly for an unresolved scored suggestion, and `BreadcrumbRenderProjection.ProjectRow`
already reads it at `:230` and `:234`. So the QFC side needs no router change to SOURCE the flag —
only to PUBLISH it.

**Every file that would have to change for "rendered distinguishably" on both surfaces:**

QuickFiler drop-down surface:
1. `UtilitiesCS/OutlookObjects/Folder/BreadcrumbRenderProjection.cs` — add `IsUnresolved` to
   `BreadcrumbRowRender` (ctor + property), set from `row.IsScoredFallback` at `:228-239`.
2. `UtilitiesCS/OutlookObjects/Folder/BreadcrumbBridgeMessages.cs` — `RowToJson` (`:315-336`) writes
   the field; `ParseRows` (`:350-367`) reads it. This file is 463 lines.
3. `QuickFiler/Resources/FolderBreadcrumb.html` — CSS class plus `rowElement.className` composition
   in `makeRow` (`:280-283`). This file is 491 lines.
4. `QuickFiler.Test/Viewers/FolderBreadcrumbAssetContractTests.cs` — the asset contract test asserts
   against the compiled resource string (`:19`), so it must be extended.

Efc list surface:
5. `UtilitiesCS/OutlookObjects/Folder/BreadcrumbRow.cs` — add the flag.
6. `UtilitiesCS/OutlookObjects/Folder/BreadcrumbRowBuilder.cs` — set it in the `segments.Count == 0`
   branch at `:124-131`.
7. `UtilitiesCS/OutlookObjects/Folder/BreadcrumbHtmlRenderer.cs` — emit the class at `:142`.
8. `UtilitiesCS/OutlookObjects/Folder/BreadcrumbDocumentAssets.cs` — the CSS rule.

That is 8 files for the "distinguishable" half of AC7 alone. AC7 offers "(or filtered)" as an
alternative; filtering the unresolved label out of the presented row set is a 1-file change and is
defensible, because a label that resolves to no snapshot node names a folder that no longer exists
and therefore cannot be filed to. Recommend the planner make this an explicit spec decision rather
than assuming the 8-file path.

---

## R6. AC8 — the leading underscore. DEFINITE CONCLUSION

**Conclusion reached: NO code path alters a leading underscore. The space in `_ Active Projects` was
introduced by the maintainer's transcription, not by the renderer.**

There are TWO independent breadcrumb documents in this repository, and both were traced end to end.

### Path 1 — QuickFiler item-view drop-down (the surface in the repro steps)

Document: `QuickFiler/Resources/FolderBreadcrumb.html` (static asset), loaded via
`QuickFiler.Properties.Resources.FolderBreadcrumb` at
`QuickFiler/Viewers/ItemViewer.Breadcrumb.cs:118` and `:202`.

1. C# projection: `BreadcrumbRenderProjection.ProjectRow`
   (`UtilitiesCS/OutlookObjects/Folder/BreadcrumbRenderProjection.cs:176-205`). Segment text is
   either `segment.DisplayName` verbatim or `SplitVerbatim(row.VerbatimText)` (`:242-246`), which is
   `string.Split(new[]{'\\'}, RemoveEmptyEntries)`. `Split` removes separators only; it inserts
   nothing. `_Active Projects` contains no `\` and survives as one part.
2. JSON: `BreadcrumbBridgeSerializer.Serialize` -> `RowToJson`
   (`UtilitiesCS/OutlookObjects/Folder/BreadcrumbBridgeMessages.cs:252-336`) builds a Newtonsoft
   `JObject` and emits `root.ToString(Formatting.None)` (`:309`). Newtonsoft escapes `"`, `\`, and
   control characters only; `_` and `U+0020` are emitted literally and `Formatting.None` adds no
   whitespace inside string values.
3. JS render: `makeCell` at `FolderBreadcrumb.html:248-258` sets
   `element.textContent = cell.text;` and `element.title = cell.text;`. `textContent` performs no
   entity decoding, no escaping, and no transformation.
4. CSS: the full stylesheet is `FolderBreadcrumb.html:10-159`. `.seg` (`:123-128`) is
   `flex / overflow:hidden / text-overflow:ellipsis / white-space:nowrap`. There is no
   `letter-spacing`, no `word-spacing`, no `text-transform`, no `::first-letter`, and no
   `word-break` anywhere in the file. The font is `"Segoe UI", sans-serif` at 13px (`:54-55`).

### Path 2 — Efc view

Document generated server-side by `BreadcrumbHtmlRenderer.RenderDocument`
(`UtilitiesCS/OutlookObjects/Folder/BreadcrumbHtmlRenderer.cs:32-52`).

1. Segment text is emitted by `AppendSegment` (`:175-190`):
   `.Append(WebUtility.HtmlEncode(segment.DisplayName))`. `WebUtility.HtmlEncode` encodes
   `& < > "` and non-ASCII above the encoder threshold. `_` (U+005F) and space (U+0020) are passed
   through unchanged; it emits no `&nbsp;`.
2. CSS: `BreadcrumbDocumentAssets.BaseCss/LightThemeCss/DarkThemeCss`
   (`UtilitiesCS/OutlookObjects/Folder/BreadcrumbDocumentAssets.cs:17-41`) — 15 rules total, none of
   which is `letter-spacing`, `word-spacing`, `text-transform`, `::first-letter`, or `word-break`.
3. `BridgeJs` (`:49-145`) inserts child names with `div.textContent = c.displayName;` (`:136`) and
   row HTML with `innerHTML = msg.html` (`:127`) where `msg.html` is the already-encoded
   renderer output.

### Repository-wide negative search

`Grep` over `{UtilitiesCS,QuickFiler,ToDoModel,TaskMaster,Tags,TaskVisualization}/**/*.{cs,html,css,js}`
for the pattern `Replace\("_|Replace\('_|letter-spacing|text-transform|first-letter|word-break|word-spacing`
returned **"No files found"**. There is no code anywhere in the product that rewrites an underscore
or applies a spacing/casing transform to rendered text.

### Transformations inspected and explicitly ruled out

`string.Split` (removes separators only) - Newtonsoft string escaping (`\`, `"`, control chars only) -
`WebUtility.HtmlEncode` (`& < > "` only) - JS `textContent` assignment (no entity decoding) -
`WinForms` mnemonic prefixing (the WinForms prefix character is `&`, not `_`, so no combo-box or
owner-draw path can be responsible either) - CSS `letter-spacing` / `word-spacing` /
`text-transform` / `::first-letter` / `word-break` (none present in either document).

**AC8 therefore requires no code change. The correct outcome is to record the verification and close
AC8 as "verified, renderer does not alter the leading underscore".**

---

## R7. Test inventory

All paths are repository-relative and were confirmed to EXIST via `Glob` and via their `<Compile>`
entries.

| Subject | Test file | Project | Status |
|---|---|---|---|
| `ArchiveStemContract` | `UtilitiesCS.Test/OutlookObjects/Folder/ArchiveStemContractTests.cs` | UtilitiesCS.Test | Exists (`csproj:281`) |
| `FolderPredictor` incl. `GetOlSubpath` | `UtilitiesCS.Test/OutlookObjects/Folder/FolderPredictorTests.cs` (`:577-591` covers `GetOlSubpath`) | UtilitiesCS.Test | Exists (`csproj:401`) |
| `FolderPredictor` (coverage fill) | `UtilitiesCS.Test/OutlookObjects/Folder/FolderPredictorCoverageExpansionTests.cs` | UtilitiesCS.Test | Exists (`csproj:400`) |
| `FolderPredictor.GetOlSubpath` (second suite) | `ToDoModel.Test/Email Utilities/FolderHandlerTests_Written.cs` | ToDoModel.Test | Exists (`csproj:74`) |
| `FolderPredictor.ProjectSuggestionPath` (direct) | — | — | **ABSENT**. Member is private; only indirectly exercised. Searched: `Grep` for `ProjectSuggestionPath` across `*.cs` -> only comments in `QfcItemController.FolderHandlingTests.Part2.cs:150,248` |
| `FolderPredictor.AddRecents` / `AddRecentRows` (direct) | — | — | **ABSENT**. `Grep` for `AddRecents` in `*.cs` produced no test call |
| `BreadcrumbRowBuilder` | `UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbRowBuilderTests.cs` | UtilitiesCS.Test | Exists (`csproj:280`) |
| `FolderTreeSnapshotQueries` (general) | `UtilitiesCS.Test/OutlookObjects/Folder/FolderTreeSnapshotQueriesTests.cs` | UtilitiesCS.Test | Exists (`csproj:314`) |
| `FolderTreeSnapshotQueries.GetAncestorChain` | `UtilitiesCS.Test/OutlookObjects/Folder/FolderTreeSnapshotQueriesAncestorChainTests.cs` | UtilitiesCS.Test | Exists (`csproj:303`) |
| `QfcItemController` folder handling | `QuickFiler.Test/Controllers/QfcItemController.FolderHandlingTests.cs` and `.Part2.cs` (`Part2:213-241` pins `ProjectPredeterminedFolder`) | QuickFiler.Test | Exists (`csproj:181,182`) |
| `QfcItemController` suggestions | `QuickFiler.Test/Controllers/QfcItemController.FolderSuggestionsTests.cs` | QuickFiler.Test | Exists |
| `EfcFormController` breadcrumb binding | `QuickFiler.Test/Controllers/EfcFormControllerTests.cs` (`:62-127`) and `EfcFormControllerTests.Part2.cs` (`:236-259`) | QuickFiler.Test | Exists (`csproj:121,122`) |
| Efc router bind/join | `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterTests.cs`, `...QueueTests.cs`, `...QueueTests.Part2.cs`, `...Issue439Tests.cs`, `...Issue439Tests.Activation.cs`, `...Issue614Tests.cs`, `...Issue637Tests.cs`, `...Tests.Selection.cs` | QuickFiler.Test | Exist |
| `OutlookFolderHierarchyProvider` | `UtilitiesCS.Test/OutlookObjects/Folder/OutlookFolderHierarchyProviderTests.cs` | UtilitiesCS.Test | Exists (`csproj:304`) |
| `IFolderHierarchyProvider` adapter | `UtilitiesCS.Test/OutlookObjects/Folder/FolderHierarchyProviderAdapterTests.cs` | UtilitiesCS.Test | Exists (`csproj:302`) |
| `FolderMinimalWrapper` | `UtilitiesCS.Test/OutlookObjects/Folder/FolderMinimalWrapperTests.cs` | UtilitiesCS.Test | Exists (`csproj:354`) |
| `FolderWrapper` | `UtilitiesCS.Test/OutlookObjects/Folder/FolderWrapperCoverageExpansionTests.cs`, `FolderWrapperStateTests.cs`, `FolderWrapperTraversalTests.cs`, plus five comparer suites | UtilitiesCS.Test | Exist (`csproj:403` and neighbours) |
| `SortItemsToExistingFolder` | `ToDoModel.Test/Email Utilities/SortItemsToExistingFolderTests.cs` and `SortItemsToExistingFolderTests_Unfinished.cs` | ToDoModel.Test | Files exist (`csproj:75,76`) but exercise **nothing** in the production type: the class under test is not compiled, and the only reference (`SortItemsToExistingFolderTests.cs:62`) is commented out. The `_Unfinished` class is named `Disabled_...` (`:12`) |
| `BreadcrumbRenderProjection` | `UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbRenderProjectionTests.cs`, `BreadcrumbRenderProjectionSelectorTests.cs` | UtilitiesCS.Test | Exist (`csproj:291,292`) |
| `BreadcrumbHtmlRenderer` | `UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbHtmlRendererTests.cs` | UtilitiesCS.Test | Exists (`csproj:278`) |
| QFC page asset contract | `QuickFiler.Test/Viewers/FolderBreadcrumbAssetContractTests.cs` | QuickFiler.Test | Exists (`csproj:107`) |
| `FolderBreadcrumbBridgeRouter` (sibling-owned) | `UtilitiesCS.Test/OutlookObjects/Folder/FolderBreadcrumbBridgeRouterTests.cs`, `...EdgeTests.cs`, `...InFlightTests.cs`, `...ReplaceItemsTests.cs`, `FolderBreadcrumbRouterSelectionConcurrencyTests.cs` | UtilitiesCS.Test | Exist (`csproj:285-289`) |
| `ArchiveStemProjection` (new) | — | — | ABSENT by definition; to be created |
| `ArchiveChainProjection` (new) | — | — | ABSENT by definition; to be created |

---

## R8. Project compile entries (all non-SDK-style; every `.cs` must be listed)

### `UtilitiesCS/UtilitiesCS.csproj`

```xml
<Compile Include="OutlookObjects\Folder\ArchiveStemContract.cs" />          <!-- :623 -->
<Compile Include="OutlookObjects\Folder\BreadcrumbRowBuilder.cs" />        <!-- :625 -->
<Compile Include="OutlookObjects\Folder\OutlookFolderHierarchyProvider.cs" />  <!-- :640 -->
<Compile Include="OutlookObjects\Folder\FolderTreeSnapshotQueries.cs" />   <!-- :650 -->
<Compile Include="OutlookObjects\Folder\FolderMinimalWrapper.cs" />        <!-- :728 -->
<Compile Include="OutlookObjects\Folder\FolderPredictor.cs" />             <!-- :808 -->
<Compile Include="OutlookObjects\Folder\FolderWrapper .cs" />              <!-- :824 -->
```

**`FolderWrapper` IS listed**, under the exact Include string `OutlookObjects\Folder\FolderWrapper .cs`
— note the single space before `.cs`, written literally with NO quoting, escaping, or `%20`.
Any new file added here follows the same one-line self-closing form.

### `QuickFiler/QuickFiler.csproj`

```xml
<Compile Include="Controllers\BreadcrumbBridgeRouter.cs" />                <!-- :291 -->
<Compile Include="Controllers\BreadcrumbBridgeRouter.Arrows.cs" />        <!-- :292 -->
<Compile Include="Controllers\BreadcrumbBridgeRouter.Selection.cs" />     <!-- :293 -->
<Compile Include="Controllers\EfcFormController.cs" />                    <!-- :296 -->
<Compile Include="Controllers\QfcItemController.FolderHandling.cs" />     <!-- :338 -->
```

### `ToDoModel/ToDoModel.csproj`

```xml
<Compile Include="Email Utilities\CaptureEmailAddressesModule.cs" />      <!-- :145 -->
```

That single line is the project's ENTIRE `Email Utilities` membership (verified by reading
`:118-152`, the complete `Compile` ItemGroup). It also demonstrates the space-in-path form for a
DIRECTORY: `Email Utilities\...`, unquoted and unescaped.

**`SortItemsToExistingFolder` is NOT listed.** Not in `ToDoModel.csproj`, and not in any other
`.csproj` (`Grep` for `SortItemsToExistingFolder\.cs` with `glob: *.csproj` -> "No matches found").
The only occurrence is `ToDoModel/ToDoModel.csproj.bak:138`, a backup file that MSBuild never reads.

### `UtilitiesCS.Test/UtilitiesCS.Test.csproj`

```xml
<Compile Include="OutlookObjects\Folder\ArchiveStemContractTests.cs" />                  <!-- :281 -->
<Compile Include="OutlookObjects\Folder\BreadcrumbRowBuilderTests.cs" />                 <!-- :280 -->
<Compile Include="OutlookObjects\Folder\FolderTreeSnapshotQueriesAncestorChainTests.cs" /> <!-- :303 -->
<Compile Include="OutlookObjects\Folder\OutlookFolderHierarchyProviderTests.cs" />       <!-- :304 -->
<Compile Include="OutlookObjects\Folder\FolderPredictorTests.cs" />                      <!-- :401 -->
```

### `QuickFiler.Test/QuickFiler.Test.csproj`

```xml
<Compile Include="Controllers\EfcFormControllerTests.cs" />                    <!-- :121 -->
<Compile Include="Controllers\QfcItemController.FolderHandlingTests.cs" />     <!-- :181 -->
<Compile Include="Viewers\FolderBreadcrumbAssetContractTests.cs" />            <!-- :107 -->
```

### `ToDoModel.Test/ToDoModel.Test.csproj` (space-in-directory form for tests)

```xml
<Compile Include="Email Utilities\FolderHandlerTests_Written.cs" />            <!-- :74 -->
<Compile Include="Email Utilities\SortItemsToExistingFolderTests.cs" />        <!-- :75 -->
```

---

## R9. Sibling separability

The concurrent sibling owns the QuickFiler folder drop-down OPEN/CLOSE lifecycle and
selection-commit ordering, in the breadcrumb bridge router and the selection session.

| File (repository-relative) | Lines | Classification |
|---|---|---|
| `UtilitiesCS/OutlookObjects/Folder/FolderBreadcrumbBridgeRouter.cs` | 489 | **BOTH** |
| `UtilitiesCS/OutlookObjects/Folder/FolderBreadcrumbBridgeRouter.SearchPresentation.cs` | 97 | LIFECYCLE/SELECTION |
| `UtilitiesCS/OutlookObjects/Folder/BreadcrumbSelectionSession.cs` | — | LIFECYCLE/SELECTION |
| `UtilitiesCS/OutlookObjects/Folder/BreadcrumbSelectionSession.Highlight.cs` | — | LIFECYCLE/SELECTION |
| `QuickFiler/Viewers/BreadcrumbBridgeCoordinator.cs` | 487 (per its own doc comment) | LIFECYCLE/SELECTION |
| `QuickFiler/Viewers/BreadcrumbBridgeCoordinator.Search.cs` | 102 | LIFECYCLE/SELECTION |

**`FolderBreadcrumbBridgeRouter.cs` is BOTH. Stated plainly, with the members on each side:**

- ROW-TEXT-PROJECTION side (this item would touch these if the trim were placed here):
  `SetSuggestionsAsync` (`:29-97`), `SetSuggestionFallbacks` (`:100-119`), `AddPlainRows` (`:156-168`),
  `CreateFallbackRow` (`:247-257`), and the constructor `FolderBreadcrumbBridgeRouter(IFolderHierarchyProvider)`
  (`:19-23`).
- LIFECYCLE/SELECTION side (the sibling's):
  `OpenSelector` (`:185-186`), `MoveSelector` (`:188-189`), `CommitSelector` (`:191-192`),
  `ActivateSelector` (`:194-195`), `ActivateSelectorSubfolder` (`:205-208`), `CancelSelector`
  (`:210-211`), `GetSelectorState` (`:213`), `SelectRow` (`:178-179`), `SelectItem` (`:182-183`),
  `Clear` (`:171-175`), `Mutate` (`:239-245`), `Transition` (`:267-276`),
  `ReplaceRowsPreservingSession` (`:478-482`), and the `_selectionSession` field (`:14`).

The two sides share the `_sync` lock (`:15`), the `_suggestionGeneration` counter (`:16`), and the
constructor. A ctor-signature change from this item would collide directly with any sibling edit.

**This is precisely why the R3 recommendation places the AC1/AC2 trim in
`OutlookFolderHierarchyProvider`.** With that placement, this item touches NONE of the six rows
above. Verify at review time that the final diff contains no hunk in any of them.

Files that could be mistaken for the sibling's but are NOT (they belong to the Efc surface, which has
no open/close lifecycle — the Efc list is always expanded):
- `QuickFiler/Controllers/BreadcrumbBridgeRouter.cs` (304 lines) — ROW-TEXT-PROJECTION for this item
  (`BindRowsAsync`, `ToHierarchyPath`, `AttachSegmentKeys`).
- `QuickFiler/Controllers/BreadcrumbBridgeRouter.Selection.cs` — flagged: it contains `FetchChainAsync`
  (`:50-80`, projection side) alongside Efc row-selection helpers. This item needs no edit there
  under the recommended design, but a reviewer should confirm.
- `QuickFiler/Controllers/BreadcrumbBridgeRouter.Arrows.cs` — LIFECYCLE-adjacent (Efc arrows). Do not
  touch.

---

## Numeric Derivation Evidence

The only enumerable population this research asserts, and which the planner may lift into a spec
acceptance criterion, is the set of archive-root stripping sites named by issue #799.

- **Complete Family**: every production C# member in this repository that derives an archive-relative
  (or ancestor-relative) path by removing a configured root prefix from a full Outlook folder path,
  restricted to the members enumerated by issue #799.
- **Exhaustive Search Scope**: all `.cs` files under `UtilitiesCS/`, `QuickFiler/`, `ToDoModel/`,
  `TaskMaster/`, `Tags/`, `TaskVisualization/`, plus every `*.csproj` in the repository for
  compile-membership. Both records cover every member of the declared family, not one named pattern.
- **Inclusion Rules**: the member must (i) accept or read a root path, (ii) produce a shortened path
  string derived from a longer one, and (iii) be reachable from display, filing, persistence, or
  search code.
- **Exclusion Rules**: root SELECTORS that return a folder rather than a shortened string; the
  contract type itself (`ArchiveStemContract`); test code; documentation; `.bak` files.

**Primary Search Strategy** — member-name enumeration. Query expression:
`Grep pattern "ProjectSuggestionPath|ProjectPredeterminedFolder|GetOlSubpath" glob "*.cs"`, plus
targeted reads of `FolderPredictor.cs:740-990`, `QfcItemController.FolderHandling.cs:200-313`,
`FolderMinimalWrapper.cs:55-115`, `FolderWrapper .cs:180-240`,
`ToDoModel/Email Utilities/SortItemsToExistingFolder.cs:1-403`.

**Primary Member Set** (normalized as `File::Member`):
1. `UtilitiesCS/OutlookObjects/Folder/FolderPredictor.cs::ProjectSuggestionPath`
2. `QuickFiler/Controllers/QfcItemController.FolderHandling.cs::ProjectPredeterminedFolder`
3. `UtilitiesCS/OutlookObjects/Folder/FolderPredictor.cs::AddRecents`
4. `UtilitiesCS/OutlookObjects/Folder/FolderPredictor.cs::GetOlSubpath`
5. `UtilitiesCS/OutlookObjects/Folder/FolderMinimalWrapper.cs::ToRelativePath`
6. `UtilitiesCS/OutlookObjects/Folder/FolderWrapper .cs::LoadRelativePath`
7. `ToDoModel/Email Utilities/SortItemsToExistingFolder.cs::MASTER_SortEmailsToExistingFolder`

**Primary Count**: 7

**Cross-check Search Strategy** — root-symbol occurrence enumeration, a different expression over a
different axis (the root value rather than the member name). Query expression:
`Grep pattern "ArchiveRootPath|OlRoot\.FolderPath|archiveRoot" glob "{UtilitiesCS,QuickFiler,ToDoModel,TaskMaster,Tags,TaskVisualization}/**/*.cs"`,
then applying the inclusion/exclusion rules to each of the 80 returned hits.

**Cross-check Member Set** (normalized identically):
1. `UtilitiesCS/OutlookObjects/Folder/FolderPredictor.cs::ProjectSuggestionPath` (hit at `:855`)
2. `QuickFiler/Controllers/QfcItemController.FolderHandling.cs::ProjectPredeterminedFolder` (hits at `:272,274,279`)
3. `UtilitiesCS/OutlookObjects/Folder/FolderPredictor.cs::AddRecents` (reached via the `_globals.AF.RecentsList` read at `:790,793`; no root symbol appears, which is itself the defect)
4. `UtilitiesCS/OutlookObjects/Folder/FolderPredictor.cs::GetOlSubpath` (root arrives as `olAncestor`, assigned from `ArchiveRootPath` at `:913` and `:751`)
5. `UtilitiesCS/OutlookObjects/Folder/FolderMinimalWrapper.cs::ToRelativePath` (hits at `:66,74,84`)
6. `UtilitiesCS/OutlookObjects/Folder/FolderWrapper .cs::LoadRelativePath` (hits at `:204,212,222`)
7. `ToDoModel/Email Utilities/SortItemsToExistingFolder.cs::MASTER_SortEmailsToExistingFolder` (hits at `:67,91,107,109`)

**Cross-check Count**: 7

**Member-set Comparison**: the normalized primary and cross-check member sets are IDENTICAL — same
seven `File::Member` pairs, no member present in one and absent from the other.

The cross-check additionally surfaced members that the inclusion rules REJECT and that are therefore
not part of the count: `UtilitiesCS/OutlookObjects/MailItem/MailItemHelper.Loading.cs::ResolveFolderRoot`
(returns a `Folder`, not a shortened string — root selector), `QuickFiler/Controllers/EfcDataModel.cs::TryGetArchiveRoot`
(accessor only), `UtilitiesCS/OutlookObjects/Folder/FolderConverter.cs` (already on the #614
contract), `TaskMaster/AppGlobals/ArchiveRootPathGuard.cs` (validator), and
`QuickFiler/Controllers/BreadcrumbBridgeRouter.cs::ToHierarchyPath` (already on the contract, and it
PREFIXES rather than strips). These rejections are recorded so the exhaustive scope is auditable.

**Assertion released**: the archive-root stripping family named by issue #799 has exactly **seven**
members, and of those, four are recommended for CONVERT (a, b, c, d) and three for LEAVE ALONE
(e, f, g).

---

## Behavior semantics (AC-by-AC)

- **AC1** — success: for every Suggestion row and every SearchResult row on both surfaces, the first
  rendered segment is the first folder BELOW the archive root, arrows and clickable ancestors are
  produced by the existing `BreadcrumbRenderProjection` / `BreadcrumbHtmlRenderer` cell pipeline
  unchanged. Failure: any row whose first segment is the mailbox or `Archive`.
  Ordering: trimming happens after chain resolution and before row construction, so row order,
  banner placement, and the trash pseudo-row are untouched.
- **AC2** — success: chain contains a node whose `FolderPath` equals the archive root, and that node
  is not the leaf. Failure paths: (i) no such node -> `logger.Error` once, empty chain, existing
  fallback; (ii) chain equals the root exactly (leaf IS the root) -> same treatment; (iii) empty
  chain from the snapshot -> existing behavior, no new log.
- **AC3** — invariant: `BreadcrumbStateRow.WithFilingTarget` (`BreadcrumbStateModel.Row.cs:77-98`)
  substitutes the presented stem into the LEAF segment's `FolderPath` only. Trimming removes LEADING
  segments, so the leaf and therefore the filing value are untouched. Same for
  `BreadcrumbRow.FilingTarget` (`BreadcrumbRow.cs:63-65`).
- **AC4** — the empty-root one-separator strip disappears because `TryMakeArchiveRelative` returns
  `false` for a whitespace-only root (`ArchiveStemContract.cs:113`).
- **AC5** — both `AddRecents` and `AddRecentRows` project.
- **AC6** — score keys and row texts are projected by the same function against the same root.
- **AC7** — one error per distinct label per provider instance; stale rows rendered
  distinguishably or filtered (spec decision required, see R5).
- **AC8** — verified, no change (R6).

## Testing implications (no test code written)

Consistent with `.claude/rules/general-unit-test.md` and the C# unit test policy (MSTest + Moq +
FluentAssertions, no temp files, no external dependencies):

- `ArchiveStemProjection.ToDisplayStem`: pure table tests over the #614 boundary cases — under root,
  equal to root, `Archive2`, trailing-separator root, empty root, null/empty path, forward-slash
  separators, mixed case.
- `ArchiveChainProjection.TryTrimBelowArchiveRoot`: chain through root; chain NOT through root; chain
  whose leaf IS the root; empty chain; single-element chain; root supplied with a trailing separator.
  All constructible from `FolderBreadcrumbSegment` literals with no snapshot and no COM.
- `OutlookFolderHierarchyProvider`: a `Mock<IOutlookFolderTreeService>` returning a hand-built
  `FolderTreeSnapshot` (the pattern already used throughout
  `UtilitiesCS.Test/OutlookObjects/Folder/OutlookFolderHierarchyProviderTests.cs`) plus a
  `Func<string>` accessor; assert the trimmed chain, and assert the AC7 gate by invoking the same
  unresolvable label twice and counting log emissions through an injected sink or a log4net memory
  appender. Prefer a delegate sink over an appender to keep the test independent of global log4net
  configuration.
- `BreadcrumbBridgeRouter.BindRowsAsync` score projection: existing router test fixtures in
  `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterIssue614Tests.cs` already build rooted/relative
  target pairs; extend with a rooted-score/relative-row case asserting a non-empty `percentText`.
- `FolderPredictor` recents: exercise `FolderRowArray` and `FolderArray` on a predictor whose
  `_globals.AF.RecentsList` holds one rooted and one relative entry; assert text parity between the
  two properties (the parity contract is documented at `FolderPredictor.cs:233-242` and is currently
  unasserted).
- `QfcItemController.FolderHandlingTests.Part2.cs:213-241` must be UPDATED, not preserved: its
  empty-root assertion (`:220-223`) encodes the behavior AC4 removes.
- Integration scenario from the issue (banner + suggestion + search result + trash + stale label)
  can be driven entirely through `BreadcrumbRowBuilder.BuildRows` and
  `BreadcrumbRenderProjection.Project` with no WebView2 and no Outlook.

---

## R10. Write set

### Production sources

`UtilitiesCS/OutlookObjects/Folder/ArchiveStemProjection.cs` (CREATE)
`UtilitiesCS/OutlookObjects/Folder/ArchiveChainProjection.cs` (CREATE)
`UtilitiesCS/OutlookObjects/Folder/OutlookFolderHierarchyProvider.cs` (MODIFY — 141 lines, ample room)
`UtilitiesCS/OutlookObjects/Folder/FolderPredictor.cs` (MODIFY — 1003 lines, already over the ceiling; the change is net line-NEUTRAL or negative because `ProjectSuggestionPath`'s 13-line body collapses to a delegation)
`QuickFiler/Controllers/QfcItemController.FolderHandling.cs` (MODIFY — 313 lines)
`QuickFiler/Controllers/BreadcrumbBridgeRouter.cs` (MODIFY — 304 lines; AC6 score projection)
`QuickFiler/Controllers/EfcFormController.cs` (MODIFY — 1320 lines; ONE argument added at the provider construction on line 1053-1055)
`QuickFiler/Controllers/QfcItemController.BreadcrumbWiring.cs` (CREATE)
`QuickFiler/Controllers/QfcItemController.ViewerSetup.cs` (MODIFY)

**Hard constraint the planner must honour**: `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs`
is **exactly 500 lines** — at the repository ceiling, not near it. The provider construction at
`:147-149` must gain one argument, which CSharpier will format as an additional line (the collapsed
single-line call is ~126 columns including indent, well past the print width). The compensating move
is to relocate `EnsureBreadcrumbPipeline` (`:132-163`, 32 lines including its comment and the
`[ExcludeFromCodeCoverage]` attribute) into the new partial
`QuickFiler/Controllers/QfcItemController.BreadcrumbWiring.cs`, leaving `ViewerSetup.cs` at roughly
468 lines. That relocation is the reason both files appear in the write set.

If AC7 takes the "rendered distinguishably" branch rather than the "filtered" branch, add:

`UtilitiesCS/OutlookObjects/Folder/BreadcrumbRenderProjection.cs`
`UtilitiesCS/OutlookObjects/Folder/BreadcrumbBridgeMessages.cs`
`UtilitiesCS/OutlookObjects/Folder/BreadcrumbRow.cs`
`UtilitiesCS/OutlookObjects/Folder/BreadcrumbRowBuilder.cs`
`UtilitiesCS/OutlookObjects/Folder/BreadcrumbHtmlRenderer.cs`
`UtilitiesCS/OutlookObjects/Folder/BreadcrumbDocumentAssets.cs`
`QuickFiler/Resources/FolderBreadcrumb.html`

### Test sources

`UtilitiesCS.Test/OutlookObjects/Folder/ArchiveStemProjectionTests.cs` (CREATE)
`UtilitiesCS.Test/OutlookObjects/Folder/ArchiveChainProjectionTests.cs` (CREATE)
`UtilitiesCS.Test/OutlookObjects/Folder/OutlookFolderHierarchyProviderTrimTests.cs` (CREATE — a new file rather than growing `OutlookFolderHierarchyProviderTests.cs`)
`UtilitiesCS.Test/OutlookObjects/Folder/FolderPredictorRecentsProjectionTests.cs` (CREATE)
`QuickFiler.Test/Controllers/QfcItemController.FolderHandlingTests.Part2.cs` (MODIFY — the empty-root assertion at `:220-223` encodes behavior AC4 removes)
`QuickFiler.Test/Controllers/BreadcrumbBridgeRouterScoreJoinTests.cs` (CREATE — AC6)

If the "rendered distinguishably" branch is taken, add:

`QuickFiler.Test/Viewers/FolderBreadcrumbAssetContractTests.cs` (MODIFY)
`UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbRenderProjectionTests.cs` (MODIFY)

### Project files

`UtilitiesCS/UtilitiesCS.csproj`
`UtilitiesCS.Test/UtilitiesCS.Test.csproj`
`QuickFiler/QuickFiler.csproj`
`QuickFiler.Test/QuickFiler.Test.csproj`

### Paths containing a SPACE

This item recommends that neither space-containing path be touched, so neither appears in the write
set above. They are restated here in prose, without backticks, so a downstream path extractor does
not misread them as write targets:

- The ToDoModel email-utilities sort file — directory ToDoModel, then the directory named Email
  Utilities (one space between the two words), then the file SortItemsToExistingFolder.cs. NOT to be
  modified: it is not a Compile item in ToDoModel.csproj and has no live caller.
- The folder wrapper source file — directory UtilitiesCS, then OutlookObjects, then Folder, then the
  file named FolderWrapper followed by one space and then .cs. NOT to be modified: 532 lines
  (already over the 500-line ceiling) and its full-path fallback is depended upon by the persisted
  RelativePath restore and the classifier corpus.

Also NOT to be modified, written without backticks for the same reason: the file UtilitiesCS,
OutlookObjects, Folder, FolderMinimalWrapper.cs — its RestoreFromRelativePath branch depends on the
divergent full-path fallback.

### Explicitly out of scope and absent from the write set

Nothing under the dot-claude, dot-codex, or dot-agents trees. Neither of the two published JSON files
under the config directory. No GitHub workflow file. Not the solution file. Not the repository-root
build property files.

**The fix does not require the solution file or any repository-root build property file.** Every new
`.cs` is added to an existing project via an explicit `<Compile Include>` line, and no new project is
introduced.
