# R2 — Projection alignment

- Timestamp: 2026-09-02T01-24
- Issue: #678
- Task: [P1-T8]
- File: `QuickFiler/Controllers/QfcItemController.FolderHandling.cs`

## The invariant

> The carried `PredeterminedFolder` and the `FolderArray` entries must be the same projection
> of the same input, so that `_itemViewer.FolderContains` matches for every archive-rooted
> suggestion the predictor can produce.

## Edit 1 — the guard

Before:

```csharp
            if (string.IsNullOrEmpty(folderPath) || string.IsNullOrEmpty(archiveRootPath))
```

After:

```csharp
            if (string.IsNullOrEmpty(folderPath) || archiveRootPath is null)
```

`string.IsNullOrEmpty(archiveRootPath)` conflated two distinct states: "there are no globals"
and "the archive root is empty". `FolderPredictor.ProjectSuggestionPath`
(`UtilitiesCS/OutlookObjects/Folder/FolderPredictor.cs:845-858`) guards only on
`_globals is null`, then forms `archivePrefix = _globals.Ol.ArchiveRootPath + "\\"`
unconditionally, so an empty archive root gives it a prefix of one separator and it **does**
strip. After this edit a null `archiveRootPath` stands for that member's `_globals is null`
guard and nothing else.

## Edit 2 — the call site

Before:

```csharp
                    _globals?.Ol?.ArchiveRootPath
```

After:

```csharp
                    _globals is null ? null : (_globals.Ol?.ArchiveRootPath ?? string.Empty)
```

The null signal now means "no globals" and only that. Previously a non-null globals with a
null `Ol`, or with a null `ArchiveRootPath`, also produced null and was treated as the
identity.

## Edit 3 — the documentation block

The block no longer claims unconditional parity. It states that the projection mirrors
`FolderPredictor.ProjectSuggestionPath` for every non-null `folderPath` and non-null
`archiveRootPath`; that a null `archiveRootPath` stands for that member's `_globals is null`
guard and yields the identity, while an empty one does not; and it names both remaining
divergences explicitly as null-safety differences rather than projection differences.

## Acceptance clauses

| # | Clause | Result |
|---|---|---|
| 1 | `A null or empty archive root` occurs zero times in the file | PASS — **0** occurrences |
| 2 | `#678 R2` occurs exactly once in the file, on a single line | PASS — **1** occurrence, **1** matching line |
| 3 | the analyzer build exits 0 | PASS — exit 0, `CoreCompile:` 60 |
| 4 | the nullable build exits 0 | PASS — exit 0, zero `CS86`, `CoreCompile:` 66 |
| 5 | the file measures at most 500 lines by Derivation D8 | PASS — **303** (was 293) |

Clause 1 is the falsifiable form of "the old doc text is gone": the literal
`A null or empty archive root` was present on exactly one line of the pre-edit file, so the
count could and did change from 1 to 0.

## Blast radius — the six boundary assertions

Re-derived against the current tree. Exactly one of the six changed, and it is the one P1-T6
corrected under the single authorisation scope constraint 4 grants.

| `(folderPath, archiveRootPath)` | Before | After |
|---|---|---|
| `(@"\\Archive\Projects\Active", null)` | identity | identity — unchanged |
| `(@"\\Archive\Projects\Active", string.Empty)` | identity | `@"\Archive\Projects\Active"` — **corrected** |
| `(null, @"\\Archive")` | null | null — unchanged |
| `(@"\\Other\Projects", @"\\Archive")` | identity | identity — unchanged |
| `(@"\\Archive\", @"\\Archive")` | identity | identity — unchanged |
| `(@"\\ARCHIVE\Projects", @"\\archive")` | `@"Projects"` | `@"Projects"` — unchanged |

No `AssignFolderComboBox` test regresses.
`AssignFolderComboBox_WhenPredeterminedFolderPresent_PreselectsThatFolder`
(`QuickFiler.Test/Controllers/QfcItemController.FolderHandlingTests.cs:440-462`) sets no
`_globals`, so the call site still yields null and the projection is still the identity.
`AssignFolderComboBox_PredeterminedFolder_PreselectsByNameAndStillPopulates`
(`QuickFiler.Test/Controllers/QfcItemController.FolderSuggestionsTests.cs:137`) uses the
predetermined folder `"Archive\\Finance"`, which has no leading separator, so no strip occurs
under either guard.
`AssignFolderComboBox_WhenArchiveRootedPredeterminedFolder_PreselectsThatFolder` supplies
`\\Archive` as the root and is unaffected. P1-T10 verifies all three by execution.

## Output Summary

Two behavioural edits and one documentation edit in one file. The guard now distinguishes a
null archive root from an empty one, and the call site now emits null only for a null
`_globals`. Analyzer build exit 0, nullable build exit 0 with zero `CS86`. The literal
`A null or empty archive root` occurs 0 times; `#678 R2` occurs exactly once on one line. The
file measures 303 lines, 197 short of the cap. Exactly one of the six boundary assertions
changes, and it is the one P1-T6 corrected.
