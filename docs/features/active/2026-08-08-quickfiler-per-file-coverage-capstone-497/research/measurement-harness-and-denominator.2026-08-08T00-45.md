# F16 Capstone Research — Measurement Harness and Denominator Mechanics

- **Feature:** `2026-08-08-quickfiler-per-file-coverage-capstone-497` (epic #136, child F16, wave 2)
- **Branch:** `feature/quickfiler-per-file-coverage-capstone` (from `origin/epic/quickfiler-per-file-coverage-integration`)
- **Timestamp:** 2026-08-08T00-45
- **Scope:** measurement mechanics only. No plan, no code.

## Session Capability Limitation (read first)

**No shell tool was available in this session.** Every finding below was derived by reading and
pattern-matching files in the checkout. Nothing was executed. Consequently:

- Every *observed* fact (file contents, counts of literal patterns, XML structure) is verified.
- Every *runtime* fact (whether `msbuild`/`vstest.console.exe`/`csharpier` resolve on this host, what
  a command prints, the current issue state of #441/#478) is **unverified** and is confined to the
  "Unverified" section at the end. Q4's PATH question in particular could not be answered by
  observation and must be re-confirmed at execution time.

---

## Q1 — Denominator derivation from `QuickFiler/QuickFiler.csproj`

### Current count on this branch: **121**

Verified by literal pattern count against
`C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-a8b4d64f3ad6053b3\QuickFiler\QuickFiler.csproj`:

| Pattern | Count |
| --- | --- |
| `<Compile Include=` | **121** |
| `<Compile Include="[^"]*\.cs"` | **121** |
| `<Compile Include="[^"]*(\*\|\.\.\\)[^"]*"` (wildcard or parent-relative) | **0** |
| `<Link>`, `<Compile Remove`, `<Compile Update`, `Exclude=` | **0** |
| `<Compile Include="(Controllers\|Helper Classes\|Interfaces\|Properties\|Viewers)\\` | **121** |

The epic's stated 121 is therefore correct **as of this branch head**. Per-directory split, read
directly from the single `<ItemGroup>` at lines 289-462:

| Directory | csproj lines | Count |
| --- | --- | --- |
| `Controllers\` | 290-341 | 52 |
| `Helper Classes\` | 342-354 | 13 |
| `Interfaces\` | 355-368 | 14 |
| `Properties\` | 369-379 | 3 |
| `Viewers\` | 380-461 | 39 |
| **Total** | | **121** |

This matches F1's expected per-directory figures exactly (F1 plan `[P1-T1]` acceptance).

### Exact parsing approach

- **Project XML namespace is present and non-default.** Line 2:
  `<Project ToolsVersion="15.0" xmlns="http://schemas.microsoft.com/developer/msbuild/2003">`.
  Any XPath-based reader **must register that namespace**; a bare `SelectNodes('//Compile')` returns
  zero nodes on a legacy non-SDK project. A namespace-agnostic form
  (`//*[local-name()='Compile']`) also works. PowerShell dotted property access
  (`$xml.Project.ItemGroup.Compile`) works without namespace registration and is what the repo's
  existing scripts style would suggest.
- **The `Include` attribute is the only path source.** All 121 values are project-relative,
  backslash-separated, and end in `.cs`.
- **No wildcards.** Zero `Include` values contain `*` or `..\`. The compile set is fully enumerable
  by attribute read; no MSBuild evaluation is required.
- **`Link` handling: not applicable.** There is no `<Link>` metadata anywhere in the file, and no
  `Include` escapes the project directory, so there is no linked-file case to resolve.
- **`DependentUpon` / `SubType` / `AutoGen` are child elements, not attributes, and must be
  ignored.** 22 of the 121 `<Compile>` entries are written in long form with children rather than as
  self-closing elements — for example lines 370-374 (`Properties\Resources.Designer.cs` with
  `AutoGen`/`DesignTime`/`DependentUpon`) and lines 435-437 (`Viewers\ItemViewer.Designer.cs` with
  `DependentUpon`). A line-oriented parser that keys on `<Compile Include=` handles both forms
  identically because the `Include` attribute is always on the opening tag. **A parser that requires
  a self-closing `/>` on the same line would miss 22 entries.**
- **`EmbeddedResource` must be excluded and lives in a separate `<ItemGroup>`.** The `.resx` items
  start at line 481 in their own `ItemGroup` (`Properties\Resources.resx`,
  `Viewers\BayesianPerformanceViewer.resx`, `Viewers\EfcViewer.resx`, `Viewers\ItemViewer.resx`, …).
  A broadened pattern `<(Compile|EmbeddedResource|None|Content|Page)\s` returns **171** occurrences
  in the file, so the 50-item gap between 171 and 121 is exactly what a sloppy pattern would pull in.
  Key on `<Compile Include=` and nothing else.
- **Line endings are CRLF.** A regex anchored with `/>$` returns 0 matches while `/>` returns
  matches, which is the observable signature of a trailing `\r`. Any parser must not assume LF. This
  corroborates the epic's warning that `sed -i` on this file produces a whole-file diff.

### Path separators and the mapping onto Cobertura `filename`

`Include` values use **backslash** (`Controllers\QfcQueue.cs`, `Helper Classes\cInfoMail.cs`).

The Cobertura `filename` attribute in a **post-processed** report is
`QuickFiler\<Include value>` — same backslashes, spaces preserved, no quoting. Verified directly in
`docs/features/active/2026-08-06-quickfiler-high-confidence-queue-init-stall-424/evidence/qa-gates/coverage-final.cobertura.xml`:

```
filename="QuickFiler\Helper Classes\EfcViewerQueue.cs"
filename="QuickFiler\Helper Classes\ConversationResolver.Loading.cs"
filename="QuickFiler\Properties\Settings.Designer.cs"
filename="QuickFiler\Interfaces\MailItemActionsAdapter.cs"
```

So the mapping is literal prefix concatenation: `"QuickFiler\" + Include`. This matches F1's spec
decision D11. Two caveats:

1. In a **raw** (un-post-processed) report the `filename` is an **absolute path** rooted at the
   capturing worktree, e.g.
   `filename="C:\Users\DanMoisan\repos\TaskMaster-wt\2026-08-04T18-38\QuickFiler\Controllers\EfcHomeController.cs"`
   (verified at `evidence/baseline/coverage-baseline.cobertura.xml:6`). The prefix rewrite is done by
   `ConvertTo-KoverageRelativePath` during post-processing only.
2. The separator is `[System.IO.Path]::DirectorySeparatorChar` by default
   (`Invoke-MSTestWithCoverage.Helpers.ps1:309`), so a report captured on a non-Windows host, or with
   an explicit `-PathSeparator '/'`, would use forward slashes. Normalise both sides to backslash and
   compare `OrdinalIgnoreCase`.

### `QuickFiler/Legacy/**` and `QuickFiler/Notes/**` — confirmed absent from the compile set

A search for `Legacy` or `Notes` anywhere in `QuickFiler.csproj` returns **zero matches**. Both
directories exist in the working tree (`QuickFiler\Legacy\` has 11 `.cs` files;
`QuickFiler\Notes\` has 2 `.cs` files plus 2 non-`.cs` files) but are not compiled. The epic's
statement is correct.

### Additional finding the epic does not record: the compile set is not the file set

There are **156 `.cs` files on disk under `QuickFiler/`** versus 121 compiled. The 35-file gap is not
just Legacy + Notes:

- `QuickFiler\Legacy\*.cs` — 11 files
- `QuickFiler\Notes\*.cs` — 2 files
- `QuickFiler\Viewers\*.cs` — 59 on disk vs 39 compiled = **20 orphan viewer files**, including
  `EfcViewer3.cs`, `Form1.cs`, `QFCItemViewerDarkNew.cs`, `QFCItemViewerLightNew.cs`,
  `QfcFormViewerDark.cs`, `QfcFormViewerExpanded.cs`, `QfcItemViewer.cs`,
  `QfcItemViewerExpandedLight.cs`, `QfcItemViewerLightSelected.cs`, `QfcItemViewerV1.cs` and their
  `.Designer.cs` companions
- `QuickFiler\Helper Classes\FormFocusListener.cs` — **on disk, not compiled**. This one is not in
  the epic's orphan list (which enumerated only the 7 attribute-carrying viewer orphans) and is not
  in F1's research §3.6 list. It is not in the denominator, but a capstone reconciliation that
  enumerates the filesystem rather than the csproj would wrongly flag it as an unledgered file.

**Consequence for the capstone:** the denominator must be derived from `<Compile Include=>` and never
from a filesystem glob. The epic's own rule (`## Mid-Wave File Creation`, rule 1) already says this;
the `FormFocusListener.cs` case is concrete proof that a glob would produce a false positive.

---

## Q2 — F1's harness as delivered

### Status on this branch: **not delivered — planned only**

Verified absences on `feature/quickfiler-per-file-coverage-capstone`:

- `scripts/vscode/` contains only `Install-RepoDotNetSdk.ps1`, `Invoke-MSTest.ps1`,
  `Invoke-MSTestWithCoverage.Helpers.ps1`, `Invoke-MSTestWithCoverage.ps1`, `Invoke-Restore.ps1`,
  `Invoke-VSBuild.ps1`, `Sync-PackageReferences.ps1`, `TaskMaster.cli.runsettings`,
  `TestProcessCleanup.ps1`. **`Get-PerFileCoverage.ps1` and `Get-PerFileCoverage.Helpers.ps1` do not
  exist.**
- A repo-wide glob for `**/coverage-ledger.*` returns **no files**. The ledger does not exist yet.
- `scripts/temp-extract-coverage.ps1` still exists (F1's plan deletes it).

So all statements below describe F1's **planned** contract as recorded in its spec and plan.

### Planned artifact paths (F1 plan `## Scope Constraints`, lines 57-69)

| Role | Path |
| --- | --- |
| Pure logic (6 functions, dot-sourceable) | `scripts/vscode/Get-PerFileCoverage.Helpers.ps1` |
| Entry point (I/O + exit code) | `scripts/vscode/Get-PerFileCoverage.ps1` |
| Machine-readable ledger | `docs/features/epics/quickfiler-per-file-coverage/coverage-ledger.json` |
| Human ledger | `docs/features/epics/quickfiler-per-file-coverage/coverage-ledger.md` |
| Pester suites | `tests/scripts/vscode/Get-PerFileCoverage.Helpers.Cobertura.Tests.ps1`, `…Helpers.Rows.Tests.ps1`, `…Helpers.Verdict.Tests.ps1`, `Get-PerFileCoverage.Tests.ps1`, `QuickFilerCoverageLedger.Tests.ps1` |
| Deleted | `scripts/temp-extract-coverage.ps1` |

### Ledger row schema (F1 `spec.md:504-541`)

Top level: `schema_version` (1), `generated_from` (`"QuickFiler/QuickFiler.csproj"`), `source_commit`,
`package` (`"QuickFiler"`), `threshold_percent` (80.0), `branch_threshold_percent` (75.0),
`new_file_line_target_percent` (90.0), `files[]`.

Each `files[]` row:

```json
{
  "path": "QuickFiler\\Controllers\\QfcQueue.cs",
  "lines": 610,
  "owning_child": "F2",
  "classification": "testable",
  "line_target_percent": null,
  "exempt_ground": null,
  "rationale": null,
  "attribute_dispositions": []
}
```

`classification` ∈ `{testable, ratified-exempt, interface-only}`. `exempt_ground` is a closed enum
`{generated-designer, interface-only, irreducible-host-wiring}` (`spec.md:486-489`). Note the epic
subsequently ratified a **fourth** ground for prohibited-to-execute adapters (WebView2) in
`epic.md` `## Epic Ruling: a fourth exemption ground`; F1's spec enum as written does **not** contain
a value for it. Whether F1's executed ledger widened the enum is unverifiable here — the capstone
must read the delivered `coverage-ledger.json`, not this spec.

Harness parameters (`spec.md:392-400`): `-CoberturaPath` (mandatory), `-LedgerJsonPath`,
`-PackageName` (default `QuickFiler`), `-ThresholdPercent` (80.0), `-BranchThresholdPercent` (75.0),
`-OutputPath`, `-ExemptFile`. Exit codes `0` pass / `1` coverage failure / `2` input error
(`spec.md:474-477`).

### The load-bearing question: does F1's plan fix BOTH #441 and #478?

**No. F1 fixes neither, by design — it only avoids reproducing them in a new, separate computation.**
This is a distinction the capstone must not blur.

Textual evidence: a search of the entire F1 feature folder for the strings `441` and `478` returns
**11 hits for #441 and zero hits for #478**. F1's plan was authored 2026-08-07T20-41; #478 was filed
later by F11. F1's plan therefore has no knowledge of #478 at all.

What F1's plan actually says about #441 (verbatim locations):

- `plan.2026-08-07T20-41.md:78` — "`scripts/vscode/Invoke-MSTestWithCoverage.ps1` and
  `…Helpers.ps1` — read-only dependencies. The double-counted `lines-valid` defect in
  `Get-CoberturaCoverageSummary` is out of scope and tracked at issue #441 (D5)."
- `plan.2026-08-07T20-41.md:565` — final QC asserts **zero** changes to those two files: "the issue
  #441 defect remains unmodified by design".
- `spec.md:234` — "**tracked separately at issue #441** (D5). This feature must not change that
  behavior".
- `user-story.md:177-178` — "It is out of scope here and tracked at issue #441. The new harness
  simply does not reproduce the defect in its own per-file computation."

What F1's new harness *does* do correctly (`spec.md:345-351`, `:303-315`; plan `[P9-T5]`):

- unions `<line>` nodes **by `filename`** across every `<class>` sharing that filename, deduped by
  line number with **max hits** — this is exactly the correct recipe, and it structurally sidesteps
  the #478 blend because it never reads the emitted `line-rate` attribute;
- **never** uses the `.//lines/line` descendant axis — sidesteps #441 in its own computation;
- **never** reads the `<class>` `line-rate` attribute;
- `[P9-T5]` is a dedicated regression test whose fixture "would produce twice the line count under a
  `.//lines/line` descendant-axis implementation".

**Assessment for the capstone.** This is a correct and sufficient posture *for per-file QuickFiler
numbers*, and it is not a Blocking finding on its own: nothing in issue #136's acceptance criteria
requires #441/#478 to be closed. But it has two hard consequences the capstone owns:

1. **The defective code paths remain live in the repository at capstone time** unless #441/#478 are
   separately fixed. The capstone must not call, cite, or transcribe any figure produced by
   `Get-CoberturaCoverageSummary` or `Merge-CoberturaClassesByFilename`.
2. **The repository-wide figure (issue #136 AC8) is produced by exactly those defective paths** —
   see Q3. This is the real exposure, and it is where the capstone needs its own recomputation.

### Every existing repository coverage script carrying the #441 or #478 defect

A search of `scripts/` for Cobertura parsing (`lines/line|line-rate|lines-valid|cobertura|Cobertura`)
returns exactly **three** files. A parallel search of `.claude/` and `.codex/` `.ps1` files returns
none.

| Script | Defect | Evidence |
| --- | --- | --- |
| `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1` | **#441** | `Get-CoberturaCoverageSummary` at `:98-144`; the descendant axis is `foreach ($line in $cls.SelectNodes('.//lines/line'))` at **line 122**. Used for the **root** `line-rate`/`lines-valid`/`branch-rate` written at `:341-347`. |
| `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1` | **#478** | `Merge-CoberturaClassesByFilename` at `:167-292`. The class-level union at `:217-268` is correct (max hits, richer `condition-coverage`). But `$mergedClassNode = $primaryNode.CloneNode($true)` at `:200` carries **only the primary class's `<methods>`**, and the non-primary members' `<methods>` are never merged; the recomputation at `:270-276` then runs `Get-CoberturaCoverageSummary` over that mixed node, i.e. correct-union-lines **plus** primary-only method lines. The emitted merged `line-rate`/`branch-rate` are the blend. |
| `scripts/vscode/Invoke-MSTestWithCoverage.ps1` | inherits both | `:340` calls `ConvertTo-KoverageCoberturaXml`, which calls `Merge-CoberturaClassesByFilename` at `Helpers:328` and `Get-CoberturaCoverageSummary` at `Helpers:341`. The **committed evidence artifact is the mutated output**, so the defect is baked into every committed report, not just into a console figure. |
| `scripts/temp-extract-coverage.ps1` | **#478-class defect, worse** | `:13` reads `[double]$c.'line-rate'` straight off each `<class>` and `:12` iterates `$pkg.classes.class` with **no filename grouping at all**, so a file split across a type and its `<>c` closure is scored on whichever `<class>` happens to be enumerated. It is also hard-coded to `UtilitiesCS` (`:7`) and hard-codes an output path from a March 2026 feature (`:3`). F1's plan deletes it (D11 / `[P0-T11]`). |

Two adjacent tools that are **not** Cobertura but which the capstone must also not mistake for a
per-file source:

- `.claude/hooks/validate-feature-review-coverage.ps1` and its `.codex/` twin read
  `artifacts/csharp/coverage.xml` as **JaCoCo**, summing every `//counter[@type="LINE"]` node
  (`:229-240`) and every `//counter[@type="BRANCH"]` node (`:194-205`). `//counter` matches at
  report, package, sourcefile, class and method level simultaneously, so this is a nested-counter
  summation, not a distinct-line count. Feeding it a Cobertura file yields zero matching nodes and
  the function returns `$null` (treated as artifact-unavailable). This is a **third**, distinct
  measurement hazard, unrelated to #441/#478.

---

## Q3 — Repository-wide before/after pair

### The runner

`scripts/vscode/Invoke-MSTestWithCoverage.ps1` is the actual runner. Entry function
`Invoke-MSTestWithCoverageMain` at `:248`.

**Parameters** (`:1-13`):

| Parameter | Type | Default |
| --- | --- | --- |
| `-SearchRoot` | string | `'.'` (resolved as `$repoRoot\.` at `:271-272`) |
| `-Configuration` | string | `'Debug'` |
| `-CoverageOutput` | string | `coverage\coverage.cobertura.xml`, joined to `$repoRoot` at `:308` |
| `-NoExecute` | switch | off; returns before collection at `:322-324` |

`$repoRoot = (Resolve-Path (Join-Path $ScriptRoot '..\..')).Path` (`:271`) — i.e. the **worktree**
root, not the canonical repo root.

**Verbatim invocation form** (the `-CoverageOutput` value is repo-relative and the parent directory
is created automatically at `:310-312`):

```powershell
pwsh -NoProfile -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 `
    -SearchRoot '.' `
    -Configuration 'Debug' `
    -CoverageOutput 'docs/features/active/2026-08-08-quickfiler-per-file-coverage-capstone-497/evidence/baseline/coverage-baseline.cobertura.xml'
```

### Test-assembly discovery (`:296-306`)

```powershell
$testAssemblies = @(Get-ChildItem -Path $resolvedSearchRoot -Recurse -Filter '*.Test.dll' |
        Where-Object {
            $_.FullName -match "\\bin\\$Configuration\\" -and
            $_.FullName -notmatch '\\obj\\' -and
            $_.FullName -notmatch '\\ref\\'
        } |
            Select-Object -ExpandProperty FullName)
```

All discovered assemblies are passed to a **single** `dotnet-coverage collect` invocation
(`Get-DotnetCoverageArgumentList`, `:70-77`), so this runner already satisfies the
"run ALL `*.Test.dll` together" requirement. The composed command is:

```
dotnet-coverage collect --output <OUT> --output-format cobertura --settings <derived>.effective-coverage.config
    -- <vstest.console.exe> <asm1> <asm2> … /Settings:scripts/vscode/TaskMaster.cli.runsettings /InIsolation /TestCaseFilter:TestCategory!=LiveOutlook
```

Note the inner `/Settings:` is `TaskMaster.cli.runsettings` (MSTest `ClassLevel` parallelization
only, **no** Code Coverage data collector — verified: the file is 9 lines and contains no
`DataCollectionRunSettings`). Instrumentation comes solely from the outer `dotnet-coverage
--settings`, whose canonical source is repo-root `coverage.config` (7 `ModulePath` excludes: Deedle,
FSharp, Castle.Core, FluentAssertions, Moq, Microsoft.Testing, MSTest) plus one derived exclusion
`.*\.Test\.dll$` injected in memory by `ConvertTo-DerivedCoverageSettingsXml` (`:99-113`). The
derived file is written next to the output and removed in a `finally` (`:238-242`).

### Stale builds under `.claude/worktrees/`

The `Where-Object` filter has **no** exclusion for `.claude`. However:

- `$resolvedSearchRoot` is `<worktree>\.`, and `.claude/worktrees/` **does not exist inside this
  worktree** (verified: a glob for `.claude/worktrees/*` returns no files). Recursion is therefore
  self-contained and this specific hazard does not fire when the runner is invoked from a worktree.
- The hazard is real when the runner is invoked from the **canonical repo root**
  `C:\Users\DanMoisan\repos\TaskMaster`, where `.claude\worktrees\` holds agent worktrees with their
  own `*/bin/Debug/*.Test.dll`.

If manual discovery is used instead of the runner, the exclusion to add is a third
`-notmatch '\\\.claude\\'` clause alongside the existing `\\obj\\` and `\\ref\\` clauses. Nothing in
the repository currently implements it, and adding it to `Invoke-MSTestWithCoverage.ps1` would be a
production change outside the capstone's "no new production files / verify only" mandate — the
capstone should instead invoke the runner from inside its own worktree, where the hazard is absent.

A second, quieter instance of the same pattern: `Get-KoverageProjectAllowlist`
(`Helpers.ps1:11-16`) recurses `$RepoRoot` for `*.csproj`/`*.vbproj`/`*.fsproj`, excluding only
`\bin\`, `\obj\`, `\packages\`. From the canonical root it would also enumerate worktree csproj
files. The effect is benign (identical assembly names, deduped into a `HashSet`), but it confirms
`.claude/worktrees/` is nowhere excluded in this toolchain.

### Post-processing — what it strips and what it corrupts

`ConvertTo-KoverageCoberturaXml` (`Helpers.ps1:294-357`) does four things, in order:

1. **Removes every `<package>` whose `name` is not in the allowlist** (`:318-322`). The allowlist is
   the set of `AssemblyName` values across repo project files, **minus** anything ending `.Test`
   (`:39-41`). Vendored packages are therefore stripped **automatically** — there is no separate
   post-processing step to configure or invoke.
2. **Rewrites `filename` to repo-relative with the native separator** (`:324-326`).
3. **Merges `<class>` elements sharing a `filename`** (`:328`) — the #478 site.
4. **Recomputes root `line-rate` / `branch-rate` / `lines-covered` / `lines-valid` /
   `branches-covered` / `branches-valid`** (`:341-347`) — the #441 site.

**Verified against the two committed #424 artifacts.** The raw baseline has 14 `<package>` elements;
the post-processed final has 9. The five removed are exactly the vendored set named in the epic:

`log4net`, `Mono.Reflection`, `Microsoft.IO.RecyclableMemoryStream`, `System.Interactive`,
`System.Linq.Async`.

The 9 retained: `QuickFiler`, `UtilitiesCS`, `TaskVisualization`, `SVGControl`, `ToDoModel`, `Tags`,
`TaskMaster`, `TaskTree`, `VBFunctions`.

### The 70.19% → 85.65% swing is now fully explained, and the epic's stated cause is wrong

The epic (`## The repository-wide comparison must be like-for-like`) attributes the swing to two
causes, the second being "different instrumented scope … `lines-valid` *rises* from 79,957 to
110,849 … the two runs did not even instrument the same body of code." **That second explanation is
refuted.** The real cause is that the two artifacts are at different stages of the same pipeline:

| | `evidence/baseline/coverage-baseline.cobertura.xml` | `evidence/qa-gates/coverage-final.cobertura.xml` |
| --- | --- | --- |
| `<sources>` element | **absent** | **present** (`<source>.</source>`) |
| `filename` form | absolute (`C:\Users\…\TaskMaster-wt\2026-08-04T18-38\QuickFiler\…`) | repo-relative (`QuickFiler\Controllers\…`) |
| root `line-rate` precision | full double (`0.7019272859161799`) | 6-decimal (`0.856453`) |
| `<package>` count | 14 | 9 |
| `lines-valid` | 79957 | 110849 |

Those three markers (`<sources>`, relative paths, 6-decimal rounding) are the exact signature of
`ConvertTo-KoverageCoberturaXml`. **The baseline is raw `dotnet-coverage` output; the final is
post-processed.**

And the `lines-valid` growth is the #441 double count, proven arithmetically: a literal count of
`<line number=` elements in `coverage-final.cobertura.xml` returns **110849**, byte-identical to its
`lines-valid="110849"`. Because Cobertura nests each executable line twice (once under
`class/methods/method/lines`, once in the `class/lines` rollup — verified structurally at
`coverage-final.cobertura.xml:162923-162943`, where a class carries one method line and a 10-line
class-level rollup), the descendant axis returns roughly twice the distinct count. The raw baseline's
79,957 is `dotnet-coverage`'s own, correct figure.

**Rule the capstone must follow.** The before/after pair is invalid unless **both** artifacts are
produced by the same complete pipeline. Concretely:

- Capture both with the same `Invoke-MSTestWithCoverage.ps1` invocation form (which always
  post-processes; there is no `-NoPostProcess` switch).
- Do **not** compare a raw artifact to a post-processed one. `<sources>` presence is a one-glance
  discriminator.
- Because the post-processed root attributes are #441-corrupted, the honest repo-wide figure is
  **recomputed** from the artifact: union `./lines/line` (direct children of `<class>` only) across
  all classes sharing a `filename` within a package, dedupe by `@number` taking `MAX(@hits)`, then
  `covered/total`. Applying the identical recomputation to both artifacts gives a self-consistent
  and *correct* pair, satisfying the epic's "identical command and identical post-processing" rule
  strictly. Reporting the raw `line-rate` attribute alongside it, labelled as the tool's own
  (defective) figure, is optional but should be marked as such.

### What `.github/workflows/ci.yml` actually does

Verified, `ci.yml:118-160`. CI does **not** use `Invoke-MSTestWithCoverage.ps1`, does **not** produce
Cobertura, and applies **no** coverage gate.

- Discovery (`:134-140`) — identical filter logic to the runner:
  `Get-ChildItem -Path $env:GITHUB_WORKSPACE -Recurse -Filter '*.Test.dll'` where
  `FullName -match "\\bin\\$($env:BUILD_CONFIGURATION)\\"` and not `\\obj\\` and not `\\ref\\`.
  `BUILD_CONFIGURATION` is `Debug` (`:45`).
- Command (`:147`): `& $vstestPath $testAssemblies /EnableCodeCoverage /InIsolation /Logger:trx /TestCaseFilter:"TestCategory!=LiveOutlook"`
- vstest resolution (`:124-132`): `vswhere.exe` at
  `${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe`, then
  `-latest -products * -find 'Common7\IDE\Extensions\TestPlatform\vstest.console.exe'`.
- Output: `TestResults/**/*.trx` and `TestResults/**/*.coverage` uploaded as an artifact (`:152-160`).
  Binary `.coverage`, never converted.
- CI has exactly two jobs: `actionlint` (ubuntu-latest) and `quality-gates` (windows-latest).
  CI does **not** run Pester.

**Important divergence.** CI's `/EnableCodeCoverage` path auto-detects repo-root
`TaskMaster.runsettings` (which carries the same 7 module excludes plus `ClassLevel`
parallelization), whereas the local runner uses `dotnet-coverage` + `coverage.config` +
`TaskMaster.cli.runsettings`. These are two different instrumentation engines. CI's assembly **set**
is what the capstone should mirror; CI's **command** is not the one that produces the repo's
Cobertura evidence. Do not cite a CI figure — none is emitted.

---

## Q4 — Toolchain command forms, verified against this checkout

### CSharpier

- The manifest is at **repository root `dotnet-tools.json`**, not `.config/dotnet-tools.json` (a glob
  for `**/dotnet-tools.json` returns exactly one result at the root). Contents verified:

```json
{ "version": 1, "isRoot": true,
  "tools": { "csharpier": { "version": "1.2.6", "commands": ["csharpier"], "rollForward": false } } }
```

- **v1.2.6 confirmed.** The bare `csharpier .` form in `CLAUDE.md` §C#1/§CUT3 and in
  `.claude/rules/csharp.md:14` is the v0 syntax and is stale. A subcommand is required.
- The **CI-proven** form is `dotnet csharpier check .` (`ci.yml:93`), preceded by
  `dotnet tool restore` (`ci.yml:89`). Since CI is green on `main`, this pair is known to work
  against the root-located manifest.
- `dotnet tool run csharpier format .` (mutating) and `dotnet tool run csharpier check .`
  (non-mutating) are the equivalent explicit-manifest forms. Both invoke the same local tool.
- **The csproj-churn hazard recorded in older agent memory is now closed.** `.csharpierignore`
  (verified, 15 lines) excludes `*.csproj`, `*.props`, `*.targets`, `**/evidence/**`,
  `*.cobertura.xml`, `*.coverage`, `*.coveragexml`, `*.trx`. A repo-wide `format .` will therefore
  no longer rewrite project files. This matters directly to the capstone, which must not perturb
  `QuickFiler.csproj`.

### MSBuild analyzer and nullable builds

Solution file: `TaskMaster.sln` at repository root (`ci.yml:44` `SOLUTION_PATH: TaskMaster.sln`).

CI-proven forms, `ci.yml:98-116` (note CI uses `/t:Rebuild` for the nullable gate and documents why):

```powershell
# analyze
msbuild TaskMaster.sln /t:Build /m /p:Configuration=Debug "/p:Platform=Any CPU" `
    /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true

# nullable / type-check  (CI uses /t:Rebuild, not /t:Build)
msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" `
    /p:TreatWarningsAsErrors=true
```

Two deviations from `CLAUDE.md` §CUT3 that CI itself documents at `ci.yml:106-112`:

1. CI uses `/t:Rebuild` because "MSBuild's incremental up-to-date check does not invalidate on this
   command-line property change alone, so a plain `/t:Build` would silently skip recompilation and
   never enforce this gate."
2. CI does **not** pass `/p:Nullable=enable`; it relies on each file's own `#nullable enable` pragma
   plus `TreatWarningsAsErrors`. `CLAUDE.md` §CUT3 and `.claude/rules/csharp.md:16` both specify
   `/p:Nullable=enable`. Committed agent memory
   (`.claude/agent-memory/atomic-executor/project_build_test_env.md:22`) records that the solution-wide
   `/p:Nullable=enable` form emits `CS8630` on `QuickFiler.Test` (C# 7.3, no `<LangVersion>`) if that
   project actually recompiles under it, and that running the analyzer build **first** avoids it. The
   capstone should run the mandated order and expect this to be a non-issue, but should be prepared
   for CS8630 if it ever runs the nullable build in isolation.

`Platform` must be quoted as `"/p:Platform=Any CPU"` (with the space) for a solution build.

### vstest.console.exe and `.coverage` → Cobertura

- The `CLAUDE.md` §CUT3 form `vstest.console.exe <paths> /EnableCodeCoverage` produces a **binary
  `.coverage`** under `TestResults/<guid>/`. Nothing in `scripts/`, `.github/`, `.claude/`, or
  `.codex/` converts it. There is **no committed converter step**.
- The repository's Cobertura evidence is produced by a different path entirely —
  `Invoke-MSTestWithCoverage.ps1` → `dotnet-coverage collect --output-format cobertura` (Q3).
  **This is the form the capstone should use**, because it is the only one whose output matches every
  committed Cobertura artifact and F1's harness input contract.
- If a `.coverage` file must be converted (e.g. to reconcile against a CI run), committed agent
  memory records two working converters:
  `dotnet-coverage merge <file>.coverage --output out.cobertura.xml --output-format cobertura`
  (`.claude/agent-memory/orchestrator/feedback_repowide_coverage_run_full_suite.md:25`;
  `.claude/agent-memory/atomic-executor/project_build_test_env.md:23`), and
  `Microsoft.CodeCoverage.Console.exe merge <file> -f xml -o out.xml` (same file, `:15`). The same
  memory records that `CodeCoverage.exe analyze` is deprecated and fails. **These are second-hand
  claims from other sessions and were not re-executed here.**

### PATH availability and resolved paths

**Not verifiable in this session — no shell tool.** What *is* verifiable from the checkout:

- Every repo script resolves MSBuild and vstest through **vswhere**, not PATH:
  `Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'` —
  `Invoke-MSTestWithCoverage.ps1:279`, `Invoke-MSTest.ps1:97`, `Invoke-VSBuild.ps1:127`,
  `ci.yml:124`. Then:
  - vstest: `& $vswherePath -latest -products * -find 'Common7\IDE\Extensions\TestPlatform\vstest.console.exe'`
  - MSBuild: `& $vswherePath -latest -requires Microsoft.Component.MSBuild -find 'MSBuild\**\Bin\MSBuild.exe'`
- `dotnet-coverage` is required to be **on PATH**: `Invoke-MSTestWithCoverage.ps1:292-294` throws
  `'dotnet-coverage not found. Install it with: dotnet tool install --global dotnet-coverage'` if
  `Get-Command 'dotnet-coverage'` fails. So it is a **global** tool, not in the local manifest.
- csharpier is a **local** manifest tool, so it is invoked through `dotnet`, never as a bare
  executable.

Committed agent memory (`.claude/agent-memory/atomic-executor/project_vs18_build_toolchain_paths.md:13`
and `project_build_test_env.md:13-14`) records that msbuild and vstest are **not** on PATH on this
host and gives these resolved absolute paths:

- `C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\amd64\MSBuild.exe`
  (also cited without `amd64\` in the second memory)
- `C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe`
- `C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\Microsoft\CodeCoverage.Console\Microsoft.CodeCoverage.Console.exe`

**These paths were not confirmed to exist in this session.** Prefer the vswhere resolution the repo
scripts already implement; fall back to these literals only if vswhere fails.

---

## Q5 — Cobertura semantics traps

All four claims tested against
`docs/features/active/2026-08-06-quickfiler-high-confidence-queue-init-stall-424/evidence/qa-gates/coverage-final.cobertura.xml`
(post-processed) and `…/evidence/baseline/coverage-baseline.cobertura.xml` (raw).

### 5.1 Multiple `<class>` per `filename`, unioned with MAX HITS — **CONFIRMED, with a critical caveat**

The mechanism is real: `Merge-CoberturaClassesByFilename` exists precisely because raw
`dotnet-coverage` emits a `<class>` per (type, source-file) pair, so a type plus its generated
`<>c` / `<>c__DisplayClass` closure produce two `<class>` elements sharing one `filename`. The
correct aggregation is a union keyed on `@number` with `MAX(@hits)` — implemented at
`Helpers.ps1:217-268`, and independently required by `epic.md` `### Two harness correctness
requirements` item 1 and F1 `spec.md:337-339`.

**Caveat the capstone must internalise: in a POST-PROCESSED artifact the merge has already run.** In
`coverage-final.cobertura.xml`, `<class>` elements with a `filename` beginning `QuickFiler\` number
exactly **70** — one per distinct file, no duplicates. Only **2** `<>c`-named `<class>` elements
survive anywhere in the document, and both are in other packages where their filename group had a
single member:

- `:162923` `name="TaskVisualization.FlagTasks.&lt;&gt;c" filename="TaskVisualization\FlagTasks.cs"`
- `:185839` `name="TaskMaster.AppOlObjects.&lt;&gt;c__DisplayClass121_0" filename="TaskMaster\AppGlobals\AppOlObjects.StoreRehook.cs"`

So the union step is **idempotent** on a post-processed report (correct to perform, but it will find
nothing to merge for QuickFiler) and **essential** on a raw one. The capstone must implement it
regardless, because it cannot know which stage an inherited artifact came from without checking for
`<sources>`.

Second caveat: the merge is **lossy in the `<methods>` subtree** — non-primary group members'
`<methods>` are discarded (`Helpers.ps1:200`, `:285-289`). Any capstone computation that reads
method-level lines from a post-processed artifact is reading an incomplete set. Read only
`./lines/line`.

### 5.2 Key the denominator on `<line>` child COUNT, never on `line-rate` — **CONFIRMED as a rule; the stated failure mode was NOT observed**

The rule is correct and must be followed, but the specific justification in `epic.md` — "a
declaration-only file reports `line-rate="0"` because it has no lines" — does not describe what this
repository's reports actually contain.

Observed facts:

- A search for `<lines />` (an empty class-level lines block) in `coverage-final.cobertura.xml`
  returns **zero matches**. There is no present-with-zero-lines `<class>` anywhere in the document.
- Declaration-only files are **absent entirely**, not present at 0%. 13 of the 14
  `QuickFiler\Interfaces\*.cs` files produce no `<class>` element. The one that does is
  `MailItemActionsAdapter.cs` at `:14448`
  (`line-rate="1" branch-rate="1" name="QuickFiler.Interfaces.MailItemActionsAdapter"`) — an
  implementation class, which serves as the positive control proving the `Interfaces\` folder was
  instrumented.
- `QuickFiler\Properties\Settings.Designer.cs` is **present** at `line-rate="0"` (`:14426`) and does
  carry real `<line>` children (`:14430-14432`, three lines at `hits="0"`). It is genuinely 0%
  covered, not a zero-line file. It is therefore **not** an example of the trap.

So the correct capstone statement is: **`line-rate="0"` in this repository's reports means genuinely
uncovered.** The zero-`<line>` state is defensive and was not observed. The *real* ambiguity lives in
5.4 below (absence), not in `line-rate="0"`.

Independent reason to distrust `line-rate` regardless: on a merged class it is the #478 blend.
Worked example, verified — `QuickFiler\Controllers\QfcHomeController.Iteration.cs` at `:22612` carries
`line-rate="0.8625"`, which #478 shows is `69/80` (union lines + primary-only method lines) while the
true class-level-union figure is `45/56 = 80.36%`. A capstone reading the attribute would record
86.25% where the truth is 80.36% — on a file whose gate is 80%.

### 5.3 Branch coverage from `condition-coverage` — **CONFIRMED**

Structure verified at `coverage-final.cobertura.xml:162958-162962`:

```xml
<line number="16" hits="1" branch="True" condition-coverage="75% (3/4)">
  <conditions>
    <condition number="0" type="jump" coverage="100%" />
    <condition number="1" type="jump" coverage="50%" />
  </conditions>
</line>
```

`branch` takes the literal capitalised values `"True"` / `"False"` (matched literally in production
code at `Helpers.ps1:128` and `:236`). `condition-coverage` has the form `"<pct>% (<covered>/<total>)"`.

**Exact per-file branch percentage recipe:**

1. Build the deduplicated class-level line set for the `filename` (union `./lines/line` across all
   `<class>` sharing the filename; on a line-number collision take `MAX(@hits)` and retain the
   **richer** `condition-coverage` — larger denominator, and on equal denominators larger numerator;
   this mirrors `Helpers.ps1:240-261`).
2. Restrict to elements with `@branch = "True"`.
3. Parse each `@condition-coverage` with `\(([0-9]+)/([0-9]+)\)` (same regex as
   `Get-CoberturaLineConditionCoverageParts`, `Helpers.ps1:154`).
4. `branchRate = sum(covered) / sum(total)`. **Not** the mean of per-line percentages — that would
   weight a 2-condition line equally with an 8-condition line.
5. A `branch="True"` line with **no** `condition-coverage` attribute contributes nothing to either
   sum; do not infer a fraction.
6. `sum(total) == 0` → report `n/a`, never `0%`, and never a branch failure.

**A file can pass line and fail branch — confirmed by the epic's own table**, which lists 12 such
files (e.g. `EfcHomeController.Timing.cs` at 100.0% line / 66.7% branch;
`QfcThemeControlSet.cs` at 100.0% line / 53.3% branch). Both gates are independent: 80.0% line
(issue #136 AC1) and 75.0% branch (`.claude/rules/general-unit-test.md`). Compare unrounded
(`-lt 0.80`, `-lt 0.75`); display rounded to one decimal under `InvariantCulture`.

Note: because `Get-CoberturaCoverageSummary` computes the root `branch-rate` over the same
double-counting descendant axis, the **root** `branch-rate` is also #441-affected. Branch sums are
less distorted than line counts (a branch line contributes its `(c/t)` pair once per occurrence, so
the ratio is preserved when duplication is uniform) but the absolute `branches-valid` is inflated.
Recompute rather than transcribe.

### 5.4 An `[ExcludeFromCodeCoverage]` file is ABSENT from the report — **CONFIRMED**

Verified for the three largest exempted families. Searching `coverage-final.cobertura.xml` for
`filename="[^"]*(QfcDatamodel|QfcCollectionController|ItemViewer)[^"]*"` returns only:

```
QuickFiler\Helper Classes\ItemViewerQueue.cs
QuickFiler\Viewers\ItemViewerExpanded.Designer.cs
QuickFiler\Viewers\ItemViewerExpanded.cs
QuickFiler\Viewers\BreadcrumbItemViewerLifecycleCoordinator.cs
```

No `QfcDatamodel.cs`, no `QfcDatamodel.FrameBuilding.cs`, no `QfcDatamodel.QueueProcessing.cs`, no
`QfcCollectionController.cs`, no `ItemViewer.cs`, no `ItemViewer.Designer.cs`. All are absent. The
8 remaining string occurrences of `QfcDatamodel`/`QfcCollectionController` in the document are
method-signature type references, not `filename` attributes.

**Absence is genuinely three-way ambiguous**, and the capstone must disambiguate from source, not
from the report:

| Cause of absence | Count in the 51 absent files (F1's decomposition) | How to detect |
| --- | --- | --- |
| `[ExcludeFromCodeCoverage]` on the type or on a **partial of the type** | 24 | grep the file **and every partial of its type** for the attribute |
| Interface/declaration-only, no executable IL | 23 | read the file — `interface`/`enum` only |
| Enum-only (`Helper Classes\QfEnums.cs`) | 1 | read the file |
| Entirely commented out (`Helper Classes\cInfoMail.cs`) | 1 | read the file |
| Assembly attributes only (`Properties\AssemblyInfo.cs`) | 1 | read the file |
| `[DebuggerNonUserCodeAttribute]` on generated code (`Properties\Resources.Designer.cs`) | 1 | grep for `DebuggerNonUserCode` |

51 absent + 70 present = 121. **The 70 figure is verified independently in this session** (70
`filename="QuickFiler\` occurrences in the post-processed artifact); the 51-way decomposition is
F1's research figure, adopted here but not re-derived.

Two traps inside the trap:

- **Partial-class propagation.** A type-level attribute on one partial suppresses **every** partial
  of that type. Confirmed by F1 at `QuickFiler/Controllers/QfcDatamodel.cs:25` (suppresses 3 files)
  and `Viewers/ItemViewer.cs:20` (suppresses 7, including the 6,224-line Designer). A per-file grep
  for the attribute will therefore *under*-report suppression. Grep by **type**, not by file.
  A type may be annotated only once (CS0579 otherwise).
- **`DebuggerNonUserCode` is a sixth absence cause** with no attribute of its own to find. It
  affects `Properties\Resources.Designer.cs` and is why that file must not be classified
  `interface-only`.

---

## Q6 — Capstone-owned reconciliation tooling

### Language: **PowerShell.** Python is not viable here.

Evidence:

- A repo-wide glob for `**/*.py` returns exactly **two** files, both inside an archived feature
  folder (`docs/features/archive/2026-07-18-stale-app-config-binding-redirects-354/scripts/fix_binding_redirects.py`
  and its test). **`scripts/` contains no Python at all.**
- There is **no Python CI job**. `.github/workflows/` contains exactly `ci.yml` and
  `codex-web-setup-test.yml`; `ci.yml` has two jobs, `actionlint` (ubuntu) and `quality-gates`
  (windows). Nothing formats, lints, type-checks, or tests Python.
- There is no `.claude/rules/python.md`; there **is** `.claude/rules/powershell.md` with a full
  toolchain, change budget, seam hierarchy, and testing standards.
- `.claude/rules/general-code-change.md` mandates a seven-stage toolchain loop. A Python script would
  have **no** repo-approved formatter, linter, type-checker, or test runner wired, so it could not
  satisfy that loop. **Adding a Python script would be ungated by any existing CI job** — which is
  itself the reason not to add one, not a licence to.
- `.claude/rules/orchestrator-state.md` references `scripts/dev_tools/*.py` validators, but that
  directory does not exist in this checkout — those references describe a validator surface reached
  through MCP, not committed repo scripts.

So a capstone reconciliation script must be **PowerShell**, matching F1's harness.

### Where it belongs

Production script: `scripts/vscode/` — this is where every existing repo-tooling script lives
(`Invoke-MSTestWithCoverage.ps1`, `Invoke-MSTest.ps1`, `Invoke-VSBuild.ps1`,
`Install-RepoDotNetSdk.ps1`, `Invoke-Restore.ps1`, `Sync-PackageReferences.ps1`,
`TestProcessCleanup.ps1`) and where F1 places `Get-PerFileCoverage.ps1`. There is no
`scripts/dev-tools/` or `scripts/powershell/` directory in this checkout (both globs return nothing).

### Mirrored test path

`.claude/rules/general-unit-test.md` § Test File Location requires `tests/` to mirror the production
tree; `.claude/rules/powershell.md:57-58` adds the `*.Tests.ps1` suffix. The existing tree confirms
the convention exactly:

```
scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1
  -> tests/scripts/vscode/Invoke-MSTestWithCoverage.Helpers.Tests.ps1
```

`tests/scripts/vscode/` currently holds four suites: `Install-RepoDotNetSdk.Tests.ps1`,
`Invoke-MSTest.RunSettings.Tests.ps1`, `Invoke-MSTestWithCoverage.Helpers.Tests.ps1`,
`Invoke-VSBuild.Tests.ps1`. So a capstone script at `scripts/vscode/<Name>.ps1` requires tests at
**`tests/scripts/vscode/<Name>.Tests.ps1`**. Colocation in `scripts/` is prohibited.

### Is a Pester harness wired for `scripts/`?

**Partly, and the coverage half is not.** Two separate answers:

- **Discovery/execution: yes.** The repo-mandated command is the MCP tool
  `mcp__drm-copilot__run_poshqc_test` (`.claude/rules/powershell.md:18`, which also says "Use the
  MCP server functions; do not substitute VS Code task wrappers"). F1's plan `[P0-T9]` establishes
  that the four existing `tests/scripts/vscode/*.Tests.ps1` suites **do** appear as `<testsuite>`
  entries in `artifacts/pester/pester-junit.xml`, i.e. `tests/scripts/vscode/` is inside the PoshQC
  Pester discovery set.
- **Coverage: no.** `.claude/rules/powershell.md:18` points at
  `scripts/powershell/PoshQC/settings/pester.runsettings.psd1` as the repo config — **that path does
  not exist** (verified: a glob for `scripts/powershell/**` returns no files). F1's plan
  `## Evidence Rules` records that PoshQC therefore falls back to its bundled config, whose
  `CodeCoverage.Path` allow-list resolves in this repository to `.claude/hooks/`, `.claude/lib/`, and
  `.codex/hooks/` **only** — so `artifacts/pester/powershell-coverage.xml` contains **no
  `scripts/vscode/` sourcefile** and cannot report coverage for a capstone script.

The documented workaround, proven feasible in F1's plan `[P0-T12]` against Pester 5.6.1, is a
supplementary direct `Invoke-Pester` run scoped to the target file. The exact command shape (outer
single quotes, inner double quotes, so neither Bash nor PowerShell expands `$c`/`$r`/`$(...)` before
`pwsh` receives it):

```
pwsh -NoProfile -Command '$c = New-PesterConfiguration; $c.Run.Path = "tests/scripts/vscode"; $c.Run.PassThru = $true; $c.CodeCoverage.Enabled = $true; $c.CodeCoverage.Path = @("scripts/vscode/<Name>.ps1"); $c.CodeCoverage.OutputFormat = "JaCoCo"; $c.CodeCoverage.OutputPath = "<evidence path>.xml"; $r = Invoke-Pester -Configuration $c; "PASSED=$($r.PassedCount) FAILED=$($r.FailedCount)"'
```

Run with the current directory at the worktree root (all paths are repo-relative). Emitted JaCoCo
carries `<class sourcefilename=...>` with a `LINE` counter and **no** `BRANCH` counter — so the
`.claude/rules/powershell.md:64` 75% branch floor is not measurable for PowerShell in this repo.
That gap is pre-existing and recorded, not something the capstone can close.

### Does the capstone actually need its own script?

F1's harness answers "is every ledgered `testable` file at >= 80% line and >= 75% branch?" It also
already implements the two reconciliation directions the epic cares about — an `UNLEDGERED` row state
(present in the report but not in the ledger) and a `NO DATA` row state (ledgered `testable` but
absent from the report), both exit code `1` per `spec.md:476`. F1's plan `[P10-T19]` is a
**csproj-completeness assertion** and `[P10-T20]`/`[P10-T21]` are csproj-drift fixture cases.

So the csproj↔ledger reconciliation the epic assigns to F16 (`## Mid-Wave File Creation`, rule 5)
may already be covered by F1's `tests/scripts/vscode/QuickFilerCoverageLedger.Tests.ps1` **as a Pester
assertion rather than as a runnable report**. The capstone should read the delivered F1 artifacts
first and only build new tooling for the genuine residue:

- re-deriving the compile set at capstone time and diffing against the ledger (may be satisfied by
  F1's Pester suite, but that suite proves a property rather than emitting evidence);
- the **repository-wide before/after recomputation** from Q3, which nothing in F1's scope covers —
  F1's harness selects `<package name="QuickFiler">` by name and fails if absent (`spec.md:352-354`),
  so it cannot produce a repo-wide figure at all. **This is the one capstone-owned computation that
  is definitely not delivered by any sibling.**

---

## Findings the capstone should treat as Blocking or near-Blocking

1. **The repository-wide figure has no correct producer in the repository.** Every path to it
   (`Get-CoberturaCoverageSummary`, the root attributes it writes, `temp-extract-coverage.ps1`, the
   JaCoCo feature-review hook) is defective or wrong-format, and F1's harness is package-scoped to
   QuickFiler by design. Issue #136 AC8 therefore cannot be closed by transcribing any existing
   tool's output. The capstone must recompute from `./lines/line`, identically on both artifacts.
2. **#441 and #478 remain open in `Invoke-MSTestWithCoverage.Helpers.ps1` and F1 does not fix
   either.** F1 sidesteps them; it does not close them. Every committed Cobertura artifact in this
   repository has a corrupted root `lines-valid`/`line-rate` and corrupted merged-class `line-rate`
   attributes. The capstone must state this explicitly in its evidence rather than silently
   recomputing, so a reviewer can tell the two numbers apart.
3. **The epic's own explanation of the 70.19% → 85.65% swing is factually wrong** ("different
   instrumented scope"). The actual cause is raw-vs-post-processed comparison plus the #441 double
   count, provable in one glance from the presence/absence of `<sources>`. If the capstone repeats
   the epic's explanation it will propagate an error into the closing evidence.
4. **`epic.md`'s "Measured Coverage Baseline" tables are quantitatively unusable as targets.** They
   are indicative-only by the epic's own statement, and their `Lines` column is derived from a
   corrected recomputation while their source artifact's attributes are not. The capstone must
   re-measure, never transcribe.
5. **F1's `exempt_ground` enum as specified has three values and the epic later ratified a fourth
   ground** (prohibited-to-execute adapters). If F1's delivered ledger did not widen the enum,
   `WebView2CoreInitializer.cs` has no valid `exempt_ground` value to carry. Check the delivered
   `coverage-ledger.json` schema before assuming.

---

## Unverified — requires execution at capstone time

These could not be established without a shell and must not be asserted as fact:

- Whether `msbuild`, `vstest.console.exe`, `csharpier`, `dotnet-coverage`, or `pwsh` resolve on this
  host, and whether the VS18 absolute paths cited from other agents' committed memory exist.
- The **current open/closed state** of issues #441 and #478. Both issue bodies were retrieved and are
  quoted accurately above, but `gh issue view` was not available and the state field was not read.
  If either has been fixed since filing, the Q2 conclusions about live defects change.
- The **actual delivered content** of F1's harness and ledger. Neither exists on this branch. Every
  F1 statement above is from `spec.md` / `plan.2026-08-07T20-41.md` and describes intent, not
  delivery. At capstone time, re-read `scripts/vscode/Get-PerFileCoverage.Helpers.ps1` and
  `docs/features/epics/quickfiler-per-file-coverage/coverage-ledger.json` directly.
- The **compile-set count at capstone execution time**. 121 is this branch's figure. F2, F3, F7, F9,
  and F11 all plan to add `<Compile Include>` entries, so the number will be higher after fan-in. The
  denominator is dynamic by epic rule; re-derive, do not assume 121.
- Whether the current `main`/integration head still has 70 instrumented QuickFiler files. The 70/51
  split is measured from feature #424's artifact, captured on a different branch.
- Whether `dotnet-coverage`'s instrumented module set is stable between runs. Committed agent memory
  (`.claude/agent-memory/atomic-executor/project_dotnet_coverage_denominator_nondeterminism.md`,
  not read in full) and #424's own evidence (`coverage-delta.2026-08-07T00-48.md:65`) both assert
  denominator instability. Since the 79,957→110,849 growth is now fully explained by post-processing,
  that instability claim is **unconfirmed** and may itself be a misreading of the same artifact pair.
  If it is real, the before/after pair needs a same-session capture to be defensible.
