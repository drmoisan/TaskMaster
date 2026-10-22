# Partition A Non-Vacuity Control (P1-T2)

- **Issue:** #635
- **Plan task:** [P1-T2]

Timestamp: 2026-08-29T06-27

## Output Summary

The identical pathspec that returned nothing for the thirteen identifiers in [P1-T1] returns thirteen
hits across four files for the token `QfcCollectionController`, which is genuinely present in the
tracked non-`.cs` corpus. The [P1-T1] zero is therefore a measurement of absence, not an artefact of an
empty or unreachable search set.

CONTROL_HITS: 13
CONTROL_FILES: 4

## Command

Command:

```
git grep -n -I -F -e QfcCollectionController -- ":(exclude)*.cs" ":(exclude)docs/*" ":(exclude).claude/*"
```

EXIT_CODE: 0

Output, verbatim, one row per printed line:

```
QuickFiler.Test/QuickFiler.Test.csproj:130:    <Compile Include="Controllers\QfcCollectionControllerTests.cs" />
QuickFiler.Test/QuickFiler.Test.csproj:131:    <Compile Include="Controllers\QfcCollectionControllerNavigationDigitsTests.cs" />
QuickFiler.Test/QuickFiler.Test.csproj:132:    <Compile Include="Controllers\QfcCollectionControllerDarkModeTests.cs" />
QuickFiler.Test/QuickFiler.Test.csproj:133:    <Compile Include="Controllers\QfcCollectionController.TestSupport.cs" />
QuickFiler.Test/QuickFiler.Test.csproj:134:    <Compile Include="Controllers\QfcCollectionControllerDefects468Tests.cs" />
QuickFiler.Test/QuickFiler.Test.csproj:135:    <Compile Include="Controllers\QfcCollectionControllerDefects468MoveTests.cs" />
QuickFiler.Test/QuickFiler.Test.csproj:136:    <Compile Include="Controllers\QfcCollectionControllerDefects468ConversationTests.cs" />
QuickFiler.Test/QuickFiler.Test.csproj:137:    <Compile Include="Controllers\QfcCollectionControllerLayout.StaTests.cs" />
QuickFiler/Notes/notes_interface_hierarchy:9:		IQfcCollectionController ->
QuickFiler/QuickFiler.csproj:312:    <Compile Include="Controllers\QfcCollectionController.cs" />
QuickFiler/QuickFiler.csproj:362:    <Compile Include="Interfaces\IQfcCollectionController.cs" />
QuickFiler/QuickFiler.csproj.bak:240:    <Compile Include="Interfaces\IQfcCollectionController.cs" />
QuickFiler/QuickFiler.csproj.bak:261:    <Compile Include="Controllers\QfcCollectionController.cs" />
```

## Per-file hit counts

| File | Kind | Hits |
|---|---|---|
| QuickFiler.Test/QuickFiler.Test.csproj | QuickFiler test project file | 8 |
| QuickFiler/QuickFiler.csproj | QuickFiler project file | 2 |
| QuickFiler/QuickFiler.csproj.bak | tracked backup of the QuickFiler project file | 2 |
| QuickFiler/Notes/notes_interface_hierarchy | extensionless tracked notes file under the QuickFiler production tree | 1 |

The per-file counts sum as `8 + 2 + 2 + 1 = 13`, which equals the recorded `CONTROL_HITS` value, and
the four files are the four distinct paths in the printed output, which equals the recorded
`CONTROL_FILES` value.

## Why the extensionless file is the decisive element of this control

The file QuickFiler/Notes/notes_interface_hierarchy carries no extension at all, so it is a file type
that the six build-input extensions of the earlier AC-16 search — `.csproj`, `.resx`, `.config`,
`.xaml`, `.json` and `.settings` — could never reach, which proves that the widened pathspec reaches
real content the narrower scope did not.

The tracked backup file QuickFiler/QuickFiler.csproj.bak makes the same point a second time on a
different extension: `.bak` is likewise outside the six, and the [P0-T5] census records eleven tracked
`.bak` files in the Partition A scope. The research document had recorded the tracked status of the
two `.bak` project backups as unverified; this control settles it, because `git grep` searches tracked
files only and QuickFiler/QuickFiler.csproj.bak appears in its output.

## Relation to [P1-T1]

This control is the non-vacuity proof for [P1-T1]. The two runs differ in exactly one respect: the
search patterns. The pathspec, the flags `-n -I -F`, the tracked-file search set of 683 files measured
by [P0-T5], and the working tree are identical. One run returns thirteen hits and exits `0`; the other
returns nothing and exits `1`. The difference is therefore attributable to the presence or absence of
the searched tokens in the corpus and not to the reachability of the corpus.
