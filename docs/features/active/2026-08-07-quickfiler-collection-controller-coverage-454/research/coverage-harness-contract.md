# Research — Coverage-Measurement Contract (Issue #454, epic #136 child F11)

- Date: 2026-08-07
- Scope: **measurement only.** The partial-split design, seam inventory, existing-test inventory,
  latent defects, `InternalsVisibleTo` situation, and STA infrastructure are covered by the two
  sibling artifacts in this folder and are taken as given here:
  - `docs/features/active/2026-08-07-quickfiler-collection-controller-coverage-454/research/qfc-collection-controller.md`
  - `docs/features/active/2026-08-07-quickfiler-collection-controller-coverage-454/research/iqfc-collection-controller.md`
- Method: every claim is cited to `file:line` from a direct read of the worktree. All paths are
  repo-relative. Cobertura citations are line numbers **within the XML file**.
- No production or test file was modified.

---

## 0. Verdict summary

| Question | Verdict |
| --- | --- |
| A. Can per-file line/branch rates be obtained for a partial-class family? | **YES.** Cobertura emits one `<class>` element per `(type, source file)` pair. Verified on two existing partial families. |
| A'. Is the `line-rate` **attribute** on those elements trustworthy? | **NO.** For any file containing an async method or a lambda, the attribute is inflated by a defect in `Merge-CoberturaClassesByFilename`. Rates must be **recomputed** from the class-level `<lines>` children. |
| B. Does the harness produce a per-file line/branch rate today? | **NO.** It produces the Cobertura XML only. No shipped script reports per-file rates correctly; `scripts/temp-extract-coverage.ps1` reads the untrustworthy attribute and is hard-coded to `UtilitiesCS`. |
| B'. Do issue #441's two code citations hold? | **YES**, both, exactly. The double-count claim also holds exactly (`110849`). |
| B''. Are epic.md's two required corrections implemented? | **Correction 1 (union per filename, max hits): partially — implemented for `<lines>` but the rate arithmetic that consumes it is wrong. Correction 2 (denominator on `<line>` count, not `line-rate`): NOT implemented.** |
| C. Is branch data emitted? | **YES**, fully — `branch`, `condition-coverage`, and `<conditions>` on every line; per-file branch rate is obtainable by the same recomputation. |
| D. Does CI filter `.claude/worktrees`? | **NO**, and neither does the local harness. CI also emits no Cobertura at all. |
| E. Do F1's artifacts exist on this branch? | **NO** (expected; not a blocker). A non-halting gate formulation is given in §E. |
| F. Does any config exclude QuickFiler from instrumentation? | **NO.** `[ExcludeFromCodeCoverage]` is the only mechanism keeping the file out. ClassLevel parallelism **is** a determinism hazard for the static counter. |

---

## A. Per-file attribution for a partial-class family

### A.1 The report under examination

`docs/features/active/2026-08-06-quickfiler-high-confidence-queue-init-stall-424/evidence/qa-gates/coverage-final.cobertura.xml`

- Header: `coverage-final.cobertura.xml:2` —
  `line-rate="0.856453" branch-rate="0.790039" lines-covered="94937" lines-valid="110849" branches-covered="22001" branches-valid="27848"`.
- It has been post-processed: `<sources><source>.</source></sources>` at `:3-5` (injected by
  `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1:330-339`) and filenames are repo-relative
  (`coverage-final.cobertura.xml:9`). The report therefore reflects the **output** of
  `ConvertTo-KoverageCoberturaXml`, including `Merge-CoberturaClassesByFilename`.
- It contains 534 `<class>` elements (count over `^        <class ` in the file).

### A.2 `<class>` elements per partial family — one per source file

**`QfcItemController` — 10 partials, 10 `<class>` elements, 10 distinct `filename` values:**

| XML line | `filename` | `line-rate` attr | `branch-rate` attr |
| --- | --- | --- | --- |
| `coverage-final.cobertura.xml:22740` | `QuickFiler\Controllers\QfcItemController.cs` | `1` | `0.7857142857142857` |
| `:23126` | `...\QfcItemController.Initialization.cs` | `0.901099` | `0.961538` |
| `:23519` | `...\QfcItemController.ViewerSetup.cs` | `0.743682` | `0.56` |
| `:24004` | `...\QfcItemController.Conversation.cs` | `0.911765` | `0.961538` |
| `:24222` | `...\QfcItemController.FolderHandling.cs` | `0.896861` | `0.686275` |
| `:24601` | `...\QfcItemController.EventWiring.cs` | `0.81993` | `0.65625` |
| `:25411` | `...\QfcItemController.EventHandlers.cs` | `0.7956989247311828` | `0.65` |
| `:25754` | `...\QfcItemController.Navigation.cs` | `0.89071` | `0.766667` |
| `:26058` | `...\QfcItemController.FocusAndTheme.cs` | `0.756032` | `0.576087` |
| `:26662` | `...\QfcItemController.MailActions.cs` | `0.777778` | `0.75` |

Every one carries `name="QuickFiler.Controllers.QfcItemController"` — the same type name, ten times,
distinguished only by `filename`.

**`QfcHomeController` — 3 partials, 3 `<class>` elements:**

| XML line | `filename` | `line-rate` attr | `branch-rate` attr |
| --- | --- | --- | --- |
| `coverage-final.cobertura.xml:21643` | `QuickFiler\Controllers\QfcHomeController.cs` | `0.713924` | `0.51` |
| `:22314` | `...\QfcHomeController.Metrics.cs` | `0.650943` | `0.625` |
| `:22612` | `...\QfcHomeController.Iteration.cs` | `0.8625` | `0.666667` |

**Answer to the three sub-questions:**

1. **How many `<class>` elements carry the type name, and what distinct `filename` values?**
   `QfcItemController`: 10 elements, 10 distinct filenames (one per partial source file).
   `QfcHomeController`: 3 elements, 3 distinct filenames. No partial file collapses into another.

2. **Does each partial source file appear as its own `filename`?** Yes. There is a 1:1 mapping
   between compiled partial `.cs` files and `<class>` elements for the type. No collapse.

3. **Are `<method>` elements grouped under one `<class>` or split across several?** **Split.** Each
   `<class>` element carries only the methods whose IL maps to that file. The instrumenter goes
   further and splits a *single method* across files when its lines span partials: the
   `QfcHomeController` constructor appears as `<method name=".ctor" signature="()">` under **both**
   `QfcHomeController.cs` (`:21645`, first line `number="30"`) **and** `QfcHomeController.Metrics.cs`
   (`:22316`, its only line is `number="17"` — the field initializer declared in the Metrics
   partial). Per-file attribution is therefore *finer* than per-method, which is exactly what F11
   needs.

### A.3 Compiler-generated companion classes sharing a `filename`

Only **two** `<class>` elements in the whole report still carry a compiler-generated name:

- `coverage-final.cobertura.xml:162923` — `name="TaskVisualization.FlagTasks.&lt;&gt;c" filename="TaskVisualization\FlagTasks.cs"`
- `coverage-final.cobertura.xml:185839` — `name="TaskMaster.AppOlObjects.&lt;&gt;c__DisplayClass121_0" filename="TaskMaster\AppGlobals\AppOlObjects.StoreRehook.cs"`

Both survive precisely because they are the *only* class element for their filename (their primary
type is exempted or otherwise absent), so `Merge-CoberturaClassesByFilename` skipped them
(`Invoke-MSTestWithCoverage.Helpers.ps1:191`, `if ($group.Count -le 1) { continue }`). Every other
compiler-generated companion has already been folded into its primary node and removed
(`Invoke-MSTestWithCoverage.Helpers.ps1:285-289`).

**Concrete example of a merge that happened, reconstructed arithmetically.** Take
`QuickFiler\Controllers\QfcHomeController.Iteration.cs` (`coverage-final.cobertura.xml:22612`):

- Its `<methods>` block (`:22613-22659`) contains exactly three methods — `Iterate` (12 lines,
  `:22616-22636`), `Iterate2` (7 lines, `:22641-22647`), `SwapStopWatch` (5 lines, `:22652-22656`)
  — **24 line elements, all `hits="1"`**, covering source lines 56-84.
- Its class-level `<lines>` block (`:22660-22738`) contains **56** line elements covering source
  lines 12-84, of which **11** have `hits="0"` (source lines 38, 39, 41, 42, 43, 44, 45, 47, 49, 50,
  52 — `coverage-final.cobertura.xml:22689-22703`).
- Source lines 12-53 are the body of `public async Task IterateQueueAsync()`
  (`QuickFiler/Controllers/QfcHomeController.Iteration.cs:11-53`). An `async` method compiles to a
  separate `<IterateQueueAsync>d__N` state-machine class. Pre-merge that was a **second `<class>`
  element sharing the same `filename`**. The merge unioned its class-level lines into the primary
  node and deleted it.

This is the concrete instance of epic.md's "aggregate per file, not per class" directive
(`docs/features/epics/quickfiler-per-file-coverage/epic.md:529-532`). It matters enormously for F11:
`QfcCollectionController` is async-heavy (§ sibling research lists ~30 `async` members), so nearly
every new partial will have one or more state-machine companion classes.

### A.4 The `line-rate` attribute on a merged class is WRONG — verified arithmetically

`Merge-CoberturaClassesByFilename` clones the primary node (`Invoke-MSTestWithCoverage.Helpers.ps1:200`),
**clears and rebuilds only the class-level `<lines>`** as the union across the group with max hits
(`:208-215`, `:217-268`), but **never merges the other group members' `<methods>`** — line 202-206
only ensures a `<methods>` element exists. It then recomputes `line-rate`/`branch-rate` by running
`Get-CoberturaCoverageSummary` over the merged node (`:270-276`), and that function selects
`.//lines/line` (`:122`), i.e. **both** the surviving primary-only `<methods>` subtree **and** the
correctly-unioned class-level `<lines>`.

Result: the primary type's own lines are counted twice; the companion class's lines once.

Verification against `QfcHomeController.Iteration.cs`:

| Quantity | Value |
| --- | --- |
| Class-level `<lines>` (the true per-file set) | 56 total, 45 covered → **80.36%** |
| Primary `<methods>` subtree | 24 total, 24 covered |
| Harness sum (`.//lines/line`) | 80 total, 69 covered → **86.25%** |
| Attribute actually written at `coverage-final.cobertura.xml:22612` | `line-rate="0.8625"` |

`69/80 = 0.8625` matches the emitted attribute exactly. The attribute **overstates the true per-file
line rate by 5.9 points** for this file.

Independent cross-check from #424's own evidence: `coverage-delta.2026-08-07T00-48.md:10` records
that its author used ad-hoc scripts with "per-line dedup by `(filename, line number)`, because
Cobertura repeats each line under both `<method><lines>` and the class-level `<lines>`", and reports
`QfcHomeController.cs` post-change at **68.40% (171/250)**
(`coverage-delta.2026-08-07T00-48.md:41`). The attribute for that same file in the same report is
`line-rate="0.713924"` = `282/395` (`coverage-final.cobertura.xml:21643`). Note `145 + 250 = 395` and
`111 + 171 = 282`: the deduped figure is exactly the class-level `<lines>` set, and the attribute is
exactly the blended figure. Two independent derivations agree.

**Consequence for epic.md's baseline table.** The table at
`docs/features/epics/quickfiler-per-file-coverage/epic.md:155-178` was built from these attributes.
Its `Lines` column is the double-counted total (e.g. `QfcHomeController.Metrics.cs | 212` = 2 x 106
real lines) and its `Line %` column is overstated for every file that has a companion class (e.g.
`QfcHomeController.cs | 395 | 71.4%` versus the true `250 | 68.4%`). The epic already labels the
table "indicative, not authoritative" (`epic.md:142-145`); this section explains *why* it is not
authoritative and by roughly how much.

### A.5 UNAMBIGUOUS VERDICT

**Yes — per-file line and branch rates are fully obtainable for a partial-class family from this
pipeline, but only by recomputation. Never read the `line-rate` / `branch-rate` attribute.**

The recipe F11's plan must encode (applies to any file, partial or not):

1. Load the post-processed Cobertura XML.
2. Select **all** `<class>` elements whose `@filename` equals the target file path (case-insensitive,
   `\`-separated, repo-relative). Post-merge there is normally exactly one per package; select all,
   because the merge is scoped per `<package>` (`Invoke-MSTestWithCoverage.Helpers.ps1:174`).
3. Union their **`./lines/line` children only** — the class-level block. **Exclude
   `./methods//lines/line` entirely.** Key the union on `@number`, taking `MAX(@hits)`.
4. `line rate = |{ line : hits > 0 }| / |lines|`. If `|lines| == 0`, report **N/A**, not 0%
   (epic.md:533-536, and the `interface-only / not-measured` bucket at epic.md:509-522).
5. For branch: over the same unioned set, take lines with `@branch="True"`, parse
   `@condition-coverage` with `\((\d+)/(\d+)\)`, and sum. `branch rate = sum(covered)/sum(total)`.
   When two class elements report the same line number with different condition counts, keep the one
   with the larger `total` (the same tie-break the harness already uses at
   `Invoke-MSTestWithCoverage.Helpers.ps1:240-243`). If `sum(total) == 0`, report **N/A**.

Step 3's "class-level only" rule is the single load-bearing correction. It simultaneously fixes the
double-count and the merge blend, because the merge already wrote the correct union into the
class-level block.

**No restructuring of the plan is required.** The per-file acceptance criteria are verifiable.

---

## B. The harness as it exists today

### B.1 Invocation signature

`scripts/vscode/Invoke-MSTestWithCoverage.ps1:1-13`:

```
param(
    [string]$SearchRoot,          # default '.'  (resolved relative to repo root, :263-272)
    [string]$Configuration,       # default 'Debug'                                (:267-269)
    [string]$CoverageOutput = "coverage\coverage.cobertura.xml",
    [switch]$NoExecute
)
```

The repo-standard invocation is `.vscode/tasks.json:190-206` (task
`test: MSTest with Coverage (Koverage)`), which passes `-SearchRoot .` and `-Configuration Debug`
and depends on `build: TaskMaster.sln (VS MSBuild)`.

Execution path:
- Assembly discovery: `Invoke-MSTestWithCoverage.ps1:296-302` — recursive `*.Test.dll` under the
  search root, kept only if the path matches `\bin\<Configuration>\` and does not match `\obj\` or
  `\ref\`.
- `vstest.console.exe` located via `vswhere` (`:279-290`); `dotnet-coverage` required on PATH (`:292-294`).
- Outer command shape: `dotnet-coverage collect --output <path> --output-format cobertura --settings <derived config> -- <vstest> <assemblies...> /Settings:scripts/vscode/TaskMaster.cli.runsettings /InIsolation /TestCaseFilter:TestCategory!=LiveOutlook`
  (`Invoke-MSTestWithCoverage.ps1:70-77`).
- The inner runsettings is deliberately the **CLI** one
  (`scripts/vscode/TaskMaster.cli.runsettings`), which carries MSTest parallelization only and **no**
  Code Coverage data collector, so instrumentation comes solely from the outer
  `--settings coverage.config` path (`Invoke-MSTestWithCoverage.ps1:19-26`). The repo-root
  `TaskMaster.runsettings` is used by Visual Studio only, not by this script.
- A derived settings file adding `.*\.Test\.dll$` to the module excludes is written next to the
  output and deleted in `finally` (`:79-116`, `:198-242`).
- Post-processing: `ConvertTo-KoverageCoberturaXml` (`:338-343`).

### B.2 Where Cobertura output is written

`Join-Path $repoRoot $CoverageOutput` (`Invoke-MSTestWithCoverage.ps1:308`), i.e. by default
`coverage/coverage.cobertura.xml` at the repo root. The directory is created if absent (`:310-312`).
The file is overwritten in place by the post-processor (`:342`).

### B.3 How a caller obtains a per-file rate today

**It cannot, correctly.** The harness's entire output is the XML. There is no per-file reporting
function in either script — `Invoke-MSTestWithCoverage.Helpers.ps1` exposes
`Get-KoverageProjectAllowlist`, `ConvertTo-KoverageRelativePath`, `Get-CoberturaCoverageSummary`,
`Get-CoberturaLineConditionCoverageParts`, `Merge-CoberturaClassesByFilename`, and
`ConvertTo-KoverageCoberturaXml`; none reports per file.

The only per-file extractor in the repository is `scripts/temp-extract-coverage.ps1`, and it is not
usable here:
- it filters to `$_.name -eq 'UtilitiesCS'` (`scripts/temp-extract-coverage.ps1:7`);
- it reads `[double]$c.'line-rate'` (`:13`) — the attribute proven wrong in §A.4;
- it reports no branch rate at all;
- its `$OutputPath` default targets a 2026-03-19 feature folder (`:3`);
- its name marks it as temporary and it hard-codes a difficulty taxonomy by regex (`:27-33`).

Do not reuse it. Do not extend it.

### B.4 Verification of issue #441's citations

The issue body itself could not be read in this session (no shell or `gh` access to this agent, and
no in-repo reference to #441 exists — a repo-wide grep finds only unrelated numeric matches). The
two code citations were verified directly against the files.

| #441 claim | Verdict | Evidence |
| --- | --- | --- |
| `Get-CoberturaCoverageSummary` is at `Invoke-MSTestWithCoverage.Helpers.ps1:98` | **HOLDS** | `function Get-CoberturaCoverageSummary {` is at `Invoke-MSTestWithCoverage.Helpers.ps1:98` |
| It selects `.//lines/line` | **HOLDS** | `foreach ($line in $cls.SelectNodes('.//lines/line'))` at `Invoke-MSTestWithCoverage.Helpers.ps1:122`, inside `foreach ($cls in $pkg.SelectNodes('.//class'))` at `:121` |
| `Merge-CoberturaClassesByFilename` is at `:167` | **HOLDS** | `function Merge-CoberturaClassesByFilename {` is at `Invoke-MSTestWithCoverage.Helpers.ps1:167` |
| `lines-valid="110849"` equals the raw `<line number=` element count rather than the distinct count | **HOLDS EXACTLY** | `coverage-final.cobertura.xml:2` reports `lines-valid="110849"`; a count of `<line number=` occurrences over the whole file returns **110849**. Identical. |

The mechanism: every `<class>` carries both a `<methods><method><lines>` subtree and a class-level
`<lines>` block that repeats the same source lines (see `coverage-final.cobertura.xml:22613-22659`
versus `:22660-22738`). The XPath `.//lines/line` matches both.

**Scope of the damage.** For a class element with no companion class, both numerator and denominator
double, so the *ratio* survives and only the absolute `lines-valid` / `lines-covered` /
`branches-valid` / `branches-covered` attributes at `coverage-final.cobertura.xml:2` are ~2x
inflated. For a class element that **was merged**, the ratio is also wrong (§A.4). Since the merged
case is the common case for async-heavy code, the defect is not cosmetic.

### B.5 Are epic.md's two required corrections implemented?

Answered separately, as requested.

**Correction 1 — "union per `filename`, taking MAX hits per line" (epic.md:529-532):
PARTIALLY IMPLEMENTED, and the part that is implemented is then discarded.**

- The union itself is present and correct: `Merge-CoberturaClassesByFilename` groups class nodes by
  `@filename` within each `<package>` (`Invoke-MSTestWithCoverage.Helpers.ps1:181-187`), picks a
  non-compiler-generated primary via `$_.name -notmatch '<'` (`:195-198`), builds a `$lineMap` keyed
  on `[int]$lineNode.number` (`:220`), and applies
  `SetAttribute('hits', [math]::Max(existing, candidate))` (`:234`). That is literally "max hits per
  line", per filename.
- What is missing: the other group members' `<methods>` are never merged in (`:202-206` only ensures
  the element exists), and the recomputed `line-rate` is then taken over `.//lines/line`
  (`:270-276` calling `:122`), which mixes the correct union with the primary-only method subtree.
  The correctly-unioned data is written to the XML but the summary attribute derived from it is not
  the union's rate. A consumer reading the attribute gets a blended number (§A.4); a consumer reading
  the class-level `<lines>` children gets the correct one.

**Correction 2 — "decide the denominator on `<line>` child count, never `line-rate`"
(epic.md:533-536): NOT IMPLEMENTED.** Nothing in either harness script makes a
present-versus-absent-denominator decision at all. There is no `interface-only` / N/A concept in the
code. The one in-repo consumer that does classify, `scripts/temp-extract-coverage.ps1:13-17`, keys
on `line-rate` — exactly the pattern the correction forbids. Note the interaction with §A.3: an
interface-only file emits **no `<class>` element whatsoever** (established in
`.../research/iqfc-collection-controller.md` §A.3), so any consumer must handle "filename absent from
the report" as a third state alongside "present with zero lines" and "present with lines".

**Recommendation for F11.** Do not wait for, and do not modify, the shared harness scripts —
`scripts/vscode/Invoke-MSTestWithCoverage*.ps1` are repository-wide files outside F11's assignment,
and #441 already owns the fix. F11's plan should implement the §A.5 recipe in a small, disposable
evidence-generation step (or consume F1's harness if it implements the recipe; see §E), record the
per-file numbers in `<FEATURE>/evidence/qa-gates/`, and explicitly note in the evidence that the
`line-rate` attribute was not used and why, citing #441.

---

## C. Branch coverage

**Branch data is emitted, complete, and per-file obtainable.** No gap exists.

Evidence, all from `coverage-final.cobertura.xml`:

- **Line level.** Every `<line>` carries a `branch` attribute; branching lines carry
  `condition-coverage` and a `<conditions>` child list. Example, class-level lines of
  `QfcHomeController.Iteration.cs`:
  - `:22663` — `<line number="15" hits="1" branch="True" condition-coverage="100% (2/2)">` with
    `<condition number="0" type="jump" coverage="100%" />` at `:22665`.
  - `:22694` — `<line number="44" hits="0" branch="True" condition-coverage="0% (0/2)">`.
  - `:22708` — `<line number="60" hits="1" branch="True" condition-coverage="50% (2/4)">` with two
    `<condition>` children (`:22710-22711`) — proving multi-condition lines are represented.
  - Non-branching lines carry `branch="False"` and no `condition-coverage` (e.g. `:22661`).
- **Class level.** Every `<class>` carries `branch-rate` (`:22612`, `:22314`, `:21643`, and all ten
  `QfcItemController` rows in §A.2).
- **Package and report level.** `branch-rate="0.7371154614462645"` on the QuickFiler package
  (`:7`); `branch-rate="0.790039" branches-covered="22001" branches-valid="27848"` on the root
  (`:2`).

**How to obtain a per-file branch rate.** Step 5 of the §A.5 recipe. Worked example on
`QfcHomeController.Iteration.cs`: the class-level branch lines are 15 (2/2), 25 (2/2), 44 (0/2), 60
(2/4), 61 (2/2) → **8/12 = 66.67%**, which is the correct per-file branch rate. The emitted
attribute `branch-rate="0.666667"` happens to agree here only because the double-counted method
subtree contributed 4/6 in the same ratio; that coincidence must not be relied on. Recompute.

**Caveat the plan must carry.** `Get-CoberturaLineConditionCoverageParts`
(`Invoke-MSTestWithCoverage.Helpers.ps1:146-165`) returns `Covered = 0, Total = 0` for a line with no
`condition-coverage` attribute, so lines without branches contribute nothing to either side. A file
with **no** branching lines yields `0/0` and must be reported **N/A** for branch, never 0%. This is
the branch-side analogue of epic.md's correction 2 and is the same failure mode. Several of the 13
proposed partials — for example the pure-arithmetic `QfcCollectionController.Layout.cs` — may
legitimately have very few branches, so this is a live case for F11, not a theoretical one.

The 75% branch gate (epic.md:487, `.claude/rules/general-unit-test.md`) is therefore **measurable and
enforceable** for every partial. No gap-handling clause is needed beyond the N/A rule above.

---

## D. Repository-wide coverage measurement

### D.1 What CI actually does

`.github/workflows/ci.yml`, step `Run MSTest suite with coverage` (`:118-150`):

- Locates `vstest.console.exe` via `vswhere` (`:124-132`).
- Enumerates test assemblies at `.github/workflows/ci.yml:134-140`:
  ```
  Get-ChildItem -Path $env:GITHUB_WORKSPACE -Recurse -Filter '*.Test.dll' |
      Where-Object {
          $_.FullName -match "\\bin\\$($env:BUILD_CONFIGURATION)\\" -and
          $_.FullName -notmatch '\\obj\\' -and
          $_.FullName -notmatch '\\ref\\'
      }
  ```
- Runs `& $vstestPath $testAssemblies /EnableCodeCoverage /InIsolation /Logger:trx /TestCaseFilter:"TestCategory!=LiveOutlook"` (`:147`).
- Uploads only `TestResults/**/*.trx` and `TestResults/**/*.coverage` (`:152-160`).

Three consequences:

1. **CI does not filter `.claude/worktrees`.** There is no such clause at `:135-139`. CI is safe only
   because it runs on a fresh checkout where no nested worktree build output exists.
2. **CI produces no Cobertura and computes no percentage.** `/EnableCodeCoverage` emits the binary
   `.coverage` format; nothing converts it or asserts a threshold. **The repository-wide figure for
   this child must be produced locally.**
3. `TaskMaster.runsettings` is not passed to CI's `vstest` invocation either, so CI's MSTest
   parallelization is the vstest default, not the repo-root file's `ClassLevel` setting.

### D.2 The same hazard exists locally, and is worse

`scripts/vscode/Invoke-MSTestWithCoverage.ps1:296-302` uses the identical filter set and likewise
has **no** `.claude` exclusion. Running with `-SearchRoot .` from the canonical repo root
`repos/TaskMaster` will therefore sweep in every `*.Test.dll` under
`.claude/worktrees/<agent>/**/bin/Debug/`. Two distinct failure modes follow:

- **Bogus failures.** Stale agent-worktree assemblies built from different source produce spurious
  assembly-initialization/signature failures.
- **A silently wrong denominator.** Loading two copies of the same assembly changes which modules
  `dotnet-coverage` instruments, which shifts `lines-valid` — precisely the "denominator instability"
  #424's own evidence documented at `coverage-delta.2026-08-07T00-48.md:65`.

`.gitignore:351` confirms `.claude/` is tracked so it materializes in worktrees; a
`.claude/worktrees` directory does **not** currently exist under this worktree (verified by glob),
but it will exist under the canonical root whenever an agent worktree is live.

### D.3 The like-for-like trap in #424's evidence — do not repeat it

`docs/features/active/2026-08-06-quickfiler-high-confidence-queue-init-stall-424/evidence/baseline/coverage-baseline.cobertura.xml:2`
reports `line-rate="0.7019272859161799" lines-valid="79957"`, and its line 3 is `<packages>` with
**no `<sources>` element** — meaning that file is **raw `dotnet-coverage` output that was never
post-processed**. The final artifact `coverage-final.cobertura.xml:2` reports
`line-rate="0.856453" lines-valid="110849"` and **was** post-processed (third-party packages
stripped, lines double-counted).

The two numbers were nevertheless placed side by side as "Merge-base baseline" versus "Post-change"
at `coverage-delta.2026-08-07T00-48.md:56-61`. The author correctly flagged the +38.6% denominator
growth as "a measurement artifact" (`:65`) but attributed it to instrumentation instability alone.
At least part of it is simply that one file went through `ConvertTo-KoverageCoberturaXml` and the
other did not. The epic then imported the raw figure as the authoritative repository baseline
(`docs/features/epics/quickfiler-per-file-coverage/epic.md:479-480`, "merge-base repository line rate
of 70.19%").

**Binding rule for F11: the before and after artifacts must be produced by the identical command and
must both be post-processed.** Comparing a raw report to a Koverage-processed report is a category
error.

### D.4 The concrete command this child should use

Run from the child's own worktree root (not the canonical repo root). Both the before-baseline and
the after-measurement use this exact sequence.

**Step 1 — pre-flight assertion (guards against stale worktree builds):**

```powershell
$stale = Get-ChildItem -Path . -Recurse -Filter '*.Test.dll' |
    Where-Object {
        $_.FullName -match '\\bin\\Debug\\' -and
        $_.FullName -notmatch '\\obj\\' -and
        $_.FullName -notmatch '\\ref\\'
    } |
    Where-Object { $_.FullName -match '\\\.claude\\' }
if ($stale) { throw "Stale worktree test assemblies present; remove before measuring:`n$($stale.FullName -join "`n")" }
```

The `Where-Object` clauses replicate `scripts/vscode/Invoke-MSTestWithCoverage.ps1:297-301` exactly,
so the assertion sees the same set the harness will. The `\.claude\` filter is the part the harness
lacks.

**Step 2 — build, then measure:**

```powershell
msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU"
pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 `
    -SearchRoot . -Configuration Debug `
    -CoverageOutput 'coverage\coverage.cobertura.xml'
```

`/t:Rebuild` rather than `/t:Build` matches CI (`.github/workflows/ci.yml:113`) and prevents a
silently skipped recompilation.

**Step 3 — record two repository-wide figures, both times, from the produced XML:**

| Figure | How | Purpose |
| --- | --- | --- |
| Harness-native | `/coverage/@line-rate` and `/coverage/@branch-rate` at line 2 of the output | Directly comparable to the before-run because the same defective method was applied to both. This is the epic AC8 "retain or improve" comparator. |
| Recomputed | §A.5 recipe summed across all `<class>` elements in the report (class-level `<lines>` only, unioned by `filename`) | The honest figure; the one to cite in prose. |

Report both. Never mix one with the other across the before/after boundary.

**Step 4 — copy both artifacts to canonical evidence locations**
(`.claude/skills/evidence-and-timestamp-conventions/SKILL.md:15-19`):

- before: `<FEATURE>/evidence/baseline/coverage-baseline.cobertura.xml`
- after: `<FEATURE>/evidence/qa-gates/coverage-final.cobertura.xml`
- per-file table: `<FEATURE>/evidence/qa-gates/per-file-coverage.<timestamp>.md`

where `<FEATURE>` is
`docs/features/active/2026-08-07-quickfiler-collection-controller-coverage-454/`.

**A note the plan must carry about the baseline run.** The before-baseline must be captured **with
`[ExcludeFromCodeCoverage]` still on `QuickFiler/Controllers/QfcCollectionController.cs:21`**, i.e.
at the merge-base state. In that report the file will be **absent entirely**, not present at 0%.
That absence is the baseline. The first *measured* number for the file comes from a second run taken
immediately after the attribute is removed and before any new test is written; that is the figure
against which the child's gain is stated. The sibling research estimates it at 12-20%
(`.../research/qfc-collection-controller.md` §C5) — treat that as an estimate to be replaced by a
measurement, not as a planning input.

---

## E. The F1 (#432) dependency as a non-halting gate

### E.1 Existence check (factual, one line, as requested)

Neither `docs/features/epics/quickfiler-per-file-coverage/coverage-ledger.md` nor
`docs/features/active/2026-08-07-quickfiler-coverage-ledger-432/` exists on this branch — the epic
folder contains only `epic.md` — which is expected while F1 is prepared concurrently and is **not** a
blocker for preparing this child.

### E.2 What epic.md says F1 will deliver

- The per-file classification ledger for all 121 compiled files, testable vs ratified-exempt, with a
  rationale per exempt file (`epic.md:318-324`).
- "The repeatable per-file coverage report harness" derived from the Cobertura output of
  `Invoke-MSTestWithCoverage.ps1` (`epic.md:259-263`, `:289-291`, `:322`).
- Disposition instructions for each of the 33 existing `[ExcludeFromCodeCoverage]` attributes,
  assigned to the owning child (`epic.md:323-324`).
- The ledger must carry **rules, not just rows**, so files created mid-wave can be classified without
  re-running F1 (`epic.md:576-578`), and the creating child appends its own rows (`epic.md:579-582`).
- Three buckets: `testable`, `ratified-exempt`, `interface-only / not-measured` (`epic.md:509-522`).

### E.3 The MINIMAL assumption set the plan should encode

Encode these four and **nothing else**. Anything beyond this couples the plan to a format F1 has not
yet fixed.

| # | Assumption | Why it is safe |
| --- | --- | --- |
| A1 | A ledger file exists at `docs/features/epics/quickfiler-per-file-coverage/coverage-ledger.md` and is a Markdown file in which each production file's repo-relative path appears as a literal substring on the row that classifies it. | Path is named in the delegation brief and follows the epic folder convention. Substring matching survives any column ordering, header wording, or path-separator choice. |
| A2 | Each such row states one of exactly three bucket tokens: `testable`, `ratified-exempt`, `interface-only`. | Fixed verbatim by `epic.md:519`. Match `interface-only` as a prefix so the `/ not-measured` suffix is optional. |
| A3 | The ledger is append-only and additive; fan-in conflicts are resolved by keeping both sides. | Stated verbatim at `epic.md:579-582`. This is what lets F11 append 17 rows without coordinating with F1. |
| A4 | New files created by this child default to `testable` at >= 90% line unless a rationale meeting one of the three exemption grounds is recorded. | Stated verbatim at `epic.md:583-585`. |

**Explicitly NOT assumed** (any of these appearing in the plan is a defect):
the ledger's column names, column count, or ordering; the ledger's path separator; the name, path,
parameters, or output format of F1's harness script; that F1's harness implements the §A.5 recipe;
that F1 pre-populates rows for the 13 new partials or the 4 new seam files.

### E.4 Recommended Phase 0 gate

Four checks. Two halt, two do not. All are evaluated at **execution** time; none of them makes the
plan unverifiable at **preparation** time, because each names a file path and a literal string rather
than a format.

**G0.1 — LEDGER PRESENT (HALT).**
Assert `docs/features/epics/quickfiler-per-file-coverage/coverage-ledger.md` exists.
If absent: **HALT**. Emit `F1_LEDGER_MISSING: coverage-ledger.md not present; F1 (#432) has not
landed on the integration branch.` Do not proceed, do not author a substitute ledger, do not remove
`[ExcludeFromCodeCoverage]`. This is the genuine halt: without F1's classification the child cannot
know whether removing the exemption is sanctioned.

**G0.2 — THIS CHILD'S CLASSIFICATION IS `testable` (HALT).**
Locate the ledger row containing the literal `QfcCollectionController.cs` (matching the
`Controllers/QfcCollectionController.cs` row, not a `.<Concern>.cs` partial row). Assert its bucket
token is `testable`.
- Row absent: **HALT**, `F1_LEDGER_ROW_MISSING: no row for QuickFiler/Controllers/QfcCollectionController.cs.`
- Bucket is `ratified-exempt`: **HALT**, `F1_LEDGER_CONFLICT: file classified ratified-exempt; this
  child's entire premise is exemption removal. Reconcile with the epic owner before proceeding.`
- Bucket is `interface-only`: **HALT** with the same conflict code — the file is 2,349 lines of
  executable code and that classification would be an F1 defect.

**G0.3 — INTERFACE-FILE CLASSIFICATION (RECORD, DO NOT HALT).**
Locate the row containing `IQfcCollectionController.cs` and assert `interface-only`. If absent or
classified otherwise, **proceed** and record
`F1_LEDGER_RECONCILE: IQfcCollectionController.cs classified <X>; expected interface-only / not-measured.`
Append or correct the row per `epic.md:579-582` and cite `.../research/iqfc-collection-controller.md`
§A.3-A.5 as the evidence. Rationale for not halting: the sibling artifact already established the
correct classification three ways, so a mismatch is a reconciliation task, not an unknown.

**G0.4 — F1 HARNESS DISCOVERY (RECORD, DO NOT HALT — FALLBACK MANDATORY).**
Search `scripts/**/*[Cc]overage*.ps1` and
`docs/features/active/2026-08-07-quickfiler-coverage-ledger-432/**` for a script that accepts a
Cobertura path and emits a per-file line **and** branch rate.
- Found, and it computes rates from class-level `<lines>` children (not the `line-rate` attribute):
  use it. Record its path and a one-line confirmation that it implements the §A.5 recipe.
- Found, but it reads the `line-rate` attribute: use the §A.5 recipe as the authoritative figure,
  additionally record F1's figure, and record
  `F1_HARNESS_DISAGREES: F1 harness reads the line-rate attribute; see #441 and this child's
  coverage-harness-contract.md §A.4. Authoritative figures recomputed per §A.5.`
- Not found: **proceed** with the §A.5 recipe and record
  `F1_HARNESS_ABSENT_FALLBACK_APPLIED: per-file rates recomputed directly from
  coverage/coverage.cobertura.xml per coverage-harness-contract.md §A.5.`

Rationale for the split: **classification is a contract F11 cannot manufacture** (halt);
**measurement is fully reproducible from the Cobertura XML with the recipe in §A.5** (never halt).
This keeps F1 a real upstream dependency for the decision that matters while removing it from the
critical path for the evidence.

### E.5 One additional cross-child note

`epic.md:579-582` makes `coverage-ledger.md` an additive shared file exactly like
`QuickFiler/QuickFiler.csproj`. F11 appends the most rows of any child — 13 partials plus 4 seam
files, per `.../research/qfc-collection-controller.md` §A3-A5. Append them as one contiguous block in
the same change that adds the `<Compile Include>` entries after
`QuickFiler/QuickFiler.csproj:311`, so both shared files present a single hunk at fan-in.

---

## F. Coverage-config interaction

### F.1 Does anything exclude the QuickFiler assembly? NO

`coverage.config` (the outer `dotnet-coverage --settings`, wired at
`scripts/vscode/Invoke-MSTestWithCoverage.ps1:320` and `:227`) contains exactly one exclusion
section, `Configuration/CodeCoverage/ModulePaths/Exclude` (`coverage.config:12-22`), listing seven
patterns: `.*Deedle.*`, `.*FSharp.*`, `.*Castle\.Core.*`, `.*FluentAssertions.*`, `.*Moq.*`,
`.*Microsoft\.Testing.*`, `.*MSTest.*` (`coverage.config:14-20`). **None matches `QuickFiler.dll`.**

There is no `Attributes`, `Functions`, `Sources`, `Companies`, or `PublicKeyTokens` exclusion section
anywhere in `coverage.config` (verified by grep, zero matches). The only other exclusion the pipeline
adds at runtime is `.*\.Test\.dll$`, injected in memory by
`Invoke-MSTestWithCoverage.ps1:99-113` — which correctly excludes `QuickFiler.Test.dll` and does not
touch `QuickFiler.dll`.

`TaskMaster.runsettings:14-24` carries the same seven patterns under its `Code Coverage` data
collector. That file is **not used by the harness at all** — the harness passes
`scripts/vscode/TaskMaster.cli.runsettings` (`Invoke-MSTestWithCoverage.ps1:33`), which contains only
the `<MSTest><Parallelize>` block and no data collector
(`scripts/vscode/TaskMaster.cli.runsettings:1-9`), by explicit design
(`Invoke-MSTestWithCoverage.ps1:19-26`). `TaskMaster.runsettings` matters only for Visual Studio's
own test runs.

Post-processing removes packages whose `name` is not in the project allowlist
(`Invoke-MSTestWithCoverage.Helpers.ps1:318-322`), built from every non-`.Test` `*.csproj`
`AssemblyName` in the tree (`:3-47`). `QuickFiler` is such a project, so its package survives — as
`coverage-final.cobertura.xml:7` (`name="QuickFiler"`) demonstrates.

`QuickFiler/Properties/AssemblyInfo.cs` carries no assembly-level `[ExcludeFromCodeCoverage]`
(verified by grep; the only non-standard attribute is `InternalsVisibleTo("QuickFiler.Test")` at
`:5`).

**Conclusion: `[ExcludeFromCodeCoverage]` at `QuickFiler/Controllers/QfcCollectionController.cs:21`
is the sole mechanism keeping the type out of instrumentation. Removing it is necessary and
sufficient.** The empirical proof that removal works is that ten `QfcItemController` partials and
three `QfcHomeController` partials, in the same assembly and same `Controllers\` folder, are all
instrumented and reported (§A.2). No shared config file needs editing, which also means F11 creates
no fan-in conflict on `coverage.config`.

One consequence worth recording in the plan: once the attribute is removed, **every one of the 13
partials plus the 4 seam files enters the denominator at once**, and the QuickFiler package
line-rate will drop measurably in the first post-removal measurement before any new test lands. Order
the plan so that the package-level regression is closed before the final QA gate, and state the
expected transient drop in the evidence so a reviewer does not read it as a defect.

### F.2 Parallelization determinism hazard — REAL for the static counter

`TaskMaster.runsettings:3-8` and `scripts/vscode/TaskMaster.cli.runsettings:3-8` both set:

```
<MSTest><Parallelize><Workers>0</Workers><Scope>ClassLevel</Scope></Parallelize></MSTest>
```

The CLI file is the one the harness actually applies (`Invoke-MSTestWithCoverage.ps1:33`, `:76`).
`Workers 0` means "use the processor count", so **`[TestClass]`-level parallelism is active** on every
harness run. Methods within one class remain serial; distinct classes run concurrently.

**Hazard 1 — `private static int removespecificcontrolgroupcounter`
(`QuickFiler/Controllers/QfcCollectionController.cs:1157`): CONFIRMED HAZARD.**

The counter is `static`, therefore shared across every test class in the process. Its lifecycle:

- `Interlocked.Increment` at `QuickFiler/Controllers/QfcCollectionController.cs:1161`
- read at `:1237` — `if (removespecificcontrolgroupcounter > 1)`
- `Interlocked.Decrement` at `:1247`

`Interlocked` makes the arithmetic atomic but does **not** make the `> 1` read deterministic. If two
test classes in different parallel workers both drive `RemoveSpecificControlGroupAsync`, each
observes the other's increment and the `> 1` branch fires or does not fire depending on scheduling.
Two failure modes follow, and the second is worse than the first:

1. **Flaky assertion.** A test asserting the `> 1` branch is not taken fails intermittently.
2. **Non-reproducible coverage.** The hit map for lines 1237-1242 varies run to run, so the per-file
   line rate for `QfcCollectionController.Removal.cs` (or `.RemoveGroup.cs` if the pre-authorized
   further split is taken — see `.../research/qfc-collection-controller.md` §A3 file 6) is not
   reproducible. Two runs of the same commit can straddle the 80% gate.

There is a further, subtler point specific to this counter: the increment at `:1161` and the
decrement at `:1247` are **not** wrapped in `try`/`finally`
(`.../research/qfc-collection-controller.md` §D2, §E6), so an exception anywhere between them leaks a
permanent `+1` into process-global state, which then poisons every subsequent test in the process
regardless of parallelism.

**Required mitigations, all three:**

- Every test class that drives `RemoveSpecificControlGroup(int)` or
  `RemoveSpecificControlGroupAsync` carries `[DoNotParallelize]`. Precedent exists in this exact test
  project: `QuickFiler.Test/Helper Classes/ViewerQueueStaticWrapperTests.cs:11` and
  `QuickFiler.Test/Helper Classes/EmailMoveMonitorTests.cs:22`.
- Confine those tests to a **single** test class so `[DoNotParallelize]` fully serializes them; two
  `[DoNotParallelize]` classes are each serialized against the rest of the run, but the attribute is
  the repository's established tool and combining the tests in one class removes any residual doubt.
- Reset the counter to `0` by reflection in `[TestInitialize]`, to defend against the missing
  `finally` and against any leak from a prior class. This matches the existing
  `[TestCleanup]`-reset discipline at `QuickFiler.Test/Helper Classes/ViewerQueueStaticWrapperTests.cs:11-22`.

**Hazard 2 — `public static string xComma(string)`
(`QuickFiler/Controllers/QfcCollectionController.cs:2330`): NO HAZARD.**

It is a pure static *function*, not static mutable state: it takes a `string` and returns a `string`
with no field access. Concurrent invocation from parallel test classes is safe, and its hit map is
deterministic. No `[DoNotParallelize]` is required for
`QfcCollectionController.Move.cs`. (The separate cross-child constraint stands unchanged: `xComma`
must remain `public static` on the type for `QuickFiler/Controllers/EfcHomeController.Metrics.cs:79`,
which belongs to F8.)

**Hazard 3 — already-known process-global statics, restated for completeness.** Tests using
`ItemViewerQueue.SetCoreForTesting` mutate process-global state and must carry `[DoNotParallelize]`
plus the reset pair, exactly as at
`QuickFiler.Test/Helper Classes/ViewerQueueStaticWrapperTests.cs:11-22`. Seam **S3** in
`.../research/qfc-collection-controller.md` §B1 exists specifically so most F11 tests avoid this.

---

## G. Consolidated obligations this child's plan must carry

1. Compute every per-file line and branch rate with the §A.5 recipe. Never read the `line-rate` or
   `branch-rate` attribute. Record in the evidence that the attribute was not used, and why, citing
   issue #441 and §A.4 of this document.
2. Report `N/A` — never `0%` — for a file with zero coverable lines (line side) or zero branch
   conditions (branch side). Handle "filename absent from the report" as a third state.
3. Do not modify `scripts/vscode/Invoke-MSTestWithCoverage.ps1` or
   `.Helpers.ps1`; they are repository-wide files outside F11's assignment and #441 owns the fix.
   Do not reuse or extend `scripts/temp-extract-coverage.ps1`.
4. Capture the before-baseline at the merge-base state (attribute still present; the file will be
   absent from the report — that absence *is* the baseline), and a second measurement immediately
   after attribute removal and before any new test, as the true starting figure.
5. Produce before and after repository-wide figures with the identical command and the identical
   post-processing. Never compare a raw report to a post-processed one (§D.3).
6. Run the §D.4 Step 1 pre-flight assertion before every measurement run.
7. Write all evidence to `<FEATURE>/evidence/baseline/` and `<FEATURE>/evidence/qa-gates/` per
   `.claude/skills/evidence-and-timestamp-conventions/SKILL.md:15-19`.
8. Apply the Phase 0 gate of §E.4: halt on G0.1/G0.2, record-and-proceed on G0.3/G0.4.
9. Apply the three §F.2 mitigations to every test touching
   `removespecificcontrolgroupcounter`.
10. State the expected transient drop in the QuickFiler package rate at the moment the exemption is
    removed, so a reviewer does not read it as a regression.

---

## Documented Deviations and corrections to inherited context

**H-1 — epic.md's "Measured Coverage Baseline" table is quantitatively wrong, not merely
indicative.** `epic.md:155-178` lists per-file `Lines` and `Line %` taken from the Cobertura
attributes. The `Lines` column is roughly double the real coverable-line count (e.g.
`QfcHomeController.Metrics.cs | 212` versus 106 real lines), and the `Line %` column is **overstated**
for every file that has a compiler-generated companion class (e.g. `QfcHomeController.cs | 71.4%`
versus the correct 68.4%). The epic already warns the table is indicative (`epic.md:142-145`); this
document supplies the mechanism (§A.4) and the direction of the error. Files listed just above 80% in
that table may in fact be below it. F11's own files are absent from the table anyway, so no F11
planning input changes.

**H-2 — the 70.19% repository baseline in `epic.md:479-480` is a raw, unprocessed figure.** It comes
from `.../424/evidence/baseline/coverage-baseline.cobertura.xml:2`, which has no `<sources>` element
and therefore never passed through `ConvertTo-KoverageCoberturaXml`. It includes third-party packages
that the post-processor strips and does not include the double-count that the post-processor
introduces. It is not comparable to any post-processed figure. F11 must not use it as its own
comparator; F11 must generate its own before-figure with its own command (§D.4).

**H-3 — epic.md's harness correction 1 is closer to done than the epic implies, and correction 2 is
untouched.** The union-with-max-hits logic already exists and is correct
(`Invoke-MSTestWithCoverage.Helpers.ps1:217-268`); what is missing is that the rate derived from it
is computed over a mixed node set. F1 should be told this precisely, because "implement the union"
would produce a duplicate implementation rather than the one-line-class fix that is actually needed.

**H-4 — `TaskMaster.runsettings` is not on the harness path.** Any plan text asserting that the
repo-root runsettings governs harness coverage exclusions is wrong; the harness passes
`scripts/vscode/TaskMaster.cli.runsettings` and takes exclusions from `coverage.config`
(`Invoke-MSTestWithCoverage.ps1:19-26`, `:33`, `:320`). The `ClassLevel` parallelization setting is
identical in both files, so the §F.2 hazard analysis is unaffected by which one applies.

**Constraints verified and NOT disproved:**
- `removespecificcontrolgroupcounter` at `QuickFiler/Controllers/QfcCollectionController.cs:1157` — confirmed.
- `public static string xComma(string)` at `QuickFiler/Controllers/QfcCollectionController.cs:2330` — confirmed.
- `Get-CoberturaCoverageSummary` at `Invoke-MSTestWithCoverage.Helpers.ps1:98` selecting `.//lines/line` at `:122` — confirmed.
- `Merge-CoberturaClassesByFilename` at `Invoke-MSTestWithCoverage.Helpers.ps1:167` — confirmed.
- `lines-valid="110849"` equals the raw `<line number=` element count — confirmed exactly.
- `<Workers>0</Workers>` / `<Scope>ClassLevel</Scope>` in both runsettings files — confirmed.
