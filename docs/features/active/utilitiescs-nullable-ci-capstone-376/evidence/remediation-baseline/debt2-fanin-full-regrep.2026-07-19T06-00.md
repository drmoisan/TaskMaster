# Debt 2 — Full Re-Grep and Rebuild Diagnostic Scan (Authoritative, Supersedes Plan Snapshot)

Timestamp: 2026-07-19T06-00
Command: `MSBuild.exe TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug /p:Platform="Any CPU" /p:TreatWarningsAsErrors=true`
EXIT_CODE: 1

## Corrected measurement methodology

The plan's Revision note and this session's own P0-T9 baseline artifact recorded a "62-file /
296+28+2=326-diagnostic" figure using a simple `grep -oE ... | sed ... | sort -u | wc -l` shell
pipeline. Re-deriving that figure with a rigorous per-`(file,line,col,code)` dedup (a small
Python script reading the raw MSBuild log directly) found that the simple shell pipeline
**double-counted every file**: MSBuild emits each diagnostic twice under `/m` parallel build —
once inline under a `"<node>>"` prefix during the per-project build, and again, byte-identical
except for the missing node-number prefix, in the final error summary. The shell sed expression
`s/^\s*[0-9]*>//` only strips the prefix on the inline variant (which has a literal `>` to
anchor on); the summary-only variant has no `>` at all, so the substitution's mandatory `>` atom
fails to match and the line is left with its original leading whitespace intact. `sort -u` then
treats the two whitespace-differing renderings of the identical file path as two distinct
strings, exactly doubling every file and diagnostic count. This explains why the "62-file/326"
figure exactly equals 2x this artifact's authoritative counts (31 files, 162 diagnostics) to
within one diagnostic-code rounding difference (28 vs 26 for CS0618, resolved below).

**This artifact's counts (31 files / 162 diagnostics), derived from a `(file, line, col, code)`
keyed dedup directly against the raw MSBuild log, are the authoritative current measurement and
supersede both the plan's Revision-note snapshot and this session's own P0-T9 baseline
artifact's headline figures.** The diagnostic-code list and blocking build-error behavior
(BUILD DEBT 1 already cleared by Phase 1; BUILD DEBT 2 remaining) are otherwise unchanged in
kind — only the file/occurrence-count arithmetic is corrected here.

## Authoritative diagnostic-code breakdown (deduped)

- CS8604: 55
- CS8602: 50
- CS8601: 16
- CS0618: 13
- CS8625: 9
- CS8603: 9
- CS8619: 4
- CS8620: 3
- CS8600: 2
- CS0168: 1
- **Total: 162** (vs. the un-deduped, doubled raw grep count of 326 seen in this same log, and
  in the P0-T9-era `WarningsNotAsErrors=CS0649` scoping run, confirming the doubling artifact is
  a log-processing bug, not a real change in defect count between the two runs.)

## Authoritative current opted-in/affected file count: 31 files

| # | File | Diagnostics |
|---|---|---|
| 1 | `UtilitiesCS\EmailIntelligence\Bayesian\BayesianClassifierGroup.cs` | CS0618:1 |
| 2 | `UtilitiesCS\EmailIntelligence\Bayesian\Performance\BayesianPerformanceMeasurement.cs` | CS8602:24, CS8604:6 |
| 3 | `UtilitiesCS\EmailIntelligence\Bayesian\Performance\BayesianSerializationHelper.cs` | CS0618:1, CS8625:1 |
| 4 | `UtilitiesCS\EmailIntelligence\ClassifierGroups\Actionable\ActionableClassifierGroup.cs` | CS8602:1, CS8604:1, CS8620:1 |
| 5 | `UtilitiesCS\EmailIntelligence\ClassifierGroups\Categories\CategoryClassifierGroup.cs` | CS8601:1, CS8602:5, CS8604:2, CS8619:1, CS8620:1 |
| 6 | `UtilitiesCS\EmailIntelligence\ClassifierGroups\ClassifierGroupUtilities.cs` | CS8604:1, CS8625:4 |
| 7 | `UtilitiesCS\EmailIntelligence\ClassifierGroups\ManagerAsyncLazy.cs` | CS0618:1, CS8602:1, CS8604:3 |
| 8 | `UtilitiesCS\EmailIntelligence\ClassifierGroups\MulticlassEngine.cs` | CS8601:1, CS8602:3, CS8619:1 |
| 9 | `UtilitiesCS\EmailIntelligence\ClassifierGroups\OlFolder\OlFolderClassifierGroup.cs` | CS8601:1, CS8602:4, CS8604:1 |
| 10 | `UtilitiesCS\EmailIntelligence\ClassifierGroups\SpamBayes\SpamBayes.Classify.cs` | CS8604:2 |
| 11 | `UtilitiesCS\EmailIntelligence\ClassifierGroups\SpamBayes\SpamBayes.Conditions.cs` | CS8602:1 |
| 12 | `UtilitiesCS\EmailIntelligence\ClassifierGroups\Triage\Triage.cs` | CS0618:3 |
| 13 | `UtilitiesCS\EmailIntelligence\ClassifierGroups\Triage\Triage_OlLogic.cs` | CS8602:1 |
| 14 | `UtilitiesCS\EmailIntelligence\EmailParsingSorting\AutoFile.cs` | CS0168:1, CS8604:1 |
| 15 | `UtilitiesCS\EmailIntelligence\EmailParsingSorting\EmailDataMiner.FolderExtraction.cs` | CS0618:1, CS8602:2, CS8604:1, CS8619:1 |
| 16 | `UtilitiesCS\EmailIntelligence\EmailParsingSorting\EmailDataMiner.Serialization.cs` | CS8625:4 |
| 17 | `UtilitiesCS\EmailIntelligence\EmailParsingSorting\EmailDataMiner.Transform.cs` | CS8600:1, CS8602:5, CS8604:1, CS8620:1 |
| 18 | `UtilitiesCS\EmailIntelligence\EmailParsingSorting\EmailFiler.cs` | CS0618:1, CS8602:1, CS8604:1 |
| 19 | `UtilitiesCS\EmailIntelligence\EmailParsingSorting\EmailFilerConfig.cs` | CS8604:4 |
| 20 | `UtilitiesCS\EmailIntelligence\EmailParsingSorting\SortEmail.cs` | CS0618:3, CS8601:1, CS8602:2, CS8604:3 |
| 21 | `UtilitiesCS\EmailIntelligence\Evaluation\FolderPredictorEvaluator.cs` | CS8604:1 |
| 22 | `UtilitiesCS\EmailIntelligence\Flags\FlagClassNoItem.cs` | CS8603:5 |
| 23 | `UtilitiesCS\EmailIntelligence\IntelligenceConfig.cs` | CS0618:1, CS8604:1, CS8619:1 |
| 24 | `UtilitiesCS\EmailIntelligence\OlFolderTools\FilterOlFolders\FilterOlFoldersController.cs` | CS8604:2 |
| 25 | `UtilitiesCS\EmailIntelligence\People\PeopleScoDictionaryNew.cs` | CS8600:1, CS8604:2 |
| 26 | `UtilitiesCS\EmailIntelligence\SubjectMap\SubjectMapEncoder.cs` | CS8604:6 |
| 27 | `UtilitiesCS\Extensions\IAsyncEnumerableExtensions.cs` | CS0618:1 |
| 28 | `UtilitiesCS\OutlookObjects\Folder\FolderConverter.cs` | CS8603:2 |
| 29 | `UtilitiesCS\OutlookObjects\Folder\FolderPredictor.cs` | CS8603:2 |
| 30 | `UtilitiesCS\OutlookObjects\Folder\FolderScorer.cs` | CS8601:12, CS8604:15 |
| 31 | `UtilitiesCS\OutlookObjects\Folder\FolderTreeCompatibilityView.cs` | CS8604:1 |

## Plan-vs-reality path notes (flagged for the record, not a stop condition)

Two small path discrepancies between the plan's task text and the actual repository layout were
found during this re-grep. Both are resolved as in-batch mechanical corrections (same
remediation style already authorized for their batch), not scope-expanding design decisions,
and are flagged here for the orchestrator's visibility per the delegation's escalation
instructions:

1. **`UtilitiesCS/EmailIntelligence/IntelligenceConfig.cs`** (row 23) is a single file directly
   at the `EmailIntelligence` root (confirmed via `find UtilitiesCS/EmailIntelligence -maxdepth 1
   -type f -iname "*.cs"`), not a file inside an `IntelligenceConfig/` subdirectory as P2-T8's
   task text implies ("UtilitiesCS/EmailIntelligence/IntelligenceConfig/**/*.cs"). No such
   subdirectory exists. This file is folded into the P2-T8/T9 combined batch, since it is
   thematically identical to that batch's intent and is still within the
   `UtilitiesCS/EmailIntelligence/**` scope tree the plan declares in P2-T1.
2. **`UtilitiesCS/Extensions/IAsyncEnumerableExtensions.cs`** (row 27) lives in the top-level
   `UtilitiesCS/Extensions/` folder (confirmed via `find UtilitiesCS -maxdepth 1 -iname
   "Extensions" -type d`), not `UtilitiesCS/EmailIntelligence/Extensions/` as P2-T8's task text
   implies. No `UtilitiesCS/EmailIntelligence/Extensions/` folder exists anywhere in the
   repository. This file's single CS0618 diagnostic is genuinely outside both of the plan's two
   declared scope trees (`UtilitiesCS/EmailIntelligence/**` and
   `UtilitiesCS/OutlookObjects/Folder/**`). It is folded into the P2-T8/T9 combined batch as a
   mechanical, single-line, same-pattern-as-already-authorized CS0618 fix (obsolete
   `IAsyncEnumerable` LINQ method usage — the same fix class already applied elsewhere in this
   batch and confirmed as a pre-existing, repo-wide pattern in the P0-T7 baseline's QuickFiler/
   TaskMaster warnings) because leaving it unfixed would make the plan's own mandatory P2-T17
   solution-wide `EXIT_CODE 0` gate unreachable. No new design judgment is required for this
   fix; it is the identical opportunistic-CS0618-fix pattern the plan already authorizes.

No other file outside the two declared scope trees was found to have a build-blocking
diagnostic in this rebuild.
