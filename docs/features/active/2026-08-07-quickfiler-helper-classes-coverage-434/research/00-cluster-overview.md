# F4 Cluster Overview — cross-cutting research for `quickfiler-helper-classes-coverage` (#434)

Timestamp: 2026-08-07T22-05

Scope of this document: cross-cutting facts that every per-file F4 research artifact and the F4
atomic plan depend on. Authored by the MOVE-MONITOR cluster researcher because that cluster is the
smallest. All claims below are verified by reading files in the worktree
`C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-a04d34f22febda6bf` and are cited
`file:line`.

Upstream contract: child F1 (`quickfiler-coverage-denominator-and-exemption-ledger`) owns (a) the
per-file line-coverage harness derived from the Cobertura output of `Invoke-MSTestWithCoverage.ps1`
and (b) the ratified exemption ledger at
`docs/features/epics/quickfiler-per-file-coverage/coverage-ledger.md`. Neither exists on disk yet;
this document does not define an alternative coverage measurement and does not treat their absence
as a blocker. Every numeric per-file coverage figure is captured at execution time via F1's harness.

---

## 1. Test-project wiring

### 1.1 Target framework and project shape

| Fact | Value | Evidence |
| --- | --- | --- |
| Test project | `QuickFiler.Test` | `QuickFiler.Test/QuickFiler.Test.csproj:16-17` |
| Target framework | `v4.8.1` (.NET Framework 4.8.1) | `QuickFiler.Test.csproj:18` |
| Package management | legacy `packages.config`, non-SDK project | `QuickFiler.Test/packages.config`; `QuickFiler.Test.csproj:176-177` |
| Project GUID | `{834DE5B0-03C8-4483-A4BC-A7975064C5A2}` | `QuickFiler.Test.csproj:13`; solution entry `TaskMaster.sln:25` |
| Project references | `QuickFiler`, `UtilitiesCS`, `TaskVisualization` | `QuickFiler.Test.csproj:414-425` |

### 1.2 Test framework and library versions

| Package | Version | Evidence |
| --- | --- | --- |
| `MSTest.TestFramework` | 4.3.3 | `packages.config:119`; ref `QuickFiler.Test.csproj:312-317` |
| `MSTest.TestAdapter` | 4.3.3 | `packages.config:118`; props `QuickFiler.Test.csproj:4` |
| `MSTest.Analyzers` | 4.3.3 (developmentDependency) | `packages.config:112-117`; `QuickFiler.Test.csproj:433-434` |
| `Moq` | 4.20.72 | `packages.config:111`; ref `QuickFiler.Test.csproj:309-311` |
| `Castle.Core` (Moq's DynamicProxy) | 5.2.1 | `packages.config:6`; ref `:187-189` |
| `FluentAssertions` | 8.10.0 | `packages.config:8`; ref `QuickFiler.Test.csproj:193-195` |
| `Microsoft.Bcl.TimeProvider` | 10.0.10 | `packages.config:18`; ref `:205-207` |
| `Microsoft.Extensions.TimeProvider.Testing` (`FakeTimeProvider`) | 10.8.0 | `packages.config:84-88`; ref `:255-257` |
| `altcover` | 8.6.45 | `QuickFiler.Test.csproj:8, 438` |

Analyzer stack wired into the test project: Meziantou.Analyzer 3.0.138, SonarAnalyzer.CSharp
10.31.0.145097, Roslynator.Analyzers 4.15.0, AsyncFixer 2.1.0,
Microsoft.CodeAnalysis.BannedApiAnalyzers 5.6.0, with the repo-root `BannedSymbols.txt` as an
`AdditionalFiles` entry (`QuickFiler.Test.csproj:459-471`). **Banned symbols apply to test code as
well as production code**: `DateTime.Now`, `DateTime.UtcNow`, `Random.Shared`, `Thread.Sleep`,
`Task.Delay` (`.claude/rules/csharp.md:79`). RS0030 is held at `suggestion` for rollout, but the F4
plan must not introduce new call sites regardless.

### 1.3 How test files are declared — UNAMBIGUOUS ANSWER

**Test files are declared by explicit `<Compile Include="..."/>` entries. There is no globbing.**
The `<ItemGroup>` at `QuickFiler.Test.csproj:57-169` lists every single `.cs` file individually; the
existing `Helper Classes` block is `QuickFiler.Test.csproj:158-165`.

Consequence for the F4 plan: **every new test file requires an edit to
`QuickFiler.Test/QuickFiler.Test.csproj`**, a file shared with all thirteen sibling children of epic
#136. This is the single highest-probability merge-conflict surface for this epic.

Mitigation the plan should adopt: insert all F4 `<Compile Include>` entries **inside the existing
contiguous `Helper Classes\` block at `QuickFiler.Test.csproj:158-165`**, in alphabetical order.
Sibling children own the `Controllers\` block (`:58-151`) and the `Viewers\` block (`:60-91`), which
are textually distant regions. Contiguous same-region insertion by a single child produces one git
hunk that does not overlap sibling hunks, so a three-way merge resolves without conflict in the
common case.

### 1.4 `SetupAssemblyInitializer.cs`

`QuickFiler.Test/SetupAssemblyInitializer.cs` declares `[TestClass] public class
SetupAssemblyInitializer` with a single `[AssemblyInitialize] public static void
AssemblyInit(TestContext context)` (`:11-20`). The method body performs exactly two calls:

```
System.Windows.Forms.Application.EnableVisualStyles();                     // :18
System.Windows.Forms.Application.SetCompatibleTextRenderingDefault(false); // :19
```

Isolation impact:

- It mutates **process-global WinForms state** once per test-assembly load. It does not create,
  show, or dispose any form, does not start a message loop, and does not initialize
  `UtilitiesCS.UiThread`.
- `SetCompatibleTextRenderingDefault` throws `InvalidOperationException` if any WinForms control has
  already been created in the process. Because the assembly initializer runs before any test, this
  is safe today, but it means **no test may construct a WinForms control before assembly
  initialization**, and it reinforces the epic's rule that unit tests do not construct live forms.
- It does **not** set apartment state and does **not** register any cleanup, so it imposes no
  ordering constraint on individual tests.
- `QuickFiler.Test.csproj:29-30` declares an **empty** `<RunSettingsFilePath>`, so no runsettings is
  bound to the project by MSBuild; parallelization defaults are whatever the invoking runner
  supplies. Individual classes opt out with `[DoNotParallelize]` (see
  `QuickFiler.Test/Helper Classes/EmailMoveMonitorTests.cs:22`).

### 1.5 Internals visibility (required for F4 tests)

- `QuickFiler/Properties/AssemblyInfo.cs:5` — `[assembly: InternalsVisibleTo("QuickFiler.Test")]`.
  Also declared at `QuickFiler/Controllers/QfcHomeController.cs:18`.
- `QuickFiler/Legacy/IAcceleratorCallbacks.cs:5` and
  `QuickFiler/Controllers/QfcHighConfidencePreFilter.cs:11` —
  `[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]`. This is what allows Moq/Castle to
  create proxies of **internal** QuickFiler interfaces such as `IEmailMoveMonitor` (proven at
  `QuickFiler.Test/Controllers/QfcQueuePurePathsTests.cs:119`).

Consequence: F4 tests may construct and exercise `internal` production types (`EmailMoveMonitor`,
`EmailMoveAction`) directly and may mock `internal` QuickFiler interfaces. No `public` widening is
needed anywhere in the F4 set.

---

## 2. Existing test-suite inventory — `QuickFiler.Test/Helper Classes/`

Eight files, 2,425 lines, 58 `[TestMethod]` declarations in total. This is the baseline the F4 plan
must not duplicate.

| # | File | Lines | `[TestClass]` (namespace) | Production type targeted | `[TestMethod]` count |
| --- | --- | --- | --- | --- | --- |
| 1 | `ConversationResolverTests.cs` | 578 | `ConversationResolverTests` (`QuickFiler.Test.HelperClasses`, `:32-33`) | `ConversationResolver` + `ConversationResolver.Loading` partial | 10 |
| 2 | `EmailMoveMonitorTests.cs` | 314 | `EmailMoveMonitorTests` (`QuickFiler.Helper_Classes.Tests`, `:21-23`), `[DoNotParallelize]` `:22` | `EmailMoveMonitor` | 8 |
| 3 | `MailItemInfoTests.cs` | 170 | `MailItemInfoTests` (`Z.Unfinished.QuickFiler.Test`, `:15-16`) | `UtilitiesCS.MailItemHelper` (`:120-123`) — **not an F4 file** | 2 |
| 4 | `QfcThemeHelperTests.cs` | 463 | `QfcThemeHelperTests` (`QuickFiler.Test.HelperClasses`, `:24-25`) | `QfcThemeHelper` | 10 |
| 5 | `TlpCellSnapShotTests.cs` | 122 | `TlpCellSnapShotTests` (`QuickFiler.Test.HelperClasses`, `:20-21`) | `TlpCellSnapShot` | 2 |
| 6 | `TlpCellStatesTests.cs` | 247 | `TlpCellStatesTests` (`QuickFiler.Test.HelperClasses`, `:11-12`) | `TlpCellStates` (declared at `QuickFiler/Helper Classes/TlpCellSnapShot.cs:12`) | 12 |
| 7 | `ViewerQueueCoreTests.cs` | 195 | `ViewerQueueCoreTests` (`QuickFiler.Test.HelperClasses`, `:12-13`) | `ViewerQueueCore` | 6 |
| 8 | `ViewerQueueStaticWrapperTests.cs` | 336 | `ViewerQueueStaticWrapperTests` (`QuickFiler.Test.HelperClasses`, `:12-13`) | `EfcViewerQueue` and `ItemViewerQueue` static wrappers (`:18-21`, `:25`, `:89`) | 8 |

Findings that change the plan's shape:

1. **`MailItemInfoTests.cs` contributes zero coverage to any F4 file.** Its two test method bodies
   are entirely commented out (`:128-137`, `:143-167`, both carrying `//TODO: Incomplete. Need to
   finish setting up the mail item mock`). It targets `UtilitiesCS.MailItemHelper`, not
   `QuickFiler/Helper Classes/cInfoMail.cs`. Its ~110 lines of Moq arrangement
   (`:34-118`) are nevertheless a high-quality, reusable reference for mocking
   `PropertyAccessor` / `AddressEntry` / `Recipient` / `Recipients` / `UserProperty` /
   `UserProperties`.
2. **`cInfoMail.cs` has no executable content.** `QuickFiler/Helper Classes/cInfoMail.cs` contains
   only `using` directives (`:1-10`); the entire `cInfoMail` class is commented out from `:13`
   (`//namespace QuickFiler`) onward, including `//    [Obsolete]` at `:15` and
   `//    public class cInfoMail` at `:16`. Its 231 lines produce **zero sequence points**. The
   correct F4 disposition is a **ledger classification, not test authoring** — see §6 of the
   per-file artifact convention. The `cInfoMail` researcher should confirm and record this rather
   than plan tests.
3. **Five F4 files have no dedicated test file**: `cInfoMail.cs`, `EfcThemeHelper.cs`,
   `EfcViewerQueue.cs` (covered indirectly by `ViewerQueueStaticWrapperTests.cs`),
   `ItemViewerQueue.cs` (likewise), `QfcThemeControlSet.cs`. Plus the three declaration-only files
   `QfEnums.cs`, `IConversationResolver.cs`, `Interfaces/IEmailMoveMonitor.cs`.
4. **Namespace inconsistency exists and should not be "fixed" by F4.** Three distinct test
   namespaces are in use (`QuickFiler.Test.HelperClasses`, `QuickFiler.Helper_Classes.Tests`,
   `Z.Unfinished.QuickFiler.Test`). New F4 test files should adopt the namespace of the test file
   they sit beside for the same production type; renaming existing namespaces is out of scope and
   would create needless diff surface.

---

## 3. Repo-established mocking patterns for Outlook Interop

### 3.1 Verdict

**Outlook Interop types are mocked directly with Moq throughout this repository. That is the
dominant, proven, first-choice pattern. No new wrapper/adapter interface is required to reach an
Interop member.** New adapters are warranted only where the call is `static`, non-virtual, or on a
sealed non-Interop type — none of which applies to `MailItem`, `MAPIFolder`, `Folder`, `Store`,
`Items`, `Application`, `NameSpace`, or `Explorer`, all of which are COM **interfaces** and
therefore proxyable by Castle DynamicProxy.

### 3.2 The enabling build setting (do not break it)

`QuickFiler.Test.csproj:270-272` references `Microsoft.Office.Interop.Outlook` with
`<EmbedInteropTypes>False</EmbedInteropTypes>`; `:318-320` does the same for `office`. Type
embedding (no-PIA) must stay **off** in test projects, because embedded interop types cannot be
proxied reliably. The stale `QuickFiler.Test.csproj.bak:57-59` shows the opposite setting
(`EmbedInteropTypes=True`) — see §6.

### 3.3 Concrete, citable precedents

Property and method mocking on `MailItem` / `Folder`:

- `QuickFiler.Test/Helper Classes/EmailMoveMonitorTests.cs:72-85` — `Mock<MailItem>` /
  `Mock<Folder>` factory helpers with `SetupGet(x => x.EntryID)` and `SetupGet(x => x.Parent)`.
- `QuickFiler.Test/Helper Classes/ConversationResolverTests.cs:276, 318, 355, 383, 486-489, 506,
  549` — `Mock<Folder>`, `Mock<NameSpace>`, `Mock<Application>`, `Mock<MailItem>` with
  `MockBehavior.Strict`.
- `TaskMaster.Test/Ribbon/RibbonControllerTests.cs:363-365` — `Mock<MAPIFolder>` strict.
- `TaskMaster.Test/AppGlobals/AppEventsStoreRehookTests.cs:26-31` — `Mock<Store>` strict +
  `Mock<Items>` loose returned as a tuple.
- `TaskMaster.Test/AppGlobals/AppOlObjectsCoverageTests.cs:252-296` — `Mock<Stores>`,
  `Mock<NameSpace>`, `Mock<Folder>`.
- `TaskMaster.Test/AppGlobals/HookReadinessCoordinatorTests.cs:123` and
  `AppOlObjectsFolderTreeServiceLifecycleTests.Coverage.cs:36-37` —
  `Mock<Microsoft.Office.Interop.Outlook.Application>` / `Mock<Outlook.NameSpace>` strict.
- `QuickFiler.Test/Helper Classes/MailItemInfoTests.cs:18-59` — `MockRepository`-driven graph of
  `PropertyAccessor`, `AddressEntry`, `Recipient`, `Recipients`, `UserProperty`, `UserProperties`.
- `Tags.Test/TagControllerSeamTests.cs:36, 61, 199, 223, 322` — `new Mock<MailItem>().Object`.
- `QuickFiler.Test/Controllers/QfcQueueTests.cs:51`, `QfcQueuePurePathsTests.cs:108-109`,
  `QfcStreamingDequeueConfidenceGateTests.cs:20`, `QfcItemController.SeamCoreTests.cs:39`,
  `QfcItemController.SeamDispatcherTests.cs:164, 338`, `QfcItemController.ViewerSetupTests.cs:88-90`.

**COM event subscribe/unsubscribe** (directly relevant to the MOVE-MONITOR cluster and to any file
that wires Interop events):

- `QuickFiler.Test/Helper Classes/EmailMoveMonitorTests.cs:101-104` —
  `folder.VerifyAdd(f => f.BeforeItemMove += It.IsAny<MAPIFolderEvents_12_BeforeItemMoveEventHandler>(), Times.Once)`.
- `QuickFiler.Test/Helper Classes/EmailMoveMonitorTests.cs:120-123, 128-131, 163-173, 216-231,
  260-263` — the matching `VerifyRemove` form.
- `TaskMaster.Test/AppGlobals/AppEventsCoverageExpansionTests.cs:139-143` —
  `items.SetupAdd(x => x.ItemAdd += It.IsAny<ItemsEvents_ItemAddEventHandler>())` and the
  `SetupRemove` counterparts on `Mock<Items>`.

Together these prove that Moq intercepts the `add_`/`remove_` accessors of Outlook Interop events.
No repository test yet **captures and invokes** a captured Interop handler; the MOVE-MONITOR
artifact `13-EmailMoveMonitor.md` §11 proposes the first such tests, with a documented reflection
fallback that is itself heavily precedented in `QuickFiler.Test` (private-field access at
`QuickFiler.Test/Viewers/BreadcrumbSelectorCoordinatorTests.cs:152-161`,
`BreadcrumbDropDownHostTests.cs:364-377`, `BreadcrumbDropDownIntegrationTests.cs:388-452`, and
`Helper Classes/EmailMoveMonitorTests.cs:33-37`).

**Interop event *raising* on a repo-owned interface** (not the Interop type itself) uses
`Mock.Raise`: `QuickFiler.Test/Controllers/QfcFormControllerSeamTests.cs:178-258`,
`QuickFiler.Test/Viewers/BreadcrumbBridgeCoordinatorTests.cs:80`,
`QuickFiler.Test/Controllers/QfcCollectionControllerDarkModeTests.cs:112, 141`.

### 3.4 Repo-owned wrapper/adapter interfaces that exist and should be REUSED, not reinvented

| Type | Location | Purpose | Owner |
| --- | --- | --- | --- |
| `IMailItemActions` | `QuickFiler/Interfaces/IMailItemActions.cs:12` | Narrow behavioral facade over `MailItem` (Reply / ReplyAll / Forward / Display / UnRead / Save / EntryID) | **F3** (epic.md `:272`) — F4 must not edit |
| `MailItemActionsAdapter` | `QuickFiler/Interfaces/MailItemActionsAdapter.cs:12-46` | Production 1:1 adapter for the above | **F3** (epic.md `:273`) |
| `IEmailMoveMonitor` | `QuickFiler/Interfaces/IEmailMoveMonitor.cs:13` | Move-hook lifecycle facade | **F4** |
| `IFolderWrapper` | `UtilitiesCS/Interfaces/IEmailIntelligence/IFolderWrapper.cs:10` | Folder abstraction for email-intelligence code | UtilitiesCS |
| `IOutlookFolderTreeService` | `UtilitiesCS/OutlookObjects/Folder/IOutlookFolderTreeService.cs:12` | Snapshot folder-tree service | UtilitiesCS |
| `IOlObjects` | `UtilitiesCS/Interfaces/IGlobals/IOlObjects.cs:11` | Outlook object-graph globals | UtilitiesCS |
| `IApplicationGlobals` | `UtilitiesCS/Interfaces/IGlobals/IApplicationGlobals.cs:7` | Application-wide globals aggregate | UtilitiesCS |
| `MailItemHelper` | consumed at `QuickFiler.Test/Helper Classes/MailItemInfoTests.cs:120-123` | Materialized mail projection | UtilitiesCS |

`MailItemActionsAdapter`'s own XML doc (`QuickFiler/Interfaces/MailItemActionsAdapter.cs:5-11`)
records the governing rationale explicitly: the adapter tier exists so that controller methods that
previously called `Mail.*` directly become testable, and *"because `MailItem` is itself a mockable
COM interface, every forward is fully exercised"*. F4 should quote this as the precedent for
preferring direct Moq over new adapters.

---

## 4. Existing clock / time-provider abstraction

**It exists. Its exact type is `System.TimeProvider` (namespace `System`), supplied on .NET
Framework 4.8.1 by the `Microsoft.Bcl.TimeProvider` 10.0.10 backport.** No repo-specific `IClock`,
`ISystemClock`, or `ITimeService` exists anywhere in the solution.

| Role | Type | Evidence |
| --- | --- | --- |
| Abstraction | `System.TimeProvider` | `QuickFiler/Controllers/QfcDatamodel.cs:112` — `internal TimeProvider TimeProvider { get; set; } = TimeProvider.System;` |
| Production default | `TimeProvider.System` | `QuickFiler/Controllers/QfcStreamingDequeueConfidenceGate.cs:69` — `_timeProvider = timeProvider ?? TimeProvider.System;` |
| Constructor-optional injection precedent | `TimeProvider timeProvider = null` | `QuickFiler/Controllers/QfcStreamingDequeueConfidenceGate.cs:37`; `QuickFiler/Controllers/QfcHomeController.cs:41, 54` |
| Property-settable injection precedent | `internal TimeProvider TimeProvider { get; set; }` | `QuickFiler/Controllers/QfcHomeController.Metrics.cs:13-17` |
| Delay via the seam (replaces `Task.Delay`) | `TimeProvider.Delay(...)` | `QuickFiler/Controllers/QfcHomeController.Metrics.cs:222`; `QfcDatamodel.QueueProcessing.cs:173`; `QfcDatamodel.FrameBuilding.cs:43` |
| Reads via the seam (replaces `DateTime.Now`) | `TimeProvider.GetLocalNow().LocalDateTime` | `QuickFiler/Controllers/QfcHomeController.Metrics.cs:27, 107`; `QfcHomeController.cs:77` |
| Test double | `Microsoft.Extensions.Time.Testing.FakeTimeProvider` | `QuickFiler.Test/Controllers/QfcStreamingDequeueConfidenceGateTests.Part2.cs:87, 127, 155, 199, 231, 272, 343, 388, 427`; `.Part3.cs:48, 81, 134`; `.cs:316` |
| Package (production backport) | `Microsoft.Bcl.TimeProvider` 10.0.10 | `QuickFiler.Test/packages.config:18`; `QuickFiler.Test.csproj:205-207` |
| Package (test double) | `Microsoft.Extensions.TimeProvider.Testing` 10.8.0 | `QuickFiler.Test/packages.config:84-88`; `QuickFiler.Test.csproj:255-257` |

`UtilitiesCS.Threading.UiThread.Init` also accepts an optional `TimeProvider?`
(`UtilitiesCS/Threading/UiThread.cs:22, 31-34, 71`), confirming `TimeProvider` is the repository-wide
answer rather than a QuickFiler-local convention.

**Plan instruction:** every F4 seam that needs time must name `System.TimeProvider`, default to
`TimeProvider.System`, and be exercised with `FakeTimeProvider`. Do not introduce a new clock
interface. Guidance is recorded normatively at `.claude/rules/csharp.md:55-63`.

---

## 5. STA test infrastructure

**Repository-wide: STA infrastructure EXISTS.** `[STATestClass]` and `[STATestMethod]` are supplied
by MSTest 4.3.3 (`Microsoft.VisualStudio.TestTools.UnitTesting`, in
`MSTest.TestFramework.Extensions.dll`, referenced at `QuickFiler.Test.csproj:315-317`).

In-use precedents:

- `[STATestClass]`: `UtilitiesCS.Test/Threading/ProgressPane_Tests.cs:28`,
  `UtilitiesCS.Test/Threading/ProgressViewer_Tests.cs:30`,
  `UtilitiesCS.Test/HelperClasses/WindowsForms/WinFormsInteractionTests.cs:10`,
  `UtilitiesCS.Test/HelperClasses/WindowsForms/ScreenAndTableLayoutTests.cs:41`,
  `Tags.Test/TagControllerRendering.StaTests.cs:18`.
- `[STATestMethod]`: `UtilitiesCS.Test/Extensions/WinFormsExtensions_Tests.cs:15, 35, 53, 75, 91,
  109, 122, 133, 149, 177, 197, 217, 284, 302, 323, 344, 409, 436, 456`;
  `UtilitiesCS.Test/HelperClasses/WindowsForms/WinFormsLayoutTests.cs:146, 164, 203, 231, 253, 276`;
  `UtilitiesCS.Test/Threading/ProgressTracker_Tests.cs:366, 411, 478`;
  `Tags.Test/TagControllerRendering.StaTests.cs:21`.
- The `*.StaTests.cs` file-naming convention required by epic.md Shared Design §3 already exists in
  five files: `Tags.Test/CheckBoxControllerWiring.StaTests.cs`,
  `Tags.Test/TagControllerRendering.StaTests.cs`,
  `TaskVisualization.Test/TaskControllerAccelerator.StaTests.cs`,
  `TaskVisualization.Test/TaskControllerAcceleratorKeyboard.StaTests.cs`,
  `TaskVisualization.Test/TaskControllerControlMaps.StaTests.cs`. Production code cross-references
  the convention at `Tags/TagController.Rendering.cs:15`.

**In `QuickFiler.Test`: NO STA infrastructure exists.** There is no `[STATestClass]`, no
`[STATestMethod]`, and no `*.StaTests.cs` file in the project. What exists is manual
apartment-thread construction inside test-support helpers:
`QuickFiler.Test/Controllers/QfcItemController.TestSupport.cs:277, 312` and
`QuickFiler.Test/Controllers/BayesianPerformanceController.TestSupport.cs:45`
(`thread.SetApartmentState(ApartmentState.STA)`).

**No runsettings binds STA to QuickFiler.Test.** `QuickFiler.Test.csproj:29-30` declares an empty
`<RunSettingsFilePath>`. The repository's `TaskMaster.runsettings` contains only an `<MSTest>
<Parallelize>` block (`:3-8`) and coverage `<ModulePath>` filters (`:22`); it sets no
`ExecutionThreadApartmentState`. The other runsettings files
(`UtilitiesCS.Test/test.runsettings`, `TaskVisualization.Test/coverage.runsettings`,
`TaskTree.Test/coverage.tasktree.runsettings`, `scripts/vscode/TaskMaster.cli.runsettings`) are not
bound to `QuickFiler.Test`.

**Conclusion for the F4 plan:** the *attributes* are already available with zero new packaging work,
but the *first* `QuickFiler.Test` STA file would be a new convention for this project. Adopt the
`Tags.Test`/`TaskVisualization.Test` shape verbatim (`<Name>.StaTests.cs` + `[STATestClass]` +
`[STATestMethod]`) if and only if a seam is proven infeasible, per epic.md Shared Design §3. Do not
add a QuickFiler-specific `.runsettings` — that would be a new shared configuration surface and is
unnecessary given attribute-level scoping. For the MOVE-MONITOR cluster specifically, **no STA test
is required** (see `13-EmailMoveMonitor.md`).

---

## 6. `QuickFiler.Test/QuickFiler.Test.csproj.bak`

**What it is:** a stale historical snapshot of the test project file, superseded by the live
`QuickFiler.Test.csproj`. Evidence of staleness, all from the `.bak`:

| Property | `.bak` value | live `.csproj` value |
| --- | --- | --- |
| `TargetFrameworkVersion` | `v4.7.2` (`.bak:14`) | `v4.8.1` (`.csproj:18`) |
| `MSTest.TestAdapter` | 2.2.10 (`.bak:4`) | 4.3.3 (`.csproj:4`) |
| Interop embedding | `<EmbedInteropTypes>True</EmbedInteropTypes>` (`.bak:57-59`) | `False` (`.csproj:270-272`) |
| Test framework ref | `Microsoft.VisualStudio.TestPlatform.TestFramework` v14 (`.bak:60`) | `MSTest.TestFramework` 4.3.3 (`.csproj:312-314`) |
| `<Compile Include>` entries | 5 total (`.bak:82, 85, 88, 89, 90`), naming `AcceleratorParser_Test.cs` and `UnitTest1.cs`, neither of which exists today | 100+ entries (`.csproj:57-169`) |
| `Helper Classes\` entries | none (grep for `Helper Classes\` / `EmailMoveMonitorTests` in the `.bak` returns no match) | 8 entries (`.csproj:158-165`) |

**Hazard verdict: not a build hazard; a low-grade repository-hygiene hazard.**

- **Nothing references it.** A repository-wide grep for `csproj.bak` returns no match in any file.
  `TaskMaster.sln:25` names `QuickFiler.Test\QuickFiler.Test.csproj` only. MSBuild does not glob
  `*.bak`, and the file is not a `<None Include>` item in the live project
  (`QuickFiler.Test.csproj:175-178` lists only `app.config` and `packages.config`).
- **It is almost certainly tracked in git.** `.gitignore:251` ignores `*.rptproj.bak` only; there is
  no `*.bak` or `*.csproj.bak` rule. So the file ships in the repository and in every worktree.
- **The residual risk is human/agent error**, not tooling: an agent editing "the test csproj" could
  match the wrong path, and its `EmbedInteropTypes=True` line is a directly misleading precedent for
  the §3.2 rule above.

**Instruction for the F4 plan:** do not modify, delete, or reference
`QuickFiler.Test/QuickFiler.Test.csproj.bak`. Every csproj edit task must name the exact absolute
path `QuickFiler.Test/QuickFiler.Test.csproj`. Removing the `.bak` is defensible repository hygiene
but is **out of F4 scope** — it is a repository-root concern shared with all thirteen siblings and
should be raised as a separate issue through the promotion lifecycle if desired.

---

## 7. Cluster-level constraints carried into every F4 per-file artifact

1. F4's production file set is exactly the 13 files under `QuickFiler/Helper Classes/` plus
   `QuickFiler/Interfaces/IEmailMoveMonitor.cs` (epic.md `:276-283`). Every other QuickFiler file
   belongs to a sibling child running in parallel. A seam that forces an edit to a sibling-owned
   file is a merge conflict.
2. Preferred seam shapes, in order, that keep all existing call sites compiling unchanged:
   new **optional constructor parameter with a production default**; new **overload**; new
   **injectable property defaulting to the real implementation**. `EmailMoveMonitor`'s existing
   `Action<System.Action> marshalToSta = null` parameter (`QuickFiler/Helper Classes/EmailMoveMonitor.cs:38`)
   is the reference implementation of this shape and should be cited as the intra-cluster precedent.
3. Any **new production file** requires a `<Compile Include>` line in `QuickFiler/QuickFiler.csproj`
   (explicit-list project; the `Helper Classes\` block is `QuickFiler/QuickFiler.csproj:342-354`,
   the `Interfaces\IEmailMoveMonitor.cs` entry is `:355`). Same shared-file conflict class as §1.3.
4. No F4 production file currently carries `[ExcludeFromCodeCoverage]` — a grep of
   `QuickFiler/Helper Classes/` for that attribute returns no match, and `QuickFiler/Interfaces/IEmailMoveMonitor.cs`
   has none. F4 therefore inherits none of the epic's 33 disputed attributes and must not add any.
5. Determinism: no `Thread.Sleep`, `Task.Delay`, `DateTime.Now`, `DateTime.UtcNow`, `Random.Shared`
   in production or test code (`.claude/rules/csharp.md:79`, `.claude/rules/general-unit-test.md`
   Determinism Infrastructure). Time comes from `System.TimeProvider` / `FakeTimeProvider` (§4).
6. No temporary files, no live forms, no popups, no UI-thread dependence
   (`CLAUDE.md` UT4; epic.md Shared Design §2).

## 8. Newly identified cross-child risk not listed in epic.md

epic.md "Known Conflict Risks" (`:405-418`) names only #400 and #424. A third is present:

**Issue #426 — `emailmovemonitor-rejected-item-hook-retention`** (promoted
2026-08-07, `docs/features/potential/promoted/2026-08-07-emailmovemonitor-rejected-item-hook-retention.md:9-10`).
It is **promoted but has no active feature folder yet** (`docs/features/active/` contains only
`...-400`, `...-420`, `...-418`, `...-424`, `...-434`), so it is not in flight today. When it goes
active it will land in F4's territory: its stated unit-coverage areas include *"`EmailMoveMonitor`
hook lifecycle"* (`:65`), and its candidate fixes (`:71-73`) edit
`QuickFiler/Controllers/QfcDatamodel.QueueProcessing.cs` (F5-owned) and the
`QfcStreamingDequeueConfidenceGate` rejection path (F2-owned). The F4 plan should record this and
avoid restructuring `QuickFiler.Test/Helper Classes/EmailMoveMonitorTests.cs`, so that #426 can add
to it later without a rebase conflict.
