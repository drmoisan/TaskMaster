# P5-T12 — AnalyzerItemRepair implemented under the preserve rule

Timestamp: 2026-09-19T09-44

Command:

```
pwsh -NoProfile -Command 'Set-Location "<execution-worktree-root>"; Import-Module "<execution-worktree-root>\scripts\dependencies\AnalyzerItemRepair.psm1" -Force -ErrorAction Stop; Get-Command -Module AnalyzerItemRepair; line count; Select-String for the computed-default-path literal, \bsed\b, external substitution executables, report and missing-segment-count identifiers, and the four ordering idioms'
```

EXIT_CODE: 0

## Output Summary

```
IMPORT=ok
LINES=399
Get-AnalyzerAssemblyPath, Get-FolderBearingElement, Get-FolderPackageIdentity,
Get-RestorePackageFolder, Invoke-AnalyzerItemRepair, Update-PackageFolderSegment
DEFAULTPATH_LITERAL=0
WORD_SED=0
EXTERNAL=0
REPORT_IDENT=4
MISSINGSEG_COUNT_IDENT=0
--- ordering companion ---
  L118: foreach ($candidate in @($KnownId | Sort-Object -Property Length -Descending)) {
```

A run of every suite under `tests/scripts/dependencies` at this point reported
`Passed=84 Failed=0 Total=84`, which includes all thirteen AnalyzerItemRepair cases and the
AC21 case that had been the single failure after P5-T8.

## Acceptance

| Clause | Required | Measured |
|---|---|---|
| Module imports without error | yes | `IMPORT=ok`, run with `-ErrorAction Stop` |
| Exports the derivation function | `Get-AnalyzerAssemblyPath` | present |
| Exports the rewrite function | `Invoke-AnalyzerItemRepair` | present |
| No computed-default-path literal | 0 | `DEFAULTPATH_LITERAL=0` |
| No folder-ordering or maximum-selection expression over Roslyn folder names | accounted | 1 occurrence, accounted below |
| No invocation of `sed` | 0 | `WORD_SED=0` |
| No other external text-substitution executable | 0 | `EXTERNAL=0` |
| Builds no report | 0 report identifiers | 4 matches, all prose; accounted below |
| Exposes no count for the missing-segment class | 0 | `MISSINGSEG_COUNT_IDENT=0` |
| At most 500 lines | <= 500 | 399 |

`DEFAULTPATH_LITERAL` searches for the three-segment literal formed from `analyzers`, the
directory separator, `dotnet`, the separator and `cs`, built at measurement time from
`[char]92` so the search pattern itself cannot be corrupted by a collapsed backslash. It
returns 0: the module never spells that path, and every item path it emits is formed from
an entry the injected listing supplied.

## Ordering companion, as the task requires

The task requires every occurrence of `Sort-Object`, `-Maximum`, `[version]` and
`Select-Object -Last` to be recorded with its line and shown not to operate on a value
derived from the injected directory listing. There is exactly one:

- **L118**, inside `Get-FolderPackageIdentity`:
  `foreach ($candidate in @($KnownId | Sort-Object -Property Length -Descending)) {`

  Its operand is `$KnownId`, the **package identifier vocabulary** the caller supplies —
  in every call site, either the single package being repaired or the identifiers the
  sibling manifest declares. It is not derived from `$DirectoryLister` and never touches a
  folder name. The ordering is by string length, and its purpose is to try the longest
  identifier first so that `Microsoft.Extensions.Configuration` cannot claim the folder
  `Microsoft.Extensions.Configuration.Binder.10.0.12`. No folder name is ordered, compared
  as a version, maximised over, or selected from anywhere in the module.

`-Maximum`, `[version]` and `Select-Object -Last` do not occur at all. A module with zero
occurrences would satisfy this trivially; this one has one and it is accounted for, which
is what stops a selection rule re-entering under a different spelling.

## The four `Report` matches are prose, and the class exposes no count

`REPORT_IDENT=4` counts case-insensitive substring matches of `report`. All four are prose,
and none is a report object, a report-building function or a count:

```
L32: module builds no report and counts nothing for that class. ConsistencyVerifier.psm1
L33: aggregates the records, counts them and emits the non-fatal reported class, so exactly
L283: version's listing does not offer is left unmodified and reported as a record. A
L327: 'cannot be derived is reported rather than guessed.')
```

L32 and L33 are the module description stating the division of labour; L283 is the
function's help; L327 is a word inside the throw message. The result object carries
`MissingSegmentRecord` — records, not a count — and `MISSINGSEG_COUNT_IDENT=0` confirms no
identifier combines the class name with a count. `ExaminedItemCount` is the count of
analyzer **items examined**, which is a different quantity and is required by the
aggregator so that a clean aggregation is distinguishable from one over nothing.

## Implementation under the preserve rule

- The rewrite moves only the `<Id>.<Version>` segment. The substitution is anchored on the
  separators either side of the segment and reuses the separator characters and the
  identifier casing the line already carries, so every following segment — including the
  Roslyn-qualified folder — is byte-identical afterwards.
- The injected listing is enumerated **only** to build the set of directory segments the
  package ships, and that set is used for one purpose: a containment test on the segment
  the existing item already names. Nothing selects from it and nothing orders it.
- An item already naming the manifest version is skipped, so the pass is a provable no-op
  on the 65 already-agreeing items in this repository.
- Every analyzer item group is visited, not only the first, because the rewrite is
  line-directed over the elements the parser returns rather than scoped to one item group.
  Sibling `<AdditionalFiles>` elements and preceding comments are untouched by
  construction.
- An absent preserved segment yields no guessed path: the item is left unmodified and one
  missing-segment record is returned naming the project, the line, the item, the missing
  segment and the segments the listing does offer.
- An empty listing means the restored directory for the manifest version does not exist,
  and the function throws.

## Duplication removed, with the reason

`Update-PackageFolderSegment` was first written in `ProjectConsistency.psm1` at P5-T6 and
is now defined once here and exported, with `ProjectConsistency.psm1` importing it. It is
the single byte-exact folder-segment substitution in the repository, and gate rule 15
turns on the correctness of exactly that substitution; two copies would be two places for
it to drift. `ProjectConsistency.psm1` fell from 365 to 322 lines as a result.

The dependency direction is `ProjectConsistency -> AnalyzerItemRepair -> PackageGraph`,
with `ConsistencyVerifier` importing all three. There is no cycle.

## Module sizes after this task

| File | Lines | Ceiling |
|---|---|---|
| `scripts/dependencies/AnalyzerItemRepair.psm1` | 399 | 500 |
| `scripts/dependencies/ProjectConsistency.psm1` | 322 | 500 |
| `scripts/dependencies/ConsistencyVerifier.psm1` | 493 | 500 |

## Amended after the P6-T2 analyzer fixes

Two of this task's recorded facts were invalidated by fixes made at P6-T2 and are restated
here rather than left stale. Nothing about the preserve rule, the derivation or the repair
behaviour changed.

- **The shared helper was renamed.** `Update-PackageFolderSegment` became
  `Get-RewrittenPackageFolderLine`. `PSUseShouldProcessForStateChangingFunctions` fires on
  the `Update-` verb, and the function is a pure string transform with no state to change,
  so the verb was wrong rather than the rule. Its counterpart in `ProjectConsistency.psm1`
  was renamed on the same ground, from `Update-ReferenceAssemblyVersion` to
  `Get-RewrittenReferenceVersionLine`. `ProjectConsistency.psm1` still imports the single
  shared implementation; there is still exactly one folder-segment substitution in the
  repository.
- **The line counts moved.** `AnalyzerItemRepair.psm1` is 402 and
  `ProjectConsistency.psm1` is 331; `ConsistencyVerifier.psm1` is unchanged at 493. The
  additions are the statement-level local bindings, and their explanatory comments, that
  the `PSReviewUnusedParameter` fixes required.

The acceptance searches were re-run against the amended module and every result is
unchanged: `DEFAULTPATH_LITERAL=0`, `WORD_SED=0`, `EXTERNAL=0`,
`MISSINGSEG_COUNT_IDENT=0`, and the ordering companion still reports exactly one
occurrence, `Sort-Object -Property Length -Descending` over `$KnownId` at L118, whose
operand is still the identifier vocabulary and not the injected listing. The exported set
is now `Get-AnalyzerAssemblyPath, Get-FolderBearingElement, Get-FolderPackageIdentity,
Get-RestorePackageFolder, Get-RewrittenPackageFolderLine, Invoke-AnalyzerItemRepair`.

## Correction made to the suite during this task

Four `AC12-` derivation cases indexed the returned path set as `$derived[0]`. PowerShell
unrolls a single-element array return to a scalar, so on the three single-result fixtures
that expression indexed into the **string** and yielded its first character. The four
assertions were changed to `@($derived)[0]`, which is the indexing the language requires;
the asserted values, the `Count` assertions beside them and the production behaviour are
unchanged. The multi-assembly case never exhibited it, because a four-element result is
still an array.
