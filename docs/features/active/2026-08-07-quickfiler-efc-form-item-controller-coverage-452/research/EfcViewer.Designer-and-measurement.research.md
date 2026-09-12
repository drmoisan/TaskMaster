# F9 (#452) — Measurement and Build-Integration Research

- **Epic:** `quickfiler-per-file-coverage` (#136)
- **Child:** F9 `2026-08-07-quickfiler-efc-form-item-controller-coverage-452`
- **Scope of this artifact:** measurement machinery, coverage-harness contract, csproj edit mechanics,
  ledger obligations, evidence paths, toolchain commands. Code-level seam analysis for the four F9
  files is covered by sibling research artifacts and is **not** duplicated here.
- **Worktree:** `C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-a721e5b2426cc0b97`
- **Branch at time of research:** `TaskMaster-wt-2026-08-07T20-23` (clean, head `74be1964`)
- **Timestamp:** 2026-08-07T23-10

> **Tooling limitation disclosed.** No Bash/shell tool was available in this session. All findings
> below are from file reads and content searches against the worktree, plus one authenticated-free
> web fetch of GitHub issue #441. No command was executed. Every numeric claim below is derived by
> reading committed artifacts, not by running the toolchain.

---

## 1. F1's contract as F9 will actually consume it

### 1.1 What exists on this branch (verified)

| Expected artifact | Path | Present? |
| --- | --- | --- |
| F1 ledger | `docs/features/epics/quickfiler-per-file-coverage/coverage-ledger.md` | **ABSENT** |
| Anything else in the epic folder | `docs/features/epics/quickfiler-per-file-coverage/` | Only `epic.md` |
| F1 feature folder | `docs/features/active/quickfiler-coverage-*` | **ABSENT** |
| F1 per-file coverage harness (any script) | repo-wide search for `coverage-ledger`, `PerFileCoverage`, `Get-PerFileCoverage` | **ABSENT** — every hit is prose in child `spec.md`/`plan.md`/`research/*.md` files, never a script |
| Existing full-run coverage script | `scripts/vscode/Invoke-MSTestWithCoverage.ps1` | Present (349 lines) |
| Cobertura post-processor | `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1` | Present (357 lines) |

A `Glob` of `docs/features/epics/quickfiler-per-file-coverage/**` returns exactly one file:
`epic.md`. There is **no** F1 output of any kind on this branch. The epic manifest still carries the
placeholder `issue_num: 1001` for F1 (`epic.md:22-24`); the epic's own note at `epic.md:92-95` says
placeholders are back-filled from the promotion receipt as preparation completes, so F1's real issue
number (#432 per the delegation prompt) is not yet reflected in the manifest either.

**There is no per-file coverage report generator anywhere in the repository today.**
`Invoke-MSTestWithCoverage.ps1` produces a single whole-repository Cobertura XML. Nothing reads it
back out into a per-file table. That capability is entirely F1's to deliver.

### 1.2 F9's own folder state

`docs/features/active/2026-08-07-quickfiler-efc-form-item-controller-coverage-452/` contains
`spec.md`, `user-story.md`, `issue.md`, `plan.2026-08-07T22-35.md`. The `spec.md` is an unpopulated
template (all section bodies are placeholder prose), and `plan.2026-08-07T22-35.md` is a boilerplate
stub whose Phase 0 still references Python instruction files
(`plan.2026-08-07T22-35.md:31`). Neither is authored yet. There is no `research/` subfolder and no
`evidence/` subfolder yet.

### 1.3 Precedent for the halt gate

F3 (#430) already shipped the pattern F9 should copy. In
`docs/features/active/2026-08-07-quickfiler-keyboard-actions-coverage-430/plan.2026-08-07T20-41.md`:

- `:59` — `[P0-T4] Verify that F1's ledger docs/features/epics/quickfiler-per-file-coverage/coverage-ledger.md exists on the integration branch; halt and report BLOCKED if it is absent.`
- `:63` — `[P0-T6] Record F1's per-file coverage harness contract: its script path, its invocation form, whether it aggregates Cobertura entries by the <class> element's filename attribute or by class, whether it normalizes relative versus absolute filename forms, and how it reports a 0/0 file.`
- `:65` — `[P0-T7] Record the disposition of F1 dependency requirements D1-D4`

F8 (#437) states the same dependency in prose:
`docs/features/active/2026-08-07-quickfiler-efc-home-controller-coverage-437/spec.md:372-373` —
"F1's harness and ledger do not exist on disk at preparation time. The plan consumes them as an
upstream contract; F1 merges to the integration branch before F8 executes."

### 1.4 Recommended Phase 0 halt-gate conditions for F9

F9's plan should assert **all eight** of the following before any Phase 1 task runs. Any failure is
`BLOCKED`, not a workaround.

1. **G1 — Ledger exists.** `docs/features/epics/quickfiler-per-file-coverage/coverage-ledger.md`
   exists on the branch and is non-empty. Record its path and the commit sha that introduced it.
2. **G2 — Ledger covers all four F9 files.** The ledger contains a row keyed on each of
   `QuickFiler/Controllers/EfcFormController.cs`, `QuickFiler/Controllers/EfcItemController.cs`,
   `QuickFiler/Viewers/EfcViewer.cs`, `QuickFiler/Viewers/EfcViewer.Designer.cs`. Transcribe each row
   verbatim with its line citation.
3. **G3 — Attribute dispositions are explicit.** The ledger states, for each of the three files that
   currently carry `[ExcludeFromCodeCoverage]` (verified present at `EfcFormController.cs:27`,
   `EfcItemController.cs:25`, `EfcViewer.cs:20`), whether F9 must remove the attribute or whether the
   exemption is ratified. F9 must not self-grant or self-revoke an exemption.
4. **G4 — `EfcViewer.Designer.cs` disposition is stated *and* the mechanism is stated.** See §4.3:
   the Designer file carries no attribute of its own; its current exclusion is a side effect of the
   type-level attribute on the `EfcViewer` partial in `EfcViewer.cs:20`. The ledger must say both what
   bucket the Designer file is in **and** how that bucket is enforced once the type-level attribute is
   removed. Absent this, F9 cannot state its own acceptance criteria.
5. **G5 — Ledger states classification *rules*, not only rows.** Required by
   `epic.md:576-578` ("The ledger carries rules, not just rows"), because F9 creates new files during
   execution that post-date the ledger.
6. **G6 — Harness exists and its path is recorded.** The harness script exists at the path F1
   documents, and it runs to completion against a committed Cobertura XML.
7. **G7 — Harness contract is recorded, with four specific answers.** (a) `AGGREGATION_BASIS:
   filename` (not class, not type-name substring); (b) `LINE_SELECTION_AXIS:` the harness must state
   whether it uses the direct-child `class/lines/line` axis or the descendant `.//lines/line` axis —
   **the descendant axis is the #441 defect and must be rejected** (see §2); (c)
   `DENOMINATOR_BASIS: line-node-count` (not `@line-rate`); (d) `ZERO_OVER_ZERO_REPORTING: N/A` (not
   `0%`).
8. **G8 — Harness emits both rates.** The harness output states, per file, a line rate **and** a
   branch rate. `epic.md:189-192` and `epic.md:500-502` make these independent gates; F8 found
   `EfcHomeController.Timing.cs` at 100% line / 66.67% branch, and that value is verifiable in the
   committed report at `coverage-final.cobertura.xml:946`.

If G7(b) reveals that F1's harness reads `@line-rate` or uses the `.//` axis, F9's correct move is to
raise a defect against F1 (per the memory precedent: record dissent, do not fabricate a local
workaround that produces a second inconsistent number). §2.4 gives the exact defect text.

---

## 2. The existing coverage tooling, and issue #441

### 2.1 What produces the Cobertura report

`scripts/vscode/Invoke-MSTestWithCoverage.ps1` is the only sanctioned coverage entry point. Its
pipeline:

1. Resolves `vstest.console.exe` via `vswhere` (`:284-290`).
2. Requires `dotnet-coverage` on `PATH` (`:292-294`).
3. Discovers `*.Test.dll` recursively under `-SearchRoot`, filtered to `\bin\<Configuration>\`
   and excluding `\obj\` and `\ref\` (`:296-302`).
4. Writes a **derived** coverage settings file next to the output, adding the module exclusion
   `.*\.Test\.dll$` to the canonical `coverage.config` (`:99-113`, `:198-243`). The canonical
   `coverage.config` itself is never mutated; it excludes only Deedle, FSharp, Castle.Core,
   FluentAssertions, Moq, Microsoft.Testing, MSTest.
5. Invokes (`:70-77`):
   `dotnet-coverage collect --output <out> --output-format cobertura --settings <derived> -- <vstest.console.exe> <assemblies...> /Settings:<scripts/vscode/TaskMaster.cli.runsettings> /InIsolation /TestCaseFilter:TestCategory!=LiveOutlook`
6. Post-processes in place via `ConvertTo-KoverageCoberturaXml` (`:338-343`), which strips non-repo
   packages, relativizes `filename`, **merges `<class>` elements sharing a `filename`**, injects
   `<sources>`, and rewrites the root `line-rate`/`branch-rate`/`lines-covered`/`lines-valid`/
   `branches-covered`/`branches-valid`.

The wrapper form used by every sibling plan is:

```
pwsh -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug -CoverageOutput <out>
```

**There is no per-file line/branch command form.** The script emits one XML; per-file extraction is
F1's deliverable.

### 2.2 Issue #441 — verified, and it DOES affect F9

Fetched from `https://github.com/drmoisan/TaskMaster/issues/441` (open). Title:
*"Cobertura post-processing double-counts `<line>` nodes, inflating lines-valid and every coverage
rate."* Body cites `Get-CoberturaCoverageSummary` (helpers `:98`) selecting `.//lines/line`, and
`Merge-CoberturaClassesByFilename` (helpers `:167`) recomputing merged line-rate on the same path,
and notes a sample with `lines-valid="110849"` matching the raw count of all `<line>` elements.

**Structural confirmation.** A Cobertura `<class>` in this repo's output contains each covered line
**twice**: once under `<methods>/<method>/<lines>/<line>` and once in a class-level `<lines>` rollup.
Verified at
`docs/features/active/2026-08-06-quickfiler-high-confidence-queue-init-stall-424/evidence/qa-gates/coverage-final.cobertura.xml:308-324`
— `</methods>` at `:310` immediately followed by a class-level `<lines>` at `:311` repeating lines
20, 21, 22, 24 that had just appeared inside the method block, then continuing with lines the methods
do not carry (e.g. line 25, hits 0). The class-level rollup is a **superset** of the method-level
lines. So `.//lines/line` = rollup + method copies ≈ 2× the true denominator.

The committed report's root element carries `lines-valid="110849"` (`:2`), which is the doubled
figure.

### 2.3 The part that matters most for F9: per-file `@line-rate` is also contaminated

This is the finding with the largest impact on F9's numeric acceptance criteria, and it is not stated
in the epic.

`Merge-CoberturaClassesByFilename` (helpers `:167-292`) runs for any group of `<class>` elements
sharing one `filename` within a package (`:189-198`; groups of size 1 are skipped at `:191-193`). For
a merged group it:

- rebuilds the class-level `<lines>` as the union across the group, taking **max hits per line**
  (`:217-268`) — this part is correct and is exactly the epic's harness requirement #1; then
- **recomputes** `line-rate` and `branch-rate` for the merged node by calling the defective
  `Get-CoberturaCoverageSummary` on a synthetic document containing the merged node (`:270-276`).

Because the merged node retains the primary class's `<methods>` subtree, that recomputation counts
`method lines + class-level lines`. The written-back `@line-rate` and `@branch-rate` on every merged
class are therefore **wrong**.

**Proof by exact arithmetic.** `QuickFiler\Controllers\FilerQueue.cs`,
`coverage-final.cobertura.xml:18365-18480`, recorded as `line-rate="0.405797" branch-rate="0.428571"`:

- Class-level `<lines>` (`:18412-18479`): 49 distinct lines; 18 with `hits > 0`
  (16,17,18,20 / 32,33,34,38 / 40,42 / 70,71,72,73,74,75,76,78). **True line rate = 18/49 = 0.367347.**
- Method-level `<lines>` (`:18368-18410`): 20 lines; 10 covered.
- Defective sum: (18+10)/(49+20) = **28/69 = 0.4057971 → `0.405797`. Exact match to the recorded
  value.**
- Branch, class level: lines 25 (0/2), 34 (1/2), 48 (0/2), 74 (4/4) ⇒ 5/10. **True branch rate = 0.5.**
- Branch, method level: 25 (0/2), 34 (1/2) ⇒ 1/4.
- Defective sum: (5+1)/(10+4) = **6/14 = 0.428571. Exact match to the recorded value.**

Consequences:

1. **The epic's own baseline table is wrong for merged files.** `epic.md:160` lists
   `Controllers/FilerQueue.cs` at "69 lines, 40.6%". The true figures are 49 lines, 36.7% line and
   50.0% branch. Any child that plans against the epic table for a merged file is planning against
   an inflated number. F9 should flag this to the epic orchestrator.
2. **Line rate is inflated; branch rate can move either way.** FilerQueue's line rate was overstated
   (40.6 vs 36.7) while its branch rate was understated (42.9 vs 50.0). There is no safe direction.
3. **The defect is silent.** Nothing crashes; the numbers simply do not mean what they say.

**How to tell a contaminated value by inspection.** `Get-CoberturaCoverageSummary` rounds to six
decimals (`[math]::Round(..., 6)`, helpers `:137-138`). dotnet-coverage's native values are full
double precision. So a class whose `@line-rate`/`@branch-rate` shows 16 significant digits was
**not** merged and is native/correct; a value with ≤6 decimals was either rewritten or is
coincidentally short. In the QuickFiler package (`coverage-final.cobertura.xml:7-27898`, 71 files),
definitively-unmerged (16-digit) files are a small minority — `ItemViewerExpanded.Designer.cs`,
`BayesianPerformanceViewer.Designer.cs`, `BayesianPerformanceViewer.cs`,
`ToolStripMenuItemCb.Designer.cs`, `ToolStripMenuItemCb.cs`, `BreadcrumbOutboundQueue.cs`,
`EmailSorter.cs`, `QfcItemController.EventHandlers.cs`, `QfcItemGroup.cs`,
`EfcHomeController.Timing.cs`, `QfcThemeControlSet.cs`, `QfcItemController.cs`. The large majority
carry the ≤6-decimal rewrite signature.

### 2.4 What F9's plan must do

**The class-level `<lines>` in the post-processed XML is correct in every case.** For merged classes
it is the max-hits union rebuilt by `Merge-CoberturaClassesByFilename:217-268`; for unmerged classes
it is dotnet-coverage's native rollup. Only the `@line-rate` / `@branch-rate` **attributes** are
corrupt. That gives a clean, no-new-tooling escape route:

**Binding rule for F9 (assert in Phase 0, restate in the acceptance criteria):**

> F9's per-file line and branch numbers are computed from the **direct-child** axis
> `/coverage/packages/package/classes/class/lines/line`, grouped by the `class/@filename` attribute,
> de-duplicated by `@number` taking `max(@hits)`. The denominator is the count of distinct `<line>`
> nodes on that axis; a file with zero such nodes reports `N/A`, never `0%`. F9 **never** reads
> `class/@line-rate`, `class/@branch-rate`, or the root `coverage/@lines-valid` / `@line-rate`, and
> **never** uses the descendant axis `.//lines/line`. Branch figures are summed from
> `condition-coverage="(c/t)"` on direct-child lines where `@branch="True"`.

This rule is immune to #441 without modifying `Invoke-MSTestWithCoverage.Helpers.ps1` (which is a
shared file outside F9's assignment and must not be edited by this child).

**Residual imprecision to disclose, not fix.** For a merged class,
`Merge-CoberturaClassesByFilename:240-261` does not union the `<conditions>` across the group; it
picks the candidate line with the larger `Total` (ties broken by larger `Covered`). Branch figures on
merged files are therefore a best-of, not a true union. F9 should record this as a known limitation
in its evidence artifact rather than attempt a fix.

**Defect text if F1's harness is found to use the descendant axis or `@line-rate`:**

> F1 per-file coverage harness reads a value corrupted by open issue #441. Cobertura `<class>`
> elements in this repository carry each line twice (method-level and class-level rollup), and
> `Merge-CoberturaClassesByFilename` writes a recomputed `@line-rate`/`@branch-rate` derived from the
> doubled count. Verified against
> `docs/features/active/2026-08-06-quickfiler-high-confidence-queue-init-stall-424/evidence/qa-gates/coverage-final.cobertura.xml:18365-18480`,
> where `FilerQueue.cs` records `line-rate="0.405797"` (= 28/69) while its true class-level rate is
> 18/49 = 0.367347. The harness must select on the direct-child axis `class/lines/line` and derive
> both rates itself.

Note also that **repository-wide** figures for the epic's AC8 "retain or improve" gate must be
computed the same way and compared like-for-like. Comparing a corrected post-change figure against
the uncorrected `70.19%` merge-base figure cited at `epic.md:479-481` would be an invalid comparison.

---

## 3. The two harness correctness requirements, tested against real output

`epic.md:524-536` binds two requirements onto F1. Both are verified below against actual Cobertura
data.

### 3.1 Requirement 1 — aggregate per file, not per class, taking max hits per line

**Confirmed necessary, with direct evidence.** `FilerQueue.cs` demonstrably had **more than one**
`<class>` element sharing its `filename` before post-processing: `Merge-CoberturaClassesByFilename`
skips groups of size 1 (`helpers:191-193`), and FilerQueue's rate carries the rewrite signature and
matches the merge arithmetic exactly (§2.3). Structurally, its merged class-level `<lines>` contains
lines 45-65 and 70-78 (`:18441-18478`) that appear in **no** method of the primary class — those
lines came from the second `<class>` in the group.

**Direct evidence that the compiler generates such classes in this repo:**
`coverage-final.cobertura.xml:162923` — `<class name="TaskVisualization.FlagTasks.&lt;&gt;c" filename="TaskVisualization\FlagTasks.cs">`;
`:185839` — `<class name="TaskMaster.AppOlObjects.&lt;&gt;c__DisplayClass121_0" filename="TaskMaster\AppGlobals\AppOlObjects.StoreRehook.cs">`.
(Both survive as singletons because their enclosing types are excluded from coverage — which is
itself notable: `[ExcludeFromCodeCoverage]` on the outer type did **not** suppress the
compiler-generated closure class.)

### 3.2 Would F9's own files produce multiple `<class>` per filename? Yes.

Neither `EfcItemController.cs` nor `EfcFormController.cs` appears in any Cobertura report today
(§4), so this cannot be observed directly. It can be established from the source, which is
determinative — C# emits a `<>c` / `<>c__DisplayClassN_M` closure class per lambda-bearing type and a
`<M>d__N` state machine per `async` method, and the coverage tool attributes all of them to the
declaring source file.

`QuickFiler/Controllers/EfcItemController.cs` (1,170 lines) contains, among others:

- `:84`, `:160` — `_listTipsDetails.ForEach(x => x.Toggle(...))` (closure)
- `:110`, `:164` — `Task.Run(() => InitializeWebViewAsync())` (closure)
- `:207` — `internal async Task InitializeWebViewAsync()` (async state machine)
- `:257`, `:673` — `Buttons.ForEach(x => ...)` (closure)
- `:331`, `:337-338`, `:341`, `:344` — LINQ `Select`/`Where` chains (closures)
- `:704`, `:711`, `:716` — `async (x) => await ...` (async lambda: closure **and** state machine)

`QuickFiler/Controllers/EfcFormController.cs` (1,086 lines) contains:

- `:179`, `:211`, `:217`, `:219`, `:228` — LINQ `Select`/`Where`/`Cast` (closures)
- `:213` — `_listTipsDetails.ForEach(x => ...)` (closure)
- `:262` — `(x) => _themes[x].SetTheme(async: true)` (closure)
- `:415`, `:431`, `:447`, `:463`, `:523` — five `public async void ButtonX_Click` handlers (state machines)
- `:637` — `async (x) => await JumpToAsync(...)` (async lambda)

**Conclusion:** both files will produce multiple `<class>` elements sharing one `filename` the moment
their `[ExcludeFromCodeCoverage]` attributes are removed. If the harness reports only the first
`<class>`, F9's numbers will be materially understated — most damagingly for the `async void` button
handlers in `EfcFormController.cs`, whose entire bodies live in the state-machine class, not in the
named type.

### 3.3 Requirement 2 — decide the denominator on `<line>` child count, never `line-rate`

**Confirmed necessary, with a positive control.** `coverage-final.cobertura.xml:14426` records
`QuickFiler\Properties\Settings.Designer.cs` at `line-rate="0" branch-rate="1"` — a genuine 0%.
Separately, interface-only files emit **no `<class>` element at all**: none of the ~24 QuickFiler
interface-only files appears in the 71-file listing, while `QuickFiler\Interfaces\MailItemActionsAdapter.cs`
(`:14448`, `line-rate="1"`) does — proving the `Interfaces\` folder was instrumented and that absence
is a real signal, not a folder-level exclusion. A harness keyed on `@line-rate` cannot tell "0% of 8
lines" from "no lines at all"; a harness keyed on `<line>` child count can.

For F9 specifically, requirement 2 has no interface-only files to protect (all four F9 files have
executable content), but it is the mechanism by which the denominator is computed at all, and §2.4
depends on it.

---

## 4. The measured baseline for F9's files

### 4.1 The epic's claim, verified — with one correction to method

`epic.md:180-187` states that exempted files "do not appear in the report at all" and names
`EfcFormController.cs` and `EfcItemController.cs` (F9) explicitly, concluding "An absent file is not
a covered file."

**Verified and confirmed.** Enumerating every `filename` attribute in
`docs/features/active/2026-08-06-quickfiler-high-confidence-queue-init-stall-424/evidence/qa-gates/coverage-final.cobertura.xml`
returns 71 distinct QuickFiler files. **None** of the following appears:

- `QuickFiler\Controllers\EfcFormController.cs`
- `QuickFiler\Controllers\EfcItemController.cs`
- `QuickFiler\Viewers\EfcViewer.cs`
- `QuickFiler\Viewers\EfcViewer.Designer.cs`

By contrast, the sibling EFC family is present and well covered: `EfcHomeController.cs` (`:9`),
`.ExecuteMoves.cs` (`:597`), `.Metrics.cs` (`:805`), `.Timing.cs` (`:946`),
`EfcHomeControllerDependencyFactories.cs` (`:1040`), `EfcHomeControllerDependencies.cs` (`:1416`).
So the QuickFiler assembly and the `Controllers\` folder were both instrumented; the absence of F9's
files is attributable to the exemption, not to a tooling gap.

**Attribute presence verified in source:**

- `QuickFiler/Controllers/EfcFormController.cs:27` — `[ExcludeFromCodeCoverage]`
- `QuickFiler/Controllers/EfcItemController.cs:25` — `[ExcludeFromCodeCoverage]`
- `QuickFiler/Viewers/EfcViewer.cs:20` — `[ExcludeFromCodeCoverage]`
- `QuickFiler/Viewers/EfcViewer3.cs:17` — `[ExcludeFromCodeCoverage]` (see §4.2)
- `QuickFiler/Viewers/EfcViewer.Designer.cs` — **no attribute** (see §4.3)

**Baseline for F9 is therefore: unknown, and most plausibly near zero.** F9 must not cite any
number for its four files as a starting point; there is none.

### 4.2 `EfcViewer3.cs` is not in the denominator

`QuickFiler/Viewers/EfcViewer3.cs` carries `[ExcludeFromCodeCoverage]` but is **not** listed as a
`<Compile Include>` in `QuickFiler/QuickFiler.csproj`. Per `epic.md:576-578` the denominator is the
csproj compile set at evaluation time, so `EfcViewer3.cs` is out of scope for F9 and for the epic. It
is not one of the four assigned files and must not be touched.

### 4.3 Load-bearing finding: `EfcViewer.Designer.cs` is exempt only by inheritance

`QuickFiler/Viewers/EfcViewer.Designer.cs` (4,276 lines) carries **no** `[ExcludeFromCodeCoverage]`
attribute of its own. Verified: a search for the attribute across `QuickFiler/**/Efc*.cs` returns
exactly four hits, none in the Designer file. Its declaration is a bare
`partial class EfcViewer` (`EfcViewer.Designer.cs:7`).

The reason it is absent from the coverage report is that `[ExcludeFromCodeCoverage]` at
`EfcViewer.cs:20` is applied to the **type** (`public partial class EfcViewer : Form`,
`EfcViewer.cs:21`). Attributes on any partial declaration merge onto the single emitted type, and the
coverage collector excludes the whole type — including `InitializeComponent` and every line of the
Designer partial.

**Direct consequence for F9, which the epic does not state:** the moment F9 removes the attribute
from `EfcViewer.cs:20` in order to bring `EfcViewer.cs` (162 lines) into the denominator, **4,276
lines of generated Designer code enter the denominator with it**, in the same edit. There is no
type-level way to separate the two partials.

Three options, with the recommendation:

- **Option A (recommended) — ledger-only exemption, plus one constructing test.** The ledger
  classifies `EfcViewer.Designer.cs` as `ratified-exempt` on the `CLAUDE.md` §UT2 (b)
  "Designer-generated code" ground, and that classification — not an attribute — is what removes it
  from F9's per-file gate. Separately, at least one F9 test constructs `new EfcViewer()` headlessly on
  an STA thread, which executes `InitializeComponent` and drives the Designer file to near-100%,
  neutralizing the repository-wide impact. **Precedent is already in the report:**
  `ItemViewerExpanded.Designer.cs` sits at `line-rate="0.9950980392156863"`
  (`coverage-final.cobertura.xml:4112`) and `BayesianPerformanceViewer.Designer.cs` at
  `0.9914285714285714` (`:5683`) purely because their owning controls are constructed in tests. Note
  F3's plan already does exactly this construction for a different purpose
  (`plan.2026-08-07T20-41.md:92`, `new EfcViewer()` headless), which is evidence the construction is
  viable, but F9 must not rely on a sibling's test to cover its own file.
- **Option B — member-level attributes on the Designer partial.** `ExcludeFromCodeCoverageAttribute`
  is legal on methods, so `[ExcludeFromCodeCoverage]` on `InitializeComponent` and `Dispose` would
  preserve the exclusion precisely. Rejected: it requires editing generated code that Visual Studio
  regenerates, and `.claude/rules/general-unit-test.md` § Coverage Exclusion Policy pushes against
  adding excludes rather than refactoring.
- **Option C — leave `EfcViewer.cs` exempt.** Rejected: `epic.md:220-225` reads the "without an
  injectable seam" qualifier as a live obligation, and makes `[ExcludeFromCodeCoverage]` on a testable
  seam a Blocking finding.

**This is the single most important item to settle with F1 before F9 plans its acceptance criteria**
(halt gate G4 above). Getting it wrong risks a ~4,000-line, near-0% addition to the denominator,
which would breach the epic's AC8 "retain or improve" gate on its own.

### 4.4 An existing test file already targets `EfcFormController`

`QuickFiler.Test/QuickFiler.Test.csproj:101` already lists
`<Compile Include="Controllers\EfcFormControllerTests.cs" />`. F9's plan should read that file before
authoring anything, and must not assume a greenfield test surface. (Its content is a code-level
concern and belongs to the sibling seam researchers; flagged here only because it affects the
build-integration picture.)

---

## 5. `QuickFiler.csproj` edit mechanics

File: `C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-a721e5b2426cc0b97\QuickFiler\QuickFiler.csproj`.
Legacy non-SDK project, **no globbing** — every source file is an explicit `<Compile Include>`
(`epic.md:594-600`).

### 5.1 Exact XML shapes

**`Controllers\*.cs` — self-closing, single line, four-space indent, no child elements.** Verbatim
(`:294`, `:301`):

```xml
    <Compile Include="Controllers\EfcFormController.cs" />
    <Compile Include="Controllers\EfcItemController.cs" />
```

**`Viewers\EfcViewer.cs` — open/close pair with a `<SubType>` child.** Verbatim (`:386-391`),
including the adjacent Designer entry which has a `<DependentUpon>` child instead:

```xml
    <Compile Include="Viewers\EfcViewer.cs">
      <SubType>Form</SubType>
    </Compile>
    <Compile Include="Viewers\EfcViewer.Designer.cs">
      <DependentUpon>EfcViewer.cs</DependentUpon>
    </Compile>
```

**Partial-split precedent for a WinForms partial family** — `ItemViewer.*` (`:412-437`) uses both
children on every non-primary partial:

```xml
    <Compile Include="Viewers\ItemViewer.DisplayState.cs">
      <DependentUpon>ItemViewer.cs</DependentUpon>
      <SubType>UserControl</SubType>
    </Compile>
```

**Implication for F9's splits.** `EfcFormController.cs` and `EfcItemController.cs` live under
`Controllers\` and are **not** form-derived, so their split partials take the plain self-closing form
with no children:

```xml
    <Compile Include="Controllers\EfcItemController.<Aspect>.cs" />
```

If F9 splits `EfcViewer.cs` (162 lines — it is already well under the 500-line limit, so a split is
not required), any new partial would need `<DependentUpon>EfcViewer.cs</DependentUpon>` and
`<SubType>Form</SubType>` to match the `ItemViewer.*` precedent.

### 5.2 Line endings

**CRLF confirmed.** A content search for the regex `Compile Include="Viewers\\EfcViewer\.cs">\r$`
matches at `:386`, which is only possible if the line terminates with `\r\n`.

`epic.md:610-612` therefore applies verbatim: *"Preserve CRLF. The file is CRLF-terminated; a
git-bash `sed -i` will strip it and produce a whole-file diff that is guaranteed to conflict. Use the
Edit tool or `perl -0777` with explicit `\r\n`."*

**Safe edit technique for F9:** use the `Edit` tool with an `old_string` that is an exact copy of one
or two existing adjacent lines, and a `new_string` that repeats those lines plus the new entries. The
`Edit` tool performs an exact substring replacement and does not rewrite untouched bytes, so line
endings elsewhere in the file are preserved. Do **not** read-modify-write the whole file, do not use
`sed -i`, and do not run any formatter over `.csproj`.

### 5.3 Ordering — append-ordered within clusters, NOT alphabetical

Verified against the full `<Compile Include>` listing (`:290-461`). The Controllers block is broadly
alphabetical but demonstrably not sorted:

- `:290` `BayesianPerformanceController.cs`, `:291` `EfcDataModel.cs`, `:292` `BreadcrumbBridgeRouter.cs`,
  `:293` `BreadcrumbOutboundQueue.cs`, `:294` `EfcFormController.cs` — `EfcDataModel` precedes
  `Breadcrumb*`.
- `:311` `QfcCollectionController.cs` precedes `:312` `EmailSorter.cs`.
- `:339` `KeyboardHandler.cs` sits after `:338` `QfcItemGroup.cs`.
- `:341` `QfcQueue.cs` is the last Controllers entry.

New entries have historically been **appended adjacent to their related siblings** or at the end of
the folder block. F3's plan encodes the same reading
(`plan.2026-08-07T20-41.md:30-31`: entries added "adjacent to the existing `Interfaces\` block at
lines 358-368", kept as "minimal hunks").

**Recommendation for F9:** insert all new `Controllers\Efc*` partial-split entries as **one
contiguous block** immediately after `:301` (`Controllers\EfcItemController.cs`). This produces a
single small hunk in the middle of a region no other wave-1 child owns (F8 owns `EfcHomeController*`
at `:295-300`, which is adjacent — coordinate by keeping F9's hunk strictly *below* line 301). Do
not re-sort, do not touch any property group, do not touch references.

`epic.md:613-617` states that fan-in conflicts on this file are expected, additive on both sides, and
resolved by keeping both sets of entries. That is not a decomposition defect.

---

## 6. Ledger row obligation for F9's new files

`epic.md:560-587` ("Mid-Wave File Creation and the Ledger Denominator") names F9 explicitly at
`:570` as a child that creates production files (the `EfcFormController.cs` / `EfcItemController.cs`
500-line splits — both currently breach the limit at 1,086 and 1,170 lines).

Binding rules, restated:

1. `epic.md:579-582` — **"Creating child appends its own row.** Any child that adds a production file
   appends a ledger row for it **in the same change** that adds the `<Compile Include>` entry."
2. `epic.md:583-586` — **"New files default to `testable` at >= 90%.** A file extracted from existing
   code is new production code and takes the `CLAUDE.md` new-module target. Claiming
   `ratified-exempt` for a newly created file requires a rationale meeting one of the three grounds."
3. `epic.md:586-587` — F16 recomputes the denominator from the csproj and fails if any compiled file
   lacks a ledger row.

So each F9 partial-split file gets a row with bucket `testable` and target **>= 90% line**, not 80%.
The 80% figure applies only to the pre-existing files.

### 6.1 Provisional row format — FLAGGED PROVISIONAL, PENDING F1

**The ledger does not exist, so no format can be quoted.** Inferred from F1's brief
(`epic.md:318-324`, `:504-536`, `:576-587`) and from the fields sibling plans already demand:

```
| File | Bucket | Line target | Branch target | Owning child | Ground / rationale | Attribute |
| --- | --- | --- | --- | --- | --- | --- |
| `QuickFiler/Controllers/EfcItemController.<Aspect>.cs` | testable | >= 90% | >= 75% | F9 (#452) | New production file extracted from `EfcItemController.cs` under the 500-line rule; no COM/WinForms binding without a seam | none |
```

Required content per the epic, whatever the column layout turns out to be:

- **File path**, keyed the same way the harness keys results — i.e. matching the Cobertura
  `class/@filename` form. Note the report uses backslash-separated, repo-relative paths such as
  `QuickFiler\Controllers\EfcHomeController.cs` (`coverage-final.cobertura.xml:9`), which is **not**
  the forward-slash form used in `epic.md`. F1's harness must normalize; F9's row must use whichever
  form F1 settles on (halt gate G7).
- **Bucket**, from the three-value set fixed at `epic.md:519-521`: `testable`, `ratified-exempt`,
  `interface-only / not-measured`.
- **Line target** — `>= 90%` for F9-created files, `>= 80%` for pre-existing files
  (`epic.md:484-490`).
- **Branch target** — `>= 75%` (`epic.md:487`), an independent gate.
- **Owning child.**
- **Rationale**, required only for `ratified-exempt`, tested against one of the three grounds.
- **Attribute disposition** — F9's new files receive **no** `[ExcludeFromCodeCoverage]`.

**Naming inconsistency to resolve at G7.** Sibling plans have already invented three different labels
for the third bucket:
`docs/features/active/2026-08-07-quickfiler-helper-classes-coverage-434/plan.2026-08-07T20-41.md:577`
uses `no-coverable-lines`, `:580` uses `interface-only`, `:583` uses `no-executable-code`. The epic
text says `interface-only / not-measured`. F9 must use F1's literal token, not any of these, and
should flag the divergence if F1 has not reconciled it.

---

## 7. Evidence paths

**Confirmed canonical.** `.claude/skills/evidence-and-timestamp-conventions/SKILL.md:10-35` declares
itself the single source of truth and non-overridable. For F9 the numeric coverage results go to:

```
docs/features/active/2026-08-07-quickfiler-efc-form-item-controller-coverage-452/evidence/qa-gates/
```

Other canonical sub-paths for this child (`SKILL.md:14-20`):

- `.../evidence/baseline/` — Phase 0 baselines (format, analyzer, typecheck, coverage-baseline XML)
- `.../evidence/regression-testing/` — fail-before artifacts, per-phase scoped runs
- `.../evidence/other/` — decisions, dispositions, referrals to F1/the epic orchestrator
- `.../evidence/qa-gates/` — final toolchain gates and the per-file coverage table
- `.../evidence/issue-updates/` — issue-comment mirrors, named `issue-452.<timestamp>.md`
- `.../evidence/remediation-baseline/` — only if a remediation cycle runs

**Forbidden** (`SKILL.md:22-30`): `artifacts/baselines/`, `artifacts/baseline/`, `artifacts/qa/`,
`artifacts/qa-gates/`, `artifacts/evidence/`, `artifacts/coverage/`, `artifacts/regression-testing/`,
`artifacts/post-change/`. Only `artifacts/orchestration/` is permitted, and only for non-evidence
orchestration state.

**Timestamp format** (`SKILL.md:44-47`): `yyyy-MM-ddTHH-mm`, e.g. `2026-08-07T23-10`.

**Required fields in every machine-checkable evidence artifact** (`SKILL.md:106-118`):

- `Timestamp: <ISO-8601>`
- `Command: <exact command>`
- `EXIT_CODE: <int>`
- `Output Summary: <1-20 lines>` — **required** for anything under `evidence/baseline/`

**Additional fields F9's per-file coverage artifact must carry** (derived from §2 and §3, and from
F8's spec at `spec.md:414-421`):

- Per file: `LINE_COVERED / LINE_VALID`, the computed line rate, `BRANCH_COVERED / BRANCH_VALID`, the
  computed branch rate, and the ledger bucket. A `0/0` file reports `N/A`.
- `DERIVATION: class/lines/line direct-child axis, grouped by class/@filename, deduped by @number
  with max(@hits)` — the §2.4 disclosure, plus an explicit statement that `@line-rate` was not read.
- `ISSUE_441_DISCLOSURE:` a statement that the root `coverage/@lines-valid` and any `class/@line-rate`
  in the committed XML are inflated by open issue #441 and were not used.
- The branch-condition best-of limitation from §2.4.
- Source XML path and the branch/commit it was produced on.

**Negative claims** (`SKILL.md:132-140`) require `SearchScope:`, `SearchPatterns:`, `SearchResult:`.
This applies to F9's Phase 0 record that F1's outputs were absent at preparation time.

---

## 8. Toolchain command forms for this environment

### 8.1 Tool resolution — nothing relevant is on `PATH`

| Tool | Resolution | Evidence |
| --- | --- | --- |
| `msbuild` | via `vswhere`, **not** `PATH` | `scripts/vscode/Invoke-VSBuild.ps1:127-135` — `vswhere.exe` at `${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe`, then `-latest -requires Microsoft.Component.MSBuild -find 'MSBuild\**\Bin\MSBuild.exe'` |
| `vstest.console.exe` | via `vswhere`, **not** `PATH` | `Invoke-MSTestWithCoverage.ps1:284-290` — `-latest -products * -find Common7\IDE\Extensions\TestPlatform\vstest.console.exe` |
| `dotnet-coverage` | must be on `PATH`; script throws if absent | `Invoke-MSTestWithCoverage.ps1:292-294` — install with `dotnet tool install --global dotnet-coverage` |
| `csharpier` | local tool manifest, **not** `PATH` | `dotnet-tools.json:5-11` — version `1.2.6`, `rollForward: false` |
| `dotnet` SDK | repo-local `.dotnet-sdk/` preferred | `global.json:2-11` — `8.0.205`, `paths: [".dotnet-sdk", "$host$"]` |

**`.dotnet-sdk/` is NOT present in this worktree** (verified: `.dotnet-sdk/dotnet.exe` does not
exist). `global.json:10` carries the error message directing to
`./scripts/vscode/Install-RepoDotNetSdk.ps1`. F9's Phase 0 must include a bootstrap task; F3's plan
does exactly this at `plan.2026-08-07T20-41.md:67-68`.

**The tool manifest is at the repository root (`dotnet-tools.json`), not `.config/dotnet-tools.json`.**
Verified: a repo-wide glob for `**/dotnet-tools.json` returns only the root file. This is unusual and
worth confirming empirically during Phase 0 — if `dotnet tool run csharpier` fails to find the
manifest, run it from the repository root, or run `dotnet tool restore` from the root first.

### 8.2 The four toolchain stages, in mandatory order

`CLAUDE.md` § CUT3 fixes the order: format → analyze → type-check → test. If any stage fails or
auto-fixes, restart from stage 1.

**1. FORMAT**

```
dotnet tool run csharpier format .
```

**Do not use `csharpier .`.** `CLAUDE.md` § C#1 and § CUT3 state `csharpier .`, which is csharpier v0
syntax. The pinned version is **1.2.6** (`dotnet-tools.json:6`), whose CLI requires the `format`
subcommand. `.vscode/tasks.json:54-66` ("format: csharpier") uses
`dotnet tool run csharpier format .`, and F3's plan records the same at
`plan.2026-08-07T20-41.md:36,41`. Treat `.vscode/tasks.json` and the pinned version as authoritative
over the CLAUDE.md prose.

Non-mutating variant for baseline capture:

```
dotnet tool run csharpier check .
```

**2. ANALYZE (.NET analyzers)**

```
msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true
```

Environment-appropriate wrapper (resolves MSBuild via vswhere and syncs package HintPaths first):

```
pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-VSBuild.ps1 -SolutionPath TaskMaster.sln -Configuration Debug -Platform "Any CPU" -EnableNETAnalyzers -EnforceCodeStyleInBuild
```

(`Invoke-VSBuild.ps1:98-104` maps the switches to `/p:EnableNETAnalyzers=true` and
`/p:EnforceCodeStyleInBuild=true`; `:73` appends `/m`.)

**3. TYPE-CHECK (nullable, warnings as errors)**

```
msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true
```

Wrapper:

```
pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-VSBuild.ps1 -SolutionPath TaskMaster.sln -Configuration Debug -Platform "Any CPU" -EnableNullable -TreatWarningsAsErrors
```

**4. TEST with coverage**

Full run, producing the Cobertura XML that F1's harness consumes:

```
pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug -CoverageOutput docs/features/active/2026-08-07-quickfiler-efc-form-item-controller-coverage-452/evidence/qa-gates/coverage-final.cobertura.xml
```

Scoped run during iteration (no coverage, fast):

```
& (vswhere -latest -products * -find Common7\IDE\Extensions\TestPlatform\vstest.console.exe) QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation /TestCaseFilter:"FullyQualifiedName~<Filter>"
```

### 8.3 Concrete `QuickFiler.Test` assembly path

`QuickFiler.Test/QuickFiler.Test.csproj` sets `<AssemblyName>QuickFiler.Test</AssemblyName>` (`:17`),
`<OutputType>Library</OutputType>` (`:14`), `<TargetFrameworkVersion>v4.8.1</TargetFrameworkVersion>`
(`:18`), and `<OutputPath>bin\Debug\</OutputPath>` for the Debug|Any CPU configuration (`:36`).
The project is in the solution at `TaskMaster.sln:25`.

Debug|Any CPU output path (absolute, this worktree):

```
C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-a721e5b2426cc0b97\QuickFiler.Test\bin\Debug\QuickFiler.Test.dll
```

Repo-relative: `QuickFiler.Test\bin\Debug\QuickFiler.Test.dll`.
(An x86 configuration also exists at `bin\x86\Debug\`, `:51`; the epic's plans all use Any CPU.)

### 8.4 Two operational hazards

**Stale agent-worktree assemblies.** `Invoke-MSTestWithCoverage.ps1:296-302` discovers `*.Test.dll`
recursively from `$repoRoot`, where `$repoRoot` is resolved two levels above the script
(`:271`). When F9 runs from **inside this worktree**, `$repoRoot` is the worktree root and no nested
`.claude/worktrees/` exists, so discovery is clean. If any run is ever launched from the main
checkout, sibling agent worktrees under `.claude/worktrees/` are picked up and produce spurious
assembly-initializer failures. F3's plan encodes the mitigation at
`plan.2026-08-07T20-41.md:36`: filter out candidates under the `.claude/worktrees` subtree. F9 should
carry the same note.

**`/InIsolation` is mandatory.** Both the script (`:76`) and F3's plan (`:36`) apply it; the
Moq-based assemblies are unreliable without it. The script also applies
`/TestCaseFilter:TestCategory!=LiveOutlook` (`:76`), which excludes live-Outlook tests from every
coverage run — relevant because F9's files are COM-bound and any test the plan marks `LiveOutlook`
will silently not contribute to the measured coverage.

---

## 9. Summary of what F9's plan must carry

1. **Phase 0 halt gate** with the eight conditions G1-G8 in §1.4, each producing an evidence artifact
   under `evidence/baseline/` or `evidence/other/`. G4 (the `EfcViewer.Designer.cs` mechanism) is the
   one most likely to be missing from F1's first draft.
2. **A binding derivation rule** (§2.4) that computes per-file line and branch rates from the
   direct-child `class/lines/line` axis grouped by `@filename`, never from `@line-rate` and never from
   the `.//` descendant axis, with an explicit issue-#441 disclosure in the evidence artifact.
3. **A defect report to F1** if F1's harness is found to read `@line-rate` or use the descendant axis
   (exact text in §2.4), plus a correction note to the epic orchestrator that the
   "Measured Coverage Baseline" table at `epic.md:155-178` is inflated for merged files
   (`FilerQueue.cs` is 36.7% / 50.0%, not 40.6%).
4. **A design decision recorded before any code change** on `EfcViewer.Designer.cs` — Option A
   (ledger-only exemption plus one headless `new EfcViewer()` construction) is recommended, with
   `ItemViewerExpanded.Designer.cs` at 99.5% as the precedent that the mechanism works.
5. **A `QuickFiler.csproj` edit** confined to a single contiguous block of self-closing
   `<Compile Include="Controllers\..." />` entries inserted immediately after line 301, made with the
   `Edit` tool to preserve CRLF, with no reordering and no property or reference change.
6. **A ledger row per created file** appended in the same change as the csproj entry, bucket
   `testable`, line target **>= 90%**, branch target **>= 75%**, no `[ExcludeFromCodeCoverage]`,
   using F1's literal bucket token (format above is provisional pending F1).
7. **All evidence under** `docs/features/active/2026-08-07-quickfiler-efc-form-item-controller-coverage-452/evidence/<kind>/`
   with `Timestamp:` / `Command:` / `EXIT_CODE:` (and `Output Summary:` for baselines).
8. **The toolchain commands in §8**, noting csharpier's `format` subcommand, the absent
   `.dotnet-sdk/`, and that msbuild and vstest resolve via vswhere rather than `PATH`.
