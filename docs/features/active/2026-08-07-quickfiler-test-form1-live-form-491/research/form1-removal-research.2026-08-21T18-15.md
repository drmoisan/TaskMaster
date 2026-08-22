---
issue: 491
epic: quickfiler-suite-determinism-foundation
created_at: 2026-08-21T18-15
status: research complete, no source changed
---

# Issue #491 — `QuickFiler.Test.Form1` live-form removal — research

All line numbers below were re-derived directly from the worktree at
`C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-a32345a9498cf124e` on 2026-08-21. No
source file was edited to produce this document.

## Verdict

`QuickFiler.Test.Form1` is **DEAD**: it has zero references anywhere in the tracked tree outside
its own three files and the four `QuickFiler.Test.csproj` entries that compile it. The correct
disposition is **removal** — delete `Form1.cs`, `Form1.Designer.cs`, `Form1.resx`, and their
`QuickFiler.Test.csproj` entries. The `.resx` carries no data entries and no
`ComponentResourceManager` consumer exists, so it is safe to remove alongside the two `.cs` files.
Removing the three files raises the measured line-coverage rate (removes 187 always-uncovered
lines from the denominator; the numerator is unaffected because those 187 lines currently
contribute 0 covered lines). Item 2 of the potential document (the three test-only `internal`
members of `ItemViewer.Breadcrumb.cs`) is **out of scope for this child**: it names a different
production file, the epic manifest scopes #491 to the live form only, and the epic's Hard
Constraint 5 area plus its "Recorded Preconditions" section explicitly prohibits any child of this
epic from writing under `docs/features/potential/**`, which further promotion of Item 2 would
require.

## A. Reachability

A repository-wide search for the identifier `Form1` (word-boundary pattern `\bForm1\b`) returns 82
files. Of those, only three are the type's own declaration files
(`QuickFiler.Test/Form1.cs`, `QuickFiler.Test/Form1.Designer.cs`) and its resource
(`QuickFiler.Test/Form1.resx`, matched via the csproj entry, not textually). The remaining hits
fall into four disjoint, non-overlapping categories, none of which reference
`QuickFiler.Test.Form1`:

1. Documentation and evidence prose (issue/potential/epic/research markdown files, and historic
   Cobertura XML evidence files under `docs/features/**/evidence/**`, which record the class by
   name as coverage data, not as a code reference).
2. `QuickFiler.Test/QuickFiler.Test.csproj` and `QuickFiler.Test/QuickFiler.Test.csproj.bak` (the
   four/five build-file entries; `.bak` is addressed under G).
3. **A different, unrelated `Form1` type**: `QuickFiler/Viewers/Form1.cs` and
   `QuickFiler/Viewers/Form1.Designer.cs`, in namespace `QuickFiler.Viewers` (production code, not
   test code). Its `Form1.cs` (read in full) is a four-line constructor-only partial class with no
   further members. It is a distinct type from `QuickFiler.Test.Form1` and out of scope for #491;
   it is noted here only because the broad `\bForm1\b` search surfaces it and a plan author must
   not confuse the two.
4. Two other test-project live forms of the same defect shape in unrelated projects
   (`UtilitiesCS.Test/Form1.cs`, `SVGControl.Test/Form1.cs`). These are separate defects in
   separate assemblies, not in scope for #491, and not addressed further here.

No test, no `[TestClass]`, no reflection-based discovery, and no resource lookup references
`QuickFiler.Test.Form1`. A targeted search for reflection patterns
(`Assembly.GetTypes`, `Activator.CreateInstance`, `GetType("`) inside `QuickFiler.Test/` returns
four call sites, none naming `Form1`:
`QuickFiler.Test/Viewers/BreadcrumbMessengerHubTests.cs:341`,
`QuickFiler.Test/Viewers/BreadcrumbSubfolderActivationTests.cs:402`, and two occurrences in
`QuickFiler.Test/Controllers/QfcItemController.FocusAndThemeTests.cs:128,177` — all of these
construct unrelated types (`Activator.CreateInstance(field.FieldType)` for a theme field, and a
message type in the breadcrumb hub tests), and none of the four is textually or semantically
connected to `Form1`.

The delegation prompt's fact 2 is confirmed independently: a direct search of
`QuickFiler.Test/Viewers/BreadcrumbCollapsedSurfaceReadinessTests.cs` for the literal `Form1`
returns zero hits (the file is absent from the 82-file `\bForm1\b` result set). The file is not a
`Form1` dependent. It is a caller of the potential document's Item 2 production surface: line 438
reads `=> Viewer.AttachBreadcrumbMessengerWhenReadyAsync(messenger, readiness);`, invoking the
`ItemViewer.Breadcrumb.cs` member named in Item 2. This confirms the delegation prompt's framing:
the file belongs to Item 2's caller set, not to Item 1 (`Form1`) at all.

**Verdict: DEAD.**

## B. Disposition

Given the DEAD verdict, deletion (option i) is correct, not a headless-construction retrofit
(option ii). A retrofit is a proportionate response only when a type has a legitimate reason to
exist under test — for example a production headless-viewer construction path (see the repository
memory precedent for `ItemViewer` headless construction). `Form1` has no such reason: it is not a
production type, it is not invoked by any test, and its own body
(`QuickFiler.Test/Form1.cs:22-34`, `LoadControlGroup`) exists solely to demonstrate manually adding
`ItemViewer` controls to a `TableLayoutPanel` at design time — a manual/visual harness, not a unit
test. Keeping it under a headless construction seam would still leave three files and roughly 190
lines of pure demonstration code inside the unit-test assembly, contradicting the epic's own
determinism leading indicator ("No unit-test run creates a visible window on the desktop") and the
repository's file-cohesion guidance in `.claude/rules/general-code-change.md`. Deletion is the
proportionate and reversible response (the file's git history remains available if a manual harness
is ever wanted, and the epic's Non-Goals section does not request one).

## C. The .resx coupling

Confirmed: `QuickFiler.Test/Form1.Designer.cs` (read in full, 227 lines) contains no
`ComponentResourceManager` and no `resources.ApplyResources` call anywhere in `InitializeComponent`
(lines 29-212) or elsewhere in the file. All control properties are set with literal values
(`System.Drawing.Point`, `System.Drawing.Size`, `System.Windows.Forms.Padding`, etc.), not via
resource lookup.

`QuickFiler.Test/Form1.resx` was read in full (120 lines). It contains only the standard ResX
schema boilerplate (`<xsd:schema>`, the two `<resheader>` elements for `resmimetype` and
`version`, and the `reader`/`writer` type-name `<resheader>` elements) and **zero `<data>`
elements**. The file carries no actual resource entries — it is an empty ResX shell, present only
because the WinForms designer always emits a sibling `.resx` for a `Form`-derived partial class,
regardless of whether any resource is used.

Consequently `Form1.resx` is orphaned in the sense that it has always been vestigial: no code reads
from it, and no code ever will, because there is nothing in it to read. Removing it breaks no
`ResourceManager` lookup and creates no satellite-assembly gap. A repository-wide search inside
`QuickFiler.Test/` for `ResourceManager`, `GetString(`, and `GetObject(` calls against a
`QuickFiler.Test`-scoped resource found none; the only `ResourceManager`-adjacent hit in the test
project is `QuickFiler.Test/ResourceTests.cs`, which belongs to a different project entirely
(`UtilitiesCS.Test`, per the earlier `\bForm1\b` file list) and is unrelated to `Form1.resx`.

## D. ItemGroup emptiness

Re-derived directly from `QuickFiler.Test/QuickFiler.Test.csproj` (CRLF file):

```
179	  <ItemGroup>
180	    <EmbeddedResource Include="Form1.resx">
181	      <DependentUpon>Form1.cs</DependentUpon>
182	    </EmbeddedResource>
183	  </ItemGroup>
```

Lines 180-182 (the `EmbeddedResource` element) are the sole child of the `<ItemGroup>` opened at
179 and closed at 183. MSBuild tolerates an empty `<ItemGroup>` — it is legal, inert XML with no
build effect — so there is no correctness requirement to remove the wrapper tags.

**Recommendation: delete the whole block, lines 179-183, not just 180-182.** Reasons:

1. Leaving an empty `<ItemGroup>` behind is dead structure with no purpose; the general repository
   guidance to keep files intentional and free of unused scaffolding applies to project files as
   much as to source files.
2. The deletion is scoped entirely inside the Form1 region the epic manifest assigns exclusively to
   #491 (see F below and the epic's Shared-Surface Coordination section). Deleting 179-183 does not
   touch any sibling child's entry: sibling #449 appends to the `Controllers` item group, which the
   re-derived csproj shows ends at line 178 (`</ItemGroup>` closing the item group that opens at
   line 57 and lists `<Compile Include="Controllers\...">`/`<Compile Include="Form1.cs">`/etc.
   entries). #449's append point (after line 178, inside or after that group) is unaffected by
   removing the wholly separate 179-183 `ItemGroup`.
3. Leaving 179 and 183 (empty tags) while removing only 180-182 gains nothing: the wrapper carries
   no attribute and no conditional logic, so there is no reason to preserve it "in case a future
   entry is added" — a future entry would simply re-open a new `<ItemGroup>` inline with the rest of
   the file's existing multi-`ItemGroup` structure (the file already has ten-plus separate
   `<ItemGroup>` blocks for compiles, resources, references, etc.), matching existing style.

## E. Coverage denominator

`coverage.config` (read in full, 25 lines) excludes only third-party module paths by regex:
`Deedle`, `FSharp`, `Castle\.Core`, `FluentAssertions`, `Moq`, `Microsoft\.Testing`, `MSTest`. It
contains no entry for `QuickFiler.Test` or any first-party assembly. `QuickFiler.Test.dll` is not
excluded from coverage instrumentation.

**Coverable-line counts for `QuickFiler.Test.Form1`, extracted from
`docs/features/active/2026-07-21-quickfiler-folder-selector-dropdown-400/evidence/baseline/diagnostic-quickfiler.2026-07-21T15-53.cobertura.xml`:**

The Cobertura file records `QuickFiler.Test.Form1` as two `<class>` elements (one per source file,
because the partial class spans two files): one at line 16154
(`filename=...\Form1.Designer.cs`) spanning to the closing `</class>` at line 16495, and one at
line 16496 (`filename=...\Form1.cs`) spanning to line 16585. Counting the distinct `<line
number="...">` entries in each class's summary `<lines>` block (lines 16332-16493 for the
Designer.cs class, lines 16554-16583 for the Form1.cs class — the class-level summary block, not
the duplicate per-method `<lines>` sub-blocks, to avoid double counting):

| File | Coverable lines | Covered lines | `hits="1"` count |
| --- | --- | --- | --- |
| `Form1.Designer.cs` | 157 | 0 | 0 |
| `Form1.cs` | 30 | 0 | 0 |
| **Total** | **187** | **0** | **0** |

Every single `<line>` entry under both classes carries `hits="0"`; the class-level `line-rate`
attributes confirm this independently (`line-rate="0"` on both classes, lines 16154 and 16496).

**Arithmetic — effect of removal on the measured rate.** This particular Cobertura file's root
element (line 2) records `lines-covered="21027"` and `lines-valid="84749"` (`line-rate=
0.24810912223153075`), and its `<packages>` element (grepped) shows twelve `<package>` entries
covering the whole solution plus several vendored/third-party assemblies (`log4net`,
`Mono.Reflection`, `System.Interactive`, `System.Linq.Async`, `Microsoft.IO.RecyclableMemoryStream`)
that the shipped harness's post-processing step (below) strips before the officially-reported
Koverage figure is produced. This file is therefore a **raw, pre-post-processing diagnostic
capture**, not the harness's final filtered artifact; per the repository memory on raw-vs-
postprocessed Cobertura root attributes, its root totals must not be compared numerically against a
Koverage-postprocessed root. The arithmetic below uses this single file's own root totals
consistently on both sides of the comparison (same file, same methodology), which is valid for
illustrating the *direction and approximate scale* of the effect; it is not a claim about the
harness's officially reported percentage.

- Before: `lines-covered = 21027`, `lines-valid = 84749` → rate `= 21027 / 84749 ≈ 0.248109`
- After removing Form1's 187 always-uncovered lines: `lines-covered' = 21027` (unchanged — Form1
  contributed 0 covered lines), `lines-valid' = 84749 − 187 = 84562` → rate
  `= 21027 / 84562 ≈ 0.248633`

**Removing Form1 raises the measured line-coverage rate.** The numerator is unaffected because
Form1's 187 lines were never covered; the denominator shrinks, so the ratio strictly increases.
This holds for any denominator scope (whole-repository, `QuickFiler.Test`-package-only, or a
first-party-only filtered scope) as long as the scope includes Form1's 187 lines before removal and
excludes them after — the direction of the effect does not depend on which of those scopes is used,
only the magnitude does.

**Exact commands for a numeric baseline and post-change comparison.** Read directly from
`scripts/vscode/Invoke-MSTestWithCoverage.ps1` (348 lines) and its sibling helper files:

```
pwsh -File scripts\vscode\Invoke-MSTestWithCoverage.ps1 `
  -SearchRoot . `
  -Configuration Debug `
  -CoverageOutput '<FEATURE>\evidence\baseline\coverage-baseline.cobertura.xml'
```

and, identically shaped, for the post-change capture:

```
pwsh -File scripts\vscode\Invoke-MSTestWithCoverage.ps1 `
  -SearchRoot . `
  -Configuration Debug `
  -CoverageOutput '<FEATURE>\evidence\qa-gates\coverage-postchange.cobertura.xml'
```

Real parameter names, confirmed from the script's `param()` block (lines 1-13) and the
`Invoke-MSTestWithCoverageMain` function signature (lines 248-259): `-SearchRoot`, `-Configuration`
(defaults to `Debug` when omitted or blank), `-CoverageOutput` (defaults to
`coverage\coverage.cobertura.xml`, repo-root-relative), and `-NoExecute` (a discovery-only switch
that returns before running collection — useful for a dry-run assembly-discovery check but produces
no XML). The script (lines 296-306) discovers test assemblies by recursively globbing
`*.Test.dll` under `\bin\$Configuration\`, excluding `\obj\` and `\ref\` paths; it does not by
itself exclude `.claude\worktrees\`, so per the epic's Execution Note 3 a plan invoking it directly
against the whole repository should scope `-SearchRoot` to avoid picking up stale worktree builds.
It resolves `vstest.console.exe` via `vswhere.exe`, requires the `dotnet-coverage` global tool, and
runs the collection through `Invoke-DotnetCoverageCollection` (lines 172-246 of the same file),
which composes the outer `dotnet-coverage collect --settings coverage.config` invocation together
with the inner `vstest.console.exe ... /InIsolation /TestCaseFilter:TestCategory!=LiveOutlook`
call, using the `TaskMaster.cli.runsettings` file resolved by `Resolve-RunSettingsPath` (lines
15-39).

**Two-denominator hazard.** After collection, the script (lines 333-341) explicitly
**post-processes** the raw Cobertura XML for "Koverage compatibility": it rewrites absolute paths
to workspace-relative paths, injects a `<sources>` element, and — critically —
**removes `<package>` elements for third-party assemblies not part of the solution** (dotnet-coverage
instruments every loaded DLL at runtime, including vendored/third-party code, which the raw capture
above shows: `log4net`, `Mono.Reflection`, `System.Interactive`, `System.Linq.Async`, and
`Microsoft.IO.RecyclableMemoryStream` all appear as `<package>` elements in the raw diagnostic file
used above). **The harness therefore emits the filtered, first-party-only figure as its final
output**, not the raw multi-package figure this research used for illustrative arithmetic. A plan
that captures a baseline and a post-change figure with this exact script will get two
directly-comparable filtered figures; it must not substitute a raw `dotnet-coverage collect` output
in place of one side of that comparison, and must not compare a filtered figure against an
unfiltered one.

`scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1` and
`scripts/vscode/Invoke-MSTestWithCoverage.ClosureFilter.ps1` exist alongside the main script (both
referenced by dot-sourcing at line 261 of the main script, `. (Join-Path $ScriptRoot
'Invoke-MSTestWithCoverage.Helpers.ps1')`); the helper file supplies `ConvertTo-KoverageCoberturaXml`
and `Assert-CoberturaLineCoverageThreshold`, called at lines 340-341 of the main script. Neither
file's internal implementation was needed to answer this question beyond confirming the
post-processing step exists and does what the main script's comments (lines 333-337) describe; a
plan author only needs the main script's command-line contract above.

## F. Item 2 scope boundary

`AttachBreadcrumbMessengerWhenReadyAsync`, `AttachBreadcrumbMessenger`, and `BreadcrumbOpenTask`
were re-verified present in `QuickFiler/Viewers/ItemViewer.Breadcrumb.cs`:
`BreadcrumbOpenTask` at line 29, `AttachBreadcrumbMessengerWhenReadyAsync` at line 100, and
`AttachBreadcrumbMessenger` at line 126 — all still `internal`. A repository-wide search for the
two method names (excluding the `Task<bool>` property, which is not directly greppable by a
distinct verb) inside `QuickFiler/` (production code only) returns exactly one file:
`QuickFiler/Viewers/ItemViewer.Breadcrumb.cs` itself (the declaration site). No other production
file under `QuickFiler/` calls any of the three members — they remain production-callerless,
confirming the potential document's Item 2 claim still holds.

**Recommendation: Item 2 does not belong in this child's (#491) scope.** Three independent reasons,
all confirmed directly rather than inferred:

1. **File-set boundary.** The epic manifest (`docs/features/epics/quickfiler-suite-determinism-foundation/epic.md`,
   Scope section, line 68-69) scopes #491 explicitly to "live form in the test project... `Form1.cs`
   and its designer." Item 2 touches `QuickFiler/Viewers/ItemViewer.Breadcrumb.cs`, a wholly
   different production file with no textual or structural overlap with `Form1`.
2. **Epic-level non-goal.** The epic's Non-Goals section states the `IItemViewer` UI-thread seam
   consolidation (#489), which rewrites `IItemViewer`, `ItemViewer.cs`, and
   `ItemViewer.WebViewThread.cs`, belongs to a later epic's ItemViewer child, not this one. Item 2's
   members live in a sibling partial-class file of the same `ItemViewer` type family
   (`ItemViewer.Breadcrumb.cs`), so a decision to promote them into the production call path is a
   design decision about `ItemViewer`'s breadcrumb-attach contract — squarely the kind of decision
   the epic reserves for the later ItemViewer-owning child, not for a determinism-cleanup child.
3. **Explicit write prohibition.** The epic's "Recorded Preconditions for Later Epics" section
   states plainly: "No child of this epic may write under `docs/features/potential/**`." The
   potential document's own Item 2 disposition options ("promote these members to the production
   attach path... or... mark them explicitly as test seams") both require either a code change to a
   file this child does not own, or a documentation update that would need to live under
   `docs/features/potential/**` (or an equivalent restricted location) to record the seam status
   formally — either path crosses a boundary this child is not permitted to cross.

The tradeoff: leaving Item 2 unaddressed means those ~40 lines of test-only production surface
persist without a resolution, and the underlying question ("should `AttachCollapsedMessenger`
route through the seam these members expose, or should the seam be documented as intentional test
infrastructure?") remains open. That is an acceptable and, per the epic's own explicit constraints,
a required deferral — not a gap introduced by this research. The orchestrator should route Item 2
to a separate issue in the later ItemViewer-owning epic (or, if urgency warrants, to a fifth
sibling issue outside this epic), rather than folding it into #491.

## G. Toolchain and build risk

**CSharpier.** `.csharpierignore` (read in full, 15 lines) excludes `*.csproj`, `*.props`, and
`*.targets` from the CSharpier check entirely — so the csproj edit removing the Form1 compile/
resource entries is invisible to `dotnet tool run csharpier check .`. The three deleted `.cs`/
`.resx` files simply cease to exist and drop out of the check's input set; CSharpier does not fail
on a file's absence. No `.csharpierignore` change is needed or implied.

**`.csproj.bak` and `TaskMaster.sln`.** `QuickFiler.Test/QuickFiler.Test.csproj.bak` also contains
`Form1` compile/resource entries (at its own line numbers 82-98, which differ from the live
`.csproj`'s 161-183 because the `.bak` predates the large test-file growth recorded in the live
project). `TaskMaster.sln` was searched for any project reference matching
`QuickFiler\.Test\.csproj` and returns exactly one hit: line 25, referencing
`QuickFiler.Test\QuickFiler.Test.csproj` (the live file). No solution entry names
`QuickFiler.Test.csproj.bak`. `.bak` files are not part of any MSBuild project or solution graph and
are not compiled by any of the four toolchain commands (CSharpier reads `*.cs`/`*.xml`/
`packages.config` only per `.csharpierignore`'s scope statement; MSBuild `/t:Rebuild` operates on
the solution's project references, which do not include `.bak`; `vstest.console.exe` runs built
test assemblies, not source). Confirmed: `.bak` presents no build risk and requires no edit as part
of this change, though a plan author may choose to delete it for hygiene — that is optional, not
required.

**Other `System.Windows.Forms` usage in `QuickFiler.Test`.** A search for `using
System.Windows.Forms;` inside `QuickFiler.Test/` returns 46 files, none of which is `Form1.cs` or
`Form1.Designer.cs` (both of which reference `System.Windows.Forms` via fully-qualified names
rather than a `using` directive, per the Designer.cs content read above, and `Form1.cs`'s own
`using` block, read above, lists only `System`). The 46 files span the `Viewers/`, `Controllers/`,
`TestSupport/`, and `Helper Classes/` directories and include, among others,
`QuickFiler.Test/TestSupport/WinFormsPumpHost.cs` (the sibling determinism child #511's subject) and
numerous breadcrumb/controller test files that construct or interact with WinForms controls under
test (e.g., via the pump host or direct control construction in isolated tests). This confirms the
project's `System.Windows.Forms` and `System.Drawing` assembly references are load-bearing far
beyond `Form1` and **must be retained** in `QuickFiler.Test.csproj`. Re-derived from the csproj
(lines 365-366, 420): `<Reference Include="System.Drawing" />`,
`<Reference Include="System.Drawing.Design" />`, and
`<Reference Include="System.Windows.Forms" />`. A plan must not propose removing any of these three
`<Reference>` entries.

## Open questions

- The exact officially-reported (Koverage-postprocessed, first-party-only) baseline percentage for
  `QuickFiler.Test` before this change was not captured in this research session — no toolchain
  command was run, per the researcher's hard constraint against running `msbuild`/`vstest`. A plan
  must capture that baseline itself using the exact `Invoke-MSTestWithCoverage.ps1` invocation
  documented under E before making the change, and a second, identically-shaped invocation after,
  to get a directly comparable pair of numbers on the harness's actual filtered denominator (as
  opposed to this research's illustrative raw-file arithmetic).
- Whether the maintainer wants `QuickFiler.Test/QuickFiler.Test.csproj.bak` deleted for hygiene
  alongside the live csproj edit is a judgment call left to the plan author; it carries no build
  risk either way (see G).
- Item 2's eventual disposition (promote to production call path vs. document as an intentional
  test seam) is unresolved and, per the analysis in F, is deliberately left unresolved by this
  research and by this child's scope.
