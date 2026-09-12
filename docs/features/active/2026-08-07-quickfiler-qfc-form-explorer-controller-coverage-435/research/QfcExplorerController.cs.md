# Research — `QuickFiler/Controllers/QfcExplorerController.cs`

## 1. Header

- **Timestamp:** `2026-08-07T00-00` — the date is taken from the session context. The exact wall-clock
  time could not be obtained: this research session has no shell tool (only Read / Grep / Glob /
  Write / Edit / WebFetch). The plan author should re-stamp with a real ISO-8601 time when this
  artifact is folded into `spec.md`.
- **Feature:** F6 `quickfiler-qfc-form-explorer-controller-coverage`, issue #435, epic #136, wave 1, band C3.
- **File under research:**
  `C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-a8220048ded06d508\QuickFiler\Controllers\QfcExplorerController.cs`
- **Exact line count:** 323 lines (line 323 is the closing namespace brace). Matches the epic's
  Feature File Assignments entry (`Controllers/QfcExplorerController.cs` (323) `[X]`).
- **Compiled:** yes — `QuickFiler/QuickFiler.csproj:316`
  `<Compile Include="Controllers\QfcExplorerController.cs" />`. It is therefore inside the coverage
  denominator.
- **Exemption attribute — exact text and location:**
  - `QfcExplorerController.cs:20` — `    [ExcludeFromCodeCoverage]`
  - Applied at **class scope**, immediately above
    `QfcExplorerController.cs:21` — `    internal class QfcExplorerController : IQfcExplorerController`
  - The `using` that supplies it: `QfcExplorerController.cs:4` — `using System.Diagnostics.CodeAnalysis;`
  - There is exactly one `[ExcludeFromCodeCoverage]` occurrence in this file; no member-level
    attributes.
- **Effect on measurement:** the repository harness collects via `dotnet-coverage` with
  `coverage.config` (`scripts/vscode/Invoke-MSTestWithCoverage.ps1:320`, output
  `coverage\coverage.cobertura.xml`, `Invoke-MSTestWithCoverage.ps1:9`). `coverage.config` excludes
  only third-party *module paths* (Deedle, FSharp, Castle.Core, FluentAssertions, Moq,
  Microsoft.Testing, MSTest) — it contains no entry for this file. The class-level
  `[ExcludeFromCodeCoverage]` is therefore the sole mechanism removing this file from the report.
- **Current numeric per-file coverage: unknown and not asserted here.** It cannot be determined
  without running the toolchain, and this session has no shell. The command that produces it is:
  `pwsh -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -CoverageOutput coverage\coverage.cobertura.xml`
  followed by per-file extraction from the Cobertura `<class filename="...">` entries (F1 delivers
  the repeatable per-file report over exactly this output). Because the attribute suppresses
  instrumentation, the expected pre-change observation is that the file is **absent** from the
  Cobertura report rather than present at 0%; F1's harness output is the authority on that.

---

## 2. Consumer map

### 2.1 Construction sites (exhaustive — verified by `\bQfcExplorerController\b` over all `*.cs`)

There are exactly **two** construction sites, and **both are already behind a `Func<>` factory seam**:

| Site | File / line | Shape |
| --- | --- | --- |
| QFC home controller | `QuickFiler/Controllers/QfcHomeController.cs:175-182` | `internal Func<InitTypeEnum, IApplicationGlobals, IFilerHomeController, IQfcExplorerController> QfcExplorerControllerLoader { get; set; } = (initType, globals, homeController) => new QfcExplorerController(initType, globals, homeController);` |
| EFC dependency factory | `QuickFiler/Controllers/EfcHomeControllerDependencyFactories.cs:149-155` | `private static IQfcExplorerController CreateProductionExplorerControllerInstance(...) { return new QfcExplorerController(initType, globals, homeController); }` |

The loader is invoked at `QfcHomeController.cs:92` (`Init()`) and `QfcHomeController.cs:137`
(`InitAsync(...)`). `QfcHomeController.cs` belongs to sibling **F7**;
`EfcHomeControllerDependencyFactories.cs` belongs to sibling **F8**. Both sites are outside F6's
ten-file set.

> **Hard consequence for the seam design:** the public constructor signature
> `QfcExplorerController(QfEnums.InitTypeEnum, IApplicationGlobals, IFilerHomeController)` must be
> preserved exactly. Any new dependency must be introduced as an *optional* settable member, not as
> a new required constructor parameter, or F7 and F8 files would have to change.

### 2.2 Consumption is entirely through the interface

Every consumer holds `IQfcExplorerController`, never the concrete type:

- `QuickFiler/Interfaces/IFilerHomeController.cs:30` — `IQfcExplorerController ExplorerController { get; set; }`
- `QuickFiler/Interfaces/IQfcHomeController.cs:8` — `IQfcExplorerController ExplCtrlr { get; set; }`
- `QuickFiler/Controllers/QfcHomeController.cs:408-413` — backing field + `ExplorerController` property; nulled at `:393`.
- `QuickFiler/Controllers/EfcHomeController.cs:356-361` — same shape; assigned at `:80` and `:229`, nulled at `:346`.
- `QuickFiler/Controllers/QfcItemController.cs:45` — `private IQfcExplorerController _explorerController;`
  assigned at `QfcItemController.Initialization.cs:373`, nulled at `QfcItemController.ViewerSetup.cs:413`.
- `QuickFiler/Controllers/EfcItemController.cs:372` — same shape; assigned at `:70`, nulled at `:271`.

### 2.3 Member call sites (exhaustive, compiled code only)

| Interface member | Call sites in compiled code |
| --- | --- |
| `OpenQFItem(MailItem)` | `QfcItemController.EventWiring.cs:172` and `:238` (keyboard char `'O'`, fire-and-forget `(x) => _ = _explorerController.OpenQFItem(Mail)`); `EfcItemController.cs:699` (same fire-and-forget shape) |
| `BlShowInConversations` | **none** in compiled code |
| `ExplConvView_ToggleOff()` | **none** in compiled code |
| `ExplConvView_ToggleOn()` | **none** in compiled code |
| `ExplConvView_Cleanup()` | **none** in compiled code |
| `ExplConvView_ReturnState()` | **none** in compiled code (only the internal call at `QfcExplorerController.cs:139`) |

All other hits are in `QuickFiler/Legacy/**` (`QuickFileController.cs`, `QfcController.cs`,
`QfcGroupOperationsLegacy.cs`, `IQfcControllerCallbacks.cs`) and `QuickFiler/Notes/**`
(`notes_interfaces.cs`), which the epic establishes are **not** `<Compile Include>` in
`QuickFiler.csproj` and are outside the denominator.

**Net finding:** five of the six interface members have zero production callers. The only externally
exercised member is `OpenQFItem`. Everything else is reachable only from inside this file or from a
test. That means seam changes to this type carry essentially zero blast radius outside F6.

### 2.4 Test-project reachability

`QuickFiler/Properties/AssemblyInfo.cs:5` — `[assembly: InternalsVisibleTo("QuickFiler.Test")]`.
The `internal` class and any `internal` members are directly constructible and settable from
`QuickFiler.Test` with no reflection.

> **Constraint discovered:** there is **no** `[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]`
> in any file compiled by `QuickFiler.csproj`. The only such attribute in the repo is
> `QuickFiler/Legacy/IAcceleratorCallbacks.cs:5`, which is not compiled, and
> `QuickFiler/Properties/AssemblyInfo.cs` belongs to sibling **F15**. Therefore **Moq cannot mock any
> `internal` interface declared in the QuickFiler assembly**, and this research must not propose one.
> Every new seam below is either (a) a delegate property (no proxy needed) or (b) reuses an existing
> `public` interface. `IQfcExplorerController`, `IFilerHomeController`, `IFilerFormController`,
> `IApplicationGlobals`, and `IOlObjects` are all `public` and already Moq-able.

> **ORCHESTRATOR CORRECTION (2026-08-07T21-55) — the constraint above is REFUTED. Do not rely on it.**
>
> `[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]` **is** compiled into the QuickFiler
> assembly. It is declared at `QuickFiler/Controllers/QfcHighConfidencePreFilter.cs:11`, and
> `QuickFiler/QuickFiler.csproj:322` contains `<Compile Include="Controllers\QfcHighConfidencePreFilter.cs" />`.
> The search that produced the original claim appears to have looked only at `Properties/AssemblyInfo.cs`
> and `Legacy/`, missing the assembly-level attribute placed in an ordinary controller file.
>
> Additional compiled assembly-level attributes confirming test reachability:
> `QuickFiler/Controllers/QfcHomeController.cs:18` — `[assembly: InternalsVisibleTo("QuickFiler.Test")]`
> (compiled at `QuickFiler.csproj:325`), and `QuickFiler/Properties/AssemblyInfo.cs:5` — the same.
>
> **Consequence for the plan:** Moq *can* create dynamic proxies for `internal` QuickFiler interfaces.
> An `internal` interface seam is therefore permitted and, under the repository seam hierarchy
> (interface seam > injectable delegate > adapter), is *preferred* over a delegate property wherever
> an interface expresses the collaboration more clearly. The delegate-property designs proposed below
> remain valid options, but they must not be justified on the grounds that no interface seam is
> possible. The atomic-planner must re-evaluate each seam in this artifact against the corrected
> constraint before fixing the design.

---

## 3. Interop dependency inventory

`using Microsoft.Office.Interop.Outlook;` (`:11`) and `using Outlook = Microsoft.Office.Interop.Outlook;` (`:16`).
`QuickFiler.csproj:77` references `Microsoft.Office.Interop.Outlook, Version=15.0.0.0`;
`QuickFiler.csproj:100` references `office, Version=15.0.0.0` (supplies `Microsoft.Office.Core.CommandBars`).
`QuickFiler.Test.csproj:270` and `:318` carry the same two references, plus Moq (`:309`),
FluentAssertions (`:193`), MSTest.TestFramework (`:312`).

| Interop type | Members used (file:line) | Existing repo abstraction? | In-repo Moq precedent |
| --- | --- | --- | --- |
| `Outlook.Explorer` | `CommandBars` (`:57`, `:74`, `:81`), `CurrentView` (`:77`), `CurrentFolder` get (`:127`, `:136`) and set (`:140`), `IsItemSelectableInView(object)` (`:156`), `ClearSelection()` (`:158`), `AddToSelection(object)` (`:159`) | **No wrapper exists.** `UtilitiesCS/OutlookObjects/**` wraps *folders* (`IOutlookFolderTreeService`, `IOutlookFolderHierarchyReader`, `IOutlookFolderNotificationSink`) and *stores*, not `Explorer`. Do not invent one. | Yes — `UtilitiesCS.Test/EmailIntelligence/AutoFile_Tests.cs:58`, `UtilitiesCS.Test/OutlookObjects/Table/OlTableExtensions_Tests.cs:529`, `TaskTree.Test/TaskTreeControllerActivateTests.cs:56,111` |
| `Microsoft.Office.Core.CommandBars` | `GetPressedMso("ShowInConversations")` (`:57`, `:74`, `:81`) | No | Yes — `UtilitiesCS.Test/EmailIntelligence/AutoFile_Tests.cs:56-59` mocks the full `Explorer -> CommandBars -> GetPressedMso` chain |
| `Outlook.View` | `Name` (`:79`, `:88`, `:114`), `XML` get and set (`:83`, `:101`), `Save()` (`:84`, `:102`), `Apply()` (`:85`, `:104`, `:128`), `Copy(string, OlViewSaveOption)` (`:97-100`), `Parent` (`:111`) | No | Yes — `UtilitiesCS.Test/OutlookObjects/Table/OlTableExtensions_Tests.cs:530-531` (`Mock<Outlook.View>` with `v.Name`) |
| `Outlook.Views` | cast from `View.Parent` (`:111`), `foreach` enumeration (`:112`), indexer `Views[string]` (`:127`) | No | **None anywhere in the repo.** This is the single unproven interop type — see §5.4 and §10-Q1 |
| `Outlook.MAPIFolder` | `FolderPath` (`:136`), `Views` (`:127`), used as the assignment target type at `:140` | No | Yes — `UtilitiesCS.Test/OutlookObjects/Table/OlToDoTable_Tests.cs:33`, `TaskMaster.Test/Ribbon/RibbonControllerTests.cs:365` |
| `Outlook.MailItem` | `Parent` (`:136`, `:140`), `Display()` (`:176`) | No | Yes — `TaskTree.Test/TaskTreeControllerActivateTests.cs:55,66` (verifies `m.Display(It.IsAny<object>())`) |
| `Outlook.Application` | `ActiveExplorer()` (`:35`, `:140`) | Yes, indirectly — `UtilitiesCS/Interfaces/IGlobals/IOlObjects.cs:13` exposes `Application App { get; }`, reached from `IApplicationGlobals.Ol` (`IApplicationGlobals.cs:11`) | Yes — `TaskTree.Test/TaskTreeControllerActivateTests.cs:34-41` (`globals.SetupGet(g => g.Ol.App)`) |
| `Outlook.OlViewSaveOption` | `olViewSaveOptionThisFolderOnlyMe` (`:99`) | n/a — plain enum | n/a |
| `Outlook.Folder`, `Outlook.AppointmentItem`, `Outlook.MeetingItem`, `Outlook.TaskItem`, `Outlook.Selection` | only inside the dead `GetCurrentExplorerFolder` (`:278-312`) — `ActiveExplorer.Selection[0]`, `.Parent` | n/a — removed by the recommended refactor | n/a |

**Types NOT touched by this file:** `Outlook.TableView` (only relevant to `OlTableExtensions`),
`Outlook.Table`, `Outlook.NameSpace`, `Outlook.Store`.

### 3.1 Non-interop external couplings

| Coupling | Line | Testability |
| --- | --- | --- |
| `UtilitiesCS.AutoFile.AreConversationsGrouped(Explorer)` — `static`, defined at `UtilitiesCS/EmailIntelligence/EmailParsingSorting/AutoFile.cs:122-137` | `:141`, `:152` | **No seam needed.** It is a static that takes the `Explorer` as a parameter, so mocking the `Explorer` fully controls it. Already proven by `UtilitiesCS.Test/EmailIntelligence/AutoFile_Tests.cs:53,73` |
| `System.Windows.Forms.MessageBox.Show(...)` | `:168-173` | **Seam required.** A popup awaiting human interaction is a unit-test-policy violation (epic Shared Design §2) |
| `Task.Run(...)` | `:154`, `:158`, `:159`, `:180` | Awaited, therefore sequentially deterministic, but a delegate seam removes thread-pool scheduling from the test entirely |
| `log4net.LogManager.GetLogger(...)` static field | `:23-25` | Executes on first type touch; no seam needed; not asserted |
| `System.IO.File.Exists` / `Path.Combine` / `UtilitiesCS.FileIO2.WriteTextFile` | `:223`, `:242` | Inside dead code; would require filesystem I/O, which the unit-test policy prohibits. Resolved by deletion (§5.1) |
| `UtilitiesCS` array extensions `IsInitialized()` (`ArrayExtensions.cs:193`), `SliceRow()` (`ArrayExtensions.cs:102`) | `:187`, `:262` | Inside dead code; already covered by `UtilitiesCS.Test/Extensions/ArrayExtensions_Tests.cs` |
| `QuickFiler.Interfaces.IFilerHomeController` -> `IFilerFormController.MinimizeFormViewer()` (`IFilerFormController.cs:17`) | `:148` | Both interfaces are `public`; Moq-able directly, no seam needed |
| `QfEnums.InitTypeEnum` (`QuickFiler/Helper Classes/QfEnums.cs:5-12`) | `:151`, `:179` | Plain enum. Note it is **not** decorated `[Flags]`, but `HasFlag` is valid on any enum; the bit values 1/2/4/8/16 are declared as powers of two |

---

## 4. Member-by-member reachability table

Classification key: **SR** = seam-reachable as-is (mock the existing injected dependency);
**SRE** = seam-reachable after extraction of pure logic; **IRR** = irreducible; **DEAD** = unreachable
production code recommended for deletion.

| # | Member (lines) | What it does | Interop coupling | Class | Proposed seam / action |
| --- | --- | --- | --- | --- | --- |
| 1 | `log` static field (`23-25`) | log4net logger | none | SR | none needed; executes when the type is first touched |
| 2 | `.ctor(InitTypeEnum, IApplicationGlobals, IFilerHomeController)` (`27-37`) | stores initType/globals/parent, captures `_globals.Ol.App.ActiveExplorer()` | `Application.ActiveExplorer()` | SR | none needed — `Mock<IApplicationGlobals>` + `SetupGet(g => g.Ol.App)` + `Mock<Application>.Setup(a => a.ActiveExplorer())`, exactly as `TaskTree.Test/TaskTreeControllerActivateTests.cs:34-41`. Optionally add `ArgumentNullException` guards (§10-Q2) |
| 3 | fields `_initType`,`_globals`,`_parent`,`_activeExplorer`,`_objView`,`_objViewMem`,`ObjViewTemp` (`39-45`) | state | `Outlook.View` typed | SR | no initializers -> no coverable lines. `ObjViewTemp` is a `public` field on an `internal` class with **no** consumer outside this file (verified) |
| 4 | `BlShowInConversations` get/set (`49-53`) | plain auto-style property over `_blShowInConversations` | none | SR | none |
| 5 | `CurrentConversationState` get (`55-58`) | `_activeExplorer.CommandBars.GetPressedMso("ShowInConversations")` | Explorer + CommandBars | SR | none — mock the chain. **No production caller anywhere** (verified); reachable from `QuickFiler.Test` via `InternalsVisibleTo` |
| 6 | `ExplConvView_Cleanup()` (`61-64`) | `throw new NotImplementedException();` | none | SR | none. The contract today is "always throws". Do **not** implement it here — the legacy body at `QuickFiler/Legacy/QuickFileController.cs:851-869` shows the intended semantics (re-apply remembered view, delete the temp view, clear the flag), and porting it is a behavior change outside a coverage child's mandate. See §10-F1 |
| 7 | `ExplConvView_ReturnState()` (`66-70`) | `if (BlShowInConversations) ExplConvView_ToggleOn();` | none directly | SR | none |
| 8 | `ExplConvView_ToggleOff()` (`72-106`) | if grouped: set flag, capture current view, strip `<upgradetoconv>1</upgradetoconv>` when already on the temp view, remember the view name (substituting `_globals.Ol.ViewWide` when the current view *is* the temp view), find-or-copy the `tmpNoConversation` sibling view, apply it | Explorer, CommandBars, View (Name/XML/Save/Apply/Copy) | SRE | mock the chain; extract three pure decisions into `QfcConversationViewPolicy` (§5.2): XML-marker strip, remembered-name resolution, temp-view-name constant |
| 9 | `GetSiblingView(Outlook.View, string)` (`108-121`) | `(Views)currentView.Parent` then linear scan for `v.Name == viewName`, first match wins | View.Parent, Views enumeration | SRE | extract the scan to a host-neutral generic `FindByName<T>(IEnumerable, string, Func<T,string>)`; the member shrinks to a 3-line cast-and-delegate. `public` on an `internal` class with a single caller (`:93`) — signature change is safe |
| 10 | `ExplConvView_ToggleOn()` (`123-131`) | if flag set: resolve `_activeExplorer.CurrentFolder.Views[_objViewMem]`, `Apply()`, clear flag | Explorer.CurrentFolder, Views indexer, View.Apply | SRE | introduce the `ViewByNameResolver` delegate seam (§5.3) so the one uncertain interop shape (the `Views` indexer) is behind a one-line default |
| 11 | `NavigateToOutlookFolder(MailItem)` private (`133-143`) | if the item's parent folder differs from the current folder: return state, set `CurrentFolder`, recompute the grouped flag | Explorer.CurrentFolder, MailItem.Parent -> MAPIFolder.FolderPath | SRE | promote to `internal` for direct testing; extract the path comparison to `QfcConversationViewPolicy.ShouldNavigate` |
| 12 | `OpenQFItem(MailItem)` async (`146-181`) | minimize the form, navigate, conditionally toggle off, select-or-prompt-and-display, conditionally toggle on | Explorer selection API, MailItem.Display, **MessageBox.Show** | SRE | `PromptUser` delegate seam replaces `MessageBox.Show`; optional `BackgroundRunner` delegate replaces `Task.Run`; the two `HasFlag & ...` decisions extract to `QfcConversationViewPolicy` |
| 13 | `SanitizeArrayLineTSV(ref string[])` private static (`185-201`) | TSV join with sanitization | none | DEAD | delete — no caller; byte-identical logic already lives and is already tested at `UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.cs:1344` (tests: `UtilitiesCS.Test/EmailIntelligence/SortEmail_Tests.cs:302`) and duplicated at `ToDoModel/Email Utilities/SortItemsToExistingFolder.cs:255` |
| 14 | `StripTabsCrLf(string)` internal static (`203-213`) | regex whitespace normalization | none | DEAD | delete — only callers are #13 and #16, both dead. Equivalent already tested at `UtilitiesCS.Test/EmailIntelligence/SortEmail_Tests.cs:141,158` and `EmailFiler_Tests.cs:43` |
| 15 | `WriteCSV_StartNewFileIfDoesNotExist(string, string)` private static (`216-246`) | builds a header row, writes a TSV file | `System.IO.File`, `FileIO2.WriteTextFile` | DEAD | delete — no caller, **and it cannot be covered without filesystem I/O**, which the unit-test policy prohibits. It also carries two latent defects: the `Path.Combine(strFileName, strFileLocation)` arguments are transposed relative to the `WriteTextFile(strFileName, ..., folderpath: strFileLocation)` call, and `strOutput` is `null` when passed by `ref` into `SanitizeArray`, which writes `strOutput[j]` -> guaranteed `NullReferenceException`. Resurrecting it would be a bug |
| 16 | `SanitizeArray(string[,], ref string[])` private static (`249-269`) | row-wise TSV sanitization | none | DEAD | delete — only caller is #15 |
| 17 | `SaveMessageAsMSG(string, IList<MailItem>)` private static (`272-275`) | `throw new NotImplementedException();` | `MailItem` in signature only | DEAD | delete — no caller; a real implementation exists at `UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.cs:1092` and is tested at `SortEmail_Tests.cs:277` |
| 18 | `GetCurrentExplorerFolder(Explorer, object)` private static (`278-312`) | type-switch from selection/item to parent `Folder` | Explorer.Selection, MailItem/AppointmentItem/MeetingItem/TaskItem `.Parent` | DEAD | delete — no caller; equivalent exists at `ToDoModel/Email Utilities/SortItemsToExistingFolder.cs:355` |
| 19 | commented-out `Cleanup_Files()` block (`314-319`) | comment only | none | DEAD | delete with the region |

**Irreducible members: none.** Every executable member is reachable behind an existing constructor
dependency, a proposed delegate seam, or an extracted pure helper. The only residue is the *body of
the default lambda* on one seam (§6).

---

## 5. Seam design proposal

Design intent, in the epic's own priority order: prefer the smallest seam that produces the most
coverage while creating **no new untestable production file**. An adapter that wraps the Explorer/View
COM surface was evaluated and **rejected** — see §5.6 — precisely because it would convert a solvable
coverage problem into a new permanent exemption-ledger entry.

### 5.1 Delete the dead `#region Email Sorting To Rewrite` (lines 183–321)

All six members in that region are `private static` (or `internal static` with only private-static
callers) and have **zero** callers anywhere in the compiled tree, verified by grep for each name.
Every one of them is a verbatim copy of code that already lives — and in three cases is already
tested — in `UtilitiesCS` or `ToDoModel`. Deleting them:

- removes ~139 uncoverable/duplicated lines from this file's coverage denominator;
- removes the file's only filesystem coupling (`File.Exists`, `FileIO2.WriteTextFile`), which the
  unit-test policy would otherwise make untestable;
- removes the `Selection`, `AppointmentItem`, `MeetingItem`, `TaskItem`, `Folder` interop surface from
  the inventory entirely;
- changes no observable behavior, because unreachable private static methods have none.

After deletion, remove whichever `using` directives the analyzer flags as newly unused (IDE0005).
Candidates, each to be confirmed at implementation time: `System.Collections.Generic` (`:2`, used only
by #17), `System.Diagnostics` (`:3`, `Debug.WriteLine` in #16), `System.IO` (`:5`, #15),
`System.Linq` (`:6`, #13/#16), `System.Text` (`:7`, already unused), `System.Text.RegularExpressions`
(`:8`, #14).

### 5.2 New host-neutral pure-logic type

**New file:** `QuickFiler/Controllers/QfcConversationViewPolicy.cs`
**Visibility:** `internal static class` with `public static` members, in `namespace QuickFiler.Controllers`.
Reachable from `QuickFiler.Test` through the existing `InternalsVisibleTo`. No Moq proxy required,
so the missing `DynamicProxyGenAssembly2` attribute is irrelevant here.

Proposed members (all pure, no interop, no I/O, no clock, net48-safe — no `init`, no `record`, no
default interface members):

| Member | Purpose | Replaces |
| --- | --- | --- |
| `public const string TemporaryViewName = "tmpNoConversation";` | single source for the magic string that appears at `:79`, `:89`, `:93`, `:98` | four string literals |
| `public const string ConversationUpgradeMarkup = "<upgradetoconv>1</upgradetoconv>";` | the marker stripped at `:83` and `:101` | two string literals |
| `public static string StripConversationUpgradeMarkup(string viewXml)` | null-safe `Replace` of the marker | inline `.Replace(...)` at `:83`, `:101` |
| `public static string ResolveRememberedViewName(string currentViewName, string wideViewName)` | returns `wideViewName` when `currentViewName == TemporaryViewName`, else `currentViewName` | `:88-90` |
| `public static T FindByName<T>(System.Collections.IEnumerable items, string name, Func<T, string> nameSelector) where T : class` | first-match-wins linear scan, null-safe on a null sequence, returns `null` on no match | the loop body of `GetSiblingView` (`:110-120`) |
| `public static bool ShouldToggleOffForOpen(QfEnums.InitTypeEnum initType, bool conversationsGrouped)` | `initType.HasFlag(Sort) & conversationsGrouped` | `:150-153` |
| `public static bool ShouldToggleOnAfterOpen(QfEnums.InitTypeEnum initType, bool showInConversations)` | `initType.HasFlag(Sort) & showInConversations` | `:179` |
| `public static bool ShouldNavigate(string currentFolderPath, string itemFolderPath)` | ordinal inequality of the two paths | `:135-137` |

`FindByName<T>` takes the non-generic `System.Collections.IEnumerable` deliberately: the production
call passes the COM `Views` collection, and a test passes a plain `List<Outlook.View>` (or even
`List<string>`-shaped fakes for the generic tests). This is what makes the loop 100% coverable
without ever mocking `Views`. It is also the most host-neutral member of the set — a future
WebView2/Office.js port reuses it unchanged.

**Projected size of the new file: 90–110 lines** including XML doc comments. Well under 500.

### 5.3 Injectable delegate seams on `QfcExplorerController`

All three are `internal` settable properties with production-behavior-preserving defaults, following
the seam pattern that already exists in this same assembly — `QuickFiler/Controllers/EfcHomeController.cs:299-305`
(`internal Action<string, string, MessageBoxButtons, MessageBoxIcon> MessageBoxShowAction { get; set; } = (text, caption, buttons, icon) => MessageBox.Show(text, caption, buttons, icon);`)
and `QuickFiler/Controllers/EfcHomeController.ExecuteMoves.cs:22-23`.

| Seam | Declaration | Default | Replaces |
| --- | --- | --- | --- |
| Prompt | `internal Func<string, string, MessageBoxButtons, MessageBoxIcon, DialogResult> MessageBoxShowFunc { get; set; }` | `(text, caption, buttons, icon) => MessageBox.Show(text, caption, buttons, icon)` | `:168-173` — mandatory; without it the test suite shows a modal dialog |
| Background work | `internal Func<System.Action, Task> BackgroundRunner { get; set; }` | `action => Task.Run(action)` | the four `Task.Run(...)` calls at `:154`, `:158`, `:159`, `:180` — optional but recommended, because it lets ordering assertions run inline and removes the thread-pool hop |
| View resolution | `internal Func<MAPIFolder, string, Outlook.View> ViewByNameResolver { get; set; }` | `(folder, name) => folder.Views[name]` | `:127` — de-risks the one interop shape with no in-repo Moq precedent (§5.4) |

Because these are `internal` on an `internal` class, they add nothing to the public surface and
require no change to `IQfcExplorerController` — so no consumer of the interface is affected.

### 5.4 Why the `Views` collection gets a seam and the rest do not

`Explorer`, `CommandBars`, `View`, `MAPIFolder`, `MailItem`, and `Application` all have direct,
working `Mock<T>` precedent in this repository (cited in §3), so no adapter is warranted for them —
introducing one would violate the "smallest seam" rule and duplicate what Moq already does.

`Outlook.Views` is the exception: grep found **no** `Mock<Views>` anywhere in the repo. Two shapes are
consumed here — the `IEnumerable` walk (`:112`) and the indexer (`:127`). The proposed design routes
around both: `FindByName<T>` takes an `IEnumerable` that a test can satisfy with a `List<>`, and
`ViewByNameResolver` puts the indexer behind a one-line default. This removes the last
unproven-mockability risk from the coverage plan at a cost of four production lines.

### 5.5 Behavior preservation for existing consumers

| Consumer | Why it is unaffected |
| --- | --- |
| `QfcHomeController.cs:180-182` (F7) | constructor signature unchanged; the loader lambda compiles as-is |
| `EfcHomeControllerDependencyFactories.cs:155` (F8) | same |
| `QfcItemController.EventWiring.cs:172,238` (F10), `EfcItemController.cs:699` (F9) | call `OpenQFItem(MailItem)` through `IQfcExplorerController`; the interface is unchanged and the default seam values reproduce today's behavior byte-for-byte |
| `IFilerHomeController.cs:30`, `IQfcHomeController.cs:8` (F6/F7 interfaces) | property types unchanged |
| `QuickFiler/Viewers/QfcFormViewer.cs` (F15) | not referenced by this file at all — **no edit** |
| `QuickFiler/Controllers/KeyboardHandler.cs` (F3) | not referenced by this file at all — **no edit** |
| `coverage.config`, `TaskMaster.runsettings`, shared build props (F1) | **no edit** |

### 5.6 Rejected alternatives (brief)

- **`IExplorerConversationViewGateway` adapter over the whole Explorer/View COM surface.** Would make
  the controller 100% pure, but the adapter itself becomes a new thin interop-wiring file with no
  testable logic — i.e. it *creates* a new irreducible-remainder candidate for F1's ledger while
  solving a problem Moq already solves for `Explorer`/`View`. Rejected: it trades one ledger entry for
  another and adds ~80 production lines.
- **STA-threaded tests per the epic's Shared Design §3 last-resort clause.** Not applicable. This is a
  controller, not a form; it constructs no WinForms control. The only UI touch is `MessageBox.Show`,
  which the delegate seam removes. Rejected as unnecessary.
- **Wrapping `AutoFile.AreConversationsGrouped` behind a delegate.** Unnecessary — the static takes the
  `Explorer` as a parameter, so mocking the `Explorer` already controls its result
  (`UtilitiesCS.Test/EmailIntelligence/AutoFile_Tests.cs:53,73`).
- **Adding new constructor parameters for the seams.** Rejected: it would force edits in F7's
  `QfcHomeController.cs` and F8's `EfcHomeControllerDependencyFactories.cs`.
- **Reaching for `Tags/IUserPrompt.cs` as the prompt abstraction.** Rejected: it lives in the `Tags`
  project, which `QuickFiler.csproj` does not reference; adopting it means a new project reference.
  The in-assembly `EfcHomeController` delegate pattern is the closer precedent.

### 5.7 Projected file sizes

| File | Now | After | Basis |
| --- | --- | --- | --- |
| `QuickFiler/Controllers/QfcExplorerController.cs` | 323 | **~180–200** | −139 (dead region) −~6 (unused usings) −~10 (`GetSiblingView` shrinks to a delegate call) −~4 (policy helpers replace inline expressions) +~19 (three seam properties, CSharpier-formatted, plus separating blank lines) |
| `QuickFiler/Controllers/QfcConversationViewPolicy.cs` | — | **~90–110** (new) | 8 members with XML docs |

Both are far below the 500-line ceiling. No partial split is required for this file.

### 5.8 Files outside the F6 ten-file set that must change

No sibling-owned **production** file needs to change. Two **shared project files** do:

- `CROSS-CHILD CONTRACT NOTE (shared build file — QuickFiler/QuickFiler.csproj)`: adding
  `QfcConversationViewPolicy.cs` requires one `<Compile Include="Controllers\QfcConversationViewPolicy.cs" />`
  line in the `Controllers` `ItemGroup` (this is a legacy non-SDK project; files are not globbed).
  This file is not assigned to any sibling in the epic's Feature File Assignments, but **every** wave-1
  child that adds a production file must touch it, so line-adjacent merge conflicts on the integration
  branch are likely. Mitigation: insert in alphabetical position within the existing block and expect
  the child's R1–R5 remediation loop to resolve.
  *If the plan author judges that risk unacceptable*, the fallback is to declare the eight pure helpers
  as `internal static` members inside `QfcExplorerController.cs` itself (projected ~270 lines, still
  under 500, still fully coverable, zero csproj edit) at the cost of the host-neutral-reuse property
  the epic's Non-Goals prefer. Recommendation: take the new file.
- `CROSS-CHILD CONTRACT NOTE (shared test project — QuickFiler.Test/QuickFiler.Test.csproj)`: each new
  test file needs its own `<Compile Include="Controllers\...Tests.cs" />` (see the existing block at
  `QuickFiler.Test.csproj:112-151`). Same shared-file conflict consideration; unavoidable for any child
  that adds tests.

Neither file is `coverage.config` nor a shared build **property** file, so the F1 ownership constraint
is not breached.

---

## 6. Attribute disposition

**Recommendation: remove `[ExcludeFromCodeCoverage]` from `QfcExplorerController.cs:20` entirely, and
propose a residual of zero to F1's ledger.**

Justification, from the member table in §4:

- 12 of the 19 catalogued members are executable production behavior; **all 12** are reachable with the
  design in §5. None is classified `irreducible`.
- 7 members (#13–#19) are dead code recommended for deletion, so they leave the denominator rather than
  entering it as exempt.
- The `[ExcludeFromCodeCoverage]` attribute here is exactly the case the epic's Shared Design §1 calls
  a Blocking finding: it sits on a *testable* seam. The CLAUDE.md § UT2 qualifier "without an
  injectable seam" does not hold — the constructor already injects `IApplicationGlobals` and
  `IFilerHomeController`, both `public` and Moq-able, and every remaining COM dependency reaches the
  file through them.
- The `using System.Diagnostics.CodeAnalysis;` at `:4` should be removed with the attribute if nothing
  else in the file uses that namespace (verify at implementation time).

**Projected post-seam line coverage: high 90s percent.** Method: after the §5 refactor the file has
roughly 60 coverable statements (ctor 4, property accessors 3, `CurrentConversationState` 1,
`ExplConvView_Cleanup` 1, `ExplConvView_ReturnState` 2, `ExplConvView_ToggleOff` ~18,
`GetSiblingView` ~3, `ExplConvView_ToggleOn` ~5, `NavigateToOutlookFolder` ~5, `OpenQFItem` ~15,
plus the three seam-default lambda bodies). Exactly **one** of those statements is not reachable from a
deterministic test: the body of the `MessageBoxShowFunc` default lambda, which calls
`MessageBox.Show` and therefore cannot be invoked without a modal popup. The `BackgroundRunner`
default (`Task.Run`) is coverable by leaving it un-overridden in one test, and the
`ViewByNameResolver` default is coverable if the `Views` indexer proves Moq-able (§10-Q1); if it does
not, the residual is 2 statements instead of 1. That is ~97–98% projected line coverage against an
80% floor.

**This is a projection, not a measurement.** The authoritative figure must come from F1's per-file
harness over `coverage\coverage.cobertura.xml` and be committed under
`<FEATURE>/evidence/qa-gates/`.

**Residual proposed to F1's ledger: none.** No member, and no extracted type, is proposed for
exemption. If implementation discovers that the one-or-two default-lambda statements block the
per-file gate (they will not at a 80% floor), the correct response is a note in the child's evidence,
not an attribute — and any exemption remains F1's ledger's decision to ratify, never this child's.

`QfcConversationViewPolicy.cs` is 100% pure and carries **no** exemption attribute.

---

## 7. Proposed test cases

Four new test files. The split exists because the 500-line ceiling applies to test code as well as
production code (`.claude/rules/general-code-change.md`, "File Size Limit"); a single file holding all
of the cases below would exceed it. Namespace follows the existing convention in
`QuickFiler.Test/Controllers/QfcFormControllerSeamTests.cs:14` — `namespace QuickFiler.Controllers.Tests`.
Each case is one atomic plan task.

### 7.0 `QuickFiler.Test/Controllers/QfcExplorerControllerTestSupport.cs` (no test methods)

Shared, deterministic mock builders — `Mock<IApplicationGlobals>` with `Ol.App.ActiveExplorer()` wired
to a supplied `Mock<Explorer>`; `Mock<CommandBars>` with a configurable `GetPressedMso` result;
`Mock<Outlook.View>` factory taking a name; `Mock<MailItem>` with a parent `Mock<MAPIFolder>` of a given
`FolderPath`; a controller factory that presets `MessageBoxShowFunc`, `BackgroundRunner` (inline), and
`ViewByNameResolver`. Modelled on `QuickFiler.Test/Controllers/QfcItemController.TestSupport.cs`.

### 7.1 `QuickFiler.Test/Controllers/QfcConversationViewPolicyTests.cs`

1. `StripConversationUpgradeMarkup_WhenXmlContainsMarker_RemovesIt` — XML with one `<upgradetoconv>1</upgradetoconv>` returns the XML without it.
2. `StripConversationUpgradeMarkup_WhenXmlHasNoMarker_ReturnsInputUnchanged` — plain XML round-trips.
3. `StripConversationUpgradeMarkup_WhenXmlIsNull_ReturnsNull` — null input does not throw.
4. `StripConversationUpgradeMarkup_WhenXmlIsEmpty_ReturnsEmpty` — boundary: empty string.
5. `StripConversationUpgradeMarkup_WhenMarkerOccursTwice_RemovesEveryOccurrence` — both markers removed.
6. `ResolveRememberedViewName_WhenCurrentIsTemporaryView_ReturnsWideViewName` — `"tmpNoConversation"` maps to the supplied wide-view name.
7. `ResolveRememberedViewName_WhenCurrentIsNamedView_ReturnsCurrentName` — any other name passes through.
8. `ResolveRememberedViewName_WhenCurrentIsNull_ReturnsNull` — boundary: null current name is returned unchanged.
9. `ResolveRememberedViewName_WhenCurrentIsTemporaryViewAndWideNameIsNull_ReturnsNull` — boundary: null substitute.
10. `FindByName_WhenMatchingItemPresent_ReturnsThatItem` — positive path.
11. `FindByName_WhenNoMatchingItem_ReturnsNull` — negative path.
12. `FindByName_WhenSequenceIsEmpty_ReturnsNull` — boundary.
13. `FindByName_WhenSequenceIsNull_ReturnsNull` — invalid input, no throw.
14. `FindByName_WhenDuplicateNamesPresent_ReturnsFirstMatch` — first-match-wins ordering invariant.
15. `FindByName_WhenNameIsNull_ReturnsNull` — invalid input.
16. `ShouldToggleOffForOpen_WhenSortFlagSetAndConversationsGrouped_ReturnsTrue`.
17. `ShouldToggleOffForOpen_WhenSortFlagSetAndConversationsNotGrouped_ReturnsFalse`.
18. `ShouldToggleOffForOpen_WhenInitTypeIsFind_ReturnsFalse` — non-`Sort` init type.
19. `ShouldToggleOnAfterOpen_WhenSortFlagSetAndShowInConversationsRecorded_ReturnsTrue`.
20. `ShouldToggleOnAfterOpen_WhenShowInConversationsNotRecorded_ReturnsFalse`.
21. `ShouldToggleOnAfterOpen_WhenInitTypeIsFind_ReturnsFalse`.
22. `ShouldNavigate_WhenFolderPathsDiffer_ReturnsTrue`.
23. `ShouldNavigate_WhenFolderPathsAreEqual_ReturnsFalse`.
24. `ShouldNavigate_WhenCurrentPathIsNull_ReturnsTrue` — boundary.
25. `ShouldNavigate_WhenBothPathsAreNull_ReturnsFalse` — boundary.
26. `TemporaryViewName_MatchesTheLiteralUsedByOutlookViewNaming` — pins the `"tmpNoConversation"` constant so a rename cannot silently break the round trip.

### 7.2 `QuickFiler.Test/Controllers/QfcExplorerControllerTests.cs` — construction and simple state

27. `Constructor_WhenGlobalsProvided_CapturesActiveExplorerExactlyOnce` — `Application.ActiveExplorer()` verified `Times.Once`.
28. `Constructor_WhenGlobalsProvided_RetainsInitTypeForLaterDecisions` — observable via a later `OpenQFItem` toggle decision.
29. `Constructor_WhenAppGlobalsIsNull_ThrowsArgumentNullException` — depends on §10-Q2; if guards are not added, rename to `..._ThrowsNullReferenceException` and assert current behavior.
30. `Constructor_WhenParentIsNull_ThrowsArgumentNullException` — same dependency on §10-Q2.
31. `Constructor_WhenActiveExplorerReturnsNull_DoesNotThrow` — boundary: today the null is stored without a guard.
32. `BlShowInConversations_ByDefault_IsFalse`.
33. `BlShowInConversations_WhenSetTrue_ReturnsTrue`.
34. `BlShowInConversations_WhenSetTrueThenFalse_ReturnsFalse` — state transition.
35. `CurrentConversationState_WhenRibbonToggleIsPressed_ReturnsTrue` — `CommandBars.GetPressedMso("ShowInConversations")` true.
36. `CurrentConversationState_WhenRibbonToggleIsNotPressed_ReturnsFalse`.
37. `CurrentConversationState_QueriesTheRibbonWithTheShowInConversationsMsoId` — verifies the exact mso identifier string.
38. `ExplConvView_Cleanup_WhenCalled_ThrowsNotImplementedException` — pins today's unimplemented contract.
39. `ExplConvView_Cleanup_WhenCalledTwice_ThrowsOnBothCallsAndLeavesStateUnchanged` — double-cleanup invariant; `BlShowInConversations` unchanged, no Explorer interaction.
40. `ExplConvView_Cleanup_WhenCalledAfterToggleOff_StillThrowsAndDoesNotDeleteTemporaryView` — cleanup-after-setup ordering.
41. `ExplConvView_ReturnState_WhenFlagIsFalse_DoesNotTouchTheExplorer` — no `CurrentFolder` read, no `Apply`.
42. `ExplConvView_ReturnState_WhenFlagIsTrue_AppliesTheRememberedViewAndClearsTheFlag`.
43. `ExplConvView_ReturnState_WhenCalledTwice_AppliesTheRememberedViewOnlyOnce` — the first call clears the flag; idempotence.

### 7.3 `QuickFiler.Test/Controllers/QfcExplorerControllerToggleTests.cs` — toggle-off / toggle-on / sibling view

44. `ExplConvView_ToggleOff_WhenConversationsAreNotGrouped_DoesNothing` — flag stays false, `CurrentView` never read.
45. `ExplConvView_ToggleOff_WhenConversationsAreGrouped_SetsBlShowInConversationsTrue`.
46. `ExplConvView_ToggleOff_WhenCurrentViewIsANamedView_RemembersThatViewName` — later toggle-on resolves that name.
47. `ExplConvView_ToggleOff_WhenCurrentViewIsTheTemporaryView_RemembersTheWideViewNameFromGlobals` — `IOlObjects.ViewWide` substitution.
48. `ExplConvView_ToggleOff_WhenCurrentViewIsTheTemporaryView_StripsUpgradeMarkupSavesAndApplies` — the nested re-entry branch at `:79-87`.
49. `ExplConvView_ToggleOff_WhenCurrentViewIsTheTemporaryViewButRibbonReportsNotPressedOnSecondQuery_SkipsTheXmlRewrite` — covers the inner `GetPressedMso` guard at `:81` returning false on the second call.
50. `ExplConvView_ToggleOff_WhenASiblingTemporaryViewExists_ReusesItWithoutCallingCopy` — `View.Copy` verified `Times.Never`.
51. `ExplConvView_ToggleOff_WhenNoSiblingTemporaryViewExists_CopiesTheCurrentViewWithThisFolderOnlyMeOption` — asserts the exact `OlViewSaveOption.olViewSaveOptionThisFolderOnlyMe` argument.
52. `ExplConvView_ToggleOff_WhenTemporaryViewIsCreated_StripsUpgradeMarkupThenSavesThenApplies` — ordering invariant via `MockSequence`.
53. `ExplConvView_ToggleOff_WhenTemporaryViewIsReused_AppliesItWithoutRewritingItsXml`.
54. `ExplConvView_ToggleOff_Always_AppliesTheTemporaryViewAsTheFinalStep` — ordering invariant.
55. `ExplConvView_ToggleOff_WhenCalledTwiceWhileGrouped_DoesNotCreateASecondTemporaryView` — idempotence.
56. `ExplConvView_ToggleOff_WhenViewCopyThrows_PropagatesTheException` — error handling; `BlShowInConversations` already set to true is observable.
57. `ExplConvView_ToggleOn_WhenFlagIsFalse_DoesNotResolveOrApplyAnyView` — toggle-on-before-toggle-off invariant.
58. `ExplConvView_ToggleOn_WhenFlagIsTrue_ResolvesTheRememberedViewAppliesItAndClearsTheFlag`.
59. `ExplConvView_ToggleOn_WhenFlagIsTrue_ResolvesTheViewFromTheCurrentFolder` — verifies `Explorer.CurrentFolder` is the resolution source.
60. `ExplConvView_ToggleOn_WhenCalledTwice_AppliesTheRememberedViewOnlyOnce` — state transition.
61. `ExplConvView_ToggleOn_WhenTheViewResolverThrows_PropagatesAndLeavesTheFlagSet` — error handling; documents that the flag is cleared only after a successful `Apply`.
62. `ExplConvView_ToggleOffThenToggleOn_RestoresTheOriginalViewName` — the round-trip ordering invariant.
63. `ExplConvView_ToggleOffThenToggleOnThenToggleOn_TheSecondToggleOnIsANoOp`.
64. `ExplConvView_ToggleOnWithoutPriorToggleOff_DoesNotResolveAViewByNullName` — boundary: `_objViewMem` is still null.
65. `ExplConvView_ToggleOn_WithTheDefaultViewByNameResolver_ResolvesThroughTheFolderViewsCollection` — covers the default seam lambda body; depends on §10-Q1.
66. `GetSiblingView_WhenTheParentCollectionContainsAMatchingView_ReturnsThatView`.
67. `GetSiblingView_WhenTheParentCollectionContainsNoMatch_ReturnsNull`.
68. `GetSiblingView_WhenTheParentCollectionIsEmpty_ReturnsNull` — boundary.
69. `GetSiblingView_WhenTheParentCollectionContainsTwoViewsWithTheSameName_ReturnsTheFirst` — first-match-wins.

### 7.4 `QuickFiler.Test/Controllers/QfcExplorerControllerOpenItemTests.cs` — `OpenQFItem` and navigation

70. `OpenQFItem_Always_MinimizesTheFormViewerBeforeTouchingTheExplorer` — ordering invariant via `MockSequence`.
71. `OpenQFItem_WhenTheMailItemIsNull_ThrowsAndNeverChangesTheCurrentFolder` — invalid input; documents today's fail behavior.
72. `OpenQFItem_WhenTheItemIsAlreadyInTheCurrentFolder_DoesNotSetCurrentFolder` — `VerifySet(... CurrentFolder = ...)` `Times.Never`.
73. `OpenQFItem_WhenTheItemIsInADifferentFolder_SetsTheCurrentFolderToTheItemsParent`.
74. `OpenQFItem_WhenTheItemIsInADifferentFolder_CallsReturnStateBeforeChangingTheCurrentFolder` — ordering invariant.
75. `OpenQFItem_WhenTheItemIsInADifferentFolder_RecomputesBlShowInConversationsFromTheRibbon` — the `AutoFile.AreConversationsGrouped` assignment at `:141`.
76. `OpenQFItem_WhenTheItemsParentIsNotAMapiFolder_Throws` — invalid input at the `(MAPIFolder)mailItem.Parent` cast.
77. `OpenQFItem_WhenInitTypeIsSortAndConversationsAreGrouped_TogglesOffBeforeSelecting` — ordering invariant.
78. `OpenQFItem_WhenInitTypeIsFind_DoesNotToggleOff`.
79. `OpenQFItem_WhenConversationsAreNotGrouped_DoesNotToggleOff`.
80. `OpenQFItem_WhenTheItemIsSelectableInView_ClearsSelectionThenAddsToSelection` — ordering invariant.
81. `OpenQFItem_WhenTheItemIsSelectableInView_DoesNotShowAPrompt` — the prompt seam is verified never invoked.
82. `OpenQFItem_WhenTheItemIsSelectableInView_DoesNotDisplayTheMailItem`.
83. `OpenQFItem_WhenTheItemIsNotSelectableInView_ShowsThePromptWithTheExpectedTextCaptionButtonsAndIcon` — pins `"Selected message is not in view. Would you like to open it?"`, `"Error"`, `MessageBoxButtons.YesNo`, `MessageBoxIcon.Error`.
84. `OpenQFItem_WhenTheItemIsNotSelectableAndTheUserAnswersYes_DisplaysTheMailItem`.
85. `OpenQFItem_WhenTheItemIsNotSelectableAndTheUserAnswersNo_DoesNotDisplayTheMailItem`.
86. `OpenQFItem_WhenTheItemIsNotSelectableAndTheUserCancels_DoesNotDisplayTheMailItem` — boundary: a `DialogResult` other than `Yes`/`No`.
87. `OpenQFItem_WhenTheItemIsNotSelectable_NeverAddsToSelection`.
88. `OpenQFItem_WhenInitTypeIsSortAndTheFlagWasRecorded_TogglesOnAfterSelection` — ordering invariant.
89. `OpenQFItem_WhenTheFlagWasNotRecorded_DoesNotToggleOn`.
90. `OpenQFItem_WhenInitTypeIsFindAndTheFlagWasRecorded_DoesNotToggleOn`.
91. `OpenQFItem_WhenTheFormControllerThrows_PropagatesAndNeverTouchesTheExplorer` — error handling.
92. `OpenQFItem_WhenAddToSelectionThrows_PropagatesAndDoesNotToggleOn` — error handling.
93. `OpenQFItem_WithTheDefaultBackgroundRunner_CompletesTheToggleWork` — covers the default `Task.Run` seam lambda; awaited, no sleeps.
94. `OpenQFItem_WhenAwaited_CompletesWithoutDeadlockOnTheCallingThread` — determinism guard for the fire-and-forget call shape used at `QfcItemController.EventWiring.cs:172`.
95. `NavigateToOutlookFolder_WhenFolderPathsMatch_MakesNoExplorerMutation` — direct test of the (promoted to `internal`) method.
96. `NavigateToOutlookFolder_WhenFolderPathsDiffer_PerformsReturnStateThenFolderSwitchThenFlagRecompute` — full ordering invariant.

**Total: 96 proposed test cases across four files** (plus one non-test support file). Each is one
atomic plan task.

---

## 8. Determinism and policy notes

- **Framework/libraries:** MSTest (`[TestClass]`/`[TestMethod]`), Moq 4.20.72
  (`QuickFiler.Test.csproj:309`), FluentAssertions 8.10 (`:193`). All already referenced; no new
  package.
- **No temporary files, no filesystem, no network, no external process.** The §5.1 deletion of the dead
  region removes the only filesystem coupling in the file, so this holds without exception.
- **No live forms, no popups.** The `MessageBoxShowFunc` seam is mandatory for this. Every test must
  set it; the shared support builder should default it to a stub so a forgotten assignment cannot show
  a dialog.
- **No `Thread.Sleep`, no `Task.Delay`, no wall-clock waits.** These are also banned symbols per
  `BannedSymbols.txt` (`.claude/rules/csharp.md`, Analyzer Stack). The `BackgroundRunner` seam set to
  `action => { action(); return Task.CompletedTask; }` makes every `await` complete synchronously, so
  ordering assertions need no timing.
- **No clock or RNG** is used by this file, so no `TimeProvider`/seeded-RNG infrastructure is needed.
- **Moq and COM interfaces.** Outlook interop types are COM *interfaces*, so Castle's dynamic proxy
  creates them normally. Confirmed working in-repo for `Explorer`, `Application`, `MailItem`,
  `MAPIFolder`, `Folder`, `TaskItem`, `Outlook.View`, `TableView`, `Selection`, and
  `Microsoft.Office.Core.CommandBars` (citations in §3).
- **Members needing care:**
  - `MailItem.Display()` is called with no argument at `:176`, but the PIA parameter is optional; the
    Moq verification shape proven in-repo is `m.Display(It.IsAny<object>())`
    (`TaskTree.Test/TaskTreeControllerActivateTests.cs:66`). Use that shape, not `m.Display()`.
  - `Explorer.CurrentView` and `View.Parent` are typed `object`; the production code casts them.
    Set them up returning the mock's `.Object`, as at `OlTableExtensions_Tests.cs:532`.
  - `Explorer.CurrentFolder` is assigned at `:140`, so assert with `VerifySet`, not `Verify`.
  - `Outlook.Views` is the one type with no in-repo Moq precedent. §5.3/§5.4 route both of its uses
    behind seams so the plan does not depend on its mockability; case 65 is the only test that exercises
    the raw indexer and can be dropped if §10-Q1 resolves negatively.
  - ~~**No `internal` QuickFiler interface may be mocked**~~ **REFUTED by orchestrator correction — see
    the correction block in §2.4.** `[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]` is
    declared at `QuickFiler/Controllers/QfcHighConfidencePreFilter.cs:11` and compiled via
    `QuickFiler.csproj:322`. `internal` QuickFiler interfaces ARE Moq-able, so an internal interface
    seam is permitted and is preferred over a delegate property where it expresses the collaboration
    more clearly.
- **Second `ActiveExplorer()` call.** Line `:140` calls `_globals.Ol.App.ActiveExplorer()` again rather
  than reusing `_activeExplorer` captured at `:35`. Tests must set up `ActiveExplorer()` to return the
  same mock instance on repeated calls. See §10-F2.
- **STA last-resort clause (epic Shared Design §3): not invoked.** This type constructs no WinForms
  control and touches no UI thread once `MessageBox.Show` is behind a seam. No `*.StaTests.cs` file is
  proposed and none should be accepted for this file.
- **net48 constraints observed:** no `init` accessors, no `record`/`record struct`, no default interface
  members in any proposed type. All new members are plain `internal static` methods, `const` fields, or
  settable delegate properties.
- **Analyzer/format:** new code must survive `csharpier .`, the analyzer build, and the
  `/p:Nullable=enable /p:TreatWarningsAsErrors=true` build. The `FindByName<T>` generic constraint
  `where T : class` and a `T` return type of `null` are nullable-clean only if the method is annotated
  appropriately for the project's nullable context — confirm the project's `<Nullable>` setting before
  authoring.

---

## 9. Upstream dependency on F1

F1 (`quickfiler-coverage-denominator-and-exemption-ledger`, wave 0) is being prepared concurrently and
its outputs are **not yet on disk**; their absence is expected and is not reported here as a gap or a
blocker. This child consumes three F1 contracts:

1. **Per-file coverage harness.** F1 delivers the repeatable per-file line-coverage report derived from
   the Cobertura output of `scripts/vscode/Invoke-MSTestWithCoverage.ps1` (default output
   `coverage\coverage.cobertura.xml`, `Invoke-MSTestWithCoverage.ps1:9`). This child runs that harness
   after implementation and commits the numeric per-file result for
   `QuickFiler/Controllers/QfcExplorerController.cs` and for the new
   `QuickFiler/Controllers/QfcConversationViewPolicy.cs` under `<FEATURE>/evidence/qa-gates/`.
   Aggregate assembly coverage does not satisfy the acceptance criterion.
2. **Denominator classification.** F1's ledger declares this file `testable`. This research supports
   that classification and finds no irreducible remainder within it. The new
   `QfcConversationViewPolicy.cs` is a *new* file created after F1's 121-file enumeration; F1's ledger
   should be extended to record it as `testable` with a >= 90% target (new-module rule, CLAUDE.md § UT2).
3. **Exemption ratification authority.** `docs/features/epics/quickfiler-per-file-coverage/coverage-ledger.md`
   is the **sole** authority for ratifying any residual exemption. This child proposes a residual of
   **zero** and therefore requests only that the ledger record the removal of the attribute at
   `QfcExplorerController.cs:20`. If implementation discovers a residual that this research did not
   foresee, the child must submit it to F1's ledger and may not decide it unilaterally.

Sequencing: the implementation work in §5 and the tests in §7 do not depend on F1 artifacts and can be
authored in parallel; only the numeric verification step consumes F1's harness.

---

## 10. Open questions and findings

### Questions for the plan author

- **Q1 — `Outlook.Views` indexer shape.** Production line `:127`
  (`_activeExplorer.CurrentFolder.Views[_objViewMem]`) compiles today, so a C# indexer exists on the PIA
  `Views` type, but its declared parameter type (`object` vs `string`) is unverified and there is no
  `Mock<Views>` precedent in the repo. This does **not** block the plan — the `ViewByNameResolver` seam
  routes around it — but it decides whether test case 65 is authorable. Verification: author one Moq
  setup and compile `QuickFiler.Test` (`msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU"`);
  the compiler resolves the signature definitively.
- **Q2 — null guards in the constructor.** The constructor has no argument validation; a null
  `appGlobals` produces a `NullReferenceException` at `:35`. Adding `ArgumentNullException` guards
  aligns with the repo's "enforce invariants at construction" rule and is reachable only from the two
  factory lambdas (both of which always pass non-null), so it is not an observable-flow change. It is
  nonetheless a change in thrown exception type. Decide: add guards (test cases 29–30 as written) or
  pin current behavior (rename those two cases to assert `NullReferenceException`).
- **Q3 — new file vs. in-file statics.** §5.8 gives both options for `QfcConversationViewPolicy`. The
  new file is recommended for host-neutral reuse; the in-file option avoids a `QuickFiler.csproj` edit
  and its integration-branch conflict risk. Decide before the first atomic task.
- **Q4 — scope of the dead-region deletion.** §5.1 recommends deleting lines 183–321 outright. The only
  alternative that preserves the code is to test it, which is impossible for
  `WriteCSV_StartNewFileIfDoesNotExist` without filesystem I/O. Confirm the deletion is in scope for a
  coverage child, or record it as a separate follow-up.
- **Q5 — `BackgroundRunner` seam.** Optional. Without it the four `Task.Run` calls still complete
  deterministically because each is awaited, but ordering assertions cross a thread-pool boundary.
  Recommendation: include it; cost is three production lines.

### Findings the plan author should be aware of

- **F1 — `ExplConvView_Cleanup` is unimplemented and always throws** (`:61-64`). It is declared on the
  `public` `IQfcExplorerController` interface (`IQfcExplorerController.cs:12`) and has no production
  caller, so nothing currently trips it. The intended semantics are visible in the non-compiled legacy
  implementation at `QuickFiler/Legacy/QuickFileController.cs:851-869`. Implementing it is out of scope
  for a coverage child (it is new behavior, not coverage). **Recommend promoting this to its own GitHub
  issue** rather than leaving the observation in a feature folder.
- **F2 — the second `ActiveExplorer()` call** at `:140` bypasses the `_activeExplorer` field captured at
  `:35`. If Outlook returns a different `Explorer` instance between the two calls, the folder switch and
  the subsequent `AutoFile.AreConversationsGrouped(_activeExplorer)` at `:141` would act on different
  objects. This is a latent defect, not a coverage problem; fixing it is a behavior change. Recommend
  promoting to its own issue and, in the meantime, writing the tests against the current two-call shape.
- **F3 — two latent defects in the dead `WriteCSV_StartNewFileIfDoesNotExist`** (`:216-246`): transposed
  `Path.Combine` arguments relative to the `FileIO2.WriteTextFile(strFileName, ..., folderpath: strFileLocation)`
  call, and a guaranteed `NullReferenceException` because `strOutput` is `null` when passed by `ref` into
  `SanitizeArray`, which immediately assigns `strOutput[j]`. This reinforces deletion over resurrection.
- **F4 — five of six interface members have no production caller.** `BlShowInConversations`,
  `ExplConvView_ToggleOff`, `ExplConvView_ToggleOn`, `ExplConvView_Cleanup`, and
  `ExplConvView_ReturnState` are unreferenced outside this file in compiled code. The interface surface
  may be wider than the feature needs, but narrowing it is out of scope here and would touch F7/F8/F9/F10
  files.
- **F5 — `ObjViewTemp` is a `public` mutable field** (`:45`) on an `internal` class, with no consumer
  outside this file. It is effectively private state. Converting it to a private field would be a small
  encapsulation win but is not required for coverage; leaving it public makes some toggle assertions
  easier to write.
- **F6 — shared project-file contention.** Both `QuickFiler/QuickFiler.csproj` and
  `QuickFiler.Test/QuickFiler.Test.csproj` are legacy non-SDK projects with explicit `<Compile Include>`
  item lists. Every wave-1 child that adds a file must edit them. This is not covered by the epic's
  Feature File Assignments and is the most likely source of integration-branch conflicts for this epic.
- **F7 — `QfEnums.InitTypeEnum` is not decorated `[Flags]`** (`QuickFiler/Helper Classes/QfEnums.cs:5`)
  even though the file uses `HasFlag` and the members are powers of two. `HasFlag` works regardless.
  `QfEnums.cs` belongs to sibling F4; do not change it.
- **F8 — current per-file coverage is unmeasured.** No number is asserted anywhere in this artifact. The
  producing command is named in §1.
