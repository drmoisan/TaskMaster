# 2026-08-28-quickfiler-carry-folder-predictor-to-item-controller — Remediation Plan, Cycle 1

- **Issue:** #678
- **Cycle:** remediation cycle 1
- **Owner:** drmoisan
- **Last Updated:** 2026-09-01T23-44
- **Status:** Draft
- **Version:** 1.0
- **Work Mode:** minor-audit, resolved from the marker `- Work Mode: minor-audit` at `docs/features/active/2026-08-28-quickfiler-carry-folder-predictor-to-item-controller-678/issue.md:13`
- **Branch:** `bug/quickfiler-carry-folder-predictor-to-item-controller-678`
- **Base ref (literal SHA, used in every git command in this plan; the ref name `origin/main` is never used because MSYS path conversion mangles it and a concurrent fetch can advance it mid-run):** `807fb0bb6e5e49f43efa6b256b05960bf078ca19`

## Requirements source

The sole requirements source for this cycle is
`docs/features/active/2026-08-28-quickfiler-carry-folder-predictor-to-item-controller-678/remediation-inputs.2026-09-01T23-44.md`,
items R1, R2, R3 and R4. That document states each item as an **invariant** rather than as a symptom,
and this plan preserves that framing: every acceptance condition below is written against the
invariant at the boundary that consumes the value, not against the textual agreement of two
expressions.

The three audit artifacts `code-review.2026-09-01T23-35.md`, `feature-audit.2026-09-01T23-35.md` and
`policy-audit.2026-09-01T23-35.md` are background only and are not a requirements source. The
original plan `plan.2026-08-31T21-12.md` is reused for its evidence conventions and for Derivations
D1 through D8, which are reproduced below with the base-ref name replaced by the literal SHA.

Explicitly out of this cycle and not to be fixed, promoted, or filed: NB-4 (AC20 per-member
coverage), NB-6 (pre-existing oversized files), NB-7 (informational) and NB-8 (AC11/AC12
criterion-text tension). AC20 stays unchecked.

## Evidence location rule (non-overridable)

Every evidence artifact in this plan resolves under
`docs/features/active/2026-08-28-quickfiler-carry-folder-predictor-to-item-controller-678/evidence/`
with sub-kind `remediation-baseline`, `regression-testing`, `qa-gates`, `issue-updates` or `other`.
Paths under `artifacts/baselines/`, `artifacts/baseline/`, `artifacts/qa/`, `artifacts/qa-gates/`,
`artifacts/evidence/`, `artifacts/coverage/`, `artifacts/regression-testing/` and
`artifacts/post-change/` are forbidden for evidence and must not be used even if a delegation prompt
supplies one. The delegation prompt for this cycle supplied only canonical paths, so no override was
rejected.

Each command-step artifact records `Timestamp:`, `Command:`, `EXIT_CODE:` and `Output Summary:`. The
two red runs additionally record `ExpectedExitCode: 1`. No helper script is placed under `evidence/`;
if the executor needs a durable helper script it goes under
`docs/features/active/2026-08-28-quickfiler-carry-folder-predictor-to-item-controller-678/scripts/`.
No artifact embeds an absolute host path.

## Artifact-name non-collision rule (load-bearing for R4)

R4 requires the thirteen existing artifacts under `evidence/qa-gates/` to keep their recorded
`Command:`, `EXIT_CODE:` and `Output Summary:` values, which are factual records of runs that already
happened. Phase 2 of this cycle re-runs the same five commands. **Phase 2 therefore writes to new
file names carrying the `remediation-` prefix and overwrites no existing artifact under
`evidence/qa-gates/`.** Overwriting `csharpier-format.md`, `csharpier-check.md`, `analyzer-build.md`,
`nullable-build.md`, `mstest-coverage-run.md`, `coverage-post-change.md`,
`coverage-post-change.jacoco.xml`, `coverage-delta.md`, `exclude-attribute-invariant.md`,
`file-size-audit.md`, `scope-confinement.md`, `final-toolchain-pass.md` or `final-commit.md` destroys
the record R4 exists to correct and is prohibited.

## Toolchain commands (verbatim; do not substitute)

- Format apply: `dotnet tool run csharpier format .`
- Format verify: `dotnet tool run csharpier check .`
- Analyzers: `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
- Nullable / type-check: `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`
- Tests with coverage: `pwsh -NoProfile -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot .`

`/t:Rebuild` is load-bearing for the two gate builds: MSBuild's up-to-date check does not invalidate
on a command-line `/p:` change, so a warm `/t:Build` returns exit 0 having skipped `CoreCompile` on
every project and the gate cannot fail. `/p:Nullable=enable` must never be added; no project carries
a `<Nullable>` element and there is no `Directory.Build.props`, so adding it conscripts files that
never opted in.

A bare `vstest.console.exe` invocation is prohibited: it omits
`/TestCaseFilter:TestCategory!=LiveOutlook` and would run a test requiring a live Outlook COM
instance. Every scoped run in Phase 1 uses Derivation D7, which always carries that filter and a
task-private `/ResultsDirectory`.

The environment is already bootstrapped by the orchestrator (`.dotnet-sdk` 8.0.205 present,
`packages/` restored, `dotnet-coverage` 18.10.0 present). No task in this plan re-bootstraps it.
`msbuild` resolves only under `pwsh`, not under `bash`. The bash tool refuses compound commands in
this worktree, so each command is issued singly or through a `-File` script.

## Scope boundary and hard constraints

In scope: `QuickFiler/`, `QuickFiler.Test/` and this feature folder. Nothing else.

1. No file under `UtilitiesCS/`, `.claude/` or the repository-root `CLAUDE.md` is modified. R2 names
   `UtilitiesCS/OutlookObjects/Folder/FolderPredictor.cs` as the **parity target** and that file must
   not be edited.
2. No acceptance-criterion text in `issue.md` is edited, added or removed. No checkbox transition is
   performed. AC20 stays `- [ ]`.
3. No `[ExcludeFromCodeCoverage]` attribute is added or removed anywhere.
4. No existing passing test is weakened, renamed away, deleted, or modified to accommodate a fix,
   with exactly one authorised exception: R2 explicitly authorises correcting the single assertion at
   `QuickFiler.Test/Controllers/QfcItemController.FolderHandlingTests.Part2.cs:219-222`, whose
   asserted parity is untrue. That correction is named in P1-T6 and nowhere else.
5. `artifacts/orchestration/orchestrator-state.json`, `.claude/agent-memory/` and `.git/info/exclude`
   are not modified.
6. File-size budget. `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs` is exactly 500 lines
   with zero headroom; `QuickFiler/Controllers/QfcQueue.cs` is 505; `QuickFiler/Controllers/QfcCollectionController.cs`
   is 2336; `QuickFiler.Test/Controllers/QfcFormControllerTests.cs` is 792. None of the four is edited
   by this plan. Any addition to one of them would go into a new partial part with a matching
   `<Compile Include>` entry, never into the existing file. Both projects use explicit
   `<Compile Include>` item lists, so every new `.cs` file needs an entry.

## Files this cycle touches (re-derived against the current tree)

Production:

| Path | Current lines | Change | Coverage status |
|---|---|---|---|
| `QuickFiler/Controllers/QfcHighConfidencePreFilter.cs` | 228 | two new static members on `QfcPreScoredItem` (`:106-150`) | measured; only `FolderScoringService` at `:198` is exempt |
| `QuickFiler/Controllers/QfcQueue.Enqueue.cs` | 216 | `ResolveCarriedHandler` (`:143-168`) body delegates | measured |
| `QuickFiler/Controllers/QfcHomeController.cs` | 465 | `RunAsync` carrier reconciliation at `:307` | measured |
| `QuickFiler/Controllers/QfcDatamodel.QueueProcessing.cs` | 292 | XML doc block `:165-170` only | `QfcDatamodel` is `[ExcludeFromCodeCoverage]` (`QfcDatamodel.cs:25`) |
| `QuickFiler/Controllers/QfcItemController.FolderHandling.cs` | 293 | R2 guard + call site + doc; R3 cancellation observation | measured |

Test:

| Path | Current lines | Change |
|---|---|---|
| `QuickFiler.Test/Controllers/QfcHomeControllerRunAsyncHighConfidenceTests.Part3.cs` | new | R1 regression test |
| `QuickFiler.Test/Controllers/QfcItemController.FolderHandlingTests.Part2.cs` | 241 | one corrected assertion, two new tests |
| `QuickFiler.Test/QuickFiler.Test.csproj` | — | one `<Compile Include>` entry |

`QuickFiler/Properties/AssemblyInfo.cs:5` carries `[assembly: InternalsVisibleTo("QuickFiler.Test")]`,
so `internal` members added to `QfcPreScoredItem` are reachable from the test assembly, exactly as the
existing `internal static` `QfcQueue.ResolveCarriedHandler` already is.

## Design derivations for R1, R2 and R3

Derivation DR1 — the leg A item-set invariant and how it is pinned at the consuming boundary.

`QuickFiler/Controllers/QfcDatamodel.QueueProcessing.cs:193` returns
`new QfcDequeueBatch(UnhookDequeuedNodes(nodes), accepted, batch.Stop)`. `Items` is the post-unhook
list; `PreScored` is `accepted`, captured before the unhook pass. `TryUnhookOrReplace` (`:31-66`) is
not read-only: on an `UnhookItem` throw it performs `nodes.Remove(node)` (`:54`), then
`node = _masterQueue.TryTakeFirst()` (`:55`), then `nodes.Insert(i, node)` (`:62`). On that path the
two collections diverge in both directions.

`QuickFiler/Controllers/QfcHomeController.cs:307` assigns `preScored = batch.PreScored` and `:318`
passes it to `LoadItemsAsync`. `QuickFiler/Controllers/QfcFormController.Actions.cs:120-153` forwards
it to `QfcCollectionController.LoadControlsAndHandlers_01Async`, whose body at
`QuickFiler/Controllers/QfcCollectionController.CarrierLoad.cs:41` derives the displayed item spine
as `preScored.Select(x => x.MailItem)` and at `:70-84` builds one `QfcItemGroup` per carrier. The
displayed set is therefore exactly the carrier list's mail-item set. That is the consuming boundary.

Leg B already avoids this: `QuickFiler/Controllers/QfcHomeController.Iteration.cs:28` takes
`batch.Items` as the spine and passes `batch.PreScored` only as a lookup table, resolved per row by
`QfcQueue.ResolveCarriedHandler` at `QuickFiler/Controllers/QfcQueue.Enqueue.cs:196`.

**The fix mirrors leg B by making `batch.Items` the leg A spine too**, reusing one matching
implementation rather than writing a second one. The matching helper is generalised from
`ResolveCarriedHandler` to return the whole carrier, and `ResolveCarriedHandler` is rewritten to
delegate to it, so exactly one EntryID-matching body exists in the tree.

Reference shape (the executor owns the edit; the acceptance conditions govern):

```csharp
internal static QfcPreScoredItem? ResolveCarrier(
    IList<QfcPreScoredItem> preScored,
    MailItem mailItem
)
{
    if (preScored is null || preScored.Count == 0 || mailItem is null)
    {
        return null;
    }

    string entryId = mailItem.EntryID;
    foreach (QfcPreScoredItem carrier in preScored)
    {
        if (ReferenceEquals(carrier.MailItem, mailItem))
        {
            return carrier;
        }
        if (
            !string.IsNullOrEmpty(entryId)
            && carrier.MailItem is not null
            && carrier.MailItem.EntryID == entryId
        )
        {
            return carrier;
        }
    }

    return null;
}

internal static IList<QfcPreScoredItem> ReconcileCarriersToItems(
    IList<MailItem> items,
    IList<QfcPreScoredItem> preScored
)
{
    IList<MailItem> spine = items ?? new List<MailItem>();
    var reconciled = new List<QfcPreScoredItem>(spine.Count);
    foreach (MailItem item in spine)
    {
        reconciled.Add(ResolveCarrier(preScored, item) ?? new QfcPreScoredItem(item, null));
    }
    return reconciled;
}
```

Four facts make this shape mandatory rather than optional:

1. **Reference identity must be tried before `EntryID`.** The existing passing test
   `RunAsync_HighConfidenceEnabled_LoadsFirstPageFromStreamingDequeue`
   (`QuickFiler.Test/Controllers/QfcHomeControllerRunAsyncHighConfidenceTests.cs:130-258`) builds its
   carrier from `new Mock<MailItem>().Object` with no `EntryID` setup, so `EntryID` is null. A
   matcher that returns null on an empty `EntryID` before trying reference identity would strand that
   item's handler and break the assertion at `:228-240`, which constraint 4 forbids. On the happy
   path the objects are literally the same instances, because
   `QfcDatamodel.QueueProcessing.cs:192` builds `nodes` from `accepted.Select(x => x.MailItem)`.
2. **`ResolveCarriedHandler`'s six existing assertions survive.** In
   `ResolveCarriedHandler_WhenNoCarrierMatches_ReturnsNull`
   (`QuickFiler.Test/Controllers/QfcQueuePurePathsTests.cs:318-340`) the first two negative cases at
   `:326` and `:327-330` pass the carrier's own `known` instance but supply a null and an empty
   carrier list, so both exit at the `preScored is null || preScored.Count == 0` guard before the
   loop runs and the added reference check is never reached. The third case at `:331` passes a null
   mail item and exits at the same guard. The remaining two, at `:332-335` and `:336-339`, pass
   distinct mock instances, so the reference check does not fire, and `:333`'s probe carries a null
   `EntryID` which the retained `!string.IsNullOrEmpty(entryId)` clause skips rather than matching
   against the carrier's own null. All five still return null; the positive case at `:283-309` still
   matches by `EntryID`.
3. **The helpers live on `QfcPreScoredItem`, not on `QfcQueue`.** `QfcHomeController` declares an
   instance property `internal IQfcQueue QfcQueue { get; set; }` at
   `QuickFiler/Controllers/QfcHomeController.cs:153`. Inside a `QfcHomeController` member the simple
   name `QfcQueue` binds to that property, whose type is `IQfcQueue` and not `QfcQueue`, so the
   colour-colour rule does not apply and `QfcQueue.ReconcileCarriersToItems(...)` would fail to
   compile. `QfcPreScoredItem` has no such shadow. It is also the cohesive home: the carrier type
   owns carrier-list reconciliation.
4. **An unmatched item gets a bare carrier, not a fabricated one.** `new QfcPreScoredItem(item, null)`
   coerces `PredeterminedFolder` to `string.Empty` (`QfcHighConfidencePreFilter.cs:130`) and leaves
   `FolderHandler` null, so the item controller falls back to its own scoring pass and to index-1
   selection — the pre-#678 behaviour for a row with no carrier.

`QfcDequeueBatch.Items` and `.PreScored` are never null (`QuickFiler/Interfaces/IQfcDatamodel.cs:71`
and `:77`), so the `LoadItemsAsync` null-guard at `QuickFiler/Controllers/QfcFormController.Actions.cs:125-135`
is unreachable from leg A both before and after this change, and an empty batch still produces an
empty carrier list. That preserves `RunAsync_HighConfidenceEmptyBatch_StillLoadsItemsAndStartsIteration`
(`QuickFiler.Test/Controllers/QfcHomeControllerRunAsyncHighConfidenceTests.Part2.cs:152-239`) and
`RunAsync_HighConfidenceScanProgress_MapsReportsIntoTheZeroToThirtyBand` (`:34-144`) unchanged.

Derivation DR2 — the R2 option chosen, and why.

R2 offers two options. **This plan chooses option 1: align the projection.** Option 2 (narrowing the
claim and the test name) would leave the stated invariant false — the invariant is that the carried
`PredeterminedFolder` and the `FolderArray` entries are the *same projection of the same input* so
that `_itemViewer.FolderContains` matches for every archive-rooted suggestion the predictor can
produce. In the (non-null globals, empty archive root) state the predictor's `FolderArray` entries
*are* separator-stripped, so an unstripped carried value cannot match and the AC12 defect reopens in
exactly that state. Renaming the test would document the gap rather than close it.

`UtilitiesCS/OutlookObjects/Folder/FolderPredictor.cs:845-858` guards on `_globals is null` and then
forms `archivePrefix = _globals.Ol.ArchiveRootPath + "\\"` unconditionally, so a null **or** empty
`ArchiveRootPath` both yield the prefix `"\"`. `QuickFiler/Controllers/QfcItemController.FolderHandling.cs:255-258`
guards instead on `string.IsNullOrEmpty(archiveRootPath)`, which conflates the globals-null state
with the empty-root state. Two edits align them:

- the helper guard becomes `if (string.IsNullOrEmpty(folderPath) || archiveRootPath is null)`, so a
  null `archiveRootPath` stands for `FolderPredictor`'s `_globals is null` guard and nothing else;
- the call site at `QuickFiler/Controllers/QfcItemController.FolderHandling.cs:222-225` passes
  `_globals is null ? null : (_globals.Ol?.ArchiveRootPath ?? string.Empty)` instead of
  `_globals?.Ol?.ArchiveRootPath`, so the null signal now means "no globals" and only that.

Two divergences remain and are deliberate, and the doc comment must name both rather than claim
unqualified parity: a null or empty `folderPath` is returned unchanged rather than dereferenced
(`FolderPredictor` does not guard it because its input comes from `Suggestions`), and a non-null
globals with a null `Ol` is treated as an empty archive root rather than reproducing
`FolderPredictor`'s null dereference.

Blast radius, re-derived against the current tree. Exactly one existing assertion changes:

| Existing assertion | Line | Before | After |
|---|---|---|---|
| `(@"\\Archive\Projects\Active", null)` | 215-218 | identity | identity — unchanged |
| `(@"\\Archive\Projects\Active", string.Empty)` | 219-222 | identity | `@"\Archive\Projects\Active"` — **corrected** |
| `(null, @"\\Archive")` | 223-226 | null | null — unchanged |
| `(@"\\Other\Projects", @"\\Archive")` | 227-230 | identity | identity — unchanged |
| `(@"\\Archive\", @"\\Archive")` | 231-234 | identity | identity — unchanged |
| `(@"\\ARCHIVE\Projects", @"\\archive")` | 235-238 | `@"Projects"` | `@"Projects"` — unchanged |

No `AssignFolderComboBox` test regresses. `AssignFolderComboBox_WhenPredeterminedFolderPresent_PreselectsThatFolder`
(`QuickFiler.Test/Controllers/QfcItemController.FolderHandlingTests.cs:440-462`) sets no `_globals`,
so the call site still yields null and the projection is still the identity.
`AssignFolderComboBox_PredeterminedFolder_PreselectsByNameAndStillPopulates`
(`QuickFiler.Test/Controllers/QfcItemController.FolderSuggestionsTests.cs:137`) uses the
predetermined folder `"Archive\\Finance"`, which has no leading separator, so no strip occurs under
either guard. `AssignFolderComboBox_WhenArchiveRootedPredeterminedFolder_PreselectsThatFolder`
(`QfcItemController.FolderHandlingTests.Part2.cs:163-204`) supplies `\\Archive` as the root and is
unaffected. After the fix the test name
`ProjectPredeterminedFolder_BoundaryCases_MatchFolderPredictorProjection` becomes accurate at the
`(folderPath, archiveRootPath)` level the test actually exercises, so it is neither renamed nor
weakened.

Derivation DR3 — the R3 pre-change outcome, restated as an observable.

Every pre-change route into the predictor ran inside `await Task.Run(..., cancel)`
(`QuickFiler/Controllers/QfcItemController.FolderHandling.cs:81-97`). For an already-cancelled token
`Task.Run` returns a cancelled task and the await throws `TaskCanceledException`, which is not an
`ArgumentNullException`, so it falls to the `catch (System.Exception e)` at `:118-122`, is logged and
rethrown. The observable pre-change outcome is therefore: **an `OperationCanceledException`
propagates out of `LoadFolderHandlerAsync` and `_folderHandler` is not assigned.**
`TaskCanceledException` derives from `OperationCanceledException`, and both callers of this member
wrap it in a `Task.Run(..., token)` whose await surfaces the cancellation:
`QuickFiler/Controllers/QfcCollectionController.cs:519-525`, whose folder tasks are awaited through
`Task.WhenAny`, and `QuickFiler/Controllers/QfcItemController.FolderHandling.cs:178`. A
`cancel.ThrowIfCancellationRequested()` therefore reproduces at both call sites the same
`OperationCanceledException` the pre-change `Task.Run(..., cancel)` route produced.

The guard goes as the **first statement inside the adoption branch** at
`QuickFiler/Controllers/QfcItemController.FolderHandling.cs:68-77`, not at the top of the member.
The `try` opens at `:79`, after that branch, and its `catch (System.Exception e)` at `:118-122`
covers the `FromField` route only: the `varList is null` route that reaches the predictor through
`Task.Run(..., cancel)` at `:81-97`. A guard at the top of the member would throw before that `try`
is entered, silently removing the `logger.Error` at `:120` which the pre-change `FromField` route
emitted for an already-cancelled token, and that is a second behaviour change this cycle is not
authorised to make. The `FromArrayOrString` route is the `else` branch at `:124-147`; it carries no
`try` or `catch` of its own and emits `logger.Debug` rather than `logger.Error`, so it is not the
route this placement protects.

## Derivations (referenced by identifier; run from the worktree root under `pwsh`)

Derivation D1 — package-set proof that a coverage report is post-processed.

```powershell
. scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1
$doc = [xml](Get-Content -LiteralPath 'coverage/coverage.cobertura.xml' -Raw -Encoding UTF8)
$names = @($doc.SelectNodes('//package') | ForEach-Object { $_.GetAttribute('name') } | Sort-Object)
$names -join ','
```

The allowlist derived from the nine non-test project files in this tree is, sorted:
`QuickFiler,SVGControl,Tags,TaskMaster,TaskTree,TaskVisualization,ToDoModel,UtilitiesCS,VBFunctions`.
The proof condition is: the observed set is a subset of that allowlist, it contains `QuickFiler`, and
it contains no `log4net` entry. A naive line search for the text `<package name=` returns zero
matches against this XML because the element emits `name` after `line-rate`; the XPath form above is
the only accepted derivation.

Derivation D2 — root-level coverage figures from a post-processed report.

```powershell
$c = $doc.SelectSingleNode('/coverage')
'{0}|{1}|{2}|{3}|{4}|{5}' -f $c.GetAttribute('line-rate'), $c.GetAttribute('lines-covered'), $c.GetAttribute('lines-valid'), $c.GetAttribute('branch-rate'), $c.GetAttribute('branches-covered'), $c.GetAttribute('branches-valid')
```

These six attributes are written by `ConvertTo-KoverageCoberturaXml` and exist only on a
post-processed document, so D2 is meaningful only after D1 passes.

Derivation D3 — per-file line summary.

```powershell
foreach ($cls in $doc.SelectNodes('//class[@filename]')) {
    $s = Get-CoberturaClassLineSummary -ClassNode $cls
    '{0}|{1}|{2}' -f $cls.GetAttribute('filename'), $s.CoveredLines, $s.TotalLines
}
```

D1, D2, D3 and D6 share the variable `$doc`, which only D1 assigns, and the helper functions that
only the `. scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1` line in D1 or D4 dot-sources. All
four must therefore be issued inside one `pwsh` session. Run in separate invocations, `$doc` is
unassigned, `$doc.SelectNodes(...)` yields `$null`, the `foreach` body never executes, and D3 and D6
return an empty result that is indistinguishable from a report in which a file genuinely has no row.
Every task that runs D3 or D6 must additionally record `@($doc.SelectNodes('//class[@filename]')).Count`
as a non-vacuity control.

`Get-CoberturaClassLineSummary` deduplicates the class-level rollup against the method-level view.
Counting `.//line` directly double-counts every source line and must not be used.
`Merge-CoberturaClassesByFilename` has already merged async state-machine classes into one entry per
file in a post-processed document, so D3 yields one row per file.

Derivation D4 — fallback post-processing when the runner threw before writing.

```powershell
. scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1
$repoRoot = (Get-Location).Path
$xml = Get-Content -LiteralPath 'coverage/coverage.cobertura.xml' -Raw -Encoding UTF8
$processed = ConvertTo-KoverageCoberturaXml -XmlContent $xml -RepoRoot $repoRoot
Set-Content -LiteralPath 'coverage/coverage.postprocessed.cobertura.xml' -Value $processed -Encoding UTF8 -NoNewline
```

`Invoke-DotnetCoverageCollection` throws on a non-zero coverage exit code and
`Assert-CoberturaLineCoverageThreshold` throws below 80 percent; both run before the report is
written to its final location, and either throw leaves the UNFILTERED report on disk. Comparing a
post-processed baseline against an unfiltered post-change report compares different denominators. D4
restores the same post-processing without the threshold assertion so both sides of a comparison are
derived identically. When D4 is used, D1, D2, D3 and D6 read
`coverage/coverage.postprocessed.cobertura.xml` instead. `coverage/*` is git-ignored, so neither raw
file is ever committed.

Derivation D5 — added production lines relative to the base ref.

```powershell
$file = ''
$added = @{}
foreach ($line in (git diff --unified=0 807fb0bb6e5e49f43efa6b256b05960bf078ca19 -- QuickFiler)) {
    if ($line -match '^\+\+\+ b/(.+)$') { $file = $Matches[1]; $added[$file] = New-Object System.Collections.Generic.List[int]; continue }
    if ($line -match '^@@ -[0-9,]+ \+([0-9]+)(,([0-9]+))? @@') {
        $start = [int]$Matches[1]
        $count = if ($Matches.ContainsKey(3) -and $Matches[3]) { [int]$Matches[3] } else { 1 }
        for ($i = 0; $i -lt $count; $i++) { $added[$file].Add($start + $i) }
    }
}
foreach ($k in $added.Keys) { '{0}|{1}' -f $k, $added[$k].Count }
```

Derivation D6 — per-line hit map for the changed-line intersection.

```powershell
foreach ($cls in $doc.SelectNodes('//class[@filename]')) {
    $s = Get-CoberturaClassLineSummary -ClassNode $cls
    foreach ($k in $s.LineMap.Keys) { '{0}|{1}|{2}' -f $cls.GetAttribute('filename'), $k, $s.LineMap[$k].Hits }
}
```

Cobertura `filename` values carry native separators, while git reports forward slashes. Replace `/`
with `\` in the git path before joining D5 to D6. An added line with no `LineMap` entry is
non-executable (brace, comment, attribute, declaration) and is excluded from the changed-line
denominator; that exclusion count is reported alongside the figure.

Derivation D7 — scoped MSTest run, retaining the live-Outlook exclusion. The example below is the
P1-T2 form; each task that cites D7 states its own `/TestCaseFilter:` and `/ResultsDirectory:` values
verbatim in its own text.

```powershell
$vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'
$vstest = & $vswhere -latest -products * -find 'Common7\IDE\Extensions\TestPlatform\vstest.console.exe' | Select-Object -First 1
& $vstest 'QuickFiler.Test/bin/Debug/QuickFiler.Test.dll' '/Settings:scripts/vscode/TaskMaster.cli.runsettings' '/InIsolation' '/TestCaseFilter:TestCategory!=LiveOutlook&FullyQualifiedName~RunAsync_HighConfidenceUnhookReplaced_LoadsPostUnhookItemSetAtLegABoundary' '/Logger:trx' '/ResultsDirectory:TestResults\p1-t2'
```

D7 is preceded, in the same task, by
`msbuild TaskMaster.sln /t:Build /m /p:Configuration=Debug "/p:Platform=Any CPU"` and the run
proceeds only when that build exits 0. Without it the scoped run reads whatever `QuickFiler.Test.dll`
a previous task produced: a newly added test is not discovered at all, and a test whose production
dependency was just edited reports its previous result. `/t:Build` rather than `/t:Rebuild` is
correct here because this is a build-for-test, not an analyzer or nullable gate; MSBuild's
up-to-date check does invalidate on a changed source timestamp, and the vacuity hazard applies only
to a `/p:` property change. `QuickFiler.Test` is a legacy non-SDK project and builds to
`QuickFiler.Test/bin/Debug/QuickFiler.Test.dll` with no target-framework subfolder.
`/ResultsDirectory` is mandatory; each scoped run uses its own `p#-t#` subdirectory, and the executor
clears that subdirectory before the run so an "exactly one TRX" reading cannot be confused by a
re-run's second timestamped file.

Derivation D8 — line count of a file.

```powershell
(Get-Content -LiteralPath 'QuickFiler/Controllers/QfcHomeController.cs').Count
```

`Measure-Object -Line` reports a different value on a file without a trailing newline and must not be
used for the 500-line cap.

Derivation D9 — real-clock write time of every artifact in a directory.

```powershell
$dir = 'docs/features/active/2026-08-28-quickfiler-carry-folder-predictor-to-item-controller-678/evidence/qa-gates'
foreach ($f in (Get-ChildItem -LiteralPath $dir -File | Sort-Object Name)) {
    '{0}|{1}|{2}' -f $f.Name, $f.LastWriteTime.ToString('yyyy-MM-ddTHH:mm:ss'), $f.LastWriteTime.ToString('yyyy-MM-ddTHH-mm')
}
```

The third column is the corrected `Timestamp:` value in the repository's `yyyy-MM-ddTHH-mm` format.
D9 must be run before any edit touches the directory, because writing a file replaces the evidence
D9 reads.

## Named baselines this cycle refers to

- `R_BASELINE_FAILURE_SET` — fully qualified names reported as failed by P0-T8.
- `R_BASELINE_TOTALS` — total, passed, failed and skipped counts from P0-T8.
- `R_BASELINE_FORMAT_DRIFT` — the file list reported by P0-T5.
- `R_BASELINE_ANALYZER_SUMMARY` — the MSBuild warning and error counts from P0-T6.
- `R_BASELINE_COVERAGE` — the six root-level Cobertura figures from P0-T9.
- `R_BASELINE_SIZE_CENSUS` — the per-file line counts from P0-T11.
- `R_TIMESTAMP_PREIMAGE` — the 13 file names, their mtimes and their declared `Timestamp:` values
  from P0-T12.

These are this cycle's own baseline. The Phase 0 figures of `plan.2026-08-31T21-12.md` are not
reused as a baseline: the tree changed when commits 8782db56 and d1f51e3a landed.

## Fail-closed evidence rule

Every evidence-producing task names its artifact path. A task whose artifact is absent, or whose
artifact omits any required field, stays unchecked. If any required baseline artifact, final-QC
artifact, or coverage-comparison artifact is missing, the verdict is BLOCKED or INCOMPLETE, never
PASS. `EXIT_CODE: SKIPPED` is not a passing outcome for any command task in this plan.

---

### Phase 0 — Remediation baseline capture

- [x] [P0-T1] Read the policy documents in the `policy-compliance-order` order and write `docs/features/active/2026-08-28-quickfiler-carry-folder-predictor-to-item-controller-678/evidence/remediation-baseline/phase0-instructions-read.md`. Acceptance: the artifact contains `Timestamp:`, `Policy Order:` and an explicit list naming all seven of `CLAUDE.md`, `.claude/rules/general-code-change.md`, `.claude/rules/general-unit-test.md`, `.claude/rules/csharp.md`, `.claude/rules/quality-tiers.md`, `.claude/rules/tonality.md` and `.claude/rules/plan-acceptance-gates.md`.

- [x] [P0-T2] Record the base-ref anchor in `docs/features/active/2026-08-28-quickfiler-carry-folder-predictor-to-item-controller-678/evidence/remediation-baseline/base-ref-anchor.md`. Run `git rev-parse HEAD` and `git merge-base 807fb0bb6e5e49f43efa6b256b05960bf078ca19 HEAD`. Acceptance, all three: both outputs are recorded verbatim; the merge-base output equals the literal string `807fb0bb6e5e49f43efa6b256b05960bf078ca19`; and the artifact records that every anchored diff in this plan uses that literal SHA and never the ref name `origin/main`. If the merge-base differs, the task stays unchecked and the executor reports the divergence rather than re-anchoring on a different ref.

- [x] [P0-T3] Record the `issue.md` acceptance-criteria preimage in `docs/features/active/2026-08-28-quickfiler-carry-folder-predictor-to-item-controller-678/evidence/remediation-baseline/issue-ac-preimage.md`, which P2-T11 compares against. A base-ref-anchored diff cannot serve as that comparison, because the previous cycle's commits 8782db56 and d1f51e3a already modified `issue.md` relative to `807fb0bb6e5e49f43efa6b256b05960bf078ca19`, so an anchored diff is non-empty before this cycle does anything; a whole-file digest captured now is the only comparison that isolates this cycle. Acceptance, all seven: the token `- Work Mode: minor-audit` occurs in `issue.md` exactly once and its line number is recorded; the heading `## Acceptance Criteria` occurs exactly once and its line number is recorded; the count of lines matching the regular expression `^- \[[ x]\] AC` is recorded and equals 23; the split of that count into checked and unchecked is recorded and is 22 checked and 1 unchecked; the single unchecked line is recorded verbatim together with its line number and its identifier is AC20; the SHA-256 digest of `issue.md` is computed with `Get-FileHash -Algorithm SHA256 -LiteralPath` and recorded verbatim as `R_ISSUE_DIGEST`; and neither `spec.md` nor `user-story.md` exists in the feature folder, recorded with `SearchScope:`, `SearchPatterns:` and `SearchResult:`.

- [x] [P0-T4] Run `dotnet tool restore` from the worktree root and record `docs/features/active/2026-08-28-quickfiler-carry-folder-predictor-to-item-controller-678/evidence/remediation-baseline/dotnet-tool-restore.md`. Acceptance, both: `EXIT_CODE: 0`; and `Output Summary:` records the CSharpier version string that the tool manifest pins, read directly from the repository-root file `dotnet-tools.json` rather than inferred from any tool output. That file, and not `.config/dotnet-tools.json`, is the manifest present in this tree.

- [x] [P0-T5] Run `dotnet tool run csharpier check .` and record `docs/features/active/2026-08-28-quickfiler-carry-folder-predictor-to-item-controller-678/evidence/remediation-baseline/csharpier-check.md`. Acceptance, all three: `EXIT_CODE:` recorded; `Output Summary:` reproduces verbatim the final summary line the run printed, which on a clean run has the shape of a checked-file count and an elapsed time; and every path the run reported as needing formatting is enumerated, that enumeration being `R_BASELINE_FORMAT_DRIFT`, recorded even when it is empty. This is a read-only check command, so its exit code is a real signal.

- [x] [P0-T6] Run `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` and record `docs/features/active/2026-08-28-quickfiler-carry-folder-predictor-to-item-controller-678/evidence/remediation-baseline/analyzer-build.md`. Acceptance, all three: `EXIT_CODE:` recorded; `Output Summary:` reproduces the MSBuild warning-count and error-count summary lines verbatim as `R_BASELINE_ANALYZER_SUMMARY`; and the number of `CoreCompile:` occurrences in the build log is recorded as a non-vacuity control, because a run that skipped compilation cannot have run any analyzer.

- [x] [P0-T7] Run `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true` and record `docs/features/active/2026-08-28-quickfiler-carry-folder-predictor-to-item-controller-678/evidence/remediation-baseline/nullable-build.md`. Acceptance, all three: `EXIT_CODE:` recorded truthfully; `Output Summary:` enumerates every `CS86` diagnostic reported, or states that none was reported; and the number of `CoreCompile:` occurrences is recorded.

- [x] [P0-T8] Run `pwsh -NoProfile -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot .` and record `docs/features/active/2026-08-28-quickfiler-carry-folder-predictor-to-item-controller-678/evidence/remediation-baseline/mstest-coverage-run.md`. `-SearchRoot .` is mandatory. Acceptance, all four: `EXIT_CODE:` recorded; `Output Summary:` states whether the run printed the literal `Done. Coverage artifact:`, which is emitted only after post-processing and the on-disk write both succeed; the total, passed, failed and skipped counts are recorded numerically as `R_BASELINE_TOTALS`; and the fully qualified names of all failing tests are enumerated as `R_BASELINE_FAILURE_SET`, recorded as the empty set when there are none.

- [x] [P0-T9] Prove the baseline coverage report is post-processed and record the numeric figures in `docs/features/active/2026-08-28-quickfiler-carry-folder-predictor-to-item-controller-678/evidence/remediation-baseline/coverage-baseline.md`. Run Derivation D1; if P0-T8 did not print `Done. Coverage artifact:`, run Derivation D4 first and read the post-processed file. Acceptance, all four: the observed package-name list from D1 is recorded verbatim; it is a subset of the nine-name allowlist; it contains `QuickFiler` and no `log4net` entry; and Derivation D2 output is recorded as six numeric values under `Output Summary:` as `R_BASELINE_COVERAGE`, with the line-rate and branch-rate additionally expressed as percentages to two decimal places. No placeholder value is accepted.

- [x] [P0-T10] Record the baseline per-file coverage of the five production files this cycle touches, using Derivation D3, in `docs/features/active/2026-08-28-quickfiler-carry-folder-predictor-to-item-controller-678/evidence/remediation-baseline/coverage-per-file-baseline.md`. Acceptance: the artifact carries one covered-over-total row for each of `QuickFiler/Controllers/QfcHighConfidencePreFilter.cs`, `QuickFiler/Controllers/QfcQueue.Enqueue.cs`, `QuickFiler/Controllers/QfcHomeController.cs`, `QuickFiler/Controllers/QfcDatamodel.QueueProcessing.cs` and `QuickFiler/Controllers/QfcItemController.FolderHandling.cs`, or records `NOT PRESENT IN REPORT` for a path with no row together with the reason, which for `QfcDatamodel.QueueProcessing.cs` is expected to be the class-level `[ExcludeFromCodeCoverage]` at `QuickFiler/Controllers/QfcDatamodel.cs:25`; and the non-vacuity control `@($doc.SelectNodes('//class[@filename]')).Count` is recorded as an integer greater than zero, so a `NOT PRESENT IN REPORT` row is distinguishable from a derivation that ran with an unassigned `$doc`. No `NOT PRESENT IN REPORT` row may be accepted while that control reads zero.

- [x] [P0-T11] Record `R_BASELINE_SIZE_CENSUS` in `docs/features/active/2026-08-28-quickfiler-carry-folder-predictor-to-item-controller-678/evidence/remediation-baseline/file-size-census.md` using Derivation D8 for each of the seven paths `QuickFiler/Controllers/QfcHighConfidencePreFilter.cs`, `QuickFiler/Controllers/QfcQueue.Enqueue.cs`, `QuickFiler/Controllers/QfcHomeController.cs`, `QuickFiler/Controllers/QfcDatamodel.QueueProcessing.cs`, `QuickFiler/Controllers/QfcItemController.FolderHandling.cs`, `QuickFiler.Test/Controllers/QfcItemController.FolderHandlingTests.Part2.cs` and `QuickFiler.Test/Controllers/QfcHomeControllerRunAsyncHighConfidenceTests.cs`. Acceptance, all three: every listed path has a numeric count and a computed headroom to 500; the artifact names `QuickFiler/Controllers/QfcHomeController.cs` as the lowest-headroom path this cycle edits and states its headroom as a number, because the R1 edit sits inside an existing method body and cannot be relocated to a new partial part; and the artifact records that `QuickFiler.Test/QuickFiler.Test.csproj` is edited by this plan but deliberately carries no census row, because the P2-T9 audit enumerates `.cs` files only.

- [x] [P0-T12] Capture `R_TIMESTAMP_PREIMAGE` with Derivation D9 into `docs/features/active/2026-08-28-quickfiler-carry-folder-predictor-to-item-controller-678/evidence/remediation-baseline/qa-gates-timestamp-preimage.md`. This task must complete before any task edits any file under `evidence/qa-gates/`, because writing a file replaces the mtime the correction is derived from. Acceptance, all five: the artifact lists exactly 13 file names, which are the complete directory listing of `evidence/qa-gates/`; each row carries the file name, its `LastWriteTime` to the second, and the `yyyy-MM-ddTHH-mm` truncation of that value; each row also carries the artifact's currently declared top-level `Timestamp:` value or the literal `NONE` where the file declares none, which is expected for `coverage-post-change.jacoco.xml`; the artifact separately enumerates the five nested `- Timestamp:` declarations inside `final-toolchain-pass.md`, naming for each the per-command artifact its own `Detail:` line references; and the artifact states the total number of `Timestamp:` declarations in scope for R4 as an integer, counting one top-level declaration per Markdown artifact plus the nested five.

---

### Phase 1 — R1 through R4

Ordering constraints. P1-T2 records the R1 red run and must run after P1-T1 and before P1-T3. P1-T7
records the R2 and R3 red run and must run after P1-T6 and before P1-T8. No suite-wide zero-failures
gate may run between P1-T1 and P1-T5, or between P1-T6 and P1-T10. P0-T12 must have completed before
P1-T12.

- [x] [P1-T1] Add the R1 regression test in the new file `QuickFiler.Test/Controllers/QfcHomeControllerRunAsyncHighConfidenceTests.Part3.cs`, declaring `public partial class QfcHomeControllerRunAsyncTests` in namespace `QuickFiler.Controllers.Tests` with no second `[TestClass]` attribute, since the attribute on the base part `QuickFiler.Test/Controllers/QfcHomeControllerRunAsyncTests.cs:23-24` covers the whole class, and add the entry `    <Compile Include="Controllers\QfcHomeControllerRunAsyncHighConfidenceTests.Part3.cs" />` to `QuickFiler.Test/QuickFiler.Test.csproj` beside the existing Part2 entry at `:157`. The test is named `RunAsync_HighConfidenceUnhookReplaced_LoadsPostUnhookItemSetAtLegABoundary`. It has two stages in one method. Stage one produces a genuine divergent batch by mirroring the arrangement of `DequeueNextItemGroupWithOutcomeAsync_DeadlineExpiredGate_ReportsDeadlineExpiredStop` at `QuickFiler.Test/Controllers/QfcQueuePurePathsTests.cs:202-260`: a `QfcDatamodel` obtained through `FormatterServices.GetUninitializedObject`, a `FakeTimeProvider` assigned to `TimeProvider`, which is mandatory because an uninitialized object leaves that property null and because `.claude/rules/general-unit-test.md` bans real wall-clock waits in tests, so if the gate's quantity-satisfied exit needs simulated time the test advances the fake clock explicitly rather than switching to `TimeProvider.System`, a `LockingLinkedList<MailItem>` master queue holding two loose `MailItem` mocks whose `EntryID` getters return the distinct values `entry-failed` and `entry-substitute`, a strict `IAppQuickFilerSettings` returning `HighConfidenceModeEnabled` true and `HighConfidenceThreshold` 0.90, a strict `IApplicationGlobals` exposing it, a strict `IFolderScoringService` supplied through `ScoringServiceFactory` that returns a score of 950 with a non-null `IFolderSearchHandler` mock, a strict `IEmailMoveMonitor` whose `UnhookItem` throws for the first item and succeeds for the second, and the private fields `_globals`, `_masterQueue`, `_moveMonitor`, `_worker` and `_remainingLoadActive` set by reflection. Stage one calls `model.DequeueNextItemGroupWithOutcomeAsync(1, 0, TimeSpan.FromSeconds(3), null)`. The quantity argument of 1 is load-bearing and is not a free choice: with 2 the gate accepts both queued items, `_masterQueue.TryTakeFirst()` at `QuickFiler/Controllers/QfcDatamodel.QueueProcessing.cs:55` returns null, no substitute is inserted at `:62`, and `batch.PreScored` holds two entries rather than the one the stage-one assertion requires. Stage two feeds the resulting `QfcDequeueBatch` into `_controller.RunAsync` through a `Mock<IQfcDatamodel>` whose `DequeueNextItemGroupWithOutcomeAsync` returns it and whose `Complete` returns true, with `SetupQfSettings(highConfidenceEnabled: true, threshold: 0.90)`, a `ProgressTracker` obtained from `SetupMockProgressTracker(tokenSource)` exactly as `QuickFiler.Test/Controllers/QfcHomeControllerRunAsyncHighConfidenceTests.cs:134-135` does, which is mandatory because `QfcHomeController.RunAsync` is declared at `QuickFiler/Controllers/QfcHomeController.cs:271` and its first statement at `:274` is `progress.Report(0, "Initializing Email Queue")`, so a null tracker throws a `NullReferenceException` there before the batch is ever read, a `Mock<IQfcFormController>` whose `ItemsPerIteration` is supplied through `SetupGet` because `RunAsync` reads it at `QuickFiler/Controllers/QfcHomeController.cs:277`, and whose `LoadItemsAsync(It.IsAny<IList<QfcPreScoredItem>>())` returns `Task.CompletedTask` and carries a `Callback<IList<QfcPreScoredItem>>` capturing the argument, and a `Mock<IQfcFormViewer>` supplying a `BackgroundWorker`. Acceptance, all five: the file is created and the `<Compile Include>` entry is present, proved by running `git add -N -- QuickFiler.Test` and then `git status --porcelain -- QuickFiler.Test` and recording both the new path and the modified `.csproj` path, the `git add -N` being required because an unstaged new file is invisible to a name-listing diff; the analyzer build command exits 0, proving the new file compiles against the current unfixed production code so the failure P1-T2 records is a runtime failure and not a build error; the test's stage-one assertions require `batch.Items` to hold exactly one element that is reference-equal to the substitute item and `batch.PreScored` to hold exactly one element whose `MailItem` is reference-equal to the failed item, so the divergence is produced by the real `TryUnhookOrReplace` throw branch and is never hand-built; the test's stage-two assertions require the captured carrier list to contain exactly one element, that element's `MailItem` to be reference-equal to the substitute, no element's `MailItem` to be reference-equal to the failed item, and that element's `FolderHandler` to be null because the substitute was never scored; and the test uses MSTest, Moq and FluentAssertions, creates no temporary file and requires no live Outlook COM. Evidence: `docs/features/active/2026-08-28-quickfiler-carry-folder-predictor-to-item-controller-678/evidence/regression-testing/r1-test-added.md`.

- [x] [P1-T2] [expect-fail] Record the R1 red run. Clear `TestResults\p1-t2`, run `msbuild TaskMaster.sln /t:Build /m /p:Configuration=Debug "/p:Platform=Any CPU"`, then run Derivation D7 with `'/TestCaseFilter:TestCategory!=LiveOutlook&FullyQualifiedName~RunAsync_HighConfidenceUnhookReplaced_LoadsPostUnhookItemSetAtLegABoundary'` and `'/ResultsDirectory:TestResults\p1-t2'`. Acceptance, all six: the pre-run build exits 0; the scoped run reports exactly 1 test discovered and executed, which is the discovery control that distinguishes a real failure from a test that never ran; the run reports that 1 test as failed; the recorded failure message is a FluentAssertions assertion failure on the captured carrier list, that is, on a stage-two assertion, and is neither a stage-one assertion failure, a build error, an assembly-load error, nor a `NullReferenceException`, the last being excluded by the `Task.CompletedTask` setup P1-T1 mandates, and a stage-one failure being excluded because it would mean the real `TryUnhookOrReplace` throw branch did not produce the divergence and the test therefore proves nothing about leg A; and the artifact's `Command:` and `EXIT_CODE:` fields record the Derivation D7 vstest invocation and not the preceding `msbuild /t:Build`, whose exit code is recorded inside `Output Summary:` instead, because `ExpectedExitCode:` is a per-file field and a build recorded as the artifact's command would be normalised against the declared expectation of 1; and the TRX under `TestResults\p1-t2` is summarised in `docs/features/active/2026-08-28-quickfiler-carry-folder-predictor-to-item-controller-678/evidence/regression-testing/r1-red.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, `ExpectedExitCode: 1` and `Output Summary:`.

- [x] [P1-T3] Implement the R1 fix. Add `ResolveCarrier` and `ReconcileCarriersToItems` as `internal static` members of `QfcPreScoredItem` in `QuickFiler/Controllers/QfcHighConfidencePreFilter.cs`, inside the struct declared at `:106-150`, following the reference shape in Derivation DR1; rewrite the body of `QfcQueue.ResolveCarriedHandler` at `QuickFiler/Controllers/QfcQueue.Enqueue.cs:143-168` to delegate to `QfcPreScoredItem.ResolveCarrier(preScored, mailItem)?.FolderHandler` without changing its signature or its accessibility, so exactly one carrier-matching body exists in the tree; rewrite the two doc blocks in that same file that state matching is by `EntryID` alone — the `ResolveCarriedHandler` summary at `:137-142`, whose sentence begins `Matching is by` at the end of `:139`, and the `EnqueueAsync` summary at `:58-67`, whose sentence `Carriers are matched to items by <c>EntryID</c> rather than by position, because` sits at `:63` — so both state that a carrier is matched first by reference identity and then by `EntryID`, adding the single-line token `#678 R1a` inside the first rewritten block and the single-line token `#678 R1b` inside the second; and replace the assignment at `QuickFiler/Controllers/QfcHomeController.cs:307` so that `preScored` is `QfcPreScoredItem.ReconcileCarriersToItems(batch.Items, batch.PreScored)`. Add the single-line token `#678 R1` in a comment at the reconciliation site. Acceptance, all eight: the analyzer build command exits 0; the nullable build command exits 0; the token `#678 R1` occurs exactly once in `QuickFiler/Controllers/QfcHomeController.cs`; `QuickFiler/Controllers/QfcHomeController.cs` measures at most 500 lines by Derivation D8, the comment being the flexible part of the edit if the budget is tight; the token `ReferenceEquals` occurs at least once in `QuickFiler/Controllers/QfcHighConfidencePreFilter.cs`, which is the identity-first matching clause DR1 requires and which no other line of that file contains today; the token `#678 R1a` occurs exactly once in `QuickFiler/Controllers/QfcQueue.Enqueue.cs`, on a single line; the token `#678 R1b` occurs exactly once in `QuickFiler/Controllers/QfcQueue.Enqueue.cs`, on a single line, the token `#678 R1` never being counted in that file so the shared prefix creates no confound; and no `[ExcludeFromCodeCoverage]` attribute is added or removed in any of the three edited files. Evidence: `docs/features/active/2026-08-28-quickfiler-carry-folder-predictor-to-item-controller-678/evidence/other/r1-reconciliation.md`, recording the three edited paths with their post-edit Derivation D8 counts and the before-and-after text of both rewritten `QfcQueue.Enqueue.cs` doc blocks.

- [x] [P1-T4] Correct the XML documentation block at `QuickFiler/Controllers/QfcDatamodel.QueueProcessing.cs:165-170`, which today asserts that `Items` and `PreScored` "describe one dequeue rather than two" unconditionally. The corrected block states that the correspondence holds on the happy path only, that on the `UnhookItem` throw path `TryUnhookOrReplace` at `:31-66` removes the failed item and inserts a substitute so `PreScored` can name an item absent from `Items` and `Items` can name an item absent from `PreScored`, and that leg A reconciles the two at the load boundary. Add the single-line token `#678 R1` inside that block. Acceptance, all four: the literal `describe one dequeue rather than two` occurs zero times in `QuickFiler/Controllers/QfcDatamodel.QueueProcessing.cs`; the token `#678 R1` occurs exactly once in that file; the token is on a single line, CSharpier not reflowing comment text; and the analyzer build command exits 0. Evidence: the same artifact as P1-T3, extended with the before-and-after text of the block.

- [x] [P1-T5] Record the R1 green run together with the three pins the fix must not break. Clear `TestResults\p1-t5`, run `msbuild TaskMaster.sln /t:Build /m /p:Configuration=Debug "/p:Platform=Any CPU"`, then run Derivation D7 with `'/TestCaseFilter:TestCategory!=LiveOutlook&(FullyQualifiedName~RunAsync_HighConfidenceUnhookReplaced_LoadsPostUnhookItemSetAtLegABoundary|FullyQualifiedName~RunAsync_HighConfidenceEnabled_LoadsFirstPageFromStreamingDequeue|FullyQualifiedName~ResolveCarriedHandler_WhenEntryIdMatchesACarrier_ReturnsThatCarriersHandler|FullyQualifiedName~ResolveCarriedHandler_WhenNoCarrierMatches_ReturnsNull)'` and `'/ResultsDirectory:TestResults\p1-t5'`. Acceptance, all four: the pre-run build exits 0; the scoped run reports exactly 4 tests discovered and executed, and each of the four names above appears individually in the run's executed-test list; all 4 pass; and the three pre-existing tests pass with their bodies unmodified, proved by `git status --porcelain -- QuickFiler.Test/Controllers/QfcHomeControllerRunAsyncHighConfidenceTests.cs QuickFiler.Test/Controllers/QfcQueuePurePathsTests.cs` producing no output, which at this point in the plan proves the two files are untouched by this cycle because P1-T14 is the first commit this cycle makes and has not yet run. A base-ref-anchored diff cannot serve here: the previous cycle rewrote `QfcHomeControllerRunAsyncHighConfidenceTests.cs` and `QfcQueuePurePathsTests.cs` relative to `807fb0bb6e5e49f43efa6b256b05960bf078ca19`, so an anchored diff is non-empty regardless of what this cycle does. Evidence: `docs/features/active/2026-08-28-quickfiler-carry-folder-predictor-to-item-controller-678/evidence/regression-testing/r1-green.md` with `Timestamp:`, `Command:`, `EXIT_CODE: 0` and `Output Summary:`.

- [x] [P1-T6] Land the R2 and R3 test changes in `QuickFiler.Test/Controllers/QfcItemController.FolderHandlingTests.Part2.cs`, which is 241 lines. Three edits, and no others in that file. First, correct the single untrue assertion at `:219-222`: `ProjectPredeterminedFolder(@"\\Archive\Projects\Active", string.Empty)` must assert the value `@"\Archive\Projects\Active"`, which is what `FolderPredictor.ProjectSuggestionPath` at `UtilitiesCS/OutlookObjects/Folder/FolderPredictor.cs:845-858` produces for a non-null globals with an empty archive root, the prefix being `"\"` and the remainder non-empty. This is the one correction constraint 4 authorises; the surrounding five assertions, the test name and the `[TestMethod]` attribute are untouched. Second, add the R2 boundary test named `AssignFolderComboBox_WhenEmptyArchiveRootAndLeadingSeparator_PreselectsProjectedFolder`, which arranges a `Mock<IApplicationGlobals>` whose `Ol.ArchiveRootPath` returns `string.Empty`, sets `_predeterminedFolder` to the raw value `@"\Projects\Active"`, sets `_folderHandler` through `BuildFolderHandlerWithArray` so the folder array holds the projected value `@"Projects\Active"`, configures the viewer mock so `FolderContains(@"Projects\Active")` returns true and `GetSelectedFolder()` returns `@"Projects\Active"`, calls `AssignFolderComboBox()`, and asserts `SetFolderSelectedItem(@"Projects\Active")` exactly once and `SetFolderSelectedIndex(It.IsAny<int>())` never, mirroring the assertion shape at `:192-203`. Third, add the R3 test named `LoadFolderHandlerAsync_WhenCarriedHandlerAndCancelledToken_ObservesCancellation`, which sets `_globals`, sets `_carriedFolderHandler` to a mock, injects the sentinel-throwing predictor factory built by `BuildThrowingPredictorFactoryMock()` at `:28-50`, passes the token of an already-cancelled `CancellationTokenSource` to `LoadFolderHandlerAsync`, and asserts that an `OperationCanceledException` is thrown, that the private field `_folderHandler` is null so the carried handler was not adopted, and that the predictor factory was invoked `Times.Never()`. Both anchored comparisons below use `HEAD` rather than `807fb0bb6e5e49f43efa6b256b05960bf078ca19`, because this file did not exist at the base ref — the previous cycle created it — so a base-anchored diff reports every line as an addition and zero removals, which would make a removal-count clause pass vacuously. `HEAD` is the correct anchor at this point in the plan because P1-T14 is the first commit this cycle makes and has not yet run. Acceptance, all five: the file contains exactly two more `[TestMethod]` declarations than at `HEAD`, proved by counting the token `[TestMethod]` in the output of `git show HEAD:QuickFiler.Test/Controllers/QfcItemController.FolderHandlingTests.Part2.cs` and in the file on disk after the edit, and recording both integers; the diff `git diff HEAD -- QuickFiler.Test/Controllers/QfcItemController.FolderHandlingTests.Part2.cs` shows exactly one removed line, which is the corrected assertion's expected-value line inside the region `:212-239`, and no other removal anywhere in the file, the added-line count being unconstrained because the two new tests and any reflow of the corrected line add lines; the analyzer build command exits 0, proving all three tests compile against the current unfixed production code so P1-T7 records runtime failures; the file measures at most 500 lines by Derivation D8; and the three tests use MSTest, Moq and FluentAssertions, create no temporary file and require no live Outlook COM. Evidence: `docs/features/active/2026-08-28-quickfiler-carry-folder-predictor-to-item-controller-678/evidence/regression-testing/r2-r3-tests-added.md`.

- [x] [P1-T7] [expect-fail] Record the R2 and R3 red run. Clear `TestResults\p1-t7`, run `msbuild TaskMaster.sln /t:Build /m /p:Configuration=Debug "/p:Platform=Any CPU"`, then run Derivation D7 with `'/TestCaseFilter:TestCategory!=LiveOutlook&(FullyQualifiedName~ProjectPredeterminedFolder_BoundaryCases_MatchFolderPredictorProjection|FullyQualifiedName~AssignFolderComboBox_WhenEmptyArchiveRootAndLeadingSeparator_PreselectsProjectedFolder|FullyQualifiedName~LoadFolderHandlerAsync_WhenCarriedHandlerAndCancelledToken_ObservesCancellation)'` and `'/ResultsDirectory:TestResults\p1-t7'`. Acceptance, all six: the pre-run build exits 0; the scoped run reports exactly 3 tests discovered and executed and names all three individually; all 3 are reported as failed; each failure is an assertion failure and none is a build error or an assembly-load error, and the recorded message for the R3 test states that no exception was thrown rather than that the wrong exception type was thrown; and the artifact's `Command:` and `EXIT_CODE:` fields record the Derivation D7 vstest invocation and not the preceding `msbuild /t:Build`, whose exit code is recorded inside `Output Summary:` instead, because `ExpectedExitCode:` is a per-file field; and the run is summarised in `docs/features/active/2026-08-28-quickfiler-carry-folder-predictor-to-item-controller-678/evidence/regression-testing/r2-r3-red.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, `ExpectedExitCode: 1` and `Output Summary:`.

- [x] [P1-T8] Implement the R2 fix in `QuickFiler/Controllers/QfcItemController.FolderHandling.cs`. Change the guard at `:255-258` so it reads `if (string.IsNullOrEmpty(folderPath) || archiveRootPath is null)`, change the second argument of the call at `:222-225` from `_globals?.Ol?.ArchiveRootPath` to `_globals is null ? null : (_globals.Ol?.ArchiveRootPath ?? string.Empty)`, and rewrite the XML documentation block at `:243-252` so it states that the projection mirrors `FolderPredictor.ProjectSuggestionPath` for every non-null `folderPath` and non-null `archiveRootPath`, that a null `archiveRootPath` stands for that member's `_globals is null` guard and yields the identity, and that the two deliberate divergences are a null or empty `folderPath` returned unchanged rather than dereferenced and a non-null globals with a null `Ol` treated as an empty archive root rather than reproducing a null dereference. Add the single-line token `#678 R2` inside that block. Acceptance, all five: the literal `A null or empty archive root` occurs zero times in `QuickFiler/Controllers/QfcItemController.FolderHandling.cs`; the token `#678 R2` occurs exactly once in that file, on a single line; the analyzer build command exits 0; the nullable build command exits 0; and the file measures at most 500 lines by Derivation D8. Evidence: `docs/features/active/2026-08-28-quickfiler-carry-folder-predictor-to-item-controller-678/evidence/other/r2-projection-alignment.md`.

- [x] [P1-T9] Implement the R3 fix in `QuickFiler/Controllers/QfcItemController.FolderHandling.cs`. Insert `cancel.ThrowIfCancellationRequested();` as the first statement inside the carried-handler adoption branch at `:68-77`, immediately after the `if (_carriedFolderHandler is not null)` opening brace and before the `_folderHandler = _carriedFolderHandler;` assignment, with a comment carrying the single-line token `#678 R3` that states why the observation is inside the branch rather than at the top of the member, namely that the pre-change `FromField` route reached the predictor through `await Task.Run(..., cancel)` at `:81-97` inside the `try` that opens at `:79`, and that hoisting the throw to the top of the member would place it before that `try` and remove the `logger.Error` at `:120` which the `catch (System.Exception e)` at `:118-122` emitted for an already-cancelled token on that route. Acceptance, all four: the token `#678 R3` occurs exactly once in that file, on a single line; the token `cancel.ThrowIfCancellationRequested();` occurs at least once in that file, which it does not today; the analyzer build command exits 0; and the nullable build command exits 0. Evidence: `docs/features/active/2026-08-28-quickfiler-carry-folder-predictor-to-item-controller-678/evidence/other/r3-cancellation-observation.md`.

- [x] [P1-T10] Record the R2 and R3 green run together with the five pins the two fixes must not break. Clear `TestResults\p1-t10`, run `msbuild TaskMaster.sln /t:Build /m /p:Configuration=Debug "/p:Platform=Any CPU"`, then run Derivation D7 with `'/TestCaseFilter:TestCategory!=LiveOutlook&(FullyQualifiedName~ProjectPredeterminedFolder_BoundaryCases_MatchFolderPredictorProjection|FullyQualifiedName~AssignFolderComboBox_WhenEmptyArchiveRootAndLeadingSeparator_PreselectsProjectedFolder|FullyQualifiedName~LoadFolderHandlerAsync_WhenCarriedHandlerAndCancelledToken_ObservesCancellation|FullyQualifiedName~LoadFolderHandlerAsync_WhenCarriedHandlerPresent_DoesNotInvokePredictorFactory|FullyQualifiedName~LoadFolderHandlerAsync_WhenCarriedHandlerPresentAndVarListProvided_InvokesPredictorFactory|FullyQualifiedName~AssignFolderComboBox_WhenArchiveRootedPredeterminedFolder_PreselectsThatFolder|FullyQualifiedName~AssignFolderComboBox_WhenPredeterminedFolderPresent_PreselectsThatFolder|FullyQualifiedName~AssignFolderComboBox_PredeterminedFolder_PreselectsByNameAndStillPopulates)'` and `'/ResultsDirectory:TestResults\p1-t10'`. Acceptance, all four: the pre-run build exits 0; the scoped run reports exactly 8 tests discovered and executed and names all eight individually, none of the eight substrings being a substring of another; all 8 pass; and the two pinned files `QuickFiler.Test/Controllers/QfcItemController.FolderHandlingTests.cs` and `QuickFiler.Test/Controllers/QfcItemController.FolderSuggestionsTests.cs` are untouched by this cycle, proved by `git status --porcelain -- QuickFiler.Test/Controllers/QfcItemController.FolderHandlingTests.cs QuickFiler.Test/Controllers/QfcItemController.FolderSuggestionsTests.cs` producing no output, which is conclusive at this point because P1-T14 is the first commit this cycle makes and has not yet run. A base-ref-anchored diff cannot serve here: the previous cycle modified `QfcItemController.FolderHandlingTests.cs` relative to `807fb0bb6e5e49f43efa6b256b05960bf078ca19`. Evidence: `docs/features/active/2026-08-28-quickfiler-carry-folder-predictor-to-item-controller-678/evidence/regression-testing/r2-r3-green.md` with `Timestamp:`, `Command:`, `EXIT_CODE: 0` and `Output Summary:`.

- [x] [P1-T11] Record the R2 decision in `docs/features/active/2026-08-28-quickfiler-carry-folder-predictor-to-item-controller-678/evidence/other/r2-decision.md`, which R2 acceptance clause 1 requires. Acceptance, all four: the artifact states that option 1, aligning the projection, was chosen and states the reason, namely that option 2 would leave the stated invariant false because the predictor's `FolderArray` entries are separator-stripped in the empty-archive-root state and an unstripped carried value cannot match at the `FolderContains` boundary; it names `UtilitiesCS/OutlookObjects/Folder/FolderPredictor.cs:845-858` as the parity target and records that the file was not modified, proved by two commands whose outputs are both recorded and both empty: `git diff 807fb0bb6e5e49f43efa6b256b05960bf078ca19 -- UtilitiesCS`, which covers the whole branch and is expected to be empty because the previous cycle's footprint also excluded `UtilitiesCS`, and `git status --porcelain -- UtilitiesCS`, which covers this cycle's uncommitted state and is the clause that can fail if this cycle edited the parity target; it enumerates the two deliberate remaining divergences and states that both are null-safety differences rather than projection differences; and it records which single existing assertion was corrected, by file, line and both its before and after expected values.

- [x] [P1-T12] Apply the R4 timestamp correction. For each Markdown artifact enumerated in `R_TIMESTAMP_PREIMAGE`, replace its top-level `Timestamp:` value with the `yyyy-MM-ddTHH-mm` column that P0-T12 recorded for that same file, and replace each of the five nested `- Timestamp:` values inside `final-toolchain-pass.md` with the corrected top-level value of the per-command artifact its own `Detail:` line references. `coverage-post-change.jacoco.xml` declares no `Timestamp:` and is not edited. Acceptance, all seven: every corrected value is the exact third-column value P0-T12 recorded for that file and no value is chosen by any other means; the corrected values are recorded in a table alongside the original values and the source mtimes; the artifact states the derivation method in one sentence, namely that each corrected value is the `yyyy-MM-ddTHH-mm` truncation of that artifact's own filesystem `LastWriteTime` captured before any edit, and that the five nested values are copied from the corrected values of the artifacts they reference; the ordering check is performed and recorded, listing the 12 Markdown artifacts that declare a top-level value sorted by that original declared value, `coverage-post-change.jacoco.xml` being excluded from the sort because it declares none, and stating whether the corrected sequence is non-decreasing in that order, with every inverting pair enumerated by both file names, both mtimes and both original values; the artifact states, where an inversion exists, that R4 acceptance clause 1's ordering sub-clause is superseded by real-clock fidelity and records the reason, namely that the declared ordering and the filesystem ordering genuinely disagree for at least the pair `mstest-coverage-run.md` (declared `2026-09-01T23-12`, mtime `2026-09-01 23:03`) and `csharpier-format.md` (declared `2026-09-01T23-45`, mtime `2026-09-01 22:42`), so no assignment of real clock values can preserve both properties, and the remediation-inputs statement that relative ordering is correct is itself inaccurate for that file; the count of corrected declarations is stated as an integer and equals the total P0-T12 recorded; and no `Command:`, `EXIT_CODE:`, `ExpectedExitCode:` or `Output Summary:` value is altered anywhere. Evidence: `docs/features/active/2026-08-28-quickfiler-carry-folder-predictor-to-item-controller-678/evidence/other/r4-timestamp-correction.md`.

- [x] [P1-T13] Prove that R4 altered no other field. Run `git diff HEAD -- docs/features/active/2026-08-28-quickfiler-carry-folder-predictor-to-item-controller-678/evidence/qa-gates` before any commit of the P1-T12 edit, so the comparison is against the last committed state of those artifacts rather than against the base ref, at which the artifacts did not yet exist. Acceptance, all four: every added line in that diff begins, after leading whitespace and an optional `- ` list marker, with the literal `Timestamp:`; every removed line does the same; the added-line count equals the removed-line count and both equal the declaration count P1-T12 recorded; and the diff touches no file outside `evidence/qa-gates/` and does not touch `coverage-post-change.jacoco.xml`. The artifact records the full diff output. Evidence: the P1-T12 artifact, extended with a `## No-other-field proof` section.

- [x] [P1-T14] Commit the production, test and evidence changes of Phase 1 so the anchored diffs in Phase 2 have a committed range to compare. Acceptance, all four: `git add -A -- QuickFiler QuickFiler.Test docs/features/active/2026-08-28-quickfiler-carry-folder-predictor-to-item-controller-678` followed by `git status --porcelain -- QuickFiler QuickFiler.Test` reports no remaining modified or untracked path under those two prefixes; `git diff --cached --name-only 807fb0bb6e5e49f43efa6b256b05960bf078ca19 -- QuickFiler QuickFiler.Test` lists at least the six paths `QuickFiler/Controllers/QfcHighConfidencePreFilter.cs`, `QuickFiler/Controllers/QfcQueue.Enqueue.cs`, `QuickFiler/Controllers/QfcHomeController.cs`, `QuickFiler/Controllers/QfcDatamodel.QueueProcessing.cs`, `QuickFiler/Controllers/QfcItemController.FolderHandling.cs` and `QuickFiler.Test/Controllers/QfcHomeControllerRunAsyncHighConfidenceTests.Part3.cs`; the commit message names issue #678 and this remediation cycle; and no path under `UtilitiesCS/`, `.claude/` or the repository-root `CLAUDE.md` appears in the staged name-only diff.

---

### Phase 2 — Final QC loop and cycle closure

The loop below is the mandatory toolchain order. If any of P2-T1 through P2-T5 fails or changes a
file under `QuickFiler/` or `QuickFiler.Test/`, restart the loop from P2-T1. A file that P2-T1
rewrote outside those two prefixes and that P2-T1 then restored does not count as a changed file for
this restart rule, because P2-T1 reproduces that rewrite on every pass and restores it on every pass.
Every command task in this phase is unconditional; `SKIPPED` is not a passing outcome for any of
them. If a restart rewrites an artifact, that artifact's `Timestamp:` is rewritten to the new real
clock value, which P2-T13 verifies.

Writing under `.claude/agent-memory/` is not part of this deliverable. The exclusion P2-T10 grants
that directory is a tolerance for session state an agent may have written incidentally.

- [x] [P2-T1] Run `dotnet tool run csharpier format .` and record `docs/features/active/2026-08-28-quickfiler-carry-folder-predictor-to-item-controller-678/evidence/qa-gates/remediation-csharpier-format.md`. Acceptance, all four: `EXIT_CODE: 0`; `Output Summary:` reproduces verbatim the summary line the run printed, noting that CSharpier prints a processed-file count rather than a rewritten-file count so that line alone does not distinguish a clean run from a repairing one; the task records `git status --porcelain` output taken immediately before and immediately after the command, which is the tree observation that does distinguish them, with every rewritten path listed by name; and any rewritten path outside the `QuickFiler/` and `QuickFiler.Test/` prefixes is restored to its base-ref content with `git checkout 807fb0bb6e5e49f43efa6b256b05960bf078ca19 --` followed by that path, each restoration recorded by path with the reason, because the footprint constraint forbids a change outside those prefixes. The command runs unconditionally; the restoration clause governs how its result is treated, not whether it runs.

- [x] [P2-T2] Run `dotnet tool run csharpier check .` and record `docs/features/active/2026-08-28-quickfiler-carry-folder-predictor-to-item-controller-678/evidence/qa-gates/remediation-csharpier-check.md`. The command runs unconditionally. Acceptance, all three: `EXIT_CODE:` is recorded; the reported set of files needing formatting contains no path under `QuickFiler/` or `QuickFiler.Test/`; and that set is either empty, in which case the exit code must be 0, or a subset of `R_BASELINE_FORMAT_DRIFT` restricted to paths restored by P2-T1, in which case every member is named and the artifact carries a line beginning `REMEDIATION-REQUIRED:` stating that a zero exit would require editing files outside the footprint and that the conflict is reported rather than resolved by editing them.

- [x] [P2-T3] Run `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` and record `docs/features/active/2026-08-28-quickfiler-carry-folder-predictor-to-item-controller-678/evidence/qa-gates/remediation-analyzer-build.md`. Acceptance, all three: `EXIT_CODE: 0` with a zero error count in the MSBuild summary; the warning count is at or below the `R_BASELINE_ANALYZER_SUMMARY` warning count from P0-T6, with any new warning named individually; and the number of `CoreCompile:` occurrences is recorded and is greater than zero, so the gate is demonstrably not vacuous.

- [x] [P2-T4] Run `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true` and record `docs/features/active/2026-08-28-quickfiler-carry-folder-predictor-to-item-controller-678/evidence/qa-gates/remediation-nullable-build.md`. Acceptance, all three: `EXIT_CODE: 0`; `Output Summary:` states that no `CS86` diagnostic was introduced relative to the P0-T7 enumeration; and the number of `CoreCompile:` occurrences is recorded and is greater than zero.

- [x] [P2-T5] Run `pwsh -NoProfile -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot .` and record `docs/features/active/2026-08-28-quickfiler-carry-folder-predictor-to-item-controller-678/evidence/qa-gates/remediation-mstest-coverage-run.md`. That script builds its inner vstest argument list at `scripts/vscode/Invoke-MSTestWithCoverage.ps1:76` and passes no `/Logger:trx`, no `/ResultsDirectory` and no console verbosity override, so its output names failing tests and prints run totals but never names a passing test. A per-test pass list cannot be read from it, so the twelve-name confirmation is taken from a second, scoped run issued in this same task. After the full-suite run, clear `TestResults\p2-t5` and run Derivation D7 with `'/TestCaseFilter:TestCategory!=LiveOutlook&(FullyQualifiedName~RunAsync_HighConfidenceUnhookReplaced_LoadsPostUnhookItemSetAtLegABoundary|FullyQualifiedName~RunAsync_HighConfidenceEnabled_LoadsFirstPageFromStreamingDequeue|FullyQualifiedName~ResolveCarriedHandler_WhenEntryIdMatchesACarrier_ReturnsThatCarriersHandler|FullyQualifiedName~ResolveCarriedHandler_WhenNoCarrierMatches_ReturnsNull|FullyQualifiedName~ProjectPredeterminedFolder_BoundaryCases_MatchFolderPredictorProjection|FullyQualifiedName~AssignFolderComboBox_WhenEmptyArchiveRootAndLeadingSeparator_PreselectsProjectedFolder|FullyQualifiedName~AssignFolderComboBox_WhenArchiveRootedPredeterminedFolder_PreselectsThatFolder|FullyQualifiedName~AssignFolderComboBox_WhenPredeterminedFolderPresent_PreselectsThatFolder|FullyQualifiedName~AssignFolderComboBox_PredeterminedFolder_PreselectsByNameAndStillPopulates|FullyQualifiedName~LoadFolderHandlerAsync_WhenCarriedHandlerPresent_DoesNotInvokePredictorFactory|FullyQualifiedName~LoadFolderHandlerAsync_WhenCarriedHandlerPresentAndVarListProvided_InvokesPredictorFactory|FullyQualifiedName~LoadFolderHandlerAsync_WhenCarriedHandlerAndCancelledToken_ObservesCancellation)'` and `'/ResultsDirectory:TestResults\p2-t5'`. D7's pre-run `/t:Build` step is not issued for this second run, because P2-T3 and P2-T4 have already rebuilt the solution in this same pass and no source has changed since. Acceptance, all six: the full-suite `EXIT_CODE:` is recorded; `Output Summary:` states whether the full-suite run printed the literal `Done. Coverage artifact:`; the full-suite total, passed, failed and skipped counts are recorded numerically; the set of failing test names is a subset of `R_BASELINE_FAILURE_SET`, the subset form being used deliberately because a repository-wide zero-failures assertion is not satisfiable when the baseline itself carries failures; the full-suite total discovered count is at least the `R_BASELINE_TOTALS` total plus 3, that added count of 3 being the `[TestMethod]` declarations added by P1-T1 and P1-T6; and the scoped run reports exactly 12 tests discovered and executed with 0 failed, and the TRX under `TestResults\p2-t5` names all twelve individually as passed, none of the twelve filter substrings being a substring of another.

- [x] [P2-T6] Prove the post-change coverage report is post-processed and record the figures in `docs/features/active/2026-08-28-quickfiler-carry-folder-predictor-to-item-controller-678/evidence/qa-gates/remediation-coverage-post-change.md`. Run Derivation D1; if P2-T5 did not print `Done. Coverage artifact:`, run Derivation D4 first and read the post-processed file, exactly as P0-T9 did. Acceptance, all five: the observed package-name list is recorded verbatim; it is a subset of the nine-name allowlist; it contains `QuickFiler` and no `log4net` entry; Derivation D2 output is recorded as six numeric values with line-rate and branch-rate also expressed as percentages to two decimal places; and the artifact states which of the two paths each side of the comparison used, and where the two sides used different paths records that both paths call `ConvertTo-KoverageCoberturaXml` with the same allowlist and separator and therefore produce the same denominator. Comparing an unfiltered report against a post-processed one is prohibited in either direction.

- [x] [P2-T7] Record the coverage comparison against this cycle's own Phase 0 baseline in `docs/features/active/2026-08-28-quickfiler-carry-folder-predictor-to-item-controller-678/evidence/qa-gates/remediation-coverage-delta.md`. Join Derivation D5 to Derivation D6 after normalising path separators. Derivation D5 is run twice with two different ref operands: once with the literal base SHA `807fb0bb6e5e49f43efa6b256b05960bf078ca19`, which spans the whole branch and therefore includes the previous cycle's lines, and once with the HEAD SHA that P0-T2 recorded, substituted as a literal, which isolates this cycle's own lines. Only the cycle-anchored figure is a pass or fail gate; the branch-wide figure is recorded for information. Gating on the branch-wide figure would let a line the previous cycle already shipped and already audited fail this cycle. Acceptance, all eight: `R_BASELINE_COVERAGE` from P0-T9 and the P2-T6 figures are both stated numerically and their differences in line-rate and branch-rate are stated, with the artifact naming P0-T9 explicitly as the baseline and stating that no figure from `plan.2026-08-31T21-12.md` was used; both changed-line covered-over-total figures are stated numerically with the ref operand each was derived from named, or `NOT APPLICABLE` with the reason when a denominator is zero; the cycle-anchored figure shows no reduction relative to the branch-wide figure that is unexplained; the count of added lines excluded as non-executable is stated for both ranges; each new or modified member in a non-exempt file is listed with its own covered-over-total figure and a pass or fail against 90 percent, the members expected in that list being `QfcPreScoredItem.ResolveCarrier`, `QfcPreScoredItem.ReconcileCarriersToItems`, `QfcQueue.ResolveCarriedHandler`, `QfcHomeController.RunAsync`, `QfcItemController.ProjectPredeterminedFolder`, `QfcItemController.AssignFolderComboBox` and `QfcItemController.LoadFolderHandlerAsync`, and any member below 90 percent is recorded as `REMEDIATION-REQUIRED` with its uncovered line numbers named; each modified member in a class carrying `[ExcludeFromCodeCoverage]` is listed as exempt with the reason, `QfcDatamodel.DequeueWithHighConfidenceGateWithOutcomeAsync` being the only expected entry and its change being comment-only; and the per-file figures for the five paths in P0-T10 are compared against `coverage-per-file-baseline.md` with no file showing a reduction that is not explained by a line deletion in that file; and the non-vacuity control `@($doc.SelectNodes('//class[@filename]')).Count` is recorded as an integer greater than zero for the D6 pass, so an empty per-member or per-file table is distinguishable from a derivation that ran with an unassigned `$doc`.

- [x] [P2-T8] Assert the `[ExcludeFromCodeCoverage]` invariant and record `docs/features/active/2026-08-28-quickfiler-carry-folder-predictor-to-item-controller-678/evidence/qa-gates/remediation-exclude-attribute-invariant.md`. Run `git add -A -- QuickFiler QuickFiler.Test` and then `git diff --cached 807fb0bb6e5e49f43efa6b256b05960bf078ca19 -- QuickFiler QuickFiler.Test`. Acceptance, both: the diff contains zero added lines and zero removed lines carrying the token `ExcludeFromCodeCoverage`, with both counts stated as 0; and the diff's total added-line and removed-line counts are recorded, so a zero attribute count taken over an empty diff is distinguishable from one taken over a real change.

- [x] [P2-T9] Audit file sizes after formatting has settled and record `docs/features/active/2026-08-28-quickfiler-carry-folder-predictor-to-item-controller-678/evidence/qa-gates/remediation-file-size-audit.md`. This task runs after P2-T1 because CSharpier reflow changes line counts. Run `git add -A -- QuickFiler QuickFiler.Test` first so files this cycle created are visible to the name-listing diff, which enumerates tracked changes only. The ref operand is the HEAD SHA that P0-T2 recorded, substituted as a literal, and not the base SHA `807fb0bb6e5e49f43efa6b256b05960bf078ca19`. A base-anchored diff lists 33 `.cs` files changed by the previous cycle, three of which are already over the 500-line cap — `QuickFiler.Test/Controllers/QfcFormControllerTests.cs` at 792, `QuickFiler/Controllers/QfcCollectionController.cs` at 2336 and `QuickFiler/Controllers/QfcQueue.cs` at 505 — and none of the three is edited by this plan or carried in `R_BASELINE_SIZE_CENSUS`, so a base-anchored audit reports three census gaps for files this cycle neither caused nor is authorised to close. Acceptance, all five: every `.cs` file listed by `git diff --cached --name-only <the HEAD SHA P0-T2 recorded> -- QuickFiler QuickFiler.Test` has its post-format count from Derivation D8 recorded; the listed set is recorded in full and every member is a file this cycle edited or created; no listed file exceeds 500 lines, or, for a file already over 500 at baseline, its count is at or below its `R_BASELINE_SIZE_CENSUS` value, and a listed file over 500 with no census entry is reported by name as a census gap rather than treated as a pass; `QuickFiler/Controllers/QfcHomeController.cs` is named individually with its post-format count and its remaining headroom, being the lowest-headroom file this cycle edits; and the one new file `QuickFiler.Test/Controllers/QfcHomeControllerRunAsyncHighConfidenceTests.Part3.cs` is named together with the `<Compile Include>` entry in `QuickFiler.Test/QuickFiler.Test.csproj` that references it, quoted verbatim. The three pre-existing over-cap paths named above are additionally recorded in the artifact with their current counts and marked out of scope under NB-6, so their exclusion is auditable rather than silent.

- [x] [P2-T10] Audit footprint confinement and record `docs/features/active/2026-08-28-quickfiler-carry-folder-predictor-to-item-controller-678/evidence/qa-gates/remediation-scope-confinement.md`. Run `git add -A -- QuickFiler QuickFiler.Test docs/features/active/2026-08-28-quickfiler-carry-folder-predictor-to-item-controller-678`, then `git diff --cached --name-only 807fb0bb6e5e49f43efa6b256b05960bf078ca19`, then `git status --porcelain` with no pathspec. Acceptance, all five: every path in the staged name-only diff begins with `QuickFiler/`, `QuickFiler.Test/` or `docs/features/active/2026-08-28-quickfiler-carry-folder-predictor-to-item-controller-678/`; the unscoped porcelain status reports no modified or untracked path outside those three prefixes, except that paths under `.claude/agent-memory/` are enumerated separately and excluded from the judgment because that directory is tracked and is agent-session state rather than a change to the product or to policy; no path under `UtilitiesCS/`, `.claude/rules/`, `.claude/skills/`, `artifacts/orchestration/` or the repository-root `CLAUDE.md` appears in either output; `.git/info/exclude` is unmodified, recorded from the unscoped porcelain status; and both command outputs are recorded in full. The staging step is required because a name-listing diff is blind to newly created files; the unscoped porcelain status is required because the staging pathspec would otherwise leave an out-of-scope path unreported.

- [x] [P2-T11] Assert the `issue.md` acceptance-criteria invariant and record `docs/features/active/2026-08-28-quickfiler-carry-folder-predictor-to-item-controller-678/evidence/issue-updates/remediation-ac-invariant.md`. Acceptance, all five: the SHA-256 digest of `issue.md` is recomputed with `Get-FileHash -Algorithm SHA256 -LiteralPath` and is byte-identical to `R_ISSUE_DIGEST` recorded by P0-T3, both digests being reproduced in the artifact, this digest comparison being used in place of a base-ref-anchored diff because the previous cycle already modified `issue.md` relative to the base ref and an anchored diff therefore cannot isolate this cycle; the count of lines matching `^- \[[ x]\] AC` is re-measured and equals the 23 recorded by P0-T3; the checked and unchecked split is re-measured and equals the 22 and 1 recorded by P0-T3; the single unchecked line is re-read verbatim and is byte-identical to the AC20 line P0-T3 recorded; and the artifact records `PostedAs: unknown` with the reason, since this plan performs no GitHub posting.

- [x] [P2-T12] Re-verify the five documentation tokens after formatting and record `docs/features/active/2026-08-28-quickfiler-carry-folder-predictor-to-item-controller-678/evidence/qa-gates/remediation-doc-token-check.md`. This task runs after P2-T1 because a formatter pass is the only step that could move a token onto a second line. Acceptance, all eight: the token `#678 R1` occurs exactly once in `QuickFiler/Controllers/QfcHomeController.cs`; the token `#678 R1` occurs exactly once in `QuickFiler/Controllers/QfcDatamodel.QueueProcessing.cs`; the token `#678 R2` occurs exactly once in `QuickFiler/Controllers/QfcItemController.FolderHandling.cs`; the token `#678 R3` occurs exactly once in `QuickFiler/Controllers/QfcItemController.FolderHandling.cs`; the token `#678 R1a` occurs exactly once in `QuickFiler/Controllers/QfcQueue.Enqueue.cs`; the token `#678 R1b` occurs exactly once in `QuickFiler/Controllers/QfcQueue.Enqueue.cs`; the literal `describe one dequeue rather than two` occurs zero times in `QuickFiler/Controllers/QfcDatamodel.QueueProcessing.cs`; and the literal `A null or empty archive root` occurs zero times in `QuickFiler/Controllers/QfcItemController.FolderHandling.cs`. Each of the eight counts is recorded as an integer with the search command used.

- [x] [P2-T13] Assert that every evidence artifact this cycle wrote carries a real clock value, which is the forward-looking half of R4, and record `docs/features/active/2026-08-28-quickfiler-carry-folder-predictor-to-item-controller-678/evidence/qa-gates/remediation-timestamp-fidelity.md`. Apply Derivation D9 to `evidence/remediation-baseline/`, `evidence/regression-testing/`, `evidence/other/`, `evidence/issue-updates/` and `evidence/qa-gates/`, restricted to the artifacts this plan created. Acceptance, all five: every artifact this plan created is listed by path with its declared `Timestamp:`, its `LastWriteTime` to the second, and the signed difference in whole minutes; the absolute difference is at most 5 minutes for every listed artifact, and any artifact exceeding that is corrected to its own mtime truncation and re-listed; the pre-existing artifacts of the previous cycle are excluded from this gate by name and counted, being the thirteen under `evidence/qa-gates/`, the nine under `evidence/other/`, the four under `evidence/regression-testing/` and the one under `evidence/issue-updates/`, twenty-seven in total, with the reason recorded for each group: the qa-gates thirteen because P1-T12 already corrected them and rewrote their mtimes in doing so, and the other fourteen because this plan neither created nor edited them; the three artifacts `remediation-timestamp-fidelity.md`, `remediation-final-toolchain-pass.md` and `remediation-final-commit.md` are excluded by name with the reason that they are written by or after this task, and the artifact states that each of those three records its own `Timestamp:` at its own write time; and the total number of artifacts checked is stated as an integer.

- [x] [P2-T14] Record the clean-pass declaration at `docs/features/active/2026-08-28-quickfiler-carry-folder-predictor-to-item-controller-678/evidence/qa-gates/remediation-final-toolchain-pass.md`. Acceptance, all four: the artifact names the five commands of P2-T1 through P2-T5 in order with each one's `Timestamp:`, `Command:`, `EXIT_CODE:` and `Output Summary:`, covering the four gates of format verification, analyzer build, nullable build and the MSTest run plus the format-apply step that precedes them; it states that all five ran in the same uninterrupted pass and that P2-T1 left no net change under `QuickFiler/` or `QuickFiler.Test/` during that pass, listing by name any path P2-T1 rewrote outside those prefixes and then restored; it states the number of loop restarts that occurred and the reason for each; and it records the four remediation items R1, R2, R3 and R4 with, for each, the evidence artifact path that closes it and the named test or token gate that pins it.

- [x] [P2-T15] Commit every evidence artifact produced by this plan and leave the worktree clean. This is the last task; no artifact is written after it. Acceptance, all four: the artifact is `docs/features/active/2026-08-28-quickfiler-carry-folder-predictor-to-item-controller-678/evidence/qa-gates/remediation-final-commit.md`, and `git status --porcelain` run after the commit and before this task's own check-off produces no output other than paths under `.claude/agent-memory/`, which are left uncommitted and are enumerated in that artifact with the reason, together with this task's own artifact and this plan file, both of which are committed by an amend after the check-off is written; `git diff --name-only 807fb0bb6e5e49f43efa6b256b05960bf078ca19 -- docs/features/active/2026-08-28-quickfiler-carry-folder-predictor-to-item-controller-678` lists every artifact path named in Phase 0, Phase 1 and Phase 2; no path under `coverage/` or `TestResults/` appears in that list; and the R4 correction is proved to have reached the branch by reading each of the twelve corrected Markdown artifacts back out of the commit with `git show HEAD:` followed by its path and recording that its `Timestamp:` value equals the corrected value tabulated in `evidence/other/r4-timestamp-correction.md`, twelve equalities in total. That read-back is used instead of a base-ref-anchored `--name-status` diff, which would report those artifacts as added rather than modified because they did not exist at `807fb0bb6e5e49f43efa6b256b05960bf078ca19`, and would therefore say nothing about whether the correction landed.

---

## Remediation-item index

| Item | Owning tasks | Red-run evidence | Green or closing evidence |
|---|---|---|---|
| R1 | P1-T1, P1-T2, P1-T3, P1-T4, P1-T5 | evidence/regression-testing/r1-red.md | evidence/regression-testing/r1-green.md |
| R2 | P1-T6, P1-T7, P1-T8, P1-T10, P1-T11 | evidence/regression-testing/r2-r3-red.md | evidence/regression-testing/r2-r3-green.md, evidence/other/r2-decision.md |
| R3 | P1-T6, P1-T7, P1-T9, P1-T10 | evidence/regression-testing/r2-r3-red.md | evidence/regression-testing/r2-r3-green.md |
| R4 | P0-T12, P1-T12, P1-T13, P2-T13 | not applicable; R4 is a record correction, not a behaviour change | evidence/other/r4-timestamp-correction.md, evidence/qa-gates/remediation-timestamp-fidelity.md |
