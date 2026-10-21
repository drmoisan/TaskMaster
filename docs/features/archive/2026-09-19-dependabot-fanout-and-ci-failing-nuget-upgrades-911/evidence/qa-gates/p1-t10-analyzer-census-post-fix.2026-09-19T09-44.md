# P1-T10 — Post-#898 analyzer census

Timestamp: 2026-09-19T13-14

Command: `git grep -c -F "Analyzer Include=" -- "*.csproj"`;
`git grep -l -F "Meziantou.Analyzer.3.0.203" -- "*.csproj"`;
`git grep -c "Analyzer Include=.*Meziantou\.Analyzer\.3\.0\.235" -- "*.csproj"`

EXIT_CODE: 0

## Totals

| Measure | Value |
|---|---|
| `Analyzer Include=` lines across `*.csproj` | **162** |
| Files carrying at least one such line | **17** |
| Files matching `Meziantou.Analyzer.3.0.203` | **0** |
| Files whose `<Analyzer Include>` names `Meziantou.Analyzer.3.0.235` | **16** |

The two positive counts guard the zero. A census that enumerated nothing would also report zero
stale files; a total of 162 across 17 files and an anchored count of 16 establish that the search
resolved the tree.

## Per-file breakdown

```
QuickFiler.Test/QuickFiler.Test.csproj:11
QuickFiler/QuickFiler.csproj:9
SVGControl.Test/SVGControl.Test.csproj:2
Tags.Test/Tags.Test.csproj:11
Tags/Tags.csproj:9
TaskMaster.Test/TaskMaster.Test.csproj:11
TaskMaster/TaskMaster.csproj:9
TaskTree.Test/TaskTree.Test.csproj:11
TaskTree/TaskTree.csproj:9
TaskVisualization.Test/TaskVisualization.Test.csproj:11
TaskVisualization/TaskVisualization.csproj:9
ToDoModel.Test/ToDoModel.Test.csproj:11
ToDoModel/ToDoModel.csproj:9
UtilitiesCS.Test/UtilitiesCS.Test.csproj:11
UtilitiesCS/UtilitiesCS.csproj:9
VBFunctions.Test/VBFunctions.Test.csproj:11
VBFunctions/VBFunctions.csproj:9
```

17 files, summing to 162. `SVGControl/SVGControl.csproj` carries none, as P0-T19 recorded; the 17th
file here is `SVGControl.Test/SVGControl.Test.csproj` with 2 items, and the 16 analyzer-bearing
Meziantou projects are the remainder.

## Acceptance evaluation

| Clause | Measured | Verdict |
|---|---|---|
| The total remains exactly 162 across exactly 17 files | 162 across 17 | PASS |
| The `3.0.203` count is exactly 0 | 0 | PASS |
| The `3.0.235` analyzer-item count is exactly 16 files, being the 15 corrected plus `TaskMaster/TaskMaster.csproj` | 16 | PASS |

The total is unchanged from the 162 P0-T19 recorded, which confirms the P1-T9 edit replaced items
rather than adding or removing any.

Output Summary: after the #898 correction the analyzer-item total is unchanged at 162 across 17
project files; no file matches the stale `Meziantou.Analyzer.3.0.203` literal; and 16 files carry
an `<Analyzer Include>` naming `Meziantou.Analyzer.3.0.235`, being the 15 corrected by P1-T9 plus
`TaskMaster/TaskMaster.csproj`, which already agreed with its manifest.
