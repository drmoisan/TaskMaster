# Research — Deedle / `netstandard` bind unsatisfiable in production (Issue #879)

- **Issue:** #879
- **Work mode:** full-bug
- **Complexity band:** C3
- **Branch under study:** `bug/deedle-netstandard-21-bind-unsatisfiable-in-production-879`
- **Written:** 2026-09-13T19-05
- **Scope:** production half only. The test half landed in PR #880 (issue #877) and is explicitly out of scope.

> Tool constraint for this session: the Bash tool was disabled, so `gh issue view 879` could not be
> executed. The maintainer-verified facts supplied in the delegation brief are treated as given and are
> marked **(given)** where relied upon. Everything else below was verified by reading files in the item
> worktree, by Grep/Glob over that worktree, or by the cited Microsoft documentation.

---

## 1. Current state — what was verified

### 1.1 The two binding-fallback installation sites

Verified by two independent searches (see `## Numeric Derivation Evidence`): the repository contains
exactly two first-party `AppDomain.CurrentDomain.AssemblyResolve` subscription sites.

| Site | Line | Installed from | Reaches production? |
|---|---|---|---|
| `SVGControl/SvgAssemblyResolver.cs` | 41 | `SvgAssemblyResolver.Install()`, called only from the `SVGControl.SvgRenderer` static constructor (`SVGControl/SvgRenderer.cs:25-28`) | Yes, but only after something renders SVG |
| `TestSupport/TestAssemblyResolver.cs` | 45 | `[AssemblyInitialize]` in `QuickFiler.Test/SetupAssemblyInitializer.cs:14-20` and `UtilitiesCS.Test/TestAssemblyInitializer.cs:14-17` | No — test-only, linked into two test projects via `<Compile Include="..\TestSupport\TestAssemblyResolver.cs">` (`QuickFiler.Test/QuickFiler.Test.csproj:230`, `UtilitiesCS.Test/UtilitiesCS.Test.csproj:80`) |

`[AssemblyInitialize]` exists in exactly two files repository-wide (`QuickFiler.Test/SetupAssemblyInitializer.cs`,
`UtilitiesCS.Test/TestAssemblyInitializer.cs`); no other test assembly has one. Verified by content
search for `AssemblyInitialize` over all `*.cs`.

### 1.2 Config facts

- `TaskMaster/app.config:69-71` contains the **production** FSharp.Core redirect
  `oldVersion="0.0.0.0-11.0.0.0" newVersion="11.0.0.0"`. The issue text reads as though the redirect were
  test-only; it is not. Production carries the same redirect, which is why the production trigger exists.
- `QuickFiler.Test/app.config:46-47` carries the same FSharp.Core redirect (matches the line number the
  brief cites).
- No `*.config` file anywhere in the tree contains the string `netstandard`. Verified by content search
  for `netstandard` restricted to `*.config`: zero matches. This confirms the **(given)** fact.
- `TaskMaster/app.config` already redirects ten assemblies that share `netstandard`'s public key token
  `cc7b13ffcd2ddd51` (`System.Memory`, `System.Buffers`, `System.Threading.Tasks.Extensions`,
  `System.Threading.Channels`, `Microsoft.Bcl.AsyncInterfaces`, `System.Text.Json`, and others), so adding
  a `netstandard` entry would be structurally uniform with what is already there.

### 1.3 Deedle references

| Assembly | Carries a Deedle reference | Evidence |
|---|---|---|
| `UtilitiesCS` | Yes | `UtilitiesCS/UtilitiesCS.csproj:57-58` (`Deedle, Version=3.0.0.0`, HintPath `..\packages\Deedle.3.0.0\lib\netstandard2.0\Deedle.dll`), `UtilitiesCS/packages.config:9` |
| `QuickFiler` | Yes | `QuickFiler/QuickFiler.csproj:44-45`, `QuickFiler/packages.config:6` |
| `ToDoModel` | Yes | `ToDoModel/ToDoModel.csproj`, `ToDoModel/packages.config` |
| `TaskMaster` (the add-in) | No `Deedle` reference; `TaskMaster/ThisAddIn.cs:62,99-104` has a `SetUpDeedle()` method that only calls `Console.SetOut(new DebugTextWriter())` — it touches no Deedle type | Read in full |

`TaskMaster/ThisAddIn.cs:99-104` is a false positive for "Deedle reach": the method name mentions Deedle
but the body only redirects `Console.Out` so that `df.Print()` output lands in the debug window. It causes
no Deedle assembly load.

---

## 2. Question 1 — Deedle's production reach

### 2.1 Production types that reference Deedle

Content search for `Deedle` over the three production assemblies that reference it, excluding `docs/`
and `.claude/`:

**`UtilitiesCS`** — `Extensions/DfDeedle.cs`, `Extensions/DfDeedle.FrameUtilities.cs`,
`Extensions/DfDeedle.QfcColumns.cs` (the `public static partial class DfDeedle`, declared at
`Extensions/DfDeedle.cs:25`); plus `using`-only references in `To Depricate/FileIO2.cs:8`,
`EmailIntelligence/SubjectMap/SubjectMapSco.cs:19`, `Extensions/NullExtensions.cs:8`,
`OutlookObjects/Table/OlTableExtensions.cs:14`, `Threading/ThreadSafeFunctions.cs:8`,
`OutlookObjects/Store/StoreWrapper.cs:6`, `HelperClasses/MergeSortImplementations.cs:5`,
`EmailIntelligence/EmailParsingSorting/SortEmail.cs:11`.

A bare `using Deedle;` / `using Deedle.Internal;` directive emits no assembly reference of its own; only
files that name a Deedle type in executable code force the load. The Deedle-typed public surface is
`DfDeedle` (returns `Frame<int, string>`), `SortEmail`, and `SubjectMapSco`.

**`QuickFiler`** — `Controllers/QfcDatamodel.cs:13`, `Controllers/QfcDatamodel.FrameBuilding.cs:4`
(both declare `Frame<int, string>`-typed members), `Controllers/EfcItemController.cs:21`.

**`ToDoModel`** — `Data Model/Tree/TreeOfToDoItems.cs`, `Data Model/Project/ProjectData.cs`,
`Data Model/ID/IDList.cs`.

### 2.2 The call chain from the reproduced ribbon entry point

Verified frame by frame:

1. `TaskMaster/Ribbon/RibbonViewer.cs:159-165` — `QuickFilerHighConfidence_Click(Office.IRibbonControl)`,
   `async void`, awaits `_commandBoundary.RunAsync(..., () => _controller.LoadQuickFilerHighConfidenceAsync())`.
2. `TaskMaster/Ribbon/RibbonController.cs:133-146` — `LoadQuickFilerHighConfidenceAsync()` calls
   `QuickFiler.Controllers.QfcHomeController.LaunchAsync(Globals, ReleaseQuickFiler)`.
3. `QuickFiler/Controllers/QfcHomeController.cs:35-84` — `LaunchAsync` creates a
   `WindowsFormsSynchronizationContext`, a `CancellationTokenSource`, and
   `new ProgressTracker(tokenSource).Initialize()` (line 56), then calls `InitAsync` (line 60).
4. `QuickFiler/Controllers/QfcHomeController.cs:109-151` — `InitAsync`. **Line 123 is the first statement
   of the method body after the field assignments**: it starts `QfcAsyncDataModelLoader(...)`, whose
   default delegate (`QfcHomeController.cs:169-171`) is
   `async (globals, cancel, cancelSource, progress) => await QfcDatamodel.LoadAsync(...)`.
   `_formViewer = new QfcFormViewer()` is line 131, i.e. **after** the data-model task is launched.
5. `QuickFiler/Controllers/QfcDatamodel.cs:54-73` — `LoadAsync` constructs `QfcDatamodel` and awaits
   `model.InitDfAsync(...)`.
6. `QuickFiler/Controllers/QfcDatamodel.FrameBuilding.cs:48-67` — `InitDfAsync` awaits
   `GetEmailsInViewDfAsync`, which at lines 82-89 calls
   `UtilitiesCS.DfDeedle.GetEmailDataInViewAsync(...)`.
7. `UtilitiesCS/Extensions/DfDeedle*.cs` — the first executable frame that touches a Deedle type, forcing
   the load of `Deedle.dll`, `FSharp.Core.dll` (redirected to 11.0.0.0) and, transitively,
   `netstandard 2.1.0.0`.

The synchronous `QfcDatamodel(IApplicationGlobals, CancellationToken)` constructor at
`QuickFiler/Controllers/QfcDatamodel.cs:43-52` calls `InitDf(_activeExplorer)` (line 49), which calls
`DfDeedle.GetEmailDataInView` at `QfcDatamodel.FrameBuilding.cs:15` — a second, synchronous Deedle entry
used by `QfcHomeController.Init()` (`QfcHomeController.cs:86-107`, via `QfcDataModelLoader` at line 161).

`QuickFiler_Click` (`RibbonViewer.cs:151-157`) reaches the same `QfcHomeController.LaunchAsync` and is
therefore equally exposed; `QuickFilerHighConfidence_Click` is not special. The only difference is
`SetHighConfidenceModeForLaunch(true)` at `RibbonController.cs:138`.

---

## 3. Question 2 — `SvgRenderer`'s production reach and ordering

### 3.1 What actually runs the `SvgRenderer` static constructor

- `SVGControl/SvgRenderer.cs:25-28` — `static SvgRenderer() { SvgAssemblyResolver.Install(); }`.
- `SVGControl/ButtonSVG.cs:22` — the `ButtonSVG` constructor does `_imageSVG = new SvgImageSelector(...)`.
- `SVGControl/SvgImageSelector.cs:34,45,49` — every `SvgImageSelector` constructor does `new SvgRenderer(...)`.
- `SVGControl/PictureBoxSVG.cs:28` — constructs a renderer path the same way.

**Correction to the issue text.** `issue.md:111-112` and `spec.md:107-108` state that the resolver is
installed because "the QuickFiler viewers instantiate `SVGControl.ButtonSVG` **and** `SVGControl.SvgResource`".
`SvgResource` is a plain data class declared at `SVGControl/ISvgResource.cs:18-22` with a parameterless
constructor and a `(string, byte[])` constructor; it names no `SvgRenderer` member. **Constructing
`SvgResource` does not install the fallback.** Only `ButtonSVG`, `PictureBoxSVG`, `SvgImageSelector`,
`ToggleSwitch` and direct `SvgRenderer` use do.

### 3.2 Which production assemblies can reach SVG at all

Only four projects reference `SVGControl` (verified by content search for `SVGControl` restricted to
`*.csproj`): `UtilitiesCS`, `QuickFiler`, `SVGControl` itself, and `SVGControl.Test`.
**`TaskMaster.csproj` does not reference `SVGControl`, and no file under `TaskMaster/` names any
`SVGControl` type** (content search over `TaskMaster/` for `SVGControl.`/`ButtonSVG`/`PictureBoxSVG`/
`SvgImageSelector` returned zero matches).

Consequence: nothing in add-in startup, nothing in `ThisAddIn`, and nothing in the ribbon layer can
install the fallback. The add-in can run for an arbitrarily long time with no fallback installed.

The SVG-bearing production sites are:

| Assembly | File | Control |
|---|---|---|
| `UtilitiesCS` | `Threading/ProgressMultiStepViewer.Designer.cs:39` | `PictureBoxSVG` |
| `UtilitiesCS` | `Dialogs/MyBoxViewer.Designer.cs:38` | `PictureBoxSVG` |
| `UtilitiesCS` | `Dialogs/FolderNotFoundViewer.Designer.cs:32-35` | four `ButtonSVG` |
| `UtilitiesCS` | `ReusableTypeClasses/NewSmartSerializable/Config/ConfigViewer.Designer.cs:41-51` | four `ButtonSVG` |
| `QuickFiler` | `Viewers/ItemViewer.Designer.cs:73-81` | six `ButtonSVG` |
| `QuickFiler` | `Viewers/EfcViewer.Designer.cs:49-54` | five `ButtonSVG` |
| `QuickFiler` | `Viewers/Form1.Designer.cs:35` | one `ButtonSVG` |

### 3.3 Ordering verdict for the Deedle path in section 2.2

**No SVG-bearing site is guaranteed to execute first, and on the reproduced path none executes first.**

- `QfcFormViewer` — the form constructed at `QfcHomeController.cs:131`, i.e. still before the awaited
  data model at line 150 — contains **no** SVG control. The content search for SVG types across
  `QuickFiler/` returns hits only in `ItemViewer`, `EfcViewer` and `Form1`; `QfcFormViewer.Designer.cs`
  is not among them.
- `ProgressTracker.Initialize()` (`QfcHomeController.cs:56`) creates a `ProgressViewer`
  (`UtilitiesCS/Threading/ProgressTracker.cs:90`), **not** `ProgressMultiStepViewer`.
  `ProgressViewer.Designer.cs` has no SVG control (not in the SVG hit list).
- `ItemViewer` instances — the first QuickFiler surface that does carry `ButtonSVG` — are produced by
  `QuickFiler/Helper Classes/ItemViewerQueue.cs:105` (`return new ItemViewer();`), which runs while the
  queue is populated from the already-loaded data frame, i.e. strictly **after** the Deedle load.

So the reproduced production ordering is: ribbon click -> `LaunchAsync` -> `InitAsync` -> Deedle load
(fails) -> `ItemViewer`/`ButtonSVG` never reached. The add-in "appeared safe" historically only if a
user first opened some other SVG-bearing surface (a `MyBox` dialog, the config viewer, the
folder-not-found dialog, or a previous QuickFiler session) in the same Outlook process.

---

## 4. Question 3 — config-file reality for a VSTO add-in **(decides remedy candidate 1)**

### 4.1 Which config the CLR consults

Microsoft's *Architecture of VSTO Add-ins* (learn.microsoft.com/visualstudio/vsto/architecture-of-vsto-add-ins,
"Loading process", steps 7-11) states:

- step 7: "The Visual Studio Tools for Office runtime **creates a new application domain** in which to
  load the VSTO Add-in assembly."
- step 9: the runtime calls `RequestComAddInAutomationService`.
- step 10: the runtime calls `RequestService`, with the note: "**the first call to the `RequestService`
  method usually happens before the call to the `ThisAddIn_Startup` method**, your VSTO Add-in should not
  make any assumptions about when the `RequestService` method will be called, or how many times it will
  be called."
- step 11: the runtime calls `ThisAddIn_Startup`.

Binding redirects are per-AppDomain and are read from that domain's configuration file
(`AppDomainSetup.ConfigurationFile`; "The configuration file describes the search rules and configuration
data for the application domain. The host that creates the application domain is responsible for supplying
this data"). The host that creates the add-in domain is the VSTO runtime, not `OUTLOOK.EXE`, so the
governing file is **not** `OUTLOOK.EXE.config`.

I did not find a Microsoft page that states in so many words "the VSTO runtime sets
`ConfigurationFile` to `<AddInAssembly>.dll.config`". That specific mapping is therefore **likely but not
confirmed from primary documentation**. It is, however, confirmed empirically — see 4.3.

### 4.2 Does this repository produce and deploy such a file?

**Yes, verified.** `TaskMaster/TaskMaster.csproj:26` sets `<OutputType>Library</OutputType>` and line 445
declares `<None Include="app.config" />`. A committed MSBuild log in the repository records the actual
copy:

```
_CopyAppConfigFile:
  Copying file from "<repo-root>\TaskMaster\app.config" to "<repo-root>\TaskMaster\bin\Debug\TaskMaster.dll.config".
```
(`docs/features/active/2026-09-08-utilitiescs-test-hygiene-residuals-817/evidence/qa-gates/nullable-rebuild-console.2026-09-09T12-15.txt:10792`)

The same log shows it being fanned out to dependent test outputs
(`.../nullable-rebuild-console.2026-09-09T12-15.txt:10945`, `:5468`). So the file lands at
`TaskMaster/bin/Debug/TaskMaster.dll.config`.

### 4.3 Would a `<bindingRedirect>` placed there be honoured when Deedle loads?

**Yes — and this is settled by the maintainer's own production repro, not by inference from
documentation alone.**

Argument: `Deedle.dll` references `FSharp.Core 4.5.0.0` **(given)**; the only deployed FSharp.Core is
11.0.0.0 **(given)**; only `TaskMaster/app.config:69-71` can bridge that gap in production, because no
`AssemblyResolve` fallback was installed on the reproduced path (section 3.3) and because the disk holds
no FSharp.Core 4.5.0.0. The reproduced production failure is a `FileNotFoundException` for
**`netstandard, Version=2.1.0.0`** — a version that *only FSharp.Core 11.0.0.0* asks for **(given)**.
Therefore FSharp.Core 11.0.0.0 did bind, therefore the `TaskMaster.dll.config` redirect **was applied**
inside the Outlook add-in AppDomain.

**Verdict: remedy candidate 1 (add a `netstandard` `<dependentAssembly>` to `TaskMaster/app.config`) is
NOT inert. The redirect will be read and applied.**

**But it is not sufficient on its own.** A `<bindingRedirect>` rewrites an identity; it cannot manufacture
an assembly. After `2.1.0.0 -> 2.0.0.0` the CLR must still locate `netstandard 2.0.0.0`. The maintainer's
trace reports that the chain falls back to `2.0.0.0` and **fails there too** **(given)**. If that
observation reflects the default binder genuinely being unable to produce `netstandard 2.0.0.0` inside the
add-in AppDomain, then the redirect alone changes nothing: it converts an unsatisfiable 2.1.0.0 request
into an unsatisfiable 2.0.0.0 request. A config-only fix must therefore be paired with something that can
actually supply the assembly (section 6).

---

## 5. Question 4 — earliest deterministic production installation point

Candidates, earliest first:

| Candidate | Runs when | Guaranteed before a ribbon `onAction`? | Notes |
|---|---|---|---|
| Module initializer on `TaskMaster.dll` | Before any type in the module is used | Yes | `TaskMaster/TaskMaster.csproj:31` sets `<LangVersion>preview</LangVersion>`, so C# 9 syntax is available; but `System.Runtime.CompilerServices.ModuleInitializerAttribute` does not exist in the net481 BCL and would have to be hand-declared. Exotic; conflicts with "simplicity first" in `.claude/rules/general-code-change.md`. |
| **`static ThisAddIn()` type initializer** (added to the hand-authored partial `TaskMaster/ThisAddIn.cs`) | Before the `ThisAddIn` instance is constructed | **Yes** | `ThisAddIn` is a `sealed partial class` (`TaskMaster/ThisAddIn.Designer.cs:18`); the VSTO runtime must construct an instance (`ThisAddIn.Designer.cs:31-34`) before it can call `RequestComAddInAutomationService`, `CreateRibbonExtensibilityObject`, or raise `Startup`. Adding an explicit static constructor also removes `beforefieldinit`, making the ordering precise rather than "at or before first use". Recommended. |
| `RequestComAddInAutomationService()` (`TaskMaster/ThisAddIn.cs:273-278`) | Loading-process step 9 | Yes | Documented to run before `RequestService` and `ThisAddIn_Startup`, but the docs warn against assuming call counts/timing. |
| `CreateRibbonExtensibilityObject()` (`TaskMaster/ThisAddIn.cs:252-256`) | Loading-process step 10 (`RequestService`) | Yes | Assigns `_ribbonController`, which `Application_Startup` consumes at `ThisAddIn.cs:67` — in-repo evidence that it precedes `Application_Startup`. Docs explicitly say not to assume when it is called. |
| `ThisAddIn_Startup` (`TaskMaster/ThisAddIn.cs:21-43`) | Loading-process step 11 | Yes for a user click | Simplest, matches the repo's existing startup conventions. |
| `Application_Startup` (`TaskMaster/ThisAddIn.cs:45-97`) | On the Outlook `Application.Startup` event | Weaker | Subscribed at `ThisAddIn.cs:42`; fires later and is the only place `_globals` exists. Too late to be the guarantee. |

**Can a ribbon callback fire before `ThisAddIn_Startup` completes?** For `onAction` callbacks such as
`QuickFilerHighConfidence_Click`, practically no: Office must first obtain the ribbon XML via
`IRibbonExtensibility.GetCustomUI` and render the UI, and the user must then click. But the documentation
declines to guarantee the ordering of `RequestService` relative to `ThisAddIn_Startup`, and the ribbon's
`getEnabled`/`getImage`/`onLoad` callbacks demonstrably can run during step 10. The `ThisAddIn` static
constructor is the only candidate whose ordering rests on a CLR guarantee rather than on host behaviour,
which is why it is recommended.

Placement of the *code*: the installer type should live in `UtilitiesCS` (host-neutral, already referenced
by `TaskMaster`, and testable), not in `TaskMaster`, whose `ThisAddIn` class carries
`[ExcludeFromCodeCoverage]` (`TaskMaster/ThisAddIn.cs:18`). The `ThisAddIn` static constructor should
contain one call and nothing else. An exception escaping a static constructor becomes a
`TypeInitializationException` that would take the whole add-in down, so the installer must be internally
non-throwing.

---

## 6. Question 5 — is the existing resolver adequate for BOTH versions?

`SVGControl/SvgAssemblyResolver.cs:45-155` resolves in three ordered strategies:

1. **Lines 52-70 — already-loaded match.** Iterate `AppDomain.CurrentDomain.GetAssemblies()`, match on
   case-insensitive simple name (`loadedName.Name` vs `requested.Name`) and on public key token
   (`SvgAssemblyProbe.PublicKeyTokensEqual`). **Version is never compared.** So *if* some `netstandard`
   with token `cc7b13ffcd2ddd51` is already loaded, a request for either 2.1.0.0 or 2.0.0.0 is satisfied
   by returning it. .NET Framework does not re-validate the identity of an assembly returned from an
   `AssemblyResolve` handler; the repository already depends on this (the same handler is what makes
   ExCSS 4.2.3 requests resolve to the deployed 4.3.2, per the comment at lines 17-29).
2. **Lines 84-105 — partial-name load.** `Assembly.Load(new AssemblyName(requested.Name))` — i.e. the
   display name `netstandard` with no version, culture or token. On .NET Framework a *partial* display
   name is not a strong-name reference, and partial names are the documented province of the obsolete
   `Assembly.LoadWithPartialName`; `Assembly.Load` with a partial name is **unlikely to reach the GAC**.
   This is stated as likely, not verified — I could not run a probe in this session. Either way it is a
   materially weaker lookup than a full display name.
3. **Lines 107-138 — file probe.** `Path.Combine(directory, requested.Name + ".dll")` over the
   directories returned by `SvgAssemblyProbe.GetProbeDirectories(...)`. `netstandard.dll` is not deployed
   to any `bin\Debug` **(given)**, so this strategy cannot fire for `netstandard`.

**Answer to the (a)/(b) discrimination.** The maintainer's production repro is, by its own premise, a run
in which no SVG had been rendered. If the handler had been installed it would have run strategy 1, and in
the test host the *identical* logic makes the same bind succeed; the production bind failed. Therefore the
production failure is reading **(a): the default binder failed before any handler ran.** The existing
handler was absent, not defective.

That is the good news for the remedy: installing this class of handler earlier is not a no-op. The bad
news is that it is **not sufficient as written**:

- Strategy 1 only works if something else already loaded a `netstandard` facade. In the vstest testhost
  that is true (which is exactly why the test host is fixed by the handler). In a fresh Outlook add-in
  AppDomain it is **unknown** whether any `netstandard` is loaded at the moment Deedle binds, and the
  maintainer's "`2.0.0.0` also fails" observation is evidence that it is not.
- Strategy 2 probably cannot reach the GAC copy (see above).
- Strategy 3 cannot fire because the file is not deployed.

**Conclusion: promoting `SvgAssemblyResolver.Install()` to a public API and calling it from `ThisAddIn`
is the smallest possible diff and would probably not fix the reproduced defect.** The handler needs a new
capability — an explicit, deterministic source for a `netstandard` facade — before its installation point
matters.

---

## 7. Question 6 — what a working resolver must return

| Source | Deployable? | Assessment |
|---|---|---|
| **The GAC copy, requested by FULL display name**: `Assembly.Load("netstandard, Version=2.0.0.0, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51")` | Yes, zero deployment | The GAC on this machine holds exactly `v4.0_2.0.0.0__cc7b13ffcd2ddd51` **(given)**. A fully-specified strong name is the reference form the GAC is searched for. This is a capability the current handler does not have (it asks by partial name). Recommended as ladder rung 2. |
| **The framework-directory facade**: `Assembly.LoadFrom(Path.Combine(RuntimeEnvironment.GetRuntimeDirectory(), "netstandard.dll"))` | Yes, zero deployment | **Verified present on this machine** at `%WINDIR%\Microsoft.NET\Framework64\v4.0.30319\netstandard.dll` (Glob probe). It ships with the .NET Framework, so any machine capable of running a `net481` VSTO add-in has it. Recommended as ladder rung 3, and it is the rung that removes the dependency on GAC-lookup behaviour entirely. |
| **An already-loaded `netstandard`** | n/a | Cheapest and correct when available, but not deterministic — it is precisely what is missing in the failing production AppDomain. Keep as rung 1, never as the only rung. |
| **A deployed `netstandard.dll` from `NETStandard.Library`** | Yes, with cost | The `net461` facade in `NETStandard.Library` 2.0.3 is a runtime type-forwarding facade and would work. Cost: a new `packages.config` entry in `TaskMaster` (and any other project that must deploy it), a new file in `bin\Debug`, and a new file entry in the VSTO application manifest for ClickOnce deployment. Strictly more moving parts than the framework-directory copy, which is already guaranteed present. Rejected as primary, viable as a fallback rung 4. |
| **`typeof(object).Assembly`** (return `mscorlib`) | n/a | **Reject.** `netstandard.dll` is a pure type-forwarding facade. Substituting `mscorlib` means every `netstandard!T` lookup resolves against `mscorlib` only; types that the real facade forwards to `System.dll`, `System.Core.dll`, `System.Runtime.dll` etc. (for example `System.Linq.Enumerable`) would not be found, converting a clean `FileNotFoundException` into a `TypeLoadException` at an arbitrary later point. Strictly worse than the current failure. |

**Clean-user-machine analysis.** Every project in the solution targets `v4.8.1`
(`TaskMaster/TaskMaster.csproj:30`; `packages.config` entries use `targetFramework="net481"`). A machine
that can load this add-in has .NET Framework 4.8.1, which ships `netstandard.dll` 2.0.0.0 both in the GAC
and in the runtime directory. Neither rung 2 nor rung 3 introduces a new machine prerequisite.
`netstandard 2.1.0.0` does not exist for .NET Framework at all, on any machine; the only possible
satisfying assembly is the 2.0.0.0 facade, returned through a handler that does not version-check.

---

## 8. Question 7 — making the acceptance test falsifiable

### 8.1 The masking problem, stated precisely

Two mechanisms mask this defect, and both must be excluded:

1. `SVGControl.SvgRenderer`'s static constructor installs the fallback the first time any SVG-bearing
   control is constructed (`SVGControl/SvgRenderer.cs:25-28`).
2. **PR #880**: `QuickFiler.Test/SetupAssemblyInitializer.cs:14-20` and
   `UtilitiesCS.Test/TestAssemblyInitializer.cs:14-17` install the same fallback from `[AssemblyInitialize]`.
   **Any test placed in `QuickFiler.Test` or `UtilitiesCS.Test` is masked by construction.**

A test that merely "passes" proves nothing. The acceptance test must be able to *fail on an unfixed build*
and must *demonstrate mechanically* that the no-prior-SVG, no-prior-handler precondition held.

### 8.2 Options, with the mechanically checkable guarantee each affords

**Option D (recommended) — fresh child `AppDomain` harness.**

Create a clean `AppDomain` inside an ordinary MSTest method via `AppDomain.CreateDomain` with an
`AppDomainSetup` whose `ApplicationBase` is the directory of the assembly under test and whose
`ConfigurationFile` is that assembly's `.dll.config`, then drive a `MarshalByRefObject` proxy inside it.

Mechanically checkable guarantees, all assertable *inside* the child domain and reported back across the
boundary:
- `AppDomain.CurrentDomain.GetAssemblies()` contains no assembly whose simple name is `SVGControl`.
  Because `SvgRenderer`'s type initializer cannot have run if `SVGControl.dll` is not loaded, this is a
  complete proof of "no prior SVG rendering".
- The `AssemblyResolve` invocation list is empty before the production installer runs. `AppDomain` exposes
  no public accessor, but the private instance field `_AssemblyResolve` is readable by reflection on
  net481 (a frozen runtime), and `Delegate.GetInvocationList()` on it yields the handler count. Assert
  zero.
- **Negative control (the load-bearing part).** In a *second* child domain, perform the bind **without**
  installing the production fix and assert it throws `FileNotFoundException` for
  `netstandard, Version=2.1.0.0`. This is what makes the suite able to tell a fixed build from an unfixed
  one: it proves the environment is genuinely unsatisfiable, so the positive assertion in the first domain
  is attributable to the fix and not to ambient luck.
- The bind itself should be forced twice, once per version: `Assembly.Load("netstandard, Version=2.1.0.0, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51")`
  and the same for `Version=2.0.0.0`, satisfying the maintainer's "must cover BOTH versions".

Costs and caveats: `AppDomain.CreateDomain` and `MarshalByRefObject` have **no precedent in this
repository** (content search for `CreateDomain|MarshalByRefObject|AppDomainSetup` over all `*.cs` returned
zero matches), so this is a greenfield technique here. The proxy type must be public and serializable-by-reference,
`AppDomain.Unload` must run in `[TestCleanup]`, and the harness must not write any file (the repository
prohibits temporary files in tests — `.claude/rules/general-unit-test.md`, "Creation and use of temporary
files in tests is strictly prohibited"). It writes none: `ApplicationBase` points at an existing build
output directory. This option can live in **any** test assembly, including `QuickFiler.Test`, because the
parent domain's handler does not propagate into a newly created child domain.

**Option A — a dedicated new test assembly with no `[AssemblyInitialize]` and no SVG-touching class.**

Guarantee: weaker than it looks. CI runs *all* `*.Test.dll` in **one** `vstest.console.exe` invocation with
`/InIsolation` (`.github/workflows/_mstest-coverage.yml:86-99`), so every test assembly shares one
`testhost` process. MSTest on .NET Framework creates a child AppDomain per test source when
`DisableAppDomain` is not set, and `TaskMaster.runsettings` does not set it — so per-source isolation
**probably** holds, but that is a defaulted framework behaviour, not an assertion in the test. The
assembly would still need the in-domain `GetAssemblies()` / invocation-list assertions of Option D to be
mechanically checkable, at which point Option D achieves the same guarantee without a new project.
Costs: new `.csproj`, `packages.config`, `app.config`, `TaskMaster.sln` registration and configuration
rows, and automatic pickup by the CI `*.Test.dll` glob.

**Option B — an existing test assembly without a resolver.** Candidates that have no `[AssemblyInitialize]`:
`TaskMaster.Test`, `ToDoModel.Test`, `Tags.Test`, `TaskTree.Test`, `TaskVisualization.Test`,
`VBFunctions.Test`, `SVGControl.Test`. `ToDoModel.Test` already references Deedle; `TaskMaster.Test`
references `TaskMaster`, `ToDoModel` and `UtilitiesCS` (`TaskMaster.Test/TaskMaster.Test.csproj:345-353`)
and so has `SVGControl.dll` in its output. Guarantee: **none by construction** — `TaskMaster.runsettings`
sets `<Scope>ClassLevel</Scope>` with `<Workers>0</Workers>`, so sibling classes in the same assembly run
in nondeterministic order and any one of them that constructs a `MyBox` dialog, `ConfigViewer`,
`FolderNotFoundViewer` or `ProgressMultiStepViewer` masks the result. A precondition assertion would make
the test fail for the wrong reason; `Assert.Inconclusive` would silently stop gating. Not recommended
alone; acceptable only as the host for Option D's child-domain harness.

**Option C — child-process harness.** Launch a fresh process, assert on exit code or stdout. Rejected:
`.claude/rules/general-unit-test.md` states unit tests "must not depend on external services (databases,
networks, remote APIs, **external processes**)", and there is no already-built console executable in this
solution to launch (`TaskMaster` is `OutputType=Library`). Building one solely for the test adds a
production project to the solution and to every coverage denominator.

**MSTest facilities available here.** The test projects are MSTest V2 on `net481`. There is no MSTest
attribute that provides process isolation; AppDomain isolation is controlled only by the runsettings
`DisableAppDomain` switch, which is absent. `/InIsolation` on the `vstest.console.exe` command line
isolates the test host from `vstest.console.exe`, **not** test assemblies from each other. So the
in-repository mechanisms actually available are: runsettings `DisableAppDomain`, and explicit
`AppDomain.CreateDomain` in test code. Option D uses the latter.

### 8.3 Recommended acceptance-criterion shape

1. A child-AppDomain harness test asserting, inside the child domain and in this order:
   (i) no `SVGControl` assembly loaded; (ii) zero `AssemblyResolve` handlers installed;
   (iii) the production installer is invoked; (iv) `Assembly.Load` of the full display names for
   `netstandard, Version=2.1.0.0` **and** `netstandard, Version=2.0.0.0` both succeed;
   (v) a Deedle type (for example the one already exercised by
   `QuickFiler.Test/Controllers/QfcInitEmailQueueZeroBatchTests.cs`, `Deedle.Reflection.convertRecordSequence`)
   initialises without `TypeInitializationException`.
2. A negative-control test in a second child domain asserting that, **without** the installer, step (iv)
   throws `FileNotFoundException` naming `netstandard`. If this test ever starts passing, the harness has
   lost its isolation and the positive test has become vacuous.
3. A static assertion that `TaskMaster/app.config` declares the `netstandard` `<dependentAssembly>`, if
   the config hardening is adopted.
4. A manual live-Outlook gate (human), unchanged from `issue.md:130-132`: fresh Outlook session, click
   the QuickFiler ribbon button without opening any SVG-bearing surface first.

---

## 9. Recommended remedy

**Install an eager, self-sufficient binding fallback in production, from the `ThisAddIn` type initializer,
and harden the config declaratively.**

Concretely:

1. New production type in `UtilitiesCS` (host-neutral, testable, no WinForms, no COM) — working name
   `UtilitiesCS.Bootstrap.AssemblyBindingFallback` — exposing `Install()` and an internal, directly
   testable `Resolve(AssemblyName requested)` seam. Resolution ladder:
   1. already-loaded assembly with matching simple name + public key token (as today);
   2. `Assembly.Load` of the **full** display name with the requested simple name, `Version=2.0.0.0` for
      `netstandard` specifically, `Culture=neutral` and the requested token — the GAC-reachable form;
   3. `Assembly.LoadFrom(Path.Combine(RuntimeEnvironment.GetRuntimeDirectory(), "netstandard.dll"))` for
      the `netstandard` identity — verified present in the framework directory;
   4. directory probe next to the executing assembly (as today).
   Non-throwing at the boundary, `Trace`-based diagnostics only (log4net inside an `AssemblyResolve`
   handler can re-enter assembly loading — the rationale already documented at
   `SVGControl/SvgAssemblyResolver.cs:98-99,140-142`), with the existing `[ThreadStatic]` re-entrance guard.
2. `static ThisAddIn() { UtilitiesCS.Bootstrap.AssemblyBindingFallback.Install(); }` added to
   `TaskMaster/ThisAddIn.cs`. One statement, nothing else.
3. Add to `TaskMaster/app.config` (inside the existing `<assemblyBinding>`):
   `netstandard` / `cc7b13ffcd2ddd51`, `oldVersion="0.0.0.0-2.1.0.0" newVersion="2.0.0.0"`. This is cheap,
   declarative, verified to be honoured in the Outlook host (section 4.3), and it removes the dependency on
   a handler for the *common* case. It is hardening, not the fix.
4. Leave `SVGControl.SvgAssemblyResolver` exactly as it is. It has an independent, documented reason to
   exist (the `devenv.exe` WinForms-designer host, `SVGControl/SvgAssemblyResolver.cs:17-29`) and removing
   or re-pointing it widens the blast radius into issue #418's territory.
5. Leave `TestSupport/TestAssemblyResolver.cs` alone (PR #880, out of scope).

**Rejected alternatives (brief).**
- *Config redirect only.* Honoured, but cannot manufacture an assembly; inert against the reported
  `2.0.0.0` failure. Kept as item 3, not as the fix.
- *Pin or downgrade FSharp.Core below 11.0.0.0.* Removes the 2.1.0.0 requirement at the root, but the
  redirect appears in five `app.config` files, the deployed package graph would have to change, and
  Deedle's own `netstandard 2.0.0.0` reference **(given)** would still be exposed to the reported
  2.0.0.0 failure. Does not satisfy "must cover BOTH versions".
- *Deploy `netstandard.dll` via `NETStandard.Library`.* Works, but adds a package, a deployed file and a
  VSTO application-manifest entry for no capability the framework directory does not already provide.
- *Make `SvgAssemblyResolver.Install()` public and call it from `ThisAddIn`.* Smallest diff, but section 6
  shows the handler as written lacks any deterministic `netstandard` source, so it would probably not fix
  the reproduced failure while creating a production dependency on an SVG rendering component.
- *Return `typeof(object).Assembly`.* Rejected on type-forwarding grounds (section 7).

---

## 10. Question 8 — blast radius (deliberately wide)

Paths a fix along the recommended remedy would touch, or must be checked against:

**Certain to change**
- `UtilitiesCS/` — new source file for the fallback type (one file, under the 500-line limit).
- `UtilitiesCS/UtilitiesCS.csproj` — a new `<Compile Include>` item. This project uses explicit `Compile`
  items, so an added file that is not registered silently does not build.
- `TaskMaster/ThisAddIn.cs` — added `static ThisAddIn()`.
- `TaskMaster/app.config` — one new `<dependentAssembly>` block.
- `docs/features/active/2026-09-13-.../spec.md` — Proposed Fix, Test Strategy and Acceptance Criteria
  sections are currently empty templates.

**Likely to change**
- A test file for the new `UtilitiesCS` type, plus its `<Compile Include>` registration in
  `UtilitiesCS.Test/UtilitiesCS.Test.csproj`. Note that `UtilitiesCS.Test` is itself masked
  (section 8.1), so pure unit tests of the ladder belong there but the *end-to-end* bind assertion does
  not.
- Wherever the child-AppDomain harness lands: a test file plus that project's `<Compile Include>`.

**Must be checked, may not need editing**
- `TaskMaster.sln` — only if Option A (new test project) is chosen instead of Option D. A new project needs
  both a `Project(...)` line and `Debug|Any CPU` / `Release|Any CPU` rows in
  `GlobalSection(ProjectConfigurationPlatforms)`. Existing projects are enumerated at
  `TaskMaster.sln:6-48`.
- `.github/workflows/_mstest-coverage.yml:86-92` — discovers test assemblies by a recursive `*.Test.dll`
  glob filtered to `\bin\<Configuration>\`. A new project named `<X>.Test` is picked up automatically;
  a project **not** named `*.Test` would be silently skipped. Naming matters.
- `.github/workflows/_build-analyzers.yml`, `_build-nullable.yml`, `_format-check.yml` — build the whole
  solution; a new project is covered automatically but must satisfy analyzers, nullable and CSharpier.
  A new file in `UtilitiesCS` carrying `#nullable enable` opts that file into `CS86xx`-as-error under
  `_build-nullable.yml`.
- `coverage.config` and `TaskMaster.runsettings` — both already exclude `.*Deedle.*` and `.*FSharp.*`
  module paths (`coverage.config:14-15`, `TaskMaster.runsettings:16-17`). A new `UtilitiesCS` type is in
  the coverage denominator and must meet the repository floor; the `TaskMaster` half is inside
  `[ExcludeFromCodeCoverage] class ThisAddIn` (`TaskMaster/ThisAddIn.cs:18`), which is why the logic must
  not live there.
- `TaskMaster/TaskMaster.csproj` — no change expected. `app.config` is already an item (line 445) and
  the `_CopyAppConfigFile` target already emits `TaskMaster.dll.config` (verified, section 4.2). **If** a
  `netstandard.dll` were deployed instead (rejected option), this file, `TaskMaster/packages.config` and
  the VSTO application manifest would all need entries.
- `quality-tiers.yml` — **does not exist at the repository root** (verified: read attempt failed). The
  `.claude/rules/quality-tiers.md` rule references it, but there is nothing to update. Flagging rather
  than silently skipping.
- `.csharpierignore` / CSharpier — `TaskMaster/app.config` is already CSharpier-formatted (the existing
  `<assemblyIdentity>` blocks are attribute-per-line); a hand-added block must match or step 1 of the
  toolchain will rewrite it.

**Explicitly not touched**
- `SVGControl/SvgAssemblyResolver.cs`, `SVGControl/SvgRenderer.cs`, `SVGControl/SvgAssemblyProbe.cs`.
- `TestSupport/TestAssemblyResolver.cs`, `QuickFiler.Test/SetupAssemblyInitializer.cs`,
  `UtilitiesCS.Test/TestAssemblyInitializer.cs` (PR #880 territory).
- Any `*.Test/app.config` FSharp.Core redirect.

---

## 11. Testing implications (no test code written)

- **Unit (host-neutral, `UtilitiesCS.Test`):** the resolution ladder's decision logic — token comparison
  including the null/empty-token edge cases already handled at `TestSupport/TestAssemblyResolver.cs:111-129`;
  ladder ordering (already-loaded wins over a fresh load); re-entrance guard returns `null` on the second
  nested request for the same simple name; the handler never throws for an unresolvable name. Inject the
  "load by display name" and "load from path" steps as delegates so no real GAC or filesystem access is
  needed. MSTest + Moq + FluentAssertions per `CLAUDE.md`.
- **Integration (child-AppDomain harness):** section 8.3 items 1 and 2. The negative control is the
  falsifiability gate and must be treated as a first-class acceptance criterion, not a nicety.
- **Static/contract:** assert `TaskMaster/app.config` contains the `netstandard` `<dependentAssembly>`,
  and assert that `typeof(ThisAddIn)` has a declared static constructor (`TypeAttributes.BeforeFieldInit`
  is **not** set) — a cheap regression guard that the eager install point has not been deleted.
  `TaskMaster.Test` already references `TaskMaster` (`TaskMaster.Test/TaskMaster.Test.csproj:345`) and
  already does reflection-shape assertions of this kind (`TaskMaster.Test/Ribbon/RibbonCommandBoundaryTests.cs:187-201`).
- **Determinism:** no `Thread.Sleep`, no wall-clock waits, no temporary files. The child-AppDomain harness
  reads only existing build output.
- **Manual gate:** unchanged from `issue.md:130-132`.

---

## 12. Open questions and known unknowns

1. **Why `netstandard 2.0.0.0` also fails in the Outlook add-in AppDomain is unknown.** The GAC holds it
   **(given)**, and a fully-specified strong-name reference should find it. Section 6 establishes only
   that no handler was running; it does not explain the 2.0.0.0 leg. The recommended remedy is designed to
   be robust to this by adding the framework-directory rung, which bypasses GAC lookup entirely. A Fusion
   log (`HKLM\SOFTWARE\Microsoft\Fusion` `EnableLog=1`, `ForceLog=1`) from one reproduced Outlook run
   would settle it and is the cheapest next measurement.
2. **Whether the VSTO runtime sets the add-in AppDomain's `ConfigurationFile` to `TaskMaster.dll.config`
   specifically** is not confirmed from primary Microsoft documentation. It is confirmed empirically by
   the FSharp.Core argument in section 4.3, which is sufficient for the decision but is inference, not a
   citation.
3. **Whether `Assembly.Load` with a partial display name reaches the GAC on .NET Framework 4.8.1** is
   stated as unlikely but was not measured in this session.
4. **Whether MSTest V2 creates one AppDomain per test source in this configuration** (which Option A leans
   on) was inferred from the absence of `DisableAppDomain` in `TaskMaster.runsettings` and not measured.
   Option D does not depend on it.
5. `QuickFiler/Viewers/Form1.Designer.cs` appears to be a leftover scratch form carrying one `ButtonSVG`;
   whether it is reachable in production was not determined and does not affect the conclusions.

---

## Numeric Derivation Evidence

Complete Family: SVGControl/SvgAssemblyResolver.cs, TestSupport/TestAssemblyResolver.cs

Exhaustive Search Scope: the entire repository source tree at the item branch head, covering every directory and every file extension present, including .cs, .csproj, .config, .xml, .yml and .md, with no path, project or directory carved out

Inclusion Rules: a member qualifies if and only if it is a first-party source file in this repository that subscribes a delegate to the AppDomain assembly-resolution event, that is, it contains a statement attaching a handler to AppDomain.CurrentDomain.AssemblyResolve, thereby installing a process-wide binding fallback for the application domain in which it runs; both production and test files qualify, and both the currently reachable and the currently unreachable installation sites qualify

Exclusion Rules: excluded are files that only mention the event name in a comment, XML documentation block or Markdown prose; files that read or enumerate AppDomain.CurrentDomain without attaching a resolution handler, such as GetAssemblies, BaseDirectory or DefineDynamicAssembly uses; generated coverage and log artifacts under docs/ that contain the compiled handler method name; agent-memory notes under .claude/; third-party assemblies under packages/; and types whose name merely contains the word Resolver but which perform domain resolution rather than assembly resolution, such as ConversationResolver, OutlookFolderHandleResolver, CidImageResolver, FakeFolderHandleResolver and DoNotSerializeContractResolver

Primary Search Strategy: a regular-expression content sweep with the Grep tool across the entire repository source tree, matching the event-subscription syntax AssemblyResolve\s*[+-]= over every .cs file and then opening each hit to confirm the statement attaches rather than merely mentions the handler, which returned exactly SVGControl/SvgAssemblyResolver.cs, TestSupport/TestAssemblyResolver.cs and no other file

Cross-check Search Strategy: an independent two-part enumeration that never mentions the event-subscription syntax at all, combining a Glob filename sweep for the pattern **/*Resolver*.cs across the whole worktree with a separate content sweep for every textual occurrence of the receiver expression AppDomain.CurrentDomain in every .cs file, then classifying each of the resulting files by reading it, which left exactly SVGControl/SvgAssemblyResolver.cs, TestSupport/TestAssemblyResolver.cs as installers while rejecting the eleven Resolver-named files and the twenty-eight BaseDirectory, GetAssemblies and DefineDynamicAssembly uses

Primary Member Set: SVGControl/SvgAssemblyResolver.cs, TestSupport/TestAssemblyResolver.cs

Cross-check Member Set: TestSupport/TestAssemblyResolver.cs, SVGControl/SvgAssemblyResolver.cs

Primary Count: 2

Cross-check Count: 2

Member-set Comparison: after normalising for ordering and letter case the primary member set and the cross-check member set are equal, both being the two-element set consisting of SVGControl/SvgAssemblyResolver.cs and TestSupport/TestAssemblyResolver.cs, so the two independent enumerations match and the asserted count of exactly two first-party AssemblyResolve installation sites in the repository is confirmed
