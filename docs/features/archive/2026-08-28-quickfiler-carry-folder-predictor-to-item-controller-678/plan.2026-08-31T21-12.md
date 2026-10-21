# 2026-08-28-quickfiler-carry-folder-predictor-to-item-controller (Plan)

- **Issue:** #678
- **Parent (optional):** none
- **Owner:** drmoisan
- **Last Updated:** 2026-08-31T21-12
- **Status:** Draft
- **Version:** 1.0
- **Work Mode:** minor-audit
- **Branch:** `bug/quickfiler-carry-folder-predictor-to-item-controller-678`
- **Base ref for every anchored diff in this plan:** the commit SHA recorded by P0-T3, which is `origin/main` as resolved at the start of Phase 0. Every anchored diff in Phase 1 and Phase 2 substitutes that literal SHA for the name `origin/main`, because `origin/main` is a remote-tracking ref that a concurrent fetch can advance mid-run, which would silently re-base every later diff on a different tree.

## Requirements source

The sole requirements source is the `## Acceptance Criteria` section of
`docs/features/active/2026-08-28-quickfiler-carry-folder-predictor-to-item-controller-678/issue.md`,
which carries `- Work Mode: minor-audit` and criteria AC1 through AC23. No acceptance criterion is
inferred from any other section of that file. `spec.md` and `user-story.md` do not exist in this
feature folder and must not be created; their presence is an integrity failure for `minor-audit`.

The preparation research at
`docs/features/active/2026-08-28-quickfiler-carry-folder-predictor-to-item-controller-678/research/2026-08-31T21-15-quickfiler-carry-folder-predictor-research.md`
corrected three premises in the issue body, and the acceptance criteria were written against the
corrected reading. Where the issue body and the research disagree, the research governs:

1. The live producer is the dequeue-time confidence gate. `QfcHighConfidencePreFilter.FilterAsync`
   is dormant and must remain dormant (AC13).
2. There are two re-scoring legs: leg A (first page, through `RunAsync`) and leg B (every subsequent
   page, through `IterateQueueAsync` into `QfcQueue`). Both are in scope (AC4, AC5, AC6).
3. `QuickFiler.Test/Controllers/QfcHomeControllerRunAsyncHighConfidenceTests.cs:246` and `:277` are
   inside high-confidence-DISABLED tests and are preserved verbatim. The enabled-mode sites that
   require rewrite are enumerated in full by P1-T10, and that enumeration, not this summary, is the
   authoritative list. It spans both
   `QuickFiler.Test/Controllers/QfcHomeControllerIssue218Tests.cs` and
   `QuickFiler.Test/Controllers/QfcHomeControllerRunAsyncHighConfidenceTests.cs`, and it is wider
   than the three sites the research document named, because the overload switch in P1-T5 also
   invalidates shared arrange steps that no verification line cites.

## Fail-closed evidence rule

Every evidence-producing task names its artifact path. A task whose artifact is absent, or whose
artifact omits any required field, stays unchecked. If any required baseline artifact, final-QC
artifact, or coverage-comparison artifact is missing, the verdict is BLOCKED or INCOMPLETE, never
PASS.

## Evidence location rule (non-overridable)

Every evidence artifact in this plan resolves under
`docs/features/active/2026-08-28-quickfiler-carry-folder-predictor-to-item-controller-678/evidence/`
with sub-kind `baseline`, `regression-testing`, `qa-gates`, `issue-updates` or `other`. Paths under
`artifacts/baselines/`, `artifacts/baseline/`, `artifacts/qa/`, `artifacts/qa-gates/`,
`artifacts/evidence/`, `artifacts/coverage/`, `artifacts/regression-testing/` and
`artifacts/post-change/` are forbidden for evidence and must not be used even if a delegation prompt
supplies one.

Each command-step artifact records `Timestamp:`, `Command:`, `EXIT_CODE:` and `Output Summary:`.
Baseline and final-QC test artifacts carry numeric coverage headline values, never placeholders.
No helper script is placed under `evidence/`.

## Toolchain commands (verbatim; do not substitute)

- Format apply: `dotnet tool run csharpier format .`
- Format verify: `dotnet tool run csharpier check .`
- Analyzers: `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
- Nullable / type-check: `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`
- Tests with coverage: `pwsh -NoProfile -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot .`

`/t:Rebuild` is load-bearing: a warm `/t:Build` skips `CoreCompile` on every project and the gate
becomes vacuous. `/p:Nullable=enable` must not be added; no project carries a `<Nullable>` element
and there is no `Directory.Build.props`, so adding it conscripts files that never opted in.

A bare `vstest.console.exe` invocation is prohibited. It omits
`/TestCaseFilter:TestCategory!=LiveOutlook` and would run a test requiring a live Outlook COM
instance. The scoped runs in Phase 1 use Derivation D7, which always carries that filter.

## Named baselines this plan refers to

- `BASELINE_FAILURE_SET` — the set of fully qualified test names reported as failed by the Phase 0
  coverage run (P0-T8). Later suite gates assert the post-change failing set is a subset of it.
- `BASELINE_FORMAT_DRIFT` — the file list reported by the Phase 0 `csharpier check .` run (P0-T5).
- `BASELINE_ANALYZER_SUMMARY` — the MSBuild warning and error counts recorded by P0-T6.
- `BASELINE_SIZE_CENSUS` — the per-file line counts recorded by P0-T12.
- `BASELINE_COVERAGE` — the root-level Cobertura figures recorded by P0-T9.

## Derivations (referenced by identifier from tasks; run from the worktree root)

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

These six attributes are written by `ConvertTo-KoverageCoberturaXml`
(`scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1:442-447`) and exist only on a post-processed
document, so D2 is meaningful only after D1 passes.

Derivation D3 — per-file line summary.

```powershell
foreach ($cls in $doc.SelectNodes('//class[@filename]')) {
    $s = Get-CoberturaClassLineSummary -ClassNode $cls
    '{0}|{1}|{2}' -f $cls.GetAttribute('filename'), $s.CoveredLines, $s.TotalLines
}
```

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

`Invoke-DotnetCoverageCollection` throws on a non-zero coverage exit code
(`scripts/vscode/Invoke-MSTestWithCoverage.ps1:235-237`) and
`Assert-CoberturaLineCoverageThreshold` throws below 80 percent
(`scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1:487-490`); both run before the `Set-Content`
at `scripts/vscode/Invoke-MSTestWithCoverage.ps1:343`. Either throw leaves the UNFILTERED report on
disk. Comparing a post-processed baseline against an unfiltered post-change report compares
different denominators and produces a regression figure that reflects the measurement rather than
the change. D4 restores the same post-processing
without the threshold assertion so both sides of the comparison are derived identically. When D4 is
used, D1, D2, D3 and D6 read `coverage/coverage.postprocessed.cobertura.xml` instead.
`coverage/*` is git-ignored (`.gitignore:144`), so neither raw file is ever committed.

Derivation D5 — added production lines relative to the base ref.

```powershell
$file = ''
$added = @{}
foreach ($line in (git diff --unified=0 origin/main -- QuickFiler)) {
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

Cobertura `filename` values carry native separators after `ConvertTo-KoverageRelativePath`, while
git reports forward slashes. Replace `/` with `\` in the git path before joining D5 to D6. An added
line with no `LineMap` entry is non-executable (brace, comment, attribute, declaration) and is
excluded from the changed-line denominator; that exclusion count is reported alongside the figure.

Derivation D7 — scoped MSTest run for a single test, retaining the live-Outlook exclusion.

```powershell
$vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'
$vstest = & $vswhere -latest -products * -find 'Common7\IDE\Extensions\TestPlatform\vstest.console.exe' | Select-Object -First 1
& $vstest 'QuickFiler.Test/bin/Debug/QuickFiler.Test.dll' '/Settings:scripts/vscode/TaskMaster.cli.runsettings' '/InIsolation' '/TestCaseFilter:TestCategory!=LiveOutlook&FullyQualifiedName~LoadFolderHandlerAsync_WhenCarriedHandlerPresent_DoesNotInvokePredictorFactory' '/Logger:trx' '/ResultsDirectory:TestResults\p1-t3'
```

D7 is preceded, in the same task, by
`msbuild TaskMaster.sln /t:Build /m /p:Configuration=Debug "/p:Platform=Any CPU"` and the run
proceeds only when that build exits 0. Without it the scoped run reads whatever
`QuickFiler.Test.dll` a previous task produced: a newly added test is not discovered at all, and a
test whose production dependency was just edited reports its previous result, so both the discovery
count and the pass or fail verdict describe a superseded assembly.
`QuickFiler.Test` is a legacy non-SDK project and builds to
`QuickFiler.Test/bin/Debug/QuickFiler.Test.dll` with no target-framework subfolder.
`/ResultsDirectory` is mandatory: without it the TRX lands in a `TestResults\` folder relative to
the current directory and later runs cannot be told apart. Each scoped run uses its own
`p#-t#` results subdirectory.

Derivation D8 — line count of a file.

```powershell
(Get-Content -LiteralPath 'QuickFiler/Controllers/QfcItemController.ViewerSetup.cs').Count
```

`Measure-Object -Line` reports a different value on a file without a trailing newline and must not be
used for the 500-line cap.

## Scope boundary

In scope: the `QuickFiler` and `QuickFiler.Test` projects and this feature folder only (AC23), across
both re-scoring legs (AC4, AC5, AC6).

Out of scope (AC22). Each item below is confirmed or not confirmed by the executor and, when
confirmed to be a real defect, is REPORTED for separate promotion and left unchanged in this branch:

1. The synchronous `QfcItemController.LoadFolderHandler` predictor-initialisation defect
   (`QuickFiler/Controllers/QfcItemController.FolderHandling.cs:27-55`).
2. De-exempting any `[ExcludeFromCodeCoverage]` class.
3. Splitting oversized files.
4. Adding `InitAsync` to `IFolderSearchHandler`.
5. Deleting the dormant post-display filter.
6. Consolidating the duplicated `MailItemHelper.FromMailItemAsync` calls.

No change to any file under `UtilitiesCS`, to `.claude/rules/`, to `CLAUDE.md`, or to any policy
document.

## File-size constraints that shape the change (AC21)

`QuickFiler/Controllers/QfcCollectionController.cs` and `QuickFiler/Controllers/QfcQueue.cs` are
already over the 500-line cap. `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs` ends at line
499 and `QuickFiler/Controllers/QfcItemController.Initialization.cs` ends at line 489.
`QuickFiler.Test/Controllers/QfcItemController.FolderHandlingTests.cs` ends at line 498. Additions of
a new member to any of these go into a NEW partial part rather than extending the file. A statement
added inside an existing method body cannot be relocated to another part; the single line P1-T7 adds
to `Cleanup` in
`QuickFiler/Controllers/QfcItemController.ViewerSetup.cs` is that case, and it takes that file from
499 lines to 500, which is at the cap and not over it.

A parameter added to an existing signature is the same kind of case and is also not relocatable on
its own, but the member that declares the signature is relocatable in full. Two files in this change
are governed by that distinction. `QuickFiler/Controllers/QfcItemController.Initialization.cs` ends
at line 489, so P0-T12 flags it as low-headroom, yet the parameter P1-T2 adds to the
`predeterminedFolder` constructor declared at `:86`, whose parameter list occupies `:87-95` one
parameter per line, cannot move to a new part by itself. The permitted remedies are, in order:
leave the constructor in place when the addition keeps the file at or below 500 lines, or move that
constructor together with its complete XML documentation block at `:77-85`, which opens with the
`/// <summary>` line at `:77`, in full into a new part, leaving no orphan documentation line in the
base file. The second file is `QuickFiler/Controllers/QfcCollectionController.cs`, handled by P1-T5.

Four test files sit close enough to the cap that a mandated collateral edit can breach it:
`QuickFiler.Test/Controllers/QfcCollectionControllerTests.cs` at 499,
`QuickFiler.Test/Controllers/QfcHomeControllerIterationTests.cs` at 497,
`QuickFiler.Test/Controllers/QfcStreamingDequeueConfidenceGateTests.cs` at 468, and
`QuickFiler.Test/Controllers/QfcFormControllerTests.cs` at 827, which is already over the cap and
must therefore not grow at all. Because CSharpier rewraps a call whose argument list crosses the
print width, an edit that adds one argument can add several lines. Where that would take one of
these files over 500, or over its `BASELINE_SIZE_CENSUS` count in the case of
`QfcFormControllerTests.cs`, the executor moves whole `[TestMethod]` members out of the file into a
new partial part rather than deleting or weakening any test. `QfcStreamingDequeueConfidenceGateTests`
is already `partial` at `QuickFiler.Test/Controllers/QfcStreamingDequeueConfidenceGateTests.cs:16`;
the other three are not, so relocation there also requires adding `partial` to the declarations at
`QuickFiler.Test/Controllers/QfcCollectionControllerTests.cs:24`,
`QuickFiler.Test/Controllers/QfcHomeControllerIterationTests.cs:26` and
`QuickFiler.Test/Controllers/QfcFormControllerTests.cs:20`, with no second `[TestClass]` attribute on
the new part. Adding a part to
`QfcCollectionController` additionally requires marking the class declaration at
`QuickFiler/Controllers/QfcCollectionController.cs:22` `partial`. Adding a part to the folder-handling
test class requires marking the declaration at
`QuickFiler.Test/Controllers/QfcItemController.FolderHandlingTests.cs:19` `partial`, with no second
`[TestClass]` attribute on the new part, mirroring
`QuickFiler.Test/Controllers/QfcItemController.InitializationTests.cs:30`. Both projects use explicit
`<Compile Include>` item lists, so every new `.cs` file needs an entry in
`QuickFiler/QuickFiler.csproj` or `QuickFiler.Test/QuickFiler.Test.csproj`.

## Coverage threshold reconciliation (AC20)

`CLAUDE.md` states a repository-wide floor of 80 percent line coverage and 90 percent for new
modules, classes and methods. `.claude/rules/general-unit-test.md` and `.claude/rules/quality-tiers.md`
state 85 percent line and 75 percent branch uniformly. Both repository-wide figures are recorded
numerically and reported. The gates this plan treats as blocking are change-scoped: no regression on
the changed lines, and at least 90 percent line coverage on each new or modified non-exempt member.
The repository-wide figure is additionally enforced by the runner itself, which throws below 80
percent. This plan supersedes no floor and grants no waiver; a repository-wide figure below a policy
floor at baseline is recorded as a pre-existing condition and reported, not silently accepted.

`FolderScoringService` (`QuickFiler/Controllers/QfcHighConfidencePreFilter.cs:166`),
`QfcCollectionController` (`QuickFiler/Controllers/QfcCollectionController.cs:21`) and `QfcDatamodel`
(`QuickFiler/Controllers/QfcDatamodel.cs:25`) carry `[ExcludeFromCodeCoverage]`. Lines added to those
three classes do not enter the coverage denominator and correspondingly cannot be pinned by a
coverage figure. Their behaviour is pinned instead by named tests landing in the non-exempt seams:
the gate propagation tests in
`QuickFiler.Test/Controllers/QfcStreamingDequeueConfidenceGateTests.cs`, and the datamodel
scoring-factory tests in `QuickFiler.Test/Controllers/QfcDatamodelTests.cs`. `QfcCollectionController`
is not pinned by an existing test: `CarrierLoad_SetsPredeterminedFolderOnItemGroup` at
`QuickFiler.Test/Controllers/QfcCollectionControllerTests.cs:302-326` replicates the group-level carry
rather than invoking `EncapsulateItemGroup`, as its own comment at `:309-310` states, so it does not
exercise any `QfcCollectionController` member. P1-T5 therefore states in `leg-a.md` which behaviour of
the exempt `QfcCollectionController` is left unpinned by any test and why the constructor-contract
assertion at `QuickFiler.Test/Controllers/QfcCollectionControllerDefects468Tests.cs:110` is the only
structural pin that survives the change.

---

### Phase 0 — Baseline capture and toolchain bootstrap

- [x] [P0-T1] Read the policy documents in the `policy-compliance-order` order and write `docs/features/active/2026-08-28-quickfiler-carry-folder-predictor-to-item-controller-678/evidence/baseline/phase0-instructions-read.md`. Acceptance: the artifact contains `Timestamp:`, `Policy Order:` and an explicit list naming all seven of `CLAUDE.md`, `.claude/rules/general-code-change.md`, `.claude/rules/general-unit-test.md`, `.claude/rules/csharp.md`, `.claude/rules/quality-tiers.md`, `.claude/rules/tonality.md` and `.claude/rules/plan-acceptance-gates.md`.

- [x] [P0-T2] Verify `minor-audit` integrity and record it in `docs/features/active/2026-08-28-quickfiler-carry-folder-predictor-to-item-controller-678/evidence/baseline/minor-audit-integrity.md`. Acceptance, all four: the token `- Work Mode: minor-audit` occurs in `issue.md`; the heading `## Acceptance Criteria` occurs in `issue.md`; each of the 23 identifiers `AC1.` through `AC23.` occurs in `issue.md` exactly once, recorded as 23 individual counts of 1; and neither `spec.md` nor `user-story.md` exists in the feature folder, recorded with `SearchScope:`, `SearchPatterns:` and `SearchResult:`.

- [x] [P0-T3] Record the base-ref anchor in `docs/features/active/2026-08-28-quickfiler-carry-folder-predictor-to-item-controller-678/evidence/baseline/base-ref-anchor.md`. Acceptance: the artifact records the output of `git rev-parse origin/main` and of `git merge-base origin/main HEAD` and states that the two values are equal. If they are not equal, the task stays unchecked and the executor reports the divergence rather than re-anchoring on a different ref.

- [x] [P0-T4] Bootstrap the toolchain with `dotnet tool restore` from the worktree root and record `docs/features/active/2026-08-28-quickfiler-carry-folder-predictor-to-item-controller-678/evidence/baseline/dotnet-tool-restore.md`. Acceptance: `EXIT_CODE: 0`, and `Output Summary:` records the CSharpier version string that the tool manifest pins, read directly from the repository-root file `dotnet-tools.json` rather than inferred from any tool output. That file, and not `.config/dotnet-tools.json`, is the manifest present in this tree.

- [x] [P0-T5] Run the baseline format verification `dotnet tool run csharpier check .` and record `docs/features/active/2026-08-28-quickfiler-carry-folder-predictor-to-item-controller-678/evidence/baseline/csharpier-check.md` with `Timestamp:`, `Command:`, `EXIT_CODE:` and `Output Summary:`. Acceptance: `Output Summary:` reproduces verbatim the final summary line the run printed and enumerates every path the run reported as needing formatting; that enumeration is `BASELINE_FORMAT_DRIFT` and is recorded even when it is empty. This is a read-only check command, so its exit code is a real signal.

- [x] [P0-T6] Run the baseline analyzer build `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` and record `docs/features/active/2026-08-28-quickfiler-carry-folder-predictor-to-item-controller-678/evidence/baseline/analyzer-build.md`. Acceptance: `EXIT_CODE:` recorded, and `Output Summary:` reproduces the MSBuild warning-count and error-count summary lines verbatim as `BASELINE_ANALYZER_SUMMARY`.

- [x] [P0-T7] Run the baseline nullable build `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true` and record `docs/features/active/2026-08-28-quickfiler-carry-folder-predictor-to-item-controller-678/evidence/baseline/nullable-build.md`. Acceptance: `EXIT_CODE:` recorded truthfully, and `Output Summary:` enumerates every `CS86` diagnostic reported, or states that none was reported.

- [x] [P0-T8] Run the baseline coverage suite `pwsh -NoProfile -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot .` and record `docs/features/active/2026-08-28-quickfiler-carry-folder-predictor-to-item-controller-678/evidence/baseline/mstest-coverage-run.md`. Acceptance, all four: `EXIT_CODE:` recorded; `Output Summary:` states whether the run printed the literal `Done. Coverage artifact:`, which is emitted only after post-processing and the on-disk write succeed; `Output Summary:` records the total, passed, failed and skipped test counts; and the fully qualified names of all failing tests are enumerated as `BASELINE_FAILURE_SET`, recorded as the empty set when there are none. `-SearchRoot .` is mandatory.

- [x] [P0-T9] Prove the baseline coverage report is post-processed and record the numeric figures in `docs/features/active/2026-08-28-quickfiler-carry-folder-predictor-to-item-controller-678/evidence/baseline/coverage-baseline.md`. Run Derivation D1; if P0-T8 did not print `Done. Coverage artifact:`, run Derivation D4 first and read the post-processed file. Acceptance, all four: the observed package-name list from D1 is recorded verbatim; it is a subset of the nine-name allowlist; it contains `QuickFiler` and no `log4net` entry; and Derivation D2 output is recorded as six numeric values under `Output Summary:` as `BASELINE_COVERAGE`, with the line-rate and branch-rate additionally expressed as percentages to two decimal places. No placeholder value is accepted.

- [x] [P0-T10] Write the compact baseline coverage summary to `docs/features/active/2026-08-28-quickfiler-carry-folder-predictor-to-item-controller-678/evidence/baseline/coverage-baseline.jacoco.xml` as a package-level JaCoCo `report` document whose per-package `counter` values are transcribed from Derivation D3 aggregated by package. Acceptance, all three: the file exists and is under 200 lines measured by Derivation D8; the file's `LINE` counter totals equal the `lines-covered` and `lines-valid` values recorded in P0-T9, where D3 is run with the node selection `//class` rather than `//class[@filename]` so it selects the same node set as `Get-CoberturaCoverageSummary` at `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1:117-128`, and any class node lacking a `filename` attribute is reported by count with its package name; and `docs/features/active/2026-08-28-quickfiler-carry-folder-predictor-to-item-controller-678/evidence/baseline/coverage-baseline.md` carries a line beginning `EVIDENCE_SUBSTITUTION:` recording the raw Cobertura report's measured line count from Derivation D8 and its measured byte size, and stating that the raw report is retained untracked under the git-ignored `coverage/` directory and is deliberately not committed because a full-repository Cobertura document is too large to carry in permanent history.

- [x] [P0-T11] Record the baseline per-file coverage of the files this change may touch in `docs/features/active/2026-08-28-quickfiler-carry-folder-predictor-to-item-controller-678/evidence/baseline/coverage-per-file-baseline.md`, using Derivation D3. Acceptance: the artifact carries one covered-over-total row for each of the twelve paths `QuickFiler/Controllers/QfcHighConfidencePreFilter.cs`, `QuickFiler/Controllers/QfcStreamingDequeueConfidenceGate.cs`, `QuickFiler/Controllers/QfcDatamodel.QueueProcessing.cs`, `QuickFiler/Controllers/QfcHomeController.cs`, `QuickFiler/Controllers/QfcHomeController.Iteration.cs`, `QuickFiler/Controllers/QfcItemGroup.cs`, `QuickFiler/Controllers/QfcCollectionController.cs`, `QuickFiler/Controllers/QfcQueue.cs`, `QuickFiler/Controllers/QfcItemController.cs`, `QuickFiler/Controllers/QfcItemController.Initialization.cs`, `QuickFiler/Controllers/QfcItemController.FolderHandling.cs` and `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs`, or records `NOT PRESENT IN REPORT` for a path with no row and states the reason.

- [x] [P0-T12] Record `BASELINE_SIZE_CENSUS` in `docs/features/active/2026-08-28-quickfiler-carry-folder-predictor-to-item-controller-678/evidence/baseline/file-size-census.md` using Derivation D8 for each of the twelve production paths listed in P0-T11 and for the thirteen test paths `QuickFiler.Test/Controllers/QfcItemController.FolderHandlingTests.cs`, `QuickFiler.Test/Controllers/QfcItemController.InitializationTests.cs`, `QuickFiler.Test/Controllers/QfcHomeControllerIssue218Tests.cs`, `QuickFiler.Test/Controllers/QfcHomeControllerRunAsyncHighConfidenceTests.cs`, `QuickFiler.Test/Controllers/QfcHighConfidencePreFilterTests.cs`, `QuickFiler.Test/Controllers/QfcDatamodelTests.cs`, `QuickFiler.Test/Controllers/QfcQueuePurePathsTests.cs`, `QuickFiler.Test/Controllers/QfcStreamingDequeueConfidenceGateTests.cs`, `QuickFiler.Test/Controllers/QfcStreamingDequeueConfidenceGateTests.Part2.cs`, `QuickFiler.Test/Controllers/QfcStreamingDequeueConfidenceGateTests.Part3.cs`, `QuickFiler.Test/Controllers/QfcFormControllerTests.cs`, `QuickFiler.Test/Controllers/QfcCollectionControllerTests.cs` and `QuickFiler.Test/Controllers/QfcHomeControllerIterationTests.cs`. The last five are censused because the P1-T4 widening reaches them: `QfcStreamingDequeueConfidenceGateTests.Part2.cs` and `.Part3.cs` pass inline two-value `scoreLoader` lambdas to the `CreateGate` helper declared at `QuickFiler.Test/Controllers/QfcStreamingDequeueConfidenceGateTests.cs:26`, and the other three carry the carrier-construction and enqueue-shape sites P1-T10 assigns to P1-T4 and P1-T6. Acceptance, all three: every listed path has a numeric count and a computed headroom to 500; the artifact names every listed path whose headroom is under 20 lines as requiring a new partial part, and for each such path states whether the edit the plan mandates for it is a whole member, which can be relocated, or a change inside an existing signature or method body, which cannot; and the artifact records that `QuickFiler/QuickFiler.csproj`, `QuickFiler.Test/QuickFiler.Test.csproj` and `docs/features/active/2026-08-28-quickfiler-carry-folder-predictor-to-item-controller-678/issue.md` are edited by this plan but deliberately carry no census row, because the 500-line audit in P2-T10 enumerates `.cs` files only and the General Code Change Policy exempts Markdown documentation from the file-size limit.

- [x] [P0-T13] Re-derive and record the complete carrier construction-site inventory required by AC3 in `docs/features/active/2026-08-28-quickfiler-carry-folder-predictor-to-item-controller-678/evidence/baseline/carrier-construction-sites.md`. Acceptance, all four: every occurrence of the token `new QfcPreScoredItem(` in `QuickFiler` and in `QuickFiler.Test` is listed with file and line; every occurrence of the token `IFolderScoringService` in `QuickFiler.Test` is listed with file and line and classified as a mock declaration, a strict-behaviour setup, or a reference of another kind; every occurrence of the token `ScoringServiceFactory` in `QuickFiler` and `QuickFiler.Test` is listed with file and line; and each list carries its own count, derived at the base ref recorded in P0-T3 rather than copied from the research document.

---

### Phase 1 — Constrained delegated implementation

Phase 1 is a delegated block, not a decomposition of the edit. The implementation engineer owns the
edit sequence within each task; this plan fixes the acceptance conditions and the ordering
constraints between tasks. Tasks P1-T2 and P1-T3 exist because a regression test that references a
member which does not yet exist causes a compile error across the whole test assembly and produces
no runtime failure to record; P1-T2 lands the compile seam so P1-T3 can record a genuine runtime
failure.

- [x] [P1-T1] Delegate implementation to the C# implementation engineer and record the handoff packet at `docs/features/active/2026-08-28-quickfiler-carry-folder-predictor-to-item-controller-678/evidence/other/implementation-handoff.md`. Acceptance, all six: the packet names AC1 through AC18 and AC21 through AC23 as the completion criteria; it reproduces the out-of-scope list AC22 item by item; it reproduces the three corrected premises from the Requirements source section above; it carries `BASELINE_SIZE_CENSUS` from P0-T12 as the file-size budget; it states that `QuickFiler.Test/Controllers/QfcItemController.FolderHandlingTests.cs` has insufficient headroom for new tests and that new tests go in a new partial part with a matching `<Compile Include>` entry; and it states that the implementation engineer edits no acceptance criterion text in `issue.md` and performs no check-off, and that check-off is performed by the executor per `acceptance-criteria-tracking` after the supporting evidence artifact verifies.

- [x] [P1-T2] Land the compile seam only: declare the carried `IFolderSearchHandler` member on `QuickFiler/Controllers/QfcItemController.cs` alongside `_predeterminedFolder` at `:248`, and the constructor or injection surface that stores it, with no adoption logic added to `LoadFolderHandlerAsync`. Acceptance, all four: the analyzer build command exits 0; the nullable build command exits 0; the token `_folderPredictorFactory(` still occurs inside the `varList is null` branch of `QuickFiler/Controllers/QfcItemController.FolderHandling.cs`, which spans `:60-106` before this change; and the reflection-based constructor assertions in `QuickFiler.Test` are enumerated by file and line with a verdict for each: the assertion at `QuickFiler.Test/Controllers/QfcItemController.FolderHandlingTests.cs:102-107` targets `FolderPredictor` and is unaffected, and the assertion at `QuickFiler.Test/Controllers/QfcCollectionControllerDefects468Tests.cs:110-131`, which requires `QfcCollectionController` to expose exactly one public constructor whose parameter 5 is typed `QuickFiler.Controllers.IQfcFormController`, is recorded as still holding, which constrains P1-T5 to add no second public constructor when it introduces a new partial part. Evidence: `docs/features/active/2026-08-28-quickfiler-carry-folder-predictor-to-item-controller-678/evidence/other/compile-seam.md`.

- [x] [P1-T3] [expect-fail] Add the AC16 single-initialisation regression test named `LoadFolderHandlerAsync_WhenCarriedHandlerPresent_DoesNotInvokePredictorFactory` in a new file `QuickFiler.Test/Controllers/QfcItemController.FolderHandlingTests.Part2.cs`, mark `QuickFiler.Test/Controllers/QfcItemController.FolderHandlingTests.cs:19` `partial`, add the matching `<Compile Include>` entry to `QuickFiler.Test/QuickFiler.Test.csproj`, and run Derivation D7. The test injects the `Object` of a Moq mock of the predictor-factory delegate type declared at `QuickFiler/Controllers/QfcItemController.cs:83-88` into the `_folderPredictorFactory` field by reflection, following the injection precedent at `QuickFiler.Test/Controllers/QfcItemController.FolderHandlingTests.cs:253`, configures that mock to throw a sentinel exception when invoked, injects a mock of the carried handler seam, and asserts the factory delegate was invoked zero times using a Moq `Times.Never()` assertion. Moq supports mocking a delegate type directly, so the `Times` assertion AC16 requires is expressible without introducing a new interface. Acceptance, all four: the scoped run reports exactly 1 test discovered and executed, which is the discovery control that distinguishes a real failure from a test that never ran; the run reports that 1 test as failed; the failure is a Moq verification failure or the sentinel exception, not a build error and not an assembly-load error; and the TRX under `TestResults\p1-t3` is summarised in `docs/features/active/2026-08-28-quickfiler-carry-folder-predictor-to-item-controller-678/evidence/regression-testing/ac16-red.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, `ExpectedExitCode: 1` and `Output Summary:`. No suite-wide zero-failures gate may run between this task and P1-T7.

- [x] [P1-T4] Implement the producer and carrier chain for AC1, AC2 and AC3: add the `IFolderSearchHandler` member and constructor parameter to `QfcPreScoredItem` at `QuickFiler/Controllers/QfcHighConfidencePreFilter.cs:98-122` without renaming or retyping `MailItem` or `PredeterminedFolder`; widen `IFolderScoringService.ScoreAsync` at `:143-147` and `FolderScoringService.ScoreAsync` at `:170-189` to publish the handler initialised at `:184`; widen the `scoreLoader` delegate and the acceptance projection of `QuickFiler/Controllers/QfcStreamingDequeueConfidenceGate.cs`, whose accepted-item construction is at `:195`; and forward the handler from `QfcDatamodel.QueueProcessing.ScoreRemainingQueueMailItemAsync` at `QuickFiler/Controllers/QfcDatamodel.QueueProcessing.cs:263-277`. Acceptance, all four: the analyzer build exits 0; the `[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]` attribute at `QuickFiler/Controllers/QfcHighConfidencePreFilter.cs:166` and the justification remark block immediately above it are unchanged; every construction site enumerated in P0-T13 populates the new member; and `docs/features/active/2026-08-28-quickfiler-carry-folder-predictor-to-item-controller-678/evidence/other/carrier-chain.md` records the post-change construction-site list and states that its member set equals the P0-T13 list.

- [x] [P1-T5] Implement leg A for AC4 and AC5: switch the high-confidence-enabled branch of `QfcHomeController.RunAsync` to the outcome-returning dequeue member `DequeueNextItemGroupWithOutcomeAsync` declared at `QuickFiler/Interfaces/IQfcDatamodel.cs:113` and select the `IList<QfcPreScoredItem>` overload of `LoadItemsAsync` in place of the unconditional call at `QuickFiler/Controllers/QfcHomeController.cs:307`; carry the handler on `QfcItemGroup` alongside `PredeterminedFolder` at `QuickFiler/Controllers/QfcItemGroup.cs:50`; and thread it through `QfcCollectionController.EncapsulateItemGroup` at `QuickFiler/Controllers/QfcCollectionController.cs:646` and the `QfcPreScoredItem` overload of `LoadControlsAndHandlers_01Async` at `:487` into the `QfcItemController` constructor. Acceptance, all four: the analyzer build exits 0; the high-confidence-disabled branch of `RunAsync` still selects the `IList<MailItem>` overload; any new member added to `QuickFiler/Controllers/QfcCollectionController.cs` lands in a new partial part with `partial` added at `:22` and a `<Compile Include>` entry in `QuickFiler/QuickFiler.csproj`, and because `EncapsulateItemGroup` at `:646` and `LoadControlsAndHandlers_01Async` at `:487` each gain a parameter on its own line under CSharpier and the file is already 2446 lines, both methods are moved in full into that new part so the base file's count does not rise above its `BASELINE_SIZE_CENSUS` value; and `docs/features/active/2026-08-28-quickfiler-carry-folder-predictor-to-item-controller-678/evidence/other/leg-a.md` records the file list changed with per-file line counts from Derivation D8.

- [x] [P1-T6] Implement leg B for AC6: forward `batch.PreScored` from `QfcHomeController.IterateQueueAsync`, which today reads only `batch.Items` at `QuickFiler/Controllers/QfcHomeController.Iteration.cs:28` and calls `EnqueueAsync` at `:33`, into `QfcQueue.EnqueueAsync` at `QuickFiler/Controllers/QfcQueue.cs:211`, and carry the handler to the `new QfcItemController(` construction at `QuickFiler/Controllers/QfcQueue.cs:405`. Where a seam is required to make this assertable, use the injectable-delegate seam described at `.claude/rules/csharp.md:52`, mirroring the existing `ScoringServiceFactory` pattern at `QuickFiler/Controllers/QfcDatamodel.QueueProcessing.cs:260-261`; introduce no new interface. Acceptance, all four: the analyzer build exits 0; `QuickFiler/Controllers/QfcQueue.cs` is at or below its `BASELINE_SIZE_CENSUS` count of 610, achieved by moving `EnqueueAsync` at `:211` and `LoadControllersViewersAsync` at `:380`, which is the member whose body contains the `new QfcItemController(` construction at `:405`, in full into a new partial part, because each gains a parameter or argument on its own line under CSharpier and a widened signature cannot be split across parts while the construction at `:405` sits inside a lambda in that member's body and so is not itself a relocatable unit, with that new part carrying a `<Compile Include>` entry in `QuickFiler/QuickFiler.csproj` and `partial` added to the declaration at `QuickFiler/Controllers/QfcQueue.cs:20`, which is `public class QfcQueue(` and carries a primary constructor whose parameter list must remain on that part alone; the seam has a production default that preserves the current construction expression; and `docs/features/active/2026-08-28-quickfiler-carry-folder-predictor-to-item-controller-678/evidence/other/leg-b.md` names the seam introduced and the test that drives it, and the `IQfcQueue.EnqueueAsync` setup at `QuickFiler.Test/Controllers/QfcHomeControllerIterationTests.cs:133` and verifications at `:175` and `:282`, together with the `DequeueNextItemGroupWithOutcomeAsync` setups and verifications at `:118`, `:194`, `:221` and `:253` in the same file, are each recorded as either unchanged or rewritten with a named reason, and no test in that file is left failing.

- [x] [P1-T7] Implement adoption and the single-initialisation invariant for AC7, AC8, AC9, AC10, AC11 and AC14: adopt a carried handler inside the `varList is null` branch of `QfcItemController.LoadFolderHandlerAsync` only; leave the `FromArrayOrString` branch and both `FolderPredictor.InitOptions.FromArrayOrString` paths of `LoadFolderHandler` and `LoadFolderHandlerAsync` unchanged; release the carried handler in `Cleanup` alongside the first of the two `_folderHandler = null;` statements, at `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs:465`; the duplicate at `:468` is pre-existing and is left in place, since removing it is not required by any acceptance criterion; and leave the `QfcDequeueStop` handling and the null-not-empty early return of the carrier overload at `QuickFiler/Controllers/QfcFormController.Actions.cs:125-135` unchanged. Acceptance, all five: the AC16 test from P1-T3 now passes on a re-run of Derivation D7 with a new `p1-t7` results directory, recorded in `docs/features/active/2026-08-28-quickfiler-carry-folder-predictor-to-item-controller-678/evidence/regression-testing/ac16-green.md`; the existing test `LoadFolderHandlerAsync_WhenVarListNull_InvokesFactoryWithExpectedArgs`, declared at `QuickFiler.Test/Controllers/QfcItemController.FolderHandlingTests.cs:230`, passes with its body unmodified; the existing test `LoadFolderHandlerAsync_WhenVarListProvided_InvokesFactoryWithArrayOrStringArgs`, declared at `:264`, passes with its body unmodified; the existing test `LoadFolderHandlerAsync_WhenPrimaryFactoryThrowsArgumentNull_InvokesEmptyFactoryFallback`, declared at `:298`, passes with its body unmodified; and the four existing `AssignFolderComboBox` tests declared at `:416` (`AssignFolderComboBox_WhenNoPredeterminedFolder_SelectsTopSuggestionViaViewer`), `:440` (`AssignFolderComboBox_WhenPredeterminedFolderPresent_PreselectsThatFolder`), `:465` (`AssignFolderComboBox_WhenFolderHandlerNull_DoesNotTouchViewer`) and `:481` (`AssignFolderComboBox_WhenSingleSuggestionNoPredeterminedMatch_SelectsIndexZero`) each pass with their bodies unmodified, which together cover the predetermined-folder case and the index fallback cases AC11 names. This task additionally names the source-text test `LoadFolderHandler_ProbabilityDebugLog_IncludesCallerSubjectEntryIdAndTopScore`, declared at `QuickFiler.Test/Controllers/QfcItemController.FolderHandlingTests.cs:133`, which reads `QuickFiler/Controllers/QfcItemController.FolderHandling.cs` from disk at `:120-129` and asserts five string literals against its source text: that test must still pass after this task's edit and after the P2-T1 reformat, and if it fails the failure is attributed to a literal moved or reflowed rather than treated as a behavioural regression.

- [x] [P1-T8] Add the AC9 negative guard test named `LoadFolderHandlerAsync_WhenCarriedHandlerPresentAndVarListProvided_InvokesPredictorFactory`, asserting a carried handler is ignored when `varList` is non-null, in `QuickFiler.Test/Controllers/QfcItemController.FolderHandlingTests.Part2.cs`. The Derivation D7 run for this task substitutes that name into the `FullyQualifiedName~` operand and uses `/ResultsDirectory:TestResults\p1-t8`. Acceptance, all three: the new test arranges both a carried handler and a non-null `varList` and asserts the sentinel-throwing `_folderPredictorFactory` IS invoked; a scoped Derivation D7 run naming that test reports exactly 1 test discovered and 1 passed, recorded in `docs/features/active/2026-08-28-quickfiler-carry-folder-predictor-to-item-controller-678/evidence/regression-testing/ac9-negative-guard.md`; and the test uses MSTest, Moq and FluentAssertions with no temporary file and no live Outlook COM.

- [x] [P1-T9] [expect-fail] Resolve the raw-versus-projected path mismatch for AC12 and AC11. The `[expect-fail]` tag governs the first of the two runs this task records; the second run is a normal pass gate. `FolderScoringService.ScoreAsync` returns the raw suggestion path at `QuickFiler/Controllers/QfcHighConfidencePreFilter.cs:187` while `FolderPredictor.FolderArray` stores the archive-prefix-stripped projection, so `_itemViewer.FolderContains` fails for archive-rooted suggestions and the selection silently falls back to index 1. Acceptance, all five: one side is normalised so the carried `PredeterminedFolder` and the `FolderArray` entries use the same form; a new test named `AssignFolderComboBox_WhenArchiveRootedPredeterminedFolder_PreselectsThatFolder`, added to `QuickFiler.Test/Controllers/QfcItemController.FolderHandlingTests.Part2.cs`, asserts `SetFolderSelectedItem` is invoked once with the archive-rooted path and `SetFolderSelectedIndex` is invoked `Times.Never()`, mirroring the existing assertion shape at `QuickFiler.Test/Controllers/QfcItemController.FolderHandlingTests.cs:456-460`; the two Derivation D7 runs for this task substitute that name into the `FullyQualifiedName~` operand and use `/ResultsDirectory:TestResults\p1-t9-red` and `TestResults\p1-t9-green`; that test is recorded as failing against the unnormalised form and passing after, in `docs/features/active/2026-08-28-quickfiler-carry-folder-predictor-to-item-controller-678/evidence/regression-testing/ac12-path-normalisation.md`; and the chosen normalisation and the reason for choosing that side are stated in the change description written by P1-T11.

- [x] [P1-T10] Reconcile the pinned test suite for AC13, AC17 and AC18. Rewrite, without deleting or weakening, every enabled-mode assertion and arrange step that P1-T5's overload switch invalidates. In `QuickFiler.Test/Controllers/QfcHomeControllerIssue218Tests.cs`: the shared `DequeueNextItemGroupAsync` setup at `:102`; inside `RunAsync_HighConfidenceEnabled_DoesNotPreFilterInitialGuiBatch` declared at `:138`, the `LoadItemsAsync(IList<MailItem>)` `Times.Once` verification at `:160-164`, the `DequeueNextItemGroupAsync` `Times.Once` verification at `:165-176`, and the carrier `Times.Never` verification at `:177-181`; inside `RunAsync_HighConfidence_LoadsInitialBatchWithoutPreFilter` declared at `:185`, the `DequeueNextItemGroupAsync` setup at `:206`, the `LoadItemsAsync(IList<MailItem>)` setup and sequence callback at `:221-223`, the `sequence.Should().Equal("LoadItemsAsync")` assertion at `:244`, the `DequeueNextItemGroupAsync` `Times.Once` verification at `:245-254`, and the carrier `Times.Never` verification at `:255-258`. In `QuickFiler.Test/Controllers/QfcHomeControllerRunAsyncHighConfidenceTests.cs`: the shared `ArrangeRunAsyncController` dequeue setups at `:44-56`, which configure only `DequeueNextItemGroupAsync` and must additionally configure `DequeueNextItemGroupWithOutcomeAsync`; the enabled-mode dequeue and load verifications at `:180-201`; the enabled-mode `RunAsync_HighConfidenceScanProgress_MapsReportsIntoTheZeroToThirtyBand` declared at `:289`, whose dequeue setup is at `:318` and whose `IList<MailItem>` load setup is at `:347`; and the enabled-mode `RunAsync_HighConfidenceEmptyBatch_StillLoadsItemsAndStartsIteration` declared at `:396`, whose dequeue setup is at `:420`, whose load setup is at `:446` and whose `LoadItemsAsync(It.Is<IList<MailItem>>(items => items.Count == 0))` `Times.Once` assertion is at `:462-463`. Every site listed here carries a named reason in the reconciliation artifact; a site not listed here is not rewritten for the reason this task governs, which is the leg-A overload switch. Collateral edits the compiler forces elsewhere in `QuickFiler.Test` are owned by the task that causes them and are recorded there, not here: the `MockBehavior.Strict` `IFolderScoringService` mocks at `QuickFiler.Test/Controllers/QfcDatamodelTests.cs:337`, `QuickFiler.Test/Controllers/QfcHighConfidencePreFilterTests.cs:72` and `QuickFiler.Test/Controllers/QfcQueuePurePathsTests.cs:160` and `:221`, the `new QfcPreScoredItem(` sites at `QuickFiler.Test/Controllers/QfcCollectionControllerTests.cs:307` and `QuickFiler.Test/Controllers/QfcFormControllerTests.cs:814`, the `scoreLoader` delegate shape at `QuickFiler.Test/Controllers/QfcStreamingDequeueConfidenceGateTests.cs:28` together with the exact-type constructor lookup that repeats that shape at `:54` and the inline two-value `scoreLoader` lambdas passed to `CreateGate` throughout that file and its `QfcStreamingDequeueConfidenceGateTests.Part2.cs` and `QfcStreamingDequeueConfidenceGateTests.Part3.cs` parts, and the `Task<(long Score, string TopFolder)>` return shape at `QuickFiler.Test/Controllers/QfcDatamodelTests.cs:370` and `:385` belong to P1-T4; the reflection constructor pin `PredeterminedFolderConstructor_StoresPredeterminedFolder` at `QuickFiler.Test/Controllers/QfcItemController.InitializationTests.cs:91-123`, which is extended rather than rewritten if the constructor gains a parameter, belongs to P1-T5; and the `IQfcQueue.EnqueueAsync` sites in `QuickFiler.Test/Controllers/QfcHomeControllerIterationTests.cs` belong to P1-T6. The lookup at `QfcStreamingDequeueConfidenceGateTests.cs:48-64` fails closed by design, as its own comment at `:43-47` records, so leaving it unwidened does not degrade quietly: it makes every test in that partial class fail. Acceptance, all six: the disabled-mode `Times.Never` verifications inside `RunAsync_HighConfidenceDisabled_DoesNotPreFilterUsesPlainOverload` at `QuickFiler.Test/Controllers/QfcHomeControllerRunAsyncHighConfidenceTests.cs:246` and inside `RunAsync_HighConfidenceDisabled_UsesPlainOverloadOnly` at `:277` are byte-identical to their base-ref text; the `preFilterInvoked` assertion at `QuickFiler.Test/Controllers/QfcHomeControllerIssue218Tests.cs:157` and the `preFilterInvoked` assertion at `QuickFiler.Test/Controllers/QfcHomeControllerRunAsyncHighConfidenceTests.cs:239` are each byte-identical to their base-ref text, and both are recorded by file, line and quoted text; the `Times.Never` verification on the unfiltered initialization batch at `QuickFiler.Test/Controllers/QfcHomeControllerRunAsyncHighConfidenceTests.cs:202-209` is rewritten onto the carrier overload so it asserts `LoadItemsAsync(It.Is<IList<QfcPreScoredItem>>(...))` was never invoked with a carrier list projected from `unfilteredInitialBatch`, and the artifact records that leaving the original `IList<MailItem>` form in place would satisfy it trivially after the change because that overload is no longer invoked at all in enabled mode; no `[TestMethod]` is deleted anywhere in `QuickFiler.Test`, proved by comparing the `[TestMethod]` count at the base ref with the post-change count and reporting both numbers; every test rewritten by this task still uses MSTest, Moq and FluentAssertions, creates no temporary file and requires no live Outlook COM, as AC18 requires; and `docs/features/active/2026-08-28-quickfiler-carry-folder-predictor-to-item-controller-678/evidence/other/test-reconciliation.md` records one named reason for every changed test.

- [x] [P1-T11] Write the change description at `docs/features/active/2026-08-28-quickfiler-carry-folder-predictor-to-item-controller-678/evidence/other/change-description.md`. Acceptance, all three: it states the AC12 normalisation decision and which side was normalised; it states the AC15 accepted behavioural delta, that reusing the scan-time suggestion set freezes conversation-derived `CtfMap` suggestions at scan time rather than re-deriving them at display time, for both legs, and that the scan-to-display interval is longer for leg B; and it states that Bayesian suggestions and the recents list are unaffected because the folder array is still built lazily at display time.

- [x] [P1-T12] Record the AC22 out-of-scope register at `docs/features/active/2026-08-28-quickfiler-carry-folder-predictor-to-item-controller-678/evidence/other/out-of-scope-register.md`. Acceptance, all three: each of the six out-of-scope items listed in the Scope boundary section above carries a verdict of `CONFIRMED-DEFECT` or `NOT-CONFIRMED` with the file and line the verdict rests on; each `CONFIRMED-DEFECT` item carries a referral record naming the promotion route it is handed to, so the follow-up carries a named owner rather than being left unassigned; and no source file outside the change footprint required by AC1 through AC18 is modified for any of the six.

- [x] [P1-T13] Commit the production and test changes on the feature branch so the anchored diffs in Phase 2 have a committed range to compare. Acceptance, all three: `git status --porcelain` reports no modified or untracked path under `QuickFiler` or `QuickFiler.Test`; `git diff --name-only origin/main -- QuickFiler QuickFiler.Test` lists at least one path; and the commit message names issue #678.

---

### Phase 2 — Final QC loop and reduced-audit handoff

The loop below is the mandatory toolchain order. If any task in P2-T1 through P2-T5 fails or changes
a file, restart the loop from P2-T1. A file that P2-T1 rewrote outside the `QuickFiler/` and
`QuickFiler.Test/` prefixes and that P2-T1 then restored under its AC23 clause does not count as a
changed file for this restart rule, because P2-T1 reproduces that rewrite on every pass and restores
it on every pass, so treating it as a change makes the loop non-terminating; the restart trigger is a
net change under `QuickFiler/` or `QuickFiler.Test/` after restoration. Every command task in this
phase is unconditional; `SKIPPED` is not a passing outcome for any of them.

Writing agent memory under `.claude/agent-memory/` is not required by this change and is not part of
the deliverable. The exclusions that P2-T11 and P2-T15 grant that directory are a tolerance for
session state an agent may have written incidentally, not an invitation to write there.

- [x] [P2-T1] Run `dotnet tool run csharpier format .` and record `docs/features/active/2026-08-28-quickfiler-carry-folder-predictor-to-item-controller-678/evidence/qa-gates/csharpier-format.md`. Acceptance, all four: `EXIT_CODE: 0`; `Output Summary:` reproduces verbatim the summary line the run printed, noting that CSharpier prints a processed-file count rather than a rewritten-file count so that line alone does not distinguish a clean run from a repairing one; the task additionally records `git status --porcelain` output taken immediately before and immediately after the command, which is the tree observation that does distinguish them, with every rewritten path listed by name; and any rewritten path outside the `QuickFiler/` and `QuickFiler.Test/` prefixes is restored to its base-ref content with `git checkout origin/main --` followed by that path, because AC23 forbids a change outside those prefixes, and each restoration is recorded by path with the reason. The command runs unconditionally; the restoration clause governs how its result is treated, not whether it runs.

- [x] [P2-T2] Run `dotnet tool run csharpier check .` and record `docs/features/active/2026-08-28-quickfiler-carry-folder-predictor-to-item-controller-678/evidence/qa-gates/csharpier-check.md`. The command runs unconditionally. Acceptance, all three: `EXIT_CODE:` is recorded; the reported set of files needing formatting contains no path under `QuickFiler/` or `QuickFiler.Test/`; and that set is either empty, in which case the exit code must be 0, or a subset of `BASELINE_FORMAT_DRIFT` restricted to paths restored by P2-T1, in which case every member is named and the artifact additionally carries a line beginning `REMEDIATION-REQUIRED:` stating that AC19 and AC23 conflict for those paths because reaching a zero exit would require editing files outside the AC23 footprint, and that the conflict is reported rather than resolved by editing them.

- [x] [P2-T3] Run `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` and record `docs/features/active/2026-08-28-quickfiler-carry-folder-predictor-to-item-controller-678/evidence/qa-gates/analyzer-build.md`. Acceptance, both: `EXIT_CODE: 0` with a zero error count in the MSBuild summary; and the warning count is at or below the `BASELINE_ANALYZER_SUMMARY` warning count recorded in P0-T6, with any new warning named individually.

- [x] [P2-T4] Run `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true` and record `docs/features/active/2026-08-28-quickfiler-carry-folder-predictor-to-item-controller-678/evidence/qa-gates/nullable-build.md`. Acceptance, both: `EXIT_CODE: 0`; and `Output Summary:` states that no `CS86` diagnostic was introduced relative to the P0-T7 baseline enumeration.

- [x] [P2-T5] Run `pwsh -NoProfile -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot .` and record `docs/features/active/2026-08-28-quickfiler-carry-folder-predictor-to-item-controller-678/evidence/qa-gates/mstest-coverage-run.md`. Acceptance, all four: `EXIT_CODE:` recorded; `Output Summary:` states whether the run printed the literal `Done. Coverage artifact:`; total, passed, failed and skipped counts are recorded numerically; and the set of failing test names is a subset of `BASELINE_FAILURE_SET` and contains no test declared in `QuickFiler.Test/Controllers/QfcItemController.FolderHandlingTests.cs`, `QuickFiler.Test/Controllers/QfcItemController.FolderHandlingTests.Part2.cs`, `QuickFiler.Test/Controllers/QfcHomeControllerIssue218Tests.cs` or `QuickFiler.Test/Controllers/QfcHomeControllerRunAsyncHighConfidenceTests.cs`. The subset form is used deliberately: a repository-wide zero-failures assertion is not satisfiable when the baseline itself carries failures. Because "name X is absent from the failure list" is also satisfied by X never running, this task additionally asserts a discovery control: the post-change total discovered count is greater than or equal to the P0-T8 baseline total plus the number of `[TestMethod]` declarations added by P1-T3, P1-T8 and P1-T9, that added count is stated as an integer, and each of the four distinct test names `LoadFolderHandlerAsync_WhenCarriedHandlerPresent_DoesNotInvokePredictorFactory` (P1-T3, re-run green by P1-T7), `LoadFolderHandlerAsync_WhenCarriedHandlerPresentAndVarListProvided_InvokesPredictorFactory` (P1-T8), `AssignFolderComboBox_WhenArchiveRootedPredeterminedFolder_PreselectsThatFolder` (P1-T9) and `LoadFolderHandler_ProbabilityDebugLog_IncludesCallerSubjectEntryIdAndTopScore` (P1-T7) is recorded as present in the run's executed-test list by name rather than merely absent from the failure list.

- [x] [P2-T6] Prove the post-change coverage report is post-processed and record the figures in `docs/features/active/2026-08-28-quickfiler-carry-folder-predictor-to-item-controller-678/evidence/qa-gates/coverage-post-change.md`. Run Derivation D1; if P2-T5 did not print `Done. Coverage artifact:`, run Derivation D4 first and read the post-processed file, exactly as P0-T9 did. Acceptance, all four: the observed package-name list is recorded verbatim; it is a subset of the nine-name allowlist; it contains `QuickFiler` and no `log4net` entry; and Derivation D2 output is recorded as six numeric values with line-rate and branch-rate also expressed as percentages to two decimal places. The artifact states which of the two paths each side used. When the two sides used different paths, the artifact records that both paths call `ConvertTo-KoverageCoberturaXml` with the same allowlist and the same path separator and therefore produce the same denominator, that the only difference is the threshold assertion, which reads the document without altering it, and that no unfiltered report was compared against a post-processed one. Comparing an unfiltered report against a post-processed one is prohibited in either direction.

- [x] [P2-T7] Record the changed-line and new-member coverage figures required by AC20 in `docs/features/active/2026-08-28-quickfiler-carry-folder-predictor-to-item-controller-678/evidence/qa-gates/coverage-delta.md`. Join Derivation D5 to Derivation D6 after normalising path separators. Acceptance, all six: baseline and post-change repository-wide line coverage are both stated numerically and their difference is stated; the changed-line covered-over-total figure is stated numerically, or `NOT APPLICABLE` with the reason when the denominator is zero because every added line is non-executable or sits in an exempt class; the count of added lines excluded as non-executable is stated; each new or modified member in a non-exempt file is listed with its own covered-over-total figure and a pass or fail against 90 percent; each new or modified member in `FolderScoringService`, `QfcCollectionController` or `QfcDatamodel` is listed as exempt with the named test that pins it instead; and the per-file figures for the twelve production paths listed in P0-T11 are compared against `coverage-per-file-baseline.md` with no file showing a reduction that is not explained by a line deletion in that file.

- [x] [P2-T8] Assert AC20's attribute invariant and record `docs/features/active/2026-08-28-quickfiler-carry-folder-predictor-to-item-controller-678/evidence/qa-gates/exclude-attribute-invariant.md`. Run `git add -A -- QuickFiler QuickFiler.Test` and then `git diff --cached origin/main -- QuickFiler QuickFiler.Test`. Acceptance, both: the diff contains zero added lines and zero removed lines carrying the token `ExcludeFromCodeCoverage`, with both counts stated as 0; and the artifact records the diff's total added-line and removed-line counts so a zero attribute count taken over an empty diff is distinguishable from one taken over a real change.

- [x] [P2-T9] Write the compact post-change coverage summary to `docs/features/active/2026-08-28-quickfiler-carry-folder-predictor-to-item-controller-678/evidence/qa-gates/coverage-post-change.jacoco.xml`, transcribed from Derivation D3 aggregated by package exactly as P0-T10 was. Acceptance, all three: the file exists and is under 200 lines by Derivation D8; its `LINE` counter totals equal the `lines-covered` and `lines-valid` values recorded in P2-T6, where D3 is run with the node selection `//class` rather than `//class[@filename]` so it selects the same node set as `Get-CoberturaCoverageSummary` at `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1:117-128`, and any class node lacking a `filename` attribute is reported by count with its package name; and `coverage-post-change.md` carries an `EVIDENCE_SUBSTITUTION:` line recording the raw Cobertura report's measured line count and byte size and stating that the raw report is retained untracked under the git-ignored `coverage/` directory and is deliberately not committed, in the same form P0-T10 used.

- [x] [P2-T10] Audit file sizes for AC21 after formatting has settled and record `docs/features/active/2026-08-28-quickfiler-carry-folder-predictor-to-item-controller-678/evidence/qa-gates/file-size-audit.md`. This task runs after P2-T1 because CSharpier reflow changes line counts. Run `git add -A -- QuickFiler QuickFiler.Test` first so files this change created are visible to the name-listing diff, which enumerates tracked changes only. Acceptance, all three: every `.cs` file listed by `git diff --cached --name-only origin/main -- QuickFiler QuickFiler.Test` has its post-format count from Derivation D8 recorded; no listed file exceeds 500 lines, or, for a file already over 500 at baseline, its count is at or below its `BASELINE_SIZE_CENSUS` value, and a listed file over 500 with no `BASELINE_SIZE_CENSUS` entry is reported by name as a census gap rather than treated as a pass; and every new file created by this change is named together with the `<Compile Include>` entry that references it.

- [x] [P2-T11] Audit scope confinement for AC23 and record `docs/features/active/2026-08-28-quickfiler-carry-folder-predictor-to-item-controller-678/evidence/qa-gates/scope-confinement.md`. Run `git add -A -- QuickFiler QuickFiler.Test docs/features/active/2026-08-28-quickfiler-carry-folder-predictor-to-item-controller-678`, then `git diff --cached --name-only origin/main`, then `git status --porcelain` with no pathspec. Acceptance, all four: every path in the anchored name-only diff begins with `QuickFiler/`, `QuickFiler.Test/` or `docs/features/active/2026-08-28-quickfiler-carry-folder-predictor-to-item-controller-678/`; the unscoped porcelain status reports no modified or untracked path outside those three prefixes, except that paths under `.claude/agent-memory/` are enumerated separately and excluded from the AC23 judgment because that directory is tracked (609 files, not git-ignored) and is agent-session state rather than a change to the product or to policy; no path under `UtilitiesCS/`, `.claude/rules/`, `.claude/skills/` or the repository-root `CLAUDE.md` appears in either output; and the artifact records both command outputs in full. The staging step is required because a name-listing diff enumerates tracked changes only and would otherwise be blind to the files this change creates; the unscoped porcelain status is required because the staging pathspec would otherwise leave an out-of-scope path unreported.

- [x] [P2-T12] Record the clean-pass declaration at `docs/features/active/2026-08-28-quickfiler-carry-folder-predictor-to-item-controller-678/evidence/qa-gates/final-toolchain-pass.md` for AC19. Acceptance, all three: the artifact names the five commands of P2-T1 through P2-T5 in order with each one's `Timestamp:`, `Command:`, `EXIT_CODE:` and `Output Summary:`, covering the four AC19 gates of format verification, analyzer build, nullable build and the MSTest run plus the format-apply step that precedes them; it states that all five ran in the same uninterrupted pass, and that P2-T1 left no net change under `QuickFiler/` or `QuickFiler.Test/` during that pass, applying the same restoration carve-out the Phase 2 preamble defines for the restart rule: a path P2-T1 rewrote outside those two prefixes and then restored is listed by name together with its restoration and does not falsify this clause; and it states the number of loop restarts that occurred and the reason for each.

- [x] [P2-T13] Record the per-criterion verdict register at `docs/features/active/2026-08-28-quickfiler-carry-folder-predictor-to-item-controller-678/evidence/issue-updates/ac-verdicts.md`. Acceptance, all four: the artifact carries one row for each of AC1 through AC23, 23 rows and no more; each row names the evidence artifact path that supports its verdict; the artifact states explicitly that the only edit made to the `## Acceptance Criteria` section of `issue.md` is the checkbox transition `- [ ]` to `- [x]` on criteria whose supporting evidence artifact exists and verifies, performed one criterion at a time per the `acceptance-criteria-tracking` skill, and that no criterion text was reworded, added or removed, and it lists which of AC1 through AC23 were checked off and which were left unchecked with the reason; and it records `PostedAs: unknown` together with the reason, since no GitHub posting is performed by this plan.

- [x] [P2-T14] Hand off to the reduced audit and record the packet at `docs/features/active/2026-08-28-quickfiler-carry-folder-predictor-to-item-controller-678/evidence/other/reduced-audit-handoff.md`. Acceptance, all five: the packet states both check-off roles the `acceptance-criteria-tracking` skill assigns, so neither task is the sole owner: the executor checks off each criterion, one criterion per edit, as that criterion's supporting evidence artifact verifies during execution, which is the state P2-T13 records, and the reduced audit then verifies those check-offs against the evidence and checks off any remaining criterion it evaluates as PASS, leaving every criterion it evaluates as PARTIAL, FAIL or UNVERIFIED unchecked with the reason recorded; it lists every evidence artifact produced by Phase 0 and Phase 2 by path; it carries the P1-T12 out-of-scope register and its referral records; it states the minor-audit fail-closed conditions, namely that the audit fails closed if `spec.md` or `user-story.md` has appeared, if the `## Acceptance Criteria` section is missing, if any required artifact is absent, or if plan checklist state contradicts evidence on disk; and it names the two artifacts recording the AC12 normalisation decision and the AC15 accepted delta.

- [x] [P2-T15] Commit every evidence artifact produced by this plan and leave the worktree clean. This is the last task; no artifact is written after it. Acceptance, all three: the artifact is `docs/features/active/2026-08-28-quickfiler-carry-folder-predictor-to-item-controller-678/evidence/qa-gates/final-commit.md`, and `git status --porcelain` run after the commit and before this task's own check-off produces no output other than paths under `.claude/agent-memory/`, which are left uncommitted and are enumerated in that artifact with the reason, together with this task's own artifact and this plan file, both of which are committed by an amend after the check-off is written; `git diff --name-only origin/main -- docs/features/active/2026-08-28-quickfiler-carry-folder-predictor-to-item-controller-678` lists every artifact path named in Phase 0 and Phase 2; and no path under `coverage/` appears in that list.

---

## Acceptance-criterion index

| AC | Owning tasks | Primary evidence |
|---|---|---|
| AC1 | P1-T4 | evidence/other/carrier-chain.md |
| AC2 | P1-T4 | evidence/other/carrier-chain.md |
| AC3 | P0-T13, P1-T4 | evidence/baseline/carrier-construction-sites.md |
| AC4 | P1-T5, P1-T10 | evidence/other/leg-a.md |
| AC5 | P1-T5 | evidence/other/leg-a.md |
| AC6 | P1-T6 | evidence/other/leg-b.md |
| AC7 | P1-T7 | evidence/regression-testing/ac16-green.md |
| AC8 | P1-T7 | evidence/regression-testing/ac16-green.md |
| AC9 | P1-T7, P1-T8 | evidence/regression-testing/ac9-negative-guard.md |
| AC10 | P1-T7 | evidence/other/carrier-chain.md |
| AC11 | P1-T7, P1-T9 | evidence/regression-testing/ac12-path-normalisation.md |
| AC12 | P1-T9, P1-T11 | evidence/regression-testing/ac12-path-normalisation.md |
| AC13 | P1-T10 | evidence/other/test-reconciliation.md |
| AC14 | P1-T7 | evidence/other/carrier-chain.md |
| AC15 | P1-T11 | evidence/other/change-description.md |
| AC16 | P1-T3, P1-T7 | evidence/regression-testing/ac16-red.md |
| AC17 | P1-T10 | evidence/other/test-reconciliation.md |
| AC18 | P1-T8, P1-T10 | evidence/other/test-reconciliation.md |
| AC19 | P2-T1 to P2-T5, P2-T12 | evidence/qa-gates/final-toolchain-pass.md |
| AC20 | P0-T9 to P0-T11, P2-T6 to P2-T9 | evidence/qa-gates/coverage-delta.md |
| AC21 | P0-T12, P2-T10 | evidence/qa-gates/file-size-audit.md |
| AC22 | P1-T12 | evidence/other/out-of-scope-register.md |
| AC23 | P2-T11 | evidence/qa-gates/scope-confinement.md |

All evidence paths in the table are relative to
`docs/features/active/2026-08-28-quickfiler-carry-folder-predictor-to-item-controller-678/`.
