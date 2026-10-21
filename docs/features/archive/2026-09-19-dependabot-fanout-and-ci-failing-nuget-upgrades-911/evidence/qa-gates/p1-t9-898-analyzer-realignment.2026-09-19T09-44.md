# P1-T9 — Issue #898 analyzer-item realignment

Timestamp: 2026-09-19T13-10

Command: a `pwsh -NoProfile -Command` byte-level rewrite using
`[System.IO.File]::ReadAllBytes` / `WriteAllBytes` with
`[System.Text.Encoding]::UTF8.GetString` and `GetBytes`; followed by
`git grep -l -F "Meziantou.Analyzer.3.0.203" -- "*.csproj"`,
`git diff --numstat 734112ed25bba293cb074e71fee2286bc3b72fae -- "*.csproj"` and
`git status --porcelain --untracked-files=all -- "*.csproj"`

EXIT_CODE: 0

## Branch taken

**Branch A — `MEZIANTOU-898-STATE: unfixed`**, the state P0-T19 declared, re-confirmed against the
working tree immediately before the edit:

| Pre-edit measurement | Value |
|---|---|
| Files matching `Meziantou.Analyzer.3.0.203` across `*.csproj` | **15**, one match each |
| Anchored `Analyzer Include=.*Meziantou\.Analyzer\.3\.0\.235` file count in the working tree | **1** (`TaskMaster/TaskMaster.csproj`) |
| Anchored file count at `<MERGE_BASE>` `734112ed25bba293cb074e71fee2286bc3b72fae` | **1** |

The sibling branch `bug/meziantou-analyzer-hintpath-skew-898` had not merged, so the tree was in
exactly the state P0-T19 recorded and Branch A applies. Had the anchored working-tree count already
read 16 before the edit, the run would have stopped and reported instead.

## The edit, performed byte-exactly per gate rule 14

`sed` through the Bash tool is prohibited for this rewrite: the tool collapses the doubled
backslashes the pattern needs, so the substitution matches nothing while `sed -i` still rewrites
all 15 files' line endings, producing a 15-file porcelain over an empty content diff. The rewrite
here reads each file as bytes, decodes with UTF-8, replaces the literal
`Meziantou.Analyzer.3.0.203` with `Meziantou.Analyzer.3.0.235`, and writes the re-encoded bytes
back. Two guards ran per file and both held on all 15: the occurrence count before replacement was
exactly **1**, and the output byte length equalled the input byte length, the two version literals
being the same length.

Representative hunk, `UtilitiesCS/UtilitiesCS.csproj`:

```
@@ -1306,7 +1306,7 @@
   <ItemGroup>
     <!-- Issue #181: analyzer-only references (first-party scope). Severities are set to suggestion in .editorconfig so none break the nullable TreatWarningsAsErrors build. -->
-    <Analyzer Include="..\packages\Meziantou.Analyzer.3.0.203\analyzers\dotnet\roslyn5.0\cs\Meziantou.Analyzer.dll" />
+    <Analyzer Include="..\packages\Meziantou.Analyzer.3.0.235\analyzers\dotnet\roslyn5.0\cs\Meziantou.Analyzer.dll" />
     <Analyzer Include="..\packages\Roslynator.Analyzers.5.0.0\analyzers\dotnet\roslyn4.7\cs\Roslynator.CSharp.Analyzers.dll" />
```

The version segment alone moved. The `analyzers\dotnet\roslyn5.0\cs` segment is byte-identical, the
sibling `Roslynator` items at `roslyn4.7` are untouched, and the `<!-- Issue #181 ... -->` comment
above the item group survives.

## Acceptance evaluation — Branch A, asserted in the order the plan fixes

| Clause | Measured | Verdict |
|---|---|---|
| Residual search for `Meziantou.Analyzer.3.0.203` across `*.csproj` returns exactly 0 files, against the 15 P0-T19 recorded | **0**, against 15 | PASS |
| `git diff --numstat <MERGE_BASE> -- "*.csproj"` totals exactly 15 added and exactly 15 deleted across exactly 15 files, one added and one deleted per file | 15 files, 15 added, 15 deleted; every per-file row reads `1  1` | PASS |
| `git status --porcelain --untracked-files=all -- "*.csproj"` lists those same 15 paths | 15 entries, all ` M`, identical to the numstat path set | PASS |

The residual clause is asserted first because it is the one the collapsed-backslash failure
defeats: a no-op substitution leaves all 15 files still matching the old literal while still
producing a 15-file porcelain and, after a line-ending rewrite, a 15-file diff. The per-file
`1  1` numstat rows are what separate a real one-line substitution from that failure.

## Preserved-folder observation

`Meziantou.Analyzer.3.0.235\analyzers\dotnet\roslyn5.0\cs` now matches in **16** files — the 15
corrected plus `TaskMaster/TaskMaster.csproj`, which already carried it — each with exactly one
match. The anchored `Analyzer Include=.*Meziantou\.Analyzer\.3\.0\.235` working-tree file count is
likewise **16**, against 1 before the edit. The preserve rule held: no item's Roslyn-qualified
folder segment changed, and the 80 items a highest-folder selection rule would have rewritten were
left alone.

## Per-file numstat

```
1	1	QuickFiler.Test/QuickFiler.Test.csproj
1	1	QuickFiler/QuickFiler.csproj
1	1	Tags.Test/Tags.Test.csproj
1	1	Tags/Tags.csproj
1	1	TaskMaster.Test/TaskMaster.Test.csproj
1	1	TaskTree.Test/TaskTree.Test.csproj
1	1	TaskTree/TaskTree.csproj
1	1	TaskVisualization.Test/TaskVisualization.Test.csproj
1	1	TaskVisualization/TaskVisualization.csproj
1	1	ToDoModel.Test/ToDoModel.Test.csproj
1	1	ToDoModel/ToDoModel.csproj
1	1	UtilitiesCS.Test/UtilitiesCS.Test.csproj
1	1	UtilitiesCS/UtilitiesCS.csproj
1	1	VBFunctions.Test/VBFunctions.Test.csproj
1	1	VBFunctions/VBFunctions.csproj
```

Output Summary: Branch A was taken, re-confirmed against the tree. All 15 stale
`<Analyzer Include>` items were rewritten byte-exactly from `Meziantou.Analyzer.3.0.203` to
`Meziantou.Analyzer.3.0.235`, version segment only. The residual count for the old literal fell
from 15 files to **0**; the merge-base diff is exactly 15 added and 15 deleted lines across exactly
15 files with one of each per file; porcelain lists the same 15 paths; and the anchored
`3.0.235` analyzer-item file count rose from 1 to 16 with `roslyn5.0` preserved everywhere.
`EXIT_CODE: SKIPPED` was not used.
