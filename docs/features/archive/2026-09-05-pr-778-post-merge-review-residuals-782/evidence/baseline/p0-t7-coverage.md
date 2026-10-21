# Baseline — Coverage (P0-T7, re-recorded under SD23)

SUPERSEDED BASELINE RE-RECORDED: SD23

RE-ANCHORED BASE: 736c2cf2

Timestamp: 2026-09-05T21-59

Amended: 2026-09-06T00-15

The amendment corrects the identification of this artifact's input document. It records both
baseline collections with their own inputs and their own figures, states which of the two is
authoritative and on what grounds, and states that the authoritative collection's output document is
not present in this worktree. It does not change any recorded figure: the authoritative first-party
counters remain 112355 lines covered and 26500 branches covered, and the counters a reader obtains
from the retained document remain 112359 and 26496. The amendment is recorded under issue #782 and is
the remediation of finding R4 of the feature review.

## Why the earlier figures are superseded

An external actor rebased the feature branch from `a007f72e` onto `origin/main` at `77c6d314`
during execution. Every prior commit received a new SHA. The base commit the superseded record was
taken at, `b95a5252`, is orphaned and is no longer an ancestor of HEAD, so the figures it carried
describe a tree that is no longer this branch's baseline.

The superseded first-party figures were line **112359/132967 = 84.50%** and branch
**26496/33480 = 79.14%**. The re-measured figures are line **112355/132967 = 84.50%** and branch
**26500/33480 = 79.15%**. The denominator is unchanged by SD23; only the covered counters moved.

## Measurement method and measuring party

This gate was measured by the **orchestrator, not the executor**, at the re-anchored base commit
`736c2cf2`, by the temporary-restore method: the orchestrator restored the six Write Set source
files Phase 1 has changed so far — `UtilitiesCS/Threading/UiThread.cs`,
`UtilitiesCS/OutlookObjects/Folder/WpfDispatcherYield.cs`, `UtilitiesCS/Threading/ProgressTracker.cs`,
`UtilitiesCS/Threading/ProgressTrackerAsync.cs`, `TaskMaster/Ribbon/RibbonViewer.EngineCommands.cs`,
and `UtilitiesCS.Test/Threading/UiThread_Tests.cs` — to their `pre-782-base` content with
`git checkout pre-782-base -- <those six paths>`, ran the four gates, restored those files to HEAD
in a `finally` block, and left the worktree clean and at HEAD afterwards.

The executor did **not** re-run the coverage collection for this task, and this artifact does not
present the figures as an executor run.

Command (the orchestrator's command):

```powershell
$derived = 'coverage\782-effective-coverage.config'
[xml]$cfg = Get-Content -LiteralPath 'coverage.config'
$excl = $cfg.Configuration.CodeCoverage.ModulePaths.Exclude
$node = $cfg.CreateElement('ModulePath'); $node.InnerText = '.*\.Test\.dll$'
$null = $excl.AppendChild($node); $cfg.Save((Join-Path (Get-Location) $derived))

dotnet-coverage collect --output coverage\782-p0-baseline.cobertura.xml --output-format cobertura `
    --settings coverage\782-effective-coverage.config -- $vstest `
    QuickFiler.Test\bin\Debug\QuickFiler.Test.dll `
    SVGControl.Test\bin\Debug\SVGControl.Test.dll `
    Tags.Test\bin\Debug\Tags.Test.dll `
    TaskMaster.Test\bin\Debug\TaskMaster.Test.dll `
    TaskTree.Test\bin\Debug\TaskTree.Test.dll `
    TaskVisualization.Test\bin\Debug\TaskVisualization.Test.dll `
    ToDoModel.Test\bin\Debug\ToDoModel.Test.dll `
    UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll `
    VBFunctions.Test\bin\Debug\VBFunctions.Test.dll `
    '/Settings:scripts\vscode\TaskMaster.cli.runsettings' '/InIsolation' '/Logger:trx' `
    '/ResultsDirectory:TestResults\782-p0-coverage' `
    '/Blame:CollectHangDump;TestTimeout=5min;HangDumpType=None' `
    '/TestCaseFilter:TestCategory!=LiveOutlook&FullyQualifiedName!~HelperClasses.ShellUtilities_Tests&FullyQualifiedName!~HelperClasses.ShellUtilitiesStatic_Tests&FullyQualifiedName!~HelperClasses.SysImageListHelperTests&FullyQualifiedName!~EmailIntelligence.OSBrowser_Tests'
```

**Amendment note on the input document named in the command above.** The `--output` argument
`coverage\782-p0-baseline.cobertura.xml` is a relative path, so the recorded command run from this
worktree root would have written or overwritten `coverage/782-p0-baseline.cobertura.xml` in this
worktree. It did not. Task [P0-T6] of the issue #782 remediation records that file's last write time
as `2026-09-05 19:26:55`, which precedes this artifact's `Timestamp: 2026-09-05T21-59`, and records
its companion log `coverage/782-p0-cov.txt` carrying `Total tests: 6992` rather than the `6997`
recorded at `evidence/baseline/p0-t6-vstest.md:71`. The test count is the discriminating observation
rather than the file timestamp, because it is a value the run itself wrote inside the log, whereas a
file timestamp is mutable filesystem metadata.

The retained document is therefore the earlier, superseded collection's output rather than the
re-measurement's. The re-measurement's own output document is not present in this worktree and
is treated as not retained. The reason for its absence is not established by any record this
artifact can cite, so no mechanism for it is asserted here.

The derived configuration is the repo-root `coverage.config` with one
`<ModulePath>.*\.Test\.dll$</ModulePath>` appended to `/Configuration/CodeCoverage/ModulePaths/Exclude`.
The `/Blame:` switch is written in single quotes so PowerShell does not truncate it at the first
semicolon. `/EnableCodeCoverage` is not passed; `dotnet-coverage` performs the instrumentation and
the two collectors conflict.

EXIT_CODE: 0

That is the exit code the **orchestrator** observed, not an exit code the executor observed.

## Counting method (SD22, load-bearing, unchanged by SD23; P7-T5 and P7-T7 must reproduce it)

The `<package>` elements in this Cobertura document carry `line-rate` and `branch-rate` attributes
but carry **no** `lines-covered`, `lines-valid`, `branches-covered`, or `branches-valid` attributes,
so those four figures are aggregated from `<line>` elements rather than read off the package, and
the denominator depends entirely on the selection used.

**The selection is the all-descendant `.//line` selection over each first-party `<package>`, and
only that one.** It reproduces the tabled first-party `lines-valid` of **132967** exactly, in the
superseded run and in the re-measured run alike. The fact that the same selection reproduces 132967
across both runs is itself the evidence that one selection was used both times.

Two narrower selections are **rejected by name and by figure** so a later reader cannot substitute
one:

| Rejected selection | Figure |
|---|---|
| `classes/class/lines/line` | 65899 |
| `classes/class/methods/method/lines/line` | 67068 |

**Both of those figures were measured against the superseded baseline document and were not
re-derived against the re-measured document.** They are recorded because the selections they name
are what must not be substituted, not because their numeric values are current.

The all-descendant selection counts a line both at class level and inside its method, so the
denominator is roughly twice the deduped one. That doubling is a property of the baseline method and
is preserved deliberately: the only requirement on it is that the baseline and the Phase 7 figure be
produced by one method and therefore be comparable.

A `<line>` counts as covered when its `hits` attribute is greater than zero. Branch figures are
summed from the `(numerator/denominator)` pair inside each `condition-coverage` attribute over the
same all-descendant line set.

The first-party allowlist is the nine production assembly names: `Tags`, `ToDoModel`,
`TaskVisualization`, `UtilitiesCS`, `QuickFiler`, `TaskTree`, `TaskMaster`, `SVGControl`,
`VBFunctions`. The document also contains vendored packages — `log4net`, `Mono.Reflection`,
`Microsoft.IO.RecyclableMemoryStream`, `System.Linq.Async`, `System.Interactive` — which are
excluded from the first-party figures. The repo-root `coverage.config` does not exclude vendored
assemblies, so this allowlist is what performs that stripping.

Output Summary:

### First-party figures, re-measured at `736c2cf2` (comparable to policy)

| Figure | Value |
|---|---|
| `lines-covered` | 112355 |
| `lines-valid` | 132967 |
| line percentage | 84.50% |
| `branches-covered` | 26500 |
| `branches-valid` | 33480 |
| branch percentage | 79.15% |

Aggregated by the all-descendant `.//line` selection pinned above, over only the `<package>`
elements whose name matches one of the nine first-party allowlist assembly names.

**Only the first-party figure is comparable to policy.** The root all-modules figure includes
vendored assemblies that this repository does not own and cannot be held to the coverage floor.

The `lines-valid` of **132967** is recorded here so the Phase 7 comparison in P7-T7 can test
comparability between the two runs' denominators.

The re-measurement supplied totals only. No per-package first-party breakdown was taken at the
re-anchored base, so none is recorded here; the superseded per-package table is not carried forward,
because its rows sum to the superseded totals rather than to the re-measured ones. P7-T6 derives its
per-package rows from the Phase 7 Cobertura document directly.

### The two baseline collections, their inputs, and which is authoritative

Two coverage collections were taken for this delivery's Phase 0 baseline. Both are recorded here with
their own input document and their own figures, so a reader who aggregates either document is not
contradicted by this artifact.

```text
BASELINE-AUTHORITATIVE-LINES-COVERED: 112355
BASELINE-AUTHORITATIVE-BRANCHES-COVERED: 26500
BASELINE-AUTHORITATIVE-OUTPUT-DOCUMENT: NOT-RETAINED
RETAINED-DOCUMENT-PATH: coverage/782-p0-baseline.cobertura.xml
RETAINED-DOCUMENT-LINES-COVERED: 112359
RETAINED-DOCUMENT-BRANCHES-COVERED: 26496
```

| Collection | Base commit | `lines-covered` | `branches-covered` | Output document | Reproducible today |
|---|---|---|---|---|---|
| Re-measurement, authoritative | `736c2cf2` | 112355 | 26500 | NOT RETAINED | No |
| Earlier collection, superseded | `b95a5252` | 112359 | 26496 | `coverage/782-p0-baseline.cobertura.xml` | Yes |

The re-measured figures are authoritative as this branch's baseline because they were taken at the
re-anchored base `736c2cf2`, which is this branch's actual base. The retained document's figures were
taken at the orphaned base `b95a5252` that the head of this artifact names, which is no longer an
ancestor of HEAD.

The denominators are identical across the two collections — `lines-valid` 132967 and `branches-valid`
33480 for both — which is the evidence that one counting selection produced both. The line percentage
is 84.50% for both. The branch percentage is 79.14% for the earlier collection and 79.15% for the
re-measurement.

The figures 112359 and 26496 are the orphaned-base measurement. They are correctly not used as this
branch's baseline side, and `evidence/qa-gates/p7-t7-changed-line-coverage.md` records at its
"Condition 2" section that neither of them is used. They are nonetheless the two figures a reader
obtains by aggregating the retained document, and they are recorded here for that reason rather than
suppressed.

### Root all-modules figures — not re-measured, and not carried forward as a baseline

**The re-measurement supplied no root all-modules figure.** The superseded run recorded root line
**70.34%** and root branch **59.18%**. Those two values are recorded here as **superseded** and are
explicitly **not** carried forward as though they had been re-measured at the re-anchored base.

No task in this plan consumes a root all-modules baseline: P7-T5 records the root figures from its
own run, and P7-T7 compares first-party figures only. The root element's counts are deduped, which is
why a root `lines-valid` is smaller than the first-party all-descendant `lines-valid` of 132967 even
though the first-party set is a subset of the modules; the two are produced by different counting
methods and must not be compared with each other.

### Test run

Each recorded test count belongs to a specific collection, and the two counts are attached to their
own collections here rather than presented as one figure.

- **The re-anchored re-measurement**, whose figures this artifact records as authoritative,
  corresponds to `Total tests: 6997`, `Passed: 6997`, `Failed: 0`, recorded in
  `evidence/baseline/p0-t6-vstest.md`. Both collections ran the same nine assemblies with the same
  local filter.
- **The earlier, superseded collection**, whose output document is the retained
  `coverage/782-p0-baseline.cobertura.xml`, corresponds to `Total tests: 6992`. That is the figure its
  companion log `coverage/782-p0-cov.txt` carries, as measured by task [P0-T6] of the issue #782
  remediation.

The difference between 6992 and 6997 is the discriminating observation for which collection wrote the
retained document, and it is independent of file timestamps.

Both counts are locally-filtered figures with the four shell-icon classes excluded, and neither is a
CI figure. CI runs those four classes and reports a larger total than either.

### Reproducing these figures

The `coverage/` directory is git-ignored by `.gitignore` at line 144, whose pattern `coverage/*`
re-includes only `coverage/.gitkeep`. The retained `coverage/782-p0-baseline.cobertura.xml`
is not committed evidence, and neither is any other document under that directory. A reader
reproducing anything below must obtain or regenerate the document locally.

#### The retained document's figures, 112359 and 26496

Aggregate `coverage/782-p0-baseline.cobertura.xml` with the all-descendant selection this artifact
pins under SD22:

```powershell
$CoberturaPath = 'coverage\782-p0-baseline.cobertura.xml'
$doc = New-Object System.Xml.XmlDocument
$doc.Load((Resolve-Path -LiteralPath $CoberturaPath).Path)
$firstParty = @('Tags','ToDoModel','TaskVisualization','UtilitiesCS','QuickFiler','TaskTree','TaskMaster','SVGControl','VBFunctions')
$lc = 0; $lv = 0; $bc = 0; $bv = 0
foreach ($pkg in $doc.SelectNodes('/coverage/packages/package')) {
    if ($firstParty -notcontains $pkg.GetAttribute('name')) { continue }
    foreach ($ln in $pkg.SelectNodes('.//line')) {
        $lv++
        $h = $ln.GetAttribute('hits')
        if ($h -and [int]$h -gt 0) { $lc++ }
        $cc = $ln.GetAttribute('condition-coverage')
        if ($cc -and $cc -match '\((\d+)/(\d+)\)') { $bc += [int]$Matches[1]; $bv += [int]$Matches[2] }
    }
}
"LINES_COVERED=$lc LINES_VALID=$lv BRANCHES_COVERED=$bc BRANCHES_VALID=$bv"
```

It prints `LINES_COVERED=112359 LINES_VALID=132967 BRANCHES_COVERED=26496 BRANCHES_VALID=33480`.
`GetAttribute` is used rather than property access so a `<line>` lacking an attribute yields an empty
string instead of throwing under `Set-StrictMode`.

#### The authoritative figures, 112355 and 26500

No output document for the authoritative collection is present in this worktree. Reproducing its
figures would require the whole procedure to be re-run:

1. Restore the six Write Set files this artifact's "Measurement method" section names to their
   `pre-782-base` content with `git checkout pre-782-base -- <those six paths>`.
2. Run the collect command recorded above from the worktree root.
3. Aggregate the written document with the snippet above.

**That run is deliberately not performed.** It would mutate the delivered worktree for the duration
of the collection, and its result would be a new third measurement rather than a confirmation of the
recorded one, because a fresh collection is a fresh observation. The authoritative figures therefore
remain unreproducible from any document available today, which is what
`BASELINE-AUTHORITATIVE-OUTPUT-DOCUMENT: NOT-RETAINED` records.
