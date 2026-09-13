# Orchestrator Citation Verification (Issue #743)

Timestamp: 2026-09-12T13-50
Collected by: orchestrator (preparation mode), independently of `Agent(task-researcher)`
Method: Read, Grep and Glob against the worktree at
`C:/Users/DanMoisan/repos/TaskMaster/.claude/worktrees/agent-a190dd2fffe21a25d`
EXIT_CODE: 0

The delegation prompt supplied a maintainer file list dated 2026-09-11 and instructed that every line
number be verified rather than assumed. This artifact records that verification. Four of the supplied
locations are wrong and are corrected below. The corrections matter beyond bookkeeping, because the
blast-radius extractor harvests backticked paths, so a wrong path propagates into run scheduling.

## Path corrections

| Supplied | Actual | Status |
|---|---|---|
| `QuickFiler/Interfaces/IItemViewer.cs` | `QuickFiler/Viewers/IItemViewer.cs` | CORRECTED — no `QuickFiler/Interfaces/` directory exists |
| `QuickFiler.Test/Controllers/WebView2BreadcrumbHostTests.cs` | `QuickFiler.Test/Viewers/WebView2BreadcrumbHostTests.cs` | CORRECTED — the file is under `Viewers/` |

## Line-number verification

| Supplied citation | Result |
|---|---|
| `QuickFiler/Viewers/ItemViewer.cs` 23-29 | CONFIRMED. Constructor at 23-29; `_context = SynchronizationContext.Current;` at 26 and `_uiDispatcher = Dispatcher.CurrentDispatcher;` at 27. |
| `QuickFiler/Viewers/ItemViewer.cs` 59-62 | CONFIRMED. `public SynchronizationContext UiSyncContext` get-only property at 59-62. |
| `QuickFiler/Viewers/IItemViewer.cs` 37 | CONFIRMED as to line, after the path correction. Line 37 is `SynchronizationContext UiSyncContext { get; }`. Line 36 is `Dispatcher UiDispatcher { get; }`. |
| `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs` 64 | CONFIRMED. `await _itemViewer.UiSyncContext;`. |
| `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs` 320, 331, 336 | NOT CONFIRMED. None of those three lines references `UiSyncContext` or `UiDispatcher`. |

The actual marshalling sites in `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs` are lines 64,
282, 287, 298, 303 and 371. Lines 36, 273 and 365 are comment references. Line 371 is
`await _itemViewer.UiDispatcher.InvokeAsync(...)`, the dispatcher path rather than the
synchronization-context path; the plan must treat the two paths separately.

## Independent count of the `PumpTimeoutMs` family

The #729 research asserts 4 declarations and exactly 19 usages, all as `[Timeout(...)]` arguments. Both
figures still hold against the current tree. A repository-wide Grep for `PumpTimeoutMs` over `*.cs`
returns 23 occurrences: 4 declarations and 19 usages.

Declarations, all with value 60000:

- `QuickFiler.Test/Viewers/WebView2BreadcrumbHostTests.cs` line 25, `private const int`
- `QuickFiler.Test/Controllers/QfcItemController.ViewerSetupTests.cs` line 34, `private const int`
- `QuickFiler.Test/Controllers/QfcItemController.SeamFactoryTests.cs` line 327, `private const int`
- `QuickFiler.Test/Controllers/QfcItemController.InitializationTests.cs` line 38, `internal const int`

Every one of the four declaring-file line numbers the maintainer supplied for these declarations is
correct. Usage distribution: `WebView2BreadcrumbHostTests.cs` 8, `QfcItemController.InitializationTests.Part3.cs` 8,
`QfcItemController.SeamFactoryTests.cs` 2, `QfcItemController.ViewerSetupTests.cs` 1. The
`InitializationTests` declaration is `internal` because `Part3` consumes it across the partial class.

## Coverage-visibility constraint that bears directly on acceptance criterion 4

This is the finding most likely to produce a vacuous acceptance gate, so it is recorded prominently.

`QuickFiler/Controllers/QfcItemController.ViewerSetup.cs` line 47 carries
`[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]` on `internal async Task InitializeWebViewAsync()`,
declared at line 48. The maintainer's primary cited marshalling site, line 64, is INSIDE that method.

`[ExcludeFromCodeCoverage]` makes a member invisible to coverage rather than reporting it as zero percent.
Two consequences follow:

1. An edit confined to `InitializeWebViewAsync` produces no coverage delta at all, so an acceptance
   criterion phrased as "coverage of `QfcItemController.ViewerSetup.cs` is retained" is satisfied
   automatically and verifies nothing.
2. Conversely, extracting logic OUT of that excluded method into a covered member CHANGES the
   denominator. Coverage can then move in either direction for reasons unrelated to test quality, so a
   naive retained-or-improved comparison can produce a false failure or a false pass.

The acceptance criterion must therefore be phrased against a named test and an explicitly stated
denominator, not against a bare per-file percentage.

Separately, `QuickFiler/Viewers/ItemViewer.cs` line 20 carries a TYPE-level `[ExcludeFromCodeCoverage]`.
The whole `ItemViewer` type is invisible to coverage, so no acceptance criterion may be phrased over
ItemViewer coverage at all.

## Incidental pre-existing defect

`QuickFiler/Controllers/QfcItemController.ViewerSetup.cs` lines 36-37 contain a stale comment reading
"the `await _itemViewer.UiSyncContext` on line 55". The statement is on line 64. This is a documentation
drift, not a behavior defect. It is recorded for the reviewer; repairing it is optional and is not an
acceptance criterion.

## Non-SDK-style project facts

Both projects use explicit `Compile` item lists, so adding or deleting any `.cs` file requires editing
the owning project file.

- `QuickFiler/QuickFiler.csproj`. `ItemViewer` is already a multi-part partial type whose secondary parts
  carry a `DependentUpon` child, for example
  `<Compile Include="Viewers\ItemViewer.DisplayState.cs">` followed by
  `<DependentUpon>ItemViewer.cs</DependentUpon>`. A new `ItemViewer` partial must follow that shape.
  `<Compile Include="Viewers\IItemViewer.cs" />` at line 400 is the plain shape.
- `QuickFiler.Test/QuickFiler.Test.csproj`. Test entries are the plain shape, for example
  `<Compile Include="Controllers\QfcItemController.UiThreadDispatcherFixture.cs" />` at line 194.

## Addendum 2026-09-12T15-20 — reconciliation with the delegated research artifact

Three points where the orchestrator's independent measurement and the research artifact differ or where
the research needs a refinement. All three were re-measured against the tree for this addendum.

1. **Concrete-cast count.** The research artifact states 14 `(ItemViewer)_itemViewer` casts. A raw Grep
   returns 19 occurrences across three files: `QfcItemController.EventWiring.cs` 6,
   `QfcItemController.Initialization.cs` 8, `QfcItemController.ViewerSetup.cs` 5. The difference is
   comment lines. In `QfcItemController.ViewerSetup.cs` the occurrence at line 39 is inside a comment
   block, leaving 4 live casts in that file. Both figures are defensible; 14 is the live count and 19 the
   raw occurrence count. A plan task that asserts a count must state which it means.

2. **`GetAllChildren` has two call sites, not one.** The research cites
   `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs:288`. The same extension is also called at
   `:234`. The declaration is `public static IEnumerable<Control> GetAllChildren(this Control root)` at
   `UtilitiesCS/Extensions/WinFormsExtensions.cs:146`, with a second overload taking an exclusion list at
   `:160`. Because it extends `System.Windows.Forms.Control` and `IItemViewer` does not derive from
   `Control`, neither call site can be routed through the interface. The research finding is confirmed and
   its blast radius is one site larger than stated.

3. **File-length headroom confirmed exactly.** `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs`
   is 467 lines. The repository limit in `.claude/rules/general-code-change.md` is 500 lines for
   production code, so there are 33 lines of headroom. This file is simultaneously the primary edit target
   for this item and the declared edit target of a sibling item in the same parallel run, which makes it
   the highest-risk file in the write set on both counts. A plan that adds more than 33 lines to it
   violates the file-size rule and must extract to a new partial instead, which in turn requires a
   `QuickFiler/QuickFiler.csproj` `Compile` entry.

## Addendum 2026-09-12T15-45 — `ItemViewer.Designer.cs` initialization citations

Verified directly against `QuickFiler/Viewers/ItemViewer.Designer.cs`:

| Location | Content |
|---|---|
| `:46` | `this._l0vhBreadcrumb_WebView2 = new Microsoft.Web.WebView2.WinForms.WebView2();` |
| `:49` | `this._l0v2h2_WebView2 = new Microsoft.Web.WebView2.WinForms.WebView2();` |
| `:89` | `BeginInit()` on `_l0v2h2_WebView2` |
| `:90` | `BeginInit()` on `_l0vhBreadcrumb_WebView2` |
| `:92` | `BeginInit()` on `_topicThread` |
| `:6165` | `EndInit()` on `_l0v2h2_WebView2` |
| `:6166` | `EndInit()` on `_l0vhBreadcrumb_WebView2` |
| `:6169` | `EndInit()` on `_topicThread` |

Two corrections follow.

1. **The `EndInit` citation in the #511 and #571 closing comments is off by one line.** Both state
   `:6166-6167`. The WebView2 `EndInit` pair is at `:6165-6166`. The substance of the premise correction
   is unaffected, because the pair exists and does create the handles. The drift is worth noting only
   because acceptance criterion 5 involves those comments, and a reconciliation that repeats a wrong line
   number propagates it further.

2. **A third control participates in the same initialization protocol.** `_topicThread` is begin-inited at
   `:92` and end-inited at `:6169`. The issue record consistently describes the fixture cost as two
   WebView2 children; there are three `ISupportInitialize` participants. This does not change the
   direction of the finding, and the two WebView2 controls are very likely the dominant pair, but a plan
   that asserts a count of initialized controls should say three rather than two, or should scope its
   claim to WebView2 controls specifically.

## Output Summary

Two supplied paths were wrong and are corrected. Three of four supplied `ViewerSetup.cs` line numbers do
not resolve and the real marshalling sites are enumerated. The #729 counts of 4 declarations and 19
usages both still hold. The controlling risk for acceptance criterion 4 is that the primary cited edit
site is already excluded from coverage measurement, which makes a bare per-file coverage criterion
unfalsifiable.
