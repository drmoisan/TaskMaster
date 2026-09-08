# Research — Issue #812 (consolidates #801, #805)

- Timestamp: 2026-09-07T23-45
- Worktree: `<repo-root>/.claude/worktrees/agent-a7ec162a25bc9de96`
- Branch: `TaskMaster-wt-2026-09-06T17-16`, HEAD `c431dc32`
- Scope: Defect A (unguarded archive-root read in the `FolderPredictor` recents projection) and
  Defect B (overstated once-per-open bound on the User Email retry).

All line numbers below were re-derived by reading the files in this worktree. Where the upstream
issue document cites a different line, the discrepancy is called out explicitly.

**Correction to the issue document.** `issue.md:38` cites "`Display.cs` lines 41-51" for the comment
block and "`StoreWrapper.cs` 214-219" for its sibling. Both are stale. The actual positions are
`StoreWrapperController.Display.cs:35-41` (comment) and `:42-45` (retry gate), and
`StoreWrapper.cs:194-199` (comment) with the assignment at `:200`. `issue.md:56` cites
`QuickFiler/Controllers/EfcDataModel.cs:280-297`, which is correct.

---

## Numeric Derivation Evidence

### N1 — Reads of `IOlObjects.ArchiveRootPath` inside `FolderPredictor`

- **Complete Family:** every syntactic read of the `ArchiveRootPath` property reached through the
  `_globals` field, in every part of the `FolderPredictor` partial class, including the
  null-conditional (`_globals?.Ol.ArchiveRootPath`) form.
- **Exhaustive Search Scope:** both files of the partial class —
  `UtilitiesCS/OutlookObjects/Folder/FolderPredictor.cs` (1002 lines) and
  `UtilitiesCS/OutlookObjects/Folder/FolderPredictor.IFolderSearchHandler.cs` (11 lines, read in
  full: it contains only `public partial class FolderPredictor : IFolderSearchHandler { }`).
- **Inclusion Rules:** any occurrence of the identifier `ArchiveRootPath` in an expression position,
  whether the receiver is `_globals.Ol` or `_globals?.Ol`.
- **Exclusion Rules:** occurrences inside comments; reads of other `IOlObjects` members
  (`_globals.Ol.Root.FolderPath` at `:460` and `:486` are excluded — different property, does not
  route through `ArchiveRootPathGuard`); reads in other types.
- **Primary Search Strategy or Query Expression:** `Grep pattern="ArchiveRootPath"` scoped to
  `UtilitiesCS/OutlookObjects/Folder/FolderPredictor.cs`.
- **Primary Member Set:** lines 305, 376, 687, 752, 795, 857, 876, 914.
- **Primary Count:** 8.
- **Cross-check Search Strategy or Query Expression:** a structurally different receiver-anchored
  regex, `Grep pattern="Ol\.ArchiveRootPath|Ol\?\.ArchiveRootPath"` scoped to the whole directory
  `UtilitiesCS/OutlookObjects/Folder/`, then filtered to the `FolderPredictor` files. This query
  anchors on the receiver rather than the bare identifier, and widens the directory so that a read
  living in the second partial file would be caught.
- **Cross-check Member Set:** `FolderPredictor.cs` lines 305, 376, 687, 752, 795, 857, 876, 914
  (the same directory query also returned `FolderConverter.cs:334` and `:339`, which are outside the
  declared family and excluded by the inclusion rule).
- **Cross-check Count:** 8.
- **Member-set Comparison:** the normalized primary set {305, 376, 687, 752, 795, 857, 876, 914} and
  the normalized cross-check set {305, 376, 687, 752, 795, 857, 876, 914} are identical. A third,
  weaker sanity query (`Grep pattern="_globals\.Ol|_globals\?\.Ol"` on the same file) returned ten
  lines, the eight above plus `:460` and `:486`, which read `Ol.Root.FolderPath` and are correctly
  outside the family. **Assertion admitted: eight reads.**

### N2 — Documents and code sites asserting the once-per-open retry bound

- **Complete Family:** every location in the repository whose prose states or restates that the
  User Email retry is bounded to one execution per dialog open.
- **Exhaustive Search Scope:** the entire worktree, all file types, case-insensitive.
- **Inclusion Rules:** a sentence that asserts the bound as a property of the shipped code or as the
  behaviour the change delivers.
- **Exclusion Rules:** occurrences of "at most once" describing unrelated subjects (Triage training
  dedup, folder-tree caching, `KaStringAsync` latch, `ApplicationIdleTimer`); the review documents
  that *refute* the bound (`code-review.2026-09-07T22-40.md`, `feature-audit.2026-09-07T22-40.md`) —
  these are recorded separately below because they already state the true bound and need no edit;
  the #812 feature folder's own `issue.md` / `spec.md` / promoted potential entry, which state the
  *desired* bound rather than a stale claim.
- **Primary Search Strategy or Query Expression:** phrase-family alternation
  `Grep -i pattern="once per dialog open|at most once per open|once per open|per dialog open"` over
  the whole worktree, followed by a second alternation
  `"bounds the added UI-thread latency|single lookup startup already performs|one lookup per|only when the address is null"`.
- **Primary Member Set:**
  1. `UtilitiesCS/OutlookObjects/Store/StoreWrapperController.Display.cs:38-39`
  2. `UtilitiesCS/OutlookObjects/Store/StoreWrapper.cs:196-198`
  3. `UtilitiesCS.Test/OutlookObjects/Store/StoreWrapperController_Tests.Display.cs:90-91`
  4. `docs/.../797/spec.md:212-213`
  5. `docs/.../797/spec.md:492`
  6. `docs/.../797/spec.md:621-622`
  7. `docs/.../797/plan.2026-09-06T22-00.md:663`
  8. `docs/.../797/plan.2026-09-06T22-00.md:832`
  9. `docs/.../797/research/research-folder-settings-persistence.md:321`
  10. `docs/.../797/evidence/issue-updates/issue-797.2026-09-06T22-00.md:46`
- **Primary Count:** 10 (3 in code/test, 7 in the #797 folder).
- **Cross-check Search Strategy or Query Expression:** a deliberately different, narrower literal
  substring `Grep -i pattern="at most once"` over the whole worktree (no alternation, no
  UI-latency phrases), plus an independent lexical sweep `Grep -i pattern="retr"` over the whole
  `UtilitiesCS/OutlookObjects/Store/` directory to catch any code comment the phrase query would
  miss.
- **Cross-check Member Set:** the `"at most once"` sweep returned code sites 1, 2, 3 and doc sites
  4, 5, 6, 7, 9 (it did not return 8 or 10, whose wording is "one lookup per dialog open" and
  "retried once per dialog open" respectively — both captured by the primary query). The
  `"retr"` directory sweep over `UtilitiesCS/OutlookObjects/Store/` returned exactly two
  bound-asserting comment sites, `StoreWrapper.cs:194,196,207` and
  `StoreWrapperController.Display.cs:36,38`; the other hits (`StoreRehookResult.cs:31`,
  `StoreLaunchReadinessEvaluator.cs:15,68`, `StoreWrapper.cs:252,270`) are unrelated subjects and are
  excluded. So the cross-check confirms **exactly two production-code sites** and adds none.
- **Cross-check Count:** 10 (union of both cross-check queries; the two queries individually return
  8 and 2 overlapping members, whose union is the same 10-member set).
- **Member-set Comparison:** the normalized primary and cross-check member sets are identical
  (10 members: 2 production comments, 1 test comment, 7 #797 documents). The cross-check
  independently establishes that no third production comment exists. **Assertion admitted: three
  code/test comment sites and seven #797 document sites.**

---

## A1 — The two graceful-degradation precedents

### `EfcDataModel.TryGetArchiveRoot` — `QuickFiler/Controllers/EfcDataModel.cs:271-297`

Exact shape (private, `bool` + `out`):

```csharp
private bool TryGetArchiveRoot(out string archiveRoot)
{
    try
    {
        archiveRoot = Globals.Ol.ArchiveRootPath;
        return true;
    }
    catch (InvalidOperationException ex)
    {
        archiveRoot = null;
        logger.Warn(
            "Cannot resolve the Outlook archive root. Details are withheld from this "
                + "message because they contain a mailbox address.",
            ex
        );
        return false;
    }
}
```

- Catches: `InvalidOperationException` **only**. Its XML doc (`:271-279`) states this explicitly:
  "Any other failure, including a COM failure, still propagates."
- Logs: `logger.Warn(message, ex)` through the class's own `log4net.ILog`.
- Returns: `false` with `archiveRoot = null`.
- Callers (`:327`, `:370`, `:394`) return early; two of them additionally raise the user-facing
  `ArchiveRootUnavailableMessage` constant (`:267-269`), which deliberately names no path.

### `OutlookFolderHierarchyProvider.TryReadArchiveRoot` — `UtilitiesCS/OutlookObjects/Folder/OutlookFolderHierarchyProvider.cs:137-164`

- Reads through an injected `System.Func<string>? ArchiveRootAccessor` (property at `:94`, ctor
  parameter at `:82`), not through a globals chain.
- Catches: bare `Exception` (`:156`).
- Logs: `logger.Debug("The archive-root accessor threw; leaving the ancestor chain untrimmed.", exception)`.
- Returns: `null`; the consumer at `:115-119` then returns the untrimmed chain.

### Which is the better template, and why

**`EfcDataModel.TryGetArchiveRoot` is the better template for this fix.** Three reasons, each
verifiable:

1. **Structural fit.** `FolderPredictor` reads the property through the same
   `<globals>.Ol.ArchiveRootPath` chain that `EfcDataModel` does, with no accessor delegate present
   and no construction site to thread one through. Adopting the provider's shape would require
   adding a `Func<string>` parameter to three of `FolderPredictor`'s four constructors
   (`FolderPredictor.cs:26`, `:36`, `:43`) and to `AppAutoFileObjects.FolderPredictorLoad.cs` and
   the QuickFiler factory delegates — a much larger change than the defect warrants.
2. **Exception-type fidelity.** The throw this fix must absorb is precisely
   `InvalidOperationException`: `ArchiveRootPathGuard.RequireResolvedArchiveRoot`
   (`TaskMaster/AppGlobals/ArchiveRootPathGuard.cs:44`, `:56`) throws only that type, and
   `AppOlObjects.ResolveValidatedArchiveRootPath` (`TaskMaster/AppGlobals/AppOlObjects.ArchiveRoot.cs:54-65`)
   explicitly normalizes a `COMException` into `InvalidOperationException` so the getter's contract
   admits only that type. Catching bare `Exception`, as the provider does, would additionally
   swallow genuine programming errors on a display path — a widening the General Code Change Policy
   §3 ("do not silently ignore errors") does not support.
3. **Log level matches the acceptance criterion.** AC1 requires "one logged warning";
   `EfcDataModel` uses `logger.Warn`, the provider uses `logger.Debug`.

**Caveat the planner must resolve.** `UtilitiesCS/OutlookObjects/Folder/FolderPredictor.cs` declares
**no** `log4net` logger — a `Grep` for `logger|log4net` over that file returns zero matches. The fix
must introduce one (the repo-standard form is
`private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);`,
as at `StoreWrapperController.cs:73-75` and `OutlookFolderHierarchyProvider.cs:45-47`), or route the
diagnostic through an injectable sink. See the file-size note in A2 for where to put it.

---

## A2 — Every read of `ArchiveRootPath` in `FolderPredictor.cs`, and the scope boundary

Eight reads (derivation in N1). Enclosing members re-derived by reading the file:

| Line | Enclosing member | Text | Guarded today | Category |
|---|---|---|---|---|
| 305 | `FindFolder(...)` (`:293`) | `emailSearchRoots = new() { _globals.Ol.ArchiveRootPath };` | no | **Search root** — only when `emailSearchRoots is null` (`:303`) |
| 376 | `FindFolderRows(...)` (`:364`) | same | no | **Search root** — only when `emailSearchRoots is null` (`:374`) |
| 687 | `CreateFolder(string,string,string)` (`:678`) | `olAncestor = _globals.Ol.ArchiveRootPath;` | no | **Creation ancestor** — only when `olAncestor.IsNullOrEmpty()` (`:685`) |
| 752 | `CreateFolderAsync(...)` (`:740`) | same | no | **Creation ancestor** — only when `olAncestor.IsNullOrEmpty()` (`:750`) |
| 795 | `AddRecents(ref List<string>)` (`:789`) | `var r = _globals.Ol.ArchiveRootPath;` | no | **Display projection** — feeds `ArchiveStemProjection.ToDisplayStem` at `:797` |
| 857 | `ProjectSuggestionPath(string)` (`:853`) | `ArchiveStemProjection.ToDisplayStem(folderPath, _globals?.Ol.ArchiveRootPath)!` | `?.` guards a null `_globals` only, **not** a throwing property | **Display projection** |
| 876 | `AddRecentRows(List<FolderRow>)` (`:863`) | `var root = _globals.Ol.ArchiveRootPath;` | no | **Display projection** — feeds `ToDisplayStem` at `:879` |
| 914 | `LoopFolders(...)` (`:904`) | `olAncestor = _globals.Ol.ArchiveRootPath;` | no | **Search ancestor** — only when `string.IsNullOrEmpty(olAncestor)` (`:912`) |

### Is `ProjectSuggestionPath` (`:857`) on the same failure path? **Yes — decisively, and it runs first.**

This is the single most important scope finding, and it is established by call-graph evidence, not
assumption. `ProjectSuggestionPath` has exactly two callers, both inside `FolderPredictor.cs`:
`AddSuggestions` at `:815` (`Suggestions.ToArray(5).Select(ProjectSuggestionPath)`) and
`AddSuggestionRows` at `:847`. A repo-wide `Grep` for `ProjectSuggestionPath` over `*.cs` returns no
other invocation (the two QuickFiler hits at `QfcItemController.FolderHandling.cs:224` and `:255`
are comments).

The four public entry points that reach the recents projection all reach the suggestion projection
**earlier in the same call frame**:

| Entry point | Suggestion call | Recents call |
|---|---|---|
| `FolderArray` getter (`:217-232`) | `AddSuggestions(ref _folderList)` at `:225`, gated on `Suggestions.Count > 0` (`:224`) | `AddRecents(ref _folderList)` at `:227` |
| `FolderRowArray` getter (`:244-259`) | `AddSuggestionRows(rows)` at `:251`, gated on `Suggestions.Count > 0` (`:249`) | `AddRecentRows(rows)` at `:255` |
| `FindFolder(...)` (`:293-343`) | `AddSuggestions(ref _folderList)` at `:337`, **unconditional** | `AddRecents(ref _folderList)` at `:340` |
| `FindFolderRows(...)` (`:364-413`) | `AddSuggestionRows(rows)` at `:407`, **unconditional** | `AddRecentRows(rows)` at `:410` |

Consequence: with an unresolvable archive root and a non-empty suggestion set, the
`InvalidOperationException` escapes from `:857` **before** `AddRecents` / `AddRecentRows` is ever
entered. Guarding only lines 795 and 876 would therefore leave the user-visible symptom unchanged in
that case, and AC1's outcome ("recents rendered as stored") would not be observable end to end.

The issue's own reproduction narrows to "zero suggestions and a non-empty recents list"
(`issue.md:16`, `:27`), which is exactly the case where `:857` is *not* reached — via the
`FolderArray` getter, because `AddSuggestions` is skipped when `Suggestions.Count == 0`. So the
issue's repro is internally consistent; it simply does not exercise the wider failure path. Note
also that via `FindFolder` / `FindFolderRows` the suggestion call is unconditional, but with an empty
`Suggestions` the `Select` / `foreach` never invokes `ProjectSuggestionPath`, so no read occurs there
either.

**Recommended scope boundary (evidence-based, for the spec to ratify):** guard the three
**display-projection** reads — `:795`, `:857`, `:876` — as one family, and leave the five
**functional** reads (`:305`, `:376`, `:687`, `:752`, `:914`) untouched. The justification is that
the three display reads all terminate in `ArchiveStemProjection.ToDisplayStem`, whose documented
contract already degrades to identity on a null root (see A5), so substituting `null` for a throwing
root is behaviour-preserving for them. The other five feed a search root, a folder-creation ancestor,
or a prefix-stripping ancestor; substituting `null` there would not "degrade gracefully" — it would
change which folders are searched or where a folder is created. Those five are pre-existing readers
with different semantics and belong in a separate issue if they need attention at all.

`ProjectSuggestionPath` is a **pre-existing reader** in the chronological sense (it predates the #799
recents change), but it is **not** out of scope, because it sits on the identical failure path and
would defeat the fix. The distinction the issue draws between "new throw on a previously
non-throwing path" (#801) and pre-existing readers does not survive contact with the call graph for
this particular member.

### Downstream coherence hazard outside `UtilitiesCS` (report-only)

`QuickFiler/Controllers/QfcItemController.FolderHandling.cs:233` reads
`_globals.Ol?.ArchiveRootPath ?? string.Empty` inside `AssignFolderComboBox`, in the **same call
frame**, six lines after consuming `_folderHandler.FolderRowArray` at `:221`. The `?.` guards a null
`Ol` only, not a throwing property. `AssignFolderComboBox` (`:191`) is not inside any `try` — a
`Grep` for `catch` in that file returns only `:113`, `:121`, `:127`, all inside
`LoadFolderHandlerAsync`, a different member. So even with `FolderPredictor` fully guarded, an
unresolvable archive root still throws out of `AssignFolderComboBox` on the UI thread.

This is in the `QuickFiler` assembly, outside the issue's named files, and outside `UtilitiesCS`.
Recommendation: record it as a follow-up defect rather than widening #812, but state in the spec
that AC1's user-visible outcome is verifiable only at the `FolderPredictor` unit level, not end to
end, until that site is also guarded.

---

## A3 — The `_globals` injection seam, and whether Moq can mock the throw

- Field: `private IApplicationGlobals _globals;` — `FolderPredictor.cs:154`.
- Constructors:
  - `FolderPredictor(Outlook.Application olApp)` — `:26-34`; navigation-only, sets `_globals = null!`.
  - `FolderPredictor(IApplicationGlobals AppGlobals)` — `:36-41`; **this is the one the existing
    recents tests use**.
  - `FolderPredictor(IApplicationGlobals appGlobals, object objItem, InitOptions options)` — `:43-49`.
- Property chain: `IApplicationGlobals.Ol` → `IOlObjects.ArchiveRootPath`.
  - `IApplicationGlobals` is a public interface at
    `UtilitiesCS/Interfaces/IGlobals/IApplicationGlobals.cs:7`; `IOlObjects Ol { get; }` at `:11`.
  - `IOlObjects` is a public interface at `UtilitiesCS/Interfaces/IGlobals/IOlObjects.cs:11`;
    `string ArchiveRootPath { get; }` at `:15`.

**`ArchiveRootPath` is an interface property, so it is fully Moq-mockable, including the throwing
case.** The acceptance criterion "a unit test covers the throwing-root case with a Moq seam" is
achievable exactly as written. The required arrangement is one line added to the existing helper:

```csharp
olObjects
    .SetupGet(x => x.ArchiveRootPath)
    .Throws(new InvalidOperationException(/* redacted-rule text */));
```

No new production seam is needed for the test. Note that `AppOlObjects` (the production implementer)
is in the `TaskMaster` assembly and is not referenced by `UtilitiesCS.Test`; the tests bind against
the interface only, which is why this works.

---

## A4 — Existing `FolderPredictor` tests and the arrangement pattern

| File | Test class | Lines | Relevance |
|---|---|---|---|
| `UtilitiesCS.Test/OutlookObjects/Folder/FolderPredictorRecentsProjectionTests.cs` | `FolderPredictorRecentsProjectionTests` (sealed, `[TestClass]` at `:27-28`) | 212 | **The #799 AC5 recents-projection suite.** Four tests covering `FolderArray`, `FolderRowArray`, text parity, and the out-of-root identity case. |
| `UtilitiesCS.Test/OutlookObjects/Folder/FolderPredictorTests.cs` | `FolderPredictorTests` (`:19`) | **1066** | `AddRecents_WhenRecentsExist_AppendsHeaderAndEntries` (`:250`), `AddSuggestions_WhenSuggestionsExist_AppendsHeaderAndTopSuggestions` (`:268`). Already far over the 500-line cap; do not add here. |
| `UtilitiesCS.Test/OutlookObjects/Folder/FolderPredictorCoverageExpansionTests.cs` | `FolderPredictorCoverageExpansionTests` | 119 | Two more `AddSuggestions` cases (`:18`, `:42`). |
| `UtilitiesCS.Test/OutlookObjects/Folder/ArchiveStemProjectionTests.cs` | (see A5) | 176 | Pure-function coverage of `ToDisplayStem`. |

**Concrete working arrangement pattern** — `FolderPredictorRecentsProjectionTests.cs:118-151`, the
closest fixture to the fix:

```csharp
private static UtilitiesCS.FolderPredictor PredictorWithRecents(params string[] recents)
{
    var archiveRoot = CreateFolder(ArchiveRootPath, new Dictionary<string, OutlookFolder>());
    var app = CreateApplication(
        new Dictionary<string, OutlookFolder> { ["ArchiveRoot"] = archiveRoot.Object });
    var globals = CreateGlobals(app, archiveRoot.Object, recents);
    return new UtilitiesCS.FolderPredictor(globals.Object);
}

private static Mock<IApplicationGlobals> CreateGlobals(
    Mock<Outlook.Application> app, OutlookFolder rootFolder, IEnumerable<string> recents)
{
    var autoFile = new Mock<IAppAutoFileObjects>();
    autoFile.SetupGet(x => x.RecentsList).Returns(new SloLinkedList<string>(recents));

    var olObjects = new Mock<IOlObjects>();
    olObjects.SetupGet(x => x.App).Returns(app.Object);
    olObjects.SetupGet(x => x.ArchiveRootPath).Returns(rootFolder.FolderPath);
    olObjects.SetupGet(x => x.Root).Returns(rootFolder);

    var globals = new Mock<IApplicationGlobals>();
    globals.SetupGet(x => x.AF).Returns(autoFile.Object);
    globals.SetupGet(x => x.Ol).Returns(olObjects.Object);
    return globals;
}
```

`FolderPredictorTests.cs:984-1005` carries a near-identical `CreateGlobals` with
`IEnumerable<string> recents = null` defaulted. Note both call `.SetupGet(x => x.ArchiveRootPath)` —
swapping `.Returns(...)` for `.Throws(...)` is the entire test arrangement change required.

**Recommended test home:** a new sibling file (for example
`FolderPredictorArchiveRootDegradationTests.cs`) or an extension of the 212-line
`FolderPredictorRecentsProjectionTests.cs`. Both stay under the 500-line cap;
`FolderPredictorTests.cs` at 1066 lines does not.

---

## A5 — `ArchiveStemProjection.ToDisplayStem` behaviour on a null or empty root

`UtilitiesCS/OutlookObjects/Folder/ArchiveStemProjection.cs:40-61`:

```csharp
public static string? ToDisplayStem(string? folderPath, string? archiveRoot)
{
    if (folderPath is null || archiveRoot is null)
    {
        return folderPath;
    }

    if (ArchiveStemContract.TryMakeArchiveRelative(folderPath, archiveRoot, out var stem)
        && stem.Length > 0)
    {
        return stem;
    }

    return folderPath;
}
```

- **Null root → identity.** The explicit guard at `:45-48` returns `folderPath` unchanged. The
  in-code comment states the guard is required for CS8604 nullable narrowing, not merely defensive.
- **Empty and whitespace-only root → identity.** The class XML doc at `:8-13` states the contract
  ("a null or empty root, and a whitespace-only root" all return the input unchanged), and the
  behaviour is pinned by tests at `ArchiveStemProjectionTests.cs:101` (`string.Empty`) and `:114`
  (`"   "`).

**This confirms the smallest-possible fix shape.** Because the projection already degrades to
identity on a null root, the fix does not need any new projection logic: it needs only to substitute
`null` for the throwing property read. AC1's stated outcome ("entries rendered as stored") follows
directly from `:45-48`.

**Gap to note:** there is no test that passes a literally `null` `archiveRoot` — the suite covers
`string.Empty` and whitespace only. A new test pinning the null-root identity case would make the
fix's dependency on `:45-48` explicit rather than implied. Recommended, cheap, and it strengthens
the AC1 chain.

---

## B1 — `RefreshUserEmailAddress` and the COM chain

`UtilitiesCS/OutlookObjects/Store/StoreWrapper.cs:192-202`:

```csharp
internal string? RefreshUserEmailAddress()
{
    // ... comment block at :194-199 ...
    UserEmailAddress = GetSmtpAddressFromStore();
    return UserEmailAddress;
}
```

- **Assignment line: `:200`.** The property is `public string? UserEmailAddress { get; internal set; }`
  at `:174`, carrying `[JsonIgnore]` at `:173`.
- **A failed lookup leaves `UserEmailAddress` null, so the gate re-fires.** `GetSmtpAddressFromStore`
  (`:204-286`) has exactly one terminal failure return: `return null;` at `:285`, preceded by
  `LastSmtpLookupError = capturedError ?? "No Exchange address, ..."` at `:282-284`. Because `:200`
  assigns that `null` back to `UserEmailAddress`, the controller gate
  `Current.UserEmailAddress is null` (`StoreWrapperController.Display.cs:42`) evaluates true again on
  the next `PopulateWithCurrent`. **Confirmed.**
- **A successful lookup leaves it non-null, so the gate stops.** Three success returns, each first
  clearing the reason: `:245` (`return primarySmtpAddress;` after `LastSmtpLookupError = null;` at
  `:244`), `:263` (`return address;` after `:262`), `:279` (`return displayName;` after `:278`).
  Each is reached only under a non-empty / at-sign-bearing guard (`:242`, `:260`, `:276`).
  **Confirmed.**
- **The call is synchronous on the calling thread.** `RefreshUserEmailAddress` is not `async` and
  returns `string?`, not `Task<string?>`. `GetSmtpAddressFromStore` is likewise synchronous and
  contains no `await`, no `Task`, and no thread dispatch. The COM chain it drives —
  `RootFolder?.Session?.CurrentUser` (`:219`), `currentUser?.AddressEntry` (`:225`),
  `addressEntry?.GetExchangeUser()` (`:231`), `exchangeUser?.PrimarySmtpAddress` (`:237`), then
  `addressEntry?.Address` (`:259`) — executes inline. **Confirmed.**
- **The calling thread is the Outlook UI thread.** `PopulateWithCurrent` marshals to it at
  `StoreWrapperController.Display.cs:19-23` (`if (Viewer.InvokeRequired) { Viewer.Invoke(...); return; }`),
  so the retry body always runs on the UI thread.
- **Prior blocking evidence.** `issue.md:43` records a `ThreadMonitor` observation inside
  `_ExchangeUser.get_PrimarySmtpAddress()` at 17:35:21 in `debug_2026-09-06.log`. `:237` is that
  exact read. Per-step `Stopwatch` instrumentation surrounds every hop (`:218`, `:224`, `:230`,
  `:236`) and logs `[Startup timing]` lines, which is how a repeat would be observed in the log.
- **Note on names in the prompt.** `ResolveCurrentUserEmailAddress` and `TryGetSmtpAddress` are
  **not** on this chain. They live in `TaskMaster/AppGlobals/AppOlObjects.cs:359` and `:384` and are
  the *globals* SMTP path, a different consumer. `AppOlObjects.ArchiveRoot.cs:17-19` cites them only
  as a shape precedent. They are not touched by this defect.

---

## B2 — Where a latch reset could live so that it is genuinely unit-testable

### The constraint, restated with evidence

`StoreWrapperController.Launch()` is `[ExcludeFromCodeCoverage]` (`StoreWrapperController.cs:115`,
method at `:116-136`) and calls `Viewer.ShowDialog()` at `:135`. Per the exemption research recorded
for issue #457, an exempt member emits no `<method>` element in the Cobertura document at all, so
lines inside `Launch` are not merely uncovered — they are invisible to the coverage gate.

**However, the premise behind AC2's "reset in `Launch`" is unnecessary.** The decisive finding:

> `StoreWrapperController` has **exactly one** production construction site:
> `TaskMaster/Ribbon/RibbonController.cs:259-263`
> ```csharp
> internal void FolderStoresSettings()
> {
>     var wrapper = new StoreWrapperController(Globals);
>     wrapper.Launch();
> }
> ```
> A repo-wide `Grep` for `new StoreWrapperController` over `*.cs` returns this one production site
> and 24 test sites. Both ribbon entry points — `RibbonViewer.cs:214` and
> `RibbonViewer.EngineCommands.cs:219` — forward to this same method. A fresh controller is
> therefore constructed for **every** dialog open, and the controller instance is never reused
> across opens.

Consequence: **a plain per-instance `bool` field needs no reset at all.** "Once per controller
instance" and "once per dialog open" are the same bound in production, and the field's initial
`false` state is established by the constructor, which *is* covered
(`StoreWrapperController_Tests.cs:80-86`, `Constructor_SetsGlobals`).

### Enumerated options with costs (not a recommendation — the spec decides)

**Option 1 — per-instance `bool` field, no reset anywhere.**
- Testability: complete. Two `PopulateWithCurrent()` calls on one controller prove one lookup;
  a second controller proves the next open gets a fresh attempt.
- Coverage: no line lands in an exempt member; nothing is invisible.
- Cost: AC2 as currently drafted must be reworded — "a new `Launch` permits one more" becomes "a new
  controller instance permits one more", which is what the production code actually does.
- Risk: if a future caller reuses a controller across opens, the bound silently becomes
  once-per-lifetime. Mitigable by an XML doc comment naming `RibbonController.FolderStoresSettings`
  as the sole construction site.

**Option 2 — reset inside `EvaluateLaunchReadiness()` (`StoreWrapperController.cs:108-111`).**
- Testability: complete. The member is `internal`, has six direct tests
  (`StoreWrapperController_Tests.Launch.cs:356`, `:373`, `:392`, `:414`, `:435`, plus the three
  `Launch` tests that drive it), and is called from exactly one place — `Launch:118`.
- Cost: gives a readiness *query* a mutating side effect, which conflicts with General Code Change
  Policy §4 (separation of concerns) and with the member's own XML doc (`:96-107`), which describes
  it purely as a predicate. It would also reset on the *non-ready* paths, where no dialog opens.

**Option 3 — extract `internal void PrepareForOpen()` and call it from `Launch`.**
- Testability: partial. The method body is testable directly; the *call* from `Launch:130` is inside
  the exempt member and can never be executed or observed by a unit test. The reset would be proven
  to work but never proven to be wired.
- Cost: one extra member; the wiring is unverifiable, which is precisely the failure mode the
  #797 review flagged for `PersistJunkFolderSelections` (reflection-based wiring that no test could
  catch).

**Option 4 — reset on the `Viewer` setter.**
- `public IStoreWrapperViewer Viewer { get; internal set; }` (`:84`) is auto-implemented; converting
  it to a full property with a side effect is a behaviour change on a member 24 test sites assign
  directly, several of which assign it *after* other state. Reset ordering becomes fragile.
- Not recommended, listed for completeness.

**Option 5 — per-store latch keyed on `StoreWrapper.StoreId` (`StoreWrapper.cs:162`).**
- Testability: complete (a `HashSet<string>` on the controller; `StoreId` is a plain settable
  property that tests already populate).
- Semantics: the bound becomes "once per store per controller instance", which is arguably the more
  useful behaviour — cycling A→B→A retries B once and does not re-retry A.
- Cost: `StoreId` can be null or whitespace (`Init` guards its read at `StoreWrapper.cs:46-55` and
  leaves the default on failure), so the latch needs a fallback branch for an unreadable StoreID;
  `BindExcludeStoreCheckbox` (`Display.cs:104-110`) already establishes the fail-safe precedent for
  that case.

**Option 6 — no latch; correct the documentation instead.** See B6.

---

## B3 — `StoreWrapperController` partial files and line counts

`StoreWrapperController` is declared `public partial class` in two files:

| File | Lines | Headroom to 500 |
|---|---|---|
| `UtilitiesCS/OutlookObjects/Store/StoreWrapperController.cs` (`partial` at `:66`) | **388** | 112 |
| `UtilitiesCS/OutlookObjects/Store/StoreWrapperController.Display.cs` (`partial` at `:15`) | **173** | 327 |

Note: the `Display.cs` header comment at `:11` states the main file "stood at 478 lines"; it is now
388, because the display members were moved out.

Files the fix is expected to touch, with current counts and risk:

| File | Lines | Risk of crossing 500 |
|---|---|---|
| `UtilitiesCS/OutlookObjects/Store/StoreWrapperController.cs` | 388 | Low. A `bool` field plus XML doc is ~8 lines. |
| `UtilitiesCS/OutlookObjects/Store/StoreWrapperController.Display.cs` | 173 | None. |
| `UtilitiesCS/OutlookObjects/Store/StoreWrapper.cs` | 302 | None (comment rewrite only). |
| `UtilitiesCS.Test/.../StoreWrapperController_Tests.Display.cs` | 252 | Low; ~248 lines of headroom. |
| `UtilitiesCS.Test/.../StoreWrapperController_Tests.cs` | 182 | None. |
| `UtilitiesCS.Test/.../StoreWrapperController_Tests.Launch.cs` | **480** | **HIGH — 20 lines of headroom. Do not add tests here.** |
| `UtilitiesCS.Test/.../StoreWrapperController_Tests.ButtonAndPopulate.cs` | 402 | Moderate; 98 lines. |
| `UtilitiesCS/OutlookObjects/Folder/FolderPredictor.cs` | **1002** | **Already 2x over cap.** Any net line addition worsens an existing violation. |
| `UtilitiesCS/OutlookObjects/Folder/FolderPredictor.IFolderSearchHandler.cs` | 11 | n/a |
| `UtilitiesCS.Test/.../FolderPredictorRecentsProjectionTests.cs` | 212 | None. |
| `UtilitiesCS.Test/.../FolderPredictorTests.cs` | **1066** | **Already over cap. Do not add tests here.** |

**Precedent for the `FolderPredictor.cs` problem.** `FolderPredictor.IFolderSearchHandler.cs:4-9`
documents the established remedy verbatim: a second partial-class part was created specifically "so
`FolderPredictor.cs` itself (already 823 lines, over the 500-line cap before this cycle) is not
touched beyond the one-word `partial` edit". The same move applies here: put the logger declaration
and the guarded accessor in a new `FolderPredictor.ArchiveRoot.cs`, and confine edits inside
`FolderPredictor.cs` to replacing the three read expressions at `:795`, `:857`, `:876` with calls to
it — a net-zero or net-negative line change in the over-cap file.

---

## B4 — Existing `StoreWrapperController` tests and the viewer double

Test files (all in `UtilitiesCS.Test/OutlookObjects/Store/`):

| File | Test class | Lines |
|---|---|---|
| `StoreWrapperController_Tests.cs` | `StoreWrapperController_Tests` — `[TestClass] [DoNotParallelize] public partial` at `:13-15` | 182 |
| `StoreWrapperController_Tests.Display.cs` | same partial (`:19`) | 252 |
| `StoreWrapperController_Tests.Launch.cs` | same partial (`:13`) | 480 |
| `StoreWrapperController_Tests.ButtonAndPopulate.cs` | same partial (`:13`) | 402 |
| `StoreWrapperController_Tests.ExcludeStore.cs` | same partial | 164 |
| `StoreWrapperControllerTests.cs` | `StoreWrapperControllerTests` (separate class) | 361 |
| `StoreWrapperTests.cs` | `StoreWrapperTests` (`:15`) | 416 |

### Coverage of the members named in the prompt

- **`PopulateWithCurrent`** — `StoreWrapperController_Tests.ButtonAndPopulate.cs:124`, `:144`, `:174`;
  `StoreWrapperController_Tests.Display.cs:66`, `:88`, `:111`, `:182`, `:212`.
- **The #797 AC6 retry** — three tests, all in `StoreWrapperController_Tests.Display.cs`:
  `PopulateWithCurrent_WhenUserEmailIsNull_RetriesLookupAndRendersAddress` (`:66`),
  `PopulateWithCurrent_WhenUserEmailIsAlreadyPopulated_DoesNotRetryLookup` (`:88`),
  `PopulateWithCurrent_WhenRetryFails_RendersSpecificMessageWithReason` (`:111`).
- **`BindExcludeStoreCheckbox`** — `StoreWrapperController_Tests.ExcludeStore.cs` (164 lines).
- **`TrimStorePrefix`** — six tests at `StoreWrapperController_Tests.Display.cs:137-179`.
- **`RefreshUserEmailAddress`** — `StoreWrapperTests.cs:254`
  (`RefreshUserEmailAddress_WhenRootFolderIsNull_ReturnsNullAndDoesNotThrow`), with
  `LastSmtpLookupError` assertions at `:185` and `:247`.

### The viewer double — **yes, it already exists, and it satisfies `Viewer.InvokeRequired`**

`StoreWrapperController_Tests.cs:160-178`:

```csharp
private static (StoreWrapperController controller, Mock<IStoreWrapperViewer> viewer)
    CreateControllerWithViewer()
{
    var mockGlobals = new Mock<IApplicationGlobals>();
    var controller = new StoreWrapperController(mockGlobals.Object);
    var mockViewer = new Mock<IStoreWrapperViewer>();
    mockViewer.Setup(v => v.InvokeRequired).Returns(false);
    mockViewer.Setup(v => v.ArchiveOutlook).Returns(new Label());
    mockViewer.Setup(v => v.ArchiveFS).Returns(new Label());
    mockViewer.Setup(v => v.JunkEmail).Returns(new Label());
    mockViewer.Setup(v => v.JunkPotential).Returns(new Label());
    mockViewer.Setup(v => v.Inbox).Returns(new Label());
    mockViewer.Setup(v => v.RootFolder).Returns(new Label());
    mockViewer.Setup(v => v.UserEmail).Returns(new Label());
    controller.Viewer = mockViewer.Object;
    return (controller, mockViewer);
}
```

`InvokeRequired => false` (`:168`) is exactly what makes the `Display.cs:19-23` marshalling guard
fall through, so the whole `PopulateWithCurrent` body executes on the test thread. `CreateController()`
(`:154-158`) is the viewer-less variant.

A second, non-Moq viewer double also exists: several tests instantiate the real
`StoreWrapperViewer` directly (`ButtonAndPopulate.cs:183`, `:293`, `:305`;
`StoreWrapperControllerTests.cs:42`, `:121`). The comment at `ButtonAndPopulate.cs:176-181` explains
why: `Mock<T>` over Task-bearing interfaces was hitting a `TypeInitializationException` from a
missing `System.Threading.Tasks.Extensions 4.2.0.1` in the test bin output, and the real viewer never
creates a Form handle so `InvokeRequired` returns false on the test thread anyway. The planner should
prefer `CreateControllerWithViewer()` (which demonstrably works today for the three AC6 tests) and
treat the direct-viewer route as the fallback.

**Stubbing `Current`** — `new StoreWrapper(null)` with object-initializer properties; the failing and
succeeding COM chains are already built by two helpers in `Display.cs`:
`CreateDisplaySmtpRootFolder(string)` at `:26-41` (succeeds) and
`CreateDisplayFailingSmtpRootFolder(string)` at `:47-63` (throws `COMException` from
`PrimarySmtpAddress` and returns a non-at-sign `Address`, so the whole chain yields null). The
second is precisely the fixture an AC2 latch test needs.

**Stubbing `Model`** — `new Mock<StoresWrapper>().Object` (`ButtonAndPopulate.cs:31`) or a real
`new StoresWrapper { Stores = new List<StoreWrapper> { ... } }` (`ButtonAndPopulate.cs:320-323`).

**Counting lookups in a test.** There is no invocation counter on `StoreWrapper` today. The existing
tests infer "no retry ran" indirectly, by arranging a mocked chain that *would* return a different
address and asserting the value is unchanged (`Display.cs:90-107`). For AC2 the direct route is
`Mock.Verify` on the mocked `ExchangeUser.PrimarySmtpAddress` getter (or on
`Recipient.AddressEntry`), asserting `Times.Once()` after two `PopulateWithCurrent()` calls. That
works against the existing helpers with no production change.

---

## B5 — Complete list of locations asserting the once-per-open bound

Derivation and completeness argument in **N2**. Ten sites, split into three that require an edit if
option (i) is *not* taken, and seven #797 documents. Exact current wording:

### Production code (2)

1. **`UtilitiesCS/OutlookObjects/Store/StoreWrapperController.Display.cs:35-41`** (comment; gate at
   `:42-45`). *(Issue document cites 41-51 — stale.)*
   > `// why: issue #797 AC6. The SMTP lookup runs once per store initialisation and is never`
   > `// retried, and a successful result is not persisted, so one transient COM failure at`
   > `// startup left the label showing a generic placeholder for the rest of the session.`
   > `// Retry here, at most once per dialog open and only when the address is null, which`
   > `// bounds the added UI-thread latency to the single lookup startup already performs.`
   > `// Every dereference on this path is null-conditional, so a null current store cannot`
   > `// throw here.`

2. **`UtilitiesCS/OutlookObjects/Store/StoreWrapper.cs:194-199`** (comment inside
   `RefreshUserEmailAddress`). *(Issue document cites 214-219 — stale.)*
   > `// why: issue #797 AC6. The lookup ran once per Init and was never retried, and the`
   > `// resolved address carries JsonIgnore so a success is not cached across restarts. The`
   > `// settings dialog calls this at most once per open, and only when the address is null,`
   > `// which bounds the added UI-thread latency to the single lookup startup already`
   > `// performs. Safe when RootFolder is null: the chain's first read is null-conditional,`
   > `// so the call yields null and records a reason rather than throwing.`

   No XML doc comment on this member repeats the bound; the `<summary>` at `:186-191` says only
   "Re-runs the SMTP lookup and republishes the result ... Safe to call when `RootFolder` is null."

### Test code (1) — missed by the issue document entirely

3. **`UtilitiesCS.Test/OutlookObjects/Store/StoreWrapperController_Tests.Display.cs:90-92`**
   (comment in `PopulateWithCurrent_WhenUserEmailIsAlreadyPopulated_DoesNotRetryLookup`):
   > `// Arrange (issue #797, AC6): the retry is attempted at most once per dialog open and`
   > `// only when the address is null, which bounds the added UI-thread latency. The mocked`
   > `// chain would yield a different address, so an unchanged value proves no retry ran.`

   This is the third code comment the #797 code review counted. It must be corrected under either
   fix direction, and it is not named anywhere in `issue.md`.

### #797 feature-folder documents (7), all under
`docs/features/active/2026-09-06-folder-settings-never-persist-and-user-email-error-loading-797/`

4. **`spec.md:212-213`** (Non-Goals item 8):
   > "AC6 is therefore satisfied with a synchronous retry on the UI thread, attempted at most once
   > per dialog open and only when the address is null."

5. **`spec.md:490-493`** (AC6 detail). *(Issue document cites 491-493; the sentence begins at 492
   and the surrounding claim runs from 490.)*
   > "The retry site is PopulateWithCurrent, which the research verified is the single method that
   > runs both when the dialog opens and on every store re-selection, and which already marshals to
   > the UI thread at its top. The retry is attempted at most once per dialog open and only when the
   > address is null."

   Note the internal contradiction already present in this paragraph: it simultaneously states that
   the site runs "on every store re-selection" and that the retry is "at most once per dialog open".

6. **`spec.md:619-624`** (Risks, item 1; the phrase is at `:621-622`):
   > "Mitigation: retry at most once per dialog open and only when the address is null, which bounds
   > the added latency to the same single lookup the startup path already performs."

7. **`plan.2026-09-06T22-00.md:663`** (task P3-T3):
   > "... inside `PopulateWithCurrent`, when the current store's user email address is null, invoke
   > the retry entry point at most once per dialog open ..."

8. **`plan.2026-09-06T22-00.md:832`**:
   > "The risk is bounded to one lookup per dialog open, attempted only when the address is null."

9. **`research/research-folder-settings-persistence.md:321`** (Conclusion for D4):
   > "... bounding the risk by attempting the retry at most once per dialog open and only when
   > `UserEmailAddress` is null ..."

10. **`evidence/issue-updates/issue-797.2026-09-06T22-00.md:46`**:
    > "... and the lookup is retried once per dialog open when the address is null."

### Documents that already state the TRUE bound — no edit needed

- `code-review.2026-09-07T22-40.md:97`, `:131`, `:139` (CR-1) — already records that the bound does
  not hold and proposes both remedies.
- `feature-audit.2026-09-07T22-40.md:138`, `:221` — states the true bound: "one lookup per populate
  invocation on a store whose address is still null."

### Editability note

The #797 folder is under `docs/features/active/`, not `archive/`, so its `spec.md` is still an
editable working document. Sites 7-10, however, are historical artifacts (a completed plan, a
research note, an issue-update record). Amending a completed plan or a dated evidence artifact
retroactively is questionable practice. **Recommendation for the spec to ratify:** correct sites 1-6
(the two production comments, the test comment, and the three `spec.md` statements) and add a single
dated correction note rather than rewriting the historical artifacts 7-10.

---

## B6 — Evidence bearing on the choice between (i) latch and (ii) documentation correction

No decision is made here. Both directions are presented with their evidence.

### Direction (i) — enforce the documented bound with a per-controller-instance latch

**Testability: fully achievable.** This is the material finding from B2. Because
`RibbonController.cs:261` constructs a fresh controller per dialog open, a plain instance `bool`
requires **no reset at all**, and every line of the change lands in `PopulateWithCurrent` — a member
with eight existing tests and a working viewer double. Nothing lands in `Launch`. AC2's premise that
the reset must live in `Launch` is therefore false, and AC2 should be reworded.

Concrete testable assertions available today, all against existing fixtures:
- Two `PopulateWithCurrent()` calls on one controller with `CreateDisplayFailingSmtpRootFolder`
  → `Mock.Verify(... PrimarySmtpAddress ..., Times.Once())`.
- A second controller with the same failing store → one further lookup.
- A store with a non-null `UserEmailAddress` → `Times.Never()` (already proven in spirit by
  `Display.cs:88`).

**Costs.** `#nullable enable` is already on both controller files (`:1` of each), so a new field
needs no pragma. The change adds a field + gate to a 388-line file (112 lines of headroom) and a
comment rewrite to a 173-line file. No file crosses 500. One behaviour change: after a failed retry,
re-selecting the same store no longer re-attempts within one dialog session, so a user who
re-selects hoping to force a retry must close and reopen the dialog.

**User-visible consequence.** Cycling the Display Name selector N times on a persistently failing
mailbox costs **one** blocking COM chain instead of N. Given the `ThreadMonitor` evidence at
`issue.md:43` that a single `_ExchangeUser.get_PrimarySmtpAddress()` already blocked the UI thread
long enough to be flagged, N repetitions is the concrete harm. The label text is unchanged either
way — `BuildUserEmailUnavailableText()` (`Display.cs:77-86`) renders the same message from the
already-captured `LastSmtpLookupError`, which persists on the `StoreWrapper` after the first failure.

**Argument from the accepted-risk record.** `spec.md:619-624` accepted the reintroduction of a
synchronous UI-thread COM read *on the stated basis* that it was bounded to once per open, and
`code-review.2026-09-07T22-40.md:131` says so explicitly: "it is the stated basis on which the
reintroduced synchronous COM read was accepted." If the bound is not enforced, the risk that was
accepted is not the risk that shipped.

### Direction (ii) — correct the documentation to the enforced bound

**Testability: trivially achievable**, since nothing executable changes. The existing three AC6 tests
stay green unmodified.

**Costs.** Ten prose sites (B5), of which four are historical artifacts whose retroactive amendment
is itself questionable. The corrected statement would be the audit's formulation
(`feature-audit.2026-09-07T22-40.md:138`): "one lookup per populate invocation on a store whose
address is still null."

**User-visible consequence: none.** The unbounded repetition of a blocking UI-thread COM chain
remains. `spec.md:619-624`'s accepted risk would have to be re-accepted at its true magnitude, which
is unbounded in the number of store re-selections rather than one per open.

**Argument for it.** Direction (ii) is the minimal, zero-behaviour-change option; it preserves the
user's ability to force a retry by re-selecting the store, which under direction (i) becomes
impossible without closing the dialog. Whether that is a feature or an accident is a product
question the research cannot settle.

### Facts that constrain either choice

- The blocking chain is synchronous and on the UI thread (B1) — established, not disputed by either
  direction.
- `spec.md:208-214` (Non-Goals item 8) records the verified finding that **no** existing seam in
  `UtilitiesCS` makes an Outlook COM property read genuinely non-blocking: the only timeout
  primitive dispatches to ThreadPool (MTA) threads and an Outlook interop object is STA-bound, so
  the STA still blocks. A third direction ("make it non-blocking") is therefore not available inside
  this issue's scope, and remains a filed follow-up.
- A hybrid is available and cheap: direction (i) *plus* correcting the three code comments and the
  three `spec.md` statements so the prose matches the newly enforced bound. AC3 already asks for the
  prose correction under either outcome, so the documentation work is not avoided by choosing (i).

---

## C1 — Toolchain command order (from `CLAUDE.md` in this worktree)

Run in this exact order; if any step fails or modifies files, restart from step 1
(`CLAUDE.md:387-390`, restated at `:405-408`):

1. `dotnet tool run csharpier format .` — verify with `dotnet tool run csharpier check .`
   (`CLAUDE.md:192-194`). Always through `dotnet tool run` so the manifest-pinned 1.2.6 is used.
   `dotnet tool restore` is required once per clone or worktree before the first invocation
   (`:190`).
2. `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
   (`CLAUDE.md:201`).
3. `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`
   (`CLAUDE.md:209`).
4. `vstest.console.exe <test-assembly-paths> /EnableCodeCoverage` (`CLAUDE.md:390`).

**`/t:Rebuild`, not `/t:Build`, for steps 2 and 3.** `CLAUDE.md:202` states the reason: MSBuild's
incremental up-to-date check compares timestamps and does not invalidate on a command-line `/p:`
change, so a warm `/t:Build` returns exit 0 with `CoreCompile` skipped on every project and runs no
analyzers — the gate cannot fail. CI uses `/t:Build` only because a runner checkout is always cold.

**Do not add `/p:Nullable=enable` to step 3.** `CLAUDE.md:210-213`: no project carries a `<Nullable>`
element and there is no `Directory.Build.props`, so the property conscripts every file that never
adopted the pragma; it produced 195 errors in `UtilitiesCS.csproj` on 2026-08-10 against zero without
it. Nullable enforcement here is per-file opt-in via `#nullable enable`. All four files this change
touches (`FolderPredictor.cs:1`, `ArchiveStemProjection.cs:1`, `StoreWrapperController.cs:1`,
`StoreWrapperController.Display.cs:1`, `StoreWrapper.cs:1`) already carry the pragma, so CS86xx
diagnostics in them **will** be promoted to errors under step 3.

---

## C2 — Relevant test assemblies

Both defects live in the `UtilitiesCS` assembly and all directly relevant tests live in one test
project.

- **Primary:** `<repo-root>/.claude/worktrees/agent-a7ec162a25bc9de96/UtilitiesCS.Test/bin/Debug/UtilitiesCS.Test.dll`
  — `<AssemblyName>UtilitiesCS.Test</AssemblyName>` (`UtilitiesCS.Test.csproj:16`),
  `<OutputType>Library</OutputType>` (`:13`), `<OutputPath>bin\Debug\</OutputPath>` for the
  Debug|AnyCPU configuration (`:51`). Contains every test named in A4 and B4.
- **Secondary (regression only, no change expected):**
  `<repo-root>/.claude/worktrees/agent-a7ec162a25bc9de96/QuickFiler.Test/bin/Debug/QuickFiler.Test.dll`
  — holds `Controllers/EfcDataModelArchiveRootTests.cs`, the precedent's test suite, and the
  `QfcItemController` folder-handling tests that consume `FolderPredictor.FolderArray` /
  `FolderRowArray`.
- **Secondary (regression only):**
  `<repo-root>/.claude/worktrees/agent-a7ec162a25bc9de96/TaskMaster.Test/bin/Debug/TaskMaster.Test.dll`
  — holds `AppGlobals/AppOlObjectsArchiveRootValidationTests.cs`, which pins the throwing behaviour
  this fix absorbs. The #797 baseline run used exactly this pair (`UtilitiesCS.Test` +
  `TaskMaster.Test`), per `evidence/baseline/phase0-vstest.2026-09-06T22-00.md:7-10`.

`vstest.console.exe` is not on `PATH`; the #797 helper resolved it through `vswhere`
(same evidence file, `:7`). Assemblies must be named explicitly, never discovered by directory scan,
or `.claude/worktrees/**` copies get picked up.

---

## C3 — Known local test hazards and the documented filter expression

**Yes, a documented filter expression already exists.** It is recorded at
`docs/features/active/2026-09-06-folder-settings-never-persist-and-user-email-error-loading-797/evidence/baseline/phase0-vstest.2026-09-06T22-00.md:29-38`
and is reusable verbatim:

```text
TestCategory!=LiveOutlook&FullyQualifiedName!~HelperClasses.ShellUtilities_Tests&FullyQualifiedName!~HelperClasses.ShellUtilitiesStatic_Tests&FullyQualifiedName!~HelperClasses.SysImageListHelperTests&FullyQualifiedName!~EmailIntelligence.OSBrowser_Tests
```

That evidence file also records the operator-precedence rule (same file `:72-75`): `&` binds tighter
than `|` in a vstest filter, so `TestCategory!=LiveOutlook` must be repeated on **every** disjunct of
an `|`-joined expression, and conjunctive exclusion clauses bind only to the disjunct they appear in.

### Fully qualified class names (re-derived from source)

The four stalling shell-icon classes:

| Fully qualified name | Declaration |
|---|---|
| `UtilitiesCS.Test.HelperClasses.ShellUtilities_Tests` | `UtilitiesCS.Test/HelperClasses/ShellUtilities_Tests.cs` — namespace `:7`, class `:10` |
| `UtilitiesCS.Test.HelperClasses.ShellUtilitiesStatic_Tests` | `UtilitiesCS.Test/HelperClasses/ShellUtilitiesStatic_Tests.cs` — namespace `:7`, class `:10` |
| `UtilitiesCS.Test.HelperClasses.SysImageListHelperTests` | `UtilitiesCS.Test/HelperClasses/SysImageListHelperTests.cs` — namespace `:9`, `[TestClass]` `:11`, class `:12` |
| `UtilitiesCS.Test.EmailIntelligence.OSBrowser_Tests` | `UtilitiesCS.Test/EmailIntelligence/OSBrowser_Tests.cs` — `[STATestClass]` `:26`, class `:27` |

The P/Invoke source is `SHGetFileInfo` in `UtilitiesCS/HelperClasses/FileSystem/ShellUtilities.cs`
and `ShellUtilitiesStatic.cs` (the only two files in the repository referencing it).

A fourth file, `UtilitiesCS.Test/HelperClasses/ShellUtilitiesTests.cs`, exists but its class is
**commented out** (`//public class ShellUtilitiesTests` at `:16`, in namespace
`UtilitiesCS.Test.HelperClasses.ObjListViewDemo` at `:13`). It contributes no tests and needs no
filter clause.

### The timing flake (issue #803)

Fully qualified: **`UtilitiesCS.Test.Extensions.DfDeedle_COM_Tests`**
(`UtilitiesCS.Test/Extensions/DfDeedle_COM_Tests.cs` — `[TestClass]` `:34`, class `:35`). The
specific flaky method, named in `phase0-vstest.2026-09-06T22-00.md:50-54`, is
`GetEmailDataInViewAsync_SeparatesTableSnapshotFromDataFrameTransform`.

The #797 protocol for it (same file `:52-54`) is **not** to filter it out but to treat a failure as a
known flake: re-run the scoped invocation once, record both attempts, and name issue 803. It passed
in the #797 baseline, so it was not a baseline failure. Recommend carrying that protocol forward
rather than adding an exclusion clause.

### Helper script availability — a gap the planner must handle

The #797 runs went through `coverage/plan797-helpers.ps1`. **That file does not exist in this
worktree** — a `Glob` for `coverage/*.ps1` returns nothing, because `coverage/` is git-ignored and the
#797 work happened in a different worktree. The filter expression above is transferable; the script
is not. Either re-create an equivalent helper for #812 or invoke `vstest.console.exe` directly with
`/InIsolation`, explicit assembly paths, and a `/ResultsDirectory:` under the git-ignored `coverage/`
tree (a TRX carries `runUser` and `computerName`, so it must not be committed —
`phase0-vstest.2026-09-06T22-00.md:40-42`).

---

## C4 — `InternalsVisibleTo` from `UtilitiesCS` to the test assembly

**Yes, granted, in three places.** The canonical grant is:

- `UtilitiesCS/Properties/AssemblyInfo.cs:19` — `[assembly: InternalsVisibleTo("UtilitiesCS.Test")]`
  (with `:18` `DynamicProxyGenAssembly2` for Moq's dynamic proxies, and `:20` `ToDoModel.Test`).

Two duplicate grants of the same attribute appear at file scope elsewhere in the assembly:

- `UtilitiesCS/HelperClasses/Tokenizer.cs:11`
- `UtilitiesCS/OutlookObjects/Item/OlItemSummary.cs:10`

Consequences for this change:

- An `internal` latch field or `internal` seam on `StoreWrapperController` is directly testable from
  `UtilitiesCS.Test`, with no reflection. This is the same reasoning `TrimStorePrefix` records in its
  own comment (`StoreWrapperController.Display.cs:161-164`): "Declared internal rather than private
  so the pure-function cases are reachable from UtilitiesCS.Test, to which this assembly already
  grants InternalsVisibleTo; internal does not widen the public surface of the controller."
- The `DynamicProxyGenAssembly2` grant at `:18` is what allows Moq to proxy `internal` interfaces and
  override `internal virtual` members (`SelectFolder` at `StoreWrapperController.cs:350` is
  `internal virtual` and is overridden by a test stub at
  `StoreWrapperController_Tests.Launch.cs:475`).
- `UtilitiesCS` does **not** grant `InternalsVisibleTo("TaskMaster")` — documented at
  `UtilitiesCS/OutlookObjects/IOutlookReadinessGate.cs:19` and
  `UtilitiesCS/OutlookObjects/OutlookReadinessGate.cs:22`. Irrelevant to this change, but it means an
  `internal` seam cannot be consumed from `RibbonController`.

---

## Testing implications (strategy only, no test code)

**Defect A.**
1. Extend the existing `Mock<IOlObjects>` helper with a `.Throws(new InvalidOperationException(...))`
   arrangement on `ArchiveRootPath` (A3). One new fixture method beside
   `FolderPredictorRecentsProjectionTests.PredictorWithRecents`.
2. Positive/negative pairs on all three display surfaces so the text-parity contract documented at
   `FolderPredictor.cs:345-356` is not broken by guarding one surface and not the other:
   `FolderArray`, `FolderRowArray`, and — depending on the A2 scope decision —
   `AddSuggestions`/`AddSuggestionRows` through a populated `Suggestions`.
3. An `act.Should().NotThrow()` assertion plus an equality assertion that the entries render exactly
   as stored (identity), which is the AC1 outcome.
4. A null-root case on `ArchiveStemProjection.ToDisplayStem` to close the gap identified in A5.
5. Location: a new file or the 212-line `FolderPredictorRecentsProjectionTests.cs`. Never
   `FolderPredictorTests.cs` (1066 lines).
6. Warning verification: `logger.Warn` cannot be asserted without mutating the process-global log4net
   repository, which UT4 forbids. The repo's established alternative is an injected diagnostic sink
   observed by tests — see `OutlookFolderHierarchyProvider.ErrorSink` (`:96-101`), whose XML doc
   states the rationale verbatim ("so no test mutates the process-global logger repository"). If AC1
   is to assert the warning at all, that sink pattern is the only in-repo precedent; otherwise the
   AC should assert non-throw + identity and leave the log unasserted.

**Defect B (if direction (i) is chosen).**
1. Reuse `CreateControllerWithViewer()` (B4) and `CreateDisplayFailingSmtpRootFolder(...)`.
2. `Mock.Verify(..., Times.Once())` on the `ExchangeUser.PrimarySmtpAddress` getter after two
   `PopulateWithCurrent()` calls on one controller.
3. A second controller instance proves the next dialog open gets a fresh attempt (this replaces
   AC2's untestable "a new `Launch` permits one more").
4. `Times.Never()` when `UserEmailAddress` is already populated — extends the existing
   `Display.cs:88` test with a direct verification instead of the indirect value-unchanged proof.
5. Location: `StoreWrapperController_Tests.Display.cs` (252 lines). Never
   `StoreWrapperController_Tests.Launch.cs` (480 lines).
6. All three existing AC6 tests must remain green unmodified except for the comment correction at
   `:90-92`.

**Determinism.** Every test above is pure Moq over interfaces and mocked Outlook interop; no live
Outlook process, no filesystem, no temp file, no timer. Consistent with UT1, UT4, and the
`[DoNotParallelize]` already on `StoreWrapperController_Tests` (`:14`), which exists because several
tests swap the process-global `MyBox.DialogInvoker`.

---

## Explicit gaps and unknowns

- **Not executed:** no build, no test run, no `csharpier` invocation. The sandbox in this worktree
  refuses most `pwsh` invocations, so every finding above is derived by reading files. Line counts
  were derived with `Grep pattern="^" output_mode=count`, which counts lines; they have not been
  cross-checked against a `Get-Content | Measure-Object` run.
- **`coverage/plan797-helpers.ps1` is absent** from this worktree (C3). The filter expression
  transfers; the runner does not.
- **The log-assertion question for AC1 is unresolved** (see Testing implications, item 6). The
  research identifies the only in-repo precedent but does not choose.
- **Whether `QfcItemController.FolderHandling.cs:233` should be folded in** is a scope decision, not
  a research finding. The evidence that it is on the same call frame is firm; the decision is the
  spec's.
- **`spec.md:490-493` of #797 contains an internal contradiction** (it states both "runs on every
  store re-selection" and "at most once per dialog open"). Whether that was an authoring error or a
  deliberate elision is not determinable from the artifacts.
