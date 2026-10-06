# P5-T9 AC9 check-off

Timestamp: 2026-10-06T18-36
Command: Grep tool over spec.md `^- \[x\] AC9 ` and `^- \[[ x]\] AC[0-9]+ ` (count); Read of the named artifact; live multiline Grep for the corrected pair and live CMD-NAME-COUNT over glob */app.config
EXIT_CODE: 0
Output Summary: AC9 met and checked off. All 15 carrier configs redirect System.Linq.AsyncEnumerable `0.0.0.0-10.0.0.12` to `10.0.0.12`, and no 10.0.0.7 redirect remains. The identity occurs in exactly the same 15 files, with no block added and none in SVGControl or SVGControl.Test.

Artifact read:
- evidence/qa-gates/p3-t13-slae-redirects.2026-10-03T11-32.md (EXIT_CODE 0): 15 files at VERSION, stale none, CMD-NAME-COUNT 15 files, and the SVGControl configs are absent from the numstat list.

Live observations (2026-10-06): the corrected-pair multiline Grep finds 15 files with 1 each; Grep `assemblyIdentity name="System\.Linq\.AsyncEnumerable"` finds the same 15 files with 1 each (Tags, TaskTree, TaskVisualization, QuickFiler, TaskMaster, ToDoModel, UtilitiesCS, VBFunctions.Test, Tags.Test, TaskTree.Test, QuickFiler.Test, TaskMaster.Test, TaskVisualization.Test, ToDoModel.Test, UtilitiesCS.Test); neither SVGControl config appears.

SPEC-LINE: `- [x] AC9 (the 15 System.Linq.AsyncEnumerable redirects corrected).` (criterion text unchanged)
