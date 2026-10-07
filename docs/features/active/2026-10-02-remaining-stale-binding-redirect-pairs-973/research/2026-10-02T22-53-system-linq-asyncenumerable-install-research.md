# Supplemental research: installing `System.Linq.AsyncEnumerable` so its 15 redirects become verifiable (issue #973)

- Date: 2026-10-02
- Branch: `bug/remaining-stale-binding-redirect-pairs-973`
- Supplements: `research/2026-10-02T22-35-stale-binding-redirect-pairs-research.md` (section 6, row 2). That file is not edited.
- Scope: research only; no file outside this research directory was modified; nothing staged or committed. Paths are repository-relative to the item worktree unless marked `<main-checkout-root>` (the primary checkout, read only, which carries a restored `packages/` tree; the item worktree has none).
- Evidence tags: `[V]` verified by Read/Grep/Glob in this session; `[V-web]` verified from api.nuget.org flat-container nuspec, nuget.org, GitHub or learn.microsoft.com in this session; `[I]` inference from verified facts; `[U]` not verified.
- Tooling note: the Bash tool was disabled in this session, so no `git log`, no DLL metadata read and no restore was performed. Assembly-version facts come from tracked text and from the shipped package XML documentation files in `<main-checkout-root>\packages`.

## 1. Decision summary

The install is feasible, but not in the form NuGet writes by default.

1. `System.Linq.Async 7.0.1` ships two different assemblies per TFM: `ref/net48/System.Linq.Async.dll` (what PackageReference consumers compile against) and `lib/net48/System.Linq.Async.dll` (runtime). The reference assembly no longer defines the type `System.Linq.AsyncEnumerable`; the runtime assembly still defines it in full, for binary compatibility (section 5.1).
2. Every HintPath in this repository points at `lib\net48\System.Linq.Async.dll` (section 2.1), so the five installing projects compile against the runtime assembly, in which `System.Linq.AsyncEnumerable` is a public type with 334 documented members including `ToListAsync`, `ToAsyncEnumerable`, `CountAsync`, `ToArrayAsync` and `Select`.
3. Adding the BCL `System.Linq.AsyncEnumerable.dll` as an ordinary (global-alias) `Reference` therefore places two public types named `System.Linq.AsyncEnumerable` in scope. Every extension call to a shared-name operator becomes CS0121 (ambiguous call) and any type-name use becomes CS0433. The repository has 60 such call lines in the four production projects (section 5.3). This is the ambiguity Ix.NET fixed for PackageReference consumers by renaming the type in the reference assembly only (`dotnet/reactive#2291`, PRs #2292/#2293 `[V-web]`); that fix does not reach a `lib/`-compiling packages.config consumer. The "PackageReference consumers already compile with both references" argument in the delegation prompt does not transfer, because those consumers compile against `ref/`.
4. The remedy that satisfies the orchestrator's goal (assembly deployed, csproj `Reference Include="System.Linq.AsyncEnumerable, Version=..."` present so the gate can verify the name, redirects set to the deployed version) without entering compile scope is to give the new `Reference` an `<Aliases>` metadata value. An aliased reference is loaded, resolved, copied local and deployed exactly like a global one, but its types are not visible in the global namespace unless a source file opts in with `extern alias`; no source file does. Ix.NET's own `System.Linq.Async.csproj` references the BCL package the same way (`Aliases="SystemLinqAsyncEnumerable"`) `[V-web]`. Recommended form in section 6.
5. Package version: `System.Linq.AsyncEnumerable 10.0.12` `[V-web]`. No additional packages: all four of its net462 dependencies are already installed at or above the declared lower bound in all five target projects (section 4).
6. Additional write set beyond the first research's 15 files plus `UtilitiesCS/app.config`: five `packages.config`, five `.csproj`, plus the `System.Linq.AsyncEnumerable` redirect line in 15 `app.config` files (14 of which are already in the write set; `UtilitiesCS/app.config` is the 15th and is already in it for the ADAL deletion). Exact list in section 10.

## 2. Current state

### 2.1 Target project set (Q3)

Projects that install `System.Linq.Async 7.0.1` and `System.Interactive.Async 7.0.1` `[V]`:

| Project | packages.config lines (Interactive.Async / Linq.Async) | csproj Reference lines (Interactive.Async / Linq.Async) | HintPath folder |
|---|---|---|---|
| UtilitiesCS | `UtilitiesCS/packages.config:88` / `:97` | `UtilitiesCS/UtilitiesCS.csproj:358-360` / `:398-400` | `..\packages\System.Interactive.Async.7.0.1\lib\net48\`, `..\packages\System.Linq.Async.7.0.1\lib\net48\` |
| QuickFiler | `QuickFiler/packages.config:40` / `:47` | `QuickFiler/QuickFiler.csproj:149-151` / `:183-185` | same |
| ToDoModel | `ToDoModel/packages.config:19` / `:20` | `ToDoModel/ToDoModel.csproj:82-84` / `:85-87` | same |
| TaskMaster | `TaskMaster/packages.config:36` / `:43` | `TaskMaster/TaskMaster.csproj:210-212` / `:244-246` | same |
| UtilitiesCS.Test | `UtilitiesCS.Test/packages.config:87` / `:91` | `UtilitiesCS.Test/UtilitiesCS.Test.csproj:881-883` / `:896-898` | same |

Every `Reference Include` is `System.Linq.Async, Version=7.0.0.0, Culture=neutral, PublicKeyToken=94bc3704cddfc263, processorArchitecture=MSIL` (and the same shape for `System.Interactive.Async`). No other csproj or packages.config names either package, and none names `System.Linq.AsyncEnumerable` `[V]` (Grep over `*/*.csproj` and `*/packages.config`).

Projects that receive `System.Linq.Async.dll` only by copy from a `ProjectReference` (and would receive `System.Linq.AsyncEnumerable.dll` the same way once it sits in the referenced project's output directory): Tags (`Tags/Tags.csproj:85` -> UtilitiesCS), TaskTree (`:78,82` -> ToDoModel, UtilitiesCS), TaskVisualization (`:134,138`), Tags.Test (`:298`), TaskTree.Test (`:295,299`), TaskVisualization.Test (`:319,323`), ToDoModel.Test (`:331,335`), TaskMaster.Test (`:375,379,383` -> TaskMaster, ToDoModel, UtilitiesCS), QuickFiler.Test (`:511,515` -> QuickFiler, UtilitiesCS), VBFunctions.Test (`:272` -> UtilitiesCS) `[V]`. SVGControl has no ProjectReference and SVGControl.Test references only SVGControl (`SVGControl.Test/SVGControl.Test.csproj:69`) `[V]`; neither ever loads `System.Linq.Async`.

Repository convention for transitive packages `[V]`: packages.config has no transitive concept, so NuGet writes the full dependency closure into the installing project's own manifest. The repository follows that: `System.Interactive.Async` (a dependency of `System.Linq.Async`) is installed in exactly the same five projects; `Microsoft.Bcl.AsyncInterfaces`, `System.Threading.Tasks.Extensions` and `System.ValueTuple` are installed in every project that installs anything depending on them (section 4). Projects that only copy a DLL from a ProjectReference do not install the package (Tags, TaskTree, TaskVisualization and the nine test projects do not install `System.Linq.Async`). The minimal consistent set is therefore the same five projects, and no more: a sixth install (for example in Tags.Test) would be the first instance of installing a package in a project that does not compile against it.

### 2.2 How the five projects compile against System.Linq.Async today `[V]`

`<main-checkout-root>\packages\System.Linq.Async.7.0.1\` contains `lib\{net48,netstandard2.0,netstandard2.1,net10.0}\System.Linq.Async.{dll,xml}` and `ref\{net48,netstandard2.0,netstandard2.1,net10.0}\System.Linq.Async.{dll,xml}` (Glob). `System.Interactive.Async.7.0.1` has the same lib/ref split. The HintPaths in 2.1 select `lib\net48`. `packages.config` projects cannot select `ref/` through NuGet; `ref/` is a PackageReference compile-asset concept.

The solution-wide RAR log at `docs/features/active/2026-09-08-etl-deadline-mechanics-follow-ups-825/evidence/other/ac8-createcancellationtokensource-proof.txt:7565-7568,7637-7638` records that both `lib\net48\System.Interactive.Async.dll` and `lib\net48\System.Linq.Async.dll` carry an assembly reference to `System.Linq.AsyncEnumerable, Version=10.0.0.6, PublicKeyToken=b03f5f7f11d50a3a`, that AutoUnify raised the request to `10.0.0.7` (the app.config redirect value), and that nothing on disk satisfied it. The build still succeeds because csc never needs that assembly: every operator the repository calls is defined inside `lib\net48\System.Linq.Async.dll` itself (section 5.1).

### 2.3 Warning configuration relevant to CS0618 / CS0121 `[V]`

- No csproj carries a `<NoWarn>`, `<TreatWarningsAsErrors>`, `<WarningsAsErrors>` or `<WarningsNotAsErrors>` element (Grep for the element forms over `*/*.csproj`: no match; the only hits for the words are the issue #181 comment lines, for example `UtilitiesCS/UtilitiesCS.csproj:1321`). `LangVersion` is `12.0` in UtilitiesCS (`:10`), `preview` in QuickFiler (`:14`) and TaskMaster (`:31`), `latest`/`Latest` elsewhere.
- `.editorconfig` sets no `dotnet_diagnostic.CS0618`, `CS0121` or `CS0433` severity (Grep: no match).
- `Directory.Build.props` exists at the repository root and sets only `RxUseUnsupportedPackagesConfig=true` (`Directory.Build.props:15-17`); `Directory.Build.targets` exists and only toggles VSTO signing for the TaskMaster project (`Directory.Build.targets:10-29`). Neither touches warnings. (The first research file's section 2.1 and CLAUDE.md state that no `Directory.Build.props` exists; that statement is stale.)
- `Microsoft.CSharp.CurrentVersion.targets:200` (`C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\`) adds `1701;1702` to `NoWarn` by default, so the version-unification warnings CS1701/CS1702 that an aliased higher-version reference could raise are suppressed before `TreatWarningsAsErrors` sees them.
- The "~14 unsuppressed CS0618" note in `.claude/agent-memory/atomic-executor/project_nullable_epic_pragma_gate_and_analyzer_restore.md:13` (dated 2026-07-19) is stale: every current call to an obsolete `System.Linq.Async` operator is wrapped in a narrow `#pragma warning disable CS0618` / `restore` pair with a rationale comment (18 sites, for example `UtilitiesCS/Extensions/IAsyncEnumerableExtensions.cs:245-263`, `UtilitiesCS/EmailIntelligence/ClassifierGroups/Triage/Triage.cs:316-429`, `QuickFiler/Controllers/QfcQueue.Enqueue.cs:166-196`, `TaskMaster/AppGlobals/AppEvents.cs:269-319`, `ToDoModel/Data Model/ToDo/ToDoEvents.Filtering.cs:85-98`), and the most recent committed solution-wide gate logs show `0 Warning(s) 0 Error(s)` for both the analyzer Rebuild (`.../825/evidence/qa-gates/qc-build-analyzers.txt:70223-70224`) and the TreatWarningsAsErrors Rebuild (`.../825/evidence/qa-gates/qc-build-nullable.txt:70648-70649`).

## 3. Package version selection (Q1)

- `System.Linq.AsyncEnumerable` versions published on nuget.org (`https://api.nuget.org/v3-flatcontainer/system.linq.asyncenumerable/index.json`) `[V-web]`: `10.0.0` through `10.0.12` stable, plus `10.0.0-preview.*`/`rc.*` and `11.0.0-preview.1` through `11.0.0-rc.1`. `10.0.12` exists and is the newest stable; nuget.org dates it 2026-09-08.
- Lower bounds `[V-web]`: `System.Linq.Async 7.0.1` nuspec, group `.NETFramework4.8`: `System.Interactive.Async >= 7.0.1`, `System.Linq.AsyncEnumerable >= 10.0.6`. `System.Interactive.Async 7.0.1` nuspec, group `.NETFramework4.8`: `System.Linq.AsyncEnumerable >= 10.0.6`. `10.0.12 >= 10.0.6`: satisfied.
- Selected: `10.0.12`. It is the version the repository's other .NET 10 servicing packages are on (`Microsoft.Bcl.Memory 10.0.12` at `UtilitiesCS/packages.config:21`, `Microsoft.Bcl.AsyncInterfaces 10.0.12` at `:18`), it is the exact version its own dependency group demands of those two packages (section 4), and the Dependabot catch-all group (`.github/dependabot.yml:16-19`) would propose the bump to it on the next weekly run if an older version were chosen.
- nuspec of `10.0.12` (`https://api.nuget.org/v3-flatcontainer/system.linq.asyncenumerable/10.0.12/system.linq.asyncenumerable.nuspec`) `[V-web]`: dependency groups `.NETFramework4.6.2` (Microsoft.Bcl.AsyncInterfaces 10.0.12, Microsoft.Bcl.Memory 10.0.12, System.Threading.Tasks.Extensions 4.6.3, System.ValueTuple 4.6.2), `.NETStandard2.0` (same minus System.ValueTuple), `net8.0`/`net9.0`/`net10.0` (none). No frameworkReferences. nuget.org lists the supported frameworks as `net462, netstandard2.0, net8.0, net9.0, net10.0` `[V-web]`.
- Lib folder for the net481 projects: `lib\net462`. NuGet selects the nearest framework at or below `net481`, which is `net462` (the only .NET Framework folder); the repository's own compatibility selector agrees (`scripts/dependencies/PackageCompatibility.psm1:42-51`, preference list ends `... net47, net462, ...`). HintPath: `..\packages\System.Linq.AsyncEnumerable.10.0.12\lib\net462\System.Linq.AsyncEnumerable.dll`.
- Assembly identity: simple name `System.Linq.AsyncEnumerable`, `PublicKeyToken=b03f5f7f11d50a3a` (from the 15 existing `assemblyIdentity` elements and the RAR log) `[V]`. Assembly version of `10.0.12`: `10.0.0.12` `[I]`. Corroboration: (a) the same assembly's `10.0.6` package carries assembly version `10.0.0.6` (the RAR log shows `System.Linq.Async 7.0.1`, whose PackageReference pins `System.Linq.AsyncEnumerable 10.0.6` `[V-web]`, referencing `Version=10.0.0.6` `[V]`); (b) the redirect NuGet wrote at the time reads `10.0.0.7` for what was evidently a `10.0.7` install `[V]`; (c) the sibling .NET 10 servicing packages installed here map `10.0.12 -> 10.0.0.12` (`Microsoft.Bcl.Memory`, `Microsoft.Bcl.AsyncInterfaces`, `Microsoft.Bcl.Numerics`, `Microsoft.Extensions.Diagnostics.Abstractions`; csproj lines in the first research 2.2 and `UtilitiesCS/UtilitiesCS.csproj:84`). The dotnet/runtime `v10.0.12` source csproj for the library declares no explicit `AssemblyVersion`/`ServicingVersion` `[V-web]`, so the value comes from the shared servicing build props and cannot be read from one file. fuget.org, which would have displayed the assembly version, now redirects to an unrelated domain `[V-web]`. The executor must read the restored DLL's `AssemblyName.Version` after restore and use that value in the csproj `Include` and the 15 redirects; the plan must not hard-code `10.0.0.12` without that step (section 8, step 3).

## 4. Transitive dependencies (Q2)

For the resolved group `.NETFramework4.6.2` of `System.Linq.AsyncEnumerable 10.0.12` `[V-web]`, per target project `[V]`:

| Dependency (lower bound) | UtilitiesCS | QuickFiler | ToDoModel | TaskMaster | UtilitiesCS.Test | Additional write set? |
|---|---|---|---|---|---|---|
| Microsoft.Bcl.AsyncInterfaces (>= 10.0.12) | 10.0.12 (`packages.config:18`; csproj `:84` Version=10.0.0.12) | 10.0.12 (`:12`; csproj `:60`) | 10.0.12 (`:9`; csproj `:50`) | 10.0.12 (`:9`; csproj `:142`) | 10.0.12 (`:14`; csproj `:611`) | None |
| Microsoft.Bcl.Memory (>= 10.0.12) | 10.0.12 (`:21`; csproj `:93`) | 10.0.12 (`:13`; csproj `:63`) | 10.0.12 (`:10`; csproj `:53`) | 10.0.12 (`:10`; csproj `:145`) | 10.0.12 (`:17`; csproj `:620`) | None |
| System.Threading.Tasks.Extensions (>= 4.6.3) | 4.6.3 (`:140`; csproj `:519` Version=4.2.4.0) | 4.6.3 (`:77`; csproj `:267`) | 4.6.3 (`:25`; csproj `:101`) | 4.6.3 (`:73`; csproj `:328`) | 4.6.3 (`:107`; csproj `:950`) | None |
| System.ValueTuple (>= 4.6.2) | 4.6.2 (`:142`; csproj `:522` framework reference, no HintPath) | 4.6.2 (`:79`; csproj `:270`) | 4.6.2 (`:26`; csproj `:104`) | 4.6.2 (`:75`; csproj `:331`) | 4.6.2 (`:108`; no csproj Reference element) | None |

Every dependency is installed in every target project at exactly the declared lower bound, so NuGet's packages.config dependency resolution installs nothing else, and no csproj `Reference` or app.config redirect for a dependency changes. The existing redirects already name the installed versions in the five configs: `Microsoft.Bcl.AsyncInterfaces` `10.0.0.12` (for example `UtilitiesCS/app.config:43-44`), `Microsoft.Bcl.Memory` `10.0.0.12` in the five installing configs (`UtilitiesCS:171-172`, `QuickFiler:166-167`, `TaskMaster:178-179`, `ToDoModel:171-172`, `UtilitiesCS.Test:174-175`), `System.Threading.Tasks.Extensions` `4.2.4.0` everywhere `[V]`. The ten stale `Microsoft.Bcl.Memory 10.0.0.7` redirects in non-installing configs are already in the first research's sweep (pair 2).

## 5. Compile impact (Q4)

### 5.1 What the two System.Linq.Async 7.0.1 assemblies declare `[V]`

Read from the shipped XML documentation files in `<main-checkout-root>\packages\System.Linq.Async.7.0.1\`:

| File | `M:System.Linq.AsyncEnumerable.*` members | `M:System.Linq.AsyncEnumerableDeprecated.*` members | Type element |
|---|---|---|---|
| `lib\net48\System.Linq.Async.xml` | 334 | 0 | `T:System.Linq.AsyncEnumerable` (`:7`) |
| `ref\net48\System.Linq.Async.xml` | 0 | 173 | none for `System.Linq.AsyncEnumerable` |

The `lib\net48` document lists, among others, `AsyncEnumerable.CountAsync` (`:601,613`), `ForEachAsync` (`:796,808`), `Select` (`:1885,1896`), `ToArrayAsync` (`:2477`), `ToAsyncEnumerable` (`:2488,2497,2506`), `ToListAsync` (`:2663`), `SelectAwait` (`:3649,3660`). The `ref\net48` document lists `AsyncEnumerableDeprecated.ForEachAsync` (`:315,327`) and `SelectAwait` (`:1897,1908`) but no `ToListAsync`, `ToAsyncEnumerable`, `CountAsync`, `ToArrayAsync` or `Select` at all: PackageReference consumers obtain those from the BCL package, which the nuspec makes a dependency for exactly that reason.

Upstream design record `[V-web]`: `dotnet/reactive#2291` ("System.Linq.Async v7 causes ambiguity errors for non-extension methods") quotes `error CS0121: The call is ambiguous between the following methods or properties: 'AsyncEnumerable.Select<TSource, TResult>(...)' and 'AsyncEnumerable.Select<TSource, TResult>(...)'` and attributes it to System.Linq.Async "continu[ing] to define a public AsyncEnumerable type in its reference assemblies". PR #2292 removed `AsyncEnumerable` from the public API "because having two AsyncEnumerable classes ... caused compiler ambiguity errors", moving the deprecated extension methods to `AsyncEnumerableDeprecated` "in reference assemblies while retaining the original name in runtime assemblies for binary compatibility". PR #2293 generates the `AsyncEnumerableDeprecated` forwarding facade "in the runtime assembly only" and states that "for binary compatibility with code that has not been compiled against Ix.NET's System.Linq.Async v7, the runtime assembly must continue to provide the whole historical AsyncEnumerable API in a class called AsyncEnumerable". `Ix.NET/Source/System.Linq.Async/EnableDeprecationFacadeInRuntimeAssembly.cs` consists of `[assembly: System.Linq.DuplicateAsyncEnumerableAsAsyncEnumerableDeprecated]`. The package README (`<main-checkout-root>\packages\System.Linq.Async.7.0.1\readme.md:3`) says to stop using the package and use `System.Linq.AsyncEnumerable` instead.

`System.Interactive.Async 7.0.1` is the same shape (lib and ref `AsyncEnumerableEx` documents both list 6 `Distinct`/`MinBy`/`MaxBy` members; PR #2280 handled its clashes by obsolescence, renaming and `!REFERENCE_ASSEMBLY` exclusion `[V-web]`). No repository source calls those members (the operator Grep in 5.3 matched none of them), so it is not a factor either way.

### 5.2 Consequence for a global-alias Reference

With `lib\net48\System.Linq.Async.dll` (defines public `System.Linq.AsyncEnumerable`, 334 members) and `lib\net462\System.Linq.AsyncEnumerable.dll` (defines public `System.Linq.AsyncEnumerable`, the BCL implementation) both referenced into the global namespace, the compiler's merged `System.Linq` namespace contains two types with that metadata name. Extension-method lookup collects candidates from both; for every operator that both define with the same parameter list (`ToListAsync`, `ToAsyncEnumerable`, `ToArrayAsync`, `CountAsync`, `Select`, `Where`, ...) overload resolution finds two equally good candidates and reports CS0121. Any use of the type name itself reports CS0433. This is the ambiguity class the upstream issue documents, and the repository would hit it at build time in all four production projects (5.3). It is an error, not a warning: no `NoWarn` or `WarningsNotAsErrors` setting changes the outcome. Overload resolution at the Ix-only names (`SelectAwait`, `WhereAwait`, `SelectAwaitWithCancellation`, `ForEachAsync`, `ForEachAwaitAsync`, `ForEachAwaitWithCancellationAsync`) would be unaffected, because the BCL class does not define them `[I]` from the ref document and the BCL's published surface; that does not rescue the build.

### 5.3 Call sites that would be affected `[V]`

Grep over `{UtilitiesCS,QuickFiler,ToDoModel,TaskMaster}/**/*.cs` for the operator names (pattern listed in the Numeric Derivation Evidence section) returns 103 occurrences in 36 files across the five installing projects' sources plus their tests; restricted to the four production projects, the shared-name operators appear on these lines:

- `ToAsyncEnumerable` (35 lines): `TaskMaster/Ribbon/RibbonController.Intelligence.cs:438`; `TaskMaster/AppGlobals/AppEvents.cs:277,317`; `TaskMaster/AppGlobals/AppItemEngines.cs:74`; `QuickFiler/Helper Classes/ConversationResolver.cs:187`; `QuickFiler/Controllers/QfcCollectionController.cs:495,2144,2148`; `QuickFiler/Controllers/QfcQueue.Enqueue.cs:174`; `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs:297,302`; `QuickFiler/Controllers/QfcDatamodel.cs:439`; `QuickFiler/Controllers/QfcItemController.FocusAndTheme.cs:225,231,241`; `UtilitiesCS/EmailIntelligence/IntelligenceConfig.cs:104`; `UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.cs:158`; `.../SortEmail.AttachmentSaving.cs:81`; `.../EmailFiler.cs:440`; `.../EmailDataMiner.FolderExtraction.cs:327`; `UtilitiesCS/EmailIntelligence/ClassifierGroups/Triage/Triage_OlLogic.cs:211,252`; `.../Triage/Triage.cs:325,355,417`; `.../ManagerAsyncLazy.cs:79`; `UtilitiesCS/OutlookObjects/Table/OlTableExtensions.Etl.cs:156,185`; `UtilitiesCS/EmailIntelligence/Bayesian/Performance/BayesianSerializationHelper.cs:321`; `.../Bayesian/BayesianClassifierGroup.cs:289`; `ToDoModel/Data Model/Tree/TreeOfToDoItems.cs:228,232,233,236`; `ToDoModel/Data Model/ToDo/ToDoItem.cs:1247`; `ToDoModel/Data Model/ToDo/ToDoEvents.Filtering.cs:72,76,77`; `ToDoModel/Data Model/ID/IDList.cs:190,197`.
- `ToListAsync` (5): `ConversationResolver.cs:202`; `QfcQueue.Enqueue.cs:195`; `QfcItemController.ViewerSetup.cs:299,304`; plus test files.
- `ToArrayAsync` (6): `AppEvents.cs:281`; `UtilitiesCS/Extensions/DfDeedle.FrameUtilities.cs:117`; `BayesianClassifierGroup.cs:263,273`; `ToDoItem.cs:1224,1257`.
- `CountAsync` (8): `QfcItemController.Conversation.cs:104,134,194`; `Triage.cs:390`; `SpamBayes.Classify.cs:76`; `BayesianClassifierGroup.cs:262`; `BayesianClassifierExtensions.cs:60`.
- `Select` on `IAsyncEnumerable<T>`: `UtilitiesCS/Extensions/IAsyncEnumerableExtensions.cs:47` (`enumerable.Select(x => ...)`).
- Ix-only names (unaffected by the duplicate type): `SelectAwait` 20 lines, `WhereAwait` 3, `ForEachAsync` 12 (including `IAsyncEnumerableExtensions.cs:252`), all within the `#pragma warning disable CS0618` regions listed in 2.3.

Test projects: `UtilitiesCS.Test/Extensions/IAsyncEnumerableExtensions_Tests.cs:23,39,190` and `UtilitiesCS.Test/OutlookObjects/Table/OlTableExtensions_Tests.cs:1050,1057,1104` call `ToListAsync`/`ToAsyncEnumerable` (UtilitiesCS.Test installs the package). The two other test hits (`TaskMaster.Test/AppGlobals/EngineInitTimingProbeTests.cs:132`, `QuickFiler.Test/Controllers/QfcItemController.SeamDispatcherTests.cs:65,69`) are a comment and a method named `RenderConversationCountAsync`, not operator calls; neither project installs `System.Linq.Async`, consistent with 2.1.

No source file in the repository uses `extern alias` (Grep over `*/**/*.cs`: no match) and no production file invokes `AsyncEnumerable.` by type name (the Grep alternation that included `AsyncEnumerable\.` produced no such match) `[V]`.

### 5.4 Behaviour under the recommended aliased Reference

With `<Aliases>SystemLinqAsyncEnumerable</Aliases>` on the new Reference, csc receives `/reference:SystemLinqAsyncEnumerable=<path>`; the assembly's types are reachable only through `extern alias SystemLinqAsyncEnumerable;`, which no file declares. Name lookup, extension-method candidate sets and overload resolution at every call site in 5.3 are byte-for-byte what they are today, because the only visible `System.Linq.AsyncEnumerable` type remains the one in `lib\net48\System.Linq.Async.dll`. No new CS0618 is introduced (the obsolete attributes live on the Ix type, which is unchanged). CS1701/CS1702 cannot surface (default `NoWarn`, 2.3). MSBuild `Aliases` metadata on `Reference` is documented for exactly this purpose ("Use with extern alias in C# to handle naming conflicts") `[V-web]`, and RAR treats the item like any other HintPath reference: resolved, `Private`/copy-local by default for a non-framework path, copied to the output directory, and found by dependents' RAR passes through the referenced project's output directory (the search path the RAR log shows as `bin\Debug\` at `:7633`) `[I]`.

## 6. Candidate approaches and recommendation

1. **Plain NuGet-form install (global Reference).** `packages.config` entry + `<Reference Include="System.Linq.AsyncEnumerable, Version=...">` with HintPath, as `Install-Package` would write. Rejected: duplicate public type `System.Linq.AsyncEnumerable` at compile time (5.1-5.3); the build fails with CS0121 at the shared-name call sites in all four production projects. No warning setting removes it.
2. **Aliased Reference (recommended).** Same `packages.config` entry and the same Reference element, plus `<Aliases>SystemLinqAsyncEnumerable</Aliases>`, and an XML comment stating why. Deploys the DLL to the five output directories and transitively to the ten dependents, gives the gate a `Reference Include="System.Linq.AsyncEnumerable, Version=<v>"` to verify against, makes the 15 redirects meaningful (System.Linq.Async's `10.0.0.6` request now redirects to a file that exists), and changes no compile result. Matches Ix.NET's own reference form `[V-web]`. The repository's dependency tooling parses `Reference`/`HintPath` lines individually and ignores other child lines (`scripts/dependencies/PackageGraph.psm1:240-283`), so `Find-OrphanedHintPath`, `Test-ReferenceCompleteness` (`scripts/dependencies/ConsistencyVerifier.psm1:156-213`, which only needs a `Reference` whose first Include segment equals the asset file stem and a matching `HintPath`) and `Find-StaleBindingRedirect` all read the aliased element exactly as a plain one `[V]`.
3. **Compile against `ref\net48\System.Linq.Async.dll` instead.** Would make the BCL reference global-safe, but a packages.config HintPath is also the copy-local source; copying a reference assembly to `bin` breaks the runtime, and splitting compile and copy paths requires non-NuGet csproj constructs the repair tooling does not model. Rejected.
4. **Migrate call sites to the BCL and drop `System.Linq.Async`.** The direction the package README prescribes, but it is a behaviour-bearing rewrite across 36 files (SelectAwait/WhereAwait/ForEachAsync have no BCL equivalent and need `Select` with `Func<T, CancellationToken, ValueTask<R>>` or `await foreach`), far outside a redirect-hygiene item. Rejected here; promote separately if wanted.

## 7. Redirect values after install (Q5)

All 15 configs that carry the `System.Linq.AsyncEnumerable` block today read `oldVersion="0.0.0.0-10.0.0.7" newVersion="10.0.0.7"` and must become `oldVersion="0.0.0.0-<v>" newVersion="<v>"` where `<v>` is the assembly version the executor reads from the restored DLL (expected `10.0.0.12`, section 3). Current line numbers (assemblyIdentity line - bindingRedirect line) `[V]`:

| Config | Lines today | Note |
|---|---|---|
| `Tags/app.config` | 210-211 | shifts by -4 after the ADAL block (62-65) is deleted |
| `TaskTree/app.config` | 210-211 | shifts by -4 (ADAL 62-65) |
| `TaskVisualization/app.config` | 210-211 | shifts by -4 (ADAL 62-65) |
| `QuickFiler/app.config` | 210-211 | shifts by -4 (ADAL 66-69) |
| `TaskMaster/app.config` | 222-223 | shifts by -4 (ADAL 74-77) |
| `ToDoModel/app.config` | 215-216 | shifts by -4 (ADAL 67-70) |
| `UtilitiesCS/app.config` | 215-216 | shifts by -4 (ADAL 66-69) |
| `VBFunctions.Test/app.config` | 246-247 | shifts by -4 (ADAL 50-53) |
| `Tags.Test/app.config` | 238-239 | no ADAL block; no shift |
| `TaskTree.Test/app.config` | 238-239 | no ADAL block; no shift |
| `QuickFiler.Test/app.config` | 250-251 | shifts by -4 (ADAL 62-65) |
| `TaskMaster.Test/app.config` | 246-247 | shifts by -4 (ADAL 70-73) |
| `TaskVisualization.Test/app.config` | 250-251 | shifts by -4 (ADAL 62-65) |
| `ToDoModel.Test/app.config` | 250-251 | shifts by -4 (ADAL 62-65) |
| `UtilitiesCS.Test/app.config` | 246-247 | shifts by -4 (ADAL 62-65) |

ADAL block positions are from the first research, section 6, row 1 (assemblyIdentity line and the three following lines). The plan must anchor the redirect edit on the `assemblyIdentity name="System.Linq.AsyncEnumerable"` text, not on line numbers.

Configs with no `System.Linq.AsyncEnumerable` block: `SVGControl/app.config`, `SVGControl.Test/app.config`. Neither project references any of the five installing projects (2.1), so neither process ever loads `System.Linq.Async` or the BCL assembly; no block is needed. The ten non-installing configs that already carry the block keep it (updated), because their hosts do load `System.Linq.Async.dll` copied from a ProjectReference, and that DLL requests `10.0.0.6`.

Gate effect: after the change `System.Linq.AsyncEnumerable` leaves `$expectedUnverifiable` (`tests/scripts/dependencies/BindingRedirectVerification.Tests.ps1:310`), which becomes `@('netstandard')` once the ADAL deletion also lands. If the redirects were left at `10.0.0.7` after the install, the gate would report 15 new findings (`newVersion` not in the csproj set), so the redirect edit and the csproj edit must land in the same commit.

## 8. Restore and toolchain (Q6)

- Restore route `[V]`: `scripts/vscode/Invoke-Restore.ps1:103-117` runs `msbuild TaskMaster.sln /t:Restore /p:Configuration=Debug "/p:Platform=Any CPU" /p:RestorePackagesConfig=true /m`, wired to the VS Code tasks `dotnet: restore` and `restore: TaskMaster.sln` (`.vscode/tasks.json:5,13,68,76`). CI runs `nuget restore $env:SOLUTION_PATH` with NuGet CLI pinned to `7.9.0` (`.github/workflows/_build-analyzers.yml:50`, `_build-nullable.yml:50`, `_mstest-coverage.yml:66`, `dependabot-repair.yml:66-74`). Both routes read `packages.config` and download into `packages\`.
- `packages/` is gitignored (`.gitignore:197` `**/[Pp]ackages/*`, with `!**/[Pp]ackages/build/` at `:199`; the new package ships no `build/` folder, 3) `[V]`. The plan writes only tracked text: `packages.config`, `.csproj`, `app.config`, the Pester test literal.
- No NuGet client edits a csproj outside Visual Studio, so the executor writes the five `packages.config` and five `.csproj` edits by hand, then restores. Suggested order: (1) edit `packages.config` x5; (2) run the restore route; (3) read `[System.Reflection.AssemblyName]::GetAssemblyName('<abs>\packages\System.Linq.AsyncEnumerable.10.0.12\lib\net462\System.Linq.AsyncEnumerable.dll').Version` via one `pwsh -NoProfile -Command` and record it as evidence; (4) edit the five csproj with that version; (5) edit the 15 redirects with that version; (6) run the C# toolchain.
- Toolchain gates triggered (the change enters `*.csproj` scope, so the C# code-change policy applies in full): `dotnet tool run csharpier check .` (a no-op for this write set: `.csharpierignore:12,16,18` exclude `*.csproj`, `**/packages.config`, `**/app.config`, but the gate is still run and recorded); `msbuild TaskMaster.sln /t:Rebuild ... /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`; `msbuild TaskMaster.sln /t:Rebuild ... /p:TreatWarningsAsErrors=true`; the `test: MSTest with Coverage (Koverage)` route. The Pester gate `tests/scripts/dependencies` runs for the test-literal change as in the first research, section 8.
- TreatWarningsAsErrors risk: none identified for the aliased form (5.4). For the global form the failure is an error regardless of the gate. MSB3277 (version conflicts) is not expected: each installing project's own app.config carries a covering redirect, which RAR reads through `AppConfigFile`, and the primary reference is the highest version in the graph `[I]`.
- `dependabot-repair.yml:136` stages `*.csproj`, `*/packages.config` and `*/app.config` on bot branches; an open bot branch touching the same files would conflict, as the first research noted.

## 9. Risks (Q7)

1. **Blocker for the plain form (concrete, verified).** Section 5. Do not plan a global Reference.
2. **Durability of `<Aliases>` under future package updates.** `nuget.exe update` / Dependabot's packages.config path uninstalls and re-adds the `Reference` element, which would drop the child `<Aliases>` `[I]`; the next build then fails loudly with CS0121 at the 5.3 sites. The in-repo repair pass does not strip it: `ProjectConsistency` reconciles `Reference` and `HintPath` text per line and never rewrites elements it does not model (`PackageGraph.psm1:240-283`; `ProjectConsistency.psm1:47`) `[V]`. Mitigations: an XML comment beside each aliased Reference stating the reason; a repository-level Pester assertion (section 11) that fails if any csproj carries a `System.Linq.AsyncEnumerable` Reference without `<Aliases>`.
3. **Redirect-down (#418 class).** None. The only requesters of `System.Linq.AsyncEnumerable` are `System.Linq.Async 7.0.1` and `System.Interactive.Async 7.0.1`, both compiled against `10.0.0.6` (`[V]` RAR log) with nuspec floor `10.0.6` `[V-web]`; `10.0.0.12` is above both and is the only version on disk after install. The four dependencies are already at the exact versions `10.0.12` demands (section 4), so no cascade.
4. **Runtime behaviour.** Today any `lib\net48\System.Linq.Async.dll` code path that reaches into the BCL assembly fails with `FileNotFoundException` (the repository's tests pass without the DLL, so the operators they exercise, `ToAsyncEnumerable`/`ToListAsync`/`Zip`, do not reach it; which members do is `[U]`). After install those paths bind. No call site's compiled target changes (5.4), so no behaviour at an existing site can change except from failure to success.
5. **Major-version drift.** `System.Linq.AsyncEnumerable 11.0.0` is at `rc.1` on nuget.org `[V-web]`. The Dependabot semver-major ignore list (`.github/dependabot.yml:25-40`) covers `Microsoft.Bcl.*` and `Microsoft.Extensions.*` but not this id, so the catch-all group will propose `11.0.0` at GA; `DependabotConfig.Tests.ps1:247-263` compares the ignore set element by element, so adding the id to the ignore list is a two-file change (`.github/dependabot.yml` plus `$script:ExpectedSemverMajorPair` in the test). Optional hardening; not a defect; outside the orchestrator's stated scope unless adopted.
6. **Assembly-version assumption.** `10.0.0.12` is `[I]` (section 3). Mitigated by step 3 in section 8.
7. **Line drift between the ADAL deletion and this edit** (section 7). Anchor on text.
8. **Nothing requires a human**: see Automation Feasibility.

## 10. Additional write set (Q8), exact repository-relative paths

Beyond the first research's items 1-15 and `UtilitiesCS/app.config` (item 16, now required for the ADAL deletion):

packages.config (insert `<package id="System.Linq.AsyncEnumerable" version="10.0.12" targetFramework="net481" />` immediately after the `System.Linq.Async` line; this keeps NuGet's case-insensitive id order, since the next entries are `System.Linq.Expressions` or `System.Memory` `[V]`):

17. `UtilitiesCS/packages.config` (after line 97)
18. `QuickFiler/packages.config` (after line 47)
19. `ToDoModel/packages.config` (after line 20)
20. `TaskMaster/packages.config` (after line 43)
21. `UtilitiesCS.Test/packages.config` (after line 91)

csproj (insert after the closing `</Reference>` of the `System.Linq.Async` element; next sibling is `System.Linq.Expressions` or `System.Memory` `[V]`):

22. `UtilitiesCS/UtilitiesCS.csproj` (after line 400)
23. `QuickFiler/QuickFiler.csproj` (after line 185)
24. `ToDoModel/ToDoModel.csproj` (after line 87)
25. `TaskMaster/TaskMaster.csproj` (after line 246)
26. `UtilitiesCS.Test/UtilitiesCS.Test.csproj` (after line 898)

Element to insert (`<v>` = verified assembly version, expected `10.0.0.12`):

```xml
    <!-- Aliased on purpose (issue #973): the project compiles against lib\net48\System.Linq.Async.dll,
         which still defines a public System.Linq.AsyncEnumerable type for binary compatibility; a global
         reference to the BCL assembly of the same type name makes every shared operator call CS0121.
         The alias keeps the assembly deployed and resolvable without putting its types in scope. -->
    <Reference Include="System.Linq.AsyncEnumerable, Version=<v>, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a, processorArchitecture=MSIL">
      <HintPath>..\packages\System.Linq.AsyncEnumerable.10.0.12\lib\net462\System.Linq.AsyncEnumerable.dll</HintPath>
      <Aliases>SystemLinqAsyncEnumerable</Aliases>
    </Reference>
```

app.config: no new files. The 15 `System.Linq.AsyncEnumerable` redirect lines (section 7) fall inside files already in the write set: items 1-14 plus `UtilitiesCS/app.config`.

Test literal: `tests/scripts/dependencies/BindingRedirectVerification.Tests.ps1:310` (`$expectedUnverifiable`), already item 15; the value becomes `@('netstandard')` when both the ADAL deletion and this install land.

Optional (risk 5, only if adopted): `.github/dependabot.yml`, `tests/scripts/dependencies/DependabotConfig.Tests.ps1`.

Explicitly not written: any `*.cs`; `SVGControl/app.config`; `SVGControl.Test/app.config`; any csproj or packages.config other than the ten above; `scripts/dependencies/*.psm1`; `Directory.Build.props`; `Directory.Build.targets`.

## 11. Behaviour semantics and testing implications

- Success: after restore and the ten manifest/project edits, `msbuild /t:Rebuild` of the solution completes with `0 Warning(s) 0 Error(s)` under both gate property sets (the same figures as the committed #825 logs); `System.Linq.AsyncEnumerable.dll` is present in `UtilitiesCS\bin\Debug`, `QuickFiler\bin\Debug`, `ToDoModel\bin\Debug`, `TaskMaster\bin\Debug`, `UtilitiesCS.Test\bin\Debug` and, by dependency copy, in the ten dependents' output directories; `Find-StaleBindingRedirect` reports zero findings and an unverifiable set of `@('netstandard')`.
- Failure conditions the plan must make observable: (a) CS0121/CS0433 in any of the four production projects means the `<Aliases>` metadata is missing or misspelt; (b) 15 findings naming `System.Linq.AsyncEnumerable|10.0.0.7` means the csproj landed without the redirect edit (or with a version string different from the DLL's); (c) `MissingReference`/`OrphanedHintPath` from `scripts/dependencies/Repair-PackageManifestConsistency.ps1` means a packages.config/csproj pair is incomplete.
- Regression test (Bugfix Workflow ordering): set `$expectedUnverifiable` to `@('netstandard')` first and run the Pester gate: it must fail naming `System.Linq.AsyncEnumerable` (and `Microsoft.IdentityModel.Clients.ActiveDirectory` until the ADAL deletion) as still unverifiable; after the ten edits plus the 15 redirect edits it must pass. Record the red and green runs under `docs/features/active/2026-10-02-remaining-stale-binding-redirect-pairs-973/evidence/regression-testing/`.
- Durability assertion (recommended, PowerShell test code only): a new `It` in `tests/scripts/dependencies/RepositoryTreeConsistency.Tests.ps1` (the existing repository-tree gate, `:63`) asserting that for every `*/*.csproj` whose text contains `Reference Include="System.Linq.AsyncEnumerable,` the same `<Reference>` element also contains `<Aliases>`; it is red if a future update strips the alias and guards risk 2 before the C# build does.
- MSTest run: required by the C# policy because `*.csproj` files change. Because the aliased reference changes no compiled call site, a green run demonstrates non-regression of the five test hosts with the BCL assembly now on disk and the redirects pointing at it; `UtilitiesCS.Test/Extensions/IAsyncEnumerableExtensions_Tests.cs` and `OlTableExtensions_Tests.cs` exercise `ToListAsync`/`ToAsyncEnumerable` through the Ix runtime class in that configuration.
- Coverage: no production C# line changes, so no coverage movement is expected; the usual projection is still produced for the record.

## Automation Feasibility

- Every step is automatable by the executor: hand edits to ten tracked manifests/projects and 15 config lines; restore through `scripts/vscode/Invoke-Restore.ps1` (needs network access to api.nuget.org, which the delegation states is available); assembly-version read via one `pwsh -NoProfile -Command` invocation; the four C# gates and the Pester gate as already scripted. No Visual Studio UI, no Package Manager Console and no maintainer action is required.
- The one judgement call that is not automatable is the choice between the aliased Reference (recommended) and abandoning the install; that is an orchestrator decision on this document, not a runtime step.
- The manual WinForms-designer and Outlook-host checks described in the first research, section 8, are unchanged in nature and remain insensitive to this change (the designer ignores app.config; the add-in now simply has one more DLL beside `TaskMaster.dll`). If the maintainer wants host evidence, a manual add-in start with a `debug_<date>.log` check for `FileNotFoundException`/`FileLoadException` naming `System.Linq.AsyncEnumerable` is the only addition.

## Numeric Derivation Evidence

### Count A: configs carrying a `System.Linq.AsyncEnumerable` redirect (15)

- Complete Family: every `app.config` directly under a root-level project directory that contains a `dependentAssembly` block whose `assemblyIdentity name="System.Linq.AsyncEnumerable"`.
- Exhaustive Search Scope: all 17 `*/app.config` files in the worktree (the first research's Glob inventory, section 2.1, unchanged on this branch); the detector's own enumeration rule (`BindingRedirectVerification.Tests.ps1:278-282`).
- Inclusion Rules: file contains the exact `assemblyIdentity name="System.Linq.AsyncEnumerable"` attribute; counted once per file.
- Exclusion Rules: files with no such block; `assemblyIdentity` elements for other names.
- Primary Search Strategy or Query Expression: Grep `System\.Linq\.AsyncEnumerable` over glob `*/app.config` with one line of trailing context (content mode).
- Primary Member Set: VBFunctions.Test, QuickFiler.Test, TaskVisualization, TaskTree.Test, TaskMaster, Tags.Test, TaskTree, TaskMaster.Test, Tags, QuickFiler, ToDoModel, TaskVisualization.Test, UtilitiesCS.Test, ToDoModel.Test, UtilitiesCS.
- Primary Count: 15
- Cross-check Search Strategy or Query Expression: complement enumeration: take the 17-file inventory (Tags, Tags.Test, TaskMaster, TaskMaster.Test, TaskTree, TaskTree.Test, TaskVisualization, TaskVisualization.Test, ToDoModel, ToDoModel.Test, UtilitiesCS, UtilitiesCS.Test, VBFunctions.Test, QuickFiler, QuickFiler.Test, SVGControl, SVGControl.Test) and remove the files the first research read directly as carrying no block for the name (SVGControl, SVGControl.Test; its section 3 totals paragraph and section 6 row 2), confirmed in this session by the absence of any SVGControl hit in a different-shaped query, Grep `assemblyIdentity name="(Microsoft\.Bcl\.AsyncInterfaces|System\.Threading\.Tasks\.Extensions|System\.ValueTuple|Microsoft\.Bcl\.Memory)"` over `*/app.config`, which did return SVGControl.Test hits for other names (so the file was searched) and none for this one.
- Cross-check Member Set: Tags, Tags.Test, TaskMaster, TaskMaster.Test, TaskTree, TaskTree.Test, TaskVisualization, TaskVisualization.Test, ToDoModel, ToDoModel.Test, UtilitiesCS, UtilitiesCS.Test, VBFunctions.Test, QuickFiler, QuickFiler.Test.
- Cross-check Count: 15
- Member-set Comparison: normalized sets are identical (15 names, none missing from either side).

### Count B: projects that must receive the package (5)

- Complete Family: every project that compiles against `System.Linq.Async` or `System.Interactive.Async`.
- Exhaustive Search Scope: all 18 `*/packages.config` and all 18 `*/*.csproj` files directly under root-level project directories.
- Inclusion Rules: packages.config declares `System.Linq.Async` or `System.Interactive.Async`; csproj carries a `Reference Include` beginning with either name.
- Exclusion Rules: projects that only receive the DLL through a `ProjectReference` copy (no manifest entry, no Reference).
- Primary Search Strategy or Query Expression: Grep `System\.(Linq\.Async|Interactive\.Async|Linq\.AsyncEnumerable)` over glob `*/packages.config`.
- Primary Member Set: TaskMaster, QuickFiler, UtilitiesCS.Test, ToDoModel, UtilitiesCS (each with both packages).
- Primary Count: 5
- Cross-check Search Strategy or Query Expression: Grep the same alternation over glob `*/*.csproj` (a different file family and element kind: `Reference Include` / `HintPath`).
- Cross-check Member Set: UtilitiesCS.Test, QuickFiler, UtilitiesCS, TaskMaster, ToDoModel (each with both References and HintPaths).
- Cross-check Count: 5
- Member-set Comparison: normalized sets are identical.

### Count C: additional packages required by the install (0)

- Complete Family: dependencies declared by `System.Linq.AsyncEnumerable 10.0.12` for the group NuGet resolves for net481 (`.NETFramework4.6.2`): Microsoft.Bcl.AsyncInterfaces >= 10.0.12, Microsoft.Bcl.Memory >= 10.0.12, System.Threading.Tasks.Extensions >= 4.6.3, System.ValueTuple >= 4.6.2 `[V-web]`.
- Exhaustive Search Scope: the five target projects' `packages.config` and `.csproj` files.
- Inclusion Rules: a dependency is additional write set if absent from a target project's packages.config or present below the lower bound.
- Exclusion Rules: dependencies present at or above the bound.
- Primary Search Strategy or Query Expression: Grep `package id="(Microsoft\.Bcl\.AsyncInterfaces|Microsoft\.Bcl\.Memory|System\.Threading\.Tasks\.Extensions|System\.ValueTuple)"` over `*/packages.config`.
- Primary Member Set: (none additional) all four ids present at 10.0.12 / 10.0.12 / 4.6.3 / 4.6.2 in UtilitiesCS, QuickFiler, ToDoModel, TaskMaster, UtilitiesCS.Test (20 lines, section 4).
- Primary Count: 0
- Cross-check Search Strategy or Query Expression: Grep `Reference Include="(Microsoft\.Bcl\.AsyncInterfaces|Microsoft\.Bcl\.Memory|System\.Threading\.Tasks\.Extensions|System\.ValueTuple)[,"]` with one trailing line over `*/*.csproj`, reading the HintPath package folders (`Microsoft.Bcl.AsyncInterfaces.10.0.12`, `Microsoft.Bcl.Memory.10.0.12`, `System.Threading.Tasks.Extensions.4.6.3`) in all five projects; `System.ValueTuple` has no HintPath'd Reference anywhere (in-box on net47+), so for it the cross-check is the app.config/packages.config pair only.
- Cross-check Member Set: (none additional).
- Cross-check Count: 0
- Member-set Comparison: both routes find no dependency missing or below its bound; sets are identical (empty).

## Rejected alternatives (summary)

- Global (NuGet-form) Reference: duplicate public type, CS0121/CS0433 at 60 production call lines (section 5).
- HintPath to `ref\net48`: copies a reference assembly to the output directory (section 6, item 3).
- Rewrite call sites to the BCL and remove System.Linq.Async: out of scope for a redirect-hygiene item (section 6, item 4).
- Installing in the ten ProjectReference-only projects as well: no precedent in the repository for installing a package a project does not compile against (section 2.1).
- Choosing `10.0.7` to avoid touching the redirects: the sweep still edits the configs for the other 15 pairs, Dependabot would bump it within a week, and the dependency set is already pinned at 10.0.12.
