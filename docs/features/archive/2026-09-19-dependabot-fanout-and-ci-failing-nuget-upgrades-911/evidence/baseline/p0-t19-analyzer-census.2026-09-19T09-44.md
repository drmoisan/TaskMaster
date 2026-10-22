# P0-T19 — Analyzer-Item Census and #898 Branch-State Declaration

Timestamp: 2026-09-19T23-10

EXIT_CODE: 0

`<MERGE_BASE>` throughout is `734112ed25bba293cb074e71fee2286bc3b72fae`, as pinned by P0-T3.

## MEZIANTOU-898-STATE: unfixed

That declaration is the output P0-T11 and P1-T9 both read. Its basis is the four measurements
below.

## 1. Total `<Analyzer Include>` items across `*.csproj`

Command:

```
git grep -h "Analyzer Include=" -- "*.csproj" | wc -l
git grep -c "Analyzer Include=" -- "*.csproj"
```

**Total: 162, across exactly 17 files.**

| Project file | Items |
|---|---|
| `QuickFiler/QuickFiler.csproj` | 9 |
| `QuickFiler.Test/QuickFiler.Test.csproj` | 11 |
| `SVGControl.Test/SVGControl.Test.csproj` | 2 |
| `Tags/Tags.csproj` | 9 |
| `Tags.Test/Tags.Test.csproj` | 11 |
| `TaskMaster/TaskMaster.csproj` | 9 |
| `TaskMaster.Test/TaskMaster.Test.csproj` | 11 |
| `TaskTree/TaskTree.csproj` | 9 |
| `TaskTree.Test/TaskTree.Test.csproj` | 11 |
| `TaskVisualization/TaskVisualization.csproj` | 9 |
| `TaskVisualization.Test/TaskVisualization.Test.csproj` | 11 |
| `ToDoModel/ToDoModel.csproj` | 9 |
| `ToDoModel.Test/ToDoModel.Test.csproj` | 11 |
| `UtilitiesCS/UtilitiesCS.csproj` | 9 |
| `UtilitiesCS.Test/UtilitiesCS.Test.csproj` | 11 |
| `VBFunctions/VBFunctions.csproj` | 9 |
| `VBFunctions.Test/VBFunctions.Test.csproj` | 11 |

`SVGControl/SVGControl.csproj` carries none and is the 18th project file. The per-file numbers sum
to 162.

## 2. Files matching `Meziantou.Analyzer.3.0.203`, with per-file match counts

Command:

```
git grep -c "Meziantou.Analyzer.3.0.203" -- "*.csproj"
git grep -l "Meziantou.Analyzer.3.0.203" -- "*.csproj" | wc -l
```

**15 files, exactly 1 match each.**

| Project file | Matches |
|---|---|
| `QuickFiler/QuickFiler.csproj` | 1 |
| `QuickFiler.Test/QuickFiler.Test.csproj` | 1 |
| `Tags/Tags.csproj` | 1 |
| `Tags.Test/Tags.Test.csproj` | 1 |
| `TaskMaster.Test/TaskMaster.Test.csproj` | 1 |
| `TaskTree/TaskTree.csproj` | 1 |
| `TaskTree.Test/TaskTree.Test.csproj` | 1 |
| `TaskVisualization/TaskVisualization.csproj` | 1 |
| `TaskVisualization.Test/TaskVisualization.Test.csproj` | 1 |
| `ToDoModel/ToDoModel.csproj` | 1 |
| `ToDoModel.Test/ToDoModel.Test.csproj` | 1 |
| `UtilitiesCS/UtilitiesCS.csproj` | 1 |
| `UtilitiesCS.Test/UtilitiesCS.Test.csproj` | 1 |
| `VBFunctions/VBFunctions.csproj` | 1 |
| `VBFunctions.Test/VBFunctions.Test.csproj` | 1 |

These 15 paths are exactly the spec `## Write Set` subsection "Project files carrying a stranded
analyzer item (#898)", member for member. This is the edit set P1-T9 rewrites.

## 3. Files whose `<Analyzer Include>` names `Meziantou.Analyzer.3.0.235`, working tree

Command:

```
git grep -c "Analyzer Include=.*Meziantou\.Analyzer\.3\.0\.235" -- "*.csproj"
```

**1 file**, being `TaskMaster/TaskMaster.csproj` alone with 1 match.

`TaskMaster/TaskMaster.csproj:575` already names `3.0.235`:

```
    <Analyzer Include="..\packages\Meziantou.Analyzer.3.0.235\analyzers\dotnet\roslyn5.0\cs\Meziantou.Analyzer.dll" />
```

**It is therefore not one of the 15.** 15 stale plus this 1 correct gives the 16 analyzer-bearing
projects; `SVGControl.Test/SVGControl.Test.csproj` carries 2 analyzer items but no Meziantou one,
and `SVGControl/SVGControl.csproj` carries none.

## 4. The anchored base-commit measurement

Command:

```
git grep -c "Analyzer Include=.*Meziantou\.Analyzer\.3\.0\.235" 734112ed25bba293cb074e71fee2286bc3b72fae -- "*.csproj"
```

Output, verbatim:

```
734112ed25bba293cb074e71fee2286bc3b72fae:TaskMaster/TaskMaster.csproj:1
```

**Number of output lines: 1. That is the figure — a file count of 1.**

### Two counter-intuitive properties of that command, recorded so they are not simplified away

**It is read as a file count and never as a sum.** `git grep -c` prints one `<rev>:<file>:<count>`
line per matching file. Adding the trailing numbers gives 33 at an unfixed base and 48 at a fixed
one, and neither is a meaningful figure here. The measurement is the *number of lines printed*, and
at this base it is 1.

**The anchor to `Analyzer Include=` is what makes it discriminate.** Measured directly, both ways,
at the same commit:

| Pattern | Files at `<MERGE_BASE>` | Files in the working tree |
|---|---|---|
| bare `Meziantou.Analyzer.3.0.235` | **16** | **16** |
| anchored `Analyzer Include=.*Meziantou\.Analyzer\.3\.0\.235` | **1** | **1** |

The bare literal also matches the `<Import>` and `EnsureNuGetPackageBuildImports` `<Error>` guards,
which already name `3.0.235` in all 16 analyzer-bearing projects at the merge-base. That asymmetry
between the guards and the analyzer item is defect #898 itself. The unanchored form therefore
returns 16 files whether or not the fix has landed and distinguishes nothing; the anchored form
returns 1 at an unfixed base and 16 at a fixed one.

The pattern carries no doubled backslash, so it is safe through the Bash tool under gate rule 14.

## Declaration

```
MEZIANTOU-898-STATE: unfixed
```

The sibling branch `bug/meziantou-analyzer-hintpath-skew-898` has not merged to `main` and this
branch has not taken such a merge. P0-T11, P1-T9 and P9-T12 take their first branch.

## Acceptance evaluation — `unfixed` branch

| Clause | Required | Measured | Verdict |
|---|---|---|---|
| Total `<Analyzer Include>` items | exactly 162 | 162 | PASS |
| Across how many files | exactly 17 | 17 | PASS |
| Stale `3.0.203` file count | exactly 15 | 15 | PASS |
| Matches per stale file | exactly 1 each | 1 each, all 15 | PASS |
| Anchored base-commit file count | exactly **1** | 1 | PASS |
| Artifact records `TaskMaster/TaskMaster.csproj:575` already names `3.0.235` and is not one of the 15 | required | recorded in section 3 | PASS |

No observation fell outside either branch, so the stop-and-re-derive path is not taken.

Output Summary: `MEZIANTOU-898-STATE: unfixed`. The tree carries 162 `<Analyzer Include>` items
across exactly 17 `*.csproj` files. Exactly 15 files match `Meziantou.Analyzer.3.0.203`, one match
each, and those 15 are exactly the spec `## Write Set` stranded-analyzer subsection. Exactly 1 file
names `3.0.235` in an `<Analyzer Include>` — `TaskMaster/TaskMaster.csproj:575`, which is therefore
not one of the 15 — and the anchored base-commit measurement at `734112ed2` returns 1 output line,
the value that identifies an unfixed base. The unanchored literal returns 16 files at that same
commit and at the working tree, which is why it is not used. All six acceptance clauses of the
`unfixed` branch hold.
