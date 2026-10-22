# Research — FSharp.Core HintPath netstandard2.1 skew (Issue #895)

- **Issue:** #895
- **Work mode:** full-bug
- **Written:** 2026-09-16T23-30
- **Tree studied:** the active feature worktree at `<repo-root>` (branch `TaskMaster-wt-2026-09-12T10-15`), plus read-only inspection of the primary checkout's restored `packages/` and built `bin/Debug` trees for cross-checks that need a restored and built tree

> Tool constraint for this session: this agent had Read, Grep, Glob, Write, Edit and WebFetch only. No Bash tool and no PowerShell tool were available, so nothing was executed: no build, no `pwsh` probe, no `git` command. Every finding below was verified by reading files or by Grep/Glob; every claim that would need execution is marked as such.

---

## 1. Current state — verified facts

### 1.1 The six HintPath entries (re-derived; line numbers are current)

| Project file | `<Reference Include>` line | `<HintPath>` line | Flavour |
|---|---|---|---|
| `QuickFiler/QuickFiler.csproj` | 51 | 52 | `lib\netstandard2.1` |
| `QuickFiler.Test/QuickFiler.Test.csproj` | 258 | 259 | `lib\netstandard2.1` |
| `ToDoModel/ToDoModel.csproj` | 41 | 42 | `lib\netstandard2.1` |
| `UtilitiesCS/UtilitiesCS.csproj` | 69 | 70 | `lib\netstandard2.0` |
| `UtilitiesCS.Test/UtilitiesCS.Test.csproj` | 598 | 599 | `lib\netstandard2.0` |
| `ToDoModel.Test/ToDoModel.Test.csproj` | 95 | 96 | `lib\netstandard2.0` |

- The issue table cites `UtilitiesCS.Test.csproj:598`; that is the `<Reference Include>` line. The `<HintPath>` line is 599. Same block, no drift in content.
- All six `Include` attributes carry the identical identity `FSharp.Core, Version=11.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a`. The flavour difference is invisible at the identity level, which is why MSBuild raises no MSB3277 conflict for it.
- None of the six `<Reference>` blocks carries a `<Private>` child, so copy-local is the default (true for a non-GAC HintPath reference).
- There are 18 `.csproj` files in the tree (Glob `**/*.csproj`). The other 12 contain no `FSharp.Core` text at all. There is no `<PackageReference>` form anywhere: `.claude/rules/csharp.md:77` records that the solution is `packages.config`-only by decision, and Grep confirms no `PackageReference` for FSharp.Core.

### 1.2 `packages.config` uniformity — one omission found

| Project | `packages.config` FSharp.Core entry |
|---|---|
| `QuickFiler/packages.config:8` | `11.0.100` |
| `QuickFiler.Test/packages.config:9` | `11.0.100` |
| `ToDoModel/packages.config:5` | `11.0.100` |
| `UtilitiesCS/packages.config:13` | `11.0.100` |
| `UtilitiesCS.Test/packages.config:10` | `11.0.100` |
| `ToDoModel.Test/packages.config` | **absent** (172 lines read in full; no `FSharp.Core` and no `Deedle` entry, although `ToDoModel.Test.csproj:92-96` carries HintPaths for both) |

No `packages.config` pins any version other than `11.0.100`. The `ToDoModel.Test` omission is a separate latent defect: its two HintPaths resolve today only because five sibling projects restore the same package folders. `scripts/vscode/Sync-PackageReferences.ps1:69-79` skips a HintPath whose package id is not in the project's own `packages.config`, so on the next FSharp.Core or Deedle bump `ToDoModel.Test` would be left pointing at a folder that no longer exists (CS0006). Recommend promoting this to its own issue rather than folding it into #895.

### 1.3 Non-csproj FSharp.Core surface (`*.config`)

- Fifteen `app.config` files carry a `FSharp.Core` `<dependentAssembly>` with `bindingRedirect oldVersion="0.0.0.0-11.0.0.0" newVersion="11.0.0.0"`: `QuickFiler`, `QuickFiler.Test`, `ToDoModel`, `ToDoModel.Test`, `UtilitiesCS`, `UtilitiesCS.Test`, `Tags`, `Tags.Test`, `TaskTree`, `TaskTree.Test`, `TaskVisualization`, `TaskVisualization.Test`, `TaskMaster`, `TaskMaster.Test`, `VBFunctions.Test`.
- All fifteen redirect on the **assembly-version axis** (`11.0.0.0`), which both flavours expose identically. None mentions a flavour or a `lib\` path. **No `*.config` change is required by, or affected by, this fix.**
- Exactly one `*.config` names `netstandard`: `TaskMaster/app.config:72-75`, `oldVersion="0.0.0.0-2.1.0.0" newVersion="2.0.0.0"`. This is the #879 declarative hardening (its research at `docs/features/active/2026-09-13-deedle-netstandard-21-bind-unsatisfiable-in-production-879/research/...` recorded zero `netstandard` config hits before #879 landed). It is deployed only as `TaskMaster.dll.config`; no test host's config has it. It is independent of this fix and must not be removed by it.
- `coverage.config:15` and `scripts/vscode/TaskMaster.cli.runsettings` exclude `.*FSharp.*` from instrumentation. Not affected.

### 1.4 The package on disk

`packages/` is gitignored and is **not restored in this worktree** (Glob `packages/FSharp.Core*/**` returned nothing). In the primary checkout it is restored and contains exactly `lib/netstandard2.0/{FSharp.Core.dll,FSharp.Core.xml}` and `lib/netstandard2.1/{FSharp.Core.dll,FSharp.Core.xml}`. No `build/`, `buildTransitive/`, `.props` or `.targets` content exists under the package, so the package injects nothing into the build besides the HintPath'd binaries.

### 1.5 Other places a `netstandard2.1` FSharp.Core.dll could enter a build — none found, one latent

Searched: every `*.props`, `*.targets`, `*.sln`, `*.json`, `*.yml`, `*.ps1`, `*.psm1` for `FSharp`; every file outside `docs/`, `.claude/`, `packages/`, `bin/`, `obj/` for `FSharp.Core.dll`. Results: the six HintPaths, three script comments, `coverage.config`/runsettings exclusions, and one test constant (`TaskMaster.Test/Bootstrap/NetstandardBindChildDomainTests.cs:54`). Therefore:

- No `<Content>`/`<None>` item copies `FSharp.Core.dll`.
- No `.targets` copies from the package `lib/` root.
- `Directory.Build.props` exists (18 lines) and sets only `RxUseUnsupportedPackagesConfig`.
- CI (`_build-analyzers.yml:61`, `_build-nullable.yml:61`, `_mstest-coverage.yml:77`) runs `nuget restore` then `msbuild` directly. Restore extracts the whole package (both flavours), which is expected; only HintPaths select between them.
- **Latent entry point: `scripts/vscode/Sync-PackageReferences.ps1`.** It runs before every build launched through `scripts/vscode/Invoke-VSBuild.ps1:250-253` (the four `.vscode/tasks.json` build tasks); CI does not call it. It rewrites any HintPath that does not resolve on disk. Lines 85-88 try the same TFM folder first, but when that folder is absent the fallback search at lines 13-19 uses a preference list that ranks `'netstandard2.1'` **ahead of** `'netstandard2.0'`. For a `net481` project that order is inverted: .NET Framework implements .NET Standard 2.0 at most, and NuGet's own compatibility rules never select `netstandard2.1` for `net481`. This script is the only tool in the repository that can write `lib\netstandard2.1` into a project file, and it produces exactly the observed split whenever a project's previous HintPath TFM folder (for example `net45` from an older FSharp.Core) disappears in a bump while projects already on `netstandard2.0` keep resolving. That origin is a hypothesis consistent with the script's logic; commit history was not available to confirm it.

After all six HintPaths point at `lib\netstandard2.0`, the script leaves them alone (the path resolves), so the fix is stable against the script until a future bump ships a package with no `netstandard2.0` folder, at which point no flavour would be loadable anyway. Reordering the preference list is a hardening change with no in-scope trigger; see section 7.

---

## 2. Output-directory enumeration

### 2.1 ProjectReference graph (from Grep `<ProjectReference Include=` over all 18 `.csproj`; 41 edges)

```
UtilitiesCS        -> SVGControl
Tags               -> UtilitiesCS
ToDoModel          -> Tags, UtilitiesCS
TaskTree           -> ToDoModel, UtilitiesCS
TaskVisualization  -> Tags, ToDoModel, UtilitiesCS
QuickFiler         -> SVGControl, TaskVisualization, ToDoModel, UtilitiesCS
TaskMaster         -> QuickFiler, Tags, TaskTree, TaskVisualization, ToDoModel, UtilitiesCS
SVGControl.Test    -> SVGControl
VBFunctions.Test   -> UtilitiesCS, VBFunctions
Tags.Test          -> Tags, UtilitiesCS
ToDoModel.Test     -> ToDoModel, UtilitiesCS
TaskTree.Test      -> TaskTree, ToDoModel, UtilitiesCS
TaskVisualization.Test -> TaskVisualization, ToDoModel, UtilitiesCS
QuickFiler.Test    -> QuickFiler, UtilitiesCS, TaskVisualization
UtilitiesCS.Test   -> TaskMaster, UtilitiesCS
TaskMaster.Test    -> TaskMaster, ToDoModel, UtilitiesCS
SVGControl, VBFunctions -> (none)
```

Every project's `Debug|Any CPU` `OutputPath` is `bin\Debug\` (verified for all 18). Some test projects also declare `bin\x86\Debug\` etc., but the mandated command builds `Debug|Any CPU` only.

### 2.2 The fifteen receiving directories and how each gets its copy

A direct `<Reference>` with a HintPath is a primary reference and takes precedence over the same identity discovered as a dependency, so the six direct projects deterministically deploy their own HintPath flavour. A project with no direct reference receives `FSharp.Core.dll` because ResolveAssemblyReferences discovers it as a dependency of one or more referenced-project outputs and resolves it from those outputs' directories; when more than one parent directory carries a different flavour under the same identity, **no source file expresses which one is chosen**. That is the nondeterminism the issue describes.

| # | Output directory (`<proj>/bin/Debug`) | Source of the copy | Flavour on the unfixed tree |
|---|---|---|---|
| 1 | `QuickFiler` | own HintPath | 2.1 (deterministic) |
| 2 | `QuickFiler.Test` | own HintPath | 2.1 (deterministic) |
| 3 | `ToDoModel` | own HintPath | 2.1 (deterministic) |
| 4 | `UtilitiesCS` | own HintPath | 2.0 (deterministic) |
| 5 | `UtilitiesCS.Test` | own HintPath | 2.0 (deterministic) |
| 6 | `ToDoModel.Test` | own HintPath | 2.0 (deterministic) |
| 7 | `Tags` | dependency via UtilitiesCS only | 2.0 (single parent) |
| 8 | `Tags.Test` | via Tags, UtilitiesCS | 2.0 (all parents 2.0) |
| 9 | `VBFunctions.Test` | via UtilitiesCS (VBFunctions has none) | 2.0 (single parent) |
| 10 | `TaskTree` | via ToDoModel (2.1), UtilitiesCS (2.0) | **unspecified** |
| 11 | `TaskVisualization` | via Tags (2.0), ToDoModel (2.1), UtilitiesCS (2.0) | **unspecified** |
| 12 | `TaskTree.Test` | via TaskTree (?), ToDoModel (2.1), UtilitiesCS (2.0) | **unspecified** |
| 13 | `TaskVisualization.Test` | via TaskVisualization (?), ToDoModel (2.1), UtilitiesCS (2.0) | **unspecified** |
| 14 | `TaskMaster` (the add-in) | via QuickFiler (2.1), Tags, TaskTree (?), TaskVisualization (?), ToDoModel (2.1), UtilitiesCS | **unspecified** |
| 15 | `TaskMaster.Test` | via TaskMaster (?), ToDoModel (2.1), UtilitiesCS (2.0) | **unspecified** |

Not receiving a copy: `SVGControl`, `SVGControl.Test`, `VBFunctions` (none references any FSharp.Core-bearing project).

Three independent enumerations agree on this fifteen-member set (see `## Numeric Derivation Evidence`). The "unspecified" column is derived from the graph, not measured: no build ran in this session. The issue's own measurement (QuickFiler.Test raises the chain; TaskMaster.Test does not) is consistent with rows 2 and 15.

Consequence for gate design: rows 1-3 fail a flavour assertion on the unfixed tree **deterministically**, independent of build order. Any per-directory criterion that includes those three directories is guaranteed to be observed failing before the fix. Rows 10-15 are the ones that can flip between builds and are the reason a per-directory check must cover all fifteen, not a sample.

---

## 3. Sufficiency question: does aligning the six HintPaths on `netstandard2.0` guarantee no output directory can receive the 2.1 flavour?

**Yes, for everything the repository controls**, by the following argument:

1. Every `FSharp.Core.dll` that reaches any of the fifteen directories originates from a HintPath-resolved primary reference in one of the six projects (section 1.5 verified there is no other copy mechanism: no PackageReference, no props/targets, no Content/None item, no package build assets, no manifest entry).
2. Transitive copies are made from the outputs of those six projects (section 2.2). If all six deploy 2.0, every parent directory a dependency can be resolved from holds 2.0, so the unspecified choice in rows 10-15 becomes a choice among identical files.
3. `nuget restore` extracting `lib/netstandard2.1` into `packages/` is harmless: nothing reads it unless a HintPath names it.

Two caveats, neither of which makes a separate "no project transitively still references 2.1" check necessary today:

- `Sync-PackageReferences.ps1` (section 1.5) can reintroduce `netstandard2.1` on a future bump only if the `netstandard2.0` folder vanishes. A source-level guard (Shape A, section 4) catches that regression at test time because it asserts the flavour, not merely agreement.
- Environmental: if a machine had FSharp.Core 11.0.0.0 in its GAC, ResolveAssemblyReferences would set copy-local false and deploy nothing. Not the case on the machines that produced the committed evidence (fifteen `bin/Debug` copies exist in the primary checkout). Out of scope.

A separate transitive check is therefore **not needed as a distinct criterion**; Shape B (section 4) subsumes it by inspecting the deployed binary in every one of the fifteen directories, which is the only observable that matters.

---

## 4. Acceptance-criterion shapes

### 4.1 Shape A — static assertion over source

Assert that every `FSharp.Core` `<HintPath>` in every `.csproj` in the solution ends in `\lib\netstandard2.0\FSharp.Core.dll` **and** that exactly six such HintPaths exist.

- Observed failing today: yes, three of six name `netstandard2.1`. No build, no restore, no `packages/` needed.
- Must assert the **flavour**, not just agreement. An agreement-only assertion passes a tree where all six were edited to `netstandard2.1`, which the orchestrator correctly identified as the hole. Pinning `netstandard2.0` closes it.
- Must assert the **count** (six). Otherwise deleting a HintPath, or moving a project to a different package folder name, passes vacuously.
- Covers every configuration (Release, x86) because it reads source, whereas Shape B reads one built configuration.
- Limitation: proves what the project files say, not what the build deployed. It cannot see the transitive-copy question or any future non-HintPath entry point.

### 4.2 Shape B — per-output-directory assertion over the deployed binary

After a real solution build, for each of the fifteen directories in section 2.2, open `<dir>/FSharp.Core.dll` and assert that the assembly's referenced-assembly table names `netstandard` at `Version=2.0.0.0` (equivalently: does not name `2.1.0.0`).

- Observed failing today: yes, deterministically in rows 1-3, with rows 10-15 as possible additional failures depending on build order.
- Directly asserts the runtime hazard the issue reports.
- Requires a built tree; not executable at planning time (and not executable in this research session at all).

**How to read the referenced-assembly table without loading the assembly** (verified availability):

| Mechanism | Available here? | Notes |
|---|---|---|
| `System.Reflection.Metadata` (`PEReader` + `MetadataReader.AssemblyReferences`) from an MSTest test | **Yes.** Every test project already references `System.Reflection.Metadata 10.0.0.12` (for example `TaskMaster.Test/TaskMaster.Test.csproj:241`; `System.Collections.Immutable 10.0.12` is in `packages.config`). No project-file edit for the dependency. | In-process, no assembly load, no AppDomain, no temp file. Reads any number of same-identity files from different paths. No existing use in the repository (Grep for `PEReader|MetadataReader` over `*.cs` returns nothing), so this is a first use, not a pattern change. **Recommended.** |
| `System.Reflection.Metadata` from `pwsh` 7 | Yes on this machine: `<program-files>\PowerShell\7\System.Reflection.Metadata.dll` exists on disk. | Would live in `scripts/vscode` and fall under the Pester coverage gate (`_pester.yml:41-45`), adding a script plus tests. Rejected as heavier than the MSTest route. Not available in Windows PowerShell 5.1 in-box. |
| `Assembly.ReflectionOnlyLoadFrom(path).GetReferencedAssemblies()` | Works on net481, but the reflection-only context de-duplicates by identity: a second load of `FSharp.Core 11.0.0.0` from a different path in the same domain fails. Fifteen directories would need fifteen child `AppDomain`s. The #879 harness (`TaskMaster.Test/Bootstrap/NetstandardBindChildDomainTests.cs`) shows that pattern is workable but heavy and needs `[DoNotParallelize]`. Rejected. |
| `AssemblyName.GetAssemblyName(path)` | Insufficient: returns only the assembly's own identity, which is `11.0.0.0` for both flavours. `Sync-PackageReferences.ps1:130` uses it for that reason (own version only). |
| `ildasm` / `dotnet` CLI | Not needed and not probed. |
| Byte-identity to `packages/FSharp.Core.11.0.100/lib/netstandard2.0/FSharp.Core.dll` (SHA-256) | Simple and flavour-discriminating, but asserts file identity rather than the referenced `netstandard` version, and hard-codes the package folder. Rejected as primary; acceptable as a secondary cross-check if a plan wants one. |

### 4.3 Recommendation: require **both**, A first

1. **Shape A is the Phase-2 regression test** (CLAUDE.md Bugfix Workflow step 1). It is the only criterion that can be observed failing with no build and no `packages/` restore, which matters given the command-channel uncertainty in section 5. It also pins the flavour so a wrong-direction edit cannot pass.
2. **Shape B is the build-verified gate**, executed after the Phase-3 edit and again in the final QA loop. It is what proves the deployed state, covers rows 10-15, and turns the issue's "consider a build-time guard" into a permanent regression guard. Its expect-fail run against the unfixed tree is required by the observed-failing rule; rows 1-3 guarantee that run fails.
3. Shape A alone is necessary but not sufficient (cannot see deployment); Shape B alone is sufficient for the hazard but cannot run without a build and does not cover non-Debug configurations. Together they cover source, deployment and the "wrong flavour everywhere" hole.

**Where they live (recommended):** one new MSTest class in `TaskMaster.Test/Bootstrap/` (kept under 500 lines), registered by a `<Compile Include>` in `TaskMaster.Test/TaskMaster.Test.csproj` (legacy project files list sources explicitly; this is a project-file edit outside the six, and the plan should say so).

- Repository root discovery: walk up from `AppDomain.CurrentDomain.BaseDirectory` to the directory containing `TaskMaster.sln`, the precedent at `TaskMaster.Test/Ribbon/RibbonControllerTests.cs:422-437` and `UtilitiesCS.Test/EmailIntelligence/SortEmail_Tests.cs:405`.
- Shape A reads the six `.csproj` files with `XDocument` (precedent for structural XML assertions: `TaskMaster.Test/Bootstrap/AddInEagerInstallShapeTests.cs:80-120`). Enumerate the `.csproj` set by Glob of the repository so a seventh HintPath added later is caught, then assert count == 6 and each ends in `\lib\netstandard2.0\FSharp.Core.dll`.
- Shape B iterates a **fixed, literal list of the fifteen directory names** from section 2.2 (not a directory glob, per the orchestrator's "non-executor-chosen list" requirement), throws `InvalidOperationException` if any directory or file is missing (the #879 harness convention at `NetstandardBindChildDomainTests.cs:412-434`: fail loud, never skip), and asserts the `netstandard` reference version.
- Include a **detector positive control**: read `packages/FSharp.Core.11.0.100/lib/netstandard2.1/FSharp.Core.dll` and assert the detector reports `2.1.0.0`. Without this, a detector that never finds a `netstandard` reference would pass every directory. Mirrors the negative-control discipline of `NegativeControl_WithoutInstall_Netstandard21Throws`. Derive the package folder from one of the six HintPaths rather than hard-coding the version string.
- No `[DoNotParallelize]` is needed: both tests are read-only over files and use no shared mutable state, so they comply with the Workers=0/ClassLevel requirement as-is.
- Determinism: no clock, no RNG, no temp files, no processes. The tests depend on the solution having been built, which is true in CI (`_mstest-coverage.yml:79-84` builds the whole solution first) and is the documented precondition locally.

### 4.4 Interaction with the #879 harness (already on this tree)

`UtilitiesCS/Bootstrap/AssemblyBindingFallback.cs`, `TaskMaster/app.config:72-75`, and `TaskMaster.Test/Bootstrap/{ChildDomainBindProbe,NetstandardBindChildDomainTests,AddInEagerInstallShapeTests}.cs` are present, so #879 has landed here.

- `NegativeControl_WithoutInstall_Netstandard21Throws` binds the `netstandard 2.1.0.0` display name directly, not through FSharp.Core, so it keeps failing-without-installer after this fix. The `ISOLATION-LOST-INVARIANT` is unaffected.
- `AfterInstall_DeedleTypeInitializerSucceeds` will pass **with or without** the #879 installer once `QuickFiler.Test/bin/Debug` deploys the 2.0 flavour, because the 2.0 flavour requests `netstandard 2.0.0.0`, which the child-domain observation (`evidence/other/netstandard-2-0-0-0-child-domain-observation...`) shows binds without any handler. Its discriminating power moves entirely to the display-name tests. This is expected and is exactly the "complementary" relationship the issue describes; it should be recorded, not fixed.
- The remarks at `NetstandardBindChildDomainTests.cs:200-208` and `:364-369` state that the QuickFiler.Test directory "deploys the flavour that references the unsatisfiable netstandard 2.1.0.0 identity". After this fix that sentence is false. Recommend a comment-only update in Phase 5, or an explicit note in the spec that the remark is historical. The #879 feature folder is still under `docs/features/active/`, so the plan should state the ownership decision.

---

## 5. Command channel — not probed

This agent had no Bash tool, so neither `pwsh -NoProfile -Command "Write-Output ok"` nor `pwsh -NoProfile -File <script>` was attempted. No refusal text and no success text was observed in this session.

What the repository's own records say:

- The refusal, when it occurs, comes from the Bash tool's worktree-isolation filter and reads: `this command runs pwsh in a plain command; what it reads or is handed as shell text cannot be shown not to run git. Refusing to run it.` It applies equally to `msbuild`, `vstest.console.exe` and `dotnet`, and naming the agent's own worktree in `-Command` or `-WorkingDirectory` does not satisfy it (user-scope memory `project_pwsh_refused_in_isolated_worktree_agents`, verified 2026-09-09).
- The same record's 2026-09-11 correction: the refusal is **session-dependent**. On 2026-09-07 an `.claude/worktrees/` agent ran `pwsh -NoProfile -Command`, `pwsh -NoProfile -File <abs>.ps1` and a 29-task plan on that channel with no refusal.
- The #879 executor on this same issue family ran `pwsh -NoProfile -Command '...'` successfully (its evidence at `.../879/evidence/baseline/build-output-premises.2026-09-13T18-22.md:8-25`), and noted it was launched **without** worktree isolation.

Recommendation for the plan: author every command task as a two-rung probe-then-fallback (attempt `pwsh -NoProfile -File`; on refusal, record the refusal text verbatim in the task's evidence artifact and stop-and-report rather than rewrite the command), and put a single explicit probe task in Phase 0 so the channel is known before any gate depends on it. Shape A does not depend on the channel at planning time, which is a further reason to make it the first gate.

---

## 6. Tier classification

- `quality-tiers.yml` does not exist at the repository root and `docs/ci.research.md` does not exist anywhere (Glob for both: no files). `.claude/rules/quality-tiers.md` and `.claude/rules/general-code-change.md:29` reference them, but there is nothing to read. Issue #494's research (`docs/features/archive/2026-08-10-coverage-threshold-policy-reconciliation-494/spec.md:153-160, 199-226`) records the same absence and concludes the 85/75/T1-T4 cluster is reference-repository leakage.
- Effective policy for this work is therefore CLAUDE.md UT2: repository-wide line coverage `>= 80%` on the testable denominator, `>= 90%` for new code, COM/VSTO/WinForms exemption for the add-in lifecycle classes.
- Impact on this fix: the production change is project-file-only (no `.cs` in the six projects changes), so the coverage delta on changed lines is nil; the new tests are test code and outside the denominator. The plan still owes the baseline/final coverage artifacts and comparison the plan template requires, and must expect the +/-15 pt run-to-run spread already recorded for repository-wide coverage rather than attributing it to this change.
- `TaskMaster` is the VSTO add-in (`ThisAddIn` carries `[ExcludeFromCodeCoverage]`); nothing in it changes here.

---

## 7. Recommended approach and rejected alternatives

**Recommended:** edit the three `netstandard2.1` HintPaths (`QuickFiler.csproj:52`, `QuickFiler.Test.csproj:259`, `ToDoModel.csproj:42`) to `..\packages\FSharp.Core.11.0.100\lib\netstandard2.0\FSharp.Core.dll`; add the two-shape MSTest guard in `TaskMaster.Test/Bootstrap/` with its `<Compile Include>`; record the #879 harness interaction. `*.csproj` is in `.csharpierignore`, so the format step does not touch the edit; `.claude/rules/csharp.md:4` lists `**/*.csproj` in that rule's scope, so the C# toolchain loop still applies.

**Rejected alternatives (brief):**
- *Align all six on `netstandard2.1`.* Unloadable on .NET Framework; would move the failure to every directory.
- *Downgrade or pin FSharp.Core below 11.* Changes fifteen redirects and the package graph; #879 research already rejected it.
- *Post-restore deletion of `lib/netstandard2.1`.* Fights `nuget restore` on every clean checkout; fragile.
- *Reorder `Sync-PackageReferences.ps1`'s TFM preference in this fix.* Correct hardening, but `tests/scripts/vscode/` has no test file for that script, so the change would have to bring Pester coverage with it under the `_pester.yml` gate. No in-scope trigger exists once the six HintPaths resolve. Recommend a follow-up issue instead.
- *Pester-hosted Shape B.* Executable (`System.Reflection.Metadata` ships with pwsh 7 here) but adds a script to the Pester coverage denominator for no capability the MSTest route lacks.
- *Child-AppDomain Shape B.* Works (the #879 harness proves the pattern) but needs fifteen domains and serialisation; MetadataReader needs neither.

---

## 8. Behaviour semantics for the gates

- Shape A success: exactly six `FSharp.Core` HintPaths across all `.csproj` files, each ending in `\lib\netstandard2.0\FSharp.Core.dll`. Failure: any other count, any other flavour, any HintPath under a different package folder name.
- Shape B success: each of the fifteen literal directories contains `FSharp.Core.dll` whose metadata `AssemblyReferences` includes `netstandard` with `Version == 2.0.0.0` and no entry with `2.1.0.0`; the detector control over the package's `netstandard2.1` binary reports `2.1.0.0`. Failure: any directory missing (broken precondition, thrown), any file missing (thrown), any `2.1.0.0` reference (assertion), control not reporting `2.1.0.0` (assertion; the detector is broken).
- Ordering: Shape A before Shape B in every loop; Shape B only after a `/t:Rebuild` of the whole solution, never after a partial build.
- Edge cases: `packages/` absent (control throws; expected in a cold worktree until restore); a directory that received no copy at all (thrown, since all fifteen must receive one).

---

## 9. Testing implications (no test code written)

- Regression tests: the Shape A and Shape B tests above, MSTest + FluentAssertions, no Moq needed (no seams).
- Expect-fail evidence: Shape A fails on the unfixed tree with count 6 but three `netstandard2.1` members; Shape B fails on the unfixed tree in rows 1-3 at minimum. Both runs must be captured before the Phase-3 edit.
- Pass-after evidence: both green after `/t:Rebuild`; the #879 `NetstandardBindChildDomainTests` class must still pass, including its negative control.
- Parallelism: no pinning; the new class is read-only.
- Manual gate (from the issue): fresh Outlook session exercising the QuickFiler ribbon path. Unchanged.

---

## Numeric Derivation Evidence

### Claim 1 — exactly six FSharp.Core HintPath entries across the solution

Complete Family: every `FSharp.Core` reference declaration in every MSBuild project file in the repository, in either the `<Reference Include="FSharp.Core...">` + `<HintPath>` form or a `<PackageReference>` form

Exhaustive Search Scope: all 18 `.csproj` files returned by Glob `**/*.csproj`, plus every `*.props`, `*.targets`, `*.sln`, `*.fsproj`, `*.vbproj` file in the tree; no directory carved out except `docs/`, `.claude/`, `packages/`, `bin/`, `obj/` in the cross-check, which by rule contain no project files

Inclusion Rules: a member qualifies if it is a project-file element that selects a specific `FSharp.Core.dll` binary for compilation and copy-local, i.e. a `<HintPath>` whose value ends in `FSharp.Core.dll`, or a `<PackageReference Include="FSharp.Core">`

Exclusion Rules: excluded are `packages.config` entries (they select a package version, not a binary), `app.config` binding redirects (identity axis only), comments in scripts, the test-source constant naming the file, and coverage exclusion patterns

Primary Search Strategy or Query Expression: Grep pattern `FSharp\.Core` with glob `**/*.csproj`, content mode; 12 matching lines forming 6 `<Reference>`/`<HintPath>` pairs; zero `PackageReference` lines

Primary Member Set: `QuickFiler/QuickFiler.csproj:52`, `QuickFiler.Test/QuickFiler.Test.csproj:259`, `ToDoModel/ToDoModel.csproj:42`, `UtilitiesCS/UtilitiesCS.csproj:70`, `UtilitiesCS.Test/UtilitiesCS.Test.csproj:599`, `ToDoModel.Test/ToDoModel.Test.csproj:96`

Primary Count: 6

Cross-check Search Strategy or Query Expression: two queries that do not use the project-file glob: (a) Grep pattern `FSharp\.Core\.dll` over all files with glob `!{docs,.claude,packages,**/bin,**/obj}/**`, which returned the six HintPath lines plus one `.cs` constant excluded by rule; (b) Grep pattern `Include="FSharp\.Core` over the same scope in count mode, which returned six files at one occurrence each

Cross-check Member Set: `UtilitiesCS.Test/UtilitiesCS.Test.csproj:599`, `QuickFiler.Test/QuickFiler.Test.csproj:259`, `QuickFiler/QuickFiler.csproj:52`, `UtilitiesCS/UtilitiesCS.csproj:70`, `ToDoModel.Test/ToDoModel.Test.csproj:96`, `ToDoModel/ToDoModel.csproj:42`

Cross-check Count: 6

Member-set Comparison: after normalising order, the primary and cross-check sets are identical six-element sets; the count of six is confirmed, with a three/three split between `netstandard2.1` and `netstandard2.0`

### Claim 2 — exactly fifteen `bin/Debug` output directories receive `FSharp.Core.dll`

Complete Family: every project output directory for `Debug|Any CPU` that receives a copy of `FSharp.Core.dll` when `TaskMaster.sln` is built, whether from the project's own HintPath or transitively via `<ProjectReference>`

Exhaustive Search Scope: all 18 projects in `TaskMaster.sln` (`TaskMaster.sln:6-48`), their `<OutputPath>` declarations (all `bin\Debug\`), and all 41 `<ProjectReference Include=` edges across all 18 `.csproj` files

Inclusion Rules: a project qualifies if it has a direct FSharp.Core HintPath, or if the transitive closure of its `<ProjectReference>` edges reaches a project that has one

Exclusion Rules: excluded are `bin\Release`, `bin\x86\*` and `bin\x64\*` directories (not built by the mandated command), `obj/` intermediates, and projects whose reference closure contains no FSharp.Core-bearing project

Primary Search Strategy or Query Expression: Grep pattern `<ProjectReference Include=` with glob `**/*.csproj`, then manual closure over the resulting 41 edges unioned with the six direct projects from Claim 1

Primary Member Set: `QuickFiler`, `QuickFiler.Test`, `ToDoModel`, `UtilitiesCS`, `UtilitiesCS.Test`, `ToDoModel.Test`, `Tags`, `Tags.Test`, `VBFunctions.Test`, `TaskTree`, `TaskVisualization`, `TaskTree.Test`, `TaskVisualization.Test`, `TaskMaster`, `TaskMaster.Test`

Primary Count: 15

Cross-check Search Strategy or Query Expression: two independent observations that do not read project references at all: (a) Glob `*/bin/Debug/FSharp.Core.dll` against the primary checkout's built tree, returning fifteen files; (b) Grep pattern `assemblyIdentity name="(FSharp\.Core|netstandard)"` with glob `**/app.config`, returning fifteen files with a `FSharp.Core` identity (the auto-generated binding redirect that ResolveAssemblyReferences emits only for projects that see FSharp.Core in their closure)

Cross-check Member Set: (a) `QuickFiler`, `QuickFiler.Test`, `ToDoModel`, `Tags`, `Tags.Test`, `TaskMaster`, `TaskMaster.Test`, `TaskTree`, `TaskTree.Test`, `TaskVisualization`, `TaskVisualization.Test`, `ToDoModel.Test`, `UtilitiesCS`, `UtilitiesCS.Test`, `VBFunctions.Test`; (b) `VBFunctions.Test`, `Tags`, `QuickFiler`, `TaskTree`, `TaskVisualization.Test`, `TaskMaster`, `TaskVisualization`, `UtilitiesCS.Test`, `TaskTree.Test`, `Tags.Test`, `TaskMaster.Test`, `QuickFiler.Test`, `UtilitiesCS`, `ToDoModel.Test`, `ToDoModel`

Cross-check Count: 15 (both observations)

Member-set Comparison: after normalising order, the primary set and both cross-check sets are the same fifteen-element set; the complement within the 18 projects is exactly `SVGControl`, `SVGControl.Test`, `VBFunctions` in all three enumerations. The count of fifteen is confirmed.
